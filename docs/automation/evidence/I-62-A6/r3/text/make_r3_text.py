# A-6 round 3 (decisiones §83): derives the round-3 text from the round-2 blob 86da0ae7 by exact substitutions (each must match once).
# Normative changes only in A6-3 rule 6, A6-5 and A6-7; the other changed sections are descriptive. Placeholders @@...@@ are filled later
# (make_r3_fills.py) with measured values; `run` guards reject any placeholder left.
import hashlib, os, sys
NL = chr(10)
D = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(D, 'I-62-A-6.r2.md')
OUT = os.path.join(D, 'I-62-A-6.r3.draft.md')
raw = open(SRC, 'rb').read()
assert hashlib.sha1(b'blob %d\x00' % len(raw) + raw).hexdigest() == '86da0ae770e683a647f2724860a4bd890061154d'
t = raw.decode('utf-8')
done = []


def rep(old, new, tag):
    global t
    n = t.count(old)
    assert n == 1, (tag, n)
    t = t.replace(old, new)
    done.append(tag)


# ------------------------------------------------------------------ header
rep('Amendment: A-6 (candidata, ronda 2; preparada, no enviada; ninguna revisión de la ronda 2 lanzada)',
    'Amendment: A-6 (candidata, ronda 3; preparada, no enviada; ninguna revisión de la ronda 3 lanzada)', 'H-amend')
rep('''§153): Architect CHANGES REQUIRED (A62-A6-R1..R5 y A62-A6-O1..O3); auditor NOT_ACCREDITED, sin acreditación excepcional (decisiones §79, punto 1). La
ronda 1 y su resultado se conservan sin cambio en Git; la ronda 2 se corrige en este mismo archivo, con blob nuevo (§11)''',
    '''§153): Architect CHANGES REQUIRED (A62-A6-R1..R5 y A62-A6-O1..O3); auditor NOT_ACCREDITED, sin acreditación excepcional (decisiones §79, punto 1).
ronda 2 = blob 86da0ae770e683a647f2724860a4bd890061154d (commit ae9fe0e9). Revisión formal R20261012T195511Z-e5c3 (decisiones §82; evidencia §161):
Architect CHANGES REQUIRED (A62-A6R2-R1 y A62-A6R2-R2; A62-A6R2-O1..O5); auditor NOT_ACCREDITED, con un único motivo («falta run.json»), sin
acreditación excepcional y sin usar el dictamen como acuerdo formal (decisiones §83, punto 1). Las rondas 1 y 2 y sus resultados se conservan sin
cambio en Git; la ronda 3 se corrige en este mismo archivo, con blob nuevo (§11)''', 'H-prev')
rep('''Classification: MATERIAL POST-FREEZE AMENDMENT, candidata (decisiones §76, punto 2; §79, punto 2). Las mismas nueve reglas, solo por añadido, al final
de V14 D.3 (A6-1, A6-2, A6-4, A6-7, A6-9 y A6-8), de V14 D.4 (A6-5 y A6-6) y de V14 D.6 (A6-3), corregidas en la ronda 2 (§3.16). M-02, M-03, M-04,
M-05 y M-08 sí; M-01, M-06 y M-07 no, reevaluadas después de las correcciones (§5)''',
    '''Classification: MATERIAL POST-FREEZE AMENDMENT, candidata (decisiones §76, punto 2; §79, punto 2; §83, punto 2). Las mismas nueve reglas, solo por
añadido, al final de V14 D.3 (A6-1, A6-2, A6-4, A6-7, A6-9 y A6-8), de V14 D.4 (A6-5 y A6-6) y de V14 D.6 (A6-3), corregidas en la ronda 2 (§3.16) y,
en la ronda 3, solo en A6-3, regla 6, A6-5 y A6-7 (§3.17). M-02, M-03, M-04, M-05 y M-08 sí; M-01, M-06 y M-07 no, reevaluadas después de las
correcciones (§5)''', 'H-class')
rep('''§78), con el alcance literal de §7 (a), que la ronda 2 no amplía (decisiones §79, puntos 2 y 6)''',
    '''§78), con el alcance literal de §7 (a), que las rondas 2 y 3 no amplían (decisiones §79, puntos 2 y 6; §83, punto 2)''', 'H-auth')
rep('''output.json, blob 96c3efbc…); paquete B-min del commit 9356edf1 (árbol 3caa0dfa…; evidencia §146). Ronda 2: decisiones §79 sobre la revisión
R20261011T215735Z-c434''',
    '''output.json, blob 96c3efbc…); paquete B-min del commit 9356edf1 (árbol 3caa0dfa…; evidencia §146). Ronda 2: decisiones §79 sobre la revisión
R20261011T215735Z-c434. Ronda 3: decisiones §83 sobre la revisión R20261012T195511Z-e5c3''', 'H-origin')
rep('''Architect review:
ronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261011T215735Z-c434). Ronda 2: NOT LAUNCHED. Ninguna revisión formal nueva sin una disposición
posterior del Coordinator (decisiones §79, puntos 5 y 7); el preflight de la ronda 2 (docs/automation/evidence/I-62-A6/r2/preflight.md) es
preparación, no acuerdo''',
    '''Architect review:
ronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261011T215735Z-c434). Ronda 2: CHANGES REQUIRED, NOT_ACCREDITED (R20261012T195511Z-e5c3). Ronda 3:
NOT LAUNCHED. Ninguna revisión formal de la ronda 3 sin una disposición posterior del Coordinator (decisiones §83, punto 6); el preflight de la
ronda 3 (docs/automation/evidence/I-62-A6/r3/preflight.md) es preparación, no acuerdo''', 'H-review')
rep('''del acuerdo (§7). La ronda 2 conserva los blobs del instrumento y del aceptante que fija (a) y no requiere otra decisión del Owner (§7). Los residuos declarados''',
    '''del acuerdo (§7). Las rondas 2 y 3 conservan los blobs del instrumento y del aceptante que fija (a) y no requieren otra decisión del Owner (§7). Los residuos declarados''',
    'H-owner')
rep('''PROPUESTA (ronda 2). No aplicada: ningún texto congelado, materializado ni de producción cambia por este registro. Sin invocación, sin medición y sin
revisión formal de la ronda 2.''',
    '''PROPUESTA (ronda 3). No aplicada: ningún texto congelado, materializado ni de producción cambia por este registro. Sin invocación, sin medición y sin
revisión formal de la ronda 3.''', 'H-state')
rep('''Límites de esta preparación (decisiones §79, punto 6): sin Codex, sin otra sesión de Principal, sin FX-U2, sin A-6 AGREED, sin ampliar la excepción
del Owner y sin cambios en producción ni en D.5.''',
    '''Límites de esta preparación (decisiones §79, punto 6; §83, punto 7): sin Codex, sin otra sesión de Principal, sin FX-U2, sin B1, sin A-6 AGREED, sin
ampliar la excepción del Owner y sin cambios en producción ni en D.5.''', 'S1-limits')

# ------------------------------------------------------------------ §2: round-3 sources
rep('''`RequiredFindings` (A62-A6-R1..R5), `OptionalFindings` (A62-A6-O1..O3) y `MaterialityConfirmation` (M-01) son entradas vinculantes de la ronda 2
(§3.16).
''',
    '''`RequiredFindings` (A62-A6-R1..R5), `OptionalFindings` (A62-A6-O1..O3) y `MaterialityConfirmation` (M-01) son entradas vinculantes de la ronda 2
(§3.16).

Fuentes añadidas en la ronda 3 (literales en `1652dd61`; blobs sin cambio en la base de la ronda 3):

RAE L228-L228, literal (§10, control nc1; sin cambio):

> - **nc1:** entrega con `CurrentSha` inexistente → `Identity`; `EXECUTION_BLOCKED/STOP`.

Revisión de la ronda 2 (`docs/automation/evidence/I-62-architect-A-6r2/R20261012T195511Z-e5c3/output.json`) y decisiones §83: sus campos
`RequiredFindings` (A62-A6R2-R1 y A62-A6R2-R2) y `OptionalFindings` (A62-A6R2-O1..O5) son entradas vinculantes de la ronda 3 (§3.17), sin que el
dictamen cuente como acuerdo formal (decisiones §83, punto 1). La cobertura de `CanonicalInputs` del cierre de VERIFY la fija el kit de FX-02, en la
`AuthorityRevision` del contrato (docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md:44-45).
''', 'S2-sources')

