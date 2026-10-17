# selftest_m7.py - self-tests of the A6-7 measurement kit (I-62), WITHOUT launching anything: Codex is never executed (the
# launch step runs only with a stub binary compiled into a temporary directory under the kit), the runtime pwsh of Codex is
# never executed (the PATH pwsh 7 stands for it), config.toml, auth.json and the sessions are never read (fixtures), and the
# measurement -C (D:\r62-a6m7\m7-run) is only read: every write goes to scratch copies under <kit>/.selftest, removed at the end.
#   python -I selftest_m7.py [--keep]      -> selftest-report.json (canonical, sanitized); exit 0 iff every case holds
import base64, copy, glob, importlib.util, json, os, re, shutil, subprocess, sys, time, uuid

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


L = _load('lib_kit')
for _k in [k for k in os.environ if k.upper().startswith('GIT_')]:
    del os.environ[_k]
T7 = _load('tools_m7')
G7 = _load('gates_m7')
R1 = _load('reconstruct_runs')
R2 = _load('reconstruct_runs_m7')
TM = _load('table_m7')
P = L.params()
ST = L.KIT + '/.selftest'
CASES = []
PWSH7 = shutil.which('pwsh')
DEC = T7.declared()
W = L.w_path()
V5_LOG = os.path.join(W, 'docs', 'automation', 'evidence', 'I-62-F6', 'OD-2', 'R20261011T102825Z-a2p2-v5-controller', 'probe-events.jsonl')


def case(group, name, expected, observed, holds):
    CASES.append({'Group': group, 'Case': name, 'Expected': expected, 'Observed': observed, 'Holds': bool(holds)})
    print(('ok   ' if holds else 'FAIL ') + group + ' ' + name, flush=True)


def sh(argv, **kw):
    return subprocess.run(argv, capture_output=True, stdin=subprocess.DEVNULL, **kw)


# ------------------------------------------------------------------ A. static
def group_static():
    reuse = {'derive_r4.py': '3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426',
             'designate_m7.py': '491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6',
             'make_eic_verify.py': '58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2',
             'table_m7.py': 'a0765733be3e140f01afe1818d3a1ff630e83cb1970acba22e593f322574c1e3',
             'table_r3.py': 'ec4d9a8ec0f665f74660bf517526b0a1c097efe630feda1f83297c8661851c9b',
             'table_r4.py': 'e1efb54a7f9189c715780cfd130f2ea8540e70d9c5739adff3aa12a5bb5f2797',
             'reconstruct_runs.py': '5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da',
             'pou_check.py': 'c3490925c63d24747952663470725487a6e45b90e2128e122910d3e49d81db7d',
             'cfg-fp.ps1': 'b1d833c0abc06fec6e27367b6df2715e7cdca321f0d6c6fbadb9f634e6a31bab',
             'procs-snapshot.ps1': 'f879db305dd296d30241cca75cf61ac0a5828077cd2be6277e4128f934afedaf',
             'baseline-keynames-F909F480.json': '076637c550ff0b3109447ef00263ff087749f0aaa71f299d524e43b8cf82b70b',
             'line-od2.txt': '3b33dd56cbae5fddab9a9431e7ad9b83170e6782e0161385bc324789634b7f25',
             'line-b.txt': 'dda4b3f9c1db0183a38e5400777a2a21d524aa4b92ad684d5c5cae1b2fc4cf27'}
    got = dict((f, L.sha256_file(L.KIT + '/' + f)) for f in reuse)
    case('A', 'REUSE-BYTES', 'every reused file byte-identical to its source (bench, v5 kit, r2static, Owner lines)',
         dict((f, got[f] == reuse[f]) for f in reuse), got == reuse)
    bad = []
    for f in sorted(glob.glob(L.KIT + '/*.py')):
        try:
            compile(L.rbytes(f).decode('utf-8'), f, 'exec')
        except SyntaxError as e:
            bad.append(os.path.basename(f))
    for f in ('run_m7.sh', 'selftest_m7.sh'):
        if sh(['bash', '-n', L.KIT + '/' + f]).returncode != 0:
            bad.append(f)
    case('A', 'SYNTAX', 'all kit Python files compile; bash -n on run_m7.sh and selftest_m7.sh', bad or 'none', not bad)
    full, strict = L.rjson(T7.SCHEMA_FULL), L.rjson(T7.SCHEMA_STRICT)
    probs = []

    def audit(s, ptr):
        if isinstance(s, dict):
            for k in s:
                if k not in ('type', 'enum', 'required', 'properties', 'additionalProperties', 'items', 'minLength', 'pattern', 'minimum',
                             'description', 'title'):
                    probs.append('%s: keyword %s' % (ptr, k))
            if s.get('type') == 'object':
                if s.get('additionalProperties') is not False or sorted(s.get('required', [])) != sorted(s.get('properties', {})):
                    probs.append(ptr + ': object not strict')
            for k, v in (s.get('properties') or {}).items():
                audit(v, ptr + '/' + k)
            if 'items' in s:
                audit(s['items'], ptr + '/items')
    audit(full, '#')
    audit(strict, '#')
    proj = T7.build_schema(True)
    case('A', 'SCHEMA', 'full schema and strict projection: keyword subset of the v5 schemas; every object closed with all properties required; '
         'projection = full without minLength; files = the generated ones',
         probs[:5] or 'no problem', not probs and strict == proj and full == T7.build_schema(False) and '"minLength":' not in json.dumps(strict)
         and '"minLength":' in json.dumps(full))
    text = T7.arm_prompt(DEC['Git'], DEC['Py'])
    orders = T7.order_argvs()
    rec_ok, ref_runtime = [], []
    for name, argv in orders:
        inner = T7.inner_cmd(argv, DEC['Git'], DEC['Py'])
        cmd = R2.render_command([DEC['Pwsh'], '-Command', inner])
        p = R2.parse_instrument_command(cmd, L.B)
        q = R1.parse_instrument_command(R2.render_command([PWSH7, '-Command', inner]), L.B)
        try:
            R1.parse_instrument_command(cmd, L.B)
            ref_runtime.append('reconstructible')
        except R1.NotReconstructible as e:
            ref_runtime.append(str(e))
        rec_ok.append(p['Argv'] == argv and q['Argv'] == argv and inner in text)
    case('A', 'REFERENCE-RUNTIME-SHELL', 'FINDING: rendered with the runtime pwsh of Codex (%USERPROFILE%\\.cache\\...\\pwsh.exe) the reference rule '
         'rejects every order (component .cache); v2 reconstructs them', sorted(set(ref_runtime)),
         set(ref_runtime) == {'outer: shell is not an absolute path to pwsh.exe'})
    case('A', 'PROMPT', 'armed prompt: ASCII, no double quote, the full blob only in the 12 orders, abbreviated name in C2, no oracle value, every '
         'order as written in the prompt reconstructible by v2 (runtime pwsh) and by the reference (a pwsh path without dot components) to the same '
         'argv; SHA-256 = FREEZE',
         {'Chars': len(text), 'BlobCount': text.count(L.B), 'Sha256': L.sha256(text.encode('utf-8'))},
         all(ord(c) < 128 for c in text) and '"' not in text and text.count(L.B) == 12 and L.B12 in text and all(rec_ok)
         and 'acceptor' not in text.lower() and 'aceptante' not in text.lower() and 'SPEC' not in text and 'controller-verification' not in text
         and L.sha256(text.encode('utf-8')) == L.rjson(L.KIT + '/FREEZE.json')['ArmedPrompt']['Sha256'])
    src = L.rbytes(L.KIT + '/tools_m7.py').decode('utf-8')
    oracle_src = src[src.index('def oracle('):src.index('# ------------------------------------------------------------------ events helpers')]
    hits = re.findall(r'bmin_\w+|pinned_bytes|run_acceptor|_load\(|L\.ACC\b|SPEC\.md|reconstruct', oracle_src)
    case('A', 'ORACLE-INDEPENDENT', 'the oracle code loads neither the instrument nor the acceptor nor the SPEC (only git plumbing through the declared '
         'git, hashlib, json, xml.etree on the custodied bytes)', hits or 'no reference', not hits)


# ------------------------------------------------------------------ B. reconstruction rule v2 against the reference
KEYS = ['Seq', 'ItemId', 'Argv', 'Shell', 'Git', 'Interpreter', 'ExitCode', 'Status', 'ErrorRun', 'EnvelopeError', 'EnvelopeSha256', 'NoiseLines', 'Problem']


def rbrief(res):
    return {'R': res['Reconstructible'], 'P': res['Problems'], 'C': res['CompletenessProblems'],
            'Runs': [dict((k, r.get(k)) for k in KEYS) for r in res['Runs']]}


