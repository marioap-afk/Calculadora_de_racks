# I-64 — Paquete de cierre del gate F1 (Workspace Foundation)

Preparado por la sesión responsable el 2026-10-11, por orden de la revisión de T3C (`F1-T3C-PROFILE-DOC/coordinator-review.md`,
evidencia §59), para **COORDINATOR_F1_GATE_CLOSE_REQUIRED**.

Este paquete:
- **no** declara F1 PASS, GATE PASS, Candidato ni FINAL_CANDIDATE_SHA. F1 PASS lo decide el Coordinator;
- no ejecuta AutoCAD ni Smoke-1, no pide la Owner Validation y no inicia F2;
- no cambia producto.

Todos los SHA y corridas de CI citados se comprobaron con `git` y `gh run view` (`headSha`, `event`, `conclusion` y jobs).

**Revisiones:**
- **Versión 1** (`b35548fd`, CI 38169928014 4/4): revisión del cierre del gate F1 = **CHANGES REQUIRED**
  (`F1-GATE/coordinator-gate-close-review-1.md`, evidencia §62). El producto queda aceptado; T1..T3D siguen cerrados o aceptados; el
  único bloqueo material es D-T3D-01 / INV-24; el paquete queda aceptado en lo sustancial, con sus residuales R-01..R-12.
- **Versión 2** (`6a574fc0`, CI 38171122754 4/4): incorpora la propuesta I-64-A-7, el mapeo de INV-24 (sección 4.1), los
  dictámenes D-T3D-01..06, la revisión de T3D, la corrección de `f1_resume_condition` y los residuales R-13 y R-14.
- **Versión 3** (esta): **I-64-A-7 AGREED / ACTIVE** (revisión del Architect `R20261011T202537Z-1990` AGREED, 0 REQUIRED,
  0 OPTIONAL; activación del Coordinator en `F1-GATE/A-7/coordinator-a7-activation.md`; evidencia §63). Fija el mapeo final de
  INV-24 (sección 4.1), dispone R-13 y R-14 y deja la siguiente acción en la revisión final del gate por el Coordinator (sección 7).
  No reescribe la evidencia histórica de las tareas. El producto sigue siendo exactamente `src/` y `tests/` de `e0205b5a`.

## 1. Identidad del gate

| Hecho | Valor |
|---|---|
| Unidad / gate | I-64 / **F1 — Workspace Foundation** (Freeze V5 §11) |
| Rama | `architecture/workspace-persistente-rackcad` |
| `origin/main` | `bb0d5522e8411f66a51fdfb3f1f0d0514b737453`, sin cambios desde el rebase de §39 |
| Producto de F1 | el árbol `src/` y `tests/` de **`e0205b5a9f000bbd44aa1f7556d30f06411c3db7`**, el último producto verificado (T3B). `git diff --quiet e0205b5a HEAD -- src tests` sigue vacío en cada punta documental posterior, incluida la de esta versión 3 |
| Suites completas sobre el producto | CI 38129212761 (push, 4/4) sobre `e0205b5a`: Core 13122/13122 y UI 1701 superadas con 17 Skip existentes |
| SHA de cierre propuesto | el commit que versiona este paquete (documental; `src/` y `tests/` idénticos a `e0205b5a`) con su CI exacta 4/4 |
| `attempts` | 3 de 3 (AttemptsRemaining 0), sin reinicio en ninguna tarea; `CorrectionsAuthorized` false |

## 2. Cadena de tareas de F1

**Tareas de producto:**

