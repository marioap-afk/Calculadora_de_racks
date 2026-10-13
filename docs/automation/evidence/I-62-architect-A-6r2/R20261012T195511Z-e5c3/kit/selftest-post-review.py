"""Self-test of post-review.py v5.2-a6r2, of transport_gate.py, of owner_gate.py (round 2) and of c3_analysis.analyze, run BEFORE the launch of the
ONE formal review of round 2 of A-6 (decisions §80, point 6). Adapted from the self-test of the round-1 kit (R20261011T215735Z-c434): the same
synthetic formats and the same transport, read, custody and gate cases (00-108), the specifics replaced by the round-2 closure (three run files,
order.txt = the §80 body, prompt.md and a6-r1.md; the object is the round-2 A-6) and the result schema rackcad-a6r2-architect-review/0, with cases
109-160 for the round-2 rules (direct read of V14 L2333-L2334, acceptor-contrast references, coverage and coherence of Round1FindingResolution,
Round1OptionalDisposition, GapAssessment, M01Verification, AcceptorCodeContrast and ProvenanceIntegrityAtUse, AGREED, schema) and cases O00-O27
plus the live probe L00 for gate 11 (owner_gate.py: Owner line of §78, CLAUDE-CLI-I62, round-2 blob at the worktree HEAD, the Coordinator
authorization after §80).

Environment. By default it audits against the kit's closure (kit/closure.json: clone = closure Transport.Cwd, run = closure RunDir), as in round 1
(phase 2: the real clone D:\\r62-arch-a6r2 and run D:\\r62-arch-a6r2-run). With RACKCAD_A6R2_KITDATA=<dir> it takes closure.json, the gate modules and
the kit copies of the run files from <dir> (phase 1: the synthetic environment of prep/make_synthetic_env.py, whose closure is built by the same
kit/make_closure.py); post-review.py and result.schema.json always come from this kit directory.
Synthetic claude-cli runs (transcript JSONL in the format measured by the characterization C1, stream-json stdout, and the <run>/launch files
launch.py writes: run.json, preflight.json, stderr.txt and the copies of the transport-gate evidence). Each case audits against a SYNTHETIC gate
(MEASURED by default, built from a c3_analysis.analyze record of a synthetic PASS probe; ACCEPTED in case 41) set through the auditor's GATE seam;
LAUNCH_DIR points to the case's temporary launch directory.

Cases (A = audit, every rule of the auditor has at least one case that FAILS without it; mutation-post-review.py checks that mechanically):
  00-108 as in round 1 (reads/Grep/Glob outside the closure, junction escape, denied tool, Bash, prompt, extra user message, model/effort,
  tampered run file, no result, references, NOT_ACCREDITED stop, AGREED/CHANGES REQUIRED coherence, run.json, preflight.json, transport gate,
  stream-json, transcript, run/launch custody, clone, schema, topics, deltas, findings, materiality, separability, D.5, Owner scope, IfAgreed,
  references, preflight record of gate 11) adapted to round 2; 108b-108d the round-2 part of the gate-11 record; 109-160 the round-2 rules.
Cases G (transport_gate.evaluate and c3_analysis.analyze, unchanged): the valid MEASURED and ACCEPTED evidence, and one case per gate problem.
Cases O (owner_gate.evaluate, pure, on synthetic decision texts built from the clone's decisions) and L00 (owner_gate.check on the real I-62
worktree, read-only git: today it must block on the missing authorization).

Every temporary file lives under <kit>/../.tmp/st and is removed at the end (with a guard against links). The only write outside it is the
temporary junction of case 03 inside the clone (target: a directory of its own under .tmp/st), removed in `finally`; the clone must end clean and
the run files identical to the kit copies.
Usage: python -B selftest-post-review.py <out.json>
"""
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
KITDATA = os.environ.get("RACKCAD_A6R2_KITDATA") or HERE
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
A6 = _CLOSURE["ObjectPath"]
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ADR48 = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
EVID = "docs/automation/evidence/I-62-A6/r2"
GRR, STJ = EVID + "/a6r2-guards-result-run.json", EVID + "/a6r2-selftest.json"
PKG = "docs/automation/evidence/I-62-F6/FX-02/bmin"
SPEC = PKG + "/SPEC.md"
ACC = _CLOSURE["AcceptorPath"]
TOOLF = PKG + "/tool/bmin_facts.py"
R1OUT = "docs/automation/evidence/I-62-architect-A-6/R20261011T215735Z-c434/output.json"
C3_ADD = r"D:\r62-c3-probe\add\canary-add.txt"
C3_OUT = r"D:\r62-c3-probe\out\canary-out.txt"
DELETE = object()
CLONE_MUTATED = False     # True while the junction of case 03 exists (the mutation harness bypasses its caches then)
BLOCKED = "BLOCKED " + chr(0x2014) + " OWNER DECISION"
DELTA_SECTIONS = {"A6-%d" % n: "### 3.%d A6-%d " % (n + 3, n) for n in range(1, 10)}
TOPIC_A6_ROWS = {"Q-A6-01": "§8, Q-A6-01", "Q-A6-02": "§8, Q-A6-02", "Q-A6-06/07": "§8, Q-A6-06, Q-A6-07", "Q-A6-12": "§8, Q-A6-12"}
IFAGREED_KEYS = ("ScopeLimitedToFX02F6ControllerVerify", "NineAppendOnlyParagraphs", "NoChangeToProtectedSurfaces", "FourTopicsSatisfied",
                 "AllNineDeltasAgreed", "Round1FindingsCorrected", "GapsAcceptableAsResiduals", "AcceptorContrastMatches", "ProvenanceAtUseAdequate",
                 "NoAdditionalOwnerDecisionRequired", "D5AndOVExcluded", "SuitableForCoordinatorAgreement")
V14_READ = (2325, 2342)                       # the direct read of Step 0 (L2333-L2334 inside)
USER_RX = re.compile(r'(?i)[a-z]:(?:\\\\|\\|/)+users(?:\\\\|\\|/)+[^\\/"\s]+')


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


def load_auditor(path=None):
    pr = load("post_review_v5_2_a6r2", path or os.path.join(HERE, "post-review.py"))
    pr.setup(CLONE, RUN, KITDATA)
    return pr


PROBE = load("c3_analysis", os.path.join(HERE, "c3_analysis.py"))


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
    r = next(x for x in PR.CLOSURE["CanonicalInputs"] if x["Path"] == rel)
    g = [x for x in r["LineRanges"] if x["Section"].startswith(section_prefix)]
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


# ------------------------------------------------------------------ synthetic transport-gate evidence (unchanged from the round-1 kit)
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
            "AcceptedBy": "principal-session", "Authority": "selftest", "Rationale": "selftest", "At": "2026-10-12T00:00:00Z"}


def synthetic_gate(status, char_b, acc_b=None):
    g = TG.initial_gate(PR.CLOSURE)
    g.update({"Status": status, "Characterization": {"Path": "%TEMP%\\selftest\\characterization.json", "Sha256": sha(char_b)},
              "Acceptance": {"Path": "%TEMP%\\selftest\\acceptance.json", "Sha256": sha(acc_b)} if acc_b is not None else None})
    return g


def gate11_record():
    """The preflight record of gate 11 that launch.py writes when the gate passes (owner_gate.check)."""
    return {"Ok": True, "Problems": [], "A6LineSha256": OG.A6_LINE_SHA, "A6LineNumbers": [1572], "ClaudeCliLineSha256": OG.CLAUDE_LINE_SHA,
            "ClaudeCliLineNumbers": [1085], "A6R2Blob": PR.OBJECT_BLOB, "A6BlobAtHead": PR.OBJECT_BLOB, "A6BlobAtHeadMatches": True,
            "RunId": PR.IDS["RunId"], "Authorized": True,
            "AuthorizationSections": [{"Number": 82, "Line": 1640, "Heading": "## 82. Disposición del Coordinator: (selftest)", "HasBlob": True,
                                       "HasRunId": True}]}


# ------------------------------------------------------------------ synthetic run (unchanged from the round-1 kit, except the gate-11 record)
class Run:
    def __init__(self):
        self.tx, self.so, self.n, self.turns = [], [], 0, 0
        self.res_patch, self.run_patch, self.init_patch, self.so_tx_override = {}, {}, {}, None
        self.final_result, self.no_result, self.no_init = None, False, False
        self.first_prompt = PROMPT
        self.gate_mode, self.gate_patch, self.pf_hook, self.after_write = "MEASURED", {}, None, None
        self.drop_launch, self.extra_launch, self.stdout_outside, self.tx_filter = set(), {}, False, None

    def entry(self, typ, **kw):
        e = {"type": typ, "isSidechain": False, "sessionId": SID, "cwd": CLONE, "version": "2.1.293", "entrypoint": "sdk-cli"}
        e.update(kw)
        self.tx.append(e)
        return e

    def header(self):
        self.tx.append({"type": "queue-operation", "operation": "enqueue", "sessionId": SID, "content": self.first_prompt})
        self.entry("user", message={"role": "user", "content": self.first_prompt}, permissionMode="dontAsk", promptSource="sdk")
        for a in ({"type": "environment", "snapshot": {"workingDirectory": CLONE, "additionalWorkingDirectories": [RUN]}},
                  {"type": "model", "identity": {"modelId": MODEL}}, {"type": "date", "date": "2026-10-12"}):
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
        self.tx.insert(len(self.tx) - 1, {"type": "attachment", "isSidechain": False, "sessionId": SID, "cwd": CLONE, "version": "2.1.293",
                                          "attachment": {"type": "structured_output", "data": copy.deepcopy(tx_input), "toolUseID": uid}})

    def write(self, d):
        launch = os.path.join(d, "launch")
        os.makedirs(launch, exist_ok=True)
        tx = [e for e in self.tx if self.tx_filter is None or self.tx_filter(e)]
        tpath = os.path.join(d, SID + ".jsonl")
        tbytes = "".join(json.dumps(e, ensure_ascii=False) + "\n" for e in tx).encode("utf-8")
        open(tpath, "wb").write(tbytes)
        init = {"type": "system", "subtype": "init", "cwd": CLONE, "session_id": SID, "tools": ["Glob", "Grep", "Read", "StructuredOutput"],
                "mcp_servers": [], "model": MODEL, "permissionMode": "dontAsk", "claude_code_version": "2.1.293", "apiKeySource": "none"}
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
        checks["4-Version"]["Got"] = VERSION
        checks["5-Auth"].update({"loggedIn": True, "authMethod": "claude.ai", "apiProvider": "firstParty"})
        checks["10-TransportGate"].update({"Status": self.gate_mode, "LaunchArgsMatchTemplate": True, "CharacterizationSha256": sha(char_b),
                                          "AcceptanceSha256": sha(acc_b) if acc_b is not None else None, "Problems": []})
        checks[OG.CHECK] = gate11_record()
        pf = {"SessionId": SID, "At": "2026-10-12T00:00:00Z", "Checks": checks}
        if self.pf_hook:
            self.pf_hook(pf)
        open(os.path.join(launch, "preflight.json"), "wb").write(jbytes(pf))
        rj = {"SessionId": SID, "Executable": BINPATH, "ExecutableSha256": BINSHA, "Args": PR.expected_args(SCHEMA), "Cwd": CLONE,
              "PromptSha256": sha(PROMPT_BYTES), "PromptBytes": len(PROMPT_BYTES), "TimeoutSeconds": PR.TRANSPORT["TimeoutSeconds"],
              "TransportGate": {"Status": self.gate_mode, "CharacterizationSha256": sha(char_b), "AcceptanceSha256": sha(acc_b) if acc_b else None},
              "Start": "2026-10-12T00:00:00Z", "Pid": 4242, "End": "2026-10-12T00:10:00Z", "ExitCode": 0, "Killed": False, "TimedOut": False,
              "StdoutSha256": sha(sbytes), "ResultMessage": not self.no_result, "Transcript": {"Path": "x", "Exists": True, "Sha256": sha(tbytes)}}
        patch(rj, self.run_patch)
        open(os.path.join(launch, "run.json"), "wb").write(jbytes(rj))
        for n in self.drop_launch:
            os.remove(os.path.join(launch, n))
        for n, b in self.extra_launch.items():
            open(os.path.join(launch, n), "wb").write(b)
        if self.after_write:
            self.after_write(launch)
        return tpath, spath, launch, gate


