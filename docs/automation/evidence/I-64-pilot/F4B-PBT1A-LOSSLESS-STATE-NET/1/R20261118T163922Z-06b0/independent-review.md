# Revisión independiente adversarial del código — I-64 F4B-PBT1A-LOSSLESS-STATE-NET, intento 1

**Objeto:** `git diff a3b63ccc..73d30982` (BaseSha = ChainBaseSha `a3b63ccc`; RED `15f1f225`; GREEN `73d30982`), rama `architecture/workspace-persistente-rackcad`, más la entrega del Worker `artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T142230Z-d31d/` (en adelante `<run>/`).
**Autoridades leídas en el GREEN:** `gate-contract.json` de la tarea, el paquete `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.4 (§2, §3 D-PBT1-12/13/14/15, §4, §5.0-5.5, §6.0-6.1, §7), el registro `I-64-f4b-pbt1-measurements.md` (E-5, E-6), `owner-pbt1-pre-decisions.md` (F4B-E09), el expediente `ob065-dossier.md` §3 y el `prompt.md` del Worker.
**Método:** solo lectura con `git` y lectura de archivos; Python de solo lectura escrito con la herramienta de archivos (análisis de TRX, comparación de líneas, tabla de mutantes, alcance). No se ejecutó dotnet ni ninguna prueba. Alias: **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`, **WC** = `….Composite.cs`, **S0** = `PushBackFullEditorCharacterizationSupport.cs`, **SC1** = `PushBackDeclaredCorrectionSupport.cs`, **SN** = `PushBackStateNetSupport.cs`.

---

## Veredicto

**REWORK** — **B/I/M = 0 / 1 / 9.**

La red es técnicamente sólida y discrimina: los quince mutantes son exactamente la tabla cerrada, cada uno hace fallar por aserción el CT de su fila, el RED cumple el criterio mecánico, el GREEN restaura W y WC byte a byte y la CI del GREEN da los conteos del contrato. El único hallazgo importante (I-01) es de **contrato, no de código**: el Worker cambió las entradas de CT-PBT1A-02 **para que MU-T1A-02 discriminara**, justo lo que la regla «SIN LATITUD» y PBT1A-C04 mandan parar, y lo presentó como «corregir mis propias entradas».

**Camino mínimo de resolución:** I-01 no exige tocar código. Si la sesión (o el Owner, por ser una decisión nueva) **ratifica CT-02 tal como está** como corrección del paquete y lo registra en `f4b-dispositions.md`, I-01 queda resuelto y el veredicto pasa a **ACEPTABLE CON MENORES (0/0/9)**. Si no lo ratifica, hace falta una delegación nueva con la fila de CT-02 corregida. En ese caso conviene aplicar a la vez M-01 y M-02.

---

## Hallazgos

