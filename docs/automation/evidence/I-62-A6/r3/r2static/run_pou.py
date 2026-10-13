# run_pou.py - Part B of the I-62 A-6 round 3 bench (r2static): point-of-use checks (i)-(viii) of pou_check.py on throwaway
# clones of the fixture. Writes only under D:\r62-a6r3\pou and part-b-pou.json next to this script. Sequential. No model, no
# codex/claude CLI, no network. Planted files are never executed (pou_check.py calls git by full path; resolution is
# Get-Command only). Run: python -I run_pou.py
import copy, importlib.util, json, os, platform, shutil, stat, subprocess, sys, time

HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


L = _load('benchlib')
P = _load('pou_check')          # only resolve_executable() is reused here, to describe the environment

WORK = 'D:/r62-a6r3/pou'
CASES = WORK + '/cases'
AUX = WORK + '/side/a1'          # never 'aux': a reserved device name for Win32 paths (git clone fails with EINVAL)
AUXCFG = WORK + '/side-cfg'
MARKERS = WORK + '/markers'
ABSENT = WORK + '/absent'
ORIGIN = 'D:/r62-fixture/fixture-origin.git'
BRANCH = 'fx/u1'
B = '9a829b0eeb1210238052b1feac06cfa8f7964aed'
R2_PIN_SHA256 = 'b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200'   # round 2 README, bmin_facts.py
R2_EXE_SHA256 = 'ad169f4cb4bfb78c7a5c030a4529c19d6643276778e33994c93e145b6191c3ec'   # round 2 inputs-t1/request.json Interpreter
RUNTIME_PWSH_SHA256 = '362a356ce7f0940ec74f73a8fc2c990a2cc24a38a11c90bbd8eca947110ad139'   # OD-2 v4/v5 runtime-pwsh.sha256
POU = os.path.join(HERE, 'pou_check.py')
GIT = L.GIT
SHELL = L.path_lookup('pwsh')
INTERP = L.path_lookup('python')


def rmtree(p):
    def onexc(f, q, e):
        os.chmod(q, stat.S_IWRITE)
        f(q)
    if os.path.isdir(p):
        shutil.rmtree(p, onexc=onexc)


def git(*a, inp=None, env=None):
    return L.run([GIT] + [str(x) for x in a], inp=inp, env=env or L.genv())


def first_line(argv, cwd=None):
    p = L.run(argv, env=L.genv(), cwd=cwd or HERE, timeout=120)
    return p.stdout.decode('utf-8', 'replace').strip().splitlines()[0] if p.stdout.strip() else None


def exe_record(path, version_argv=None):
    rec = {'Path': path}
    try:
        tgt, r = P.resolve_executable(path)
        rec.update({'Resolution': r, 'HashedFile': tgt, 'Sha256': L.sha256(L.rbytes(tgt)), 'Size': os.path.getsize(tgt)})
    except Exception as e:
        rec['Error'] = str(e)
    if version_argv:
        rec['Version'] = first_line(version_argv)
    return rec


def environment():
    base_launch = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    git_rec = exe_record(GIT, [GIT, '--version'])
    real_git = os.path.join(os.path.dirname(os.path.dirname(GIT)), 'mingw64', 'bin', 'git.exe')
    real_rec = exe_record(real_git, [real_git, '--version']) if os.path.isfile(real_git) else None
    py_rec = exe_record(INTERP, [INTERP, '--version'])
    sh_rec = exe_record(SHELL, [SHELL, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command',
                                "[Console]::Out.Write($PSVersionTable.PSVersion.ToString() + ' ' + $ExecutionContext.SessionState.LanguageMode)"])
    alias2 = os.path.join(os.environ.get('LOCALAPPDATA', ''), 'Microsoft', 'WindowsApps', 'pwsh.exe')
    return {
        'Os': platform.platform(), 'BenchPython': {'Version': platform.python_version(), 'SysExecutable': sys.executable,
                                                    'BasePrefixExe': os.path.join(sys.base_prefix, os.path.basename(sys.executable))},
        'Git': git_rec, 'GitRealBinaryBehindLauncher': real_rec, 'Interpreter': py_rec, 'Shell': sh_rec,
        'ShellAliasInWindowsApps': exe_record(alias2) if os.path.lexists(alias2) else None,
        'InterpreterSha256EqualsRound2ExecutableSha256': py_rec.get('Sha256') == R2_EXE_SHA256,
        'ShellSha256EqualsCodexRuntimePwshSha256': sh_rec.get('Sha256') == RUNTIME_PWSH_SHA256,
        'NoDefaultCurrentDirectoryInExePath': os.environ.get('NoDefaultCurrentDirectoryInExePath'),
        'PATHEXT': os.environ.get('PATHEXT'),
        'InheritedGitVariables': dict((k, v) for k, v in os.environ.items() if k.upper().startswith('GIT_')),
        'BaseLaunchEnvHasGitVariables': any(k.upper().startswith('GIT_') for k in base_launch),
    }


