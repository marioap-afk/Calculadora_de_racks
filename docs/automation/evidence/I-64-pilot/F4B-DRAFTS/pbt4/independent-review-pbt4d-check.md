# Comprobación acotada de PB-T4d (CA-T4d) sobre el paquete v5.2

**Revisor independiente y adversarial.** No participé en el paquete. Fecha: 2026-11-22.

**Base:** el cierre de PB-T4c, `6c2820bab4d8b41e3e229e23f3a1638ccbb04735`, leído solo por objetos git desde el checkout principal (`git show`, `grep`, `ls-tree`, `log`). No leí el worktree. No compilé ni ejecuté pruebas. No escribí en el repositorio.

**Las piezas que recibí, con su sha256 comprobado:**

| Pieza | sha256 |
|---|---|
| El paquete, `pbt4/I-64-f4b-pbt4-gate-contract-package-v52.md` | `466ab3db…f13e` |
| El GREEN, `pbt4/pbt4d-green-v52.txt` | `4059860f…4932` |
| El RED, `pbt4/pbt4d-red-v52.txt` | `26734e73…cf42` |
| La lista, `pbt4/pbt4d-retire-list-229.tsv` | `1943468b…1fad` |
| `pbt4dcc/compile/pbt4d-precondition6.json` | `56bcba77…70b5` |
| `pbt4dcc/compile/compile-check.md` | `9c4adedab…8934` |
| Las 27 sustitutas, `v52/run/PushBackRetirePilotRunTests.cs` | `e8b93c2e…c5c8` |
| CT-PBT4D-01, `v52/ct/PushBackRetirePilotGuardTests.cs` | `0d64806a…adb3` |
| CT-PBT4D-02, `v52/ct/PushBackRetirePilotCensusGuardTests.cs` | `1874c478…3f20` |

**Mis propios guiones de lectura**, en la carpeta `rv4d/` del scratchpad de la sesión, fuera del paquete, con `python -I`:
- `apply_green_rv.py` y `apply_red_rv.py`: aplican las dos piezas en memoria sobre los objetos git de la base, con un aplicador mío.
- `covdiff_rv.py`: compara la cobertura JSON de coverlet de la suite completa de Core, en la base y en el GREEN, en los 605 archivos de `src/`.
- `cob_lines_rv.py` y `r6k_who_rv.py`: leen las coberturas Cobertura de `r6j` y de `r6k`.
- `listcheck_rv.py`, `tests_rv.py` y `members_rv.py`: casan los cambios del RED con la lista.
- `runeq_rv.py`: compara las sustitutas con su original más el mapa.
- `dead_rv.py`: busca los ayudantes que el RED deja sin uso.
- `secdiff_rv.py`: compara las secciones de la v5.1 y la v5.2.

## Veredicto

**EMITIBLE TRAS CORRECCIONES. B 0 / I 2 / M 7.**

La v5.2 **no se puede emitir** con la autorización condicional, porque tiene dos importantes:
- **I-01** se arregla dentro de lo que el Owner ya pidió («lo que también prueba código que se queda conserva esa parte»).
- **Pero vuelve al Owner por su propia regla.** `owner-pbt4d-run-coverage-decision.md` dice: «Si aparece […] un camino que se quede sin prueba, vuelve al Owner».
- **Después hacen falta tres cosas:** una v5.3, la precondición 6 otra vez y una comprobación acotada del delta.

El resto del paquete está bien:
- el GREEN es exacto;
- el RED se reproduce byte a byte;
- las 27 sustitutas son el mapa exacto de su original, salvo las 2 hechas a mano y declaradas;
- las CT fijan lo que dicen;
- los totales cuadran.

## Hallazgos

