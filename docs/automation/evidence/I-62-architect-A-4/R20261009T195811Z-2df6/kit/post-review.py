"""I-62 A-4, correction 3 (docs/initiatives/I-62-A-4.md, blob 5e2ba68e, published at c2dbc225 over b25dde6b; receipt 24284a4a, decisions §63):
post-review audit v5.1, declared and self-tested BEFORE the launch (Coordinator disposition §63, point 6; decisions §57, point 3; Owner
CLAUDE-CLI-I62 = A). It audits ONE claude-cli run of the SECOND formal independent Architect re-review of A-4; the principal session runs it
afterwards, never before, and the auditor is not corrected after the run.

v5.1-a4r2 = the v5.1-a4r auditor of the first A-4 re-review (R20261009T150948Z-fdc2; ACCREDITED, decisions §63, point 1), adapted to the corrected
object and closure, with every rule kept: the reviewer has ONLY the Read, Grep and Glob tools (plus the runtime's StructuredOutput); effective paths
(`.`/`..` collapsed, realpath, so a junction that leaves the clone is OUTSIDE); read fidelity line by line against the commit; failed reads never
credited; premises found (the quote inside the cited lines, no tolerance) AND delivered faithfully; prompt.md never a premise; run custody; clone
clean after the run; the transport gate D-01 re-evaluated on the evidence copied by launch.py; preflight.json and run.json audited (measured
executable, hashes of stdout, transcript and prompt). Adaptation to the second re-review only: the run premise files are order.txt, order-s62.txt,
order-s61.txt, delta.diff, A-4.0d954376.md and A-4.7d863219.md; the coverage is Q-A4-01..Q-A4-22 and the eight Focus topics of the kit order,
each with an explicit disposition linked to findings like the questions; the prior-finding rules now cover two REQUIRED priors (A62-A4-01, to
confirm it stays CLOSED, and A62-A4-02: CLOSED | STILL_OPEN with at least one premise each; a STILL_OPEN linked to a REQUIRED) and two OPTIONAL
priors (A62-A4-O9, A62-A4-O10: APPLIED | NOT_APPLIED | N/A); a prior id reused only to restate a STILL_OPEN / NOT_APPLIED; DeltaConfirmation (A4-1,
A4-2, A4-3 and A4-5 identical to 0d954376, A4-4 introduction and rules 2-4 identical); two separate verdicts, A45Verdict and A46Verdict, with the
same rules; OwnerConsumptionLines adds EffectiveOnlyAfterAgreement; and AGREED requires both REQUIRED priors CLOSED, DeltaConfirmation all
CONFIRMED, OwnerConsumptionLines = ONLY_CONSUMPTION with EffectiveOnlyAfterAgreement = YES, and A45Verdict and A46Verdict in AGREED or EXCLUDE.
Defensive handling (no behavior change on the measured formats): a stream-json or transcript entry whose `message` is not an object (e.g., a
system/permission_denied entry with a string message), a JSON line that is not an object, or a permission denial that is not an object never
crashes the audit. Small seams (list_dir, clone_git, clone_reparse, validate_schema, GATE, LAUNCH_DIR) let the self-test simulate a tampered run
dir, launch dir or clone without touching them; the real audit never changes them.

Checks (each failure is a reason; ACCREDITED only with 0 reasons):
  * launch: stdout.jsonl in <run>/launch; run.json there: exit code 0, not killed, not timed out, session id, executable = the closure binary
    (measured resolved path and SHA-256), argument list = closure flags + --add-dir + --session-id + compact schema, cwd = clone, PromptSha256 =
    run/prompt.md, StdoutSha256 = the audited stdout, Transcript.Sha256 = the audited transcript; preflight.json there: the ten checks present and
    Ok, session id, measured binary = closure (path and SHA-256), version = 2.1.293 (Claude Code), loggedIn = true, gate = kit/transport-gate.json;
  * transport gate (D-01): transport_gate.evaluate(kit/transport-gate.json, closure, launch copies) without problems;
  * stream-json: init (session id, cwd = clone, model, permissionMode, tools = {Glob, Grep, Read, StructuredOutput}, no MCP, version) and a result
    message (success, not is_error, session id, structured_output present, permission_denials empty, modelUsage only the requested model, no
    subagents);
  * transcript: one session id (= the fixed one, = file name), cwd = clone, version, no sidechain entries; the first user message is byte-equal
    to run/prompt.md and there is no other user prompt (a compaction summary counts as one); model and effort observed on EVERY assistant message
    (claude-opus-5-5, xhigh);
  * tools used ⊆ {Read, Grep, Glob, StructuredOutput}; every Read/Grep path is a closure file or a run file; Grep without path, on a directory,
    or with glob/type is a violation; a Glob is a violation when its base leaves the clone and the run dir or when its scope (computed over the
    files on disk) or its returned paths contain any non-closure file; any permission denial (transcript or result) is a violation;
  * read fidelity of every Read; premises (PremiseRefs in prior-finding dispositions, findings, Focus topics and question dispositions) found in
    the cited lines and delivered faithfully by Read, only from closure files or the run files order.txt, order-s62.txt, order-s61.txt,
    delta.diff, A-4.0d954376.md and A-4.7d863219.md;
  * structured output: the result's structured_output equals the input of the last successful StructuredOutput call (and its attachment), is
    valid against result.schema.json and coherent (coverage of the four prior findings, Q-A4-01..22 and the eight Focus topics; DeltaConfirmation;
    A45Verdict and A46Verdict; AGREED / CHANGES REQUIRED / BLOCKED / NOT_ACCREDITED rules; reviewed identity; ids);
  * run custody: each run file = kit copy = closure RunFileHashes; the run dir holds only the run files and launch/; launch/ holds only the files
    launch.py writes;
  * clone after the run: HEAD = the commit, only main, no remote, clean with ignored files, designated blobs unchanged, no reparse points.

Usage: python post-review.py <transcript.jsonl> <stdout.jsonl> <clone> <run dir> <kit dir> <result.schema.json> <audit.json> [<output.json>]
(run.json, preflight.json and the gate copies are read from <run dir>/launch; the optional output.json receives the result's structured_output,
UTF-8, indent 1.)
"""
import hashlib
import importlib.util
import json
import os
import posixpath
import re
import stat
import subprocess
import sys
import tempfile

AUDITOR_VERSION = "v5.1-a4r2 (claude-cli; Read/Grep/Glob only; transport gate D-01; declared and self-tested before the launch; disposition §63, point 6)"
REPLACEMENT_CHAR = chr(0xFFFD)   # U+FFFD in a tool result: a transport artifact, recorded


