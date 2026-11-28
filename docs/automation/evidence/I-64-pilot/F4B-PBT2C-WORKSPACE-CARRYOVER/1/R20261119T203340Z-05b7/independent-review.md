# I-64 F4B PB-T2c — revisión independiente del código (intento 1)

`F4B-PBT2C-WORKSPACE-CARRYOVER`, WorkRunId `R20261119T203340Z-05b7`.
BaseSha = ChainBaseSha `05beabfd`, RedSha `7aeb6170`, CurrentSha `b1968341`. Sin reparación del RED.

## Veredicto

**ACEPTABLE CON MENORES. B 0 / I 0 / M 2.**

Además:
- **2 observaciones sin severidad** (O-01 y O-02);
- **1 hallazgo OWNER informativo** (OWNER-01), que no bloquea y no es un defecto de la entrega.

**En resumen:**
- **El alcance es exacto.**
  - El RED son la clase y las tres sustituciones de la tabla.
  - El GREEN es la reversión, con los blobs de W y WC iguales a los de BaseSha.
- **Las 76 líneas fijas** (49 + 14 + 13) coinciden byte a byte con sus fuentes.
- **Los 49 gestos** son el código de `Gestos()` sin cambio.
- **Los mutantes** fallan por aserción en el CT de su fila.
- **La atribución** cubre los 21 fallos existentes del RED, y cada una la corroboran los mensajes del TRX.
- **CI del GREEN:** 4/4, Core 19 094 y UI 2 450 (2 429 superadas y 21 omitidas).
- **Contención:** limpia.

## Hallazgos

| Id | Sev. | Dónde (`b1968341`) | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-01 | M | `tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs:99-102` y `:27-28` | **El gesto y el clic de CT-01 van sin `Paso`.** En `Aplicada`, la casilla neutra y `Click(w, "UpdateButton")` no pasan por `Paso`. En CT-03 sí pasan (`:458`, `:462`), y en CT-02 los envuelve `UnaVentana` (`:226`). Si el producto lanzara ahí, CT-01 fallaría por excepción y no por aserción (C04), en vez de dejar una línea `EXCEPCION`. Hoy no lanza: p1, r1, m1 a m3 y g1 a g3 fallan o pasan por aserción.<br>**Y un comentario inexacto:** la documentación de la clase dice «su carga va DENTRO del `try`», pero en WORKSPACE la carga es `ForDraft`, dentro de `Nueva` (`:58`), antes del `try`. Es el mismo patrón que PB-T2b (`PushBackWorkspaceApplyCloseTests.cs:397-400`) y no tiene efecto práctico | No reabrir. En una corrección futura de la clase, envolver los dos pasos en `Paso` y ajustar el comentario |
| M-02 | M | `rv/worker-handoff.json`, `KnownLimitations[2]` y `WorkCompleted` §6 | **El Worker lee mal INV-PBT2C-10.** Dice que la atribución se hizo «por análisis declarado […] no por corridas aisladas de otros filtros (el regimen de INV-PBT2C-10 solo admite el filtro de la clase nueva)». No es así: INV-10 admite en UI todos los filtros del contrato y solo prohíbe la suite completa. Correr los filtros existentes bajo cada mutante aislado estaba permitido; no era obligatorio, porque INV-04 no lo exige.<br>**La atribución es correcta igualmente.** Comprobé los 21 mensajes del TRX del RED, y cada uno lleva la firma del mutante atribuido (tabla de abajo) | Ninguna sobre el código. Si la sesión quiere, puede anotar en la verificación que esa frase interpreta de más |

### Observaciones (sin severidad)

- **O-01. CT-01 no distingue MU-T2C-03, y eso viene del diseño del paquete.**
  - En m3, CT-01 pasa (`rv/tests/m3/m3.trx`).
  - El motivo: el borrador recibe `Copy(appliedState)`, es decir W0, que ya trae los exactos del sobre. `aplicado` solo prueba que el borrador cambió de objeto.
  - Así, CT-01 prueba que los exactos llegan al borrador, pero no que vienen del estado editado. Eso lo cubren CT-03 (`igual-a-la-ventana`) y CT-PBT2B-04.
  - El §5.4 asigna a CT-01 solo MU-T2C-01. No es un defecto de la entrega.
