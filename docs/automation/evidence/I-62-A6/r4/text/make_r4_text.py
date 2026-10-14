# A-6 round 4 (decisiones §85): derives the round-4 text from the round-3 blob e62f27b8 by exact substitutions (each must match once).
# Normative change only in A6-3 rule 6 (b); the other changed sections are descriptive. Placeholders @@...@@ are filled later
# (make_r4_fills.py) with measured values; `run` guards reject any placeholder left.
import hashlib, os, sys
NL = chr(10)
D = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(D, 'I-62-A-6.r3.md')
OUT = os.path.join(D, 'I-62-A-6.r4.draft.md')
raw = open(SRC, 'rb').read()
assert hashlib.sha1(b'blob %d\x00' % len(raw) + raw).hexdigest() == 'e62f27b8b1329f1b9cfa69a707feb31d22bad1d1'
t = raw.decode('utf-8')
done = []


def rep(old, new, tag):
    global t
    n = t.count(old)
    assert n == 1, (tag, n)
    t = t.replace(old, new)
    done.append(tag)


def wrap_quote(par, width=149, prefix='>    '):
    """Greedy wrap of one paragraph into quoted lines; inline code spans are never split."""
    toks, cur, inside = [], '', False
    for ch in par:
        if ch == '`':
            inside = not inside
        if ch == ' ' and not inside:
            toks.append(cur)
            cur = ''
        else:
            cur += ch
    toks.append(cur)
    lines, line = [], prefix
    for w in toks:
        cand = (line + w) if line == prefix else (line + ' ' + w)
        if len(cand) > width and line != prefix:
            lines.append(line)
            line = prefix + w
        else:
            line = cand
    lines.append(line)
    for l in lines:
        assert len(l) <= width, l
    return lines


# ------------------------------------------------------------------ header
rep('Amendment: A-6 (candidata, ronda 3; preparada, no enviada; ninguna revisión de la ronda 3 lanzada)',
    'Amendment: A-6 (candidata, ronda 4; preparada, no enviada; ninguna revisión de la ronda 4 lanzada)', 'H-amend')
rep('''acreditación excepcional y sin usar el dictamen como acuerdo formal (decisiones §83, punto 1). Las rondas 1 y 2 y sus resultados se conservan sin
cambio en Git; la ronda 3 se corrige en este mismo archivo, con blob nuevo (§11)''',
    '''acreditación excepcional y sin usar el dictamen como acuerdo formal (decisiones §83, punto 1).
ronda 3 = blob e62f27b8b1329f1b9cfa69a707feb31d22bad1d1 (commit a128efef). Revisión formal R20261013T183701Z-9f80 (decisiones §84; evidencia §167):
auditor ACCREDITED; Architect CHANGES REQUIRED (A62-A6R3-R1; A62-A6R3-O1..O7). Decisiones §85, punto 1: A62-A6R3-R1 ACCEPTED REQUIRED y
A62-A6R2-R2 CORRECTED. Las rondas 1 a 3 y sus resultados se conservan sin cambio en Git; la ronda 4 se corrige en este mismo archivo, con blob
nuevo (§11)''', 'H-prev')
rep('''Classification: MATERIAL POST-FREEZE AMENDMENT, candidata (decisiones §76, punto 2; §79, punto 2; §83, punto 2). Las mismas nueve reglas, solo por
añadido, al final de V14 D.3 (A6-1, A6-2, A6-4, A6-7, A6-9 y A6-8), de V14 D.4 (A6-5 y A6-6) y de V14 D.6 (A6-3), corregidas en la ronda 2 (§3.16) y,
en la ronda 3, solo en A6-3, regla 6, A6-5 y A6-7 (§3.17). M-02, M-03, M-04, M-05 y M-08 sí; M-01, M-06 y M-07 no, reevaluadas después de las
correcciones (§5)''',
    '''Classification: MATERIAL POST-FREEZE AMENDMENT, candidata (decisiones §76, punto 2; §79, punto 2; §83, punto 2; §85, punto 2). Las mismas nueve
reglas, solo por añadido, al final de V14 D.3 (A6-1, A6-2, A6-4, A6-7, A6-9 y A6-8), de V14 D.4 (A6-5 y A6-6) y de V14 D.6 (A6-3), corregidas en la
ronda 2 (§3.16), en la ronda 3 solo en A6-3, regla 6, A6-5 y A6-7 (§3.17) y, en la ronda 4, solo en A6-3, regla 6 (b) (§3.18). M-02, M-03, M-04,
M-05 y M-08 sí; M-01, M-06 y M-07 no, reevaluadas después de las correcciones (§5)''', 'H-class')
rep('''§78), con el alcance literal de §7 (a), que las rondas 2 y 3 no amplían (decisiones §79, puntos 2 y 6; §83, punto 2)''',
    '''§78), con el alcance literal de §7 (a), que las rondas 2 a 4 no amplían (decisiones §79, puntos 2 y 6; §83, punto 2; §85, punto 7)''', 'H-auth')
