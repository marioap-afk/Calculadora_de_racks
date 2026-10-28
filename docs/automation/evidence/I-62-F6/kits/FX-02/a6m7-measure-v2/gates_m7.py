# gates_m7.py - pre-launch gates and identity measurement of the A6-7 measurement kit (I-62). Stdlib only.
# Never executes Codex: the Codex binary is only hashed. The fingerprint of config.toml is measured by cfg-fp.ps1 (v5 kit, byte
# copy), which emits the SHA-256 and the sanitized key names only, never a value.
#   python -I gates_m7.py gates  --mode launch|dry-run [--fixture <dir>]  -> out/gates.json; exit 0 = every gate passed, 2 = refused
#   python -I gates_m7.py measure <tag> [--fixture <dir>]                -> out/<tag>-identity.json; exit 0 = identity = trio, 3 = differs
# Fixture mode (dry-run and self-tests only) replaces the sources (decisions file, Codex bin root, config home, AppX version,
# Claude projects root, -C, out dir) and the expected identity; launch mode reads only the kit's own parameters and refuses any
# override. The dry-run never reaches a launch: run_m7.sh stops after the gates.
import base64, glob, importlib.util, json, os, re, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


L = _load('lib_kit')
DEFAULT_PATHEXT = '.COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC;.CPL'
MARKER_KEYS = ('M7-CELDA', 'M7-TRIO', 'M7-INSTRUMENTO', 'M7-INTERPRETE', 'M7-GIT', 'M7-C', 'M7-CIERRE', 'M7-RECONSTRUCCION',
               'M7-ESCRITURA', 'M7-KIT')
FREEZE_EXCLUDE_DIRS = ('out', '.selftest', '.build-tmp', '__pycache__')
FREEZE_EXCLUDE_FILES = ('FREEZE.json', 'M7_LAUNCHED')


class Config(object):
    """Every source and expectation of the gates. real(): the kit parameters only. fixture(): dry-run / self-tests."""

    def __init__(self):
        p = L.params()
        self.mode = 'launch'
        self.kit = L.KIT
        self.out = L.OUT
        self.marker = L.MARKER
        self.repo = p['Repo']
        self.frozen_snapshot = L.FROZEN + '/m7-run.snapshot.json'
        self.decisions = L.expand(p['DecisionsFile'])
        self.real_decisions = self.decisions
        self.codex_path = L.expand(p['Codex']['Path'])
        self.real_codex_path = self.codex_path
        self.codex_bin_root = L.expand(p['Codex']['BinRoot'])
        self.cfg_home = None                       # None: the real %USERPROFILE% (only at the measurement)
        self.appx_version = None                   # None: Get-AppxPackage (only at the measurement)
        self.claude_projects = L.expand(p['ClaudeProjectsRoot'])
        self.sessions_root = L.expand(p['SessionsRoot'])
        self.runtime_pwsh = L.expand(p['RuntimePwsh']['Path'])
        self.expect = {'BinaryHash': p['Trio']['BinaryHash'], 'AppVersion': p['Trio']['AppVersion'],
                       'Fingerprint': p['Trio']['Fingerprint'], 'Companions': p['Codex']['CompanionsSha256First16'],
                       'RuntimePwshSha256': p['RuntimePwsh']['Sha256']}
        self.baseline = L.KIT + '/' + p['Codex']['KeyNamesBaseline']
        self.git = L.expand(p['Git']['Path'])
        self.py = L.expand(p['Interpreter']['Path'])
        self.params = p
        self.check_processes = True
        self.codex_proc_name = 'codex.exe'
        self.static_dirs_extra = []
        self.timeout_s = p['TimeoutS']

    @classmethod
    def fixture(cls, fixdir, mode):
        c = cls()
        c.mode = mode
        f = L.rjson(os.path.join(fixdir, 'fixture.json'))
        for k, v in f.get('Set', {}).items():
            setattr(c, k, v)
        c.expect.update(f.get('Expect', {}))
        if os.path.normcase(os.path.abspath(c.decisions)) == os.path.normcase(os.path.abspath(c.real_decisions)):
            raise SystemExit('fixture mode never reads the real decisions file')
        if os.path.normcase(os.path.abspath(c.codex_path)) == os.path.normcase(os.path.abspath(c.real_codex_path)):
            raise SystemExit('fixture mode never uses the real Codex binary path')
        return c


