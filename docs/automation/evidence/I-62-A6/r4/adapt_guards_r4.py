# Derives a6r4-guards.py (A-6 round 4, decisiones §85) from the round-3 guards (r3/a6r3-guards.py, blob 103d69c6) by exact substitutions.
# Each substitution must match exactly once (or the declared count). Output next to this script; diff in a6r4-guards.diff.
# Read-only on the I-62 worktree (git show of its HEAD); writes only a6r4-guards.py and a6r4-guards.diff next to this script.
import difflib, hashlib, os, subprocess
D = os.path.dirname(os.path.abspath(__file__))
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
SRC_BLOB = '103d69c62025b852a6b2233b1a55d78ba30f083d'
ENV = dict(os.environ, GIT_NO_REPLACE_OBJECTS='1', GIT_OPTIONAL_LOCKS='0')
src = subprocess.run(['git', '-C', WT, 'show', 'HEAD:docs/automation/evidence/I-62-A6/r3/a6r3-guards.py'], capture_output=True, env=ENV,
                     check=True).stdout
src = src.replace(b'\r\n', b'\n')
assert hashlib.sha1(b'blob %d\x00' % len(src) + src).hexdigest() == SRC_BLOB
t = src.decode('utf-8')
orig = t
n_done = []


def rep(old, new, count=1):
    global t
    c = t.count(old)
    assert c == count, (old[:80], c)
    t = t.replace(old, new)
    n_done.append(old[:40])


# ------------------------------------------------------------------ docstring
i = t.index('"""', t.index('"""') + 3)
t = '''#!/usr/bin/env python3
"""Guardas mecánicas de la candidata de la ronda 4 de la enmienda I-62 A-6 (decisiones §85).

Adaptadas de docs/automation/evidence/I-62-A6/r3/a6r3-guards.py (ronda 3, blob 103d69c6…), que no se modifica, con adapt_guards_r4.py
(sustituciones exactas; diff en a6r4-guards.diff). Autoridad: decisiones §85, punto 5 («Autoprueba y mutantes del texto corregido; negativos sobre
N4 y nc1; compatibilidad con los otros deltas de A-6; verificación de los blobs fijados del instrumento y del aceptante; integridad de las
autorizaciones del Owner; guardas sobre el commit exacto»).

Uso (siempre con `python -I -B`; solo lectura del repositorio; sin red; nunca ejecuta runtimes de IA ni CLI de modelos):
  python -I -B a6r4-guards.py self-test --repo <repo> --a6 <I-62-A-6.md> --out <json> [--base <sha>] [--tmp <dir>]
  python -I -B a6r4-guards.py preview --repo <repo> --a6 <I-62-A-6.md> --out <json> [--base <sha>] [--pkgdir <dir>] [--r4dir <dir>]
                                      [--selftest <json>] [--cand-repo <clon temporal> --cand-head <sha>] [--final] [--tmp <dir>]
  python -I -B a6r4-guards.py run --repo <repo> --base <sha> --head <sha> --out <json> [--tmp <dir>]

Guardas (las de la ronda 3, adaptadas a la ronda 4):
  G1  cabecera de la ronda 4; bloques literales de §2; citas «…»; prefijos, commits abreviados, identificadores de 40 hexadecimales y SHA-256
      conocidos o del paquete r4/; referencias «A6-n, regla k» existentes y, en el texto conservado de la ronda 3, a la misma regla; privacidad.
  G2  rutas del commit: solo docs/initiatives/I-62-A-6.md modificado (de la ronda 3, blob e62f27b8…, a la ronda 4) y archivos añadidos bajo
      docs/automation/evidence/I-62-A6/r4/; privacidad de lo añadido.
  G3  superficies prohibidas (más las revisiones de las rondas 1 a 3 y todo I-62-A6/ fuera de r4/), objetos congelados (entre ellos los árboles
      r2/ y r3/ y la revisión de la ronda 3), C-20b y rutas del commit.
  G4  dictamen, M-01..M-08 (columnas Dictamen, Ronda 1 y Ronda 2), revisión de la ronda 1 (§3.16), de la ronda 2 (§3.17, que la ronda 4 solo
      amplía por añadido en la celda «Prueba» de R1 (b)-(d)) y de la ronda 3 (R1 y O1-O7 en §3.18), ubicaciones coherentes con las líneas
      «Cubre» (la de A6-3 nombra R1 de la ronda 3), evidencia existente y compatibilidad con A-1..A-5.
  G5  pureza del delta (nueve párrafos en las mismas anclas); A6-1, A6-2 y A6-4 a A6-9 idénticos byte a byte a la ronda 3 (párrafo y sección);
      en A6-3, todo salvo la regla 6 (b) y el segmento de la ronda 3 de su línea «Cubre», byte a byte; la regla 6 (b) distinta de la de la ronda 3,
      con `RelayFacts` antes de `Closure`, los apartados (i)-(v) y la frase final, y cada frase exigida una sola vez y en su apartado; las secciones
      de §3 que la ronda 4 no cambia, más §6 (cabecera) y §9, idénticas; frases exigidas y prohibidas; cifras de topes; prohibiciones de §3.13.
  G6  líneas del Owner idénticas a las de la ronda anterior; (a) en decisiones §78.
  G7  identidad por blob del instrumento, del aceptante y de la especificación.
  G8  estado: PROPUESTA (ronda 4), ronda 4 NOT LAUNCHED, sin veredicto del Coordinator sobre la ronda 4, sin AGREED, FX-02 PASS, C-24 PASS ni
      F6 GATE PASS, ninguna línea del Owner pedida; en `run` (o --final), ningún marcador pendiente.
  G9  alcance de R5 (solo VERIFY, PLAN excluida).
  G10 resultado del aceptante: la tabla de A6-5, regla 2 (con la condición de R4 sobre `Verification` de A6-3, regla 6 (c)), frente a los bytes
      del aceptante fijado; entradas de la solicitud en A6-3, reglas 5-6 (`Closure` por su definición); correspondencias fijas de A6-5 con AP, RAE
      y V14.
  G11 fuentes y rangos de líneas para el revisor siguiente, incluidas las fuentes añadidas en la ronda 4 (§2).
  G5b autoprueba custodiada: Result PASS, ToolBlob = blob de a6r4-guards.py, A6TextBlob = blob de la A-6, todos los mutantes muertos.
Los blobs de los textos se calculan con CRLF -> LF. Todas las lecturas Git con GIT_NO_REPLACE_OBJECTS=1 y GIT_OPTIONAL_LOCKS=0. Sin escrituras
salvo --out y un directorio temporal propio. Nunca escribe la ruta del repositorio ni identidades del host.
''' + t[i:]

# ------------------------------------------------------------------ identities
rep('R2_BLOB = "86da0ae770e683a647f2724860a4bd890061154d"     # A-6 ronda 2: texto anterior de la ronda 3\nPREV_BLOB = R2_BLOB\n',
    'R2_BLOB = "86da0ae770e683a647f2724860a4bd890061154d"     # A-6 ronda 2\n'
    'R3_COMMIT = "a128efef8f83890f8deae66c286c553bed2663e3"   # commit que publicó la ronda 3\n'
    'R3_BLOB = "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1"     # A-6 ronda 3: texto anterior de la ronda 4\n'
    'PREV_BLOB = R3_BLOB\n')
rep('EVID_R3 = EVID + "r3/"\nGUARDS, SELFTEST = EVID_R3 + "a6r3-guards.py", EVID_R3 + "a6r3-selftest.json"\n',
    'EVID_R3 = EVID + "r3/"\nEVID_R4 = EVID + "r4/"\nGUARDS, SELFTEST = EVID_R4 + "a6r4-guards.py", EVID_R4 + "a6r4-selftest.json"\n')
rep('EVID_R12_TREE = "1c7f8fbb25ebe173dad12f56157a6e6aeaa84a3c"  # árbol de I-62-A6/ en la base de la ronda 3 (rondas 1 y 2), sin r3\n',
    'EVID_R12_TREE = "1c7f8fbb25ebe173dad12f56157a6e6aeaa84a3c"  # árbol de I-62-A6/ en la base de la ronda 3 (rondas 1 y 2), sin r3\n'
    'R3_REVIEW_DIR = "docs/automation/evidence/I-62-architect-A-6r3"\n'
    'R3_REVIEW = R3_REVIEW_DIR + "/R20261013T183701Z-9f80/output.json"\n'
    'R3_REVIEW_TREE = "f4442fdf2cdeb9a4e3abbe041443f32f77139745"\n'
    'R3_REVIEW_BLOB = "691bbc8fabdefaa853c1d6e29465b6c1a1889107"\n'
    'EVID_R3_TREE = "a4e93edba524476d28e1d7d50df6e9ef1fccb5ab"   # árbol de I-62-A6/r3/ (ronda 3)\n'
    'EVID_R123_TREE = "b03ca6bd2b14ea69bdee733263eb2c2b6b8ebe41"  # árbol de I-62-A6/ en la base de la ronda 4 (rondas 1 a 3), sin r4\n')
rep('    R2_REVIEW_DIR: R2_REVIEW_TREE, R2_REVIEW: R2_REVIEW_BLOB, EVID_R2: EVID_R2_TREE,\n}',
    '    R2_REVIEW_DIR: R2_REVIEW_TREE, R2_REVIEW: R2_REVIEW_BLOB, EVID_R2: EVID_R2_TREE,\n'
    '    R3_REVIEW_DIR: R3_REVIEW_TREE, R3_REVIEW: R3_REVIEW_BLOB, EVID_R3: EVID_R3_TREE,\n}')
rep('FORBIDDEN_SURFACES = R1_SURFACES + [R1_REVIEW_DIR + "/", R2_REVIEW_DIR + "/"]\nEVID_OUTSIDE_R2_PHRASE = "todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r3/`"',
    'FORBIDDEN_SURFACES = R1_SURFACES + [R1_REVIEW_DIR + "/", R2_REVIEW_DIR + "/", R3_REVIEW_DIR + "/"]\nEVID_OUTSIDE_R2_PHRASE = "todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r4/`"')

# ------------------------------------------------------------------ header of round 4
rep('# ---------------------------------------------------------------- cabecera de la ronda 2\n',
    '# ---------------------------------------------------------------- cabecera de la ronda 4\n')
