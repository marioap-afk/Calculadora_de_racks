"""I-62, corrección del lanzamiento para el SIGUIENTE kit de revisión (tras la ronda 2 de A-6, RunId R20261012T195511Z-e5c3, que acabó sin
run.json). Deriva launch.r3-template.py del launch.py CONGELADO de la ronda 2 por sustituciones EXACTAS de texto: cada una debe aparecer
exactamente una vez en el texto en curso (si no, se detiene sin escribir nada), de modo que el diff sea mínimo y revisable.

  G  guarda de la orden congelada (FROZEN_COMMAND), evaluada antes de cargar transport_gate.py / owner_gate.py y de cualquier preflight;
  R  registro posterior a prueba de fallos: tasklist en bytes, PID buscado en bytes, cada paso posterior aparte (PostRunErrors) y run.json
     escrito siempre una vez lanzado el hijo (try/finally);
  P  --record-probe: el mismo código que lanza y registra, con un hijo sustituto inocuo, en un temporal fuera del run;
  D  --dry-run (y el lanzamiento real, antes de la preflight) ejecutan también la sonda P e informan de G y de P.

Lee el launch.py de la ronda 2 SOLO para leer (comprueba su SHA-256 = el del manifiesto custodiado del kit) y escribe, junto a este script (o en
<dir de salida>): launch.r3-template.py y launch.r3-template.diff (unified diff, 3 líneas de contexto).
Uso: python -B adapt_launch_r3.py [<launch.py de la ronda 2>] [<dir de salida>]
"""
import difflib
import hashlib
import json
import os
import sys

ROUND2_LAUNCH = os.path.expandvars(r"%TEMP%\claude\D--Documentos-Codex-Calculadora-de-racks\e5124bf0-ed19-48a1-97ba-574baf9d33a2\scratchpad"
                                   r"\i62\day3\kit-a6r2-review\kit\launch.py")
ROUND2_SHA = "f2f58cec38b9f34e04d04e94f866d7d75a6bbd4864ccfbac734f8a52965f78ac"   # kit-manifest.json de la ronda 2 (Files["launch.py"])
OUT_NAME, DIFF_NAME = "launch.r3-template.py", "launch.r3-template.diff"

SUBS = []


def sub(label, old, new):
    SUBS.append((label, old, new))


# ---------------------------------------------------------------------------------------------------------------- docstring: uso y correcciones
sub("D0-docstring-uso", r'''Uso: python -B launch.py             → preflight y, si todo pasa, el único lanzamiento
     python -B launch.py --dry-run   → solo la preflight, sin ejecutar el binario (ni --version ni auth status: 4 y 5 quedan sin ejecutar), sin
                                       escribir y sin lanzar (código 2 siempre); la 11 sí se evalúa (solo lectura de git)
''', r'''Plantilla r3 (corrección del lanzamiento tras la ronda 2, RunId R20261012T195511Z-e5c3: lanzada con `-X utf8` añadido, el lector de
`tasklist` con text=True falló con UnicodeDecodeError, q.stdout quedó en None, el TypeError siguiente abortó launch.py y run.json no se escribió):
  G. guarda de la orden congelada, evaluada ANTES de todo (antes de cargar transport_gate.py y owner_gate.py y de cualquier preflight): se niega
     (código 2, sin escribir nada) si la orden no es exactamente FROZEN_COMMAND más, como mucho, un modo (--dry-run o --record-probe):
     sys.orig_argv[1:] = ["-B", <script>, *modo], con <script> normalizado (realpath + normcase: cualquier grafía que resuelva a ESTE archivo);
     sys.flags.dont_write_bytecode = 1, sys.flags.utf8_mode = 0, sys.flags.isolated = 0 y sys._xoptions vacío. Lo observado (argv, flags,
     opciones -X, intérprete y codificaciones) va a preflight.json (FrozenCommandGuard) y a la salida de --dry-run y de --record-probe.
  R. registro posterior a prueba de fallos: tasklist se captura en bytes (sin decodificar) y el PID se busca en bytes; cada paso posterior al fin
     del proceso va aparte y su excepción queda en run.json (PostRunErrors: paso, clase y mensaje; también las de hilos, como el lector de
     subprocess); run.json se escribe SIEMPRE una vez lanzado el hijo (try/finally), con los campos que se hayan podido reunir.
  P. --record-probe: el MISMO código que lanza y registra (run_and_record → spawn_and_wait → post_run → write_record) con un hijo sustituto
     inocuo (este intérprete con -c: lee stdin, imprime una línea JSON type=result y sale con 0) en un directorio temporal FUERA del run, del
     clon y del kit; escribe record-probe.json (nunca run.json ni <run>/launch), lo valida (campos de run.json presentes y serializables, tasklist
     consultado y tratado, ningún PostRunErrors) y borra el temporal. No ejecuta claude.exe ni lee la transcripción del session id real.
     Código 0 = el registro funciona; 1 = no (con el motivo en Problems).
  D. la sonda P también corre en --dry-run (informada junto a la guarda G) y en el lanzamiento real ANTES de la preflight: si falla, el
     lanzamiento se niega con código 2 sin escribir nada en el run.
Uso (la orden congelada, sin flags añadidos; FROZEN_COMMAND, desde la raíz del kit):
     python -B kit/launch.py                → G, P, preflight y, si todo pasa, el único lanzamiento
     python -B kit/launch.py --dry-run      → G, P (en un temporal que se borra) y solo la preflight, sin ejecutar el binario (ni --version ni
                                              auth status: 4 y 5 quedan sin ejecutar), sin escribir en el kit, el run ni el clon y sin lanzar
                                              (código 2 siempre); la 11 sí se evalúa (solo lectura de git)
     python -B kit/launch.py --record-probe → G y solo la sonda P (código 0 si el registro funciona en este entorno)
''')