# ------------------------------------------------------------------ identity (trio, single binary, companions, key names)
def measure(cfg, tag, outdir):
    os.makedirs(outdir, exist_ok=True)
    rec = {'Tag': tag, 'UtcTime': L.utc()}
    env = dict(os.environ)
    if cfg.cfg_home:
        env['USERPROFILE'] = cfg.cfg_home
    cfgjson = os.path.join(outdir, tag + '-cfg.json')
    try:
        p = subprocess.run(['pwsh', '-NoProfile', '-File', os.path.join(cfg.kit, 'cfg-fp.ps1'), '-Out', cfgjson],
                           capture_output=True, stdin=subprocess.DEVNULL, env=env, timeout=120)
        rec['Fingerprint'] = p.stdout.decode('ascii', 'replace').strip()
        kn = json.loads(L.rbytes(cfgjson).decode('utf-8-sig'))['KeyNames']
        rec['KeyNamesEqualBaseline'] = kn == json.loads(L.rbytes(cfg.baseline).decode('utf-8'))
        rec['KeyNamesCount'] = len(kn)
    except Exception as e:
        rec['Fingerprint'] = None
        rec['KeyNamesEqualBaseline'] = False
        rec['FingerprintError'] = type(e).__name__
    bins = sorted(glob.glob(os.path.join(cfg.codex_bin_root, '*', 'codex.exe')))
    rec['Bins'] = len(bins)
    rec['BinaryHash'] = L.sha256_file(cfg.codex_path) if os.path.isfile(cfg.codex_path) else None
    d = os.path.dirname(cfg.codex_path)
    lines = ''.join('%s %s\n' % (os.path.basename(f), L.sha256_file(f)) for f in sorted(glob.glob(os.path.join(d, '*.exe'))))
    rec['CompanionLines'] = lines.splitlines()
    rec['Companions'] = L.sha256(lines.encode('utf-8'))[:16]
    if cfg.appx_version is not None:
        rec['AppVersion'] = cfg.appx_version
    else:
        q = subprocess.run(['powershell.exe', '-NoProfile', '-Command', '(Get-AppxPackage %s).Version' % cfg.params['Codex']['AppxQuery']],
                           capture_output=True, stdin=subprocess.DEVNULL, timeout=120)
        rec['AppVersion'] = q.stdout.decode('utf-8', 'replace').strip()
    rec['RuntimePwshSha256'] = L.sha256_file(cfg.runtime_pwsh) if os.path.isfile(cfg.runtime_pwsh) else None
    diffs = []
    for k in ('BinaryHash', 'AppVersion', 'Fingerprint', 'Companions', 'RuntimePwshSha256'):
        if rec.get(k) != cfg.expect[k]:
            diffs.append(k)
    if rec['Bins'] != 1:
        diffs.append('Bins')
    if not rec['KeyNamesEqualBaseline']:
        diffs.append('KeyNames')
    rec['Differs'] = diffs
    rec['EqualsTrio'] = not diffs
    L.wjson(os.path.join(outdir, tag + '-identity.json'), rec)
    return rec


# ------------------------------------------------------------------ decisions file: disposition markers and Owner lines
def expected_markers(cfg):
    """The literal markers the Coordinator's measurement disposition must contain (built from the frozen values)."""
    f = L.rjson(os.path.join(cfg.kit, 'disposition-markers.json'))
    fz = os.path.join(cfg.kit, 'FREEZE.json')
    return list(f['Markers']) + ['M7-KIT: FREEZE.json SHA-256 ' + (L.sha256_file(fz) if os.path.isfile(fz) else '<ausente>')]