rep('''    "Amendment: A-6 (candidata, ronda 3;",''', '''    "Amendment: A-6 (candidata, ronda 4;",''')
rep('''    "ronda 2 = blob %s (commit %s). Revisión formal R20261012T195511Z-e5c3" % (R2_BLOB, R2_COMMIT[:8]),''',
    '''    "ronda 2 = blob %s (commit %s). Revisión formal R20261012T195511Z-e5c3" % (R2_BLOB, R2_COMMIT[:8]),
    "ronda 3 = blob %s (commit %s). Revisión formal R20261013T183701Z-9f80" % (R3_BLOB, R3_COMMIT[:8]),''')
rep('''    "(R20261012T195511Z-e5c3). Ronda 3:\\nNOT LAUNCHED.",''',
    '''    "(R20261012T195511Z-e5c3). Ronda 3:\\nCHANGES REQUIRED, ACCREDITED (R20261013T183701Z-9f80). Ronda 4: NOT LAUNCHED.",''')
rep('''    "State:\\nPROPUESTA (ronda 3). No aplicada",
]''', '''    "State:\\nPROPUESTA (ronda 4). No aplicada",
]''')
rep('''    "que las rondas 2 y 3 no amplían",''', '''    "que las rondas 2 a 4 no amplían",''')
rep('''    "en la ronda 3, solo en A6-3, regla 6, A6-5 y A6-7 (§3.17)",''',
    '''    "en la ronda 3 solo en A6-3, regla 6, A6-5 y A6-7 (§3.17)",
    "y, en la ronda 4, solo en A6-3, regla 6 (b) (§3.18)",
    "auditor ACCREDITED; Architect CHANGES REQUIRED (A62-A6R3-R1; A62-A6R3-O1..O7)",
    "Decisiones §85, punto 1: A62-A6R3-R1 ACCEPTED REQUIRED y A62-A6R2-R2 CORRECTED",
    "Las rondas 2 a 4 conservan los blobs del instrumento y del aceptante que fija (a)",''')
rep('''r"Owner decision:\\n\\s*(?:NOT )?REQUIRED", r"(?i)ronda 3:\\s*\\**(?:AGREED|CHANGES REQUIRED|LAUNCHED)",
                    r"(?i)ronda 2:\\s*\\**AGREED"]''',
    '''r"Owner decision:\\n\\s*(?:NOT )?REQUIRED", r"(?i)ronda 4:\\s*\\**(?:AGREED|CHANGES REQUIRED|LAUNCHED)",
                    r"(?i)ronda 3:\\s*\\**(?:AGREED|NOT LAUNCHED)", r"(?i)ronda 2:\\s*\\**AGREED"]''')

# ------------------------------------------------------------------ short prefixes: round 3 by commit; round-3 review and r3/ trees
rep('''               "411949b0": EVID_R2}
SHORT_AT_REV = {"eac473c0": (R1_COMMIT, A6), "86da0ae7": (R2_COMMIT, A6)}   # prefijos de las rondas anteriores, en su commit
SHORT_EVID_R1 = "7f504a17"                                                   # árbol de I-62-A6/ de la ronda 1: recalculado sin r2/ ni r3/''',
    '''               "411949b0": EVID_R2, "691bbc8f": R3_REVIEW, "f4442fdf": R3_REVIEW_DIR, "a4e93edb": EVID_R3}
SHORT_AT_REV = {"eac473c0": (R1_COMMIT, A6), "86da0ae7": (R2_COMMIT, A6), "e62f27b8": (R3_COMMIT, A6)}   # rondas anteriores, en su commit
SHORT_EVID_R1 = "7f504a17"                                                   # árbol de I-62-A6/ de la ronda 1: recalculado sin r2/, r3/ ni r4/''')

# ------------------------------------------------------------------ round-3 review findings (§3.18)
rep('R2_ROW_RX = re.compile(r"^((?:R[12] \\([a-d]\\))|O[1-5]):")\n',
    'R2_ROW_RX = re.compile(r"^((?:R[12] \\([a-d]\\))|O[1-5]):")\n'
    'R3_ROWS = ["R1", "O1", "O2", "O3", "O4", "O5", "O6", "O7"]   # filas de §3.18 (hallazgos de la ronda 3)\n'
    'R3_FINDINGS = list(R3_ROWS)\n'
    'R3_ROW_RX = re.compile(r"^(R1|O[1-7]):")\n'
    'R3_317_APPENDED = ["R1 (b)", "R1 (c)", "R1 (d)"]   # filas de §3.17 cuya celda «Prueba» la ronda 4 solo amplía por añadido\n')

# ------------------------------------------------------------------ deltas: identical to round 3; rule 6 (b) of A6-3; unchanged sections
rep('IDENTICAL_TO_R1 = ["A6-1", "A6-2", "A6-4", "A6-6", "A6-8", "A6-9"]      # párrafo idéntico byte a byte al de la ronda 2\n'
    'SECTION_IDENTICAL = ["A6-1", "A6-2", "A6-4", "A6-6", "A6-8"]          # sección entera idéntica (la de A6-9 lleva el registro G1-G6)\n'
    'A63_SAME_RULES = [0, 1, 2, 3, 4, 5, 7]                                  # en A6-3 solo cambia la regla 6\n',
    'IDENTICAL_TO_R1 = ["A6-1", "A6-2", "A6-4", "A6-5", "A6-6", "A6-7", "A6-8", "A6-9"]   # párrafo idéntico byte a byte al de la ronda 3\n'
    'SECTION_IDENTICAL = list(IDENTICAL_TO_R1)                                            # y sección entera idéntica\n'
    'A63_SAME_RULES = [0, 1, 2, 3, 4, 5, 7]                                  # en A6-3 solo cambia la regla 6, y en ella solo el apartado (b)\n'
    'SAME_SECTIONS = ["## 3. ", "### 3.2 ", "### 3.3 ", "### 3.13 ", "### 3.14 ", "### 3.15 ", "### 3.16 ",   # §3 sin cambio en la ronda 4\n'
    '                 "## 6. ", "## 9. "]                                                                # cabecera de §6 y §9\n'
    'R6_MARK = re.compile(r"^>\\s+(?:\\(([a-d])\\)|\\(([ivx]+)\\)|(La prueba previa )|(El aceptante vuelve a ejecutar ))")\n'
    'R6_ORDER = ["6", "a", "b", "i", "ii", "iii", "iv", "v", "cierre", "c", "d", "final"]   # regla 6 de A6-3 en la ronda 4\n'
    'R6_ORDER_PREV = ["6", "a", "b", "c", "d", "final"]                                       # y en la ronda 3\n'
    'R6B_KEYS = ["b", "i", "ii", "iii", "iv", "v", "cierre"]                                  # el apartado (b) completo\n'
    'R6B_REQUIRED = [  # (parte del apartado (b), frase exigida una sola vez en (b))\n'
    '    ("b", "con su tabla aplicada así cuando una de sus entradas no da un valor"), ("i", "faltan si E no fija su blob"),\n'
    '    ("i", "un único objeto JSON en UTF-8 sin claves repetidas"), ("i", "Se omite entonces todo término de la tabla que lee la entrada que falta"),\n'
    '    ("ii", "no está, es `null` o está vacío no aporta ningún término"), ("ii", "40 caracteres hexadecimales en minúscula"),\n'
    '    ("ii", "`git --no-replace-objects cat-file -t`"), ("iii", "da el conjunto vacío"), ("iv", "no se sustituye por ningún otro valor"),\n'
    '    ("v", "registra cada término omitido con su causa"), ("v", "se custodia con la solicitud y el cierre"),\n'
    '    ("cierre", "derivan su cierre con esta misma regla"),\n'
    '    ("ii", "si no (en nc1, H.CurrentSha), se omite"),   # añadida por las guardas: el valor de commit inválido se omite\n'
    ']\n')

# ------------------------------------------------------------------ claims of round 4
rep('''    (r"(?i)ronda 3\\s*[:=]\\s*\\**(?!NOT\\b)(?:AGREED|LAUNCHED|CHANGES REQUIRED|PASS)", "veredicto o lanzamiento de la ronda 3 declarado"),
    (r"(?i)(?<!ninguna )revisión (?:formal )?de la ronda 3 (?:lanzada|ejecutada|AGREED)", "revisión de la ronda 3 declarada lanzada"),
    (r"(?i)ronda 2\\s*[:=]\\s*\\**(?:AGREED|PASS)", "ronda 2 declarada AGREED"),''',
    '''    (r"(?i)ronda 4\\s*[:=]\\s*\\**(?!NOT\\b)(?:AGREED|LAUNCHED|CHANGES REQUIRED|PASS)", "veredicto o lanzamiento de la ronda 4 declarado"),
    (r"(?i)(?<!ninguna )revisión (?:formal )?de la ronda 4 (?:lanzada|ejecutada|AGREED)", "revisión de la ronda 4 declarada lanzada"),
    (r"(?i)ronda 3\\s*[:=]\\s*\\**(?:AGREED|PASS)", "ronda 3 declarada AGREED"),
    (r"(?i)ronda 2\\s*[:=]\\s*\\**(?:AGREED|PASS)", "ronda 2 declarada AGREED"),''')

# ------------------------------------------------------------------ G10: `Closure` by its definition («el cierre» is now frequent in rule 6 (b))
rep('''"Closure": ["el cierre"]''', '''"Closure": ["`Closure` = el cierre"]''')

# ------------------------------------------------------------------ G11: sources added in round 4
rep('G11_REQUIRED = [("V14", 2333, 2334)]\n',
    'G11_REQUIRED = [("V14", 2333, 2334)]\n'
    'G11_REQUIRED_REFS = [AE + "/README.md:219-220", KITCC + ":23"]   # fuentes añadidas en la ronda 4 (§2), además de la revisión de la ronda 3\n')
rep('BASENAME_DIRS = [BMIN + "/", "docs/automation/evidence/I-62-F6/kits/FX-02/", R1_REVIEW_DIR + "/", R2_REVIEW_DIR + "/", RULING_DIR + "/", EVID]',
    'BASENAME_DIRS = [BMIN + "/", "docs/automation/evidence/I-62-F6/kits/FX-02/", R1_REVIEW_DIR + "/", R2_REVIEW_DIR + "/", R3_REVIEW_DIR + "/",\n'
    '                 RULING_DIR + "/", EVID]')

