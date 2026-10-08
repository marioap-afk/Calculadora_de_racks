"""I-62 A-2 corregida: lanzador de la UNA re-revisión formal del Architect por una celda claude-cli medida y elegible (decisiones §56, puntos 3 y
6-9; autorización del Owner CLAUDE-CLI-I62 = A). Lo ejecuta la sesión principal, una sola vez y sin reintento; quien prepara el kit no lo ejecuta.

Antes de lanzar comprueba, y se NIEGA a lanzar (código 2, sin escribir nada) si falla cualquiera de estas condiciones:
  1. integridad del kit: cada archivo de kit-manifest.json con su SHA-256 (incluidos este script, transport_gate.py y transport-gate.json);
  2. transporte del cierre = constantes de este script (flags, --add-dir, session id, cwd, binario) y ArgsTemplate del cierre = la plantilla;
  3. binario elegible: existe en %APPDATA%, ruta resuelta = la del cierre, SHA-256 = 8693c4a0…, firma Authenticode válida de «Anthropic, PBC»;
  4. versión: `claude.exe --version` = «2.1.293 (Claude Code)»;
  5. autenticación: `claude.exe auth status --json` con loggedIn = true (solo se conservan loggedIn, authMethod y apiProvider; ni correo ni
     organización; ninguna credencial se lee);
  6. clon: HEAD = el commit, rama única main, sin remoto, árbol limpio con ignorados, blobs designados, sin enlaces ni uniones;
  7. run: exactamente los cuatro archivos (sin launch/), cada uno = copia del kit = RunFileHashes del cierre = kit-manifest.json;
  8. sesión nueva: sin transcripción con el session id fijado en ningún proyecto y sin directorio de proyecto para el clon (sin memoria previa);
  9. longitud de la línea de órdenes de Windows (< 32 000 caracteres);
 10. compuerta de transporte (desviación D-01): transport-gate.json en MEASURED (sonda C3 de la lista exacta con --add-dir) o ACCEPTED (aceptación
     explícita registrada), con el SHA-256 de cada registro fijado y sus predicados satisfechos (transport_gate.evaluate); la plantilla de los
     argumentos que se van a lanzar = la de la compuerta. En PENDING se niega: la lista con --add-dir no está medida.
Después escribe <run>/launch/preflight.json y las copias de los registros de la compuerta (transport-characterization.json y, en ACCEPTED,
transport-acceptance.json); vuelve a medir el binario JUSTO antes de lanzarlo (si su SHA-256 cambió, aborta sin lanzar) y lanza la ruta resuelta
con la lista exacta de argumentos (flags medidos + --add-dir del run + --session-id + --json-schema compacto), el prompt (bytes de prompt.md del
run) por stdin y cwd = el clon. Aplica el tope de 3 600 s con terminación (terminate y, 60 s después, kill) y escribe en <run>/launch/:
stdout.jsonl, stderr.txt y run.json (ruta resuelta y SHA-256 medidos del ejecutable, PID, inicio y fin, código de salida, terminado o no,
SHA-256 de stdout y de la transcripción, clon después).
Uso: python -B launch.py             → preflight y, si todo pasa, el único lanzamiento
     python -B launch.py --dry-run   → solo la preflight, sin `auth status`, sin escribir y sin lanzar (código 2 siempre)
"""
import datetime
import glob
import hashlib
import importlib.util
import json
import os
import stat
import subprocess
import sys
import threading

KIT = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("transport_gate", os.path.join(KIT, "transport_gate.py"))
TG = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(TG)
CLONE, RUN = r"D:\r62-arch-a2r", r"D:\r62-arch-a2r-run"
REV = "fd411b136f888ecf301f9edaa7fcc068da8dbf22"
SESSION_ID = "26a89860-a107-4135-9981-a46f95816d16"
BIN_DISPLAY = r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe"
BIN = os.path.join(os.environ.get("APPDATA", ""), "Claude", "claude-code", "2.1.293", "83cb0bd7fed4", "claude.exe")
BIN_SHA = "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa"
VERSION = "2.1.293 (Claude Code)"
FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
         "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
