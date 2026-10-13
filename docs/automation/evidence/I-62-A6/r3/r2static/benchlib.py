# benchlib.py - shared helpers of the I-62 A-6 round 3 bench (r2static): hygiene of the deliverables, canonical JSON,
# hashing and repository snapshots. Stdlib only. Loaded through importlib (python -I does not put the script dir on sys.path).
# Never writes in the I-62 worktree or in any checkout; never executes anything under %USERPROFILE%\.codex or .cache\codex-runtimes.
import datetime, hashlib, json, os, re, subprocess

HERE = os.path.dirname(os.path.abspath(__file__))
NL = '\n'
DEVNULL = subprocess.DEVNULL
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')


def sha256(b):
    return hashlib.sha256(b).hexdigest()


def blob_sha1(b):
    return hashlib.sha1(b'blob ' + str(len(b)).encode('ascii') + b'\0' + b).hexdigest()


def canon(o):
    return json.dumps(o, ensure_ascii=True, sort_keys=True, separators=(',', ':'))


def utc():
    return datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%fZ')


def rbytes(p):
    with open(p, 'rb') as fh:
        return fh.read()


def ensure_dir(p):
    os.makedirs(p, exist_ok=True)
    return p


def wbytes(p, b):
    ensure_dir(os.path.dirname(p))
    with open(p, 'wb') as fh:
        fh.write(b)


# ------------------------------------------------------------------ deliverable hygiene (same rules as round 2)
_EMAIL = re.compile(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}')


def _user_forms():
    prof = os.environ.get('USERPROFILE') or ''
    forms = []
    if len(prof) > 3:
        forms += [prof, prof.replace('\\', '/'), prof.replace('\\', '\\\\')]
    return forms


def _user_tokens():
    """Account name and its 8.3 short form, derived at run time (never written in this source)."""
    toks = set()
    user = os.path.basename(os.environ.get('USERPROFILE') or '')
    if len(user) > 2:
        toks.add(user)
    for v in (os.environ.get('TEMP'), os.environ.get('TMP'), HERE):
        m = re.match(r'(?i)^[a-z]:[\\/]users[\\/]([^\\/]+)', v or '')
        if m and len(m.group(1)) > 2:
            toks.add(m.group(1))
    return sorted(toks, key=len, reverse=True)


_USER_TOKENS = _user_tokens()


def sanitize_str(s):
    out = s
    for form in sorted(_user_forms(), key=len, reverse=True):
        out = re.sub(re.escape(form), '%USERPROFILE%', out, flags=re.IGNORECASE)
    for tok in _USER_TOKENS:
        out = re.sub(re.escape(tok), '<usuario>', out, flags=re.IGNORECASE)
    out = _EMAIL.sub('<email>', out)
    return out


def sanitize(obj):
    if isinstance(obj, dict):
        return dict((sanitize_str(k) if isinstance(k, str) else k, sanitize(v)) for k, v in obj.items())
    if isinstance(obj, (list, tuple)):
        return [sanitize(x) for x in obj]
    if isinstance(obj, str):
        return sanitize_str(obj)
    return obj


def leak_check(data):
    """True when the bytes contain none of the forms that must not leave the host."""
    s = data.decode('ascii', 'replace')
    if _EMAIL.search(s):
        return False
    for t in _USER_TOKENS:
        if t.lower() in s.lower():
            return False
    return True


def write_result(name, obj):
    """Deliverable JSON: sanitized, canonical ASCII (sorted keys), LF terminated."""
    p = os.path.join(HERE, name)
    data = (canon(sanitize(obj)) + NL).encode('ascii')
    if not leak_check(data):
        raise SystemExit('refusing to write a deliverable that still holds an account name or an e-mail address: ' + name)
    with open(p, 'wb') as fh:
        fh.write(data)
    return p, sha256(data)


# ------------------------------------------------------------------ processes
def path_lookup(name, env=None):
    """First <name>.exe along PATH, in PATH order, never the current directory (CreateProcess would also look there)."""
    for d in ((env or os.environ).get('PATH') or '').split(';'):
        d = d.strip().strip('"')
        if d:
            p = os.path.join(d, name + '.exe')
            if os.path.isfile(p):
                return os.path.normpath(p)
    return None