# ------------------------------------------------------------------ privacy: synthetic commit identities of the round-4 bench (r1probes/indep/make_synth.py, run_r4.py)
rep('PRIVACY_OK = {"fixture@example.invalid", "neg@example.invalid"}',
    'PRIVACY_OK = {"fixture@example.invalid", "neg@example.invalid", "synth@example.invalid", "banco-r4@example.invalid"}')

# ------------------------------------------------------------------ covers_findings: O6 and O7 of round 3
rep('''            out |= set(re.findall(r"\\b([RO][1-5])\\b", seg))''', '''            out |= set(re.findall(r"\\b([RO][1-9])\\b", seg))''')

# ------------------------------------------------------------------ collection
rep('''    inp["R1Text"] = inp["Sources"]["A-6"]          # texto de la ronda anterior (ronda 2): referencia de G5, G6 y de las reglas conservadas''',
    '''    inp["R1Text"] = inp["Sources"]["A-6"]          # texto de la ronda anterior (ronda 3): referencia de G5, G6 y de las reglas conservadas''')
rep('''    inp["R2CommitA6"] = git.obj(R2_COMMIT, A6)
    inp["Review2"] = json.loads(git.show(base, R2_REVIEW) or "null")''',
    '''    inp["R2CommitA6"] = git.obj(R2_COMMIT, A6)
    inp["Review2"] = json.loads(git.show(base, R2_REVIEW) or "null")
    inp["R3CommitA6"] = git.obj(R3_COMMIT, A6)
    inp["Review3"] = json.loads(git.show(base, R3_REVIEW) or "null")''')
rep('''    ents = [e for e in inp["EvidTop"]["Base"] if e[3] not in ("r2", "r3")]''',
    '''    ents = [e for e in inp["EvidTop"]["Base"] if e[3] not in ("r2", "r3", "r4")]''')
rep('''    inp["R2BaseFiles"] = {p[len(EVID_R2):]: b for p, b in inp["EvidFiles"]["Base"].items() if p.startswith(EVID_R2)}''',
    '''    inp["R2BaseFiles"] = {p[len(EVID_R2):]: b for p, b in inp["EvidFiles"]["Base"].items() if p.startswith(EVID_R2)}
    inp["R3BaseFiles"] = {p[len(EVID_R3):]: b for p, b in inp["EvidFiles"]["Base"].items() if p.startswith(EVID_R3)}''')
rep('''    inp["PathFiles"] = {p: git.show(base, p) for p in (TOOL, ACC, SPEC, ACC_NOTES, KITCC, R1_REVIEW, R2_REVIEW)}''',
    '''    inp["PathFiles"] = {p: git.show(base, p) for p in (TOOL, ACC, SPEC, ACC_NOTES, KITCC, R1_REVIEW, R2_REVIEW, R3_REVIEW)}''')
rep('''    inp["R3Head"] = None
    inp["R3Local"] = None
    return inp''', '''    inp["R4Head"] = None
    inp["R4Local"] = None
    return inp''')
rep('''    inp["R3Head"] = {}
    for p, b in git.ls_r(head, EVID_R3).items():
        raw = git.cat_raw(b)
        inp["R3Head"][p[len(EVID_R3):]] = {"Blob": b, "Sha256": hashlib.sha256(raw).hexdigest()}''',
    '''    inp["R4Head"] = {}
    for p, b in git.ls_r(head, EVID_R4).items():
        raw = git.cat_raw(b)
        inp["R4Head"][p[len(EVID_R4):]] = {"Blob": b, "Sha256": hashlib.sha256(raw).hexdigest()}''')
rep('''    """Identificadores de los archivos del paquete r3/ disponibles (cabeza del commit o paquete local)."""
    out = {}
    for src in (inp.get("R3Head"), inp.get("R3Local")):''',
    '''    """Identificadores de los archivos del paquete r4/ disponibles (cabeza del commit o paquete local)."""
    out = {}
    for src in (inp.get("R4Head"), inp.get("R4Local")):''')

# ------------------------------------------------------------------ G1
rep('''            f.append("cabecera con «%s» (prohibido en la ronda 3)" % pat)''', '''            f.append("cabecera con «%s» (prohibido en la ronda 4)" % pat)''')
rep('''    if inp.get("R2CommitA6") != R2_BLOB:
        f.append("el commit %s no contiene la ronda 2 (blob %s)" % (R2_COMMIT[:8], R2_BLOB[:8]))''',
    '''    if inp.get("R2CommitA6") != R2_BLOB:
        f.append("el commit %s no contiene la ronda 2 (blob %s)" % (R2_COMMIT[:8], R2_BLOB[:8]))
    if inp.get("R3CommitA6") != R3_BLOB:
        f.append("el commit %s no contiene la ronda 3 (blob %s)" % (R3_COMMIT[:8], R3_BLOB[:8]))''')
rep('''            f.append("prefijo de blob sin fijar en las guardas ni en el paquete r3/: %s…" % pre)''',
    '''            f.append("prefijo de blob sin fijar en las guardas ni en el paquete r4/: %s…" % pre)''')
rep('''            f.append("identificador de 40 hexadecimales que no es un objeto del repositorio ni del paquete r3/: %s" % h[:12])''',
    '''            f.append("identificador de 40 hexadecimales que no es un objeto del repositorio ni del paquete r4/: %s" % h[:12])''')
rep('''                f.append("SHA-256 sin fijar en las guardas, ni del paquete r3/, ni registrado en decisiones o evidencia: %s" % h[:16])''',
    '''                f.append("SHA-256 sin fijar en las guardas, ni del paquete r4/, ni registrado en decisiones o evidencia: %s" % h[:16])''')
rep('''                f.append("texto conservado de la ronda 2 cita %s, regla %s; la ronda 2 citaba la %s, que en la ronda 3 es la %s: «…%s…»"''',
    '''                f.append("texto conservado de la ronda 3 cita %s, regla %s; la ronda 3 citaba la %s, que en la ronda 4 es la %s: «…%s…»"''')

# ------------------------------------------------------------------ G2
rep('''        f.append("%s en la base no es la ronda 2 (blob %s…)" % (A6, PREV_BLOB[:8]))''',
    '''        f.append("%s en la base no es la ronda 3 (blob %s…)" % (A6, PREV_BLOB[:8]))''')
rep('''        if p.startswith(EVID_R3):
            if s != "A":
                f.append("%s con estado %s (en %s solo se añade)" % (p, s, EVID_R3))''',
    '''        if p.startswith(EVID_R4):
            if s != "A":
                f.append("%s con estado %s (en %s solo se añade)" % (p, s, EVID_R4))''')

# ------------------------------------------------------------------ G3
rep('''    rb = {p: b for p, b in (ev.get("Base") or {}).items() if not p.startswith(EVID_R3)}
    rh = {p: b for p, b in (ev.get("Head") or {}).items() if not p.startswith(EVID_R3)}
    if rb != rh:
        diff = sorted(set(rb.items()) ^ set(rh.items()))
        f.append("archivos de las rondas 1 y 2 de %s distintos entre base y head (fuera de r3/): %s" % (EVID, sorted({p for p, _ in diff})[:4]))
    for lab in ("Base", "Head"):
        ents = [e for e in (inp.get("EvidTop") or {}).get(lab) or [] if e[3] != "r3"]
        th = tree_hash(ents) if ents else None
        if th != EVID_R12_TREE:
            f.append("árbol de %s sin r3/ en %s distinto del de las rondas 1 y 2 (%s…): %s" % (EVID, lab, EVID_R12_TREE[:8], (th or "")[:12]))''',
    '''    rb = {p: b for p, b in (ev.get("Base") or {}).items() if not p.startswith(EVID_R4)}
    rh = {p: b for p, b in (ev.get("Head") or {}).items() if not p.startswith(EVID_R4)}
    if rb != rh:
        diff = sorted(set(rb.items()) ^ set(rh.items()))
        f.append("archivos de las rondas 1 a 3 de %s distintos entre base y head (fuera de r4/): %s" % (EVID, sorted({p for p, _ in diff})[:4]))
    for lab in ("Base", "Head"):
        ents = [e for e in (inp.get("EvidTop") or {}).get(lab) or [] if e[3] != "r4"]
        th = tree_hash(ents) if ents else None
        if th != EVID_R123_TREE:
            f.append("árbol de %s sin r4/ en %s distinto del de las rondas 1 a 3 (%s…): %s" % (EVID, lab, EVID_R123_TREE[:8], (th or "")[:12]))''')
rep('''                     or (p.startswith(EVID) and not p.startswith(EVID_R3)))
    if touched:
        f.append("rutas del commit bajo las superficies prohibidas, del mapa o de las rondas 1 y 2: %s" % touched[:6])''',
    '''                     or (p.startswith(EVID) and not p.startswith(EVID_R4)))
    if touched:
        f.append("rutas del commit bajo las superficies prohibidas, del mapa o de las rondas 1 a 3: %s" % touched[:6])''')
rep('''    return {"Check": "G3 superficies prohibidas, objetos congelados, rondas 1 y 2 intactas y C-20b",''',
    '''    return {"Check": "G3 superficies prohibidas, objetos congelados, rondas 1 a 3 intactas y C-20b",''')

# ------------------------------------------------------------------ G4: §3.17 append-only; §3.18 (round-3 review); evidence r2/ r3/ in base, r4/ in the package
rep('''    if rv.get("Verdict") != "CHANGES REQUIRED" or rv.get("ReviewedBlob") != PREV_BLOB:
        f.append("la revisión de la ronda 2 en la base no es CHANGES REQUIRED sobre el blob de la ronda 2")''',
    '''    if rv.get("Verdict") != "CHANGES REQUIRED" or rv.get("ReviewedBlob") != R2_BLOB:
        f.append("la revisión de la ronda 2 en la base no es CHANGES REQUIRED sobre el blob de la ronda 2")''')