| Id | Gravedad | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| **I-01** | **Importante** | La lista, filas 130 y 131. Las pruebas, en `6c2820ba:tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs:890-963`. El código, en `6c2820ba:src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs:26-62` | **Dos bajas dejan sin prueba código que se queda.** Las dos son de WPUR: la n.º 48 (`TheSystemIsRebuiltFromTheDesignTextOfAMemberOfThePlan`) y la n.º 49 (`TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope`).<br>**Lo que dice la lista:** «borrar con registro», con el porqué «usa la corrida del piloto como vehiculo; lo que ejercita de la corrida generica lo cubren otras pruebas que se quedan».<br>**Ese porqué es falso:** no corren la corrida. Prueban `PushBackPilotSystemRebuild` (`Build`, `BuildForMember` e `IsComposite`), y del piloto solo usan `PilotUpdateSetup().ExpectedPlan()` para tener un plan.<br>**`PushBackPilotSystemRebuild` se queda:**<br>• lo llama `DesiredNamesOf`, en `PushBackPilotUpdateRun.cs:312` de la base (`:244` del GREEN);<br>• lo llama el escritor real de «Actualizar», en `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs:131`.<br>**Medido con los archivos de la propia precondición 6:**<br>• `r6j-cv`: la línea `:32` (`throw … UnreadableDesignReason`) tiene 4 golpes en `wpur` y 0 en `resto`. Las ramas de `:28`, `:29`, `:31`, `:48`, `:51` y `:60` están enteras en `wpur` y a medias en `resto`;<br>• `r6k-ct`: la `:32` solo la cubren la 48 y la 49; las ramas de `:28` y de `:60`, solo la 48; las de `:29`, `:48` y `:51`, solo la 49;<br>• `r6r-p6/core-green-cov`: la `:32` tiene 0 golpes y esas seis líneas quedan con la mitad de sus ramas.<br>**También se pierde lo que fija la 49 de INV-C1-01** (F4-T4-HOST-C1, prueba 1 de D-C1-09): el nombre crudo, con espacios, en blanco o nulo, y `UnreadableEnvelopeReason`.<br>**Va contra dos decisiones del Owner:**<br>• `owner-pbt4d-substitutes-decision.md`: «lo que también prueba código que se queda conserva esa parte»;<br>• `owner-pbt4d-run-coverage-decision.md`: «un camino que se quede sin prueba, vuelve al Owner».<br>**La afirmación de §6.4.1** (cobertura «PASA», «0 ramas perdidas») vale solo dentro de su universo (I-02) | **Volver al Owner con una propuesta:** sustituir también la 48 y la 49 en `PushBackRetirePilotRunTests`, con el mapa (`FullUpdateSetup().ExpectedPlan()`). Serían 29 sustitutas, con 31 resultados.<br>**Lo que cambia en la v5.3:**<br>• las filas 130 y 131 pasan a «sustituir con registro»;<br>• el `MinSelected` del n.º 88 pasa de 29 a 31;<br>• los totales previstos de Core, a medir: RED 19 418 = 19 014 + 1 + 403 y GREEN 19 418 = 19 015 + 0 + 403;<br>• se regeneran la pieza del RED, la lista, el `NewFileSha`, §4.4, §5.6, §6.4.1 y el anexo V52-A;<br>• se repite la precondición 6.<br>**La otra opción para el Owner:** declararlas como pérdida |
| **I-02** | **Importante** | §6.4.1: INV-PBT4D-06 (línea 2849 del paquete), PBT4D-C16 (2874) y el paso 7 de la precondición 6 (2920). §4.4, «Lo que la medida no ve» (1539). `pbt4dcc/compile/cov_p6_v52.py:31`, `v52/cov_v52.py:20` y `v52/cov_tests_v52.py:18` (`FILES = ['PushBackPilotUpdateRun.cs', 'PushBackPilotUpdatePlan.cs']`) | **El método de cobertura es correcto rama por rama, pero su universo es más estrecho de lo que el Owner pidió.**<br>**Lo que hace bien:**<br>• compara conjuntos de (punto, path), no cuentas;<br>• toma como referencia la unión de la suite completa de la base;<br>• lleva las líneas al GREEN por texto;<br>• exige todas las ramas de las líneas nuevas.<br>**Lo que no mira:**<br>• solo las 436 líneas que WPUR ejecutaba, y solo en dos archivos;<br>• no ve a quien llama la corrida desde otros archivos (`PushBackPilotSystemRebuild`, I-01);<br>• no ve lo que pierden las demás bajas, fuera de WPUR.<br>**Lo que pidió el Owner:** «que no quede sin prueba ningún camino que se queda». El propio texto de INV-PBT4D-06 también es más amplio.<br>**Lo medí con los dos JSON que ya existen** (`r6s-bc/base.coverage.json` y `r6r-p6/core-green-cov.coverage.json`), en los 605 archivos de `src/`:<br>• 419 líneas cubiertas en la base son de lo retirado;<br>• fuera de eso, se pierden 1 línea y 7 líneas con ramas: 6 de I-01 y 1 de M-02;<br>• nada más | **Ampliar el criterio del paso 7, de INV-PBT4D-06 y de C16:** cada línea y cada rama de `src/` que la suite de Core cubre en `BaseSha`, fuera de lo que retira el GREEN, sigue cubierta en `CurrentSha`. Las excepciones son las que el Owner acepte.<br>**Lo demás:**<br>• las 436 y las 9 ramas se quedan como conjunto vigilado;<br>• se reescribe «Lo que la medida no ve» |
| M-01 | Menor | CT-PBT3B-06 (iii), en `6c2820ba:tests/RackCad.Tests/PushBackFullWritePrepareTests.cs:596-614` | **Una parte de lo que se quita también probaba código que se queda.** El RED quita (ii) y (iii) como «drafts of the PILOT».<br>**Lo que hacía (iii):** pasaba 15 diseños del corpus por `BuildCore`, el ensamblador y la barrera (`PushBackEditorOutputGate`), y esperaba `Ready`. Esa es la prueba de que la barrera no bloquea diseños válidos.<br>**Lo que queda:** ninguna prueba pasa el corpus por `PrepareFull`, y la barrera y `BuildCore` se quedan.<br>**Por qué no lo ve la cobertura:** es cobertura de datos, no de líneas | Declararlo como pérdida en la entrada «## PB-T4d» del registro. O bien añadir un equivalente sobre `PrepareFull` con `FullWriteKit`, junto a I-01 |
| M-02 | Menor | `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs:233` (`MirrorSelection`) | **Una rama que se queda pierde su cobertura en Core.**<br>**En la base:** la rama del lado activo B solo la cubría Core a través del `Copy` del comparador retirado (`PushBackPilotStateComparer.cs:76-81`, que repite `SetActiveSelection` con el lado activo B), en pruebas que se borran. Medido: en la base, 2/2.<br>**En el GREEN:** 1/2.<br>**Probablemente la cubre la UI** (la ventana clásica), pero no está medido | Declararla, o medirla con la cobertura de los filtros de UI, al aplicar el criterio ampliado de I-02. Puede ir en la misma pregunta al Owner que I-01 |
| M-03 | Menor | El RED: ayudantes que se quedan sin uso | **El RED deja sin uso ayudantes privados que la base sí usaba.** No los nombra la lista:<br>• **WPUR:** `HappyCalls`, `Boom`, `Cause` y `NamedScan`, y comentarios de sección huérfanos;<br>• **WPP:** `AssertHeld`, `Held`, `DraftHeld`, `WithExtra` y `YetAnotherBase`;<br>• **WPRH:** `Hold`;<br>• **WPUG:** `ComposeForWriteViolations` y `MaterializerPath`;<br>• **Materialization:** `ScalePath` y `Variant`;<br>• **WPEH:** `TextCorpus`;<br>• **en la UI:** `Fixture` (CanonicalLineEnding), `Texts` (ValidationParity), y `DesignNumber`, `Diff`, `Fixture` y `LoadInWindow` (WLC).<br>**No dan ningún `warning CS` medido.** Pero con C02 solo se podrán quitar en otra tarea con su propia lista | Añadirlos a la v5.3 como «borrar con registro (ayudante sin uso tras la retirada)». O declararlos para una limpieza posterior |
| M-04 | Menor | La lista: filas 46 a 52, 77 a 84, 137, 138, 217, 218 y 240, y la columna `porque` | **La lista no describe, fila a fila, lo que edita el RED:**<br>**(a) Las sobrecargas.**<br>• La fila 217, «DraftOf 74-80», y la 218, «Hold 126-128», cubren una sobrecarga cada una.<br>• El RED borra también `DraftOf(DraftStaleBase, string)` (WPP:82-90) y `Hold(DraftSession<PushBackPilotDraftState>)` (WPP:2055-2059), fuera de todo tramo.<br>• El tramo 126-128 es el `Hold` de `DraftForm`, que no nombra lo retirado: el porqué no vale.<br>**(b) Los ayudantes editados sin fila.**<br>• Las filas «reescribir» de E-3/E-5/E-6, de PEG (46-51), de M-04 (240), de H-06 (52) y de Ownership (77-84) apuntan a pruebas cuyo cuerpo no cambia.<br>• Lo que se edita son ayudantes sin fila propia: `E3Violations`, `E5Violations`, `HostViolations`, `WriteViolations` (WPEG:906), `AfterMatchViolations` y `Kinds`.<br>**(c) Los porqué de unas 38 filas C-7.** Dicen «solo fija lo retirado: sustituir con registro (C-7: …)», y su acción es «borrar con registro».<br>**La pieza del RED manda en C02,** pero el registro no dice qué se edita | Añadir filas, o una columna «tramo editado», para los ayudantes y las sobrecargas. Corregir los porqué |
| M-05 | Menor | El texto del GREEN y la prosa del paquete | **(a) Comentarios que siguen describiendo el piloto como presente:**<br>• el resumen de `PushBackPilotMaterializer` («…and composes the state to write»);<br>• `PushBackPilotUpdateRun.cs:49` del GREEN («serves the two models…: the one of the pilot and the full one»);<br>• `:142` y `:180` («same run as the one of the pilot», «as for the pilot»);<br>• `PushBackPilotUpdatePlan.cs:346` («same order as the one of the pilot»);<br>• `:202` y `:266` («empty in the pilot»).<br>**(b) La condición de `allowErase`:** D-PBT4-10 (línea 1008) y C13 dicen «la condición allowErase de sus tres ramas», pero el GREEN la quita en cuatro sitios: `NoAddress`, `ObsoleteLateral`, `FrontalBOfOneWay` y `AllObsolete`. `cov-p6-v52.txt` lista esas cuatro líneas de texto cambiado: 590, 592, 598 y 629 | (a) Opcional: ampliar D-S15 en la v5.3.<br>(b) Escribir «cuatro condiciones: tres en el bucle y la de `AllObsolete`» |
| M-06 | Menor | `PushBackRetirePilotRunTests.cs:19` (en el `NewFileSha`) y §8.5 | **El comentario de la clase dice «The other 22 tests of the original class are deleted».** Son 21 las borradas, y una más se queda (`TheDesiredNamesOfACompositeRack…`).<br>**La errata de la pregunta del Owner** («se borran 22») el paquete la aplica como 21 sin declararla. La declara el traspaso F4 | Corregir el comentario, que cambia de todos modos con I-01. Declarar la errata en §8.5 |
| M-07 | Menor | La precondición 6 (`driver-r6r-p6.txt`, `compile-check.md`) | **(a) La copia principal no corrió sola.** El paso de cobertura del GREEN de `r6r` (unos 04:04 a 04:14) se solapó con `r6s` (04:01 a 04:28), con `r6t` y con `r6u`, y su registro acaba con «procesos: 4».<br>• Los resultados no cambian.<br>• Las duraciones (593 s y 8,1 s) son orientativas.<br>**(b) Los «18 filtros» de `r6r` no son los del contrato.** Son los de PB-T4c sin `PushBackEditAllWindowTests`; su n.º 11 es el `PushBackFieldValidationConcordanceTests` retirado, con 0 resultados.<br>• `PushBackEditAllWindowTests` corrió en `r6u`, en las dos ramas.<br>• **Los 18 de PB-T4d sí corrieron todos,** pero no como dice el paso 4 | Declararlo en `compile-check.md` y en §6.4.1. En la v5.3, correr la copia principal sola y con los 18 filtros del contrato |

## Lo que comprobé y está bien

**1. La lista frente a las decisiones del Owner:**
- **El alcance B, más la rama muerta, más las 27.** Grupos y acciones (242 filas, 229 con acción y 13 que se quedan):
  - C-7: 77 + 27 + 7 + 27 = 138;
  - `original`: 55;
  - `apoyo`: 21 + 2 + 6 = 29;
  - `compilacion`: 12;
  - `ejecucion`: 3;
  - M-04: 3;
  - `lenta`: 1;
  - SUS-01: 1.

  Es lo que fija `owner-pbt4d-list-decisions.md`, pregunta 1. Las WPUR son 28 «sustituir» (las 27 y `StatusAfter`) y 21 «borrar».
- **Las 7 CT de PB-T3b y sus 6 ayudantes se quedan intactas.** Son las filas 3, 69 a 74 y 197 a 202. El RED no toca su tramo, y pasan en la suite completa del GREEN.
- **Ninguna prueba sale ni cambia sin su fila.** Lo comprobé por nombre y por tramo (`tests_rv.py` y `listcheck_rv.py`). Lo que cae fuera de toda fila es una de tres cosas:
  - una línea en blanco o una llave;
  - el atributo o el comentario contiguo de una fila borrada;
  - un ayudante de M-04 de este informe.

**2. El GREEN.** Lo apliqué con mi propio aplicador sobre los objetos git. Los cuatro sha256 que salen son los de `GreenFiles` del JSON.
- **Quita solo lo de D-PBT4-10:**
  - los dos archivos;
  - `ComposeForWrite`, `KeepPilotEditsOn` y `PushBackPilotComposition`;
  - `PushBackPilotPrepareInput`, `Prepare` y `Build`;
  - el `Precheck` del piloto, `Execute` y `ComparePost`;
  - en `BuildCore`, `allowErase`, la rama de `ViewUnavailable` y sus dos constantes, y el nombre del sobre releído;
  - los `cref` de D-S15.
