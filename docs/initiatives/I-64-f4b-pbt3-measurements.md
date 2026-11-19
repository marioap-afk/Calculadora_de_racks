> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`, `<worktree-de-I-64>`). El resto no cambia.

# I-64 F4B PB-T3 — Registro de mediciones del paquete (v3)

**Qué es:** las mediciones en las que se apoya `I-64-f4b-pbt3-gate-contract-package-v3.md`. Cada hecho lleva su ancla (archivo y línea en `2f5d3dec`, salvo donde se dice otra cosa) y el registro que lo mide (R-n).

**La remedición de la emisión de PB-T3a** (precondición 2 del paquete v4.4): **R-19**, sobre el cierre de PB-T2c (`f273eae1`, F4B-E40). No cambia nada frente a lo previsto.

**Lo que cambia en la v3** (la recomprobación de la v2, N-05 y N-07; V3-02, V3-05 y V3-07):
- R-0 y R-17, medidos otra vez en `833ea6bf`, la HEAD de la rama: PB-T1b CERRADA, PB-T1c EMITIDA, F4B-E24 y F4B-E25 en git, y `src` igual que en `2f5d3dec`;
- R-11 y R-12, al día (la medición pasa a INV-PBT3B-15, V3-08);
- R-15, con la cuenta de la variante (A′): 77;
- y un registro nuevo, **R-18:** las declaraciones de PP, UR y WR en `833ea6bf`, en las que se apoyan PBT3B-C13 y PBT3C-C13.

Las anclas de `src/` de R-1 a R-16 valen igual en `833ea6bf`, porque `git diff 2f5d3dec 833ea6bf -- src` es vacío.

**Lo que cambió en la v2:**
- R-0, con la corrección de PB-T1b ya en git;
- R-6, corregido con la revisión de la v1: S-6 queda vacía, y la variante (A′);
- R-11 y R-12;
- y cinco registros nuevos:
  - R-13: el origen borrado (B-01);
  - R-14: la barrera del piloto;
  - R-15: `tests/RackCad.Tests/Workspace/`;
  - R-16: los ayudantes de I-58;
  - R-17: las decisiones de F4B y la Snapshot J.

Todo se volvió a leer solo por objetos git, desde el checkout principal.

**Base:** rama `architecture/workspace-persistente-rackcad` en `2f5d3dec`, la custodia del intento 1 de PB-T1b revisado, con F4B-E21 y F4B-E22. `main` no se mira.

**Método:**
- solo objetos git desde el checkout principal, que comparte el almacén: `git show 2f5d3dec:<ruta>`, `git grep … 2f5d3dec -- <rutas>`, `git ls-tree`, `git log`, `git diff`;
- el worktree de la rama no se leyó, no se listó y su ruta no se escribió en ninguna orden;
- sin compilar, sin ejecutar pruebas, sin órdenes git que escriban y sin AutoCAD;
- tres scripts de solo lectura, escritos con la herramienta de archivos, que solo llaman a `git ls-tree` y `git show`: `census_pins.py`, `census_methods.py` y `census_methods2.py`, con sus salidas `census-pins-out.txt`, `census-methods-out.txt` y `census-methods2-out.txt` (en `<carpeta-del-paquete>`).

**Alias:**
- **PP** = `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs`;
- **UR** = `…/Pilot/PushBackPilotUpdateRun.cs`; **UP** = `…/Pilot/IPushBackPilotUpdatePort.cs`;
- **PM** = `…/Pilot/PushBackPilotMaterializer.cs`; **IR** = `…/Pilot/PushBackPilotIntentReader.cs`; **SR** = `…/Pilot/PushBackPilotSystemRebuild.cs`;
- **WR** = `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`;
- **WHC** = `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`; **DS** = `src/RackCad.Application/Workspace/Drafts/DraftSession.cs`;
- **RPC** = `src/RackCad.Plugin/RackPushBackCommands.cs`; **RCS** = `src/RackCad.Plugin/RackCommandSupport.cs`;
- **PES** = `src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs`; **PESC** = `…/PushBackEditableStateComparer.cs`; **PESO** = `…/PushBackEditableStateOwnership.cs`; **GATE** = `…/PushBackEditorOutputGate.cs`;
- **RVA** = `src/RackCad.Application/Systems/Shared/RackViewAvailability.cs`;
- **BM** = `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/block-m/block-m-analysis.md`;
- **[P]** = `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1; **[PKT1]** = el paquete de PB-T1 (v2.7 al redactar la v1; v2.8 custodiada en `af46fcd5`, igual en `833ea6bf`).

---

## R-0. Identidad de la base

| Medición | Resultado |
|---|---|
| `git log --oneline -15 2f5d3dec` | `2f5d3dec` (intento 1 de PB-T1b revisado, F4B-E21 y F4B-E22) ← `a0a5f699` (GREEN de PB-T1b) ← `2d9febc6` (RED) ← `4b0498e3` (emisión v2.7) |
| `git diff a0a5f699 2f5d3dec -- src tests` | vacío: el producto es el GREEN de PB-T1b |
| `git show --stat 2f5d3dec` | 11 archivos, todos de `docs/` (decisiones, disposiciones, revisión y análisis de la corrección) |
| Estado de la unidad (`docs/automation/state/I-64.yml`) | `current_phase: F4B`, `attempts: 2`; siguiente: la corrección de PB-T1b en el intento 2 (F4B-E22) |
| CI del GREEN de PB-T1b (dato de `direct-review.md:23` del intento 1) | run `44405531606`, 4/4; Core 19 086; UI 2 437 (2 416 `Passed`, 21 `Skipped`) |
| La corrección de PB-T1b (`analysis.md` del intento 1, §2 y §4) | Solo pruebas; el GREEN restaura la misma implementación. La autoprueba de I01 es una línea de CT-14, no un método nuevo: las cifras de métodos no cambian |
| PB-T1c | No emitido. Solo cambia W y WC ([PKT1] D-PBT1-13); su GREEN previsto: Core + 2, UI + 2 ([PKT1] §6.3) |
| **La corrección de PB-T1b, ya en git** (v2) | `git log origin/architecture/workspace-persistente-rackcad`: `a18ba345` (GREEN, «la implementacion del intento 1 restaurada sin cambios de producto») ← `126fd65f` (RED) ← `2f5d3dec` |
| `git diff 2f5d3dec a18ba345 -- src` | **vacío:** el producto es el mismo |
| `git diff --stat 2f5d3dec a18ba345 -- tests` | cinco archivos de pruebas de PB-T1b (286 inserciones y 111 borrados) |
| Métodos `[Fact]`/`[Theory]` de esos cinco archivos, antes → después | `PushBackEditableStateCopyTests` 7 → 7, `PushBackEditableStateLoadTests` 5 → 5, `PushBackEditorOutputGateTests` 1 → 1, `PushBackSharedReferenceGuardTests` 2 → 2, `PushBackEditableStateParityTests` 1 → 1. Las cifras de la CI no cambian por la corrección |
| `PushBackSharedReferenceGuardTests.cs` en `a18ba345` | `Rule1b` (`:60`) se aplica también a los alias (`:167`: `Rule1b(member, aliases.Keys)`): es la corrección I01 que CT-PBT3A-13 copia |
| **La HEAD de la rama** (v3; `git log --format='%h %s' 2f5d3dec..833ea6bf`) | `833ea6bf` («PB-T1c: emite el contrato de gate (AuthorityRevision af46fcd5)») ← `af46fcd5` («paquete del contrato v2.8 y anclajes de PB-T1c (F4B-E27)») ← `95373af6` («PB-T1b: cadena, decisiones del Owner y cierre (F4B-E23 a F4B-E26)») ← `a18ba345` ← `126fd65f` ← `2f5d3dec` |
| `git diff 2f5d3dec 833ea6bf -- src` (v3) | **vacío** |
| `git diff --name-only 2f5d3dec 833ea6bf -- tests` (v3) | los mismos cinco archivos de pruebas de PB-T1b (286 inserciones y 111 borrados); ninguno de `Workspace/` |
| `git show --stat 833ea6bf` (v3) | cuatro archivos de `docs/`: `I-64-evidence.md`, `I-64.yml` y el `gate-contract.json` y `make_pbt1c_contract.py` de `F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/` |
| **PB-T1b, CERRADA** (v3; `f4b-dispositions.md:711` en `833ea6bf`, F4B-E26; [PKT1] v2.8, «Línea base de la CI» de §6.3) | CLOSED / PASS WITH LIMITATIONS sobre el GREEN `a18ba345`, attempts 2 de 3; custodia del cierre `95373af6`; CI `44438060317`, 4/4: Core 19 086; UI 2 437 (2 416 `Passed`, 21 `Skipped`) |
| **PB-T1c, EMITIDA** (v3; `f4b-dispositions.md:761`, F4B-E27; `I-64.yml:9` y `:57` en `833ea6bf`) | contrato en `F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json`, `AuthorityRevision` `af46fcd5`, attempts 1 de 3; siguiente: la planificación del Controller |
| `git diff --stat af46fcd5 833ea6bf -- docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` (v3) | vacío: la v2.8 custodiada es la misma; PBT1C-C13 está en `:1338` |

