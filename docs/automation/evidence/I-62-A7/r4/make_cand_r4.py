# make_cand_r4.py - derives the FOURTH candidate of I-62 A-7 (round 4; decisiones §98) from the round-3 blob (8bcccb2f) by exact
# replacements, each of which must occur exactly once. Normative change only in A7-1, points 1 (c), 2 (c) and 5 (c) (A62-A7R3-R1); the
# other replacements are descriptive (header, §2 L23-L27, §3.3, §3.4 with O3, §3.5 residuals of O1 and O2, §3.7, new §3.8, §4 with O3,
# §5, §6, §8, §10, §11, Annex).   python -I make_cand_r4.py <round-3 I-62-A-7.md> <out I-62-A-7.md>
import hashlib, subprocess, sys

R3_BLOB = '8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff'
NL = chr(10)


def blob_id(b):
    return hashlib.sha1(b'blob %d' % len(b) + bytes([0]) + b).hexdigest()


R = []


def add(old, new):
    R.append((old, new))


# ---- header
add('Amendment: A-7 (candidata, ronda 3; preparada, no enviada; ninguna revisión de la ronda 3 lanzada)',
    'Amendment: A-7 (candidata, ronda 4; preparada, no enviada; ninguna revisión de la ronda 4 lanzada)')