- **El comportamiento de lo que se queda no cambia.** `PushBackViewDispositionKind` tiene cuatro valores (`PushBackViewDisposition.cs:13`). Con `allowErase` verdadero, `disposition != Redraw` tras las tres ramas era inalcanzable, y las otras cuatro condiciones y `memberName` se reducen a lo mismo.
- **Las referencias colgando:** en `src/` de la base, lo retirado solo aparece en los seis archivos del alcance. En el GREEN no queda ningún nombre retirado.
- **Las constantes que quita, `ViewUnavailablePrefix` y `ViewUnavailableSuffix`,** solo las usaba la rama muerta. En `tests/` solo las usaban pruebas que el RED borra o edita (WPP:1775, WPUR:205 y E-3).
- **Las constantes del materializador se quedan, con razón:** `PushBackFullComposition.cs:69`, `:75` y `:82` usan `InvalidScaleReason`, `BaseChangedReason` y `ComposeFailurePrefix`.
- **El censo de C13:** 57 quitadas (56 nombres, `StructuralEquality.Equal` x2), 0 nuevas y 0 `allowErase`.

**3. El RED.** Mi aplicador da los mismos sha256 que `RedFiles` y que los cinco `NewFileSha`: 26 existentes y 5 nuevos.
- **Las ediciones con registro conservan lo que protegían de lo que se queda:**
  - CT-PBT3B-05, 06 (i) y 09;
  - E-3, E-5 y E-6;
  - H-06, sobre `ForFullWrite`;
  - PEG `HostViolations`;
  - Ownership, con las filas «box»;
  - StaleBase, sobre `PushBackFullDraftReader.Read`;
  - `MaterializeReproduces…`;
  - los vacíos M-04 de PB-T4b (D-S13). No pasan en vacío: `Src.BodyOf` admite cuerpos de expresión y la firma `private DraftSession<PushBackFullDraftState>? FindPilotDraft` existe en `WorkspaceHostCore.cs:1841`.
- **Las 27 sustitutas** (`runeq_rv.py`): 25 son el mapa exacto de su original. La n.º 10 es la misma frontera (el `Invalid` de PREPARE dentro de la corrida da `NotAppliedPreflight` sin `PrepareUnits`), y la n.º 43 tiene los mismos casos con `ComparePostFull`.
- **Los equivalentes de D-S12:**
  - `PushBackRetirePilotPlanGuardTests`: sus originales, con el cambio declarado;
  - `PushBackRetirePilotFullCopyGuardTests`: el de PB-T4c, con solo los `BaseSha`, los recuentos y dos `CopySha` cambiados. Esos dos `CopySha` coinciden con mis sha del RED: `70d4f004…` para WPEGF y `ca88ddf8…` para WPPF.
- **La prueba lenta:** el bucle es `touch % 97 == 0 || touch == total - 1` y las aserciones no cambian. `Assert.Contains(… SystemFailurePrefix)` impide que pase en vacío.
- **Las dos CT fijan lo que dice §5.6:**
  - CT-01 tiene 35 líneas, con autopruebas por inyección. `ComparePost` y `Build(` no casan con `ComparePostFull`, `BuildFull` ni `BuildCore`;
  - CT-02 tiene 11 líneas. Su límite declarado: enmascara entera una cadena interpolada, agujeros incluidos, y la compilación cubre ese caso;
  - el control de `r6t`: en la base caen las dos, y en la variante A cae CT-01 y pasa CT-02.

**4. La cobertura,** en lo que mide:
- `cov-p6-v52.txt` es rama por rama, por conjuntos, no por cuentas;
- las siete ramas del alto, la 525 y la 594 están igual en las dos ramas;
- mi comparación de toda la suite de Core lo confirma en los dos archivos;
- el fallo es de universo (I-02).

**5. El contrato literal de §6.4:**
- `AllowedWriteScope`: 37 rutas = 6 de `src/` + 26 existentes + 5 nuevas, iguales a las de las piezas;
- `UNIT_CHANGE`: 6;
- `StopConditions`: 17 estándar + 17 propias = 34;
- `RequiredTests`: 88 (83 heredados y 5 nuevos). Las bajas de `MinSelected` siguen «base menos GREEN», con 0 solapamientos;
- **los totales cuadran por aritmética** desde la base: se quitan 237 resultados (61 omitidos y 176 aprobados), 3 pasan a omitidos y se añaden 34:
  - Core GREEN 19 416 = 19 013 + 0 + 403;
  - Core RED 19 416 = 19 012 + 1 + 403;
  - UI 2 464 = 2 416 + 0 + 48.

  Las 22 bajas de UI están todas dentro de los filtros: `RackCad.UI.Tests.Workspace` casa como subcadena con las clases `WorkspaceDraftPilot*`.
- **Las secciones que no cambian:** §5.1 a §5.3, §6.1 a §6.3 y el anexo V51-A son iguales a la v5.1. §6.0 solo añade filas de PB-T4d.

**6. La precondición 6:**
- **La base local,** `r6s` (19 619 = 19 158 + 0 + 461), casa con la CI del cierre de PB-T4c tal como la registra el repo en `6c2820ba`:
  - `f4b-dispositions.md:2032`;
  - `superseding-characterization.md:511`;
  - el `controller-verification.json` de nc1 de PB-T4c: «core current run 19619/19158/0/461».
- **La UI fuera de los filtros:**
  - el GREEN no toca UI ni el Plugin;
  - el RED solo borra pruebas de UI dentro de los filtros;
  - las pruebas de UI que leen `src/` solo leen `RackCad.UI` o `RackCad.Plugin`;
  - las que nombran tipos del piloto están todas en clases de los filtros;
  - no hay reflexión por nombre sobre lo retirado en UI.

  El riesgo que queda es bajo, y lo cubre la CI completa de UI (C06/C16).

## Lo que no verifiqué

- **La CI real (`44959653142`) en GitHub.** Solo lo que el repo registra de ella; no consulté la red.
- **La suite completa de UI y la cobertura de UI.** Por eso M-02 queda como «probablemente cubierta».
- **No recalculé desde los TRX** los recuentos por filtro ni los totales. Usé los registros de `r6r`, `r6s` y `r6u`, y la aritmética.
- **El texto B medido en la copia `r6e`,** frente a la parte B del GREEN.
- **El cálculo interno de los `BaseSha`** del equivalente de CT-PBT4B-20. Solo vi su diff con el original y que casa con mis sha del RED.
- **Que las corridas se solaparan** lo deduzco de las horas de los archivos.
- **Un desliz mío:** lancé por error un `python -I -c "print(1)"`, que no leyó nada. No se repitió.

## Decisiones nuevas del Owner

**Hace falta una, por I-01:**
- **la pregunta:** sustituir la 48 y la 49 de WPUR (29 sustitutas) o declarar esa pérdida de prueba de `PushBackPilotSystemRebuild`;
- **lo que puede ir en la misma vuelta:** M-01 (el barrido del corpus de CT-PBT3B-06 (iii)) y M-02 (`MirrorSelection` con el lado B), como pérdidas declaradas o como sustitutas;
- **lo demás** se corrige en la v5.3 sin el Owner.

> EMITIBLE TRAS CORRECCIONES; B 0 / I 2 / M 7; las importantes a corregir en una v5.3 (I-01: las pruebas 48 y 49 de WPUR prueban PushBackPilotSystemRebuild, que se queda, y la lista las borra; I-02: el criterio de cobertura solo mira 436 lineas de 2 archivos); con una decision nueva del Owner (I-01, por su regla de owner-pbt4d-run-coverage-decision.md: sustituir la 48 y la 49, 29 sustitutas, o declarar la perdida); precondicion 6 sobre 6c2820ba: base 19619 = 19158 + 0 + 461 igual a la CI 44959653142 registrada, RED 19416 = 19012 + 1 + 403, GREEN 19416 = 19013 + 0 + 403, UI 2464 = 2416 + 0 + 48, las 436 lineas PASAN pero la suite completa de Core pierde PushBackPilotSystemRebuild.cs:32 y 6 ramas (I-01) y 1 rama de PushBackCompositeEditorState.cs:233 (M-02); GREEN y RED reproducidos por objetos git con los mismos sha256

## Recomprobación 1 (v5.3)

**Fecha:** 2026-11-22. Lo anterior de este archivo no cambia: son sus primeros 21 743 bytes, sha256 `439837f9…dced3`, la versión que cita el Owner.

**Las reglas de lectura fueron las mismas:**
- solo objetos git de `6c2820ba`;
- nada en el worktree;
- sin compilar ni ejecutar pruebas;
- sin escribir en el repositorio.

Mis guiones están en `rv4d/`. Los de esta vuelta son:
- `apply_red_rv53.py`: el RED, con los bloques DELETE;
- `codeonly_cmp_rv.py`: el GREEN sin comentarios, frente al de la v5.2;
- `cov53_rv.py` y `cov53c_rv.py`: la cobertura reproducida desde los JSON de coverlet;
- `listcheck53_rv.py`;
- `p6json_rv.py`.

**Las entradas, con su sha256 comprobado:**

