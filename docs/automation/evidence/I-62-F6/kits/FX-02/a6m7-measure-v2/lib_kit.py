# lib_kit.py - shared helpers of the A6-7 measurement kit v2 (I-62, A-6 and A-7 AGREED, decisiones §90 and §100). Stdlib only.
# Constants of the measurement M (designation a7-designation/4, Kind MEASUREMENT, I.TaskId = T1-m7), sanitizing, canonical JSON,
# git through the DECLARED git by full path with every GIT_* variable removed, and the zero-write snapshot of the -C.
# Never writes outside the kit directory and its -C D:\r62-a6m7\m7-run2 (and the temporary areas the self-tests create under the kit).
import datetime, hashlib, json, os, re, shutil, stat, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
KIT = HERE.replace('\\', '/')
OUT = KIT + '/out'
FROZEN = KIT + '/frozen'
INPUTS = KIT + '/inputs'
MARKER = KIT + '/M7_LAUNCHED'                       # P-07: created before the launch, never deleted

# ------------------------------------------------------------------ the -C of the measurement and its source
CDIR = 'D:/r62-a6m7/m7-run2'                       # kit v2: own -C (v1: D:/r62-a6m7/m7-run, untouched)
CDIR_WIN = 'D:\\r62-a6m7\\m7-run2'
SRC_DELIV = 'D:/r62-a6r2/r3/deliv'                   # read only (r2 part (d) delivery repository)
R2_INP = 'D:/r62-a6r2/r3/runs/d/inputs'              # read only (round-2 inputs: delegation, delivery, TRX)
W_REL = '.codex/worktrees/architecture-portabilidad-coordinador-principal'

# ------------------------------------------------------------------ pinned pieces (A-6 §3.3)
B = '9a829b0eeb1210238052b1feac06cfa8f7964aed'
B_SHA256 = 'b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200'
B12 = B[:12]                                         # abbreviated name for the delivery check (never the full id)
ACC = '434ef1fccb0d15762b30244b4a60928120698411'
ACC_SHA256 = '173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0'
INSTRUMENT = {'Path': 'docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py', 'Blob': B}

# ------------------------------------------------------------------ scenario (r2 part (d) / r4 / bench m7)
TIP = '95bdc29d428b35d2188a10e1c5462d6caf2be4df'     # D.BaseSha = H.BaseSha
RED = '53436cb20adf1fe87dc91b0382dd3123a1fac44f'     # H.RedSha
G = 'b417d36cb820a27c78d6c7d50c234460469c89f8'       # H.CurrentSha = I.Target.Commit
MAIN = 'fbe25347799e5b801ef708454212335537448bd1'    # D.MainSha = K.MainSha
AR = '1746b404e57d69170ecfc084fd24fa3e699916d7'      # AuthorityRevision
BR = 'fx/u1'
K = 'docs/automation/decisions/FX-U1-T1.gate-contract.json'
K_BLOB = '628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0'
DEC = 'docs/automation/decisions/FX-U1.md'
STATE = 'docs/automation/state/FX-U1.yml'
MAP = 'docs/automation/agent-execution/compatibility/I62-clause-map.json'
HANDOFF_SCHEMA = 'docs/automation/agent-execution/schemas/worker-handoff.schema.json'
AP = 'docs/AUTOMATION_PLAN.md'
TARGET_PATH = 'src/Fixture.Lib/Calculator.cs'
TARGET_BLOB = '380892ee869194aa4bd56dcf32dfd76ef0203ab9'
DELIV_FILES = ['docs/automation/evidence/FX-U1-agent/T1/worker-notes.md', 'src/Fixture.Lib/Calculator.cs',
               'tests/Fixture.Tests/CalculatorTests.cs']
