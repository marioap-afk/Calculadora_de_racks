#!/usr/bin/env python3
"""Guardas mecánicas del alcance de la candidata I-62 A-7 (ronda 1; decisiones §92, punto 7).

Uso (siempre con `python -I`; solo lectura del repositorio de I-62; sin red; nunca ejecuta runtimes de IA ni CLI de modelos):
  python -I a7-guards.py preview --cand <I-62-A-7.md> --evid <dir> --out <json> [--repo <repo I-62>] [--base <sha>] [--tmp <dir>]
  python -I a7-guards.py run --base <sha> --head <sha> --out <json> [--repo <repo I-62>]
  python -I a7-guards.py selftest --cand <I-62-A-7.md> --evid <dir> --out <json> [--repo <repo I-62>] [--base <sha>] [--tmp <dir>]
  python -I a7-guards.py privacy --dir <dir>            (G7 sobre un directorio; código 0 limpio, 1 si hay hallazgos)

`preview` construye un commit hipotético sobre <base> en un repositorio desnudo temporal propio (alternates de solo lectura al almacén de
objetos del repositorio de I-62; ni refs ni índice ni árbol del repositorio de I-62 cambian) con la candidata en docs/initiatives/I-62-A-7.md y
el contenido de <evid> bajo docs/automation/evidence/I-62-A7/, y aplica las guardas de `run` a (base, commit hipotético).
`selftest` mata, con un mutante por condición, cada condición de cada guarda (las guardas son nuevas; sin batería sobre componentes inalterados).

Guardas:
  G1 el commit solo AÑADE docs/initiatives/I-62-A-7.md y archivos bajo docs/automation/evidence/I-62-A7/ (ninguna modificación ni borrado) y
     añade la enmienda.
  G2 superficies protegidas sin cambio entre base y head: V14, registro del Freeze, A-1..A-6 (además por blob: los de la cabecera de A-7 y los
     fijados aquí), AUTOMATION_PLAN, RAE, esquemas, adapters, decisiones, estado, evidencia fuera de I-62-A7/, src/, tests/, I-64, I-65 y el kit
     a6m7-measure.
  G3 cada literal de A-7 §2 (L1-L15) está en las líneas citadas del blob fijado de su fuente (citas «…» literales; filas parafraseadas por
     anclas fijadas aquí), con el blob abreviado de la fila igual al fijado; y las dos referencias adicionales del texto (fin de A6-8 en A-6
     L815-L816; determinación custodiada en 703f1c3f).
  G4 estructura de A7-1: frase de alcance, cláusula «sin cambiar el texto de §20.3.1, de B.9, de AUTOMATION_PLAN 16.24», siete puntos
     numerados con sus rótulos y en orden, categorías (a)-(d) en el punto 1, ubicación tras A6-8, tabla de §3.3 con las cuatro categorías; y el
     texto corregido tras el preflight: regla 1 (al menos una categoría; con varias, el blob de cada referencia), 1 (a) estrechada, 2 (a) con
     Evaluate y Validate y con la repetición admitida, 2 (c) con la ruta de la tarea o del control y el RunId de los bytes, 5 (a) sin entrada,
     5 (c) sin contrato o delegación no hay lanzamiento, ninguna frase superada de la ronda 1, residuo declarado de §3.5 con su comprobación
     por cobertura (S-03) y sin el criterio superado de blobs distintos, 2 (c) con el registro de ruta y blob de cada artefacto, Q-A7-06 y la
     columna de ausencia de §3.3.
  G5 cabecera: State PROPUESTA (ronda 1), Architect review NOT LAUNCHED, Classification MATERIAL POST-FREEZE AMENDMENT, Owner decision no
     requerida, sin AGREED de A-7; §11 con la revisión del Architect de la ronda 1 NOT LAUNCHED.
  G6 tabla de materialidad: M-01 no, M-02..M-05 sí, M-06..M-08 no, con motivo, y la misma partición en la cabecera.
  G7 privacidad de lo añadido: ni el nombre de usuario de Windows, ni la identidad Git del host (leídos en tiempo de ejecución, nunca
     escritos), ni rutas C:\\Users\\<nombre>, ni direcciones de correo salvo *.invalid y noreply@anthropic.com.
Los blobs de los textos se leen por su objeto Git. Lecturas Git con --no-replace-objects y GIT_OPTIONAL_LOCKS=0. Nunca escribe la ruta del
repositorio ni identidades del host.
"""
import hashlib, json, os, re, shutil, subprocess, sys

sys.dont_write_bytecode = True
BASE = 'bb439090f6798a502c077da450edc1319e416fe5'
A7 = 'docs/initiatives/I-62-A-7.md'
EVID = 'docs/automation/evidence/I-62-A7/'
V14 = 'docs/initiatives/I-62-proposal-v14.md'
FREEZE = 'docs/initiatives/I-62-consensus-freeze.md'
AP = 'docs/AUTOMATION_PLAN.md'
RAE = 'docs/automation/agent-execution/README.md'
A6 = 'docs/initiatives/I-62-A-6.md'
PINNED = {V14: '34ad80ea1bfff144bfc5169f62920a4c904c1bfa', FREEZE: '0f6b860837e2478db8525e1fb30a485bc4646816',
          'docs/initiatives/I-62-A-1.md': 'c01899a72b940503bb85a0fab42bc085c603fd0f',
          'docs/initiatives/I-62-A-2.md': 'f1e1d6f08d3cf677500794c7d0019433a4dc7d4e',
          'docs/initiatives/I-62-A-3.md': 'ea6721f78cb63fbc4f9d99563f3262eb36a32c5d',
          'docs/initiatives/I-62-A-4.md': '5e2ba68efdc1b902aa1c28aff4453b4b6973be7f',
          'docs/initiatives/I-62-A-5.md': 'ad94ceedacbf3dbf70f0217a583978d0e32592fc',
          A6: '4bc919dd4768ab81abc74b70d3a4a3821c07feb5', AP: 'f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5',
          RAE: '592dcfd45e8743fd83b3423249fbd3c73e3d3055'}
