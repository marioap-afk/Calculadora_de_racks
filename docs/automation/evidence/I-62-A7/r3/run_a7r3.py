# run_a7r3.py - single entry point of the targeted preflight of the THIRD candidate of I-62 A-7 (round 3; decisiones §96, puntos 2-5;
# validacion incremental de §88).   python -I run_a7r3.py all --cand <I-62-A-7.md>
# Phases (results next to this file): q0 environment, identities, reuse and the exact delta against round 2; q1-q5 the round-2 phases
# (TRX, RelayFacts, N4 and nc1, B1-like, invalidator) re-run with the round-3 pieces (r2lib.py: the round-2 runner with a7-designation/3);
# q6 compatibility (the round-2 checks plus A-5, A-6, EFF/EFF^1, the invalidator rule, the Owner authorizations, the budgets and the FX-02
# oracles, decisiones §96, punto 5); q7 the round-3 scope guards (preview and self-test); q8 the RebaseMap of the 16.7 reverification on a real
# rebase of the V repository (decisiones §96, punto 4); then summary.json, preflight.md, a privacy pass and MANIFEST.sha256. Stdlib only. Never
# runs Codex, claude or any model CLI; no network; never writes in the I-62 worktree, the main checkout, the bench, the kit or the evidence of
# rounds 1 and 2: every write goes to this directory or to the work area (D:/r62-a7/work/r3, temporary clones and copies).
import datetime, difflib, hashlib, importlib.util, json, os, re, shutil, subprocess, sys, unicodedata

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
WORK = os.path.join(ROOT, 'work')
W = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
BASE = '5da2007f45160ced37b958a3a772c46ac64874ee'
PREV_BLOB = '8a528133c7cccd83356e31f8fd79cd2ebb0cec15'     # round 2 (the base holds it)
A7P = 'docs/initiatives/I-62-A-7.md'
R2_PIECES = {'make_eic_a7.py': '0e5f9305525f8d0e91d09127dc1a0d2238883a37b28d29b08cfaf98803584aad',
             'check_a7_closure.py': '59e26e118697bc8371e0496d8b44490a5105e74536c5e67ca56343c854bc96b1',
             'a7-designation-2.schema.json': 'ae33ad31dde5f38094d766b6a576b3f354ff2e27066fa791718a751c9bf306c0',
             'designate_a7.py': '759df4f0332b1e4557ab3460973323e3016e485d6cb92cb1793df56f01842ab9',
             'derive_r4.py': '3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426'}
REUSED_SAME = ('derive_r4.py', 'make_eic_verify.py', 'designate_m7.py', 'lib_r1r4.py', 'check_citation_blobs.py')
RB_RUN, RV_RUN = 'R20261023T100000Z-0b01', 'R20261023T101000Z-0c02'   # the rebase record and the reverification (synthetic, deterministic)
FIXED_ENV = {'GIT_AUTHOR_NAME': 'A7 r3 preflight', 'GIT_AUTHOR_EMAIL': 'preflight@a7.invalid', 'GIT_COMMITTER_NAME': 'A7 r3 preflight',
             'GIT_COMMITTER_EMAIL': 'preflight@a7.invalid', 'GIT_AUTHOR_DATE': '2026-10-23T10:00:00+00:00', 'GIT_COMMITTER_DATE': '2026-10-23T10:00:00+00:00'}


