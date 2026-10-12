# run_r1.py - evidence for candidate procedure R1 (identity of the effective Git object at execution time), I-62 A-6 round 2.
# python -I run_r1.py            (sequential; work area D:\r62-a6r2\r1r4\r1; results next to this script)
# P1 preparation write, P2 pre-launch identity check (pwsh 7, cmd.exe, Windows PowerShell 5.1 as a negative), P3 execution command
# vs a file copy of the tool, and the attacks (a)-(g) of the order plus (d2) and (h) as extras. No model is invoked; no network.
import hashlib, importlib.util, json, os, struct, subprocess, sys, zlib

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_s = importlib.util.spec_from_file_location('lib_r1r4', os.path.join(HERE, 'lib_r1r4.py'))
L = importlib.util.module_from_spec(_s)
_s.loader.exec_module(L)

R1 = L.WORK + '/r1'
DECISION = 'BANCO-r1r4 (sin disposicion: banco de pruebas de la evidencia A-6 ronda 2; no es una DecisionRef normativa)'
SUBSET = ['identity', 'diff', 'scope', 'hashes', 'cleantree']

FAKE_TOOL = b'''# FAKE TOOL (bench r1r4, attacks a/e2/d2/h): harmless; it only prints a fabricated facts envelope.
import hashlib, json, platform, sys
def canon(o):
    return json.dumps(o, ensure_ascii=True, sort_keys=True, separators=(",", ":"))
argv = sys.argv[1:]
cmd = argv[0] if argv else None
args = {}
i = 1
while i < len(argv):
    k = argv[i][2:]
    if i + 1 < len(argv) and not argv[i + 1].startswith("--"):
        args[k] = argv[i + 1]
        i += 2
    else:
        args[k] = True
        i += 1
if cmd == "identity":
    facts = {"AbbrevRef": args.get("branch"), "Ancestry": {"base->current": True, "base->red": True, "red->current": True},
             "Head": args.get("current"), "OriginBranch": args.get("current"), "OriginMain": "0" * 40, "Toplevel": args.get("repo")}
elif cmd == "diff":
    facts = {"Commits": [args.get("to")], "CoAuthoredBy": {args.get("to"): ["synthetic"]}, "Paths": ["src/Fixture.Lib/Calculator.cs"]}
else:
    facts = {"Fabricated": True}
env = {"Schema": "rackcad-bmin-facts/0-experimental", "Command": cmd, "Args": args, "Inputs": [], "GitCalls": [],
       "Facts": facts, "FactsSha256": hashlib.sha256(canon(facts).encode("ascii")).hexdigest(), "Error": None,
       "Runtime": {"Python": platform.python_version(), "Isolated": sys.flags.isolated == 1}}
sys.stdout.buffer.write(canon(env).encode("ascii") + b"\\n")
'''


def env_summary(raw):
    e = L.envelope_facts(raw)
    if not isinstance(e, dict):
        return {'Parsable': False, 'StdoutSha256': L.sha256(raw)}
    f = e.get('Facts')
    return {'Parsable': True, 'StdoutSha256': L.sha256(raw), 'Error': e.get('Error'), 'FactsSha256': e.get('FactsSha256'),
            'GitCalls': len(e.get('GitCalls') or []), 'FactsKeys': sorted(f) if isinstance(f, dict) else None,
            'Fabricated': isinstance(f, dict) and (f.get('OriginMain') == '0' * 40 or f.get('Fabricated') is True or e.get('GitCalls') == [])}


def facts_diff(a, b):
    ea, eb = L.envelope_facts(a), L.envelope_facts(b)
    if not (isinstance(ea, dict) and isinstance(eb, dict)):
        return {'Comparable': False}
    fa, fb = ea.get('Facts'), eb.get('Facts')
    keys = sorted(set(fa or {}) | set(fb or {})) if isinstance(fa, dict) or isinstance(fb, dict) else []
    changed = [k for k in keys if L.canon((fa or {}).get(k)) != L.canon((fb or {}).get(k))]
    return {'Comparable': True, 'BytesEqual': a == b, 'ErrorBefore': ea.get('Error'), 'ErrorAfter': eb.get('Error'),
            'FactsKeysChanged': changed, 'FactsShaBefore': ea.get('FactsSha256'), 'FactsShaAfter': eb.get('FactsSha256')}


def detail_brief(acc, rid, n=600):
    d = L.check_detail(acc, rid)
    s = L.canon(d) if d is not None else None
    return s[:n] if s else None


