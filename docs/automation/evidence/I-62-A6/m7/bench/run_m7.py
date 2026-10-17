# run_m7.py - supervision-side bench for the preparation of the A6-7 measurement (I-62, A-6 AGREED, decisiones §90 points 2
# and 3): closure of VERIFY (A6-7 rule 7) in two variants, designation of D/H/K by the kit's input paths (O3) with fail-closed
# checks (O2), static tests of N5-N7 (O6), the measurement invocation M, and the declared interpreter and Git.
#   python -I run_m7.py all | env | prep | build | v | n4 | nc1 | n5 | n6 | n7 | m | summary | readme
# Sequential. Work area D:\r62-a6m7\bench (copies c-*, sessions work/). No model, no model CLI, no network, no gh; nothing
# under %USERPROFILE%\.codex is read except the I-62 worktree through git (blobs); the Codex runtime pwsh is only hashed.
import ctypes, importlib.util, json, os, platform, re, shutil, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


M = _load('lib_m7')
X, L, Y, TB, DV, MK, DS, TM = M.X, M.L, M.Y, M.TB, M.DV, M.MK, M.DS, M.TM
W = M.WORK
VARIANTS = ('R7-A', 'R7-B')
CASES = ('V', 'N4', 'NC1', 'NC2', 'NC3', 'N5', 'N6', 'N7A', 'N7AR-OWN', 'N7AR-SAME', 'N7B', 'M')
COPY = {'V': 'v', 'N4': 'n4', 'NC1': 'nc1', 'NC2': 'v', 'NC3': 'v', 'N5': 'n5', 'N6': 'n6', 'N7A': 'n7', 'N7AR-OWN': 'n7',
        'N7AR-SAME': 'n7', 'N7B': 'n7', 'M': 'm'}


# ------------------------------------------------------------------ helpers
def case(group, name, what, expected, obs, holds, **extra):
    d = {'Group': group, 'Case': name, 'What': what, 'Expected': expected, 'Observed': obs, 'Holds': bool(holds)}
    d.update(extra)
    brief = (obs.get('Decision'), mm_of(obs)) if isinstance(obs, dict) and 'Checks' in obs else ''
    print('%-7s %-26s holds=%s %s' % (group, name, bool(holds), brief), flush=True)
    return d


def mm_of(obs):
    return sorted(k for k, v in (obs.get('Checks') or {}).items() if v == 'MISMATCH')


def na_of(obs):
    return sorted(k for k, v in (obs.get('Checks') or {}).items() if v == 'NOT_APPLICABLE')


def save_state(name, obj):
    L.wbytes(W + '/state-' + name + '.json', (json.dumps(obj, ensure_ascii=True, indent=1, sort_keys=True) + '\n').encode('ascii'))


def load_state(name):
    return json.loads(L.rbytes(W + '/state-' + name + '.json').decode('ascii'))


def write_group(name, obj):
    p, h = L.write_result(name, obj)
    print('wrote', p, h, flush=True)


def r9_runs(acc_result):
    for c in acc_result.get('Checks', []):
        if c.get('Id') == 'R9':
            d = c.get('Detail') or {}
            return dict((r.get('Name'), r.get('Violations')) for r in (d.get('Runs') or []) if isinstance(r, dict))
    return {}


def acc_obs(T, reqp, table):
    acc = L.run_acceptor(reqp, snap_repos=[T], proc_paths=[T])
    out = L.rbytes(acc['StdoutPath'])
    tab = table(acc['Rc'], out)
    pc = acc['Processes'] or {}
    r = acc['_Result']
    return {'Rc': acc['Rc'], 'Decision': acc['Decision'], 'Checks': acc['Checks'], 'Error': acc.get('Error'),
            'MismatchDetails': dict((rid, X.detail(r, rid)) for rid, st in acc['Checks'].items() if st == 'MISMATCH'),
            'NotApplicableDetails': dict((rid, X.detail(r, rid, 300)) for rid, st in acc['Checks'].items() if st == 'NOT_APPLICABLE'),
            'R9Runs': dict((k, v) for k, v in r9_runs(r).items() if v), 'R9RunsChecked': sorted(r9_runs(r).keys()),
            'RequestSha256': acc['RequestSha256'], 'AcceptorStdoutSha256': acc['StdoutSha256'],
            'ZeroWrite': all(z['Equal'] for z in acc['ZeroWrite']),
            'ProcessTreeDead': not pc.get('Matching') and not pc.get('ChildrenOfGivenParents') and 'Error' not in pc,
            'Table': tab}


def outside_cmds(obs):
    return sorted(set(k.split('-', 1)[1] for k in obs['R9Runs']))


def req_brief(p):
    q = json.loads(L.rbytes(p).decode('utf-8'))
    return {'Path': p, 'Sha256': M.sha_file(p), 'Runs': len(q.get('Runs') or []), 'VerificationIncluded': 'Verification' in q,
            'RelayFactsIncluded': 'RelayFacts' in q, 'NotApplicable': sorted((q.get('NotApplicable') or {}).keys()), 'Closure': q.get('Closure')}


def relay_save(work, name, sess, cvp, run_id):
    rr = X.relay_record(sess, cvp, run_id)
    p = work + '/' + name
    L.wjson_local(p, rr)
    return p, X.validate('relay-record.v2.schema.json', rr)


def deliver(case_, files):
    """Byte copies of the work files of one invocation into inputs/<case>/ (hygiene-checked)."""
    out = {}
    for src in files:
        if src and os.path.isfile(src):
            dst = M.copy_file_deliv(src, case_ + '/' + os.path.basename(src))
            out[os.path.basename(src)] = M.sha_file(dst)
    return out


def ok_accept(o, r4):
    return (o['Rc'] == 0 and o['Decision'] == 'ACCEPT_FACTS' and o['Checks'].get('R9') == 'MATCH' and o['Checks'].get('R4') == r4
            and not o['R9Runs'] and o['ZeroWrite'] and o['ProcessTreeDead'] and o['Table']['Accepted'])


# ------------------------------------------------------------------ group env (task 5)
def _sha(p):
    try:
        return L.sha256(L.rbytes(p)), os.path.getsize(p)
    except OSError as e:
        return 'unreadable: ' + type(e).__name__, None


def _image_path():
    buf = ctypes.create_unicode_buffer(32768)
    n = ctypes.windll.kernel32.GetModuleFileNameW(None, buf, 32768)
    return buf.value if n else None


def group_env():
    out = {'Schema': 'm7-bench/p0-env'}
    py_img = _image_path()
    xsha, src = L.exe_sha256()
    out['Python'] = {'DeclaredPath': X.PY_EXE, 'Version': platform.python_version(), 'SysVersion': sys.version,
                     'ExecutableSha256': xsha, 'ExecutableSha256Source': src, 'ProcessImage': py_img,
                     'ProcessImageSha256': _sha(py_img)[0] if py_img else None, 'Isolated': sys.flags.isolated == 1}
    gv = subprocess.run([X.GIT_EXE, '--version'], capture_output=True, stdin=subprocess.DEVNULL, env=L.genv())
    ge = subprocess.run([X.GIT_EXE, '--exec-path'], capture_output=True, stdin=subprocess.DEVNULL, env=L.genv())
    gs, gz = _sha(X.GIT_EXE)
    out['Git'] = {'DeclaredPath': X.GIT_EXE, 'Version': gv.stdout.decode('ascii', 'replace').strip(), 'Sha256': gs, 'Size': gz,
                  'ExecPath': ge.stdout.decode('utf-8', 'replace').strip()}
    pv = subprocess.run([L.PWSH, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command',
                         '$PSVersionTable.PSVersion.ToString(); (Get-Process -Id $PID).Path; $ExecutionContext.SessionState.LanguageMode'],
                        capture_output=True, stdin=subprocess.DEVNULL, env=L.genv())
    pl = pv.stdout.decode('utf-8', 'replace').split()
    out['PwshOfTheDeclaredForm'] = {'Path': L.PWSH, 'Version': pl[0] if pl else None, 'ProcessImage': ' '.join(pl[1:-1]) if len(pl) > 2 else None,
                                    'LanguageMode': pl[-1] if pl else None,
                                    'ProcessImageSha256': _sha(' '.join(pl[1:-1]))[0] if len(pl) > 2 else None,
                                    'IsCodexRuntimePwsh': False}
    rt_dir = os.path.join(os.environ['USERPROFILE'], '.cache', 'codex-runtimes', 'codex-primary-runtime', 'dependencies', 'native', 'powershell')
    rt = os.path.join(rt_dir, 'pwsh.exe')
    rs, rz = _sha(rt) if os.path.isfile(rt) else ('absent', None)
    out['CodexRuntimePwsh'] = {'Path': rt, 'Exists': os.path.isfile(rt), 'Sha256': rs, 'Size': rz, 'Executed': False}
    # resolution of `git` by name in a child environment whose PATH starts with the runtime pwsh directory
    host_path = os.environ.get('PATH', '')
    child_path = rt_dir + os.pathsep + host_path
    pathext = [e for e in (os.environ.get('PATHEXT') or '').split(';') if e]
    dirs = [d for d in child_path.split(os.pathsep) if d]
    hits = []
    for d in dirs:
        for ext in ['.ps1'] + pathext:
            p = os.path.join(d, 'git' + ext)
            if os.path.isfile(p):
                hits.append(p)
    rt_names = [os.path.join(rt_dir, 'git' + e) for e in ['', '.ps1'] + pathext + ['.exe'] if os.path.exists(os.path.join(rt_dir, 'git' + e))]
    out['ResolutionPathOrder'] = {'Rule': 'pwsh: entries of PATH in order; in each, git + (.ps1 and PATHEXT); first file found',
                                  'ChildPathFirstEntry': rt_dir, 'RuntimeDirGitCandidates': rt_names, 'FirstHit': hits[0] if hits else None,
                                  'Hits': hits[:6], 'FirstHitIsDeclaredGit': bool(hits) and os.path.normcase(hits[0]) == os.path.normcase(X.GIT_EXE)}
    win = os.environ.get('SystemRoot') or r'C:\Windows'
    cp_dirs = [os.path.dirname(py_img) if py_img else None, '<cwd = -C, unless NoDefaultCurrentDirectoryInExePath>',
               os.path.join(win, 'System32'), os.path.join(win, 'System'), win] + dirs
    cp_hits = []
    for d in cp_dirs:
        if d and not d.startswith('<') and os.path.isfile(os.path.join(d, 'git.exe')):
            cp_hits.append(os.path.join(d, 'git.exe'))
    out['ResolutionCreateProcess'] = {'Rule': 'the instrument starts "git" with subprocess (CreateProcess, no application name): '
                                              'directory of the interpreter image, current directory (skipped when '
                                              'NoDefaultCurrentDirectoryInExePath is set), System32, System, Windows, then PATH; only git.exe',
                                      'Order': cp_dirs[:6] + ['<PATH entries>'], 'FirstHit': cp_hits[0] if cp_hits else None,
                                      'FirstHitIsDeclaredGit': bool(cp_hits) and os.path.normcase(cp_hits[0]) == os.path.normcase(X.GIT_EXE),
                                      'NoDefaultCurrentDirectoryInExePathOfTheBench': os.environ.get('NoDefaultCurrentDirectoryInExePath')}
    probes = {'Run': False}
    if not rt_names:
        env = dict(L.genv(), PATH=child_path)
        T = M.BENCH + '/c-m' if os.path.isdir(M.BENCH + '/c-m') else M.BENCH
        pp = subprocess.run([L.PWSH, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command',
                             '(Get-Command git -CommandType Application -All | Select-Object -ExpandProperty Source) -join "|"'],
                            capture_output=True, stdin=subprocess.DEVNULL, env=env, cwd=T)
        py = subprocess.run([X.PY_EXE, '-I', '-c', 'import subprocess; p = subprocess.run(["git", "--exec-path"], capture_output=True); '
                                                 'print(p.returncode, p.stdout.decode().strip())'],
                            capture_output=True, stdin=subprocess.DEVNULL, env=env, cwd=T)
        probes = {'Run': True, 'Cwd': T, 'PwshGetCommandAll': pp.stdout.decode('utf-8', 'replace').strip().split('|'),
                  'PythonSubprocessGitExecPath': py.stdout.decode('utf-8', 'replace').strip(),
                  'SameInstallationAsDeclared': py.stdout.decode('utf-8', 'replace').strip().endswith(out['Git']['ExecPath'])}
    out['Probes'] = probes
    out['Versions'] = L.versions()
    write_group('p0-env.json', out)


