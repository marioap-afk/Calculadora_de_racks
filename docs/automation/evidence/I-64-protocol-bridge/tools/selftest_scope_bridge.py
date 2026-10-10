#!/usr/bin/env python3
"""Self-test of verify_scope_bridge.py (I64-SCOPE-BRIDGE-01) on a synthetic, throw-away Git repository.

It never reads the I-64 task evidence: the T1 executions of the bridge (REAL and ADVERSARIAL) happen only after the
Coordinator agrees A-1. Commits use fixed identities and dates, so object ids and the JSON summary are deterministic.
Python standard library only. Exit 0 only when every case matches its expectation and every run is reproducible.
"""
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
VERIFIER = os.path.join(HERE, 'verify_scope_bridge.py')
if '--verifier' in sys.argv:  # regression demonstration against another verifier blob
    VERIFIER = os.path.abspath(sys.argv[sys.argv.index('--verifier') + 1])
RUN_DELEGATION = 'R20000101T000000Z-0001'
RUN_WORK = 'R20000101T000100Z-0002'
FAKE_SHA = '1111111111111111111111111111111111111111'


def clean_env(extra=None):
    env = {k: v for k, v in os.environ.items() if not k.upper().startswith('GIT_')}
    env.update({'GIT_AUTHOR_NAME': 'selftest', 'GIT_AUTHOR_EMAIL': 'selftest@example.invalid',
                'GIT_COMMITTER_NAME': 'selftest', 'GIT_COMMITTER_EMAIL': 'selftest@example.invalid',
                'GIT_AUTHOR_DATE': '2000-01-01T00:00:00+0000', 'GIT_COMMITTER_DATE': '2000-01-01T00:00:00+0000',
                'GIT_TERMINAL_PROMPT': '0'})
    if extra:
        env.update(extra)
    return env


def git(repo, *args):
    base = ['git', '-c', 'core.autocrlf=false', '-c', 'commit.gpgsign=false', '-c', 'core.hooksPath=' + os.path.join(repo, '.nohooks')]
    out = subprocess.run(base + ['-C', repo] + list(args), capture_output=True, env=clean_env(), check=True)
    return out.stdout.decode('ascii').strip()


def write(repo, rel, text):
    path = os.path.join(repo, *rel.split('/'))
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'wb') as fh:
        fh.write(text.encode('ascii'))


def commit(repo, msg):
    git(repo, 'add', '-A')
    git(repo, 'commit', '-q', '--no-verify', '-m', msg)
    return git(repo, 'rev-parse', 'HEAD')


def build_repo(root):
    repo = os.path.join(root, 'repo')
    os.makedirs(repo)
    git(repo, 'init', '-q')
    git(repo, 'checkout', '-q', '-b', 'selftest')
    write(repo, 'allowed/a.txt', 'a1\n'); write(repo, 'allowed/keep.txt', 'k\n')
    write(repo, 'forbidden/x.txt', 'x1\n'); write(repo, 'other/o.txt', 'o1\n')
    shas = {'A': commit(repo, 'A base')}
    write(repo, 'allowed/a.txt', 'a2\n'); shas['B'] = commit(repo, 'B allowed change')
    git(repo, 'checkout', '-q', shas['A'])
    write(repo, 'other/o.txt', 'o2\n'); shas['C'] = commit(repo, 'C outside allowed')
    git(repo, 'checkout', '-q', shas['A'])
    write(repo, 'forbidden/x.txt', 'x2\n'); shas['D'] = commit(repo, 'D forbidden change')
    git(repo, 'checkout', '-q', shas['A'])
    git(repo, 'mv', 'forbidden/x.txt', 'allowed/x.txt'); shas['E'] = commit(repo, 'E rename forbidden into allowed')
    git(repo, 'checkout', '-q', 'selftest')
    return repo, shas


def delegation(base, **over):
    d = {'Schema': 'rackcad-delegation/v1', 'TaskId': 'SELFTEST', 'RunId': RUN_DELEGATION, 'BaseSha': base,
         'AllowedWriteScope': ['allowed/'], 'ForbiddenWriteScope': ['forbidden/']}
    d.update(over)
    return d


def handoff(base, current, **over):
    h = {'Schema': 'rackcad-worker-handoff/v1', 'TaskId': 'SELFTEST', 'RunId': RUN_WORK, 'DelegationRunId': RUN_DELEGATION,
         'BaseSha': base, 'CurrentSha': current}
    h.update(over)
    return h


