import hashlib, json, os, subprocess, sys
def g(*a):
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    env['GIT_OPTIONAL_LOCKS'] = '0'
    p = subprocess.run(['git'] + list(a), capture_output=True, env=env, stdin=subprocess.DEVNULL)
    return p.returncode, p.stdout
def h(b): return hashlib.sha256(b).hexdigest()
wt = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
o = 'D:/r62-fixture/fixture-origin.git'
main = 'D:/Documentos/Codex/Calculadora de racks'
out = {}
rc, refs = g('--git-dir=' + o, 'for-each-ref', '--format=%(refname) %(objectname)')
out['OriginRefsSha256'] = h(refs); out['OriginRefCount'] = len(refs.splitlines())
out['OriginFxU1'] = [l.decode() for l in refs.splitlines() if l.startswith(b'refs/heads/fx/u1 ')]
rc, x = g('-C', wt, 'rev-parse', 'HEAD'); out['WorktreeHead'] = x.decode().strip()
rc, x = g('-C', wt, 'status', '--porcelain=v1', '-z', '--ignored'); out['WorktreeStatusSha256'] = h(x)
for blob in ('9a829b0eeb1210238052b1feac06cfa8f7964aed', '434ef1fccb0d15762b30244b4a60928120698411'):
    rc, x = g('--no-replace-objects', '-C', wt, 'cat-file', 'blob', blob); out['Blob ' + blob] = h(x)
rc, x = g('-C', main, 'rev-parse', 'HEAD'); out['MainHead'] = x.decode().strip()
rc, x = g('-C', main, 'status', '--porcelain=v1', '-z'); out['MainStatusSha256'] = h(x); out['MainStatusEmpty'] = (x == b'')
print(json.dumps(out, indent=1, sort_keys=True))
