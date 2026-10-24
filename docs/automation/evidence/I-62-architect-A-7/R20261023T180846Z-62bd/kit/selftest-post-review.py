"""Self-test of post-review.py v7.0-a7r3, of transport_gate.py, of owner_gate.py (A-7 round 3), of pin_authorization.values and of
c3_analysis.analyze, run BEFORE the launch of the ONE formal review of ROUND 3 of amendment A-7, bounded to A62-A7R2-R1 and its consequences
(decisions §96, points 2, 3 and 6). Adapted from the self-test of the A-7 round-2 kit (R20261019T201030Z-d1bf, itself from round 1 with the
read-fidelity cases of phase 1): the same synthetic formats and the same transport, read, custody, launcher, gate and fidelity cases (00-55,
126-153, G00-G58, X01-X05, F00-F20), the specifics replaced by the round-3 closure (three run files besides the faithful copies: order.txt = the §96
body, a7-r2.md = the round-2 text, and prompt.md; the object is A-7 round 3; the direct read is AUTOMATION_PLAN 16.7 L593-L595) and the result
schema rackcad-a7r3-architect-review/0, with:
  * cases 56-121j for the coherence and coverage rules of the round-3 schema (PriorFindingResolution of A62-A7R2-R1, DeltaPurity with its
    Changes WITHIN_SCOPE | OUTSIDE_SCOPE, RegressionCheck of what round 2 found corrected or conforming, CompatibilityVerdicts A5 and A6 — the
    explicit verdict of decisions §96, point 6: 69-73, 81, 88, 89, 100b, 102, 111, 114h, 114i, 121i —, Materiality with its Triggers and
    A7Declared, OwnerReservedConsequence, QuestionAnswers, AGREED, CHANGES REQUIRED, REJECTED, BLOCKED, NOT_ACCREDITED, schema; 77-78: the round-2
    fields OptionalDispositionConformance and RelatedImpacts are rejected by the schema; 114b-114i are nested schema violations that only the
    auditor's in-process schema check detects, because the Test-Json of pwsh 7.6.7 validates only the first level of nesting), the direct read of
    AUTOMATION_PLAN L593-L595 and the references of the nested elements;
  * the reference mechanism of decisions §96, point 6 («Exigir referencias relativas válidas, presentes en el cierre y comprobadas contra el
    archivo real»): 15, 93-93h and 109 (a file outside the closure, the misspelled path of round 2 docs/initiatives/I-62-A7.md, `..`, backslashes,
    an absolute path into the clone, another spelling that the schema admits, a premise with another spelling or by its absolute path): the schema
    rejects what it can, and the auditor reports every one as non-canonical with the reference as written and why (outside the closure, lines out of
    range, or not the exact relative path); 93f shows the auditor's exactness rule on its own (schema valid); F09 a faithful copy cited; X06 the
    reference pattern of result.schema.json against every closure path, the premises and the negatives;
  * cases 150-151p for the preflight record of gate 11: the object blob and the agreed A-6 blob at the worktree HEAD and the authorization pinned in
    owner_gate.py; the kit is delivered with the authorization PENDING, so the audit cases use SYNTHETIC pins (a synthetic §97 appended to the
    clone's decisions, set on the auditor's module object, never on the file); case 151p checks that a kit with the authorization pending cannot be
    accredited;
  * cases O00-O30 (owner_gate.evaluate, pure, on decision texts built from the clone's decisions, which end at §96, plus a SYNTHETIC §97): the
    authorization pending (blocked with the single «authorization pending: …» reason), pinned on a section after §96 (passes), a later section
    (blocked), the object and agreed A-6 blobs and the inherited checks; O30 exercises pin_authorization.values; and the live probe L00
    (owner_gate.check on the real I-62 worktree, read-only git, with the module exactly as the kit holds it: pending, it must block with only the
    pending reason; pinned, it must pass with exactly the pinned section and no later section);
  * cases X01-X06: launch.py and the auditor agree (RUN_JSON_FIELDS, outcome_categories on a set of records, the frozen command, the environment
    prefixes and, X05, check 7 with the faithful copies: run_files_check); launch.py is read with ast, never imported (importing it would evaluate
    its frozen-command guard); X06, the reference pattern of the schema;
  * cases F00-F20 (read fidelity and faithful copies, decisions §94, point 5): flagged originals read on faithful lines (informational); the r1
    failure reproduced (the TRX original with its BOM silently dropped: DEGRADED, a reason); the TRX through its copy, cited by the original path
    (credited); the copy altered by one byte in the run, altered in delivery, absent, of another original, non-canonical, with the BOM dropped
    inside it, and the original's blob pin altered (each a reason, nothing credited); a long line through the copy with and without its appendix
    and truncated in the original (the TruncatedKnownLongLines rule); the direct read of AUTOMATION_PLAN through its copy; a copy cited as a
    reference; and, only with the fixture of a test clone (zz-fidelity-fixture/), bare CR, NUL, invalid UTF-8 and every fixture copy (skipped, with a
    PASS row that says so, when the closure has no fixture).
Environment. By default it audits against the kit's closure (kit/closure.json: clone = closure Transport.Cwd, run = closure RunDir): the real
clone D:\\r62-arch-a7r2 and run D:\\r62-arch-a7r2-run. With RACKCAD_A7_KITDATA=<dir> it takes closure.json, the gate modules and the kit copies of
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
EVID = "docs/automation/evidence/I-62-A7/r3"                 # el preflight de la ronda 3
GRR, SUMMARY, PREFLIGHT_MD = EVID + "/a7-guards-result-run.json", EVID + "/summary.json", EVID + "/preflight.md"
Q6 = EVID + "/q6-compat.json"                                # la compatibilidad del preflight de la ronda 3 (A-5, A-6, …)
R2OUT = "docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/output.json"
PKG = "docs/automation/evidence/I-62-F6/FX-02/bmin"
SPEC = PKG + "/SPEC.md"
ACC = _CLOSURE["AcceptorPath"]
TOOLF = PKG + "/tool/bmin_facts.py"
OTHER_A7_BLOB = "8a528133c7cccd83356e31f8fd79cd2ebb0cec15"   # la ronda 2 de A-7: nunca el objeto de esta revisión
OTHER_A6_BLOB = "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1"   # la A-6 de la ronda 3: nunca la acordada
C3_ADD = r"D:\r62-c3-probe\add\canary-add.txt"
C3_OUT = r"D:\r62-c3-probe\out\canary-out.txt"
DELETE = object()
CLONE_MUTATED = False     # True while the junction of case 03 exists (the mutation harness bypasses its caches then)
BLOCKED = "BLOCKED " + chr(0x2014) + " OWNER DECISION"
IFAGREED_KEYS = ("ScopeLimitedToR2R1AndConsequences", "PriorFindingCorrected", "DeltaLimitedToR2R1", "NoRegressionOfPriorConformities",
                 "CompatibleWithA5AndA6", "MaterialityConfirmed", "NoUnidentifiedOwnerReservedConsequence", "NoUnauthorizedChanges",
                 "SuitableForCoordinatorAgreement")
V14_READ = (2223, 2241)                       # a V14 range used by the fidelity cases (B.9)
AP_DIRECT = (593, 595)                        # AUTOMATION_PLAN 16.7, the reverification «después de escribir, sin conflictos» (L19 of A-7 §2)
AP_READ = (585, 603)                          # the direct read of Step 0 (AUTOMATION_PLAN 16.7 L593-L595 inside; the prompt's offset and limit)
BASE_SECTION = 96                             # decisions §96: the last section of the clone; the authorization has to come after it
SYNTH_AUTH_SECTION = 97                       # synthetic disposition after §96 (the kit is delivered with the authorization PENDING)
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
                node.targets[0].id in ("FROZEN_COMMAND", "LAUNCH_ENV_PREFIXES", "RUN_JSON_FIELDS", "RUN_FILES"):
            out[node.targets[0].id] = ast.literal_eval(node.value)
        if isinstance(node, ast.FunctionDef) and node.name == "outcome_categories":
            ns = {}
            exec(compile(ast.Module(body=[node], type_ignores=[]), "launch.py:outcome_categories", "exec"), ns)
            out["outcome_categories"] = ns["outcome_categories"]
        if isinstance(node, ast.FunctionDef) and node.name == "run_files_check":
            out["run_files_check_node"] = node      # compiled in X05 with the auditor's fidelity module (decisions §94, point 5)
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
    """Lines of a SYNTHETIC Coordinator disposition after §96 (not a real disposition), appended to the clone's decisions by the self-test."""
    return ["## %d. Disposición del Coordinator: autorización de la revisión formal de la ronda 3 de A-7 (selftest)" % number, "",
            "Texto sintético de la autoprueba, no una disposición real: autoriza el lanzamiento único del kit %s sobre el blob %s." % (run_id, blob)]