def cases(s):
    A, B, C, D, E = s['A'], s['B'], s['C'], s['D'], s['E']
    drop = object()
    return [
        ('T01', 'real-like delivery inside AllowedWriteScope', delegation(A), handoff(A, B), A, B, 'PASS', 0, None),
        ('T02', 'adversarial-like: prefix replaced by the other files under it, so the changed file is excluded',
         delegation(A, AllowedWriteScope=['allowed/keep.txt']), handoff(A, B), A, B, 'FAIL', 1, 'OUTSIDE_ALLOWED_WRITE_SCOPE'),
        ('T03', 'nonexistent BaseSha', delegation(FAKE_SHA), handoff(FAKE_SHA, B), FAKE_SHA, B, 'FAIL', 1, 'GIT_BASESHA_NOT_A_COMMIT'),
        ('T04', 'nonexistent CurrentSha', delegation(A), handoff(A, FAKE_SHA), A, FAKE_SHA, 'FAIL', 1, 'GIT_CURRENTSHA_NOT_A_COMMIT'),
        ('T05', 'malformed delegation JSON', '{ "Schema": "rackcad-delegation/v1", ', handoff(A, B), A, B, 'FAIL', 1, 'DELEGATION_UNREADABLE'),
        ('T06', 'missing AllowedWriteScope', delegation(A, AllowedWriteScope=drop), handoff(A, B), A, B, 'FAIL', 1, 'AllowedWriteScope_MISSING_OR_NOT_LIST'),
        ('T07', 'BaseSha not an ancestor of CurrentSha', delegation(B), handoff(B, C), B, C, 'FAIL', 1, 'BASE_NOT_ANCESTOR'),
        ('T08', 'changed path covered by AllowedWriteScope but inside ForbiddenWriteScope',
         delegation(A, AllowedWriteScope=['allowed/', 'forbidden/']), handoff(A, D), A, D, 'FAIL', 1, 'FORBIDDEN_WRITE_SCOPE_INTERSECTION'),
        ('T09', 'rename from a forbidden path into the allowed scope (rename source is a changed path)',
         delegation(A), handoff(A, E), A, E, 'FAIL', 1, 'OUTSIDE_ALLOWED_WRITE_SCOPE'),
        ('T10', 'handoff.DelegationRunId differs', delegation(A), handoff(A, B, DelegationRunId='R20000101T000000Z-9999'), A, B, 'FAIL', 1, 'DELEGATION_RUNID_MISMATCH'),
        ('T11', 'handoff.CurrentSha differs from --expected-current', delegation(A), handoff(A, C), A, B, 'FAIL', 1, 'HANDOFF_CURRENT_MISMATCH'),
        ('T12', 'invalid scope syntax (glob, parent segment, absolute path)',
         delegation(A, AllowedWriteScope=['allowed/*', '../allowed/', '/allowed/']), handoff(A, B), A, B, 'FAIL', 1, 'AllowedWriteScope_INVALID_ENTRY'),
        ('T13', 'malformed --expected-base', delegation(A), handoff(A, B), 'ABC', B, 'FAIL', 1, 'EXPECTEDBASE_INVALID_SHA'),
        ('T14', 'empty diff (BaseSha = CurrentSha)', delegation(A), handoff(A, A), A, A, 'FAIL', 1, 'EMPTY_DIFF'),
        ('T15', 'missing handoff file', delegation(A), None, A, B, 'FAIL', 1, 'HANDOFF_UNREADABLE'),
        ('T16', 'delegation.BaseSha differs from --expected-base', delegation(C), handoff(A, B), A, B, 'FAIL', 1, 'DELEGATION_BASE_MISMATCH'),
        ('T17', 'unexpected delegation Schema', delegation(A, Schema='rackcad-delegation/v2'), handoff(A, B), A, B, 'FAIL', 1, 'DELEGATION_SCHEMA_UNEXPECTED'),
        ('T18', 'repository path is not a Git repository', delegation(A), handoff(A, B), A, B, 'FAIL', 1, 'GIT_REPOSITORY_INVALID'),
        ('T19', 'duplicate key in delegation JSON',
         '{"Schema": "rackcad-delegation/v1", "AllowedWriteScope": ["allowed/"], "AllowedWriteScope": ["x/"]}', handoff(A, B), A, B, 'FAIL', 1, 'DELEGATION_UNREADABLE'),
        ('T20', 'missing ForbiddenWriteScope', delegation(A, ForbiddenWriteScope=drop), handoff(A, B), A, B, 'FAIL', 1, 'ForbiddenWriteScope_MISSING_OR_NOT_LIST'),
        ('T21', 'usage error: --handoff omitted', delegation(A), handoff(A, B), A, B, 'FAIL', 2, 'USAGE_ERROR'),
        ('T22', 'usage: -h does not end with exit 0', delegation(A), handoff(A, B), A, B, 'FAIL', 2, 'USAGE_ERROR'),
        ('T23', 'inherited GIT_DIR pointing at another repository is ignored', delegation(A), handoff(A, B), A, B, 'PASS', 0, None),
        ('T24', 'trailing LF in delegation.RunId and handoff.DelegationRunId (I64-A2-RUNID-FAIL-OPEN)',
         delegation(A, RunId=RUN_DELEGATION + chr(10)), handoff(A, B, DelegationRunId=RUN_DELEGATION + chr(10)), A, B, 'FAIL', 1, 'DELEGATION_RUNID_INVALID'),
        ('T25', 'trailing LF in --expected-base', delegation(A), handoff(A, B), A + chr(10), B, 'FAIL', 1, 'EXPECTEDBASE_INVALID_SHA'),
    ], drop


