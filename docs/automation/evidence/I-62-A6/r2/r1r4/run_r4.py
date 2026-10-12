# run_r4.py - evidence for candidate procedure R4 (where and when the acceptor runs), I-62 A-6 round 2.
# python -I run_r4.py            (sequential; work area D:\r62-a6r2\r1r4\r4; results next to this script)
# The acceptor runs on the SAME repository/working tree T the Controller's tool runs used, after the Entrada of the VERIFY invocation and
# before any later session write; the request's Runs are recovered from a (simulated) Controller session log. No model is invoked.
import base64, hashlib, importlib.util, json, os, platform, shutil, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_s = importlib.util.spec_from_file_location('lib_r1r4', os.path.join(HERE, 'lib_r1r4.py'))
L = importlib.util.module_from_spec(_s)
_s.loader.exec_module(L)

R4 = L.WORK + '/r4'
DECISION = 'BANCO-r1r4 (sin disposicion: banco de pruebas de la evidencia A-6 ronda 2; no es una DecisionRef normativa)'
ORDER = ['Termination', 'Handoff', 'Authority', 'Contract', 'Identity', 'Remote', 'Scope', 'CleanTree', 'Ci', 'Tests', 'Trailer', 'Routing',
         'FreeText', 'Denials']
ROW_DISP = {'Termination': 'BLOCKED', 'Authority': 'STOP', 'Contract': 'STOP', 'Identity': 'STOP', 'Remote': 'BLOCKED', 'Scope': 'STOP',
            'CleanTree': 'REWORK', 'Ci': 'BLOCKED', 'Tests': 'REWORK', 'Trailer': 'REWORK', 'Routing': 'BLOCKED', 'FreeText': 'REWORK',
            'Denials': 'BLOCKED'}
SCHEMAS = {}


def schema(name):
    if name not in SCHEMAS:
        SCHEMAS[name] = L.load_schema(name)
    return SCHEMAS[name]


def same_path(a, b):
    return os.path.normcase(os.path.abspath(a)) == os.path.normcase(os.path.abspath(b))


def git_in(T, *args):
    """A git read made by the (simulated) Controller inside its cession: cwd = T, read-only settings."""
    p = subprocess.run(['git'] + list(args), cwd=T, capture_output=True, env=L.genv(), stdin=L.DEVNULL)
    return p.returncode, p.stdout.decode('utf-8', 'replace')


def proc_rows(pc):
    return [{'Pid': int(m['Pid']), 'ParentPid': int(m['ParentPid']), 'Name': m['Name'], 'CreationDateUtc': m.get('CreationDateUtc') or '',
             'Classification': 'participant'} for m in (pc.get('Matching') or [])]


def op_in_progress(T):
    gd = L.abspath_dir(T, '--git-dir')
    return any(os.path.exists(os.path.join(gd, x)) for x in ('MERGE_HEAD', 'CHERRY_PICK_HEAD', 'REVERT_HEAD', 'BISECT_LOG', 'rebase-merge', 'rebase-apply'))


def fingerprint():
    # config.toml is never read in this bench (hard limit): Kind NONE.
    return {'Kind': 'NONE', 'Sha256': None, 'KeyNames': [], 'AcceptanceDecisionRef': None}


def salida(T):
    pc = L.procs_check([T])
    lsr = L.gout(T, 'ls-remote', 'origin', 'refs/heads/' + L.BR).split()
    return {'HeadSha': L.gout(T, 'rev-parse', 'HEAD'), 'LsRemoteSha': lsr[0] if lsr else '0' * 40,
            'OriginMainSha': L.gout(T, 'rev-parse', 'refs/remotes/origin/main'),
            'CleanTree': L.git(T, 'status', '--porcelain').stdout == b'', 'GitOperationInProgress': op_in_progress(T),
            'SummaryCommitSha': L.gout(T, 'rev-parse', 'HEAD'), 'Fingerprint': fingerprint(), 'Processes': proc_rows(pc),
            'OpenDelegations': 1, 'DelegationStatus': 'ACCEPTED_OPEN'}, pc


def entrada(T):
    pc = L.procs_check([T])
    lsr = L.gout(T, 'ls-remote', 'origin', 'refs/heads/' + L.BR).split()
    return {'HeadSha': L.gout(T, 'rev-parse', 'HEAD'), 'LsRemoteSha': lsr[0] if lsr else '0' * 40,
            'CleanTree': L.git(T, 'status', '--porcelain').stdout == b'', 'Fingerprint': fingerprint(), 'FingerprintChanged': False,
            'Processes': proc_rows(pc)}, pc