| Pieza | sha256 |
|---|---|
| El paquete, `pbt4/I-64-f4b-pbt4-gate-contract-package-v53.md` | `ae338b2d…9daf` |
| El GREEN, `pbt4/pbt4d-green-v53.txt` | `51a40209…1436` |
| El RED, `pbt4/pbt4d-red-v53.txt` | `de0d3a0e…c24e` |
| La lista, `pbt4/pbt4d-retire-list-270.tsv` | `7e65c9e4…081f` |
| Las 29 sustitutas, `v53/run/PushBackRetirePilotRunTests.cs` | `3554df0d…fe69` |
| `pbt4d-precondition6.json` | `4cc06549…5567` |
| `compile-check.md` | `58455b41…aa0e` |
| `cov-p6-v53.txt` | `8090f08a…6615` |
| D-S16, en `session-pbt4d-design-decisions.md` | `27010f8f…a52e` |
| `owner-pbt4d-cat4d-decisions.md` | `fa07db38…03fa1` |

**Lo que no casa:**
- **La decisión del Owner:** el mensaje del coordinador y `v53/handoff-pbt4d-v53.md:9` le dan una cola errónea, «…0d75», que es la de otro registro. El paquete cita bien el hash entero (`v53.md:8`).
- **D-S11 a D-S15 siguen intactos:** los primeros 7 457 bytes de `session-pbt4d-design-decisions.md` dan el sha256 `c5ddb937…80f2` de la v5.2.

**Veredicto:** EMITIBLE, B 0 / I 0 / M 6 (de M-08 a M-13). Los nueve de la v5.2 están resueltos o declarados, y ningún menor nuevo pasa de menor.

### Los hallazgos de la v5.2, uno a uno

| Id | Estado | Evidencia |
|---|---|---|
| **I-01** | **Resuelto** | **Las sustitutas** (`v53/run/PushBackRetirePilotRunTests.cs:530-541` y `:549-602`): con mi propia comparación contra el original más el mapa de PB-T4b (`runeq_rv.py`), 26 de las 29 son el mapa exacto, y entre ellas la n.º 48. La n.º 49 solo cambia la línea de comentario sobre el lector; sus aserciones, el mapa exacto. La 10 y la 43 son las declaradas.<br>**`PushBackPilotSystemRebuild` en el GREEN** (`r6y-p6/core-green-cov.coverage.json`, reproducido con `cov53_rv.py`):<br>• la línea 32 y las ramas de `:28`, `:29`, `:31` (las 4), `:48`, `:51` y `:60` las cubre Core;<br>• de sus 37 claves instrumentadas, solo queda sin cubrir la rama verdadera de `:35` (`NotPushBackReason`), que **tampoco la cubría la base**. No es una pérdida, pero «entero» (`v53/handoff-pbt4d-v53.md:24`) exagera.<br>**Las filas 130 y 131:** pasan a «sustituir con registro», con el porqué verdadero.<br>**`MinSelected` del n.º 88:** pasa de 29 a 31. Es la única diferencia de `required-pbt4d.tsv` frente a `required-pbt4d-v52.tsv` |
| **I-02** | **Resuelto** | Lo detallo en el apartado siguiente |
| M-01 | Aplicado | CT-PBT3B-06 (iii), en su sitio:<br>• los 15 diseños se pasan como borradores completos (`FullWriteKit.Read`, `DraftOf`, `Edit`) por `PrepareFull`;<br>• las líneas esperadas son las mismas;<br>• `admitido` sale ahora del lector completo;<br>• (ii) es lo único que sale (diff de `PushBackFullWritePrepareTests.cs:569-593`).<br>Es la única sustituta nueva sobre las 29 |
| M-02 | Resuelto por medida | `PushBackCompositeEditorState.cs:233` (0,0):<br>• en la base, Core la cubre, con las tres corridas juntas y 7 filtros en su proceso;<br>• en el GREEN no la cubre Core, pero sí las tres juntas y los mismos 7 filtros, cada uno en su proceso.<br>La cubre la UI, como pide la regla, y no necesita sustituta |
| M-03 | Aplicado | 32 filas de ayudantes (243 a 274) y 3 archivos con bloque DELETE (275 a 277). Mi búsqueda de ayudantes privados que el RED deja sin uso (`dead_rv.py` sobre el árbol del RED v5.3) no da ninguno. Queda texto suelto (M-12) |
| M-04 | Aplicado | **Las columnas nuevas:** `declaraciones-6c2820ba` (217: «74-80, 82-90»; 218: «126-128, 2055-2059») y `tramo-editado`.<br>**Las filas nuevas:** 6 `ayudante-editado` (278 a 283).<br>**Los porqué de C-7:** ya no dicen «sustituir» (0 filas).<br>**Mi comprobación** (`listcheck53_rv.py`): todo cambio de código del RED cae en una fila, salvo un `using` y un artefacto de difflib (M-12) |
| M-05 | Aplicado | **(a) Los comentarios:** el GREEN v5.3 es el de la v5.2, con sus 25 bloques intactos y en orden, más 11 bloques nuevos. **Sin comentarios, los cuatro archivos dan el mismo código que en la v5.2** (`codeonly_cmp_rv.py`).<br>**Los sha256** de los cuatro archivos del GREEN, aplicados por mí, son los de `GreenFiles` del JSON.<br>**(b) Las cuatro condiciones:** C13 (`v53.md:2904`) y D-PBT4-10 ya dicen «cuatro condiciones» |
| M-06 | Aplicado | **El comentario de la clase:** «Of the other 20 tests of the original class, 19 are deleted with their record and one stays in it», en `PushBackRetirePilotRunTests.cs:21-22`.<br>**La errata:** declarada en §8.5 |
| M-07 | Aplicado | **La copia principal corrió sola** (`driver-r6y-p6.txt`): 0 procesos al empezar (08:06:00) y al acabar (08:47:37). Las corridas de la base, `r6w` (06:57 a 08:05), terminan antes, y el control, `r6z`, empieza después.<br>**Lo que coincidió:** a las 08:11 se escribió un análisis en Python (`r6x-t2/cov-r6x-5.txt`). No es `dotnet` y no cambia nada.<br>**Los filtros:** los 18 del contrato (`ui_filters_pbt4d_v53.txt`).<br>**Lo de `r6r`:** declarado en `compile-check.md:13-15` |

### I-02: el método de `cov_p6_v53.py`, y su reproducción

**El universo.** Todo `src/` que instrumenta coverlet: Domain, Application y UI, 775 archivos en la base y 773 en el GREEN, que ya no tiene los 2 borrados.
- **En los dos lados entra lo mismo:**
  - la suite entera de Core;
  - las tres corridas de todos los filtros juntos (`ui_union_v53.txt`, el mismo texto en los dos lados);
  - una corrida por filtro, en su proceso.
- **La puerta** (`cov_p6_v53.py:155-194`) compara, línea a línea y rama por rama, la unión de todo en la base con la unión de todo en el GREEN.
- **Lo que hace con cada línea:**
  - la lleva al GREEN por difflib, solo en los 4 archivos que cambian;
  - exige todas las ramas a las 4 líneas de texto cambiado y a la línea nueva.

**La simetría.** La unión es simétrica, salvo por el filtro de la clase que se borra entera, `PushBackFieldValidationConcordanceTests`:
- **está en la base** (19 filtros) y no en el GREEN (18);
- ese desequilibrio solo puede añadir pérdidas, no taparlas;
- **las 118 líneas o ramas que solo bajan por él** se tratan aparte.

Las corridas se casan por el texto del filtro, no por su número, así que el orden distinto de las dos listas no importa.

**Lo reproduje yo** con `cov53_rv.py`, a partir de los 43 JSON de `r6w-bc` y de `r6y-p6`, sin usar `cov_p6_v53.py`:

| Lo medido | Resultado |
|---|---|
| Líneas que se quedan | 69 838 (el autor dice 69 842: hay 4 líneas con ramas cubiertas y sin golpes de línea) |
| Ramas que se quedan | 28 818, igual que el autor |
| Retiradas | 419 líneas, igual que el autor |
| Perdidas en la unión | 0 |
| Lo que Core sola pierde | solo `PushBackCompositeEditorState.cs:233` (0,0) |
| PERDIDA, a la manera de D-S16 | 0 |
| «Bajan», sin emparejar | 16 919 = 16 801 + 118, como el autor |
| Claves que en el GREEN solo cubren las corridas juntas | 0: ni Core ni ningún filtro en su proceso las cubre |

**El filtro `RackCad.UI.Tests.Workspace`** (`cov53c_rv.py`). Lo que su corrida cubría en la base y no en el GREEN:
- 14 501 claves las cubre Core en el GREEN;
- 2 145, las tres juntas y al menos otro filtro en su proceso;
- ninguna queda solo en las juntas, ni solo en un filtro a ratos, ni sin nada.

**La clase que se borra entera:** de lo que solo cubría ella de más, 191 claves las cubre Core en el GREEN y 31, las juntas y otro filtro.

**Ninguna entrada de «Bajan» esconde una pérdida real.**

**Las definiciones** (`cov_p6_v53.py:248-251`) están bien aplicadas:
- PERDIDA: estable en la base (Core, o las tres juntas) y no en el GREEN;
- VARIA: no estable en ninguno de los dos lados;
- ESTABLE: estable en el GREEN.