rep('''R20261011T215735Z-c434. Ronda 3: decisiones §83 sobre la revisión R20261012T195511Z-e5c3''',
    '''R20261011T215735Z-c434. Ronda 3: decisiones §83 sobre la revisión R20261012T195511Z-e5c3. Ronda 4: decisiones §85 sobre la revisión
R20261013T183701Z-9f80''', 'H-origin')
rep('''ronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261011T215735Z-c434). Ronda 2: CHANGES REQUIRED, NOT_ACCREDITED (R20261012T195511Z-e5c3). Ronda 3:
NOT LAUNCHED. Ninguna revisión formal de la ronda 3 sin una disposición posterior del Coordinator (decisiones §83, punto 6); el preflight de la
ronda 3 (docs/automation/evidence/I-62-A6/r3/preflight.md) es preparación, no acuerdo''',
    '''ronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261011T215735Z-c434). Ronda 2: CHANGES REQUIRED, NOT_ACCREDITED (R20261012T195511Z-e5c3). Ronda 3:
CHANGES REQUIRED, ACCREDITED (R20261013T183701Z-9f80). Ronda 4: NOT LAUNCHED. Ninguna revisión formal de la ronda 4 sin una disposición posterior
del Coordinator (decisiones §85, punto 6); el preflight de la ronda 4 (docs/automation/evidence/I-62-A6/r4/preflight.md) es preparación, no
acuerdo''', 'H-review')
rep('''del acuerdo (§7). Las rondas 2 y 3 conservan los blobs del instrumento y del aceptante que fija (a) y no requieren otra decisión del Owner (§7). Los residuos declarados''',
    '''del acuerdo (§7). Las rondas 2 a 4 conservan los blobs del instrumento y del aceptante que fija (a) y no requieren otra decisión del Owner (§7). Los residuos declarados''',
    'H-owner')
rep('''PROPUESTA (ronda 3). No aplicada: ningún texto congelado, materializado ni de producción cambia por este registro. Sin invocación, sin medición y sin
revisión formal de la ronda 3.''',
    '''PROPUESTA (ronda 4). No aplicada: ningún texto congelado, materializado ni de producción cambia por este registro. Sin invocación, sin medición y sin
revisión formal de la ronda 4.''', 'H-state')

# ------------------------------------------------------------------ §1 and §2
rep('''Límites de esta preparación (decisiones §79, punto 6; §83, punto 7): sin Codex, sin otra sesión de Principal, sin FX-U2, sin B1, sin A-6 AGREED, sin
ampliar la excepción del Owner y sin cambios en producción ni en D.5.''',
    '''Límites de esta preparación (decisiones §79, punto 6; §83, punto 7; §85, punto 7): sin Codex, sin otra sesión de Principal, sin FX-U2, sin B1, sin
A-6 AGREED, sin cambios en el instrumento ni en el aceptante, sin ampliar la excepción del Owner y sin cambios en producción, en I-65 ni en D.5.''', 'S1-limits')
rep('''`AuthorityRevision` del contrato (docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md:44-45).

## 3. Delta''',
    '''`AuthorityRevision` del contrato (docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md:44-45).

Fuentes añadidas en la ronda 4:

Revisión de la ronda 3 (`docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/output.json`, ACCREDITED) y decisiones §85: su
campo `RequiredFindings` (A62-A6R3-R1) es la entrada vinculante de la ronda 4 (§3.18), y sus `OptionalFindings` (A62-A6R3-O1..O7) se tratan como
dispone decisiones §85, punto 4, sin cambios normativos fuera de A6-3, regla 6 (b). Los controles negativos con invocación usan el `Target` de la
verificación real (docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md:23) y copias de las entradas que conservan sus `RunId`
internos salvo el campo mutado (docs/automation/agent-execution/README.md:219-220).

## 3. Delta''', 'S2-sources')

# ------------------------------------------------------------------ §3.1
rep('''  exigían antes de la ronda (A62-A6R2-R2). La ronda 3 los corrige solo en A6-3, regla 6, A6-5 y A6-7, sin cambiar el instrumento ni el aceptante
  (§3.17).
''',
    '''  exigían antes de la ronda (A62-A6R2-R2). La ronda 3 los corrige solo en A6-3, regla 6, A6-5 y A6-7, sin cambiar el instrumento ni el aceptante
  (§3.17).

- **Ronda 4** (decisiones §85). La revisión de la ronda 3, la primera acreditada, encontró un defecto que impide el acuerdo: la tabla de derivación
  del cierre del aceptante no fija ningún resultado cuando falta la entrega (N4) o cuando un commit que nombra no existe (nc1), así que en esos
  controles la solicitud del aceptante no se podía construir sin un paso de juicio, y las sondas de la ronda 3 usaron otro cierre, el del extremo a
  extremo (A62-A6R3-R1). La ronda 4 lo corrige solo en A6-3, regla 6 (b), sin cambiar A6-9, su tabla, el instrumento ni el aceptante (§3.18).
''', 'S31-round4')

