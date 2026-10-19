#!/usr/bin/env python3
"""Guardas mecánicas del alcance de la SEGUNDA candidata de I-62 A-7 (ronda 2; decisiones §94, puntos 2, 3 y 6).

Uso (siempre con `python -I`; solo lectura del repositorio de I-62; sin red; nunca ejecuta runtimes de IA ni CLI de modelos):
  python -I a7-guards.py preview --cand <I-62-A-7.md> --evid <dir r2> --out <json> [--repo <repo I-62>] [--base <sha>] [--tmp <dir>]
  python -I a7-guards.py run --base <sha> --head <sha> --out <json> [--repo <repo I-62>]
  python -I a7-guards.py selftest --cand <I-62-A-7.md> --evid <dir r2> --out <json> [--repo <repo I-62>] [--base <sha>] [--tmp <dir>]
  python -I a7-guards.py privacy --dir <dir>            (G7 sobre un directorio; código 0 limpio, 1 si hay hallazgos)

`preview` construye un commit hipotético sobre <base> en un repositorio desnudo temporal propio (alternates de solo lectura al almacén de
objetos del repositorio de I-62) con la candidata en docs/initiatives/I-62-A-7.md (modificación de la ronda 1) y el contenido de <evid> bajo
docs/automation/evidence/I-62-A7/r2/, y aplica las guardas de `run`. `selftest` mata, con un mutante por condición, cada condición NUEVA o
CAMBIADA en la ronda 2; las condiciones sin cambio conservan el código de la ronda 1 y su autoprueba (I-62-A7/p4-guards-selftest.json).

Guardas (ronda 2):
  G1 el commit de publicación solo MODIFICA docs/initiatives/I-62-A-7.md (que en la base es la ronda 1, blob 6c29747d…) y AÑADE archivos bajo
     docs/automation/evidence/I-62-A7/r2/; nada más (la evidencia de la ronda 1 queda intacta).
  G2 superficies protegidas sin cambio (las de la ronda 1, más la evidencia de la ronda 1 en I-62-A7/ fuera de r2/); blobs fijados y los de la
     cabecera; la ronda anterior que declara la cabecera es la que está en la base.
  G3 cada literal de A-7 §2 (L1-L17) en las líneas citadas del blob fijado de su fuente (citas «…»; filas parafraseadas por anclas), y las dos
     referencias adicionales (fin de A6-8 en A-6 L815-L816; determinación custodiada en 703f1c3f).
  G4 estructura de A7-1 en la ronda 2: frase de alcance, cláusula «sin cambiar…», ocho puntos numerados con sus rótulos (el 7, invalidador), las
     categorías (a)-(d), los seis papeles de (c), la precisión de A6-2, regla 1, las reglas 2 (c), 5 (c), 6 y 7 de la ronda 2, el texto
     corregido de la ronda 1 sin cambio, ninguna frase superada (de la ronda 1 o anterior), §3.1 (O7), §3.4 (A6-2, regla 1; invalidador por la
     regla), §3.5 (residuo con 16.7), §3.6, §5 M-04 (O1), preguntas de la ronda 2, la precisión declarada en cabecera, §1 y §6, tabla de §3.3.
     Tras fix_a7r2_b.py (blob 897d7fca): punto 1 (c) «si la entrega exige RED (16.9, `Ci` y `Tests`)» y «la invocación de medición … no tiene
     hechos del relevo (A6-7, regla 1)», punto 7 con «la implementación de la derivación del cierre del aceptante» entre las piezas, fila (c)
     de §3.3 al día, y las dos frases sustituidas como superadas. Tras fix_a7r2_c.py (blob 8a528133): L16 cita también AP 16.8 L616-617
     (filas con varias fuentes separadas por «;», un grupo de líneas por fuente), punto 1 (c) «(16.8; 16.9, `Ci` y `Tests`)», regla 5 (c) con la
     exigencia de RED indeterminada (la condición rule5c-169 se parte alrededor de la frase insertada) y la cita sin 16.8 como superada.
  G5 cabecera de la ronda 2: Amendment ronda 2, State PROPUESTA (ronda 2), Architect review (ronda 1 CHANGES REQUIRED / NOT_ACCREDITED; ronda 2
     NOT LAUNCHED), Previous round con la ronda 1, Classification MATERIAL, Owner decision no requerida, sin AGREED, §11 al día.
  G6 tabla de materialidad: M-01 no, M-02..M-05 sí, M-06..M-08 no, con motivo, y la misma partición en la cabecera.
  G7 privacidad de lo añadido: ni el nombre de usuario de Windows, ni la identidad Git del host (leídos en tiempo de ejecución, nunca
     escritos), ni rutas C:\\Users\\<nombre>, ni direcciones de correo salvo *.invalid y noreply@anthropic.com.
Los blobs de los textos se leen por su objeto Git. Lecturas Git con --no-replace-objects y GIT_OPTIONAL_LOCKS=0. Nunca escribe la ruta del
repositorio ni identidades del host.
"""
import hashlib, json, os, re, shutil, subprocess, sys

