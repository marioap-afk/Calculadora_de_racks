# I-64 — Paquete del contrato de F4-T4 (PREPARE, MUTATE, POST y UNDO)

**Estado:** v3. La prepara la sesión principal el 2026-11-13.
- La v1 pasó por una revisión adversarial independiente: EMITIBLE TRAS CORRECCIONES (1/12/14).
- La v2 aplicó esa revisión y las cuatro decisiones del Owner (`F4-DRAFTS/coordinator/owner-f4t4-decomposition-decision.md`).
- La re-revisión independiente de la v2 dio:
  - la descomposición, **EMITIBLE**;
  - el contrato de T4A1, **EMITIBLE TRAS CORRECCIONES** (1/3/11), y añade: «basta con que la sesión compruebe N-1 a N-4 en la v3 antes de emitir».
- La v3 aplica sus 15 hallazgos (§10).

**Código base:** `4e34bb56`, el producto de F4-T3C2. HEAD es `7297e1f1`, la custodia de su cierre; entre las dos no cambia nada en `src/` ni en `tests/`.

**Autorización:** la del Owner y el Coordinator, `F4-DRAFTS/coordinator/owner-f4t4-preparation-authorization.md`.

**Mapas previos:** `map-1-legacy-write-i55-undo.md`, `map-2-drafts-freeze.md` y `map-3-host-seams-guards.md`. Se custodian con el contrato.

Este paquete fija la descomposición de F4-T4 y el contrato de F4-T4A1. No declara GATE PASS.

## 1. Autoridades

- **El Freeze** (`docs/initiatives/I-64-proposal-v5.md`):
  - D-12: la autoridad es la relectura dentro de MUTATE, y la comparación es cruda, sin normalizar. O-04 es una comprobación previa que «no sustituye» a esa relectura. «Comprobar ahora» y la reconciliación son explícitas, sin fusión automática.
  - D-13:
    - todo o nada sobre el authored del rack, con los borrados de fantasmas dentro;
    - el residuo de biblioteca OM-5;
    - «la mecánica no se congela»;
    - la vía de I-55;
    - POST desde una relectura.
  - D-04 y D-10/INV-26 (Dirty semántico).
  - INV-06: una diferencia en la atribución, en un sobre o en el registro es No aplicado por Conflicto.
- **El paquete de F4** (`docs/initiatives/I-64-f4-gate-contract-package.md`):
  - §3, con la descomposición original de T4, que este paquete sustituye por orden del Owner;
  - §4, con C-F4-02, C-F4-04 y C-F4-05;
  - §5, con las obligaciones 5, 11-13 y 16-18;
  - INV-08, INV-10, INV-11, INV-22, INV-25, INV-28, INV-30, INV-31 e INV-39.
- **El paquete de F4-T2:** §3 (la clausura de T1B) y los puntos 152-153.
- **El contrato de F4-T1:** INV-F4T1-04, INV-F4T1-11 e INV-F4T1-20 (`F4-T1-DRAFT-CORE/coordinator/gate-contract-attempt3.json`).
- **IC-07** (`docs/initiatives/I-64-implementation-clarifications.md`).
- **Las disposiciones** (`F4-DRAFTS/f4-dispositions.md`):
  - F4-E18 (T1B);
  - F4-E24;
  - F4-E25 (los arrastres (1)-(6));
  - F4-E26;
  - F4-E35;
  - F4-E38 (la regla de nc1);
  - F4-E46 a F4-E50.
- **Las órdenes del Owner:**
  - la autorización de preparación de F4-T4;
  - las decisiones de descomposición de F4-T4;
  - `owner-f4t3-authorization.md`;
  - `owner-f4t3c2-authorization.md`.
- **Los cierres:** F4-T2A (los arrastres de `ComposeForWrite`), F4-T3C1 (R-3) y F4-T3C2 (C2-M-1 a C2-M-8).

## 2. Hechos medidos en `7297e1f1` (código de `4e34bb56`)

### 2.1 El núcleo de borradores y el piloto

**H-01.** `DraftSession<TState>` separa tres estados:
- `StaleBase`, inmutable (266);
- `InitialDraftState`, una copia (269);
- `CurrentDraftState`, por referencia (272).

Además:
- `IsDirty` es semántico (283).
- `Status` es el estado **efectivo**, `_explicitStatus ?? (IsDirty ? Dirty : Clean)` (285). Este paquete llama «implícito» a la parte Clean/Dirty.
- `IsPossiblyStale` (288) es una marca ortogonal; hoy solo la limpia `Rebase` (`DraftSession.cs:446-457`).
- `NeedsLazyReload` es «registrado, marcado y Clean» (291).
- `LastApplyCause` (303).

**H-02.** La StaleBase guarda por miembro la atribución, `View`, `Section` y el JSON **crudo** del sobre (`DraftStaleBaseEntry`).
- `CompareWith` es ordinal y exacta, como fija D-12.
- «Semántica donde el Freeze lo diga» se aplica a Dirty (INV-26), no al Conflicto.

**H-03.** La máquina de aplicar ya existe:
- `TryBeginApply` (467-483) acepta Clean o Dirty sin texto pendiente, y responde `AnotherUpdateInProgress` si la ranura está tomada;
- `CompleteApply` (490-513) y `StatusBeforeAttempt` (592);
- los ocho resultados, fijados en `WorkspaceDraftApplyTests.cs:591`: NotAppliedConflict, NotAppliedAdmission, NotAppliedPreflight, NotAppliedFailure, CommitThrew, Applied, AppliedWithWarnings y AppliedPostRereadFailed.

**Ningún código de producto llama a `TryBeginApply`, `CompleteApply` ni `KeepMineOnCurrentVersion`.** `DiscardAndReload` y `CheckNow` sí tienen llamador: `TryReloadPilotEdit` y `TryCheckPilotEdit` (`WorkspaceHostCore.cs:489` y 410).

**H-04.** `CheckNow` (571-589) solo actúa en Unknown(CommitThrew), como fija `WorkspaceDraftApplyTests.cs:660`. La comprobación general de D-12 no existe.

**H-05.** `ComposeForWrite` (189-215):
- rechaza lo que el piloto no edita;
- rematerializa desde `initial.Origin`;
- solo copia `AnnotationScale`.

La `PushBackPilotComposition` es mutable (60-88). **Siguen pendientes los cinco arrastres de F4-E25 que F4-T2A pasó a T4A** (§4, D-T4A1-05).

**H-06.** Con MATCH crudo, `initial.Origin` es byte a byte el texto del sobre de origen de la relectura (T2 152-153). `ComposeForWrite` **solo es seguro después de un MATCH**.

**H-07.** Las lecturas del host y el puerto:
- **Las lecturas del host** son `TryBeginPilotEdit` (280-358), `TryCheckPilotEdit` (365-428) y `TryReloadPilotEdit` (438-512). Tienen implementaciones explícitas del puerto (`TryBeginEdit`, `TryCheckNow` y `TryReload`, 553-593). Las tres:
  - comprueban la aplicabilidad antes de leer (paso 6);
  - responden Waiting con `_pilotReadInProgress` (555);
  - contienen toda excepción como Failed.
