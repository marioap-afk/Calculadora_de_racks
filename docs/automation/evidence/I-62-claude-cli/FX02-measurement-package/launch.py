"""I-62 / FX-02 — lanzador de la ÚNICA invocación read-only de claude-cli que mide las operaciones 5, 7, 8 y 9 del descriptor
adapters/claude-cli.md (decisiones §66, punto 6; autoridad: CLAUDE-CLI-MEDICION-FX02 = A del Owner y la disposición del Coordinator que nombra
el SHA-256 de MANIFEST.json). Lo ejecuta la sesión principal, una sola vez, desde PowerShell, sin reintento. Quien prepara el paquete solo
ejecuta `--dry-run`, que no ejecuta el binario ni escribe nada.

Preflight (se NIEGA a lanzar, código 2 y sin escribir nada, si falla cualquiera):
  G1 decisiones: el archivo de decisiones de I-62 en HEAD del worktree contiene como LÍNEAS COMPLETAS exactas la línea del Owner de
     gates.template.json y el marcador del Coordinator con el SHA-256 de MANIFEST.json;
  G2 binario: %APPDATA%\\Claude\\claude-code\\2.1.293\\83cb0bd7fed4\\claude.exe presente, SHA-256 8693c4a0…, Authenticode válida de Anthropic;
  G3 clon: D:\\r62-fixture\\tmp-ccli-meas en d30fb6a9, solo main, sin remoto, limpio con ignorados, blob del objeto, sin .claude/, sin enlaces,
     solo la historia de HEAD, core.autocrlf=false;
  G4 procesos: ninguno con el session id del paquete en su línea de órdenes (ningún claude lanzado por el paquete vivo) y ninguno ajeno con la
     ruta del clon;
  G5 un solo uso: run/ no existe; G6 integridad del paquete (MANIFEST.json); G7 CLAUDE_CONFIG_DIR ausente; G8 sesión nueva (sin transcripción
  con el session id ni carpeta de proyecto del clon); G9 línea de órdenes < 32 000; G10 muestreador CIM operativo; G11 argumentos = plantilla.
Lanzamiento (solo con todo en Ok): crea run/ y run/ONE-SHOT.marker (nunca se borran: marcan el único uso), huella antes, muestreador CIM (1 s)
y rastreador rápido (100 ms) en marcha ANTES del proceso, vuelve a medir el binario justo antes, lanza la ruta fijada con los flags medidos +
--session-id + --json-schema (el esquema canónico VERBATIM, compacto), prompt por stdin y cwd = el clon; tope 1 800 s (árbol matado si vence).
Tras la salida: S1 a los 10 s (identidades del árbol vivas por handle y por CIM, huérfanos, procesos con la ruta del clon); si queda algo, S2 a
los 60 s; si aún queda, se mata y S3. Después para los muestreadores, huella después, clon después y hashes. No hay `--version` ni `auth status`
(una sola ejecución del binario); un fallo rápido «Failed to refresh OAuth token» se clasifica TRANSPORT_AUTH_REFRESH_CONFLICT → INVALID_LAUNCH.
Uso:  python -I -B launch.py            → preflight y, si todo pasa, el único lanzamiento
      python -I -B launch.py --dry-run  → solo preflight (sin ejecutar el binario, sin escribir); admite --clone <ruta> para ensayo
"""
import datetime
import importlib.util
import json
import os
import shutil
import subprocess
import sys
import tempfile
import threading
import time

PKG = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(PKG, name + ".py"))
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


C = _load("ccli_common")
PT = _load("proctree")
K = C.load_constants(PKG)
GATES = json.load(open(os.path.join(PKG, K["GatesFile"]), encoding="utf-8"))
BIN = C.expand(K["Binary"]["Display"])
SID = K["SessionId"]
CLONE = K["Clone"]["Path"]
RUN = os.path.join(PKG, "run")
LAUNCH = os.path.join(RUN, "launch")
PROJECTS = os.path.join(os.path.expanduser("~"), ".claude", "projects")
CLOSED = [n.lower() for n in K["ClosedProcessList"]]
EPOCH_FT = 116444736000000000


