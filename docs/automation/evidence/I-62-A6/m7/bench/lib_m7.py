# lib_m7.py - helpers of the A6-7 preparation bench (I-62, A-6, decisiones §90 points 2-3): the round-4 bench libraries
# (byte copies: lib_r4 -> lib_r3 -> lib_r1r4, reconstruct_runs, table_r3; derive_r4) with this bench's work area, the
# delivery scenario of r2/independence (d), designations, inputs of every invocation (RAE §10 copies), E built by
# make_eic_verify.py, closures derived after designate_m7.py, sessions in the declared form and synthetic Controller outputs.
# Stdlib only. Never writes in the I-62 worktree, the main checkout, D:\r62-fixture, D:\r62-a6r2, D:\r62-a6r3 or D:\r62-a6r4.
import copy, importlib.util, json, os, shutil, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


Y = _load('lib_r4')                      # byte copy of round 4 (loads lib_r3, lib_r1r4, reconstruct_runs, table_r3, derive_r4)
X, L, R, TB, DV = Y.X, Y.L, Y.R, Y.TB, Y.DV
MK = _load('make_eic_verify')
DS = _load('designate_m7')
TM = _load('table_m7')

BENCH = 'D:/r62-a6m7/bench'
WORK = BENCH + '/work'
Y.WORK = BENCH                           # copies at BENCH/c-<name>
X.WORK = WORK                            # sessions at WORK/<label>
L.WORK = WORK
L.PIN = WORK + '/pin'
L.PIN_TOOL = L.PIN + '/bmin_facts.py'
L.PIN_ACC = L.PIN + '/bmin_accept.py'
L.BIN = WORK + '/bin'
L.HERE = HERE
INPUTS = HERE.replace('\\', '/') + '/inputs'
DESIG = HERE.replace('\\', '/') + '/designations'

TIP, RED, G, MAIN, AR = Y.TIP, Y.RED, Y.G, Y.MAIN, Y.AR
K, MAP, STATE, DEC = Y.K, Y.MAP, Y.STATE, Y.DEC
K_BLOB = '628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0'           # custodied contract (kit: Authorization StateRef 628d89af)
TRX_E = Y.TRX_E
D_RUN, H_RUN = Y.D_RUN, Y.H_RUN
INSTRUMENT = {'Path': 'docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py', 'Blob': L.B}
OTHER_REF = 'refs/remotes/origin/fx/u1-fx05'
OTHER_D_RUN = 'R20261011T080000Z-d000'                          # delegation RunId of "another run" (N7)
OTHER_H_RUN = 'R20261011T081000Z-a000'                          # work RunId of "another run" (N7, O6 pair)
RUN = {'V': Y.V_RUN, 'N4': 'R20261014T000000Z-a4n4', 'NC1': 'R20261014T000000Z-a4c1',          # V, N4, nc1: the r4 RunIds
       'NC2': 'R20261016T000000Z-m7c2', 'NC3': 'R20261016T000000Z-m7c3', 'N5': 'R20261016T000000Z-m7n5',
       'N6': 'R20261016T000000Z-m7n6', 'N7A': 'R20261016T000000Z-m7a7', 'N7AR': 'R20261016T000000Z-m7ar',
       'N7B': 'R20261016T000000Z-m7b7', 'M': 'R20261016T000000Z-m7m0'}
TASKDIR = {'V': 'T1', 'N4': 'T1-n4', 'NC1': 'T1-nc1', 'NC2': 'T1-nc2', 'NC3': 'T1-nc3', 'N5': 'T1-n5', 'N6': 'T1-n6',
           'N7A': 'T1-n7a', 'N7AR': 'T1-n7a', 'N7B': 'T1-n7b', 'M': 'T1-m7'}
NAMES_FULL = ['identity', 'diff', 'scope', 'contract', 'hashes', 'cleantree', 'handoff', 'freetext', 'clausemap', 'trx',
              'read-text', 'read-json']
DECISION = 'BANCO-m7 (sin disposicion: preparacion de A6-7, decisiones §90; no es una DecisionRef normativa)'


def sha_file(p):
    return X.sha_file(p)


def jdump(p, obj):
    L.wjson_local(p, obj)
    return p


def wdeliv(rel, obj):
    """Deliverable JSON (sanitized, sorted keys, ASCII, LF) under the bench directory."""
    p, h = L.write_deliverable_json(rel, obj)
    return p.replace('\\', '/'), h


