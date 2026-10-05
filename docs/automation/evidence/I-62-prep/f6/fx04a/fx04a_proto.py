"""I-62 F6 preparation: FX-04a (resume and decide, Proposal V14 D.3) as a ONE-COMMAND mechanical rehearsal, without opening any session of the system
under test (that needs OD-5) and without invoking any model.

What it proves now: Principal A → durable state (Git only) → clean host B (a `--no-local` clone, no transient artifacts, no transcript) → mechanical
reconstruction of the facts and of the next decision → field-by-field comparison with an oracle whose SHA-256 is fixed BEFORE B starts. N11: a clone
where one fact (a correction launch) is removed from the canonical state must make B declare UNKNOWN for it, never the hidden value.

In F6 the reconstructor is replaced by the Principal B session (claude-desktop-session or codex-desktop-session); the fixture, the oracle, the hash
discipline and the comparator stay as they are. Usage: python fx04a_proto.py <out.json>
"""
import copy
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "f4", "yaml-subset"))
sys.path.insert(0, os.path.join(HERE, "..", "..", "f4", "oracle"))
import yaml_subset as Y  # noqa: E402
import state_oracle as O  # noqa: E402

OUT = sys.argv[1]
ROOT = tempfile.mkdtemp(prefix="r62-fx04a-")
DATE = "2026-10-04T12:00:00+00:00"
ENV = {**os.environ, "GIT_AUTHOR_NAME": "fixture", "GIT_AUTHOR_EMAIL": "fixture@example.invalid", "GIT_COMMITTER_NAME": "fixture",
       "GIT_COMMITTER_EMAIL": "fixture@example.invalid", "GIT_AUTHOR_DATE": DATE, "GIT_COMMITTER_DATE": DATE}
FX_CID = "f0f0f0f0-0000-4000-8000-0000000000u1".replace("u", "0")


def git(cwd, *a, check=True):
    r = subprocess.run(["git", "-C", cwd] + list(a), capture_output=True, text=True, encoding="utf-8", errors="replace", env=ENV)
    if check and r.returncode != 0:
        raise RuntimeError("git %s: %s" % (" ".join(a), r.stderr))
    return r.stdout.strip()


def put(cwd, path, text):
    full = os.path.join(cwd, path)
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    git(cwd, "add", path)


def commit(cwd, msg):
    git(cwd, "commit", "-q", "-m", msg)
    return git(cwd, "rev-parse", "HEAD")


# ------------------------------------------------------------------ fixture (D.1, minimal): bare origin, F_seed, F_norm → F_eff, FX-U1
origin = os.path.join(ROOT, "fixture-origin.git")
A = os.path.join(ROOT, "A")
subprocess.run(["git", "init", "-q", "--bare", "-b", "main", origin], check=True, env=ENV)
subprocess.run(["git", "clone", "-q", origin, A], check=True, env=ENV, capture_output=True)
git(A, "config", "core.autocrlf", "false")
put(A, "AGENTS.md", "# AGENTS (FIXTURE_LOCAL)\n\nJobs requeridos del fixture: fixture-build, fixture-tests.\n")
put(A, "FIXTURE-MANIFEST.json", json.dumps({"AGENTS.md": {"Kind": "FIXTURE_LOCAL", "Reason": "jobs del fixture"}, "TEST-ACTIVATION": True}, indent=1) + "\n")
f_seed = commit(A, "F_seed")
git(A, "checkout", "-q", "-b", "fixture/i62-norm")
put(A, "docs/norm.md", "textos I62 del fixture\n")
f_norm = commit(A, "F_norm\n\nAgent-Protocol-Normative: I-62")
git(A, "checkout", "-q", "main")
git(A, "merge", "-q", "--no-ff", "-m", "F_eff (TEST-ACTIVATION)", f_norm)
f_eff = git(A, "rev-parse", "HEAD")
git(A, "push", "-q", "origin", "main")
git(A, "checkout", "-q", "-b", "fx/u1")
git(A, "commit", "-q", "--allow-empty", "-m", "claim FX-U1\n\nClaim-Id: " + FX_CID)
claim = git(A, "rev-parse", "HEAD")


def ref(name):
    return {"path": "docs/automation/evidence/FX-U1-agent/" + name, "blob": "b" * 40}