Hay un límite, que no se da en esta medida: ESTABLE admite «3/3 juntas» sin ningún filtro en su proceso, así que en principio aceptaría una cobertura que dependa del orden (M-11).

**Las líneas 37 y 38 de `RackViewBatchDialogPresenter` son ruido:**
- las cubre la misma prueba, CT-14 de `PushBackClassicCharacterizationOwnerTests`, que no cambia, y son código de UI que el GREEN no toca;
- con su filtro solo, la base da 1, 0 y 1, y el GREEN 1, 1 y 1 (`driver-noise-batch.txt`, cuyo sha256 lleva `Noise.Driver`);
- en `r6w` y en `r6y`, los dos lados dan 1/3 juntas más su filtro: **no bajan**.

### Los tres desajustes del generador

| # | Lo que dice el generador | Confirmado | Gravedad | ¿Basta el arreglo? |
|---|---|---|---|---|
| 1 | La fila de la precondición 6 de §6.4.2 (`v53.md:3078`) nombra `GreenUiCoverage`, y el JSON no la tiene | **Sí.** Es la única clave del esquema que falta en el JSON | Menor (M-08) | **No del todo.**<br>**Lo que sí está en el JSON:**<br>• las tres corridas juntas del GREEN: `GreenWithCoverage.UI.Trx` (`1d89e956…`), con su JSON en `Coverage.GreenUiJson` (`518b76a0…`), y `UIRepeats[0..1]`, con `Trx` y `Json`;<br>• las 18 corridas por filtro: solo su TRX (`Green.UI[n].Trx`).<br>**Lo que no está:** el sha256 de los 18 `green-uiNN.coverage.json`, que no aparece ni en el JSON ni en `compile-check.md` ni en `driver-r6y-p6.txt`. Son entradas del PASA (la unión de D-S16).<br>**En la base:** los 19 JSON por filtro solo quedan como prefijos de 16 caracteres en `driver-uif-r6w.txt` (cuyo sha256 completo lleva `BaseCoverage.UIByFilter.Driver`).<br>**Lo que propongo:** en vez de quitar `GreenUiCoverage`, llenarla con los sha256 de los JSON de cobertura de UI del GREEN (las 3 juntas y los 18 por filtro), o añadir `Json` a cada `Green.UI[n]`, y poner el sha256 completo en `BaseCoverage.UIByFilter.Runs` |
| 2 | `RedDeleted` y `Noise` están en el JSON y no en la lista | **Sí:**<br>• `RedDeleted` son los 3 archivos de los bloques DELETE;<br>• `Noise` lleva `Driver` = sha256 de `driver-noise-batch.txt`.<br>Fuera de las claves de metadatos (`Schema`, `Package` y `Task`), no sobra ni falta ninguna otra | Menor (parte de M-08) | **Sí,** basta añadir las dos: `RedDeleted`, comparada con los DELETE, y `Noise`, como dato informativo |
| 3 | [13] de §6.4.1 (`v53.md:2935`) dice «D-S11 a D-S15 y lo que la sesion fijo tras CA-T4d» | **Sí.** No es falso, pero es impreciso, y es un texto literal del contrato | Menor (M-09) | **Sí:** «D-S11 a D-S16» |

**Ninguno de los tres pasa de menor.** El 1 no cambia el PASA, que reproduje desde esos mismos archivos, pero deja sin custodiar por hash entradas de la medida: el arreglo tiene que fijarlas, no solo quitar el nombre.

### Los menores nuevos

| Id | Gravedad | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| M-08 | Menor | §6.4.2 (`v53.md:3078`) y `pbt4d-precondition6.json` | Los desajustes 1 y 2 de la tabla anterior | El arreglo de la tabla: llenar `GreenUiCoverage` con los sha256, o añadir `Json` a `Green.UI[]`, y añadir `RedDeleted` y `Noise` a la lista |
| M-09 | Menor | `v53.md:2935` ([13]) | El desajuste 3 | «D-S11 a D-S16» |
| M-10 | Menor | La cabecera, `v53.md:11` («Lo que cambia») | **No nombra §0, §1, §6.0, §7 ni §9, y también cambian:**<br>• §0, la fila de PB-T4d: 270 filas y 29 sustitutas;<br>• §1, D-S16, CA-T4d y la 14.ª decisión;<br>• §6.0, RR-2 (borra 3);<br>• §7, R-T4-34 a 38;<br>• §9, las copias de la v5.3.<br>Los cambios son coherentes con las filas V53, pero la tabla V53 tampoco los nombra en su columna «Dónde» | Completar «Lo que cambia» |
| M-11 | Menor | D-S16 y `cov-p6-v53.txt` («Bajan»), `cov_p6_v53.py:248-251` | **(a) D-S16 pide «la prueba que la cubría».** La entrada nombra la corrida (el filtro), porque coverlet no da la prueba, y no se declara.<br>**(b) ESTABLE acepta «3/3 juntas» sin ningún filtro en su proceso.** Medido aquí: 0 casos | Declarar en D-S16 o en §6.4.1 que la entrada nombra la corrida. En la definición de ESTABLE solo por la UI, exigir también un filtro en su proceso, o declarar que ese caso se comprobó y dio 0 |
| M-12 | Menor | El texto que queda | **(a) WPUR en el RED:** el resumen de la clase (`:13-15`) aún describe «the run of the update of the pilot», y quedan comentarios huérfanos de las pruebas 48 y 49 (`:50` y `:52-54`). Son solo comentarios.<br>**(b) La pieza del GREEN v5.3** dice en su cabecera «Lo aplica green_pbt4d_v52.py» (`pbt4d-green-v53.txt:6`).<br>**(c) El porqué de la fila 282** (`WriteViolations`) arrastra el texto de `HostViolations`.<br>**(d) El `using RackCad.Application.Systems.PushBack.Draft;`** que el RED añade en `WorkspaceDraftPilotIntentStaleBaseTests.cs:6` no cae en ninguna fila, aunque §4.4 dice que toda línea de código que el RED cambia cae en una | En la v5.4 de solo texto: quitar los comentarios huérfanos, corregir la cabecera de la pieza y el porqué de la 282, y o bien dar una fila o un tramo al `using`, o bien matizar la frase de §4.4 |
| M-13 | Menor | V53-02, §4.4 y §6.4.1 («conjunto vigilado de la v5.2: 537 lineas de WPUR, 433 se quedan») | La v5.2 decía 436 líneas, con 433 llevadas al GREEN. La v5.3 cuenta 537, porque ahora incluye las líneas de WPUR que caen dentro de los tramos que el GREEN borra entero. La base del recuento cambió sin decirlo | Una frase que lo explique |

### La regla del Owner

- **Ningún camino que se queda pierde su única prueba sin sustituta.**
  - En la unión: 0 líneas y 0 ramas perdidas.
  - Lo único que Core deja de cubrir es la rama de `:233`, y la cubren 7 filtros de UI, cada uno en su proceso.
  - El corpus (M-01), que es cobertura de datos, se resolvió con una sustituta.
- **Las sustitutas nuevas sobre las 29:** 1, así que 10 o menos.
- **Ninguna se dejó de escribir.**

### El diff de la v5.2 a la v5.3

**Cambia lo de la tabla V53** (`v53.md:21-36`):
- D-PBT4-10, §4.4, §5.6, §6.4, §6.4.1 y §6.4.2, §8.3, §8.5 y §11;
- el anexo, que pasa a V53-A;
- y, sin nombrarlos en la cabecera, §0, §1, §6.0, §7 y §9 (M-10).

**No cambian:**
- §5.1 a §5.3 y §6.1 a §6.3;
- el anexo V51-A;
- D-S1 a D-S15.

**Los cambios de la lista, de 229 a 270 filas con acción:** son 41 más.
- 35 son del grupo M-03 (V53-05).
- 6 son `ayudante-editado` (V53-06).
- **Las filas 130 y 131** cambian de acción (V53-01), sin cambiar la cuenta.

**Los totales:** +2 en Core, por I-01. **Los filtros:** los 18 del contrato (V53-09). Cada cambio tiene su fila V53.

### La precondición 6

**La base sigue casando con la CI del cierre de PB-T4c:**
- Core: 19 619 = 19 158 + 0 + 461 (`r6w-bc/base-core-cov-res.txt`);
- UI, la unión de filtros: 921 = 890 + 0 + 31.

**En `r6y`** (`driver-r6y-p6.txt`):
- **Compilación:** 5 compilaciones con 0 errores y 0 `warning CS`.
- **El RED:** Core 19 418 = 19 014 + 1 + 403, y solo cae CT-PBT4D-01, por `Assert.Equal`.
- **El GREEN:** Core 19 418 = 19 015 + 0 + 403.
- **La UI:** los 18 filtros del contrato, 0 `Failed` en las dos ramas. UI de la CI: 2 464 = 2 416 + 0 + 48 en las dos.

**`RequiredTests`:** 88; solo cambia el n.º 88, de 29 a 31.

**El control, en `r6z`:**
- con la base y las dos CT, caen las dos;
- con la variante A, cae CT-01 y pasa CT-02.

