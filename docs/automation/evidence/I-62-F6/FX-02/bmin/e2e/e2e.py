# B-min end-to-end integration (supervision, plane a; decisions §75). No model is invoked.
# 1. fresh clone of the fixture origin OUTSIDE D:\r62-fixture; 2. the real tool on the FX-U1 scenario facts; 3. zero-write snapshot around all runs;
# 4. the independent acceptor (R1-R10) over every envelope; 5. tampered envelopes; 6. every tool mutant against the acceptor on real data;
# 7. supervision-only cross-check with the custodied probe-2 reference (never given to tool, acceptor or Controller).
import hashlib, importlib.util, json, os, platform, shutil, stat, subprocess, sys
NL = chr(10)
HERE = os.path.dirname(os.path.abspath(__file__))
PKG = os.path.dirname(HERE)
TOOL = os.path.join(PKG, 'tool', 'bmin_facts.py')
ACC = os.path.join(PKG, 'accept', 'bmin_accept.py')
OUT = os.path.join(HERE, 'out')
ROOT = 'D:/r62-bmin-tmp/e2e'
CLONE = ROOT + '/clone'
INP = ROOT + '/inputs'
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
TRX = os.path.join(WT, 'docs', 'automation', 'evidence', 'I-62-F6', 'FX-02', 'U07-trx-R37949808974', 'fixture-tests.trx')
P2REF = os.path.join(WT, 'docs', 'automation', 'evidence', 'I-62-F6', 'OD-2', 'R20261011T102825Z-a2p2-v5-controller', 'reference.json')
BR = 'fx/u1'
BASE = '1a4fc9c62bb564d7c51fad8158876a5d5a8a5bdf'
HEADX = 'ae25b596a11fe3508ac60a3ef7de5dc23df971ce'
RED = 'd30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99'
MAIN = 'fbe25347799e5b801ef708454212335537448bd1'
TIP = '95bdc29d428b35d2188a10e1c5462d6caf2be4df'
K = 'docs/automation/decisions/FX-U1-T1.gate-contract.json'
STATE = 'docs/automation/state/FX-U1.yml'
DEC = 'docs/automation/decisions/FX-U1.md'
IGN = 'artifacts/orchestration/FX-U1/T1/0/R20261011T000000Z-e2e0/output.json'
# closed list of 16.13 Surfaces (AP f525cb1e L738-L739): the normative input of MV-3, read as pathspecs
SURFACES = ['AGENTS.md', 'CLAUDE.md', 'docs/AUTOMATION_PLAN.md', 'docs/FOUNDATIONS.md', 'docs/INITIATIVE_LIFECYCLE.md', 'docs/WORKFLOW.md', 'docs/adr/',
            'docs/automation/agent-execution/', 'docs/initiatives/PROMPT_TEMPLATES.md']
GENV = dict(os.environ, GIT_OPTIONAL_LOCKS='0', GIT_TERMINAL_PROMPT='0', MSYS_NO_PATHCONV='1')
sha256f = lambda p: hashlib.sha256(open(p, 'rb').read()).hexdigest()


def blob_sha1(p):
    b = open(p, 'rb').read()
    return hashlib.sha1(b'blob ' + str(len(b)).encode() + b'\0' + b).hexdigest()


def git(*a, cwd=CLONE):
    return subprocess.run(['git', '-C', cwd] + list(a), capture_output=True, env=GENV).stdout.decode('utf-8', 'replace')


def rmtree(p):
    def onexc(f, q, e):
        os.chmod(q, stat.S_IWRITE)
        f(q)
    if os.path.isdir(p):
        shutil.rmtree(p, onexc=onexc)