def group_recon():
    files = sorted(glob.glob('D:/r62-a6r3/r1/s-recon/*-events.jsonl')) + sorted(glob.glob('D:/r62-a6r4/r1/s-*/session-events.jsonl')) \
        + sorted(glob.glob('D:/r62-a6m7/bench/work/s-*/session-events.jsonl')) \
        + sorted(glob.glob(os.path.join(W, 'docs', 'automation', 'evidence', 'I-62-F6', 'OD-2*', '*', '*events.jsonl')))
    same, diffs = 0, []
    ok_runs = None
    for f in files:
        b = L.rbytes(f)
        a, c = rbrief(R1.reconstruct(b, L.B)), rbrief(R2.reconstruct(b, L.B))
        if a == c:
            same += 1
        else:
            diffs.append((os.path.basename(f), a['R'], c['R'], a['P'][:1], c['P'][:1]))
        if f.endswith('s-recon/OK-events.jsonl') or f.endswith('s-recon\\OK-events.jsonl'):
            ok_runs = c['Runs']
    exp_diff = sorted(x[0] for x in diffs)
    fo = [x for x in diffs if x[0] == 'F-outer-flags-events.jsonl']
    fc = [x for x in diffs if x[0] == 'F-cmd-shell-events.jsonl']
    case('B', 'V2-EQ-REFERENCE', 'every r3/r4/bench/custodied log: same reconstruction as the reference (Reconstructible, Runs, Problems), except '
         'F-outer-flags (the -NoProfile rendering v2 adds) and F-cmd-shell (both not reconstructible; message differs: cmd with &)',
         {'Logs': len(files), 'Identical': same, 'Different': exp_diff},
         exp_diff == ['F-cmd-shell-events.jsonl', 'F-outer-flags-events.jsonl'] and fo and fo[0][1] is False and fo[0][2] is True
         and fc and fc[0][1] is False and fc[0][2] is False and same == len(files) - 2)
    # F-outer-flags under v2 = the OK log runs (argv and envelopes)
    b = L.rbytes('D:/r62-a6r3/r1/s-recon/F-outer-flags-events.jsonl')
    v = rbrief(R2.reconstruct(b, L.B))
    case('B', 'V2-NOPROFILE-R3CASE', 'F-outer-flags (r3) under v2: reconstructible with the argv and envelope SHA-256 of OK',
         {'Reconstructible': v['R'], 'Runs': len(v['Runs'])},
         v['R'] and [(r['Argv'], r['EnvelopeSha256']) for r in v['Runs']] == [(r['Argv'], r['EnvelopeSha256']) for r in ok_runs])
    # new renderings and malformed variants, on the OK log of r3
    okb = L.rbytes('D:/r62-a6r3/r1/s-recon/OK-events.jsonl')

    def rewrite(fn):
        out = []
        for line in okb.split(b'\n'):
            if not line.strip():
                continue
            e = json.loads(line.decode('utf-8'))
            it = e.get('item')
            if isinstance(it, dict) and it.get('type') == 'command_execution' and L.B in it.get('command', ''):
                p = R1.parse_instrument_command(it['command'], L.B)
                it['command'] = fn(p)
            out.append(json.dumps(e, ensure_ascii=False, separators=(',', ':')))
        return ('\n'.join(out) + '\n').encode('utf-8')

    def pw_inner(p):
        return '& ' + T7.pw_sq(p['Git']) + ' --no-replace-objects cat-file blob ' + L.B + ' | & ' + T7.pw_sq(p['Interpreter']) + ' -I - ' + ' '.join(p['Argv'])

    def cmd_inner(p):
        return '"' + p['Git'] + '" --no-replace-objects cat-file blob ' + L.B + ' | "' + p['Interpreter'] + '" -I - ' + ' '.join(p['Argv'])
    cmdexe = 'C:\\WINDOWS\\system32\\cmd.exe'
    good = {
        'R2-NoProfile': lambda p: R2.render_command([p['Shell'], '-NoProfile', '-Command', pw_inner(p)]),
        'R3-cmd': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p)]),
        'R1-runtime-pwsh': lambda p: R2.render_command([DEC['Pwsh'], '-Command', pw_inner(p)]),
        'R2-runtime-pwsh': lambda p: R2.render_command([DEC['Pwsh'], '-NoProfile', '-Command', pw_inner(p)]),
    }
    for name, fn in good.items():
        v = rbrief(R2.reconstruct(rewrite(fn), L.B))
        ref = rbrief(R1.reconstruct(rewrite(fn), L.B))
        case('B', 'V2-NEW-' + name, 'new rendering: v2 reconstructs with the argv and envelopes of OK; the reference does not',
             {'V2': v['R'], 'Reference': ref['R']},
             v['R'] and not ref['R'] and [(r['Argv'], r['EnvelopeSha256']) for r in v['Runs']] == [(r['Argv'], r['EnvelopeSha256']) for r in ok_runs])
    bad = {
        'cmd-amp': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p) + ' & echo x']),
        'cmd-percent': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p) + ' %TEMP%']),
        'cmd-caret': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p).replace(' | ', ' ^| ')]),
        'cmd-bang': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p) + ' !x!']),
        'cmd-redirect': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p) + ' > x.json']),
        'cmd-unquoted-git': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p).replace('"' + p['Git'] + '"', p['Git'])]),
        'cmd-bare-python': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p).replace('"' + p['Interpreter'] + '"', '"python"')]),
        'cmd-quoted-arg': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p).replace(' --repo ', ' "--repo" ')]),
        'cmd-second-pipe': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p) + ' | more']),
        'cmd-noI': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p).replace(' -I - ', ' - ')]),
        'cmd-noNRO': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p).replace(' --no-replace-objects', '')]),
        'cmd-double-space': lambda p: R2.render_command([cmdexe, '/c', cmd_inner(p).replace(' | ', '  | ')]),
        'cmd-upper-C': lambda p: R2.render_command([cmdexe, '/C', cmd_inner(p)]),
        'cmd-d-c': lambda p: R2.render_command([cmdexe, '/d', '/c', cmd_inner(p)]),
        'cmd-s-c': lambda p: R2.render_command([cmdexe, '/s', '/c', cmd_inner(p)]),
        'cmd-relative': lambda p: R2.render_command(['cmd.exe', '/c', cmd_inner(p)]),
        'cmd-pwsh-inner': lambda p: R2.render_command([cmdexe, '/c', pw_inner(p)]),
        'pwsh-noprofile-lower': lambda p: R2.render_command([p['Shell'], '-noprofile', '-Command', pw_inner(p)]),
        'pwsh-noprofile-noninteractive': lambda p: R2.render_command([p['Shell'], '-NoProfile', '-NonInteractive', '-Command', pw_inner(p)]),
        'pwsh-command-then-noprofile': lambda p: R2.render_command([p['Shell'], '-Command', '-NoProfile', pw_inner(p)]),
        'pwsh-nologo-command': lambda p: R2.render_command([p['Shell'], '-NoLogo', '-Command', pw_inner(p)]),
        'powershell51-noprofile': lambda p: R2.render_command(['C:\\WINDOWS\\System32\\WindowsPowerShell\\v1.0\\powershell.exe', '-NoProfile', '-Command', pw_inner(p)]),
        'pwsh-noprofile-stmt': lambda p: R2.render_command([p['Shell'], '-NoProfile', '-Command', pw_inner(p) + '; exit 0']),
        'pwsh-noprofile-bare-git': lambda p: R2.render_command([p['Shell'], '-NoProfile', '-Command', pw_inner(p).replace(T7.pw_sq(p['Git']), 'git')]),
        'shell-dotdot': lambda p: R2.render_command(['C:\\x\\..\\y\\pwsh.exe', '-Command', pw_inner(p)]),
        'shell-dot': lambda p: R2.render_command(['C:\\x\\.\\pwsh.exe', '-Command', pw_inner(p)]),
        'shell-trailing-dot': lambda p: R2.render_command(['C:\\x.\\pwsh.exe', '-Command', pw_inner(p)]),
        'shell-dots-only': lambda p: R2.render_command(['C:\\...\\pwsh.exe', '-Command', pw_inner(p)]),
        'git-dot-component': lambda p: R2.render_command([p['Shell'], '-Command', pw_inner(dict(p, Git='C:\\u\\.cache\\git.exe'))]),
    }
    res = {}
    for name, fn in bad.items():
        v = R2.reconstruct(rewrite(fn), L.B)
        res[name] = (v['Reconstructible'], (v['Problems'] or [''])[0][:90])
    enc = rewrite(lambda p: R2.render_command([p['Shell'], '-EncodedCommand', base64.b64encode(pw_inner(p).encode('utf-16-le')).decode()]))
    e1, e2 = R1.reconstruct(enc, L.B), R2.reconstruct(enc, L.B)
    case('B', 'V2-ENCODED-NOT-IDENTIFIED', 'LIMIT (unchanged from the reference): an execution that does not name the blob (-EncodedCommand) is '
         'not identified; reference and v2 both count 0 executions (the comparator then reports the 12 orders as not executed)',
         {'Reference': e1['InstrumentExecutions'], 'V2': e2['InstrumentExecutions']}, e1['InstrumentExecutions'] == 0 and e2['InstrumentExecutions'] == 0)
    case('B', 'V2-MALFORMED', 'every malformed or unlisted rendering fails closed under v2 (cmd with & %% ^ ! >, unquoted or bare paths, quoted args, '
         'second pipe, no -I, no --no-replace-objects, double space, /C, /d /c, /s /c, relative cmd, pwsh inner under cmd, other pwsh flags or order, '
         'powershell.exe 5.1, statement, bare git, -EncodedCommand)', dict((k, v[1]) for k, v in res.items()),
         all(v[0] is False for v in res.values()))


