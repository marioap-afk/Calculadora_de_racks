# lib_r3.py - helpers of the A-6 round 3 bench (I-62): round-2 lib (lib_r1r4.py, byte copy) reused with this bench's
# paths, plus the declared execution form, the Codex-format session log, synthetic Controller outputs and the acceptor
# runner. Stdlib only. Never writes in the I-62 worktree, the main checkout or D:\r62-fixture.
import importlib.util, json, os, platform, shutil, subprocess, sys, uuid

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


L = _load('lib_r1r4')
R = _load('reconstruct_runs')
TB = _load('table_r3')

WORK = 'D:/r62-a6r3/r1'
L.WORK = WORK
L.PIN = WORK + '/pin'
L.PIN_TOOL = L.PIN + '/bmin_facts.py'
L.PIN_ACC = L.PIN + '/bmin_accept.py'
L.BIN = WORK + '/bin'
L.HERE = HERE

GIT_EXE = os.path.normpath(shutil.which('git'))
PY_EXE = sys.executable
PWSH = L.PWSH
NX = 'deadbeef' * 5                     # 40-hex id that is not an object of the fixture (checked at run time)
ORDER = ['Termination', 'Handoff', 'Authority', 'Contract', 'Identity', 'Remote', 'Scope', 'CleanTree', 'Ci', 'Tests', 'Trailer',
         'Routing', 'FreeText', 'Denials']
ROW_DISP = {'Termination': 'BLOCKED', 'Authority': 'STOP', 'Contract': 'STOP', 'Identity': 'STOP', 'Remote': 'BLOCKED', 'Scope': 'STOP',
            'CleanTree': 'REWORK', 'Ci': 'BLOCKED', 'Tests': 'REWORK', 'Trailer': 'REWORK', 'Routing': 'BLOCKED', 'FreeText': 'REWORK',
            'Denials': 'BLOCKED'}
V5_LOG = os.path.join(L.WT, 'docs', 'automation', 'evidence', 'I-62-F6', 'OD-2', 'R20261011T102825Z-a2p2-v5-controller', 'probe-events.jsonl')
SCHEMAS = {}


def schema(name):
    if name not in SCHEMAS:
        SCHEMAS[name] = L.load_schema(name)
    return SCHEMAS[name]


def sha_file(p):
    return L.sha256(L.rbytes(p))


def canon_bytes(o):
    return (L.canon(o) + '\n').encode('ascii')


# ------------------------------------------------------------------ declared execution form (P3 with explicit executables)
def pw_sq(s):
    return "'" + s.replace("'", "''") + "'"


def inner_cmd(argv, git=None, py=None):
    L.check_tokens(argv)
    return ('& ' + pw_sq(git or GIT_EXE) + ' --no-replace-objects cat-file blob ' + L.B + ' | & ' + pw_sq(py or PY_EXE) + ' -I - '
            + ' '.join(argv))


def run_pwsh(inner, cwd):
    argv = [PWSH, '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', inner]
    p = subprocess.run(argv, cwd=cwd, capture_output=True, env=L.genv(), stdin=subprocess.DEVNULL, timeout=900)
    return p.returncode, p.stdout, p.stderr, argv


def recorded_command(inner):
    """The command string as the Codex adapter records it ("<pwsh>" -Command <inner>, measured rendering)."""
    return R.render_command([PWSH, '-Command', inner])


def profile_noise_line():
    """The stderr line that precedes every command output in the custodied v5 Controller log (read, not retyped)."""
    for line in L.rbytes(V5_LOG).split(b'\n'):
        if not line.strip():
            continue
        e = json.loads(line.decode('utf-8'))
        it = e.get('item') or {}
        if e['type'] == 'item.completed' and it.get('type') == 'command_execution':
            ao = it['aggregated_output']
            return ao[:ao.index('\n') + 1]
    raise SystemExit('no command in the v5 log')