# ------------------------------------------------------------------ §3.1
rep('''  el instrumento ni el aceptante (§3.16).
''',
    '''  el instrumento ni el aceptante (§3.16).

- **Ronda 3** (decisiones §83). La revisión de la ronda 2 encontró dos defectos que impiden el acuerdo. La regla de aceptación de A6-5 no era total
  sobre las invocaciones VERIFY de su alcance: N4, una invocación VERIFY sin ninguna ejecución del instrumento y las comprobaciones que el Controller
  deja en `not_run` después del primer fallo nunca podían quedar aceptadas (A62-A6R2-R1). Y la reconstrucción de `Runs` desde el registro real de
  sesión, de la que depende toda aceptación, y la cobertura de las lecturas legítimas del instrumento por el cierre de VERIFY no se demostraban ni se
  exigían antes de la ronda (A62-A6R2-R2). La ronda 3 los corrige solo en A6-3, regla 6, A6-5 y A6-7, sin cambiar el instrumento ni el aceptante
  (§3.17).
''', 'S31-r3')

# ------------------------------------------------------------------ A6-3: Cubre + rule 6
rep('Cubre: punto 3 del dictamen; R1, R4, O2 y O3 de la ronda 1.',
    'Cubre: punto 3 del dictamen; R1, R4, O2 y O3 de la ronda 1; R1 y R2 de la ronda 2.', 'A63-cubre')
rep('''> 6. la supervisión construye la solicitud del aceptante de forma mecánica: `Repo` = el `-C`; `Tool` = el blob fijado, ejecutado desde el objeto
>    del `-C`; `Interpreter` = el intérprete declarado; `Runs` = toda ejecución del instrumento que conste en el registro de sesión del adapter de
>    esa invocación, con su argv y los bytes de su salida tal como constan; `Verification` = la salida `controller-verification/v2` de la
>    invocación, con la delegación y la entrega que cita, la ruta ignorada del paquete, la revisión de los términos de 16.10 y el `MainSha` de la
>    invocación; `Closure` = el cierre derivado por la regla de A-6 sobre la independencia; `RelayFacts` = los hechos del relevo que cita la
>    verificación, con el SHA-256 citado; y `NotApplicable`, solo por la tabla de la regla de A-6 sobre el resultado del aceptante. El aceptante
>    vuelve a ejecutar el instrumento fijado y calcula sus propios hechos, de modo que la interpretación del Controller nunca es autoridad para él;''',
    '''> 6. la supervisión construye la solicitud del aceptante de forma mecánica, solo para una invocación VERIFY en que corrió el instrumento (con
>    ejecuciones, según la regla de A-6 sobre el resultado del aceptante), y la custodia con el resultado:
>    (a) `Runs` = toda ejecución del instrumento que conste en el registro de sesión del adapter de esa invocación, reconstruida de ese registro con
>    la regla de reconstrucción que demostró la medición de la regla de A-6 sobre la elegibilidad y que nombra la disposición del Coordinator. El
>    registro tiene que estar completo (inicio y fin del turno, y el fin de cada orden iniciada) y ser legible. Es ejecución del instrumento toda
>    orden registrada que contiene el blob fijado. Su `Argv` sale del texto registrado de la orden, que tiene exactamente la forma de la orden
>    declarada, con el Git y el intérprete declarados por su ruta y con `-I`; su `Envelope` son los bytes de su salida tal como constan, que
>    contienen exactamente un sobre del instrumento, en su serialización canónica y terminado en LF, sin alterar ningún byte; y su código de salida
>    consta. Si una ejecución no se puede reconstruir así (otra forma, salida truncada o sin sobre, más de un sobre, otro fin de línea, otra salida
>    mezclada dentro del sobre, código ausente), o si el registro está incompleto o es ilegible, no hay solicitud y la salida queda sin aceptar;
>    (b) `Repo` = el `-C`; `Tool` = el blob fijado, ejecutado desde el objeto del `-C`; `Interpreter` = el intérprete declarado; `Closure` = el
>    cierre derivado por la regla de A-6 sobre la independencia; `RelayFacts` = los hechos del relevo que cita la verificación, con el SHA-256
>    citado;
>    (c) `Verification` = la salida `controller-verification/v2` de la invocación, con la delegación y la entrega que cita, la ruta ignorada del
>    paquete, la revisión de los términos de 16.10 y el `MainSha` de la invocación, solo si esos tres archivos existen y son legibles y ninguna de
>    las comprobaciones `Identity`, `Scope`, `CleanTree`, `Trailer` y `FreeText` está en `not_run` en esa salida. En cualquier otro caso, entre
>    ellos N4 (entrega ausente), la solicitud no lleva `Verification` y declara R4 no aplicable, con la causa;
>    (d) `NotApplicable`, solo por la tabla de la regla de A-6 sobre el resultado del aceptante.
>    El aceptante vuelve a ejecutar cada `Argv` con el instrumento fijado, compara con el sobre registrado sus argumentos, entradas, llamadas a Git,
>    hechos y error (R3) y calcula sus propios hechos, de modo que la interpretación del Controller nunca es autoridad para él. La regla 5 se
>    refiere a las invocaciones en que corrió el instrumento; en una invocación VERIFY sin ejecuciones no hay solicitud ni corrida del aceptante: la
>    supervisión registra el recuento cero con el SHA-256 del registro de sesión, y la retención termina con ese registro;''', 'A63-r6')

# ------------------------------------------------------------------ A6-5
rep('Cubre: punto 5 del dictamen; R2, R3 (prueba previa de los hechos) y O2 de la ronda 1.',
    'Cubre: punto 5 del dictamen; R2, R3 (prueba previa de los hechos) y O2 de la ronda 1; R1, R2 y O5 de la ronda 2.', 'A65-cubre')
rep('''> «**Fallos cerrados del instrumento y resultado del aceptante en FX-02 (A-6).** Solo para FX-02 en F6, en las invocaciones VERIFY con el
> instrumento de la regla de A-6 en D.3:''',
    '''> «**Fallos cerrados del instrumento y resultado del aceptante en FX-02 (A-6).** Solo para FX-02 en F6, en las invocaciones VERIFY con el
> instrumento de la regla de A-6 en D.3, con el ámbito de la regla 7:''', 'A65-lead')
rep('''>    **aceptada** solo si el código es 0, la `Decision` es `ACCEPT_FACTS` y, además: R1, R2, R3, R4, R9 y R10 están en `MATCH`; R8 está en `MANUAL`;
>    y R5, R6 y R7 están en `MATCH` o en `NOT_APPLICABLE`, esto último solo cuando la solicitud lo declaró por esta tabla porque no contiene ningún
>    dato que compare esa comprobación (R5: una orden `identity`, `diff`, `scope` o `hashes` en `Runs`; R6: una orden `clausemap` en `Runs` o una
>    `AuthorityResolution` no vacía en la verificación; R7: una orden `scope`, `contract` o `clausemap` en `Runs`). La `Decision` sola no basta,
>    porque el aceptante también da `ACCEPT_FACTS` con comprobaciones declaradas no aplicables: esta regla lee las `Checks`;''',
    '''>    **aceptada** solo si el código es 0, la `Decision` es `ACCEPT_FACTS` y, además: R1, R2, R3, R9 y R10 están en `MATCH`; R8 está en `MANUAL`;
>    R4 está en `MATCH` si la solicitud lleva `Verification`, y en `NOT_APPLICABLE` solo si la regla de A-6 en D.6 la dejó fuera; y R5, R6 y R7 están
>    en `MATCH` o en `NOT_APPLICABLE`, esto último solo cuando la solicitud lo declaró por esta tabla porque no contiene ningún dato que compare esa
>    comprobación (R5: una orden `identity`, `diff`, `scope` o `hashes` en `Runs`; R6: una orden `clausemap` en `Runs` o una `AuthorityResolution`
>    no vacía en la `Verification` de la solicitud; R7: una orden `scope`, `contract` o `clausemap` en `Runs`). Una salida `EXECUTION_VERIFIED` solo
>    queda aceptada con R4 en `MATCH`. La `Decision` sola no basta, porque el aceptante también da `ACCEPT_FACTS` con comprobaciones declaradas no
>    aplicables: esta regla lee las `Checks`. R4 compara el `pass` del Controller con su propio cálculo en `Identity`, `Scope`, `CleanTree`,
>    `Trailer` y `FreeText`, y exige que una `Authority` en `pass` no tenga defectos de mapa; para R4, `fail` y `not_run` son por igual distintos de
>    `pass` (16.22). Una comprobación que el Controller deja en `not_run` después del primer `fail`, también en un control negativo, la juzga 16.9
>    sin cambio (cuenta como `fail`, con la disposición de su fila o sin ella si una comprobación anterior en `fail` la dejó sin entrada), no R4: si
>    es una de esas cinco, la solicitud no lleva `Verification`. En un control negativo, el juicio del control sigue siendo su oráculo relativo (RAE
>    §10); el aceptante solo decide si su salida se ingiere;''', 'A65-r2')
rep('''> 4. **salida no aceptada:** cualquier otro caso (`REJECT_FACTS`; una comprobación fuera de la tabla; una lectura P-22 de las reglas de A-6 en D.3
>    o D.6; o ningún resultado del aceptante en el intervalo de la regla de A-6 en D.6) es una salida conforme pero falsa o no demostrada. Incluye''',
    '''> 4. **salida no aceptada:** cualquier otro caso (`REJECT_FACTS`; una comprobación fuera de la tabla; una lectura P-22 de las reglas de A-6 en D.3
>    o D.6; ningún resultado del aceptante en el intervalo de la regla de A-6 en D.6; un registro que no permite reconstruir las ejecuciones; o una
>    salida sin ejecuciones que cita una orden del instrumento o un `FactsSha256`) es una salida conforme pero falsa o no demostrada. Incluye''',
    'A65-r4a')