# ---------------------------------------------------------------------------------------------------------------- G: imports y guarda
sub("G1-imports-y-guarda", r'''import json
import os
import stat
import subprocess
import sys
import threading
import time

KIT = os.path.dirname(os.path.abspath(__file__))
''', r'''import json
import locale
import os
import re
import shutil
import stat
import subprocess
import sys
import tempfile
import threading
import time

KIT = os.path.dirname(os.path.abspath(__file__))
FROZEN_COMMAND = "python -B kit/launch.py"   # G: la ÚNICA orden admitida (README y guarda); detrás, como mucho, un modo de FROZEN_MODES
FROZEN_MODES = ([], ["--dry-run"], ["--record-probe"])
FROZEN_GUARD_ENFORCED = True                 # el kit lo entrega en True; solo una copia de prueba (test_launchfix.py) lo pone a False


def _userless(p):
    """Ruta con %TEMP%, %LOCALAPPDATA%, %APPDATA% o %USERPROFILE% en vez del perfil (como transport_gate.portable, que aún no está cargado)."""
    s = str(p)
    for var in ("TEMP", "LOCALAPPDATA", "APPDATA", "USERPROFILE"):
        v = os.environ.get(var)
        for base in ((v, os.path.realpath(v)) if v else ()):
            b = base.rstrip("\\")
            if s.lower() == b.lower() or s.lower().startswith(b.lower() + "\\"):
                return "%" + var + "%" + s[len(b):]
    return s


def _encodings():
    """Codificaciones que decide text=True y páginas de códigos de la consola (solo lectura; tasklist escribe en la de la consola)."""
    e = {"LocalePreferred": locale.getpreferredencoding(False), "FileSystem": sys.getfilesystemencoding(),
         "Stdout": getattr(sys.stdout, "encoding", None)}
    try:
        import ctypes
        k = ctypes.windll.kernel32
        e.update({"ConsoleOutputCP": k.GetConsoleOutputCP(), "ConsoleCP": k.GetConsoleCP(), "OEMCP": k.GetOEMCP(), "ACP": k.GetACP()})
    except Exception:   # fuera de Windows: sin páginas de códigos
        pass
    return e


def frozen_command_guard(orig_argv, flags, xoptions, script):
    """G (pura): la orden es FROZEN_COMMAND (+ un modo de FROZEN_MODES) y los flags del intérprete son los de esa orden. → {Ok, Problems, …}.
    Normalización de <script>: realpath + normcase de la ruta dada (relativa al cwd de arranque) = la de este archivo."""
    probs = []
    words = FROZEN_COMMAND.split()
    opts, documented = words[1:-1], words[-1]
    tail = [str(a) for a in list(orig_argv or [])[1:]]
    script_arg = tail[len(opts)] if len(tail) > len(opts) else None
    mode = tail[len(opts) + 1:]
    resolves = script_arg is not None and os.path.normcase(os.path.realpath(script_arg)) == os.path.normcase(os.path.realpath(script))
    if tail[:len(opts)] != opts:
        probs.append("opciones del intérprete %s distintas de las de la orden congelada %s («%s»)" % (tail[:len(opts)], opts, FROZEN_COMMAND))
    if not resolves:
        probs.append("el argumento del script (%r) no resuelve a este archivo" % (_userless(script_arg) if script_arg else script_arg))
    if mode not in [list(m) for m in FROZEN_MODES]:
        probs.append("argumentos %s no admitidos: solo ninguno, --dry-run o --record-probe" % [_userless(a) for a in mode])
    if flags.get("dont_write_bytecode") != 1:
        probs.append("sys.flags.dont_write_bytecode = %r (se exige 1: -B)" % flags.get("dont_write_bytecode"))
    if flags.get("utf8_mode") != 0:
        probs.append("sys.flags.utf8_mode = %r (se exige 0: sin -X utf8 ni PYTHONUTF8)" % flags.get("utf8_mode"))
    if flags.get("isolated") != 0:
        probs.append("sys.flags.isolated = %r (se exige 0: sin -I)" % flags.get("isolated"))
    if xoptions:
        probs.append("opciones -X presentes %s (no se admite ninguna)" % sorted(xoptions))
    return {"Ok": not probs, "Problems": probs, "FrozenCommand": FROZEN_COMMAND, "Enforced": FROZEN_GUARD_ENFORCED,
            "OrigArgvTail": [_userless(a) for a in tail], "ScriptArgResolvesToThisFile": resolves,
            "ScriptArgIsDocumentedSpelling": script_arg is not None and script_arg.replace("\\", "/") == documented, "Mode": mode,
            "Flags": flags, "XOptions": {k: str(v) for k, v in dict(xoptions).items()}, "Interpreter": _userless(sys.executable),
            "PythonVersion": sys.version.split()[0], "Encodings": _encodings()}


FROZEN = frozen_command_guard(getattr(sys, "orig_argv", None), {n: getattr(sys.flags, n) for n in type(sys.flags).__match_args__},
                              dict(getattr(sys, "_xoptions", {})), os.path.abspath(__file__))
if __name__ == "__main__" and FROZEN_GUARD_ENFORCED and not FROZEN["Ok"]:   # G: antes de cargar nada del kit y de cualquier preflight
    sys.stdout.reconfigure(errors="backslashreplace")
    print(json.dumps({"Refused": "orden distinta de la congelada «%s»: no se ejecuta nada y no se escribe nada" % FROZEN_COMMAND,
                      "FrozenCommandGuard": FROZEN}, ensure_ascii=False, indent=1))
    sys.exit(2)
''')