def copy_file_deliv(src, rel):
    """Byte copy of a work file into the deliverable tree (inputs/), after a hygiene check (no user name, no e-mail)."""
    b = L.rbytes(src)
    s = b.decode('utf-8', 'replace')
    if L.sanitize_str(s) != s:
        raise SystemExit('hygiene: %s would need sanitizing; not copied' % src)
    dst = INPUTS + '/' + rel
    L.wbytes(dst, b)
    return dst


def invocation(case, task_id='T1', target=G):
    return {'UnitId': 'FX-U1', 'TaskId': task_id, 'Attempt': 0, 'RunId': RUN[case],
            'Target': {'Commit': target, 'Path': Y.TARGET_PATH, 'Blob': Y.TARGET_BLOB}}


def ignore_path(inv):
    return DS.ignore_path(inv)


def copy_repo(name):
    T = BENCH + '/c-' + name
    L.rmtree(T)
    shutil.copytree(Y.SRC_DELIV, T)
    if L.gout(T, 'rev-parse', 'HEAD') != G or L.gout(T, 'rev-parse', 'refs/remotes/origin/fx/u1') != G:
        raise SystemExit('unexpected copy of the delivery repository')
    return T


def prepare(name):
    """Copy of the delivery repository with refs preserved; P1 (loose-object write of the instrument from its blob) and P2
    (point-of-use check (i)-(iv) through pwsh) before the first run."""
    L.ensure_dir(WORK)
    pins = L.ensure_pins()
    T = copy_repo(name)
    p1 = L.p1(T, DECISION)
    p2 = L.p2(T, 'pwsh')
    info = {'T': T, 'Source': Y.SRC_DELIV, 'Head': L.gout(T, 'rev-parse', 'HEAD'),
            'P1': {'ReturnedId': p1['ReturnedId'], 'IdEqualsPinned': p1['IdEqualsPinned'], 'OnlyNewLooseObject': p1['OnlyNewLooseObject'], 'Held': p1['Held']},
            'P2': {'Launch': p2['Launch'], 'FailedSteps': p2['FailedSteps']},
            'Pins': dict((k, {'Blob': v['Blob'], 'Sha256': v['Sha256']}) for k, v in pins.items())}
    if not (p1['Held'] and p2['Launch']):
        raise SystemExit('P1/P2 failed on ' + T)
    return T, info


# ------------------------------------------------------------------ inputs (RAE §10: copies keeping TaskId/RunId/DelegationRunId/Attempt except the mutated field)
def write_inputs(T, inp, hand_mut=None, deleg_mut=None, hand_present=True):
    deleg, hand = Y.write_inputs(T, inp, handoff_mut=hand_mut, handoff_present=hand_present)
    if deleg_mut:
        deleg_mut(deleg)
        jdump(inp + '/deleg.json', deleg)
    return deleg, hand


def designation(case, T, inp, inv, d_run=D_RUN, h_run=H_RUN, instrument=True, taskdir=None, run_path=None, extra=None):
    td = taskdir or TASKDIR[case]
    des = {'Schema': DS.SCHEMA, 'Case': case, 'Repo': T, 'AuthorityRevision': AR, 'Invocation': inv,
           'RunPath': run_path if run_path is not None else ignore_path(inv)[:-len('output.json')],
           'Inputs': [{'Role': 'DELEG', 'Path': Y.deliv_path(td, d_run, 'output.json'), 'Custody': inp + '/deleg.json'},
                      {'Role': 'H', 'Path': Y.deliv_path(td, h_run, 'worker-handoff.json'), 'Custody': inp + '/handoff.json'},
                      {'Role': 'CONTRACT', 'Path': K, 'Custody': None, 'Blob': K_BLOB},
                      {'Role': 'TRX', 'Path': TRX_E, 'Custody': inp + '/fixture-tests.trx'}],
           'Instrument': INSTRUMENT if instrument else None, 'MapPath': MAP, 'Adapter': 'codex-cli'}
    if extra:
        des.update(extra)
    return des


def build_e(des, variant, work, label, instrument=True):
    E, rep = MK.build(des, variant, instrument=instrument)
    p = work + '/E-' + label + '.json'
    L.wbytes(p, MK.serialize(E))
    return p, E, rep


