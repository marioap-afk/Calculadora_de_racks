# lib_r1r4.py - common helpers of the A-6 round 2 evidence bench for R1 and R4 (I-62).
# Stdlib only. Loaded by run_r1.py / run_r4.py through importlib (python -I does not add the script dir to sys.path).
# Never writes in the I-62 worktree, the main checkout, D:\r62-fixture or any D:\r62-arch-* / D:\r62-bmin-tmp directory.
# The pinned tool and acceptor are obtained ONLY by blob (git cat-file blob) and never modified.
import base64, datetime, hashlib, json, os, platform, re, shutil, stat, struct, subprocess, sys, zlib

HERE = os.path.dirname(os.path.abspath(__file__))
WORK = 'D:/r62-a6r2/repro/r1r4'
PIN = WORK + '/pin'
PIN_TOOL = PIN + '/bmin_facts.py'
PIN_ACC = PIN + '/bmin_accept.py'
BIN = WORK + '/bin'
ORIGIN = 'D:/r62-fixture/fixture-origin.git'
B = '9a829b0eeb1210238052b1feac06cfa8f7964aed'          # pinned tool blob (bmin_facts.py)
ACC_BLOB = '434ef1fccb0d15762b30244b4a60928120698411'    # pinned acceptor blob (bmin_accept.py)
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
TRX_REL = 'docs/automation/evidence/I-62-F6/FX-02/U07-trx-R37949808974/fixture-tests.trx'
TRX_SHA = '4ccdf2cecba561259b877a471d577f1afa6ac4a9ad00ceb4fa0417e7c61a7513'
SCHEMA_DIR = 'docs/automation/agent-execution/schemas/'
# FX-U1 scenario (same revisions as bmin/e2e/e2e.py)
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
MAP = 'docs/automation/agent-execution/compatibility/I62-clause-map.json'
SURFACES = ['AGENTS.md', 'CLAUDE.md', 'docs/AUTOMATION_PLAN.md', 'docs/FOUNDATIONS.md', 'docs/INITIATIVE_LIFECYCLE.md', 'docs/WORKFLOW.md', 'docs/adr/',
            'docs/automation/agent-execution/', 'docs/initiatives/PROMPT_TEMPLATES.md']
DELEG_RUN = 'R20261012T000000Z-a6d1'
WORK_RUN = 'R20261012T000000Z-a6e2'
CV_RUN = 'R20261012T000000Z-a6c3'
RELAY_RUN = 'R20261012T000000Z-a6f4'
PWSH = shutil.which('pwsh') or 'pwsh'
PS51 = r'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe'
CMD = os.environ.get('ComSpec') or r'C:\Windows\System32\cmd.exe'
SAFE_TOKEN = re.compile(r'[A-Za-z0-9_./:=+~-]+')
DEVNULL = subprocess.DEVNULL
NL = '\n'


# ------------------------------------------------------------------ basics
def sha256(b):
    return hashlib.sha256(b).hexdigest()


def blob_sha1(b):
    return hashlib.sha1(b'blob ' + str(len(b)).encode('ascii') + b'\0' + b).hexdigest()


def canon(o):
    return json.dumps(o, ensure_ascii=True, sort_keys=True, separators=(',', ':'))


def utc():
    return datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%fZ')


def utc_s():
    return datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ')


def genv(extra=None):
    """Environment for git and shells: inherited GIT_* removed, read-only git settings."""
    e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    e['GIT_OPTIONAL_LOCKS'] = '0'
    e['GIT_TERMINAL_PROMPT'] = '0'
    if extra:
        e.update(extra)
    return e


def git(repo, *args, inp=None, env=None, cwd=None):
    cmd = ['git'] + (['-C', repo] if repo else []) + [str(a) for a in args]
    if inp is None:
        return subprocess.run(cmd, stdin=DEVNULL, capture_output=True, env=env or genv(), cwd=cwd)
    return subprocess.run(cmd, input=inp, capture_output=True, env=env or genv(), cwd=cwd)


def gout(repo, *args, **kw):
    return git(repo, *args, **kw).stdout.decode('utf-8', 'replace').strip()


def rmtree(p):
    def onexc(f, q, e):
        os.chmod(q, stat.S_IWRITE)
        f(q)
    if os.path.isdir(p):
        shutil.rmtree(p, onexc=onexc)


def ensure_dir(p):
    os.makedirs(p, exist_ok=True)
    return p


def wbytes(p, b):
    ensure_dir(os.path.dirname(p))
    with open(p, 'wb') as fh:
        fh.write(b)


def wjson_local(p, obj, pretty=True):
    """Work-area JSON (inputs of the tool/acceptor): UTF-8, LF, as e2e.py writes them."""
    ensure_dir(os.path.dirname(p))
    with open(p, 'w', encoding='utf-8', newline=NL) as fh:
        fh.write(json.dumps(obj, ensure_ascii=False, indent=1) + NL if pretty else canon(obj) + NL)


def rbytes(p):
    with open(p, 'rb') as fh:
        return fh.read()


# ------------------------------------------------------------------ deliverable hygiene
_EMAIL = re.compile(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}')


def _user_forms():
    prof = os.environ.get('USERPROFILE') or ''
    forms = []
    if len(prof) > 3:
        forms += [prof, prof.replace('\\', '/')]
    return forms