**Consecuencia:** la línea base de la CI de PB-T3 es la del cierre de PB-T1c. **Prevista, no medida:** Core 19 088; UI 2 439 (2 418 `Passed`, 21 `Skipped`). Es el GREEN previsto de PB-T1c en la v2.8 custodiada: Core 19 086 + 2 y UI 2 437 + 2.

## R-1. Lo que escribe hoy el piloto, punto por punto del [OD]

| Punto del [OD] | Hoy (medido) | Ancla |
|---|---|---|
| **Campos** | Solo la escala de anotación. `ComposeForWrite` neutraliza la escala en una copia del estado inicial y compara: cualquier otra diferencia es `NonPilotContentReason` («El borrador cambia contenido que el piloto no edita: no se compone.»). Después rematerializa desde `initial.Origin` y solo aplica la escala | PM:196-234 (comparación PM:212-215; rematerialización PM:217-224; escala PM:227) |
| | El adaptador de texto pendiente solo admite la clave `AnnotationScale` | `PushBackPilotFieldAdapter.cs:24`, `:37-40` |
| **Módulos y frentes** | No: el copiador del piloto no lleva la autoridad de módulos ni la cola (H-PB-27, [PKT1] HT1-20), y cualquier cambio de frentes es contenido «que el piloto no edita» | PM:212-215; `PushBackPilotStateComparer.cs:58-111` |
| **Lado B** | No: un diseño compuesto es `OutOfPilot` («Rack de dos lados (A/B): fuera del piloto.») y una cola aparcada también («El diseño aparca una cola compuesta: fuera del piloto.») | PM:106, :108, :148, :169-170; regla 12 del lector IR:225-229 |
| **Renombrar** | No: el nombre del sobre saliente es el del sobre **releído** de cada vista (`source.Name`). Los nombres de las definiciones se calculan desde ese nombre | PP:436; nombres deseados UR:190-223 (base `embed.Name.Trim()`, UR:202) |
| **Borrar vistas obsoletas** | No: una vista no disponible en el sistema saliente hace la preparación `Invalid` con «El piloto no borra vistas y la vista … ya no está disponible en el sistema saliente: usa RACKEDITAR.». El escritor «never erases a view» | PP:293-295, PP:390-401; WR:26 |
| **Barrera de diagnósticos** | No existe. PREPARE solo para si falla la construcción del sistema saliente (`SystemFailurePrefix`). No llama a `PushBackEditorOutputGate` ni a `RackBomOutputGate` | PP:375-388 |
| **PREPARE** | `Prepare`: NotApplicable → PendingText → NothingToApply → Conflict (comparación cruda de la base con la relectura, **antes** de la admisión) → NotAdmitted → Invalid (a-e) → Ready. Total: una excepción es `Invalid` | PP:309-334; `Build` PP:337-430 |
| **MUTATE** | Bajo el ámbito de la corrida: la relectura del ámbito es la autoridad; el origen se elige otra vez; la base del plan se compara con la relectura (conflicto); el destino (claves y bloques preparados); supervivientes y capas bloqueadas; una `WriteUnit` por miembro; `Seal` | UR:347-432 |
| **POST** | Purga una vez sobre la unión de lo que dejaron las unidades, renombrado con los nombres deseados, lectura de nombres, una regeneración y la relectura; `ComparePost` | UR:460-493; WR:160-207 |
| **Deshacer** | Una transacción propia en MUTATE (`StartTransaction` WR:155, `Commit` WR:305); POST fuera de ella, en el mismo contexto de comando. Smoke-2 midió **una sola entrada** de Deshacer por «Actualizar» del panel (S2-03 PASS) | WR:146-157, :301-307; `F4-SMOKE-2/run/run-log-2.md:50-52`; `F4-DRAFTS/gate-close.md` (fila S2-03) |
| **`ComparePost`** | Mismas claves que el plan, sobre de cada clave igual en ordinal y la misma escala (`postRead.Read.InitialState!.Inputs.Annotations.AnnotationScale`). Las causas: `POST_NOT_ADMITTED`, `POST_KEYS_DIFFER`, `POST_ENVELOPE_DIFFERS`, `POST_SCALE_DIFFERS` | UR:163-183 |
| **Rechazo AUTH-13** | Reglas 10 y 11 del lector: `RackAuthoredComparatorPorts.PushBack().Compare(...)`; `Divergent` → «No editable aquí: las definiciones del rack no coinciden»; cualquier otro resultado que no sea `Single` → «No editable aquí: ilegible o de versión futura». Toda vía sin admisión lleva «Usa RACKEDITAR para editarlo con el editor clásico.» | IR:205-222; IR:35, :50, :56 |
| **Conflictos** | En PREPARE, `input.StaleBase.CompareWith(reread.StaleBase)` antes de la admisión (PP:322-323); en MUTATE, `DraftStaleBase.Create(plan.ExpectedBase, …).CompareWith(reread)` (UR:402-405). Un conflicto es `NotAppliedConflict` y el borrador queda en Conflicto; «Conservar mi borrador» del panel = `KeepMineOnCurrentVersion(pilot.Read, PushBackPilotMaterializer.KeepPilotEditsOn, confirmed: true)` | PP:322-323; UR:402-405; WHC:694; DS:469-486 |
| **«Conservar mi borrador» hoy** | `KeepPilotEditsOn`: una copia de la base nueva con la escala del borrador | PM:242-251 |
| **El anfitrión** | `TryUpdatePilot` es el único que llama a `PushBackPilotUpdateRun.Execute`; el borrador es `DraftSession<PushBackPilotDraftState>` en todo el núcleo | WHC:844-945 (llamada WHC:895); `_drafts.Find<PushBackPilotDraftState>` en WHC:403, :498, :583, :667, :754, :859, :1307, :1723, :1909 |

