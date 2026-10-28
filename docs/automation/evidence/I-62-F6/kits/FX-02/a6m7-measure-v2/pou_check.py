# pou_check.py - point-of-use identity check (A-6 rule A6-3, rule 3, steps (i)-(viii)) run by the supervision immediately
# before each VERIFY launch. Bench candidate of the I-62 A-6 round 3 (r2static). Stdlib only, Windows.
#
#   python -I pou_check.py --repo <the invocation's -C> --decl <decl.json> --registered-config <cfg.json> [--out <result.json>]
#
# The launch environment is THIS process's environment: run the check in exactly the environment the session will use to
# launch the invocation (or pass --launch-env-json with that environment as a JSON object).
#
# decl.json (the disposition):
#   {"Blob": "<pinned blob>",
#    "Git":         {"Path": "<full path>", "Version": "<exact --version line>", "Sha256": "<hex>", "InvocationName": "git"},
#    "Interpreter": {"Path": "<full path>", "Version": "<exact --version line>", "Sha256": "<hex>", "InvocationName": "python"},
#    "Shell":       {"Path": "<full path of the invocation's shell (pwsh)>"}}
# registered-config.json: the snapshot written at the preparation write (see snapshot_config()).
#
# Steps run with read-only git commands and file-existence tests only. The supervision's own git calls use the DECLARED git
# by full path, with every GIT_* variable removed (so a planted GIT_DIR cannot redirect the checks themselves). Step (iii)
# runs through the declared shell, with cwd = repo and the launch environment unchanged, and calls git by its declared full
# path, so nothing is ever resolved from the working directory. No file in the working tree is ever executed.
#
# Fail closed: a step that cannot be established is UNESTABLISHED and counts as failed. Exit code 0 = Launch true,
# 1 = Launch false, 2 = internal error (no verdict). Only exit 0 together with a readable result whose Launch is true may
# be taken as permission to launch; anything else (no output, unparsable output, another exit code) means no launch.
import argparse, ctypes, datetime, hashlib, json, os, re, struct, subprocess, sys, traceback

DEVNULL = subprocess.DEVNULL
DEFAULT_PATHEXT = '.COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC;.CPL'
IO_REPARSE_TAG_APPEXECLINK = 0x8000001B
IO_REPARSE_TAG_SYMLINK = 0xA000000C


def utc():
    return datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%fZ')


def sha256(b):
    return hashlib.sha256(b).hexdigest()


def rbytes(p):
    with open(p, 'rb') as fh:
        return fh.read()


def strip_git(env):
    e = dict((k, v) for k, v in env.items() if not k.upper().startswith('GIT_'))
    e['GIT_OPTIONAL_LOCKS'] = '0'
    e['GIT_TERMINAL_PROMPT'] = '0'
    return e


class Unestablished(Exception):
    pass


# ------------------------------------------------------------------ executables: what the declared path really is
def _appexeclink_target(path):
    """Target executable of a Windows app execution alias (reparse tag 0x8000001B), read with FSCTL_GET_REPARSE_POINT."""
    from ctypes import wintypes as W
    k32 = ctypes.WinDLL('kernel32', use_last_error=True)
    k32.CreateFileW.restype = W.HANDLE
    k32.CreateFileW.argtypes = [W.LPCWSTR, W.DWORD, W.DWORD, W.LPVOID, W.DWORD, W.DWORD, W.HANDLE]
    k32.DeviceIoControl.argtypes = [W.HANDLE, W.DWORD, W.LPVOID, W.DWORD, W.LPVOID, W.DWORD, ctypes.POINTER(W.DWORD), W.LPVOID]
    k32.CloseHandle.argtypes = [W.HANDLE]
    h = k32.CreateFileW(path, 0x80, 7, None, 3, 0x00200000 | 0x02000000, None)   # FILE_READ_ATTRIBUTES; OPEN_REPARSE_POINT|BACKUP_SEMANTICS
    if h is None or h == W.HANDLE(-1).value:
        raise Unestablished('cannot open reparse point (error %d)' % ctypes.get_last_error())
    try:
        buf = ctypes.create_string_buffer(16384)
        n = W.DWORD(0)
        if not k32.DeviceIoControl(h, 0x000900A8, None, 0, buf, 16384, ctypes.byref(n), None):
            raise Unestablished('FSCTL_GET_REPARSE_POINT failed (error %d)' % ctypes.get_last_error())
        data = buf.raw[:n.value]
    finally:
        k32.CloseHandle(h)
    tag, ln, _ = struct.unpack_from('<IHH', data, 0)
    if tag != IO_REPARSE_TAG_APPEXECLINK:
        raise Unestablished('unexpected reparse tag %s' % hex(tag))
    payload = data[8:8 + ln]
    ver = struct.unpack_from('<I', payload, 0)[0]
    strs = payload[4:].decode('utf-16-le').split('\0')
    if ver != 3 or len(strs) < 3 or not strs[2]:
        raise Unestablished('unrecognised app execution alias layout (version %d)' % ver)
    return strs[2], {'PackageFamilyName': strs[0], 'AppUserModelId': strs[1]}