# ------------------------------------------------------------------ Codex-format session log
class CodexLog(object):
    def __init__(self):
        self.events = [{'type': 'thread.started', 'thread_id': str(uuid.uuid4())}, {'type': 'turn.started'}]
        self.n = 0

    def agent(self, text):
        self.events.append({'type': 'item.completed', 'item': {'id': 'item_%d' % self.n, 'type': 'agent_message', 'text': text}})
        self.n += 1

    def command(self, command, aggregated, exit_code, status=None):
        iid = 'item_%d' % self.n
        self.n += 1
        self.events.append({'type': 'item.started', 'item': {'id': iid, 'type': 'command_execution', 'command': command,
                                                             'aggregated_output': '', 'exit_code': None, 'status': 'in_progress'}})
        self.events.append({'type': 'item.completed', 'item': {'id': iid, 'type': 'command_execution', 'command': command,
                                                               'aggregated_output': aggregated, 'exit_code': exit_code,
                                                               'status': status or ('completed' if exit_code == 0 else 'failed')}})

    def finish(self):
        self.events.append({'type': 'turn.completed', 'usage': {'input_tokens': 0, 'cached_input_tokens': 0, 'cache_write_input_tokens': 0,
                                                                'output_tokens': 0, 'reasoning_output_tokens': 0}})

    def data(self):
        return ''.join(json.dumps(e, ensure_ascii=False, separators=(',', ':')) + '\n' for e in self.events).encode('utf-8')


def log_from_execs(execs, noise='', mutate=None, finish=True):
    """Builds a Codex-format log from execution records {Command, Stdout, Stderr, Rc}; mutate(i, rec) -> rec or None."""
    lg = CodexLog()
    lg.agent('Controller simulado por el banco r3: VERIFY de FX-U1/T1 (sin modelo).')
    for i, x in enumerate(execs):
        rec = dict(x)
        rec['Aggregated'] = noise + rec['Stderr'].decode('utf-8', 'replace') + rec['Stdout'].decode('utf-8', 'replace')
        rec['Status'] = None
        if mutate:
            rec = mutate(i, rec)
            if rec is None:
                continue
        lg.command(rec['Command'], rec['Aggregated'], rec['Rc'], rec.get('Status'))
    lg.agent('fin de la verificacion (banco r3)')
    if finish:
        lg.finish()
    return lg


# ------------------------------------------------------------------ relay facts (as round 2)
def op_in_progress(T):
    gd = L.abspath_dir(T, '--git-dir')
    return any(os.path.exists(os.path.join(gd, x)) for x in ('MERGE_HEAD', 'CHERRY_PICK_HEAD', 'REVERT_HEAD', 'BISECT_LOG', 'rebase-merge', 'rebase-apply'))


def fingerprint():
    return {'Kind': 'NONE', 'Sha256': None, 'KeyNames': [], 'AcceptanceDecisionRef': None}


def proc_rows(pc):
    return [{'Pid': int(m['Pid']), 'ParentPid': int(m['ParentPid']), 'Name': m['Name'], 'CreationDateUtc': m.get('CreationDateUtc') or '',
             'Classification': 'participant'} for m in (pc.get('Matching') or [])]


def salida(T):
    pc = L.procs_check([T])
    lsr = L.gout(T, 'ls-remote', 'origin', 'refs/heads/' + L.BR).split()
    return {'HeadSha': L.gout(T, 'rev-parse', 'HEAD'), 'LsRemoteSha': lsr[0] if lsr else '0' * 40,
            'OriginMainSha': L.gout(T, 'rev-parse', 'refs/remotes/origin/main'),
            'CleanTree': L.git(T, 'status', '--porcelain').stdout == b'', 'GitOperationInProgress': op_in_progress(T),
            'SummaryCommitSha': L.gout(T, 'rev-parse', 'HEAD'), 'Fingerprint': fingerprint(), 'Processes': proc_rows(pc),
            'OpenDelegations': 1, 'DelegationStatus': 'ACCEPTED_OPEN'}


def entrada(T):
    pc = L.procs_check([T])
    lsr = L.gout(T, 'ls-remote', 'origin', 'refs/heads/' + L.BR).split()
    return {'HeadSha': L.gout(T, 'rev-parse', 'HEAD'), 'LsRemoteSha': lsr[0] if lsr else '0' * 40,
            'CleanTree': L.git(T, 'status', '--porcelain').stdout == b'', 'Fingerprint': fingerprint(), 'FingerprintChanged': False,
            'Processes': proc_rows(pc)}