add('''CORRECTED salvo el residuo de R1 que nombra A62-A7R2-R1. Decisiones §96: A62-A7R2-R1 ACCEPTED REQUIRED. Las rondas 1 y 2 se conservan en Git; la ronda 3
se corrige en este mismo archivo, con blob nuevo (§3.7)''',
    '''CORRECTED salvo el residuo de R1 que nombra A62-A7R2-R1. Decisiones §96: A62-A7R2-R1 ACCEPTED REQUIRED.
ronda 3 = blob 8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff (commit 37ac6ca4). Revisión formal R20261023T180846Z-62bd (decisiones §97-§98; evidencia §201):
auditor ACCREDITED; Architect CHANGES REQUIRED (A62-A7R3-R1; A62-A7R3-O1..O3), con A62-A7R2-R1 PARTIALLY_CORRECTED por el residuo que nombra
A62-A7R3-R1. Decisiones §98: A62-A7R3-R1 ACCEPTED REQUIRED. Las rondas 1 a 3 se conservan en Git; la ronda 4 se corrige en este mismo archivo, con
blob nuevo (§3.8)''')
add('''de V14 D.3, después del párrafo de A6-8, corregida en la ronda 2 solo por A62-A7-R1..R3 (§3.6) y en la ronda 3 solo por A62-A7R2-R1 (§3.7). M-02, M-03,
M-04 y M-05 sí; M-01, M-06, M-07 y M-08 no (§5)''',
    '''de V14 D.3, después del párrafo de A6-8, corregida en la ronda 2 solo por A62-A7-R1..R3 (§3.6), en la ronda 3 solo por A62-A7R2-R1 (§3.7) y en la
ronda 4 solo por A62-A7R3-R1 (§3.8). M-02, M-03, M-04 y M-05 sí; M-01, M-06, M-07 y M-08 no (§5)''')
add('''revisión R20261019T010200Z-4a72. Ronda 3: decisiones §96 sobre la revisión R20261019T201030Z-d1bf
''',
    '''revisión R20261019T010200Z-4a72. Ronda 3: decisiones §96 sobre la revisión R20261019T201030Z-d1bf. Ronda 4: decisiones §98 sobre la
revisión R20261023T180846Z-62bd
''')
add('''ronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261019T010200Z-4a72). Ronda 2: CHANGES REQUIRED, NOT_ACCREDITED (R20261019T201030Z-d1bf). Ronda 3:
NOT LAUNCHED. Ninguna revisión formal sin una disposición nueva del Coordinator (decisiones §96, punto 7)''',
    '''ronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261019T010200Z-4a72). Ronda 2: CHANGES REQUIRED, NOT_ACCREDITED (R20261019T201030Z-d1bf). Ronda 3:
CHANGES REQUIRED, ACCREDITED (R20261023T180846Z-62bd). Ronda 4: NOT LAUNCHED. Ninguna revisión formal sin una disposición nueva del Coordinator
(decisiones §98, punto 7)''')
add('''PROPUESTA (ronda 3). No aplicada: ningún texto congelado, materializado ni de producción cambia por este registro. Sin invocación, sin medición y sin
revisión formal de la ronda 3.''',
    '''PROPUESTA (ronda 4). No aplicada: ningún texto congelado, materializado ni de producción cambia por este registro. Sin invocación, sin medición y sin
revisión formal de la ronda 4.''')
# ---- §2 sources L23-L27
add('''| L22 | V14 B.8.7 | 2110-2114 | un rebase dentro de una ventana se lleva «en el `RebaseMap` del `relay-record/v2` del diario» |
''',
    '''| L22 | V14 B.8.7 | 2110-2114 | un rebase dentro de una ventana se lleva «en el `RebaseMap` del `relay-record/v2` del diario» |
| L23 | RAE §10 | 219-221 | los controles con invocación se ejecutan «solo sobre las entradas de la verificación `EXECUTION_VERIFIED` del último `WorkRunId` de la cadena», cada uno con una ruta y un registro propios, «con copias de todas las entradas que conservan sus `TaskId`, `RunId`, `DelegationRunId` y `Attempt` internos salvo el campo mutado» |
| L24 | contratos del Controller de FX-02 (`90929ad1`) | 23 | nc1, nc2, nc3, N4, N5, N6, N7a y N7b son VERIFY con el `Target` «de la verificación real (OQ-31)» |
| L25 | AP 16.4 | 536-537 | orden de commits: «(5) ninguna escritura Git de la sesión hasta terminar la verificación, la reverificación de 16.7 si la hay y los controles negativos, salvo el rebase de 16.7» |
| L26 | AP 16.9 | 634-635 | `Identity`: «tras un rebase, sobre los originales del `RebaseMap` y sus imágenes, con `patch-id` igual»; `Remote`: «`origin/main` = `MainSha`» |
| L27 | V14 D.3 | 2441 | los ocho controles con invocación: nc1, nc2, nc3 y «N4 handoff ausente → BLOCKED, N5 0 pruebas → REWORK, N6 trabajo sin commit → CleanTree REWORK, N7a otra corrida con Identity pass → REWORK, N7b otra corrida con Identity fail → BLOCKED/STOP» |
''')
# ---- A7-1, point 1 (c) (normative, A62-A7R3-R1)
add('''>    la verificación; y, solo en la reverificación de 16.7 sin conflictos, el registro de rebase de la sesión (`relay-record/v2` con `Phase: REBASE` y
>    su `RebaseMap`, 16.6, 16.7 y B.8.7), del que la reverificación toma las imágenes y `MainSha_eval` (16.13); en ella, la corrida de `CurrentSha` es
>    la de su imagen (`Ci` sobre `CurrentSha'`) y la de `RedSha`, la original (16.7). La invocación de medición, que no produce una verificación, no
>    tiene hechos del relevo (A6-7, regla 1), y ninguna invocación distinta de la reverificación tiene registro de rebase;''',
    '''>    la verificación; y, solo en la reverificación de 16.7 sin conflictos y en cada control negativo con invocación que se ejecuta sobre sus
>    entradas (RAE §10), el registro de rebase de la sesión (`relay-record/v2` con `Phase: REBASE` y su `RebaseMap`, 16.6, 16.7 y B.8.7), del que la
>    reverificación toma las imágenes y `MainSha_eval` (16.13) y del que el control recibe una copia; en ellos, la corrida de `CurrentSha` es la de su
>    imagen (`Ci` sobre `CurrentSha'`) y la de `RedSha`, la original (16.7). La invocación de medición, que no produce una verificación, no tiene
>    hechos del relevo (A6-7, regla 1), y ninguna otra invocación (la verificación ordinaria, sus controles y la medición) tiene registro de rebase;''')
# ---- A7-1, point 2 (c) (normative, A62-A7R3-R1)
add('''>    delegación, entrega, TRX de la corrida de `CurrentSha`, TRX de la corrida de `RedSha`, hechos del relevo y, en la reverificación, registro de
>    rebase), con un artefacto como máximo por papel, y registra la ruta y el blob de cada uno; la delegación y la entrega van bajo
>    `artifacts/orchestration/`, en la ruta de la tarea o del control (RAE §10), en un directorio cuyo nombre es el `RunId` de sus propios bytes; el
>    registro de rebase va en el directorio del intento de la tarea (`artifacts/orchestration/<UnitId>/<TaskId>/<Attempt>/`), en un directorio cuyo
>    nombre es su propio `RunId` (16.6), distinto del de la delegación, el de la entrega y el de la reverificación, y la designación registra también ese
>    `RunId`. El registro de rebase es coherente si es un `relay-record/v2` con `Phase` = `REBASE`, ese `RunId` y el `TaskId` y el `Attempt` de la
>    invocación, y su `RebaseMap` no tiene rutas en conflicto ni commits sin imagen, su `MainBeforeSha` es `K.MainSha`, su `AuthorityRevision` es la del
>    contrato con la igualdad de 16.7 registrada, su `BaseSha` es el de la delegación y asigna al `CurrentSha` de la entrega la imagen `Target.commit`
>    con igualdad de `patch-id`; `MainSha_eval` de la reverificación es el `MainAfterSha` de ese `RebaseMap` (16.13), un commit del `-C` posterior a
>    su `MainBeforeSha` del que desciende `Target.commit`; (d) por la regla de A-6;''',
    '''>    delegación, entrega, TRX de la corrida de `CurrentSha`, TRX de la corrida de `RedSha`, hechos del relevo y, en la reverificación y en sus
>    controles, registro de rebase), con un artefacto como máximo por papel, y registra la ruta y el blob de cada uno; la delegación y la entrega van bajo
>    `artifacts/orchestration/`, en la ruta de la tarea o del control (RAE §10), en un directorio cuyo nombre es el `RunId` de sus propios bytes; el
>    registro de rebase de la reverificación va en el directorio del intento de la tarea (`artifacts/orchestration/<UnitId>/<TaskId>/<Attempt>/`), en
>    un directorio cuyo nombre es su propio `RunId` (16.6), distinto del de la delegación, el de la entrega y el de la reverificación, y la designación
>    registra también ese `RunId`. El registro de rebase es coherente si es un `relay-record/v2` con `Phase` = `REBASE`, ese `RunId` y el `TaskId` y el
>    `Attempt` de la invocación, y su `RebaseMap` no tiene rutas en conflicto ni commits sin imagen, su `MainBeforeSha` es `K.MainSha`, su
>    `AuthorityRevision` es la del contrato con la igualdad de 16.7 registrada, su `BaseSha` es el de la delegación y asigna al `CurrentSha` de la
>    entrega la imagen `Target.commit` con igualdad de `patch-id`; `MainSha_eval` de la reverificación es el `MainAfterSha` de ese `RebaseMap` (16.13),
>    un commit del `-C` posterior a su `MainBeforeSha` del que desciende `Target.commit`. Un control negativo con invocación que se ejecuta sobre las
>    entradas de una reverificación (RAE §10) solo tiene cierre si su designación identifica esa reverificación por su `RunId` y por el SHA-256 de su
>    designación, que cumple este punto con su registro de rebase presente, verificado y coherente con su propia entrega, su `Target` y su contrato; si
>    el control tiene el `UnitId`, el `TaskId`, el `Attempt`, la `AuthorityRevision` y el `Target` de esa reverificación y un `RunId` propio; si su
>    registro de rebase es la copia del de la reverificación, con el mismo blob y el mismo `RunId`, que la designación registra, en el directorio del
>    intento de la ruta del control (`artifacts/orchestration/<UnitId>/<TaskId>-<control>/<Attempt>/`), en un directorio cuyo nombre es ese `RunId`; y
>    si cada otro artefacto de (c), salvo los hechos del relevo, que son los del registro propio del control, es la copia del de su papel en la
>    reverificación, con el mismo blob y en la misma ruta (la delegación y la entrega, en la ruta del control), y solo difiere el artefacto que lleva los
>    campos que el control muta, y solo en ellos, o falta la entrega que el control deja ausente (RAE §10; D.3). En ese control, la coherencia del
>    registro es la de la reverificación, comprobada con las entradas de esta y no con la entrega mutada o ausente del control, y `MainSha_eval` es el
>    `MainAfterSha` de ese registro; (d) por la regla de A-6;''')
# ---- A7-1, point 5 (c) (normative, A62-A7R3-R1)
add('''>    `MainSha` del contrato); en cualquier otra invocación, un registro de rebase designado no se admite; comprobar que un artefacto designado falta, en
>    la ruta que fija la designación, no es una lectura fuera del cierre ni aporta contenido.''',
    '''>    `MainSha` del contrato); en un control negativo sobre las entradas de una reverificación, si la reverificación identificada, su registro de
>    rebase, la copia de ese registro o alguna de las copias del control no cumplen el punto 2 (c), no hay cierre ni lanzamiento, y nada los
>    sustituye; la entrega mutada (nc1) o ausente (N4) del control no impide fijar `MainSha_eval`; en cualquier otra invocación (la verificación
>    ordinaria, sus controles y la medición), un registro de rebase designado no se admite; comprobar que un artefacto designado falta, en la ruta que
>    fija la designación, no es una lectura fuera del cierre ni aporta contenido.''')
# ---- §3.3, row (c)
add('''solo en la reverificación de 16.7, el registro de rebase (`relay-record/v2`, `Phase: REBASE`, con su `RebaseMap`), fuente de las imágenes y de `MainSha_eval` |''',
    '''solo en la reverificación de 16.7 y en los controles sobre sus entradas, el registro de rebase (`relay-record/v2`, `Phase: REBASE`, con su `RebaseMap`; en el control, su copia), fuente de las imágenes y de `MainSha_eval` |''')