def pinned_bytes():
    p = git('--no-replace-objects', '-C', L.WT, 'cat-file', 'blob', B)
    if p.returncode != 0 or L.blob_sha1(p.stdout) != B:
        raise SystemExit('pinned blob not readable or wrong')
    return p.stdout


def clone(dest):
    rmtree(dest)
    L.ensure_dir(os.path.dirname(dest))
    p = git('clone', '-q', '--no-hardlinks', '-c', 'core.autocrlf=false', '-b', BRANCH, ORIGIN, dest)
    if p.returncode != 0:
        raise SystemExit('clone failed: ' + p.stderr.decode('utf-8', 'replace'))
    return dest


def p1(T, data):
    before = L.snapshot(GIT, T)
    t = L.utc()
    p = git('-C', T, 'hash-object', '-w', '--no-filters', '--stdin', inp=data)
    oid = p.stdout.decode('ascii', 'replace').strip()
    after = L.snapshot(GIT, T)
    d = L.snapdiff(before, after)
    only_new = set(d) == {'GitFiles'} and d['GitFiles'] == {'Added': ['objects/' + B[:2] + '/' + B[2:]], 'Removed': [], 'Modified': []}
    return {'Repo': T, 'UtcTime': t, 'SourceCommand': 'git --no-replace-objects -C <I-62 worktree> cat-file blob ' + B,
            'WriteCommand': 'git -C ' + T + ' hash-object -w --no-filters --stdin', 'WriteExit': p.returncode, 'ReturnedId': oid,
            'IdEqualsPinned': oid == B, 'OnlyNewLooseObject': only_new, 'SnapshotDiff': d, 'Held': oid == B and only_new}


def baseline_decl(env_rec):
    return {'Schema': 'i62-a6r3-pou-decl/0-bench', 'Blob': B,
            'Git': {'Path': GIT, 'Version': env_rec['Git']['Version'], 'Sha256': env_rec['Git']['Sha256'], 'InvocationName': 'git'},
            'Interpreter': {'Path': INTERP, 'Version': env_rec['Interpreter']['Version'], 'Sha256': env_rec['Interpreter']['Sha256'],
                            'InvocationName': 'python'},
            'Shell': {'Path': SHELL, 'Kind': 'pwsh', 'Sha256': env_rec['Shell']['Sha256'], 'Version': env_rec['Shell']['Version']}}


def flip(h):
    return h[:-1] + ('0' if h[-1] != '0' else '1')


def wtext(p, s):
    L.ensure_dir(os.path.dirname(p))
    with open(p, 'w', encoding='ascii', newline='') as fh:
        fh.write(s)


def marker_bat(case, name):
    m = (MARKERS + '/' + case + '-' + name + '.txt').replace('/', '\\')
    return '@echo off\r\nrem planted by the I-62 A-6 r3 bench; must never be executed\r\necho executed>"' + m + '"\r\n'


def marker_ps1(case, name):
    m = (MARKERS + '/' + case + '-' + name + '.txt').replace('/', '\\')
    return "# planted by the I-62 A-6 r3 bench; must never be executed\r\nSet-Content -LiteralPath '" + m + "' -Value executed\r\n"