def controller_session(T, inp, H, work, shell='pwsh'):
    """Simulated VERIFY invocation: every tool run goes through P3 in `shell` with cwd = T; primary git reads for contrast; all logged."""
    runs = L.runs_for(T, inp, H)
    log = []
    ex, pc_exit = salida(T)
    c0 = L.utc_s()
    for name, argv in runs:
        t = L.utc()
        rc, out, cl, err = L.run_tool_shell(T, argv, shell)
        log.append({'Seq': len(log), 'Utc': t, 'Kind': 'shell', 'Shell': shell, 'Cwd': T, 'CommandLine': cl, 'ExitCode': rc,
                    'StdoutSha256': L.sha256(out), 'StdoutB64': base64.b64encode(out).decode('ascii')})
    prim = {
        'Identity': ['rev-parse', 'HEAD'],
        'Scope': ['diff', '--name-only', '--no-renames', L.BASE + '..' + L.HEADX],
        'CleanTree': ['status', '--porcelain'],
        'Trailer': ['log', '--format=%H', L.BASE + '..' + L.HEADX],
        'Authority': ['rev-parse', L.MAIN + ':' + L.MAP],
    }
    for check, args in prim.items():
        rc, out = git_in(T, *args)
        log.append({'Seq': len(log), 'Utc': L.utc(), 'Kind': 'primary-git', 'Check': check, 'Argv': ['git'] + args, 'Cwd': T, 'ExitCode': rc,
                    'StdoutB64': base64.b64encode(out.encode('utf-8')).decode('ascii')})
    c1 = L.utc_s()
    en, pc_entry = entrada(T)
    logp = work + '/controller-session.jsonl'
    with open(logp, 'w', encoding='ascii', newline='\n') as fh:
        for e in log:
            fh.write(L.canon(e) + '\n')
    return {'LogPath': logp, 'LogSha256': L.sha256(L.rbytes(logp)), 'Exit': ex, 'ExitProcesses': pc_exit, 'CessionStartUtc': c0,
            'CessionEndUtc': c1, 'Entry': en, 'EntryProcesses': pc_entry, 'Runs': runs}


def recover_runs(logp, env_dir, runs_table):
    """Rebuilds the request Runs (argv + envelope bytes) from the session log; never from the Controller's interpretation."""
    names = dict((L.canon(a), n) for n, a in runs_table)
    out, problems, used = [], [], 0
    lead = 'git --no-replace-objects cat-file blob ' + L.B + ' | python -I - '
    with open(logp, 'rb') as fh:
        for line in fh:
            e = json.loads(line.decode('ascii'))
            if e.get('Kind') != 'shell':
                continue
            cl = e['CommandLine']
            if not cl.startswith(lead):
                problems.append('Seq %d: not the declared execution path' % e['Seq'])
                continue
            argv = cl[len(lead):].split(' ')
            raw = base64.b64decode(e['StdoutB64'])
            if L.sha256(raw) != e['StdoutSha256']:
                problems.append('Seq %d: stdout digest' % e['Seq'])
            name = names.get(L.canon(argv), argv[0] + '-' + str(e['Seq']))
            p = env_dir + '/' + name + '.json'
            L.wbytes(p, raw)
            out.append((name, p, argv))
            used += 1
    return out, {'ToolRunsRecovered': used, 'Problems': problems, 'AllViaDeclaredPath': not problems}


def facts_of(path):
    e = json.loads(L.rbytes(path).decode('ascii'))
    return e['Facts'], e['FactsSha256']