sys.dont_write_bytecode = True
BASE = 'b9334f22204c95872e7defe41bbdaaf849097585'
A7 = 'docs/initiatives/I-62-A-7.md'
EVID = 'docs/automation/evidence/I-62-A7/r2/'
R1EVID = 'docs/automation/evidence/I-62-A7/'
PREV_BLOB = '6c29747d61c5f689a195095ad3712e6d9dba62aa'
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
           'L13': ['cierre de VERIFY de la regla 7) anula esa medición'],
           'L16': ['sesión → Controller', 'Corrida `push` de `CurrentSha`', 'si la entrega exige RED, `pass` con `RedSha` no `null`',
                   # after fix_a7r2_c.py: L16 must also cite the definition of AP 16.8 (L616-617)
                   '**exige RED** si `ChainRedSha` es `null` o si `git diff --name-only BaseSha..CurrentSha` toca `ChainRedFiles`'],
           'L17': ['R1, R2, R3, R9 y R10 están en `MATCH`']}
EXTRA_REFS = [('A-6', '815-816', '…otra corrección de A-6 y otra determinación del Owner.', 'otra corrección de A-6 y otra determinación del Owner.')]
DETERMINATION = ('703f1c3f', 'docs/automation/evidence/I-62-A6/m7/s91/i6-determination.md')
SCOPE = ('**Admisión de los insumos del cierre de VERIFY de FX-02 (A-7).** Solo para el cierre efectivo de insumos (`rackcad-input-closure/v1`) de '
         'las invocaciones VERIFY del EXECUTION_CONTROLLER de la Topología A (FX-02) en F6 —la verificación, su reverificación de 16.7 y cada control '
         'negativo con invocación— y de la invocación de medición de la regla de A-6 sobre la elegibilidad, con o sin el instrumento de A-6')
NOCHANGE = 'sin cambiar el texto de §20.3.1, de B.9, de AUTOMATION_PLAN 16.24'
POINTS = ['categorías:', 'origen, identidad y revisión:', 'comprobación de blobs (I6):', 'fidelidad:', 'ausencia:', 'auditoría y derivación:',
          'invalidador:', 'límites:']
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
    ('rule2c', 'la designación nombra el papel de cada artefacto (contrato, delegación, entrega, TRX de la corrida de `CurrentSha`, TRX de la '
               'corrida de `RedSha`, hechos del relevo), con un artefacto como máximo por papel, y registra la ruta y el blob de cada uno; la delegación '
               'y la entrega van bajo `artifacts/orchestration/`, en la ruta de la tarea o del control (RAE §10), en un directorio cuyo nombre es el '
               '`RunId` de sus propios bytes;'),
    ('rule1c-roles', '(c) **artefactos custodiados:** el contrato de gate de la tarea; la delegación; la entrega; cada resultado de pruebas (TRX) de las '
                     'corridas que la verificación lee según 16.9 —el de la corrida de `CurrentSha` y, si la entrega exige RED (16.8; 16.9, `Ci` y `Tests`), el '
                     'de la corrida de `RedSha`—; y los hechos del relevo de la sesión (`RelayFacts`) que alimentan `Termination`, `Remote`, `Ci`, `Routing` '
                     'y `Denials`, cuyo SHA-256 cita la verificación;'),
    ('rule1c-measurement', 'la invocación de medición, que no produce una verificación, no tiene hechos del relevo (A6-7, regla 1);'),
    ('rule1-a62r1', 'En este ámbito, las entradas de (c) que no son artefactos versionados son, además del instrumento de A6-2, regla 1, excepciones a la '
                    'clase de §20.3.1, y se admiten solo por el blob de sus bytes custodiados;'),
    ('rule5a', '(a) una entrada de (a) que no existe en su revisión no aporta entrada; su ausencia la tratan las reglas que la exigen (16.3, 16.13, '
               'por ejemplo como MAP_INVALID, o 16.24) y nada la sustituye;'),
    ('rule5c', '(c) sin un contrato y una delegación presentes, con el blob registrado y legibles, (a) y (b) no se pueden calcular y no hay '
               'lanzamiento; la entrega, un TRX o los hechos del relevo que no existen, cuyos bytes no tienen el blob registrado o que no son legibles no '
               'aportan entrada;'),
    # rule5c-169 is split around the sentence that fix_a7r2_c.py inserts (text of both parts unchanged)
    ('rule5c-169', 'nada la sustituye (ni otra copia, ni otra corrida, ni otro papel, ni el `Target`) y la invocación sigue las reglas de 16.9 y de A-6 para '
                   'una entrada o una señal ausente, con la clasificación de fallo que fija 16.9;'),
    ('rule5c-undetermined', 'si la exigencia de RED de 16.8 no se puede determinar porque falta la entrega, el TRX de la corrida de `RedSha` ni se admite '
                            'ni se exige, y se registra la causa;'),
    ('rule5c-absence-check', 'comprobar que un artefacto designado falta, en la ruta que fija la designación, no es una lectura fuera del cierre ni aporta '
                             'contenido.'),
    ('rule5c-p22', 'Un artefacto no designado, de otra corrida, de otro papel o con un blob distinto del registrado no entra en el cierre, y su lectura es '
                   'una lectura fuera del cierre (P-22);'),
    ('rule6-relay', 'identificando el contrato, la delegación, la entrega y cada TRX solo por las rutas de la designación, y los hechos del relevo de la '
                    'solicitud son los de (c);'),
    ('rule7-invalidator', '7. **invalidador:** el «cierre de VERIFY» que la regla de A-6 sobre la elegibilidad hace invalidador es esta regla, en el blob '
                          'acordado de A-7, aplicada con el procedimiento de construcción y de validación que nombra la disposición de esa regla, '
                          'identificado por el blob o el SHA-256 de cada pieza (el constructor, el validador, el formato de la designación y la '
                          'implementación de la derivación del cierre del aceptante).'),
    ('rule7-instance', 'No es el SHA-256 de un cierre concreto: el cierre de cada invocación difiere por construcción (su `Target`, su `RunId`, las copias '
                       'de cada control) y su SHA-256 se custodia con esa invocación.'),
    ('rule7-annul', 'Anulan la medición y la elegibilidad otro blob de A-7, una A-n que cambie esta regla o un cambio de cualquier pieza del procedimiento '
                    'nombrado; no las anula el cierre concreto de una invocación construido y validado con ese procedimiento sobre sus propias entradas;'),
]
SUPERSEDED_A71 = ['pertenece a exactamente una de estas categorías', 'Una ruta figura una vez por cada blob distinto', 'deja el cierre sin calcular',
                  'el resultado de pruebas (TRX) que la invocación verifica', 'que nombra cuál es el contrato, la delegación, la entrega y el TRX',
                  'la entrega o el TRX que no existen', 'la delegación, la entrega y el TRX solo por las rutas de la designación',
                  '`artifacts/orchestration/<UnitId>/<TaskId>/<Attempt>/<RunId>/`', 'los archivos del mapa y el punto de entrada',
                  # superseded by fix_a7r2_b.py (observations 1 and 3 of the first preflight of round 2)
                  'si la delegación exige RED', 'cada pieza (el constructor, el validador y el formato de la designación)',
                  # superseded by fix_a7r2_c.py: the RED condition cited without 16.8
                  'exige RED (16.9, `Ci` y `Tests`)']
