#!/usr/bin/env python3
"""Synthetic self-test of I64-IDENTITY-BRIDGE-01 (I-64 A-13). T01-T15 are the minimum cases ordered by the Coordinator; T16-T31 cover
the exact repository occurrence, mutation exactness, relay binding and parser hardening. Uses ONLY synthetic fixtures built in a temporary
directory: a deterministic Git repository with a local bare origin, schema-valid handoffs and relay records. No real I-64 evidence.
T32-T53 cover the relay record against the official schema; T54-T69 the object query, simulating only that one Git call; P01-P04 the
schema-only pre-check tool.
Usage: python selftest_identity_bridge.py --verifier <path> --precheck <path> [--out <path>]. Exit 0 only when every case behaves as expected.
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


Z = '1' * 40
Y = 'A' * 64
PROCESSES = [{'Pid': 10, 'ParentPid': 1, 'Name': 'synthetic', 'CreationDateUtc': '2026-01-01T00:00:00Z', 'Classification': 'own-tree'}]


def relay(raw_hash, **over):
    """A complete rackcad-relay-record/v1 record: every required field, at every level."""
    r = {'Schema': 'rackcad-relay-record/v1', 'TaskId': TASK, 'RunId': WORK, 'Attempt': ATTEMPT, 'Phase': 'WORK',
         'Participant': {'Kind': 'subagent', 'Profile': None, 'ModelRequested': 'm', 'EffortRequested': 'high', 'ModelEffective': None, 'EffortEffective': None, 'EffectiveSource': None, 'EffectiveSourceSha256': None},
         'Exit': {'HeadSha': Z, 'LsRemoteSha': Z, 'OriginMainSha': Z, 'CleanTree': True, 'GitOperationInProgress': False, 'SummaryCommitSha': Z, 'ConfigTomlSha256': Y, 'ConfigTomlKeys': ['model'], 'Processes': copy.deepcopy(PROCESSES), 'OpenDelegations': 0},
         'Cession': {'StartUtc': '2026-01-01T00:00:00Z', 'EndUtc': '2026-01-01T01:00:00Z', 'SessionOperated': False},
         'Outcome': {'Kind': 'COMPLETED', 'LaunchedPid': None, 'ExitCode': 0, 'TerminalEvent': 'turn.completed', 'TreeKilled': False, 'DeathConfirmed': None, 'OutputPath': 'worker-handoff.json', 'OutputSha256': raw_hash.upper(),
                     'OutputWrittenUtc': '2026-01-01T00:30:00.500000+00:00', 'Acceptance': None},
         'Entry': {'HeadSha': Z, 'LsRemoteSha': Z, 'CleanTree': True, 'ConfigTomlSha256': Y, 'ConfigTomlKeys': ['model'], 'ConfigChanged': False, 'Processes': copy.deepcopy(PROCESSES)},
         'RemoteFacts': None, 'RebaseMap': None, 'Disposition': 'NONE', 'Notes': ['synthetic']}
    for k, v in over.items():
        if k in ('Cession', 'Outcome'):
            r[k].update(v)
        else:
            r[k] = v
    return r


def populate(r):
    """Every nullable section present, so that the deepest nested constraints are reachable."""
    r['Outcome']['Acceptance'] = {'A%d' % i: 'pass' for i in range(1, 9)}
    run = {'GhRunId': 1, 'Event': 'push', 'Ref': BRANCH, 'HeadSha': Z, 'Status': 'completed', 'Conclusion': 'success', 'Jobs': [{'Name': 'Tests', 'Conclusion': 'success'}]}
    r['RemoteFacts'] = {'CurrentRun': copy.deepcopy(run), 'RedRun': copy.deepcopy(run), 'TestArtifacts': [{'RunRef': 'green', 'Name': 'core', 'Sha256': Y, 'Selected': 3, 'Passed': 3, 'Failed': 0, 'FailedTests': []}],
                        'OriginMainSha': Z, 'LsRemoteSha': Z, 'Commands': ['gh run view'], 'ObtainedUtc': '2026-01-01T00:40:00Z', 'Diagnosis': None}
    r['RebaseMap'] = {'MainBeforeSha': Z, 'MainAfterSha': Z, 'G2CloseSha': Z, 'G2CloseImageSha': None, 'AuthorityRevision': Z, 'AuthorityRevisionImageSha': None, 'UnitDocBlobsEqual': None, 'UnitChangeSectionsEqual': None,
                      'BaseSha': None, 'BaseImageSha': None, 'ChainBaseSha': None, 'ChainBaseImageSha': None, 'Commits': [{'OriginalSha': Z, 'ImageSha': None, 'PatchIdEqual': None}], 'ConflictPaths': [], 'CoordinatorDecision': None}
    return r


def edit(fn):
    def apply(r):
        fn(r)
        return r
    return apply


def drop(path):
    def fn(r):
        for k in path[:-1]:
            r = r[k]
        del r[path[-1]]
    return edit(fn)


def put(path, value, populated=False):
    def fn(r):
        if populated:
            populate(r)
        for k in path[:-1]:
            r = r[k]
        r[path[-1]] = value
    return edit(fn)


# Simulates only the object query of the verifier under test; every other git call is real. The verifier has no test hook.
FAULT_RUNNER = r"""import base64, importlib.util, subprocess, sys
path, fault = sys.argv[1], sys.argv[2]
spec = importlib.util.spec_from_file_location('bridge_under_test', path)
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)
real_run = subprocess.run
def fake_run(cmd, *a, **k):
    if '--batch-check' not in cmd:
        return real_run(cmd, *a, **k)
    sha = k['input'].decode('ascii').strip().encode('ascii')
    if fault == 'exec-not-found':
        raise FileNotFoundError(2, 'git')
    if fault == 'exec-denied':
        raise PermissionError(13, 'git')
    if fault == 'timeout':
        raise subprocess.TimeoutExpired(cmd, 120)
    rc, out, err = {
        'clean-missing': (0, sha + b' missing\n', b''),
        'clean-exists': (0, sha + b' commit 123\n', b''),
        'rc-128': (128, b'', b'fatal: not a git repository\n'),
        'rc-129': (129, b'', b'usage: git cat-file\n'),
        'rc-128-plausible': (128, sha + b' missing\n', b''),
        'malformed': (0, b'garbage\n', b''),
        'empty': (0, b'', b''),
        'stderr-plausible': (0, sha + b' missing\n', b'warning: object store damaged\n'),
        'truncated': (0, sha + b' miss', b''),
        'no-newline': (0, sha + b' missing', b''),
        'extra': (0, sha + b' missing\nextra output\n', b''),
        'other-object': (0, b'1' * 40 + b' missing\n', b''),
        'ambiguous': (0, sha + b' ambiguous\n', b''),
    }[fault]
    return subprocess.CompletedProcess(cmd, rc, stdout=out, stderr=err)
