"""config.toml frente a los snapshots activos E y F. Mismo extractor de nombres que cfg_check_e.py; sin valores.
Uso: python cfg_check_f.py [salida.json]; codigo 0 si es exactamente E o F con la misma lista de nombres, 7 si no."""
import datetime, hashlib, json, os, re, sys
OK = {'7C1516A9135D59FA38F6004D56B71C7E1313502D6C6BFEA6FB5D8F97A62DD723': 'G', 'A859B66CC9A4631FEC01AA2E798AAC4F7E07F911352735A6A35D8169D9639864': 'H', '0B47701A333171A60B80E5426EAA314D63D26DF82433F91D246A34898FFA1021': 'I', '57B0FED688317D347BCC726F99549BDE6AC9EC108D6E2CCBA10A9D71F233F961': 'J', 'E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE': 'K'}  # H: decision del Owner tras el reinicio del 2026-11-13; I: sonda neutra 6603 PASS y decision del Owner del 2026-11-14 (actualizacion de la app de Codex a 26.1111.21308); J: sonda neutra d32c PASS y decision del Owner del 2026-11-18 (actualizacion de la app a 26.1111.2228.0 tras el reinicio); K: sonda neutra 2a9a PASS y decision del Owner del 2026-11-19 (actualizacion de la app de Codex, BROWSER_USE_CODEX_APP_VERSION 26.1111.71704)
NAMES = 'EB8775C5F9B3CC5C15031ED064391FFE000AB4A7220360C04F2F89D83D49F820'
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
res['Accepted'] = sha in OK and names_sha == NAMES and len(names) == 104
if len(sys.argv) > 1:
    open(sys.argv[1], 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
print(json.dumps({k: res[k] for k in ('ConfigSha256', 'Snapshot', 'Accepted', 'NameCount')}))
sys.exit(0 if res['Accepted'] else 7)
