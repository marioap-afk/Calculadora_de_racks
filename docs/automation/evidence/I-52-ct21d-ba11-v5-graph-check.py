"""Mechanical check of the BA-11 dependency graph (decisions section 217, AR4-17). Informative evidence, outside the agreed text.

Usage: python I-52-ct21d-ba11-v5-graph-check.py <repo root> <registry md, repository-relative> <output json>
Parses the complete edge list of section 2.1 (`X -> Y`, X depends on Y) and the seal order, and checks:
  1. the graph has no cycle;
  2. the seal order is a topological order (every dependency sealed in an earlier tier), BA-11 last;
  3. the DEPENDS_ON cells of the section 3 table equal the edge list;
  4. each artifact's own header DEPENDS_ON line (first .md path of its entry) names exactly the entries of its cell; an artifact
     version written before the header convention (no DEPENDS_ON line, e.g. BA-02a V3) is listed as not declared, not failed;
  5. the forbidden edges BA-05 -> BA-08, BA-07 -> BA-05, BA-10 -> BA-04 and BA-04 -> BA-02b are absent.
Exit code 0 only if every check holds."""
import io
import json
import os
import re
import sys

root, reg, out = sys.argv[1], sys.argv[2], sys.argv[3]
md = io.open(os.path.join(root, *reg.split('/')), encoding='utf-8').read().replace('\r\n', '\n')
sec = md[md.index('### 2.1'):md.index('## 3. Registry entries')]
blocks = re.findall(r'```text\n(.*?)```', sec, re.S)
edges = [tuple(x.strip() for x in line.split('->')) for line in blocks[0].strip().split('\n')]
tiers = [[x.strip() for x in t.split(',')] for t in blocks[1].strip().split('->')]
tiers[-1] = [x.replace('(last)', '').strip() for x in tiers[-1]]
deps = {}
for x, y in edges:
    deps.setdefault(x, []).append(y)
pos = {x: i for i, t in enumerate(tiers) for x in t}
res = {}
color, cycles = {}, []


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
res['no_cycle'] = not cycles
res['cycles'] = cycles
bad = [(x, y) for x, y in edges if x != 'BA-11' and not (x in pos and y in pos and pos[y] < pos[x])]
res['seal_order_topological'] = not bad and tiers[-1] == ['BA-11']
res['seal_order_violations'] = bad
table = md[md.index('## 3. Registry entries'):md.index('### 3.1')]
cells, paths = {}, {}
for line in table.split('\n'):
    if line.startswith('| BA-') and not line.startswith('| BA-11') and not line.startswith('| BA-01 '):
        c = [x.strip() for x in line.split('|')]
        cells[c[1]] = sorted(x.strip() for x in c[10].split(',')) if c[10] != 'none' else []
        paths[c[1]] = re.findall(r'`([^`]+)`', c[3])
edge_map = {k: sorted(v) for k, v in deps.items() if k != 'BA-11'}
res['table_matches_edges'] = edge_map == {k: v for k, v in cells.items() if v}
res['table_cells'] = cells
hdr = {}
for k, ps in paths.items():
    mdp = [p for p in ps if p.endswith('.md')][0]
    txt = io.open(os.path.join(root, *mdp.split('/')), encoding='utf-8').read().replace('\r\n', '\n')
    m = re.search(r'^>?\s*DEPENDS_ON\s*=\s*(.*)$', txt, re.M)
    if not m:
        hdr[k] = 'NO_HEADER_LINE'
        continue
    val = m.group(1).split('(')[0]
    hdr[k] = sorted(set(re.findall(r'BA-\d\d[ab]?', val)))
res['header_lines'] = hdr
res['headers_not_declared'] = sorted(k for k in cells if hdr[k] == 'NO_HEADER_LINE')
res['headers_match_cells'] = all(hdr[k] == cells[k] for k in cells if hdr[k] != 'NO_HEADER_LINE')
res['header_mismatches'] = {k: {'header': hdr[k], 'cell': cells[k]} for k in cells if hdr[k] not in ('NO_HEADER_LINE', cells[k])}
forbidden = [('BA-05', 'BA-08'), ('BA-07', 'BA-05'), ('BA-10', 'BA-04'), ('BA-04', 'BA-02b')]
res['forbidden_edges_absent'] = not any(e in edges for e in forbidden)
res['edges'] = ['%s -> %s' % e for e in edges]
res['seal_order_tiers'] = tiers
res['ALL_PASS'] = all(res[k] for k in ('no_cycle', 'seal_order_topological', 'table_matches_edges', 'headers_match_cells', 'forbidden_edges_absent'))
io.open(out, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, indent=1) + '\n')
print('ALL_PASS' if res['ALL_PASS'] else 'FAIL', json.dumps({k: res[k] for k in ('no_cycle', 'seal_order_topological', 'table_matches_edges', 'headers_match_cells', 'forbidden_edges_absent')}))
sys.exit(0 if res['ALL_PASS'] else 1)
