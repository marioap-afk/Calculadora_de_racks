"""I-62 / FX-02 — ENSAYO EN SECO del paquete ccli-meas: ejercita todo menos la invocación de claude.
Nunca ejecuta claude: CCLI_DRYRUN=1 hace que measure() rechace cualquier ejecutable claude; las raíces medidas son cmd.exe con ping; y un
rastreador rápido enraizado en este mismo proceso registra todos sus descendientes para demostrar al final que ninguno fue claude.exe.
Pasos (cada uno con lo esperado, lo observado y Pass):
  DR-00 guardas anti-claude; DR-01 clon de ensayo D:\\r62-fixture\\tmp-ccli-dry (make_clone.py); DR-02 preflight real de launch.py --dry-run
  (debe bloquear SOLO G1: faltan la línea del Owner y el marcador del Coordinator); DR-03 G4 con procesos señuelo (session id y ruta del clon
  en la línea de órdenes); DR-04 matriz de G1; DR-05 G2; DR-06 G3 negativos; DR-07 un solo uso y manifiesto; DR-08 G7; DR-09 G8;
  DR-10 árbol de procesos natural (ops 7/8, mecánica); DR-11 árbol con un resto vivo (KILLED_RESIDUAL → op 7 NOT_DEMONSTRATED);
  DR-12 regla de huérfanos (sintética); DR-13 validación de esquema (válido e inválidos; stdlib y Test-Json); DR-14 clasificación del stream
  (éxito, rechazo del esquema, conflicto OAuth, sin result); DR-15 saneado y barrido de fugas; DR-16 huella en modo ensayo (claves solo de
  U1); DR-17 análisis estático de aceptación del esquema por el binario (leído como datos, sin ejecutarlo); DR-18 audit.py de extremo a
  extremo sobre corridas sintéticas; DR-19 barrido de fugas del paquete; DR-20 limpieza (clon borrado, sin carpetas de proyecto, sin run/).
Escribe dry-run-report.json y dry-run-report.md (saneados) junto a este script. Uso: python -I -B dryrun.py
"""
import copy
import importlib.util
import json
import mmap
import os
import shutil
import subprocess
import sys
import tempfile
import time

PKG = os.path.dirname(os.path.abspath(__file__))
os.environ["CCLI_DRYRUN"] = "1"


def _load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(PKG, name + ".py"))
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


C = _load("ccli_common")
PT = _load("proctree")
SC = _load("schemacheck")
L = _load("launch")
A = _load("audit")
K = C.load_constants(PKG)
SAN = C.Sanitizer()
DRY_CLONE = r"D:\r62-fixture\tmp-ccli-dry"
CMD = os.path.join(os.environ.get("SystemRoot", r"C:\Windows"), "System32", "cmd.exe")
PY = sys.executable
STEPS = []


def step(sid, title, expected, observed, ok):
    STEPS.append({"Id": sid, "Title": title, "Expected": expected, "Observed": observed, "Pass": bool(ok)})
    print("%s %s: %s" % (sid, "PASS" if ok else "FAIL", title), flush=True)


def run_py(script, *args, env=None):
    e = dict(os.environ)
    e.update(env or {})
    r = subprocess.run([PY, "-I", "-B", os.path.join(PKG, script)] + list(args), capture_output=True, text=True, encoding="utf-8",
                       errors="replace", env=e)
    return r.returncode, r.stdout, r.stderr


def dummy(cmdline_tail, seconds=20):
    """Proceso señuelo: cmd.exe que espera con ping; la cola (session id o ruta) queda en SU línea de órdenes, no en la de este script."""
    return subprocess.Popen([CMD, "/d", "/c", "ping -n %d 127.0.0.1 >nul & rem %s" % (seconds, cmdline_tail)],
                            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)


def kill_tree(p):
    subprocess.run(["taskkill", "/PID", str(p.pid), "/T", "/F"], capture_output=True)
    try:
        p.wait(timeout=10)
    except subprocess.TimeoutExpired:
        pass


def short_timing():
    T = dict(K["Timing"])
    T.update({"TimeoutS": 60, "Settle1S": 3, "Settle2S": 8})
    return T


def synthetic_result():
    fb = A.fixed_block()
    r = dict(fb)
    obj = K["Clone"]
    r.update({
        "InjectedContextDeclaration": {"State": "NONE_DECLARED", "Items": []},
        "InputsRead": [obj["ObjectPath"]],
        "KnownLimitations": ["medición del transporte (ensayo sintético)"],
        "RecommendedNextAction": "ninguna (ensayo sintético)",
        "Verdict": "AGREED",
        "RequiredFindings": [],
        "OptionalFindings": [{"FindingId": "DRY-O1", "LineageId": None, "Source": "ensayo", "AffectedSection": "RequiredTests",
                              "Evidence": "ejemplo sintético", "PremiseRefs": [{"Path": obj["ObjectPath"], "Section": "RequiredTests", "LineStart": 41,
                                                                               "LineEnd": 43, "Quote": "\"RequiredTests\": ["}],
                              "WhyItMatters": "ejemplo", "CorrectionRequired": "ninguna"}],
        "FindingDispositions": [{"FindingId": "DRY-O1", "LineageId": "DRY-O1", "State": "OPEN", "SupersededBy": [], "Rationale": "ejemplo",
                                 "EvaluatedObject": {"commit": obj["Commit"], "path": obj["ObjectPath"], "blob": obj["ObjectBlob"]},
                                 "PremiseRefs": [{"Path": obj["ObjectPath"], "Section": "RequiredTests", "LineStart": 41, "LineEnd": 43,
                                                  "Quote": "\"RequiredTests\": ["}]}],
    })
    return r