PROTECTED_PREFIXES = ['docs/automation/agent-execution/schemas/', 'docs/automation/agent-execution/adapters/', 'docs/automation/decisions/',
                      'docs/automation/state/', 'src/', 'tests/', 'docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/']
SOURCES = {'V14': V14, 'AP': AP, 'RAE': RAE, 'A-6': A6}
# anchors of the rows (or parts of rows) of §2 that paraphrase instead of quoting; each anchor must be on the cited lines
ANCHORS = {'L7': ['{UNIT_DOC, UNIT_CHANGE} → AR', 'EXTERNAL y P = I62 → MainSha_eval'],
           'L8': ['MV-3 | **completitud de archivos:** {p bajo `Surfaces` : blob(EFF^1, p) ≠ blob(EFF, p)}', 'MV-4 | MODIFIED ⇒ `BaseBlob` = blob(EFF^1, p)',
                  'con H1 = secciones en EFF^1 y H2 = secciones en EFF', '16.3 en EFF sin el puntero = 16.3 en EFF^1'],
           'L9': ['el objeto exacto, sus registros de revisión y sus autoridades', 'lectura registrada fuera del cierre es INVALID_REVIEW_CONTEXT (P-22)'],
           'L12': ['(iv) un término omitido no se sustituye por ningún otro valor', '(v) la derivación registra cada término omitido con su causa'],
           'L13': ['cierre de VERIFY de la regla 7) anula esa medición']}
EXTRA_REFS = [('A-6', '815-816', '…otra corrección de A-6 y otra determinación del Owner.', 'otra corrección de A-6 y otra determinación del Owner.')]
DETERMINATION = ('703f1c3f', 'docs/automation/evidence/I-62-A6/m7/s91/i6-determination.md')
SCOPE = ('**Admisión de los insumos del cierre de VERIFY de FX-02 (A-7).** Solo para el cierre efectivo de insumos (`rackcad-input-closure/v1`) de '
         'las invocaciones VERIFY del EXECUTION_CONTROLLER de la Topología A (FX-02) en F6 —la verificación, su reverificación de 16.7 y cada control '
         'negativo con invocación— y de la invocación de medición de la regla de A-6 sobre la elegibilidad, con o sin el instrumento de A-6')
NOCHANGE = 'sin cambiar el texto de §20.3.1, de B.9, de AUTOMATION_PLAN 16.24'
POINTS = ['categorías:', 'origen, identidad y revisión:', 'comprobación de blobs (I6):', 'fidelidad:', 'ausencia:', 'auditoría y derivación:', 'límites:']
CATS = ['(a) **autoridades:**', '(b) **objetos versionados del `Target`:**', '(c) **artefactos custodiados:**', '(d) **instrumento:**']
REQUIRED_A71 = [
    ('rule1', '1. **categorías:** cada entrada `{Path, Blob}` del cierre pertenece al menos a una de estas categorías, y ninguna otra entrada se '
              'admite; si pertenece a más de una, su blob coincide con la referencia de cada una de ellas:'),
    ('rule1a', 'los archivos cuyo contenido lee la resolución de autoridades de 16.13: el mapa y su esquema, cada archivo MODIFIED del mapa, y '
               '`WORKFLOW.md` y `AUTOMATION_PLAN.md` en las lecturas de Evaluate y de Validate;'),
    ('rule2a', '`MainSha_eval` para las autoridades EXTERNAL y para `WORKFLOW.md` y `AUTOMATION_PLAN.md` en Evaluate; EFF para el mapa, su esquema y '
               'cada archivo MODIFIED, y para `WORKFLOW.md` y `AUTOMATION_PLAN.md` en Validate; EFF^1 para cada archivo MODIFIED y para '
               '`AUTOMATION_PLAN.md` en Validate.'),
    ('rule2-repeat', 'Una misma ruta puede figurar con un blob por cada revisión de lectura en que difiera, y una misma entrada puede figurar en más de '
                     'una lista del cierre cuando 16.24 lo exige;'),
    ('rule2c', 'la delegación y la entrega van bajo `artifacts/orchestration/`, en la ruta de la tarea o del control (RAE §10), en un directorio cuyo '
               'nombre es el `RunId` de sus propios bytes;'),
    ('rule2c-register', 'que nombra cuál es el contrato, la delegación, la entrega y el TRX y registra la ruta y el blob de cada uno;'),
    ('rule5a', '(a) una entrada de (a) que no existe en su revisión no aporta entrada; su ausencia la tratan las reglas que la exigen (16.3, 16.13, '
               'por ejemplo como MAP_INVALID, o 16.24) y nada la sustituye;'),
    ('rule5c', '(c) sin un contrato y una delegación presentes, con el blob registrado y legibles, (a) y (b) no se pueden calcular y no hay '
               'lanzamiento; la entrega o el TRX que no existen, cuyos bytes no tienen el blob registrado o que no son legibles no aportan entrada;'),
]
SUPERSEDED_A71 = ['pertenece a exactamente una de estas categorías', 'Una ruta figura una vez por cada blob distinto', 'deja el cierre sin calcular',
                  '`artifacts/orchestration/<UnitId>/<TaskId>/<Attempt>/<RunId>/`', 'los archivos del mapa y el punto de entrada']
RESIDUAL = ['**Residuo declarado (lecturas de `clausemap` fuera de las revisiones de 16.13).**', 'un rechazo falso, nunca una aceptación falsa']
RESIDUAL_CHECK = ('si alguna lectura de contenido que `clausemap` hace de una cita del contrato cae en una revisión en que el blob de esa ruta no es el '
                  'de una entrada del cierre, no hay lanzamiento y el caso vuelve al Coordinator (S-03); una revisión en que la ruta no existe no aporta '
                  'lectura. Una cita cuya lectura sí está fijada por el cierre (por ejemplo, `AUTOMATION_PLAN.md` en EFF^1, categoría (a)) no bloquea.')