RESIDUAL = ['**Residuo declarado (lecturas de `clausemap` fuera de las revisiones de 16.13).**', 'un rechazo falso, nunca una aceptación falsa']
RESIDUAL_CHECK = ('si alguna lectura de contenido que `clausemap` hace de una cita del contrato cae en una revisión en que el blob de esa ruta no es el '
                  'de una entrada del cierre, no hay lanzamiento y el caso vuelve al Coordinator (S-03); una revisión en que la ruta no existe no aporta '
                  'lectura. Una cita cuya lectura sí está fijada por el cierre (por ejemplo, `AUTOMATION_PLAN.md` en EFF^1, categoría (a)) no bloquea.')
SUPERSEDED_35 = ['si alguna cita del contrato tiene blobs distintos en esas revisiones']
TABLE_A_ABSENCE = 'sin entrada; la tratan 16.3, 16.13 (p. ej., MAP_INVALID) o 16.24; sin sustitución'
TABLE_C_ABSENCE = ('contrato o delegación ausentes: sin cierre y sin lanzamiento; entrega, TRX o relevo ausentes: sin entrada, causa registrada, sin '
                   'sustitución, clasificación de 16.9; comprobar la ausencia no es P-22; un artefacto no designado, de otro papel o con otro blob no '
                   'entra (su lectura es P-22)')
TABLE_C_ROLES = ('TRX de la corrida de `CurrentSha` y, si la entrega exige RED, de la de `RedSha`; hechos del relevo (`RelayFacts`; la medición no '
                 'los tiene)')
OUTSIDE_R2 = [('s34-a62r1', '### 3.4', '### 3.5', '**A6-2, regla 1.**'),
              ('s34-a62r1', '### 3.4', '### 3.5', 'también son excepciones a esa clase y se admiten solo por el blob de sus bytes custodiados'),
              ('s34-a67', '### 3.4', '### 3.5', 'no el SHA-256 de una instancia'),
              ('s31-o7', '### 3.1 Motivo', '### 3.2', 'las lecturas de contenido de MV-6 y MV-7'),
              ('s35-o2', '### 3.5', '### 3.6', 'también de la reverificación de 16.7 con su `MainSha_eval` de 16.13'),
              ('s36', '### 3.6', '## 4.', '### 3.6 Correcciones de la ronda 2 (decisiones §94)'),
              ('s5-m04', '## 5. Materialidad', '## 6.', '((a) sin entrada, tratada por 16.3, 16.13 o 16.24;'),
              ('q-r2', '## 8. Preguntas', '## 9.', '| Q-A7R2-01 |'), ('q-r2', '## 8. Preguntas', '## 9.', '| Q-A7R2-02 |'),
              ('q-r2', '## 8. Preguntas', '## 9.', '| Q-A7R2-03 |'),
              ('a62r1-declared', 'Previous amendments:', 'Previous round:', 'ámbito precisa A6-2, regla 1'),
              ('a62r1-declared', '## 1. Alcance', '## 2.', 'precisa A6-2, reglas 1 y 3, sin editarlas (§3.4)'),
              ('a62r1-declared', '## 6.', '## 7.', 'Dentro de su ámbito, A7-1 precisa A6-2, reglas 1 y 3')]