def setup(clone, run, kit):
    global CLONE, RUN, KIT, CLOSURE, REV, IDS, ALLOWED_LC, RUN_FILES, RUN_FILES_LC, PREMISE_RUN, LONG_LINES, OBJECT, OBJECT_BLOB, QUESTION_IDS
    global FOCUS_TOPICS, TRANSPORT, SESSION_ID, CLONE_REAL, RUN_REAL, ALLOWED_TOOLS, DESIGNATED, TG, GATE, LAUNCH_DIR, LAUNCH_FILES
    global PRIOR_IDS, DELTA_KEYS
    CLONE, RUN, KIT = clone, run, kit
    CLOSURE = json.load(open(os.path.join(kit, "closure.json"), encoding="utf-8"))
    assert norm(CLOSURE["RunDir"]).lower() == norm(run).lower(), "run dir distinto del RunDir del cierre"
    REV, IDS = CLOSURE["AuthorityRevision"], CLOSURE["Ids"]
    ALLOWED_LC = {r["Path"].lower(): r["Path"] for r in CLOSURE["CanonicalInputs"] + CLOSURE["AllowedTransitiveInputs"]}
    DESIGNATED = {r["Path"]: r["Blob"] for r in CLOSURE["CanonicalInputs"]}
    RUN_FILES = list(CLOSURE["RunFiles"])
    RUN_FILES_LC = {n.lower(): n for n in RUN_FILES}
    PREMISE_RUN = set(CLOSURE["PremiseRunFiles"])
    LONG_LINES = {p: set(v) for p, v in CLOSURE["ReadPolicy"]["LongLines"].items()}
    OBJECT, OBJECT_BLOB = CLOSURE["ObjectPath"], CLOSURE["ObjectBlob"]
    QUESTION_IDS, FOCUS_TOPICS = list(CLOSURE["QuestionIds"]), list(CLOSURE["FocusTopics"])
    PRIOR_IDS, DELTA_KEYS = list(CLOSURE["PriorFindingIds"]), list(CLOSURE["DeltaConfirmationKeys"])
    TRANSPORT, SESSION_ID = CLOSURE["Transport"], CLOSURE["Transport"]["SessionId"]
    ALLOWED_TOOLS = set(CLOSURE["AllowedTools"])
    CLONE_REAL, RUN_REAL = real(CLONE), real(RUN)
    spec = importlib.util.spec_from_file_location("transport_gate", os.path.join(kit, "transport_gate.py"))
    TG = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(TG)
    GATE = json.load(open(os.path.join(kit, TG.GATE_FILE), encoding="utf-8"))
    LAUNCH_DIR = os.path.join(run, "launch")
    LAUNCH_FILES = {"stdout.jsonl", "stderr.txt", "run.json", "preflight.json", TG.COPY_CHARACTERIZATION, TG.COPY_ACCEPTANCE}


# ------------------------------------------------------------------ seams (the self-test replaces them; the real audit never does)
def list_dir(p):
    return sorted(os.listdir(p)) if os.path.isdir(p) else None


def clone_git(*a):
    return subprocess.run(["git", "-C", CLONE] + list(a), capture_output=True, text=True).stdout


def clone_reparse():
    out = []
    for r, ds, fs in os.walk(CLONE, followlinks=False):
        for n in ds + fs:
            st = os.lstat(os.path.join(r, n))
            if stat.S_ISLNK(st.st_mode) or (getattr(st, "st_file_attributes", 0) & 0x400):
                out.append(userless(os.path.join(r, n)))
    return out


def validate_schema(result, schema):
    """Test-Json (PowerShell 7) against the declared schema → (valid, detail)."""
    tmp = tempfile.mkdtemp()
    rp = os.path.join(tmp, "result.json")
    with open(rp, "w", encoding="utf-8") as fh:
        json.dump(result, fh, ensure_ascii=False)
    ps = "try { $null = Get-Content -Raw -LiteralPath '%s' | Test-Json -SchemaFile '%s' -ErrorAction Stop; 'True' } catch { 'False: ' + $_.Exception.Message }" % (rp, schema)
    o = subprocess.run(["pwsh", "-NoProfile", "-Command", ps], capture_output=True, text=True, encoding="utf-8", errors="replace").stdout.strip()
    os.remove(rp)
    os.rmdir(tmp)
    return o == "True", o[:400]


# ------------------------------------------------------------------ paths
def norm(p):
    p = str(p).strip().strip('"').strip("'").replace("\\", "/")
    p = re.sub(r"^//[?.]/(?:unc/)?", "", p, flags=re.I)
    m = re.match(r"^/([a-zA-Z])/(.*)$", p) or re.match(r"^/([a-zA-Z])$", p)
    if m:
        p = m.group(1) + ":/" + (m.group(2) if m.lastindex and m.lastindex >= 2 else "")
    return re.sub(r"/+", "/", p).rstrip("/") or "/"


def real(p):
    """Effective path: `.`/`..` collapsed; symbolic links and junctions resolved for the part that exists; lower case."""
    q = os.path.normpath(norm(p))
    try:
        q = os.path.realpath(q)
    except OSError:
        pass
    return norm(q).lower()


def under(p, root):
    return p == root or p.startswith(root.rstrip("/") + "/")


def absolutize(p, cwd):
    raw = norm(os.path.expanduser(p) if str(p).startswith("~") else p)
    if not re.match(r"^[a-zA-Z]:/", raw) and not raw.startswith("/") and cwd:
        raw = norm(cwd) + "/" + raw
    return raw


def classify(p, cwd=None):
    """→ (kind, ref): CLOSURE rel | RUN name | RUNDIR | ROOT | DIR (directory inside the clone) | OUTSIDE raw. Only the effective destination counts."""
    raw = absolutize(p, cwd)
    eff = real(raw)
    if eff == RUN_REAL:
        return ("RUNDIR", raw)
    if under(eff, RUN_REAL):
        name = eff[len(RUN_REAL) + 1:]
        return ("RUN", RUN_FILES_LC[name]) if name in RUN_FILES_LC else ("OUTSIDE", raw)
    if eff == CLONE_REAL:
        return ("ROOT", raw)
    if under(eff, CLONE_REAL):
        rel = eff[len(CLONE_REAL) + 1:]
        if rel in ALLOWED_LC:
            return ("CLOSURE", ALLOWED_LC[rel])
        if os.path.isdir(eff):
            return ("DIR", raw)
        return ("OUTSIDE", raw)
    return ("OUTSIDE", raw)


def userless(p):
    s = str(p).replace("\\", "/")
    home = os.path.expanduser("~").replace("\\", "/")
    s = re.sub(re.escape(home), "%USERPROFILE%", s, flags=re.I)
    return re.sub(r"^[A-Za-z]:/Users/[^/]+", "%USERPROFILE%", s)


def same_binary_path(a, b):
    return norm(TG.expand(str(a or ""))).lower() == norm(TG.expand(str(b or ""))).lower()


# ------------------------------------------------------------------ canonical text
canon_cache = {}


def canon(rel):
    if rel not in canon_cache:
        canon_cache[rel] = subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel]).decode("utf-8").replace("\r\n", "\n").split("\n")
    return canon_cache[rel]


def read_run_bytes(name):
    """Bytes of a run file (the self-test replaces this function to simulate tampering without touching the run)."""
    return open(os.path.join(RUN, name), "rb").read()


