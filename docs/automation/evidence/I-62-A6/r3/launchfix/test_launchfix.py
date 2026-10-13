"""I-62, corrección del lanzamiento del siguiente kit: pruebas de launch.r3-template.py (guarda G, registro R, sonda P, dry-run D) con las
formas EXACTAS de la orden, sobre copias que imitan la disposición del kit en D:\\r62-a6r3\\launchfix (desechables). No ejecuta claude.exe ni
ningún CLI de modelos, no toca el kit congelado de la ronda 2 (solo lee y copia launch.py, transport_gate.py, owner_gate.py y kit-params.json,
y comprueba sus SHA-256 antes y después), no escribe en ningún worktree de git y no usa la red.

Casos:
  i    python -B kit/launch.py --record-probe (y las grafías kit\\launch.py, ruta absoluta y `python -B launch.r3-template.py --record-probe`
       desde kit/) → código 0;
  ii   python -X utf8 -B kit/launch.py --record-probe → la guarda lo rechaza (código 2); en una copia de prueba con la guarda desactivada, el
       registro robusto escribe el registro (bytes: sin error) y, con el fallo de la ronda 2 inyectado (las líneas 267-268 originales), captura
       la clase del error en vez de abortar;
  iii  el fragmento ORIGINAL de la ronda 2 (líneas 267-268) con un PID inexistente: con -X utf8, UnicodeDecodeError → q.stdout None → TypeError;
       con -B solo, funciona;
  iv   python -B -X utf8 …, python -I -B …, python kit/launch.py … (sin -B), PYTHONUTF8=1, modos no admitidos → rechazados por la guarda sin
       escribir nada; y la guarda corre antes de cargar transport_gate.py (marcador);
  v    --dry-run (preflight sustituida en la copia de prueba) informa de la guarda y de la sonda;
  vi   lanzamiento real (preflight sustituida, binario inexistente): si la sonda falla se niega antes de la preflight; si pasa, sigue a la
       preflight;
  vii  reproducibilidad del adaptador, kit congelado intacto, README coherente con FROZEN_COMMAND, sin __pycache__ ni temporales de la sonda,
       saneamiento de OUT.
Escribe test-results.json (saneado) junto a este script. Uso: python -B test_launchfix.py
"""
import ctypes
import glob
import hashlib
import json
import os
import platform
import re
import shutil
import subprocess
import sys
import tempfile
import time

OUT = os.path.dirname(os.path.abspath(__file__))
WORK = r"D:\r62-a6r3\launchfix"
FROZEN_KIT = os.path.expandvars(r"%TEMP%\claude\D--Documentos-Codex-Calculadora-de-racks\e5124bf0-ed19-48a1-97ba-574baf9d33a2\scratchpad"
                                r"\i62\day3\kit-a6r2-review\kit")
FROZEN_README = os.path.join(os.path.dirname(FROZEN_KIT), "README.md")
FROZEN_FILES = {   # kit-manifest.json de la ronda 2 (Files)
    "launch.py": "f2f58cec38b9f34e04d04e94f866d7d75a6bbd4864ccfbac734f8a52965f78ac",
    "transport_gate.py": "b05fedd17b9b6856d618d2ffcde88efc1ceafaa3e6c2d5373667919a718129c6",
    "owner_gate.py": "969844a928571fe2d72be806fab21b5c7cb332e6ffd5e0c2d6142b027608ff1c",
    "kit-params.json": "6f6a76ed43807daa4eb9b1c758ee10f4842aa5a04866f31c0f4430fc56e1d4a7",
}
ROUND2_RUN_LAUNCH = r"D:\r62-arch-a6r2-run\launch"   # solo se lista (antes y después), nunca se abre para escribir
TEMPLATE = os.path.join(OUT, "launch.r3-template.py")
NONEXISTENT_PID = 99999999   # = 3 (mod 4): los PID de Windows son múltiplos de 4, así que no puede existir
ROUND2_L267 = '    q = subprocess.run(["tasklist", "/FI", "PID eq %d" % p.pid, "/NH"], capture_output=True, text=True)'
ROUND2_L268 = '    rec["ProcessGoneAfter"] = str(p.pid) not in q.stdout'
EMAIL_RX = re.compile(r"[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)+")


# ------------------------------------------------------------------------------------------------------------ saneamiento
def _short(p):
    buf = ctypes.create_unicode_buffer(1024)
    n = ctypes.windll.kernel32.GetShortPathNameW(p, buf, 1024)
    return buf.value if n else p


_UP = os.environ.get("USERPROFILE", "")
PREFIXES = sorted({x for x in (_UP, os.path.realpath(_UP) if _UP else "", _short(_UP) if _UP else "") if x}, key=len, reverse=True)
USERS = sorted({os.path.basename(x) for x in PREFIXES} | ({os.environ["USERNAME"]} if os.environ.get("USERNAME") else set()), key=len,
               reverse=True)


def _variants(pre):
    return {pre, pre.replace("\\", "\\\\"), pre.replace("\\", "/"), "/" + pre[0].lower() + pre[2:].replace("\\", "/")}


def sanitize(s):
    for pre in PREFIXES:
        for v in sorted(_variants(pre), key=len, reverse=True):
            s = re.sub(re.escape(v), lambda m: "%USERPROFILE%", s, flags=re.I)
    for u in USERS:
        s = re.sub(re.escape(u), lambda m: "<usuario>", s, flags=re.I)
    return EMAIL_RX.sub(lambda m: "<email>", s)


