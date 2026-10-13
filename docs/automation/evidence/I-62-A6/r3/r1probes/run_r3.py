# run_r3.py - probes of the round-3 candidate rule (I-62, A-6 round 3). Sequential; work area D:\r62-a6r3\r1.
#   python -I run_r3.py t1 | zero | n4 | nc1 | mutants | recon | summary | all
# Pinned acceptor and instrument (by blob), real instrument runs on fresh fixture clones, synthetic Controller outputs.
# No model, no network, no read of auth.json / credentials / config.toml.
import importlib.util, json, os, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_s = importlib.util.spec_from_file_location('lib_r3', os.path.join(HERE, 'lib_r3.py'))
X = importlib.util.module_from_spec(_s)
_s.loader.exec_module(X)
L, R, TB = X.L, X.R, X.TB
W = X.WORK
DECISION = 'BANCO-r3 (sin disposicion: banco de la ronda 3 de A-6; no es una DecisionRef normativa)'
R2W = 'D:/r62-a6r2/r1r4/r4/w-t1'
R2_RESULT = os.path.join(L.WT, 'docs', 'automation', 'evidence', 'I-62-A6', 'r2', 'r1r4', 'r4-t1.json')
ALL = ('R1', 'R2', 'R3', 'R4', 'R5', 'R6', 'R7', 'R8', 'R9', 'R10')


# ------------------------------------------------------------------ helpers
def save_state(name, obj):
    L.wbytes(W + '/state-' + name + '.json', (json.dumps(obj, ensure_ascii=True, indent=1, sort_keys=True) + '\n').encode('ascii'))


def load_state(name):
    return json.loads(L.rbytes(W + '/state-' + name + '.json').decode('ascii'))


def save_execs(label, execs):
    d = W + '/' + label + '/exec'
    L.ensure_dir(d)
    idx = []
    for i, x in enumerate(execs):
        L.wbytes('%s/%02d.out' % (d, i), x['Stdout'])
        L.wbytes('%s/%02d.err' % (d, i), x['Stderr'])
        idx.append({'Name': x['Name'], 'Argv': x['Argv'], 'Inner': x['Inner'], 'Command': x['Command'], 'Rc': x['Rc'], 'Instrument': x['Instrument'],
                    'Out': '%s/%02d.out' % (d, i), 'Err': '%s/%02d.err' % (d, i)})
    L.wbytes(d + '/index.json', (json.dumps(idx, ensure_ascii=True, indent=1) + '\n').encode('ascii'))


def load_execs(label):
    d = W + '/' + label + '/exec'
    idx = json.loads(L.rbytes(d + '/index.json').decode('ascii'))
    for x in idx:
        x['Stdout'] = L.rbytes(x['Out'])
        x['Stderr'] = L.rbytes(x['Err'])
    return idx


def prepare(name):
    L.ensure_dir(W)
    L.ensure_pins()
    T = W + '/c-' + name
    L.clone(T)
    p1 = L.p1(T, DECISION)
    p2 = L.p2(T, 'pwsh')
    inp = W + '/c-' + name + '-in'
    L.rmtree(inp)
    H, deleg, handoff, k = L.build_inputs(T, inp)
    info = {'T': T, 'Head': L.gout(T, 'rev-parse', 'HEAD'), 'HeadIsTip': L.gout(T, 'rev-parse', 'HEAD') == L.TIP,
            'P1': {'ReturnedId': p1['ReturnedId'], 'IdEqualsPinned': p1['IdEqualsPinned'], 'OnlyNewLooseObject': p1['OnlyNewLooseObject'], 'Held': p1['Held']},
            'P2': {'Launch': p2['Launch'], 'FailedSteps': p2['FailedSteps']}}
    if not (p1['Held'] and p2['Launch']):
        raise SystemExit('P1/P2 failed on ' + T)
    return T, inp, H, deleg, handoff, k, info


def pick(runs, names):
    d = dict(runs)
    return [(n, d[n]) for n in names]


def prim_reads(T, current):
    return [('Identity', 'git rev-parse HEAD') if current is None else ('Identity', 'git cat-file -t ' + current),
            ('Scope', 'git diff --name-only --no-renames %s..%s' % (L.BASE, current or L.HEADX)),
            ('CleanTree', 'git status --porcelain'),
            ('Trailer', 'git log --format=%%H %s..%s' % (L.BASE, current or L.HEADX)),
            ('Authority', 'git rev-parse %s:%s' % (L.MAIN, L.MAP))]


def case(group, name, what, expected, obs, holds, **extra):
    d = {'Group': group, 'Case': name, 'What': what, 'Expected': expected, 'Observed': obs, 'Holds': bool(holds)}
    d.update(extra)
    print('%-8s %-16s holds=%s %s' % (group, name, bool(holds), (obs.get('Decision'), mm_of(obs)) if isinstance(obs, dict) and 'Checks' in obs else ''))
    return d


def mm_of(obs):
    return sorted(k for k, v in obs.get('Checks', {}).items() if v == 'MISMATCH')


def na_of(obs):
    return sorted(k for k, v in obs.get('Checks', {}).items() if v == 'NOT_APPLICABLE')


def req_brief(p):
    q = json.loads(L.rbytes(p).decode('utf-8'))
    return {'Path': p, 'Sha256': X.sha_file(p), 'Runs': len(q.get('Runs') or []), 'VerificationIncluded': 'Verification' in q,
            'NotApplicable': sorted((q.get('NotApplicable') or {}).keys()), 'NotApplicableReasons': q.get('NotApplicable') or {}}


def write_group(name, obj):
    p, h = L.write_result(name, obj)
    print('wrote', p, h)


def cv_save(work, name, cv):
    p = work + '/' + name
    L.wjson_local(p, cv)
    return p


def relay_save(work, name, sess, cvp, run_id):
    rr = X.relay_record(sess, cvp, run_id)
    p = work + '/' + name
    L.wjson_local(p, rr)
    return p, X.validate('relay-record.v2.schema.json', rr)


