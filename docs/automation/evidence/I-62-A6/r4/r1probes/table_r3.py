# table_r3.py - pure evaluator of the round-3 candidate rule (I-62, A-6 round 3 bench). Stdlib only.
# Reads files, never writes, never runs a process. Two parts:
#   construct(...)  : the mechanical request construction of the supervision (R4 inclusion rule, data-condition NA).
#   evaluate(...)   : recomputes that construction from the Controller output and the files, compares it with the
#                     request actually given to the acceptor, and applies the acceptance table to the acceptor output.
#
# Rule under test (round-3 candidate):
#   * A VERIFY invocation "with executions" = its session log (complete, readable) has >= 1 instrument execution.
#     Only those get a request and an acceptor run; zero executions -> no request, no acceptor.
#   * R4: include Verification (Controller output + delegation + delivery pinned by SHA-256, IgnorePath, TermsRev,
#     MainSha) iff (1) the three files exist and are readable AND (2) none of Identity, Scope, CleanTree, Trailer,
#     FreeText is not_run in the Controller output. Otherwise omit it and declare NotApplicable.R4 with the cause.
#   * R5 NA iff Runs has no identity/diff/scope/hashes; R6 NA iff Runs has no clausemap AND (Verification omitted OR
#     its AuthorityResolution is empty); R7 NA iff Runs has no scope/contract/clausemap. R10: RelayFacts always.
#   * Accepted iff rc 0, Decision ACCEPT_FACTS, R1 R2 R3 R9 R10 MATCH, R8 MANUAL, R4 MATCH when Verification was
#     included / NOT_APPLICABLE only when omitted by the R4 rule, R5 R6 R7 MATCH or NOT_APPLICABLE only by their data
#     condition; Classification EXECUTION_VERIFIED is never accepted without R4 MATCH; the request must equal the
#     recomputed construction (and its Runs the executions reconstructed from the session log, when given).
import hashlib
import json
import os
import sys

REQUEST_SCHEMA = 'rackcad-bmin-accept-request/0-experimental'
CV_SCHEMA = 'rackcad-controller-verification/v2'
R4_FIVE = ('Identity', 'Scope', 'CleanTree', 'Trailer', 'FreeText')
R5_COMMANDS = frozenset(['identity', 'diff', 'scope', 'hashes'])
R7_COMMANDS = frozenset(['scope', 'contract', 'clausemap'])
NEVER_NA = ('R1', 'R2', 'R3', 'R8', 'R9', 'R10')
MUST_MATCH = ('R1', 'R2', 'R3', 'R9', 'R10')


def sha256(b):
    return hashlib.sha256(b).hexdigest()


def same_path(a, b):
    if not isinstance(a, str) or not isinstance(b, str) or not a or not b:
        return False
    return os.path.normcase(os.path.abspath(a)) == os.path.normcase(os.path.abspath(b))


def _read(path):
    try:
        with open(path, 'rb') as fh:
            return fh.read()
    except (OSError, TypeError, ValueError):
        return None


def _json(data):
    try:
        return json.loads(data.decode('utf-8'))
    except (AttributeError, UnicodeDecodeError, ValueError):
        return None