def _user_tokens():
    """The account name and its 8.3 short form, derived at run time (never written in this source)."""
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
    for form in _user_forms():
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


def write_result(name, obj):
    """Deliverable results JSON: sanitized, canonical ASCII + LF."""
    p = os.path.join(HERE, name)
    data = (canon(sanitize(obj)) + NL).encode('ascii')
    with open(p, 'wb') as fh:
        fh.write(data)
    return p, sha256(data)


def write_deliverable_json(rel, obj):
    p = os.path.join(HERE, rel)
    ensure_dir(os.path.dirname(p))
    data = (json.dumps(sanitize(obj), ensure_ascii=True, indent=1, sort_keys=True) + NL).encode('ascii')
    with open(p, 'wb') as fh:
        fh.write(data)
    return p, sha256(data)


# ------------------------------------------------------------------ pinned bytes
def pinned_bytes(blob):
    p = git(None, '--no-replace-objects', '-C', WT, 'cat-file', 'blob', blob)
    if p.returncode != 0 or blob_sha1(p.stdout) != blob:
        raise SystemExit('pinned blob not readable or wrong: ' + blob)
    return p.stdout


def ensure_pins():
    ensure_dir(PIN)
    ensure_dir(BIN)
    rec = {}
    for blob, path in ((B, PIN_TOOL), (ACC_BLOB, PIN_ACC)):
        b = pinned_bytes(blob)
        if not os.path.isfile(path) or rbytes(path) != b:
            wbytes(path, b)
        rec[os.path.basename(path)] = {'Blob': blob, 'Sha256': sha256(b), 'Size': len(b), 'CopyPath': path,
                                       'Source': 'git --no-replace-objects -C %USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal cat-file blob ' + blob}
    digest = ('import hashlib, sys\n'
              'd = sys.stdin.buffer.read()\n'
              'h = hashlib.sha1(b"blob " + str(len(d)).encode("ascii") + b"\\0" + d).hexdigest()\n'
              'sys.stdout.write("%d %s %s %d\\n" % (len(d), h, hashlib.sha256(d).hexdigest(), d.count(b"\\r")))\n').encode('ascii')
    wbytes(BIN + '/digest.py', digest)
    return rec


def wt_show(rev_path):
    """Read a blob from the I-62 worktree repository (read-only)."""
    p = git(None, '--no-replace-objects', '-C', WT, 'show', rev_path)
    if p.returncode != 0:
        raise SystemExit('cannot read ' + rev_path)
    return p.stdout


def load_schema(name):
    rev = gout(None, '-C', WT, 'rev-parse', 'HEAD')
    data = wt_show(rev + ':' + SCHEMA_DIR + name)
    blob = gout(None, '-C', WT, 'rev-parse', rev + ':' + SCHEMA_DIR + name)
    return json.loads(data.decode('utf-8')), {'Path': SCHEMA_DIR + name, 'Commit': rev, 'Blob': blob}


# ------------------------------------------------------------------ JSON-schema subset validator (same keyword subset the schemas use)
SUPPORTED = {'type', 'enum', 'const', 'required', 'properties', 'additionalProperties', 'items', 'minItems', 'maxItems',
             'uniqueItems', 'minLength', 'pattern', 'minimum', 'maximum'}
ANNOT = {'$schema', 'title', 'description', '$id', '$comment', 'default', 'examples'}


def _tok(v, t):
    return {'null': v is None, 'boolean': isinstance(v, bool), 'object': isinstance(v, dict), 'array': isinstance(v, list),
            'string': isinstance(v, str), 'integer': isinstance(v, int) and not isinstance(v, bool),
            'number': isinstance(v, (int, float)) and not isinstance(v, bool)}.get(t, False)


def validate(s, v, ptr='#', out=None):
    out = [] if out is None else out
    if s is True or s is None:
        return out
    if s is False:
        out.append(ptr + ' false')
        return out
    for k in s:
        if k not in SUPPORTED and k not in ANNOT:
            out.append(ptr + ' unsupported keyword ' + k)
    t = s.get('type')
    if t is not None and not any(_tok(v, x) for x in (t if isinstance(t, list) else [t])):
        out.append(ptr + ' type')
    if 'enum' in s and not any(canon(v) == canon(e) for e in s['enum']):
        out.append(ptr + ' enum')
    if 'const' in s and canon(v) != canon(s['const']):
        out.append(ptr + ' const')
    if isinstance(v, dict):
        for r in s.get('required', []):
            if r not in v:
                out.append(ptr + ' required ' + r)
        props = s.get('properties', {})
        for k, x in v.items():
            if k in props:
                validate(props[k], x, ptr + '/' + k, out)
            elif s.get('additionalProperties') is False:
                out.append(ptr + '/' + k + ' additionalProperties')
            elif isinstance(s.get('additionalProperties'), dict):
                validate(s['additionalProperties'], x, ptr + '/' + k, out)
    if isinstance(v, list):
        if 'items' in s:
            for i, x in enumerate(v):
                validate(s['items'], x, ptr + '/' + str(i), out)
        if 'minItems' in s and len(v) < s['minItems']:
            out.append(ptr + ' minItems')
        if 'maxItems' in s and len(v) > s['maxItems']:
            out.append(ptr + ' maxItems')
        if s.get('uniqueItems') and len(set(canon(x) for x in v)) != len(v):
            out.append(ptr + ' uniqueItems')
    if isinstance(v, str) and v is not None:
        if 'minLength' in s and len(v) < s['minLength']:
            out.append(ptr + ' minLength')
        if 'pattern' in s:
            pat = s['pattern']
            if pat.endswith('$') and not pat.endswith('\\$'):
                pat = pat[:-1] + '\\Z'
            if re.search(pat, v) is None:
                out.append(ptr + ' pattern')
    if isinstance(v, (int, float)) and not isinstance(v, bool):
        if 'minimum' in s and v < s['minimum']:
            out.append(ptr + ' minimum')
        if 'maximum' in s and v > s['maximum']:
            out.append(ptr + ' maximum')
    return out


