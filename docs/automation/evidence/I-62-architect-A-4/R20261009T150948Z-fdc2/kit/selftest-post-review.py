"""Self-test of post-review.py v5.1-a4r and of transport_gate.py (and of c3_analysis.analyze), run BEFORE the launch of the formal RE-REVIEW of
A-4 (Coordinator disposition §62, point 2). Adapted from the self-test of the first A-4 kit (R20261009T122416Z-bce3): same cases and synthetic
formats, the A-4 review specifics replaced by the corrected object (0d954376), the re-review closure (six run files, among them delta.diff and
A-4.7d863219.md) and result schema (PriorFindingDispositions, eight Focus topics, Q-A4-01..17, DeltaConfirmation), plus cases 70-79 for the rules
the re-review restores or adds (prior-finding dispositions as in the A-2 re-review auditor, DeltaConfirmation).
Synthetic claude-cli runs (transcript JSONL in the format measured by the characterization C1, stream-json stdout, and the <run>/launch files
launch.py writes: run.json, preflight.json, stderr.txt and the copies of the transport-gate evidence) audited against the real kit clone, run
dir and closure. Each case audits against a SYNTHETIC gate (MEASURED by default, built from a c3_analysis.analyze record of a synthetic PASS
probe; ACCEPTED in case 41) set through the auditor's GATE seam; LAUNCH_DIR points to the case's temporary launch directory.

Cases (A = audit, every rule of the auditor has at least one case that FAILS without it; mutation-post-review.py checks that mechanically):
  00 accredited base; 01-08 reads/Grep/Glob outside the closure, junction escape, accepted forms, denied tool, Bash attempt; 09-11 prompt,
  extra user message, model/effort; 12 tampered run file; 13 no result and exit 1; 14-15 premises; 16 coherent NOT_ACCREDITED stop; 17-20
  coherence; 21 structured output inconsistent; 22 sidechain/subagents; 23 launch args and init; 24 run-dir and directory reads; 25 run-file
  premises accepted; 26 failed and degraded reads; 27 transcript session/cwd/version; 28-33 run.json; 34-37 preflight.json; 38-42 transport
  gate; 43-47 stream-json init/result; 48-49 transcript without assistant messages / without the first user message; 50 Glob `..` after a
  wildcard; 51-53 run dir entry, launch/ file, stdout outside launch/; 54 designated blob changed; 55 schema-invalid result; 56-64 coherence
  (duplicate ids, AGREED with consumption lines exceeding, A45Verdict NOT_ASSESSED with a verdict, Focus coverage, Q without its finding,
  NO_FINDING with ids, other commit, BLOCKED without OwnerDecisionRequired, other invocation ids); 65-69 Focus topic REQUIRED without its finding,
  A45Verdict CHANGES REQUIRED without a REQUIRED, BLOCKED without OwnerDecisionRequired and unknown ids, and AGREED with A4-5 excluded (accepted);
  70-79 prior findings (coverage, disposition values, A62-A4-01 without premises, STILL_OPEN / NOT_APPLIED without a link or without a REQUIRED
  link, links to unknown ids, prior ids reused without STILL_OPEN / NOT_APPLIED, a coherent STILL_OPEN re-review accepted) and DeltaConfirmation
  (NOT_DETERMINED with a verdict, NOT_CONFIRMED with AGREED).
Cases G (transport_gate.evaluate and c3_analysis.analyze): the valid MEASURED and ACCEPTED evidence, and one case per gate problem.

Every temporary file lives under <kit>/../.tmp/st and is removed at the end (with a guard against links). The only write outside the kit
directory is the temporary junction of case 03 inside the clone (target: the first A-4 clone D:\\r62-arch-a4, read target only), removed in `finally`;
the clone must end clean and the run files identical to the kit.
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
TMPROOT = os.path.join(os.path.dirname(HERE), ".tmp", "st")
os.makedirs(os.path.join(TMPROOT, "tmp"), exist_ok=True)
for k in ("TEMP", "TMP", "TMPDIR"):
    os.environ[k] = os.path.join(TMPROOT, "tmp")
tempfile.tempdir = os.path.join(TMPROOT, "tmp")
CLONE, RUN = r"D:\r62-arch-a4r", r"D:\r62-arch-a4r-run"
OTHER_CLONE = r"D:\r62-arch-a4"           # another kit's clone: only ever a junction target or a path that must be classified OUTSIDE
OBJ = "docs/initiatives/I-62-A-4.md"
PKG = "docs/initiatives/I-62-architect-package-A-4.md"
GR = "docs/automation/evidence/I-62-A4/a4-guards-result-c2.json"
ST_JSON = "docs/automation/evidence/I-62-A4/a4-selftest.json"
V14 = "docs/initiatives/I-62-proposal-v14.md"
A1 = "docs/initiatives/I-62-A-1.md"
CORRECTION_COMMIT = "a33324957f8778dd286a5a871881d403fd5a2d14"   # corrección 2 (≠ recibo)
C3_ADD = r"D:\r62-c3-probe\add\canary-add.txt"
C3_OUT = r"D:\r62-c3-probe\out\canary-out.txt"
DELETE = object()
CLONE_MUTATED = False     # True while the junction of case 03 exists (the mutation harness bypasses its caches then)


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


def load_auditor(path=None):
    pr = load("post_review_v5_1_a4r", path or os.path.join(HERE, "post-review.py"))
    pr.setup(CLONE, RUN, HERE)
    return pr


PROBE = load("c3_analysis", os.path.join(HERE, "c3_analysis.py"))


def init_globals(pr):
    global PR, TG, SID, REV, MODEL, EFFORT, SCHEMA, PROMPT, PROMPT_BYTES, ORIG_READ_RUN, TEMPLATE, BINPATH, BINSHA, VERSION
    PR, TG = pr, pr.TG
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


def patch(d, p):
    for k, v in p.items():
        if v is DELETE:
            d.pop(k, None)
        else:
            d[k] = v
    return d


# ------------------------------------------------------------------ synthetic transport-gate evidence
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
            "AcceptedBy": "principal-session", "Authority": "selftest", "Rationale": "selftest", "At": "2026-10-09T00:00:00Z"}


def synthetic_gate(status, char_b, acc_b=None):
    g = TG.initial_gate(PR.CLOSURE)
    g.update({"Status": status, "Characterization": {"Path": "%TEMP%\\selftest\\characterization.json", "Sha256": sha(char_b)},
              "Acceptance": {"Path": "%TEMP%\\selftest\\acceptance.json", "Sha256": sha(acc_b)} if acc_b is not None else None})
    return g


# ------------------------------------------------------------------ synthetic run
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
                  {"type": "model", "identity": {"modelId": MODEL}}, {"type": "date", "date": "2026-10-09"}):
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
        pf = {"SessionId": SID, "At": "2026-10-09T00:00:00Z", "Checks": checks}
        if self.pf_hook:
            self.pf_hook(pf)
        open(os.path.join(launch, "preflight.json"), "wb").write(jbytes(pf))
        rj = {"SessionId": SID, "Executable": BINPATH, "ExecutableSha256": BINSHA, "Args": PR.expected_args(SCHEMA), "Cwd": CLONE,
              "PromptSha256": sha(PROMPT_BYTES), "PromptBytes": len(PROMPT_BYTES), "TimeoutSeconds": PR.TRANSPORT["TimeoutSeconds"],
              "TransportGate": {"Status": self.gate_mode, "CharacterizationSha256": sha(char_b), "AcceptanceSha256": sha(acc_b) if acc_b else None},
              "Start": "2026-10-09T00:00:00Z", "Pid": 4242, "End": "2026-10-09T00:10:00Z", "ExitCode": 0, "Killed": False, "TimedOut": False,
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


def step0(r):
    r.call("Read", {"file_path": RUN + "\\order.txt"}, numbered(lines_of(run="order.txt")))
    r.call("Read", {"file_path": RUN + "\\delta.diff", "offset": 1, "limit": 4}, numbered(lines_of(run="delta.diff")[:4]))
    r.call("Read", {"file_path": absp(PKG), "offset": 1, "limit": 52}, numbered(lines_of(PKG)[:52]))
    r.call("Read", {"file_path": absp(GR), "offset": 1, "limit": 7}, numbered(lines_of(GR)[:7]))
    r.call("Read", {"file_path": absp(GR), "offset": 1421, "limit": 17}, numbered(lines_of(GR)[1420:1437], 1421))
    r.call("Read", {"file_path": absp(ST_JSON), "offset": 30, "limit": 21}, numbered(lines_of(ST_JSON)[29:50], 30))


def base_calls(r):
    r.header()
    step0(r)
    r.call("Read", {"file_path": absp(OBJ), "offset": 204, "limit": 22}, numbered(lines_of(OBJ)[203:225], 204))
    r.call("Read", {"file_path": absp(OBJ), "offset": 317, "limit": 12}, numbered(lines_of(OBJ)[316:328], 317))
    r.call("Read", {"file_path": absp(OBJ), "offset": 358, "limit": 25}, numbered(lines_of(OBJ)[357:382], 358))
    r.call("Read", {"file_path": absp(V14), "offset": 2519, "limit": 10}, numbered(lines_of(V14)[2518:2528], 2519))


def stop_calls(r):
    r.header()
    step0(r)


def schema_keys(prop):
    schema = json.load(open(SCHEMA, encoding="utf-8"))
    return [k for k in schema["properties"][prop]["required"]]


def base_result():
    q1n, q1 = find_line(OBJ, "| Q-A4-01 |")
    h1n, h1 = find_line(OBJ, "> «**Aceptación preautorizada de bindings dentro de la ventana (A-4).**")
    m2n, m2 = find_line(OBJ, "| M-02 | **sí** |")
    ids = PR.IDS
    return {
        "Schema": "rackcad-architect-review-result/v1 (selftest)", "RunId": ids["RunId"], "InvocationId": ids["InvocationId"],
        "LogicalReviewRequestId": ids["LogicalReviewRequestId"], "AttemptSeq": ids["AttemptSeq"], "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN",
        "ReviewedUnit": "I-62", "ReviewedCommit": REV, "ReviewedPath": OBJ, "ReviewedBlob": PR.OBJECT_BLOB,
        "ReviewerMode": "SEPARATE SESSION", "SamePersonAsAuthor": "selftest",
        "ReviewerDeclaredIdentity": {"Runtime": "x", "Model": "x", "Effort": "x", "SessionOrThread": "x"},
        "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["x"]},
        "IdentityCheck": dict([("Basis", "INVOKER_VERIFIED_CLONE")] + [(k, True) for k in schema_keys("IdentityCheck") if k not in ("Basis", "Note")] +
                              [("Note", "x")]),
        "InputsRead": ["x"], "Verdict": "AGREED",
        "PriorFindingDispositions": [{"FindingId": f, "Disposition": ("CLOSED" if f == "A62-A4-01" else "N/A" if f == "A62-A4-O8" else "APPLIED"),
                                      "LinkedFindingIds": [], "Rationale": "x",
                                      "PremiseRefs": ([{"Path": OBJ, "Section": "§5", "LineStart": m2n, "LineEnd": m2n, "Quote": m2}]
                                                      if f == "A62-A4-01" else [])} for f in PR.PRIOR_IDS],
        "RequiredFindings": [], "OptionalFindings": [],
        "Focus": [{"Topic": t, "Disposition": "NO_FINDING", "FindingIds": [], "Rationale": "x",
                   "PremiseRefs": ([{"Path": OBJ, "Section": "§3.4", "LineStart": h1n, "LineEnd": h1n, "Quote": h1}]
                                   if t == "H1_A4-3_NO_FABRICATED_ACCEPTANCE" else [])}
                  for t in PR.FOCUS_TOPICS],
        "A45Verdict": {"Verdict": "AGREED", "FindingIds": [], "Rationale": "x"},
        "QuestionDispositions": [{"Id": q, "Disposition": "NO_FINDING", "FindingIds": [], "Rationale": "x",
                                  "PremiseRefs": ([{"Path": OBJ, "Section": "§8", "LineStart": q1n, "LineEnd": q1n, "Quote": q1}] if q == "Q-A4-01" else [])}
                                 for q in PR.QUESTION_IDS],
        "DeltaConfirmation": {k: {"Assessment": "CONFIRMED", "Note": "x"} for k in PR.DELTA_KEYS},
        "NoChangeConfirmation": {k: {"Assessment": "NO_CHANGE", "Note": "x"} for k in schema_keys("NoChangeConfirmation")},
        "OwnerConsumptionLines": {"Assessment": "ONLY_CONSUMPTION", "Note": "x"},
        "GuardsVerification": {"Notes": [{"Guard": "G5b", "ClaimHolds": "NOT_VERIFIED", "Note": "x"}], "Note": "x"},
        "Materiality": {"Overall": {"M0%d" % i: ("YES" if i in (2, 3, 4, 5) else "NO") for i in range(1, 9)}, "Note": "x"},
        "OwnerAuthorityCheck": dict([(k, "NONE") for k in schema_keys("OwnerAuthorityCheck") if k != "OwnerDecisionRequired"] +
                                    [("OwnerDecisionRequired", False)]),
        "IfAgreed": {k: True for k in schema_keys("IfAgreed")}, "KnownLimitations": [], "RecommendedNextAction": "x"}


def required_finding(fid="A62-A4-02", vi="x", ce=""):
    n, l = find_line(OBJ, "| Q-A4-06 |")
    return {"FindingId": fid, "AffectedDelta": "§3.2, A4-1", "ViolatedInvariant": vi, "Counterexample": ce, "CorrectionRequired": "x",
            "PremiseRefs": [{"Path": OBJ, "Section": "§8", "LineStart": n, "LineEnd": n, "Quote": l}], "WhyItMatters": "x"}


def optional_finding(fid="A62-A4-O9"):
    return {"FindingId": fid, "AffectedDelta": "§3.2", "Note": "x", "PremiseRefs": []}


def changes_required(res):
    """A coherent CHANGES REQUIRED skeleton (A62-A4-02 REQUIRED, Q-A4-06 REQUIRED; A62-A4-01 CLOSED) for the targeted coherence cases."""
    res.update({"Verdict": "CHANGES REQUIRED", "RequiredFindings": [required_finding()], "IfAgreed": {k: None for k in res["IfAgreed"]}})
    q = {d["Id"]: d for d in res["QuestionDispositions"]}
    q["Q-A4-06"].update({"Disposition": "REQUIRED", "FindingIds": ["A62-A4-02"]})
    return q


def focus(res):
    return {d["Topic"]: d for d in res["Focus"]}


def prior(res):
    return {d["FindingId"]: d for d in res["PriorFindingDispositions"]}


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
    d = os.path.join(TMPROOT, "t", name.split(" ")[0])
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
    add(variant("00-base-acreditada"))

    def outside_abs(r):
        r.call("Read", {"file_path": absp("docs/HANDOFF.md"), "offset": 1, "limit": 5}, numbered(["# HANDOFF"]))
        r.call("Read", {"file_path": os.path.expanduser("~") + "\\.claude\\projects\\D--Documentos-Codex-Calculadora-de-racks\\memory\\MEMORY.md"},
               "File does not exist.", is_error=True)
    add(variant("01-lectura-fuera-absoluta", outside_abs, ["lectura fuera del cierre: D:/r62-arch-a4r/docs/HANDOFF.md",
                                                           "lectura fuera del cierre (intento fallido"]))

    def outside_rel(r):
        r.call("Read", {"file_path": "docs/HANDOFF.md"}, numbered(["# HANDOFF"]))
        r.call("Read", {"file_path": "..\\r62-arch-a3\\docs\\initiatives\\I-62-A-3.md", "offset": 1, "limit": 3}, numbered(["# x"]))
        r.call("Read", {"file_path": "docs\\initiatives\\..\\..\\AGENTS.md"}, numbered(["# AGENTS"]))
        r.call("Read", {"file_path": "docs/initiatives/I-62-A-4.md", "offset": 358, "limit": 5}, numbered(lines_of(OBJ)[357:362], 358))
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
        r.call("Grep", {"pattern": "INVALID_LAUNCH", "path": CLONE + "\\docs"}, "docs/x.md")
        r.call("Grep", {"pattern": "x", "path": OTHER_CLONE}, "x")
        r.call("Grep", {"pattern": "x"}, "x")
        r.call("Grep", {"pattern": "x", "path": absp(OBJ), "glob": "*.md"}, "1:x")
        r.call("Grep", {"pattern": "x", "path": RUN}, "x")
    add(variant("04-grep-directorios-y-fuera", grep_dirs, ["Grep sobre un directorio", "Grep fuera del cierre", "Grep sin path", "Grep con glob o type"],
                counts={"Grep sobre un directorio": 2}))

    def glob_out(r):
        r.call("Glob", {"pattern": "**/*.md", "path": OTHER_CLONE}, "No files found")
        r.call("Glob", {"pattern": "C:/Users/*/.claude/projects/**/*.jsonl"}, "No files found")
        r.call("Glob", {"pattern": "docs/adr/*.md"}, "D:\\r62-arch-a4r\\docs\\adr\\x.md")
        r.call("Glob", {"pattern": "../r62-arch-a3/**"}, "No files found")
        r.call("Glob", {"pattern": "docs/initiatives/I-62-A-4.md"}, CLONE + "\\docs\\HANDOFF.md")
    add(variant("05-glob-fuera-del-cierre", glob_out, ["Glob fuera del clon y del run", "Glob alcanza", "Glob devolvió una ruta fuera del cierre"],
                counts={"Glob fuera del clon y del run": 3}))

    def accepted(r):
        r.call("Glob", {"pattern": "docs/initiatives/I-62-A-4.md"}, absp(OBJ))
        r.call("Glob", {"pattern": "**/I-62-architect-package-A-4.md"}, absp(PKG))
        r.call("Glob", {"pattern": "*.txt", "path": RUN}, RUN + "\\order.txt")
        n, l = find_line(OBJ, "| Q-A4-01 |")
        r.call("Grep", {"pattern": "Q-A4-01", "path": absp(OBJ), "output_mode": "content", "-n": True}, "%d:%s" % (n, l))
        r.call("Grep", {"pattern": "U-09", "path": RUN + "\\order.txt", "output_mode": "content", "-n": True},
               "36:" + lines_of(run="order.txt")[35])
    add(variant("06-glob-y-grep-aceptados", accepted))

    def denied(r):
        r.call("Read", {"file_path": r"D:\r62-fixture\A4\README.md"}, "Permission to read D:\\r62-fixture\\A4\\README.md has been denied.", is_error=True)
        r.res_patch["permission_denials"] = [{"tool_name": "Read", "tool_use_id": "toolu_010", "tool_input": {"file_path": r"D:\r62-fixture\A4\README.md"}}]
    add(variant("07-herramienta-denegada", denied, ["lectura fuera del cierre (intento fallido", "permiso denegado", "permission_denials: Read"]))

    def bash(r):
        r.call("Bash", {"command": "git -C D:/r62-arch-a4r rev-parse HEAD"}, "<tool_use_error>Error: No such tool available: Bash</tool_use_error>",
               is_error=True)
    add(variant("08-intento-de-bash", bash, ["herramienta no permitida (Bash)"]))

    def prompt_hook(r):
        r.first_prompt = PROMPT + "\n"
    add(variant("09-prompt-distinto", expected=["fidelidad del prompt"], run_hook=prompt_hook))

    def extra_user(r):
        r.entry("user", message={"role": "user", "content": "continúa con la revisión"})
    add(variant("10-mensaje-de-usuario-extra", extra_user, ["mensajes de usuario además del prompt"]))

    def wrong_model(r):
        r.call("Read", {"file_path": absp(OBJ), "offset": 1, "limit": 3}, numbered(lines_of(OBJ)[:3]), model="claude-sonnet-5", effort="high")
        r.res_patch["modelUsage"] = {MODEL: {}, "claude-haiku-4-5": {}}
    add(variant("11-modelo-y-effort-distintos", wrong_model, ["modelo observado distinto", "effort observado distinto", "modelUsage con modelos distintos",
                                                               "mensaje del asistente con modelo"]))

    try:
        PR.read_run_bytes = lambda n: ORIG_READ_RUN(n) + (b"\n+tampered\n" if n == "order-s60.txt" else b"")
        add(variant("12-archivo-del-run-alterado", expected=["archivo del run distinto del custodiado (copia del kit y RunFileHashes del cierre): order-s60.txt"],
                    counts={"archivo del run distinto del custodiado": 1}))
    finally:
        PR.read_run_bytes = ORIG_READ_RUN

    def killed(r):
        r.call("Read", {"file_path": absp(V14), "offset": 2530, "limit": 25}, None, result=False)
        r.no_result = True
        r.run_patch.update({"ExitCode": 1, "Killed": True})
    add(variant("13-sin-result-y-salida-1", killed, ["sin mensaje result", "código de salida 1", "proceso terminado", "no hay resultado estructurado",
                                                     "llamada sin resultado"], final=False, schema_valid=False))

    def premise_unread(res, r):
        a1 = PR.canon(A1)
        res["OptionalFindings"] = [{"FindingId": "A62-A4-O9", "AffectedDelta": "§3.3", "Note": "x", "PremiseRefs": [
            {"Path": A1, "Section": "§2", "LineStart": 100, "LineEnd": 101, "Quote": a1[99]}]}]
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-03"].update({"Disposition": "OPTIONAL", "FindingIds": ["A62-A4-O9"]})
    add(variant("14-premisa-sobre-lineas-no-leidas", expected=["premisa sobre líneas no entregadas fielmente en A62-A4-O9 (docs/initiatives/I-62-A-1.md 100-101)"],
                result_patch=premise_unread))

    def premise_bad(res, r):
        res["OptionalFindings"] = [{"FindingId": "A62-A4-O9", "AffectedDelta": "§3.2", "Note": "x", "PremiseRefs": [
            {"Path": RUN + "\\prompt.md", "Section": "cabecera", "LineStart": 1, "LineEnd": 1, "Quote": PROMPT.split("\n")[0][:40]},
            {"Path": RUN + "\\order.txt", "Section": "2", "LineStart": 22, "LineEnd": 22, "Quote": lines_of(run="order.txt")[20]}]}]
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-03"].update({"Disposition": "OPTIONAL", "FindingIds": ["A62-A4-O9"]})
    add(variant("15-premisa-sobre-prompt-o-linea-desplazada", expected=["premisa no canónica en A62-A4-O9 (prompt.md 1-1)",
                                                                         "premisa no canónica en A62-A4-O9 (order.txt 22-22)"], result_patch=premise_bad))

    def stop(res, r):
        res.update({"Verdict": "NOT_ACCREDITED", "QuestionDispositions": [], "Focus": [], "PriorFindingDispositions": [], "InputsRead": [RUN + "\\order.txt"],
                    "KnownLimitations": ["Paso 0: discordancia de identidad; revisión detenida antes de leer el objeto"],
                    "IfAgreed": {k: None for k in res["IfAgreed"]}, "ReviewedCommit": None, "ReviewedBlob": None,
                    "A45Verdict": {"Verdict": "NOT_ASSESSED", "FindingIds": [], "Rationale": "no evaluado"}})
        res["IdentityCheck"].update({k: None for k in res["IdentityCheck"] if k not in ("Basis", "Note")})
        res["IdentityCheck"]["DeltaIndexMatchesBlobs"] = False
        res["DeltaConfirmation"] = {k: {"Assessment": "NOT_DETERMINED", "Note": "no evaluado"} for k in res["DeltaConfirmation"]}
        res["Materiality"] = {"Overall": {"M0%d" % i: "NOT_ASSESSED" for i in range(1, 9)}, "Note": "no evaluada"}
        res["OwnerAuthorityCheck"]["OwnerDecisionRequired"] = None
        res["NoChangeConfirmation"] = {k: {"Assessment": "NOT_DETERMINED", "Note": "no evaluado"} for k in res["NoChangeConfirmation"]}
        res["OwnerConsumptionLines"] = {"Assessment": "NOT_DETERMINED", "Note": "no evaluado"}
        res["GuardsVerification"] = {"Notes": [], "Note": "no evaluadas"}
    add(variant("16-parada-not-accredited-coherente", result_patch=stop, base=stop_calls, require_coherent=True))

    def incoherent_agreed(res, r):
        res["RequiredFindings"] = [required_finding()]
        res["QuestionDispositions"] = [d for d in res["QuestionDispositions"] if d["Id"] != "Q-A4-17"]
        res["NoChangeConfirmation"]["Production"]["Assessment"] = "CHANGE"
    add(variant("17-coherencia-agreed", expected=["QuestionDispositions debe disponer Q-A4-01..Q-A4-17", "AGREED exige"], result_patch=incoherent_agreed))

    def incoherent_cr(res, r):
        res["Verdict"] = "CHANGES REQUIRED"
        res["OptionalFindings"] = [optional_finding()]
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-05"].update({"Disposition": "OPTIONAL", "FindingIds": ["A62-A4-O9"]})
    add(variant("18-coherencia-changes-required", expected=["CHANGES REQUIRED sin ningún REQUIRED", "IfAgreed debe ir en null"],
                result_patch=incoherent_cr))

    def required_shape(res, r):
        res.update({"Verdict": "CHANGES REQUIRED", "RequiredFindings": [required_finding(vi="", ce="")], "IfAgreed": {k: None for k in res["IfAgreed"]}})
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-06"].update({"Disposition": "REQUIRED", "FindingIds": ["A62-A4-02"]})
        res["Materiality"]["Overall"]["M03"] = "NOT_ASSESSED"
        res["IdentityCheck"]["GuardsResultComparesWithPreviousBlob"] = None
        res["OwnerAuthorityCheck"]["OwnerDecisionRequired"] = None
    add(variant("19-coherencia-forma-del-required-y-nulos", expected=["A62-A4-02 REQUIRED sin ViolatedInvariant ni Counterexample",
                                                                      "Materiality.Overall M01..M08 va en YES o NO", "IdentityCheck no admite null",
                                                                      "OwnerDecisionRequired no admite null"], result_patch=required_shape))

    def na_no_reason(res, r):
        res.update({"Verdict": "NOT_ACCREDITED", "KnownLimitations": []})
    add(variant("20-not-accredited-sin-motivo", expected=["NOT_ACCREDITED sin el motivo en KnownLimitations", "IfAgreed debe ir en null"],
                result_patch=na_no_reason))

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

    order = lines_of(run="order.txt")
    s60 = lines_of(run="order-s60.txt")
    s61 = lines_of(run="order-s61.txt")
    dd = lines_of(run="delta.diff")
    prev = lines_of(run="A-4.7d863219.md")
    s60_n = next(i + 1 for i, l in enumerate(s60) if l.startswith("2. F6-OBS-03"))
    s60_m = next(i + 1 for i, l in enumerate(s60) if l.startswith("3. Prioridad absoluta"))
    s61_n = next(i + 1 for i, l in enumerate(s61) if "27ffa26b" in l)
    dd_n = next(i + 1 for i, l in enumerate(dd) if l.startswith("+| M-02 |"))
    prev_n = next(i + 1 for i, l in enumerate(prev) if l.startswith("| M-02 |"))

    def run_reads(r):
        r.call("Read", {"file_path": RUN + "\\order-s60.txt"}, numbered(s60))
        r.call("Read", {"file_path": RUN + "\\order-s61.txt"}, numbered(s61))
        r.call("Read", {"file_path": RUN + "\\delta.diff"}, numbered(dd))
        r.call("Read", {"file_path": RUN + "\\A-4.7d863219.md", "offset": 290, "limit": 12}, numbered(prev[289:301], 290))

    def premise_run(res, r):
        res["OptionalFindings"] = [{"FindingId": "A62-A4-O9", "AffectedDelta": "§3.2", "Note": "x", "PremiseRefs": [
            {"Path": RUN + "\\order.txt", "Section": "2", "LineStart": 21, "LineEnd": 21, "Quote": order[20]},
            {"Path": "order.txt", "Section": "3", "LineStart": 36, "LineEnd": 36, "Quote": order[35]},
            {"Path": RUN + "\\order-s60.txt", "Section": "2", "LineStart": s60_n, "LineEnd": s60_n, "Quote": s60[s60_n - 1]},
            {"Path": "order-s60.txt", "Section": "3", "LineStart": s60_m, "LineEnd": s60_m, "Quote": s60[s60_m - 1]},
            {"Path": RUN + "\\order-s61.txt", "Section": "2", "LineStart": s61_n, "LineEnd": s61_n, "Quote": s61[s61_n - 1]},
            {"Path": RUN + "\\delta.diff", "Section": "§5", "LineStart": dd_n, "LineEnd": dd_n, "Quote": dd[dd_n - 1]},
            {"Path": "A-4.7d863219.md", "Section": "§5", "LineStart": prev_n, "LineEnd": prev_n, "Quote": prev[prev_n - 1]}]}]
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-06"].update({"Disposition": "OPTIONAL", "FindingIds": ["A62-A4-O9"]})
    add(variant("25-premisas-sobre-archivos-del-run-aceptadas", run_reads, result_patch=premise_run, require_coherent=True))

    def failed_degraded(r):
        r.call("Read", {"file_path": absp(A1), "offset": 90, "limit": 10}, "File does not exist.", is_error=True)
        v = lines_of(V14)[2529:2532]
        r.call("Read", {"file_path": absp(V14), "offset": 2530, "limit": 3}, numbered([v[0], v[1] + " X", v[2]], 2530))

    a1_n = next(n for n in range(90, 100) if PR.canon(A1)[n - 1].strip())    # a non-empty line inside the failed Read (90-99)

    def premise_failed(res, r):
        a1 = PR.canon(A1)
        res["OptionalFindings"] = [{"FindingId": "A62-A4-O9", "AffectedDelta": "§3.3", "Note": "x", "PremiseRefs": [
            {"Path": A1, "Section": "§2", "LineStart": a1_n, "LineEnd": a1_n, "Quote": a1[a1_n - 1]}]}]
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-03"].update({"Disposition": "OPTIONAL", "FindingIds": ["A62-A4-O9"]})
    rv = variant("26-lectura-fallida-y-degradada", failed_degraded, ["premisa sobre líneas no entregadas fielmente en A62-A4-O9 (docs/initiatives/I-62-A-1.md %d-%d)" % (a1_n, a1_n),
                                                                     "fidelidad: docs/initiatives/I-62-proposal-v14.md"], result_patch=premise_failed)
    rv[0]["FailedReadRecorded"] = any(f["Path"] == A1 for f in rv[1]["Fidelity"]["FailedReadsNeverCredited"])
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
                expected=["el result no trae structured_output", "no hay resultado estructurado"], schema_valid=False))

    # ---- transcript
    q1n = find_line(OBJ, "| Q-A4-01 |")[0]
    h1n = find_line(OBJ, "> «**Aceptación preautorizada de bindings dentro de la ventana (A-4).**")[0]
    m2n = find_line(OBJ, "| M-02 | **sí** |")[0]
    add(variant("48-transcripcion-sin-asistente", run_hook=hook(tx_filter=lambda e: e.get("type") != "assistant"),
                expected=["ningún mensaje del asistente", "structured_output del result distinto",
                          "premisa sobre líneas no entregadas fielmente en Q-A4-01 (%s %d-%d)" % (OBJ, q1n, q1n),
                          "premisa sobre líneas no entregadas fielmente en H1_A4-3_NO_FABRICATED_ACCEPTANCE (%s %d-%d)" % (OBJ, h1n, h1n),
                          "premisa sobre líneas no entregadas fielmente en A62-A4-01 (%s %d-%d)" % (OBJ, m2n, m2n)]))
    add(variant("49-transcripcion-sin-primer-usuario",
                run_hook=hook(tx_filter=lambda e: not (e.get("type") == "user" and isinstance((e.get("message") or {}).get("content"), str))),
                expected=["transcripción: sin primer mensaje de usuario"]))

    def glob_dotdot(r):
        r.call("Glob", {"pattern": "docs/*/../../docs/initiatives/I-62-A-4.md"}, absp(OBJ))
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
        PR.clone_git = lambda *a: ("0" * 40 + "\n") if a == ("rev-parse", "HEAD:" + OBJ) else orig_git(*a)
        add(variant("54-clon-blob-cambiado", expected=["el clon no conserva los blobs designados del cierre"]))
    finally:
        PR.clone_git = orig_git

    # ---- result schema and coherence
    def extra_field(res, r):
        res["Extra"] = 1
    add(variant("55-esquema-invalido", result_patch=extra_field, expected=["resultado: no valida contra result.schema.json"], schema_valid=False))

    def dup_ids(res, r):
        changes_required(res)
        res["RequiredFindings"].append(required_finding())
    add(variant("56-coherencia-ids-repetidos", result_patch=dup_ids, expected=["identificadores de hallazgo repetidos"]))

    def lines_exceed(res, r):
        res["OwnerConsumptionLines"]["Assessment"] = "EXCEEDS_CONSUMPTION"
    add(variant("57-coherencia-agreed-lineas-del-owner", result_patch=lines_exceed, expected=["AGREED exige"]))

    def a45_not_assessed(res, r):
        res["A45Verdict"]["Verdict"] = "NOT_ASSESSED"
    add(variant("58-coherencia-a45-no-evaluado-con-veredicto", result_patch=a45_not_assessed, expected=["A45Verdict no admite NOT_ASSESSED",
                                                                                                        "AGREED exige"]))

    def focus_short(res, r):
        res["Focus"] = res["Focus"][:-1]
    add(variant("59-coherencia-focus-incompleto", result_patch=focus_short, expected=["Focus debe disponer los ocho temas"]))

    def q_without(res, r):
        q = changes_required(res)
        res["OptionalFindings"] = [optional_finding()]
        q["Q-A4-06"]["FindingIds"] = ["A62-A4-O9"]
        q["Q-A4-03"].update({"Disposition": "OPTIONAL", "FindingIds": []})
        q["Q-A4-04"].update({"Disposition": "REQUIRED", "FindingIds": ["A62-A4-02"]})
    add(variant("60-coherencia-q-sin-su-hallazgo", result_patch=q_without, expected=["Q-A4-06 REQUIRED sin un REQUIRED de RequiredFindings",
                                                                                    "Q-A4-03 OPTIONAL sin un OPTIONAL de OptionalFindings"]))

    def nofinding_ids(res, r):
        res["OptionalFindings"] = [optional_finding()]
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-05"]["FindingIds"] = ["A62-A4-O9"]
    add(variant("61-coherencia-no-finding-con-ids", result_patch=nofinding_ids, expected=["Q-A4-05 NO_FINDING con hallazgos vinculados"]))

    def other_object(res, r):
        res["ReviewedCommit"] = CORRECTION_COMMIT
    add(variant("62-coherencia-objeto-distinto", result_patch=other_object, expected=["objeto revisado distinto del designado"]))

    def blocked(res, r):
        res.update({"Verdict": "BLOCKED — OWNER DECISION", "IfAgreed": {k: None for k in res["IfAgreed"]}})
    add(variant("63-coherencia-blocked-sin-owner", result_patch=blocked, expected=["BLOCKED — OWNER DECISION sin OwnerDecisionRequired"]))

    def other_ids(res, r):
        res["RunId"] = "R20261009T000000Z-0000"
    add(variant("64-coherencia-ids-de-invocacion", result_patch=other_ids, expected=["identificadores de la invocación distintos"]))

    def focus_required_without(res, r):
        changes_required(res)
        res["OptionalFindings"] = [optional_finding()]
        focus(res)["F6-OBS-03_A4-1_CMD_ROUTE"].update({"Disposition": "REQUIRED", "FindingIds": ["A62-A4-O9"]})
    add(variant("65-coherencia-tema-required-sin-su-hallazgo", result_patch=focus_required_without,
                expected=["F6-OBS-03_A4-1_CMD_ROUTE REQUIRED sin un REQUIRED de RequiredFindings"]))

    def a45_cr_without(res, r):
        changes_required(res)
        res["A45Verdict"] = {"Verdict": "CHANGES REQUIRED", "FindingIds": [], "Rationale": "x"}
    add(variant("66-coherencia-a45-changes-required-sin-required", result_patch=a45_cr_without,
                expected=["A45Verdict CHANGES REQUIRED sin un REQUIRED vinculado"]))

    def a45_blocked_without(res, r):
        changes_required(res)
        res["A45Verdict"] = {"Verdict": "BLOCKED — OWNER DECISION", "FindingIds": [], "Rationale": "x"}
    add(variant("67-coherencia-a45-blocked-sin-owner", result_patch=a45_blocked_without,
                expected=["A45Verdict BLOCKED — OWNER DECISION sin OwnerDecisionRequired"]))

    def a45_unknown(res, r):
        res["A45Verdict"]["FindingIds"] = ["A62-A4-O10"]
    add(variant("68-coherencia-a45-hallazgo-inexistente", result_patch=a45_unknown, expected=["A45Verdict cita en FindingIds hallazgos que no existen"]))

    def agreed_without_a45(res, r):
        res["OptionalFindings"] = [optional_finding()]
        res["A45Verdict"] = {"Verdict": "EXCLUDE", "FindingIds": ["A62-A4-O9"], "Rationale": "x"}
        focus(res)["A4-5_SEPARABILITY"].update({"Disposition": "OPTIONAL", "FindingIds": ["A62-A4-O9"]})
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-14"].update({"Disposition": "OPTIONAL", "FindingIds": ["A62-A4-O9"]})
    add(variant("69-agreed-con-a4-5-excluida-aceptado", result_patch=agreed_without_a45, require_coherent=True))

    # ---- prior findings (rules restored from the A-2 re-review auditor) and DeltaConfirmation
    def prior_short(res, r):
        res["PriorFindingDispositions"] = [d for d in res["PriorFindingDispositions"] if d["FindingId"] != "A62-A4-O8"]
    add(variant("70-anteriores-cobertura", result_patch=prior_short, expected=["PriorFindingDispositions debe disponer"]))

    def prior_values(res, r):
        prior(res)["A62-A4-01"]["Disposition"] = "APPLIED"
        prior(res)["A62-A4-O3"]["Disposition"] = "CLOSED"
    add(variant("71-anteriores-valores", result_patch=prior_values, expected=["A62-A4-01 debe disponerse CLOSED o STILL_OPEN",
                                                                             "A62-A4-O3 debe disponerse APPLIED, NOT_APPLIED o N/A", "AGREED exige"]))

    def prior_no_premise(res, r):
        prior(res)["A62-A4-01"]["PremiseRefs"] = []
    add(variant("72-anterior-sin-premisa", result_patch=prior_no_premise, expected=["A62-A4-01 sin PremiseRefs"]))

    def prior_unlinked(res, r):
        changes_required(res)
        prior(res)["A62-A4-01"]["Disposition"] = "STILL_OPEN"
        prior(res)["A62-A4-O2"]["Disposition"] = "NOT_APPLIED"
    add(variant("73-anteriores-abiertos-sin-vinculo", result_patch=prior_unlinked, expected=["A62-A4-01 STILL_OPEN sin un hallazgo vinculado",
                                                                                            "A62-A4-O2 NOT_APPLIED sin un hallazgo vinculado"]))

    def prior_optional_link(res, r):
        changes_required(res)
        res["OptionalFindings"] = [optional_finding()]
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-05"].update({"Disposition": "OPTIONAL", "FindingIds": ["A62-A4-O9"]})
        prior(res)["A62-A4-01"].update({"Disposition": "STILL_OPEN", "LinkedFindingIds": ["A62-A4-O9"]})
    add(variant("74-anterior-abierto-sin-required", result_patch=prior_optional_link, expected=["A62-A4-01 STILL_OPEN sin un REQUIRED vinculado"]))

    def prior_unknown_link(res, r):
        prior(res)["A62-A4-O4"]["LinkedFindingIds"] = ["A62-A4-O11"]
    add(variant("75-anterior-vinculo-inexistente", result_patch=prior_unknown_link, expected=["A62-A4-O4 cita en LinkedFindingIds hallazgos que no existen"]))

    def prior_reused(res, r):
        changes_required(res)
        res["RequiredFindings"].append(required_finding("A62-A4-01"))
        res["OptionalFindings"] = [optional_finding("A62-A4-O4")]
    add(variant("76-ids-anteriores-reutilizados", result_patch=prior_reused, expected=["id anterior A62-A4-01 reutilizado sin disponerlo STILL_OPEN",
                                                                                       "id anterior A62-A4-O4 reutilizado sin disponerlo NOT_APPLIED"]))

    def prior_still_open_ok(res, r):
        res.update({"Verdict": "CHANGES REQUIRED", "RequiredFindings": [required_finding("A62-A4-01")], "OptionalFindings": [optional_finding("A62-A4-O5")],
                    "IfAgreed": {k: None for k in res["IfAgreed"]}})
        prior(res)["A62-A4-01"].update({"Disposition": "STILL_OPEN", "LinkedFindingIds": ["A62-A4-01"]})
        prior(res)["A62-A4-O5"].update({"Disposition": "NOT_APPLIED", "LinkedFindingIds": ["A62-A4-O5"]})
        q = {d["Id"]: d for d in res["QuestionDispositions"]}
        q["Q-A4-15"].update({"Disposition": "REQUIRED", "FindingIds": ["A62-A4-01"]})
        q["Q-A4-05"].update({"Disposition": "OPTIONAL", "FindingIds": ["A62-A4-O5"]})
        focus(res)["MATERIALITY_M02_M05"].update({"Disposition": "REQUIRED", "FindingIds": ["A62-A4-01"]})
    add(variant("77-re-revision-still-open-coherente-aceptada", result_patch=prior_still_open_ok, require_coherent=True))

    def delta_undetermined(res, r):
        res["DeltaConfirmation"]["A44Rules1to4IdenticalTo7d863219"]["Assessment"] = "NOT_DETERMINED"
    add(variant("78-delta-no-determinada-con-veredicto", result_patch=delta_undetermined,
                expected=["DeltaConfirmation va en CONFIRMED o NOT_CONFIRMED", "AGREED exige"]))

    def delta_not_confirmed(res, r):
        res["DeltaConfirmation"]["A42IdenticalTo7d863219"]["Assessment"] = "NOT_CONFIRMED"
    add(variant("79-agreed-con-delta-no-confirmada", result_patch=delta_not_confirmed, expected=["AGREED exige"]))
    return cases


# ------------------------------------------------------------------ gate cases (transport_gate.evaluate, c3_analysis.analyze)
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


def run_cases(pr):
    """All cases against an auditor module (the mutation harness passes mutants). → list of rows."""
    init_globals(pr)
    return audit_cases() + gate_cases()


def main(out_path):
    pr = load_auditor()
    cases = run_cases(pr)
    status = subprocess.run(["git", "-C", CLONE, "status", "--porcelain", "--ignored"], capture_output=True, text=True).stdout
    head = subprocess.run(["git", "-C", CLONE, "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
    run_match = {n: hashlib.sha256(open(os.path.join(RUN, n), "rb").read()).hexdigest() == hashlib.sha256(open(os.path.join(HERE, n), "rb").read()).hexdigest()
                 for n in pr.RUN_FILES}
    run_listing = sorted(os.listdir(RUN))
    audit_rows = [c for c in cases if not c["Case"].startswith("G")]
    gate_rows = [c for c in cases if c["Case"].startswith("G")]
    doc = {"Selftest": "post-review.py v5.1-a4r + transport_gate.py + c3_analysis.analyze", "Clone": CLONE, "Run": RUN, "Rev": pr.REV,
           "SessionId": pr.SESSION_ID, "KitGateStatus": pr.GATE.get("Status"),
           "CloneHeadAfterSelftest": head, "CloneCleanAfterSelftest": status.strip() == "" and head == pr.REV,
           "RunFilesMatchKit": run_match, "RunDirListingAfterSelftest": run_listing,
           "Cases": cases,
           "Totals": {"Cases": len(cases), "Pass": sum(1 for c in cases if c["Result"] == "PASS"),
                      "AuditCases": len(audit_rows), "AuditPass": sum(1 for c in audit_rows if c["Result"] == "PASS"),
                      "GateCases": len(gate_rows), "GatePass": sum(1 for c in gate_rows if c["Result"] == "PASS")},
           "AllPass": all(c["Result"] == "PASS" for c in cases) and status.strip() == "" and head == pr.REV and all(run_match.values()) and
           run_listing == sorted(pr.RUN_FILES)}
    text = json.dumps(doc, ensure_ascii=False, indent=1)
    text = re.sub(r'(?i)[a-z]:(?:\\\\|/)+users(?:\\\\|/)+[^\\\\/"\s]+', "%USERPROFILE%", text)
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text + "\n")
    for c in cases:
        print("%-50s %s %s" % (c["Case"][:50], c["Result"], ("missing=%s unexpected=%s bad=%s sv=%s coh=%s" % (
            c.get("Missing"), c.get("Unexpected"), c.get("BadCounts"), c.get("SchemaValid"), c.get("ResultCoherence"))) if c["Result"] != "PASS" else ""))
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


if __name__ == "__main__":
    sys.exit(main(sys.argv[1]))