# ---------------------------------------------------------------------------------------------------------------- R: git con repo y decodificación fija
sub("R1-git-repo-y-decodificacion", r'''def git(*a):
    return subprocess.run(["git", "-C", CLONE] + list(a), capture_output=True, text=True).stdout
''', r'''def git(*a, repo=CLONE):
    return subprocess.run(["git", "-C", repo] + list(a), capture_output=True, text=True, encoding="utf-8", errors="replace").stdout
''')

# ---------------------------------------------------------------------------------------------------------------- R/P: registro y campos comunes
sub("R2-write_record-y-base_record", r'''def write_json(path, obj):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(obj, ensure_ascii=False, indent=1) + "\n")
''', r'''def write_json(path, obj):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(obj, ensure_ascii=False, indent=1) + "\n")


def write_record(path, rec):
    """R: write_json estricto; si algún valor no fuera JSON, lo anota en PostRunErrors y escribe con repr() (el registro no se pierde)."""
    try:
        text = json.dumps(rec, ensure_ascii=False, indent=1, allow_nan=False)
    except (TypeError, ValueError) as e:
        rec.setdefault("PostRunErrors", []).append({"Step": "WriteRecord", "Class": type(e).__name__, "Message": str(e)[:500]})
        text = json.dumps(rec, ensure_ascii=False, indent=1, default=repr)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text + "\n")


def base_record(session_id, executable, executable_sha, args, cwd, prompt, timeout_s, gate, settle):
    """Campos de run.json anteriores al lanzamiento (los de la ronda 2, sin cambios); comunes al lanzamiento real y a la sonda P."""
    return {"SessionId": session_id, "Executable": executable, "ExecutableSha256": executable_sha, "Args": args, "Cwd": cwd,
            "PromptSha256": hashlib.sha256(prompt).hexdigest(), "PromptBytes": len(prompt), "TimeoutSeconds": timeout_s,
            "TransportGate": gate,
            "EnvironmentVariableNames": sorted(k for k in os.environ if k.upper().startswith(("CLAUDE", "ANTHROPIC"))), "AuthSettle": settle}
''')