SUPERSEDED_35 = ['si alguna cita del contrato tiene blobs distintos en esas revisiones']
TABLE_A_ABSENCE = 'sin entrada; la tratan 16.3, 16.13 (p. ej., MAP_INVALID) o 16.24; sin sustitución'
TABLE_C_ABSENCE = 'contrato o delegación ausentes: sin cierre y sin lanzamiento'
STATE_TABLE = '| Architect independiente | ronda 1: NOT LAUNCHED |'
MATERIALITY = {'M-01': 'no', 'M-02': '**sí**', 'M-03': '**sí**', 'M-04': '**sí**', 'M-05': '**sí**', 'M-06': 'no', 'M-07': 'no', 'M-08': 'no'}
EMAIL = re.compile(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}')
USERS_PATH = re.compile(r'[A-Za-z]:(?:\\\\|\\|/)+Users(?:\\\\|\\|/)+([^\\/\s"\'<>%]+)', re.I)


def default_repo():
    return os.path.join(os.environ.get('USERPROFILE', ''), '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')


def genv(extra=None):
    e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    e['GIT_OPTIONAL_LOCKS'] = '0'
    e['GIT_TERMINAL_PROMPT'] = '0'
    e.update(extra or {})
    return e


def git(repo, *args, inp=None, extra=None):
    p = subprocess.run(['git', '--no-replace-objects', '-C', repo] + list(args), input=inp, capture_output=True, env=genv(extra),
                       stdin=None if inp is not None else subprocess.DEVNULL)
    return p.returncode, p.stdout


def gout(repo, *args):
    rc, out = git(repo, *args)
    if rc != 0:
        raise SystemExit('git failed: %s' % args[0])
    return out


_TREES, _TEXTS = {}, {}


def tree(repo, rev):
    """Path -> blob id of a commit (memoized by commit id: a commit id names one tree)."""
    if re.fullmatch(r'[0-9a-f]{40}', rev) and rev in _TREES:
        return dict(_TREES[rev])
    t = {}
    for rec in gout(repo, 'ls-tree', '-r', '-z', '--full-tree', rev).split(b'\0'):
        if rec:
            meta, name = rec.split(b'\t', 1)
            t[name.decode('utf-8', 'surrogateescape')] = meta.decode('ascii').split()[2]
    if re.fullmatch(r'[0-9a-f]{40}', rev):
        _TREES[rev] = dict(t)
    return t


def read_blobs(repo, oids):
    """Bytes of several blobs with one `git cat-file --batch` (the ids are passed on stdin and declared here)."""
    if not oids:
        return {}
    rc, out = git(repo, 'cat-file', '--batch', inp=('\n'.join(oids) + '\n').encode('ascii'))
    res, i = {}, 0
    for oid in oids:
        j = out.index(b'\n', i)
        hdr = out[i:j].decode('ascii').split()
        size = int(hdr[2])
        res[oid] = out[j + 1:j + 1 + size]
        i = j + 1 + size + 1
    return res


def blob_text(repo, oid):
    if oid not in _TEXTS:
        _TEXTS[oid] = gout(repo, 'cat-file', 'blob', oid).decode('utf-8').replace('\r\n', '\n')
    return _TEXTS[oid]


def norm(s):
    return re.sub(r'\s+', ' ', s).strip()


def lines_norm(text, a, b):
    ls = text.split('\n')[a - 1:b]
    return norm(' '.join(re.sub(r'^\s*>\s?', '', x) for x in ls))


def host_identities(repo):
    vals = set()
    for k in ('USERNAME', 'USER'):
        if os.environ.get(k):
            vals.add(os.environ[k])
    if os.environ.get('USERPROFILE'):
        vals.add(os.path.basename(os.path.normpath(os.environ['USERPROFILE'])))
    for key in ('user.name', 'user.email'):
        rc, out = git(repo, 'config', '--get', key)
        if rc == 0 and out.strip():
            vals.add(out.decode('utf-8', 'replace').strip())
    return set(v for v in vals if len(v) >= 3)


def privacy_findings(name, text, idents):
    f = []
    low = text.lower()
    for v in idents:
        if v.lower() in low:
            f.append('%s: host identity present' % name)
    for m in USERS_PATH.finditer(text):
        if m.group(1) not in ('<usuario>', 'Public', 'Default'):
            f.append('%s: C:' % name + chr(92) + 'Users' + chr(92) + '<name> path')
    for m in EMAIL.finditer(text):
        e = m.group(0)
        if not (e.lower().endswith('.invalid') or e.lower() == 'noreply@anthropic.com'):
            f.append('%s: e-mail address' % name)
    return sorted(set(f))


# ------------------------------------------------------------------ guards (on texts and trees)
def section(text, start, end):
    i = text.index(start)
    j = text.index(end, i + len(start)) if end else len(text)
    return text[i:j]


def g1(diff):
    out = []
    added = [p for st, p in diff if st == 'A']
    for st, p in diff:
        if st != 'A':
            out.append('G1-only-adds: %s %s' % (st, p))
        elif not (p == A7 or p.startswith(EVID)):
            out.append('G1-allowed-paths: added outside the allowed set: %s' % p)
    if A7 not in added:
        out.append('G1-amendment-added: %s not added' % A7)
    return out


def g2(base_t, head_t, a7text):
    out = []
    for p, b in PINNED.items():
        if base_t.get(p) != b or head_t.get(p) != b:
            out.append('G2-pinned: %s not at its pinned blob in base and head' % p)
    claims = dict(re.findall(r'(docs/initiatives/I-62-A-[1-6]\.md), blob ([0-9a-f]{40})', a7text))
    claims.update(dict(re.findall(r'record\s+= (docs/initiatives/I-62-consensus-freeze\.md) \(blob ([0-9a-f]{40})\)', a7text)))
    m = re.search(r'proposal\s+= (docs/initiatives/I-62-proposal-v14\.md)\n\s+commit\s+= [0-9a-f]{40}\n\s+blob\s+= ([0-9a-f]{40})', a7text)
    if m:
        claims[m.group(1)] = m.group(2)
    want = [p for p in PINNED if p not in (AP, RAE)]
    for p in want:
        if claims.get(p) != PINNED[p]:
            out.append('G2-header-claims: the A-7 header does not state %s at its pinned blob' % p)
    keys = set(base_t) | set(head_t)
    for p in sorted(keys):
        prot = (p in PINNED or any(p.startswith(x) for x in PROTECTED_PREFIXES)
                or (p.startswith('docs/automation/evidence/') and not p.startswith(EVID)) or re.search(r'I-6[45]', p))
        if prot and base_t.get(p) != head_t.get(p):
            kind = 'kit' if p.startswith(PROTECTED_PREFIXES[-1]) else ('I-64/I-65' if re.search(r'I-6[45]', p) else 'surface')
            out.append('G2-protected(%s): %s changed' % (kind, p))
    return out