def ft_now():
    return time.time_ns() // 100 + EPOCH_FT


def ps_exe():
    return shutil.which("pwsh") or shutil.which("powershell") or "powershell"


def sampler_once(out_dir, clone, tree_identities=(), window_start=None, window_end=None, exclude=()):
    os.makedirs(out_dir, exist_ok=True)
    out = os.path.join(out_dir, "cim-once-%d.json" % int(time.time() * 1000))
    cfg = {"Mode": "Once", "OutFile": out, "ClonePath": clone, "Sid": SID, "WindowStartUtc": window_start, "WindowEndUtc": window_end,
           "Closed": CLOSED, "ExcludePids": list(exclude) + [os.getpid()], "TreeIdentities": list(tree_identities)}
    r = subprocess.run([ps_exe(), "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", os.path.join(PKG, "sampler.ps1")],
                       input=json.dumps(cfg) + "\n", capture_output=True, text=True, timeout=120)
    try:
        o = json.load(open(out, encoding="utf-8-sig"))
    except (OSError, ValueError):
        o = {"Error": (r.stderr or "")[-400:], "ExitCode": r.returncode}
    return o, out


def compact_schema():
    return json.dumps(json.load(open(os.path.join(PKG, K["SchemaFile"]), encoding="utf-8")), separators=(",", ":"), ensure_ascii=True)


def build_args():
    return [BIN] + list(K["Flags"]) + ["--session-id", SID, "--json-schema", compact_schema()]


def templatize(args):
    out, i = [], 0
    while i < len(args):
        out.append(args[i])
        if args[i] == "--session-id" and i + 1 < len(args):
            out.append("<session-id>")
            i += 1
        elif args[i] == "--json-schema" and i + 1 < len(args):
            out.append("<json-schema>")
            i += 1
        i += 1
    return out


def decisions_text():
    repo = C.expand(K["Decisions"]["Repo"])
    ref, path = K["Decisions"]["Ref"], K["Decisions"]["Path"]
    r = subprocess.run(["git", "-C", repo, "show", "%s:%s" % (ref, path)], capture_output=True)
    head = C.git(repo, "rev-parse", ref).strip()
    blob = C.git(repo, "rev-parse", "%s:%s" % (ref, path)).strip()
    return (r.stdout.decode("utf-8", "replace") if r.returncode == 0 else ""), head, blob


def gate_processes(clone):
    """G4: ningún proceso con el session id del paquete en su línea de órdenes (ningún claude lanzado por el paquete sigue vivo) y ningún
    proceso ajeno con la ruta del clon. Se registran solo cuentas y nombres de imagen."""
    tmp = tempfile.mkdtemp(prefix="ccli-pre-")
    once, _ = sampler_once(tmp, clone)
    shutil.rmtree(tmp, ignore_errors=True)
    ok_once = "Total" in once and once.get("Total", 0) > 0
    g = {"Ok": ok_once and not once.get("SidProcs") and not once.get("CloneRefs"),
         "SidProcs": len(once.get("SidProcs") or []), "CloneRefs": len(once.get("CloneRefs") or []),
         "Names": sorted({p["Name"] for p in (once.get("SidProcs") or []) + (once.get("CloneRefs") or [])})}
    return g, once