| Id | Sev. | Hallazgo | Evidencia | Qué hacer |
|---|---|---|---|---|
| **I-01** | **I** (+OWNER) | **CT-02: entradas cambiadas para que el mutante discrimine (SIN LATITUD / PBT1A-C04).** En p2, CT-02 se escribió según su fila y midió: «lb» sola no llega a la salida, «ton» se conserva y `PalletsDeepBox=9` no mueve nada. Con esa forma, MU-T1A-02 (unidad fija a «kg») **no cambiaba ninguna línea** (la caja ya muestra «kg» con «ton» cargada, y `SamePallet` ignora la unidad). El Worker añadió en p3 `DepthBox=54` tras «lb», una segunda ventana «ton + DepthBox=54» y «+ frente» tras `PalletsDeepBox`, y lo declara así: «para que MU-T1A-02 y MU-T1A-09 discriminen». En m02, MU-T1A-02 solo falla en la línea añadida (pos 2, «DepthBox=54 […"lb"…]»). El prompt lo prohíbe literalmente: «No cambies el CT, sus pasos, sus entradas ni su linea esperada para que el mutante discrimine», y «nunca lo que un CT fija». Lo que correspondía era PBT1A-C04, un defecto del contrato que vuelve a la sesión. En el fondo, el CT resultante es un **superconjunto** de la fila (sigue fijando «lb» sola, «ton» con Actualizar y `PalletsDeepBox`) y sus líneas son medidas, sin ajustar. MU-T1A-09 no es mutante de la fila de CT-02, así que hacerlo discriminar ahí sobraba. | `prompt.md:154-161`; contrato PBT1A-C04; paquete §5.2, fila 02 (`:734`); `<run>/tests/p2/p2.trx` frente a `p3/p3.trx` y `p4/p4.trx` (CT-02); `<run>/tests/m02/m02.trx` (pos 2); `tests/RackCad.UI.Tests/PushBackStateNetRackControlsTests.cs:152`, `:169-176`, `:186-189`; `worker-handoff.json` Deviations[0] | Disposición de la sesión o del Owner: ratificar CT-02 como corrección del paquete (la fila pasa a decir «lb con un cambio que reconstruye la estructura»), o reemitir. Corregir el registro: no es un error de escritura propio. |
| M-01 | M | **CT-14, G-2: una huella de respaldo que tapa un «Actualizar» que no hace nada.** `huella = w.InsertRequested && w.DesignToInsert != null ? … : null` y después `huella ?? HuellaDe(w)`. Si «Actualizar» no crea la petición, la línea G-2 usa en silencio la huella de `LastComputation`, que hoy es **idéntica** a G-1 en los 20 diseños. Además no se fija que «la ventana se cierra», que la fila pide. G-2 no puede fallar por un «Actualizar» inerte. Lo mitigan CT-02 (`Persist(ton.DesignToInsert)` lanzaría) y `CT_PBTC_02` y `PushBackDeclaredCorrectionStandardRestoreTests` (`Assert.True(w.InsertRequested)`). | `PushBackStateNetFingerprintTests.cs:42-45`, `:64`; paquete §5.2, fila 14 (`:746`); `<run>/tests/p4/ct14.txt` (G-1 = G-2 = G-3 en todos) | Si hay reemisión: sin respaldo, y añadir `cerrada=<b>` o `peticion=<b>` a la línea G-2. |
| M-02 | M (+OWNER info) | **F-02 se reabre, cosa que la receta no dice, y sus seis líneas son idénticas a las de `pb-20-ab`.** `BuildF02` persiste y reabre el diseño, pero el §5.0 dice «`pb-20-ab` con `FrontalSideBox` a 1. Solo en la ventana». Como el lado frontal no se persiste (HT1-04; `frontalSide` solo entra en `BuildFrom` para los cortes frontales, DA:467-488), F-02 G-1…G-6 = `27db1c…`, `c32b6e…`, `b83104…`, `a3e223…`, igual que `pb-20-ab`. Aun aplicada ventana por ventana, la línea de CT-14 (huella del diseño) no vería el lado frontal. El censo del paquete §5.5 atribuye `FrontalSideBox` a «CT-PBT1A-14 (F-02, F-03)», pero solo F-03 lo cubre (`frontal=A`). La entrega no lo declara. | `PushBackStateNetSupport.cs:176-194`; paquete `:677`, `:832`; `<run>/tests/p4/ct14.txt` | Declararlo; nota para la sesión: CT-PBT1C-02 es la única cobertura real de `FrontalSideBox` fuera de I-04. |
| M-03 | M | **`BuildX01` no es «copia literal».** El original pasa el **mismo objeto** `design` a la segunda ventana (`PushBackFullEditorCharacterizationStateTests.cs:110`). La copia reabre un diseño **deserializado otra vez** (`Reabierto(persistido)`). Además usa `Ventana()` + `Cargar` en lugar de `Abrir`, pero eso sí lo manda el §5.0. Es defendible, porque evita compartir un objeto mutable entre ventanas, que es el espíritu de D2, pero no se declara y el comentario dice «copia literal». El original no se tocó. | `PushBackStateNetSupport.cs:118-142`; paquete `:674` | Declararlo. |
| M-04 | M | **D-PBT1-12 (M-05 de PB-TC) no se aplica a la letra:** «las pruebas nuevas confirman o cancelan los módulos en un `finally` interno». CT-06, CT-07 (avanzados), F-01 y G-5 escenifican y confirman sin ese `finally`. El riesgo de M-05 (un `MessageBox` real) lo neutralizan el aviso de descarte sustituido y el contador `descartes=` (0 en todas las líneas). | paquete `:446`; `PushBackStateNetModuleAndSafetyTests.cs:50-80`; `PushBackStateNetCellAndFrontTests.cs:222-236`; `PushBackStateNetSupport.cs:158-165`; `PushBackStateNetFingerprintTests.cs:54-57` | Declararlo; aplicarlo si hay reemisión. |
| M-05 | M | **CT-03, rediseño p3→p4 (declarado).** p2 fallaba por excepción del propio código de medición (`JsonReaderException` al leer «NO-VÁLIDA»): corregirlo es legítimo. Después, p3 midió que la cadena sobre F1 se vuelve inválida con `DepthStartBox=2` («Cada frente debe contener…»). p4 pasa a una ventana por caja, con `DepthStartBox` en F0. El motivo no fue la discriminación (MU-T1A-03 también habría discriminado p3) y la fila («… en un frente») se sigue cumpliendo. Pero deja sin fijar un hecho medido (la invalidez) y ejerce `DepthStartBox` en otro frente. | `<run>/tests/p2`, `p3`, `p4` (CT-03); `PushBackStateNetCellAndFrontTests.cs:55-70` | Aceptable; anotarlo. |
| M-06 | M | **Corridas t01-t14 (T-8) fuera de la letra de INV-PBT1A-10 y del paso 4b.** Corren en local 42 pruebas de UI **fuera de los filtros de RequiredTests** (`PushBackBootUiFlowTests`, `PushBackBootSidesUiFlowTests`, `PushBackHeaderCartesianTests`, `PushBackPalletScopeTests`…). Después del commit RED rehacen W y WC (`git restore --source=BaseSha` + `git apply`), cuando la restauración solo estaba prevista en los pasos 3 y 5. La T-8 tenía que salir «por análisis declarado». Se declara como desviación, pero no como incumplimiento de un INV. El perfil queda intacto (prueba de limpieza antes = después; la disposición del configurador y `settings.json` coinciden hoy con lo medido) y la atribución sale más fuerte. | `prompt.md:187`, `:180`; INV-PBT1A-10; `<run>/tests/t8-index.txt`; Deviations[1] | Registrarlo como desviación de INV-10, sin efecto. |
| M-07 | M | **`Command` no es la orden exacta.** t01: `--filter "<las 42 pruebas existentes…: contenido de …>"`; t02-t14: «(mismo filtro y opciones que t01)»; rb-*: «(con los quince mutantes)»; cf y ci-*: comentarios entre paréntesis. INV-PBT1A-11 y el prompt piden la orden exacta. | `worker-handoff.json` TestsExecuted[43-62], [84], [88-89] | Higiene de la evidencia. |
| M-08 | M | **INV-PBT1A-04, «transcritas en la entrega antes del RED», no se puede demostrar.** La entrega es de las 10:06 y `ct-lines.txt` de las 09:59; el RED es de las 09:13. Lo que sí precede al RED es `p4.trx` (08:41), y sus líneas son **idénticas byte a byte** a los catorce arreglos esperados (comprobado). | marcas de tiempo de `<run>/worker-handoff.json`, `tests/p4/*`; `git log` | En el fondo se cumple; anotarlo. |
| M-09 | M | **Exactitud de `UnexpectedFindings`, que van al Owner.** (a) [3] dice «AbsentSlotsB=[0] aunque el documento no la traiga», pero la línea de CT-09 es de `pb-01-base`, cuyo documento **sí** trae `Composite.AbsentSlotsB=[0]`: en esa línea no hay hallazgo. (b) [0] presenta c-09 como «ya caracterizada en PB-T0», pero CT-PB0-17 y la medición E-5 solo fijaron la **carga** («ton se conserva», «No» es fuente de verdad). La conversión ton→kg al reconstruir contradice E-5 y HT1-15 («solo `PostBox` decide el estado»). Ver OWNER-2. | `Fixtures/I64/pb-01-base.json` (Composite); measurements `:116`; paquete HT1-15 (`:211`) | Corregir la lista antes de llevarla al Owner. |