**El censo:**
- `census-c13-v53.txt` es igual al de la v5.2, salvo el nombre de la pieza;
- 57 quitadas, 0 nuevas y 0 `allowErase`.

**Nada cae fuera de la lista.** Con mi comprobación por nombre (`tests_rv.py` sobre el RED v5.3), ninguna prueba sale sin su fila.

**La copia principal corrió sola** (M-07).

### Lo que no verifiqué

- **La CI real en GitHub.**
- **La UI fuera de los filtros.**
- **Los totales,** que no recalculé desde los TRX: me fié de los registros de `r6w` y de `r6y`.
- **Los 18 JSON de cobertura por filtro del GREEN:** los usé tal como estaban en la carpeta, porque no hay sha que los fije (M-08).

**Mis deslices:**
- añadí con un heredoc de Bash un trozo a un guion mío de lectura (`cov53b_rv.py`) y retoqué con `sed` otros dos (`secdiff_rv.py` y `apply_red_rv53.py`), en lugar de escribirlos con Write;
- todos están en `rv4d/` y solo leen;
- no se ejecutó Python fuera de `python -I <archivo>`, ni se tocó el repositorio.

### Decisiones nuevas del Owner

**No hace falta ninguna.**
- La regla permanente cubre lo medido: 1 sustituta nueva, y ningún camino perdido.
- **Los seis menores** pueden ir en la v5.4 de solo texto que propone la sesión. Para M-08, conviene fijar los sha256 de las corridas de UI, no solo quitar el nombre.

> ; recomprobacion v5.3: EMITIBLE; B 0 / I 0 / M 6 (M-08 a M-13, para una v5.4 de solo texto); I-01 resuelto (29 sustitutas, 26 por el mapa; PushBackPilotSystemRebuild.cs:32 y sus 6 ramas cubiertas por Core en el GREEN) e I-02 resuelto (cobertura de todo src/ reproducida desde los 43 JSON de coverlet: 0 lineas y 0 ramas perdidas, Core sola solo pierde PushBackCompositeEditorState.cs:233, que cubren 7 filtros de UI en su proceso, PERDIDA 0, bajan 16801 + 118, ninguna solo por las corridas juntas); M-01 a M-07 aplicados; 1 sustituta nueva sobre las 29; sin decisiones nuevas del Owner; los tres desajustes del generador son menores, y el 1 pide fijar los sha256 de los 18 JSON de UI por filtro del GREEN, no solo quitar GreenUiCoverage; precondicion 6 sobre 6c2820ba: base 19619 = 19158 + 0 + 461 igual a la CI, RED 19418 = 19014 + 1 + 403, GREEN 19418 = 19015 + 0 + 403, UI 2464 = 2416 + 0 + 48, copia principal sola, 0 warning CS

## Recomprobación 2 (v5.4, solo el diff)

**Fecha:** 2026-11-22. **Revisor independiente y adversarial:** no participé en el paquete ni en las revisiones anteriores. Lo anterior de este archivo no cambia: son sus primeros 39 559 bytes, sha256 `76a10c8b…679f`, la versión que cita la v5.4 (`v54.md:5`).

**Las reglas de lectura:**
- el código, solo por objetos git de `6c2820ba`, desde el checkout principal (`git show` y `git grep`). `git diff --no-index` solo lo usé entre archivos del paquete;
- nada en el worktree;
- sin compilar ni ejecutar pruebas;
- Python solo como `python -I <archivo>`, con guiones escritos con Write en `rv4d2/` (uno, retocado con Edit);
- sin escribir en el repositorio ni fuera de `rv4d2/`, salvo esta sección.

**Mis guiones**, en `rv4d2/`, todos de solo lectura:
- `pkgdiff_rv2.py` y `pkgdiff_words_rv2.py`: el diff de la v5.3 a la v5.4 del paquete, por líneas y por palabras, en 45 tramos;
- `p6json_shape_rv2.py`, `p6json_diff_rv2.py` y `redfiles_rv2.py`: las claves del JSON, su diff hoja a hoja y la fila del esquema de §6.4.2;
- `p6json_sha_rv2.py` y `driver_prefix_rv2.py`: el sha256 de las TRX y de los JSON de cobertura de `r6y-p6/` y de `r6w-bc/`, y los prefijos que escribieron los conductores;
- `pieces_rv2.py`: el sha256 de cada pieza que nombra el JSON;
- `m11_green_only_rv2.py`: lo que en el GREEN solo cubren las corridas juntas;
- `apply_red_rv2.py`, `apply_green_rv2.py` y `usings_rv2.py`: mi propio aplicador de las piezas sobre los objetos git, el código sin comentarios y las directivas `using`;
- `annex_green_rv2.py`: el GREEN del anexo frente a la pieza, y la lista por grupo y acción;
- `prefix_sha_rv2.py`: qué prefijo de un archivo da un sha256 dado.

**Las entradas, con el sha256 que calculé yo:**

| Pieza | v5.3 (historia) | v5.4 |
|---|---|---|
| El paquete | `ae338b2d…9daf` | `e37843fc…fc63` (869 447 bytes) |
| El RED | `de0d3a0e…c24e` | `80f8cc0c…81db` |
| El GREEN | `51a40209…1436` | `aaf6cab8…fbf0` |
| La lista | `pbt4d-retire-list-270.tsv`, `7e65c9e4…081f` | `pbt4d-retire-list-271.tsv`, `05a4d3d1…75ac` |
| Las ediciones con registro | `pbt4d_test_edits_v53.py`, `81f62c6a…7182` | `pbt4d_test_edits_v54.py`, `b8c78f72…0aea` |
| El JSON de la precondición 6 | `pbt4d-precondition6-v53.json`, `4cc06549…5567` | `pbt4d-precondition6.json`, `52e03e06…8989` |
| `compile-check.md` | `compile-check-v53.md`, `58455b41…aa0e` | `d13afdb0…de42` |
| La cobertura | `cov-p6-v53.txt`, `8090f08a…6615`; `cov_p6_v53.py`, `6495db6e…6552` | `cov-p6-v54.txt`, `9c1bde53…bd34`; `cov_p6_v54.py`, `8d6aaccc…03b6` |

Y, de la v5.4:
- **la reaplicación:** `driver-r70-ra.txt`, `2044f0fc…5f36`; `p6_reapply_v54.sh`, `e4d3a0dd…c71d`; y `r70-ra/apply-red.txt`, `82167d2b…24df`;
- **los guiones del autor:** `v54/make_red_v54.py`, `7ed0074e…268f`; `v54/make_list_v54.py`, `5e6acc32…dc05`; `v54/codeonly_v54.py`, `09c9f3f0…6384`; `v54/codeonly-v53-v54.txt`, `401a246b…c05d`; `v54/edit_frags_v54.py`, `8c4b3a41…e476`; y `make_v54.py`, `31251f35…a1eb`;
- **el traspaso,** `v54/handoff-pbt4d-v54.md`: `6822608d…d13e`;
- **D-S16 con su nota,** `session-pbt4d-design-decisions.md`: `bd02d210…01c8`.
  - Sus primeros 9 098 bytes dan `27010f8f…a52e`, el de la v5.3.
  - Sus primeros 7 457 dan `c5ddb937…80f2`, el de la v5.2.
  - La nota son los 762 bytes del final;
- **los conductores de las corridas:** `driver-r6y-p6.txt`, `69a31f28…c6fa`; y `driver-uif-r6w.txt`, `b3b829cb…e40f`.

Todos casan con los que da la sesión.

**Veredicto:** EMITIBLE, B 0 / I 0 / M 3 (M-14 a M-16). Los seis menores de la recomprobación 1 están resueltos. Los tres nuevos son de texto y de custodia, y ninguno cambia el contrato, la medida ni la lista.

### Los seis menores de la recomprobación 1