rep('''>    control negativo; la tercera → STOP (P-04) y, en un control negativo, además control no superado. Cada reejecución es un lanzamiento que cuenta
>    en los topes de D.3, y esta regla no abre ninguna invocación. Una corrida del aceptante con código 64 es un error de la supervisión, no un
>    resultado: la supervisión corrige la solicitud y lo vuelve a ejecutar mientras dure la retención; terminada la retención, no hay resultado;''',
    '''>    control negativo; la tercera → STOP (P-04) y, en un control negativo, además control no superado. Cada reejecución es un lanzamiento que cuenta
>    en los topes de D.3, y esta regla no abre ninguna invocación. Si la salida no aceptada registra S-04 por una discrepancia entre un hecho del
>    instrumento y la orden Git primaria (regla 1), la sesión no la reejecuta: la devuelve al Coordinator como evidencia contradictoria (S-04), con
>    STOP, porque una reejecución con el mismo instrumento fijado no puede resolverla. Una corrida del aceptante con código 64 es un error de la
>    supervisión, no un resultado: la supervisión corrige la solicitud y lo vuelve a ejecutar mientras dure la retención; terminada la retención, no
>    hay resultado;''', 'A65-r4b')
rep('''>    evidencia no ingerible. En D.4, FX-02 solo es PASS con salidas aceptadas y conformes al oráculo sellado; FAIL sigue siendo una violación
>    observada en una salida aceptada frente a ese oráculo; y si la ronda termina sin una verificación aceptada (STOP P-04), FX-02 queda UNVERIFIED
>    con esa causa (registro incompleto), y el STOP sigue 16.11;''',
    '''>    evidencia no ingerible. En D.4, FX-02 solo es PASS con salidas ingeridas (aceptadas, o sin ejecuciones por la regla 7) y conformes al oráculo
>    sellado; FAIL sigue siendo una violación observada en una salida ingerida frente a ese oráculo; y si la ronda termina sin una verificación
>    ingerida (STOP P-04), FX-02 queda UNVERIFIED con esa causa (registro incompleto), y el STOP sigue 16.11;''', 'A65-r5')
rep('''>    cualquier otro efecto. Además, antes de esa primera invocación, la supervisión ejecuta el instrumento y el aceptante sobre los datos del
>    fixture de la ronda, sin Controller, con las órdenes que usará la verificación, y exige `ACCEPT_FACTS` en el nivel de los hechos: si el
>    aceptante rechaza hechos correctos del instrumento por una divergencia entre las dos piezas, no se usa tal como está.»''',
    '''>    cualquier otro efecto. Además, antes de esa primera invocación, la supervisión ejecuta el instrumento y el aceptante sobre los datos del
>    fixture de la ronda, sin Controller, con las órdenes que usará la verificación y con un cierre derivado del cierre efectivo real de las
>    invocaciones VERIFY del kit, con la cobertura que fija la disposición de la regla de A-6 sobre la elegibilidad, y exige `ACCEPT_FACTS` en el
>    nivel de los hechos (R4 y R10 no aplicables, porque no hay verificación): si el aceptante rechaza hechos correctos del instrumento por una
>    divergencia entre las dos piezas, o si una lectura que la verificación necesita queda fuera de ese cierre, no se usa tal como está;
> 7. **ámbito:** una invocación VERIFY con el instrumento es toda invocación VERIFY de FX-02 (la verificación, su reverificación de 16.7 si la hay
>    y cada control negativo con invocación) cuyo cierre admite el instrumento por la regla de A-6 sobre la admisión; a todas se aplica la
>    comprobación del punto de uso de la regla de A-6 en D.6. Ninguna está obligada a ejecutar el instrumento: su uso es del Controller (regla de
>    A-6 en D.3, regla 1). Según el registro de sesión del adapter:
>    (a) **con ejecuciones:** el registro, completo y legible, tiene al menos una ejecución del instrumento. Solo a ellas se aplican el aceptante
>    (regla de A-6 en D.6, reglas 5 y 6) y las reglas 2-5, sea cual sea la salida: verificación o control negativo, VERIFIED, REWORK, BLOCKED o
>    STOP;
>    (b) **sin ejecuciones:** el registro, completo y legible, no tiene ninguna. No hay solicitud ni aceptante: la salida se trata como sin el
>    instrumento, con 16.9, 16.11 y 16.22 sin cambio, y no puede apoyarse en ningún hecho del instrumento; si su `Evidence` cita una orden del
>    instrumento o un `FactsSha256`, es una salida no aceptada (regla 4);
>    (c) **registro insuficiente:** si el registro falta, está incompleto o es ilegible, o si una ejecución no se puede reconstruir (regla de A-6 en
>    D.6, regla 6), la salida no se acepta (regla 4);
>    (d) **dónde no corresponde ejecutarlo:** el Controller no ejecuta el instrumento sobre una entrada cuya comprobación Git primaria ya falló
>    (regla de A-6 en D.3, regla 7) ni para una comprobación que 16.9 deja sin entrada por una comprobación anterior en `fail`. En N4 (entrega
>    ausente) no hay ninguna orden del instrumento sobre la entrega, y en nc1 (`CurrentSha` inexistente) ninguna sobre ese `CurrentSha`; si un
>    control negativo o una salida de parada ejecutan el instrumento sobre las entradas que sí la tienen, son invocaciones con ejecuciones, con la
>    solicitud de la regla de A-6 en D.6, regla 6.»''', 'A65-r6r7')

# ------------------------------------------------------------------ A6-7
rep('Cubre: punto 7 del dictamen; R5 y O3 de la ronda 1.',
    'Cubre: punto 7 del dictamen; R5 y O3 de la ronda 1; R2 y O3 de la ronda 2.', 'A67-cubre')
rep('''>    cláusulas en la forma que exige `AuthorityResolution`; el contraste de cada hecho usado con la orden Git primaria; y salidas estructuradas
>    completas, sin omitir ningún requisito obligatorio. La medición no demuestra ninguna operación de la planificación, que no usa el instrumento;''',
    '''>    cláusulas en la forma que exige `AuthorityResolution`; el contraste de cada hecho usado con la orden Git primaria; y salidas estructuradas
>    completas, sin omitir ningún requisito obligatorio. Sobre el registro de sesión real de esa misma invocación demuestra además: que `git`,
>    resuelto por nombre en el entorno de la invocación, es el Git declarado; la reconstrucción de `Runs` de todas sus ejecuciones del instrumento
>    con una regla que cumple la regla de A-6 en D.6, regla 6 (a), sin alterar ningún byte; y una corrida del aceptante fijado sobre esas `Runs`, en
>    el mismo `-C` y después de su Entrada, con el cierre derivado de su cierre efectivo, que da `ACCEPT_FACTS` con R1, R2, R3 y R9 en `MATCH`, R5, R6
>    y R7 en `MATCH` o no aplicables por la tabla de la regla de A-6 sobre el resultado del aceptante, y R4 y R10 no aplicables, porque la medición no
>    produce una verificación. La medición no demuestra ninguna operación de la planificación, que no usa el instrumento;''', 'A67-r1')
rep('''>    routing, instancia del host, blob del instrumento, orden declarada, intérprete declarado o Git declarado) anula esa medición y la elegibilidad''',
    '''>    routing, instancia del host, blob del instrumento, orden declarada, intérprete declarado, Git declarado, regla de reconstrucción de `Runs` o
>    cierre de VERIFY de la regla 7) anula esa medición y la elegibilidad''', 'A67-r5')
rep('''> 6. no amplía permisos, alcance de escritura, autoridad del rol, capacidades declaradas ni consumo. P-01 y OD-2 rigen sin cambio: ninguna
>    invocación con una huella sin aceptar.»''',
    '''> 6. no amplía permisos, alcance de escritura, autoridad del rol, capacidades declaradas ni consumo. P-01 y OD-2 rigen sin cambio: ninguna
>    invocación con una huella sin aceptar;
> 7. antes de la medición, una disposición del Coordinator fija cómo el cierre efectivo de las invocaciones VERIFY de FX-02, que fija el kit, cubre
>    las lecturas legítimas del instrumento, en particular las de `CurrentSha` y las de los demás commits que reúne la tabla de derivación de la
>    regla de A-6 sobre la independencia, o limita las órdenes del instrumento a las lecturas que ese cierre admite, y nombra la regla de
>    reconstrucción de `Runs`. La medición usa ese cierre y esa regla, y la disposición que autoriza cada invocación VERIFY posterior nombra los
>    mismos. Sin esa disposición no hay medición; sin una medición que demuestre lo que exige la regla 1 sobre su registro de sesión real, la celda no
>    es elegible con el instrumento y el aceptante no se usa. Ninguna de estas condiciones se da por cumplida con evidencia que todavía no existe.»''',
    'A67-r7')