# ------------------------------------------------------------------ group prep: copies with refs preserved, P1 and P2
def group_prep():
    L.rmtree(W)                      # every run starts from an empty work area
    L.ensure_dir(W)
    info = {}
    for name in ('v', 'n4', 'nc1', 'n5', 'n6', 'n7', 'm'):
        T, inf = M.prepare(name)
        info[name] = inf
        print('prepared', T, inf['P1']['Held'], inf['P2']['Launch'], flush=True)
    # N6 (work without commit): an uncommitted change in the -C, written after P1/P2 (bench state, not an RAE §10 input mutation)
    T6 = M.BENCH + '/c-n6'
    f = T6 + '/src/Fixture.Lib/Calculator.cs'
    b = L.rbytes(f)
    L.wbytes(f, b + b'// trabajo sin commit (banco m7, N6)\n')
    info['n6']['Dirty'] = {'Path': 'src/Fixture.Lib/Calculator.cs', 'StatusPorcelain': L.gout(T6, 'status', '--porcelain')}
    save_state('prep', info)


# ------------------------------------------------------------------ group build: designations, E, regression, derivations (no acceptor)
def inputs_for(case_, T, inp):
    """Writes the custodied inputs of an invocation (RAE §10 copies) and returns (deleg, hand, h_run, mutation note)."""
    other = L.gout(T, 'rev-parse', M.OTHER_REF)

    def put(**kv):
        def f(h):
            h.update(kv)
        return f
    if case_ == 'V' or case_ == 'M' or case_ == 'N6':
        d, h = M.write_inputs(T, inp)
        return d, h, M.H_RUN, 'ninguna (copias de la verificacion real)' if case_ != 'N6' else 'ninguna en las entradas: cambio sin commit en el -C'
    if case_ == 'N4':
        d, h = M.write_inputs(T, inp, hand_present=False)
        return d, h, M.H_RUN, 'entrega ausente (sin archivo)'
    if case_ == 'NC1':
        d, h = M.write_inputs(T, inp, hand_mut=put(CurrentSha=Y.NX))
        return d, h, M.H_RUN, 'H.CurrentSha = deadbeef... (no es un objeto)'
    if case_ == 'NC2':
        def mut(dd):
            diff = sorted(L.gout(T, 'diff', '--name-only', '--no-renames', M.TIP + '..' + M.G).split())
            first, rest = diff[0], diff[1:]
            out = []
            for e in dd['AllowedWriteScope']:
                core = e[:-2] if e.endswith('/**') else e
                if e == first or (e.endswith('/**') and first.startswith(core)):
                    out += [x for x in rest if x not in dd['AllowedWriteScope'] and x not in out]
                else:
                    out.append(e)
            dd['AllowedWriteScope'] = out
        d, h = M.write_inputs(T, inp, deleg_mut=mut)
        return d, h, M.H_RUN, 'D.AllowedWriteScope sin el primer archivo del diff (prefijo sustituido por la enumeracion de los demas)'
    if case_ == 'NC3':
        d, h = M.write_inputs(T, inp, hand_mut=lambda hh: hh.update(WorkCompleted=hh['WorkCompleted'] + ' GATE PASS'))
        return d, h, M.H_RUN, 'H.WorkCompleted con el termino de gate GATE PASS'
    if case_ == 'N5':
        d, h = M.write_inputs(T, inp, hand_mut=lambda hh: hh.update(TestResults=[dict(hh['TestResults'][0], Selected=0, Passed=0)]))
        return d, h, M.H_RUN, 'H.TestResults con 0 pruebas (Selected 0, Passed 0)'
    if case_ == 'N7A':
        d, h = M.write_inputs(T, inp, hand_mut=put(DelegationRunId=M.OTHER_D_RUN))
        return d, h, M.H_RUN, 'H.DelegationRunId de otra corrida (%s)' % M.OTHER_D_RUN
    if case_ in ('N7AR-OWN', 'N7AR-SAME'):
        d, h = M.write_inputs(T, inp, hand_mut=put(RunId=M.OTHER_H_RUN))
        return d, h, (M.OTHER_H_RUN if case_ == 'N7AR-OWN' else M.H_RUN), 'H.RunId de otra corrida (%s)' % M.OTHER_H_RUN
    if case_ == 'N7B':
        d, h = M.write_inputs(T, inp, hand_mut=put(DelegationRunId=M.OTHER_D_RUN, CurrentSha=other))
        return d, h, M.H_RUN, 'H.DelegationRunId de otra corrida y H.CurrentSha = %s (cabeza de origin/fx/u1-fx05)' % other[:12]
    raise ValueError(case_)


def inv_of(case_):
    base = case_.split('-')[0] if case_.startswith('N7AR') else case_
    key = 'N7AR' if case_.startswith('N7AR') else base
    return M.invocation(key, task_id='T1-m7' if case_ == 'M' else 'T1')


def taskdir_of(case_):
    return M.TASKDIR['N7AR' if case_.startswith('N7AR') else case_]