if fault == 'tampered-schema':
    m.RELAY_RECORD_SCHEMA_B64 = base64.b64encode(b'{"type": "object"}').decode('ascii')
else:
    subprocess.run = fake_run
sys.exit(m.main(sys.argv[3:]))
"""


VERIFIER = None


def run_case(cid, desc, s, real=None, mut=None, relay_over=None, args_over=None, mut_raw=None, real_raw=None, relay_hash=None, expect=None, root=None, relay_fn=None, relay_raw=None, fault=None):
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
    if relay_fn is not None:
        rec = relay_fn(rec)
    relay_bytes = relay_raw(dumps(rec)) if relay_raw is not None else dumps(rec)
    write(paths['relay'], relay_bytes)
    args = {'--repo': s['repo'], '--task-id': TASK, '--attempt': str(ATTEMPT), '--delegation-run-id': DELEG, '--work-run-id': WORK, '--branch': BRANCH, '--base-sha': s['base'], '--red-sha': s['red'],
            '--current-sha': s['verified'], '--synthetic-sha': SYNTHETIC, '--real-handoff': paths['real'], '--mutated-handoff': paths['mut'], '--relay-record': paths['relay']}
    args.update(args_over or {})
    cmd = [sys.executable, '-I', VERIFIER] if fault is None else [sys.executable, '-I', os.path.join(root, 'fault_runner.py'), VERIFIER, fault]
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


PRECHECK = None


def run_precheck(cid, desc, data, expected_hash, want_result, want_exit, root):
    path = os.path.join(root, cid + '.json')
    write(path, data)
    p = subprocess.run([sys.executable, '-I', PRECHECK, '--relay-record', path, '--expected-sha256', expected_hash], capture_output=True)
    d = json.loads(p.stdout.decode('ascii'))
    got = {'Result': d['Result'], 'ExitCode': p.returncode}
    want = {'Result': want_result, 'ExitCode': want_exit}
    asserts = {k: got[k] == want[k] for k in got}
    return {'Id': cid, 'Description': desc, 'Expected': want, 'ExpectedCode': None, 'Got': got, 'ReasonCodes': [], 'Asserts': asserts, 'Ok': all(asserts.values())}


E = dict
PASSCTRL = E(HandoffIntegrity='PASS', Identity='FAIL', ControlResult='PASS', FailClosed=False, ExitCode=0)


def hi_fail(code):
    return E(HandoffIntegrity='FAIL', Identity='NOT_EVALUATED', ControlResult='FAIL', FailClosed=True, ExitCode=1, Code=code)


def closed(code):
    return E(HandoffIntegrity='PASS', Identity='NOT_EVALUATED', ControlResult='FAIL', FailClosed=True, ExitCode=1, Code=code)


def unread(code):
    return E(HandoffIntegrity='NOT_EVALUATED', Identity='NOT_EVALUATED', ControlResult='FAIL', FailClosed=True, ExitCode=1, Code=code)


def query_fail():
    return E(HandoffIntegrity='PASS', Identity='FAIL', ControlResult='FAIL', FailClosed=True, ExitCode=1, Code='GIT_OBJECT_QUERY_FAILED')


def control_fail(identity, code):
    return E(HandoffIntegrity='PASS', Identity=identity, ControlResult='FAIL', FailClosed=False, ExitCode=1, Code=code)


def fresh(root, name):
    d = os.path.join(root, name)
    os.makedirs(d)
    return build(d)


def main():
    global VERIFIER, PRECHECK
    p = argparse.ArgumentParser(add_help=False, allow_abbrev=False)
    p.add_argument('--verifier', required=True)
    p.add_argument('--precheck', required=True)
    p.add_argument('--out', default=None)
    a = p.parse_args()
    VERIFIER = os.path.abspath(a.verifier)
    PRECHECK = os.path.abspath(a.precheck)
    root = tempfile.mkdtemp(prefix='i64id-')
    cases = []
    try:
        s = fresh(root, 'main')
        cr = os.path.join(root, 'cases')
        os.makedirs(cr)
        write(os.path.join(cr, 'fault_runner.py'), FAULT_RUNNER.encode('ascii'))

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

        # Relay record against the official schema (A-13, root cause 1)
        bad = unread('RELAY_SCHEMA_INVALID')
        rd = unread('RELAY_UNREADABLE')
        c('T32', 'a relay record with every nullable section present is accepted', relay_fn=edit(populate), expect=PASSCTRL)
        c('T33', 'relay: required top-level field missing', relay_fn=drop(['Participant']), expect=bad)
        c('T34', 'relay: required nested field missing', relay_fn=drop(['Outcome', 'TreeKilled']), expect=bad)
        c('T35', 'relay: forbidden additional top-level property', relay_fn=put(['Extra'], 1), expect=bad)
        c('T36', 'relay: forbidden additional nested property', relay_fn=put(['Outcome', 'Extra'], 1), expect=bad)
        c('T37', 'relay: wrong scalar type', relay_fn=put(['Attempt'], str(ATTEMPT)), expect=bad)
        c('T38', 'relay: wrong nested type', relay_fn=put(['Cession', 'SessionOperated'], 'no'), expect=bad)
        c('T39', 'relay: wrong container type', relay_fn=put(['Notes'], 'x'), expect=bad)
        c('T40', 'relay: invalid enum', relay_fn=put(['Phase'], 'NOPE'), expect=bad)
        c('T41', 'relay: invalid nested enum', relay_fn=put(['Outcome', 'Kind'], 'NOPE'), expect=bad)
        c('T42', 'relay: invalid pattern', relay_fn=put(['RunId'], 'bad'), expect=bad)
        c('T43', 'relay: invalid nested pattern', relay_fn=put(['Exit', 'HeadSha'], 'xyz'), expect=bad)
        c('T44', 'relay: invalid enum inside an array item', relay_fn=put(['Exit', 'Processes', 0, 'Classification'], 'bad'), expect=bad)
        c('T45', 'relay: wrong item type in an array of strings', relay_fn=put(['Notes'], [1]), expect=bad)
        c('T46', 'relay: wrong type several levels deep', relay_fn=put(['RemoteFacts', 'CurrentRun', 'Jobs', 0, 'Name'], 1, populated=True), expect=bad)
        c('T47', 'relay: forbidden property inside a deep array item', relay_fn=put(['RebaseMap', 'Commits', 0, 'Extra'], 1, populated=True), expect=bad)
        c('T48', 'relay: null in a non-nullable section', relay_fn=put(['Cession'], None), expect=bad)
        c('T49', 'relay: malformed JSON', relay_raw=lambda b: b'{"Schema": "rackcad-relay-record/v1",', expect=rd)
        c('T50', 'relay: non-JSON constant', relay_raw=lambda b: b.replace(b'"ExitCode": 0', b'"ExitCode": NaN', 1), expect=rd)
        c('T51', 'relay: duplicate key', relay_raw=lambda b: b.replace(b'  "Phase"', b'  "Phase": "WORK",\n  "Phase"', 1), expect=rd)
        c('T52', 'relay: byte order mark', relay_raw=lambda b: b'\xef\xbb\xbf' + b, expect=rd)
        c('T53', 'the embedded official schema was altered: the blob pin rejects it', fault='tampered-schema', expect=bad)

        # Object existence: only one exact answer proves nonexistence (A-13, root cause 2). Everything else in the bridge stays real.
        q = query_fail()
        c('T54', 'simulated query: exact missing answer', fault='clean-missing', expect=PASSCTRL)
        c('T55', 'simulated query: the object exists', fault='clean-exists', expect=control_fail('FAIL', 'MUTATED_SHA_EXISTS'))
        c('T56', 'object query: git exit 128', fault='rc-128', expect=q)
        c('T57', 'object query: git exit 129', fault='rc-129', expect=q)
        c('T58', 'object query: git exit 128 with a plausible answer', fault='rc-128-plausible', expect=q)
        c('T59', 'object query: git executable not found', fault='exec-not-found', expect=q)
        c('T60', 'object query: git cannot be started', fault='exec-denied', expect=q)
        c('T61', 'object query: timeout', fault='timeout', expect=q)
        c('T62', 'object query: malformed stdout', fault='malformed', expect=q)
        c('T63', 'object query: empty stdout', fault='empty', expect=q)
        c('T64', 'object query: unexpected stderr with an otherwise plausible answer', fault='stderr-plausible', expect=q)
        c('T65', 'object query: truncated answer', fault='truncated', expect=q)
        c('T66', 'object query: answer without its final newline', fault='no-newline', expect=q)
        c('T67', 'object query: extra output after the answer', fault='extra', expect=q)
        c('T68', 'object query: the answer is about another object', fault='other-object', expect=q)
        c('T69', 'object query: ambiguous', fault='ambiguous', expect=q)

        # Schema-only pre-check tool (no Git, no bridge)
        good = dumps(relay('a' * 64))
        cases.append(run_precheck('P01', 'pre-check: complete relay record', good, hashlib.sha256(good).hexdigest(), 'SCHEMA_VALID', 0, cr))
        broken = dumps(drop(['Notes'])(relay('a' * 64)))
        cases.append(run_precheck('P02', 'pre-check: schema-invalid relay record', broken, hashlib.sha256(broken).hexdigest(), 'SCHEMA_INVALID', 1, cr))
        junk = b'{"Schema": '
        cases.append(run_precheck('P03', 'pre-check: malformed JSON', junk, hashlib.sha256(junk).hexdigest(), 'SCHEMA_INVALID', 1, cr))
        cases.append(run_precheck('P04', 'pre-check: the input is not the bound file, nothing is validated', good, '0' * 64, 'INPUT_SHA256_MISMATCH', 2, cr))
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
