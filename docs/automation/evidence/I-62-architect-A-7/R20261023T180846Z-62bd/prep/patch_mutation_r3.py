"""Deriva kit/mutation-post-review.py de la ronda 3 (decisiones §96) del de la ronda 2: solo el docstring y el texto del campo Mutation; el arnés
(sitios, mutantes, memos, MemoFid, juicio de fidelity.py) no cambia. Uso: python -I -B patch_mutation_r3.py <kit> <prefijo del SHA-256 de partida>
"""
import hashlib
import os
import sys

p = os.path.join(sys.argv[1], "mutation-post-review.py")
t = open(p, encoding="utf-8", newline="").read()
assert hashlib.sha256(t.encode("utf-8")).hexdigest().startswith(sys.argv[2]), "mutation-post-review.py de partida (el de la ronda 2)"
R = [
    ('"""Mutation test of the auditor v6.0-a7r2 (post-review.py), of transport_gate.py (unchanged from A-6 round 4 and round 3: the D-01 gate and the\n'
     'explicit Windows decoding of decisions §84, point 5), of owner_gate.py (A-7 round 2: Owner line of §78, CLAUDE-CLI-I62, the round-2 A-7 and agreed\n'
     'A-6 blobs at the worktree HEAD and a Coordinator authorization after §94, pinned with its number, heading, blob and text SHA-256 and no later\n'
     'section; delivered PENDING, so the audit cases pin a synthetic §95) and of fidelity.py, for the ONE formal review of ROUND 2 of amendment A-7\n'
     '(decisions §94, point 6): «cada regla del auditor tiene un caso que falla sin ella», checked mechanically. The harness of the A-7 round-1 kit\n'
     '(R20261019T010200Z-4a72, with the read-fidelity extension of phase 1), unchanged except its names; it honours RACKCAD_A7_KITDATA like the self-test\n'
     '(by default, the real clone and run of the kit). This is the FULL battery (every site of the four modules), run once at the end of the kit\'s\n'
     'preparation (decisions §88, point 4: auditor rules and launcher check 7 changed).\n',
     '"""Mutation test of the auditor v7.0-a7r3 (post-review.py), of transport_gate.py (unchanged from A-6 round 4 and round 3: the D-01 gate and the\n'
     'explicit Windows decoding of decisions §84, point 5), of owner_gate.py (A-7 round 3: Owner line of §78, CLAUDE-CLI-I62, the round-3 A-7 and agreed\n'
     'A-6 blobs at the worktree HEAD and a Coordinator authorization after §96, pinned with its number, heading, blob and text SHA-256 and no later\n'
     'section; delivered PENDING, so the audit cases pin a synthetic §97) and of fidelity.py, for the ONE formal review of ROUND 3 of amendment A-7\n'
     '(decisions §96, points 6 and 7): «cada regla del auditor tiene un caso que falla sin ella», checked mechanically. The harness of the A-7 round-2\n'
     'kit (R20261019T201030Z-d1bf, itself the round-1 harness with the read-fidelity extension of phase 1), unchanged except its names; it honours\n'
     'RACKCAD_A7_KITDATA like the self-test (by default, the real clone and run of the kit). This is the FULL battery (every site of the four modules),\n'
     'run once at the end of the kit\'s preparation (decisions §88, point 4: the auditor, v7.0-a7r3 with the reference-exactness rule, and the result\n'
     'schema changed).\n'),
    ('    doc = {"Mutation": "post-review.py v6.0-a7r2 + transport_gate.py + owner_gate.py (A-7 ronda 2) + fidelity.py (decisiones §94, punto 5): cada sitio de "\n',
     '    doc = {"Mutation": "post-review.py v7.0-a7r3 + transport_gate.py + owner_gate.py (A-7 ronda 3) + fidelity.py (decisiones §94, punto 5): cada sitio de "\n'),
]
for a, b in R:
    assert t.count(a) == 1, ("no único", t.count(a), a[:100])
    t = t.replace(a, b)
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest())