# ---------------------------------------------------------------------------------------------------------------- P/D: despacho de modos
sub("P1-main-record-probe", r'''def main(argv):
    dry_run = "--dry-run" in argv
''', r'''def main(argv):
    if argv == ["--record-probe"]:   # P: solo la sonda de registro (ni preflight, ni binario, ni <run>/launch)
        probe = record_probe()
        print(json.dumps({"RecordProbe": probe, "FrozenCommandGuard": FROZEN}, ensure_ascii=False, indent=1))
        return 0 if probe["Ok"] else 1
    dry_run = "--dry-run" in argv
''')

sub("D1-sonda-antes-de-la-preflight", r'''    checks, ok, args, (char_b, acc_b) = preflight(dry_run)
    if dry_run:
        print(json.dumps({"DryRun": "solo preflight: no se ejecuta el binario, no se escribe ni se lanza",
''', r'''    probe = record_probe()   # D: paso previo obligatorio (temporal fuera del run, del clon y del kit; se borra)
    if not dry_run and not probe["Ok"]:
        print(json.dumps({"Refused": "la sonda de registro falló: no se lanza (nada escrito en el run)", "RecordProbe": probe,
                          "FrozenCommandGuard": FROZEN}, ensure_ascii=False, indent=1))
        return 2
    checks, ok, args, (char_b, acc_b) = preflight(dry_run)
    if dry_run:
        print(json.dumps({"DryRun": "guarda de la orden congelada, sonda de registro (en un temporal que se borra) y preflight: no se ejecuta "
                                    "el binario, no se escribe en el kit, el run ni el clon y no se lanza",
                          "FrozenCommandGuard": FROZEN, "RecordProbe": probe, "RecordProbeOk": probe["Ok"],
''')

sub("G2-preflight-json", r'''    write_json(os.path.join(out_dir, "preflight.json"), {"SessionId": SESSION_ID, "At": now(), "Checks": checks})
''', r'''    write_json(os.path.join(out_dir, "preflight.json"), {"SessionId": SESSION_ID, "At": now(), "Checks": checks, "FrozenCommandGuard": FROZEN,
                                                         "RecordProbe": probe})
''')

sub("R3-base_record-en-main", r'''    rec = {"SessionId": SESSION_ID, "Executable": TG.portable(resolved), "ExecutableSha256": sha_file(resolved), "Args": args[1:], "Cwd": CLONE,
           "PromptSha256": hashlib.sha256(prompt).hexdigest(), "PromptBytes": len(prompt), "TimeoutSeconds": TIMEOUT_S,
           "TransportGate": {k: checks["10-TransportGate"][k] for k in ("Status", "CharacterizationSha256", "AcceptanceSha256")},
           "EnvironmentVariableNames": sorted(k for k in os.environ if k.upper().startswith(("CLAUDE", "ANTHROPIC"))), "AuthSettle": settle}
''', r'''    rec = base_record(SESSION_ID, TG.portable(resolved), sha_file(resolved), args[1:], CLONE, prompt, TIMEOUT_S,
                      {k: checks["10-TransportGate"][k] for k in ("Status", "CharacterizationSha256", "AcceptanceSha256")}, settle)
''')