No encontré **ningún bloqueante**: el alcance, los mutantes, la discriminación, el criterio del RED, la restauración del GREEN, la CI, D2 y la contención están conformes (ver abajo).

---

## 1. Alcance

- `git diff --name-only a3b63ccc 73d30982`: **exactamente los siete archivos nuevos** de `tests/RackCad.UI.Tests/PushBackStateNet*.cs`; `git diff a3b63ccc 73d30982 -- src` está **vacío**.
- RED (`a3b63ccc..15f1f225`): los siete archivos más W y WC. GREEN (`15f1f225..73d30982`): solo W y WC; `git diff 15f1f225 73d30982 -- tests` está vacío.
- Blobs: W `6ca4f29e…` y WC `c8dd9d93…` son iguales en la BaseSha y en el GREEN; en el RED, W `c1e16a7c…` y WC `30924c2d…`. Los SHA-256 de los blobs coinciden con la tabla de bytes de la entrega (W `c6b75de0…`/`7f96de42…`; WC `27c6b689…`/`b220619a…`).
- Comparado contra el contrato: las 9 rutas cambiadas están en `AllowedWriteScope` (9 entradas) y ninguna cae en `ForbiddenWriteScope` (131). `RequiredTests` = 19 y `StopConditions` = 30, como dice el paquete.
- `PushBackFullEditorCharacterizationStateTests.cs` (el original de `BuildX01`) no cambia. El árbol de trabajo está limpio; no hay archivos sin versionar fuera de `artifacts/`.