SUPERSEDED_OUT = [('superseded-34', '### 3.4', '### 3.5', 'nombra ese cierre (su SHA-256)'),
                  ('superseded-31', '### 3.1 Motivo', '### 3.2', 'las lecturas de MV-3, MV-4, MV-6 y MV-7'),
                  ('superseded-5', '## 5. Materialidad', '## 6.', 'sin lanzamiento en (a) y en el objeto de (b)')]
STATE_TABLE = '| Architect independiente | ronda 1: CHANGES REQUIRED (R20261019T010200Z-4a72), NOT_ACCREDITED; ronda 2: NOT LAUNCHED'
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


def g1(diff, base_t):
    out = []
    for st, p in diff:
        if p == A7:
            if st != 'M':
                out.append('G1-amendment-modified: %s %s (the round-2 commit modifies the published amendment)' % (st, p))
        elif not (p.startswith(EVID) and st == 'A'):
            out.append('G1-only-allowed: %s %s outside "modify the amendment, add under I-62-A7/r2/"' % (st, p))
    if not any(p == A7 for st, p in diff):
        out.append('G1-amendment-modified: %s not modified' % A7)
    if base_t.get(A7) != PREV_BLOB:
        out.append('G1-amendment-modified: the base does not hold round 1 (%s) at %s' % (PREV_BLOB[:8], A7))
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
    m = re.search(r'\nPrevious round:\nronda 1 = blob ([0-9a-f]{40}) \(commit ([0-9a-f]{7,40})\)', a7text)
    if not m or m.group(1) != PREV_BLOB or base_t.get(A7) != m.group(1):
        out.append('G2-previous-round: the header does not declare round 1 as the blob the base holds at %s' % A7)
    keys = set(base_t) | set(head_t)
    for p in sorted(keys):
        prot = (p in PINNED or any(p.startswith(x) for x in PROTECTED_PREFIXES)
                or (p.startswith('docs/automation/evidence/') and not p.startswith(EVID)) or re.search(r'I-6[45]', p))
        if prot and base_t.get(p) != head_t.get(p):
            kind = 'kit' if p.startswith(PROTECTED_PREFIXES[-1]) else ('I-64/I-65' if re.search(r'I-6[45]', p) else
                                                                     ('r1-evidence' if p.startswith(R1EVID) else 'surface'))
            out.append('G2-protected(%s): %s changed' % (kind, p))
    return out


def g3(a7text, src_text, det_present):
    out = []
    sec2 = section(a7text, '## 2. Fuentes', '## 3. Delta')
    rows = [r for r in sec2.split('\n') if re.match(r'\| L\d+ \|', r)]
    ids = [re.match(r'\| (L\d+) \|', r).group(1) for r in rows]
    if ids != ['L%d' % i for i in range(1, 18)]:
        out.append('G3-rows: §2 does not list L1..L17 in order')
    current = None
    for r in rows:
        cells = [c.strip() for c in r.strip().strip('|').split('|')]
        if len(cells) < 4:
            out.append('G3-rows: malformed row %s' % cells[:1])
            continue
        lid, src, lines, lit = cells[0], cells[1], cells[2], ' | '.join(cells[3:])
        # a row may cite several sources separated by «;» (after fix_a7r2_c.py, L16: «AP 16.8 (RED de la cadena); AP 16.9»), with one
        # «;»-separated group of lines per source, in the same order; only when the lines cell has «;» (a «;» inside the description of
        # a single source, as in L8 or L17, keeps the row single-source, as before)
        groups = [g.strip() for g in lines.split(';')]
        srcs = [s.strip() for s in src.split(';')] if len(groups) > 1 else [src]
        keys = [next((k for k in SOURCES if s.startswith(k + ' ')), None) for s in srcs]
        if None in keys:
            out.append('G3-source: %s cites an unknown source' % lid)
            continue
        if len(srcs) > 1 and len(groups) != len(srcs):
            out.append('G3-lines: %s cites %d sources and %d groups of lines' % (lid, len(srcs), len(groups)))
            continue
        current = keys[0]
        for s, key in zip(srcs, keys):
            m = re.search(r'\(`([0-9a-f]{7,40})`\)', s)
            if m and not PINNED[SOURCES[key]].startswith(m.group(1)):
                out.append('G3-blob: %s cites blob %s, not the pinned %s' % (lid, m.group(1), PINNED[SOURCES[key]][:8]))
        segs, bad = [], False
        for gi, grp in enumerate(groups):
            text = src_text[SOURCES[keys[gi if len(keys) > 1 else 0]]]
            for part in grp.split(','):
                a, _, b = part.strip().partition('-')
                if not a.isdigit() or (b and not b.isdigit()):
                    bad = True
                    break
                segs.append(lines_norm(text, int(a), int(b or a)))
            if bad:
                break
        if bad:
            out.append('G3-lines: %s line reference %r' % (lid, lines))
            segs = []
        srcname = ' + '.join(sorted(set(SOURCES[k] for k in keys)))
        quotes = re.findall(r'«(.*?)»', lit)
        needles = [norm(q) for q in quotes] + [norm(x) for x in ANCHORS.get(lid, [])]
        if not needles:
            out.append('G3-literal: %s has no quote and no anchor' % lid)
        for n in needles:
            if not any(n in s for s in segs):
                out.append('G3-literal: %s: %r not on the cited lines %s of %s' % (lid, n[:60], lines, srcname))
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
    if len(pos) == 8 and pos != sorted(pos):
        out.append('G4-points: points out of order')
    if re.search(r'(?<![\d(])9\. \*\*', quote):
        out.append('G4-points: more than eight points')
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
            out.append('G4-superseded: A7-1 keeps the superseded phrase «%s»' % phrase)
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
    if TABLE_C_ROLES not in rc:
        out.append('G4-table-c-roles: the row (c) of §3.3 lacks «%s…»' % TABLE_C_ROLES[:60])
    for cond, start, end, phrase in OUTSIDE_R2:
        try:
            sec = norm(section(a7text, start, end))
        except ValueError:
            sec = ''
        if norm(phrase) not in sec:
            out.append('G4-%s: lacks «%s»' % (cond, norm(phrase)[:60]))
    for cond, start, end, phrase in SUPERSEDED_OUT:
        try:
            sec = norm(section(a7text, start, end))
        except ValueError:
            sec = ''
        if norm(phrase) in sec:
            out.append('G4-%s: keeps the round-1 phrase «%s»' % (cond, phrase))
    return out


