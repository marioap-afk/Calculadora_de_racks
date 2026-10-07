"""I-62 F6, FX-04a (C-25a; Proposal V14 D.3, D.4, D.6) — tool of the supervisor for the REAL run with a Principal B session.

The oracle and the field-by-field comparison are those of the measured prototype (`I-62-prep/f6/fx04a/fx04a_proto.py`); here they act on the real fixture:
  oracle   <fixture origin> <QH commit> <out dir>   → oracle.json (kept OUTSIDE every clone of B) and its SHA-256, to publish BEFORE B starts
  hash     <B response.json>                         → SHA-256 of B's response, to register BEFORE the oracle is handed over
  compare  <oracle.json> <B response.json>           → PASS only with total equality (isolation is judged separately, D.6)
  n11      <fixture origin> <QH commit> <B2 dir>     → B2's clean clone at QH with one correction_launches entry removed from the canonical state
B's response follows `response.schema.json` (same fields as the oracle; no value is given to B). Usage: python fx04a_real.py <command> ...
Revision 2026-10-07 (decisiones §51 punto 8, «hechos más pobres» sin FX-02): binding REBIND when the canonical state carries no Controller binding;
facts.stops_in_force and decision.preconditions in a closed vocabulary (compared as sets); N11 NOT_APPLICABLE without a correction_launches entry.
"""
import hashlib
import json
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.normpath(os.path.join(HERE, "..", "..", "..", "I-62-prep", "f4", "yaml-subset")))
import yaml_subset as Y  # noqa: E402

STATE = "docs/automation/state/FX-U1.yml"


def get(s, path, default=None):
    cur = s
    for part in path.split("."):
        if not isinstance(cur, dict) or part not in cur:
            return default
        cur = cur[part]
    return cur


def facts_and_decision(s):
    """The oracle of D.3 point 3 (port of the prototype): facts with exact equality and the next decision by its fields."""
    facts = {"branch": get(s, "automation_state.branch", "UNKNOWN"), "claim_id": get(s, "automation_state.claim_id", "UNKNOWN"),
             "last_point": get(s, "custody.point", "UNKNOWN"), "record_version": get(s, "custody.record_version", "UNKNOWN"),
             "protocol": get(s, "protocol.set", "UNKNOWN"), "principal_state": get(s, "custody.principal.state", "UNKNOWN"),
             "attempts": get(s, "automation_state.attempts", "UNKNOWN"),
             "correction_launches": [[e["seq"], e["task_id"], e["failure_class"]] for e in (get(s, "counters.correction_launches") or [])],
             "invocations": [[e["scope"], e["launched"], e["uncertain"]] for e in (get(s, "counters.invocations") or [])],
             "chains": [[c["task_id"], c["state"], c["chain_red_sha"]] for c in (get(s, "custody.chains") or [])],
             "last_window": [get(s, "custody.last_window.closure"), get(s, "custody.last_window.verified_sha")],
             "task_intent": [get(s, "custody.task_intent.task_id"), get(s, "custody.task_intent.kind"), get(s, "custody.task_intent.attempt")],
             # STOP vigentes (D.3 punto 3): the closed, deterministic set of STOP codes of the canonical next action at the point.
             "stops_in_force": sorted({m for c in (get(s, "orchestration.next_action.stop_conditions") or [])
                                       for m in re.findall(r"\b[PS]-[0-9]{2}\b", str(c))})}
    ok_qh = facts["last_point"] == "QH" and facts["principal_state"] == "RELEASED"
    next_points = ["QR", "Q0", "CONTROLLER_PLANNING"] if ok_qh else ["STOP"]
    # Controller binding (D.3 punto 3): REUSE only exists if the canonical state carries a Controller binding; with none (no FX-02), REBIND.
    planned = [r for r in (get(s, "custody.task_intent.planned_roles") or []) if isinstance(r, dict) and r.get("role") == "EXECUTION_CONTROLLER"]
    has_binding = any(r.get("binding") for r in planned)
    # Preconditions (D.3 puntos 3 y 7), closed vocabulary of response.schema.json, derived from the frozen T16 / P-10 rules for this point.
    pre = []
    if ok_qh:
        pre += ["COORDINATOR_DESIGNATION", "PREDECESSOR_TERMINATION_ACCREDITED", "CUSTODY_PREFLIGHT_MATCH", "MAIN_UNCHANGED_SINCE_EFFECTIVE"]
    if "CONTROLLER_PLANNING" in next_points:
        pre.append("CONTROLLER_BINDING_ACCEPTED")
    decision = {"next_points": next_points,
                "next_window_seq": (get(s, "custody.window.seq") or 0) + 1 if ok_qh else None,
                "task_id": facts["task_intent"][0], "attempt": facts["task_intent"][2], "role": "EXECUTION_CONTROLLER",
                "protocol_set": "rackcad-protocol/I62",
                "binding": "REUSE_IF_INVALIDATORS_UNCHANGED_ELSE_REBIND" if has_binding else "REBIND", "worker": None,
                "preconditions": sorted(pre)}
    return {"facts": facts, "decision": decision}