## R-2. Lo que hace hoy el «Actualizar» clásico (a igualar en resultado)

| Hecho | Ancla |
|---|---|
| **Nombre:** `name = IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName`; `system.Name = name`; el GUID nunca cambia (`id = embed.Id`, con el de la ventana solo de respaldo) | RPC:186-190 |
| **Vistas a redibujar:** todas las definiciones del GUID (`FindRackBlocks`) | RPC:198 |
| **Cortes y hechos de disponibilidad, una vez:** `new PushBackSystemLateralBuilder().Cortes(system, catalog)` y `new PushBackViewAvailabilityFacts(cortes.PostIndex, system.IsComposite)` | RPC:268-271 |
| **Planta y frontales:** siempre se redibujan; el frontal no consulta la disponibilidad (H-PB-18) | RPC:276-319 |
| **Lateral obsoleto:** si la disponibilidad no es `Available` o el corte no existe, la definición va a `staleViewBlocks` y no se redibuja | RPC:320-333 |
| **Regla del último vínculo:** borra los obsoletos solo si queda alguna superviviente (`blocks.Count - stale.Count > 0`); si no, el aviso «todas las vistas Push Back quedaron obsoletas; no se elimino el ultimo vinculo del rack.» | RPC:348-356 |
| **Una regeneración** si algo cambió; el mensaje final | RPC:358-383 |
| **Crear vistas:** solo en la rama `!window.UpdateOnly` (Insertar) | RPC:193-196, :369-374 |
| **El borrado clásico:** `RackCommandSupport.EraseViewBlocks` con su propio bloqueo y su propia transacción (RCS:229-231); por cada definición captura las definiciones anidadas, borra **todas** las referencias directas (`GetBlockReferenceIds(directOnly: true, forceValidity: true)`, RCS:256-262), borra la definición, confirma (RCS:269) y purga las anidadas tras la confirmación (RCS:274) | RCS:218-275 |
| **Precedente de un borrado dentro de la transacción del llamador:** `RackSelectivoInsertIntegration.EraseDefinition(Transaction, ObjectId)` (referencias directas y definición, sin confirmar) | `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs:281-289` |
| **La disponibilidad pura de Push Back:** planta siempre; lateral si su poste está entre los cortes; frontal de A siempre; frontal de B solo si el sistema es compuesto | RVA:94-116; `Evaluate` RVA:136-146 |

## R-3. Lo que el bloque M de PB-T0 deja para PB-T3

Todo de BM (cerrado el 2026-11-17):

| Id | Hecho | Ancla |
|---|---|---|
| M-a | **H-PB-16 confirmado:** ni «+ frente», ni la conversión a A/B, ni la vuelta a un sentido crean vistas. Fila 1 del §6.4: PB-T3 iguala al clásico **sin crear vistas**, sin volver al Owner | BM:20, :24, :92-97 |
| M-b | **El borrado del corte obsoleto** borra la definición y su referencia; la regla del último vínculo descarta la edición entera (no se escribe nada) | BM:21-22, :61 |
| M-c | **Renombrar:** definiciones y `Name` de los sobres al nombre nuevo; con colisión, la definición no cambia de nombre y no hay sufijo | BM:23 |
| M-d | **Frontal B al quitar B (H-PB-18):** no se borra; se redibuja como frontal de un sentido, se renombra sin el sufijo de lado y su sobre **conserva `Section` 2**. Lo que pasa si ya hay un frontal de A con ese nombre no se midió | BM:25, :67-72 |
| M-e | **H-PB-17 en lo que toca a B: NO EJERCIDO** en AutoCAD (el rack solo tenía el lateral del poste 0) | BM:25 |
| M-f | **Un `U` por «Actualizar»**, también con borrado, renombrado o conversión A/B | BM:31-45 |
| M-g | **Vistas por definición, no por referencia:** una definición sin referencias cuenta como superviviente y se redibuja | BM:49-53 |
| M-h | **Referencias anidadas:** el borrado alcanza todas las referencias directas, también las anidadas en otras definiciones (lectura de código, no medido) | BM:54-60 |
| M-i | **`AbsentSlotsB` tras la ida y vuelta A/B:** pasa de `[0]` a las 72 ranuras con `SideB` nulo; PB-T3 «tiene que reproducirlo o declararlo» | BM:62-66 |
| M-j | **Sin decisión nueva del Owner** | BM:141 |

## R-4. El API de PB-T1b que PB-T3 consume (en `2f5d3dec`)

| Miembro | Ancla |
|---|---|
| `PushBackEditableState` (constructor y portadores) | PES:38-95 |
| `LoadNewEditor`, `LoadDesignEditor`, `RestoreEditor` | PES:100-126 |
| `AdoptCarriers`, `SeedDefaultSafety`, `LoadNew`, `LoadDesign(design, rackName, resolver)`, `Restore` | PES:130-203 |
| `BuildInputs(assembler)` (objeto nuevo en cada llamada) | PES:367-405 |
| `ResolvePostCatalogId`, `ResolveWeightUnit`, `SupportOf` | PES:415-465 |
| `PushBackEditableStateComparer.Instance`, `Equals`, `GetHashCode`, `Copy` | PESC:29-63 |
| El mapa: `PushBackFieldOwner`, `PushBackDocumentLevel`, `PushBackOwnershipRow`, `Table`, `Normalize`, `Of` | PESO:9-176 (`Of` PESO:82-113; tabla PESO:133-175) |
| `PushBackEditorOutputGate.Diagnostics`, `IsBlocked`, `StatusOr` | GATE:16-40 |

**No hay API de asiento en el producto:** el ayudante `Pasada` solo existe copiado en las clases de prueba ([PKT1] §5.0; D-PBT1-04: «No hay API de asiento (I-03)»). El arrastre A-2 de [PKT1] §10 la prevé «si PB-T3 o PB-T4 la necesitan».

**Arrastres de producto de PB-T1b** (independent-review del intento 1, `:45-51`; `analysis.md` §3): M09 (`Of` decide «declarada» por la tabla), M11 (`LoadDesignEditor`/`RestoreEditor` con un diseño nulo cambian el estado antes de lanzar), M12 (`SameAs` con `NaN`), M15 (coste de `Equals`: CT-06 27,7 s y CT-08 34,9 s en la CI).

## R-5. La ExtensionData y AUTH-13