def g3(a7text, src_text, det_present):
    out = []
    sec2 = section(a7text, '## 2. Fuentes', '## 3. Delta')
    rows = [r for r in sec2.split('\n') if re.match(r'\| L\d+ \|', r)]
    ids = [re.match(r'\| (L\d+) \|', r).group(1) for r in rows]
    if ids != ['L%d' % i for i in range(1, 16)]:
        out.append('G3-rows: §2 does not list L1..L15 in order')
    current = None
    for r in rows:
        cells = [c.strip() for c in r.strip().strip('|').split('|')]
        if len(cells) < 4:
            out.append('G3-rows: malformed row %s' % cells[:1])
            continue
        lid, src, lines, lit = cells[0], cells[1], cells[2], ' | '.join(cells[3:])
        key = next((k for k in SOURCES if src.startswith(k + ' ')), None)
        if key is None:
            out.append('G3-source: %s cites an unknown source' % lid)
            continue
        current = key
        m = re.search(r'\(`([0-9a-f]{7,40})`\)', src)
        if m and not PINNED[SOURCES[key]].startswith(m.group(1)):
            out.append('G3-blob: %s cites blob %s, not the pinned %s' % (lid, m.group(1), PINNED[SOURCES[key]][:8]))
        text = src_text[SOURCES[key]]
        ranges = []
        for part in lines.split(','):
            a, _, b = part.strip().partition('-')
            if not a.isdigit() or (b and not b.isdigit()):
                out.append('G3-lines: %s line reference %r' % (lid, lines))
                ranges = []
                break
            ranges.append((int(a), int(b or a)))
        segs = [lines_norm(text, a, b) for a, b in ranges]
        quotes = re.findall(r'«(.*?)»', lit)
        needles = [norm(q) for q in quotes] + [norm(x) for x in ANCHORS.get(lid, [])]
        if not needles:
            out.append('G3-literal: %s has no quote and no anchor' % lid)
        for n in needles:
            if not any(n in s for s in segs):
                out.append('G3-literal: %s: %r not on the cited lines %s of %s' % (lid, n[:60], lines, SOURCES[key]))
    for key, rng, a7quote, srcquote in EXTRA_REFS:
        a, b = (int(x) for x in rng.split('-'))
        if norm(a7quote) not in norm(a7text) or ('A-6 L%d-L%d' % (a, b)) not in a7text:
            out.append('G3-extra: the A-7 text no longer cites the end of A6-8 at A-6 L%d-L%d' % (a, b))
        if norm(srcquote) not in lines_norm(src_text[SOURCES[key]], a, b):
            out.append('G3-extra: end of A6-8 not at A-6 L%d-L%d' % (a, b))
    if DETERMINATION[0] not in a7text or not det_present:
        out.append('G3-extra: the determination is not custodied at %s' % DETERMINATION[0])
    return out


def g4(a7text):
    out = []
    try:
        s32 = section(a7text, '### 3.2 A7-1', '### 3.3')
        s33 = section(a7text, '### 3.3 Categorías de entrada', '### 3.4')
    except ValueError:
        return ['G4-sections: §3.2 or §3.3 missing']
    quote = norm(' '.join(re.sub(r'^>\s?', '', x) for x in s32.split('\n') if x.startswith('>')))
    if not quote.startswith('«' + norm(SCOPE)):
        out.append('G4-scope: the scope phrase of A7-1 differs')
    if norm(NOCHANGE) not in quote:
        out.append('G4-nochange: the clause «%s» is missing' % NOCHANGE)
    if not quote.endswith('»'):
        out.append('G4-quote: A7-1 is not one closed quotation')
    pos = []
    for k, label in enumerate(POINTS, 1):
        hits = [m.start() for m in re.finditer(r'(?<![\d(])%d\. \*\*%s\*\*' % (k, re.escape(label)), quote)]
        if len(hits) != 1:
            out.append('G4-points: point %d «%s» found %d times' % (k, label, len(hits)))
        else:
            pos.append(hits[0])
    if len(pos) == 7 and pos != sorted(pos):
        out.append('G4-points: points out of order')
    if re.search(r'(?<![\d(])8\. \*\*', quote):
        out.append('G4-points: more than seven points')
    p1 = quote[quote.find('1. **categorías:**'):quote.find('2. **origen')] if '1. **categorías:**' in quote else ''
    cp = [p1.find(norm(c)) for c in CATS]
    if any(x < 0 for x in cp) or cp != sorted(cp):
        out.append('G4-categories: (a)-(d) not all in point 1, in order')
    if 'Ubicación: al final de V14 D.3, a continuación del párrafo de A6-8' not in s32:
        out.append('G4-location: A7-1 is not placed after A6-8 at the end of V14 D.3')
    rows = [r for r in s33.split('\n') if r.startswith('| **(')]
    if [re.match(r'\| \*\*(\([a-d]\))', r).group(1) for r in rows] != ['(a)', '(b)', '(c)', '(d)']:
        out.append('G4-table: §3.3 does not list the categories (a)-(d)')
    # the text as corrected after the preflight (findings F1-F5, F7, F9) and the declared residual (F6)
    for cond, phrase in REQUIRED_A71:
        if norm(phrase) not in quote:
            out.append('G4-%s: A7-1 lacks «%s…»' % (cond, norm(phrase)[:60]))
    for phrase in SUPERSEDED_A71:
        if norm(phrase) in quote:
            out.append('G4-superseded: A7-1 keeps the round-1 phrase «%s»' % phrase)
    try:
        s35 = norm(section(a7text, '### 3.5 Lo que A-7 no hace', '## 4.'))
        s8 = section(a7text, '## 8. Preguntas para la revisión', '## 9.')
    except ValueError:
        return out + ['G4-residual: §3.5 or §8 missing']
    if not all(norm(x) in s35 for x in RESIDUAL):
        out.append('G4-residual: §3.5 lacks the declared residual of clausemap')
    if norm(RESIDUAL_CHECK) not in s35:
        out.append('G4-residual-coverage: §3.5 lacks the coverage criterion of the fail-closed check (S-03)')
    if any(norm(x).lower() in s35.lower() for x in SUPERSEDED_35):
        out.append('G4-superseded-35: §3.5 keeps the superseded criterion «distinct blobs in those revisions»')
    if not re.search(r'^\| Q-A7-06 \| .*clausemap.*\|\s*$', s8, re.M):
        out.append('G4-q06: §8 lacks Q-A7-06 on the residual')
    ra = next((r for r in rows if r.startswith('| **(a)')), '')
    rc = next((r for r in rows if r.startswith('| **(c)')), '')
    if TABLE_A_ABSENCE not in ra or TABLE_C_ABSENCE not in rc:
        out.append('G4-table-absence: the absence column of §3.3 for (a) or (c) is not the corrected one')
    return out


