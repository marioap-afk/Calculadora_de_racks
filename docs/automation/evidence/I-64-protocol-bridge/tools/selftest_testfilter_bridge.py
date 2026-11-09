#!/usr/bin/env python3
"""Synthetic self-test of I64-TESTFILTER-BRIDGE-01 (I-64 A-8). Uses ONLY synthetic fixtures built in a temporary directory:
a deterministic Git repository, VSTest TRX files, schema-valid handoffs and a synthetic 16.10 section. No real I-64 evidence.

Fixture universe (Workspace substring): FQN 6, display name 7, class only 5 (the 53af failure class: one fewer than FQN),
case-sensitive FQN 4, method only 1, so every rejected selection method disagrees with FQN somewhere.
Usage: python selftest_testfilter_bridge.py --verifier <path> [--out <path>]. Exit 0 only when every case behaves as expected.
Deterministic LF-only JSON (no paths, no timestamps)."""
import argparse
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

NS = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'
APPEND = ' GATE PASS.'
TERM_SET = ('# Plan sintetico\n\n### 16.10 Términos de gate\n\n'
            'Coincidencia por palabra o frase completa, sin distinguir mayúsculas, en `WorkCompleted`, `Evidence`, `UnexpectedFindings`, `KnownLimitations`, `Deviations`, `OpenQuestions` y\n'
            '`RecommendedNextAction`: `GATE PASS`, `GATE_PASS`, `Candidato`, `Candidate`, `FINAL_CANDIDATE_SHA`, `gate cerrado`, `gate closed`, `cierre del gate`, `rama integrada`,\n'
            '`integrada en main`, `integrated into main`, `merged into main`, `lista para integrar`, `ready to merge`, `Owner Validation APROBADA` y `Owner Validation APPROVED`. No se aplica al\n'
            'nombre de propiedad `Gate` ni a su valor. El Controller los comprueba en la entrega; el Coordinator, en el texto libre de la verificación, como control manual registrado.\n\n'
            '### 16.11 Siguiente\n\nTexto.\n')
ASSERT_MSG = 'Assert.Equal() Failure: Values differ'
NRE_MSG = 'System.NullReferenceException : Object reference not set to an instance of an object.'
# (className, method, display, red outcome, red message); every test passes in GREEN
UNIVERSE = [
    ('Synth.Tests.WorkspaceCacheTests', 'CachesPerSession', None, 'Failed', ASSERT_MSG),
    ('Synth.Tests.WorkspaceCacheTests', 'InvalidatesOnDrain', None, 'Passed', ''),
    ('Synth.Tests.WorkspaceCacheTests', 'EndsWithSession', None, 'Passed', ''),
    ('Synth.Tests.G8ContractTests', 'WORKSPACE_PROJECTS_THE_VALUE', None, 'Passed', ''),
    ('Synth.Tests.OtherTests', 'Theory_Row', 'Synth.Tests.OtherTests.Theory_Row(collection: "class X { Workspace W; }")', 'Passed', ''),
    ('Synth.Tests.WorkspaceLayoutTests', 'Builds', None, 'Failed', ASSERT_MSG),
    ('Synth.Tests.workspacelowerTests', 'Runs', None, 'Failed', NRE_MSG),
    ('Synth.Tests.MathTests', 'Adds', None, 'Passed', ''),
    ('Synth.Tests.MathTests', 'Subtracts', None, 'Passed', ''),
]
RT = ['tests/New/NavTestsA.cs', 'tests/New/NavTestsB.cs']
COMPOSITION = 'System.NullReferenceException=1,xunit-assertion=2'


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
    return p.stdout.decode('ascii').strip()


def write(path, text):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'wb') as f:
        f.write(text.encode('utf-8'))


def commit(repo, files, msg):
    for rel, text in files.items():
        write(os.path.join(repo, *rel.split('/')), text)
    g(repo, 'add', '-A')
    g(repo, 'commit', '-q', '-m', msg)
    return g(repo, 'rev-parse', 'HEAD')