rep('''    f += g4_round2(inp, md, rules, info)
''', '''    f += g4_round2(inp, md, rules, info)
    f += g4_317_append(md, inp["R1Text"])
    f += g4_round3(inp, md, rules, info)
''')
rep('''    return {"Check": "G4 dictamen, revisiones de las rondas 1 (R1-R5, O1-O3) y 2 (R1, R2, O1-O5), M-01..M-08 y compatibilidad",''',
    '''    return {"Check": "G4 dictamen, revisiones de las rondas 1 (R1-R5, O1-O3), 2 (R1, R2, O1-O5) y 3 (R1, O1-O7), M-01..M-08 y compatibilidad",''')
rep('''def evidence_paths(fid, evid, inp, info):
    """Rutas de evidencia de una fila: `r2/…` deben existir en la base; `r3/…` en el paquete r3/ disponible."""
    f = []
    base_r2 = inp.get("R2BaseFiles") or {}
    for p in re.findall(r"`r2/([^`]*)`", evid):
        if not any(q == p or q.startswith(p.rstrip("/") + "/") for q in base_r2):
            f.append("%s: evidencia `r2/%s` ausente de r2/ en la base" % (fid, p))
    r3 = r2_ids(inp)
    for p in re.findall(r"`r3/([^`]*)`", evid):
        if r3:
            if not any(q == p or q.startswith(p.rstrip("/") + "/") for q in r3):
                f.append("%s: evidencia `r3/%s` ausente del paquete r3/" % (fid, p))
        else:
            info.append("%s: evidencia `r3/%s` sin comprobar (paquete r3/ no disponible)" % (fid, p))
    return f''',
    '''def evidence_paths(fid, evid, inp, info):
    """Rutas de evidencia de una fila: `r2/…` y `r3/…` deben existir en la base; `r4/…` en el paquete r4/ disponible."""
    f = []
    for rd, key in (("r2", "R2BaseFiles"), ("r3", "R3BaseFiles")):
        base_rd = inp.get(key) or {}
        for p in re.findall(r"`%s/([^`]*)`" % rd, evid):
            if not any(q == p or q.startswith(p.rstrip("/") + "/") for q in base_rd):
                f.append("%s: evidencia `%s/%s` ausente de %s/ en la base" % (fid, rd, p, rd))
    r4 = r2_ids(inp)
    for p in re.findall(r"`r4/([^`]*)`", evid):
        if r4:
            if not any(q == p or q.startswith(p.rstrip("/") + "/") for q in r4):
                f.append("%s: evidencia `r4/%s` ausente del paquete r4/" % (fid, p))
        else:
            info.append("%s: evidencia `r4/%s` sin comprobar (paquete r4/ no disponible)" % (fid, p))
    return f


def g4_317_append(md, prev):
    """§3.17 de la ronda 3, sin cambio salvo la celda «Prueba» de R1 (b), R1 (c) y R1 (d), que la ronda 4 solo amplía por añadido."""
    f = []
    s, e = section(md, "### 3.17 ")
    ps, pe = section(prev, "### 3.17 ")
    if s is None or ps is None:
        return ["falta §3.17 en la ronda 4 o en la ronda 3"]
    new, old = md[s:e].rstrip("\\n").split("\\n"), prev[ps:pe].rstrip("\\n").split("\\n")
    nt, ot = [l for l in new if l.startswith("|")], [l for l in old if l.startswith("|")]
    if [l for l in new if not l.startswith("|")] != [l for l in old if not l.startswith("|")]:
        f.append("§3.17: el texto fuera de las tablas no es idéntico byte a byte al de la ronda 3")
    if len(nt) != len(ot):
        f.append("§3.17: las tablas tienen %d líneas; en la ronda 3, %d" % (len(nt), len(ot)))
    for x, y in zip(nt, ot):
        if x == y:
            continue
        cx, cy = [c.strip() for c in x.strip().strip("|").split("|")], [c.strip() for c in y.strip().strip("|").split("|")]
        m = R2_ROW_RX.match(cy[0]) if cy else None
        lab = m.group(1) if m else (cy[0][:20] if cy else "?")
        if (lab in R3_317_APPENDED and len(cx) == len(cy) == 4 and cx[:3] == cy[:3] and cx[3].startswith(cy[3])
                and len(cx[3]) > len(cy[3])):
            continue
        f.append("§3.17: la fila %s cambia respecto de la ronda 3 fuera del añadido permitido (celda «Prueba» de R1 (b)-(d))" % lab)
    return f


def g4_round3(inp, md, rules, info):
    """§3.18: hallazgos de la revisión de la ronda 3 (R1 y O1-O7), una fila cada uno y en orden, con corrección, ubicación coherente con las
    líneas «Cubre» (ronda 3; la de A6-3 nombra R1) y evidencia existente; la revisión de la ronda 3 en la base tiene exactamente esos hallazgos,
    es CHANGES REQUIRED sobre el blob de la ronda 3 y no evaluó la materialidad (§5)."""
    f = []
    rv = inp.get("Review3") or {}
    req = [x.get("FindingId") for x in rv.get("RequiredFindings") or []]
    opt = [x.get("FindingId") for x in rv.get("OptionalFindings") or []]
    if req != ["A62-A6R3-R1"] or opt != ["A62-A6R3-O%d" % i for i in range(1, 8)]:
        f.append("la revisión de la ronda 3 en la base no tiene exactamente A62-A6R3-R1 y A62-A6R3-O1..O7: %s %s" % (req, opt))
    if rv.get("Verdict") != "CHANGES REQUIRED" or rv.get("ReviewedBlob") != PREV_BLOB:
        f.append("la revisión de la ronda 3 en la base no es CHANGES REQUIRED sobre el blob de la ronda 3")
    s5, e5 = section(md, "## 5. ", r"^## ")
    if s5 is None or "la revisión acotada de la ronda 3 no evaluó la materialidad" not in norm(md[s5:e5]):
        f.append("§5 no declara que la revisión acotada de la ronda 3 no evaluó la materialidad")
    if "MaterialityConfirmation" in rv:
        f.append("la revisión de la ronda 3 evaluó la materialidad: la tabla M necesitaría su columna")
    rows = table_rows(md, "### 3.18 ")
    if rows is None:
        return f + ["falta §3.18 (correcciones de la ronda 4)"]
    got, order = {}, []
    for r in rows:
        m = R3_ROW_RX.match(r[0]) if r else None
        if not m:
            continue
        order.append(m.group(1))
        if len(r) != 4:
            f.append("fila %s de §3.18 mal formada (%d columnas)" % (m.group(1), len(r)))
            continue
        got.setdefault(m.group(1), r)
    if order != R3_ROWS:
        f.append("§3.18 no tiene R1 y O1-O7, una fila cada uno y en orden: %s" % order)
    items = {d: rule_items(v["Paragraph"]) for d, v in rules.items()}
    named = {}
    for rid, r in got.items():
        corr, where, evid = r[1], r[2], r[3]
        if len(norm(corr)) < 20:
            f.append("%s: corrección vacía o insuficiente en §3.18" % rid)
        for d in where_deltas(where):
            if d not in rules:
                f.append("%s: «Dónde» de §3.18 cita %s, que no existe" % (rid, d))
        for m in re.finditer(r"\\b(A6-[1-9]), reglas? (\\d+(?:(?:-| y |, )\\d+)*)", where):
            d = m.group(1)
            for part in re.split(r", | y ", m.group(2)):
                lo, hi = (part.split("-") + [part])[:2] if "-" in part else (part, part)
                for k in range(int(lo), int(hi) + 1):
                    if k not in items.get(d, {}):
                        f.append("%s: «Dónde» de §3.18 cita %s, regla %d, que no existe" % (rid, d, k))
        named.setdefault(rid, set()).update(d for d in where_deltas(where) if d in rules)
        f += evidence_paths(rid, evid, inp, info)
    cov = {d: covers_findings(v["Covers"], 3) for d, v in rules.items()}
    if "A6-3" in rules and "R1" not in cov.get("A6-3", set()):
        f.append("la línea «Cubre» de A6-3 no nombra R1 de la ronda 3")
    for fid in R3_FINDINGS:
        covering = {d for d, c in cov.items() if fid in c}
        if named.get(fid, set()) != covering:
            f.append("%s (ronda 3): «Dónde» de §3.18 (%s) distinto de los deltas cuya línea «Cubre» lo nombra para la ronda 3 (%s)"
                     % (fid, sorted(named.get(fid, set())), sorted(covering)))
    return f''')

# ------------------------------------------------------------------ G5: identical to round 3; A6-3 rule 6 (b); unchanged sections
rep('''            f.append("%s: el párrafo no es idéntico byte a byte al de la ronda 2" % r)''',
    '''            f.append("%s: el párrafo no es idéntico byte a byte al de la ronda 3" % r)''')
rep('''            f.append("%s: la sección no es idéntica byte a byte a la de la ronda 2" % r)''',
    '''            f.append("%s: la sección no es idéntica byte a byte a la de la ronda 3" % r)''')
rep('''            f.append("A6-3: la %s no es idéntica a la de la ronda 2 (en la ronda 3 solo cambia la regla 6)" % ("cabecera" if k == 0 else "regla %d" % k))
    if sorted(it3) != sorted(it3p):
        f.append("A6-3: el número de reglas cambió respecto de la ronda 2 (%s)" % sorted(it3))
    for d, keep in (("A6-5", range(0, 7)), ("A6-7", range(0, 7))):
        itp = rule_items(r1rules.get(d, {}).get("Paragraph", ""))
        itn = rule_items(paras[d])
        if any(k not in itn for k in keep if k in itp) or max(itn) < max(itp):
            f.append("%s: se retiró o renumeró una regla de la ronda 2 (la ronda 3 solo añade la regla 7)" % d)''',
    '''            f.append("A6-3: la %s no es idéntica a la de la ronda 3 (en la ronda 4 solo cambia la regla 6 (b))" % ("cabecera" if k == 0 else "regla %d" % k))
    if sorted(it3) != sorted(it3p):
        f.append("A6-3: el número de reglas cambió respecto de la ronda 3 (%s)" % sorted(it3))
    f += g5_rule6b(rules["A6-3"], r1rules.get("A6-3") or {})
    f += g5_a63_section(rules["A6-3"], r1rules.get("A6-3") or {})
    for h in SAME_SECTIONS:
        s, e = section(md, h)
        ps, pe = section(inp["R1Text"], h)
        if s is None or ps is None or md[s:e].rstrip("\\n") != inp["R1Text"][ps:pe].rstrip("\\n"):
            f.append("%s: la sección no es idéntica byte a byte a la de la ronda 3 (la ronda 4 no la cambia)" % h.strip())''')