def derive_cv(recovered, deleg, handoff, logp):
    """Simulated Controller verification: every R4 check derived mechanically from the recovered envelopes, contrasted with a primary git read."""
    F, S, A = {}, {}, {}
    for n, p, a in recovered:
        F[n], S[n] = facts_of(p)
        A[n] = a
    prim = {}
    with open(logp, 'rb') as fh:
        for line in fh:
            e = json.loads(line.decode('ascii'))
            if e.get('Kind') == 'primary-git':
                prim[e['Check']] = (e['Argv'], base64.b64decode(e['StdoutB64']).decode('utf-8'))

    def ev(n, check, agrees, extra=''):
        pa = prim.get(check)
        ptxt = ('primaria: ' + ' '.join(pa[0]) + ' -> coincide con los hechos: ' + ('si' if agrees else 'NO')) if pa else 'primaria: no hay orden Git primaria para esta comprobacion'
        return ('instrumento: bmin_facts ' + ' '.join(A[n]) + '; FactsSha256=' + S[n] + '; ' + ptxt + (('; ' + extra) if extra else ''))

    idf = F['identity']
    ident_ok = (idf['AbbrevRef'] == handoff['Branch'] == deleg['ExpectedBranch'] and same_path(idf['Toplevel'], handoff['Worktree'])
                and same_path(handoff['Worktree'], deleg['ExpectedWorktree']) and handoff['BaseSha'] == deleg['BaseSha']
                and all(idf['Ancestry'].values()) and idf['Head'] == handoff['CurrentSha'] == idf['OriginBranch'])
    sc = F['scope']
    scope_ok = (not sc['MalformedEntries']) and all(p['AllowedCoveredBy'] and not p['ForbiddenCoveredBy'] for p in sc['PerPath'])
    ct = F['cleantree']
    clean_ok = ct['StatusPorcelain'] == [] and ct['CheckIgnoreRc'] == 0
    df = F['diff']
    declared = handoff['Worker']['Trailer'].split(':', 1)[1].strip() if handoff['Worker']['Trailer'].lower().startswith('co-authored-by:') else handoff['Worker']['Trailer']
    trailer_ok = all(declared in df['CoAuthoredBy'].get(c, []) for c in df['Commits'])
    ft = F['freetext']
    free_ok = not ft['Matches']
    cm = F['clausemap']
    v = cm.get('Validate') or {}
    mv4 = v.get('MV4') or {}
    mv7 = v.get('MV7') or {}
    defects = [k for k in ('LiteralViolations', 'AddedPresentInEff1', 'EntryFilePresentInEff1', 'EntryFilesOutsideSchema') if mv4.get(k)]
    defects += [k for k in ('PointerIsFirstSentenceS16', 'PointerIsLastSentenceS163') if mv7.get(k) is False]
    auth_ok = cm['Derive']['Eff'] is not None and not defects
    ctr = F['contract']
    contract_ok = all(x['ContainedBy'] for x in ctr['A3']) and all(x['ContainedBy'] for x in ctr['A4']) and not any(ctr['A5Missing'].values()) and ctr['SupersededEqual']
    ho = F['handoff']
    hf = ho.get('Fields') or {}
    handoff_ok = bool(ho['Present'] and ho['ValidJson'] and not ho['MissingRequiredKeys'] and hf.get('TaskId') == deleg['TaskId']
                      and hf.get('DelegationRunId') == deleg['RunId'] and hf.get('Attempt') == deleg['Attempt'])
    # contrasts with the primary git reads
    p_head = prim['Identity'][1].strip()
    p_scope = [x for x in prim['Scope'][1].splitlines() if x]
    p_clean = prim['CleanTree'][1] == ''
    p_commits = sorted(x for x in prim['Trailer'][1].split() if x)
    p_map = prim['Authority'][1].strip()
    res = lambda ok: 'pass' if ok else 'fail'
    nr = lambda why: {'Result': 'not_run', 'Evidence': 'sin senal en el banco r1r4 (' + why + ')'}
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
        'Ci': {'Result': 'not_run', 'RedPart': 'not_applicable', 'Evidence': 'sin corrida de CI en el banco r1r4'},
        'Tests': {'Result': 'not_run', 'RedPart': 'not_applicable', 'Evidence': 'sin TRX de RedSha en el banco r1r4'},
        'Trailer': {'Result': res(trailer_ok), 'Evidence': ev('diff', 'Trailer', p_commits == sorted(df['Commits']), 'declarado=' + declared)},
        'Routing': nr('sin modelo'),
        'FreeText': {'Result': res(free_ok), 'Evidence': ev('freetext', 'FreeText', True, 'coincidencias=%d' % len(ft['Matches']))},
        'Denials': nr('sin eventos ni transcripcion'),
    }
    first = next((c for c in ORDER if checks[c]['Result'] != 'pass'), None)
    disp = set()
    for c in ORDER:
        if checks[c]['Result'] != 'pass':
            disp.add('REWORK' if (c == 'Handoff' and ident_ok) else ('STOP' if c == 'Handoff' else ROW_DISP[c]))
    d = 'STOP' if 'STOP' in disp else ('BLOCKED' if 'BLOCKED' in disp else ('REWORK' if 'REWORK' in disp else 'NONE'))
    cls = {'NONE': 'EXECUTION_VERIFIED', 'REWORK': 'EXECUTION_REWORK_REQUIRED', 'BLOCKED': 'EXECUTION_BLOCKED', 'STOP': 'EXECUTION_BLOCKED'}[d]
    ar = [{'Path': c['Path'], 'Section': c['Section'], 'Class': c['Class'], 'Mode': 'NORMAL', 'RevisionSha': cm['Resolve']['MainSha'], 'Parts': [],
           'EffectiveSha': cm['Derive']['Eff'], 'ClauseMapBlob': v['MV1']['MapBlob'], 'UnitProtocol': 'rackcad-protocol/I62'}
          for c in cm['Resolve']['Citations']]
    cv = {'Schema': 'rackcad-controller-verification/v2', 'TaskId': 'T1', 'RunId': L.CV_RUN, 'DelegationRunId': deleg['RunId'],
          'WorkRunId': handoff['RunId'], 'Attempt': 0, 'VerifiedSha': None,
          'Verifier': {'Role': 'EXECUTION_CONTROLLER',
                       'BindingRef': {'UnitId': 'FX-U1', 'Scope': 'TASK', 'TaskId': 'T1', 'Role': 'EXECUTION_CONTROLLER', 'BindingId': 'B20261012T000000Z-a6b5',
                                      'Sha256': '0' * 64, 'Location': {'Kind': 'TRANSIENT', 'Path': 'banco-r1r4', 'Commit': None, 'Blob': None}},
                       'Actor': {'AdapterId': 'bench-r1r4', 'InstanceId': 'NOT_STARTED', 'InstanceIdSource': 'banco r1r4: Controller simulado, sin modelo', 'Assurance': 'NONE'}},
          'Checks': checks, 'AuthorityResolution': ar, 'Classification': cls, 'Disposition': d, 'FailureClass': first or 'NONE',
          'TriggeredStopConditions': ['S-T1-01'] if not scope_ok else [],
          'Findings': ['Salida sintetica del banco r1r4: Controller simulado por la supervision, derivada mecanicamente de los sobres del instrumento recuperados del registro de sesion.',
                       'AuthorityResolution: Mode y RevisionSha son valores de banco (no derivados de E.3.1); EffectiveSha y ClauseMapBlob salen de los hechos clausemap.'],
          'RecommendedNextAction': 'devolver al Coordinator (banco r1r4)'}
    return cv