def build_repo(root):
    repo = os.path.join(root, 'repo')
    os.makedirs(repo)
    g(repo, 'init', '-q')
    g(repo, 'config', 'core.autocrlf', 'false')
    top = g(repo, 'rev-parse', '--show-toplevel')
    if os.path.normcase(os.path.realpath(top)) != os.path.normcase(os.path.realpath(repo)):
        raise SystemExit('fixture repository is not its own top level')
    s = {}
    s['base'] = commit(repo, {'src/App.cs': 'class App {}\n', 'tests/Existing/OldTests.cs': 'class OldTests {}\n'}, 'base')
    s['red'] = commit(repo, {'tests/New/NavTestsA.cs': 'class A {}\n', 'tests/New/NavTestsB.cs': 'class B {}\n'}, 'red')
    s['green'] = commit(repo, {'src/App.cs': 'class App { int x; }\n'}, 'green')
    g(repo, 'checkout', '-q', '-b', 'notests', s['base'])
    s['red_notests'] = commit(repo, {'src/App.cs': 'class App { int y; }\n'}, 'red without tests')
    s['green_notests'] = commit(repo, {'src/App.cs': 'class App { int z; }\n'}, 'green after red without tests')
    g(repo, 'checkout', '-q', '-b', 'touch', s['red'])
    s['green_touch'] = commit(repo, {'src/App.cs': 'class App { int w; }\n', 'tests/New/NavTestsA.cs': 'class A { int changed; }\n'}, 'green touching red tests')
    return repo, s


def esc(text):
    return text.replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;').replace('"', '&quot;')


def trx(rows, counters=None, prolog='', bom=False, inner=False, extra_result=None, duplicate_id=False):
    """rows: list of (className, method, display, outcome, message)."""
    defs, res = [], []
    for i, (cls, m, disp, outcome, msg) in enumerate(rows):
        tid = '%08x-0000-4000-8000-000000000000' % (i + 1)
        if duplicate_id and i == 1:
            tid = '%08x-0000-4000-8000-000000000000' % 1
        name = disp or (cls + '.' + m)
        defs.append('<UnitTest name="%s" id="%s"><TestMethod codeBase="Synth.Tests.dll" className="%s" name="%s" /></UnitTest>' % (esc(name), tid, esc(cls), esc(m)))
        body = ''
        if msg:
            body = '<Output><ErrorInfo><Message>%s</Message></ErrorInfo></Output>' % esc(msg)
        if inner and i == 0:
            body += '<InnerResults></InnerResults>'
        res.append('<UnitTestResult executionId="e%d" testId="%s" testName="%s" outcome="%s">%s</UnitTestResult>' % (i + 1, tid, esc(name), outcome, body))
    if extra_result:
        res.append(extra_result)
    total = len(res) if counters is None else counters[0]
    passed = sum(1 for r in rows if r[3] == 'Passed') if counters is None else counters[1]
    failed = sum(1 for r in rows if r[3] == 'Failed') if counters is None else counters[2]
    text = (prolog + '<?xml version="1.0" encoding="utf-8"?>' if not prolog.startswith('<?xml') else prolog) if prolog else '<?xml version="1.0" encoding="utf-8"?>'
    text += '<TestRun id="run" name="synthetic" xmlns="%s"><TestDefinitions>%s</TestDefinitions><Results>%s</Results>' % (NS, ''.join(defs), ''.join(res))
    text += '<ResultSummary outcome="Completed"><Counters total="%d" executed="%d" passed="%d" failed="%d" /></ResultSummary></TestRun>' % (total, total, passed, failed)
    data = text.encode('utf-8')
    return (b'\xef\xbb\xbf' + data) if bom else data


def green_rows():
    return [(c, m, d, 'Passed', '') for c, m, d, _, _ in UNIVERSE]


def red_rows():
    return list(UNIVERSE)