def resolve_executable(path):
    """Returns (file whose bytes are hashed, record). Raises Unestablished when the bytes cannot be established."""
    rec = {'DeclaredPath': path}
    if not path or not isinstance(path, str):
        raise Unestablished('no declared path')
    if not os.path.lexists(path):
        rec['Exists'] = False
        raise Unestablished('declared path does not exist')
    st = os.lstat(path)
    tag = getattr(st, 'st_reparse_tag', 0)
    rec.update({'Exists': True, 'ReparseTag': hex(tag), 'IsDirectory': os.path.isdir(path)})
    if os.path.isdir(path):
        raise Unestablished('declared path is a directory')
    if tag == 0:
        rec['Kind'] = 'regular-file'
        return path, rec
    if tag == IO_REPARSE_TAG_APPEXECLINK:
        tgt, info = _appexeclink_target(path)
        rec.update({'Kind': 'app-execution-alias', 'AliasTarget': tgt, 'Alias': info})
        if not os.path.isfile(tgt):
            raise Unestablished('app execution alias target is not a readable file')
        return tgt, rec
    if tag == IO_REPARSE_TAG_SYMLINK:
        tgt = os.path.realpath(path)
        rec.update({'Kind': 'symlink', 'LinkTarget': tgt})
        if not os.path.isfile(tgt):
            raise Unestablished('symlink target is not a readable file')
        return tgt, rec
    raise Unestablished('reparse point of an unsupported kind: %s' % hex(tag))