# ------------------------------------------------------------------ cases
def plant_file(name, content_fn=None):
    def f(T, case):
        p = os.path.join(T, name)
        if content_fn is None:
            open(p, 'wb').close()
        else:
            wtext(p, content_fn(case, name))
        return {'File': name, 'Bytes': os.path.getsize(p), 'Kind': 'empty' if content_fn is None else 'harmless text (marker writer if executed)'}
    return f


def append_config(text):
    def f(T, case):
        p = os.path.join(T, '.git', 'config')
        with open(p, 'ab') as fh:
            fh.write(text.encode('ascii'))
        return {'AppendedToLocalConfig': text}
    return f


def git_config(*cmds):
    def f(T, case):
        out = []
        for c in cmds:
            p = git('-C', T, 'config', *c)
            out.append({'Args': list(c), 'Exit': p.returncode})
        return {'GitConfig': out}
    return f


def write_worktree_config_only(T, case):
    p = os.path.join(T, '.git', 'config.worktree')
    wtext(p, '[bench]\n\tplanted = 1\n')
    return {'WroteFile': '.git/config.worktree', 'ExtensionsWorktreeConfig': 'not set'}


def decl_set(path, value):
    def f(d):
        cur = d
        for k in path[:-1]:
            cur = cur[k]
        old = cur.get(path[-1])
        if value is None:
            cur.pop(path[-1], None)
        else:
            cur[path[-1]] = value(old) if callable(value) else value
        return {'Field': '.'.join(path), 'Declared': None if value is None else cur[path[-1]], 'Removed': value is None}
    return f


