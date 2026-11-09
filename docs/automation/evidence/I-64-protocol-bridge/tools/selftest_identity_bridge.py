#!/usr/bin/env python3
"""Synthetic self-test of I64-IDENTITY-BRIDGE-01 (I-64 A-12). T01-T15 are the minimum cases ordered by the Coordinator; T16-T31 cover
the exact repository occurrence, mutation exactness, relay binding and parser hardening. Uses ONLY synthetic fixtures built in a temporary
directory: a deterministic Git repository with a local bare origin, schema-valid handoffs and relay records. No real I-64 evidence.
Usage: python selftest_identity_bridge.py --verifier <path> [--out <path>]. Exit 0 only when every case behaves as expected.
Deterministic LF-only JSON (no paths, no timestamps)."""
import argparse
import copy
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

BRANCH = 'synth/delivery'
SYNTHETIC = 'deadbeef' * 5
TASK, ATTEMPT, DELEG, WORK = 'SYNTH-TASK', 2, 'R20260101T000000Z-aaaa', 'R20260101T000100Z-bbbb'


def git_env():
    env = {k: v for k, v in os.environ.items() if not k.upper().startswith('GIT_')}
    env.update({'GIT_AUTHOR_NAME': 'Synthetic', 'GIT_AUTHOR_EMAIL': 'synthetic@example.invalid', 'GIT_COMMITTER_NAME': 'Synthetic',
                'GIT_COMMITTER_EMAIL': 'synthetic@example.invalid', 'GIT_AUTHOR_DATE': '2026-01-01T00:00:00+00:00',
                'GIT_COMMITTER_DATE': '2026-01-01T00:00:00+00:00', 'GIT_TERMINAL_PROMPT': '0'})
    return env


def g(repo, *args):
    p = subprocess.run(['git', '-C', repo] + list(args), capture_output=True, env=git_env())
    if p.returncode != 0:
        raise SystemExit('fixture git failed: %s %s' % (args, p.stderr[-300:]))
    return p.stdout.decode('ascii', 'replace').strip()


def write(path, data):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'wb') as f:
        f.write(data)


def commit(repo, files, msg):
    for rel, text in files.items():
        write(os.path.join(repo, *rel.split('/')), text.encode('utf-8'))
    g(repo, 'add', '-A')
    g(repo, 'commit', '-q', '-m', msg)
    return g(repo, 'rev-parse', 'HEAD')


def build(root):
    """Repository: base -> red -> verified on the bound branch, side commits, and a local bare origin at the verified commit."""
    bare, repo = os.path.join(root, 'origin.git'), os.path.join(root, 'repo')
    os.makedirs(bare)
    os.makedirs(repo)
    subprocess.run(['git', '-C', bare, 'init', '-q', '--bare'], capture_output=True, env=git_env(), check=True)
    g(repo, 'init', '-q')
    g(repo, 'config', 'core.autocrlf', 'false')
    g(repo, 'checkout', '-q', '-b', BRANCH)
    s = {'repo': repo, 'bare': bare}
    s['base'] = commit(repo, {'src/App.cs': 'class App {}\n', 'docs/readme.md': 'x\n'}, 'base')
    s['red'] = commit(repo, {'tests/NavTests.cs': 'class T {}\n'}, 'red')
    s['verified'] = commit(repo, {'src/App.cs': 'class App { int x; }\n'}, 'verified')
    g(repo, 'checkout', '-q', '-b', 'side', s['base'])
    s['side'] = commit(repo, {'src/Side.cs': 'class Side {}\n'}, 'side')
    g(repo, 'checkout', '-q', BRANCH)
    g(repo, 'remote', 'add', 'origin', bare)
    g(repo, 'push', '-q', 'origin', BRANCH)
    g(repo, 'fetch', '-q', 'origin')
    return s


def advance(s, files, msg):
    sha = commit(s['repo'], files, msg)
    g(s['repo'], 'push', '-q', 'origin', BRANCH)
    g(s['repo'], 'fetch', '-q', 'origin')
    return sha