- **O-02. La custodia de la evidencia.**
  - Los `test.log` y los TRX de `rv/tests/` y los `Command` de `TestsExecuted` llevan rutas locales absolutas, y los TRX llevan `computerName`.
  - `artifacts/` está en `.gitignore` (`.gitignore:3`), así que hoy no salen al repositorio.
  - Si la sesión custodia algo de ahí en el repositorio público, va redactado, como pedía M-14 de CA-T2c.

## 1. Alcance y forma

**El RED** (`git diff 05beabfd 7aeb6170`) toca tres archivos: W (M), WC (M) y la clase nueva (A, 549 líneas). Los mutantes son tres sustituciones de una línea, cada una en su línea y con su sangría de 12 espacios:

| Mutante | Línea en el RED | Antes (BaseSha) | Después | ¿= anclajes? |
|---|---|---|---|---|
| MU-T2C-01 | W:3713 (`Keep`) | `=> !field.Value.HasValue \|\| SameShown(field.Value.Value, current) ? current : field.Value.Value;` | `=> !field.Value.HasValue ? current : field.Value.Value;` | Sí (`pbt2c-anchors-out.txt:9-12`) |
| MU-T2C-02 | WC:95 (`UpdateSideBEmptyNotice`) | `SideBEmptyNoticeText.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;` | `SideBEmptyNoticeText.Visibility = Visibility.Collapsed;` | Sí (`:13-16`) |
| MU-T2C-03 | W:3273 (`ApplyToDraft`) | `var result = draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable));` | `… Copy(appliedState));` | Sí (`:17-20`) |

En W y WC no cambia nada más: numstat 2/2 y 1/1, y WX sigue en `fb094bef` en los tres SHA.

**El GREEN** (`7aeb6170..b1968341`) solo toca W y WC, sin `tests/`. Los blobs quedan así:
- **W:** BaseSha `da66eba0`, RED `860f0d32`, GREEN `da66eba0`;
- **WC:** BaseSha `1a3300b4`, RED `81b03d4f`, GREEN `1a3300b4`.

Por tanto, `git diff 05beabfd b1968341 -- src` sale vacío, y `--name-only` da solo la clase nueva (INV-PBT2C-01).

**Las pruebas y ayudantes existentes no cambian.** `PushBackWorkspaceSupport.cs` es el blob `e896cfd8` en los tres SHA, y el diff neto tiene un solo archivo.

**La cadena:**
- `b1968341` es hijo directo de `7aeb6170`, y no hay commit «RED repair».
- Los árboles son los declarados: RED `c588335e` y GREEN `fd13aa35`.
- El RED se empujó antes del GREEN: el commit es de las 21:23:11Z, su CI arrancó a las 21:24:28Z y el GREEN es de las 21:30:37Z.
- Los dos commits llevan el trailer `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`, que es igual a `Worker.Trailer`.

## 2. CT → código