# ------------------------------------------------------------------ the round-2 reads and the base result
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
    read_range(r, DEC, *closure_range(DEC, "§79-§80"))
    r.call("Read", {"file_path": absp(GRR)}, numbered(lines_of(GRR)))
    read_range(r, STJ, *closure_range(STJ, "Summary"))
    read_range(r, R1OUT, *closure_range(R1OUT, "RunId, ids"))
    for f in PR.R1_IDS + PR.O1_IDS:
        read_range(r, R1OUT, *closure_range(R1OUT, "hallazgo " + f))


def a6_reads(r):
    n = len(lines_of(A6))
    read_range(r, A6, 300, 599)
    read_range(r, A6, 600, 899)
    read_range(r, A6, 900, n)


def lc_reads(r):
    read_range(r, LC, *closure_range(LC, "## 3. "))


def acceptor_reads(r):
    read_range(r, ACC, 75, 90)
    read_range(r, ACC, 2790, len(lines_of(ACC)))


def a6r1_reads(r):
    read_run_range(r, "a6-r1.md", 1, 300)


def base_calls(r, direct=True):
    r.header()
    step0(r, direct)
    a6_reads(r)
    lc_reads(r)
    acceptor_reads(r)
    a6r1_reads(r)


def stop_calls(r):
    r.header()
    step0(r)


def schema_keys(prop):
    schema = json.load(open(SCHEMA, encoding="utf-8"))
    return [k for k in schema["properties"][prop]["required"]]


def schema_enum(prop, key):
    schema = json.load(open(SCHEMA, encoding="utf-8"))
    return list(schema["properties"][prop]["items"]["properties"][key]["enum"])


def m_row(m):
    return find_line(LC, "| **%s** |" % m)[0]


def a6_m_row(m):
    return find_line(A6, "| %s |" % m)[0]


def sec_ref(prefix):
    return ref(A6, *closure_range(A6, prefix))


def a6_ref(label):
    return ref(A6, *closure_range(A6, label))


def topic_refs(t):
    return [a6_ref(TOPIC_A6_ROWS[t])]


def base_result():
    ids = PR.IDS
    sch = json.load(open(SCHEMA, encoding="utf-8"))["properties"]
    acc_ref = ref(ACC, 79, 83)
    return {
        "Schema": "rackcad-a6r2-architect-review/0", "RunId": ids["RunId"], "InvocationId": ids["InvocationId"],
        "LogicalReviewRequestId": ids["LogicalReviewRequestId"], "AttemptSeq": ids["AttemptSeq"], "RequestedRole": "ARCHITECT",
        "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62", "ReviewedCommit": PR.REV, "ReviewedPath": PR.OBJECT, "ReviewedBlob": PR.OBJECT_BLOB,
        "ReviewerMode": "SEPARATE SESSION", "SamePersonAsAuthor": "selftest",
        "ReviewerDeclaredIdentity": {"Runtime": "x", "Model": "x", "Effort": "x", "SessionOrThread": "x"},
        "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["x"]},
        "Step0": dict([("Basis", "INVOKER_VERIFIED_CLONE")] + [(k, True) for k in schema_keys("Step0") if k not in ("Basis", "Note")] + [("Note", "x")]),
        "DirectReads": [{"Ref": ref(V14, 2333, 2334), "Purpose": "x"}],
        "Reads": ["x"], "Verdict": "AGREED", "RequiredFindings": [], "OptionalFindings": [],
        "TopicVerdicts": [{"Topic": t, "Disposition": "SATISFIED", "Rationale": "x", "ClauseRefs": topic_refs(t), "FindingIds": []} for t in PR.TOPICS],
        "Round1FindingResolution": [{"Round1Finding": f, "Status": "CORRECTED", "Rationale": "x",
                                     "ClauseRefs": [a6_ref("§3.16, %s" % f[-2:]), ref(R1OUT, *closure_range(R1OUT, "hallazgo " + f))], "FindingIds": []}
                                    for f in PR.R1_IDS],
        "Round1OptionalDisposition": [{"Round1Optional": f, "Status": "INCORPORATED", "Rationale": "x", "ClauseRefs": [a6_ref("§3.16, %s" % f[-2:])],
                                       "FindingIds": []} for f in PR.O1_IDS],
        "DeltaVerdicts": [{"Delta": d, "Verdict": "AGREED", "Rationale": "x", "ClauseRefs": [sec_ref(DELTA_SECTIONS[d])], "FindingIds": []}
                          for d in PR.DELTAS],
        "GapAssessment": [{"Gap": g, "Assessment": "ACCEPTABLE_RESIDUAL", "MitigationTested": "partial", "Rationale": "x",
                           "ClauseRefs": [a6_ref("§3.12, %s" % g)], "FindingIds": []} for g in PR.GAP_IDS],
        "M01Verification": {"A6Declared": PR.MAT_A6["M-01"], "Activated": PR.MAT_A6["M-01"], "Agreement": "AGREE", "Rationale": "x",
                            "ClauseRefs": [ref(A6, a6_m_row("M-01")), ref(LC, m_row("M-01"))], "FindingIds": []},
        "MaterialityConfirmation": [{"Id": m, "A6Declared": PR.MAT_A6[m], "ReviewerAssessment": PR.MAT_A6[m], "Agreement": "AGREE", "Rationale": "x",
                                     "ClauseRefs": [ref(A6, a6_m_row(m)), ref(LC, m_row(m))], "FindingIds": []} for m in PR.MAT_IDS],
        "AcceptorCodeContrast": [{"Claim": c, "A6Refs": [a6_ref("A6-5, regla 2")], "AcceptorRefs": [acc_ref], "Status": "MATCHES", "Rationale": "x",
                                  "FindingIds": []} for c in PR.CLAIMS],
        "ProvenanceIntegrityAtUse": [{"Item": p, "Status": "ADEQUATE", "Rationale": "x", "ClauseRefs": [a6_ref("A6-3, regla 3")], "FindingIds": []}
                                     for p in PR.PROV_ITEMS],
        "Separability": {"WithinA6NoDeltaSeparable": "CONFIRMED", "SeparableFromA1toA5": "CONFIRMED", "OwnerLinesSeparable": "CONFIRMED",
                         "Rationale": "x", "ClauseRefs": [sec_ref("## 9. ")], "FindingIds": []},
        "CompatibilityA1toA5": {k: {"Assessment": "COMPATIBLE", "Note": "x"} for k in sch["CompatibilityA1toA5"]["required"]},
        "NoChangeConfirmation": {k: {"Assessment": "NO_CHANGE", "Note": "x"} for k in sch["NoChangeConfirmation"]["required"]},
        "ScopeConfirmation": {k: {"Assessment": "CONFIRMED", "Note": "x"} for k in sch["ScopeConfirmation"]["required"]},
        "D5AndOwnerValidation": {"Disposition": "EXCLUSION_CORRECT", "OwnerValidationAffected": "NO", "Rationale": "x",
                                 "ClauseRefs": [sec_ref("### 3.15 "), "order.txt:58"], "FindingIds": []},
        "OwnerDecisionsScope": {"LineA": {"Assessment": "LIMITED_TO_STATED_SCOPE", "Note": "x"},
                                "LineB": {"Assessment": "LIMITED_TO_STATED_SCOPE", "Note": "x"}, "SeparateAndNotImplied": "CONFIRMED",
                                "NoResultAcceptedInAdvance": "CONFIRMED", "Rationale": "x",
                                "ClauseRefs": [a6_ref("§7, línea del Owner (a)"), a6_ref("§7, línea del Owner (b)")]},
        "OwnerAuthorityCheck": {"AdditionalOwnerReservedConsequences": [], "OwnerDecisionRequired": False, "Note": "x"},
        "IfAgreed": {k: True for k in IFAGREED_KEYS},
        "KnownLimitations": [], "RecommendedNextAction": "x"}


def topic(res):
    return {d["Topic"]: d for d in res["TopicVerdicts"]}


def delta(res):
    return {d["Delta"]: d for d in res["DeltaVerdicts"]}


def mat(res):
    return {d["Id"]: d for d in res["MaterialityConfirmation"]}


