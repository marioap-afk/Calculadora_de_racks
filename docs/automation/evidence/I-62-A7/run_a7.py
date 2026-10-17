# run_a7.py - single entry point of the preflight of the candidate amendment I-62 A-7 (decisiones §92, puntos 2, 3, 6 y 7).
#   python -I run_a7.py all [--cand <I-62-A-7.md>]
# Phases (results next to this file): p0 environment and identities; p1 reproducibility on R7-A (bench designations and the
# kit's frozen E); p2 B1-like case on a temporary clone of candidate-b.bundle; p3 negatives (absent, replaced, foreign);
# p5 compatibility and preservation; p6 reconstruction static check; p4 scope guards (self-test and preview); then
# summary.json, preflight.md, a privacy pass over this directory and MANIFEST.sha256.
# Stdlib only. Never runs Codex, claude or any model CLI; no network; never writes in the I-62 worktree, the main checkout,
# the bench, the kit or D:\r62-a6m7\m7-run: every write goes to this directory or to ..\work (temporary clones and copies).
import datetime, hashlib, importlib.util, json, os, platform, re, shutil, stat, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
WORK = os.path.join(ROOT, 'work')
W = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
BENCH = 'D:/r62-a6m7/bench'
KIT_REL = 'docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure'
BUNDLE_REL = 'docs/automation/evidence/I-62-F6/MAP-INVALID/candidate-b.bundle'
BASE = 'bb439090f6798a502c077da450edc1319e416fe5'
INSTR = '9a829b0eeb1210238052b1feac06cfa8f7964aed'
ACC = '434ef1fccb0d15762b30244b4a60928120698411'
CAND_BLOB = '6c29747d61c5f689a195095ad3712e6d9dba62aa'
CASES = ['V', 'N4', 'NC1', 'NC2', 'NC3', 'N5', 'N6', 'N7A', 'N7AR-OWN', 'N7B', 'M']
REUSED = {'make_eic_verify.py': ('58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2', BENCH + '/make_eic_verify.py'),
          'derive_r4.py': ('3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426', BENCH + '/derive_r4.py'),
          'designate_m7.py': ('491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6', BENCH + '/designate_m7.py'),
          'reconstruct_runs.py': ('5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da', BENCH + '/reconstruct_runs.py'),
          'reconstruct_runs_m7.py': ('1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab', None)}
FROZEN_E_SHA = '137c7c3bba6591c84fdef4c5f6ef443f72da1cfd871478968c64a5c7cb5a1152'
REQUEST_SCHEMA = 'rackcad-bmin-accept-request/0-experimental'
EMAIL = re.compile(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}')
FIXED_ENV = {'GIT_AUTHOR_NAME': 'A7 preflight', 'GIT_AUTHOR_EMAIL': 'preflight@a7.invalid', 'GIT_COMMITTER_NAME': 'A7 preflight',
             'GIT_COMMITTER_EMAIL': 'preflight@a7.invalid', 'GIT_AUTHOR_DATE': '2026-10-17T00:00:00+00:00',
             'GIT_COMMITTER_DATE': '2026-10-17T00:00:00+00:00'}


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


MEV = _load('make_eic_verify')
A7B = _load('make_eic_a7')
CHK = _load('check_a7_closure')
CIT = _load('check_citation_blobs')
DS = _load('designate_m7')
DV = _load('derive_r4')


# ------------------------------------------------------------------ basics, sanitization
def sha256(b):
    return hashlib.sha256(b).hexdigest()


def rb(p):
    with open(p, 'rb') as fh:
        return fh.read()


def wb(p, data):
    os.makedirs(os.path.dirname(p), exist_ok=True)
    with open(p, 'wb') as fh:
        fh.write(data)
    return p


def fwd(p):
    return p.replace('\\', '/')


def genv(extra=None):
    e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    e['GIT_OPTIONAL_LOCKS'] = '0'
    e['GIT_TERMINAL_PROMPT'] = '0'
    e.update(extra or {})
    return e


def git(repo, *args, inp=None, extra=None):
    p = subprocess.run(['git', '--no-replace-objects', '-C', repo] + [str(a) for a in args], input=inp, capture_output=True,
                       env=genv(extra), stdin=None if inp is not None else subprocess.DEVNULL)
    return p.returncode, p.stdout


def gtext(repo, *args):
    rc, out = git(repo, *args)
    return out.decode('utf-8', 'replace').strip() if rc == 0 else None


def _host_tokens():
    t = set()
    for k in ('USERNAME', 'USER'):
        if os.environ.get(k) and len(os.environ[k]) >= 3:
            t.add(os.environ[k])
    for key in ('user.name', 'user.email'):
        v = gtext(W, 'config', '--get', key)
        if v and len(v) >= 3:
            t.add(v)
    return t


HOST = _host_tokens()
UP = os.environ.get('USERPROFILE', '')


def sanitize(o):
    if isinstance(o, dict):
        return dict((sanitize(k), sanitize(v)) for k, v in o.items())
    if isinstance(o, list):
        return [sanitize(x) for x in o]
    if not isinstance(o, str):
        return o
    s = o
    for v in set([UP, UP.replace('\\', '/'), UP.replace('\\', '\\\\')]):
        if v:
            s = re.sub(re.escape(v), '%USERPROFILE%', s, flags=re.I)
    s = re.sub(r'[A-Za-z]:(?:\\\\|\\|/)+Users(?:\\\\|\\|/)+[^\\/\s"\'<>%]+', '%USERPROFILE%', s, flags=re.I)
    for t in HOST:
        s = re.sub(re.escape(t), '<email>' if '@' in t else '<usuario>', s, flags=re.I)
    return EMAIL.sub(lambda m: m.group(0) if (m.group(0).lower().endswith('.invalid') or m.group(0).lower() == 'noreply@anthropic.com')
                     else '<email>', s)


def jdump(p, o):
    return wb(p, (json.dumps(sanitize(o), ensure_ascii=False, indent=1) + '\n').encode('utf-8'))


def case(cid, what, expected, observed, ok, **data):
    d = {'Caso': cid, 'Qué': what, 'Esperado': expected, 'Observado': observed, 'Cumple': bool(ok)}
    if data:
        d['Datos'] = data
    return d


def rmtree(p):
    def onerr(func, path, exc):
        os.chmod(path, stat.S_IWRITE)
        func(path)
    if os.path.isdir(p):
        shutil.rmtree(p, onerror=onerr)


def mkcommit(repo, parent, changes, msg, idx):
    """Deterministic synthetic commit in a temporary repository: read-tree of parent into a private index, then the changes
    ({path: bytes | None}), write-tree, commit-tree (fixed *.invalid identity and dates). No ref, no work tree."""
    env = dict(FIXED_ENV, GIT_INDEX_FILE=idx)
    if os.path.exists(idx):
        os.remove(idx)
    assert git(repo, 'read-tree', parent, extra=env)[0] == 0
    for p, data in sorted(changes.items()):
        if data is None:
            git(repo, 'update-index', '--force-remove', p, extra=env)
        else:
            rc, oid = git(repo, 'hash-object', '-w', '--no-filters', '--stdin', inp=data, extra=env)
            assert git(repo, 'update-index', '--add', '--cacheinfo', '100644,%s,%s' % (oid.decode().strip(), p), extra=env)[0] == 0
    rc, t = git(repo, 'write-tree', extra=env)
    rc, c = git(repo, 'commit-tree', t.decode().strip(), '-p', parent, '-m', msg, extra=env)
    os.remove(idx)
    return c.decode().strip()


def build_a7(des, reading):
    try:
        E, rep = A7B.build(des, reading)
        return E, rep, None
    except A7B.BuildError as e:
        return None, e.rep, {'Code': e.code, 'Detail': e.detail}


def verdict_brief(v):
    return {'Accepted': v['Accepted'], 'NoClosureExpected': v['NoClosureExpected'], 'Foreign': [(x['Path'], x['Blob'][:8], x['Why']) for x in v['Foreign']],
            'Missing': [(x['Label'], x['Path'], x['Blob'][:8]) for x in v['Missing']], 'Duplicates': [(x['List'], x['Path'], x['Blob'][:8]) for x in v['Duplicates']],
            'ListViolations': v['ListViolations'], 'Problems': v['Problems'], 'Overlaps': v['Overlaps'], 'Absences': v['Absences'],
            'RepeatedPairs': len(v['RepeatedPairs']), 'UnregisteredBlobs': v.get('UnregisteredBlobs')}


# ------------------------------------------------------------------ p0 environment and identities
def p0():
    cases = []
    versions = {'git': gtext(WORK, 'version') if os.path.isdir(WORK) else None, 'python': platform.python_version(), 'os': platform.platform()}
    pw = shutil.which('pwsh')
    if pw:
        versions['pwsh'] = subprocess.run([pw, '-NoLogo', '-NoProfile', '-Command', '$PSVersionTable.PSVersion.ToString()'], capture_output=True,
                                          stdin=subprocess.DEVNULL).stdout.decode().strip()
    head = gtext(W, 'rev-parse', 'HEAD')
    cases.append(case('ENV-W', 'HEAD del worktree de I-62 (solo lectura)', BASE, head, head == BASE))
    for name, (want, src) in sorted(REUSED.items()):
        got = sha256(rb(os.path.join(HERE, name)))
        srcp = src or os.path.join(W, KIT_REL, name)
        same = rb(srcp) == rb(os.path.join(HERE, name))
        cases.append(case('REUSE-' + name, 'copia byte a byte de %s' % (fwd(srcp).replace(fwd(W), '<W>')), 'SHA-256 ' + want + ' e igual a la fuente',
                          'SHA-256 %s; igual a la fuente: %s' % (got, 'sí' if same else 'no'), got == want and same))
    for blob, label in ((INSTR, 'instrumento'), (ACC, 'aceptante')):
        rc, b = git(W, 'cat-file', 'blob', blob)
        ok = rc == 0 and DV.blob_id(b) == blob
        cases.append(case('PIN-' + label, '%s desde su objeto Git del repositorio de I-62, sin modificar' % label, 'blob ' + blob,
                          'blob recalculado %s; %d bytes; SHA-256 %s' % (DV.blob_id(b) if rc == 0 else None, len(b), sha256(b)), ok))
    cb = rb(CAND)
    cases.append(case('CAND', 'candidata A-7 leída por completo', 'blob ' + CAND_BLOB, 'blob %s, %d bytes, %d líneas, LF' % (DV.blob_id(cb), len(cb), cb.count(b'\n')),
                      DV.blob_id(cb) == CAND_BLOB and b'\r' not in cb))
    return {'Phase': 'p0-env', 'Versions': versions, 'Cases': cases}