def group_build():
    prep = load_state('prep')
    work = W + '/build'
    L.rmtree(work)
    L.ensure_dir(work)
    L.rmtree(M.INPUTS)               # deliverable copies are rewritten from this run only
    L.rmtree(M.DESIG)
    cases, st = [], {'Cases': {}}
    # ---- regression of the builder (no instrument entry): r2 part (d) and the E files of r4, byte for byte
    r2inp = Y.R2_INP
    inv_v = M.invocation('V')

    def des_r(repo, inp_files, td):
        d = {'Schema': DS.SCHEMA, 'Case': 'REG', 'Repo': repo, 'AuthorityRevision': M.AR, 'Invocation': inv_v,
             'RunPath': M.ignore_path(inv_v)[:-len('output.json')],
             'Inputs': [{'Role': 'DELEG', 'Path': Y.deliv_path(td, M.D_RUN, 'output.json'), 'Custody': inp_files[0]},
                        {'Role': 'H', 'Path': Y.deliv_path(td, M.H_RUN, 'worker-handoff.json'), 'Custody': inp_files[1]},
                        {'Role': 'CONTRACT', 'Path': M.K, 'Custody': None, 'Blob': M.K_BLOB},
                        {'Role': 'TRX', 'Path': M.TRX_E, 'Custody': inp_files[2]}],
             'Instrument': None, 'MapPath': M.MAP, 'Adapter': 'codex-cli'}
        return d
    r2d = des_r(Y.SRC_DELIV, [r2inp + '/deleg-deliv.json', r2inp + '/handoff-deliv.json', r2inp + '/fixture-tests.trx'], 'T1')
    M.jdump(work + '/REG-R2.designation.json', r2d)
    for label, variant, ref in (('REG-R2', 'R7-A', Y.R2_EIC), ('REG-R2-STRICT', 'R7-B-K', Y.R2_EIC_STRICT)):
        E, rep = MK.build(r2d, variant, instrument=False)
        b = MK.serialize(E)
        rb = L.rbytes(ref)
        diff = None if b == rb else _json_diff(json.loads(rb.decode('utf-8')), E)
        cases.append(case('build', label, 'Constructor %s sin la entrada del instrumento, designacion de r2 (d): reproduce el %s custodiado' % (variant, os.path.basename(ref)),
                          'bytes identicos (SHA-256 igual)', {'Sha256Built': L.sha256(b), 'Sha256Custodied': L.sha256(rb), 'BytesEqual': b == rb, 'Diff': diff},
                          b == rb))
    # derivation of the regression E = r4 D-R2 (reuse by identity: same E bytes, same vector -> same closure)
    E_r2p = work + '/E-REG-R2.json'
    L.wbytes(E_r2p, MK.serialize(MK.build(r2d, 'R7-A', instrument=False)[0]))
    dr = M.derive(r2d, E_r2p, work, 'REG-R2')
    r2c = json.loads(L.rbytes(Y.R2_CLOSURE).decode('utf-8'))
    cases.append(case('build', 'REG-R2-DERIVE', 'Designacion de r2 -> comprobacion de designacion -> derive_r4.py: el cierre custodiado de r2 (y el D-R2 de r4)',
                      'designacion valida; cierre identico a EIC-VERIFY.bmin-closure.json; omitido solo D.ChainRedSha FIELD_NULL (= r4 D-R2, p7-derive.json)',
                      M.brief(dr), dr['Derived'] and Y.closure_diff(dr['Result']['Closure'], r2c) == {}
                      and Y.omitted_pairs(dr['Result']['Omitted']) == [('C1.D.ChainRedSha', 'FIELD_NULL')]))
    for label, rr, td in (('REG-R4-V', 'v', 'T1'), ('REG-R4-N4', 'n4', 'T1-n4'), ('REG-R4-NC1', 'nc1', 'T1-nc1')):
        repo = 'D:/r62-a6r4/r1/c-' + rr
        inp = 'D:/r62-a6r4/r1/c-%s-in' % rr
        d = des_r(repo, [inp + '/deleg.json', inp + '/handoff.json', inp + '/fixture-tests.trx'], td)
        E, rep = MK.build(d, 'R7-A', instrument=False)
        b = MK.serialize(E)
        ref = 'D:/r62-a6r4/r1/s-%s/E.json' % rr
        rb = L.rbytes(ref)
        cases.append(case('build', label, 'Constructor R7-A sin instrumento con la designacion de r4 (%s): reproduce el E de r4' % rr,
                          'bytes identicos al E custodiado de r4 (s-%s/E.json)' % rr,
                          {'Sha256Built': L.sha256(b), 'Sha256R4': L.sha256(rb), 'BytesEqual': b == rb,
                           'Diff': None if b == rb else _json_diff(json.loads(rb.decode('utf-8')), E)}, b == rb))
    # ---- designations, E and derivations of every invocation (real designations of this bench)
    for c in CASES:
        T = M.BENCH + '/c-' + COPY[c]
        inp = W + '/in-' + c.lower()
        deleg, hand, h_run, mut = inputs_for(c, T, inp)
        inv = inv_of(c)
        des = M.designation(c, T, inp, inv, h_run=h_run, taskdir=taskdir_of(c))
        dp = M.jdump(M.DESIG + '/' + c + '.json', des)
        st['Cases'][c] = {'T': T, 'Inp': inp, 'Designation': dp, 'Mutation': mut, 'Inv': inv, 'HRun': h_run, 'Variants': {}}
        for v in VARIANTS + (('R7-B-K',) if c in ('V', 'M') else ()):
            Ep, E, rep = M.build_e(des, v, work, c + '-' + v)
            dres = M.derive(des, Ep, work, c + '-' + v, bypass=('KPR-2',) if c == 'N7AR-SAME' else ())
            st['Cases'][c]['Variants'][v] = {'E': Ep, 'Excluded': rep['Excluded'], 'I6Violations': [(r['Path'], r['Status']) for r in rep['I6']['Violations']],
                                             'Derived': dres['Derived'], 'Check': dres['Check'],
                                             'Closure': dres.get('ClosurePath'), 'Record': dres.get('RecordPath'), 'Vector': dres.get('VectorPath'),
                                             'DesignationPath': dres['DesignationPath'], 'Brief': M.brief(dres),
                                             'EntryClasses': [(r['Path'], r['Class']) for r in rep['Entries']],
                                             'TransitiveAtEFFAllEqual': rep['TransitiveAtEFF']['AllEqual'],
                                             'SchemaErrors': X.validate('input-closure.v1.schema.json', E)}
    save_state('build', st)
    # ---- expectations on the derivations
    vA = st['Cases']['V']['Variants']['R7-A']
    r4v = json.loads(L.rbytes('D:/r62-a6r4/r1/s-v/closure-v.closure.json').decode('utf-8'))
    myv = json.loads(L.rbytes(vA['Closure']).decode('utf-8'))
    dv = Y.closure_diff(myv, r4v)
    EA = json.loads(L.rbytes(vA['E']).decode('utf-8'))
    E0 = MK.build(json.loads(L.rbytes(st['Cases']['V']['Designation']).decode('utf-8')), 'R7-A', instrument=False)[0]
    ediff = _json_diff(E0, EA)
    cases.append(case('build', 'E-V-INSTRUMENT', 'R7-A de V con la entrada del instrumento frente al mismo E sin ella',
                      'solo una entrada mas en CanonicalInputs ({Path de procedencia, Blob 9a829b0e}) y una accion mas en AllowedActions (A6-2, reglas 1 y 2)',
                      ediff, sorted(ediff.keys()) == ['AllowedActions', 'CanonicalInputs']
                      and ediff['CanonicalInputs']['Added'] == [M.INSTRUMENT] and len(ediff['AllowedActions']['Added']) == 1
                      and not ediff['CanonicalInputs']['Removed']))
    cases.append(case('build', 'D-V-A-vs-R4', 'Cierre derivado de V (R7-A) frente al cierre de la verificacion real de r4 (D-V)',
                      'solo la ruta del instrumento de mas en C4 (A6-9: toda ruta de E entra en Paths) y los SHA-256 de DELEG y H (ruta del clon)',
                      dv, set(dv) == {'Files', 'Paths'} and dv['Paths'] == {'Extra': [M.INSTRUMENT['Path']], 'Missing': []}
                      and sorted(n for n, _ in dv['Files']['Extra']) == ['DELEG', 'H']))
    base_om = [('C1.D.ChainRedSha', 'FIELD_NULL')]
    H_ABS = [('C1.H.BaseSha', 'INPUT_ABSENT'), ('C1.H.CurrentSha', 'INPUT_ABSENT'), ('C1.H.RedSha', 'INPUT_ABSENT'),
             ('C1.RevList(D.BaseSha..H.CurrentSha)', 'INPUT_ABSENT'), ('C6.H', 'INPUT_ABSENT')]
    K_ABS = [('C1.K.AuthorityRevision', 'INPUT_ABSENT'), ('C1.K.MainSha', 'INPUT_ABSENT'), ('C1.MergeBase(D.MainSha,K.AuthorityRevision)', 'INPUT_ABSENT'),
             ('C4.K.Authorities', 'INPUT_ABSENT'), ('C6.CONTRACT', 'INPUT_ABSENT')]
    NC1_OM = [('C1.H.CurrentSha', 'NOT_A_COMMIT'), ('C1.RevList(D.BaseSha..H.CurrentSha)', 'PRIMITIVE_EMPTY')]
    vcommits = set(myv['Commits'])
    other = L.gout(M.BENCH + '/c-n7', 'rev-parse', M.OTHER_REF)
    for c in CASES:
        for v in VARIANTS:
            s = st['Cases'][c]['Variants'][v]
            b = s['Brief']
            om = sorted(base_om + (H_ABS if c == 'N4' else []) + (NC1_OM if c == 'NC1' else []) + (K_ABS if v == 'R7-B' else []))
            if c == 'N7AR-SAME':
                exp = 'la comprobacion de designacion falla cerrada (KPR-2: Name(p) de H indefinido); sin la comprobacion, derive_r4.py da un cierre sin C6 H y sin ninguna omision registrada'
                hold = (not s['Check']['Ok'] and any(p['Code'] == 'KPR-2' for p in s['Check']['Bypassed'] + s['Check']['Problems'])
                        and b.get('Derived') and 'H' not in b['Files'] and b['Omitted'] == om)
            else:
                files = sorted(['DELEG', 'TRX'] + ([] if c == 'N4' else ['H']) + ([] if v == 'R7-B' else ['CONTRACT']))
                exp = 'designacion valida; omitidos: %s; C6 = %s' % ('; '.join('%s %s' % x for x in om), ', '.join(files))
                hold = s['Check']['Ok'] and b.get('Derived') and b['Omitted'] == om and b['Files'] == files and not s['SchemaErrors']
                if v == 'R7-A' and c not in ('N4', 'N7B'):
                    hold = hold and b['Counts']['Commits'] == len(vcommits)
                if c == 'N7B':
                    cl = json.loads(L.rbytes(s['Closure']).decode('utf-8'))
                    hold = hold and other in cl['Commits']
                    exp += '; el CurrentSha de la otra corrida (%s) entra en C1' % other[:12]
            cases.append(case('derive', 'D-%s-%s' % (c, v), '%s: %s' % (c, st['Cases'][c]['Mutation']), exp,
                              dict(b, Excluded=[(x['Path'], x['Cause']) for x in s['Excluded']], I6Violations=s['I6Violations'],
                                   DesignationProblems=s['Check']['Problems'], Bypassed=s['Check']['Bypassed']), hold))
    eff = dict(('%s/%s' % (c, v), s['TransitiveAtEFFAllEqual']) for c in CASES for v, s in st['Cases'][c]['Variants'].items())
    cases.append(case('build', 'E-EFF-COVERAGE', 'Transitivos de E fijados en la AuthorityRevision frente a sus blobs en EFF (= D.MainSha) y EFF^1, donde los lee clausemap',
                      'en este fixture, mismos blobs en las tres revisiones para todo transitivo de todo E (si difirieran, R7-A no cubriria esas lecturas)',
                      {'AllEqual': all(eff.values()), 'Different': sorted(k for k, x in eff.items() if not x)}, all(eff.values())))
    for c in ('V', 'M'):
        s = st['Cases'][c]['Variants']['R7-B-K']
        cases.append(case('derive', 'D-%s-R7-B-K' % c, '%s con la variante I6 estricta de r2 (contrato conservado): comparacion' % c,
                          'designacion valida; omitido solo D.ChainRedSha; C6 con CONTRACT (igual que el EIC-I6-STRICT de r2 salvo la entrada del instrumento)',
                          dict(s['Brief'], Excluded=[(x['Path'], x['Cause']) for x in s['Excluded']]),
                          s['Check']['Ok'] and s['Brief']['Omitted'] == base_om and 'CONTRACT' in s['Brief']['Files']))
    # ---- designation negatives (O2, O3), on V's designation and E (derivation level, no acceptor)
    cases += designation_negatives(st, work)
    # ---- Name(p) of C6 per control under two kit path rules (O6)
    cases += name_table(st)
    # deliverable copies
    for c in CASES:
        files = [st['Cases'][c]['Designation']]
        for v, s in st['Cases'][c]['Variants'].items():
            files += [s['E'], s['Closure'], s['Record'], s['Vector']]
        deliver(c, files)
    write_group('p1-build.json', {'Schema': 'm7-bench/p1-build', 'Cases': cases})