# ------------------------------------------------------------------ construction
def r4_decision(cv_path, deleg_path, handoff_path):
    """R4 inclusion rule. Returns {Include, Cause, Files, ControllerReadable, Classification, NotRun, ArNonEmpty}."""
    files = {}
    missing = []
    for label, p in (('Controller', cv_path), ('Delegation', deleg_path), ('Handoff', handoff_path)):
        data = _read(p)
        if data is None:
            missing.append(label)
            files[label] = None
        else:
            files[label] = {'Path': p, 'Sha256': sha256(data)}
    cv = _json(_read(cv_path)) if files['Controller'] else None
    res = {'Files': files, 'Missing': missing, 'ControllerReadable': isinstance(cv, dict) and cv.get('Schema') == CV_SCHEMA}
    not_run, unread = [], []
    if res['ControllerReadable']:
        checks = cv.get('Checks') if isinstance(cv.get('Checks'), dict) else {}
        for name in R4_FIVE:
            r = checks.get(name, {}).get('Result') if isinstance(checks.get(name), dict) else None
            if r == 'not_run':
                not_run.append(name)
            elif r not in ('pass', 'fail'):
                unread.append(name)
        ar = cv.get('AuthorityResolution')
        res['ArNonEmpty'] = isinstance(ar, list) and len(ar) > 0
        res['Classification'] = cv.get('Classification')
    else:
        res['ArNonEmpty'] = False
        res['Classification'] = None
    res['NotRun'] = not_run
    res['UnreadableResults'] = unread
    causes = []
    if missing:
        causes.append('inputs absent or unreadable: ' + ','.join(missing))
    if files['Controller'] and not res['ControllerReadable']:
        causes.append('Controller output is not a readable controller-verification/v2')
    if not_run:
        causes.append('not_run in the Controller output: ' + ','.join(not_run))
    res['Include'] = not causes and not unread
    res['Cause'] = '; '.join(causes) if causes else None
    # a Controller output whose five results cannot be read is a construction failure (fail closed), never an NA
    res['ConstructionFailure'] = bool(files['Controller'] and (not res['ControllerReadable'] or unread))
    return res


def na_conditions(run_cmds, include_verification, ar_non_empty):
    cmds = set(run_cmds)
    return {'R5': not (cmds & R5_COMMANDS),
            'R6': ('clausemap' not in cmds) and ((not include_verification) or (not ar_non_empty)),
            'R7': not (cmds & R7_COMMANDS)}


def construct(cv_path, deleg_path, handoff_path, runs, consts):
    """Mechanical request of the supervision. runs: [{'Name','Envelope','Argv'}]; consts: Repo, Tool, Interpreter,
    Closure {Path, Sha256}, RelayFacts {Path, CitedSha256}, IgnorePath, TermsRev, MainSha."""
    dec = r4_decision(cv_path, deleg_path, handoff_path)
    req = {'Schema': REQUEST_SCHEMA, 'Repo': consts['Repo'], 'Tool': consts['Tool'], 'Interpreter': consts['Interpreter'],
           'Runs': runs, 'Closure': consts['Closure'], 'RelayFacts': consts['RelayFacts']}
    na = {}
    if dec['Include']:
        f = dec['Files']
        req['Verification'] = {'Controller': f['Controller'], 'Delegation': f['Delegation'], 'Handoff': f['Handoff'],
                               'IgnorePath': consts['IgnorePath'], 'TermsRev': consts['TermsRev'], 'MainSha': consts['MainSha']}
    else:
        na['R4'] = 'R4 omitted by the round-3 rule: ' + (dec['Cause'] or 'construction failure')
    cond = na_conditions([r['Argv'][0] for r in runs], dec['Include'], dec['ArNonEmpty'])
    reasons = {'R5': 'Runs has no identity/diff/scope/hashes', 'R6': 'Runs has no clausemap and no AuthorityResolution rows to compare',
               'R7': 'Runs has no scope/contract/clausemap'}
    for rid in ('R5', 'R6', 'R7'):
        if cond[rid]:
            na[rid] = 'data condition: ' + reasons[rid]
    if na:
        req['NotApplicable'] = na
    return req, dec


