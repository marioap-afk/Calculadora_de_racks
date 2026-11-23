"""Solo lectura: busca en los registros de la app de Codex los nombres de canalizacion codex-computer-use-<GUID> y los compara
con el de config.toml, solo por SHA-256 (nunca imprime el GUID). Uso: python -I opaque_logs.py <carpeta-de-logs>"""
import glob, hashlib, os, re, sys, tomllib
U = os.environ['USERPROFILE']
cfg = tomllib.loads(open(os.path.join(U, '.codex', 'config.toml'), encoding='utf-8').read())
cur = cfg['mcp_servers']['node_repl']['env']['SKY_CUA_NATIVE_PIPE_DIRECTORY']
G = re.compile(r'codex-computer-use-([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})')
m = G.search(cur)
assert m, 'el valor actual no tiene la forma codex-computer-use-<GUID>'
h = lambda s: hashlib.sha256(s.lower().encode()).hexdigest()[:12].upper()
curh = h(m.group(1))
print('config.toml: forma', G.sub('codex-computer-use-<GUID>', cur.replace(chr(92), '/')), 'guid-sha', curh, 'pipe-vivo', os.path.exists(cur))
root = sys.argv[1]
for day in ('2026/11/21', '2026/11/22', '2026/11/23'):
    for f in sorted(glob.glob(os.path.join(root, *day.split('/'), '*.log'))):
        txt = open(f, encoding='utf-8', errors='replace').read()
        found = {}
        for mm in G.finditer(txt):
            found.setdefault(h(mm.group(1)), 0)
            found[h(mm.group(1))] += 1
        ts = re.findall(r'\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}', txt)
        cfgw = len(re.findall(r'config\.toml', txt))
        print(day, os.path.basename(f)[:60], 'desde', ts[0] if ts else None, 'hasta', ts[-1] if ts else None, 'guids', {k: (v, 'ACTUAL' if k == curh else 'otro') for k, v in found.items()}, 'menciones config.toml', cfgw)