def relay_record(sess, cv_path, run_id):
    host = {'HostLabelHash': L.sha256(b'r3-bench-host'), 'HostInstanceHash': 'UNKNOWN', 'HostInstanceState': 'UNOBSERVED', 'Os': platform.platform()}
    return {'Schema': 'rackcad-relay-record/v2', 'TaskId': 'T1', 'RunId': run_id, 'Attempt': 0, 'Phase': 'VERIFICATION', 'WindowSeq': 1,
            'PrevRelaySha256': None, 'Host': host, 'Participant': None, 'Exit': sess['Exit'],
            'Cession': {'StartUtc': sess['CessionStartUtc'], 'EndUtc': sess['CessionEndUtc'], 'SessionOperated': False},
            'Outcome': {'Kind': 'COMPLETED', 'LaunchedPid': None, 'ExitCode': 0, 'TerminalEvent': None, 'TreeKilled': False, 'DeathConfirmed': True,
                        'OutputPath': cv_path, 'OutputSha256': sha_file(cv_path), 'OutputWrittenUtc': L.utc_s(), 'Acceptance': None},
            'Entry': sess['Entry'], 'RemoteFacts': None, 'RebaseMap': None, 'RoleInvocation': None, 'ReadAudit': None,
            'InputFidelity': {'path': 'bmin_facts.py (objeto Git suelto en T)', 'blob': L.B}, 'Disposition': 'NONE',
            'Notes': ['Registro de relevo del banco r3 (sin participante real: Participant null); hechos del relevo para R10.']}


# ------------------------------------------------------------------ VERIFY session (simulated Controller, real executions)
def verify_session(label, T, runs, primary=()):
    """Every instrument run goes through the declared form in pwsh (cwd = T). primary: [(check, inner)] plain reads.
    Writes the Codex-format log; returns the session record and the execution records."""
    work = WORK + '/' + label
    L.ensure_dir(work)
    ex = salida(T)
    s_pre = L.snapshot(T)
    c0 = L.utc_s()
    execs, prim = [], {}
    for name, argv in runs:
        inner = inner_cmd(argv)
        rc, out, err, real = run_pwsh(inner, T)
        execs.append({'Name': name, 'Argv': argv, 'Inner': inner, 'Command': recorded_command(inner), 'ExecutedArgv': real, 'Rc': rc,
                      'Stdout': out, 'Stderr': err, 'Instrument': True})
    for check, inner in primary:
        rc, out, err, real = run_pwsh(inner, T)
        execs.append({'Name': 'primary-' + check, 'Argv': None, 'Inner': inner, 'Command': recorded_command(inner), 'ExecutedArgv': real,
                      'Rc': rc, 'Stdout': out, 'Stderr': err, 'Instrument': False})
        prim[check] = (inner, (err + out).decode('utf-8', 'replace'), rc)
    c1 = L.utc_s()
    s_post = L.snapshot(T)
    en = entrada(T)
    lg = log_from_execs(execs)
    logp = work + '/session-events.jsonl'
    L.wbytes(logp, lg.data())
    sess = {'Label': label, 'T': T, 'LogPath': logp, 'LogSha256': sha_file(logp), 'Exit': ex, 'Entry': en, 'CessionStartUtc': c0,
            'CessionEndUtc': c1, 'ZeroWriteToolRuns': L.snapdiff(s_pre, s_post) == {}, 'Primary': prim,
            'Executions': [{'Name': x['Name'], 'Instrument': x['Instrument'], 'Rc': x['Rc'], 'StdoutSha256': L.sha256(x['Stdout']),
                            'StdoutLen': len(x['Stdout']), 'StderrLen': len(x['Stderr']), 'RecordedCommand': x['Command'],
                            'ExecutedArgv': x['ExecutedArgv']} for x in execs]}
    return sess, execs


def reconstruct_into(logp, env_dir):
    rec = R.reconstruct(L.rbytes(logp), L.B, env_dir=env_dir)
    runs = [{'Name': 'r%02d-%s' % (r['Seq'], r['Argv'][0]), 'Envelope': r['EnvelopePath'], 'Argv': r['Argv']} for r in rec['Runs']] \
        if rec['Reconstructible'] else None
    return rec, runs