# ------------------------------------------------------------------ evaluation
def check_construction(req, cv_path, deleg_path, handoff_path, consts, executions=None, reconstructed=None):
    """Recomputes the construction and lists every way the given request departs from it."""
    v = []
    if executions == 0:
        v.append('ZERO_EXECUTIONS: the session log records no instrument execution: no request and no acceptor run')
    if not isinstance(req, dict) or req.get('Schema') != REQUEST_SCHEMA:
        return v + ['REQUEST_UNREADABLE'], None
    dec = r4_decision(cv_path, deleg_path, handoff_path)
    if dec['ConstructionFailure']:
        v.append('CONTROLLER_OUTPUT_UNREADABLE: the five R4 results cannot be read')
    na = req.get('NotApplicable') if isinstance(req.get('NotApplicable'), dict) else {}
    ver = req.get('Verification')
    if dec['Include']:
        if ver is None:
            v.append('R4_RULE: Verification omitted although the rule requires it')
        else:
            f = dec['Files']
            for k in ('Controller', 'Delegation', 'Handoff'):
                got = ver.get(k) if isinstance(ver, dict) else None
                if not (isinstance(got, dict) and same_path(got.get('Path'), f[k]['Path']) and got.get('Sha256') == f[k]['Sha256']):
                    v.append('R4_RULE: Verification.%s differs from the file pinned by the rule' % k)
            for k in ('IgnorePath', 'TermsRev', 'MainSha'):
                if not isinstance(ver, dict) or ver.get(k) != consts.get(k):
                    v.append('R4_RULE: Verification.%s differs' % k)
        if 'R4' in na:
            v.append('R4_RULE: NotApplicable.R4 declared although the rule requires Verification')
    else:
        if ver is not None:
            v.append('R4_RULE: Verification included although the rule omits it (%s)' % dec['Cause'])
        if 'R4' not in na:
            v.append('R4_RULE: NotApplicable.R4 not declared although the rule omits Verification')
    runs = req.get('Runs') if isinstance(req.get('Runs'), list) else []
    cmds = [r.get('Argv')[0] for r in runs if isinstance(r, dict) and isinstance(r.get('Argv'), list) and r.get('Argv')]
    cond = na_conditions(cmds, dec['Include'], dec['ArNonEmpty'])
    for rid in ('R5', 'R6', 'R7'):
        if cond[rid] and rid not in na:
            v.append('%s_DATA: data condition holds but NotApplicable.%s is not declared' % (rid, rid))
        if (not cond[rid]) and rid in na:
            v.append('%s_DATA: NotApplicable.%s declared although Runs/Verification supply data' % (rid, rid))
    for rid in NEVER_NA:
        if rid in na:
            v.append('NA_FORBIDDEN: NotApplicable.%s' % rid)
    rf = req.get('RelayFacts')
    data = _read(rf.get('Path')) if isinstance(rf, dict) else None
    if data is None or rf.get('CitedSha256') != sha256(data):
        v.append('R10: RelayFacts absent or not pinned to the file')
    cl = req.get('Closure')
    cdata = _read(cl.get('Path')) if isinstance(cl, dict) else None
    if cdata is None or cl.get('Sha256') != sha256(cdata):
        v.append('R9: Closure absent or not pinned to the file')
    if not runs:
        v.append('RUNS_EMPTY: a request needs at least one execution')
    if reconstructed is not None:
        mine = []
        for r in runs:
            env = _read(r.get('Envelope')) if isinstance(r, dict) else None
            mine.append((r.get('Argv') if isinstance(r, dict) else None, sha256(env) if env is not None else None))
        theirs = [(r['Argv'], r['EnvelopeSha256']) for r in reconstructed]
        if len(mine) != len(theirs):
            v.append('RUNS_VS_LOG: request has %d runs, the session log records %d instrument executions' % (len(mine), len(theirs)))
        elif mine != theirs:
            bad = [i for i, (a, b) in enumerate(zip(mine, theirs)) if a != b]
            v.append('RUNS_VS_LOG: runs differ from the reconstruction at positions ' + ','.join(str(i) for i in bad))
    return v, dec