def g5(a7text):
    out = []
    head = a7text[:a7text.index('## 1. Alcance')]
    for label, pat in (('state', r'\nState:\nPROPUESTA \(ronda 2\)\.'),
                       ('architect', r'\nArchitect review:\nronda 1: CHANGES REQUIRED, NOT_ACCREDITED \(R20261019T010200Z-4a72\)\. Ronda 2: NOT LAUNCHED\.'),
                       ('classification', r'\nClassification: MATERIAL POST-FREEZE AMENDMENT, candidata'),
                       ('owner', r'\nOwner decision:\nno requerida'), ('amendment', r'Amendment: A-7 \(candidata, ronda 2;'),
                       ('previous-round', r'\nPrevious round:\nronda 1 = blob 6c29747d61c5f689a195095ad3712e6d9dba62aa \(commit 6fee8fd6\)')):
        if not re.search(pat, head):
            out.append('G5-%s: header field differs' % label)
    if re.search(r'A-7 (?:=\s*)?AGREED|State:\s*\n\s*AGREED', a7text):
        out.append('G5-no-agreed: A-7 declared AGREED')
    if STATE_TABLE not in a7text:
        out.append('G5-state-table: §11 does not record round 1 CHANGES REQUIRED / NOT_ACCREDITED and round 2 NOT LAUNCHED')
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
    want = [(p, head_t[p]) for st, p in diff if (st == 'A' or p == A7) and p in head_t]   # round 2: the amendment is modified, G7 reads it too
    data = read_blobs(objrepo, [o for _, o in want])
    added = [(p, data[o]) for p, o in want]
    res = {}
    res['G1'] = g1(diff, base_t)
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
    """One killing mutant per condition that is NEW or CHANGED in round 2 (the unchanged conditions keep the round-1 code and its
    self-test, I-62-A7/p4-guards-selftest.json)."""
    t = a7.decode('utf-8')

    def rep(old, new, count=1):
        if old not in t:
            raise SystemExit('selftest: anchor not found for a mutant: ' + old[:60])
        return t.replace(old, new, count).encode('utf-8')

    def reps(*pairs):
        u = t
        for old, new in pairs:
            if old not in u:
                raise SystemExit('selftest: anchor not found for a mutant: ' + old[:60])
            u = u.replace(old, new, 1)
        return u.encode('utf-8')
    return [
        ('G1', 'only-allowed', 'añade un archivo fuera de r2/ (docs/automation/evidence/I-62-A7/extra.txt)', None, {'add': 'docs/automation/evidence/I-62-A7/extra.txt'}),
        ('G1', 'amendment-modified', 'el commit no modifica la enmienda (la ronda 1 queda como está)', None, {'revert_a7': True}),
        ('G2', 'r1-evidence', 'cambia un archivo de la evidencia de la ronda 1 (I-62-A7/preflight.md)', None, {'modify': 'docs/automation/evidence/I-62-A7/preflight.md'}),
        ('G2', 'previous-round', 'la cabecera declara otra ronda anterior', rep('ronda 1 = blob 6c29747d61c5f689a195095ad3712e6d9dba62aa (commit',
                                                                               'ronda 1 = blob 6c29747d61c5f689a195095ad3712e6d9dba62ab (commit'), {}),
        ('G3', 'rows', 'falta la fila L17', rep('| L17 |', '| L18 |'), {}),
        ('G3', 'L16', 'L16 con otro conjunto de líneas de 16.9 (sin la 638)', rep('; 630, 635, 638, 639, 641, 643 |', '; 630, 635, 637, 639, 641, 643 |'), {}),
        ('G3', 'L16-168-lines', 'L16 sin las líneas 616-617 de 16.8 (la cita de «exige RED» queda fuera de las líneas citadas)',
         rep('| AP 16.8 (RED de la cadena); AP 16.9 | 616-617; 630,', '| AP 16.8 (RED de la cadena); AP 16.9 | 618-619; 630,'), {}),
        ('G3', 'L16-168-quote', 'L16 con la cita de 16.8 alterada («o si» → «y si»)',
         rep('«**exige RED** si `ChainRedSha` es `null` o si `git diff', '«**exige RED** si `ChainRedSha` es `null` y si `git diff'), {}),
        ('G3', 'L16-168-required', 'L16 vuelve a la fila anterior a fix_a7r2_c.py (solo AP 16.9, sin la definición de 16.8)',
         reps(('| AP 16.8 (RED de la cadena); AP 16.9 | 616-617; 630,', '| AP 16.9 | 630,'),
              ('; una entrega «**exige RED** si `ChainRedSha` es `null` o si `git diff --name-only BaseSha..CurrentSha` toca `ChainRedFiles`» (16.8) |', ' |')),
         {}),
        ('G3', 'L17', 'L17 con una palabra cambiada en la cita', rep('«`RelayFacts` = los hechos del relevo que cita la verificación',
                                                                     '«`RelayFacts` = los hechos del relevo que firma la verificación'), {}),
        ('G4', 'points', 'el punto 7 (invalidador) deja de estar numerado', rep('> 7. **invalidador:**', '> **invalidador:**'), {}),
        ('G4', 'rule1c-roles', '(c): la condición del TRX de RedSha sin la referencia a 16.8 (vuelve a «(16.9, `Ci` y `Tests`)»)',
         rep('si la entrega exige RED (16.8; 16.9, `Ci` y `Tests`), el de la corrida de', 'si la entrega exige RED (16.9, `Ci` y `Tests`), el de la corrida de'), {}),
        ('G4', 'rule1c-measurement', '(c) sin «la invocación de medición … no tiene hechos del relevo»',
         rep('la invocación de medición, que no produce una verificación, no tiene hechos del relevo (A6-7, regla 1);',
             'la invocación de medición también tiene hechos del relevo;'), {}),
        ('G4', 'rule1-a62r1', 'sin la precisión de A6-2, regla 1, en el punto 1', rep('son, además del instrumento de A6-2, regla 1, excepciones a la clase de',
                                                                                      'son excepciones a la clase de'), {}),
        ('G4', 'rule2c', 'regla 2 (c) sin «un artefacto como máximo por papel»', rep('con un artefacto como máximo por papel,', 'con varios artefactos por papel,'), {}),
        ('G4', 'rule5c', 'regla 5 (c) sin los hechos del relevo', rep('la entrega, un TRX o los hechos del', 'la entrega, un TRX o los datos del'), {}),
        ('G4', 'rule5c-169', 'regla 5 (c) sin la clasificación de 16.9', rep('con la clasificación de fallo que fija 16.9;', 'con la clasificación que se decida;'), {}),
        ('G4', 'rule5c-undetermined', 'regla 5 (c): con la exigencia de RED indeterminada, el TRX de RedSha se admite',
         rep('el TRX de la corrida de `RedSha` ni se admite ni se exige, y se registra la causa;', 'el TRX de la corrida de `RedSha` se admite;'), {}),
        ('G4', 'rule5c-absence-check', 'regla 5 (c): comprobar la ausencia vuelve a ser una lectura fuera del cierre',
         rep('fija la designación, no es una lectura fuera del cierre ni aporta contenido.', 'fija la designación, es una lectura fuera del cierre.'), {}),
        ('G4', 'rule5c-p22', 'regla 5 (c): la lectura de lo no designado deja de ser P-22', rep('y su lectura es una lectura fuera del cierre (P-22);', 'y su lectura se tolera;'), {}),
        ('G4', 'rule6-relay', 'regla 6 sin los hechos del relevo de la solicitud', rep('y los hechos del relevo de la solicitud son los de (c);', ';'), {}),
        ('G4', 'rule7-invalidator', 'regla 7 sin la implementación de la derivación del cierre del aceptante entre las piezas',
         rep(', el formato de la designación y la implementación de la derivación del cierre del aceptante)', ' y el formato de la designación)'), {}),
        ('G4', 'rule7-instance', 'regla 7: el invalidador vuelve a ser el SHA-256 de un cierre', rep('cierre del aceptante). No es el SHA-256', 'cierre del aceptante). Es el SHA-256'), {}),
        ('G4', 'rule7-annul', 'regla 7 sin lo que no anula la medición', rep('no las anula el cierre concreto de una', 'tampoco las conserva el cierre concreto de una'), {}),
        ('G4', 'superseded', 'reinsertada la frase de la ronda 1 «el resultado de pruebas (TRX) que la invocación verifica»',
         rep('> 8. **límites:** esta regla', '> 8. **límites:** el resultado de pruebas (TRX) que la invocación verifica; esta regla'), {}),
        ('G4', 'superseded-r2b', 'reinsertada la frase superada por fix_a7r2_b.py «si la delegación exige RED»',
         rep('> 8. **límites:** esta regla', '> 8. **límites:** si la delegación exige RED; esta regla'), {}),
        ('G4', 'superseded-r2c', 'reinsertada la cita superada por fix_a7r2_c.py «exige RED (16.9, `Ci` y `Tests`)» (sin 16.8)',
         rep('> 8. **límites:** esta regla', '> 8. **límites:** si la entrega exige RED (16.9, `Ci` y `Tests`); esta regla'), {}),
        ('G4', 's34-a62r1', '§3.4 sin la viñeta de A6-2, regla 1', rep('- **A6-2, regla 1.** Dice que', '- **A6-2.** Dice que'), {}),
        ('G4', 's34-a67', '§3.4 sin «no el SHA-256 de una instancia»', rep('no el SHA-256 de una instancia', 'y el SHA-256 de su instancia'), {}),
        ('G4', 'superseded-34', '§3.4 reinsertado «nombra ese cierre (su SHA-256)»', rep('- **R7-A.** Referencia técnica', '- La disposición nombra ese cierre (su SHA-256).\n- **R7-A.** Referencia técnica'), {}),
        ('G4', 's31-o7', '§3.1 sin «las lecturas de contenido de MV-6 y MV-7»', rep('las lecturas de contenido de MV-6 y MV-7', 'las lecturas de MV-6 y MV-7'), {}),
        ('G4', 'superseded-31', '§3.1 vuelve a «las lecturas de MV-3, MV-4, MV-6 y MV-7»', rep('las lecturas de contenido de MV-6 y MV-7', 'las lecturas de MV-3, MV-4, MV-6 y MV-7 y las lecturas de contenido de MV-6 y MV-7'), {}),
        ('G4', 's35-o2', '§3.5 sin la reverificación de 16.7', rep('también de la reverificación\nde 16.7 con su `MainSha_eval` de 16.13,', 'también de la verificación,'), {}),
        ('G4', 's36', 'sin §3.6', rep('### 3.6 Correcciones de la ronda 2 (decisiones §94)', '### 3.6 Correcciones'), {}),
        ('G4', 's5-m04', 'motivo de M-04 sin la regla 5 vigente para (a)', rep('((a) sin entrada, tratada por 16.3, 16.13 o 16.24;', '((a) con efecto definido;'), {}),
        ('G4', 'superseded-5', 'motivo de M-04 vuelve a «sin lanzamiento en (a) y en el objeto de (b)»',
         rep('| M-04 | **sí** | cambia qué falla y cómo:', '| M-04 | **sí** | cambia qué falla y cómo: sin lanzamiento en (a) y en el objeto de (b);'), {}),
        ('G4', 'q-r2', 'sin la pregunta Q-A7R2-03', rep('| Q-A7R2-03 |', '| Q-A7R2-04 |'), {}),
        ('G4', 'a62r1-declared', '§6 sin la precisión de A6-2, reglas 1 y 3', rep('Dentro de su ámbito, A7-1 precisa A6-2, reglas 1 y 3;', 'Dentro de su ámbito, A7-1 precisa A6-2, regla 3;'), {}),
        ('G4', 'table-absence', 'la columna de ausencia de (c) en §3.3 sin la clasificación de 16.9', rep('sin sustitución, clasificación de 16.9; comprobar la ausencia', 'sin sustitución; comprobar la ausencia'), {}),
        ('G4', 'table-c-roles', 'la fila (c) de §3.3 vuelve a «con RED» y sin «la medición no los tiene»',
         rep('y, si la entrega exige RED, de la de `RedSha`; hechos del relevo (`RelayFacts`; la medición no los tiene)',
             'y, con RED, de la de `RedSha`; hechos del relevo (`RelayFacts`)'), {}),
        ('G5', 'amendment', 'Amendment ronda 1', rep('Amendment: A-7 (candidata, ronda 2;', 'Amendment: A-7 (candidata, ronda 1;'), {}),
        ('G5', 'state', 'State PROPUESTA (ronda 1)', rep('PROPUESTA (ronda 2). No aplicada', 'PROPUESTA (ronda 1). No aplicada'), {}),
        ('G5', 'architect', 'Architect review sin la ronda 1', rep('ronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261019T010200Z-4a72). Ronda 2: NOT LAUNCHED.',
                                                                    'Ronda 2: NOT LAUNCHED.'), {}),
        ('G5', 'previous-round', 'sin el bloque Previous round', rep('\nPrevious round:\nronda 1 = blob', '\nPrevious:\nronda 1 = blob'), {}),
        ('G7', 'amendment-scanned', 'una dirección de correo en la enmienda modificada (en la ronda 2 la enmienda no es un archivo añadido)', rep('## 9. Separabilidad', '## 9. Separabilidad ' + 'persona' + chr(64) + 'example.com'), {}),
        ('G5', 'state-table', '§11: Architect ronda 2 LAUNCHED', rep('NOT_ACCREDITED; ronda 2: NOT LAUNCHED', 'NOT_ACCREDITED; ronda 2: LAUNCHED'), {}),
    ]