def run_file(name):
    return read_run_bytes(name).decode("utf-8").replace("\r\n", "\n").split("\n")


def sha(b):
    return hashlib.sha256(b).hexdigest()


def read_bytes_or_none(p):
    try:
        return open(p, "rb").read()
    except OSError:
        return None


def read_json_or_none(p):
    b = read_bytes_or_none(p)
    if b is None:
        return None
    try:
        return json.loads(b.decode("utf-8"))
    except ValueError:
        return {"__unparsed__": True}


# ------------------------------------------------------------------ Glob scope
def expand_braces(p):
    m = re.search(r"\{([^{}]*)\}", p)
    if not m:
        return [p]
    out = []
    for alt in m.group(1).split(","):
        out += expand_braces(p[:m.start()] + alt + p[m.end():])
    return out


def glob_to_regex(p):
    out, i = "", 0
    while i < len(p):
        if p.startswith("**/", i):
            out += "(?:[^/]*/)*"
            i += 3
            continue
        if p.startswith("**", i):
            out += ".*"
            i += 2
            continue
        c = p[i]
        if c == "*":
            out += "[^/]*"
        elif c == "?":
            out += "[^/]"
        elif c == "[":
            j = p.find("]", i + 2)
            if j < 0:
                out += re.escape(c)
            else:
                body = p[i + 1:j]
                body = ("^" + body[1:]) if body.startswith("!") else body
                out += "[" + body.replace("\\", "\\\\") + "]"
                i = j
        else:
            out += re.escape(c)
        i += 1
    return re.compile("^" + out + "$", re.I)


universe_cache = {}


def files_under(root):
    """Every file on disk under `root` (no link is followed); the auditor's universe for a Glob scope."""
    if root not in universe_cache:
        acc = []
        for r, ds, fs in os.walk(root, followlinks=False):
            acc += [norm(os.path.join(r, f)).lower() for f in fs]
            for d in ds:
                p = os.path.join(r, d)
                st = os.lstat(p)
                if stat.S_ISLNK(st.st_mode) or (getattr(st, "st_file_attributes", 0) & 0x400):
                    acc.append(norm(p).lower())          # a link is listed as a leaf: never followed, never closure
        universe_cache[root] = acc
    return universe_cache[root]


def audit_glob(inp, cwd, out):
    """→ list of problems. The scope is evaluated on disk (conservative superset of what the tool could list) and on the returned paths."""
    probs = []
    pattern = str(inp.get("pattern", ""))
    base = absolutize(inp.get("path") or cwd or "", cwd)
    for pat in expand_braces(pattern.replace("\\", "/")):
        full = norm(pat) if re.match(r"^[a-zA-Z]:/|^/", pat) else base + "/" + pat
        full = norm(full)
        segs = full.split("/")
        k = next((i for i, s in enumerate(segs) if re.search(r"[*?\[]", s)), len(segs))
        prefix, rest = "/".join(segs[:k]), segs[k:]
        if ".." in rest:
            probs.append("Glob con `..` después de un comodín: %s" % pattern)
            continue
        scope_root = real(posixpath.normpath(prefix)) if prefix else ""
        prefix_eff = scope_root
        if not scope_root or not (under(scope_root, CLONE_REAL) or under(scope_root, RUN_REAL)):
            probs.append("Glob fuera del clon y del run: %s (base %s)" % (pattern, userless(base)))
            continue
        rx = glob_to_regex("/".join([prefix_eff] + rest) if rest else prefix_eff)
        root_dir = scope_root if os.path.isdir(scope_root) else os.path.dirname(scope_root)
        hits = [f for f in files_under(root_dir) if rx.match(f)]
        bad = [f for f in hits if classify(f)[0] not in ("CLOSURE", "RUN")]
        if bad:
            probs.append("Glob alcanza %d archivo(s) fuera del cierre: %s (p. ej., %s)" % (len(bad), pattern, ", ".join(userless(b) for b in bad[:3])))
    for line in (out or "").replace("\r\n", "\n").split("\n"):
        t = line.strip()
        if not t or t.lower().startswith("no files found") or t.startswith("(") or t.lower().startswith("found "):
            continue
        if re.match(r"^[a-zA-Z]:[\\/]|^/|^\.{0,2}[\\/]|^[\w.-]+[\\/]", t):
            if classify(t, cwd)[0] not in ("CLOSURE", "RUN"):
                probs.append("Glob devolvió una ruta fuera del cierre: %s" % userless(t))
    return probs


# ------------------------------------------------------------------ helpers for the JSON streams
def load_jsonl(path):
    out = []
    for line in open(path, encoding="utf-8"):
        line = line.strip()
        if not line:
            continue
        try:
            o = json.loads(line)
        except ValueError:
            o = {"__unparsed__": line[:200]}
        out.append(o if isinstance(o, dict) else {"__non_object__": line[:200]})
    return out


def text_of(content):
    if isinstance(content, str):
        return content
    return "".join(x.get("text", "") for x in (content or []) if isinstance(x, dict) and x.get("type") in (None, "text"))


def expected_args(schema_path):
    compact = json.dumps(json.load(open(schema_path, encoding="utf-8")), separators=(",", ":"), ensure_ascii=True)
    return list(TRANSPORT["Flags"]) + list(TRANSPORT["AddDir"]) + ["--session-id", SESSION_ID, "--json-schema", compact]


DENIAL_RX = re.compile(r"(?i)permission|haven't granted|has been denied|was denied|not allowed to")