def facts_by_cmd(runs):
    """{name: (facts, factsSha, argv)} from reconstructed runs (name = argv-based label as in round 2)."""
    out = {}
    for r in runs:
        e = json.loads(L.rbytes(r['Envelope']).decode('ascii'))
        a = r['Argv']
        name = a[0]
        if a[0] == 'read-text':
            name = 'read-text-' + a[a.index('--lines') + 1].split('-')[0]
        out[name] = (e['Facts'], e['FactsSha256'], a, e['Error'])
    return out


# ------------------------------------------------------------------ Controller outputs (synthetic, 16.9-coherent)
def _ev(F, n, extra=''):
    f, s, a, _ = F[n]
    return 'instrumento: bmin_facts ' + ' '.join(a) + '; FactsSha256=' + s + (('; ' + extra) if extra else '')


def authority_from(F):
    cm = F['clausemap'][0]
    v = cm.get('Validate') or {}
    mv4 = v.get('MV4') or {}
    mv7 = v.get('MV7') or {}
    defects = [k for k in ('LiteralViolations', 'AddedPresentInEff1', 'EntryFilePresentInEff1', 'EntryFilesOutsideSchema') if mv4.get(k)]
    defects += [k for k in ('PointerIsFirstSentenceS16', 'PointerIsLastSentenceS163') if mv7.get(k) is False]
    ok = cm['Derive']['Eff'] is not None and not defects
    ar = [{'Path': c['Path'], 'Section': c['Section'], 'Class': c['Class'], 'Mode': 'NORMAL', 'RevisionSha': cm['Resolve']['MainSha'], 'Parts': [],
           'EffectiveSha': cm['Derive']['Eff'], 'ClauseMapBlob': v['MV1']['MapBlob'], 'UnitProtocol': 'rackcad-protocol/I62'}
          for c in cm['Resolve']['Citations']]
    return ok, defects, ar


def contract_from(F):
    c = F['contract'][0]
    return all(x['ContainedBy'] for x in c['A3']) and all(x['ContainedBy'] for x in c['A4']) and not any(c['A5Missing'].values()) and c['SupersededEqual']


def cleantree_from(F):
    ct = F['cleantree'][0]
    return ct['StatusPorcelain'] == [] and ct['CheckIgnoreRc'] == 0


def handoff_from(F, deleg):
    ho = F['handoff'][0]
    hf = ho.get('Fields') or {}
    return bool(ho['Present'] and ho['ValidJson'] and not ho['MissingRequiredKeys'] and hf.get('TaskId') == deleg['TaskId']
                and hf.get('DelegationRunId') == deleg['RunId'] and hf.get('Attempt') == deleg['Attempt'])


def res(ok):
    return 'pass' if ok else 'fail'


def nr_bench(why):
    return {'Result': 'not_run', 'Evidence': 'sin senal en el banco r3 (' + why + '); not_run con la disposicion de su fila (AP 16.9)'}


def nr_noinput(cause, red=False):
    d = {'Result': 'not_run', 'Evidence': 'sin entrada: ' + cause + '; not_run sin disposicion (AP 16.9)'}
    if red:
        d['RedPart'] = 'not_applicable'
    return d


def cv_doc(checks, ar, disp, cls, findings, run_id, work_run, deleg_run, triggered=()):
    first = next((c for c in ORDER if checks[c]['Result'] != 'pass'), None)
    return {'Schema': 'rackcad-controller-verification/v2', 'TaskId': 'T1', 'RunId': run_id, 'DelegationRunId': deleg_run,
            'WorkRunId': work_run, 'Attempt': 0, 'VerifiedSha': None,
            'Verifier': {'Role': 'EXECUTION_CONTROLLER',
                         'BindingRef': {'UnitId': 'FX-U1', 'Scope': 'TASK', 'TaskId': 'T1', 'Role': 'EXECUTION_CONTROLLER', 'BindingId': 'B20261013T000000Z-a6b5',
                                        'Sha256': '0' * 64, 'Location': {'Kind': 'TRANSIENT', 'Path': 'banco-r3', 'Commit': None, 'Blob': None}},
                         'Actor': {'AdapterId': 'bench-r3', 'InstanceId': 'NOT_STARTED', 'InstanceIdSource': 'banco r3: Controller simulado, sin modelo', 'Assurance': 'NONE'}},
            'Checks': checks, 'AuthorityResolution': ar, 'Classification': cls, 'Disposition': disp, 'FailureClass': first or 'NONE',
            'TriggeredStopConditions': list(triggered), 'Findings': list(findings), 'RecommendedNextAction': 'devolver al Coordinator (banco r3)'}