| Tarea | RED (CI) | Producto verificado (CI push 4/4) | Verificación del Controller | Cierre de protocolo | Revisión del Coordinator |
|---|---|---|---|---|---|
| **F1-T1-MODEL** | `a9778068` (37019198364, RED) | `39f7caa411a5ebb372614c233a32255de7398cca` (37046833476). Tras el rebase: `f593cb8b`, mismo patch-id; revalidación `R20261010T032134Z-0e9a` sobre `8e6a1b4e` (Core 12646/12646, Workspace 51/51; CI 38020256918) | `R20261002T182925Z-5c9b`: EXECUTION_VERIFIED; antes `R20261002T143410Z-ae0b` sobre `8d9a0c6e` (CI 37020159700) | COMPLETE_UNDER_I64_SCOPE_BRIDGE (A-4; ScopeBridge `R20261010T031627Z-124b`) | aceptación en `F1-T2-BRIDGE/coordinator-decision-gate-contract.md` (sobre `9ff0a287`, CI 38022131380, §40); no hay un `coordinator-close-review.md` propio |
| **F1-T2-BRIDGE** | `31e3c57a` (38084788650, RED) | `e2f642b86197441eea0838c8055c1db666606f11` (38085381131) | `R20261010T225551Z-41fd`: EXECUTION_VERIFIED / NONE | SATISFIED UNDER A-4 + A-6 | `F1-T2-BRIDGE/coordinator-close-review.md`: **PASS** sobre `32d38688` (CI 38109192296), §47 |
| **F1-T3A-HOST-CORE** | `de8e7673` (38116482614, RED) | `ba52b3b983fb7d2a02393df643e8323b125b3d03` (38116594290) | `R20261011T061322Z-5734`: EXECUTION_VERIFIED / NONE | SATISFIED UNDER A-4 + A-6 | `F1-T3A-HOST-CORE/coordinator-close-review.md`: **PASS** sobre `7869e5fa` (CI 38120061176), §52; registrada en `ee8018e0` (CI 38120717985) |
| **F1-T3B-HOST-UI** | `9ac76ee2` (38128156191, RED) | **`e0205b5a9f000bbd44aa1f7556d30f06411c3db7`** (38129212761) | `R20261011T100844Z-37e1` STOP C-02 (conservada); re-verificación excepcional `R20261011T153749Z-7910`: EXECUTION_VERIFIED / NONE | SATISFIED UNDER A-4 + A-6 | `F1-T3B-HOST-UI/coordinator-close-review.md`: **PASS** sobre `ffd111a9` (CI 38155884290), §57; registrada en `827917aa` (CI 38156668348) |

**Tareas documentales:**

| Tarea | Entregable | Commit (CI push 4/4) | Revisión |
|---|---|---|---|
| F1-T0-ADR | ADR `propuesto` desde el anexo A | sustituida por T1: `docs/adr/0047-workspace-persistente-modeless-rackcad.md` (**propuesto**), creado en el RED de T1 (`36c344dc` → `b29a1f46` tras el rebase) y sin cambios después | cubierta por la aceptación de T1 |
| **F1-T3C-PROFILE-DOC** | `docs/initiatives/I-64-f1-t3c-profile-diff-procedure.md`, con herramientas y registro de validación | `9af9ac75` (38158957028) | `F1-T3C-PROFILE-DOC/coordinator-review.md`: **PASS**, COMPLETE, APROBADO PARA USO FUTURO EN SMOKE-1; registrada en `795c95c4` (CI 38168220998), §59 |
| **F1-T3D-SMOKE-PROCEDURE** | `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md` y la plantilla del registro del operador | `8fe54f76` (38169083861) | **ACEPTADO CON CORRECCIONES DOCUMENTALES** en la revisión del cierre del gate F1 (§62); correcciones en la versión 2 (`6a574fc0`). D-T3D-01 resuelto por I-64-A-7 AGREED / ACTIVE y campos de los dos dibujos obligatorios en esta versión 3 (§63) |

**Controles por tarea** (los históricos se conservan sin reetiquetar):

| Tarea | nc1 | nc2 | nc3 | nc4 | Puentes |
|---|---|---|---|---|---|
| T1 | PASS `b10b` | FAIL permanente `8415` | PASS `8b6f` | PASS `8d3c` | Scope `124b` PASS |
| T2 | PASS `0935` | NOT PASSED `e415` | NOT PASSED / P-04 | PASS `c445` | Scope `647e` PASS; FreeText `cb29` PASS |
| T3A | PASS `a33e` | NOT PASSED `162f` | NOT PASSED / P-04 | PASS `5e57` | Scope `324b` PASS; FreeText `7dd5` PASS |
| T3B | PASS `c857` | NOT PASSED `d68b` | NOT PASSED / P-04 | PASS `151d` | Scope `b8c9` PASS; FreeText `b06a` PASS |

## 3. Resultado del gate (Freeze §11, fila F1) y su evidencia

