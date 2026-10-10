"""I-62 / FX-02 — medición única de claude-cli: fuente RÁPIDA del árbol de procesos (operaciones 7 y 8).
Win32 por ctypes, sin privilegios de administrador y sin herramientas externas:
  * instantánea Toolhelp32 (PID, PPID, nombre) cada FastPeriodMs (100 ms);
  * por cada miembro nuevo del árbol (PPID = un miembro vivo o conocido, y CreationDate del hijo >= la del padre: guarda contra la reutilización
    del PID) se abre un handle SYNCHRONIZE | PROCESS_QUERY_LIMITED_INFORMATION (y PROCESS_TERMINATE si se concede) y se conserva hasta el final:
    mientras el handle está abierto el PID no puede reutilizarse, y el handle da la hora de salida y el código de salida exactos;
  * la terminación de cada identidad se confirma con WaitForSingleObject(handle, 0) (WAIT_OBJECT_0 = terminado).
Límite declarado: un proceso que nace y muere entre dos muestras (< 100 ms) y no deja descendientes vivos no se observa; un nieto de ese proceso
quedaría con un PPID desconocido: lo cubre la regla de huérfanos (find_orphans). Nunca ejecuta claude.
"""
import ctypes
import ctypes.wintypes as wt
import datetime
import os
import subprocess
import threading
import time

TH32CS_SNAPPROCESS = 0x00000002
PROCESS_TERMINATE = 0x0001
PROCESS_QUERY_LIMITED_INFORMATION = 0x1000
SYNCHRONIZE = 0x00100000
WAIT_OBJECT_0, WAIT_TIMEOUT = 0x0, 0x102
STILL_ACTIVE = 259
INVALID_HANDLE_VALUE = ctypes.c_void_p(-1).value

k32 = ctypes.WinDLL("kernel32", use_last_error=True)


class PROCESSENTRY32W(ctypes.Structure):
    _fields_ = [("dwSize", wt.DWORD), ("cntUsage", wt.DWORD), ("th32ProcessID", wt.DWORD), ("th32DefaultHeapID", ctypes.c_size_t),
                ("th32ModuleID", wt.DWORD), ("cntThreads", wt.DWORD), ("th32ParentProcessID", wt.DWORD), ("pcPriClassBase", ctypes.c_long),
                ("dwFlags", wt.DWORD), ("szExeFile", ctypes.c_wchar * 260)]


k32.CreateToolhelp32Snapshot.restype = wt.HANDLE
k32.CreateToolhelp32Snapshot.argtypes = [wt.DWORD, wt.DWORD]
k32.Process32FirstW.argtypes = [wt.HANDLE, ctypes.POINTER(PROCESSENTRY32W)]
k32.Process32NextW.argtypes = [wt.HANDLE, ctypes.POINTER(PROCESSENTRY32W)]
k32.OpenProcess.restype = wt.HANDLE
k32.OpenProcess.argtypes = [wt.DWORD, wt.BOOL, wt.DWORD]
k32.CloseHandle.argtypes = [wt.HANDLE]
k32.GetProcessTimes.argtypes = [wt.HANDLE] + [ctypes.POINTER(wt.FILETIME)] * 4
k32.QueryFullProcessImageNameW.argtypes = [wt.HANDLE, wt.DWORD, wt.LPWSTR, ctypes.POINTER(wt.DWORD)]
k32.WaitForSingleObject.argtypes = [wt.HANDLE, wt.DWORD]
k32.WaitForSingleObject.restype = wt.DWORD
k32.GetExitCodeProcess.argtypes = [wt.HANDLE, ctypes.POINTER(wt.DWORD)]
k32.TerminateProcess.argtypes = [wt.HANDLE, wt.UINT]


def snapshot():
    """→ lista de (pid, ppid, nombre) de todos los procesos (Toolhelp32)."""
    h = k32.CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0)
    if not h or h == INVALID_HANDLE_VALUE:
        return []
    out = []
    try:
        e = PROCESSENTRY32W()
        e.dwSize = ctypes.sizeof(PROCESSENTRY32W)
        ok = k32.Process32FirstW(h, ctypes.byref(e))
        while ok:
            out.append((int(e.th32ProcessID), int(e.th32ParentProcessID), e.szExeFile))
            ok = k32.Process32NextW(h, ctypes.byref(e))
    finally:
        k32.CloseHandle(h)
    return out


def _ft(ft):
    return (ft.dwHighDateTime << 32) | ft.dwLowDateTime