# ------------------------------------------------------------------ repositories
def clone(dest, branch=BR, checkout=True):
    rmtree(dest)
    ensure_dir(os.path.dirname(dest))
    args = ['clone', '-q', '--no-hardlinks', '-c', 'core.autocrlf=false', '-b', branch]
    if not checkout:
        args.append('--no-checkout')
    p = git(None, *(args + [ORIGIN, dest]))
    if p.returncode != 0:
        raise SystemExit('clone failed: ' + p.stderr.decode('utf-8', 'replace'))
    return dest


def worktree_add(main_clone, dest, branch=BR):
    rmtree(dest)
    p = git(main_clone, 'worktree', 'add', dest, branch)
    if p.returncode != 0:
        raise SystemExit('worktree add failed: ' + p.stderr.decode('utf-8', 'replace'))
    return dest


def abspath_dir(T, flag):
    return os.path.normpath(gout(T, 'rev-parse', '--path-format=absolute', flag))


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


def snapshot(T):
    """Zero-write snapshot of a repository / worktree: all refs (refs/replace included), HEAD, index, status with
    ignored files, every working-tree file, every file of the git dir and of the common dir (size + SHA-256)."""
    top = os.path.normpath(gout(T, 'rev-parse', '--show-toplevel'))
    gd = abspath_dir(T, '--git-dir')
    cd = abspath_dir(T, '--git-common-dir')
    refs = git(T, 'for-each-ref', '--format=%(refname) %(objectname) %(symref)').stdout.decode('utf-8', 'replace').splitlines()
    head_sym = gout(T, 'symbolic-ref', '-q', 'HEAD')
    head = gout(T, 'rev-parse', 'HEAD')
    idx = os.path.join(gd, 'index')
    st = git(T, 'status', '--porcelain=v1', '-z', '--ignored').stdout
    wt_skip = [os.path.join(top, '.git')] if os.path.isdir(os.path.join(top, '.git')) else []
    files = _walk(top, wt_skip)
    gfiles = _walk(cd)
    if not os.path.normcase(gd).startswith(os.path.normcase(cd)):
        for k, v in _walk(gd).items():
            gfiles['gitdir:' + k] = v
    return {'Toplevel': top, 'GitDir': gd, 'CommonDir': cd, 'Refs': refs, 'HeadSymref': head_sym, 'Head': head,
            'IndexSha256': sha256(rbytes(idx)) if os.path.isfile(idx) else None,
            'StatusZSha256': sha256(st), 'StatusZ': [x for x in st.decode('utf-8', 'replace').split('\0') if x],
            'WorktreeFiles': files, 'GitFiles': gfiles}


def snapdiff(a, b):
    d = {}
    for k in ('Toplevel', 'GitDir', 'CommonDir', 'Refs', 'HeadSymref', 'Head', 'IndexSha256', 'StatusZSha256', 'StatusZ'):
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
    return {'Head': s['Head'], 'HeadSymref': s['HeadSymref'], 'IndexSha256': s['IndexSha256'], 'StatusZSha256': s['StatusZSha256'],
            'StatusEntries': len(s['StatusZ']), 'RefsSha256': sha256(canon(s['Refs']).encode()), 'RefCount': len(s['Refs']),
            'ReplaceRefs': [r for r in s['Refs'] if r.startswith('refs/replace/')],
            'WorktreeFilesSha256': sha256(canon(s['WorktreeFiles']).encode()), 'WorktreeFileCount': len(s['WorktreeFiles']),
            'GitFilesSha256': sha256(canon(s['GitFiles']).encode()), 'GitFileCount': len(s['GitFiles'])}


def loose_rel(oid):
    return 'objects/' + oid[:2] + '/' + oid[2:]


