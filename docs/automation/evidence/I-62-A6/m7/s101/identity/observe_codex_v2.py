# Passive observation of the Codex identity (decisiones §101, point 3; v2 of the §90 script: kit v2 cfg-fp.ps1 and baseline 0A9F6A34, process count). Never executes Codex; never reads a value of config.toml
# (only its SHA-256, size, mtime and the sanitized key names through cfg-fp.ps1, as the probe kits did); never reads auth.json.
#   python -I observe_codex.py OUT.json
import datetime, glob, hashlib, json, os, subprocess, sys
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
KIT = os.path.join(WT, 'docs', 'automation', 'evidence', 'I-62-F6', 'kits', 'FX-02', 'a6m7-measure-v2')
LA = os.environ['LOCALAPPDATA']
out = sys.argv[1]
sha = lambda p: hashlib.sha256(open(p, 'rb').read()).hexdigest()
utc = lambda: datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ')
o = {'Schema': 'i62-codex-passive-observation/1', 'ObservedUtcStart': utc()}
bins = sorted(glob.glob(os.path.join(LA, 'OpenAI', 'Codex', 'bin', '*', 'codex.exe')))
o['CodexBinaries'] = [{'Path': '%LOCALAPPDATA%/' + os.path.relpath(b, LA).replace(os.sep, '/'), 'Size': os.path.getsize(b),
                       'MtimeUtc': datetime.datetime.fromtimestamp(os.path.getmtime(b), datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'),
                       'Sha256': sha(b)} for b in bins]
if len(bins) == 1:
    d = os.path.dirname(bins[0])
    lines = ''.join('%s %s\n' % (os.path.basename(f), sha(f)) for f in sorted(glob.glob(os.path.join(d, '*.exe'))))
    o['Companions'] = {'Label': os.path.basename(d), 'Lines': lines.splitlines(), 'Sha256First16': hashlib.sha256(lines.encode()).hexdigest()[:16]}
app = subprocess.run(['powershell.exe', '-NoProfile', '-Command', '(Get-AppxPackage *OpenAI.Codex*).Version'], capture_output=True, text=True)
o['AppVersion'] = app.stdout.strip()
tmp = out + '.cfg.json'
fp = subprocess.run(['pwsh', '-NoProfile', '-File', os.path.join(KIT, 'cfg-fp.ps1'), '-Out', tmp], capture_output=True, text=True)
cfg = json.load(open(tmp, encoding='utf-8-sig'))
os.remove(tmp)
base = json.load(open(os.path.join(KIT, 'baseline-keynames-0A9F6A34.json'), encoding='utf-8'))
o['Fingerprint'] = {'Sha256': cfg['Sha256'], 'Length': cfg['Length'], 'LastWriteTimeUtc': cfg['LastWriteTimeUtc'],
                    'KeyNamesCount': len(cfg['KeyNames']), 'KeyNamesEqualBaseline0A9F6A34': cfg['KeyNames'] == base}
assert fp.stdout.strip() == cfg['Sha256']
rt = os.path.join(os.environ['USERPROFILE'], '.cache', 'codex-runtimes', 'codex-primary-runtime')
pw = os.path.join(rt, 'dependencies', 'native', 'powershell', 'pwsh.exe')
rj = json.load(open(os.path.join(rt, 'runtime.json'), encoding='utf-8'))
o['RuntimePwsh'] = {'Path': '%USERPROFILE%/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell/pwsh.exe', 'Sha256': sha(pw),
                    'RuntimeBundleVersion': rj.get('bundleVersion')}
pr = subprocess.run(['powershell.exe', '-NoProfile', '-Command', '@(Get-Process -Name codex -ErrorAction SilentlyContinue).Count'], capture_output=True, text=True)
o['CodexProcessCount'] = int(pr.stdout.strip() or 0)
o['ObservedUtcEnd'] = utc()
s = json.dumps(o, ensure_ascii=True, indent=1) + '\n'
u = os.path.basename(os.environ['USERPROFILE'].rstrip('\\/')).lower()
assert u not in s.lower() and '@' not in s
open(out, 'w', encoding='ascii', newline='\n').write(s)
print(o['ObservedUtcStart'], [b['Sha256'][:8] for b in o['CodexBinaries']], o['AppVersion'], o['Fingerprint']['Sha256'][:8],
      o['Fingerprint']['KeyNamesEqualBaseline0A9F6A34'], o.get('Companions', {}).get('Sha256First16'), o['RuntimePwsh']['Sha256'][:8], o['CodexProcessCount'])