- **El puerto** tiene 12 operaciones, y `EditCheckStatus` está fijado (`WorkspaceEditIntentHostTests.cs:1022`).
- **`EditIntentStatus`** termina en `Reloaded` (`WorkspaceEditIntent.cs:123-124`) y no tiene NotApplicable. Lo fijan como prefijo ordenado tres guardas: la de 7 de `WorkspaceEditIntentHostTests.cs:1020`, y las de 10 de `WorkspaceLifecycleGuardTests.cs:805-816` y `WorkspaceLifecycleHostTests.cs:1488-1490`. Los resultados son valores (`WorkspaceEditIntentHostTests.cs:1014-1016`).
- **`PilotFieldView`** tiene un solo constructor, cuyo último parámetro es `string? errorReason = null` (`WorkspaceHostHardeningGuardTests.cs:363-369`). No lleva el origen de Desconocido ni el Conflicto.
- **Los textos** son los 28 de la tabla (`WorkspaceHostHardeningGuardTests.cs:398-428`), con sus cuentas exactas (449-455).

**H-08.** El lector del Plugin (`WorkspaceNavigationHost.PilotIntentReader`, 196-260) exige reposo y ningún comando en curso, y lee con `CaptureDrawing`, que confirma su transacción. Por eso:
- **vale** para «Comprobar ahora» en contexto de aplicación;
- **no vale dentro del contexto de comando de Actualizar**, donde el lector responde Busy y la compuerta Waiting (`PassGate`, 1055-1058);
- **tampoco vale dentro de MUTATE** (INV-39).

**H-09.** El puerto tiene tres dobles de prueba:
- `WorkspacePilotEditPortDouble.cs:25`, en la UI;
- `CommandAdvisorPortDouble` y `ForwardingRiskPort` (`WorkspaceCommandAdvisorTests.cs:17` y 1104). `CommandAdvisorPortDouble` registra toda operación no permitida y lanza (`WorkspaceCommandAdvisorTests.cs:13-15` y 47-53).

Los implementadores de producto son el host, la compuerta (`WorkspacePilotSupport.cs:170`) y el adaptador del controlador.

**H-10.** Las guardas del puerto y del host:
- En `WorkspacePilotEditGuardTests.cs`:
  - `Operations` y `ReadOperations` (43-63);
  - los nombres `Draft*` y `PushBack*` prohibidos en el puerto (73);
  - los nombres de tipo del puerto y del soporte (747-762), que no pueden contener `Draft`, `Editor`, `Subscription` ni `Tab`, ni ser una palabra de `Drafts/`;
  - la marca de lectura (357-375), la compuerta (411-439), los reenvíos (490-500) y `WriteViolations` (664-737).
- La marca «conocido limpio» (`WorkspaceLifecycleGuardTests.cs:438-511`): una lista fija de cinco operaciones.
- La contención como Failed (`WorkspaceEditIntentGuardTests.cs:575-589`): solo dos cuerpos.
- Los nombres prohibidos en el drenaje (592-610).
- El uso de la base (635-663): `AllowedConsumers` solo admite `.Begin(`, `.DiscardAndReload(` y `.CheckNow(`.
- Los portadores (`WorkspaceEditIntentHostTests.cs:969-986`).
- El comentario del puerto (`IWorkspacePilotEditPort.cs:14-24`).

**H-11.** Son de Application:
- `RackEmbedComposer.Compose(source, kind, id, name, view, section, design)` (`RackEmbedComposer.cs:43`);
- `RackProject.WithSourceMetadataFrom` (`RackProject.cs:75`);
- `PushBackCompositeEditorAssembler.BuildDesign` (38);
- `PushBackSystemLateralBuilder.Cortes` (91);
- `RackProjectStore.PreflightInnerSources` (`RackProjectStore.cs:228`);
- `RackViewAvailability`.

Dos restricciones:
- `WorkspaceHostSourceGuardTests.cs:373-378` prohíbe `RackEmbedComposer` en `Application/Workspace`, así que el planificador vive en `Systems/PushBack/Pilot/`.
- **El censo exacto de I-54** (`CustomPropertiesEnvelopeGuardTests.cs:69-75`) compara todas las llamadas a `Compose` de `src/` con `ExpectedComposeCensus` (191-203), ordenado por ruta ordinal.
  - Exige que el primer argumento sea un **parámetro** `RackEmbedDocument` del miembro (85-107).
  - El precedente es I-55 G16 (186-190).

**H-12.** **Push Back sí tiene hechos de disponibilidad:** `PushBackViewAvailabilityFacts` (`RackViewAvailability.cs:94`). RACKEDITAR:
- los construye desde los cortes laterales (`RackPushBackCommands.cs:269-271`);
- marca obsoleto un lateral cuyo poste ya no existe, y lo borra si quedan supervivientes (348-356).

### 2.2 La escritura heredada, I-55 y Undo

**H-13.** El Actualizar de RACKEDITAR (`RackPushBackCommands.cs:198-384`) compone todo desde la ventana y no es atómico. Solo sirven sus primitivas, y no se toca (D-11).
- La carga de cada vista es `RackProjectStore.Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(innerSource))`, seguido de `Compose` y `RackEmbedStore.Serialize` (`BuildPushBackPayload`, 439-461, del Plugin).
- El proyecto interior de cada vista lo resuelve `PreflightInnerSources` (221-226).

**H-14.** La costura de I-55 es la vía de D-13, y Push Back no la usa.
- Una excepción de `Commit()` da `Discarded` sin inyección Debug (INV-30).
- `SiblingRedrawSeamGuardTests.cs:13-14` congela su archivo.

**H-15.** El producto no tiene ninguna API de Undo ni de contexto de comando.
- `ExecuteInCommandContextAsync` está prohibido en todo `src` (`WorkspaceNavigationHostTests.cs:436`).
- En Smoke-NAV, el callback corre en `EXECUTEFUNCTION`.

**H-16.** `AnnotationScale` cambia la geometría: Actualizar es un redibujo completo de las vistas miembro.

**H-17.** Las pistas no llevan handle y se agrupan por sesión (`WorkspaceSession.cs:24-26`). No existe ningún ámbito de mutación propia.

**H-18.** La importación de biblioteca que D-13 pone en PREPARE escribe definiciones de bloque (OM-5).

**H-19.** T1B no se ha ejecutado. F4-E18 la fija «antes de la primera tarea que modifique un archivo de esa clausura, y como muy tarde antes de T4A».

## 3. Descomposición de F4-T4 (decisión del Owner)