def _json_diff(a, b):
    """{list field: {'Added': [...], 'Removed': [...]}} between two input-closure objects (b relative to a)."""
    out = {}
    for k in sorted(set(a) | set(b)):
        x, y = a.get(k), b.get(k)
        if x == y:
            continue
        if isinstance(x, list) and isinstance(y, list):
            out[k] = {'Added': [e for e in y if e not in x], 'Removed': [e for e in x if e not in y]}
        else:
            out[k] = {'Before': x, 'After': y}
    return out


def designation_negatives(st, work):
    cases = []
    sV = st['Cases']['V']
    des = json.loads(L.rbytes(sV['Designation']).decode('utf-8'))
    E = json.loads(L.rbytes(sV['Variants']['R7-A']['E']).decode('utf-8'))
    T = sV['T']
    hpath = [i for i in des['Inputs'] if i['Role'] == 'H'][0]['Path']
    neg = W + '/neg'
    L.rmtree(neg)
    L.ensure_dir(neg)

    def raw_derive(d, Ex):
        """derive_r4.py alone with the designation's paths (no check): what the r4 derivation does with this input."""
        ep = M.jdump(neg + '/E-raw-%d.json' % len(os.listdir(neg)), Ex)
        ro = dict((i['Role'], i) for i in d['Inputs'] if isinstance(i, dict) and i.get('Role') in DS.ROLES)
        cust = dict((ro[r]['Path'], ro[r]['Custody']) for r in ('DELEG', 'H', 'TRX', 'CONTRACT') if r in ro and ro[r].get('Custody'))
        vec = {'Repo': d['Repo'], 'E': ep, 'I': d['Invocation'], 'Designation': {'D': ro['DELEG']['Path'], 'H': ro['H']['Path'], 'K': ro['CONTRACT']['Path']},
               'Custody': cust, 'MapPath': d['MapPath']}
        try:
            r = DV.derive_vector(vec)
            return {'Outcome': 'derived', 'Counts': M.counts(r['Closure']), 'Omitted': Y.omitted_pairs(r['Omitted']),
                    'Files': sorted(f['Name'] for f in r['Closure']['Files'])}, r
        except Exception as e:
            return {'Outcome': 'exception', 'Type': type(e).__name__, 'Message': str(e)[:160]}, None

    def one(label, what, d, Ex, codes, expected, extra_check=None):
        chk = DS.check(d, Ex)
        raw, rr = raw_derive(d, Ex)
        got = sorted(set(p['Code'] for p in chk['Problems']))
        hold = (got == sorted(codes)) if codes else chk['Ok']
        if extra_check:
            hold = hold and extra_check(chk, raw, rr)
        cases.append(case('desig', label, what, expected, {'CheckOk': chk['Ok'], 'Codes': got, 'Problems': chk['Problems'], 'Names': chk['Names'],
                                                          'InputsPresent': dict((k, v['Present']) for k, v in chk['Inputs'].items()),
                                                          'Derive_r4Alone': raw}, hold))
        return chk, raw, rr

    one('DES-OK', 'Designacion de V (rutas del kit) sobre su E R7-A', des, E, [], 'valida: D, H, K presentes; Name(p) define DELEG, H, CONTRACT y TRX en las rutas designadas')
    sN4 = st['Cases']['N4']
    one('DES-N4', 'Designacion de N4: la ruta de H designada no esta en E', json.loads(L.rbytes(sN4['Designation']).decode('utf-8')),
        json.loads(L.rbytes(sN4['Variants']['R7-A']['E']).decode('utf-8')), [], 'valida: H ausente por la regla (i) (no es un fallo de la designacion)',
        lambda c, r, rr: c['Inputs']['H']['Present'] is False)
    d2 = json.loads(json.dumps(des))
    d2['Inputs'].append({'Role': 'NOTES', 'Path': 'docs/automation/evidence/FX-U1-agent/T1/worker-notes.md', 'Custody': None})
    one('DES-ROLE-EXTRA', 'Designacion con un rol fuera de DELEG/H/CONTRACT/TRX', d2, E, ['ROLE_OUTSIDE_DHK'], 'falla cerrada: ROLE_OUTSIDE_DHK')
    # two entries at H's path (another blob): derive_r4 alone takes the first one
    E2 = json.loads(json.dumps(E))
    E2['CanonicalInputs'].append({'Path': hpath, 'Blob': Y.TARGET_BLOB})
    one('DES-MULTI', 'E con dos entradas en la ruta de la entrega (otro blob)', des, E2, ['MULTIPLE_PINS'],
        'falla cerrada: MULTIPLE_PINS (derive_r4.py solo toma la primera entrada)')
    # O3: E also pins the handoff of another run at its own path; identification by pattern vs by designated path
    oh = json.loads(L.rbytes(sV['Inp'] + '/handoff.json').decode('utf-8'))
    other = L.gout(T, 'rev-parse', M.OTHER_REF)
    oh.update(RunId=M.OTHER_H_RUN, DelegationRunId=M.OTHER_D_RUN, CurrentSha=other)
    ohp = M.jdump(neg + '/handoff-other-run.json', oh)
    opath = Y.deliv_path('T1', M.OTHER_H_RUN, 'worker-handoff.json')
    E3 = json.loads(json.dumps(E))
    E3['CanonicalInputs'].append({'Path': opath, 'Blob': Y.blob_file(ohp)})
    pattern = [e['Path'] for e in E3['CanonicalInputs'] if re.fullmatch(r'artifacts/orchestration/[^/]+/[^/]+/[0-9]+/([^/]+)/worker-handoff\.json', e['Path'])]
    d3 = json.loads(json.dumps(des))
    chk3, raw3, rr3 = one('DES-O3-OTHER-RUN', 'E fija ademas la entrega de otra corrida en su propia ruta (O3)', des, E3, ['UNDESIGNATED_INPUT'],
                          'falla cerrada: UNDESIGNATED_INPUT; por patron hay dos candidatas a H, por la ruta designada una')
    d3b = json.loads(json.dumps(des))
    for i in d3b['Inputs']:
        if i['Role'] == 'H':
            i['Path'], i['Custody'] = opath, ohp
    rawb, rrb = raw_derive(d3b, E3)
    diff = Y.closure_diff(rrb['Closure'], rr3['Closure']) if rr3 and rrb else None
    cases.append(case('desig', 'DES-O3-PATTERN', 'O3: el mismo E con H identificada por patron (la otra candidata) en vez de por la ruta designada',
                      'cierres distintos (C1 con el CurrentSha de la otra corrida, C6 H con otro SHA-256): la identificacion tiene que venir de las rutas del kit',
                      {'PatternCandidates': pattern, 'Diff': diff}, len(pattern) == 2 and bool(diff)))
    # TRX custodied bytes with another blob
    tb = neg + '/trx-bad.trx'
    L.wbytes(tb, L.rbytes(sV['Inp'] + '/fixture-tests.trx') + b'\n')
    d4 = json.loads(json.dumps(des))
    for i in d4['Inputs']:
        if i['Role'] == 'TRX':
            i['Custody'] = tb
    one('DES-TRX-BAD', 'Los bytes custodiados del TRX no tienen el blob que fija E', d4, E, ['TRX_UNVERIFIED'],
        'falla cerrada: TRX_UNVERIFIED (derive_r4.py se detiene con una excepcion: error del banco)')
    # clause map pinned with a blob that does not exist
    E5 = json.loads(json.dumps(E))
    for e in E5['AllowedTransitiveInputs']:
        if e['Path'] == M.MAP:
            e['Blob'] = '0123456789abcdef0123456789abcdef01234567'
    one('DES-MAP-NX', 'El mapa M fijado por E con un blob que no existe en el -C', des, E5, ['MAP_UNUSABLE'],
        'falla cerrada: MAP_UNUSABLE (derive_r4.py usa sin aviso un mapa vacio: C4 sin Surfaces ni Files)',
        lambda c, r, rr: r.get('Outcome') == 'derived')
    # wrong JSON types of non-commit fields the table reads
    for label, role, field, value, code, what in (
            ('DES-TYPE-BRANCH', 'DELEG', 'ExpectedBranch', 5, 'WRONG_TYPE', 'D.ExpectedBranch = 5 (numero)'),
            ('DES-TYPE-HRUN', 'H', 'RunId', 7, 'WRONG_TYPE', 'H.RunId = 7 (numero)'),
            ('DES-TYPE-AUTH', 'CONTRACT', 'Authorities', 'x', 'WRONG_TYPE', 'K.Authorities = "x" (cadena)')):
        src = {'DELEG': sV['Inp'] + '/deleg.json', 'H': sV['Inp'] + '/handoff.json', 'CONTRACT': sV['Inp'] + '/K.json'}[role]
        o = json.loads(L.rbytes(src).decode('utf-8'))
        o[field] = value
        mp = M.jdump(neg + '/%s.json' % label, o)
        d6 = json.loads(json.dumps(des))
        rpath = None
        for i in d6['Inputs']:
            if i['Role'] == role:
                i['Custody'] = mp
                rpath = i['Path']
                if role == 'CONTRACT':
                    i.pop('Blob', None)
        E6 = json.loads(json.dumps(E))
        for e in E6['CanonicalInputs']:
            if e['Path'] == rpath:
                e['Blob'] = Y.blob_file(mp)
        one(label, what, d6, E6, [code], 'falla cerrada: WRONG_TYPE (derive_r4.py: excepcion o uso silencioso)')
    d7 = json.loads(json.dumps(des))
    d7['Invocation'] = dict(d7['Invocation'], Target=M.G)
    one('DES-TYPE-TARGET', 'I.Target es una cadena, no un objeto', d7, E, ['WRONG_TYPE'],
        'falla cerrada: WRONG_TYPE (derive_r4.py lo registra sin aviso como I.Target.Commit FIELD_EMPTY)',
        lambda c, r, rr: r.get('Outcome') == 'derived' and ('C1.I.Target.Commit', 'FIELD_EMPTY') in [tuple(x) for x in r['Omitted']])
    d8 = json.loads(json.dumps(des))
    d8['RunPath'] = 'artifacts/orchestration/FX-U1/T1-n4/0/%s/' % d8['Invocation']['RunId']
    one('DES-KPR1', 'RunPath bajo T1-n4 con I.TaskId = T1 (la salida no esta en IgnorePath(I))', d8, E, ['KPR-1'],
        'falla cerrada: KPR-1 (un cleantree con esa ruta ignorada quedaria fuera de C4)')
    return cases