| Id | Estado | Evidencia |
|---|---|---|
| M-08 | **Resuelto** | **El JSON** (`52e03e06…8989`):<br>• `GreenUiCoverage.Union`: las 3 corridas juntas (`ui-green-cov`, `-r-1` y `-r-2`), con `Trx` y `Json`;<br>• `GreenUiCoverage.ByFilter`: los 18 filtros, con `Trx` y `Json`;<br>• `Green.UI[n].Json`: las 18, iguales a `ByFilter`;<br>• `BaseCoverage.UIByFilter.Runs`: las 19, con `Trx` y `Json` de 64 caracteres.<br>**Mis sha256** (`p6json_sha_rv2.py`), sobre `r6y-p6/` y `r6w-bc/`:<br>• casan los 42 del GREEN (3 + 18, TRX y JSON) y los 38 de la base (19, TRX y JSON);<br>• casan también `Coverage.GreenCoreJson`, `BaseCoverage.CoreJson`, `UiJson` y `UIRepeats`;<br>• cada TRX por filtro solo tiene pruebas de su filtro, en el orden de `ui_filters_pbt4d_v53.txt` y de `ui_filters_base19_v53.txt`.<br>**Contra lo que escribieron los conductores** (`driver_prefix_rv2.py`): casan con los sha completos los 19 prefijos `json=` de `driver-uif-r6w.txt:2-20` y los 4 de `driver-r6y-p6.txt` (Core y las 3 juntas). De los 18 JSON por filtro del GREEN no hay registro de la corrida (lo digo en «Lo no verificado»).<br>**El esquema** (`v54.md:3105`):<br>• nombra las 32 claves de primer nivel del JSON, en su orden, sin que falte ni sobre ninguna;<br>• `RedDeleted` es, como conjunto, los 3 bloques DELETE del RED v5.4;<br>• `Noise` va como informativo, con «el generador no lo lee» |
| M-09 | **Resuelto** | `v54.md:2962`: «…y de la sesion (D-S11 a D-S16)» |
| M-10 | **Resuelto** | **La cabecera de la v5.3** (`v54.md:37`) nombra §0, §1, §6.0, §7 y §9.<br>**La columna «Dónde»:** V53-01 «§0; §1» (`:51`); V53-05 «§6.0 (RR-2); §7 (R-T4-36)» (`:55`); V53-09 «§7 (R-T4-37)» (`:59`); y V53-10 «§9» (`:60`).<br>**Queda un resto, sin consecuencia:**<br>• V53-02 pone «§1: D-S16; §7: R-T4-34 y R-T4-38» en «Qué cambia» (`:52`), no en «Dónde»;<br>• R-T4-35 (`:3129`, que cita V53-01) no lo nombra ninguna fila.<br>**El mismo modo vuelve en la cabecera de la v5.4** (M-14) |
| M-11 | **Resuelto** | **(b) La regla:** `cov_p6_v54.py:290` dice `stable_green = gcore or (gun == gut and bool(gfil))`, donde `gfil` son los filtros del GREEN que la cubren en su proceso (`:260`, `:279`).<br>**Su diff con `cov_p6_v53.py`** es solo eso: la línea 1 (la versión), el comentario de ESTABLE (`:251-252`), la `:290` y el texto del informe (`:328`).<br>**`cov-p6-v54.txt`:**<br>• «cobertura PASA» (`:4`);<br>• «entradas 16801: PERDIDA 0, VARIA 0, ESTABLE 16801; y […] 118 lineas o ramas en 3 archivos» (`:52`);<br>• su diff con `cov-p6-v53.txt` son solo la `:2` (la pieza) y la `:51` (la definición), así que las ESTABLE salen iguales línea a línea.<br>**Mi comprobación** (`m11_green_only_rv2.py`, sobre los 22 JSON de `r6y-p6/`): en el GREEN no hay ninguna línea ni rama que cubran las tres juntas sin Core ni filtro en su proceso (0), ni ninguna que solo cubran a ratos las juntas (0). Sin depender de cómo se lleva la base al GREEN, **la regla nueva no puede reclasificar ninguna entrada, ni las 118**.<br>**(a) «Nombra la corrida, no la prueba»:** `cov-p6-v54.txt:51`, `v54.md:1606`, `v54.md:3233`, `compile-check.md:30` y la nota de D-S16.<br>**D-S16** se cita con `bd02d210…01c8` (`v54.md:7`).<br>**Quedan restos de texto:** M-15 |
| M-12 | **Resuelto** | **(a) Mi aplicador** (`apply_red_rv2.py`), con los dos RED sobre los objetos de `6c2820ba`:<br>• salen 28 archivos y los mismos 3 borrados; 27 tienen el mismo texto, con los sha256 de `RedFiles`;<br>• **solo cambia WPUR:** el resumen de la clase (líneas 13 a 15 del RED) y, tras la `}` de la prueba que se queda, 2 líneas en blanco y las 4 de comentario de la n.º 48 y la n.º 49;<br>• **sin comentarios ni líneas en blanco, los 28 dan el mismo código**;<br>• el sha256 de WPUR en el RED v5.4, `7ea61704…ba13`, es el que imprime `r70-ra/apply-red.txt`.<br>**En la pieza:**<br>• la edición del resumen es un bloque nuevo (`pbt4d-red-v54.txt:3781-3790`);<br>• la de los comentarios va fundida con las bajas de la 48 y la 49 en un solo bloque (`:4644` en adelante), porque el anexo las aplica «sobre el texto ya sin las bajas» (`v54.md:5495`);<br>• las dos piezas tienen 119 EDIT, 3 DELETE y 5 NEWFILE.<br>**(b) El GREEN:**<br>• su diff con el v5.3 son solo las líneas 1 y 6, de cabecera (`@@@@ #`);<br>• mi aplicador (`apply_green_rv2.py`) da 34 EDIT y 2 DELETE, y los 4 sha256 de `GreenFiles` y los 2 de `GreenDeleted`;<br>• el anexo (`v54.md:4929-5492`) es la pieza byte a byte.<br>**(c) La fila 282** dice «el ayudante busca FindPilotDraft con la firma del borrador completo (D-S13)». Es lo que hace la edición `pbt4d-red-v54.txt:1664-1669` sobre `6c2820ba:tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:906`.<br>**(d) La fila 284**, del grupo `directiva`, dice «anadida (linea 7 del RED)», y mi RED la pone en la línea 7. Con `usings_rv2.py`, es la única directiva `using` que el RED añade o quita en los 23 archivos que edita.<br>**La lista, de 270 a 271:** solo cambian la fila 282 (el porqué) y la 284 (nueva). Son 284 filas, 271 con acción y 39 «reescribir», como dice el JSON |
| M-13 | **Resuelto** | **`v54.md:1606`:** «desde la v5.3 se cuentan 537 (433 se quedan), porque ahora entran también las líneas de WPUR que caen dentro de los tramos que el GREEN borra enteros».<br>**Lo confirma el código:** `cov_p6_v52.py:62-70` quitaba las líneas `retired` antes de contar, y `cov_p6_v54.py:226-234` no las quita |

### Los hallazgos nuevos

| Id | Gravedad | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| M-14 | Menor | `v54.md:9` y `:22` | **«Lo que cambia» de la v5.4 repite el modo de M-10:**<br>• **nombra §7, que no cambia** de la v5.3 a la v5.4: no hay ningún tramo del diff entre `v54.md:3112` y `:3143`;<br>• **no nombra lo que sí cambia:** D-PBT4-10 (`:1075` y `:1084`: la pieza del GREEN, el anexo y la lista), §5.6 (`:1899`: el anexo) y la apertura de §6.4 (`:2882-2896`: la lista, el RED y el GREEN, con su sha256);<br>• **la columna «Dónde» de V54-04** no nombra §8.5, que cambia con la nota de D-S16 (`:3233`).<br>Todos los cambios caben en alguna fila V54, y ninguno toca el contrato | En «Lo que cambia»: quitar §7 y añadir D-PBT4-10, §5.6 y §6.4. En V54-04: añadir §8.5.<br>El modo se repite justo en la versión que corrige M-10, así que conviene que lo mire el Coordinator |
| M-15 | Menor | La nota de D-S16 y `cov_p6_v54.py` | **Restos de M-11:**<br>**(a) La nota de D-S16** (el último párrafo de `session-pbt4d-design-decisions.md`, `bd02d210…01c8`) dice «la v5.4 lo aplica en `cov_p6_v53.py`».<br>• Lo aplica `cov_p6_v54.py` (`8d6aaccc…03b6`).<br>• `cov_p6_v53.py` (`6495db6e…6552`, sin cambio) sigue con la regla de la v5.3.<br>**(b) La definición de PERDIDA:** `cov_p6_v54.py:248-249` y `:327`, y por eso `cov-p6-v54.txt:51`, siguen diciendo «en el GREEN no la cubre ni Core ni las tres juntas». Pero lo contrario de la ESTABLE nueva es «ni Core ni las tres juntas con un filtro». Medido: 0 casos.<br>**(c) El uso:** `cov_p6_v54.py:17` dice «Uso: python -I cov_p6_v53.py» | **(a)** Declarar la errata en la entrada «## PB-T4d» o en el paquete, sin tocar el archivo cuyo sha cita la v5.4.<br>**(b) y (c),** si el guion se vuelve a emitir.<br>Nada de esto cambia la medida |
| M-16 | Menor | `pbt4d-precondition6.json` (`RedFiles`), §6.4.2 (`v54.md:3105`) y §11.9 (`v54.md:3282`) | **(a) En WPUR, `RedFiles` es el RED que corrió (v5.3), no el que se emite (v5.4):**<br>• `RedFiles["tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs"].sha256` es `62c6ff80…5489`, el del RED v5.3 según mi aplicador;<br>• el RED v5.4 da `7ea61704…ba13`;<br>• §6.4.2 nombra `RedFiles` sin decirlo;<br>• ni el JSON ni el paquete fijan ese sha. Solo lo da `r70-ra/apply-red.txt`, que no tiene hash, y, de forma indirecta, el sha de la pieza;<br>• el generador lo trata a propósito (`make_pbt4d_contract.py:807-822`, «decision de la sesion»), pero no es una de mis entradas.<br>**(b) La custodia de §11.9:**<br>• lleva «las dos piezas» (las de la v5.4) y la lista 271, con la 229 como historia;<br>• no lleva las piezas de la v5.3 (`pbt4d-red-v53.txt` y `pbt4d-green-v53.txt`) ni la lista 270, con las que corrió la precondición 6;<br>• el JSON las nombra en `Pieces`, y el generador lee `pbt4d-red-v53.txt` de la revisión (`make_pbt4d_contract.py:98`, `:537-543` y `:814-816`) | **(a)** Una frase en §6.4.2: `RedFiles` es el RED de la copia `r6y`; WPUR solo difiere en comentarios, y su sha en la v5.4 es `7ea61704…ba13`. O añadir ese sha a `Reapply`.<br>**(b)** Añadir a §11.9, como historia, las dos piezas de la v5.3 y la lista 270 |