| Tarea | Responsabilidad | Escrituras authored |
|---|---|---|
| **F4-T4A1-PREPARE-CORE** | El núcleo y el host de la comprobación y la reconciliación, el PREPARE puro con el plan inmutable, los arrastres de `ComposeForWrite` y R-3, todo probado en Core (§4) | Ninguna |
| **F4-T4A2-PREPARE-UI** | «Comprobar ahora» y la reconciliación en el panel, las guardas C2-M-1 y C2-M-2 y el arrastre (5) de F4-E25 (§6) | Ninguna |
| **F4-T4B-MUTATE-POST** | La transacción atómica, la lectura caller-owned (decisión 4 del Owner), las fachadas, las unidades, la importación y POST, probados con puertos simulados y guardas (§7) | Sí |
| **F4-T4C-HOST-UNDO** | El contexto de comando, el ámbito de mutación propia, la UI de Actualizar y la medición en host de Undo como primer paso. Si Undo no es un solo paso o el callback es diferido, va una A-n al Owner (§7) | Sí, la del adaptador del host |

**Orden:** A1 → A2 → B → C.
- Cada tarea se cierra antes de emitir la siguiente y empieza con `attempts` 1 de 3.
- Los paquetes de A2, B y C se fijan sobre el código real de la tarea anterior.

**Presupuesto:** la división duplica la capacidad de intentos de T4A y de T4B. Lo decidió el Owner y se registra en F4-E51.

**Decisiones del Owner sobre el diseño:**
- **Decisión 3:** se acepta la canonicalización `LegacyDeltas`, como en Application. Se declara en T4B y se observa en Smoke-2 con un dibujo antiguo.
- **Decisión 4:** la variante caller-owned de la lectura (INV-39) va en T4B, donde se usa dentro de la transacción. T4A entrega la comprobación previa O-04, la comparación, la clasificación y el plan. **La autoridad de D-12 sigue siendo la relectura dentro de MUTATE (T4B).**

### 3.1 La reubicación de T1B (disposición de la sesión, F4-E51)

**La regla nueva:** T1B se ejecuta antes de la primera tarea que **use las fachadas de Push Back para escribir o modifique un archivo de la clausura de la ventana clásica**, es decir, antes de T4B.

**El motivo:** F4-E18 la puso «como muy tarde antes de T4A» porque el T4A original tocaba las fachadas. A1 y A2 no lo hacen.

**La clausura** es la de §3 del paquete de F4-T2 (`I-64-f4-t2-gate-contract-package.md`):
- la ventana, sus tipos de estado y sus ensambladores en Application y en la UI;
- los controles y temas que usa;
- sus subdiálogos;
- las fachadas de escritura de Push Back del Plugin.

**Salvedad declarada:** `src/RackCad.Application/Systems/PushBack/Pilot/` queda fuera de la clausura, porque es el piloto. Allí van el materializador y el archivo nuevo del plan.

A1 y A2 excluyen la clausura de tres maneras:
- queda fuera de su lista blanca exacta, que Scope hace cumplir;
- tienen una parada propia;
- su `ForbiddenWriteScope` nombra sus raíces: `src/RackCad.UI/` completo, `RackPushBackCommands.cs` y las carpetas del Plugin.

Los archivos de `Systems/PushBack/` fuera de `Pilot/` no se enumeran uno a uno. Son 52, y el riesgo de copia del Controller crece con la longitud de las listas (F4-E25).

### 3.2 Reasignaciones declaradas frente al paquete de F4

**La importación de biblioteca pasa a T4B**, fuera de PREPARE, por la orden del Owner.
- Conserva lo que congela D-13: el residuo OM-5 queda fuera de la transacción authored, y O-04 va antes de importar.
- **Declarado:** un Conflicto que solo detecte MUTATE deja residuo, como admite D-13.
- La agrupación de Undo de la importación se mide en T4C y en Smoke-2.

**A T4B o T4C van** INV-30, la parte de escritura de INV-10, INV-39, C-F4-05, las guardas de fachadas y la inyección Debug.

## 4. Decisiones de F4-T4A1-PREPARE-CORE

### D-T4A1-01. `ConfirmBase` («Comprobar ahora», D-12)

`DraftSession` gana `public DraftConfirmResult ConfirmBase(DraftStaleBase reread)` en `DraftSession.cs`.
- Los tipos nuevos van en el mismo archivo, como `DraftCheckStatus`: `DraftConfirmStatus` { NotApplicable, Confirmed, Conflict } y `DraftConfirmResult(Status, DraftStatus, Differences)`.
- **No hay archivo nuevo en `Drafts/`.**

**Comportamiento:**

| Estado del borrador | Relectura | Resultado y efecto |
|---|---|---|
| No registrado, Applying, Unknown (cualquier origen) o Error | — | NotApplicable. **Nada cambia.** Unknown(CommitThrew) es de `CheckNow`, y Unknown(PostReread) solo sale por «Descartar y recargar»: comparar con la base anterior a la escritura daría un Conflicto falso |
| Clean o Dirty (sin estado explícito) | MATCH | Confirmed. **Solo limpia `IsPossiblyStale`** |
| Clean o Dirty | con diferencias | Conflict. Pone el estado explícito en Conflict y `ConflictDifferences` con las diferencias ordenadas. Es el literal de D-12, en lugar de la recarga perezosa de O-01 |
| Conflict | MATCH | Confirmed. **El borrador sigue en Conflict con todos sus datos**: solo sale con `DiscardAndReload` o con «Conservar» confirmado, así que INV-F4T1-11 no cambia. El resultado dice que la base vuelve a coincidir |
| Conflict | con diferencias | Conflict. Sustituye `ConflictDifferences` por las nuevas y sigue en Conflict |

**En todos los casos:**
- no cambian la StaleBase, el `InitialDraftState`, el `CurrentDraftState`, el texto pendiente, `LastApplyCause` ni `Warnings`;
- **nunca se limpia Dirty**;
- es pura: no lee el dibujo ni toma la ranura;
- un `reread` nulo lanza `ArgumentNullException`, como en `CheckNow`.

**Pruebas:**
- `AfterCheckNowFoundAConflictTheDraftOnlyLeavesThroughTheReconciliationActions` (`WorkspaceDraftApplyTests.cs:701-716`) sigue en verde sin cambios.
- Una prueba nueva fija que, en Conflict, `ConfirmBase` con MATCH no saca de Conflict.

### D-T4A1-02. «Conservar mi borrador» atómico para el piloto

`DraftSession` gana una sobrecarga aditiva:

`public DraftReloadResult KeepMineOnCurrentVersion(DraftIntentRead<TState> read, Func<TState, TState, TState> keepOn, bool confirmed)`

**Orden de ejecución:**
1. Si `keepOn` es nulo, lanza `ArgumentNullException` antes de cualquier otra cosa. `Func` es de `System`, como el copiador del constructor.
2. Hace las mismas comprobaciones, en el mismo orden, que la versión de dos parámetros: registrado y en Dirty o Conflict; después `confirmed`; después `read.IsAdmitted`.
   - Responde NotAllowed, NotConfirmed y NotAdmitted.
   - **Sin admisión no se llama a `keepOn`.**