| Resultado congelado | Evidencia |
|---|---|
| `RACKPANEL` | T3A: comando sin alias y censo de comandos de 37 a 38 (`CustomPropertiesCommandGuardTests`, `SelectiveEditorOpenTests`); T3B: muestra la paleta. Procedimiento T3D: SM-03 y SM-04 |
| Contexto del documento activo | T1 (sesiones e identidad de instancia); T3A (`WorkspaceHostCore`, instantánea y sesión activa); T3B (relevo de `ActiveSessionChanged`) |
| Selección fina (Ninguno, Uno, Selección (N) con recuento, Sin identidad, Diagnóstico) sin enumeración global | T1 `SelectionContext.Classify` (`WorkspaceSelectionContextTests`); T3A `WorkspaceSelectionReader` (`WorkspaceSelectionMappingTests`); T3B presentación (`WorkspacePanelContextTests`) |
| Puente único con encolado y drenaje | T2: `WorkspaceBridgeSubscriptionLifecycleTests`, `WorkspaceBridgeHintAndDrainTests`, `WorkspaceBridgeDegradedModeTests`, `WorkspaceBridgeSourceGuardTests` |
| Identidad de sesión | T1 `WorkspaceSessionTests`; T2 identificador nativo reciclado; T3A `WorkspaceHostAuthorizationTests` y `WorkspaceBridgeAuthorizeTests` |
| Política de foco | T3B: `KeepFocus` público, mecánica no congelada (D-01); su verificación es solo de host (INV-24): S1-06f..h en Smoke-1 y, por I-64-A-7 (AGREED / ACTIVE), S2-16..S2-22 en Smoke-2, obligatorios y vinculantes (sección 4.1) |
| Contención | T2: modo degradado; T3A: contención de lecturas fallidas; T3B: estado «Error del panel» y «Reintentar» (`WorkspacePanelErrorTests`) |
| Instrumentación | T3A `WorkspaceInstrumentation` en memoria (`WorkspaceInstrumentationTests`); T3B presentación e interruptor (`WorkspacePanelInstrumentationTests`) |
| ADR `propuesto` | ADR 0047 (sección 2) |
| Procedimiento de diff de perfil | T3C, APROBADO |
| Cero escrituras authored | INV-21 (sección 4) |

## 4. Invariantes de cierre de F1 y su evidencia exacta

Las pruebas se nombran por clase y, entre paréntesis, la tarea que creó el archivo y algunos métodos clave. Todas existen en
`e0205b5a` y pasan en su CI 38129212761, y en cada CI 4/4 posterior sobre el mismo árbol de producto. Los contratos citados son los
`gate-contract.json` custodiados de cada tarea; en todas, `delegation.json` lleva los mismos invariantes.