def sha(obj):
    return hashlib.sha256(json.dumps(obj, sort_keys=True, ensure_ascii=False).encode("utf-8")).hexdigest()


def state_at(origin, commit):
    text = subprocess.run(["git", "-C", origin, "show", "%s:%s" % (commit, STATE)], capture_output=True, check=True).stdout.decode("utf-8")
    return Y.loads(text)


SETS = {("facts", "stops_in_force"), ("decision", "preconditions")}


def compare(o, r):
    diffs = []
    for part in ("facts", "decision"):
        for k in o[part]:
            got = (r.get(part) or {}).get(k)
            if (part, k) in SETS and isinstance(got, list):
                got = sorted(set(got))
            if o[part][k] != got:
                diffs.append({"Field": part + "." + k, "Oracle": o[part][k], "Response": (r.get(part) or {}).get(k)})
    return diffs


def main():
    cmd = sys.argv[1]
    if cmd == "oracle":
        origin, commit, out = sys.argv[2:5]
        oracle = facts_and_decision(state_at(origin, commit))
        os.makedirs(out, exist_ok=True)
        with open(os.path.join(out, "oracle.json"), "w", encoding="utf-8", newline="\n") as f:
            json.dump(oracle, f, ensure_ascii=False, indent=1)
        print(json.dumps({"QH": commit, "OracleSha256": sha(oracle)}))
    elif cmd == "hash":
        print(json.dumps({"ResponseSha256": sha(json.load(open(sys.argv[2], encoding="utf-8")))}))
    elif cmd == "compare":
        o = json.load(open(sys.argv[2], encoding="utf-8"))
        r = json.load(open(sys.argv[3], encoding="utf-8"))
        diffs = compare(o, r)
        print(json.dumps({"Diffs": diffs, "Comparison": "PASS" if not diffs else "FAIL"}, ensure_ascii=False, indent=1))
    elif cmd == "n11":
        origin, commit, b2 = sys.argv[2:5]
        subprocess.run(["git", "clone", "-q", "--no-local", "-c", "core.autocrlf=false", origin, b2], check=True)
        subprocess.run(["git", "-C", b2, "checkout", "-q", "-b", "fx/u1-n11", commit], check=True)
        path = os.path.join(b2, STATE.replace("/", os.sep))
        s = Y.loads(open(path, encoding="utf-8").read())
        removed = (s.get("counters", {}).get("correction_launches") or [])[:1]
        if not removed:
            print(json.dumps({"N11": "NOT_APPLICABLE", "Reason": "no correction launch exists in this poorer-facts starting state (counters.correction_launches empty at QH); no entry is fabricated to remove"}, ensure_ascii=False))
            return
        s["counters"]["correction_launches"] = s["counters"]["correction_launches"][1:]
        open(path, "w", encoding="utf-8", newline="\n").write(Y.dumps(s))
        subprocess.run(["git", "-C", b2, "commit", "-q", "-am", "N11: hecho retirado del estado canónico (clon de B2, no se publica)"], check=True)
        print(json.dumps({"B2": b2, "Removed": removed}, ensure_ascii=False))
    else:
        raise SystemExit("unknown command " + cmd)


if __name__ == "__main__":
    main()