def derive(des, E_path, work, label, bypass=()):
    """designate_m7.check (fail closed), then derive_r4.derive_vector; writes designation, vector, closure and record."""
    L.ensure_dir(work)
    dp = jdump(work + '/' + label + '.designation.json', des)
    E = json.loads(L.rbytes(E_path).decode('utf-8'))
    chk = DS.check(des, E, bypass=bypass)
    out = {'Check': {'Ok': chk['Ok'], 'Problems': chk['Problems'], 'Bypassed': chk['Bypassed'], 'Inputs': chk['Inputs'], 'Names': chk['Names']},
           'DesignationPath': dp, 'EPath': E_path}
    vec = chk['Vector']      # built only without (non-bypassed) problems; a bypassed check only shows its effect
    if vec is None:
        out['Derived'] = False
        return out
    vec = dict(vec, E=E_path)
    vp = jdump(work + '/' + label + '.vector.json', vec)
    res = DV.derive_vector(vec)
    cp = jdump(work + '/' + label + '.closure.json', res['Closure'])
    rp = jdump(work + '/' + label + '.derivation.json',
               {'Schema': 'a6m7-closure-derivation/1', 'Designation': dp, 'DesignationSha256': sha_file(dp), 'Vector': vp,
                'VectorSha256': sha_file(vp), 'E': E_path, 'ESha256': sha_file(E_path), 'Closure': cp, 'ClosureSha256': sha_file(cp),
                'DesignationCheck': out['Check'], 'Omitted': res['Omitted'], 'Inputs': res['Inputs']})
    out.update({'Derived': True, 'VectorPath': vp, 'ClosurePath': cp, 'RecordPath': rp, 'Result': res})
    return out


def counts(c):
    return dict((k, len(c[k])) for k in ('Commits', 'RevPaths', 'Refs', 'Paths', 'Objects', 'Files'))


def brief(d):
    if not d.get('Derived'):
        return {'Derived': False, 'Problems': d['Check']['Problems']}
    res = d['Result']
    return {'Derived': True, 'Counts': counts(res['Closure']), 'Omitted': Y.omitted_pairs(res['Omitted']),
            'Files': sorted(f['Name'] for f in res['Closure']['Files']), 'Inputs': res['Inputs'], 'Bypassed': d['Check']['Bypassed']}


# ------------------------------------------------------------------ instrument orders (declared form) with the delivered CurrentSha as a parameter
def file_arg(name, p):
    return Y.file_arg(name, p)


def runs(T, inp, inv, names, current=G):
    k, d, h, t = inp + '/K.json', inp + '/deleg.json', inp + '/handoff.json', inp + '/fixture-tests.trx'
    allr = {
        'identity': lambda: ['identity', '--repo', T, '--branch', Y.BR, '--base', TIP, '--current', current, '--red', RED],
        'diff': lambda: ['diff', '--repo', T, '--from', TIP, '--to', current],
        'scope': lambda: ['scope', '--repo', T, '--from', TIP, '--to', current] + file_arg('DELEG', d),
        'contract': lambda: ['contract', '--repo', T] + file_arg('CONTRACT', k) + file_arg('DELEG', d),
        'hashes': lambda: ['hashes', '--repo', T, '--rev', G, '--paths'] + sorted(Y.DELIV_FILES),
        'cleantree': lambda: ['cleantree', '--repo', T, '--ignore-path', ignore_path(inv)],
        'handoff': lambda: ['handoff', '--repo', T] + file_arg('H', h) + ['--schema-rev', G],
        'freetext': lambda: ['freetext', '--repo', T] + file_arg('H', h) + ['--terms-rev', MAIN],
        'clausemap': lambda: ['clausemap', '--repo', T, '--main', MAIN, '--state-rev', G, '--state-path', STATE] + file_arg('CONTRACT', k) + ['--base', TIP],
        'trx': lambda: ['trx', '--repo', T] + file_arg('TRX', t) + ['--filter-substring', 'Subtract'],
        'read-text': lambda: ['read-text', '--repo', T, '--rev', G, '--path', DEC, '--lines', '1-1'],
        'read-json': lambda: ['read-json', '--repo', T, '--rev', G, '--path', K, '--pointer', '/Objective', '--pointer', '/Schema',
                              '--pointer', '/MainSha', '--pointer', '/AuthorityRevision'],
    }
    return [(n, allr[n]()) for n in names]