def acceptance_rows(acc_rc, acc, include_verification, cond, classification):
    reasons = []
    checks = {}
    if isinstance(acc, dict):
        for c in acc.get('Checks') or []:
            if isinstance(c, dict):
                checks[c.get('Id')] = c.get('Status')
    if acc_rc != 0:
        reasons.append('acceptor exit code %r' % (acc_rc,))
    if not isinstance(acc, dict) or acc.get('Decision') != 'ACCEPT_FACTS':
        reasons.append('Decision %r' % (acc.get('Decision') if isinstance(acc, dict) else None))
    for rid in MUST_MATCH:
        if checks.get(rid) != 'MATCH':
            reasons.append('%s = %s (MATCH required)' % (rid, checks.get(rid)))
    if checks.get('R8') != 'MANUAL':
        reasons.append('R8 = %s (MANUAL required)' % checks.get('R8'))
    want4 = 'MATCH' if include_verification else 'NOT_APPLICABLE'
    if checks.get('R4') != want4:
        reasons.append('R4 = %s (%s required: Verification %s)' % (checks.get('R4'), want4, 'included' if include_verification else 'omitted by the rule'))
    for rid in ('R5', 'R6', 'R7'):
        want = 'NOT_APPLICABLE' if cond[rid] else 'MATCH'
        if checks.get(rid) != want:
            reasons.append('%s = %s (%s required by its data condition)' % (rid, checks.get(rid), want))
    if classification == 'EXECUTION_VERIFIED' and checks.get('R4') != 'MATCH':
        reasons.append('EXECUTION_VERIFIED without R4 MATCH')
    return reasons, checks


def evaluate(request_path, acc_rc, acc_stdout, cv_path, deleg_path, handoff_path, consts, executions=None, reconstructed=None):
    """Full table: construction recomputed + acceptance rows. Returns a canonical-ready dict."""
    req = _json(_read(request_path))
    cons, dec = check_construction(req, cv_path, deleg_path, handoff_path, consts, executions, reconstructed)
    acc = _json(acc_stdout) if isinstance(acc_stdout, (bytes, bytearray)) else acc_stdout
    rows, checks = [], {}
    if dec is not None and isinstance(req, dict):
        runs = req.get('Runs') if isinstance(req.get('Runs'), list) else []
        cmds = [r['Argv'][0] for r in runs if isinstance(r, dict) and isinstance(r.get('Argv'), list) and r.get('Argv')]
        cond = na_conditions(cmds, dec['Include'], dec['ArNonEmpty'])
        rows, checks = acceptance_rows(acc_rc, acc, dec['Include'], cond, dec['Classification'])
    else:
        rows = ['request unreadable']
    return {'Accepted': not cons and not rows, 'ConstructionViolations': cons, 'AcceptanceReasons': rows,
            'R4Rule': None if dec is None else {'Include': dec['Include'], 'Cause': dec['Cause'], 'NotRun': dec['NotRun'],
                                                'Missing': dec['Missing'], 'Classification': dec['Classification']},
            'Statuses': checks, 'Decision': acc.get('Decision') if isinstance(acc, dict) else None, 'AcceptorRc': acc_rc}


def main(argv):
    a = dict(zip(argv[::2], argv[1::2]))
    need = ('--request', '--acceptor-stdout', '--acceptor-rc', '--controller', '--delegation', '--handoff', '--consts')
    if any(k not in a for k in need):
        sys.stderr.write('usage: python -I table_r3.py ' + ' '.join(k + ' X' for k in need) + ' [--executions N] [--reconstruction R.json]\n')
        return 64
    consts = _json(_read(a['--consts']))
    rec = _json(_read(a['--reconstruction']))['Runs'] if '--reconstruction' in a else None
    ex = int(a['--executions']) if '--executions' in a else None
    res = evaluate(a['--request'], int(a['--acceptor-rc']), _read(a['--acceptor-stdout']), a['--controller'], a['--delegation'],
                   a['--handoff'], consts, ex, rec)
    sys.stdout.write(json.dumps(res, ensure_ascii=True, sort_keys=True, separators=(',', ':')) + '\n')
    return 0 if res['Accepted'] else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