def handoff(s, **over):
    d = {'Schema': 'rackcad-worker-handoff/v1', 'TaskId': TASK, 'RunId': WORK, 'DelegationRunId': DELEG, 'Initiative': 'I-SYNTH', 'Gate': 'F2', 'Attempt': ATTEMPT,
         'BaseSha': s['base'], 'RedSha': s['red'], 'CurrentSha': s['verified'], 'Branch': BRANCH, 'Worktree': s['repo'], 'Pushed': True, 'FilesChanged': ['src/App.cs', 'tests/NavTests.cs'],
         'WorkCompleted': 'Synthetic delivery at %s.' % s['verified'], 'TestsExecuted': [{'RunRef': 'green', 'Command': 'dotnet test', 'Phase': 'GREEN', 'TreeSha': s['verified'], 'ResultsFile': None}],
         'TestResults': [{'RunRef': 'green', 'Selected': 3, 'Passed': 3, 'Failed': 0, 'Skipped': 0, 'FailedTests': []}], 'Evidence': ['synthetic'], 'UnexpectedFindings': [], 'KnownLimitations': [],
         'Deviations': [], 'OpenQuestions': [], 'WorkerStatus': 'IMPLEMENTATION_COMPLETE', 'Disposition': 'NONE', 'TriggeredStopConditions': [], 'RecommendedNextAction': 'none',
         'Worker': {'Provider': 'Synthetic', 'ModelRequested': 'm', 'EffortRequested': 'high', 'Trailer': 'Co-Authored-By: Synthetic <synthetic@example.invalid>'}}
    d.update(over)
    return d


def dumps(d):
    return (json.dumps(d, indent=2, ensure_ascii=False) + '\n').encode('utf-8')


def relay(raw_hash, **over):
    r = {'Schema': 'rackcad-relay-record/v1', 'TaskId': TASK, 'RunId': WORK, 'Attempt': ATTEMPT, 'Phase': 'WORK', 'Cession': {'StartUtc': '2026-01-01T00:00:00Z', 'EndUtc': '2026-01-01T01:00:00Z', 'SessionOperated': False},
         'Outcome': {'Kind': 'COMPLETED', 'OutputSha256': raw_hash.upper(), 'OutputWrittenUtc': '2026-01-01T00:30:00.500000+00:00'}}
    for k, v in over.items():
        if k in ('Cession', 'Outcome'):
            r[k].update(v)
        else:
            r[k] = v
    return r


VERIFIER = None


def run_case(cid, desc, s, real=None, mut=None, relay_over=None, args_over=None, mut_raw=None, real_raw=None, relay_hash=None, expect=None, root=None):
    case_dir = os.path.join(root, cid)
    os.makedirs(case_dir)
    real_bytes = real_raw if real_raw is not None else dumps(real if real is not None else handoff(s))
    if mut_raw is not None:
        mut_bytes = mut_raw
    else:
        mut_bytes = dumps(mut if mut is not None else handoff(s, CurrentSha=SYNTHETIC))
    h = relay_hash if relay_hash is not None else hashlib.sha256(mut_bytes).hexdigest()
    rec = relay(h, **(relay_over or {}))
    paths = {n: os.path.join(case_dir, n + '.json') for n in ('real', 'mut', 'relay')}
    write(paths['real'], real_bytes)
    write(paths['mut'], mut_bytes)
    write(paths['relay'], dumps(rec))
    args = {'--repo': s['repo'], '--task-id': TASK, '--attempt': str(ATTEMPT), '--delegation-run-id': DELEG, '--work-run-id': WORK, '--branch': BRANCH, '--base-sha': s['base'], '--red-sha': s['red'],
            '--current-sha': s['verified'], '--synthetic-sha': SYNTHETIC, '--real-handoff': paths['real'], '--mutated-handoff': paths['mut'], '--relay-record': paths['relay']}
    args.update(args_over or {})
    cmd = [sys.executable, '-I', VERIFIER]
    for k, v in args.items():
        cmd += [k, v]
    p = subprocess.run(cmd, capture_output=True)
    d = json.loads(p.stdout.decode('ascii'))
    codes = sorted({r['Code'] for r in d['Reasons']})
    got = {'HandoffIntegrity': d['HandoffIntegrity'], 'Identity': d['Identity'], 'ControlResult': d['ControlResult'], 'FailClosed': d['FailClosed'], 'ExitCode': p.returncode}
    want = {k: expect[k] for k in got}
    asserts = {k: got[k] == want[k] for k in got}
    asserts['ReasonCode'] = expect['Code'] in codes if expect.get('Code') else codes == []
    return {'Id': cid, 'Description': desc, 'Expected': want, 'ExpectedCode': expect.get('Code'), 'Got': got, 'ReasonCodes': codes, 'Asserts': asserts, 'Ok': all(asserts.values())}


E = dict
PASSCTRL = E(HandoffIntegrity='PASS', Identity='FAIL', ControlResult='PASS', FailClosed=False, ExitCode=0)


def hi_fail(code):
    return E(HandoffIntegrity='FAIL', Identity='NOT_EVALUATED', ControlResult='FAIL', FailClosed=True, ExitCode=1, Code=code)