# ------------------------------------------------------------------ T1 (probe 4) and the shared T1 context
def group_t1():
    L.ensure_dir(W)
    pins = L.ensure_pins()
    vers = L.versions()
    T, inp, H, deleg, handoff, k, info = prepare('t1')
    closure = L.build_closure(T, inp, H, k)
    runs = L.runs_for(T, inp, H)
    sess, execs = X.verify_session('s-t1', T, runs, prim_reads(T, None))
    save_execs('s-t1', execs)
    work = W + '/s-t1'
    rec, rruns = X.reconstruct_into(sess['LogPath'], work + '/env')
    if not rec['Reconstructible'] or len(rruns) != 13:
        raise SystemExit('T1 log not reconstructible: ' + json.dumps(rec['Problems']))
    F = X.facts_by_cmd(rruns)
    cv = X.cv_t1(F, deleg, handoff, sess['Primary'])
    cvp = cv_save(work, 'controller-output.json', cv)
    relayp, relay_sv = relay_save(work, 'relay-facts.json', sess, cvp, L.CV_RUN)
    consts = X.consts_for(T, closure, relayp)
    dp, hp = inp + '/deleg.json', inp + '/handoff.json'
    req, dec = TB.construct(cvp, dp, hp, rruns, consts)
    reqp = X.write_request(work + '/request-r3.json', req)
    obs = X.accept_and_table(T, reqp, cvp, dp, hp, consts, rec['InstrumentExecutions'], rec['Runs'])
    sv = {'delegation.v2': X.validate('delegation.v2.schema.json', deleg), 'worker-handoff': X.validate('worker-handoff.schema.json', handoff),
          'controller-verification.v2': X.validate('controller-verification.v2.schema.json', cv), 'relay-record.v2': relay_sv}
    # round 2 (custodied result in the I-62 worktree, and its request/output files in D:\r62-a6r2, read only)
    r2 = json.loads(L.rbytes(R2_RESULT).decode('ascii'))['Test3']['Acceptor']
    r2rows = [(x['Check'], x['Controller'], x['AcceptorExpects'], x['Status']) for x in r2['R4Rows']]
    myrows = [(x['Check'], x['Controller'], x['AcceptorExpects'], x['Status']) for x in obs['R4Rows']]
    same_as_r2 = obs['Checks'] == r2['Checks'] and obs['Decision'] == r2['Decision'] and obs['Rc'] == r2['Rc'] and myrows == r2rows
    out = {'Schema': 'r3-bench/p4-t1', 'Versions': vers, 'Pins': pins, 'Clone': info, 'Inputs': H, 'SchemaValidation': sv,
           'SchemaValid': all(not v for v in sv.values()),
           'Session': {k2: sess[k2] for k2 in ('LogPath', 'LogSha256', 'CessionStartUtc', 'CessionEndUtc', 'ZeroWriteToolRuns', 'Executions')},
           'Reconstruction': {k2: rec[k2] for k2 in ('Complete', 'CompletenessProblems', 'CommandExecutions', 'InstrumentExecutions', 'Reconstructible', 'Problems')},
           'ReconstructedRuns': [{'Seq': r['Seq'], 'Argv0': r['Argv'][0], 'ExitCode': r['ExitCode'], 'EnvelopeSha256': r['EnvelopeSha256'],
                                  'NoiseLines': r['NoiseLines']} for r in rec['Runs']],
           'ControllerChecks': dict((c, cv['Checks'][c]['Result']) for c in X.ORDER),
           'Coherence169': X.coherence_169(cv, set()),
           'ControllerOutput': {'Path': cvp, 'Sha256': X.sha_file(cvp)}, 'RelayFacts': {'Path': relayp, 'Sha256': X.sha_file(relayp)},
           'Closure': {'Path': closure, 'Sha256': X.sha_file(closure)}}
    cases = []
    cases.append(case('T1', 'T1-r3', 'Solicitud T1 construida con la regla de la ronda 3 (Runs reconstruidas del registro Codex) y aceptante fijado',
                      'R4 incluido por la regla (ningun not_run entre Identity/Scope/CleanTree/Trailer/FreeText); ACCEPT_FACTS rc 0; R4 MATCH; ningun NOT_APPLICABLE; R8 MANUAL; tabla aceptada; mismos estados, decision, rc y filas R4 que la ronda 2 (r4-t1.json Test3)',
                      obs, dec['Include'] and obs['Decision'] == 'ACCEPT_FACTS' and obs['Checks'].get('R4') == 'MATCH' and not na_of(obs)
                      and obs['Table']['Accepted'] and same_as_r2,
                      Request=req_brief(reqp), R4Rule={'Include': dec['Include'], 'Cause': dec['Cause'], 'NotRun': dec['NotRun']},
                      Round2={'Checks': r2['Checks'], 'Decision': r2['Decision'], 'Rc': r2['Rc'], 'R4Rows': r2rows}, SameAsRound2=same_as_r2))
    # the round-2 request itself under the round-3 table (static: files in D:\r62-a6r2 read only, no acceptor re-run)
    r2consts = {'IgnorePath': L.IGN, 'TermsRev': L.MAIN, 'MainSha': L.MAIN}
    tab2 = TB.evaluate(R2W + '/request.json', r2['Rc'], L.rbytes(R2W + '/request.acceptor-stdout.json'), R2W + '/controller-output.json',
                       R2W + '/in/deleg.json', R2W + '/in/handoff.json', r2consts, executions=13, reconstructed=None)
    wt_inputs = os.path.join(L.WT, 'docs', 'automation', 'evidence', 'I-62-A6', 'r2', 'r1r4', 'inputs-t1')
    ties = dict((n, X.sha_file(os.path.join(wt_inputs, n)) == X.sha_file(src)) for n, src in
                (('request.json', R2W + '/request.json'), ('controller-output.json', R2W + '/controller-output.json'),
                 ('deleg.json', R2W + '/in/deleg.json'), ('handoff.json', R2W + '/in/handoff.json'), ('relay-facts.json', R2W + '/relay-facts.json')))
    cases.append(case('T1', 'T1-r2-static', 'La solicitud T1 de la ronda 2 (D:\\r62-a6r2, solo lectura) y su salida del aceptante evaluadas con table_r3 (sin reejecutar)',
                      'la construccion recalculada incluye Verification (ningun not_run entre las cinco) y coincide con la solicitud de la ronda 2; tabla aceptada',
                      {'Table': tab2, 'Decision': tab2['Decision'], 'Checks': tab2['Statuses']}, tab2['Accepted'],
                      CustodiedCopiesEqualToWorkFiles=ties,
                      Note='Runs no se contrastan con un registro Codex: la ronda 2 uso un JSONL propio del banco'))
    out['Cases'] = cases
    write_group('p4-t1.json', out)
    save_state('t1', {'T': T, 'Inp': inp, 'Closure': closure, 'Controller': cvp, 'Relay': relayp, 'Consts': consts, 'Runs': rruns,
                      'Reconstruction': rec, 'Delegation': dp, 'Handoff': hp, 'Request': reqp, 'Checks': obs['Checks'], 'Decision': obs['Decision'],
                      'Rc': obs['Rc'], 'LogPath': sess['LogPath']})


# ------------------------------------------------------------------ probe 1: ZERO
def group_zero():
    st = load_state('t1')
    T, consts, cvp, dp, hp = st['T'], st['Consts'], st['Controller'], st['Delegation'], st['Handoff']
    work = W + '/s-zero'
    L.ensure_dir(work)
    execs = [x for x in load_execs('s-t1') if not x['Instrument']]
    lg = X.log_from_execs(execs)
    logp = work + '/session-events.jsonl'
    L.wbytes(logp, lg.data())
    rec = R.reconstruct(L.rbytes(logp), L.B)
    cases = [case('ZERO', 'ZERO-log', 'Registro Codex de una invocacion VERIFY sin ejecuciones del instrumento (las 5 lecturas Git primarias reales de T1)',
                  'registro completo; 0 ejecuciones del instrumento -> la regla no construye solicitud ni ejecuta el aceptante',
                  {k: rec[k] for k in ('Complete', 'CommandExecutions', 'InstrumentExecutions', 'Reconstructible', 'Problems')},
                  rec['Complete'] and rec['InstrumentExecutions'] == 0 and rec['CommandExecutions'] == 5)]
    # ZERO-a: mechanical construction with Runs = []
    req, dec = TB.construct(cvp, dp, hp, [], consts)
    pa = X.write_request(work + '/request-zero-a.json', req)
    oa = X.accept_and_table(T, pa, cvp, dp, hp, consts, executions=0, reconstructed=[])
    cases.append(case('ZERO', 'ZERO-a', 'Runs = [] con la construccion mecanica: Verification T1, Closure, RelayFacts (R5/R7 NA por su condicion de datos)',
                      'REJECT_FACTS; R3 y R9 MISMATCH (R3: sin datos y no dispensable; R9: MISSING_INPUT sin sobres); el resto MATCH/NA/MANUAL; tabla no aceptada',
                      oa, oa['Decision'] == 'REJECT_FACTS' and mm_of(oa) == ['R3', 'R9'] and not oa['Table']['Accepted'], Request=req_brief(pa)))
    # ZERO-b: every waivable check declared NA, no Closure, no Verification, no RelayFacts
    reqb = {'Schema': TB.REQUEST_SCHEMA, 'Repo': consts['Repo'], 'Tool': consts['Tool'], 'Interpreter': consts['Interpreter'], 'Runs': [],
            'NotApplicable': {'R4': 'ZERO-b', 'R5': 'ZERO-b', 'R6': 'ZERO-b', 'R7': 'ZERO-b', 'R10': 'ZERO-b'}}
    pb = X.write_request(work + '/request-zero-b.json', reqb)
    ob = X.accept_and_table(T, pb, cvp, dp, hp, consts, executions=0, reconstructed=[])
    cases.append(case('ZERO', 'ZERO-b', 'Runs = [], NotApplicable en R4 R5 R6 R7 R10, sin Closure ni Verification ni RelayFacts',
                      'REJECT_FACTS; R3 y R9 MISMATCH (sin datos; no son dispensables); R4 R5 R6 R7 R10 NOT_APPLICABLE; tabla no aceptada',
                      ob, ob['Decision'] == 'REJECT_FACTS' and mm_of(ob) == ['R3', 'R9'] and na_of(ob) == ['R10', 'R4', 'R5', 'R6', 'R7']
                      and not ob['Table']['Accepted'], Request=req_brief(pb)))
    # ZERO-c: also NA on R3 and R9 (not waivable)
    reqc = dict(reqb)
    reqc['NotApplicable'] = dict(reqb['NotApplicable'], R3='ZERO-c', R9='ZERO-c')
    pc = X.write_request(work + '/request-zero-c.json', reqc)
    oc = X.accept_and_table(T, pc, cvp, dp, hp, consts, executions=0, reconstructed=[])
    cases.append(case('ZERO', 'ZERO-c', 'ZERO-b mas NotApplicable en R3 y R9',
                      'REJECT_FACTS; R3 y R9 MISMATCH con "this check cannot be declared not applicable"; tabla no aceptada',
                      oc, oc['Decision'] == 'REJECT_FACTS' and mm_of(oc) == ['R3', 'R9']
                      and all('cannot be declared not applicable' in (oc['MismatchDetails'].get(r) or '') for r in ('R3', 'R9'))
                      and not oc['Table']['Accepted'], Request=req_brief(pc)))
    write_group('p1-zero.json', {'Schema': 'r3-bench/p1-zero', 'Log': {'Path': logp, 'Sha256': X.sha_file(logp)}, 'Cases': cases})


# ------------------------------------------------------------------ probe 2: N4 (delivery absent)
NOIN_N4 = 'la comprobacion Handoff esta en fail (entrega ausente: no hay CurrentSha ni texto de la entrega)'