3. Antes de asignar nada, copia el inicial nuevo y calcula el actual sustituto: `_copy(keepOn(_copy(read.InitialState!), _current))`.
   - La base no se copia, porque es inmutable.
   - Si una copia o `keepOn` lanzan, el borrador no cambia.
4. Llama a `Rebase(read, initial)` y sustituye el actual. El texto pendiente se conserva.

**Relación con INV-F4T1-04:**
- La versión de dos parámetros conserva el `CurrentDraftState`. No cambia, y sus pruebas siguen.
- La sobrecarga lo **sustituye** por el resultado de aplicar las ediciones del piloto sobre la versión actual. Es IC-07 punto 2 (`I-64-implementation-clarifications.md:125-127`), y es una **ampliación declarada** de INV-F4T1-04, no un cambio.

**El host la llama así:** `draft.KeepMineOnCurrentVersion(pilot.Read, PushBackPilotMaterializer.KeepPilotEditsOn, confirmed: true)`. No hay estado intermedio observable, y el host no nombra `.InitialState`.

**El arrastre (2) de F4-E25:** el escenario (b) y «Conservar» se separan en variantes distintas, con pruebas en `WorkspaceDraftPilotMaterializationTests.cs`.

### D-T4A1-03. Las operaciones nuevas del host y del puerto

**Las operaciones:**

| Puerto | Host (público, con implementación explícita como en H-07) | Devuelve |
|---|---|---|
| `TryConfirmBase(EditIntentRequest)` | `TryConfirmPilotBase` | `EditConfirmOutcome` (nuevo) |
| `TryKeepMine(EditIntentRequest)` | `TryKeepPilotEdit` | `EditIntentOutcome` |
| `TryDiscardAndReload(EditIntentRequest)` | `TryDiscardAndReloadPilotEdit` | `EditIntentOutcome` |

**Los cuerpos del host** siguen los pasos 1-8 de `TryCheckPilotEdit` y `TryReloadPilotEdit`:

1. Los errores de la petición, con los mismos textos.
2. **La compuerta `PassGate`:**
   - sin lector: Unavailable;
   - sin documento, con un modal o con un comando en curso: Waiting;
   - otro documento: Rejected;
   - dentro de un aviso: Waiting;
   - con `_pilotReadInProgress`: Waiting.
3. NotFound si no hay borrador.
4. **La aplicabilidad, antes de leer:**
   - `TryConfirmPilotBase`: Clean, Dirty o Conflict. En otro caso, NotApplicable con A1-01.
   - `TryKeepPilotEdit`: Conflict, o Dirty con `IsPossiblyStale`. En Applying, Waiting con el texto existente «El borrador se está aplicando o ya no está registrado.». En cualquier otro caso, NotNeeded con A1-02.
   - `TryDiscardAndReloadPilotEdit`: cualquier estado registrado salvo Applying. En Applying, Waiting con el texto existente.
5. La marca de lectura, con `try`/`finally`.
6. **Una** lectura de la intención por `_editIntentReader` (H-08), con los mismos mapeos de estado de lectura.
7. **El consumo:**
   - `draft.ConfirmBase(pilot.StaleBase)`.
     - Confirmed da `EditConfirmOutcome.Confirmed`. Si la lectura no está admitida, lleva **su diagnóstico de admisión** (M-5).
     - Conflict da `EditConfirmOutcome.Conflict(DescribeDifferences(...))`.
   - La sobrecarga de D-T4A1-02, con este mapeo:
     - Reloaded da `EditIntentOutcome.Kept(witness)`, que es nuevo;
     - NotAdmitted da NotAdmitted, como `TryReloadPilotEdit`;
     - NotAllowed da Waiting;
     - NotConfirmed y el resto dan Failed.
   - `draft.DiscardAndReload(pilot.Read)`, con el mismo mapeo que `TryReloadPilotEdit`, que **reutiliza `EditIntentStatus.Reloaded` y su fábrica**.
8. **`_knownClean.Remove(request.Session)`** en todo resultado que cambie el borrador: el Conflict de `TryConfirmPilotBase`, Kept y Reloaded.

Toda excepción se contiene como Failed, con `Report(` y sin degradar la sesión.

**Los tipos del puerto** van en `WorkspaceEditIntent.cs`, como `EditCheckOutcome`:
- `EditConfirmStatus` { Unavailable, Waiting, Rejected, NotFound, NotApplicable, Confirmed, Conflict, Failed };
- `EditConfirmOutcome`, un valor (`struct`) como los demás resultados, con `Status`, `Reason`, `Differences` (una cadena, como en `EditCheckOutcome.Conflict`) y `AdmissionDiagnostic`. Este último es null salvo en un Confirmed sin admisión. Su valor por defecto es Unavailable, y la prueba de los valores y los nombres exactos (`WorkspaceEditIntentHostTests.cs:1012-1022`) lo incluye.

**`EditIntentStatus` solo gana `Kept`**, en la posición 10, al final. Las tres guardas de prefijo de H-07 lo admiten.

**`PilotFieldView` gana tres parámetros opcionales** al final, después de `errorReason`, cada uno con su propiedad:
- `PilotUnknownOrigin unknownOrigin = PilotUnknownOrigin.None`, con un enumerado nuevo del puerto: { None, CommitThrew, PostReread }. Su nombre no es una palabra de `Drafts/` ni contiene una parte prohibida (H-10). Vale None salvo en Unknown.
- `string? conflictText = null`, con `DescribeDifferences(ConflictDifferences)`. Solo se rellena en Conflict.
- `string? lastCause = null`, con el `LastApplyCause` del borrador tal cual.

Es un **cambio declarado** de la guarda de `PilotFieldView` (H-07): sigue habiendo un solo constructor, `errorReason` sigue siendo opcional, y después solo van esos tres parámetros, en ese orden.

**Con esta vista, A2 decide el despacho:**
- Unknown(CommitThrew): «Comprobar ahora», por `TryCheckNow`.
- Unknown(PostReread): «Recargar», por `TryDiscardAndReload`.
- Clean, Dirty y Conflict: «Comprobar ahora», por `TryConfirmBase`.

**Los reenvíos:**
- la compuerta (`WorkspacePilotSupport.cs`) reenvía las tres operaciones y responde Waiting dentro de un aviso;
- el adaptador del controlador solo reenvía.

**Los tres dobles** (H-09) siguen el patrón de cada uno, sin cambiar ninguna aserción existente:
- `CommandAdvisorPortDouble`: `Refuse<T>(nombre, request.Session)`, que registra y lanza;
- `ForwardingRiskPort`: el reenvío y el registro de `_calls` de sus operaciones;
- el doble de la UI: un respondedor opcional por operación, inerte por defecto.