EXPECT = dict(((g, c), 'G%s-%s' % (g[1:], c)) for g, c in [
    ('G1', 'only-allowed'), ('G1', 'amendment-modified'), ('G2', 'previous-round'), ('G3', 'rows'), ('G4', 'points'), ('G4', 'rule1c-roles'),
    ('G4', 'rule1-a62r1'), ('G4', 'rule2c'), ('G4', 'rule5c'), ('G4', 'rule5c-169'), ('G4', 'rule5c-p22'), ('G4', 'rule6-relay'),
    ('G4', 'rule7-invalidator'), ('G4', 'rule7-instance'), ('G4', 'rule7-annul'), ('G4', 'superseded'), ('G4', 's34-a62r1'), ('G4', 's34-a67'),
    ('G4', 'superseded-34'), ('G4', 's31-o7'), ('G4', 'superseded-31'), ('G4', 's35-o2'), ('G4', 's36'), ('G4', 's5-m04'), ('G4', 'superseded-5'),
    ('G4', 'q-r2'), ('G4', 'a62r1-declared'), ('G4', 'table-absence'), ('G5', 'amendment'), ('G5', 'state'), ('G5', 'architect'),
    ('G5', 'previous-round'), ('G5', 'state-table')])
EXPECT.update({('G2', 'r1-evidence'): 'G2-protected(r1-evidence)', ('G3', 'L16'): 'G3-literal: L16:', ('G3', 'L17'): 'G3-literal: L17:',
               ('G4', 'rule2c'): 'G4-rule2c:', ('G4', 'rule5c'): 'G4-rule5c:', ('G4', 'rule7-invalidator'): 'G4-rule7-invalidator:',
               ('G7', 'amendment-scanned'): 'e-mail address',
               # new or changed after fix_a7r2_b.py (blob 897d7fca): the three phrases of observations 1-3
               ('G4', 'rule1c-roles'): 'G4-rule1c-roles:', ('G4', 'rule1c-measurement'): 'G4-rule1c-measurement:',
               ('G4', 'table-c-roles'): 'G4-table-c-roles:',
               ('G4', 'superseded-r2b'): 'G4-superseded: A7-1 keeps the superseded phrase «si la delegación exige RED»',
               # new or changed after fix_a7r2_c.py (blob 8a528133): L16 with AP 16.8 L616-617, (16.8; 16.9 ...), rule 5 (c) undetermined RED
               ('G3', 'L16-168-lines'): 'G3-literal: L16:', ('G3', 'L16-168-quote'): 'G3-literal: L16:', ('G3', 'L16-168-required'): 'G3-literal: L16:',
               ('G4', 'rule5c-undetermined'): 'G4-rule5c-undetermined:', ('G4', 'rule5c-absence-check'): 'G4-rule5c-absence-check:',
               ('G4', 'rule5c-169'): 'G4-rule5c-169:',
               ('G4', 'superseded-r2c'): 'G4-superseded: A7-1 keeps the superseded phrase «exige RED (16.9, `Ci` y `Tests`)»'})
