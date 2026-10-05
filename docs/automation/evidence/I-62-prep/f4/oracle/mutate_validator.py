"""Mutation of the oracle itself: each mutation disables one invariant check of state_oracle.py (its `a(...)` call becomes a no-op call with the
same arguments); the transition suite (test_oracle.py) must then fail.

Usage: python mutate_validator.py <out.json>
"""
import json
import os
import shutil
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = open(os.path.join(HERE, "state_oracle.py"), encoding="utf-8").read()
MUTATIONS = {
    "I-P01 record_version": 'a("I-P01", get(n, "custody.record_version")',
    "I-P02 transiciones": 'a("I-P02", np_ in I_P02.get(pp, set())',
    "I-P04 window.seq": 'a("I-P04", get(n, "custody.window.seq")',
    "I-P06 inmutables": 'a("I-P06", get(p, path) == get(n, path)',
    "I-P09 marcadores de G0": 'a("I-P09", {"I62-CLASSIFICATION: I62"',
    "I-P10 imágenes": 'a("I-P10", o_ == n_ or rmap.get(o_) == n_',
    "I-S08 ABANDONED": 'a("I-S08", (lw["closure"] == "ABANDONED")',
    "I-S15 Q0 con aceptaciones": 'a("I-S15", get(s, "protocol.g0_acceptance.state") == "ACCEPTED"',
    "I-P13 loop.object": 'a("I-P13", ok, "loop.object cambia',
    "A1-D1-5 LOOP_CLOSED": 'a("A1-D1-5", not any(r["state"] == "OPEN"',
    "A1-D1-4 reutilización": 'a("A1-D1-4", (ln.get("action_validity") or {}).get("authorization_id") not in',
}
res = {}
for name, head in MUTATIONS.items():
    assert SRC.count(head) == 1, name
    mutated = SRC.replace(head, "(lambda *args: None)(" + head[2:])
    tmp = tempfile.mkdtemp()
    open(os.path.join(tmp, "state_oracle.py"), "w", encoding="utf-8").write(mutated)
    shutil.copy(os.path.join(HERE, "test_oracle.py"), tmp)
    r = subprocess.run([sys.executable, os.path.join(tmp, "test_oracle.py"), os.path.join(tmp, "r.json")], capture_output=True, text=True, encoding="utf-8",
                       errors="replace", env={**os.environ, "PYTHONIOENCODING": "utf-8"})
    failing = [l.split(" LITERAL")[0].split(" A1 ")[0][5:] for l in r.stdout.splitlines() if l.startswith("FAIL")]
    res[name] = {"SuiteExit": r.returncode, "Detected": r.returncode == 1, "FailingTransitions": failing[:5],
                 "Crash": (r.stderr or "")[-300:] if r.returncode not in (0, 1) else ""}
    shutil.rmtree(tmp)
json.dump(res, open(sys.argv[1], "w", encoding="utf-8", newline="\n"), ensure_ascii=False, indent=1)
print(json.dumps({k: v["Detected"] for k, v in res.items()}, ensure_ascii=False))
sys.exit(0 if all(v["Detected"] for v in res.values()) else 1)