| CT | Método y ayudantes (`b1968341`) | Frente al §5.4 |
|---|---|---|
| **CT-PBT2C-01** | `CT_PBT2C_01_…` `:112-193`; `Aplicada` `:87-110`; `Leida` `:63-64`; `Cotas` `:72-80`; esperadas `:174-190`; `Assert.Equal` `:192` | **Conforme.** El detalle: |
| | | - **Los estados:** `Estado(VarianteDe(f))` y `Estado(Fixture(…))` sobre una ventana `ForDraft`, con la sonda antes de cualquier gesto (`:97`). |
| | | - **El gesto neutro** es `DrawRackNameCheck` invertido sin elevar `Click` (`:99-100`). W:814 lo lee en `ReadInputs`. |
| | | - **`aplicado`:** `actual` se toma justo antes del clic (`:101-103`). |
| | | - **Las filas F:** `Num(DisenoDe(draft.CurrentDraftState, A), f.Path)` (`:130`), que el paquete iguala a `Persist(c.Design)`. |
| | | - **`pb-08`:** escala y altura de `DisenoDe`, y `sistema.AnnotationScale` de `c.System.Structure.AnnotationScale`, con `c = Pasada(Copy(draft.CurrentDraftState), A)` (`:139-143`). |
| | | - **El estilo:** `c.Design.Structure.DimensionStyle` y `c.System.Structure.DimensionStyle` (`:162-163`), y `Cotas(c)`, no `w.LastComputation` (`:156`). |
| | | - **La copia de `Cotas`:** es la de `PushBackDeclaredCorrectionDimensionStyleTests.cs:83-92`, cambiando solo el parámetro. |
| | | - **La petición clásica no se lee:** ninguna aparición de `DesignToInsert` ni `SystemToInsert` en la clase. |
| | | - **La lectura:** justo tras «Aplicar» (`:104`). La línea se arma antes del `Cerrar`, y ` descartes=` va después (`:105-106`). |
| **CT-PBT2C-02** | `CT_PBT2C_02_…` `:234-324`; `UnaVentana` `:216-232`; `Linea` `:206-210`; esperadas `:306-321`; `Assert.Equal` `:323` | **Conforme.** El detalle: |
| | | - **Los gestos de CT-PBTC-07,** copiados de `PushBackDeclaredCorrectionSideBNoticeTests.cs:48-81` (`:250-284`). Solo cambian la carga (`ForDraft` en vez de `Cargar`) y el nombre `Paso` → `Linea`. |
| | | - **Las doce líneas** son las de `:89-100`, byte a byte, con ` descartes=1` en `:315` y `:319`. |
| | | - **La tercera ventana** (`:291-302`): declarar B, «Aplicar», y `c` leída justo después para `diseno-del-borrador-compuesto` (`:296`, `:301`). `borrador-B-presente` sale de `draft.CurrentDraftState.Editor.SideBPresent`. |
| **CT-PBT2C-03** | `CT_PBT2C_03_…` `:474-547`; `Gestos()` `:357-436`; `Censo` `:443-472`; esperadas `:495-543`; `Assert.Equal` `:546` | **Conforme.** El detalle: |
| | | - **Los gestos:** las 68 líneas de código de `Gestos()`, sin comentarios, son idénticas a `PushBackWindowOnStateTests.cs:457-532`, con los 49 `G(…)` en orden. Los 4 `SinComparar` de D2 quedan fuera. |
| | | - **Las ventanas:** una de WORKSPACE nueva por gesto, sobre `Estado(Fixture(gesto.Diseno))`. |
| | | - **Las lecturas:** `actual` justo antes del clic, e `igual-a-la-ventana` y `diseno-igual` (`DisenoDe` frente a `DisenoDeVentana`, como CT-PBT2B-02) justo tras «Aplicar» (`:461-465`). |
| | | - **Las 49 líneas** son las de `pbt2c-anchors-out.txt:24-76` sin las tres `  porque:`, byte a byte, y los nombres van en el mismo orden. |

**Cómo lo comprobé:** con guiones de solo lectura sobre los blobs de `b1968341`, sin `-c`.
- **CT-03:** 49 = 49, `True`.
- **CT-02:** las 12 de CT-PBTC-07, más los dos sufijos y la línea de «Aplicar», `True`.
- **CT-01:** las 14 líneas del texto del §5.4, `True`.
- **`Gestos()`:** 68 = 68, `True`.

**El régimen del §5.0:**
- la clase es `public sealed` y lleva `[Collection(EditorDiscardPromptCollection.Name)]`;
- STA con la cultura invariante (`Sta`), y el STA se arranca antes de `Substitute`, el patrón de PB-T2b;
- un solo `Assert.Equal` por método;
- «R» (`Texto` y `Num`) y el orden ordinal en `estilos` (`:160`);
- `GUID-PBT2-0001` y `SessionId(64)` (`Borrador`);
- sin `Show`, `MessageBox`, archivos ni costuras de BOM, biblioteca o configurador.

**Tautologías:** ninguna. Las líneas más débiles son `peticion=False` y `cotas>0=True`, que fija el propio paquete. Ninguna línea se lee en otro momento del que dice su CT (lecciones de F4B-E30 y F4B-E33).

## 3. Los mutantes aislados (`rv/tests/m1` a `m3`)