## 2. Los mutantes (tabla cerrada §5.2)

Lo comprobé con un script sobre los blobs: cada cambio del RED es **la sustitución exacta de una línea**, con la sangría original, en la línea de la tabla; no hay ningún otro cambio en `src/` (SequenceMatcher: 14 sustituciones en W y 1 en WC, ninguna inserción ni borrado). Ningún mutante está fuera de la tabla y no falta ninguno. MU-T1A-13 está en **W:313** (`LoadExisting`); el texto aparece también en W:296 y esa línea no se toca. Los quince `mu-*.patch` tienen 1 borrado y 1 adición cada uno; `mu-all.patch` tiene 15 y 15.

| Fila | Cambio (BaseSha → RED) | CT que falla aislado (posición del primer elemento distinto) | Valoración |
|---|---|---|---|
| MU-T1A-01 | W:874 `NumberFrontsCheck` → `NumberLevelsCheck` | **CT-01** (pos 1); CT-14 (pos 10, `pb-03-fondos` G-1) | Conforme; discrimina por la casilla que cambia |
| MU-T1A-02 | W:852 la unidad de la caja → `"kg"` | **CT-02** (pos 2, «DepthBox=54 […"lb"…]») | Discrimina **solo por el paso que añadió el Worker** (I-01); con la forma de la fila no habría discriminado |
| MU-T1A-03 | W:817 `IntVal(DepthStartBox, …)` → `front.DepthStartPosition` | **CT-03** (pos 1, «DepthStartBox=2 F0») | Conforme |
| MU-T1A-04 | W:827 `SelectedPeralte(...)` → `cell.IntermediateBeamDepth` | **CT-04** (pos 0) | Conforme |
| MU-T1A-05 | WC:209 → `SetCentralSeparator(false)` | **CT-05** (pos 1, `CentralSeparator=<null>`) | Conforme |
| MU-T1A-06 | W:2133 → `(double?)null` | **CT-06** (pos 1) | Conforme |
| MU-T1A-07a | W:3684 `Keep` sin `SameShown` | **CT-07** (pos 0), CT-08 (pos 0), CT-10 (pos 4), CT-14 (pos 35, `pb-08-redondeo`) | Conforme: el redondeo aparece ya en la primera pasada de la carga, que es el mecanismo de M-03 de PB-TC |
| MU-T1A-07b | W:696 → `ModuleCommit != null` | **CT-07** (pos 9, el caso negativo de D-PB-11) | Conforme, por la razón exacta del paquete |
| MU-T1A-08 | W:1510 `ReadCellValues(side)` → `ReadCellValues()` | **CT-08** (pos 3, «datos del frente sin escribir») | Conforme |
| MU-T1A-09 | W:1171 `+ 1` → `+ 2` | **CT-09** (pos 1); CT-02 (pos 6, por el «+ frente» añadido) | Conforme en CT-09; el acople con CT-02 sale de I-01 |
| MU-T1A-10 | W:3666 → `: null;` | **CT-10** (pos 2, la caja de «sin lado B» sale vacía) | Discrimina primero por el camino mixto **legítimo**, antes del obsoleto: el override ya se pierde en la pasada de «Ambos». Es la misma rama de M-07; aceptable |
| MU-T1A-11 | W:2682 → `;` | **CT-11** (pos 3, `ventana=[]`, delta n=1) | Conforme (el portador de botas de CT-16) |
| MU-T1A-12 | W:3568 BOM con el aviso | **CT-12** (pos 1, `Bom=False`) | Conforme |
| MU-T1A-13 | W:313 retorno con tolerancia ≤ 0 | **CT-13** (pos 0, `tipo=<ninguna>`) | Conforme |
| MU-T1A-14 | W:419 → `rackAnnotationScale = 1.0` | **CT-14** (pos 35, `pb-08-redondeo`, como dice el paquete); CT-07 (pos 0) | Conforme |