# ------------------------------------------------------------------ A6-3: Cubre and rule 6 (b) (the only normative change)
rep('Cubre: punto 3 del dictamen; R1, R4, O2 y O3 de la ronda 1; R1 y R2 de la ronda 2.',
    'Cubre: punto 3 del dictamen; R1, R4, O2 y O3 de la ronda 1; R1 y R2 de la ronda 2; R1 y O7 de la ronda 3.', 'A63-cubre')
old_b = ('''>    (b) `Repo` = el `-C`; `Tool` = el blob fijado, ejecutado desde el objeto del `-C`; `Interpreter` = el intérprete declarado; `Closure` = el
>    cierre derivado por la regla de A-6 sobre la independencia; `RelayFacts` = los hechos del relevo que cita la verificación, con el SHA-256
>    citado;
''')
pars = [p for p in open(os.path.join(D, 'r6b-r4.txt'), encoding='utf-8').read().split(NL) if p.strip()]
assert len(pars) == 7 and pars[0].startswith('(b) ') and pars[-1].startswith('La prueba previa'), [p[:12] for p in pars]
new_b = NL.join(l for p in pars for l in wrap_quote(p)) + NL
rep(old_b, new_b, 'A63-r6b')

# ------------------------------------------------------------------ §3.17: which closure each round-3 probe used
rep('''la de la ronda 3 da `ACCEPT_FACTS` con R4 no aplicable y R5, R6 y R7 en `MATCH`; con una ejecución con error, rechazo |''',
    '''la de la ronda 3 da `ACCEPT_FACTS` con R4 no aplicable y R5, R6 y R7 en `MATCH`; con una ejecución con error, rechazo (con el cierre del extremo a extremo; repetida con el cierre derivado en la ronda 4, §3.18) |''', 'S317-b')
rep('''con una ejecución sobre el `CurrentSha` inexistente, rechazo |''',
    '''con una ejecución sobre el `CurrentSha` inexistente, rechazo (con el cierre del extremo a extremo; repetida con el cierre derivado en la ronda 4, §3.18) |''', 'S317-c')
rep('''mutantes de la solicitud (cuatro solo los detecta la tabla, no el aceptante) y reconstrucción de referencia |''',
    '''mutantes de la solicitud (cuatro solo los detecta la tabla, no el aceptante) y reconstrucción de referencia; N4 y nc1, con el cierre del extremo a extremo y no con el de la tabla de A6-9 (A62-A6R3-R1; repetidos en la ronda 4, §3.18) |''', 'S317-d')