| Corrida | Mutante | Resultado (TRX) | El CT de su fila | Por qué falla |
|---|---|---|---|---|
| m1 | MU-T2C-01 | 1 de 3 fallan | CT-01, por `Assert.Equal` (`:192`) | La primera diferencia es la fila F1. Los primeros 50 caracteres coinciden hasta `diseno=4`, así que difiere el valor exacto (redondeado), que es lo que dice la fila. `descartes` no puede diferir: tras «Aplicar», `appliedState` es la copia de `editable` |
| m2 | MU-T2C-02 | 1 de 3 fallan | CT-02, por `Assert.Equal` (`:323`) | Posición 1: `declarar B aviso=Collapsed` en vez de `Visible` |
| m3 | MU-T2C-03 | 2 de 3 fallan | CT-03, por `Assert.Equal` (`:546`) | Posición 0: `DepthBox=54 … igual-a-la-ventana=False`, porque el borrador recibe W0 |

En m3 también falla CT-02, en la posición 12 (la tercera ventana). El borrador recibe W0, sin B, y la entrega lo declara (`UnexpectedFindings[1]`). No contradice INV-PBT2C-03.

**Las tres corridas filtran solo la clase nueva.** Sus pilas apuntan a `:192`, `:323` y `:546`, las líneas del `Assert.Equal` de la clase commiteada, aunque el `TreeSha` es nulo porque corrieron antes del commit. Ningún TRX contiene `EXCEPCION`.

**Sin mutantes,** p1 (BaseSha + clase) da 3/3 superadas, así que C04 y C15 no saltan.

## 4. El RED y la atribución

**El filtro con `ExpectRed`:**
- en local, r1 da 3/3 fallidas por aserción;
- en la CI del RED, las 3 nuevas fallan por `Assert.Equal`.

**La CI del RED (run 44618262418):**
- Tests en `failure`: Core 19 094, 1 fallo;
- UI Tests en `failure`: 2 450 resultados, 23 fallos (las 3 nuevas y 20 existentes);
- Build UI en `success`;
- Build Plugin `skipped`.

Es lo que pide la `ExpectedEvidence`.

**La atribución.** No falta ninguna: la unión de los fallos del TRX de la CI del RED es exactamente la de la tabla (15 + 3 + 2 en UI y 1 en Core). En local cuadra igual: ru1 1, ru2 2, ru4 1, ru5 1, ru6 5 y ru9 10.

| Mutante | Prueba existente | La firma en el TRX del RED |
|---|---|---|
| MU-T2C-01 | `PushBackWorkspaceModeTests.CT_PBT2B_02` | `pb-08-redondeo iguales=False` |
| MU-T2C-01 | `PushBackWindowOnStateTests.CT_PBT1C_01` | `pb-08-redondeo LoadExisting iguales=False` |
| MU-T2C-01 | `PushBackEditableStateParityTests.CT_PBT1B_01` | `pb-08-redondeo LoadExisting iguales=False` |
| MU-T2C-01 | `PushBackStateNetCellAndFrontTests.CT_PBT1A_07` | `c00=61.235` en vez de `61.23456`; `PalletTolerance=3` en vez de `3.00044` |
| MU-T2C-01 | `PushBackStateNetCellAndFrontTests.CT_PBT1A_08` | `A.alto=61.235` en vez de `61.23456` |
| MU-T2C-01 | `PushBackStateNetCompositeTests.CT_PBT1A_10` | La línea M-07, posición 4 (truncada; coherente) |
| MU-T2C-01 | `PushBackStateNetFingerprintTests.CT_PBT1A_14` | La huella de `pb-08-redondeo` G-1 (truncada; coherente) |
| MU-T2C-01 | `PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01`, `02` y `03` | `AnnotationScale=1.235` en vez de `1.23456` (03); F1 en la posición 0 (01 y 02) |
| MU-T2C-01 | `PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06` y `10` | Un `delta=[$.PushBack.…]` no vacío en `pb-08` y en F1 |
| MU-T2C-01 | `PushBackDeclaredCorrection2RoundingTests.CT_PBTC2_01` y `02` | `Gap=54` en vez de `54.00044`; `Length=47.992` en vez de `47.99213` |
| MU-T2C-01 | `PushBackDeclaredCorrection2MixedBoxTests.CT_PBTC2_04` | `A.alto=61.235` en vez de `61.23456` |
| MU-T2C-02 | `PushBackDeclaredCorrectionSideBNoticeTests.CT_PBTC_07` y `08` | `aviso=Collapsed` en vez de `Visible` |
| MU-T2C-02 | `PushBackStateNetActionsTests.CT_PBT1A_12` | `con aviso aviso=Collapsed` |
| MU-T2C-03 | `PushBackWorkspaceApplyCloseTests.CT_PBT2B_03` | La línea de `UpdateButton` (posición 14; `borrador-igual`) |
| MU-T2C-03 | `PushBackWorkspaceApplyCloseTests.CT_PBT2B_04` | `segunda llamadas=2 igual-a-la-ventana=False` |
| MU-T2C-03 | Core: `PushBackWorkspaceModeSourceGuardTests.CT_PBT2B_08` | `regla-R4=False` |

