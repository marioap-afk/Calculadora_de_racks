# Derives a6r3-guards.py (A-6 round 3, decisiones §83) from the round-2 guards (r2/a6r2-guards.py, blob dea3db71) by exact substitutions.
# Each substitution must match exactly once (or the declared count). Output next to this script; diff in a6r3-guards.diff.
import difflib, hashlib, os, subprocess
D = os.path.dirname(os.path.abspath(__file__))
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
SRC_BLOB = 'dea3db71'
src = subprocess.run(['git', '-C', WT, 'show', 'HEAD:docs/automation/evidence/I-62-A6/r2/a6r2-guards.py'], capture_output=True).stdout
src = src.replace(b'\r\n', b'\n')
assert hashlib.sha1(b'blob %d\x00' % len(src) + src).hexdigest().startswith(SRC_BLOB)
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
"""Guardas mecánicas de la candidata de la ronda 3 de la enmienda I-62 A-6 (decisiones §83).

Adaptadas de docs/automation/evidence/I-62-A6/r2/a6r2-guards.py (ronda 2, blob dea3db71…), que no se modifica, con adapt_guards_r3.py
(sustituciones exactas; diff en a6r3-guards.diff). Autoridad: decisiones §83, punto 5 («Preparar la candidata ronda 3 con: matriz R1/R2 → texto
corregido → prueba; pruebas negativas y mutantes; guardas sobre el blob exacto; comprobación de compatibilidad con los acuerdos A-1..A-5; decisión
sobre los cinco opcionales de ronda 2; identificación de riesgos residuales G1–G6»).

Uso (siempre con `python -I -B`; solo lectura del repositorio; sin red; nunca ejecuta runtimes de IA ni CLI de modelos):
  python -I -B a6r3-guards.py self-test --repo <repo> --a6 <I-62-A-6.md> --out <json> [--base <sha>] [--tmp <dir>]
  python -I -B a6r3-guards.py preview --repo <repo> --a6 <I-62-A-6.md> --out <json> [--base <sha>] [--pkgdir <dir>] [--r3dir <dir>]
                                      [--selftest <json>] [--cand-repo <clon temporal> --cand-head <sha>] [--final] [--tmp <dir>]
  python -I -B a6r3-guards.py run --repo <repo> --base <sha> --head <sha> --out <json> [--tmp <dir>]

Guardas (las de la ronda 2, adaptadas a la ronda 3):
  G1  cabecera de la ronda 3; bloques literales de §2; citas «…»; prefijos, commits abreviados, identificadores de 40 hexadecimales y SHA-256
      conocidos o del paquete r3/; referencias «A6-n, regla k» existentes y, en el texto conservado de la ronda 2, a la misma regla; privacidad.
  G2  rutas del commit: solo docs/initiatives/I-62-A-6.md modificado (de la ronda 2, blob 86da0ae7…, a la ronda 3) y archivos añadidos bajo
      docs/automation/evidence/I-62-A6/r3/; privacidad de lo añadido.
  G3  superficies prohibidas (más las revisiones de las rondas 1 y 2 y todo I-62-A6/ fuera de r3/), objetos congelados, C-20b y rutas del commit.
  G4  dictamen, M-01..M-08 (columnas Dictamen, Ronda 1 y Ronda 2), revisión de la ronda 1 (§3.16) y de la ronda 2 (R1, R2 y O1-O5 en §3.17,
      ubicaciones coherentes con las líneas «Cubre», evidencia existente) y compatibilidad con A-1..A-5.
  G5  pureza del delta (nueve párrafos en las mismas anclas); A6-1, A6-2, A6-4, A6-6, A6-8 y A6-9 idénticos byte a byte a la ronda 2, y en A6-3
      todas las reglas salvo la 6; frases exigidas y prohibidas; cifras de topes; prohibiciones de §3.13.
  G6  líneas del Owner idénticas a las de la ronda anterior; (a) en decisiones §78.
  G7  identidad por blob del instrumento, del aceptante y de la especificación.
  G8  estado: PROPUESTA (ronda 3), ronda 3 NOT LAUNCHED, sin veredicto del Coordinator sobre la ronda 3, sin AGREED, FX-02 PASS, C-24 PASS ni
      F6 GATE PASS, ninguna línea del Owner pedida; en `run` (o --final), ningún marcador pendiente.
  G9  alcance de R5 (solo VERIFY, PLAN excluida).
  G10 resultado del aceptante: la tabla de A6-5, regla 2 (con la condición de R4 sobre `Verification` de A6-3, regla 6 (c)), frente a los bytes
      del aceptante fijado; correspondencias fijas de A6-5 con AP, RAE y V14.
  G11 fuentes y rangos de líneas para el revisor siguiente.
  G5b autoprueba custodiada: Result PASS, ToolBlob = blob de a6r3-guards.py, A6TextBlob = blob de la A-6, todos los mutantes muertos.
Los blobs de los textos se calculan con CRLF -> LF. Todas las lecturas Git con GIT_NO_REPLACE_OBJECTS=1 y GIT_OPTIONAL_LOCKS=0. Sin escrituras
salvo --out y un directorio temporal propio. Nunca escribe la ruta del repositorio ni identidades del host.
''' + t[i:]

# ------------------------------------------------------------------ identities
rep('R1_BLOB = "eac473c0d2e58cf1c4594f48a8816b3833af6509"     # A-6 ronda 1\n',
    'R1_BLOB = "eac473c0d2e58cf1c4594f48a8816b3833af6509"     # A-6 ronda 1\n'
    'R2_COMMIT = "ae9fe0e94b2dea9bc840d358d876deb6a6407391"   # commit que publicó la ronda 2\n'
    'R2_BLOB = "86da0ae770e683a647f2724860a4bd890061154d"     # A-6 ronda 2: texto anterior de la ronda 3\n'
    'PREV_BLOB = R2_BLOB\n')
rep('EVID_R2 = EVID + "r2/"\nGUARDS, SELFTEST = EVID_R2 + "a6r2-guards.py", EVID_R2 + "a6r2-selftest.json"\n',
    'EVID_R2 = EVID + "r2/"\nEVID_R3 = EVID + "r3/"\nGUARDS, SELFTEST = EVID_R3 + "a6r3-guards.py", EVID_R3 + "a6r3-selftest.json"\n')
rep('EVID_R1_TREE = "7f504a1710c593f896390474562574623552813c"   # árbol de I-62-A6/ de la ronda 1, recalculado sin la entrada r2\n',
    'EVID_R1_TREE = "7f504a1710c593f896390474562574623552813c"   # árbol de I-62-A6/ de la ronda 1, recalculado sin la entrada r2\n'
    'R2_REVIEW_DIR = "docs/automation/evidence/I-62-architect-A-6r2"\n'
    'R2_REVIEW = R2_REVIEW_DIR + "/R20261012T195511Z-e5c3/output.json"\n'
    'R2_REVIEW_TREE = "acf2fa30bc7a1db4756e3f743cb1dcc227734b8b"\n'
    'R2_REVIEW_BLOB = "fc357d09f1d345fa5ad55ca5aeffb6dc7510abaa"\n'
    'EVID_R2_TREE = "411949b0f20cb6edb1a01a208ccdfb1b3b19db76"   # árbol de I-62-A6/r2/ (ronda 2)\n'
    'EVID_R12_TREE = "1c7f8fbb25ebe173dad12f56157a6e6aeaa84a3c"  # árbol de I-62-A6/ en la base de la ronda 3 (rondas 1 y 2), sin r3\n')
rep('    R1_REVIEW_DIR: R1_REVIEW_TREE, R1_REVIEW: R1_REVIEW_BLOB,\n}',
    '    R1_REVIEW_DIR: R1_REVIEW_TREE, R1_REVIEW: R1_REVIEW_BLOB,\n    R2_REVIEW_DIR: R2_REVIEW_TREE, R2_REVIEW: R2_REVIEW_BLOB, EVID_R2: EVID_R2_TREE,\n}')
rep('FORBIDDEN_SURFACES = R1_SURFACES + [R1_REVIEW_DIR + "/"]\nEVID_OUTSIDE_R2_PHRASE = "todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r2/`"',
    'FORBIDDEN_SURFACES = R1_SURFACES + [R1_REVIEW_DIR + "/", R2_REVIEW_DIR + "/"]\nEVID_OUTSIDE_R2_PHRASE = "todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r3/`"')
rep('"A-5": (A5, FROZEN[A5]), "A-6": (A6, R1_BLOB)}', '"A-5": (A5, FROZEN[A5]), "A-6": (A6, PREV_BLOB)}')

# ------------------------------------------------------------------ header of round 3
rep('''    "Amendment: A-6 (candidata, ronda 2;",
    "Previous round:\\nronda 1 = blob %s (commit %s)." % (R1_BLOB, R1_COMMIT[:8]),''',
    '''    "Amendment: A-6 (candidata, ronda 3;",
    "Previous round:\\nronda 1 = blob %s (commit %s)." % (R1_BLOB, R1_COMMIT[:8]),
    "ronda 2 = blob %s (commit %s). Revisión formal R20261012T195511Z-e5c3" % (R2_BLOB, R2_COMMIT[:8]),''')
rep('''    "Architect review:\\nronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261011T215735Z-c434). Ronda 2: NOT LAUNCHED.",''',
    '''    "Architect review:\\nronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261011T215735Z-c434). Ronda 2: CHANGES REQUIRED, NOT_ACCREDITED "
    "(R20261012T195511Z-e5c3). Ronda 3:\\nNOT LAUNCHED.",''')
rep('''    "State:\\nPROPUESTA (ronda 2). No aplicada",
]''', '''    "State:\\nPROPUESTA (ronda 3). No aplicada",
]''')
rep('''    "que la ronda 2 no amplía",''', '''    "que las rondas 2 y 3 no amplían",
    "Architect CHANGES REQUIRED (A62-A6R2-R1 y A62-A6R2-R2; A62-A6R2-O1..O5); auditor NOT_ACCREDITED",
    "sin usar el dictamen como acuerdo formal (decisiones §83, punto 1)",
    "en la ronda 3, solo en A6-3, regla 6, A6-5 y A6-7 (§3.17)",''')
rep('''r"Owner decision:\\n\\s*(?:NOT )?REQUIRED", r"(?i)ronda 2:\\s*\\**(?:AGREED|CHANGES REQUIRED|LAUNCHED)"]''',
    '''r"Owner decision:\\n\\s*(?:NOT )?REQUIRED", r"(?i)ronda 3:\\s*\\**(?:AGREED|CHANGES REQUIRED|LAUNCHED)",
                    r"(?i)ronda 2:\\s*\\**AGREED"]''')

# ------------------------------------------------------------------ short prefixes: the two A-6 rounds by commit; I-62-A6/ of round 1 recomputed
rep('''               "eac473c0": A6, "7f504a17": EVID.rstrip("/"), "c5cc6168": EVID + "a6-guards.py", "8a61a8c4": EVID + "a6-selftest.json"}''',
    '''               "c5cc6168": EVID + "a6-guards.py", "8a61a8c4": EVID + "a6-selftest.json", "fc357d09": R2_REVIEW, "acf2fa30": R2_REVIEW_DIR,
               "411949b0": EVID_R2}
SHORT_AT_REV = {"eac473c0": (R1_COMMIT, A6), "86da0ae7": (R2_COMMIT, A6)}   # prefijos de las rondas anteriores, en su commit
SHORT_EVID_R1 = "7f504a17"                                                   # árbol de I-62-A6/ de la ronda 1: recalculado sin r2/ ni r3/''')

# ------------------------------------------------------------------ round-2 review findings
rep('R1_FINDINGS = ["R1", "R2", "R3", "R4", "R5", "O1", "O2", "O3"]\n',
    'R1_FINDINGS = ["R1", "R2", "R3", "R4", "R5", "O1", "O2", "O3"]\n'
    'R2_ROWS = ["R1 (a)", "R1 (b)", "R1 (c)", "R1 (d)", "R2 (a)", "R2 (b)", "O1", "O2", "O3", "O4", "O5"]   # filas de §3.17\n'
    'R2_FINDINGS = ["R1", "R2", "O1", "O2", "O3", "O4", "O5"]\n'
    'R2_ROW_RX = re.compile(r"^((?:R[12] \\([a-d]\\))|O[1-5]):")\n')

# ------------------------------------------------------------------ deltas: identical to round 2
rep('IDENTICAL_TO_R1 = ["A6-2", "A6-6"]\n',
    'IDENTICAL_TO_R1 = ["A6-1", "A6-2", "A6-4", "A6-6", "A6-8", "A6-9"]      # párrafo idéntico byte a byte al de la ronda 2\n'
    'SECTION_IDENTICAL = ["A6-1", "A6-2", "A6-4", "A6-6", "A6-8"]          # sección entera idéntica (la de A6-9 lleva el registro G1-G6)\n'
    'A63_SAME_RULES = [0, 1, 2, 3, 4, 5, 7]                                  # en A6-3 solo cambia la regla 6\n')

# REQUIRED: A6-3 rule 6, A6-5 and A6-7 (round-3 phrases); round-2 phrases that changed
rep('''             "`Runs` = toda ejecución del instrumento que conste en el registro de sesión del adapter",
             "la interpretación del Controller nunca es autoridad para él",''',
    '''             "`Runs` = toda ejecución del instrumento que conste en el registro de sesión del adapter",
             "solo para una invocación VERIFY en que corrió el instrumento",
             "reconstruida de ese registro con la regla de reconstrucción que demostró la medición",
             "El registro tiene que estar completo", "Es ejecución del instrumento toda orden registrada que contiene el blob fijado",
             "que tiene exactamente la forma de la orden declarada",
             "exactamente un sobre del instrumento, en su serialización canónica y terminado en LF, sin alterar ningún byte",
             "no hay solicitud y la salida queda sin aceptar",
             "solo si esos tres archivos existen y son legibles y ninguna de las comprobaciones `Identity`, `Scope`, `CleanTree`, `Trailer` y "
             "`FreeText` está en `not_run` en esa salida",
             "entre ellos N4 (entrega ausente), la solicitud no lleva `Verification` y declara R4 no aplicable, con la causa",
             "compara con el sobre registrado sus argumentos, entradas, llamadas a Git, hechos y error (R3)",
             "en una invocación VERIFY sin ejecuciones no hay solicitud ni corrida del aceptante",
             "la interpretación del Controller nunca es autoridad para él",''')
rep('''             "FX-02 solo es PASS con salidas aceptadas y conformes al oráculo sellado", "FAIL sigue siendo una violación observada en una salida aceptada",''',
    '''             "FX-02 solo es PASS con salidas ingeridas (aceptadas, o sin ejecuciones por la regla 7) y conformes al oráculo sellado",
             "FAIL sigue siendo una violación observada en una salida ingerida",''')
rep('''             "cambiarlo exige otro blob, otra corrección de A-6 y otra determinación del Owner", "ambigüedad material (S-03), con STOP"],''',
    '''             "cambiarlo exige otro blob, otra corrección de A-6 y otra determinación del Owner", "ambigüedad material (S-03), con STOP",
             "con el ámbito de la regla 7",
             "R4 está en `MATCH` si la solicitud lleva `Verification`, y en `NOT_APPLICABLE` solo si la regla de A-6 en D.6 la dejó fuera",
             "Una salida `EXECUTION_VERIFIED` solo queda aceptada con R4 en `MATCH`",
             "para R4, `fail` y `not_run` son por igual distintos de `pass` (16.22)", "la juzga 16.9 sin cambio",
             "el juicio del control sigue siendo su oráculo relativo (RAE §10)", "un registro que no permite reconstruir las ejecuciones",
             "una salida sin ejecuciones que cita una orden del instrumento o un `FactsSha256`",
             "la sesión no la reejecuta: la devuelve al Coordinator como evidencia contradictoria (S-04), con STOP",
             "con un cierre derivado del cierre efectivo real de las invocaciones VERIFY del kit",
             "o si una lectura que la verificación necesita queda fuera de ese cierre", "**ámbito:**",
             "Ninguna está obligada a ejecutar el instrumento", "**con ejecuciones:**", "Solo a ellas se aplican el aceptante",
             "**sin ejecuciones:**", "No hay solicitud ni aceptante",
             "con 16.9, 16.11 y 16.22 sin cambio, y no puede apoyarse en ningún hecho del instrumento", "**registro insuficiente:**",
             "**dónde no corresponde ejecutarlo:**", "En N4 (entrega ausente) no hay ninguna orden del instrumento sobre la entrega",
             "en nc1 (`CurrentSha` inexistente) ninguna sobre ese `CurrentSha`"],''')
rep('''             "P-01 y OD-2 rigen sin cambio"],''',
    '''             "P-01 y OD-2 rigen sin cambio",
             "Sobre el registro de sesión real de esa misma invocación demuestra además",
             "que `git`, resuelto por nombre en el entorno de la invocación, es el Git declarado",
             "la reconstrucción de `Runs` de todas sus ejecuciones del instrumento", "sin alterar ningún byte",
             "una corrida del aceptante fijado sobre esas `Runs`", "que da `ACCEPT_FACTS` con R1, R2, R3 y R9 en `MATCH`",
             "R4 y R10 no aplicables, porque la medición no produce una verificación", "regla de reconstrucción de `Runs` o cierre de VERIFY de la regla 7",
             "antes de la medición, una disposición del Coordinator fija cómo el cierre efectivo de las invocaciones VERIFY de FX-02",
             "en particular las de `CurrentSha`", "o limita las órdenes del instrumento a las lecturas que ese cierre admite",
             "Sin esa disposición no hay medición", "la celda no es elegible con el instrumento y el aceptante no se usa",
             "Ninguna de estas condiciones se da por cumplida con evidencia que todavía no existe"],''')
rep('''    "A6-3": ["lee el worktree real", "con ref dedicada", "sobre un clon del fixture", "siguen siendo entradas privadas prohibidas"],''',
    '''    "A6-3": ["lee el worktree real", "con ref dedicada", "sobre un clon del fixture", "siguen siendo entradas privadas prohibidas",
             "con su argv y los bytes de su salida tal como constan", "normalizando el fin de línea", "con un registro simulado"],''')
rep('''    "A6-5": ["también la deja en `not_run`", "es FAIL si el rechazo", "dan REJECT_FACTS", "Coordinator rechaza el VERIFIED"],''',
    '''    "A6-5": ["también la deja en `not_run`", "es FAIL si el rechazo", "dan REJECT_FACTS", "Coordinator rechaza el VERIFIED",
             "R1, R2, R3, R4, R9 y R10 están en `MATCH`", "el instrumento es obligatorio", "R4 siempre en `MATCH`", "se reejecuta aunque registre S-04"],''')
rep('''    "A6-7": ["Su tope es dos", "reutiliza el bloque", "usar el bloque agotado"],''',
    '''    "A6-7": ["Su tope es dos", "reutiliza el bloque", "usar el bloque agotado", "con un registro simulado", "basta un registro simulado"],''')

# ------------------------------------------------------------------ claims of round 3
rep('''    (r"(?i)ronda 2\\s*[:=]\\s*\\**(?!NOT\\b)(?:AGREED|LAUNCHED|CHANGES REQUIRED|PASS)", "veredicto o lanzamiento de la ronda 2 declarado"),
    (r"(?i)(?<!ninguna )revisión (?:formal )?de la ronda 2 (?:lanzada|ejecutada|AGREED)", "revisión de la ronda 2 declarada lanzada"),''',
    '''    (r"(?i)ronda 3\\s*[:=]\\s*\\**(?!NOT\\b)(?:AGREED|LAUNCHED|CHANGES REQUIRED|PASS)", "veredicto o lanzamiento de la ronda 3 declarado"),
    (r"(?i)(?<!ninguna )revisión (?:formal )?de la ronda 3 (?:lanzada|ejecutada|AGREED)", "revisión de la ronda 3 declarada lanzada"),
    (r"(?i)ronda 2\\s*[:=]\\s*\\**(?:AGREED|PASS)", "ronda 2 declarada AGREED"),''')

# ------------------------------------------------------------------ G10 correspondences and acceptor facts
rep('''    (5, "FX-02 solo es PASS con salidas aceptadas y conformes al oráculo sellado", "V14", 2534, "| **PASS** | ejecutado y conforme al oráculo |"),''',
    '''    (5, "FX-02 solo es PASS con salidas ingeridas (aceptadas, o sin ejecuciones por la regla 7) y conformes al oráculo sellado", "V14", 2534,
     "| **PASS** | ejecutado y conforme al oráculo |"),''')
rep('''    (6, "ambigüedad material (S-03), con STOP", "AP", 686, "| S-03 | material ambiguity | STOP |"),
]''', '''    (6, "ambigüedad material (S-03), con STOP", "AP", 686, "| S-03 | material ambiguity | STOP |"),
    (2, "para R4, `fail` y `not_run` son por igual distintos de `pass` (16.22)", "AP", 1204, "es `not_run`, y `not_run` cuenta como fail"),
    (2, "sin ella si una comprobación anterior en `fail` la dejó sin entrada", "AP", 623,
     "salvo el causado porque una comprobación anterior en `fail` dejó sin entrada a la comprobación"),
    (4, "como evidencia contradictoria (S-04), con STOP", "AP", 687, "| S-04 | contradictory evidence | STOP |"),
    (7, "en nc1 (`CurrentSha` inexistente)", "RAE", 228, "entrega con `CurrentSha` inexistente → `Identity`"),
    (7, "En N4 (entrega ausente)", "V14", 2441, "N4 handoff ausente → BLOCKED"),
]''')
rep('''    (AP, FROZEN[AP], [(1236, 1237), (1240, 1240), (610, 611), (625, 625), (653, 653), (686, 687), (1204, 1204), (1251, 1251)]),
    (RAE, FROZEN[RAE], [(220, 220), (225, 226)]),
    (V14, FROZEN[V14], [(2534, 2536)]),''',
    '''    (AP, FROZEN[AP], [(1236, 1237), (1240, 1240), (610, 611), (623, 623), (625, 625), (653, 653), (686, 687), (1204, 1204), (1251, 1251)]),
    (RAE, FROZEN[RAE], [(220, 220), (225, 226), (228, 228)]),
    (V14, FROZEN[V14], [(2441, 2441), (2534, 2536)]),''')
rep('''ACC_FUNCS = ["check_r5", "check_r6", "check_r7", "_compare_r5", "_compare_r7", "_run_comparisons", "check_r8", "check_r10", "check_r2",
             "check_r1", "check_r9", "accept", "main"]
ACC_CONSTS = ["CHECK_IDS", "WAIVABLE", "R5_COMMANDS", "R7_COMMANDS", "COMPARED_COMMANDS"]''',
    '''ACC_FUNCS = ["check_r5", "check_r6", "check_r7", "_compare_r5", "_compare_r7", "_run_comparisons", "check_r8", "check_r10", "check_r2",
             "check_r1", "check_r9", "accept", "main", "check_r3", "check_r4"]
ACC_CONSTS = ["CHECK_IDS", "WAIVABLE", "R5_COMMANDS", "R7_COMMANDS", "COMPARED_COMMANDS", "R4_CHECKS"]''')
rep('BASENAME_DIRS = [BMIN + "/", "docs/automation/evidence/I-62-F6/kits/FX-02/", R1_REVIEW_DIR + "/", RULING_DIR + "/", EVID]',
    'BASENAME_DIRS = [BMIN + "/", "docs/automation/evidence/I-62-F6/kits/FX-02/", R1_REVIEW_DIR + "/", R2_REVIEW_DIR + "/", RULING_DIR + "/", EVID]')

# ------------------------------------------------------------------ covers_findings by round
rep('''def covers_findings(line):
    m = re.search(r"\\bdel dictamen(.*)$", line)
    tail = m.group(1) if m else ""
    if "ronda 1" not in tail:
        return set()
    return set(re.findall(r"\\b([RO][1-5])\\b", tail))''',
    '''def covers_findings(line, rnd=1):
    """Hallazgos de la ronda rnd que nombra la línea «Cubre» (segmentos separados por «;» que terminan en «de la ronda N»)."""
    m = re.search(r"\\bdel dictamen(.*)$", line)
    tail = m.group(1) if m else ""
    out = set()
    for seg in tail.split(";"):
        mm = re.search(r"de la ronda (\\d)\\b", seg)
        if mm and int(mm.group(1)) == rnd:
            out |= set(re.findall(r"\\b([RO][1-5])\\b", seg))
    return out''')

# ------------------------------------------------------------------ collection
rep('''    inp["R1Text"] = inp["Sources"]["A-6"]
    inp["R1CommitA6"] = git.obj(R1_COMMIT, A6)''',
    '''    inp["R1Text"] = inp["Sources"]["A-6"]          # texto de la ronda anterior (ronda 2): referencia de G5, G6 y de las reglas conservadas
    inp["R1Orig"] = git.cat_raw(R1_BLOB).decode("utf-8").replace("\\r\\n", "\\n")   # texto de la ronda 1 (citas de la ronda 1)
    inp["R1CommitA6"] = git.obj(R1_COMMIT, A6)
    inp["R2CommitA6"] = git.obj(R2_COMMIT, A6)
    inp["Review2"] = json.loads(git.show(base, R2_REVIEW) or "null")''')
rep('''    inp["ShortAtBase"] = {pre: sb.get(p) for pre, p in SHORT_BLOBS.items()}''',
    '''    inp["ShortAtBase"] = {pre: sb.get(p) for pre, p in SHORT_BLOBS.items()}
    for pre, (rev, p) in SHORT_AT_REV.items():
        inp["ShortAtBase"][pre] = git.obj(rev, p)''')
rep('''    inp["EvidFiles"] = {"Base": git.ls_r(base, EVID), "Head": git.ls_r(head, EVID)}''',
    '''    inp["EvidFiles"] = {"Base": git.ls_r(base, EVID), "Head": git.ls_r(head, EVID)}
    ents = [e for e in inp["EvidTop"]["Base"] if e[3] not in ("r2", "r3")]
    inp["ShortAtBase"][SHORT_EVID_R1] = tree_hash(ents) if ents else None
    inp["R2BaseFiles"] = {p[len(EVID_R2):]: b for p, b in inp["EvidFiles"]["Base"].items() if p.startswith(EVID_R2)}''')
rep('''    inp["PathFiles"] = {p: git.show(base, p) for p in (TOOL, ACC, SPEC, ACC_NOTES, KITCC, R1_REVIEW)}''',
    '''    inp["PathFiles"] = {p: git.show(base, p) for p in (TOOL, ACC, SPEC, ACC_NOTES, KITCC, R1_REVIEW, R2_REVIEW)}''')
rep('''    inp["R2Head"] = None
    inp["R2Local"] = None
    return inp''', '''    inp["R3Head"] = None
    inp["R3Local"] = None
    return inp''')
rep('''    inp["R2Head"] = {}
    for p, b in git.ls_r(head, EVID_R2).items():
        raw = git.cat_raw(b)
        inp["R2Head"][p[len(EVID_R2):]] = {"Blob": b, "Sha256": hashlib.sha256(raw).hexdigest()}


def r2_ids(inp):
    """Identificadores de los archivos del paquete r2/ disponibles (cabeza del commit o paquete local)."""
    out = {}
    for src in (inp.get("R2Head"), inp.get("R2Local")):''',
    '''    inp["R3Head"] = {}
    for p, b in git.ls_r(head, EVID_R3).items():
        raw = git.cat_raw(b)
        inp["R3Head"][p[len(EVID_R3):]] = {"Blob": b, "Sha256": hashlib.sha256(raw).hexdigest()}


def r2_ids(inp):
    """Identificadores de los archivos del paquete r3/ disponibles (cabeza del commit o paquete local)."""
    out = {}
    for src in (inp.get("R3Head"), inp.get("R3Local")):''')

# ------------------------------------------------------------------ G1
rep('''            f.append("cabecera con «%s» (prohibido en la ronda 2)" % pat)''', '''            f.append("cabecera con «%s» (prohibido en la ronda 3)" % pat)''')
rep('''    if inp.get("R1CommitA6") != R1_BLOB:
        f.append("el commit %s no contiene la ronda 1 (blob %s)" % (R1_COMMIT[:8], R1_BLOB[:8]))''',
    '''    if inp.get("R1CommitA6") != R1_BLOB:
        f.append("el commit %s no contiene la ronda 1 (blob %s)" % (R1_COMMIT[:8], R1_BLOB[:8]))
    if inp.get("R2CommitA6") != R2_BLOB:
        f.append("el commit %s no contiene la ronda 2 (blob %s)" % (R2_COMMIT[:8], R2_BLOB[:8]))''')
rep('''    r1n = norm(inp["R1Text"])''', '''    r1n = norm(inp.get("R1Orig") or "")''')
rep('''            f.append("prefijo de blob sin fijar en las guardas ni en el paquete r2/: %s…" % pre)''',
    '''            f.append("prefijo de blob sin fijar en las guardas ni en el paquete r3/: %s…" % pre)''')
rep('''        elif not any(b.startswith(pre) for b in r2_blobs | {h for h in full_in_text if inp.get("Hex40Exists", {}).get(h)}):''',
    '''        elif pre in SHORT_AT_REV or pre == SHORT_EVID_R1:
            if not (inp["ShortAtBase"].get(pre) or "").startswith(pre):
                f.append("prefijo %s… distinto del objeto real de la ronda anterior" % pre)
        elif not any(b.startswith(pre) for b in r2_blobs | {h for h in full_in_text if inp.get("Hex40Exists", {}).get(h)}):''')
rep('''            f.append("identificador de 40 hexadecimales que no es un objeto del repositorio ni del paquete r2/: %s" % h[:12])''',
    '''            f.append("identificador de 40 hexadecimales que no es un objeto del repositorio ni del paquete r3/: %s" % h[:12])''')
rep('''                f.append("SHA-256 sin fijar en las guardas, ni del paquete r2/, ni registrado en decisiones o evidencia: %s" % h[:16])''',
    '''                f.append("SHA-256 sin fijar en las guardas, ni del paquete r3/, ni registrado en decisiones o evidencia: %s" % h[:16])''')
rep('''                f.append("texto conservado de la ronda 1 cita %s, regla %s; la ronda 1 citaba la %s, que en la ronda 2 es la %s: «…%s…»"''',
    '''                f.append("texto conservado de la ronda 2 cita %s, regla %s; la ronda 2 citaba la %s, que en la ronda 3 es la %s: «…%s…»"''')

# ------------------------------------------------------------------ G2
rep('''    if inp.get("BaseA6") != R1_BLOB:
        f.append("%s en la base no es la ronda 1 (blob %s…)" % (A6, R1_BLOB[:8]))''',
    '''    if inp.get("BaseA6") != PREV_BLOB:
        f.append("%s en la base no es la ronda 2 (blob %s…)" % (A6, PREV_BLOB[:8]))''')
rep('''        if p.startswith(EVID_R2):
            if s != "A":
                f.append("%s con estado %s (en %s solo se añade)" % (p, s, EVID_R2))''',
    '''        if p.startswith(EVID_R3):
            if s != "A":
                f.append("%s con estado %s (en %s solo se añade)" % (p, s, EVID_R3))''')

# ------------------------------------------------------------------ G3
rep('''    rb = {p: b for p, b in (ev.get("Base") or {}).items() if not p.startswith(EVID_R2)}
    rh = {p: b for p, b in (ev.get("Head") or {}).items() if not p.startswith(EVID_R2)}
    if rb != rh:
        diff = sorted(set(rb.items()) ^ set(rh.items()))
        f.append("archivos de la ronda 1 de %s distintos entre base y head (fuera de r2/): %s" % (EVID, sorted({p for p, _ in diff})[:4]))
    for lab in ("Base", "Head"):
        ents = [e for e in (inp.get("EvidTop") or {}).get(lab) or [] if e[3] != "r2"]
        th = tree_hash(ents) if ents else None
        if th != EVID_R1_TREE:
            f.append("árbol de %s sin r2/ en %s distinto del de la ronda 1 (%s…): %s" % (EVID, lab, EVID_R1_TREE[:8], (th or "")[:12]))''',
    '''    rb = {p: b for p, b in (ev.get("Base") or {}).items() if not p.startswith(EVID_R3)}
    rh = {p: b for p, b in (ev.get("Head") or {}).items() if not p.startswith(EVID_R3)}
    if rb != rh:
        diff = sorted(set(rb.items()) ^ set(rh.items()))
        f.append("archivos de las rondas 1 y 2 de %s distintos entre base y head (fuera de r3/): %s" % (EVID, sorted({p for p, _ in diff})[:4]))
    for lab in ("Base", "Head"):
        ents = [e for e in (inp.get("EvidTop") or {}).get(lab) or [] if e[3] != "r3"]
        th = tree_hash(ents) if ents else None
        if th != EVID_R12_TREE:
            f.append("árbol de %s sin r3/ en %s distinto del de las rondas 1 y 2 (%s…): %s" % (EVID, lab, EVID_R12_TREE[:8], (th or "")[:12]))''')
rep('''                     or (p.startswith(EVID) and not p.startswith(EVID_R2)))
    if touched:
        f.append("rutas del commit bajo las superficies prohibidas, del mapa o de la ronda 1: %s" % touched[:6])''',
    '''                     or (p.startswith(EVID) and not p.startswith(EVID_R3)))
    if touched:
        f.append("rutas del commit bajo las superficies prohibidas, del mapa o de las rondas 1 y 2: %s" % touched[:6])''')
rep('''    return {"Check": "G3 superficies prohibidas, objetos congelados, ronda 1 intacta y C-20b",''',
    '''    return {"Check": "G3 superficies prohibidas, objetos congelados, rondas 1 y 2 intactas y C-20b",''')

# ------------------------------------------------------------------ G4: §3.16 evidence in r2/ (base); §3.17; materiality with the Ronda 2 column
rep('''            for p in re.findall(r"`r2/([^`]*)`", evid):
                if r2:
                    if not any(q == p or q.startswith(p.rstrip("/") + "/") for q in r2):
                        f.append("%s: evidencia `r2/%s` ausente del paquete r2/" % (fid, p))
                else:
                    info.append("%s: evidencia `r2/%s` sin comprobar (paquete r2/ no disponible)" % (fid, p))''',
    '''            f += evidence_paths(fid, evid, inp, info)''')
rep('''    # materialidad
    mrows = table_rows(md, "## 5. ", r"^## ")''',
    '''    f += g4_round2(inp, md, rules, info)
    # materialidad
    rv2 = inp.get("Review2") or {}
    conf2 = {m.get("Id"): m for m in rv2.get("MaterialityConfirmation") or []}
    mrows = table_rows(md, "## 5. ", r"^## ")''')
rep('''        if len(r) != 5:
            f.append("%s: fila de la tabla M sin las cinco columnas (M, Activado, Dictamen, Ronda 1, Motivo)" % mid)
            continue''',
    '''        if len(r) != 6:
            f.append("%s: fila de la tabla M sin las seis columnas (M, Activado, Dictamen, Ronda 1, Ronda 2, Motivo)" % mid)
            continue
        c2 = conf2.get(mid) or {}
        want2 = "%s (%s)" % (c2.get("ReviewerAssessment"), c2.get("Agreement"))
        if r[4] != want2:
            f.append("%s: columna Ronda 2 «%s» distinta de MaterialityConfirmation de la revisión de la ronda 2 («%s»)" % (mid, r[4], want2))
        if c2.get("A6Declared") != ("yes" if mid in M_YES else "no"):
            f.append("%s: la revisión de la ronda 2 no registra el valor declarado de la ronda 2" % mid)''')
rep('''        if r[2] != mat.get(mid) or not r[4].strip():''', '''        if r[2] != mat.get(mid) or not r[5].strip():''')
rep('''            miss = [fid for fid in fids if not any(re.search(r"\\b%s\\b" % d, r[4]) for d in where_of.get(fid, []))]''',
    '''            miss = [fid for fid in fids if not any(re.search(r"\\b%s\\b" % d, r[5]) for d in where_of.get(fid, []))]''')
rep('''    return {"Check": "G4 dictamen, revisión de la ronda 1 (R1-R5, O1-O3), M-01..M-08 y compatibilidad", "Points": len(rows),
            "Findings": f, "Info": info, "Result": "PASS" if not f else "FAIL"}''',
    '''    return {"Check": "G4 dictamen, revisiones de las rondas 1 (R1-R5, O1-O3) y 2 (R1, R2, O1-O5), M-01..M-08 y compatibilidad",
            "Points": len(rows), "Findings": f, "Info": info, "Result": "PASS" if not f else "FAIL"}


def evidence_paths(fid, evid, inp, info):
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
    return f


def g4_round2(inp, md, rules, info):
    """§3.17: hallazgos de la revisión de la ronda 2 (R1 (a)-(d), R2 (a)-(b), O1-O5), una fila cada uno y en orden, con corrección, ubicación
    coherente con las líneas «Cubre» (ronda 2) y evidencia existente; la revisión de la ronda 2 en la base tiene exactamente esos hallazgos."""
    f = []
    rv = inp.get("Review2") or {}
    req = [x.get("FindingId") for x in rv.get("RequiredFindings") or []]
    opt = [x.get("FindingId") for x in rv.get("OptionalFindings") or []]
    if req != ["A62-A6R2-R1", "A62-A6R2-R2"] or opt != ["A62-A6R2-O%d" % i for i in range(1, 6)]:
        f.append("la revisión de la ronda 2 en la base no tiene exactamente A62-A6R2-R1, R2 y A62-A6R2-O1..O5: %s %s" % (req, opt))
    if rv.get("Verdict") != "CHANGES REQUIRED" or rv.get("ReviewedBlob") != PREV_BLOB:
        f.append("la revisión de la ronda 2 en la base no es CHANGES REQUIRED sobre el blob de la ronda 2")
    rows = table_rows(md, "### 3.17 ")
    if rows is None:
        return f + ["falta §3.17 (correcciones de la ronda 3)"]
    got, order = {}, []
    for r in rows:
        m = R2_ROW_RX.match(r[0]) if r else None
        if not m:
            continue
        order.append(m.group(1))
        if len(r) != 4:
            f.append("fila %s de §3.17 mal formada (%d columnas)" % (m.group(1), len(r)))
            continue
        got.setdefault(m.group(1), r)
    if order != R2_ROWS:
        f.append("§3.17 no tiene R1 (a)-(d), R2 (a)-(b) y O1-O5, una fila cada uno y en orden: %s" % order)
    items = {d: rule_items(v["Paragraph"]) for d, v in rules.items()}
    named = {}
    for rid, r in got.items():
        corr, where, evid = r[1], r[2], r[3]
        if len(norm(corr)) < 20:
            f.append("%s: corrección vacía o insuficiente en §3.17" % rid)
        for d in where_deltas(where):
            if d not in rules:
                f.append("%s: «Dónde» de §3.17 cita %s, que no existe" % (rid, d))
        for m in re.finditer(r"\\b(A6-[1-9]), reglas? (\\d+(?:(?:-| y |, )\\d+)*)", where):
            d = m.group(1)
            for part in re.split(r", | y ", m.group(2)):
                lo, hi = (part.split("-") + [part])[:2] if "-" in part else (part, part)
                for k in range(int(lo), int(hi) + 1):
                    if k not in items.get(d, {}):
                        f.append("%s: «Dónde» de §3.17 cita %s, regla %d, que no existe" % (rid, d, k))
        named.setdefault(rid.split(" ")[0], set()).update(d for d in where_deltas(where) if d in rules)
        f += evidence_paths(rid, evid, inp, info)
    cov = {d: covers_findings(v["Covers"], 2) for d, v in rules.items()}
    for fid in R2_FINDINGS:
        covering = {d for d, c in cov.items() if fid in c}
        if named.get(fid, set()) != covering:
            f.append("%s (ronda 2): «Dónde» de §3.17 (%s) distinto de los deltas cuya línea «Cubre» lo nombra para la ronda 2 (%s)"
                     % (fid, sorted(named.get(fid, set())), sorted(covering)))
    return f''')

# ------------------------------------------------------------------ G5: identical to round 2
rep('''    r1rules = a6_rules(inp["R1Text"])
    for r in IDENTICAL_TO_R1:
        if rules[r]["Block"] != r1rules.get(r, {}).get("Block"):
            f.append("%s: el párrafo no es idéntico byte a byte al de la ronda 1" % r)
        if rules[r]["Section"].rstrip("\\n") != r1rules.get(r, {}).get("Section", "").rstrip("\\n"):
            f.append("%s: la sección no es idéntica byte a byte a la de la ronda 1" % r)''',
    '''    r1rules = a6_rules(inp["R1Text"])
    for r in IDENTICAL_TO_R1:
        if rules[r]["Block"] != r1rules.get(r, {}).get("Block"):
            f.append("%s: el párrafo no es idéntico byte a byte al de la ronda 2" % r)
        if r in SECTION_IDENTICAL and rules[r]["Section"].rstrip("\\n") != r1rules.get(r, {}).get("Section", "").rstrip("\\n"):
            f.append("%s: la sección no es idéntica byte a byte a la de la ronda 2" % r)
    it3, it3p = rule_items(paras["A6-3"]), rule_items(r1rules.get("A6-3", {}).get("Paragraph", ""))
    for k in A63_SAME_RULES:
        if it3.get(k) != it3p.get(k):
            f.append("A6-3: la %s no es idéntica a la de la ronda 2 (en la ronda 3 solo cambia la regla 6)" % ("cabecera" if k == 0 else "regla %d" % k))
    if sorted(it3) != sorted(it3p):
        f.append("A6-3: el número de reglas cambió respecto de la ronda 2 (%s)" % sorted(it3))
    for d, keep in (("A6-5", range(0, 7)), ("A6-7", range(0, 7))):
        itp = rule_items(r1rules.get(d, {}).get("Paragraph", ""))
        itn = rule_items(paras[d])
        if any(k not in itn for k in keep if k in itp) or max(itn) < max(itp):
            f.append("%s: se retiró o renumeró una regla de la ronda 2 (la ronda 3 solo añade la regla 7)" % d)''')

# ------------------------------------------------------------------ G6 wording
rep('''            f.append("%s: la línea no es idéntica byte a byte a la de la ronda 1" % name)''',
    '''            f.append("%s: la línea no es idéntica byte a byte a la de la ronda anterior" % name)''')
rep('''    return {"Check": "G6 líneas del Owner (idénticas a la ronda 1; (a) registrada en §78)",''',
    '''    return {"Check": "G6 líneas del Owner (idénticas a la ronda anterior; (a) registrada en §78)",''')

# ------------------------------------------------------------------ G8: round 3
rep('''    if "State:\\nPROPUESTA (ronda 2). No aplicada" not in hb:
        f.append("cabecera sin «State: PROPUESTA (ronda 2). No aplicada»")
    if re.search(r"(?m)^State:\\s*\\n(?!PROPUESTA \\(ronda 2\\))", hb):
        f.append("estado distinto de PROPUESTA (ronda 2)")
    if "Ronda 2: NOT LAUNCHED." not in hb:
        f.append("cabecera sin «Ronda 2: NOT LAUNCHED.»")''',
    '''    if "State:\\nPROPUESTA (ronda 3). No aplicada" not in hb:
        f.append("cabecera sin «State: PROPUESTA (ronda 3). No aplicada»")
    if re.search(r"(?m)^State:\\s*\\n(?!PROPUESTA \\(ronda 3\\))", hb):
        f.append("estado distinto de PROPUESTA (ronda 3)")
    if "Ronda 3:\\nNOT LAUNCHED." not in hb:
        f.append("cabecera sin «Ronda 3: NOT LAUNCHED.»")''')
rep('''    if "ronda 2: NOT LAUNCHED" not in arch:
        f.append("§11 sin «ronda 2: NOT LAUNCHED» para el Architect")
    if "sin veredicto sobre la ronda 2" not in coord:
        f.append("§11 sin «sin veredicto sobre la ronda 2» para el Coordinator")''',
    '''    if "ronda 3: NOT LAUNCHED" not in arch:
        f.append("§11 sin «ronda 3: NOT LAUNCHED» para el Architect")
    if "sin veredicto sobre la ronda 3" not in coord:
        f.append("§11 sin «sin veredicto sobre la ronda 3» para el Coordinator")
    if "sin usar el dictamen como acuerdo" not in coord:
        f.append("§11 sin «sin usar el dictamen como acuerdo» para el Coordinator (decisiones §83, punto 1)")''')

# ------------------------------------------------------------------ G10: acceptor facts for R4 and the new table form
rep('''    out["NaWaivableOnly"] = "if na is not None and rid in WAIVABLE:''',
    '''    out["R4Checks"] = list(consts.get("R4_CHECKS") or [])
    r4p = set()
    for c in ast.walk(funcs.get("check_r4") or ast.Module(body=[], type_ignores=[])):
        if (isinstance(c, ast.Call) and isinstance(c.func, ast.Name) and c.func.id == "read_pinned" and c.args
                and isinstance(c.args[0], ast.Call) and isinstance(c.args[0].func, ast.Attribute) and c.args[0].func.attr == "get"
                and c.args[0].args and isinstance(c.args[0].args[0], ast.Constant)):
            r4p.add(c.args[0].args[0].value)
    out["R4Pinned"] = sorted(r4p)
    out["R4NotRunIsNonPass"] = ('r if r in ("pass", "fail", "not_run") else None' in src
                                and 'good = res is not None and ctl is not None and ((ctl == "pass") == res)' in src)
    out["R4AuthorityRule"] = 'row["Rule"] = "map part: any defect forbids pass"' in src and 'good = ctl is not None and not (defects and ctl == "pass")' in src
    out["R3NoRunsNoData"] = "def check_r3(ctx):\\n    if not ctx.runs:\\n        return None" in src
    out["R9NoRunsError"] = 'raise AcceptError("MISSING_INPUT", "no tool envelope to check against the closure")' in src
    out["NaWaivableOnly"] = "if na is not None and rid in WAIVABLE:''')
rep('''    t = re.search(r"\\*\\*aceptada\\*\\* solo si el código es (\\d+), la `Decision` es `([A-Z_]+)` y, además: (.+?) están en `MATCH`; (.+?) está en "
                  r"`MANUAL`; y (.+?) están en `MATCH` o en `NOT_APPLICABLE`, esto último solo cuando la solicitud lo declaró por esta tabla", r2)
    if not t:
        f.append("A6-5, regla 2: no se reconoce la tabla de aceptación (MATCH, MANUAL, MATCH o NOT_APPLICABLE)")
        sets = None
    else:
        if int(t.group(1)) != acc["ExitCodes"].get("ACCEPT_FACTS") or t.group(2) != acc.get("DecisionTrue"):
            f.append("A6-5, regla 2: la aceptación no exige el código y la Decision de ACCEPT_FACTS del aceptante")
        sets = {"MATCH": rset(t.group(3)), "MANUAL": rset(t.group(4)), "NA": rset(t.group(5))}''',
    '''    t = re.search(r"\\*\\*aceptada\\*\\* solo si el código es (\\d+), la `Decision` es `([A-Z_]+)` y, además: (.+?) están en `MATCH`; (.+?) está en "
                  r"`MANUAL`; (R\\d+) está en `MATCH` si la solicitud lleva `Verification`, y en `NOT_APPLICABLE` solo si la regla de A-6 en D\\.6 la "
                  r"dejó fuera; y (.+?) están en `MATCH` o en `NOT_APPLICABLE`, esto último solo cuando la solicitud lo declaró por esta tabla", r2)
    if not t:
        f.append("A6-5, regla 2: no se reconoce la tabla de aceptación (MATCH, MANUAL, R4 condicionada a `Verification`, MATCH o NOT_APPLICABLE)")
        sets = None
    else:
        if int(t.group(1)) != acc["ExitCodes"].get("ACCEPT_FACTS") or t.group(2) != acc.get("DecisionTrue"):
            f.append("A6-5, regla 2: la aceptación no exige el código y la Decision de ACCEPT_FACTS del aceptante")
        sets = {"MATCH": rset(t.group(3)), "MANUAL": rset(t.group(4)), "NA": rset(t.group(6)) | {t.group(5)}}
        if t.group(5) != "R4" or "Verification" not in acc["RequestKeys"].get("R4", []):
            f.append("A6-5, regla 2: la comprobación condicionada a `Verification` (%s) no es la que lee `Verification` en el aceptante (R4)" % t.group(5))''')
rep('''    # entradas de la solicitud que necesitan las comprobaciones exigidas, frente a A6-3, reglas 5-6''',
    '''    # condición de R4 (A6-3, regla 6 (c)) frente al aceptante: los tres archivos que lee y las cinco comprobaciones que recalcula
    a3c = rule_items(rules["A6-3"]["Paragraph"]).get(6, "")
    if acc.get("R4Pinned") != ["Controller", "Delegation", "Handoff"]:
        f.append("aceptante: R4 no lee exactamente la salida del Controller, la delegación y la entrega (%s)" % acc.get("R4Pinned"))
    if "solo si esos tres archivos existen y son legibles" not in a3c:
        f.append("A6-3, regla 6 (c): `Verification` no se condiciona a que existan los tres archivos que lee R4")
    mm = re.search(r"ninguna de las comprobaciones ((?:`[A-Za-z]+`(?:, | y )?)+) está en `not_run`", a3c)
    five = set(re.findall(r"`([A-Za-z]+)`", mm.group(1))) if mm else set()
    if not acc.get("R4Checks") or five != set(acc["R4Checks"]) - {"Authority"}:
        f.append("A6-3, regla 6 (c): las comprobaciones de la condición de `not_run` (%s) no son las que R4 recalcula (%s, sin Authority)"
                 % (sorted(five), acc.get("R4Checks")))
    if not acc.get("R4NotRunIsNonPass"):
        f.append("aceptante: R4 no trata `not_run` como distinto de `pass`, como dice A6-5, regla 2")
    if not acc.get("R4AuthorityRule") or "exige que una `Authority` en `pass` no tenga defectos de mapa" not in r2:
        f.append("A6-5, regla 2: la regla de `Authority` de R4 no coincide con el aceptante (un defecto de mapa prohíbe pass)")
    if not (acc.get("R3NoRunsNoData") and acc.get("R9NoRunsError")) or "Ninguna está obligada a ejecutar el instrumento" not in it.get(7, ""):
        f.append("A6-5, regla 7: sin `Runs`, el aceptante fijado nunca acepta (R3 sin datos, R9 sin sobres); la regla debe excluir esas invocaciones")
    # entradas de la solicitud que necesitan las comprobaciones exigidas, frente a A6-3, reglas 5-6''')

# ------------------------------------------------------------------ G11: r3 pending paths
rep('''        if p.startswith(EVID_R2):
            rel = p[len(EVID_R2):]
            if r2 and not any(q == rel or q.startswith(rel.rstrip("/") + "/") for q in r2) and rel:
                f.append("ruta del paquete r2/ que no existe en el paquete: %s" % p)''',
    '''        if p.startswith(EVID_R3):
            rel = p[len(EVID_R3):]
            if r2 and not any(q == rel or q.startswith(rel.rstrip("/") + "/") for q in r2) and rel:
                f.append("ruta del paquete r3/ que no existe en el paquete: %s" % p)''')
rep('''            "Paths": paths_out, "PendingR2Paths": pending,''', '''            "Paths": paths_out, "PendingR3Paths": pending,''')

# ------------------------------------------------------------------ G5b / self-test base / CLI
rep('''        return {"Check": "G5b autoprueba custodiada", "Findings": ["sin a6r2-selftest.json"], "Result": "FAIL"}''',
    '''        return {"Check": "G5b autoprueba custodiada", "Findings": ["sin a6r3-selftest.json"], "Result": "FAIL"}''')
rep('''        f.append("a6r2-selftest.json con Result %s" % st.get("Result"))
    if st.get("ToolBlob") != inp.get("ToolBlob"):
        f.append("ToolBlob de la autoprueba distinto del blob de a6r2-guards.py")''',
    '''        f.append("a6r3-selftest.json con Result %s" % st.get("Result"))
    if st.get("ToolBlob") != inp.get("ToolBlob"):
        f.append("ToolBlob de la autoprueba distinto del blob de a6r3-guards.py")''')
rep('''    base["Status"] = {A6: "M", GUARDS: "A", SELFTEST: "A", EVID_R2 + "preflight.md": "A"}
    base["HeadTexts"] = {A6: base["A6"], GUARDS: "# guardas\\n", SELFTEST: "{}\\n", EVID_R2 + "preflight.md": "# preflight\\n"}''',
    '''    base["Status"] = {A6: "M", GUARDS: "A", SELFTEST: "A", EVID_R3 + "preflight.md": "A"}
    base["HeadTexts"] = {A6: base["A6"], GUARDS: "# guardas\\n", SELFTEST: "{}\\n", EVID_R3 + "preflight.md": "# preflight\\n"}''')
rep('''"Head": list(base["EvidTop"]["Base"]) + [("040000", "tree", "d" * 40, "r2")]}''',
    '''"Head": list(base["EvidTop"]["Base"]) + [("040000", "tree", "d" * 40, "r3")]}''')
rep('''    ap.add_argument("--r2dir")''', '''    ap.add_argument("--r3dir")''')
rep('''    td = a.tmp or tempfile.mkdtemp(prefix="a6r2-guards-")''', '''    td = a.tmp or tempfile.mkdtemp(prefix="a6r3-guards-")''')
rep('''            if a.r2dir:
                texts.update({"r2/" + k: v for k, v in pkg_texts(a.r2dir).items()})
                inp["R2Local"] = local_tree(a.r2dir)''',
    '''            if a.r3dir:
                texts.update({"r3/" + k: v for k, v in pkg_texts(a.r3dir).items()})
                inp["R3Local"] = local_tree(a.r3dir)''')
rep('''            stp = a.selftest or os.path.join(pk, "a6r2-selftest.json")''', '''            stp = a.selftest or os.path.join(pk, "a6r3-selftest.json")''')
rep('''                           tuple(x for x in (a.repo, td, a.cand_repo, a.r2dir, a.pkgdir, os.path.abspath(a.a6) if a.a6 else None) if x))''',
    '''                           tuple(x for x in (a.repo, td, a.cand_repo, a.r3dir, a.pkgdir, os.path.abspath(a.a6) if a.a6 else None) if x))''')
rep('''    for i in range(1, 9):
        mid = "M-0%d" % i
        out.append(("G4", "%s: motivo vaciado" % mid, m_cell("## 5. ", mid, 4, "")))''',
    '''    for i in range(1, 9):
        mid = "M-0%d" % i
        out.append(("G4", "%s: motivo vaciado" % mid, m_cell("## 5. ", mid, 5, "")))
        out.append(("G4", "%s: columna Ronda 2 alterada" % mid, m_cell("## 5. ", mid, 4, "UNKNOWN (DISAGREE)")))
    for rid in R2_ROWS:
        out.append(("G4", "§3.17: corrección de %s vaciada" % rid, m_cell("### 3.17 ", rid + ":", 1, "—")))''')

# ------------------------------------------------------------------ G10: the definition of `Verification` in the request
rep('''    if "solo si esos tres archivos existen y son legibles" not in a3c:''',
    '''    if "`Verification` = la salida `controller-verification/v2` de la invocación" not in a3c:
        f.append("A6-3, regla 6 (c): la solicitud no define `Verification` como la salida `controller-verification/v2` de la invocación")
    if "solo si esos tres archivos existen y son legibles" not in a3c:''')

# ------------------------------------------------------------------ mutants: round-3 wording
rep('''    ("G1", "cabecera: Architect review con la ronda 2 AGREED", m_text("Ronda 2: NOT LAUNCHED. Ninguna", "Ronda 2: AGREED. Ninguna")),''',
    '''    ("G1", "cabecera: Architect review con la ronda 3 AGREED", m_text("Ronda 3:\\nNOT LAUNCHED. Ninguna", "Ronda 3:\\nAGREED. Ninguna")),
    ("G1", "commit de la ronda 2 sin la ronda 2", m_set(["R2CommitA6"], "0" * 40)),
    ("G1", "prefijo de la ronda 2 falso (86da0ae7…)", m_text("el blob `86da0ae7…` del commit `ae9fe0e9`", "el blob `86da0ae8…` del commit `ae9fe0e9`")),
    ("G1", "prefijo del árbol de la ronda 1 de I-62-A6/ falso (7f504a17…)", m_text("de la ronda 1 `7f504a17…` fuera de `r2/`", "de la ronda 1 `7f504a18…` fuera de `r2/`")),
    ("G1", "prefijo del árbol de la revisión de la ronda 2 falso", m_text("`I-62-architect-A-6r2/` `acf2fa30…`", "`I-62-architect-A-6r2/` `acf2fa31…`")),''')
rep('''    ("G3", "revisión de la ronda 1 retirada de la lista de A-6", m_text("`docs/automation/evidence/I-62-architect-A-6/` (la revisión de la ronda 1) y ", "")),
    ("G3", "cláusula «fuera de r2/» retirada de la lista", m_text(" y todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r2/` (la evidencia de la ronda 1)", "")),''',
    '''    ("G3", "revisión de la ronda 1 retirada de la lista de A-6", m_text("`docs/automation/evidence/I-62-architect-A-6/` (la revisión de la ronda 1), ", "")),
    ("G3", "revisión de la ronda 2 retirada de la lista de A-6", m_text("`docs/automation/evidence/I-62-architect-A-6r2/` (la revisión de la ronda 2) y ", "y ")),
    ("G3", "cláusula «fuera de r3/» retirada de la lista", m_text(" y todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r3/` (la evidencia de las rondas 1 y 2)", "")),
    ("G3", "árbol de la revisión de la ronda 2 cambiado en head", m_set(["Objs", "Head", R2_REVIEW_DIR], "7" * 40)),
    ("G3", "output.json de la revisión de la ronda 2 cambiado en la base", m_set(["Objs", "Base", R2_REVIEW], "8" * 40)),
    ("G3", "árbol de r2/ cambiado en head", m_set(["Objs", "Head", EVID_R2], "9" * 40)),
    ("G3", "archivo de r2/ cambiado en head", lambda inp: inp["EvidFiles"]["Head"].__setitem__(EVID_R2 + "preflight.md", "9" * 40)),''')
rep('''    ("G4", "§3.16: evidencia r2/ ausente del paquete", lambda inp: inp.__setitem__("R2Local", {"a6r2-guards.py": {"Blob": "0" * 40, "Sha256": "0" * 64}})),''',
    '''    ("G4", "§3.16: evidencia r2/ ausente de r2/ en la base", lambda inp: inp.__setitem__("R2BaseFiles", {"a6r2-guards.py": "0" * 40})),
    ("G4", "§3.17: evidencia r3/ ausente del paquete r3/", lambda inp: inp.__setitem__("R3Local", {"otro.json": {"Blob": "0" * 40, "Sha256": "0" * 64}})),
    ("G4", "§3.17 sin la fila R1 (b)", m_re(r"^\\| R1 \\(b\\): [^\\n]*\\n", "")),
    ("G4", "§3.17 con O4 y O5 en otro orden", m_re(r"(^\\| O4: [^\\n]*\\n)(^\\| O5: [^\\n]*\\n)", r"\\2\\1")),
    ("G4", "§3.17: «Dónde» de O5 sin A6-5", m_cell("### 3.17 ", "O5:", 2, "§3.12")),
    ("G4", "§3.17: «Dónde» de R2 (a) con un delta inexistente", m_cell("### 3.17 ", "R2 (a):", 2, "A6-3, regla 6 (a); A6-10")),
    ("G4", "§3.17: «Dónde» de R1 (b) con una regla inexistente (A6-5, regla 9)", m_cell("### 3.17 ", "R1 (b):", 2, "A6-3, regla 6 (c); A6-5, regla 9")),
    ("G4", "línea «Cubre» de A6-7 sin R2 y O3 de la ronda 2", m_text("R5 y O3 de la ronda 1; R2 y O3 de la ronda 2.", "R5 y O3 de la ronda 1.")),
    ("G4", "línea «Cubre» de A6-3 con O5 de la ronda 2", m_text("R1, R4, O2 y O3 de la ronda 1; R1 y R2 de la ronda 2.", "R1, R4, O2 y O3 de la ronda 1; R1, R2 y O5 de la ronda 2.")),
    ("G4", "revisión de la ronda 2 sin O5", lambda inp: inp["Review2"].__setitem__("OptionalFindings", inp["Review2"]["OptionalFindings"][:4])),
    ("G4", "revisión de la ronda 2 con otro veredicto", m_set(["Review2", "Verdict"], "AGREED")),
    ("G4", "revisión de la ronda 2 sobre otro blob", m_set(["Review2", "ReviewedBlob"], "1" * 40)),''')
rep('''    ("G8", "estado AGREED", m_text("State:\\nPROPUESTA (ronda 2). No aplicada", "State:\\nAGREED (ronda 2). No aplicada")),
    ("G8", "estado PROPUESTA sin «ronda 2»", m_text("State:\\nPROPUESTA (ronda 2). No aplicada", "State:\\nPROPUESTA. No aplicada")),
    ("G8", "ronda 2 lanzada en la cabecera", m_text("Ronda 2: NOT LAUNCHED.", "Ronda 2: LAUNCHED.")),
    ("G8", "§11: Architect con la ronda 2 AGREED", m_text("ronda 2: NOT LAUNCHED, hasta una disposición posterior", "ronda 2: AGREED, hasta una disposición posterior")),
    ("G8", "§11: Coordinator con veredicto", m_text("sin veredicto sobre la ronda 2", "veredicto AGREED sobre la ronda 2")),''',
    '''    ("G8", "estado AGREED", m_text("State:\\nPROPUESTA (ronda 3). No aplicada", "State:\\nAGREED (ronda 3). No aplicada")),
    ("G8", "estado PROPUESTA sin «ronda 3»", m_text("State:\\nPROPUESTA (ronda 3). No aplicada", "State:\\nPROPUESTA. No aplicada")),
    ("G8", "ronda 3 lanzada en la cabecera", m_text("Ronda 3:\\nNOT LAUNCHED.", "Ronda 3:\\nLAUNCHED.")),
    ("G8", "§11: Architect con la ronda 3 AGREED", m_text("ronda 3: NOT LAUNCHED, hasta una disposición posterior", "ronda 3: AGREED, hasta una disposición posterior")),
    ("G8", "§11: Coordinator con veredicto", m_text("sin veredicto sobre la ronda 3", "veredicto AGREED sobre la ronda 3")),
    ("G8", "§11: Coordinator usa el dictamen de la ronda 2 como acuerdo", m_text("sin usar el dictamen como acuerdo; sin veredicto", "usando el dictamen como acuerdo; sin veredicto")),
    ("G8", "ronda 2 declarada AGREED", m_insert_ev("Ronda 2: AGREED.")),''')
rep('''    ("G8", "revisión de la ronda 2 declarada lanzada", m_insert_ev("La revisión formal de la ronda 2 lanzada el 2026-10-12.")),''',
    '''    ("G8", "revisión de la ronda 3 declarada lanzada", m_insert_ev("La revisión formal de la ronda 3 lanzada el 2026-10-13.")),''')
rep('''    ("G5", "fila de tabla en el delta", m_in_delta("A6-7", "invocación con una huella sin aceptar.»", "invocación con una huella sin aceptar.\\n> | fila | nueva |»")),''',
    '''    ("G5", "fila de tabla en el delta", m_in_delta("A6-7", "con evidencia que todavía no existe.»", "con evidencia que todavía no existe.\\n> | fila | nueva |»")),
    ("G5", "A6-3, regla 2, alterada (debe ser idéntica a la ronda 2)", m_in_delta("A6-3", "el árbol, el índice, las refs y el estado canónico del fixture no cambian", "el árbol y el índice no cambian")),
    ("G5", "A6-9 alterado (debe ser idéntico a la ronda 2)", m_in_delta("A6-9", "su resultado queda del lado del Coordinator", "su resultado queda del lado de la sesión")),
    ("G5", "sección de A6-1 alterada fuera del párrafo (Cubre)", m_text("Cubre: punto 1 del dictamen; R1, R2 y R5 de la ronda 1.", "Cubre: punto 1 del dictamen; R1, R2 y R5 de la ronda 1; R1 de la ronda 2.")),
    ("G5", "A6-5 sin la regla 4 de la ronda 2 (renumeración)", m_text("> 4. **salida no aceptada:**", ">    **salida no aceptada:**")),
    ("G5", "A6-5 vuelve a la tabla de la ronda 2 (R4 siempre en MATCH)", m_in_delta("A6-5", "R1, R2, R3, R9 y R10 están en `MATCH`", "R1, R2, R3, R4, R9 y R10 están en `MATCH`")),''')
rep('''    ("G10", "tabla: R4 admitida en NOT_APPLICABLE", m_in_delta("A6-5", "R1, R2, R3, R4, R9 y R10 están en `MATCH`", "R1, R2, R3, R9 y R10 están en `MATCH`")),''',
    '''    ("G10", "tabla: R4 en NOT_APPLICABLE sin la condición de `Verification`", m_in_delta("A6-5", "R4 está en `MATCH` si la solicitud lleva `Verification`, y en `NOT_APPLICABLE` solo si la regla de A-6 en D.6 la dejó fuera; y R5, R6 y R7", "y R4, R5, R6 y R7")),
    ("G10", "condición de R4 con Authority en lugar de FreeText", m_in_delta("A6-3", "`Trailer` y `FreeText` está en `not_run`", "`Trailer` y `Authority` está en `not_run`")),
    ("G10", "condición de R4 sin los tres archivos", m_in_delta("A6-3", "solo si esos tres archivos existen y son legibles y ninguna", "solo si ninguna")),
    ("G10", "aceptante: R4_CHECKS sin FreeText", m_acc('R4_CHECKS = ("Identity", "Scope", "CleanTree", "Trailer", "FreeText", "Authority")', 'R4_CHECKS = ("Identity", "Scope", "CleanTree", "Trailer", "Authority")')),
    ("G10", "aceptante: R4 sin not_run como distinto de pass", m_acc('r if r in ("pass", "fail", "not_run") else None', 'r if r in ("pass", "fail") else None')),
    ("G10", "aceptante: R3 con datos sin ejecuciones", m_acc("def check_r3(ctx):\\n    if not ctx.runs:\\n        return None", "def check_r3(ctx):\\n    if not ctx.runs:\\n        return (\\"MATCH\\", {})")),
    ("G10", "aceptante: R4 lee otra entrada además de la entrega", m_acc('read_pinned(v.get("Handoff"), "Verification.Handoff")', 'read_pinned(v.get("Worker"), "Verification.Handoff")')),
    ("G10", "A6-5, regla 7, sin «Ninguna está obligada»", m_in_delta("A6-5", "Ninguna está obligada a ejecutar el instrumento", "Toda invocación ejecuta el instrumento")),''')
rep('''    ("G10", "UNVERIFIED sin «registro incompleto»", m_in_delta("A6-5", "UNVERIFIED\\n>    con esa causa (registro incompleto)", "UNVERIFIED\\n>    con esa causa")),''',
    '''    ("G10", "UNVERIFIED sin «registro incompleto»", m_in_delta("A6-5", "FX-02 queda UNVERIFIED con esa causa (registro incompleto)", "FX-02 queda UNVERIFIED con esa causa")),''')
rep('''    ("G11", "ruta del paquete r2/ ausente del paquete disponible", lambda inp: inp.__setitem__("R2Local", {"a6r2-guards.py": {"Blob": "0" * 40, "Sha256": "0" * 64}})),''',
    '''    ("G11", "ruta del paquete r3/ ausente del paquete disponible", lambda inp: inp.__setitem__("R3Local", {"otro.json": {"Blob": "0" * 40, "Sha256": "0" * 64}})),''')
rep('''    ("G2", "archivo de r2/ borrado", m_status(EVID_R2 + "viejo.md", "D")),''',
    '''    ("G2", "archivo de r2/ borrado", m_status(EVID_R2 + "viejo.md", "D")),
    ("G2", "archivo de r3/ modificado en lugar de añadido", m_status(EVID_R3 + "preflight.md", "M")),
    ("G2", "base con la ronda 1 en lugar de la ronda 2", m_set(["BaseA6"], R1_BLOB)),''')

out = os.path.join(D, 'a6r3-guards.py')
open(out, 'w', encoding='utf-8', newline='\n').write(t)
diff = difflib.unified_diff(orig.splitlines(True), t.splitlines(True), 'r2/a6r2-guards.py', 'r3/a6r3-guards.py')
open(os.path.join(D, 'a6r3-guards.diff'), 'w', encoding='utf-8', newline='\n').writelines(diff)
compile(t, out, 'exec')
print('ok', len(n_done), 'substitutions;', len(t.splitlines()), 'lines')