**Cobertura:** los catorce CT fallan con al menos un mutante. Que haya partes de un CT sin mutante propio es lo que prevé el paquete (un mutante por fila): las casillas de cotas en CT-01, `PalletsDeepBox` y «ton» en CT-02, `CellFondoOverrideBox` y `CellDrawPalletCheck` en CT-04, la estructura, la topología y `FrontCountBox` en CT-05, el alcance (W:1346) en CT-08 y la segunda línea de CT-13.

## 3. Cada CT

Comprobaciones comunes, con resultado positivo en las 14: las clases son `public sealed`, llevan `[Collection(EditorDiscardPromptCollection.Name)]` y usan un `namespace` de bloque, con el reparto exacto del §5.2 (01-02 / 03, 04, 07, 08 / 05, 09, 10 / 06, 11 / 12, 13 / 14). Hay un `[Fact]` por CT y cada uno acaba en **un solo** `Assert.Equal` (`RackControls:114, :195`; `CellAndFront:96, :165, :240, :321`; `Composite:123, :194, :267`; `ModuleAndSafety:84, :152`; `Actions:71, :112`; `Fingerprint:97`), sin otra aserción en el cuerpo. Las ventanas usan `Ventana()`, cargan dentro del `try` y cierran con `Cerrar` en el `finally`. Todo corre dentro de `EditorDiscardPrompt.Substitute` y el sufijo `descartes=<n>` se lee después de cerrar. Solo se muestra la ventana de CT-11, y exactamente como la describe el paquete (`ModuleAndSafety:123-130`). No hay teclado, foco real, `MessageBox`, BOM, biblioteca, configurador ni «Insertar» (comprobado con grep). Ninguna aserción está dentro de una costura (`SafetyDialog` solo cuenta y devuelve copias profundas). **Las líneas esperadas son idénticas, byte a byte, a las medidas en p4** en los 14 CT (lo comparé con un script contra `p4.trx`).

| CT | ¿Fija lo que promete la fila, sobre la salida? | Valoración |
|---|---|---|
| 01 | Sí: cada casilla y cada índice de `DimensionsBox` en una ventana propia; `DimensionViews` nulo se queda nulo; tocar una casilla de cotas escribe 6, 5 o 3 (I-50) | Conforme |
| 02 | Fija los tres elementos de la fila y además lo añadido (`DepthBox`, «ton + DepthBox», «+ frente») | **I-01**; las líneas son buenas y descubren la conversión ton→kg (OWNER-2) |
| 03 | Sí: un rack de un sentido, una caja por ventana, y «Ambos» en el compuesto | Conforme (M-05) |
| 04 | Sí, en la celda y en `Fronts[0]` de la salida | Conforme |
| 05 | Sí: separador, estructura con Aplicar y Restaurar, topología, sentido, alcance y `FrontCountBox` | Conforme |
| 06 | Sí: nada hasta «Confirmar» y cancelar no deja nada (también lee `PendingWork`) | Conforme (M-04) |
| 07 | Sí: 61,23456, 3,00044 y 1,23456 exactos tras «Todas», «Todos los frentes», «Este frente», el cambio de celda y «Restaurar»; y el negativo de D-PB-11 | Conforme |
| 08 | Sí: la caja escrita llega a los dos lados por el alcance y por los datos del frente; sin escribirla, cada lado conserva el suyo (una línea lee el `Text` de las cajas) | Conforme |
| 09 | Sí: `Fronts`, `SideB.Fronts`, `AbsentSlotsA/B` con «+», «−» y «En blanco», con y sin B | Conforme |
| 10 | Sí, con «INCUMPLIMIENTO CARACTERIZADO» en el nombre del método y en las dos líneas de M-07 (`HasError=False IsOptional=True`, override conservado) | Conforme |
| 11 | Sí: `Structure.SafetySelections` y el BOM antes y después; ventana fuera de pantalla, con la carga dentro del `try`. También lee la costura `SafetySelections` | Conforme |
| 12 | Sí: los once botones de la barra (`WX:750-790`), con el aviso y sin él | Conforme |
| 13 | Sí: tipo y mensaje (`ArgumentException`, la cultura invariante fija el sufijo `(Parameter 'design')`) y cierre en el `finally`. La segunda línea comprueba sobre todo el propio `finally` de la prueba, pero la fila lo pide | Conforme |
| 14 | El corpus va en orden ordinal: los 17, X-01, F-01, F-02 y F-03. G-1…G-6 en ese orden, cada uno en una ventana nueva; G-4 solo en compuestos (`SideBPresent` = `IsComposite` de Domain: solo `pb-20-ab` de los 17), así que hay 105 líneas. **F-03** tiene su guion único G-F03 y su línea fija `pasadas-restaurar=2 frontal=A PalletTolerance=3.00044`; la autocomprobación es parte del `Assert.Equal` | Conforme (M-01, M-02, M-03) |