def name_table(st):
    """Name(p) of C6 for the DELEG and H copies of every invocation under two kit path rules: P-SAME (directory = RunId of
    the input of the real verification) and P-OWN (directory = RunId of the bytes placed there; KPR-2)."""
    cases = []
    rows = []
    for c in CASES:
        if c == 'N7AR-SAME':
            continue
        s = st['Cases'][c]
        inp = s['Inp']
        td = taskdir_of(c)
        d = json.loads(L.rbytes(inp + '/deleg.json').decode('utf-8'))
        hp = inp + '/handoff.json'
        h = json.loads(L.rbytes(hp).decode('utf-8')) if os.path.isfile(hp) else None
        row = {'Case': c, 'Mutation': s['Mutation']}
        for rule, d_dir, h_dir in (('P-SAME', M.D_RUN, M.H_RUN), ('P-OWN', d['RunId'], h['RunId'] if h else None)):
            dpath = Y.deliv_path(td, d_dir, 'output.json')
            nd = DS.name_of(dpath, M.K, d['RunId'], h['RunId'] if h else None)
            if h is None:
                nh = 'H ausente (N4): sin entrada en E'
            else:
                nh = DS.name_of(Y.deliv_path(td, h_dir, 'worker-handoff.json'), M.K, d['RunId'], h['RunId'])
            row[rule] = {'DELEG': nd, 'H': nh}
        rows.append(row)
    same_bad = [r['Case'] for r in rows if r['P-SAME']['DELEG'] != 'DELEG' or r['P-SAME']['H'] not in ('H', 'H ausente (N4): sin entrada en E')]
    own_bad = [r['Case'] for r in rows if r['P-OWN']['DELEG'] != 'DELEG' or r['P-OWN']['H'] not in ('H', 'H ausente (N4): sin entrada en E')]
    cases.append(case('desig', 'NAME-TABLE', 'Name(p) de C6 para las copias de DELEG y H de cada invocacion con dos reglas de ruta del kit',
                      'P-SAME (directorio = RunId de la verificacion real): indefinido solo si el campo mutado es H.RunId (N7a-RID); P-OWN (directorio = RunId de los bytes copiados, KPR-2): definido en todas',
                      {'Rows': rows, 'UndefinedUnderPSame': same_bad, 'UndefinedUnderPOwn': own_bad},
                      same_bad == ['N7AR-OWN'] and own_bad == []))
    return cases


# ------------------------------------------------------------------ synthetic outputs of N4 and nc1 (round-4 texts, run_r4.py)
NOIN_N4 = 'la comprobacion Handoff esta en fail (entrega ausente: no hay CurrentSha ni texto de la entrega)'
NOIN_NC1 = 'la comprobacion Identity esta en fail (CurrentSha no es un commit existente)'


def cv_n4(F, deleg, handoff_path, prim, run_id):
    auth_ok, defects, ar = X.authority_from(F)
    t = prim['Handoff']
    checks = {
        'Termination': {'Result': 'pass', 'Evidence': 'registro de la sesion (sintetico, banco m7): trabajo del Worker COMPLETED, muerte confirmada'},
        'Handoff': {'Result': 'fail', 'Evidence': 'entrega ausente en ' + handoff_path + ' (primaria: ' + t[0] + ' -> ' + t[1].strip() + ')'},
        'Authority': {'Result': X.res(auth_ok), 'Evidence': X._ev(F, 'clausemap', 'defectos del mapa: ' + (','.join(defects) or 'ninguno'))},
        'Contract': {'Result': X.res(X.contract_from(F)), 'Evidence': X._ev(F, 'contract')},
        'Identity': X.nr_noinput(NOIN_N4), 'Remote': X.nr_noinput(NOIN_N4), 'Scope': X.nr_noinput(NOIN_N4),
        'CleanTree': {'Result': X.res(X.cleantree_from(F)), 'Evidence': X._ev(F, 'cleantree')},
        'Ci': X.nr_noinput(NOIN_N4, True), 'Tests': X.nr_noinput(NOIN_N4, True), 'Trailer': X.nr_noinput(NOIN_N4),
        'Routing': X.nr_bench('sin modelo'), 'FreeText': X.nr_noinput(NOIN_N4),
        'Denials': X.nr_bench('sin eventos ni transcripcion del Worker'),
    }
    findings = ['Salida sintetica del banco m7 para N4 (entrega ausente): Controller simulado por la supervision.']
    return X.cv_doc(checks, ar, 'BLOCKED', 'EXECUTION_BLOCKED', findings, run_id, M.H_RUN, deleg['RunId'])


def cv_nc1(F, deleg, handoff, prim, variant, run_id):
    auth_ok, defects, ar = X.authority_from(F)
    pi, ps, pt = prim['Identity'], prim['Scope'], prim['Trailer']
    checks = {
        'Termination': {'Result': 'pass', 'Evidence': 'registro de la sesion (sintetico, banco m7): trabajo del Worker COMPLETED, muerte confirmada'},
        'Handoff': {'Result': X.res(X.handoff_from(F, deleg)), 'Evidence': X._ev(F, 'handoff')},
        'Authority': {'Result': X.res(auth_ok), 'Evidence': X._ev(F, 'clausemap', 'defectos del mapa: ' + (','.join(defects) or 'ninguno'))},
        'Contract': {'Result': X.res(X.contract_from(F)), 'Evidence': X._ev(F, 'contract')},
        'Identity': {'Result': 'fail', 'Evidence': 'CurrentSha %s no es un commit existente (primaria: %s -> rc %d)' % (handoff['CurrentSha'], pi[0], pi[2])},
        'Remote': X.nr_bench('sin registro de ls-remote del relevo en la salida del Controller'),
        'Scope': {'Result': 'fail', 'Evidence': 'git diff BaseSha..CurrentSha no resoluble (primaria: %s -> rc %d)' % (ps[0], ps[2])},
        'CleanTree': {'Result': X.res(X.cleantree_from(F)), 'Evidence': X._ev(F, 'cleantree')} if 'cleantree' in F else None,
        'Ci': X.nr_noinput(NOIN_NC1, True), 'Tests': X.nr_noinput(NOIN_NC1, True),
        'Trailer': {'Result': 'fail', 'Evidence': 'rango BaseSha..CurrentSha no resoluble (primaria: %s -> rc %d)' % (pt[0], pt[2])},
        'Routing': X.nr_bench('sin modelo'),
        'FreeText': {'Result': X.res(not F['freetext'][0]['Matches']), 'Evidence': X._ev(F, 'freetext', 'coincidencias=%d' % len(F['freetext'][0]['Matches']))},
        'Denials': X.nr_bench('sin eventos ni transcripcion del Worker'),
    }
    if variant == 'V2':
        checks['Scope'] = X.nr_noinput(NOIN_NC1)
        checks['Trailer'] = X.nr_noinput(NOIN_NC1)
    if variant == 'V3':
        checks['CleanTree'] = {'Result': 'not_run', 'Evidence': 'no ejecutada por el Controller (variante sintetica V3); not_run con la disposicion de su fila (AP 16.9)'}
    findings = ['Salida sintetica del banco m7 para nc1 (CurrentSha inexistente), variante ' + variant + '.']
    return X.cv_doc(checks, ar, 'STOP', 'EXECUTION_BLOCKED', findings, run_id, handoff['RunId'], deleg['RunId'])