| Hecho | Ancla |
|---|---|
| Cajón de campos desconocidos solo en tres niveles: sobre, proyecto y carga útil | `RackEmbedDocument.cs:74`; `RackProjectDocument.cs:51`; `PushBackDesignDocument.cs:74-76` |
| El sobre saliente conserva el cajón y las `CustomProperties` del **sobre fuente** y no rebaja su versión | `RackEmbedComposer.cs:43-60` |
| El proyecto saliente conserva el cajón y la versión del **proyecto fuente** (`PreserveUnknownMetadata`) | `RackProjectStore.cs:60`, `:71-95` |
| La carga útil saliente conserva el cajón y la versión de la **carga útil fuente** (`project.SourceDocument?.PushBack`) | `SystemRegistry.Default.cs:168`; `PushBackDesignDocument.cs:92-98` |
| El plan del piloto compone cada miembro sobre el sobre **releído** y con el proyecto **releído** de esa vista como fuente (`WithSourceMetadataFrom(preflight.ResolvedSources[index])`) | PP:421-422, PP:434-438 |
| AUTH-13 rechaza un campo desconocido en el envoltorio, en la carga útil y en el sobre (CT58-15) y en cada pieza anidada de Push Back, módulos y lado B incluidos (CT58-16), con resultado `Unreadable` | `tests/RackCad.Tests/I58F1Matrix.cs:58-64`, `:97-102` |
| En el lector del piloto, ese `Unreadable` es «No editable aquí: ilegible o de versión futura» con la pista de RACKEDITAR | IR:221-222, IR:35, IR:56 |

## R-6. Las guardas de F4 que leen como texto los archivos que PB-T3 cambiaría

**Cómo:** `census_pins.py` (todas las clases de `tests/` que nombran como literal las rutas candidatas) y `census_methods2.py` (por método, con las constantes concatenadas y los ayudantes de una línea resueltos). Los recuentos de métodos son estáticos (`[Fact]`/`[Theory]`); las filas de las `Theory` no se cuentan.

| Clase | Métodos | Archivos de producto que lee |
|---|---|---|
| `WorkspacePilotPrepareGuardTests` | 17 | PP (el plan), DS, PM y una lista de constructores |
| `WorkspacePilotUpdateGuardTests` | 42 | UR, UP, SR, WR, `RackSiblingScan.cs`, WHC, DS, PM y la carpeta `Pilot/` |
| `WorkspacePilotUpdateIntegrationGuardTests` | 43 | UR, `PushBackPilotHostRun.cs`, WR, `RackPilotUpdateCommandContext.cs`, `RackPilotIntentAdapter.cs`, WHC y todo el producto |
| `WorkspaceDraftPilotIntentGuardTests` | 13 | IR, `PushBackPilotIntentResult.cs`, `PushBackPilotIntentScan.cs` |
| `CustomPropertiesEnvelopeGuardTests` | 9 | el censo de todo `src/` de las llamadas a `RackEmbedComposer.Compose` (I-54 G4B) |

**Lo que fijan y que el diseño de PB-T3 toca** (lectura de cada método frente al cambio previsto):

| Id | Método (clase, línea) | Qué fija por su letra | ¿Lo rompe PB-T3b? |
|---|---|---|---|
| S-1 | `TheFileOfThePlanComposesExactlyOnceInAMemberOfItsOwnWithAParameterSourceAndTheNameOfTheRereadEnvelope` (PrepareGuard:121-125; `ComposeViolations` :102-119) | El miembro `ComposeMemberEnvelope(RackEmbedDocument source, string view, int section, string designJson)` exacto y `source.Id, source.Name,`: «The name is the one of the envelope that was re-read: never a value of the draft» (:117-118) | **Sí:** renombrar exige el nombre del borrador |
| S-2 | `TheComposeGuardTripsOnAnotherCallAnUnsourcedCompositionANullableSourceOrAnotherName` (PrepareGuard:127-139, `Theory` de 5 filas) | `Assert.Contains(from, plan)` con `"source.Id, source.Name,"` y `"RackEmbedDocument source, string view"` | **Sí en dos filas (1 y 3), que no encuentran su texto; las otras tres (2, 4 y 5) quedan vacías**: el texto real ya viola S-1, así que la mutación no cambia el resultado (v2) |
| S-3 | `TheMemberPlansAreBuiltOnlyFromTheMutableMembersOfTheRereadInOrdinalOrderAndNeverErase` (PrepareGuard:229-251) | `Build` sin `Erase`, `Delete`, `Remove`, `Phantom` ni `Obsolete`; sin `.Name`; contiene `input.CurrentDraftState.Inputs.Annotations.AnnotationScale` | **Sí:** borrar y renombrar cambian su premisa |
| S-4 | `TheRunReleasesBothScopesInFinallyBlocksOfItsOwnAndNeverWithAUsing` (UpdateGuard:126-141) | El literal `private DraftApplyOutcome<PushBackPilotDraftState> Post(IPushBackPilotUpdateScope scope)` | **Sí, solo por el tipo:** la corrida genérica cambia el literal; la premisa (ámbitos en `finally`, POST tras liberar MUTATE) se conserva |
| S-5 | `M01TheEscapeOfTheRunIsClassifiedByTheStateThatTheRunAlreadyHadAndNeverAsAPlainFailureAfterAWriteOrASeal` (IntegrationGuard:895-900; `RunEscapeViolations` :880-893) | `public static DraftApplyOutcome<PushBackPilotDraftState> Execute(` con `var execution = new Execution(` antes del `try`, y `public DraftApplyOutcome<PushBackPilotDraftState> OutcomeOfAnEscape(` | **Sí, solo por el tipo** |
| S-6 | `M01TheEscapeGuardTripsOnACatchThatDoesNotConsultTheStateAndOnAnEscapeThatAnswersAPlainFailureFirst` (IntegrationGuard:902-910) | Los reemplazos de `"var execution = new Execution(draft, request, port);"` y de la primera rama de `OutcomeOfAnEscape` | **No falla: queda vacía** (corregido en la v2, con la revisión). Con `Execution<TState>`, `RunEscapeViolations` ya da una violación sobre el texto real (`IndexOf("var execution = new Execution(")` = −1). Las tres sustituciones no encuentran su texto o no cambian nada, y `Assert.NotEmpty` sigue cierto. Hay que saltarla igual, porque ya no comprueba nada |

**Lo que el diseño mantiene en verde a propósito** (con la elección de diseño que lo permite):