def r1(res):
    return {d["Round1Finding"]: d for d in res["Round1FindingResolution"]}


def o1(res):
    return {d["Round1Optional"]: d for d in res["Round1OptionalDisposition"]}


def gap(res):
    return {d["Gap"]: d for d in res["GapAssessment"]}


def claim(res):
    return {d["Claim"]: d for d in res["AcceptorCodeContrast"]}


def prov(res):
    return {d["Item"]: d for d in res["ProvenanceIntegrityAtUse"]}


def req_finding(fid="A62-A6R2-R1", evidence=None):
    return {"FindingId": fid, "Section": "§3.6 A6-3, regla 3", "ViolatedInvariant": "x", "Counterexample": "",
            "Evidence": evidence or [sec_ref("### 3.6 A6-3 ")], "WhyItMatters": "x", "CorrectionRequired": "x"}


def opt_finding(fid="A62-A6R2-O1"):
    return {"FindingId": fid, "Section": "§9", "Note": "x", "Evidence": [sec_ref("## 9. ")], "SuggestedCorrection": "x"}


def not_agreed(res, verdict):
    res["Verdict"] = verdict
    res["IfAgreed"] = {k: None for k in IFAGREED_KEYS}


def changes_required(res):
    """A coherent CHANGES REQUIRED skeleton: one REQUIRED on A6-3 that leaves Q-A6-02 NOT_SATISFIED."""
    not_agreed(res, "CHANGES REQUIRED")
    res["RequiredFindings"] = [req_finding()]
    delta(res)["A6-3"].update({"Verdict": "CHANGES REQUIRED", "FindingIds": ["A62-A6R2-R1"]})
    topic(res)["Q-A6-02"].update({"Disposition": "NOT_SATISFIED", "FindingIds": ["A62-A6R2-R1"]})