add('''el registro de rebase, en el directorio del intento, en un directorio con su propio `RunId`) |''',
    '''el registro de rebase, en el directorio del intento, en un directorio con su propio `RunId`, y en el control, su copia en el directorio del intento de la ruta del control) |''')
add('''para el registro de rebase, también su `RunId` |''',
    '''para el registro de rebase, también su `RunId`; para un control sobre las entradas de una reverificación, además el `RunId` y el SHA-256 de la designación de esa reverificación |''')
add('''en la reverificación, sin un registro de rebase presente, verificado y coherente (o sin la entrega): sin cierre y sin lanzamiento; en otra invocación, el registro de rebase no se admite;''',
    '''en la reverificación, sin un registro de rebase presente, verificado y coherente (o sin la entrega): sin cierre y sin lanzamiento; en un control sobre sus entradas, sin esa reverificación válida, sin la copia de su registro o con una copia que no es la suya: sin cierre y sin lanzamiento (la entrega mutada o ausente del control no lo impide); en la verificación ordinaria, sus controles y la medición, el registro de rebase no se admite;''')
# ---- §3.4 (A62-A7R3-O3; the copy of the record outside C6)
add('''  artefactos de (c) que no son artefactos versionados (la delegación, la entrega, los TRX y los hechos del relevo) también son excepciones a esa clase y
  se admiten solo por el blob de sus bytes custodiados (A7-1, punto 1). Es la alternativa que deja la determinación de §91, que los sitúa fuera de la
''',
    '''  artefactos de (c) que no son artefactos versionados (la delegación, la entrega, los TRX, los hechos del relevo y, en la reverificación y en sus
  controles, el registro de rebase) también son excepciones a esa clase y se admiten solo por el blob de sus bytes custodiados (A7-1, punto 1). Es la
  alternativa que deja la determinación de §91, que los sitúa fuera de la
''')
add('''  identificación por rutas de la designación es la que A-6 §3.18 ya atribuye al kit. El registro de rebase de la reverificación, como los hechos del
  relevo, no recibe nombre en C6: el instrumento no lo lee, y la tabla de A6-9 no cambia.''',
    '''  identificación por rutas de la designación es la que A-6 §3.18 ya atribuye al kit. El registro de rebase de la reverificación y su copia en los
  controles, como los hechos del relevo, no reciben nombre en C6: el instrumento no los lee, y la tabla de A6-9 no cambia (sobre `MainSha_eval` y las
  imágenes del rebase en C1, §3.5).''')