# ------------------------------------------------------------------ p1 reproducibility on R7-A
def p1():
    cases, rows = [], []
    out = os.path.join(WORK, 'p1')
    os.makedirs(out, exist_ok=True)
    for c in CASES:
        dp = BENCH + '/designations/%s.json' % c
        des = json.loads(rb(dp).decode('utf-8'))
        cust = rb(BENCH + '/inputs/%s/E-%s-R7-A.json' % (c, c))
        r7a = MEV.serialize(MEV.build(des, 'R7-A')[0])
        row = {'Case': c, 'Designation': dp, 'DesignationSha256': sha256(rb(dp)), 'R7ACustodiedSha256': sha256(cust),
               'R7AFreshEqualsCustodied': r7a == cust}
        for reading in ('no-closure', 'no-entry'):
            E, rep, err = build_a7(des, reading)
            if err:
                row[reading] = {'Closure': False, 'NoClosure': err, 'Absences': rep.get('Absences') if rep else None}
            else:
                data = MEV.serialize(E)
                wb(os.path.join(out, 'E-%s-A7-%s.json' % (c, reading)), data)
                row[reading] = {'Closure': True, 'Sha256': sha256(data), 'EqualsR7A': data == cust, 'Entries': len(rep['Entries']),
                                'Absences': [(a['Role'], a['Path'], a.get('Revision')) for a in rep['Absences']],
                                'NotAdmittedMapFiles': len(rep['NotAdmittedMapFiles']),
                                'DifferFromAuthorityRevision': rep['DifferFromAuthorityRevision']}
        Ev = json.loads(rb(os.path.join(out, 'E-%s-A7-no-entry.json' % c)).decode('utf-8')) if row['no-entry']['Closure'] else None
        if Ev is not None:
            row['Check'] = {'default': verdict_brief(CHK.check(des, Ev, 'no-entry', 'text', W, BASE)),
                            'old-5a': verdict_brief(CHK.check(des, Ev, 'no-closure', 'text', W, BASE)),
                            'old-path': verdict_brief(CHK.check(des, Ev, 'no-entry', 'strict', W, BASE))}
        rows.append(row)
        V = row['no-entry']
        ck = row.get('Check', {})
        okv = V['Closure'] and V['EqualsR7A'] and ck['default']['Accepted']
        cases.append(case('REPRO-' + c, 'E de A7-1 (texto corregido) frente a E de R7-A (designación del banco %s.json) y validador con las lecturas del texto' % c,
                          'E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta',
                          'R7-A fresco = custodiado (%s): %s; E A7 SHA-256 %s, igual a R7-A: %s; ausencias (a) registradas sin entrada: %s; validador: %s'
                          % (row['R7ACustodiedSha256'][:16] + '…', 'sí' if row['R7AFreshEqualsCustodied'] else 'no', V.get('Sha256'),
                             'sí' if V.get('EqualsR7A') else 'no', ', '.join('%s@%s' % (a[1].split('/')[-1], a[2]) for a in V.get('Absences', []) if a[2]) or 'ninguna',
                             'acepta' if ck.get('default', {}).get('Accepted') else 'rechaza'),
                          okv and row['R7AFreshEqualsCustodied']))
    nl = [r['Case'] for r in rows if not r['no-closure']['Closure']]
    why = sorted(set(r['no-closure']['NoClosure']['Detail'] for r in rows if not r['no-closure']['Closure']))
    rej = [r['Case'] for r in rows if not r['Check']['old-5a']['Accepted']]
    cases.append(case('REPRO-OLD-5A', 'evidencia negativa: las once designaciones con la regla 5 (a) de la ronda 1 antes de su corrección (una entrada de (a) '
                      'ausente dejaba el cierre sin calcular), conmutador --absent-a=no-closure',
                      'sin cierre en las once (el motivo de F1); el texto corregido no lo hace',
                      'sin cierre en %d de 11; causa: %s; el validador con esa lectura rechaza %d de 11' % (len(nl), '; '.join(why), len(rej)),
                      len(nl) == 11 and len(rej) == 11))
    st = [r['Case'] for r in rows if not r['Check']['old-path']['Accepted']]
    cases.append(case('REPRO-OLD-PATH', 'evidencia negativa: regla 2 (c) de la ronda 1 (artifacts/orchestration/<UnitId>/<I.TaskId>/<Attempt>/<RunId>/), '
                      'conmutador --path-rule=strict', 'rechaza las copias de los controles en T1-<control> (el motivo de F2); el texto corregido las admite',
                      'rechaza %d de 11 (%s); con el texto corregido, los 11 cumplen la regla 2 (c)' % (len(st), ', '.join(st)),
                      len(st) == 9 and all(r['Check']['default']['Accepted'] for r in rows)))
    rpall = [(r['Case'], CHK.check(json.loads(rb(r['Designation']).decode('utf-8')),
                                   json.loads(rb(os.path.join(out, 'E-%s-A7-no-entry.json' % r['Case'])).decode('utf-8')), 'no-entry', 'text', W, BASE))
             for r in rows]
    rp = sorted(set((x['Path'], tuple(x['Lists'])) for _, v in rpall for x in v['RepeatedPairs']))
    dups = sum(len(v['Duplicates']) for _, v in rpall)
    cases.append(case('REPRO-UNIQ', 'regla 2 (a) corregida: «una misma ruta puede figurar con un blob por cada revisión de lectura en que difiera, y una misma '
                      'entrada puede figurar en más de una lista del cierre cuando 16.24 lo exige»',
                      'los pares repetidos de los E (heredados de R7-A) solo aparecen en listas distintas o con RequiredBy distinto; ningún duplicado',
                      '%d pares repetidos: %s; duplicados dentro de una lista y con el mismo RequiredBy: %d'
                      % (len(rp), '; '.join('%s en %s' % (p, '+'.join(l)) for p, l in rp), dups), dups == 0))
    cit = []
    for r in rows:
        des = json.loads(rb(r['Designation']).decode('utf-8'))
        Ev = json.loads(rb(os.path.join(out, 'E-%s-A7-no-entry.json' % r['Case'])).decode('utf-8'))
        cit.append((r['Case'], dict((m, CIT.check(des, m, Ev)['Result']) for m in ('text', 'unit', 'coverage'))))
    okc = all(v['coverage'] == 'LAUNCH_MAY_PROCEED' for _, v in cit)
    cases.append(case('CIT-FXU1', 'precondición de §3.5 (check_citation_blobs.py, criterio de cobertura) en las once designaciones del banco: toda lectura de '
                      'contenido de clausemap de una cita en EFF^1, EFF o MainSha_eval es de un blob que fija el cierre',
                      'se puede lanzar en las once',
                      'cobertura (normativo): %s; evidencia negativa de redacciones anteriores: texto %s, unit %s' % tuple(
                          ', '.join(sorted(set(v[m] for _, v in cit))) for m in ('coverage', 'text', 'unit')), okc, PerCase=cit))
    unreg = [(r['Case'], r['Check']['default']['UnregisteredBlobs']) for r in rows]
    okr = all(u == ['DELEG', 'H', 'TRX'] for _, u in unreg)
    cases.append(case('DES-REG', 'regla 2 (c) corregida: la designación registra la ruta y el blob de cada artefacto; el validador comprueba el blob registrado '
                      'y avisa del que falta', 'el formato a6m7-designation/1 del banco solo registra el del contrato: aviso para DELEG, H y TRX en las once '
                      '(la próxima preparación del kit tiene que registrarlos; el formato del banco no se cambia)',
                      'sin blob registrado: %s' % '; '.join(sorted(set('%s' % ', '.join(u) for _, u in unreg))), okr, PerCase=unreg))
    # the kit's frozen E: designation of the kit with the -C and the custody pointed at copies (same commits, same bytes)
    kdes = json.loads(rb(os.path.join(W, KIT_REL, 'frozen', 'designation-M.json')).decode('utf-8'))
    kdes['Repo'] = fwd(os.path.join(WORK, 'deliv'))
    for i in kdes['Inputs']:
        if i.get('Custody'):
            i['Custody'] = fwd(os.path.join(WORK, 'kit-inputs', os.path.basename(i['Custody'])))
    kdp = jdump(os.path.join(out, 'designation-kit-M.local.json'), kdes)
    frozen = rb(os.path.join(W, KIT_REL, 'frozen', 'E.json'))
    Ef = json.loads(frozen.decode('utf-8'))
    E, rep, err = build_a7(kdes, 'no-entry')
    Ek = MEV.serialize(E) if E is not None else None
    vk = verdict_brief(CHK.check(kdes, Ef, 'no-entry', 'text', W, BASE))
    vkl = verdict_brief(CHK.check(kdes, Ef, 'no-closure', 'text', W, BASE))
    _e, _r, errl = build_a7(kdes, 'no-closure')
    kit = {'FrozenESha256': sha256(frozen), 'FrozenExpected': FROZEN_E_SHA, 'A7EqualsFrozen': Ek == frozen,
           'OldReading5aNoClosure': errl, 'Check': vk, 'CheckOldReading5a': vkl, 'DesignationLocal': kdp, 'DesignationLocalSha256': sha256(rb(kdp))}
    cases.append(case('REPRO-KIT', 'E congelado del kit a6m7-measure (137c7c3b…) frente a A7-1 y al validador',
                      'el validador acepta el E congelado; E A7 = E congelado',
                      'SHA-256 del congelado %s; regla 5 (a) de la ronda 1 (evidencia negativa): %s; texto corregido: E A7 = congelado: %s; validador: %s; validador con la regla 5 (a) de la ronda 1: %s'
                      % (sha256(frozen), ('sin cierre: ' + errl['Code']) if errl else 'cierre', 'sí' if Ek == frozen else 'no',
                         'acepta' if vk['Accepted'] else 'rechaza', 'acepta' if vkl['Accepted'] else 'rechaza (NoClosureExpected)'),
                      sha256(frozen) == FROZEN_E_SHA and Ek == frozen and vk['Accepted']))
    return {'Phase': 'p1-repro', 'Rows': rows, 'Kit': kit, 'Cases': cases}


# ------------------------------------------------------------------ p2 B1-like case
def pw_sq(s):
    return "'" + s.replace("'", "''") + "'"


def run_declared(T, argv):
    """The declared form of the instrument in pwsh 7, cwd = T: & <git> --no-replace-objects cat-file blob <blob> | & <python> -I - <argv>."""
    for a in argv:
        if not re.fullmatch(r'[A-Za-z0-9_./:=+~-]+', a):
            raise SystemExit('unsafe token in the instrument argv')
    inner = '& ' + pw_sq(os.path.normpath(shutil.which('git'))) + ' --no-replace-objects cat-file blob ' + INSTR + ' | & ' + pw_sq(sys.executable) \
            + ' -I - ' + ' '.join(argv)
    p = subprocess.run([shutil.which('pwsh'), '-NoLogo', '-NoProfile', '-NonInteractive', '-Command', inner], cwd=T, capture_output=True,
                       env=genv(), stdin=subprocess.DEVNULL, timeout=900)
    return p.returncode, p.stdout, p.stderr, inner


