"""Solo lectura: prueba si config.toml actual (M) es la Snapshot L con un unico cambio, el GUID de la canalizacion
codex-computer-use-<GUID> de SKY_CUA_NATIVE_PIPE_DIRECTORY. Toma el GUID que registro la instancia de la app de Codex de la L
(su log del 2026-11-22), lo pone en lugar del actual en los bytes de config.toml y compara el SHA-256 con el de la L.
Nunca imprime un GUID ni el valor. Uso: python -I opaque_reconstruct_l.py <log-de-la-instancia-de-L>"""
import hashlib, os, re, sys, tomllib
L_SHA = 'B71650A637551747F522BB1D2F4BA1BB9E25AAFF1A61CE1B7BC066197FFFEBB3'
M_SHA = '3E9BC35B0E72641A81241EC3E32BF45010B8CC3CC93FDD4E3C5E8188E6D19FFC'
U = os.environ['USERPROFILE']
raw = open(os.path.join(U, '.codex', 'config.toml'), 'rb').read()
assert hashlib.sha256(raw).hexdigest().upper() == M_SHA, 'config.toml ya no es 3E9BC35B'
G = re.compile(rb'codex-computer-use-([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})')
cur = G.findall(raw)
assert len(set(cur)) == 1, ('GUIDs distintos en config.toml', len(set(cur)))
log = open(sys.argv[1], 'rb').read()
old = sorted(set(G.findall(log)))
assert len(old) == 1, ('GUIDs distintos en el log', len(old))
old = old[0]
assert old.lower() != cur[0].lower()
rebuilt = raw.replace(cur[0], old)
ok = hashlib.sha256(rebuilt).hexdigest().upper() == L_SHA
h = lambda b: hashlib.sha256(b.lower()).hexdigest()[:12].upper()
toml_ok = tomllib.loads(rebuilt.decode('utf-8')) is not None
print('ocurrencias del GUID en config.toml', raw.count(cur[0]), 'guid-sha actual', h(cur[0]), 'guid-sha de la instancia de L', h(old))
print('config.toml con el GUID de la instancia de L -> SHA-256 == Snapshot L:', ok, '| TOML valido:', toml_ok)
sys.exit(0 if ok else 1)