def relay_record(T, sess, cv_path, cv_sha):
    host = {'HostLabelHash': L.sha256(b'r1r4-bench-host'), 'HostInstanceHash': 'UNKNOWN', 'HostInstanceState': 'UNOBSERVED', 'Os': platform.platform()}
    return {'Schema': 'rackcad-relay-record/v2', 'TaskId': 'T1', 'RunId': L.CV_RUN, 'Attempt': 0, 'Phase': 'VERIFICATION', 'WindowSeq': 1,
            'PrevRelaySha256': None, 'Host': host, 'Participant': None, 'Exit': sess['Exit'],
            'Cession': {'StartUtc': sess['CessionStartUtc'], 'EndUtc': sess['CessionEndUtc'], 'SessionOperated': False},
            'Outcome': {'Kind': 'COMPLETED', 'LaunchedPid': None, 'ExitCode': 0, 'TerminalEvent': None, 'TreeKilled': False, 'DeathConfirmed': True,
                        'OutputPath': cv_path, 'OutputSha256': cv_sha, 'OutputWrittenUtc': L.utc_s(), 'Acceptance': None},
            'Entry': sess['Entry'], 'RemoteFacts': None, 'RebaseMap': None, 'RoleInvocation': None, 'ReadAudit': None,
            'InputFidelity': {'path': 'bmin_facts.py (objeto Git suelto en T)', 'blob': L.B}, 'Disposition': 'NONE',
            'Notes': ['Registro de relevo del banco r1r4 (sin participante real: Participant null); hechos del relevo para R10.']}