# ---- §3.5 residuals declared for A62-A7R3-O1 and A62-A7R3-O2 (descriptive; decisiones §98, punto 5)
add('''está fijada por el cierre (por ejemplo, `AUTOMATION_PLAN.md` en EFF^1, categoría (a)) no bloquea.
''',
    '''está fijada por el cierre (por ejemplo, `AUTOMATION_PLAN.md` en EFF^1, categoría (a)) no bloquea.

**Residuo declarado (C1 del aceptante en la reverificación y en sus controles; A62-A7R3-O1).** La tabla de A6-9, que A-7 no cambia, lleva a C1
`D.MainSha`, `K.MainSha`, `I.Target.Commit`, `RevList(D.BaseSha..H.CurrentSha)` sobre los originales, la cadena de primer padre de `D.MainSha` y la base
común (A-6 L855-L859), pero no el `MainAfterSha` del registro de rebase ni las imágenes del rebase distintas de `Target.commit`. En la reverificación y
en sus controles, una orden del instrumento que lee en `MainSha_eval` (`clausemap --main`) o en una de esas imágenes registra un commit fuera de C1, y
el aceptante da R9 `MISMATCH`: un rechazo falso, nunca una aceptación falsa, y la salida no se acepta (A6-7, regla 4). La comprobación del kit descrita
arriba mira blobs, no commits, y no lo detecta. No afecta a la verificación ordinaria, a sus controles ni a la medición de A6-7, cuyo `MainSha_eval` es
`K.MainSha` (en C1), ni a una invocación sin ejecuciones del instrumento (A6-7, regla 7 (b)). A-7 no amplía la tabla ni el cierre por esta causa y no la
da por resuelta; la evaluación y su evidencia están en el preflight de la ronda 4 (`I-62-A7/r4/`, casos O1-*).

**Residuo declarado (segunda recuperación de 16.7; A62-A7R3-O2).** 16.7 admite como máximo dos recuperaciones por `TaskId` (AP L598). Tras una segunda
recuperación, el registro del segundo rebase tiene como `MainBeforeSha` el `MainAfterSha` del primero, no `K.MainSha`, y no es coherente según el punto
2 (c) (si su `RebaseMap` asigna las imágenes del primero, tampoco asigna el `CurrentSha` de la entrega): la segunda reverificación y sus controles no
tienen cierre ni lanzamiento, nada sustituye ese registro (ni el del primer rebase ni `K.MainSha`) y el caso vuelve al Coordinator. A-7 no define la
composición de registros de rebase encadenados (preflight de la ronda 4, `I-62-A7/r4/`, casos O2-*).
''')
# ---- §3.7 note and the new §3.8
add('''M-04), §6, §8, §10, §11 y el Anexo. Los opcionales A62-A7R2-O1..O4 de la revisión de la ronda 2 no se incorporan (§96, punto 2: corrección limitada al
`RebaseMap`).''',
    '''M-04), §6, §8, §10, §11 y el Anexo. Los opcionales A62-A7R2-O1..O4 de la revisión de la ronda 2 no se incorporan (§96, punto 2: corrección limitada al
`RebaseMap`). La ronda 4 corrige la frase final de esta corrección («en otra invocación, no admitido») solo para los controles sobre las entradas de la
reverificación (A62-A7R3-R1, §3.8); la no admisión en la verificación ordinaria, en sus controles y en la medición se conserva.''')
add('''uso en una VERIFY ordinaria, un control y la medición; N4, nc1 y tipo B1 |
''',
    '''uso en una VERIFY ordinaria, un control y la medición; N4, nc1 y tipo B1 |

### 3.8 Corrección de la ronda 4 (decisiones §98)

Cambio normativo solo por A62-A7R3-R1 (decisiones §98, punto 2), en A7-1, puntos 1 (c), 2 (c) y 5 (c). Los demás puntos y categorías de A7-1 conservan el
texto de la ronda 3, y la no admisión del registro de rebase en la verificación ordinaria, en sus controles y en la medición de A6-7 no cambia. Las demás
secciones que cambian son descriptivas: la cabecera, §2 (fuentes L23-L27), §3.3, §3.4 (con la enumeración de A62-A7R3-O3), §3.5 (residuos de
A62-A7R3-O1 y A62-A7R3-O2, sin cambio normativo), §3.7, esta sección, §4 (con A62-A7R3-O3), §5 (M-03 y M-04), §6, §8, §10, §11 y el Anexo. Los
opcionales no abren ninguna enmienda (decisiones §98, punto 5).

| Hallazgo de la ronda 3 | Corrección | Dónde | Prueba de la ronda 4 |
|---|---|---|---|
| A62-A7R3-R1: los controles negativos con invocación que RAE §10 ejecuta sobre las entradas de una reverificación de 16.7 VERIFIED no admiten el registro de rebase, y su coherencia se comprobaría con la entrega mutada (nc1) o ausente (N4) del control | el control admite la copia del registro de rebase de esa reverificación (mismo blob y mismo `RunId`, registrados) en el directorio del intento de la ruta del control; su designación identifica la reverificación por su `RunId` y por el SHA-256 de su designación, y el control tiene su `UnitId`, `TaskId`, `Attempt`, `AuthorityRevision` y `Target`; la coherencia es la de la reverificación, comprobada con sus entradas, y `MainSha_eval` es su `MainAfterSha`; cada otro artefacto de (c) es copia (mismo blob y misma ruta; la delegación y la entrega, en la ruta del control), salvo los hechos del relevo (registro propio) y el que lleva los campos mutados, que difiere solo en ellos (o la entrega ausente de N4); sin eso, sin cierre ni lanzamiento; en la verificación ordinaria, sus controles y la medición, no admitido | A7-1, puntos 1 (c), 2 (c) y 5 (c); §3.3; §4 | `I-62-A7/r4/` (preflight de la ronda 4): nc1, nc2, nc3, N4, N5, N6, N7a y N7b sobre las entradas de RV, con el cotejo de sus entradas frente a las de RV; sustitución y manipulación de la copia del registro, de la identidad de RV, del vínculo y de las copias; mutación ausente, de más o en otro papel; control de V con registro como uso indebido; sin regresión de las fases de la ronda 3 |

**Matriz de modalidades (decisiones §98, punto 3).** Resumen; la matriz completa (entradas, procedencia, cierre, mutaciones permitidas, comprobaciones y
resultado esperado de cada modalidad, con los casos del preflight que la ejercen) está en `docs/automation/evidence/I-62-A7/r4/modalities.md`.

| Modalidad | Registro de rebase | `MainSha_eval` | Cierre |
|---|---|---|---|
| verificación (VERIFY ordinaria), con o sin el instrumento | no se admite | `K.MainSha` | A7-1; (d) solo con el instrumento |
| controles nc1, nc2, nc3 y N4-N7b de la verificación ordinaria | no se admite | `K.MainSha` | A7-1 con las copias del control |
| rebase de 16.7 antes de escribir (contrato y delegación reemitidos) | no es entrada | `K.MainSha` del contrato reemitido | el de una verificación ordinaria |
| reverificación de 16.7 sin conflictos, con o sin el instrumento | obligatorio y coherente con su entrega | `MainAfterSha` | sin registro coherente o sin la entrega: sin cierre |
| controles nc1, nc2, nc3 y N4-N7b sobre las entradas de la reverificación | copia del de la reverificación | `MainAfterSha` de esa copia | coherencia de la reverificación; copias salvo los campos mutados; si no, sin cierre |
| segunda recuperación de 16.7 y sus controles | incoherente (`MainBeforeSha` ≠ `K.MainSha`) | no se puede fijar | sin cierre ni lanzamiento (§3.5) |
| recuperación con conflictos | — (16.7: sin reverificación) | — | ninguna invocación |
| medición de A6-7 | no se admite | `K.MainSha` | A7-1 sin hechos del relevo |
| nc4 y los negativos de sesión N8-N10 | — (sin invocación: RAE §10; D.3) | — | fuera del ámbito |
''')
# ---- §4 (A62-A7R3-O3)
add('''| Registro de rebase de la reverificación (`RebaseMap`) | no es un artefacto versionado: no admisible; la reverificación de 16.7 no puede fijar `MainSha_eval` sin una lectura fuera del cierre | admitido solo en la reverificación, por el blob de sus bytes, con su `RunId` y su coherencia; fuente de las imágenes y de `MainSha_eval`; en otra invocación, no admitido |''',
    '''| Registro de rebase de la reverificación (`RebaseMap`) | no es un artefacto versionado: no admisible; la reverificación de 16.7 y los controles sobre sus entradas no pueden fijar `MainSha_eval` sin una lectura fuera del cierre | admitido solo en la reverificación y, como su copia, en los controles sobre sus entradas, por el blob de sus bytes, como excepción a la clase de §20.3.1 (precisión de A6-2, regla 1), con su `RunId` y su coherencia (en el control, la de la reverificación); fuente de las imágenes y de `MainSha_eval`; en la verificación ordinaria, sus controles y la medición, no admitido |''')