# ------------------------------------------------------------------ §3.18 (new)
S318 = '''
### 3.18 Corrección de la ronda 4 (decisiones §85)

Cambio normativo solo en A6-3, regla 6 (b) (decisiones §85, punto 2). A6-1, A6-2, A6-4, A6-5, A6-6, A6-7, A6-8 y A6-9, las reglas 1-5 y 7 de A6-3
y los apartados (a), (c) y (d) de su regla 6 conservan el texto de la ronda 3 byte a byte; A6-9, regla 4, y su tabla de derivación (§3.12) no
cambian; el instrumento (`9a829b0e…`) y el aceptante (`434ef1fc…`) conservan su blob; los presupuestos, la receta 16.4 y el alcance de la línea (a)
del Owner no cambian. Las demás secciones que cambian son descriptivas: la cabecera, §1, §2 (fuentes de la ronda 4), §3.1, la línea `Cubre:` de
A6-3, tres celdas de §3.17, esta sección, §4, §5, §6, §7, §8, §10, §11 y el anexo.

| Hallazgo de la ronda 3 | Corrección | Dónde | Prueba de la ronda 4 |
|---|---|---|---|
| R1: el `Closure` de N4 y nc1 no lo define la tabla de A6-9 | aplicación fija de la tabla cuando una entrada no da valor: si falta la delegación, la entrega o el contrato, se omiten todos sus términos; un campo ausente, `null` o vacío no aporta término; un valor de commit que no nombra un commit existente se omite; una primitiva de grafo con un extremo omitido o sin resultado da el conjunto vacío; ningún término omitido se sustituye; cada omisión se registra con su causa y el registro se custodia con la solicitud y el cierre; la prueba previa y la medición usan la misma regla con las entradas de su invocación | A6-3, regla 6 (b) | `r4/r1probes/`: @@R4.CASES@@; detalle abajo |
| O1: identificación de las ejecuciones por el blob | no bloqueante (decisiones §85, punto 4): sin cambio de alcance ni de texto; los dos límites siguen declarados en §3.17 | — | — |
| O2: la solicitud tiene que ser su construcción mecánica | cotejo mecánico, en la evidencia: la tabla de la ronda 3 (construcción de `Runs`, `Verification` y `NotApplicable`, y `Runs` frente al registro) más, para el cierre, su derivación de nuevo desde el vector custodiado y la comparación del registro de omisiones; una solicitud distinta queda sin aceptar en el banco | evidencia (`r4/r1probes/table_r4.py`); sin cambio normativo | `r4/r1probes/` |
| O3: `clausemap` en la cobertura del cierre de VERIFY | verificada: `clausemap` (estado en `CurrentSha`) queda dentro del cierre derivado en la verificación, en N4 y en nc1; con la variante I6 estricta de E queda fuera junto con `hashes` y `read-text` en `CurrentSha`, así que la disposición de A6-7, regla 7, tiene que cubrirla | evidencia; sin cambio normativo | `r4/r1probes/` (O3-STRICT) |
| O4: momento y efecto de la prueba previa | precisados abajo, en la evidencia, sin cambio normativo | evidencia | — |
| O5: R4 en los demás negativos | no bloqueante (decisiones §85, punto 4) | — | — |
| O6: redacción del S-04 de A6-5, regla 4 | no bloqueante (decisiones §85, punto 4); la revisión de la ronda 3 juzgó la regla coherente con 16.11 y 16.8 | — | — |
| O7: custodia de la medición | asegurada: la medición es una invocación VERIFY con el instrumento, y A6-3, regla 6, exige construir y custodiar su solicitud con el resultado; la regla 6 (b) añade que su cierre se deriva con la misma regla y que el registro de derivación se custodia con la solicitud y el cierre; el anexo (paso 4) fija además la custodia de su registro de sesión por SHA-256 antes de la disposición de elegibilidad | A6-3, regla 6 (b); anexo | — |

**N4 y nc1 con el cierre normativo** (decisiones §85, punto 3; `r4/r1probes/`, sin Codex). Escenario de la evidencia (d) de la ronda 2: la
verificación de T1 sobre la entrega simulada, con su cierre efectivo E (el de `r2/independence/`, con las copias de cada invocación) y el `Target`
de la verificación real. Cada sonda ejecuta de verdad el instrumento fijado por la forma declarada, reconstruye `Runs` de un registro con el formato
de Codex, construye la solicitud con el cierre derivado de la regla corregida y ejecuta el aceptante fijado sobre la misma copia del clon:

- **Derivación.** Con todas las entradas presentes, el cierre es idéntico al custodiado de la ronda 2 (7 commits, 89 `RevPaths`, 3 refs, 48 rutas y
  4 archivos); el registro solo anota `D.ChainRedSha` en `null`. En N4 (entrega ausente) se omiten H.BaseSha, H.CurrentSha, H.RedSha, la
  `RevList` y la entrada H de C6, con la causa «entrada ausente»: quedan 6 commits (sale RED, que solo aportaba la entrega; G sigue por
  I.Target.Commit), 75 `RevPaths`, 47 rutas y 3 archivos, todo contenido en el cierre de la verificación real. La entrega que E no fija, la que fija
  con los bytes ausentes, con otro blob, truncada, con una clave repetida, con BOM o como arreglo dan las mismas omisiones; los cierres difieren solo
  en la ruta de la entrega, que es una entrada de E cuando E la fija. En nc1 se omiten H.CurrentSha («no nombra un commit existente») y la `RevList`
  (primitiva vacía); el cierre conserva los 7 commits, porque RED y G entran por H.RedSha y por el `Target`. Un `CurrentSha` ausente, `null`, vacío,
  en mayúsculas, abreviado, un árbol, un blob o la ref `HEAD` dan el mismo cierre, salvo el SHA-256 de la entrada H de C6, con su propia causa;
  un `RedSha` ausente, `null` o vacío solo quita ese término, y RED sigue dentro por la `RevList`. Por generalidad, la delegación o el contrato
  ausentes, un `Target` inexistente y un `MergeBase` sin base común siguen la misma regla.
- **Cierre calculado de forma independiente.** Una segunda implementación, escrita aparte y solo desde el texto de la regla y de la tabla, da el
  mismo cierre y las mismas omisiones con la misma causa en
  @@R4.INDEP@@.
- **Aceptante fijado.** N4: con siete órdenes que no dependen de la entrega (`clausemap` incluida), `ACCEPT_FACTS` con R4 no aplicable y R9 en
  `MATCH`; con una ejecución sobre un commit inexistente, rechazo (R3, R5 y R9). nc1: V1 (las cinco comprobaciones evaluadas) acepta con R4 en
  `MATCH`, V2 y V3 (`not_run`) con R4 no aplicable, todas con R9 en `MATCH`; con una ejecución sobre el `CurrentSha` inexistente, rechazo. La
  verificación real acepta con R9 en `MATCH` en sus 12 órdenes.
- **Insumos añadidos o inventados y sustituciones.** Una lectura de una entrega válida puesta en lugar de la ausente (N4) o de un commit existente
  puesto en lugar del inexistente (nc1) queda fuera del cierre derivado: R9 en `MISMATCH` y rechazo. Con el cierre derivado de la sustitución, el
  aceptante solo da `ACCEPT_FACTS`, y únicamente el cotejo con la derivación lo rechaza. Un commit, una ruta, un `RevPath`, una ref o un archivo de
  más, un commit de menos, un registro vacío, el cierre del extremo a extremo de la ronda 3 o la entrega completada con el `Target` quedan
  rechazados por el cotejo. En nc1, sustituir el `CurrentSha` inexistente por G no cambia el cierre en este escenario: solo el registro de derivación
  lo muestra, y el cotejo lo rechaza por el registro (regla 6 (b) (v)).

**Relación con A6-9, regla 4** (decisiones §85, punto 8). La corrección no edita la regla 4 ni la tabla. La tabla define el cierre cuando sus
entradas dan valor; A6-3, regla 6 (b), fija cómo se aplica cuando no lo dan, y solo quita términos: el cierre de N4 está contenido en el de la
verificación real, nunca admite una lectura que la tabla no admita y no es una segunda definición de las lecturas permitidas. Con todas las
entradas presentes, el resultado no cambia. Ninguna contradicción exige modificar A6-9. La línea `Entradas:` de la tabla presupone que E fija la
entrega, y la regla (i) define el caso en que no la fija. La identificación de D, H y K entre las entradas de E la da el kit, por las rutas de entrada
de la invocación, como en la evidencia (d) de la ronda 2.

**Momento y efecto de la prueba previa** (O4; lectura que aplicará la supervisión, no forma parte del delta). La prueba previa de A6-5, regla 6, se
hace después de la entrega y del cálculo del cierre efectivo de las invocaciones VERIFY, con el cierre derivado por A6-3, regla 6 (b), antes del
primer lanzamiento VERIFY con el instrumento y fuera de toda cesión. Sin la disposición sobre las lecturas normativas o con la prueba fallida, el
aceptante no se usa tal como está, ninguna salida con ejecuciones del instrumento podría quedar aceptada (A6-5, regla 4), y no se lanza ninguna
invocación VERIFY cuyo cierre admita el instrumento.

**Riesgos residuales G1-G6 en la ronda 4.** Ninguno cambia: la corrección solo fija la construcción de una entrada de la solicitud.

**Kit de la revisión de la ronda 4** (decisiones §85, punto 6; no forma parte del delta). Se deriva del kit acreditado de la ronda 3, con el mismo
lanzador, la misma orden congelada `python -B kit/launch.py`, la misma compuerta de transporte y la misma comprobación previa de registro. Su alcance
se limita a A62-A6R3-R1 y a sus consecuencias directas. Se congela y se custodia sin lanzar, y su compuerta de autorización exige una disposición
posterior del Coordinator.
'''
rep('''lleva una comprobación previa de registro que no ejecuta el modelo ni escribe un `run.json` de la revisión (`r3/launchfix/`: orden congelada `python -B kit/launch.py` comprobada por el propio lanzador; registro escrito siempre que se lanzó el proceso; comprobación previa de registro con un proceso sustituto; 28 de 28 pruebas).
''',
    '''lleva una comprobación previa de registro que no ejecuta el modelo ni escribe un `run.json` de la revisión (`r3/launchfix/`: orden congelada `python -B kit/launch.py` comprobada por el propio lanzador; registro escrito siempre que se lanzó el proceso; comprobación previa de registro con un proceso sustituto; 28 de 28 pruebas).
''' + S318, 'S318')