rep('''# ---------------------------------------------------------------- G6
OWNER_LINE = ''', '''def r6_parts(block):
    """(líneas, [(marca, inicio, fin)]) de la regla 6 de un bloque citado de A6-3: «6» (cabecera), (a)-(d), (i)-(v), «cierre» (la frase que
    empieza por «La prueba previa») y «final» (el párrafo que empieza por «El aceptante vuelve a ejecutar»), en el orden del texto."""
    ls = block.split("\\n")
    st = next((i for i, l in enumerate(ls) if re.match(r"^> 6\\. ", l)), None)
    if st is None:
        return ls, []
    en = next((i for i in range(st + 1, len(ls)) if re.match(r"^> \\d+\\. ", ls[i])), len(ls))
    marks = [("6", st)]
    for i in range(st + 1, en):
        m = R6_MARK.match(ls[i])
        if m:
            marks.append((m.group(1) or m.group(2) or ("cierre" if m.group(3) else "final"), i))
    return ls, [(k, a, marks[j + 1][1] if j + 1 < len(marks) else en) for j, (k, a) in enumerate(marks)]


def r6_text(ls, parts, keys):
    return norm(" ".join(ls[i][2:] if ls[i].startswith("> ") else ls[i].lstrip(">") for k, a, b in parts if k in keys for i in range(a, b)))


def g5_rule6b(v, pv):
    """A6-3, regla 6: la cabecera, (a), (c), (d) y el párrafo final, y todo el párrafo salvo (b), byte a byte como en la ronda 3; (b) distinta de
    la de la ronda 3, con `RelayFacts` antes de `Closure`, los apartados (i)-(v) y la frase final, y cada frase exigida una vez y en su apartado."""
    f = []
    ls, parts = r6_parts(v.get("Block") or "")
    pls, pparts = r6_parts(pv.get("Block") or "")
    keys, pkeys = [k for k, _, _ in parts], [k for k, _, _ in pparts]
    if keys != R6_ORDER:
        f.append("A6-3, regla 6: no tiene (a), (b) con (i)-(v) y la frase final, (c), (d) y el párrafo final, una vez cada uno y en orden: %s" % keys)
    if pkeys != R6_ORDER_PREV:
        f.append("A6-3, regla 6, de la ronda 3 sin su forma fijada: %s" % pkeys)
    cur, old = {}, {}
    for k, a, b in parts:
        cur.setdefault(k, ls[a:b])
    for k, a, b in pparts:
        old.setdefault(k, pls[a:b])
    for k, lab in (("6", "la cabecera de la regla 6"), ("a", "el apartado (a)"), ("c", "el apartado (c)"), ("d", "el apartado (d)"),
                   ("final", "el párrafo final («El aceptante vuelve a ejecutar…»)")):
        if cur.get(k) != old.get(k):
            f.append("A6-3, regla 6: %s cambia respecto de la ronda 3 (debe ser igual byte a byte)" % lab)
    rest = [l for i, l in enumerate(ls) if not any(a <= i < b for k, a, b in parts if k in R6B_KEYS)]
    prest = [l for i, l in enumerate(pls) if not any(a <= i < b for k, a, b in pparts if k in R6B_KEYS)]
    if rest != prest:
        f.append("A6-3: fuera de la regla 6 (b), el párrafo no es idéntico byte a byte al de la ronda 3")
    btext, pbtext = r6_text(ls, parts, R6B_KEYS), r6_text(pls, pparts, R6B_KEYS)
    if not btext or btext == pbtext:
        f.append("A6-3, regla 6 (b): igual a la de la ronda 3 o vacía (la ronda 4 la corrige)")
    for k in ("i", "ii", "iii", "iv", "v", "cierre"):
        if k in keys and len(r6_text(ls, parts, [k])) < 20:
            f.append("A6-3, regla 6 (b): el apartado %s está vacío" % k)
    for k, ph in R6B_REQUIRED:
        c = btext.count(norm(ph))
        if c != 1:
            f.append("A6-3, regla 6 (b): la frase exigida «%s» aparece %d veces (exactamente una)" % (ph[:60], c))
        elif norm(ph) not in r6_text(ls, parts, [k]):
            f.append("A6-3, regla 6 (b): la frase exigida «%s» está fuera de su apartado (%s)" % (ph[:60], k))
    head = r6_text(ls, parts, ["b"])
    i1, i2 = head.find("`RelayFacts` ="), head.find("`Closure` =")
    if i1 < 0 or i2 < 0 or i1 > i2:
        f.append("A6-3, regla 6 (b): `RelayFacts` no está antes de `Closure` en la cabecera del apartado")
    return f


def g5_a63_section(v, pv):
    """Sección de A6-3: fuera del párrafo citado y de la línea «Cubre», idéntica a la de la ronda 3; la línea «Cubre», la de la ronda 3 más un
    segmento «… de la ronda 3.»."""
    f = []

    def rest(x):
        body = (x.get("Section") or "").replace(x.get("Block") or "\\0", "", 1)
        ls = body.split("\\n")
        i = next((k for k, l in enumerate(ls) if l.startswith("Cubre:")), None)
        if i is not None:
            j = i + 1
            while j < len(ls) and ls[j].strip() and not ls[j].startswith(">"):
                j += 1
            ls = ls[:i] + ls[j:]
        return "\\n".join(ls).rstrip("\\n")
    if rest(v) != rest(pv):
        f.append("A6-3: la sección, fuera del párrafo citado y de la línea «Cubre», no es idéntica byte a byte a la de la ronda 3")
    pc, nc = pv.get("Covers") or "", v.get("Covers") or ""
    stem = pc[:-1] if pc.endswith(".") else pc
    if not stem or not nc.startswith(stem + "; ") or not re.fullmatch(r"[^;]* de la ronda 3\\.", nc[len(stem) + 2:]):
        f.append("A6-3: la línea «Cubre» no es la de la ronda 3 más un segmento «… de la ronda 3.»: «%s»" % nc[len(stem):][:70])
    return f


# ---------------------------------------------------------------- G6
OWNER_LINE = ''')

# ------------------------------------------------------------------ G8: round 4
rep('''    if "State:\\nPROPUESTA (ronda 3). No aplicada" not in hb:
        f.append("cabecera sin «State: PROPUESTA (ronda 3). No aplicada»")
    if re.search(r"(?m)^State:\\s*\\n(?!PROPUESTA \\(ronda 3\\))", hb):
        f.append("estado distinto de PROPUESTA (ronda 3)")
    if "Ronda 3:\\nNOT LAUNCHED." not in hb:
        f.append("cabecera sin «Ronda 3: NOT LAUNCHED.»")''',
    '''    if "State:\\nPROPUESTA (ronda 4). No aplicada" not in hb:
        f.append("cabecera sin «State: PROPUESTA (ronda 4). No aplicada»")
    if re.search(r"(?m)^State:\\s*\\n(?!PROPUESTA \\(ronda 4\\))", hb):
        f.append("estado distinto de PROPUESTA (ronda 4)")
    if "Ronda 4: NOT LAUNCHED." not in hb:
        f.append("cabecera sin «Ronda 4: NOT LAUNCHED.»")''')
rep('''    if "ronda 3: NOT LAUNCHED" not in arch:
        f.append("§11 sin «ronda 3: NOT LAUNCHED» para el Architect")
    if "sin veredicto sobre la ronda 3" not in coord:
        f.append("§11 sin «sin veredicto sobre la ronda 3» para el Coordinator")''',
    '''    if "ronda 4: NOT LAUNCHED" not in arch:
        f.append("§11 sin «ronda 4: NOT LAUNCHED» para el Architect")
    if "sin veredicto sobre la ronda 4" not in coord:
        f.append("§11 sin «sin veredicto sobre la ronda 4» para el Coordinator")''')

# ------------------------------------------------------------------ G11: r4 pending paths; sources added in round 4
rep('''            f.append("las fuentes del revisor no incluyen %s L%d-L%d (decisiones §79, punto 5)" % (tag, a, b))
''', '''            f.append("las fuentes del revisor no incluyen %s L%d-L%d (decisiones §79, punto 5)" % (tag, a, b))
    for r in G11_REQUIRED_REFS:
        if r not in refs:
            f.append("las fuentes del revisor no incluyen «%s» (fuente añadida en la ronda 4, §2)" % r)
''')
rep('''        if p.startswith(EVID_R3):
            rel = p[len(EVID_R3):]
            if r2 and not any(q == rel or q.startswith(rel.rstrip("/") + "/") for q in r2) and rel:
                f.append("ruta del paquete r3/ que no existe en el paquete: %s" % p)''',
    '''        if p.startswith(EVID_R4):
            rel = p[len(EVID_R4):]
            if r2 and not any(q == rel or q.startswith(rel.rstrip("/") + "/") for q in r2) and rel:
                f.append("ruta del paquete r4/ que no existe en el paquete: %s" % p)''')
rep('''    # líneas en que se apoyan G9 y G10
''', '''    if R3_REVIEW not in {x["Path"] for x in paths_out}:
        f.append("las fuentes del revisor no incluyen la salida de la revisión de la ronda 3 (%s)" % R3_REVIEW)
    # líneas en que se apoyan G9 y G10
''')
rep('''            "Paths": paths_out, "PendingR3Paths": pending,''', '''            "Paths": paths_out, "PendingR4Paths": pending,''')

# ------------------------------------------------------------------ G5b / self-test base / CLI
rep('''        return {"Check": "G5b autoprueba custodiada", "Findings": ["sin a6r3-selftest.json"], "Result": "FAIL"}''',
    '''        return {"Check": "G5b autoprueba custodiada", "Findings": ["sin a6r4-selftest.json"], "Result": "FAIL"}''')
rep('''        f.append("a6r3-selftest.json con Result %s" % st.get("Result"))
    if st.get("ToolBlob") != inp.get("ToolBlob"):
        f.append("ToolBlob de la autoprueba distinto del blob de a6r3-guards.py")''',
    '''        f.append("a6r4-selftest.json con Result %s" % st.get("Result"))
    if st.get("ToolBlob") != inp.get("ToolBlob"):
        f.append("ToolBlob de la autoprueba distinto del blob de a6r4-guards.py")''')