CHANGED_R2B = ['rule1c-roles', 'rule1c-measurement', 'rule7-invalidator', 'table-c-roles', 'superseded-r2b']
CHANGED_R2C = ['L16', 'L16-168-lines', 'L16-168-quote', 'L16-168-required', 'rule1c-roles', 'rule5c-169', 'rule5c-undetermined', 'rule5c-absence-check', 'superseded-r2c']
UNCHANGED = ['G3 literal/lines/blob/anchor/extra para L1-L15', 'G4 scope, nochange, quote, categories, location, table, rule1, rule1a, rule2a, '
             'rule2-repeat, rule5a, residual, residual-coverage, superseded-35, q06', 'G5 classification, owner, no-agreed', 'G6 values, motive, header',
             'G7 host-identity, users-path, e-mail', 'G2 pinned, header-claims, protected (surface, kit, I-64/I-65)']


def selftest(repo, base, cand, evid, tmp):
    r0, sc, files = preview(repo, base, cand, evid, tmp)
    rows = [{'Guard': 'ALL', 'Condition': 'candidate', 'Mutant': 'la candidata sin mutar', 'Killed': None, 'Result': r0['Result']}]
    idents = host_identities(repo)
    head_t = tree(sc.dir, base)
    for guard, cond, what, text, op in mutants(files[A7], idents):
        f = dict(files)
        if text is not None:
            f[A7] = text
        if op.get('revert_a7'):
            f[A7] = gout(sc.dir, 'cat-file', 'blob', head_t[A7])
        if op.get('add'):
            f[op['add']] = b'mutante\n'
        if op.get('modify'):
            f[op['modify']] = gout(sc.dir, 'cat-file', 'blob', head_t[op['modify']]) + b'\nmutante\n'
        head = sc.commit(base, f)
        r = evaluate(repo, base, head, sc.dir)
        fnd = r['Guards'][guard]['Findings']
        want = EXPECT[(guard, cond)]
        killed = any(want in x for x in fnd)
        rows.append({'Guard': guard, 'Condition': cond, 'Mutant': what, 'Killed': killed, 'GuardStatus': r['Guards'][guard]['Status'],
                     'ExpectedFinding': want.split(':')[0], 'Findings': len(fnd)})
    ok = r0['Result'] == 'PASS' and all(x['Killed'] for x in rows[1:])
    return {'Mode': 'selftest', 'Base': base, 'Candidate': r0, 'Mutants': rows, 'MutantsTotal': len(rows) - 1, 'UnchangedConditions': UNCHANGED,
            'ChangedAfterFixA7r2b': CHANGED_R2B, 'ChangedAfterFixA7r2c': CHANGED_R2C,
            'UnchangedReusedFrom': 'docs/automation/evidence/I-62-A7/p4-guards-selftest.json (SHA-256 7a670e4945fece2c6a54d24d7eadc052d1c721c5693aabac8b91c49ecd9c6732)',
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