# ------------------------------------------------------------------ §4
rep('''| Invocación VERIFY sin ejecuciones del instrumento | — |''',
    '''| Cierre del aceptante con una entrada que falta o un commit inexistente (N4, nc1) | — | la tabla de derivación se aplica sin esos términos: entrada ausente, campo ausente, `null` o vacío y valor que no nombra un commit existente, omitidos con su causa y sin sustituirlos; registro custodiado con la solicitud y el cierre; la misma regla en la prueba previa y en la medición (A6-3, regla 6 (b)) |
| Invocación VERIFY sin ejecuciones del instrumento | — |''', 'S4-row')

# ------------------------------------------------------------------ §5
rep('''Reevaluada después de las correcciones de la ronda 3 (decisiones §83, punto 2); no se hereda de las rondas anteriores ni del dictamen. La columna
Dictamen es el valor literal de su campo `Materiality[].Activated`, y las columnas Ronda 1 y Ronda 2, la evaluación del revisor de cada ronda
(`MaterialityConfirmation`).''',
    '''Reevaluada después de la corrección de la ronda 4 (decisiones §83, punto 2; §85, punto 2); no se hereda de las rondas anteriores ni del dictamen.
La columna Dictamen es el valor literal de su campo `Materiality[].Activated`, y las columnas Ronda 1 y Ronda 2, la evaluación del revisor de cada
ronda (`MaterialityConfirmation`); la revisión acotada de la ronda 3 no evaluó la materialidad.''', 'S5-intro')
rep('''el ámbito de A6-5, regla 7, remite a 16.9 sin cambio las invocaciones sin ejecuciones y las comprobaciones en `not_run` |''',
    '''el ámbito de A6-5, regla 7, remite a 16.9 sin cambio las invocaciones sin ejecuciones y las comprobaciones en `not_run`. La ronda 4 tampoco: A6-3, regla 6 (b), solo fija cómo se aplica la tabla de A6-9 cuando falta una entrada |''', 'S5-M01')
