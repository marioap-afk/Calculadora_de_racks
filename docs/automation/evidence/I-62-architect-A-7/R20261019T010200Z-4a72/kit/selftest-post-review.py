"""Self-test of post-review.py v5.5-a7, of transport_gate.py, of owner_gate.py (A-7), of pin_authorization.values and of c3_analysis.analyze, run
BEFORE the launch of the ONE formal review of amendment A-7, bounded to A7-1 and its consequences on the VERIFY input closure of FX-02/F6 and the
A6-7 measurement (decisions §93). Adapted from the self-test of the accredited kit of A-6 round 4 (R20261014T180556Z-5e02, prep/adapt_selftest.py):
the same synthetic formats and the same transport, read, custody, launcher and gate cases (00-55, 126-149, 152-153, G00-G58, X01-X04), the specifics
replaced by the A-7 closure (two run files, order.txt = the §93 body, and prompt.md; the object is A-7; the direct read is V14 L2231-L2233) and the
result schema rackcad-a7-architect-review/0, with:
  * cases 56-121 for the coherence and coverage rules of the A-7 schema (TopicVerdicts, Materiality with its Triggers and A7Declared,
    OwnerReservedConsequence, RelatedImpacts with ContractExclusionD1 and FailClosedProposals, QuestionAnswers, AGREED, CHANGES REQUIRED, REJECTED,
    BLOCKED, NOT_ACCREDITED, schema; 108b-108g are nested schema violations that only the auditor's in-process schema check detects, because the
    Test-Json of pwsh 7.6.7 validates only the first level of nesting), the direct read of V14 L2231-L2233 and the references of the nested elements;
  * cases 150-151p for the preflight record of gate 11 of A-7: the object blob and the agreed A-6 blob at the worktree HEAD and the authorization
    pinned in owner_gate.py (the Coordinator disposition §93, after §92); the audit cases use the kit's own pins (a SYNTHETIC §94 appended to the
    clone's decisions only if the kit's pins were pending); case 151p checks that a kit with the authorization pending cannot be accredited;
  * cases O00-O30 (owner_gate.evaluate, pure, on decision texts built from the clone's decisions, which end at §93: the decisions before §93 play
    the clone's text where a case isolates the authorization rule): the authorization pending (blocked with the single «authorization pending: …»
    reason), pinned on §93 (passes), a later section (blocked), the object and agreed A-6 blobs and the inherited checks; O30 exercises
    pin_authorization.values; and the live probe L00 (owner_gate.check on the real I-62 worktree, read-only git, with the module exactly as the kit
    holds it: pinned, it must pass with exactly §93 and no later section; pending, it must block with only the pending reason);
  * cases X01-X04: launch.py and the auditor agree (RUN_JSON_FIELDS, outcome_categories on a set of records, the frozen command and the environment
    prefixes); launch.py is read with ast, never imported (importing it would evaluate its frozen-command guard).

Environment. By default it audits against the kit's closure (kit/closure.json: clone = closure Transport.Cwd, run = closure RunDir): the real
clone D:\\r62-arch-a7 and run D:\\r62-arch-a7-run. With RACKCAD_A7_KITDATA=<dir> it takes closure.json, the gate modules and the kit copies of
the run files from <dir>; post-review.py and result.schema.json always come from this kit directory.
Synthetic claude-cli runs (transcript JSONL in the format measured by the characterization C1, stream-json stdout, and the <run>/launch files
launch.py writes: run.json, preflight.json, stderr.txt and the copies of the transport-gate evidence). Each case audits against a SYNTHETIC gate
(MEASURED by default, built from a c3_analysis.analyze record of a synthetic PASS probe; ACCEPTED in case 41) set through the auditor's GATE seam;
LAUNCH_DIR points to the case's temporary launch directory. The synthetic run.json gets its Outcome and OutcomeCategories from launch.py's own
outcome_categories (read with ast) unless the case sets them, so every case also checks that the auditor's recomputation agrees with the launcher.

Every temporary file lives under <kit>/../.tmp/st and is removed at the end (with a guard against links). The only write outside it is the
temporary junction of case 03 inside the clone (target: a directory of its own under .tmp/st), removed in `finally`; the clone must end clean and
the run files identical to the kit copies.
Usage: python -B selftest-post-review.py <out.json>
"""
import ast
import copy
import hashlib
import importlib.util
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
KITDATA = os.environ.get("RACKCAD_A7_KITDATA") or HERE
TMPROOT = os.path.join(os.path.dirname(HERE), ".tmp", "st")
os.makedirs(os.path.join(TMPROOT, "tmp"), exist_ok=True)
for k in ("TEMP", "TMP", "TMPDIR"):
    os.environ[k] = os.path.join(TMPROOT, "tmp")
