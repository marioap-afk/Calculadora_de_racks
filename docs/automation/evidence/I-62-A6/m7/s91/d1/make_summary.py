# make_summary.py - summary.json of the D-1 / nc2 analysis (I-62, A-6; decisiones §91, punto 3).
# Reads min/min-result.json and nc2/nc2-result.json, adds the static checks of the A6-7 measurement inputs (bench M and the
# frozen kit a6m7-measure, read only from the I-62 worktree by `git show HEAD:<path>`) and of the D-1 condition on every
# delegation of this analysis, a hygiene scan of the deliverables and a file manifest. Stdlib only.
#   python -I make_summary.py
import hashlib, json, os, re, subprocess, sys

HERE = os.path.dirname(os.path.abspath(__file__)).replace('\\', '/')
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
EMAIL = re.compile(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}')
TRANSIENT = ('min/repo', 'nc2/w', 'lib/__pycache__')            # work areas: clones, session logs, loose objects (not deliverables)


def sha(b):
    return hashlib.sha256(b).hexdigest()


def rb(p):
    with open(p, 'rb') as fh:
        return fh.read()


def wt_show(path):
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    env['GIT_OPTIONAL_LOCKS'] = '0'
    p = subprocess.run(['git', '--no-replace-objects', '-C', WT, 'show', 'HEAD:' + path], capture_output=True, stdin=subprocess.DEVNULL, env=env)
    b = subprocess.run(['git', '-C', WT, 'rev-parse', 'HEAD:' + path], capture_output=True, stdin=subprocess.DEVNULL, env=env)
    if p.returncode != 0:
        raise SystemExit('cannot read ' + path)
    return p.stdout, b.stdout.decode('ascii').strip()


def u19(e):
    return e[:-2] if e.endswith('/**') else e


def cov(entry, path):
    return path == entry or (entry.endswith('/') and path.startswith(entry))


def readings(K, D):
    out = {}
    for name, f in (('Instrument', lambda b, e: cov(b, e)), ('U19Both', lambda b, e: cov(u19(b), u19(e)))):
        out[name] = {'A3': [[e, sorted(b for b in K['AllowedWriteScope'] if f(b, e))] for e in D['AllowedWriteScope']],
                     'A4': [[e, sorted(g for g in D['ForbiddenWriteScope'] if f(g, e))] for e in K['ForbiddenWriteScope']]}
    pairs = [['A3', a, b] for a, bs in out['U19Both']['A3'] for b in bs if b not in dict(out['Instrument']['A3']).get(a, [])]
    pairs += [['A4', a, b] for a, bs in out['U19Both']['A4'] for b in bs if b not in dict(out['Instrument']['A4']).get(a, [])]
    verdict = lambda r: all(x[1] for x in r['A3']) and all(x[1] for x in r['A4'])
    return {'D1Pairs': pairs, 'D1': bool(pairs), 'A3A4PassInstrumentReading': verdict(out['Instrument']),
            'A3A4PassU19Reading': verdict(out['U19Both'])}