def handoff(s, red_key='red', green_key='green', green=(6, 6, 0), red=(6, 3, 3), **over):
    h = {'Schema': 'rackcad-worker-handoff/v1', 'TaskId': 'SYNTH-TASK', 'RunId': 'R20260101T000000Z-aaaa', 'DelegationRunId': 'R20260101T000000Z-bbbb',
         'Initiative': 'I-64', 'Gate': 'F2', 'Attempt': 1, 'BaseSha': s['base'], 'RedSha': s[red_key], 'CurrentSha': s[green_key],
         'Branch': 'synthetic', 'Worktree': 'synthetic', 'Pushed': True, 'FilesChanged': ['src/App.cs'] + RT,
         'WorkCompleted': 'Synthetic work completed.', 'TestsExecuted': [],
         'TestResults': [{'RunRef': 'red-filter', 'Selected': red[0], 'Passed': red[1], 'Failed': red[2], 'Skipped': 0, 'FailedTests': []},
                         {'RunRef': 'green-filter', 'Selected': green[0], 'Passed': green[1], 'Failed': green[2], 'Skipped': 0, 'FailedTests': []}],
         'Evidence': [], 'UnexpectedFindings': [], 'KnownLimitations': [], 'Deviations': [], 'OpenQuestions': [],
         'WorkerStatus': 'IMPLEMENTATION_COMPLETE', 'Disposition': 'NONE', 'TriggeredStopConditions': [],
         'RecommendedNextAction': 'Synthetic next action.', 'Worker': {'Provider': 'synthetic', 'ModelRequested': 'synthetic', 'EffortRequested': 'high', 'Trailer': 'Co-Authored-By: Synthetic'}}
    h.update(over)
    return h


def dump(obj):
    return (json.dumps(obj, indent=2, ensure_ascii=False) + '\n').encode('utf-8')


def run_case(verifier, work, repo, s, case):
    d = os.path.join(work, case['Id'])
    os.makedirs(d)
    files = {'green.trx': case.get('green_trx', trx(green_rows())), 'red.trx': case.get('red_trx', trx(red_rows())),
             'real.json': case.get('real', dump(handoff(s))), 'term-set.md': TERM_SET.encode('utf-8'),
             'chain-red-files.json': case.get('expected_rt', dump(RT))}
    real_bytes = files['real.json']
    files['mutated.json'] = case.get('mutated', None) or None
    if files['mutated.json'] is None:
        r = json.loads(real_bytes.decode('utf-8'))
        r['WorkCompleted'] = r['WorkCompleted'] + APPEND
        files['mutated.json'] = dump(r)
    for name, data in files.items():
        with open(os.path.join(d, name), 'wb') as f:
            f.write(data)
    args = {'--repo': case.get('repo', repo), '--task-id': 'SYNTH-TASK', '--base-sha': s['base'], '--chain-base-sha': s['base'],
            '--red-sha': s[case.get('red_key', 'red')], '--current-sha': s[case.get('green_key', 'green')],
            '--filter': case.get('filter', 'FullyQualifiedName~Workspace'), '--min-selected': case.get('min', '4'),
            '--green-trx': os.path.join(d, 'green.trx'), '--red-trx': os.path.join(d, 'red.trx'),
            '--green-runref': 'green-filter', '--red-runref': 'red-filter',
            '--expect-green': case.get('expect_green', '6/6/0'), '--expect-red': case.get('expect_red', '6/3/3'),
            '--expect-red-composition': case.get('composition', COMPOSITION),
            '--expected-chain-red-files': os.path.join(d, 'chain-red-files.json'),
            '--real-handoff': os.path.join(d, 'real.json'), '--mutated-handoff': os.path.join(d, 'mutated.json'), '--term-set': os.path.join(d, 'term-set.md')}
    for k, v in case.get('override_args', {}).items():
        args[k] = v
    argv = case['argv'] if 'argv' in case else [x for kv in args.items() for x in kv]
    p = subprocess.run([sys.executable, '-I', verifier] + argv, capture_output=True)
    try:
        doc = json.loads(p.stdout.decode('ascii'))
    except Exception:
        doc = None
    got = doc['Result'] if doc else 'UNREADABLE_OUTPUT'
    codes = sorted({r['Code'] for r in doc['Reasons']}) if doc else []
    ok = got == case['expect'] and p.returncode == case.get('exit', 0 if case['expect'] == 'PASS' else 1)
    if case.get('code'):
        ok = ok and case['code'] in codes
    asserts = {}
    for name, fn in case.get('asserts', {}).items():
        try:
            asserts[name] = bool(fn(doc))
        except Exception:
            asserts[name] = False
    ok = ok and all(asserts.values())
    return {'Id': case['Id'], 'Description': case['desc'], 'Expected': case['expect'], 'ExpectedCode': case.get('code'),
            'Got': got, 'ExitCode': p.returncode, 'ReasonCodes': codes, 'Asserts': asserts, 'Ok': ok}