rep('''    base["Status"] = {A6: "M", GUARDS: "A", SELFTEST: "A", EVID_R3 + "preflight.md": "A"}
    base["HeadTexts"] = {A6: base["A6"], GUARDS: "# guardas\\n", SELFTEST: "{}\\n", EVID_R3 + "preflight.md": "# preflight\\n"}''',
    '''    base["Status"] = {A6: "M", GUARDS: "A", SELFTEST: "A", EVID_R4 + "preflight.md": "A"}
    base["HeadTexts"] = {A6: base["A6"], GUARDS: "# guardas\\n", SELFTEST: "{}\\n", EVID_R4 + "preflight.md": "# preflight\\n"}''')
rep('''"Head": list(base["EvidTop"]["Base"]) + [("040000", "tree", "d" * 40, "r3")]}''',
    '''"Head": list(base["EvidTop"]["Base"]) + [("040000", "tree", "d" * 40, "r4")]}''')
rep('''    ap.add_argument("--r3dir")''', '''    ap.add_argument("--r4dir")''')
rep('''    td = a.tmp or tempfile.mkdtemp(prefix="a6r3-guards-")''', '''    td = a.tmp or tempfile.mkdtemp(prefix="a6r4-guards-")''')
rep('''            if a.r3dir:
                texts.update({"r3/" + k: v for k, v in pkg_texts(a.r3dir).items()})
                inp["R3Local"] = local_tree(a.r3dir)''',
    '''            if a.r4dir:
                texts.update({"r4/" + k: v for k, v in pkg_texts(a.r4dir).items()})
                inp["R4Local"] = local_tree(a.r4dir)''')
rep('''            stp = a.selftest or os.path.join(pk, "a6r3-selftest.json")''', '''            stp = a.selftest or os.path.join(pk, "a6r4-selftest.json")''')
rep('''                           tuple(x for x in (a.repo, td, a.cand_repo, a.r3dir, a.pkgdir, os.path.abspath(a.a6) if a.a6 else None) if x))''',
    '''                           tuple(x for x in (a.repo, td, a.cand_repo, a.r4dir, a.pkgdir, os.path.abspath(a.a6) if a.a6 else None) if x))''')

# ------------------------------------------------------------------ mutant helpers: rule 6 (b) of A6-3; append to a table cell
rep('''OWN_A = "A6-ADR0046-9-FX02 = A ("''', '''def m_cell_append(table_start, row_prefix, col, suffix):
    """Añade suffix al final de la celda col de la fila de la tabla de la sección cuya primera celda empieza por row_prefix."""
    def f(inp):
        md = inp["A6"]
        s, e = section(md, table_start, r"^## |^### ")
        assert s is not None, "mutante no aplicable: %s" % table_start
        for mm in re.finditer(r"(?m)^\\|.*$", md[s:e]):
            cells = mm.group(0).strip().strip("|").split("|")
            if cells and cells[0].strip().startswith(row_prefix):
                cells[col] = cells[col].rstrip() + suffix + " "
                inp["A6"] = md[:s + mm.start()] + "|" + "|".join(cells) + "|" + md[s + mm.end():]
                return
        raise AssertionError("mutante no aplicable: fila %s" % row_prefix)
    return f


def m_r6b(fn):
    """Reescribe las líneas de la regla 6 del bloque citado de A6-3 con fn(inp, líneas, partes)."""
    def f(inp):
        md = inp["A6"]
        r = a6_rules(md).get("A6-3")
        assert r, "mutante no aplicable: A6-3"
        a, b = r["BlockSpan"]
        ls, parts = r6_parts(md[a:b])
        new = fn(inp, ls, parts)
        assert new is not None and new != ls, "mutante no aplicable: regla 6 de A6-3"
        inp["A6"] = md[:a] + "\\n".join(new) + md[b:]
    return f


def m_drop_r6b(key):
    """Retira de la regla 6 de A6-3 la parte key (un apartado (i)-(v) o la frase final)."""
    def fn(inp, ls, parts):
        for k, i0, i1 in parts:
            if k == key:
                return ls[:i0] + ls[i1:]
        return None
    return m_r6b(fn)


def m_r6b_prev():
    """Vuelve a poner en A6-3 el apartado (b) de la regla 6 de la ronda 3."""
    def fn(inp, ls, parts):
        pls, pparts = r6_parts(a6_rules(inp["R1Text"])["A6-3"]["Block"])
        pb = [pls[i] for k, i0, i1 in pparts if k == "b" for i in range(i0, i1)]
        idx = [i for k, i0, i1 in parts if k in R6B_KEYS for i in range(i0, i1)]
        if not idx or not pb:
            return None
        return ls[:idx[0]] + pb + ls[idx[-1] + 1:]
    return m_r6b(fn)


def m_r6b_insert(before_key, line):
    """Inserta una línea citada en la regla 6 de A6-3 antes de la parte before_key."""
    def fn(inp, ls, parts):
        for k, i0, i1 in parts:
            if k == before_key:
                return ls[:i0] + [line] + ls[i0:]
        return None
    return m_r6b(fn)


OWN_A = "A6-ADR0046-9-FX02 = A ("''')

# ------------------------------------------------------------------ mutants: round-4 wording (adapted) and new round-4 mutants
rep('''    ("G1", "cabecera: Architect review con la ronda 3 AGREED", m_text("Ronda 3:\\nNOT LAUNCHED. Ninguna", "Ronda 3:\\nAGREED. Ninguna")),''',
    '''    ("G1", "cabecera: Architect review con la ronda 4 AGREED", m_text("Ronda 4: NOT LAUNCHED. Ninguna", "Ronda 4: AGREED. Ninguna")),
    ("G1", "cabecera: la ronda 3 vuelve a NOT LAUNCHED", m_text("Ronda 3:\\nCHANGES REQUIRED, ACCREDITED (R20261013T183701Z-9f80)", "Ronda 3:\\nNOT LAUNCHED")),
    ("G1", "cabecera: la ronda 3 AGREED", m_text("Ronda 3:\\nCHANGES REQUIRED, ACCREDITED", "Ronda 3:\\nAGREED, ACCREDITED")),
    ("G1", "cabecera: la ronda 3 NOT_ACCREDITED", m_text("Ronda 3:\\nCHANGES REQUIRED, ACCREDITED", "Ronda 3:\\nCHANGES REQUIRED, NOT_ACCREDITED")),
    ("G1", "cabecera: la ronda 3 con otra revisión", m_text("(commit a128efef). Revisión formal R20261013T183701Z-9f80", "(commit a128efef). Revisión formal R20261013T183701Z-9f81")),
    ("G1", "cabecera: la ronda 3 con otro commit", m_text("e62f27b8b1329f1b9cfa69a707feb31d22bad1d1 (commit a128efef)", "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1 (commit 9cd074aa)")),
    ("G1", "cabecera: hallazgos de la ronda 3 sin O7", m_text("(A62-A6R3-R1; A62-A6R3-O1..O7)", "(A62-A6R3-R1; A62-A6R3-O1..O6)")),
    ("G1", "cabecera: A62-A6R2-R2 sin CORRECTED", m_text("A62-A6R2-R2 CORRECTED", "A62-A6R2-R2 OPEN")),
    ("G1", "cabecera: Classification amplía la ronda 4 a A6-9", m_text("en la ronda 4, solo en A6-3, regla 6 (b) (§3.18)", "en la ronda 4, en A6-3 y A6-9 (§3.18)")),
    ("G1", "cabecera: Authority con las rondas 2 y 3", m_text("que las rondas 2 a 4 no amplían", "que las rondas 2 y 3 no amplían")),
    ("G1", "cabecera: ronda 4 con CHANGES REQUIRED", m_text("Ronda 4: NOT LAUNCHED.", "Ronda 4: CHANGES REQUIRED.")),
    ("G1", "commit de la ronda 3 sin la ronda 3", m_set(["R3CommitA6"], "0" * 40)),
    ("G1", "prefijo de la ronda 3 falso (e62f27b8…)", m_text("el blob `e62f27b8…` del commit `a128efef`", "el blob `e62f27b9…` del commit `a128efef`")),
    ("G1", "prefijo del árbol de la revisión de la ronda 3 falso", m_text("`I-62-architect-A-6r3/` `f4442fdf…`", "`I-62-architect-A-6r3/` `f4442fde…`")),
    ("G1", "prefijo de output.json de la revisión de la ronda 3 falso", m_text("(`output.json` `691bbc8f…`)", "(`output.json` `691bbc8e…`)")),
    ("G1", "prefijo del árbol r3/ falso", m_text("árbol `I-62-A6/r3/` `a4e93edb…`", "árbol `I-62-A6/r3/` `a4e93edc…`")),''')
rep('''    ("G2", "archivo de r3/ modificado en lugar de añadido", m_status(EVID_R3 + "preflight.md", "M")),
    ("G2", "base con la ronda 1 en lugar de la ronda 2", m_set(["BaseA6"], R1_BLOB)),''',
    '''    ("G2", "archivo de r3/ modificado (evidencia de la ronda 3)", m_status(EVID_R3 + "preflight.md", "M")),
    ("G2", "archivo nuevo en r3/ (fuera de r4/)", m_status(EVID_R3 + "nuevo.md", "A")),
    ("G2", "archivo de r4/ modificado en lugar de añadido", m_status(EVID_R4 + "preflight.md", "M")),
    ("G2", "archivo de r4/ borrado", m_status(EVID_R4 + "viejo.md", "D")),
    ("G2", "base con la ronda 1 en lugar de la ronda 3", m_set(["BaseA6"], R1_BLOB)),
    ("G2", "base con la ronda 2 en lugar de la ronda 3", m_set(["BaseA6"], R2_BLOB)),
    ("G2", "revisión de la ronda 3 tocada", m_status(R3_REVIEW_DIR + "/R20261013T183701Z-9f80/nota.md", "A")),''')
rep('''    ("G2", "privacidad en un archivo añadido", m_status(EVID_R2 + "nota.md", "A", "ruta " + SYNTH_WIN_PATH + "\\n")),''',
    '''    ("G2", "privacidad en un archivo añadido", m_status(EVID_R4 + "nota.md", "A", "ruta " + SYNTH_WIN_PATH + "\\n")),''')