TRX_E = 'artifacts/ci/R37949808974/fixture-tests.trx'   # E identifier of the TRX (designation path)
UNIT, TASK, ATTEMPT = 'FX-U1', 'T1-m7', 0
D_RUN = 'R20261012T090000Z-d001'
H_RUN = 'R20261012T091000Z-a001'
RUN_ID = 'R20261028T024440Z-27a9'                    # RunId of M (kit v2: the AP 16.6 form R<UTC>Z-<4 hex>; decisiones §92, punto 6)
DECISION_REF = 'I62-A6-7-M7'                         # DecisionRef that the Coordinator's measurement disposition carries
ORDERS = ['identity', 'diff', 'scope', 'hashes', 'cleantree', 'handoff', 'freetext', 'clausemap', 'trx', 'read-text', 'read-json']
# kit v2: ORDERS is the DECLARED order set, the B-min orders without `contract` (D-1; decisiones §100, punto 3). A run whose Argv[0] is
# outside it is not reconstructible (reconstruct_runs_m7.py v3). The instrument executions of the measurement are the 11 orders plus two
# read-json --file reads of CONTRACT and DELEG with the eight pointers of Contract (how the Controller keeps verifying Contract, with U-19,
# without `contract`).
CONTRACT_POINTERS = ['/AllowedWriteScope', '/ForbiddenWriteScope', '/Invariants', '/StopConditions', '/Authorities', '/RequiredTests',
                     '/RoleRequirements', '/SupersededCommits']
EXEC_LABELS = ORDERS + ['read-json-contract', 'read-json-deleg']
EXEC_ORDER = dict([(n, n) for n in ORDERS] + [('read-json-contract', 'read-json'), ('read-json-deleg', 'read-json')])
JSON_POINTERS = ['/Objective', '/Schema', '/MainSha', '/AuthorityRevision']
HANDOFF_FIELDS = ['TaskId', 'Attempt', 'DelegationRunId', 'RunId', 'BaseSha', 'RedSha', 'CurrentSha', 'Branch', 'Worktree', 'Worker']


def exec_label(argv):
    """Execution label of an instrument argv: its order, or read-json-contract / read-json-deleg for the two read-json --file reads."""
    a = list(argv or [])
    if a[:1] == ['read-json'] and '--file' in a and a.index('--file') + 1 < len(a):
        return 'read-json-' + a[a.index('--file') + 1].split('=', 1)[0].lower()
    return a[0] if a else None


def deliv_path(run, name):
    return 'artifacts/orchestration/%s/%s/%s/%s/%s' % (UNIT, TASK, ATTEMPT, run, name)


def invocation():
    return {'UnitId': UNIT, 'TaskId': TASK, 'Attempt': ATTEMPT, 'RunId': RUN_ID,
            'Target': {'Commit': G, 'Path': TARGET_PATH, 'Blob': TARGET_BLOB}}


IGNORE_PATH = 'artifacts/orchestration/%s/%s/%s/%s/output.json' % (UNIT, TASK, ATTEMPT, RUN_ID)
RUN_PATH = IGNORE_PATH[:-len('output.json')]
DELEG_E = deliv_path(D_RUN, 'output.json')            # KPR-2 path of the delegation copy (= E path)
H_E = deliv_path(H_RUN, 'worker-handoff.json')        # KPR-2 path of the delivery copy (= E path)
TRX_PLACE = RUN_PATH + 'ci-current/fixture-tests.trx'  # ignored location of the TRX copy in the -C (v5 precedent)
PLACED = {'DELEG': DELEG_E, 'H': H_E, 'TRX': TRX_PLACE}
CUSTODY = {'DELEG': INPUTS + '/deleg.json', 'H': INPUTS + '/handoff.json', 'TRX': INPUTS + '/fixture-tests.trx'}

# ------------------------------------------------------------------ basics
NL = '\n'
_EMAIL = re.compile(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}')


def utc():
    return datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ')


def sha256(b):
    return hashlib.sha256(b).hexdigest()


def blob_sha1(b):
    return hashlib.sha1(b'blob ' + str(len(b)).encode('ascii') + b'\0' + b).hexdigest()


def rbytes(p):
    with open(p, 'rb') as fh:
        return fh.read()


def sha256_file(p):
    return sha256(rbytes(p))


def wbytes(p, b):
    d = os.path.dirname(p)
    if d:
        os.makedirs(d, exist_ok=True)
    with open(p, 'wb') as fh:
        fh.write(b)


def canon(o):
    return json.dumps(o, ensure_ascii=True, sort_keys=True, separators=(',', ':'))


def rjson(p):
    return json.loads(rbytes(p).decode('utf-8'))


def rmtree(p):
    def onexc(f, q, e):
        os.chmod(q, stat.S_IWRITE)
        f(q)
    if os.path.isdir(p):
        shutil.rmtree(p, onexc=onexc)


