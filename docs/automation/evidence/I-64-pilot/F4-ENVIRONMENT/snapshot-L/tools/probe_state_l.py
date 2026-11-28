"""Estado sin red para la sonda de F3: repositorio real, binario y config. Uso: python probe_state_f.py <salida.json>"""
import hashlib, json, os, subprocess, sys
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-workspace-persistente-rackcad')
L = os.environ['<datos-locales-del-usuario>']
BIN_DIR = os.path.join(L, 'OpenAI', 'Codex', 'bin')
g = lambda *a: subprocess.run(['git', '--no-optional-locks', '-C', WT] + list(a), capture_output=True, text=True, encoding='utf-8').stdout.strip()
bins = sorted(os.path.relpath(os.path.join(r, f), L).replace(chr(92), '/') for r, _, fs in os.walk(BIN_DIR) for f in fs if f.lower() == 'codex.exe')
new = os.path.join(BIN_DIR, '5449c1f0f26f1f98', 'codex.exe')
rec = {'Real': {'Head': g('rev-parse', 'HEAD'), 'TrackingBranch': g('rev-parse', 'refs/remotes/origin/architecture/workspace-persistente-rackcad'), 'TrackingMain': g('rev-parse', 'refs/remotes/origin/main'), 'StatusPorcelain': g('status', '--porcelain')},
       'CodexExes': bins, 'BinarySha256': hashlib.sha256(open(new, 'rb').read()).hexdigest().upper(), 'ConfigSha256': hashlib.sha256(open(os.path.join(os.environ['USERPROFILE'], '.codex', 'config.toml'), 'rb').read()).hexdigest().upper()}
open(sys.argv[1], 'w', encoding='ascii', newline='\n').write(json.dumps(rec, indent=2) + '\n')
print(json.dumps({k: rec[k] for k in ('CodexExes', 'BinarySha256', 'ConfigSha256')}), 'realhead', rec['Real']['Head'][:8], 'status[%s]' % rec['Real']['StatusPorcelain'])