def _load(name, alias=None):
    s = importlib.util.spec_from_file_location(alias or name.replace('-', '_'), os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


R2 = _load('r2lib')
case, jdump, rb, wb, fwd, git, gtext, sha256, blob_id = R2.case, R2.jdump, R2.rb, R2.wb, R2.fwd, R2.git, R2.gtext, R2.sha256, R2.blob_id
build, vb, CHK, DSA, MEV, LIB, R1M = R2.build, R2.vb, R2.CHK, R2.DSA, R2.MEV, R2.LIB, R2.R1M
CAND = None


def norm(s):
    return re.sub(r'\s+', ' ', s).strip()


def gitenv(repo, *args, inp=None):
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    env.update(FIXED_ENV)
    env['GIT_OPTIONAL_LOCKS'] = '0'
    p = subprocess.run(['git', '-C', repo] + list(args), capture_output=True, input=inp, env=env)
    return p.returncode, p.stdout, p.stderr


# ------------------------------------------------------------------ q0 environment, identities, exact delta
CAT_MARKS = {'1': ['(a) **autoridades:**', '(b) **objetos versionados', '(c) **artefactos custodiados:**', '(d) **instrumento:**'],
             '2': ['(a) cada entrada, por su blob', '(b) por su blob en `Target.commit`', '(c) por el blob Git', '(d) por la regla de A-6;'],
             '5': ['(a) una entrada de (a)', '(b) sin el objeto exacto', '(c) sin un contrato', '(d) la regla de A-6 sobre la vía']}


def a71_segments(text):
    """{(point, category or '-'): normalized text} of the A7-1 quotation (§3.2); the categories of points 1, 2 and 5 start at their literal
    beginnings (a bare «(a) » also occurs inside the text)."""
    s32 = text[text.index('### 3.2 A7-1'):text.index('### 3.3')]
    q = norm(' '.join(re.sub(r'^>\s?', '', x) for x in s32.split(chr(10)) if x.startswith('>')))
    marks = [(m.start(), m.group(1)) for m in re.finditer(r'(?<![\d(])(\d)\. \*\*', q)]
    out = {('0', '-'): q[:marks[0][0]]}
    for k, (pos, n) in enumerate(marks):
        seg = q[pos:marks[k + 1][0] if k + 1 < len(marks) else len(q)]
        if n in CAT_MARKS:
            cps, start = [], 0
            for mk in CAT_MARKS[n]:
                i = seg.index(norm(mk), start)
                cps.append(i)
                start = i + 1
            out[(n, '-')] = seg[:cps[0]]
            for i, c in enumerate('abcd'):
                out[(n, c)] = seg[cps[i]:cps[i + 1] if i < 3 else len(seg)]
        else:
            out[(n, '-')] = seg
    return out


def section_of(lines, j):
    for k in range(j, -1, -1):
        if lines[k].startswith('## ') or lines[k].startswith('### '):
            return lines[k]
    return 'HEADER'


ALLOWED_SECTIONS = ('HEADER', '## 2. Fuentes', '### 3.3 Categorías de entrada', '### 3.4 Relación con A-6', '### 3.6 Correcciones de la ronda 2',
                    '### 3.7 Corrección de la ronda 3', '## 4. Semántica', '## 5. Materialidad', '## 6. No cambia producción', '## 8. Preguntas',
                    '## 10. Evidencia', '## 11. Estado de revisión', '## Anexo')


def q0(P):
    cases = []
    head = gtext(W, 'rev-parse', 'HEAD')
    cases.append(case('ENV-W', 'HEAD del worktree de I-62 (solo lectura)', BASE, head, head == BASE))
    cb = rb(CAND)
    cases.append(case('CAND', 'candidata A-7 ronda 3 leída por completo', 'LF, sin CR', 'blob %s, %d bytes, %d líneas' % (blob_id(cb), len(cb), cb.count(b'\n')),
                      b'\r' not in cb))
    pub = gtext(W, 'rev-parse', 'HEAD:' + A7P)
    cases.append(case('PREV', 'ronda 2 en la base (la que la ronda 3 corrige en el mismo archivo)', PREV_BLOB, pub, pub == PREV_BLOB))
    old = git(W, 'cat-file', 'blob', PREV_BLOB)[1].decode('utf-8')
    new = cb.decode('utf-8')
    so, sn = a71_segments(old), a71_segments(new)
    changed = sorted(k for k in set(so) | set(sn) if so.get(k) != sn.get(k))
    cases.append(case('DELTA-A71', 'texto normalizado de cada punto y categoría de A7-1 frente a la ronda 2 (§96, puntos 2 y 3)',
                      'solo cambian los puntos 1 (c), 2 (c) y 5 (c); el alcance, los rótulos, (a), (b), (d) y los puntos 3, 4, 6, 7 y 8 sin cambio',
                      'segmentos cambiados: %s (de %d)' % (', '.join('%s (%s)' % k for k in changed), len(sn)),
                      changed == [('1', 'c'), ('2', 'c'), ('5', 'c')], Segments=sorted('%s%s' % k for k in sn)))
    a, b = old.split('\n'), new.split('\n')
    hunks = []
    for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes():
        if tag == 'equal':
            continue
        secs = sorted(set(section_of(b, j) for j in range(j1, j2) if b[j].strip()) | (set([section_of(a, i1)]) if j1 == j2 else set()))
        hunks.append({'Old': [i1 + 1, i2], 'New': [j1 + 1, j2], 'Sections': secs})
    outside = sorted(set(s for h in hunks for s in h['Sections'] if not s.startswith('### 3.2') and not any(s.startswith(x) for x in ALLOWED_SECTIONS)))
    cases.append(case('DELTA-SECTIONS', 'tramos del diff fuera de §3.2, por sección', 'solo las secciones descriptivas que declara §3.7: cabecera, §2, §3.3, §3.4, '
                      '§3.7, §4, §5, §6, §8, §10, §11 y el Anexo', '%d tramos; secciones: %s; fuera de lo declarado: %s' % (
                          len(hunks), ', '.join(sorted(set(s[:30] for h in hunks for s in h['Sections']))), ', '.join(outside) or 'ninguna'),
                      not outside, Hunks=hunks))
    rc, out = git(W, 'ls-tree', '-r', '-z', 'HEAD', 'docs/automation/evidence/I-62-A7/')
    tree = {}
    for rec in out.split(b'\0'):
        if rec:
            meta, name = rec.split(b'\t', 1)
            tree[name.decode('utf-8')] = meta.split()[2].decode('ascii')
    cases.append(case('R3-ABSENT', 'ningún archivo de r3/ publicado en la base', 'I-62-A7/r3/ vacío', '%d archivos' % sum(1 for n in tree if '/r3/' in n),
                      not any('/r3/' in n for n in tree)))
    man = git(W, 'cat-file', 'blob', 'HEAD:docs/automation/evidence/I-62-A7/r2/MANIFEST.sha256')[1].decode('ascii')
    rows = [l.split('  ', 1) for l in man.splitlines() if l.strip()]
    bad = [r for h, r in rows if sha256(git(W, 'cat-file', 'blob', 'HEAD:docs/automation/evidence/I-62-A7/r2/' + r)[1]) != h]
    cases.append(case('R2-EVID', 'evidencia de la ronda 2 (I-62-A7/r2/) en la base frente a su MANIFEST.sha256', 'idéntica (la ronda 3 no la toca)',
                      '%d entradas; discrepancias: %s' % (len(rows), ', '.join(bad) or 'ninguna'), not bad and len(rows) >= 48))
    rv = 'docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/'
    t1 = gtext(W, 'rev-parse', '3f912c84:' + rv)
    t2 = gtext(W, 'rev-parse', 'HEAD:' + rv)
    cases.append(case('R2-REVIEW', 'custodia de la revisión de la ronda 2 (output.json, audit.json y su evidencia) en la base', 'el árbol de 3f912c84, sin cambio (§96, punto 1)',
                      'árbol %s (en 3f912c84: %s)' % (t2[:12], t1[:12]), t1 == t2 and len(t1) == 40))
    for name in REUSED_SAME:
        got = sha256(rb(os.path.join(HERE, name)))
        want = sha256(git(W, 'cat-file', 'blob', 'HEAD:docs/automation/evidence/I-62-A7/r2/' + name)[1])
        cases.append(case('REUSE-' + name, 'copia byte a byte de la pieza de la ronda 2 (sin cambio en la ronda 3)', 'SHA-256 ' + want[:16], 'SHA-256 ' + got[:16], got == want))
    for name, r2sha in sorted(R2_PIECES.items()):
        cur = name.replace('-2.schema', '-3.schema')
        got = sha256(rb(os.path.join(HERE, cur)))
        same = got == r2sha
        cases.append(case('PIECE-' + cur, 'pieza del procedimiento (A7-1, punto 7): ronda 2 → ronda 3', 'cambia solo si depende del registro de rebase '
                          '(constructor, validador, formato, designate_a7); derive_r4.py sin cambio',
                          '%s… → %s… (%s)' % (r2sha[:12], got[:12], 'sin cambio' if same else 'cambia'), same == (cur == 'derive_r4.py')))
    for blob, label in ((R2.INSTR, 'instrumento'), (R2.ACC, 'aceptante')):
        rc, bb = git(W, 'cat-file', 'blob', blob)
        cases.append(case('PIN-' + label, '%s desde su objeto Git, sin modificar' % label, 'blob ' + blob, 'blob recalculado %s' % blob_id(bb), blob_id(bb) == blob))
    sv = dict((k, v) for k, v in P['SchemaViolations'].items() if v)
    known = dict((k, v) for k, v in sv.items() if k.startswith('relay-facts-') and v == ['#/RunId pattern'])
    cases.append(case('INPUTS', 'designaciones a7-designation/3 de las invocaciones del banco (esquema a7-designation-3.schema.json) y hechos del relevo '
                      'sintéticos (relay-record/v2)', 'todas válidas salvo el patrón de RunId de los controles del banco, ya conocido (decisiones §92, punto 6)',
                      '%d archivos; con violaciones: %s' % (len(P['SchemaViolations']), ', '.join(sv) or 'ninguno'), sv == known))
    return {'Phase': 'q0-env', 'Cases': cases, 'Prep': P, 'CandidateBlob': blob_id(cb)}


# ------------------------------------------------------------------ q8 the RebaseMap of the 16.7 reverification
def rebase_fixture(out):
    """A real rebase of the V repository (copy of work/deliv): main advances one commit (only docs/WORKFLOW.md changes, so the reads at
    MainSha_eval change), the 20 commits of the unit branch are rebased onto it without conflicts, and the session records the rebase as a
    relay-record/v2 with Phase REBASE and its RebaseMap (B.8.7 fields)."""
    T8 = os.path.join(WORK, 'r3', 'rebase-repo')
    R1M.rmtree(T8)
    os.makedirs(os.path.dirname(T8), exist_ok=True)
    p = subprocess.run(['git', 'clone', '-q', '--no-hardlinks', '-c', 'core.autocrlf=false', fwd(os.path.join(WORK, 'deliv')), fwd(T8)], capture_output=True)
    assert p.returncode == 0, p.stderr
    gitenv(T8, 'hash-object', '-w', '--no-filters', '--stdin', inp=git(W, 'cat-file', 'blob', R2.INSTR)[1])
    V = json.loads(rb(os.path.join(HERE, 'designations', 'V.json')).decode('utf-8'))
    ro = dict((a['Role'], a) for a in V['Artifacts'])
    K = json.loads(git(T8, 'cat-file', 'blob', ro['CONTRACT']['Blob'])[1].decode('utf-8'))
    D = json.loads(rb(ro['DELEG']['Custody']).decode('utf-8-sig'))
    H = json.loads(rb(ro['H']['Custody']).decode('utf-8-sig'))
    mb, cur = K['MainSha'], H['CurrentSha']
    assert gitenv(T8, 'checkout', '-q', '--detach', mb)[0] == 0
    wf = os.path.join(T8, 'docs', 'WORKFLOW.md')
    with open(wf, 'ab') as fh:
        fh.write(b'\n<!-- avance de main posterior a la entrega (preflight de A-7 ronda 3, sintetico) -->\n')
    gitenv(T8, 'add', 'docs/WORKFLOW.md')
    assert gitenv(T8, 'commit', '-q', '-m', 'main: avance posterior a la entrega (preflight A-7 r3)')[0] == 0
    m2 = gitenv(T8, 'rev-parse', 'HEAD')[1].decode().strip()
    orig = gitenv(T8, 'rev-list', '--reverse', '%s..%s' % (mb, cur))[1].decode().split()
    assert gitenv(T8, 'checkout', '-q', '--detach', cur)[0] == 0
    rc, so, se = gitenv(T8, 'rebase', '-q', '--onto', m2, mb)
    assert rc == 0, se
    img_tip = gitenv(T8, 'rev-parse', 'HEAD')[1].decode().strip()
    imgs = gitenv(T8, 'rev-list', '--reverse', '%s..%s' % (m2, img_tip))[1].decode().split()
    assert len(imgs) == len(orig)
    gitenv(T8, 'update-ref', 'refs/heads/fx/u1-rebased', img_tip)
    gitenv(T8, 'update-ref', 'refs/heads/main-advanced', m2)

    def pid(c):
        """git patch-id --stable of the commit, or None for an empty commit (the claim commit of the unit has no diff)."""
        o = gitenv(T8, 'patch-id', '--stable', inp=gitenv(T8, 'show', c)[1])[1].decode().split()
        return o[0] if o else None
    image = dict(zip(orig, imgs))
    commits = [{'OriginalSha': o, 'ImageSha': i, 'PatchId': pid(i), 'PatchIdEqual': pid(o) == pid(i)} for o, i in zip(orig, imgs)]
    ar = V['AuthorityRevision']
    unit_eq = all(gitenv(T8, 'rev-parse', '%s:%s' % (ar, pth))[1] == gitenv(T8, 'rev-parse', '%s:%s' % (image[ar], pth))[1]
                  for pth in ('docs/automation/decisions/FX-U1.md', 'docs/automation/state/FX-U1.yml'))
    rmap = {'MainBeforeSha': mb, 'MainAfterSha': m2, 'BranchBeforeSha': cur, 'BranchAfterSha': img_tip, 'MaterializationCloseSha': ar,
            'MaterializationCloseImageSha': image[ar], 'AuthorityRevision': ar, 'AuthorityRevisionImageSha': image[ar], 'UnitDocBlobsEqual': unit_eq,
            'UnitChangeSectionsEqual': unit_eq, 'BaseSha': D['BaseSha'], 'BaseImageSha': image[D['BaseSha']], 'ChainBaseSha': D['ChainBaseSha'],
            'ChainBaseImageSha': image[D['ChainBaseSha']], 'Commits': commits,
            'StateFields': [{'Field': 'chains[T1].chain_base_sha', 'OriginalSha': D['ChainBaseSha'], 'ImageSha': image[D['ChainBaseSha']]}],
            'CiRuns': [{'OriginalSha': '53436cb20adf1fe87dc91b0382dd3123a1fac44f', 'GhRunId': 37949800001}], 'Unmapped': [], 'ConflictPaths': [],
            'CoordinatorDecision': None}
    rec = R2.relay_facts('REBASE', RB_RUN, img_tip, None, m2, 'rebase-record.json', None)
    rec.update({'RunId': RB_RUN, 'Phase': 'REBASE', 'RemoteFacts': None, 'RebaseMap': rmap,
                'Notes': ['Rebase de 16.7 posterior a la entrega, sin conflictos: main avanza y la rama de la unidad se rebasa (sintetico, preflight de A-7 r3).']})
    inp = os.path.join(HERE, 'inputs-rebase')
    R1M.rmtree(inp)
    recp = wb(os.path.join(inp, 'rebase-record.json'), R1M.jbytes(rec))
    green = rb(ro['TRX_CURRENT']['Custody'])
    trx_img = wb(os.path.join(inp, 'fixture-tests-current-image.trx'), green)
    tpath = V['Invocation']['Target']['Path']
    tblob = gitenv(T8, 'rev-parse', '%s:%s' % (img_tip, tpath))[1].decode().strip()
    relay = R2.relay_facts('RV', RV_RUN, img_tip, '53436cb20adf1fe87dc91b0382dd3123a1fac44f', m2, 'controller-output.json', sha256(rb(ro['TRX_RED']['Custody'])))
    relp = wb(os.path.join(inp, 'relay-facts-RV.json'), R1M.jbytes(relay))
    rv = json.loads(json.dumps(V))
    rv['Case'], rv['Repo'] = 'RV', fwd(T8)
    rv['Invocation'].update({'RunId': RV_RUN, 'Kind': 'REVERIFICATION', 'Target': {'Commit': img_tip, 'Path': tpath, 'Blob': tblob}})
    rv['RunPath'] = 'artifacts/orchestration/FX-U1/T1/0/%s/' % RV_RUN
    arts = []
    for a in rv['Artifacts']:
        if a['Role'] == 'TRX_CURRENT':
            a = {'Role': 'TRX_CURRENT', 'Path': 'artifacts/ci/R37949808975/fixture-tests.trx', 'Blob': blob_id(green), 'Custody': fwd(trx_img)}
        if a['Role'] == 'RELAY':
            a = {'Role': 'RELAY', 'Path': 'artifacts/orchestration/FX-U1/T1/0/%s/relay-facts.json' % RV_RUN, 'Blob': blob_id(rb(relp)), 'Custody': fwd(relp)}
        arts.append(a)
    arts.append({'Role': 'REBASE', 'Path': 'artifacts/orchestration/FX-U1/T1/0/%s/rebase-record.json' % RB_RUN, 'Blob': blob_id(rb(recp)), 'Custody': fwd(recp),
                 'RunId': RB_RUN})
    rv['Artifacts'] = arts
    dp = R2.jdump(os.path.join(HERE, 'designations-r3', 'RV.json'), rv)
    fx = {'Repo': fwd(T8), 'MainBefore': mb, 'MainAfter': m2, 'BranchBefore': cur, 'BranchAfter': img_tip, 'Commits': len(commits),
          'PatchIdsEqual': all(c['PatchIdEqual'] for c in commits), 'UnitDocsEqual': unit_eq,
          'MainChange': gitenv(T8, 'diff', '--name-only', mb, m2)[1].decode().split(), 'RebaseRecord': fwd(recp), 'Designation': fwd(dp),
          'K': K, 'D': D, 'H': H, 'Image': image}
    return T8, rv, fx


def q8():
    cases = []
    out = R2.out_dir('q8')
    T8, rv, fx = rebase_fixture(out)
    rschema = json.loads(git(W, 'cat-file', 'blob', '%s:docs/automation/agent-execution/schemas/relay-record.v2.schema.json' % BASE)[1].decode('utf-8'))
    dschema = json.loads(rb(os.path.join(HERE, 'a7-designation-3.schema.json')).decode('utf-8'))
    rec = json.loads(rb(fx['RebaseRecord']).decode('utf-8'))
    sv_rec, sv_des = R2.schema_ok(rec, rschema), R2.schema_ok(rv, dschema)
    cases.append(case('RB-FIXTURE', 'rebase real del repositorio de V: main avanza un commit (solo docs/WORKFLOW.md) y los commits de la rama de la unidad se '
                      'rebasan sin conflictos; registro de rebase relay-record/v2 con Phase REBASE', 'patch-id iguales; documentos de la unidad iguales en la AR y '
                      'su imagen; registro y designación válidos contra sus esquemas', 'main %s → %s (%s); %d commits; patch-id iguales: %s; docs de la unidad iguales: '
                      '%s; esquema del registro: %s; esquema de la designación: %s' % (fx['MainBefore'][:8], fx['MainAfter'][:8], ', '.join(fx['MainChange']),
                                                                                         fx['Commits'], fx['PatchIdsEqual'], fx['UnitDocsEqual'], sv_rec or 'válido',
                                                                                         sv_des or 'válida'),
                      fx['MainChange'] == ['docs/WORKFLOW.md'] and fx['PatchIdsEqual'] and fx['UnitDocsEqual'] and not sv_rec and not sv_des, Fixture=dict(
                          (k, v) for k, v in fx.items() if k not in ('K', 'D', 'H', 'Image'))))
    K = fx['K']
    rid = lambda d: next(a for a in d['Artifacts'] if a['Role'] == 'REBASE')

    def variant(f):
        d = json.loads(json.dumps(rv))
        f(d)
        return d

    def rec_variant(name, f, register=True):
        """A substituted rebase record: the bytes change; with register the designation registers the new blob."""
        r = json.loads(json.dumps(rec))
        f(r)
        pth = wb(os.path.join(out, 'rebase-record-%s.json' % name), R1M.jbytes(r))
        d = json.loads(json.dumps(rv))
        a = rid(d)
        a['Custody'] = fwd(pth)
        if register:
            a['Blob'] = blob_id(rb(pth))
        return d
    # ---- positive
    E, rep, err = build(rv)
    assert E is not None, err
    Ep = wb(os.path.join(out, 'E-rv.json'), MEV.serialize(E))
    v = CHK.check(rv, E, i62_repo=W, i62_commit=BASE)
    dchk = DSA.check(rv, E)
    cl = R2.derive(rv, Ep, out, 'rv')
    rb_entries = [e for e in E['CanonicalInputs'] if e['Path'] == rid(rv)['Path']]
    Vdes = json.loads(rb(os.path.join(HERE, 'designations', 'V.json')).decode('utf-8'))
    EV, repV, errV = build(Vdes)
    cases.append(case('RB-POS', 'reverificación de 16.7 con su registro de rebase presente, verificado y coherente',
                      'cierre construido; MainSha_eval = MainAfterSha del RebaseMap (≠ K.MainSha); la entrada del registro en CanonicalInputs; validador acepta; '
                      'designate_a7 válida; derivación del cierre del aceptante con C6 sin nombre para el registro; EFF y EFF^1 = los de V',
                      'MainShaEval %s (%s); K.MainSha %s; entrada del registro: %s; validador: %s (fuente %s); designate_a7: %s; C6: %s; EFF %s / EFF^1 %s (V: %s / %s)' % (
                          rep['Revisions']['MainShaEval'][:8], rep.get('MainShaEvalSource'), K['MainSha'][:8], len(rb_entries),
                          'acepta' if v['Accepted'] else 'rechaza %s' % vb(v), v.get('MainShaEvalSource'), 'válida' if dchk['Ok'] else dchk['Problems'],
                          R2.files_of(cl) if 'Closure' in cl else cl, rep['Revisions']['EFF'][:8], (rep['Revisions']['EFF^1'] or '-')[:8],
                          repV['Revisions']['EFF'][:8], (repV['Revisions']['EFF^1'] or '-')[:8]),
                      rep['Revisions']['MainShaEval'] == fx['MainAfter'] != K['MainSha'] and len(rb_entries) == 1 and v['Accepted'] and dchk['Ok'] and
                      'Closure' in cl and 'REBASE' not in [f['Name'] for f in cl['Closure']['Files']] and
                      rep['Revisions']['EFF'] == repV['Revisions']['EFF'] and rep['Revisions']['EFF^1'] == repV['Revisions']['EFF^1'], Closure=cl.get('Sha256')))
    wf_at_m2 = gitenv(T8, 'rev-parse', '%s:docs/WORKFLOW.md' % fx['MainAfter'])[1].decode().strip()
    wf_at_k = gitenv(T8, 'rev-parse', '%s:docs/WORKFLOW.md' % K['MainSha'])[1].decode().strip()
    # the read of Evaluate at MainSha_eval has the same (path, blob) as the (b) entry at Target.commit (the image carries the advance of main),
    # so the builder pins the pair once; the validator labels it with the MainSha_eval read
    wf_cls = [c for c in v['Classified'] if c['Path'] == 'docs/WORKFLOW.md' and c['Blob'] == wf_at_m2]
    wf_labels = sorted(set(l for c in wf_cls for l in c['Labels']))
    cases.append(case('RB-MSE-READS', 'lecturas de (a) en MainSha_eval en la reverificación (Evaluate: WORKFLOW.md y AUTOMATION_PLAN.md)',
                      'WORKFLOW.md fijado con su blob en MainAfterSha (distinto del de K.MainSha) y clasificado por el validador como la lectura de Evaluate en '
                      'MainSha_eval', 'par (WORKFLOW.md, %s) en el cierre: %s; etiquetas: %s; en K.MainSha el blob es %s' % (
                          wf_at_m2[:8], bool(wf_cls), ', '.join(wf_labels), wf_at_k[:8]),
                      bool(wf_cls) and 'a:16.13-EVALUATE@MainSha_eval' in wf_labels and wf_at_m2 != wf_at_k))
    Ebad = json.loads(json.dumps(E))
    for lst in ('CanonicalInputs', 'AllowedTransitiveInputs'):
        for e in Ebad[lst]:
            if e['Path'] == 'docs/WORKFLOW.md' and e['Blob'] == wf_at_m2:
                e['Blob'] = wf_at_k
    vbad = CHK.check(rv, Ebad, i62_repo=W, i62_commit=BASE)
    cases.append(case('RB-MSE-KMAIN', 'cierre de la reverificación con las lecturas de MainSha_eval fijadas en K.MainSha (como si no hubiera RebaseMap)',
                      'el validador lo rechaza (falta la entrada en MainAfterSha)', 'validador: %s; faltan %d' % ('acepta' if vbad['Accepted'] else 'rechaza', len(vbad['Missing'])),
                      not vbad['Accepted'] and vbad['Missing']))
    dm = variant(lambda d: d.update({'MainShaEval': K['MainSha']}))
    Em, repm, errm = build(dm)
    vm = CHK.check(dm, E, i62_repo=W, i62_commit=BASE)
    cases.append(case('RB-MSE-DESIGNATED', 'designación que fija MainShaEval = K.MainSha (MainSha_eval designado en lugar de derivado)',
                      'el formato lo rechaza: el constructor no da cierre, el validador da un problema, el esquema lo rechaza',
                      'constructor: %s; validador: %s; esquema: %s' % ((errm or {}).get('Code'), [x for x in vm['Problems'] if 'MainShaEval' in x][:1],
                                                                       R2.schema_ok(dm, dschema)), errm and errm['Code'] == 'DESIGNATION_FORMAT' and
                      not vm['Accepted'] and R2.schema_ok(dm, dschema)))

    def no_closure(cid, what, d, want_code='NO_CLOSURE', why=None, E_for_validator=E):
        Ex, repx, errx = build(d)
        vx = CHK.check(d, E_for_validator, i62_repo=W, i62_commit=BASE)
        detail = (errx or {}).get('Detail') or ''
        ok = Ex is None and errx['Code'] == want_code and (why is None or why in detail) and not vx['Accepted']
        cases.append(case(cid, what, 'sin cierre (%s%s) y el validador rechaza el cierre de RB-POS con esa designación' % (want_code, ': ' + why if why else ''),
                          'constructor: %s %s; validador: %s' % ((errx or {}).get('Code'), detail[:150], 'acepta' if vx['Accepted'] else 'rechaza (%s)' % (
                              '; '.join((vx['NoClosureExpected'] + vx['Problems'])[:2])[:150])), ok))
    no_closure('RB-ABSENT', 'reverificación sin registro de rebase designado (obligatorio)', variant(lambda d: d['Artifacts'].remove(rid(d))),
               why='designates no rebase record')
    no_closure('RB-ABSENT-FILE', 'registro designado sin bytes custodiados', variant(lambda d: rid(d).update({'Custody': fwd(os.path.join(out, 'no-existe.json'))})),
               why='absent, unregistered')
    no_closure('RB-BLOB-REGISTERED', 'blob registrado alterado (un dígito)', variant(lambda d: rid(d).update({'Blob': rid(d)['Blob'][:-1] + ('0' if rid(d)['Blob'][-1] != '0' else '1')})),
               why='absent, unregistered')
    alt = wb(os.path.join(out, 'rebase-record-byte.json'), rb(fx['RebaseRecord']) + b' ')
    no_closure('RB-BLOB-BYTES', 'bytes custodiados alterados (un byte más) con el blob registrado del original',
               variant(lambda d: rid(d).update({'Custody': fwd(alt)})), why='absent, unregistered')
    no_closure('RB-SUBST-NOADVANCE', 'registro sustituido (blob registrado): MainAfterSha = MainBeforeSha (main no avanza; MainSha_eval sería K.MainSha)',
               rec_variant('noadvance', lambda r: r['RebaseMap'].update({'MainAfterSha': r['RebaseMap']['MainBeforeSha']})), why='MainAfterSha is not a commit after')
    no_closure('RB-SUBST-OTHERMAIN', 'registro sustituido (blob registrado): MainAfterSha = un commit de main del que no desciende Target.commit',
               rec_variant('othermain', lambda r: r['RebaseMap'].update({'MainAfterSha': '930288c51ab1b5dbd370c8ebf9206d78670b89a0'})), why='MainAfterSha is not a commit after')
    no_closure('RB-SUBST-MAINBEFORE', 'registro de otro rebase (blob registrado): MainBeforeSha ≠ K.MainSha',
               rec_variant('mainbefore', lambda r: r['RebaseMap'].update({'MainBeforeSha': '930288c51ab1b5dbd370c8ebf9206d78670b89a0'})), why='MainBeforeSha of the RebaseMap is not K.MainSha')
    no_closure('RB-OTHER-TASK', 'registro de otra corrida: otra tarea (TaskId T2)', rec_variant('task', lambda r: r.update({'TaskId': 'T2'})), why='another task or attempt')
    no_closure('RB-OTHER-ATTEMPT', 'registro de otra corrida: otro intento (Attempt 1)', rec_variant('attempt', lambda r: r.update({'Attempt': 1})), why='another task or attempt')
    no_closure('RB-OTHER-DELIVERY', 'registro de otra entrega: Commits no asigna el CurrentSha de la entrega', rec_variant('delivery', lambda r: [
        c.update({'OriginalSha': 'b6d294ede5e1ce7e9412447629b2b9d77ffec0d4'}) for c in r['RebaseMap']['Commits'] if c['OriginalSha'] == fx['BranchBefore']]),
               why='does not map the CurrentSha')
    no_closure('RB-RUNID-RECORD', 'RunId del registro distinto del registrado en la designación', rec_variant('runid', lambda r: r.update({'RunId': 'R20261023T100000Z-0b02'})),
               why='RunId of the rebase record is not the registered one')
    no_closure('RB-RUNID-DIR', 'el directorio de la ruta no es el RunId registrado', variant(lambda d: rid(d).update({'Path': 'artifacts/orchestration/FX-U1/T1/0/%s/rebase-record.json' % RV_RUN})),
               why='attempt directory')
    no_closure('RB-RUNID-REUSED', 'el registro usa el RunId de la reverificación (16.6: un RunId propio)', variant(lambda d: (
        rid(d).update({'RunId': RV_RUN, 'Path': 'artifacts/orchestration/FX-U1/T1/0/%s/rebase-record.json' % RV_RUN}),
        rid(d).update({'Custody': fwd(wb(os.path.join(out, 'rebase-record-reused.json'), R1M.jbytes(dict(rec, RunId=RV_RUN)))),
                       'Blob': blob_id(R1M.jbytes(dict(rec, RunId=RV_RUN)))}))), why='that of the reverification')
    no_closure('RB-RUNID-FORMAT', 'RunId registrado sin la forma de 16.6', variant(lambda d: rid(d).update({'RunId': 'rebase-1'})), want_code='DESIGNATION')
    no_closure('RB-RUNID-OTHER-ROLE', 'RunId registrado en otro papel (solo el registro de rebase lo lleva)', variant(lambda d: next(
        a for a in d['Artifacts'] if a['Role'] == 'RELAY').update({'RunId': RV_RUN})), want_code='DESIGNATION')
    for cid, what, f, why in (('RB-CONFLICTS', 'RebaseMap con rutas en conflicto', lambda r: r['RebaseMap'].update({'ConflictPaths': ['src/Fixture.Lib/Calculator.cs']}), 'conflict paths'),
                              ('RB-UNMAPPED', 'RebaseMap con entradas sin mapear', lambda r: r['RebaseMap'].update({'Unmapped': ['x']}), 'conflict paths or unmapped'),
                              ('RB-PATCHID', 'el commit de la entrega con patch-id distinto', lambda r: [c.update({'PatchIdEqual': False}) for c in r['RebaseMap']['Commits']
                                                                                                    if c['OriginalSha'] == fx['BranchBefore']], 'equal patch-id'),
                              ('RB-NOIMAGE', 'un commit sin imagen', lambda r: r['RebaseMap']['Commits'][0].update({'ImageSha': None}), 'commit without image'),
                              ('RB-AR', 'AuthorityRevision del RebaseMap distinta de la del contrato', lambda r: r['RebaseMap'].update({'AuthorityRevision': '930288c51ab1b5dbd370c8ebf9206d78670b89a0'}),
                               'AuthorityRevision'),
                              ('RB-UNITDOC', 'igualdad de 16.7 de los UNIT_DOC no registrada', lambda r: r['RebaseMap'].update({'UnitDocBlobsEqual': False}), 'AuthorityRevision'),
                              ('RB-BASESHA', 'BaseSha del RebaseMap distinto del de la delegación', lambda r: r['RebaseMap'].update({'BaseSha': fx['MainBefore']}), 'BaseSha'),
                              ('RB-PHASE', 'registro con Phase WORK', lambda r: r.update({'Phase': 'WORK'}), 'Phase REBASE'),
                              ('RB-NOMAP', 'registro sin RebaseMap', lambda r: r.update({'RebaseMap': None}), 'no RebaseMap')):
        no_closure(cid, 'registro incoherente: ' + what, rec_variant(cid.lower(), f), why=why)
    no_closure('RB-NO-H', 'reverificación sin la entrega (la coherencia del registro no se puede comprobar)', variant(lambda d: d['Artifacts'].remove(
        next(a for a in d['Artifacts'] if a['Role'] == 'H'))), why='without the delivery')
    # ---- misuse in an ordinary VERIFY, a control and the measurement
    for c in ('V', 'N4', 'M'):
        bd = json.loads(rb(os.path.join(HERE, 'designations', '%s.json' % c)).decode('utf-8'))
        Eb, repb, errb = build(bd)
        d = json.loads(json.dumps(bd))
        d['Artifacts'].append(dict(rid(rv), Path='artifacts/orchestration/%s/%s/%s/%s/rebase-record.json' % (
            bd['Invocation']['UnitId'], bd['Invocation']['TaskId'], bd['Invocation']['Attempt'], RB_RUN)))
        Ex, repx, errx = build(d)
        same = Ex is not None and Eb is not None and MEV.serialize(Ex) == MEV.serialize(Eb)
        ab = [a for a in (repx or {}).get('Absences', []) if a['Role'] == 'REBASE']
        vx = CHK.check(d, Ex, i62_repo=W, i62_commit=BASE) if Ex else {'Accepted': False}
        Einj = json.loads(json.dumps(Ex)) if Ex else None
        if Einj:
            Einj['CanonicalInputs'].append({'Path': d['Artifacts'][-1]['Path'], 'Blob': d['Artifacts'][-1]['Blob']})
        vinj = CHK.check(d, Einj, i62_repo=W, i62_commit=BASE) if Einj else {'Accepted': True, 'Foreign': []}
        dinj = DSA.check(d, Einj) if Einj else {'Problems': []}
        kind = bd['Invocation'].get('Kind', 'VERIFY')
        cases.append(case('RB-MISUSE-%s' % c, '%s (%s) con un registro de rebase designado' % (c, kind),
                          'no se admite: no entra en el cierre (E idéntico al de %s sin él), ausencia NOT_ADMITTED registrada, MainSha_eval = K.MainSha; con la entrada '
                          'añadida a mano, el validador la marca ajena y designate_a7 la rechaza' % c,
                          'E igual al de %s: %s; ausencia: %s; MainSha_eval %s; validador sobre E: %s; con la entrada inyectada: %s (ajenas %d), designate_a7 %s' % (
                              c, same, ab[0]['Cause'][:60] if ab else '-', (repx or {}).get('Revisions', {}).get('MainShaEval', '-')[:8],
                              'acepta' if vx['Accepted'] else 'rechaza', 'acepta' if vinj['Accepted'] else 'rechaza', len(vinj.get('Foreign', [])),
                              [p_['Code'] for p_ in dinj['Problems']]),
                          same and ab and vx['Accepted'] and not vinj['Accepted'] and vinj.get('Foreign') and
                          any(p_['Code'] == 'REBASE_NOT_ADMITTED' for p_ in dinj['Problems'])))
    # ---- invented or extra entries
    Ex = json.loads(json.dumps(E))
    Ex['CanonicalInputs'].append({'Path': 'artifacts/orchestration/FX-U1/T1/0/R20261023T100500Z-0b03/rebase-record.json', 'Blob': rid(rv)['Blob']})
    vx, dx = CHK.check(rv, Ex, i62_repo=W, i62_commit=BASE), DSA.check(rv, Ex)
    cases.append(case('RB-EXTRA-RECORD', 'cierre de la reverificación con un segundo registro de rebase no designado', 'validador: ajeno; designate_a7: UNDESIGNATED_INPUT',
                      'validador %s (ajenas %d); designate_a7 %s' % ('acepta' if vx['Accepted'] else 'rechaza', len(vx['Foreign']), [p_['Code'] for p_ in dx['Problems']]),
                      not vx['Accepted'] and vx['Foreign'] and any(p_['Code'] == 'UNDESIGNATED_INPUT' for p_ in dx['Problems'])))
    Ey = json.loads(json.dumps(E))
    Ey['AllowedTransitiveInputs'].append({'Path': 'docs/WORKFLOW.md', 'Blob': 'f' * 40, 'RequiredBy': Ey['AllowedTransitiveInputs'][0]['RequiredBy']})
    vy = CHK.check(rv, Ey, i62_repo=W, i62_commit=BASE)
    cases.append(case('RB-EXTRA-INVENTED', 'cierre con una entrada inventada (WORKFLOW.md con un blob que no es el de ninguna revisión de lectura)', 'el validador la marca ajena',
                      'validador %s (ajenas %d)' % ('acepta' if vy['Accepted'] else 'rechaza', len(vy['Foreign'])), not vy['Accepted'] and vy['Foreign']))
    dd = variant(lambda d: d['Artifacts'].append(dict(rid(d), Path=rid(d)['Path'].replace('rebase-record', 'rebase-record-2'))))
    Ed, repd, errd = build(dd)
    vdd = CHK.check(dd, E, i62_repo=W, i62_commit=BASE)
    cases.append(case('RB-EXTRA-ROLE', 'designación con dos registros de rebase (un artefacto como máximo por papel)', 'sin cierre (DESIGNATION); el validador da un problema',
                      'constructor %s; validador %s' % ((errd or {}).get('Code'), vdd['Problems'][:1]), errd and errd['Code'] == 'DESIGNATION' and not vdd['Accepted']))
    # ---- acceptor (A-6 unchanged): the derived closure of the reverification with the two TRX runs
    ro = dict((a['Role'], a) for a in rv['Artifacts'])
    runs = {}
    for name, a in (('trx-current', ro['TRX_CURRENT']), ('trx-red', ro['TRX_RED'])):
        runs[name], rcx, env = R2.instr_run(T8, out, name, ['trx', '--repo', fwd(T8), '--file', 'TRX=' + a['Custody'], '--sha256', 'TRX=' + sha256(rb(a['Custody'])),
                                                           '--filter-substring', 'Subtract'])
    acc = R2.accept_req(R2.minimal_request(T8, [runs['trx-current'], runs['trx-red']], cl, (ro['RELAY']['Custody'], sha256(rb(ro['RELAY']['Custody'])))), out, 'rv', [T8])
    cases.append(case('RB-ACCEPTOR', 'aceptante fijado (A-6 sin cambio) sobre las dos corridas trx con el cierre derivado de la reverificación',
                      'ACCEPT_FACTS: R9 MATCH (C6 fija los dos TRX), R10 MATCH (RelayFacts de (c)); el registro de rebase no entra en C6 ni lo lee el instrumento',
                      '%s rc %d; R9 %s; R10 %s; cero escrituras: %s' % (acc['Decision'], acc['Rc'], acc['Checks'].get('R9'), acc['Checks'].get('R10'), acc['ZeroWrite']),
                      acc['Decision'] == 'ACCEPT_FACTS' and acc['Checks'].get('R9') == 'MATCH' and acc['Checks'].get('R10') == 'MATCH', Acceptor=acc))
    b_wf = [e for e in rep['Entries'] if e['Path'] == 'docs/WORKFLOW.md' and e['Category'] == 'b']
    cases.append(case('RB-B-INCLUSION', 'informativo (Q-A7R3-03): (b) de la reverificación con el BaseSha de la delegación (la ronda 3 no cambia (b))',
                      'registrado: git diff --name-only BaseSha..Target.commit incluye la ruta que main cambió (docs/WORKFLOW.md), fijada en Target.commit; no es una lectura '
                      'fuera del cierre', '(b) WORKFLOW.md: %s; entradas (b): %d' % ([e['Blob'][:8] for e in b_wf], len([e for e in rep['Entries'] if e['Category'] == 'b'])),
                      len(b_wf) == 1 and b_wf[0]['Blob'] == wf_at_m2))
    return {'Phase': 'q8-rebasemap', 'Cases': cases, 'Fixture': dict((k, v) for k, v in fx.items() if k not in ('K', 'D', 'H', 'Image'))}


# ------------------------------------------------------------------ q6 compatibility (round-2 checks plus §96, punto 5)
def q6(R):
    res = R2.q6()
    cases = res['Cases']
    old = git(W, 'cat-file', 'blob', PREV_BLOB)[1].decode('utf-8')
    new = rb(CAND).decode('utf-8')
    so, sn = a71_segments(old), a71_segments(new)
    cases.append(case('COMPAT-INVALIDATOR-RULE', 'regla del invalidador (A7-1, punto 7) y piezas del procedimiento', 'texto del punto 7 igual al de la ronda 2; piezas recalculadas '
                      '(constructor, validador, formato y designate_a7 nuevas; derive_r4.py igual): ninguna medición previa que anular (A6-7 sin consumir)',
                      'punto 7 igual: %s; piezas: %s' % (so[('7', '-')] == sn[('7', '-')], ', '.join('%s %s…' % (k, v[:10]) for k, v in sorted(R['q5']['Identity'].items()))),
                      so[('7', '-')] == sn[('7', '-')] and R['q5']['Identity']['DerivationDeriveSha256'] == R2_PIECES['derive_r4.py']))
    sec6o, sec6n = old[old.index('## 6.'):old.index('## 7.')], new[new.index('## 6.'):new.index('## 7.')]
    orc = '- **Oráculos, presupuestos y autorizaciones.**'
    get = lambda s: norm(s[s.index(orc):s.index('- **Guardas y preflight:**')])
    cases.append(case('COMPAT-BUDGETS-ORACLES', 'presupuestos, oráculos de FX-02 y autorizaciones (A7-1, punto 8; §6)', 'el punto 8 y la viñeta de §6 sin cambio; V14 (D.3), A-6 '
                      '(A6-6, C-24) y RAE (§10) en sus blobs fijados', 'punto 8 igual: %s; viñeta de §6 igual: %s; blobs fijados: %s' % (
                          so[('8', '-')] == sn[('8', '-')], get(sec6o) == get(sec6n), all(c['Cumple'] for c in cases if c['Caso'].startswith('PIN-'))),
                      so[('8', '-')] == sn[('8', '-')] and get(sec6o) == get(sec6n) and all(c['Cumple'] for c in cases if c['Caso'].startswith('PIN-'))))
    owner = [c for c in cases if c['Caso'].startswith('OWNER-')]
    cases.append(case('COMPAT-OWNER', 'autorizaciones del Owner (líneas (a) y (b) de A-6 §7, decisiones §78, OD-2 y OD-2c)', 'sin cambio y no reescritas por A-7 r3',
                      '%d de %d líneas conformes' % (sum(1 for c in owner if c['Cumple']), len(owner)), owner and all(c['Cumple'] for c in owner)))
    q8c = dict((c['Caso'], c) for c in R['q8']['Cases'])
    q4ok = all(c['Cumple'] for c in R['q4']['Cases'])
    cases.append(case('COMPAT-EFF', 'EFF/EFF^1 (A-5 B1 y la reverificación)', 'el caso tipo B1 (EFF^1 distinto) pasa con las piezas de la ronda 3; en la reverificación EFF y EFF^1 se '
                      'derivan de MainSha_eval y coinciden con los de V (main avanza sin merge)', 'q4 (B1-like): %s; RB-POS: %s' % (
                          'conforme' if q4ok else 'no conforme', 'conforme' if q8c['RB-POS']['Cumple'] else 'no conforme'), q4ok and q8c['RB-POS']['Cumple']))
    a5 = git(W, 'cat-file', 'blob', 'HEAD:docs/initiatives/I-62-A-5.md')[1].decode('utf-8')
    cases.append(case('COMPAT-A5-R3', 'A-5 frente a la ronda 3 (decisiones §96, punto 5)', 'A-5 (blob ad94ceed) no regula la reverificación de 16.7 ni el RebaseMap; A-7 r3 lo declara '
                      'en §6', 'A-5 menciona RebaseMap: %s; reverificación: %s; nota de §6: %s' % ('RebaseMap' in a5, '16.7' in a5,
                                                                                                  'El registro de rebase solo entra en una reverificación de 16.7, que A-5 no regula.' in new),
                      'RebaseMap' not in a5 and 'El registro de rebase solo entra en una reverificación de 16.7, que A-5 no regula.' in new))
    cases.append(case('COMPAT-A6-R3', 'A-6 frente a la ronda 3', 'A-6 (blob 4bc919dd) sin cambio; A6-9 y derive_r4.py sin cambio: el registro de rebase sin nombre en C6 y el '
                      'aceptante acepta la reverificación (RB-ACCEPTOR); la precisión de A6-2, reglas 1 y 3, sin cambio de texto',
                      'A-6 en la base: %s; RB-ACCEPTOR: %s; derive_r4 igual: %s; punto 1 (c) conserva la precisión de A6-2, regla 1: %s' % (
                          gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-6.md')[:8], q8c['RB-ACCEPTOR']['Cumple'],
                          sha256(rb(os.path.join(HERE, 'derive_r4.py'))) == R2_PIECES['derive_r4.py'],
                          'además del instrumento de A6-2, regla 1, excepciones a la clase de §20.3.1' in sn[('1', 'd')]),
                      gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-6.md') == '4bc919dd4768ab81abc74b70d3a4a3821c07feb5' and q8c['RB-ACCEPTOR']['Cumple'] and
                      sha256(rb(os.path.join(HERE, 'derive_r4.py'))) == R2_PIECES['derive_r4.py'] and
                      'además del instrumento de A6-2, regla 1, excepciones a la clase de §20.3.1' in sn[('1', 'd')]))
    return res


# ------------------------------------------------------------------ q7 guards
def q7():
    tmp = os.path.join(WORK, 'guards-tmp-r3')
    R1M.rmtree(tmp)
    os.makedirs(tmp)
    res = {}
    for mode, outn in (('selftest', 'q7-guards-selftest.json'), ('preview', 'q7-guards-preview.json')):
        subprocess.run([sys.executable, '-I', os.path.join(HERE, 'a7-guards.py'), mode, '--cand', CAND, '--evid', HERE, '--out', os.path.join(HERE, outn),
                        '--repo', W, '--base', BASE, '--tmp', tmp], capture_output=True, stdin=subprocess.DEVNULL)
        res[mode] = json.loads(rb(os.path.join(HERE, outn)).decode('utf-8'))
    st, pv = res['selftest'], res['preview']
    cases = [case('GUARD-PREVIEW', 'guardas G1-G7 sobre el commit hipotético (base %s + candidata r3 como modificación + este directorio bajo I-62-A7/r3/)' % BASE[:8],
                  'PASS en las siete', '; '.join('%s %s' % (k, v['Status']) for k, v in sorted(pv['Guards'].items())), pv['Result'] == 'PASS', Head=pv['Head']),
             case('GUARD-SELFTEST', 'autoprueba: un mutante por condición nueva o cambiada en la ronda 3', 'candidata PASS y todos los mutantes muertos',
                  'candidata %s; mutantes muertos %d de %d' % (st['Candidate']['Result'], st['Killed'], st['MutantsTotal']), st['Result'] == 'PASS')]
    for m in st['Mutants'][1:]:
        cases.append(case('MUT-%s-%s' % (m['Guard'], m['Condition']), m['Mutant'], '%s FAIL (%s)' % (m['Guard'], m['ExpectedFinding']),
                          '%s %s' % (m['Guard'], m['GuardStatus']), m['Killed']))
    return {'Phase': 'q7-guards', 'Cases': cases, 'Preview': {'Result': pv['Result'], 'Head': pv['Head'], 'CandidateBlob': pv['CandidateBlob'],
                                                            'Guards': dict((k, v['Status']) for k, v in pv['Guards'].items())},
            'Selftest': {'Result': st['Result'], 'Killed': st['Killed'], 'Total': st['MutantsTotal']}}


# ------------------------------------------------------------------ report
PHASE_TITLES = {'q0': 'Entorno, identidades, reutilización y delta exacto frente a la ronda 2', 'q1': 'TRX de CurrentSha y RedSha (fase de la ronda 2)',
                'q2': 'RelayFacts (fase de la ronda 2)', 'q3': 'N4 y nc1 (fase de la ronda 2)', 'q4': 'Caso tipo B1, EFF^1 distinto (fase de la ronda 2)',
                'q5': 'Invalidador de A6-7 (fase de la ronda 2)', 'q6': 'Compatibilidad: A-5, A-6, EFF/EFF^1, invalidador, Owner, presupuestos y oráculos',
                'q7': 'Guardas de alcance de la ronda 3', 'q8': 'RebaseMap de la reverificación de 16.7'}


def render(R, summary):
    L = ['# I-62 A-7 ronda 3 — preflight dirigido (decisiones §96)', '',
         'Candidata: `docs/initiatives/I-62-A-7.md`, blob `%s` (ronda 2: `%s`). Base: `%s`. Inicio: %s.' % (summary['Candidate']['Blob'], PREV_BLOB, BASE, summary['StartUtc']), '',
         'Las fases q1-q5 son las de la ronda 2 (`r2lib.py`, el ejecutor de la ronda 2 con el formato `a7-designation/3`), repetidas con las piezas de la ronda 3; '
         'sus descripciones conservan el texto de la ronda 2. q0, q6 (ampliada), q7 y q8 son de la ronda 3. Nunca ejecuta Codex, claude ni ningún CLI de modelos; '
         'sin red; solo lectura del worktree de I-62.', '',
         '**Resultado: %d de %d casos se cumplen**.' % (summary['Hold'], summary['Cases']), '', '| Fase | Casos | Se cumplen |', '|---|---|---|']
    for k in R:
        L.append('| %s %s | %d | %d |' % (k, PHASE_TITLES[k], summary['Totals'][k]['Cases'], summary['Totals'][k]['Hold']))
    L += ['', 'Piezas del procedimiento (A7-1, punto 7): ' + ', '.join('%s `%s`' % (k, v[:16]) for k, v in sorted(summary['Invalidator'].items())), '']
    for k in R:
        L += ['## %s — %s' % (k, PHASE_TITLES[k]), '', R1M.table(R[k]['Cases']), '']
    return '\n'.join(L) + '\n'


def main(argv):
    global CAND
    if len(argv) < 2 or argv[1] != 'all' or '--cand' not in argv:
        sys.stderr.write('usage: python -I run_a7r3.py all --cand <I-62-A-7.md>\n')
        return 64
    CAND = argv[argv.index('--cand') + 1]
    R2.CAND = CAND
    for n in os.listdir(HERE):
        if re.match(r'q\d-.*\.json$', n) or n in ('summary.json', 'preflight.md', 'MANIFEST.sha256'):
            os.remove(os.path.join(HERE, n))
    t0 = datetime.datetime.now(datetime.timezone.utc)
    R = {}
    P = R2.prep()
    R['q0'] = q0(P)
    jdump(os.path.join(HERE, 'q0-env.json'), R['q0'])
    R['q1'], R['q2'] = R2.q12(P)
    jdump(os.path.join(HERE, 'q1-trx.json'), R['q1'])
    jdump(os.path.join(HERE, 'q2-relay.json'), R['q2'])
    R['q3'] = R2.q3()
    jdump(os.path.join(HERE, 'q3-n4-nc1.json'), R['q3'])
    R['q4'] = R2.q4(P)
    jdump(os.path.join(HERE, 'q4-b1.json'), R['q4'])
    R['q5'] = R2.q5()
    jdump(os.path.join(HERE, 'q5-invalidator.json'), R['q5'])
    R['q8'] = q8()
    jdump(os.path.join(HERE, 'q8-rebasemap.json'), R['q8'])
    R['q6'] = q6(R)
    jdump(os.path.join(HERE, 'q6-compat.json'), R['q6'])
    R['q7'] = q7()
    jdump(os.path.join(HERE, 'q7-guards.json'), R['q7'])
    order = ['q0', 'q1', 'q2', 'q3', 'q4', 'q5', 'q6', 'q7', 'q8']
    R = dict((k, R[k]) for k in order)
    allc = [(k, c) for k in order for c in R[k]['Cases']]
    summary = {'Schema': 'i62-a7r3-preflight-summary/1', 'Candidate': {'Path': A7P + ' (candidata ronda 3)', 'Blob': R['q0']['CandidateBlob']},
               'PreviousRound': PREV_BLOB, 'Base': BASE, 'StartUtc': t0.strftime('%Y-%m-%dT%H:%M:%SZ'),
               'ToolChanges': ['a7-designation/3: role REBASE (with its RunId) and Invocation.Kind REVERIFICATION; MainShaEval no longer designated',
                               'builder and validator: the rebase record of the 16.7 reverification, its coherence and MainSha_eval = MainAfterSha',
                               'designate_a7.py: REBASE without Name in C6, not admitted outside the reverification, path in the attempt directory'],
               'Totals': dict((k, {'Cases': len(R[k]['Cases']), 'Hold': sum(1 for c in R[k]['Cases'] if c['Cumple'])}) for k in order),
               'Hold': sum(1 for _, c in allc if c['Cumple']), 'Cases': len(allc), 'NotHolding': [(k, c['Caso']) for k, c in allc if not c['Cumple']],
               'Guards': R['q7']['Preview'], 'Selftest': R['q7']['Selftest'], 'Invalidator': R['q5']['Identity']}
    jdump(os.path.join(HERE, 'summary.json'), summary)
    wb(os.path.join(HERE, 'preflight.md'), R1M.sanitize(render(R, summary)).encode('utf-8'))
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