| Guarda | Por qué sigue en verde |
|---|---|
| PrepareGuard `TheFileOfThePlanIsNullableEnabled…DeclaresTheTypesOfThePackage` (:81-98): la firma de `From(DraftSession<PushBackPilotDraftState>)` y **una sola** `new PushBackPilotPrepareInput(` | La fábrica del piloto conserva su firma; el modelo completo tiene su propio tipo, `PushBackFullPrepareInput`, con su constructor privado (corregido en la v2: la v1 hablaba aquí de un «único `Create`», que el diseño no usa; M-16 de la revisión) |
| PrepareGuard `ComposeForWriteIsCalledOnlyByThePlannerAndOnlyAfterTheComparisonOfTheBase` (:154-176) y su autoprueba (:178-190) | La llamada textual a `PushBackPilotMaterializer.ComposeForWrite(` sigue en `Build`, después de `.CompareWith(` y una vez; la línea `var comparison = input.StaleBase.CompareWith(reread.StaleBase);` no cambia |
| PrepareGuard `TheOrderOfPrepareIsTheOneOfThePackage…` (:194-227) | `Prepare` conserva su orden y sus textos |
| UpdateGuard: pureza de UP, UR y SR (`Impure`, :57-115), incluida la palabra `Transaction` en comentarios, y la carpeta `Pilot/` sin `Transaction` (:117-124; IntegrationGuard:81) | Los archivos nuevos y cambiados de `Pilot/` no nombran esas palabras, ni en comentarios |
| UpdateGuard `ThePortHasTheOperationsAndTheValuesOfThePackageAndNoWordOfAutoCad` (:143-159) | UP no cambia: el borrador de vistas es una interfaz nueva en su propio archivo |
| IntegrationGuard G-2 (:351-365): un solo implementador de `IPushBackPilotUpdatePort` | El puerto no cambia de implementador |
| IntegrationGuard G-7 (:651-675): las llamadas `.PrepareUnits(`, `.BeginMutation(`, `.WriteUnit(`, `.Seal(` solo en UR | La corrida sigue en UR |
| IntegrationGuard G-9 (:681-719): los archivos que nombran el piloto con API de escritura, con sus cuentas exactas (WR: `RedrawInTransaction` 3, `SystemBlockWriter` 2, `Commit` 1, `StartTransaction` 1, `StartOpenCloseTransaction` 1, `SyncName` 1) | PB-T3b no toca el Plugin; PB-T3c llama a una fachada nueva cuyo nombre no es ninguno de los términos y cuyo archivo no nombra el piloto |
| UpdateGuard, guardas del escritor (prohibiciones :182-212, estructura :215-255, inyección de depuración :410-442, relectura de MUTATE :444-470) | En PB-T3c, el escritor no nombra `GetBlockReferenceIds`, `Erase(`, `EraseViewBlocks`, `ForWrite` ni `UpgradeOpen` (los lleva la fachada); un solo `StartTransaction`, un `Commit`, una purga; la inyección de unidad se reutiliza sin un tercer `[Conditional("DEBUG")]` |
| `WorkspaceDraftPilotIntentGuardTests` (IR llama una vez a la clasificación, al comparador y al materializador) | IR no cambia: el lector completo es un archivo nuevo que llama a `PushBackPilotIntentReader.Read` |
| `CustomPropertiesEnvelopeGuardTests` `ExpectedComposeCensus` (:194-207) | El único compositor del piloto sigue siendo PP `ComposeMemberEnvelope` con primer argumento `source`; ningún archivo nuevo llama a `RackEmbedComposer` ni construye un `RackEmbedDocument` |

**La variante (A′)** (v2; la revisión la proponía con «solo S-1 y S-2»). El `Build` y la `Execution` del piloto quedan textualmente intactos, y el modelo completo lleva su propio armado y su propia corrida privados en PP y UR. **Medido por lectura de UpdateGuard:126-141:** S-4 hace `Assert.Equal(1, Src.Count(code, @"\bmutation\.Dispose\s*\("))` y lo mismo con `scope.Dispose` sobre **todo** UR (`:134-135`). Una segunda corrida en UR con sus propias liberaciones da 2, y S-4 falla. Evitarlo cambiando los nombres de las variables de la copia sería debilitar la guarda por su letra. Así que (A′) exige **S-1, S-2 y S-4**. S-3 sigue en verde (el `Build` del piloto conserva sus agujas y no nombra `.Name`, si llama a una sobrecarga de cuatro argumentos de `ComposeMemberEnvelope`), y S-5 y S-6 también (la `Execute` del piloto y su `OutcomeOfAnEscape` no cambian y van primero en el archivo).

**Lo que no se midió:** si alguna otra guarda de F4 o de I-5x falla por el diff real (solo hay diseño). Los de la tabla son los que el diseño rompe o deja vacíos **por su letra**, leídos uno a uno. La revisión, que leyó método a método las cuatro clases, no encontró otro (su §6). Por eso:
- el contrato de PB-T3b pide al Worker **medir la lista sobre el texto del GREEN**, con una corrida temporal sin los `Skip` (INV-PBT3B-15 en la v3; INV-PBT3B-07 en la v2);
- y lleva la parada C16 para cualquier otra.

La comprobación acotada no puede medirla «sobre el esqueleto», como decía la v1, porque el RED deja el piloto sin tocar.

## R-7. La premisa «el piloto nunca borra» en el Plugin

- **Comentario del escritor:** «It never erases a view.» (WR:26). Va en un comentario; `Src.Code` quita los comentarios antes de aplicar las guardas.
- **Las guardas lo fijan por nombres:** `\bErase\s*\(` y `EraseViewBlocks` prohibidos en el texto del escritor (UpdateGuard:189), con sus autopruebas (:317-318).
- **Consecuencia para PB-T3c:** si el borrado vive en una fachada propia, llamada desde el escritor, esas guardas siguen en verde por su letra. La premisa sí cambia, y el paquete lo declara; no se sustituye ninguna guarda del escritor (§8 del paquete, D-2).

## R-8. Recuentos de pruebas que el paquete usa

| Filtro o clase | Recuento | Fuente |
|---|---|---|
| Core `Workspace` | 3 271 | [PKT1] §6.2 (medido en la CI) |
| UI `Workspace` | 547 | [PKT1] §6.3 |
| `WorkspacePilotPrepareTests` / `WorkspacePilotUpdateRunTests` / `WorkspaceDraftPilotMaterializationTests` / `WorkspacePilotUpdateHostTests` / `WorkspacePilotUpdateRequestTests` / `WorkspacePilotReconciliationHostTests` / `WorkspaceDraftPilotIntentReadTests` / `WorkspaceDraftPilotStateTests` | 67 / 49 / 41 / 35 / 44 / 28 / 63 / 27 métodos | conteo estático en `2f5d3dec` |
| Las cuatro guardas de F4 de R-6 | 17 / 42 / 43 / 13 métodos | ídem |
| `PushBackEditableState` (Core), `PushBackEditorOutputGate`, `PushBackSharedReferenceGuardTests` | 13, 1, 2 | [PKT1] §6.2 |
| `PushBackClassicCharacterization` (UI y Core), `PushBackFullEditorCharacterization` (UI y Core), `PushBackStateNet`, `PushBackDeclaredCorrection` (UI), `Cantilever` (UI) | 55 y 10, 16 y 3, 14, 14, 67 | [PKT1] §6.1 y §6.2 |
| `I58F1CharacterizationTests`, `I58F1OracleChecks` | 2 y 8 métodos | conteo estático |
| `CustomPropertiesEnvelopeGuardTests`, `NamespaceFolderGuardTests` | 9, 7 | conteo estático; [PKT1] §6.2 |
| `PushBackFullEditorCharacterizationLateralCutTests` (CT-PB0-10, la decisión pura del borrado) | clase de Core de PB-T0 | `tests/RackCad.Tests/PushBackFullEditorCharacterizationLateralCutTests.cs:15-23` |

