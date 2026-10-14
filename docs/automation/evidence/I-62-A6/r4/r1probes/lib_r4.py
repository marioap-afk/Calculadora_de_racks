# lib_r4.py - helpers of the A-6 round 4 bench (I-62): the round-3 bench libraries (byte copies) with this bench's work area,
# the delivery scenario of the round-2 independence evidence (part (d): VERIFY of T1 on the simulated delivery), the effective
# input closures and role invocations of each probe, and the vectors of the corrected derivation (derive_r4.py).
# Stdlib only. Never writes in the I-62 worktree, the main checkout, D:\r62-fixture, D:\r62-a6r2 or D:\r62-a6r3.
import copy, hashlib, importlib.util, json, os, shutil, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


X = _load('lib_r3')                     # loads lib_r1r4, reconstruct_runs and table_r3 (byte copies of round 3)
L, R, TB = X.L, X.R, X.TB
DV = _load('derive_r4')

WORK = 'D:/r62-a6r4/r1'
X.WORK = WORK
L.WORK = WORK
L.PIN = WORK + '/pin'
L.PIN_TOOL = L.PIN + '/bmin_facts.py'
L.PIN_ACC = L.PIN + '/bmin_accept.py'
L.BIN = WORK + '/bin'
L.HERE = HERE

# ------------------------------------------------------------------ the delivery scenario (round 2, part (d)); read only
SRC_DELIV = 'D:/r62-a6r2/r3/deliv'
R2D = 'D:/r62-a6r2/r3/runs/d'
R2_EIC = R2D + '/EIC-VERIFY.input-closure.json'
R2_EIC_STRICT = R2D + '/EIC-I6-STRICT.input-closure.json'
R2_CLOSURE = R2D + '/EIC-VERIFY.bmin-closure.json'
R2_CLOSURE_STRICT = R2D + '/EIC-I6-STRICT.bmin-closure.json'
R2_INP = R2D + '/inputs'
TIP = '95bdc29d428b35d2188a10e1c5462d6caf2be4df'          # D.BaseSha = H.BaseSha of the delivery
RED = '53436cb20adf1fe87dc91b0382dd3123a1fac44f'          # H.RedSha
G = 'b417d36cb820a27c78d6c7d50c234460469c89f8'            # H.CurrentSha = I.Target.Commit (GREEN)
MAIN = 'fbe25347799e5b801ef708454212335537448bd1'
AR = '1746b404e57d69170ecfc084fd24fa3e699916d7'
OTHER = '13654f9'                                          # head of origin/fx/u1-fx05 (resolved at run time; not in the closures)
BR = 'fx/u1'
K = 'docs/automation/decisions/FX-U1-T1.gate-contract.json'
DEC = 'docs/automation/decisions/FX-U1.md'
STATE = 'docs/automation/state/FX-U1.yml'
MAP = 'docs/automation/agent-execution/compatibility/I62-clause-map.json'
TARGET_PATH = 'src/Fixture.Lib/Calculator.cs'
TARGET_BLOB = '380892ee869194aa4bd56dcf32dfd76ef0203ab9'
DELIV_FILES = ['docs/automation/evidence/FX-U1-agent/T1/worker-notes.md', 'src/Fixture.Lib/Calculator.cs', 'tests/Fixture.Tests/CalculatorTests.cs']
TRX_E = 'artifacts/ci/R37949808974/fixture-tests.trx'
D_RUN = 'R20261012T090000Z-d001'
H_RUN = 'R20261012T091000Z-a001'
V_RUN = 'R20261012T092000Z-c001'                           # the real verification (round 2, part (d))
NX = X.NX


def sha_file(p):
    return X.sha_file(p)


def blob_file(p):
    return L.blob_sha1(L.rbytes(p))


def jdump_local(p, obj):
    L.wjson_local(p, obj)
    return p


def deliv_path(task, run, name):
    return 'artifacts/orchestration/FX-U1/%s/0/%s/%s' % (task, run, name)


def ignore_path(inv):
    return 'artifacts/orchestration/%s/%s/%s/%s/output.json' % (inv['UnitId'], inv['TaskId'], inv['Attempt'], inv['RunId'])


def invocation(run_id, target_commit=G):
    return {'UnitId': 'FX-U1', 'TaskId': 'T1', 'Attempt': 0, 'RunId': run_id,
            'Target': {'Commit': target_commit, 'Path': TARGET_PATH, 'Blob': TARGET_BLOB}}