# ------------------------------------------------------------------ VERIFY invocations with runs
def vctx(c, v):
    st = load_state('build')
    s = st['Cases'][c]
    sv = s['Variants'][v]
    return s, sv


def verify_request(label, T, s, sv, rruns, rec, cvp, relayp, work):
    dp, hp = s['Inp'] + '/deleg.json', s['Inp'] + '/handoff.json'
    consts = M.consts_for(T, sv['Closure'], relayp, s['Inv'])
    req, dec = TB.construct(cvp, dp, hp, rruns, consts)
    reqp = X.write_request(work + '/request-' + label + '.json', req)
    o = acc_obs(T, reqp, lambda rc, out: TM.evaluate_v(reqp, rc, out, cvp, dp, hp, consts, sv['Vector'], sv['Record'], sv['DesignationPath'],
                                                        rec['InstrumentExecutions'], rec['Runs']))
    return o, reqp, dec


def run_verify_case(group, c, names, current, cv_fn, what, r4_expected, do_b=True):
    """R7-A: full order set; R7-B: the same runs against the R7-B closure (which orders are excluded), then a session with
    only the admitted orders and its request."""
    s, sA = vctx(c, 'R7-A')
    _, sB = vctx(c, 'R7-B')
    T = s['T']
    work = W + '/s-' + c.lower()
    L.ensure_dir(work)
    deleg = json.loads(L.rbytes(s['Inp'] + '/deleg.json').decode('utf-8'))
    hand = json.loads(L.rbytes(s['Inp'] + '/handoff.json').decode('utf-8'))
    run_id = s['Inv']['RunId']
    rs = M.runs(T, s['Inp'], s['Inv'], names, current=current)
    sess, execs, rec, rruns = M.session('s-%s-a' % c.lower(), T, rs, Y.prim_reads(current))
    F = X.facts_by_cmd(rruns)
    cv = cv_fn(F, deleg, hand, sess['Primary'], run_id)
    cvp = work + '/controller-output.json'
    L.wjson_local(cvp, cv)
    relayp, rsv = relay_save(work, 'relay-facts.json', sess, cvp, run_id)
    oA, reqA, decA = verify_request('A', T, s, sA, rruns, rec, cvp, relayp, work)
    cases = [case(group, '%s-A' % c, '%s con R7-A: %s; %d ordenes reconstruidas del registro Codex' % (c, what, len(rruns)),
                  'Verification incluida: %s; ACCEPT_FACTS rc 0; R4 %s; R9 MATCH (ninguna corrida fuera); cero escrituras; tabla aceptada (construccion, aceptacion, cotejo del cierre y designacion)'
                  % ('si' if r4_expected == 'MATCH' else 'no', r4_expected),
                  oA, ok_accept(oA, r4_expected) and decA['Include'] == (r4_expected == 'MATCH'),
                  Request=req_brief(reqA), Derivation=sA['Brief'], Session=M.sess_brief(sess), Reconstruction=M.rec_brief(rec))]
    files = [cvp, reqA]
    out = {'Clone': T, 'Mutation': s['Mutation'], 'ControllerChecks': dict((k, cv['Checks'][k]['Result']) for k in X.ORDER),
           'Classification': [cv['Classification'], cv['Disposition'], cv['FailureClass']],
           'SchemaValidation': {'controller-verification.v2': X.validate('controller-verification.v2.schema.json', cv), 'relay-record.v2': rsv,
                                'delegation.v2': X.validate('delegation.v2.schema.json', deleg), 'worker-handoff': X.validate('worker-handoff.schema.json', hand)}}
    if do_b:
        oBs, reqBs, _ = verify_request('B-static', T, s, sB, rruns, rec, cvp, relayp, work)
        excl = outside_cmds(oBs)
        cases.append(case(group, '%s-B-STATIC' % c, '%s: las mismas %d corridas frente al cierre R7-B (I6 literal; contrato excluido)' % (c, len(rruns)),
                          'R9 MISMATCH solo en las ordenes que leen el contrato, el objeto o los registros en el Target: contract, clausemap, hashes, read-text, read-json',
                          dict(oBs, ExcludedOrders=excl, R9Violations=oBs['R9Runs']),
                          oBs['Checks'].get('R9') == 'MISMATCH' and excl == ['clausemap', 'contract', 'hashes', 'read-json', 'read-text'],
                          Derivation=sB['Brief']))
        names_b = [n for n in names if n not in excl]
        rs_b = M.runs(T, s['Inp'], s['Inv'], names_b, current=current)
        sess_b, _, rec_b, rruns_b = M.session('s-%s-b' % c.lower(), T, rs_b, Y.prim_reads(current))
        cv_b = M.cv_without_instrument(cv, excl)
        cvp_b = work + '/controller-output-B.json'
        L.wjson_local(cvp_b, cv_b)
        relay_b, _ = relay_save(work, 'relay-facts-B.json', sess_b, cvp_b, run_id)
        oBl, reqBl, decB = verify_request('B-limited', T, s, sB, rruns_b, rec_b, cvp_b, relay_b, work)
        cases.append(case(group, '%s-B-LIMITED' % c, '%s con R7-B: solo las %d ordenes que admite el cierre (%s)' % (c, len(names_b), ', '.join(names_b)),
                          'ACCEPT_FACTS; R4 %s; R9 MATCH; tabla aceptada; las comprobaciones de las ordenes excluidas, sin hecho del instrumento' % r4_expected,
                          oBl, ok_accept(oBl, r4_expected), Request=req_brief(reqBl), Session=M.sess_brief(sess_b)))
        out['R7BExcludedOrders'] = excl
        out['R7BLimitedOrders'] = names_b
        files += [cvp_b, reqBl]
    deliver(c, files)
    return cases, out, (sess, rec, rruns, cvp, relayp)


def group_v():
    cases, out, _ = run_verify_case('V', 'V', M.NAMES_FULL, M.G, lambda F, d, h, p, r: M.cv_case(F, d, h, p, r),
                                    'verificacion real de T1 sobre la entrega', 'MATCH')
    write_group('p2-v.json', dict(out, Schema='m7-bench/p2-v', Cases=cases))


def group_n4():
    s, sA = vctx('N4', 'R7-A')
    T = s['T']
    work = W + '/s-n4'
    L.ensure_dir(work)
    deleg = json.loads(L.rbytes(s['Inp'] + '/deleg.json').decode('utf-8'))
    hp = s['Inp'] + '/handoff.json'
    names = ['contract', 'hashes', 'cleantree', 'read-text', 'read-json', 'trx', 'clausemap']
    rs = M.runs(T, s['Inp'], s['Inv'], names)
    sess, _, rec, rruns = M.session('s-n4-a', T, rs, Y.prim_reads(None, hp, n4=True))
    F = X.facts_by_cmd(rruns)
    cv = cv_n4(F, deleg, hp, sess['Primary'], s['Inv']['RunId'])
    cvp = work + '/controller-output.json'
    L.wjson_local(cvp, cv)
    relayp, _ = relay_save(work, 'relay-facts.json', sess, cvp, s['Inv']['RunId'])
    o, reqp, dec = verify_request('A', T, s, sA, rruns, rec, cvp, relayp, work)
    cases = [case('N4', 'N4-A', 'N4 con R7-A (entrega ausente: terminos de H omitidos); 7 ordenes que no dependen de la entrega',
                  'ACCEPT_FACTS rc 0; R4 NOT_APPLICABLE; R9 MATCH; tabla aceptada (igual que N4-c de r4, con la ruta del instrumento en C4)',
                  o, ok_accept(o, 'NOT_APPLICABLE') and not dec['Include'], Request=req_brief(reqp), Derivation=sA['Brief'],
                  Session=M.sess_brief(sess))]
    deliver('N4', [cvp, reqp])
    write_group('p3-n4.json', {'Schema': 'm7-bench/p3-n4', 'HandoffAbsent': not os.path.exists(hp),
                               'ControllerChecks': dict((k, cv['Checks'][k]['Result']) for k in X.ORDER), 'Cases': cases})


def group_nc1():
    s, sA = vctx('NC1', 'R7-A')
    T = s['T']
    work = W + '/s-nc1'
    L.ensure_dir(work)
    deleg = json.loads(L.rbytes(s['Inp'] + '/deleg.json').decode('utf-8'))
    hand = json.loads(L.rbytes(s['Inp'] + '/handoff.json').decode('utf-8'))
    if L.git(T, 'cat-file', '-e', Y.NX).returncode == 0:
        raise SystemExit('NX exists')
    names = ['contract', 'hashes', 'cleantree', 'read-text', 'read-json', 'trx', 'handoff', 'freetext', 'clausemap']
    rs = M.runs(T, s['Inp'], s['Inv'], names)
    prim = Y.prim_reads(Y.NX)
    sess, _, rec, rruns = M.session('s-nc1-a', T, rs, prim)
    F = X.facts_by_cmd(rruns)
    cases, files = [], []
    for variant, include in (('V1', True), ('V2', False)):
        cv = cv_nc1(F, deleg, hand, sess['Primary'], variant, s['Inv']['RunId'])
        cvp = work + '/controller-output-%s.json' % variant
        L.wjson_local(cvp, cv)
        relayp, _ = relay_save(work, 'relay-facts-%s.json' % variant, sess, cvp, s['Inv']['RunId'])
        o, reqp, dec = verify_request(variant, T, s, sA, rruns, rec, cvp, relayp, work)
        want = 'MATCH' if include else 'NOT_APPLICABLE'
        cases.append(case('nc1', 'NC1-%s-A' % variant, 'nc1 %s con R7-A (H.CurrentSha omitido, RevList vacia); 9 ordenes' % variant,
                          'ACCEPT_FACTS; R4 %s; R9 MATCH; tabla aceptada' % want, o, ok_accept(o, want) and dec['Include'] == include,
                          Request=req_brief(reqp)))
        files += [cvp, reqp]
    rs3 = [r for r in rs if r[0] != 'cleantree']
    sess3, _, rec3, rruns3 = M.session('s-nc1-v3', T, rs3, prim)
    F3 = X.facts_by_cmd(rruns3)
    cv3 = cv_nc1(F3, deleg, hand, sess3['Primary'], 'V3', s['Inv']['RunId'])
    cvp3 = work + '/controller-output-V3.json'
    L.wjson_local(cvp3, cv3)
    relay3, _ = relay_save(work, 'relay-facts-V3.json', sess3, cvp3, s['Inv']['RunId'])
    o3, reqp3, dec3 = verify_request('V3', T, s, sA, rruns3, rec3, cvp3, relay3, work)
    cases.append(case('nc1', 'NC1-V3-A', 'nc1 V3 (CleanTree not_run, sin cleantree) con R7-A', 'ACCEPT_FACTS; R4 NOT_APPLICABLE; R9 MATCH; tabla aceptada',
                      o3, ok_accept(o3, 'NOT_APPLICABLE') and not dec3['Include'], Request=req_brief(reqp3)))
    files += [cvp3, reqp3]
    deliver('NC1', files)
    write_group('p4-nc1.json', {'Schema': 'm7-bench/p4-nc1', 'Derivation': sA['Brief'], 'Cases': cases})