def cv_n4(F, deleg, handoff_path, prim):
    auth_ok, defects, ar = X.authority_from(F)
    t = prim['Handoff']
    checks = {
        'Termination': {'Result': 'pass', 'Evidence': 'registro de la sesion (sintetico, banco r3): trabajo del Worker COMPLETED, muerte confirmada'},
        'Handoff': {'Result': 'fail', 'Evidence': 'entrega ausente en ' + handoff_path + ' (primaria: ' + t[0] + ' -> ' + t[1].strip() + ')'},
        'Authority': {'Result': X.res(auth_ok), 'Evidence': X._ev(F, 'clausemap', 'defectos del mapa: ' + (','.join(defects) or 'ninguno'))},
        'Contract': {'Result': X.res(X.contract_from(F)), 'Evidence': X._ev(F, 'contract')},
        'Identity': X.nr_noinput(NOIN_N4), 'Remote': X.nr_noinput(NOIN_N4), 'Scope': X.nr_noinput(NOIN_N4),
        'CleanTree': {'Result': X.res(X.cleantree_from(F)), 'Evidence': X._ev(F, 'cleantree')},
        'Ci': X.nr_noinput(NOIN_N4, True), 'Tests': X.nr_noinput(NOIN_N4, True), 'Trailer': X.nr_noinput(NOIN_N4),
        'Routing': X.nr_bench('sin modelo'), 'FreeText': X.nr_noinput(NOIN_N4),
        'Denials': X.nr_bench('sin eventos ni transcripcion del Worker'),
    }
    findings = ['Salida sintetica del banco r3 para N4 (entrega ausente): Controller simulado por la supervision.',
                'Disposition BLOCKED segun el modo de fallo "Entrega ausente" de AP 16.9; Authority (fixture con defectos del mapa) tiene fila STOP: ver Coherence169.']
    return X.cv_doc(checks, ar, 'BLOCKED', 'EXECUTION_BLOCKED', findings, 'R20261013T000000Z-ad04', L.WORK_RUN, deleg['RunId'])


def group_n4():
    T, inp, H, deleg, handoff, k, info = prepare('n4')
    hp = inp + '/handoff.json'
    h_sha = H['handoff.json']
    os.remove(hp)
    closure = L.build_closure(T, inp, H, k)
    cl = json.loads(L.rbytes(closure).decode('utf-8'))
    cl['Files'] = [f for f in cl['Files'] if f['Name'] != 'H']
    L.wjson_local(closure, cl)
    allr = dict(L.runs_for(T, inp, H))
    hashes_main = ['hashes', '--repo', T, '--rev', L.MAIN, '--paths', 'docs/AUTOMATION_PLAN.md', 'src/Fixture.Lib/Calculator.cs']
    runs = [('contract', allr['contract']), ('hashes-main', hashes_main), ('cleantree', allr['cleantree']), ('read-text-1', allr['read-text-1']),
            ('read-text-73', allr['read-text-73']), ('read-json', allr['read-json']), ('clausemap', allr['clausemap'])]
    if L.git(T, 'cat-file', '-e', X.NX).returncode == 0:
        raise SystemExit('NX exists')
    prim = [('Handoff', "Test-Path '" + hp + "'"), ('CleanTree', 'git status --porcelain'), ('Authority', 'git rev-parse %s:%s' % (L.MAIN, L.MAP))]
    sess, execs = X.verify_session('s-n4', T, runs, prim)
    work = W + '/s-n4'
    rec, rruns = X.reconstruct_into(sess['LogPath'], work + '/env')
    if not rec['Reconstructible']:
        raise SystemExit('N4 log not reconstructible ' + json.dumps(rec['Problems']))
    F = X.facts_by_cmd(rruns)
    cv = cv_n4(F, deleg, hp, sess['Primary'])
    cvp = cv_save(work, 'controller-output.json', cv)
    relayp, relay_sv = relay_save(work, 'relay-facts.json', sess, cvp, 'R20261013T000000Z-ad04')
    consts = X.consts_for(T, closure, relayp)
    dp = inp + '/deleg.json'
    coh = X.coherence_169(cv, {'Identity', 'Remote', 'Scope', 'Ci', 'Tests', 'Trailer', 'FreeText'}, handoff_absent=True)
    cases = []
    # (a) round-2 construction: Verification pointing at the absent delivery
    ra = X.round2_request(consts, rruns, cvp, dp, hp, h_sha)
    pa = X.write_request(work + '/request-a-r2.json', ra)
    oa = X.accept_and_table(T, pa, cvp, dp, hp, consts, rec['InstrumentExecutions'], rec['Runs'])
    cases.append(case('N4', 'N4-a', 'Construccion de la ronda 2: Verification incluida con la entrega ausente (SHA-256 de la entrega que se habria escrito)',
                      'REJECT_FACTS; R4 MISMATCH (MISSING_INPUT: Verification.Handoff ilegible); tabla no aceptada',
                      oa, oa['Decision'] == 'REJECT_FACTS' and mm_of(oa) == ['R4'] and 'MISSING_INPUT' in (oa['MismatchDetails'].get('R4') or '')
                      and not oa['Table']['Accepted'], Request=req_brief(pa)))
    # (b) Verification omitted without NotApplicable.R4
    rb, _ = TB.construct(cvp, dp, hp, rruns, consts)
    rb.pop('Verification', None)
    na = dict(rb.get('NotApplicable') or {})
    na.pop('R4', None)
    if na:
        rb['NotApplicable'] = na
    else:
        rb.pop('NotApplicable', None)
    pb = X.write_request(work + '/request-b-noR4.json', rb)
    ob = X.accept_and_table(T, pb, cvp, dp, hp, consts, rec['InstrumentExecutions'], rec['Runs'])
    cases.append(case('N4', 'N4-b', 'Verification omitida sin declarar NotApplicable.R4',
                      'REJECT_FACTS; R4 MISMATCH (sin datos y sin declaracion); tabla no aceptada',
                      ob, ob['Decision'] == 'REJECT_FACTS' and mm_of(ob) == ['R4'] and not ob['Table']['Accepted'], Request=req_brief(pb)))
    # (c) round-3 construction
    rc_, dec = TB.construct(cvp, dp, hp, rruns, consts)
    pc = X.write_request(work + '/request-c-r3.json', rc_)
    oc = X.accept_and_table(T, pc, cvp, dp, hp, consts, rec['InstrumentExecutions'], rec['Runs'])
    cases.append(case('N4', 'N4-c', 'Construccion de la ronda 3: entrega ausente y Identity/Scope/Trailer/FreeText en not_run -> Verification omitida, NotApplicable.R4 con la causa',
                      'ACCEPT_FACTS rc 0; R4 NOT_APPLICABLE; R5 R6 R7 MATCH (hashes, clausemap, contract en Runs); tabla aceptada',
                      oc, oc['Decision'] == 'ACCEPT_FACTS' and oc['Checks'].get('R4') == 'NOT_APPLICABLE' and na_of(oc) == ['R4']
                      and oc['Table']['Accepted'], Request=req_brief(pc), R4Rule={'Include': dec['Include'], 'Cause': dec['Cause'], 'NotRun': dec['NotRun'], 'Missing': dec['Missing']}))
    # (d) one more instrument run with an error (identity with a nonexistent --current), in its own VERIFY session
    err = ['identity', '--repo', T, '--branch', L.BR, '--base', L.BASE, '--current', X.NX, '--red', L.RED]
    sess_d, execs_d = X.verify_session('s-n4d', T, runs + [('identity-nx', err)], prim)
    rec_d, rruns_d = X.reconstruct_into(sess_d['LogPath'], W + '/s-n4d/env')
    relayd, _ = relay_save(W + '/s-n4d', 'relay-facts.json', sess_d, cvp, 'R20261013T000000Z-ad04')
    consts_d = X.consts_for(T, closure, relayd)
    if rec_d['Reconstructible']:
        rd, _ = TB.construct(cvp, dp, hp, rruns_d, consts_d)
        pd = X.write_request(W + '/s-n4d/request-d-r3.json', rd)
        od = X.accept_and_table(T, pd, cvp, dp, hp, consts_d, rec_d['InstrumentExecutions'], rec_d['Runs'])
    else:
        pd, od = None, {'Decision': None, 'Checks': {}, 'Table': {'Accepted': False}, 'Reconstruction': rec_d['Problems']}
    erun = [r for r in rec_d['Runs'] if r.get('Argv', [''])[0] == 'identity']
    cases.append(case('N4', 'N4-d', 'N4-c mas una ejecucion del instrumento con error (identity --current inexistente), reconstruida del registro como corrida con error',
                      'la reconstruccion la conserva como corrida con error (exit != 0, Error del sobre); REJECT_FACTS; tabla no aceptada',
                      od, rec_d['Reconstructible'] and bool(erun) and erun[0]['ErrorRun'] and od['Decision'] == 'REJECT_FACTS' and not od['Table']['Accepted'],
                      Request=req_brief(pd) if pd else None, ErrorRun=erun,
                      PwshExitForErrorRun=[x['Rc'] for x in execs_d if x['Name'] == 'identity-nx']))
    sv = {'delegation.v2': X.validate('delegation.v2.schema.json', deleg), 'controller-verification.v2': X.validate('controller-verification.v2.schema.json', cv),
          'relay-record.v2': relay_sv}
    write_group('p2-n4.json', {'Schema': 'r3-bench/p2-n4', 'Clone': info, 'HandoffRemoved': {'Path': hp, 'Exists': os.path.exists(hp), 'ShaBeforeRemoval': h_sha},
                               'SchemaValidation': sv, 'SchemaValid': all(not v for v in sv.values()),
                               'Session': {k2: sess[k2] for k2 in ('LogPath', 'LogSha256', 'ZeroWriteToolRuns', 'Executions')},
                               'Reconstruction': {k2: rec[k2] for k2 in ('Complete', 'CommandExecutions', 'InstrumentExecutions', 'Reconstructible', 'Problems')},
                               'ControllerChecks': dict((c, cv['Checks'][c]['Result']) for c in X.ORDER),
                               'Classification': [cv['Classification'], cv['Disposition'], cv['FailureClass']], 'Coherence169': coh,
                               'ControllerOutput': {'Path': cvp, 'Sha256': X.sha_file(cvp)}, 'Cases': cases})