# ------------------------------------------------------------------ §3.12 (descriptive)
rep('''`MATCH`) y fuera las 4 que leen fuera (R9 `MISMATCH`). Si el cierre efectivo aplica la regla I6 de forma estricta, lecturas legítimas sobre la
entrega en `CurrentSha` quedan fuera: es la pregunta Q-A6R2-06 (§8).''',
    '''`MATCH`) y fuera las 4 que leen fuera (R9 `MISMATCH`). Si el cierre efectivo aplica la regla I6 de forma estricta, lecturas legítimas sobre la
entrega en `CurrentSha` quedan fuera (Q-A6R2-06, §8): A6-7, regla 7, condiciona la medición y el uso del aceptante a una disposición del
Coordinator sobre esa cobertura.''', 'S312-derivation')
rep('''**Registro de las brechas residuales G1-G6** (decisiones §80, punto 3). Solo un cambio del instrumento o del aceptante cerraría cada una, con
otro blob, otra corrección de A-6 y otra determinación del Owner (A6-8, regla 5). La ronda 2 no cambia ninguna pieza. **Ninguna brecha queda
cerrada**, y una mitigación no ensayada se señala como tal.''',
    '''**Registro de las brechas residuales G1-G6** (decisiones §80, punto 3; §83, punto 5). Solo un cambio del instrumento o del aceptante cerraría
cada una, con otro blob, otra corrección de A-6 y otra determinación del Owner (A6-8, regla 5). Las rondas 2 y 3 no cambian ninguna pieza.
**Ninguna brecha queda cerrada**, y una mitigación no ensayada se señala como tal. La revisión de la ronda 2 valoró las seis como residuos
aceptables. La ronda 3 actualiza dos puntos: la mitigación de R4 en G1 y G3 solo actúa cuando la solicitud lleva `Verification` (A6-3, regla 6
(c)), y la cobertura real de G4 se precisa (A62-A6R2-O2) con las comprobaciones (v)-(viii) del punto de uso ensayadas en `r3/r2static/`.''',
    'S312-intro')
rep('''  - *Mitigación:* R4 siempre en `MATCH` (A6-5, regla 2), cuya parte de mapa detecta un `Authority` en pass con defectos.''',
    '''  - *Mitigación:* R4 en `MATCH` en toda salida cuya solicitud lleva `Verification`, y por tanto en toda salida VERIFIED (A6-5, regla 2); su parte
    de mapa detecta un `Authority` en pass con defectos.''', 'G1-mit')
rep('''  - *Efecto normativo:* MV-1 no es un hecho crítico de A6-9, regla 3. Su independencia en el nivel de los hechos no está demostrada.''',
    '''  - *Efecto normativo:* MV-1 no es un hecho crítico de A6-9, regla 3. Su independencia en el nivel de los hechos no está demostrada. Desde la
    ronda 3, en una salida sin `Verification` en la solicitud (N4, o alguna de las cinco comprobaciones de R4 en `not_run`), que nunca es VERIFIED,
    un hecho de MV-1 falso no tiene control en el nivel de la decisión: solo puede llevar a un resultado distinto de VERIFIED.''', 'G1-eff')
rep('''  - *Mitigación:*
    - R4 para `CleanTree` y `FreeText`;''',
    '''  - *Mitigación:*
    - R4 para `CleanTree` y `FreeText`, cuando la solicitud lleva `Verification` (siempre en una salida VERIFIED);''', 'G3-mit')
rep('''  - *Efecto normativo:* la independencia de A6-9, regla 3, se limita a los hechos críticos. Para `Tests`, `Handoff` y la parte A5 de `Contract`,
    la verificación descansa en el Controller y en el oráculo.''',
    '''  - *Efecto normativo:* la independencia de A6-9, regla 3, se limita a los hechos críticos. Para `Tests`, `Handoff` y la parte A5 de `Contract`,
    la verificación descansa en el Controller y en el oráculo. En una salida sin `Verification` en la solicitud, un hecho falso de `cleantree` o
    `freetext` tampoco tiene el control de R4: puede llevar a un REWORK, un BLOCKED o un STOP indebidos, nunca a un VERIFIED.''', 'G3-eff')
rep('''  - *Cobertura real:*
    - la comprobación del punto de uso está ensayada sobre datos reales (`r2/r1r4/`);
    - la auditoría del registro de sesión **no está ensayada** con un registro real de Codex.''',
    '''  - *Cobertura real:*
    - de la comprobación del punto de uso, los pasos (i)-(iv) están ensayados sobre datos reales (`r2/r1r4/`) y los pasos (v)-(viii), en un clon del
      fixture (`r3/r2static/`), con el pwsh 7 del `PATH` y no con el del runtime de Codex;
    - la auditoría del registro de sesión **no está ensayada** con un registro real de Codex con ejecuciones del instrumento; el formato de los
      registros reales de la identidad vigente está medido (`r3/r2static/`).
  - *Llamadas a Git del instrumento* (A62-A6R2-O3): el instrumento lanza `git` por su nombre, resuelto por el `PATH` del entorno de la invocación.
    La medición de A6-7 demuestra que es el Git declarado (A6-7, regla 1); entre invocaciones lo mitigan la comprobación (viii) y la reejecución
    del aceptante en el entorno de la supervisión (R3 y R5). Residuo declarado.
  - *Alcance de R9* (A62-A6R2-O4): R9 aplica la correspondencia de A6-4, regla 3, a las `GitCalls` y a las `Inputs` que declara cada sobre, y a los
    `--file` de las órdenes que el aceptante compara. Las rutas del argv como tales y los `--file` de `freetext`, `handoff`, `trx`, `read-text` y
    `read-json` dependen de lo que declara el instrumento.''', 'G4-cov')

# ------------------------------------------------------------------ §3.14 (descriptive, the R4 statement)
rep('''| R4 sin ejercer en el extremo a extremo | A6-9, regla 3 (c): R4 ejercida sobre salidas conformes y alteradas en la evidencia de la ronda 2; A6-5, regla 2: R4 siempre en `MATCH` para aceptar |''',
    '''| R4 sin ejercer en el extremo a extremo | A6-9, regla 3 (c): R4 ejercida sobre salidas conformes y alteradas en la evidencia de la ronda 2; A6-5, regla 2: R4 en `MATCH` para aceptar toda salida cuya solicitud lleva `Verification`, y siempre una VERIFIED |''',
    'S314-r4')

# ------------------------------------------------------------------ §3.16 (O3 evidence cell) + §3.17
rep('''| O3: dónde se comprueba el intérprete | en la comprobación del punto de uso y en la Entrada de cada invocación VERIFY | A6-3, regla 3 (v); A6-7, regla 5 | `r2/r1r4/` |''',
    '''| O3: dónde se comprueba el intérprete | en la comprobación del punto de uso y en la Entrada de cada invocación VERIFY | A6-3, regla 3 (v); A6-7, regla 5 | `r3/r2static/` (ronda 3; `r2/r1r4/` no la ensayó) |''',
    'S316-o3')