# ------------------------------------------------------------------ clones (copies of the delivery repository, refs preserved)
def copy_repo(name):
    T = WORK + '/c-' + name
    L.rmtree(T)
    shutil.copytree(SRC_DELIV, T)
    if L.gout(T, 'rev-parse', 'HEAD') != G or L.gout(T, 'rev-parse', 'refs/remotes/origin/fx/u1') != G:
        raise SystemExit('unexpected copy of the delivery repository')
    return T


def prepare(name, decision):
    L.ensure_dir(WORK)
    L.ensure_pins()
    T = copy_repo(name)
    p1 = L.p1(T, decision)
    p2 = L.p2(T, 'pwsh')
    info = {'T': T, 'Source': SRC_DELIV, 'Head': L.gout(T, 'rev-parse', 'HEAD'),
            'P1': {'ReturnedId': p1['ReturnedId'], 'IdEqualsPinned': p1['IdEqualsPinned'], 'OnlyNewLooseObject': p1['OnlyNewLooseObject'], 'Held': p1['Held']},
            'P2': {'Launch': p2['Launch'], 'FailedSteps': p2['FailedSteps']}}
    if not (p1['Held'] and p2['Launch']):
        raise SystemExit('P1/P2 failed on ' + T)
    return T, info


# ------------------------------------------------------------------ inputs of an invocation (copies of the real ones, RAE §10)
def write_inputs(T, inp, handoff_mut=None, handoff_present=True):
    """K.json and fixture-tests.trx byte-identical to round 2; deleg.json and handoff.json = the round-2 files with the
    worktree of this copy (ExpectedWorktree / Worktree), every other field unchanged; handoff_mut(dict) for nc1."""
    L.rmtree(inp)
    L.ensure_dir(inp)
    for n in ('K.json', 'fixture-tests.trx'):
        L.wbytes(inp + '/' + n, L.rbytes(R2_INP + '/' + n))
    deleg = json.loads(L.rbytes(R2_INP + '/deleg-deliv.json').decode('utf-8'))
    deleg['ExpectedWorktree'] = T
    jdump_local(inp + '/deleg.json', deleg)
    hand = json.loads(L.rbytes(R2_INP + '/handoff-deliv.json').decode('utf-8'))
    hand['Worktree'] = T
    if handoff_mut:
        handoff_mut(hand)
    if handoff_present:
        jdump_local(inp + '/handoff.json', hand)
    return deleg, hand


def file_arg(name, p):
    return ['--file', name + '=' + p, '--sha256', name + '=' + sha_file(p)]


def runs(T, inp, inv, names):
    k, d, h, t = inp + '/K.json', inp + '/deleg.json', inp + '/handoff.json', inp + '/fixture-tests.trx'
    allr = {
        'identity': lambda: ['identity', '--repo', T, '--branch', BR, '--base', TIP, '--current', G, '--red', RED],
        'diff': lambda: ['diff', '--repo', T, '--from', TIP, '--to', G],
        'scope': lambda: ['scope', '--repo', T, '--from', TIP, '--to', G] + file_arg('DELEG', d),
        'contract': lambda: ['contract', '--repo', T] + file_arg('CONTRACT', k) + file_arg('DELEG', d),
        'hashes': lambda: ['hashes', '--repo', T, '--rev', G, '--paths'] + sorted(DELIV_FILES),
        'cleantree': lambda: ['cleantree', '--repo', T, '--ignore-path', ignore_path(inv)],
        'handoff': lambda: ['handoff', '--repo', T] + file_arg('H', h) + ['--schema-rev', G],
        'freetext': lambda: ['freetext', '--repo', T] + file_arg('H', h) + ['--terms-rev', MAIN],
        'clausemap': lambda: ['clausemap', '--repo', T, '--main', MAIN, '--state-rev', G, '--state-path', STATE] + file_arg('CONTRACT', k) + ['--base', TIP],
        'trx': lambda: ['trx', '--repo', T] + file_arg('TRX', t) + ['--filter-substring', 'Subtract'],
        'read-text': lambda: ['read-text', '--repo', T, '--rev', G, '--path', DEC, '--lines', '1-1'],
        'read-json': lambda: ['read-json', '--repo', T, '--rev', G, '--path', K, '--pointer', '/Objective', '--pointer', '/Schema',
                              '--pointer', '/MainSha', '--pointer', '/AuthorityRevision'],
        'identity-nx': lambda: ['identity', '--repo', T, '--branch', BR, '--base', TIP, '--current', NX, '--red', RED],
    }
    return [(n, allr[n]()) for n in names]