rep('''la condición de `Verification` en la solicitud y la disposición sobre el cierre de VERIFY (A6-3, regla 6; A6-7, regla 7) |''',
    '''la condición de `Verification` en la solicitud y la disposición sobre el cierre de VERIFY (A6-3, regla 6; A6-7, regla 7); en la ronda 4, la aplicación de la tabla de derivación cuando falta una entrada o un commit, con su registro custodiado (A6-3, regla 6 (b)) |''', 'S5-M05')

# ------------------------------------------------------------------ §6.1
rep('''- **Diff permitido del commit que publique la ronda 3** (sobre la HEAD de la rama en ese momento): solo `docs/initiatives/I-62-A-6.md` (modificado:
  blob de la ronda 2 → blob de la ronda 3) y archivos nuevos de `docs/automation/evidence/I-62-A6/r3/` (guardas, autoprueba, pruebas del aceptante,
  evidencia estática de R2, procedimiento de lanzamiento, resultados y preflight). Los archivos de las rondas 1 y 2 en
  `docs/automation/evidence/I-62-A6/` no cambian. El registro en decisiones, en la evidencia, en el estado o en el README de F6 va en un commit
  aparte.''',
    '''- **Diff permitido del commit que publique la ronda 4** (sobre la HEAD de la rama en ese momento): solo `docs/initiatives/I-62-A-6.md` (modificado:
  blob de la ronda 3 → blob de la ronda 4) y archivos nuevos de `docs/automation/evidence/I-62-A6/r4/` (guardas, autoprueba, derivación del cierre,
  implementación independiente, pruebas del aceptante, resultados y preflight). Los archivos de las rondas 1 a 3 en
  `docs/automation/evidence/I-62-A6/` no cambian. El registro en decisiones, en la evidencia, en el estado o en el README de F6 va en un commit
  aparte.''', 'S61-diff')
rep('''**Superficies prohibidas** (ninguna cambia en el commit que publique la ronda 3;''',
    '''**Superficies prohibidas** (ninguna cambia en el commit que publique la ronda 4;''', 'S61-forb1')
rep('''`docs/automation/evidence/I-62-architect-A-6r2/` (la revisión de la ronda 2) y todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r3/` (la evidencia de las rondas 1 y 2).''',
    '''`docs/automation/evidence/I-62-architect-A-6r2/` (la revisión de la ronda 2), `docs/automation/evidence/I-62-architect-A-6r3/` (la revisión de la ronda 3 y su kit) y todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r4/` (la evidencia de las rondas 1 a 3).''', 'S61-forb2')
rep('''  (`output.json` `fc357d09…`), árbol `I-62-A6/` de la ronda 1 `7f504a17…` fuera de `r2/` y árbol `I-62-A6/r2/` `411949b0…`.''',
    '''  (`output.json` `fc357d09…`), árbol de la revisión de la ronda 3 `I-62-architect-A-6r3/` `f4442fdf…` (`output.json` `691bbc8f…`), árbol
  `I-62-A6/` de la ronda 1 `7f504a17…` fuera de `r2/`, árbol `I-62-A6/r2/` `411949b0…` y árbol `I-62-A6/r3/` `a4e93edb…`.''', 'S61-blobs')

# ------------------------------------------------------------------ §6.2
rep('''16.9 sin cambio, como antes de A-6.
''',
    '''16.9 sin cambio, como antes de A-6.

Ronda 4: la corrección de A6-3, regla 6 (b), no cambia ninguna de estas interacciones: solo fija cómo la supervisión deriva el cierre del aceptante
cuando falta una entrada de la tabla o un commit que nombra, sin consumir ninguna invocación ni presupuesto de A-1..A-5.
''', 'S62')