class Attack(object):
    """One attack repository: fresh --no-hardlinks clone + P1 + inputs + closure + baseline envelopes."""

    def __init__(self, name, subset=SUBSET):
        self.name = name
        self.T = R1 + '/' + name
        L.clone(self.T)
        self.p1 = L.p1(self.T, DECISION)
        self.inp = R1 + '/in-' + name
        L.rmtree(self.inp)
        self.H, self.deleg, self.handoff, self.k = L.build_inputs(self.T, self.inp)
        self.runs = dict(L.runs_for(self.T, self.inp, self.H))
        self.closure = L.build_closure(self.T, self.inp, self.H, self.k)
        self.relay = self.inp + '/relay.json'
        L.wjson_local(self.relay, {'Schema': 'synthetic-relay-facts', 'Note': 'banco r1r4, ataque ' + name + '; sin valor normativo'})
        self.subset = subset
        self.base = self.run('base', 'pwsh', plain=False)
        self.p2_before = L.p2(self.T, 'pwsh')

    def run(self, tag, shell, plain=False, env=None):
        out = {}
        for n in self.subset:
            argv = self.runs[n]
            if env is None:
                rc, o, cl, err = L.run_tool_shell(self.T, argv, shell, plain)
            else:
                cl = L.p3_cmdline(argv, plain)
                rc, o, err = L.shell_run(shell, cl, self.T, env=env)
            p = self.inp + '/env-' + tag + '/' + n + '.json'
            L.wbytes(p, o)
            out[n] = {'Exit': rc, 'Path': p, 'Bytes': o, 'CommandLine': cl}
        return out

    def accept(self, tag, envs, na):
        runs = [(n, envs[n]['Path'], self.runs[n]) for n in self.subset]
        req = L.build_request(self.inp + '/request-' + tag + '.json', self.T, runs, self.closure, self.relay, gitdir=self.T, na=na)
        return L.run_acceptor(req, snap_repos=[self.T], proc_paths=[self.T])

    def compare(self, envs):
        return dict((n, facts_diff(self.base[n]['Bytes'], envs[n]['Bytes'])) for n in self.subset)


def summary_acc(acc):
    s = L.strip_private(acc)
    s['MismatchDetails'] = dict((rid, detail_brief(acc, rid)) for rid, st in acc['Checks'].items() if st == 'MISMATCH')
    return s


def write_obj(T, data, kind='blob'):
    p = L.git(T, 'hash-object', '-t', kind, '-w', '--stdin', inp=data)
    return p.stdout.decode('ascii').strip(), p.returncode


def loose_path(T, oid):
    return os.path.join(L.abspath_dir(T, '--git-common-dir'), 'objects', oid[:2], oid[2:])


def make_pack(oid_hex, content):
    """Hand-made pack v2 + idx v2 holding ONE entry named oid_hex whose data is 'content' (deliberately inconsistent)."""
    size = len(content)
    c = (3 << 4) | (size & 0x0f)
    size >>= 4
    hdr = bytearray()
    while size:
        hdr.append(c | 0x80)
        c = size & 0x7f
        size >>= 7
    hdr.append(c)
    comp = zlib.compress(content)
    body = b'PACK' + struct.pack('>II', 2, 1) + bytes(hdr) + comp
    trailer = hashlib.sha1(body).digest()
    name = bytes.fromhex(oid_hex)
    fan = b''.join(struct.pack('>I', 1 if i >= name[0] else 0) for i in range(256))
    idx = b'\xfftOc' + struct.pack('>I', 2) + fan + name + struct.pack('>I', zlib.crc32(bytes(hdr) + comp) & 0xffffffff) + struct.pack('>I', 12) + trailer
    idx += hashlib.sha1(idx).digest()
    return body + trailer, idx, trailer.hex()


def cat_views(T):
    """What the different read paths deliver for B in T."""
    plain = L.git(T, 'cat-file', 'blob', L.B)
    nr = L.git(T, '--no-replace-objects', 'cat-file', 'blob', L.B)
    t = L.git(T, 'cat-file', '-t', L.B)
    return {'PlainCatFile': {'Exit': plain.returncode, 'BlobSha1OfBytes': L.blob_sha1(plain.stdout), 'Sha256': L.sha256(plain.stdout),
                             'IsPinned': L.blob_sha1(plain.stdout) == L.B and plain.returncode == 0},
            'NoReplaceCatFile': {'Exit': nr.returncode, 'BlobSha1OfBytes': L.blob_sha1(nr.stdout), 'Sha256': L.sha256(nr.stdout),
                                 'IsPinned': L.blob_sha1(nr.stdout) == L.B and nr.returncode == 0},
            'CatFileType': t.stdout.decode('ascii', 'replace').strip(), 'CatFileTypeExit': t.returncode}


def rewrite_check(T):
    """The round-1 A6-3 rule-1 check repeated now: hash-object -w of the pinned bytes returns the pinned id?"""
    data = L.pinned_bytes(L.B)
    p = L.git(T, 'hash-object', '-w', '--no-filters', '--stdin', inp=data)
    oid = p.stdout.decode('ascii').strip()
    return {'Command': 'git -C T hash-object -w --no-filters --stdin  (pinned bytes)', 'ReturnedId': oid, 'EqualsPinned': oid == L.B}