def synthetic_stream(sid, clone, result, mode):
    init = {"type": "system", "subtype": "init", "session_id": sid, "cwd": clone, "model": "claude-opus-5-5", "permissionMode": "dontAsk",
            "tools": ["Glob", "Grep", "Read", "StructuredOutput"], "mcp_servers": [], "claude_code_version": "2.1.293", "output_style": "default",
            "apiKeySource": "none", "plugins": [{"name": "github@claude-plugins-official"}], "agents": ["Explore"], "uuid": "x"}
    a = lambda tu: {"type": "assistant", "session_id": sid, "message": {"model": "claude-opus-5-5", "content": [tu]}}
    path = K["Clone"]["ObjectPath"]
    if mode == "success":
        lines = [init,
                 a({"type": "tool_use", "id": "t1", "name": "Glob", "input": {"pattern": path}}),
                 a({"type": "tool_use", "id": "t2", "name": "Grep", "input": {"pattern": "\"Role\"", "path": path, "output_mode": "content"}}),
                 a({"type": "tool_use", "id": "t3", "name": "Read", "input": {"file_path": os.path.join(clone, path)}}),
                 a({"type": "tool_use", "id": "t4", "name": "StructuredOutput", "input": result}),
                 {"type": "result", "subtype": "success", "is_error": False, "session_id": sid, "structured_output": result, "num_turns": 5,
                  "terminal_reason": "completed", "permission_denials": [], "usage": {"input_tokens": 10, "output_tokens": 10}}]
        return lines, ""
    if mode == "schema-rejected":
        return [], "Error: --json-schema is not a valid JSON Schema: no schema with key or ref \"https://json-schema.org/draft/2020-12/schema\"\n"
    if mode == "auth":
        return [init, {"type": "result", "subtype": "success", "is_error": True, "session_id": sid, "num_turns": 1, "terminal_reason": "api_error",
                       "result": "Failed to refresh OAuth token: another Claude Code process is refreshing it", "usage": {"input_tokens": 0, "output_tokens": 0}}], ""
    if mode == "no-result":
        return [init], ""
    raise ValueError(mode)


def write_launch_dir(base, run, pre, stream_lines, stderr_text, proc_src, fb, fa):
    launch = os.path.join(base, "launch")
    os.makedirs(launch)
    with open(os.path.join(base, "ONE-SHOT.marker"), "w", encoding="utf-8") as f:
        f.write("{\"Marker\": \"ONE-SHOT (ensayo)\"}\n")
    with open(os.path.join(launch, "stdout.jsonl"), "w", encoding="utf-8", newline="\n") as f:
        for o in stream_lines:
            f.write(json.dumps(o, ensure_ascii=False) + "\n")
    with open(os.path.join(launch, "stderr.txt"), "w", encoding="utf-8") as f:
        f.write(stderr_text)
    for n in ("proc-fast.json", "proc-cim.jsonl", "termination.json"):
        shutil.copy(os.path.join(proc_src, n), os.path.join(launch, n))
    C.write_json(os.path.join(launch, "run.json"), run)
    C.write_json(os.path.join(launch, "preflight.json"), pre)
    C.write_json(os.path.join(launch, "fingerprint-before.json"), fb)
    C.write_json(os.path.join(launch, "fingerprint-after.json"), fa)
    return launch