def cases(s):
    base_h = handoff(s)

    def h(**kw):
        return dump(handoff(s, **kw))

    def mut(**over):
        r = handoff(s)
        r['WorkCompleted'] = r['WorkCompleted'] + APPEND
        r.update(over)
        return dump(r)
    g_rows = green_rows()
    red_ok = red_rows()
    red_allpass = [(c, m, d, 'Passed', '') for c, m, d, _, _ in UNIVERSE]
    red_outside = red_rows()[:-1] + [('Synth.Tests.MathTests', 'Subtracts', None, 'Failed', ASSERT_MSG)]
    red_swap = red_rows()[:2] + [('Synth.Tests.WorkspaceNewTests', 'Replaced', None, 'Passed', '')] + red_rows()[3:]
    green_failed = [(c, m, d, ('Failed' if m == 'InvalidatesOnDrain' else 'Passed'), (ASSERT_MSG if m == 'InvalidatesOnDrain' else '')) for c, m, d, _, _ in UNIVERSE]
    good = lambda d: d['Green']['Selected'] == 6 and d['Red']['Selected'] == 6
    return [
        {'Id': 'T01', 'desc': 'correct FQN selection, coherent counts, exact mutation', 'expect': 'PASS',
         'asserts': {'Fqn6': good, 'Composition': lambda d: d['Red']['FailureComposition'] == {'System.NullReferenceException': 1, 'xunit-assertion': 2}}},
        {'Id': 'T02', 'desc': 'declared counts computed by display name (7) -> FAIL', 'expect': 'FAIL', 'code': 'EXPECTED_COUNT_MISMATCH',
         'real': h(green=(7, 7, 0), red=(7, 4, 3)), 'expect_green': '7/7/0', 'expect_red': '7/4/3'},
        {'Id': 'T03', 'desc': 'declared counts computed by class name only (5, one fewer than FQN: the 53af class) -> FAIL', 'expect': 'FAIL', 'code': 'EXPECTED_COUNT_MISMATCH',
         'real': h(green=(5, 5, 0), red=(5, 2, 3)), 'expect_green': '5/5/0', 'expect_red': '5/2/3',
         'asserts': {'ClassOnlyIsOneFewer': lambda d: d['Diagnostics']['Green']['ClassNameOnly'] == d['Green']['Selected'] - 1}},
        {'Id': 'T04', 'desc': 'declared counts computed case-sensitively (4) -> FAIL', 'expect': 'FAIL', 'code': 'EXPECTED_COUNT_MISMATCH',
         'real': h(green=(4, 4, 0), red=(4, 2, 2)), 'expect_green': '4/4/0', 'expect_red': '4/2/2', 'composition': 'xunit-assertion=2'},
        {'Id': 'T05', 'desc': 'declared counts computed by method name only (1) -> FAIL', 'expect': 'FAIL', 'code': 'EXPECTED_COUNT_MISMATCH',
         'real': h(green=(1, 1, 0), red=(1, 1, 0)), 'expect_green': '1/1/0', 'expect_red': '1/1/0'},
        {'Id': 'T06', 'desc': 'Theory argument text contains the substring but the FQN does not: not selected', 'expect': 'PASS',
         'asserts': {'NotSelected': lambda d: d['Green']['Selected'] == 6 and d['Diagnostics']['Green']['DisplayNameCaseInsensitive'] == 7}},
        {'Id': 'T07', 'desc': 'substring only in the method name (upper case): selected', 'expect': 'PASS',
         'asserts': {'Selected': lambda d: d['Green']['Selected'] == 6 and d['Diagnostics']['Green']['ClassNameOnly'] == 5 and d['Diagnostics']['Green']['CaseSensitiveFqn'] == 4}},
        {'Id': 'T08', 'desc': 'selection below MinSelected -> FAIL', 'expect': 'FAIL', 'code': 'SELECTION_BELOW_MINIMUM', 'min': '7'},
        {'Id': 'T09', 'desc': 'GREEN has a failed selected test -> FAIL', 'expect': 'FAIL', 'code': 'GREEN_NOT_ALL_PASSED',
         'green_trx': trx(green_failed), 'real': h(green=(6, 5, 1)), 'expect_green': '6/5/1'},
        {'Id': 'T10', 'desc': 'RED has no failed selected test -> FAIL', 'expect': 'FAIL', 'code': 'RED_WITHOUT_SELECTED_FAILURES',
         'red_trx': trx(red_allpass), 'real': h(red=(6, 6, 0)), 'expect_red': '6/6/0'},
        {'Id': 'T11', 'desc': 'ChainRedFiles omitted (the RED commit changes no tests/ path) -> FAIL', 'expect': 'FAIL', 'code': 'CHAIN_RED_FILES_EMPTY',
         'red_key': 'red_notests', 'green_key': 'green_notests', 'real': h(red_key='red_notests', green_key='green_notests')},
        {'Id': 'T12', 'desc': 'GREEN modifies a ChainRedFile -> FAIL', 'expect': 'FAIL', 'code': 'GREEN_MODIFIES_CHAIN_RED_FILES',
         'green_key': 'green_touch', 'real': h(green_key='green_touch')},
        {'Id': 'T13', 'desc': 'nc3 handoff without the prohibited-term mutation -> FAIL', 'expect': 'FAIL', 'code': 'MUTATION_TERM_NOT_EXACTLY_ONE',
         'mutated': dump(base_h)},
        {'Id': 'T14', 'desc': 'nc3 handoff changes an additional field -> FAIL', 'expect': 'FAIL', 'code': 'MUTATION_NOT_ONLY_WORKCOMPLETED',
         'mutated': mut(Deviations=['extra change'])},
        {'Id': 'T15', 'desc': 'malformed TRX -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_UNREADABLE', 'green_trx': trx(g_rows)[:-40]},
        {'Id': 'T16', 'desc': 'TRX result references an undefined testId -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_UNREADABLE',
         'green_trx': trx(g_rows, extra_result='<UnitTestResult executionId="x" testId="ffffffff-0000-4000-8000-000000000000" testName="x" outcome="Passed" />')},
        {'Id': 'T17', 'desc': 'duplicate UnitTest id -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_UNREADABLE', 'green_trx': trx(g_rows, duplicate_id=True)},
        {'Id': 'T18', 'desc': 'DOCTYPE / ENTITY in TRX -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_UNREADABLE',
         'green_trx': trx(g_rows).replace(b'<TestRun', b'<!DOCTYPE TestRun [<!ENTITY x "y">]><TestRun', 1)},
        {'Id': 'T19', 'desc': 'unsupported filter expression -> FAIL closed', 'expect': 'FAIL', 'code': 'UNSUPPORTED_FILTER',
         'filter': 'FullyQualifiedName~Workspace|Category=Slow'},
        {'Id': 'T20', 'desc': 'Counters incoherent with the Results rows -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_UNREADABLE',
         'green_trx': trx(g_rows, counters=(len(g_rows), len(g_rows) - 1, 0))},
        {'Id': 'T21', 'desc': 'InnerResults present -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_UNREADABLE', 'green_trx': trx(g_rows, inner=True)},
        {'Id': 'T22', 'desc': 'RED failure composition differs from the expected one -> FAIL', 'expect': 'FAIL', 'code': 'RED_COMPOSITION_MISMATCH',
         'composition': 'xunit-assertion=3'},
        {'Id': 'T23', 'desc': 'RED failure outside the selection -> FAIL', 'expect': 'FAIL', 'code': 'RED_FAILURE_OUTSIDE_SELECTION', 'red_trx': trx(red_outside)},
        {'Id': 'T24', 'desc': '--repo is not the top level of the repository -> FAIL closed', 'expect': 'FAIL', 'code': 'GIT_REPOSITORY_INVALID', 'repo_sub': 'src'},
        {'Id': 'T25', 'desc': 'mutation introduces the term in WorkCompleted and in another governed field -> FAIL', 'expect': 'FAIL', 'code': 'MUTATION_NOT_ONLY_WORKCOMPLETED',
         'mutated': mut(RecommendedNextAction='Synthetic next action. GATE PASS.')},
        {'Id': 'T26', 'desc': 'real handoff not schema-valid (Attempt as a string) -> FAIL', 'expect': 'FAIL', 'code': 'REAL_SCHEMA_INVALID', 'real': h(Attempt='1')},
        {'Id': 'T27', 'desc': 'appended text is not the authorized append (other term) -> FAIL', 'expect': 'FAIL', 'code': 'MUTATION_NOT_AUTHORIZED_APPEND',
         'mutated': mut(WorkCompleted='Synthetic work completed. Candidato.')},
        {'Id': 'T28', 'desc': 'selected test identities differ between GREEN and RED with equal counts -> FAIL', 'expect': 'FAIL', 'code': 'SELECTION_DIFFERS_GREEN_RED',
         'red_trx': trx(red_swap)},
        {'Id': 'T29', 'desc': 'handoff not bound to the occurrence (CurrentSha) -> FAIL', 'expect': 'FAIL', 'code': 'BINDING_MISMATCH', 'real': h(green_key='green_touch')},
        {'Id': 'T30', 'desc': '-h is a usage error (exit 2), never PASS', 'expect': 'FAIL', 'code': 'USAGE_ERROR', 'exit': 2, 'argv': ['-h']},
        {'Id': 'T31', 'desc': 'unsupported TRX outcome -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_UNREADABLE',
         'green_trx': trx(g_rows).replace(b'outcome="Passed"', b'outcome="Timeout"', 1)},
        {'Id': 'T32', 'desc': 'accredited ChainRedFiles list omits a derived file -> FAIL', 'expect': 'FAIL', 'code': 'CHAIN_RED_FILES_MISMATCH', 'expected_rt': dump(RT[:1])},
        {'Id': 'T33', 'desc': 'real handoff already contains a prohibited term -> FAIL', 'expect': 'FAIL', 'code': 'REAL_HAS_PROHIBITED_TERM',
         'real': h(Evidence=['Candidate ready']), 'mutated': dump(dict(handoff(s, Evidence=['Candidate ready']), WorkCompleted='Synthetic work completed.' + APPEND))},
        {'Id': 'T34', 'desc': 'TRX with a UTF-8 byte order mark is read', 'expect': 'PASS', 'green_trx': trx(g_rows, bom=True), 'red_trx': trx(red_ok, bom=True)},
        {'Id': 'T35', 'desc': 'TRX that is not strict UTF-8 -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_UNREADABLE',
         'green_trx': trx(g_rows).replace(b'synthetic', b'synth\xffetic', 1)},
    ]