**Observaciones, que no cuento:**
- `Reapply.TestsTextChanged` y `Reapply.TestsCodeChanged` son cadenas con la forma de una lista de Python, no listas JSON. El generador las lee con `replace("'", '"')`, y `Outcomes` de la base ya era así.
- El paso 7 de §6.4.1 (`v54.md:2990`) dice «las 436 líneas y las 9 ramas», junto a las 537 de `:2998`. La explicación está en §4.4 (`:1606`), pero el paso 7 no remite a ella.
- «Lo que sale de la v5.4» (`v54.md:11`) no nombra `v54/codeonly_v54.py`, aunque el JSON lo fija en `Pieces` y lo usa la reaplicación.

### El diff de la v5.3 a la v5.4

**El paquete:** 45 tramos (`pkgdiff_words_rv2.py`), y cada uno es de una fila V54:
- la cabecera y su registro;
- la cabecera y el registro de la v5.3 (V54-03);
- el nombre y el sha de las piezas, de la lista y del anexo, en §0, D-PBT4-10, §4.4, §5.6, §6.4, §6.4.1, §8.3, §11 y el anexo (V54-05 y V54-07);
- la tabla de §4.4, con la fila `directiva` y los totales 284/271;
- las frases de M-11 y de M-13 en §4.4;
- [13] (V54-02);
- el JSON, `compile-check.md` y la cobertura (V54-01 y V54-04);
- la fila del esquema de §6.4.2;
- la nota de D-S16 en §8.5;
- las dos ediciones de WPUR en el anexo.

**Lo que no cambia del paquete:**
- §1 a §3, salvo D-PBT4-10;
- §5.1 a §5.5 y §6.0 a §6.3;
- la tabla de `RequiredTests`;
- §7, §9 y §10;
- los anexos V49-A y V51-A;
- los registros y las cabeceras anteriores a la v5.3.

Las únicas referencias a la v5.3 que quedan están en su cabecera y en su registro (`:40-42` y `:52`).

**Las piezas, la lista y las ediciones:** lo dicho en M-12. `pbt4d_test_edits_v54.py` solo cambia en la docstring, y añade al final las dos ediciones de WPUR (`:315-336`).

**El JSON** (`p6json_diff_rv2.py`). Cambian:
- `Package`;
- `Pieces`: 7 entradas de la v5.4 más, y la lista 270 menos;
- `List`: 284/271, 39 «reescribir» y el grupo `directiva`;
- `Green.UI[n].Json`: las 18, nuevas;
- `GreenUiCoverage` y `Reapply`, nuevas;
- `BaseCoverage.UIByFilter.Runs`: las 19 pasan de tripletas a objetos con `Trx` y `Json`, con la corrida, el filtro y los resultados iguales;
- `Coverage.Output` y `Coverage.Script`;
- `Expected`: se añaden `reapply-cs-0` y `reapply-code-equal`.

Todo lo demás es igual hoja a hoja: `Red`, `Green.Core`, `GreenWithCoverage`, `RedFiles`, `RedDeleted`, `GreenFiles`, `Census`, `Control`, `Rehearsal`, `ClassResults`, `RequiredTests`, `UiTotals` y `Noise`.

Con `pieces_rv2.py`, casan mis sha de:
- las 7 entradas nuevas de `Pieces`;
- las 7 de `Reapply.Pieces`;
- `Reapply.Driver`, `Coverage.Output`, `Coverage.Script` y `Noise.Driver`.

**Siguen iguales:**
- **los totales:** la base, 19 619 = 19 158 + 0 + 461; el RED, 19 418 = 19 014 + 1 + 403; el GREEN, 19 418 = 19 015 + 0 + 403; y la UI, 2 464 = 2 416 + 0 + 48;
- **los 88 `RequiredTests`,** con el n.º 88 en 31;
- **los 18 filtros** (`ui_filters_pbt4d_v53.txt`, `4432ca30…`);
- **la lista de C02:** las 283 filas de la v5.3 siguen iguales, salvo el porqué de la 282, y se añade la 284;
- **las decisiones:** ninguna nueva del Owner, y D-S11 a D-S16 intactas, salvo la nota añadida.

**La reaplicación** (`r70-ra/`): leí sus registros, sin compilar.
- Las 5 compilaciones dan «Compilación correcta», con 0 `warning CS` y 0 errores.
- Según `codeonly_v54.py`, en `tests/` solo cambia el texto de WPUR, y el código de ninguno.
- `src/` no tiene ninguna diferencia con `r6y`.

### Lo que no verifiqué

- **Las 5 compilaciones de `r70`:** no las repetí; solo leí sus registros.
- **Que los 18 JSON de UI por filtro del GREEN sean los de la corrida.**
  - Su sha256 lo calculó la v5.4 después: el conductor de `r6y` no lo registró.
  - Solo lo apoyan sus fechas de modificación, de 08:24:25 a 08:32:44, dentro de la corrida (08:06:00 a 08:47:37).
  - Lo que fija el JSON es el archivo que hay ahora.
- **Si los comentarios nuevos de WPUR cambian alguna prueba que lee el texto de `tests/`.** No ejecuté nada. Vi dos lectores, y ninguno puede cambiar:
  - `6c2820ba:tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs:878-894`, que lee con `Src.CodeOnly`;
  - `NamespaceFolderGuardTests.cs:219-235`, que solo mira las líneas que empiezan por `namespace `.

  No revisé todos los lectores posibles.
- **Cómo se lleva la base al GREEN en la cobertura:** no lo reproduje. Me fié de tres cosas:
  - mi comprobación del lado del GREEN;
  - el diff de `cov-p6`;
  - la reproducción de la v5.3 que hizo la recomprobación 1.
- **El generador** (`make_pbt4d_contract.py`, `a0c9ccdb…366e`): no es una de mis entradas. Solo leí lo que toca a `RedFiles`, `RedDeleted` y `Noise`, y no lo ejecuté.
- **`classes_v51.py`,** que nombra `Pieces`: no lo encontré en `pbt4/` ni en `pbt4dcc/compile/`. No cambia desde la v5.3.
- **La CI real.**

**Deslices:** ninguno.
- Escribí todos los guiones con Write en `rv4d2/`, y retoqué uno mío con Edit (`apply_red_rv2.py`).
- Python solo corrió como `python -I <archivo>`.
- No toqué el repositorio ni el worktree.

### Decisiones nuevas del Owner

**No hace falta ninguna.**
- La regla permanente no cambia: la v5.4 no añade sustitutas ni cambia la medida (PERDIDA 0, VARIA 0).
- **Los tres menores** son de texto y de custodia. La sesión elige cómo corregirlos:
  - en el commit de emisión: la errata de D-S16 en la entrada «## PB-T4d», la frase de `RedFiles` y las piezas de la v5.3 en la custodia, que el generador ya pide;
  - o en una v5.5 de solo texto.
- **M-14 repite el modo de M-10.** Lo señalo para el Coordinator.

> ; recomprobacion v5.4: EMITIBLE; B 0 / I 0 / M 3 (M-14 a M-16, de texto y de custodia, sin decision del Owner); M-08 a M-13 resueltos: el JSON fija el sha256 de los 3 + 18 JSON de UI del GREEN y el completo de las TRX y los JSON de los 19 filtros de la base, recalculados por mi y casan, y 6.4.2 nombra sus 32 claves en orden con RedDeleted contra los DELETE y Noise sin leer; [13] dice D-S11 a D-S16; la cabecera de la v5.3 nombra 0, 1, 6.0, 7 y 9; cov_p6_v54.py exige un filtro en su proceso para ESTABLE solo por la UI, PASA, PERDIDA 0, VARIA 0, ESTABLE 16801 + 118, y en el GREEN hay 0 claves que solo cubran las corridas juntas; el RED v5.4 solo cambia comentarios de WPUR (codigo igual en los 28 archivos, aplicado por mi) y el GREEN solo su cabecera; la lista 271 solo cambia el porque de la fila 282 y anade la 284 (directiva); el paso de 436 a 537 queda explicado; nuevos: M-14 (Lo que cambia de la v5.4 nombra la seccion 7, que no cambia, y omite D-PBT4-10, 5.6 y 6.4, el modo de M-10), M-15 (la nota de D-S16 dice cov_p6_v53.py y la definicion de PERDIDA no se ajusto) y M-16 (RedFiles de WPUR es el RED v5.3 sin declararlo, y 11.9 no custodia las piezas de la v5.3); totales, RequiredTests, filtros y decisiones sin cambio