| INV | Freeze: verificación / señal de fallo | Invariantes de contrato | Pruebas en `e0205b5a` | Notas |
|---|---|---|---|---|
| **INV-01** | Core con puente simulado: reposos sin pistas → cero lecturas, resolver y `Regen` / un drenaje por reposo cuenta lecturas | INV-F1-T3A-09; INV-F1-T3B-06 | `WorkspaceHintDrainTests` (T1: `WithoutHintsThereIsNoWork…`); `WorkspaceBridgeHintAndDrainTests` (T2: `WithoutPendingHintsAnIdlePassDoesNothing`); `WorkspaceHostLifecycleTests` (T3A: `AVisibleIdlePanelReadsNothing…`); `WorkspacePanelNoWorkTests` y `WorkspacePaletteHostSourceGuardTests` (T3B) | parte de host: S1-09 y escenario 2 (T3D SM-12) |
| **INV-02** | Core/UI con fuente simulada (`RACKPANEL` repetido, A→B→A, ocultar y mostrar, `Idle`, destrucción) y guarda: `+=` solo en la carpeta del puente / el recuento crece | INV-F1-T2-01, 02; INV-F1-T3A-01; INV-F1-T3B-04, 05 (G3) | `WorkspaceBridgeSubscriptionLifecycleTests` y `WorkspaceBridgeSourceGuardTests` (T2); `WorkspaceSessionTests` (T1); `WorkspaceHostLifecycleTests` (T3A: `ARepeatedOpen…`), `WorkspaceHostPolicyTests`, `WorkspaceBridgeFacadeSourceGuardTests` (T3A); `WorkspaceHostSourceGuardTests` G3 y `WorkspacePaletteHostSourceGuardTests` (T3B) | «10 pestañas» es de F3 (las pestañas no existen en F1) |
| **INV-03** (lado de eventos) | Core: contexto previo + acción + eco → cero llamadas de selección o zoom / un contexto que reescribe | INV-F1-T2-03; INV-F1-T3A-08 | `WorkspaceBridgeHintAndDrainTests` (T2: `NoEventAndNoDrainWrites…`), `WorkspaceBridgeSourceGuardTests`; `WorkspaceHostSourceGuardTests` (T3A: `NoHostFileWritesTheSelectionTheView…`); `WorkspaceHintDrainTests` (T1: `HintsAreQueuedAndNeverChangeTheContext`) | la parte de pestañas y eco es de F2 |
| **INV-07** | Core: el mismo RackId en dos sesiones → contextos independientes / una clave por RackId o ruta mezcla sesiones | **ningún contrato emitido cita INV-07**; el más cercano es INV-F1-T1-02 | `WorkspaceSessionTests` (T1: `RackKeysNeverCrossSessions`, `TwoOpenInstances…`); `WorkspaceHintDrainTests` (T1: `HintsOfOneSessionNeverReachAnother`); `WorkspaceHostLifecycleTests` (T3A: `…KeepsEachContextApart`) | borradores y pestañas son de F4; host: S1-05 (T3D SM-20..24). Residual R-01 |
| **INV-13** | existente: `UiSystemBoundaryGuardTests`, ampliada / sigue en verde | INV-F1-T3B-07 | `WorkspacePaletteHostSourceGuardTests` (T3B); guardas globales sin cambios: `UiSystemBoundaryGuardTests`, `ProjectVariablesConformanceTests.LA_UI_ENTERA_SIGUE_SIN_CONOCER_AUTOCAD` y `CantileverRoundTwoSourceGuardTests.NingunProyectoFueraDelPluginNombraAutodesk`, que recorren el proyecto UI entero | discrepancia registrada en el paquete de T3B: `UiSystemBoundaryGuardTests` solo comprueba `x:Class`, carpetas y URIs; la protección real son las guardas globales. Residual R-02 |
| **INV-14** | guarda de fuente (`StartTransaction`, `StartOpenCloseTransaction` y `LockDocument` solo en `using`; sin campos de `Transaction`, `DBObject` ni `Document`; puerto sin tipos de AutoCAD); comprobación Debug de `TopTransaction == null` «registrada en los smokes» | INV-F1-T2-07; INV-F1-T3A-03, 12 | `WorkspaceBridgeSourceGuardTests` (T2); `WorkspaceHostSourceGuardTests` (T3A: `EveryTransactionAndLock…InsideUsing`, `TheReaderChecksInDebug…`); `WorkspaceHostPortGuardTests` (T3A); `Debug.Assert` en `WorkspaceController.cs:137` y `WorkspaceSelectionReader.cs:92` | el registro de host es de Smoke-1 (T3D SM-38 y SM-39; D-T3D-04) |
| **INV-16** | Core: sobre sin Id, también en blanco → diagnóstico, no editable, sin clave / el sobre recibe clave o `Classify` lanza | INV-F1-T3A-07; INV-F1-T3B-08 | `WorkspaceSelectionContextTests` (T1: `OneIsNeverBuiltFromABlankId`); `WorkspaceHostDrainTests` y `WorkspaceSelectionMappingTests` (T3A); `WorkspaceHostSourceGuardTests` (T3A: `NoRackIdIsEverStamped…`); `WorkspacePanelContextTests` (T3B) | host: S1-04 «Sin identidad» (T3D SM-10) |
| **INV-17** | Core: cada caso ilegible o de versión futura → diagnóstico visible y no editable / un filtro que lo omite | INV-F1-T3A-07; INV-F1-T3B-08 | `WorkspaceSelectionContextTests` (T1: `AnUnreadableDefinitionIsADiagnostic…`); las mismas de T3A y T3B que INV-16 | host: S1-04 «Diagnóstico» (T3D SM-11; D-T3D-03) |
| **INV-21** | guarda: el puerto no expone escrituras salvo Actualizar y el descarte; host: diff de perfil de Smoke-1 / el puerto expone otra escritura | INV-F1-T2-08 (acotado por C-T2-04); INV-F1-T3A-04, 13 | `WorkspaceModelBoundaryTests` (T1: `ModelDoesNotPersist…`); `WorkspaceBridgeSourceGuardTests` (T2); `WorkspaceHostPortGuardTests` y `WorkspaceHostSourceGuardTests` (T3A: `NoHostFileWritesAFileTheRegistry…`); `WorkspacePaletteHostSourceGuardTests` (T3B: `TheUiFolderWritesNothing…`) | host: diff de perfil de T3C en Smoke-1 (R-F-08 y la sección 15 de T3C) |
| **INV-22** | Core: petición de A con B activo → rechazada; identificador reciclado = sesión vacía / una petición sin identidad se ejecuta | INV-F1-T2-10; INV-F1-T3A-03 | `WorkspaceSessionTests` (T1: `RequestBornInAExecutedWithBActive…`, identificador reciclado); `WorkspaceBridgeSubscriptionLifecycleTests` (T2: `ARecycledNativeIdentifier…`); `WorkspaceHostAuthorizationTests`, `WorkspaceBridgeAuthorizeTests` y `WorkspaceHostPortGuardTests` (T3A) | — |
| **INV-23** | Core con fuente simulada: pistas durante un modal o un comando → en cola; al cumplirse las condiciones, un drenaje / se drenan en cuanto llegan | INV-F1-T2-06 | `WorkspaceHintDrainTests` (T1: `HintsKeepArrivingDuringAModal…`); `WorkspaceBridgeHintAndDrainTests` (T2: `NothingDrainsWhileACommand…`, `…RackCadModalIsActive…`); `WorkspaceHostLifecycleTests` (T3A: `ARackCadModalDisables…`, `WhenTheModalCloses…`) | `SetModalRackCadActive` no tiene ningún llamador fuera de la tubería del Workspace (H-B5 y C-T3B-09; D-T3D-06). Residual R-03 |
| **INV-25** (panel) | UI: excepción en un manejador y en la materialización → no se propaga; estado Error; borrador intacto / la excepción escapa | INV-F1-T3B-09, 10 | `WorkspacePanelErrorTests` (T3B); `WorkspacePaletteHostSourceGuardTests` (T3B: `EveryEntry…IsContained…`); relacionadas: `WorkspaceBridgeDegradedModeTests` (T2) y la contención de lecturas en `WorkspaceHostDrainTests` (T3A) | «borrador intacto» es de F4; sin inyecciones en F1 (D-T3D-02). Residual R-05 |
| **INV-33** | Core con puente simulado: panel oculto + pistas → cero lecturas; al mostrar, una reconciliación / se lee con el panel oculto | INV-F1-T3A-09; INV-F1-T3B-04, 06 | `WorkspaceHintDrainTests` (T1); `WorkspaceBridgeHintAndDrainTests` (T2: `…ThePanelHiddenNothingDrains`); `WorkspaceHostLifecycleTests` (T3A: `AHiddenPanelReadsNothing…`, `ShowingTheHiddenPanelReconciles…`); `WorkspacePaletteHostSourceGuardTests` (T3B: `…RefreshesThePanelOnlyWhileVisible…`) | host: T3D SM-16 |