state = {"schema": "rackcad-automation-state/v2",
         "automation_state": {"initiative": "FX-U1", "branch": "fx/u1", "claim_id": FX_CID, "current_phase": "T2", "state": "waiting", "gate": "none",
                              "attempts": 1, "next_action": "QR y Q0 de la ventana 2 para planificar T2", "last_evidence_commit": f_eff},
         "protocol": {"set": "rackcad-protocol/I62", "effective_sha": f_eff,
                      "basis": {"claim_id": FX_CID, "claim_commit": claim, "claim_parent_sha": f_eff, "claim_parent_contains_effective": True, "adoption_at": "G0"},
                      "g0_acceptance": {"state": "ACCEPTED", "decision": ref("decisions.md")}},
         "custody": {"record_version": 9, "point": "QH", "point_kind": None, "last_rebase": None,
                     "principal": {"state": "RELEASED", "binding": ref("principal-A.json"), "acceptance": {"state": "ACCEPTED", "decision": ref("decisions.md")},
                                   "preflight": ref("preflight-A.json"), "designation": None, "since_record_version": 1},
                     "window": {"seq": 1, "state": "CLOSED"},
                     "task_intent": {"task_id": "T2", "attempt": 1, "kind": "FIRST", "contract": ref("T2/contract.json"), "continues_task_id": None,
                                     "planned_roles": [{"role": "EXECUTION_CONTROLLER", "binding": ref("ctrl.json")}]},
                     "last_window": {"seq": 1, "task_id": "T1", "attempt": 1, "closure": "VERIFIED", "closure_source": "CONTROLLER", "delegation_run_id": "R-T1",
                                     "verification": ref("T1/verification.json"), "verified_sha": f_eff, "journal": ref("T1/journal.json"), "reconstruction": None},
                     "chains": [{"task_id": "T1", "state": "VERIFIED", "chain_base_sha": f_eff, "chain_red_sha": f_eff, "chain_red_files": ["tests/t1.txt"],
                                 "continues_task_id": None, "correction_authorized_pending": False}],
                     "unverified_commits": []},
         "counters": {"correction_launches": [{"seq": 1, "task_id": "T1", "failure_class": "Tests", "correction_of_run_id": "R-T1a", "attempts_after": 1,
                                               "record_version": 5}],
                      "blocked_reruns": [], "rebase_recoveries": [], "invocations": [{"scope": "F6-A", "launched": 4, "uncertain": 0, "cap": 15, "cap_source": ref("cap.json"),
                                                                                      "reconstructed": False, "reconstruction": None}]},
         "orchestration": {"loop": {"type": "NONE", "phase": "NONE", "object": None, "authorization": None, "action_validity": None},
                           "next_action": {"role": "PRINCIPAL_COORDINATOR", "action": "CUSTODY"}, "review_requests": [], "findings": [],
                           "budgets": {"review_rounds": 0, "logical_requests": 0, "architect_launches": 0, "transport_reruns": [], "correction_rounds": 0,
                                       "corrections_by_lineage": [], "caps": ref("caps.json")},
                           "escalation": {"state": "NONE", "reason": None, "required_decision": None}, "autonomy_gaps": []}}
file_violations = O.validate_file(state)
put(A, "docs/automation/state/FX-U1.yml", Y.dumps(state))
qh = commit(A, "QH record_version 9 (A libera; T2 planificada)")
git(A, "push", "-q", "origin", "fx/u1")
# A writes a transient artifact on ITS host; B must never read it
put(A, ".agent-runs/FX-U1/transient-note.txt", "nota privada de A\n")


# ------------------------------------------------------------------ the oracle (Coordinator, outside the host), hash fixed before B starts
def facts_and_decision(s):
    def known(path, default="UNKNOWN"):
        v = O.get(s, path, default)
        return v
    facts = {"branch": known("automation_state.branch"), "claim_id": known("automation_state.claim_id"), "last_point": known("custody.point"),
             "record_version": known("custody.record_version"), "protocol": known("protocol.set"), "principal_state": known("custody.principal.state"),
             "attempts": known("automation_state.attempts"),
             "correction_launches": [(e["seq"], e["task_id"], e["failure_class"]) for e in (O.get(s, "counters.correction_launches") or [])],
             "invocations": [(e["scope"], e["launched"], e["uncertain"]) for e in (O.get(s, "counters.invocations") or [])],
             "chains": [(c["task_id"], c["state"], c["chain_red_sha"]) for c in (O.get(s, "custody.chains") or [])],
             "last_window": (O.get(s, "custody.last_window.closure"), O.get(s, "custody.last_window.verified_sha")),
             "task_intent": (O.get(s, "custody.task_intent.task_id"), O.get(s, "custody.task_intent.kind"), O.get(s, "custody.task_intent.attempt"))}
    ok_qh = facts["last_point"] == "QH" and facts["principal_state"] == "RELEASED"
    decision = {"action": "QR (CAS) → Q0 ventana %d → CONTROLLER_PLANNING" % ((O.get(s, "custody.window.seq") or 0) + 1) if ok_qh else "STOP",
                "task_id": facts["task_intent"][0], "attempt": facts["task_intent"][2], "role": "EXECUTION_CONTROLLER", "protocol_set": "rackcad-protocol/I62",
                "binding": "REUSE si los invalidadores del PreflightRef no cambian; si no, re-observar y REBIND", "worker": None}
    return {"facts": facts, "decision": decision}