def coherence_169(cv, no_input, handoff_absent=False):
    """Mechanical 16.9 reading: FailureClass = first fail/not_run; VERIFIED iff 14 pass; valid pairs; strict max of the row
    dispositions, skipping not_run without input (no_input). Returns the expected values next to the declared ones."""
    ch = cv['Checks']
    first = next((c for c in ORDER if ch[c]['Result'] != 'pass'), None)
    disp = set()
    contrib = {}
    for c in ORDER:
        r = ch[c]['Result']
        if r == 'pass' or (r == 'not_run' and c in no_input):
            continue
        if c == 'Handoff':
            d = 'BLOCKED' if handoff_absent else ('REWORK' if ch['Identity']['Result'] == 'pass' else 'STOP')
        else:
            d = ROW_DISP[c]
        contrib[c] = d
        disp.add(d)
    exp = 'STOP' if 'STOP' in disp else ('BLOCKED' if 'BLOCKED' in disp else ('REWORK' if 'REWORK' in disp else 'NONE'))
    pairs = {('EXECUTION_VERIFIED', 'NONE'), ('EXECUTION_REWORK_REQUIRED', 'REWORK'), ('EXECUTION_BLOCKED', 'BLOCKED'), ('EXECUTION_BLOCKED', 'STOP')}
    allpass = all(ch[c]['Result'] == 'pass' for c in ORDER)
    return {'FailureClassExpected': first or 'NONE', 'FailureClassDeclared': cv['FailureClass'],
            'DispositionStrictMax': exp, 'DispositionDeclared': cv['Disposition'], 'Contributions': contrib,
            'NoInputNotRun': sorted(no_input), 'ValidPair': (cv['Classification'], cv['Disposition']) in pairs,
            'VerifiedIffAllPass': (cv['Classification'] == 'EXECUTION_VERIFIED') == allpass,
            'FailureClassOk': (first or 'NONE') == cv['FailureClass'], 'DispositionOk': exp == cv['Disposition']}