# ---- §5
add('''registro de rebase de la reverificación, EFF^1) quedan dentro del cierre |''',
    '''registro de rebase de la reverificación y su copia en los controles sobre sus entradas, EFF^1) quedan dentro del cierre |''')
add('''en la reverificación, sin un registro de rebase coherente no hay cierre ni lanzamiento)''',
    '''en la reverificación, sin un registro de rebase coherente, y en sus controles, sin esa reverificación válida y la copia de su registro, no hay cierre ni lanzamiento)''')
# ---- §6
add('''El registro de rebase solo entra en una reverificación de 16.7, que A-5 no regula.''',
    '''El registro de rebase solo entra en una reverificación de 16.7 y en los controles sobre sus entradas, que A-5 no regula.''')
# ---- §8
add('''o (b) debería usar la imagen de `BaseSha` del `RebaseMap`? |
''',
    '''o (b) debería usar la imagen de `BaseSha` del `RebaseMap`? |

Respuestas de la revisión de la ronda 3 (R20261023T180846Z-62bd): Q-A7R3-02 y Q-A7R3-03 SATISFIED; Q-A7R3-01 NOT_SATISFIED por A62-A7R3-R1, corregido en
la ronda 4 (§3.8).

**Preguntas de la ronda 4:**

| Id | Pregunta |
|---|---|
| Q-A7R4-01 | Un control sobre las entradas de una reverificación admite la copia del registro de rebase de esa reverificación, con su coherencia comprobada con las entradas de la reverificación y `MainSha_eval` = su `MainAfterSha` (A7-1, punto 2 (c)). ¿Basta para que los controles de RAE §10 lean el mismo `RebaseMap` que la reverificación sin admitirlo en la verificación ordinaria, en sus controles ni en la medición? |
| Q-A7R4-02 | La designación del control identifica la reverificación por su `RunId` y por el SHA-256 de su designación, y exige que cada artefacto de (c) sea copia del de la reverificación (mismo blob y misma ruta; la delegación y la entrega, en la ruta del control), salvo los hechos del relevo (registro propio) y el que lleva los campos mutados (o la entrega ausente de N4). ¿Es la lectura correcta de «copias de todas las entradas … salvo el campo mutado» (RAE §10) para fijar `MainSha_eval` sin sustituciones? |
| Q-A7R4-03 | Los residuos de A62-A7R3-O1 (C1 del aceptante sin `MainAfterSha` ni las imágenes del rebase) y de A62-A7R3-O2 (segunda recuperación sin cierre) se declaran en §3.5 sin cambio normativo y con fallo cerrado. ¿Es aceptable para el ámbito de A-7? |
''')
# ---- §10
add('''  `docs/automation/evidence/I-62-A7/r2/`; el de la ronda 3, en `docs/automation/evidence/I-62-A7/r3/`.''',
    '''  `docs/automation/evidence/I-62-A7/r2/`; el de la ronda 3, en `docs/automation/evidence/I-62-A7/r3/`; el de la ronda 4, con la matriz de modalidades,
  en `docs/automation/evidence/I-62-A7/r4/`.''')