def preflight(clone, dry_run):
    c = {}
    man = C.check_manifest(PKG, K["AllowedUnlistedFiles"])
    c["G6-Package"] = man
    txt, head, blob = decisions_text()
    g1 = C.gate_decisions(txt, GATES["OwnerLine"], GATES["CoordinatorMarkerTemplate"], GATES["Placeholder"], man.get("ManifestSha256"))
    g1["OwnerLineTemplateIntegrity"] = C.sha_bytes(GATES["OwnerLine"].encode("utf-8")) == GATES["OwnerLineSha256"]
    g1["Ok"] = g1["Ok"] and g1["OwnerLineTemplateIntegrity"] and bool(txt)
    c["G1-Decisions"] = dict(g1, Repo=K["Decisions"]["Repo"], Ref=K["Decisions"]["Ref"], Commit=head, Blob=blob)
    g2 = C.gate_binary(BIN, K["Binary"]["Sha256"])
    vdir = C.expand(K["Binary"]["VersionsDir"])
    g2["Path"] = K["Binary"]["Display"]
    g2["InstalledVersions"] = sorted(os.listdir(vdir)) if os.path.isdir(vdir) else []
    c["G2-Binary"] = g2
    g3 = C.gate_clone(clone, K["Clone"]["Commit"], K["Clone"]["ObjectPath"], K["Clone"]["ObjectBlob"], K["Clone"]["RequiredPrefix"])
    g3["Path"] = clone
    g3["PathIsConstant"] = C.norm(clone) == C.norm(CLONE)
    if not dry_run:
        g3["Ok"] = g3["Ok"] and g3["PathIsConstant"]
    c["G3-Clone"] = g3
    c["G4-Processes"], once = gate_processes(clone)
    ok_once = "Total" in once and once.get("Total", 0) > 0
    c["G5-OneShot"] = C.gate_one_shot(PKG)
    g7 = C.gate_env(os.environ)
    c["G7-Env"] = {"Ok": g7["Ok"], "ClaudeConfigDirSet": g7["ClaudeConfigDirSet"], "ClaudeAnthropicNameCount": len(g7["ClaudeAnthropicNames"])}
    c["G8-NewSession"] = C.gate_new_session(PROJECTS, SID, [C.project_dir_name(clone)])
    args = build_args()
    cl = len(subprocess.list2cmdline(args))
    c["G9-CommandLine"] = {"Ok": cl < K["MaxCommandLineChars"], "Chars": cl}
    c["G10-Sampler"] = {"Ok": ok_once, "Total": once.get("Total"), "Error": once.get("Error")}
    c["G11-ArgsTemplate"] = {"Ok": templatize(args[1:]) == K["ArgsTemplate"], "SchemaRendering": K["SchemaRendering"],
                             "SchemaArgSha256": C.sha_bytes(compact_schema().encode("utf-8")),
                             "SchemaArgEqualsCanonical": json.loads(compact_schema()) == json.load(open(os.path.join(PKG, K["SchemaFile"]), encoding="utf-8"))}
    c["G11-ArgsTemplate"]["Ok"] = c["G11-ArgsTemplate"]["Ok"] and c["G11-ArgsTemplate"]["SchemaArgEqualsCanonical"]
    return c, all(v.get("Ok") is True for v in c.values()), args