def exe_sha256():
    try:
        return sha256(rb(sys.executable))
    except OSError:
        return sha256(rb(os.path.join(sys.base_prefix, os.path.basename(sys.executable))))


def snapshot(T):
    return {'Refs': gtext(T, 'for-each-ref', '--format=%(refname) %(objectname)'), 'Objects': gtext(T, 'count-objects', '-v'),
            'Index': os.path.exists(os.path.join(T, '.git', 'index'))}


def deep_replace(o, old, new):
    if isinstance(o, dict):
        return dict((k, deep_replace(v, old, new)) for k, v in o.items())
    if isinstance(o, list):
        return [deep_replace(x, old, new) for x in o]
    return o.replace(old, new) if isinstance(o, str) else o


def jbytes(o):
    return (json.dumps(o, ensure_ascii=False, indent=1) + '\n').encode('utf-8')


def b1_inputs(T, label, U1, U2, G, runs, decision_ref):
    """Custodied inputs (contract, delegation, handoff, TRX) in the shapes of the FX-02 kit, with synthetic values, and the
    designation of one VERIFY invocation of the synthetic unit FX-U2 of the temporary clone."""
    Tf = fwd(T)
    EFF = '88ab34c398f03070c94a584f95196a394d869f7c'
    calc, dec, st = 'src/Fixture.Lib/Calculator.cs', 'docs/automation/decisions/FX-U2.md', 'docs/automation/state/FX-U2.yml'
    bdir = os.path.join(HERE, 'b1-like', label)
    os.makedirs(bdir, exist_ok=True)
    K0 = json.loads(git(os.path.join(WORK, 'deliv'), 'cat-file', 'blob', '628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0')[1].decode('utf-8'))
    K = deep_replace(K0, 'FX-U1', 'FX-U2')
    K['AuthorityRevision'] = U1
    K['MainSha'] = EFF
    K['Authorities'] = [{'Path': dec, 'Section': 'documento completo', 'Class': 'UNIT_DOC'},
                        {'Path': st, 'Section': 'documento completo', 'Class': 'UNIT_DOC'},
                        {'Path': calc, 'Section': 'documento completo', 'Class': 'UNIT_CHANGE'},
                        {'Path': 'AGENTS.md', 'Section': '## Jobs requeridos', 'Class': 'EXTERNAL'},
                        {'Path': 'docs/AUTOMATION_PLAN.md', 'Section': '## 16. Ejecución delegada bajo orden del Coordinator', 'Class': 'EXTERNAL'}]
    D_RUN, H_RUN, I_RUN = runs
    D0 = json.loads(rb(os.path.join(WORK, 'kit-inputs', 'deleg.json')).decode('utf-8'))
    D = deep_replace(deep_replace(D0, 'FX-U1', 'FX-U2'), 'R20261012T090000Z-d001', D_RUN)
    D.update({'RunId': D_RUN, 'AuthorityRevision': U1, 'MainSha': EFF, 'BaseSha': U2, 'ExpectedBranch': 'fx/u2', 'ExpectedWorktree': Tf,
              'Authorities': K['Authorities'], 'ChainBaseSha': U2, 'ChainRedSha': None})
    H0 = json.loads(rb(os.path.join(WORK, 'kit-inputs', 'handoff.json')).decode('utf-8'))
    H = deep_replace(deep_replace(H0, 'FX-U1', 'FX-U2'), 'R20261012T090000Z-d001', D_RUN)
    diffp = sorted(x.decode() for x in git(T, 'diff', '--name-only', '-z', U2 + '..' + G)[1].split(b'\0') if x)
    H.update({'RunId': H_RUN, 'DelegationRunId': D_RUN, 'BaseSha': U2, 'RedSha': None, 'CurrentSha': G, 'Branch': 'fx/u2', 'Worktree': Tf,
              'FilesChanged': diffp})
    kp = wb(os.path.join(bdir, 'FX-U2-T1.gate-contract.json'), jbytes(K))
    dp_ = wb(os.path.join(bdir, 'deleg.json'), jbytes(D))
    hp = wb(os.path.join(bdir, 'handoff.json'), jbytes(H))
    tp = wb(os.path.join(bdir, 'fixture-tests.trx'), rb(os.path.join(WORK, 'kit-inputs', 'fixture-tests.trx')))
    des = {'Schema': 'a6m7-designation/1', 'Case': 'B1-LIKE-' + label, 'Repo': Tf, 'AuthorityRevision': U1,
           'Invocation': {'UnitId': 'FX-U2', 'TaskId': 'T1', 'Attempt': 0, 'RunId': I_RUN,
                          'Target': {'Commit': G, 'Path': calc, 'Blob': MEV.blob_at(T, G, calc)}},
           'RunPath': 'artifacts/orchestration/FX-U2/T1/0/%s/' % I_RUN,
           'Inputs': [{'Role': 'DELEG', 'Path': 'artifacts/orchestration/FX-U2/T1/0/%s/output.json' % D_RUN, 'Custody': fwd(dp_), 'Blob': DV.blob_id(rb(dp_))},
                      {'Role': 'H', 'Path': 'artifacts/orchestration/FX-U2/T1/0/%s/worker-handoff.json' % H_RUN, 'Custody': fwd(hp), 'Blob': DV.blob_id(rb(hp))},
                      {'Role': 'CONTRACT', 'Path': 'docs/automation/decisions/FX-U2-T1.gate-contract.json', 'Custody': fwd(kp),
                       'Blob': DV.blob_id(rb(kp))},
                      {'Role': 'TRX', 'Path': 'artifacts/ci/R37949808974/fixture-tests.trx', 'Custody': fwd(tp), 'Blob': DV.blob_id(rb(tp))}],
           'Instrument': {'Path': 'docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py', 'Blob': INSTR},
           'MapPath': 'docs/automation/agent-execution/compatibility/I62-clause-map.json', 'Adapter': 'codex-cli', 'DecisionRef': decision_ref}
    desp = jdump(os.path.join(bdir, 'designation.json'), des)
    return des, desp, kp, dict((os.path.basename(x), sha256(rb(x))) for x in (kp, dp_, hp, tp, desp))


def derive_closure(des, Ep, out, label):
    E = json.loads(rb(Ep).decode('utf-8'))
    chk = DS.check(des, E)
    if not chk['Ok']:
        return {'Designation': chk['Problems']}
    vec = dict(chk['Vector'], E=fwd(Ep))
    jdump(os.path.join(out, 'vector-%s.json' % label), vec)
    res = DV.derive_vector(vec)
    cp = jdump(os.path.join(out, 'closure-%s.json' % label), res['Closure'])
    return {'Path': cp, 'Sha256': sha256(rb(cp)), 'Counts': dict((k, len(res['Closure'][k])) for k in ('Commits', 'RevPaths', 'Paths', 'Files')),
            'Omitted': [(o['Term'], o['Cause']) for o in res['Omitted']], 'Designation': 'Ok', 'RevPaths': res['Closure']['RevPaths']}


def accept(T, argv, envp, cl, out, label):
    """The fixed acceptor (434ef1fc, from its Git object) on one run, with the request of the measurement (A6-7 rule 1:
    no Verification, no RelayFacts; R4 and R10 not applicable; R5 by its data condition)."""
    pin = os.path.join(WORK, 'pin')
    acc = os.path.join(pin, 'bmin_accept.py')
    if not os.path.isfile(acc) or DV.blob_id(rb(acc)) != ACC:
        wb(acc, git(W, 'cat-file', 'blob', ACC)[1])
    assert DV.blob_id(rb(acc)) == ACC
    Tf = fwd(T)
    req = {'Schema': REQUEST_SCHEMA, 'Repo': Tf, 'Tool': {'PinnedBlob': INSTR, 'RunFrom': 'object', 'GitObject': {'GitDir': Tf, 'Blob': INSTR}},
           'Interpreter': {'Python': platform.python_version(), 'ExecutableSha256': exe_sha256()},
           'Runs': [{'Name': 'r00-clausemap', 'Envelope': fwd(envp), 'Argv': argv}],
           'Closure': {'Path': fwd(cl['Path']), 'Sha256': cl['Sha256']},
           'NotApplicable': {'R4': 'A6-7 rule 1: the measurement produces no verification (no Verification in the request)',
                             'R10': 'A6-7 rule 1: the measurement has no relay facts (no RelayFacts in the request)',
                             'R5': 'data condition: Runs has no identity/diff/scope/hashes'}}
    reqp = wb(os.path.join(out, 'request-%s.json' % label), jbytes(req))
    s0 = snapshot(T)
    p = subprocess.run([sys.executable, '-I', acc, '--request', reqp], capture_output=True, cwd=pin, env=genv(), stdin=subprocess.DEVNULL, timeout=3600)
    s1 = snapshot(T)
    res = json.loads(p.stdout.decode('ascii'))
    wb(os.path.join(out, 'acceptor-%s.json' % label), p.stdout)
    checks = dict((c.get('Id'), c.get('Status')) for c in res.get('Checks', []))
    r9 = next((c.get('Detail') for c in res.get('Checks', []) if c.get('Id') == 'R9'), None)
    viol = []
    for run in ((r9 or {}).get('Runs', []) if isinstance(r9, dict) else []):
        viol += run.get('Violations') or []
    return {'Rc': p.returncode, 'Decision': res.get('Decision'), 'Checks': checks, 'R9Violations': viol[:14], 'R9ViolationCount': len(viol),
            'ZeroWrite': s0 == s1, 'StdoutSha256': sha256(p.stdout)}