def pipeline(label, T):
    """Tests 2-3 on T: P1, P2 before the VERIFY launch, the VERIFY invocation, recovery of the Runs, controller output, relay, request."""
    work = R4 + '/w-' + label
    L.rmtree(work)
    inp = work + '/in'
    rec = {'Label': label, 'T': T, 'Work': work}
    rec['P1'] = L.p1(T, DECISION)
    rec['P2BeforeVerify'] = L.p2(T, 'pwsh')
    if not rec['P2BeforeVerify']['Launch']:
        rec['Stopped'] = 'P2 failed: no launch'
        return rec, None
    H, deleg, handoff, k = L.build_inputs(T, inp)
    sv = {'delegation.v2.schema.json': L.validate(schema('delegation.v2.schema.json')[0], deleg),
          'worker-handoff.schema.json': L.validate(schema('worker-handoff.schema.json')[0], handoff)}
    closure = L.build_closure(T, inp, H, k)
    s_pre = L.snapshot(T)
    sess = controller_session(T, inp, H, work)
    s_entry = L.snapshot(T)
    recovered, rinfo = recover_runs(sess['LogPath'], work + '/env', sess['Runs'])
    exits = {}
    for n, p, a in recovered:
        e = json.loads(L.rbytes(p).decode('ascii'))
        exits[n] = {'Error': e['Error'], 'FactsSha256': e['FactsSha256'], 'EnvelopeSha256': L.sha256(L.rbytes(p))}
    cv = derive_cv(recovered, deleg, handoff, sess['LogPath'])
    sv['controller-verification.v2.schema.json'] = L.validate(schema('controller-verification.v2.schema.json')[0], cv)
    cv_path = work + '/controller-output.json'
    L.wjson_local(cv_path, cv)
    relay = relay_record(T, sess, cv_path, L.sha256(L.rbytes(cv_path)))
    sv['relay-record.v2.schema.json'] = L.validate(schema('relay-record.v2.schema.json')[0], relay)
    relay_path = work + '/relay-facts.json'
    L.wjson_local(relay_path, relay)
    verification = {'Controller': {'Path': cv_path, 'Sha256': L.sha256(L.rbytes(cv_path))},
                    'Delegation': {'Path': inp + '/deleg.json', 'Sha256': H['deleg.json']},
                    'Handoff': {'Path': inp + '/handoff.json', 'Sha256': H['handoff.json']},
                    'IgnorePath': L.IGN, 'TermsRev': L.MAIN, 'MainSha': L.MAIN}
    req = L.build_request(work + '/request.json', T, recovered, closure, relay_path, gitdir=T, verification=verification)
    rec.update({'Inputs': H, 'SchemaValidation': sv, 'SchemaValid': all(not x for x in sv.values()), 'SchemaBlobs': dict((n, schema(n)[1]) for n in sv),
                'Session': {'LogPath': sess['LogPath'], 'LogSha256': sess['LogSha256'], 'CessionStartUtc': sess['CessionStartUtc'],
                            'CessionEndUtc': sess['CessionEndUtc'], 'Exit': sess['Exit'], 'Entry': sess['Entry'],
                            'ExitProcesses': sess['ExitProcesses'], 'EntryProcesses': sess['EntryProcesses']},
                'ZeroWriteToolRuns': {'Equal': L.snapdiff(s_pre, s_entry) == {}, 'Diff': L.snapdiff(s_pre, s_entry)},
                'Recovery': rinfo, 'Envelopes': exits,
                'ControllerChecks': dict((c, cv['Checks'][c]['Result']) for c in ORDER),
                'Classification': [cv['Classification'], cv['Disposition'], cv['FailureClass']],
                'ControllerOutput': {'Path': cv_path, 'Sha256': L.sha256(L.rbytes(cv_path))},
                'RelayFacts': {'Path': relay_path, 'Sha256': L.sha256(L.rbytes(relay_path))},
                'Closure': {'Path': closure, 'Sha256': L.sha256(L.rbytes(closure))},
                'Request': {'Path': req, 'Sha256': L.sha256(L.rbytes(req))}})
    ctx = {'work': work, 'inp': inp, 'H': H, 'deleg': deleg, 'handoff': handoff, 'cv': cv, 'recovered': recovered, 'closure': closure,
           'relay': relay_path, 'verification': verification, 'request': req, 's_entry': s_entry}
    return rec, ctx


def r4_rows(acc):
    d = L.check_detail(acc, 'R4')
    if not isinstance(d, dict):
        return d
    return [{'Check': r.get('Check'), 'Controller': r.get('Controller'), 'AcceptorExpects': r.get('AcceptorExpects'), 'Status': r.get('Status'),
             'Reasons': r.get('Reasons'), 'Defects': (len(r['Defects']) if isinstance(r.get('Defects'), list) else r.get('Defects'))}
            for r in d.get('Checks', [])]