# ------------------------------------------------------------------ §6.3
rep('''`docs/automation/evidence/I-62-A6/r3/a6r3-guards.py` (solo biblioteca estándar; modos `self-test`, `preview` y `run`), adaptadas de las de la
ronda 2 (`r2/a6r2-guards.py`), que no cambian:''',
    '''`docs/automation/evidence/I-62-A6/r4/a6r4-guards.py` (solo biblioteca estándar; modos `self-test`, `preview` y `run`), adaptadas de las de la
ronda 3 (`r3/a6r3-guards.py`), que no cambian:''', 'S63-intro')
rep('''- **G2:** rutas del commit: solo `I-62-A-6.md` modificado y archivos añadidos bajo `r3/`.
- **G3:** superficies prohibidas, objetos congelados y C-20b, incluidas las revisiones y la evidencia de las rondas 1 y 2.
- **G4:** correspondencia con el dictamen y con los hallazgos de las rondas 1 (§3.16) y 2 (§3.17), coherente con la línea `Cubre:` de cada delta.
- **G5:** pureza del delta; A6-1, A6-2, A6-4, A6-6, A6-8, A6-9 y las reglas de A6-3 distintas de la 6, byte a byte como en la ronda 2.''',
    '''- **G2:** rutas del commit: solo `I-62-A-6.md` modificado y archivos añadidos bajo `r4/`.
- **G3:** superficies prohibidas, objetos congelados y C-20b, incluidas las revisiones y la evidencia de las rondas 1 a 3.
- **G4:** correspondencia con el dictamen y con los hallazgos de las rondas 1 (§3.16), 2 (§3.17) y 3 (§3.18), coherente con la línea `Cubre:` de
  cada delta.
- **G5:** pureza del delta; A6-1, A6-2, A6-4 a A6-9, las reglas de A6-3 distintas de la 6 y los apartados (a), (c) y (d) de la regla 6, byte a byte
  como en la ronda 3; la regla 6 (b), con sus frases exigidas.''', 'S63-g')
rep('''La autoprueba mata todos sus mutantes sobre este texto, y los controles negativos de extremo a extremo dan el resultado esperado. Los recuentos y
los resultados de `preview` y de `run` sobre el commit que publique la ronda 3 están en `r3/` y en `r3/preflight.md`. Ninguna revisión formal si no
pasan.''',
    '''La autoprueba mata todos sus mutantes sobre este texto, y los controles negativos de extremo a extremo dan el resultado esperado. Los recuentos y
los resultados de `preview` y de `run` sobre el commit que publique la ronda 4 están en `r4/` y en `r4/preflight.md`. Ninguna revisión formal si no
pasan.''', 'S63-end')

# ------------------------------------------------------------------ §7
rep('''autorizaría (b), sin ampliarla.
''',
    '''autorizaría (b), sin ampliarla.

**Ronda 4.** La corrección de A6-3, regla 6 (b), tampoco cambia el instrumento ni el aceptante: (a) los sigue cubriendo con el mismo alcance y no
hace falta ninguna línea nueva del Owner. La medición que autorizaría (b) deriva su cierre con la misma regla, sin ampliarla.
''', 'S7')

# ------------------------------------------------------------------ §8
rep('## 8. Disposición de las rondas 1 y 2 y preguntas de la ronda 3 (no forman parte del delta)',
    '## 8. Disposición de las rondas 1 a 3 y preguntas de la ronda 4 (no forman parte del delta)', 'S8-title')
q3_old = t[t.index('**Preguntas de la ronda 3:**'):t.index('## 9. Separabilidad')]
rep(q3_old, '''**Preguntas de la ronda 3**, según la revisión R20261013T183701Z-9f80 y decisiones §85:

| Id | Estado en la ronda 4 |
|---|---|
| Q-A6R3-01 | aceptable tal como se acota, según la revisión; sin cambio |
| Q-A6R3-02 | compatible con A6-6, regla 3, y con A6-8, según la revisión; sin cambio |
| Q-A6R3-03 | basta como precondición con fallo cerrado, según la revisión; la orden declarada (A6-1) no cambia |
| Q-A6R3-04 | coherente con 16.11 y con 16.8, según la revisión; la precisión de redacción (A62-A6R3-O6) queda no bloqueante (decisiones §85, punto 4) |

**Preguntas de la ronda 4:**

| Id | Pregunta |
|---|---|
| Q-A6R4-01 | A6-3, regla 6 (b), fija cómo se aplica la tabla de A6-9 cuando falta una entrada o un commit, sin editar A6-9, regla 4, ni su tabla (decisiones §85, punto 2). ¿Basta esa ubicación, o la regla 4 tendría que remitir a ella? |
| Q-A6R4-02 | La regla (i) trata igual la entrega que E no fija y la que E fija con los bytes ausentes o ilegibles; los dos cierres difieren solo en la ruta de la entrega dentro de C4, sin admitir ninguna lectura de la entrega. ¿Es aceptable esa diferencia? |
| Q-A6R4-03 | En nc1, sustituir el `CurrentSha` inexistente por G no cambia el cierre en este escenario, porque G y RED entran por el `Target` y por H.RedSha; solo el registro de derivación muestra la sustitución. ¿Basta el cotejo del registro que custodia la regla (v)? |
| Q-A6R4-04 | Las reglas (i)-(iii) cubren también una delegación o un contrato ausentes, aunque ningún control de FX-02 los deja ausentes. ¿Es aceptable esa generalidad dentro de la corrección de A62-A6R3-R1? |

''', 'S8-q')