def case_list(env_rec):
    real_git = env_rec['GitRealBinaryBehindLauncher'] or {}
    benign_inc = AUXCFG + '/benign.inc'
    C = []
    def add(cid, group, step, desc, decl=None, env=None, repo=None, launch_true=False, reuse=None, no_registered=False):
        C.append({'Id': cid, 'Group': group, 'ExpectedStep': step, 'ExpectedLaunch': launch_true, 'Description': desc,
                  'Decl': decl, 'Env': env, 'Repo': repo, 'Reuse': reuse, 'NoRegistered': no_registered})
    add('B0', 'baseline', None, 'linea base: clon nuevo, P1, configuracion registrada, entorno de lanzamiento sin GIT_*', launch_true=True)
    add('v-a', 'v', 'v', 'SHA-256 declarado del interprete erroneo', decl=decl_set(['Interpreter', 'Sha256'], flip))
    add('v-b', 'v', 'v', 'version declarada del interprete erronea', decl=decl_set(['Interpreter', 'Version'], 'Python 3.13.13'))
    add('v-b2', 'v', 'v', '(extra) version declarada de git erronea', decl=decl_set(['Git', 'Version'], 'git version 2.53.0.windows.1'))
    add('v-c', 'v', 'v', 'SHA-256 declarado de git erroneo', decl=decl_set(['Git', 'Sha256'], flip))
    add('v-d', 'v', 'v', 'ruta declarada del interprete inexistente', decl=decl_set(['Interpreter', 'Path'], os.path.normpath(ABSENT + '/python.exe')))
    add('v-d2', 'v', 'v', '(extra) ruta declarada de git inexistente', decl=decl_set(['Git', 'Path'], os.path.normpath(ABSENT + '/git.exe')))
    if real_git.get('Sha256'):
        def ve(d):
            d['Git'].update({'Path': real_git['Path'], 'Sha256': real_git['Sha256'], 'Version': real_git['Version']})
            return {'Field': 'Git.{Path,Sha256,Version}', 'Declared': d['Git']}
        add('v-e', 'v', 'v', '(extra) git declarado = binario real detras del lanzador (SHA y version correctos), pero la shell resuelve otro', decl=ve)
    add('vi-0', 'vi', 'vi', '(extra) entorno heredado de la sesion del banco, sin depurar', env=lambda e: dict(os.environ))
    add('vi-a', 'vi', 'vi', 'GIT_DIR (hacia un clon auxiliar con el objeto fijado)', env=lambda e: dict(e, GIT_DIR=os.path.normpath(AUX + '/.git')))
    add('vi-b', 'vi', 'vi', 'GIT_ALTERNATE_OBJECT_DIRECTORIES (objetos del clon auxiliar)',
        env=lambda e: dict(e, GIT_ALTERNATE_OBJECT_DIRECTORIES=os.path.normpath(AUX + '/.git/objects')))
    add('vi-c', 'vi', 'vi', "GIT_CONFIG_PARAMETERS ('bench.planted'='1')", env=lambda e: dict(e, GIT_CONFIG_PARAMETERS="'bench.planted'='1'"))
    add('vi-d', 'vi', 'vi', 'GIT_REPLACE_REF_BASE (refs/bench-replace/)', env=lambda e: dict(e, GIT_REPLACE_REF_BASE='refs/bench-replace/'))
    add('vii-a', 'vii', 'vii', 'include.path en .git/config (archivo incluido benigno)',
        repo=append_config('[include]\n\tpath = ' + benign_inc + '\n'))
    add('vii-b', 'vii', 'vii', 'includeIf en .git/config',
        repo=append_config('[includeIf "gitdir:D:/r62-a6r3/"]\n\tpath = ' + benign_inc + '\n'))
    add('vii-c', 'vii', 'vii', 'valor cambiado frente a la instantanea registrada (core.useReplaceRefs=false)',
        repo=git_config(('--local', 'core.useReplaceRefs', 'false')))
    add('vii-d', 'vii', 'vii', 'config.worktree con extensions.worktreeConfig=true',
        repo=git_config(('--local', 'extensions.worktreeConfig', 'true'), ('--worktree', 'bench.planted', '1')))
    add('vii-e', 'vii', 'vii', '(extra) config.worktree sin extensions.worktreeConfig (git lo ignora)', repo=write_worktree_config_only)
    add('viii-a', 'viii', 'viii', 'git.exe vacio en la raiz del arbol de trabajo', repo=plant_file('git.exe'))
    add('viii-b', 'viii', 'viii', 'git.cmd (texto que escribiria un marcador si se ejecutara)', repo=plant_file('git.cmd', marker_bat))
    add('viii-c', 'viii', 'viii', 'git.bat (texto que escribiria un marcador si se ejecutara)', repo=plant_file('git.bat', marker_bat))
    add('viii-d', 'viii', 'viii', 'python.exe vacio', repo=plant_file('python.exe'))
    add('viii-e', 'viii', 'viii', 'nombre exacto del interprete (destino del alias: python3.13.exe) vacio', repo=plant_file('python3.13.exe'))
    add('viii-f', 'viii', 'viii', '(extra) Git.PS1 (mayusculas/minusculas y .ps1; marcador si se ejecutara)', repo=plant_file('Git.PS1', marker_ps1))
    add('FC-a', 'fail-closed', 'iii', 'shell declarada inexistente: (iii) no se puede establecer', decl=decl_set(['Shell', 'Path'], os.path.normpath(ABSENT + '/pwsh.exe')))
    add('FC-b', 'fail-closed', 'v', 'declaracion sin Sha256 del interprete: (v) no se puede establecer', decl=decl_set(['Interpreter', 'Sha256'], None))
    add('FC-c', 'fail-closed', 'vii', 'sin instantanea registrada de la configuracion: (vii) no se puede establecer', no_registered=True)
    add('B1', 'baseline', None, 'repeticion de la linea base sobre el clon de B0 al final (sin clon nuevo ni P1 nueva)', launch_true=True, reuse='B0')
    return C


