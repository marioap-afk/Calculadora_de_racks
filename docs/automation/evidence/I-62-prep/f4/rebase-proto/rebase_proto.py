"""I-62 F4 preparation: rebase out of window (§8.8, B.8.7) with real Git in a DISPOSABLE repository, in the LITERAL and the A1 variant.

Nothing touches the real branch: a bare «origin», a holder clone A, an intruder clone I (to test the lease) and a clean successor clone C, all under a
temporary directory. The state file is written with the canonical YAML writer of yaml-subset and read back in STRICT mode; the pairs and I-H02 are
checked with the oracle of ../oracle.

Steps:
  1. main M0; unit branch: claim, BOOTSTRAP, Proposal version X1 (object of an open review), Q7 with a verified chain; QU with an open review
     (loop.object = X1, request OPEN, attempt BUDGET_RESERVED with Target = X1);
  2. main advances (M1); A rebases; RebaseMap: Commits[] {Original, Image, PatchId} with equal patch-ids, StateFields for every live SHA;
  3. lease: the intruder pushes to the branch first → A's `--force-with-lease=<branch>:<branch_before>` is REJECTED (T10, STOP); A restores and the
     intruder commit is removed from the scenario; second attempt with a correct lease → accepted;
  4. QU REBASE_RECONCILIATION: LITERAL (only the literal StateFields) and A1 (+ live orchestration commits, attempt replanned) → oracle;
  5. clean successor clone C (fresh clone of origin, without the original commits): reads the state, checks I-H02 against its own history, recomputes the
     patch-ids of the images and checks that the live target commit exists.

Usage: python rebase_proto.py <out.json>
"""
import copy
import json
import os
import shutil
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "oracle"))
sys.path.insert(0, os.path.join(HERE, "..", "yaml-subset"))
import state_oracle as O  # noqa: E402
import yaml_subset as Y  # noqa: E402
from yaml_subset import dumps  # noqa: E402  (canonical writer)

OUT = sys.argv[1]
ROOT = tempfile.mkdtemp(prefix="r62-rebase-")
ENV = {**os.environ, "GIT_AUTHOR_NAME": "fx", "GIT_AUTHOR_EMAIL": "fx@example.invalid", "GIT_COMMITTER_NAME": "fx", "GIT_COMMITTER_EMAIL": "fx@example.invalid"}


def git(cwd, *args, check=True):
    r = subprocess.run(["git", "-C", cwd] + list(args), capture_output=True, text=True, encoding="utf-8", errors="replace", env=ENV)
    if check and r.returncode != 0:
        raise RuntimeError("git %s: %s" % (" ".join(args), r.stderr))
    return r


def commit(cwd, path, content, msg):
    full = os.path.join(cwd, path)
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, "w", encoding="utf-8", newline="\n") as f:
        f.write(content)
    git(cwd, "add", path)
    git(cwd, "commit", "-q", "-m", msg)
    return git(cwd, "rev-parse", "HEAD").stdout.strip()


def patch_id(cwd, sha):
    diff = git(cwd, "show", sha).stdout
    r = subprocess.run(["git", "-C", cwd, "patch-id", "--stable"], input=diff, capture_output=True, text=True, encoding="utf-8", env=ENV)
    return r.stdout.split()[0] if r.stdout else None


origin = os.path.join(ROOT, "origin.git")
A = os.path.join(ROOT, "A")
subprocess.run(["git", "init", "-q", "--bare", "-b", "main", origin], check=True, env=ENV)
subprocess.run(["git", "clone", "-q", origin, A], check=True, env=ENV, capture_output=True)
git(A, "config", "core.autocrlf", "false")
m0 = commit(A, "README.md", "fixture\n", "M0")
git(A, "push", "-q", "origin", "main")
git(A, "checkout", "-q", "-b", "unit")
claim = commit(A, "docs/claim.md", "claim\n", "claim I-99")
x1 = commit(A, "docs/initiatives/I-99-proposal-v1.md", "propuesta v1\n", "Proposal X1")
work = commit(A, "src/a.txt", "trabajo\n", "worker G (verificado)")
red = commit(A, "tests/t.txt", "red\n", "worker R")
SHA = lambda: git(A, "rev-parse", "HEAD").stdout.strip()