**Las recetas:** X-01 (M-03), F-01 conforme (separador A y luego `B:`, +2,0 cada uno, «Confirmar» y reapertura), F-02 (M-02) y F-03 conforme, paso a paso con el §5.0 (`SN:203-227`).

## 4. Discriminación (INV-PBT1A-03)

Lo comprobé en los 15 TRX m01…m14: el CT de la fila está en `Failed`, el `Message` empieza por `Assert.Equal() Failure` sin prefijo de tipo, y el primer frame del `StackTrace` está en `tests/RackCad.UI.Tests/<archivo>.cs` **en la línea exacta del `Assert.Equal` de ese método** en el GREEN. Esto último muestra además que los mutantes se corrieron contra los archivos de prueba definitivos. Las razones coinciden con el paquete en todos, con dos matices: MU-T1A-02 (I-01) y MU-T1A-10, que discrimina primero por el camino mixto legítimo.

## 5. El RED

- `red-ci-ui/ui.trx` (CI 44333116533, sobre `15f1f225`): 2436 resultados, 2359 Passed, 56 Failed y 21 NotExecuted. El filtro `PushBackStateNet` da **14 de 14 en Failed** por aserción (sin prefijo; primer frame en `tests/`, en la línea del `Assert.Equal`); ninguno NotExecuted ni Skipped; Failed = Selected = 14. Lo mismo en `r1` y en `rr1` (TreeSha `8d573363…` = `15f1f225^{tree}`).
- `red-ci-core/core.trx`: 19070 de 19070 en Passed.
- **T-8:** las 42 pruebas existentes que fallan en la CI del RED están atribuidas, y lo comprobé con las corridas aisladas t01…t14: **ninguna queda sin atribuir** y ninguna corrida aislada hace fallar una prueba que no esté entre las 42. Las 23 que caen dentro de los filtros de RequiredTests coinciden con rd01…rd18. Que MU-T1A-07a (y MU-T1A-10, en `CT_PBTC2_04`) rompa pruebas de PB-TC2 está previsto en el §2.8. La atribución es creíble y está respaldada empíricamente (con la salvedad de procedimiento de M-06).

## 6. El GREEN y la estabilidad

- `ci-ui/ui.trx` (CI 44338475965, sobre `73d30982`): **2436 resultados, 0 Failed, 21 NotExecuted (Skipped), 2415 Passed**. `ci-core`: **19070 Passed**. Coinciden con el §6.1 sin ninguna diferencia. Los cuatro jobs en `success` y el RED con `ui-tests` en `failure` y los demás en `success` constan en `relay-record.json` (lo escribe la sesión; no lo consulté en GitHub).
- En local: g1, g2 y g3 consecutivas con 14 de 14; gr01…gr18 en verde (PB-T0 con 16 Passed y 2 Skip, SUS-03 y SUS-04); cf con 19070 de 19070. Las builds Release b0, rb y gb dan los mismos avisos por código (solo `xUnit*` y `MSB3277`) y 0 errores.
- Dependencias: no hay reloj, aleatoriedad ni GUID nuevo (`Ventana()` usa `FixedId`); la cultura es invariante (`Sta`); los fixtures van en orden ordinal y se resuelven por la raíz del repositorio; no hay foco real (se eleva `LostFocus`). Cada prueba usa su propio contador. Toda ventana se cierra en un `finally`; cuando «Actualizar» cierra la ventana, `Cerrar` no hace nada. Al final de la sesión, la instantánea de procesos no muestra ningún testhost vivo. No vi dependencias de orden. Los resultados de la CI y los locales coinciden, así que no hay dependencia de la máquina.