def session(label, T, rs, prim):
    sess, execs = X.verify_session(label, T, rs, prim)
    rec, rruns = X.reconstruct_into(sess['LogPath'], WORK + '/' + label + '/env')
    if not rec['Reconstructible']:
        raise SystemExit(label + ' log not reconstructible: ' + json.dumps(rec['Problems']))
    return sess, execs, rec, rruns


def rec_brief(rec):
    return {k: rec[k] for k in ('Complete', 'CommandExecutions', 'InstrumentExecutions', 'Reconstructible', 'Problems')}


def sess_brief(sess):
    return {k: sess[k] for k in ('LogPath', 'LogSha256', 'ZeroWriteToolRuns', 'Executions')}


# ------------------------------------------------------------------ synthetic Controller outputs (round-3/4 rules and texts)
def reclassify(cv):
    """The cv_t1 rule of round 3 (lib_r3): every check outside pass contributes its row disposition (Handoff: REWORK if
    Identity passes, otherwise STOP); strict maximum; FailureClass = first check outside pass in the 16.9 order."""
    ch = cv['Checks']
    ident_ok = ch['Identity']['Result'] == 'pass'
    disp = set()
    for c in X.ORDER:
        if ch[c]['Result'] != 'pass':
            disp.add('REWORK' if (c == 'Handoff' and ident_ok) else ('STOP' if c == 'Handoff' else X.ROW_DISP[c]))
    d = 'STOP' if 'STOP' in disp else ('BLOCKED' if 'BLOCKED' in disp else ('REWORK' if 'REWORK' in disp else 'NONE'))
    cv['Disposition'] = d
    cv['Classification'] = {'NONE': 'EXECUTION_VERIFIED', 'REWORK': 'EXECUTION_REWORK_REQUIRED', 'BLOCKED': 'EXECUTION_BLOCKED',
                            'STOP': 'EXECUTION_BLOCKED'}[d]
    cv['FailureClass'] = next((c for c in X.ORDER if ch[c]['Result'] != 'pass'), 'NONE')
    return cv


def cv_case(F, deleg, hand, prim, run_id, overrides=None, finding=None):
    """cv_t1 of round 3 (byte-copied lib_r3) for this invocation's RunId, then the mutated check of the control, reclassified."""
    L.CV_RUN = run_id
    cv = X.cv_t1(F, deleg, hand, prim)
    for c, v in (overrides or {}).items():
        cv['Checks'][c] = v
    if overrides:
        reclassify(cv)
    if finding:
        cv['Findings'] = list(cv['Findings']) + [finding]
    return cv


def cv_without_instrument(cv, excluded_cmds):
    """The same Controller results, with the Evidence of the checks whose instrument order is outside the R7-B closure
    rewritten to the primary Git read (the Controller computes them without the instrument)."""
    cv = copy.deepcopy(cv)
    owner = {'clausemap': 'Authority', 'contract': 'Contract', 'handoff': 'Handoff', 'freetext': 'FreeText', 'identity': 'Identity',
             'scope': 'Scope', 'cleantree': 'CleanTree', 'diff': 'Trailer'}
    for cmd in excluded_cmds:
        c = owner.get(cmd)
        if c and cv['Checks'][c]['Result'] in ('pass', 'fail'):
            cv['Checks'][c]['Evidence'] = ('sin instrumento: la orden %s queda fuera del cierre R7-B (A6-7, regla 7, limitar); '
                                           'resultado por la lectura Git primaria (banco m7)' % cmd)
    cv['Findings'] = list(cv['Findings']) + ['Variante R7-B (banco m7): ordenes del instrumento limitadas al cierre derivado; excluidas: '
                                             + ', '.join(sorted(excluded_cmds))]
    return cv


def consts_for(T, closure_path, relayp, inv):
    c = X.consts_for(T, closure_path, relayp)
    c['IgnorePath'] = ignore_path(inv)
    return c


def consts_m(T, closure_path):
    xsha, _ = L.exe_sha256()
    import platform
    return {'Repo': T, 'Tool': {'PinnedBlob': L.B, 'RunFrom': 'object', 'GitObject': {'GitDir': T, 'Blob': L.B}},
            'Interpreter': {'Python': platform.python_version(), 'ExecutableSha256': xsha},
            'Closure': {'Path': closure_path, 'Sha256': sha_file(closure_path)}}