def apply_pins(og):
    """While the kit's owner_gate.py held the authorization pending, the audit cases would need a pinned authorization: SYNTHETIC pins (§97 appended
    to the clone's decisions; values from pin_authorization.values) set on the auditor's owner_gate module object (never on the file). With the
    kit's pins set (§93, as delivered), they are used as they are."""
    global PINS
    if og.authorization_pending(og.pinned_authorization()):
        text = "\n".join(clone_decision_lines() + [""] + synthetic_authorization(PR.OBJECT_BLOB, PR.IDS["RunId"])) + "\n"
        v = PA.values(text, SYNTH_AUTH_SECTION, PR.OBJECT_BLOB)
        assert v["Ready"], v
        og.AUTH_SECTION, og.AUTH_HEADING, og.AUTH_TEXT_SHA = SYNTH_AUTH_SECTION, v["Values"]["AUTH_HEADING"], v["Values"]["AUTH_TEXT_SHA"]
        PINS = {"Source": "SYNTHETIC (owner_gate.py del kit con la autorización pendiente; §97 sintética añadida a las decisiones del clon)",
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


# ------------------------------------------------------------------ the A-7 round-3 reads and the base result
def read_range(r, rel, a, b):
    r.call("Read", {"file_path": absp(rel), "offset": a, "limit": b - a + 1}, numbered(lines_of(rel)[a - 1:b], a))


def read_run_range(r, name, a, b):
    r.call("Read", {"file_path": RUN + "\\" + name, "offset": a, "limit": b - a + 1}, numbered(lines_of(run=name)[a - 1:b], a))


def ap_direct(r):
    read_range(r, AP, *AP_READ)


def step0(r, direct=True):
    if direct:
        ap_direct(r)
    r.call("Read", {"file_path": RUN + "\\order.txt"}, numbered(lines_of(run="order.txt")))
    read_range(r, DEC, *closure_range(DEC, "## 96. "))
    r.call("Read", {"file_path": absp(GRR)}, numbered(lines_of(GRR)))
    r.call("Read", {"file_path": absp(SUMMARY)}, numbered(lines_of(SUMMARY)))
    read_range(r, R2OUT, 1, 230)


def a7_reads(r):
    r.call("Read", {"file_path": absp(A7)}, numbered(lines_of(A7)))
    r.call("Read", {"file_path": RUN + "\\a7-r2.md"}, numbered(lines_of(run="a7-r2.md")))


def evidence_reads(r):
    read_range(r, PREFLIGHT_MD, 1, 60)
    r.call("Read", {"file_path": absp(Q6)}, numbered(lines_of(Q6)))


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


PRIOR_ROWS = {"A62-A7R2-R1": "§3.7, fila A62-A7R2-R1"}
REG_ROWS = {"R1_TRX_AND_RELAYFACTS_AS_CORRECTED": "§3.6, fila R1", "R2_INVALIDATOR_RULE_AND_PROCEDURE": "§3.6, fila R2",
            "R3_A62_RULE1_PRECISION": "§3.6, fila R3",
            "T04_EFF_EFF1_B1_MAP_VALIDATION": "§3.1, Más allá de R7-A", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE": "A7-1, punto 5",
            "T06_CLAUSEMAP_COVERAGE_RULE": "§3.5, residuo declarado (clausemap)", "T07_MULTI_CATEGORY_ENTRIES": "A7-1, punto 1",
            "T09_NO_UNAUTHORIZED_CHANGES": "A7-1, punto 8"}


def base_result():
    ids = PR.IDS
    return {
        "Schema": "rackcad-a7r3-architect-review/0", "RunId": ids["RunId"], "InvocationId": ids["InvocationId"],
        "LogicalReviewRequestId": ids["LogicalReviewRequestId"], "AttemptSeq": ids["AttemptSeq"], "RequestedRole": "ARCHITECT",
        "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62", "ReviewedCommit": PR.REV, "ReviewedPath": PR.OBJECT, "ReviewedBlob": PR.OBJECT_BLOB,
        "ReviewerMode": "SEPARATE SESSION", "SamePersonAsAuthor": "selftest",
        "ReviewerDeclaredIdentity": {"Runtime": "x", "Model": "x", "Effort": "x", "SessionOrThread": "x"},
        "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["x"]},
        "Step0": dict([("Basis", "INVOKER_VERIFIED_CLONE")] + [(k, True) for k in schema_keys("Step0") if k not in ("Basis", "Note")] + [("Note", "x")]),
        "DirectReads": [{"Ref": ref(AP, *AP_DIRECT), "Purpose": "x"}],
        "Reads": ["x"], "Verdict": "AGREED", "RequiredFindings": [], "OptionalFindings": [],
        "PriorFindingResolution": [{"PriorFinding": f, "Status": "CORRECTED", "Rationale": "x", "ClauseRefs": [a7_ref(PRIOR_ROWS[f])], "FindingIds": []}
                                   for f in PR.ROUND1],
        "DeltaPurity": {"Status": "CONFIRMED",
                        "Changes": [{"Change": k, "Assessment": "WITHIN_SCOPE", "Rationale": "x",
                                     "ClauseRefs": [a7_ref("§3.7, tabla de la corrección de la ronda 3")], "FindingIds": []} for k in PR.CHANGES],
                        "Rationale": "x", "ClauseRefs": [sec_ref("### 3.7 ")], "FindingIds": []},
        "RegressionCheck": [{"Topic": t, "Status": "NO_REGRESSION", "Rationale": "x", "ClauseRefs": [a7_ref(REG_ROWS[t])], "FindingIds": []}
                            for t in PR.REGRESSION],
        "CompatibilityVerdicts": {"A5": {"Status": "COMPATIBLE", "Rationale": "x", "ClauseRefs": [a7_ref("§6, A-5")], "FindingIds": []},
                                  "A6": {"Status": "COMPATIBLE", "Rationale": "x", "ClauseRefs": [a7_ref("§6, A-6")], "FindingIds": []}},
        "Materiality": {"Triggers": [{"Trigger": m, "A7Declared": PR.MAT_A7[m], "Assessment": "AGREE", "Rationale": "x",
                                      "ClauseRefs": [a7_ref("§5, %s" % m)], "FindingIds": []} for m in PR.TRIGGERS],
                        "Rationale": "x", "ClauseRefs": [sec_ref("## 5. ")], "FindingIds": []},
        "OwnerReservedConsequence": {"OwnerDecisionRequired": False, "UnidentifiedConsequences": [], "Rationale": "x",
                                     "ClauseRefs": [a7_ref("§7, consecuencias reservadas al Owner")], "FindingIds": []},
        "QuestionAnswers": [{"Question": q, "Outcome": "SATISFIED", "Answer": "x", "ClauseRefs": [a7_ref("§8, %s" % q)], "FindingIds": []}
                            for q in PR.QUESTIONS],
        "IfAgreed": {k: True for k in IFAGREED_KEYS},
        "KnownLimitations": [], "RecommendedNextAction": "x"}


def rrd(res):
    return {d["PriorFinding"]: d for d in res["PriorFindingResolution"]}


def rgd(res):
    return {d["Topic"]: d for d in res["RegressionCheck"]}


def chd(res):
    return {d["Change"]: d for d in res["DeltaPurity"]["Changes"]}


def trd(res):
    return {d["Trigger"]: d for d in res["Materiality"]["Triggers"]}


def qad(res):
    return {d["Question"]: d for d in res["QuestionAnswers"]}


def cvd(res):
    return res["CompatibilityVerdicts"]


def mat(res):
    return res["Materiality"]


def orc(res):
    return res["OwnerReservedConsequence"]


def dp(res):
    return res["DeltaPurity"]


def req_finding(fid="A62-A7R3-R1", evidence=None):
    return {"FindingId": fid, "Section": "§3.2 A7-1, punto 1 (c)", "ViolatedInvariant": "x", "Counterexample": "",
            "Evidence": evidence or [a7_ref("A7-1, punto 1")], "WhyItMatters": "x", "CorrectionRequired": "x"}


def opt_finding(fid="A62-A7R3-O1"):
    return {"FindingId": fid, "Section": "§8", "Note": "x", "Evidence": [a7_ref("§8, Q-A7R3-01")], "SuggestedCorrection": "x"}


def not_agreed(res, verdict):
    res["Verdict"] = verdict
    res["IfAgreed"] = {k: None for k in IFAGREED_KEYS}


def changes_required(res, verdict="CHANGES REQUIRED"):
    """A coherent CHANGES REQUIRED (or REJECTED) skeleton: one REQUIRED that leaves A62-A7R2-R1 in PARTIALLY_CORRECTED."""
    not_agreed(res, verdict)
    res["RequiredFindings"] = [req_finding()]
    rrd(res)["A62-A7R2-R1"].update({"Status": "PARTIALLY_CORRECTED", "FindingIds": ["A62-A7R3-R1"]})


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
        n_o = next(i for i, l in enumerate(order) if "No ejecutar Codex" in l)
        r.call("Grep", {"pattern": "No ejecutar Codex", "path": RUN + "\\order.txt", "output_mode": "content", "-n": True},
               "%d:%s" % (n_o + 1, order[n_o]))
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
        rrd(res)["A62-A7R2-R1"]["ClauseRefs"] += ["prompt.md:1", "order.txt:99"]
    add(variant("15-referencia-a-prompt-o-fuera-de-rango", expected=[
        "resultado: no valida contra result.schema.json",
        "referencia no canónica en PriorFindingResolution[A62-A7R2-R1] (prompt.md:1: no es un archivo del cierre ni una premisa del run)",
        "referencia no canónica en PriorFindingResolution[A62-A7R2-R1] (order.txt:99: líneas fuera de rango del archivo)"],
                result_patch=ref_bad, schema_valid=False))

    def stop(res, r):
        nds = {"Rationale": "no evaluado", "ClauseRefs": [], "FindingIds": []}
        res.update({"Verdict": "NOT_ACCREDITED", "Reads": [RUN + "\\order.txt"], "RequiredFindings": [], "OptionalFindings": [],
                    "PriorFindingResolution": [], "RegressionCheck": [], "QuestionAnswers": [],
                    "DeltaPurity": dict(nds, Status="NOT_DETERMINED", Changes=[]),
                    "CompatibilityVerdicts": {"A5": dict(nds, Status="NOT_DETERMINED"), "A6": dict(nds, Status="NOT_DETERMINED")},
                    "Materiality": dict(nds, Triggers=[]),
                    "OwnerReservedConsequence": dict(nds, OwnerDecisionRequired=None, UnidentifiedConsequences=[]),
                    "ReviewedCommit": None, "ReviewedBlob": None,
                    "KnownLimitations": ["Paso 0: el resultado `run` de las guardas no nombra la publicación como Head; revisión detenida"],
                    "IfAgreed": {k: None for k in IFAGREED_KEYS}})
        res["Step0"].update({"GuardsRunModeBaseHeadResult": False, "PreflightSummaryCandidateAndTotals": None})
    add(variant("16-parada-not-accredited-coherente", result_patch=stop, base=stop_calls, require_coherent=True))

    def agreed_with_required(res, r):
        res["RequiredFindings"] = [req_finding()]
        rgd(res)["T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"].update({"Status": "REGRESSION", "FindingIds": ["A62-A7R3-R1"]})
    add(variant("17-agreed-con-un-required", result_patch=agreed_with_required, expected=["AGREED exige cero REQUIRED",
                                                                                          "AGREED exige todos los temas de RegressionCheck en NO_REGRESSION"]))

    def cr_without_required(res, r):
        not_agreed(res, "CHANGES REQUIRED")
    add(variant("18-changes-required-sin-required", result_patch=cr_without_required, expected=["CHANGES REQUIRED sin ningún REQUIRED"]))

    def agreed_trigger_disagree(res, r):
        res["OptionalFindings"] = [opt_finding()]
        trd(res)["M-08"].update({"Assessment": "DISAGREE", "FindingIds": ["A62-A7R3-O1"]})
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
        rrd(res)["A62-A7R2-R1"]["ClauseRefs"] += ["order.txt:3-9", "order.txt:43", "a7-r2.md:121-125"]
        mat(res)["ClauseRefs"].append("order.txt:1")
    add(variant("25-referencias-a-order-y-a7-r2-aceptadas", result_patch=ref_run, require_coherent=True))

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
                          "lectura directa: %s no entregadas fielmente" % ref(AP, *AP_DIRECT)],
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

    # ---- result schema and coherence (A-7 round 3: rackcad-a7r3-architect-review/0)
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
    R, O, PF = "A62-A7R3-R1", "A62-A7R3-O1", "A62-A7R2-R1"
    DPC, CVA5, CVA6 = "DeltaPurity.Changes", "CompatibilityVerdicts.A5", "CompatibilityVerdicts.A6"
    SV = "resultado: no valida contra result.schema.json"
    NC = "no es un archivo del cierre ni una premisa del run"
    NE = "no es la ruta relativa exacta de un archivo del cierre ni el nombre exacto de una premisa del run"

    def link_elsewhere(res):
        """The REQUIRED disposed by a regression topic (so «REQUIRED sin disposición» does not fire)."""
        rgd(res)["T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"].update({"Status": "REGRESSION", "FindingIds": [R]})

    def open_unlinked(res):
        rrd(res)[PF]["FindingIds"] = []
        link_elsewhere(res)
    p("56-resolucion-abierta-sin-required", cr(open_unlinked), ["PriorFindingResolution[A62-A7R2-R1] PARTIALLY_CORRECTED sin el REQUIRED residual vinculado"])
    p("57-resolucion-corregida-con-required", cr(lambda res: rrd(res)[PF].update({"Status": "CORRECTED"})),
      ["PriorFindingResolution[A62-A7R2-R1] CORRECTED con un REQUIRED vinculado"])

    def not_corrected_unlinked(res):
        rrd(res)[PF].update({"Status": "NOT_CORRECTED", "FindingIds": []})
        link_elsewhere(res)
    p("58-resolucion-no-corregida-sin-required", cr(not_corrected_unlinked),
      ["PriorFindingResolution[A62-A7R2-R1] NOT_CORRECTED sin el REQUIRED residual vinculado"])
    p("59-regresion-sin-required", cr(lambda res: rgd(res)["T06_CLAUSEMAP_COVERAGE_RULE"].update({"Status": "REGRESSION"})),
      ["RegressionCheck[T06_CLAUSEMAP_COVERAGE_RULE] REGRESSION sin un REQUIRED vinculado"])
    p("60-sin-regresion-con-required", cr(lambda res: rgd(res)["R1_TRX_AND_RELAYFACTS_AS_CORRECTED"].update({"FindingIds": [R]})),
      ["RegressionCheck[R1_TRX_AND_RELAYFACTS_AS_CORRECTED] NO_REGRESSION con un REQUIRED vinculado"])
    p("61-cambio-fuera-sin-required", cr(lambda res: chd(res)["R2R1_REBASE_COHERENCE"].update({"Assessment": "OUTSIDE_SCOPE"})),
      [DPC + "[R2R1_REBASE_COHERENCE] OUTSIDE_SCOPE sin un REQUIRED vinculado", "DeltaPurity CONFIRMED con un cambio OUTSIDE_SCOPE"])
    p("62-cambio-dentro-con-required", cr(lambda res: chd(res)["DESCRIPTIVE_SECTIONS"].update({"FindingIds": [R]})),
      [DPC + "[DESCRIPTIVE_SECTIONS] WITHIN_SCOPE con un REQUIRED vinculado"])
    p("63-pureza-no-confirmada-sin-cambio-fuera", cr(lambda res: dp(res).update({"Status": "NOT_CONFIRMED", "FindingIds": [R]})),
      ["DeltaPurity NOT_CONFIRMED con todos los cambios WITHIN_SCOPE"])

    def nc_no_req(res):
        chd(res)["R2R1_MAINSHA_EVAL_FROM_REBASEMAP"].update({"Assessment": "OUTSIDE_SCOPE", "FindingIds": [R]})
        dp(res).update({"Status": "NOT_CONFIRMED"})
    p("64-pureza-no-confirmada-sin-required", cr(nc_no_req), ["DeltaPurity NOT_CONFIRMED sin un REQUIRED vinculado"])
    p("65-disparador-en-desacuerdo-sin-required", cr(lambda res: trd(res)["M-03"].update({"Assessment": "DISAGREE"})),
      ["Materiality.Triggers[M-03] DISAGREE sin un REQUIRED vinculado"])
    p("66-disparador-de-acuerdo-con-required", cr(lambda res: trd(res)["M-01"].update({"FindingIds": [R]})),
      ["Materiality.Triggers[M-01] AGREE con un REQUIRED vinculado"])
    p("67-declaracion-de-a7-mal-copiada", lambda res: trd(res)["M-08"].update({"A7Declared": "yes"}),
      ["con un veredicto, A7Declared distinto de lo que declara A-7 §5 (M-08)"])
    p("68-pregunta-no-satisfecha-sin-hallazgo", cr(lambda res: qad(res)["Q-A7R3-02"].update({"Outcome": "NOT_SATISFIED"})),
      ["QuestionAnswers[Q-A7R3-02] NOT_SATISFIED sin un hallazgo vinculado"])
    # the explicit verdict on A-5 and A-6 (decisions §96, point 6)
    p("69-compatibilidad-a5-incompatible-sin-required", cr(lambda res: cvd(res)["A5"].update({"Status": "INCOMPATIBLE"})),
      [CVA5 + " INCOMPATIBLE sin un REQUIRED vinculado"])
    p("70-compatibilidad-a6-compatible-con-required", cr(lambda res: cvd(res)["A6"].update({"FindingIds": [R]})),
      [CVA6 + " COMPATIBLE con un REQUIRED vinculado"])
    p("71-compatibilidad-incompatible-coherente-aceptada", cr(lambda res: cvd(res)["A6"].update({"Status": "INCOMPATIBLE", "FindingIds": [R]})),
      coherent=True)
    p("72-compatibilidad-sin-referencias", lambda res: cvd(res)["A5"].update({"ClauseRefs": []}), ["con un veredicto, " + CVA5 + " sin ClauseRefs"])
    p("73-compatibilidad-referencia-a-a5-leida-aceptada", lambda res: cvd(res)["A5"]["ClauseRefs"].append(ref(A5, 175, 199)), coherent=True,
      extra=lambda r: read_range(r, A5, 169, 221))
    p("74-decision-del-owner-sin-consecuencias", bl(lambda res: orc(res).update({"UnidentifiedConsequences": []})),
      ["OwnerDecisionRequired = true sin UnidentifiedConsequences"])
    p("75-sin-decision-del-owner-con-consecuencias", lambda res: orc(res).update({"UnidentifiedConsequences": ["x"]}),
      ["OwnerDecisionRequired = false con UnidentifiedConsequences"])
    p("76-blocked-sin-decision-del-owner", lambda res: not_agreed(res, BLOCKED), ["BLOCKED — OWNER DECISION sin OwnerDecisionRequired = true"])
    p("77-disposiciones-opcionales-de-la-ronda-2-rechazadas-por-el-esquema",
      lambda res: res.update({"OptionalDispositionConformance": [{"Round1Optional": "A62-A7R2-O1", "Status": "CONFORMS", "Rationale": "x",
                                                                   "ClauseRefs": [], "FindingIds": []}]}), [SV], schema_valid=False)
    p("78-impactos-relacionados-de-la-ronda-2-rechazados-por-el-esquema",
      lambda res: res.update({"RelatedImpacts": {"ChangedByRound2": False, "Note": "", "ClauseRefs": [], "FindingIds": []}}), [SV], schema_valid=False)
    p("79-ifagreed-no-nulo-sin-agreed", cr(lambda res: res["IfAgreed"].update({"SuitableForCoordinatorAgreement": True})),
      ["IfAgreed debe ir en null salvo con AGREED"])
    p("80-agreed-con-ifagreed-falso", lambda res: res["IfAgreed"].update({"MaterialityConfirmed": False}),
      ["AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true"])
    p("81-no-determinado-con-veredicto", cr(lambda res: (dp(res).update({"Status": "NOT_DETERMINED"}), cvd(res)["A6"].update({"Status": "NOT_DETERMINED"}))),
      ["con un veredicto, NOT_DETERMINED o null no se admiten (DeltaPurity.Status, CompatibilityVerdicts.A6.Status)"])
    p("82-decision-del-owner-nula-con-veredicto", cr(lambda res: orc(res).update({"OwnerDecisionRequired": None})),
      ["OwnerReservedConsequence.OwnerDecisionRequired no admite null"])
    p("83-paso0-falso-con-veredicto", lambda res: res["Step0"].update({"Decisions96DeclaresOrderBytesAndSha256": False}),
      ["las siete booleanas de Step0 van en true"])
    p("84-paso0-nulo-con-veredicto", lambda res: res["Step0"].update({"AP167DirectReadL593L595": None}), ["las siete booleanas de Step0 van en true"])
    p("85-objeto-otro-blob", lambda res: res.update({"ReviewedBlob": OTHER_A7_BLOB}), ["objeto revisado distinto del designado"])
    p("86-objeto-otro-commit", lambda res: res.update({"ReviewedCommit": PR.CLOSURE["Publication"]["Parent"]}), ["objeto revisado distinto del designado"])
    p("87-ids-de-invocacion-de-la-ronda-2", lambda res: res.update({"RunId": "R20261019T201030Z-d1bf"}), ["identificadores de la invocación distintos"])

    def no_refs(res):
        rrd(res)[PF]["ClauseRefs"] = []
        rgd(res)["T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"]["ClauseRefs"] = []
        chd(res)["R2R1_ABSENCE_AND_NON_ADMISSION"]["ClauseRefs"] = []
        qad(res)["Q-A7R3-03"]["ClauseRefs"] = []
        trd(res)["M-02"]["ClauseRefs"] = []
        for x in (mat(res), orc(res), dp(res), cvd(res)["A6"]):
            x["ClauseRefs"] = []
    p("88-elementos-sin-referencias", no_refs, ["con un veredicto, PriorFindingResolution[A62-A7R2-R1] sin ClauseRefs",
                                                "con un veredicto, RegressionCheck[T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE] sin ClauseRefs",
                                                "con un veredicto, DeltaPurity.Changes[R2R1_ABSENCE_AND_NON_ADMISSION] sin ClauseRefs",
                                                "con un veredicto, QuestionAnswers[Q-A7R3-03] sin ClauseRefs",
                                                "con un veredicto, Materiality.Triggers[M-02] sin ClauseRefs",
                                                "con un veredicto, Materiality sin ClauseRefs",
                                                "con un veredicto, OwnerReservedConsequence sin ClauseRefs",
                                                "con un veredicto, DeltaPurity sin ClauseRefs",
                                                "con un veredicto, " + CVA6 + " sin ClauseRefs"])

    def fid_missing(res):
        rrd(res)[PF]["FindingIds"] = ["A62-A7R3-O7"]
        rgd(res)["T07_MULTI_CATEGORY_ENTRIES"]["FindingIds"] = ["A62-A7R3-O9"]
        chd(res)["PROCEDURE_PIECES_ROUND3"]["FindingIds"] = ["A62-A7R3-O8"]
        qad(res)["Q-A7R3-01"]["FindingIds"] = ["A62-A7R3-O6"]
        trd(res)["M-04"]["FindingIds"] = ["A62-A7R3-O5"]
        dp(res)["FindingIds"] = ["A62-A7R3-O4"]
        cvd(res)["A5"]["FindingIds"] = ["A62-A7R3-O3"]
    p("89-findingids-inexistentes", fid_missing, ["PriorFindingResolution[A62-A7R2-R1] cita en FindingIds hallazgos que no existen",
                                                  "RegressionCheck[T07_MULTI_CATEGORY_ENTRIES] cita en FindingIds hallazgos que no existen",
                                                  DPC + "[PROCEDURE_PIECES_ROUND3] cita en FindingIds hallazgos que no existen",
                                                  "QuestionAnswers[Q-A7R3-01] cita en FindingIds hallazgos que no existen",
                                                  "Materiality.Triggers[M-04] cita en FindingIds hallazgos que no existen",
                                                  "DeltaPurity cita en FindingIds hallazgos que no existen",
                                                  CVA5 + " cita en FindingIds hallazgos que no existen"])
    p("90-ids-de-hallazgo-repetidos", lambda res: res.update({"OptionalFindings": [opt_finding(), opt_finding()]}),
      ["identificadores de hallazgo repetidos"])
    p("91-required-sin-invariante-ni-contraejemplo", cr(lambda res: res["RequiredFindings"][0].update({"ViolatedInvariant": " ", "Counterexample": ""})),
      ["A62-A7R3-R1 REQUIRED sin ViolatedInvariant ni Counterexample"])
    p("92-required-sin-evidencia", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": []})), [SV], schema_valid=False)
    # ---- the reference mechanism (decisions §96, point 6): relative, exact, present in the closure, checked against the real file
    a5r = closure_range(A7, "## 5. ")[0]
    p("93-evidencia-fuera-del-cierre", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": ["docs/HANDOFF.md:1"]})),
      [SV, "referencia no canónica en RequiredFindings[A62-A7R3-R1] (docs/HANDOFF.md:1: %s)" % NC], schema_valid=False)
    p("93b-referencia-mal-escrita-de-la-ronda-2", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": ["docs/initiatives/I-62-A7.md:120"]})),
      [SV, "referencia no canónica en RequiredFindings[A62-A7R3-R1] (docs/initiatives/I-62-A7.md:120: %s)" % NC], schema_valid=False)
    dd = EVID + "/../r3/summary.json:3"
    p("93c-referencia-con-punto-punto", lambda res: mat(res)["ClauseRefs"].append(dd),
      [SV, "referencia no canónica en Materiality (%s: %s)" % (dd, NE)], schema_valid=False)
    p("93d-referencia-con-barras-invertidas", lambda res: mat(res)["ClauseRefs"].append(A7.replace("/", "\\") + ":%d" % a5r),
      [SV, "referencia no canónica en Materiality (%s:%d: %s)" % (A7, a5r, NE)], schema_valid=False)
    p("93e-referencia-con-punto", lambda res: mat(res)["ClauseRefs"].append(EVID + "/./summary.json:3"),
      [SV, "referencia no canónica en Materiality (%s/./summary.json:3: %s)" % (EVID, NE)], schema_valid=False)
    other = SUMMARY.replace("summary.json", "SUMMARY.json")
    p("93f-otra-grafia-admitida-por-el-esquema-rechazada-por-el-auditor", lambda res: mat(res)["ClauseRefs"].append(other + ":3"),
      ["referencia no canónica en Materiality (%s:3: %s)" % (other, NE)])
    p("93g-premisa-con-otra-grafia", lambda res: mat(res)["ClauseRefs"].append("ORDER.TXT:3"),
      [SV, "referencia no canónica en Materiality (ORDER.TXT:3: %s)" % NE], schema_valid=False)
    p("93h-premisa-por-su-ruta-absoluta", lambda res: mat(res)["ClauseRefs"].append(RUN + "\\order.txt:3"),
      [SV, "referencia no canónica en Materiality (%s/order.txt:3: %s)" % (PR.userless(RUN), NE)], schema_valid=False)
    p("94-evidencia-sobre-lineas-no-leidas", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": [ref(A6, 10, 12)]})),
      ["referencia sobre líneas no entregadas fielmente en RequiredFindings[A62-A7R3-R1] (%s)" % ref(A6, 10, 12)])
    p("95-evidencia-opcional-no-leida", lambda res: res.update({"OptionalFindings": [dict(opt_finding(), Evidence=[ref(V14, 2408)])]}),
      ["referencia sobre líneas no entregadas fielmente en OptionalFindings[A62-A7R3-O1]"])
    p("96-required-sin-disposicion", cr(lambda res: res["RequiredFindings"].append(req_finding("A62-A7R3-R2"))),
      ["REQUIRED sin disposición: todo REQUIRED lo cita al menos un elemento evaluado (A62-A7R3-R2)"])

    def agreed_with(update):
        def f(res):
            res["RequiredFindings"] = [req_finding()]
            update(res)
        return f
    AG0 = "AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true"
    p("97-agreed-resolucion-no-corregida", agreed_with(lambda res: rrd(res)[PF].update({"Status": "NOT_CORRECTED", "FindingIds": [R]})),
      [AG0, "AGREED exige el hallazgo previo A62-A7R2-R1 en CORRECTED"])

    def ag_impure(res):
        chd(res)["S33_ROW_C_AND_S34_C6_NOTE"].update({"Assessment": "OUTSIDE_SCOPE", "FindingIds": [R]})
        dp(res).update({"Status": "NOT_CONFIRMED", "FindingIds": [R]})
    p("98-agreed-pureza-no-confirmada", agreed_with(ag_impure), [AG0, "AGREED exige DeltaPurity CONFIRMED con todos los cambios de la ronda 3 WITHIN_SCOPE"])
    p("99-agreed-regresion", agreed_with(lambda res: rgd(res)["T09_NO_UNAUTHORIZED_CHANGES"].update({"Status": "REGRESSION", "FindingIds": [R]})),
      [AG0, "AGREED exige todos los temas de RegressionCheck en NO_REGRESSION"])
    p("100-agreed-disparador-en-desacuerdo", agreed_with(lambda res: trd(res)["M-05"].update({"Assessment": "DISAGREE", "FindingIds": [R]})),
      [AG0, "AGREED exige los ocho disparadores de Materiality en AGREE"])
    p("100b-agreed-con-a5-incompatible", agreed_with(lambda res: cvd(res)["A5"].update({"Status": "INCOMPATIBLE", "FindingIds": [R]})),
      [AG0, "AGREED exige A-5 y A-6 COMPATIBLE"])

    def agreed_owner(res):
        orc(res).update({"OwnerDecisionRequired": True, "UnidentifiedConsequences": ["x"]})
    p("101-agreed-con-decision-del-owner", agreed_owner, [AG0])

    def cr_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        rrd(res)[PF].update({"Status": "NOT_CORRECTED", "FindingIds": [R, O]})
        rgd(res)["T06_CLAUSEMAP_COVERAGE_RULE"].update({"FindingIds": [O]})
        chd(res)["R2R1_REBASE_COHERENCE"].update({"Assessment": "OUTSIDE_SCOPE", "FindingIds": [R]})
        dp(res).update({"Status": "NOT_CONFIRMED", "FindingIds": [R]})
        trd(res)["M-06"].update({"Assessment": "DISAGREE", "FindingIds": [R]})
        qad(res)["Q-A7R3-03"].update({"Outcome": "NOT_SATISFIED", "FindingIds": [O]})
        cvd(res)["A6"].update({"Status": "INCOMPATIBLE", "FindingIds": [R]})
        cvd(res)["A5"].update({"FindingIds": [O]})
    p("102-changes-required-coherente-aceptado", cr(cr_ok), coherent=True)
    p("103-rejected-coherente-aceptado", cr(lambda res: trd(res)["M-07"].update({"Assessment": "DISAGREE", "FindingIds": [R]}), "REJECTED"), coherent=True)
    p("104-rejected-sin-required", lambda res: not_agreed(res, "REJECTED"), ["REJECTED sin ningún REQUIRED"])
    p("105-blocked-coherente-aceptado", bl(lambda res: None), coherent=True)

    def ag_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        qad(res)["Q-A7R3-01"].update({"Outcome": "NOT_SATISFIED", "FindingIds": [O]})
        rgd(res)["T07_MULTI_CATEGORY_ENTRIES"]["FindingIds"] = [O]
    p("106-agreed-con-un-opcional-aceptado", ag_ok, coherent=True)
    p("107-id-de-hallazgo-mal-formado", lambda res: res.update({"OptionalFindings": [dict(opt_finding(), FindingId="A62-A7R3-1")]}), [SV],
      schema_valid=False)

    def r2_space_fid(res):
        res["RequiredFindings"][0]["FindingId"] = "A62-A7R2-R2"
        rrd(res)[PF]["FindingIds"] = ["A62-A7R2-R2"]
    p("108-id-nuevo-con-el-espacio-de-la-ronda-2", cr(r2_space_fid), [SV], schema_valid=False)
    p("109-referencia-absoluta", lambda res: mat(res).update({"ClauseRefs": [absp(A7) + ":" + str(a5r)]}),
      [SV, "referencia no canónica en Materiality (%s:%d: %s)" % (PR.userless(absp(A7)), a5r, NE)], schema_valid=False)
    p("110-referencia-mal-formada", lambda res: mat(res).update({"ClauseRefs": ["order.txt"]}), ["referencia mal formada en Materiality", SV],
      schema_valid=False)
    p("111-esquema-estados-invalidos", lambda res: (rrd(res)[PF].update({"Status": "MAYBE"}), trd(res)["M-01"].update({"Assessment": "OK"}),
                                                    rgd(res)["T04_EFF_EFF1_B1_MAP_VALIDATION"].update({"Status": "IGNORED"}),
                                                    cvd(res)["A6"].update({"Status": "PARTIAL"})),
      [SV, "AGREED exige el hallazgo previo A62-A7R2-R1 en CORRECTED", "AGREED exige los ocho disparadores de Materiality en AGREE",
       "AGREED exige todos los temas de RegressionCheck en NO_REGRESSION", "AGREED exige A-5 y A-6 COMPATIBLE"], schema_valid=False)
    p("112-esquema-booleano-como-texto", lambda res: res["Step0"].update({"OrderReadWhole": "true"}), [SV, "las siete booleanas de Step0 van en true"],
      schema_valid=False)
    p("113-esquema-veredicto-inexistente", lambda res: not_agreed(res, "ACCEPTED"), [SV], schema_valid=False)
    # nested schema violations that Test-Json of pwsh 7.6.7 does not see (only the in-process check of the auditor does)
    p("114b-esquema-propiedad-sobrante-anidada", lambda res: res["Step0"].update({"Extra": True}), [SV], schema_valid=False)
    p("114c-esquema-campo-requerido-ausente-anidado", lambda res: rrd(res)[PF].pop("Rationale"), [SV], schema_valid=False)
    p("114d-esquema-texto-vacio-anidado", lambda res: mat(res).update({"Rationale": ""}), [SV], schema_valid=False)
    p("114e-esquema-elemento-de-lista-no-texto", lambda res: orc(res).update({"UnidentifiedConsequences": [1]}),
      [SV, "OwnerDecisionRequired = false con UnidentifiedConsequences"], schema_valid=False)
    p("114f-esquema-null-no-admitido-anidado", lambda res: orc(res).update({"Rationale": None}), [SV], schema_valid=False)
    p("114g-esquema-propiedad-sobrante-doble-anidada", lambda res: chd(res)["R2R1_REBASE_COHERENCE"].update({"X": 1}), [SV], schema_valid=False)
    p("114h-esquema-propiedad-sobrante-en-la-compatibilidad", lambda res: cvd(res)["A5"].update({"X": 1}), [SV], schema_valid=False)
    p("114i-esquema-compatibilidad-ausente", lambda res: res["CompatibilityVerdicts"].pop("A5"),
      [SV, "con un veredicto, " + CVA5 + " sin ClauseRefs", "con un veredicto, NOT_DETERMINED o null no se admiten (CompatibilityVerdicts.A5.Status)",
       "AGREED exige A-5 y A-6 COMPATIBLE"], schema_valid=False)

    # ---- coverage of the round-3 arrays
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
    p("115-resolucion-ausente", drop(["PriorFindingResolution"], "PriorFinding", PF),
      ["PriorFindingResolution debe disponer cada id una vez (faltan ['A62-A7R2-R1']", "AGREED exige el hallazgo previo A62-A7R2-R1 en CORRECTED"])
    p("115b-resolucion-repetida", lambda res: res["PriorFindingResolution"].append(copy.deepcopy(res["PriorFindingResolution"][0])),
      ["PriorFindingResolution con identificadores repetidos", "PriorFindingResolution debe disponer cada id una vez",
       "AGREED exige el hallazgo previo A62-A7R2-R1 en CORRECTED"])
    p("116-tema-de-regresion-ausente", drop(["RegressionCheck"], "Topic", "R3_A62_RULE1_PRECISION"),
      ["RegressionCheck debe disponer cada id una vez (faltan ['R3_A62_RULE1_PRECISION']",
       "AGREED exige todos los temas de RegressionCheck en NO_REGRESSION"])
    p("116b-tema-de-regresion-repetido", dup(["RegressionCheck"], "Topic", "R2_INVALIDATOR_RULE_AND_PROCEDURE", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"),
      ["RegressionCheck con identificadores repetidos", "RegressionCheck debe disponer cada id una vez"])
    p("117-cambio-ausente", drop(["DeltaPurity", "Changes"], "Change", "PROCEDURE_PIECES_ROUND3"),
      ["DeltaPurity.Changes debe disponer cada id una vez (faltan ['PROCEDURE_PIECES_ROUND3']",
       "AGREED exige DeltaPurity CONFIRMED con todos los cambios de la ronda 3 WITHIN_SCOPE"])
    p("117b-cambio-repetido", dup(["DeltaPurity", "Changes"], "Change", "R2R1_REBASE_ROLE_REVERIFICATION_ONLY", "DESCRIPTIVE_SECTIONS"),
      ["DeltaPurity.Changes con identificadores repetidos", "DeltaPurity.Changes debe disponer cada id una vez"])
    p("118-disparador-ausente", drop(["Materiality", "Triggers"], "Trigger", "M-07"),
      ["Materiality.Triggers debe disponer cada id una vez (faltan ['M-07']", "AGREED exige los ocho disparadores de Materiality"])
    p("118b-disparador-repetido", dup(["Materiality", "Triggers"], "Trigger", "M-01", "M-06"),
      ["Materiality.Triggers con identificadores repetidos", "Materiality.Triggers debe disponer cada id una vez"])
    p("119-pregunta-ausente", drop(["QuestionAnswers"], "Question", "Q-A7R3-03"),
      ["QuestionAnswers debe disponer cada id una vez (faltan ['Q-A7R3-03']"])
    p("119b-pregunta-repetida", dup(["QuestionAnswers"], "Question", "Q-A7R3-01", "Q-A7R3-02"),
      ["QuestionAnswers con identificadores repetidos", "QuestionAnswers debe disponer cada id una vez"])

    def stop_with_dup(res):
        stop(res, None)
        res["RegressionCheck"] = [{"Topic": "T04_EFF_EFF1_B1_MAP_VALIDATION", "Status": "NO_REGRESSION", "Rationale": "x", "ClauseRefs": [],
                                   "FindingIds": []}] * 2
    p("120-not-accredited-con-ids-repetidos", stop_with_dup, ["RegressionCheck con identificadores repetidos"], base=stop_calls)

    # ---- direct read of AUTOMATION_PLAN 16.7 L593-L595 (round 3; the rule of v5.x) and the references of nested elements
    apd = ref(AP, *AP_DIRECT)
    p("121-sin-lectura-directa-de-ap", lambda res: None,
      ["lectura directa: %s no entregadas fielmente" % apd, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % apd],
      base=lambda r: base_calls(r, direct=False))
    p("121b-cita-de-ap-leida-solo-en-a7", lambda res: mat(res)["ClauseRefs"].append(apd),
      ["lectura directa: %s no entregadas fielmente" % apd, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % apd,
       "referencia sobre líneas no entregadas fielmente en Materiality (%s)" % apd], base=lambda r: base_calls(r, direct=False))
    p("121c-directreads-sin-ap", lambda res: res.update({"DirectReads": [{"Ref": sec_ref("### 3.4 "), "Purpose": "x"}]}),
      ["lectura directa: DirectReads sin una entrada que cubra"])
    p("121d-directreads-cubre-solo-593", lambda res: res.update({"DirectReads": [{"Ref": ref(AP, AP_DIRECT[0]), "Purpose": "x"}]}),
      ["lectura directa: DirectReads sin una entrada que cubra"])

    def cited_after_direct(res):
        mat(res)["ClauseRefs"].append(apd)
        rrd(res)[PF]["ClauseRefs"].append(apd)
        res["DirectReads"].append({"Ref": sec_ref("### 3.4 "), "Purpose": "x"})
    p("121e-cita-de-ap-tras-la-lectura-directa-aceptada", cited_after_direct, coherent=True)
    p("121f-referencia-anidada-no-leida", lambda res: trd(res)["M-02"]["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en Materiality.Triggers[M-02] (%s)" % ref(A6, 10, 12)])
    p("121g-referencia-anidada-de-evidencia-leida", lambda res: chd(res)["R2R1_REBASE_COHERENCE"]["ClauseRefs"].append(preflight_ref()), coherent=True)
    p("121h-referencia-de-la-pureza-no-leida", lambda res: dp(res)["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en DeltaPurity (%s)" % ref(A6, 10, 12)])
    p("121i-referencia-de-la-compatibilidad-no-leida", lambda res: cvd(res)["A6"]["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en %s (%s)" % (CVA6, ref(A6, 10, 12))])
    p("121j-referencia-a-la-ronda-2-y-a-su-salida-aceptada",
      lambda res: rrd(res)[PF]["ClauseRefs"].extend(["a7-r2.md:197-200", ref(R2OUT, 3), ref(Q6, 3)]), coherent=True)

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
    for row in fidelity_cases():
        cases.append(row)
    return cases


# ------------------------------------------------------------------ read fidelity and faithful copies (decisions §94, point 5; v5.6-a7)
TRX = "docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/fixture-tests.trx"   # the r1 failure: line 1 DEGRADED (BOM dropped)
TRX_MAIN, TRX_UC = EVID + "/b1-like/main/fixture-tests.trx", EVID + "/b1-like/uc/fixture-tests.trx"   # the round-3 B1-like case
FIX = "zz-fidelity-fixture/"                    # fixture of the phase-1 test clone (only there); the cases that need it are skipped elsewhere
T03 = "A62-A7R2-R1"                            # the element the fidelity cases cite from


def copy_of(orig):
    return next(e for e in PR.CLOSURE["FaithfulCopies"] if e["Original"] == orig)["Copy"]


def read_copy(r, orig, a=None, b=None, alter=None):
    """Read of the faithful copy of `orig` in the run (lines a..b of the copy; whole by default); `alter(lines)` simulates a transport change."""
    name = copy_of(orig)
    L = lines_of(run=name)
    a, b = (1, len(L)) if a is None else (a, b)
    got = list(L[a - 1:b])
    if alter:
        got = alter(got)
    inp = {"file_path": RUN + "\\" + name} if (a, b) == (1, len(L)) else {"file_path": RUN + "\\" + name, "offset": a, "limit": b - a + 1}
    r.call("Read", inp, numbered(got, a))


def copy_layout(orig):
    return PR.FID.layout(PR.read_run_bytes(copy_of(orig)))


def cite(rel, a, b=None):
    def p(res, r):
        rrd(res)[T03]["ClauseRefs"].append(ref(rel, a, b))
    return p


def fvariant(name, check=None, **kw):
    row, out = variant(name, **kw)
    if check is not None:
        ok, detail = check(out)
        row["FidelityCheck"] = detail
        if not ok:
            row["Result"] = "FAIL"
    return row, out


def copy_doc(out, orig):
    return next((x for x in out["Fidelity"]["FaithfulCopies"] if x["Original"] == orig), {})


def fidelity_cases():
    cases = []
    add = lambda rv: cases.append(rv[0])
    BOMC = chr(0xFEFF)
    trx_copy = copy_of(TRX)
    custody = "archivo del run distinto del custodiado (copia del kit y RunFileHashes del cierre): "
    not_faithful = "copia fiel: %s no entrega byte a byte %s"
    not_delivered = "referencia sobre líneas no entregadas fielmente en PriorFindingResolution[%s] (%s)"

    # F00: the base flow reads flagged ORIGINALS (V14, decisions) only on lines Read delivers faithfully: accredited, recorded as informational
    add(fvariant("F00-base-lecturas-de-originales-marcados-informativas", require_coherent=True,
                 check=lambda out: (sorted({x["Path"] for x in out["Fidelity"]["FlaggedOriginalReads"]}) == sorted([DEC, AP]) and
                                    all(x["Status"] == "FAITHFUL_NORMALIZED" for x in out["Fidelity"]["FlaggedOriginalReads"]) and
                                    all(x["Verified"] for x in out["Fidelity"]["FaithfulCopies"]) and
                                    len(out["Fidelity"]["FaithfulCopies"]) == len(PR.CLOSURE["FaithfulCopies"]),
                                    {"FlaggedOriginalReads": [x["Path"] for x in out["Fidelity"]["FlaggedOriginalReads"]],
                                     "CopiesVerified": sum(1 for x in out["Fidelity"]["FaithfulCopies"] if x["Verified"])})))

    # F01: the r1 failure reproduced — the TRX ORIGINAL read directly, line 1 delivered without its BOM: DEGRADED, a reason (never faithful)
    def bom_dropped(r):
        L = lines_of(TRX)
        r.call("Read", {"file_path": absp(TRX)}, numbered([L[0].lstrip(BOMC)] + L[1:]))
    add(fvariant("F01-bom-original-leido-directamente", extra=bom_dropped, result_patch=cite(TRX, 1),
                 expected=["fidelidad: %s (Read, llamada" % TRX, "líneas alteradas [1]", not_delivered % (T03, ref(TRX, 1))],
                 counts={"fidelidad: ": 1}))

    # F02: the TRX read through its faithful copy and cited by the ORIGINAL path: credited, accredited
    n_trx = len(lines_of(TRX))
    add(fvariant("F02-bom-copia-leida-y-citada", extra=lambda r: read_copy(r, TRX), result_patch=cite(TRX, 1, n_trx), require_coherent=True,
                 check=lambda out: (copy_doc(out, TRX).get("OriginalLinesCredited") == n_trx and copy_doc(out, TRX).get("Verified") is True,
                                    copy_doc(out, TRX))))

    # F03: the copy altered by ONE byte in the run (read and cited): custody, the copy check and the reference
    def flip(name, pos_from_text):
        def rb(n):
            b = ORIG_READ_RUN(n)
            if n != name:
                return b
            i = b.index(pos_from_text)
            return b[:i] + bytes([b[i] ^ 0x01]) + b[i + 1:]
        return rb
    try:
        PR.read_run_bytes = flip(trx_copy, b"<TestRun ")
        add(fvariant("F03-copia-alterada-un-byte-en-el-run", extra=lambda r: read_copy(r, TRX), result_patch=cite(TRX, 1, 3),
                     expected=[custody + trx_copy, not_faithful % (trx_copy, TRX), "no reproduce los bytes del original",
                               "SHA-256 de la copia distinto del fijado en el cierre", not_delivered % (T03, ref(TRX, 1, 3))]))
    finally:
        PR.read_run_bytes = ORIG_READ_RUN

    # F04: the copy intact on disk, its line 1 delivered altered by the transport (the BOM escape dropped): DEGRADED on the copy, nothing credited
    add(fvariant("F04-copia-entregada-alterada", extra=lambda r: read_copy(r, TRX, alter=lambda g: [g[0].replace("\\xEF\\xBB\\xBF", "", 1)] + g[1:]),
                 result_patch=cite(TRX, 1),
                 expected=["fidelidad: %s (Read, llamada" % trx_copy, "líneas alteradas [1]", not_delivered % (T03, ref(TRX, 1))]))

    # F05-F07: a long line (V14 L2352): the copy with its appendix credits it; without the appendix it does not; the ORIGINAL truncates it
    lay = copy_layout(V14)

    def long_full(r):
        read_copy(r, V14, 2350, 2353)
        for x in lay[2352][1:]:
            read_copy(r, V14, x, x)
    add(fvariant("F05-linea-larga-copia-con-apendice", extra=long_full, result_patch=cite(V14, 2352), require_coherent=True,
                 check=lambda out: (len(lay[2352]) >= 2 and copy_doc(out, V14).get("OriginalLinesCredited") == 4,
                                    {"Chunks": len(lay[2352]), "Credited": copy_doc(out, V14).get("OriginalLinesCredited")})))
    add(fvariant("F06-linea-larga-copia-sin-apendice", extra=lambda r: read_copy(r, V14, 2350, 2353), result_patch=cite(V14, 2352),
                 expected=[not_delivered % (T03, ref(V14, 2352))]))

    def long_truncated(r):
        L = lines_of(V14)
        got = L[2349:2353]
        got[2] = got[2][:2000] + "…"
        r.call("Read", {"file_path": absp(V14), "offset": 2350, "limit": 4}, numbered(got, 2350))
    add(fvariant("F07-linea-larga-original-truncada", extra=long_truncated, result_patch=cite(V14, 2352),
                 expected=[not_delivered % (T03, ref(V14, 2352))],
                 check=lambda out: ([(x["Path"], x["Line"]) for x in out["Fidelity"]["TruncatedKnownLongLines"]] == [(V14, 2352)],
                                    out["Fidelity"]["TruncatedKnownLongLines"])))

    # F08: the direct read of AUTOMATION_PLAN 16.7 L593-L595 through its faithful copy satisfies Step 0
    def direct_by_copy(r):
        r.header()
        read_copy(r, AP, *AP_READ)
        step0(r, direct=False)
        a7_reads(r)
        evidence_reads(r)
        acceptor_reads(r)
        a6_reads(r)
    add(fvariant("F08-lectura-directa-por-la-copia-de-automation-plan", base=direct_by_copy, require_coherent=True,
                 check=lambda out: (out["DirectRead"]["DeliveredByRead"] is True and
                                    AP not in [x["Path"] for x in out["Fidelity"]["FlaggedOriginalReads"]], out["DirectRead"])))

    # F09: a faithful copy is never a reference (it is not a premise run file): cite the original
    add(fvariant("F09-referencia-a-la-copia", extra=lambda r: read_copy(r, TRX),
                 result_patch=lambda res, r: rrd(res)[T03]["ClauseRefs"].append(trx_copy + ":1"),
                 expected=["resultado: no valida contra result.schema.json",
                           "referencia no canónica en PriorFindingResolution[%s] (%s:1: no es un archivo del cierre ni una premisa del run)"
                           % (T03, trx_copy)], schema_valid=False))

    # F10-F14: the copy itself absent, another original's copy, a non-canonical spelling, the BOM silently dropped INSIDE the copy, and the
    # original's blob pin altered in the closure: each a reason, nothing credited
    main_copy, uc_copy = copy_of(TRX_MAIN), copy_of(TRX_UC)

    def replace_run(name, fn):
        def rb(n):
            if n != name:
                return ORIG_READ_RUN(n)
            return fn(ORIG_READ_RUN(n))
        return rb

    def absent(n):
        if n == trx_copy:
            raise OSError("missing")
        return ORIG_READ_RUN(n)
    try:
        PR.read_run_bytes = absent
        add(fvariant("F10-copia-ausente", expected=[custody + trx_copy, not_faithful % (trx_copy, TRX), "copia ausente"]))
    finally:
        PR.read_run_bytes = ORIG_READ_RUN
    try:
        PR.read_run_bytes = replace_run(main_copy, lambda b: ORIG_READ_RUN(uc_copy))
        add(fvariant("F11-copia-de-otro-original", expected=[custody + main_copy, not_faithful % (main_copy, TRX_MAIN),
                                                               "la copia nombra otro original (%s)" % TRX_UC,
                                                               "SHA-256 de la copia distinto del fijado en el cierre"]))
    finally:
        PR.read_run_bytes = ORIG_READ_RUN
    try:
        PR.read_run_bytes = replace_run(trx_copy, lambda b: b.replace(b"<TestRun ", b"\\x3CTestRun ", 1))
        add(fvariant("F12-copia-no-canonica", expected=[custody + trx_copy, not_faithful % (trx_copy, TRX),
                                                          "la copia no es la codificación canónica de lo que decodifica",
                                                          "SHA-256 de la copia distinto del fijado en el cierre"]))
    finally:
        PR.read_run_bytes = ORIG_READ_RUN
    try:
        PR.read_run_bytes = replace_run(trx_copy, lambda b: PR.FID.encode(TRX, PR.canon_bytes(TRX)[3:]))
        add(fvariant("F13-bom-eliminado-dentro-de-la-copia", extra=lambda r: read_copy(r, TRX), result_patch=cite(TRX, 1),
                     expected=[custody + trx_copy, not_faithful % (trx_copy, TRX), "no reproduce los bytes del original",
                               "SHA-256 de la copia distinto del fijado en el cierre", not_delivered % (T03, ref(TRX, 1))]))
    finally:
        PR.read_run_bytes = ORIG_READ_RUN
    saved = PR.FAITHFUL[trx_copy]["OriginalBlob"]
    try:
        PR.FAITHFUL[trx_copy]["OriginalBlob"] = "0" * 40
        add(fvariant("F14-blob-del-original-distinto-del-fijado", extra=lambda r: read_copy(r, TRX), result_patch=cite(TRX, 1),
                     expected=["copia fiel: %s no entrega byte a byte %s (blob 00000000)" % (trx_copy, TRX),
                               "blob del original distinto del fijado en el cierre", not_delivered % (T03, ref(TRX, 1))]))
    finally:
        PR.FAITHFUL[trx_copy]["OriginalBlob"] = saved

    # F15-F20: the classes of the phase-1 fixture (bare CR, NUL, invalid UTF-8, controls, U+FEFF inside, a line separator, a PDF extension, a CRLF
    # file with a BOM): the ORIGINAL delivered as Read could deliver it is DEGRADED or a failed read; the copy, read and cited, is credited
    fx = [e["Original"] for e in PR.CLOSURE["FaithfulCopies"] if e["Original"].startswith(FIX)]
    if not fx:
        cases.append({"Case": "F15-F20-fixture-de-clases", "Result": "PASS", "Skipped": "sin el fixture de la fase 1 en este cierre"})
        return cases

    def orig_split_cr(r):
        r.call("Read", {"file_path": absp(FIX + "bare-cr.txt")}, numbered(["linea 1", "antes", "despues del CR suelto", "linea 3"]))
    add(fvariant("F15-cr-suelto-original-desplazado", extra=orig_split_cr, result_patch=cite(FIX + "bare-cr.txt", 3),
                 expected=["fidelidad: %sbare-cr.txt (Read, llamada" % FIX, "líneas alteradas [2, 3, 4]",
                           not_delivered % (T03, ref(FIX + "bare-cr.txt", 3))]))

    def orig_nul_failed(r):
        r.call("Read", {"file_path": absp(FIX + "nul-byte.txt")}, "This tool cannot read binary files.", is_error=True)
    add(fvariant("F16-nul-original-lectura-fallida", extra=orig_nul_failed, result_patch=cite(FIX + "nul-byte.txt", 2),
                 expected=[not_delivered % (T03, ref(FIX + "nul-byte.txt", 2))],
                 check=lambda out: ([x["Path"] for x in out["Fidelity"]["FailedReadsNeverCredited"]] == [FIX + "nul-byte.txt"],
                                    out["Fidelity"]["FailedReadsNeverCredited"])))

    def orig_invalid(r):
        L = lines_of(FIX + "invalid-utf8.txt")
        r.call("Read", {"file_path": absp(FIX + "invalid-utf8.txt")},
               numbered([L[0], "byte %s suelto y secuencia truncada %s" % (chr(0xFFFD), chr(0xFFFD)), L[2]]))
    add(fvariant("F17-utf8-invalido-original-con-sustitucion", extra=orig_invalid, result_patch=cite(FIX + "invalid-utf8.txt", 2),
                 expected=["fidelidad: %sinvalid-utf8.txt (Read, llamada" % FIX, "líneas alteradas [2]",
                           not_delivered % (T03, ref(FIX + "invalid-utf8.txt", 2))],
                 check=lambda out: (len(out["Fidelity"]["UnicodeReplacementChars"]) == 1, out["Fidelity"]["UnicodeReplacementChars"])))

    def all_fixture_copies(r):
        for o in fx:
            read_copy(r, o)

    def cite_all(res, r):
        for o in fx:
            rrd(res)[T03]["ClauseRefs"].append(ref(o, 1, len(lines_of(o))))
    add(fvariant("F18-copias-del-fixture-leidas-y-citadas", extra=all_fixture_copies, result_patch=cite_all, require_coherent=True,
                 check=lambda out: (all(copy_doc(out, o).get("OriginalLinesCredited") == len(lines_of(o)) for o in fx) and len(fx) == 10,
                                    {o: copy_doc(out, o).get("OriginalLinesCredited") for o in fx})))

    def orig_long_fixture(r):
        L = lines_of(FIX + "long-line.txt")
        r.call("Read", {"file_path": absp(FIX + "long-line.txt")}, numbered([L[0], L[1][:2000] + "…", L[2]]))
    add(fvariant("F19-linea-larga-del-fixture-original-truncada", extra=orig_long_fixture, result_patch=cite(FIX + "long-line.txt", 1, 3),
                 expected=[not_delivered % (T03, ref(FIX + "long-line.txt", 1, 3))],
                 check=lambda out: ([(x["Path"], x["Line"]) for x in out["Fidelity"]["TruncatedKnownLongLines"]] == [(FIX + "long-line.txt", 2)],
                                    out["Fidelity"]["TruncatedKnownLongLines"])))
    try:
        PR.read_run_bytes = replace_run(copy_of(FIX + "control.txt"), lambda b: b.replace(b"a\\x09b", b"a\\x09c", 1))
        add(fvariant("F20-copia-del-fixture-alterada-un-byte", extra=lambda r: read_copy(r, FIX + "control.txt"),
                     result_patch=cite(FIX + "control.txt", 1),
                     expected=[custody + copy_of(FIX + "control.txt"), not_faithful % (copy_of(FIX + "control.txt"), FIX + "control.txt"),
                               "no reproduce los bytes del original", "SHA-256 de la copia distinto del fijado en el cierre",
                               not_delivered % (T03, ref(FIX + "control.txt", 1))]))
    finally:
        PR.read_run_bytes = ORIG_READ_RUN
    return cases


# ------------------------------------------------------------------ gate 11 cases (owner_gate.evaluate on decision texts built from the clone's; A-7)
def owner_cases():
    """Gate 11 of the round-3 kit (owner_gate.evaluate, pure): the clone's decisions end at §96 (the base section); the authorization is a FUTURE
    disposition after §96, played here by a SYNTHETIC §97 appended to the clone's text (never a real disposition)."""
    cases = []
    dec_lines = clone_decision_lines()
    a6_text = "\n".join(PR.canon(A6))
    blob, run_id = PR.OBJECT_BLOB, PR.IDS["RunId"]
    sha_ok = PR.TRANSPORT["Binary"]["Sha256"]
    a6l = next(l for l in dec_lines if OG.line_sha(l) == OG.A6_LINE_SHA)
    claude_l = next(l for l in dec_lines if OG.line_sha(l) == OG.CLAUDE_LINE_SHA)
    s78 = next(i for i, l in enumerate(dec_lines) if l.startswith("## 78. "))
    s_base = next(i for i, l in enumerate(dec_lines) if l.startswith("## %d. " % BASE_SECTION))
    assert not any(l.startswith("## ") for l in dec_lines[s_base + 1:]), "§96 no es la última sección de las decisiones del clon"
    AS = SYNTH_AUTH_SECTION
    base_lines = list(dec_lines)
    dec_base = "\n".join(base_lines) + "\n"              # decisiones del clon (terminan en §96): el texto de referencia de la compuerta
    syn_text = "\n" + "\n".join(synthetic_authorization(blob, run_id, AS)) + "\n"
    dec_auth = dec_base + syn_text                        # la HEAD del worktree con una disposición posterior a §96 (SINTÉTICA)
    pending = {"Section": None, "Heading": None, "TextSha256": None}

    def pin(text, number=AS):
        v = PA.values(text, number, blob)["Values"]
        return {"Section": number, "Heading": v["AUTH_HEADING"], "TextSha256": v["AUTH_TEXT_SHA"]}
    auth95 = pin(dec_auth)

    def text(lines):
        return "\n".join(lines) + "\n"

    def before_auth(extra):
        return text(base_lines + extra)

    def replaced(old, new):
        out = "\n".join(new if l == old else l for l in base_lines) + "\n"
        assert out != dec_base or old == new
        return out

    def run_case(name, dec_head, expected=(), clean=True, descends=True, measured=None, clone_text=None, obj_head=None, pinned=None, ok=None,
                 dec_cl=None, auth=None, a6_head=None):
        ev_ = OG.evaluate(dec_head, dec_base if dec_cl is None else dec_cl, clean, descends, sha_ok if measured is None else measured, sha_ok,
                          a6_text if clone_text is None else clone_text, blob if obj_head is None else obj_head, blob if pinned is None else pinned,
                          run_id, OG.A6_AGREED_BLOB if a6_head is None else a6_head, auth95 if auth is None else auth)
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
    NOAUTH, LATER, UNIQUE, NOTAFTER = "falta la autorización", "sección posterior a §%d" % AS, \
        "decisiones sin un único encabezado §%d" % AS, "no es una sección posterior a §%d" % BASE_SECTION
    e = run_case("O00-autorizacion-pendiente-bloquea", dec_base, [PEND], auth=pending)
    if not (e["AuthorizationPending"] is True and e["AuthorizationSections"] == [] and e["LaterSections"] == [] and e["Problems"] == [PEND] and
            e["SectionsAfterBase"] == []):
        cases[-1]["Result"] = "FAIL"
    e = run_case("O00b-pendiente-con-una-seccion-posterior-a-96", dec_base + "\n## %d. Nota (selftest)\n" % AS, [PEND], auth=pending)
    if [x["Number"] for x in e["SectionsAfterBase"]] != [AS] or e["Problems"] != [PEND]:
        cases[-1]["Result"] = "FAIL"
    fresh = load("owner_gate_as_frozen", os.path.join(KITDATA, "owner_gate.py"))
    pins = fresh.pinned_authorization()
    structural = (all(v is None for v in pins.values()) or
                  (type(pins["Section"]) is int and pins["Section"] > fresh.BASE_SECTION and str(pins["Heading"]).startswith("## %d. " % pins["Section"])
                   and re.match(r"^[0-9a-f]{64}$", str(pins["TextSha256"])) is not None))
    cases.append({"Case": "O00c-modulo-del-kit-pendiente-o-fijado-entero", "Pins": pins, "Pending": fresh.authorization_pending(pins),
                  "Result": "PASS" if structural and fresh.BASE_SECTION == BASE_SECTION and fresh.PENDING_REASON == PEND and
                  fresh.OBJECT_BLOB == blob and fresh.RUN_ID == run_id else "FAIL"})
    run_case("O01-autorizacion-posterior-a-96-fijada-pasa", dec_auth)
    run_case("O01b-autorizacion-con-el-texto-de-la-head-como-referencia", dec_auth, dec_cl=dec_auth)
    run_case("O02-seccion-posterior-a-la-fijada", dec_auth + "\n## %d. Disposición del Coordinator: otra (selftest)\n\ntexto\n" % (AS + 1), [LATER])
    run_case("O03-seccion-99-cualquiera-posterior", dec_auth + "\n## 99. Nota (selftest)\n", [LATER])
    t = dec_auth.rstrip("\n") + " (selftest)\n"
    run_case("O04-fijada-con-otro-texto", t, [NOAUTH])
    t = dec_base + syn_text.replace("Disposición del Coordinator", "Disposición del Owner")
    run_case("O05-fijada-con-otro-encabezado", t, [NOAUTH])
    t = dec_base + syn_text.replace(blob, blob[:8] + "…")
    run_case("O06-fijada-con-el-blob-abreviado", t, [NOAUTH], auth=pin(t))
    t = dec_base + syn_text.replace(blob, blob + "0")
    run_case("O07-fijada-con-el-blob-pegado", t, [NOAUTH], auth=pin(t))
    run_case("O08-sin-la-seccion-fijada", dec_base, [UNIQUE + " (0)", NOAUTH])
    run_case("O09-dos-secciones-fijadas", dec_auth + "\n## %d. Duplicado (selftest)\n" % AS, [UNIQUE + " (2)", NOAUTH])
    run_case("O10-fijada-no-posterior-a-96", dec_base, [NOTAFTER, NOAUTH], auth=pin(dec_base, BASE_SECTION))
    run_case("O10b-fijada-con-un-numero-que-no-es-entero", dec_auth, [NOTAFTER, "decisiones sin un único encabezado §%d (0)" % AS, NOAUTH],
             auth=dict(auth95, Section=str(AS)))
    run_case("O11-a7-en-la-head-con-otro-blob", dec_auth, ["no tiene el blob de A-7 fijado en el kit"], obj_head=OTHER_A7_BLOB)
    run_case("O11b-a6-en-la-head-con-otro-blob", dec_auth, ["no tiene el blob acordado"], a6_head=OTHER_A6_BLOB)
    run_case("O11c-a6-ausente-en-la-head", dec_auth, ["no tiene el blob acordado"], a6_head="")
    run_case("O12-blob-no-fijado", dec_auth, ["blob de A-7 no fijado en el kit", "no tiene el blob de A-7 fijado", NOAUTH], pinned="")
    run_case("O12b-blob-fijado-de-la-ronda-2", dec_auth, ["blob de A-7 no fijado en el kit", "no tiene el blob de A-7 fijado", NOAUTH],
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
    t = before_auth(["A6-ADR0046-9-FX02 = B (revoca la determinación anterior)", ""])
    run_case("O20-revocacion-anterior-a-la-autorizacion", t + syn_text, ["otra línea que empieza por «A6-ADR0046-9-FX02 =»"], dec_cl=t,
             auth=pin(t + syn_text))
    t = replaced(claude_l, "CLAUDE-CLI-I62 = (retirada)")
    run_case("O21-sin-claude-cli-i62", t + syn_text, ["falta, como línea completa de decisiones, la línea del Owner CLAUDE-CLI-I62 = A",
                                                      "otra línea que empieza por «CLAUDE-CLI-I62 =»"], dec_cl=t, auth=pin(t + syn_text))
    t = before_auth(["CLAUDE-CLI-I62 = B (revocada)", ""])
    run_case("O22-otra-linea-claude-cli-i62", t + syn_text, ["otra línea que empieza por «CLAUDE-CLI-I62 =»"], dec_cl=t, auth=pin(t + syn_text))
    t = dec_auth.replace("suma 3 198 bytes", "suma 3 199 bytes")
    assert t != dec_auth
    run_case("O23-decision-96-cambiada-en-la-head", t, ["no crece solo por añadido"], dec_cl=dec_base)
    t = dec_auth.replace("suma 3 299 bytes", "suma 3 298 bytes")
    assert t != dec_auth
    run_case("O23b-decision-92-cambiada-en-la-head", t, ["no crece solo por añadido"], dec_cl=dec_base)
    run_case("O24-cambios-sin-commit", dec_auth, ["cambios sin commit"], clean=False)
    run_case("O25-head-no-desciende-del-clon", dec_auth, ["no desciende del commit del clon"], descends=False)
    run_case("O26-otro-binario", dec_auth, ["SHA-256 medido del binario distinto del cierre"], measured="0" * 64)
    run_case("O27-linea-fijada-ausente-de-la-a6-acordada", dec_auth, ["no está, como línea completa, en la A-6 acordada"],
             clone_text=a6_text.replace(a6l, para))
    t = before_auth(["## 78. Duplicado (selftest)", ""])
    run_case("O28-dos-encabezados-78", t + syn_text, ["decisiones sin un único encabezado §78 (2)"], dec_cl=t, auth=pin(t + syn_text))
    run_case("O29-sha-del-texto-fijado-distinto", dec_auth, [NOAUTH], auth=dict(auth95, TextSha256="0" * 64))
    v_ok = PA.values(dec_auth, AS, blob)
    v_later = PA.values(dec_auth + "\n## %d. Otra (selftest)\n" % (AS + 1), AS, blob)
    v_base = PA.values(dec_base, BASE_SECTION, blob)
    v_none = PA.values(dec_base, AS, blob)
    lines = PA.python_lines(v_ok["Values"])
    ok30 = (v_ok["Ready"] is True and v_ok["Values"]["AUTH_HEADING"] == auth95["Heading"] and v_ok["Values"]["AUTH_TEXT_SHA"] == auth95["TextSha256"] and
            v_later["Ready"] is False and [x["Number"] for x in v_later["Checks"]["LaterSections"]] == [AS + 1] and
            v_base["Ready"] is False and v_base["Checks"]["AfterBaseSection"] is False and v_base["Checks"]["HasBlobToken"] is False and
            v_none["Ready"] is False and v_none["Values"]["AUTH_HEADING"] is None and
            len(lines) == 3 and lines[0].startswith("AUTH_SECTION = %d" % AS) and ast.literal_eval(lines[1].split(" = ", 1)[1]) == auth95["Heading"] and
            ast.literal_eval(lines[2].split(" = ", 1)[1].split("   #")[0]) == auth95["TextSha256"])
    cases.append({"Case": "O30-pin-authorization-valores", "Values": v_ok["Values"], "PythonLines": lines,
                  "LaterReady": v_later["Ready"], "BaseSectionReady": v_base["Ready"], "AbsentReady": v_none["Ready"], "Result": "PASS" if ok30 else "FAIL"})
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
    # X05 (decisions §94, point 5): launch.py's check 7 (run_files_check, read with ast) expects exactly the auditor's run files (order.txt,
    # prompt.md and the faithful copies) and blocks a closure without FaithfulCopies, a manifest without a copy, a run without a copy and a run
    # with launch/
    node = LAUNCH.get("run_files_check_node")
    if node is None:
        row("X05-comprobacion-7-del-lanzador-con-copias-fieles", False, Got="launch.py sin run_files_check")
        return cases
    ns = {"os": os, "FID": PR.FID, "RUN_FILES": LAUNCH["RUN_FILES"], "sha_file": lambda p: hashlib.sha256(open(p, "rb").read()).hexdigest()}
    exec(compile(ast.Module(body=[node], type_ignores=[]), "launch.py:run_files_check", "exec"), ns)
    f = ns["run_files_check"]
    man = {"RunFiles": {n: {"Sha256": h["Sha256"]} for n, h in PR.CLOSURE["RunFileHashes"].items()}}
    last = PR.RUN_FILES[-1]
    ok_ = f(PR.CLOSURE, man, RUN, KITDATA)
    no_fc = f({k: v for k, v in PR.CLOSURE.items() if k != "FaithfulCopies"}, man, RUN, KITDATA)
    no_man = f(PR.CLOSURE, {"RunFiles": {n: v for n, v in man["RunFiles"].items() if n != last}}, RUN, KITDATA)
    t1, t2 = os.path.join(TMPROOT, "x05a"), os.path.join(TMPROOT, "x05b")
    for t, names, extra_dir in ((t1, PR.RUN_FILES[:-1], False), (t2, PR.RUN_FILES, True)):
        os.makedirs(t, exist_ok=True)
        for n in names:
            shutil.copyfile(os.path.join(RUN, n), os.path.join(t, n))
        if extra_dir:
            os.makedirs(os.path.join(t, "launch"), exist_ok=True)
    no_copy, with_launch = f(PR.CLOSURE, man, t1, KITDATA), f(PR.CLOSURE, man, t2, KITDATA)
    row("X05-comprobacion-7-del-lanzador-con-copias-fieles",
        ok_["Ok"] is True and ok_["Expected"] == PR.RUN_FILES and no_fc["Ok"] is False and any("FaithfulCopies" in str(x) for x in no_fc["Mismatches"]) and
        no_man["Ok"] is False and no_man["Mismatches"] == [last] and no_copy["Ok"] is False and with_launch["Ok"] is False and
        with_launch["Mismatches"] == [],
        RunFiles=len(PR.RUN_FILES), Copies=len(PR.CLOSURE["FaithfulCopies"]), Consistent=ok_["Ok"], WithoutFaithfulCopies=no_fc["Mismatches"],
        ManifestWithoutLast=no_man["Mismatches"], RunWithoutLast=no_copy["Ok"], RunWithLaunch=with_launch["Ok"])
    # X06 (decisions §96, point 6): the reference pattern of result.schema.json (the same in every Ref, Evidence and ClauseRefs) admits every
    # closure path and the premise run files, and none of the negatives (the misspelled path of round 2, absolute, leading slash, backslashes,
    # `..`, `.`, a faithful copy, prompt.md, the round-1 premise, a file outside the closure, no line, line 0, another spelling of a premise)
    pats = []

    def walk(o):
        if isinstance(o, dict):
            if isinstance(o.get("pattern"), str) and o["pattern"].endswith(":[1-9][0-9]*(-[1-9][0-9]*)?$"):
                pats.append(o["pattern"])
            for v in o.values():
                walk(v)
        elif isinstance(o, list):
            for v in o:
                walk(v)
    walk(json.load(open(SCHEMA, encoding="utf-8")))
    rx = re.compile(pats[0]) if pats else None
    positives = [x["Path"] + ":1" for x in PR.CLOSURE["CanonicalInputs"]] + [n + ":3" for n in PR.CLOSURE["PremiseRunFiles"]]
    negatives = ["docs/initiatives/I-62-A7.md:120", "/" + A7 + ":1", absp(A7) + ":1", A7.replace("/", "\\") + ":1", EVID + "/../r2/summary.json:3",
                 EVID + "/./summary.json:3", PR.RUN_FILES[-1] + ":1", "prompt.md:1", "a7-r1.md:3", "docs/HANDOFF.md:1", A7, A7 + ":0", "ORDER.TXT:3"]
    bad_pos = [x for x in positives if not (rx and rx.match(x))]
    bad_neg = [x for x in negatives if rx and rx.match(x)]
    row("X06-patron-de-referencias-del-esquema", bool(pats) and len(set(pats)) == 1 and len(pats) >= 3 and not bad_pos and not bad_neg,
        Patterns=len(pats), Distinct=len(set(pats)), Positives=len(positives), PositivesRejected=bad_pos, NegativesAccepted=bad_neg)
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
    doc = {"Selftest": "post-review.py v7.0-a7r3 + transport_gate.py + owner_gate.py (A-7 ronda 3) + pin_authorization.values + c3_analysis.analyze + "
                       "coherencia con launch.py y patrón de referencias del esquema",
           "Environment": "SYNTHETIC" if pr.CLOSURE.get("Synthetic") else "REAL (clon y run del kit)",
           "Clone": CLONE, "Run": RUN, "Rev": pr.REV, "Object": {"Path": pr.OBJECT, "Blob": pr.OBJECT_BLOB}, "Provisional": pr.CLOSURE.get("Provisional"),
           "SessionId": pr.SESSION_ID, "RunId": pr.IDS["RunId"], "KitGateStatus": pr.GATE.get("Status"),
           "AuthorizationPinsForAuditCases": PINS,
           "Modules": {n: hashlib.sha256(open(os.path.join(HERE, n), "rb").read()).hexdigest()
                       for n in ("post-review.py", "transport_gate.py", "owner_gate.py", "pin_authorization.py", "c3_analysis.py", "result.schema.json",
                                 "selftest-post-review.py", "launch.py", "fidelity.py")},
           "KitDataModules": {n: hashlib.sha256(open(os.path.join(KITDATA, n), "rb").read()).hexdigest()
                              for n in ("fidelity.py", "owner_gate.py", "transport_gate.py", "closure.json")},
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