def find_disposition(text, markers, after_section):
    """The section (## N., N > after_section) that contains every marker; Owner lines are checked later against its heading."""
    lines = text.replace('\r\n', '\n').split('\n')
    heads = [(i, int(m.group(1))) for i, l in enumerate(lines) for m in [re.match(r'^## (\d+)\.\s', l)] if m]
    pos90 = [i for i, n in heads if n == after_section]
    res = {'Section90Line': pos90[0] + 1 if pos90 else None, 'Candidates': []}
    if not pos90:
        return None, res
    sections = []
    for k, (i, n) in enumerate(heads):
        end = heads[k + 1][0] if k + 1 < len(heads) else len(lines)
        if n > after_section and i > pos90[0]:
            sections.append((i, end, n))
    hits = []
    for i, end, n in sections:
        body = '\n'.join(lines[i:end])
        missing = [m for m in markers if m not in body]
        res['Candidates'].append({'Section': n, 'Line': i + 1, 'Missing': [m.split(':', 1)[0] for m in missing]})
        if not missing:
            hits.append((i, end, n))
    if len(hits) != 1:
        return None, res
    return hits[0], res


def check_decisions(cfg):
    out = {'File': cfg.decisions}
    probs = []
    if not os.path.isfile(cfg.decisions):
        return {'Ok': False, 'Problems': ['decisions file absent']}
    text = L.rbytes(cfg.decisions).decode('utf-8')
    lines = text.replace('\r\n', '\n').split('\n')
    markers = expected_markers(cfg)
    for m in markers:
        if len(m) < 20:
            probs.append('marker too short to identify a record: %s' % m[:20])
    hit, info = find_disposition(text, markers, cfg.params['LastSectionBeforeDisposition'])
    out['Search'] = info
    if hit is None:
        probs.append('no single Coordinator disposition after section %d contains every marker' % cfg.params['LastSectionBeforeDisposition'])
    else:
        i, end, n = hit
        out['Disposition'] = {'Section': n, 'Line': i + 1}
        body = lines[i:end]
        # A6-5 rule 6 readings disposition: M7-LECTURAS: ... §<N> with an existing later section that names A6-5, regla 6
        lect = [l for l in body if 'M7-LECTURAS:' in l]
        ref = None
        if len(lect) == 1:
            mm = re.search(r'M7-LECTURAS:.*?§\s?(\d+)', lect[0])
            ref = int(mm.group(1)) if mm else None
        if ref is None:
            probs.append('M7-LECTURAS: no single reference to the A6-5 rule 6 readings disposition (§N)')
        else:
            heads = [(k, int(x.group(1))) for k, l in enumerate(lines) for x in [re.match(r'^## (\d+)\.\s', l)] if x]
            sec = [k for k, num in heads if num == ref]
            ok = False
            if ref > cfg.params['LastSectionBeforeDisposition'] and len(sec) == 1:
                k0 = sec[0]
                nxt = [k for k, _ in heads if k > k0]
                stext = '\n'.join(lines[k0:(nxt[0] if nxt else len(lines))])
                ok = 'A6-5, regla 6' in stext
            out['ReadingsDisposition'] = {'Section': ref, 'Ok': ok}
            if not ok:
                probs.append('M7-LECTURAS: section §%d does not exist after §%d or does not name A6-5, regla 6' % (ref, cfg.params['LastSectionBeforeDisposition']))
        # Owner lines: complete lines of the file, after the disposition heading
        for name in ('line-od2.txt', 'line-b.txt'):
            p = os.path.join(cfg.kit, name)
            want = L.rbytes(p).decode('utf-8')
            if '\n' in want or '\r' in want:
                probs.append('%s must be one line without a newline' % name)
                continue
            at = [k for k, l in enumerate(lines) if l == want]
            after = [k for k in at if k > i]
            out[name] = {'CompleteLineAt': [k + 1 for k in at], 'AfterDisposition': bool(after)}
            if not after:
                probs.append('%s is not a complete line of the decisions file after the disposition' % name)
    od2 = L.rbytes(os.path.join(cfg.kit, 'line-od2.txt')).decode('utf-8')
    lb = L.rbytes(os.path.join(cfg.kit, 'line-b.txt')).decode('utf-8')
    t = cfg.params['Trio']
    if t['Fingerprint'] not in od2 or t['BinaryHash'] not in od2:
        probs.append('line-od2.txt does not name the fingerprint and the binary of the trio')
    if '%s/%s/%s' % (t['BinaryHash'], t['AppVersion'], t['Fingerprint']) not in lb or L.B not in lb:
        probs.append('line-b.txt does not name the exact trio in order and the instrument blob')
    if 'gpt-6-luna/high' not in lb:
        probs.append('line-b.txt does not name the cell gpt-6-luna/high')
    out['Problems'] = probs
    out['Ok'] = not probs
    return out