def prim_reads(current, handoff_path=None, n4=False):
    if n4:
        return [('Handoff', "Test-Path '" + handoff_path + "'"), ('CleanTree', 'git status --porcelain'),
                ('Authority', 'git rev-parse %s:%s' % (MAIN, MAP))]
    return [('Identity', 'git rev-parse HEAD') if current == G else ('Identity', 'git cat-file -t ' + current),
            ('Scope', 'git diff --name-only --no-renames %s..%s' % (TIP, current)),
            ('CleanTree', 'git status --porcelain'),
            ('Trailer', 'git log --format=%%H %s..%s' % (TIP, current)),
            ('Authority', 'git rev-parse %s:%s' % (MAIN, MAP))]


# ------------------------------------------------------------------ effective input closure E of each invocation
def eic(task, run_d, run_h, inp, h_mode='pinned', strict=False, h_blob=None):
    """E = the custodied round-2 EIC (VERIFY of T1 on the delivery; or its I6-strict variant) with the three off-Git entries
    replaced by this invocation's copies: delegation and TRX always; the delivery per h_mode:
      'pinned' : at its path with the blob of inp/handoff.json (or h_blob);
      'absent' : no entry (N4 as the kit computes E: a missing file has no blob);
      'stale'  : at its path with the blob of the round-2 delivery, though inp/handoff.json does not exist (N4, sloppy E)."""
    E = json.loads(L.rbytes(R2_EIC_STRICT if strict else R2_EIC).decode('utf-8'))
    old_d, old_h = deliv_path('T1', D_RUN, 'output.json'), deliv_path('T1', H_RUN, 'worker-handoff.json')
    new_d, new_h = deliv_path(task, run_d, 'output.json'), deliv_path(task, run_h, 'worker-handoff.json')
    ci = []
    for e in E['CanonicalInputs']:
        if e['Path'] == old_d:
            ci.append({'Path': new_d, 'Blob': blob_file(inp + '/deleg.json')})
        elif e['Path'] == old_h:
            if h_mode == 'pinned':
                ci.append({'Path': new_h, 'Blob': h_blob or blob_file(inp + '/handoff.json')})
            elif h_mode == 'stale':
                ci.append({'Path': new_h, 'Blob': e['Blob']})
        elif e['Path'] == TRX_E:
            ci.append({'Path': TRX_E, 'Blob': blob_file(inp + '/fixture-tests.trx')})
        else:
            ci.append(e)
    E['CanonicalInputs'] = ci
    custody = {new_d: inp + '/deleg.json', new_h: inp + '/handoff.json', TRX_E: inp + '/fixture-tests.trx'}
    des = {'D': new_d, 'H': new_h, 'K': K}
    return E, custody, des


def vector(T, E_path, inv, des, custody):
    return {'Repo': T, 'E': E_path, 'I': inv, 'Designation': des, 'Custody': custody, 'MapPath': MAP}


def derive_to(work, label, vec):
    """Writes the vector, the derived closure and its derivation record; returns (closure path, record path, result)."""
    L.ensure_dir(work)
    vp = work + '/' + label + '.vector.json'
    jdump_local(vp, vec)
    res = DV.derive_vector(vec)
    cp = work + '/' + label + '.closure.json'
    jdump_local(cp, res['Closure'])
    rp = work + '/' + label + '.derivation.json'
    jdump_local(rp, {'Schema': 'a6r4-closure-derivation/1', 'Vector': vp, 'VectorSha256': sha_file(vp), 'Closure': cp,
                     'ClosureSha256': sha_file(cp), 'Omitted': res['Omitted'], 'Inputs': res['Inputs']})
    return cp, rp, res


def canon_closure(c):
    out = {}
    for k in ('Commits', 'Refs', 'Paths', 'Objects'):
        out[k] = sorted(c.get(k) or [])
    out['RevPaths'] = sorted((x['Rev'], x['Path']) for x in (c.get('RevPaths') or []))
    out['Files'] = sorted((x['Name'], x['Sha256']) for x in (c.get('Files') or []))
    out['Schema'] = c.get('Schema')
    return out


def closure_diff(given, derived):
    """{field: {'Extra': [...], 'Missing': [...]}} for every field where the given closure departs from the derived one."""
    a, b = canon_closure(given), canon_closure(derived)
    out = {}
    for k in a:
        if k == 'Schema':
            if a[k] != b[k]:
                out[k] = {'Given': a[k], 'Derived': b[k]}
            continue
        ea = [x for x in a[k] if x not in b[k]]
        mi = [x for x in b[k] if x not in a[k]]
        if ea or mi:
            out[k] = {'Extra': ea, 'Missing': mi}
    return out


def omitted_pairs(omitted):
    return sorted((o['Term'], o['Cause']) for o in omitted)