def main():
    L.rmtree(R1)
    L.ensure_dir(R1)
    pins = L.ensure_pins()
    vers = L.versions()
    nd = os.environ.get('NoDefaultCurrentDirectoryInExePath')

    # ================================================================ P1 / P2 / P3 on T0
    T0 = R1 + '/t0'
    L.clone(T0)
    p1 = L.p1(T0, DECISION)
    p2s = dict((sh, L.p2(T0, sh)) for sh in ('pwsh', 'cmd', 'ps51'))
    inp0 = R1 + '/in-t0'
    H0, _d, _h, k0 = L.build_inputs(T0, inp0)
    runs0 = L.runs_for(T0, inp0, H0)
    s_before = L.snapshot(T0)
    p3rows = []
    for name, argv in runs0:
        rcf, of = L.run_tool_file(argv)
        row = {'Name': name, 'Argv': argv, 'FileCopyExit': rcf, 'FileCopyStdoutSha256': L.sha256(of), 'FileCopyFactsSha256': (L.envelope_facts(of) or {}).get('FactsSha256')}
        for sh in ('pwsh', 'cmd'):
            rc, o, cl, err = L.run_tool_shell(T0, argv, sh)
            row[sh] = {'CommandLine': cl, 'Exit': rc, 'StdoutSha256': L.sha256(o), 'BytesEqualToFileCopy': o == of}
        if name in ('identity', 'diff'):
            rc, o, cl, err = L.run_tool_shell(T0, argv, 'ps51')
            row['ps51_negative'] = {'CommandLine': cl, 'Exit': rc, 'StdoutSha256': L.sha256(o), 'BytesEqualToFileCopy': o == of,
                                    'Note': 'envelope comparison cannot see the byte change made by Windows PowerShell 5.1 (see P2 step 3)'}
        p3rows.append(row)
    s_after = L.snapshot(T0)
    p3_all_equal = all(r['pwsh']['BytesEqualToFileCopy'] and r['cmd']['BytesEqualToFileCopy'] and r['FileCopyExit'] == 0 for r in p3rows)
    res_p = {'Schema': 'r1r4-bench/r1-p1-p2-p3', 'Versions': vers, 'Pins': pins, 'NoDefaultCurrentDirectoryInExePath': nd,
             'T0': T0,
             'P1': p1,
             'P2': p2s,
             'P2Expectation': {'pwsh': 'Launch true', 'cmd': 'Launch true', 'ps51': 'Launch false at step 3 (bytes changed by the 5.1 pipe)'},
             'P2Held': {'pwsh': p2s['pwsh']['Launch'] is True, 'cmd': p2s['cmd']['Launch'] is True,
                        'ps51': p2s['ps51']['Launch'] is False and p2s['ps51']['FailedSteps'] == ['3']},
             'P3': {'Rows': p3rows, 'AllPwshAndCmdEnvelopesByteEqualToFileCopy': p3_all_equal,
                    'ZeroWriteAroundAllRuns': L.snapdiff(s_before, s_after) == {}, 'SnapshotDiff': L.snapdiff(s_before, s_after)},
             'Held': {'P1': p1['Held'], 'P3': p3_all_equal and L.snapdiff(s_before, s_after) == {}}}
    print('P1', p1['Held'], 'P2', res_p['P2Held'], 'P3', res_p['Held']['P3'])
    L.write_result('r1-p1-p2-p3.json', res_p)

    attacks = {}
    fake_blob = L.blob_sha1(FAKE_TOOL)

    # ================================================================ (a) refs/replace/B -> substitute blob
    A = Attack('ta', subset=['identity', 'diff'])
    fid, _ = write_obj(A.T, FAKE_TOOL)
    rp = L.git(A.T, 'replace', L.B, fid)
    views = cat_views(A.T)
    rw = rewrite_check(A.T)
    exists = L.git(A.T, 'cat-file', '-e', L.B).returncode
    plain = A.run('plain', 'pwsh', plain=True)
    p3 = A.run('p3', 'pwsh', plain=False)
    p2a = L.p2(A.T, 'pwsh')
    acc_fab = A.accept('fabricated', plain, {'R4': 'ataque (a): sin salida del Controller', 'R6': 'sin corrida clausemap en el ataque',
                                             'R7': 'sin corrida scope/contract/clausemap en el ataque'})
    acc_p3 = A.accept('p3', p3, {'R4': 'ataque (a): sin salida del Controller', 'R6': 'sin corrida clausemap en el ataque',
                                 'R7': 'sin corrida scope/contract/clausemap en el ataque'})
    attacks['a'] = {
        'Attack': 'refs/replace/<B> pointing to a substitute blob (harmless fake tool printing fabricated facts)',
        'Repo': A.T, 'P1': A.p1, 'P2Before': A.p2_before, 'SubstituteBlob': fid, 'ReplaceExit': rp.returncode, 'GitViews': views,
        'NameLevelChecks': {'CatFileExistsExit': exists, 'RewriteCheck': rw},
        'PlainPathEnvelopes': dict((n, env_summary(v['Bytes'])) for n, v in plain.items()),
        'PlainPathCommandLine': plain['identity']['CommandLine'],
        'P3Envelopes': dict((n, env_summary(v['Bytes'])) for n, v in p3.items()),
        'P3EqualsBaseline': dict((n, p3[n]['Bytes'] == A.base[n]['Bytes']) for n in A.subset),
        'P2After': p2a, 'AcceptorOnFabricated': summary_acc(acc_fab), 'AcceptorOnP3': summary_acc(acc_p3)}
    attacks['a']['Held'] = {
        'plain git returns substitute': views['PlainCatFile']['BlobSha1OfBytes'] == fid,
        '--no-replace-objects returns original': views['NoReplaceCatFile']['IsPinned'],
        'round-1 write-time check still passes': rw['EqualsPinned'],
        'plain execution path runs the substitute': all(env_summary(v['Bytes'])['Fabricated'] for v in plain.values()),
        'P3 runs the pinned tool (byte-equal to baseline)': all(attacks['a']['P3EqualsBaseline'].values()),
        'P2 fails at (1) only': p2a['FailedSteps'] == ['1'],
        'acceptor rejects fabricated envelopes, R1 MATCH': acc_fab['Decision'] == 'REJECT_FACTS' and acc_fab['Checks'].get('R1') == 'MATCH',
        'acceptor accepts P3 envelopes': acc_p3['Decision'] == 'ACCEPT_FACTS'}
    print('a', attacks['a']['Held'])

    # ================================================================ (b1) replace ref for HEADX (the Worker's current commit) -> crafted commit
    A = Attack('tb1')
    raw = L.git(A.T, '--no-replace-objects', 'cat-file', 'commit', L.HEADX).stdout
    red_tree = L.gout(A.T, '--no-replace-objects', 'rev-parse', L.RED + '^{tree}')
    lines = raw.split(b'\n')
    assert lines[0].startswith(b'tree ')
    crafted = b'\n'.join([b'tree ' + red_tree.encode('ascii')] + lines[1:])
    cid, _ = write_obj(A.T, crafted, 'commit')
    rp = L.git(A.T, 'replace', L.HEADX, cid)
    gv = {'PlainDiffNames': L.gout(A.T, 'diff', '--name-only', L.BASE + '..' + L.HEADX).splitlines(),
          'NoReplaceDiffNames': L.gout(A.T, '--no-replace-objects', 'diff', '--name-only', L.BASE + '..' + L.HEADX).splitlines()}
    p3 = A.run('p3', 'pwsh')
    p3env = A.run('p3-envvar', 'pwsh', env=L.genv({'GIT_NO_REPLACE_OBJECTS': '1'}))
    p2a = L.p2(A.T, 'pwsh')
    acc = A.accept('p3', p3, {'R4': 'ataque (b1): sin salida del Controller', 'R6': 'sin corrida clausemap en el ataque'})
    attacks['b1'] = {'Attack': 'refs/replace/<HEADX> -> crafted commit (same parent and message, tree of RED)', 'Repo': A.T,
                     'P1': A.p1, 'P2Before': A.p2_before, 'CraftedCommit': cid, 'ReplaceExit': rp.returncode, 'GitViews': gv,
                     'ToolFactsVsBaseline': A.compare(p3),
                     'ToolFactsVsBaseline_WithGIT_NO_REPLACE_OBJECTS_inShell': A.compare(p3env),
                     'P2After': p2a, 'AcceptorOnP3': summary_acc(acc)}
    attacks['b1']['Held'] = {
        'tool facts change under the replace ref (diff, scope)': bool(attacks['b1']['ToolFactsVsBaseline']['diff']['FactsKeysChanged']),
        'GIT_NO_REPLACE_OBJECTS in the shell does not reach the tool': attacks['b1']['ToolFactsVsBaseline_WithGIT_NO_REPLACE_OBJECTS_inShell']['diff']['FactsKeysChanged']
        == attacks['b1']['ToolFactsVsBaseline']['diff']['FactsKeysChanged'] != [],
        'P2 fails at (1) only': p2a['FailedSteps'] == ['1'],
        'acceptor rejects (R5 MISMATCH, R3 MATCH)': acc['Decision'] == 'REJECT_FACTS' and acc['Checks'].get('R5') == 'MISMATCH' and acc['Checks'].get('R3') == 'MATCH'}
    print('b1', attacks['b1']['Held'])

    # ================================================================ (b2) replace ref for EFF (= MAIN merge) -> crafted commit with an extra I-62 trailer
    A = Attack('tb2', subset=['identity', 'clausemap'])
    raw = L.git(A.T, '--no-replace-objects', 'cat-file', 'commit', L.MAIN).stdout
    crafted = raw.rstrip(b'\n') + b'\n\nAgent-Protocol-Normative: I-62\n'
    cid, _ = write_obj(A.T, crafted, 'commit')
    rp = L.git(A.T, 'replace', L.MAIN, cid)
    p3 = A.run('p3', 'pwsh')
    p2a = L.p2(A.T, 'pwsh')
    acc = A.accept('p3', p3, {'R4': 'ataque (b2): sin salida del Controller'})
    attacks['b2'] = {'Attack': 'refs/replace/<EFF=MAIN> -> crafted merge commit (same tree and parents, extra Agent-Protocol-Normative trailer)',
                     'Repo': A.T, 'P1': A.p1, 'P2Before': A.p2_before, 'CraftedCommit': cid, 'ReplaceExit': rp.returncode,
                     'ToolFactsVsBaseline': A.compare(p3), 'P3Envelopes': dict((n, env_summary(v['Bytes'])) for n, v in p3.items()),
                     'P2After': p2a, 'AcceptorOnP3': summary_acc(acc)}
    cm = attacks['b2']['ToolFactsVsBaseline']['clausemap']
    attacks['b2']['Held'] = {'tool clausemap output changes': (not cm.get('BytesEqual', True)),
                             'P2 fails at (1) only': p2a['FailedSteps'] == ['1'],
                             'acceptor rejects': acc['Decision'] == 'REJECT_FACTS'}
    print('b2', attacks['b2']['Held'])

    # ================================================================ (c) info/grafts changes a parent (HEADX -> BASE, dropping RED)
    A = Attack('tc')
    cd = L.abspath_dir(A.T, '--git-common-dir')
    L.wbytes(os.path.join(cd, 'info', 'grafts'), (L.HEADX + ' ' + L.BASE + '\n').encode('ascii'))
    gv = {'PlainRevList': L.gout(A.T, 'rev-list', L.BASE + '..' + L.HEADX).splitlines(),
          'NoReplaceRevList': L.gout(A.T, '--no-replace-objects', 'rev-list', L.BASE + '..' + L.HEADX).splitlines(),
          'EnvNoReplaceRevList': L.gout(A.T, 'rev-list', L.BASE + '..' + L.HEADX, env=L.genv({'GIT_NO_REPLACE_OBJECTS': '1'})).splitlines(),
          'IsShallow': L.gout(A.T, 'rev-parse', '--is-shallow-repository')}
    p3 = A.run('p3', 'pwsh')
    p2a = L.p2(A.T, 'pwsh')
    acc = A.accept('p3', p3, {'R4': 'ataque (c): sin salida del Controller', 'R6': 'sin corrida clausemap en el ataque'})
    attacks['c'] = {'Attack': 'info/grafts: HEADX grafted onto BASE (RED dropped)', 'Repo': A.T, 'P1': A.p1, 'P2Before': A.p2_before,
                    'GitViews': gv, 'ToolFactsVsBaseline': A.compare(p3), 'P2After': p2a, 'AcceptorOnP3': summary_acc(acc)}
    attacks['c']['Held'] = {'grafts are NOT disabled by --no-replace-objects / GIT_NO_REPLACE_OBJECTS': gv['NoReplaceRevList'] == gv['PlainRevList'] == gv['EnvNoReplaceRevList'] == [L.HEADX],
                            'tool facts change (identity ancestry, diff commits)': bool(attacks['c']['ToolFactsVsBaseline']['identity']['FactsKeysChanged']),
                            'P2 fails at (2) only': p2a['FailedSteps'] == ['2'],
                            'acceptor rejects (R5 MISMATCH, R3 MATCH)': acc['Decision'] == 'REJECT_FACTS' and acc['Checks'].get('R5') == 'MISMATCH' and acc['Checks'].get('R3') == 'MATCH'}
    print('c', attacks['c']['Held'])

    # ================================================================ (d1) objects/info/alternates (benign foreign object store)
    A = Attack('td1')
    alt_src = R1 + '/alt-src'
    L.clone(alt_src)
    cd = L.abspath_dir(A.T, '--git-common-dir')
    L.wbytes(os.path.join(cd, 'objects', 'info', 'alternates'), (alt_src + '/.git/objects\n').encode('ascii'))
    p3 = A.run('p3', 'pwsh')
    p2a = L.p2(A.T, 'pwsh')
    attacks['d1'] = {'Attack': 'objects/info/alternates pointing to another clone of the fixture (benign content)', 'Repo': A.T, 'P1': A.p1,
                     'P2Before': A.p2_before, 'ToolFactsVsBaseline': A.compare(p3), 'P2After': p2a}
    attacks['d1']['Held'] = {'P2 fails at (2) only': p2a['FailedSteps'] == ['2']}
    print('d1', attacks['d1']['Held'])

    # ================================================================ (d2) extra: alternate store with a hand-made pack that maps id B to other bytes
    alt = R1 + '/alt-d2/objects'
    pack, idx, pname = make_pack(L.B, FAKE_TOOL)
    L.wbytes(alt + '/pack/pack-' + pname + '.pack', pack)
    L.wbytes(alt + '/pack/pack-' + pname + '.idx', idx)
    d2 = {'Attack': 'alternate object store whose pack index claims id B for the fake-tool bytes', 'PackName': pname}
    # (i) alternates added AFTER P1 (B present as a local loose object)
    A = Attack('td2a', subset=['identity'])
    cd = L.abspath_dir(A.T, '--git-common-dir')
    L.wbytes(os.path.join(cd, 'objects', 'info', 'alternates'), (alt + '\n').encode('ascii'))
    d2['AfterP1'] = {'Repo': A.T, 'P1': A.p1, 'GitViews': cat_views(A.T), 'P2After': L.p2(A.T, 'pwsh')}
    p3 = A.run('p3', 'pwsh')
    d2['AfterP1']['P3Envelopes'] = dict((n, env_summary(v['Bytes'])) for n, v in p3.items())
    if any(env_summary(v['Bytes'])['Fabricated'] for v in p3.values()):
        d2['AfterP1']['Acceptor'] = summary_acc(A.accept('p3', p3, {'R4': 'ataque (d2)', 'R6': 'sin clausemap', 'R7': 'sin scope/contract/clausemap'}))
    # (ii) alternates added BEFORE P1
    T = R1 + '/td2b'
    L.clone(T)
    cd = L.abspath_dir(T, '--git-common-dir')
    L.wbytes(os.path.join(cd, 'objects', 'info', 'alternates'), (alt + '\n').encode('ascii'))
    d2['BeforeP1'] = {'Repo': T, 'P1': L.p1(T, DECISION), 'GitViews': cat_views(T), 'P2After': L.p2(T, 'pwsh')}
    inpb = R1 + '/in-td2b'
    Hb, _d, _h, _k = L.build_inputs(T, inpb)
    rc, o, cl, err = L.run_tool_shell(T, dict(L.runs_for(T, inpb, Hb))['identity'], 'pwsh')
    d2['BeforeP1']['P3Identity'] = env_summary(o)
    attacks['d2'] = d2
    b4, af = d2['BeforeP1'], d2['AfterP1']
    attacks['d2']['Held'] = {
        'after P1: the local loose object wins (git serves the pin)': af['GitViews']['NoReplaceCatFile']['IsPinned'],
        'after P1: P2 fails at (2) and fsck (4) reports the corrupt alternate pack': af['P2After']['FailedSteps'] == ['2', '4'],
        'before P1: P1 returns the pinned id but writes no local object (its snapshot check fails)': b4['P1']['IdEqualsPinned'] and not b4['P1']['OnlyNewLooseObject'],
        'before P1: git serves the alternate-pack bytes under id B': b4['GitViews']['NoReplaceCatFile']['BlobSha1OfBytes'] == fake_blob,
        'before P1: P3 executes the fake': b4['P3Identity']['Fabricated'],
        'before P1: P2 fails at (2), (3) and (4)': b4['P2After']['FailedSteps'] == ['2', '3', '4']}
    print('d2', attacks['d2']['Held'], d2['AfterP1']['GitViews'], d2['BeforeP1']['P1']['ReturnedId'], d2['BeforeP1']['P1']['OnlyNewLooseObject'])

    # ================================================================ (e) loose object file of B overwritten with different zlib content
    # (e1) same object content, other zlib level (benign)
    A = Attack('te1', subset=['identity'])
    lp = loose_path(A.T, L.B)
    orig = L.rbytes(lp)
    content = zlib.decompress(orig)
    new = None
    for lvl in (9, 0, 6, 3):
        cand = zlib.compress(content, lvl)
        if cand != orig:
            new, used = cand, lvl
            break
    os.chmod(lp, 0o666)
    L.wbytes(lp, new)
    p2e1 = L.p2(A.T, 'pwsh')
    attacks['e1'] = {'Attack': 'loose object file of B recompressed (zlib level %d): different file bytes, same object content' % used, 'Repo': A.T,
                     'P1': A.p1, 'FileSha256Before': L.sha256(orig), 'FileSha256After': L.sha256(new), 'GitViews': cat_views(A.T), 'P2After': p2e1}
    attacks['e1']['Held'] = {'git returns the pinned bytes': attacks['e1']['GitViews']['NoReplaceCatFile']['IsPinned'], 'P2 passes (benign)': p2e1['Launch']}
    print('e1', attacks['e1']['Held'])
    # (e2) different object content under the id B
    A = Attack('te2', subset=['identity', 'diff'])
    lp = loose_path(A.T, L.B)
    os.chmod(lp, 0o666)
    L.wbytes(lp, zlib.compress(b'blob ' + str(len(FAKE_TOOL)).encode('ascii') + b'\0' + FAKE_TOOL))
    views = cat_views(A.T)
    p3 = A.run('p3', 'pwsh')
    p2e2 = L.p2(A.T, 'pwsh')
    acc = A.accept('p3', p3, {'R4': 'ataque (e2): sin salida del Controller', 'R6': 'sin clausemap', 'R7': 'sin scope/contract/clausemap'})
    attacks['e2'] = {'Attack': 'loose object file of B replaced by zlib(blob header + fake-tool bytes)', 'Repo': A.T, 'P1': A.p1, 'GitViews': views,
                     'P3Envelopes': dict((n, env_summary(v['Bytes'])) for n, v in p3.items()), 'P2After': p2e2, 'AcceptorOnP3': summary_acc(acc)}
    attacks['e2']['Held'] = {'git cat-file blob B returns the fake bytes': views['NoReplaceCatFile']['BlobSha1OfBytes'] == fake_blob,
                             'P3 executes the fake bytes': all(env_summary(v['Bytes'])['Fabricated'] for v in p3.values()),
                             'P2 fails at (3) and (4)': p2e2['FailedSteps'] == ['3', '4'],
                             'acceptor rejects with R1 MISMATCH': acc['Decision'] == 'REJECT_FACTS' and acc['Checks'].get('R1') == 'MISMATCH'}
    print('e2', attacks['e2']['Held'])

    # ================================================================ (f) shallow file
    A = Attack('tf')
    cd = L.abspath_dir(A.T, '--git-common-dir')
    L.wbytes(os.path.join(cd, 'shallow'), (L.RED + '\n').encode('ascii'))
    gv = {'IsShallow': L.gout(A.T, 'rev-parse', '--is-shallow-repository'),
          'MergeBaseBaseRed': L.git(A.T, 'merge-base', '--is-ancestor', L.BASE, L.RED).returncode}
    p3 = A.run('p3', 'pwsh')
    p2a = L.p2(A.T, 'pwsh')
    acc = A.accept('p3', p3, {'R4': 'ataque (f): sin salida del Controller', 'R6': 'sin corrida clausemap en el ataque'})
    attacks['f'] = {'Attack': 'shallow file declaring RED as a shallow boundary', 'Repo': A.T, 'P1': A.p1, 'P2Before': A.p2_before, 'GitViews': gv,
                    'ToolFactsVsBaseline': A.compare(p3), 'P2After': p2a, 'AcceptorOnP3': summary_acc(acc)}
    attacks['f']['Held'] = {'P2 fails at (2) only': p2a['FailedSteps'] == ['2'],
                            'tool facts change': any(v.get('FactsKeysChanged') for v in attacks['f']['ToolFactsVsBaseline'].values()),
                            'acceptor rejects': acc['Decision'] == 'REJECT_FACTS'}
    print('f', attacks['f']['Held'])

    # ================================================================ (g) sequencing: a Worker write between PLAN and VERIFY
    T = R1 + '/tg'
    L.clone(T)
    tl = []
    tl.append({'Step': 'P1 (supervision, outside any cession, before the PLAN Salida)', 'Utc': L.utc(), 'Record': L.p1(T, DECISION)})
    tl.append({'Step': 'P2 before the PLAN launch', 'Utc': L.utc(), 'Record': L.p2(T, 'pwsh')})
    inp = R1 + '/in-tg'
    H, _d, _h, k = L.build_inputs(T, inp)
    argv = dict(L.runs_for(T, inp, H))['identity']
    rc, o, cl, err = L.run_tool_shell(T, argv, 'pwsh')
    tl.append({'Step': 'PLAN invocation (simulated Controller): P3 identity', 'Utc': L.utc(), 'CommandLine': cl, 'Exit': rc, 'Envelope': env_summary(o)})
    tl.append({'Step': 'Entrada of the PLAN invocation: participant processes on T', 'Utc': L.utc(), 'Processes': L.procs_check([T])})
    t_ws = L.utc()
    fid, _ = write_obj(T, FAKE_TOOL)
    rp = L.git(T, 'replace', L.B, fid)
    tl.append({'Step': 'WORKER cession (simulated): writes refs/replace/<B> -> substitute in the same repository', 'Utc': t_ws, 'SubstituteBlob': fid, 'Exit': rp.returncode})
    tl.append({'Step': 'Entrada of the Worker cession', 'Utc': L.utc(), 'Processes': L.procs_check([T])})
    tl.append({'Step': 'round-1 A6-3 rule-1 style check repeated (write-time id check)', 'Utc': L.utc(), 'Record': rewrite_check(T)})
    p2v = L.p2(T, 'pwsh')
    tl.append({'Step': 'P2 before the VERIFY launch (after the Worker Entrada)', 'Utc': L.utc(), 'Record': p2v})
    attacks['g'] = {'Attack': 'sequencing: a write made by the Worker between PLAN and VERIFY', 'Repo': T, 'Timeline': tl}
    attacks['g']['Held'] = {'P2 before PLAN passes': tl[1]['Record']['Launch'] is True,
                            'write-time check still passes after the Worker write': tl[6]['Record']['EqualsPinned'],
                            'P2 before VERIFY fails at (1): no launch': p2v['FailedSteps'] == ['1'] and p2v['Launch'] is False}
    print('g', attacks['g']['Held'])

    # ================================================================ (h) extra: executable planted in the working tree (cmd.exe searches the current directory)
    A = Attack('th', subset=['identity', 'diff'])
    bat = ('@echo off\r\n'
           'if "%~1"=="hash-object" (echo ' + L.B + '& exit /b 0)\r\n'
           'if "%~3"=="-t" (echo blob& exit /b 0)\r\n'
           'if "%~3"=="blob" (type "%~dp0fake.py"& exit /b 0)\r\n'
           'exit /b 1\r\n').encode('ascii')
    L.wbytes(A.T + '/git.bat', bat)
    L.wbytes(A.T + '/fake.py', FAKE_TOOL)
    na_h = {'R4': 'ataque (h): sin salida del Controller', 'R6': 'sin clausemap', 'R7': 'sin scope/contract/clausemap'}
    variants = {}
    for lab, env in (('inherited', L.genv()), ('unset', L.env_without_nodefaultcwd())):
        v = {'NoDefaultCurrentDirectoryInExePath': [val for key, val in env.items() if key.upper() == 'NODEFAULTCURRENTDIRECTORYINEXEPATH'] or None}
        v['P2_cmd'] = L.p2(A.T, 'cmd', env=env)
        v['P2_pwsh'] = L.p2(A.T, 'pwsh', env=env)
        ce = A.run('p3-cmd-' + lab, 'cmd', env=env)
        pe = A.run('p3-pwsh-' + lab, 'pwsh', env=env)
        v['P3_cmd'] = dict((n, env_summary(x['Bytes'])) for n, x in ce.items())
        v['P3_cmdEqualsBaseline'] = dict((n, ce[n]['Bytes'] == A.base[n]['Bytes']) for n in A.subset)
        v['P3_pwshEqualsBaseline'] = dict((n, pe[n]['Bytes'] == A.base[n]['Bytes']) for n in A.subset)
        if any(x['Fabricated'] for x in v['P3_cmd'].values()):
            v['AcceptorOnCmdEnvelopes'] = summary_acc(A.accept('p3-cmd-' + lab, ce, na_h))
        variants[lab] = v
    inh, uns = variants['inherited'], variants['unset']
    attacks['h'] = {'Attack': 'git.bat + fake.py planted in the working tree root (untracked); P2 step 3 and P3 run with cwd = T',
                    'Repo': A.T, 'P1': A.p1, 'Variants': variants}
    attacks['h']['Held'] = {
        'inherited env (NoDefaultCurrentDirectoryInExePath set): cmd.exe uses the real git (P2 ok, P3 = baseline)':
            inh['P2_cmd']['Launch'] and inh['P2_cmd']['Steps']['3']['Extra_ConsumerDigest'].get('EqualsPinned') is True and all(inh['P3_cmdEqualsBaseline'].values()),
        'variable unset: P2 through cmd.exe is fooled (Launch true while the delivered bytes are not the pin)':
            uns['P2_cmd']['Launch'] is True and uns['P2_cmd']['Steps']['3']['Extra_ConsumerDigest'].get('EqualsPinned') is False,
        'variable unset: P3 through cmd.exe executes the fake': all(x['Fabricated'] for x in uns['P3_cmd'].values()),
        'variable unset: acceptor rejects the cmd envelopes': (uns.get('AcceptorOnCmdEnvelopes') or {}).get('Decision') == 'REJECT_FACTS',
        'pwsh unaffected in both variants (P2 Launch, P3 = baseline)': all(variants[x]['P2_pwsh']['Launch'] and all(variants[x]['P3_pwshEqualsBaseline'].values())
                                                                        for x in variants)}
    print('h', attacks['h']['Held'])

    res_a = {'Schema': 'r1r4-bench/r1-attacks', 'Versions': vers, 'FakeToolBlob': fake_blob, 'FakeToolSha256': L.sha256(FAKE_TOOL), 'Attacks': attacks}
    L.write_result('r1-attacks.json', res_a)
    print('done')


if __name__ == '__main__':
    main()