# ------------------------------------------------------------------ kit integrity (FREEZE.json)
def kit_files(kit):
    out = []
    for r, ds, fs in os.walk(kit):
        rel = os.path.relpath(r, kit).replace(os.sep, '/')
        if rel != '.' and rel.split('/')[0] in FREEZE_EXCLUDE_DIRS:
            ds[:] = []
            continue
        ds[:] = sorted(d for d in ds if not (rel == '.' and d in FREEZE_EXCLUDE_DIRS))
        for f in sorted(fs):
            q = (f if rel == '.' else rel + '/' + f)
            if q in FREEZE_EXCLUDE_FILES:
                continue
            out.append(q)
    return sorted(out)


def check_kit(cfg):
    fp = os.path.join(cfg.kit, 'FREEZE.json')
    if not os.path.isfile(fp):
        return {'Ok': False, 'Problems': ['FREEZE.json absent']}
    fr = L.rjson(fp)
    probs = []
    files = kit_files(cfg.kit)
    want = fr.get('Files', {})
    for f in files:
        if f not in want:
            probs.append('file not frozen: ' + f)
        elif L.sha256_file(os.path.join(cfg.kit, f)) != want[f]['Sha256']:
            probs.append('file changed after the freeze: ' + f)
    for f in want:
        if f not in files:
            probs.append('frozen file missing: ' + f)
    return {'Ok': not probs, 'Problems': probs, 'FreezeSha256': L.sha256_file(fp), 'Files': len(files)}


# ------------------------------------------------------------------ P-07, state of the -C, static git scan, runtime pwsh
def check_p07(cfg):
    probs = []
    if os.path.lexists(cfg.marker):
        probs.append('P-07: launch marker present (tope 1, no retry)')
    for n in ('events.jsonl', 'launch.json', 'raw/events.jsonl'):
        if os.path.lexists(os.path.join(cfg.out, n)):
            probs.append('P-07: a previous launch left out/' + n)
    return {'Ok': not probs, 'Problems': probs}


def check_state(cfg):
    probs = []
    frozen = L.rjson(cfg.frozen_snapshot)
    cur = L.snapshot(cfg.repo, exe=cfg.git)
    d = L.snapdiff(frozen, cur)
    ok_obj = L.only_loose_object(d, L.B)
    if not ok_obj:
        probs.append('state of the -C is not the frozen snapshot plus exactly the instrument loose object: %s' % json.dumps(sorted(d))[:300])
    pw = os.path.join(cfg.out, 'prep-write.json')
    pwrec = L.rjson(pw) if os.path.isfile(pw) else None
    if not pwrec or not pwrec.get('Held') or pwrec.get('DecisionRef') != L.DECISION_REF:
        probs.append('no held preparation write (A6-3 rule 1) with DecisionRef %s in out/prep-write.json' % L.DECISION_REF)
    elif pwrec.get('AfterSnapshotSha256') != L.snap_sha(cur):
        probs.append('the -C changed after the preparation write')
    if not os.path.isfile(os.path.join(cfg.out, 'registered-config.json')):
        probs.append('no registered config of the preparation write (out/registered-config.json)')
    return {'Ok': not probs, 'Problems': probs, 'Diff': sorted(d), 'CurrentSnapshotSha256': L.snap_sha(cur)}, cur


def scan_dir(d, stems):
    rec = {'Dir': d}
    try:
        names = os.listdir(d)
        rec['Method'] = 'listdir'
        rec['Matches'] = sorted(n for n in names if any(n.lower() == s or n.lower().startswith(s + '.') for s in stems))
    except FileNotFoundError:
        rec['Method'] = 'absent'
        rec['Matches'] = []
    except OSError as e:
        rec['Method'] = 'unestablished: ' + type(e).__name__
        rec['Matches'] = None
    return rec