# ------------------------------------------------------------------ probe 3: nc1 (nonexistent CurrentSha)
NOIN_NC1 = 'la comprobacion Identity esta en fail (CurrentSha no es un commit existente)'


def cv_nc1(F, deleg, handoff, prim, variant, run_id):
    auth_ok, defects, ar = X.authority_from(F)
    pi, ps, pt = prim['Identity'], prim['Scope'], prim['Trailer']
    checks = {
        'Termination': {'Result': 'pass', 'Evidence': 'registro de la sesion (sintetico, banco r3): trabajo del Worker COMPLETED, muerte confirmada'},
        'Handoff': {'Result': X.res(X.handoff_from(F, deleg)), 'Evidence': X._ev(F, 'handoff')},
        'Authority': {'Result': X.res(auth_ok), 'Evidence': X._ev(F, 'clausemap', 'defectos del mapa: ' + (','.join(defects) or 'ninguno'))},
        'Contract': {'Result': X.res(X.contract_from(F)), 'Evidence': X._ev(F, 'contract')},
        'Identity': {'Result': 'fail', 'Evidence': 'CurrentSha %s no es un commit existente (primaria: %s -> rc %d)' % (handoff['CurrentSha'], pi[0], pi[2])},
        'Remote': X.nr_bench('sin registro de ls-remote del relevo en la salida del Controller'),
        'Scope': {'Result': 'fail', 'Evidence': 'git diff BaseSha..CurrentSha no resoluble (primaria: %s -> rc %d)' % (ps[0], ps[2])},
        'CleanTree': {'Result': X.res(X.cleantree_from(F)), 'Evidence': X._ev(F, 'cleantree')} if 'cleantree' in F else None,
        'Ci': X.nr_noinput(NOIN_NC1, True), 'Tests': X.nr_noinput(NOIN_NC1, True),
        'Trailer': {'Result': 'fail', 'Evidence': 'rango BaseSha..CurrentSha no resoluble (primaria: %s -> rc %d)' % (pt[0], pt[2])},
        'Routing': X.nr_bench('sin modelo'),
        'FreeText': {'Result': X.res(not F['freetext'][0]['Matches']), 'Evidence': X._ev(F, 'freetext', 'coincidencias=%d' % len(F['freetext'][0]['Matches']))},
        'Denials': X.nr_bench('sin eventos ni transcripcion del Worker'),
    }
    no_input = {'Ci', 'Tests'}
    if variant == 'V2':
        checks['Scope'] = X.nr_noinput(NOIN_NC1)
        checks['Trailer'] = X.nr_noinput(NOIN_NC1)
        no_input |= {'Scope', 'Trailer'}
    if variant == 'V3':
        checks['CleanTree'] = {'Result': 'not_run', 'Evidence': 'no ejecutada por el Controller (variante sintetica V3); not_run con la disposicion de su fila (AP 16.9)'}
    findings = ['Salida sintetica del banco r3 para nc1 (CurrentSha inexistente), variante ' + variant + '.',
                'FailureClass = Authority: el mapa del fixture tiene defectos (MV-4) y Authority (#3) precede a Identity (#5) en el orden fijo.']
    return X.cv_doc(checks, ar, 'STOP', 'EXECUTION_BLOCKED', findings, run_id, handoff['RunId'], deleg['RunId']), no_input