def g5(a7text):
    out = []
    head = a7text[:a7text.index('## 1. Alcance')]
    for label, pat in (('state', r'\nState:\nPROPUESTA \(ronda 1\)\.'), ('architect', r'\nArchitect review:\nNOT LAUNCHED\.'),
                       ('classification', r'\nClassification: MATERIAL POST-FREEZE AMENDMENT, candidata'),
                       ('owner', r'\nOwner decision:\nno requerida'), ('amendment', r'Amendment: A-7 \(candidata, ronda 1;')):
        if not re.search(pat, head):
            out.append('G5-%s: header field differs' % label)
    if re.search(r'A-7 (?:=\s*)?AGREED|State:\s*\n\s*AGREED', a7text):
        out.append('G5-no-agreed: A-7 declared AGREED')
    if STATE_TABLE not in a7text:
        out.append('G5-state-table: §11 does not keep the Architect review of round 1 NOT LAUNCHED')
    return out


def g6(a7text):
    out = []
    sec = section(a7text, '## 5. Materialidad', '## 6.')
    got = {}
    for r in sec.split('\n'):
        m = re.match(r'\| (M-0[1-8]) \| ([^|]+) \| ([^|]*) \|', r)
        if m:
            got[m.group(1)] = (m.group(2).strip(), m.group(3).strip())
    for k, want in MATERIALITY.items():
        if k not in got or got[k][0] != want:
            out.append('G6-values: %s is %r, expected %r' % (k, got.get(k, ('<absent>',))[0], want))
        elif not got[k][1]:
            out.append('G6-motive: %s without motive' % k)
    if 'M-02, M-03, M-04 y M-05 sí; M-01, M-06, M-07 y M-08 no' not in norm(a7text[:a7text.index('## 1. Alcance')]):
        out.append('G6-header: the header partition of M-01..M-08 differs')
    return out


def g7(files, idents):
    out = []
    for name, data in files:
        try:
            text = data.decode('utf-8')
        except UnicodeDecodeError:
            text = data.decode('latin-1')
        out += ['G7-' + x for x in privacy_findings(name, text, idents)]
    return out


def evaluate(repo, base, head, objrepo=None):
    """All guards on (base, head); objrepo is where head lives (the temporary repository in preview)."""
    objrepo = objrepo or repo
    rc, out = git(objrepo, 'diff', '--name-status', '--no-renames', '-z', base, head)
    toks = out.split(b'\0')
    diff = [(toks[i].decode(), toks[i + 1].decode('utf-8')) for i in range(0, len(toks) - 1, 2)]
    base_t, head_t = tree(objrepo, base), tree(objrepo, head)
    a7text = blob_text(objrepo, head_t[A7]) if A7 in head_t else ''
    src_text = dict((p, blob_text(objrepo, PINNED[p])) for p in SOURCES.values())
    det = git(objrepo, 'cat-file', '-e', '%s:%s' % DETERMINATION)[0] == 0
    want = [(p, head_t[p]) for st, p in diff if st == 'A' and p in head_t]
    data = read_blobs(objrepo, [o for _, o in want])
    added = [(p, data[o]) for p, o in want]
    res = {}
    res['G1'] = g1(diff)
    res['G2'] = g2(base_t, head_t, a7text)
    res['G3'] = g3(a7text, src_text, det) if a7text else ['G3: no amendment text']
    res['G4'] = g4(a7text) if a7text else ['G4: no amendment text']
    res['G5'] = g5(a7text) if a7text else ['G5: no amendment text']
    res['G6'] = g6(a7text) if a7text else ['G6: no amendment text']
    res['G7'] = g7(added, host_identities(repo))
    return {'Base': base, 'Head': head, 'A7Blob': head_t.get(A7), 'Added': len(added),
            'Guards': dict((k, {'Status': 'PASS' if not v else 'FAIL', 'Findings': v}) for k, v in res.items()),
            'Result': 'PASS' if not any(res.values()) else 'FAIL'}


