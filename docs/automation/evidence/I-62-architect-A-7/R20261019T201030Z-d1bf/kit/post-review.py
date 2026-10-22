"""I-62, decisiones §94: post-review audit v6.0-a7r2 of ONE claude-cli run of the ONE FORMAL REVIEW of ROUND 2 of amendment A-7
(docs/initiatives/I-62-A-7.md, blob 8a528133… pinned in the closure and in owner_gate.py, PROPUESTA round 2), bounded to A62-A7-R1..R3 and their
consequences (decisions §94, point 6), by an independent ARCHITECT. Declared and self-tested BEFORE the launch (Owner CLAUDE-CLI-I62 = A; the launch
also requires gate 11, owner_gate.py: the Owner line of §78, the A-7 blob and the agreed A-6 blob at the worktree HEAD and a Coordinator disposition
AFTER §94 naming this review, pinned in owner_gate.py with its number, heading, full blob and text SHA-256, and no later section; the kit is
delivered with that authorization PENDING); the principal session runs it afterwards, never before, on the complete run, and the auditor is not
corrected after the run.

v6.0-a7r2 = the v5.6-a7 auditor (the v5.5-a7 auditor of A-7 round 1, R20261019T010200Z-4a72, plus the read-fidelity correction of decisions §94,
point 5: faithful copies RACKCAD-COPIA-FIEL/1, verified byte for byte against the originals and crediting the originals' lines): every launch,
transport-gate, stream-json, transcript, tool, read-fidelity, faithful-copy, reference, direct-read, run-custody, clone, run.json, FrozenCommandGuard,
RecordProbe, Environment and `Categories` rule is kept. The run has three files besides the faithful copies (order.txt, the §94 body, and a7-r1.md,
the round-1 text, both premises; prompt.md); the direct read is AUTOMATION_PLAN 16.8 L616-L617 (the definition of «exige RED» that A7-1, point 1
(c), cites), taken from closure.json like before. Changes:
  * preflight.json, gate 11: the authorization pinned after §94 (messages); the rule of round 1 otherwise;
  * schema coherence (rackcad-a7r2-architect-review/0), the round-N pattern of the A-6 rounds: ids unique in Round1FindingResolution,
    RegressionCheck, QuestionAnswers, OptionalDispositionConformance, Materiality.Triggers and DeltaPurity.Changes and, with a verdict, exactly
    A62-A7-R1..R3, the five round-1 CONFORMS topics, Q-A7R2-01..03, M-01..M-08 and the ten round-2 changes once each (OptionalDispositionConformance
    is optional: any subset of A62-A7-O1..O8, no repeats); a resolution PARTIALLY_CORRECTED or NOT_CORRECTED => the residual REQUIRED linked,
    CORRECTED => none; a topic REGRESSION => a linked REQUIRED, NO_REGRESSION => none; a change OUTSIDE_R1_R3 => a linked REQUIRED, WITHIN_R1_R3 =>
    none, DeltaPurity CONFIRMED <=> no change OUTSIDE (NOT_CONFIRMED => a linked REQUIRED); a trigger DISAGREE => a linked REQUIRED, AGREE => none,
    and A7Declared = the A-7 §5 table of the closure (MaterialityA7); a question NOT_SATISFIED and an optional disposition DOES_NOT_CONFORM => a
    linked finding; OwnerDecisionRequired true <=> UnidentifiedConsequences non-empty; ChangedByRound2 true => a Note; with a verdict, ClauseRefs in
    every element (optional dispositions NOT_ASSESSED excepted), no NOT_DETERMINED in DeltaPurity and no null in OwnerDecisionRequired or
    ChangedByRound2, every FindingIds entry exists and every REQUIRED is cited by at least one element; AGREED => no REQUIRED, no Owner decision
    pending, IfAgreed all true, R1-R3 CORRECTED, DeltaPurity CONFIRMED with the ten changes WITHIN, the five topics NO_REGRESSION and every
    trigger AGREE; CHANGES REQUIRED or REJECTED => at least one REQUIRED;
  * kept: findings (new ids A62-A7R2-R<n> / A62-A7R2-O<n>), NOT_ACCREDITED, BLOCKED — OWNER DECISION, IfAgreed, invocation ids, reviewed object,
    Step0 (seven booleans, now the round-2 Step 0), validate_schema (Test-Json AND schema_errors) and the direct read rule.
Read-fidelity correction (phase 1) as in v5.6-a7: v5.5-a7 plus the read-fidelity correction of decisions §94, point 5 (the r1 run R20261019T010200Z-4a72 was NOT_ACCREDITED because Read
silently dropped the UTF-8 BOM of a TRX, line 1 DEGRADED, and the closure tooling had not detected BOM files). Every v5.5 rule is kept; in
particular a Read of an ORIGINAL whose delivery is altered is still DEGRADED and a reason (a silently dropped BOM is never faithful), and the known
truncation of a long line (ReadPolicy.LongLines; TruncatedKnownLongLines) still credits nothing. Added (fidelity.py, loaded from the kit):
  * the run files are closure RunFiles, whose tail must be exactly the FaithfulCopies (fidelity.run_files_of): each file the detector flagged has a
    faithful copy RACKCAD-COPIA-FIEL/1 in the run, read INSTEAD of the original, with the same line numbers (custody like every run file);
  * every faithful copy, read or not, is verified against the original's exact bytes at the commit: fidelity.verify (decodes into exactly those
    bytes, names that original, is the canonical encoding of what it decodes), its SHA-256 = the closure's CopySha256 and the original's blob = the
    closure's OriginalBlob; any problem is a reason and the copy credits nothing;
  * a verified copy credits line n of the ORIGINAL only when Read delivered faithfully line n of the copy and every continuation line @n.k of its
    appendix (fidelity.layout); references, DirectReads and the direct read resolve against the original's lines, delivered by Read in the
    original or in its faithful copy; a copy is never a reference (it is not a premise run file);
  * the canonical text of a closure file is decoded with surrogateescape (a non-UTF-8 original no longer stops the audit: its altered delivery is
    DEGRADED), and every Read of a flagged original is recorded (Fidelity.FlaggedOriginalReads; informational: the DEGRADED rule decides).
Small seams (list_dir, clone_git, clone_reparse, validate_schema, GATE, LAUNCH_DIR) let the self-test simulate a tampered run dir, launch dir or
clone without touching them; the real audit never changes them.

Checks (each failure is a reason; ACCREDITED only with 0 reasons):
  * launch: stdout.jsonl in <run>/launch; run.json there: exit code 0, not killed, not timed out, session id, executable = the closure binary
    (measured resolved path and SHA-256), argument list = closure flags + --add-dir + --session-id + compact schema, cwd = clone, PromptSha256 =
    run/prompt.md, StdoutSha256 = the audited stdout, Transcript.Sha256 = the audited transcript, Outcome/PostRunErrors/fields as in v5.3;
    preflight.json there: the ten checks present and Ok (and gate 11 as above), session id, measured binary = closure (path and SHA-256), version
    = 2.1.299 (Claude Code), loggedIn = true, gate = kit/transport-gate.json, FrozenCommandGuard, RecordProbe and Environment as in v5.3;
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
  * read fidelity of every Read; references (every Evidence and every ClauseRefs, nested ones included, and also every DirectReads) resolve to
    lines of a closure file or of a premise run file (order.txt), all delivered faithfully by Read in that very file; the direct read
    of AUTOMATION_PLAN L616-L617 (the closure's DirectRead) by Read in that file or in its faithful copy, with an entry of DirectReads that
    covers it (a quotation inside A-7 is not a read of the source);
  * structured output: the result's structured_output equals the input of the last successful StructuredOutput call (and its attachment), is
    valid against result.schema.json (Test-Json and schema_errors) and coherent (above);
  * run custody: each run file = kit copy = closure RunFileHashes; the run dir holds only the run files and launch/; launch/ holds only the files
    launch.py writes;
  * clone after the run: HEAD = the commit, detached (no local branch), no remote, clean with ignored files, designated blobs unchanged, no
    reparse points.

Usage: python -B kit/post-review.py <transcript.jsonl> <stdout.jsonl> <clone> <run dir> <kit dir> <result.schema.json> <audit.json> [<output.json>]
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

AUDITOR_VERSION = "v6.0-a7r2 (claude-cli; Read/Grep/Glob only; transport gate D-01; Owner and authorization gate 11 on a Coordinator disposition after §94 pinned in owner_gate.py (pending in the kit as delivered); run.json Outcome and PostRunErrors; frozen command and record probe; direct read of AUTOMATION_PLAN L616-L617; faithful copies RACKCAD-COPIA-FIEL/1 of the files Read cannot deliver faithfully, decisions §94, point 5; round-2 schema rackcad-a7r2-architect-review/0; declared and self-tested before the launch; decisions §94)"
REPLACEMENT_CHAR = chr(0xFFFD)   # U+FFFD in a tool result: a transport artifact, recorded


def setup(clone, run, kit):
    global CLONE, RUN, KIT, CLOSURE, REV, IDS, ALLOWED_LC, RUN_FILES, RUN_FILES_LC, PREMISE_RUN, LONG_LINES, OBJECT_FILES, OBJECT, OBJECT_BLOB
    global TRANSPORT, SESSION_ID, CLONE_REAL, RUN_REAL, ALLOWED_TOOLS, DESIGNATED, TG, OG, GATE
    global LAUNCH_DIR, LAUNCH_FILES, ROUND1, OPTIONALS, REGRESSION, CHANGES, TRIGGERS, MAT_A7, QUESTIONS, DIRECT, LAUNCH_COMMAND, FID, FAITHFUL, COPY_OF
    CLONE, RUN, KIT = clone, run, kit
    CLOSURE = json.load(open(os.path.join(kit, "closure.json"), encoding="utf-8"))
    assert norm(CLOSURE["RunDir"]).lower() == norm(run).lower(), "run dir distinto del RunDir del cierre"
    REV, IDS = CLOSURE["AuthorityRevision"], CLOSURE["Ids"]
    ALLOWED_LC = {r["Path"].lower(): r["Path"] for r in CLOSURE["CanonicalInputs"] + CLOSURE["AllowedTransitiveInputs"]}
    DESIGNATED = {r["Path"]: r["Blob"] for r in CLOSURE["CanonicalInputs"]}
    spec = importlib.util.spec_from_file_location("fidelity", os.path.join(kit, "fidelity.py"))
    FID = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(FID)
    RUN_FILES = FID.run_files_of(CLOSURE)                    # ValueError (no audit) if RunFiles does not end in exactly the FaithfulCopies
    FAITHFUL = {e["Copy"]: e for e in CLOSURE["FaithfulCopies"]}
    COPY_OF = {e["Original"]: e["Copy"] for e in CLOSURE["FaithfulCopies"]}
    RUN_FILES_LC = {n.lower(): n for n in RUN_FILES}
    PREMISE_RUN = set(CLOSURE["PremiseRunFiles"])
    LONG_LINES = {p: set(v) for p, v in CLOSURE["ReadPolicy"]["LongLines"].items()}
    OBJECT_FILES, OBJECT, OBJECT_BLOB = dict(CLOSURE["ObjectFiles"]), CLOSURE["ObjectPath"], CLOSURE["ObjectBlob"]
    ROUND1, OPTIONALS, QUESTIONS = list(CLOSURE["Round1Findings"]), list(CLOSURE["Round1Optionals"]), list(CLOSURE["QuestionIds"])
    REGRESSION, CHANGES = list(CLOSURE["RegressionTopics"]), list(CLOSURE["DeltaChanges"])
    MAT_A7 = dict(CLOSURE["MaterialityA7"])
    TRIGGERS = sorted(MAT_A7)
    DIRECT, LAUNCH_COMMAND = CLOSURE["DirectRead"], CLOSURE["Transport"]["LaunchCommand"]
    TRANSPORT, SESSION_ID = CLOSURE["Transport"], CLOSURE["Transport"]["SessionId"]
    ALLOWED_TOOLS = set(CLOSURE["AllowedTools"])
    CLONE_REAL, RUN_REAL = real(CLONE), real(RUN)
    spec = importlib.util.spec_from_file_location("transport_gate", os.path.join(kit, "transport_gate.py"))
    TG = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(TG)
    spec = importlib.util.spec_from_file_location("owner_gate", os.path.join(kit, "owner_gate.py"))
    OG = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(OG)
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


def schema_errors(value, schema, path="$"):
    """In-process check of `value` against `schema` for the keyword subset measured with claude-cli 2.1.293 (type, additionalProperties false,
    required, properties, enum, pattern, minLength, minItems, items); an unknown keyword is an error (fail closed). → list of errors.
    Added in v5.4-a6r4: pwsh 7.6.7 (installed 2026-10-14) Test-Json validates only the first level of nesting (measured), so validate_schema
    requires both."""
    errs = []
    unknown = sorted(set(schema) - {"type", "additionalProperties", "required", "properties", "enum", "pattern", "minLength", "minItems", "items"})
    if unknown or schema.get("additionalProperties", False) not in (True, False):
        return ["%s: palabras clave fuera del subconjunto medido %s" % (path, unknown or ["additionalProperties"])]

    def is_type(v, t):
        return {"object": isinstance(v, dict), "array": isinstance(v, list), "string": isinstance(v, str), "boolean": isinstance(v, bool),
                "integer": isinstance(v, int) and not isinstance(v, bool), "null": v is None}.get(t, False)
    types = schema["type"] if isinstance(schema.get("type"), list) else ([schema["type"]] if "type" in schema else None)
    if types is not None and not any(is_type(value, t) for t in types):
        return ["%s: tipo %s, se espera %s" % (path, type(value).__name__, types)]
    if "enum" in schema and not any(json.dumps(value, sort_keys=True) == json.dumps(e, sort_keys=True) for e in schema["enum"]):
        errs.append("%s: valor fuera del enum" % path)
    if isinstance(value, str) and "pattern" in schema and re.search(schema["pattern"], value) is None:
        errs.append("%s: no casa con el patrón" % path)
    if isinstance(value, str) and len(value) < schema.get("minLength", 0):
        errs.append("%s: más corto que minLength" % path)
    if isinstance(value, list):
        if len(value) < schema.get("minItems", 0):
            errs.append("%s: menos elementos que minItems" % path)
        for i, x in enumerate(value):
            errs += schema_errors(x, schema["items"], "%s[%d]" % (path, i)) if "items" in schema else []
    if isinstance(value, dict):
        props = schema.get("properties", {})
        errs += ["%s: falta %s" % (path, k) for k in schema.get("required", []) if k not in value]
        errs += ["%s: propiedad no admitida %s" % (path, k) for k in value if schema.get("additionalProperties") is False and k not in props]
        for k, v in value.items():
            errs += schema_errors(v, props[k], "%s.%s" % (path, k)) if k in props else []
    return errs


def validate_schema(result, schema):
    """Test-Json (PowerShell 7) against the declared schema AND the in-process check of the measured keyword subset (schema_errors) →
    (valid, detail). v5.4-a6r4: both are required, because pwsh 7.6.7 Test-Json checks only the first level of nesting."""
    tmp = tempfile.mkdtemp()
    rp = os.path.join(tmp, "result.json")
    with open(rp, "w", encoding="utf-8") as fh:
        json.dump(result, fh, ensure_ascii=False)
    ps = "try { $null = Get-Content -Raw -LiteralPath '%s' | Test-Json -SchemaFile '%s' -ErrorAction Stop; 'True' } catch { 'False: ' + $_.Exception.Message }" % (rp, schema)
    o = subprocess.run(["pwsh", "-NoProfile", "-Command", ps], capture_output=True, text=True, encoding="utf-8", errors="replace").stdout.strip()
    os.remove(rp)
    os.rmdir(tmp)
    errs = schema_errors(result, json.load(open(schema, encoding="utf-8")))
    return o == "True" and not errs, ("Test-Json: %s | subconjunto: %s" % (o[:300], "; ".join(errs[:5]) if errs else "sin errores"))[:600]


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


def canon_bytes(rel):
    """Exact bytes of a closure file at the commit (the faithful copies are verified against them)."""
    if ("bytes", rel) not in canon_cache:
        canon_cache[("bytes", rel)] = subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel])
    return canon_cache[("bytes", rel)]


def canon(rel):
    if rel not in canon_cache:
        canon_cache[rel] = canon_bytes(rel).decode("utf-8", "surrogateescape").replace("\r\n", "\n").split("\n")
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
        launch.update({"Outcome": run_json.get("Outcome"), "OutcomeCategories": run_json.get("OutcomeCategories"),
                       "OutcomeRecomputed": outcome_categories(run_json), "PostRunErrors": run_json.get("PostRunErrors"),
                       "LaunchError": run_json.get("LaunchError"), "Aborted": run_json.get("Aborted")})
        if run_json.get("Outcome") != "COMPLETED":
            add("lanzamiento: Outcome de run.json %r (se exige COMPLETED; categorías registradas %s)" % (run_json.get("Outcome"),
                                                                                                  run_json.get("OutcomeCategories")))
        if run_json.get("OutcomeCategories") != launch["OutcomeRecomputed"] or run_json.get("Outcome") != launch["OutcomeRecomputed"][0]:
            add("lanzamiento: Outcome y OutcomeCategories de run.json incoherentes con sus campos (recalculado: %s)" % launch["OutcomeRecomputed"])
        if run_json.get("PostRunErrors") != []:
            add("lanzamiento: PostRunErrors de run.json no vacío o ausente: %s" % str(run_json.get("PostRunErrors"))[:200])
        if "LaunchError" in run_json or "Aborted" in run_json:
            add("lanzamiento: run.json registra un fallo de lanzamiento (LaunchError o Aborted)")
        missing_fields = [k for k in RUN_JSON_FIELDS if k not in run_json]
        if missing_fields:
            add("lanzamiento: run.json sin los campos del registro %s" % missing_fields)
    if preflight is None:
        add("lanzamiento: falta preflight.json en launch/")
    else:
        checks = preflight.get("Checks") or {}
        launch["PreflightChecks"] = {k: (v or {}).get("Ok") for k, v in checks.items()}
        missing = [k for k in TG.PREFLIGHT_CHECKS + [OG.CHECK] if k not in checks]
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
        og = checks.get(OG.CHECK) or {}
        if og.get("A6LineSha256") != OG.A6_LINE_SHA or og.get("ClaudeCliLineSha256") != OG.CLAUDE_LINE_SHA or not og.get("A6LineNumbers") or \
                not og.get("ClaudeCliLineNumbers") or og.get("Problems"):
            add("preflight: compuerta 11 (%s) sin las líneas fijadas A6-ADR0046-9-FX02 y CLAUDE-CLI-I62 = A, o con problemas" % OG.CHECK)
        auth = [a for a in (og.get("AuthorizationSections") or []) if isinstance(a, dict)]
        pins = OG.pinned_authorization()
        if OG.authorization_pending(pins) or og.get("AuthorizationPending") is not False:
            add("preflight: compuerta 11 (%s) con la autorización pendiente (owner_gate.py del kit sin la disposición posterior a §94 fijada, o "
                "registro con AuthorizationPending distinto de false)" % OG.CHECK)
        if og.get("ObjectBlob") != OBJECT_BLOB or og.get("ObjectBlobAtHeadMatches") is not True or og.get("A6AgreedBlobAtHeadMatches") is not True or \
                og.get("RunId") != IDS["RunId"] or \
                og.get("Authorized") is not True or og.get("LaterSections") != [] or len(auth) != 1 or \
                auth[0].get("Number") != pins["Section"] or auth[0].get("HeadingMatches") is not True or auth[0].get("HasBlob") is not True or \
                auth[0].get("TextSha256") != pins["TextSha256"] or og.get("AuthorizationTextSha256") != pins["TextSha256"] or \
                og.get("AuthorizationSection") != pins["Section"] or og.get("AuthorizationHeading") != pins["Heading"]:
            add("preflight: compuerta 11 (%s) sin el blob de A-7 o el de la A-6 acordada en la HEAD del worktree, sin la autorización fijada en "
                "owner_gate.py (sección posterior a §94, encabezado, blob completo y SHA-256 del texto) o con una sección posterior a ella" % OG.CHECK)
        fg = preflight.get("FrozenCommandGuard") if isinstance(preflight.get("FrozenCommandGuard"), dict) else {}
        launch["FrozenCommandGuard"] = {k: fg.get(k) for k in ("Ok", "Enforced", "FrozenCommand", "OrigArgvTail", "Problems")}
        if fg.get("Ok") is not True or fg.get("Enforced") is not True or fg.get("FrozenCommand") != LAUNCH_COMMAND or fg.get("Problems"):
            add("preflight: guarda de la orden congelada (FrozenCommandGuard) ausente, no aplicada, fallida o con otra orden que %s" % LAUNCH_COMMAND)
        rp = preflight.get("RecordProbe") if isinstance(preflight.get("RecordProbe"), dict) else {}
        launch["RecordProbe"] = {k: rp.get(k) for k in ("Ok", "Problems", "RunAndRecordExit", "Outcome")}
        if rp.get("Ok") is not True or rp.get("Problems"):
            add("preflight: sonda de registro (RecordProbe) ausente o fallida")
        env = preflight.get("Environment") if isinstance(preflight.get("Environment"), dict) else {}
        names, pyvals = env.get("Names"), env.get("PythonValues")
        launch["Environment"] = {"Names": names, "PythonValues": pyvals}
        if env.get("Prefixes") != list(LAUNCH_ENV_PREFIXES) or not isinstance(names, list) or not isinstance(pyvals, dict) or \
                sorted(pyvals) != sorted(n for n in names if str(n).upper().startswith("PYTHON")) or \
                any(not str(n).upper().startswith(LAUNCH_ENV_PREFIXES) for n in names):
            add("preflight: entorno del lanzador sin registrar o mal registrado (Environment: prefijos %s, nombres y valores solo de PYTHON*)"
                % list(LAUNCH_ENV_PREFIXES))

    launch["ExecutionCategory"] = "RUN_JSON_MISSING" if run_json is None else (
        run_json.get("Outcome") if isinstance(run_json.get("Outcome"), str) else "OUTCOME_MISSING")

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

    # ---------------- faithful copies (decisions §94, point 5): a copy is a run file read INSTEAD of its original; it credits the original's lines
    flagged_reads = [dict(f, Copy=COPY_OF[f["Path"]], Flags=sorted(FAITHFUL[COPY_OF[f["Path"]]]["Flags"]))
                     for f in fidelity if f["Kind"] == "CLOSURE" and f["Path"] in COPY_OF]
    copies_doc = []
    for name in sorted(FAITHFUL):
        e = FAITHFUL[name]
        try:
            cb = read_run_bytes(name)
        except OSError:
            cb = None
        ob = canon_bytes(e["Original"])
        probs = FID.verify(e["Original"], ob, cb)
        if cb is not None and sha(cb) != e["CopySha256"]:
            probs.append("SHA-256 de la copia distinto del fijado en el cierre")
        if FID.git_blob(ob) != e["OriginalBlob"]:
            probs.append("blob del original distinto del fijado en el cierre")
        credited = []
        if probs:
            add("copia fiel: %s no entrega byte a byte %s (blob %s): %s" % (name, e["Original"], e["OriginalBlob"][:8], "; ".join(probs[:4])))
        else:
            got = delivered.get(name, set())
            credited = [n for n, ls in sorted(FID.layout(cb).items()) if all(x in got for x in ls)]
            delivered.setdefault(e["Original"], set()).update(credited)
        copies_doc.append({"Copy": name, "Original": e["Original"], "OriginalBlob": e["OriginalBlob"], "Flags": e["Flags"], "Verified": not probs,
                           "Problems": probs, "CopyLinesDelivered": len(delivered.get(name, set())), "OriginalLinesCredited": len(credited)})

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
                   "LocalBranches": g("for-each-ref", "--format=%(refname)", "refs/heads").split(), "Remotes": g("remote").split(),
                   "Detached": g("rev-parse", "--symbolic-full-name", "HEAD").strip() == "HEAD"}
    blobs_now = {p: g("rev-parse", "HEAD:" + p).strip() for p in DESIGNATED}
    clone_after["DesignatedBlobsUnchanged"] = all(blobs_now[p] == DESIGNATED[p] for p in DESIGNATED)
    reparse = clone_reparse()
    clone_after["ReparsePoints"] = reparse
    if not (clone_after["Head"] == REV and clone_after["StatusPorcelainIgnored"].strip() == "" and clone_after["LocalBranches"] == [] and
            clone_after["Detached"] and clone_after["Remotes"] == []):
        add("el clon no quedó limpio, cambió de HEAD, dejó de estar separado, ganó ramas o remotos")
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

    # ---------------- clause references (ApplicableClauses[].Ref + Excerpt; every ClauseRefs array)
    ref_check = []
    for group, ref, excerpt in collect_refs(result):
        m = REF_RX.match(str(ref or ""))
        if not m:
            add("referencia mal formada en %s: %r" % (group, str(ref)[:80]))
            ref_check.append({"Group": group, "Ref": str(ref)[:120], "WellFormed": False})
            continue
        rel_in, a = m.group(1), int(m.group(2))
        b = int(m.group(3)) if m.group(3) else a
        kind, rel = classify(rel_in, CLONE)
        if kind == "OUTSIDE" and norm(rel_in).lower() in RUN_FILES_LC:
            kind, rel = "RUN", RUN_FILES_LC[norm(rel_in).lower()]   # bare name: no closure file has a run-file name
        in_range = found = delivered_ok = False
        lines = []
        if kind == "CLOSURE" or (kind == "RUN" and rel in PREMISE_RUN):
            lines = canon(rel) if kind == "CLOSURE" else run_file(rel)
            in_range = 1 <= a <= b <= len(lines)
            delivered_ok = in_range and all(n in delivered.get(rel, set()) for n in range(a, b + 1))
        if in_range and excerpt is not None:
            found = " ".join(str(excerpt).split()) in " ".join(" ".join(lines[a - 1:b]).split()) and bool(str(excerpt).strip())
        shown = "%s:%s" % (rel if kind in ("CLOSURE", "RUN") else userless(rel), m.group(2) + ("-" + m.group(3) if m.group(3) else ""))
        ref_check.append({"Group": group, "Ref": shown, "Kind": kind, "Lines": [a, b], "InRange": in_range,
                          "ExcerptFound": (found if excerpt is not None else None), "DeliveredFaithfully": delivered_ok})
        if not in_range:
            add("referencia no canónica en %s (%s)" % (group, shown))
        elif not delivered_ok:
            add("referencia sobre líneas no entregadas fielmente en %s (%s)" % (group, shown))

    # ---------------- direct read of V14 L2333-L2334 (decisions §80, point 6)
    r_ = result if isinstance(result, dict) else {}
    dpath, da, db = DIRECT["Path"], int(DIRECT["Start"]), int(DIRECT["End"])
    direct = {"Path": dpath, "Lines": [da, db], "DeliveredByRead": all(n in delivered.get(dpath, set()) for n in range(da, db + 1))}
    if not direct["DeliveredByRead"]:
        add("lectura directa: %s:%d-%d no entregadas fielmente por Read en ese mismo archivo ni en su copia fiel" % (dpath, da, db))
    cover = []
    for x in items(r_.get("DirectReads")):
        m = REF_RX.match(str(x.get("Ref") or ""))
        if m and classify(m.group(1), CLONE) == ("CLOSURE", dpath) and int(m.group(2)) <= da and int(m.group(3) or m.group(2)) >= db:
            cover.append(x.get("Ref"))
    direct["DirectReadsEntries"] = cover
    if not cover:
        add("lectura directa: DirectReads sin una entrada que cubra %s:%d-%d" % (dpath, da, db))

    # ---------------- coverage (informational, not reasons)
    def complete(ref, n_lines):
        return all(n in delivered.get(ref, set()) for n in range(1, n_lines + 1))
    coverage = {n: complete(n, CLOSURE["RunFileHashes"][n]["Lines"]) for n in CLOSURE["PremiseRunFiles"]}
    for r in CLOSURE["CanonicalInputs"]:
        if r["Path"] in OBJECT_FILES:
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
                     "FaithfulCopies": copies_doc, "FlaggedOriginalReads": flagged_reads,
                     "UnicodeReplacementChars": unicode_anomalies,
                     "GrepContentLines": {"Total": len(grep_info), "Exact": sum(1 for x in grep_info if x["Exact"])}},
        "CoverageInformational": coverage,
        "Result": {"Check": rcheck, "Literal": result},
        "ReferenceCheck": ref_check, "DirectRead": direct,
        "Categories": {"Execution": launch.get("ExecutionCategory"),
                       "ArchitectVerdict": result.get("Verdict") if isinstance(result, dict) else None,
                       "Note": "decisiones §84, punto 4: la categoría de ejecución y custodia (Outcome de run.json) y el resultado de contenido del "
                               "Architect (Verdict) se informan por separado; cualquier defecto de integridad es un motivo y impide ACCREDITED"},
        "Accreditation": {"Status": "ACCREDITED" if not reasons else "NOT_ACCREDITED", "Reasons": reasons},
    }


REF_RX = re.compile(r"^(.+):([1-9][0-9]*)(?:-([1-9][0-9]*))?$")
STEP0_KEYS = ("AP168DirectReadL616L617", "OrderReadWhole", "Decisions94DeclaresOrderBytesAndSha256",
              "Decisions94AcceptsR1R3AndAuthorizesSecondCandidate", "GuardsRunModeBaseHeadResult", "PreflightSummaryCandidateAndTotals",
              "Round1OutputIdentifiesRunAndFindings")
NOT_DET = "NOT_DETERMINED"
BLOCKED = "BLOCKED " + chr(0x2014) + " OWNER DECISION"
ARRAYS = (("Round1FindingResolution", "Round1Finding"), ("RegressionCheck", "Topic"), ("QuestionAnswers", "Question"),
          ("OptionalDispositionConformance", "Round1Optional"))
NESTED = (("Materiality", "Triggers", "Trigger"), ("DeltaPurity", "Changes", "Change"))
SINGLES = ("Materiality", "OwnerReservedConsequence", "DeltaPurity", "RelatedImpacts")
NESTED_SINGLES = ()
OPEN_RESOLUTION = ("PARTIALLY_CORRECTED", "NOT_CORRECTED")
LAUNCH_ENV_PREFIXES = ("PYTHON", "GIT_", "CLAUDE", "ANTHROPIC")   # §84, punto 2 (los de launch.py)
RUN_JSON_FIELDS = ("SessionId", "Executable", "ExecutableSha256", "Args", "Cwd", "PromptSha256", "PromptBytes", "TimeoutSeconds", "TransportGate",
                   "EnvironmentVariableNames", "AuthSettle", "Start", "PostRunErrors", "Pid", "End", "ExitCode", "Killed", "TimedOut",
                   "TasklistQuery", "ProcessGoneAfter", "StdoutSha256", "StdoutLines", "StderrBytes", "ResultMessage", "TransportFailure",
                   "Transcript", "CloneAfter", "Outcome", "OutcomeCategories")   # los de launch.py (la autoprueba lo comprueba)


def outcome_categories(rec):
    """The auditor's own recomputation of the launch categories of decisions §84, point 4, from the record's fields (same rule as launch.py;
    the self-test checks they agree): every category that holds, by precedence, or COMPLETED."""
    rec = rec if isinstance(rec, dict) else {}
    started = "Pid" in rec
    cats = []
    if "LaunchError" in rec or "Aborted" in rec or not started:
        cats.append("LAUNCH_FAILURE")
    if started and (rec.get("ExitCode") != 0 or rec.get("Killed") is not False or rec.get("TimedOut") is not False):
        cats.append("REVIEW_PROCESS_FAILURE")
    if rec.get("PostRunErrors"):
        cats.append("POST_RUN_RECORDING_FAILURE")
    if started and (rec.get("ResultMessage") is not True or rec.get("TransportFailure") is not None):
        cats.append("TRANSPORT_FAILURE")
    return cats or ["COMPLETED"]


def items(x):
    return [y if isinstance(y, dict) else {} for y in (x or [])]


def dct(x):
    return x if isinstance(x, dict) else {}


def collect_refs(result):
    """→ [(group, ref, None)]: every Evidence of a finding and every ClauseRefs entry (no excerpts in this review)."""
    r = dct(result)
    out = []
    for key in ("RequiredFindings", "OptionalFindings"):
        for x in items(r.get(key)):
            out += [("%s[%s]" % (key, x.get("FindingId")), ref, None) for ref in (x.get("Evidence") or [])]
    for key, label in ARRAYS:
        for x in items(r.get(key)):
            out += [("%s[%s]" % (key, x.get(label)), ref, None) for ref in (x.get("ClauseRefs") or [])]
    for parent, key, label in NESTED:
        for x in items(dct(r.get(parent)).get(key)):
            out += [("%s.%s[%s]" % (parent, key, x.get(label)), ref, None) for ref in (x.get("ClauseRefs") or [])]
    out += [("DirectReads", x.get("Ref"), None) for x in items(r.get("DirectReads"))]
    for key in SINGLES:
        out += [(key, ref, None) for ref in (dct(r.get(key)).get("ClauseRefs") or [])]
    for parent, key in NESTED_SINGLES:
        out += [("%s.%s" % (parent, key), ref, None) for ref in (dct(dct(r.get(parent)).get(key)).get("ClauseRefs") or [])]
    return out


def coherence(result, c):
    v = result.get("Verdict")
    req, opt = items(result.get("RequiredFindings")), items(result.get("OptionalFindings"))
    req_ids, opt_ids = [f.get("FindingId") for f in req], [f.get("FindingId") for f in opt]
    all_ids = set(req_ids) | set(opt_ids)
    rr, rg, qa = items(result.get("Round1FindingResolution")), items(result.get("RegressionCheck")), items(result.get("QuestionAnswers"))
    od = items(result.get("OptionalDispositionConformance"))
    mat, dp = dct(result.get("Materiality")), dct(result.get("DeltaPurity"))
    trig, ch = items(mat.get("Triggers")), items(dp.get("Changes"))
    orc, ri, ia, step0 = dct(result.get("OwnerReservedConsequence")), dct(result.get("RelatedImpacts")), dct(result.get("IfAgreed")), \
        dct(result.get("Step0"))
    owner, changed = orc.get("OwnerDecisionRequired"), ri.get("ChangedByRound2")
    arrays = (("Round1FindingResolution", rr, "Round1Finding", ROUND1), ("RegressionCheck", rg, "Topic", REGRESSION),
              ("QuestionAnswers", qa, "Question", QUESTIONS), ("Materiality.Triggers", trig, "Trigger", TRIGGERS),
              ("DeltaPurity.Changes", ch, "Change", CHANGES))
    singles = (("Materiality", mat), ("OwnerReservedConsequence", orc), ("DeltaPurity", dp), ("RelatedImpacts", ri))
    optional = [("OptionalDispositionConformance[%s]" % x.get("Round1Optional"), x) for x in od]

    def linked(x):
        return x.get("FindingIds") or []

    def has_req(x):
        return any(i in req_ids for i in linked(x))
    if len(set(req_ids + opt_ids)) != len(req_ids + opt_ids):
        c.append("identificadores de hallazgo repetidos")
    for f in req:
        if not str(f.get("ViolatedInvariant", "")).strip() and not str(f.get("Counterexample", "")).strip():
            c.append("%s REQUIRED sin ViolatedInvariant ni Counterexample" % f.get("FindingId"))
    holders = [("%s[%s]" % (label, x.get(key)), x) for label, xs, key, _ in arrays for x in xs] + list(singles)
    for label, x in holders + optional:
        if any(i not in all_ids for i in linked(x)):
            c.append("%s cita en FindingIds hallazgos que no existen" % label)
    for label, xs, key in [(a, b, k) for a, b, k, _ in arrays] + [("OptionalDispositionConformance", od, "Round1Optional")]:
        ids_ = [str(x.get(key)) for x in xs]
        if len(set(ids_)) != len(ids_):
            c.append("%s con identificadores repetidos" % label)
    for x in rr:
        if x.get("Status") in OPEN_RESOLUTION and not has_req(x):
            c.append("Round1FindingResolution[%s] %s sin el REQUIRED residual vinculado" % (x.get("Round1Finding"), x.get("Status")))
        if x.get("Status") == "CORRECTED" and has_req(x):
            c.append("Round1FindingResolution[%s] CORRECTED con un REQUIRED vinculado" % x.get("Round1Finding"))
    for x in rg:
        if x.get("Status") == "REGRESSION" and not has_req(x):
            c.append("RegressionCheck[%s] REGRESSION sin un REQUIRED vinculado" % x.get("Topic"))
        if x.get("Status") == "NO_REGRESSION" and has_req(x):
            c.append("RegressionCheck[%s] NO_REGRESSION con un REQUIRED vinculado" % x.get("Topic"))
    for x in ch:
        if x.get("Assessment") == "OUTSIDE_R1_R3" and not has_req(x):
            c.append("DeltaPurity.Changes[%s] OUTSIDE_R1_R3 sin un REQUIRED vinculado (decisiones §94, punto 2)" % x.get("Change"))
        if x.get("Assessment") == "WITHIN_R1_R3" and has_req(x):
            c.append("DeltaPurity.Changes[%s] WITHIN_R1_R3 con un REQUIRED vinculado" % x.get("Change"))
    if dp.get("Status") == "CONFIRMED" and any(x.get("Assessment") == "OUTSIDE_R1_R3" for x in ch):
        c.append("DeltaPurity CONFIRMED con un cambio OUTSIDE_R1_R3")
    if dp.get("Status") == "NOT_CONFIRMED" and ch and all(x.get("Assessment") == "WITHIN_R1_R3" for x in ch):
        c.append("DeltaPurity NOT_CONFIRMED con todos los cambios WITHIN_R1_R3")
    if dp.get("Status") == "NOT_CONFIRMED" and not has_req(dp):
        c.append("DeltaPurity NOT_CONFIRMED sin un REQUIRED vinculado")
    for x in trig:
        if x.get("Assessment") == "DISAGREE" and not has_req(x):
            c.append("Materiality.Triggers[%s] DISAGREE sin un REQUIRED vinculado" % x.get("Trigger"))
        if x.get("Assessment") == "AGREE" and has_req(x):
            c.append("Materiality.Triggers[%s] AGREE con un REQUIRED vinculado" % x.get("Trigger"))
    for x in qa:
        if x.get("Outcome") == "NOT_SATISFIED" and not linked(x):
            c.append("QuestionAnswers[%s] NOT_SATISFIED sin un hallazgo vinculado" % x.get("Question"))
    for x in od:
        if x.get("Status") == "DOES_NOT_CONFORM" and not linked(x):
            c.append("OptionalDispositionConformance[%s] DOES_NOT_CONFORM sin un hallazgo vinculado" % x.get("Round1Optional"))
    if owner is True and not orc.get("UnidentifiedConsequences"):
        c.append("OwnerDecisionRequired = true sin UnidentifiedConsequences")
    if owner is False and orc.get("UnidentifiedConsequences"):
        c.append("OwnerDecisionRequired = false con UnidentifiedConsequences")
    if changed is True and not str(ri.get("Note") or "").strip():
        c.append("RelatedImpacts.ChangedByRound2 = true sin Note")
    if v != "NOT_ACCREDITED":
        if any(step0.get(k) is not True for k in STEP0_KEYS):
            c.append("con un veredicto, las siete booleanas de Step0 van en true")
        if result.get("ReviewedCommit") != REV or result.get("ReviewedPath") != OBJECT or result.get("ReviewedBlob") != OBJECT_BLOB:
            c.append("objeto revisado distinto del designado (commit, ruta o blob)")
        for label, xs, key, want in arrays:
            got_ = [str(x.get(key)) for x in xs]
            if sorted(got_) != sorted(want):
                c.append("%s debe disponer cada id una vez (faltan %s; sobran o repetidos %s)"
                         % (label, [w for w in want if w not in got_], sorted({g for g in got_ if g not in want or got_.count(g) > 1})))
        wrong_decl = [str(x.get("Trigger")) for x in trig if x.get("Trigger") in MAT_A7 and x.get("A7Declared") != MAT_A7[x.get("Trigger")]]
        if wrong_decl:
            c.append("con un veredicto, A7Declared distinto de lo que declara A-7 §5 (%s)" % ", ".join(wrong_decl))
        for label, x in holders + [(lb, x) for lb, x in optional if x.get("Status") != "NOT_ASSESSED"]:
            if not x.get("ClauseRefs"):
                c.append("con un veredicto, %s sin ClauseRefs" % label)
        undetermined = [k for k, bad in (("DeltaPurity.Status", dp.get("Status") == NOT_DET), ("RelatedImpacts.ChangedByRound2", changed is None)) if bad]
        if undetermined:
            c.append("con un veredicto, NOT_DETERMINED o null no se admiten (%s)" % ", ".join(undetermined))
        if owner is None:
            c.append("con un veredicto, OwnerReservedConsequence.OwnerDecisionRequired no admite null")
        cited = set()
        for _, x in holders + optional:
            cited |= set(linked(x))
        uncited = [i for i in req_ids if i not in cited]
        if uncited:
            c.append("REQUIRED sin disposición: todo REQUIRED lo cita al menos un elemento evaluado (%s)" % ", ".join(str(i) for i in uncited))
    if v == "AGREED" and (req or owner is not False or not ia or not all(ia.get(k) is True for k in ia)):
        c.append("AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true")
    if v == "AGREED" and (len(rr) != len(ROUND1) or any(x.get("Status") != "CORRECTED" for x in rr)):
        c.append("AGREED exige A62-A7-R1, R2 y R3 en CORRECTED")
    if v == "AGREED" and (dp.get("Status") != "CONFIRMED" or len(ch) != len(CHANGES) or any(x.get("Assessment") != "WITHIN_R1_R3" for x in ch)):
        c.append("AGREED exige DeltaPurity CONFIRMED con los diez cambios WITHIN_R1_R3")
    if v == "AGREED" and (len(rg) != len(REGRESSION) or any(x.get("Status") != "NO_REGRESSION" for x in rg)):
        c.append("AGREED exige los cinco temas de RegressionCheck en NO_REGRESSION")
    if v == "AGREED" and (len(trig) != len(TRIGGERS) or any(x.get("Assessment") != "AGREE" for x in trig)):
        c.append("AGREED exige los ocho disparadores de Materiality en AGREE")
    if v != "AGREED" and any(x is not None for x in ia.values()):
        c.append("IfAgreed debe ir en null salvo con AGREED")
    if v in ("CHANGES REQUIRED", "REJECTED") and not req:
        c.append("%s sin ningún REQUIRED" % v)
    if v == BLOCKED and owner is not True:
        c.append("BLOCKED — OWNER DECISION sin OwnerDecisionRequired = true")
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