def step_detail(res, step):
    s = (res.get('Steps') or {}).get(step) or {}
    if step == 'v':
        out = {}
        for role in ('Git', 'Interpreter'):
            r = s.get(role) or {}
            out[role] = {'Sub': r.get('Sub'), 'MeasuredSha256': r.get('MeasuredSha256'), 'HashedFile': r.get('HashedFile'),
                         'Version': (r.get('VersionCommand') or {}).get('Stdout'), 'ShellResolution': r.get('ShellResolution'),
                         'Error': r.get('Error') or r.get('ResolutionError')}
        return out
    if step == 'vi':
        return {'GitVariables': s.get('GitVariables')}
    if step == 'vii':
        return {'Sub': s.get('Sub'), 'Diff': s.get('Diff'), 'IncludeFoundIn': s.get('IncludeFoundIn'), 'Error': s.get('Error')}
    if step == 'viii':
        return {'Matches': s.get('Matches'), 'Stems': s.get('Stems'), 'Error': s.get('Error')}
    if step == 'iii':
        return {k: s.get(k) for k in ('Type', 'Hash', 'TypeExit', 'HashExit', 'Error')}
    return s


def run_case(c, decl0, data, base_env, state):
    cid = c['Id']
    T = os.path.normpath(CASES + '/' + cid) if not c['Reuse'] else state[c['Reuse']]['Repo']
    meta = os.path.normpath(CASES + '/' + cid + '.meta')
    rmtree(meta)
    L.ensure_dir(meta)
    rec = {'Id': cid, 'Group': c['Group'], 'Description': c['Description'], 'Repo': T,
           'Expected': {'Launch': c['ExpectedLaunch'], 'FailingStep': c['ExpectedStep']}}
    if not c['Reuse']:
        clone(T)
        rec['P1'] = p1(T, data)
        if not rec['P1']['Held']:
            rec['Aborted'] = 'P1 did not hold'
            return rec
    # preparation write: register the config snapshot with the declared (baseline) git
    base_decl_path = os.path.join(meta, 'decl-baseline.json')
    L.wbytes(base_decl_path, (json.dumps(decl0, ensure_ascii=True, indent=1, sort_keys=True) + '\n').encode('ascii'))
    regp = os.path.join(meta, 'registered-config.json')
    if c['Reuse']:
        shutil.copyfile(state[c['Reuse']]['Registered'], regp)
    else:
        p = L.run([sys.executable, '-I', POU, '--snapshot-config', '--repo', T, '--decl', base_decl_path], env=base_env, cwd=meta)
        if p.returncode != 0:
            rec['Aborted'] = 'config registration failed: ' + p.stderr.decode('utf-8', 'replace')[:300]
            return rec
        L.wbytes(regp, p.stdout)
    rec['RegisteredConfigSha256'] = L.sha256(L.rbytes(regp))
    registered_path = regp
    if c['NoRegistered']:
        registered_path = os.path.join(meta, 'registered-config.MISSING.json')
    # plant
    decl = copy.deepcopy(decl0)
    planted = {}
    if c['Decl']:
        planted['Decl'] = c['Decl'](decl)
    env = dict(base_env)
    if c['Env']:
        env = c['Env'](env)
        planted['EnvGitVariables'] = dict((k, v) for k, v in env.items() if k.upper().startswith('GIT_'))
    if c['Repo']:
        planted['Repo'] = c['Repo'](T, cid)
    if c['NoRegistered']:
        planted['RegisteredConfig'] = 'not provided (path does not exist)'
    rec['Planted'] = planted
    decl_path = os.path.join(meta, 'decl.json')
    L.wbytes(decl_path, (json.dumps(decl, ensure_ascii=True, indent=1, sort_keys=True) + '\n').encode('ascii'))
    # run the check in the launch environment, from a neutral directory
    before = L.snapshot(GIT, T)
    out = os.path.join(meta, 'result.json')
    t0 = time.time()
    p = L.run([sys.executable, '-I', POU, '--repo', T, '--decl', decl_path, '--registered-config', registered_path, '--out', out],
              env=env, cwd=meta, timeout=3600)
    dt = round(time.time() - t0, 1)
    after = L.snapshot(GIT, T)
    d = L.snapdiff(before, after)
    try:
        res = json.loads(L.rbytes(out).decode('ascii'))
    except Exception as e:
        res = {'Unreadable': type(e).__name__, 'Stderr': p.stderr.decode('utf-8', 'replace')[:500]}
    markers = sorted(os.listdir(MARKERS)) if os.path.isdir(MARKERS) else []
    obs_launch = res.get('Launch') is True and p.returncode == 0
    failed = res.get('FailedSteps')
    exp_step = c['ExpectedStep']
    holds = (obs_launch == c['ExpectedLaunch']) and ((exp_step is None and failed == []) or (exp_step is not None and exp_step in (failed or [])))
    rec['Observed'] = {'Rc': p.returncode, 'Launch': res.get('Launch'), 'FailedSteps': failed, 'UnestablishedSteps': res.get('UnestablishedSteps'),
                       'StepStatus': dict((k, v.get('Status')) for k, v in (res.get('Steps') or {}).items()),
                       'ExpectedStepDetail': step_detail(res, exp_step) if exp_step else None, 'Seconds': dt,
                       'ResultSha256': L.sha256(L.rbytes(out)) if os.path.isfile(out) else None, 'ResultPath': out,
                       'StderrHead': p.stderr.decode('utf-8', 'replace')[:300]}
    rec['OtherFailedSteps'] = [x for x in (failed or []) if x != exp_step]
    rec['CheckZeroWriteInRepo'] = d == {}
    rec['CheckSnapshotDiff'] = d
    rec['MarkerFilesPresent'] = markers
    rec['Holds'] = bool(holds) and d == {} and not markers
    state[cid] = {'Repo': T, 'Registered': regp}
    return rec


