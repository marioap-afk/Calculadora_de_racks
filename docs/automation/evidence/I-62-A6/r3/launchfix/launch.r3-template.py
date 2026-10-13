"""I-62, decisiones §80, punto 6: lanzador de la ÚNICA revisión formal independiente del Architect de la RONDA 2 de la enmienda A-6 (blob de la
ronda 2 fijado en kit-params.json y en el cierre), por una celda claude-cli medida y elegible (autorización del Owner CLAUDE-CLI-I62 = A). Commit
del clon: kit-params.json. Lo ejecuta la sesión principal, una sola vez y sin reintento, y SOLO después de una disposición posterior del
Coordinator que autorice esta revisión (decisiones §79, punto 7; §80, punto 5): la comprobación 11 lo exige. Quien prepara el kit no lo ejecuta
(solo su modo --dry-run, que no ejecuta el binario). Adaptado del launch.py del kit de la ronda 1 (prep/adapt_launch.py).

Antes de lanzar comprueba, y se NIEGA a lanzar (código 2, sin escribir nada) si falla cualquiera de estas condiciones:
  1. integridad del kit: cada archivo de kit-manifest.json con su SHA-256 (incluidos este script, transport_gate.py y transport-gate.json);
  2. transporte del cierre = constantes de este script (flags, --add-dir, session id, cwd, binario, tope) y ArgsTemplate del cierre = la plantilla;
     el cierre designa el blob de la ronda 2 de kit-params.json y lleva el RunId del kit;
  3. binario elegible: existe en %APPDATA%, ruta resuelta = la del cierre, SHA-256 = 8693c4a0…, firma Authenticode válida de «Anthropic, PBC»;
  4. versión: `claude.exe --version` = «2.1.293 (Claude Code)»;
  5. autenticación: `claude.exe auth status --json` con loggedIn = true (solo se conservan loggedIn, authMethod y apiProvider; ni correo ni
     organización; ninguna credencial se lee);
  6. clon: HEAD = el commit, separado (sin ninguna rama local), sin remoto, árbol limpio con ignorados, blobs designados, sin enlaces ni uniones;
  7. run: exactamente los tres archivos (order.txt, prompt.md y a6-r1.md; sin launch/), cada uno = copia del kit = RunFileHashes del cierre =
     kit-manifest.json, y a6-r1.md con el blob Git de la ronda 1 (eac473c0);
  8. sesión nueva: sin transcripción con el session id fijado en ningún proyecto y sin directorio de proyecto para el clon ni para el run (sin
     memoria previa; patrón D--r62-arch-a6r2*);
  9. longitud de la línea de órdenes de Windows (< 32 000 caracteres);
 10. compuerta de transporte (D-01): transport-gate.json en MEASURED (sonda C3 de la lista exacta con --add-dir, en el registro de caracterización
     fijado por su SHA-256) o ACCEPTED (aceptación explícita registrada), con sus predicados satisfechos (transport_gate.evaluate); la plantilla de
     los argumentos que se van a lanzar = la de la compuerta;
 11. compuerta del Owner y de la autorización (owner_gate.py; decisiones §78, §79 y §80), BLOQUEANTE: en la HEAD del worktree de I-62, la línea
     del Owner A6-ADR0046-9-FX02 (SHA-256 83152eae…) como línea completa después de §78 y en la A-6 de la ronda 2 del clon; CLAUDE-CLI-I62 = A
     vigente y el binario medido = el del cierre; A-6 en la HEAD con el blob de la ronda 2 fijado; una disposición del Coordinator numerada
     después de §80 que contenga el blob completo y el RunId del kit; decisiones crece solo por añadido desde el clon, sin cambios sin commit, y
     la HEAD desciende del clon (solo lectura de git en el worktree: rev-parse, show, hash-object sin -w y merge-base). Se evalúa antes de las
     comprobaciones 4 y 5: si no pasa, el binario no se ejecuta (ni --version ni auth status).
Después escribe <run>/launch/preflight.json y las copias de los registros de la compuerta (transport-characterization.json y, en ACCEPTED,
transport-acceptance.json); espera AUTH_SETTLE_S = 120 s contados desde el final de `auth status` (comprobación 5), para que una renovación del
token OAuth que esa llamada haya iniciado termine antes del lanzamiento (mitigación de TRANSPORT_AUTH_REFRESH_CONFLICT, que dejó el intento 1
de la revisión de A-3 en INVALID_LAUNCH); vuelve a medir el binario JUSTO antes de lanzarlo (si su SHA-256 cambió, aborta sin lanzar) y lanza la ruta resuelta
con la lista exacta de argumentos (flags medidos + --add-dir del run + --session-id + --json-schema compacto), el prompt (bytes de prompt.md del
run) por stdin y cwd = el clon. Aplica el tope de 3 600 s con terminación (terminate y, 60 s después, kill) y escribe en <run>/launch/:
stdout.jsonl, stderr.txt y run.json (ruta resuelta y SHA-256 medidos del ejecutable, PID, inicio y fin, código de salida, terminado o no,
SHA-256 de stdout y de la transcripción, clon después, espera tras `auth status`). Si el mensaje result contiene «Failed to refresh OAuth token»
con 0 tokens, run.json lo registra en TransportFailure como TRANSPORT_AUTH_REFRESH_CONFLICT. No hay reintento en ningún caso.
Plantilla r3 (corrección del lanzamiento tras la ronda 2, RunId R20261012T195511Z-e5c3: lanzada con `-X utf8` añadido, el lector de
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
"""
import datetime
import glob
import hashlib
import importlib.util
import json
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
_spec = importlib.util.spec_from_file_location("transport_gate", os.path.join(KIT, "transport_gate.py"))
TG = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(TG)
_spec = importlib.util.spec_from_file_location("owner_gate", os.path.join(KIT, "owner_gate.py"))
OG = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(OG)
CLONE, RUN = r"D:\r62-arch-a6r2", r"D:\r62-arch-a6r2-run"
PARAMS = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
assert PARAMS.get("Provisional") is False and PARAMS.get("Clone") == CLONE and PARAMS.get("Run") == RUN, "kit-params.json provisional o de otro clon"
REV = PARAMS["CloneCommit"]   # commit del clon (A-6 ronda 2 publicada y el resultado `run` de sus guardas custodiado)
WORKTREE = os.path.join(os.environ.get("USERPROFILE", ""), ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")   # solo lectura (11)
SESSION_ID = "d95fcaba-f7db-4408-af85-497e565d114e"   # uuid4 del único lanzamiento (generado una vez en la fase 1)
BIN_DISPLAY = r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe"
BIN = os.path.join(os.environ.get("APPDATA", ""), "Claude", "claude-code", "2.1.293", "83cb0bd7fed4", "claude.exe")
BIN_SHA = "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa"
VERSION = "2.1.293 (Claude Code)"
FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
         "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
ADD_DIR = ["--add-dir", RUN]   # D-01: solo se lanza si transport-gate.json ata la medición C3 o la aceptación registrada
RUN_FILES = ["order.txt", "prompt.md", "a6-r1.md"]
A6R1_BLOB = "eac473c0d2e58cf1c4594f48a8816b3833af6509"   # a6-r1.md: el texto de A-6 de la ronda 1 (decisiones §77, punto 1)
TIMEOUT_S, KILL_GRACE_S = 3600, 60
AUTH_SETTLE_S = 120                                    # espera entre `auth status` y el lanzamiento (heredada del intento 2 de A-3)
AUTH_REFRESH_MARK = "Failed to refresh OAuth token"    # texto del result del intento 1 (TRANSPORT_AUTH_REFRESH_CONFLICT)
AUTH_DONE = []                                         # instante monotónico del final de `auth status` (preflight 5)
PROJECTS = os.path.join(os.path.expanduser("~"), ".claude", "projects")
PROJECT_DIR = os.path.join(PROJECTS, "D--r62-arch-a6r2")
TRANSCRIPT = os.path.join(PROJECT_DIR, SESSION_ID + ".jsonl")
now = lambda: datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%fZ")


def sha_file(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest()


def git_blob(b):
    return hashlib.sha1(b"blob %d\x00" % len(b) + b).hexdigest()


def git(*a, repo=CLONE):
    return subprocess.run(["git", "-C", repo] + list(a), capture_output=True, text=True, encoding="utf-8", errors="replace").stdout


def build_args(schema_path):
    compact = json.dumps(json.load(open(schema_path, encoding="utf-8")), separators=(",", ":"), ensure_ascii=True)
    return [BIN] + FLAGS + ADD_DIR + ["--session-id", SESSION_ID, "--json-schema", compact]


def same_path(a, b):
    return TG.norm(TG.expand(a)) == TG.norm(TG.expand(b))


def transport_failure(stdout_bytes):
    """Clasifica un fallo del transporte por el último mensaje result de stream-json. Solo registra; nunca reintenta.
    → {"Class": "TRANSPORT_AUTH_REFRESH_CONFLICT", …} si el result contiene AUTH_REFRESH_MARK con 0 tokens; si no, None."""
    res = None
    for line in stdout_bytes.splitlines():
        try:
            o = json.loads(line)
        except ValueError:
            continue
        if isinstance(o, dict) and o.get("type") == "result":
            res = o
    if res is None:
        return None
    usage = res.get("usage") if isinstance(res.get("usage"), dict) else {}
    tokens = sum(int(usage.get(k) or 0) for k in ("input_tokens", "output_tokens", "cache_creation_input_tokens", "cache_read_input_tokens"))
    if AUTH_REFRESH_MARK in str(res.get("result") or "") and tokens == 0:
        return {"Class": "TRANSPORT_AUTH_REFRESH_CONFLICT", "Tokens": tokens, "TotalCostUsd": res.get("total_cost_usd"),
                "IsError": res.get("is_error"), "TerminalReason": res.get("terminal_reason"), "NumTurns": res.get("num_turns"),
                "Retry": "ninguno: el kit no reintenta; la sesión principal registra el intento y decide"}
    return None


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
                               t.get("TimeoutSeconds") == TIMEOUT_S and t.get("AuthSettleSeconds") == AUTH_SETTLE_S and
                               closure["AuthorityRevision"] == REV and closure.get("ObjectBlob") == PARAMS["Object"]["Blob"] and
                               closure.get("ObjectBlob") != OG.A6R1_BLOB and closure["Ids"]["RunId"] == OG.RUN_ID and closure.get("Provisional") is False and
                               t.get("ArgsTemplate") == TG.templatize(args[1:])}
    m = TG.measure_binary(BIN)
    c["3-Binary"] = dict({"Ok": m["Exists"] and m["Sha256"] == BIN_SHA and same_path(m["Path"], BIN_DISPLAY) and m["Authenticode"] == "Valid" and
                          m["SignerIsAnthropic"]}, **m)
    og = OG.check(WORKTREE, CLONE, REV, closure, m["Sha256"])   # 11 (§78-§80: Owner y autorización), evaluada antes de ejecutar el binario
    if dry_run:
        c["4-Version"] = {"Ok": None, "NotRun": "--dry-run: el binario no se ejecuta (`--version` solo en el lanzamiento real)"}
        c["5-Auth"] = {"Ok": None, "NotRun": "--dry-run: el binario no se ejecuta (`auth status` solo en el lanzamiento real)"}
    else:
        ver = TG.cli_version(BIN, KIT) if c["3-Binary"]["Ok"] and og["Ok"] else None   # sin la compuerta del Owner no se ejecuta el binario
        c["4-Version"] = {"Ok": ver == VERSION, "Got": ver, "NotExecutedBecauseOwnerGate": not og["Ok"]}
        auth = TG.auth_status(BIN, KIT) if c["4-Version"]["Ok"] else {}
        AUTH_DONE.append(time.monotonic())
        c["5-Auth"] = dict({"Ok": auth.get("loggedIn") is True}, **auth, CompletedAt=now())
    reparse = []
    for root, dirs, fs in os.walk(CLONE, followlinks=False):
        for n in dirs + fs:
            st = os.lstat(os.path.join(root, n))
            if stat.S_ISLNK(st.st_mode) or (getattr(st, "st_file_attributes", 0) & 0x400):
                reparse.append(n)
    blobs_bad = [r["Path"] for r in closure["CanonicalInputs"] if git("rev-parse", REV + ":" + r["Path"]).strip() != r["Blob"]]
    head, status = git("rev-parse", "HEAD").strip(), git("status", "--porcelain", "--ignored")
    branches, remotes = git("for-each-ref", "--format=%(refname)", "refs/heads").split(), git("remote").split()
    detached = git("rev-parse", "--symbolic-full-name", "HEAD").strip() == "HEAD"
    c["6-Clone"] = {"Ok": head == REV and status == "" and branches == [] and detached and remotes == [] and not blobs_bad and not reparse,
                    "Head": head, "Detached": detached, "Clean": status == "", "LocalBranches": branches, "Remotes": remotes,
                    "BlobMismatches": blobs_bad, "ReparsePoints": len(reparse)}
    listing = sorted(os.listdir(RUN))
    run_bad = []
    for n in RUN_FILES:
        s = sha_file(os.path.join(RUN, n)) if os.path.isfile(os.path.join(RUN, n)) else None
        if not (s and s == sha_file(os.path.join(KIT, n)) == closure["RunFileHashes"][n]["Sha256"] == manifest["RunFiles"][n]["Sha256"]):
            run_bad.append(n)
        elif n == "a6-r1.md" and git_blob(open(os.path.join(RUN, n), "rb").read()) != A6R1_BLOB:
            run_bad.append(n)
    c["7-RunFiles"] = {"Ok": listing == sorted(RUN_FILES) and not run_bad, "Listing": listing, "Mismatches": run_bad}
    prior = glob.glob(os.path.join(PROJECTS, "*", SESSION_ID + ".jsonl"))
    project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(PROJECTS, "D--r62-arch-a6r2*")))
    c["8-NewSession"] = {"Ok": not prior and not project_dirs, "PriorTranscripts": len(prior), "ProjectDirsForCloneOrRun": project_dirs}
    cl = len(subprocess.list2cmdline(args))
    c["9-CommandLine"] = {"Ok": cl < 32000, "Chars": cl}
    gate = json.load(open(os.path.join(KIT, TG.GATE_FILE), encoding="utf-8"))
    char_b, acc_b = TG.read_pinned(gate.get("Characterization")), TG.read_pinned(gate.get("Acceptance"))
    ev = TG.evaluate(gate, closure, char_b, acc_b)
    launch_template_ok = TG.templatize(args[1:]) == ev["ArgsTemplate"] == gate.get("ArgsTemplate")
    c["10-TransportGate"] = {"Ok": ev["Ok"] and launch_template_ok, "Status": ev["Status"], "LaunchArgsMatchTemplate": launch_template_ok,
                             "CharacterizationSha256": ev["CharacterizationSha256"], "AcceptanceSha256": ev["AcceptanceSha256"],
                             "Problems": ev["Problems"]}
    c[OG.CHECK] = og   # 11: compuerta del Owner y de la autorización (§78-§80)
    assert list(c) == TG.PREFLIGHT_CHECKS + [OG.CHECK]
    return c, all(v["Ok"] is True for v in c.values()), args, (char_b, acc_b)


