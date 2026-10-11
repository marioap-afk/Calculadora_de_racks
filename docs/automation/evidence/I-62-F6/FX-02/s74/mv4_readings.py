"""Read-only: counts MV-4 'mismatches' under three readings on the fixture (git ls-tree/show only)."""
import json
import subprocess
import sys

F = sys.argv[1]
EFF = 'fbe25347799e5b801ef708454212335537448bd1'
E1 = '930288c51ab1b5dbd370c8ebf9206d78670b89a0'
MP = 'docs/automation/agent-execution/compatibility/I62-clause-map.json'
SP = 'docs/automation/agent-execution/compatibility/clause-map.schema.json'


def git(*a):
    return subprocess.run(['git', '-C', F] + list(a), capture_output=True, check=True).stdout


def tree(rev):
    out = {}
    for ln in git('ls-tree', '-r', '--full-tree', rev).decode('utf-8').splitlines():
        meta, p = ln.split('\t', 1)
        out[p] = meta.split()[2]
    return out


t1, t2 = tree(E1), tree(EFF)
M = json.loads(git('show', EFF + ':' + MP).decode('utf-8'))
surf = M['Surfaces']
under = lambda p: any(p == s or (s.endswith('/') and p.startswith(s)) for s in surf)
changed = sorted(p for p in set(t1) | set(t2) if under(p) and t1.get(p) != t2.get(p) and p != MP)
A, B, C = [], [], []
for x in M['Files']:
    p, k, bb, eb = x['Path'], x['FileKind'], x['BaseBlob'], x['EffBlob']
    o1, o2 = t1.get(p), t2.get(p)
    if k == 'MODIFIED':
        okA = bb == o1 and eb == o2 and o1 is not None and o2 is not None
    else:
        okA = bb is None and eb == o2 and o2 is not None and (k != 'ENTRY' or p == SP)
    # B: model C35 reading: BaseBlob must equal the observed base (null = absent) for every kind
    okB = bb == (o1 if k == 'MODIFIED' else None) and eb == o2 and ((o1 is not None) if k == 'MODIFIED' else (o1 is None))
    # C: clause_map.py: the row equals the derived row (derivation = changed paths only)
    if p in changed and o2 is not None:
        dk = 'ENTRY' if p == SP else ('ADDED' if o1 is None else 'MODIFIED')
        okC = (k, bb, eb) == (dk, o1 if dk == 'MODIFIED' else None, o2)
    else:
        okC = False
    if not okA: A.append(p)
    if not okB: B.append(p)
    if not okC: C.append(p)
print(json.dumps({'changed_under_surfaces': changed, 'files': len(M['Files']),
                  'A_literal': len(A), 'B_observed_base': len(B), 'C_derived_row': len(C),
                  'B_minus_A': sorted(set(B) - set(A)), 'A_list': A,
                  'B_minus_A_kinds': sorted({x['FileKind'] for x in M['Files'] if x['Path'] in set(B) - set(A)})}, ensure_ascii=True, indent=1))