# ------------------------------------------------------------------ P1 / P2
def p1(T, decision_ref):
    src = git(None, '--no-replace-objects', '-C', WT, 'cat-file', 'blob', B)
    data = src.stdout
    before = snapshot(T)
    t = utc()
    p = git(T, 'hash-object', '-w', '--no-filters', '--stdin', inp=data)
    oid = p.stdout.decode('ascii', 'replace').strip()
    after = snapshot(T)
    d = snapdiff(before, after)
    only_new = (set(d) == {'GitFiles'} and d['GitFiles'] == {'Added': [loose_rel(B)], 'Removed': [], 'Modified': []})
    return {'Repo': T, 'UtcTime': t, 'DecisionRef': decision_ref,
            'SourceCommand': 'git --no-replace-objects -C <I-62 worktree> cat-file blob ' + B,
            'SourceBlobSha1': blob_sha1(data), 'SourceSha256': sha256(data), 'SourceSize': len(data),
            'WriteCommand': 'git -C ' + T + ' hash-object -w --no-filters --stdin', 'WriteExit': p.returncode,
            'ReturnedId': oid, 'IdEqualsPinned': oid == B, 'SnapshotBefore': snap_digest(before), 'SnapshotAfter': snap_digest(after),
            'SnapshotDiff': d, 'OnlyNewLooseObject': only_new, 'Held': oid == B and only_new}