**Clases de prueba de Workspace por tarea** (métodos `[Fact]`/`[Theory]` en `e0205b5a`):
- **T1:** `WorkspaceSessionTests` 8, `WorkspaceHintDrainTests` 8, `WorkspaceModelBoundaryTests` 7, `WorkspaceSelectionContextTests` 10.
- **T2:** `WorkspaceBridgeSubscriptionLifecycleTests` 17, `WorkspaceBridgeHintAndDrainTests` 16, `WorkspaceBridgeDegradedModeTests` 11,
  `WorkspaceBridgeSourceGuardTests` 21.
- **T3A:** `WorkspaceHostLifecycleTests` 33, `WorkspaceHostDrainTests` 32, `WorkspaceHostSourceGuardTests` 43,
  `WorkspaceSelectionMappingTests` 23, `WorkspaceHostPolicyTests` 18, `WorkspaceHostPortGuardTests` 16,
  `WorkspaceHostAuthorizationTests` 13, `WorkspaceBridgeReconciliationTests` 17, `WorkspaceBridgeAuthorizeTests` 7,
  `WorkspaceBridgeFacadeSourceGuardTests` 17, `WorkspaceInstrumentationTests` 16, `WorkspaceSourceGuardExceptionTests` 10.
- **T3B:**
  - Core: `WorkspacePaletteHostSourceGuardTests` 41;
  - UI: `WorkspacePanelErrorTests` 11, `WorkspacePanelContextTests` 11, `WorkspacePanelStateTests` 12,
    `WorkspacePanelInstrumentationTests` 10, `WorkspacePanelNoWorkTests` 8, `WorkspacePanelViewTests` 8.