def p2():
    cases = []
    T = os.path.join(WORK, 'b1')
    rmtree(T)
    rmtree(os.path.join(HERE, 'b1-like'))
    bundle = os.path.join(W, BUNDLE_REL)
    r = subprocess.run(['git', '-c', 'core.longpaths=true', 'clone', '-q', '--no-checkout', bundle, T], capture_output=True, env=genv(),
                       stdin=subprocess.DEVNULL)
    assert r.returncode == 0, 'clone of the bundle failed'
    Tf = fwd(T)
    EFF = '88ab34c398f03070c94a584f95196a394d869f7c'
    EFF1 = '55d190043458f461f187588ea79d8e9839440e28'
    CLAIM = '6040097c5f05277d4d786a99bfc80970665c1e5f'
    idx = os.path.join(WORK, 'b1.index')
    calc, dec, st = 'src/Fixture.Lib/Calculator.cs', 'docs/automation/decisions/FX-U2.md', 'docs/automation/state/FX-U2.yml'
    c0 = git(T, 'cat-file', 'blob', '%s:%s' % (CLAIM, calc))[1]
    head = b'# FX-U2 (unidad sintetica B1-like del preflight de A-7)\n\nBOOTSTRAP.\n'

    def chain(tag, calc_at_ar):
        ch = {dec: head, st: b'unit: FX-U2\nrecord_version: 1\n'}
        if calc_at_ar:
            ch[calc] = c0 + b'// cambio de la unidad anterior a la AuthorityRevision (variante %s)\n' % tag.encode()
        U1 = mkcommit(T, CLAIM, ch, 'FX-U2: BOOTSTRAP sintetico (AuthorityRevision, %s)' % tag, idx)
        c1 = git(T, 'cat-file', 'blob', '%s:%s' % (U1, calc))[1]
        U2 = mkcommit(T, U1, {st: b'unit: FX-U2\nrecord_version: 2\n', calc: c1 + b'// base sintetica de T1 (B1-like)\n'},
                      'FX-U2: base sintetica de T1 (BaseSha, %s)' % tag, idx)
        c2 = git(T, 'cat-file', 'blob', '%s:%s' % (U2, calc))[1]
        G = mkcommit(T, U2, {calc: c2 + b'// entrega sintetica de T1 (B1-like)\n', dec: head + b'\nT1: entrega sintetica.\n'},
                     'FX-U2 T1: entrega sintetica (Target, %s)' % tag, idx)
        return U1, U2, G
    U1, U2, G = chain('main', False)
    git(T, 'update-ref', 'refs/heads/fx/u2', G)
    ins_bytes = git(W, 'cat-file', 'blob', INSTR)[1]
    rc, oid = git(T, 'hash-object', '-w', '--no-filters', '--stdin', inp=ins_bytes)
    assert oid.decode().strip() == INSTR
    des, desp, kp, inputs = b1_inputs(T, 'main', U1, U2, G, ('R20261017T090000Z-b1d1', 'R20261017T091000Z-b1a1', 'R20261017T092000Z-b1v1'),
                                      'BANCO-A7-B1LIKE')
    out = os.path.join(WORK, 'p2')
    rmtree(out)
    os.makedirs(out)
    Ea7, rep, err = build_a7(des, 'no-entry')
    assert err is None, err
    Er7 = MEV.build(des, 'R7-A')[0]
    pa7 = wb(os.path.join(out, 'E-B1-A7.json'), MEV.serialize(Ea7))
    pr7 = wb(os.path.join(out, 'E-B1-R7-A.json'), MEV.serialize(Er7))
    pairs = lambda E: set((e['Path'], e['Blob']) for k in ('CanonicalInputs', 'AllowedTransitiveInputs', 'AutomaticInstructions') for e in E[k])
    at_eff1 = [e for e in rep['Entries'] if e['Revision'] == 'EFF^1']
    diff_ar = [e for e in at_eff1 if e['Blob'] != e['BlobAtAuthorityRevision']]
    r7_has = [e['Path'] for e in diff_ar if (e['Path'], e['Blob']) in pairs(Er7)]
    cases.append(case('B1-PIN-EFF1', 'B1-like (clon temporal de candidate-b, MAP_VALID; EFF^1 lleva los blobs anteriores a la activación): entradas de A7-1 en EFF^1',
                      'A7-1 fija en EFF^1 los archivos MODIFIED del mapa (y AUTOMATION_PLAN para MV-7) con blobs distintos de los de la AuthorityRevision; R7-A no',
                      'A7: %d entradas en EFF^1, %d con blob distinto del de la AR (%s); R7-A contiene %d de ellas'
                      % (len(at_eff1), len(diff_ar), ', '.join('%s %s/AR %s' % (e['Path'].split('/')[-1], e['Blob'][:8], (e['BlobAtAuthorityRevision'] or '-')[:8]) for e in diff_ar), len(r7_has)),
                      len(diff_ar) >= 5 and not r7_has, Entries=diff_ar))
    va = verdict_brief(CHK.check(des, Ea7, 'no-entry', 'text', W, BASE))
    vr = verdict_brief(CHK.check(des, Er7, 'no-entry', 'text', W, BASE))
    miss = sorted(set('%s@%s' % (m[1].split('/')[-1], m[0].split('@')[-1]) for m in vr['Missing']))
    cases.append(case('B1-CHECK', 'validador sobre los dos E del caso B1-like', 'acepta el E de A7-1; rechaza el de R7-A (faltan las lecturas en EFF^1)',
                      'A7: %s; R7-A: %s (faltan %d: %s)' % ('acepta' if va['Accepted'] else 'rechaza', 'acepta' if vr['Accepted'] else 'rechaza', len(miss), ', '.join(miss)),
                      va['Accepted'] and not vr['Accepted'] and vr['Missing'], A7=va, R7A=vr))
    closures = dict((lab, derive_closure(des, Ep, out, lab)) for lab, Ep in (('A7', pa7), ('R7-A', pr7)))
    eff1rp = dict((k, sorted(rp['Path'] for rp in v.get('RevPaths', []) if rp['Rev'] == EFF1)) for k, v in closures.items())
    cases.append(case('B1-DERIVE', 'derivación del cierre del aceptante (designate_m7 + derive_r4, sin cambio) desde cada E',
                      'designación válida en ambos; el cierre de A7 tiene RevPaths en EFF^1 para las lecturas de MV-6/MV-7 que el de R7-A no tiene',
                      'A7: %s, RevPaths en EFF^1: %d; R7-A: %s, RevPaths en EFF^1: %d' % (closures['A7'].get('Counts'), len(eff1rp['A7']),
                                                                                       closures['R7-A'].get('Counts'), len(eff1rp['R7-A'])),
                      closures['A7'].get('Designation') == 'Ok' and closures['R7-A'].get('Designation') == 'Ok' and set(eff1rp['A7']) > set(eff1rp['R7-A']),
                      RevPathsAtEFF1=eff1rp))
    argv = ['clausemap', '--repo', Tf, '--main', EFF, '--file', 'CONTRACT=' + fwd(kp), '--sha256', 'CONTRACT=' + sha256(rb(kp)), '--base', U2]
    rc, so, se, inner = run_declared(T, argv)
    envp = wb(os.path.join(out, 'env', 'r00-clausemap.json'), so)
    env = json.loads(so.decode('ascii'))
    eff1_reads = sorted(set(i['Path'] for i in env['Inputs'] if i.get('Kind') == 'git-blob' and i.get('Rev') == EFF1))
    mv = (env.get('Facts') or {}).get('Validate') or {}
    cases.append(case('B1-CLAUSEMAP', 'clausemap por la forma declarada (& <git> --no-replace-objects cat-file blob 9a829b0e… | & <python> -I - clausemap …), '
                      'objeto suelto escrito en el clon temporal', 'rc 0, Error nulo, sobre canónico; el instrumento lee contenido en EFF^1',
                      'rc %d; Error %s; hechos MV1-MV7 presentes: %s; lecturas de contenido en EFF^1: %s'
                      % (rc, env.get('Error'), 'sí' if all(mv.get('MV%d' % i) is not None for i in range(1, 8)) else 'no', ', '.join(p.split('/')[-1] for p in eff1_reads)),
                      rc == 0 and env.get('Error') is None and len(eff1_reads) >= 5, Eff1Reads=eff1_reads, EnvelopeSha256=sha256(so), StderrBytes=len(se)))
    accres = dict((lab, accept(T, argv, envp, closures[lab], out, lab)) for lab in ('A7', 'R7-A'))
    a, b = accres['A7'], accres['R7-A']
    cases.append(case('B1-R9', 'aceptante fijado (434ef1fc…) sobre la corrida real de clausemap con cada cierre derivado (solicitud de la medición: sin Verification ni RelayFacts)',
                      'R9 MATCH con el cierre de A7-1; R9 MISMATCH con el de R7-A (lecturas en EFF^1 fuera del cierre)',
                      'A7: %s rc %d, R9 %s; R7-A: %s rc %d, R9 %s (%d violaciones; p. ej. %s); cero escrituras: %s'
                      % (a['Decision'], a['Rc'], a['Checks'].get('R9'), b['Decision'], b['Rc'], b['Checks'].get('R9'), b['R9ViolationCount'],
                         '; '.join(b['R9Violations'][:2]), 'sí' if a['ZeroWrite'] and b['ZeroWrite'] else 'no'),
                      a['Checks'].get('R9') == 'MATCH' and b['Checks'].get('R9') == 'MISMATCH' and a['ZeroWrite'] and b['ZeroWrite'], Acceptor=accres))
    # variant: the unit changed its UNIT_CHANGE file before the AuthorityRevision (the file has another blob at EFF^1 / EFF than at the AR)
    U1v, U2v, Gv = chain('uc', True)
    dv_, dvp, kvp, inputs_v = b1_inputs(T, 'uc', U1v, U2v, Gv, ('R20261017T100000Z-b2d1', 'R20261017T101000Z-b2a1', 'R20261017T102000Z-b2v1'),
                                        'BANCO-A7-B1LIKE-UC')
    outv = os.path.join(out, 'uc')
    os.makedirs(outv)
    Ev, repv, errv = build_a7(dv_, 'no-entry')
    assert errv is None, errv
    pv = wb(os.path.join(outv, 'E-B1UC-A7.json'), MEV.serialize(Ev))
    vv = verdict_brief(CHK.check(dv_, Ev, 'no-entry', 'text', W, BASE))
    clv = derive_closure(dv_, pv, outv, 'A7')
    argv_v = ['clausemap', '--repo', Tf, '--main', EFF, '--file', 'CONTRACT=' + fwd(kvp), '--sha256', 'CONTRACT=' + sha256(rb(kvp)), '--base', U2v]
    rc2, so2, se2, _ = run_declared(T, argv_v)
    envv = wb(os.path.join(outv, 'env', 'r00-clausemap.json'), so2)
    av = accept(T, argv_v, envv, clv, outv, 'A7')
    calc_ar, calc_eff = MEV.blob_at(T, U1v, calc), MEV.blob_at(T, EFF, calc)
    cit_uc = dict((m, CIT.check(dv_, m, Ev)) for m in ('coverage', 'text', 'unit'))
    cases.append(case('B1-UC-R9', 'residuo declarado (A-7 §3.5, Q-A7-06): el archivo UNIT_CHANGE citado (Calculator.cs) cambió antes de la AuthorityRevision '
                      '(blob en la AR %s; en EFF^1, EFF y MainSha_eval %s); clausemap compara cada cita en EFF^1, EFF y MainSha_eval' % (calc_ar[:8], calc_eff[:8]),
                      'el E de A7-1 es válido; con él, R9 MISMATCH sobre clausemap (rechazo falso, nunca aceptación falsa: el comportamiento declarado); '
                      'la precondición de §3.5 bloquea el lanzamiento (S-03)',
                      'validador sobre el E de A7-1: %s; clausemap rc %d; aceptante: %s rc %d, R9 %s (%d violaciones: %s); precondición (cobertura): %s'
                      % ('acepta' if vv['Accepted'] else 'rechaza', rc2, av['Decision'], av['Rc'], av['Checks'].get('R9'), av['R9ViolationCount'],
                         '; '.join(av['R9Violations'][:3]), cit_uc['coverage']['Result']),
                      vv['Accepted'] and av['Checks'].get('R9') == 'MISMATCH' and av['Decision'] == 'REJECT_FACTS' and cit_uc['coverage']['Result'] == 'NO_LAUNCH_S-03',
                      Acceptor=av, Validator=vv, Precondition=cit_uc))
    cases.append(case('CIT-B1-UC', 'precondición de §3.5 en la variante B1-UC', 'el criterio de cobertura bloquea (NO_LAUNCH, S-03): clausemap leería Calculator.cs en EFF^1, EFF y MainSha_eval con un blob que el cierre no fija',
                      '; '.join('%s: %s (%s)' % (m, r['Result'], ', '.join(b['Path'].split('/')[-1] for b in r['Blocking'])) for m, r in cit_uc.items()),
                      cit_uc['coverage']['Result'] == 'NO_LAUNCH_S-03', Precondition=cit_uc))
    cit_main = dict((m, CIT.check(des, m, Ea7)) for m in ('coverage', 'text', 'unit'))
    cases.append(case('CIT-B1-MAIN', 'precondición de §3.5 en el caso B1-like principal (el contrato cita AUTOMATION_PLAN.md como EXTERNAL, como el de FX-U1; '
                      'AUTOMATION_PLAN.md es MODIFIED en todo mapa válido)',
                      'el criterio de cobertura deja lanzar (AUTOMATION_PLAN.md en EFF^1 lo fija el cierre, categoría (a); R9 da MATCH en B1-R9); la redacción anterior («blobs distintos en esas revisiones», modo text) bloqueaba: evidencia negativa de F10',
                      '; '.join('%s: %s%s' % (m, r['Result'], (' (' + '; '.join(b['Path'].split('/')[-1] + ': ' + b['Why'] for b in r['Blocking']) + ')') if r['Blocking'] else '')
                                for m, r in cit_main.items()),
                      cit_main['coverage']['Result'] == 'LAUNCH_MAY_PROCEED' and cit_main['text']['Result'] == 'NO_LAUNCH_S-03', Precondition=cit_main))
    vreg = verdict_brief(CHK.check(des, Ea7, 'no-entry', 'text', W, BASE))['UnregisteredBlobs']
    cases.append(case('DES-REG-B1', 'designación B1-like con la ruta y el blob de cada artefacto registrados (regla 2 (c) corregida)',
                      'ningún aviso de blob sin registrar; el validador comprueba los cuatro', 'sin blob registrado: %s' % (', '.join(vreg) or 'ninguno'), vreg == []))
    return {'Phase': 'p2-b1', 'Commits': {'Claim': CLAIM, 'AuthorityRevision': U1, 'BaseSha': U2, 'Target': G, 'EFF': EFF, 'EFF^1': EFF1,
                                          'Variant-uc': {'AuthorityRevision': U1v, 'BaseSha': U2v, 'Target': Gv}},
            'Designation': fwd(desp), 'Inputs': inputs, 'InputsVariant': inputs_v,
            'EA7Sha256': sha256(rb(pa7)), 'ER7ASha256': sha256(rb(pr7)),
            'Closures': dict((k, dict((x, y) for x, y in v.items() if x != 'RevPaths')) for k, v in closures.items()),
            'InstrumentArgv': argv, 'DeclaredFormShape': '& <git declarado> --no-replace-objects cat-file blob %s | & <python declarado> -I - clausemap ...' % INSTR,
            'Acceptor': accres, 'AcceptorVariant': av, 'Cases': cases, '_des': des, '_EA7': pa7}


