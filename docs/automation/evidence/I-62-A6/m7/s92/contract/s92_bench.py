# s92_bench.py - static/bench evidence for the limited disposition that excludes the B-min `contract` order from the FX-02 VERIFY
# invocations and from the A6-7 measurement (I-62, decisiones §92, punto 4). Reuses existing components byte for byte:
#   - the kit's reconstruction rule reconstruct_runs_m7.py (W: kits/FX-02/a6m7-measure, SHA-256 1eceecee...), run as its CLI;
#   - the m7 bench libraries through d1_nc2.py (its own byte check against bench/README.md §1) and the S1 copy of the delivery
#     repository left by d1_nc2.py (nc2/w/c-x, G2 0e27c946, with the loose object of the instrument);
#   - the result of the nc2 run (nc2/nc2-result.json, SHA-256 86cd607e..., custodied in W at s91/d1/nc2/).
# No mutant battery, no model, no network. Writes only under D:\r62-a6m7\d1\s92 (and the read-only acceptor run on nc2/w/c-x,
# with the bench's zero-write check).
#   (A) the kit's rule on four Codex-format logs (two with `contract`, two without) + the proposed declared-order check;
#   (B) nc2 S1 without `contract`: accepted, relative oracle intact (from nc2-result.json);
#   (C) one VERIFY-like session on S1 nc2 with the 11 declared orders plus `read-json --file CONTRACT` and `read-json --file
#       DELEG` (eight pointers): reconstruction with the kit's rule, the declared-order check, the acceptor and the table, the
#       Controller computing Contract from the read-json values with U-19, and the supervision cross-check;
#   (X) the supervision cross-check (contract_crosscheck.py) on every Controller output of the nc2 run.
# Run from pwsh:  python -I s92_bench.py
import hashlib, importlib.util, json, os, subprocess, sys, time

sys.dont_write_bytecode = True
S92 = os.path.dirname(os.path.abspath(__file__)).replace('\\', '/')
D1 = os.path.dirname(S92)
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
KIT_RULE = 'docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/reconstruct_runs_m7.py'
KIT_RULE_SHA = '1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab'
NC2_RESULT_SHA = '86cd607ef81f4428af48bf4f6bcd0e11c688d28962682b04c5e76a8a1fb107be'
POINTERS = ['/AllowedWriteScope', '/ForbiddenWriteScope', '/Invariants', '/StopConditions', '/Authorities', '/RequiredTests',
            '/RoleRequirements', '/SupersededCommits']


def sha(b):
    return hashlib.sha256(b).hexdigest()


def rb(p):
    with open(p, 'rb') as fh:
        return fh.read()


def load(name, path):
    s = importlib.util.spec_from_file_location(name, path)
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


def genv():
    e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    e['GIT_OPTIONAL_LOCKS'] = '0'
    return e


def kit_rule():
    p = subprocess.run(['git', '--no-replace-objects', '-C', WT, 'show', 'HEAD:' + KIT_RULE], capture_output=True,
                       stdin=subprocess.DEVNULL, env=genv())
    blob = subprocess.run(['git', '-C', WT, 'rev-parse', 'HEAD:' + KIT_RULE], capture_output=True, stdin=subprocess.DEVNULL,
                          env=genv()).stdout.decode('ascii').strip()
    if p.returncode != 0 or sha(p.stdout) != KIT_RULE_SHA:
        raise SystemExit('kit reconstruction rule not identical to the custodied one')
    dst = S92 + '/lib/reconstruct_runs_m7.py'
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    if not os.path.isfile(dst) or rb(dst) != p.stdout:
        with open(dst, 'wb') as fh:
            fh.write(p.stdout)
    return dst, blob


def run_rule(rule, log, blob, env_dir=None):
    argv = [sys.executable, '-I', rule, '--log', log, '--blob', blob] + (['--env-dir', env_dir] if env_dir else [])
    p = subprocess.run(argv, capture_output=True, stdin=subprocess.DEVNULL, env=genv())
    return p.returncode, json.loads(p.stdout.decode('ascii'))


