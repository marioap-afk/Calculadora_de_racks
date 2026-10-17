# table_m7.py - request construction and acceptance table of the A6-7 measurement M (I-62, A-6, decisiones §90), and the
# closure cotejo with the designation check (designate_m7.py) for every request of this bench. Stdlib only; never writes.
#
# M is not a role invocation of FX-02 (A6-7 rule 2) and produces no verification: its request carries no Verification and
# no RelayFacts and declares R4 and R10 not applicable (A6-7 rule 1: "R4 y R10 no aplicables, porque la medicion no produce
# una verificacion"; A6-5 rule 6, same wording for the prior test). R5, R6, R7: by their data condition (A6-5 rule 2).
# Accepted iff rc 0, ACCEPT_FACTS, R1 R2 R3 R9 MATCH, R8 MANUAL, R4 and R10 NOT_APPLICABLE, R5-R7 MATCH or NOT_APPLICABLE only
# by their data condition, Runs = the executions reconstructed from the session log, and the Closure / derivation record
# equal to a fresh derivation from the custodied vector (table_r4.closure_check), after the designation check.
import importlib.util, json, os, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


TB = _load('table_r3')
T4 = _load('table_r4')
DS = _load('designate_m7')
M_NA = {'R4': 'A6-7 rule 1: the measurement produces no verification (no Verification in the request)',
        'R10': 'A6-7 rule 1: the measurement has no relay facts (no RelayFacts in the request)'}


def construct_m(runs, consts):
    req = {'Schema': TB.REQUEST_SCHEMA, 'Repo': consts['Repo'], 'Tool': consts['Tool'], 'Interpreter': consts['Interpreter'],
           'Runs': runs, 'Closure': consts['Closure']}
    na = dict(M_NA)
    cond = TB.na_conditions([r['Argv'][0] for r in runs], False, False)
    reasons = {'R5': 'Runs has no identity/diff/scope/hashes', 'R6': 'Runs has no clausemap and there is no Verification',
               'R7': 'Runs has no scope/contract/clausemap'}
    for rid in ('R5', 'R6', 'R7'):
        if cond[rid]:
            na[rid] = 'data condition: ' + reasons[rid]
    req['NotApplicable'] = na
    return req, cond


def check_construction_m(req, executions=None, reconstructed=None):
    v = []
    if executions == 0:
        v.append('ZERO_EXECUTIONS')
    if not isinstance(req, dict) or req.get('Schema') != TB.REQUEST_SCHEMA:
        return ['REQUEST_UNREADABLE'], None
    if 'Verification' in req:
        v.append('M_RULE: Verification present in a measurement request')
    if 'RelayFacts' in req:
        v.append('M_RULE: RelayFacts present in a measurement request')
    na = req.get('NotApplicable') if isinstance(req.get('NotApplicable'), dict) else {}
    for rid in ('R4', 'R10'):
        if rid not in na:
            v.append('M_RULE: NotApplicable.%s not declared' % rid)
    runs = req.get('Runs') if isinstance(req.get('Runs'), list) else []
    cmds = [r.get('Argv')[0] for r in runs if isinstance(r, dict) and isinstance(r.get('Argv'), list) and r.get('Argv')]
    cond = TB.na_conditions(cmds, False, False)
    for rid in ('R5', 'R6', 'R7'):
        if cond[rid] and rid not in na:
            v.append('%s_DATA: data condition holds but NotApplicable.%s is not declared' % (rid, rid))
        if (not cond[rid]) and rid in na:
            v.append('%s_DATA: NotApplicable.%s declared although Runs supply data' % (rid, rid))
    for rid in ('R1', 'R2', 'R3', 'R8', 'R9'):
        if rid in na:
            v.append('NA_FORBIDDEN: NotApplicable.%s' % rid)
    cl = req.get('Closure')
    cdata = TB._read(cl.get('Path')) if isinstance(cl, dict) else None
    if cdata is None or cl.get('Sha256') != TB.sha256(cdata):
        v.append('R9: Closure absent or not pinned to the file')
    if not runs:
        v.append('RUNS_EMPTY')
    if reconstructed is not None:
        mine = []
        for r in runs:
            env = TB._read(r.get('Envelope')) if isinstance(r, dict) else None
            mine.append((r.get('Argv') if isinstance(r, dict) else None, TB.sha256(env) if env is not None else None))
        theirs = [(r['Argv'], r['EnvelopeSha256']) for r in reconstructed]
        if mine != theirs:
            v.append('RUNS_VS_LOG: runs differ from the reconstruction')
    return v, cond