def check_static_git(cfg, launch_env=None):
    """No git.* in the directories that come before PATH in the CreateProcess search of the instrument's interpreter (its image
    directory and its alias directory), System32, System and Windows, the -C root, and the runtime pwsh directory."""
    pc = _load('pou_check')
    rows = []
    probs = []
    try:
        img, _ = pc.resolve_executable(cfg.py)
        img_dir = os.path.dirname(img)
    except Exception as e:
        img_dir = None
        probs.append('interpreter image directory unestablished: ' + str(e))
    windir = os.environ.get('SystemRoot') or os.environ.get('windir') or 'C:\\Windows'
    dirs = [os.path.dirname(cfg.py)] + ([img_dir] if img_dir else []) + [os.path.join(windir, 'System32'), os.path.join(windir, 'System'),
                                                                     windir, cfg.repo, os.path.dirname(cfg.runtime_pwsh)] + cfg.static_dirs_extra
    for d in dirs:
        r = scan_dir(d, ['git'])
        rows.append(r)
        if r['Matches'] is None:
            probs.append('cannot establish the contents of ' + d)
        elif r['Matches']:
            probs.append('git.* found in %s: %s' % (d, r['Matches']))
    return {'Ok': not probs, 'Problems': probs, 'Dirs': rows}


def check_runtime_pwsh(cfg):
    ok = os.path.isfile(cfg.runtime_pwsh) and L.sha256_file(cfg.runtime_pwsh) == cfg.expect['RuntimePwshSha256']
    return {'Ok': ok, 'Problems': [] if ok else ['runtime pwsh absent or with another SHA-256']}


def check_claude_project(cfg):
    name = re.sub(r'[^A-Za-z0-9]', '-', L.CDIR_WIN if cfg.repo == L.CDIR else cfg.repo.replace('/', '\\'))
    p = os.path.join(cfg.claude_projects, name)
    return {'Ok': not os.path.lexists(p), 'Problems': [] if not os.path.lexists(p) else ['a Claude project directory exists for the -C'],
            'Path': p}


# ------------------------------------------------------------------ processes (P-02)
_PS = r'''
$ErrorActionPreference = 'Stop'
$needles = @($env:M7_NEEDLES -split '\|' | Where-Object { $_ })
$all = @(Get-CimInstance Win32_Process | Select-Object ProcessId, ParentProcessId, Name, CommandLine)
$byId = @{}; foreach ($p in $all) { $byId[[int]$p.ProcessId] = $p }
$anc = @{}; $cur = $PID
while ($byId.ContainsKey($cur) -and -not $anc.ContainsKey($cur)) { $anc[$cur] = 1; $cur = [int]$byId[$cur].ParentProcessId }
$rows = @()
foreach ($p in $all) {
  $id = [int]$p.ProcessId
  if ($anc.ContainsKey($id)) { continue }
  $cl = [string]$p.CommandLine
  $n = $cl.ToLower().Replace('/', '\')
  $hit = $false; foreach ($x in $needles) { if ($x -and $n.Contains($x)) { $hit = $true } }
  if ($p.Name -ieq $env:M7_PROCNAME -or $hit) {
    $rows += [pscustomobject]@{ Pid = $id; ParentPid = [int]$p.ParentProcessId; Name = $p.Name; CommandLineReadable = [bool]$cl; MatchesNeedle = $hit }
  }
}
ConvertTo-Json -InputObject @($rows) -Depth 3 -Compress
'''


def procs(needles, procname='codex.exe'):
    env = dict(os.environ)
    env['M7_PROCNAME'] = procname
    env['M7_NEEDLES'] = '|'.join(os.path.normpath(x).lower() for x in needles)
    enc = base64.b64encode(_PS.encode('utf-16-le')).decode('ascii')
    p = subprocess.run(['pwsh', '-NoLogo', '-NoProfile', '-NonInteractive', '-EncodedCommand', enc], capture_output=True,
                       stdin=subprocess.DEVNULL, env=env, timeout=300)
    try:
        r = json.loads(p.stdout.decode('utf-8', 'replace') or '[]')
    except ValueError:
        return None
    return r if isinstance(r, list) else [r]


def check_processes(cfg):
    if not cfg.check_processes:
        return {'Ok': True, 'Problems': [], 'Skipped': 'fixture mode'}
    rows = procs([cfg.repo, cfg.repo.replace('/', '\\')], cfg.codex_proc_name)
    if rows is None:
        return {'Ok': False, 'Problems': ['process query failed']}
    codex = [r for r in rows if str(r.get('Name', '')).lower() == cfg.codex_proc_name.lower()]
    onc = [r for r in rows if r.get('MatchesNeedle')]
    probs = []
    if codex:
        probs.append('P-02: %d %s process(es) alive (no other %s may run at the launch)' % (len(codex), cfg.codex_proc_name, cfg.codex_proc_name))
    if onc:
        probs.append('P-02: %d process(es) with the -C in their command line' % len(onc))
    return {'Ok': not probs, 'Problems': probs, 'Rows': rows}