# ------------------------------------------------------------------ §10
rep('- Decisiones §73-§83 (DEC L1479 en adelante); evidencia §140-§163.',
    '- Decisiones §73-§85 (DEC L1479 en adelante); evidencia §140-§168.', 'S10-dec')
rep('''  - procedimiento de lanzamiento corregido para el kit siguiente y su comprobación previa de registro (`launchfix/`);
  - guardas, negativos y preflight.''',
    '''  - procedimiento de lanzamiento corregido para el kit siguiente y su comprobación previa de registro (`launchfix/`);
  - guardas, negativos y preflight.
- Revisión de la ronda 3: `docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/` (kit, `output.json`, `audit.json`,
  `README.md`).
- Ronda 4: `docs/automation/evidence/I-62-A6/r4/`:
  - derivación del cierre con la regla corregida, implementación independiente, sustituciones, insumos añadidos o inventados y pruebas del
    aceptante fijado con N4, nc1 y la verificación real (`r1probes/`);
  - guardas, negativos y preflight.''', 'S10-r4')

# ------------------------------------------------------------------ §11
rep('''ronda 2: CHANGES REQUIRED (R20261012T195511Z-e5c3), NOT_ACCREDITED; ronda 3: NOT LAUNCHED, hasta una disposición posterior del Coordinator | decisiones §79, §82 y §83 |''',
    '''ronda 2: CHANGES REQUIRED (R20261012T195511Z-e5c3), NOT_ACCREDITED; ronda 3: CHANGES REQUIRED (R20261013T183701Z-9f80), ACCREDITED; ronda 4: NOT LAUNCHED, hasta una disposición posterior del Coordinator | decisiones §79 y §82-§85 |''', 'S11-arch')
rep('''sin usar el dictamen como acuerdo; sin veredicto sobre la ronda 3 | decisiones §79 y §83 |''',
    '''sin usar el dictamen como acuerdo; ronda 3: A62-A6R3-R1 aceptado como REQUIRED y A62-A6R2-R2 CORRECTED; sin veredicto sobre la ronda 4 | decisiones §79, §83 y §85 |''', 'S11-coord')
rep('''commit `7b7aba46`, y la ronda 2, el blob `86da0ae7…` del commit `ae9fe0e9`. Una vez acordada, solo la corrige, revoca o sustituye la A-n siguiente
(LIFECYCLE §6).''',
    '''commit `7b7aba46`; la ronda 2, el blob `86da0ae7…` del commit `ae9fe0e9`; y la ronda 3, el blob `e62f27b8…` del commit `a128efef`. Una vez acordada,
solo la corrige, revoca o sustituye la A-n siguiente (LIFECYCLE §6).''', 'S11-hist')

# ------------------------------------------------------------------ annex
rep('''1. **Revisión de la ronda 3.** Con una disposición del Coordinator que la autorice (decisiones §83, punto 6): una única revisión formal (`claude-cli`,
   dentro de CLAUDE-CLI-I62), con acceso directo a todas las fuentes que cite, lanzada con la orden congelada exacta y con la comprobación previa de
   registro (§3.17).
2. **Acuerdo.** Architect y Coordinator, sobre el blob exacto de A-6; las guardas de la ronda 3 en modo `run` sobre el commit que la publique.''',
    '''1. **Revisión de la ronda 4.** Con una disposición del Coordinator que la autorice (decisiones §85, punto 6): una única revisión formal (`claude-cli`,
   dentro de CLAUDE-CLI-I62), acotada a A62-A6R3-R1 y a sus consecuencias directas, con acceso directo a todas las fuentes que cite, lanzada con la
   orden congelada exacta y con la comprobación previa de registro (§3.17, §3.18).
2. **Acuerdo.** Architect y Coordinator, sobre el blob exacto de A-6; las guardas de la ronda 4 en modo `run` sobre el commit que la publique.''', 'AX-12')
rep('''   invocación de medición, sin reintento, con la reconstrucción de `Runs` y la corrida del aceptante sobre su registro real (A6-7, regla 1). Si
   falla, FX-02 sigue UNVERIFIED.''',
    '''   invocación de medición, sin reintento, con la reconstrucción de `Runs` y la corrida del aceptante sobre su registro real (A6-7, regla 1), con el
   cierre derivado por A6-3, regla 6 (b). Su registro de sesión, la solicitud, el cierre con su registro de derivación y el resultado del aceptante
   se custodian con su SHA-256 antes de la disposición de elegibilidad (O7 de la ronda 3). Si falla, FX-02 sigue UNVERIFIED.''', 'AX-4')

open(OUT, 'w', encoding='utf-8', newline=NL).write(t)
b = t.encode('utf-8')
print('ok', len(done), 'replacements;', len(t.split(NL)), 'lines; blob', hashlib.sha1(b'blob %d\x00' % len(b) + b).hexdigest())