# ------------------------------------------------------------------ main audit
def audit(transcript, stdout_path, schema):
    reasons = []
    add = reasons.append
    entries = load_jsonl(transcript)
    stream = load_jsonl(stdout_path)
    transcript_bytes, stdout_bytes = open(transcript, "rb").read(), open(stdout_path, "rb").read()
    prompt_bytes = read_run_bytes("prompt.md")
    prompt_text = prompt_bytes.decode("utf-8")

    # ---------------- launch (run.json and preflight.json in <run>/launch)
    run_json_path, preflight_path = os.path.join(LAUNCH_DIR, "run.json"), os.path.join(LAUNCH_DIR, "preflight.json")
    run_json, preflight = read_json_or_none(run_json_path), read_json_or_none(preflight_path)
    launch = {"LaunchDir": userless(LAUNCH_DIR), "StdoutInLaunchDir": real(os.path.dirname(os.path.abspath(stdout_path))) == real(LAUNCH_DIR),
              "RunJsonPresent": run_json is not None, "PreflightPresent": preflight is not None}
    if not launch["StdoutInLaunchDir"]:
        add("lanzamiento: stdout.jsonl fuera de %s" % userless(LAUNCH_DIR))
    if run_json is None:
        add("lanzamiento: falta run.json en launch/")
    else:
        launch.update({k: run_json.get(k) for k in ("Pid", "Start", "End", "ExitCode", "Killed", "TimedOut", "SessionId", "Executable",
                                                    "ExecutableSha256", "PromptSha256", "StdoutSha256")})
        if run_json.get("ExitCode") != 0:
            add("lanzamiento: código de salida %r (se exige 0)" % run_json.get("ExitCode"))
        if run_json.get("Killed") or run_json.get("TimedOut"):
            add("lanzamiento: proceso terminado por el invocador (Killed/TimedOut)")
        if run_json.get("SessionId") != SESSION_ID:
            add("lanzamiento: SessionId de run.json distinto del fijado en el kit")
        if not same_binary_path(run_json.get("Executable"), TRANSPORT["Binary"]["Path"]) or \
                run_json.get("ExecutableSha256") != TRANSPORT["Binary"]["Sha256"]:
            add("lanzamiento: ejecutable medido distinto del binario elegible del cierre (ruta resuelta o SHA-256)")
        launch["ArgsMatch"] = run_json.get("Args") == expected_args(schema)
        if not launch["ArgsMatch"]:
            add("lanzamiento: argumentos del lanzamiento distintos de los del cierre (flags, --add-dir, --session-id o esquema)")
        launch["Cwd"] = run_json.get("Cwd")
        if real(run_json.get("Cwd", "")) != CLONE_REAL:
            add("lanzamiento: cwd del proceso distinto del clon")
        if run_json.get("PromptSha256") != sha(prompt_bytes):
            add("lanzamiento: PromptSha256 de run.json distinto de prompt.md del run")
        if run_json.get("StdoutSha256") != sha(stdout_bytes):
            add("lanzamiento: StdoutSha256 de run.json distinto del stdout.jsonl auditado")
        if (run_json.get("Transcript") or {}).get("Sha256") != sha(transcript_bytes):
            add("lanzamiento: Transcript.Sha256 de run.json distinto de la transcripción auditada")
    if preflight is None:
        add("lanzamiento: falta preflight.json en launch/")
    else:
        checks = preflight.get("Checks") or {}
        launch["PreflightChecks"] = {k: (v or {}).get("Ok") for k, v in checks.items()}
        missing = [k for k in TG.PREFLIGHT_CHECKS if k not in checks]
        if missing:
            add("preflight: faltan comprobaciones %s" % missing)
        not_ok = [k for k, v in checks.items() if (v or {}).get("Ok") is not True]
        if not_ok:
            add("preflight: comprobaciones sin Ok: %s" % not_ok)
        if preflight.get("SessionId") != SESSION_ID:
            add("preflight: SessionId distinto del fijado en el kit")
        b = checks.get("3-Binary") or {}
        if b.get("Sha256") != TRANSPORT["Binary"]["Sha256"] or not same_binary_path(b.get("Path"), TRANSPORT["Binary"]["Path"]):
            add("preflight: binario medido (ruta resuelta o SHA-256) distinto del cierre")
        if (checks.get("4-Version") or {}).get("Got") != TRANSPORT["Binary"]["Version"]:
            add("preflight: versión medida distinta de %s" % TRANSPORT["Binary"]["Version"])
        if (checks.get("5-Auth") or {}).get("loggedIn") is not True:
            add("preflight: auth status sin loggedIn = true")
        g = checks.get("10-TransportGate") or {}
        if g.get("Status") != GATE.get("Status") or g.get("CharacterizationSha256") != (GATE.get("Characterization") or {}).get("Sha256") or \
                g.get("AcceptanceSha256") != (GATE.get("Acceptance") or {}).get("Sha256"):
            add("preflight: compuerta de transporte distinta de kit/transport-gate.json")

    # ---------------- transport gate (D-01), re-evaluated on the copies launch.py wrote
    char_b = read_bytes_or_none(os.path.join(LAUNCH_DIR, TG.COPY_CHARACTERIZATION))
    acc_b = read_bytes_or_none(os.path.join(LAUNCH_DIR, TG.COPY_ACCEPTANCE))
    gate_eval = TG.evaluate(GATE, CLOSURE, char_b, acc_b)
    for p in gate_eval["Problems"]:
        add("compuerta de transporte: " + p)

    # ---------------- stream-json (stdout)
    init = next((o for o in stream if o.get("type") == "system" and o.get("subtype") == "init"), None)
    results_msgs = [o for o in stream if o.get("type") == "result"]
    res = results_msgs[-1] if results_msgs else None
    stream_check = {"InitPresent": init is not None, "ResultPresent": res is not None}
    exp_init = TRANSPORT["ExpectedInit"]
    if init is None:
        add("stream-json: falta el mensaje init")
    else:
        stream_check["Init"] = {k: init.get(k) for k in ("session_id", "cwd", "model", "permissionMode", "tools", "mcp_servers", "claude_code_version",
                                                       "apiKeySource")}
        if init.get("session_id") != SESSION_ID:
            add("stream-json: session_id del init distinto del fijado")
        if real(init.get("cwd", "")) != CLONE_REAL:
            add("stream-json: cwd del init distinto del clon")
        if init.get("model") != TRANSPORT["Model"]:
            add("stream-json: modelo del init distinto de %s" % TRANSPORT["Model"])
        if init.get("permissionMode") != exp_init["permissionMode"]:
            add("stream-json: permissionMode del init distinto de %s" % exp_init["permissionMode"])
        if sorted(init.get("tools") or []) != sorted(exp_init["tools"]):
            add("stream-json: herramientas del init distintas de %s: %s" % (exp_init["tools"], init.get("tools")))
        if (init.get("mcp_servers") or []) != exp_init["mcp_servers"]:
            add("stream-json: servidores MCP en el init")
        if init.get("claude_code_version") != exp_init["claude_code_version"]:
            add("stream-json: versión del init distinta de %s" % exp_init["claude_code_version"])
    if res is None:
        add("stream-json: sin mensaje result (terminación sin resultado)")
    else:
        stream_check["Result"] = {k: res.get(k) for k in ("subtype", "is_error", "terminal_reason", "num_turns", "session_id", "permission_denials",
                                                         "total_cost_usd")}
        stream_check["Result"]["modelUsage"] = sorted((res.get("modelUsage") or {}).keys())
        stream_check["Result"]["subagent_spawned"] = (res.get("subagent_stats") or {}).get("spawned")
        if res.get("subtype") != "success" or res.get("is_error"):
            add("stream-json: el result no es success (subtype %r, is_error %r)" % (res.get("subtype"), res.get("is_error")))
        if res.get("session_id") != SESSION_ID:
            add("stream-json: session_id del result distinto del fijado")
        for d in res.get("permission_denials") or []:
            d = d if isinstance(d, dict) else {"tool_name": str(d)[:80]}
            ti = d.get("tool_input") or {}
            add("permission_denials: %s %s" % (d.get("tool_name"), userless("; ".join("%s=%s" % (k, v) for k, v in ti.items())[:160] if isinstance(ti, dict) else ti)))
        if set((res.get("modelUsage") or {}).keys()) != {TRANSPORT["Model"]}:
            add("stream-json: modelUsage con modelos distintos de %s: %s" % (TRANSPORT["Model"], sorted((res.get("modelUsage") or {}).keys())))
        if ((res.get("subagent_stats") or {}).get("spawned") or 0) > 0:
            add("stream-json: subagentes lanzados")
        if "structured_output" not in res or res.get("structured_output") is None:
            add("stream-json: el result no trae structured_output")
    for o in stream:
        if o.get("type") == "assistant":
            m = o.get("message") if isinstance(o.get("message"), dict) else {}
            if m.get("model") != TRANSPORT["Model"]:
                add("stream-json: mensaje del asistente con modelo %r" % m.get("model"))
                break
    if any(o.get("type") in ("assistant", "user") and o.get("parent_tool_use_id") for o in stream):
        add("stream-json: mensajes de un subagente (parent_tool_use_id)")

    # ---------------- transcript
    ident = {"SessionIds": set(), "Cwds": set(), "Versions": set(), "Entrypoints": set(), "Models": {}, "Efforts": {}, "SidechainEntries": 0,
             "TranscriptFileStem": os.path.splitext(os.path.basename(transcript))[0]}
    user_prompts, meta_users, uses, results, errors, cwd_of, order, so_calls, so_attachments = [], 0, {}, {}, {}, {}, [], [], []
    for o in entries:
        for k, key in (("SessionIds", "sessionId"), ("Cwds", "cwd"), ("Versions", "version"), ("Entrypoints", "entrypoint")):
            if o.get(key):
                ident[k].add(o[key])
        if o.get("isSidechain"):
            ident["SidechainEntries"] += 1
        msg = o.get("message") if isinstance(o.get("message"), dict) else {}
        if o.get("type") == "assistant":
            m, e = msg.get("model"), o.get("effort")
            ident["Models"][str(m)] = ident["Models"].get(str(m), 0) + 1
            ident["Efforts"][str(e)] = ident["Efforts"].get(str(e), 0) + 1
        if o.get("type") == "attachment" and (o.get("attachment") or {}).get("type") == "structured_output":
            so_attachments.append((o["attachment"] or {}).get("data"))
        content = msg.get("content")
        if o.get("type") == "user" and not o.get("isSidechain"):
            has_result = isinstance(content, list) and any(isinstance(x, dict) and x.get("type") == "tool_result" for x in content)
            if not has_result:
                if o.get("isMeta") or o.get("isCompactSummary"):
                    meta_users += 1
                    if o.get("isCompactSummary"):
                        user_prompts.append(("COMPACT_SUMMARY", text_of(content)))
                else:
                    user_prompts.append(("PROMPT", text_of(content)))
        if not isinstance(content, list):
            continue
        for part in content:
            if not isinstance(part, dict):
                continue
            if part.get("type") == "tool_use":
                uses[part["id"]] = {"name": part.get("name"), "input": part.get("input") or {}}
                cwd_of[part["id"]] = o.get("cwd")
                order.append(part["id"])
            elif part.get("type") == "tool_result":
                c = part.get("content")
                results[part.get("tool_use_id")] = c if isinstance(c, str) else "".join(x.get("text", "") for x in (c or []) if isinstance(x, dict))
                errors[part.get("tool_use_id")] = bool(part.get("is_error"))
    if ident["SessionIds"] != {SESSION_ID} or ident["TranscriptFileStem"] != SESSION_ID:
        add("transcripción: sesión distinta de la fijada en el kit (%s; archivo %s)" % (sorted(ident["SessionIds"]), ident["TranscriptFileStem"]))
    if any(real(c) != CLONE_REAL for c in ident["Cwds"]) or not ident["Cwds"]:
        add("transcripción: cwd distinto del clon: %s" % ", ".join(sorted(userless(c) for c in ident["Cwds"])))
    if ident["Versions"] != {exp_init["claude_code_version"]}:
        add("transcripción: versión distinta de %s: %s" % (exp_init["claude_code_version"], sorted(ident["Versions"])))
    if ident["SidechainEntries"]:
        add("transcripción: %d entradas de cadena lateral (subagentes)" % ident["SidechainEntries"])
    bad_models = {m: n for m, n in ident["Models"].items() if m != TRANSPORT["Model"]}
    bad_efforts = {e: n for e, n in ident["Efforts"].items() if e != TRANSPORT["Effort"]}
    if not ident["Models"]:
        add("transcripción: ningún mensaje del asistente")
    if bad_models:
        add("transcripción: modelo observado distinto de %s en %s mensajes del asistente: %s" % (TRANSPORT["Model"], sum(bad_models.values()), bad_models))
    if bad_efforts:
        add("transcripción: effort observado distinto de %s en %s mensajes del asistente: %s" % (TRANSPORT["Effort"], sum(bad_efforts.values()), bad_efforts))
    prompt_check = {"PromptMdSha256": sha(prompt_bytes), "UserPrompts": len(user_prompts), "MetaUserEntries": meta_users}
    if not user_prompts:
        add("transcripción: sin primer mensaje de usuario")
    else:
        first = user_prompts[0][1]
        prompt_check["FirstUserMessageSha256"] = sha(first.encode("utf-8"))
        prompt_check["Equal"] = first == prompt_text
        prompt_check["EqualAfterStrip"] = first.strip() == prompt_text.strip()
        if not prompt_check["Equal"]:
            add("fidelidad del prompt: el primer mensaje de usuario es distinto de prompt.md")
        if len(user_prompts) > 1:
            add("transcripción: %d mensajes de usuario además del prompt (otro prompt o un resumen de compactación)" % (len(user_prompts) - 1))

    # ---------------- tool calls
    rows, fidelity, delivered, failed_reads, truncated_known, grep_info, unicode_anomalies = [], [], {}, [], [], [], []

    def compare(ref, kind, numbered):
        lines = canon(ref) if kind == "CLOSURE" else run_file(ref)
        bad = []
        for n, body in numbered:
            if 1 <= n <= len(lines) and lines[n - 1] == body:
                delivered.setdefault(ref, set()).add(n)
            elif kind == "CLOSURE" and n in LONG_LINES.get(ref, set()) and len(body) >= 1000 and \
                    lines[n - 1].startswith(body.rstrip().rstrip("…").rstrip(".").rstrip()):
                truncated_known.append({"Path": ref, "Line": n, "DeliveredChars": len(body), "CanonicalChars": len(lines[n - 1])})
            else:
                bad.append(n)
        return bad

    so_ok_inputs = []
    for idx, uid in enumerate(order):
        u = uses[uid]
        name, inp, out, cwd = u["name"], u["input"], results.get(uid), cwd_of.get(uid)
        failed = errors.get(uid, False) or not (out or "").strip()
        row = {"Index": idx, "Tool": name, "Input": {k: (v if len(str(v)) < 300 else str(v)[:300] + "…") for k, v in inp.items()},
               "Class": None, "Failed": failed, "Problems": []}
        if name == "Read":
            kind, ref = classify(inp.get("file_path", ""), cwd)
            row["Class"] = kind
            if kind in ("ROOT", "DIR", "RUNDIR"):
                row["Problems"].append("lectura de un directorio: %s" % userless(ref))
            elif kind not in ("CLOSURE", "RUN"):
                row["Problems"].append("lectura fuera del cierre" + (" (intento fallido; sigue siendo una acción fuera del contrato)" if failed else "") +
                                       ": %s" % userless(ref))
            elif failed:
                failed_reads.append({"Index": idx, "Path": ref, "Effect": "lectura fallida: nada acreditado"})
            else:
                pairs = [(int(m.group(1)), m.group(2)) for m in (re.match(r"^\s*(\d+)\t(.*)$", raw) for raw in out.replace("\r\n", "\n").split("\n")) if m]
                bad = compare(ref, kind, pairs)
                fidelity.append({"Index": idx, "Path": ref, "Kind": kind, "Offset": inp.get("offset"), "Limit": inp.get("limit"), "Lines": len(pairs),
                                 "Altered": bad[:50], "Status": "FAITHFUL_NORMALIZED" if pairs and not bad else ("DEGRADED" if bad else "EMPTY")})
        elif name == "Grep":
            p = inp.get("path")
            kind, ref = classify(p, cwd) if p else ("NOPATH", "(directorio de trabajo)")
            row["Class"] = kind
            if kind == "NOPATH":
                row["Problems"].append("Grep sin path (busca en el directorio de trabajo, que es un directorio)")
            elif kind in ("ROOT", "DIR", "RUNDIR"):
                row["Problems"].append("Grep sobre un directorio: %s" % userless(ref))
            elif kind not in ("CLOSURE", "RUN"):
                row["Problems"].append("Grep fuera del cierre: %s" % userless(ref))
            if inp.get("glob") or inp.get("type"):
                row["Problems"].append("Grep con glob o type")
            if kind in ("CLOSURE", "RUN") and out and not failed:
                lines = canon(ref) if kind == "CLOSURE" else run_file(ref)
                for raw in out.replace("\r\n", "\n").split("\n"):
                    m = re.match(r"^(?:.*?:)?(\d+)[:-](.*)$", raw)
                    if m:
                        n, body = int(m.group(1)), m.group(2)
                        grep_info.append({"Path": ref, "Line": n, "Exact": 1 <= n <= len(lines) and lines[n - 1] == body})
        elif name == "Glob":
            row["Class"] = "GLOB"
            row["Problems"] += audit_glob(inp, cwd, out)
        elif name == "StructuredOutput":
            row["Class"] = "RESULT"
            if not failed:
                so_ok_inputs.append(inp)
        else:
            row["Class"] = "FORBIDDEN_TOOL"
            row["Problems"].append("herramienta no permitida (%s)" % name)
        if failed and out and DENIAL_RX.search(out):
            row["Problems"].append("permiso denegado: %s" % userless(out.strip()[:160]))
        if out and REPLACEMENT_CHAR in out:
            row["ReplacementChars"] = out.count(REPLACEMENT_CHAR)
            unicode_anomalies.append({"Index": idx, "Tool": name, "Count": out.count(REPLACEMENT_CHAR)})
        if uid not in results:
            row["Problems"].append("llamada sin resultado (terminación incompleta)")
        rows.append(row)
        for p in row["Problems"]:
            add("llamada %d (%s): %s" % (idx, name, p))
    for f in fidelity:
        if f["Status"] == "DEGRADED":
            add("fidelidad: %s (Read, llamada %d) líneas alteradas %s" % (f["Path"], f["Index"], f["Altered"][:10]))

    # ---------------- run custody
    custody = {}
    for n in RUN_FILES:
        try:
            run_sha = sha(read_run_bytes(n))
        except OSError:
            run_sha = None
        kp = os.path.join(KIT, n)
        kit_sha = sha(open(kp, "rb").read()) if os.path.isfile(kp) else None
        clo_sha = (CLOSURE["RunFileHashes"].get(n) or {}).get("Sha256")
        custody[n] = {"RunSha256": run_sha, "KitCopySha256": kit_sha, "ClosureSha256": clo_sha, "Match": run_sha is not None and run_sha == kit_sha == clo_sha}
        if not custody[n]["Match"]:
            add("archivo del run distinto del custodiado (copia del kit y RunFileHashes del cierre): %s" % n)
    listing = list_dir(RUN) or []
    extra = [x for x in listing if x not in RUN_FILES and x != "launch"]
    if extra:
        add("directorio del run con entradas ajenas al run: %s" % extra)
    launch_listing = list_dir(LAUNCH_DIR)
    unexpected = [x for x in (launch_listing or []) if x not in LAUNCH_FILES]
    if unexpected:
        add("launch/ con archivos inesperados: %s" % unexpected)
    custody_doc = {"Files": custody, "RunDirListing": listing, "LaunchDirListing": launch_listing}

    # ---------------- clone after the run
    g = clone_git
    clone_after = {"Head": g("rev-parse", "HEAD").strip(), "StatusPorcelainIgnored": g("status", "--porcelain", "--ignored"),
                   "Branches": g("branch", "--list", "--format=%(refname:short)").split(), "Remotes": g("remote").split()}
    blobs_now = {p: g("rev-parse", "HEAD:" + p).strip() for p in DESIGNATED}
    clone_after["DesignatedBlobsUnchanged"] = all(blobs_now[p] == DESIGNATED[p] for p in DESIGNATED)
    reparse = clone_reparse()
    clone_after["ReparsePoints"] = reparse
    if not (clone_after["Head"] == REV and clone_after["StatusPorcelainIgnored"].strip() == "" and clone_after["Branches"] == ["main"] and
            clone_after["Remotes"] == []):
        add("el clon no quedó limpio, cambió de HEAD, de ramas o de remotos")
    if not clone_after["DesignatedBlobsUnchanged"]:
        add("el clon no conserva los blobs designados del cierre")
    if reparse:
        add("el clon tiene enlaces o uniones después de la corrida: %s" % reparse[:3])

    # ---------------- structured output
    result = res.get("structured_output") if res else None
    rcheck = {"Found": result is not None, "SchemaValid": False, "Coherence": [], "StructuredOutputCalls": len(so_ok_inputs)}
    if result is not None:
        rcheck["ConsistentWithTranscript"] = bool(so_ok_inputs) and so_ok_inputs[-1] == result and all(a == result for a in so_attachments[-1:])
        if not rcheck["ConsistentWithTranscript"]:
            add("resultado: structured_output del result distinto de la última llamada StructuredOutput de la transcripción (o de su adjunto)")
        rcheck["SchemaValid"], rcheck["SchemaDetail"] = validate_schema(result, schema)
        c = rcheck["Coherence"]
        coherence(result, c)
        for x in c:
            add("resultado: " + x)
        if not rcheck["SchemaValid"]:
            add("resultado: no valida contra result.schema.json")
    else:
        add("no hay resultado estructurado")

    # ---------------- premises
    premise_check = []
    for key, idk in (("PriorFindingDispositions", "FindingId"), ("RequiredFindings", "FindingId"), ("OptionalFindings", "FindingId"),
                     ("Focus", "Topic"), ("QuestionDispositions", "Id")):
        for f in (result or {}).get(key) or []:
            for pr in f.get("PremiseRefs") or []:
                rel_in = pr.get("Path", "")
                kind, rel = classify(rel_in, CLONE)
                if kind == "OUTSIDE" and norm(rel_in).lower() in RUN_FILES_LC:
                    kind, rel = "RUN", RUN_FILES_LC[norm(rel_in).lower()]   # bare name: no closure file has a run-file name
                a, b = pr.get("LineStart", 1), pr.get("LineEnd", 0)
                found = delivered_ok = False
                if kind == "CLOSURE" or (kind == "RUN" and rel in PREMISE_RUN):
                    lines = canon(rel) if kind == "CLOSURE" else run_file(rel)
                    if 1 <= a <= b <= len(lines):
                        found = " ".join(str(pr.get("Quote", "")).split()) in " ".join(" ".join(lines[a - 1:b]).split())
                    delivered_ok = all(n in delivered.get(rel, set()) for n in range(a, b + 1))
                premise_check.append({"Group": key, "Finding": f.get(idk), "Path": rel if kind in ("CLOSURE", "RUN") else userless(rel), "Kind": kind,
                                      "Lines": [a, b], "QuoteFound": found, "DeliveredFaithfully": delivered_ok})
                if not found:
                    add("premisa no canónica en %s (%s %s-%s)" % (f.get(idk), rel if kind in ("CLOSURE", "RUN") else userless(rel), a, b))
                elif not delivered_ok:
                    add("premisa sobre líneas no entregadas fielmente en %s (%s %s-%s)" % (f.get(idk), rel, a, b))

    # ---------------- coverage (informational, not reasons)
    def complete(ref, n_lines):
        return all(n in delivered.get(ref, set()) for n in range(1, n_lines + 1))
    coverage = {n: complete(n, CLOSURE["RunFileHashes"][n]["Lines"]) for n in CLOSURE["PremiseRunFiles"]}
    for r in CLOSURE["CanonicalInputs"]:
        if r["Path"] in (OBJECT, CLOSURE["PackagePath"]):
            coverage[r["Path"]] = complete(r["Path"], r["Lines"])
    return {
        "AuditorVersion": AUDITOR_VERSION,
        "Inputs": {"Transcript": {"Path": userless(transcript), "Sha256": sha(transcript_bytes), "Entries": len(entries)},
                   "Stdout": {"Path": userless(stdout_path), "Sha256": sha(stdout_bytes), "Lines": len(stream)},
                   "RunJson": {"Path": userless(run_json_path), "Sha256": sha(read_bytes_or_none(run_json_path)) if run_json is not None else None},
                   "Preflight": {"Path": userless(preflight_path), "Sha256": sha(read_bytes_or_none(preflight_path)) if preflight is not None else None},
                   "TransportCopies": {TG.COPY_CHARACTERIZATION: sha(char_b) if char_b is not None else None,
                                       TG.COPY_ACCEPTANCE: sha(acc_b) if acc_b is not None else None},
                   "Gate": {"Path": userless(os.path.join(KIT, TG.GATE_FILE)), "Status": GATE.get("Status")},
                   "Schema": {"Path": userless(schema), "Sha256": sha(open(schema, "rb").read())}},
        "RuntimeIdentity": {k: (sorted(v) if isinstance(v, set) else v) for k, v in ident.items()},
        "Launch": launch, "TransportGate": gate_eval, "Stream": stream_check, "PromptFidelity": prompt_check, "RunCustody": custody_doc,
        "CloneAfterRun": clone_after,
        "ToolAudit": {"Calls": len(rows), "WithProblems": [r for r in rows if r["Problems"]], "Rows": rows},
        "Fidelity": {"Records": fidelity, "DeliveredLinesByPath": {k: len(v) for k, v in sorted(delivered.items())},
                     "FailedReadsNeverCredited": failed_reads, "TruncatedKnownLongLines": truncated_known,
                     "UnicodeReplacementChars": unicode_anomalies,
                     "GrepContentLines": {"Total": len(grep_info), "Exact": sum(1 for x in grep_info if x["Exact"])}},
        "CoverageInformational": coverage,
        "Result": {"Check": rcheck, "Literal": result},
        "PremiseCheck": premise_check,
        "Accreditation": {"Status": "ACCREDITED" if not reasons else "NOT_ACCREDITED", "Reasons": reasons},
    }