def group_nc1():
    T, inp, H, deleg, handoff, k, info = prepare('nc1')
    if L.git(T, 'cat-file', '-e', X.NX).returncode == 0:
        raise SystemExit('NX exists')
    hp, dp = inp + '/handoff.json', inp + '/deleg.json'
    handoff['CurrentSha'] = X.NX
    L.wjson_local(hp, handoff)
    H['handoff.json'] = X.sha_file(hp)
    closure = L.build_closure(T, inp, H, k)
    allr = L.runs_for(T, inp, H)
    names = ['contract', 'hashes', 'cleantree', 'read-text-1', 'read-text-73', 'read-json', 'trx', 'handoff', 'freetext', 'clausemap']
    runs = pick(allr, names)
    prim = prim_reads(T, X.NX)
    sess, execs = X.verify_session('s-nc1', T, runs, prim)
    work = W + '/s-nc1'
    rec, rruns = X.reconstruct_into(sess['LogPath'], work + '/env')
    if not rec['Reconstructible']:
        raise SystemExit('nc1 log not reconstructible ' + json.dumps(rec['Problems']))
    F = X.facts_by_cmd(rruns)
    out = {'Schema': 'r3-bench/p3-nc1', 'Clone': info, 'Nx': X.NX, 'HandoffSha256': H['handoff.json'],
           'HandoffSchema': X.validate('worker-handoff.schema.json', handoff), 'Variants': {}}
    cases = []
    ctx = {}
    for variant, run_id in (('V1', 'R20261013T000000Z-ac11'), ('V2', 'R20261013T000000Z-ac12')):
        cv, noin = cv_nc1(F, deleg, handoff, sess['Primary'], variant, run_id)
        cvp = cv_save(work, 'controller-output-%s.json' % variant, cv)
        relayp, rsv = relay_save(work, 'relay-facts-%s.json' % variant, sess, cvp, run_id)
        consts = X.consts_for(T, closure, relayp)
        ctx[variant] = (cvp, consts)
        out['Variants'][variant] = {'ControllerChecks': dict((c, cv['Checks'][c]['Result']) for c in X.ORDER),
                                    'Classification': [cv['Classification'], cv['Disposition'], cv['FailureClass']],
                                    'Coherence169': X.coherence_169(cv, noin), 'Schema': X.validate('controller-verification.v2.schema.json', cv),
                                    'RelaySchema': rsv, 'ControllerOutput': {'Path': cvp, 'Sha256': X.sha_file(cvp)}, 'Runs': len(rruns)}
    # V1 round 3
    cvp, consts = ctx['V1']
    r1, d1 = TB.construct(cvp, dp, hp, rruns, consts)
    p1 = X.write_request(work + '/request-V1-r3.json', r1)
    o1 = X.accept_and_table(T, p1, cvp, dp, hp, consts, rec['InstrumentExecutions'], rec['Runs'])
    cases.append(case('nc1', 'V1-r3', 'V1: las cinco evaluadas (Identity fail, Scope fail, CleanTree pass, Trailer fail, FreeText fail) -> la ronda 3 incluye Verification',
                      'ACCEPT_FACTS; R4 MATCH; tabla aceptada', o1,
                      d1['Include'] and o1['Decision'] == 'ACCEPT_FACTS' and o1['Checks'].get('R4') == 'MATCH' and o1['Table']['Accepted'],
                      Request=req_brief(p1)))
    # V2 round 3 and round 2
    cvp, consts = ctx['V2']
    r2_, d2 = TB.construct(cvp, dp, hp, rruns, consts)
    p2 = X.write_request(work + '/request-V2-r3.json', r2_)
    o2 = X.accept_and_table(T, p2, cvp, dp, hp, consts, rec['InstrumentExecutions'], rec['Runs'])
    cases.append(case('nc1', 'V2-r3', 'V2: Scope y Trailer not_run (sin entrada tras Identity fail) -> la ronda 3 omite Verification, NotApplicable.R4',
                      'ACCEPT_FACTS; R4 NOT_APPLICABLE; tabla aceptada', o2,
                      (not d2['Include']) and o2['Decision'] == 'ACCEPT_FACTS' and o2['Checks'].get('R4') == 'NOT_APPLICABLE' and o2['Table']['Accepted'],
                      Request=req_brief(p2), R4Rule={'Include': d2['Include'], 'Cause': d2['Cause']}))
    p2b = X.write_request(work + '/request-V2-r2.json', X.round2_request(consts, rruns, cvp, dp, hp, H['handoff.json']))
    o2b = X.accept_and_table(T, p2b, cvp, dp, hp, consts, rec['InstrumentExecutions'], rec['Runs'])
    rows = dict((r['Check'], r) for r in (o2b['R4Rows'] or []) if isinstance(r, dict))
    cases.append(case('nc1', 'V2-r2', 'V2 con la construccion de la ronda 2 (Verification incluida): filas R4 de not_run frente a "not pass" del aceptante',
                      'documentar: el aceptante trata not_run como "no pass"; con Scope/Trailer que el aceptante calcula "not pass", R4 MATCH y ACCEPT_FACTS; table_r3 la rechaza por construccion (Verification incluida pese a not_run)',
                      o2b, o2b['Decision'] == 'ACCEPT_FACTS' and o2b['Checks'].get('R4') == 'MATCH'
                      and rows.get('Scope', {}).get('Controller') == 'not_run' and rows.get('Scope', {}).get('AcceptorExpects') == 'not pass'
                      and not o2b['Table']['Accepted'], Request=req_brief(p2b)))
    # V3: CleanTree not_run (own session without the cleantree run)
    runs3 = [r for r in runs if r[0] != 'cleantree']
    sess3, execs3 = X.verify_session('s-nc1v3', T, runs3, prim)
    w3 = W + '/s-nc1v3'
    rec3, rruns3 = X.reconstruct_into(sess3['LogPath'], w3 + '/env')
    F3 = X.facts_by_cmd(rruns3)
    cv3, noin3 = cv_nc1(F3, deleg, handoff, sess3['Primary'], 'V3', 'R20261013T000000Z-ac13')
    cvp3 = cv_save(w3, 'controller-output-V3.json', cv3)
    relay3, rsv3 = relay_save(w3, 'relay-facts-V3.json', sess3, cvp3, 'R20261013T000000Z-ac13')
    consts3 = X.consts_for(T, closure, relay3)
    out['Variants']['V3'] = {'ControllerChecks': dict((c, cv3['Checks'][c]['Result']) for c in X.ORDER),
                             'Classification': [cv3['Classification'], cv3['Disposition'], cv3['FailureClass']],
                             'Coherence169': X.coherence_169(cv3, noin3), 'Schema': X.validate('controller-verification.v2.schema.json', cv3),
                             'RelaySchema': rsv3, 'ControllerOutput': {'Path': cvp3, 'Sha256': X.sha_file(cvp3)}, 'Runs': len(rruns3)}
    p3a = X.write_request(w3 + '/request-V3-r2.json', X.round2_request(consts3, rruns3, cvp3, dp, hp, H['handoff.json']))
    o3a = X.accept_and_table(T, p3a, cvp3, dp, hp, consts3, rec3['InstrumentExecutions'], rec3['Runs'])
    cases.append(case('nc1', 'V3-r2', 'V3: CleanTree not_run (el aceptante calcula pass) con la construccion de la ronda 2',
                      'REJECT_FACTS; R4 MISMATCH (fila CleanTree: not_run frente a pass)', o3a,
                      o3a['Decision'] == 'REJECT_FACTS' and mm_of(o3a) == ['R4'] and not o3a['Table']['Accepted'], Request=req_brief(p3a)))
    r3b, d3 = TB.construct(cvp3, dp, hp, rruns3, consts3)
    p3b = X.write_request(w3 + '/request-V3-r3.json', r3b)
    o3b = X.accept_and_table(T, p3b, cvp3, dp, hp, consts3, rec3['InstrumentExecutions'], rec3['Runs'])
    cases.append(case('nc1', 'V3-r3', 'V3 con la construccion de la ronda 3 (CleanTree not_run -> Verification omitida)',
                      'ACCEPT_FACTS; R4 NOT_APPLICABLE; tabla aceptada', o3b,
                      (not d3['Include']) and o3b['Decision'] == 'ACCEPT_FACTS' and o3b['Checks'].get('R4') == 'NOT_APPLICABLE' and o3b['Table']['Accepted'],
                      Request=req_brief(p3b), R4Rule={'Include': d3['Include'], 'Cause': d3['Cause']}))
    # nc1-err: an instrument run on the nonexistent CurrentSha
    err = ['identity', '--repo', T, '--branch', L.BR, '--base', L.BASE, '--current', X.NX, '--red', L.RED]
    sess_e, execs_e = X.verify_session('s-nc1err', T, runs + [('identity-nx', err)], prim)
    we = W + '/s-nc1err'
    rec_e, rruns_e = X.reconstruct_into(sess_e['LogPath'], we + '/env')
    cvp1, _ = ctx['V1']
    relaye, _ = relay_save(we, 'relay-facts.json', sess_e, cvp1, 'R20261013T000000Z-ac11')
    consts_e = X.consts_for(T, closure, relaye)
    if rec_e['Reconstructible']:
        re_, _ = TB.construct(cvp1, dp, hp, rruns_e, consts_e)
        pe = X.write_request(we + '/request-err-r3.json', re_)
        oe = X.accept_and_table(T, pe, cvp1, dp, hp, consts_e, rec_e['InstrumentExecutions'], rec_e['Runs'])
    else:
        pe, oe = None, {'Decision': None, 'Checks': {}, 'Table': {'Accepted': False}, 'Reconstruction': rec_e['Problems']}
    erun = [r for r in rec_e['Runs'] if r.get('Argv', [''])[0] == 'identity']
    cases.append(case('nc1', 'nc1-err', 'V1 mas una ejecucion del instrumento sobre el CurrentSha inexistente (sobre de error)',
                      'corrida con error conservada por la reconstruccion; REJECT_FACTS; tabla no aceptada', oe,
                      rec_e['Reconstructible'] and bool(erun) and erun[0]['ErrorRun'] and oe['Decision'] == 'REJECT_FACTS' and not oe['Table']['Accepted'],
                      Request=req_brief(pe) if pe else None, ErrorRun=erun))
    out['Session'] = {k2: sess[k2] for k2 in ('LogPath', 'LogSha256', 'ZeroWriteToolRuns', 'Executions')}
    out['Reconstruction'] = {k2: rec[k2] for k2 in ('Complete', 'CommandExecutions', 'InstrumentExecutions', 'Reconstructible', 'Problems')}
    out['Cases'] = cases
    write_group('p3-nc1.json', out)


