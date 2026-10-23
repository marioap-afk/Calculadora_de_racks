# r2lib.py - the round-2 preflight runner (run_a7r2.py) as a library for the round-3 preflight (run_a7r3.py; decisiones §96): phases q1-q6
# reused with the round-3 pieces; only the designation format (a7-designation/3), its schema file, the base and the work subdirectory change.
# run_a7r2.py - single entry point of the targeted preflight of the SECOND candidate of I-62 A-7 (round 2; decisiones §94,
# puntos 2, 3 y 5; validacion incremental de §88).
#   python -I run_a7r2.py all [--cand <I-62-A-7.md>] [--reuse q3,q4]
# Phases (results next to this file): q0 environment, identities and reuse by SHA-256; q1 TRX of CurrentSha and RedSha (with
# the BOM); q2 RelayFacts; q3 N4 and nc1 with the round-2 designation and builder; q4 B1-like (EFF^1 differs) with the round-2
# builder; q5 identity of the A6-7 invalidator; q6 compatibility with A-5 and A-6; q7 scope guards (self-test of the new or
# changed conditions, preview on the candidate); then summary.json, preflight.md, a privacy pass and MANIFEST.sha256.
# Run on the second candidate after fix_a7r2_b.py (blob 897d7fca; the first version 76eaba8a is kept as I-62-A-7.run1.md next
# to the copy for CAND-DIFF). Only the tests affected by the round-2 changes are repeated; everything else is reused by SHA-256 from the round-1
# evidence (docs/automation/evidence/I-62-A7/, which this run never writes). Stdlib only. Never runs Codex, claude or any model
# CLI; no network; never writes in the I-62 worktree, the main checkout, the bench, the kit, D:\r62-a6m7\m7-run or the round-1
# evidence: every write goes to this directory or to the work area (temporary clones and copies).
import datetime, difflib, hashlib, importlib.util, json, os, platform, re, shutil, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
R1 = next(d for d in (os.path.dirname(HERE), os.path.join(os.path.dirname(HERE), 'evid')) if os.path.isfile(os.path.join(d, 'run_a7.py')))
ROOT = os.path.dirname(HERE)
WORK = os.path.join(ROOT, 'work')
W = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
BENCH = 'D:/r62-a6m7/bench'
BASE = '5da2007f45160ced37b958a3a772c46ac64874ee'   # round 3 (r2lib)
PREV_BLOB = '6c29747d61c5f689a195095ad3712e6d9dba62aa'
CAND_BLOB = '8a528133c7cccd83356e31f8fd79cd2ebb0cec15'      # second candidate after fix_a7r2_c.py (AP 16.8; undetermined RED)
CAND_BLOB_RUN2 = '897d7fca742e1f9f60376be764a05c098f3afe55'  # after fix_a7r2_b.py (observations 1-3 of the first preflight)
CAND_BLOB_RUN1 = '76eaba8a81bf55a458749ff112a7c440bbd3de53'  # second candidate as first published by make_a7r2.py
INSTR = '9a829b0eeb1210238052b1feac06cfa8f7964aed'
ACC = '434ef1fccb0d15762b30244b4a60928120698411'
REQUEST_SCHEMA = 'rackcad-bmin-accept-request/0-experimental'
GREEN_TRX_BLOB = '40295387f9b489d2bf7ff4e517adc2d99c0d35ff'
RED_TRX_PATH = 'artifacts/ci/R37949800001/fixture-tests.trx'
CUR_TRX_PATH = 'artifacts/ci/R37949808974/fixture-tests.trx'
CASES = ['V', 'N4', 'NC1', 'NC2', 'N5', 'N6', 'N7A', 'N7B', 'M']
BENCH_RELAY = {'V': None, 'N4': 's-n4/relay-facts.json', 'NC1': 's-nc1/relay-facts-V1.json', 'NC2': None, 'N5': 's-n5/relay-facts.json',
               'N6': 's-n6/relay-facts.json', 'N7A': 's-n7a/relay-facts.json', 'N7B': 's-n7b/relay-facts.json', 'M': None}
R1_REUSE = {'derive_r4.py': '3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426',
            'make_eic_verify.py': '58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2',
            'designate_m7.py': '491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6',
            'lib_r1r4.py': 'c3995a1cd377822593837c6e34eff30425de61da73b1928d5a0460ce93bfa8c5'}