# ------------------------------------------------------------------ medición (la usa el lanzamiento real y, con cmd.exe, el ensayo en seco)
def measure(argv, cwd, stdin_bytes, out_dir, T, clone_for_refs, allow_claude=False, pinned_path=None):
    """Lanza argv con los dos muestreadores en marcha y confirma la terminación del árbol. Escribe en out_dir: stdout.jsonl, stderr.txt,
    pid.json, proc-cim.jsonl, sampler-stderr.txt, proc-fast.json, termination.json. → registro (dict)."""
    exe_name = os.path.basename(argv[0]).lower()
    if "claude" in exe_name and (not allow_claude or os.environ.get("CCLI_DRYRUN") == "1"):
        raise RuntimeError("measure(): ejecutable claude rechazado fuera del lanzamiento autorizado")
    rec = {"WindowStartUtc": C.now()}
    cim_out = os.path.join(out_dir, "proc-cim.jsonl")
    stop_file = os.path.join(out_dir, "sampler.stop")
    pid_file = os.path.join(out_dir, "pid.json")
    cfg = {"Mode": "Loop", "PidFile": pid_file, "StopFile": stop_file, "OutFile": cim_out, "ClonePath": clone_for_refs,
           "PeriodMs": T["CimPeriodMs"], "MaxSeconds": T["TimeoutS"] + T["Settle2S"] + 600, "WindowStartUtc": rec["WindowStartUtc"], "Closed": CLOSED}
    se = open(os.path.join(out_dir, "sampler-stderr.txt"), "wb")
    sp = subprocess.Popen([ps_exe(), "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", os.path.join(PKG, "sampler.ps1")],
                          stdin=subprocess.PIPE, stdout=subprocess.DEVNULL, stderr=se)
    sp.stdin.write((json.dumps(cfg) + "\n").encode("utf-8"))
    sp.stdin.close()
    rec["SamplerPid"] = sp.pid
    t_wait = time.monotonic()
    while time.monotonic() - t_wait < T["SamplerStartS"]:
        if os.path.isfile(cim_out) and os.path.getsize(cim_out) > 0:
            break
        time.sleep(0.2)
    if not (os.path.isfile(cim_out) and os.path.getsize(cim_out) > 0):
        open(stop_file, "w").close()
        try:
            sp.wait(timeout=T["SamplerStopS"])
        except subprocess.TimeoutExpired:
            sp.kill()
        se.close()
        rec["Aborted"] = "SAMPLER_NOT_STARTED: el muestreador CIM no produjo ninguna muestra; no se lanza"
        return rec
    ft = PT.FastTracker(T["FastPeriodMs"] / 1000.0)
    ft.start()
    if pinned_path is not None:
        rec["ExecutableSha256BeforeLaunch"] = C.sha_file(pinned_path)
        if rec["ExecutableSha256BeforeLaunch"] != K["Binary"]["Sha256"]:
            rec["Aborted"] = "BINARY_CHANGED: el binario cambió entre la preflight y el lanzamiento; no se lanza"
            ft.stop()
            ft.close()
            open(stop_file, "w").close()
            try:
                sp.wait(timeout=T["SamplerStopS"])
            except subprocess.TimeoutExpired:
                sp.kill()
            se.close()
            return rec
    rec["LaunchStartUtc"] = C.now()
    launch_ft = ft_now()
    t_launch = time.monotonic()
    with open(os.path.join(out_dir, "stdout.jsonl"), "wb") as fo, open(os.path.join(out_dir, "stderr.txt"), "wb") as fe:
        p = subprocess.Popen(argv, cwd=cwd, stdin=subprocess.PIPE, stdout=fo, stderr=fe)
        rec["Pid"] = p.pid
        root_key = ft.set_root(p.pid, getattr(p, "_handle", None))
        root = ft.members.get(root_key, {})
        C.write_json(pid_file, {"Pid": p.pid, "CreationUtc": root.get("CreationUtc")})
        rec["RootKey"] = root_key

        def feed():
            try:
                p.stdin.write(stdin_bytes)
                p.stdin.close()
            except OSError as e:
                rec["StdinError"] = str(e)
        th = threading.Thread(target=feed, daemon=True)
        th.start()
        rec["TimedOut"] = False
        try:
            p.wait(timeout=T["TimeoutS"])
        except subprocess.TimeoutExpired:
            rec["TimedOut"] = True
            rec["TerminateTreeAt"] = C.now()
            rec["TaskkillTree"] = PT.taskkill_tree(p.pid)
            rec["KilledByHandle"] = ft.kill_alive()
            try:
                p.wait(timeout=T["KillGraceS"])
            except subprocess.TimeoutExpired:
                p.kill()
                p.wait()
        th.join(timeout=5)
    t_exit = time.monotonic()
    exit_ft = ft_now()
    rec.update({"ExitUtc": C.now(), "ExitCode": p.returncode, "DurationS": round(t_exit - t_launch, 3)})
    term = {"Checks": [], "Mode": None}

    def cim_tree_identities():
        ids = {}
        try:
            for line in open(cim_out, encoding="utf-8-sig"):
                try:
                    o = json.loads(line)
                except ValueError:
                    continue
                for r in o.get("Tree") or []:
                    ids[(r["Pid"], r["CreationUtc"])] = {"Pid": r["Pid"], "CreationUtc": r["CreationUtc"]}
        except OSError:
            pass
        return list(ids.values())

    def do_check(label):
        # primero la instantánea CIM (lenta: arranque de PowerShell) y después la fuente rápida, para que esta refleje el estado más reciente;
        # Residual = vivo según CUALQUIERA de las dos (conservador)
        tree_ids = [{"Pid": m["Pid"], "CreationUtc": m["CreationUtc"]} for m in ft.export()["Members"]]
        seen = {(t["Pid"], (t["CreationUtc"] or "")[:23]) for t in tree_ids}
        for t in cim_tree_identities():
            if (t["Pid"], (t["CreationUtc"] or "")[:23]) not in seen:
                tree_ids.append(t)
        once, once_path = sampler_once(out_dir, clone_for_refs, tree_ids, rec["LaunchStartUtc"], rec["ExitUtc"], exclude=[sp.pid])
        fast = ft.check(label)
        snap = PT.snapshot_with_creation(set(CLOSED))
        tree_pc = {(m["Pid"], m["Creation100ns"]) for m in ft.export()["Members"]}
        orphans = PT.find_orphans(snap, tree_pc, launch_ft - 10_000_000, exit_ft, set(CLOSED))
        chk = {"Label": label, "Fast": fast, "CimOnce": {k: once.get(k) for k in ("Utc", "Total", "AliveTree", "CloneRefs", "ClosedInWindow", "Error")},
               "CimOnceFile": os.path.basename(once_path), "OrphansFast": orphans}
        chk["Residual"] = bool(fast["Alive"]) or bool(once.get("AliveTree")) or bool(orphans)
        term["Checks"].append(chk)
        return chk

    time.sleep(max(0.0, T["Settle1S"] - (time.monotonic() - t_exit)))
    s = do_check("S1")
    if s["Residual"]:
        time.sleep(max(0.0, T["Settle2S"] - (time.monotonic() - t_exit)))
        s = do_check("S2")
        if s["Residual"]:
            term["KilledResidual"] = ft.kill_alive()
            time.sleep(2)
            s = do_check("S3")
            term["Mode"] = "KILLED_RESIDUAL"
        else:
            term["Mode"] = "NATURAL_BY_S2"
    else:
        term["Mode"] = "NATURAL_BY_S1"
    if rec["TimedOut"]:
        term["Mode"] = "TIMEOUT_TREE_KILLED"
    t_end_cover = time.monotonic()
    open(stop_file, "w").close()
    try:
        sp.wait(timeout=T["SamplerStopS"])
        rec["SamplerExit"] = sp.returncode
    except subprocess.TimeoutExpired:
        sp.kill()
        rec["SamplerExit"] = "KILLED"
    se.close()
    ft.stop()
    exp = ft.export()
    exp["Gaps"] = {"Launch→S1": ft.gaps(t_launch, t_end_cover)}
    exp["LaunchFt100"], exp["ExitFt100"] = launch_ft, exit_ft
    ft.close()
    C.write_json(os.path.join(out_dir, "proc-fast.json"), exp)
    C.write_json(os.path.join(out_dir, "termination.json"), term)
    rec["TerminationMode"] = term["Mode"]
    return rec