ADD_DIR = ["--add-dir", RUN]   # D-01: solo se lanza si transport-gate.json ata la medición C3 o la aceptación registrada
RUN_FILES = ["order.txt", "prompt.md", "delta.diff", "A-2.d47f71b6.md"]
TIMEOUT_S, KILL_GRACE_S = 3600, 60
PROJECTS = os.path.join(os.path.expanduser("~"), ".claude", "projects")
TRANSCRIPT = os.path.join(PROJECTS, "D--r62-arch-a2r", SESSION_ID + ".jsonl")
now = lambda: datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%fZ")


def sha_file(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest()


def git(*a):
    return subprocess.run(["git", "-C", CLONE] + list(a), capture_output=True, text=True).stdout


def build_args(schema_path):
    compact = json.dumps(json.load(open(schema_path, encoding="utf-8")), separators=(",", ":"), ensure_ascii=True)
    return [BIN] + FLAGS + ADD_DIR + ["--session-id", SESSION_ID, "--json-schema", compact]


def same_path(a, b):
    return TG.norm(TG.expand(a)) == TG.norm(TG.expand(b))


def preflight(dry_run=False):
    c = {}
    closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
    manifest = json.load(open(os.path.join(KIT, "kit-manifest.json"), encoding="utf-8"))
    listed = set(manifest["Files"]) | {"kit-manifest.json"}
    bad = [n for n, r in manifest["Files"].items() if not os.path.isfile(os.path.join(KIT, n)) or sha_file(os.path.join(KIT, n)) != r["Sha256"]]
    unlisted = sorted(n for n in os.listdir(KIT) if os.path.isfile(os.path.join(KIT, n)) and n not in listed)
    c["1-KitIntegrity"] = {"Ok": not bad and not unlisted, "Mismatches": bad, "Unlisted": unlisted}
    t = closure["Transport"]
    args = build_args(os.path.join(KIT, "result.schema.json"))
    c["2-ClosureTransport"] = {"Ok": t["Flags"] == FLAGS and t["AddDir"] == ADD_DIR and t["SessionId"] == SESSION_ID and t["Cwd"] == CLONE and
                               t["Binary"]["Path"] == BIN_DISPLAY and t["Binary"]["Sha256"] == BIN_SHA and t["Binary"]["Version"] == VERSION and
                               closure["AuthorityRevision"] == REV and t.get("ArgsTemplate") == TG.templatize(args[1:])}
    m = TG.measure_binary(BIN)
    c["3-Binary"] = dict({"Ok": m["Exists"] and m["Sha256"] == BIN_SHA and same_path(m["Path"], BIN_DISPLAY) and m["Authenticode"] == "Valid" and
                          m["SignerIsAnthropic"]}, **m)
    ver = TG.cli_version(BIN, KIT) if c["3-Binary"]["Ok"] else None
    c["4-Version"] = {"Ok": ver == VERSION, "Got": ver}
    if dry_run:
        c["5-Auth"] = {"Ok": None, "NotRun": "--dry-run: `auth status` no se ejecuta"}
    else:
        auth = TG.auth_status(BIN, KIT) if c["4-Version"]["Ok"] else {}
        c["5-Auth"] = dict({"Ok": auth.get("loggedIn") is True}, **auth)
    reparse = []
    for root, dirs, fs in os.walk(CLONE, followlinks=False):
        for n in dirs + fs:
            st = os.lstat(os.path.join(root, n))
            if stat.S_ISLNK(st.st_mode) or (getattr(st, "st_file_attributes", 0) & 0x400):
                reparse.append(n)
    blobs_bad = [r["Path"] for r in closure["CanonicalInputs"] if git("rev-parse", REV + ":" + r["Path"]).strip() != r["Blob"]]
    head, status = git("rev-parse", "HEAD").strip(), git("status", "--porcelain", "--ignored")
    branches, remotes = git("branch", "--list", "--format=%(refname:short)").split(), git("remote").split()
    c["6-Clone"] = {"Ok": head == REV and status == "" and branches == ["main"] and remotes == [] and not blobs_bad and not reparse,
                    "Head": head, "Clean": status == "", "Branches": branches, "Remotes": remotes, "BlobMismatches": blobs_bad, "ReparsePoints": len(reparse)}
    listing = sorted(os.listdir(RUN))
    run_bad = []
    for n in RUN_FILES:
        s = sha_file(os.path.join(RUN, n)) if os.path.isfile(os.path.join(RUN, n)) else None
        if not (s and s == sha_file(os.path.join(KIT, n)) == closure["RunFileHashes"][n]["Sha256"] == manifest["RunFiles"][n]["Sha256"]):
            run_bad.append(n)
    c["7-RunFiles"] = {"Ok": listing == sorted(RUN_FILES) and not run_bad, "Listing": listing, "Mismatches": run_bad}
    prior = glob.glob(os.path.join(PROJECTS, "*", SESSION_ID + ".jsonl"))
    c["8-NewSession"] = {"Ok": not prior and not os.path.exists(os.path.dirname(TRANSCRIPT)), "PriorTranscripts": len(prior),
                         "ProjectDirForCloneExists": os.path.exists(os.path.dirname(TRANSCRIPT))}
    cl = len(subprocess.list2cmdline(args))
    c["9-CommandLine"] = {"Ok": cl < 32000, "Chars": cl}
    gate = json.load(open(os.path.join(KIT, TG.GATE_FILE), encoding="utf-8"))
    char_b, acc_b = TG.read_pinned(gate.get("Characterization")), TG.read_pinned(gate.get("Acceptance"))
    ev = TG.evaluate(gate, closure, char_b, acc_b)
    launch_template_ok = TG.templatize(args[1:]) == ev["ArgsTemplate"] == gate.get("ArgsTemplate")
    c["10-TransportGate"] = {"Ok": ev["Ok"] and launch_template_ok, "Status": ev["Status"], "LaunchArgsMatchTemplate": launch_template_ok,
                             "CharacterizationSha256": ev["CharacterizationSha256"], "AcceptanceSha256": ev["AcceptanceSha256"],
                             "Problems": ev["Problems"]}
    assert list(c) == TG.PREFLIGHT_CHECKS
    return c, all(v["Ok"] is True for v in c.values()), args, (char_b, acc_b)


def write_json(path, obj):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(obj, ensure_ascii=False, indent=1) + "\n")