def rows_m(acc_rc, acc, cond):
    reasons, checks = [], {}
    if isinstance(acc, dict):
        for c in acc.get('Checks') or []:
            if isinstance(c, dict):
                checks[c.get('Id')] = c.get('Status')
    if acc_rc != 0:
        reasons.append('acceptor exit code %r' % (acc_rc,))
    if not isinstance(acc, dict) or acc.get('Decision') != 'ACCEPT_FACTS':
        reasons.append('Decision %r' % (acc.get('Decision') if isinstance(acc, dict) else None))
    for rid in ('R1', 'R2', 'R3', 'R9'):
        if checks.get(rid) != 'MATCH':
            reasons.append('%s = %s (MATCH required)' % (rid, checks.get(rid)))
    if checks.get('R8') != 'MANUAL':
        reasons.append('R8 = %s (MANUAL required)' % checks.get('R8'))
    for rid in ('R4', 'R10'):
        if checks.get(rid) != 'NOT_APPLICABLE':
            reasons.append('%s = %s (NOT_APPLICABLE required in the measurement)' % (rid, checks.get(rid)))
    for rid in ('R5', 'R6', 'R7'):
        want = 'NOT_APPLICABLE' if cond[rid] else 'MATCH'
        if checks.get(rid) != want:
            reasons.append('%s = %s (%s required by its data condition)' % (rid, checks.get(rid), want))
    return reasons, checks


def designation_check(designation_path, vector_path):
    des = TB._json(TB._read(designation_path))
    vec = TB._json(TB._read(vector_path))
    E = TB._json(TB._read(vec.get('E'))) if isinstance(vec, dict) else None
    if not isinstance(des, dict) or not isinstance(E, dict):
        return ['DESIGNATION_UNREADABLE']
    r = DS.check(des, E)
    v = ['DESIGNATION: %s %s' % (p['Code'], p['Detail']) for p in r['Problems']]
    if r['Ok']:
        want = dict(r['Vector'], E=vec.get('E'))
        for k in ('Repo', 'I', 'Designation', 'Custody', 'MapPath'):
            if vec.get(k) != want.get(k):
                v.append('DESIGNATION: vector field %s is not the one the designation gives' % k)
    return v


def evaluate_m(request_path, acc_rc, acc_stdout, vector_path, record_path, designation_path, executions=None, reconstructed=None):
    req = TB._json(TB._read(request_path))
    cons, cond = check_construction_m(req, executions, reconstructed)
    acc = TB._json(acc_stdout) if isinstance(acc_stdout, (bytes, bytearray)) else acc_stdout
    rows, checks = rows_m(acc_rc, acc, cond) if cond is not None else (['request unreadable'], {})
    cv, fresh = T4.closure_check(request_path, vector_path, record_path)
    dv = designation_check(designation_path, vector_path)
    return {'Accepted': not cons and not rows and not cv and not dv, 'ConstructionViolations': cons, 'AcceptanceReasons': rows,
            'ClosureViolations': cv, 'DesignationViolations': dv, 'Statuses': checks,
            'Decision': acc.get('Decision') if isinstance(acc, dict) else None, 'AcceptorRc': acc_rc,
            'Omitted': [(o['Term'], o['Cause']) for o in fresh['Omitted']] if fresh else None}


def evaluate_v(request_path, acc_rc, acc_stdout, cv_path, deleg_path, handoff_path, consts, vector_path, record_path,
               designation_path, executions=None, reconstructed=None):
    """table_r4.evaluate (round-3 construction and table + closure cotejo), plus the designation check."""
    base = T4.evaluate(request_path, acc_rc, acc_stdout, cv_path, deleg_path, handoff_path, consts, vector_path, record_path,
                       executions, reconstructed)
    dv = designation_check(designation_path, vector_path)
    base = dict(base)
    base['DesignationViolations'] = dv
    base['Accepted'] = base['Accepted'] and not dv
    return base