GIT = path_lookup('git')


def genv(base=None, extra=None):
    """Environment for the bench's own git calls: GIT_* removed, read-only settings."""
    src = os.environ if base is None else base
    e = dict((k, v) for k, v in src.items() if not k.upper().startswith('GIT_'))
    e['GIT_OPTIONAL_LOCKS'] = '0'
    e['GIT_TERMINAL_PROMPT'] = '0'
    if extra:
        e.update(extra)
    return e


def run(argv, inp=None, env=None, cwd=None, timeout=600):
    if inp is None:
        return subprocess.run(argv, stdin=DEVNULL, capture_output=True, env=env, cwd=cwd, timeout=timeout)
    return subprocess.run(argv, input=inp, capture_output=True, env=env, cwd=cwd, timeout=timeout)


# ------------------------------------------------------------------ repository snapshots (round-2 logic)
def _walk(root, skip_dirs=()):
    out = {}
    skip = set(os.path.normcase(os.path.normpath(d)) for d in skip_dirs)
    for r, ds, fs in os.walk(root):
        ds[:] = [d for d in ds if os.path.normcase(os.path.normpath(os.path.join(r, d))) not in skip]
        for f in fs:
            q = os.path.join(r, f)
            b = rbytes(q)
            out[os.path.relpath(q, root).replace(os.sep, '/')] = [len(b), sha256(b)]
    return out


def snapshot(git_exe, T):
    def g(*a):
        return run([git_exe, '-C', T] + list(a), env=genv()).stdout.decode('utf-8', 'replace').strip()
    top = os.path.normpath(g('rev-parse', '--show-toplevel'))
    gd = os.path.normpath(g('rev-parse', '--path-format=absolute', '--git-dir'))
    cd = os.path.normpath(g('rev-parse', '--path-format=absolute', '--git-common-dir'))
    refs = g('for-each-ref', '--format=%(refname) %(objectname) %(symref)').splitlines()
    head = g('rev-parse', 'HEAD')
    st = run([git_exe, '-C', T, 'status', '--porcelain=v1', '-z', '--ignored'], env=genv()).stdout
    idx = os.path.join(gd, 'index')
    wt_skip = [os.path.join(top, '.git')] if os.path.isdir(os.path.join(top, '.git')) else []
    return {'Toplevel': top, 'GitDir': gd, 'CommonDir': cd, 'Refs': refs, 'Head': head,
            'IndexSha256': sha256(rbytes(idx)) if os.path.isfile(idx) else None, 'StatusZSha256': sha256(st),
            'StatusZ': [x for x in st.decode('utf-8', 'replace').split('\0') if x],
            'WorktreeFiles': _walk(top, wt_skip), 'GitFiles': _walk(cd)}


def snapdiff(a, b):
    d = {}
    for k in ('Toplevel', 'GitDir', 'CommonDir', 'Refs', 'Head', 'IndexSha256', 'StatusZSha256', 'StatusZ'):
        if a[k] != b[k]:
            d[k] = {'Before': a[k], 'After': b[k]}
    for k in ('WorktreeFiles', 'GitFiles'):
        x, y = a[k], b[k]
        add = sorted(set(y) - set(x))
        rem = sorted(set(x) - set(y))
        mod = sorted(p for p in set(x) & set(y) if x[p] != y[p])
        if add or rem or mod:
            d[k] = {'Added': add, 'Removed': rem, 'Modified': mod}
    return d


def snap_digest(s):
    return {'Head': s['Head'], 'IndexSha256': s['IndexSha256'], 'StatusZSha256': s['StatusZSha256'], 'StatusEntries': len(s['StatusZ']),
            'RefsSha256': sha256(canon(s['Refs']).encode()), 'RefCount': len(s['Refs']),
            'ReplaceRefs': [r for r in s['Refs'] if r.startswith('refs/replace/')],
            'WorktreeFilesSha256': sha256(canon(s['WorktreeFiles']).encode()), 'WorktreeFileCount': len(s['WorktreeFiles']),
            'GitFilesSha256': sha256(canon(s['GitFiles']).encode()), 'GitFileCount': len(s['GitFiles'])}