**Los textos:**
- Se reutilizan las constantes (`NoDraftReason`, `NotThisDocumentReason` y demás) y los literales existentes de H-07.
- La tabla de `LiteralViolations` pasa de **28 a 30 filas** con A1-01 y A1-02. Su original de referencia en inglés no puede aparecer.
- El `Times` de cada fila reutilizada pasa a la cuenta exacta nueva, y el handoff declara el valor de antes y el de después.
- Las cuentas de textos distintos y el nombre de la prueba pasan de 28 a 30.
- Ningún texto lleva una subcadena de G-04.

**Tabla T-A1 (cerrada):**

| Id | Original de referencia | Texto |
|---|---|---|
| A1-01 | `The draft cannot be checked in its current status.` | `El borrador no admite comprobación en su estado actual.` |
| A1-02 | `Only a draft in conflict or with edits marked as possibly stale is kept.` | `Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.` |

### D-T4A1-04. El PREPARE puro y el plan inmutable

**Archivo nuevo:** `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs`, con `#nullable enable` como sus vecinos.

**La entrada es una instantánea explícita**, `PushBackPilotPrepareInput`:
- Lleva `StaleBase`, `InitialDraftState` (copia), `CurrentDraftState` (copia), `HasPendingText`, `IsDirty` y `ConsideredStatus`.
- **`ConsideredStatus`** es `Status`, el estado efectivo, fuera de Applying. Bajo Applying es `StatusBeforeAttempt` (N-7).
- La construye **una sola fábrica** del mismo archivo, `PushBackPilotPrepareInput.From(DraftSession<PushBackPilotDraftState> draft)`. Así el host no nombra `.StaleBase`. `Systems/PushBack/Pilot` ya nombra `Drafts`.
- **A1 no añade ninguna operación del host para el plan.** El llamador es de T4B.

**`PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput input, PushBackPilotIntentResult reread, RackCatalog catalog)`** es puro y total. Cualquier excepción interna da Invalid con su motivo. Los resultados se evalúan en este orden:

1. **NotApplicable:** `ConsideredStatus` no es Clean ni Dirty.
2. **PendingText.**
3. **NothingToApply:** `ConsideredStatus` es Clean. **No mira la relectura.**
4. **Conflict,** con las diferencias: `input.StaleBase.CompareWith(reread.StaleBase)` no es MATCH. Va **antes de la admisión** (INV-06; `WorkspaceDraftPilotIntentStaleBaseTests.cs:551-615`).
5. **NotAdmitted,** con el diagnóstico: hay MATCH, pero `reread.Read` no está admitida.
6. **Invalid,** con el motivo, en estos casos:
   - **(a)** el estado actual no se compone, por los arrastres de D-T4A1-05;
   - **(b)** ningún miembro Mutable de la relectura tiene un `EnvelopeText` byte a byte igual a `initial.Origin`, o lo tienen varios;
   - **(c)** aborta `PreflightInnerSources` de Application. Se llama como en RACKEDITAR (H-13): sobre los diseños interiores de los sobres releídos de los miembros Mutable, en orden ordinal de clave, con `RackSystemKind.PushBack` y con el proyecto interior del sobre de origen como iniciador;
   - **(d)** el builder de Application no construye el sistema del diseño saliente (D-13, paso 1). El error es tipado y no hay escritura;
   - **(e)** algún miembro Mutable **no está disponible** en el sistema saliente según `RackViewAvailability.Evaluate`, con su sobre releído decodificado por `RackViewCodec.Decode` de Application (`RackViewCodec.cs:115`); un sobre que no se decodifica también da Invalid. Los `PushBackViewAvailabilityFacts` se construyen con los postes de `PushBackSystemLateralBuilder.Cortes(system, catalog)` y con `system.IsComposite` (H-12), y el motivo remite a RACKEDITAR.
     - **El piloto nunca borra una vista** (N-4): un fantasma que ya estuviera en el dibujo no se toca, y el plan solo redibuja vistas existentes.
7. **Ready,** con el plan.

**El plan, `PushBackPilotUpdatePlan`,** es inmutable y solo guarda cadenas y valores:
- `RackId` y la clave del origen, que es la del miembro de 6 (b).
- `ExpectedBase`: las entradas de `input.StaleBase` en orden ordinal de clave. T4B las compara dentro de MUTATE con la relectura caller-owned.
- El `AnnotationScale` nuevo.
- **Por cada miembro Mutable, en orden ordinal de clave:**
  - la clave;
  - la vista y la sección salientes, decodificadas del sobre releído como en RACKEDITAR (`RackPushBackCommands.cs:274-345`): Planta con `-1`, Frontal con la `Section` del sobre y Lateral con el índice de poste decodificado;
  - **el texto serializado del diseño saliente de esa vista:** `RackProjectStore.Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(resuelto))`. El `design` sale de `ComposeForWrite` y luego de `PushBackCompositeEditorAssembler.BuildDesign`; `resuelto` es el proyecto interior de esa vista, resuelto en 6 (c);
  - **el texto serializado de su sobre saliente** (INV-10): `RackEmbedStore.Serialize(RackEmbedComposer.Compose(source, RackEmbedDocument.KindPushBack, source.Id, source.Name, view, section, designJson))`, sobre el sobre **releído** de esa vista (M-4). `Name` nunca sale del borrador.
- **La expectativa de POST:** la escala y los textos de los sobres salientes por clave. T4B los compara con la relectura de POST antes de dar Limpio (§7).
- **La igualdad es explícita:** las cadenas se comparan de forma ordinal, las colecciones elemento a elemento y en orden, y la escala con `Equals`.

**La única llamada a `Compose`** está en un miembro propio del archivo del plan con un parámetro `RackEmbedDocument source`, sin `?` (`CustomPropertiesEnvelopeGuardTests.cs:85-107`). Por ejemplo:

`private static string ComposeMemberEnvelope(RackEmbedDocument source, string view, int section, string designJson)`

Su entrada en el censo exacto de I-54 es un **cambio declarado** de `ExpectedComposeCensus` (H-11):
- una entrada nueva, `ComposeCall("src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs", "<miembro>", "source")`, en su posición ordinal por ruta, que es la primera;
- clasificada como «rack existente que compone con su sobre real (el releído)», según D-21 de I-54;
- y una línea en el comentario de 186-190, con el precedente de I-55 G16.

**`Prepare` no cambia el borrador:** no lo pone en Conflict, no limpia Dirty y no hace commit.

**Correspondencia con `CompleteApply`, para T4B:**

| Resultado de `Prepare` | En T4B |
|---|---|
| NotApplicable o PendingText | Sin `TryBeginApply`: el núcleo los rechaza (`DraftSession.cs:471-473`). Con PendingText, antes se cruza la frontera Update |
| NothingToApply | T4B decide que está Clean **antes** de `TryBeginApply` y no toma la ranura. Bajo Applying no puede darse, porque un Clean no se edita mientras se aplica; si llegara, NotAppliedPreflight |
| Conflict | NotAppliedConflict |
| NotAdmitted | NotAppliedAdmission |
| Invalid | NotAppliedPreflight |