def materialize(path, value, drop):
    if value is None:
        return
    if isinstance(value, str):
        data = value.encode('ascii')
    else:
        data = json.dumps({k: v for k, v in value.items() if v is not drop}, ensure_ascii=True, indent=2).encode('ascii')
    with open(path, 'wb') as fh:
        fh.write(data)


def run_case(case, repo, other_git_dir, root, drop):
    cid, desc, deleg, hand, base, cur, exp_result, exp_exit, exp_code = case
    cdir = os.path.join(root, cid)
    os.makedirs(cdir)
    dpath, hpath = os.path.join(cdir, 'delegation.json'), os.path.join(cdir, 'handoff.json')
    materialize(dpath, deleg, drop)
    materialize(hpath, hand, drop)
    target = os.path.join(root, 'not-a-repo') if cid == 'T18' else repo
    os.makedirs(os.path.join(root, 'not-a-repo'), exist_ok=True)
    argv = ['--repo', target, '--delegation', dpath, '--handoff', hpath, '--expected-base', base, '--expected-current', cur]
    if cid == 'T21':
        argv = argv[:4] + argv[6:]
    if cid == 'T22':
        argv = ['-h']
    env = dict(os.environ)
    if cid == 'T23':
        env['GIT_DIR'] = other_git_dir
    outs = []
    for _ in range(2):
        p = subprocess.run([sys.executable, VERIFIER] + argv, capture_output=True, env=env)
        outs.append((p.returncode, p.stdout))
    (code1, out1), (code2, out2) = outs
    doc = json.loads(out1.decode('ascii'))
    codes = [r['Code'] for r in doc['Reasons']]
    ok = (doc['Result'] == exp_result and code1 == exp_exit and (exp_code is None or exp_code in codes)
          and (exp_result != 'PASS' or not codes) and out1 == out2 and code1 == code2)
    return {'Id': cid, 'Description': desc, 'ExpectedResult': exp_result, 'ExpectedExit': exp_exit, 'ExpectedCode': exp_code,
            'ActualResult': doc['Result'], 'ActualExit': code1, 'ActualCodes': codes, 'OutsideAllowed': doc.get('OutsideAllowed'),
            'ForbiddenHits': doc.get('ForbiddenHits'), 'ChangedPaths': doc.get('ChangedPaths'),
            'Reproducible': out1 == out2 and code1 == code2, 'Ok': bool(ok)}


def main():
    root = tempfile.mkdtemp(prefix='i64sb-')
    try:
        repo, shas = build_repo(root)
        other = os.path.join(root, 'other')
        os.makedirs(other)
        git(other, 'init', '-q')
        all_cases, drop = cases(shas)
        results = [run_case(c, repo, os.path.join(other, '.git'), root, drop) for c in all_cases]
        extra_ok = True
        for r in results:
            if r['Id'] == 'T02':
                extra_ok &= r['OutsideAllowed'] == ['allowed/a.txt']
            if r['Id'] == 'T09':
                extra_ok &= r['ChangedPaths'] == ['allowed/x.txt', 'forbidden/x.txt'] and r['OutsideAllowed'] == ['forbidden/x.txt']
        with open(VERIFIER, 'rb') as fh:
            verifier_sha = hashlib.sha256(fh.read()).hexdigest()
        summary = {'Schema': 'rackcad-i64-scope-bridge-selftest/v1', 'VerifierSha256': verifier_sha, 'Commits': shas,
                   'Result': 'PASS' if extra_ok and all(r['Ok'] for r in results) else 'FAIL',
                   'Cases': results}
    finally:
        shutil.rmtree(root, ignore_errors=True)
    sys.stdout.buffer.write((json.dumps(summary, ensure_ascii=True, indent=2) + '\n').encode('ascii'))
    sys.stdout.buffer.flush()
    return 0 if summary['Result'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main())