**El corpus extendido de CT-PBT1A-14** (los 17 de `Fixtures/I64`, X-01, F-01, F-02 y F-03) da `bloqueado=False` en todas sus líneas esperadas: `bloqueado=True` aparece 0 veces en `tests/RackCad.UI.Tests/PushBackStateNetFingerprintTests.cs`. La barrera de PB-T3 no cambia la salida de ese corpus.

## R-9. Costuras y tipos de prueba que PB-T3 puede reutilizar

| Qué | Ancla |
|---|---|
| El doble del puerto de la actualización (registra cada llamada, aplica las escrituras en el sellado, se le puede pedir que lance o que cambie el dibujo entre pasos) | `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateScopeDouble.cs:13-60` (`internal sealed class PilotUpdateScopeDouble`) |
| `InternalsVisibleTo` de Application a `RackCad.Tests` (el constructor `internal` de `PushBackPilotIntentResult` se puede usar en pruebas) | `src/RackCad.Application/RackCad.Application.csproj:11`; `PushBackPilotIntentResult.cs:27-41` |
| `DraftSession<TState>` es genérico (comparador y copiador inyectados) | DS:239-264 |
| Los tres proyectos de C# son `net8.0` (el Plugin, `net8.0-windows`) | los cuatro `.csproj` |

## R-10. La regla de supervivientes del panel

`IsSurvivor` = miembro `Redraw` con al menos una referencia en un espacio de diseño (`src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:64`). La corrida exige al menos uno antes de importar y otra vez en MUTATE (UR:536-547). El clásico cuenta definiciones con o sin referencia (M-g). El panel es más estricto, y así sigue.


## R-11. Lo que no se pudo medir

- **Nada se compiló ni se ejecutó.** Las cifras de la CI de PB-T3 son previsiones; la línea base se mide sobre el cierre de PB-T1c.
- **El diff real de PB-T3b frente a las guardas de F4:** solo la lectura de R-6. Lo mide el Worker sobre el texto del GREEN (INV-PBT3B-15; en la v2, INV-PBT3B-07), con la reactivación temporal, local y sin commit que admiten el invariante común 07 y C02 (V3-03), y lo cubre la parada C16 del contrato.
- **Cómo cuenta xUnit una `Theory` con `Skip`** en la TRX (S-2 tiene cinco filas): una o cinco filas `Skipped`. Se mide en la CI del RED de PB-T3b, que ya lleva los `Skip`, y fija el `MinSelected` de `Workspace`.
- **Si el asiento** (la secuencia de la carga de `LoadExisting` con sus pasadas, fuera de la ventana) da el mismo estado que la ventana en todo el corpus: por lectura sí, porque es la secuencia de CT-PBT1C-01; lo fija CT-PBT3A-01.
- **El Deshacer con borrado dentro de la transacción del panel:** por lectura, una sola entrada (una transacción y el mismo contexto de comando, como S2-03); se mide en el Smoke-PB de PB-T5.
- **El frontal B con un frontal de A del mismo nombre** (BM:72): no medido en AutoCAD. PB-T3 lo rechaza por defecto, así que no lo necesita.
- **Si el lector del piloto admite `pb-01-base` con `Composite.StructureOverrideA = 1`** sin que el resolver lance, y si el piloto admite `I58F1Fixtures.Rich("pushback")`: por lectura, el primero sí (CT-PBT1B-01 carga ese bloqueante en la ventana con `LoadExisting` y la ventana muestra su estado), y el segundo no (trae el lado B). CT-PBT3B-06 y CT-PBT3A-04 lo comprueban en su propia línea, con la parada C07.
- **El estado de bloqueo de los diseños que usan las clases de comportamiento del piloto de F4:** no se midió diseño a diseño. Va a la remedición (precondición 2), y la parada PBT3B-C14 cubre una prueba que dependa de escribir un diseño bloqueado.
- **PB-T1c:** emitida en `833ea6bf`, pero su producto no existe todavía. Este registro se vuelve a medir sobre el cierre de PB-T1 antes de emitir.
- **El censo de declaraciones de PB-T3b y PB-T3c** (R-18): lo que se añade sale del diseño, no de un producto. Lo mide el Worker en su entrega.

## R-12. Incidencia de procedimiento: un modo de fallo repetido

- **En la v1,** una orden lanzó `python -I -` con un heredoc que solo imprimía `noop`, para comprobar el intérprete. **Incumple la regla del encargo** («Python … nunca con `-c` ni heredoc»). No leyó ningún archivo ni ejecutó nada más.
- **Es un modo de fallo repetido:** la medición H-12 de PB-T1 registró la misma clase de incidencia (`I-64-f4b-pbt1-measurements.md:227`; M-12 de la revisión). **Se devuelve al Coordinator como modo repetido**, antes de clasificarlo, en lugar de declararlo otra vez como incidencia aislada.
- **En la v2 no se usó ningún heredoc.** Una orden lanzó por error `python -I census_methods.py` sin argumentos: es un script escrito con la herramienta de archivos, no un heredoc. Falló en `sys.argv[1]` antes de llamar a git, sin leer nada.
- **En la v3 tampoco.** Dos scripts de solo lectura, escritos con la herramienta de archivos, que no llaman a git y solo leen el paquete: `count_question.py` (cuenta la pregunta de D-1, su `header` y sus opciones) y `check_tables.py` (comprueba las columnas de las tablas y que no queden rutas locales). Algunas ediciones mecánicas del paquete de la carpeta de salida se hicieron con `sed -i` (por ejemplo, el cambio del nombre de la medición a INV-PBT3B-15), sin tocar nada fuera de esa carpeta.

## R-13. El origen borrado (B-01 de la revisión)

| Hecho | Ancla |
|---|---|
| El origen del plan es el **único** miembro mutable cuyo texto es, byte a byte, el origen del borrador; puede ser un lateral | PP:357-361 |
| MUTATE elige el origen otra vez y exige que sea la clave del plan; el origen todavía existe en MUTATE | UR:398-405 |
| POST relee con `ScanOf(values.Scan, plan.OriginKey)` | UR:480-481 |
| La regla 2 del lector: si la clave del origen no está entre los miembros → «No editable aquí: el origen ya no está en el dibujo» | IR:162-166; IR:29 |
| Un `NotAdmitted` en POST → `POST_NOT_ADMITTED: …` → `AppliedPostRereadFailed`, con la escritura ya confirmada | UR:167, UR:482-483, UR:579-580 |
| El clásico borra los laterales obsoletos entre **todas** las definiciones del GUID, la elegida incluida, si queda alguna superviviente | RPC:198, :320-333, :348-352 |
| Ninguna guarda de F4 fija el texto `plan.OriginKey` de POST (`git grep OriginKey` en `tests/RackCad.Tests/Workspace/*Guard*.cs`: 0) | — |
| Las lecturas del anfitrión usan la referencia elegida que manda el panel | WHC:328, :409, :675 (`request.ChosenReference`) |