rep('''    ("G3", "revisión de la ronda 2 retirada de la lista de A-6", m_text("`docs/automation/evidence/I-62-architect-A-6r2/` (la revisión de la ronda 2) y ", "y ")),
    ("G3", "cláusula «fuera de r3/» retirada de la lista", m_text(" y todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r3/` (la evidencia de las rondas 1 y 2)", "")),''',
    '''    ("G3", "revisión de la ronda 2 retirada de la lista de A-6", m_text("`docs/automation/evidence/I-62-architect-A-6r2/` (la revisión de la ronda 2), ", "")),
    ("G3", "revisión de la ronda 3 retirada de la lista de A-6", m_text("`docs/automation/evidence/I-62-architect-A-6r3/` (la revisión de la ronda 3 y su kit) y ", "y ")),
    ("G3", "cláusula «fuera de r4/» retirada de la lista", m_text(" y todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r4/` (la evidencia de las rondas 1 a 3)", "")),
    ("G3", "cláusula «fuera de r3/» en lugar de «fuera de r4/»", m_text("`docs/automation/evidence/I-62-A6/` fuera de `r4/`", "`docs/automation/evidence/I-62-A6/` fuera de `r3/`")),
    ("G3", "árbol de la revisión de la ronda 3 cambiado en head", m_set(["Objs", "Head", R3_REVIEW_DIR], "7" * 40)),
    ("G3", "output.json de la revisión de la ronda 3 cambiado en la base", m_set(["Objs", "Base", R3_REVIEW], "8" * 40)),
    ("G3", "árbol de r3/ cambiado en head", m_set(["Objs", "Head", EVID_R3], "9" * 40)),
    ("G3", "archivo de r3/ cambiado en head", lambda inp: inp["EvidFiles"]["Head"].__setitem__(EVID_R3 + "preflight.md", "9" * 40)),
    ("G3", "archivo de r3/ en el commit", m_status(EVID_R3 + "a6r3-guards.py", "M")),
    ("G3", "árbol de I-62-A6/ sin r4/ con un r3/ distinto en head", lambda inp: inp["EvidTop"]["Head"].__setitem__(
        [k for k, e in enumerate(inp["EvidTop"]["Head"]) if e[3] == "r3"][0], ("040000", "tree", "c" * 40, "r3"))),''')
rep('''    ("G4", "§3.17: evidencia r3/ ausente del paquete r3/", lambda inp: inp.__setitem__("R3Local", {"otro.json": {"Blob": "0" * 40, "Sha256": "0" * 64}})),''',
    '''    ("G4", "§3.17: evidencia r3/ ausente de r3/ en la base", lambda inp: inp.__setitem__("R3BaseFiles", {"a6r3-guards.py": "0" * 40})),
    ("G4", "§3.18: evidencia r4/ ausente del paquete r4/", lambda inp: inp.__setitem__("R4Local", {"otro.json": {"Blob": "0" * 40, "Sha256": "0" * 64}})),
    ("G4", "§3.18 sin la fila R1", m_re(r"(### 3\\.18 .*?\\n)\\| R1: [^\\n]*\\n", r"\\1")),
    ("G4", "§3.18 sin la fila O4", m_re(r"(### 3\\.18 .*?\\n)\\| O4: [^\\n]*\\n", r"\\1")),
    ("G4", "§3.18 sin la fila O7", m_re(r"^\\| O7: [^\\n]*\\n", "")),
    ("G4", "§3.18 con O6 y O7 en otro orden", m_re(r"(^\\| O6: [^\\n]*\\n)(^\\| O7: [^\\n]*\\n)", r"\\2\\1")),
    ("G4", "§3.18 con la fila R1 duplicada", m_re(r"(### 3\\.18 .*?\\n)(\\| R1: [^\\n]*\\n)", r"\\1\\2\\2")),
    ("G4", "§3.18: corrección de R1 insuficiente", m_cell("### 3.18 ", "R1:", 1, "corregida")),
    ("G4", "§3.18: «Dónde» de R1 sin A6-3", m_cell("### 3.18 ", "R1:", 2, "§3.12")),
    ("G4", "§3.18: «Dónde» de R1 con una regla inexistente (A6-3, regla 9)", m_cell("### 3.18 ", "R1:", 2, "A6-3, regla 9 (b)")),
    ("G4", "§3.18: «Dónde» de O1 con un delta inexistente", m_cell("### 3.18 ", "O1:", 2, "A6-10")),
    ("G4", "§3.18: «Dónde» de O3 con A6-9 (sin «Cubre»)", m_cell("### 3.18 ", "O3:", 2, "A6-9, regla 4")),
    ("G4", "§3.18: «Dónde» de R1 también en A6-5", m_cell("### 3.18 ", "R1:", 2, "A6-3, regla 6 (b); A6-5, regla 2")),
    ("G4", "línea «Cubre» de A6-3 sin el segmento de la ronda 3", m_re(r"(^Cubre: punto 3 del dictamen;[^\\n]*; R1 y R2 de la ronda 2); [^;\\n]* de la ronda 3\\.$", r"\\1.")),
    ("G4", "línea «Cubre» de A6-5 con R1 de la ronda 3", m_text("R1, R2 y O5 de la ronda 2.", "R1, R2 y O5 de la ronda 2; R1 de la ronda 3.")),
    ("G4", "revisión de la ronda 3 sin O7", lambda inp: inp["Review3"].__setitem__("OptionalFindings", inp["Review3"]["OptionalFindings"][:6])),
    ("G4", "revisión de la ronda 3 con otro veredicto", m_set(["Review3", "Verdict"], "AGREED")),
    ("G4", "revisión de la ronda 3 sobre otro blob", m_set(["Review3", "ReviewedBlob"], R2_BLOB)),
    ("G4", "revisión de la ronda 3 con dos hallazgos exigidos", lambda inp: inp["Review3"]["RequiredFindings"].append({"FindingId": "A62-A6R3-R2"})),
    ("G4", "revisión de la ronda 3 con evaluación de la materialidad", m_set(["Review3", "MaterialityConfirmation"], [])),
    ("G4", "§5 sin «no evaluó la materialidad»", m_text("la revisión acotada de la ronda 3 no evaluó la materialidad", "la revisión acotada de la ronda 3 confirmó la materialidad")),
    ("G4", "§3.17: celda «Prueba» de R1 (b) reescrita, no ampliada", m_row_sub("### 3.17 ", "R1 (b):", "con una ejecución con error, rechazo (con el cierre", "con una ejecución con error, aceptación (con el cierre")),
    ("G4", "§3.17: celda «Prueba» de O3 ampliada (fuera de R1 (b)-(d))", m_cell_append("### 3.17 ", "O3:", 3, " (ampliada en la ronda 4)")),
    ("G4", "§3.17: celda «Corrección» de R1 (b) ampliada", m_cell_append("### 3.17 ", "R1 (b):", 1, " y algo más")),
    ("G4", "§3.17: texto fuera de las tablas cambiado", m_text("**Precondiciones de R2** (decisiones §83, punto 4)", "**Precondiciones de R2** (decisiones §83, puntos 4 y 5)")),''')
rep('''    ("G4", "línea «Cubre» de A6-3 con O5 de la ronda 2", m_text("R1, R4, O2 y O3 de la ronda 1; R1 y R2 de la ronda 2.", "R1, R4, O2 y O3 de la ronda 1; R1, R2 y O5 de la ronda 2.")),''',
    '''    ("G4", "línea «Cubre» de A6-3 con O5 de la ronda 2", m_text("R1, R4, O2 y O3 de la ronda 1; R1 y R2 de la ronda 2;", "R1, R4, O2 y O3 de la ronda 1; R1, R2 y O5 de la ronda 2;")),''')
rep('''    ("G8", "estado AGREED", m_text("State:\\nPROPUESTA (ronda 3). No aplicada", "State:\\nAGREED (ronda 3). No aplicada")),
    ("G8", "estado PROPUESTA sin «ronda 3»", m_text("State:\\nPROPUESTA (ronda 3). No aplicada", "State:\\nPROPUESTA. No aplicada")),
    ("G8", "ronda 3 lanzada en la cabecera", m_text("Ronda 3:\\nNOT LAUNCHED.", "Ronda 3:\\nLAUNCHED.")),
    ("G8", "§11: Architect con la ronda 3 AGREED", m_text("ronda 3: NOT LAUNCHED, hasta una disposición posterior", "ronda 3: AGREED, hasta una disposición posterior")),
    ("G8", "§11: Coordinator con veredicto", m_text("sin veredicto sobre la ronda 3", "veredicto AGREED sobre la ronda 3")),
    ("G8", "§11: Coordinator usa el dictamen de la ronda 2 como acuerdo", m_text("sin usar el dictamen como acuerdo; sin veredicto", "usando el dictamen como acuerdo; sin veredicto")),
    ("G8", "ronda 2 declarada AGREED", m_insert_ev("Ronda 2: AGREED.")),''',
    '''    ("G8", "estado AGREED", m_text("State:\\nPROPUESTA (ronda 4). No aplicada", "State:\\nAGREED (ronda 4). No aplicada")),
    ("G8", "estado PROPUESTA sin «ronda 4»", m_text("State:\\nPROPUESTA (ronda 4). No aplicada", "State:\\nPROPUESTA. No aplicada")),
    ("G8", "estado PROPUESTA (ronda 3)", m_text("State:\\nPROPUESTA (ronda 4). No aplicada", "State:\\nPROPUESTA (ronda 3). No aplicada")),
    ("G8", "ronda 4 lanzada en la cabecera", m_text("Ronda 4: NOT LAUNCHED.", "Ronda 4: LAUNCHED.")),
    ("G8", "§11: Architect con la ronda 4 AGREED", m_text("ronda 4: NOT LAUNCHED, hasta una disposición posterior", "ronda 4: AGREED, hasta una disposición posterior")),
    ("G8", "§11: Coordinator con veredicto", m_text("sin veredicto sobre la ronda 4", "veredicto AGREED sobre la ronda 4")),
    ("G8", "§11: Coordinator usa el dictamen de la ronda 2 como acuerdo", m_text("sin usar el dictamen como acuerdo; ronda 3:", "usando el dictamen como acuerdo; ronda 3:")),
    ("G8", "ronda 2 declarada AGREED", m_insert_ev("Ronda 2: AGREED.")),
    ("G8", "ronda 3 declarada AGREED", m_insert_ev("Ronda 3: AGREED.")),
    ("G8", "ronda 4 declarada PASS", m_insert_ev("Ronda 4 = PASS.")),''')
