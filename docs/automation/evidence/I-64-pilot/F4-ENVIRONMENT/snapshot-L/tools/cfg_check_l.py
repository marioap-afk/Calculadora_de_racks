"""config.toml frente a los snapshots activos E y F. Mismo extractor de nombres que cfg_check_e.py; sin valores.
Uso: python cfg_check_f.py [salida.json]; codigo 0 si es exactamente E o F con la misma lista de nombres, 7 si no."""
import datetime, hashlib, json, os, re, sys
OK = {'B71650A637551747F522BB1D2F4BA1BB9E25AAFF1A61CE1B7BC066197FFFEBB3': 'L'}  # solo para la sonda de la L
NAMES = '3ECF8E072CC52AE60A6E6D508DB54DCBFFFAFB04EAF9B52905A2CEAB0E0812E3'  # la L: 105 nombres (se anade mcp_servers.node_repl.env.NODE_REPL_LAZY_TRUSTED_WORKER)
cfg = os.path.join(os.environ['USERPROFILE'], '.codex', 'config.toml')
raw = open(cfg, 'rb').read()
sha = hashlib.sha256(raw).hexdigest().upper()
red = lambda k: re.sub(r'^\[projects\..*\]$', '[projects.<redactado>]', k)
names = []
for ln in raw.decode('utf-8').splitlines():
    s = ln.strip()
    if not s or s.startswith('#'):
        continue
    if s.startswith('[') and s.endswith(']') and not re.match(r'^\[\s*["\']', s):
        names.append(s)
        continue
    m = re.match(r'^([A-Za-z0-9_\-]+|"[^"]+")\s*=', s)
    if m:
        names.append(m.group(1))
names_sha = hashlib.sha256('\n'.join(names).encode('utf-8')).hexdigest().upper()
res = {'CheckedUtc': datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%fZ'), 'ConfigSha256': sha, 'Snapshot': OK.get(sha), 'ShaInAllowlist': sha in OK, 'NameCount': len(names), 'OrderedNamesSha256': names_sha,
       'LastWriteUtc': datetime.datetime.fromtimestamp(os.path.getmtime(cfg), datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%fZ'), 'ConfigTomlKeys': [red(n) for n in names]}
res['Accepted'] = sha in OK and names_sha == NAMES and len(names) == 105
if len(sys.argv) > 1:
    open(sys.argv[1], 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
print(json.dumps({k: res[k] for k in ('ConfigSha256', 'Snapshot', 'Accepted', 'NameCount')}))
sys.exit(0 if res['Accepted'] else 7)