rep('''Lo que la ronda 2 no cambia: A6-2 y A6-6, acordados en la ronda 1, conservan su texto; el instrumento (`9a829b0e…`) y el aceptante (`434ef1fc…`)
conservan su blob; las líneas del Owner de §7 conservan su texto literal.
''',
    '''Lo que la ronda 2 no cambia: A6-2 y A6-6, acordados en la ronda 1, conservan su texto; el instrumento (`9a829b0e…`) y el aceptante (`434ef1fc…`)
conservan su blob; las líneas del Owner de §7 conservan su texto literal.

### 3.17 Correcciones de la ronda 3 (decisiones §83)

Cambios normativos solo en A6-3, regla 6, A6-5 y A6-7 (decisiones §83, punto 2). A6-1, A6-2, A6-4, A6-6, A6-8, A6-9 y las reglas 1-5 y 7 de A6-3
conservan el texto de la ronda 2 byte a byte; el instrumento (`9a829b0e…`) y el aceptante (`434ef1fc…`) conservan su blob; los presupuestos, la
receta 16.4 y el alcance de la línea (a) del Owner no cambian. Las demás secciones que cambian son descriptivas: la cabecera, §1, §2 (fuentes de la
ronda 3), §3.1, el registro G1-G6 de §3.12, una fila de §3.14 y una celda de §3.16, esta sección, §4, §5, §6, §7, §8, §10, §11 y el anexo.

| Hallazgo de la ronda 2 | Corrección | Dónde | Prueba de la ronda 3 |
|---|---|---|---|
| R1 (a): qué es una invocación VERIFY «con el instrumento» y cómo se trata una sin ejecuciones | ámbito exhaustivo: con ejecuciones (aceptante y tabla), sin ejecuciones (16.9 sin cambio y sin hechos del instrumento) y registro insuficiente (no aceptada); el instrumento nunca es obligatorio | A6-5, regla 7; A6-3, regla 6 | `r3/r1probes/`: @@P_ZERO@@ |
| R1 (b): solicitud y tabla de N4 | sin `Verification` y con R4 no aplicable cuando falta uno de los tres archivos que lee R4 | A6-3, regla 6 (c); A6-5, regla 2 | `r3/r1probes/`: @@P_N4@@ |
| R1 (c): comprobaciones en `not_run` tras el primer fallo | las juzga 16.9 sin cambio; R4 solo con las cinco comprobaciones evaluadas; una salida VERIFIED, siempre con R4 | A6-5, regla 2; A6-3, regla 6 (c) | `r3/r1probes/`: @@P_NC1@@ |
| R1 (d): demostración con el aceptante fijado | ensayo del aceptante fijado sobre las entradas de N4, nc1 y una invocación VERIFY sin ejecuciones, con datos sintéticos y custodiados y sin Codex | — | `r3/r1probes/`: @@P_ALL@@ |
| R2 (a): `Runs` desde el registro real y corrida del aceptante sobre ellas | regla de reconstrucción con fallo cerrado; la medición la demuestra sobre su registro real, con el aceptante | A6-3, regla 6 (a); A6-7, reglas 1, 5 y 7 | estático: @@P_R2A@@; real: pendiente de la medición |
| R2 (b): cobertura de las lecturas legítimas por el cierre de VERIFY (Q-A6R2-06) | disposición del Coordinator antes de la medición; prueba previa con un cierre derivado del real | A6-7, regla 7; A6-5, regla 6 | estático: `r2/independence/` (d); real: pendiente de la disposición y de la medición |
| O1: instantáneas de la escritura de preparación | no incorporada: exige cambiar A6-3, regla 1, fuera de decisiones §83, punto 2; el riesgo lo cubren la comprobación previa (ii) y la del punto de uso | — | — |
| O2: cobertura real de G4 y fila O3 de §3.16 | cobertura real de G4 precisada paso a paso y evidencia de la fila O3 de §3.16 corregida | §3.12 (G4); §3.16 | `r3/r2static/`: @@P_POU@@ |
| O3: Git de las llamadas del instrumento | en parte: la medición demuestra que `git` por nombre es el Git declarado; el resto queda como residuo declarado en G4 | A6-7, regla 1; §3.12 (G4) | pendiente de la medición |
| O4: alcance de R9 | no incorporada en A6-4 (fuera de decisiones §83, punto 2); precisión en el registro de G4 | §3.12 (G4) | — |
| O5: S-04 frente a la reejecución | incorporada: la salida no aceptada con S-04 por una discrepancia con Git vuelve al Coordinator antes de reejecutar | A6-5, regla 4 | — |

**Precondiciones de R2** (decisiones §83, punto 4): qué se puede comprobar ya, sin Codex, y qué exige la medición real de A6-7. Ninguna se declara
cumplida con evidencia que todavía no existe.

| Precondición | Regla | Comprobable ya | Exige la medición real |
|---|---|---|---|
| reconstrucción de las ejecuciones desde los eventos reales de sesión | A6-3, regla 6 (a); A6-7, reglas 1 y 7 | @@PRE_RECON_STATIC@@ | @@PRE_RECON_REAL@@ |
| comparación de argv, entradas, bytes y resultados | A6-3, regla 6 (a); R3 del aceptante | R3 y la comprobación del sobre canónico, ejercidas en `r2/r1r4/` y en `r3/r1probes/` (@@PRE_CMP_STATIC@@) | las mismas comparaciones sobre las ejecuciones reales reconstruidas |
| identidad en el momento de uso | A6-3, regla 3; A6-7, reglas 1 y 5 | (i)-(iv) en `r2/r1r4/`; (v)-(viii) en `r3/r2static/` (@@PRE_POU_STATIC@@) | la entrega de los bytes por el shell real de la invocación y la resolución de `git` en su entorno |
| cobertura completa de las lecturas legítimas en el cierre efectivo | A6-7, regla 7; A6-5, regla 6 | con la regla I6 estricta, las corridas de `hashes` y `read-text` sobre `CurrentSha` quedan fuera (`r2/independence/`, (d)): hace falta la disposición del Coordinator | R9 en `MATCH` sobre las ejecuciones reales, con el cierre dispuesto |
| rechazo ante evidencia incompleta o inconsistente | A6-3, regla 6 (a); A6-5, reglas 4 y 7 | casos de fallo cerrado de la reconstrucción y del aceptante en `r3/r1probes/` (@@PRE_REJ_STATIC@@) | — (es una regla: la medición la aplica) |

**Riesgos residuales G1-G6 en la ronda 3.** Ninguna brecha se cierra. G2, G5 y G6 no cambian. G1 y G3 pierden la mitigación de R4 en las salidas
sin `Verification` en la solicitud, que nunca son VERIFIED: el error posible va en la dirección conservadora. G4 precisa su cobertura real y suma
dos residuos declarados (O3 y O4 de la ronda 2). Ninguna mitigación nueva se presenta como ensayada si no lo está.

**Procedimiento de lanzamiento del kit siguiente** (decisiones §83, punto 5; no forma parte del delta). La revisión de la ronda 2 quedó NOT_ACCREDITED
porque la sesión principal añadió `-X utf8` a la orden congelada y el lanzador cayó después de terminar el modelo sin escribir `run.json`. El kit
de la revisión siguiente se lanza con la orden congelada exacta, sin flags añadidos; su lanzador comprueba sus propias opciones de intérprete y
lleva una comprobación previa de registro que no ejecuta el modelo ni escribe un `run.json` de la revisión (`r3/launchfix/`: @@P_LAUNCH@@).
''', 'S317')