# ------------------------------------------------------------------ fixtures: scratch -C copies, decisions, identity, stub
def fixture_decisions(path, drop=(), alter=None, owner=('line-od2.txt', 'line-b.txt'), owner_before=False, lect='§92', lect_section=True,
                      two=False, no90=False, owner_mut=None):
    markers = G7.expected_markers(G7.Config())
    lines = ['# I-62 decisiones (fixture del autotest; no es el archivo real)', '']
    if not no90:
        lines += ['## 90. Disposicion del Coordinator: A-6 AGREED (fixture)', '', 'texto', '']
    ol = []
    for n in owner:
        t = L.rbytes(L.KIT + '/' + n).decode('utf-8')
        ol.append(owner_mut(n, t) if owner_mut else t)
    if owner_before:
        lines += ol + ['']
    body = ['## 91. Disposicion del Coordinator: medicion A6-7 del Controller de FX-02 (fixture)', '']
    for m in markers:
        if m.split(':', 1)[0] in drop:
            continue
        body.append('- ' + (alter(m) if alter else m))
    if lect is not None:
        body.append('- M7-LECTURAS: disposicion de A6-5, regla 6: ' + lect)
    lines += body + ['']
    if two:
        lines += [body[0].replace('## 91.', '## 93.')] + body[1:] + ['']
    if not owner_before:
        lines += ol + ['']
    if lect_section:
        lines += ['## 92. Disposicion del Coordinator: lecturas normativas del aceptante (A6-5, regla 6) (fixture)', '', 'texto', '']
    L.wbytes(path, ('\n'.join(lines) + '\n').encode('utf-8'))
    return path


def make_identity_fixture(d):
    """Fake Codex bin root (dummy codex.exe + the stub), fake config home, fake AppX version; expectations computed from them."""
    os.makedirs(d, exist_ok=True)
    home = d + '/home'
    L.wbytes(home + '/.codex/config.toml', b'[a]\nx = 1\n')
    L.wbytes(d + '/baseline.json', json.dumps(['[a]', 'a.x'], indent=1).encode('ascii'))
    return home


def build_stub(bindir):
    src = r'''
using System; using System.IO; using System.Text; using System.Threading;
class P { static int Main(string[] a) {
  string d = AppDomain.CurrentDomain.BaseDirectory; var u = new UTF8Encoding(false);
  File.WriteAllText(Path.Combine(d, "cmdline.txt"), Environment.CommandLine, u);
  File.WriteAllText(Path.Combine(d, "path.txt"), Environment.GetEnvironmentVariable("PATH") ?? "", u);
  if (a.Length > 0) File.WriteAllText(Path.Combine(d, "prompt.txt"), a[a.Length - 1], u);
  var sb = new StringBuilder(); for (int i = 0; i < a.Length - 1; i++) sb.Append(a[i]).Append('\n');
  File.WriteAllText(Path.Combine(d, "args.txt"), sb.ToString(), u);
  string o = null; for (int i = 0; i < a.Length - 1; i++) if (a[i] == "-o") o = a[i + 1];
  if (File.Exists(Path.Combine(d, "sleep"))) Thread.Sleep(300000);
  if (o != null && File.Exists(Path.Combine(d, "output.json"))) File.Copy(Path.Combine(d, "output.json"), o, true);
  var ev = File.ReadAllBytes(Path.Combine(d, "events.jsonl")); var so = Console.OpenStandardOutput(); so.Write(ev, 0, ev.Length); so.Flush();
  return 0; } }
'''
    os.makedirs(bindir, exist_ok=True)
    L.wbytes(bindir + '/stub.cs', src.encode('ascii'))
    csc = os.path.join(os.environ.get('SystemRoot', 'C:\\Windows'), 'Microsoft.NET', 'Framework64', 'v4.0.30319', 'csc.exe')
    p = sh([csc, '/nologo', '/out:' + bindir.replace('/', '\\') + '\\m7stubcodex.exe', bindir.replace('/', '\\') + '\\stub.cs'])
    if p.returncode != 0:
        raise SystemExit('stub build failed: ' + p.stdout.decode('utf-8', 'replace')[:300])
    shutil.copyfile(bindir + '/m7stubcodex.exe', bindir + '/codex.exe')      # dummy for the Bins count; never executed
    return bindir + '/m7stubcodex.exe'


def launch_env():
    """The launch environment the runner builds: runtime pwsh dir first, then the Windows PATH (Machine;User), no GIT_*."""
    q = sh(['powershell.exe', '-NoProfile', '-Command',
            "[Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [Environment]::GetEnvironmentVariable('Path','User')"])
    winpath = q.stdout.decode('utf-8', 'replace').strip()
    e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    e['PATH'] = os.path.dirname(DEC['Pwsh']) + ';' + winpath
    return e


def noise_line():
    for line in L.rbytes(V5_LOG).split(b'\n'):
        if not line.strip():
            continue
        e = json.loads(line.decode('utf-8'))
        it = e.get('item') or {}
        if e['type'] == 'item.completed' and it.get('type') == 'command_execution':
            ao = it['aggregated_output']
            return ao[:ao.index('\n') + 1]
    raise SystemExit('no noise line')