# ------------------------------------------------------------------ launch environment and armed prompt
def check_launch_env(cfg, env_path):
    if not os.path.isfile(env_path):
        return {'Ok': False, 'Problems': ['launch environment not captured (out/raw/launch-env.json)']}, None
    env = json.loads(L.rbytes(env_path).decode('utf-8'))
    probs = []
    gitvars = sorted(k for k in env if k.upper().startswith('GIT_'))
    if gitvars:
        probs.append('launch environment has GIT_* variables: ' + ','.join(gitvars))
    path = env.get('PATH') or env.get('Path') or ''
    first = path.split(';')[0] if path else ''
    if os.path.normcase(os.path.normpath(first)) != os.path.normcase(os.path.normpath(os.path.dirname(cfg.runtime_pwsh))):
        probs.append('the runtime pwsh directory is not the first PATH entry of the child')
    return {'Ok': not probs, 'Problems': probs, 'GitVariables': gitvars, 'PathFirst': first,
            'NoDefaultCurrentDirectoryInExePath': env.get('NoDefaultCurrentDirectoryInExePath', env.get('NODEFAULTCURRENTDIRECTORYINEXEPATH'))}, env


def check_prompt(cfg):
    T = _load('tools_m7')
    text = T.arm_prompt(cfg.git, cfg.py)
    data = text.encode('utf-8')
    fr = L.rjson(os.path.join(cfg.kit, 'FREEZE.json'))
    want = fr.get('ArmedPrompt', {}).get('Sha256')
    os.makedirs(os.path.join(cfg.out, 'raw'), exist_ok=True)
    L.wbytes(os.path.join(cfg.out, 'raw', 'prompt.txt'), data)
    probs = []
    if L.sha256(data) != want:
        probs.append('armed prompt differs from the frozen one')
    return {'Ok': not probs, 'Problems': probs, 'Sha256': L.sha256(data), 'Chars': len(text)}


# ------------------------------------------------------------------ all gates
def gates(cfg):
    res = {'Schema': 'i62-a6m7-gates/1', 'Mode': cfg.mode, 'UtcTime': L.utc()}
    order = []

    def put(name, r):
        res[name] = r
        order.append(name)

    put('Kit', check_kit(cfg))
    put('Decisions', check_decisions(cfg))
    ident = measure(cfg, 'pre-launch', cfg.out)
    put('IdentityBefore', {'Ok': ident['EqualsTrio'], 'Problems': ['identity differs: ' + ','.join(ident['Differs'])] if ident['Differs'] else [],
                           'Record': ident})
    put('P07', check_p07(cfg))
    st, cur = check_state(cfg)
    put('State', st)
    put('RuntimePwsh', check_runtime_pwsh(cfg))
    put('StaticGit', check_static_git(cfg))
    le, env = check_launch_env(cfg, os.path.join(cfg.out, 'raw', 'launch-env.json'))
    put('LaunchEnv', le)
    put('Processes', check_processes(cfg))
    put('ClaudeProject', check_claude_project(cfg))
    put('Prompt', check_prompt(cfg))
    res['Order'] = order
    res['Refused'] = [n for n in order if not res[n]['Ok']]
    res['Pass'] = not res['Refused']
    L.wjson(os.path.join(cfg.out, 'gates.json'), res)
    return res