tempfile.tempdir = os.path.join(TMPROOT, "tmp")
_CLOSURE = json.load(open(os.path.join(KITDATA, "closure.json"), encoding="utf-8"))
CLONE, RUN = _CLOSURE["Transport"]["Cwd"], _CLOSURE["RunDir"]
OTHER_CLONE = os.path.join(TMPROOT, "o")   # another tree: only ever a junction target or a path that must be classified OUTSIDE
os.makedirs(os.path.join(OTHER_CLONE, "docs", "initiatives"), exist_ok=True)
open(os.path.join(OTHER_CLONE, "docs", "initiatives", "I-62-A-3.md"), "w").write("# x\n")
WORKTREE = os.path.join(os.environ.get("USERPROFILE", ""), ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")   # L00, read-only
DEC = "docs/automation/decisions/I-62.md"
AP = "docs/AUTOMATION_PLAN.md"
LC = "docs/INITIATIVE_LIFECYCLE.md"
V14 = "docs/initiatives/I-62-proposal-v14.md"
A5 = "docs/initiatives/I-62-A-5.md"
A6 = "docs/initiatives/I-62-A-6.md"
A7 = _CLOSURE["ObjectPath"]
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ADR48 = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
EVID = "docs/automation/evidence/I-62-A7"
GRR, SUMMARY, PREFLIGHT_MD = EVID + "/a7-guards-result-run.json", EVID + "/summary.json", EVID + "/preflight.md"
PKG = "docs/automation/evidence/I-62-F6/FX-02/bmin"
SPEC = PKG + "/SPEC.md"
ACC = _CLOSURE["AcceptorPath"]
TOOLF = PKG + "/tool/bmin_facts.py"
OTHER_A7_BLOB = "a2aed3cc894037d6b42d462fbec9d6d0c7f629a6"   # un borrador de A-7 anterior al preflight (preflight.md): nunca el objeto
OTHER_A6_BLOB = "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1"   # la A-6 de la ronda 3: nunca la acordada
C3_ADD = r"D:\r62-c3-probe\add\canary-add.txt"
C3_OUT = r"D:\r62-c3-probe\out\canary-out.txt"
DELETE = object()
CLONE_MUTATED = False     # True while the junction of case 03 exists (the mutation harness bypasses its caches then)
BLOCKED = "BLOCKED " + chr(0x2014) + " OWNER DECISION"
IFAGREED_KEYS = ("ScopeLimitedToA71AndConsequences", "A71CompatibleWithAuthorities", "AllMandatoryTopicsConform", "MaterialityConfirmed",
                 "NoUnidentifiedOwnerReservedConsequence", "NoUnauthorizedChanges", "RelatedImpactsRevealNoA7Defect", "SuitableForCoordinatorAgreement")
V14_READ = (2223, 2241)                       # the direct read of Step 0 (L2231-L2233 inside)
AUTH_SECTION = 93                             # the Coordinator disposition pinned in the kit (decisions §93, after §92)
SYNTH_AUTH_SECTION = 94                       # synthetic disposition, only if the kit's pins were pending (never in the kit as delivered)
USER_RX = re.compile(r'(?i)[a-z]:(?:\\\\|\\|/)+users(?:\\\\|\\|/)+[^\\/"\s]+')


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


def load_auditor(path=None):
    pr = load("post_review_v5_5_a7", path or os.path.join(HERE, "post-review.py"))
    pr.setup(CLONE, RUN, KITDATA)
    return pr


def launch_definitions():
    """FROZEN_COMMAND, LAUNCH_ENV_PREFIXES, RUN_JSON_FIELDS and outcome_categories of kit/launch.py, read with ast. launch.py is never imported:
    importing it would evaluate its frozen-command guard and load kit-params.json."""
    tree = ast.parse(open(os.path.join(HERE, "launch.py"), encoding="utf-8").read())
    out = {}
    for node in tree.body:
        if isinstance(node, ast.Assign) and len(node.targets) == 1 and isinstance(node.targets[0], ast.Name) and \
                node.targets[0].id in ("FROZEN_COMMAND", "LAUNCH_ENV_PREFIXES", "RUN_JSON_FIELDS"):
            out[node.targets[0].id] = ast.literal_eval(node.value)
        if isinstance(node, ast.FunctionDef) and node.name == "outcome_categories":
            ns = {}
            exec(compile(ast.Module(body=[node], type_ignores=[]), "launch.py:outcome_categories", "exec"), ns)
            out["outcome_categories"] = ns["outcome_categories"]
    return out


PROBE = load("c3_analysis", os.path.join(HERE, "c3_analysis.py"))
PA = load("pin_authorization", os.path.join(KITDATA, "pin_authorization.py"))
LAUNCH = launch_definitions()
PINS = {}


def clone_decision_lines():
    L = list(PR.canon(DEC))
    while L and L[-1] == "":
        L.pop()
    return L


def synthetic_authorization(blob, run_id, number=SYNTH_AUTH_SECTION):
    """Lines of a SYNTHETIC Coordinator disposition after §93 (not a real disposition), appended to the clone's decisions by the self-test."""
    return ["## %d. Disposición del Coordinator: autorización de la revisión formal de A-7 (selftest)" % number, "",
            "Texto sintético de la autoprueba, no una disposición real: autoriza el lanzamiento único del kit %s sobre el blob %s." % (run_id, blob)]


def apply_pins(og):
    """While the kit's owner_gate.py held the authorization pending, the audit cases would need a pinned authorization: SYNTHETIC pins (§94 appended
    to the clone's decisions; values from pin_authorization.values) set on the auditor's owner_gate module object (never on the file). With the
    kit's pins set (§93, as delivered), they are used as they are."""
    global PINS
    if og.authorization_pending(og.pinned_authorization()):
        text = "\n".join(clone_decision_lines() + [""] + synthetic_authorization(PR.OBJECT_BLOB, PR.IDS["RunId"])) + "\n"
        v = PA.values(text, SYNTH_AUTH_SECTION, PR.OBJECT_BLOB)
        assert v["Ready"], v
        og.AUTH_SECTION, og.AUTH_HEADING, og.AUTH_TEXT_SHA = SYNTH_AUTH_SECTION, v["Values"]["AUTH_HEADING"], v["Values"]["AUTH_TEXT_SHA"]
        PINS = {"Source": "SYNTHETIC (owner_gate.py del kit con la autorización pendiente; §94 sintética añadida a las decisiones del clon)",
                "Pins": og.pinned_authorization()}
    else:
        PINS = {"Source": "KIT (autorización fijada en owner_gate.py)", "Pins": og.pinned_authorization()}


def init_globals(pr):
    global PR, TG, OG, SID, REV, MODEL, EFFORT, SCHEMA, PROMPT, PROMPT_BYTES, ORIG_READ_RUN, TEMPLATE, BINPATH, BINSHA, VERSION
    PR, TG, OG = pr, pr.TG, pr.OG
    SID, REV, MODEL, EFFORT = PR.SESSION_ID, PR.REV, PR.TRANSPORT["Model"], PR.TRANSPORT["Effort"]
    SCHEMA = os.path.join(HERE, "result.schema.json")
    PROMPT_BYTES = open(os.path.join(RUN, "prompt.md"), "rb").read()
    PROMPT = PROMPT_BYTES.decode("utf-8")
    ORIG_READ_RUN = PR.read_run_bytes
    TEMPLATE = TG.template_of(PR.TRANSPORT)
    BINPATH, BINSHA, VERSION = PR.TRANSPORT["Binary"]["Path"], PR.TRANSPORT["Binary"]["Sha256"], PR.TRANSPORT["Binary"]["Version"]
    apply_pins(OG)


def sha(b):
    return hashlib.sha256(b).hexdigest()


def jbytes(o):
    return (json.dumps(o, ensure_ascii=False, indent=1) + "\n").encode("utf-8")


def absp(rel):
    return CLONE + "\\" + rel.replace("/", "\\")


def numbered(lines, start=1):
    return "\n".join("%d\t%s" % (start + i, l) for i, l in enumerate(lines))


def lines_of(rel=None, run=None):
    L = PR.canon(rel) if rel else PR.run_file(run)
    return L[:-1] if L and L[-1] == "" else L


def find_line(rel, prefix):
    L = PR.canon(rel)
    hits = [i + 1 for i, l in enumerate(L) if l.startswith(prefix)]
    assert len(hits) == 1, (rel, prefix, hits)
    return hits[0], L[hits[0] - 1]


def closure_range(rel, section_prefix):
    """The closure range of `rel` whose label is exactly `section_prefix` or, if none is, the only one that starts with it."""
    r = next(x for x in PR.CLOSURE["CanonicalInputs"] if x["Path"] == rel)
    g = [x for x in r["LineRanges"] if x["Section"] == section_prefix] or [x for x in r["LineRanges"] if x["Section"].startswith(section_prefix)]
    assert len(g) == 1, (rel, section_prefix, g)
    return g[0]["Start"], g[0]["End"]


def ref(rel, a, b=None):
    return "%s:%d" % (rel, a) if b is None or b == a else "%s:%d-%d" % (rel, a, b)


def patch(d, p):
    for k, v in p.items():
        if v is DELETE:
            d.pop(k, None)
        else:
            d[k] = v
    return d


# ------------------------------------------------------------------ synthetic transport-gate evidence (unchanged from the round-3 kit)
def c3_stream(add_reply, out_reply, out_error=True, denial=True, add_error=False):
    msgs = [{"type": "system", "subtype": "init", "cwd": r"D:\r62-cli-char", "session_id": "c3", "tools": ["Glob", "Grep", "Read", "StructuredOutput"],
             "mcp_servers": [], "model": MODEL, "permissionMode": "dontAsk", "claude_code_version": VERSION.split()[0], "apiKeySource": "none"}]
    for i, (path, reply, err) in enumerate(((C3_ADD, add_reply, add_error), (C3_OUT, out_reply, out_error))):
        msgs.append({"type": "assistant", "message": {"model": MODEL, "content": [{"type": "tool_use", "id": "t%d" % i, "name": "Read",
                                                                                  "input": {"file_path": path}}]}, "parent_tool_use_id": None})
        msgs.append({"type": "user", "message": {"content": [{"type": "tool_result", "tool_use_id": "t%d" % i, "content": reply, "is_error": err}]},
                     "parent_tool_use_id": None})
    so = {"add_first_line": "C3-ADD-0011", "out_first_line": "DENEGADO", "tools_available": ["Glob", "Grep", "Read", "StructuredOutput"]}
    msgs.append({"type": "result", "subtype": "success", "is_error": False, "terminal_reason": "completed", "num_turns": 3, "structured_output": so,
                 "modelUsage": {MODEL: {}}, "permission_denials": ([{"tool_name": "Read", "tool_input": {"file_path": C3_OUT}}] if denial else [])})
    tx = [{"type": "assistant", "message": {"model": MODEL}, "effort": EFFORT} for _ in range(3)]
    return msgs, tx


def c3_record(kind="PASS"):
    add_reply, out_reply, kw = "1\tC3-ADD-0011", "Permission to read %s has been denied." % C3_OUT, {}
    if kind == "ADD_DENIED":
        add_reply, kw = "Claude requested permissions to read from %s, but you haven't granted it yet." % C3_ADD, {"add_error": True}
    if kind == "OUT_DELIVERED":
        out_reply, kw = "1\tC3-OUT-0022", {"out_error": False, "denial": False}
    stream, tx = c3_stream(add_reply, out_reply, **kw)
    rec = PROBE.analyze(stream, tx, C3_ADD, "C3-ADD-0011", C3_OUT, "C3-OUT-0022", MODEL, EFFORT)
    values = {"<add-dir>": r"D:\r62-c3-probe\add", "<session-id>": "c3c3c3c3-0000-4000-8000-000000000003"}
    proc = {"SessionId": values["<session-id>"], "Binary": {"Path": BINPATH, "Sha256": BINSHA, "Version": VERSION}, "ArgsTemplate": TEMPLATE,
            "Args": [values.get(x, x) for x in TEMPLATE], "Cwd": r"D:\r62-cli-char", "ExitCode": 0, "Killed": False, "TimedOut": False}
    return dict(proc, **rec)


def synthetic_char(with_c3=True):
    c1 = [{"<session-id>": "052a696f-6951-48a4-9692-ee8b0ebcae7c"}.get(x, x) for x in TG.without_add_dir(TEMPLATE)]
    char = {"Executables": [{"Path": BINPATH, "Version": VERSION, "Sha256": BINSHA, "Usable": True}],
            "Probes": {"C1": {"Args": c1, "ExitCode": 0}},
            "Eligibility": {"STALE": False, "ARCHITECT": "ELIGIBLE (selftest)"}}
    if with_c3:
        char["Probes"]["C3"] = c3_record()
        char["Eligibility"]["MeasuredArgsTemplate"] = list(TEMPLATE)
    return char


def synthetic_acceptance(char_sha):
    return {"Schema": TG.ACCEPTANCE_SCHEMA, "RunId": PR.IDS["RunId"], "Decision": "ACCEPT_UNMEASURED_FLAG", "UnmeasuredFlags": ["--add-dir"],
            "ArgsTemplate": list(TEMPLATE), "BinarySha256": BINSHA, "Version": VERSION, "CharacterizationSha256": char_sha,
            "AcceptedBy": "principal-session", "Authority": "selftest", "Rationale": "selftest", "At": "2026-10-14T00:00:00Z"}


def synthetic_gate(status, char_b, acc_b=None):
    g = TG.initial_gate(PR.CLOSURE)
    g.update({"Status": status, "Characterization": {"Path": "%TEMP%\\selftest\\characterization.json", "Sha256": sha(char_b)},
              "Acceptance": {"Path": "%TEMP%\\selftest\\acceptance.json", "Sha256": sha(acc_b)} if acc_b is not None else None})
    return g


def gate11_record():
    """The preflight record of gate 11 that launch.py writes when the gate passes (owner_gate.check, A-7: the object and agreed A-6 blobs at the
    worktree HEAD, the pinned authorization §93 after §92, exactly one such section and no later section)."""
    return {"Ok": True, "Problems": [], "A6LineSha256": OG.A6_LINE_SHA, "A6LineNumbers": [1572], "ClaudeCliLineSha256": OG.CLAUDE_LINE_SHA,
            "ClaudeCliLineNumbers": [1085], "ObjectBlob": PR.OBJECT_BLOB, "ObjectBlobAtHead": PR.OBJECT_BLOB, "ObjectBlobAtHeadMatches": True,
            "A6AgreedBlob": OG.A6_AGREED_BLOB, "A6BlobAtHead": OG.A6_AGREED_BLOB, "A6AgreedBlobAtHeadMatches": True,
            "RunId": PR.IDS["RunId"], "BaseSection": OG.BASE_SECTION, "AuthorizationPending": False, "Authorized": True,
            "AuthorizationSection": OG.AUTH_SECTION, "AuthorizationHeading": OG.AUTH_HEADING, "AuthorizationTextSha256": OG.AUTH_TEXT_SHA,
            "AuthorizationSections": [{"Number": OG.AUTH_SECTION, "Line": 1825, "Heading": OG.AUTH_HEADING, "HeadingMatches": True, "HasBlob": True,
                                       "TextSha256": OG.AUTH_TEXT_SHA, "TextSha256Matches": True}],
            "LaterSections": [], "SectionsAfterBase": [{"Number": OG.AUTH_SECTION, "Line": 1825, "Heading": OG.AUTH_HEADING}]}


def frozen_guard_record():
    """The FrozenCommandGuard record of launch.py for the frozen command (G)."""
    return {"Ok": True, "Problems": [], "FrozenCommand": PR.LAUNCH_COMMAND, "Enforced": True, "OrigArgvTail": ["-B", "kit/launch.py"],
            "ScriptArgResolvesToThisFile": True, "ScriptArgIsDocumentedSpelling": True, "Mode": [], "Flags": {"dont_write_bytecode": 1, "utf8_mode": 0},
            "XOptions": {}, "Interpreter": "%LOCALAPPDATA%\\Microsoft\\WindowsApps\\python.exe", "PythonVersion": "3.13.14", "Encodings": {}}


def environment_record():
    """The Environment record of launch.py (the launcher's environment record of round 3, §84, point 2)."""
    return {"Prefixes": list(PR.LAUNCH_ENV_PREFIXES), "Names": ["CLAUDECODE", "GIT_EDITOR", "PYTHONDONTWRITEBYTECODE"],
            "PythonValues": {"PYTHONDONTWRITEBYTECODE": "1"}, "SetOrStrippedByLauncher": [], "ChildEnvironment": "heredado sin cambios"}


# ------------------------------------------------------------------ synthetic run (the round-3 one, with the A-7 record of gate 11)
class Run:
    def __init__(self):
        self.tx, self.so, self.n, self.turns = [], [], 0, 0
        self.res_patch, self.run_patch, self.init_patch, self.so_tx_override = {}, {}, {}, None
        self.final_result, self.no_result, self.no_init = None, False, False
        self.first_prompt = PROMPT
        self.gate_mode, self.gate_patch, self.pf_hook, self.after_write = "MEASURED", {}, None, None
        self.drop_launch, self.extra_launch, self.stdout_outside, self.tx_filter = set(), {}, False, None

    def entry(self, typ, **kw):
        e = {"type": typ, "isSidechain": False, "sessionId": SID, "cwd": CLONE, "version": "2.1.299", "entrypoint": "sdk-cli"}
        e.update(kw)
        self.tx.append(e)
        return e

    def header(self):
        self.tx.append({"type": "queue-operation", "operation": "enqueue", "sessionId": SID, "content": self.first_prompt})
        self.entry("user", message={"role": "user", "content": self.first_prompt}, permissionMode="dontAsk", promptSource="sdk")
        for a in ({"type": "environment", "snapshot": {"workingDirectory": CLONE, "additionalWorkingDirectories": [RUN]}},
                  {"type": "model", "identity": {"modelId": MODEL}}, {"type": "date", "date": "2026-10-14"}):
            self.entry("attachment", attachment=a)

    def call(self, name, inp, out, is_error=False, cwd=CLONE, model=None, effort=None, result=True):
        self.n += 1
        self.turns += 1
        uid = "toolu_%03d" % self.n
        msg = {"model": model or MODEL, "role": "assistant", "type": "message", "content": [{"type": "tool_use", "id": uid, "name": name, "input": inp}]}
        self.entry("assistant", message=msg, effort=effort or EFFORT, cwd=cwd)
        self.so.append({"type": "assistant", "message": copy.deepcopy(msg), "parent_tool_use_id": None, "session_id": SID})
        if result:
            self.entry("user", message={"role": "user", "content": [{"type": "tool_result", "tool_use_id": uid, "content": out, "is_error": is_error}]},
                       cwd=cwd)
        return uid

    def final(self, result):
        self.final_result = result
        tx_input = self.so_tx_override if self.so_tx_override is not None else result
        uid = self.call("StructuredOutput", copy.deepcopy(tx_input), "Structured output provided successfully")
        self.tx.insert(len(self.tx) - 1, {"type": "attachment", "isSidechain": False, "sessionId": SID, "cwd": CLONE, "version": "2.1.299",
                                          "attachment": {"type": "structured_output", "data": copy.deepcopy(tx_input), "toolUseID": uid}})

    def write(self, d):
        launch = os.path.join(d, "launch")
        os.makedirs(launch, exist_ok=True)
        tx = [e for e in self.tx if self.tx_filter is None or self.tx_filter(e)]
        tpath = os.path.join(d, SID + ".jsonl")
        tbytes = "".join(json.dumps(e, ensure_ascii=False) + "\n" for e in tx).encode("utf-8")
        open(tpath, "wb").write(tbytes)
        init = {"type": "system", "subtype": "init", "cwd": CLONE, "session_id": SID, "tools": ["Glob", "Grep", "Read", "StructuredOutput"],
                "mcp_servers": [], "model": MODEL, "permissionMode": "dontAsk", "claude_code_version": "2.1.299", "apiKeySource": "none"}
        patch(init, self.init_patch)
        lines = ([] if self.no_init else [init]) + self.so
        if not self.no_result:
            res = {"type": "result", "subtype": "success", "is_error": False, "terminal_reason": "completed", "num_turns": self.turns, "session_id": SID,
                   "permission_denials": [], "structured_output": copy.deepcopy(self.final_result), "result": json.dumps(self.final_result, ensure_ascii=False),
                   "modelUsage": {MODEL: {"inputTokens": 1, "outputTokens": 1}}, "subagent_stats": {"spawned": 0}, "total_cost_usd": 0.0}
            patch(res, self.res_patch)
            lines.append(res)
        sbytes = "".join(json.dumps(o, ensure_ascii=False) + "\n" for o in lines).encode("utf-8")
        spath = os.path.join(d if self.stdout_outside else launch, "stdout.jsonl")
        open(spath, "wb").write(sbytes)
        open(os.path.join(launch, "stderr.txt"), "wb").write(b"")
        char = synthetic_char()
        char_b = jbytes(char)
        acc_b = jbytes(synthetic_acceptance(sha(char_b))) if self.gate_mode == "ACCEPTED" else None
        gate = synthetic_gate(self.gate_mode, char_b, acc_b)
        patch(gate, self.gate_patch)
        open(os.path.join(launch, TG.COPY_CHARACTERIZATION), "wb").write(char_b)
        if acc_b is not None:
            open(os.path.join(launch, TG.COPY_ACCEPTANCE), "wb").write(acc_b)
        checks = {k: {"Ok": True} for k in TG.PREFLIGHT_CHECKS}
        checks["3-Binary"].update({"Exists": True, "Path": BINPATH, "Sha256": BINSHA, "Authenticode": "Valid", "SignerIsAnthropic": True})
        checks["4-Version"].update({"Got": VERSION, "ReturnCode": 0, "Decoding": {"Ok": True, "Problems": [], "Bom": False}})
        checks["5-Auth"].update({"loggedIn": True, "authMethod": "claude.ai", "apiProvider": "firstParty", "ReturnCode": 0,
                                 "Decoding": {"Ok": True, "Problems": [], "Bom": False}})
        checks["10-TransportGate"].update({"Status": self.gate_mode, "LaunchArgsMatchTemplate": True, "CharacterizationSha256": sha(char_b),
                                          "AcceptanceSha256": sha(acc_b) if acc_b is not None else None, "Problems": []})
        checks[OG.CHECK] = gate11_record()
        pf = {"SessionId": SID, "At": "2026-10-14T00:00:00Z", "Checks": checks, "FrozenCommandGuard": frozen_guard_record(),
              "RecordProbe": {"Ok": True, "Problems": [], "RunAndRecordExit": 0, "Outcome": "COMPLETED"}, "Environment": environment_record()}
        if self.pf_hook:
            self.pf_hook(pf)
        open(os.path.join(launch, "preflight.json"), "wb").write(jbytes(pf))
        rj = {"SessionId": SID, "Executable": BINPATH, "ExecutableSha256": BINSHA, "Args": PR.expected_args(SCHEMA), "Cwd": CLONE,
              "PromptSha256": sha(PROMPT_BYTES), "PromptBytes": len(PROMPT_BYTES), "TimeoutSeconds": PR.TRANSPORT["TimeoutSeconds"],
              "TransportGate": {"Status": self.gate_mode, "CharacterizationSha256": sha(char_b), "AcceptanceSha256": sha(acc_b) if acc_b else None},
              "EnvironmentVariableNames": ["CLAUDECODE"], "AuthSettle": {"Seconds": 120, "WaitedSeconds": 120.0},
              "Start": "2026-10-14T00:00:00Z", "PostRunErrors": [], "Pid": 4242, "End": "2026-10-14T00:10:00Z", "ExitCode": 0, "Killed": False,
              "TimedOut": False, "TasklistQuery": {"ReturnCode": 0, "StdoutBytes": 89, "NonAsciiBytes": True}, "ProcessGoneAfter": True,
              "StdoutSha256": sha(sbytes), "StdoutLines": len(lines), "StderrBytes": 0, "ResultMessage": not self.no_result, "TransportFailure": None,
              "Transcript": {"Path": "x", "Exists": True, "Sha256": sha(tbytes)}, "CloneAfter": {"Head": REV, "StatusPorcelainIgnored": ""}}
        patch(rj, self.run_patch)
        if "Outcome" not in self.run_patch and "OutcomeCategories" not in self.run_patch:
            cats = LAUNCH["outcome_categories"](rj)      # the launcher's own rule (launch.py, read with ast)
            rj["OutcomeCategories"], rj["Outcome"] = cats, cats[0]
        open(os.path.join(launch, "run.json"), "wb").write(jbytes(rj))
        for n in self.drop_launch:
            os.remove(os.path.join(launch, n))
        for n, b in self.extra_launch.items():
            open(os.path.join(launch, n), "wb").write(b)
        if self.after_write:
            self.after_write(launch)
        return tpath, spath, launch, gate


# ------------------------------------------------------------------ the A-7 reads and the base result
def read_range(r, rel, a, b):
    r.call("Read", {"file_path": absp(rel), "offset": a, "limit": b - a + 1}, numbered(lines_of(rel)[a - 1:b], a))


def read_run_range(r, name, a, b):
    r.call("Read", {"file_path": RUN + "\\" + name, "offset": a, "limit": b - a + 1}, numbered(lines_of(run=name)[a - 1:b], a))


def v14_direct(r):
    read_range(r, V14, *V14_READ)


def step0(r, direct=True):
    if direct:
        v14_direct(r)
    r.call("Read", {"file_path": RUN + "\\order.txt"}, numbered(lines_of(run="order.txt")))
    read_range(r, DEC, *closure_range(DEC, "## 93. "))
    read_range(r, DEC, *closure_range(DEC, "§90, acuerdo registrado"))
    r.call("Read", {"file_path": absp(GRR)}, numbered(lines_of(GRR)))
    r.call("Read", {"file_path": absp(SUMMARY)}, numbered(lines_of(SUMMARY)))


def a7_reads(r):
    r.call("Read", {"file_path": absp(A7)}, numbered(lines_of(A7)))


def evidence_reads(r):
    read_range(r, PREFLIGHT_MD, 1, 60)


def acceptor_reads(r):
    read_range(r, ACC, 75, 90)
    read_range(r, ACC, 2790, len(lines_of(ACC)))


def a6_reads(r):
    read_range(r, A6, 500, 799)


def base_calls(r, direct=True):
    r.header()
    step0(r, direct)
    a7_reads(r)
    evidence_reads(r)
    acceptor_reads(r)
    a6_reads(r)


def stop_calls(r):
    r.header()
    step0(r)


def schema_keys(prop):
    schema = json.load(open(SCHEMA, encoding="utf-8"))
    return [k for k in schema["properties"][prop]["required"]]


def sec_ref(prefix):
    return ref(A7, *closure_range(A7, prefix))


def a7_ref(label):
    return ref(A7, *closure_range(A7, label))


def preflight_ref():
    return ref(PREFLIGHT_MD, 1, 3)


TOPIC_ROWS = {"T01_A71_VS_AUTHORITIES": "§3.2, A7-1 (texto de la regla)", "T02_CATEGORIES": "A7-1, punto 1", "T03_PROVENANCE_REVISION_IDENTITY_FIDELITY":
              "A7-1, punto 2", "T04_EFF_EFF1_B1_MAP_VALIDATION": "§3.1, Más allá de R7-A", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE": "A7-1, punto 5",
              "T06_CLAUSEMAP_COVERAGE_RULE": "§3.5, residuo declarado (clausemap)", "T07_MULTI_CATEGORY_ENTRIES": "A7-1, punto 1",
              "T08_COMPATIBILITY_A5_A6": "§6, A-6", "T09_NO_UNAUTHORIZED_CHANGES": "A7-1, punto 7",
              "T10_APPLICABILITY_N4_NC1_N5_N7": "§3.4, A6-7, reglas 5 y 7", "T11_NEGATIVES_AND_PREFLIGHT": "§6, Guardas y preflight"}


def base_result():
    ids = PR.IDS
    return {
        "Schema": "rackcad-a7-architect-review/0", "RunId": ids["RunId"], "InvocationId": ids["InvocationId"],
        "LogicalReviewRequestId": ids["LogicalReviewRequestId"], "AttemptSeq": ids["AttemptSeq"], "RequestedRole": "ARCHITECT",
        "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62", "ReviewedCommit": PR.REV, "ReviewedPath": PR.OBJECT, "ReviewedBlob": PR.OBJECT_BLOB,
        "ReviewerMode": "SEPARATE SESSION", "SamePersonAsAuthor": "selftest",
        "ReviewerDeclaredIdentity": {"Runtime": "x", "Model": "x", "Effort": "x", "SessionOrThread": "x"},
        "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["x"]},
        "Step0": dict([("Basis", "INVOKER_VERIFIED_CLONE")] + [(k, True) for k in schema_keys("Step0") if k not in ("Basis", "Note")] + [("Note", "x")]),
        "DirectReads": [{"Ref": ref(V14, 2231, 2233), "Purpose": "x"}],
        "Reads": ["x"], "Verdict": "AGREED", "RequiredFindings": [], "OptionalFindings": [],
        "TopicVerdicts": [{"Topic": t, "Status": "CONFORMS", "Rationale": "x", "ClauseRefs": [a7_ref(TOPIC_ROWS[t])], "FindingIds": []}
                          for t in PR.TOPICS],
        "Materiality": {"Triggers": [{"Trigger": m, "A7Declared": PR.MAT_A7[m], "Assessment": "AGREE", "Rationale": "x",
                                      "ClauseRefs": [a7_ref("§5, %s" % m)], "FindingIds": []} for m in PR.TRIGGERS],
                        "Rationale": "x", "ClauseRefs": [sec_ref("## 5. ")], "FindingIds": []},
        "OwnerReservedConsequence": {"OwnerDecisionRequired": False, "UnidentifiedConsequences": [], "Rationale": "x",
                                     "ClauseRefs": [a7_ref("§7, consecuencias reservadas al Owner")], "FindingIds": []},
        "RelatedImpacts": {"ContractExclusionD1": {"Status": "NO_CONCERN", "RevealsA7Defect": False, "Concerns": [], "Rationale": "x",
                                                   "ClauseRefs": [a7_ref("§3.5, lo que A-7 no hace")], "FindingIds": []},
                           "FailClosedProposals": [{"Proposal": f, "ChangesPassCriteria": False, "ExcludesValidC24Cases": False,
                                                    "ReliesOnImplementationCoincidence": False, "RevealsA7Defect": False, "Status": "NO_CONCERN",
                                                    "Rationale": "x", "ClauseRefs": [preflight_ref()], "FindingIds": []} for f in PR.FCS],
                           "Note": "x"},
        "QuestionAnswers": [{"Question": q, "Outcome": "SATISFIED", "Answer": "x", "ClauseRefs": [a7_ref("§8, %s" % q)], "FindingIds": []}
                            for q in PR.QUESTIONS],
        "IfAgreed": {k: True for k in IFAGREED_KEYS},
        "KnownLimitations": [], "RecommendedNextAction": "x"}


def tvd(res):
    return {d["Topic"]: d for d in res["TopicVerdicts"]}


def trd(res):
    return {d["Trigger"]: d for d in res["Materiality"]["Triggers"]}


def fcd(res):
    return {d["Proposal"]: d for d in res["RelatedImpacts"]["FailClosedProposals"]}


def qad(res):
    return {d["Question"]: d for d in res["QuestionAnswers"]}


def mat(res):
    return res["Materiality"]


def orc(res):
    return res["OwnerReservedConsequence"]


def d1(res):
    return res["RelatedImpacts"]["ContractExclusionD1"]


def req_finding(fid="A62-A7-R1", evidence=None):
    return {"FindingId": fid, "Section": "§3.2 A7-1, punto 2", "ViolatedInvariant": "x", "Counterexample": "",
            "Evidence": evidence or [a7_ref("A7-1, punto 2")], "WhyItMatters": "x", "CorrectionRequired": "x"}


def opt_finding(fid="A62-A7-O1"):
    return {"FindingId": fid, "Section": "§8", "Note": "x", "Evidence": [a7_ref("§8, Q-A7-01")], "SuggestedCorrection": "x"}


def not_agreed(res, verdict):
    res["Verdict"] = verdict
    res["IfAgreed"] = {k: None for k in IFAGREED_KEYS}


def changes_required(res, verdict="CHANGES REQUIRED"):
    """A coherent CHANGES REQUIRED (or REJECTED) skeleton: one REQUIRED that leaves T02_CATEGORIES in DOES_NOT_CONFORM."""
    not_agreed(res, verdict)
    res["RequiredFindings"] = [req_finding()]
    tvd(res)["T02_CATEGORIES"].update({"Status": "DOES_NOT_CONFORM", "FindingIds": ["A62-A7-R1"]})


def blocked(res):
    """A coherent BLOCKED — OWNER DECISION skeleton."""
    not_agreed(res, BLOCKED)
    orc(res).update({"OwnerDecisionRequired": True, "UnidentifiedConsequences": ["x"]})


def mklink(link, target):
    if os.path.lexists(link):
        os.rmdir(link)
    r = subprocess.run(["cmd", "/c", "mklink", "/J", link, target], capture_output=True)    # bytes: the console code page is not UTF-8
    return "junction" if r.returncode == 0 else None


def variant(name, extra=None, expected=(), result_patch=None, base=None, counts=None, require_coherent=False, schema_valid=True,
            run_hook=None, final=True):
    r = Run()
    if run_hook:
        run_hook(r)
    (base or base_calls)(r)
    if extra:
        extra(r)
    res = base_result()
    if result_patch:
        result_patch(res, r)
    if final:
        r.final(res)
    else:
        r.final_result = None
    d = os.path.join(TMPROOT, "t", name.split("-")[0])   # short: MAX_PATH
    if os.path.isdir(d):
        shutil.rmtree(d)
    tpath, spath, launch, gate = r.write(d)
    saved = (PR.LAUNCH_DIR, PR.GATE)
    PR.LAUNCH_DIR, PR.GATE = launch, gate
    try:
        out = PR.audit(tpath, spath, SCHEMA)
    finally:
        PR.LAUNCH_DIR, PR.GATE = saved
    got = out["Accreditation"]["Reasons"]
    missing = [e for e in expected if not any(e in g for g in got)]
    unexpected = [g for g in got if not any(e in g for e in expected)]
    bad_counts = {k: sum(1 for g in got if k in g) for k, n in (counts or {}).items() if sum(1 for g in got if k in g) != n}
    row = {"Case": name, "Expected": list(expected), "Got": [PR.userless(g) for g in got], "Missing": missing, "Unexpected": unexpected,
           "SchemaValid": out["Result"]["Check"]["SchemaValid"], "Accreditation": out["Accreditation"]["Status"],
           "Categories": out.get("Categories", {}).get("Execution")}
    if counts:
        row["ExpectedCounts"], row["BadCounts"] = counts, bad_counts
    ok = not missing and not unexpected and not bad_counts and out["Result"]["Check"]["SchemaValid"] == schema_valid
    if require_coherent:
        row["ResultCoherence"] = out["Result"]["Check"]["Coherence"]
        ok = ok and not row["ResultCoherence"]
    row["Result"] = "PASS" if ok else "FAIL"
    return row, out


# ------------------------------------------------------------------ audit cases
def audit_cases():
    global CLONE_MUTATED
    cases = []
    add = lambda rv: cases.append(rv[0])
    add(variant("00-base-acreditada", require_coherent=True))
    out_reason = lambda rel: "lectura fuera del cierre: %s" % PR.userless(PR.norm(absp(rel)))

    def outside_abs(r):
        r.call("Read", {"file_path": absp("docs/HANDOFF.md"), "offset": 1, "limit": 5}, numbered(["# HANDOFF"]))
        r.call("Read", {"file_path": os.path.expanduser("~") + "\\.claude\\projects\\D--Documentos-Codex-Calculadora-de-racks\\memory\\MEMORY.md"},
               "File does not exist.", is_error=True)
    add(variant("01-lectura-fuera-absoluta", outside_abs, [out_reason("docs/HANDOFF.md"), "lectura fuera del cierre (intento fallido"]))

    def outside_rel(r):
        r.call("Read", {"file_path": "docs/HANDOFF.md"}, numbered(["# HANDOFF"]))
        r.call("Read", {"file_path": "..\\r62-arch-a3\\docs\\initiatives\\I-62-A-3.md", "offset": 1, "limit": 3}, numbered(["# x"]))
        r.call("Read", {"file_path": "docs\\automation\\..\\..\\AGENTS.md"}, numbered(["# AGENTS"]))
        r.call("Read", {"file_path": A5, "offset": 319, "limit": 5}, numbered(lines_of(A5)[318:323], 319))
    add(variant("02-lectura-fuera-relativa", outside_rel, ["lectura fuera del cierre"], counts={"lectura fuera del cierre": 3}))

    link = os.path.join(CLONE, "zz-selftest-link")
    PR.universe_cache.clear()
    try:
        kind = mklink(link, OTHER_CLONE)
        CLONE_MUTATED = True

        def junction(r):
            r.call("Read", {"file_path": CLONE + "\\zz-selftest-link\\docs\\initiatives\\I-62-A-3.md", "offset": 1, "limit": 2}, numbered(["# x", ""]))
            r.call("Grep", {"pattern": "A-3", "path": CLONE + "\\zz-selftest-link\\docs\\initiatives\\I-62-A-3.md"}, "1:x")
            r.call("Glob", {"pattern": "zz-selftest-link/**/*.md"}, "No files found")
        if kind:
            add(variant("03-escape-por-union (%s)" % kind, junction,
                        ["lectura fuera del cierre", "Grep fuera del cierre", "Glob fuera del clon y del run", "el clon no quedó limpio",
                         "el clon tiene enlaces o uniones"]))
        else:
            cases.append({"Case": "03-escape-por-union", "Result": "FAIL", "Got": "no se pudo crear la unión de prueba"})
    finally:
        if os.path.lexists(link):
            os.rmdir(link)
        CLONE_MUTATED = False
        PR.universe_cache.clear()

    def grep_dirs(r):
        r.call("Grep", {"pattern": "MAP_INVALID", "path": CLONE + "\\docs"}, "docs/x.md")
        r.call("Grep", {"pattern": "x", "path": OTHER_CLONE}, "x")
        r.call("Grep", {"pattern": "x"}, "x")
        r.call("Grep", {"pattern": "x", "path": absp(A5), "glob": "*.md"}, "1:x")
        r.call("Grep", {"pattern": "x", "path": RUN}, "x")
    add(variant("04-grep-directorios-y-fuera", grep_dirs, ["Grep sobre un directorio", "Grep fuera del cierre", "Grep sin path", "Grep con glob o type"],
                counts={"Grep sobre un directorio": 2}))

    def glob_out(r):
        r.call("Glob", {"pattern": "**/*.md", "path": OTHER_CLONE}, "No files found")
        r.call("Glob", {"pattern": "C:/Users/*/.claude/projects/**/*.jsonl"}, "No files found")
        r.call("Glob", {"pattern": "docs/adr/*.md"}, absp("docs/adr/x.md"))
        r.call("Glob", {"pattern": "../r62-arch-a3/**"}, "No files found")
        r.call("Glob", {"pattern": A5}, absp("docs/HANDOFF.md"))
    add(variant("05-glob-fuera-del-cierre", glob_out, ["Glob fuera del clon y del run", "Glob alcanza", "Glob devolvió una ruta fuera del cierre"],
                counts={"Glob fuera del clon y del run": 3}))

    order = lines_of(run="order.txt")

    def accepted(r):
        r.call("Glob", {"pattern": SPEC}, absp(SPEC))
        r.call("Glob", {"pattern": "**/FX-02/bmin/SPEC.md"}, absp(SPEC))
        r.call("Glob", {"pattern": "*.txt", "path": RUN}, RUN + "\\order.txt")
        r.call("Glob", {"pattern": PKG + "/**"}, absp(SPEC))
        r.call("Glob", {"pattern": "**/*", "path": absp(EVID)}, absp(GRR))
        n, l = find_line(AP, "| `CanonicalInputs[]` |")
        r.call("Grep", {"pattern": "CanonicalInputs", "path": absp(AP), "output_mode": "content", "-n": True}, "%d:%s" % (n, l))
        r.call("Grep", {"pattern": "sin reintento", "path": RUN + "\\order.txt", "output_mode": "content", "-n": True}, "54:" + order[53])
    add(variant("06-glob-y-grep-aceptados", accepted))

    def denied(r):
        r.call("Read", {"file_path": r"D:\r62-fixture\A2\README.md"}, "Permission to read D:\\r62-fixture\\A2\\README.md has been denied.", is_error=True)
        r.res_patch["permission_denials"] = [{"tool_name": "Read", "tool_use_id": "toolu_010", "tool_input": {"file_path": r"D:\r62-fixture\A2\README.md"}}]
    add(variant("07-herramienta-denegada", denied, ["lectura fuera del cierre (intento fallido", "permiso denegado", "permission_denials: Read"]))

    def bash(r):
        r.call("Bash", {"command": "git -C D:/r62-arch-a7 rev-parse HEAD"}, "<tool_use_error>Error: No such tool available: Bash</tool_use_error>",
               is_error=True)
    add(variant("08-intento-de-bash", bash, ["herramienta no permitida (Bash)"]))

    def prompt_hook(r):
        r.first_prompt = PROMPT + "\n"
    add(variant("09-prompt-distinto", expected=["fidelidad del prompt"], run_hook=prompt_hook))

    def extra_user(r):
        r.entry("user", message={"role": "user", "content": "continúa con la revisión"})
    add(variant("10-mensaje-de-usuario-extra", extra_user, ["mensajes de usuario además del prompt"]))

    def wrong_model(r):
        r.call("Read", {"file_path": absp(A5), "offset": 1, "limit": 3}, numbered(lines_of(A5)[:3]), model="claude-sonnet-5", effort="high")
        r.res_patch["modelUsage"] = {MODEL: {}, "claude-haiku-4-5": {}}
    add(variant("11-modelo-y-effort-distintos", wrong_model, ["modelo observado distinto", "effort observado distinto", "modelUsage con modelos distintos",
                                                               "mensaje del asistente con modelo"]))

    try:
        PR.read_run_bytes = lambda n: ORIG_READ_RUN(n) + (b"\ntampered\n" if n == "order.txt" else b"")
        add(variant("12-archivo-del-run-alterado", expected=["archivo del run distinto del custodiado (copia del kit y RunFileHashes del cierre): order.txt"],
                    counts={"archivo del run distinto del custodiado": 1}))
    finally:
        PR.read_run_bytes = ORIG_READ_RUN

    def killed(r):
        r.call("Read", {"file_path": absp(V14), "offset": 2431, "limit": 20}, None, result=False)
        r.no_result = True
        r.run_patch.update({"ExitCode": 1, "Killed": True})
    add(variant("13-sin-result-y-salida-1", killed, ["sin mensaje result", "código de salida 1", "proceso terminado", "no hay resultado estructurado",
                                                     "llamada sin resultado", "lectura directa: DirectReads sin una entrada",
                                                     "Outcome de run.json 'REVIEW_PROCESS_FAILURE'"], final=False, schema_valid=False))

    a48, b48 = closure_range(ADR48, "Decisión, punto 6")

    def ref_unread(res, r):
        mat(res)["ClauseRefs"].append(ref(ADR48, a48, b48))
    add(variant("14-referencia-sobre-lineas-no-leidas", expected=["referencia sobre líneas no entregadas fielmente en Materiality (%s)"
                                                                  % ref(ADR48, a48, b48)], result_patch=ref_unread))

    def ref_bad(res, r):
        tvd(res)["T01_A71_VS_AUTHORITIES"]["ClauseRefs"] += ["prompt.md:1", "order.txt:99"]
    add(variant("15-referencia-a-prompt-o-fuera-de-rango", expected=["referencia no canónica en TopicVerdicts[T01_A71_VS_AUTHORITIES] (prompt.md:1)",
                                                                     "referencia no canónica en TopicVerdicts[T01_A71_VS_AUTHORITIES] (order.txt:99)"],
                result_patch=ref_bad))

    def stop(res, r):
        nds = {"Rationale": "no evaluado", "ClauseRefs": [], "FindingIds": []}
        res.update({"Verdict": "NOT_ACCREDITED", "Reads": [RUN + "\\order.txt"], "RequiredFindings": [], "OptionalFindings": [],
                    "TopicVerdicts": [], "QuestionAnswers": [],
                    "Materiality": dict(nds, Triggers=[]),
                    "OwnerReservedConsequence": dict(nds, OwnerDecisionRequired=None, UnidentifiedConsequences=[]),
                    "RelatedImpacts": {"ContractExclusionD1": dict(nds, Status="NOT_DETERMINED", RevealsA7Defect=None, Concerns=[]),
                                       "FailClosedProposals": [], "Note": ""},
                    "ReviewedCommit": None, "ReviewedBlob": None,
                    "KnownLimitations": ["Paso 0: el resultado `run` de las guardas no nombra la publicación como Head; revisión detenida"],
                    "IfAgreed": {k: None for k in IFAGREED_KEYS}})
        res["Step0"].update({"GuardsRunModeBaseHeadResult": False, "PreflightSummaryCandidateAndTotals": None})
    add(variant("16-parada-not-accredited-coherente", result_patch=stop, base=stop_calls, require_coherent=True))

    def agreed_with_required(res, r):
        res["RequiredFindings"] = [req_finding()]
        tvd(res)["T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"].update({"Status": "DOES_NOT_CONFORM", "FindingIds": ["A62-A7-R1"]})
    add(variant("17-agreed-con-un-required", result_patch=agreed_with_required, expected=["AGREED exige cero REQUIRED",
                                                                                          "AGREED exige los once temas de TopicVerdicts en CONFORMS"]))

    def cr_without_required(res, r):
        not_agreed(res, "CHANGES REQUIRED")
    add(variant("18-changes-required-sin-required", result_patch=cr_without_required, expected=["CHANGES REQUIRED sin ningún REQUIRED"]))

    def agreed_trigger_disagree(res, r):
        res["OptionalFindings"] = [opt_finding()]
        trd(res)["M-08"].update({"Assessment": "DISAGREE", "FindingIds": ["A62-A7-O1"]})
    add(variant("19-agreed-con-un-disparador-en-desacuerdo", result_patch=agreed_trigger_disagree,
                expected=["Materiality.Triggers[M-08] DISAGREE sin un REQUIRED vinculado", "AGREED exige los ocho disparadores de Materiality en AGREE"]))

    def na_no_reason(res, r):
        res.update({"Verdict": "NOT_ACCREDITED", "KnownLimitations": [], "IfAgreed": {k: None for k in IFAGREED_KEYS}})
    add(variant("20-not-accredited-sin-motivo", result_patch=na_no_reason, expected=["NOT_ACCREDITED sin el motivo en KnownLimitations"]))

    def so_mismatch(r):
        alt = base_result()
        alt["RecommendedNextAction"] = "otra cosa"
        r.so_tx_override = alt
    add(variant("21-structured-output-distinto", run_hook=so_mismatch, expected=["structured_output del result distinto"]))

    def sidechain(r):
        r.entry("assistant", isSidechain=True, effort=EFFORT, message={"model": MODEL, "role": "assistant", "content": [{"type": "text", "text": "x"}]})
        r.so.append({"type": "assistant", "message": {"model": MODEL, "content": []}, "parent_tool_use_id": "toolu_x", "session_id": SID})
        r.res_patch["subagent_stats"] = {"spawned": 1}
    add(variant("22-cadena-lateral-y-subagentes", sidechain, ["entradas de cadena lateral", "subagentes lanzados", "mensajes de un subagente"]))

    def launch_bad(r):
        args = PR.expected_args(SCHEMA)
        args.remove("--safe-mode")
        r.run_patch["Args"] = args
        r.init_patch.update({"tools": ["Bash", "Glob", "Grep", "Read", "StructuredOutput"], "permissionMode": "default",
                             "session_id": "00000000-0000-4000-8000-000000000000"})
    add(variant("23-lanzamiento-e-init-distintos", run_hook=launch_bad, expected=["argumentos del lanzamiento distintos", "herramientas del init distintas",
                                                                               "permissionMode del init distinto", "session_id del init distinto"]))

    def run_dir_reads(r):
        r.call("Read", {"file_path": RUN + "\\launch\\stdout.jsonl"}, "File does not exist.", is_error=True)
        r.call("Read", {"file_path": RUN}, "EISDIR: illegal operation on a directory, read", is_error=True)
        r.call("Read", {"file_path": CLONE + "\\docs"}, "EISDIR: illegal operation on a directory, read", is_error=True)
    add(variant("24-lecturas-del-run-y-de-directorios", run_dir_reads, ["lectura fuera del cierre (intento fallido", "lectura de un directorio"],
                counts={"lectura de un directorio": 2}))

    def ref_run(res, r):
        tvd(res)["T01_A71_VS_AUTHORITIES"]["ClauseRefs"] += ["order.txt:3-11", "order.txt:54"]
        mat(res)["ClauseRefs"].append("order.txt:1")
    add(variant("25-referencias-a-order-aceptadas", result_patch=ref_run, require_coherent=True))

    def failed_degraded(r):
        r.call("Read", {"file_path": absp(ADR48), "offset": a48, "limit": b48 - a48 + 1}, "File does not exist.", is_error=True)
        v = lines_of(V14)[2529:2532]
        r.call("Read", {"file_path": absp(V14), "offset": 2530, "limit": 3}, numbered([v[0], v[1] + " X", v[2]], 2530))

    def ref_failed(res, r):
        mat(res)["ClauseRefs"].append(ref(ADR48, a48))
    rv = variant("26-lectura-fallida-y-degradada", failed_degraded, ["referencia sobre líneas no entregadas fielmente en Materiality (%s)"
                                                                     % ref(ADR48, a48), "fidelidad: docs/initiatives/I-62-proposal-v14.md"],
                 result_patch=ref_failed)
    rv[0]["FailedReadRecorded"] = any(f["Path"] == ADR48 for f in rv[1]["Fidelity"]["FailedReadsNeverCredited"])
    if not rv[0]["FailedReadRecorded"]:
        rv[0]["Result"] = "FAIL"
    add(rv)

    def tx_identity(r):
        r.entry("system", sessionId="11111111-1111-4111-8111-111111111111", cwd=OTHER_CLONE, version="2.1.289", content="x")
    add(variant("27-transcripcion-sesion-cwd-version", tx_identity, ["transcripción: sesión distinta", "transcripción: cwd distinto",
                                                                     "transcripción: versión distinta"]))

    # ---- run.json (launch.py writes it in <run>/launch)
    def hook(**kw):
        def h(r):
            for k, v in kw.items():
                setattr(r, k, v) if not isinstance(getattr(r, k, None), (dict, set)) else getattr(r, k).update(v)
        return h
    rv = variant("28-run-json-ausente", run_hook=hook(drop_launch={"run.json"}), expected=["lanzamiento: falta run.json"])
    rv[0]["ExecutionCategory"] = rv[1]["Categories"]["Execution"]
    if rv[0]["ExecutionCategory"] != "RUN_JSON_MISSING":
        rv[0]["Result"] = "FAIL"
    add(rv)
    add(variant("29-run-json-sessionid", run_hook=hook(run_patch={"SessionId": "00000000-0000-4000-8000-000000000000"}),
                expected=["SessionId de run.json distinto"]))
    add(variant("30-run-json-ejecutable-ruta", run_hook=hook(run_patch={"Executable": r"%USERPROFILE%\.local\bin\claude.exe"}),
                expected=["ejecutable medido distinto del binario elegible"]))
    add(variant("31-run-json-ejecutable-sha", run_hook=hook(run_patch={"ExecutableSha256": "fd7f35ec7761195ab5ba4eff423e48a78a7849e78f60d93ec31256cdb1a9ec7e"}),
                expected=["ejecutable medido distinto del binario elegible"]))
    add(variant("32-run-json-cwd", run_hook=hook(run_patch={"Cwd": OTHER_CLONE}), expected=["cwd del proceso distinto del clon"]))
    add(variant("33-run-json-hashes", run_hook=hook(run_patch={"PromptSha256": "0" * 64, "StdoutSha256": "1" * 64, "Transcript": {"Sha256": "2" * 64}}),
                expected=["PromptSha256 de run.json distinto", "StdoutSha256 de run.json distinto", "Transcript.Sha256 de run.json distinto"]))

    # ---- preflight.json
    add(variant("34-preflight-ausente", run_hook=hook(drop_launch={"preflight.json"}), expected=["lanzamiento: falta preflight.json"]))

    def pf_checks(pf):
        pf["Checks"]["6-Clone"]["Ok"] = False
        del pf["Checks"]["9-CommandLine"]
    add(variant("35-preflight-comprobaciones", run_hook=hook(pf_hook=pf_checks), expected=["preflight: faltan comprobaciones ['9-CommandLine']",
                                                                                           "preflight: comprobaciones sin Ok: ['6-Clone']"]))

    def pf_measures(pf):
        pf["Checks"]["3-Binary"]["Sha256"] = "fd7f35ec7761195ab5ba4eff423e48a78a7849e78f60d93ec31256cdb1a9ec7e"
        pf["Checks"]["4-Version"]["Got"] = "2.1.270 (Claude Code)"
        pf["Checks"]["5-Auth"]["loggedIn"] = None
    add(variant("36-preflight-medidas", run_hook=hook(pf_hook=pf_measures), expected=["preflight: binario medido", "preflight: versión medida distinta",
                                                                                      "preflight: auth status sin loggedIn"]))

    def pf_path(pf):
        pf["Checks"]["3-Binary"]["Path"] = r"%USERPROFILE%\.local\bin\claude.exe"
    add(variant("36b-preflight-ruta-del-binario", run_hook=hook(pf_hook=pf_path), expected=["preflight: binario medido"]))

    def pf_gate(pf):
        pf["Checks"]["10-TransportGate"]["CharacterizationSha256"] = "3" * 64
        pf["SessionId"] = "00000000-0000-4000-8000-000000000000"
    add(variant("37-preflight-compuerta-y-sesion", run_hook=hook(pf_hook=pf_gate), expected=["preflight: compuerta de transporte distinta",
                                                                                             "preflight: SessionId distinto"]))

    # ---- transport gate (D-01)
    add(variant("38-compuerta-pendiente", run_hook=hook(gate_patch={"Status": "PENDING"}),
                expected=["compuerta de transporte: Status 'PENDING'", "preflight: compuerta de transporte distinta"]))

    def alter_copy(launch):
        p = os.path.join(launch, TG.COPY_CHARACTERIZATION)
        open(p, "ab").write(b" ")
    add(variant("39-compuerta-copia-alterada", run_hook=hook(after_write=alter_copy),
                expected=["compuerta de transporte: SHA-256 del registro de caracterización distinto"]))
    add(variant("40-compuerta-copia-ausente", run_hook=hook(drop_launch={TG.COPY_CHARACTERIZATION}),
                expected=["compuerta de transporte: falta el registro de caracterización"]))
    add(variant("41-compuerta-aceptada-acreditada", run_hook=hook(gate_mode="ACCEPTED")))

    def bad_acceptance(launch):
        p = os.path.join(launch, TG.COPY_ACCEPTANCE)
        a = json.load(open(p, encoding="utf-8"))
        a["AcceptedBy"] = "reviewer"
        open(p, "wb").write(jbytes(a))
    add(variant("42-compuerta-aceptacion-invalida", run_hook=hook(gate_mode="ACCEPTED", after_write=bad_acceptance),
                expected=["compuerta de transporte: SHA-256 del registro de aceptación distinto", "compuerta de transporte: AcceptedBy distinto"]))

    # ---- stream-json init and result
    add(variant("43-init-ausente", run_hook=hook(no_init=True), expected=["stream-json: falta el mensaje init"]))
    add(variant("44-init-cwd-modelo-version-mcp", run_hook=hook(init_patch={"cwd": OTHER_CLONE, "model": "claude-sonnet-5",
                                                                            "claude_code_version": "2.1.289", "mcp_servers": [{"name": "github"}]}),
                expected=["cwd del init distinto", "modelo del init distinto", "versión del init distinta", "servidores MCP en el init"]))
    add(variant("45-result-no-success", run_hook=hook(res_patch={"subtype": "error_max_turns", "is_error": True}), expected=["el result no es success"]))
    add(variant("46-result-session-id", run_hook=hook(res_patch={"session_id": "00000000-0000-4000-8000-000000000000"}),
                expected=["session_id del result distinto"]))
    add(variant("47-result-sin-structured-output", run_hook=hook(res_patch={"structured_output": DELETE}),
                expected=["el result no trae structured_output", "no hay resultado estructurado", "lectura directa: DirectReads sin una entrada"],
                schema_valid=False))

    # ---- transcript
    n_refs = len(PR.collect_refs(base_result()))
    add(variant("48-transcripcion-sin-asistente", run_hook=hook(tx_filter=lambda e: e.get("type") != "assistant"),
                expected=["ningún mensaje del asistente", "structured_output del result distinto", "referencia sobre líneas no entregadas fielmente",
                          "lectura directa: docs/initiatives/I-62-proposal-v14.md:2231-2233 no entregadas"],
                counts={"referencia sobre líneas no entregadas fielmente": n_refs}))
    add(variant("49-transcripcion-sin-primer-usuario",
                run_hook=hook(tx_filter=lambda e: not (e.get("type") == "user" and isinstance((e.get("message") or {}).get("content"), str))),
                expected=["transcripción: sin primer mensaje de usuario"]))

    def glob_dotdot(r):
        r.call("Glob", {"pattern": "docs/*/../../" + SPEC}, absp(SPEC))
    add(variant("50-glob-puntos-tras-comodin", glob_dotdot, ["Glob con `..` después de un comodín"]))

    # ---- custody of the run and launch dirs, and the clone
    orig_list = PR.list_dir
    try:
        PR.list_dir = lambda p: (orig_list(p) or []) + ["intruso.txt", "launch"] if PR.real(p) == PR.RUN_REAL else orig_list(p)
        add(variant("51-run-dir-entrada-ajena", expected=["directorio del run con entradas ajenas al run: ['intruso.txt']"]))
    finally:
        PR.list_dir = orig_list
    add(variant("52-launch-archivo-inesperado", run_hook=hook(extra_launch={"notas.txt": b"x"}), expected=["launch/ con archivos inesperados: ['notas.txt']"]))
    add(variant("53-stdout-fuera-de-launch", run_hook=hook(stdout_outside=True), expected=["lanzamiento: stdout.jsonl fuera de"]))
    orig_git = PR.clone_git
    try:
        PR.clone_git = lambda *a: ("0" * 40 + "\n") if a == ("rev-parse", "HEAD:" + A7) else orig_git(*a)
        add(variant("54-clon-blob-cambiado", expected=["el clon no conserva los blobs designados del cierre"]))
    finally:
        PR.clone_git = orig_git

    # ---- result schema and coherence (A-7)
    def extra_field(res, r):
        res["Extra"] = 1
    add(variant("55-esquema-invalido", result_patch=extra_field, expected=["resultado: no valida contra result.schema.json"], schema_valid=False))

    def p(name, fn, expected=(), counts=None, schema_valid=True, coherent=False, run_hook=None, base=None, extra=None):
        add(variant(name, extra=extra, result_patch=lambda res, r: fn(res), expected=expected, counts=counts, schema_valid=schema_valid,
                    require_coherent=coherent, run_hook=run_hook, base=base))

    def cr(extra, verdict="CHANGES REQUIRED"):
        def f(res):
            changes_required(res, verdict)
            extra(res)
        return f

    def bl(extra):
        def f(res):
            blocked(res)
            extra(res)
        return f

    R1, O1 = "A62-A7-R1", "A62-A7-O1"
    T1, T2, T3 = "T01_A71_VS_AUTHORITIES", "T02_CATEGORIES", "T03_PROVENANCE_REVISION_IDENTITY_FIDELITY"
    D1L, FCL = "RelatedImpacts.ContractExclusionD1", "RelatedImpacts.FailClosedProposals"

    def r1_to_t3(res):
        """Move the REQUIRED link from T02 to T03 (so «REQUIRED sin disposición» does not fire)."""
        tvd(res)[T2]["FindingIds"] = []
        tvd(res)[T3].update({"Status": "DOES_NOT_CONFORM", "FindingIds": [R1]})
    p("56-tema-no-conforme-sin-required", cr(r1_to_t3), ["TopicVerdicts[T02_CATEGORIES] DOES_NOT_CONFORM sin un REQUIRED vinculado"])
    p("57-tema-conforme-con-required", cr(lambda res: tvd(res)["T04_EFF_EFF1_B1_MAP_VALIDATION"].update({"FindingIds": [R1]})),
      ["TopicVerdicts[T04_EFF_EFF1_B1_MAP_VALIDATION] CONFORMS con un REQUIRED vinculado"])
    p("58-disparador-en-desacuerdo-sin-required", cr(lambda res: trd(res)["M-03"].update({"Assessment": "DISAGREE"})),
      ["Materiality.Triggers[M-03] DISAGREE sin un REQUIRED vinculado"])
    p("59-disparador-de-acuerdo-con-required", cr(lambda res: trd(res)["M-01"].update({"FindingIds": [R1]})),
      ["Materiality.Triggers[M-01] AGREE con un REQUIRED vinculado"])
    p("60-declaracion-de-a7-mal-copiada", lambda res: trd(res)["M-08"].update({"A7Declared": "yes"}),
      ["con un veredicto, A7Declared distinto de lo que declara A-7 §5 (M-08)"])
    p("61-pregunta-no-satisfecha-sin-hallazgo", cr(lambda res: qad(res)["Q-A7-02"].update({"Outcome": "NOT_SATISFIED"})),
      ["QuestionAnswers[Q-A7-02] NOT_SATISFIED sin un hallazgo vinculado"])
    p("62-decision-del-owner-sin-consecuencias", bl(lambda res: orc(res).update({"UnidentifiedConsequences": []})),
      ["OwnerDecisionRequired = true sin UnidentifiedConsequences"])
    p("63-sin-decision-del-owner-con-consecuencias", lambda res: orc(res).update({"UnidentifiedConsequences": ["x"]}),
      ["OwnerDecisionRequired = false con UnidentifiedConsequences"])
    p("64-blocked-sin-decision-del-owner", lambda res: not_agreed(res, BLOCKED), ["BLOCKED — OWNER DECISION sin OwnerDecisionRequired = true"])
    p("65-d1-revela-defecto-sin-required", cr(lambda res: d1(res).update({"RevealsA7Defect": True})),
      [D1L + " RevealsA7Defect = true sin un REQUIRED vinculado"])
    p("66-d1-sin-defecto-con-required", cr(lambda res: d1(res).update({"FindingIds": [R1]})),
      [D1L + " RevealsA7Defect = false con un REQUIRED vinculado"])
    p("67-fc-revela-defecto-sin-required", cr(lambda res: fcd(res)["FC-10"].update({"RevealsA7Defect": True})),
      [FCL + "[FC-10] RevealsA7Defect = true sin un REQUIRED vinculado"])
    p("68-fc-sin-defecto-con-required", cr(lambda res: fcd(res)["FC-18"].update({"FindingIds": [R1]})),
      [FCL + "[FC-18] RevealsA7Defect = false con un REQUIRED vinculado"])
    p("69-d1-preocupaciones-sin-lista", lambda res: d1(res).update({"Status": "CONCERNS_FOR_COORDINATOR"}),
      [D1L + " CONCERNS_FOR_COORDINATOR sin Concerns"])
    p("70-d1-sin-preocupacion-con-lista", lambda res: d1(res).update({"Concerns": ["x"]}), [D1L + " NO_CONCERN con Concerns"])
    p("71-fc-sin-preocupacion-con-bandera", lambda res: fcd(res)["FC-01"].update({"ChangesPassCriteria": True}), [FCL + "[FC-01] NO_CONCERN con"])
    p("72-fc-preocupacion-sin-bandera", lambda res: fcd(res)["FC-04"].update({"Status": "CONCERNS_FOR_COORDINATOR"}),
      [FCL + "[FC-04] CONCERNS_FOR_COORDINATOR sin ninguna de"])

    def ag_concerns(res):
        d1(res).update({"Status": "CONCERNS_FOR_COORDINATOR", "Concerns": ["x"]})
        fcd(res)["FC-13"].update({"ExcludesValidC24Cases": True, "Status": "CONCERNS_FOR_COORDINATOR"})
        fcd(res)["FC-16"].update({"ReliesOnImplementationCoincidence": True, "Status": "CONCERNS_FOR_COORDINATOR"})
    p("73-agreed-con-preocupaciones-sin-defecto-aceptado", ag_concerns, coherent=True)
    p("74-ifagreed-no-nulo-sin-agreed", cr(lambda res: res["IfAgreed"].update({"SuitableForCoordinatorAgreement": True})),
      ["IfAgreed debe ir en null salvo con AGREED"])
    p("75-agreed-con-ifagreed-falso", lambda res: res["IfAgreed"].update({"MaterialityConfirmed": False}),
      ["AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true"])
    p("76-no-determinado-con-veredicto", cr(lambda res: d1(res).update({"Status": "NOT_DETERMINED", "RevealsA7Defect": None})),
      ["con un veredicto, NOT_DETERMINED o null no se admiten (RelatedImpacts.ContractExclusionD1.Status, "
       "RelatedImpacts.ContractExclusionD1.RevealsA7Defect)"])
    p("77-decision-del-owner-nula-con-veredicto", cr(lambda res: orc(res).update({"OwnerDecisionRequired": None})),
      ["OwnerReservedConsequence.OwnerDecisionRequired no admite null"])
    p("78-paso0-falso-con-veredicto", lambda res: res["Step0"].update({"Decisions93DeclaresOrderBytesAndSha256": False}),
      ["las siete booleanas de Step0 van en true"])
    p("79-paso0-nulo-con-veredicto", lambda res: res["Step0"].update({"V14DirectReadB9L2231L2233": None}), ["las siete booleanas de Step0 van en true"])
    p("80-objeto-otro-blob", lambda res: res.update({"ReviewedBlob": OTHER_A7_BLOB}), ["objeto revisado distinto del designado"])
    p("81-objeto-otro-commit", lambda res: res.update({"ReviewedCommit": PR.CLOSURE["Publication"]["Parent"]}), ["objeto revisado distinto del designado"])
    p("82-ids-de-invocacion-de-otra-revision", lambda res: res.update({"RunId": "R20261014T180556Z-5e02"}), ["identificadores de la invocación distintos"])

    def no_refs(res):
        tvd(res)[T1]["ClauseRefs"] = []
        qad(res)["Q-A7-03"]["ClauseRefs"] = []
        trd(res)["M-02"]["ClauseRefs"] = []
        fcd(res)["FC-24"]["ClauseRefs"] = []
        mat(res)["ClauseRefs"] = []
        orc(res)["ClauseRefs"] = []
        d1(res)["ClauseRefs"] = []
    p("83-elementos-sin-referencias", no_refs, ["con un veredicto, TopicVerdicts[T01_A71_VS_AUTHORITIES] sin ClauseRefs",
                                                "con un veredicto, QuestionAnswers[Q-A7-03] sin ClauseRefs",
                                                "con un veredicto, Materiality.Triggers[M-02] sin ClauseRefs",
                                                "con un veredicto, RelatedImpacts.FailClosedProposals[FC-24] sin ClauseRefs",
                                                "con un veredicto, Materiality sin ClauseRefs",
                                                "con un veredicto, OwnerReservedConsequence sin ClauseRefs",
                                                "con un veredicto, RelatedImpacts.ContractExclusionD1 sin ClauseRefs"])

    def fid_missing(res):
        tvd(res)["T06_CLAUSEMAP_COVERAGE_RULE"]["FindingIds"] = ["A62-A7-O7"]
        trd(res)["M-04"]["FindingIds"] = ["A62-A7-O9"]
        qad(res)["Q-A7-01"]["FindingIds"] = ["A62-A7-O8"]
        d1(res)["FindingIds"] = ["A62-A7-O6"]
        fcd(res)["FC-12"]["FindingIds"] = ["A62-A7-O5"]
    p("84-findingids-inexistentes", fid_missing, ["TopicVerdicts[T06_CLAUSEMAP_COVERAGE_RULE] cita en FindingIds hallazgos que no existen",
                                                  "Materiality.Triggers[M-04] cita en FindingIds hallazgos que no existen",
                                                  "QuestionAnswers[Q-A7-01] cita en FindingIds hallazgos que no existen",
                                                  D1L + " cita en FindingIds hallazgos que no existen",
                                                  FCL + "[FC-12] cita en FindingIds hallazgos que no existen"])
    p("85-ids-de-hallazgo-repetidos", lambda res: res.update({"OptionalFindings": [opt_finding(), opt_finding()]}),
      ["identificadores de hallazgo repetidos"])
    p("86-required-sin-invariante-ni-contraejemplo", cr(lambda res: res["RequiredFindings"][0].update({"ViolatedInvariant": " ", "Counterexample": ""})),
      ["A62-A7-R1 REQUIRED sin ViolatedInvariant ni Counterexample"])
    p("87-required-sin-evidencia", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": []})),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)
    p("88-evidencia-fuera-del-cierre", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": ["docs/HANDOFF.md:1"]})),
      ["referencia no canónica en RequiredFindings[A62-A7-R1] (%s:1)" % PR.userless(PR.norm(absp("docs/HANDOFF.md")))])
    p("89-evidencia-sobre-lineas-no-leidas", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": [ref(A6, 10, 12)]})),
      ["referencia sobre líneas no entregadas fielmente en RequiredFindings[A62-A7-R1] (%s)" % ref(A6, 10, 12)])
    p("90-evidencia-opcional-no-leida", lambda res: res.update({"OptionalFindings": [dict(opt_finding(), Evidence=[ref(V14, 2408)])]}),
      ["referencia sobre líneas no entregadas fielmente en OptionalFindings[A62-A7-O1]"])
    p("91-required-sin-disposicion", cr(lambda res: res["RequiredFindings"].append(req_finding("A62-A7-R2"))),
      ["REQUIRED sin disposición: todo REQUIRED lo cita al menos un elemento evaluado (A62-A7-R2)"])

    def agreed_with(update):
        def f(res):
            res["RequiredFindings"] = [req_finding()]
            update(res)
        return f
    AG0 = "AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true"
    p("92-agreed-tema-no-conforme", agreed_with(lambda res: tvd(res)["T09_NO_UNAUTHORIZED_CHANGES"].update({"Status": "DOES_NOT_CONFORM",
                                                                                                         "FindingIds": [R1]})),
      [AG0, "AGREED exige los once temas de TopicVerdicts en CONFORMS"])
    p("93-agreed-disparador-en-desacuerdo", agreed_with(lambda res: trd(res)["M-05"].update({"Assessment": "DISAGREE", "FindingIds": [R1]})),
      [AG0, "AGREED exige los ocho disparadores de Materiality en AGREE"])
    p("94-agreed-impacto-revela-defecto", agreed_with(lambda res: fcd(res)["FC-16"].update({"RevealsA7Defect": True, "FindingIds": [R1]})),
      [AG0, "AGREED exige RevealsA7Defect = false en RelatedImpacts"])

    def agreed_owner(res):
        orc(res).update({"OwnerDecisionRequired": True, "UnidentifiedConsequences": ["x"]})
    p("95-agreed-con-decision-del-owner", agreed_owner, [AG0])

    def cr_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        tvd(res)[T1].update({"Status": "DOES_NOT_CONFORM", "FindingIds": [R1, O1]})
        tvd(res)["T06_CLAUSEMAP_COVERAGE_RULE"].update({"FindingIds": [O1]})
        trd(res)["M-06"].update({"Assessment": "DISAGREE", "FindingIds": [R1]})
        d1(res).update({"Status": "CONCERNS_FOR_COORDINATOR", "Concerns": ["x"], "RevealsA7Defect": True, "FindingIds": [R1]})
        fcd(res)["FC-24"].update({"ChangesPassCriteria": True, "Status": "CONCERNS_FOR_COORDINATOR", "RevealsA7Defect": True, "FindingIds": [R1]})
        fcd(res)["FC-12"].update({"ExcludesValidC24Cases": True, "Status": "CONCERNS_FOR_COORDINATOR"})
        qad(res)["Q-A7-05"].update({"Outcome": "NOT_SATISFIED", "FindingIds": [O1]})
    p("96-changes-required-coherente-aceptado", cr(cr_ok), coherent=True)
    p("97-rejected-coherente-aceptado", cr(lambda res: trd(res)["M-07"].update({"Assessment": "DISAGREE", "FindingIds": [R1]}), "REJECTED"), coherent=True)
    p("98-rejected-sin-required", lambda res: not_agreed(res, "REJECTED"), ["REJECTED sin ningún REQUIRED"])
    p("99-blocked-coherente-aceptado", bl(lambda res: None), coherent=True)

    def ag_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        qad(res)["Q-A7-04"].update({"Outcome": "NOT_SATISFIED", "FindingIds": [O1]})
        tvd(res)["T07_MULTI_CATEGORY_ENTRIES"]["FindingIds"] = [O1]
    p("100-agreed-con-un-opcional-aceptado", ag_ok, coherent=True)
    p("101-id-de-hallazgo-mal-formado", lambda res: res.update({"OptionalFindings": [dict(opt_finding(), FindingId="A62-A7-1")]}),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)

    def a6_space_fid(res):
        res["RequiredFindings"][0]["FindingId"] = "A62-A6R4-R2"
        tvd(res)[T2]["FindingIds"] = ["A62-A6R4-R2"]
    p("102-id-nuevo-con-el-espacio-de-a6", cr(a6_space_fid), ["resultado: no valida contra result.schema.json"], schema_valid=False)
    p("103-referencia-absoluta", lambda res: mat(res).update({"ClauseRefs": [absp(A7) + ":" + str(closure_range(A7, "## 5. ")[0])]}),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)
    p("104-referencia-mal-formada", lambda res: mat(res).update({"ClauseRefs": ["order.txt"]}),
      ["referencia mal formada en Materiality", "resultado: no valida contra result.schema.json"], schema_valid=False)
    p("105-esquema-estados-invalidos", lambda res: (tvd(res)[T1].update({"Status": "MAYBE"}), trd(res)["M-01"].update({"Assessment": "OK"}),
                                                    fcd(res)["FC-01"].update({"Status": "IGNORED"})),
      ["resultado: no valida contra result.schema.json", "AGREED exige los once temas de TopicVerdicts en CONFORMS",
       "AGREED exige los ocho disparadores de Materiality en AGREE"], schema_valid=False)
    p("106-esquema-booleano-como-texto", lambda res: d1(res).update({"RevealsA7Defect": "false"}),
      ["resultado: no valida contra result.schema.json", "AGREED exige RevealsA7Defect = false en RelatedImpacts"], schema_valid=False)
    p("107-esquema-veredicto-inexistente", lambda res: not_agreed(res, "ACCEPTED"), ["resultado: no valida contra result.schema.json"],
      schema_valid=False)
    # nested schema violations that Test-Json of pwsh 7.6.7 does not see (only the in-process check of the auditor does)
    p("108b-esquema-propiedad-sobrante-anidada", lambda res: res["Step0"].update({"Extra": True}), ["resultado: no valida contra result.schema.json"],
      schema_valid=False)
    p("108c-esquema-campo-requerido-ausente-anidado", lambda res: tvd(res)[T1].pop("Rationale"), ["resultado: no valida contra result.schema.json"],
      schema_valid=False)
    p("108d-esquema-texto-vacio-anidado", lambda res: mat(res).update({"Rationale": ""}), ["resultado: no valida contra result.schema.json"],
      schema_valid=False)
    p("108e-esquema-elemento-de-lista-no-texto", lambda res: d1(res).update({"Concerns": [1]}),
      ["resultado: no valida contra result.schema.json", D1L + " NO_CONCERN con Concerns"], schema_valid=False)
    p("108f-esquema-null-no-admitido-anidado", lambda res: orc(res).update({"Rationale": None}),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)
    p("108g-esquema-propiedad-sobrante-doble-anidada", lambda res: fcd(res)["FC-04"].update({"X": 1}),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)

    # ---- coverage of the A-7 arrays
    def drop(path, field, value):
        def f(res):
            holder = res if len(path) == 1 else res[path[0]]
            holder[path[-1]] = [x for x in holder[path[-1]] if x[field] != value]
        return f

    def dup(path, field, value, replace):
        def f(res):
            holder = res if len(path) == 1 else res[path[0]]
            items_ = holder[path[-1]]
            src = next(x for x in items_ if x[field] == value)
            holder[path[-1]] = [copy.deepcopy(src) if x[field] == replace else x for x in items_]
        return f
    p("109-tema-ausente", drop(["TopicVerdicts"], "Topic", "T11_NEGATIVES_AND_PREFLIGHT"),
      ["TopicVerdicts debe disponer cada id una vez (faltan ['T11_NEGATIVES_AND_PREFLIGHT']", "AGREED exige los once temas de TopicVerdicts"])
    p("109b-tema-repetido", dup(["TopicVerdicts"], "Topic", T1, T2),
      ["TopicVerdicts con identificadores repetidos", "TopicVerdicts debe disponer cada id una vez"])
    p("110-disparador-ausente", drop(["Materiality", "Triggers"], "Trigger", "M-07"),
      ["Materiality.Triggers debe disponer cada id una vez (faltan ['M-07']", "AGREED exige los ocho disparadores de Materiality"])
    p("110b-disparador-repetido", dup(["Materiality", "Triggers"], "Trigger", "M-01", "M-06"),
      ["Materiality.Triggers con identificadores repetidos", "Materiality.Triggers debe disponer cada id una vez"])
    p("111-propuesta-ausente", drop(["RelatedImpacts", "FailClosedProposals"], "Proposal", "FC-18"),
      ["RelatedImpacts.FailClosedProposals debe disponer cada id una vez (faltan ['FC-18']", "AGREED exige RevealsA7Defect = false en RelatedImpacts"])
    p("111b-propuesta-repetida", dup(["RelatedImpacts", "FailClosedProposals"], "Proposal", "FC-01", "FC-04"),
      ["RelatedImpacts.FailClosedProposals con identificadores repetidos", "RelatedImpacts.FailClosedProposals debe disponer cada id una vez"])
    p("112-pregunta-ausente", drop(["QuestionAnswers"], "Question", "Q-A7-06"),
      ["QuestionAnswers debe disponer cada id una vez (faltan ['Q-A7-06']"])
    p("112b-pregunta-repetida", dup(["QuestionAnswers"], "Question", "Q-A7-01", "Q-A7-02"),
      ["QuestionAnswers con identificadores repetidos", "QuestionAnswers debe disponer cada id una vez"])

    def stop_with_dup(res):
        stop(res, None)
        res["TopicVerdicts"] = [{"Topic": T1, "Status": "CONFORMS", "Rationale": "x", "ClauseRefs": [], "FindingIds": []}] * 2
    p("113-not-accredited-con-ids-repetidos", stop_with_dup, ["TopicVerdicts con identificadores repetidos"], base=stop_calls)

    # ---- direct read of V14 L2231-L2233 (the rule of v5.3/v5.4, now on B.9) and the references of nested elements
    v14d = ref(V14, 2231, 2233)
    p("114-sin-lectura-directa-de-v14", lambda res: None,
      ["lectura directa: %s no entregadas fielmente" % v14d, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % v14d],
      base=lambda r: base_calls(r, direct=False))
    p("115-cita-de-v14-leida-solo-en-a7", lambda res: mat(res)["ClauseRefs"].append(v14d),
      ["lectura directa: %s no entregadas fielmente" % v14d, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % v14d,
       "referencia sobre líneas no entregadas fielmente en Materiality (%s)" % v14d], base=lambda r: base_calls(r, direct=False))
    p("116-directreads-sin-v14", lambda res: res.update({"DirectReads": [{"Ref": sec_ref("### 3.4 "), "Purpose": "x"}]}),
      ["lectura directa: DirectReads sin una entrada que cubra"])
    p("117-directreads-cubre-solo-2231", lambda res: res.update({"DirectReads": [{"Ref": ref(V14, 2231), "Purpose": "x"}]}),
      ["lectura directa: DirectReads sin una entrada que cubra"])

    def cited_after_direct(res):
        mat(res)["ClauseRefs"].append(v14d)
        tvd(res)[T1]["ClauseRefs"].append(v14d)
        res["DirectReads"].append({"Ref": sec_ref("### 3.4 "), "Purpose": "x"})
    p("118-cita-de-v14-tras-la-lectura-directa-aceptada", cited_after_direct, coherent=True)
    p("119-referencia-anidada-no-leida", lambda res: trd(res)["M-02"]["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en Materiality.Triggers[M-02] (%s)" % ref(A6, 10, 12)])
    p("120-referencia-anidada-de-evidencia-leida", lambda res: fcd(res)["FC-01"]["ClauseRefs"].append(preflight_ref()), coherent=True)
    p("121-referencia-del-impacto-d1-no-leida", lambda res: d1(res)["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en RelatedImpacts.ContractExclusionD1 (%s)" % ref(A6, 10, 12)])

    # ---- run.json of the launcher (the round-3 rules, decisions §84, point 4)
    out_ = "Outcome de run.json"
    add(variant("126-run-json-fallo-del-proceso", run_hook=hook(run_patch={"ExitCode": 3}),
                expected=["código de salida 3", out_ + " 'REVIEW_PROCESS_FAILURE'"]))
    add(variant("127-run-json-fallo-del-transporte", run_hook=hook(run_patch={"TransportFailure": {"Class": "TRANSPORT_AUTH_REFRESH_CONFLICT", "Tokens": 0}}),
                expected=[out_ + " 'TRANSPORT_FAILURE'"]))
    add(variant("128-run-json-sin-mensaje-result-registrado", run_hook=hook(run_patch={"ResultMessage": False}),
                expected=[out_ + " 'TRANSPORT_FAILURE'"]))
    add(variant("129-run-json-fallo-del-registro-posterior",
                run_hook=hook(run_patch={"PostRunErrors": [{"Step": "ProcessGoneAfter", "Class": "UnicodeDecodeError", "Message": "byte 0xe0"}]}),
                expected=[out_ + " 'POST_RUN_RECORDING_FAILURE'", "PostRunErrors de run.json no vacío"]))
    add(variant("130-run-json-fallo-de-lanzamiento", run_hook=hook(run_patch={"LaunchError": {"Class": "FileNotFoundError", "Message": "x",
                                                                                               "ChildRunning": False}}),
                expected=[out_ + " 'LAUNCH_FAILURE'", "run.json registra un fallo de lanzamiento"]))
    add(variant("131-run-json-abortado", run_hook=hook(run_patch={"Aborted": "el binario cambió"}),
                expected=[out_ + " 'LAUNCH_FAILURE'", "run.json registra un fallo de lanzamiento"]))
    add(variant("132-run-json-outcome-incoherente", run_hook=hook(run_patch={"ExitCode": 2, "Outcome": "COMPLETED", "OutcomeCategories": ["COMPLETED"]}),
                expected=["código de salida 2", "Outcome y OutcomeCategories de run.json incoherentes"]))
    add(variant("133-run-json-categorias-incoherentes", run_hook=hook(run_patch={"Outcome": "COMPLETED",
                                                                                 "OutcomeCategories": ["COMPLETED", "TRANSPORT_FAILURE"]}),
                expected=["Outcome y OutcomeCategories de run.json incoherentes"]))
    add(variant("134-run-json-sin-outcome", run_hook=hook(run_patch={"Outcome": DELETE, "OutcomeCategories": DELETE}),
                expected=[out_ + " None", "Outcome y OutcomeCategories de run.json incoherentes", "run.json sin los campos del registro ['Outcome', "
                                                                                                    "'OutcomeCategories']"]))
    add(variant("135-run-json-sin-campos-del-registro", run_hook=hook(run_patch={"TasklistQuery": DELETE, "CloneAfter": DELETE}),
                expected=["run.json sin los campos del registro ['TasklistQuery', 'CloneAfter']"]))
    add(variant("136-run-json-postrunerrors-ausente", run_hook=hook(run_patch={"PostRunErrors": DELETE}),
                expected=["PostRunErrors de run.json no vacío o ausente", "run.json sin los campos del registro ['PostRunErrors']"]))
    add(variant("137-run-json-fallo-del-proceso-y-del-registro",
                run_hook=hook(run_patch={"Killed": True, "TimedOut": True, "ExitCode": 1,
                                         "PostRunErrors": [{"Step": "Stdout", "Class": "OSError", "Message": "x"}]}),
                expected=["código de salida 1", "proceso terminado por el invocador", out_ + " 'REVIEW_PROCESS_FAILURE'",
                          "PostRunErrors de run.json no vacío"]))

    # ---- preflight.json of the launcher (the round-3 rules, decisions §84, points 2 and 3)
    fcg, rpb, env = "preflight: guarda de la orden congelada", "preflight: sonda de registro", "preflight: entorno del lanzador"

    def pf(fn):
        return hook(pf_hook=fn)
    add(variant("138-preflight-sin-guarda-de-la-orden", run_hook=pf(lambda f: f.pop("FrozenCommandGuard")), expected=[fcg]))
    add(variant("139-preflight-guarda-no-aplicada", run_hook=pf(lambda f: f["FrozenCommandGuard"].update({"Enforced": False})), expected=[fcg]))
    add(variant("140-preflight-guarda-con-otra-orden", run_hook=pf(lambda f: f["FrozenCommandGuard"].update({"FrozenCommand": "python -X utf8 -B kit/launch.py"})),
                expected=[fcg]))
    add(variant("141-preflight-guarda-fallida", run_hook=pf(lambda f: f["FrozenCommandGuard"].update({"Ok": False, "Problems": ["opciones -X"]})),
                expected=[fcg]))
    add(variant("142-preflight-guarda-con-problemas", run_hook=pf(lambda f: f["FrozenCommandGuard"].update({"Problems": ["x"]})), expected=[fcg]))
    add(variant("143-preflight-sin-sonda-de-registro", run_hook=pf(lambda f: f.pop("RecordProbe")), expected=[rpb]))
    add(variant("144-preflight-sonda-fallida", run_hook=pf(lambda f: f["RecordProbe"].update({"Ok": False, "Problems": ["falta run.json"]})),
                expected=[rpb]))
    add(variant("145-preflight-sin-entorno", run_hook=pf(lambda f: f.pop("Environment")), expected=[env]))
    add(variant("146-preflight-entorno-con-valor-no-python", run_hook=pf(lambda f: f["Environment"]["PythonValues"].update({"GIT_EDITOR": "x"})),
                expected=[env]))
    add(variant("147-preflight-entorno-sin-valor-python", run_hook=pf(lambda f: f["Environment"].update({"PythonValues": {}})), expected=[env]))
    add(variant("148-preflight-entorno-otros-prefijos", run_hook=pf(lambda f: f["Environment"].update({"Prefixes": ["PYTHON"]})), expected=[env]))
    add(variant("149-preflight-entorno-nombre-ajeno", run_hook=pf(lambda f: f["Environment"]["Names"].append("PATH")), expected=[env]))

    # ---- preflight record of gate 11 (A-7: object and agreed A-6 blobs at the HEAD; the authorization §93 after §92, no later section)
    g11 = "preflight: compuerta 11 (%s)" % OG.CHECK
    g11a = g11 + " sin el blob de A-7"
    g11p = g11 + " con la autorización pendiente"
    add(variant("150-preflight-sin-compuerta-11", run_hook=pf(lambda f: f["Checks"].pop(OG.CHECK)),
                expected=["preflight: faltan comprobaciones ['%s']" % OG.CHECK, g11 + " sin las líneas fijadas", g11p, g11a]))
    add(variant("150b-preflight-compuerta-11-otra-linea", run_hook=pf(lambda f: f["Checks"][OG.CHECK].update({"A6LineSha256": "4" * 64})),
                expected=[g11 + " sin las líneas fijadas"]))
    add(variant("150c-preflight-compuerta-11-fallida", run_hook=pf(lambda f: f["Checks"][OG.CHECK].update({"Ok": False, "A6LineNumbers": [],
                                                                                                           "Problems": ["falta la línea"]})),
                expected=["preflight: comprobaciones sin Ok: ['%s']" % OG.CHECK, g11 + " sin las líneas fijadas"]))
    for name, fn in (
            ("151-preflight-compuerta-11-sin-autorizacion", lambda f: f["Checks"][OG.CHECK].update({"AuthorizationSections": [], "Authorized": False})),
            ("151b-preflight-compuerta-11-blob-en-la-head", lambda f: f["Checks"][OG.CHECK].update({"ObjectBlobAtHead": OTHER_A7_BLOB,
                                                                                                   "ObjectBlobAtHeadMatches": False})),
            ("151b2-preflight-compuerta-11-a6-en-la-head", lambda f: f["Checks"][OG.CHECK].update({"A6BlobAtHead": OTHER_A6_BLOB,
                                                                                                  "A6AgreedBlobAtHeadMatches": False})),
            ("151c-preflight-compuerta-11-otro-runid", lambda f: f["Checks"][OG.CHECK].update({"RunId": "R20261014T180556Z-5e02"})),
            ("151d-preflight-compuerta-11-seccion-posterior", lambda f: f["Checks"][OG.CHECK].update(
                {"LaterSections": [{"Number": OG.AUTH_SECTION + 1, "Line": 1850, "Heading": "## %d. Disposición del Coordinator: (selftest)"
                                    % (OG.AUTH_SECTION + 1)}]})),
            ("151e-preflight-compuerta-11-sha-del-texto", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"][0].update({"TextSha256": "0" * 64})),
            ("151f-preflight-compuerta-11-encabezado", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"][0].update({"HeadingMatches": False})),
            ("151g-preflight-compuerta-11-dos-secciones", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"].append(
                dict(f["Checks"][OG.CHECK]["AuthorizationSections"][0], Line=1860))),
            ("151h-preflight-compuerta-11-no-autorizada", lambda f: f["Checks"][OG.CHECK].update({"Authorized": False})),
            ("151i-preflight-compuerta-11-sha-fijado-distinto", lambda f: f["Checks"][OG.CHECK].update({"AuthorizationTextSha256": "1" * 64})),
            ("151j-preflight-compuerta-11-otra-seccion", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"][0].update({"Number": 92})),
            ("151k-preflight-compuerta-11-sin-blob", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"][0].update({"HasBlob": False})),
            ("151l-preflight-compuerta-11-otro-objeto", lambda f: f["Checks"][OG.CHECK].update({"ObjectBlob": OTHER_A7_BLOB})),
            ("151m-preflight-compuerta-11-otra-seccion-fijada", lambda f: f["Checks"][OG.CHECK].update({"AuthorizationSection": 92})),
            ("151n-preflight-compuerta-11-otro-encabezado-fijado", lambda f: f["Checks"][OG.CHECK].update(
                {"AuthorizationHeading": "## 93. Otra disposición (selftest)"}))):
        add(variant(name, run_hook=pf(fn), expected=[g11a]))
    add(variant("151o-preflight-compuerta-11-registro-pendiente", run_hook=pf(lambda f: f["Checks"][OG.CHECK].update({"AuthorizationPending": True})),
                expected=[g11p]))
    saved_pins = (OG.AUTH_SECTION, OG.AUTH_HEADING, OG.AUTH_TEXT_SHA)
    try:
        OG.AUTH_SECTION = OG.AUTH_HEADING = OG.AUTH_TEXT_SHA = None   # a kit with the authorization pending: never accreditable
        add(variant("151p-kit-con-la-autorizacion-pendiente", expected=[g11p]))
    finally:
        OG.AUTH_SECTION, OG.AUTH_HEADING, OG.AUTH_TEXT_SHA = saved_pins

    # ---- categories (execution vs content) reported separately
    rv = variant("152-categorias-de-ejecucion-y-contenido", run_hook=hook(run_patch={"ExitCode": 3}), result_patch=lambda res, r: changes_required(res),
                 expected=["código de salida 3", out_ + " 'REVIEW_PROCESS_FAILURE'"])
    rv[0]["CategoriesReported"] = rv[1]["Categories"]
    if rv[1]["Categories"].get("Execution") != "REVIEW_PROCESS_FAILURE" or rv[1]["Categories"].get("ArchitectVerdict") != "CHANGES REQUIRED":
        rv[0]["Result"] = "FAIL"
    add(rv)
    rv = variant("153-categoria-completed-acreditada")
    if rv[1]["Categories"].get("Execution") != "COMPLETED" or rv[1]["Categories"].get("ArchitectVerdict") != "AGREED":
        rv[0]["Result"] = "FAIL"
    add(rv)
    return cases


# ------------------------------------------------------------------ gate 11 cases (owner_gate.evaluate on decision texts built from the clone's; A-7)
def owner_cases():
    cases = []
    dec_lines = clone_decision_lines()
    dec_clone = "\n".join(dec_lines) + "\n"
    a6_text = "\n".join(PR.canon(A6))
    blob, run_id = PR.OBJECT_BLOB, PR.IDS["RunId"]
    sha_ok = PR.TRANSPORT["Binary"]["Sha256"]
    a6l = next(l for l in dec_lines if OG.line_sha(l) == OG.A6_LINE_SHA)
    claude_l = next(l for l in dec_lines if OG.line_sha(l) == OG.CLAUDE_LINE_SHA)
    s78 = next(i for i, l in enumerate(dec_lines) if l.startswith("## 78. "))
    s93 = next(i for i, l in enumerate(dec_lines) if l.startswith("## %d. " % AUTH_SECTION))
    assert not any(l.startswith("## ") for l in dec_lines[s93 + 1:]), "§93 no es la última sección de las decisiones del clon"
    base_lines, syn = dec_lines[:s93], dec_lines[s93:]
    dec_base = "\n".join(base_lines) + "\n"              # decisiones antes de §93: el texto del clon cuando un caso aísla la regla de la autorización
    syn_text = "\n".join(syn) + "\n"
    assert dec_base + syn_text == dec_clone
    pending = {"Section": None, "Heading": None, "TextSha256": None}

    def pin(text, number=AUTH_SECTION):
        v = PA.values(text, number, blob)["Values"]
        return {"Section": number, "Heading": v["AUTH_HEADING"], "TextSha256": v["AUTH_TEXT_SHA"]}
    auth93 = pin(dec_clone)

    def text(lines):
        return "\n".join(lines) + "\n"

    def before93(extra):
        return text(base_lines + extra)

    def replaced(old, new):
        out = "\n".join(new if l == old else l for l in base_lines) + "\n"
        assert out != dec_base or old == new
        return out

    def run_case(name, dec_head, expected=(), clean=True, descends=True, measured=None, clone_text=None, obj_head=None, pinned=None, ok=None,
                 dec_cl=None, auth=None, a6_head=None):
        ev_ = OG.evaluate(dec_head, dec_base if dec_cl is None else dec_cl, clean, descends, sha_ok if measured is None else measured, sha_ok,
                          a6_text if clone_text is None else clone_text, blob if obj_head is None else obj_head, blob if pinned is None else pinned,
                          run_id, OG.A6_AGREED_BLOB if a6_head is None else a6_head, auth93 if auth is None else auth)
        got = ev_["Problems"]
        missing = [e for e in expected if not any(e in g for g in got)]
        unexpected = [g for g in got if not any(e in g for e in expected)]
        want_ok = not expected if ok is None else ok
        cases.append({"Case": name, "Expected": list(expected), "Got": got, "Missing": missing, "Unexpected": unexpected, "Ok": ev_["Ok"],
                      "AuthorizationPending": ev_["AuthorizationPending"], "Authorized": ev_["Authorized"], "LaterSections": ev_["LaterSections"],
                      "SectionsAfterBase": ev_["SectionsAfterBase"],
                      "Result": "PASS" if not missing and not unexpected and ev_["Ok"] is want_ok else "FAIL"})
        return ev_

    PEND = OG.PENDING_REASON
    NOAUTH, LATER, UNIQUE, NOTAFTER = "falta la autorización", "sección posterior a §%d" % AUTH_SECTION, \
        "decisiones sin un único encabezado §%d" % AUTH_SECTION, "no es una sección posterior a §92"
    e = run_case("O00-autorizacion-pendiente-bloquea", dec_clone, [PEND], auth=pending)
    if not (e["AuthorizationPending"] is True and e["AuthorizationSections"] == [] and e["LaterSections"] == [] and e["Problems"] == [PEND] and
            [x["Number"] for x in e["SectionsAfterBase"]] == [AUTH_SECTION]):
        cases[-1]["Result"] = "FAIL"
    e = run_case("O00b-pendiente-con-otra-seccion-posterior-a-92", dec_clone + "\n## 94. Nota (selftest)\n", [PEND], auth=pending)
    if [x["Number"] for x in e["SectionsAfterBase"]] != [AUTH_SECTION, 94] or e["Problems"] != [PEND]:
        cases[-1]["Result"] = "FAIL"
    fresh = load("owner_gate_as_frozen", os.path.join(KITDATA, "owner_gate.py"))
    pins = fresh.pinned_authorization()
    structural = (all(v is None for v in pins.values()) or
                  (type(pins["Section"]) is int and pins["Section"] > fresh.BASE_SECTION and str(pins["Heading"]).startswith("## %d. " % pins["Section"])
                   and re.match(r"^[0-9a-f]{64}$", str(pins["TextSha256"])) is not None))
    cases.append({"Case": "O00c-modulo-del-kit-pendiente-o-fijado-entero", "Pins": pins, "Pending": fresh.authorization_pending(pins),
                  "PinsEqualRecomputedFromClone": pins == auth93,
                  "Result": "PASS" if structural and fresh.BASE_SECTION == 92 and fresh.PENDING_REASON == PEND and
                  (fresh.authorization_pending(pins) or pins == auth93) else "FAIL"})
    run_case("O01-autorizacion-93-fijada-pasa", dec_clone)
    run_case("O01b-autorizacion-93-con-el-texto-real-del-clon", dec_clone, dec_cl=dec_clone)
    run_case("O02-seccion-posterior-a-la-fijada", dec_clone + "\n## %d. Disposición del Coordinator: otra (selftest)\n\ntexto\n" % (AUTH_SECTION + 1),
             [LATER])
    run_case("O03-seccion-99-cualquiera-posterior", dec_clone + "\n## 99. Nota (selftest)\n", [LATER])
    t = dec_clone.rstrip("\n") + " (selftest)\n"
    run_case("O04-fijada-con-otro-texto", t, [NOAUTH])
    t = dec_base + syn_text.replace("Disposición del Coordinator", "Disposición del Owner")
    run_case("O05-fijada-con-otro-encabezado", t, [NOAUTH])
    t = dec_base + syn_text.replace(blob, blob[:8] + "…")
    run_case("O06-fijada-con-el-blob-abreviado", t, [NOAUTH], auth=pin(t))
    t = dec_base + syn_text.replace(blob, blob + "0")
    run_case("O07-fijada-con-el-blob-pegado", t, [NOAUTH], auth=pin(t))
    run_case("O08-sin-la-seccion-fijada", dec_base, [UNIQUE + " (0)", NOAUTH])
    run_case("O09-dos-secciones-fijadas", dec_clone + "\n## %d. Duplicado (selftest)\n" % AUTH_SECTION, [UNIQUE + " (2)", NOAUTH])
    run_case("O10-fijada-no-posterior-a-92", dec_base, [NOTAFTER, NOAUTH], auth=pin(dec_base, 92))
    run_case("O10b-fijada-con-un-numero-que-no-es-entero", dec_clone, [NOTAFTER, "decisiones sin un único encabezado §93 (0)", NOAUTH],
             auth=dict(auth93, Section="93"))
    run_case("O11-a7-en-la-head-con-otro-blob", dec_clone, ["no tiene el blob de A-7 fijado en el kit"], obj_head=OTHER_A7_BLOB)
    run_case("O11b-a6-en-la-head-con-otro-blob", dec_clone, ["no tiene el blob acordado"], a6_head=OTHER_A6_BLOB)
    run_case("O11c-a6-ausente-en-la-head", dec_clone, ["no tiene el blob acordado"], a6_head="")
    run_case("O12-blob-no-fijado", dec_clone, ["blob de A-7 no fijado en el kit", "no tiene el blob de A-7 fijado", NOAUTH], pinned="")
    run_case("O12b-blob-fijado-de-otro-borrador", dec_clone, ["blob de A-7 no fijado en el kit", "no tiene el blob de A-7 fijado", NOAUTH],
             pinned=OTHER_A7_BLOB, obj_head=OTHER_A7_BLOB)
    para = a6l.replace("excepción limitada", "excepción acotada")
    assert para != a6l
    for name, new, exp in (
            ("O13-sin-la-linea-del-owner", "(línea retirada)", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02"]),
            ("O14-parafrasis", para, ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02", "otra línea que empieza por «A6-ADR0046-9-FX02 =»"]),
            ("O15-fragmento-citado-en-tabla", "| **Owner** | `" + a6l + "` |", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02"]),
            ("O16-cita-con-sangria-de-bloque", "> " + a6l, ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02"]),
            ("O17-espacio-final", a6l + " ", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02", "otra línea que empieza por «A6-ADR0046-9-FX02 =»"]),
            ("O18-retorno-de-carro", a6l + "\r", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02", "otra línea que empieza por «A6-ADR0046-9-FX02 =»"])):
        t = replaced(a6l, new)
        run_case(name, t + syn_text, exp, dec_cl=t, auth=pin(t + syn_text))
    moved = base_lines[:s78] + [a6l] + [l for l in base_lines[s78:] if l != a6l]
    t = text(moved)
    run_case("O19-linea-antes-de-78", t + syn_text, ["no está registrada después del encabezado de decisiones §78"], dec_cl=t, auth=pin(t + syn_text))
    t = before93(["A6-ADR0046-9-FX02 = B (revoca la determinación anterior)", ""])
    run_case("O20-revocacion-anterior-a-93", t + syn_text, ["otra línea que empieza por «A6-ADR0046-9-FX02 =»"], dec_cl=t, auth=pin(t + syn_text))
    t = replaced(claude_l, "CLAUDE-CLI-I62 = (retirada)")
    run_case("O21-sin-claude-cli-i62", t + syn_text, ["falta, como línea completa de decisiones, la línea del Owner CLAUDE-CLI-I62 = A",
                                                      "otra línea que empieza por «CLAUDE-CLI-I62 =»"], dec_cl=t, auth=pin(t + syn_text))
    t = before93(["CLAUDE-CLI-I62 = B (revocada)", ""])
    run_case("O22-otra-linea-claude-cli-i62", t + syn_text, ["otra línea que empieza por «CLAUDE-CLI-I62 =»"], dec_cl=t, auth=pin(t + syn_text))
    t = dec_clone.replace("suma 4 057 bytes", "suma 4 058 bytes")
    assert t != dec_clone
    run_case("O23-decision-93-cambiada-en-la-head", t, ["no crece solo por añadido", NOAUTH], dec_cl=dec_clone)
    t = dec_clone.replace("suma 3 299 bytes", "suma 3 298 bytes")
    assert t != dec_clone
    run_case("O23b-decision-92-cambiada-en-la-head", t, ["no crece solo por añadido"], dec_cl=dec_clone)
    run_case("O24-cambios-sin-commit", dec_clone, ["cambios sin commit"], clean=False)
    run_case("O25-head-no-desciende-del-clon", dec_clone, ["no desciende del commit del clon"], descends=False)
    run_case("O26-otro-binario", dec_clone, ["SHA-256 medido del binario distinto del cierre"], measured="0" * 64)
    run_case("O27-linea-fijada-ausente-de-la-a6-acordada", dec_clone, ["no está, como línea completa, en la A-6 acordada"],
             clone_text=a6_text.replace(a6l, para))
    t = before93(["## 78. Duplicado (selftest)", ""])
    run_case("O28-dos-encabezados-78", t + syn_text, ["decisiones sin un único encabezado §78 (2)"], dec_cl=t, auth=pin(t + syn_text))
    run_case("O29-sha-del-texto-fijado-distinto", dec_clone, [NOAUTH], auth=dict(auth93, TextSha256="0" * 64))
    v_ok = PA.values(dec_clone, AUTH_SECTION, blob)
    v_later = PA.values(dec_clone + "\n## 94. Otra (selftest)\n", AUTH_SECTION, blob)
    v_92 = PA.values(dec_base, 92, blob)
    v_none = PA.values(dec_base, AUTH_SECTION, blob)
    lines = PA.python_lines(v_ok["Values"])
    ok30 = (v_ok["Ready"] is True and v_ok["Values"]["AUTH_HEADING"] == auth93["Heading"] and v_ok["Values"]["AUTH_TEXT_SHA"] == auth93["TextSha256"] and
            v_later["Ready"] is False and [x["Number"] for x in v_later["Checks"]["LaterSections"]] == [94] and
            v_92["Ready"] is False and v_92["Checks"]["AfterBaseSection"] is False and v_92["Checks"]["HasBlobToken"] is False and
            v_none["Ready"] is False and v_none["Values"]["AUTH_HEADING"] is None and
            len(lines) == 3 and lines[0].startswith("AUTH_SECTION = 93") and ast.literal_eval(lines[1].split(" = ", 1)[1]) == auth93["Heading"] and
            ast.literal_eval(lines[2].split(" = ", 1)[1].split("   #")[0]) == auth93["TextSha256"])
    cases.append({"Case": "O30-pin-authorization-valores", "Values": v_ok["Values"], "PythonLines": lines,
                  "LaterReady": v_later["Ready"], "Section92Ready": v_92["Ready"], "AbsentReady": v_none["Ready"], "Result": "PASS" if ok30 else "FAIL"})
    return cases


def live_owner_gate_case():
    """L00: owner_gate.check on the REAL I-62 worktree (read-only git), with owner_gate.py loaded exactly as the kit holds it (not the module of
    the audit cases). Pinned (the kit as delivered: §93) it must pass with exactly the pinned section (heading, full blob, text SHA-256), the object
    and agreed A-6 blobs at the HEAD and no later section; with the pins pending it must block with ONLY the pending reason."""
    def wt(*a):
        return subprocess.run(["git", "-C", WORKTREE] + list(a), capture_output=True, text=True, env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))
    if not os.path.isdir(WORKTREE):
        return {"Case": "L00-compuerta-11-real", "Result": "FAIL", "Got": "worktree no disponible"}
    head = wt("rev-parse", "HEAD").stdout.strip()
    og = load("owner_gate_live", os.path.join(KITDATA, "owner_gate.py"))
    closure_like = {"ObjectBlob": PR.OBJECT_BLOB, "Ids": {"RunId": PR.IDS["RunId"]}, "Transport": {"Binary": {"Sha256": PR.TRANSPORT["Binary"]["Sha256"]}}}
    ev_ = og.check(WORKTREE, CLONE, PR.REV, closure_like, PR.TRANSPORT["Binary"]["Sha256"])
    auth = ev_.get("AuthorizationSections") or []
    pending = og.authorization_pending(og.pinned_authorization())
    if pending:
        ok = ev_["Ok"] is False and ev_["Problems"] == [og.PENDING_REASON] and ev_["AuthorizationPending"] is True and auth == [] and \
            ev_["LaterSections"] == [] and ev_["ObjectBlobAtHeadMatches"] is True and ev_["A6AgreedBlobAtHeadMatches"] is True
    else:
        ok = ev_["Ok"] is True and ev_["Authorized"] is True and ev_["LaterSections"] == [] and len(auth) == 1 and \
            auth[0]["Number"] == og.AUTH_SECTION and auth[0]["HeadingMatches"] and auth[0]["HasBlob"] and auth[0]["TextSha256Matches"] and \
            ev_["ObjectBlobAtHeadMatches"] is True and ev_["A6AgreedBlobAtHeadMatches"] is True
    return {"Case": "L00-compuerta-11-real", "ExpectedState": "PENDING" if pending else "PINNED", "WorktreeHead": head, "CloneCommitUsed": PR.REV,
            "Ok": ev_["Ok"], "Problems": ev_["Problems"], "ObjectBlobAtHead": ev_["ObjectBlobAtHead"], "A6BlobAtHead": ev_["A6BlobAtHead"],
            "AuthorizationPending": ev_["AuthorizationPending"], "AuthorizationSections": auth, "LaterSections": ev_["LaterSections"],
            "SectionsAfterBase": ev_["SectionsAfterBase"], "A6LineNumbers": ev_["A6LineNumbers"], "ClaudeCliLineNumbers": ev_["ClaudeCliLineNumbers"],
            "Note": "binario no medido en la sonda: se pasa el SHA-256 del cierre; nada se escribe en el worktree",
            "Result": "PASS" if ok else "FAIL"}


# ------------------------------------------------------------------ gate cases (transport_gate.evaluate, c3_analysis.analyze; and the decoding of §84.5)
def standin(name, payload):
    """A stand-in executable (.bat) that writes `payload` (bytes) to its stdout with this interpreter and ignores its arguments; it is not claude.exe
    and runs no model. It lives under <kit>/../.tmp/st and is removed with it."""
    d = os.path.join(TMPROOT, "standin")
    os.makedirs(d, exist_ok=True)
    p = os.path.join(d, name + ".bat")
    code = "import sys; sys.stdout.buffer.write(bytes.fromhex('%s'))" % payload.hex()
    with open(p, "w", encoding="ascii", newline="\r\n") as f:
        f.write('@echo off\n"%s" -B -I -c "%s"\n' % (sys.executable, code))
    return p


def gate_cases():
    cases = []
    closure = PR.CLOSURE

    def run_case(name, status="MEASURED", char_fn=None, acc_fn=None, gate_fn=None, char_bytes_fn=None, acc_bytes_fn=None, expected=()):
        char = synthetic_char(with_c3=(status != "ACCEPTED"))
        if char_fn:
            char_fn(char)
        cb = jbytes(char)
        ab = None
        if status == "ACCEPTED" or acc_fn:
            acc = synthetic_acceptance(sha(cb))
            if acc_fn:
                acc_fn(acc)
            ab = jbytes(acc)
        gate = synthetic_gate(status, cb, ab)
        if gate_fn:
            gate_fn(gate)
        cb = char_bytes_fn(cb) if char_bytes_fn else cb
        ab = acc_bytes_fn(ab) if acc_bytes_fn else ab
        ev_ = TG.evaluate(gate, closure, cb, ab)
        got = ev_["Problems"]
        missing = [e for e in expected if not any(e in g for g in got)]
        unexpected = [g for g in got if not any(e in g for e in expected)]
        cases.append({"Case": name, "Expected": list(expected), "Got": got, "Missing": missing, "Unexpected": unexpected,
                      "Result": "PASS" if not missing and not unexpected else "FAIL"})

    def setp(path, value):
        def f(o):
            cur = o
            for k in path[:-1]:
                cur = cur[k]
            if value is DELETE:
                cur.pop(path[-1], None)
            else:
                cur[path[-1]] = value
        return f

    no_safe = [x for x in TEMPLATE if x != "--safe-mode"]
    run_case("G00-medida-c3-valida")
    run_case("G01-pendiente", gate_fn=setp(["Status"], "PENDING"), expected=["D-01 abierto"])
    run_case("G02-plantilla-de-la-compuerta", gate_fn=setp(["ArgsTemplate"], TG.without_add_dir(TEMPLATE)), expected=["ArgsTemplate de la compuerta"])
    run_case("G03-runid-de-la-compuerta", gate_fn=setp(["RunId"], "R20261009T000000Z-0000"), expected=["RunId, SessionId o Binary de la compuerta"])
    run_case("G04-sin-registro", char_bytes_fn=lambda b: None, expected=["falta el registro de caracterización"])
    run_case("G05-registro-alterado", char_bytes_fn=lambda b: b + b" ", expected=["SHA-256 del registro de caracterización distinto"])
    run_case("G06-registro-ilegible", gate_fn=setp(["Characterization", "Sha256"], sha(b"{")), char_bytes_fn=lambda b: b"{",
             expected=["registro de caracterización ilegible"])
    run_case("G07-sin-binario-elegible", char_fn=setp(["Executables"], []), expected=["no registra el binario elegible"])
    run_case("G08-c1-otra-lista", char_fn=setp(["Probes", "C1", "Args"], TG.without_add_dir(no_safe)), expected=["C1 no midió la lista"])
    run_case("G09-c1-salida", char_fn=setp(["Probes", "C1", "ExitCode"], 1), expected=["C1 sin código de salida 0"])
    run_case("G10-stale", char_fn=setp(["Eligibility", "STALE"], True), expected=["Eligibility.STALE no es false"])
    run_case("G11-architect-no-elegible", char_fn=setp(["Eligibility", "ARCHITECT"], "NOT_ELIGIBLE"), expected=["Eligibility.ARCHITECT no es ELIGIBLE"])
    run_case("G12-sin-c3", char_fn=setp(["Probes", "C3"], DELETE), expected=["no tiene la sonda C3"])
    run_case("G13-c3-veredicto", char_fn=setp(["Probes", "C3", "Verdict"], "FAIL"), expected=["C3 sin veredicto PASS limpio"])
    run_case("G14-c3-plantilla-sin-add-dir", char_fn=lambda c: c["Probes"]["C3"].update({"ArgsTemplate": TG.without_add_dir(TEMPLATE)}),
             expected=["C3 no midió la lista exacta"])
    run_case("G15-c3-argumentos", char_fn=setp(["Probes", "C3", "Args"], no_safe), expected=["los argumentos registrados de C3"])
    run_case("G16-c3-binario", char_fn=setp(["Probes", "C3", "Binary", "Sha256"], "0" * 64), expected=["C3 con otro binario o versión"])
    run_case("G17-c3-salida", char_fn=setp(["Probes", "C3", "ExitCode"], 1), expected=["C3 sin salida 0"])
    run_case("G18-c3-result", char_fn=setp(["Probes", "C3", "Result", "subtype"], "error_during_execution"), expected=["C3 sin result success"])
    run_case("G19-c3-init", char_fn=setp(["Probes", "C3", "Init", "tools"], ["Bash", "Glob", "Grep", "Read", "StructuredOutput"]),
             expected=["init de C3 distinto"])
    run_case("G20-c3-effort", char_fn=setp(["Probes", "C3", "ObservedEffortPerMessage"], ["high"]), expected=["C3 con modelo o effort"])
    run_case("G21-c3-lectura-anadida", char_fn=setp(["Probes", "C3", "AddDirRead", "Denied"], True), expected=["dentro del directorio añadido"])
    run_case("G22-c3-lectura-exterior", char_fn=setp(["Probes", "C3", "OutsideRead", "Denied"], False), expected=["lectura fuera del cwd"])
    run_case("G23-c3-denegaciones", char_fn=lambda c: c["Probes"]["C3"]["Result"]["permission_denials"].append({"Tool": "Read", "Path": C3_ADD}),
             expected=["permission_denials con rutas distintas"])
    run_case("G24-sin-plantilla-medida", char_fn=setp(["Eligibility", "MeasuredArgsTemplate"], DELETE), expected=["Eligibility.MeasuredArgsTemplate"])
    run_case("G25-aceptada-valida", status="ACCEPTED")
    run_case("G26-aceptacion-ausente", status="ACCEPTED", acc_bytes_fn=lambda b: None, expected=["falta el registro de aceptación"])
    run_case("G27-aceptacion-alterada", status="ACCEPTED", acc_bytes_fn=lambda b: b + b" ", expected=["SHA-256 del registro de aceptación distinto"])
    run_case("G28-aceptacion-ilegible", status="ACCEPTED", gate_fn=setp(["Acceptance", "Sha256"], sha(b"[")), acc_bytes_fn=lambda b: b"[",
             expected=["registro de aceptación ilegible"])
    for n, (key, value, exp) in enumerate([("Schema", "x/v0", "aceptación con otro esquema"), ("RunId", "R20261009T000000Z-0000", "aceptación de otro RunId"),
                                           ("Decision", "ACCEPT", "aceptación sin Decision"), ("UnmeasuredFlags", [], "aceptación con UnmeasuredFlags"),
                                           ("ArgsTemplate", no_safe, "aceptación de otra plantilla"), ("BinarySha256", "0" * 64, "aceptación de otro binario"),
                                           ("CharacterizationSha256", "0" * 64, "aceptación sobre otro registro"),
                                           ("AcceptedBy", "Owner", "AcceptedBy distinto"), ("Authority", " ", "aceptación sin Authority")]):
        run_case("G%d-aceptacion-%s" % (29 + n, key), status="ACCEPTED", acc_fn=setp([key], value), expected=[exp])
    for name, kind, exp_probe, exp_gate in (
            ("G38-sonda-lectura-anadida-denegada", "ADD_DENIED", ["dentro del directorio añadido"],
             ["C3 sin veredicto PASS limpio", "lectura dentro del directorio añadido"]),
            ("G39-sonda-lectura-exterior-entregada", "OUT_DELIVERED", ["lectura exterior no quedó denegada"],
             ["C3 sin veredicto PASS limpio", "lectura fuera del cwd"])):
        rec = c3_record(kind)
        probe_missing = [e for e in exp_probe if not any(e in p for p in rec["Problems"])]
        char = synthetic_char()
        char["Probes"]["C3"] = rec
        got = TG.check_c3(char, TG.binary_of(closure), TEMPLATE, MODEL, EFFORT)
        missing = [e for e in exp_gate if not any(e in g for g in got)]
        unexpected = [g for g in got if not any(e in g for e in exp_gate)]
        cases.append({"Case": name, "ProbeVerdict": rec["Verdict"], "ProbeProblems": rec["Problems"], "Expected": exp_gate, "Got": got,
                      "Missing": probe_missing + missing, "Unexpected": unexpected,
                      "Result": "PASS" if rec["Verdict"] == "FAIL" and not probe_missing and not missing and not unexpected else "FAIL"})
    rec = c3_record("PASS")
    cases.append({"Case": "G40-sonda-pass", "ProbeVerdict": rec["Verdict"], "ProbeProblems": rec["Problems"],
                  "Result": "PASS" if rec["Verdict"] == "PASS" and not rec["Problems"] else "FAIL"})

    # ---- decisions §84, point 5: explicit Windows decoding of the binary's output (pure function and stand-in executables)
    def decode_case(name, b, expected=(), text=None, bom=False, hexpart=None, context=8):
        d = TG.decode_cli_output(b, context) if context != 8 else TG.decode_cli_output(b)
        got = d["Problems"]
        missing = [e for e in expected if not any(e in g for g in got)]
        unexpected = [g for g in got if not any(e in g for e in expected)]
        extra_ok = d["Ok"] is (not expected) and d["Text"] == text and d["Bom"] is bom and (hexpart is None or d["HexExcerpt"] == hexpart)
        cases.append({"Case": name, "Expected": list(expected), "Got": got, "Missing": missing, "Unexpected": unexpected,
                      "Record": {k: d[k] for k in ("Ok", "Bytes", "Bom", "HexExcerpt")}, "TextMatches": d["Text"] == text,
                      "Result": "PASS" if not missing and not unexpected and extra_ok else "FAIL"})
    v = b"2.1.299 (Claude Code)\n"
    cp850 = "INFORMACIÓN: no hay tareas".encode("cp850")
    decode_case("G41-decodificacion-ascii", v, text=v.decode("ascii"))
    decode_case("G42-decodificacion-bom-utf8", TG.UTF8_BOM + v, text=v.decode("ascii"), bom=True)
    decode_case("G43-decodificacion-utf8-no-ascii", "Organización Á Í Ï Ð Ý".encode("utf-8"), text="Organización Á Í Ï Ð Ý")
    decode_case("G44-decodificacion-cp850-informacion", cp850, ["no es UTF-8 estricto"], hexpart=cp850[9 - 8:9 + 1 + 8].hex(" "))   # 0xE0 en la posición 9
    decode_case("G45-decodificacion-utf8-invalido", b"ab\xc3(cd", ["no es UTF-8 estricto"], hexpart="61 62 c3 28 63 64")
    decode_case("G46-decodificacion-utf16-con-bom", "2.1.299 (Claude Code)".encode("utf-16"), ["BOM UTF-16"])
    decode_case("G47-decodificacion-vacia", b"", ["salida del binario vacía"], hexpart="")
    decode_case("G47b-decodificacion-solo-espacios", b" \r\n", ["salida del binario vacía"])
    decode_case("G48-decodificacion-no-bytes", None, ["no capturada en bytes"])
    decode_case("G48b-decodificacion-texto-ya-decodificado", "2.1.299", ["no capturada en bytes"])
    decode_case("G49-decodificacion-sin-contexto", "org: INFORMACIÓN".encode("cp850"), ["no es UTF-8 estricto"], hexpart="e0", context=0)

    def standin_case(name, payload, fn, check):
        p = standin(name.split("-")[0], payload)
        r = fn(p, TMPROOT)
        problems = check(r)
        cases.append({"Case": name, "Record": json.loads(USER_RX.sub("%USERPROFILE%", json.dumps(r, ensure_ascii=False))), "Problems": problems,
                      "Result": "PASS" if not problems else "FAIL"})

    def version_check(version, decoding_problem=None, bom=False):
        def f(r):
            probs = []
            if r.get("Version") != version:
                probs.append("Version %r != %r" % (r.get("Version"), version))
            d = r.get("Decoding") or {}
            if decoding_problem is None and (d.get("Ok") is not True or d.get("Problems")):
                probs.append("Decoding no Ok: %s" % d.get("Problems"))
            if decoding_problem is not None and not any(decoding_problem in x for x in d.get("Problems") or []):
                probs.append("Decoding sin el problema %r: %s" % (decoding_problem, d.get("Problems")))
            if d.get("Bom") is not bom or "Text" in d or r.get("ReturnCode") != 0:
                probs.append("Bom, Text o ReturnCode inesperados: %s" % {k: d.get(k) for k in ("Bom",)})
            return probs
        return f

    def auth_check(logged, decoding_problem=None, flag=None, max_hex=None):
        def f(r):
            probs = []
            if r.get("loggedIn") is not logged:
                probs.append("loggedIn %r != %r" % (r.get("loggedIn"), logged))
            s = json.dumps(r, ensure_ascii=False)
            if "example.invalid" in s or "Organizaci" in s or "orgName" in s or "email" in s:
                probs.append("el registro conserva el correo o la organización")
            if set(r) - {"loggedIn", "authMethod", "apiProvider", "ReturnCode", "Decoding", "DecodeError", "ParseError"}:
                probs.append("claves inesperadas: %s" % sorted(set(r)))
            d = r.get("Decoding") or {}
            if decoding_problem is not None and not any(decoding_problem in x for x in d.get("Problems") or []):
                probs.append("Decoding sin el problema %r" % decoding_problem)
            if flag and r.get(flag) is not True:
                probs.append("falta %s" % flag)
            if max_hex is not None and len((d.get("HexExcerpt") or "").split()) > max_hex:
                probs.append("extracto hexadecimal con contexto: %r" % d.get("HexExcerpt"))
            return probs
        return f
    auth_ok = json.dumps({"loggedIn": True, "authMethod": "claude.ai", "apiProvider": "firstParty", "email": "persona@example.invalid",
                          "orgName": "Organización Á"}, ensure_ascii=False)
    standin_case("G50-sustituto-version-ascii", b"2.1.299 (Claude Code)\r\n", TG.cli_version, version_check("2.1.299 (Claude Code)"))
    standin_case("G51-sustituto-version-cp850", "INFORMACIÓN: 2.1.299".encode("cp850"), TG.cli_version, version_check(None, "no es UTF-8 estricto"))
    standin_case("G52-sustituto-version-utf16", "2.1.299 (Claude Code)".encode("utf-16"), TG.cli_version, version_check(None, "BOM UTF-16"))
    standin_case("G53-sustituto-version-vacia", b"", TG.cli_version, version_check(None, "salida del binario vacía"))
    standin_case("G54-sustituto-version-bom-utf8", TG.UTF8_BOM + b"2.1.299 (Claude Code)\n", TG.cli_version, version_check("2.1.299 (Claude Code)", bom=True))
    standin_case("G55-sustituto-auth-utf8-no-ascii", auth_ok.encode("utf-8"), TG.auth_status, auth_check(True))
    standin_case("G56-sustituto-auth-cp850", auth_ok.encode("cp850", "replace"), TG.auth_status,
                 auth_check(None, "no es UTF-8 estricto", "DecodeError", max_hex=3))
    standin_case("G57-sustituto-auth-json-invalido", b"loggedIn: yes\n", TG.auth_status, auth_check(None, flag="ParseError"))
    standin_case("G58-sustituto-auth-utf16", auth_ok.encode("utf-16"), TG.auth_status, auth_check(None, "BOM UTF-16", "DecodeError", max_hex=2))
    return cases


# ------------------------------------------------------------------ consistency of launch.py and the auditor (X)
def consistency_cases():
    cases = []

    def row(name, ok, **detail):
        cases.append(dict({"Case": name, "Result": "PASS" if ok else "FAIL"}, **detail))
    row("X01-campos-de-run-json-iguales", tuple(LAUNCH.get("RUN_JSON_FIELDS") or ()) == tuple(PR.RUN_JSON_FIELDS),
        Launch=list(LAUNCH.get("RUN_JSON_FIELDS") or ()), Auditor=list(PR.RUN_JSON_FIELDS))
    base = {"Pid": 1, "ExitCode": 0, "Killed": False, "TimedOut": False, "PostRunErrors": [], "ResultMessage": True, "TransportFailure": None}
    records = [base, dict(base, ExitCode=1), dict(base, Killed=True), dict(base, TimedOut=True), dict(base, ResultMessage=False),
               dict(base, TransportFailure={"Class": "TRANSPORT_AUTH_REFRESH_CONFLICT"}), dict(base, PostRunErrors=[{"Step": "x"}]),
               dict(base, LaunchError={"Class": "OSError"}), dict(base, Aborted="x"), {"Aborted": "x"}, {"LaunchError": {"Class": "OSError"}},
               dict(base, ExitCode=None), dict(base, Killed=None), dict(base, ExitCode=1, PostRunErrors=[{"Step": "x"}], ResultMessage=False), {}]
    got = [(LAUNCH["outcome_categories"](copy.deepcopy(r)), PR.outcome_categories(copy.deepcopy(r))) for r in records]
    row("X02-categorias-iguales", all(a == b for a, b in got) and got[0][0] == ["COMPLETED"], Pairs=[{"Launch": a, "Auditor": b} for a, b in got])
    row("X03-orden-congelada", LAUNCH.get("FROZEN_COMMAND") == PR.LAUNCH_COMMAND == "python -B kit/launch.py",
        Launch=LAUNCH.get("FROZEN_COMMAND"), Closure=PR.LAUNCH_COMMAND)
    row("X04-prefijos-de-entorno", tuple(LAUNCH.get("LAUNCH_ENV_PREFIXES") or ()) == tuple(PR.LAUNCH_ENV_PREFIXES) == ("PYTHON", "GIT_", "CLAUDE", "ANTHROPIC"),
        Launch=list(LAUNCH.get("LAUNCH_ENV_PREFIXES") or ()), Auditor=list(PR.LAUNCH_ENV_PREFIXES))
    return cases


def run_cases(pr, live=False):
    """All cases against an auditor module (the mutation harness passes mutants). → list of rows."""
    init_globals(pr)
    return audit_cases() + gate_cases() + owner_cases() + consistency_cases() + ([live_owner_gate_case()] if live else [])


def main(out_path):
    pr = load_auditor()
    cases = run_cases(pr, live=True)
    status = subprocess.run(["git", "-C", CLONE, "status", "--porcelain", "--ignored"], capture_output=True, text=True).stdout
    head = subprocess.run(["git", "-C", CLONE, "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
    run_match = {n: hashlib.sha256(open(os.path.join(RUN, n), "rb").read()).hexdigest() == hashlib.sha256(open(os.path.join(KITDATA, n), "rb").read()).hexdigest()
                 for n in pr.RUN_FILES}
    run_listing = sorted(os.listdir(RUN))
    audit_rows = [c for c in cases if not c["Case"].startswith(("G", "O", "L", "X"))]
    gate_rows = [c for c in cases if c["Case"].startswith("G")]
    owner_rows = [c for c in cases if c["Case"].startswith(("O", "L"))]
    x_rows = [c for c in cases if c["Case"].startswith("X")]
    doc = {"Selftest": "post-review.py v5.5-a7 + transport_gate.py + owner_gate.py (A-7) + pin_authorization.values + c3_analysis.analyze + "
                       "coherencia con launch.py",
           "Environment": "SYNTHETIC" if pr.CLOSURE.get("Synthetic") else "REAL (clon y run del kit)",
           "Clone": CLONE, "Run": RUN, "Rev": pr.REV, "Object": {"Path": pr.OBJECT, "Blob": pr.OBJECT_BLOB}, "Provisional": pr.CLOSURE.get("Provisional"),
           "SessionId": pr.SESSION_ID, "RunId": pr.IDS["RunId"], "KitGateStatus": pr.GATE.get("Status"),
           "AuthorizationPinsForAuditCases": PINS,
           "Modules": {n: hashlib.sha256(open(os.path.join(HERE, n), "rb").read()).hexdigest()
                       for n in ("post-review.py", "transport_gate.py", "owner_gate.py", "pin_authorization.py", "c3_analysis.py", "result.schema.json",
                                 "selftest-post-review.py", "launch.py")},
           "CloneHeadAfterSelftest": head, "CloneCleanAfterSelftest": status.strip() == "" and head == pr.REV,
           "RunFilesMatchKit": run_match, "RunDirListingAfterSelftest": run_listing,
           "Cases": cases,
           "Totals": {"Cases": len(cases), "Pass": sum(1 for c in cases if c["Result"] == "PASS"),
                      "AuditCases": len(audit_rows), "AuditPass": sum(1 for c in audit_rows if c["Result"] == "PASS"),
                      "GateCases": len(gate_rows), "GatePass": sum(1 for c in gate_rows if c["Result"] == "PASS"),
                      "OwnerGateCases": len(owner_rows), "OwnerGatePass": sum(1 for c in owner_rows if c["Result"] == "PASS"),
                      "ConsistencyCases": len(x_rows), "ConsistencyPass": sum(1 for c in x_rows if c["Result"] == "PASS")},
           "AllPass": all(c["Result"] == "PASS" for c in cases) and status.strip() == "" and head == pr.REV and all(run_match.values()) and
           run_listing == sorted(pr.RUN_FILES)}
    text = json.dumps(doc, ensure_ascii=False, indent=1)
    text = USER_RX.sub("%USERPROFILE%", text)
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text + "\n")
    for c in cases:
        line = "%-60s %s %s" % (c["Case"][:60], c["Result"], ("missing=%s unexpected=%s bad=%s sv=%s coh=%s probs=%s" % (
            c.get("Missing"), c.get("Unexpected"), c.get("BadCounts"), c.get("SchemaValid"), c.get("ResultCoherence"), c.get("Problems")))
            if c["Result"] != "PASS" else "")
        print(USER_RX.sub("%USERPROFILE%", line))
    print("clean", doc["CloneCleanAfterSelftest"], "runfiles", run_match, "listing", run_listing, "all", doc["AllPass"], doc["Totals"])
    safe_remove_tmproot()
    return 0 if doc["AllPass"] else 1


def safe_remove_tmproot():
    """Remove <kit>/../.tmp/st only if no link or junction is left inside it (never follow a link)."""
    for root, dirs, files in os.walk(TMPROOT, followlinks=False):
        for d in dirs:
            p = os.path.join(root, d)
            if os.path.islink(p) or getattr(os.path, "isjunction", lambda x: False)(p) or (getattr(os.lstat(p), "st_file_attributes", 0) & 0x400):
                raise SystemExit("enlace o unión sin borrar en %s: no se borra el directorio temporal" % p)
    shutil.rmtree(TMPROOT)
    parent = os.path.dirname(TMPROOT)
    if os.path.isdir(parent) and not os.listdir(parent):
        os.rmdir(parent)


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1]))