# ------------------------------------------------------------------ hypothetical commits (preview, selftest)
class Scratch(object):
    def __init__(self, repo, tmp):
        tmp = os.path.abspath(tmp)
        self.repo = repo
        self.dir = os.path.join(tmp, 'a7-guards-preview.git')
        if os.path.isdir(self.dir):
            def onerr(func, path, exc):
                os.chmod(path, 0o666)
                func(path)
            shutil.rmtree(self.dir, onerror=onerr)
        subprocess.run(['git', 'init', '-q', '--bare', self.dir], check=True, capture_output=True, env=genv())
        common = gout(repo, 'rev-parse', '--path-format=absolute', '--git-common-dir').decode('utf-8').strip()
        with open(os.path.join(self.dir, 'objects', 'info', 'alternates'), 'w', encoding='utf-8', newline='\n') as fh:
            fh.write(os.path.join(common, 'objects').replace('\\', '/') + '\n')
        self.index = os.path.join(tmp, 'a7-guards-preview.index')
        self.oids = {}

    def commit(self, base, files, removals=()):
        env = {'GIT_INDEX_FILE': self.index, 'GIT_AUTHOR_NAME': 'A7 preview', 'GIT_AUTHOR_EMAIL': 'preview@a7.invalid',
               'GIT_COMMITTER_NAME': 'A7 preview', 'GIT_COMMITTER_EMAIL': 'preview@a7.invalid',
               'GIT_AUTHOR_DATE': '2026-10-17T00:00:00+00:00', 'GIT_COMMITTER_DATE': '2026-10-17T00:00:00+00:00'}
        if os.path.exists(self.index):
            os.remove(self.index)
        if git(self.dir, 'read-tree', base, extra=env)[0] != 0:
            raise SystemExit('read-tree failed')
        info = []
        for p, data in sorted(files.items()):
            key = hashlib.sha256(data).hexdigest()
            if key not in self.oids:
                rc, oid = git(self.dir, 'hash-object', '-w', '--no-filters', '--stdin', inp=data, extra=env)
                self.oids[key] = oid.decode().strip()
            info.append('100644 %s\t%s' % (self.oids[key], p))
        for p in removals:
            info.append('0 %s\t%s' % ('0' * 40, p))
        if git(self.dir, 'update-index', '--index-info', inp=('\n'.join(info) + '\n').encode('utf-8'), extra=env)[0] != 0:
            raise SystemExit('update-index failed')
        rc, t = git(self.dir, 'write-tree', extra=env)
        rc, c = git(self.dir, 'commit-tree', t.decode().strip(), '-p', base, '-m', 'A-7 preview (hypothetical commit)', extra=env)
        return c.decode().strip()


def evid_files(evid):
    out = {}
    for root, dirs, names in os.walk(evid):
        dirs.sort()
        for n in sorted(names):
            if n.endswith('.pyc') or '__pycache__' in root:
                continue
            full = os.path.join(root, n)
            rel = os.path.relpath(full, evid).replace('\\', '/')
            with open(full, 'rb') as fh:
                out[EVID + rel] = fh.read()
    return out


def preview(repo, base, cand, evid, tmp, skip=()):
    with open(cand, 'rb') as fh:
        a7 = fh.read()
    files = dict((p, d) for p, d in evid_files(evid).items() if p not in skip)
    files[A7] = a7
    sc = Scratch(repo, tmp)
    head = sc.commit(base, files)
    r = evaluate(repo, base, head, sc.dir)
    r['Mode'] = 'preview'
    r['CandidateBlob'] = r.pop('A7Blob')
    return r, sc, files