def shell_argv(shell, cmdline):
    if shell == 'pwsh':
        return [PWSH, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', cmdline]
    if shell == 'ps51':
        return [PS51, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', cmdline]
    if shell == 'cmd':
        return [CMD, '/d', '/s', '/c', cmdline]
    raise ValueError(shell)


def shell_run(shell, cmdline, cwd, env=None, timeout=900):
    p = subprocess.run(shell_argv(shell, cmdline), cwd=cwd, capture_output=True, env=env or genv(), stdin=DEVNULL, timeout=timeout)
    return p.returncode, p.stdout, p.stderr


def env_without_nodefaultcwd():
    return dict((k, v) for k, v in genv().items() if k.upper() != 'NODEFAULTCURRENTDIRECTORYINEXEPATH')


def p2(T, shell, blob=B, env=None):
    """Pre-launch identity check (candidate R1, P2). Git commands and file-existence tests only; step 3 runs through the shell."""
    steps = {}
    # (1) no replace refs
    p = git(T, 'for-each-ref', 'refs/replace/')
    out = p.stdout.decode('utf-8', 'replace')
    steps['1'] = {'Command': 'git -C T for-each-ref refs/replace/', 'Exit': p.returncode, 'Output': out.splitlines(),
                  'Ok': p.returncode == 0 and out.strip() == ''}
    # (2) grafts, alternates, shallow
    cd = abspath_dir(T, '--git-common-dir')
    grafts = os.path.join(cd, 'info', 'grafts')
    alts = os.path.join(cd, 'objects', 'info', 'alternates')
    gp_grafts = gout(T, 'rev-parse', '--path-format=absolute', '--git-path', 'info/grafts')
    gp_alts = gout(T, 'rev-parse', '--path-format=absolute', '--git-path', 'objects/info/alternates')
    shallow = gout(T, 'rev-parse', '--is-shallow-repository')
    steps['2'] = {'CommonDir': cd, 'GraftsExists': os.path.exists(grafts), 'AlternatesExists': os.path.exists(alts),
                  'IsShallowRepository': shallow,
                  'Extra_GitPathGrafts': gp_grafts, 'Extra_GitPathGraftsExists': os.path.exists(gp_grafts),
                  'Extra_GitPathAlternates': gp_alts, 'Extra_GitPathAlternatesExists': os.path.exists(gp_alts),
                  'Ok': (not os.path.exists(grafts)) and (not os.path.exists(alts)) and shallow == 'false'}
    # (3) through the shell the Controller will use, with the Controller's working directory
    c1 = 'git --no-replace-objects cat-file -t ' + blob
    c2 = 'git --no-replace-objects cat-file blob ' + blob + ' | git hash-object --no-filters --stdin'
    c3 = 'git --no-replace-objects cat-file blob ' + blob + ' | python -I ' + BIN + '/digest.py'
    r1 = shell_run(shell, c1, T, env=env)
    r2 = shell_run(shell, c2, T, env=env)
    r3 = shell_run(shell, c3, T, env=env)
    t1 = r1[1].decode('ascii', 'replace').strip()
    t2 = r2[1].decode('ascii', 'replace').strip()
    t3 = r3[1].decode('ascii', 'replace').strip().split()
    steps['3'] = {'Shell': shell, 'Cwd': T, 'EnvNoDefaultCurrentDirectoryInExePath': (env or genv()).get('NODEFAULTCURRENTDIRECTORYINEXEPATH',
                                                                                                    (env or genv()).get('NoDefaultCurrentDirectoryInExePath')),
                  'TypeCommand': c1, 'TypeExit': r1[0], 'Type': t1,
                  'HashCommand': c2, 'HashExit': r2[0], 'Hash': t2,
                  'Extra_ConsumerDigestCommand': c3,
                  'Extra_ConsumerDigest': ({'Size': int(t3[0]), 'BlobSha1': t3[1], 'Sha256': t3[2], 'CrBytes': int(t3[3]),
                                            'EqualsPinned': t3[1] == blob} if len(t3) == 4 else {'Raw': ' '.join(t3)}),
                  'Ok': r1[0] == 0 and t1 == 'blob' and r2[0] == 0 and t2 == blob}
    # (4) fsck, and fsck writes nothing
    before = snapshot(T)
    p = git(T, 'fsck', '--full', '--no-dangling', '--no-progress', env=genv({'GIT_OPTIONAL_LOCKS': '0'}))
    after = snapshot(T)
    d = snapdiff(before, after)
    steps['4'] = {'Command': 'git -C T fsck --full --no-dangling --no-progress (GIT_OPTIONAL_LOCKS=0)', 'Exit': p.returncode,
                  'StderrHead': p.stderr.decode('utf-8', 'replace').splitlines()[:6], 'StdoutHead': p.stdout.decode('utf-8', 'replace').splitlines()[:6],
                  'FsckWroteNothing': d == {}, 'SnapshotDiff': d, 'Ok': p.returncode == 0 and d == {}}
    failed = [k for k in ('1', '2', '3', '4') if not steps[k]['Ok']]
    return {'Repo': T, 'UtcTime': utc(), 'Shell': shell, 'Steps': steps, 'FailedSteps': failed, 'Launch': not failed}


# ------------------------------------------------------------------ tool runs
def check_tokens(argv):
    for a in argv:
        if not SAFE_TOKEN.fullmatch(a):
            raise SystemExit('unsafe token for a shell command line: ' + repr(a))


def p3_cmdline(argv, plain=False):
    check_tokens(argv)
    g = ('git cat-file blob ' if plain else 'git --no-replace-objects cat-file blob ') + B
    return g + ' | python -I - ' + ' '.join(argv)


def run_tool_file(argv, cwd=None):
    p = subprocess.run([sys.executable, '-I', PIN_TOOL] + argv, capture_output=True, env=genv(), cwd=cwd, stdin=DEVNULL)
    return p.returncode, p.stdout


def run_tool_shell(T, argv, shell='pwsh', plain=False):
    cl = p3_cmdline(argv, plain)
    rc, out, err = shell_run(shell, cl, T)
    return rc, out, cl, err


def envelope_facts(raw):
    try:
        e = json.loads(raw.decode('ascii'))
    except Exception:
        try:
            e = json.loads(raw.decode('utf-8', 'replace'))
        except Exception:
            return None
    return e


# ------------------------------------------------------------------ inputs (same as e2e.py, with a delegation/v2 and a schema-conformant handoff)
def trx_bytes():
    rev = gout(None, '-C', WT, 'rev-parse', 'HEAD')
    b = wt_show(rev + ':' + TRX_REL)
    if sha256(b) == TRX_SHA:
        return b, 'blob ' + gout(None, '-C', WT, 'rev-parse', rev + ':' + TRX_REL)
    raise SystemExit('TRX blob SHA-256 differs from the pinned value')


def build_inputs(T, inp):
    """Writes K.json, deleg.json, handoff.json, fixture-tests.trx into inp (outside T). Returns {name: sha256}."""
    ensure_dir(inp)
    kb = git(T, '--no-replace-objects', 'show', TIP + ':' + K).stdout
    wbytes(inp + '/K.json', kb)
    k = json.loads(kb.decode('utf-8'))
    deleg = {'Schema': 'rackcad-delegation/v2', 'TaskId': 'T1', 'RunId': DELEG_RUN, 'Initiative': 'FX-U1', 'Unit': 'FX-U1',
             'Gate': k['Gate'], 'Attempt': 0, 'AuthorityRevision': k['AuthorityRevision'], 'MainSha': k['MainSha'], 'BaseSha': BASE,
             'ExpectedBranch': BR, 'ExpectedWorktree': T, 'Owner': {'Kind': 'session-internal', 'Id': 'bench-r1r4'},
             'TaskClass': 'bench', 'Dimensions': {'Ambiguity': 'Low', 'ArchitecturalSensitivity': 'Low', 'Breadth': 'Low', 'ToolUse': 'Low',
                                                  'FailureCost': 'Low', 'MechanicalRepetition': 'Low', 'Horizon': 'Low'},
             'Executor': {'Provider': 'bench', 'Transport': 'session-internal', 'Role': 'WORKER', 'Cell': 'bench',
                          'BindingRef': {'UnitId': 'FX-U1', 'Scope': 'TASK', 'TaskId': 'T1', 'Role': 'WORKER', 'BindingId': 'B20261012T000000Z-a6b6',
                                         'Sha256': '0' * 64, 'Location': {'Kind': 'TRANSIENT', 'Path': 'banco-r1r4', 'Commit': None, 'Blob': None}}},
             'Model': 'bench', 'Effort': {'Semantic': 'Routine', 'Provider': 'bench'}, 'PromptProfile': 'ROUTINE_IMPLEMENTATION',
             'RoutingReason': 'banco r1r4 (sin modelo)', 'ModelEscalationReason': None, 'RoutingEnforcement': k['RoutingEnforcement'],
             'Authorities': k['Authorities'], 'Objective': k['Objective'], 'AllowedWriteScope': k['AllowedWriteScope'],
             'ForbiddenWriteScope': k['ForbiddenWriteScope'], 'Invariants': k['Invariants'], 'AcceptanceCriteria': [],
             'RequiredTests': k['RequiredTests'], 'ExpectedEvidence': k['ExpectedEvidence'], 'StopConditions': k['StopConditions'],
             'MaxReworkLoops': 3, 'AttemptsRemaining': 3, 'ChainBaseSha': BASE, 'ChainRedSha': RED, 'ChainRedFiles': [],
             'CorrectionOf': None, 'ExpectedHandoffPath': 'artifacts/orchestration/FX-U1/T1/{WorkRunId}/worker-handoff.json',
             'IssuedBy': 'banco r1r4 (supervision; sin valor normativo)', 'RoleRequirements': k['RoleRequirements'],
             'SupersededCommits': k['SupersededCommits']}
    wjson_local(inp + '/deleg.json', deleg)
    handoff = {'Schema': 'rackcad-worker-handoff/v1', 'TaskId': 'T1', 'RunId': WORK_RUN, 'DelegationRunId': DELEG_RUN,
               'Initiative': 'FX-U1', 'Gate': k['Gate'], 'Attempt': 0, 'BaseSha': BASE, 'RedSha': RED, 'CurrentSha': HEADX, 'Branch': BR,
               'Worktree': T, 'Pushed': True, 'FilesChanged': ['src/Fixture.Lib/Calculator.cs'],
               'WorkCompleted': 'A\u00f1ad\u00ed Subtract \u2014 escenario sint\u00e9tico de integraci\u00f3n B-min',
               'TestsExecuted': [{'RunRef': 'bench', 'Command': 'dotnet test --filter FullyQualifiedName~Subtract', 'Phase': 'GREEN',
                                  'TreeSha': None, 'ResultsFile': None}],
               'TestResults': [{'RunRef': 'bench', 'Selected': 1, 'Passed': 1, 'Failed': 0, 'Skipped': 0, 'FailedTests': []}],
               'Evidence': ['GATE PASS (t\u00e9rmino de 16.10 a prop\u00f3sito, para el hecho de freetext)'],
               'UnexpectedFindings': [], 'KnownLimitations': [], 'Deviations': [], 'OpenQuestions': [],
               'WorkerStatus': 'IMPLEMENTATION_COMPLETE', 'Disposition': 'NONE', 'TriggeredStopConditions': [],
               'RecommendedNextAction': 'verificar',
               'Worker': {'Provider': 'bench', 'ModelRequested': 'bench', 'EffortRequested': 'bench', 'Trailer': 'Co-Authored-By: synthetic'}}
    wjson_local(inp + '/handoff.json', handoff)
    tb, tsrc = trx_bytes()
    wbytes(inp + '/fixture-tests.trx', tb)
    return {n: sha256(rbytes(inp + '/' + n)) for n in ('K.json', 'deleg.json', 'handoff.json', 'fixture-tests.trx')}, deleg, handoff, k


def runs_for(T, inp, H):
    """The 13 commands of e2e.py, with --repo = T; DELEG = the delegation/v2 file (e2e used the contract as DELEG)."""
    return [
        ('identity', ['identity', '--repo', T, '--branch', BR, '--base', BASE, '--current', HEADX, '--red', RED]),
        ('diff', ['diff', '--repo', T, '--from', BASE, '--to', HEADX]),
        ('scope', ['scope', '--repo', T, '--from', BASE, '--to', HEADX, '--file', 'DELEG=' + inp + '/deleg.json', '--sha256', 'DELEG=' + H['deleg.json']]),
        ('contract', ['contract', '--repo', T, '--file', 'CONTRACT=' + inp + '/K.json', '--sha256', 'CONTRACT=' + H['K.json'],
                      '--file', 'DELEG=' + inp + '/deleg.json', '--sha256', 'DELEG=' + H['deleg.json']]),
        ('hashes', ['hashes', '--repo', T, '--rev', TIP, '--paths', K, 'src/Fixture.Lib/Calculator.cs']),
        ('cleantree', ['cleantree', '--repo', T, '--ignore-path', IGN]),
        ('read-text-1', ['read-text', '--repo', T, '--rev', TIP, '--path', DEC, '--lines', '1-1']),
        ('read-text-73', ['read-text', '--repo', T, '--rev', TIP, '--path', DEC, '--lines', '73-73']),
        ('read-json', ['read-json', '--repo', T, '--rev', TIP, '--path', K, '--pointer', '/Objective', '--pointer', '/Schema',
                       '--pointer', '/MainSha', '--pointer', '/AuthorityRevision']),
        ('trx', ['trx', '--repo', T, '--file', 'TRX=' + inp + '/fixture-tests.trx', '--sha256', 'TRX=' + H['fixture-tests.trx'], '--filter-substring', 'Subtract']),
        ('handoff', ['handoff', '--repo', T, '--file', 'H=' + inp + '/handoff.json', '--sha256', 'H=' + H['handoff.json'], '--schema-rev', TIP]),
        ('freetext', ['freetext', '--repo', T, '--file', 'H=' + inp + '/handoff.json', '--sha256', 'H=' + H['handoff.json'], '--terms-rev', MAIN]),
        ('clausemap', ['clausemap', '--repo', T, '--main', MAIN, '--state-rev', BASE, '--state-path', STATE,
                       '--file', 'CONTRACT=' + inp + '/K.json', '--sha256', 'CONTRACT=' + H['K.json'], '--base', BASE]),
    ]


def build_closure(T, inp, H, k):
    """Closure (rackcad-bmin-closure) as e2e.py builds it, computed with replacement disabled."""
    nr = ('--no-replace-objects',)
    commits = [c for c in gout(T, *nr, 'rev-list', '--all').split() if c]
    AR = k['AuthorityRevision']
    MB = gout(T, *nr, 'merge-base', MAIN, AR)
    EFF1, EFF2 = gout(T, *nr, 'rev-parse', MAIN + '^1'), gout(T, *nr, 'rev-parse', MAIN + '^2')
    named = []
    for c in (TIP, BASE, HEADX, RED, MAIN, EFF1, EFF2, AR, MB):
        if c and c not in named:
            named.append(c)
    m = json.loads(git(T, *nr, 'show', MAIN + ':' + MAP).stdout.decode('utf-8'))
    map_paths = sorted(set([f['Path'] for f in m.get('Files', [])] + [e['Path'] for e in m.get('Entries', [])]))
    revpaths = [{'Rev': c, 'Path': p} for c in named for p in gout(T, *nr, 'ls-tree', '-r', '--name-only', '--full-tree', c).splitlines() if p]
    closure = {'Schema': 'rackcad-bmin-closure/0-experimental', 'Commits': commits, 'RevPaths': revpaths,
               'Refs': ['HEAD', 'refs/remotes/origin/' + BR, 'refs/remotes/origin/main'],
               'Paths': [IGN, K, STATE, DEC, 'src/Fixture.Lib/Calculator.cs', 'docs/AUTOMATION_PLAN.md', 'docs/WORKFLOW.md',
                         MAP, 'docs/automation/agent-execution/compatibility/clause-map.schema.json',
                         'docs/automation/agent-execution/schemas/worker-handoff.schema.json'] + [c['Path'] for c in k.get('Authorities', [])]
                        + SURFACES + map_paths,
               'Files': [{'Name': n, 'Sha256': H[f]} for n, f in (('DELEG', 'deleg.json'), ('CONTRACT', 'K.json'), ('H', 'handoff.json'),
                                                                  ('TRX', 'fixture-tests.trx'))]}
    wjson_local(inp + '/closure.json', closure)
    return inp + '/closure.json'


def exe_sha256():
    try:
        return sha256(rbytes(sys.executable)), 'sys.executable'
    except OSError:
        return sha256(rbytes(os.path.join(sys.base_prefix, os.path.basename(sys.executable)))), 'sys.base_prefix'


def build_request(path, repo, runs, closure, relay, gitdir=None, verification=None, na=None, tool_path=None, run_from='object'):
    xsha, _ = exe_sha256()
    tool = {'PinnedBlob': B, 'RunFrom': run_from}
    if run_from == 'object' or gitdir:
        tool['GitObject'] = {'GitDir': gitdir or repo, 'Blob': B}
    if tool_path:
        tool['Path'] = tool_path
    req = {'Schema': 'rackcad-bmin-accept-request/0-experimental', 'Repo': repo, 'Tool': tool,
           'Interpreter': {'Python': platform.python_version(), 'ExecutableSha256': xsha},
           'Runs': [{'Name': n, 'Envelope': e, 'Argv': a} for n, e, a in runs],
           'Closure': {'Path': closure, 'Sha256': sha256(rbytes(closure))},
           'RelayFacts': {'Path': relay, 'CitedSha256': sha256(rbytes(relay))}}
    if verification:
        req['Verification'] = verification
    if na:
        req['NotApplicable'] = na
    wjson_local(path, req)
    return path


# ------------------------------------------------------------------ processes (AP 16.4 style query, Win32_Process)
_PS_PROCS = r'''
$ErrorActionPreference = 'Stop'
$needles = @($env:R1R4_NEEDLES -split '\|' | Where-Object { $_ })
$parents = @($env:R1R4_PARENTS -split '\|' | Where-Object { $_ } | ForEach-Object { [int]$_ })
$all = @(Get-CimInstance Win32_Process | Select-Object ProcessId, ParentProcessId, Name, CommandLine, CreationDate)
$byId = @{}; foreach ($p in $all) { $byId[[int]$p.ProcessId] = $p }
$anc = @{}; $cur = $PID
while ($byId.ContainsKey($cur) -and -not $anc.ContainsKey($cur)) { $anc[$cur] = 1; $cur = [int]$byId[$cur].ParentProcessId }
$closed = @('git.exe', 'python.exe', 'python3.exe', 'pythonw.exe', 'pwsh.exe', 'powershell.exe', 'bash.exe', 'cmd.exe', 'codex.exe', 'claude.exe', 'node.exe', 'dotnet.exe')
$match = @(); $unreadable = @(); $children = @()
foreach ($p in $all) {
  $id = [int]$p.ProcessId
  if ($anc.ContainsKey($id)) { continue }
  $cd = if ($p.CreationDate) { $p.CreationDate.ToUniversalTime().ToString('o') } else { $null }
  if ($parents -contains [int]$p.ParentProcessId) { $children += [pscustomobject]@{ Pid = $id; ParentPid = [int]$p.ParentProcessId; Name = $p.Name; CreationDateUtc = $cd } }
  $cl = $p.CommandLine
  if (-not $cl) {
    if ($closed -contains $p.Name.ToLower()) {
      $pp = [int]$p.ParentProcessId; $par = $byId[$pp]
      $gp = if ($par) { $byId[[int]$par.ParentProcessId] } else { $null }
      $unreadable += [pscustomobject]@{ Pid = $id; Name = $p.Name; ParentPid = $pp; ParentName = $(if ($par) { $par.Name } else { $null });
                                        GrandParentName = $(if ($gp) { $gp.Name } else { $null }); CreationDateUtc = $cd;
                                        ParentCommandLineHasNeedle = $(if ($par -and $par.CommandLine) { $pn = $par.CommandLine.ToLower().Replace('/', '\'); [bool]($needles | Where-Object { $pn.Contains($_) }) } else { $null }) }
    }
    continue
  }
  $n = $cl.ToLower().Replace('/', '\')
  foreach ($x in $needles) { if ($n.Contains($x)) { $match += [pscustomobject]@{ Pid = $id; ParentPid = [int]$p.ParentProcessId; Name = $p.Name; CommandLine = $cl; CreationDateUtc = $cd }; break } }
}
[pscustomobject]@{ CheckerPid = $PID; Total = $all.Count; Matching = $match; ChildrenOfGivenParents = $children; UnreadableClosedList = $unreadable } | ConvertTo-Json -Depth 4 -Compress
'''


def procs_check(paths, parent_pids=()):
    needles = '|'.join(os.path.normpath(p).lower().replace('/', '\\') for p in paths)
    env = genv({'R1R4_NEEDLES': needles, 'R1R4_PARENTS': '|'.join(str(x) for x in parent_pids)})
    enc = base64.b64encode(_PS_PROCS.encode('utf-16-le')).decode('ascii')
    p = subprocess.run([PWSH, '-NoLogo', '-NoProfile', '-NonInteractive', '-EncodedCommand', enc], stdin=DEVNULL,
                       capture_output=True, env=env, timeout=300)
    try:
        r = json.loads(p.stdout.decode('utf-8', 'replace'))
    except Exception:
        return {'Error': 'process query failed', 'Stderr': p.stderr.decode('utf-8', 'replace')[:500], 'UtcTime': utc()}
    for k in ('Matching', 'ChildrenOfGivenParents', 'UnreadableClosedList'):
        v = r.get(k)
        r[k] = [] if v is None else (v if isinstance(v, list) else [v])
    r['Needles'] = [os.path.normpath(p) for p in paths]
    r['UtcTime'] = utc()
    return r


# ------------------------------------------------------------------ acceptor
def run_acceptor(req_path, snap_repos=(), proc_paths=()):
    before = [snapshot(r) for r in snap_repos]
    t0 = utc()
    pr = subprocess.Popen([sys.executable, '-I', PIN_ACC, '--request', req_path], stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                          stdin=DEVNULL, env=genv(), cwd=WORK)
    out, err = pr.communicate(timeout=3600)
    t1 = utc()
    after = [snapshot(r) for r in snap_repos]
    procs = procs_check(list(proc_paths), [pr.pid]) if proc_paths else None
    try:
        res = json.loads(out.decode('ascii'))
    except Exception:
        res = {'Raw': out.decode('utf-8', 'replace')[:2000], 'Stderr': err.decode('utf-8', 'replace')[:2000]}
    wbytes(req_path[:-5] + '.acceptor-stdout.json', out)
    checks = dict((c.get('Id'), c.get('Status')) for c in res.get('Checks', []))
    return {'Request': req_path, 'RequestSha256': sha256(rbytes(req_path)), 'Rc': pr.returncode, 'Pid': pr.pid, 'StartUtc': t0, 'EndUtc': t1,
            'Decision': res.get('Decision'), 'Checks': checks, 'Error': res.get('Error'),
            'StdoutSha256': sha256(out), 'StdoutPath': req_path[:-5] + '.acceptor-stdout.json',
            'ZeroWrite': [{'Repo': r, 'Equal': snapdiff(b, a) == {}, 'Diff': snapdiff(b, a)} for r, b, a in zip(snap_repos, before, after)],
            'Processes': procs, '_Result': res}


def check_detail(acc, rid):
    for c in acc['_Result'].get('Checks', []):
        if c.get('Id') == rid:
            return c.get('Detail')
    return None


def strip_private(acc):
    d = dict(acc)
    d.pop('_Result', None)
    return d


# ------------------------------------------------------------------ versions
def versions():
    def first(cmd):
        try:
            p = subprocess.run(cmd, capture_output=True, stdin=DEVNULL, timeout=120)
            return p.stdout.decode('utf-8', 'replace').strip().splitlines()[0] if p.stdout.strip() else None
        except Exception as e:
            return 'unavailable: ' + type(e).__name__
    return {'Git': first(['git', '--version']), 'Python': platform.python_version(), 'PythonImplementation': platform.python_implementation(),
            'Pwsh': first([PWSH, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', '$PSVersionTable.PSVersion.ToString()']),
            'WindowsPowerShell': first([PS51, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', '$PSVersionTable.PSVersion.ToString()']),
            'Cmd': (lambda s: (re.search(r'\d+\.\d+\.\d+(\.\d+)?', s or '') or re.search('', '')).group(0) or s)(first([CMD, '/d', '/c', 'ver'])),
            'Os': platform.platform(),
            'PwshPath': PWSH, 'GitPath': shutil.which('git'), 'PythonOnPath': shutil.which('python')}