**Fuera del plan, para T4B y T4C,** porque necesitan el dibujo: las capas bloqueadas, las piezas faltantes, la importación y los supervivientes.

### D-T4A1-05. Los arrastres de `ComposeForWrite` (F4-E25)

En `PushBackPilotMaterializer.cs`, con el literal de F4-E25:
- **(1)** «el rechazo de `ComposeForWrite` debe comparar `current` sin normalizar contra una copia de `initial` con la escala neutralizada».
- **(3)** «comprobar que la base rematerializada coincide con `initial` antes de componer, por si cambia el catálogo». La comparación se hace con el comparador del piloto (`PushBackPilotStateComparer`).
- **(6)** «rechazar una escala no válida (NaN, 0 o negativa) que haya llegado por edición en sitio», con una **ampliación declarada**: también se rechaza la escala infinita. Es coherente con la validación del campo, «número finito mayor que cero» (`WorkspaceHostHardeningGuardTests.cs:426`).

(2) está en D-T4A1-02 y (5) en A2. Las pruebas existentes de `WorkspaceDraftPilotMaterializationTests.cs` siguen en verde.

### D-T4A1-06. R-3 (cambio declarado del contrato de T4)

- Una **guarda por cuerpo** sobre `ViewOfPilotDraft` (`WorkspaceHostCore.cs:861-911`), con su autoprueba en negativo: `errorReason` solo se pasa cuando el estado es `PilotFieldState.Error`.
- Un invariante del núcleo, también por guarda: `ErrorReason` solo se asigna junto con `_explicitStatus = DraftStatus.Error`, o a null.

### D-T4A1-07. Las guardas extendidas (obligación)

Se extienden en positivo a las tres operaciones nuevas, cada una con su autoprueba en negativo:

| Guarda | Extensión |
|---|---|
| `WorkspacePilotEditGuardTests.cs` | `Operations` y `ReadOperations`; el lector solo en las lecturas; la tabla de la marca de lectura; la compuerta (Waiting dentro de un aviso); los reenvíos literales del controlador; `WriteViolations` por cuerpo, que es **el oráculo de cero escrituras de PREPARE**; y los nombres de tipo nuevos del puerto (`EditConfirmOutcome`, `EditConfirmStatus` y `PilotUnknownOrigin`), dentro de sus reglas |
| `WorkspaceLifecycleGuardTests.cs` | La marca «conocido limpio» (438-511): los tres cuerpos del host entran en la lista de operaciones que cambian un borrador; `ChangesADraft` reconoce `.KeepMineOnCurrentVersion(` y `.ConfirmBase(`; y la cota inferior de `_knownClean.Remove(` sube con las nuevas llamadas |
| `WorkspaceEditIntentGuardTests.cs` | La contención como Failed (575-589) recorre los tres cuerpos nuevos; los nombres prohibidos en el drenaje y en el Idle (592-610); y `AllowedConsumers` (635-663) admite `.ConfirmBase(` y `.KeepMineOnCurrentVersion(`, con un recuento por operación como los existentes |
| `WorkspaceEditIntentHostTests.cs` | Los portadores sin tipos de Drafts ni de Systems (969-986), con `EditConfirmOutcome` |
| `WorkspaceHostHardeningGuardTests.cs` | `PilotFieldView` (D-T4A1-03); la tabla de literales, de 28 a 30; y R-3 (D-T4A1-06) |
| `CustomPropertiesEnvelopeGuardTests.cs` | El censo (D-T4A1-04) |

**El comentario del puerto** (14-24) pasa a decir que **seis** operaciones leen el dibujo.

### D-T4A1-08. C2-M-8

Como A1 toca `WorkspaceCommandAdvisorTests.cs` por sus dos dobles, la teoría de tres casos idénticos pasa a ser un `Fact` (C2-M-8). Intro y Esc quedan para Smoke-2.

## 5. Contrato propuesto de F4-T4A1 (a fijar en la emisión)

**`AllowedWriteScope`** (lista blanca exacta, medida en la emisión):
- **Application:**
  - `Workspace/Drafts/DraftSession.cs`;
  - `Systems/PushBack/Pilot/PushBackPilotMaterializer.cs`;
  - `Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs` (nuevo);
  - `Workspace/WorkspaceHostCore.cs`, `Workspace/IWorkspacePilotEditPort.cs`, `Workspace/WorkspacePilotSupport.cs` y `Workspace/WorkspaceEditIntent.cs`.
- **Plugin:** `Plugin/Workspace/WorkspaceController.cs`, solo para los reenvíos.
- **UI.Tests:** `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs`.
- **Tests:**
  - `tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs`;
  - en `tests/RackCad.Tests/Workspace/`:
    - `WorkspaceCommandAdvisorTests.cs`;
    - `WorkspacePilotPrepareTests.cs` (nuevo);
    - `WorkspacePilotPrepareGuardTests.cs` (nuevo);
    - `WorkspaceDraftPilotMaterializationTests.cs`;
    - `WorkspacePilotEditGuardTests.cs`;
    - `WorkspaceLifecycleGuardTests.cs`;
    - `WorkspaceEditIntentGuardTests.cs`;
    - `WorkspaceEditIntentHostTests.cs`;
    - `WorkspaceHostHardeningGuardTests.cs`.

**`ForbiddenWriteScope`** (106 entradas, por raíces, como en F4-T3C2):
- `docs/` y `.github/`, de forma explícita;
- la clausura de T1B por sus raíces (§3.1);
- las carpetas de Application salvo `Workspace/` y `Systems/`, y las de `Systems/` salvo `PushBack/`;
- los seis archivos del piloto que no cambian, entre ellos el lector `PushBackPilotIntentReader.cs`;
- los cuatro archivos de `Drafts/` que no cambian;
- los archivos fijos de `Workspace/`, entre ellos `IWorkspaceEditIntentReader.cs` y `EventBridgeCore.cs`;
- `src/RackCad.UI/` completo;
- las carpetas del Plugin, que contienen `RackSiblingScan.cs`, el adaptador de la intención y la costura de I-55;
- `RackPushBackCommands.cs` y `RackCommandSupport.cs`;
- los archivos de `Plugin/Workspace/` salvo el controlador;
- las guardas fuera del alcance;
- las raíces de las pruebas.

Lo que no se enumera lo cubre la lista blanca exacta, más las paradas F4T4A1-C01 y F4T4A1-C04.