def ref(name):
    return {"path": "docs/automation/evidence/I-99-agent/" + name, "blob": "b" * 40}


def base_state(variant):
    return {"schema": "rackcad-automation-state/v2",
            "automation_state": {"initiative": "I-99", "branch": "unit", "claim_id": "11111111-2222-3333-4444-555555555555", "current_phase": "F1", "state": "implementing",
                                 "gate": "none", "attempts": 0, "next_action": "revisión del Architect", "last_evidence_commit": work},
            "protocol": {"set": "rackcad-protocol/I62", "effective_sha": m0,
                         "basis": {"claim_id": "11111111-2222-3333-4444-555555555555", "claim_commit": claim, "claim_parent_sha": m0,
                                   "claim_parent_contains_effective": True, "adoption_at": "G0"},
                         "g0_acceptance": {"state": "ACCEPTED", "decision": ref("decisions.md")}},
            "custody": {"record_version": 6, "point": "QU", "point_kind": "ORDINARY", "last_rebase": None,
                        "principal": {"state": "HELD", "binding": ref("binding.json"), "acceptance": {"state": "ACCEPTED", "decision": ref("decisions.md")},
                                      "preflight": ref("preflight.json"), "designation": None, "since_record_version": 1},
                        "window": {"seq": 1, "state": "CLOSED"}, "task_intent": None,
                        "last_window": {"seq": 1, "task_id": "T-01", "attempt": 0, "closure": "VERIFIED", "closure_source": "CONTROLLER", "delegation_run_id": "R-del-1",
                                        "verification": ref("verif.json"), "verified_sha": work, "journal": ref("journal.json"), "reconstruction": None},
                        "chains": [{"task_id": "T-01", "state": "VERIFIED", "chain_base_sha": x1, "chain_red_sha": red, "chain_red_files": ["tests/t.txt"],
                                    "continues_task_id": None, "correction_authorized_pending": False}],
                        "unverified_commits": []},
            "counters": {"correction_launches": [], "blocked_reruns": [], "rebase_recoveries": [], "invocations": []},
            "orchestration": {"loop": {"type": "ARCHITECT_REVIEW", "phase": "REVIEW_PENDING",
                                       "object": {"commit": x1, "path": "docs/initiatives/I-99-proposal-v1.md", "blob": "c" * 40}, "authorization": ref("decisions.md"),
                                       "action_validity": {"authorization_id": "RLA-1", "state": "OPEN", "ended_at": None, "ended_utc": None, "ended_reason": None,
                                                           "ended_by": None}},
                              "next_action": {"role": "PRINCIPAL_COORDINATOR", "action": "LAUNCH"},
                              "review_requests": [dict({"logical_review_request_id": "L1", "object": {"commit": x1, "path": "docs/initiatives/I-99-proposal-v1.md", "blob": "c" * 40},
                                                        "round": 1, "state": "OPEN",
                                                        "attempts": [{"attempt_seq": 1, "invocation": ref("inv-1.json"), "binding": ref("arch.json"), "state": "BUDGET_RESERVED",
                                                                      "reserved_at": 6, "run_id": None, "result": None, "ingested_at": None, "target_commit": x1}]},
                                                       **({"authorization_id": "RLA-1"} if variant == "A1" else {}))],
                              "findings": [],
                              "budgets": ([{"authorization_id": "RLA-1", "review_rounds": 1, "logical_requests": 1, "architect_launches": 1, "transport_reruns": [],
                                            "correction_rounds": 0, "corrections_by_lineage": [], "caps": ref("caps.json")}] if variant == "A1" else
                                          {"review_rounds": 1, "logical_requests": 1, "architect_launches": 1, "transport_reruns": [], "correction_rounds": 0,
                                           "corrections_by_lineage": [], "caps": ref("caps.json")}),
                              "escalation": {"state": "NONE", "reason": None, "required_decision": None}, "autonomy_gaps": []}}