def blocked(res):
    """A coherent BLOCKED — OWNER DECISION skeleton."""
    not_agreed(res, BLOCKED)
    res["OwnerAuthorityCheck"].update({"OwnerDecisionRequired": True, "AdditionalOwnerReservedConsequences": ["x"]})


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
           "SchemaValid": out["Result"]["Check"]["SchemaValid"], "Accreditation": out["Accreditation"]["Status"]}
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
        r.call("Grep", {"pattern": "L2333", "path": RUN + "\\order.txt", "output_mode": "content", "-n": True}, "44:" + order[43])
    add(variant("06-glob-y-grep-aceptados", accepted))

    def denied(r):
        r.call("Read", {"file_path": r"D:\r62-fixture\A2\README.md"}, "Permission to read D:\\r62-fixture\\A2\\README.md has been denied.", is_error=True)
        r.res_patch["permission_denials"] = [{"tool_name": "Read", "tool_use_id": "toolu_010", "tool_input": {"file_path": r"D:\r62-fixture\A2\README.md"}}]
    add(variant("07-herramienta-denegada", denied, ["lectura fuera del cierre (intento fallido", "permiso denegado", "permission_denials: Read"]))

    def bash(r):
        r.call("Bash", {"command": "git -C D:/r62-arch-a6r2 rev-parse HEAD"}, "<tool_use_error>Error: No such tool available: Bash</tool_use_error>",
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
                                                     "llamada sin resultado", "lectura directa: DirectReads sin una entrada"], final=False, schema_valid=False))

    a48, b48 = closure_range(ADR48, "Decisión, punto 6")

    def ref_unread(res, r):
        res["Separability"]["ClauseRefs"].append(ref(ADR48, a48, b48))
    add(variant("14-referencia-sobre-lineas-no-leidas", expected=["referencia sobre líneas no entregadas fielmente en Separability (%s)" %
                                                                  ref(ADR48, a48, b48)], result_patch=ref_unread))

    def ref_bad(res, r):
        topic(res)["Q-A6-01"]["ClauseRefs"] += ["prompt.md:1", "order.txt:99"]
    add(variant("15-referencia-a-prompt-o-fuera-de-rango", expected=["referencia no canónica en TopicVerdicts[Q-A6-01] (prompt.md:1)",
                                                                     "referencia no canónica en TopicVerdicts[Q-A6-01] (order.txt:99)"],
                result_patch=ref_bad))

    def stop(res, r):
        nd = {"Assessment": "NOT_DETERMINED", "Note": "no evaluado"}
        res.update({"Verdict": "NOT_ACCREDITED", "Reads": [RUN + "\\order.txt"], "RequiredFindings": [], "OptionalFindings": [], "TopicVerdicts": [],
                    "Round1FindingResolution": [], "Round1OptionalDisposition": [], "DeltaVerdicts": [], "GapAssessment": [],
                    "MaterialityConfirmation": [], "AcceptorCodeContrast": [], "ProvenanceIntegrityAtUse": [],
                    "M01Verification": {"A6Declared": "NOT_DETERMINED", "Activated": "UNKNOWN", "Agreement": "NOT_DETERMINED", "Rationale": "no evaluado",
                                        "ClauseRefs": [], "FindingIds": []},
                    "ReviewedCommit": None, "ReviewedBlob": None,
                    "KnownLimitations": ["Paso 0: el resultado `run` de las guardas no nombra la publicación como Head; revisión detenida"],
                    "Separability": {"WithinA6NoDeltaSeparable": "NOT_DETERMINED", "SeparableFromA1toA5": "NOT_DETERMINED",
                                     "OwnerLinesSeparable": "NOT_DETERMINED", "Rationale": "no evaluado", "ClauseRefs": [], "FindingIds": []},
                    "CompatibilityA1toA5": {k: dict(nd) for k in res["CompatibilityA1toA5"]},
                    "NoChangeConfirmation": {k: dict(nd) for k in res["NoChangeConfirmation"]},
                    "ScopeConfirmation": {k: dict(nd) for k in res["ScopeConfirmation"]},
                    "D5AndOwnerValidation": {"Disposition": "NOT_DETERMINED", "OwnerValidationAffected": "NOT_DETERMINED", "Rationale": "no evaluado",
                                             "ClauseRefs": [], "FindingIds": []},
                    "OwnerDecisionsScope": {"LineA": dict(nd), "LineB": dict(nd), "SeparateAndNotImplied": "NOT_DETERMINED",
                                            "NoResultAcceptedInAdvance": "NOT_DETERMINED", "Rationale": "no evaluado", "ClauseRefs": []},
                    "OwnerAuthorityCheck": {"AdditionalOwnerReservedConsequences": [], "OwnerDecisionRequired": None, "Note": "no evaluado"},
                    "IfAgreed": {k: None for k in IFAGREED_KEYS}})
        res["Step0"].update({"GuardsRunModeBaseHeadResult": False, "SelftestA6TextBlobMatchesObject": None})
    add(variant("16-parada-not-accredited-coherente", result_patch=stop, base=stop_calls, require_coherent=True))

    def agreed_with_required(res, r):
        res["RequiredFindings"] = [req_finding()]
        topic(res)["Q-A6-02"].update({"Disposition": "NOT_SATISFIED", "FindingIds": ["A62-A6R2-R1"]})
    add(variant("17-agreed-con-un-required", result_patch=agreed_with_required, expected=["AGREED exige cero REQUIRED"]))

    def cr_without_required(res, r):
        not_agreed(res, "CHANGES REQUIRED")
    add(variant("18-changes-required-sin-required", result_patch=cr_without_required, expected=["CHANGES REQUIRED sin ningún REQUIRED"]))

    def agreed_topic_not_satisfied(res, r):
        res["OptionalFindings"] = [opt_finding()]
        topic(res)["Q-A6-01"].update({"Disposition": "NOT_SATISFIED", "FindingIds": ["A62-A6R2-O1"]})
    add(variant("19-agreed-con-un-tema-no-satisfecho", result_patch=agreed_topic_not_satisfied, expected=["AGREED exige cero REQUIRED"]))

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
        topic(res)["Q-A6-12"]["ClauseRefs"] += ["order.txt:41-51", "order.txt:70", "a6-r1.md:1-3"]
        res["Separability"]["ClauseRefs"].append("order.txt:1")
    add(variant("25-referencias-a-order-y-a6r1-aceptadas", result_patch=ref_run, require_coherent=True))

    def failed_degraded(r):
        r.call("Read", {"file_path": absp(ADR48), "offset": a48, "limit": b48 - a48 + 1}, "File does not exist.", is_error=True)
        v = lines_of(V14)[2529:2532]
        r.call("Read", {"file_path": absp(V14), "offset": 2530, "limit": 3}, numbered([v[0], v[1] + " X", v[2]], 2530))

    def ref_failed(res, r):
        res["Separability"]["ClauseRefs"].append(ref(ADR48, a48))
    rv = variant("26-lectura-fallida-y-degradada", failed_degraded, ["referencia sobre líneas no entregadas fielmente en Separability (%s)" % ref(ADR48, a48),
                                                                     "fidelidad: docs/initiatives/I-62-proposal-v14.md"], result_patch=ref_failed)
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
    add(variant("28-run-json-ausente", run_hook=hook(drop_launch={"run.json"}), expected=["lanzamiento: falta run.json"]))
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
                          "lectura directa: docs/initiatives/I-62-proposal-v14.md:2333-2334 no entregadas"],
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
        PR.clone_git = lambda *a: ("0" * 40 + "\n") if a == ("rev-parse", "HEAD:" + A6) else orig_git(*a)
        add(variant("54-clon-blob-cambiado", expected=["el clon no conserva los blobs designados del cierre"]))
    finally:
        PR.clone_git = orig_git

    # ---- result schema and coherence
    def extra_field(res, r):
        res["Extra"] = 1
    add(variant("55-esquema-invalido", result_patch=extra_field, expected=["resultado: no valida contra result.schema.json"], schema_valid=False))

    def p(name, fn, expected=(), counts=None, schema_valid=True, coherent=False, run_hook=None, base=None, extra=None):
        add(variant(name, extra=extra, result_patch=lambda res, r: fn(res), expected=expected, counts=counts, schema_valid=schema_valid,
                    require_coherent=coherent, run_hook=run_hook, base=base))

    def cr(extra):
        def f(res):
            changes_required(res)
            extra(res)
        return f

    def bl(extra):
        def f(res):
            blocked(res)
            extra(res)
        return f

    def t_req_only_optional(res):
        res["OptionalFindings"] = [opt_finding()]
        topic(res)["Q-A6-12"].update({"Disposition": "NOT_SATISFIED", "FindingIds": ["A62-A6R2-O1"]})
    p("56-tema-required-no-satisfecho-sin-required", cr(t_req_only_optional),
      ["tema Q-A6-12 (REQUIRED REVIEW TOPIC) NOT_SATISFIED sin un REQUIRED vinculado"])

    def t01_no_finding(res):
        topic(res)["Q-A6-01"]["Disposition"] = "NOT_SATISFIED"
    p("57-tema-q01-no-satisfecho-sin-hallazgo", cr(t01_no_finding), ["tema Q-A6-01 NOT_SATISFIED sin un hallazgo vinculado"])

    def t02_no_finding(res):
        topic(res)["Q-A6-06/07"]["Disposition"] = "NOT_SATISFIED"
    p("58-tema-required-no-satisfecho-sin-hallazgos", cr(t02_no_finding), ["tema Q-A6-06/07 NOT_SATISFIED sin un hallazgo vinculado",
                                                                          "tema Q-A6-06/07 (REQUIRED REVIEW TOPIC) NOT_SATISFIED sin un REQUIRED"])

    def t_satisfied_req(res):
        topic(res)["Q-A6-06/07"]["FindingIds"] = ["A62-A6R2-R1"]
    p("59-tema-satisfecho-con-required", cr(t_satisfied_req), ["tema Q-A6-06/07 SATISFIED con un REQUIRED vinculado"])

    def t_needs_owner(res):
        topic(res)["Q-A6-12"]["Disposition"] = "NEEDS_OWNER"
    p("60-tema-needs-owner-sin-decision", cr(t_needs_owner), ["tema Q-A6-12 NEEDS_OWNER sin OwnerDecisionRequired = true"])

    def t_missing(res):
        res["TopicVerdicts"] = [t for t in res["TopicVerdicts"] if t["Topic"] != "Q-A6-12"]
    p("61-tema-ausente", t_missing, ["TopicVerdicts debe disponer los cuatro temas", "AGREED exige cero REQUIRED"])

    def t_dup(res):
        res["TopicVerdicts"].append(copy.deepcopy(topic(res)["Q-A6-02"]))
    p("62-tema-repetido", t_dup, ["TopicVerdicts debe disponer los cuatro temas", "AGREED exige cero REQUIRED"])

    def d_missing(res):
        res["DeltaVerdicts"] = [d for d in res["DeltaVerdicts"] if d["Delta"] != "A6-9"]
    p("63-delta-ausente", d_missing, ["DeltaVerdicts debe disponer A6-1..A6-9", "AGREED exige cero REQUIRED"])

    def d_dup(res):
        res["DeltaVerdicts"][8] = copy.deepcopy(res["DeltaVerdicts"][4])
    p("64-delta-repetido", d_dup, ["DeltaVerdicts debe disponer A6-1..A6-9"])

    def d_cr_no_req(res):
        delta(res)["A6-5"]["Verdict"] = "CHANGES REQUIRED"
    p("65-delta-changes-required-sin-required", cr(d_cr_no_req), ["delta A6-5 CHANGES REQUIRED sin un REQUIRED vinculado"])

    def d_agreed_req(res):
        delta(res)["A6-1"]["FindingIds"] = ["A62-A6R2-R1"]
    p("66-delta-agreed-con-required", cr(d_agreed_req), ["delta A6-1 AGREED con un REQUIRED vinculado"])

    def req_uncited(res):
        res["RequiredFindings"].append(req_finding("A62-A6R2-R2"))
    p("67-required-sin-disposicion", cr(req_uncited), ["REQUIRED sin disposición: todo REQUIRED lo cita al menos un delta o un tema (A62-A6R2-R2)"])

    def fid_missing(res):
        delta(res)["A6-7"]["FindingIds"] = ["A62-A6R2-O9"]
    p("68-findingids-inexistente", fid_missing, ["DeltaVerdicts[A6-7] cita en FindingIds hallazgos que no existen"])

    def fid_dup(res):
        res["OptionalFindings"] = [opt_finding(), opt_finding()]
    p("69-ids-de-hallazgo-repetidos", fid_dup, ["identificadores de hallazgo repetidos"])

    def req_no_invariant(res):
        res["RequiredFindings"][0].update({"ViolatedInvariant": " ", "Counterexample": ""})
    p("70-required-sin-invariante-ni-contraejemplo", cr(req_no_invariant), ["A62-A6R2-R1 REQUIRED sin ViolatedInvariant ni Counterexample"])

    def req_no_evidence(res):
        res["RequiredFindings"][0]["Evidence"] = []
    p("71-required-sin-evidencia", cr(req_no_evidence), ["resultado: no valida contra result.schema.json"], schema_valid=False)

    def ev_outside(res):
        res["RequiredFindings"][0]["Evidence"] = ["docs/HANDOFF.md:1"]
    p("72-evidencia-fuera-del-cierre", cr(ev_outside), ["referencia no canónica en RequiredFindings[A62-A6R2-R1] (%s:1)" % PR.userless(PR.norm(absp("docs/HANDOFF.md")))])

    def ev_unread(res):
        res["RequiredFindings"][0]["Evidence"] = [ref(A6, 10, 12)]
    p("73-evidencia-sobre-lineas-no-leidas", cr(ev_unread), ["referencia sobre líneas no entregadas fielmente en RequiredFindings[A62-A6R2-R1] (%s)"
                                                              % ref(A6, 10, 12)])

    def opt_unread(res):
        res["OptionalFindings"] = [dict(opt_finding(), Evidence=[ref(V14, 2408)])]
    p("74-evidencia-opcional-no-leida", opt_unread, ["referencia sobre líneas no entregadas fielmente en OptionalFindings[A62-A6R2-O1]"])

    def m_missing(res):
        res["MaterialityConfirmation"] = res["MaterialityConfirmation"][:7]
    p("75-materialidad-fila-ausente", m_missing, ["MaterialityConfirmation debe disponer M-01..M-08"])

    def m_declared(res):
        mat(res)["M-01"].update({"A6Declared": "yes", "ReviewerAssessment": "yes"})
        res["M01Verification"]["Activated"] = "yes"
        res["M01Verification"].update({"Agreement": "DISAGREE", "FindingIds": ["A62-A6R2-O1"]})
        res["OptionalFindings"] = [opt_finding()]
    p("76-materialidad-a6declared-distinto", m_declared, ["MaterialityConfirmation[M-01]: A6Declared distinto del valor de A-6 §5 (no)"])

    def m_agreement(res):
        mat(res)["M-03"]["ReviewerAssessment"] = "no"
    p("77-materialidad-acuerdo-incoherente", m_agreement, ["MaterialityConfirmation[M-03]: Agreement incoherente"])

    def m_disagree(res):
        mat(res)["M-06"].update({"ReviewerAssessment": "yes", "Agreement": "DISAGREE"})
    p("78-materialidad-desacuerdo-sin-hallazgo", m_disagree, ["MaterialityConfirmation[M-06] DISAGREE sin un hallazgo vinculado"])

    def m_refs(res):
        mat(res)["M-04"]["ClauseRefs"] = []
    p("79-materialidad-sin-referencias", m_refs, ["con un veredicto, MaterialityConfirmation[M-04] sin ClauseRefs"])

    def sep_nc(res):
        res["Separability"]["WithinA6NoDeltaSeparable"] = "NOT_CONFIRMED"
    p("80-separabilidad-no-confirmada-sin-hallazgo", sep_nc, ["Separability con una afirmación NOT_CONFIRMED sin un hallazgo vinculado"])

    def sep_fid(res):
        res["Separability"]["FindingIds"] = ["A62-A6R2-R7"]
    p("81-separabilidad-hallazgo-inexistente", sep_fid, ["Separability cita en FindingIds hallazgos que no existen"])

    def d5_cover(res):
        res["D5AndOwnerValidation"]["Disposition"] = "COVER_IN_A6"
    p("82-d5-cubrir-sin-required", cr(d5_cover), ["D5AndOwnerValidation COVER_IN_A6 sin un REQUIRED vinculado"])

    def d5_owner(res):
        res["D5AndOwnerValidation"]["Disposition"] = "OWNER_DECISION_REQUIRED"
    p("83-d5-decision-del-owner-sin-owner", cr(d5_owner), ["D5AndOwnerValidation OWNER_DECISION_REQUIRED sin OwnerDecisionRequired = true"])

    def owner_no_items(res):
        res["OwnerAuthorityCheck"]["AdditionalOwnerReservedConsequences"] = []
    p("84-decision-del-owner-sin-consecuencias", bl(owner_no_items), ["OwnerDecisionRequired = true sin AdditionalOwnerReservedConsequences"])

    def blocked_no_owner(res):
        not_agreed(res, BLOCKED)
    p("85-blocked-sin-decision-del-owner", blocked_no_owner, ["BLOCKED — OWNER DECISION sin OwnerDecisionRequired = true"])

    def ifagreed_not_null(res):
        res["IfAgreed"]["SuitableForCoordinatorAgreement"] = True
    p("86-ifagreed-no-nulo-sin-agreed", cr(ifagreed_not_null), ["IfAgreed debe ir en null salvo con AGREED"])

    def ifagreed_false(res):
        res["IfAgreed"]["Round1FindingsCorrected"] = False
    p("87-agreed-con-ifagreed-falso", ifagreed_false, ["AGREED exige cero REQUIRED"])

    def agreed_change(res):
        res["NoChangeConfirmation"]["Recipe164"]["Assessment"] = "CHANGE"
    p("88-agreed-con-un-cambio-protegido", agreed_change, ["AGREED exige cero REQUIRED"])

    def agreed_owner_scope(res):
        res["OwnerDecisionsScope"]["LineB"]["Assessment"] = "EXCEEDS_STATED_SCOPE"
    p("89-agreed-con-una-linea-del-owner-excedida", agreed_owner_scope, ["AGREED exige cero REQUIRED"])

    def agreed_d5(res):
        res["OptionalFindings"] = [opt_finding()]
        res["D5AndOwnerValidation"].update({"Disposition": "OWNER_DECISION_REQUIRED", "FindingIds": ["A62-A6R2-O1"]})
        res["OwnerAuthorityCheck"].update({"OwnerDecisionRequired": True, "AdditionalOwnerReservedConsequences": ["x"]})
    p("90-agreed-con-decision-del-owner", agreed_d5, ["AGREED exige cero REQUIRED"])

    def nd_with_verdict(res):
        res["ScopeConfirmation"]["NoFrozenTextReplaced"]["Assessment"] = "NOT_DETERMINED"
        res["OwnerDecisionsScope"]["SeparateAndNotImplied"] = "NOT_DETERMINED"
    p("91-no-determinado-con-veredicto", cr(nd_with_verdict), ["con un veredicto, NOT_DETERMINED no se admite (NoFrozenTextReplaced, SeparateAndNotImplied)"])

    def owner_null(res):
        res["OwnerAuthorityCheck"]["OwnerDecisionRequired"] = None
    p("92-decision-del-owner-nula-con-veredicto", cr(owner_null), ["OwnerAuthorityCheck.OwnerDecisionRequired no admite null"])

    def step0_false(res):
        res["Step0"]["Decisions80DeclaresOrderBytesAndSha256"] = False
    p("93-paso0-falso-con-veredicto", step0_false, ["las siete booleanas de Step0 van en true"])

    def step0_null(res):
        res["Step0"]["V14DirectReadL2333L2334"] = None
    p("94-paso0-nulo-con-veredicto", step0_null, ["las siete booleanas de Step0 van en true"])

    def obj_blob(res):
        res["ReviewedBlob"] = "eac473c0d2e58cf1c4594f48a8816b3833af6509"
    p("95-objeto-blob-de-la-ronda-1", obj_blob, ["objeto revisado distinto del designado"])

    def obj_commit(res):
        res["ReviewedCommit"] = "7b5ffac9d9c04f90b9708a867089d32e885dc815"
    p("96-objeto-commit-de-la-ronda-1", obj_commit, ["objeto revisado distinto del designado"])

    def other_ids(res):
        res["RunId"] = "R20261011T215735Z-c434"
    p("97-ids-de-invocacion-de-la-ronda-1", other_ids, ["identificadores de la invocación distintos"])

    def delta_no_refs(res):
        delta(res)["A6-4"]["ClauseRefs"] = []
    p("98-delta-sin-referencias", delta_no_refs, ["con un veredicto, delta A6-4 sin ClauseRefs"])

    def scope_no_refs(res):
        res["OwnerDecisionsScope"]["ClauseRefs"] = []
        res["Separability"]["ClauseRefs"] = []
        res["D5AndOwnerValidation"]["ClauseRefs"] = []
        topic(res)["Q-A6-02"]["ClauseRefs"] = []
    p("99-secciones-sin-referencias", scope_no_refs, ["con un veredicto, OwnerDecisionsScope sin ClauseRefs", "con un veredicto, Separability sin ClauseRefs",
                                                      "con un veredicto, D5AndOwnerValidation sin ClauseRefs", "con un veredicto, tema Q-A6-02 sin ClauseRefs"])

    def cr_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        mat(res)["M-01"].update({"ReviewerAssessment": "UNKNOWN", "Agreement": "DISAGREE", "FindingIds": ["A62-A6R2-O1"]})
        res["M01Verification"].update({"Activated": "UNKNOWN", "Agreement": "DISAGREE", "FindingIds": ["A62-A6R2-O1"]})
        res["Separability"].update({"OwnerLinesSeparable": "NOT_CONFIRMED", "FindingIds": ["A62-A6R2-O1"]})
        res["D5AndOwnerValidation"].update({"Disposition": "COVER_IN_A6", "FindingIds": ["A62-A6R2-R1"]})
        res["ScopeConfirmation"]["D6AppendAfterV14L2587"]["Assessment"] = "NOT_CONFIRMED"
    p("100-changes-required-coherente-aceptado", cr(cr_ok), coherent=True)

    def bl_ok(res):
        topic(res)["Q-A6-12"]["Disposition"] = "NEEDS_OWNER"
        res["D5AndOwnerValidation"].update({"Disposition": "OWNER_DECISION_REQUIRED", "OwnerValidationAffected": "YES"})
        res["OwnerDecisionsScope"]["LineA"]["Assessment"] = "EXCEEDS_STATED_SCOPE"
    p("101-blocked-coherente-aceptado", bl(bl_ok), coherent=True)

    def ag_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        mat(res)["M-06"].update({"ReviewerAssessment": "UNKNOWN", "Agreement": "DISAGREE", "FindingIds": ["A62-A6R2-O1"]})
        topic(res)["Q-A6-01"]["FindingIds"] = ["A62-A6R2-O1"]
        delta(res)["A6-2"]["FindingIds"] = ["A62-A6R2-O1"]
    p("102-agreed-con-un-opcional-aceptado", ag_ok, coherent=True)

    def bad_fid(res):
        res["OptionalFindings"] = [dict(opt_finding(), FindingId="A62-A6R2-1")]
    p("103-id-de-hallazgo-mal-formado", bad_fid, ["resultado: no valida contra result.schema.json"], schema_valid=False)

    def round1_fid(res):
        res["RequiredFindings"][0]["FindingId"] = "A62-A6-R1"
        delta(res)["A6-3"]["FindingIds"] = ["A62-A6-R1"]
        topic(res)["Q-A6-02"]["FindingIds"] = ["A62-A6-R1"]
    p("103b-id-nuevo-con-el-espacio-de-la-ronda-1", cr(round1_fid), ["resultado: no valida contra result.schema.json"], schema_valid=False)

    def ref_absolute(res):
        delta(res)["A6-6"]["ClauseRefs"] = [absp(A6) + ":" + str(closure_range(A6, DELTA_SECTIONS["A6-6"])[0])]
    p("104-referencia-absoluta", ref_absolute, ["resultado: no valida contra result.schema.json"], schema_valid=False)

    def ref_malformed(res):
        res["D5AndOwnerValidation"]["ClauseRefs"] = ["order.txt"]
    p("105-referencia-mal-formada", ref_malformed, ["referencia mal formada en D5AndOwnerValidation", "resultado: no valida contra result.schema.json"],
      schema_valid=False)

    # ---- preflight record of gate 11 (launch preflight 11)
    g11 = "preflight: compuerta 11 (%s)" % OG.CHECK

    def pf_no_owner(pf):
        del pf["Checks"][OG.CHECK]
    add(variant("106-preflight-sin-compuerta-11", run_hook=hook(pf_hook=pf_no_owner),
                expected=["preflight: faltan comprobaciones ['%s']" % OG.CHECK, g11 + " sin las líneas fijadas", g11 + " sin el blob de la ronda 2"]))

    def pf_owner_other_line(pf):
        pf["Checks"][OG.CHECK]["A6LineSha256"] = "4" * 64
    add(variant("107-preflight-compuerta-11-otra-linea", run_hook=hook(pf_hook=pf_owner_other_line), expected=[g11 + " sin las líneas fijadas"]))

    def pf_owner_failed(pf):
        pf["Checks"][OG.CHECK].update({"Ok": False, "A6LineNumbers": [], "Problems": ["falta la línea"]})
    add(variant("108-preflight-compuerta-11-fallida", run_hook=hook(pf_hook=pf_owner_failed),
                expected=["preflight: comprobaciones sin Ok: ['%s']" % OG.CHECK, g11 + " sin las líneas fijadas"]))

    def pf_no_auth(pf):
        pf["Checks"][OG.CHECK]["AuthorizationSections"] = [{"Number": 81, "Line": 1617, "Heading": "## 81. Disposición del Owner", "HasBlob": False,
                                                             "HasRunId": False}]
    add(variant("108b-preflight-compuerta-11-sin-autorizacion", run_hook=hook(pf_hook=pf_no_auth), expected=[g11 + " sin el blob de la ronda 2"]))

    def pf_head_blob(pf):
        pf["Checks"][OG.CHECK].update({"A6BlobAtHead": "eac473c0d2e58cf1c4594f48a8816b3833af6509", "A6BlobAtHeadMatches": False})
    add(variant("108c-preflight-compuerta-11-blob-en-la-head", run_hook=hook(pf_hook=pf_head_blob), expected=[g11 + " sin el blob de la ronda 2"]))

    def pf_runid(pf):
        pf["Checks"][OG.CHECK]["RunId"] = "R20261011T215735Z-c434"
    add(variant("108d-preflight-compuerta-11-otro-runid", run_hook=hook(pf_hook=pf_runid), expected=[g11 + " sin el blob de la ronda 2"]))

    # ---- round 2: direct read of V14 L2333-L2334 (decisions §80, point 6, a)
    v14d = ref(V14, 2333, 2334)
    p("109-sin-lectura-directa-de-v14", lambda res: None,
      ["lectura directa: %s no entregadas fielmente" % v14d, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % v14d],
      base=lambda r: base_calls(r, direct=False))

    def a6_quote_only(res):
        delta(res)["A6-6"]["ClauseRefs"].append(v14d)
    p("110-cita-de-v14-leida-solo-en-a6-como-en-la-ronda-1", a6_quote_only,
      ["lectura directa: %s no entregadas fielmente" % v14d, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % v14d,
       "referencia sobre líneas no entregadas fielmente en DeltaVerdicts[A6-6] (%s)" % v14d], base=lambda r: base_calls(r, direct=False))

    def no_direct_entry(res):
        res["DirectReads"] = [{"Ref": sec_ref("### 3.9 A6-6 "), "Purpose": "x"}]
    p("111-directreads-sin-v14", no_direct_entry, ["lectura directa: DirectReads sin una entrada que cubra"])

    def partial_direct_entry(res):
        res["DirectReads"] = [{"Ref": ref(V14, 2333), "Purpose": "x"}]
    p("111b-directreads-cubre-solo-2333", partial_direct_entry, ["lectura directa: DirectReads sin una entrada que cubra"])

    p("111c-lectura-directa-con-tramo-que-no-cubre-2334", lambda res: res.update(DirectReads=[{"Ref": ref(V14, 2330, 2333), "Purpose": "x"}]),
      ["lectura directa: %s no entregadas fielmente" % v14d, "lectura directa: DirectReads sin una entrada que cubra"],
      base=lambda r: (base_calls(r, direct=False), read_range(r, V14, 2330, 2333)))

    # ---- round 2: acceptor-contrast references (e)
    def acc_other_file(res):
        claim(res)["CHECK_IDS"]["AcceptorRefs"] = [ref(TOOLF, 1, 3)]
    p("112-acceptorrefs-en-otro-archivo", acc_other_file, ["AcceptorCodeContrast[CHECK_IDS].AcceptorRefs: referencia fuera de %s" % ACC],
      extra=lambda r: read_range(r, TOOLF, 1, 3))

    def a6refs_other_file(res):
        claim(res)["EXIT_CODES"]["A6Refs"] = [ref(V14, 2333, 2334)]
    p("113-a6refs-en-otro-archivo", a6refs_other_file, ["AcceptorCodeContrast[EXIT_CODES].A6Refs: referencia fuera de %s" % A6])

    def acc_unread(res):
        claim(res)["DECISION_FORMULA"]["AcceptorRefs"] = [ref(ACC, 2675, 2699)]
    p("114-acceptorrefs-sobre-lineas-no-leidas", acc_unread,
      ["referencia sobre líneas no entregadas fielmente en AcceptorCodeContrast[DECISION_FORMULA].AcceptorRefs (%s)" % ref(ACC, 2675, 2699)])

    # ---- round 2: coverage of the new arrays (b, c, e, f)
    def drop(key, field, value):
        def f(res):
            res[key] = [x for x in res[key] if x[field] != value]
        return f

    def dup(key, field, value, replace):
        def f(res):
            items_ = res[key]
            src = next(x for x in items_ if x[field] == value)
            res[key] = [copy.deepcopy(src) if x[field] == replace else x for x in items_]
        return f
    p("115-r-de-la-ronda-1-ausente", drop("Round1FindingResolution", "Round1Finding", "A62-A6-R5"),
      ["Round1FindingResolution debe disponer cada id una vez (faltan ['A62-A6-R5']", "AGREED exige A62-A6-R1..R5"])
    p("116-r-de-la-ronda-1-repetido", dup("Round1FindingResolution", "Round1Finding", "A62-A6-R1", "A62-A6-R5"),
      ["Round1FindingResolution con identificadores repetidos", "Round1FindingResolution debe disponer cada id una vez"])
    p("117-o-de-la-ronda-1-ausente", drop("Round1OptionalDisposition", "Round1Optional", "A62-A6-O2"),
      ["Round1OptionalDisposition debe disponer cada id una vez (faltan ['A62-A6-O2']"])
    p("118-o-de-la-ronda-1-repetido", dup("Round1OptionalDisposition", "Round1Optional", "A62-A6-O1", "A62-A6-O3"),
      ["Round1OptionalDisposition con identificadores repetidos", "Round1OptionalDisposition debe disponer cada id una vez"])
    p("119-brecha-ausente", drop("GapAssessment", "Gap", "G6"), ["GapAssessment debe disponer cada id una vez (faltan ['G6']", "AGREED exige G1..G6"])
    p("120-brecha-repetida", dup("GapAssessment", "Gap", "G1", "G2"),
      ["GapAssessment con identificadores repetidos", "GapAssessment debe disponer cada id una vez"])
    p("121-afirmacion-ausente", drop("AcceptorCodeContrast", "Claim", "R10_RELAY_FACTS"),
      ["AcceptorCodeContrast debe disponer cada id una vez (faltan ['R10_RELAY_FACTS']", "AGREED exige todas las afirmaciones"])
    p("122-afirmacion-repetida", dup("AcceptorCodeContrast", "Claim", "CHECK_IDS", "WAIVABLE"),
      ["AcceptorCodeContrast con identificadores repetidos", "AcceptorCodeContrast debe disponer cada id una vez"])
    p("123-pieza-de-procedencia-ausente", drop("ProvenanceIntegrityAtUse", "Item", "POU_VIII_NO_SHADOW_EXECUTABLES"),
      ["ProvenanceIntegrityAtUse debe disponer cada id una vez (faltan ['POU_VIII_NO_SHADOW_EXECUTABLES']",
       "AGREED exige ProvenanceIntegrityAtUse todo en ADEQUATE"])
    p("124-pieza-de-procedencia-repetida", dup("ProvenanceIntegrityAtUse", "Item", "PREPARATION_WRITE", "FAIL_CLOSED"),
      ["ProvenanceIntegrityAtUse con identificadores repetidos", "ProvenanceIntegrityAtUse debe disponer cada id una vez"])

    def stop_with_dup(res):
        stop(res, None)
        res["GapAssessment"] = [{"Gap": "G1", "Assessment": "ACCEPTABLE_RESIDUAL", "MitigationTested": "no", "Rationale": "x", "ClauseRefs": [],
                                 "FindingIds": []}] * 2
    p("124b-not-accredited-con-ids-repetidos", stop_with_dup, ["GapAssessment con identificadores repetidos"], base=stop_calls)

    # ---- round 2: coherence of the new arrays
    def r_partial(res):
        r1(res)["A62-A6-R2"]["Status"] = "PARTIALLY_CORRECTED"
    p("125-r-parcial-sin-required", cr(r_partial), ["A62-A6-R2 PARTIALLY_CORRECTED sin un REQUIRED vinculado"])

    def r_not(res):
        r1(res)["A62-A6-R4"]["Status"] = "NOT_CORRECTED"
    p("126-r-no-corregido-sin-required", cr(r_not), ["A62-A6-R4 NOT_CORRECTED sin un REQUIRED vinculado"])

    def r_corrected_req(res):
        r1(res)["A62-A6-R1"]["FindingIds"] = ["A62-A6R2-R1"]
    p("127-r-corregido-con-required", cr(r_corrected_req), ["A62-A6-R1 CORRECTED con un REQUIRED vinculado"])

    def o_not_addressed(res):
        o1(res)["A62-A6-O2"]["Status"] = "NOT_ADDRESSED"
    p("128-o-no-tratado-sin-hallazgo", o_not_addressed, ["A62-A6-O2 NOT_ADDRESSED sin un hallazgo vinculado"])

    def g_piece(res):
        gap(res)["G2"]["Assessment"] = "REQUIRES_PIECE_CHANGE"
    p("129-brecha-cambio-de-pieza-sin-required", cr(g_piece), ["GapAssessment[G2] REQUIRES_PIECE_CHANGE sin un REQUIRED vinculado"])

    def g_text(res):
        gap(res)["G3"]["Assessment"] = "REQUIRES_TEXT_CHANGE"
    p("130-brecha-cambio-de-texto-sin-required", cr(g_text), ["GapAssessment[G3] REQUIRES_TEXT_CHANGE sin un REQUIRED vinculado"])

    def g_owner(res):
        gap(res)["G4"]["Assessment"] = "REQUIRES_OWNER_DECISION"
    p("131-brecha-decision-del-owner-sin-owner", cr(g_owner), ["GapAssessment[G4] REQUIRES_OWNER_DECISION sin OwnerDecisionRequired = true"])

    def g_residual_req(res):
        gap(res)["G5"]["FindingIds"] = ["A62-A6R2-R1"]
    p("132-brecha-residual-con-required", cr(g_residual_req), ["GapAssessment[G5] ACCEPTABLE_RESIDUAL con un REQUIRED vinculado"])

    def c_differs(res):
        claim(res)["WAIVABLE"]["Status"] = "DIFFERS"
    p("133-afirmacion-difiere-sin-required", cr(c_differs), ["AcceptorCodeContrast[WAIVABLE] DIFFERS sin un REQUIRED vinculado"])

    def c_matches_req(res):
        claim(res)["EXIT_CODES"]["FindingIds"] = ["A62-A6R2-R1"]
    p("134-afirmacion-coincide-con-required", cr(c_matches_req), ["AcceptorCodeContrast[EXIT_CODES] MATCHES con un REQUIRED vinculado"])

    def p_inadequate(res):
        prov(res)["POU_IV_FSCK"]["Status"] = "INADEQUATE"
    p("135-pieza-inadecuada-sin-required", cr(p_inadequate), ["ProvenanceIntegrityAtUse[POU_IV_FSCK] INADEQUATE sin un REQUIRED vinculado"])

    def p_adequate_req(res):
        prov(res)["TIMING_AT_USE"]["FindingIds"] = ["A62-A6R2-R1"]
    p("136-pieza-adecuada-con-required", cr(p_adequate_req), ["ProvenanceIntegrityAtUse[TIMING_AT_USE] ADEQUATE con un REQUIRED vinculado"])

    def m01_disagree(res):
        res["OptionalFindings"] = [opt_finding()]
        mat(res)["M-01"].update({"ReviewerAssessment": "UNKNOWN", "Agreement": "DISAGREE", "FindingIds": ["A62-A6R2-O1"]})
        res["M01Verification"].update({"Activated": "UNKNOWN", "Agreement": "DISAGREE"})
    p("137-m01-desacuerdo-sin-hallazgo", m01_disagree, ["M01Verification DISAGREE sin un hallazgo vinculado"])

    def m01_incoherent(res):
        res["OptionalFindings"] = [opt_finding()]
        res["M01Verification"].update({"Agreement": "DISAGREE", "FindingIds": ["A62-A6R2-O1"]})
    p("138-m01-acuerdo-incoherente", m01_incoherent, ["M01Verification: Agreement incoherente con Activated y A6Declared"])

    def m01_declared(res):
        res["M01Verification"].update({"A6Declared": "yes", "Activated": "yes"})
    p("139-m01-a6declared-distinto-de-a6", m01_declared, ["con un veredicto, M01Verification lleva A6Declared = A-6 §5 (no)",
                                                          "M01Verification.Activated distinto de MaterialityConfirmation[M-01].ReviewerAssessment"])

    def m01_nd(res):
        res["M01Verification"]["Agreement"] = "NOT_DETERMINED"
    p("140-m01-no-determinado-con-veredicto", m01_nd, ["con un veredicto, M01Verification lleva A6Declared = A-6 §5 (no) y Agreement AGREE o DISAGREE"])

    def m01_activated(res):
        res["OptionalFindings"] = [opt_finding()]
        res["M01Verification"].update({"Activated": "UNKNOWN", "Agreement": "DISAGREE", "FindingIds": ["A62-A6R2-O1"]})
    p("141-m01-distinto-de-la-materialidad", m01_activated, ["M01Verification.Activated distinto de MaterialityConfirmation[M-01].ReviewerAssessment"])

    def new_no_refs(res):
        r1(res)["A62-A6-R3"]["ClauseRefs"] = []
        o1(res)["A62-A6-O1"]["ClauseRefs"] = []
        gap(res)["G4"]["ClauseRefs"] = []
        prov(res)["DECLARED_ORDER"]["ClauseRefs"] = []
        res["M01Verification"]["ClauseRefs"] = []
    p("142-elementos-nuevos-sin-referencias", new_no_refs, ["con un veredicto, Round1FindingResolution[A62-A6-R3] sin ClauseRefs",
                                                            "con un veredicto, Round1OptionalDisposition[A62-A6-O1] sin ClauseRefs",
                                                            "con un veredicto, GapAssessment[G4] sin ClauseRefs",
                                                            "con un veredicto, ProvenanceIntegrityAtUse[DECLARED_ORDER] sin ClauseRefs",
                                                            "con un veredicto, M01Verification sin ClauseRefs"])

    def new_fid_missing(res):
        gap(res)["G4"]["FindingIds"] = ["A62-A6R2-O7"]
        claim(res)["R4_INPUTS"]["FindingIds"] = ["A62-A6R2-R9"]
    p("143-elementos-nuevos-con-hallazgos-inexistentes", new_fid_missing, ["GapAssessment[G4] cita en FindingIds hallazgos que no existen",
                                                                          "AcceptorCodeContrast[R4_INPUTS] cita en FindingIds hallazgos que no existen"])

    def agreed_r(res):
        res["RequiredFindings"] = [req_finding()]
        r1(res)["A62-A6-R3"].update({"Status": "PARTIALLY_CORRECTED", "FindingIds": ["A62-A6R2-R1"]})
        delta(res)["A6-9"].update({"Verdict": "CHANGES REQUIRED", "FindingIds": ["A62-A6R2-R1"]})
    p("144-agreed-con-r-no-corregido", agreed_r, ["AGREED exige cero REQUIRED", "AGREED exige A62-A6-R1..R5 de la ronda 1 en CORRECTED"])

    def agreed_g(res):
        res["RequiredFindings"] = [req_finding()]
        gap(res)["G2"].update({"Assessment": "REQUIRES_TEXT_CHANGE", "FindingIds": ["A62-A6R2-R1"]})
        delta(res)["A6-9"].update({"Verdict": "CHANGES REQUIRED", "FindingIds": ["A62-A6R2-R1"]})
    p("145-agreed-con-brecha-requires", agreed_g, ["AGREED exige cero REQUIRED", "AGREED exige G1..G6 en ACCEPTABLE_RESIDUAL"])

    def agreed_c(res):
        res["RequiredFindings"] = [req_finding()]
        claim(res)["R7_DATA_CONDITION"].update({"Status": "DIFFERS", "FindingIds": ["A62-A6R2-R1"]})
        delta(res)["A6-5"].update({"Verdict": "CHANGES REQUIRED", "FindingIds": ["A62-A6R2-R1"]})
    p("146-agreed-con-afirmacion-que-difiere", agreed_c, ["AGREED exige cero REQUIRED", "AGREED exige todas las afirmaciones de AcceptorCodeContrast en MATCHES"])

    def agreed_p(res):
        res["RequiredFindings"] = [req_finding()]
        prov(res)["POU_III_TYPE_AND_BYTES_REHASH"].update({"Status": "INADEQUATE", "FindingIds": ["A62-A6R2-R1"]})
        delta(res)["A6-3"].update({"Verdict": "CHANGES REQUIRED", "FindingIds": ["A62-A6R2-R1"]})
    p("147-agreed-con-pieza-inadecuada", agreed_p, ["AGREED exige cero REQUIRED", "AGREED exige ProvenanceIntegrityAtUse todo en ADEQUATE"])

    def schema_mitigation(res):
        gap(res)["G1"]["MitigationTested"] = "maybe"
    p("148-esquema-mitigacion-invalida", schema_mitigation, ["resultado: no valida contra result.schema.json"], schema_valid=False)

    def schema_r6(res):
        r1(res)["A62-A6-R5"]["Round1Finding"] = "A62-A6-R6"
    p("149-esquema-r-inexistente", schema_r6, ["resultado: no valida contra result.schema.json",
                                               "Round1FindingResolution debe disponer cada id una vez"], schema_valid=False)

    def schema_acc_no_refs(res):
        claim(res)["STATUS_VALUES"]["AcceptorRefs"] = []
    p("150-esquema-afirmacion-sin-referencias-del-aceptante", schema_acc_no_refs, ["resultado: no valida contra result.schema.json"], schema_valid=False)

    def cr_r2_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        r1(res)["A62-A6-R3"].update({"Status": "PARTIALLY_CORRECTED", "FindingIds": ["A62-A6R2-R1"]})
        delta(res)["A6-9"].update({"Verdict": "CHANGES REQUIRED", "FindingIds": ["A62-A6R2-R1"]})
        topic(res)["Q-A6-12"].update({"Disposition": "NOT_SATISFIED", "FindingIds": ["A62-A6R2-R1"]})
        gap(res)["G2"].update({"Assessment": "REQUIRES_PIECE_CHANGE", "MitigationTested": "no", "FindingIds": ["A62-A6R2-R1"]})
        claim(res)["WAIVABLE"].update({"Status": "DIFFERS", "FindingIds": ["A62-A6R2-R1"]})
        prov(res)["POU_IV_FSCK"].update({"Status": "INADEQUATE", "FindingIds": ["A62-A6R2-R1"]})
        o1(res)["A62-A6-O2"].update({"Status": "NOT_ADDRESSED", "FindingIds": ["A62-A6R2-O1"]})
        mat(res)["M-01"].update({"ReviewerAssessment": "UNKNOWN", "Agreement": "DISAGREE", "FindingIds": ["A62-A6R2-O1"]})
        res["M01Verification"].update({"Activated": "UNKNOWN", "Agreement": "DISAGREE", "FindingIds": ["A62-A6R2-O1"]})
    p("151-changes-required-ronda-2-coherente-aceptado", cr(cr_r2_ok), coherent=True)

    def bl_r2_ok(res):
        gap(res)["G4"].update({"Assessment": "REQUIRES_OWNER_DECISION", "MitigationTested": "partial"})
    p("152-blocked-por-brecha-coherente-aceptado", bl(bl_r2_ok), coherent=True)

    def cited_after_direct(res):
        delta(res)["A6-6"]["ClauseRefs"].append(v14d)
        gap(res)["G1"]["ClauseRefs"].append(v14d)
        res["DirectReads"].append({"Ref": sec_ref("### 3.9 A6-6 "), "Purpose": "x"})
    p("153-cita-de-v14-tras-la-lectura-directa-aceptada", cited_after_direct, coherent=True)
    return cases