def cv_t1(F, deleg, handoff, prim):
    """Round-2 derive_cv (run_r4.py), same rules and texts, primary reads taken from the Codex-format session."""
    def ev(n, check, agrees, extra=''):
        pa = prim.get(check)
        ptxt = ('primaria: ' + pa[0] + ' -> coincide con los hechos: ' + ('si' if agrees else 'NO')) if pa else 'primaria: no hay orden Git primaria para esta comprobacion'
        return _ev(F, n, ptxt + (('; ' + extra) if extra else ''))

    idf = F['identity'][0]
    same = lambda a, b: os.path.normcase(os.path.abspath(a)) == os.path.normcase(os.path.abspath(b))
    ident_ok = (idf['AbbrevRef'] == handoff['Branch'] == deleg['ExpectedBranch'] and same(idf['Toplevel'], handoff['Worktree'])
                and same(handoff['Worktree'], deleg['ExpectedWorktree']) and handoff['BaseSha'] == deleg['BaseSha']
                and all(idf['Ancestry'].values()) and idf['Head'] == handoff['CurrentSha'] == idf['OriginBranch'])
    sc = F['scope'][0]
    scope_ok = (not sc['MalformedEntries']) and all(p['AllowedCoveredBy'] and not p['ForbiddenCoveredBy'] for p in sc['PerPath'])
    ct = F['cleantree'][0]
    clean_ok = cleantree_from(F)
    df = F['diff'][0]
    declared = handoff['Worker']['Trailer'].split(':', 1)[1].strip() if handoff['Worker']['Trailer'].lower().startswith('co-authored-by:') else handoff['Worker']['Trailer']
    trailer_ok = all(declared in df['CoAuthoredBy'].get(c, []) for c in df['Commits'])
    ft = F['freetext'][0]
    free_ok = not ft['Matches']
    auth_ok, defects, ar = authority_from(F)
    cm = F['clausemap'][0]
    v = cm.get('Validate') or {}
    contract_ok = contract_from(F)
    ho = F['handoff'][0]
    handoff_ok = handoff_from(F, deleg)
    p_head = prim['Identity'][1].strip()
    p_scope = [x for x in prim['Scope'][1].splitlines() if x]
    p_clean = prim['CleanTree'][1] == ''
    p_commits = sorted(x for x in prim['Trailer'][1].split() if x)
    p_map = prim['Authority'][1].strip()
    nr = lambda why: {'Result': 'not_run', 'Evidence': 'sin senal en el banco r3 (' + why + ')'}
    checks = {
        'Termination': nr('Controller simulado; sin registro de terminacion del Worker'),
        'Handoff': {'Result': res(handoff_ok), 'Evidence': ev('handoff', 'Handoff', True, 'MissingRequiredKeys=%d' % len(ho['MissingRequiredKeys']))},
        'Authority': {'Result': res(auth_ok), 'Evidence': ev('clausemap', 'Authority', p_map == (v.get('MV1') or {}).get('MapBlob'),
                                                             'defectos del mapa: ' + (','.join(defects) or 'ninguno'))},
        'Contract': {'Result': res(contract_ok), 'Evidence': ev('contract', 'Contract', True)},
        'Identity': {'Result': res(ident_ok), 'Evidence': ev('identity', 'Identity', p_head == idf['Head'],
                                                             'Head=%s CurrentSha=%s' % (idf['Head'], handoff['CurrentSha']))},
        'Remote': nr('sin registro de ls-remote del relevo en la salida del Controller'),
        'Scope': {'Result': res(scope_ok), 'Evidence': ev('scope', 'Scope', p_scope == sc['Paths'])},
        'CleanTree': {'Result': res(clean_ok), 'Evidence': ev('cleantree', 'CleanTree', p_clean == (ct['StatusPorcelain'] == []))},
        'Ci': {'Result': 'not_run', 'RedPart': 'not_applicable', 'Evidence': 'sin corrida de CI en el banco r3'},
        'Tests': {'Result': 'not_run', 'RedPart': 'not_applicable', 'Evidence': 'sin TRX de RedSha en el banco r3'},
        'Trailer': {'Result': res(trailer_ok), 'Evidence': ev('diff', 'Trailer', p_commits == sorted(df['Commits']), 'declarado=' + declared)},
        'Routing': nr('sin modelo'),
        'FreeText': {'Result': res(free_ok), 'Evidence': ev('freetext', 'FreeText', True, 'coincidencias=%d' % len(ft['Matches']))},
        'Denials': nr('sin eventos ni transcripcion'),
    }
    disp = set()
    for c in ORDER:
        if checks[c]['Result'] != 'pass':
            disp.add('REWORK' if (c == 'Handoff' and ident_ok) else ('STOP' if c == 'Handoff' else ROW_DISP[c]))
    d = 'STOP' if 'STOP' in disp else ('BLOCKED' if 'BLOCKED' in disp else ('REWORK' if 'REWORK' in disp else 'NONE'))
    cls = {'NONE': 'EXECUTION_VERIFIED', 'REWORK': 'EXECUTION_REWORK_REQUIRED', 'BLOCKED': 'EXECUTION_BLOCKED', 'STOP': 'EXECUTION_BLOCKED'}[d]
    findings = ['Salida sintetica del banco r3: Controller simulado por la supervision, derivada mecanicamente de los sobres del instrumento reconstruidos del registro de sesion (formato Codex).',
                'AuthorityResolution: Mode y RevisionSha son valores de banco (no derivados de E.3.1); EffectiveSha y ClauseMapBlob salen de los hechos clausemap.']
    return cv_doc(checks, ar, d, cls, findings, L.CV_RUN, handoff['RunId'], deleg['RunId'], ['S-T1-01'] if not scope_ok else [])


def write_json_input(p, obj):
    L.wjson_local(p, obj)
    return p