# ------------------------------------------------------------------ §4 (descriptive)
rep('''| Aceptante | — | mismo `-C`, después de la Entrada y antes de cualquier escritura, con la sesión retenida (A6-3, reglas 5-7) |
| Salida aceptada | — | se ingiere con su clasificación (A6-5, regla 3) |
| Salida no aceptada (REJECT_FACTS, comprobación fuera de la tabla, P-22 o sin resultado) | — | INVALID_OUTPUT con efecto P-19: no se aplica su clasificación; reejecución de transporte dentro de 16.8 y de los topes de D.3; tercera → STOP (P-04) (A6-5, regla 4) |
| Resultado de FX-02 (D.4) | — | PASS solo con salidas aceptadas conformes al oráculo; FAIL, violación observada en una salida aceptada; sin verificación aceptada al final, UNVERIFIED con causa (A6-5, regla 5) |''',
    '''| Aceptante | — | solo en las invocaciones VERIFY con ejecuciones del instrumento; mismo `-C`, después de la Entrada y antes de cualquier escritura, con la sesión retenida; solicitud reconstruida del registro de sesión con fallo cerrado (A6-3, reglas 5-7; A6-5, regla 7) |
| Invocación VERIFY sin ejecuciones del instrumento | — | sin solicitud ni aceptante; 16.9, 16.11 y 16.22 sin cambio; no puede apoyarse en hechos del instrumento (A6-5, regla 7) |
| Controles negativos y comprobaciones en `not_run` | — | aceptante solo si ejecutan el instrumento; sin `Verification` en la solicitud, R4 no aplicable (N4, o una de las cinco comprobaciones de R4 en `not_run`); juicio del control por su oráculo relativo (A6-5, reglas 2 y 7) |
| Salida aceptada | — | se ingiere con su clasificación (A6-5, regla 3) |
| Salida no aceptada (REJECT_FACTS, comprobación fuera de la tabla, P-22, sin resultado, registro insuficiente o cita del instrumento sin ejecuciones) | — | INVALID_OUTPUT con efecto P-19: no se aplica su clasificación; reejecución de transporte dentro de 16.8 y de los topes de D.3; tercera → STOP (P-04); con S-04 por una discrepancia con Git, al Coordinator antes de reejecutar (A6-5, reglas 4 y 7) |
| Resultado de FX-02 (D.4) | — | PASS solo con salidas ingeridas conformes al oráculo; FAIL, violación observada en una salida ingerida; sin verificación ingerida al final, UNVERIFIED con causa (A6-5, regla 5) |''',
    'S4-rows')
rep('''| Elegibilidad de la celda | sin celda elegible (DEC L1507); P-10 | una invocación medida con el instrumento, solo para VERIFY, con disposición y consumo del Owner; tope 1; sin reintento (A6-7) |''',
    '''| Elegibilidad de la celda | sin celda elegible (DEC L1507); P-10 | una invocación medida con el instrumento, solo para VERIFY, con disposición y consumo del Owner; tope 1; sin reintento; con la reconstrucción de `Runs` y el aceptante sobre su registro real y la disposición previa sobre el cierre de VERIFY (A6-7) |''',
    'S4-elig')

# ------------------------------------------------------------------ §5 (descriptive)
rep('''Reevaluada después de las correcciones de la ronda 2 (decisiones §79, punto 4); no se hereda de la ronda 1 ni del dictamen. La columna Dictamen es
el valor literal de su campo `Materiality[].Activated`, y la columna Ronda 1, la evaluación del revisor de la ronda 1 (`MaterialityConfirmation`).

| M | Activado | Dictamen | Ronda 1 | Motivo |
|---|---|---|---|---|''',
    '''Reevaluada después de las correcciones de la ronda 3 (decisiones §83, punto 2); no se hereda de las rondas anteriores ni del dictamen. La columna
Dictamen es el valor literal de su campo `Materiality[].Activated`, y las columnas Ronda 1 y Ronda 2, la evaluación del revisor de cada ronda
(`MaterialityConfirmation`).

| M | Activado | Dictamen | Ronda 1 | Ronda 2 | Motivo |
|---|---|---|---|---|---|''', 'S5-head')
rep('''| M-01 | no | no | UNKNOWN (DISAGREE) | ningún dueño nuevo''', '''| M-01 | no | no | UNKNOWN (DISAGREE) | no (AGREE) | ningún dueño nuevo''', 'S5-m01a')
rep('''(A6-9, regla 4). AUTOMATION_PLAN 16.9 y 16.13 siguen siendo dueñas de las reglas, el Controller del resultado y el Coordinator de las comprobaciones entre artefactos; la determinación de ADR-0046 #9 es del Owner (A6-8) |''',
    '''(A6-9, regla 4). AUTOMATION_PLAN 16.9 y 16.13 siguen siendo dueñas de las reglas, el Controller del resultado y el Coordinator de las comprobaciones entre artefactos; la determinación de ADR-0046 #9 es del Owner (A6-8). La ronda 3 no añade ningún dueño: el ámbito de A6-5, regla 7, remite a 16.9 sin cambio las invocaciones sin ejecuciones y las comprobaciones en `not_run` |''',
    'S5-m01b')
rep('''| M-02 | **sí** | yes | AGREE |''', '''| M-02 | **sí** | yes | AGREE | yes (AGREE) |''', 'S5-m02')
rep('''| M-03 | **sí** | yes | AGREE |''', '''| M-03 | **sí** | yes | AGREE | yes (AGREE) |''', 'S5-m03')
rep('''| M-04 | **sí** | yes | AGREE |''', '''| M-04 | **sí** | yes | AGREE | yes (AGREE) |''', 'S5-m04a')
rep('''invalidadores nuevos: el blob, la orden y el intérprete (A6-1, regla 8; A6-7, regla 5); sin FAITHFUL no hay lanzamiento (A6-4, regla 1) |''',
    '''invalidadores nuevos: el blob, la orden y el intérprete (A6-1, regla 8; A6-7, regla 5); sin FAITHFUL no hay lanzamiento (A6-4, regla 1); en la ronda 3, registro insuficiente o cita del instrumento sin ejecuciones como salida no aceptada, S-04 antes de reejecutar y reconstrucción de `Runs` con fallo cerrado (A6-3, regla 6; A6-5, reglas 4 y 7), y una medición que exige además la reconstrucción y el aceptante sobre su registro real (A6-7, reglas 1 y 7) |''',
    'S5-m04b')
rep('''| M-05 | **sí** | yes | AGREE |''', '''| M-05 | **sí** | yes | AGREE | yes (AGREE) |''', 'S5-m05a')
rep('''los textos de AUTOMATION_PLAN 16.24, del procedimiento subordinado y de los esquemas no cambian |''',
    '''los textos de AUTOMATION_PLAN 16.24, del procedimiento subordinado y de los esquemas no cambian; en la ronda 3, la regla de reconstrucción de `Runs`, la condición de `Verification` en la solicitud y la disposición sobre el cierre de VERIFY (A6-3, regla 6; A6-7, regla 7) |''',
    'S5-m05b')
rep('''| M-06 | no | no | AGREE |''', '''| M-06 | no | no | AGREE | no (AGREE) |''', 'S5-m06')
rep('''| M-07 | no | no | AGREE |''', '''| M-07 | no | no | AGREE | no (AGREE) |''', 'S5-m07')
rep('''| M-08 | **sí** | yes | AGREE |''', '''| M-08 | **sí** | yes | AGREE | yes (AGREE) |''', 'S5-m08')

# ------------------------------------------------------------------ §6.1-§6.3 (descriptive)
rep('''- **Diff permitido del commit que publique la ronda 2** (sobre la HEAD de la rama en ese momento): solo `docs/initiatives/I-62-A-6.md` (modificado:
  blob de la ronda 1 → blob de la ronda 2) y archivos nuevos de `docs/automation/evidence/I-62-A6/r2/` (guardas, autoprueba, evidencia de
  reproducción e independencia, resultados y preflight). Los archivos de la ronda 1 en `docs/automation/evidence/I-62-A6/` no cambian. El registro
  en decisiones, en la evidencia, en el estado o en el README de F6 va en un commit aparte.

**Superficies prohibidas** (ninguna cambia en el commit que publique la ronda 2; las guardas comparan la lista y sus objetos en la base y en la cabeza):''',
    '''- **Diff permitido del commit que publique la ronda 3** (sobre la HEAD de la rama en ese momento): solo `docs/initiatives/I-62-A-6.md` (modificado:
  blob de la ronda 2 → blob de la ronda 3) y archivos nuevos de `docs/automation/evidence/I-62-A6/r3/` (guardas, autoprueba, pruebas del aceptante,
  evidencia estática de R2, procedimiento de lanzamiento, resultados y preflight). Los archivos de las rondas 1 y 2 en
  `docs/automation/evidence/I-62-A6/` no cambian. El registro en decisiones, en la evidencia, en el estado o en el README de F6 va en un commit
  aparte.

**Superficies prohibidas** (ninguna cambia en el commit que publique la ronda 3; las guardas comparan la lista y sus objetos en la base y en la cabeza):''',
    'S61-diff')
rep('''`docs/automation/evidence/I-62-architect-A-6/` (la revisión de la ronda 1) y todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r2/` (la evidencia de la ronda 1).''',
    '''`docs/automation/evidence/I-62-architect-A-6/` (la revisión de la ronda 1), `docs/automation/evidence/I-62-architect-A-6r2/` (la revisión de la ronda 2) y todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r3/` (la evidencia de las rondas 1 y 2).''',
    'S61-forb')