def main():
    p = argparse.ArgumentParser(add_help=False, allow_abbrev=False)
    p.add_argument('--verifier', required=True)
    p.add_argument('--out', default=None)
    a = p.parse_args()
    work = tempfile.mkdtemp(prefix='i64tf-selftest-')
    try:
        repo, s = build_repo(work)
        results = []
        for case in cases(s):
            if case.pop('repo_sub', None):
                case['repo'] = os.path.join(repo, 'src')
            results.append(run_case(a.verifier, work, repo, s, case))
    finally:
        shutil.rmtree(work, ignore_errors=True)
    with open(a.verifier, 'rb') as f:
        vsha = hashlib.sha256(f.read()).hexdigest()
    passed = sum(1 for r in results if r['Ok'])
    doc = {'Schema': 'rackcad-i64-testfilter-bridge-selftest/v1', 'VerifierSha256': vsha, 'Result': 'PASS' if passed == len(results) else 'FAIL',
           'Passed': passed, 'Total': len(results), 'FixtureOnly': 'synthetic Git repository, TRX, handoffs and 16.10 section; no real I-64 evidence', 'Cases': results}
    data = (json.dumps(doc, indent=2, ensure_ascii=True) + '\n').encode('ascii')
    if a.out:
        with open(a.out, 'wb') as f:
            f.write(data)
    sys.stdout.buffer.write(data)
    return 0 if doc['Result'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main())