## R-14. La barrera del piloto

| Hecho | Ancla |
|---|---|
| `PushBackCompositeDiagnostics.Evaluate(system)` devuelve una lista vacía si `system.Composite` es nulo | `src/RackCad.Application/Systems/PushBack/PushBackCompositeDiagnostics.cs:142-149` |
| `IntentDiagnostics()` evalúa siempre el hueco, el separador y las dos estructuras manuales, sea el rack compuesto o no | `PushBackCompositeEditorState.cs:629-631`; `PushBackCompositeDiagnostics.cs:94-140` |
| El texto de la estructura manual no construible: «Lado {0}: la estructura manual ({1}) no es construible; el minimo fisico es {2} fondos. Corrige el valor o pulsa «Restaurar estructura» para volver a la propuesta.» | `PushBackCompositeDiagnostics.cs:133-139` |
| `PushBackOutputGateTests.Blocking()` es **compuesto** (dos lados con la estructura manual al mínimo); el piloto no lo admite | `tests/RackCad.Tests/PushBackOutputGateTests.cs:35-46`; PM:148 |
| CT-PBT1B-01 ya carga en la ventana «estructura manual de 1» como bloqueante, con `LoadExisting` | [PKT1] §5.3, CT-PBT1B-01 |

## R-15. `tests/RackCad.Tests/Workspace/`

| Hecho | Resultado |
|---|---|
| `git ls-tree --name-only 2f5d3dec tests/RackCad.Tests/Workspace/` | 79 archivos |
| De ellos, los tres de guardas de D-1 (`WorkspacePilotPrepareGuardTests.cs`, `WorkspacePilotUpdateGuardTests.cs`, `WorkspacePilotUpdateIntegrationGuardTests.cs`) | 3 |
| La lista explícita del `ForbiddenWriteScope` de PB-T3b, con (A) | 79 − 3 = **76**, que vuelve a medir el generador |
| **Con (A′)** (v3; N-05, V3-05): solo dos archivos de guardas en el `AllowedWriteScope` (`WorkspacePilotPrepareGuardTests.cs` y `WorkspacePilotUpdateGuardTests.cs`) | 79 − 2 = **77**; `WorkspacePilotUpdateIntegrationGuardTests.cs` queda prohibido |
| `git ls-tree --name-only 833ea6bf tests/RackCad.Tests/Workspace/` (v3) | 79 archivos: la misma cuenta |
| La lista curada de PB-T0 lleva el prefijo `tests/RackCad.Tests/Workspace/` | `I-64-f4b-pbt1-measurements.md:155`; `F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json` (lo midió la revisión) |

## R-16. Los ayudantes de I-58 que usa CT-PBT3A-04

| Hecho | Ancla |
|---|---|
| `I58F1Fixtures` es `internal static class`; `Root`, `Structure`, `Sibling`, `Set` y `Rich` son `public static` | `tests/RackCad.Tests/I58F1Fixtures.cs:30`, `:98-125` |
| `Rich("pushback")` añade niveles, seguridad completa, líneas de cabecera, postes derivados, peraltes, fondos, tarimas, topes y topologías | `I58F1Fixtures.cs:125-146` |
| `I58F1Matrix.Nested("pushback")` es **privado** y da 28 rutas: 20 bajo `PushBack.Structure` y 8 de Push Back (`PushBack.Structure`, `PushBack.Fronts.0`, `PushBack.SideB`, `PushBack.SideB.Fronts.0`, `PushBack.SideB.FrontConfigs.0`, `PushBack.Composite`, `PushBack.Composite.Topologies.0` y `PushBack.RearTopeOffCells.0`) | `tests/RackCad.Tests/I58F1Matrix.cs:97-102` |

## R-17. Las decisiones de F4B y la Snapshot J

| Hecho | Estado en git |
|---|---|
| **F4B-E19,** la Snapshot J (`F4B-DRAFTS/coordinator/owner-config-snapshot-j-decision.md`; `F4-ENVIRONMENT/snapshot-J/snapshot-j.json`) | En git, en `2f5d3dec` y en `a18ba345` (`f4b-dispositions.md:572`) |
| **F4B-E23** (D-PB-07, «BOM y biblioteca») | No afecta a PB-T3 |
| **F4B-E24,** la ExtensionData anidada (`owner-f4b-dpb07-extensiondata-decisions.md`, pregunta 2) | El número lo fija la v2.8 de PB-T1 (V28-25). En la v2: no estaba en git en `a18ba345`. **En la v3: en git desde `95373af6`** (`f4b-dispositions.md:684` en `833ea6bf`; el archivo, en `F4B-DRAFTS/coordinator/`). `git log -S '## F4B-E24.'` da `95373af6`. Queda de la precondición 8 que entre en la `AuthorityRevision` |
| **F4B-E25,** las rutas `UNIT_CHANGE` en una corrección (`owner-f4b-correction-unit-change-decision.md`) | Ídem (V28-22 y V28-23). **En la v3: en git desde `95373af6`** (`f4b-dispositions.md:693`; `git log -S '## F4B-E25.'` da `95373af6`) |
| **F4B-E26 y F4B-E27** (v3) | `f4b-dispositions.md:711` (el cierre de PB-T1b) y `:761` (la emisión de PB-T1c), en `833ea6bf` |
| `I-64.yml` en `2f5d3dec` | `:57` (`f4b_status`) cita la Snapshot J (F4B-E19); `:58` (`codex_environment`) todavía dice «Snapshot G». La línea base del Controller la fija F4B-E19 |
| `I-64.yml` en `833ea6bf` (v3) | `:9` `attempts: 1` (los de PB-T1c); `:57` (`f4b_status`): PB-T0, PB-TC, PB-TC2, PB-T1a y PB-T1b CLOSED / PASS WITH LIMITATIONS (PB-T1b sobre `a18ba345`, attempts 2 de 3), PB-T1c EMITIDA, attempts 1 de 3 (F4B-E27); `:58` sigue diciendo «Snapshot G», como en `2f5d3dec` |
| F4B-E19 en `833ea6bf` (v3) | `f4b-dispositions.md:572`, sin cambio |

## R-18. Las declaraciones de PP, UR y WR (v3; N-02, V3-02)

**Método:** `git show 833ea6bf:<archivo>`, filtrado por las líneas que empiezan por un modificador de acceso, y `git grep` de los nombres nuevos en `833ea6bf` sobre `src/RackCad.Application/Systems/PushBack/Pilot/` y WR. `src` es igual que en `2f5d3dec`.