rep('''  `I-62-architect-A-6/` `520e7a1f…` (`output.json` `e90d685b…`) y árbol `I-62-A6/` de la ronda 1 `7f504a17…` fuera de `r2/`.''',
    '''  `I-62-architect-A-6/` `520e7a1f…` (`output.json` `e90d685b…`), árbol de la revisión de la ronda 2 `I-62-architect-A-6r2/` `@@R2_REVIEW_TREE8@@…`
  (`output.json` `fc357d09…`), árbol `I-62-A6/` de la ronda 1 `7f504a17…` fuera de `r2/` y árbol `I-62-A6/r2/` `@@R2_EVID_TREE8@@…`.''',
    'S61-blobs')
rep('''`docs/automation/evidence/I-62-A6/r2/a6r2-guards.py` (solo biblioteca estándar; modos `self-test`, `preview` y `run`), adaptadas de las de la
ronda 1, que no cambian:''',
    '''`docs/automation/evidence/I-62-A6/r3/a6r3-guards.py` (solo biblioteca estándar; modos `self-test`, `preview` y `run`), adaptadas de las de la
ronda 2 (`r2/a6r2-guards.py`), que no cambian:''', 'S63-intro')
rep('''- **G2:** rutas del commit: solo `I-62-A-6.md` modificado y archivos añadidos bajo `r2/`.
- **G3:** superficies prohibidas, objetos congelados y C-20b, incluidas la revisión y la evidencia de la ronda 1.
- **G4:** correspondencia con el dictamen y con los hallazgos de la ronda 1 (§3.16), coherente con la línea `Cubre:` de cada delta.
- **G5:** pureza del delta; A6-2 y A6-6, byte a byte como en la ronda 1.''',
    '''- **G2:** rutas del commit: solo `I-62-A-6.md` modificado y archivos añadidos bajo `r3/`.
- **G3:** superficies prohibidas, objetos congelados y C-20b, incluidas las revisiones y la evidencia de las rondas 1 y 2.
- **G4:** correspondencia con el dictamen y con los hallazgos de las rondas 1 (§3.16) y 2 (§3.17), coherente con la línea `Cubre:` de cada delta.
- **G5:** pureza del delta; A6-1, A6-2, A6-4, A6-6, A6-8, A6-9 y las reglas de A6-3 distintas de la 6, byte a byte como en la ronda 2.''',
    'S63-g')
rep('''- **G10, nueva:** el resultado del aceptante de A6-5 frente a los bytes del aceptante fijado (comprobaciones, dispensables, fórmula de la
  `Decision`, códigos de salida, datos de cada comprobación y entradas de la solicitud de A6-3) y los estados normativos citados en §2.''',
    '''- **G10:** el resultado del aceptante de A6-5 frente a los bytes del aceptante fijado (comprobaciones, dispensables, fórmula de la `Decision`,
  códigos de salida, datos de cada comprobación, entradas de la solicitud de A6-3 y la condición de R4 sobre `Verification`) y los estados
  normativos citados en §2.''', 'S63-g10')
rep('''La autoprueba mata todos sus mutantes sobre este texto, y los controles negativos de extremo a extremo dan el resultado esperado. Los recuentos y
los resultados de `preview` y de `run` sobre el commit que publique la ronda 2 están en `r2/` y en `r2/preflight.md`. Ninguna revisión formal si no
pasan.''',
    '''La autoprueba mata todos sus mutantes sobre este texto, y los controles negativos de extremo a extremo dan el resultado esperado. Los recuentos y
los resultados de `preview` y de `run` sobre el commit que publique la ronda 3 están en `r3/` y en `r3/preflight.md`. Ninguna revisión formal si no
pasan.''', 'S63-end')
rep('''| A-5 | AGREED, decisiones §73, `ad94ceed…` | A-6 se aplica en B1 y FX-U2 y en el clon de la medición; el instrumento no entra en F_seed ni en F_norm de B1 (A6-3, regla 2), así que A5-1 y su precondición MV-1..MV-7 no cambian; A5-2 sin cambio: la sesión de FX-U2 ejecuta FX-02 con A-6 solo si A-6 se acuerda; los topes de FX-02 siguen en continuidad | ninguno |
''',
    '''| A-5 | AGREED, decisiones §73, `ad94ceed…` | A-6 se aplica en B1 y FX-U2 y en el clon de la medición; el instrumento no entra en F_seed ni en F_norm de B1 (A6-3, regla 2), así que A5-1 y su precondición MV-1..MV-7 no cambian; A5-2 sin cambio: la sesión de FX-U2 ejecuta FX-02 con A-6 solo si A-6 se acuerda; los topes de FX-02 siguen en continuidad | ninguno |

Ronda 3: las correcciones de A6-3, regla 6, A6-5 y A6-7 no cambian ninguna de estas interacciones. La medición de A6-7 sigue contándose aparte de
A2-P2 y de A4-1; la reconstrucción de `Runs`, la corrida del aceptante sobre su registro real y la disposición sobre el cierre de VERIFY son
actividades de la supervisión y del Coordinator, y no consumen ninguna invocación ni presupuesto de A-1..A-5. Las salidas sin ejecuciones siguen
16.9 sin cambio, como antes de A-6.
''', 'S62-r3')

# ------------------------------------------------------------------ §7 (descriptive)
rep('''fija los blobs `9a829b0e…` y `434ef1fc…` (A6-8, regla 5).
''',
    '''fija los blobs `9a829b0e…` y `434ef1fc…` (A6-8, regla 5).

**Ronda 3.** Las correcciones de la ronda 3 tampoco cambian el instrumento ni el aceptante: (a) los sigue cubriendo con el mismo alcance y no hace
falta ninguna línea nueva del Owner. Las nuevas exigencias de A6-7 (reconstrucción de `Runs` y corrida del aceptante sobre el registro real de la
medición; disposición previa sobre el cierre de VERIFY) son actividades de la supervisión y del Coordinator dentro de la misma medición que
autorizaría (b), sin ampliarla.
''', 'S7-r3')

# ------------------------------------------------------------------ §8 (descriptive)
rep('## 8. Disposición de la ronda 1 y preguntas de la ronda 2 (no forman parte del delta)',
    '## 8. Disposición de las rondas 1 y 2 y preguntas de la ronda 3 (no forman parte del delta)', 'S8-title')