def closed(code):
    return E(HandoffIntegrity='PASS', Identity='NOT_EVALUATED', ControlResult='FAIL', FailClosed=True, ExitCode=1, Code=code)


def unread(code):
    return E(HandoffIntegrity='NOT_EVALUATED', Identity='NOT_EVALUATED', ControlResult='FAIL', FailClosed=True, ExitCode=1, Code=code)


def control_fail(identity, code):
    return E(HandoffIntegrity='PASS', Identity=identity, ControlResult='FAIL', FailClosed=False, ExitCode=1, Code=code)


def fresh(root, name):
    d = os.path.join(root, name)
    os.makedirs(d)
    return build(d)


def main():
    global VERIFIER
    p = argparse.ArgumentParser(add_help=False, allow_abbrev=False)
    p.add_argument('--verifier', required=True)
    p.add_argument('--out', default=None)
    a = p.parse_args()
    VERIFIER = os.path.abspath(a.verifier)
    root = tempfile.mkdtemp(prefix='i64id-')
    cases = []
    try:
        s = fresh(root, 'main')
        cr = os.path.join(root, 'cases')
        os.makedirs(cr)

        def c(cid, desc, st=None, **kw):
            cases.append(run_case(cid, desc, st or s, root=cr, **kw))

        H = handoff
        M = lambda **o: H(s, CurrentSha=SYNTHETIC, **o)
        c('T01', 'real handoff and real repository state: integrity and Identity both pass, and no control result', mut=H(s), expect=control_fail('PASS', 'MUTATION_NOT_REJECTED_BY_IDENTITY'))
        c('T02', 'only CurrentSha mutated, to an existing commit: integrity passes and Identity fails', mut=H(s, CurrentSha=s['red']), expect=control_fail('FAIL', 'MUTATED_SHA_NOT_AUTHORIZED'))
        c('T03', 'TaskId also changed', mut=M(TaskId='OTHER'), expect=hi_fail('HANDOFF_TASK_ID_MISMATCH'))
        c('T04', 'Attempt also changed', mut=M(Attempt=1), expect=hi_fail('HANDOFF_ATTEMPT_MISMATCH'))
        c('T05', 'DelegationRunId also changed', mut=M(DelegationRunId='R20260101T000000Z-cccc'), expect=hi_fail('HANDOFF_DELEGATION_RUN_ID_MISMATCH'))
        c('T06', 'WorkRunId also changed', mut=M(RunId='R20260101T000100Z-dddd'), expect=hi_fail('HANDOFF_WORK_RUN_ID_MISMATCH'))
        c('T07', 'Branch also changed', mut=M(Branch='other/branch'), expect=hi_fail('HANDOFF_BRANCH_MISMATCH'))
        c('T08', 'the relay output hash does not match the mutated handoff', relay_hash='0' * 64, expect=hi_fail('RELAY_OUTPUT_HASH_MISMATCH'))
        c('T09', 'the mutated CurrentSha equals HEAD: the control fails', mut=H(s, CurrentSha=s['verified']), expect=control_fail('PASS', 'MUTATION_NOT_REJECTED_BY_IDENTITY'))
        t10 = fresh(root, 't10')
        subprocess.run(['git', '-C', t10['bare'], 'update-ref', 'refs/heads/' + BRANCH, t10['red']], capture_output=True, env=git_env(), check=True)
        c('T10', 'HEAD and the remote branch disagree', st=t10, real=H(t10), mut=H(t10, CurrentSha=SYNTHETIC), expect=closed('HEAD_REMOTE_DISAGREE'))
        c('T11', 'BaseSha is not an ancestor of RedSha', real=H(s, BaseSha=s['verified']), mut=M(BaseSha=s['verified']), args_over={'--base-sha': s['verified']}, expect=closed('BASE_NOT_ANCESTOR'))
        c('T12', 'RedSha is not on the expected chain', real=H(s, RedSha=s['side']), mut=M(RedSha=s['side']), args_over={'--red-sha': s['side']}, expect=closed('RED_NOT_ON_CHAIN'))
        c('T13', 'malformed handoff JSON', mut_raw=b'{"Schema": "rackcad-worker-handoff/v1",', expect=unread('MUTATED_UNREADABLE'))
        nested = copy.deepcopy(M())
        nested['TestResults'][0]['Passed'] = 2
        c('T14', 'a second mutation hidden in a nested field', mut=nested, expect=hi_fail('MUTATION_NOT_ONLY_CURRENT_SHA'))
        c('T15', 'nonexistent CurrentSha only: the exact nc1 substitute passes', expect=PASSCTRL)

        # Repository occurrence: exact semantics only
        desc = fresh(root, 'desc')
        advance(desc, {'docs/automation/evidence/custody.md': 'custody\n'}, 'docs-only descendant')
        c('T16', 'HEAD is a docs-only descendant of the verified SHA: no custody semantics, fails closed', st=desc, real=H(desc), mut=H(desc, CurrentSha=SYNTHETIC), expect=closed('HEAD_NOT_VERIFIED_SHA'))
        other = fresh(root, 'other')
        g(other['repo'], 'checkout', '-q', '-b', 'not-the-branch')
        c('T17', 'HEAD is on another branch', st=other, real=H(other), mut=H(other, CurrentSha=SYNTHETIC), expect=closed('BRANCH_MISMATCH'))

        # Exactness of the mutation
        base_bytes = dumps(M())
        c('T18', 'bytes reformatted without any change of value', mut_raw=json.dumps(M(), indent=4, ensure_ascii=False).encode('utf-8') + b'\n', expect=hi_fail('MUTATION_BYTES_NOT_EXACT'))
        reordered = dict(reversed(list(M().items())))
        c('T19', 'top-level key order changed', mut=reordered, expect=hi_fail('MUTATION_KEY_ORDER_CHANGED'))
        c('T20', 'a second top-level field changed (WorkCompleted)', mut=M(WorkCompleted='Changed.'), expect=hi_fail('MUTATION_NOT_ONLY_CURRENT_SHA'))
        c('T21', 'Worktree also changed', mut=M(Worktree=os.path.join(root, 'elsewhere')), expect=hi_fail('HANDOFF_WORKTREE_MISMATCH'))
        c('T22', 'duplicate JSON key', mut_raw=base_bytes.replace(b'  "Gate"', b'  "Gate": "F2",\n  "Gate"', 1), expect=unread('MUTATED_UNREADABLE'))
        c('T23', 'byte order mark', mut_raw=b'\xef\xbb\xbf' + base_bytes, expect=unread('MUTATED_UNREADABLE'))
        c('T24', 'schema: CurrentSha is not 40 lowercase hex characters', mut=H(s, CurrentSha='DEADBEEF' * 5), expect=unread('MUTATED_SCHEMA_INVALID'))

        # Binding and relay
        c('T25', 'the real handoff is not the verified delivery', real=H(s, CurrentSha=s['red']), expect=hi_fail('REAL_CURRENT_SHA_MISMATCH'))
        c('T26', 'relay for another task', relay_over={'TaskId': 'OTHER'}, expect=hi_fail('RELAY_BINDING_MISMATCH'))
        c('T27', 'relay outcome not COMPLETED', relay_over={'Outcome': {'Kind': 'TIMEOUT'}}, expect=hi_fail('RELAY_NOT_COMPLETED'))
        c('T28', 'output written before the cession started', relay_over={'Outcome': {'OutputWrittenUtc': '2025-12-31T23:00:00Z'}}, expect=hi_fail('RELAY_OUTPUT_TIME_INVALID'))
        c('T29', 'output time without a UTC offset', relay_over={'Outcome': {'OutputWrittenUtc': '2026-01-01T00:30:00'}}, expect=hi_fail('RELAY_OUTPUT_TIME_INVALID'))
        c('T30', 'a nonexistent SHA that is not the bound synthetic', mut=H(s, CurrentSha='abc123ff' * 5), expect=control_fail('FAIL', 'MUTATED_SHA_NOT_AUTHORIZED'))
        c('T31', 'the bound synthetic names an existing object', mut=H(s, CurrentSha=s['side']), args_over={'--synthetic-sha': s['side']}, expect=control_fail('FAIL', 'MUTATED_SHA_EXISTS'))
    finally:
        shutil.rmtree(root, ignore_errors=True)
    ok = sum(1 for x in cases if x['Ok'])
    out = {'Schema': 'rackcad-i64-identity-bridge-selftest/v1', 'VerifierSha256': hashlib.sha256(open(VERIFIER, 'rb').read()).hexdigest(), 'Result': 'PASS' if ok == len(cases) else 'FAIL', 'Passed': ok, 'Total': len(cases), 'Cases': cases}
    data = (json.dumps(out, indent=2, ensure_ascii=True) + '\n').encode('ascii')
    if a.out:
        with open(a.out, 'wb') as f:
            f.write(data)
    sys.stdout.buffer.write(data)
    return 0 if out['Result'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main())