def mutants(a7, idents):
    t = a7.decode('utf-8')
    user = sorted(idents)[0] if idents else 'nobody'

    def rep(old, new, count=1):
        if old not in t:
            raise SystemExit('selftest: anchor not found for a mutant')
        return t.replace(old, new, count).encode('utf-8')
    return [
        ('G1', 'only-adds', 'modifica un archivo existente (docs/automation/decisions/I-62.md)', None, {'modify': 'docs/automation/decisions/I-62.md'}),
        ('G1', 'allowed-paths', 'añade un archivo fuera de lo permitido (docs/automation/evidence/I-62-A6/extra.txt)', None, {'add': 'docs/automation/evidence/I-62-A6/extra.txt'}),
        ('G1', 'amendment-added', 'el commit no añade la enmienda', None, {'drop_a7': True}),
        ('G2', 'protected', 'cambia el archivo de estado de I-62 (superficie protegida)', None, {'modify': 'docs/automation/state/I-62.yml'}),
        ('G2', 'kit', 'cambia un archivo del kit a6m7-measure', None, {'modify': 'docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/README.md'}),
        ('G2', 'I-64/I-65', 'añade una ruta de I-65', None, {'add': 'docs/initiatives/I-65-x.md'}),
        ('G2', 'pinned', 'cambia el blob de A-5 en el commit', None, {'modify': 'docs/initiatives/I-62-A-5.md'}),
        ('G2', 'header-claims', 'la cabecera de A-7 cita otro blob de A-5', rep('blob ad94ceedacbf3dbf70f0217a583978d0e32592fc', 'blob ad94ceedacbf3dbf70f0217a583978d0e32592fd'), {}),
        ('G3', 'literal', 'L1 con una palabra cambiada en la cita', rep('el objeto exacto, sus registros de revisión y sus autoridades» |', 'el objeto exacto, sus registros y sus autoridades» |'), {}),
        ('G3', 'lines', 'L4 con otra línea (2234)', rep('| L4 | V14 B.9 | 2233 |', '| L4 | V14 B.9 | 2234 |'), {}),
        ('G3', 'blob', 'L6 con otro blob abreviado de AP', rep('AP 16.3 (`f525cb1e`)', 'AP 16.3 (`f525cb1f`)'), {}),
        ('G3', 'anchor', 'L7 (fila parafraseada) con otro rango de líneas', rep('| 823-826 |', '| 833-836 |'), {}),
        ('G3', 'rows', 'falta la fila L15', rep('| L15 |', '| L16 |'), {}),
        ('G3', 'extra', 'el texto cita el fin de A6-8 en otras líneas', rep('(A-6 L815-L816)', '(A-6 L825-L826)'), {}),
        ('G4', 'scope', 'frase de alcance sin «en F6»', rep('de la Topología A (FX-02) en F6 —la', 'de la Topología A (FX-02) —la'), {}),
        ('G4', 'nochange', 'sin la cláusula «sin cambiar el texto de §20.3.1, de B.9, de AUTOMATION_PLAN 16.24»', rep('§20.3.1, de B.9, de AUTOMATION_PLAN 16.24 ni', '§20.3.1, de AUTOMATION_PLAN 16.24 ni'), {}),
        ('G4', 'points', 'el punto 7 deja de estar numerado', rep('> 7. **límites:**', '> **límites:**'), {}),
        ('G4', 'categories', 'la categoría (d) sale del punto 1', rep('>    (d) **instrumento:**', '>    (e) **instrumento:**'), {}),
        ('G4', 'location', 'otra ubicación', rep('Ubicación: al final de V14 D.3, a continuación del párrafo de A6-8', 'Ubicación: al final de V14 D.4, a continuación del párrafo de A6-6'), {}),
        ('G4', 'table', 'la tabla de §3.3 pierde la fila (c)', rep('| **(c) Artefactos custodiados**', '| (c) Artefactos custodiados'), {}),
        ('G4', 'rule1', 'regla 1 sin «al menos»', rep('pertenece al menos a una de estas categorías', 'pertenece a una de estas categorías'), {}),
        ('G4', 'rule1a', 'regla 1 (a) sin «cuyo contenido lee»', rep('los archivos cuyo contenido lee la resolución', 'los archivos que lee la resolución'), {}),
        ('G4', 'rule2a', 'regla 2 (a) excluye AUTOMATION_PLAN en EFF^1', rep('EFF^1 para cada archivo MODIFIED y para', 'EFF^1 para cada archivo MODIFIED y no para'), {}),
        ('G4', 'rule2-repeat', 'regla 2 (a): «en que coincida» en lugar de «en que difiera»', rep('revisión de lectura en que difiera', 'revisión de lectura en que coincida'), {}),
        ('G4', 'rule2c', 'regla 2 (c) sin la ruta del control', rep('en la ruta de la tarea o del control (RAE §10)', 'en la ruta de la tarea'), {}),
        ('G4', 'rule5a', 'regla 5 (a) vuelve a «deja el cierre sin calcular»', rep('que no existe en su revisión no aporta entrada', 'que no existe en su revisión deja el cierre sin calcular'), {}),
        ('G4', 'rule5c', 'regla 5 (c) sin la delegación', rep('(c) sin un contrato y una', '(c) sin una'), {}),
        ('G4', 'superseded', 'reinsertada la frase de unicidad de la ronda 1', rep('> 7. **límites:** esta regla', '> 7. **límites:** Una ruta figura una vez por cada blob distinto que deba leerse; esta regla'), {}),
        ('G4', 'residual', 'sin el rótulo del residuo declarado de §3.5', rep('**Residuo declarado (lecturas', '**Residuo (lecturas'), {}),
        ('G4', 'rule2c-register', 'regla 2 (c) sin el registro de ruta y blob de cada artefacto', rep('el TRX y registra la ruta y el blob de cada uno;', 'el TRX;'), {}),
        ('G4', 'residual-coverage', '§3.5: «es el de una entrada del cierre» cambiado', rep('el blob de esa ruta no es el de una entrada del', 'el blob de esa ruta no es el de la'), {}),
        ('G4', 'superseded-35', '§3.5: reinsertado el criterio de blobs distintos', rep('## 4. Semántica antes y después', 'Si alguna cita del contrato tiene blobs distintos en esas revisiones, no hay lanzamiento.' + chr(10) + chr(10) + '## 4. Semántica antes y después'), {}),
        ('G4', 'q06', 'sin la pregunta Q-A7-06', rep('| Q-A7-06 |', '| Q-A7-6 |'), {}),
        ('G4', 'table-absence', 'la columna de ausencia de (c) en §3.3 vuelve a «sin entrada»', rep('contrato o delegación ausentes: sin cierre y sin lanzamiento', 'contrato o delegación ausentes: sin entrada'), {}),
        ('G5', 'state-table', '§11: Architect «ronda 1: LAUNCHED»', rep('| Architect independiente | ronda 1: NOT LAUNCHED |', '| Architect independiente | ronda 1: LAUNCHED |'), {}),
        ('G5', 'state', 'State AGREED', rep('PROPUESTA (ronda 1). No aplicada', 'AGREED (ronda 1). No aplicada'), {}),
        ('G5', 'architect', 'Architect review LAUNCHED', rep('NOT LAUNCHED. Ninguna revisión formal', 'LAUNCHED. Ninguna revisión formal'), {}),
        ('G5', 'classification', 'clasificación no material', rep('Classification: MATERIAL POST-FREEZE AMENDMENT', 'Classification: NON-MATERIAL AMENDMENT'), {}),
        ('G5', 'owner', 'decisión del Owner requerida', rep('Owner decision:\nno requerida', 'Owner decision:\nrequerida'), {}),
        ('G6', 'values', 'M-03 = no', rep('| M-03 | **sí** |', '| M-03 | no |'), {}),
        ('G6', 'header', 'la cabecera declara M-05 no', rep('M-02, M-03, M-04 y M-05 sí; M-01, M-06, M-07 y M-08 no', 'M-02, M-03 y M-04 sí; M-01, M-05, M-06, M-07 y M-08 no'), {}),
        ('G7', 'host-identity', 'la identidad del host (leída en tiempo de ejecución) en el texto', rep('## 9. Separabilidad', '## 9. Separabilidad ' + user), {}),
        ('G7', 'users-path', 'una ruta de perfil de Windows con nombre', rep('## 9. Separabilidad', '## 9. Separabilidad ' + 'C:' + chr(92) + 'Users' + chr(92) + 'someone' + chr(92) + 'x'), {}),
        ('G7', 'e-mail', 'una dirección de correo', rep('## 9. Separabilidad', '## 9. Separabilidad ' + 'persona' + chr(64) + 'example.com'), {}),
    ]