def snapshot():
    idx = os.path.join(CLONE, '.git', 'index')
    files = []
    for r, ds, fs in os.walk(CLONE):
        for f in fs:
            q = os.path.join(r, f)
            files.append((os.path.relpath(q, CLONE).replace(os.sep, '/'), os.path.getsize(q)))
    return {'IndexSha256': sha256f(idx), 'Refs': hashlib.sha256(git('for-each-ref').encode()).hexdigest(),
            'StatusIgnored': git('status', '--porcelain', '--ignored'), 'FilesSha256': hashlib.sha256(json.dumps(sorted(files)).encode()).hexdigest(),
            'FileCount': len(files)}


def exe_sha256():
    try:
        return sha256f(sys.executable), 'sys.executable'
    except OSError:
        return sha256f(os.path.join(sys.base_prefix, os.path.basename(sys.executable))), 'sys.base_prefix'


def run_tool(tool, argv):
    p = subprocess.run([sys.executable, '-I', tool] + argv, capture_output=True, env=GENV)
    return p.returncode, p.stdout


rmtree(ROOT)
os.makedirs(INP)
os.makedirs(OUT, exist_ok=True)
subprocess.run(['git', 'clone', '-q', '--no-local', '-c', 'core.autocrlf=false', '-b', BR, 'D:/r62-fixture/fixture-origin.git', CLONE], check=True, env=GENV)
assert git('rev-parse', 'HEAD').strip() == TIP
# pinned inputs (outside the clone)
kb = subprocess.run(['git', '-C', CLONE, 'show', TIP + ':' + K], capture_output=True, check=True).stdout
open(INP + '/K.json', 'wb').write(kb)
handoff = {'Schema': 'rackcad-worker-handoff/v1', 'TaskId': 'T1', 'RunId': 'R20261011T000000Z-e2e0', 'DelegationRunId': 'R20261011T000000Z-e2ep',
           'Initiative': 'FX-U1', 'Gate': 'G1', 'Attempt': 0, 'BaseSha': BASE, 'RedSha': RED, 'CurrentSha': HEADX, 'Branch': BR,
           'Worktree': CLONE, 'Pushed': True, 'FilesChanged': ['src/Fixture.Lib/Calculator.cs'],
           'WorkCompleted': 'A\u00f1ad\u00ed Subtract \u2014 escenario sint\u00e9tico de integraci\u00f3n B-min', 'TestsExecuted': ['FullyQualifiedName~Subtract'],
           'TestResults': 'sint\u00e9tico', 'Evidence': ['GATE PASS (t\u00e9rmino de 16.10 a prop\u00f3sito, para el hecho de freetext)'],
           'UnexpectedFindings': [], 'KnownLimitations': [], 'Deviations': [], 'OpenQuestions': [], 'WorkerStatus': 'COMPLETED',
           'Disposition': 'NONE', 'TriggeredStopConditions': [], 'RecommendedNextAction': 'verificar', 'Worker': {'Trailer': 'Co-Authored-By: synthetic'}}