def brief(acc, rid, n=700):
    d = L.check_detail(acc, rid)
    s = L.canon(d) if d is not None else None
    return s[:n] if s else None


def accept_summary(acc, extra_details=()):
    s = L.strip_private(acc)
    s['R4Rows'] = r4_rows(acc)
    s['MismatchDetails'] = dict((rid, brief(acc, rid)) for rid, st in acc['Checks'].items() if st == 'MISMATCH')
    for rid in extra_details:
        s.setdefault('Details', {})[rid] = brief(acc, rid, 1500)
    return s


def run_accept(T, req, procs=None):
    """Acceptor run by the supervision while the session holds; read-only; process tree confirmed dead before release."""
    hold = L.utc()
    acc = L.run_acceptor(req, snap_repos=list(procs or [T]), proc_paths=procs or [T])
    acc['HoldStartUtc'] = hold
    pc = acc['Processes'] or {}
    acc['ProcessTreeDead'] = not pc.get('Matching') and not pc.get('ChildrenOfGivenParents') and 'Error' not in pc
    acc['ReleaseUtc'] = L.utc() if acc['ProcessTreeDead'] else None
    return acc


def flipped_request(ctx, label, check, new):
    cv = json.loads(json.dumps(ctx['cv']))
    old = cv['Checks'][check]['Result']
    cv['Checks'][check]['Result'] = new
    p = ctx['work'] + '/controller-output-flip-' + label + '.json'
    L.wjson_local(p, cv)
    ver = dict(ctx['verification'])
    ver['Controller'] = {'Path': p, 'Sha256': L.sha256(L.rbytes(p))}
    req = L.build_request(ctx['work'] + '/request-flip-' + label + '.json', ctx['recovered_repo'], ctx['recovered'], ctx['closure'], ctx['relay'],
                          gitdir=ctx['recovered_repo'], verification=ver)
    return req, {'Check': check, 'From': old, 'To': new, 'ControllerOutputSha256': L.sha256(L.rbytes(p))}


def copy_deliverable(src, rel):
    raw = L.rbytes(src)
    txt = raw.decode('utf-8')
    dst = os.path.join(HERE, rel)
    L.ensure_dir(os.path.dirname(dst))
    clean = L.sanitize_str(txt)
    data = raw if clean == txt else clean.encode('utf-8')
    L.wbytes(dst, data)
    return {'Source': src, 'Deliverable': rel, 'ByteIdenticalToSource': clean == txt, 'Sha256': L.sha256(data)}