# ------------------------------------------------------------------ S. synthetic session: the 17 executions really run in pwsh 7
def run_session(T, env):
    """C1..C17 exactly as the prompt orders them, through the PATH pwsh 7 (never the runtime pwsh), cwd = T, launch environment.
    Returns the Codex-format log (the shell rendered as the runtime pwsh, like Codex records it) and the raw executions."""
    if os.path.normcase(os.path.dirname(PWSH7)) == os.path.normcase(os.path.dirname(DEC['Pwsh'])):
        raise SystemExit('refusing: the PATH pwsh is the runtime pwsh')
    noise = noise_line()
    scripts = [('C1', 'where.exe git; git --exec-path; git --version'),
               ('C2', '& %s --no-replace-objects cat-file -t %s; & %s --no-replace-objects cat-file blob %s | & %s hash-object --no-filters --stdin'
                % (T7.pw_sq(DEC['Git']), L.B12, T7.pw_sq(DEC['Git']), L.B12, T7.pw_sq(DEC['Git']))),
               ('C3', T7.precond_script(T))]
    for name, argv in T7.order_argvs(T):
        scripts.append(('I:' + name, T7.inner_cmd(argv, DEC['Git'], DEC['Py'])))
    scripts.append(('C16', T7.primary_script('A', T)))
    execs = []
    for label, s in scripts:
        p = sh([PWSH7, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', s], cwd=T, env=env, timeout=900)
        execs.append({'Label': label, 'Script': s, 'Rc': p.returncode, 'Stdout': p.stdout, 'Stderr': p.stderr})
    cm = json.loads([x for x in execs if x['Label'] == 'I:clausemap'][0]['Stdout'].decode('ascii'))
    eff = cm['Facts']['Derive']['Eff']
    sB = T7.primary_script('B', T).replace('<Eff>', eff)
    p = sh([PWSH7, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', sB], cwd=T, env=env, timeout=900)
    execs.append({'Label': 'C17', 'Script': sB, 'Rc': p.returncode, 'Stdout': p.stdout, 'Stderr': p.stderr})
    tid = str(uuid.uuid4())
    ev = [{'type': 'thread.started', 'thread_id': tid}, {'type': 'turn.started'},
          {'type': 'item.completed', 'item': {'id': 'item_0', 'type': 'agent_message', 'text': 'Controller sintetico del autotest m7 (sin modelo).'}}]
    n = 1
    for x in execs:
        cmd = R2.render_command([DEC['Pwsh'], '-Command', x['Script']])
        agg = noise + x['Stderr'].decode('utf-8', 'replace') + x['Stdout'].decode('utf-8', 'replace')
        iid = 'item_%d' % n
        n += 1
        ev.append({'type': 'item.started', 'item': {'id': iid, 'type': 'command_execution', 'command': cmd, 'aggregated_output': '', 'exit_code': None,
                                                     'status': 'in_progress'}})
        ev.append({'type': 'item.completed', 'item': {'id': iid, 'type': 'command_execution', 'command': cmd, 'aggregated_output': agg,
                                                       'exit_code': x['Rc'], 'status': 'completed' if x['Rc'] == 0 else 'failed'}})
    ev.append({'type': 'item.completed', 'item': {'id': 'item_%d' % n, 'type': 'agent_message', 'text': '{}'}})
    ev.append({'type': 'turn.completed', 'usage': {'input_tokens': 0, 'cached_input_tokens': 0, 'output_tokens': 0, 'reasoning_output_tokens': 0}})
    data = ''.join(json.dumps(e, ensure_ascii=False, separators=(',', ':')) + '\n' for e in ev).encode('utf-8')
    return data, execs, tid, eff


def perfect_output(execs, T, eff):
    """What a faithful Controller would answer: every value read from the envelopes and the recorded outputs (never the oracle)."""
    env = dict((x['Label'][2:], json.loads(x['Stdout'].decode('ascii'))) for x in execs if x['Label'].startswith('I:'))
    F = lambda n: env[n]['Facts']
    lines = lambda x: [l.strip() for l in x['Stdout'].decode('utf-8', 'replace').replace('\r\n', '\n').split('\n') if l.strip()]
    by = dict((x['Label'], x) for x in execs)
    c = lambda label: 'C%d' % ([x['Label'] for x in execs].index(label) + 1)
    g = lines(by['C1'])
    pre = lines(by['C3'])
    segA = T7.segments(by['C16']['Stdout'].decode('utf-8', 'replace'))
    segB = T7.segments(by['C17']['Stdout'].decode('utf-8', 'replace'))
    prim = []
    for pid, blk, order, _ in T7.PRIMARY:
        seg = (segA if blk == 'A' else segB).get(pid, [])
        prim.append({'Id': pid, 'Invocation': 'C16' if blk == 'A' else 'C17', 'GitCommand': T7.primary_cmd(pid, eff, T), 'OutputLines': [s.strip() for s in seg],
                     'InstrumentValue': 'facts of ' + order, 'Equal': True})
    idf = F('identity')
    cm = F('clausemap')
    mapb = segB.get('P23', [None])[0]
    ar = [{'Path': ci['Path'], 'Section': ci['Section'], 'Class': ci['Class'], 'Mode': 'NORMAL',
           'RevisionSha': cm['Resolve']['Ar'] if ci['Class'] in ('UNIT_DOC', 'UNIT_CHANGE') else cm['Resolve']['ContractMainSha'], 'Parts': [],
           'EffectiveSha': cm['Derive']['Eff'], 'ClauseMapBlob': mapb, 'UnitProtocol': 'rackcad-protocol/I62'} for ci in cm['Resolve']['Citations']]
    jd = lambda v: json.dumps(v, ensure_ascii=True)
    o = {'Schema': 'rackcad-a6m7-measurement/1', 'Mode': 'A6-7_MEDICION', 'RunId': L.RUN_ID, 'ShellInvocations': len(execs),
         'GitByName': {'Invocation': 'C1', 'WhereLines': [l for l in g if re.match(r'^[A-Za-z]:\\', l)],
                       'ExecPath': [l for l in g if 'libexec/git-core' in l][0], 'Version': [l for l in g if l.startswith('git version')][0]},
         'Delivery': {'Invocation': 'C2', 'ObjectType': lines(by['C2'])[0], 'RehashedId': lines(by['C2'])[1]},
         'Preconditions': {'Invocation': 'C3', 'Commits': [{'Sha': s, 'Type': t} for s, t in zip(T7.PRECOND_COMMITS, pre[:4])],
                           'Files': [{'Path': f, 'Exists': v == 'True'} for f, v in zip(T7.precond_files(T), pre[4:])]},
         'Runs': [{'Order': n, 'Invocation': c('I:' + n), 'ExitCode': by['I:' + n]['Rc'], 'FactsSha256': env[n]['FactsSha256'],
                   'ErrorCode': None} for n in L.ORDERS],
         'Primary': prim,
         'Identity': {'Head': idf['Head'], 'AbbrevRef': idf['AbbrevRef'], 'Toplevel': idf['Toplevel'], 'OriginBranch': idf['OriginBranch'],
                      'OriginMain': idf['OriginMain'], 'BaseIsAncestorOfCurrent': idf['Ancestry']['base->current'],
                      'BaseIsAncestorOfRed': idf['Ancestry']['base->red'], 'RedIsAncestorOfCurrent': idf['Ancestry']['red->current']},
         'Diff': {'Paths': F('diff')['Paths'], 'Commits': F('diff')['Commits']},
         'Scope': {'PerPath': [{'Path': x['Path'], 'AllowedCoveredBy': x['AllowedCoveredBy'], 'ForbiddenCoveredBy': x['ForbiddenCoveredBy']}
                               for x in F('scope')['PerPath']]},
         'Trailer': {'PerCommit': [{'Commit': cc, 'CoAuthoredBy': F('diff')['CoAuthoredBy'][cc]} for cc in F('diff')['Commits']]},
         'CleanTree': {'StatusPorcelain': F('cleantree')['StatusPorcelain'], 'StatusIgnored': F('cleantree')['StatusIgnored'],
                       'CheckIgnoreRc': F('cleantree')['CheckIgnoreRc']},
         'Hashes': {'PerPath': F('hashes')['PerPath']},
         'Reads': {'ContractPointersJson': dict((p[1:], jd(F('read-json')['Values'][p])) for p in L.JSON_POINTERS),
                   'DecisionsFirstLineJson': jd(F('read-text')['Lines'][0]['Text']), 'DecisionsFirstLineNonAsciiCount': F('read-text')['NonAsciiCount'],
                   'HandoffFieldsJson': jd(F('handoff')['Fields']), 'TrxSha256': F('trx')['Sha256'], 'TrxCountersJson': jd(F('trx')['Counters']),
                   'TrxFilterSelected': F('trx')['FilterSelected'], 'FreeTextTermsJson': jd(F('freetext')['Terms']),
                   'FreeTextMatchCount': len(F('freetext')['Matches']), 'ContractA5MissingJson': jd(F('contract')['A5Missing']),
                   'ContractSupersededEqual': F('contract')['SupersededEqual']},
         'ClauseMap': {'Eff': cm['Derive']['Eff'], 'EffParent1': cm['Derive']['EffParent1'], 'MapBlob': mapb},
         'AuthorityResolution': ar, 'Findings': []}
    return o


def session_record(root, tid, model='gpt-6-luna', effort='high'):
    p = os.path.join(root, '2026', '10', '17', 'rollout-2026-10-17T00-00-00-%s.jsonl' % tid)
    lines = [{'type': 'session_meta', 'payload': {'id': tid, 'model': ''}},
             {'type': 'turn_context', 'payload': {'model': model, 'effort': effort, 'cwd': 'fixture'}}]
    L.wbytes(p, ''.join(json.dumps(x) + '\n' for x in lines).encode('ascii'))
    return p


def identity_rec(cfg, tag, out):
    return G7.measure(cfg, tag, out)


def fixture_config(fixdir, setv, expect, mode='dry-run'):
    L.wbytes(fixdir + '/fixture.json', json.dumps({'Set': setv, 'Expect': expect}, indent=1).encode('ascii'))
    return G7.Config.fixture(fixdir, mode)


# ------------------------------------------------------------------ main flow
def main(argv):
    keep = '--keep' in argv
    t0 = time.time()
    L.rmtree(ST)
    os.makedirs(ST)
    group_static()
    group_recon()
    PC = _load('prep_clone')
    # scratch copies of the -C (same procedure as m7-run)
    c1, c2 = ST + '/c1', ST + '/c2'
    for c in (c1, c2):
        r = sh([sys.executable, '-I', L.KIT + '/prep_clone.py', '--dest', c, '--snapshot-out', c + '.snapshot.json'])
        if r.returncode != 0:
            raise SystemExit('prep_clone scratch failed: ' + r.stderr.decode('utf-8', 'replace')[:500])
    s_real = L.rjson(L.FROZEN + '/m7-run.snapshot.json')
    s_c1 = L.rjson(c1 + '.snapshot.json')
    keys_same = all(s_real[k] == s_c1[k] for k in ('Refs', 'Head', 'HeadSymref', 'StatusZ', 'WorktreeDirs')) and s_real['WorktreeFiles'] == s_c1['WorktreeFiles']
    case('P', 'CLONE-DETERMINISTIC', 'a scratch copy made by prep_clone.py has the refs, HEAD, status, file list and file contents of m7-run',
         {'SameRefsHeadStatusFiles': keys_same}, keys_same)
    m7_before = L.snapshot(L.CDIR, exe=DEC['Git'])
    rr = sh([sys.executable, '-I', L.KIT + '/prep_clone.py'])
    m7_after = L.snapshot(L.CDIR, exe=DEC['Git'])
    case('P', 'CLONE-REFUSES-EXISTING', 'prep_clone.py with its defaults refuses because m7-run exists (nothing written); m7-run equal to its frozen '
         'snapshot (no instrument object yet)', {'Rc': rr.returncode, 'M7RunUnchanged': L.snapdiff(m7_before, m7_after) == {},
                                                 'M7RunEqualsFrozen': L.snapdiff(s_real, m7_after) == {}},
         rr.returncode == 2 and L.snapdiff(m7_before, m7_after) == {} and L.snapdiff(s_real, m7_after) == {})
    dec_ok = fixture_decisions(ST + '/decisions-ok.md')
    # ---- P. prep_write negatives on c2 (each refuses before writing), then positive on c1 and c2
    gitx = DEC['Git']

    def plant_include():
        shutil.copyfile(c2 + '/.git/config', ST + '/c2-config.bak')
        with open(c2 + '/.git/config', 'ab') as fh:
            fh.write(b'[include]\n\tpath = x.cfg\n')

    def pw(repo, outd, decisions=dec_ok, snap=None):
        return sh([sys.executable, '-I', L.KIT + '/prep_write.py', '--repo', repo, '--decisions', decisions, '--out-dir', outd,
                   '--snapshot', snap or (repo + '.snapshot.json')])
    plants = {
        'replace-ref': (lambda: L.git(c2, 'update-ref', 'refs/replace/' + L.TIP, L.RED), lambda: L.git(c2, 'update-ref', '-d', 'refs/replace/' + L.TIP)),
        'alternates': (lambda: L.wbytes(c2 + '/.git/objects/info/alternates', (c1 + '/.git/objects\n').encode()),
                       lambda: os.remove(c2 + '/.git/objects/info/alternates')),
        'include-config': (plant_include, lambda: shutil.copyfile(ST + '/c2-config.bak', c2 + '/.git/config')),
        'no-disposition': (lambda: None, lambda: None),
        'drift-file': (lambda: L.wbytes(c2 + '/stray.txt', b'x'), lambda: os.remove(c2 + '/stray.txt')),
    }
    for name, (plant, unplant) in plants.items():
        before = L.snapshot(c2, exe=gitx)
        plant()
        dd = dec_ok if name != 'no-disposition' else fixture_decisions(ST + '/decisions-noesc.md', drop=('M7-ESCRITURA',))
        r = pw(c2, ST + '/out-c2-' + name, decisions=dd)
        objabs = L.git(c2, 'cat-file', '-e', L.B, exe=gitx).returncode != 0
        unplant()
        after = L.snapshot(c2, exe=gitx)
        case('P', 'PREP-WRITE-REFUSE-' + name, 'prep_write refuses before writing (rc 2), no loose object, the -C unchanged after removing the plant',
             {'Rc': r.returncode, 'ObjectAbsent': objabs, 'Unchanged': L.snapdiff(before, after) == {}},
             r.returncode == 2 and objabs and L.snapdiff(before, after) == {})
    out1 = ST + '/out1'
    r = pw(c1, out1)
    rec = L.rjson(out1 + '/prep-write.json') if os.path.isfile(out1 + '/prep-write.json') else {}
    case('P', 'PREP-WRITE-HELD', 'prep_write on a scratch copy: id = pinned blob, snapshots differ in exactly one new loose object, DecisionRef from the '
         'disposition, registered config written', {'Rc': r.returncode, 'Held': rec.get('Held'), 'DecisionRef': rec.get('DecisionRef'),
                                                    'Diff': sorted(rec.get('SnapshotDiff', {}))},
         r.returncode == 0 and rec.get('Held') is True and rec.get('DecisionRef') == L.DECISION_REF and rec.get('OnlyNewLooseObject') is True
         and os.path.isfile(out1 + '/registered-config.json'))
    r2 = pw(c1, out1)
    case('P', 'PREP-WRITE-ONCE', 'a second prep_write refuses (the write is done once)', {'Rc': r2.returncode}, r2.returncode == 2)
    out2 = ST + '/out-c2'
    pw(c2, out2)
    # ---- U. point of use on c2 (pou_m7.py; the PATH pwsh 7 stands for the runtime pwsh)
    env = launch_env()
    L.wbytes(out2 + '/launch-env.json', json.dumps(env).encode('utf-8'))
    decl = {'Blob': L.B, 'Git': {'Path': DEC['Git'], 'Version': P['Git']['Version'], 'Sha256': P['Git']['Sha256'], 'InvocationName': 'git'},
            'Interpreter': {'Path': DEC['Py'], 'Version': P['Interpreter']['Version'], 'Sha256': P['Interpreter']['Sha256'], 'InvocationName': 'python'},
            'Shell': {'Path': PWSH7}}
    L.wbytes(out2 + '/decl.json', json.dumps(decl).encode('ascii'))

    def pou(envp=out2 + '/launch-env.json', declp=out2 + '/decl.json', outp=out2 + '/pou.json'):
        r = sh([sys.executable, '-I', L.KIT + '/pou_m7.py', '--repo', c2, '--decl', declp, '--registered-config', out2 + '/registered-config.json',
                '--launch-env-json', envp, '--out', outp], timeout=1800)
        res = L.rjson(outp) if os.path.isfile(outp) else {}
        return r.returncode, res
    rc_, res = pou()
    case('U', 'POU-PASS', 'point of use (i)-(viii) on the prepared scratch copy: Launch true; git and python by name resolve to the declared paths; '
         'shell language mode recorded', {'Rc': rc_, 'Failed': res.get('FailedSteps'), 'LanguageMode': res.get('ShellLanguageMode')},
         rc_ == 0 and res.get('Launch') is True)
    upl = {
        'i-replace-ref': ('i', lambda: L.git(c2, 'update-ref', 'refs/replace/' + L.TIP, L.RED), lambda: L.git(c2, 'update-ref', '-d', 'refs/replace/' + L.TIP), None, None),
        'vi-GIT_DIR': ('vi', None, None, dict(env, GIT_DIR=c1 + '/.git'), None),
        'viii-git-exe': ('viii', lambda: L.wbytes(c2 + '/git.exe', b''), lambda: os.remove(c2 + '/git.exe'), None, None),
        'v-sha': ('v', None, None, None, dict(decl, Interpreter=dict(decl['Interpreter'], Sha256='0' * 64))),
        'vii-include': ('vii', plants['include-config'][0], plants['include-config'][1], None, None),
        'iii-no-shell': ('iii', None, None, None, dict(decl, Shell={'Path': ST + '/absent/pwsh.exe'})),
    }
    for name, (step, plant, unplant, envx, declx) in upl.items():
        if plant:
            plant()
        ep, dp = out2 + '/launch-env.json', out2 + '/decl.json'
        if envx:
            ep = out2 + '/env-x.json'
            L.wbytes(ep, json.dumps(envx).encode('utf-8'))
        if declx:
            dp = out2 + '/decl-x.json'
            L.wbytes(dp, json.dumps(declx).encode('ascii'))
        rc_, res = pou(ep, dp, out2 + '/pou-x.json')
        if unplant:
            unplant()
        case('U', 'POU-REFUSE-' + name, 'point of use fails closed in step (%s): Launch false, exit 1' % step,
             {'Rc': rc_, 'Failed': res.get('FailedSteps')}, rc_ == 1 and res.get('Launch') is False and step in (res.get('FailedSteps') or []))
    # ---- S. synthetic session on c1 (12 orders really run through the declared form) -> perfect output -> compare + real acceptor
    events, execs, tid, eff = run_session(c1, env)
    out = perfect_output(execs, c1, eff)
    orc = T7.oracle(c1)
    L.wbytes(ST + '/oracle-c1.json', (json.dumps(L.sanitize(orc), ensure_ascii=True, indent=1, sort_keys=True) + '\n').encode('ascii'))
    sroot = ST + '/sessions'
    session_record(sroot, tid)
    rcn = R2.reconstruct(events, L.B, env_dir=ST + '/env-s')
    case('S', 'SYNTH-RECON', 'the synthetic Codex-format log of the 17 executions reconstructs with v2: 12 instrument executions, rendering R1, '
         'declared git and interpreter', {'Reconstructible': rcn['Reconstructible'], 'Executions': rcn['InstrumentExecutions'], 'Problems': rcn['Problems'][:2]},
         rcn['Reconstructible'] and rcn['InstrumentExecutions'] == 12 and all(r['Rendering'] == 'R1' for r in rcn['Runs']))
    # request + real acceptor on c1 + table
    runs = [{'Name': 'r%02d-%s' % (r['Seq'], r['Argv'][0]), 'Envelope': r['EnvelopePath'], 'Argv': r['Argv']} for r in rcn['Runs']]
    consts = {'Repo': c1, 'Tool': {'PinnedBlob': L.B, 'RunFrom': 'object', 'GitObject': {'GitDir': c1, 'Blob': L.B}},
              'Interpreter': {'Python': P['Interpreter']['PythonVersion'], 'ExecutableSha256': P['Interpreter']['Sha256']},
              'Closure': {'Path': L.FROZEN + '/closure.json', 'Sha256': L.sha256_file(L.FROZEN + '/closure.json')}}
    req, cond = TM.construct_m(runs, consts)
    reqp = ST + '/request-s.json'
    with open(reqp, 'w', encoding='utf-8', newline='\n') as fh:
        fh.write(json.dumps(req, ensure_ascii=False, indent=1) + '\n')
    cfgS = G7.Config()
    cfgS.repo = c1
    cfgS.check_processes = True
    acc = T7.run_acceptor(cfgS, reqp, env)
    tb = TM.evaluate_m(reqp, acc['Rc'], acc['Stdout'], L.FROZEN + '/vector.json', L.FROZEN + '/derivation.json', L.FROZEN + '/designation-M.json',
                       executions=rcn['InstrumentExecutions'], reconstructed=[{'Argv': r['Argv'], 'EnvelopeSha256': r['EnvelopeSha256']} for r in rcn['Runs']])
    acc.pop('Stdout')
    case('S', 'SYNTH-ACCEPTOR', 'fixed acceptor (bytes by blob), declared interpreter, same -C: ACCEPT_FACTS rc 0, R1 R2 R3 R9 MATCH, R4 R10 '
         'NOT_APPLICABLE, R5-R7 MATCH, R8 MANUAL; zero writes; process tree ended; table of M accepted (construction, rows, closure cotejo, designation)',
         {'Rc': acc['Rc'], 'Decision': acc['Decision'], 'Statuses': acc['Statuses'], 'Provenance': dict((k, acc['Provenance'].get(k)) for k in
                                                                                                         ('ZeroWrite', 'ProcessTreeEnded', 'Ok')),
          'TableAccepted': tb['Accepted'], 'Violations': (tb['ConstructionViolations'] + tb['AcceptanceReasons'] + tb['ClosureViolations']
                                                          + tb['DesignationViolations'])[:4]},
         acc['Rc'] == 0 and acc['Decision'] == 'ACCEPT_FACTS' and tb['Accepted'] and acc['Provenance']['Ok'])
    idfx = ST + '/idfix'
    home = make_identity_fixture(idfx)
    stubdir = ST + '/stubbin/e78bad9887fa50cf'
    stub = build_stub(stubdir)
    cfgI = G7.Config()
    cfgI.cfg_home, cfgI.appx_version, cfgI.codex_bin_root, cfgI.codex_path, cfgI.baseline = home, '9.9.9.9', ST + '/stubbin', stub, idfx + '/baseline.json'
    m0 = G7.measure(cfgI, 'probe', ST + '/idprobe')
    expect = {'BinaryHash': m0['BinaryHash'], 'AppVersion': '9.9.9.9', 'Fingerprint': m0['Fingerprint'], 'Companions': m0['Companions'],
              'RuntimePwshSha256': P['RuntimePwsh']['Sha256']}
    cfgI.expect.update(expect)
    ipre = G7.measure(cfgI, 'pre', ST + '/idpre')
    ipost = G7.measure(cfgI, 'post', ST + '/idpost')
    # Entry of the synthetic session: state before/after the 17 executions
    snap = L.snapshot(c1, exe=gitx)
    ctx = {'Output': (json.dumps(out, ensure_ascii=True) + '\n').encode('ascii'), 'Events': events, 'Reconstruction': rcn, 'Table': tb, 'Acceptor': acc,
           'IdentityPre': ipre, 'IdentityPost': ipost, 'Session': T7.session_extract(sroot, tid),
           'Entry': {'StateOk': True, 'StateDetail': None, 'DeclaredOk': True}, 'Launch': {'Exit': 0, 'TimedOut': False, 'DeathConfirmed': True},
           'Gates': {'StaticGit': G7.check_static_git(cfgS)}, 'Pou': {}, 'Oracle': json.loads(L.rbytes(ST + '/oracle-c1.json').decode('ascii')),
           'RuleSha256': L.sha256_file(L.KIT + '/reconstruct_runs_m7.py'), 'RuleNamed': L.sha256_file(L.KIT + '/reconstruct_runs_m7.py'), 'Repo': c1}
    cp = T7.compare(ctx)
    case('S', 'COMPARE-POSITIVE', 'perfect synthetic Controller (values read from the real envelopes and outputs) against the oracle computed from Git '
         'primitives: every operation DEMONSTRATED, every record item OK -> ALL_OPERATIONS_DEMONSTRATED (shows oracle = instrument facts here)',
         {'Verdict': cp['Verdict'], 'Causes': cp['Causes'], 'Reading': cp['Operations']['OP7_mapa_de_clausulas_AuthorityResolution'].get('MatchedReading'),
          'FailingChecks': [c for o in cp['Operations'].values() for c in o['Causes']][:6]},
         cp['Verdict'] == 'ALL_OPERATIONS_DEMONSTRATED')
    # ---- N. comparator negatives (each one alteration of the synthetic data)
    def neg(name, mut, want):
        c2x = copy.deepcopy(ctx)
        o2 = copy.deepcopy(out)
        r = mut(c2x, o2)
        if r != 'raw':
            c2x['Output'] = (json.dumps(o2, ensure_ascii=True) + '\n').encode('ascii')
        res = T7.compare(c2x)
        case('N', 'COMPARE-' + name, 'NOT_DEMONSTRATED with cause %s' % want, {'Verdict': res['Verdict'], 'Causes': res['Causes']},
             res['Verdict'] == 'NOT_DEMONSTRATED' and want in res['Causes'])

    def ev_mut(fn):
        def m(c, o):
            evs = [json.loads(x) for x in c['Events'].decode('utf-8').split('\n') if x.strip()]
            fn(evs)
            done = dict((e['item']['id'], e['item'].get('command')) for e in evs if e.get('type') == 'item.completed' and isinstance(e.get('item'), dict))
            for e in evs:
                if e.get('type') == 'item.started' and e['item']['id'] in done:
                    e['item']['command'] = done[e['item']['id']]
            c['Events'] = ''.join(json.dumps(e, ensure_ascii=False, separators=(',', ':')) + '\n' for e in evs).encode('utf-8')
            c['Reconstruction'] = R2.reconstruct(c['Events'], L.B, env_dir=ST + '/env-n')
        return m

    def cmd_items(evs):
        return [e['item'] for e in evs if e.get('type') == 'item.completed' and e['item'].get('type') == 'command_execution']
    neg('diff-path', lambda c, o: o['Diff']['Paths'].append('x.txt'), 'OP3_diff_y_pertenencia_Scope')
    neg('scope-member', lambda c, o: o['Scope']['PerPath'][0]['AllowedCoveredBy'].clear(), 'OP3_diff_y_pertenencia_Scope')
    neg('identity-head', lambda c, o: o['Identity'].__setitem__('Head', L.TIP), 'OP4_lecturas_Identity_Remote_CleanTree_Trailer')
    neg('trailer', lambda c, o: o['Trailer']['PerCommit'][0]['CoAuthoredBy'].append('Otro <x@y.z>'), 'OP4_lecturas_Identity_Remote_CleanTree_Trailer')
    neg('hash', lambda c, o: o['Hashes']['PerPath'][0].__setitem__('WorktreeSha256', '0' * 64), 'OP5_blobs_y_hashes')
    neg('json-not-ascii', lambda c, o: o['Reads']['ContractPointersJson'].__setitem__('Objective', json.dumps(json.loads(o['Reads']['ContractPointersJson']['Objective']), ensure_ascii=False)),
        'OP6_lectura_fiel_JSON_y_texto')
    neg('text-altered', lambda c, o: o['Reads'].__setitem__('DecisionsFirstLineJson', json.dumps('otra linea')), 'OP6_lectura_fiel_JSON_y_texto')
    neg('ar-blob', lambda c, o: o['AuthorityResolution'][0].__setitem__('ClauseMapBlob', '0' * 40), 'OP7_mapa_de_clausulas_AuthorityResolution')
    neg('ar-reading', lambda c, o: o['AuthorityResolution'][0].__setitem__('Mode', 'COMPAT'), 'OP7_mapa_de_clausulas_AuthorityResolution')
    neg('primary-missing', lambda c, o: o.__setitem__('Primary', o['Primary'][:-1]), 'OP8_contraste_con_orden_Git_primaria')
    neg('primary-equal-false', lambda c, o: o['Primary'][3].__setitem__('Equal', False), 'OP8_contraste_con_orden_Git_primaria')
    neg('field-omitted', lambda c, o: o.pop('Findings'), 'OP9_salida_estructurada_completa')
    neg('bad-cref', lambda c, o: o['Runs'][0].__setitem__('Invocation', 'C99'), 'OP9_salida_estructurada_completa')
    neg('no-output', lambda c, o: c.__setitem__('Output', None) or 'raw', 'OP9_salida_estructurada_completa')
    neg('delivery-wrong', lambda c, o: o['Delivery'].__setitem__('RehashedId', '0' * 40), 'OP1_entrega_exacta_bytes')
    neg('where-other-git', ev_mut(lambda evs: cmd_items(evs)[0].__setitem__('aggregated_output', cmd_items(evs)[0]['aggregated_output'].replace(
        DEC['Git'], DEC['Git'].replace('\\cmd\\git.exe', '\\mingw64\\bin\\git.exe'), 1))), 'RA_git_por_nombre_es_el_declarado')
    neg('blob-in-other-command', ev_mut(lambda evs: cmd_items(evs)[0].__setitem__('command', cmd_items(evs)[0]['command'].replace('git --version', 'git cat-file -t ' + L.B))),
        'RB_reconstruccion_Runs_regla_nombrada')
    neg('crlf-envelope', ev_mut(lambda evs: cmd_items(evs)[3].__setitem__('aggregated_output', cmd_items(evs)[3]['aggregated_output'].replace('}\n', '}\r\n'))),
        'RB_reconstruccion_Runs_regla_nombrada')
    neg('acceptor-reject', lambda c, o: c['Table'].__setitem__('Accepted', False), 'RC_aceptante_ACCEPT_FACTS_tabla')
    neg('identity-after', lambda c, o: c['IdentityPost'].__setitem__('EqualsTrio', False), 'RD_identidad_antes_y_despues')
    neg('effort', lambda c, o: c.__setitem__('Session', T7.session_extract(sroot + '-x', tid) if session_record(sroot + '-x', tid, effort='medium') else None),
        'RE_modelo_effort_efectivos')
    neg('p22-read', ev_mut(lambda evs: cmd_items(evs)[0].__setitem__('command', cmd_items(evs)[0]['command'].replace('git --version', 'git --version; Get-Content '
        + W.replace('\\', '/') + '/AGENTS.md'))), 'RF_sin_lectura_P22')
    neg('clone-write', lambda c, o: c.__setitem__('Entry', {'StateOk': False, 'DeclaredOk': True}), 'RG_sin_escritura_en_el_clon')
    neg('timeout', lambda c, o: c.__setitem__('Launch', {'Exit': 124, 'TimedOut': True, 'DeathConfirmed': True}), 'RH_lanzamiento_terminacion_entrada')
    neg('usage-limit', ev_mut(lambda evs: evs.insert(2, {'type': 'item.completed', 'item': {'id': 'item_x', 'type': 'agent_message', 'text': 'usage limit reached'}})),
        'RH_lanzamiento_terminacion_entrada')
    # ---- G. gates (fixture configurations; each refusal by the specific gate)
    fixdir = ST + '/fix'
    os.makedirs(fixdir, exist_ok=True)
    L.wbytes(stubdir + '/events.jsonl', events)
    L.wbytes(stubdir + '/output.json', (json.dumps(out, ensure_ascii=True, indent=1) + '\n').encode('ascii'))
    clp = ST + '/claude-projects'
    os.makedirs(clp, exist_ok=True)
    base_set = {'decisions': dec_ok, 'codex_bin_root': ST + '/stubbin', 'codex_path': stub, 'cfg_home': home, 'appx_version': '9.9.9.9',
                'baseline': idfx + '/baseline.json', 'claude_projects': clp, 'repo': c1, 'out': out1, 'frozen_snapshot': c1 + '.snapshot.json',
                'marker': ST + '/M7_LAUNCHED.fixture', 'sessions_root': sroot, 'pou_shell': PWSH7, 'codex_proc_name': 'm7stubcodex.exe',
                'oracle_path': ST + '/oracle-c1.json'}
    os.makedirs(out1 + '/raw', exist_ok=True)
    L.wbytes(out1 + '/raw/launch-env.json', json.dumps(env).encode('utf-8'))
    cfg = fixture_config(fixdir, base_set, expect)
    gr = G7.gates(cfg)
    case('G', 'GATES-PASS', 'every gate passes on the fixture (temporary decisions file with the disposition and both Owner lines, identity fixtures, '
         'scratch -C with the instrument object, launch environment, processes, armed prompt)', {'Refused': gr['Refused']}, gr['Pass'])
    os.remove(out1 + '/pre-launch-identity.json')

    def dec_case(name, **kw):
        p = fixture_decisions(ST + '/dec-%s.md' % name, **kw)
        c = copy.copy(cfg)
        c.decisions = p
        r = G7.check_decisions(c)
        case('G', 'DECISIONS-' + name, 'decisions gate refuses', r['Problems'][:2], not r['Ok'])
    for m in G7.MARKER_KEYS:
        dec_case('missing-' + m, drop=(m,))
    dec_case('altered-trio', alter=lambda m: m.replace(P['Trio']['AppVersion'], '26.1013.3656.1'))
    dec_case('altered-interpreter-sha', alter=lambda m: m.replace(P['Interpreter']['Sha256'], P['Interpreter']['Sha256'][:-1] + '0'))
    dec_case('altered-closure-sha', alter=lambda m: m.replace(L.sha256_file(L.FROZEN + '/E.json'), '0' * 64))
    dec_case('no-lecturas', lect=None)
    dec_case('lecturas-section-absent', lect='§97')
    dec_case('lecturas-section-without-A6-5', lect_section=False)
    dec_case('od2-missing', owner=('line-b.txt',))
    dec_case('b-missing', owner=('line-od2.txt',))
    dec_case('od2-altered', owner_mut=lambda n, t: t.replace('6EFA5B0E', '6EFA5B0F') if n == 'line-od2.txt' else t)
    dec_case('b-not-complete-line', owner_mut=lambda n, t: ('- ' + t) if n == 'line-b.txt' else t)
    dec_case('owner-before-disposition', owner_before=True)
    dec_case('no-section-90', no90=True)
    dec_case('two-dispositions', two=True)

    def idcase(name, mutate, undo):
        mutate()
        r = G7.measure(cfg, 'neg', ST + '/idneg')
        undo()
        case('G', 'IDENTITY-' + name, 'identity gate refuses (identity != trio)', r['Differs'], not r['EqualsTrio'])
    cfgtxt = L.rbytes(home + '/.codex/config.toml')
    idcase('fingerprint', lambda: L.wbytes(home + '/.codex/config.toml', cfgtxt.replace(b'1', b'2')), lambda: L.wbytes(home + '/.codex/config.toml', cfgtxt))
    idcase('keynames', lambda: L.wbytes(home + '/.codex/config.toml', cfgtxt + b'y = 2\n'), lambda: L.wbytes(home + '/.codex/config.toml', cfgtxt))
    idcase('bins-2', lambda: L.wbytes(ST + '/stubbin/other/codex.exe', b'x'), lambda: shutil.rmtree(ST + '/stubbin/other'))
    idcase('companions', lambda: L.wbytes(stubdir + '/codex-extra.exe', b'x'), lambda: os.remove(stubdir + '/codex-extra.exe'))
    orig_app = cfg.appx_version
    idcase('appversion', lambda: setattr(cfg, 'appx_version', '9.9.9.8'), lambda: setattr(cfg, 'appx_version', orig_app))
    orig_bin = cfg.expect['BinaryHash']
    idcase('binaryhash', lambda: cfg.expect.__setitem__('BinaryHash', '0' * 64), lambda: cfg.expect.__setitem__('BinaryHash', orig_bin))
    orig_rt = cfg.expect['RuntimePwshSha256']
    idcase('runtime-pwsh', lambda: cfg.expect.__setitem__('RuntimePwshSha256', '0' * 64), lambda: cfg.expect.__setitem__('RuntimePwshSha256', orig_rt))
    # P-07
    L.wbytes(cfg.marker, b'x')
    r = G7.check_p07(cfg)
    os.remove(cfg.marker)
    case('G', 'P07-MARKER', 'P-07 refuses with the launch marker present', r['Problems'], not r['Ok'])
    L.wbytes(out1 + '/raw/events.jsonl', b'')
    r = G7.check_p07(cfg)
    os.remove(out1 + '/raw/events.jsonl')
    case('G', 'P07-PREVIOUS-EVENTS', 'P-07 refuses with events of a previous launch', r['Problems'], not r['Ok'])
    # state drift
    for name, plant, unplant in (('extra-file', lambda: L.wbytes(c1 + '/stray.txt', b'x'), lambda: os.remove(c1 + '/stray.txt')),
                                 ('extra-ref', lambda: L.git(c1, 'update-ref', 'refs/heads/extra', L.TIP), lambda: L.git(c1, 'update-ref', '-d', 'refs/heads/extra')),
                                 ('ignored-file', lambda: L.wbytes(c1 + '/' + L.IGNORE_PATH, b'{}'), lambda: os.remove(c1 + '/' + L.IGNORE_PATH))):
        plant()
        r, _ = G7.check_state(cfg)
        unplant()
        case('G', 'STATE-' + name, 'state gate refuses (the -C is not the frozen snapshot plus exactly the loose object)', r['Problems'][:1], not r['Ok'])
    c3 = copy.copy(cfg)
    c3.repo, c3.frozen_snapshot = c2, c2 + '.snapshot.json'
    c3.out = ST + '/out-none'
    r, _ = G7.check_state(c3)
    case('G', 'STATE-no-prep-write', 'state gate refuses without a held preparation write record', r['Problems'][:2], not r['Ok'])
    cnoobj = copy.copy(cfg)
    cnoobj.frozen_snapshot = c1 + '.snapshot.json'
    obj = c1 + '/.git/' + L.loose_rel(L.B)
    shutil.move(obj, ST + '/obj.bak')
    r, _ = G7.check_state(cnoobj)
    shutil.move(ST + '/obj.bak', obj)
    case('G', 'STATE-no-object', 'state gate refuses without the instrument loose object', r['Problems'][:1], not r['Ok'])
    # launch environment
    for name, envx in (('GIT_var', dict(env, GIT_EDITOR='true')), ('path-order', dict(env, PATH=env['PATH'].split(';', 1)[1]))):
        L.wbytes(ST + '/env-%s.json' % name, json.dumps(envx).encode('utf-8'))
        r, _ = G7.check_launch_env(cfg, ST + '/env-%s.json' % name)
        case('G', 'LAUNCHENV-' + name, 'launch-environment gate refuses (GIT_* present / runtime pwsh not first in PATH)', r['Problems'], not r['Ok'])
    # static git scan
    L.wbytes(c1 + '/git.cmd', b'')
    r = G7.check_static_git(cfg)
    os.remove(c1 + '/git.cmd')
    case('G', 'STATICGIT-root', 'static scan refuses a git.* in the -C root', r['Problems'][:1], not r['Ok'])
    os.makedirs(ST + '/fakesys', exist_ok=True)
    L.wbytes(ST + '/fakesys/git.exe', b'')
    cs = copy.copy(cfg)
    cs.static_dirs_extra = [ST + '/fakesys']
    r = G7.check_static_git(cs)
    case('G', 'STATICGIT-dir', 'static scan refuses a git.exe in a scanned directory', r['Problems'][:1], not r['Ok'])
    # processes: a live process with the -C in its command line
    pp = subprocess.Popen([sys.executable, '-I', '-c', 'import time; time.sleep(60)', c1.replace('/', '\\')], stdin=subprocess.DEVNULL)
    time.sleep(1)
    r = G7.check_processes(cfg)
    pp.kill()
    pp.wait()
    case('G', 'PROCESSES-on-C', 'P-02 refuses with a live process whose command line names the -C', r['Problems'], not r['Ok'])
    cpn = copy.copy(cfg)
    cpn.codex_proc_name = 'm7stubcodex.exe'
    L.wbytes(stubdir + '/sleep', b'')
    pp = subprocess.Popen([stub.replace('/', '\\')], stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, cwd=ST)
    time.sleep(2)
    r = G7.check_processes(cpn)
    subprocess.run(['taskkill', '/PID', str(pp.pid), '/T', '/F'], capture_output=True)
    pp.wait()
    os.remove(stubdir + '/sleep')
    case('G', 'PROCESSES-forbidden-name', 'P-02 refuses with a live process of the forbidden name (codex.exe at the measurement; the stub name here)',
         r['Problems'], not r['Ok'])
    # Claude project directory
    name = re.sub(r'[^A-Za-z0-9]', '-', c1.replace('/', '\\'))
    os.makedirs(os.path.join(clp, name))
    r = G7.check_claude_project(cfg)
    shutil.rmtree(os.path.join(clp, name))
    case('G', 'CLAUDE-PROJECT', 'P-02 refuses with a Claude project directory for the -C', r['Problems'], not r['Ok'])
    # kit integrity and prompt (on a copy of the kit with one byte changed)
    kc = ST + '/kitcopy'
    shutil.copytree(L.KIT, kc, ignore=shutil.ignore_patterns('.selftest', 'out', '.build-tmp'))
    t = L.rbytes(kc + '/prompt.m7.template.txt')
    L.wbytes(kc + '/prompt.m7.template.txt', t.replace(b'Responde solo', b'Responde ya'))
    ck = copy.copy(cfg)
    ck.kit = kc
    r = G7.check_kit(ck)
    case('G', 'KIT-CHANGED', 'kit gate refuses a file changed after the freeze', r['Problems'][:1], not r['Ok'])
    T7b = T7.arm_prompt(DEC['Git'], DEC['Py'], template=kc + '/prompt.m7.template.txt')
    case('G', 'PROMPT-CHANGED', 'the armed prompt of a changed template differs from the frozen SHA-256 (prompt gate refuses)',
         L.sha256(T7b.encode('utf-8'))[:16], L.sha256(T7b.encode('utf-8')) != L.rjson(L.KIT + '/FREEZE.json')['ArmedPrompt']['Sha256'])
    shutil.rmtree(kc)
    # fixture mode never touches the real decisions file nor the real binary
    bad = []
    for k, v in (('decisions', L.expand(P['DecisionsFile'])), ('codex_path', L.expand(P['Codex']['Path']))):
        try:
            fixture_config(ST + '/fixbad', dict(base_set, **{k: v}), expect)
            bad.append(k)
        except SystemExit:
            pass
    case('G', 'FIXTURE-GUARD', 'fixture mode refuses the real decisions file and the real Codex binary path', bad or 'both refused', not bad)
    # ---- D. bash runner: usage, dry-run positive and negative, stub launch end to end, watchdog
    runner = L.KIT + '/run_m7.sh'
    u1 = sh(['bash', runner])
    u2 = sh(['bash', runner, 'launch', 'extra'])
    u3 = sh(['bash', runner, '--dry-run'])
    case('D', 'USAGE', 'the runner accepts only its documented forms (exit 64 otherwise)', [u1.returncode, u2.returncode, u3.returncode],
         [u1.returncode, u2.returncode, u3.returncode] == [64, 64, 64])
    fixture_config(fixdir, base_set, expect)
    d1 = sh(['bash', runner, '--dry-run', fixdir], timeout=1800)
    so = d1.stdout.decode('utf-8', 'replace')
    case('D', 'DRYRUN-PASS', 'bash run_m7.sh --dry-run <fixture>: every gate and the point of use pass; stops before the marker and the launch',
         {'Rc': d1.returncode, 'Tail': so.strip().splitlines()[-1:] if so.strip() else None, 'Marker': os.path.exists(base_set['marker']),
          'Events': os.path.exists(out1 + '/raw/events.jsonl')},
         d1.returncode == 0 and 'DRY-RUN: stopping before' in so and not os.path.exists(base_set['marker']) and not os.path.exists(out1 + '/raw/events.jsonl'))
    fixd2 = ST + '/fix-neg'
    os.makedirs(fixd2, exist_ok=True)
    fixture_config(fixd2, dict(base_set, decisions=fixture_decisions(ST + '/dec-neg.md', owner=('line-od2.txt',))), expect)
    d2 = sh(['bash', runner, '--dry-run', fixd2], timeout=1800)
    case('D', 'DRYRUN-REFUSE', 'dry-run with the decisions fixture lacking line (b): refused (exit 2) before any launch step',
         {'Rc': d2.returncode, 'Err': d2.stderr.decode('utf-8', 'replace').strip().splitlines()[-1:]}, d2.returncode == 2 and not os.path.exists(base_set['marker']))
    d3 = sh(['bash', runner, '--selftest-launch', fixdir], timeout=1800)
    so3 = d3.stdout.decode('utf-8', 'replace')
    cmpf = out1 + '/comparison.json'
    cmpr = L.rjson(cmpf) if os.path.isfile(cmpf) else {}
    prompt_sent = L.rbytes(stubdir + '/prompt.txt') if os.path.isfile(stubdir + '/prompt.txt') else b''
    path_sent = L.rbytes(stubdir + '/path.txt').decode('utf-8', 'replace') if os.path.isfile(stubdir + '/path.txt') else ''
    args_sent = L.rbytes(stubdir + '/args.txt').decode('utf-8', 'replace').split('\n') if os.path.isfile(stubdir + '/args.txt') else []
    armed = L.rbytes(out1 + '/raw/prompt.txt') if os.path.isfile(out1 + '/raw/prompt.txt') else None
    want_args = ['exec', '-C', c1.replace('/', '\\'), '-s', 'read-only', '-m', 'gpt-6-luna', '-c', 'model_reasoning_effort="high"', '--output-schema',
                 L.KIT.replace('/', '\\') + '\\schema.m7.strict-projection.json', '-o', c1.replace('/', '\\') + '\\' + L.IGNORE_PATH.replace('/', '\\'), '--json']
    first = path_sent.split(';')[0] if path_sent else ''
    wh = sh(['where.exe', 'git'], env=dict(env, PATH=path_sent)) if path_sent else None
    case('D', 'STUB-LAUNCH-ARGV', 'stub launch through the runner: the binary receives exactly the recipe (exec -C <-C> -s read-only -m gpt-6-luna -c '
         'model_reasoning_effort="high" --output-schema <projection> -o <IgnorePath> --json <prompt>), the prompt byte-identical to the armed one, PATH '
         'with the runtime pwsh directory first, git by name = the declared git, no GIT_* in the child, P-07 marker created',
         {'ArgsEqual': args_sent[:len(want_args)] == want_args, 'PromptEqual': armed is not None and prompt_sent == armed,
          'PathFirstIsRuntimeDir': T7.same_path(first, os.path.dirname(DEC['Pwsh'])),
          'WhereFirst': L.sanitize_str(wh.stdout.decode().splitlines()[0]) if wh and wh.stdout else None,
          'Marker': os.path.exists(base_set['marker'])},
         args_sent[:len(want_args)] == want_args and armed is not None and prompt_sent == armed and T7.same_path(first, os.path.dirname(DEC['Pwsh']))
         and wh is not None and T7.same_path(wh.stdout.decode().splitlines()[0], DEC['Git']) and os.path.exists(base_set['marker']))
    lj = L.rjson(out1 + '/launch.json') if os.path.isfile(out1 + '/launch.json') else {}
    case('D', 'STUB-LAUNCH-POST', 'stub launch end to end: exit 0, death confirmed, post (identity after, Entry with the -o file only, session record '
         'fixture, reconstruction, request, real acceptor on the scratch -C, table, comparison, custody) -> ALL_OPERATIONS_DEMONSTRATED; custody '
         'manifest written; no unsanitized text in out/custody',
         {'Rc': d3.returncode, 'Launch': lj, 'Verdict': cmpr.get('Verdict'), 'Causes': cmpr.get('Causes')},
         d3.returncode == 0 and lj.get('Exit') == 0 and lj.get('DeathConfirmed') is True and cmpr.get('Verdict') == 'ALL_OPERATIONS_DEMONSTRATED'
         and os.path.isfile(out1 + '/custody/manifest.json')
         and all(L.hygienic(L.rbytes(f).decode('utf-8', 'replace')) for f in glob.glob(out1 + '/custody/*.json')))
    d4 = sh(['bash', runner, '--selftest-launch', fixdir], timeout=600)
    case('D', 'STUB-NO-RETRY', 'a second launch with the same marker is refused (P-07, tope 1, no retry)', {'Rc': d4.returncode}, d4.returncode == 2)
    # watchdog: a stub that hangs is killed (tree) at the cap and its death confirmed
    if os.path.isfile(c1 + '/' + L.IGNORE_PATH):
        os.remove(c1 + '/' + L.IGNORE_PATH)
    out3 = ST + '/out3'
    os.makedirs(out3 + '/raw', exist_ok=True)
    for f in ('prep-write.json', 'registered-config.json'):
        shutil.copyfile(out1 + '/' + f, out3 + '/' + f)
    L.wbytes(stubdir + '/sleep', b'')
    fixd3 = ST + '/fix-wd'
    os.makedirs(fixd3, exist_ok=True)
    cwd3 = fixture_config(fixd3, dict(base_set, out=out3, marker=ST + '/M7_LAUNCHED.wd', timeout_s=25), expect)
    t1 = time.time()
    d5 = sh(['bash', runner, '--selftest-launch', fixd3], timeout=900)
    lj3 = L.rjson(out3 + '/launch.json') if os.path.isfile(out3 + '/launch.json') else {}
    cm3 = L.rjson(out3 + '/comparison.json') if os.path.isfile(out3 + '/comparison.json') else {}
    alive = G7.procs([c1], 'm7stubcodex.exe')
    case('D', 'STUB-WATCHDOG', 'a hanging stub is tree-killed at the cap (fixture cap 25 s), its death confirmed, post still runs and the verdict is '
         'NOT_DEMONSTRATED', {'Launch': lj3, 'Verdict': cm3.get('Verdict'), 'Seconds': int(time.time() - t1), 'Alive': alive},
         lj3.get('TimedOut') is True and lj3.get('DeathConfirmed') is True and cm3.get('Verdict') == 'NOT_DEMONSTRATED' and not alive)
    os.remove(stubdir + '/sleep')
    # ---- report
    n_ok = sum(1 for c in CASES if c['Holds'])
    by = {}
    for c in CASES:
        by.setdefault(c['Group'], [0, 0])
        by[c['Group']][0] += 1
        by[c['Group']][1] += 1 if c['Holds'] else 0
    rep = {'Schema': 'i62-a6m7-selftest/1', 'UtcTime': L.utc(), 'Seconds': int(time.time() - t0), 'Cases': CASES, 'Total': len(CASES), 'Holding': n_ok,
           'ByGroup': dict((k, {'Total': v[0], 'Holding': v[1]}) for k, v in sorted(by.items())),
           'Environment': {'PwshOfTheSessions': L.sanitize_str(PWSH7), 'RuntimePwshExecuted': False, 'CodexExecuted': False,
                           'ConfigTomlRead': False, 'SessionsRead': False, 'M7RunWritten': False}}
    L.wjson(L.KIT + '/selftest-report.json', rep, pretty=True)
    print('SELFTEST %d/%d (%s)' % (n_ok, len(CASES), ', '.join('%s %d/%d' % (k, v['Holding'], v['Total']) for k, v in sorted(rep['ByGroup'].items()))))
    if not keep:
        L.rmtree(ST)
    return 0 if n_ok == len(CASES) else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv))