# ---------------------------------------------------------------------------------------------------------------- R: lanzamiento y registro comunes
sub("R4-run_and_record-y-spawn_and_wait", r'''    rec["Start"] = now()
    with open(os.path.join(out_dir, "stdout.jsonl"), "wb") as fo, open(os.path.join(out_dir, "stderr.txt"), "wb") as fe:
        p = subprocess.Popen([resolved] + args[1:], cwd=CLONE, stdin=subprocess.PIPE, stdout=fo, stderr=fe)
        rec["Pid"] = p.pid
''', r'''    rc = run_and_record(rec, [resolved] + args[1:], CLONE, prompt, out_dir, "run.json", TIMEOUT_S, TRANSCRIPT,
                        "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a6r2\\" + SESSION_ID + ".jsonl", CLONE)
    print(json.dumps({k: rec.get(k) for k in ("SessionId", "Pid", "Start", "End", "ExitCode", "Killed", "TimedOut", "ResultMessage",
                                              "TransportFailure", "PostRunErrors", "LaunchError")}, ensure_ascii=False))
    return rc


def run_and_record(rec, cmd, cwd, stdin_bytes, out_dir, record_name, timeout_s, transcript, transcript_display, repo):
    """R: lanza `cmd` (spawn_and_wait), ejecuta los pasos posteriores (post_run) y escribe <out_dir>/<record_name> SIEMPRE (finally), con lo
    reunido. Común al lanzamiento real (run.json) y a la sonda P (record-probe.json). Las excepciones de hilos (p. ej. el lector de subprocess,
    el de stdin) también quedan en PostRunErrors. → 0 si el hijo salió con 0, hay mensaje result y no hubo errores; si no, 1."""
    rec["Start"] = now()
    rec["PostRunErrors"] = []
    prev_hook = threading.excepthook

    def thread_hook(a):
        rec["PostRunErrors"].append({"Step": "hilo %s" % (a.thread.name if a.thread is not None else "?"), "Class": a.exc_type.__name__,
                                     "Message": str(a.exc_value)[:500]})
        prev_hook(a)
    threading.excepthook = thread_hook
    procs = []
    try:
        try:
            spawn_and_wait(rec, procs, cmd, cwd, stdin_bytes, out_dir, timeout_s)
        except Exception as e:   # el hijo no arrancó o la espera falló: queda en el registro (en la ronda 2: traza y sin run.json)
            p = procs[0] if procs else None
            rec["LaunchError"] = {"Class": type(e).__name__, "Message": str(e)[:500], "ChildRunning": p is not None and p.poll() is None}
        if procs:
            post_run(rec, procs[0], out_dir, transcript, transcript_display, repo)
    finally:
        threading.excepthook = prev_hook
        write_record(os.path.join(out_dir, record_name), rec)
    ok = rec.get("ExitCode") == 0 and rec.get("ResultMessage") is True and not rec["PostRunErrors"] and "LaunchError" not in rec
    return 0 if ok else 1


def spawn_and_wait(rec, procs, cmd, cwd, stdin_bytes, out_dir, timeout_s):
    """Lanzamiento, stdin y tope, sin cambios frente a la ronda 2 salvo los parámetros y `procs`, que recibe el proceso nada más crearlo."""
    with open(os.path.join(out_dir, "stdout.jsonl"), "wb") as fo, open(os.path.join(out_dir, "stderr.txt"), "wb") as fe:
        p = subprocess.Popen(cmd, cwd=cwd, stdin=subprocess.PIPE, stdout=fo, stderr=fe)
        procs.append(p)
        rec["Pid"] = p.pid
''')

sub("R5-stdin", r'''                p.stdin.write(prompt)
''', r'''                p.stdin.write(stdin_bytes)
''')

sub("R6-tope", r'''            p.wait(timeout=TIMEOUT_S)
''', r'''            p.wait(timeout=timeout_s)
''')

