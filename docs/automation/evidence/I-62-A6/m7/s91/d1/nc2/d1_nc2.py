# d1_nc2.py - nc2 (RAE §10) under the current protocol, without changing any pinned blob (I-62, A-6; decisiones §91, punto 3).
#
# Reuses the A6-7 preparation bench byte for byte (run_m7.py and its libraries, taken from the I-62 worktree by
# `git show HEAD:<path>` and checked against the SHA-256 of bench/README.md §1), with this folder as the work area. The
# instrument (blob 9a829b0e) runs only through the declared form in pwsh 7 (cwd = the copy), the session log is written in
# the Codex format and `Runs` is reconstructed with reconstruct_runs.py; the acceptor (blob 434ef1fc) runs read only on the
# same copy, with the zero-write and process checks of the bench. Controller outputs are synthetic (round-3/4 rules, as
# the bench): they are not the C-24 oracle, and the acceptor does not judge them.
#
# Two scenarios:
#   S0  the bench scenario (r2 part (d) / r4 / m7): verification of T1 on the simulated delivery (BASE 95bdc29d, G b417d36c);
#       diff = worker-notes.md, Calculator.cs, CalculatorTests.cs (ONE file under the evidence prefix).
#   S1  hypothetical delivery = S0 plus one commit G2 on top of G that adds a second evidence file under the same prefix
#       (docs/automation/evidence/FX-U1-agent/T1/worker-summary.md); written only in a copy of the delivery repository.
# In each scenario, the nc2 delegation is built with the bench's own reading of RAE §10 (run_m7.inputs_for 'NC2', checked
# for equality on S0) and two kit choices are run:
#   -A  the Controller runs the 12 orders of the verification, `contract` included (path b);
#   -C  the Controller runs the same orders except `contract` and computes A3-A5 itself with AP 16.5 and U-19 (path c).
# The real verification of the same scenario and kit choice is run too: it is the reference of nc2's relative oracle.
#
# Stdlib only. Writes only under D:\r62-a6m7\d1. Run (from pwsh, so that `git` resolves to the declared Git):
#   python -I d1_nc2.py
import copy, hashlib, importlib.util, json, os, re, shutil, subprocess, sys, time

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__)).replace('\\', '/')        # D:/r62-a6m7/d1/nc2
D1 = os.path.dirname(HERE)
LIB = D1 + '/lib'
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
WT_BENCH = 'docs/automation/evidence/I-62-A6/m7/bench/'
LIBS = {   # bench/README.md §1 (SHA-256 of the custodied copies)
    'derive_r4.py': '3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426',
    'lib_r1r4.py': 'c3995a1cd377822593837c6e34eff30425de61da73b1928d5a0460ce93bfa8c5',
    'lib_r3.py': '357a775123ec2bcb0dc9ae6323806d92e77716aaaa04a5bdc56b4fbe4701481c',
    'lib_r4.py': 'f1d94cc0eb2849bc69eaac90db374de2ef946aff6ce139bb9b396a1dfde8ab60',
    'reconstruct_runs.py': '5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da',
    'table_r3.py': 'ec4d9a8ec0f665f74660bf517526b0a1c097efe630feda1f83297c8661851c9b',
    'table_r4.py': 'e1efb54a7f9189c715780cfd130f2ea8540e70d9c5739adff3aa12a5bb5f2797',
    'make_eic_verify.py': '58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2',
    'designate_m7.py': '491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6',
    'table_m7.py': 'a0765733be3e140f01afe1818d3a1ff630e83cb1970acba22e593f322574c1e3',
    'lib_m7.py': '50e8c8957d37d63118a9fa41373488f8269e84909c91ada7bc0b8997106fe797',
    'run_m7.py': 'eee0a19e09559965c4315fc26cea7f6fc9a6d643b81d71caa1968e0c6f9623d0',
}
DECL_PY_SHA256 = 'ad169f4cb4bfb78c7a5c030a4529c19d6643276778e33994c93e145b6191c3ec'
DECL_GIT_SHA256 = '81ef35ae005ca9318018d18e3327578ce939fb99feaad6b2d7c8ab15f3de8db5'
EMAIL = re.compile(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}')


def _sha(b):
    return hashlib.sha256(b).hexdigest()