def main(argv):
    dry_run = "--dry-run" in argv
    if os.path.exists(os.path.join(RUN, "launch")):
        print(json.dumps({"Refused": "ya existe %s\\launch: una sola invocación, sin reintento" % RUN}, ensure_ascii=False))
        return 2
    checks, ok, args, (char_b, acc_b) = preflight(dry_run)
    if dry_run:
        print(json.dumps({"DryRun": "solo preflight: no se escribe ni se lanza", "BlockingChecks": [k for k, v in checks.items() if v["Ok"] is False],
                          "Checks": checks}, ensure_ascii=False, indent=1))
        return 2
    if not ok:
        print(json.dumps({"Refused": "preflight fallida: no se lanza", "Checks": checks}, ensure_ascii=False, indent=1))
        return 2
    out_dir = os.path.join(RUN, "launch")
    os.makedirs(out_dir)
    write_json(os.path.join(out_dir, "preflight.json"), {"SessionId": SESSION_ID, "At": now(), "Checks": checks})
    open(os.path.join(out_dir, TG.COPY_CHARACTERIZATION), "wb").write(char_b)
    if acc_b is not None:
        open(os.path.join(out_dir, TG.COPY_ACCEPTANCE), "wb").write(acc_b)
    prompt = open(os.path.join(RUN, "prompt.md"), "rb").read()
    resolved = os.path.realpath(BIN)
    rec = {"SessionId": SESSION_ID, "Executable": TG.portable(resolved), "ExecutableSha256": sha_file(resolved), "Args": args[1:], "Cwd": CLONE,
           "PromptSha256": hashlib.sha256(prompt).hexdigest(), "PromptBytes": len(prompt), "TimeoutSeconds": TIMEOUT_S,
           "TransportGate": {k: checks["10-TransportGate"][k] for k in ("Status", "CharacterizationSha256", "AcceptanceSha256")},
           "EnvironmentVariableNames": sorted(k for k in os.environ if k.upper().startswith(("CLAUDE", "ANTHROPIC")))}
    if rec["ExecutableSha256"] != BIN_SHA or not same_path(rec["Executable"], BIN_DISPLAY):
        rec["Aborted"] = "el binario cambió entre la preflight y el lanzamiento: no se lanza (borrar launch/ y volver a custodiar)"
        write_json(os.path.join(out_dir, "run.json"), rec)
        print(json.dumps({"Refused": rec["Aborted"]}, ensure_ascii=False))
        return 2
    rec["Start"] = now()
    with open(os.path.join(out_dir, "stdout.jsonl"), "wb") as fo, open(os.path.join(out_dir, "stderr.txt"), "wb") as fe:
        p = subprocess.Popen([resolved] + args[1:], cwd=CLONE, stdin=subprocess.PIPE, stdout=fo, stderr=fe)
        rec["Pid"] = p.pid

        def feed():
            try:
                p.stdin.write(prompt)
                p.stdin.close()
            except OSError as e:
                rec["StdinError"] = str(e)
        th = threading.Thread(target=feed, daemon=True)
        th.start()
        killed = timed_out = False
        try:
            p.wait(timeout=TIMEOUT_S)
        except subprocess.TimeoutExpired:
            timed_out = killed = True
            rec["TerminateAt"] = now()
            p.terminate()
            try:
                p.wait(timeout=KILL_GRACE_S)
            except subprocess.TimeoutExpired:
                rec["KillAt"] = now()
                p.kill()
                p.wait()
        th.join(timeout=5)
    rec.update({"End": now(), "ExitCode": p.returncode, "Killed": killed, "TimedOut": timed_out})
    q = subprocess.run(["tasklist", "/FI", "PID eq %d" % p.pid, "/NH"], capture_output=True, text=True)
    rec["ProcessGoneAfter"] = str(p.pid) not in q.stdout
    so = open(os.path.join(out_dir, "stdout.jsonl"), "rb").read()
    rec.update({"StdoutSha256": hashlib.sha256(so).hexdigest(), "StdoutLines": len(so.splitlines()),
                "StderrBytes": os.path.getsize(os.path.join(out_dir, "stderr.txt"))})

    def kind(line):
        try:
            return json.loads(line).get("type")
        except ValueError:
            return None
    rec["ResultMessage"] = any(kind(l) == "result" for l in so.splitlines() if l.strip())
    rec["Transcript"] = {"Path": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a2r\\" + SESSION_ID + ".jsonl", "Exists": os.path.isfile(TRANSCRIPT),
                         "Sha256": sha_file(TRANSCRIPT) if os.path.isfile(TRANSCRIPT) else None}
    rec["CloneAfter"] = {"Head": git("rev-parse", "HEAD").strip(), "StatusPorcelainIgnored": git("status", "--porcelain", "--ignored")}
    write_json(os.path.join(out_dir, "run.json"), rec)
    print(json.dumps({k: rec[k] for k in ("SessionId", "Pid", "Start", "End", "ExitCode", "Killed", "TimedOut", "ResultMessage")}, ensure_ascii=False))
    return 0 if rec["ExitCode"] == 0 and rec["ResultMessage"] else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