# ------------------------------------------------------------------ p3 negatives
def p3(b1):
    cases = []
    out = os.path.join(WORK, 'p3')
    rmtree(out)
    os.makedirs(out)
    deliv = fwd(os.path.join(WORK, 'deliv'))
    vdes = json.loads(rb(BENCH + '/designations/V.json').decode('utf-8'))
    Ev = json.loads(rb(os.path.join(WORK, 'p1', 'E-V-A7-no-entry.json')).decode('utf-8'))
    tgt = vdes['Invocation']['Target']['Commit']
    base = '95bdc29d428b35d2188a10e1c5462d6caf2be4df'

    def local(des):
        d = json.loads(json.dumps(des))
        d['Repo'] = deliv
        return d

    def role(d, r):
        return next(i for i in d['Inputs'] if i['Role'] == r)

    def builder_case(cid, what, des, expected, want_closure, extra_ok=None, reading='no-entry'):
        E, rep, err = build_a7(des, reading)
        abs_ = [(a['Role'], a['Path'], a['Cause']) for a in (rep or {}).get('Absences', [])]
        subst = any(a.get('Substituted') for a in (rep or {}).get('Absences', []))
        obs = ('sin cierre: %s (%s)' % (err['Code'], err['Detail'])) if err else 'cierre con %d entradas' % len(rep['Entries'])
        if abs_:
            obs += '; ausencias registradas: ' + '; '.join('%s %s: %s' % a for a in abs_)
        ok = (err is None) == want_closure and not subst
        v = None
        if E is not None:
            v = verdict_brief(CHK.check(des, E, reading, 'text', W, BASE))
            obs += '; validador: %s' % ('acepta' if v['Accepted'] else 'rechaza')
        if extra_ok is not None:
            more, why = extra_ok(E, rep, err, v)
            ok = ok and more
            obs += ('; ' + why) if why else ''
        cases.append(case(cid, what, expected, obs, ok, Absences=abs_, Validator=v))
        return E, rep, err

    # ---------------- absent
    k0 = git(deliv, 'cat-file', 'blob', '628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0')[1]
    K = json.loads(k0.decode('utf-8'))
    K['Authorities'] = K['Authorities'] + [{'Path': 'docs/automation/decisions/FX-U1-ausente.md', 'Section': 'documento completo', 'Class': 'UNIT_DOC'}]
    kx = wb(os.path.join(out, 'K-extra-authority.json'), jbytes(K))
    d = local(vdes)
    role(d, 'CONTRACT').update({'Custody': fwd(kx), 'Blob': DV.blob_id(rb(kx))})

    def auth_absent(E, rep, err, v):
        present = E is not None and any(e['Path'] == 'docs/automation/decisions/FX-U1-ausente.md' for e in E['CanonicalInputs'])
        rec = any(a['Path'] == 'docs/automation/decisions/FX-U1-ausente.md' for a in (rep or {}).get('Absences', []))
        return (E is not None and not present and rec and v['Accepted']), 'entrada para la autoridad ausente: %s; ausencia registrada: %s' % (
            'sí' if present else 'no', 'sí' if rec else 'no')
    builder_case('NEG-ABS-AUTH', 'ausente: una autoridad citada por el contrato no existe en su revisión (UNIT_DOC en la AR)', d,
                 'regla 5 (a) corregida: cierre sin esa entrada, ausencia registrada, nada la sustituye (la trata 16.3); el validador lo acepta', True, auth_absent)
    builder_case('NEG-ABS-AUTH-OLD', 'evidencia negativa: la misma designación con la regla 5 (a) de la ronda 1 (--absent-a=no-closure)', d,
                 'sin cierre (lo que el texto corregido ya no hace)', False, reading='no-closure')
    d = local(vdes)
    d['Invocation']['Target'] = dict(d['Invocation']['Target'], Path='src/Fixture.Lib/Ausente.cs', Blob=None)
    builder_case('NEG-ABS-TARGET', 'ausente: el objeto exacto del Target no existe en Target.commit', d, 'sin cierre (regla 5 (b))', False)
    v = verdict_brief(CHK.check(d, Ev, 'no-entry', 'text', W, BASE))
    cases.append(case('NEG-ABS-TARGET-CHK', 'validador: el E de V con ese Target', 'rechaza: NoClosureExpected (regla 5 (b))',
                      'acepta' if v['Accepted'] else 'rechaza: ' + '; '.join(v['NoClosureExpected']), not v['Accepted'] and any('5 (b)' in x for x in v['NoClosureExpected'])))
    idx = os.path.join(WORK, 'deliv.index')
    g_del = mkcommit(deliv, tgt, {'src/Fixture.Lib/Fixture.Lib.csproj': None}, 'sintetico: borra una ruta presente en BaseSha', idx)
    d = local(vdes)
    d['Invocation']['Target']['Commit'] = g_del

    def no_entry(path):
        def f(E, rep, err, v):
            present = E is not None and any(e['Path'] == path for e in E['CanonicalInputs'])
            return (E is not None and not present and v['Accepted']), 'entrada para %s: %s' % (path, 'sí' if present else 'no')
        return f
    builder_case('NEG-ABS-DIFFPATH', 'ausente: ruta del diff BaseSha..Target.commit borrada en Target.commit (commit sintético %s en la copia del repositorio)' % g_del[:8],
                 d, 'cierre sin entrada para esa ruta (regla 5 (b)), causa registrada; el validador lo acepta (positivo)', True, no_entry('src/Fixture.Lib/Fixture.Lib.csproj'))
    g_rec = mkcommit(deliv, tgt, {'docs/automation/state/FX-U1.yml': None}, 'sintetico: borra el archivo de estado de la unidad', idx)
    d = local(vdes)
    d['Invocation']['Target']['Commit'] = g_rec

    def no_record(E, rep, err, v):
        present = E is not None and any(e['Path'] == 'docs/automation/state/FX-U1.yml' and e['Blob'] != '52a3f7d77a4df1a2e70e5d443eece3384cebd670'
                                        for e in E['CanonicalInputs'])
        return (E is not None and not present and v['Accepted']), 'entrada (b) del registro: %s; la autoridad UNIT_DOC sigue en la AR' % ('sí' if present else 'no')
    builder_case('NEG-ABS-RECORD', 'ausente: el registro de estado de la unidad no existe en Target.commit (commit sintético %s)' % g_rec[:8], d,
                 'cierre sin entrada (b) para el registro (regla 5 (b)), causa registrada; el validador lo acepta', True, no_record)
    junk = wb(os.path.join(out, 'otro-blob.bin'), b'bytes que no son los custodiados\n')
    os.makedirs(os.path.join(out, 'no-legible-dir'), exist_ok=True)
    for r in ('DELEG', 'H', 'CONTRACT', 'TRX'):
        for mode in ('ausente', 'otro-blob', 'ilegible'):
            d = local(vdes)
            it = role(d, r)
            if mode == 'ausente':
                if r == 'CONTRACT':
                    it.update({'Custody': None, 'Blob': 'deadbeef' * 5})
                else:
                    it['Custody'] = fwd(os.path.join(out, 'no-existe-' + r.lower()))
            elif mode == 'otro-blob':
                src = it.get('Custody')
                if r == 'CONTRACT':
                    it.update({'Custody': fwd(wb(os.path.join(out, 'K-otro.json'), k0 + b' ')), 'Blob': '628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0'})
                else:
                    it['Blob'] = DV.blob_id(rb(junk))
            else:
                it['Custody'] = fwd(os.path.join(out, 'no-legible-dir'))
            dhk = r in ('DELEG', 'CONTRACT')
            path = it['Path']

            def chk(E, rep, err, v, path=path, r=r):
                if E is None:
                    return True, 'sin K o sin D, (a) y (b) no se pueden calcular: sin cierre ni lanzamiento (regla 5 (c))'
                present = any(e['Path'] == path for e in E['CanonicalInputs'])
                arts = [e['Path'] for e in E['CanonicalInputs'] if e['Path'].startswith('artifacts/') or e['Path'].endswith('.trx')]
                ds = DS.check(d, E)
                extra, ok_extra = '', True
                if r == 'H' and ds['Ok']:
                    Ep = wb(os.path.join(out, 'E-H-%s.json' % mode), MEV.serialize(E))
                    res = DV.derive_vector(dict(ds['Vector'], E=fwd(Ep)))
                    om = sorted(o['Term'] for o in res['Omitted'] if o['Cause'] == 'INPUT_ABSENT')
                    want = ['C1.H.BaseSha', 'C1.H.CurrentSha', 'C1.H.RedSha', 'C1.RevList(D.BaseSha..H.CurrentSha)', 'C6.H']
                    ok_extra = om == sorted(want)
                    extra = '; derivación como N4: omitidos por INPUT_ABSENT: %s' % ', '.join(om)
                return (not present and len(arts) == 2 and v['Accepted'] and ok_extra), 'entrada en la ruta designada: %s; otras entradas fuera de Git: %d; designate_m7: %s%s' % (
                    'sí' if present else 'no', len(arts), 'Ok' if ds['Ok'] else ', '.join(p['Code'] for p in ds['Problems']), extra)
            builder_case('NEG-ABS-%s-%s' % (r, mode.upper()), 'ausente: %s %s' % (r, {'ausente': 'sin bytes custodiados', 'otro-blob': 'con bytes de otro blob que el registrado',
                                                                                    'ilegible': 'no legible (la ruta es un directorio)'}[mode]), d,
                         ('regla 5 (c) corregida: sin cierre y sin lanzamiento, causa registrada, nada sustituido' if dhk else
                          'regla 5 (c): sin entrada, causa registrada, nada sustituido; el validador acepta el E' + ('; como N4 en la derivación' if r == 'H' else '')),
                         not dhk, chk)
    # ---------------- replaced (validator on mutated copies of E)
    def mut_case(cid, what, des, E, expected, test, reading='no-entry'):
        v = verdict_brief(CHK.check(des, E, reading, 'text', W, BASE))
        obs = 'acepta' if v['Accepted'] else 'rechaza: ' + '; '.join(
            ['FOREIGN %s %s (%s)' % x for x in v['Foreign']] + ['MISSING %s %s %s' % x for x in v['Missing']] + ['DUPLICATE %s %s %s' % x for x in v['Duplicates']]
            + v['Problems'] + v['NoClosureExpected'])
        cases.append(case(cid, what, expected, obs, (not v['Accepted']) and test(v), Validator=v))

    def edit(E, f):
        E2 = json.loads(json.dumps(E))
        f(E2)
        return E2

    def repl(E2, path, old, new):
        for e in E2['CanonicalInputs']:
            if e['Path'] == path and e['Blob'] == old:
                e['Blob'] = new
                return
        raise SystemExit('mutation anchor missing: ' + path)
    calc = 'src/Fixture.Lib/Calculator.cs'
    b_base = MEV.blob_at(deliv, base, calc)
    mut_case('NEG-REP-BASE-V', 'reemplazada: el objeto (Calculator.cs) fijado en BaseSha en vez de Target.commit (V)', vdes,
             edit(Ev, lambda E2: repl(E2, calc, '380892ee869194aa4bd56dcf32dfd76ef0203ab9', b_base)),
             'rechaza: falta la entrada (b) en Target.commit; el blob de BaseSha coincide aquí con el de la AR (duplicado de la autoridad UNIT_CHANGE)',
             lambda v: any(m[1] == calc for m in v['Missing']))
    des_b1, Eb1 = b1['_des'], json.loads(rb(b1['_EA7']).decode('utf-8'))
    T = des_b1['Repo']
    tb = des_b1['Invocation']['Target']['Blob']
    bb = MEV.blob_at(T, b1['Commits']['BaseSha'], calc)
    mut_case('NEG-REP-BASE-B1', 'reemplazada: el objeto fijado en BaseSha en vez de Target.commit (B1-like, donde BaseSha cambió el objeto)', des_b1,
             edit(Eb1, lambda E2: repl(E2, calc, tb, bb)), 'rechaza: entrada ajena (blob de BaseSha) y falta la del Target',
             lambda v: any(f[0] == calc for f in v['Foreign']) and any(m[1] == calc for m in v['Missing']), reading='no-entry')
    dec = 'docs/automation/decisions/FX-U1.md'
    mut_case('NEG-REP-AUTH-TGT', 'reemplazada: la autoridad UNIT_DOC (decisiones) fijada en Target.commit en vez de en su revisión de lectura (AR)', vdes,
             edit(Ev, lambda E2: repl(E2, dec, 'd3f3ec72974a65e05ebd7ad4d1c0b3ced7a07c93', '3fcfffc1aa7ad442422d4da2e5edaf89c48239ea')),
             'rechaza: falta la autoridad en la AR (la entrada queda duplicada con el registro (b))',
             lambda v: any(m[1] == dec and m[0].startswith('a:AUTHORITY') for m in v['Missing']))
    kother = DV.blob_id(rb(kx))
    mut_case('NEG-REP-CONTRACT', 'reemplazada: el contrato por otro contrato (otro blob en la ruta designada)', vdes,
             edit(Ev, lambda E2: repl(E2, 'docs/automation/decisions/FX-U1-T1.gate-contract.json', '628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0', kother)),
             'rechaza: entrada ajena y falta el contrato custodiado', lambda v: any(m[0] == 'c:CONTRACT' for m in v['Missing']) and v['Foreign'])
    own = json.loads(rb(BENCH + '/designations/N7AR-OWN.json').decode('utf-8'))
    oh = role(own, 'H')
    other_h = 'artifacts/orchestration/FX-U1/T1/0/R20261011T081000Z-a000/worker-handoff.json'
    hp = role(vdes, 'H')['Path']

    def swap_h(E2):
        for e in E2['CanonicalInputs']:
            if e['Path'] == hp:
                e['Path'], e['Blob'] = other_h, DV.blob_id(rb(oh['Custody']))
    mut_case('NEG-REP-H-OTHER', 'reemplazada: la entrega por la copia de otra corrida (R20261011T081000Z-a000) en una ruta no designada', vdes, edit(Ev, swap_h),
             'rechaza: entrada fuera de Git no designada y falta la entrega designada', lambda v: any(f[0] == other_h for f in v['Foreign']) and any(m[0] == 'c:H' for m in v['Missing']))
    eff1_ent = next(e for e in Eb1['AllowedTransitiveInputs'] if e['Path'] == 'docs/WORKFLOW.md' and e['Blob'] == MEV.blob_at(T, b1['Commits']['EFF^1'], 'docs/WORKFLOW.md'))
    effb = MEV.blob_at(T, b1['Commits']['EFF'], 'docs/WORKFLOW.md')

    def eff1_to_eff(E2):
        for e in E2['AllowedTransitiveInputs']:
            if e['Path'] == eff1_ent['Path'] and e['Blob'] == eff1_ent['Blob']:
                e['Blob'] = effb
    mut_case('NEG-REP-EFF1', 'reemplazada: la entrada de EFF^1 (WORKFLOW, MV-6/MV-7) por su blob de EFF (B1-like)', des_b1, edit(Eb1, eff1_to_eff),
             'rechaza: falta la lectura en EFF^1 (y la entrada queda duplicada)', lambda v: any(m[1] == 'docs/WORKFLOW.md' and m[0].endswith('EFF^1') for m in v['Missing']),
             reading='no-entry')
    # ---------------- foreign
    def add(E2, ent, lst='CanonicalInputs'):
        E2[lst].append(ent)
    csproj = 'src/Fixture.Lib/Fixture.Lib.csproj'
    mut_case('NEG-FOR-TGT-OUT', 'ajena: un archivo de Target.commit fuera del diff y de los registros (%s)' % csproj, vdes,
             edit(Ev, lambda E2: add(E2, {'Path': csproj, 'Blob': MEV.blob_at(deliv, tgt, csproj)})), 'rechaza: FOREIGN', lambda v: any(f[0] == csproj for f in v['Foreign']))
    a6 = 'docs/initiatives/I-62-A-6.md'
    mut_case('NEG-FOR-OTHER-REPO', 'ajena: una ruta de otro repositorio (el worktree de I-62: %s, blob 4bc919dd)' % a6, vdes,
             edit(Ev, lambda E2: add(E2, {'Path': a6, 'Blob': '4bc919dd4768ab81abc74b70d3a4a3821c07feb5'})), 'rechaza: FOREIGN', lambda v: any(f[0] == a6 for f in v['Foreign']))
    trx2 = 'artifacts/ci/R00000000000/otro.trx'
    mut_case('NEG-FOR-TRX', 'ajena: un TRX no designado', vdes, edit(Ev, lambda E2: add(E2, {'Path': trx2, 'Blob': DV.blob_id(rb(junk))})),
             'rechaza: FOREIGN (fuera de Git o TRX no designado)', lambda v: any(f[0] == trx2 for f in v['Foreign']))
    dn = json.loads(json.dumps(vdes))
    dn['Instrument'] = None
    mut_case('NEG-FOR-INSTR', 'ajena: la entrada del instrumento (y su acción) en una invocación que no lo admite (designación sin instrumento)', dn, Ev,
             'rechaza: FOREIGN (instrumento no admitido) y la acción del instrumento', lambda v: v['Foreign'] and any('ACTION' in p for p in v['Problems']))
    En, repn, errn = build_a7(dn, 'no-entry')
    cases.append(case('NEG-FOR-INSTR-BUILD', 'constructor con la misma designación sin instrumento (control)', 'cierre sin la entrada ni la acción del instrumento',
                      'entrada del instrumento: %s; acción: %s' % ('sí' if any(e['Blob'] == INSTR for e in En['CanonicalInputs']) else 'no',
                                                                 'sí' if any(INSTR in a['Action'] for a in En['AllowedActions']) else 'no'),
                      errn is None and not any(e['Blob'] == INSTR for e in En['CanonicalInputs']) and not any(INSTR in a['Action'] for a in En['AllowedActions'])))
    mut_case('NEG-FOR-DUP', 'ajena: una entrada duplicada con el mismo blob (el registro de decisiones en Target.commit, dos veces)', vdes,
             edit(Ev, lambda E2: add(E2, {'Path': dec, 'Blob': '3fcfffc1aa7ad442422d4da2e5edaf89c48239ea'})), 'rechaza: DUPLICATE',
             lambda v: any(x[1] == dec for x in v['Duplicates']))
    # ---------------- corrected text: narrowed (a), overlap rule, path rule of 2 (c)
    eff_v = '930288c51ab1b5dbd370c8ebf9206d78670b89a0'
    added = 'docs/automation/agent-execution/adapters/codex-cli.md'
    rb_ap = next(e['RequiredBy'] for e in Ev['AllowedTransitiveInputs'] if e['RequiredBy'].get('Section', '').startswith('### 16.13'))
    mut_case('NEG-FOR-WIDE', 'ajena con el texto corregido: un archivo ADDED del mapa en EFF (%s), que Validate solo compara por su blob' % added, vdes,
             edit(Ev, lambda E2: add(E2, {'Path': added, 'Blob': MEV.blob_at(deliv, 'fbe25347799e5b801ef708454212335537448bd1', added), 'RequiredBy': rb_ap},
                                     'AllowedTransitiveInputs')),
             'rechaza: FOREIGN (la regla 1 (a) corregida solo admite mapa, esquema, MODIFIED y WORKFLOW/AUTOMATION_PLAN)',
             lambda v: any(f[0] == added for f in v['Foreign']))
    ar_dec = git(deliv, 'cat-file', 'blob', 'd3f3ec72974a65e05ebd7ad4d1c0b3ced7a07c93')[1]
    g_ov = mkcommit(deliv, tgt, {dec: ar_dec}, 'sintetico: el registro de decisiones vuelve al blob de la AuthorityRevision', idx)
    d = local(vdes)
    d['Invocation']['Target']['Commit'] = g_ov

    def overlap(E, rep, err, v):
        n = sum(1 for e in E['CanonicalInputs'] if e['Path'] == dec and e['Blob'] == 'd3f3ec72974a65e05ebd7ad4d1c0b3ced7a07c93') if E else 0
        ov = [o for o in v['Overlaps'] if o['Path'] == dec] if v else []
        return (n == 1 and ov and ov[0]['Categories'] == ['a', 'b'] and v['Accepted']), 'entradas de ese par: %d; categorías: %s' % (
            n, ov[0]['Categories'] if ov else '-')
    builder_case('NEG-POS-OVERLAP', 'positivo de la regla 1 corregida: el registro de decisiones en Target.commit tiene el blob de la AR (commit sintético %s), '
                 'así que el par es a la vez (a) UNIT_DOC y (b) registro y objeto del diff' % g_ov[:8], d,
                 'una sola entrada, en dos categorías y con el blob de la referencia de cada una; el validador la acepta', True, overlap)
    same = json.loads(rb(BENCH + '/designations/N7AR-SAME.json').decode('utf-8'))
    Es, reps, errs = build_a7(same, 'no-entry')
    vs = verdict_brief(CHK.check(same, Es, 'no-entry', 'text', W, BASE))
    cases.append(case('NEG-PATH-RUNID', 'regla 2 (c) corregida: la copia de la entrega en un directorio con el RunId de la verificación real y no el de sus bytes '
                      '(designación N7AR-SAME del banco, el riesgo O6)', 'rechaza: PATH_RULE (el directorio no lleva el RunId de los bytes)',
                      'acepta' if vs['Accepted'] else 'rechaza: ' + '; '.join(vs['Problems']),
                      (not vs['Accepted']) and any(p.startswith('PATH_RULE: H') for p in vs['Problems']), Validator=vs))
    reuse = {'Note': 'Los negativos de sustitución e inflación a nivel de la derivación del cierre del aceptante no se repiten: se reutilizan por identidad '
                     '(derive_r4.py y designate_m7.py sin cambio, SHA-256 3de12c54… y 491dcbd6…).',
             'Files': [{'Path': 'docs/automation/evidence/I-62-A6/r4/r1probes/p7-derive.json', 'Sha256': '099000fb80a33bee394bdfc9690398aa7f558e79a11d6fbea3ef71c829de7444',
                        'Cases': 'grupo neg: S-N4-REALH, S-N4-TARGET, S-NC1-G, S-NC1-OTHER, S-N4-E2E, X-N4-COMMIT, X-N4-NX, X-N4-PATH, X-N4-REVPATH, X-N4-FILE, X-N4-REF'},
                       {'Path': 'docs/automation/evidence/I-62-A6/r4/r1probes/p9-n4.json', 'Sha256': 'b3dcf564ef67f9da0dafa21caf9d5d95a2114a8e72b422f972b5d67873efea51',
                        'Cases': 'N4-x, N4-x-subst'},
                       {'Path': 'docs/automation/evidence/I-62-A6/r4/r1probes/p10-nc1.json', 'Sha256': '068cd82fcb77abff0f5374418ca3bf20b886ee19851fb0b2e1a1b84844a92870',
                        'Cases': 'nc1-x'},
                       {'Path': 'docs/automation/evidence/I-62-A6/m7/bench/p1-build.json (grupo desig)', 'Sha256': None,
                        'Cases': 'DES-MULTI, DES-O3-OTHER-RUN, DES-O3-PATTERN, DES-TRX-BAD, DES-MAP-NX'}]}
    for f in reuse['Files']:
        if f['Sha256']:
            f['Verified'] = sha256(rb(os.path.join(W, f['Path']))) == f['Sha256']
        else:
            f['Sha256'] = sha256(rb(BENCH + '/p1-build.json'))
            f['Verified'] = rb(BENCH + '/p1-build.json') == rb(os.path.join(W, 'docs/automation/evidence/I-62-A6/m7/bench/p1-build.json'))
    return {'Phase': 'p3-neg', 'SyntheticCommits': {'DiffPathDeleted': g_del, 'RecordAbsent': g_rec, 'RecordAtAR': g_ov}, 'ReusedByIdentity': reuse, 'Cases': cases}