open(INP + '/handoff.json', 'w', encoding='utf-8', newline=NL).write(json.dumps(handoff, ensure_ascii=False, indent=1) + NL)
relay = {'Schema': 'synthetic-relay-facts', 'Note': 'integracion B-min; sin valor normativo'}
open(INP + '/relay.json', 'w', encoding='utf-8', newline=NL).write(json.dumps(relay) + NL)
shutil.copyfile(TRX, INP + '/fixture-tests.trx')
H = {n: sha256f(INP + '/' + n) for n in ('K.json', 'handoff.json', 'relay.json', 'fixture-tests.trx')}
assert H['fixture-tests.trx'] == '4ccdf2cecba561259b877a471d577f1afa6ac4a9ad00ceb4fa0417e7c61a7513'
RUNS = [
    ('identity', ['identity', '--repo', CLONE, '--branch', BR, '--base', BASE, '--current', HEADX, '--red', RED]),
    ('diff', ['diff', '--repo', CLONE, '--from', BASE, '--to', HEADX]),
    ('scope', ['scope', '--repo', CLONE, '--from', BASE, '--to', HEADX, '--file', 'DELEG=' + INP + '/K.json', '--sha256', 'DELEG=' + H['K.json']]),
    ('contract', ['contract', '--repo', CLONE, '--file', 'CONTRACT=' + INP + '/K.json', '--sha256', 'CONTRACT=' + H['K.json'],
                  '--file', 'DELEG=' + INP + '/K.json', '--sha256', 'DELEG=' + H['K.json']]),
    ('hashes', ['hashes', '--repo', CLONE, '--rev', TIP, '--paths', K, 'src/Fixture.Lib/Calculator.cs']),
    ('cleantree', ['cleantree', '--repo', CLONE, '--ignore-path', IGN]),
    ('read-text-1', ['read-text', '--repo', CLONE, '--rev', TIP, '--path', DEC, '--lines', '1-1']),
    ('read-text-73', ['read-text', '--repo', CLONE, '--rev', TIP, '--path', DEC, '--lines', '73-73']),
    ('read-json', ['read-json', '--repo', CLONE, '--rev', TIP, '--path', K, '--pointer', '/Objective', '--pointer', '/Schema',
                   '--pointer', '/MainSha', '--pointer', '/AuthorityRevision']),
    ('trx', ['trx', '--repo', CLONE, '--file', 'TRX=' + INP + '/fixture-tests.trx', '--sha256', 'TRX=' + H['fixture-tests.trx'], '--filter-substring', 'Subtract']),
    ('handoff', ['handoff', '--repo', CLONE, '--file', 'H=' + INP + '/handoff.json', '--sha256', 'H=' + H['handoff.json'], '--schema-rev', TIP]),
    ('freetext', ['freetext', '--repo', CLONE, '--file', 'H=' + INP + '/handoff.json', '--sha256', 'H=' + H['handoff.json'], '--terms-rev', MAIN]),
    ('clausemap', ['clausemap', '--repo', CLONE, '--main', MAIN, '--state-rev', BASE, '--state-path', STATE,
                   '--file', 'CONTRACT=' + INP + '/K.json', '--sha256', 'CONTRACT=' + H['K.json'], '--base', BASE]),
]
before = snapshot()
env_paths = {}
rcs = {}
for name, argv in RUNS:
    rc, out = run_tool(TOOL, argv)
    rcs[name] = rc
    p = os.path.join(OUT, 'envelope-' + name + '.json')
    open(p, 'wb').write(out)
    env_paths[name] = p
after_tool = snapshot()
# closure (R9): every commit of the clone, the refs read, the pinned files
commits = [c for c in git('rev-list', '--all').split() if c]
kj = json.loads(kb.decode('utf-8'))
# RevPaths (errata E7): every path of the full tree at each commit the scenario names (the Controller may read the whole fixture clone at those commits)
AR = kj['AuthorityRevision']
MB = git('merge-base', MAIN, AR).strip()
EFF1, EFF2 = git('rev-parse', MAIN + '^1').strip(), git('rev-parse', MAIN + '^2').strip()
named = []
for c in (TIP, BASE, HEADX, RED, MAIN, EFF1, EFF2, AR, MB):
    if c and c not in named:
        named.append(c)
# paths listed by the clause map at EFF (Files and Entries): normative inputs of Validate (MV-3..MV-6), read even where absent in a tree
_m = json.loads(subprocess.run(['git', '-C', CLONE, 'show', MAIN + ':docs/automation/agent-execution/compatibility/I62-clause-map.json'], capture_output=True, check=True).stdout.decode('utf-8'))
MAP_PATHS = sorted(set([f['Path'] for f in _m.get('Files', [])] + [e['Path'] for e in _m.get('Entries', [])]))
revpaths = [{'Rev': c, 'Path': p} for c in named for p in git('ls-tree', '-r', '--name-only', '--full-tree', c).splitlines() if p]
closure = {'Schema': 'rackcad-bmin-closure/0-experimental', 'Commits': commits, 'RevPaths': revpaths,
           'Refs': ['HEAD', 'refs/remotes/origin/' + BR, 'refs/remotes/origin/main'],
           'Paths': [IGN, K, STATE, DEC, 'src/Fixture.Lib/Calculator.cs', 'docs/AUTOMATION_PLAN.md', 'docs/WORKFLOW.md',
                     'docs/automation/agent-execution/compatibility/I62-clause-map.json', 'docs/automation/agent-execution/compatibility/clause-map.schema.json',
                     'docs/automation/agent-execution/schemas/worker-handoff.schema.json'] + [c['Path'] for c in kj.get('Authorities', [])]
                    + SURFACES + MAP_PATHS,
           'Files': [{'Name': n, 'Sha256': H[f]} for n, f in (('DELEG', 'K.json'), ('CONTRACT', 'K.json'), ('H', 'handoff.json'), ('TRX', 'fixture-tests.trx'))]}