def ft_to_iso(v):
    if not v:
        return None
    return (datetime.datetime(1601, 1, 1, tzinfo=datetime.timezone.utc) + datetime.timedelta(microseconds=v // 10)).strftime("%Y-%m-%dT%H:%M:%S.%fZ")


def open_proc(pid, terminate=True):
    for acc in ((SYNCHRONIZE | PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_TERMINATE) if terminate else None,
                SYNCHRONIZE | PROCESS_QUERY_LIMITED_INFORMATION):
        if acc is None:
            continue
        h = k32.OpenProcess(acc, False, pid)
        if h:
            return h
    return None


def times(h):
    c, e, k, u = wt.FILETIME(), wt.FILETIME(), wt.FILETIME(), wt.FILETIME()
    if not k32.GetProcessTimes(h, ctypes.byref(c), ctypes.byref(e), ctypes.byref(k), ctypes.byref(u)):
        return None, None
    return _ft(c), _ft(e)


def image(h):
    buf = ctypes.create_unicode_buffer(1024)
    n = wt.DWORD(1024)
    return buf.value if k32.QueryFullProcessImageNameW(h, 0, buf, ctypes.byref(n)) else None


def exited(h):
    return k32.WaitForSingleObject(h, 0) == WAIT_OBJECT_0


def exit_code(h):
    c = wt.DWORD()
    return int(c.value) if k32.GetExitCodeProcess(h, ctypes.byref(c)) else None


def creation_of(pid):
    h = k32.OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, False, pid)
    if not h:
        return None
    try:
        return times(h)[0]
    finally:
        k32.CloseHandle(h)


class FastTracker:
    """Árbol del proceso lanzado por identidad (PID + CreationDate), muestreado cada period_s."""

    def __init__(self, period_s=0.1):
        self.period_s = period_s
        self.members = {}          # key "pid@creation100ns" → registro
        self.handles = {}          # key → handle (abierto hasta close())
        self.root_key = None
        self.samples = 0
        self.sample_times = []     # instantes monotónicos de cada muestra (para los huecos)
        self.errors = []
        self._stop = threading.Event()
        self._lock = threading.Lock()
        self._thread = None
        self.t0_mono = time.monotonic()

    def set_root(self, pid, launch_handle=None):
        h = open_proc(pid)
        if not h:
            self.errors.append("no se pudo abrir el handle de la raíz %d" % pid)
            return None
        cr, _ = times(h)
        if launch_handle is not None:
            cr2, _ = times(wt.HANDLE(int(launch_handle)))
            if cr2 is not None and cr2 != cr:
                self.errors.append("CreationDate de la raíz distinta entre el handle de Popen y el propio")
        key = "%d@%d" % (pid, cr)
        with self._lock:
            self.root_key = key
            self.handles[key] = h
            self.members[key] = {"Pid": pid, "Ppid": None, "Name": None, "Path": image(h), "Creation100ns": cr, "CreationUtc": ft_to_iso(cr),
                                 "IsRoot": True, "FirstSeenS": round(time.monotonic() - self.t0_mono, 3), "LastSeenAliveS": None}
        return key

    def _tick(self):
        snap = snapshot()
        t = time.monotonic()
        with self._lock:
            self.samples += 1
            self.sample_times.append(t)
            if self.root_key is None:
                return
            by_pid = {}
            for k, m in self.members.items():
                by_pid.setdefault(m["Pid"], []).append((k, m))
            alive_pids = {p for p, _, _ in snap}
            for pid, ppid, name in snap:
                for k, m in by_pid.get(pid, []):
                    if m["Name"] is None:
                        m["Name"] = name
                    if m["Ppid"] is None:
                        m["Ppid"] = ppid
                    m["LastSeenAliveS"] = round(t - self.t0_mono, 3)
            changed = True
            while changed:
                changed = False
                for pid, ppid, name in snap:
                    if pid in by_pid and any(m["Pid"] == pid and not exited(self.handles[k]) for k, m in by_pid[pid]):
                        continue
                    parents = by_pid.get(ppid, [])
                    if not parents:
                        continue
                    h = open_proc(pid)
                    if not h:
                        self.errors.append("handle denegado para el PID %d (%s), hijo de %d" % (pid, name, ppid))
                        continue
                    cr, _ = times(h)
                    if cr is None or not any(pm["Creation100ns"] <= cr for _, pm in parents):
                        k32.CloseHandle(h)
                        continue
                    key = "%d@%d" % (pid, cr)
                    if key in self.members:
                        k32.CloseHandle(h)
                        continue
                    pk = max((pk for pk, pm in parents if pm["Creation100ns"] <= cr), key=lambda x: self.members[x]["Creation100ns"])
                    self.handles[key] = h
                    self.members[key] = {"Pid": pid, "Ppid": ppid, "ParentKey": pk, "Name": name, "Path": image(h), "Creation100ns": cr,
                                         "CreationUtc": ft_to_iso(cr), "IsRoot": False, "FirstSeenS": round(t - self.t0_mono, 3),
                                         "LastSeenAliveS": round(t - self.t0_mono, 3)}
                    by_pid.setdefault(pid, []).append((key, self.members[key]))
                    changed = True
            del alive_pids

    def _run(self):
        while not self._stop.is_set():
            start = time.monotonic()
            try:
                self._tick()
            except Exception as e:  # nunca interrumpe el lanzamiento; queda registrado
                self.errors.append("tick: %r" % (e,))
            self._stop.wait(max(0.0, self.period_s - (time.monotonic() - start)))

    def start(self):
        self._thread = threading.Thread(target=self._run, daemon=True)
        self._thread.start()

    def stop(self):
        self._stop.set()
        if self._thread:
            self._thread.join(timeout=5)

    def check(self, label):
        """Estado de cada identidad observada: terminada (con hora y código de salida) o viva."""
        with self._lock:
            rows, alive = [], []
            for k, m in self.members.items():
                h = self.handles.get(k)
                done = exited(h) if h else None
                _, ex = times(h) if h else (None, None)
                rows.append({"Key": k, "Exited": done, "ExitUtc": ft_to_iso(ex) if done else None, "ExitCode": exit_code(h) if (h and done) else None})
                if done is False:
                    alive.append(k)
            return {"Label": label, "At": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%fZ"),
                    "Members": rows, "Alive": alive}

    def kill_alive(self):
        killed = []
        with self._lock:
            for k, h in self.handles.items():
                if h and not exited(h):
                    ok = bool(k32.TerminateProcess(h, 1))
                    killed.append({"Key": k, "TerminateProcess": ok})
        return killed

    def gaps(self, t_from=None, t_to=None):
        ts = [t for t in self.sample_times if (t_from is None or t >= t_from) and (t_to is None or t <= t_to)]
        if len(ts) < 2:
            return {"Samples": len(ts), "MaxGapS": None}
        return {"Samples": len(ts), "MaxGapS": round(max(b - a for a, b in zip(ts, ts[1:])), 3)}

    def export(self):
        with self._lock:
            return {"PeriodS": self.period_s, "Samples": self.samples, "RootKey": self.root_key,
                    "Members": [dict({k: v for k, v in m.items()}, Key=key) for key, m in self.members.items()], "Errors": list(self.errors)}

    def close(self):
        with self._lock:
            for h in self.handles.values():
                if h:
                    k32.CloseHandle(h)
            self.handles = {}


def find_orphans(snap_with_creation, tree_pids_creation, window_start100, window_end100, closed_names):
    """Regla de huérfanos (AUTOMATION_PLAN 16.4, adaptada): proceso de la lista cerrada, fuera del árbol, creado en la ventana, cuyo padre no
    existe o existe con una CreationDate posterior (PID reutilizado). Entrada: [(pid, ppid, name, creation100ns|None)]."""
    by_pid = {p: (pp, n, c) for p, pp, n, c in snap_with_creation}
    tree = set(tree_pids_creation)
    out = []
    for pid, ppid, name, cr in snap_with_creation:
        if (name or "").lower() not in closed_names:
            continue
        if (pid, cr) in tree:
            continue
        if cr is None:
            parent = by_pid.get(ppid)
            if parent is None:
                out.append({"Pid": pid, "Name": name, "CreationUtc": None, "Reason": "UNATTRIBUTABLE: CreationDate ilegible y padre inexistente"})
            continue
        if not (window_start100 <= cr <= window_end100):
            continue
        parent = by_pid.get(ppid)
        if parent is None:
            out.append({"Pid": pid, "Name": name, "CreationUtc": ft_to_iso(cr), "Reason": "padre inexistente"})
        elif parent[2] is not None and parent[2] > cr:
            out.append({"Pid": pid, "Name": name, "CreationUtc": ft_to_iso(cr), "Reason": "padre con CreationDate posterior (PID reutilizado)"})
    return out


def snapshot_with_creation(closed_names):
    """Instantánea completa; la CreationDate solo se lee para los procesos de la lista cerrada y sus padres (mínimo necesario)."""
    snap = snapshot()
    need = {p for p, pp, n in snap if n.lower() in closed_names}
    need |= {pp for p, pp, n in snap if n.lower() in closed_names}
    cr = {p: creation_of(p) for p in need}
    return [(p, pp, n, cr.get(p)) for p, pp, n in snap]


def taskkill_tree(pid):
    r = subprocess.run(["taskkill", "/PID", str(pid), "/T", "/F"], capture_output=True, text=True)
    return {"Pid": pid, "ExitCode": r.returncode}
