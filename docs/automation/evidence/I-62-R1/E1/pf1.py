# PF-1 (A-8 §3.2): run `resume` read-only on the existing worktrees with the script of a given commit of the I-62 branch, and compare each verdict
# against an independent check made here from Git and the YAML (PyYAML), without the script. Nothing is written in the worktrees:
# `git status --porcelain` (no optional locks), HEAD and the SHA-256 of every state file are taken before and after.
#   python -B pf1.py <script.ps1> <out.json>
import hashlib, json, os, subprocess, sys
import yaml

NL = chr(10)
CR = chr(13)
UP = os.environ['USERPROFILE']
WTS = ['architecture-portabilidad-coordinador-principal', 'architecture-workspace-persistente-rackcad', 'architecture-interop-analisis-estructural',
       'fix-placedclone-copia-anotaciones', 'feature-rackmirror-espejo-semantico']
STATES = ['claimed', 'implementing', 'validating', 'ci-failed', 'waiting', 'review-ready', 'integration-ready', 'completed']
GATES = ['none', 'owner-decision', 'owner-validation', 'autocad', 'plugin-build', 'ci', 'dependency', 'conflict', 'permissions', 'scope']
FIELDS = ['initiative', 'branch', 'claim_id', 'current_phase', 'state', 'gate', 'attempts', 'next_action', 'last_evidence_commit']
script, out = sys.argv[1], sys.argv[2]


def git(w, *a):
    p = subprocess.run(['git', '--no-optional-locks', '-C', w] + list(a), capture_output=True, text=True, encoding='utf-8')
    return p.returncode, p.stdout.strip()


def snapshot(w):
    d = os.path.join(w, 'docs', 'automation', 'state')
    blobs = {}
    if os.path.isdir(d):
        for f in sorted(os.listdir(d)):
            if f.endswith('.yml'):
                blobs[f] = hashlib.sha256(open(os.path.join(d, f), 'rb').read()).hexdigest()
    return {'head': git(w, 'rev-parse', 'HEAD')[1], 'status': git(w, 'status', '--porcelain')[1], 'state_files': blobs}


def independent(w):
    branch = git(w, 'rev-parse', '--abbrev-ref', 'HEAD')[1]
    head = git(w, 'rev-parse', 'HEAD')[1]
    reasons = []
    d = os.path.join(w, 'docs', 'automation', 'state')
    found = []
    for f in sorted(os.listdir(d)) if os.path.isdir(d) else []:
        if f.endswith('.yml'):
            raw = open(os.path.join(d, f), encoding='utf-8').read().replace(CR + NL, NL)
            try:
                y = yaml.safe_load(raw)
            except Exception:
                if (NL + '  branch: ' + branch + NL) in raw:
                    found.append((f, None, None))
                continue
            a = (y or {}).get('automation_state') or {}
            if str(a.get('branch')) == branch:
                found.append((f, y, a))
    st = None
    if not found:
        reasons.append('no state for branch')
    elif len(found) > 1:
        reasons.append('several states for branch')
    elif found[0][1] is None:
        reasons.append('state file is not valid YAML')
    else:
        f, y, st = found[0]
        if y.get('schema') != 'rackcad-automation-state/v1':
            reasons.append('schema')
        for k in FIELDS:
            if st.get(k) in (None, ''):
                reasons.append('empty ' + k)
        if st.get('state') not in STATES:
            reasons.append('state vocabulary')
        if st.get('gate') not in GATES:
            reasons.append('gate vocabulary')
        if str(st.get('initiative')) != f[:-4]:
            reasons.append('initiative != file name')
        lec = str(st.get('last_evidence_commit') or '')
        if len(lec) != 40 or git(w, 'cat-file', '-e', lec + '^{commit}')[0] != 0 or git(w, 'merge-base', '--is-ancestor', lec, 'HEAD')[0] != 0:
            reasons.append('evidence not an ancestor')
    if git(w, 'status', '--porcelain')[1]:
        reasons.append('dirty tree')
    remote = git(w, 'ls-remote', 'origin', 'refs/heads/' + branch)[1].split()
    if not remote or remote[0] != head:
        reasons.append('head != origin')
    if reasons:
        verdict = 'STOP'
    elif st['state'] == 'completed':
        verdict = 'DONE'
    elif st['gate'] != 'none':
        verdict = 'WAIT ' + st['gate']
    elif st['state'] not in ('claimed', 'implementing', 'validating', 'ci-failed'):
        verdict = 'WAIT ' + st['state']
    else:
        verdict = 'CONTINUE'
    return {'branch': branch, 'verdict': verdict, 'reasons': reasons, 'next_action': (st or {}).get('next_action')}


rows = []
for name in WTS:
    w = os.path.join(UP, '.codex', 'worktrees', name)
    before = snapshot(w)
    p = subprocess.run(['pwsh', '-NoProfile', '-NonInteractive', '-File', script, 'resume', '-Path', w, '-Json'], capture_output=True, text=True,
                       encoding='utf-8')
    res = json.loads(p.stdout)
    after = snapshot(w)
    ind = independent(w)
    match = res['Verdict'] == ind['verdict'] and bool(res['Reasons']) == bool(ind['reasons']) and (
        ind['verdict'] == 'STOP' or (res.get('NextAction') or None) == (ind['next_action'] or None))
    rows.append({'Worktree': name, 'Branch': ind['branch'], 'Exit': p.returncode, 'Verdict': res['Verdict'], 'Reasons': res['Reasons'],
                 'StateFile': res.get('StateFile'), 'Independent': {'Verdict': ind['verdict'], 'Reasons': ind['reasons']},
                 'VerdictMatches': match, 'Unchanged': before == after})
ok = all(r['VerdictMatches'] and r['Unchanged'] for r in rows)
res = {'Script': script.replace(UP, '%USERPROFILE%'), 'ScriptSha256': hashlib.sha256(open(script, 'rb').read()).hexdigest(), 'PF1': 'PASS' if ok else 'FAIL',
       'Rows': rows}
open(out, 'w', encoding='utf-8', newline=NL).write(json.dumps(res, ensure_ascii=False, indent=1) + NL)
for r in rows:
    print(r['Worktree'][:40].ljust(42), r['Verdict'].ljust(22), 'match' if r['VerdictMatches'] else 'MISMATCH', 'unchanged' if r['Unchanged'] else 'CHANGED')
print('PF-1', res['PF1'])