def _load(name, d=HERE, alias=None):
    s = importlib.util.spec_from_file_location(alias or name, os.path.join(d, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


R1M = _load('run_a7', R1, 'run_a7_r1')       # round-1 runner: helpers reused by identity (sanitize, mkcommit, run_declared, ...)
MEV = _load('make_eic_verify')
A7B = _load('make_eic_a7')
CHK = _load('check_a7_closure')
DSA = _load('designate_a7')
DSM = _load('designate_m7')
DV = _load('derive_r4')
CIT = _load('check_citation_blobs')
LIB = _load('lib_r1r4')
sha256, rb, wb, fwd, git, gtext, genv = R1M.sha256, R1M.rb, R1M.wb, R1M.fwd, R1M.git, R1M.gtext, R1M.genv
case, jdump, sanitize, rmtree, mkcommit, jbytes, deep_replace = R1M.case, R1M.jdump, R1M.sanitize, R1M.rmtree, R1M.mkcommit, R1M.jbytes, R1M.deep_replace
CAND = None


def blob_id(b):
    return DV.blob_id(b)


def vb(v):
    return R1M.verdict_brief(v) if 'NotApplicableRoles' not in v else dict(R1M.verdict_brief(v), RedRequired=v.get('RedRequired'),
                                                                            NotApplicableRoles=v.get('NotApplicableRoles'))


def build(des, legacy=False, red_rule='16.8'):
    try:
        E, rep = A7B.build(des, 'no-entry', True, legacy, red_rule)
        return E, rep, None
    except A7B.BuildError as e:
        return None, e.rep, {'Code': e.code, 'Detail': e.detail}


# ------------------------------------------------------------------ synthetic inputs (deterministic)
def red_trx_bytes(green):
    """TRX of the RedSha run: the green TRX of the kit (UTF-8 with BOM, LF) plus a failed result of the ExpectRed test; the BOM
    and the end of line are kept, so the blob is computed on bytes that start with EF BB BF."""
    assert green.startswith(b'\xef\xbb\xbf')
    t = green[3:].decode('utf-8')
    tid, eid = 'b0c4a2e0-5a7e-4c1d-9e21-7a0000000001', 'b0c4a2e0-5a7e-4c1d-9e21-7a0000000002'
    t = t.replace('3619581b-60b0-429d-91f7-089b4c713498', 'b0c4a2e0-5a7e-4c1d-9e21-7a0000000000')
    t = t.replace('2026-10-09 15:11:23', '2026-10-09 14:55:02').replace('2026-10-09_15_11_23', '2026-10-09_14_55_02')
    res = ('    <UnitTestResult executionId="%s" testId="%s" testName="Fixture.Tests.CalculatorTests.SubtractReturnsDifference" '
           'computerName="runnervmmprz5" duration="00:00:00.0101010" startTime="2026-10-09T14:55:01.5000000+00:00" '
           'endTime="2026-10-09T14:55:01.5101010+00:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Failed" '
           'testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="%s">\n      <Output>\n        <ErrorInfo>\n'
           '          <Message>Assert.Equal() Failure (RED sintetico del preflight de A-7 r2: Subtract no existe todavia)</Message>\n'
           '        </ErrorInfo>\n      </Output>\n    </UnitTestResult>\n') % (eid, tid, eid)
    t = t.replace('  </Results>', res + '  </Results>')
    ut = ('    <UnitTest name="Fixture.Tests.CalculatorTests.SubtractReturnsDifference" storage="/home/runner/work/rackcad-i62-fixture/'
          'rackcad-i62-fixture/tests/fixture.tests/bin/release/net8.0/fixture.tests.dll" id="%s">\n      <Execution id="%s" />\n'
          '      <TestMethod codeBase="/home/runner/work/rackcad-i62-fixture/rackcad-i62-fixture/tests/Fixture.Tests/bin/Release/net8.0/'
          'Fixture.Tests.dll" adapterTypeName="executor://xunit/VsTestRunner2/netcoreapp" className="Fixture.Tests.CalculatorTests" '
          'name="SubtractReturnsDifference" />\n    </UnitTest>\n') % (tid, eid)
    t = t.replace('  </TestDefinitions>', ut + '  </TestDefinitions>')
    te = '    <TestEntry testId="%s" executionId="%s" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" />\n' % (tid, eid)
    t = t.replace('  </TestEntries>', te + '  </TestEntries>')
    t = t.replace('<ResultSummary outcome="Completed">', '<ResultSummary outcome="Failed">')
    t = t.replace('total="1" executed="1" passed="1" failed="0"', 'total="2" executed="2" passed="1" failed="1"')
    return b'\xef\xbb\xbf' + t.encode('utf-8')


def relay_facts(case_id, run_id, current, red, origin_main, cv_name, red_sha):
    """Relay facts of one VERIFY invocation (relay-record/v2 of the FX-02 kit): Termination (Outcome COMPLETED, operation 7),
    RemoteFacts (CurrentSha run and, with RED, the RedSha run; their TRX artifacts), Routing (effective model and effort) and
    Denials (none) in Notes. Synthetic values; Participant null as in the bench."""
    trx_cur = {'RunRef': 'gh-37949808974', 'Name': 'fixture-tests', 'Sha256': '4ccdf2cecba561259b877a471d577f1afa6ac4a9ad00ceb4fa0417e7c61a7513',
               'Selected': 1, 'Passed': 1, 'Failed': 0, 'Skipped': 0, 'FailedTests': []}
    return {'Schema': 'rackcad-relay-record/v2', 'TaskId': 'T1', 'RunId': run_id, 'Attempt': 0, 'Phase': 'VERIFICATION', 'WindowSeq': 1,
            'PrevRelaySha256': None,
            'Host': {'HostLabelHash': sha256(b'a7r2-preflight-host'), 'HostInstanceHash': 'UNKNOWN', 'HostInstanceState': 'UNOBSERVED',
                     'Os': 'synthetic'},
            'Participant': None,
            'Exit': {'HeadSha': current, 'LsRemoteSha': current, 'OriginMainSha': origin_main, 'CleanTree': True, 'GitOperationInProgress': False,
                     'SummaryCommitSha': current, 'Fingerprint': {'Kind': 'NONE', 'Sha256': None, 'KeyNames': [], 'AcceptanceDecisionRef': None},
                     'Processes': [], 'OpenDelegations': 1, 'DelegationStatus': 'ACCEPTED_OPEN'},
            'Cession': {'StartUtc': '2026-10-19T09:00:00Z', 'EndUtc': '2026-10-19T09:05:00Z', 'SessionOperated': False},
            'Outcome': {'Kind': 'COMPLETED', 'LaunchedPid': 4242, 'ExitCode': 0, 'TerminalEvent': 'turn.completed', 'TreeKilled': False,
                        'DeathConfirmed': True, 'OutputPath': 'artifacts/orchestration/FX-U1/T1/0/%s/%s' % (run_id, cv_name),
                        'OutputSha256': sha256(('controller-output ' + case_id).encode('ascii')), 'OutputWrittenUtc': '2026-10-19T09:05:00Z',
                        'Acceptance': None},
            'Entry': {'HeadSha': current, 'LsRemoteSha': current, 'CleanTree': True,
                      'Fingerprint': {'Kind': 'NONE', 'Sha256': None, 'KeyNames': [], 'AcceptanceDecisionRef': None}, 'FingerprintChanged': False,
                      'Processes': []},
            'RemoteFacts': {'CurrentRun': {'GhRunId': 37949808974, 'Event': 'push', 'Ref': 'refs/heads/fx/u1', 'HeadSha': current,
                                           'Status': 'completed', 'Conclusion': 'success',
                                           'Jobs': [{'Name': n, 'Conclusion': 'success'} for n in ('fixture-build', 'fixture-tests', 'core', 'docs')]},
                            'RedRun': ({'GhRunId': 37949800001, 'Event': 'push', 'Ref': 'refs/heads/fx/u1', 'HeadSha': red, 'Status': 'completed',
                                        'Conclusion': 'failure', 'Jobs': [{'Name': 'fixture-build', 'Conclusion': 'success'},
                                                                          {'Name': 'fixture-tests', 'Conclusion': 'failure'}]} if red else None),
                            'TestArtifacts': [trx_cur] + ([{'RunRef': 'gh-37949800001', 'Name': 'fixture-tests', 'Sha256': red_sha, 'Selected': 2,
                                                            'Passed': 1, 'Failed': 1, 'Skipped': 0,
                                                            'FailedTests': ['Fixture.Tests.CalculatorTests.SubtractReturnsDifference']}] if red else []),
                            'OriginMainSha': origin_main, 'LsRemoteSha': current,
                            'Commands': ['git ls-remote origin refs/heads/fx/u1', 'git rev-parse refs/remotes/origin/main',
                                         'gh run view (synthetic)'],
                            'ObtainedUtc': '2026-10-19T09:05:30Z', 'Diagnosis': None},
            'RebaseMap': None, 'RoleInvocation': None, 'ReadAudit': {'Coverage': 'RECORDED', 'ReadsOutsideClosure': []},
            'InputFidelity': {'path': 'bmin_facts.py (objeto Git suelto en el -C)', 'blob': INSTR}, 'Disposition': 'NONE',
            'Notes': ['Termination: Outcome COMPLETED y operacion 7 del descriptor codex-cli registrada (sintetico, preflight de A-7 r2).',
                      'Routing: modelo y effort efectivos = solicitados (RoutingEnforcement required; sintetico).',
                      'Denials: ninguna denegacion ni orden fallida pendiente (sintetico).']}


def designation_r2(case_id, bench_des, red_trx, relay):
    """a7-designation/2 of a bench invocation: the four roles of the bench (TRX -> TRX_CURRENT) with their registered blobs, plus
    TRX_RED (the delegation requires RED) and RELAY (the relay facts of the invocation, when it has them)."""
    d = json.loads(json.dumps(bench_des))
    arts = []
    for i in d.pop('Inputs'):
        r = {'TRX': 'TRX_CURRENT'}.get(i['Role'], i['Role'])
        if i.get('Custody'):
            blob = blob_id(rb(i['Custody'])) if os.path.isfile(i['Custody']) else None
        else:
            blob = i.get('Blob')
        if blob is None and r == 'H':
            blob = 'ffffffffffffffffffffffffffffffffffffffff'   # N4: the handoff of the control is absent; the blob registered for it never verifies
        arts.append({'Role': r, 'Path': i['Path'], 'Blob': blob, 'Custody': i.get('Custody')})
    arts.append({'Role': 'TRX_RED', 'Path': RED_TRX_PATH, 'Blob': blob_id(rb(red_trx)), 'Custody': fwd(red_trx)})
    if relay:
        arts.append({'Role': 'RELAY', 'Path': 'artifacts/orchestration/%s/%s/%s/%s/relay-facts.json' % (
            d['Invocation']['UnitId'], d['Invocation']['TaskId'], d['Invocation']['Attempt'], d['Invocation']['RunId']),
            'Blob': blob_id(rb(relay)), 'Custody': fwd(relay)})
    d['Schema'] = 'a7-designation/3'
    d['Invocation']['Kind'] = 'MEASUREMENT' if case_id == 'M' else 'VERIFY'   # A7-1, point 1 (c), after fix_a7r2_b.py
    d['Artifacts'] = arts
    d.pop('Case', None)
    return dict({'Schema': d.pop('Schema'), 'Case': case_id}, **d)


def schema_ok(obj, schema):
    """a7-designation/2 or relay-record/v2 against its JSON schema, with the JSON-schema subset validator of the bench."""
    s = dict(schema)
    s.pop('$schema', None)
    s.pop('$comment', None)
    return LIB.validate(s, obj)


def prep():
    """Inputs of round 2 (inputs/, designations/): RED TRX, relay facts, the a7-designation/2 of the bench invocations."""
    inp = os.path.join(HERE, 'inputs')
    dsd = os.path.join(HERE, 'designations')
    rmtree(inp)
    rmtree(dsd)
    green = rb(BENCH + '/work/in-v/fixture-tests.trx')
    red = wb(os.path.join(inp, 'fixture-tests-red.trx'), red_trx_bytes(green))
    rschema = json.loads(git(W, 'cat-file', 'blob', '%s:docs/automation/agent-execution/schemas/relay-record.v2.schema.json' % BASE)[1].decode('utf-8'))
    dschema = json.loads(rb(os.path.join(HERE, 'a7-designation-3.schema.json')).decode('utf-8'))
    G, RED, MAIN = 'b417d36cb820a27c78d6c7d50c234460469c89f8', '53436cb20adf1fe87dc91b0382dd3123a1fac44f', 'fbe25347799e5b801ef708454212335537448bd1'
    out = {'RedTrx': fwd(red), 'Relay': {}, 'Designations': {}, 'SchemaViolations': {}}
    for c in CASES:
        bd = json.loads(rb(BENCH + '/designations/%s.json' % c).decode('utf-8'))
        if BENCH_RELAY[c]:
            relay = BENCH + '/work/' + BENCH_RELAY[c]
        elif c == 'M':
            relay = None
        else:
            rf = relay_facts(c, bd['Invocation']['RunId'], G, RED, MAIN, 'controller-output.json', sha256(rb(red)))
            relay = wb(os.path.join(inp, 'relay-facts-%s.json' % c), jbytes(rf))
            out['SchemaViolations']['relay-facts-%s.json' % c] = schema_ok(rf, rschema)
        des = designation_r2(c, bd, red, relay)
        p = jdump(os.path.join(dsd, '%s.json' % c), des)
        out['Designations'][c] = {'Path': fwd(p), 'Sha256': sha256(rb(p))}
        out['SchemaViolations']['designations/%s.json' % c] = schema_ok(des, dschema)
        if relay:
            out['Relay'][c] = {'Path': fwd(relay), 'Sha256': sha256(rb(relay))}
    return out


# ------------------------------------------------------------------ shared helpers
def out_dir(name):
    d = os.path.join(WORK, 'r3', name)
    rmtree(d)
    os.makedirs(d)
    return d


def derive(des, Ep, out, label):
    """designate_a7 (round 2) then derive_r4 (unchanged)."""
    E = json.loads(rb(Ep).decode('utf-8'))
    chk = DSA.check(des, E)
    if not chk['Ok']:
        return {'Designation': chk['Problems']}
    vec = dict(chk['Vector'], E=fwd(Ep))
    jdump(os.path.join(out, 'vector-%s.json' % label), vec)
    res = DV.derive_vector(vec)
    cp = jdump(os.path.join(out, 'closure-%s.json' % label), res['Closure'])
    return {'Path': cp, 'Sha256': sha256(rb(cp)), 'Closure': res['Closure'], 'Omitted': [(o['Term'], o['Cause']) for o in res['Omitted']],
            'Designation': 'Ok', 'Names': chk['Names']}


def ensure_pin():
    pin = os.path.join(WORK, 'pin')
    acc = os.path.join(pin, 'bmin_accept.py')
    if not os.path.isfile(acc) or blob_id(rb(acc)) != ACC:
        wb(acc, git(W, 'cat-file', 'blob', ACC)[1])
    assert blob_id(rb(acc)) == ACC
    return pin, acc


def accept_req(req, out, label, snap=()):
    pin, acc = ensure_pin()
    reqp = wb(os.path.join(out, 'request-%s.json' % label), jbytes(req))
    s0 = [R1M.snapshot(r) for r in snap]
    p = subprocess.run([sys.executable, '-I', acc, '--request', reqp], capture_output=True, cwd=pin, env=genv(), stdin=subprocess.DEVNULL, timeout=3600)
    s1 = [R1M.snapshot(r) for r in snap]
    res = json.loads(p.stdout.decode('ascii'))
    wb(os.path.join(out, 'acceptor-%s.json' % label), p.stdout)
    checks = dict((c.get('Id'), c.get('Status')) for c in res.get('Checks', []))
    det = dict((c.get('Id'), c.get('Detail')) for c in res.get('Checks', []))
    viol = []
    r9 = det.get('R9')
    for run in ((r9 or {}).get('Runs', []) if isinstance(r9, dict) else []):
        viol += ['%s: %s' % (run.get('Name'), x) for x in (run.get('Violations') or [])]
    return {'Rc': p.returncode, 'Decision': res.get('Decision'), 'Checks': checks, 'R9Violations': viol[:10], 'R9ViolationCount': len(viol), 'R10': det.get('R10'),
            'ZeroWrite': s0 == s1, 'StdoutSha256': sha256(p.stdout)}


def minimal_request(T, runs, closure, relay=None, extra_na=None):
    """Minimal data request of the preflight (it is not the acceptance of a Controller output): no Verification, so R4 is
    declared not applicable with the A6-3 rule 6 (c) cause; R5-R7 by their data condition; R10 only from RelayFacts."""
    cmds = set(r['Argv'][0] for r in runs)
    na = {'R4': 'A6-3 rule 6 (c): minimal data request of the A-7 r2 preflight, without Verification'}
    if not cmds & {'identity', 'diff', 'scope', 'hashes'}:
        na['R5'] = 'data condition: Runs has no identity/diff/scope/hashes'
    if 'clausemap' not in cmds:
        na['R6'] = 'data condition: Runs has no clausemap and there is no Verification'
    if not cmds & {'scope', 'contract', 'clausemap'}:
        na['R7'] = 'data condition: Runs has no scope/contract/clausemap'
    na.update(extra_na or {})
    req = {'Schema': REQUEST_SCHEMA, 'Repo': fwd(T), 'Tool': {'PinnedBlob': INSTR, 'RunFrom': 'object', 'GitObject': {'GitDir': fwd(T), 'Blob': INSTR}},
           'Interpreter': {'Python': platform.python_version(), 'ExecutableSha256': R1M.exe_sha256()},
           'Runs': runs, 'Closure': {'Path': fwd(closure['Path']), 'Sha256': closure['Sha256']}, 'NotApplicable': na}
    if relay:
        req['RelayFacts'] = {'Path': fwd(relay[0]), 'CitedSha256': relay[1]}
    return req


def instr_run(T, out, name, argv):
    rc, so, se, inner = R1M.run_declared(T, argv)
    envp = wb(os.path.join(out, 'env', name + '.json'), so)
    env = json.loads(so.decode('ascii'))
    return {'Name': name, 'Envelope': fwd(envp), 'Argv': argv}, rc, env


def files_of(cl):
    return sorted((f['Name'], f['Sha256'][:12]) for f in cl['Closure']['Files'])


# ------------------------------------------------------------------ q0 environment, identities, reuse
def q0(P):
    cases = []
    head = gtext(W, 'rev-parse', 'HEAD')
    cases.append(case('ENV-W', 'HEAD del worktree de I-62 (solo lectura)', BASE, head, head == BASE))
    cb = rb(CAND)
    cases.append(case('CAND', 'candidata A-7 ronda 2 (tras fix_a7r2_b.py y fix_a7r2_c.py) leída por completo', 'blob ' + CAND_BLOB,
                      'blob %s, %d bytes, %d líneas, LF' % (blob_id(cb), len(cb), cb.count(b'\n')), blob_id(cb) == CAND_BLOB and b'\r' not in cb))
    d = os.path.dirname(CAND)
    steps = [('CAND-DIFF-B', 'I-62-A-7.run1.md', CAND_BLOB_RUN1, 'I-62-A-7.run2.md', CAND_BLOB_RUN2, 'fix_a7r2_b.py',
              (('si la entrega exige RED (16.9, `Ci` y `Tests`)', 'punto 1 (c)'), ('la implementación de la derivación del cierre del aceptante', 'punto 7'),
               ('| **(c) Artefactos custodiados**', 'fila (c) de §3.3'))),
             ('CAND-DIFF-C', 'I-62-A-7.run2.md', CAND_BLOB_RUN2, None, CAND_BLOB, 'fix_a7r2_c.py',
              (('| L16 | AP 16.8 (RED de la cadena); AP 16.9 | 616-617;', 'fila L16 de §2'), ('si la entrega exige RED (16.8; 16.9, `Ci` y `Tests`)', 'punto 1 (c)'),
               ('si la exigencia de RED de 16.8 no se puede determinar porque', 'punto 5 (c)')))]
    for cid, fa, ba, fb, bb, fix, marks in steps:
        pa, pb = os.path.join(d, fa), (os.path.join(d, fb) if fb else CAND)
        if not (os.path.isfile(pa) and blob_id(rb(pa)) == ba and os.path.isfile(pb) and blob_id(rb(pb)) == bb):
            cases.append(case(cid, 'versiones %s… → %s…' % (ba[:8], bb[:8]), 'copias disponibles', 'no disponibles', False))
            continue
        a, b = rb(pa).decode('utf-8').split('\n'), rb(pb).decode('utf-8').split('\n')
        hunks = [(i1 + 1, i2, j1 + 1, j2) for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes() if tag != 'equal']
        where, found = [], []
        for i1, i2, j1, j2 in hunks:
            new = '\n'.join(b[j1 - 1:j2])
            k = next((n for n, (m, _) in enumerate(marks) if m in new), None)
            found.append(k)
            where.append('L%d-L%d → L%d-L%d (%s)' % (i1, i2, j1, j2, marks[k][1] if k is not None else 'otro'))
        cases.append(case(cid, 'candidata %s… frente a %s… (%s)' % (bb[:8], ba[:8], fix), 'solo las tres frases de %s: %s' % (fix, ', '.join(l for _, l in marks)),
                          '%d tramos: %s' % (len(hunks), '; '.join(where)), found == [0, 1, 2]))
    pub = gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-7.md')
    cases.append(case('PREV', 'ronda 1 en la base (la que la ronda 2 corrige en el mismo archivo)', PREV_BLOB, pub, pub == PREV_BLOB))
    rc, out = git(W, 'ls-tree', '-r', '-z', 'HEAD', 'docs/automation/evidence/I-62-A7/')
    pubfiles = {}
    for rec in out.split(b'\0'):
        if rec:
            meta, name = rec.split(b'\t', 1)
            pubfiles[name.decode('utf-8')[len('docs/automation/evidence/I-62-A7/'):]] = meta.split()[2].decode('ascii')
    same = [n for n, b in pubfiles.items() if os.path.isfile(os.path.join(R1, n)) and blob_id(rb(os.path.join(R1, n))) == b]
    only_pub = sorted(n for n in pubfiles if not os.path.isfile(os.path.join(R1, n)))
    cases.append(case('R1-EVID', 'evidencia de la ronda 1 publicada (docs/automation/evidence/I-62-A7/, %d archivos) frente a la local reutilizada' % len(pubfiles),
                      'idéntica byte a byte (la ronda 2 no la toca; el resultado de la guarda run lo añadió el Coordinator)',
                      'iguales %d; solo en la publicación: %s; ningún archivo de r2/ publicado todavía: %s' % (
                          len(same), ', '.join(only_pub) or '-', 'sí' if not any(n.startswith('r2/') for n in pubfiles) else 'no'),
                      len(same) + len(only_pub) == len(pubfiles) and only_pub == ['a7-guards-result-run.json']))
    for name, want in sorted(R1_REUSE.items()):
        got = sha256(rb(os.path.join(HERE, name)))
        cases.append(case('REUSE-' + name, 'copia byte a byte reutilizada por identidad', 'SHA-256 ' + want, 'SHA-256 ' + got, got == want))
    for blob, label in ((INSTR, 'instrumento'), (ACC, 'aceptante')):
        rc, b = git(W, 'cat-file', 'blob', blob)
        cases.append(case('PIN-' + label, '%s desde su objeto Git, sin modificar' % label, 'blob ' + blob, 'blob recalculado %s' % blob_id(b), blob_id(b) == blob))
    sv = dict((k, v) for k, v in P['SchemaViolations'].items() if v)
    known = dict((k, v) for k, v in sv.items() if k.startswith('relay-facts-') and v == ['#/RunId pattern'])
    cases.append(case('INPUTS', 'entradas de la ronda 2: designaciones a7-designation/2 (esquema a7-designation-3.schema.json) y hechos del relevo sintéticos '
                      '(relay-record/v2 del repositorio de I-62)', 'todas válidas contra su esquema (validador del subconjunto de lib_r1r4.py)',
                      '%d archivos; con violaciones: %s%s' % (len(P['SchemaViolations']), ', '.join(sv) or 'ninguno', ' (solo el patrón de RunId de relay-record/v2 frente a los RunId de los controles del banco, ya conocido: decisiones §92, punto 6)' if sv and sv == known else ''), sv == known))
    return {'Phase': 'q0-env', 'Cases': cases, 'Prep': P}


# ------------------------------------------------------------------ q1 TRX and q2 RelayFacts (shared invocation V on a copy)
def q12(P):
    c1, c2 = [], []
    out = out_dir('q12')
    T = os.path.join(WORK, 'deliv')
    rc, oid = git(T, 'hash-object', '-w', '--no-filters', '--stdin', inp=git(W, 'cat-file', 'blob', INSTR)[1])
    assert oid.decode().strip() == INSTR
    base = json.loads(rb(os.path.join(HERE, 'designations', 'V.json')).decode('utf-8'))
    base['Repo'] = fwd(T)
    role = lambda d, r: next(a for a in d['Artifacts'] if a['Role'] == r)
    cur, red, relay = role(base, 'TRX_CURRENT'), role(base, 'TRX_RED'), role(base, 'RELAY')

    def variant(name, f):
        d = json.loads(json.dumps(base))
        f(d)
        return d
    # ---- positive: two TRX entries
    E, rep, err = build(base)
    Ep = wb(os.path.join(out, 'E-pos.json'), MEV.serialize(E))
    v = vb(CHK.check(base, E, i62_repo=W, i62_commit=BASE))
    trx = [e for e in E['CanonicalInputs'] if e['Path'].endswith('.trx')]
    cl = derive(base, Ep, out, 'pos')
    c1.append(case('TRX-POS', 'la entrega exige RED (16.8: ChainRedSha nulo en la delegación del banco): TRX de la corrida de CurrentSha y TRX de la de RedSha '
                   'designados con su papel, ruta y blob', 'dos entradas TRX en el cierre; validador acepta; designate_a7 válida; C6 con (TRX, sha) de cada una',
                   'RedRequired %s (%s); entradas TRX: %s; validador: %s; C6: %s' % (
                       rep['RedRequired'], rep.get('RedBasis'), ', '.join('%s %s' % (e['Path'], e['Blob'][:8]) for e in trx),
                       'acepta' if v['Accepted'] else 'rechaza %s' % v, files_of(cl) if 'Closure' in cl else cl),
                   rep['RedRequired'] is True and v['RedRequired'] is True and len(trx) == 2 and v['Accepted'] and 'Closure' in cl and
                   sum(1 for f in cl['Closure']['Files'] if f['Name'] == 'TRX') == 2, Entries=trx, Closure=cl.get('Sha256')))
    # instrument runs (declared form): both designated TRX, a third TRX, the TRX without its BOM
    green, redb = rb(cur['Custody']), rb(red['Custody'])
    third = wb(os.path.join(out, 'third.trx'), redb.replace(b'14:55:02', b'13:13:13'))
    nobom = wb(os.path.join(out, 'fixture-tests-sin-bom.trx'), green[3:])
    runs = {}
    for name, fp in (('trx-current', cur['Custody']), ('trx-red', red['Custody']), ('trx-third', third), ('trx-nobom', nobom)):
        runs[name], rcx, env = instr_run(T, out, name, ['trx', '--repo', fwd(T), '--file', 'TRX=' + fwd(fp), '--sha256', 'TRX=' + sha256(rb(fp)),
                                                       '--filter-substring', 'Subtract'])
        runs[name + ':facts'] = (rcx, env.get('Facts'), env.get('Error'))
    relay_t = (relay['Custody'], sha256(rb(relay['Custody'])))
    a_pos = accept_req(minimal_request(T, [runs['trx-current'], runs['trx-red']], cl, relay_t), out, 'pos', [T])
    c1.append(case('TRX-POS-R9', 'aceptante fijado sobre dos corridas `trx --file TRX=` (los dos TRX designados), con el cierre derivado',
                   'R9 MATCH (el cierre fija los dos TRX en C6); R10 MATCH (RelayFacts = la entrada RELAY de (c)); ACCEPT_FACTS',
                   '%s rc %d; R9 %s; R10 %s; cero escrituras: %s' % (a_pos['Decision'], a_pos['Rc'], a_pos['Checks'].get('R9'), a_pos['Checks'].get('R10'),
                                                                    'sí' if a_pos['ZeroWrite'] else 'no'),
                   a_pos['Decision'] == 'ACCEPT_FACTS' and a_pos['Checks'].get('R9') == 'MATCH' and a_pos['Checks'].get('R10') == 'MATCH', Acceptor=a_pos))
    rcf, facts, ferr = runs['trx-red:facts']
    c1.append(case('TRX-RED-FACTS', 'hechos del instrumento sobre el TRX de RedSha', 'la prueba con ExpectRed entre las fallidas (lo que lee Tests RedPart de 16.9)',
                   'rc %d; FailedTests %s; Sha256 = el de C6: %s' % (rcf, (facts or {}).get('FailedTests'),
                                                                    'sí' if (facts or {}).get('Sha256') in [f['Sha256'] for f in cl['Closure']['Files']] else 'no'),
                   rcf == 0 and (facts or {}).get('FailedTests') == ['Fixture.Tests.CalculatorTests.SubtractReturnsDifference']))
    # ---- RED required, TRX of RedSha absent (file absent; and not designated)
    for nm, f in (('ausente', lambda d: role(d, 'TRX_RED').update({'Custody': fwd(os.path.join(out, 'no-existe.trx'))})),
                  ('no-designado', lambda d: d['Artifacts'].remove(role(d, 'TRX_RED')))):
        d = variant(nm, f)
        E2, rep2, err2 = build(d)
        v2 = vb(CHK.check(d, E2, i62_repo=W, i62_commit=BASE))
        trx2 = [e['Path'] for e in E2['CanonicalInputs'] if e['Path'].endswith('.trx')]
        ab = [a for a in rep2['Absences'] if a['Role'] == 'TRX_RED']
        c1.append(case('TRX-RED-%s' % nm.upper(), 'RED exigido y el TRX de RedSha %s' % ('sin bytes custodiados' if nm == 'ausente' else 'sin designar'),
                       'sin entrada, causa registrada, nada sustituido (ni el TRX de CurrentSha ni otra copia); el validador acepta el cierre',
                       'entradas TRX: %s; ausencia: %s; validador: %s' % (trx2, ab[0]['Cause'] if ab else '-', 'acepta' if v2['Accepted'] else 'rechaza'),
                       trx2 == [CUR_TRX_PATH] and ab and not ab[0]['Substituted'] and v2['Accepted']))
        if nm == 'ausente':
            Ep2 = wb(os.path.join(out, 'E-red-absent.json'), MEV.serialize(E2))
            cl2 = derive(d, Ep2, out, 'red-absent')
            a2 = accept_req(minimal_request(T, [runs['trx-current'], runs['trx-red']], cl2, relay_t), out, 'red-absent', [T])
            c1.append(case('TRX-RED-ABSENT-169', 'clasificación de 16.9 con el TRX de RedSha ausente: la lectura de ese TRX con el cierre derivado',
                           'R9 MISMATCH sobre la corrida del TRX de RedSha (fuera del cierre, P-22): Tests RedPart no puede ser pass (AP 16.9 fila 10, «las '
                           'fallidas del TRX del RedSha»), así que Tests no pasa (REWORK) y nada lo sustituye',
                           '%s; R9 %s (%s); C6 del cierre: %s' % (a2['Decision'], a2['Checks'].get('R9'), '; '.join(a2['R9Violations'][:2]), files_of(cl2)),
                           a2['Checks'].get('R9') == 'MISMATCH' and any('trx-red' in x for x in a2['R9Violations']) and
                           not any('trx-current' in x for x in a2['R9Violations']), Acceptor=a2))
    # ---- whether the delivery requires RED (AP 16.8, RED de la cadena, used by 16.9 Ci and Tests and cited by A7-1, point 1 (c)):
    #      ChainRedSha null, or git diff --name-only BaseSha..CurrentSha touches ChainRedFiles; ExpectRed (always the contract's)
    #      does not decide it. The superseded tool rule of the first run of round 2 (ExpectRed of the delegation) is run on the
    #      same inputs as negative evidence, to show where it diverges.
    D0 = json.loads(rb(role(base, 'DELEG')['Custody']).decode('utf-8'))
    RED_SHA = '53436cb20adf1fe87dc91b0382dd3123a1fac44f'
    NOT_TOUCHED, TOUCHED = 'tests/Fixture.Tests/Fixture.Tests.csproj', 'tests/Fixture.Tests/CalculatorTests.cs'
    cur_sha = json.loads(rb(role(base, 'H')['Custody']).decode('utf-8-sig'))['CurrentSha']
    diffn = sorted(x.decode() for x in git(T, 'diff', '--name-only', '-z', '%s..%s' % (D0['BaseSha'], cur_sha))[1].split(b'\0') if x)
    assert NOT_TOUCHED not in diffn and TOUCHED in diffn

    def deleg_variant(tag, chain_red, chain_files, expect_red=None):
        Dx = json.loads(json.dumps(D0))
        Dx['ChainRedSha'], Dx['ChainRedFiles'] = chain_red, chain_files
        if expect_red is not None:
            for t in Dx.get('RequiredTests') or []:
                t['ExpectRed'] = expect_red
        p = wb(os.path.join(out, 'deleg-%s.json' % tag), jbytes(Dx))
        return lambda d: role(d, 'DELEG').update({'Custody': fwd(p), 'Blob': blob_id(rb(p))})

    def with_red_entry(Ex):
        Em = json.loads(json.dumps(Ex))
        if not any(e['Path'] == RED_TRX_PATH for e in Em['CanonicalInputs']):
            Em['CanonicalInputs'].insert(-1, {'Path': RED_TRX_PATH, 'Blob': red['Blob']})
        return Em

    def red_case(d, rule='16.8'):
        Ex, repx, errx = build(d, red_rule=rule)
        vx = vb(CHK.check(d, Ex, i62_repo=W, i62_commit=BASE, red_rule=rule))
        vm = vb(CHK.check(d, with_red_entry(Ex), i62_repo=W, i62_commit=BASE, red_rule=rule))
        ab = [a for a in repx['Absences'] if a['Role'] == 'TRX_RED']
        return {'RedRequired': repx['RedRequired'], 'ValidatorRedRequired': vx['RedRequired'], 'Basis': repx.get('RedBasis'),
                'TrxRedEntry': any(e['Path'] == RED_TRX_PATH for e in Ex['CanonicalInputs']), 'TrxRedAbsence': ab[0]['Cause'] if ab else None,
                'Accepted': vx['Accepted'], 'AcceptedWithRedEntry': vm['Accepted'], 'ForeignWithRedEntry': [f[0] for f in vm['Foreign']]}
    nr = deleg_variant('red-no-exigido', RED_SHA, [NOT_TOUCHED])
    d_nr = variant('nr', nr)
    n16 = red_case(d_nr)
    c1.append(case('TRX-RED-NOT-REQUIRED', 'la entrega no exige RED según 16.8 (ChainRedSha %s… no nulo; git diff --name-only BaseSha..CurrentSha no toca '
                   'ChainRedFiles = [%s]); ExpectRed del contrato sin cambio (true) y la designación nombra un TRX de RedSha' % (RED_SHA[:8], NOT_TOUCHED),
                   'RedRequired false en constructor y validador; el constructor no admite el TRX de RedSha (causa registrada); un cierre que lo incluya es '
                   'rechazado (FOREIGN: no hay ese papel); el cierre sin él se acepta',
                   'RedRequired %s / %s (%s); constructor: %s; validador: %s; con la entrada del TRX de RedSha: %s' % (
                       n16['RedRequired'], n16['ValidatorRedRequired'], n16['Basis'], n16['TrxRedAbsence'] or 'admitido', 'acepta' if n16['Accepted'] else 'rechaza',
                       'acepta' if n16['AcceptedWithRedEntry'] else 'rechaza: FOREIGN %s' % n16['ForeignWithRedEntry']),
                   n16['RedRequired'] is False and n16['ValidatorRedRequired'] is False and (n16['TrxRedAbsence'] or '').startswith('NOT_ADMITTED')
                   and not n16['TrxRedEntry'] and n16['Accepted'] and not n16['AcceptedWithRedEntry'] and RED_TRX_PATH in n16['ForeignWithRedEntry'], Detail=n16))
    d_nr_nd = variant('nr-nd', lambda d: (nr(d), d['Artifacts'].remove(role(d, 'TRX_RED'))))
    E_nd, rep_nd, _ = build(d_nr_nd)
    E_nd_old, rep_nd_old, _ = build(d_nr_nd, red_rule='expectred')
    o16 = red_case(d_nr, 'expectred')
    ab_nd = [a for a in rep_nd['Absences'] if a['Role'] == 'TRX_RED']
    ab_nd_old = [a for a in rep_nd_old['Absences'] if a['Role'] == 'TRX_RED']
    c1.append(case('RED-DIV-EXPECTRED', 'divergencia de la regla anterior de las herramientas (RED si algún RequiredTests[].ExpectRed de la delegación es '
                   'true) frente a 16.8, con las mismas entradas de TRX-RED-NOT-REQUIRED (una delegación de una cadena que ya acreditó un RED: A5 '
                   'obliga a ExpectRed = el del contrato, y 16.8 dice que ExpectRed «es siempre el del contrato»)',
                   'la regla anterior exige RED y admite (y, sin designar, exige) el TRX de RedSha donde 16.8 no lo exige: las dos reglas divergen, y '
                   'la regla 16.8 (la de las herramientas ahora) da la respuesta de 16.8',
                   'regla anterior: RedRequired %s, entrada del TRX de RedSha %s, validador con ella: %s, sin designar: %s; regla 16.8: RedRequired %s, '
                   'sin designar: %s' % (o16['RedRequired'], 'sí' if o16['TrxRedEntry'] else 'no', 'acepta' if o16['AcceptedWithRedEntry'] else 'rechaza',
                                         ab_nd_old[0]['Cause'][:60] if ab_nd_old else 'sin ausencia', n16['RedRequired'],
                                         ab_nd[0]['Cause'][:60] if ab_nd else 'sin ausencia del TRX de RedSha (no se exige)'),
                   o16['RedRequired'] is True and o16['TrxRedEntry'] and o16['AcceptedWithRedEntry'] and ab_nd_old and ab_nd_old[0]['Cause'].startswith('NOT_DESIGNATED')
                   and n16['RedRequired'] is False and not ab_nd, Old=o16))
    d_t = variant('touch', deleg_variant('red-toca', RED_SHA, [TOUCHED]))
    t16, t_old = red_case(d_t), red_case(d_t, 'expectred')
    c1.append(case('RED-169-TOUCH', 'ChainRedSha no nulo y git diff --name-only BaseSha..CurrentSha toca ChainRedFiles = [%s]' % TOUCHED,
                   'la entrega exige RED (16.8): el TRX de RedSha entra; validador acepta; las dos reglas coinciden aquí',
                   'RedRequired %s / %s (%s); entrada del TRX de RedSha: %s; validador: %s; regla anterior: RedRequired %s' % (
                       t16['RedRequired'], t16['ValidatorRedRequired'], t16['Basis'], 'sí' if t16['TrxRedEntry'] else 'no', 'acepta' if t16['Accepted'] else 'rechaza',
                       t_old['RedRequired']),
                   t16['RedRequired'] is True and t16['ValidatorRedRequired'] is True and t16['TrxRedEntry'] and t16['Accepted'] and t_old['RedRequired'] is True))
    d_c = variant('null-noexpect', deleg_variant('red-nulo-sin-expectred', None, [], expect_red=False))
    c16, c_old = red_case(d_c), red_case(d_c, 'expectred')
    c1.append(case('RED-DIV-NULL', 'ChainRedSha nulo y ningún RequiredTests con ExpectRed true (divergencia de principio)',
                   '16.8: la entrega exige RED (ChainRedSha nulo): el TRX de RedSha entra y el validador acepta; la regla anterior diría lo contrario',
                   'regla 16.8: RedRequired %s / %s, entrada %s, validador %s; regla anterior: RedRequired %s, constructor: %s, validador con la entrada: %s' % (
                       c16['RedRequired'], c16['ValidatorRedRequired'], 'sí' if c16['TrxRedEntry'] else 'no', 'acepta' if c16['Accepted'] else 'rechaza',
                       c_old['RedRequired'], (c_old['TrxRedAbsence'] or 'admitido')[:40], 'acepta' if c_old['AcceptedWithRedEntry'] else 'rechaza (FOREIGN)'),
                   c16['RedRequired'] is True and c16['ValidatorRedRequired'] is True and c16['TrxRedEntry'] and c16['Accepted']
                   and c_old['RedRequired'] is False and not c_old['AcceptedWithRedEntry']))
    d_u = variant('undet', lambda d: (nr(d), role(d, 'H').update({'Custody': fwd(os.path.join(out, 'no-existe-handoff.json'))})))
    u16 = red_case(d_u)
    d_un = variant('undet-nd', lambda d: (nr(d), role(d, 'H').update({'Custody': fwd(os.path.join(out, 'no-existe-handoff.json'))}),
                                          d['Artifacts'].remove(role(d, 'TRX_RED'))))
    E_un, rep_un, _ = build(d_un)
    ab_un = [a for a in rep_un['Absences'] if a['Role'] == 'TRX_RED']
    c1.append(case('RED-UNDETERMINED', 'ChainRedSha no nulo y la entrega ausente (sin CurrentSha): la exigencia de RED de 16.8 no se puede determinar; '
                   'A7-1, regla 5 (c), tras fix_a7r2_c.py: «el TRX de la corrida de `RedSha` ni se admite ni se exige, y se registra la causa»',
                   'coincide con el texto: RedRequired indeterminado en constructor y validador; designado, el TRX de RedSha no se admite (causa registrada) '
                   'y un cierre que lo incluya es FOREIGN; sin designar, no se exige (ninguna ausencia por él) y la causa queda registrada (RedBasis); el '
                   'resto del cierre se acepta',
                   'RedRequired %s / %s; designado: %s, validador con la entrada: %s; sin designar: ausencias del TRX de RedSha %d, RedBasis %s; validador: %s' % (
                       u16['RedRequired'], u16['ValidatorRedRequired'], (u16['TrxRedAbsence'] or 'admitido')[:70],
                       'acepta' if u16['AcceptedWithRedEntry'] else 'rechaza: FOREIGN', len(ab_un), (rep_un.get('RedBasis') or '-')[:40],
                       'acepta' if u16['Accepted'] else 'rechaza'),
                   u16['RedRequired'] is None and u16['ValidatorRedRequired'] is None and (u16['TrxRedAbsence'] or '').startswith('NOT_ADMITTED: UNDETERMINED')
                   and u16['Accepted'] and not u16['AcceptedWithRedEntry'] and rep_un['RedRequired'] is None and not ab_un
                   and (rep_un.get('RedBasis') or '').startswith('UNDETERMINED') and not any(e['Path'] == RED_TRX_PATH for e in E_un['CanonicalInputs']),
                   Detail=u16))
    # ---- a third, undesignated TRX
    E4 = json.loads(json.dumps(E))
    E4['CanonicalInputs'].insert(-1, {'Path': 'artifacts/ci/R37949800002/fixture-tests.trx', 'Blob': blob_id(rb(third))})
    v4 = vb(CHK.check(base, E4, i62_repo=W, i62_commit=BASE))
    a4 = accept_req(minimal_request(T, [runs['trx-current'], runs['trx-third']], cl, relay_t), out, 'third', [T])
    c1.append(case('TRX-THIRD', 'un tercer TRX no designado: en el cierre y como lectura', 'validador: FOREIGN; aceptante: R9 MISMATCH sobre su lectura (P-22)',
                   'validador: %s; aceptante: %s, R9 %s (%s)' % ('acepta' if v4['Accepted'] else 'rechaza FOREIGN', a4['Decision'], a4['Checks'].get('R9'),
                                                                 '; '.join(a4['R9Violations'][:1])),
                   (not v4['Accepted']) and a4['Checks'].get('R9') == 'MISMATCH' and any('trx-third' in x for x in a4['R9Violations'])))
    # ---- replaced TRX (other bytes, the registered blob unchanged)
    d = variant('repl', lambda d: role(d, 'TRX_RED').update({'Custody': fwd(third)}))
    E5, rep5, err5 = build(d)
    ab = [a for a in rep5['Absences'] if a['Role'] == 'TRX_RED']
    c1.append(case('TRX-REPLACED', 'el TRX de RedSha sustituido por otros bytes (el blob registrado no cambia)', 'sin entrada (BLOB_MISMATCH), nada sustituido',
                   'causa: %s; entradas TRX: %s' % (ab[0]['Cause'] if ab else '-', [e['Path'] for e in E5['CanonicalInputs'] if e['Path'].endswith('.trx')]),
                   ab and ab[0]['Cause'].startswith('BLOB_MISMATCH') and not any(e['Path'] == RED_TRX_PATH for e in E5['CanonicalInputs'])))
    # ---- BOM: fidelity of the bytes
    bom = green[:3] == b'\xef\xbb\xbf'
    c1.append(case('BOM-POS', 'el TRX del banco empieza con un BOM UTF-8: su blob se calcula sobre los bytes con el BOM',
                   'entrada con el blob de los bytes con BOM (= el registrado, 40295387…); el blob sin el BOM es otro; los hechos del instrumento citan el '
                   'SHA-256 de los bytes con BOM', 'BOM: %s; blob con BOM %s (registrado %s); sin BOM %s; Sha256 de los hechos = SHA-256 con BOM: %s' % (
                       'sí' if bom else 'no', blob_id(green)[:8], cur['Blob'][:8], blob_id(green[3:])[:8],
                       'sí' if (runs['trx-current:facts'][1] or {}).get('Sha256') == sha256(green) else 'no'),
                   bom and blob_id(green) == cur['Blob'] == GREEN_TRX_BLOB and blob_id(green[3:]) != cur['Blob']
                   and (runs['trx-current:facts'][1] or {}).get('Sha256') == sha256(green)))
    d = variant('nobom', lambda d: role(d, 'TRX_CURRENT').update({'Custody': fwd(nobom)}))
    E6, rep6, err6 = build(d)
    ab = [a for a in rep6['Absences'] if a['Role'] == 'TRX_CURRENT']
    Ep6 = wb(os.path.join(out, 'E-nobom.json'), MEV.serialize(E6))
    cl6 = derive(d, Ep6, out, 'nobom')
    a6 = accept_req(minimal_request(T, [runs['trx-nobom']], cl6, relay_t), out, 'nobom', [T])
    c1.append(case('BOM-STRIPPED', 'una copia del TRX sin el BOM (bytes distintos) en la ruta designada; el blob registrado es el de los bytes con BOM',
                   'sin entrada (BLOB_MISMATCH: quitar el BOM no es fiel); su lectura queda fuera del cierre (R9 MISMATCH)',
                   'causa: %s; aceptante: %s, R9 %s' % (ab[0]['Cause'] if ab else '-', a6['Decision'], a6['Checks'].get('R9')),
                   ab and ab[0]['Cause'].startswith('BLOB_MISMATCH') and a6['Checks'].get('R9') == 'MISMATCH', Acceptor=a6))
    # ---- checking the absence of a designated artifact is not a read outside the closure (rule 5 (c))
    hrole = role(base, 'H')
    d = variant('h-absent', lambda d: role(d, 'H').update({'Custody': fwd(os.path.join(out, 'no-existe-handoff.json'))}))
    Eh, reph, errh = build(d)
    Eph = wb(os.path.join(out, 'E-h-absent.json'), MEV.serialize(Eh))
    clh = derive(d, Eph, out, 'h-absent')
    G = d['Invocation']['Target']['Commit']
    runh, rch, envh = instr_run(T, out, 'handoff-absent', ['handoff', '--repo', fwd(T), '--file', 'H=' + fwd(os.path.join(out, 'no-existe-handoff.json')),
                                                            '--sha256', 'H=' + sha256(rb(hrole['Custody'])), '--schema-rev', G, '--allow-absent'])
    ah = accept_req(minimal_request(T, [runh], clh, relay_t), out, 'h-absent', [T])
    c1.append(case('ABSENCE-CHECK', 'comprobar que la entrega designada falta (handoff --allow-absent sobre su ruta de custodia) con el cierre sin entrega (como N4)',
                   'no es una lectura fuera del cierre ni aporta contenido (A7-1, punto 5 (c)): R9 MATCH; los hechos dicen Present false',
                   'hechos: %s; aceptante: %s, R9 %s; C6 del cierre sin H: %s' % ((envh.get('Facts') or {}), ah['Decision'], ah['Checks'].get('R9'),
                                                                                  'sí' if not any(f['Name'] == 'H' for f in clh['Closure']['Files']) else 'no'),
                   (envh.get('Facts') or {}).get('Present') is False and ah['Checks'].get('R9') == 'MATCH'
                   and not any(f['Name'] == 'H' for f in clh['Closure']['Files']), Acceptor=ah))
    # ---- RelayFacts
    rel_e = [e for e in E['CanonicalInputs'] if e['Path'] == relay['Path']]
    c2.append(case('RELAY-POS', 'hechos del relevo sintéticos (relay-record/v2: Outcome COMPLETED y operación 7, RemoteFacts con las corridas de CurrentSha y '
                   'RedSha y sus TRX, Routing, Denials) designados con su papel, ruta y blob',
                   'una entrada RELAY; C6 no le da nombre (el instrumento no la lee); la solicitud lleva RelayFacts con su SHA-256 y R10 MATCH',
                   'entrada: %s; Name(p) de C6 para la ruta: %s; R10 %s (%s)' % (
                       '%s %s' % (rel_e[0]['Path'], rel_e[0]['Blob'][:8]) if rel_e else 'ninguna',
                       'ninguno' if relay['Path'] not in sum(cl['Names'].values(), []) else 'definido', a_pos['Checks'].get('R10'), a_pos['R10']),
                   len(rel_e) == 1 and relay['Path'] not in sum(cl['Names'].values(), []) and a_pos['Checks'].get('R10') == 'MATCH'))
    d = variant('relay-absent', lambda d: role(d, 'RELAY').update({'Custody': fwd(os.path.join(out, 'no-existe-relay.json'))}))
    E7, rep7, err7 = build(d)
    ab = [a for a in rep7['Absences'] if a['Role'] == 'RELAY']
    Ep7 = wb(os.path.join(out, 'E-relay-absent.json'), MEV.serialize(E7))
    cl7 = derive(d, Ep7, out, 'relay-absent')
    a7 = accept_req(minimal_request(T, [runs['trx-current'], runs['trx-red']], cl7, None), out, 'relay-absent', [T])
    c2.append(case('RELAY-ABSENT', 'hechos del relevo ausentes', 'sin entrada, causa registrada; la solicitud no lleva RelayFacts (A7-1, punto 6: los de (c)) y R10 no '
                   'puede dar MATCH: la salida no quedaría aceptada (A6-5, regla 2, exige R10 en MATCH); nada lo sustituye',
                   'causa: %s; aceptante: %s, R10 %s (%s)' % (ab[0]['Cause'] if ab else '-', a7['Decision'], a7['Checks'].get('R10'), a7['R10']),
                   ab and not any(e['Path'] == relay['Path'] for e in E7['CanonicalInputs']) and a7['Checks'].get('R10') != 'MATCH', Acceptor=a7))
    subst = wb(os.path.join(out, 'relay-facts-otro.json'), rb(relay['Custody']).replace(b'"Disposition": "NONE"', b'"Disposition": "STOP"'))
    d = variant('relay-subst', lambda d: role(d, 'RELAY').update({'Custody': fwd(subst)}))
    E8, rep8, err8 = build(d)
    ab = [a for a in rep8['Absences'] if a['Role'] == 'RELAY']
    a8 = accept_req(minimal_request(T, [runs['trx-current'], runs['trx-red']], cl, (subst, relay_t[1])), out, 'relay-subst', [T])
    c2.append(case('RELAY-SUBST', 'hechos del relevo sustituidos (otros bytes en la ruta designada; blob registrado sin cambio)',
                   'sin entrada (BLOB_MISMATCH); si la solicitud citara el SHA-256 registrado con esos bytes, R10 MISMATCH',
                   'causa: %s; aceptante con el SHA-256 citado y los bytes sustituidos: R10 %s (%s)' % (ab[0]['Cause'] if ab else '-', a8['Checks'].get('R10'), a8['R10']),
                   ab and ab[0]['Cause'].startswith('BLOB_MISMATCH') and a8['Checks'].get('R10') == 'MISMATCH', Acceptor=a8))
    E9 = json.loads(json.dumps(E))
    E9['CanonicalInputs'].insert(-1, {'Path': 'artifacts/orchestration/FX-U1/T1/0/R20261011T081000Z-a000/relay-facts.json',
                                      'Blob': blob_id(rb(BENCH + '/work/s-n4/relay-facts.json'))})
    v9 = vb(CHK.check(base, E9, i62_repo=W, i62_commit=BASE))
    c2.append(case('RELAY-FOREIGN', 'hechos del relevo de otra corrida, en una ruta no designada', 'FOREIGN (no admitidos; su lectura sería P-22)',
                   'validador: %s' % ('acepta' if v9['Accepted'] else 'rechaza: ' + '; '.join('FOREIGN %s' % f[0] for f in v9['Foreign'])),
                   (not v9['Accepted']) and any('relay-facts' in f[0] for f in v9['Foreign'])))
    # ---- the measurement invocation has no relay facts (A7-1, point 1 (c), after fix_a7r2_b.py; A6-7, rule 1)
    mdes = json.loads(rb(os.path.join(HERE, 'designations', 'M.json')).decode('utf-8'))
    Em, repm, errm = build(mdes)
    vm = vb(CHK.check(mdes, Em, i62_repo=W, i62_commit=BASE))
    m_rel = [e for e in Em['CanonicalInputs'] if e['Path'].endswith('/relay-facts.json')]
    m_abs = [a for a in repm['Absences'] if a['Role'] == 'RELAY']
    m_note = [n for n in repm['Notes'] if n.startswith('RELAY:')]
    c2.append(case('RELAY-MEASUREMENT', 'invocación de medición M (a7-designation/2 con Invocation.Kind MEASUREMENT), sin hechos del relevo',
                   'sin entrada RELAY; no es una ausencia (la medición no los tiene: nada que clasificar por 16.9), solo una nota; validador acepta',
                   'entradas RELAY: %d; ausencias RELAY: %d; nota: %s; validador: %s' % (len(m_rel), len(m_abs), m_note[0][:70] if m_note else '-',
                                                                                     'acepta' if vm['Accepted'] else 'rechaza'),
                   not m_rel and not m_abs and m_note and vm['Accepted']))
    m2 = json.loads(json.dumps(mdes))
    m2['Artifacts'].append({'Role': 'RELAY', 'Path': 'artifacts/orchestration/FX-U1/T1-m7/0/%s/relay-facts.json' % m2['Invocation']['RunId'],
                            'Blob': relay['Blob'], 'Custody': relay['Custody']})
    Em2, repm2, errm2 = build(m2)
    m2_abs = [a for a in repm2['Absences'] if a['Role'] == 'RELAY']
    Em2x = json.loads(json.dumps(Em2))
    Em2x['CanonicalInputs'].insert(-1, {'Path': m2['Artifacts'][-1]['Path'], 'Blob': relay['Blob']})
    vm2 = vb(CHK.check(m2, Em2x, i62_repo=W, i62_commit=BASE))
    m3 = json.loads(json.dumps(m2))
    m3['Invocation'].pop('Kind')
    Em3, repm3, errm3 = build(m3)
    c2.append(case('RELAY-MEASUREMENT-DESIGNATED', 'una medición cuya designación nombra hechos del relevo (papel inexistente para ella)',
                   'el constructor no los admite (causa registrada); un cierre que los incluya es FOREIGN; sin la marca Kind la medición no se distinguiría '
                   'de una verificación (los admitiría): la marca es necesaria en el formato',
                   'constructor: %s; validador con la entrada: %s; sin Kind: entrada RELAY %s' % (
                       m2_abs[0]['Cause'][:60] if m2_abs else 'admitidos', 'acepta' if vm2['Accepted'] else 'rechaza: FOREIGN %s' % [f[0].split('/')[-1] for f in vm2['Foreign']],
                       'sí' if any(e['Path'].endswith('/relay-facts.json') for e in Em3['CanonicalInputs']) else 'no'),
                   m2_abs and m2_abs[0]['Cause'].startswith('NOT_ADMITTED') and not any(e['Path'].endswith('/relay-facts.json') for e in Em2['CanonicalInputs'])
                   and not vm2['Accepted'] and any(f[0].endswith('/relay-facts.json') for f in vm2['Foreign'])
                   and any(e['Path'].endswith('/relay-facts.json') for e in Em3['CanonicalInputs'])))
    return ({'Phase': 'q1-trx', 'Cases': c1, 'Runs': dict((k, v) for k, v in runs.items() if not k.endswith(':facts'))},
            {'Phase': 'q2-relay', 'Cases': c2})


# ------------------------------------------------------------------ q3 N4 and nc1 with the round-2 designation and builder
def q3():
    cases = []
    out = out_dir('q3')
    bench_res = {'N4': json.loads(rb(BENCH + '/p3-n4.json').decode('utf-8')), 'NC1': json.loads(rb(BENCH + '/p4-nc1.json').decode('utf-8'))}
    for c, reqs in (('N4', [('A', BENCH + '/inputs/N4/request-A.json', 'N4-A')]),
                    ('NC1', [('V1', BENCH + '/inputs/NC1/request-V1.json', 'NC1-V1-A'), ('V2', BENCH + '/inputs/NC1/request-V2.json', 'NC1-V2-A'),
                             ('V3', BENCH + '/inputs/NC1/request-V3.json', 'NC1-V3-A')])):
        des = json.loads(rb(os.path.join(HERE, 'designations', '%s.json' % c)).decode('utf-8'))
        E, rep, err = build(des)
        Ep = wb(os.path.join(out, 'E-%s.json' % c), MEV.serialize(E))
        v = vb(CHK.check(des, E, i62_repo=W, i62_commit=BASE))
        r1e = rb(BENCH + '/inputs/%s/E-%s-R7-A.json' % (c, c))
        extra = [e for e in E['CanonicalInputs'] if e['Path'] in (RED_TRX_PATH,) or e['Path'].endswith('/relay-facts.json')]
        Er = json.loads(json.dumps(E))
        Er['CanonicalInputs'] = [e for e in Er['CanonicalInputs'] if e not in extra]
        same_r1 = MEV.serialize(Er) == r1e
        cl = derive(des, Ep, out, c)
        r1cl = json.loads(rb(BENCH + '/inputs/%s/%s-R7-A.closure.json' % (c, c)).decode('utf-8'))
        r1om = [(o['Term'], o['Cause']) for o in json.loads(rb(BENCH + '/inputs/%s/%s-R7-A.derivation.json' % (c, c)).decode('utf-8'))['Omitted']]
        sup = all(set(json.dumps(x, sort_keys=True) for x in r1cl[k]) <= set(json.dumps(x, sort_keys=True) for x in cl['Closure'][k])
                  for k in ('Commits', 'RevPaths', 'Refs', 'Paths', 'Objects', 'Files'))
        cases.append(case('%s-E' % c, '%s: E de A7-1 r2 (a7-designation/2 con TRX_RED y RELAY) frente al E de la ronda 1 / R7-A custodiado en el banco' % c,
                          'el E r2 es el de la ronda 1 más las entradas TRX_RED y RELAY (en el mismo orden); validador acepta; ausencias de la invocación '
                          'registradas (N4: la entrega)', 'quitando %d entradas (%s), bytes iguales al E del banco: %s; validador: %s; ausencias (c): %s' % (
                              len(extra), ', '.join(e['Path'].split('/')[-1] for e in extra), 'sí' if same_r1 else 'no', 'acepta' if v['Accepted'] else 'rechaza',
                              ', '.join('%s %s' % (a['Role'], a['Cause'].split(':')[0]) for a in rep['Absences'] if a['Category'] == 'c') or 'ninguna'),
                          same_r1 and v['Accepted'] and len(extra) == 2, ESha256=sha256(rb(Ep))))
        cases.append(case('%s-CLOSURE' % c, '%s: cierre del aceptante derivado (designate_a7 + derive_r4 sin cambio) frente al del banco' % c,
                          'superconjunto del cierre de la ronda 1 (más C4 de las dos rutas nuevas y C6 del segundo TRX), con las mismas omisiones',
                          'superconjunto: %s; omisiones iguales: %s; C6: %s' % ('sí' if sup else 'no', 'sí' if cl['Omitted'] == r1om else 'no', files_of(cl)),
                          sup and cl['Omitted'] == r1om))
        relay = next(a for a in des['Artifacts'] if a['Role'] == 'RELAY')
        snaps = [BENCH + '/c-' + c.lower()]
        for lab, rq, bench_case in reqs:
            req = json.loads(rb(rq).decode('utf-8'))
            same_relay = req['RelayFacts']['CitedSha256'] == sha256(rb(relay['Custody'])) if lab in ('A', 'V1') else None
            req['Closure'] = {'Path': fwd(cl['Path']), 'Sha256': cl['Sha256']}
            a = accept_req(req, out, '%s-%s' % (c, lab), snaps)
            bc = next(x for x in bench_res[c]['Cases'] if (x.get('Case') or x.get('Caso')) == bench_case)
            bchecks = (bc.get('Observed') or bc.get('Observado') or {}).get('Checks')
            cases.append(case('%s-%s-R2' % (c, lab), '%s: la misma solicitud del banco (%s) con el cierre derivado r2' % (c, bench_case),
                              'mismo resultado del aceptante que en el banco (las comprobaciones no dependen del cierre salvo R9, monótona: un cierre mayor no '
                              'crea violaciones); cero escrituras en el repositorio del banco',
                              '%s rc %d; comprobaciones iguales a las del banco: %s%s; cero escrituras: %s' % (
                                  a['Decision'], a['Rc'], 'sí' if a['Checks'] == bchecks else 'no (%s / %s)' % (a['Checks'], bchecks),
                                  ('; RelayFacts del banco = entrada RELAY de (c): %s' % ('sí' if same_relay else 'no')) if same_relay is not None else '',
                                  'sí' if a['ZeroWrite'] else 'no'),
                              a['Checks'] == bchecks and a['ZeroWrite'] and same_relay is not False, Acceptor=a, Bench=bchecks))
    return {'Phase': 'q3-n4-nc1', 'Cases': cases}


# ------------------------------------------------------------------ q4 B1-like (EFF^1 differs) with the round-2 builder
def b1_designation(T, label, U1, U2, G, runs, red_trx, decision_ref):
    """Round-1 B1-like inputs (contract of the fixture, delegation and handoff of the kit, synthetic values; the same commits) in
    a7-designation/2, with TRX_CURRENT, TRX_RED (the delegation requires RED) and RELAY."""
    Tf = fwd(T)
    EFF = '88ab34c398f03070c94a584f95196a394d869f7c'
    calc, dec, st = 'src/Fixture.Lib/Calculator.cs', 'docs/automation/decisions/FX-U2.md', 'docs/automation/state/FX-U2.yml'
    bdir = os.path.join(HERE, 'b1-like', label)
    os.makedirs(bdir, exist_ok=True)
    K = deep_replace(json.loads(git(os.path.join(WORK, 'deliv'), 'cat-file', 'blob', '628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0')[1].decode('utf-8')), 'FX-U1', 'FX-U2')
    K.update({'AuthorityRevision': U1, 'MainSha': EFF, 'Authorities': [
        {'Path': dec, 'Section': 'documento completo', 'Class': 'UNIT_DOC'}, {'Path': st, 'Section': 'documento completo', 'Class': 'UNIT_DOC'},
        {'Path': calc, 'Section': 'documento completo', 'Class': 'UNIT_CHANGE'}, {'Path': 'AGENTS.md', 'Section': '## Jobs requeridos', 'Class': 'EXTERNAL'},
        {'Path': 'docs/AUTOMATION_PLAN.md', 'Section': '## 16. Ejecución delegada bajo orden del Coordinator', 'Class': 'EXTERNAL'}]})
    D_RUN, H_RUN, I_RUN = runs
    D = deep_replace(deep_replace(json.loads(rb(os.path.join(WORK, 'kit-inputs', 'deleg.json')).decode('utf-8')), 'FX-U1', 'FX-U2'), 'R20261012T090000Z-d001', D_RUN)
    D.update({'RunId': D_RUN, 'AuthorityRevision': U1, 'MainSha': EFF, 'BaseSha': U2, 'ExpectedBranch': 'fx/u2', 'ExpectedWorktree': Tf,
              'Authorities': K['Authorities'], 'ChainBaseSha': U2, 'ChainRedSha': None})
    H = deep_replace(deep_replace(json.loads(rb(os.path.join(WORK, 'kit-inputs', 'handoff.json')).decode('utf-8')), 'FX-U1', 'FX-U2'), 'R20261012T090000Z-d001', D_RUN)
    H.update({'RunId': H_RUN, 'DelegationRunId': D_RUN, 'BaseSha': U2, 'RedSha': None, 'CurrentSha': G, 'Branch': 'fx/u2', 'Worktree': Tf,
              'FilesChanged': sorted(x.decode() for x in git(T, 'diff', '--name-only', '-z', U2 + '..' + G)[1].split(b'\0') if x)})
    kp = wb(os.path.join(bdir, 'FX-U2-T1.gate-contract.json'), jbytes(K))
    dp_ = wb(os.path.join(bdir, 'deleg.json'), jbytes(D))
    hp = wb(os.path.join(bdir, 'handoff.json'), jbytes(H))
    tp = wb(os.path.join(bdir, 'fixture-tests.trx'), rb(os.path.join(WORK, 'kit-inputs', 'fixture-tests.trx')))
    rp = wb(os.path.join(bdir, 'relay-facts.json'), jbytes(dict(relay_facts('B1-' + label, I_RUN, G, None, EFF, 'controller-output.json', None),
                                                              TaskId='T1')))
    art = lambda r, p, f: {'Role': r, 'Path': p, 'Blob': blob_id(rb(f)), 'Custody': fwd(f)}
    des = {'Schema': 'a7-designation/3', 'Case': 'B1-LIKE-' + label, 'Repo': Tf, 'AuthorityRevision': U1,
           'Invocation': {'UnitId': 'FX-U2', 'TaskId': 'T1', 'Attempt': 0, 'RunId': I_RUN, 'Target': {'Commit': G, 'Path': calc, 'Blob': MEV.blob_at(T, G, calc)}},
           'RunPath': 'artifacts/orchestration/FX-U2/T1/0/%s/' % I_RUN,
           'Artifacts': [art('DELEG', 'artifacts/orchestration/FX-U2/T1/0/%s/output.json' % D_RUN, dp_),
                         art('H', 'artifacts/orchestration/FX-U2/T1/0/%s/worker-handoff.json' % H_RUN, hp),
                         art('CONTRACT', 'docs/automation/decisions/FX-U2-T1.gate-contract.json', kp),
                         art('TRX_CURRENT', CUR_TRX_PATH, tp), art('TRX_RED', RED_TRX_PATH, red_trx),
                         art('RELAY', 'artifacts/orchestration/FX-U2/T1/0/%s/relay-facts.json' % I_RUN, rp)],
           'Instrument': {'Path': 'docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py', 'Blob': INSTR},
           'MapPath': 'docs/automation/agent-execution/compatibility/I62-clause-map.json', 'Adapter': 'codex-cli', 'DecisionRef': decision_ref}
    desp = jdump(os.path.join(bdir, 'designation.json'), des)
    legacy = dict((k, v) for k, v in des.items() if k != 'Artifacts')
    legacy.update({'Schema': 'a6m7-designation/1', 'Inputs': [dict(a, Role={'TRX_CURRENT': 'TRX'}.get(a['Role'], a['Role'])) for a in des['Artifacts']
                                                               if a['Role'] in ('DELEG', 'H', 'CONTRACT', 'TRX_CURRENT')]})
    return des, legacy, kp


def q4(P):
    cases = []
    out = out_dir('q4')
    T = os.path.join(WORK, 'b1r2')
    rmtree(T)
    rmtree(os.path.join(HERE, 'b1-like'))
    r = subprocess.run(['git', '-c', 'core.longpaths=true', 'clone', '-q', '--no-checkout',
                        os.path.join(W, 'docs/automation/evidence/I-62-F6/MAP-INVALID/candidate-b.bundle'), T], capture_output=True, env=genv(), stdin=subprocess.DEVNULL)
    assert r.returncode == 0
    Tf = fwd(T)
    EFF, EFF1, CLAIM = '88ab34c398f03070c94a584f95196a394d869f7c', '55d190043458f461f187588ea79d8e9839440e28', '6040097c5f05277d4d786a99bfc80970665c1e5f'
    idx = os.path.join(WORK, 'b1r2.index')
    calc, dec, st = 'src/Fixture.Lib/Calculator.cs', 'docs/automation/decisions/FX-U2.md', 'docs/automation/state/FX-U2.yml'
    c0 = git(T, 'cat-file', 'blob', '%s:%s' % (CLAIM, calc))[1]
    head = b'# FX-U2 (unidad sintetica B1-like del preflight de A-7)\n\nBOOTSTRAP.\n'

    def chain(tag, calc_at_ar):
        ch = {dec: head, st: b'unit: FX-U2\nrecord_version: 1\n'}
        if calc_at_ar:
            ch[calc] = c0 + b'// cambio de la unidad anterior a la AuthorityRevision (variante %s)\n' % tag.encode()
        U1 = mkcommit(T, CLAIM, ch, 'FX-U2: BOOTSTRAP sintetico (AuthorityRevision, %s)' % tag, idx)
        c1 = git(T, 'cat-file', 'blob', '%s:%s' % (U1, calc))[1]
        U2 = mkcommit(T, U1, {st: b'unit: FX-U2\nrecord_version: 2\n', calc: c1 + b'// base sintetica de T1 (B1-like)\n'}, 'FX-U2: base sintetica de T1 (BaseSha, %s)' % tag, idx)
        c2 = git(T, 'cat-file', 'blob', '%s:%s' % (U2, calc))[1]
        G = mkcommit(T, U2, {calc: c2 + b'// entrega sintetica de T1 (B1-like)\n', dec: head + b'\nT1: entrega sintetica.\n'}, 'FX-U2 T1: entrega sintetica (Target, %s)' % tag, idx)
        return U1, U2, G
    U1, U2, G = chain('main', False)
    r1c = json.loads(rb(os.path.join(R1, 'p2-b1.json')).decode('utf-8'))['Commits']
    rc, oid = git(T, 'hash-object', '-w', '--no-filters', '--stdin', inp=git(W, 'cat-file', 'blob', INSTR)[1])
    red = os.path.join(HERE, 'inputs', 'fixture-tests-red.trx')
    des, legacy, kp = b1_designation(T, 'main', U1, U2, G, ('R20261017T090000Z-b1d1', 'R20261017T091000Z-b1a1', 'R20261017T092000Z-b1v1'), red, 'BANCO-A7R2-B1LIKE')
    E, rep, err = build(des)
    Er7 = MEV.build(legacy, 'R7-A')[0]
    pa7 = wb(os.path.join(out, 'E-B1-A7r2.json'), MEV.serialize(E))
    pr7 = wb(os.path.join(out, 'E-B1-R7-A.json'), MEV.serialize(Er7))
    at1 = [e for e in rep['Entries'] if e['Revision'] == 'EFF^1' and e['Blob'] != e['BlobAtAuthorityRevision']]
    v = vb(CHK.check(des, E, i62_repo=W, i62_commit=BASE))
    cases.append(case('B1R2-E', 'B1-like (candidate-b, EFF^1 con los blobs anteriores a la activación) con a7-designation/2 y el constructor r2 (mismos commits '
                      'sintéticos que en la ronda 1)', 'las mismas fijaciones en EFF^1 que en la ronda 1 (6, blob distinto del de la AR) más TRX_RED y RELAY; '
                      'validador acepta', 'commits iguales a la ronda 1: %s; entradas en EFF^1 con blob distinto de la AR: %d; (c): %s; validador: %s' % (
                          'sí' if (U1, U2, G) == (r1c['AuthorityRevision'], r1c['BaseSha'], r1c['Target']) else 'no', len(at1),
                          ', '.join(e['Kind'] for e in rep['Entries'] if e['Category'] == 'c'), 'acepta' if v['Accepted'] else 'rechaza'),
                      (U1, U2, G) == (r1c['AuthorityRevision'], r1c['BaseSha'], r1c['Target']) and len(at1) == 6 and v['Accepted']))
    cl_a = derive(des, pa7, out, 'A7r2')
    chk7 = DSM.check(legacy, Er7)
    vec7 = dict(chk7['Vector'], E=fwd(pr7))
    res7 = DV.derive_vector(vec7)
    cp7 = jdump(os.path.join(out, 'closure-R7-A.json'), res7['Closure'])
    cl_r = {'Path': cp7, 'Sha256': sha256(rb(cp7)), 'Closure': res7['Closure']}
    argv = ['clausemap', '--repo', Tf, '--main', EFF, '--file', 'CONTRACT=' + fwd(kp), '--sha256', 'CONTRACT=' + sha256(rb(kp)), '--base', U2]
    run, rcx, env = instr_run(T, out, 'r00-clausemap', argv)
    relay = next(a for a in des['Artifacts'] if a['Role'] == 'RELAY')
    rel = (relay['Custody'], sha256(rb(relay['Custody'])))
    aa = accept_req(minimal_request(T, [run], cl_a, rel), out, 'A7r2', [T])
    ar = accept_req(minimal_request(T, [run], cl_r, rel), out, 'R7-A', [T])
    cases.append(case('B1R2-R9', 'clausemap por la forma declarada y el aceptante fijado, con el cierre derivado del E r2 y con el de R7-A',
                      'R9 MATCH con A7-1 r2; R9 MISMATCH con R7-A (lecturas en EFF^1 fuera del cierre); R10 MATCH con los hechos del relevo de (c)',
                      'clausemap rc %d; A7 r2: %s, R9 %s, R10 %s; R7-A: %s, R9 %s (%d violaciones); cero escrituras: %s' % (
                          rcx, aa['Decision'], aa['Checks'].get('R9'), aa['Checks'].get('R10'), ar['Decision'], ar['Checks'].get('R9'), ar['R9ViolationCount'],
                          'sí' if aa['ZeroWrite'] and ar['ZeroWrite'] else 'no'),
                      rcx == 0 and aa['Checks'].get('R9') == 'MATCH' and aa['Checks'].get('R10') == 'MATCH' and ar['Checks'].get('R9') == 'MISMATCH'
                      and aa['ZeroWrite'] and ar['ZeroWrite'], A7=aa, R7A=ar))
    U1v, U2v, Gv = chain('uc', True)
    dv, lv, kv = b1_designation(T, 'uc', U1v, U2v, Gv, ('R20261017T100000Z-b2d1', 'R20261017T101000Z-b2a1', 'R20261017T102000Z-b2v1'), red, 'BANCO-A7R2-B1LIKE-UC')
    Ev, repv, errv = build(dv)
    pv = wb(os.path.join(out, 'E-B1UC-A7r2.json'), MEV.serialize(Ev))
    clv = derive(dv, pv, out, 'uc')
    argv_v = ['clausemap', '--repo', Tf, '--main', EFF, '--file', 'CONTRACT=' + fwd(kv), '--sha256', 'CONTRACT=' + sha256(rb(kv)), '--base', U2v]
    runv, rcv, envv = instr_run(T, out, 'r00-clausemap-uc', argv_v)
    rv = next(a for a in dv['Artifacts'] if a['Role'] == 'RELAY')
    av = accept_req(minimal_request(T, [runv], clv, (rv['Custody'], sha256(rb(rv['Custody'])))), out, 'uc', [T])
    pre = CIT.check(dv, 'coverage', Ev)
    cases.append(case('B1R2-UC', 'residuo declarado (§3.5): Calculator.cs cambió antes de la AR; clausemap lo compara en EFF^1, EFF y MainSha_eval',
                      'sin cambio respecto de la ronda 1: R9 MISMATCH con el cierre de A7-1 r2 (rechazo falso, nunca aceptación falsa) y la precondición de '
                      'cobertura bloquea el lanzamiento (S-03)', 'aceptante: %s, R9 %s; precondición: %s (%s)' % (
                          av['Decision'], av['Checks'].get('R9'), pre['Result'], ', '.join(b['Path'].split('/')[-1] for b in pre['Blocking'])),
                      av['Checks'].get('R9') == 'MISMATCH' and pre['Result'] == 'NO_LAUNCH_S-03', Acceptor=av))
    prem = CIT.check(des, 'coverage', E)
    cases.append(case('B1R2-CIT', 'precondición de cobertura en el caso B1-like principal', 'se puede lanzar (AUTOMATION_PLAN en EFF^1 está fijado por el cierre)',
                      prem['Result'], prem['Result'] == 'LAUNCH_MAY_PROCEED'))
    return {'Phase': 'q4-b1', 'Cases': cases, 'Commits': {'AuthorityRevision': U1, 'BaseSha': U2, 'Target': G, 'EFF': EFF, 'EFF^1': EFF1,
                                                         'Variant-uc': {'AuthorityRevision': U1v, 'BaseSha': U2v, 'Target': Gv}}}


# ------------------------------------------------------------------ q5 identity of the A6-7 invalidator (A7-1 point 7)
PIECE_KEYS = ('A7Blob', 'BuilderSha256', 'ValidatorSha256', 'DesignationFormatSha256', 'DerivationDesignateSha256', 'DerivationDeriveSha256')


def identity(a7, builder, validator, fmt, designate, derive_impl):
    """A6-7 invalidator of the closure procedure (A7-1, point 7, after fix_a7r2_b.py): the A7-1 blob and the named pieces: the
    builder, the validator, the designation format and the implementation of the derivation of the acceptor closure
    (designate_a7.py: designation -> derivation vector and C6 names; derive_r4.py: the derivation, reused unchanged)."""
    return dict(zip(PIECE_KEYS, (blob_id(rb(a7)), sha256(rb(builder)), sha256(rb(validator)), sha256(rb(fmt)), sha256(rb(designate)),
                                 sha256(rb(derive_impl)))))


def q5():
    cases = []
    out = out_dir('q5')
    pieces = (CAND, os.path.join(HERE, 'make_eic_a7.py'), os.path.join(HERE, 'check_a7_closure.py'), os.path.join(HERE, 'a7-designation-3.schema.json'),
              os.path.join(HERE, 'designate_a7.py'), os.path.join(HERE, 'derive_r4.py'))
    ident = identity(*pieces)
    rows = []
    for c in CASES:
        des = json.loads(rb(os.path.join(HERE, 'designations', '%s.json' % c)).decode('utf-8'))
        E, rep, err = build(des)
        data = MEV.serialize(E)
        wb(os.path.join(out, 'E-%s.json' % c), data)
        v = CHK.check(des, E, i62_repo=W, i62_commit=BASE)
        rows.append({'Case': c, 'ESha256': sha256(data), 'Accepted': v['Accepted'], 'Identity': identity(*pieces)})
    shas = [r['ESha256'] for r in rows]
    cases.append(case('INV-DISTINCT', 'cierres de V, N4, nc1, nc2, N5, N6, N7a, N7b y M con a7-designation/2 y el constructor r2',
                      'SHA-256 distintos por construcción, todos válidos bajo A7-1 con el mismo procedimiento (constructor, validador, formato de designación '
                      'e implementación de la derivación)',
                      '%d cierres, %d SHA-256 distintos; aceptados %d; procedimiento: A-7 %s, constructor %s…, validador %s…, formato %s…, designate_a7 %s…, '
                      'derive_r4 %s…' % (
                          len(rows), len(set(shas)), sum(1 for r in rows if r['Accepted']), ident['A7Blob'][:8], ident['BuilderSha256'][:12],
                          ident['ValidatorSha256'][:12], ident['DesignationFormatSha256'][:12], ident['DerivationDesignateSha256'][:12],
                          ident['DerivationDeriveSha256'][:12]),
                      len(set(shas)) == len(rows) and all(r['Accepted'] for r in rows) and all(r['Identity'] == ident for r in rows), Rows=rows, Identity=ident))
    # only an invocation's own inputs change: the closure changes, the invalidator does not
    d = json.loads(rb(os.path.join(HERE, 'designations', 'V.json')).decode('utf-8'))
    rel = next(a for a in d['Artifacts'] if a['Role'] == 'RELAY')
    alt = wb(os.path.join(out, 'relay-facts-V-otra-sesion.json'), rb(rel['Custody']).replace(b'"EndUtc": "2026-10-19T09:05:00Z"', b'"EndUtc": "2026-10-19T09:06:00Z"'))
    rel.update({'Custody': fwd(alt), 'Blob': blob_id(rb(alt))})
    E2, rep2, err2 = build(d)
    v2 = CHK.check(d, E2, i62_repo=W, i62_commit=BASE)
    cases.append(case('INV-INPUT-ONLY', 'V con otros hechos del relevo (otra sesión): cambian solo las entradas de la invocación',
                      'otro cierre, válido, y el mismo invalidador: no anula la medición', 'SHA-256 del cierre %s (antes %s); validador: %s; procedimiento igual: %s' % (
                          sha256(MEV.serialize(E2))[:16], rows[0]['ESha256'][:16], 'acepta' if v2['Accepted'] else 'rechaza', 'sí' if identity(*pieces) == ident else 'no'),
                      sha256(MEV.serialize(E2)) != rows[0]['ESha256'] and v2['Accepted'] and identity(*pieces) == ident))
    changed = []
    for k, label in enumerate(PIECE_KEYS):
        src = pieces[k]
        mod = wb(os.path.join(out, 'pieza-%d' % k + os.path.splitext(src)[1]), rb(src) + b'\n')
        pp = list(pieces)
        pp[k] = mod
        diff = [x for x in ident if identity(*pp)[x] != ident[x]]
        changed.append((label, diff))
    cases.append(case('INV-PIECE', 'un byte más en cada pieza del procedimiento nombrado (A-7; el constructor, el validador, el formato de la designación y la '
                      'implementación de la derivación del cierre del aceptante: designate_a7.py y derive_r4.py), una a una',
                      'cada cambio es un cambio del invalidador (anula la medición y la elegibilidad, A7-1, punto 7) y solo cambia esa pieza',
                      '; '.join('%s → cambia %s' % (l, ', '.join(d) or 'nada') for l, d in changed), all(d == [l] for l, d in changed)))
    # the derivation implementation: E and its SHA-256 do not change, the invalidator does (the procedure, not the instance)
    vdes = json.loads(rb(os.path.join(HERE, 'designations', 'V.json')).decode('utf-8'))
    rows_d = []
    for k, label in ((4, 'DerivationDesignateSha256'), (5, 'DerivationDeriveSha256')):
        src = pieces[k]
        mod = wb(os.path.join(out, 'derivacion-%s' % os.path.basename(src)), rb(src) + b'# cambio de la implementacion de la derivacion (preflight)\n')
        pp = list(pieces)
        pp[k] = mod
        Ev, repv, errv = build(vdes)
        old4 = identity(*pp)
        rows_d.append({'Piece': os.path.basename(src), 'ESha256': sha256(MEV.serialize(Ev)), 'Changed': [x for x in ident if old4[x] != ident[x]],
                       'ChangedUnderRun1List': [x for x in PIECE_KEYS[:4] if old4[x] != ident[x]]})
    cases.append(case('INV-DERIVATION', 'cambio de designate_a7.py y, aparte, de derive_r4.py (la implementación de la derivación del cierre del aceptante, '
                      'pieza nueva del punto 7 tras fix_a7r2_b.py)', 'cada uno es un cambio del invalidador aunque el E de la invocación (y su SHA-256) no '
                      'cambie; con la lista de piezas de la primera versión de la ronda 2 (sin la derivación) no se habría notado',
                      '; '.join('%s: E %s (igual al de V: %s), cambia %s; con la lista anterior cambia %s' % (
                          r['Piece'], r['ESha256'][:12], 'sí' if r['ESha256'] == rows[0]['ESha256'] else 'no', ', '.join(r['Changed']) or 'nada',
                          ', '.join(r['ChangedUnderRun1List']) or 'nada') for r in rows_d),
                      all(r['ESha256'] == rows[0]['ESha256'] and len(r['Changed']) == 1 and not r['ChangedUnderRun1List'] for r in rows_d)
                      and [r['Changed'][0] for r in rows_d] == ['DerivationDesignateSha256', 'DerivationDeriveSha256'], Rows=rows_d))
    return {'Phase': 'q5-invalidator', 'Cases': cases, 'Identity': ident, 'Rows': rows}


# ------------------------------------------------------------------ q6 compatibility with A-5 and A-6
def q6():
    cases = []
    a7 = rb(CAND).decode('utf-8')
    G = _load('a7-guards', alias='a7_guards_r2')
    for p, b in sorted(G.PINNED.items()):
        got = gtext(W, 'rev-parse', 'HEAD:' + p)
        cases.append(case('PIN-' + p.split('/')[-1], 'blob de %s en la base %s' % (p, BASE[:8]), b, got, got == b))
    a6 = git(W, 'cat-file', 'blob', G.PINNED['docs/initiatives/I-62-A-6.md'])[1].decode('utf-8').split('\n')
    dec = rb(os.path.join(W, 'docs/automation/decisions/I-62.md')).decode('utf-8').split('\n')
    k78 = dec.index(next(x for x in dec if x.startswith('## 78.')))
    l78 = next(x for x in dec[k78:k78 + 12] if x.startswith('A6-ADR0046-9-FX02 = A ('))
    owner = [(i + 1, x) for i, x in enumerate(a6) if x.startswith('A6-ADR0046-9-FX02 = A (') or x.startswith('A6-CONTROLLER-MEDICION-CONSUMO = A (')]
    od2 = [(i + 1, x) for i, x in enumerate(dec) if x.startswith('| **OD-2** | **APPROVED') or x.startswith('| **OD-2c** | **APPROVED')]
    r1p5 = dict((c['Caso'], c) for c in json.loads(rb(os.path.join(R1, 'p5-compat.json')).decode('utf-8'))['Cases'])
    for label, text in [('A-6 §7 L%d' % n, t) for n, t in owner] + [('decisiones §78', l78)] + [('decisiones L%d' % n, t) for n, t in od2]:
        h = sha256(text.encode('utf-8'))
        w_ = R1M.longest_window(text, a7)
        prev = [c for c in r1p5.values() if c['Caso'].startswith('PRES-OWNER') and h in c['Observado']]
        cases.append(case('OWNER-' + label.replace(' ', '-').replace('§', 'S'), 'línea del Owner: %s' % label,
                          'sin cambio (mismo SHA-256 que en la ronda 1); A-7 r2 no la reescribe (tramo común ≤ 60 caracteres)',
                          'SHA-256 %s; igual al del preflight de la ronda 1: %s; tramo común más largo con A-7 r2: %d' % (h[:16], 'sí' if prev else 'no', w_),
                          bool(prev) and w_ <= 60))
    norm = lambda x: re.sub(r'\s+', ' ', x)
    t = norm(a7)
    p8 = norm(a7[a7.index('> 8. **límites:**'):a7.index('### 3.3')])
    checks = {'cabecera': 'ámbito precisa A6-2, regla 1' in t, '§1': 'precisa A6-2, reglas 1 y 3, sin editarlas (§3.4)' in t,
              '§3.4': '**A6-2, regla 1.**' in t and 'que precisa sus reglas 1 y 3 en ese ámbito' in t,
              '§6': 'Dentro de su ámbito, A7-1 precisa A6-2, reglas 1 y 3' in t, 'punto 8': 'ni ninguna otra regla de A-6' in re.sub(r'\s*>\s*', ' ', p8)}
    touched = [{'Regla': 'A6-1', 'Cómo': 'no tocada'},
               {'Regla': 'A6-2, regla 1', 'Cómo': 'precisada dentro del ámbito de A-7 (ronda 2, R3): además del instrumento, las entradas de (c) que no son artefactos '
                                                  'versionados son excepciones a la clase de §20.3.1, solo por el blob de sus bytes custodiados'},
               {'Regla': 'A6-2, regla 2', 'Cómo': 'sin cambio (la categoría (d) remite a ella)'},
               {'Regla': 'A6-2, regla 3', 'Cómo': 'precisada dentro del ámbito de A-7: referencia de comparación (I6) por categoría; la AR para las autoridades de la unidad'},
               {'Regla': 'A6-3, reglas 1-7', 'Cómo': 'sin cambio; la regla 6 (b) deriva del cierre de A7-1 tal como es y su RelayFacts son los de (c)'},
               {'Regla': 'A6-4 a A6-6, A6-8', 'Cómo': 'no tocadas'},
               {'Regla': 'A6-7, reglas 5 y 7', 'Cómo': 'sin cambio de texto: el «cierre de VERIFY» es A7-1 con el procedimiento nombrado (A7-1, punto 7)'},
               {'Regla': 'A6-9, regla 4 y tabla', 'Cómo': 'sin cambio (C6 no nombra los hechos del relevo)'}]
    cases.append(case('COMPAT-A6-RULES', 'reglas de A-6 que toca A7-1 en la ronda 2', 'A6-2, reglas 1 y 3, precisadas en su ámbito y declaradas en cabecera, §1, §3.4 y §6; '
                      'ninguna otra (punto 8)', '; '.join('%s: %s' % (k, 'sí' if ok else 'no') for k, ok in checks.items()), all(checks.values()), Table=touched))
    a5 = gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-5.md')
    cases.append(case('COMPAT-A5', 'A-5 (A5-1, A5-2)', 'blob ad94ceed sin cambio; A-7 r2 declara A5-1 y A5-2 sin cambio y no crea B1',
                      'blob %s; «A5-1 y A5-2 sin cambio»: %s; «no crea B1»: %s' % (a5, 'sí' if 'A5-1 y A5-2 sin cambio' in a7 else 'no', 'sí' if 'no crea B1' in a7 else 'no'),
                      a5 == G.PINNED['docs/initiatives/I-62-A-5.md'] and 'A5-1 y A5-2 sin cambio' in a7 and 'no crea B1' in a7))
    return {'Phase': 'q6-compat', 'Cases': cases, 'A6RulesTouched': touched}


# ------------------------------------------------------------------ q7 guards
def q7():
    tmp = os.path.join(WORK, 'guards-tmp-r2')
    rmtree(tmp)
    os.makedirs(tmp)
    res = {}
    for mode, outn in (('selftest', 'q7-guards-selftest.json'), ('preview', 'q7-guards-preview.json')):
        r = subprocess.run([sys.executable, '-I', os.path.join(HERE, 'a7-guards.py'), mode, '--cand', CAND, '--evid', HERE, '--out', os.path.join(HERE, outn),
                            '--repo', W, '--base', BASE, '--tmp', tmp], capture_output=True, stdin=subprocess.DEVNULL)
        res[mode] = json.loads(rb(os.path.join(HERE, outn)).decode('utf-8'))
    st, pv = res['selftest'], res['preview']
    cases = [case('GUARD-PREVIEW', 'guardas G1-G7 sobre el commit hipotético (base %s + candidata r2 como modificación + este directorio bajo I-62-A7/r2/)' % BASE[:8],
                  'PASS en las siete', '; '.join('%s %s' % (k, v['Status']) for k, v in sorted(pv['Guards'].items())), pv['Result'] == 'PASS', Head=pv['Head']),
             case('GUARD-SELFTEST', 'autoprueba: un mutante por condición nueva o cambiada en la ronda 2', 'candidata PASS y todos los mutantes muertos',
                  'candidata %s; mutantes muertos %d de %d; condiciones sin cambio reutilizadas de la autoprueba de la ronda 1' % (
                      st['Candidate']['Result'], st['Killed'], st['MutantsTotal']), st['Result'] == 'PASS')]
    r2b = [m for m in st['Mutants'][1:] if m['Condition'] in (st.get('ChangedAfterFixA7r2b') or [])]
    cases.append(case('GUARD-R2B', 'condiciones de G4 nuevas o cambiadas por las tres frases de fix_a7r2_b.py (punto 1 (c) RED y medición, punto 7, fila (c) '
                      'de §3.3, frases sustituidas como superadas)', 'un mutante por condición, todos muertos',
                      '%d de %d muertos: %s' % (sum(1 for m in r2b if m['Killed']), len(r2b), ', '.join(m['Condition'] for m in r2b)),
                      len(r2b) == 5 and all(m['Killed'] for m in r2b)))
    r2c = [m for m in st['Mutants'][1:] if m['Condition'] in (st.get('ChangedAfterFixA7r2c') or [])]
    cases.append(case('GUARD-R2C', 'condiciones de G3 y G4 nuevas o cambiadas por fix_a7r2_c.py (L16 con AP 16.8 L616-617; punto 1 (c) «(16.8; 16.9, …)»; '
                      'regla 5 (c) con la exigencia de RED indeterminada; la cita sin 16.8 como superada)', 'un mutante por condición, todos muertos',
                      '%d de %d muertos: %s' % (sum(1 for m in r2c if m['Killed']), len(r2c), ', '.join('%s %s' % (m['Guard'], m['Condition']) for m in r2c)),
                      len(r2c) == 9 and all(m['Killed'] for m in r2c)))
    for m in st['Mutants'][1:]:
        cases.append(case('MUT-%s-%s' % (m['Guard'], m['Condition']), m['Mutant'], '%s FAIL (%s)' % (m['Guard'], m['ExpectedFinding']),
                          '%s %s' % (m['Guard'], m['GuardStatus']), m['Killed']))
    return {'Phase': 'q7-guards', 'Cases': cases, 'Preview': {'Result': pv['Result'], 'Head': pv['Head'], 'CandidateBlob': pv['CandidateBlob'],
                                                            'Guards': dict((k, v['Status']) for k, v in pv['Guards'].items())},
            'Selftest': {'Result': st['Result'], 'Killed': st['Killed'], 'Total': st['MutantsTotal'], 'Unchanged': st.get('UnchangedConditions')}}


# ------------------------------------------------------------------ main
def main(argv):
    global CAND
    if len(argv) < 2 or argv[1] != 'all':
        sys.stderr.write('usage: python -I run_a7r2.py all [--cand <I-62-A-7.md>] [--reuse q3,q4]\n')
        return 64
    CAND = argv[argv.index('--cand') + 1] if '--cand' in argv else os.path.join(WORK, 'cand-r2', 'I-62-A-7.md')
    # --reuse q3,q4: phases that do not depend on the text and whose code and inputs are unchanged are reused by SHA-256 from the
    # previous run of this directory (validacion incremental, §88): the procedure pieces other than the A-7 blob and the
    # designations must be byte-identical to those of that run, otherwise the run stops.
    reuse = [x for x in (argv[argv.index('--reuse') + 1].split(',') if '--reuse' in argv else []) if x]
    assert set(reuse) <= {'q3', 'q4'}, 'only q3 and q4 can be reused'
    QF = {'q3': 'q3-n4-nc1.json', 'q4': 'q4-b1.json'}
    prev = None
    if reuse:
        prev = {'Summary': json.loads(rb(os.path.join(HERE, 'summary.json')).decode('utf-8')),
                'Q0': json.loads(rb(os.path.join(HERE, 'q0-env.json')).decode('utf-8')),
                'Files': dict((k, sha256(rb(os.path.join(HERE, QF[k])))) for k in reuse)}
    for n in os.listdir(HERE):
        if (re.match(r'q\d-.*\.json$', n) and n not in [QF[k] for k in reuse]) or n in ('summary.json', 'preflight.md', 'MANIFEST.sha256'):
            os.remove(os.path.join(HERE, n))
    t0 = datetime.datetime.now(datetime.timezone.utc)
    R = {}
    P = prep()
    R['q0'] = q0(P)
    if reuse:
        pieces = (CAND, os.path.join(HERE, 'make_eic_a7.py'), os.path.join(HERE, 'check_a7_closure.py'), os.path.join(HERE, 'a7-designation-3.schema.json'),
                  os.path.join(HERE, 'designate_a7.py'), os.path.join(HERE, 'derive_r4.py'))
        now = identity(*pieces)
        was = prev['Summary']['Invalidator']
        same_tools = all(now[k] == was.get(k) for k in PIECE_KEYS[1:])
        same_des = all(P['Designations'][c]['Sha256'] == prev['Q0']['Prep']['Designations'][c]['Sha256'] for c in CASES)
        R['q0']['Cases'].append(case('REUSE-PHASES', 'fases reutilizadas de la corrida anterior de este directorio (sin dependencia del texto): %s' % ', '.join(reuse),
                                     'las piezas del procedimiento salvo el blob de A-7 y las designaciones, iguales byte a byte a las de esa corrida (A-7 %s…)' % was['A7Blob'][:8],
                                     'piezas iguales: %s; designaciones iguales: %s; archivos reutilizados: %s' % (
                                         'sí' if same_tools else 'no', 'sí' if same_des else 'no', ', '.join('%s %s…' % (QF[k], prev['Files'][k][:16]) for k in reuse)),
                                     same_tools and same_des, Files=dict((QF[k], prev['Files'][k]) for k in reuse), PreviousA7Blob=was['A7Blob']))
        assert same_tools and same_des, 'reuse refused: the procedure or the designations changed'
    jdump(os.path.join(HERE, 'q0-env.json'), R['q0'])
    R['q1'], R['q2'] = q12(P)
    jdump(os.path.join(HERE, 'q1-trx.json'), R['q1'])
    jdump(os.path.join(HERE, 'q2-relay.json'), R['q2'])
    for k, fn, f in (('q3', 'q3-n4-nc1.json', lambda: q3()), ('q4', 'q4-b1.json', lambda: q4(P))):
        if k in reuse:
            R[k] = json.loads(rb(os.path.join(HERE, fn)).decode('utf-8'))
        else:
            R[k] = f()
            jdump(os.path.join(HERE, fn), R[k])
    R['q5'] = q5()
    jdump(os.path.join(HERE, 'q5-invalidator.json'), R['q5'])
    R['q6'] = q6()
    jdump(os.path.join(HERE, 'q6-compat.json'), R['q6'])
    R['q7'] = q7()
    jdump(os.path.join(HERE, 'q7-guards.json'), R['q7'])
    allc = [(k, c) for k in ('q0', 'q1', 'q2', 'q3', 'q4', 'q5', 'q6', 'q7') for c in R[k]['Cases']]
    summary = {'Schema': 'i62-a7r2-preflight-summary/1',
               'Candidate': {'Path': 'docs/initiatives/I-62-A-7.md (candidata ronda 2, tras fix_a7r2_b.py y fix_a7r2_c.py)', 'Blob': CAND_BLOB},
               'CandidateRun1': CAND_BLOB_RUN1, 'CandidateRun2': CAND_BLOB_RUN2, 'PreviousRound': PREV_BLOB, 'Base': BASE,
               'StartUtc': t0.strftime('%Y-%m-%dT%H:%M:%SZ'),
               'ReusedPhases': dict((QF[k], prev['Files'][k]) for k in reuse) if reuse else {},
               'ToolChanges': ['RED required per AP 16.8 (ChainRedSha null or BaseSha..CurrentSha touching ChainRedFiles), not D.RequiredTests[].ExpectRed',
                               'a7-designation/2 Invocation.Kind (MEASUREMENT: no RELAY role)', 'invalidator pieces include designate_a7.py and derive_r4.py'],
               'Totals': dict((k, {'Cases': len(R[k]['Cases']), 'Hold': sum(1 for c in R[k]['Cases'] if c['Cumple'])}) for k in R),
               'Hold': sum(1 for _, c in allc if c['Cumple']), 'Cases': len(allc), 'NotHolding': [(k, c['Caso']) for k, c in allc if not c['Cumple']],
               'Guards': R['q7']['Preview'], 'Selftest': R['q7']['Selftest'], 'Invalidator': R['q5']['Identity'],
               'ReusedFromRound1': 'docs/automation/evidence/I-62-A7/ (sin cambio): reconstrucción estática p6, negativos de p3 no afectados, reproducción p1, '
                                   'preservación de p5 no afectada, guardas sin cambio'}
    jdump(os.path.join(HERE, 'summary.json'), summary)
    PT = _load('preflight_text_r2')
    wb(os.path.join(HERE, 'preflight.md'), sanitize(PT.render(R, summary, R1M.table)).encode('utf-8'))
    r = subprocess.run([sys.executable, '-I', os.path.join(HERE, 'a7-guards.py'), 'privacy', '--dir', HERE, '--repo', W], capture_output=True, stdin=subprocess.DEVNULL)
    priv = json.loads(r.stdout.decode('utf-8'))
    lines = []
    for root, dirs, names in os.walk(HERE):
        dirs[:] = sorted(d for d in dirs if d != '__pycache__')
        for n in sorted(names):
            if n == 'MANIFEST.sha256':
                continue
            p = os.path.join(root, n)
            lines.append('%s  %s' % (sha256(rb(p)), fwd(os.path.relpath(p, HERE))))
    wb(os.path.join(HERE, 'MANIFEST.sha256'), ('\n'.join(lines) + '\n').encode('ascii'))
    sys.stdout.write(json.dumps({'Cases': summary['Cases'], 'Hold': summary['Hold'], 'NotHolding': summary['NotHolding'], 'Guards': summary['Guards']['Result'],
                                 'Selftest': summary['Selftest'], 'Privacy': priv['Result']}, ensure_ascii=False) + '\n')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))