| Hecho | Ancla en `833ea6bf` |
|---|---|
| PP declara `PushBackPilotPrepareInput` (constructor privado; `RackId`, `StaleBase`, `InitialDraftState`, `CurrentDraftState`, `HasPendingText`, `IsDirty`, `ConsideredStatus`; `From`), `PushBackPilotPrepareKind`, `PushBackPilotPrepareResult`, `PushBackPilotKeyedText`, `PushBackPilotMemberPlan`, `PushBackPilotPostExpectation`, `PushBackPilotUpdatePlan` y `PushBackPilotUpdatePlanner` | PP:19-58, :74, :99, :138, :144, :181, :208, :275 |
| `PushBackPilotPostExpectation` y `PushBackPilotUpdatePlan` tienen **un** constructor interno cada uno | PP:183; PP:210-217 (siete parámetros) |
| `PushBackPilotUpdatePlanner`: las constantes, un solo `Prepare`, el `Build` privado y **un** `ComposeMemberEnvelope` privado de cuatro parámetros | PP:277-301; PP:309; PP:337; PP:434 |
| UR declara `PushBackPilotPrecheck` y `PushBackPilotUpdateRun`: las constantes, **un** `Precheck`, **un** `Execute`, **un** `ComparePost` y `DesiredNamesOf` | UR:15-35; UR:51-116; UR:122; UR:139; UR:163; UR:190 |
| `Execution`, clase privada anidada: los campos `_draft`, `_request`, `_port`, `_warnings`, `_input`, `_plan`, `_catalog`, `_writeStarted`, `_sealReturned` y `_mutationDisposeFailure`; el constructor; y `Run`, `OutcomeOfAnEscape`, `Prepare`, `Mutate`, `Write`, `WithAbortFailure`, `Post`, `DesiredNamesOrNone`, `ScanOf`, `ChooseOrigin`, `SurvivorsAndLayers`, `WithScopeDisposeFailure`, `PostFailed`, `NotAppliedFailure` y `NotAppliedPreflight` | UR:227-587 |
| **Los miembros de `Execution` con `PushBackPilotDraftState` en su firma:** el campo `_draft`, el constructor, `Run`, `OutcomeOfAnEscape`, `Prepare`, `Mutate`, `Write`, `WithAbortFailure`, `Post`, `ChooseOrigin`, `SurvivorsAndLayers`, `WithScopeDisposeFailure`, `PostFailed`, `NotAppliedFailure` y `NotAppliedPreflight`. La recomprobación nombraba todos menos `_draft`, `ChooseOrigin` y `SurvivorsAndLayers` | UR:229, :241, :250, :293, :303, :347, :391, :435, :460, :521, :537, :550-551, :579, :582, :585 |
| `_input` es `PushBackPilotPrepareInput?`, y `Prepare` y `Write` leen de él el origen del estado inicial | UR:234; UR:305 (la asignación), :311 y :399 (`_input.InitialDraftState.Origin` y `_input!.InitialDraftState.Origin`) |
| **Ningún nombre nuevo de PBT3B-C13 existe** en `Pilot/`: `BuildCore`, `BuildFull`, `ComparePostCore`, `Erasures`, `ErasedKeys`, `PostOriginKey`, `BlockedPrefix`, `FrontalBOfOneWay…`, `AllViewsObsoleteReason`, `EraseUnsupportedReason`, `PostErasedStillPresentPrefix`, `_prepare`, `_originOf`, `_postRead`, `_origin`, `FullExecution` y `PushBackFullPrepareInput` | `git grep -w` en `833ea6bf -- src/RackCad.Application/Systems/PushBack/Pilot/`: 0 |
| WR declara `RackPilotUpdateWriter`, `RackPilotUpdateScope` y `RackPilotMutationScope`; el constructor de `RackPilotMutationScope` es `internal` y de cuatro parámetros | WR:32, :70, :227; WR:239-243 |
| `new RackPilotMutationScope(` aparece una sola vez en `src/` y `tests/`, en `BeginMutation` | WR:156 |
| **Ningún nombre nuevo de PBT3C-C13 existe** en WR: `_erasures`, `EraseUnit`, `RackViewBlockEraser` y `EraseInTransaction` | `git grep` en `833ea6bf -- src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`: 0 |

**Consecuencia:** el censo que pide la entrega (§6.2 y §6.3 del paquete) parte de estas listas. Con (A), el diseño añade 34 declaraciones (22 en PP, 10 en UR y 2 en el archivo nuevo) y retira una (`_input`); con PB-T3c, cinco (tres en WR y dos en la fachada). Son cuentas del diseño, no de un producto.

## R-19. La remedición sobre el cierre de PB-T2c (`f273eae1`; precondición 2 de la v4.4)

**Método:**
- Es el seco del generador (`make_pbt3a_contract.py`) sobre el cierre de PB-T2c, por objetos git desde el checkout principal, con el paquete v4.4 y el informe de CA-T3a. La orden: `--objects-rev f273eae1 --pt2c-close f273eae1`.
- `measure_pbt3a.py` lo corre y extrae su sección de anclas y sus recuentos. La salida, `pbt3a-anchors-out.txt`, los compara con lo previsto en el paquete.
- La orden del extractor: `python -I measure_pbt3a.py <checkout-principal> f273eae1b0156e78260b6a5efe980a79bfe075b3`.
- Nada se ejecutó con dotnet y no se leyó el worktree.

| Hecho | Medido en `f273eae1` | Frente a lo previsto |
|---|---|---|
| La base | Desciende de `95373af6`, `5a8366c1`, `0dc623ca`, `56cb026f` y `05beabfd`. `src` es el del cierre de PB-T2b (`git diff 56cb026f f273eae1 -- src` vacío). Su entrada es F4B-E40, con el `task-close-package.md` de PB-T2c y el contrato emitido | Igual |
| `tests/` | Desde `05beabfd` solo gana `PushBackWorkspaceCarryoverTests.cs`, la clase de PB-T2c. Las nueve clases de PB-T2 están | Igual |
| Las anclas de W, por su contenido | La adopción, W:624-669; su `catch`, W:627-635; los ensambladores, W:150-151; `LastComputation`, W:221; `LoadExisting`, W:345; la costura, W:197; el `Recompute();` final de `LoadFromModel`, W:404. `Recompute`, `LoadFromModel` y `ReadInputs` tienen el cuerpo del cierre de PB-T1c | Igual: las previstas de la precondición 2 (V43-10), porque el GREEN de PB-T2c restaura W |
| Los recuentos | Core 29 y UI 68 en `tests/`; los `MinSelected` medidos y los de PB-T2, iguales a los del paquete (`PushBackWorkspaceCarryoverTests`, 3); Pasada, 11; I58F1, 28 rutas; Fixtures/I64, 17; la lista cerrada de C13, 96 declaraciones; 17 nombres nuevos y 31 de custodia libres | Igual |
| La línea base de la CI | Run `44619492077` sobre `b1968341`: Core 19 094 y UI 2 450 (2 429 pasadas y 21 omitidas) | Igual a la prevista |
| La Snapshot K, C11, F4B-E35 y la tabla de RR-3 | Como en `05beabfd` | Igual |
| La decisión del Owner sobre nc1 (F4B-E40, `owner-pbt2c-nc1-decision.md`): la firma nueva PD-F4B-01 y su sustituto para el resto de F4B | Entra como autoridad del contrato sin cambiar la v4.4: los contratos de PB-T3 que no nombran el sustituto lo usan por esa decisión | Nueva, sin efecto en el texto del paquete |

**Consecuencia:** la remedición no cambia nada material. El seco de PB-T3a sobre el cierre solo deja los AVISOS de la custodia de la emisión.