## 7. Contención

- La prueba de limpieza antes y después es idéntica. Hoy releí la disposición del configurador (`7AAB8FF7…`, 2026-11-14T22:55:38Z) y `settings.json` (`A7667DBD…`), y coinciden con lo medido. El sondeo resuelve la biblioteca igual que `UserSettingsStore.ResolveDesignLibraryPath`.
- El Plugin solo se compiló con `-o` bajo `<run>/tests/*/plugin-out`; no se cargó.
- El código temporal del paso 1 no entró en ningún commit: no hay `Assert.Fail` en los siete archivos del RED ni en los del GREEN, y solo hay dos commits sobre la BaseSha.

## 8. D2 y su límite (PBT1A-C17)

Ninguna receta ni prueba muta el `PushBackSystem` ni el `RackModuleReconciliationResult` de una ventana viva. Solo hay lecturas: `PushBackHeaderTestSupport.Lines`, que lee `WorkingBaseline.Structure`, `LastComputation.Design` y `LastComputation.Bom`. Las recetas solo tocan controles y costuras existentes (`SelectMatrixCell`, `SetFrontBlank`, `SafetyDialog`, que devuelve **copias profundas**). La reapertura de X-01, F-01 y F-02 a partir de una copia deserializada evita compartir por referencia el diseño entre ventanas. PBT1A-C17 no se activa.

## 9. INV-PBT1A-13: ¿está completa la lista?

Revisé todas las líneas fijadas, incluidas las 105 huellas. **No encontré ninguna línea con una pérdida o un control releído que cambie el estado o la salida sin edición que la entrega no declare.** Dos de sus entradas son inexactas (M-09). El reinicio de `FrontalSideBox` de B a A en F-03 no es un hallazgo nuevo: lo describe el paquete (HT1-10b, N-M-08) y el lado frontal es contexto de sesión (HT1-04).

## 10. Las desviaciones declaradas y las corridas p1 a p4

| Corrida | Qué midió | Qué cambió y por qué |
|---|---|---|
| p1 (08:32) | Solo CT-14: 105 líneas | — |
| p2 (08:37) | Los 14 CT | CT-03 **falla por excepción** de su propio código: lee como JSON el texto «NO-VÁLIDA». CT-02, en su forma de la fila, mide que «lb» sola no llega, «ton» se conserva y `PalletsDeepBox` no mueve nada |
| p3 (08:39) | CT-01, CT-02 y CT-03 | CT-01 no cambia. CT-02 **gana entradas** (`DepthBox=54` dos veces y «+ frente») **para que el mutante discrimine** (I-01). CT-03 corrige la lectura de «NO-VÁLIDA» y mide que la cadena sobre F1 se vuelve inválida con `DepthStartBox=2` |
| p4 (08:40) | Los 14 CT | CT-03 pasa a una caja por ventana, con `DepthStartBox` en F0 (M-05). CT-04 a CT-14 son idénticos a p2 (hash del mensaje) y CT-14 es idéntico en p1, p2 y p4 (md5 `dcd00132…`). Estas son las líneas fijadas |

- **Deviations[0](a), CT-02:** incumple «SIN LATITUD» y PBT1A-C04; está en I-01. Sigue fijando lo que dice su fila, y más.
- **Deviations[0](b), CT-03:** aceptable; el paso p2→p3 corrige un error de escritura propio, y el paso p3→p4 rediseña sin motivo de discriminación (M-05). Sigue fijando lo de su fila.
- **[1] T-8:** M-06. **[2] rr1:** sin efecto. **[3] procesos en segundo plano:** no verificable. **[4] `dotnet` sin ruta:** sin efecto. **[5] LF:** sin efecto. **[6] `TestResults` generado por script:** coincide con los TRX en lo que comprobé.
- Ninguna desviación toca PBT1A-C07: los hechos del §2 que usan los CT se sostienen, los textos de la tabla de mutantes están en su línea y F-03 da `pasadas-restaurar=2` y `frontal=A`. La salvedad es HT1-15 y E-5 frente a c-09 (OWNER-2), que va por INV-PBT1A-13 y no es una parada.