open(INP + '/closure.json', 'w', encoding='utf-8', newline=NL).write(json.dumps(closure, indent=1) + NL)
xsha, xsrc = exe_sha256()


def request(tool_path, pinned, envs, name='request.json'):
    req = {'Schema': 'rackcad-bmin-accept-request/0-experimental', 'Repo': CLONE,
           'Tool': {'Path': tool_path, 'PinnedBlob': pinned, 'RunFrom': 'path'},
           'Interpreter': {'Python': platform.python_version(), 'ExecutableSha256': xsha},
           'Runs': [{'Name': n, 'Envelope': envs[n], 'Argv': a} for n, a in RUNS],
           'Closure': {'Path': INP + '/closure.json', 'Sha256': sha256f(INP + '/closure.json')},
           'RelayFacts': {'Path': INP + '/relay.json', 'CitedSha256': H['relay.json']},
           'NotApplicable': {'R4': 'sin salida del Controller en la integracion: la aporta la medicion; R4 cubierto por las pruebas del aceptante'}}
    p = os.path.join(INP, name)
    open(p, 'w', encoding='utf-8', newline=NL).write(json.dumps(req, indent=1) + NL)
    return p


def accept(req):
    p = subprocess.run([sys.executable, '-I', ACC, '--request', req], capture_output=True, env=GENV)
    try:
        return p.returncode, json.loads(p.stdout.decode('ascii'))
    except Exception:
        return p.returncode, {'Raw': p.stdout.decode('utf-8', 'replace')[:2000], 'Stderr': p.stderr.decode('utf-8', 'replace')[:2000]}