result = {"TempRoot": ROOT, "Steps": {}}
p_lit = base_state("LITERAL")
state_commit = commit(A, "docs/automation/state/I-99.yml", dumps(p_lit), "QU record_version 6")
git(A, "push", "-q", "origin", "unit")
branch_before = SHA()
# main advances
git(A, "checkout", "-q", "main")
m1 = commit(A, "docs/other.md", "hermana\n", "M1 (avance de main)")
git(A, "push", "-q", "origin", "main")
git(A, "checkout", "-q", "unit")
originals = git(A, "rev-list", "--reverse", "main@{1}..unit" if False else m0 + "..unit").stdout.split()
pids_before = {c: patch_id(A, c) for c in originals}
git(A, "rebase", "-q", "main")
images = git(A, "rev-list", "--reverse", m1 + "..unit").stdout.split()
pids_after = {c: patch_id(A, c) for c in images}
commits = [{"Original": o, "Image": i, "PatchId": pids_before[o], "PatchIdEqual": pids_before[o] == pids_after[i]} for o, i in zip(originals, images)]
rmap = {c["Original"]: c["Image"] for c in commits}
result["Steps"]["rebase"] = {"Originals": len(originals), "Images": len(images), "AllPatchIdsEqual": all(c["PatchIdEqual"] for c in commits)}

# lease test: the intruder publishes first → A's lease is rejected
I = os.path.join(ROOT, "I")
subprocess.run(["git", "clone", "-q", "-b", "unit", origin, I], check=True, env=ENV, capture_output=True)
intr = commit(I, "docs/intruso.md", "x\n", "escritura ajena")
git(I, "push", "-q", "origin", "unit")
lease1 = git(A, "push", "--force-with-lease=unit:" + branch_before, "origin", "unit", check=False)
# the scenario restores origin/unit to branch_before (as a Coordinator decision would after T10), then the holder retries
git(I, "push", "-q", "--force", "origin", branch_before + ":unit")
lease2 = git(A, "push", "--force-with-lease=unit:" + branch_before, "origin", "unit", check=False)
result["Steps"]["lease"] = {"WithIntruder": "REJECTED" if lease1.returncode != 0 else "ACCEPTED", "Retry": "ACCEPTED" if lease2.returncode == 0 else "REJECTED"}
branch_after = SHA()
ancestors = set(git(A, "rev-list", "HEAD").stdout.split())

# reconciliation: what each variant may write
image_state_commit = rmap[state_commit]
p_lit = Y.loads(git(A, "show", "HEAD:docs/automation/state/I-99.yml").stdout)
p_a1 = base_state("A1")
last_rebase = {"map": ref("rebase/R-reb-1/rebase-map.json"), "main_before": m0, "main_after": m1, "branch_before": branch_before, "branch_after": branch_after,
               "record_version": 7}


def reconcile(p, variant):
    n = copy.deepcopy(p)
    n["custody"].update({"record_version": 7, "point": "QU", "point_kind": "REBASE_RECONCILIATION", "last_rebase": last_rebase})
    n["automation_state"]["last_evidence_commit"] = rmap[p["automation_state"]["last_evidence_commit"]]
    n["custody"]["last_window"]["verified_sha"] = rmap[p["custody"]["last_window"]["verified_sha"]]
    for c in n["custody"]["chains"]:
        c["chain_base_sha"], c["chain_red_sha"] = rmap[c["chain_base_sha"]], rmap[c["chain_red_sha"]]
    if variant == "A1":
        o = n["orchestration"]
        o["loop"]["object"]["commit"] = rmap[o["loop"]["object"]["commit"]]
        r = o["review_requests"][0]
        r["object"]["commit"] = rmap[r["object"]["commit"]]
        a = r["attempts"][0]
        a["target_commit"], a["invocation"] = rmap[a["target_commit"]], ref("inv-1-replanned.json")
    return n