# ------------------------------------------------------------------ gate 11 cases (owner_gate.evaluate on synthetic decision texts)
def owner_cases():
    cases = []
    dec_lines = list(PR.canon(DEC))
    while dec_lines and dec_lines[-1] == "":
        dec_lines.pop()
    dec_clone = "\n".join(dec_lines) + "\n"
    a6_text = "\n".join(PR.canon(A6))
    blob, run_id = PR.OBJECT_BLOB, PR.IDS["RunId"]
    sha_ok = PR.TRANSPORT["Binary"]["Sha256"]
    a6l = next(l for l in dec_lines if OG.line_sha(l) == OG.A6_LINE_SHA)
    claude_l = next(l for l in dec_lines if OG.line_sha(l) == OG.CLAUDE_LINE_SHA)
    s78 = next(i for i, l in enumerate(dec_lines) if l.startswith("## 78. "))
    auth_body = "Se autoriza la única revisión formal de la ronda 2 de A-6: blob `%s`, kit `%s`." % (blob, run_id)
    auth = lambda body=auth_body, num=82, who="Coordinator": "\n## %d. Disposición del %s: revisión formal de la ronda 2 de A-6 (selftest)\n\n%s\n" % (num, who, body)

    def replaced(old, new):
        out = "\n".join(new if l == old else l for l in dec_lines) + "\n"
        assert out != dec_clone or old == new
        return out

    def run_case(name, dec_head, expected=(), clean=True, descends=True, measured=None, clone_text=None, a6_head=None, pinned=None, ok=None,
                 dec_cl=None):
        ev = OG.evaluate(dec_head, dec_clone if dec_cl is None else dec_cl, clean, descends, sha_ok if measured is None else measured, sha_ok,
                         a6_text if clone_text is None else clone_text, blob if a6_head is None else a6_head, blob if pinned is None else pinned,
                         run_id)
        got = ev["Problems"]
        missing = [e for e in expected if not any(e in g for g in got)]
        unexpected = [g for g in got if not any(e in g for e in expected)]
        want_ok = not expected if ok is None else ok
        cases.append({"Case": name, "Expected": list(expected), "Got": got, "Missing": missing, "Unexpected": unexpected, "Ok": ev["Ok"],
                      "Authorized": ev["Authorized"], "Result": "PASS" if not missing and not unexpected and ev["Ok"] is want_ok else "FAIL"})

    NOAUTH = "falta la autorización"
    run_case("O00-autorizacion-tras-80-aceptada", dec_clone + auth())
    run_case("O01-sin-autorizacion", dec_clone, [NOAUTH])
    run_case("O02-disposicion-del-owner-con-los-tokens", dec_clone + auth(who="Owner"), [NOAUTH])
    run_case("O03-autorizacion-con-numero-80", dec_clone + auth(num=80), [NOAUTH, "decisiones sin un único encabezado §80 (2)"])
    run_case("O04-blob-abreviado", dec_clone + auth(auth_body.replace(blob, blob[:8] + "…")), [NOAUTH])
    run_case("O05-sin-runid", dec_clone + auth(auth_body.replace(run_id, "R00000000T000000Z-0000")), [NOAUTH])
    run_case("O06-blob-de-la-ronda-1", dec_clone + auth(auth_body.replace(blob, OG.A6R1_BLOB)), [NOAUTH])
    run_case("O07-tokens-pegados", dec_clone + auth(auth_body.replace(blob, blob + "0").replace(run_id, "x" + run_id)), [NOAUTH])
    run_case("O08-blob-y-runid-en-secciones-distintas", dec_clone + auth("blob `%s`" % blob) + auth("kit `%s`" % run_id, num=83), [NOAUTH])
    run_case("O09-a6-en-la-head-con-otro-blob", dec_clone + auth(), ["no tiene el blob de la ronda 2 fijado en el kit"], a6_head=OG.A6R1_BLOB)
    run_case("O10-blob-no-fijado", dec_clone + auth(), ["blob de la A-6 de la ronda 2 no fijado en el kit", "no tiene el blob de la ronda 2 fijado",
                                                        NOAUTH], pinned="")
    run_case("O10b-blob-fijado-de-la-ronda-1", dec_clone + auth(auth_body.replace(blob, OG.A6R1_BLOB)),
             ["blob de la A-6 de la ronda 2 no fijado en el kit", "no tiene el blob de la ronda 2 fijado", NOAUTH], pinned=OG.A6R1_BLOB,
             a6_head=OG.A6R1_BLOB)
    para = a6l.replace("excepción limitada", "excepción acotada")
    assert para != a6l
    for name, new, exp in (
            ("O11-sin-la-linea-del-owner", "(línea retirada)", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02"]),
            ("O12-parafrasis", para, ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02", "otra línea que empieza por «A6-ADR0046-9-FX02 =»"]),
            ("O13-fragmento-citado-en-tabla", "| **Owner** | `" + a6l + "` |", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02"]),
            ("O14-cita-con-sangria-de-bloque", "> " + a6l, ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02"]),
            ("O15-espacio-final", a6l + " ", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02", "otra línea que empieza por «A6-ADR0046-9-FX02 =»"]),
            ("O16-retorno-de-carro", a6l + "\r", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02", "otra línea que empieza por «A6-ADR0046-9-FX02 =»"])):
        t = replaced(a6l, new)
        run_case(name, t + auth(), exp, dec_cl=t)
    moved = dec_lines[:s78] + [a6l] + [l for l in dec_lines[s78:] if l != a6l]
    t = "\n".join(moved) + "\n"
    run_case("O17-linea-antes-de-78", t + auth(), ["no está registrada después del encabezado de decisiones §78"], dec_cl=t)
    run_case("O18-revocacion-posterior", dec_clone + auth(auth_body + "\n\n" + "A6-ADR0046-9-FX02 = B (revoca la determinación anterior)"),
             ["otra línea que empieza por «A6-ADR0046-9-FX02 =»"])
    t = replaced(claude_l, "CLAUDE-CLI-I62 = (retirada)")
    run_case("O19-sin-claude-cli-i62", t + auth(), ["falta, como línea completa de decisiones, la línea del Owner CLAUDE-CLI-I62 = A",
                                                    "otra línea que empieza por «CLAUDE-CLI-I62 =»"], dec_cl=t)
    run_case("O20-otra-linea-claude-cli-i62", dec_clone + auth(auth_body + "\n\nCLAUDE-CLI-I62 = B (revocada)"), ["otra línea que empieza por «CLAUDE-CLI-I62 =»"])
    run_case("O21-decision-80-cambiada", dec_clone.replace("suma 2 833 bytes", "suma 2 834 bytes") + auth(), ["no crece solo por añadido"])
    run_case("O22-cambios-sin-commit", dec_clone + auth(), ["cambios sin commit"], clean=False)
    run_case("O23-head-no-desciende-del-clon", dec_clone + auth(), ["no desciende del commit del clon"], descends=False)
    run_case("O24-otro-binario", dec_clone + auth(), ["SHA-256 medido del binario distinto del cierre"], measured="0" * 64)
    run_case("O25-linea-fijada-ausente-de-a6r2", dec_clone + auth(), ["no está, como línea completa, en la A-6 de la ronda 2"],
             clone_text=a6_text.replace(a6l, para))
    t = dec_clone + "\n## 78. Duplicado (selftest)\n"
    run_case("O26-dos-encabezados-78", t + auth(), ["decisiones sin un único encabezado §78 (2)"], dec_cl=t)
    run_case("O27-autorizacion-anterior-al-clon-en-el-cuerpo-de-80", dec_clone.replace("I-64 conserva la prioridad |",
                                                                                        "I-64 conserva la prioridad; blob %s; %s |" % (blob, run_id)),
             [NOAUTH, "no crece solo por añadido"])
    return cases


def live_owner_gate_case():
    """L00: owner_gate.check on the REAL I-62 worktree (read-only git). Today (no later Coordinator disposition) it must block, and only on the
    authorization and, before the publication of round 2, on the A-6 blob at the HEAD."""
    def wt(*a):
        return subprocess.run(["git", "-C", WORKTREE] + list(a), capture_output=True, text=True, env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))
    if not os.path.isdir(WORKTREE):
        return {"Case": "L00-compuerta-11-real-bloquea", "Result": "FAIL", "Got": "worktree no disponible"}
    head = wt("rev-parse", "HEAD").stdout.strip()
    commit = PR.REV if wt("cat-file", "-e", PR.REV + "^{commit}").returncode == 0 else head
    closure_like = {"ObjectBlob": PR.OBJECT_BLOB, "Ids": {"RunId": PR.IDS["RunId"]}, "Transport": {"Binary": {"Sha256": PR.TRANSPORT["Binary"]["Sha256"]}}}
    ev = OG.check(WORKTREE, WORKTREE, commit, closure_like, PR.TRANSPORT["Binary"]["Sha256"])
    allowed = ("falta la autorización", "no tiene el blob de la ronda 2 fijado en el kit")
    ok = ev["Ok"] is False and any("falta la autorización" in x for x in ev["Problems"]) and all(any(a in x for a in allowed) for x in ev["Problems"])
    return {"Case": "L00-compuerta-11-real-bloquea", "WorktreeHead": head, "CloneCommitUsed": commit, "Ok": ev["Ok"], "Problems": ev["Problems"],
            "A6BlobAtHead": ev["A6BlobAtHead"], "AuthorizationSections": ev["AuthorizationSections"], "A6LineNumbers": ev["A6LineNumbers"],
            "Note": "binario no medido en la sonda: se pasa el SHA-256 del cierre; nada se escribe en el worktree",
            "Result": "PASS" if ok else "FAIL"}


# ------------------------------------------------------------------ gate cases (transport_gate.evaluate, c3_analysis.analyze; unchanged)
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
        ev = TG.evaluate(gate, closure, cb, ab)
        got = ev["Problems"]
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
    return cases


def run_cases(pr, live=False):
    """All cases against an auditor module (the mutation harness passes mutants). → list of rows."""
    init_globals(pr)
    return audit_cases() + gate_cases() + owner_cases() + ([live_owner_gate_case()] if live else [])


def main(out_path):
    pr = load_auditor()
    cases = run_cases(pr, live=True)
    status = subprocess.run(["git", "-C", CLONE, "status", "--porcelain", "--ignored"], capture_output=True, text=True).stdout
    head = subprocess.run(["git", "-C", CLONE, "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
    run_match = {n: hashlib.sha256(open(os.path.join(RUN, n), "rb").read()).hexdigest() == hashlib.sha256(open(os.path.join(KITDATA, n), "rb").read()).hexdigest()
                 for n in pr.RUN_FILES}
    run_listing = sorted(os.listdir(RUN))
    audit_rows = [c for c in cases if not c["Case"].startswith(("G", "O", "L"))]
    gate_rows = [c for c in cases if c["Case"].startswith("G")]
    owner_rows = [c for c in cases if c["Case"].startswith(("O", "L"))]
    doc = {"Selftest": "post-review.py v5.2-a6r2 + transport_gate.py + owner_gate.py (ronda 2) + c3_analysis.analyze",
           "Environment": "SYNTHETIC (fase 1: prep/make_synthetic_env.py)" if pr.CLOSURE.get("Synthetic") else "REAL (clon y run del kit)",
           "Clone": CLONE, "Run": RUN, "Rev": pr.REV, "Object": {"Path": pr.OBJECT, "Blob": pr.OBJECT_BLOB}, "Provisional": pr.CLOSURE.get("Provisional"),
           "SessionId": pr.SESSION_ID, "RunId": pr.IDS["RunId"], "KitGateStatus": pr.GATE.get("Status"),
           "Modules": {n: hashlib.sha256(open(os.path.join(HERE, n), "rb").read()).hexdigest()
                       for n in ("post-review.py", "transport_gate.py", "owner_gate.py", "c3_analysis.py", "result.schema.json", "selftest-post-review.py")},
           "CloneHeadAfterSelftest": head, "CloneCleanAfterSelftest": status.strip() == "" and head == pr.REV,
           "RunFilesMatchKit": run_match, "RunDirListingAfterSelftest": run_listing,
           "Cases": cases,
           "Totals": {"Cases": len(cases), "Pass": sum(1 for c in cases if c["Result"] == "PASS"),
                      "AuditCases": len(audit_rows), "AuditPass": sum(1 for c in audit_rows if c["Result"] == "PASS"),
                      "GateCases": len(gate_rows), "GatePass": sum(1 for c in gate_rows if c["Result"] == "PASS"),
                      "OwnerGateCases": len(owner_rows), "OwnerGatePass": sum(1 for c in owner_rows if c["Result"] == "PASS")},
           "AllPass": all(c["Result"] == "PASS" for c in cases) and status.strip() == "" and head == pr.REV and all(run_match.values()) and
           run_listing == sorted(pr.RUN_FILES)}
    text = json.dumps(doc, ensure_ascii=False, indent=1)
    text = USER_RX.sub("%USERPROFILE%", text)
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text + "\n")
    for c in cases:
        line = "%-60s %s %s" % (c["Case"][:60], c["Result"], ("missing=%s unexpected=%s bad=%s sv=%s coh=%s" % (
            c.get("Missing"), c.get("Unexpected"), c.get("BadCounts"), c.get("SchemaValid"), c.get("ResultCoherence"))) if c["Result"] != "PASS" else "")
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