# ------------------------------------------------------------------ probe 5: request-construction mutants
def group_mutants():
    st = load_state('t1')
    T, consts, cvp, dp, hp, rruns, rec = st['T'], st['Consts'], st['Controller'], st['Delegation'], st['Handoff'], st['Runs'], st['Reconstruction']
    work = W + '/s-mut'
    L.ensure_dir(work)
    base, dec = TB.construct(cvp, dp, hp, rruns, consts)
    cases = []

    def run(name, req, what, expected, holds_fn, cv=cvp, recon=rec['Runs'], ex=rec['InstrumentExecutions']):
        p = X.write_request(work + '/request-' + name + '.json', req)
        o = X.accept_and_table(T, p, cv, dp, hp, consts, ex, recon)
        by_acc = o['Decision'] != 'ACCEPT_FACTS'
        by_tab = not o['Table']['Accepted']
        cases.append(case('MUT', name, what, expected, o, holds_fn(o) and (by_acc or by_tab), Request=req_brief(p),
                          CaughtByAcceptor=by_acc, CaughtByTable=bool(o['Table']['ConstructionViolations'] or o['Table']['AcceptanceReasons'])))

    m1 = json.loads(json.dumps(base))
    m1['NotApplicable'] = dict(m1.get('NotApplicable') or {}, R4='mutante M1')
    run('M1', m1, 'NotApplicable.R4 declarado con Verification incluida', 'aceptante: R4 MISMATCH ("declared not applicable but data was supplied"); tabla: violacion R4_RULE',
        lambda o: o['Checks'].get('R4') == 'MISMATCH' and any('R4_RULE' in v for v in o['Table']['ConstructionViolations']))
    m2 = json.loads(json.dumps(base))
    m2.pop('Verification')
    m2['NotApplicable'] = dict(m2.get('NotApplicable') or {}, R4='mutante M2')
    run('M2', m2, 'Verification omitida (con NotApplicable.R4) aunque la regla la exige', 'aceptante: ACCEPT_FACTS (R4 NOT_APPLICABLE); tabla: violacion R4_RULE -> no aceptada',
        lambda o: o['Decision'] == 'ACCEPT_FACTS' and not o['Table']['Accepted'] and any('R4_RULE' in v for v in o['Table']['ConstructionViolations']))
    m3 = json.loads(json.dumps(base))
    m3['NotApplicable'] = dict(m3.get('NotApplicable') or {}, R5='mutante M3')
    run('M3', m3, 'NotApplicable.R5 con identity/diff/scope/hashes en Runs', 'aceptante: R5 MISMATCH; tabla: violacion R5_DATA',
        lambda o: o['Checks'].get('R5') == 'MISMATCH' and any('R5_DATA' in v for v in o['Table']['ConstructionViolations']))
    m4 = json.loads(json.dumps(base))
    m4['NotApplicable'] = dict(m4.get('NotApplicable') or {}, R6='mutante M4')
    run('M4', m4, 'NotApplicable.R6 con clausemap en Runs', 'aceptante: R6 MISMATCH; tabla: violacion R6_DATA',
        lambda o: o['Checks'].get('R6') == 'MISMATCH' and any('R6_DATA' in v for v in o['Table']['ConstructionViolations']))
    # M5: synthetic EXECUTION_VERIFIED outputs
    cv = json.loads(L.rbytes(cvp).decode('utf-8'))
    allpass = json.loads(json.dumps(cv))
    for c in X.ORDER:
        allpass['Checks'][c]['Result'] = 'pass'
        if 'RedPart' in allpass['Checks'][c]:
            allpass['Checks'][c]['RedPart'] = 'not_applicable'
    allpass.update({'Classification': 'EXECUTION_VERIFIED', 'Disposition': 'NONE', 'FailureClass': 'NONE', 'TriggeredStopConditions': [],
                    'RunId': 'R20261013T000000Z-a5e5', 'Findings': ['Salida sintetica EXECUTION_VERIFIED del banco r3 (mutante M5): incoherente con los hechos a proposito.']})
    m5cv = json.loads(json.dumps(allpass))
    m5cv['Checks']['Scope'] = {'Result': 'not_run', 'Evidence': 'mutante M5: not_run con Classification EXECUTION_VERIFIED (incoherente con 16.9 a proposito)'}
    p5 = cv_save(work, 'controller-output-M5.json', m5cv)
    m5, _ = TB.construct(p5, dp, hp, rruns, consts)
    run('M5', m5, 'Salida sintetica EXECUTION_VERIFIED con Scope not_run -> la regla omite Verification (R4 NA)',
        'aceptante: ACCEPT_FACTS (R4 NOT_APPLICABLE); tabla: "EXECUTION_VERIFIED without R4 MATCH" -> no aceptada',
        lambda o: o['Decision'] == 'ACCEPT_FACTS' and o['Checks'].get('R4') == 'NOT_APPLICABLE' and not o['Table']['Accepted']
        and any('EXECUTION_VERIFIED' in v for v in o['Table']['AcceptanceReasons']), cv=p5)
    p5b = cv_save(work, 'controller-output-M5b.json', allpass)
    m5b, _ = TB.construct(p5b, dp, hp, rruns, consts)
    m5b.pop('Verification')
    m5b['NotApplicable'] = dict(m5b.get('NotApplicable') or {}, R4='mutante M5b')
    run('M5b', m5b, 'Salida sintetica EXECUTION_VERIFIED con las 14 en pass, solicitud con Verification omitida y NotApplicable.R4',
        'aceptante: ACCEPT_FACTS; tabla: R4_RULE y "EXECUTION_VERIFIED without R4 MATCH" -> no aceptada',
        lambda o: o['Decision'] == 'ACCEPT_FACTS' and not o['Table']['Accepted'] and any('R4_RULE' in v for v in o['Table']['ConstructionViolations'])
        and any('EXECUTION_VERIFIED' in v for v in o['Table']['AcceptanceReasons']), cv=p5b)
    m5c, _ = TB.construct(p5b, dp, hp, rruns, consts)
    run('M5c', m5c, 'La misma salida EXECUTION_VERIFIED (14 en pass) con la construccion de la ronda 3 (Verification incluida)',
        'aceptante: REJECT_FACTS con R4 MISMATCH (los hechos contradicen los pass); tabla no aceptada',
        lambda o: o['Decision'] == 'REJECT_FACTS' and 'R4' in mm_of(o), cv=p5b)
    # M6: Runs missing one execution that the session log records
    keep = [r for r in rruns if not (r['Argv'][0] == 'read-text' and '73-73' in r['Argv'])]
    m6, _ = TB.construct(cvp, dp, hp, keep, consts)
    run('M6', m6, 'Runs sin una ejecucion que el registro de sesion recoge (read-text 73-73): 12 corridas frente a 13 reconstruidas',
        'aceptante: ACCEPT_FACTS (no ve la omision); tabla: RUNS_VS_LOG (12 frente a 13) -> no aceptada',
        lambda o: o['Decision'] == 'ACCEPT_FACTS' and any('RUNS_VS_LOG' in v for v in o['Table']['ConstructionViolations']))
    write_group('p5-mutants.json', {'Schema': 'r3-bench/p5-mutants', 'BaseRequestR4Include': dec['Include'], 'Cases': cases})