# ---------------------------------------------------------------------------------------------------------------- R: pasos posteriores + P: sonda
sub("R7-post_run-y-P2-record_probe", r'''    rec.update({"End": now(), "ExitCode": p.returncode, "Killed": killed, "TimedOut": timed_out})
    q = subprocess.run(["tasklist", "/FI", "PID eq %d" % p.pid, "/NH"], capture_output=True, text=True)
    rec["ProcessGoneAfter"] = str(p.pid) not in q.stdout
    so = open(os.path.join(out_dir, "stdout.jsonl"), "rb").read()
    rec.update({"StdoutSha256": hashlib.sha256(so).hexdigest(), "StdoutLines": len(so.splitlines()),
                "StderrBytes": os.path.getsize(os.path.join(out_dir, "stderr.txt"))})

    def kind(line):
        try:
            o = json.loads(line)
            return o.get("type") if isinstance(o, dict) else None
        except ValueError:
            return None
    rec["ResultMessage"] = any(kind(l) == "result" for l in so.splitlines() if l.strip())
    rec["TransportFailure"] = transport_failure(so)
    rec["Transcript"] = {"Path": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a6r2\\" + SESSION_ID + ".jsonl", "Exists": os.path.isfile(TRANSCRIPT),
                         "Sha256": sha_file(TRANSCRIPT) if os.path.isfile(TRANSCRIPT) else None}
    rec["CloneAfter"] = {"Head": git("rev-parse", "HEAD").strip(), "StatusPorcelainIgnored": git("status", "--porcelain", "--ignored")}
    write_json(os.path.join(out_dir, "run.json"), rec)
    print(json.dumps({k: rec[k] for k in ("SessionId", "Pid", "Start", "End", "ExitCode", "Killed", "TimedOut", "ResultMessage", "TransportFailure")},
                     ensure_ascii=False))
    return 0 if rec["ExitCode"] == 0 and rec["ResultMessage"] else 1
''', r'''    rec.update({"End": now(), "ExitCode": p.returncode, "Killed": killed, "TimedOut": timed_out})
    return p


def post_run(rec, p, out_dir, transcript, transcript_display, repo):
    """R: pasos posteriores al fin del proceso. Cada uno va aparte: su excepción queda en rec["PostRunErrors"] (paso, clase, mensaje) y los
    demás siguen. tasklist se captura en BYTES y el PID se busca en bytes: en la ronda 2, text=True con -X utf8 decodificó como UTF-8 la salida
    de tasklist en la página de la consola (cp850: «INFORMACIÓN:» con 0xE0), el hilo lector lanzó UnicodeDecodeError, q.stdout quedó en None y
    el TypeError de la línea siguiente abortó launch.py antes de escribir run.json."""
    so = []

    def tasklist():
        q = subprocess.run(["tasklist", "/FI", "PID eq %d" % p.pid, "/NH"], capture_output=True)   # bytes: sin decodificar
        rec["TasklistQuery"] = {"ReturnCode": q.returncode, "StdoutBytes": len(q.stdout), "NonAsciiBytes": any(b > 0x7F for b in q.stdout)}
        rec["ProcessGoneAfter"] = re.search(rb"(?<![0-9])%d(?![0-9])" % p.pid, q.stdout) is None

    def stdout():
        so.append(open(os.path.join(out_dir, "stdout.jsonl"), "rb").read())
        rec.update({"StdoutSha256": hashlib.sha256(so[0]).hexdigest(), "StdoutLines": len(so[0].splitlines()),
                    "StderrBytes": os.path.getsize(os.path.join(out_dir, "stderr.txt"))})

    def read_stdout():
        if not so:
            raise RuntimeError("stdout.jsonl no leído (falló el paso Stdout)")
        return so[0]

    def kind(line):
        try:
            o = json.loads(line)
            return o.get("type") if isinstance(o, dict) else None
        except ValueError:
            return None

    def result_message():
        rec["ResultMessage"] = any(kind(l) == "result" for l in read_stdout().splitlines() if l.strip())

    def transport():
        rec["TransportFailure"] = transport_failure(read_stdout())

    def transcript_after():
        rec["Transcript"] = {"Path": transcript_display, "Exists": os.path.isfile(transcript),
                             "Sha256": sha_file(transcript) if os.path.isfile(transcript) else None}

    def clone_after():
        rec["CloneAfter"] = {"Head": git("rev-parse", "HEAD", repo=repo).strip(),
                             "StatusPorcelainIgnored": git("status", "--porcelain", "--ignored", repo=repo)}

    for name, step in (("ProcessGoneAfter", tasklist), ("Stdout", stdout), ("ResultMessage", result_message), ("TransportFailure", transport),
                       ("Transcript", transcript_after), ("CloneAfter", clone_after)):
        try:
            step()
        except Exception as e:   # queda registrada; los demás pasos siguen y el registro se escribe igual (finally de run_and_record)
            rec["PostRunErrors"].append({"Step": name, "Class": type(e).__name__, "Message": str(e)[:500]})


# ---------------------------------------------------------------------------------------------------------------- P: sonda de registro
PROBE_RECORD = "record-probe.json"   # nunca run.json; solo dentro del temporal de la sonda
PROBE_TIMEOUT_S = 120
PROBE_STDIN = b"record-probe\n"
PROBE_CHILD = ("import json, sys; d = sys.stdin.buffer.read(); print(json.dumps({'type': 'result', 'subtype': 'success', 'is_error': False, "
               "'result': 'record-probe', 'num_turns': 0, 'stdin_bytes': len(d), 'usage': {'input_tokens': 0, 'output_tokens': 0}}))")
RUN_JSON_FIELDS = ("SessionId", "Executable", "ExecutableSha256", "Args", "Cwd", "PromptSha256", "PromptBytes", "TimeoutSeconds", "TransportGate",
                   "EnvironmentVariableNames", "AuthSettle", "Start", "PostRunErrors", "Pid", "End", "ExitCode", "Killed", "TimedOut",
                   "TasklistQuery", "ProcessGoneAfter", "StdoutSha256", "StdoutLines", "StderrBytes", "ResultMessage", "TransportFailure",
                   "Transcript", "CloneAfter")   # los que run.json lleva en un lanzamiento completo (opcionales: StdinError, TerminateAt, KillAt)


def probe_interpreter():
    """Hijo sustituto: este intérprete. → (ruta para lanzar, SHA-256, archivo medido). El alias de la Microsoft Store (sys.executable) no se
    puede abrir para leer (OSError 22): entonces se mide el binario homónimo de sys.base_prefix."""
    exe = sys.executable
    for cand in (exe, os.path.join(sys.base_prefix, os.path.basename(exe))):
        try:
            return exe, sha_file(cand), TG.portable(cand)
        except OSError:
            continue
    return exe, None, None


def inside(path, root):
    a, b = os.path.normcase(os.path.realpath(path)), os.path.normcase(os.path.realpath(root))
    return a == b or a.startswith(b.rstrip("\\") + "\\")


def record_probe():
    """P: ejerce run_and_record (el código que escribe run.json) con el hijo sustituto, en un temporal fuera del run, del clon y del kit, y
    valida record-probe.json. No ejecuta claude.exe, no lee ~/.claude/projects y no crea <run>/launch. git del paso CloneAfter va contra una
    ruta INEXISTENTE del temporal: ejerce el subprocess y la decodificación sin descubrir ningún repositorio superior. → {Ok, Problems, …}."""
    probs = []
    launch_dir = os.path.join(RUN, "launch")
    launch_before, kit_before = os.path.exists(launch_dir), sorted(os.listdir(KIT))
    tmp = tempfile.mkdtemp(prefix="r62-record-probe-")
    out = {"Ok": False, "Problems": probs, "TempDir": TG.portable(tmp), "RecordFile": PROBE_RECORD,
           "StandInChild": "sys.executable -B -I -c PROBE_CHILD (lee stdin, una línea JSON type=result, código 0)"}
    try:
        for name, root in (("run", RUN), ("clon", CLONE), ("kit", KIT)):
            if inside(tmp, root):
                probs.append("el temporal de la sonda está dentro del %s" % name)
        if probs:
            return out
        out_dir, cwd = os.path.join(tmp, "launch-standin"), os.path.join(tmp, "cwd-standin")
        os.makedirs(out_dir)
        os.makedirs(cwd)
        transcript = os.path.join(tmp, "transcript-standin.jsonl")
        with open(transcript, "wb") as f:
            f.write(b'{"type":"record-probe"}\n')
        exe, exe_sha, exe_measured = probe_interpreter()
        cmd = [exe, "-B", "-I", "-c", PROBE_CHILD]
        rec = base_record("record-probe", TG.portable(exe), exe_sha, cmd[1:], TG.portable(cwd), PROBE_STDIN, PROBE_TIMEOUT_S,
                          {"Status": None, "CharacterizationSha256": None, "AcceptanceSha256": None}, {"Seconds": 0, "StandIn": True})
        rc = run_and_record(rec, cmd, cwd, PROBE_STDIN, out_dir, PROBE_RECORD, PROBE_TIMEOUT_S, transcript,
                            "<temporal de la sonda>\\transcript-standin.jsonl", os.path.join(tmp, "no-repo"))
        path = os.path.join(out_dir, PROBE_RECORD)
        back = json.load(open(path, encoding="utf-8")) if os.path.isfile(path) else None
        out.update({"RunAndRecordExit": rc, "RecordWritten": isinstance(back, dict), "RecordUnderTempDir": inside(path, tmp),
                    "Interpreter": TG.portable(exe), "InterpreterSha256": exe_sha, "InterpreterMeasured": exe_measured})
        if not isinstance(back, dict):
            probs.append("run_and_record no escribió %s" % PROBE_RECORD)
            return out
        missing = [k for k in RUN_JSON_FIELDS if k not in back]
        not_json = []
        for k in RUN_JSON_FIELDS:
            try:
                json.dumps(rec.get(k), allow_nan=False)
            except (TypeError, ValueError):
                not_json.append(k)
        child = {}
        try:
            child = json.loads(open(os.path.join(out_dir, "stdout.jsonl"), "rb").read())
        except ValueError:
            pass
        tq = back.get("TasklistQuery")
        out.update({"RecordKeys": sorted(back), "MissingFields": missing, "NotJsonSerialisable": not_json,
                    "PostRunErrors": back.get("PostRunErrors"), "LaunchError": back.get("LaunchError"), "TasklistQuery": tq,
                    "ProcessGoneAfter": back.get("ProcessGoneAfter"), "ExitCode": back.get("ExitCode"), "ResultMessage": back.get("ResultMessage"),
                    "StdoutLines": back.get("StdoutLines"), "ChildStdinBytes": child.get("stdin_bytes") if isinstance(child, dict) else None})
        if missing:
            probs.append("faltan campos de run.json en el registro: %s" % missing)
        if not_json:
            probs.append("campos no serializables como JSON estricto: %s" % not_json)
        if back.get("PostRunErrors") != []:
            probs.append("errores del registro posterior: %s" % [(e.get("Step"), e.get("Class")) for e in back.get("PostRunErrors") or []
                                                                  if isinstance(e, dict)])
        if "LaunchError" in back or "StdinError" in back:
            probs.append("el hijo sustituto no se lanzó o no recibió stdin: %s" % (back.get("LaunchError") or back.get("StdinError")))
        if not (isinstance(tq, dict) and isinstance(tq.get("ReturnCode"), int) and isinstance(tq.get("StdoutBytes"), int) and tq["StdoutBytes"] > 0):
            probs.append("tasklist no consultado o su salida no se trató")
        if back.get("ProcessGoneAfter") is not True:
            probs.append("ProcessGoneAfter = %r: se esperaba true (el hijo sustituto ya terminó)" % back.get("ProcessGoneAfter"))
        if back.get("ExitCode") != 0 or back.get("Killed") is not False or back.get("TimedOut") is not False:
            probs.append("hijo sustituto sin salida 0 o terminado por el invocador")
        if back.get("ResultMessage") is not True or back.get("StdoutLines") != 1 or back.get("TransportFailure") is not None or \
                out["ChildStdinBytes"] != len(PROBE_STDIN):
            probs.append("stdout del hijo sustituto no registrado como una línea result con el stdin entregado")
        t = back.get("Transcript") or {}
        if t.get("Exists") is not True or t.get("Sha256") != sha_file(transcript):
            probs.append("transcripción sustituta no registrada")
        if sorted(back.get("CloneAfter") or {}) != ["Head", "StatusPorcelainIgnored"]:
            probs.append("CloneAfter sin sus dos campos")
        if rc != 0:
            probs.append("run_and_record devolvió %d" % rc)
    except Exception as e:
        probs.append("la sonda no terminó: %s: %s" % (type(e).__name__, str(e)[:300]))
    finally:
        shutil.rmtree(tmp, ignore_errors=True)
        out["TempDirRemoved"] = not os.path.exists(tmp)
        out["RunLaunchExistedBefore"], out["RunLaunchExistsAfter"] = launch_before, os.path.exists(launch_dir)
        if out["RunLaunchExistsAfter"] and not launch_before:
            probs.append("<run>/launch apareció durante la sonda")
        out["KitListingUnchanged"] = sorted(os.listdir(KIT)) == kit_before
        if not out["KitListingUnchanged"]:
            probs.append("el listado del kit cambió durante la sonda")
        out["Ok"] = not probs
    return out
''')