def main():
    if sys.flags.isolated != 1:
        raise SystemExit('run with python -I')
    t0 = time.time()
    DO = load('declared_orders', S92 + '/declared_orders.py')
    CC = load('contract_crosscheck', S92 + '/contract_crosscheck.py')
    D = load('d1_nc2', D1 + '/nc2/d1_nc2.py')               # byte check of the bench libraries + work-area overrides
    M, X, L, Y, RM = D.M, D.X, D.L, D.Y, D.RM
    # this run's work area (the S1 copy stays where d1_nc2.py left it: nc2/w/c-x)
    W = S92 + '/w/work'
    L.rmtree(S92 + '/w')
    L.rmtree(S92 + '/out')
    for mod in (X, L, M):
        mod.WORK = W
    L.PIN, L.BIN = W + '/pin', W + '/bin'
    L.PIN_TOOL, L.PIN_ACC = L.PIN + '/bmin_facts.py', L.PIN + '/bmin_accept.py'
    L.HERE = S92
    RM.W = W
    L.ensure_dir(W)
    pins = L.ensure_pins()
    B = L.B
    rule, rule_blob = kit_rule()
    res = {'Schema': 'rackcad-i62-s92-bench/0-experimental', 'Environment': D.environment(),
           'Pins': dict((k, {'Blob': v['Blob'], 'Sha256': v['Sha256']}) for k, v in pins.items()),
           'KitRule': {'Path': KIT_RULE, 'Blob': rule_blob, 'Sha256': KIT_RULE_SHA, 'Rule': None},
           'DeclaredOrders': list(DO.DECLARED_ORDERS_S92)}

    # ---------------- (B) the nc2 result (identity first)
    nc2p = D1 + '/nc2/nc2-result.json'
    if sha(rb(nc2p)) != NC2_RESULT_SHA:
        raise SystemExit('nc2-result.json changed')
    nc = json.loads(rb(nc2p).decode('ascii'))

    # ---------------- (A) the kit's rule on existing Codex-format logs
    logs = {'S1/NC2-A (12 ordenes, con contract)': D1 + '/nc2/w/work/s-s1-nc2-a-log/session-events.jsonl',
            'S1/NC2-C (11 ordenes, sin contract)': D1 + '/nc2/w/work/s-s1-nc2-c-log/session-events.jsonl',
            'S0/V-A (12 ordenes, con contract)': D1 + '/nc2/w/work/s-s0-v-a-log/session-events.jsonl',
            'S0/V-C (11 ordenes, sin contract)': D1 + '/nc2/w/work/s-s0-v-c-log/session-events.jsonl'}
    keys = {'S1/NC2-A (12 ordenes, con contract)': ('S1', 'NC2-A'), 'S1/NC2-C (11 ordenes, sin contract)': ('S1', 'NC2-C'),
            'S0/V-A (12 ordenes, con contract)': ('S0', 'V-A'), 'S0/V-C (11 ordenes, sin contract)': ('S0', 'V-C')}
    A = {}
    for label, log in logs.items():
        rc, rec = run_rule(rule, log, B)
        res['KitRule']['Rule'] = rec.get('Rule')
        sn, inv = keys[label]
        i = nc['Scenarios'][sn]['Invocations'][inv]
        bench_env = [e['Sha256'] for e in i['Envelopes']]
        chk = DO.check(rec)
        A[label] = {'LogSha256': sha(rb(log)), 'LogSha256InNc2Result': i['Session']['LogSha256'],
                    'RuleExit': rc, 'Reconstructible': rec['Reconstructible'], 'InstrumentExecutions': rec['InstrumentExecutions'],
                    'Orders': [r.get('Argv', [None])[0] for r in rec['Runs']], 'Renderings': sorted(set(r.get('Rendering') for r in rec['Runs'])),
                    'EnvelopesEqualBenchReconstruction': [r.get('EnvelopeSha256') for r in rec['Runs']] == bench_env,
                    'DeclaredOrderCheck': chk}
        print('A', label, 'rule', rec['Reconstructible'], rec['InstrumentExecutions'], 'check', chk['Ok'], chk['Outside'], flush=True)
    res['A'] = A

    o = nc['Oracle']
    res['B'] = {'Nc2ResultSha256': NC2_RESULT_SHA, 'Nc2ResultBlobInW': 'docs/automation/evidence/I-62-A6/m7/s91/d1/nc2/nc2-result.json',
                'S1/C/c': o['S1/C/c'], 'S1/A/b1': o['S1/A/b1'], 'S1/A/b2': o['S1/A/b2'],
                'S1NC2C': {k: nc['Scenarios']['S1']['Invocations']['NC2-C']['Outputs']['c'][k] for k in ('ControllerResults', 'AcceptedA65r2', 'VerificationIncluded')},
                'S1NC2CAcceptor': nc['Scenarios']['S1']['Invocations']['NC2-C']['Outputs']['c']['Acceptor'],
                'S1VC': {k: nc['Scenarios']['S1']['Invocations']['V-C']['Outputs']['c'][k] for k in ('ControllerResults', 'AcceptedA65r2')}}

    # ---------------- (C) VERIFY-like session on S1 nc2: 11 declared orders + read-json --file CONTRACT / DELEG
    T = M.BENCH + '/c-x'
    g2 = L.gout(T, 'rev-parse', 'HEAD')
    if g2 != nc['Scenarios']['S1']['G2'] or L.gout(T, 'status', '--porcelain'):
        raise SystemExit('S1 copy not in the state left by d1_nc2.py')
    p2 = L.p2(T, 'pwsh')
    if not p2['Launch']:
        raise SystemExit('point-of-use check failed on the S1 copy')
    inp = D1 + '/nc2/w/work/in-s1-nc2'
    M.RUN['NC2XR'] = 'R20261017T000000Z-d1e4'
    M.TASKDIR['NC2XR'] = 'T1-nc2'
    inv = M.invocation('NC2XR', target=g2)
    des = M.designation('NC2XR', T, inp, inv)
    Ep, E, rep = M.build_e(des, 'R7-A', W + '/build', 's92-nc2-r')
    dres = M.derive(des, Ep, W + '/build', 's92-nc2-r')
    if not dres['Derived']:
        raise SystemExit('derivation failed: ' + json.dumps(dres['Check']['Problems']))
    sv = {'Closure': dres['ClosurePath'], 'Vector': dres['VectorPath'], 'Record': dres['RecordPath'], 'DesignationPath': dres['DesignationPath']}
    kp, dp = inp + '/K.json', inp + '/deleg.json'
    ptr = sum([['--pointer', x] for x in POINTERS], [])
    rs = D.runs_s1(T, inp, inv, D.NAMES_C, g2)
    rs += [('read-json', ['read-json', '--repo', T] + M.file_arg('CONTRACT', kp) + ptr),
           ('read-json', ['read-json', '--repo', T] + M.file_arg('DELEG', dp) + ptr)]
    sess, execs, rec, rruns = M.session('s92-nc2-r-log', T, rs, D.prim_s1(g2))
    rc_k, rec_k = run_rule(rule, sess['LogPath'], B)
    chk_k = DO.check(rec_k)
    # Controller: values of the two read-json --file runs -> A3-A5 (+16.22) with U-19
    vals = {}
    cites = []
    for r in rruns:
        a = r['Argv']
        if a[0] == 'read-json' and '--file' in a:
            name = a[a.index('--file') + 1].split('=', 1)[0]
            env = json.loads(L.rbytes(r['Envelope']).decode('ascii'))
            vals[name] = dict((p[1:], env['Facts']['Values'][p]) for p in POINTERS)
            cites.append('instrumento: bmin_facts %s; FactsSha256=%s' % (' '.join(a), env['FactsSha256']))
    Kd, Dd = json.loads(L.rbytes(kp).decode('utf-8')), json.loads(L.rbytes(dp).decode('utf-8'))
    faithful = {'CONTRACT': all(vals['CONTRACT'][p[1:]] == Kd[p[1:]] for p in POINTERS),
                'DELEG': all(vals['DELEG'][p[1:]] == Dd[p[1:]] for p in POINTERS)}
    ok, det = D.contract_by_controller(vals['CONTRACT'], vals['DELEG'])
    F = X.facts_by_cmd(rruns)
    F2 = dict(F)
    F2['contract'] = (dict(det, A5Missing=dict(det['A5Missing'], RoleRequirements=det['RoleRequirementsMissing'])), '-', ['contract'], None)
    deleg, hand = Dd, json.loads(L.rbytes(inp + '/handoff.json').decode('utf-8'))
    cv = M.cv_case(F2, deleg, hand, sess['Primary'], inv['RunId'])
    cv['Checks']['Contract'] = {'Result': 'pass' if ok else 'fail',
                                'Evidence': '; '.join(cites) + '; A3-A5 y A3\'-A5\' calculadas por el Controller con AP 16.5 y U-19 sobre esos valores '
                                            '(sin la orden contract); contraste primario: SHA-256 de CONTRACT y DELEG = los --sha256 de las dos ordenes'}
    M.reclassify(cv)
    cv['Findings'] = list(cv['Findings']) + ['Salida sintetica s92: sin la orden contract; Contract desde read-json --file CONTRACT y DELEG.']
    work = W + '/s-s92-nc2-r'
    L.ensure_dir(work)
    cvp = work + '/controller-output.json'
    L.wjson_local(cvp, cv)
    relayp, _ = RM.relay_save(work, 'relay-facts.json', sess, cvp, inv['RunId'])
    oacc, reqp, dec = RM.verify_request('r', T, {'Inp': inp, 'Inv': inv}, sv, rruns, rec, cvp, relayp, work)
    v_res = nc['Scenarios']['S1']['Invocations']['V-C']['Outputs']['c']['ControllerResults']
    n_res = D.results_of(cv)
    cross = CC.compare(Kd, Dd, cv)
    def keep(rel, src):
        b = L.rbytes(src)
        t = b.decode('utf-8', 'replace')
        if L.sanitize_str(t) != t:
            b = L.sanitize_str(t).encode('utf-8')
        L.wbytes(S92 + '/out/' + rel, b)
        return {'Path': 'out/' + rel, 'Sha256': sha(b), 'Sanitized': L.sanitize_str(t) != t}
    out_files = {'Controller': keep('s92-nc2-r/controller-output.json', cvp),
                 'Request': keep('s92-nc2-r/request.json', reqp),
                 'AcceptorStdout': keep('s92-nc2-r/acceptor-stdout.json', reqp[:-5] + '.acceptor-stdout.json')}
    for r in rruns:
        if r['Argv'][0] == 'read-json' and '--file' in r['Argv']:
            nm = r['Argv'][r['Argv'].index('--file') + 1].split('=', 1)[0]
            out_files['Envelope-read-json-' + nm] = keep('s92-nc2-r/envelope-read-json-%s.json' % nm, r['Envelope'])
    res['C'] = {'RunId': inv['RunId'], 'Clone': 'nc2/w/c-x', 'G2': g2, 'P2': {'Launch': p2['Launch'], 'FailedSteps': p2['FailedSteps']},
                'Orders': [n for n, _ in rs], 'Derivation': M.brief(dres),
                'Session': {k: sess[k] for k in ('LogSha256', 'ZeroWriteToolRuns')},
                'BenchReconstruction': M.rec_brief(rec),
                'KitRule': {'Exit': rc_k, 'Reconstructible': rec_k['Reconstructible'], 'InstrumentExecutions': rec_k['InstrumentExecutions'],
                            'Orders': [r.get('Argv', [None])[0] for r in rec_k['Runs']],
                            'EnvelopesEqualBenchReconstruction': [r.get('EnvelopeSha256') for r in rec_k['Runs']] == [r['EnvelopeSha256'] for r in rec['Runs']]},
                'DeclaredOrderCheck': chk_k, 'ReadJsonFaithful': faithful,
                'ContractByController': {'Result': 'pass' if ok else 'fail', 'A3': det['A3'], 'A4Uncontained': [x['Entry'] for x in det['A4'] if not x['ContainedBy']]},
                'ControllerResults': n_res, 'Classification': [cv['Classification'], cv['Disposition'], cv['FailureClass']],
                'SchemaErrors': X.validate('controller-verification.v2.schema.json', cv),
                'Acceptor': {'Rc': oacc['Rc'], 'Decision': oacc['Decision'], 'Checks': oacc['Checks'], 'MismatchDetails': oacc['MismatchDetails'],
                             'R9OutsideClosure': oacc['R9Runs'], 'ZeroWrite': oacc['ZeroWrite'], 'ProcessTreeDead': oacc['ProcessTreeDead'],
                             'RequestSha256': oacc['RequestSha256'], 'StdoutSha256': oacc['AcceptorStdoutSha256']},
                'VerificationIncluded': dec['Include'], 'Table': oacc['Table'], 'AcceptedA65r2': RM.ok_accept(oacc, 'MATCH' if dec['Include'] else 'NOT_APPLICABLE'),
                'OracleVsS1VC': D.oracle(v_res, n_res), 'CrossCheck': cross, 'Files': out_files}
    print('C', 'kit rule', rec_k['Reconstructible'], 'check', chk_k['Ok'], 'acceptor', oacc['Rc'], oacc['Decision'], oacc['Checks'].get('R7'),
          'accepted', res['C']['AcceptedA65r2'], 'oracle', res['C']['OracleVsS1VC']['Holds'], 'cross', cross['Agree'], flush=True)

    # ---------------- (X) supervision cross-check on every Controller output of the nc2 run
    pairs = {'s0-v-a': 's0-v', 's0-v-c': 's0-v', 's0-nc2-a': 's0-nc2', 's0-nc2-c': 's0-nc2',
             's1-v-a': 's1-v', 's1-v-c': 's1-v', 's1-nc2-a': 's1-nc2', 's1-nc2-c': 's1-nc2'}
    Kc = json.loads(rb(D1 + '/nc2/out/inputs/K.json').decode('utf-8'))
    XR = {}
    for lab, inpd in sorted(pairs.items()):
        Dc = json.loads(rb(D1 + '/nc2/out/inputs/%s/deleg.json' % inpd).decode('utf-8'))
        for f in sorted(os.listdir(D1 + '/nc2/out/' + lab)):
            if f.startswith('controller-output-'):
                cvx = json.loads(rb(D1 + '/nc2/out/%s/%s' % (lab, f)).decode('utf-8'))
                c = CC.compare(Kc, Dc, cvx)
                XR['%s/%s' % (lab, f[len('controller-output-'):-5])] = {'Recomputed': c['Recomputed'], 'Controller': c['Controller'],
                                                                         'Agree': c['Agree'], 'NotCovered': [r['Entry'] for r in c['NotCovered']]}
    res['X'] = XR
    for k, v in XR.items():
        print('X', k, v, flush=True)
    res['Seconds'] = round(time.time() - t0, 1)
    data = (json.dumps(L.sanitize(res), ensure_ascii=True, sort_keys=True, indent=1) + '\n').encode('ascii')
    L.wbytes(S92 + '/s92-result.json', data)
    print('wrote s92-result.json', sha(data), 'seconds', res['Seconds'])


if __name__ == '__main__':
    main()