add('''- Revisión de la ronda 2: `docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/` (kit, `output.json`, `audit.json`, `README.md`).''',
    '''- Revisión de la ronda 2: `docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/` (kit, `output.json`, `audit.json`, `README.md`).
- Revisión de la ronda 3: `docs/automation/evidence/I-62-architect-A-7/R20261023T180846Z-62bd/` (kit, `output.json`, `audit.json`, `README.md`).''')
# ---- §11
add('''ronda 2: CHANGES REQUIRED (R20261019T201030Z-d1bf), NOT_ACCREDITED; ronda 3: NOT LAUNCHED, hasta una disposición nueva del Coordinator | decisiones §93-§96 |''',
    '''ronda 2: CHANGES REQUIRED (R20261019T201030Z-d1bf), NOT_ACCREDITED; ronda 3: CHANGES REQUIRED (R20261023T180846Z-62bd), ACCREDITED; ronda 4: NOT LAUNCHED, hasta una disposición nueva del Coordinator | decisiones §93-§98 |''')
add('''ronda 2: A62-A7R2-R1 aceptado como REQUIRED; sin veredicto sobre la ronda 3 | decisiones §94 y §96 |''',
    '''ronda 2: A62-A7R2-R1 aceptado como REQUIRED; ronda 3: A62-A7R3-R1 aceptado como REQUIRED; sin veredicto sobre la ronda 4 | decisiones §94, §96 y §98 |''')