variants = {}
for variant, p in (("LITERAL", p_lit), ("A1", p_a1)):
    n = reconcile(p, variant)
    pair = O.validate_pair(p, n, variant, {"rebase_map": rmap}) + O.validate_file(n, variant)
    hist = O.validate_history(n, ancestors, "A1")   # I-H02 as A-1 would extend it (live orchestration commits)
    hist_literal = O.validate_history(n, ancestors, "LITERAL")
    variants[variant] = {"PairAndFile": pair[:4] or "VALID", "IH02_Literal": hist_literal or "VALID", "IH02_A1": hist or "VALID",
                         "LiveTarget": n["orchestration"]["review_requests"][0]["attempts"][0]["target_commit"]}
    if variant == "LITERAL":
        # the literal validator also REJECTS reconciling the orchestration (I-P05/I-P13)
        forced = reconcile(p, "A1")
        variants[variant]["ReconcileOrchestration"] = (O.validate_pair(p, forced, "LITERAL", {"rebase_map": rmap}) + O.validate_file(forced, "LITERAL"))[:3]
    state_text = dumps(n)
    variants[variant]["StateRoundTrip"] = Y.loads(state_text) == n
    variants[variant]["_state_text"] = state_text
# publish the A1 reconciliation (the LITERAL one is analysed in memory) and reconstruct from a clean clone
commit(A, "docs/automation/state/I-99.yml", variants["A1"]["_state_text"], "QU REBASE_RECONCILIATION (A1)")
git(A, "push", "-q", "origin", "unit")
C = os.path.join(ROOT, "C")
# --no-local: a real transport, so that only reachable objects arrive (a local clone would copy the unreachable originals too)
subprocess.run(["git", "clone", "-q", "--no-local", "-b", "unit", origin, C], check=True, env=ENV, capture_output=True)
c_state = Y.loads(git(C, "show", "HEAD:docs/automation/state/I-99.yml").stdout)
c_anc = set(git(C, "rev-list", "HEAD").stdout.split())
succ = {"ReadsStateStrict": True, "IH02_A1": O.validate_history(c_state, c_anc, "A1") or "VALID",
        "OriginalTargetPresent": git(C, "cat-file", "-e", x1 + "^{commit}", check=False).returncode == 0,
        "ImageTargetPresent": git(C, "cat-file", "-e", rmap[x1] + "^{commit}", check=False).returncode == 0,
        "ImagePatchIdsRecomputed": all(patch_id(C, c["Image"]) == c["PatchId"] for c in commits)}
result["Steps"]["successor"] = succ
for v in variants.values():
    v.pop("_state_text")
result["Variants"] = variants
expect = {
    "rebase.AllPatchIdsEqual": result["Steps"]["rebase"]["AllPatchIdsEqual"],
    "lease rejected with an intruder": result["Steps"]["lease"]["WithIntruder"] == "REJECTED",
    "lease accepted on retry": result["Steps"]["lease"]["Retry"] == "ACCEPTED",
    "LITERAL reconciliation valid for the literal fields": variants["LITERAL"]["PairAndFile"] == "VALID",
    "LITERAL rejects reconciling the orchestration": bool(variants["LITERAL"]["ReconcileOrchestration"]),
    "LITERAL leaves a stale live target (A-1 I-H02 detects it)": variants["LITERAL"]["IH02_A1"] != "VALID" and variants["LITERAL"]["IH02_Literal"] == "VALID",
    "A1 reconciliation valid": variants["A1"]["PairAndFile"] == "VALID" and variants["A1"]["IH02_A1"] == "VALID",
    "successor: I-H02 valid under A1": succ["IH02_A1"] == "VALID",
    "successor: original target absent, image present": (not succ["OriginalTargetPresent"]) and succ["ImageTargetPresent"],
    "successor: patch-ids recomputed equal": succ["ImagePatchIdsRecomputed"],
}
result["Checks"] = expect
result["AllPass"] = all(expect.values())
json.dump(result, open(OUT, "w", encoding="utf-8", newline="\n"), ensure_ascii=False, indent=1)
shutil.rmtree(ROOT, ignore_errors=True)
print(json.dumps({"checks": expect, "all": result["AllPass"]}, ensure_ascii=False, indent=0))
sys.exit(0 if result["AllPass"] else 1)