def main(argv):
    dry_run = "--dry-run" in argv
    clone = CLONE
    if "--clone" in argv or os.environ.get("CCLI_DRY_CLONE"):
        if not dry_run:
            print(json.dumps({"Refused": "--clone / CCLI_DRY_CLONE solo se admiten con --dry-run"}, ensure_ascii=False))
            return 2
        # por el entorno, para que la ruta del clon de ensayo no aparezca en ninguna línea de órdenes (G4)
        clone = argv[argv.index("--clone") + 1] if "--clone" in argv else os.environ["CCLI_DRY_CLONE"]
    checks, ok, args = preflight(clone, dry_run)
    if dry_run:
        print(json.dumps({"DryRun": "solo preflight: el binario no se ejecuta, no se escribe nada y no se lanza",
                          "BlockingChecks": [k for k, v in checks.items() if v.get("Ok") is not True], "Checks": checks},
                         ensure_ascii=False, indent=1))
        return 2
    if not ok:
        print(json.dumps({"Refused": "preflight fallida: no se lanza", "BlockingChecks": [k for k, v in checks.items() if v.get("Ok") is not True],
                          "Checks": checks}, ensure_ascii=False, indent=1))
        return 2
    try:
        os.mkdir(RUN)
    except FileExistsError:
        print(json.dumps({"Refused": "run/ ya existe: una sola invocación, sin reintento"}, ensure_ascii=False))
        return 2
    marker = {"Marker": "ONE-SHOT", "RunId": K["RunId"], "SessionId": SID, "CreatedUtc": C.now(),
              "ManifestSha256": checks["G6-Package"]["ManifestSha256"], "Rule": "no borrar nunca: marca el único uso del paquete"}
    with open(os.path.join(RUN, "ONE-SHOT.marker"), "x", encoding="utf-8") as f:
        f.write(json.dumps(marker, ensure_ascii=False, indent=1) + "\n")
    os.mkdir(LAUNCH)
    C.write_json(os.path.join(LAUNCH, "preflight.json"), {"SessionId": SID, "At": C.now(), "Checks": checks})
    C.write_json(os.path.join(LAUNCH, "fingerprint-before.json"), C.fingerprint(CLONE))
    prompt = open(os.path.join(PKG, K["PromptFile"]), "rb").read()
    rec = {"RunId": K["RunId"], "SessionId": SID, "Executable": K["Binary"]["Display"], "Args": args[1:], "Cwd": CLONE,
           "PromptSha256": C.sha_bytes(prompt), "PromptBytes": len(prompt), "SchemaRendering": K["SchemaRendering"],
           "SchemaArgSha256": C.sha_bytes(args[-1].encode("utf-8")), "Timing": K["Timing"],
           "EnvironmentVariableNames": C.gate_env(os.environ)["ClaudeAnthropicNames"],
           "EnvironmentExtraNames": sorted(k for k in os.environ if k.upper() in C.ENV_EXTRA),
           "LauncherPid": os.getpid(), "ManifestSha256": checks["G6-Package"]["ManifestSha256"],
           "DecisionsCommit": checks["G1-Decisions"]["Commit"], "DecisionsBlob": checks["G1-Decisions"]["Blob"]}
    rec.update(measure(args, CLONE, prompt, LAUNCH, K["Timing"], CLONE, allow_claude=True, pinned_path=BIN))
    C.write_json(os.path.join(LAUNCH, "fingerprint-after.json"), C.fingerprint(CLONE))
    so_path = os.path.join(LAUNCH, "stdout.jsonl")
    so = open(so_path, "rb").read() if os.path.isfile(so_path) else b""
    rec["StdoutSha256"], rec["StdoutLines"] = C.sha_bytes(so), len(so.splitlines())
    se_path = os.path.join(LAUNCH, "stderr.txt")
    rec["StderrBytes"] = os.path.getsize(se_path) if os.path.isfile(se_path) else None
    transcript = os.path.join(PROJECTS, C.project_dir_name(CLONE), SID + ".jsonl")
    rec["Transcript"] = {"Path": "%USERPROFILE%\\.claude\\projects\\" + C.project_dir_name(CLONE) + "\\" + SID + ".jsonl",
                         "Exists": os.path.isfile(transcript), "Sha256": C.sha_file(transcript) if os.path.isfile(transcript) else None}
    rec["CloneAfter"] = {"Head": C.git(CLONE, "rev-parse", "HEAD").strip(),
                         "StatusPorcelainIgnored": C.git(CLONE, "status", "--porcelain", "--ignored", "--untracked-files=all")}
    C.write_json(os.path.join(LAUNCH, "run.json"), rec)
    print(json.dumps({k: rec.get(k) for k in ("RunId", "SessionId", "Pid", "LaunchStartUtc", "ExitUtc", "ExitCode", "TimedOut",
                                              "TerminationMode", "Aborted")}, ensure_ascii=False))
    return 0 if rec.get("ExitCode") == 0 else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8", errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