def main():
    L.ensure_dir(R4)
    pins = L.ensure_pins()
    vers = L.versions()

    # ================================================================ T1: tests 1-4 and 7
    T1 = R4 + '/t1'
    L.clone(T1)
    t1 = {'Schema': 'r1r4-bench/r4-t1', 'Versions': vers, 'Pins': pins,
          'Test1': {'T1': T1, 'Kind': 'fresh git clone --no-hardlinks -c core.autocrlf=false -b fx/u1', 'Head': L.gout(T1, 'rev-parse', 'HEAD'),
                    'HeadIsTip': L.gout(T1, 'rev-parse', 'HEAD') == L.TIP}}
    rec, ctx = pipeline('t1', T1)
    t1['Pipeline'] = rec
    ctx['recovered_repo'] = T1
    s_pre_acc = L.snapshot(T1)
    t1['StateUnchangedBetweenEntryAndAcceptor'] = L.snapdiff(ctx['s_entry'], s_pre_acc) == {}
    acc = run_accept(T1, ctx['request'])
    t1['Test3'] = {'Expected': 'ACCEPT_FACTS, R4 MATCH, no NOT_APPLICABLE, R8 MANUAL', 'Acceptor': accept_summary(acc, ('R3', 'R6'))}
    t1['Test3']['Held'] = (acc['Decision'] == 'ACCEPT_FACTS' and acc['Checks'].get('R4') == 'MATCH' and 'NOT_APPLICABLE' not in acc['Checks'].values()
                           and acc['Checks'].get('R8') == 'MANUAL')
    print('T1 test3', acc['Decision'], acc['Checks'])
    t1['Test7'] = {'ZeroWrite': acc['ZeroWrite'], 'Processes': acc['Processes'], 'ProcessTreeDead': acc['ProcessTreeDead'],
                   'Held': all(z['Equal'] for z in acc['ZeroWrite']) and acc['ProcessTreeDead']}
    # test 4: one Check result flipped
    flips = []
    for lab, chk, new in (('cleantree', 'CleanTree', 'fail'), ('identity', 'Identity', 'pass')):
        req, info = flipped_request(ctx, lab, chk, new)
        a = run_accept(T1, req)
        info.update({'Acceptor': accept_summary(a), 'Held': a['Decision'] == 'REJECT_FACTS' and a['Checks'].get('R4') == 'MISMATCH'
                     and [k for k, v in a['Checks'].items() if v == 'MISMATCH'] == ['R4'],
                     'ZeroWriteAndProcesses': all(z['Equal'] for z in a['ZeroWrite']) and a['ProcessTreeDead']})
        flips.append(info)
        print('T1 flip', lab, a['Decision'], a['Checks'])
    t1['Test4'] = {'Expected': 'R4 MISMATCH (only) and REJECT_FACTS', 'Flips': flips, 'Held': all(f['Held'] for f in flips)}
    # deliverable copies of the T1 inputs (no envelopes: they carry e-mail addresses in CoAuthoredBy)
    t1['DeliverableCopies'] = [copy_deliverable(ctx['request'], 'inputs-t1/request.json'),
                               copy_deliverable(ctx['work'] + '/controller-output.json', 'inputs-t1/controller-output.json'),
                               copy_deliverable(ctx['inp'] + '/deleg.json', 'inputs-t1/deleg.json'),
                               copy_deliverable(ctx['inp'] + '/handoff.json', 'inputs-t1/handoff.json'),
                               copy_deliverable(ctx['relay'], 'inputs-t1/relay-facts.json')]
    L.write_result('r4-t1.json', t1)

    # ================================================================ T3: test 5 (same request and envelopes on another clone with the same SHAs)
    T3 = R4 + '/t3'
    L.clone(T3)
    t3 = {'Schema': 'r1r4-bench/r4-t3', 'Versions': vers, 'T3': T3, 'P1': L.p1(T3, DECISION),
          'SameShas': {'Head': L.gout(T3, 'rev-parse', 'HEAD') == L.gout(T1, 'rev-parse', 'HEAD'),
                       'Refs': L.gout(T3, 'for-each-ref', '--format=%(refname) %(objectname)') == L.gout(T1, 'for-each-ref', '--format=%(refname) %(objectname)')}}
    # 5a: request Repo/GitDir = T3, Runs exactly as recovered on T1
    req5a = L.build_request(R4 + '/w-t1/request-t3-a.json', T3, ctx['recovered'], ctx['closure'], ctx['relay'], gitdir=T3, verification=ctx['verification'])
    a5a = run_accept(T3, req5a, procs=[T3, T1])
    # 5b: request Repo/GitDir = T3 and every argv rewritten to --repo T3 (envelopes still the T1 ones)
    rew = [(n, p, [T3 if (i > 0 and argv[i - 1] == '--repo') else x for i, x in enumerate(argv)]) for n, p, argv in ctx['recovered']]
    req5b = L.build_request(R4 + '/w-t1/request-t3-b.json', T3, rew, ctx['closure'], ctx['relay'], gitdir=T3, verification=ctx['verification'])
    a5b = run_accept(T3, req5b, procs=[T3, T1])
    t3['Test5a'] = {'What': 'Repo = T3, GitObject.GitDir = T3, Runs (argv --repo T1 + envelopes) as recovered on T1', 'Acceptor': accept_summary(a5a, ('R3',)),
                    'Held': a5a['Decision'] == 'REJECT_FACTS'}
    t3['Test5b'] = {'What': 'Repo = T3, GitDir = T3, argv rewritten to --repo T3, envelopes from T1', 'Acceptor': accept_summary(a5b, ('R3',)),
                    'Held': a5b['Decision'] == 'REJECT_FACTS'}
    print('T3 5a', a5a['Decision'], a5a['Checks'])
    print('T3 5b', a5b['Decision'], a5b['Checks'])
    L.write_result('r4-t3.json', t3)

    # ================================================================ test 6: a state change in T between the tool runs and the acceptor
    t6 = {'Schema': 'r1r4-bench/r4-t6', 'Versions': vers}
    for lab, change in (('t6a', 'session-commit'), ('t6b', 'modified-tracked-file')):
        T = R4 + '/' + lab
        L.clone(T)
        rec, c = pipeline(lab, T)
        c['recovered_repo'] = T
        if change == 'session-commit':
            L.wbytes(T + '/docs/automation/evidence/FX-U1-agent/bench-session-commit.txt', b'commit de sesion del banco r1r4\n')
            L.git(T, 'add', 'docs/automation/evidence/FX-U1-agent/bench-session-commit.txt')
            cp = L.git(T, '-c', 'user.name=r1r4-bench', '-c', 'user.email=r1r4-bench', 'commit', '-q', '-m', 'bench: commit de sesion (prueba 6)')
            what = {'Change': 'session commit of a new file (HEAD moves; tree clean)', 'CommitExit': cp.returncode, 'NewHead': L.gout(T, 'rev-parse', 'HEAD')}
        else:
            p = T + '/src/Fixture.Lib/Calculator.cs'
            L.wbytes(p, L.rbytes(p) + b'// banco r1r4: cambio sin commit (prueba 6)\n')
            what = {'Change': 'tracked file modified in the working tree (src/Fixture.Lib/Calculator.cs)'}
        s_now = L.snapshot(T)
        d = L.snapdiff(c['s_entry'], s_now)
        what['StateDiffSinceEntry'] = {'Keys': sorted(d), 'WorktreeFiles': d.get('WorktreeFiles'), 'Head': d.get('Head')}
        a = run_accept(T, c['request'])
        mm = [k for k, v in a['Checks'].items() if v == 'MISMATCH']
        t6[lab] = {'Pipeline': {'P2BeforeVerify': rec['P2BeforeVerify']['Launch'], 'ControllerChecks': rec.get('ControllerChecks'),
                                'Request': rec.get('Request')},
                   'StateChange': what, 'Acceptor': accept_summary(a, ('R3', 'R5', 'R4')), 'MismatchedChecks': mm,
                   'Held': a['Decision'] == 'REJECT_FACTS'}
        print('T6', lab, a['Decision'], a['Checks'])
    L.write_result('r4-t6.json', t6)

    # ================================================================ T2: test 8 (linked worktree of another clone): 2-4 and 7
    C2 = R4 + '/c2'
    T2 = R4 + '/t2wt'
    L.clone(C2, branch='main')
    L.worktree_add(C2, T2, L.BR)
    t2 = {'Schema': 'r1r4-bench/r4-t2', 'Versions': vers,
          'Test1': {'C2': C2, 'T2': T2, 'Kind': 'git worktree add T2 fx/u1 on a fresh --no-hardlinks clone C2 (checked out at main)',
                    'Head': L.gout(T2, 'rev-parse', 'HEAD'), 'HeadIsTip': L.gout(T2, 'rev-parse', 'HEAD') == L.TIP,
                    'GitDir': L.abspath_dir(T2, '--git-dir'), 'CommonDir': L.abspath_dir(T2, '--git-common-dir'),
                    'WorktreeList': L.gout(C2, 'worktree', 'list', '--porcelain').splitlines()}}
    rec, ctx2 = pipeline('t2', T2)
    ctx2['recovered_repo'] = T2
    t2['Pipeline'] = rec
    acc = run_accept(T2, ctx2['request'], procs=[T2])
    t2['Test3'] = {'Acceptor': accept_summary(acc, ('R3',)),
                   'Held': acc['Decision'] == 'ACCEPT_FACTS' and acc['Checks'].get('R4') == 'MATCH' and 'NOT_APPLICABLE' not in acc['Checks'].values()}
    t2['Test7'] = {'ZeroWrite': acc['ZeroWrite'], 'Processes': acc['Processes'], 'ProcessTreeDead': acc['ProcessTreeDead'],
                   'ProcessesOnC2': L.procs_check([C2]), 'Held': all(z['Equal'] for z in acc['ZeroWrite']) and acc['ProcessTreeDead']}
    req, info = flipped_request(ctx2, 'cleantree', 'CleanTree', 'fail')
    a = run_accept(T2, req, procs=[T2])
    info.update({'Acceptor': accept_summary(a), 'Held': a['Decision'] == 'REJECT_FACTS' and [k for k, v in a['Checks'].items() if v == 'MISMATCH'] == ['R4'],
                 'ZeroWriteAndProcesses': all(z['Equal'] for z in a['ZeroWrite']) and a['ProcessTreeDead']})
    t2['Test4'] = info
    print('T2', acc['Decision'], acc['Checks'], 'flip', a['Decision'], a['Checks'])
    L.write_result('r4-t2.json', t2)
    print('done')


if __name__ == '__main__':
    main()
