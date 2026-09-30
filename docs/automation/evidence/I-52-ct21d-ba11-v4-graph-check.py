"""Mechanical check of the BA-11 V4 dependency graph (decisions section 214, AR3-25).

Reads docs/initiatives/I-52-ct21d-baseline-ba-11-hash-registry-v4.md, parses the complete edge list of section 2.1
(`X -> Y`, X depends on Y) and the seal order, and checks:
  1. the graph has no cycle;
  2. the seal order is a topological order (every dependency sealed in an earlier tier), BA-11 last;
  3. the DEPENDS_ON cells of the section 3 table equal the edge list;
  4. BA-05 has no edge to BA-08, BA-07 has none to BA-05, BA-10 has none to BA-04 (AR3-17, AR3-23, AR3-24).
Usage: python I-52-ct21d-ba11-v4-graph-check.py <repo root> <output json>
Exit code 0 only if every check holds."""
import io
import json
import os
import re
import sys

root, out = sys.argv[1], sys.argv[2]
md = io.open(os.path.join(root, 'docs', 'initiatives', 'I-52-ct21d-baseline-ba-11-hash-registry-v4.md'), encoding='utf-8').read().replace('\r\n', '\n')
sec = md[md.index('### 2.1'):md.index('## 3. Registry entries')]
blocks = re.findall(r'```text\n(.*?)```', sec, re.S)
edges = [tuple(x.strip() for x in line.split('->')) for line in blocks[0].strip().split('\n')]
order_line = blocks[1].strip()
tiers = [[x.strip() for x in t.split(',')] for t in order_line.split('->')]
tiers[-1] = [x.replace('(last)', '').strip() for x in tiers[-1]]
deps = {}
for x, y in edges:
    deps.setdefault(x, []).append(y)
pos = {x: i for i, t in enumerate(tiers) for x in t}
results = {}
# 1. cycles
color = {}
cycles = []


def dfs(n, path):
    color[n] = 1
    for m in deps.get(n, []):
        if color.get(m) == 1:
            cycles.append(path + [m])
        elif not color.get(m):
            dfs(m, path + [m])
    color[n] = 2


for n in sorted(deps):
    if not color.get(n):
        dfs(n, [n])
results['no_cycle'] = not cycles
results['cycles'] = cycles
# 2. topological seal order
bad = [(x, y) for x, y in edges if x != 'BA-11' and not (x in pos and y in pos and pos[y] < pos[x])]
results['seal_order_topological'] = not bad and tiers[-1] == ['BA-11']
results['seal_order_violations'] = bad
# 3. table cells
table = md[md.index('## 3. Registry entries'):md.index('### 3.1')]
cells = {}
for line in table.split('\n'):
    if line.startswith('| BA-') and not line.startswith('| BA-11'):
        c = [x.strip() for x in line.split('|')]
        cells[c[1]] = [] if c[10] == 'none' else [x.strip() for x in c[10].split(',')]
edge_map = {k: sorted(v) for k, v in deps.items() if k != 'BA-11'}
cell_map = {k: sorted(v) for k, v in cells.items() if v}
results['table_matches_edges'] = edge_map == cell_map
results['table_cells'] = cell_map
# 4. forbidden edges
forbidden = [('BA-05', 'BA-08'), ('BA-07', 'BA-05'), ('BA-10', 'BA-04')]
results['forbidden_edges_absent'] = not any(e in edges for e in forbidden)
results['edges'] = ['%s -> %s' % e for e in edges]
results['seal_order_tiers'] = tiers
results['ALL_PASS'] = all(results[k] for k in ('no_cycle', 'seal_order_topological', 'table_matches_edges', 'forbidden_edges_absent'))
io.open(out, 'w', encoding='utf-8', newline='\n').write(json.dumps(results, indent=1) + '\n')
print('ALL_PASS' if results['ALL_PASS'] else 'FAIL', json.dumps({k: results[k] for k in ('no_cycle', 'seal_order_topological', 'table_matches_edges', 'forbidden_edges_absent')}))
sys.exit(0 if results['ALL_PASS'] else 1)