rep('''**Preguntas de la ronda 2:**

| Id | Pregunta |
|---|---|
| Q-A6R2-01 | A6-3, regla 5, retiene a la sesión entre la Entrada de cada invocación VERIFY y su paso siguiente mientras corre el aceptante. ¿Es compatible con 16.4 como regla añadida solo para FX-02 en F6, o exige otra forma? |
| Q-A6R2-02 | A6-5, regla 4, trata toda salida no aceptada como INVALID_OUTPUT con el efecto de P-19 y la recuperación de transporte. ¿Es la correspondencia exacta con 16.9, 16.23 y RAE §10, o falta algún efecto? |
| Q-A6R2-03 | A6-5, regla 5, deja FX-02 en UNVERIFIED («registro incompleto») si la ronda termina sin una verificación aceptada. ¿Es la lectura correcta de D.4? |
| Q-A6R2-04 | A6-1, regla 7, hace que toda ejecución del instrumento registrada entre en `Runs`, y una ejecución con error deja la salida sin aceptar. ¿Es aceptable ese fallo cerrado para los controles negativos (por ejemplo, nc1)? |
| Q-A6R2-05 | ¿Basta la evidencia de independencia de §3.12, con lo que declara como no demostrado, para A6-9, regla 3? ¿Son aceptables como residuos declarados G1-G6, o alguno exige cambiar una pieza (blob nuevo y otra determinación del Owner)? |
| Q-A6R2-06 | La tabla de derivación del cierre del aceptante sigue los blobs que fija el cierre efectivo. Si ese cierre aplica de forma estricta la regla I6 (todo blob igual al de la `AuthorityRevision`), lecturas legítimas de VERIFY sobre la entrega en `CurrentSha` quedan fuera. ¿Es una cuestión del cierre de VERIFY del kit de FX-02 (OQ-31), ajena a A-6, o debe fijarla A-6? |''',
    '''**Preguntas de la ronda 2**, según la revisión R20261012T195511Z-e5c3 y decisiones §83:

| Id | Estado en la ronda 3 |
|---|---|
| Q-A6R2-01 | sin hallazgo en la revisión de la ronda 2 sobre la retención; sin cambio (A6-3, regla 5) |
| Q-A6R2-02 | tema Q-A6-06/07 NOT_SATISFIED en la revisión por A62-A6R2-R1; corregido: ámbito total de A6-5 (regla 7), N4 y `not_run` (regla 2) y S-04 antes de reejecutar (regla 4) |
| Q-A6R2-03 | sin hallazgo; lectura conservada, ahora con salidas ingeridas (A6-5, regla 5) |
| Q-A6R2-04 | respondida por A6-5, regla 7 (d): sin órdenes del instrumento sobre la entrada que falló; una ejecución con error sigue sin aceptar (regla 4) |
| Q-A6R2-05 | la revisión valoró G1-G6 como residuos aceptables y Q-A6-12 SATISFIED; sin cambio de piezas (§3.12) |
| Q-A6R2-06 | según la revisión, el cierre corresponde al kit, pero A-6 condiciona el uso del aceptante a su resolución: A6-7, regla 7, y A6-5, regla 6 |

**Preguntas de la ronda 3:**

| Id | Pregunta |
|---|---|
| Q-A6R3-01 | A6-3, regla 6 (c), deja fuera `Verification`, y con ello R4, cuando falta uno de los tres archivos o alguna de las cinco comprobaciones de R4 está en `not_run`. Así R4 ya no puede rechazar una salida correcta por un `not_run` que 16.9 admite, y sigue juzgando toda salida VERIFIED. ¿Es aceptable perder el control de R4 en esas salidas, que nunca son VERIFIED (G1 y G3)? |
| Q-A6R3-02 | A6-5, regla 7 (b), trata una invocación VERIFY sin ejecuciones como sin el instrumento, con 16.9 sin cambio. ¿Es compatible con la calificación de C-24 de A6-6, regla 3, y con A6-8? |
| Q-A6R3-03 | Los registros reales de Codex mezclan la salida estándar y la de error en un solo campo de texto (`r3/r2static/`). A6-3, regla 6 (a), exige aislar exactamente un sobre canónico terminado en LF sin alterar ningún byte, y si no, deja la salida sin aceptar. ¿Basta esa regla, o hace falta cambiar la orden declarada para separar la salida de error, lo que tocaría A6-1, fuera de decisiones §83, punto 2? |
| Q-A6R3-04 | A6-5, regla 4, devuelve al Coordinator como S-04, sin reejecutar, una salida no aceptada que registra una discrepancia entre el instrumento y Git. ¿Es coherente con 16.11 y con la recuperación de transporte de 16.8? |''',
    'S8-q')

# ------------------------------------------------------------------ §10, §11, annex (descriptive)
rep('- Decisiones §73-§79 (DEC L1479 en adelante); evidencia §140-§154.',
    '- Decisiones §73-§83 (DEC L1479 en adelante); evidencia §140-§163.', 'S10-dec')
rep('''  - reproducción desde cero de los dos paquetes, REPRODUCED, con la autoría, la ejecución y la revisión distinguidas (`repro/`);
  - guardas y preflight.''',
    '''  - reproducción desde cero de los dos paquetes, REPRODUCED, con la autoría, la ejecución y la revisión distinguidas (`repro/`);
  - guardas y preflight.
- Revisión de la ronda 2: `docs/automation/evidence/I-62-architect-A-6r2/R20261012T195511Z-e5c3/` (`output.json`, `audit.json`,
  `launch-recording-defect.json`, `README.md`).
- Ronda 3: `docs/automation/evidence/I-62-A6/r3/`:
  - pruebas del aceptante fijado con N4, nc1, VERIFY sin instrumento, la regresión de T1, mutantes de la solicitud y la reconstrucción de
    referencia de `Runs` (`r1probes/`);
  - formato de los registros reales de Codex y comprobaciones (v)-(viii) del punto de uso (`r2static/`);
  - procedimiento de lanzamiento corregido para el kit siguiente y su comprobación previa de registro (`launchfix/`);
  - guardas, negativos y preflight.''', 'S10-r3')
rep('''| Architect independiente | ronda 1: CHANGES REQUIRED (R20261011T215735Z-c434), NOT_ACCREDITED; ronda 2: NOT LAUNCHED, hasta una disposición posterior del Coordinator | decisiones §79 |
| Coordinator | ronda 1: R1-R5 aceptados como asuntos que resolver; sin veredicto sobre la ronda 2 | decisiones §79 |''',
    '''| Architect independiente | ronda 1: CHANGES REQUIRED (R20261011T215735Z-c434), NOT_ACCREDITED; ronda 2: CHANGES REQUIRED (R20261012T195511Z-e5c3), NOT_ACCREDITED; ronda 3: NOT LAUNCHED, hasta una disposición posterior del Coordinator | decisiones §79, §82 y §83 |
| Coordinator | ronda 1: R1-R5 aceptados como asuntos que resolver; ronda 2: A62-A6R2-R1 y A62-A6R2-R2 aceptados como hallazgos técnicos que resolver, sin usar el dictamen como acuerdo; sin veredicto sobre la ronda 3 | decisiones §79 y §83 |''',
    'S11-table')
rep('''Mientras A-6 sea PROPUESTA, se corrige en este mismo archivo, con blob nuevo, y la historia queda en Git: la ronda 1 es el blob `eac473c0…` del
commit `7b7aba46`. Una vez acordada, solo la corrige, revoca o sustituye la A-n siguiente (LIFECYCLE §6).''',
    '''Mientras A-6 sea PROPUESTA, se corrige en este mismo archivo, con blob nuevo, y la historia queda en Git: la ronda 1 es el blob `eac473c0…` del
commit `7b7aba46`, y la ronda 2, el blob `86da0ae7…` del commit `ae9fe0e9`. Una vez acordada, solo la corrige, revoca o sustituye la A-n siguiente
(LIFECYCLE §6).''', 'S11-hist')
rep('''1. **Revisión de la ronda 2.** Con una disposición del Coordinator que la autorice (decisiones §79, punto 7): una única revisión formal (`claude-cli`,
   dentro de CLAUDE-CLI-I62), con acceso directo a todas las fuentes que cite.''',
    '''1. **Revisión de la ronda 3.** Con una disposición del Coordinator que la autorice (decisiones §83, punto 6): una única revisión formal (`claude-cli`,
   dentro de CLAUDE-CLI-I62), con acceso directo a todas las fuentes que cite, lanzada con la orden congelada exacta y con la comprobación previa de
   registro (§3.17).''', 'AX-1')
rep('''2. **Acuerdo.** Architect y Coordinator, sobre el blob exacto de A-6; las guardas de la ronda 2 en modo `run` sobre el commit que la publique.
3. **Lecturas normativas del aceptante** (A6-5, regla 6): disposición del Coordinator con la tabla de coincidencia, antes de la medición.''',
    '''2. **Acuerdo.** Architect y Coordinator, sobre el blob exacto de A-6; las guardas de la ronda 3 en modo `run` sobre el commit que la publique.
3. **Lecturas normativas del aceptante y cierre de VERIFY** (A6-5, regla 6; A6-7, regla 7): disposiciones del Coordinator con la tabla de
   coincidencia, la cobertura de las lecturas legítimas del instrumento por el cierre de VERIFY y la regla de reconstrucción de `Runs`, antes de la
   medición.''', 'AX-23')
rep('''   intérprete, blob, trío y `-C` (un clon con historia, el objeto del instrumento escrito por A6-3 y su comprobación del punto de uso): una sola
   invocación de medición, sin reintento. Si falla, FX-02 sigue UNVERIFIED.''',
    '''   intérprete, blob, trío y `-C` (un clon con historia, el objeto del instrumento escrito por A6-3 y su comprobación del punto de uso): una sola
   invocación de medición, sin reintento, con la reconstrucción de `Runs` y la corrida del aceptante sobre su registro real (A6-7, regla 1). Si
   falla, FX-02 sigue UNVERIFIED.''', 'AX-4')
rep('''   de uso antes de cada lanzamiento (A6-3), y el aceptante después de cada Entrada VERIFY, con la sesión retenida (A6-3, A6-5).''',
    '''   de uso antes de cada lanzamiento (A6-3), y el aceptante después de cada Entrada VERIFY con ejecuciones, con la sesión retenida (A6-3, A6-5).''',
    'AX-5')

open(OUT, 'w', encoding='utf-8', newline=NL).write(t)
print('ok', len(done), 'substitutions;', len(t.encode('utf-8')), 'bytes;', t.count(NL), 'lines;', 'placeholders:',
      sorted(set(__import__('re').findall(r'@@[A-Z0-9_]+@@', t))))