oracle = facts_and_decision(state)
oracle_sha = hashlib.sha256(json.dumps(oracle, sort_keys=True).encode()).hexdigest()


# ------------------------------------------------------------------ B: clean host, only Git (D.6), mechanical reconstruction
def reconstruct(clone_dir, hidden=None):
    text = git(clone_dir, "show", "HEAD:docs/automation/state/FX-U1.yml")
    s = Y.loads(text)
    if hidden:
        # N11: the fact is removed from the canonical state that B reads; B must not invent it
        s["counters"]["correction_launches"] = []
    fd = facts_and_decision(s)
    if hidden and fd["facts"]["attempts"] != len(fd["facts"]["correction_launches"]):
        # attempts = 1 but no CorrectionLaunch explains it: B declares the fact UNKNOWN instead of guessing (I-S05 cannot be established)
        fd["facts"]["correction_launches"] = "UNKNOWN"
    reads = ["HEAD:docs/automation/state/FX-U1.yml"]
    return fd, reads


B = os.path.join(ROOT, "B")
subprocess.run(["git", "clone", "-q", "--no-local", "-b", "fx/u1", origin, B], check=True, env=ENV, capture_output=True)
transient_visible = os.path.exists(os.path.join(B, ".agent-runs"))
resp, reads = reconstruct(B)
resp_sha = hashlib.sha256(json.dumps(resp, sort_keys=True).encode()).hexdigest()   # published BEFORE the oracle is handed over


def compare(o, r):
    diffs = []
    for k in o["facts"]:
        if o["facts"][k] != r["facts"].get(k):
            diffs.append(("facts." + k, o["facts"][k], r["facts"].get(k)))
    for k in o["decision"]:
        if o["decision"][k] != r["decision"].get(k):
            diffs.append(("decision." + k, o["decision"][k], r["decision"].get(k)))
    return diffs


diffs = compare(oracle, resp)
B2 = os.path.join(ROOT, "B2")
subprocess.run(["git", "clone", "-q", "--no-local", "-b", "fx/u1", origin, B2], check=True, env=ENV, capture_output=True)
resp2, _ = reconstruct(B2, hidden="correction_launches")
n11 = {"DeclaredUnknown": resp2["facts"]["correction_launches"] == "UNKNOWN", "HiddenValueNotInvented": resp2["facts"]["correction_launches"] != oracle["facts"]["correction_launches"]}
result = {"Fixture": {"F_seed": f_seed, "F_norm": f_norm, "F_eff": f_eff, "QH": qh},
          "StateAtQH": {"FileInvariants": file_violations or "VALID"},
          "OracleSha256": oracle_sha, "ResponseSha256": resp_sha, "ResponsePublishedBeforeOracle": True,
          "Isolation": {"TransientArtifactVisibleToB": transient_visible, "ReadsByB": reads},
          "Comparison": {"Diffs": diffs, "Result": "PASS" if not diffs and not transient_visible else "FAIL"},
          "N11": n11}
result["AllPass"] = (not file_violations) and result["Comparison"]["Result"] == "PASS" and all(n11.values())
json.dump(result, open(OUT, "w", encoding="utf-8", newline="\n"), ensure_ascii=False, indent=1, default=str)
shutil.rmtree(ROOT, ignore_errors=True)
print(json.dumps({"comparison": result["Comparison"]["Result"], "n11": n11, "isolation": result["Isolation"]["TransientArtifactVisibleToB"],
                  "state": result["StateAtQH"], "all": result["AllPass"]}, ensure_ascii=False))
sys.exit(0 if result["AllPass"] else 1)