def group_n5():
    def cvf(F, d, h, p, r):
        return M.cv_case(F, d, h, p, r, overrides={'Tests': {'Result': 'fail', 'RedPart': 'not_applicable',
                                                             'Evidence': 'entrega con 0 pruebas: TestResults[0].Selected = 0 (copia mutada, RAE §10); sin TRX de RedSha en el banco'}},
                         finding='Salida sintetica del banco m7 para N5 (0 pruebas): Tests en fail por la entrega.')
    cases, out, _ = run_verify_case('N5', 'N5', M.NAMES_FULL, M.G, cvf, 'entrega con 0 pruebas', 'MATCH')
    write_group('p5-n5.json', dict(out, Schema='m7-bench/p5-n5', Cases=cases))


def group_n6():
    st = load_state('prep')
    cases, out, _ = run_verify_case('N6', 'N6', M.NAMES_FULL, M.G,
                                    lambda F, d, h, p, r: M.cv_case(F, d, h, p, r, finding='Salida sintetica del banco m7 para N6 (trabajo sin commit en el -C).'),
                                    'trabajo sin commit en el -C (cambio en src/Fixture.Lib/Calculator.cs)', 'MATCH')
    cl = out['ControllerChecks']
    cases.append(case('N6', 'N6-CLEANTREE', 'N6: el hecho cleantree del instrumento ve el trabajo sin commit y el Controller sintetico deja CleanTree en fail',
                      'CleanTree fail; Identity, Scope, Trailer y FreeText pass, como en V', {'ControllerChecks': cl, 'Dirty': st['n6'].get('Dirty')},
                      cl['CleanTree'] == 'fail' and all(cl[k] == 'pass' for k in ('Identity', 'Scope', 'Trailer', 'FreeText'))))
    write_group('p6-n6.json', dict(out, Schema='m7-bench/p6-n6', Dirty=st['n6'].get('Dirty'), Cases=cases))


def group_n7():
    cases = []
    c1, o1, _ = run_verify_case('N7', 'N7A', M.NAMES_FULL, M.G,
                                lambda F, d, h, p, r: M.cv_case(F, d, h, p, r, finding='Salida sintetica del banco m7 para N7a (entrega de otra corrida: DelegationRunId).'),
                                'entrega de otra corrida (H.DelegationRunId), Identity pass', 'MATCH')
    cases += c1
    # O6 pair: H.RunId mutated; one session (same bytes), two closures (P-OWN valid; P-SAME derived without the KPR-2 check)
    s, sA = vctx('N7AR-OWN', 'R7-A')
    _, sS = vctx('N7AR-SAME', 'R7-A')
    T = s['T']
    work = W + '/s-n7ar'
    L.ensure_dir(work)
    deleg = json.loads(L.rbytes(s['Inp'] + '/deleg.json').decode('utf-8'))
    hand = json.loads(L.rbytes(s['Inp'] + '/handoff.json').decode('utf-8'))
    run_id = s['Inv']['RunId']
    rs = M.runs(T, s['Inp'], s['Inv'], M.NAMES_FULL)
    sess, _, rec, rruns = M.session('s-n7ar-a', T, rs, Y.prim_reads(M.G))
    F = X.facts_by_cmd(rruns)
    cv = M.cv_case(F, deleg, hand, sess['Primary'], run_id,
                   overrides={'Handoff': {'Result': 'fail', 'Evidence': 'entrega de otra corrida: RunId %s distinto del WorkRunId de la ventana %s (copia mutada, RAE §10)' % (hand['RunId'], M.H_RUN)}},
                   finding='Salida sintetica del banco m7 para N7a con H.RunId de otra corrida (par O6).')
    cvp = work + '/controller-output.json'
    L.wjson_local(cvp, cv)
    relayp, _ = relay_save(work, 'relay-facts.json', sess, cvp, run_id)
    oO, reqO, _ = verify_request('own', T, s, sA, rruns, rec, cvp, relayp, work)
    cases.append(case('N7', 'N7AR-OWN-A', 'N7a con H.RunId de otra corrida; la copia de H en <dir>/0/<RunId de sus bytes>/ (KPR-2)',
                      'designacion valida; Name(p) = H; ACCEPT_FACTS; R4 MATCH; R9 MATCH; tabla aceptada', oO, ok_accept(oO, 'MATCH'),
                      Request=req_brief(reqO), Derivation=sA['Brief']))
    sS2 = dict(s, Inp=s['Inp'])
    oS, reqS, _ = verify_request('same', T, sS2, sS, rruns, rec, cvp, relayp, work)
    cases.append(case('N7', 'N7AR-SAME-A', 'La misma sesion con la copia de H en <dir>/0/<RunId de la verificacion real>/ (riesgo O6), cierre derivado sin KPR-2',
                      'el cierre no tiene C6 H (Name(p) indefinido) y el registro de derivacion no lo declara; REJECT_FACTS por R9 MISMATCH en handoff y freetext (archivo fuera del cierre); tabla no aceptada (designacion KPR-2)',
                      oS, oS['Decision'] == 'REJECT_FACTS' and outside_cmds(oS) == ['freetext', 'handoff'] and not oS['Table']['Accepted']
                      and any('KPR-2' in v for v in oS['Table']['DesignationViolations']),
                      Request=req_brief(reqS), Derivation=sS['Brief']))
    deliver('N7AR-OWN', [cvp, reqO])
    deliver('N7AR-SAME', [reqS])
    st = load_state('build')
    other = st['Cases']['N7B']['Inv']['Target']['Commit']
    hb = json.loads(L.rbytes(st['Cases']['N7B']['Inp'] + '/handoff.json').decode('utf-8'))
    c2, o2, _ = run_verify_case('N7', 'N7B', M.NAMES_FULL, hb['CurrentSha'],
                                lambda F, d, h, p, r: M.cv_case(F, d, h, p, r, finding='Salida sintetica del banco m7 para N7b (entrega de otra corrida, Identity fail).'),
                                'entrega de otra corrida con su CurrentSha (%s), Identity fail' % hb['CurrentSha'][:12], 'MATCH')
    cases += c2
    def handoff_row(o):
        return 'REWORK' if o['ControllerChecks']['Identity'] == 'pass' else 'STOP'
    cases.append(case('N7', 'N7-CONTROLLER', 'Fila Handoff de las salidas sinteticas de N7a y N7b (16.9 #2: otra corrida -> REWORK si Identity pasa; si no, STOP)',
                      'N7a: Handoff fail e Identity pass (aporta REWORK); N7b: Handoff fail e Identity fail (aporta STOP). La Disposition global de toda salida sintetica del banco es STOP porque Authority esta en fail desde r4 (mapa del fixture); el aceptante no la juzga',
                      {'N7A': {'Handoff': o1['ControllerChecks']['Handoff'], 'Identity': o1['ControllerChecks']['Identity'], 'HandoffRow': handoff_row(o1),
                               'Authority': o1['ControllerChecks']['Authority'], 'Classification': o1['Classification']},
                       'N7B': {'Handoff': o2['ControllerChecks']['Handoff'], 'Identity': o2['ControllerChecks']['Identity'], 'HandoffRow': handoff_row(o2),
                               'Authority': o2['ControllerChecks']['Authority'], 'Classification': o2['Classification']}},
                      o1['ControllerChecks']['Handoff'] == 'fail' and handoff_row(o1) == 'REWORK'
                      and o2['ControllerChecks']['Handoff'] == 'fail' and handoff_row(o2) == 'STOP'))
    write_group('p7-n7.json', {'Schema': 'm7-bench/p7-n7', 'N7A': o1, 'N7B': o2, 'Cases': cases})


# ------------------------------------------------------------------ the measurement invocation M (A6-7 rule 1)
A67_OPS = [
    ('bytes', 'entrega exacta de los bytes del objeto por la orden declarada en el shell de la invocacion', None),
    ('isolated', 'ejecucion desde el objeto y en modo aislado', None),
    ('diff-scope', 'git diff --name-only <base>..<head> reproducible y pertenencia por ruta de Scope', ['diff', 'scope']),
    ('git-reads', 'lecturas de Git de Identity, Remote, CleanTree y Trailer', ['identity', 'cleantree', 'diff']),
    ('hashes', 'calculo de blobs y hashes', ['hashes']),
    ('json-text', 'lectura fiel de las entradas JSON y de texto', ['read-json', 'read-text', 'handoff', 'contract', 'trx', 'freetext']),
    ('clausemap', 'resolucion por el mapa de clausulas en la forma de AuthorityResolution', ['clausemap']),
    ('primary', 'contraste de cada hecho usado con la orden Git primaria', None),
    ('structured', 'salidas estructuradas completas', None),
]