# ------------------------------------------------------------------ hygiene (no user name, no e-mail address)
def _user_tokens():
    toks = set()
    user = os.path.basename((os.environ.get('USERPROFILE') or '').rstrip('\\/'))
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
    for env, name in (('LOCALAPPDATA', '%LOCALAPPDATA%'), ('USERPROFILE', '%USERPROFILE%')):
        v = (os.environ.get(env) or '').rstrip('\\/')
        if len(v) > 3:
            for form in (v, v.replace('\\', '/'), v.replace('\\', '\\\\')):
                out = re.sub(re.escape(form), name.replace('\\', '\\\\'), out, flags=re.IGNORECASE)
    for tok in _USER_TOKENS:
        out = re.sub(re.escape(tok), '<usuario>', out, flags=re.IGNORECASE)
    return _EMAIL.sub('<email>', out)


def sanitize(obj):
    if isinstance(obj, dict):
        return dict((sanitize_str(k) if isinstance(k, str) else k, sanitize(v)) for k, v in obj.items())
    if isinstance(obj, (list, tuple)):
        return [sanitize(x) for x in obj]
    if isinstance(obj, str):
        return sanitize_str(obj)
    return obj


def hygienic(text):
    return sanitize_str(text) == text


def wjson(p, obj, pretty=True):
    """Deliverable JSON: sanitized, sorted keys, ASCII, LF. Refuses to write if a user name or an address would remain."""
    o = sanitize(obj)
    s = (json.dumps(o, ensure_ascii=True, indent=1, sort_keys=True) if pretty else canon(o)) + NL
    if not hygienic(s):
        raise SystemExit('hygiene: refusing to write %s' % p)
    wbytes(p, s.encode('ascii'))
    return p


def expand(s):
    """%USERPROFILE% / %LOCALAPPDATA% placeholders of the kit parameters, expanded at run time (never stored expanded)."""
    for env in ('USERPROFILE', 'LOCALAPPDATA'):
        s = s.replace('%' + env + '%', (os.environ.get(env) or '').rstrip('\\/'))
    return s


def params():
    p = rjson(KIT + '/m7-params.json')
    return p


def w_path():
    return os.path.join(os.environ['USERPROFILE'], *W_REL.split('/'))


# ------------------------------------------------------------------ git (declared git by full path, GIT_* removed)
def genv(extra=None, base=None):
    e = dict((k, v) for k, v in (base if base is not None else os.environ).items() if not k.upper().startswith('GIT_'))
    e['GIT_OPTIONAL_LOCKS'] = '0'
    e['GIT_TERMINAL_PROMPT'] = '0'
    if extra:
        e.update(extra)
    return e


def declared_git():
    return expand(params()['Git']['Path'])


def git(repo, *args, inp=None, exe=None, timeout=900):
    cmd = [exe or declared_git(), '--no-replace-objects'] + (['-C', repo] if repo else []) + [str(a) for a in args]
    if inp is None:
        return subprocess.run(cmd, stdin=subprocess.DEVNULL, capture_output=True, env=genv(), timeout=timeout)
    return subprocess.run(cmd, input=inp, capture_output=True, env=genv(), timeout=timeout)


def gout(repo, *args, **kw):
    return git(repo, *args, **kw).stdout.decode('utf-8', 'replace').strip()


def glines(repo, *args, **kw):
    return [x for x in git(repo, *args, **kw).stdout.decode('utf-8', 'replace').replace('\r\n', '\n').split('\n') if x]


# ------------------------------------------------------------------ zero-write snapshot (A6-3 rule 1: refs incl. refs/replace,
# HEAD, index SHA-256, status with ignored, file list; plus every git-dir file and the local/worktree config)
def _walk(root, skip=()):
    out = {}
    sk = set(os.path.normcase(os.path.normpath(x)) for x in skip)
    for r, ds, fs in os.walk(root):
        ds[:] = sorted(d for d in ds if os.path.normcase(os.path.normpath(os.path.join(r, d))) not in sk)
        for f in sorted(fs):
            q = os.path.join(r, f)
            b = rbytes(q)
            out[os.path.relpath(q, root).replace(os.sep, '/')] = [len(b), sha256(b)]
    return out