def is_required_id(fid):
    return bool(re.match(r"^A62-A4-[0-9]{2}$", str(fid)))


def coherence(result, c):
    v = result.get("Verdict")
    req = result.get("RequiredFindings") or []
    opt = result.get("OptionalFindings") or []
    req_ids = [f.get("FindingId") for f in req]
    opt_ids = [f.get("FindingId") for f in opt]
    qd = result.get("QuestionDispositions") or []
    fo = result.get("Focus") or []
    ia = result.get("IfAgreed") or {}
    nc = result.get("NoChangeConfirmation") or {}
    ocl = result.get("OwnerConsumptionLines") or {}
    verdicts = {k: (result.get(k) or {}) for k in ("A45Verdict", "A46Verdict")}
    oac = result.get("OwnerAuthorityCheck") or {}
    owner_needed = bool(oac.get("OwnerDecisionRequired"))
    pf = result.get("PriorFindingDispositions") or []
    prior = {d.get("FindingId"): d for d in pf}
    dcf = result.get("DeltaConfirmation") or {}
    all_ids = set(req_ids) | set(opt_ids)
    prior_required = [x for x in PRIOR_IDS if is_required_id(x)]
    if len(set(req_ids)) != len(req_ids) or len(set(opt_ids)) != len(opt_ids):
        c.append("identificadores de hallazgo repetidos")
    for f in req:
        if not str(f.get("ViolatedInvariant", "")).strip() and not str(f.get("Counterexample", "")).strip():
            c.append("%s REQUIRED sin ViolatedInvariant ni Counterexample" % f.get("FindingId"))
    # ---- hallazgos anteriores (reglas del auditor de la re-revisión de A-2, con dos REQUIRED anteriores)
    for fid in sorted(all_ids & set(PRIOR_IDS)):
        want = "STILL_OPEN" if is_required_id(fid) else "NOT_APPLIED"
        if (prior.get(fid) or {}).get("Disposition") != want:
            c.append("id anterior %s reutilizado sin disponerlo %s" % (fid, want))
    for d in pf:
        fid, disp, links = d.get("FindingId"), d.get("Disposition"), d.get("LinkedFindingIds") or []
        if is_required_id(fid):
            if disp not in ("CLOSED", "STILL_OPEN"):
                c.append("%s debe disponerse CLOSED o STILL_OPEN" % fid)
            if not d.get("PremiseRefs"):
                c.append("%s sin PremiseRefs (su disposición exige evidencia)" % fid)
        elif disp not in ("APPLIED", "NOT_APPLIED", "N/A"):
            c.append("%s debe disponerse APPLIED, NOT_APPLIED o N/A" % fid)
        if any(x not in all_ids for x in links):
            c.append("%s cita en LinkedFindingIds hallazgos que no existen" % fid)
        if disp in ("STILL_OPEN", "NOT_APPLIED"):
            if not links:
                c.append("%s %s sin un hallazgo vinculado" % (fid, disp))
            elif is_required_id(fid) and not any(x in req_ids for x in links):
                c.append("%s STILL_OPEN sin un REQUIRED vinculado" % fid)
    if v != "NOT_ACCREDITED":
        if sorted(d.get("Id") for d in qd) != sorted(QUESTION_IDS):
            c.append("QuestionDispositions debe disponer %s..%s una vez cada una" % (QUESTION_IDS[0], QUESTION_IDS[-1]))
        if sorted(x.get("Topic") for x in fo) != sorted(FOCUS_TOPICS):
            c.append("Focus debe disponer los ocho temas (%s) una vez cada uno" % ", ".join(FOCUS_TOPICS))
        if sorted(d.get("FindingId") for d in pf) != sorted(PRIOR_IDS):
            c.append("PriorFindingDispositions debe disponer %s una vez cada uno" % ", ".join(PRIOR_IDS))
        if any((dcf.get(k) or {}).get("Assessment") not in ("CONFIRMED", "NOT_CONFIRMED") for k in DELTA_KEYS):
            c.append("con un veredicto, DeltaConfirmation va en CONFIRMED o NOT_CONFIRMED (NOT_DETERMINED solo con NOT_ACCREDITED)")
        for d in qd + fo:
            fids, kind, label = d.get("FindingIds") or [], d.get("Disposition"), d.get("Id") or d.get("Topic")
            if kind == "REQUIRED" and (not fids or any(x not in req_ids for x in fids)):
                c.append("%s REQUIRED sin un REQUIRED de RequiredFindings que lo sostenga" % label)
            if kind == "OPTIONAL" and (not fids or any(x not in opt_ids for x in fids)):
                c.append("%s OPTIONAL sin un OPTIONAL de OptionalFindings que lo sostenga" % label)
            if kind == "NO_FINDING" and fids:
                c.append("%s NO_FINDING con hallazgos vinculados" % label)
        if any(x not in ("YES", "NO") for x in ((result.get("Materiality") or {}).get("Overall") or {}).values()):
            c.append("con un veredicto, Materiality.Overall M01..M08 va en YES o NO (NOT_ASSESSED solo con NOT_ACCREDITED)")
        if any(x is None for k, x in (result.get("IdentityCheck") or {}).items() if k not in ("Basis", "Note")):
            c.append("con un veredicto, IdentityCheck no admite null")
        if oac.get("OwnerDecisionRequired") is None:
            c.append("con un veredicto, OwnerAuthorityCheck.OwnerDecisionRequired no admite null")
        for name, vd in verdicts.items():
            if vd.get("Verdict") == "NOT_ASSESSED":
                c.append("con un veredicto, %s no admite NOT_ASSESSED" % name)
        if result.get("ReviewedCommit") != REV or result.get("ReviewedBlob") != OBJECT_BLOB or result.get("ReviewedPath") != OBJECT:
            c.append("objeto revisado distinto del designado")
    for name, vd in verdicts.items():
        vv, vf = vd.get("Verdict"), vd.get("FindingIds") or []
        if any(x not in all_ids for x in vf):
            c.append("%s cita en FindingIds hallazgos que no existen" % name)
        if vv == "CHANGES REQUIRED" and not any(x in req_ids for x in vf):
            c.append("%s CHANGES REQUIRED sin un REQUIRED vinculado" % name)
        if vv == "BLOCKED — OWNER DECISION" and not owner_needed:
            c.append("%s BLOCKED — OWNER DECISION sin OwnerDecisionRequired" % name)
    if v == "AGREED" and (req or any(d.get("Disposition") == "REQUIRED" for d in qd + fo) or owner_needed or
                          any((prior.get(x) or {}).get("Disposition") != "CLOSED" for x in prior_required) or
                          not ia or not all(ia.get(k) is True for k in ia) or
                          any((nc.get(k) or {}).get("Assessment") != "NO_CHANGE" for k in nc) or
                          any((dcf.get(k) or {}).get("Assessment") != "CONFIRMED" for k in DELTA_KEYS) or
                          ocl.get("Assessment") != "ONLY_CONSUMPTION" or ocl.get("EffectiveOnlyAfterAgreement") != "YES" or
                          any(vd.get("Verdict") not in ("AGREED", "EXCLUDE") for vd in verdicts.values())):
        c.append("AGREED exige cero REQUIRED (también en Focus y en las preguntas), A62-A4-01 y A62-A4-02 CLOSED, A45Verdict y A46Verdict en "
                 "AGREED o EXCLUDE, sin decisión del Owner, OwnerConsumptionLines = ONLY_CONSUMPTION con EffectiveOnlyAfterAgreement = YES, "
                 "NoChangeConfirmation todo NO_CHANGE, DeltaConfirmation todo CONFIRMED e IfAgreed todo en true")
    if v != "AGREED" and any(x is not None for x in ia.values()):
        c.append("IfAgreed debe ir en null salvo con AGREED")
    if v == "CHANGES REQUIRED" and not req:
        c.append("CHANGES REQUIRED sin ningún REQUIRED")
    if v == "BLOCKED — OWNER DECISION" and not owner_needed:
        c.append("BLOCKED — OWNER DECISION sin OwnerDecisionRequired")
    if v == "NOT_ACCREDITED" and not result.get("KnownLimitations"):
        c.append("NOT_ACCREDITED sin el motivo en KnownLimitations")
    if any(result.get(k) != IDS[k] for k in ("RunId", "InvocationId", "LogicalReviewRequestId")):
        c.append("identificadores de la invocación distintos de los del cierre")


if __name__ == "__main__":
    TRANSCRIPT, STDOUT, CLONE_ARG, RUN_ARG, KIT_ARG, SCHEMA, OUT = sys.argv[1:8]
    OUTPUT_JSON = sys.argv[8] if len(sys.argv) > 8 else None
    setup(CLONE_ARG, RUN_ARG, KIT_ARG)
    doc = audit(TRANSCRIPT, STDOUT, SCHEMA)
    with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(doc, fh, ensure_ascii=False, indent=1)
        fh.write("\n")
    if OUTPUT_JSON and doc["Result"]["Literal"] is not None:
        with open(OUTPUT_JSON, "w", encoding="utf-8", newline="\n") as fh:
            json.dump(doc["Result"]["Literal"], fh, ensure_ascii=False, indent=1)
            fh.write("\n")
    print(json.dumps({"Accreditation": doc["Accreditation"]["Status"], "Reasons": len(doc["Accreditation"]["Reasons"]),
                      "Calls": doc["ToolAudit"]["Calls"], "Verdict": (doc["Result"]["Literal"] or {}).get("Verdict")}, ensure_ascii=True))