### 4.1 INV-24 (no es de cierre de F1): mapeo final bajo I-64-A-7 (AGREED / ACTIVE)

INV-24 no está en la lista de cierre de F1 (Freeze §11) y A-7 no la añade. El bloqueo material de la revisión del cierre (D-T3D-01)
queda resuelto por **I-64-A-7, AGREED / ACTIVE**:
- M-04 = YES; autoridad Architect + Coordinator;
- Architect AGREED (`R20261011T202537Z-1990`, `gpt-6.1-sol` / high / read-only, 0 REQUIRED, 0 OPTIONAL);
- Coordinator AGREED;
- aprobación del Owner NOT REQUIRED; Owner INFORMED.

OV-19 y la obligación del campo editable no se retiran, sustituyen ni debilitan: su verificación literal se reasigna y sigue siendo
obligatoria.

| Caso | Hito | Estado bajo A-7 (AGREED / ACTIVE) | Evidencia |
|---|---|---|---|
| S1-06a..S1-06e (campo del panel) | Smoke-1 | **NOT_APPLICABLE_BY_DESIGN**: el Workspace de F1 no tiene, por diseño, ningún campo de texto editable (`WorkspacePanelView.xaml` en `e0205b5a`). **Nunca se informan como PASS en F1** | — |
| SM-14a..SM-14e (casilla «Medir en memoria») | Smoke-1 | **SUPPLEMENTARY_EVIDENCE** únicamente; no sustituyen el comportamiento de un texto editable | T3D, sección 6.6 |
| S1-06f (comando activo → acciones deshabilitadas) | Smoke-1 | **obligatoria** | T3D SM-13; pruebas Core de la política: `WorkspaceHostPolicyTests` y `WorkspaceHostLifecycleTests` (T3A; bloqueo `CommandActive`). Ninguna prueba de UI cita el texto «Hay un comando en curso.» |
| S1-06g (el foco vuelve solo según D-01) | Smoke-1 | **obligatoria**; en F1 solo vuelve por una acción explícita | T3D SM-15; `KeepFocus` en `WorkspacePaletteHost.cs` |
| S1-06h (`DBMOD` bit 1) | Smoke-1 | **obligatoria**; sobre S1-06f, S1-06g y SM-14a..e | T3D SM-14h y SM-18 |
| S2-16 (un texto con espacios se queda en el campo editable y no llega a AutoCAD) | Smoke-2 (F4) | **MANDATORY / BINDING** | futuro |
| S2-17 (Ctrl+Z edita el texto y no ejecuta el UNDO de AutoCAD) | Smoke-2 (F4) | **MANDATORY / BINDING** | futuro |
| S2-18 (Ctrl+C / Ctrl+V operan sobre el texto, no sobre objetos del dibujo) | Smoke-2 (F4) | **MANDATORY / BINDING** | futuro |
| S2-19 (Supr en el campo no borra objetos del dibujo) | Smoke-2 (F4) | **MANDATORY / BINDING** | futuro |
| S2-20 (Esc / Intro en el campo no cancelan ni repiten comandos de AutoCAD) | Smoke-2 (F4) | **MANDATORY / BINDING** | futuro |
| S2-21 (`DBMOD` bit 1 sin cambios en S2-16..S2-20) | Smoke-2 (F4) | **MANDATORY / BINDING** | futuro |
| S2-22 (terminar una edición no devuelve el foco al dibujo solo por haber terminado) | Smoke-2 (F4) | **MANDATORY / BINDING**, aditiva | futuro |