**Invariantes:**
- **cero escrituras authored**, con la guarda por cuerpo de D-T4A1-07;
- PREPARE no cambia el borrador;
- `ConfirmBase` nunca limpia Dirty y nunca saca de Conflict (INV-F4T1-11 intacto);
- la separación entre la StaleBase, el `CurrentDraftState` y la relectura;
- la comparación cruda de D-12, con **Conflict antes que la admisión**;
- el plan inmutable, con los textos serializados y la expectativa de POST;
- **el piloto nunca borra una vista**;
- `ComposeForWrite` solo tras un MATCH, con los arrastres;
- «Conservar» atómico, con la admisión antes de `keepOn`;
- `Kept` como único estado nuevo de `EditIntentStatus`;
- el censo de `Compose`, con una sola entrada nueva;
- R-3 por guarda;
- las guardas extendidas;
- el RED inerte;
- los límites.

**`RequiredTests`** (las líneas base son las de la CI del GREEN de F4-T3C2, `4e34bb56`):
- `RackCad.Tests.Workspace`: `MinSelected` 2668, `ExpectRed` true.
- `RackCad.UI.Tests.Workspace`: `MinSelected` 458, `ExpectRed` false, porque solo cambia el doble.

**`StopConditions`:**
- una escritura authored;
- tocar la clausura de T1B, el lector o la costura;
- que el builder o la disponibilidad necesiten el Plugin;
- otra llamada a `Compose`, o un cambio del censo distinto del declarado;
- un cambio de INV-F4T1-11;
- la replanificación reforzada única;
- las de F4-T3C2.

**`attempts`:** 1 de 3.

## 6. F4-T4A2-PREPARE-UI: reglas vinculantes

Su paquete se fija tras cerrar A1.

| Regla | Contenido |
|---|---|
| R-A2-01 | «Comprobar ahora» en el VM del campo, con el despacho de D-T4A1-03. El navegador no nombra `TryCheckNow`, `TryConfirmBase`, `TryKeepMine` ni `TryDiscardAndReload`, como hoy fija `WorkspacePilotCloseSyncGuardTests.cs:849` |
| R-A2-02 | El Conflicto muestra la causa (`lastCause`) y qué cambió (`conflictText`). En Conflict con MATCH dice que la base vuelve a coincidir. Ofrece «Descartar y recargar» y «Conservar mi borrador», **los dos con confirmación explícita**, porque descartar un Dirty pierde ediciones (D-14) |
| R-A2-03 | Los avisos de INV-31 y de la tabla por frontera de D-13 |
| R-A2-04 | C2-M-1 y C2-M-2: las guardas del Sí/No y de `Authorize` detectan una implementación invertida. Es una corrección solo de guarda, elegida así, y sus `RequiredTests` incluyen Core |
| R-A2-05 | El arrastre (5) de F4-E25, en `RackCad.UI.Tests` |
| R-A2-06 | Su `ForbiddenWriteScope` incluye la clausura de T1B (§3.1). Si A2 necesitara `Themes/` o `Controls/` de la clausura, es STOP: T1B iría antes |

## 7. F4-T4B-MUTATE-POST y F4-T4C-HOST-UNDO: reglas vinculantes

**R-T4-01 (B). La lectura caller-owned** (INV-39, decisión 4 del Owner):
- una variante aditiva de `CaptureDrawing`, sin bloqueo, transacción ni `Commit` propios, con su guarda;
- **equivalencia** (F4-E26): para el mismo dibujo, los tres caminos de lectura dan la misma `DraftStaleBase`, con la misma `BuildStaleBase`. Los tres caminos son el lector de «Comprobar ahora», la vía de O-04 en contexto de comando (R-T4-06) y la lectura caller-owned.

**R-T4-02 (B). MUTATE:** una transacción poseída por el paso de mutación.
- Dentro de ella van la relectura caller-owned, la comparación con `plan.ExpectedBase` (la autoridad de D-12) y la comprobación de destino.
- Después vienen las unidades con los sobres del plan (INV-10), y `Commit()` en su propio `try`, con la inyección Debug de INV-30.
- **Todo o nada**, y `TopTransaction == null` al salir.
- `MutationDestinationBinding` es un patrón de Push Back.

**R-T4-03 (B). La importación de biblioteca** va antes de la transacción authored (§3.2). Se acepta `LegacyDeltas` (decisión 3 del Owner).

**R-T4-04 (B). POST** sale de una relectura: `SyncName`, la purga, un `Regen` y la relectura, que **se compara byte a byte con la expectativa de POST del plan**.
- **Si coincide:** `CompleteApply(Applied(postRead))`, `CurrentDraftState := InitialDraftState` y el texto pendiente vacío. **Dirty pasa a false solo desde el estado posterior verificado.**
- **Si no coincide, o si la relectura falla:** `AppliedPostRereadFailed`, que deja Unknown(PostReread), con los avisos de N-5.
- Si T4B mide que la vía de escritura no conserva los bytes, es STOP a la sesión: la comparación no se debilita.
- `CompleteApply` tiene que ser robusto ante un copiador que lanza (F4-E25 (4)).
- C-F4-05, con `eraseByKind` nulo.

**R-T4-05 (C). Contexto de comando** (C-F4-02, ratificado):
- `ExecuteInCommandContextAsync` solo en el archivo exacto del adaptador, nuevo en `Plugin/Views`;
- las carpetas Workspace siguen sin `async`, `await` ni `.Result`;
- **el primer paso de T4C es medir en host** si el callback corre antes de que vuelva la llamada y si un Actualizar da un solo paso de Undo. Si no es así, va una A-n al Owner (D-13).

**R-T4-06 (C). Un único `using LockDocument`** alrededor de PREPARE, MUTATE y POST (D-13, paso 2).
- O-04 se hace dentro, en contexto de comando, con una vía de lectura sin la puerta de reposo y con la revalidación de la sesión.
- **La ranura y el cerrojo de T1** (INV-F4T1-20), que son oráculo de T4B y T4C:
  - NothingToApply se decide antes de tomar la ranura;
  - `TryBeginApply` va antes de PREPARE, y `CompleteApply` en `finally`, exactamente una vez por salida;
  - un segundo Actualizar recibe `AnotherUpdateInProgress` (`DraftSession.cs:474`);
  - Error y Descartar no liberan la ranura antes de tiempo.

**R-T4-07 (C). La marca de host «actualización en curso»:** `DiscardRack`, `DiscardDocument` y `DiscardByUser` no actúan sobre un Applying. Actualizar llama a `NotePilotEdit` (F4-E35).

**R-T4-08 (C). El ámbito de mutación propia** (C-F4-04):
- por sesión y por miembros;
- activo solo en la ventana de mutación y liberado en toda salida;
- **no oculta otro rack**;
- se fija una de las dos vías de C-F4-04, con una IC o con una A-n.

**R-T4-09 (C). La UI de Actualizar:**
- el botón, la frontera `PilotBoundaryKind.Update` y la presentación de Applying y de los avisos;
- la selección implícita que cancela `EXECUTEFUNCTION`;
- K = 1 tras POST.

**R-T4-10 (C). Undo:** un solo paso lógico. Undo restaura el estado previo y Redo el posterior. **Solo lo prueba Smoke-2.**