def validate(name, obj):
    return L.validate(schema(name)[0], obj)


# ------------------------------------------------------------------ requests and acceptor
def consts_for(T, closure, relay):
    xsha, _ = L.exe_sha256()
    return {'Repo': T, 'Tool': {'PinnedBlob': L.B, 'RunFrom': 'object', 'GitObject': {'GitDir': T, 'Blob': L.B}},
            'Interpreter': {'Python': platform.python_version(), 'ExecutableSha256': xsha},
            'Closure': {'Path': closure, 'Sha256': sha_file(closure)}, 'RelayFacts': {'Path': relay, 'CitedSha256': sha_file(relay)},
            'IgnorePath': L.IGN, 'TermsRev': L.MAIN, 'MainSha': L.MAIN}


def round2_request(consts, runs, cv_path, deleg_path, handoff_path, handoff_sha, na=None):
    """Round-2 construction: Verification always included (no R4 rule)."""
    req = {'Schema': TB.REQUEST_SCHEMA, 'Repo': consts['Repo'], 'Tool': consts['Tool'], 'Interpreter': consts['Interpreter'], 'Runs': runs,
           'Closure': consts['Closure'], 'RelayFacts': consts['RelayFacts'],
           'Verification': {'Controller': {'Path': cv_path, 'Sha256': sha_file(cv_path)}, 'Delegation': {'Path': deleg_path, 'Sha256': sha_file(deleg_path)},
                            'Handoff': {'Path': handoff_path, 'Sha256': handoff_sha}, 'IgnorePath': consts['IgnorePath'],
                            'TermsRev': consts['TermsRev'], 'MainSha': consts['MainSha']}}
    if na:
        req['NotApplicable'] = na
    return req


def write_request(p, req):
    L.wjson_local(p, req)
    return p


def r4_rows(res_obj):
    for c in res_obj.get('Checks', []):
        if c.get('Id') == 'R4' and isinstance(c.get('Detail'), dict):
            d = c['Detail']
            if 'Checks' in d:
                return [{'Check': r.get('Check'), 'Controller': r.get('Controller'), 'AcceptorExpects': r.get('AcceptorExpects'), 'Status': r.get('Status'),
                         'Reasons': r.get('Reasons'), 'Defects': (len(r['Defects']) if isinstance(r.get('Defects'), list) else r.get('Defects'))}
                        for r in d['Checks']]
            return d
    return None


def detail(res_obj, rid, n=900):
    for c in res_obj.get('Checks', []):
        if c.get('Id') == rid:
            s = L.canon(c.get('Detail'))
            return s[:n]
    return None


def accept_and_table(T, req_path, cv_path, deleg_path, handoff_path, consts, executions=None, reconstructed=None):
    """Acceptor run by the supervision (read-only, zero-write snapshot, process check) + table_r3 evaluation."""
    acc = L.run_acceptor(req_path, snap_repos=[T], proc_paths=[T])
    out = L.rbytes(acc['StdoutPath'])
    tab = TB.evaluate(req_path, acc['Rc'], out, cv_path, deleg_path, handoff_path, consts, executions, reconstructed)
    pc = acc['Processes'] or {}
    r = acc['_Result']
    obs = {'Rc': acc['Rc'], 'Decision': acc['Decision'], 'Checks': acc['Checks'], 'Error': acc.get('Error'),
           'MismatchDetails': dict((rid, detail(r, rid)) for rid, st in acc['Checks'].items() if st == 'MISMATCH'),
           'NotApplicableDetails': dict((rid, detail(r, rid, 400)) for rid, st in acc['Checks'].items() if st == 'NOT_APPLICABLE'),
           'R4Rows': r4_rows(r), 'RequestSha256': acc['RequestSha256'], 'AcceptorStdoutSha256': acc['StdoutSha256'],
           'ZeroWrite': all(z['Equal'] for z in acc['ZeroWrite']),
           'ProcessTreeDead': not pc.get('Matching') and not pc.get('ChildrenOfGivenParents') and 'Error' not in pc,
           'Table': tab}
    return obs


def mismatches(obs):
    return sorted(k for k, v in obs['Checks'].items() if v == 'MISMATCH')