**Mapeo final:**
- **INV-24, parte de F1 = SATISFIED UNDER I-64-A-7**, con:
  - S1-06f, S1-06g y S1-06h, que siguen en Smoke-1;
  - SM-14a..e como evidencia complementaria;
  - los casos con campo editable diferidos y vinculantes a S2-16..S2-22.
- **No** se afirma que INV-24 quede verificada del todo en F1.
- **INV-24 en todo el ciclo de vida sigue OPEN** hasta que se ejecuten esos escenarios de host diferidos y su asignación OV final.
  **No bloquea F1 PASS** una vez satisfecho el resto del gate.
- F4 / Smoke-2 no puede omitir S2-16..S2-22 sin otra A-n aceptada. Si un hito anterior del Owner contiene un campo de texto editable
  real del Workspace, pueden ejercitarse además allí, pero eso no retira su asignación vinculante a F4 sin enmienda.
- OV-19 pasa a S1-06f..h y S2-16..S2-22, y sigue ejecutándose sobre `FINAL_CANDIDATE_SHA`.

## 5. Lo que F1 deja para Smoke-1, después de F1 PASS

Por el Freeze, estas verificaciones son de host y no son criterio de cierre de F1 (A64-PV3-03: ningún cierre exige evidencia de host
que el Worker no puede producir):
- **INV-24:** foco, S1-06f..h (obligatorias), con SM-14a..e como evidencia complementaria. S1-06a..e son NOT_APPLICABLE_BY_DESIGN, nunca PASS; la obligación literal pasa a S2-16..S2-22, obligatorias y vinculantes en Smoke-2, por I-64-A-7 AGREED / ACTIVE (sección 4.1).
- **Parte de host de INV-21:** el diff de perfil (S1-11).
- **Registro de la comprobación Debug de INV-14.**
- **Escenarios 1, 2, 3, 9 y 10:** OV-22, como registro sin umbrales.
- **S1-01..S1-12:** OV-01..04, 14, 15, 17, 19 y 20.

El procedimiento exacto es el de T3D. Smoke-1 exige:
- un Candidato completo: UI Full local y la CI del SHA exacto con `ui-tests` en success;
- los **dos dibujos obligatorios** (T3D PR-12 y PR-13), con su archivo de origen, SHA-256, procedencia y propósito, título y texto
  exactos esperados y la regla de no guardarlos:
  - **SIN IDENTIDAD:** un objeto que RackCad reconoce y puede leer, con un sobre lo bastante válido para clasificarse como RackCad
    pero con el RackId en blanco; esperado «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.». Un bloque
    genérico sin datos de RackCad **no** vale;
  - **DIAGNÓSTICO:** la condición del clasificador actual de definición ilegible, inválida o futura; esperado «Diagnóstico» / «No se
    pudo interpretar la definición del objeto seleccionado.»;
  - si falta cualquiera de los dos: **S1-04 = INCOMPLETE** y Smoke-1 no puede ser PASS. No se lanza AutoCAD para crearlos antes de
    la autorización de Smoke-1;
- el RELEASE explícito de I-52. A día de hoy no consta ninguno: §258 de I-52, P2 H10 no autorizada.

## 6. Residuales y hallazgos para la revisión del Coordinator