rep('''    ("G8", "revisión de la ronda 3 declarada lanzada", m_insert_ev("La revisión formal de la ronda 3 lanzada el 2026-10-13.")),''',
    '''    ("G8", "revisión de la ronda 4 declarada lanzada", m_insert_ev("La revisión formal de la ronda 4 lanzada el 2026-10-14.")),''')
rep('''    ("G5", "A6-3, regla 2, alterada (debe ser idéntica a la ronda 2)",''', '''    ("G5", "A6-3, regla 2, alterada (debe ser idéntica a la ronda 3)",''')
rep('''    ("G5", "A6-9 alterado (debe ser idéntico a la ronda 2)",''', '''    ("G5", "A6-9 alterado (debe ser idéntico a la ronda 3)",''')
rep('''    ("G5", "nombre de producto en A6-7",''', '''    ("G5", "regla 6 (b) sin el apartado (i)", m_drop_r6b("i")),
    ("G5", "regla 6 (b) sin el apartado (ii)", m_drop_r6b("ii")),
    ("G5", "regla 6 (b) sin el apartado (iii)", m_drop_r6b("iii")),
    ("G5", "regla 6 (b) sin el apartado (iv)", m_drop_r6b("iv")),
    ("G5", "regla 6 (b) sin el apartado (v)", m_drop_r6b("v")),
    ("G5", "regla 6 (b) sin la frase final (prueba previa y medición)", m_drop_r6b("cierre")),
    ("G5", "regla 6 (b): «no se sustituye» → «se sustituye»", m_in_delta("A6-3", "un término omitido no se sustituye por ningún otro valor", "un término omitido se sustituye por otro valor")),
    ("G5", "regla 6 (b) sin «sin claves repetidas»", m_in_delta("A6-3", "en UTF-8 sin claves repetidas", "en UTF-8")),
    ("G5", "regla 6 (b): «minúscula» → «mayúscula»", m_in_delta("A6-3", "hexadecimales en minúscula", "hexadecimales en mayúscula")),
    ("G5", "regla 6 (b): «Se omite entonces» → «Se conserva entonces»", m_in_delta("A6-3", "Se omite entonces todo término", "Se conserva entonces todo término")),
    ("G5", "regla 6 (b) (ii): «se omite» → «se conserva»", m_in_delta("A6-3", "H.CurrentSha), se omite;", "H.CurrentSha), se conserva;")),
    ("G5", "regla 6 (b) de la ronda 3 reintroducida", m_r6b_prev()),
    ("G5", "regla 6 (b): `Closure` antes de `RelayFacts` (orden de la ronda 3)", m_in_delta("A6-3", "`RelayFacts` = los hechos del relevo que cita la verificación, con el SHA-256 citado; `Closure` = el cierre derivado por la regla de A-6 sobre la independencia,", "`Closure` = el cierre derivado por la regla de A-6 sobre la independencia; `RelayFacts` = los hechos del relevo que cita la verificación, con el SHA-256 citado,")),
    ("G5", "regla 6 (b) con un apartado (vi) añadido", m_r6b_insert("cierre", ">    (vi) un término omitido se repone con la entrega de otra invocación;")),
    ("G5", "regla 6 (b) con una frase exigida repetida", m_r6b_insert("iv", ">    y un extremo omitido da el conjunto vacío;")),
    ("G5", "regla 6 (b) con (iii) y (iv) en otro orden", m_r6b(lambda inp, ls, parts: (lambda d: ls[:d["iii"][0]] + ls[d["iv"][0]:d["iv"][1]] + ls[d["iii"][0]:d["iii"][1]] + ls[d["iv"][1]:])({k: (a, b) for k, a, b in parts}))),
    ("G5", "regla 6 (a) alterada (debe ser idéntica a la ronda 3)", m_in_delta("A6-3", "y su código de salida consta", "y su código de salida queda registrado")),
    ("G5", "regla 6 (c) alterada (debe ser idéntica a la ronda 3)", m_in_delta("A6-3", "la ruta ignorada del paquete", "la ruta ignorada del paquete y su árbol")),
    ("G5", "regla 6 (c) con un espacio doble (byte, mismo texto normalizado)", m_in_delta("A6-3", "existen y son legibles y ninguna", "existen y son legibles  y ninguna")),
    ("G5", "regla 6 (d) alterada (debe ser idéntica a la ronda 3)", m_in_delta("A6-3", "(d) `NotApplicable`, solo por la tabla", "(d) `NotApplicable`, solo y siempre por la tabla")),
    ("G5", "párrafo final de la regla 6 alterado", m_in_delta("A6-3", "y la retención termina con ese registro", "y la retención termina después")),
    ("G5", "cabecera de la regla 6 alterada", m_in_delta("A6-3", "y la custodia con el resultado:", "y la custodia con el resultado y el registro:")),
    ("G5", "A6-3, regla 7, con un espacio doble (byte)", m_in_delta("A6-3", "ni sobre el directorio de trabajo durante una", "ni sobre el directorio de trabajo  durante una")),
    ("G5", "A6-3: «Ubicación» alterada fuera del párrafo", m_text("antes de `### D.7`. Ninguna línea de D.6 cambia de texto.", "antes de `### D.7`. Ninguna línea de D.6 cambia.")),
    ("G5", "línea «Cubre» de A6-3 cambiada en la ronda 1", m_text("Cubre: punto 3 del dictamen; R1, R4, O2 y O3 de la ronda 1;", "Cubre: punto 3 del dictamen; R1, R4 y O2 de la ronda 1;")),
    ("G5", "línea «Cubre» de A6-3 con el segmento de la ronda 3 sin «de la ronda 3»", m_re(r"(^Cubre: punto 3 del dictamen;[^\\n]*; R1 y R2 de la ronda 2; [^;\\n]*?) de la ronda 3\\.$", r"\\1.")),
    ("G5", "A6-5 alterado en un byte (espacio doble)", m_in_delta("A6-5", "solo queda aceptada con R4 en `MATCH`", "solo queda aceptada con R4 en  `MATCH`")),
    ("G5", "A6-7 alterado (debe ser idéntico a la ronda 3)", m_in_delta("A6-7", "Sin esa disposición no hay medición", "Sin esa disposición no hay medición alguna")),
    ("G5", "sección de A6-5 alterada fuera del párrafo (Ubicación)", m_text("antes de `### D.5`.\\nNinguna línea de D.4 cambia de texto.", "antes de `### D.5`.\\nNinguna línea de D.4 cambia.")),
    ("G5", "sección de A6-7 alterada fuera del párrafo (Cubre)", m_text("Cubre: punto 7 del dictamen; R5 y O3 de la ronda 1; R2 y O3 de la ronda 2.", "Cubre: punto 7 del dictamen; R5 y O3 de la ronda 1; R2 de la ronda 2.")),
    ("G5", "tabla de derivación de A6-9 alterada (§3.12)", m_text("U {MergeBase(D.MainSha, K.AuthorityRevision)}", "U {MergeBase(D.MainSha, H.CurrentSha)}")),
    ("G5", "sección de A6-9 alterada fuera del párrafo", m_text("(A6-9, regla 4; mecánica, sin juicio)", "(A6-9, regla 4; mecánica)")),
    ("G5", "nombre de producto en A6-7",''')
rep('''    ("G11", "ruta del paquete r3/ ausente del paquete disponible", lambda inp: inp.__setitem__("R3Local", {"otro.json": {"Blob": "0" * 40, "Sha256": "0" * 64}})),''',
    '''    ("G11", "ruta del paquete r4/ ausente del paquete disponible", lambda inp: inp.__setitem__("R4Local", {"otro.json": {"Blob": "0" * 40, "Sha256": "0" * 64}})),
    ("G11", "sin la fuente RAE L219-L220 de la ronda 4", m_text("(docs/automation/agent-execution/README.md:219-220)", "(docs/automation/agent-execution/README.md)")),
    ("G11", "sin la línea 23 del kit (fuente de la ronda 4)", m_text("controller-contracts.md:23)", "controller-contracts.md)")),
    ("G11", "sin la salida de la revisión de la ronda 3", m_text("R20261013T183701Z-9f80/output.json`, ACCREDITED)", "R20261013T183701Z-9f80/`, ACCREDITED)")),''')

# ------------------------------------------------------------------ auto mutants: §3.18 rows, rule 6 (b) phrases, unchanged sections
rep('''    for rid in R2_ROWS:
        out.append(("G4", "§3.17: corrección de %s vaciada" % rid, m_cell("### 3.17 ", rid + ":", 1, "—")))
    return out''', '''    for rid in R2_ROWS:
        out.append(("G4", "§3.17: corrección de %s vaciada" % rid, m_cell("### 3.17 ", rid + ":", 1, "—")))
    for rid in R3_ROWS:
        out.append(("G4", "§3.18: corrección de %s vaciada" % rid, m_cell("### 3.18 ", rid + ":", 1, "—")))
    for k, ph in R6B_REQUIRED:
        out.append(("G5", "regla 6 (b) (%s): frase exigida desactivada («%s»)" % (k, ph[:40]), m_in_delta("A6-3", ph, "[retirada]")))
    for h in SAME_SECTIONS:
        s, e = section(md, h)
        if s is not None:
            out.append(("G5", "%s alterada (sección sin cambio en la ronda 4)" % h.strip(), m_pos(md.find("\\n", s), " ·")))
    return out''')

out = os.path.join(D, 'a6r4-guards.py')
open(out, 'w', encoding='utf-8', newline='\n').write(t)
diff = difflib.unified_diff(orig.splitlines(True), t.splitlines(True), 'r3/a6r3-guards.py', 'r4/a6r4-guards.py')
open(os.path.join(D, 'a6r4-guards.diff'), 'w', encoding='utf-8', newline='\n').writelines(diff)
compile(t, out, 'exec')
print('ok', len(n_done), 'substitutions;', len(t.splitlines()), 'lines; blob',
      hashlib.sha1(b'blob %d\x00' % len(t.encode('utf-8')) + t.encode('utf-8')).hexdigest())