add('''`6fee8fd6`, y la ronda 2, el blob `8a528133…` del commit `e90b444d`. Una vez acordada, solo la corrige, revoca o sustituye la A-n siguiente''',
    '''`6fee8fd6`; la ronda 2, el blob `8a528133…` del commit `e90b444d`; y la ronda 3, el blob `8bcccb2f…` del commit `37ac6ca4`. Una vez acordada,
solo la corrige, revoca o sustituye la A-n siguiente''')
# ---- Annex
add('''1. **Revisión formal** de A-7 ronda 3 (una, acotada a A62-A7R2-R1 y sus consecuencias), con una disposición del Coordinator que la autorice, un kit
   con lectura fiel, referencias comprobadas contra el cierre y un dictamen expreso sobre la compatibilidad con A-5 y con A-6 (decisiones §96, punto 6).''',
    '''1. **Revisión formal** de A-7 ronda 4 (una, acotada a A62-A7R3-R1 y sus consecuencias), con una disposición del Coordinator que la autorice, un kit
   con lectura fiel, referencias comprobadas contra el cierre y un dictamen expreso sobre la compatibilidad con A-5 y con A-6 (decisiones §98, punto 7).''')


def main(argv):
    if len(argv) != 3:
        sys.stderr.write('usage: python -I make_cand_r4.py <round-3 I-62-A-7.md> <out I-62-A-7.md>\n')
        return 64
    src = open(argv[1], 'rb').read()
    assert blob_id(src) == R3_BLOB, 'the source is not the round-3 blob'
    t = src.decode('utf-8')
    for i, (old, new) in enumerate(R):
        n = t.count(old)
        if n != 1:
            sys.stderr.write('replacement %d: the old text occurs %d times: %r\n' % (i, n, old[:80]))
            return 1
        t = t.replace(old, new, 1)
    out = t.encode('utf-8')
    assert b'\r' not in out and not any(c < 32 and c != 10 for c in out)
    open(argv[2], 'wb').write(out)
    sys.stdout.write('%s %d replacements, %d bytes, %d lines, blob %s\n' % (argv[2], len(R), len(out), out.count(b'\n'), blob_id(out)))
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))
