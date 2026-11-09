#!/usr/bin/env python3
"""Synthetic self-test of I64-TESTFILTER-BRIDGE-01 (I-64 A-10; T01-T35 from A-8; T36-T62 from A-9 for the three REQUIRED
findings of the Architect review R20261109T062937Z-e1d0: ordinal T36-T38 and T56-T62, counters T39-T48, execution identity
T49-T55; T63-T92 for the two REQUIRED findings of R20261109T112612Z-04e8: test-id GUID identity T63-T73, failure-message
cardinality T74-T89, consumed TRX elements only in their place and namespace T90-T92).
Uses ONLY synthetic fixtures built in a temporary directory:
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
import xml.etree.ElementTree as ET

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


COUNTER_ORDER = ('total', 'executed', 'passed', 'failed', 'error', 'timeout', 'aborted', 'inconclusive', 'passedButRunAborted', 'notRunnable',
                 'notExecuted', 'disconnected', 'warning', 'completed', 'inProgress', 'pending')


def eid(i):
    return '%08x-1111-4111-8111-%012x' % (i + 1, i + 1)


def trx(rows, counters=None, bom=False, inner=False, extra_result=None, duplicate_id=False, exec_ids=None, shared=None,
        test_ids=None, result_ids=None, bodies=None):
    """rows: list of (className, method, display, outcome, message). shared: {row index: earlier row index} whose testId the row
    reuses (another execution of the same test, e.g. a Theory case). exec_ids: {row index: executionId or None (omitted)}.
    counters: {attribute: value or None (attribute omitted)} overriding the values derived from the rows.
    test_ids: {row index: UnitTest@id or None (omitted)} for a row with its own definition. result_ids: {row index:
    UnitTestResult@testId or None (omitted)} overriding the reference to the row's definition. bodies: {row index: inner XML
    of the UnitTestResult} replacing the Output/ErrorInfo/Message built from the message. Defaults give the A-9 fixtures."""
    defs, res, tids = [], [], []
    for i, (cls, m, disp, outcome, msg) in enumerate(rows):
        name = disp or (cls + '.' + m)
        if i in (shared or {}):
            tid = tids[shared[i]]
        else:
            tid = '%08x-0000-4000-8000-000000000000' % (i + 1)
            if duplicate_id and i == 1:
                tid = '%08x-0000-4000-8000-000000000000' % 1
            tid = (test_ids or {}).get(i, tid)
            ida = '' if tid is None else ' id="%s"' % esc(tid)
            defs.append('<UnitTest name="%s"%s><TestMethod codeBase="Synth.Tests.dll" className="%s" name="%s" /></UnitTest>' % (esc(name), ida, esc(cls), esc(m)))
        tids.append(tid)
        body = ''
        if msg:
            body = '<Output><ErrorInfo><Message>%s</Message></ErrorInfo></Output>' % esc(msg)
        if i in (bodies or {}):
            body = bodies[i]
        if inner and i == 0:
            body += '<InnerResults></InnerResults>'
        e = (exec_ids or {}).get(i, eid(i))
        ea = '' if e is None else ' executionId="%s"' % e
        rid = (result_ids or {}).get(i, tid)
        ta = '' if rid is None else ' testId="%s"' % esc(rid)
        res.append('<UnitTestResult%s%s testName="%s" outcome="%s">%s</UnitTestResult>' % (ea, ta, esc(name), outcome, body))
    if extra_result:
        res.append(extra_result)
    passed = sum(1 for r in rows if r[3] == 'Passed')
    failed = sum(1 for r in rows if r[3] == 'Failed')
    values = {k: 0 for k in COUNTER_ORDER}
    values.update({'total': len(res), 'executed': passed + failed, 'passed': passed, 'failed': failed,
                   'notExecuted': sum(1 for r in rows if r[3] == 'NotExecuted')})
    for k, v in (counters or {}).items():
        values[k] = v
    attrs = ' '.join('%s="%s"' % (k, v) for k, v in values.items() if v is not None)
    text = '<?xml version="1.0" encoding="utf-8"?>'
    text += '<TestRun id="run" name="synthetic" xmlns="%s"><TestDefinitions>%s</TestDefinitions><Results>%s</Results>' % (NS, ''.join(defs), ''.join(res))
    text += '<ResultSummary outcome="Completed"><Counters %s /></ResultSummary></TestRun>' % attrs
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
    CACHE = 'Synth.Tests.WorkspaceCacheTests'
    LONG_S = 'Synth.Tests.Work' + chr(0x17F) + 'paceCacheTests'
    DOTLESS = 'Synth.Tests.F' + chr(0x131) + 'lterCacheTests'
    rename = lambda rows, new: [((new if c == CACHE else c), m, d, o, msg) for c, m, d, o, msg in rows]
    g_long, r_long = rename(g_rows, LONG_S), rename(red_ok, LONG_S)
    g_dot = rename(g_rows, DOTLESS)
    r_dot = [((DOTLESS if c == CACHE else c), m, d, ('Failed' if (c == CACHE and m == 'CachesPerSession') else 'Passed'),
              (ASSERT_MSG if (c == CACHE and m == 'CachesPerSession') else '')) for c, m, d, _, _ in UNIVERSE]
    g_ne = [(c, m, d, ('NotExecuted' if m == 'Subtracts' else o), msg) for c, m, d, o, msg in g_rows]
    r_ne = [(c, m, d, ('NotExecuted' if m == 'Subtracts' else o), ('' if m == 'Subtracts' else msg)) for c, m, d, o, msg in red_ok]
    n = len(UNIVERSE)
    theory = lambda row: [(row[0], row[1], row[0] + '.' + row[1] + '(case: %d)' % k, row[3], row[4]) for k in (1, 2)]
    # one extra Passed row (GREEN and RED) with the given className and method
    plus = lambda cls, m: {'green_trx': trx(g_rows + [(cls, m, None, 'Passed', '')]), 'red_trx': trx(red_ok + [(cls, m, None, 'Passed', '')])}
    seven = {'real': h(green=(7, 7, 0), red=(7, 4, 3)), 'expect_green': '7/7/0', 'expect_red': '7/4/3'}
    UNK = 'UNKNOWN_POTENTIAL_MATCH'
    nonascii = lambda d, role, **want: all(d[role]['NonAsciiFqn'][k] == v for k, v in want.items())
    PAIR = chr(0x10400)  # DESERET CAPITAL LETTER LONG I: one code point, two UTF-16 code units (D801 DC00)
    # ---- A-10 fixtures. RED rows 0 (WorkspaceCacheTests.CachesPerSession) and 5 (WorkspaceLayoutTests.Builds) fail with an
    # assertion and row 6 (workspacelowerTests.Runs) with a NullReferenceException; row 7 (MathTests.Adds) is not selected.
    both = lambda **kw: {'green_trx': trx(g_rows, **kw), 'red_trx': trx(red_ok, **kw)}
    again = lambda **kw: {'green_trx': trx(g_rows + [g_rows[1]], **kw), 'red_trx': trx(red_ok + [red_ok[1]], **kw)}
    selected6 = lambda d: good(d) and d['Red']['FailureComposition'] == {'System.NullReferenceException': 1, 'xunit-assertion': 2}
    IOE_MSG = 'System.InvalidOperationException : contradictory failure'
    STACK = '<StackTrace>   at Synth.Tests.Frame.Run()</StackTrace>'
    xm = lambda text: '<Message>%s</Message>' % esc(text)
    xe = lambda *parts: '<ErrorInfo>%s</ErrorInfo>' % ''.join(parts)
    xo = lambda *parts: '<Output>%s</Output>' % ''.join(parts)
    vstest = lambda text: xo('<StdOut>synthetic output</StdOut>', xe(xm(text), STACK))  # the complete VSTest failure shape
    full = lambda rows: {i: vstest(r[4]) for i, r in enumerate(rows) if r[3] == 'Failed'}
    red_body = lambda **b: {'red_trx': trx(red_ok, bodies={int(k[1:]): v for k, v in b.items()})}

    def rows_and_counters(data):
        root = ET.fromstring(data)
        return [[sorted(e.attrib.items()) for e in root.iter('{%s}%s' % (NS, tag))] for tag in ('UnitTest', 'TestMethod', 'UnitTestResult', 'Counters')]
    t81_red = trx(red_ok, bodies={6: xo(xe(xm(NRE_MSG), xm(ASSERT_MSG)))})
    t81_same = rows_and_counters(t81_red) == rows_and_counters(trx(red_ok))
    bulk = ([('Synth.Tests.WorkspaceBulkTests', 'Case%02d' % k, None, 'Failed', ASSERT_MSG) for k in range(50)]
            + [('Synth.Tests.WorkspaceBulkTests', 'NullCase', None, 'Failed', NRE_MSG), ('Synth.Tests.MathTests', 'Adds', None, 'Passed', '')])
    bulk_green = [(c, m, d, 'Passed', '') for c, m, d, _, _ in bulk]
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
        {'Id': 'T16', 'desc': 'TRX result references an undefined testId -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_TEST_ID_UNKNOWN',
         'green_trx': trx(g_rows, extra_result='<UnitTestResult executionId="%s" testId="ffffffff-0000-4000-8000-000000000000" testName="x" outcome="Passed" />' % eid(999))},
        {'Id': 'T17', 'desc': 'duplicate UnitTest id -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_TEST_ID_DUPLICATE', 'green_trx': trx(g_rows, duplicate_id=True)},
        {'Id': 'T18', 'desc': 'DOCTYPE / ENTITY in TRX -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_UNREADABLE',
         'green_trx': trx(g_rows).replace(b'<TestRun', b'<!DOCTYPE TestRun [<!ENTITY x "y">]><TestRun', 1)},
        {'Id': 'T19', 'desc': 'unsupported filter expression -> FAIL closed', 'expect': 'FAIL', 'code': 'UNSUPPORTED_FILTER',
         'filter': 'FullyQualifiedName~Workspace|Category=Slow'},
        {'Id': 'T20', 'desc': 'Counters incoherent with the Results rows -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTERS_INCOHERENT',
         'green_trx': trx(g_rows, counters={'passed': len(g_rows) - 1})},
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
        # ---- A-9: I64-A8-ORDINAL-FILTER-MISMATCH ----
        {'Id': 'T36', 'desc': 'Work + U+017F (long s) + pace under ~Workspace: the non-ASCII unit could decide the match -> UNSUPPORTED_NON_ASCII_FQN (FAIL closed)', 'expect': 'FAIL', 'code': 'UNSUPPORTED_NON_ASCII_FQN',
         'green_trx': trx(g_long), 'red_trx': trx(r_long), 'asserts': {'ThreeUndecidable': lambda d: nonascii(d, 'Green', Rows=3, **{UNK: 3})}},
        {'Id': 'T37', 'desc': 'F + U+0131 (dotless i) + lter under ~Filter: the non-ASCII unit could decide the match -> UNSUPPORTED_NON_ASCII_FQN (FAIL closed)', 'expect': 'FAIL', 'code': 'UNSUPPORTED_NON_ASCII_FQN',
         'green_trx': trx(g_dot), 'red_trx': trx(r_dot), 'filter': 'FullyQualifiedName~Filter', 'min': '1',
         'real': h(green=(3, 3, 0), red=(3, 2, 1)), 'expect_green': '3/3/0', 'expect_red': '3/2/1', 'composition': 'xunit-assertion=1',
         'asserts': {'ThreeUndecidable': lambda d: nonascii(d, 'Red', Rows=3, **{UNK: 3})}},
        {'Id': 'T38', 'desc': 'ASCII mixed-case filter substring selects the same rows -> PASS', 'expect': 'PASS', 'filter': 'FullyQualifiedName~wOrKsPaCe', 'asserts': {'Fqn6': good}},
        # ---- A-9: I64-A8-COUNTER-COHERENCE ----
        {'Id': 'T39', 'desc': 'executed = 0 with Passed rows -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTERS_INCOHERENT', 'green_trx': trx(g_rows, counters={'executed': 0})},
        {'Id': 'T40', 'desc': 'total mismatch -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTERS_INCOHERENT', 'green_trx': trx(g_rows, counters={'total': len(g_rows) + 1})},
        {'Id': 'T41', 'desc': 'passed mismatch (RED) -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTERS_INCOHERENT', 'red_trx': trx(red_ok, counters={'passed': 5})},
        {'Id': 'T42', 'desc': 'failed mismatch (RED) -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTERS_INCOHERENT', 'red_trx': trx(red_ok, counters={'failed': 2})},
        {'Id': 'T43', 'desc': 'notExecuted mismatch -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTERS_INCOHERENT', 'green_trx': trx(g_rows, counters={'notExecuted': 1})},
        {'Id': 'T44', 'desc': 'non-neutral counter category nonzero (error) -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTER_UNSUPPORTED', 'green_trx': trx(g_rows, counters={'error': 1})},
        {'Id': 'T45', 'desc': 'Counters attribute not understood by the bridge -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTER_UNSUPPORTED', 'green_trx': trx(g_rows, counters={'flaky': 0})},
        {'Id': 'T46', 'desc': 'coherent Passed/Failed/NotExecuted counters (one NotExecuted row outside the selection) -> PASS', 'expect': 'PASS',
         'green_trx': trx(g_ne), 'red_trx': trx(r_ne),
         'asserts': {'NotExecutedCoherent': lambda d: d['Green']['Counters']['notExecuted'] == 1 and d['Green']['Counters']['executed'] == len(UNIVERSE) - 1 and good(d)}},
        {'Id': 'T47', 'desc': 'required counter attribute missing (executed) -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTERS_INCOHERENT', 'green_trx': trx(g_rows, counters={'executed': None})},
        {'Id': 'T48', 'desc': 'counter value not a non-negative integer -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_COUNTERS_INCOHERENT', 'green_trx': trx(g_rows, counters={'passed': '9x'})},
        # ---- A-9: I64-A8-DUPLICATE-RESULT-FAIL-OPEN ----
        {'Id': 'T49', 'desc': 'duplicate executionId (selected rows, both TRX) -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_EXECUTION_ID_DUPLICATE',
         'green_trx': trx(g_rows, exec_ids={2: eid(1)}), 'red_trx': trx(red_ok, exec_ids={2: eid(1)})},
        {'Id': 'T50', 'desc': 'duplicate execution record cannot inflate MinSelected (7 declared, min 7) -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_EXECUTION_ID_DUPLICATE',
         'green_trx': trx(g_rows + [g_rows[0]], shared={n: 0}, exec_ids={n: eid(0)}), 'red_trx': trx(red_ok + [red_ok[0]], shared={n: 0}, exec_ids={n: eid(0)}),
         'min': '7', 'real': h(green=(7, 7, 0), red=(7, 3, 4)), 'expect_green': '7/7/0', 'expect_red': '7/3/4', 'composition': 'System.NullReferenceException=1,xunit-assertion=3'},
        {'Id': 'T51', 'desc': 'same testId with distinct executionIds is allowed and counted -> PASS', 'expect': 'PASS',
         'green_trx': trx(g_rows + [g_rows[1]], shared={n: 1}), 'red_trx': trx(red_ok + [red_ok[1]], shared={n: 1}),
         'real': h(green=(7, 7, 0), red=(7, 4, 3)), 'expect_green': '7/7/0', 'expect_red': '7/4/3',
         'asserts': {'Counted7': lambda d: d['Green']['Selected'] == 7 and d['Red']['Selected'] == 7}},
        {'Id': 'T52', 'desc': 'Theory-like executions of one selected method with distinct executionIds stay separate rows -> PASS', 'expect': 'PASS',
         'green_trx': trx(g_rows + theory(g_rows[3]), shared={n: 3, n + 1: 3}), 'red_trx': trx(red_ok + theory(red_ok[3]), shared={n: 3, n + 1: 3}),
         'real': h(green=(8, 8, 0), red=(8, 5, 3)), 'expect_green': '8/8/0', 'expect_red': '8/5/3',
         'asserts': {'Counted8': lambda d: d['Green']['Selected'] == 8 and d['Red']['Selected'] == 8}},
        {'Id': 'T53', 'desc': 'missing executionId -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_EXECUTION_ID_MISSING', 'green_trx': trx(g_rows, exec_ids={0: None})},
        {'Id': 'T54', 'desc': 'malformed executionId -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_EXECUTION_ID_MALFORMED', 'green_trx': trx(g_rows, exec_ids={0: 'e1'})},
        {'Id': 'T55', 'desc': 'executionIds equal except for hex letter case are duplicates -> FAIL closed', 'expect': 'FAIL', 'code': 'TRX_EXECUTION_ID_DUPLICATE',
         'green_trx': trx(g_rows, exec_ids={1: 'abcdef01-aaaa-4bbb-8ccc-0000000000ab', 2: 'ABCDEF01-AAAA-4BBB-8CCC-0000000000AB'})},
        # ---- A-9 (pre-versioning disposition): potential-match semantics for I64-A8-ORDINAL-FILTER-MISMATCH ----
        dict({'Id': 'T56', 'desc': 'ASCII ordinary match RackCad.Tests.WorkspaceNavigationTests.SomeTest -> SELECTED (PASS)', 'expect': 'PASS',
              'asserts': {'Selected7': lambda d: d['Green']['Selected'] == 7 and d['Red']['Selected'] == 7 and nonascii(d, 'Green', Rows=0)}},
             **plus('RackCad.Tests.WorkspaceNavigationTests', 'SomeTest'), **seven),
        dict({'Id': 'T57', 'desc': 'ASCII mixed-case match RackCad.Tests.wOrKsPaCeNavigationTests.SomeTest -> SELECTED (PASS)', 'expect': 'PASS',
              'asserts': {'Selected7': lambda d: d['Green']['Selected'] == 7 and d['Diagnostics']['Green']['CaseSensitiveFqn'] == 4}},
             **plus('RackCad.Tests.wOrKsPaCeNavigationTests', 'SomeTest'), **seven),
        dict({'Id': 'T58', 'desc': 'unrelated U+00F1: Create_AcuñaUnVariableIdNUEVO under ~Workspace has no possible window -> NOT SELECTED, no failure (PASS)', 'expect': 'PASS',
              'asserts': {'NotSelected': lambda d: d['Green']['Selected'] == 6 and d['Red']['Selected'] == 6 and nonascii(d, 'Green', Rows=1, NOT_SELECTED=1, **{UNK: 0})
                          and nonascii(d, 'Red', Rows=1, NOT_SELECTED=1, **{UNK: 0})}},
             **plus('RackCad.Tests.ProjectVariableMutationPreflightTests', 'Create_Acu' + chr(0xF1) + 'aUnVariableIdNUEVO')),
        dict({'Id': 'T59', 'desc': 'unrelated non-ASCII text plus an actual ASCII Workspace substring -> SELECTED (PASS)', 'expect': 'PASS',
              'asserts': {'Selected7': lambda d: d['Green']['Selected'] == 7 and nonascii(d, 'Green', Rows=1, SELECTED=1, **{UNK: 0})}},
             **plus('RackCad.Tests.WorkspaceNavigationTests', 'Navega_A' + chr(0xF1) + 'adeDestino'), **seven),
        dict({'Id': 'T60', 'desc': 'a potential long-s window and an all-ASCII Workspace window in the same FQN: inclusion cannot depend on Unicode -> SELECTED (PASS)', 'expect': 'PASS',
              'asserts': {'Selected7': lambda d: d['Green']['Selected'] == 7 and nonascii(d, 'Green', Rows=1, SELECTED=1, **{UNK: 0})}},
             **plus('Synth.Tests.Work' + chr(0x17F) + 'paceWorkspaceTests', 'Runs'), **seven),
        dict({'Id': 'T61', 'desc': 'surrogate pair as TWO units: Wo + U+10400 + space fills r,k of Workspace -> UNSUPPORTED_NON_ASCII_FQN (collapsing the pair to one position would wrongly report not selected)',
              'expect': 'FAIL', 'code': 'UNSUPPORTED_NON_ASCII_FQN', 'asserts': {'OneUndecidable': lambda d: nonascii(d, 'Green', Rows=1, **{UNK: 1})}},
             **plus('Synth.Tests.Wo' + PAIR + 'spaceTests', 'Runs')),
        dict({'Id': 'T62', 'desc': 'surrogate pair as TWO units: Work + U+10400 + pace has no possible window -> NOT SELECTED (collapsing the pair to one position would wrongly fail closed) (PASS)',
              'expect': 'PASS', 'asserts': {'NotSelected': lambda d: d['Green']['Selected'] == 6 and nonascii(d, 'Green', Rows=1, NOT_SELECTED=1, **{UNK: 0})}},
             **plus('Synth.Tests.Work' + PAIR + 'paceTests', 'Runs')),
        # ---- A-10: I64-A9-TESTID-GUID-ALIAS (UnitTest@id and UnitTestResult@testId are GUID identities) ----
        dict({'Id': 'T63', 'desc': 'two UnitTest definitions of the same method whose ids differ only by hex capitalization (a second execution under each) -> TRX_TEST_ID_DUPLICATE (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_TEST_ID_DUPLICATE'},
             **again(test_ids={1: 'abcdef02-aaaa-4bbb-8ccc-0000000000ab', 9: 'ABCDEF02-AAAA-4BBB-8CCC-0000000000AB'}), **seven),
        dict({'Id': 'T64', 'desc': 'conflicting TestMethod definitions (one selected, one not) under GUID-equivalent ids abcdef03.../ABCDEF03... -> TRX_TEST_ID_DUPLICATE (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_TEST_ID_DUPLICATE'},
             **both(test_ids={0: 'abcdef03-0000-4000-8000-0000000000cd', 7: 'ABCDEF03-0000-4000-8000-0000000000CD'})),
        dict({'Id': 'T65', 'desc': 'UnitTestResult@testId written in the other hex casing of its definition (lower->UPPER, UPPER->lower) resolves to that definition -> PASS',
              'expect': 'PASS', 'asserts': {'Resolved': selected6}},
             **both(test_ids={0: 'abcdef04-0000-4000-8000-0000000000ef', 6: 'ABCDEF05-0000-4000-8000-0000000000EF'},
                    result_ids={0: 'ABCDEF04-0000-4000-8000-0000000000EF', 6: 'abcdef05-0000-4000-8000-0000000000ef'})),
        {'Id': 'T66', 'desc': 'malformed UnitTest@id (non-hexadecimal digits), referenced verbatim by its result -> TRX_TEST_ID_MALFORMED (FAIL closed)',
         'expect': 'FAIL', 'code': 'TRX_TEST_ID_MALFORMED', 'green_trx': trx(g_rows, test_ids={2: 'zzzzzzzz-0000-4000-8000-000000000000'})},
        {'Id': 'T67', 'desc': 'malformed UnitTestResult@testId -> TRX_TEST_ID_MALFORMED (FAIL closed)',
         'expect': 'FAIL', 'code': 'TRX_TEST_ID_MALFORMED', 'green_trx': trx(g_rows, result_ids={2: 'not-a-guid'})},
        {'Id': 'T68', 'desc': 'well-formed UnitTestResult@testId with no UnitTest definition (RED, rows and Counters unchanged) -> TRX_TEST_ID_UNKNOWN (FAIL closed)',
         'expect': 'FAIL', 'code': 'TRX_TEST_ID_UNKNOWN', 'red_trx': trx(red_ok, result_ids={2: 'fedcba98-0000-4000-8000-000000000000'})},
        dict({'Id': 'T69', 'desc': 'distinct GUIDs differing in one hex digit (abcdef06...0a / ABCDEF06...0B) stay two definitions, each resolving to its own -> PASS',
              'expect': 'PASS', 'asserts': {'Distinct': selected6}},
             **both(test_ids={0: 'abcdef06-0000-4000-8000-00000000000a', 7: 'ABCDEF06-0000-4000-8000-00000000000B'})),
        dict({'Id': 'T70', 'desc': 'two executions of one test sharing its testId, the second written in the other hex casing, distinct executionIds -> PASS, counted 7',
              'expect': 'PASS', 'asserts': {'Counted7': lambda d: d['Green']['Selected'] == 7 and d['Red']['Selected'] == 7}},
             **again(shared={9: 1}, test_ids={1: 'abcdef07-0000-4000-8000-0000000000ab'}, result_ids={9: 'ABCDEF07-0000-4000-8000-0000000000AB'}), **seven),
        {'Id': 'T71', 'desc': 'UnitTest without id -> TRX_TEST_ID_MISSING (FAIL closed)', 'expect': 'FAIL', 'code': 'TRX_TEST_ID_MISSING',
         'green_trx': trx(g_rows, test_ids={3: None}, result_ids={3: '00000004-0000-4000-8000-000000000000'})},
        {'Id': 'T72', 'desc': 'UnitTestResult without testId -> TRX_TEST_ID_MISSING (FAIL closed)', 'expect': 'FAIL', 'code': 'TRX_TEST_ID_MISSING',
         'red_trx': trx(red_ok, result_ids={3: None})},
        {'Id': 'T73', 'desc': 'GUID in a .NET form other than the TRX 8-4-4-4-12 form (braces), referenced verbatim -> TRX_TEST_ID_MALFORMED (FAIL closed)',
         'expect': 'FAIL', 'code': 'TRX_TEST_ID_MALFORMED', 'green_trx': trx(g_rows, test_ids={2: '{00000003-0000-4000-8000-000000000000}'})},
        # ---- A-10: I64-A9-FAILURE-MESSAGE-AMBIGUITY (cardinality of UnitTestResult/Output/ErrorInfo/Message before classifying) ----
        dict({'Id': 'T74', 'desc': 'exactly one Output/ErrorInfo/Message on every failed row, in the complete VSTest shape (StdOut, Message, StackTrace) -> PASS',
              'expect': 'PASS', 'asserts': {'Composition': selected6}}, red_trx=trx(red_ok, bodies=full(red_ok))),
        dict({'Id': 'T75', 'desc': 'duplicate Message, the second contradictory (System.InvalidOperationException after the assertion message) -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'}, **red_body(r0=xo(xe(xm(ASSERT_MSG), xm(IOE_MSG), STACK)))),
        dict({'Id': 'T76', 'desc': 'duplicate Message with the same text -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'}, **red_body(r0=xo(xe(xm(ASSERT_MSG), xm(ASSERT_MSG))))),
        dict({'Id': 'T77', 'desc': 'duplicate ErrorInfo in the Output, the second contradictory -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'}, **red_body(r0=xo(xe(xm(ASSERT_MSG)), xe(xm(IOE_MSG))))),
        dict({'Id': 'T78', 'desc': 'duplicate Output, the second with a contradictory ErrorInfo -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'}, **red_body(r0=xo(xe(xm(ASSERT_MSG))) + xo(xe(xm(IOE_MSG))))),
        dict({'Id': 'T79', 'desc': 'Failed row with Output/ErrorInfo but no Message (StackTrace only) -> TRX_FAILURE_MESSAGE_MISSING (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_MISSING'}, **red_body(r5=xo(xe(STACK)))),
        dict({'Id': 'T80', 'desc': 'second contradictory exception type in a parallel ErrorInfo directly under the NullReferenceException UnitTestResult -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'}, **red_body(r6=xo(xe(xm(NRE_MSG))) + xe(xm(IOE_MSG)))),
        {'Id': 'T81', 'desc': 'rows, definitions and Counters identical to the T01 fixture; only the NullReferenceException row carries a second Message -> still FAIL closed',
         'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS', 'red_trx': t81_red, 'asserts': {'RowsAndCountersAsT01': lambda d: t81_same}},
        {'Id': 'T82', 'desc': 'Passed GREEN row with two Output elements: the structure is checked on every result although its message is not consumed -> FAIL closed',
         'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS', 'green_trx': trx(g_rows, bodies={1: xo('<StdOut>a</StdOut>') + xo('<StdOut>b</StdOut>')})},
        {'Id': 'T83', 'desc': 'expected 50 assertion + 1 NullReferenceException composition, unambiguous, in the complete VSTest shape -> PASS',
         'expect': 'PASS', 'green_trx': trx(bulk_green), 'red_trx': trx(bulk, bodies=full(bulk)),
         'real': h(green=(51, 51, 0), red=(51, 0, 51)), 'expect_green': '51/51/0', 'expect_red': '51/0/51', 'composition': 'System.NullReferenceException=1,xunit-assertion=50',
         'asserts': {'Composition51': lambda d: d['Red']['FailureComposition'] == {'System.NullReferenceException': 1, 'xunit-assertion': 50} and d['Red']['Failed'] == 51}},
        dict({'Id': 'T84', 'desc': 'Message with a child element (ambiguous nested structure) -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'},
             **red_body(r0=xo(xe('<Message>Assert.Equal() Failure<Detail>System.InvalidOperationException : nested</Detail></Message>')))),
        dict({'Id': 'T85', 'desc': 'the only Message of a failed row lies below ErrorInfo/StackTrace, not on the path -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'}, **red_body(r5=xo(xe('<StackTrace>%s</StackTrace>' % xm(ASSERT_MSG))))),
        dict({'Id': 'T86', 'desc': 'a Message in another XML namespace beside the TRX Message (counted by local name) -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'},
             **red_body(r0=xo(xe('<Message xmlns="urn:synthetic:other">%s</Message>' % esc(IOE_MSG), xm(ASSERT_MSG))))),
        dict({'Id': 'T87', 'desc': 'the only ErrorInfo of a failed row is directly under UnitTestResult, outside Output -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'}, **red_body(r5=xe(xm(ASSERT_MSG)))),
        dict({'Id': 'T88', 'desc': 'a Message outside ErrorInfo (Output/TextMessages) beside the single failure Message is not on the consumed path -> PASS, composition unchanged',
              'expect': 'PASS', 'asserts': {'Composition': selected6}},
             **red_body(r0=xo('<TextMessages>%s</TextMessages>' % xm(IOE_MSG), xe(xm(ASSERT_MSG))))),
        dict({'Id': 'T89', 'desc': 'a contradictory Message directly under Output (neither in ErrorInfo nor in TextMessages) beside the failure Message -> TRX_FAILURE_MESSAGE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_FAILURE_MESSAGE_AMBIGUOUS'}, **red_body(r0=xo(xm(IOE_MSG), xe(xm(ASSERT_MSG))))),
        # ---- A-10: consumed TRX elements only in their place and namespace (same principle: nothing parallel is ignored) ----
        dict({'Id': 'T90', 'desc': 'a Failed UnitTestResult nested in another result (not through InnerResults), rows and Counters otherwise as T01 -> TRX_STRUCTURE_AMBIGUOUS (FAIL closed)',
              'expect': 'FAIL', 'code': 'TRX_STRUCTURE_AMBIGUOUS'},
             **red_body(r1=xo('<UnitTestResult executionId="%s" testId="00000002-0000-4000-8000-000000000000" testName="x" outcome="Failed" />' % eid(900)))),
        {'Id': 'T91', 'desc': 'a second Results element in another XML namespace with a Failed row -> TRX_STRUCTURE_AMBIGUOUS (FAIL closed)',
         'expect': 'FAIL', 'code': 'TRX_STRUCTURE_AMBIGUOUS',
         'red_trx': trx(red_ok).replace(b'<ResultSummary', ('<Results xmlns="urn:synthetic:other"><UnitTestResult executionId="%s" testId="00000002-0000-4000-8000-000000000000" outcome="Failed" /></Results><ResultSummary' % eid(901)).encode('ascii'), 1)},
        {'Id': 'T92', 'desc': 'a TestMethod in another XML namespace beside the TRX TestMethod of a definition -> TRX_STRUCTURE_AMBIGUOUS (FAIL closed)',
         'expect': 'FAIL', 'code': 'TRX_STRUCTURE_AMBIGUOUS',
         'green_trx': trx(g_rows).replace(b'<TestMethod ', b'<TestMethod xmlns="urn:synthetic:other" className="Synth.Tests.WorkspaceShadowTests" name="Shadow" /><TestMethod ', 1)},
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