**Lo previsto y lo nuevo.** Lo que el paquete preveía (PB-TC, PB-TC2, PB-T1a, CT-PBTC-07 y 08, CT-PBT2B-03, 04 y 08) está todo en la tabla. Lo que no preveía (CT-PBT2B-02, CT-PBT1C-01, CT-PBT1B-01 y CT-PBT1A-12) tiene un análisis creíble. CT-PBT2B-06 no falla, como decía el paquete.

En las corridas aisladas solo corrió la clase nueva, así que no hay fallos existentes que atribuir (ver M-02).

**La reparación del RED (RR-1 a RR-7): no aplica.** No hay commit «RED repair», `Deviations` está vacío y la clase es el blob `5efab0a1` en el RED y en el GREEN. Aun así comprobé la lista cerrada sin RR, y sigue byte a byte (§2).

## 5. D2 (INV-PBT2C-14, C17)

Las líneas nuevas solo leen:
- `draft.CurrentDraftState`, `w.EditableStateForTest` y `w.LastComputation`;
- y `Copy`, `Pasada` y `Persist` sobre copias.

**`Pasada` sobre una copia** sustituye la línea base de la copia y no muta la anterior: `SetWorkingBaseline` está en `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs:101-107`. Esa operación ya la mide CT-PBT1B-16, y `DisenoDe` ya la usa en PB-T2b.

Ninguna línea nueva:
- asigna un campo de un `PushBackSystem` ni de un `RackModuleReconciliationResult`;
- comparte un objeto mutable nuevo fuera de las dos referencias declaradas.

`c.System.Structure.*`, `c.Design.*` y `Editor.SideBPresent` solo se leen. **No hay C17.**

## 6. El GREEN y la estabilidad

**La CI del GREEN (run 44619492077):** 4/4 en `success`.
- **Core:** 19 094 superadas y 0 fallos, igual que la línea base.
- **UI:** 2 450 resultados, con 2 429 superadas, 0 fallos y 21 omitidas, la línea base + 3.

**En local:**
- g1, g2 y g3 dan 3/3 cada una;
- gu1 a gu23 y gc1 a gc15 están en verde, y gcf (Core completa) también;
- los avisos de compilación por código son idénticos en b0, r1 y g1, así que la clase no añade avisos.

**Dependencias de orden, cultura o estado estático:** no encontré ninguna.
- El aviso de descarte va sustituido por prueba, con su `PromptProbe` y `Reiniciar` por ventana, dentro de la colección.
- El STA se arranca antes de `Substitute`.
- La cultura es la invariante.
- Lo estático compartido (el catálogo `Lazy` y la tabla de ventanas cerradas) solo se lee o es débil.

## 7. Contención

- **Ventanas y diálogos:** ninguna ventana mostrada, ningún modal real, ningún `MessageBox`, ni el configurador, el BOM, la biblioteca o «Insertar». Ningún gesto de los 49 pulsa `ConfigureModuleHeaderButton`, que es el único que abriría el configurador real (W:2406).
- **El disco:** no hay escrituras de la clase. La disposición del configurador cambió una vez, a las 21:41:29Z, dentro de la corrida gu21 (`RichEditor`, 15:41:21-15:41:29 hora local). Es INV-PBT2C-10 y no cuenta. La biblioteca y la carpeta de `ArchivoTemporal` siguen iguales.
- **Las corridas locales de UI:** las 76 están en los filtros del contrato, ninguna lleva un filtro de método, y nunca corrió la suite completa de UI. Core completa sí corrió (rc1 y gcf), que está permitido.
- **El Plugin, WX y AutoCAD:** el Plugin y WX no cambian. El Plugin solo se compila, con `-o` hacia la carpeta de la entrega. No vi rastro de AutoCAD, pero no puedo afirmarlo del proceso (ver «Lo no verificado»).