# ------------------------------------------------------------------ p5 compatibility and preservation
def longest_window(needle, hay):
    """Length of the longest substring of needle (normalized) present in hay (normalized)."""
    n, h = re.sub(r'\s+', ' ', needle), re.sub(r'\s+', ' ', hay)
    lo, hi = 0, len(n)
    while lo < hi:
        mid = (lo + hi + 1) // 2
        if any(n[i:i + mid] in h for i in range(0, len(n) - mid + 1)):
            lo = mid
        else:
            hi = mid - 1
    return lo


def p5():
    cases = []
    a7 = rb(CAND).decode('utf-8')
    guards = _load_guards()
    pinned = guards.PINNED
    for p, b in sorted(pinned.items()):
        got = gtext(W, 'rev-parse', 'HEAD:' + p)
        cases.append(case('PIN-' + p.split('/')[-1], 'blob de %s en la base' % p, b, got, got == b))
    v14 = git(W, 'cat-file', 'blob', pinned['docs/initiatives/I-62-proposal-v14.md'])[1].decode('utf-8').split('\n')
    i0 = v14.index('### D.3 Hoja de invocaciones y escenarios')
    i1 = v14.index('### D.4 Resultados y cobertura')
    rows = [x for x in v14[i0:i1] if x.startswith('| ') and not x.startswith('|---') and ('Codex' in x or 'Controller' in x or x.startswith('| A ')
                                                                                         or x.startswith('| FX-04') or x.startswith('| B (FX-03)'))]
    mx = max(longest_window(r, a7) for r in rows)
    cases.append(case('PRES-D3-BUDGET', 'filas de presupuesto de V14 D.3 (Controller, Codex, totales por ronda: %d filas, L%d-L%d)' % (len(rows), i0 + 1, i1),
                      'sin cambio (V14 34ad80ea) y no reescritas por A-7 (ningún tramo de más de 60 caracteres de una fila en A-7)',
                      'V14 en la base = 34ad80ea: %s; tramo común más largo con A-7: %d caracteres' % ('sí' if gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-proposal-v14.md') == pinned['docs/initiatives/I-62-proposal-v14.md'] else 'no', mx),
                      mx <= 60, RowsSha256=[sha256(r.encode('utf-8')) for r in rows]))
    a6 = git(W, 'cat-file', 'blob', pinned['docs/initiatives/I-62-A-6.md'])[1].decode('utf-8').split('\n')
    j0 = next(i for i, x in enumerate(a6) if x.startswith('### 3.9 A6-6'))
    j1 = next(i for i, x in enumerate(a6) if x.startswith('### 3.10 A6-7'))
    oracle = '\n'.join(x for x in a6[j0:j1] if x.startswith('>'))
    mo = longest_window(oracle, a7)
    cases.append(case('PRES-A6-6', 'oráculo de C-24 (A6-6, A-6 L%d-L%d)' % (j0 + 1, j1), 'sin cambio (A-6 4bc919dd) y no reescrito por A-7',
                      'SHA-256 del párrafo %s; tramo común más largo con A-7: %d caracteres' % (sha256(oracle.encode('utf-8')), mo), mo <= 60))
    owner = [(i + 1, x) for i, x in enumerate(a6) if x.startswith('A6-ADR0046-9-FX02 = A (') or x.startswith('A6-CONTROLLER-MEDICION-CONSUMO = A (')]
    dec = rb(os.path.join(W, 'docs/automation/decisions/I-62.md')).decode('utf-8').split('\n')
    k78 = dec.index(next(x for x in dec if x.startswith('## 78.')))
    l78 = next(x for x in dec[k78:k78 + 12] if x.startswith('A6-ADR0046-9-FX02 = A ('))
    od2 = [(i + 1, x) for i, x in enumerate(dec) if x.startswith('| **OD-2** | **APPROVED') or x.startswith('| **OD-2c** | **APPROVED')]
    kit_lines = [(n, rb(os.path.join(W, KIT_REL, n)).decode('utf-8').rstrip('\n')) for n in ('line-b.txt', 'line-od2.txt')]
    for label, text, want in ([('A-6 §7 L%d' % n, t, None) for n, t in owner] + [('decisiones §78', l78, '83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14')]
                              + [('decisiones L%d' % n, t, None) for n, t in od2] + [('kit %s' % n, t, None) for n, t in kit_lines]):
        h = sha256(text.encode('utf-8'))
        w_ = longest_window(text, a7)
        cases.append(case('PRES-OWNER-' + label.replace(' ', '-').replace('§', 'S'), 'línea del Owner: %s' % label,
                          'sin cambio%s; A-7 no la reescribe ni la altera (solo la nombra)' % ((' (SHA-256 ' + want[:16] + '…)') if want else ''),
                          'SHA-256 %s (%d bytes); tramo común más largo con A-7: %d caracteres' % (h, len(text.encode('utf-8')), w_),
                          (want is None or h == want) and w_ <= 60))
    same78 = [t for n, t in owner if t.startswith('A6-ADR0046-9-FX02')][0] == l78
    cases.append(case('PRES-OWNER-78-A6', 'la línea (a) de A-6 §7 es byte a byte la registrada en decisiones §78', 'idénticas', 'idénticas' if same78 else 'distintas', same78))
    a5 = gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-5.md')
    cases.append(case('COMPAT-A5', 'A-5 (A5-1 construcción de B1, A5-2 presupuesto de FX-U2)', 'A-5 sin cambio (ad94ceed); A-7 §6 declara A5-1 y A5-2 sin cambio y no crea B1',
                      'blob %s; «A5-1 y A5-2 sin cambio» en A-7: %s; «no crea B1» en A-7: %s' % (a5, 'sí' if 'A5-1 y A5-2 sin cambio' in a7 else 'no', 'sí' if 'no crea B1' in a7 else 'no'),
                      a5 == pinned['docs/initiatives/I-62-A-5.md'] and 'A5-1 y A5-2 sin cambio' in a7 and 'no crea B1' in a7))
    rule7 = a7[a7.index('> 7. **límites:**'):a7.index('### 3.3')]
    touched = [{'Regla': 'A6-1', 'Cómo': 'no tocada'},
               {'Regla': 'A6-2, reglas 1 y 2', 'Cómo': 'sin cambio: la categoría (d) remite a ellas'},
               {'Regla': 'A6-2, regla 3', 'Cómo': 'la única tocada: dentro del ámbito de A-7, la referencia de comparación (I6) de las entradas de (a) leídas fuera de la AR, de (b) y de (c) deja de ser la AR; para las autoridades de la unidad sigue siendo la AR; el texto de A6-2 no cambia (A-7 §3.4)'},
               {'Regla': 'A6-3, reglas 1-5 y 7', 'Cómo': 'sin cambio; la regla 4 (lectura de otro repositorio = P-22) se conserva (A-7 §3.5)'},
               {'Regla': 'A6-3, regla 6 (b)', 'Cómo': 'sin cambio: la derivación usa el cierre de A7-1 tal como es; omisiones (i)-(v) iguales'},
               {'Regla': 'A6-4', 'Cómo': 'sin cambio: la fidelidad del instrumento, por remisión (A7-1, regla 4 (d))'},
               {'Regla': 'A6-5, A6-6, A6-8', 'Cómo': 'no tocadas (oráculos, resultado del aceptante, determinación del Owner)'},
               {'Regla': 'A6-7, reglas 5 y 7', 'Cómo': 'sin cambio: el cierre de A7-1 es el «cierre de VERIFY» que nombran'},
               {'Regla': 'A6-9, regla 4 y tabla', 'Cómo': 'sin cambio: el cierre del aceptante se deriva del de A7-1'}]
    ok_r7 = 'ni ninguna otra regla de A-6' in re.sub(r'\s*\n>\s*', ' ', rule7)
    cases.append(case('COMPAT-A6-RULES', 'reglas de A-6 que toca A7-1', 'solo la referencia de comparación de A6-2, regla 3, dentro del ámbito de A-7; A7-1, regla 7, excluye cualquier otra',
                      '«ni ninguna otra regla de A-6» en la regla 7: %s; A-6 sin cambio de blob: %s' % ('sí' if ok_r7 else 'no', 'sí' if gtext(W, 'rev-parse', 'HEAD:docs/initiatives/I-62-A-6.md') == pinned['docs/initiatives/I-62-A-6.md'] else 'no'),
                      ok_r7, Table=touched))
    return {'Phase': 'p5-compat', 'Cases': cases, 'A6RulesTouched': touched}


def _load_guards():
    s = importlib.util.spec_from_file_location('a7_guards', os.path.join(HERE, 'a7-guards.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


# ------------------------------------------------------------------ p6 reconstruction, p4 guards
def p6():
    outp = os.path.join(WORK, 'p6-recon.raw.json')
    r = subprocess.run([sys.executable, '-I', os.path.join(HERE, 'recon_static.py'), os.path.join(HERE, 'reconstruct_runs.py'),
                        os.path.join(HERE, 'reconstruct_runs_m7.py'), outp], capture_output=True, stdin=subprocess.DEVNULL)
    d = json.loads(rb(outp).decode('utf-8'))
    cases = [case('RECON-' + x['Definition'].split(' ')[0], 'identidad AST de %s (%s)' % (x['Definition'], x['Role']), 'idéntica tras la normalización declarada',
                  'idéntica' if x['Identical'] else 'distinta', x['Identical'], Normalization=x['Normalization']) for x in d['Rows']]
    rec = [x for x in d['Rows'] if x['Definition'] == 'reconstruct'][0]
    cases.append(case('RECON-HEADER', 'cambios de la v2 frente a lo que declara su cabecera', 'solo lo declarado: RULE, CMD_NAME, CMD_FORBIDDEN, _abs_shell, _parse_cmd_inner, '
                      '_parse_pwsh_inner (extracción), despachador, campos Rule y Rendering',
                      'nuevas o cambiadas: %s; no declaradas: %s; además se retiró del original una sentencia sin efecto: %s'
                      % (', '.join(d['NewOrChangedInV2']), ', '.join(d['UndeclaredNewOrChanged']) or 'ninguna', '; '.join(rec['NoopStatements']) or 'ninguna'),
                      not d['UndeclaredNewOrChanged']))
    return {'Phase': 'p6-recon', 'Raw': d, 'Cases': cases, 'Rc': r.returncode}


def p4():
    tmp = os.path.join(WORK, 'guards-tmp')
    rmtree(tmp)
    os.makedirs(tmp)
    res = {}
    for mode, outn in (('selftest', 'p4-guards-selftest.json'), ('preview', 'p4-guards-preview.json')):
        args = [sys.executable, '-I', os.path.join(HERE, 'a7-guards.py'), mode, '--cand', CAND, '--evid', HERE, '--out', os.path.join(HERE, outn),
                '--repo', W, '--base', BASE, '--tmp', tmp]
        r = subprocess.run(args, capture_output=True, stdin=subprocess.DEVNULL)
        res[mode] = json.loads(rb(os.path.join(HERE, outn)).decode('utf-8'))
        res[mode + 'Rc'] = r.returncode
    st, pv = res['selftest'], res['preview']
    cases = [case('GUARD-PREVIEW', 'guardas G1-G7 sobre el commit hipotético (base %s + candidata + este directorio)' % BASE[:8], 'PASS en las siete',
                  '; '.join('%s %s' % (k, v['Status']) for k, v in sorted(pv['Guards'].items())), pv['Result'] == 'PASS', Head=pv['Head'], Added=pv['Added'])]
    cases.append(case('GUARD-SELFTEST', 'autoprueba: un mutante por condición de guarda', 'candidata PASS y todos los mutantes muertos',
                      'candidata %s; mutantes muertos %d de %d' % (st['Candidate']['Result'], st['Killed'], st['MutantsTotal']), st['Result'] == 'PASS'))
    for m in st['Mutants'][1:]:
        cases.append(case('MUT-%s-%s' % (m['Guard'], m['Condition']), m['Mutant'], '%s FAIL (%s)' % (m['Guard'], m['ExpectedFinding']),
                          '%s %s' % (m['Guard'], m['GuardStatus']), m['Killed']))
    return {'Phase': 'p4-guards', 'Preview': {'Result': pv['Result'], 'Head': pv['Head'], 'CandidateBlob': pv['CandidateBlob'],
                                              'Guards': dict((k, v['Status']) for k, v in pv['Guards'].items())},
            'Selftest': {'Result': st['Result'], 'Killed': st['Killed'], 'Total': st['MutantsTotal']}, 'Cases': cases}


# ------------------------------------------------------------------ report
def table(cases):
    out = ['| Caso | Qué | Esperado | Observado | ¿Se cumple? |', '|---|---|---|---|---|']
    for c in cases:
        cell = lambda s: str(s).replace('|', chr(92) + '|').replace(chr(10), ' ')
        out.append('| %s | %s | %s | %s | %s |' % (c['Caso'], cell(c['Qué']), cell(c['Esperado']), cell(c['Observado']), 'sí' if c['Cumple'] else '**no**'))
    return '\n'.join(out)


def main(argv):
    global CAND
    if len(argv) < 2 or argv[1] != 'all':
        sys.stderr.write('usage: python -I run_a7.py all [--cand <I-62-A-7.md>]\n')
        return 64
    CAND = argv[argv.index('--cand') + 1] if '--cand' in argv else os.path.join(WORK, 'cand', 'I-62-A-7.md')
    for n in os.listdir(HERE):
        if re.match(r'p\d-.*\.json$', n) or n in ('summary.json', 'preflight.md', 'MANIFEST.sha256'):
            os.remove(os.path.join(HERE, n))
    rmtree(os.path.join(HERE, 'b1-like'))
    t0 = datetime.datetime.now(datetime.timezone.utc)
    R = {}
    R['p0'] = p0()
    jdump(os.path.join(HERE, 'p0-env.json'), R['p0'])
    R['p1'] = p1()
    jdump(os.path.join(HERE, 'p1-repro.json'), R['p1'])
    R['p2'] = p2()
    jdump(os.path.join(HERE, 'p2-b1.json'), dict((k, v) for k, v in R['p2'].items() if not k.startswith('_')))
    R['p3'] = p3(R['p2'])
    jdump(os.path.join(HERE, 'p3-neg.json'), R['p3'])
    R['p5'] = p5()
    jdump(os.path.join(HERE, 'p5-compat.json'), R['p5'])
    R['p6'] = p6()
    jdump(os.path.join(HERE, 'p6-recon.json'), R['p6'])
    R['p4'] = p4()
    jdump(os.path.join(HERE, 'p4-guards.json'), R['p4'])
    allc = [(k, c) for k in ('p0', 'p1', 'p2', 'p3', 'p4', 'p5', 'p6') for c in R[k]['Cases']]
    summary = {'Schema': 'i62-a7-preflight-summary/1', 'Candidate': {'Path': 'docs/initiatives/I-62-A-7.md (candidata)', 'Blob': CAND_BLOB},
               'Base': BASE, 'StartUtc': t0.strftime('%Y-%m-%dT%H:%M:%SZ'),
               'Totals': dict((k, {'Cases': len(R[k]['Cases']), 'Hold': sum(1 for c in R[k]['Cases'] if c['Cumple'])}) for k in R),
               'Hold': sum(1 for _, c in allc if c['Cumple']), 'Cases': len(allc),
               'NotHolding': [(k, c['Caso']) for k, c in allc if not c['Cumple']],
               'Guards': R['p4']['Preview'], 'Selftest': R['p4']['Selftest'],
               'Versions': R['p0']['Versions']}
    jdump(os.path.join(HERE, 'summary.json'), summary)
    PT = _load('preflight_text')
    wb(os.path.join(HERE, 'preflight.md'), sanitize(PT.render(R, summary, table)).encode('utf-8'))
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
    sys.stdout.write(json.dumps({'Cases': summary['Cases'], 'Hold': summary['Hold'], 'NotHolding': summary['NotHolding'],
                                 'Guards': summary['Guards']['Result'], 'Selftest': summary['Selftest'], 'Privacy': priv['Result']}, ensure_ascii=False) + '\n')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))