def main(argv):
    src = argv[0] if len(argv) > 0 else ROUND2_LAUNCH
    out_dir = argv[1] if len(argv) > 1 else os.path.dirname(os.path.abspath(__file__))
    raw = open(src, "rb").read()
    got_sha = hashlib.sha256(raw).hexdigest()
    if got_sha != ROUND2_SHA:
        print(json.dumps({"Refused": "launch.py de la ronda 2 con otro SHA-256 (no es el custodiado)", "Got": got_sha, "Expected": ROUND2_SHA}))
        return 2
    text = raw.decode("utf-8")
    assert "\r" not in text, "se esperaban finales de línea LF"
    report = []
    for label, old, new in SUBS:
        n = text.count(old)
        if n != 1:
            print(json.dumps({"Refused": "sustitución %s: el texto original aparece %d veces (se exige exactamente 1); nada escrito" % (label, n)},
                             ensure_ascii=False))
            return 2
        text = text.replace(old, new, 1)
        report.append({"Substitution": label, "Matches": n, "OldLines": old.count("\n"), "NewLines": new.count("\n")})
    compile(text, OUT_NAME, "exec")   # sintaxis válida (en memoria; no escribe bytecode)
    new_raw = text.encode("utf-8")
    diff = "".join(difflib.unified_diff(raw.decode("utf-8").splitlines(True), text.splitlines(True),
                                        "a/kit-a6r2-review/kit/launch.py (ronda 2, SHA-256 %s)" % ROUND2_SHA[:12],
                                        "b/%s" % OUT_NAME, n=3))
    with open(os.path.join(out_dir, OUT_NAME), "wb") as f:
        f.write(new_raw)
    with open(os.path.join(out_dir, DIFF_NAME), "wb") as f:
        f.write(diff.encode("utf-8"))
    added = sum(1 for l in diff.splitlines() if l.startswith("+") and not l.startswith("+++"))
    removed = sum(1 for l in diff.splitlines() if l.startswith("-") and not l.startswith("---"))
    print(json.dumps({"Input": {"Sha256": got_sha, "Bytes": len(raw)}, "Substitutions": report,
                      "Output": {"File": OUT_NAME, "Sha256": hashlib.sha256(new_raw).hexdigest(), "Bytes": len(new_raw)},
                      "Diff": {"File": DIFF_NAME, "Sha256": hashlib.sha256(diff.encode("utf-8")).hexdigest(), "Added": added, "Removed": removed}},
                     ensure_ascii=False, indent=1))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