## 8. Hallazgos OWNER

**OWNER-01 (informativo; no bloquea).** El lado B declarado sin frentes sobrevive en el borrador de WORKSPACE.
- **Qué fija CT-02:** `:320` (paquete §5.4) dice que, tras declarar B sin frentes y «Aplicar», el borrador queda con `borrador-B-presente=True` y un diseño no compuesto (`diseno-del-borrador-compuesto=False`), con el aviso «así no se guardará» visible.
- **En CLASSIC,** «Actualizar» escribe ese mismo diseño no compuesto (CT-PBTC-08: `compuesto=False`), y la declaración de B se pierde al reabrir con RACKEDITAR.
- **En WORKSPACE,** el estado del borrador conserva la declaración. Una reapertura del editor desde ese borrador cargaría `SideBPresent=True` y volvería a mostrar el aviso (D-PBT2-05: carga una copia del estado, sin pasar por un diseño). Lo deduzco del código; no lo ejecuté.
- **Lo que significa:** es una diferencia observable entre los dos modos durante la vida del borrador.
  - El paquete la fija como línea literal, pero no la presenta al Owner como diferencia de modo: el §8.4 no la recoge.
  - El aviso sigue siendo cierto respecto al dibujo.
  - Qué escribe el panel queda para PB-T3 y PB-T4.
- **Qué hacer:** la sesión decide si la añade a lo que se informa al Owner.

**Lo que miré y descarté como hallazgo OWNER:**
- **«Restaurar» tras editar** da `aplicado=True`, porque restaura el último cálculo válido, que ya incluye la edición. Es igual en CLASSIC.
- **La selección y el lado frontal** no llegan al borrador. Está declarado en el §5.4 y en L-PBT2-4.
- **`StructureOverrideBox=6` se niega** en los dos modos. Es el tercer caso del §5.4.
- **`PostBox`, `CellInOutBeamBox` y `CellInOutPeralteBox`** dan gestos inertes. Es M-13 de CA-T2c, declarado en `KnownLimitations[4]`.

## Lo no verificado

- **No ejecuté dotnet ni pruebas,** por la regla del encargo. Todo lo de ejecución sale de los TRX y logs copiados en `rv/`.
- **Los mensajes de xUnit van truncados:**
  - en m1, solo veo que la fila F1 difiere después de `diseno=4`; deduzco el valor redondeado;
  - en m3 (CT-02, posición 12), deduzco `borrador-B-presente=False`;
  - lo mismo en CT-PBT1A-10, CT-PBT1A-14, CT-PBTC-01 y 02 y CT-PBT2B-03 del RED: las firmas que se ven son coherentes con la atribución, pero no completas.
- **La clase de las corridas m1 a m3** no se puede atar por `TreeSha`, que es nulo. Solo coinciden las líneas de pila (`:192`, `:323` y `:546`) con la clase commiteada.
- **Los parches `mut/mu1.patch` a `mu3.patch`** no están en `rv/`; no los leí.
- **Los run de GitHub (44618262418 y 44619492077)** no los contrasté con GitHub: el conector de GitHub no estaba disponible. Me baso en `RemoteFacts` del registro del relevo y en los TRX copiados.
- **Que no corriera AutoCAD** solo se apoya en el registro de procesos del relevo.
- **OWNER-01:** el comportamiento de reapertura lo deduzco del constructor de WORKSPACE (W:172-190); no lo ejecuté.
- **Una desviación mía del procedimiento:** al principio ejecuté una vez `python -I -c` para listar las claves de `worker-handoff.json`. Fue de solo lectura y sin efecto, pero el encargo prohíbe `-c`. Después usé solo guiones escritos con la herramienta de archivos. No leí, listé ni ejecuté nada en el worktree de la rama, y no escribí en el repositorio.