def check_executable(role, d, launch_env, repo, shell_path, workdir):
    out = {'Role': role}
    sub = {}
    try:
        for f in ('Path', 'Version', 'Sha256', 'InvocationName'):
            if not isinstance(d.get(f), str) or not d.get(f):
                raise Unestablished('declaration lacks ' + f)
        out['Declared'] = dict((k, d[k]) for k in ('Path', 'Version', 'Sha256', 'InvocationName'))
        # path and bytes
        try:
            hashed, rec = resolve_executable(d['Path'])
            out['Resolution'] = rec
            b = rbytes(hashed)
            out['HashedFile'] = hashed
            out['MeasuredSha256'] = sha256(b)
            out['MeasuredSize'] = len(b)
            sub['Sha256'] = 'PASS' if out['MeasuredSha256'] == d['Sha256'].lower() else 'FAIL'
            out['HashedFileName'] = os.path.basename(hashed)
        except Unestablished as e:
            out['ResolutionError'] = str(e)
            sub['Path'] = 'FAIL' if 'does not exist' in str(e) else 'UNESTABLISHED'
            sub['Sha256'] = 'UNESTABLISHED'
        if 'Path' not in sub:
            sub['Path'] = 'PASS'
        # version: the declared executable itself, by full path, outside the working tree, GIT_* removed
        if sub['Path'] == 'PASS':
            try:
                p = subprocess.run([d['Path'], '--version'], stdin=DEVNULL, capture_output=True, cwd=workdir, env=strip_git(launch_env), timeout=120)
                v = p.stdout.decode('utf-8', 'replace').strip()
                out['VersionCommand'] = {'Exit': p.returncode, 'Stdout': v, 'StderrHead': p.stderr.decode('utf-8', 'replace')[:200]}
                sub['Version'] = 'PASS' if (p.returncode == 0 and v == d['Version']) else 'FAIL'
            except Exception as e:
                out['VersionCommand'] = {'Error': type(e).__name__ + ': ' + str(e)[:200]}
                sub['Version'] = 'UNESTABLISHED'
        else:
            sub['Version'] = 'UNESTABLISHED'
        # what the invocation's shell resolves for the bare name, with cwd = repo and the launch environment (resolution only)
        if not shell_path or not os.path.isfile(shell_path):
            sub['ResolvedByShell'] = 'UNESTABLISHED'
            out['ShellResolution'] = {'Error': 'declared shell not found'}
        else:
            name = d['InvocationName']
            if not re.fullmatch(r'[A-Za-z0-9_.-]+', name):
                raise Unestablished('unsafe invocation name')
            script = ("$c = Get-Command -Name '%s' -CommandType Application -ErrorAction Stop | Select-Object -First 1; "
                      "[Console]::Out.Write($c.Source)") % name
            p = subprocess.run([shell_path, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', script], stdin=DEVNULL,
                               capture_output=True, cwd=repo, env=launch_env, timeout=120)
            src = p.stdout.decode('utf-8', 'replace').strip()
            out['ShellResolution'] = {'Exit': p.returncode, 'Resolved': src}
            if p.returncode != 0 or not src:
                sub['ResolvedByShell'] = 'UNESTABLISHED'
            else:
                same = os.path.normcase(os.path.normpath(src)) == os.path.normcase(os.path.normpath(d['Path']))
                sub['ResolvedByShell'] = 'PASS' if same else 'FAIL'
    except Unestablished as e:
        out['Error'] = str(e)
        sub.setdefault('Declaration', 'UNESTABLISHED')
    except Exception as e:
        out['Error'] = type(e).__name__ + ': ' + str(e)[:300]
        sub.setdefault('Internal', 'UNESTABLISHED')
    out['Sub'] = sub
    out['Ok'] = bool(sub) and all(v == 'PASS' for v in sub.values())
    return out


# ------------------------------------------------------------------ repository config
def snapshot_config(git, repo):
    """Local and worktree config as registered at the preparation write (also used for the point-of-use comparison)."""
    env = strip_git(os.environ)
    def g(*a):
        p = subprocess.run([git, '-C', repo] + list(a), stdin=DEVNULL, capture_output=True, env=env, timeout=120)
        if p.returncode != 0:
            raise Unestablished('git %s failed: %s' % (' '.join(a), p.stderr.decode('utf-8', 'replace')[:200]))
        return p.stdout.decode('utf-8', 'replace').strip()
    gd = os.path.normpath(g('rev-parse', '--path-format=absolute', '--git-dir'))
    cd = os.path.normpath(g('rev-parse', '--path-format=absolute', '--git-common-dir'))
    files = {'Local': os.path.join(cd, 'config'), 'Worktree': os.path.join(gd, 'config.worktree')}
    rec = {'GitDir': gd, 'CommonDir': cd, 'Files': {}}
    for role, path in files.items():
        f = {'Path': path, 'Exists': os.path.lexists(path)}
        if f['Exists']:
            if not os.path.isfile(path):
                raise Unestablished(role + ' config is not a regular file')
            b = rbytes(path)
            f['Sha256'] = sha256(b)
            f['Size'] = len(b)
            p = subprocess.run([git, 'config', '--file', path, '--no-includes', '--list', '-z'], stdin=DEVNULL, capture_output=True, env=env, timeout=120)
            if p.returncode != 0:
                raise Unestablished(role + ' config does not parse: ' + p.stderr.decode('utf-8', 'replace')[:200])
            entries = [x for x in p.stdout.decode('utf-8', 'replace').split('\0') if x]
            f['Entries'] = [e.replace('\n', '=', 1) for e in entries]
            f['IncludeKeys'] = [e.split('\n', 1)[0] for e in entries if e.split('\n', 1)[0].lower().startswith(('include.', 'includeif.'))]
            f['RawIncludeSection'] = bool(re.search(r'(?im)^\s*\[\s*include(if)?\b', b.decode('latin-1')))
        rec['Files'][role] = f
    return rec


def check_config(git, repo, registered):
    out = {}
    try:
        if not isinstance(registered, dict) or 'Files' not in registered:
            raise Unestablished('no registered config snapshot')
        cur = snapshot_config(git, repo)
        out['Current'] = cur
        sub = {}
        inc = []
        for role in ('Local', 'Worktree'):
            f = cur['Files'][role]
            r = registered['Files'].get(role)
            if r is None:
                raise Unestablished('registered snapshot lacks ' + role)
            same = (f['Exists'] == r['Exists']) and (f.get('Sha256') == r.get('Sha256'))
            sub[role + 'EqualsRegistered'] = 'PASS' if same else 'FAIL'
            if f.get('IncludeKeys') or f.get('RawIncludeSection'):
                inc.append(role)
            if not same:
                a, b = set(r.get('Entries') or []), set(f.get('Entries') or [])
                out.setdefault('Diff', {})[role] = {'ExistsRegistered': r['Exists'], 'ExistsNow': f['Exists'],
                                                    'EntriesAdded': sorted(b - a), 'EntriesRemoved': sorted(a - b)}
        sub['NoIncludeOrIncludeIf'] = 'FAIL' if inc else 'PASS'
        out['IncludeFoundIn'] = inc
        out['Sub'] = sub
        out['Ok'] = all(v == 'PASS' for v in sub.values())
    except Unestablished as e:
        out['Error'] = str(e)
        out['Sub'] = {'Config': 'UNESTABLISHED'}
        out['Ok'] = False
    except Exception as e:
        out['Error'] = type(e).__name__ + ': ' + str(e)[:300]
        out['Sub'] = {'Internal': 'UNESTABLISHED'}
        out['Ok'] = False
    return out


# ------------------------------------------------------------------ zero-write snapshot around fsck (round-2 logic)
def _walk(root, skip):
    out = {}
    sk = set(os.path.normcase(os.path.normpath(x)) for x in skip)
    for r, ds, fs in os.walk(root):
        ds[:] = [d for d in ds if os.path.normcase(os.path.normpath(os.path.join(r, d))) not in sk]
        for f in fs:
            q = os.path.join(r, f)
            b = rbytes(q)
            out[os.path.relpath(q, root).replace(os.sep, '/')] = [len(b), sha256(b)]
    return out


def tree_state(git, repo):
    env = strip_git(os.environ)
    def g(*a):
        return subprocess.run([git, '-C', repo] + list(a), stdin=DEVNULL, capture_output=True, env=env, timeout=300).stdout
    top = os.path.normpath(g('rev-parse', '--show-toplevel').decode('utf-8', 'replace').strip())
    cd = os.path.normpath(g('rev-parse', '--path-format=absolute', '--git-common-dir').decode('utf-8', 'replace').strip())
    return {'Refs': g('for-each-ref', '--format=%(refname) %(objectname)').decode('utf-8', 'replace'),
            'Status': sha256(g('status', '--porcelain=v1', '-z', '--ignored')),
            'Worktree': _walk(top, [os.path.join(top, '.git')]), 'GitFiles': _walk(cd, [])}


# ------------------------------------------------------------------ the check
def run_check(repo, decl, registered, launch_env, workdir):
    steps = {}
    blob = decl.get('Blob') if isinstance(decl, dict) else None
    git_d = (decl.get('Git') or {}) if isinstance(decl, dict) else {}
    interp_d = (decl.get('Interpreter') or {}) if isinstance(decl, dict) else {}
    shell = ((decl.get('Shell') or {}).get('Path')) if isinstance(decl, dict) else None
    git = git_d.get('Path')
    git_usable = isinstance(git, str) and os.path.isfile(git)
    genv = strip_git(launch_env)

    def gitrun(*a, env=None, timeout=300):
        return subprocess.run([git, '-C', repo] + list(a), stdin=DEVNULL, capture_output=True, env=env or strip_git(os.environ), timeout=timeout)

    # (vi) launch environment: no GIT_* variable (names compared case-insensitively)
    names = sorted(k for k in launch_env if k.upper().startswith('GIT_'))
    steps['vi'] = {'GitVariables': dict((k, launch_env[k]) for k in names), 'Ok': not names,
                   'NoDefaultCurrentDirectoryInExePath': launch_env.get('NoDefaultCurrentDirectoryInExePath', launch_env.get('NODEFAULTCURRENTDIRECTORYINEXEPATH'))}

    # (viii) root of the working directory: no file named like git or the interpreter with an executable extension
    try:
        pathext = launch_env.get('PATHEXT') or launch_env.get('Pathext')
        exts = set(x.strip().lower() for x in (pathext or DEFAULT_PATHEXT).split(';') if x.strip())
        exts |= set(x.lower() for x in DEFAULT_PATHEXT.split(';'))     # never narrower than the Windows default
        exts.add('.ps1')                                                # pwsh runs scripts by name too
        stems = set()
        for dd in (git_d, interp_d):
            if isinstance(dd.get('InvocationName'), str):
                stems.add(dd['InvocationName'].lower())
            if isinstance(dd.get('Path'), str):
                stems.add(os.path.splitext(os.path.basename(dd['Path']))[0].lower())
        try:
            tgt, _ = resolve_executable(interp_d.get('Path'))
            stems.add(os.path.splitext(os.path.basename(tgt))[0].lower())       # e.g. the alias target python3.13.exe
        except Unestablished:
            pass
        try:
            tgt, _ = resolve_executable(git)
            stems.add(os.path.splitext(os.path.basename(tgt))[0].lower())
        except Unestablished:
            pass
        if not stems:
            raise Unestablished('no names to look for')
        bad = sorted(n for n in os.listdir(repo) if any(n.lower() == s + e for s in stems for e in exts))
        steps['viii'] = {'Root': repo, 'Stems': sorted(stems), 'Extensions': sorted(exts), 'PathextFromLaunchEnv': pathext,
                         'Matches': bad, 'Ok': not bad}
    except Exception as e:
        steps['viii'] = {'Error': type(e).__name__ + ': ' + str(e)[:300], 'Ok': False, 'Status': 'UNESTABLISHED'}

    # (v) declared interpreter and git: path, version, SHA-256, and what the invocation's shell resolves
    vg = check_executable('Git', git_d, launch_env, repo, shell, workdir)
    vi_ = check_executable('Interpreter', interp_d, launch_env, repo, shell, workdir)
    steps['v'] = {'Git': vg, 'Interpreter': vi_, 'Ok': vg['Ok'] and vi_['Ok']}

    # (vii) repository config (local and worktree) = registered snapshot, without include / includeIf
    if git_usable:
        steps['vii'] = check_config(git, repo, registered)
    else:
        steps['vii'] = {'Error': 'declared git not usable', 'Sub': {'Config': 'UNESTABLISHED'}, 'Ok': False}

    if not git_usable:
        for k in ('i', 'ii', 'iii', 'iv'):
            steps[k] = {'Error': 'declared git not usable', 'Status': 'UNESTABLISHED', 'Ok': False}
    else:
        # (i) no ref under refs/replace/
        try:
            p = gitrun('for-each-ref', 'refs/replace/')
            o = p.stdout.decode('utf-8', 'replace')
            steps['i'] = {'Exit': p.returncode, 'Output': o.splitlines(), 'Ok': p.returncode == 0 and o.strip() == ''}
        except Exception as e:
            steps['i'] = {'Error': str(e)[:300], 'Status': 'UNESTABLISHED', 'Ok': False}
        # (ii) no info/grafts, no objects/info/alternates in the common dir; not shallow
        try:
            cdp = gitrun('rev-parse', '--path-format=absolute', '--git-common-dir')
            shp = gitrun('rev-parse', '--is-shallow-repository')
            if cdp.returncode != 0 or shp.returncode != 0:
                raise Unestablished('rev-parse failed')
            cd = os.path.normpath(cdp.stdout.decode('utf-8', 'replace').strip())
            gr = os.path.join(cd, 'info', 'grafts')
            al = os.path.join(cd, 'objects', 'info', 'alternates')
            sh = shp.stdout.decode('ascii', 'replace').strip()
            steps['ii'] = {'CommonDir': cd, 'GraftsExists': os.path.lexists(gr), 'AlternatesExists': os.path.lexists(al),
                           'ShallowFileExists': os.path.lexists(os.path.join(cd, 'shallow')), 'IsShallowRepository': sh,
                           'Ok': (not os.path.lexists(gr)) and (not os.path.lexists(al)) and sh == 'false'}
        except Exception as e:
            steps['ii'] = {'Error': str(e)[:300], 'Status': 'UNESTABLISHED', 'Ok': False}
        # (iii) through the invocation's shell, cwd = repo, launch environment unchanged, declared git by full path
        try:
            if not isinstance(blob, str) or not re.fullmatch(r'[0-9a-f]{40}', blob):
                raise Unestablished('no pinned blob in the declaration')
            if not shell or not os.path.isfile(shell):
                raise Unestablished('declared shell not found')
            if "'" in git:
                raise Unestablished('git path not quotable')
            q = "& '" + git + "'"
            c1 = q + ' --no-replace-objects cat-file -t ' + blob
            c2 = q + ' --no-replace-objects cat-file blob ' + blob + ' | ' + q + ' hash-object --no-filters --stdin'
            def sh_run(c):
                return subprocess.run([shell, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', c], stdin=DEVNULL,
                                      capture_output=True, cwd=repo, env=launch_env, timeout=600)
            r1, r2 = sh_run(c1), sh_run(c2)
            t1 = r1.stdout.decode('ascii', 'replace').strip()
            t2 = r2.stdout.decode('ascii', 'replace').strip()
            steps['iii'] = {'Shell': shell, 'Cwd': repo, 'TypeCommand': c1, 'TypeExit': r1.returncode, 'Type': t1,
                            'HashCommand': c2, 'HashExit': r2.returncode, 'Hash': t2,
                            'StderrHead': (r1.stderr + r2.stderr).decode('utf-8', 'replace')[:300],
                            'Ok': r1.returncode == 0 and t1 == 'blob' and r2.returncode == 0 and t2 == blob}
        except Exception as e:
            steps['iii'] = {'Error': type(e).__name__ + ': ' + str(e)[:300], 'Status': 'UNESTABLISHED', 'Ok': False}
        # (iv) fsck --full clean, and fsck writes nothing
        try:
            before = tree_state(git, repo)
            p = gitrun('fsck', '--full', '--no-dangling', '--no-progress', timeout=1800)
            after = tree_state(git, repo)
            steps['iv'] = {'Exit': p.returncode, 'StdoutHead': p.stdout.decode('utf-8', 'replace').splitlines()[:6],
                           'StderrHead': p.stderr.decode('utf-8', 'replace').splitlines()[:6], 'FsckWroteNothing': before == after,
                           'Ok': p.returncode == 0 and before == after}
        except Exception as e:
            steps['iv'] = {'Error': type(e).__name__ + ': ' + str(e)[:300], 'Status': 'UNESTABLISHED', 'Ok': False}

    order = ['i', 'ii', 'iii', 'iv', 'v', 'vi', 'vii', 'viii']
    for k in order:
        if 'Status' not in steps[k]:
            steps[k]['Status'] = 'PASS' if steps[k]['Ok'] else ('UNESTABLISHED' if 'Error' in steps[k] else 'FAIL')
    failed = [k for k in order if not steps[k]['Ok']]
    return {'Schema': 'i62-a6r3-pou-check/1', 'Repo': repo, 'UtcTime': utc(), 'Python': sys.version.split()[0], 'Steps': steps,
            'FailedSteps': failed, 'UnestablishedSteps': [k for k in order if steps[k]['Status'] == 'UNESTABLISHED'], 'Launch': not failed}


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--repo', required=True)
    ap.add_argument('--decl', required=True)
    ap.add_argument('--registered-config')
    ap.add_argument('--launch-env-json')
    ap.add_argument('--out')
    ap.add_argument('--snapshot-config', action='store_true', help='print the config snapshot of --repo (preparation write) and exit')
    a = ap.parse_args()
    decl = json.loads(rbytes(a.decl).decode('utf-8'))
    if a.snapshot_config:
        snap = snapshot_config(decl['Git']['Path'], os.path.normpath(a.repo))
        sys.stdout.write(json.dumps(snap, ensure_ascii=True, sort_keys=True) + '\n')
        return 0
    registered = None
    if a.registered_config and os.path.isfile(a.registered_config):
        try:
            registered = json.loads(rbytes(a.registered_config).decode('utf-8'))
        except Exception:
            registered = None
    launch_env = dict(os.environ)
    if a.launch_env_json:
        launch_env = json.loads(rbytes(a.launch_env_json).decode('utf-8'))
    res = run_check(os.path.normpath(a.repo), decl, registered, launch_env, os.path.dirname(os.path.abspath(a.decl)))
    data = (json.dumps(res, ensure_ascii=True, sort_keys=True) + '\n').encode('ascii')
    if a.out:
        with open(a.out, 'wb') as fh:
            fh.write(data)
    else:
        sys.stdout.buffer.write(data)
    return 0 if res['Launch'] else 1


if __name__ == '__main__':
    try:
        sys.exit(main())
    except SystemExit:
        raise
    except BaseException:
        sys.stderr.write(traceback.format_exc())
        sys.exit(2)