def main():
    t0 = time.time()
    selfwatch = PT.FastTracker(0.1)
    selfwatch.set_root(os.getpid())
    selfwatch.start()
    tmp_root = tempfile.mkdtemp(prefix="ccli-dry-")
    try:
        # DR-00
        try:
            L.measure([C.expand(K["Binary"]["Display"]), "--help"], PKG, b"", tmp_root, short_timing(), DRY_CLONE, allow_claude=True)
            refused = False
        except RuntimeError:
            refused = True
        step("DR-00", "measure() rechaza el binario claude con CCLI_DRYRUN=1 (sin ejecutarlo)", "RuntimeError antes de cualquier Popen",
             {"Refused": refused}, refused)

        # DR-01
        if os.path.exists(DRY_CLONE):
            shutil.rmtree(DRY_CLONE, onerror=lambda f, p, e: (os.chmod(p, 0o666), f(p)))
        rc, out, err = run_py("make_clone.py", "--target", DRY_CLONE)
        v = json.loads(out) if out.strip().startswith("{") else {"Raw": err[-300:]}
        step("DR-01", "clon de ensayo creado y verificado (G3)", "G3 Ok", {k: v.get(k) for k in ("Ok", "Head", "Branches", "Remotes", "Clean",
             "ObjectBlob", "OnlyHeadHistory", "AutoCrlf")}, rc == 0 and v.get("Ok"))

        # DR-02
        rc, out, err = run_py("launch.py", "--dry-run", env={"CCLI_DRY_CLONE": DRY_CLONE})
        pre = json.loads(out) if out.strip().startswith("{") else {"Raw": err[-500:]}
        blocking = pre.get("BlockingChecks")
        step("DR-02", "preflight real (--dry-run): sin ejecutar el binario y sin escribir; bloquea solo G1 (falta la autoridad)",
             "código 2; BlockingChecks = [G1-Decisions]; G2..G11 Ok; run/ sigue sin existir",
             {"ExitCode": rc, "BlockingChecks": blocking, "G1": {k: (pre.get("Checks") or {}).get("G1-Decisions", {}).get(k) for k in
                                                                 ("OwnerLinePresent", "CoordinatorMarkerPresent", "Commit", "Blob", "ExpectedMarker")},
              "RunDirExists": os.path.exists(os.path.join(PKG, "run"))},
             rc == 2 and blocking == ["G1-Decisions"] and not os.path.exists(os.path.join(PKG, "run")))
        preflight_checks = pre.get("Checks") or {}

        # DR-03
        g_clean, _ = L.gate_processes(DRY_CLONE)
        d1 = dummy(K["SessionId"])
        time.sleep(1.5)
        g_sid, _ = L.gate_processes(DRY_CLONE)
        kill_tree(d1)
        d2 = dummy(DRY_CLONE)
        time.sleep(1.5)
        g_clone, _ = L.gate_processes(DRY_CLONE)
        kill_tree(d2)
        g_after, _ = L.gate_processes(DRY_CLONE)
        ok = g_clean["Ok"] and not g_sid["Ok"] and g_sid["SidProcs"] >= 1 and not g_clone["Ok"] and g_clone["CloneRefs"] >= 1 and g_after["Ok"]
        step("DR-03", "G4: un señuelo con el session id y otro con la ruta del clon hacen rechazar; sin señuelos pasa",
             "limpio Ok; con session id rechazo; con ruta del clon rechazo; tras matar los señuelos Ok",
             {"Clean": g_clean, "WithSid": g_sid, "WithClonePath": g_clone, "After": g_after}, ok)

        # DR-04
        ow, mt, ph = L.GATES["OwnerLine"], L.GATES["CoordinatorMarkerTemplate"], L.GATES["Placeholder"]
        msha = C.check_manifest(PKG, K["AllowedUnlistedFiles"])["ManifestSha256"] or "0" * 64
        mk = mt.replace(ph, msha)
        cases = [
            ("ninguna línea", "texto\nsin autoridad\n", False),
            ("solo Owner", "a\n" + ow + "\nb\n", False),
            ("solo marcador", "a\n" + mk + "\n", False),
            ("ambas exactas (LF)", "x\n```text\n" + ow + "\n```\n" + mk + "\n", True),
            ("ambas exactas (CRLF)", "x\r\n" + ow + "\r\n" + mk + "\r\n", True),
            ("Owner con prefijo '- '", "- " + ow + "\n" + mk + "\n", False),
            ("Owner con espacio final", ow + " \n" + mk + "\n", False),
            ("marcador con otro SHA-256", ow + "\n" + mt.replace(ph, "f" * 64) + "\n", False),
            ("Owner abreviado", ow[:-20] + ")\n" + mk + "\n", False),
        ]
        res = []
        for name, text, exp in cases:
            g = C.gate_decisions(text, ow, mt, ph, msha)
            res.append({"Case": name, "Expected": exp, "Got": g["Ok"]})
        step("DR-04", "G1: líneas completas exactas (matriz de 9 casos)", "solo los dos casos exactos pasan", res,
             all(r["Expected"] == r["Got"] for r in res))

        # DR-05
        gb = C.gate_binary(C.expand(K["Binary"]["Display"]), K["Binary"]["Sha256"])
        fake = os.path.join(tmp_root, "claude.exe")
        open(fake, "wb").write(b"not the binary")
        gf = C.gate_binary(fake, K["Binary"]["Sha256"], check_signature=False)
        gm = C.gate_binary(os.path.join(tmp_root, "missing.exe"), K["Binary"]["Sha256"], check_signature=False)
        step("DR-05", "G2: binario fijado Ok; archivo con otro SHA-256 y archivo ausente rechazados",
             "real Ok (SHA-256 y Authenticode de Anthropic); falso rechazo; ausente rechazo",
             {"Real": {k: gb[k] for k in ("Ok", "Sha256", "Authenticode", "SignerIsAnthropic")}, "WrongHash": gf["Ok"], "Missing": gm["Ok"]},
             gb["Ok"] and not gf["Ok"] and not gm["Ok"])

        # DR-06
        cc = K["Clone"]
        neg = {}
        gc = lambda: C.gate_clone(DRY_CLONE, cc["Commit"], cc["ObjectPath"], cc["ObjectBlob"], cc["RequiredPrefix"])["Ok"]
        p = os.path.join(DRY_CLONE, "untracked.txt")
        open(p, "w").write("x")
        neg["untracked"] = gc()
        os.remove(p)
        C.git(DRY_CLONE, "remote", "add", "origin", "D:/nowhere.git")
        neg["remote"] = gc()
        C.git(DRY_CLONE, "remote", "remove", "origin")
        C.git(DRY_CLONE, "branch", "extra")
        neg["extra-branch"] = gc()
        C.git(DRY_CLONE, "branch", "-D", "extra")
        os.mkdir(os.path.join(DRY_CLONE, ".claude"))
        neg["dot-claude"] = gc()
        os.rmdir(os.path.join(DRY_CLONE, ".claude"))
        restored = gc()
        step("DR-06", "G3 negativos: archivo sin seguimiento, remoto, rama extra y .claude/ rechazados; restaurado Ok",
             "los cuatro False y restaurado True", dict(neg, restored=restored), not any(neg.values()) and restored)

        # DR-07
        fakepkg = os.path.join(tmp_root, "pkgcopy")
        shutil.copytree(PKG, fakepkg, ignore=shutil.ignore_patterns("run"))
        base = C.check_manifest(fakepkg, K["AllowedUnlistedFiles"])["Ok"]
        os.mkdir(os.path.join(fakepkg, "run"))
        one_shot = C.gate_one_shot(fakepkg)["Ok"]
        os.rmdir(os.path.join(fakepkg, "run"))
        with open(os.path.join(fakepkg, "prompt.md"), "ab") as f:
            f.write(b" ")
        tampered = C.check_manifest(fakepkg, K["AllowedUnlistedFiles"])
        open(os.path.join(fakepkg, "extra.txt"), "w").write("x")
        os.mkdir(os.path.join(fakepkg, "__pycache__"))
        unl = C.check_manifest(fakepkg, K["AllowedUnlistedFiles"])
        step("DR-07", "G5 un solo uso y G6 manifiesto: run/ existente, archivo alterado, archivo extra y __pycache__ rechazados",
             "copia íntegra Ok; con run/ rechazo; alterado → Mismatches; extra y __pycache__ → Unlisted",
             {"CopyOk": base, "OneShotWithRun": one_shot, "Mismatches": tampered["Mismatches"], "Unlisted": unl["Unlisted"]},
             base and not one_shot and tampered["Mismatches"] == ["prompt.md"] and set(unl["Unlisted"]) >= {"extra.txt", "__pycache__"})

        # DR-08
        e_bad = C.gate_env({"CLAUDE_CONFIG_DIR": "x", "PATH": "y"})
        e_ok = C.gate_env({"PATH": "y", "CLAUDE_CODE_X": "z"})
        step("DR-08", "G7: CLAUDE_CONFIG_DIR presente rechazada; ausente Ok (solo nombres)", "False / True",
             {"WithConfigDir": e_bad["Ok"], "Without": e_ok["Ok"]}, not e_bad["Ok"] and e_ok["Ok"])

        # DR-09
        proj = os.path.join(tmp_root, "projects")
        os.makedirs(os.path.join(proj, "D--other"))
        g_ok = C.gate_new_session(proj, K["SessionId"], [C.project_dir_name(K["Clone"]["Path"])])["Ok"]
        open(os.path.join(proj, "D--other", K["SessionId"] + ".jsonl"), "w").close()
        g_t = C.gate_new_session(proj, K["SessionId"], [C.project_dir_name(K["Clone"]["Path"])])["Ok"]
        os.remove(os.path.join(proj, "D--other", K["SessionId"] + ".jsonl"))
        os.makedirs(os.path.join(proj, C.project_dir_name(K["Clone"]["Path"])))
        g_d = C.gate_new_session(proj, K["SessionId"], [C.project_dir_name(K["Clone"]["Path"])])["Ok"]
        step("DR-09", "G8: transcripción con el session id o carpeta de proyecto del clon rechazadas", "True / False / False",
             {"Empty": g_ok, "PriorTranscript": g_t, "ProjectDir": g_d, "ProjectDirName": C.project_dir_name(K["Clone"]["Path"])},
             g_ok and not g_t and not g_d)

        # DR-10
        out10 = os.path.join(tmp_root, "tree-natural")
        os.makedirs(out10)
        rec10 = L.measure([CMD, "/d", "/c", "ping -n 3 127.0.0.1 >nul & ping -n 2 127.0.0.1 >nul"], out10, b"", out10, short_timing(), DRY_CLONE)
        fast10 = json.load(open(os.path.join(out10, "proc-fast.json"), encoding="utf-8"))
        term10 = json.load(open(os.path.join(out10, "termination.json"), encoding="utf-8"))
        ticks10 = A.cim_ticks(os.path.join(out10, "proc-cim.jsonl"))
        sa_empty = {"Init": None, "ToolUses": []}
        v7, r7, v8, r8, d78 = A.eval_ops78(sa_empty, rec10, fast10, ticks10, term10, CMD, None, representative_required=False)
        step("DR-10", "árbol natural (cmd → ping, ping): identidades, clases, cobertura y terminación confirmada",
             "op 7 y op 8 DEMONSTRATED con el criterio de representatividad desactivado (no hay stream)",
             {"Mode": term10["Mode"], "Op7": [v7, r7], "Op8": [v8, r8], "ClassCounts": d78["ClassCounts"], "Coverage": d78["Coverage"],
              "Members": [(x["Name"], x["Class"], x["Sources"]) for x in d78["Tree"]]},
             v7 == "DEMONSTRATED" and v8 == "DEMONSTRATED" and len(d78["Tree"]) >= 3)

        # DR-11
        out11 = os.path.join(tmp_root, "tree-residual")
        os.makedirs(out11)
        rec11 = L.measure([CMD, "/d", "/c", "start /b ping -n 60 127.0.0.1 >nul"], out11, b"", out11, short_timing(), DRY_CLONE)
        fast11 = json.load(open(os.path.join(out11, "proc-fast.json"), encoding="utf-8"))
        term11 = json.load(open(os.path.join(out11, "termination.json"), encoding="utf-8"))
        ticks11 = A.cim_ticks(os.path.join(out11, "proc-cim.jsonl"))
        v7b, r7b, v8b, r8b, d78b = A.eval_ops78(sa_empty, rec11, fast11, ticks11, term11, CMD, None, representative_required=False)
        last = term11["Checks"][-1]
        step("DR-11", "árbol con un resto vivo (start /b ping): S1 y S2 lo ven, se mata y S3 lo confirma muerto",
             "Mode KILLED_RESIDUAL; op 7 NOT_DEMONSTRATED (R7.3); S3 sin identidades vivas",
             {"Mode": term11["Mode"], "Labels": [c["Label"] for c in term11["Checks"]], "Op7": [v7b, r7b], "S3FastAlive": last["Fast"]["Alive"],
              "S3CimAlive": last["CimOnce"]["AliveTree"], "Killed": term11.get("KilledResidual")},
             term11["Mode"] == "KILLED_RESIDUAL" and v7b == "NOT_DEMONSTRATED" and not last["Fast"]["Alive"] and not last["CimOnce"]["AliveTree"]
             and any(k.get("TerminateProcess") for k in (term11.get("KilledResidual") or [])))

        # DR-11b
        out11b = os.path.join(tmp_root, "tree-timeout")
        os.makedirs(out11b)
        tt = short_timing()
        tt["TimeoutS"] = 3
        rec11b = L.measure([CMD, "/d", "/c", "ping -n 30 127.0.0.1 >nul"], out11b, b"", out11b, tt, DRY_CLONE)
        term11b = json.load(open(os.path.join(out11b, "termination.json"), encoding="utf-8"))
        fast11b = json.load(open(os.path.join(out11b, "proc-fast.json"), encoding="utf-8"))
        ticks11b = A.cim_ticks(os.path.join(out11b, "proc-cim.jsonl"))
        v7c, r7c, _, _, _ = A.eval_ops78(sa_empty, rec11b, fast11b, ticks11b, term11b, CMD, None, representative_required=False)
        lastb = term11b["Checks"][-1]
        step("DR-11b", "tope vencido (3 s): taskkill /T del árbol, S1 sin identidades vivas; terminación no natural",
             "TimedOut; Mode TIMEOUT_TREE_KILLED; S1 sin vivos; op 7 NOT_DEMONSTRATED (R7.3)",
             {"TimedOut": rec11b.get("TimedOut"), "Taskkill": rec11b.get("TaskkillTree"), "Mode": term11b["Mode"], "S1FastAlive": lastb["Fast"]["Alive"],
              "S1CimAlive": lastb["CimOnce"]["AliveTree"], "Op7": [v7c, r7c]},
             rec11b.get("TimedOut") and term11b["Mode"] == "TIMEOUT_TREE_KILLED" and not lastb["Fast"]["Alive"] and not lastb["CimOnce"]["AliveTree"]
             and v7c == "NOT_DEMONSTRATED")

        # DR-12
        snap = [(1, 0, "explorer.exe", 100), (10, 1, "claude.exe", 200), (11, 10, "rg.exe", 300), (12, 99, "git.exe", 400),
                (13, 1, "git.exe", 450), (14, 20, "rg.exe", 500), (20, 1, "cmd.exe", 600), (15, 98, "notepad.exe", 400), (16, 97, "node.exe", 50)]
        orph = PT.find_orphans(snap, {(10, 200), (11, 300)}, 150, 1000, set(K["ClosedProcessList"]))
        got = sorted((o["Pid"], o["Reason"]) for o in orph)
        step("DR-12", "regla de huérfanos (sintética): padre inexistente o con CreationDate posterior; fuera de ventana y fuera de la lista no cuentan",
             "huérfanos = PID 12 (padre inexistente) y 14 (padre posterior)", got,
             [p for p, _ in got] == [12, 14])

        # DR-13
        schema_path = os.path.join(PKG, K["SchemaFile"])
        valid = synthetic_result()
        variants = []

        def var(name, fn, exp_valid):
            o = copy.deepcopy(valid)
            fn(o)
            variants.append((name, o, exp_valid))
        var("válido", lambda o: None, True)
        var("LogicalReviewRequestId null (permitido)", lambda o: o.__setitem__("LogicalReviewRequestId", None), True)
        var("falta Verdict", lambda o: o.pop("Verdict"), False)
        var("propiedad extra", lambda o: o.__setitem__("Extra", 1), False)
        var("ReviewedCommit corto", lambda o: o.__setitem__("ReviewedCommit", "d30fb6a9"), False)
        var("Verdict fuera del enum", lambda o: o.__setitem__("Verdict", "PASS"), False)
        var("AttemptSeq 0", lambda o: o.__setitem__("AttemptSeq", 0), False)
        var("Location.Kind fuera del enum", lambda o: o["ReviewerBinding"]["Location"].__setitem__("Kind", "LOCAL"), False)
        var("SizeOrSha256 inválido", lambda o: o.__setitem__("InjectedContextDeclaration",
                                                              {"State": "DECLARED", "Items": [{"Kind": "k", "Source": "s", "SizeOrSha256": "12kb"}]}), False)
        var("InvocationId con salto final ($ ECMA)", lambda o: o.__setitem__("InvocationId", o["InvocationId"] + "\n"), False)
        var("PremiseRefs.LineStart 0", lambda o: o["OptionalFindings"][0]["PremiseRefs"][0].__setitem__("LineStart", 0), False)
        rows = []
        for name, o, exp in variants:
            ck = SC.check(o, schema_path)
            rows.append({"Case": name, "Expected": exp, "Stdlib": ck["Stdlib"]["Valid"], "TestJson": ck["TestJson"].get("Valid"),
                         "Agree": ck["Agree"], "Combined": ck["Valid"]})
        ok13 = all(r["Combined"] == r["Expected"] and r["Stdlib"] == r["Expected"] for r in rows)
        step("DR-13", "validación contra el esquema canónico: stdlib y Test-Json (pwsh 7)", "Combined = Expected en los 11 casos", rows, ok13)

        # DR-14
        sid = K["SessionId"]
        outcomes = {}
        for mode in ("success", "schema-rejected", "auth", "no-result"):
            lines, se = synthetic_stream(sid, DRY_CLONE, valid, mode)
            sa = A.analyze_stream(lines, [], se)
            v5, r5, _, _ = A.eval_op5(sa, schema_path, sid)
            outcomes[mode] = {"Op5": v5, "Reasons": r5, "SchemaRejected": sa["SchemaRejected"], "AuthRefreshConflict": sa["AuthRefreshConflict"]}
        ok14 = (outcomes["success"]["Op5"] == "DEMONSTRATED" and outcomes["schema-rejected"]["SchemaRejected"]
                and outcomes["schema-rejected"]["Op5"] == "NOT_DEMONSTRATED" and outcomes["auth"]["AuthRefreshConflict"]
                and outcomes["no-result"]["Op5"] == "NOT_DEMONSTRATED")
        step("DR-14", "clasificación del stream: éxito, rechazo del esquema, conflicto OAuth y sin result",
             "éxito → op 5 DEMONSTRATED; rechazo → SchemaRejected y NOT_DEMONSTRATED; OAuth → AuthRefreshConflict; sin result → NOT_DEMONSTRATED",
             outcomes, ok14)

        # DR-15
        up = os.environ.get("USERPROFILE", "")
        short = C._short_path(up) or up
        user, host = os.environ.get("USERNAME", ""), os.environ.get("COMPUTERNAME", "")
        samples = [up + "\\x\\y.txt", short + "\\AppData\\Local\\Temp\\z", up.replace("\\", "/") + "/a", up.replace("\\", "\\\\") + "\\\\b",
                   "usuario " + user + " en " + host, "correo " + "persona.prueba" + chr(64) + "example.org", os.environ.get("APPDATA", "") + "\\Claude\\x"]
        leaks_before = [bool(SAN.leak_classes(s)) for s in samples]
        cleaned = [SAN.text(s) for s in samples]
        leaks_after = [SAN.leak_classes(s) for s in cleaned]
        scan_dir = os.path.join(tmp_root, "leakscan")
        os.makedirs(scan_dir)
        open(os.path.join(scan_dir, "bad.json"), "w", encoding="utf-8").write(json.dumps({"p": samples[0]}))
        open(os.path.join(scan_dir, "good.json"), "w", encoding="utf-8").write(json.dumps({"p": cleaned[0]}))
        scan = SAN.leak_scan(scan_dir)
        step("DR-15", "saneado (perfil largo y 8.3, / y \\\\, usuario, equipo, correo, %APPDATA%) y barrido de fugas",
             "todas las muestras con fuga antes y ninguna después; el barrido detecta solo bad.json",
             {"LeakBefore": leaks_before, "LeakAfter": leaks_after, "Cleaned": cleaned[2:3] + cleaned[5:7], "ScanFiles": [s["File"] for s in scan]},
             all(leaks_before) and not any(leaks_after) and [s["File"] for s in scan] == ["bad.json"])

        # DR-16
        fp = C.fingerprint(DRY_CLONE, keys_for={"U1"})
        fp2 = C.fingerprint(DRY_CLONE, keys_for={"U1"})
        diff = C.fingerprint_diff(fp, fp2)
        env_names = fp["EnvNames"]["ClaudeAnthropic"]
        u1 = next(c for c in fp["Candidates"] if c["Id"] == "U1")
        step("DR-16", "huella en modo ensayo: existencia, SHA-256 y nombres de claves SOLO de U1; .claude.json solo metadatos; nombres de variables",
             "U1 con KeyNames lista; el resto sin claves listadas; dos mediciones seguidas estables",
             {"Candidates": [{k: c.get(k) for k in ("Id", "Path", "Exists", "Sha256", "KeyNames")} for c in fp["Candidates"]],
              "EnvNamesCount": len(env_names), "EnvVsA4": {"Added": sorted(set(env_names) - set(K["ReferenceEnvNamesA4"])),
                                                         "Missing": sorted(set(K["ReferenceEnvNamesA4"]) - set(env_names))},
              "Stable": diff["Stable"]},
             isinstance(u1.get("KeyNames"), list) and diff["Stable"] and all(
                 c.get("KeyNames") in (None, "NO_LISTADAS_EN_ESTE_MODO") or c["Id"] == "U1" for c in fp["Candidates"] if c["Exists"]))

        # DR-17
        schema = json.load(open(schema_path, encoding="utf-8"))
        kws = set()

        def walk(s):
            if isinstance(s, dict):
                for k2, v2 in s.items():
                    kws.add(k2)
                    if k2 == "properties":
                        for vv in v2.values():
                            walk(vv)
                    elif isinstance(v2, (dict, list)) and k2 not in ("enum",):
                        walk(v2)
            elif isinstance(s, list):
                for x in s:
                    walk(x)
        walk(schema)
        draft07 = {"type", "enum", "const", "properties", "required", "additionalProperties", "items", "minLength", "maxLength", "pattern",
                   "minimum", "maximum", "title", "description", "$schema"}
        needles = {
            "error_template": b"Error: --json-schema is not a valid JSON Schema: ",
            "so_validator_default_ajv": b"new n({allErrors:!0,validateFormats:!1,multipleOfPrecision:6});if(!r.validateSchema(e))",
            "so_uses_class_Ajv": b"let{Ajv:n}=aAn()",
            "ajv_default_meta_draft07": b"Qt=\"http://json-schema.org/draft-07/schema\";class pt extends Sh.default",
            "validateSchema_resolves_dollar_schema": b"if(r=r||this.opts.defaultMeta||this.defaultMeta(),!r)return this.logger.warn(\"meta-schema not available\"),this.errors=null,!0;let s=this.validate(r,e);",
            "no_schema_with_key_or_ref": b"no schema with key or ref \"",
        }
        found = {}
        binp = C.expand(K["Binary"]["Display"])
        with open(binp, "rb") as f:
            m = mmap.mmap(f.fileno(), 0, access=mmap.ACCESS_READ)
            for k2, nd in needles.items():
                found[k2] = m.find(nd)
            m.close()
        all_found = all(v2 >= 0 for v2 in found.values())
        step("DR-17", "aceptación del esquema canónico VERBATIM por 2.1.293: análisis estático del binario (leído como datos, sin ejecutarlo)",
             "informativo: se documenta la predicción; Pass = el análisis se pudo hacer sobre el binario fijado",
             {"SchemaDollarSchema": schema.get("$schema"), "Keywords": sorted(kws - {"properties"}) + ["properties"],
              "NonDraft07Keywords": sorted(k2 for k2 in kws if k2 not in draft07 and k2 not in ("properties",)),
              "BinarySha256Pinned": C.sha_file(binp) == K["Binary"]["Sha256"], "NeedleOffsets": found,
              "Prediction": ("RECHAZO PROBABLE (INFERENCIA): la ruta de --json-schema compila con la clase Ajv por defecto (meta-esquema draft-07) "
                             "y llama a validateSchema; con $schema = draft 2020-12, validate($schema) no encuentra el meta-esquema y lanza "
                             "«no schema with key or ref», que la CLI convierte en «Error: --json-schema is not a valid JSON Schema: …» y termina "
                             "antes de llamar al modelo") if all_found else "SIN PREDICCIÓN: no se encontraron todas las cadenas",
              "AlternativeNotApplied": "sin \"$schema\" el resto de palabras clave pertenece al vocabulario de draft-07 (se compilaría); no se aplica: el paquete conserva el esquema VERBATIM por orden"},
             True)

        # DR-18
        e2e = {}
        pre_obj = {"SessionId": sid, "At": C.now(), "Checks": preflight_checks}
        base_run = dict(rec10, SessionId=sid, Args=L.build_args()[1:], Executable=CMD, StdoutSha256="0" * 64,
                        Transcript={"Exists": False, "Sha256": None})
        for mode, expect in (("success", {"5": "DEMONSTRATED", "7": "DEMONSTRATED", "8": "DEMONSTRATED", "9": "DEMONSTRATED"}),
                             ("schema-rejected", {"5": "NOT_DEMONSTRATED", "7": "NOT_DEMONSTRATED", "8": "NOT_DEMONSTRATED", "9": "NOT_DEMONSTRATED"}),
                             ("auth", {"5": "NOT_DEMONSTRATED", "7": "NOT_DEMONSTRATED", "8": "NOT_DEMONSTRATED", "9": "NOT_DEMONSTRATED"})):
            base = os.path.join(tmp_root, "e2e-" + mode)
            lines, se = synthetic_stream(sid, DRY_CLONE, valid, mode)
            run = dict(base_run, ExitCode=0 if mode == "success" else 1)
            write_launch_dir(base, run, pre_obj, lines, se, out10, fp, fp2)
            rc, out, err = run_py("audit.py", "--run-dir", base, "--pinned-path", CMD, "--clone", DRY_CLONE, "--label", "dry")
            au = json.load(open(os.path.join(base, "audit.json"), encoding="utf-8")) if os.path.isfile(os.path.join(base, "audit.json")) else {}
            got = {k: v["Verdict"] for k, v in (au.get("Verdicts") or {}).items()}
            e2e[mode] = {"ExitCode": rc, "LaunchClass": au.get("LaunchClass"), "Verdicts": got, "Expected": expect,
                         "Reasons": {k: v["Reasons"] for k, v in (au.get("Verdicts") or {}).items()},
                         "CustodyReady": au.get("CustodyReady"), "CustodyFiles": sorted(os.listdir(os.path.join(base, "custody")))
                         if os.path.isdir(os.path.join(base, "custody")) else [], "Op9Kind": ((au.get("Op9") or {}).get("Declaration") or {}).get("Kind"),
                         "Stderr": err[-300:] if rc else ""}
        ok18 = all(e2e[m]["Verdicts"] == e2e[m]["Expected"] and e2e[m]["CustodyReady"] for m in e2e) and e2e["auth"]["LaunchClass"] == "INVALID_LAUNCH"
        step("DR-18", "audit.py de extremo a extremo sobre tres corridas sintéticas (éxito, rechazo del esquema, conflicto OAuth) con el árbol de DR-10",
             "éxito: las cuatro DEMONSTRATED; rechazo y OAuth: las cuatro NOT_DEMONSTRATED (OAuth = INVALID_LAUNCH); custodia saneada sin fugas", e2e, ok18)

        # DR-19
        pk = SAN.leak_scan(PKG, skip_dirs=("run",))
        pk = [x for x in pk if x["File"] not in ("dry-run-report.json", "dry-run-report.md")]
        step("DR-19", "barrido de fugas del paquete (usuario, equipo, rutas del perfil, correos)", "ninguna fuga", pk, not pk)
    finally:
        selfwatch.stop()
        members = selfwatch.export()["Members"]
        selfwatch.close()
        names = sorted({(m.get("Name") or "?").lower() for m in members})
        claude_seen = [n for n in names if "claude" in n]
        step("DR-00b", "ningún descendiente de este ensayo fue claude (rastreador rápido enraizado en el propio proceso)",
             "sin claude.exe entre los descendientes", {"DescendantImageNames": names, "Count": len(members)}, not claude_seen)
        # DR-20
        if os.path.exists(DRY_CLONE):
            shutil.rmtree(DRY_CLONE, onerror=lambda f, p, e: (os.chmod(p, 0o666), f(p)))
        shutil.rmtree(tmp_root, ignore_errors=True)
        import glob
        left = glob.glob(r"D:\r62-fixture\tmp-ccli-*")
        projs = glob.glob(os.path.join(os.path.expanduser("~"), ".claude", "projects", "D--r62-fixture-tmp-ccli*"))
        step("DR-20", "limpieza: clon de ensayo borrado, sin D:\\r62-fixture\\tmp-ccli-*, sin carpetas de proyecto del clon, sin run/ en el paquete",
             "todo vacío", {"TmpClones": left, "ProjectDirs": [os.path.basename(p) for p in projs], "RunDir": os.path.exists(os.path.join(PKG, "run"))},
             not left and not projs and not os.path.exists(os.path.join(PKG, "run")))
    report = {"Schema": "rackcad-ccli-meas-dry-run/v1", "RunId": K["RunId"], "At": C.now(), "DurationS": round(time.time() - t0, 1),
              "ManifestSha256": C.check_manifest(PKG, K["AllowedUnlistedFiles"])["ManifestSha256"], "ClaudeInvoked": False,
              "Passed": sum(1 for s in STEPS if s["Pass"]), "Total": len(STEPS), "Steps": STEPS}
    report = SAN.obj(report)
    C.write_json(os.path.join(PKG, "dry-run-report.json"), report)
    md = ["# Ensayo en seco del paquete ccli-meas", "",
          "RunId `%s` · MANIFEST.json SHA-256 `%s` · %s · claude **no** se ejecutó · %d/%d pasos en Pass" % (
              report["RunId"], report["ManifestSha256"], report["At"], report["Passed"], report["Total"]), "",
          "| Paso | Resultado | Qué se ejercita | Esperado |", "|---|---|---|---|"]
    for s in report["Steps"]:
        md.append("| %s | %s | %s | %s |" % (s["Id"], "PASS" if s["Pass"] else "FAIL", s["Title"].replace("|", "/"), str(s["Expected"]).replace("|", "/")))
    md += ["", "El detalle observado de cada paso está en `dry-run-report.json` (saneado)."]
    with open(os.path.join(PKG, "dry-run-report.md"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(md) + "\n")
    print(json.dumps({"Passed": report["Passed"], "Total": report["Total"]}, ensure_ascii=False))
    return 0 if report["Passed"] == report["Total"] else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8", errors="backslashreplace")
    sys.exit(main())