EXPECT = {('G1', 'only-adds'): 'G1-only-adds', ('G1', 'allowed-paths'): 'G1-allowed-paths', ('G1', 'amendment-added'): 'G1-amendment-added',
          ('G2', 'protected'): 'G2-protected(surface)', ('G2', 'kit'): 'G2-protected(kit)', ('G2', 'I-64/I-65'): 'G2-protected(I-64/I-65)',
          ('G2', 'pinned'): 'G2-pinned', ('G2', 'header-claims'): 'G2-header-claims', ('G3', 'literal'): 'G3-literal: L1:',
          ('G3', 'lines'): 'G3-literal: L4:', ('G3', 'blob'): 'G3-blob: L6', ('G3', 'anchor'): 'G3-literal: L7:', ('G3', 'rows'): 'G3-rows',
          ('G3', 'extra'): 'G3-extra', ('G4', 'scope'): 'G4-scope', ('G4', 'nochange'): 'G4-nochange', ('G4', 'points'): 'G4-points',
          ('G4', 'categories'): 'G4-categories', ('G4', 'location'): 'G4-location', ('G4', 'table'): 'G4-table', ('G5', 'state'): 'G5-state',
          ('G5', 'architect'): 'G5-architect', ('G5', 'classification'): 'G5-classification', ('G5', 'owner'): 'G5-owner',
          ('G6', 'values'): 'G6-values', ('G6', 'header'): 'G6-header', ('G7', 'host-identity'): 'host identity present',
          ('G7', 'users-path'): 'Users' + chr(92) + '<name> path', ('G7', 'e-mail'): 'e-mail address',
          ('G4', 'rule1'): 'G4-rule1:', ('G4', 'rule1a'): 'G4-rule1a:', ('G4', 'rule2a'): 'G4-rule2a:', ('G4', 'rule2-repeat'): 'G4-rule2-repeat:',
          ('G4', 'rule2c'): 'G4-rule2c:', ('G4', 'rule5a'): 'G4-rule5a:', ('G4', 'rule5c'): 'G4-rule5c:', ('G4', 'superseded'): 'G4-superseded',
          ('G4', 'residual'): 'G4-residual', ('G4', 'q06'): 'G4-q06', ('G4', 'table-absence'): 'G4-table-absence', ('G5', 'state-table'): 'G5-state-table',
          ('G4', 'rule2c-register'): 'G4-rule2c-register:', ('G4', 'residual-coverage'): 'G4-residual-coverage', ('G4', 'superseded-35'): 'G4-superseded-35'}


def selftest(repo, base, cand, evid, tmp):
    r0, sc, files = preview(repo, base, cand, evid, tmp)
    rows = [{'Guard': 'ALL', 'Condition': 'candidate', 'Mutant': 'la candidata sin mutar', 'Killed': None, 'Result': r0['Result']}]
    idents = host_identities(repo)
    head_t = tree(sc.dir, base)
    for guard, cond, what, text, op in mutants(files[A7], idents):
        f = dict(files)
        rem = []
        if text is not None:
            f[A7] = text
        if op.get('drop_a7'):
            f.pop(A7)
        if op.get('add'):
            f[op['add']] = b'mutante\n'
        if op.get('modify'):
            f[op['modify']] = gout(sc.dir, 'cat-file', 'blob', head_t[op['modify']]) + b'\nmutante\n'
        if op.get('drop_a7'):
            # keep a text to evaluate G3-G6 on: the amendment is put outside the allowed path (it is not added as I-62-A-7.md)
            f['docs/automation/evidence/I-62-A7/I-62-A-7.md.txt'] = files[A7]
        head = sc.commit(base, f, rem)
        r = evaluate(repo, base, head, sc.dir)
        fnd = r['Guards'][guard]['Findings']
        want = EXPECT[(guard, cond)]
        killed = any(want in x for x in fnd)
        rows.append({'Guard': guard, 'Condition': cond, 'Mutant': what, 'Killed': killed, 'GuardStatus': r['Guards'][guard]['Status'],
                     'ExpectedFinding': want.split(':')[0], 'Findings': len(fnd)})
    ok = r0['Result'] == 'PASS' and all(x['Killed'] for x in rows[1:])
    return {'Mode': 'selftest', 'Base': base, 'Candidate': r0, 'Mutants': rows, 'MutantsTotal': len(rows) - 1,
            'Killed': sum(1 for x in rows[1:] if x['Killed']), 'Result': 'PASS' if ok else 'FAIL'}


def main(argv):
    if not argv:
        sys.stderr.write(__doc__)
        return 64
    mode, rest = argv[0], argv[1:]
    opts = {}
    i = 0
    while i < len(rest):
        if not rest[i].startswith('--') or i + 1 >= len(rest):
            sys.stderr.write(__doc__)
            return 64
        opts[rest[i][2:]] = rest[i + 1]
        i += 2
    repo = opts.get('repo') or default_repo()
    if mode == 'privacy':
        idents = host_identities(repo)
        fs = []
        for root, dirs, names in os.walk(opts['dir']):
            dirs[:] = [d for d in dirs if d != '__pycache__']
            for n in names:
                with open(os.path.join(root, n), 'rb') as fh:
                    fs.append((os.path.relpath(os.path.join(root, n), opts['dir']).replace('\\', '/'), fh.read()))
        f = g7(fs, idents)
        sys.stdout.write(json.dumps({'Files': len(fs), 'Findings': f, 'Result': 'PASS' if not f else 'FAIL'}) + '\n')
        return 0 if not f else 1
    tmp = opts.get('tmp') or os.path.join(os.path.dirname(os.path.abspath(opts.get('out', '.'))), '..', 'work', 'guards-tmp')
    os.makedirs(tmp, exist_ok=True)
    if mode == 'run':
        r = evaluate(repo, opts['base'], opts['head'])
        r['Mode'] = 'run'
    elif mode == 'preview':
        r, _sc, _f = preview(repo, opts.get('base', BASE), opts['cand'], opts['evid'], tmp, skip=(EVID + os.path.basename(opts['out']),))
    elif mode == 'selftest':
        r = selftest(repo, opts.get('base', BASE), opts['cand'], opts['evid'], tmp)
    else:
        sys.stderr.write(__doc__)
        return 64
    data = (json.dumps(r, ensure_ascii=False, indent=1, sort_keys=True) + '\n').encode('utf-8')
    with open(opts['out'], 'wb') as fh:
        fh.write(data)
    sys.stdout.write(json.dumps({'Mode': mode, 'Result': r['Result']}) + '\n')
    return 0 if r['Result'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