def write_json(path, obj):
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


def main(argv):
    if argv == ["--record-probe"]:   # P: solo la sonda de registro (ni preflight, ni binario, ni <run>/launch)
        probe = record_probe()
        print(json.dumps({"RecordProbe": probe, "FrozenCommandGuard": FROZEN}, ensure_ascii=False, indent=1))
        return 0 if probe["Ok"] else 1
    dry_run = "--dry-run" in argv
    if os.path.exists(os.path.join(RUN, "launch")):
        print(json.dumps({"Refused": "ya existe %s\\launch: una sola invocación, sin reintento" % RUN}, ensure_ascii=False))
        return 2
    probe = record_probe()   # D: paso previo obligatorio (temporal fuera del run, del clon y del kit; se borra)
    if not dry_run and not probe["Ok"]:
        print(json.dumps({"Refused": "la sonda de registro falló: no se lanza (nada escrito en el run)", "RecordProbe": probe,
                          "FrozenCommandGuard": FROZEN}, ensure_ascii=False, indent=1))
        return 2
    checks, ok, args, (char_b, acc_b) = preflight(dry_run)
    if dry_run:
        print(json.dumps({"DryRun": "guarda de la orden congelada, sonda de registro (en un temporal que se borra) y preflight: no se ejecuta "
                                    "el binario, no se escribe en el kit, el run ni el clon y no se lanza",
                          "FrozenCommandGuard": FROZEN, "RecordProbe": probe, "RecordProbeOk": probe["Ok"],
                          "BlockingChecks": [k for k, v in checks.items() if v["Ok"] is False],
                          "AuthorizationSections": checks[OG.CHECK].get("AuthorizationSections"),   # fase 2: visibles para el Coordinator
                          "NotRunInDryRun": [k for k, v in checks.items() if v["Ok"] is None], "Checks": checks}, ensure_ascii=False, indent=1))
        return 2
    if not ok:
        print(json.dumps({"Refused": "preflight fallida: no se lanza", "Checks": checks}, ensure_ascii=False, indent=1))
        return 2
    out_dir = os.path.join(RUN, "launch")
    os.makedirs(out_dir)
    write_json(os.path.join(out_dir, "preflight.json"), {"SessionId": SESSION_ID, "At": now(), "Checks": checks, "FrozenCommandGuard": FROZEN,
                                                         "RecordProbe": probe})
    open(os.path.join(out_dir, TG.COPY_CHARACTERIZATION), "wb").write(char_b)
    if acc_b is not None:
        open(os.path.join(out_dir, TG.COPY_ACCEPTANCE), "wb").write(acc_b)
    prompt = open(os.path.join(RUN, "prompt.md"), "rb").read()
    settle = {"Seconds": AUTH_SETTLE_S, "AuthStatusCompletedAt": checks["5-Auth"].get("CompletedAt"), "WaitStartedAt": now()}
    waited = max(0.0, AUTH_SETTLE_S - (time.monotonic() - AUTH_DONE[-1])) if AUTH_DONE else float(AUTH_SETTLE_S)
    time.sleep(waited)
    settle.update({"WaitedSeconds": round(waited, 3), "WaitEndedAt": now()})
    resolved = os.path.realpath(BIN)
    rec = base_record(SESSION_ID, TG.portable(resolved), sha_file(resolved), args[1:], CLONE, prompt, TIMEOUT_S,
                      {k: checks["10-TransportGate"][k] for k in ("Status", "CharacterizationSha256", "AcceptanceSha256")}, settle)
    if rec["ExecutableSha256"] != BIN_SHA or not same_path(rec["Executable"], BIN_DISPLAY):
        rec["Aborted"] = "el binario cambió entre la preflight y el lanzamiento: no se lanza (borrar launch/ y volver a custodiar)"
        write_json(os.path.join(out_dir, "run.json"), rec)
        print(json.dumps({"Refused": rec["Aborted"]}, ensure_ascii=False))
        return 2
    rc = run_and_record(rec, [resolved] + args[1:], CLONE, prompt, out_dir, "run.json", TIMEOUT_S, TRANSCRIPT,
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

        def feed():
            try:
                p.stdin.write(stdin_bytes)
                p.stdin.close()
            except OSError as e:
                rec["StdinError"] = str(e)
        th = threading.Thread(target=feed, daemon=True)
        th.start()
        killed = timed_out = False
        try:
            p.wait(timeout=timeout_s)
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


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