def _dirs(root, skip=()):
    out = []
    sk = set(os.path.normcase(os.path.normpath(x)) for x in skip)
    for r, ds, fs in os.walk(root):
        ds[:] = sorted(d for d in ds if os.path.normcase(os.path.normpath(os.path.join(r, d))) not in sk)
        for d in ds:
            out.append(os.path.relpath(os.path.join(r, d), root).replace(os.sep, '/'))
    return sorted(out)


def config_files(T, exe=None):
    gd = os.path.normpath(gout(T, 'rev-parse', '--path-format=absolute', '--git-dir', exe=exe))
    cd = os.path.normpath(gout(T, 'rev-parse', '--path-format=absolute', '--git-common-dir', exe=exe))
    out = {}
    for role, p in (('Local', os.path.join(cd, 'config')), ('Worktree', os.path.join(gd, 'config.worktree'))):
        out[role] = {'Exists': os.path.isfile(p), 'Sha256': sha256_file(p) if os.path.isfile(p) else None}
    return out


def snapshot(T, exe=None):
    top = os.path.normpath(gout(T, 'rev-parse', '--show-toplevel', exe=exe))
    gd = os.path.normpath(gout(T, 'rev-parse', '--path-format=absolute', '--git-dir', exe=exe))
    cd = os.path.normpath(gout(T, 'rev-parse', '--path-format=absolute', '--git-common-dir', exe=exe))
    refs = git(T, 'for-each-ref', '--format=%(refname) %(objectname) %(symref)', exe=exe).stdout.decode('utf-8', 'replace').splitlines()
    idx = os.path.join(gd, 'index')
    st = git(T, 'status', '--porcelain=v1', '-z', '--ignored', exe=exe).stdout
    skip = [os.path.join(top, '.git')]
    gfiles = _walk(cd)
    if not os.path.normcase(gd).startswith(os.path.normcase(cd)):
        for k, v in _walk(gd).items():
            gfiles['gitdir:' + k] = v
    return {'Schema': 'i62-a6m7-snapshot/1', 'Toplevel': top.replace('\\', '/'), 'Refs': refs,
            'HeadSymref': gout(T, 'symbolic-ref', '-q', 'HEAD', exe=exe), 'Head': gout(T, 'rev-parse', 'HEAD', exe=exe),
            'IndexSha256': sha256_file(idx) if os.path.isfile(idx) else None,
            'StatusZ': [x for x in st.decode('utf-8', 'replace').split('\0') if x],
            'WorktreeFiles': _walk(top, skip), 'WorktreeDirs': _dirs(top, skip), 'GitFiles': gfiles,
            'Config': config_files(T, exe=exe)}


SNAP_KEYS = ('Toplevel', 'Refs', 'HeadSymref', 'Head', 'IndexSha256', 'StatusZ', 'WorktreeDirs', 'Config')


def snapdiff(a, b):
    d = {}
    for k in SNAP_KEYS:
        if a.get(k) != b.get(k):
            d[k] = {'Before': a.get(k), 'After': b.get(k)}
    for k in ('WorktreeFiles', 'GitFiles'):
        x, y = a[k], b[k]
        add = sorted(set(y) - set(x))
        rem = sorted(set(x) - set(y))
        mod = sorted(p for p in set(x) & set(y) if x[p] != y[p])
        if add or rem or mod:
            d[k] = {'Added': add, 'Removed': rem, 'Modified': mod}
    return d


def snap_sha(s):
    return sha256((canon(s) + NL).encode('ascii'))


def loose_rel(oid):
    return 'objects/' + oid[:2] + '/' + oid[2:]


def only_loose_object(d, oid):
    return set(d) == {'GitFiles'} and d['GitFiles'] == {'Added': [loose_rel(oid)], 'Removed': [], 'Modified': []}


# ------------------------------------------------------------------ instrument / acceptor bytes from the I-62 repository (by blob)
def pinned_bytes(blob, want_sha256):
    p = git(w_path(), 'cat-file', 'blob', blob)
    if p.returncode != 0 or blob_sha1(p.stdout) != blob or sha256(p.stdout) != want_sha256:
        raise SystemExit('pinned blob not readable or wrong: ' + blob)
    return p.stdout