**R-T4-11 (B y C). Las autoridades integradas:**
- `RackSiblingMembership` e I-55, y las de identidad y vistas;
- `NavigationTargets` solo para navegar;
- **solo se aplican los cambios que posee el piloto**;
- nunca se borra una vista (D-T4A1-04, 6 (e)).

**R-T4-12 (B). T1B va antes de T4B** (§3.1).

**R-T4-13 (B y C).** C2-M-3 a C2-M-7, donde se toquen esas superficies.

## 8. Smoke-2

Es obligatorio, con los 30 puntos de la orden y con dibujos reales representativos, entre ellos uno antiguo para `LegacyDeltas`. El procedimiento se fija tras T4C.

## 9. Nota de custodia de F4-T3C2 (orden del Owner)

Va en el commit de emisión de T4A1, que es el siguiente commit útil de documentación, dentro de F4-E51. **No reescribe F4-E50**, y solo dice esto:
- el script auxiliar del Worker que contenía una ruta local se excluyó de la custodia pública;
- solo se conservó su SHA-256;
- las rutas sintéticas de las pruebas se redactaron según las reglas de custodia;
- no se publicó ninguna ruta personal ni local.

## 10. Disposición de las revisiones

### 10.1 La revisión adversarial de la v1 (EMITIBLE TRAS CORRECCIONES, 1/12/14)

| Hallazgo | Disposición |
|---|---|
| B-1 | **Aceptado.** La instantánea con `ConsideredStatus`, la tabla hacia `CompleteApply` y el llamador de T4B (D-T4A1-04). Se completa con N-7 y N-8 |
| I-1 | **Aceptado.** Primero Conflict; NotAdmitted solo con MATCH; NothingToApply antes de leer |
| I-2 | **Aceptado.** El plan lleva los diseños y los sobres salientes por vista, la validación del sistema, la disponibilidad y la expectativa de POST. Se completa con N-1, N-4 y N-9 |
| I-3 | **Aceptado.** La sobrecarga atómica (D-T4A1-02), completada con N-13 |
| I-4 | **Aceptado.** La guarda por cuerpo y el invariante del núcleo (D-T4A1-06) |
| I-5 | **Aceptado.** `unknownOrigin`, `conflictText` y `lastCause`, con su tipo (N-6), el despacho fijado y el cambio declarado de su guarda |
| I-6 | **Aceptado.** `WorkspaceCommandAdvisorTests.cs` entra en el alcance, con C2-M-8 (N-10) |
| I-7 | **Aceptado.** La tabla de D-T4A1-07, completada con N-3 |
| I-8 | **Aceptado.** H-08, R-T4-06 y R-T4-01, que incluye el tercer camino de lectura |
| I-9 | **Decidido por el Owner (decisión 4)** |
| I-10 | **Decidido por el Owner (decisión 2)** |
| I-11 | **Decidido por el Owner (decisión 1)**, con el efecto de presupuesto registrado en F4-E51 |
| I-12 | **Aceptado.** (2) en A1 y (5) en A2 |
| M-1 | **Aceptado.** Los literales de F4-E25 (N-12) |
| M-2 | **Aceptado.** La igualdad explícita sobre cadenas y valores (N-9) |
| M-3 | **Aceptado.** Invalid solo si ningún miembro Mutable tiene el texto del origen, o si lo tienen varios (N-9) |
| M-4 | **Aceptado.** Los sobres salen de la relectura |
| M-5 | **Aceptado, con la opción (a) de N-2:** Conflict con MATCH sigue en Conflict |
| M-6 | **Aceptado.** Solo `Kept` es nuevo (N-5); la aplicabilidad se comprueba antes de leer; `_knownClean`; Waiting |
| M-7 | **Aceptado.** Las constantes y los literales reutilizados, con las cuentas declaradas (N-11) |
| M-8 | **Aceptado.** R-A2-02 y el despacho |
| M-9 | **Aceptado.** §3.1, con la clausura por referencia (N-14) |
| M-10 | **Aceptado.** §3.2 |
| M-11 | **Decidido por el Owner (decisión 3)** |
| M-12 | **Aceptado.** R-T4-02, R-T4-04, R-T4-07 y R-T4-09 |
| M-13 | **Aceptado.** R-A2-04 |
| M-14 | **Aceptado.** §9, con los cuatro puntos literales (N-15) |

### 10.2 La re-revisión de la v2 (contrato de T4A1 EMITIBLE TRAS CORRECCIONES, 1/3/11)

| Hallazgo | Disposición |
|---|---|
| N-1 (B, censo de `Compose`) | **Aceptado.** `CustomPropertiesEnvelopeGuardTests.cs` entra en el alcance, con una entrada declarada y un miembro con el parámetro `RackEmbedDocument source` (D-T4A1-04). La sesión lo comprobó en el código (H-11) |
| N-2 (I, INV-F4T1-11) | **Aceptado, opción (a).** Conflict con MATCH responde Confirmed sin salir de Conflict, y los tres detalles quedan fijados en la tabla de D-T4A1-01. INV-F4T1-11 no cambia |
| N-3 (I, guardas omitidas) | **Aceptado.** `WorkspaceLifecycleGuardTests.cs` entra en el alcance; la marca y la contención están en D-T4A1-07 |
| N-4 (I, Redraw/Erase) | **Aceptado.** La disponibilidad se evalúa sobre el sistema saliente con `PushBackViewAvailabilityFacts`, y un miembro no disponible da Invalid: el piloto nunca borra (6 (e) y R-T4-11). No va al Owner, porque el piloto no gana ninguna escritura |
| N-5 | **Aceptado.** Se reutiliza `Reloaded` y solo `Kept` es nuevo, con las guardas de prefijo citadas |
| N-6 | **Aceptado.** `PilotUnknownOrigin`, `string? conflictText` y `string? lastCause` |
| N-7 | **Aceptado.** `ConsideredStatus` queda definido, con la fábrica `From`, y A1 no añade ninguna operación del host para el plan |
| N-8 | **Aceptado.** NothingToApply se decide antes de `TryBeginApply` |
| N-9 | **Aceptado.** El plan guarda textos por vista, incluye el aborto de `PreflightInnerSources`, la `view` sale del sobre, y la regla de M-3 queda precisada |
| N-10 | **Aceptado.** Los tres dobles, con el patrón de cada uno |
| N-11 | **Aceptado.** La tabla de literales pasa de 28 a 30, con las cuentas declaradas |
| N-12 | **Aceptado.** H-03 y H-13 corregidos; los literales de F4-E25; «infinita» declarada como ampliación |
| N-13 | **Aceptado.** Primero la admisión y después `keepOn`, con la relación con INV-F4T1-04 e IC-07 declarada |
| N-14 | **Aceptado.** La clausura por referencia, R-A2-01 ampliada y R-A2-06 |
| N-15 | **Aceptado.** INV-F4T1-20 y `AnotherUpdateInProgress` en R-T4-06; los tres lectores en R-T4-01; los cuatro puntos en §9 |