def main():
    if sys.flags.isolated != 1:
        raise SystemExit('run with python -I')
    mn = json.loads(rb(HERE + '/min/min-result.json').decode('ascii'))
    nc = json.loads(rb(HERE + '/nc2/nc2-result.json').decode('ascii'))
    K =json.loads(rb(HERE + '/nc2/out/inputs/K.json').decode('utf-8'))
    static = {}
    # A6-7: the delegation of the bench measurement M and of the frozen kit a6m7-measure
    mdeleg_b, mdeleg_blob = wt_show('docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/deleg.json')
    mdeleg = json.loads(mdeleg_b.decode('utf-8'))
    static['A67-kit-a6m7-measure'] = dict(readings(K, mdeleg), Delegation={'Path': 'docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/deleg.json',
                                                                            'Blob': mdeleg_blob, 'Sha256': sha(mdeleg_b)},
                                          ListsEqualContract=mdeleg['AllowedWriteScope'] == K['AllowedWriteScope'] and mdeleg['ForbiddenWriteScope'] == K['ForbiddenWriteScope'])
    bm = 'D:/r62-a6m7/bench/work/in-m/deleg.json'
    if os.path.isfile(bm):
        bmd = json.loads(rb(bm).decode('utf-8'))
        static['A67-bench-M'] = dict(readings(K, bmd), Delegation={'Path': bm, 'Sha256': sha(rb(bm))},
                                     ListsEqualContract=bmd['AllowedWriteScope'] == K['AllowedWriteScope'] and bmd['ForbiddenWriteScope'] == K['ForbiddenWriteScope'])
    p8, p8blob = wt_show('docs/automation/evidence/I-62-A6/m7/bench/p8-m.json')
    p8d = json.loads(p8.decode('ascii'))
    ma = [c for c in p8d['Cases'] if c['Case'] == 'M-A'][0]
    static['A67-bench-p8-m'] = {'Blob': p8blob, 'Sha256': sha(p8), 'Case': 'M-A', 'Decision': ma['Observed']['Decision'],
                                'Checks': ma['Observed']['Checks'], 'TableAccepted': ma['Observed']['Table']['Accepted'], 'Holds': ma['Holds']}
    sm = 'D:/r62-a6m7/bench/work/s-m/request-A.acceptor-stdout.json'
    if os.path.isfile(sm):
        r = json.loads(rb(sm).decode('ascii'))
        r7 = [c for c in r['Checks'] if c['Id'] == 'R7'][0]
        static['A67-bench-p8-m']['R7Runs'] = [{'Command': x['Command'], 'Compared': x.get('Compared'), 'Mismatches': len(x.get('Mismatches') or [])}
                                              for x in r7['Detail']['Runs']]
    # the delegations of this analysis
    for tag in ('s0-v', 's0-nc2', 's1-v', 's1-nc2'):
        D = json.loads(rb(HERE + '/nc2/out/inputs/%s/deleg.json' % tag).decode('utf-8'))
        static['deleg-' + tag] = dict(readings(K, D), AllowedWriteScope=D['AllowedWriteScope'])
    # what nc2 needs for D-1 on T1: a VERIFIED diff with >= 2 files under the evidence prefix (first file excluded, others enumerated)
    cases = []
    for c in mn['Cases']:
        cases.append({'Name': c['Name'], 'Contract': [c['Contract']['AllowedWriteScope'], c['Contract']['ForbiddenWriteScope']],
                      'Delegation': [c['Delegation']['AllowedWriteScope'], c['Delegation']['ForbiddenWriteScope']],
                      'ToolA3': c['ToolA3'], 'ToolA4': c['ToolA4'], 'Decision': c['Decision'], 'R7': c['Statuses'].get('R7'),
                      'R7Mismatches': c['R7Mismatches'], 'FactsLevelAccepted': c['FactsLevelAccepted'], 'Diverge': c['Diverge']})
    inv = {}
    for sn, sc in nc['Scenarios'].items():
        for name, i in sc['Invocations'].items():
            for tag, o in i['Outputs'].items():
                inv['%s/%s/%s' % (sn, name, tag)] = {
                    'Orders': len(i['Orders']), 'ContractRun': 'contract' in i['Orders'],
                    'ContractFactsA3': [[x['Entry'], x['ContainedBy']] for x in (i.get('ContractFacts') or {}).get('A3', [])] or None,
                    'ContractByU19Ok': i['ContractByU19']['Ok'], 'Controller': o['ControllerResults'], 'Classification': o['Classification'],
                    'Acceptor': {'Rc': o['Acceptor']['Rc'], 'Decision': o['Acceptor']['Decision'], 'Checks': o['Acceptor']['Checks']},
                    'VerificationIncluded': o['VerificationIncluded'], 'TableAccepted': o['Table']['Accepted'],
                    'AcceptanceReasons': o['Table']['AcceptanceReasons'], 'AcceptedA65r2': o['AcceptedA65r2'],
                    'ZeroWrite': o['Acceptor']['ZeroWrite'], 'ProcessTreeDead': o['Acceptor']['ProcessTreeDead']}
    # hygiene of the deliverables and manifest
    user = os.path.basename(os.environ.get('USERPROFILE', ''))
    files, dirty, transient_dirty = {}, [], 0
    for root, dirs, fs in os.walk(HERE):
        rel_root = os.path.relpath(root, HERE).replace('\\', '/')
        if any(rel_root == t or rel_root.startswith(t + '/') for t in TRANSIENT):
            for f in fs:
                try:
                    t = rb(os.path.join(root, f)).decode('utf-8', 'replace')
                except OSError:
                    continue
                if (len(user) > 2 and user.lower() in t.lower()) or EMAIL.search(t):
                    transient_dirty += 1
            continue
        for f in sorted(fs):
            rel = (rel_root + '/' + f) if rel_root != '.' else f
            if rel == 'summary.json':
                continue
            b = rb(os.path.join(root, f))
            t = b.decode('utf-8', 'replace')
            if (len(user) > 2 and user.lower() in t.lower()) or EMAIL.search(t):
                dirty.append(rel)
            files[rel] = sha(b)
    summary = {'Schema': 'rackcad-i62-d1-summary/0-experimental',
               'Pinned': mn['Pinned'], 'Environment': {'Python': mn['Environment']['Python'], 'Git': mn['Environment']['Git'],
                                                       'Pwsh': mn['Environment']['Pwsh']},
               'MinimalCase': {'Repo': mn['Repo'], 'Cases': cases},
               'Nc2': {'S0': {k: nc['Scenarios']['S0'][k] for k in ('Diff', 'FirstDiffFile', 'RealAllowed', 'Nc2Allowed', 'BenchMutationEqualsThisReading')},
                       'S1': {k: nc['Scenarios']['S1'][k] for k in ('G2', 'ExtraPath', 'Diff', 'FirstDiffFile', 'RealAllowed', 'Nc2Allowed', 'P1', 'P2')},
                       'Invocations': inv, 'Oracle': nc['Oracle'], 'Seconds': nc['Seconds']},
               'Static': static, 'Hygiene': {'DeliverablesWithUserNameOrEmail': dirty, 'TransientDirs': list(TRANSIENT),
                                             'TransientFilesWithUserNameOrEmail': transient_dirty},
               'Files': files}
    data = (json.dumps(summary, ensure_ascii=True, sort_keys=True, indent=1) + '\n').encode('ascii')
    with open(HERE + '/summary.json', 'wb') as fh:
        fh.write(data)
    print('wrote summary.json', sha(data), 'dirty deliverables:', dirty, 'transient files with user name/e-mail:', transient_dirty)


if __name__ == '__main__':
    main()