# ------------------------------------------------------------------ probe 6: reconstruction from Codex-format events
def group_recon():
    st = load_state('t1')
    T, consts, cvp, dp, hp = st['T'], st['Consts'], st['Controller'], st['Delegation'], st['Handoff']
    execs = load_execs('s-t1')
    inst = [x for x in execs if x['Instrument']]
    work = W + '/s-recon'
    L.rmtree(work)
    L.ensure_dir(work)
    noise = X.profile_noise_line()
    ref = [(r['Argv'], r['EnvelopeSha256']) for r in st['Reconstruction']['Runs']]
    cases = []

    def build(name, mutate=None, noise_='', finish=True, raw_mut=None, extra=None):
        ex = execs + (extra or [])
        lg = X.log_from_execs(ex, noise=noise_, mutate=mutate, finish=finish)
        data = lg.data()
        if raw_mut:
            data = raw_mut(data, lg)
        p = work + '/' + name + '-events.jsonl'
        L.wbytes(p, data)
        rec = R.reconstruct(data, L.B, env_dir=work + '/' + name + '-env')
        return p, rec

    def brief(rec):
        return {k: rec[k] for k in ('Complete', 'CompletenessProblems', 'CommandExecutions', 'InstrumentExecutions', 'Reconstructible', 'Problems')}

    def accept(name, rec):
        runs = [{'Name': 'r%02d-%s' % (r['Seq'], r['Argv'][0]), 'Envelope': r['EnvelopePath'], 'Argv': r['Argv']} for r in rec['Runs']]
        req, _ = TB.construct(cvp, dp, hp, runs, consts)
        p = X.write_request(work + '/request-' + name + '.json', req)
        return X.accept_and_table(T, p, cvp, dp, hp, consts, rec['InstrumentExecutions'], rec['Runs']), p

    def not_rec(name, what, expect_sub, **kw):
        p, rec = build(name, **kw)
        ok = (not rec['Reconstructible']) and any(expect_sub in x for x in rec['Problems'] + rec['CompletenessProblems'])
        cases.append(case('RECON', name, what, 'no reconstruible (' + expect_sub + '); sin solicitud ni aceptante; resultado no aceptado',
                          dict(brief(rec), Accepted=False), ok, Log={'Path': p, 'Sha256': X.sha_file(p)}))

    def idx_of(argv0, lines=None):
        for i, x in enumerate(execs):
            if x['Instrument'] and x['Argv'][0] == argv0 and (lines is None or lines in x['Argv']):
                return i
        raise SystemExit('no exec ' + argv0)

    # OK and NOISE
    for name, nz in (('OK', ''), ('NOISE', noise)):
        p, rec = build(name, noise_=nz)
        same_env = rec['Reconstructible'] and [(r['Argv'], r['EnvelopeSha256']) for r in rec['Runs']] == ref
        o, rq = accept(name, rec) if rec['Reconstructible'] else ({'Decision': None, 'Checks': {}, 'Table': {'Accepted': False}}, None)
        eq = o['Checks'] == st['Checks'] and o['Decision'] == st['Decision']
        cases.append(case('RECON', name, 'Registro Codex sintetico con las 13 ejecuciones reales de T1' + (' y la linea de aviso de profile.ps1 (medida en el registro v5) delante de cada salida' if nz else ''),
                          '13 corridas reconstruidas, argv y sobres identicos a T1; mismo resultado del aceptante que T1 (= ronda 2); tabla aceptada',
                          dict(o, Reconstruction=brief(rec), NoiseLinesPerRun=[r.get('NoiseLines') for r in rec['Runs']]),
                          same_env and eq and o['Table']['Accepted'], Log={'Path': p, 'Sha256': X.sha_file(p)}, Request=req_brief(rq) if rq else None))
    k0 = idx_of('identity')

    def m_at(i, fn):
        return lambda j, rec: fn(rec) if j == i else rec

    def set_(rec, **kw):
        rec.update(kw)
        return rec

    not_rec('CRLF', 'El sobre de identity termina en CRLF', 'CRLF',
            mutate=m_at(k0, lambda r: set_(r, Aggregated=r['Aggregated'][:-1] + '\r\n')))
    not_rec('TRUNC', 'Salida de identity truncada a la mitad del sobre (sin LF final)', 'not LF-terminated',
            mutate=m_at(k0, lambda r: set_(r, Aggregated=r['Aggregated'][:len(r['Aggregated']) // 2])))
    not_rec('TRUNC-LF', 'Salida de identity truncada a la mitad del sobre y con LF final', 'not JSON',
            mutate=m_at(k0, lambda r: set_(r, Aggregated=r['Aggregated'][:len(r['Aggregated']) // 2] + '\n')))
    not_rec('TWO', 'Dos sobres en la salida de una orden (identity)', '2 candidate lines',
            mutate=m_at(k0, lambda r: set_(r, Aggregated=r['Aggregated'] + r['Aggregated'])))
    # FORM variants (the recorded command of identity rewritten; the output is the real one)
    a0 = execs[k0]['Argv']
    sq = X.pw_sq
    forms = [
        ('F-noI', 'sin -I', '& %s --no-replace-objects cat-file blob %s | & %s - %s' % (sq(X.GIT_EXE), L.B, sq(X.PY_EXE), ' '.join(a0)), 'missing -I'),
        ('F-noNRO', 'sin --no-replace-objects', '& %s cat-file blob %s | & %s -I - %s' % (sq(X.GIT_EXE), L.B, sq(X.PY_EXE), ' '.join(a0)), 'missing --no-replace-objects'),
        ('F-bare-P3r2', 'forma P3 de la ronda 2 (git y python por nombre, sin &)', 'git --no-replace-objects cat-file blob %s | python -I - %s' % (L.B, ' '.join(a0)), 'call operator'),
        ('F-bare-git', 'git por nombre desnudo (& git)', '& git --no-replace-objects cat-file blob %s | & %s -I - %s' % (L.B, sq(X.PY_EXE), ' '.join(a0)), 'bare name'),
        ('F-bare-py', 'interprete por nombre desnudo (& python)', '& %s --no-replace-objects cat-file blob %s | & python -I - %s' % (sq(X.GIT_EXE), L.B, ' '.join(a0)), 'bare name'),
        ('F-extra-stmt', 'sentencia extra al final (; Write-Output fin)', X.inner_cmd(a0) + '; Write-Output fin', 'not allowed'),
        ('F-lead-stmt', 'sentencia extra delante (Set-Location ...;)', 'Set-Location D:/r62-a6r3/r1; ' + X.inner_cmd(a0), 'not allowed'),
        ('F-redir', 'redireccion 2>&1', X.inner_cmd(a0) + ' 2>&1', 'not allowed'),
        ('F-redir-file', 'redireccion a archivo', X.inner_cmd(a0) + ' > D:/r62-a6r3/r1/x.json', 'not allowed'),
        ('F-var', 'variable como ejecutable ($g)', '& $g --no-replace-objects cat-file blob %s | & %s -I - %s' % (L.B, sq(X.PY_EXE), ' '.join(a0)), 'not allowed'),
        ('F-dq-expand', 'literal entre comillas dobles con expansion ($env:...)', '& "$env:LOCALAPPDATA\\Programs\\Git\\cmd\\git.exe" --no-replace-objects cat-file blob %s | & %s -I - %s' % (L.B, sq(X.PY_EXE), ' '.join(a0)), 'expansion'),
        ('F-second-pipe', 'segunda tuberia tras la herramienta', X.inner_cmd(a0) + ' | & ' + sq(X.PY_EXE) + ' -I -', 'extra pipeline'),
    ]
    for name, what, inner, sub in forms:
        not_rec(name, 'Forma: ' + what, sub, mutate=m_at(k0, lambda r, inner=inner: set_(r, Command=X.recorded_command(inner))))
    not_rec('F-outer-flags', 'Forma externa con -NoProfile antes de -Command', 'not the form',
            mutate=m_at(k0, lambda r: set_(r, Command=R.render_command([X.PWSH, '-NoProfile', '-Command', X.inner_cmd(a0)]))))
    not_rec('F-cmd-shell', 'Shell cmd.exe en lugar de pwsh', 'not the form',
            mutate=m_at(k0, lambda r: set_(r, Command=R.render_command([os.environ.get('ComSpec', r'C:\WINDOWS\system32\cmd.exe'), '/c', X.inner_cmd(a0)]))))
    blob_mention = {'Name': 'p2-like', 'Argv': None, 'Inner': 'git --no-replace-objects cat-file -t ' + L.B,
                    'Command': X.recorded_command('git --no-replace-objects cat-file -t ' + L.B), 'Rc': 0, 'Stdout': b'blob\n', 'Stderr': b'', 'Instrument': False}
    not_rec('F-blob-mention', 'Orden no instrumental que menciona el blob fijado (comprobacion tipo P2 dentro de la sesion)', 'call operator', extra=[blob_mention])
    # positive control: interpreter in a pwsh double-quoted literal without expansions
    inner_dq = '& %s --no-replace-objects cat-file blob %s | & "%s" -I - %s' % (sq(X.GIT_EXE), L.B, X.PY_EXE, ' '.join(a0))
    p, rec = build('P-dq', mutate=m_at(k0, lambda r: set_(r, Command=X.recorded_command(inner_dq))))
    cases.append(case('RECON', 'P-dq', 'Control positivo: interprete en literal pwsh entre comillas dobles sin expansiones',
                      'reconstruible; argv y sobres identicos a OK', dict(brief(rec)),
                      rec['Reconstructible'] and [(r['Argv'], r['EnvelopeSha256']) for r in rec['Runs']] == ref, Log={'Path': p, 'Sha256': X.sha_file(p)}))
    # INCOMPLETE logs
    not_rec('INC-noturn', 'Registro sin turn.completed', 'turn.completed', finish=False)
    k5 = idx_of('cleantree')

    def drop_completed(data, lg):
        lines = data.split(b'\n')
        target = 'item_%d' % (k5 + 1)
        out = [ln for ln in lines if not (b'"item.completed"' in ln and ('"id":"%s"' % target).encode() in ln)]
        return b'\n'.join(out)
    not_rec('INC-nocompleted', 'Un item.started (cleantree) sin su item.completed', 'without item.completed', raw_mut=drop_completed)
    not_rec('INC-cutlast', 'Ultima linea del registro cortada (sin LF final)', 'last line incomplete', raw_mut=lambda d, lg: d[:-7])
    # EXIT cases
    not_rec('EXIT-mismatch', 'exit_code 1 (failed) con un sobre sin error', 'inconsistent with envelope',
            mutate=m_at(k0, lambda r: set_(r, Rc=1)))
    not_rec('EXIT-null', 'exit_code no registrado (null) con status completed', 'exit_code not recorded',
            mutate=m_at(k0, lambda r: set_(r, Rc=None, Status='completed')))
    not_rec('EXIT-status', 'exit_code 0 con status failed', 'inconsistent with exit_code',
            mutate=m_at(k0, lambda r: set_(r, Status='failed')))
    # EXIT-err: a real error execution added to the T1 log
    err = ['identity', '--repo', T, '--branch', L.BR, '--base', L.BASE, '--current', X.NX, '--red', L.RED]
    inner = X.inner_cmd(err)
    rc, out_, er_, real = X.run_pwsh(inner, T)
    ex_err = {'Name': 'identity-nx', 'Argv': err, 'Inner': inner, 'Command': X.recorded_command(inner), 'Rc': rc, 'Stdout': out_, 'Stderr': er_, 'Instrument': True}
    p, rec = build('EXIT-err', extra=[ex_err])
    if rec['Reconstructible']:
        o, rq = accept('EXIT-err', rec)
    else:
        o, rq = {'Decision': None, 'Checks': {}, 'Table': {'Accepted': False}}, None
    er = [r for r in rec['Runs'] if r.get('ErrorRun')]
    cases.append(case('RECON', 'EXIT-err', 'Ejecucion real con error (identity --current inexistente) anadida al registro de T1',
                      'reconstruible como corrida con error (exit != 0 y Error en el sobre); el aceptante la ve: REJECT_FACTS; tabla no aceptada',
                      dict(o, Reconstruction=brief(rec), ErrorRuns=er, PwshExit=rc, StderrLen=len(er_)),
                      rec['Reconstructible'] and len(er) == 1 and o['Decision'] == 'REJECT_FACTS' and not o['Table']['Accepted'],
                      Log={'Path': p, 'Sha256': X.sha_file(p)}, Request=req_brief(rq) if rq else None))
    # SWAP: argv of one run with the envelope of another
    for name, (ia, ib), what in (('SWAP', (idx_of('read-text', '1-1'), idx_of('read-text', '73-73')), 'read-text 1-1 <-> 73-73 (mismo comando)'),
                                 ('SWAP-cmd', (idx_of('identity'), idx_of('diff')), 'identity <-> diff (comandos distintos)')):
        def sw(j, rec, ia=ia, ib=ib):
            if j == ia:
                rec['Aggregated'] = execs[ib]['Stderr'].decode() + execs[ib]['Stdout'].decode()
            elif j == ib:
                rec['Aggregated'] = execs[ia]['Stderr'].decode() + execs[ia]['Stdout'].decode()
            return rec
        p, rec = build(name, mutate=sw)
        if rec['Reconstructible']:
            o, rq = accept(name, rec)
        else:
            o, rq = {'Decision': None, 'Checks': {}, 'Table': {'Accepted': False}}, None
        cases.append(case('RECON', name, 'SWAP: argv de una corrida con el sobre de otra: ' + what,
                          'reconstruible (la forma es correcta); el aceptante: R3 MISMATCH y REJECT_FACTS; tabla no aceptada',
                          dict(o, Reconstruction=brief(rec)), rec['Reconstructible'] and 'R3' in mm_of(o) and o['Decision'] == 'REJECT_FACTS'
                          and not o['Table']['Accepted'], Log={'Path': p, 'Sha256': X.sha_file(p)}, Request=req_brief(rq) if rq else None))
    # grammar units (no log)
    units = []
    for label, inner, expect in (('sq-escape', "& 'C:\\a''b\\git.exe' --no-replace-objects cat-file blob %s | & 'C:\\p\\python.exe' -I - identity --repo D:/x" % L.B, True),
                                 ('smart-quote', "& " + chr(0x2018) + "C:\\g\\git.exe" + chr(0x2019) + " --no-replace-objects cat-file blob %s | & 'C:\\p\\python.exe' -I - identity" % L.B, False),
                                 ('double-space', "& 'C:\\g\\git.exe'  --no-replace-objects cat-file blob %s | & 'C:\\p\\python.exe' -I - identity" % L.B, False),
                                 ('newline', "& 'C:\\g\\git.exe' --no-replace-objects cat-file blob %s | & 'C:\\p\\python.exe' -I - identity\nWrite-Output x" % L.B, False),
                                 ('relative-git', "& '.\\git.exe' --no-replace-objects cat-file blob %s | & 'C:\\p\\python.exe' -I - identity" % L.B, False),
                                 ('dotdot-git', "& 'C:\\g\\..\\git.exe' --no-replace-objects cat-file blob %s | & 'C:\\p\\python.exe' -I - identity" % L.B, False),
                                 ('tilde-arg', "& 'C:\\g\\git.exe' --no-replace-objects cat-file blob %s | & 'C:\\p\\python.exe' -I - identity --repo ~/x" % L.B, False),
                                 ('wrong-blob', "& 'C:\\g\\git.exe' --no-replace-objects cat-file blob %s | & 'C:\\p\\python.exe' -I - identity" % ('0' * 40), False)):
        cmd = R.render_command(['C:\\Program Files\\PowerShell\\7\\pwsh.exe', '-Command', inner])
        try:
            got = R.parse_instrument_command(cmd, L.B)
            units.append({'Unit': label, 'Reconstructible': True, 'Argv': got['Argv'], 'Git': got['Git'], 'Expected': expect, 'Holds': expect is True})
        except R.NotReconstructible as e:
            units.append({'Unit': label, 'Reconstructible': False, 'Problem': str(e), 'Expected': expect, 'Holds': expect is False})
    cases.append(case('RECON', 'GRAMMAR-units', 'Unidades de la gramatica interna (sin registro)', 'sq-escape reconstruible; el resto no',
                      {'Units': units}, all(u['Holds'] for u in units)))
    # the custodied real logs: completeness, instrument count, outer decoder and renderer on every recorded command
    root = os.path.join(L.WT, 'docs', 'automation', 'evidence', 'I-62-F6')
    import glob
    files = sorted(glob.glob(os.path.join(root, 'OD-2', '*', '*events.jsonl')) + glob.glob(os.path.join(root, 'OD-2b-PROBE', '*', '*events.jsonl')))
    real = []
    tot = dec_ok = same = 0
    for f in files:
        data = L.rbytes(f)
        rec = R.reconstruct(data, L.B)
        cmds = []
        for line in data.split(b'\n'):
            if line.strip():
                e = json.loads(line.decode('utf-8'))
                it = e.get('item') or {}
                if e['type'] == 'item.completed' and it.get('type') == 'command_execution':
                    cmds.append(it['command'])
        d1 = s1 = 0
        for c in cmds:
            try:
                toks = R.split_outer(c)
                d1 += 1
                s1 += R.render_command(toks) == c
            except R.NotReconstructible:
                pass
        tot += len(cmds)
        dec_ok += d1
        same += s1
        real.append({'Log': os.path.relpath(f, L.WT).replace('\\', '/'), 'Sha256': X.sha_file(f), 'Events': rec['Events'], 'Complete': rec['Complete'],
                     'CompletenessProblems': rec['CompletenessProblems'], 'CommandExecutions': rec['CommandExecutions'],
                     'InstrumentExecutions': rec['InstrumentExecutions'], 'OuterDecoded': d1, 'RerenderIdentical': s1})
    cases.append(case('RECON', 'REAL-logs', 'Los registros Codex custodiados de OD-2 y OD-2b-PROBE (encontrados: %d)' % len(files),
                      'completos; 0 ejecuciones del instrumento en cada uno; (extra) el decodificador externo y el renderizador reproducen cada orden registrada',
                      {'Logs': real, 'CommandsTotal': tot, 'OuterDecoded': dec_ok, 'RerenderIdentical': same, 'LogsFound': len(files), 'LogsExpectedByTask': 7},
                      all(r['Complete'] and r['InstrumentExecutions'] == 0 for r in real) and dec_ok == tot and same == tot))
    write_group('p6-reconstruct.json', {'Schema': 'r3-bench/p6-reconstruct', 'ProfileNoiseLine': noise, 'ReferenceRuns': len(ref), 'Cases': cases})


# ------------------------------------------------------------------ summary
COPIES = [('s-t1/controller-output.json', 'inputs/t1-controller-output.json'), ('s-t1/request-r3.json', 'inputs/t1-request-r3.json'),
          ('s-n4/controller-output.json', 'inputs/n4-controller-output.json'), ('s-n4/request-c-r3.json', 'inputs/n4-request-c-r3.json'),
          ('s-n4/request-a-r2.json', 'inputs/n4-request-a-r2.json'),
          ('s-nc1/controller-output-V1.json', 'inputs/nc1-V1-controller-output.json'), ('s-nc1/controller-output-V2.json', 'inputs/nc1-V2-controller-output.json'),
          ('s-nc1v3/controller-output-V3.json', 'inputs/nc1-V3-controller-output.json'), ('s-nc1/request-V2-r3.json', 'inputs/nc1-V2-request-r3.json'),
          ('s-nc1/request-V2-r2.json', 'inputs/nc1-V2-request-r2.json'), ('s-nc1v3/request-V3-r3.json', 'inputs/nc1-V3-request-r3.json'),
          ('s-mut/controller-output-M5.json', 'inputs/m5-controller-output.json'), ('s-mut/request-M2.json', 'inputs/m2-request.json'),
          ('s-zero/session-events.jsonl', 'inputs/zero-session-events.jsonl')]


def copy_inputs():
    out = []
    for src, rel in COPIES:
        raw = L.rbytes(W + '/' + src)
        txt = raw.decode('utf-8')
        clean = L.sanitize_str(txt)
        data = raw if clean == txt else clean.encode('utf-8')
        if L._EMAIL.search(clean):
            raise SystemExit('e-mail address in ' + src)
        L.wbytes(os.path.join(HERE, *rel.split('/')), data)
        out.append({'Source': W + '/' + src, 'SourceSha256': L.sha256(raw), 'Deliverable': rel, 'ByteIdenticalToSource': clean == txt, 'Sha256': L.sha256(data)})
    return out


def group_summary():
    copies = copy_inputs()
    rows = []
    for name in ('p4-t1.json', 'p1-zero.json', 'p2-n4.json', 'p3-nc1.json', 'p5-mutants.json', 'p6-reconstruct.json'):
        d = json.loads(L.rbytes(os.path.join(HERE, name)).decode('ascii'))
        for c in d['Cases']:
            o = c['Observed']
            brief = {}
            if isinstance(o, dict) and 'Checks' in o:
                brief = {'Decision': o.get('Decision'), 'Rc': o.get('Rc'), 'Mismatch': mm_of(o), 'NotApplicable': na_of(o),
                         'TableAccepted': (o.get('Table') or {}).get('Accepted')}
            elif isinstance(o, dict):
                brief = dict((k, o[k]) for k in ('Reconstructible', 'InstrumentExecutions', 'Complete', 'Problems', 'CompletenessProblems',
                                                 'LogsFound', 'CommandsTotal', 'OuterDecoded', 'RerenderIdentical') if k in o)
                if 'Units' in o:
                    brief['Units'] = dict((u['Unit'], u['Reconstructible']) for u in o['Units'])
            if 'CaughtByAcceptor' in c:
                brief['CaughtByAcceptor'] = c['CaughtByAcceptor']
                brief['CaughtByTable'] = c['CaughtByTable']
            rows.append({'File': name, 'Group': c['Group'], 'Case': c['Case'], 'Expected': c['Expected'], 'Observed': brief, 'Holds': c['Holds']})
    files = {}
    for n in sorted(os.listdir(HERE)):
        if n.endswith('.py') or (n.endswith('.json') and n != 'summary.json'):
            files[n] = X.sha_file(os.path.join(HERE, n))
    for c in copies:
        files[c['Deliverable']] = c['Sha256']
    write_group('summary.json', {'Schema': 'r3-bench/summary', 'Cases': rows, 'Total': len(rows), 'Held': sum(1 for r in rows if r['Holds']),
                                 'NotHeld': [r['Case'] for r in rows if not r['Holds']], 'Files': files, 'InputCopies': copies})


GROUPS = {'t1': group_t1, 'zero': group_zero, 'n4': group_n4, 'nc1': group_nc1, 'mutants': group_mutants, 'recon': group_recon, 'summary': group_summary}

if __name__ == '__main__':
    sel = sys.argv[1:] or ['all']
    order = ['t1', 'zero', 'n4', 'nc1', 'mutants', 'recon', 'summary'] if sel == ['all'] else sel
    for g in order:
        print('==== group', g, L.utc())
        GROUPS[g]()
    print('done', L.utc())