---

## Hallazgos OWNER (antes de PB-T1b, precondición 5)

1. **OWNER-1 (= I-01). La premisa de CT-02 y MU-T1A-02 no se sostiene.** «lb» sola no llega a la salida (`SamePallet` ignora la unidad); la unidad solo se escribe cuando otra edición reconstruye la estructura. El Worker rediseñó CT-02 en lugar de parar. Hay que decidir si se ratifica el CT-02 entregado como corrección del paquete o se reemite.
2. **OWNER-2 (INV-13, más grave de lo que dice la entrega). c-09 al reconstruir.** Con un documento en «ton», una edición ajena (`DepthBox=54`) hace que la salida pase a «kg» sin que el usuario toque la unidad («carga ton y DepthBox=54 [… "kg" …]»). `WeightUnitBox` se comporta como un **segundo control fuente de verdad** (la definición de H-PB-44), al estilo de c-02. Eso contradice HT1-15 («solo `PostBox` decide el estado») y la medición E-5 («No»). PB-T0 solo había fijado la carga. Decisión análoga a la de c-02 (F4B-E09, pregunta 2): ¿regla explícita del estado, o reproducir sin más?
3. **OWNER-3 (INV-13). Controles inertes por sí solos.** Elegir «lb» no cambia la salida, ni siquiera tras «Actualizar», si nada reconstruye la estructura: la elección del usuario se descarta en silencio. `PalletsDeepBox=9` no mueve la salida ni el fondo del frente que se añade. PB-T1 los reproduce tal cual.
4. **OWNER-4 (INV-13). Aceptar Seguridad sin cambios cambia la salida** en `pb-05-topes`: el portador de botas `PROTECTOR_BOTA_H_3_16_18` aparece en `Structure.SafetySelections[0]` (28 líneas) más `DefensePieceId`, sin cambio en el BOM (12 líneas, 459 piezas). Confirma la observación de CT-16 de PB-T0; si llega al dibujo queda para Smoke-PB.
5. **OWNER-5 (informativo, paquete). F-02 no aporta nada a CT-14** (M-02). El censo del §5.5 sobrestima la cobertura de `FrontalSideBox`; solo la dan F-03 y, más adelante, CT-PBT1C-02.
6. **OWNER-6 (informativo).** G-1 = G-2 = G-3 en los 20 diseños, así que G-2 y G-3 solo detectan cambios del diseño escrito, no un «Actualizar» o un «Restaurar» inertes (M-01; F-03 sí cubre «Restaurar»). `pb-10`, `pb-11`, `pb-12` y `pb-13` dan en G-1 la huella de `pb-01-base`; son normalizaciones de carga ya fijadas (por ejemplo, `WorkspaceDraftPilotMaterializationTests.cs:716` para `pb-13`).

---

## Lo no verificado

- No consulté GitHub: las conclusiones de los cuatro jobs y la ausencia de *blame-hang* y de *timeout* en las CI del RED y del GREEN salen de `relay-record.json` (escrito por la sesión) y de los TRX, que son coherentes.
- No pude comprobar que cada corrida m01…m14 y t01…t14 tuviera **solo** su mutante aplicado: no hay `TreeSha` (null, como manda la regla). Lo apoyan los fallos, que son coherentes con un único mutante, y los números de línea de los frames.
- No pude comprobar que la transcripción de las líneas en la entrega fuera anterior al RED (M-08), ni lo que declara Deviations[3] sobre las órdenes pasadas a segundo plano por el entorno.
- No ejecuté las pruebas ni observé la ventana de CT-11 fuera de pantalla. No verifiqué si el portador de botas llega al dibujo (exige AutoCAD).
- La clasificación de las normalizaciones de `pb-10`, `pb-11` y `pb-12` dentro de la T-2 de PB-T0 la acepto por coherencia, sin releer la entrega de PB-T0.
- **Nota de método:** lancé por error una invocación de `python` con la entrada estándar vacía. No ejecutó código ni escribió nada; la detuve.