def m_request(label, T, sv, rruns, rec, work, designation_path=None):
    consts = M.consts_m(T, sv['Closure'])
    req, cond = TM.construct_m(rruns, consts)
    reqp = X.write_request(work + '/request-' + label + '.json', req)
    o = acc_obs(T, reqp, lambda rc, out: TM.evaluate_m(reqp, rc, out, sv['Vector'], sv['Record'], designation_path or sv['DesignationPath'],
                                                        rec['InstrumentExecutions'], rec['Runs']))
    return o, reqp, cond


def ok_m(o, cond):
    want = dict((r, 'NOT_APPLICABLE' if cond[r] else 'MATCH') for r in ('R5', 'R6', 'R7'))
    return (o['Rc'] == 0 and o['Decision'] == 'ACCEPT_FACTS' and all(o['Checks'].get(r) == 'MATCH' for r in ('R1', 'R2', 'R3', 'R9'))
            and o['Checks'].get('R4') == 'NOT_APPLICABLE' and o['Checks'].get('R10') == 'NOT_APPLICABLE' and o['Checks'].get('R8') == 'MANUAL'
            and all(o['Checks'].get(r) == w for r, w in want.items()) and not o['R9Runs'] and o['ZeroWrite'] and o['ProcessTreeDead']
            and o['Table']['Accepted'])


def group_m():
    s, sA = vctx('M', 'R7-A')
    _, sB = vctx('M', 'R7-B')
    T = s['T']
    work = W + '/s-m'
    L.ensure_dir(work)
    rs = M.runs(T, s['Inp'], s['Inv'], M.NAMES_FULL)
    sess, _, rec, rruns = M.session('s-m-a', T, rs, Y.prim_reads(M.G))
    oA, reqA, condA = m_request('A', T, sA, rruns, rec, work)
    cases = [case('M', 'M-A', 'Medicion M con R7-A: las 12 ordenes (identity, diff, scope, contract, hashes, cleantree, handoff, freetext, clausemap, trx, read-text, read-json)',
                  'sin Verification ni RelayFacts; ACCEPT_FACTS; R1 R2 R3 R9 MATCH; R5 R6 R7 MATCH; R4 y R10 NOT_APPLICABLE; R8 MANUAL; cero escrituras; tabla de M aceptada',
                  oA, ok_m(oA, condA), Request=req_brief(reqA), Derivation=sA['Brief'], Session=M.sess_brief(sess), Reconstruction=M.rec_brief(rec))]
    # KPR-1 negative: the same runs (cleantree with the M run path) against the closure derived with I.TaskId = T1
    des = json.loads(L.rbytes(s['Designation']).decode('utf-8'))
    d1 = json.loads(json.dumps(des))
    d1['Invocation'] = dict(d1['Invocation'], TaskId='T1')
    chk_bad = DS.check(d1, json.loads(L.rbytes(sA['E']).decode('utf-8')))
    d1['RunPath'] = M.ignore_path(d1['Invocation'])[:-len('output.json')]
    dres = M.derive(d1, sA['E'], work, 'M-IGN-T1')
    sv1 = {'Closure': dres['ClosurePath'], 'Vector': dres['VectorPath'], 'Record': dres['RecordPath'], 'DesignationPath': dres['DesignationPath']}
    oI, reqI, _ = m_request('ign-T1', T, sv1, rruns, rec, work)
    cases.append(case('M', 'M-IGN-T1', 'Las mismas corridas frente al cierre derivado con I.TaskId = T1 (la salida de M esta bajo T1-m7)',
                      'con RunPath T1-m7 la designacion falla cerrada (KPR-1); con RunPath = IgnorePath(I) de T1, REJECT_FACTS por R9 MISMATCH solo en cleantree (ruta ignorada fuera de C4)',
                      dict(oI, DesignationWithT1m7RunPath=[p['Code'] for p in chk_bad['Problems']]),
                      [p['Code'] for p in chk_bad['Problems']] == ['KPR-1'] and oI['Decision'] == 'REJECT_FACTS' and outside_cmds(oI) == ['cleantree'],
                      Request=req_brief(reqI)))
    # R7-B: which orders are excluded, then only the admitted ones
    oBs, reqBs, _ = m_request('B-static', T, sB, rruns, rec, work)
    excl = outside_cmds(oBs)
    cases.append(case('M', 'M-B-STATIC', 'Las mismas 12 corridas de M frente al cierre R7-B', 'R9 MISMATCH en contract, clausemap, hashes, read-text y read-json',
                      dict(oBs, ExcludedOrders=excl, R9Violations=oBs['R9Runs']), excl == ['clausemap', 'contract', 'hashes', 'read-json', 'read-text'],
                      Derivation=sB['Brief']))
    names_b = [n for n in M.NAMES_FULL if n not in excl]
    rs_b = M.runs(T, s['Inp'], s['Inv'], names_b)
    sess_b, _, rec_b, rruns_b = M.session('s-m-b', T, rs_b, Y.prim_reads(M.G))
    oBl, reqBl, condB = m_request('B-limited', T, sB, rruns_b, rec_b, work)
    cases.append(case('M', 'M-B-LIMITED', 'M con R7-B: solo las ordenes que admite el cierre (%s)' % ', '.join(names_b),
                      'ACCEPT_FACTS con R6 NOT_APPLICABLE por su condicion de datos (sin clausemap); tabla de M aceptada; operaciones de A6-7, regla 1, sin demostrar',
                      oBl, ok_m(oBl, condB), Request=req_brief(reqBl), Session=M.sess_brief(sess_b)))
    cov = []
    for key, text, cmds in A67_OPS:
        if cmds is None:
            a = b = 'fuera del alcance del banco (exige la sesion real)' if key in ('bytes', 'primary', 'structured') else 'por cada corrida (R2: Runtime.Isolated)'
        else:
            a = 'si' if all(x in M.NAMES_FULL for x in cmds) else 'no'
            inb = [x for x in cmds if x in names_b]
            b = 'si' if len(inb) == len(cmds) else ('en parte (%s)' % ', '.join(inb) if inb else 'no')
        cov.append({'Key': key, 'Operation': text, 'Orders': cmds, 'R7A': a, 'R7B': b})
    cases.append(case('M', 'M-A67-COVERAGE', 'Operaciones de A6-7, regla 1, que cada variante deja ejercer al instrumento en la medicion',
                      'R7-A: todas las que dependen de una orden; R7-B: sin hashes, sin la lectura de JSON y texto en el Target y del contrato, sin clausemap',
                      {'Rows': cov}, all(r['R7A'] in ('si',) or r['Orders'] is None for r in cov)
                      and [r['R7B'] for r in cov if r['Orders'] and r['Orders'][0] in ('hashes', 'clausemap')] == ['no', 'no']))
    deliver('M', [reqA, reqI, reqBl, dres['RecordPath'], dres['ClosurePath']])
    write_group('p8-m.json', {'Schema': 'm7-bench/p8-m', 'Clone': T, 'R7BExcludedOrders': excl, 'R7BLimitedOrders': names_b, 'Coverage': cov,
                              'Cases': cases})


# ------------------------------------------------------------------ summary and README
RESULTS = ('p0-env.json', 'p1-build.json', 'p2-v.json', 'p3-n4.json', 'p4-nc1.json', 'p5-n5.json', 'p6-n6.json', 'p7-n7.json', 'p8-m.json')
NEW = ('make_eic_verify.py', 'designate_m7.py', 'table_m7.py', 'lib_m7.py', 'run_m7.py', 'make_readme_m7.py')
REUSED = ('derive_r4.py', 'lib_r1r4.py', 'lib_r3.py', 'lib_r4.py', 'reconstruct_runs.py', 'table_r3.py', 'table_r4.py')


def group_summary():
    allc, files = [], {}
    for n in RESULTS:
        p = os.path.join(HERE, n)
        if not os.path.isfile(p):
            continue
        d = json.loads(L.rbytes(p).decode('ascii'))
        files[n] = L.sha256(L.rbytes(p))
        allc += [{'Group': c['Group'], 'Case': c['Case'], 'Expected': c['Expected'], 'Holds': c['Holds']} for c in d.get('Cases', [])]
    for n in NEW + REUSED:
        files[n] = L.sha256(L.rbytes(os.path.join(HERE, n)))
    for root, _d, fs in os.walk(os.path.join(HERE, 'designations')):
        for f in sorted(fs):
            files['designations/' + f] = L.sha256(L.rbytes(os.path.join(root, f)))
    write_group('summary.json', {'Schema': 'm7-bench/summary', 'Cases': allc, 'Total': len(allc), 'Holding': sum(1 for c in allc if c['Holds']),
                                 'NotHolding': [c['Case'] for c in allc if not c['Holds']], 'Files': files, 'Versions': L.versions()})


def group_readme():
    RM = _load('make_readme_m7')
    RM.main()


GROUPS = {'env': group_env, 'prep': group_prep, 'build': group_build, 'v': group_v, 'n4': group_n4, 'nc1': group_nc1, 'n5': group_n5,
          'n6': group_n6, 'n7': group_n7, 'm': group_m, 'summary': group_summary, 'readme': group_readme}
ORDER = ['prep', 'env', 'build', 'v', 'n4', 'nc1', 'n5', 'n6', 'n7', 'm', 'summary', 'readme']

if __name__ == '__main__':
    which = sys.argv[1] if len(sys.argv) > 1 else 'all'
    for g in (ORDER if which == 'all' else [which]):
        GROUPS[g]()