| Id | Hallazgo | Propuesta |
|---|---|---|
| R-01 | INV-07 no aparece citado en ningún contrato emitido. Su evidencia existe en pruebas de T1 y T3A (sección 4) | aceptar la evidencia por pruebas o pedir una nota de trazabilidad |
| R-02 | INV-13: el Freeze nombra `UiSystemBoundaryGuardTests`, que no cubre referencias a AutoCAD; las guardas globales sí (discrepancia del paquete de T3B) | aceptar las guardas globales como verificación de INV-13 |
| R-03 | INV-23: `SetModalRackCadActive` sin llamador. Conectar los modales existentes reabriría comandos clásicos (M-03; C-T3B-09) | aceptar la evidencia Core del Freeze («Core con fuente simulada») y dejar la conexión para una decisión posterior; D-T3D-06 para Smoke-1 |
| R-04 | INV-02: la parte de «10 pestañas» no se puede verificar en F1 | es de F3 por diseño |
| R-05 | INV-25: sin inyecciones de fallo en F1; «borrador intacto» es de F4 | D-T3D-02 |
| R-06 | INV-14 y INV-17 en el host | D-T3D-04 y D-T3D-03 |
| R-07 | decisiones de T3D antes de ejecutar Smoke-1 | D-T3D-01..06. **Dispuestas** en la revisión (§62) y en la activación de A-7 (§63): D-T3D-01 resuelto por I-64-A-7 AGREED / ACTIVE; D-T3D-02 aceptada; D-T3D-03: los dos dibujos (Diagnóstico y Sin identidad) son prerrequisito de Smoke-1 PASS; D-T3D-04 aceptada con corrección de redacción; D-T3D-05 confirmada; D-T3D-06 aceptada para S1-07 |
| R-08 | `protocol_debt` activa: el Controller puede dar Scope pass cuando falla su comparación auxiliar (nc2 `8415`) | sigue para I-62; no bloquea I-64 (A-4 y A-6) |
| R-09 | `f1_resume_condition` en `I-64.yml` conserva la redacción anterior a A-4, mientras `f1_blocking_dependency` dice «resuelta para I-64» | **corregida en la versión 2** (§62), por orden de la revisión; texto anterior en `b35548fd`. Propuesta original: corregir esa línea en el commit del dictamen, si el Coordinator lo ordena |
| R-10 | ADR 0047 sigue `propuesto` y no figura en `docs/adr/README.md` (archivo caliente) | su aceptación es del Owner; fuera del cierre de F1 |
| R-11 | T1 no tiene un `coordinator-close-review.md`; su aceptación está en la decisión del contrato de T2 (§40) | aceptar esa decisión como revisión de cierre de T1 |
| R-12 | El Controller nunca cumplió el oráculo de nc2 (T1-T3B) ni el de nc3 (T2-T3B). Los cierres de protocolo son por los puentes sustitutos A-4 y A-6, y se conservan las desviaciones aceptadas: regla de nc3 por analogía en T3A, espacio de `HostCorePath` en T3B y la señal P-02 falsa | ninguna acción; registro |
| R-13 | **I-64-A-7** (`docs/initiatives/I-64-A-7.md`). Versión 2: PROPOSED / NOT ACTIVE | **DISPUESTO**: **AGREED / ACTIVE** (§63). M-04 = YES; Architect + Coordinator; Architect AGREED en `R20261011T202537Z-1990` (0 REQUIRED, 0 OPTIONAL); Coordinator AGREED; Owner NOT REQUIRED / INFORMED. El texto de `I-64-A-7.md` no se edita (append-only); su estado consta en el estado y la evidencia |
| R-14 | **Corrección de T3D hallada por la sesión:** un bloque sin datos de RackCad da «Selección actual: 0 racks y 1 otros objetos.», no «Sin identidad» (`SelectionContext.Classify` y `WorkspaceSelectionReader`). «Sin identidad» exige un sobre legible con Id vacío. T3D separa SM-10a y SM-10b y añade el dibujo de PR-13 | **DISPUESTO**: el Coordinator confirma que los **dos** dibujos (PR-12 Diagnóstico y PR-13 Sin identidad) son obligatorios para Smoke-1 PASS, con los campos de la sección 5; sin cualquiera, S1-04 = INCOMPLETE (§63) |

## 7. Lo que queda

1. **Revisión final del gate F1 por el Coordinator** (**COORDINATOR_F1_GATE_CLOSE_REQUIRED**) sobre esta versión 3 y su CI exacta
   4/4. A-7 ya está AGREED / ACTIVE; R-01..R-12 siguen aceptados en lo sustancial (con R-07 y R-09 al día); R-13 y R-14 quedan dispuestos. No hace falta una
   verificación de producto nueva: `src/` y `tests/` no cambian. F1 PASS no se declara antes de esa revisión.
2. Tras F1 PASS:
   - Candidato de Smoke-1;
   - los dos dibujos PR-12 y PR-13, con sus campos;
   - RELEASE y acuse de I-52, todavía no pedidos;
   - Smoke-1 según T3D.

   F2 no se inicia sin autorización.
3. INV-24 en todo el ciclo de vida sigue OPEN hasta S2-16..S2-22 (Smoke-2, F4) y su asignación OV final; no bloquea F1 PASS.
4. Smoke-1 PASS y Smoke-NAV PASS son la condición de entrada de F4, donde se ejecutan S2-16..S2-22.