def main():
    if not (GIT and SHELL and INTERP):
        raise SystemExit('git, pwsh or python not found on PATH')
    L.ensure_dir(WORK)
    for d in (CASES, MARKERS, AUXCFG):
        rmtree(d) if d == MARKERS else None
        L.ensure_dir(d)
    env_rec = environment()
    data = pinned_bytes()
    pin = {'Blob': B, 'BlobSha1Recomputed': L.blob_sha1(data), 'Sha256': L.sha256(data), 'Size': len(data),
           'Sha256EqualsRound2': L.sha256(data) == R2_PIN_SHA256}
    wtext(AUXCFG + '/benign.inc', '[bench]\n\tincluded = 1\n')
    clone(AUX)
    aux_p1 = p1(os.path.normpath(AUX), data)
    decl0 = baseline_decl(env_rec)
    base_env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    state, recs = {}, []
    for c in case_list(env_rec):
        r = run_case(c, decl0, data, base_env, state)
        recs.append(r)
        print(r['Id'], 'Launch=%s' % (r.get('Observed') or {}).get('Launch'), 'Failed=%s' % (r.get('Observed') or {}).get('FailedSteps'),
              'Holds=%s' % r.get('Holds'), r.get('Aborted') or '', flush=True)
    b0 = next(r for r in recs if r['Id'] == 'B0')
    reg_b0 = json.loads(L.rbytes(os.path.normpath(CASES + '/B0.meta/registered-config.json')).decode('ascii'))
    out = {'Schema': 'i62-a6r3-part-b-pou/1', 'GeneratedUtc': L.utc(), 'Environment': env_rec, 'Pin': pin, 'AuxClone': {'Repo': AUX, 'P1': aux_p1},
           'BaselineDeclaration': decl0, 'BaselineLaunchEnv': {'GitVariables': [], 'Construction': 'environment of the bench process minus every GIT_* name'},
           'RegisteredConfigB0': reg_b0, 'PouCheckSha256': L.sha256(L.rbytes(POU)), 'Cases': recs,
           'Summary': {'Cases': len(recs), 'Holds': sum(1 for r in recs if r.get('Holds')), 'NotHolding': [r['Id'] for r in recs if not r.get('Holds')],
                       'WithOtherFailedSteps': dict((r['Id'], r['OtherFailedSteps']) for r in recs if r.get('OtherFailedSteps')),
                       'Aborted': [r['Id'] for r in recs if r.get('Aborted')], 'AllZeroWrite': all(r.get('CheckZeroWriteInRepo') for r in recs),
                       'MarkersEverCreated': sorted(set(m for r in recs for m in r.get('MarkerFilesPresent', [])))}}
    p, h = L.write_result('part-b-pou.json', out)
    print(p, h)


if __name__ == '__main__':
    main()
