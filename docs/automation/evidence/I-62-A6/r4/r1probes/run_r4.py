# run_r4.py - probes of the round-4 rule (I-62, A-6 round 4): the acceptor closure of N4 and nc1 derived from the corrected
# A6-3 rule 6 (b), with the pinned acceptor and instrument (by blob). Sequential; work area D:\r62-a6r4\r1.
#   python -I run_r4.py derive | v | n4 | nc1 | indep | summary | all
# No model, no network, no read of auth.json / credentials / config.toml.
import json, os, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
import importlib.util
_s = importlib.util.spec_from_file_location('lib_r4', os.path.join(HERE, 'lib_r4.py'))
Y = importlib.util.module_from_spec(_s)
_s.loader.exec_module(Y)
_t = importlib.util.spec_from_file_location('table_r4', os.path.join(HERE, 'table_r4.py'))
T4 = importlib.util.module_from_spec(_t)
_t.loader.exec_module(T4)
X, L, TB, DV = Y.X, Y.L, Y.TB, Y.DV
W = Y.WORK
DECISION = 'BANCO-r4 (sin disposicion: banco de la ronda 4 de A-6; no es una DecisionRef normativa)'
INDEP = 'D:/r62-a6r4/indep/derive_indep.py'
N4_RUN, NC1_RUN = 'R20261014T000000Z-a4n4', 'R20261014T000000Z-a4c1'


# ------------------------------------------------------------------ helpers
def case(group, name, what, expected, obs, holds, **extra):
    d = {'Group': group, 'Case': name, 'What': what, 'Expected': expected, 'Observed': obs, 'Holds': bool(holds)}
    d.update(extra)
    brief = (obs.get('Decision'), mm_of(obs)) if isinstance(obs, dict) and 'Checks' in obs else ''
    print('%-7s %-22s holds=%s %s' % (group, name, bool(holds), brief))
    return d


def mm_of(obs):
    return sorted(k for k, v in (obs.get('Checks') or {}).items() if v == 'MISMATCH')


def na_of(obs):
    return sorted(k for k, v in (obs.get('Checks') or {}).items() if v == 'NOT_APPLICABLE')


def save_state(name, obj):
    L.wbytes(W + '/state-' + name + '.json', (json.dumps(obj, ensure_ascii=True, indent=1, sort_keys=True) + '\n').encode('ascii'))


def load_state(name):
    return json.loads(L.rbytes(W + '/state-' + name + '.json').decode('ascii'))


def write_group(name, obj):
    p, h = L.write_result(name, obj)
    print('wrote', p, h)


def counts(c):
    return dict((k, len(c[k])) for k in ('Commits', 'RevPaths', 'Refs', 'Paths', 'Objects', 'Files'))


def brief_derivation(res):
    return {'Counts': counts(res['Closure']), 'Omitted': Y.omitted_pairs(res['Omitted']),
            'Inputs': res['Inputs'], 'Files': sorted(f['Name'] for f in res['Closure']['Files'])}


def fake_request(work, label, closure_path):
    """A request whose only meaningful field here is Closure (closure-level negatives: the acceptor is not run)."""
    p = work + '/' + label + '.request.json'
    Y.jdump_local(p, {'Schema': TB.REQUEST_SCHEMA, 'Closure': {'Path': closure_path, 'Sha256': Y.sha_file(closure_path)}})
    return p


def record_for(work, label, closure_path, omitted):
    p = work + '/' + label + '.record.json'
    Y.jdump_local(p, {'Schema': 'a6r4-closure-derivation/1', 'Closure': closure_path, 'ClosureSha256': Y.sha_file(closure_path),
                      'Omitted': omitted})
    return p


def r9_runs(acc_result):
    for c in acc_result.get('Checks', []):
        if c.get('Id') == 'R9':
            d = c.get('Detail') or {}
            return dict((r.get('Name'), r.get('Violations')) for r in (d.get('Runs') or []) if isinstance(r, dict))
    return {}


def accept4(T, req_path, cv_path, deleg_path, handoff_path, consts, vec_path, rec_path, executions=None, reconstructed=None):
    acc = L.run_acceptor(req_path, snap_repos=[T], proc_paths=[T])
    out = L.rbytes(acc['StdoutPath'])
    tab = T4.evaluate(req_path, acc['Rc'], out, cv_path, deleg_path, handoff_path, consts, vec_path, rec_path, executions, reconstructed)
    pc = acc['Processes'] or {}
    r = acc['_Result']
    return {'Rc': acc['Rc'], 'Decision': acc['Decision'], 'Checks': acc['Checks'], 'Error': acc.get('Error'),
            'MismatchDetails': dict((rid, X.detail(r, rid)) for rid, st in acc['Checks'].items() if st == 'MISMATCH'),
            'NotApplicableDetails': dict((rid, X.detail(r, rid, 400)) for rid, st in acc['Checks'].items() if st == 'NOT_APPLICABLE'),
            'R9Runs': dict((k, v) for k, v in r9_runs(r).items() if v), 'R9RunsChecked': sorted(r9_runs(r).keys()),
            'R4Rows': X.r4_rows(r), 'RequestSha256': acc['RequestSha256'], 'AcceptorStdoutSha256': acc['StdoutSha256'],
            'ZeroWrite': all(z['Equal'] for z in acc['ZeroWrite']),
            'ProcessTreeDead': not pc.get('Matching') and not pc.get('ChildrenOfGivenParents') and 'Error' not in pc,
            'Table': tab}


def req_brief(p):
    q = json.loads(L.rbytes(p).decode('utf-8'))
    return {'Path': p, 'Sha256': Y.sha_file(p), 'Runs': len(q.get('Runs') or []), 'VerificationIncluded': 'Verification' in q,
            'NotApplicable': sorted((q.get('NotApplicable') or {}).keys()), 'Closure': q.get('Closure')}


def relay_save(work, name, sess, cvp, run_id):
    rr = X.relay_record(sess, cvp, run_id)
    p = work + '/' + name
    L.wjson_local(p, rr)
    return p, X.validate('relay-record.v2.schema.json', rr)


def session(label, T, rs, prim):
    sess, execs = X.verify_session(label, T, rs, prim)
    rec, rruns = X.reconstruct_into(sess['LogPath'], W + '/' + label + '/env')
    if not rec['Reconstructible']:
        raise SystemExit(label + ' log not reconstructible: ' + json.dumps(rec['Problems']))
    return sess, execs, rec, rruns


def consts_for(T, closure_path, relayp, inv):
    c = X.consts_for(T, closure_path, relayp)
    c['IgnorePath'] = Y.ignore_path(inv)
    return c


def rec_brief(rec):
    return {k: rec[k] for k in ('Complete', 'CommandExecutions', 'InstrumentExecutions', 'Reconstructible', 'Problems')}


# ------------------------------------------------------------------ synthetic Controller outputs (round-3 texts, delivery scenario)
NOIN_N4 = 'la comprobacion Handoff esta en fail (entrega ausente: no hay CurrentSha ni texto de la entrega)'
NOIN_NC1 = 'la comprobacion Identity esta en fail (CurrentSha no es un commit existente)'


def cv_n4(F, deleg, handoff_path, prim, run_id):
    auth_ok, defects, ar = X.authority_from(F)
    t = prim['Handoff']
    checks = {
        'Termination': {'Result': 'pass', 'Evidence': 'registro de la sesion (sintetico, banco r4): trabajo del Worker COMPLETED, muerte confirmada'},
        'Handoff': {'Result': 'fail', 'Evidence': 'entrega ausente en ' + handoff_path + ' (primaria: ' + t[0] + ' -> ' + t[1].strip() + ')'},
        'Authority': {'Result': X.res(auth_ok), 'Evidence': X._ev(F, 'clausemap', 'defectos del mapa: ' + (','.join(defects) or 'ninguno'))},
        'Contract': {'Result': X.res(X.contract_from(F)), 'Evidence': X._ev(F, 'contract')},
        'Identity': X.nr_noinput(NOIN_N4), 'Remote': X.nr_noinput(NOIN_N4), 'Scope': X.nr_noinput(NOIN_N4),
        'CleanTree': {'Result': X.res(X.cleantree_from(F)), 'Evidence': X._ev(F, 'cleantree')},
        'Ci': X.nr_noinput(NOIN_N4, True), 'Tests': X.nr_noinput(NOIN_N4, True), 'Trailer': X.nr_noinput(NOIN_N4),
        'Routing': X.nr_bench('sin modelo'), 'FreeText': X.nr_noinput(NOIN_N4),
        'Denials': X.nr_bench('sin eventos ni transcripcion del Worker'),
    }
    findings = ['Salida sintetica del banco r4 para N4 (entrega ausente): Controller simulado por la supervision.',
                'Disposition BLOCKED segun el modo de fallo "Entrega ausente" de AP 16.9; Authority tiene fila STOP: ver Coherence169.']
    return X.cv_doc(checks, ar, 'BLOCKED', 'EXECUTION_BLOCKED', findings, run_id, Y.H_RUN, deleg['RunId'])