def main(argv):
    if len(argv) < 2:
        sys.stderr.write(__doc__ or '')
        return 64
    fixture = None
    if '--fixture' in argv:
        fixture = argv[argv.index('--fixture') + 1]
    mode = argv[argv.index('--mode') + 1] if '--mode' in argv else 'launch'
    if mode == 'launch' and fixture:
        sys.stderr.write('REFUSED: launch mode takes no fixture\n')
        return 2
    if mode not in ('launch', 'dry-run', 'selftest-launch'):
        return 64
    if mode != 'launch' and not fixture:
        sys.stderr.write('REFUSED: %s needs --fixture\n' % mode)
        return 2
    cfg = Config.fixture(fixture, mode) if fixture else Config()
    if argv[1] == 'env-sh':
        # bash assignments for run_m7.sh (paths only; no secret, no value of config.toml)
        def q(s):
            return "'" + str(s).replace("'", "'\\''") + "'"
        print('OUT=%s\nRAW=%s\nREPO=%s\nREPO_WIN=%s\nMARKER=%s\nCODEX=%s\nRTPS_DIR_WIN=%s\nIGN=%s\nMODEL=%s\nEFFORT=%s\nTIMEOUT_S=%d\nKILL_AFTER=%d\nPYDECL=%s'
              % (q(cfg.out), q(cfg.out + '/raw'), q(cfg.repo), q(cfg.repo.replace('/', '\\')), q(cfg.marker), q(cfg.codex_path),
                 q(os.path.dirname(cfg.runtime_pwsh)), q(L.IGNORE_PATH), q(cfg.params['Cell']['Model']), q(cfg.params['Cell']['Effort']),
                 cfg.timeout_s, cfg.params['KillAfterS'], q(cfg.py)))
        return 0
    if argv[1] == 'decl':
        shell = getattr(cfg, 'pou_shell', None) or cfg.runtime_pwsh
        d = {'Blob': L.B, 'Git': {'Path': cfg.git, 'Version': cfg.params['Git']['Version'], 'Sha256': cfg.params['Git']['Sha256'],
                                  'InvocationName': cfg.params['Git']['InvocationName']},
             'Interpreter': {'Path': cfg.py, 'Version': cfg.params['Interpreter']['Version'], 'Sha256': cfg.params['Interpreter']['Sha256'],
                             'InvocationName': cfg.params['Interpreter']['InvocationName']},
             'Shell': {'Path': shell}}
        os.makedirs(os.path.join(cfg.out, 'raw'), exist_ok=True)
        L.wbytes(os.path.join(cfg.out, 'raw', 'decl.json'), (json.dumps(d, ensure_ascii=True, indent=1) + '\n').encode('ascii'))
        return 0
    if argv[1] == 'snap':
        L.wbytes(argv[2], (L.canon(L.snapshot(cfg.repo, exe=cfg.git)) + '\n').encode('ascii'))
        return 0
    if argv[1] == 'procs':
        r = check_processes(cfg)
        L.wjson(os.path.join(cfg.out, 'procs-%s.json' % (argv[2] if len(argv) > 2 and not argv[2].startswith('--') else 'check')), r)
        print('PROCS ' + ('NONE' if r['Ok'] else '; '.join(r['Problems'])))
        return 0 if r['Ok'] else 2
    if argv[1] == 'killtree':
        rows = procs([cfg.repo, cfg.repo.replace('/', '\\')], cfg.codex_proc_name) or []
        killed = []
        for r in rows:
            if str(r.get('Name', '')).lower() == cfg.codex_proc_name.lower() and r.get('MatchesNeedle'):
                k = subprocess.run(['taskkill', '/PID', str(r['Pid']), '/T', '/F'], capture_output=True, stdin=subprocess.DEVNULL)
                killed.append({'Pid': r['Pid'], 'Rc': k.returncode})
        L.wjson(os.path.join(cfg.out, 'killtree.json'), {'UtcTime': L.utc(), 'Killed': killed})
        print('KILLTREE %d' % len(killed))
        return 0
    if argv[1] == 'gates':
        r = gates(cfg)
        for n in r['Refused']:
            sys.stderr.write('REFUSED %s: %s\n' % (n, '; '.join(r[n]['Problems'])[:600]))
        print('GATES ' + ('PASS' if r['Pass'] else 'REFUSED: ' + ','.join(r['Refused'])))
        return 0 if r['Pass'] else 2
    if argv[1] == 'measure':
        r = measure(cfg, argv[2], cfg.out)
        print('MEASURE %s %s' % (argv[2], 'EQUAL' if r['EqualsTrio'] else 'DIFFERS: ' + ','.join(r['Differs'])))
        return 0 if r['EqualsTrio'] else 3
    return 64


if __name__ == '__main__':
    sys.exit(main(sys.argv))