tblob = blob_sha1(TOOL)
rc_ok, res_ok = accept(request(TOOL, tblob, env_paths))
after_acc = snapshot()
open(os.path.join(OUT, 'acceptor-result.json'), 'w', encoding='ascii', newline=NL).write(json.dumps(res_ok, indent=1) + NL)
# tampered envelopes: drop one diff path, with and without recomputed FactsSha256
tam = {}
for mode in ('keep-sha', 'recompute-sha'):
    e = json.loads(open(env_paths['diff'], 'rb').read())
    e['Facts']['Paths'] = e['Facts']['Paths'][1:]
    if mode == 'recompute-sha':
        e['FactsSha256'] = hashlib.sha256(json.dumps(e['Facts'], ensure_ascii=True, sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    tp = os.path.join(INP, 'tampered-diff-' + mode + '.json')
    open(tp, 'wb').write((json.dumps(e, ensure_ascii=True, sort_keys=True, separators=(',', ':')) + NL).encode('ascii'))
    envs = dict(env_paths, diff=tp)
    rc, res = accept(request(TOOL, tblob, envs, 'request-tampered-' + mode + '.json'))
    tam[mode] = {'rc': rc, 'Decision': res.get('Decision'), 'Mismatches': [c['Id'] for c in res.get('Checks', []) if c.get('Status') == 'MISMATCH']}
# mutants of the real tool, run on the real fixture data and judged by the acceptor
spec = importlib.util.spec_from_file_location('bmin_mutate', os.path.join(PKG, 'tool', 'tests', 'mutate.py'))
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
src = open(TOOL, encoding='utf-8').read()
mutres = []
MD = ROOT + '/mutants'
os.makedirs(MD)
for mid, desc, anchor, repl, _h in mod.MUTANTS:
    assert src.count(anchor) == 1, mid
    mp = MD + '/' + mid + '.py'
    open(mp, 'w', encoding='utf-8', newline='').write(src.replace(anchor, repl))
    envs = {}
    for name, argv in RUNS:
        rc, out = run_tool(mp, argv)
        q = MD + '/' + mid + '-' + name + '.json'
        open(q, 'wb').write(out)
        envs[name] = q
    rc, res = accept(request(mp, blob_sha1(mp), envs, 'request-' + mid + '.json'))
    changed = [n for n in env_paths if open(envs[n], 'rb').read() != open(env_paths[n], 'rb').read()]
    mutres.append({'Id': mid, 'Description': desc, 'EnvelopesChanged': changed, 'Decision': res.get('Decision'),
                   'Mismatches': [c['Id'] for c in res.get('Checks', []) if c.get('Status') == 'MISMATCH']})
after_all = snapshot()
# supervision-only cross-check against the custodied probe-2 reference
ref = json.load(open(P2REF, encoding='utf-8'))
cm = json.loads(open(env_paths['clausemap'], 'rb').read())['Facts']
xc = {'ReferenceKeysSample': sorted(list(ref.keys()))[:12]}
try:
    v = cm['Validate']
    xc.update({'Eff': cm['Derive']['Eff'], 'MV4LiteralViolations': len(v['MV4']['LiteralViolations']), 'MV4AddedPresentInEff1': len(v['MV4']['AddedPresentInEff1']),
               'MV4EntryFilePresentInEff1': len(v['MV4']['EntryFilePresentInEff1']), 'MV7': {k: v['MV7'][k] for k in ('PointerIsFirstSentenceS16', 'PointerIsLastSentenceS163', 'S163EqualAfterPointerRemoval')},
               'MapBlob': v['MV1'].get('MapBlob'), 'SchemaBlob': v['MV1'].get('SchemaBlob')})
except Exception as ex:
    xc['Error'] = repr(ex)
summary = {'Tip': TIP, 'ToolSha256': sha256f(TOOL), 'ToolBlob': tblob, 'AcceptorSha256': sha256f(ACC), 'AcceptorBlob': blob_sha1(ACC),
           'Python': platform.python_version(), 'ExecutableSource': xsrc, 'ToolExitCodes': rcs,
           'ZeroWrite': {'BeforeEqualsAfterTool': before == after_tool, 'BeforeEqualsAfterAcceptor': before == after_acc, 'BeforeEqualsAfterAll': before == after_all,
                         'Before': before, 'AfterAll': after_all},
           'Acceptor': {'rc': rc_ok, 'Decision': res_ok.get('Decision'), 'Checks': [(c.get('Id'), c.get('Status')) for c in res_ok.get('Checks', [])]},
           'Tampered': tam, 'Mutants': mutres, 'CrossCheckSupervisionOnly': xc,
           'AllAsciiEnvelopes': all(max(open(p, 'rb').read(), default=0) < 128 for p in env_paths.values())}
open(os.path.join(OUT, 'e2e-summary.json'), 'w', encoding='ascii', newline=NL).write(json.dumps(summary, indent=1, ensure_ascii=True) + NL)
print(json.dumps({k: summary[k] for k in ('ToolExitCodes', 'Acceptor', 'Tampered', 'AllAsciiEnvelopes')}, indent=1))
print('zero-write', summary['ZeroWrite']['BeforeEqualsAfterTool'], summary['ZeroWrite']['BeforeEqualsAfterAcceptor'], summary['ZeroWrite']['BeforeEqualsAfterAll'])
print('mutants', [(m['Id'], m['Decision'], m['Mismatches'], m['EnvelopesChanged'][:3]) for m in mutres])
print('xcheck', json.dumps(xc, ensure_ascii=True))