def cv_nc1(F, deleg, handoff, prim, variant, run_id):
    auth_ok, defects, ar = X.authority_from(F)
    pi, ps, pt = prim['Identity'], prim['Scope'], prim['Trailer']
    checks = {
        'Termination': {'Result': 'pass', 'Evidence': 'registro de la sesion (sintetico, banco r4): trabajo del Worker COMPLETED, muerte confirmada'},
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
    findings = ['Salida sintetica del banco r4 para nc1 (CurrentSha inexistente), variante ' + variant + '.',
                'FailureClass = la primera comprobacion fuera de pass en el orden fijo de AP 16.9.']
    return X.cv_doc(checks, ar, 'STOP', 'EXECUTION_BLOCKED', findings, run_id, handoff['RunId'], deleg['RunId']), no_input


# ------------------------------------------------------------------ group derive: the corrected derivation without the acceptor
def group_derive():
    L.ensure_dir(W)
    T = Y.copy_repo('d')
    work = W + '/derive'
    L.rmtree(work)
    L.ensure_dir(work)
    cases, store = [], {}

    def run_vec(label, vec):
        cp, rp, res = Y.derive_to(work, label, vec)
        store[label] = (cp, rp, res)
        return res

    # D-R2: the round-2 vector, unchanged (regression of the part the round does not change)
    r2vec = Y.vector(T, Y.R2_EIC, Y.invocation(Y.V_RUN),
                     {'D': Y.deliv_path('T1', Y.D_RUN, 'output.json'), 'H': Y.deliv_path('T1', Y.H_RUN, 'worker-handoff.json'), 'K': Y.K},
                     {Y.deliv_path('T1', Y.D_RUN, 'output.json'): Y.R2_INP + '/deleg-deliv.json',
                      Y.deliv_path('T1', Y.H_RUN, 'worker-handoff.json'): Y.R2_INP + '/handoff-deliv.json', Y.TRX_E: Y.R2_INP + '/fixture-tests.trx'})
    res = run_vec('D-R2', r2vec)
    r2c = json.loads(L.rbytes(Y.R2_CLOSURE).decode('utf-8'))
    cases.append(case('derive', 'D-R2', 'Vector de la ronda 2 (EIC-VERIFY, D, H y K de r2/independence (d)) con la regla corregida',
                      'cierre identico al custodiado de la ronda 2 (EIC-VERIFY.bmin-closure.json); un solo termino omitido: D.ChainRedSha (null)',
                      brief_derivation(res), Y.closure_diff(res['Closure'], r2c) == {} and Y.omitted_pairs(res['Omitted']) == [('C1.D.ChainRedSha', 'FIELD_NULL')],
                      Round2ClosureSha256=Y.sha_file(Y.R2_CLOSURE)))
    vs = dict(r2vec, E=Y.R2_EIC_STRICT)
    res = run_vec('D-R2-STRICT', vs)
    r2s = json.loads(L.rbytes(Y.R2_CLOSURE_STRICT).decode('utf-8'))
    cases.append(case('derive', 'D-R2-STRICT', 'Vector de la ronda 2 con la variante I6 estricta de E', 'cierre identico al custodiado de la ronda 2 (EIC-I6-STRICT)',
                      brief_derivation(res), Y.closure_diff(res['Closure'], r2s) == {}))
    # inputs of the probes (copies with this repository as worktree)
    inp_v = W + '/derive/in-v'
    deleg, hand = Y.write_inputs(T, inp_v)
    E, cust, des = Y.eic('T1', Y.D_RUN, Y.H_RUN, inp_v)
    Ep = Y.jdump_local(work + '/E-v.json', E)
    vec_v = Y.vector(T, Ep, Y.invocation(Y.V_RUN), des, cust)
    rv = run_vec('D-V', vec_v)
    dv = Y.closure_diff(rv['Closure'], r2c)
    cases.append(case('derive', 'D-V', 'Verificacion real (T1 sobre la entrega): copias de D y H con la ruta de este clon',
                      'igual al cierre de la ronda 2 salvo los SHA-256 de DELEG y H, que cambian con la ruta del clon',
                      dict(brief_derivation(rv), DiffWithRound2=dv), set(dv) == {'Files'} and sorted(n for n, _ in dv['Files']['Extra']) == ['DELEG', 'H']
                      and sorted(n for n, _ in dv['Files']['Missing']) == ['DELEG', 'H']))
    vcommits = set(rv['Closure']['Commits'])
    vfiles = set(f['Name'] for f in rv['Closure']['Files'])

    # ---------------- N4: the delivery is absent
    H_ABS = [('C1.H.BaseSha', 'INPUT_ABSENT'), ('C1.H.CurrentSha', 'INPUT_ABSENT'), ('C1.H.RedSha', 'INPUT_ABSENT'),
             ('C1.RevList(D.BaseSha..H.CurrentSha)', 'INPUT_ABSENT'), ('C6.H', 'INPUT_ABSENT')]
    base_om = [('C1.D.ChainRedSha', 'FIELD_NULL')]
    want_n4 = sorted(H_ABS + base_om)

    def fresh(label):
        return W + '/derive/in-' + label

    n4 = {}
    for label, mode, what in (('D-N4-ABSENT', 'absent', 'E sin entrada de la entrega (el kit no puede fijar el blob de un archivo que no existe)'),
                              ('D-N4-STALE', 'stale', 'E fija la ruta de la entrega con el blob de la entrega real, pero el archivo no existe')):
        inp4 = fresh(label)
        Y.write_inputs(T, inp4, handoff_present=False)
        E4, c4, d4 = Y.eic('T1-n4', Y.D_RUN, Y.H_RUN, inp4, h_mode=mode)
        vec = Y.vector(T, Y.jdump_local(work + '/E-' + label + '.json', E4), Y.invocation(N4_RUN), d4, c4)
        r = run_vec(label, vec)
        n4[label] = (r, vec)
        cl = r['Closure']
        ok = (Y.omitted_pairs(r['Omitted']) == want_n4 and Y.RED not in cl['Commits'] and Y.G in cl['Commits'] and Y.TIP in cl['Commits']
              and 'H' not in [f['Name'] for f in cl['Files']] and set(cl['Commits']) <= vcommits)
        cases.append(case('derive', label, what,
                          'H ausente: se omiten H.BaseSha, H.CurrentSha, H.RedSha, RevList(D.BaseSha..H.CurrentSha) y C6 H con INPUT_ABSENT; RED fuera (solo lo aportaban H); G dentro por I.Target.Commit; TIP por D.BaseSha; ningun commit fuera de los de la verificacion real',
                          brief_derivation(r), ok, HPathInPaths=d4['H'] in cl['Paths']))
    dd = Y.closure_diff(n4['D-N4-STALE'][0]['Closure'], n4['D-N4-ABSENT'][0]['Closure'])
    cases.append(case('derive', 'D-N4-STALE-vs-ABSENT', 'Diferencia entre las dos formas de E en N4',
                      'solo la ruta de la entrega en C4 (es una entrada de E en STALE); los commits, C3, C2 y C6 iguales',
                      dd, set(dd) == {'Paths'} and dd['Paths']['Extra'] == [n4['D-N4-STALE'][1]['Designation']['H']] and not dd['Paths']['Missing']))
    # unreadable deliveries: E pins a blob, the bytes are not a usable object
    real_h = L.rbytes(Y.R2_INP + '/handoff-deliv.json')
    for label, data, what in (
            ('D-N4-BLOB', real_h.replace(b'"Pushed": true', b'"Pushed": false'), 'bytes de la entrega con otro blob que el que fija E'),
            ('D-N4-NOTJSON', real_h[:len(real_h) // 2], 'entrega truncada (no es JSON)'),
            ('D-N4-DUPKEY', real_h.replace(b'"TaskId": "T1",', b'"TaskId": "T1",\n "TaskId": "T1",', 1), 'entrega con una clave repetida'),
            ('D-N4-BOM', b'\xef\xbb\xbf' + real_h, 'entrega con BOM UTF-8'),
            ('D-N4-ARRAY', b'[' + real_h + b']', 'entrega que es un arreglo JSON, no un objeto')):
        inp4 = fresh(label)
        Y.write_inputs(T, inp4, handoff_present=False)
        if data == real_h:
            raise SystemExit('mutation did not apply: ' + label)
        L.wbytes(inp4 + '/handoff.json', data)
        pin = L.blob_sha1(real_h) if label == 'D-N4-BLOB' else L.blob_sha1(data)
        E4, c4, d4 = Y.eic('T1-n4', Y.D_RUN, Y.H_RUN, inp4, h_mode='pinned', h_blob=pin)
        vec = Y.vector(T, Y.jdump_local(work + '/E-' + label + '.json', E4), Y.invocation(N4_RUN), d4, c4)
        r = run_vec(label, vec)
        same = Y.closure_diff(r['Closure'], n4['D-N4-STALE'][0]['Closure'])
        cases.append(case('derive', label, what, 'H ausente por la regla (i): las mismas omisiones y el mismo cierre que D-N4-STALE',
                          dict(brief_derivation(r), InputWhy=r['Inputs']['H']['Why']), Y.omitted_pairs(r['Omitted']) == want_n4 and same == {}))

    # ---------------- nc1: CurrentSha does not name an existing commit; and absent versus empty fields
    def nc1_vec(label, mut):
        inpc = fresh(label)
        Y.write_inputs(T, inpc, handoff_mut=mut)
        Ec, cc, dc = Y.eic('T1-nc1', Y.D_RUN, Y.H_RUN, inpc)
        return Y.vector(T, Y.jdump_local(work + '/E-' + label + '.json', Ec), Y.invocation(NC1_RUN), dc, cc)

    def put(k, v):
        def f(h):
            h[k] = v
        return f

    def drop(k):
        def f(h):
            h.pop(k)
        return f

    vec_nc1 = nc1_vec('D-NC1', put('CurrentSha', Y.NX))
    rnc = run_vec('D-NC1', vec_nc1)
    want_nc1 = sorted(base_om + [('C1.H.CurrentSha', 'NOT_A_COMMIT'), ('C1.RevList(D.BaseSha..H.CurrentSha)', 'PRIMITIVE_EMPTY')])
    cases.append(case('derive', 'D-NC1', 'nc1: H.CurrentSha = deadbeef... (40 hex, no es un objeto del clon)',
                      'H.CurrentSha NOT_A_COMMIT y RevList PRIMITIVE_EMPTY; RED sigue dentro por H.RedSha y G por I.Target.Commit: mismos commits que la verificacion real; C6 H = SHA-256 de la entrega mutada',
                      brief_derivation(rnc), Y.omitted_pairs(rnc['Omitted']) == want_nc1 and set(rnc['Closure']['Commits']) == vcommits
                      and 'H' in [f['Name'] for f in rnc['Closure']['Files']]))

    def nofiles(c):
        c = dict(c)
        c['Files'] = [f for f in c['Files'] if f['Name'] != 'H']
        return c
    tree_g = L.gout(T, 'rev-parse', Y.G + '^{tree}')
    for label, mut, cause, what in (
            ('F-CUR-MISSING', drop('CurrentSha'), 'FIELD_MISSING', 'H sin el campo CurrentSha'),
            ('F-CUR-NULL', put('CurrentSha', None), 'FIELD_NULL', 'H.CurrentSha = null'),
            ('F-CUR-EMPTY', put('CurrentSha', ''), 'FIELD_EMPTY', 'H.CurrentSha = ""'),
            ('F-CUR-UPPER', put('CurrentSha', Y.G.upper()), 'NOT_A_COMMIT', 'H.CurrentSha = G en mayusculas'),
            ('F-CUR-ABBREV', put('CurrentSha', Y.G[:12]), 'NOT_A_COMMIT', 'H.CurrentSha = prefijo de 12 caracteres de G'),
            ('F-CUR-TREE', put('CurrentSha', tree_g), 'NOT_A_COMMIT', 'H.CurrentSha = el arbol de G (objeto existente, no commit)'),
            ('F-CUR-BLOB', put('CurrentSha', Y.TARGET_BLOB), 'NOT_A_COMMIT', 'H.CurrentSha = un blob existente'),
            ('F-CUR-REF', put('CurrentSha', 'HEAD'), 'NOT_A_COMMIT', 'H.CurrentSha = "HEAD" (una ref, no un nombre de commit)')):
        r = run_vec(label, nc1_vec(label, mut))
        want = sorted(base_om + [('C1.H.CurrentSha', cause), ('C1.RevList(D.BaseSha..H.CurrentSha)', 'PRIMITIVE_EMPTY')])
        cases.append(case('derive', label, what, 'se omiten H.CurrentSha (%s) y RevList (PRIMITIVE_EMPTY); el cierre es el de D-NC1 salvo el SHA-256 de C6 H' % cause,
                          brief_derivation(r), Y.omitted_pairs(r['Omitted']) == want
                          and Y.closure_diff(nofiles(r['Closure']), nofiles(rnc['Closure'])) == {}))
    for label, mut, cause, what in (
            ('F-RED-MISSING', drop('RedSha'), 'FIELD_MISSING', 'H sin el campo RedSha'),
            ('F-RED-NULL', put('RedSha', None), 'FIELD_NULL', 'H.RedSha = null'),
            ('F-RED-EMPTY', put('RedSha', ''), 'FIELD_EMPTY', 'H.RedSha = ""')):
        r = run_vec(label, nc1_vec(label, mut))
        want = sorted(base_om + [('C1.H.RedSha', cause)])
        cases.append(case('derive', label, what, 'se omite H.RedSha (%s); RED sigue dentro por RevList(D.BaseSha..H.CurrentSha): mismos commits que la verificacion real' % cause,
                          brief_derivation(r), Y.omitted_pairs(r['Omitted']) == want and set(r['Closure']['Commits']) == vcommits))
    # D and K absent; Target nonexistent; MergeBase without result (generality of rules (i)-(iii))
    E_dabs, cdab, ddab = Y.eic('T1', Y.D_RUN, Y.H_RUN, inp_v)
    E_dabs['CanonicalInputs'] = [e for e in E_dabs['CanonicalInputs'] if e['Path'] != ddab['D']]
    r = run_vec('D-D-ABSENT', Y.vector(T, Y.jdump_local(work + '/E-D-D-ABSENT.json', E_dabs), Y.invocation(Y.V_RUN), ddab, cdab))
    want = sorted([('C1.D.BaseSha', 'INPUT_ABSENT'), ('C1.D.MainSha', 'INPUT_ABSENT'), ('C1.D.ChainBaseSha', 'INPUT_ABSENT'),
                   ('C1.D.ChainRedSha', 'INPUT_ABSENT'), ('C1.RevList(D.BaseSha..H.CurrentSha)', 'INPUT_ABSENT'),
                   ('C1.FirstParentChain(D.MainSha)', 'INPUT_ABSENT'), ('C1.MergeBase(D.MainSha,K.AuthorityRevision)', 'INPUT_ABSENT'),
                   ('C2.origin/D.ExpectedBranch', 'INPUT_ABSENT'), ('C6.DELEG', 'INPUT_ABSENT')])
    cases.append(case('derive', 'D-D-ABSENT', 'Delegacion no fijada por E (generalidad de la regla (i))',
                      'todos los terminos de D omitidos con INPUT_ABSENT, incluida la ref de C2 y C6 DELEG; sin la cadena de primer padre de D.MainSha',
                      brief_derivation(r), Y.omitted_pairs(r['Omitted']) == want and r['Closure']['Refs'] == ['HEAD', 'refs/remotes/origin/main']))
    E_kabs, ckab, dkab = Y.eic('T1', Y.D_RUN, Y.H_RUN, inp_v)
    E_kabs['CanonicalInputs'] = [e for e in E_kabs['CanonicalInputs'] if e['Path'] != Y.K]
    r = run_vec('D-K-ABSENT', Y.vector(T, Y.jdump_local(work + '/E-D-K-ABSENT.json', E_kabs), Y.invocation(Y.V_RUN), dkab, ckab))
    want = sorted(base_om + [('C1.K.AuthorityRevision', 'INPUT_ABSENT'), ('C1.K.MainSha', 'INPUT_ABSENT'),
                             ('C1.MergeBase(D.MainSha,K.AuthorityRevision)', 'INPUT_ABSENT'), ('C4.K.Authorities', 'INPUT_ABSENT'), ('C6.CONTRACT', 'INPUT_ABSENT')])
    cases.append(case('derive', 'D-K-ABSENT', 'Contrato no fijado por E (generalidad de la regla (i))',
                      'K.AuthorityRevision, K.MainSha, MergeBase, las rutas de K.Authorities y C6 CONTRACT omitidos con INPUT_ABSENT',
                      brief_derivation(r), Y.omitted_pairs(r['Omitted']) == want and 'CONTRACT' not in [f['Name'] for f in r['Closure']['Files']]))
    r = run_vec('D-TARGET-NX', dict(vec_v, I=Y.invocation(Y.V_RUN, target_commit=Y.NX)))
    cases.append(case('derive', 'D-TARGET-NX', 'I.Target.Commit inexistente', 'I.Target.Commit NOT_A_COMMIT; G sigue dentro por H.CurrentSha',
                      brief_derivation(r), Y.omitted_pairs(r['Omitted']) == sorted(base_om + [('C1.I.Target.Commit', 'NOT_A_COMMIT')])
                      and set(r['Closure']['Commits']) == vcommits))
    # MergeBase without result: K.AuthorityRevision = a root commit unrelated to D.MainSha (written in this copy only)
    empty_tree = L.gout(T, 'hash-object', '-t', 'tree', '-w', '--stdin', inp=b'')
    orphan = subprocess.run(['git', '-C', T, '-c', 'user.name=banco-r4', '-c', 'user.email=banco-r4@example.invalid', 'commit-tree', empty_tree,
                             '-m', 'raiz sin relacion (banco r4)'],
                            capture_output=True, stdin=subprocess.DEVNULL, env=L.genv()).stdout.decode('ascii').strip()
    if L.gout(T, 'cat-file', '-t', orphan) != 'commit':
        raise SystemExit('orphan commit not written')
    inp_mb = fresh('D-MB-EMPTY')
    Y.write_inputs(T, inp_mb)
    kb = json.loads(L.rbytes(inp_mb + '/K.json').decode('utf-8'))
    kb['AuthorityRevision'] = orphan
    kmb = (json.dumps(kb, ensure_ascii=False, indent=1) + '\n').encode('utf-8')
    kblob = L.gout(T, 'hash-object', '-w', '--no-filters', '--stdin', inp=kmb)
    E_mb, cmb, dmb = Y.eic('T1', Y.D_RUN, Y.H_RUN, inp_mb)
    E_mb['CanonicalInputs'] = [dict(e, Blob=kblob) if e['Path'] == Y.K else e for e in E_mb['CanonicalInputs']]
    r = run_vec('D-MB-EMPTY', Y.vector(T, Y.jdump_local(work + '/E-D-MB-EMPTY.json', E_mb), Y.invocation(Y.V_RUN), dmb, cmb))
    cases.append(case('derive', 'D-MB-EMPTY', 'K.AuthorityRevision = una raiz sin relacion con D.MainSha (MergeBase sin base comun)',
                      'MergeBase PRIMITIVE_EMPTY; la raiz entra por K.AuthorityRevision; nada sustituye la base comun ausente',
                      dict(brief_derivation(r), Orphan=orphan), Y.omitted_pairs(r['Omitted']) == sorted(base_om + [('C1.MergeBase(D.MainSha,K.AuthorityRevision)', 'PRIMITIVE_EMPTY')])
                      and set(r['Closure']['Commits']) == vcommits | {orphan}))

    # ---------------- substitution negatives (iv): a non-existent input replaced by a valid one
    n4cp, n4rp, n4r = store['D-N4-ABSENT']
    n4vec = n4['D-N4-ABSENT'][1]
    nccp, ncrp, ncr = store['D-NC1']
    sub = {}

    def neg(label, closure_path, omitted, normative_vec, what, expected, want_kinds):
        """Closure-level negative: request pinning closure_path, a record tied to it listing `omitted`; cotejo against the
        derivation of normative_vec (the acceptor is not run)."""
        rq = fake_request(work, label, closure_path)
        recp = record_for(work, label, closure_path, omitted)
        vp = work + '/' + label + '.normative-vector.json'
        Y.jdump_local(vp, normative_vec)
        v, _ = T4.closure_check(rq, vp, recp)
        kinds = sorted(set(x.split(':')[0] for x in v))
        given = json.loads(L.rbytes(closure_path).decode('utf-8'))
        diff = Y.closure_diff(given, DV.derive_vector(normative_vec)['Closure'])
        sub[label] = (closure_path, recp)
        cases.append(case('neg', label, what, expected, {'Violations': v, 'Diff': diff, 'Kinds': kinds}, kinds == want_kinds))

    inp_s1 = fresh('S-N4-REALH')
    Y.write_inputs(T, inp_s1)
    Es, cs, ds = Y.eic('T1-n4', Y.D_RUN, Y.H_RUN, inp_s1)
    rs1 = run_vec('S-N4-REALH', Y.vector(T, Y.jdump_local(work + '/E-S-N4-REALH.json', Es), Y.invocation(N4_RUN), ds, cs))
    neg('S-N4-REALH', store['S-N4-REALH'][0], rs1['Omitted'], n4vec,
        'N4: la entrega ausente se sustituye por la entrega real de la verificacion (una copia valida)',
        'el cotejo rechaza: RED, la ruta de H y C6 H de mas (el cierre no es el derivado) y el registro no declara H ausente',
        ['CLOSURE_NOT_DERIVED', 'DERIVATION_RECORD_DIFFERS'])
    inp_s2 = fresh('S-N4-TARGET')
    Y.write_inputs(T, inp_s2)
    syn = {'BaseSha': Y.TIP, 'CurrentSha': Y.G, 'RedSha': None, 'RunId': Y.H_RUN, 'Schema': 'rackcad-worker-handoff/v1'}
    Y.jdump_local(inp_s2 + '/handoff.json', syn)
    Es2, cs2, ds2 = Y.eic('T1-n4', Y.D_RUN, Y.H_RUN, inp_s2)
    rs2 = run_vec('S-N4-TARGET', Y.vector(T, Y.jdump_local(work + '/E-S-N4-TARGET.json', Es2), Y.invocation(N4_RUN), ds2, cs2))
    neg('S-N4-TARGET', store['S-N4-TARGET'][0], rs2['Omitted'], n4vec,
        'N4: la supervision completa la entrega ausente con el Target (una entrega sintetica con CurrentSha = I.Target.Commit)',
        'el cotejo rechaza: RED (RevList hasta el Target), la ruta de H y C6 H de mas; registro distinto', ['CLOSURE_NOT_DERIVED', 'DERIVATION_RECORD_DIFFERS'])
    # nc1 with CurrentSha := G or another commit at derivation time only (the custodied bytes stay the mutated ones)
    other = L.gout(T, 'rev-parse', Y.OTHER)
    hm = json.loads(L.rbytes(vec_nc1['Custody'][vec_nc1['Designation']['H']]).decode('utf-8'))
    hsha = Y.sha_file(vec_nc1['Custody'][vec_nc1['Designation']['H']])
    for label, val, what, expected, kinds in (
            ('S-NC1-G', Y.G, 'nc1: la supervision sustituye el CurrentSha inexistente por G solo al derivar (los bytes custodiados siguen siendo los mutados)',
             'el cierre resultante es IGUAL al normativo (RED y G ya estaban por H.RedSha y el Target); solo el registro de derivacion muestra la sustitucion, y el cotejo la rechaza por el registro',
             ['DERIVATION_RECORD_DIFFERS']),
            ('S-NC1-OTHER', other, 'nc1: el CurrentSha inexistente se sustituye por otro commit existente (cabeza de origin/fx/u1-fx05)',
             'el cotejo rechaza: commits de RevList hasta ese commit de mas, y registro distinto', ['CLOSURE_NOT_DERIVED', 'DERIVATION_RECORD_DIFFERS'])):
        hs = dict(hm, CurrentSha=val)
        hsp = Y.jdump_local(W + '/derive/' + label + '-handoff.json', hs)
        Eg = json.loads(L.rbytes(vec_nc1['E']).decode('utf-8'))
        Eg['CanonicalInputs'] = [dict(e, Blob=Y.blob_file(hsp)) if e['Path'] == vec_nc1['Designation']['H'] else e for e in Eg['CanonicalInputs']]
        vg = dict(vec_nc1, E=Y.jdump_local(work + '/E-' + label + '.json', Eg), Custody=dict(vec_nc1['Custody'], **{vec_nc1['Designation']['H']: hsp}))
        rg = DV.derive_vector(vg)
        clg = dict(rg['Closure'])
        clg['Files'] = [dict(f, Sha256=hsha) if f['Name'] == 'H' else f for f in clg['Files']]
        cpg = Y.jdump_local(work + '/' + label + '.closure.json', clg)
        neg(label, cpg, rg['Omitted'], vec_nc1, what, expected, kinds)
    # the round-3 closure (end to end: every commit of the clone, every path of nine commits) used for N4
    e2e_inp = fresh('S-N4-E2E')
    Y.write_inputs(T, e2e_inp)
    hmap = {'deleg.json': Y.sha_file(e2e_inp + '/deleg.json'), 'K.json': Y.sha_file(e2e_inp + '/K.json'),
            'handoff.json': Y.sha_file(e2e_inp + '/handoff.json'), 'fixture-tests.trx': Y.sha_file(e2e_inp + '/fixture-tests.trx')}
    e2e = L.build_closure(T, e2e_inp, hmap, json.loads(L.rbytes(e2e_inp + '/K.json').decode('utf-8')))
    neg('S-N4-E2E', e2e, n4r['Omitted'], n4vec, 'N4 con el cierre del extremo a extremo de la ronda 3 (build_closure) en lugar del derivado',
        'el cotejo rechaza: cientos de elementos de mas (todos los commits del clon, rutas de nueve commits, C6 H)', ['CLOSURE_NOT_DERIVED'])

    # ---------------- added or invented inputs on the normative closures (the acceptor is not needed to see them)
    def mutate(label, base_cp, fn):
        c = json.loads(L.rbytes(base_cp).decode('utf-8'))
        fn(c)
        return Y.jdump_local(work + '/' + label + '.closure.json', c)
    readme = 'README.md'
    for label, base, om, vec, fn, what in (
            ('X-N4-COMMIT', n4cp, n4r['Omitted'], n4vec, lambda c: c['Commits'].append(other), 'N4 + un commit existente fuera de la tabla'),
            ('X-N4-NX', n4cp, n4r['Omitted'], n4vec, lambda c: c['Commits'].append(Y.NX), 'N4 + el commit inexistente deadbeef... (objeto inventado)'),
            ('X-N4-PATH', n4cp, n4r['Omitted'], n4vec, lambda c: c['Paths'].append(readme), 'N4 + la ruta README.md'),
            ('X-N4-REVPATH', n4cp, n4r['Omitted'], n4vec, lambda c: c['RevPaths'].append({'Rev': Y.G, 'Path': readme}), 'N4 + (G, README.md)'),
            ('X-N4-FILE', n4cp, n4r['Omitted'], n4vec, lambda c: c['Files'].append({'Name': 'H', 'Sha256': '0' * 64}), 'N4 + C6 H inventado'),
            ('X-N4-REF', n4cp, n4r['Omitted'], n4vec, lambda c: c['Refs'].append('refs/remotes/origin/fx/u1-fx05'), 'N4 + una ref fuera de C2'),
            ('X-N4-DROP', n4cp, n4r['Omitted'], n4vec, lambda c: c['Commits'].remove(Y.TIP), 'N4 sin el commit TIP (D.BaseSha)'),
            ('X-NC1-NX', nccp, ncr['Omitted'], vec_nc1, lambda c: c['Commits'].append(Y.NX), 'nc1 + el CurrentSha inexistente como commit del cierre')):
        cp = mutate(label, base, fn)
        neg(label, cp, om, vec, what, 'el cotejo rechaza: el cierre no es el derivado', ['CLOSURE_NOT_DERIVED'])
    neg('X-N4-RECORD', n4cp, [], n4vec, 'N4 con el cierre normativo y un registro de derivacion vacio',
        'el cotejo rechaza: el registro no declara los terminos omitidos', ['DERIVATION_RECORD_DIFFERS'])
    neg('X-N4-OK', n4cp, n4r['Omitted'], n4vec, 'Control positivo: cierre normativo de N4 con su registro', 'el cotejo no encuentra ninguna diferencia', [])
    neg('X-NC1-OK', nccp, ncr['Omitted'], vec_nc1, 'Control positivo: cierre normativo de nc1 con su registro', 'el cotejo no encuentra ninguna diferencia', [])
    write_group('p7-derive.json', {'Schema': 'r4-bench/p7-derive', 'Repo': T, 'Other': other, 'Orphan': orphan, 'Cases': cases})
    save_state('derive', {'T': T, 'Store': dict((k, [v[0], v[1]]) for k, v in store.items()), 'Subst': sub, 'Other': other})


# ------------------------------------------------------------------ group v: regression of the real verification
def group_v():
    T, info = Y.prepare('v', DECISION)
    inp = W + '/c-v-in'
    deleg, hand = Y.write_inputs(T, inp)
    inv = Y.invocation(Y.V_RUN)
    E, cust, des = Y.eic('T1', Y.D_RUN, Y.H_RUN, inp)
    work = W + '/s-v'
    L.ensure_dir(work)
    vec = Y.vector(T, Y.jdump_local(work + '/E.json', E), inv, des, cust)
    cp, rp, dres = Y.derive_to(work, 'closure-v', vec)
    names = ['identity', 'diff', 'scope', 'contract', 'hashes', 'cleantree', 'handoff', 'freetext', 'clausemap', 'trx', 'read-text', 'read-json']
    sess, execs, rec, rruns = session('s-v', T, Y.runs(T, inp, inv, names), Y.prim_reads(Y.G))
    F = X.facts_by_cmd(rruns)
    L.CV_RUN = Y.V_RUN
    cv = X.cv_t1(F, deleg, hand, sess['Primary'])
    cvp = work + '/controller-output.json'
    L.wjson_local(cvp, cv)
    relayp, rsv = relay_save(work, 'relay-facts.json', sess, cvp, Y.V_RUN)
    consts = consts_for(T, cp, relayp, inv)
    dp, hp = inp + '/deleg.json', inp + '/handoff.json'
    req, dec = TB.construct(cvp, dp, hp, rruns, consts)
    reqp = X.write_request(work + '/request.json', req)
    vp = work + '/closure-v.vector.json'
    obs = accept4(T, reqp, cvp, dp, hp, consts, vp, rp, rec['InstrumentExecutions'], rec['Runs'])
    cases = [case('V', 'V-r4', 'Verificacion real de T1 sobre la entrega: 12 corridas reconstruidas del registro Codex; Closure derivado con la regla corregida',
                  'la regla incluye Verification; ACCEPT_FACTS rc 0; R9 MATCH (ninguna corrida fuera, clausemap dentro); tabla de la ronda 4 aceptada (construccion, aceptacion y cotejo del cierre)',
                  obs, obs['Decision'] == 'ACCEPT_FACTS' and obs['Checks'].get('R9') == 'MATCH' and dec['Include'] and not obs['R9Runs']
                  and obs['Table']['Accepted'], Request=req_brief(reqp), Derivation=brief_derivation(dres))]
    # O3: the same runs with the closure derived from the I6-strict variant of E
    Es, cs, ds = Y.eic('T1', Y.D_RUN, Y.H_RUN, inp, strict=True)
    vs = Y.vector(T, Y.jdump_local(work + '/E-strict.json', Es), inv, ds, cs)
    cps, rps, sres = Y.derive_to(work, 'closure-v-strict', vs)
    consts_s = consts_for(T, cps, relayp, inv)
    req_s, _ = TB.construct(cvp, dp, hp, rruns, consts_s)
    reqs = X.write_request(work + '/request-strict.json', req_s)
    obs_s = accept4(T, reqs, cvp, dp, hp, consts_s, work + '/closure-v-strict.vector.json', rps, rec['InstrumentExecutions'], rec['Runs'])
    outside = sorted(k.split('-', 1)[1] for k in obs_s['R9Runs'])
    cases.append(case('V', 'O3-STRICT', 'O3: las mismas 12 corridas con el cierre derivado de la variante I6 estricta de E (todo blob = el de la AuthorityRevision)',
                      'R9 MISMATCH; quedan fuera las lecturas legitimas en G: hashes, read-text y clausemap (estado en G); read-json no (el blob del contrato en G es el que fija E); la cobertura depende de la disposicion de A6-7, regla 7',
                      dict(obs_s, OutsideRuns=outside), obs_s['Checks'].get('R9') == 'MISMATCH' and outside == ['clausemap', 'hashes', 'read-text'],
                      Derivation=brief_derivation(sres)))
    sv = {'delegation.v2': X.validate('delegation.v2.schema.json', deleg), 'worker-handoff': X.validate('worker-handoff.schema.json', hand),
          'controller-verification.v2': X.validate('controller-verification.v2.schema.json', cv), 'relay-record.v2': rsv}
    write_group('p8-v.json', {'Schema': 'r4-bench/p8-v', 'Clone': info, 'SchemaValidation': sv, 'SchemaValid': all(not v for v in sv.values()),
                              'Session': {k: sess[k] for k in ('LogPath', 'LogSha256', 'ZeroWriteToolRuns', 'Executions')},
                              'Reconstruction': rec_brief(rec), 'ControllerChecks': dict((c, cv['Checks'][c]['Result']) for c in X.ORDER),
                              'Classification': [cv['Classification'], cv['Disposition'], cv['FailureClass']], 'Cases': cases})


# ------------------------------------------------------------------ group n4
def group_n4():
    T, info = Y.prepare('n4', DECISION)
    inp = W + '/c-n4-in'
    deleg, _ = Y.write_inputs(T, inp, handoff_present=False)
    inv = Y.invocation(N4_RUN)
    work = W + '/s-n4'
    L.ensure_dir(work)
    E, cust, des = Y.eic('T1-n4', Y.D_RUN, Y.H_RUN, inp, h_mode='absent')
    vec = Y.vector(T, Y.jdump_local(work + '/E.json', E), inv, des, cust)
    cp, rp, dres = Y.derive_to(work, 'closure-n4', vec)
    vp = work + '/closure-n4.vector.json'
    hp = inp + '/handoff.json'
    names = ['contract', 'hashes', 'cleantree', 'read-text', 'read-json', 'trx', 'clausemap']
    rs = Y.runs(T, inp, inv, names)
    prim = Y.prim_reads(None, hp, n4=True)
    sess, execs, rec, rruns = session('s-n4', T, rs, prim)
    F = X.facts_by_cmd(rruns)
    cv = cv_n4(F, deleg, hp, sess['Primary'], N4_RUN)
    cvp = work + '/controller-output.json'
    L.wjson_local(cvp, cv)
    relayp, rsv = relay_save(work, 'relay-facts.json', sess, cvp, N4_RUN)
    consts = consts_for(T, cp, relayp, inv)
    dp = inp + '/deleg.json'
    coh = X.coherence_169(cv, {'Identity', 'Remote', 'Scope', 'Ci', 'Tests', 'Trailer', 'FreeText'}, handoff_absent=True)
    cases = []
    rc_, dec = TB.construct(cvp, dp, hp, rruns, consts)
    pc = X.write_request(work + '/request-c.json', rc_)
    oc = accept4(T, pc, cvp, dp, hp, consts, vp, rp, rec['InstrumentExecutions'], rec['Runs'])
    cases.append(case('N4', 'N4-c', 'N4 con el cierre derivado de la norma corregida (H ausente: terminos de H omitidos) y la construccion de la ronda 3',
                      'ACCEPT_FACTS rc 0; R4 NOT_APPLICABLE; R5 R6 R7 MATCH; R9 MATCH (las 7 corridas dentro, clausemap incluida); tabla de la ronda 4 aceptada',
                      oc, oc['Decision'] == 'ACCEPT_FACTS' and oc['Checks'].get('R9') == 'MATCH' and na_of(oc) == ['R4'] and oc['Table']['Accepted'],
                      Request=req_brief(pc), Derivation=brief_derivation(dres), R4Rule={'Include': dec['Include'], 'Cause': dec['Cause']}))
    # N4-d: plus an instrument run with an error (identity on the nonexistent commit), own session
    sess_d, _, rec_d, rruns_d = session('s-n4d', T, rs + Y.runs(T, inp, inv, ['identity-nx']), prim)
    relayd, _ = relay_save(W + '/s-n4d', 'relay-facts.json', sess_d, cvp, N4_RUN)
    consts_d = consts_for(T, cp, relayd, inv)
    rd, _ = TB.construct(cvp, dp, hp, rruns_d, consts_d)
    pd = X.write_request(W + '/s-n4d/request-d.json', rd)
    od = accept4(T, pd, cvp, dp, hp, consts_d, vp, rp, rec_d['InstrumentExecutions'], rec_d['Runs'])
    erun = [r for r in rec_d['Runs'] if r['Argv'][0] == 'identity']
    cases.append(case('N4', 'N4-d', 'N4-c mas una ejecucion con error (identity --current inexistente)',
                      'corrida con error conservada; REJECT_FACTS con R3, R5 y R9 MISMATCH (el commit inexistente no esta en el cierre derivado); tabla no aceptada',
                      od, bool(erun) and erun[0]['ErrorRun'] and od['Decision'] == 'REJECT_FACTS' and {'R3', 'R9'} <= set(mm_of(od)) and not od['Table']['Accepted'],
                      Request=req_brief(pd)))
    # N4-x: the Controller reads a valid delivery substituted for the absent one (handoff --file H=<real delivery copy>)
    real = json.loads(L.rbytes(Y.R2_INP + '/handoff-deliv.json').decode('utf-8'))
    real['Worktree'] = T
    subp = inp + '/handoff-subst.json'
    Y.jdump_local(subp, real)
    xrun = [('handoff-subst', ['handoff', '--repo', T] + Y.file_arg('H', subp) + ['--schema-rev', Y.G])]
    sess_x, _, rec_x, rruns_x = session('s-n4x', T, rs + xrun, prim)
    relayx, _ = relay_save(W + '/s-n4x', 'relay-facts.json', sess_x, cvp, N4_RUN)
    consts_x = consts_for(T, cp, relayx, inv)
    rx, _ = TB.construct(cvp, dp, hp, rruns_x, consts_x)
    px = X.write_request(W + '/s-n4x/request-x.json', rx)
    ox = accept4(T, px, cvp, dp, hp, consts_x, vp, rp, rec_x['InstrumentExecutions'], rec_x['Runs'])
    cases.append(case('N4', 'N4-x', 'N4-c mas una lectura de una entrega valida sustituida por la ausente (handoff --file H=copia de la entrega real), con el cierre derivado',
                      'REJECT_FACTS: R9 MISMATCH en esa corrida (C6 no tiene H); tabla no aceptada',
                      ox, ox['Decision'] == 'REJECT_FACTS' and 'R9' in mm_of(ox) and any('handoff' in k for k in ox['R9Runs']) and not ox['Table']['Accepted'],
                      Request=req_brief(px)))
    # N4-x with the substituted closure (the real delivery in C6): the acceptor alone accepts; the cotejo rejects
    Es, cs_, ds_ = Y.eic('T1-n4', Y.D_RUN, Y.H_RUN, inp, h_mode='pinned', h_blob=Y.blob_file(subp))
    cs_ = dict(cs_, **{ds_['H']: subp})
    vsub = Y.vector(T, Y.jdump_local(work + '/E-subst.json', Es), inv, ds_, cs_)
    cps, rps, _ = Y.derive_to(work, 'closure-n4-subst', vsub)
    consts_s = consts_for(T, cps, relayx, inv)
    rsq, _ = TB.construct(cvp, dp, hp, rruns_x, consts_s)
    psq = X.write_request(W + '/s-n4x/request-x-subst.json', rsq)
    osq = accept4(T, psq, cvp, dp, hp, consts_s, vp, rps, rec_x['InstrumentExecutions'], rec_x['Runs'])
    cases.append(case('N4', 'N4-x-subst', 'La misma sesion con el cierre derivado de la entrega sustituida (control negativo: entrada inexistente sustituida por una valida)',
                      'el aceptante solo da ACCEPT_FACTS (R9 MATCH: el cierre inflado admite la lectura); la tabla de la ronda 4 lo rechaza por el cotejo del cierre y del registro',
                      osq, osq['Decision'] == 'ACCEPT_FACTS' and not osq['Table']['Accepted']
                      and any(v.startswith('CLOSURE_NOT_DERIVED') for v in osq['Table']['ClosureViolations']), Request=req_brief(psq)))
    sv = {'delegation.v2': X.validate('delegation.v2.schema.json', deleg), 'controller-verification.v2': X.validate('controller-verification.v2.schema.json', cv),
          'relay-record.v2': rsv}
    write_group('p9-n4.json', {'Schema': 'r4-bench/p9-n4', 'Clone': info, 'HandoffAbsent': {'Path': hp, 'Exists': os.path.exists(hp)},
                               'SchemaValidation': sv, 'SchemaValid': all(not v for v in sv.values()),
                               'Session': {k: sess[k] for k in ('LogPath', 'LogSha256', 'ZeroWriteToolRuns', 'Executions')},
                               'Reconstruction': rec_brief(rec), 'ControllerChecks': dict((c, cv['Checks'][c]['Result']) for c in X.ORDER),
                               'Classification': [cv['Classification'], cv['Disposition'], cv['FailureClass']], 'Coherence169': coh, 'Cases': cases})


# ------------------------------------------------------------------ group nc1
def group_nc1():
    T, info = Y.prepare('nc1', DECISION)
    inp = W + '/c-nc1-in'

    def mut(h):
        h['CurrentSha'] = Y.NX
    deleg, hand = Y.write_inputs(T, inp, handoff_mut=mut)
    if L.git(T, 'cat-file', '-e', Y.NX).returncode == 0:
        raise SystemExit('NX exists')
    inv = Y.invocation(NC1_RUN)
    work = W + '/s-nc1'
    L.ensure_dir(work)
    E, cust, des = Y.eic('T1-nc1', Y.D_RUN, Y.H_RUN, inp)
    vec = Y.vector(T, Y.jdump_local(work + '/E.json', E), inv, des, cust)
    cp, rp, dres = Y.derive_to(work, 'closure-nc1', vec)
    vp = work + '/closure-nc1.vector.json'
    dp, hp = inp + '/deleg.json', inp + '/handoff.json'
    names = ['contract', 'hashes', 'cleantree', 'read-text', 'read-json', 'trx', 'handoff', 'freetext', 'clausemap']
    rs = Y.runs(T, inp, inv, names)
    prim = Y.prim_reads(Y.NX)
    sess, execs, rec, rruns = session('s-nc1', T, rs, prim)
    F = X.facts_by_cmd(rruns)
    out = {'Schema': 'r4-bench/p10-nc1', 'Clone': info, 'Nx': Y.NX, 'HandoffSchema': X.validate('worker-handoff.schema.json', hand),
           'Derivation': brief_derivation(dres), 'Variants': {}}
    cases, ctx = [], {}
    for variant, run_id in (('V1', NC1_RUN), ('V2', NC1_RUN)):
        cv, noin = cv_nc1(F, deleg, hand, sess['Primary'], variant, run_id)
        cvp = work + '/controller-output-%s.json' % variant
        L.wjson_local(cvp, cv)
        relayp, rsv = relay_save(work, 'relay-facts-%s.json' % variant, sess, cvp, run_id)
        consts = consts_for(T, cp, relayp, inv)
        ctx[variant] = (cvp, consts)
        out['Variants'][variant] = {'ControllerChecks': dict((c, cv['Checks'][c]['Result']) for c in X.ORDER),
                                    'Classification': [cv['Classification'], cv['Disposition'], cv['FailureClass']],
                                    'Coherence169': X.coherence_169(cv, noin), 'Schema': X.validate('controller-verification.v2.schema.json', cv), 'RelaySchema': rsv}
    for variant, include in (('V1', True), ('V2', False)):
        cvp, consts = ctx[variant]
        r_, d_ = TB.construct(cvp, dp, hp, rruns, consts)
        p_ = X.write_request(work + '/request-%s.json' % variant, r_)
        o_ = accept4(T, p_, cvp, dp, hp, consts, vp, rp, rec['InstrumentExecutions'], rec['Runs'])
        want4 = 'MATCH' if include else 'NOT_APPLICABLE'
        cases.append(case('nc1', variant + '-r4', variant + ' con el cierre derivado de la norma corregida (H.CurrentSha omitido, RevList vacia)',
                          'ACCEPT_FACTS; R4 %s; R9 MATCH (9 corridas dentro, clausemap incluida); tabla de la ronda 4 aceptada' % want4, o_,
                          d_['Include'] == include and o_['Decision'] == 'ACCEPT_FACTS' and o_['Checks'].get('R4') == want4 and o_['Checks'].get('R9') == 'MATCH'
                          and o_['Table']['Accepted'], Request=req_brief(p_)))
    # V3: CleanTree not_run (own session without cleantree)
    rs3 = [r for r in rs if r[0] != 'cleantree']
    sess3, _, rec3, rruns3 = session('s-nc1v3', T, rs3, prim)
    F3 = X.facts_by_cmd(rruns3)
    cv3, noin3 = cv_nc1(F3, deleg, hand, sess3['Primary'], 'V3', NC1_RUN)
    cvp3 = W + '/s-nc1v3/controller-output-V3.json'
    L.wjson_local(cvp3, cv3)
    relay3, rsv3 = relay_save(W + '/s-nc1v3', 'relay-facts-V3.json', sess3, cvp3, NC1_RUN)
    consts3 = consts_for(T, cp, relay3, inv)
    out['Variants']['V3'] = {'ControllerChecks': dict((c, cv3['Checks'][c]['Result']) for c in X.ORDER), 'Coherence169': X.coherence_169(cv3, noin3),
                             'Schema': X.validate('controller-verification.v2.schema.json', cv3), 'RelaySchema': rsv3}
    r3_, d3 = TB.construct(cvp3, dp, hp, rruns3, consts3)
    p3 = X.write_request(W + '/s-nc1v3/request-V3.json', r3_)
    o3 = accept4(T, p3, cvp3, dp, hp, consts3, vp, rp, rec3['InstrumentExecutions'], rec3['Runs'])
    cases.append(case('nc1', 'V3-r4', 'V3 (CleanTree not_run) con el cierre derivado', 'ACCEPT_FACTS; R4 NOT_APPLICABLE; R9 MATCH; tabla de la ronda 4 aceptada', o3,
                      (not d3['Include']) and o3['Decision'] == 'ACCEPT_FACTS' and o3['Checks'].get('R4') == 'NOT_APPLICABLE' and o3['Checks'].get('R9') == 'MATCH'
                      and o3['Table']['Accepted'], Request=req_brief(p3)))
    # nc1-err: an instrument run on the nonexistent CurrentSha
    sess_e, _, rec_e, rruns_e = session('s-nc1err', T, rs + Y.runs(T, inp, inv, ['identity-nx']), prim)
    cvp1, _ = ctx['V1']
    relaye, _ = relay_save(W + '/s-nc1err', 'relay-facts.json', sess_e, cvp1, NC1_RUN)
    consts_e = consts_for(T, cp, relaye, inv)
    re_, _ = TB.construct(cvp1, dp, hp, rruns_e, consts_e)
    pe = X.write_request(W + '/s-nc1err/request-err.json', re_)
    oe = accept4(T, pe, cvp1, dp, hp, consts_e, vp, rp, rec_e['InstrumentExecutions'], rec_e['Runs'])
    erun = [r for r in rec_e['Runs'] if r['Argv'][0] == 'identity']
    cases.append(case('nc1', 'nc1-err', 'V1 mas una ejecucion sobre el CurrentSha inexistente (sobre de error)',
                      'REJECT_FACTS con R3, R5 y R9 MISMATCH (el commit inexistente no esta en el cierre derivado); tabla no aceptada', oe,
                      bool(erun) and erun[0]['ErrorRun'] and oe['Decision'] == 'REJECT_FACTS' and {'R3', 'R9'} <= set(mm_of(oe)) and not oe['Table']['Accepted'],
                      Request=req_brief(pe)))
    # nc1-x: a read at a commit substituted for the nonexistent one (an existing commit outside the derived closure)
    other = L.gout(T, 'rev-parse', Y.OTHER)
    pick = None
    Ej = json.loads(L.rbytes(vec['E']).decode('utf-8'))
    for e in Ej['CanonicalInputs'] + Ej['AllowedTransitiveInputs'] + Ej['AutomaticInstructions']:
        b = DV.Derivation(vec).blob_at(other, e['Path'])
        if b == e['Blob'] and not e['Path'].endswith('.json'):
            pick = e['Path']
            break
    xr = [('read-text-other', ['read-text', '--repo', T, '--rev', other, '--path', pick, '--lines', '1-1'])]
    sess_x, _, rec_x, rruns_x = session('s-nc1x', T, rs + xr, prim)
    relayx, _ = relay_save(W + '/s-nc1x', 'relay-facts.json', sess_x, cvp1, NC1_RUN)
    consts_x = consts_for(T, cp, relayx, inv)
    rx, _ = TB.construct(cvp1, dp, hp, rruns_x, consts_x)
    px = X.write_request(W + '/s-nc1x/request-x.json', rx)
    ox = accept4(T, px, cvp1, dp, hp, consts_x, vp, rp, rec_x['InstrumentExecutions'], rec_x['Runs'])
    cases.append(case('nc1', 'nc1-x', 'V1 mas una lectura en un commit existente que sustituye al inexistente (%s:%s), con el cierre derivado' % (other[:8], pick),
                      'REJECT_FACTS: R9 MISMATCH en esa corrida (el commit no esta en el cierre derivado); tabla no aceptada', ox,
                      ox['Decision'] == 'REJECT_FACTS' and 'R9' in mm_of(ox) and not ox['Table']['Accepted'], Request=req_brief(px)))
    # nc1-x with the closure derived as if CurrentSha were that commit: the acceptor alone accepts; the cotejo rejects
    hs = json.loads(L.rbytes(hp).decode('utf-8'))
    hs['CurrentSha'] = other
    hsp = W + '/s-nc1x/handoff-as-other.json'
    Y.jdump_local(hsp, hs)
    Es = json.loads(L.rbytes(vec['E']).decode('utf-8'))
    Es['CanonicalInputs'] = [dict(e, Blob=Y.blob_file(hsp)) if e['Path'] == des['H'] else e for e in Es['CanonicalInputs']]
    vsub = dict(vec, E=Y.jdump_local(W + '/s-nc1x/E-as-other.json', Es), Custody=dict(cust, **{des['H']: hsp}))
    rsub = DV.derive_vector(vsub)
    cls = dict(rsub['Closure'])
    cls['Files'] = [dict(f, Sha256=Y.sha_file(hp)) if f['Name'] == 'H' else f for f in cls['Files']]
    cps = Y.jdump_local(W + '/s-nc1x/closure-as-other.json', cls)
    rps = record_for(W + '/s-nc1x', 'closure-as-other', cps, rsub['Omitted'])
    consts_s = consts_for(T, cps, relayx, inv)
    rsq, _ = TB.construct(cvp1, dp, hp, rruns_x, consts_s)
    psq = X.write_request(W + '/s-nc1x/request-x-subst.json', rsq)
    osq = accept4(T, psq, cvp1, dp, hp, consts_s, vp, rps, rec_x['InstrumentExecutions'], rec_x['Runs'])
    cases.append(case('nc1', 'nc1-x-subst', 'La misma sesion con el cierre derivado como si CurrentSha fuera ese commit (control negativo)',
                      'el aceptante solo da ACCEPT_FACTS (R9 MATCH); la tabla de la ronda 4 lo rechaza por el cotejo del cierre y del registro', osq,
                      osq['Decision'] == 'ACCEPT_FACTS' and not osq['Table']['Accepted'] and any(v.startswith('CLOSURE_NOT_DERIVED') for v in osq['Table']['ClosureViolations']),
                      Request=req_brief(psq)))
    out['Session'] = {k: sess[k] for k in ('LogPath', 'LogSha256', 'ZeroWriteToolRuns', 'Executions')}
    out['Reconstruction'] = rec_brief(rec)
    out['Cases'] = cases
    write_group('p10-nc1.json', out)


# ------------------------------------------------------------------ group indep: the independent implementation on every vector
def group_indep():
    vecs = []
    for root, _d, fs in os.walk(W):
        for f in fs:
            if f.endswith('.vector.json'):
                vecs.append(os.path.join(root, f).replace('\\', '/'))
    syn = 'D:/r62-a6r4/indep/synvec'
    synv = []
    for root, _d, fs in os.walk('D:/r62-a6r4/indep/vectors') if os.path.isdir('D:/r62-a6r4/indep/vectors') else []:
        synv += [os.path.join(root, f).replace('\\', '/') for f in fs if f.endswith('.json')]
    for root, _d, fs in os.walk(syn) if os.path.isdir(syn) else []:
        synv += [os.path.join(root, f).replace('\\', '/') for f in fs if f.endswith('.json') and 'vector' in f.lower()]
    cases = []
    tmp = W + '/indep-out'
    L.rmtree(tmp)
    L.ensure_dir(tmp)
    agree = 0
    rows = []
    for i, vp in enumerate(sorted(vecs) + sorted(synv)):
        try:
            vec = json.loads(L.rbytes(vp).decode('utf-8'))
            if not all(k in vec for k in ('Repo', 'E', 'I', 'Designation')):
                continue
        except Exception:
            continue
        op = '%s/%03d.json' % (tmp, i)
        p = subprocess.run([sys.executable, '-I', INDEP, vp, op], capture_output=True, stdin=subprocess.DEVNULL, env=L.genv(), timeout=900)
        try:
            mine, mine_err = DV.derive_vector(vec), None
        except Exception as e:
            mine, mine_err = None, '%s: %s' % (type(e).__name__, str(e)[:200])
        if p.returncode != 0 or not os.path.isfile(op) or mine is None:
            both = (p.returncode != 0 or not os.path.isfile(op)) and mine is None
            agree += both
            rows.append({'Vector': vp, 'Agree': both, 'BothBenchError': both, 'IndepRc': p.returncode, 'MineError': mine_err,
                         'Stderr': p.stderr.decode('utf-8', 'replace')[-400:]})
            continue
        theirs = json.loads(L.rbytes(op).decode('utf-8'))
        d = Y.closure_diff(theirs['Closure'], mine['Closure'])
        om_t = sorted((o['Term'], o['Cause']) for o in theirs['Omitted'])
        om_m = Y.omitted_pairs(mine['Omitted'])
        ok = d == {} and om_t == om_m
        agree += ok
        rows.append({'Vector': vp, 'Agree': ok, 'ClosureDiff': d, 'OmittedIndep': om_t, 'OmittedMine': om_m, 'Counts': counts(mine['Closure'])})
    cases.append(case('indep', 'INDEP-ALL', 'Implementacion independiente (escrita aparte, solo desde el texto normativo) frente a derive_r4.py en cada vector del banco y del propio agente',
                      'mismo cierre y mismos terminos omitidos con la misma causa en todos los vectores',
                      {'Vectors': len(rows), 'Agree': agree}, rows and agree == len(rows), Rows=rows,
                      IndepSha256=Y.sha_file(INDEP) if os.path.isfile(INDEP) else None))
    write_group('p11-indep.json', {'Schema': 'r4-bench/p11-indep', 'Cases': cases})


# ------------------------------------------------------------------ summary
def group_summary():
    allc = []
    files = {}
    for n in ('p7-derive.json', 'p8-v.json', 'p9-n4.json', 'p10-nc1.json', 'p11-indep.json'):
        p = os.path.join(HERE, n)
        d = json.loads(L.rbytes(p).decode('ascii'))
        files[n] = L.sha256(L.rbytes(p))
        allc += [{'Group': c['Group'], 'Case': c['Case'], 'Expected': c['Expected'], 'Holds': c['Holds']} for c in d['Cases']]
    for n in ('derive_r4.py', 'table_r4.py', 'lib_r4.py', 'run_r4.py', 'lib_r1r4.py', 'lib_r3.py', 'reconstruct_runs.py', 'table_r3.py'):
        files[n] = L.sha256(L.rbytes(os.path.join(HERE, n)))
    write_group('summary.json', {'Schema': 'r4-bench/summary', 'Cases': allc, 'Total': len(allc), 'Holding': sum(1 for c in allc if c['Holds']),
                                 'Files': files, 'Versions': L.versions()})


GROUPS = {'derive': group_derive, 'v': group_v, 'n4': group_n4, 'nc1': group_nc1, 'indep': group_indep, 'summary': group_summary}

if __name__ == '__main__':
    which = sys.argv[1] if len(sys.argv) > 1 else 'all'
    order = ['derive', 'v', 'n4', 'nc1', 'indep', 'summary'] if which == 'all' else [which]
    for g in order:
        GROUPS[g]()