def leaks(s):
    found = [u for u in USERS if u.lower() in s.lower()]
    found += [v for pre in PREFIXES for v in _variants(pre) if v.lower() in s.lower()]
    found += ["email" for _ in EMAIL_RX.finditer(s)]
    return sorted(set(found))


# ------------------------------------------------------------------------------------------------------------ utilidades
def sha_file(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest()


def decode(b):
    try:
        return b.decode("utf-8")
    except UnicodeDecodeError:
        return b.decode("cp1252", errors="replace")


def snapshot(root):
    out = []
    for d, dirs, fs in os.walk(root):
        for n in sorted(dirs + fs):
            p = os.path.join(d, n)
            st = os.stat(p)
            out.append((os.path.relpath(p, root), st.st_size if os.path.isfile(p) else -1, st.st_mtime_ns))
    return sorted(out)


def probe_temps():
    return set(glob.glob(os.path.join(tempfile.gettempdir(), "r62-record-probe-*")))


def run(argv, cwd, env_extra=None, timeout=180):
    env = dict(os.environ)
    env.update(env_extra or {})
    t0 = time.monotonic()
    r = subprocess.run(argv, cwd=cwd, env=env, capture_output=True, timeout=timeout)
    so, se = decode(r.stdout), decode(r.stderr)
    try:
        js = json.loads(so)
    except ValueError:
        js = None
    return {"rc": r.returncode, "stdout": so, "stderr": se, "json": js, "seconds": round(time.monotonic() - t0, 2)}


def exact_once(text, old, new, label):
    n = text.count(old)
    assert n == 1, "sustitución de prueba %s: %d coincidencias (se exige 1)" % (label, n)
    return text.replace(old, new, 1)


def make_copy(name, subs=(), params=None, gate_marker=False):
    """Copia que imita el kit: <WORK>\\<name>\\kit\\launch.py (= plantilla con las sustituciones de prueba, cada una exacta y única) +
    launch.r3-template.py (sin tocar) + transport_gate.py, owner_gate.py y kit-params.json del kit congelado (solo leídos)."""
    root = os.path.join(WORK, name)
    kit = os.path.join(root, "kit")
    os.makedirs(kit)
    text = open(TEMPLATE, encoding="utf-8").read()
    for label, old, new in subs:
        text = exact_once(text, old, new, label)
    with open(os.path.join(kit, "launch.py"), "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    if not subs:
        shutil.copyfile(TEMPLATE, os.path.join(kit, "launch.r3-template.py"))
    for n in ("transport_gate.py", "owner_gate.py"):
        shutil.copyfile(os.path.join(FROZEN_KIT, n), os.path.join(kit, n))
    if gate_marker:   # la guarda debe correr ANTES de cargar transport_gate.py: si se carga, deja un marcador
        src = open(os.path.join(kit, "transport_gate.py"), encoding="utf-8").read()
        src = exact_once(src, "KIT = os.path.dirname(os.path.abspath(__file__))\n",
                         "KIT = os.path.dirname(os.path.abspath(__file__))\nopen(os.path.join(KIT, 'IMPORTED.marker'), 'w').close()   # prueba\n",
                         "marcador")
        with open(os.path.join(kit, "transport_gate.py"), "w", encoding="utf-8", newline="\n") as f:
            f.write(src)
    if params:
        p = json.load(open(os.path.join(FROZEN_KIT, "kit-params.json"), encoding="utf-8"))
        p.update(params)
        with open(os.path.join(kit, "kit-params.json"), "w", encoding="utf-8", newline="\n") as f:
            f.write(json.dumps(p, ensure_ascii=False, indent=1) + "\n")
    else:
        shutil.copyfile(os.path.join(FROZEN_KIT, "kit-params.json"), os.path.join(kit, "kit-params.json"))
    return root, kit


# sustituciones de prueba (solo en copias desechables; nunca en la plantilla entregada)
SUB_GUARD_OFF = ("guarda-desactivada", "FROZEN_GUARD_ENFORCED = True", "FROZEN_GUARD_ENFORCED = False")
SUB_FAULT = ("fallo-ronda-2-inyectado", r'''        q = subprocess.run(["tasklist", "/FI", "PID eq %d" % p.pid, "/NH"], capture_output=True)   # bytes: sin decodificar
        rec["TasklistQuery"] = {"ReturnCode": q.returncode, "StdoutBytes": len(q.stdout), "NonAsciiBytes": any(b > 0x7F for b in q.stdout)}
        rec["ProcessGoneAfter"] = re.search(rb"(?<![0-9])%d(?![0-9])" % p.pid, q.stdout) is None
''', "    " + ROUND2_L267 + "   # INYECCIÓN (copia de prueba): launch.py L267 de la ronda 2\n" + "    " + ROUND2_L268 + "   # L268\n" +
    '        rec["TasklistQuery"] = {"ReturnCode": q.returncode, "StdoutBytes": len(q.stdout), "NonAsciiBytes": any(ord(c) > 0x7F for c in q.stdout)}\n')
SUB_BIN_NONE = ("binario-inexistente", 'BIN = os.path.join(os.environ.get("APPDATA", ""), "Claude", "claude-code", "2.1.293", "83cb0bd7fed4", "claude.exe")',
                r'BIN = r"D:\r62-a6r3\launchfix\no-such-claude.exe"')
SUB_PREFLIGHT_STUB = ("preflight-sustituida", "def preflight(dry_run=False):\n    c = {}\n",
                      'def preflight(dry_run=False):\n    return ({"11-OwnerAndAuthorizationGate": {"Ok": None, "AuthorizationSections": [], '
                      '"Stub": "preflight sustituida en la copia de prueba"}}, False, [], (None, None))\n    c = {}\n')


def sub_run_clone(root):
    clone, runp = os.path.join(root, "clone-standin"), os.path.join(root, "run-standin")
    return (("run-y-clon-desechables", 'CLONE, RUN = r"D:\\r62-arch-a6r2", r"D:\\r62-arch-a6r2-run"',
             'CLONE, RUN = r"%s", r"%s"' % (clone, runp)), {"Clone": clone, "Run": runp}, runp)


# ------------------------------------------------------------------------------------------------------------ hechos del host
def host_facts():
    k = ctypes.windll.kernel32
    cp = k.GetConsoleOutputCP() or k.GetOEMCP()   # cmd escribe en la página de la consola
    f = {"Platform": platform.platform(), "Win32Ver": list(platform.win32_ver()),
         "Ver": subprocess.run(["cmd", "/c", "ver"], capture_output=True).stdout.decode("cp%d" % cp, errors="replace").strip()}
    chcp = subprocess.run(["cmd", "/c", "chcp"], capture_output=True).stdout
    m = re.search(rb"([0-9]+)\s*$", chcp)   # `chcp` sin argumentos solo informa; el texto del mensaje no siempre va en la página activa
    f["Chcp"] = {"Raw": chcp.hex(), "ActiveCodePage": int(m.group(1)) if m else None}
    ps = subprocess.run(["pwsh", "-NoProfile", "-NonInteractive", "-Command",
                         "[Console]::OutputEncoding.CodePage; [Console]::OutputEncoding.WebName; [Console]::InputEncoding.CodePage"],
                        capture_output=True)
    f["PwshConsoleEncoding"] = decode(ps.stdout).split()
    f["Kernel32"] = {"ConsoleOutputCP": k.GetConsoleOutputCP(), "ConsoleCP": k.GetConsoleCP(), "OEMCP": k.GetOEMCP(), "ACP": k.GetACP(),
                     "HasConsoleWindow": bool(k.GetConsoleWindow())}
    f["Python"] = {"Version": sys.version, "Executable": sys.executable, "WhichPython": shutil.which("python"), "BasePrefix": sys.base_prefix,
                   "Flags": {n: getattr(sys.flags, n) for n in type(sys.flags).__match_args__}, "OrigArgv": sys.orig_argv}
    for label, p in (("ExecutableSha256", sys.executable), ("BaseInterpreterSha256", os.path.join(sys.base_prefix, "python.exe"))):
        try:
            f["Python"][label] = sha_file(p)
        except OSError as e:
            f["Python"][label] = "no legible: %s" % e
    f["PythonEnvNames"] = sorted(n for n in os.environ if n.upper().startswith("PYTHON"))
    f["PYTHONUTF8"] = os.environ.get("PYTHONUTF8")
    q = subprocess.run(["tasklist", "/FI", "PID eq %d" % NONEXISTENT_PID, "/NH"], capture_output=True)
    try:
        q.stdout.decode("utf-8")
        utf8_ok = True
    except UnicodeDecodeError:
        utf8_ok = False
    f["TasklistNonexistentPid"] = {"ReturnCode": q.returncode, "FirstBytesHex": q.stdout[:16].hex(), "ValidUtf8": utf8_ok,
                                   "AsCp850": q.stdout.decode("cp850", errors="replace").strip()}
    return f


# ------------------------------------------------------------------------------------------------------------ casos
RESULTS = []


def record(cid, case, command, expected, observed, holds, details=None):
    RESULTS.append({"Id": cid, "Case": case, "Command": command, "Expected": expected, "Observed": observed, "Holds": bool(holds),
                    "Details": details or {}})
    print("%-5s %-5s %s" % (cid, "OK" if holds else "FALLA", case))


def probe_summary(js):
    rp = (js or {}).get("RecordProbe") or {}
    fg = (js or {}).get("FrozenCommandGuard") or {}
    return {"RecordProbeOk": rp.get("Ok"), "Problems": rp.get("Problems"), "RecordWritten": rp.get("RecordWritten"), "LaunchError": rp.get("LaunchError"),
            "ExitCode": rp.get("ExitCode"), "RecordKeys": rp.get("RecordKeys"),
            "MissingFields": rp.get("MissingFields"), "PostRunErrors": rp.get("PostRunErrors"), "TasklistQuery": rp.get("TasklistQuery"),
            "ProcessGoneAfter": rp.get("ProcessGoneAfter"), "TempDirRemoved": rp.get("TempDirRemoved"),
            "RunLaunchExistedBefore": rp.get("RunLaunchExistedBefore"), "RunLaunchExistsAfter": rp.get("RunLaunchExistsAfter"),
            "KitListingUnchanged": rp.get("KitListingUnchanged"), "GuardOk": fg.get("Ok"), "GuardEnforced": fg.get("Enforced"),
            "GuardProblems": fg.get("Problems"), "GuardFlags": {k: (fg.get("Flags") or {}).get(k) for k in ("dont_write_bytecode", "utf8_mode", "isolated")},
            "GuardXOptions": fg.get("XOptions"), "GuardOrigArgvTail": fg.get("OrigArgvTail"), "Encodings": fg.get("Encodings")}


def case_accept(cid, root, cwd, argv, label):
    snap, temps = snapshot(root), probe_temps()
    r = run(argv, cwd)
    s = probe_summary(r["json"])
    after = snapshot(root)
    holds = r["rc"] == 0 and s["RecordProbeOk"] is True and s["GuardOk"] is True and s["PostRunErrors"] == [] and \
        isinstance(s["TasklistQuery"], dict) and s["TempDirRemoved"] is True and after == snap and probe_temps() == temps
    record(cid, "sonda de registro con la orden congelada (%s)" % label, " ".join(argv) + "   (cwd: %s)" % sanitize(cwd),
           "código 0; RecordProbe.Ok y FrozenCommandGuard.Ok = true; PostRunErrors = []; tasklist tratado; temporal borrado; nada escrito en la copia",
           "código %d; RecordProbe.Ok = %s; guarda Ok = %s; PostRunErrors = %s; TasklistQuery = %s; temporal borrado = %s; copia sin cambios = %s"
           % (r["rc"], s["RecordProbeOk"], s["GuardOk"], s["PostRunErrors"], s["TasklistQuery"], s["TempDirRemoved"], after == snap),
           holds, dict(s, Seconds=r["seconds"], StderrTail=r["stderr"][-600:]))


def case_refuse(cid, root, cwd, argv, label, env_extra=None, expect_rule=None):
    snap, temps = snapshot(root), probe_temps()
    r = run(argv, cwd, env_extra)
    js = r["json"] or {}
    fg = js.get("FrozenCommandGuard") or {}
    probs = fg.get("Problems") or []
    after = snapshot(root)
    pyc = glob.glob(os.path.join(root, "**", "__pycache__"), recursive=True)
    rule_ok = expect_rule is None or any(expect_rule in p for p in probs)
    holds = r["rc"] == 2 and "Refused" in js and "RecordProbe" not in js and after == snap and probe_temps() == temps and not pyc and rule_ok
    record(cid, "guarda: %s" % label, ("%s " % " ".join("%s=%s" % kv for kv in (env_extra or {}).items()) if env_extra else "") + " ".join(argv),
           "código 2 (Refused), sin sonda ni preflight; nada escrito (copia sin cambios, sin __pycache__, sin temporal)" +
           ("; regla: «%s»" % expect_rule if expect_rule else ""),
           "código %d; Refused = %s; problemas = %s; copia sin cambios = %s; __pycache__ = %d; temporales nuevos = %d"
           % (r["rc"], "Refused" in js, probs, after == snap, len(pyc), len(probe_temps() - temps)),
           holds, {"Flags": {k: (fg.get("Flags") or {}).get(k) for k in ("dont_write_bytecode", "utf8_mode", "isolated", "ignore_environment")},
                   "XOptions": fg.get("XOptions"), "OrigArgvTail": fg.get("OrigArgvTail"), "Problems": probs, "Seconds": r["seconds"]})


def snippet_case(cid, root, argv, utf8_env_is_round2):
    r = run(argv, root)
    js = r["json"] or {}
    te = js.get("ThreadErrors") or []
    exc = js.get("Exception") or {}
    if "-X" in argv:
        chain = any(t.get("Class") == "UnicodeDecodeError" for t in te) and js.get("QStdoutIsNone") is True and exc.get("Class") == "TypeError"
        expected = ("hilo lector: UnicodeDecodeError (byte 0xe0, posición 9); q.stdout = None; L268: TypeError «argument of type 'NoneType' is "
                    "not iterable»") if utf8_env_is_round2 else "sin error (tasklist emite UTF-8 válido en este entorno)"
        holds = chain if utf8_env_is_round2 else (not te and not exc)
    else:
        expected = "sin error: q.stdout es str y ProcessGoneAfter = true"
        holds = not te and not exc and js.get("QStdoutIsNone") is False and js.get("ProcessGoneAfter") is True
    record(cid, "fragmento original de la ronda 2 (L267-268), PID %d inexistente" % NONEXISTENT_PID, " ".join(argv), expected,
           "hilos: %s; q.stdout None = %s; excepción = %s; ProcessGoneAfter = %s" % (
               [(t.get("Class"), t.get("Message")) for t in te], js.get("QStdoutIsNone"), exc or None, js.get("ProcessGoneAfter")),
           holds, {"Result": js, "StderrTail": r["stderr"][-900:], "rc": r["rc"]})


def main():
    t_start = time.time()
    frozen_before = {n: sha_file(os.path.join(FROZEN_KIT, n)) for n in FROZEN_FILES}
    r2launch_before = snapshot(ROUND2_RUN_LAUNCH) if os.path.isdir(ROUND2_RUN_LAUNCH) else None
    temps_before = probe_temps()
    os.makedirs(WORK, exist_ok=True)
    for d in glob.glob(os.path.join(WORK, "case-*")) + glob.glob(os.path.join(WORK, "adapt-rerun")):
        shutil.rmtree(d)
    facts = host_facts()
    print("hechos del host reunidos")

    # ---------------- i: orden congelada, sonda → 0
    root, kit = make_copy("case-i")
    case_accept("i-a", root, root, ["python", "-B", "kit/launch.py", "--record-probe"], "grafía documentada kit/launch.py")
    case_accept("i-b", root, kit, ["python", "-B", "launch.r3-template.py", "--record-probe"], "la plantilla con su nombre, desde kit/")
    case_accept("i-c", root, root, ["python", "-B", "kit\\launch.py", "--record-probe"], "grafía kit\\launch.py")
    case_accept("i-d", root, root, ["python", "-B", os.path.join(kit, "launch.py"), "--record-probe"], "ruta absoluta")

    # ---------------- iv (y la primera mitad de ii): rechazos de la guarda
    case_refuse("ii-a", root, root, ["python", "-X", "utf8", "-B", "kit/launch.py", "--record-probe"], "-X utf8 -B (la orden de la ronda 2)",
                expect_rule="utf8_mode")
    case_refuse("iv-a", root, root, ["python", "-B", "-X", "utf8", "kit/launch.py", "--record-probe"], "-B -X utf8", expect_rule="utf8_mode")
    case_refuse("iv-b", root, root, ["python", "-I", "-B", "kit/launch.py", "--record-probe"], "-I -B", expect_rule="isolated")
    case_refuse("iv-c", root, root, ["python", "kit/launch.py", "--record-probe"], "sin -B (PYTHONDONTWRITEBYTECODE=1 en el entorno)",
                expect_rule="opciones del intérprete")
    case_refuse("iv-d", root, root, ["python", "-B", "kit/launch.py", "--record-probe"], "orden exacta con PYTHONUTF8=1 en el entorno",
                env_extra={"PYTHONUTF8": "1"}, expect_rule="utf8_mode")
    case_refuse("iv-e", root, root, ["python", "-B", "kit/launch.py", "--record-probe", "--dry-run"], "dos modos", expect_rule="no admitidos")
    case_refuse("iv-f", root, root, ["python", "-B", "kit/launch.py", "--bogus"], "argumento desconocido", expect_rule="no admitidos")
    root_m, kit_m = make_copy("case-iv-order", gate_marker=True)
    case_refuse("iv-g", root_m, root_m, ["python", "-X", "utf8", "-B", "kit/launch.py", "--record-probe"],
                "corre antes de cargar transport_gate.py (sin marcador)", expect_rule="utf8_mode")
    marker = os.path.join(kit_m, "IMPORTED.marker")
    refused_no_marker = not os.path.exists(marker)
    r = run(["python", "-B", "kit/launch.py", "--record-probe"], root_m)
    record("iv-h", "control del marcador: con la orden congelada transport_gate.py sí se carga (después de la guarda)",
           "python -B kit/launch.py --record-probe", "sin marcador tras iv-g; marcador presente y código 0 tras la orden congelada",
           "sin marcador tras iv-g = %s; código %d; marcador = %s" % (refused_no_marker, r["rc"], os.path.exists(marker)),
           refused_no_marker and r["rc"] == 0 and os.path.exists(marker))

    # ---------------- ii (segunda mitad): guarda desactivada en copias de prueba
    root2, _ = make_copy("case-ii-guard-off", subs=[SUB_GUARD_OFF])
    r = run(["python", "-X", "utf8", "-B", "kit/launch.py", "--record-probe"], root2)
    s = probe_summary(r["json"])
    record("ii-b", "guarda desactivada (copia de prueba), -X utf8: el registro robusto (tasklist en bytes) escribe el registro sin error",
           "python -X utf8 -B kit/launch.py --record-probe",
           "código 0; guarda informada (Ok = false, Enforced = false); RecordProbe.Ok = true; PostRunErrors = []; tasklist con bytes no ASCII tratado",
           "código %d; guarda Ok = %s, Enforced = %s; RecordProbe.Ok = %s; PostRunErrors = %s; TasklistQuery = %s"
           % (r["rc"], s["GuardOk"], s["GuardEnforced"], s["RecordProbeOk"], s["PostRunErrors"], s["TasklistQuery"]),
           r["rc"] == 0 and s["GuardOk"] is False and s["GuardEnforced"] is False and s["RecordProbeOk"] is True and s["PostRunErrors"] == [],
           dict(s, StderrTail=r["stderr"][-600:]))
    root3, _ = make_copy("case-ii-fault", subs=[SUB_GUARD_OFF, SUB_FAULT])
    r = run(["python", "-X", "utf8", "-B", "kit/launch.py", "--record-probe"], root3)
    s = probe_summary(r["json"])
    classes = sorted({e.get("Class") for e in s["PostRunErrors"] or []})
    steps = sorted({e.get("Step") for e in s["PostRunErrors"] or []})
    record("ii-c", "guarda desactivada + fallo de la ronda 2 inyectado (L267-268 originales), -X utf8: se escribe el registro y se captura la clase",
           "python -X utf8 -B kit/launch.py --record-probe",
           "sin traza fatal: código 1 (la sonda informa del fallo); registro escrito; PostRunErrors con TypeError (paso ProcessGoneAfter) y "
           "UnicodeDecodeError (hilo lector); solo faltan TasklistQuery y ProcessGoneAfter; temporal borrado",
           "código %d; registro escrito = %s; clases = %s; pasos = %s; faltan = %s; temporal borrado = %s"
           % (r["rc"], s["RecordWritten"], classes, steps, s["MissingFields"], s["TempDirRemoved"]),
           r["rc"] == 1 and r["json"] is not None and s["RecordWritten"] is True and classes == ["TypeError", "UnicodeDecodeError"] and
           sorted(s["MissingFields"] or []) == ["ProcessGoneAfter", "TasklistQuery"] and s["TempDirRemoved"] is True,
           dict(s, StderrTail=r["stderr"][-900:]))
    r = run(["python", "-B", "kit/launch.py", "--record-probe"], root3)
    s = probe_summary(r["json"])
    record("ii-d", "la misma copia con el fallo inyectado, con la orden congelada (-B solo): el texto decodificado en cp1252 no falla",
           "python -B kit/launch.py --record-probe", "código 0; PostRunErrors = []",
           "código %d; PostRunErrors = %s; TasklistQuery = %s" % (r["rc"], s["PostRunErrors"], s["TasklistQuery"]),
           r["rc"] == 0 and s["PostRunErrors"] == [], dict(s, StderrTail=r["stderr"][-600:]))
    root_e, _ = make_copy("case-ii-nolaunch", subs=[("hijo-inexistente", "    exe = sys.executable\n    for cand in",
                                                      '    exe = r"D:\\r62-a6r3\\launchfix\\no-such-python.exe"\n    for cand in')])
    r = run(["python", "-B", "kit/launch.py", "--record-probe"], root_e)
    s = probe_summary(r["json"])
    le = s["LaunchError"] or {}
    record("ii-e", "el hijo no arranca (copia de prueba: ejecutable inexistente): el registro se escribe igual, con LaunchError",
           "python -B kit/launch.py --record-probe",
           "código 1 sin traza fatal; registro escrito; LaunchError.Class = FileNotFoundError; temporal borrado",
           "código %d; registro escrito = %s; LaunchError = %s; temporal borrado = %s" % (r["rc"], s["RecordWritten"], le.get("Class"),
                                                                                         s["TempDirRemoved"]),
           r["rc"] == 1 and r["json"] is not None and s["RecordWritten"] is True and le.get("Class") == "FileNotFoundError" and
           s["TempDirRemoved"] is True, dict(s, StderrTail=r["stderr"][-600:]))
    root_f, _ = make_copy("case-ii-timeout", subs=[("tope-2-s", "PROBE_TIMEOUT_S = 120", "PROBE_TIMEOUT_S = 2"),
                                                    ("hijo-que-no-termina", "RUN_JSON_FIELDS = (",
                                                     'PROBE_CHILD = "import time; time.sleep(60)"   # prueba: hijo que no termina\nRUN_JSON_FIELDS = (')])
    r = run(["python", "-B", "kit/launch.py", "--record-probe"], root_f)
    s = probe_summary(r["json"])
    term = any("terminado por el invocador" in p for p in s["Problems"] or [])
    record("ii-f", "tope alcanzado (copia de prueba: tope 2 s, hijo que duerme): terminate, registro completo escrito",
           "python -B kit/launch.py --record-probe",
           "código 1; registro escrito con todos los campos (MissingFields = []); PostRunErrors = []; la sonda informa «terminado por el invocador»",
           "código %d; registro escrito = %s; faltan = %s; PostRunErrors = %s; terminado = %s; ProcessGoneAfter = %s"
           % (r["rc"], s["RecordWritten"], s["MissingFields"], s["PostRunErrors"], term, s["ProcessGoneAfter"]),
           r["rc"] == 1 and s["RecordWritten"] is True and s["MissingFields"] == [] and s["PostRunErrors"] == [] and term and
           s["ProcessGoneAfter"] is True, dict(s, Seconds=r["seconds"]))

    # ---------------- iii: fragmento original de la ronda 2
    lines = open(os.path.join(FROZEN_KIT, "launch.py"), encoding="utf-8").read().split("\n")
    assert lines[266] == ROUND2_L267 and lines[267] == ROUND2_L268, "las líneas 267-268 del launch.py de la ronda 2 no son las esperadas"
    root4 = os.path.join(WORK, "case-iii")
    os.makedirs(root4)
    snippet = "\n".join([
        '"""Fragmento ORIGINAL de la ronda 2: launch.py L267-268, copiadas literalmente del kit congelado (ver test_launchfix.py)."""',
        "import json, subprocess, sys, threading, types",
        "thread_errors = []",
        "_prev = threading.excepthook",
        "def _hook(a):",
        "    thread_errors.append({'Thread': a.thread.name if a.thread else None, 'Class': a.exc_type.__name__, 'Message': str(a.exc_value)})",
        "    _prev(a)",
        "threading.excepthook = _hook",
        "p = types.SimpleNamespace(pid=int(sys.argv[1]))",
        "rec = {}",
        "res = {'Utf8Mode': sys.flags.utf8_mode, 'XOptions': {k: str(v) for k, v in sys._xoptions.items()}, 'Exception': None}",
        "try:",
        lines[266] + "   # launch.py L267 (ronda 2)",
        "    res['QStdoutIsNone'] = q.stdout is None",
        lines[267] + "   # launch.py L268 (ronda 2)",
        "    res['ProcessGoneAfter'] = rec['ProcessGoneAfter']",
        "except Exception as e:",
        "    res['Exception'] = {'Class': type(e).__name__, 'Message': str(e)}",
        "res['ThreadErrors'] = thread_errors",
        "print(json.dumps(res))", ""])
    with open(os.path.join(root4, "snippet_round2.py"), "w", encoding="utf-8", newline="\n") as f:
        f.write(snippet)
    round2_env = not facts["TasklistNonexistentPid"]["ValidUtf8"]
    snippet_case("iii-a", root4, ["python", "-X", "utf8", "-B", "snippet_round2.py", str(NONEXISTENT_PID)], round2_env)
    snippet_case("iii-b", root4, ["python", "-B", "snippet_round2.py", str(NONEXISTENT_PID)], round2_env)

    # ---------------- v: dry-run (preflight sustituida, run y clon desechables, binario inexistente)
    root5 = os.path.join(WORK, "case-v-dry")
    sub_rc, params, runp = sub_run_clone(root5)
    root5, _ = make_copy("case-v-dry", subs=[sub_rc, SUB_BIN_NONE, SUB_PREFLIGHT_STUB], params=params)
    snap = snapshot(root5)
    r = run(["python", "-B", "kit/launch.py", "--dry-run"], root5)
    js = r["json"] or {}
    s = probe_summary(js)
    holds = r["rc"] == 2 and "DryRun" in js and s["GuardOk"] is True and s["RecordProbeOk"] is True and js.get("RecordProbeOk") is True and \
        "11-OwnerAndAuthorizationGate" in (js.get("Checks") or {}) and not os.path.exists(os.path.join(runp, "launch")) and snapshot(root5) == snap
    record("v", "--dry-run informa de la guarda y de la sonda (preflight sustituida en la copia de prueba)", "python -B kit/launch.py --dry-run",
           "código 2; DryRun con FrozenCommandGuard.Ok = true y RecordProbe.Ok = true; <run>/launch no creado; copia sin cambios",
           "código %d; DryRun = %s; guarda Ok = %s; RecordProbe.Ok = %s; RecordProbeOk = %s; <run>/launch = %s; copia sin cambios = %s"
           % (r["rc"], "DryRun" in js, s["GuardOk"], s["RecordProbeOk"], js.get("RecordProbeOk"), os.path.exists(os.path.join(runp, "launch")),
              snapshot(root5) == snap), holds, dict(s, Keys=sorted(js)))

    # ---------------- vi: lanzamiento real con la sonda fallando / pasando (preflight sustituida, binario inexistente)
    root6 = os.path.join(WORK, "case-vi-real")
    sub_rc, params, runp = sub_run_clone(root6)
    root6, _ = make_copy("case-vi-real", subs=[SUB_GUARD_OFF, SUB_FAULT, sub_rc, SUB_BIN_NONE, SUB_PREFLIGHT_STUB], params=params)
    snap = snapshot(root6)
    r = run(["python", "-X", "utf8", "-B", "kit/launch.py"], root6)
    js = r["json"] or {}
    s = probe_summary(js)
    holds = r["rc"] == 2 and "la sonda de registro falló" in str(js.get("Refused")) and "Checks" not in js and \
        not os.path.exists(os.path.join(runp, "launch")) and snapshot(root6) == snap
    record("vi-a", "lanzamiento real (guarda desactivada, fallo inyectado, -X utf8): la sonda falla y se niega ANTES de la preflight",
           "python -X utf8 -B kit/launch.py", "código 2; Refused por la sonda; sin preflight; <run>/launch no creado",
           "código %d; Refused = %s; PostRunErrors = %s; <run>/launch = %s" % (r["rc"], js.get("Refused"), [(e.get("Step"), e.get("Class"))
                                                                               for e in s["PostRunErrors"] or []],
                                                                               os.path.exists(os.path.join(runp, "launch"))), holds, s)
    r = run(["python", "-B", "kit/launch.py"], root6)
    js = r["json"] or {}
    holds = r["rc"] == 2 and js.get("Refused") == "preflight fallida: no se lanza" and not os.path.exists(os.path.join(runp, "launch")) and \
        snapshot(root6) == snap
    record("vi-b", "lanzamiento real con la orden congelada (misma copia): la sonda pasa y sigue a la preflight (sustituida → se niega)",
           "python -B kit/launch.py", "código 2; Refused = «preflight fallida» (se llegó a la preflight); <run>/launch no creado",
           "código %d; Refused = %s; <run>/launch = %s" % (r["rc"], js.get("Refused"), os.path.exists(os.path.join(runp, "launch"))), holds)

    # ---------------- vii: adaptador, kit congelado, README, restos, saneamiento
    rerun = os.path.join(WORK, "adapt-rerun")
    os.makedirs(rerun)
    r = run(["python", "-B", os.path.join(OUT, "adapt_launch_r3.py"), os.path.join(FROZEN_KIT, "launch.py"), rerun], OUT)
    same = all(os.path.isfile(os.path.join(rerun, n)) and sha_file(os.path.join(rerun, n)) == sha_file(os.path.join(OUT, n))
               for n in ("launch.r3-template.py", "launch.r3-template.diff"))
    subs = [(x.get("Substitution"), x.get("Matches")) for x in ((r["json"] or {}).get("Substitutions") or [])]
    record("vii-a", "el adaptador reproduce la plantilla y el diff byte a byte (cada sustitución exacta y única)",
           "python -B adapt_launch_r3.py <launch.py de la ronda 2> <dir desechable>", "código 0; 12 sustituciones con 1 coincidencia; mismos SHA-256",
           "código %d; sustituciones = %d (todas 1: %s); mismos bytes = %s" % (r["rc"], len(subs), all(m == 1 for _, m in subs), same),
           r["rc"] == 0 and len(subs) == 12 and all(m == 1 for _, m in subs) and same, {"Substitutions": subs})
    frozen_after = {n: sha_file(os.path.join(FROZEN_KIT, n)) for n in FROZEN_FILES}
    r2launch_after = snapshot(ROUND2_RUN_LAUNCH) if os.path.isdir(ROUND2_RUN_LAUNCH) else None
    record("vii-b", "kit congelado de la ronda 2 intacto y <run>/launch de la ronda 2 sin cambios",
           "(SHA-256 antes y después; listado de D:\\r62-arch-a6r2-run\\launch antes y después)",
           "SHA-256 = manifiesto antes y después; listado idéntico",
           "SHA-256 = manifiesto: antes %s, después %s; listado idéntico = %s" % (frozen_before == FROZEN_FILES, frozen_after == FROZEN_FILES,
                                                                                 r2launch_before == r2launch_after),
           frozen_before == FROZEN_FILES and frozen_after == FROZEN_FILES and r2launch_before == r2launch_after,
           {"RunLaunchEntries": len(r2launch_after or [])})
    tpl = open(TEMPLATE, encoding="utf-8").read()
    m = re.search(r'^FROZEN_COMMAND = "([^"]+)"', tpl, re.M)
    frozen_cmd = m.group(1) if m else None
    r2_doc = "`%s`" % frozen_cmd in open(FROZEN_README, encoding="utf-8").read() if frozen_cmd else False
    readme = os.path.join(OUT, "README.md")
    blocks = re.findall(r"```[a-z]*\n(.*?)```", open(readme, encoding="utf-8").read(), re.S) if os.path.isfile(readme) else []
    in_block = any(b.strip() == frozen_cmd for b in blocks)
    record("vii-c", "FROZEN_COMMAND = la orden documentada de la ronda 2 y = el bloque del README", "(lectura de la plantilla y de los README)",
           "FROZEN_COMMAND «python -B kit/launch.py»; aparece literal en el README de la ronda 2; un bloque del README de OUT es exactamente esa orden",
           "FROZEN_COMMAND = %r; en el README de la ronda 2 = %s; bloque exacto en README.md = %s" % (frozen_cmd, r2_doc, in_block),
           frozen_cmd == "python -B kit/launch.py" and r2_doc and in_block)
    pyc = glob.glob(os.path.join(WORK, "**", "__pycache__"), recursive=True) + glob.glob(os.path.join(OUT, "**", "__pycache__"), recursive=True)
    left = probe_temps() - temps_before
    record("vii-d", "sin __pycache__ en las copias ni en OUT; ningún temporal de la sonda queda en %TEMP%", "(listado)",
           "0 y 0", "__pycache__ = %d; temporales r62-record-probe-* nuevos = %d" % (len(pyc), len(left)), not pyc and not left)

    out = {"Schema": "rackcad-i62-launchfix-tests/1", "Started": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime(t_start)),
           "Finished": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()), "Runner": "python -B test_launchfix.py", "HostFacts": facts,
           "Template": {"File": "launch.r3-template.py", "Sha256": sha_file(TEMPLATE)}, "FrozenKitSha256": frozen_after, "Results": RESULTS}
    text = sanitize(json.dumps(out, ensure_ascii=False, indent=1))
    bad = {}
    for d, _, fs in os.walk(OUT):
        for n in fs:
            if n == "test-results.json":
                continue
            lk = leaks(open(os.path.join(d, n), encoding="utf-8", errors="replace").read())
            if lk:
                bad[os.path.relpath(os.path.join(d, n), OUT)] = len(lk)
    lk = leaks(text)
    if lk:
        bad["test-results.json"] = len(lk)
    RESULTS.append({"Id": "vii-e", "Case": "saneamiento de OUT (perfil, cuenta, forma 8.3, correos)", "Command": "(barrido de los archivos)",
                    "Expected": "ningún archivo con la ruta del perfil, el nombre de la cuenta, su forma 8.3 o correos",
                    "Observed": "archivos con fugas: %s" % (sorted(bad) or "ninguno"), "Holds": not bad, "Details": {}})
    print("%-5s %-5s %s" % ("vii-e", "OK" if not bad else "FALLA", "saneamiento de OUT"))
    out["Results"] = RESULTS
    out["Summary"] = {"Cases": len(RESULTS), "Hold": sum(1 for x in RESULTS if x["Holds"]), "Fail": [x["Id"] for x in RESULTS if not x["Holds"]]}
    text = sanitize(json.dumps(out, ensure_ascii=False, indent=1))
    with open(os.path.join(OUT, "test-results.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(text + "\n")
    print(json.dumps(out["Summary"], ensure_ascii=False))
    return 0 if not out["Summary"]["Fail"] else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main())