def setup_libs():
    os.makedirs(LIB, exist_ok=True)
    rec = {}
    for name, want in sorted(LIBS.items()):
        env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
        env['GIT_OPTIONAL_LOCKS'] = '0'
        p = subprocess.run(['git', '--no-replace-objects', '-C', WT, 'show', 'HEAD:' + WT_BENCH + name], capture_output=True,
                           stdin=subprocess.DEVNULL, env=env)
        blob = subprocess.run(['git', '-C', WT, 'rev-parse', 'HEAD:' + WT_BENCH + name], capture_output=True,
                              stdin=subprocess.DEVNULL, env=env).stdout.decode('ascii').strip()
        if p.returncode != 0 or _sha(p.stdout) != want:
            raise SystemExit('bench library not identical to the custodied copy: ' + name)
        dst = LIB + '/' + name
        if not os.path.isfile(dst) or open(dst, 'rb').read() != p.stdout:
            with open(dst, 'wb') as fh:
                fh.write(p.stdout)
        rec[name] = {'Sha256': want, 'Blob': blob}
    return rec


LIBREC = setup_libs()


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(LIB, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


RM = _load('run_m7')                       # loads lib_m7 -> lib_r4 -> lib_r3 -> lib_r1r4, reconstruct_runs, table_r3, ...
M = RM.M
X, L, Y, TB, DV, MK, DS, TM = M.X, M.L, M.Y, M.TB, M.DV, M.MK, M.DS, M.TM
# ---- work area of this run (every library global that names a path)
M.BENCH = HERE + '/w'
M.WORK = M.BENCH + '/work'
Y.WORK = M.BENCH
X.WORK = M.WORK
L.WORK = M.WORK
L.PIN = M.WORK + '/pin'
L.PIN_TOOL = L.PIN + '/bmin_facts.py'
L.PIN_ACC = L.PIN + '/bmin_accept.py'
L.BIN = M.WORK + '/bin'
L.HERE = HERE
M.INPUTS = HERE + '/inputs'
M.DESIG = HERE + '/designations'
RM.W = M.WORK
W = M.WORK
DECISION = 'BANCO-d1 (sin disposicion: analisis de D-1 y nc2, decisiones §91 punto 3; no es una DecisionRef normativa)'

TIP, RED, G, MAIN = M.TIP, M.RED, M.G, M.MAIN
EVID = 'docs/automation/evidence/FX-U1-agent/'
EXTRA = EVID + 'T1/worker-summary.md'
EXTRA_TEXT = (b'# Notas adicionales del Worker (escenario d1, sintetico)\n\n'
              b'Segundo archivo de evidencia bajo el prefijo de evidencia del contrato T1, para el analisis de D-1 y nc2.\n')
# RunIds of the invocations of this run (RunId form of AP 16.6: 4 hex digits); V and NC2 keep the bench's RunIds
M.RUN.update({'VC': 'R20261017T000000Z-d1c0', 'NC2C': 'R20261017T000000Z-d1c2', 'VX': 'R20261017T000000Z-d1e0',
              'VXC': 'R20261017T000000Z-d1e1', 'NC2X': 'R20261017T000000Z-d1e2', 'NC2XC': 'R20261017T000000Z-d1e3'})
M.TASKDIR.update({'VC': 'T1', 'NC2C': 'T1-nc2', 'VX': 'T1', 'VXC': 'T1', 'NC2X': 'T1-nc2', 'NC2XC': 'T1-nc2'})
ORDER = X.ORDER
NAMES_A = list(M.NAMES_FULL)                                   # the 12 orders of the bench verification
NAMES_C = [n for n in NAMES_A if n != 'contract']              # kit choice without `contract` (11 orders)


# ------------------------------------------------------------------ helpers
def diff_paths(T, a, b):
    return sorted(x for x in L.gout(T, '--no-replace-objects', 'diff', '--name-only', '--no-renames', a + '..' + b).split('\n') if x)


def nc2_allowed(allowed, diff):
    """RAE §10 nc2 with the reading of the m7 bench (run_m7.inputs_for 'NC2', same algorithm, diff as a parameter): exclude
    the first diff file in lexicographic order; the entry that covers it (exact, or '/**' prefix) is replaced by the
    enumeration of the other diff files not already in the list."""
    first, rest = diff[0], diff[1:]
    out = []
    for e in allowed:
        core = e[:-2] if e.endswith('/**') else e
        if e == first or (e.endswith('/**') and first.startswith(core)):
            out += [x for x in rest if x not in allowed and x not in out]
        else:
            out.append(e)
    return out, first


def u19(e):
    return e[:-2] if e.endswith('/**') else e


def covers(entry, path):
    return path == entry or (entry.endswith('/') and path.startswith(entry))


def contained(a, b):
    """AP 16.5 L553-L554 with U-19 (decisiones §62, U-19) applied to both entries; written here, not imported."""
    return covers(u19(b), u19(a))


LEVEL = {'NOT_REQUIRED': 0, 'PREFERRED': 1, 'REQUIRED': 2}


def contract_by_controller(K, D):
    """A3-A5 of AP 16.5 with the 16.22 additions (A3'-A5'), computed by the (simulated) Controller from the primary reads of
    CONTRACT and DELEG, U-19 on both entries of every containment. Returns (ok, detail)."""
    a3 = [{'Entry': a, 'ContainedBy': [b for b in K['AllowedWriteScope'] if contained(a, b)]} for a in D['AllowedWriteScope']]
    a4 = [{'Entry': a, 'ContainedBy': [b for b in D['ForbiddenWriteScope'] if contained(a, b)]} for a in K['ForbiddenWriteScope']]
    inv = [x for x in K['Invariants'] if x not in D['Invariants']]
    stop = [x['Id'] for x in K['StopConditions'] if x['Id'] not in set(y['Id'] for y in D['StopConditions'])]
    key = lambda x: (x['Path'], x['Section'], x['Class'])
    auth = [key(x) for x in K['Authorities'] if key(x) not in set(key(y) for y in D['Authorities'])]
    tests = []
    for t in K['RequiredTests']:
        m = [x for x in D['RequiredTests'] if x.get('Filter') == t['Filter']]
        if len(m) != 1 or not (m[0].get('MinSelected', -1) >= t['MinSelected']) or m[0].get('ExpectRed') != t['ExpectRed']:
            tests.append(t['Filter'])
    roles = []
    for r in K['RoleRequirements']:
        m = [x for x in D['RoleRequirements'] if x.get('Role') == r['Role'] and x.get('Action') == r.get('Action')]
        ok = len(m) == 1 and set(r.get('Mandatory') or []) <= set(m[0].get('Mandatory') or [])
        if ok:
            for ind in r.get('Independence') or []:
                n = [y for y in (m[0].get('Independence') or []) if y.get('ReferenceRole') == ind.get('ReferenceRole')]
                ok = ok and len(n) == 1 and all(LEVEL.get(n[0].get(dim), -1) >= LEVEL.get(lv, 99)
                                                for dim, lv in ind.items() if dim != 'ReferenceRole')
        if not ok:
            roles.append('%s/%s' % (r['Role'], r.get('Action')))
    sup = json.dumps(K['SupersededCommits'], sort_keys=True) == json.dumps(D['SupersededCommits'], sort_keys=True)
    ok = (all(x['ContainedBy'] for x in a3) and all(x['ContainedBy'] for x in a4) and not inv and not stop and not auth
          and not tests and not roles and sup)
    return ok, {'A3': a3, 'A4': a4, 'A5Missing': {'Invariants': inv, 'StopConditions': stop, 'Authorities': [list(x) for x in auth],
                                                  'RequiredTests': tests}, 'RoleRequirementsMissing': roles, 'SupersededEqual': sup}


def rjson(p):
    return json.loads(L.rbytes(p).decode('utf-8'))


def cv_rely(F, deleg, hand, prim, run_id):
    """Path (b1): the Controller relies on the instrument facts (cv_t1 of round 3: Contract from the `contract` facts)."""
    return M.cv_case(F, deleg, hand, prim, run_id)


def cv_contrast(F, deleg, hand, prim, run_id, K):
    """Path (b2): the Controller runs `contract`, cites it, contrasts it with its own reading of 16.5 and U-19 and keeps its
    own result for Contract. The instrument facts are still the ones in the session log."""
    cv = M.cv_case(F, deleg, hand, prim, run_id)
    ok, det = contract_by_controller(K, deleg)
    tool = F['contract'][0]
    diff = [x['Entry'] for x, y in zip(tool['A3'], det['A3']) if bool(x['ContainedBy']) != bool(y['ContainedBy'])]
    diff += [x['Entry'] for x, y in zip(tool['A4'], det['A4']) if bool(x['ContainedBy']) != bool(y['ContainedBy'])]
    cv['Checks']['Contract'] = {'Result': 'pass' if ok else 'fail',
                                'Evidence': X._ev(F, 'contract', 'contrastado con la lectura de CONTRACT y DELEG con AP 16.5 y U-19 en las dos '
                                                  'entradas: el instrumento no contiene %s; el Controller conserva su propio resultado' % (diff or 'nada'))}
    M.reclassify(cv)
    cv['Findings'] = list(cv['Findings']) + ['Salida sintetica d1 (b2): Contract por la lectura propia del Controller, distinta de los hechos `contract`.']
    return cv


def cv_without_contract(F, deleg, hand, prim, run_id, K, kp, dp):
    """Path (c): no `contract` order; Contract computed by the Controller with 16.5 and U-19 from the primary reads; no citation
    of any instrument order for Contract."""
    ok, det = contract_by_controller(K, deleg)
    F2 = dict(F)
    F2['contract'] = (dict(det, A5Missing=dict(det['A5Missing'], RoleRequirements=det['RoleRequirementsMissing'])), '-', ['contract'], None)
    cv = M.cv_case(F2, deleg, hand, prim, run_id)
    cv['Checks']['Contract'] = {'Result': 'pass' if ok else 'fail',
                                'Evidence': 'sin instrumento: A3-A5 (con A3\'-A5\' de 16.22) calculadas por el Controller con AP 16.5 y U-19 '
                                            'sobre la lectura primaria de CONTRACT (sha256 %s) y DELEG (sha256 %s); A3 sin contener: %s; A4 sin '
                                            'contener: %s' % (M.sha_file(kp), M.sha_file(dp), [x['Entry'] for x in det['A3'] if not x['ContainedBy']],
                                                              [x['Entry'] for x in det['A4'] if not x['ContainedBy']])}
    M.reclassify(cv)
    cv['Findings'] = list(cv['Findings']) + ['Salida sintetica d1 (c): sin la orden contract; Contract por el Controller sin el instrumento.']
    return cv


def results_of(cv):
    return dict((k, cv['Checks'][k]['Result']) for k in ORDER)


def oracle(v_res, n_res, mutated='Scope'):
    """RAE §10 relative oracle: the mutated check in fail, FailureClass = that check, and every earlier check (AP 16.9 order)
    with the same Result as in the real verification. The bench verification is not VERIFIED (Termination not_run and
    Authority fail are bench values), so FailureClass is read relatively: the first check whose Result differs from the
    real verification (= the FailureClass the control would have if the real verification were VERIFIED)."""
    before = ORDER[:ORDER.index(mutated)]
    differ = [c for c in before if v_res[c] != n_res[c]]
    first_diff = next((c for c in ORDER if v_res[c] != n_res[c]), None)
    return {'MutatedFail': n_res[mutated] == 'fail', 'EarlierDiffer': differ, 'RelativeFailureClass': first_diff,
            'Holds': n_res[mutated] == 'fail' and not differ and first_diff == mutated}


def clean(s):
    return L.sanitize_str(s)


def keep(rel, src):
    """Byte copy of a work file into out/ when it needs no sanitizing; otherwise a sanitized copy (recorded)."""
    b = L.rbytes(src)
    t = b.decode('utf-8', 'replace')
    dst = HERE + '/out/' + rel
    L.ensure_dir(os.path.dirname(dst))
    if clean(t) == t:
        L.wbytes(dst, b)
        return {'Path': 'out/' + rel, 'Sha256': _sha(b), 'Sanitized': False}
    s = clean(t).encode('utf-8')
    L.wbytes(dst, s)
    return {'Path': 'out/' + rel, 'Sha256': _sha(s), 'Sanitized': True, 'SourceSha256': _sha(b)}


# ------------------------------------------------------------------ one invocation
def invocation(group, key, T, inp, kit, current, runs_fn, prim, cv_fns, label):
    """Designation + E (R7-A) + derived closure; one session with the kit's orders; one request and acceptor run per
    Controller output. Returns the record."""
    work = W + '/s-' + label
    L.ensure_dir(work)
    inv = M.invocation(key, target=current)
    des = M.designation(key, T, inp, inv)
    Ep, E, rep = M.build_e(des, 'R7-A', W + '/build', label)
    dres = M.derive(des, Ep, W + '/build', label)
    if not dres['Derived']:
        raise SystemExit('derivation failed for ' + label + ': ' + json.dumps(dres['Check']['Problems']))
    sv = {'Closure': dres['ClosurePath'], 'Vector': dres['VectorPath'], 'Record': dres['RecordPath'],
          'DesignationPath': dres['DesignationPath']}
    names = NAMES_A if kit == 'A' else NAMES_C
    rs = runs_fn(T, inp, inv, names, current)
    t0 = time.time()
    sess, execs, rec, rruns = M.session('s-' + label + '-log', T, rs, prim)
    F = X.facts_by_cmd(rruns)
    deleg, hand = rjson(inp + '/deleg.json'), rjson(inp + '/handoff.json')
    K = rjson(inp + '/K.json')
    run_id = inv['RunId']
    envs = []
    for r in rruns:
        b = L.rbytes(r['Envelope'])
        envs.append({'Name': r['Name'], 'Sha256': _sha(b), 'HasEmail': bool(EMAIL.search(b.decode('ascii', 'replace')))})
    out = {'Group': group, 'Label': label, 'Key': key, 'Kit': kit, 'RunId': run_id, 'Clone': T, 'Current': current,
           'Orders': names, 'Derivation': M.brief(dres), 'Session': M.sess_brief(sess), 'Reconstruction': M.rec_brief(rec),
           'Envelopes': envs, 'Outputs': {}, 'Files': {}}
    out['Session'].pop('LogPath', None)
    if 'contract' in F:
        out['ContractFacts'] = {'A3': F['contract'][0]['A3'], 'A4': F['contract'][0]['A4'], 'FactsSha256': F['contract'][1]}
        env_path = [r['Envelope'] for r in rruns if r['Argv'][0] == 'contract'][0]
        out['Files']['ContractEnvelope'] = keep(label + '/contract-envelope.json', env_path)
    ok, det = contract_by_controller(K, deleg)
    out['ContractByU19'] = {'Ok': ok, 'A3': det['A3'], 'A4': det['A4']}
    for tag, fn in cv_fns:
        cv = fn(F, deleg, hand, sess['Primary'], run_id)
        cvp = work + '/controller-output-' + tag + '.json'
        L.wjson_local(cvp, cv)
        relayp, rsv = RM.relay_save(work, 'relay-facts-' + tag + '.json', sess, cvp, run_id)
        s = {'Inp': inp, 'Inv': inv}
        o, reqp, dec = RM.verify_request(tag, T, s, sv, rruns, rec, cvp, relayp, work)
        r7 = None
        res = json.loads(L.rbytes(reqp[:-5] + '.acceptor-stdout.json').decode('ascii'))
        for c in res.get('Checks', []):
            if c.get('Id') == 'R7':
                r7 = c
        out['Outputs'][tag] = {
            'ControllerResults': results_of(cv), 'Classification': [cv['Classification'], cv['Disposition'], cv['FailureClass']],
            'ContractEvidence': cv['Checks']['Contract']['Evidence'][:400],
            'SchemaValid': X.validate('controller-verification.v2.schema.json', cv),
            'Acceptor': {'Rc': o['Rc'], 'Decision': o['Decision'], 'Checks': o['Checks'], 'MismatchDetails': o['MismatchDetails'],
                         'NotApplicable': sorted(k for k, v in o['Checks'].items() if v == 'NOT_APPLICABLE'),
                         'R9OutsideClosure': o['R9Runs'], 'ZeroWrite': o['ZeroWrite'], 'ProcessTreeDead': o['ProcessTreeDead'],
                         'RequestSha256': o['RequestSha256'], 'StdoutSha256': o['AcceptorStdoutSha256']},
            'R7Detail': r7, 'VerificationIncluded': dec['Include'], 'Table': o['Table'],
            'AcceptedA65r2': RM.ok_accept(o, 'MATCH' if dec['Include'] else 'NOT_APPLICABLE')}
        out['Files'][tag] = {'Controller': keep(label + '/controller-output-' + tag + '.json', cvp),
                             'Request': keep(label + '/request-' + tag + '.json', reqp),
                             'AcceptorStdout': keep(label + '/acceptor-stdout-' + tag + '.json', reqp[:-5] + '.acceptor-stdout.json')}
        print('%-6s %-10s %-4s rc=%s %s R7=%s accepted=%s contract=%s scope=%s' % (
            group, label, tag, o['Rc'], o['Decision'], o['Checks'].get('R7'), out['Outputs'][tag]['AcceptedA65r2'],
            cv['Checks']['Contract']['Result'], cv['Checks']['Scope']['Result']), flush=True)
    out['Seconds'] = round(time.time() - t0, 1)
    return out


# ------------------------------------------------------------------ orders of S1 (the bench's M.runs with CurrentSha everywhere)
def runs_s1(T, inp, inv, names, current):
    k, d, h, t = inp + '/K.json', inp + '/deleg.json', inp + '/handoff.json', inp + '/fixture-tests.trx'
    files = diff_paths(T, TIP, current)
    fa = M.file_arg
    allr = {
        'identity': ['identity', '--repo', T, '--branch', Y.BR, '--base', TIP, '--current', current, '--red', RED],
        'diff': ['diff', '--repo', T, '--from', TIP, '--to', current],
        'scope': ['scope', '--repo', T, '--from', TIP, '--to', current] + fa('DELEG', d),
        'contract': ['contract', '--repo', T] + fa('CONTRACT', k) + fa('DELEG', d),
        'hashes': ['hashes', '--repo', T, '--rev', current, '--paths'] + files,
        'cleantree': ['cleantree', '--repo', T, '--ignore-path', M.ignore_path(inv)],
        'handoff': ['handoff', '--repo', T] + fa('H', h) + ['--schema-rev', current],
        'freetext': ['freetext', '--repo', T] + fa('H', h) + ['--terms-rev', MAIN],
        'clausemap': ['clausemap', '--repo', T, '--main', MAIN, '--state-rev', current, '--state-path', Y.STATE] + fa('CONTRACT', k) + ['--base', TIP],
        'trx': ['trx', '--repo', T] + fa('TRX', t) + ['--filter-substring', 'Subtract'],
        'read-text': ['read-text', '--repo', T, '--rev', current, '--path', Y.DEC, '--lines', '1-1'],
        'read-json': ['read-json', '--repo', T, '--rev', current, '--path', Y.K, '--pointer', '/Objective', '--pointer', '/Schema',
                      '--pointer', '/MainSha', '--pointer', '/AuthorityRevision'],
    }
    return [(n, allr[n]) for n in names]


def runs_s0(T, inp, inv, names, current):
    return M.runs(T, inp, inv, names, current=current)


def prim_s1(current):
    return [('Identity', 'git rev-parse HEAD'), ('Scope', 'git diff --name-only --no-renames %s..%s' % (TIP, current)),
            ('CleanTree', 'git status --porcelain'), ('Trailer', 'git log --format=%%H %s..%s' % (TIP, current)),
            ('Authority', 'git rev-parse %s:%s' % (MAIN, Y.MAP))]


# ------------------------------------------------------------------ scenario S1: the copy with a second evidence file
def make_s1():
    T = M.BENCH + '/c-x'
    L.rmtree(T)
    shutil.copytree(Y.SRC_DELIV, T)
    if L.gout(T, 'rev-parse', 'HEAD') != G or L.gout(T, 'rev-parse', 'refs/remotes/origin/fx/u1') != G or L.gout(T, 'status', '--porcelain'):
        raise SystemExit('unexpected copy of the delivery repository')
    blob = L.git(T, 'hash-object', '-w', '--no-filters', '--stdin', inp=EXTRA_TEXT).stdout.decode('ascii').strip()
    tmp = T + '/.git/d1-tmp-index'
    ienv = L.genv({'GIT_INDEX_FILE': tmp})
    for args in (('read-tree', G), ('update-index', '--add', '--cacheinfo', '100644,%s,%s' % (blob, EXTRA))):
        if L.git(T, *args, env=ienv).returncode != 0:
            raise SystemExit('index step failed: ' + args[0])
    tree = L.git(T, 'write-tree', env=ienv).stdout.decode('ascii').strip()
    os.remove(tmp)
    who = dict((k, L.gout(T, 'log', '-1', '--format=%' + f, G)) for k, f in (('an', 'an'), ('ae', 'ae'), ('cn', 'cn'), ('ce', 'ce')))
    cenv = L.genv({'GIT_AUTHOR_NAME': who['an'], 'GIT_AUTHOR_EMAIL': who['ae'], 'GIT_AUTHOR_DATE': '2025-10-11T00:03:00+0000',
                   'GIT_COMMITTER_NAME': who['cn'], 'GIT_COMMITTER_EMAIL': who['ce'], 'GIT_COMMITTER_DATE': '2025-10-11T00:03:00+0000'})
    msg = b'FX-U1 T1: notas adicionales de evidencia (escenario d1, sintetico)\n\nCo-Authored-By: R3 Synthetic Worker (FX-U1 T1)\n'
    g2 = L.git(T, 'commit-tree', tree, '-p', G, inp=msg, env=cenv).stdout.decode('ascii').strip()
    for ref in ('refs/heads/fx/u1', 'refs/remotes/origin/fx/u1'):
        if L.git(T, 'update-ref', ref, g2, G).returncode != 0:
            raise SystemExit('update-ref failed: ' + ref)
    L.git(T, 'reset', '-q', '--hard', g2)
    st = {'Head': L.gout(T, 'rev-parse', 'HEAD'), 'OriginBranch': L.gout(T, 'rev-parse', 'refs/remotes/origin/fx/u1'),
          'StatusPorcelain': L.gout(T, 'status', '--porcelain'), 'G2': g2, 'Parent': L.gout(T, 'rev-parse', g2 + '^'),
          'ExtraPath': EXTRA, 'ExtraBlob': blob, 'Diff': diff_paths(T, TIP, g2),
          'Trailer': L.gout(T, 'log', '-1', '--format=%(trailers:key=Co-Authored-By,valueonly)', g2)}
    if not (st['Head'] == g2 == st['OriginBranch'] and st['StatusPorcelain'] == '' and st['Parent'] == G):
        raise SystemExit('S1 copy not in the expected state: ' + json.dumps(st))
    p1 = L.p1(T, DECISION)
    p2 = L.p2(T, 'pwsh')
    st['P1'] = {'ReturnedId': p1['ReturnedId'], 'IdEqualsPinned': p1['IdEqualsPinned'], 'OnlyNewLooseObject': p1['OnlyNewLooseObject'], 'Held': p1['Held']}
    st['P2'] = {'Launch': p2['Launch'], 'FailedSteps': p2['FailedSteps']}
    if not (p1['Held'] and p2['Launch']):
        raise SystemExit('P1/P2 failed on ' + T)
    return T, g2, st


# ------------------------------------------------------------------ main
def environment():
    xsha, src = L.exe_sha256()
    git_exe = shutil.which('git')
    gsha = _sha(L.rbytes(git_exe))
    rec = {'Python': {'Version': sys.version.split()[0], 'ExecutableSha256': xsha, 'Source': src, 'EqualsDeclared': xsha == DECL_PY_SHA256},
           'Git': {'Sha256': gsha, 'EqualsDeclared': gsha == DECL_GIT_SHA256, 'DeclaredFormGitIsThisOne': os.path.normcase(os.path.normpath(git_exe)) == os.path.normcase(X.GIT_EXE)},
           'Versions': L.versions()}
    if not (rec['Python']['EqualsDeclared'] and rec['Git']['EqualsDeclared'] and rec['Git']['DeclaredFormGitIsThisOne']):
        raise SystemExit('interpreter or Git differ from the declared ones')
    return rec


def main():
    if sys.flags.isolated != 1:
        raise SystemExit('run with python -I')
    t_start = time.time()
    L.rmtree(M.BENCH)
    L.rmtree(HERE + '/out')
    L.ensure_dir(W)
    result = {'Schema': 'rackcad-i62-d1-nc2/0-experimental', 'Libraries': LIBREC, 'Environment': environment(), 'Scenarios': {}}
    pins = L.ensure_pins()
    result['Pins'] = dict((k, {'Blob': v['Blob'], 'Sha256': v['Sha256']}) for k, v in pins.items())

    # ---------------- S0: the bench scenario
    T0, info0 = M.prepare('v')
    diff0 = diff_paths(T0, TIP, G)
    inp_v, inp_n = W + '/in-s0-v', W + '/in-s0-nc2'
    d_v, h_v, _, _ = RM.inputs_for('V', T0, inp_v)
    d_n, h_n, _, mut = RM.inputs_for('NC2', T0, inp_n)
    mine, first0 = nc2_allowed(d_v['AllowedWriteScope'], diff0)
    s0 = {'Clone': T0, 'P1': info0['P1'], 'P2': info0['P2'], 'Diff': diff0, 'FirstDiffFile': first0,
          'RealAllowed': d_v['AllowedWriteScope'], 'Nc2Allowed': d_n['AllowedWriteScope'],
          'Nc2AllowedThisReading': mine, 'BenchMutationEqualsThisReading': mine == d_n['AllowedWriteScope'],
          'Nc2DelegationDiffersOnlyIn': sorted(k for k in set(d_v) | set(d_n) if d_v.get(k) != d_n.get(k)),
          'Invocations': {}}
    if not s0['BenchMutationEqualsThisReading']:
        raise SystemExit('nc2 reading differs from the bench mutation on S0')
    K0 = rjson(inp_v + '/K.json')
    kp0, dpn0 = inp_v + '/K.json', inp_n + '/deleg.json'
    prim0 = Y.prim_reads(G)
    s0['Invocations']['V-A'] = invocation('S0', 'V', T0, inp_v, 'A', G, runs_s0, prim0, [('b1', cv_rely)], 's0-v-a')
    s0['Invocations']['NC2-A'] = invocation('S0', 'NC2', T0, inp_n, 'A', G, runs_s0, prim0,
                                            [('b1', cv_rely), ('b2', lambda F, d, h, p, r: cv_contrast(F, d, h, p, r, K0))], 's0-nc2-a')
    s0['Invocations']['V-C'] = invocation('S0', 'VC', T0, inp_v, 'C', G, runs_s0, prim0,
                                          [('c', lambda F, d, h, p, r: cv_without_contract(F, d, h, p, r, K0, kp0, inp_v + '/deleg.json'))], 's0-v-c')
    s0['Invocations']['NC2-C'] = invocation('S0', 'NC2C', T0, inp_n, 'C', G, runs_s0, prim0,
                                            [('c', lambda F, d, h, p, r: cv_without_contract(F, d, h, p, r, K0, kp0, dpn0))], 's0-nc2-c')
    result['Scenarios']['S0'] = s0

    # ---------------- S1: a second evidence file under the prefix
    T1, g2, st1 = make_s1()
    diff1 = st1['Diff']
    inp_vx, inp_nx = W + '/in-s1-v', W + '/in-s1-nc2'
    hmut = lambda h: h.update(CurrentSha=g2, FilesChanged=list(diff1))
    d_vx, h_vx = Y.write_inputs(T1, inp_vx, handoff_mut=hmut)
    nx_allowed, first1 = nc2_allowed(d_vx['AllowedWriteScope'], diff1)
    d_nx, h_nx = M.write_inputs(T1, inp_nx, hand_mut=hmut, deleg_mut=lambda dd: dd.update(AllowedWriteScope=nx_allowed))
    s1 = dict(st1, Clone=T1, FirstDiffFile=first1, RealAllowed=d_vx['AllowedWriteScope'], Nc2Allowed=d_nx['AllowedWriteScope'],
              Nc2DelegationDiffersOnlyIn=sorted(k for k in set(d_vx) | set(d_nx) if d_vx.get(k) != d_nx.get(k)),
              HandoffDiffersFromS0In=sorted(k for k in set(h_v) | set(h_vx) if h_v.get(k) != h_vx.get(k)), Invocations={})
    K1 = rjson(inp_vx + '/K.json')
    kp1 = inp_vx + '/K.json'
    prim1 = prim_s1(g2)
    s1['Invocations']['V-A'] = invocation('S1', 'VX', T1, inp_vx, 'A', g2, runs_s1, prim1, [('b1', cv_rely)], 's1-v-a')
    s1['Invocations']['NC2-A'] = invocation('S1', 'NC2X', T1, inp_nx, 'A', g2, runs_s1, prim1,
                                            [('b1', cv_rely), ('b2', lambda F, d, h, p, r: cv_contrast(F, d, h, p, r, K1))], 's1-nc2-a')
    s1['Invocations']['V-C'] = invocation('S1', 'VXC', T1, inp_vx, 'C', g2, runs_s1, prim1,
                                          [('c', lambda F, d, h, p, r: cv_without_contract(F, d, h, p, r, K1, kp1, inp_vx + '/deleg.json'))], 's1-v-c')
    s1['Invocations']['NC2-C'] = invocation('S1', 'NC2XC', T1, inp_nx, 'C', g2, runs_s1, prim1,
                                            [('c', lambda F, d, h, p, r: cv_without_contract(F, d, h, p, r, K1, kp1, inp_nx + '/deleg.json'))], 's1-nc2-c')
    result['Scenarios']['S1'] = s1

    # ---------------- oracle of nc2 (relative to the real verification of the same scenario and kit choice)
    orc = {}
    for sn, sc in (('S0', s0), ('S1', s1)):
        for kit, vtag, ntags in (('A', 'b1', ('b1', 'b2')), ('C', 'c', ('c',))):
            v = sc['Invocations']['V-' + kit]['Outputs'][vtag]
            for nt in ntags:
                n = sc['Invocations']['NC2-' + kit]['Outputs'][nt]
                o = oracle(v['ControllerResults'], n['ControllerResults'])
                o.update({'VerificationAccepted': v['AcceptedA65r2'], 'ControlAccepted': n['AcceptedA65r2'],
                          'ControlDecision': n['Acceptor']['Decision'], 'ControlR7': n['Acceptor']['Checks'].get('R7'),
                          'ControlPassable': o['Holds'] and n['AcceptedA65r2']})
                orc['%s/%s/%s' % (sn, kit, nt)] = o
    result['Oracle'] = orc
    # inputs (copies, hygiene-checked)
    files = {}
    for tag, inp in (('s0-v', inp_v), ('s0-nc2', inp_n), ('s1-v', inp_vx), ('s1-nc2', inp_nx)):
        for f in ('deleg.json', 'handoff.json'):
            files['%s/%s' % (tag, f)] = keep('inputs/%s/%s' % (tag, f), inp + '/' + f)
    files['K.json'] = keep('inputs/K.json', inp_v + '/K.json')
    result['InputFiles'] = files
    result['Seconds'] = round(time.time() - t_start, 1)
    data = (json.dumps(L.sanitize(result), ensure_ascii=True, sort_keys=True, indent=1) + '\n').encode('ascii')
    L.wbytes(HERE + '/nc2-result.json', data)
    print('wrote nc2-result.json', _sha(data), 'seconds', result['Seconds'])
    for k, o in sorted(orc.items()):
        print(k, 'oracle=%s earlierDiffer=%s relFC=%s accepted=%s decision=%s R7=%s' % (
            o['Holds'], o['EarlierDiffer'], o['RelativeFailureClass'], o['ControlAccepted'], o['ControlDecision'], o['ControlR7']))


if __name__ == '__main__':
    main()
