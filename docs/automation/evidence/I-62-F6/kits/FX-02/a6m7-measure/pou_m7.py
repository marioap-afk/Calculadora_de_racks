# pou_m7.py - point-of-use check (A6-3 rule 3, steps (i)-(viii)) of the A6-7 measurement (I-62), run by the supervision
# immediately before the launch, after the gates, with the session retained. It is pou_check.py of r3/r2static (byte copy,
# SHA-256 c3490925...) with ONE replacement: the resolution of the bare names by the invocation's shell (part of step (v))
# uses a script without .NET method calls ((Get-Command ...).Source on the pipeline instead of [Console]::Out.Write), so that
# it also runs if the shell of the invocation (the runtime pwsh) is in ConstrainedLanguage; and it records the language mode
# of that shell. Steps (i)-(iv), (vi)-(viii), the config snapshot and the verdict are pou_check.py unchanged.
#   python -I pou_m7.py --repo <-C> --decl <decl.json> --registered-config <cfg.json> --launch-env-json <env.json> --out <result.json>
#   python -I pou_m7.py --snapshot-config --repo <-C> --decl <decl.json>          (as pou_check.py)
# Exit 0 = Launch true; 1 = Launch false; 2 = internal error. Anything but 0 with a readable result means: do not launch.
import importlib.util, json, os, re, subprocess, sys, traceback

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_s = importlib.util.spec_from_file_location('pou_check', os.path.join(HERE, 'pou_check.py'))
pc = importlib.util.module_from_spec(_s)
_s.loader.exec_module(pc)
Unestablished = pc.Unestablished
DEVNULL = subprocess.DEVNULL


def shell_resolve(shell_path, name, repo, launch_env):
    script = "(Get-Command -Name '%s' -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source" % name
    p = subprocess.run([shell_path, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', script], stdin=DEVNULL,
                       capture_output=True, cwd=repo, env=launch_env, timeout=120)
    lines = [x.strip() for x in p.stdout.decode('utf-8', 'replace').replace('\r\n', '\n').split('\n') if x.strip()]
    return p.returncode, (lines[-1] if lines else '')


def check_executable(role, d, launch_env, repo, shell_path, workdir):
    """pou_check.check_executable with the shell resolution script replaced (no method call)."""
    out = {'Role': role}
    sub = {}
    try:
        for f in ('Path', 'Version', 'Sha256', 'InvocationName'):
            if not isinstance(d.get(f), str) or not d.get(f):
                raise Unestablished('declaration lacks ' + f)
        out['Declared'] = dict((k, d[k]) for k in ('Path', 'Version', 'Sha256', 'InvocationName'))
        try:
            hashed, rec = pc.resolve_executable(d['Path'])
            out['Resolution'] = rec
            b = pc.rbytes(hashed)
            out['HashedFile'] = hashed
            out['MeasuredSha256'] = pc.sha256(b)
            out['MeasuredSize'] = len(b)
            sub['Sha256'] = 'PASS' if out['MeasuredSha256'] == d['Sha256'].lower() else 'FAIL'
            out['HashedFileName'] = os.path.basename(hashed)
        except Unestablished as e:
            out['ResolutionError'] = str(e)
            sub['Path'] = 'FAIL' if 'does not exist' in str(e) else 'UNESTABLISHED'
            sub['Sha256'] = 'UNESTABLISHED'
        if 'Path' not in sub:
            sub['Path'] = 'PASS'
        if sub['Path'] == 'PASS':
            try:
                p = subprocess.run([d['Path'], '--version'], stdin=DEVNULL, capture_output=True, cwd=workdir, env=pc.strip_git(launch_env), timeout=120)
                v = p.stdout.decode('utf-8', 'replace').strip()
                out['VersionCommand'] = {'Exit': p.returncode, 'Stdout': v, 'StderrHead': p.stderr.decode('utf-8', 'replace')[:200]}
                sub['Version'] = 'PASS' if (p.returncode == 0 and v == d['Version']) else 'FAIL'
            except Exception as e:
                out['VersionCommand'] = {'Error': type(e).__name__ + ': ' + str(e)[:200]}
                sub['Version'] = 'UNESTABLISHED'
        else:
            sub['Version'] = 'UNESTABLISHED'
        if not shell_path or not os.path.isfile(shell_path):
            sub['ResolvedByShell'] = 'UNESTABLISHED'
            out['ShellResolution'] = {'Error': 'declared shell not found'}
        else:
            name = d['InvocationName']
            if not re.fullmatch(r'[A-Za-z0-9_.-]+', name):
                raise Unestablished('unsafe invocation name')
            rc, src = shell_resolve(shell_path, name, repo, launch_env)
            out['ShellResolution'] = {'Exit': rc, 'Resolved': src}
            if rc != 0 or not src:
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


pc.check_executable = check_executable


def language_mode(shell_path, repo, launch_env):
    try:
        p = subprocess.run([shell_path, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', '$ExecutionContext.SessionState.LanguageMode'],
                           stdin=DEVNULL, capture_output=True, cwd=repo, env=launch_env, timeout=120)
        return p.stdout.decode('utf-8', 'replace').strip() or None
    except Exception as e:
        return 'unestablished: ' + type(e).__name__


def main():
    a = sys.argv[1:]
    opt = dict(zip(a[::2], a[1::2])) if '--snapshot-config' not in a else {}
    if '--snapshot-config' in a:
        repo = a[a.index('--repo') + 1]
        decl = json.loads(pc.rbytes(a[a.index('--decl') + 1]).decode('utf-8'))
        sys.stdout.write(json.dumps(pc.snapshot_config(decl['Git']['Path'], os.path.normpath(repo)), ensure_ascii=True, sort_keys=True) + '\n')
        return 0
    for k in ('--repo', '--decl', '--registered-config', '--launch-env-json', '--out'):
        if k not in opt:
            sys.stderr.write('usage: python -I pou_m7.py --repo R --decl D --registered-config C --launch-env-json E --out O\n')
            return 64
    decl = json.loads(pc.rbytes(opt['--decl']).decode('utf-8'))
    registered = None
    if os.path.isfile(opt['--registered-config']):
        try:
            registered = json.loads(pc.rbytes(opt['--registered-config']).decode('utf-8'))
        except Exception:
            registered = None
    launch_env = json.loads(pc.rbytes(opt['--launch-env-json']).decode('utf-8'))
    repo = os.path.normpath(opt['--repo'])
    res = pc.run_check(repo, decl, registered, launch_env, os.path.dirname(os.path.abspath(opt['--decl'])))
    res['Schema'] = 'i62-a6m7-pou-check/1 (pou_check.py c3490925 + CLM-safe shell resolution)'
    shell = (decl.get('Shell') or {}).get('Path')
    res['ShellLanguageMode'] = language_mode(shell, repo, launch_env) if shell and os.path.isfile(shell) else None
    data = (json.dumps(res, ensure_ascii=True, sort_keys=True) + '\n').encode('ascii')
    with open(opt['--out'], 'wb') as fh:
        fh.write(data)
    return 0 if res['Launch'] else 1


if __name__ == '__main__':
    try:
        sys.exit(main())
    except SystemExit:
        raise
    except BaseException:
        sys.stderr.write(traceback.format_exc())
        sys.exit(2)
