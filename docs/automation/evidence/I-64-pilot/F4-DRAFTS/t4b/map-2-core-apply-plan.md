# I-64 F4-T4B-MUTATE-POST — Mapa 2: el núcleo de Application del Actualizar

**Objeto:** la ranura de aplicación de `DraftSession`, el PREPARE puro y su plan, la vía de lectura y la StaleBase, el host del Workspace, C-F4-05 y `LegacyDeltas`, y las pruebas y guardas que fijan hoy ese núcleo.

**Base medida:** rama `architecture/workspace-persistente-rackcad` en `c3ae1c97`.
- Contiene el GREEN de T4A1 (`bbd9a35e`) y el GREEN del intento 2 de T4A2 (`ae0a7b40`).
- Desde `bbd9a35e`, `src/` solo cambió en `WorkspaceEditIntent.cs`, `WorkspaceHostCore.cs` y la UI (`git diff --stat bbd9a35e c3ae1c97 -- src`).
- Por tanto, `DraftSession.cs`, `PushBackPilotUpdatePlan.cs` y `PushBackPilotMaterializer.cs` están como los dejó T4A1, y los menores M-1, M-5, M-6 y M-10 siguen abiertos.

**Método:** solo lectura (Read/Grep y `git show|grep|ls-tree|diff --stat`). No se compiló ni se ejecutaron pruebas. Todas las rutas son relativas a la raíz del repositorio.

**Aviso de deriva de líneas:** el paquete de T4 cita líneas anteriores a `bbd9a35e`, y T4A1 desplazó el archivo 41 líneas:
- `DraftSession.cs:471-474` del paquete (rechazos de `TryBeginApply` y `AnotherUpdateInProgress`) está hoy en **`:511-515`**. Lo verifica `git show 443b5b3a:…DraftSession.cs`, con las mismas sentencias en 470-474.
- `WorkspaceHostCore.cs:861-911` (`ViewOfPilotDraft`) está hoy en **`:1154-1217`**.
- `WorkspaceDraftApplyTests.cs:701-716` está hoy en **`:702-717`**.

---

## 1. La ranura de aplicación de `DraftSession`

### Estado del borrador

**H-1. Campos privados.** `_initial` y `_current` (`DraftSession.cs:249-250`), `_holdsApplySlot` (`:252`), `_deferredErrorReason` (`:253`), `_explicitStatus` (`:254`) y `_statusBeforeAttempt` (`:255`). El copiador inyectado es `_copy` (`:243`).

**H-2. Superficie pública:**
- `StaleBase` tiene `private set` (`:281`).
- `InitialDraftState` devuelve **una copia** (`:284`).
- `CurrentDraftState` devuelve **la referencia**, que se edita en sitio (`:287`).
- `PendingText` es una instantánea ordenada (`:293`).
- `IsDirty` es «pendiente o comparador distinto» (`:298`).
- `Status` es el explícito, o si no lo hay Dirty o Clean (`:300`).
- `IsPossiblyStale` (`:303`), `UnknownOrigin` (`:312`), `ConflictDifferences` (`:315`), `LastApplyCause` (`:318`), `Warnings` (`:321`), `ErrorReason` (`:323`) y `DeferredErrorReason` (`:329`).
- `StatusBeforeAttempt` (`:662`).
- `CanEdit` solo deja editar sin estado explícito o en Conflict (`:331-332`).

### `TryBeginApply` (`:508-524`), en este orden

1. NotRegistered (`:511`).
2. `InvalidStatus` si no es Clean ni Dirty (`:512-513`). **Clean se acepta:** «NothingToApply antes de la ranura» es solo disciplina del llamador.
3. `PendingText` (`:514`).
4. **`AnotherUpdateInProgress`** si `_registry.TryAcquireApplySlot` falla (`:515`).

Si arranca:
- toma la ranura;
- limpia `_deferredErrorReason`;
- guarda `_statusBeforeAttempt`;
- pone `Applying`;
- y vacía `LastApplyCause` y `Warnings` (`:517-523`).

**No toca** `IsPossiblyStale`, la base, el inicial, el actual ni el pendiente.

**H-3. `NothingToApply` no es un rechazo del núcleo.** Es un resultado de `Prepare` (`PushBackPilotUpdatePlan.cs:82-83`, `:306`). El enumerado de rechazos tiene exactamente None, NotRegistered, InvalidStatus, PendingText y AnotherUpdateInProgress (`DraftApplyOutcome.cs:108-123`).

**H-4. La ranura es única por registro** y pertenece a la operación:
- `_applying` (`DraftRegistry.cs:130`);
- `TryAcquireApplySlot` y `ReleaseApplySlot` (`:356-367`);
- `HasUpdateInProgress` es público (`:198`).

**Ni `Detach` ni `EnterError` liberan la ranura** (`DraftSession.cs:690-697`, `:671-686`). Lo fija `TheApplyingSlotIsNotFreedByErrorDiscardDiscardAllOrRetireWhileTheOperationIsInFlight` (`WorkspaceDraftApplyTests.cs:128`).

### `CompleteApply` (`DraftSession.cs:531-554`)

**H-5. Recorrido:**
1. Un resultado nulo lanza (`:533`).
2. Sin ranura responde `NotApplying` y no cambia nada (`:534-535`).
3. **Libera la ranura antes de clasificar** (`:537-538`).
4. Si el borrador está desregistrado responde `ClosedDetached` y no cambia nada más (`:539-540`).
5. Pone `_explicitStatus = null` (`:542`).
6. Llama a `Classify` (`:543`).
7. Si había un `EnterError` diferido, va a Error **conservando** lo que resultó (`:544-553`). La prueba es `WorkspaceDraftApplyTests.cs:261`.

**H-6. `Classify` (`:556-606`), por resultado:**

| Resultado | Estado | Base, inicial, actual y pendiente | Otros |
|---|---|---|---|
| `NotAppliedConflict` | Conflict | sin cambios | `ConflictDifferences := outcome.Differences`, `LastApplyCause := outcome.Cause`, que es **null** porque la fábrica no lleva causa (`DraftApplyOutcome.cs:73`) (`:560-564`) |
| `NotAppliedAdmission`, `NotAppliedPreflight` y `NotAppliedFailure` | explícito null: vuelve a Clean o Dirty derivado | sin cambios | `LastApplyCause := cause` (`:566-570`) |
| `CommitThrew` | Unknown(CommitThrew) | sin cambios | causa (`:572-576`) |
| `Applied` o `AppliedWithWarnings` con `PostRead` nulo o no admitido | Unknown(PostReread); la clase devuelta es `AppliedPostRereadFailed` | sin cambios | causa := diagnóstico de la admisión (`:581-589`) |
| `Applied` o `AppliedWithWarnings` admitido | Clean | `Rebase`: base := la de POST, `_initial` := copia, `_current` := copia de `PostRead.InitialState`, **pendiente vaciado** (`:591-595`) | `Rebase` limpia `IsPossiblyStale`, el explícito, `UnknownOrigin`, `ConflictDifferences`, `LastApplyCause`, `Warnings` y `ErrorReason` (`:487-498`); con avisos, `Warnings := outcome.Warnings` (`:596`) |
| `AppliedPostRereadFailed` y cualquier otro | Unknown(PostReread) | sin cambios | causa (`:600-604`) |

Consecuencia para R-T4-04:
- `CompleteApply(Applied(postRead))` ya hace hoy `CurrentDraftState := InitialDraftState`, con el pendiente vacío y Dirty en false desde el estado releído.
- **No compara** con ninguna expectativa de POST. La comparación byte a byte tiene que hacerla el llamador antes, y elegir entre `Applied` y `AppliedPostRereadFailed`.

**H-7. Las fábricas** (`DraftApplyOutcome.cs:69-105`):
- `Applied` y `AppliedWithWarnings` exigen un `postRead` no nulo (`:87-99`);
- las fábricas con causa exigen una causa no vacía (`:101-105`);
- `AppliedPostRereadFailed(string cause)` **solo lleva causa, no avisos** (`:84`);
- las ocho clases las fija `TheOutcomeKindsAreExactlyTheEightOfTheTable` (`WorkspaceDraftApplyTests.cs:591`).

### Robustez ante un copiador que lanza (F4-E25 (4), `f4-dispositions.md:456-458`)

**H-8. En `Applied`, las dos copias van después de liberar la ranura y de poner el explícito a null** (`DraftSession.cs:591-592` frente a `:537-542`). Si el copiador lanza:
- la excepción sale de `CompleteApply`;
- la ranura ya está libre;
- la base, el inicial y el actual siguen como antes del intento;
- el estado pasa a **Clean o Dirty derivado sobre la base anterior** (ni Unknown ni Applying);
- `_deferredErrorReason` queda sin aplicar y sin limpiar;
- y no se devuelve ningún `DraftApplyResult`.

La prueba vigente `AReloadWhoseCopierThrowsLeavesTheStaleBaseTheInitialStateAndTheCurrentStateExactlyAsTheyWere`, en sus filas `applied` y `warnings` (`WorkspaceDraftPilotOwnershipTests.cs:417-458`):
- **exige que se lance** (`Assert.Throws`, `:444-450`);
- y deliberadamente **no** comprueba el estado (`:457` solo lo hace para `discard` y `keep`).

Corregir (4) cambia esa prueba: es un cambio declarado.

**H-9. Una segunda vía de excepción tras liberar la ranura:**
- todas las ramas de `Classify` devuelven `DraftApplyResult.Closed(…, Status, …)` (`:564`, `:570`, `:576`, `:588`, `:597`, `:604`);
- con el explícito a null, `Status` llama al comparador (`:298-300`);
- y `ClosedDetached` también evalúa `Status` (`:540`).

Un comparador que lanza escapa de `CompleteApply` con la ranura ya liberada. Lo mismo vale para el llamador en un `finally`: el arrastre de F4-E16 dice «`CompleteApply` en `finally`» (`f4-dispositions.md:61`, `:231`).

### `CheckNow`, `ConfirmBase` y `EnterError`

**H-10. `CheckNow`** (`:612-630`):
- solo sirve Unknown(CommitThrew);
- con MATCH recupera el estado de antes del intento;
- con diferencias pasa a Conflict.

**`ConfirmBase`** (`:641-659`):
- sirve Clean, Dirty y Conflict;
- con MATCH solo limpia `IsPossiblyStale` si no hay estado explícito;
- con diferencias pasa a Conflict.

**`EnterError` bajo Applying** solo anota `_deferredErrorReason` (`:676-680`).

**H-11. El XML de la sobrecarga atómica de «Conservar»** (`:460-466`) promete «if one of them throws the draft stays exactly as it was». Pero:
- la llamada es `_copy(keepOn(_copy(read.InitialState!), _current))` (`:480`), que pasa `_current` **por referencia** a `keepOn`;
- la precondición «keepOn no muta sus argumentos» (menor M-10) no está escrita;
- la función de producción, `KeepPilotEditsOn`, es pura (`PushBackPilotMaterializer.cs:242-251`).

---

## 2. El PREPARE puro y el plan inmutable

### `PushBackPilotPrepareInput` (`PushBackPilotUpdatePlan.cs:19-71`)

**H-12.** Tiene constructor privado (`:21`) y una sola fábrica, `From(DraftSession<PushBackPilotDraftState>)` (`:58-70`). Toma:
- `RackId` y `StaleBase` por referencia (inmutable);
- `InitialDraftState`, ya una copia del borrador (`:65`);
- `CurrentDraftState`, copiado con `PushBackPilotStateComparer.Copy` (`:66`): el copiador del piloto directamente, no el `_copy` inyectado; en producción son el mismo (`WorkspaceHostCore.cs:337`);
- `HasPendingText` e `IsDirty`;
- y `ConsideredStatus`, que bajo Applying es `StatusBeforeAttempt` (`:69`, N-7).

### `Prepare` (`:297-322`)

**H-13. Orden de evaluación:**
1. `input` nulo da Invalid(`MissingInputReason`) (`:301`).
2. NotApplicable (`:303-304`).
3. PendingText (`:305`).
4. NothingToApply (`:306`).
5. **`reread == null || catalog == null` da Invalid(`MissingInputReason`)** (`:308`), antes de la comparación: es el menor M-6.
6. Conflict, con `input.StaleBase.CompareWith(reread.StaleBase)` (`:310-311`).
7. NotAdmitted (`:313-314`).
8. `Build` (`:316`).

Cualquier excepción da Invalid(`PrepareFailurePrefix + mensaje`) (`:318-321`).

Hecho útil para T4B: los pasos 2 a 4 no miran ni la relectura ni el catálogo. `Prepare(From(draft), null, null)` clasifica NotApplicable, PendingText y NothingToApply **sin leer**. Para un Dirty da Invalid por `:308`. Lo prueban `ACleanDraftHasNothingToApplyAndTheRereadIsNotLookedAt` (`WorkspacePilotPrepareTests.cs:1466-1485`, con `null!` como catálogo y la nota «N-8: never takes the slot») y `PrepareIsTotalAMissingPieceIsInvalidAndNeverAnException` (`:1795-1816`).

### `Build` (`:325-410`)

**H-14. Los casos Invalid:**
- **(a)** `ComposeForWrite(input.InitialDraftState, input.CurrentDraftState, catalog)` (`:328-329`).
- Los miembros son los **Mutable de la relectura**, en orden ordinal, cada uno con su sobre releído deserializado. Un sobre ilegible da Invalid(`UnreadableViewPrefix + key`) (`:332-339`).
- **(b)** Exactamente un miembro con `EnvelopeText` igual, byte a byte, a `initial.Origin` (`:342-345`).
- **(c)** `PreflightInnerSources`, con el proyecto del origen como iniciador (`:348-357`).
- **(d)** El ensamblador, el resolver y `Cortes` dentro de un `try`, con `SystemFailurePrefix` (`:360-372`).
- **(e)** La disponibilidad, con `RackViewCodec.Decode` y `RackViewAvailability.Evaluate` (`:379-381`).

**Menor M-5 (`:379-381`):** un descriptor que no se decodifica da el mismo texto que un fantasma, `ViewUnavailablePrefix + key + ViewUnavailableSuffix` («El piloto no borra vistas y la vista … ya no está disponible …», `:285-287`). Lo fija la fila `corrupt-descriptor` de `WorkspacePilotPrepareTests.cs:1758` con `Assert.StartsWith(ViewUnavailablePrefix + "B2")` (`:1772`). Corregir M-5 cambia esa aserción.

**H-15. La vista y la sección salientes**, como en RACKEDITAR (`:385-399`):
- Planta con `-1`;
- Frontal con `embed.Section`;
- Lateral con `decoded.Address.Variant.Index`.

**El texto del diseño** es `store.Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(preflight.ResolvedSources[index]))` (`:401`).

**El sobre** sale de `ComposeMemberEnvelope(embed, view, section, designText)` (`:402`), que es la única composición del archivo (`:414-418`): parte de `source`, con `source.Id` y `source.Name`.

### Los tipos del plan

**H-16. `PushBackPilotUpdatePlan`** (`:208-260`). Sus campos (`:228-245`):
- `RackId`, `OriginKey` y `AnnotationScale`;
- `ExpectedBase`: las entradas de `input.StaleBase.Entries`, en orden ordinal (`:406`);
- **`ExpectedVariableRegistryText`** (`:214`, `:237`): es M-7, aceptado en F4-E53;
- `Members`, una lista de `PushBackPilotMemberPlan(Key, View, Section, DesignText, EnvelopeText)` (`:144-178`);
- `PostExpectation`, que es `PushBackPilotPostExpectation(AnnotationScale, Envelopes: [Key, Text])`, por clave y con los textos de los sobres salientes (`:181-200`, `:407`).

La igualdad es explícita y ordinal (`:247-255`). Los hash solo mezclan cuentas (`:199`, `:259`): es D-2.

**H-17. Lo que el plan NO lleva:**
- ningún `ObjectId`, handle numérico ni cuenta de referencias: el «destino» es solo la **clave** de definición, que es el handle en texto del barrido (`RackSiblingScan.cs:122`);
- ningún valor de supervivientes (H-31);
- ningún catálogo ni estado del borrador.

**H-18. `ExpectedBase` pierde la información de ambigüedad de la base**, porque `Entries` excluye las claves ambiguas (`DraftStaleBase.cs:112-113`). Es benigno en la práctica:
- un Ready implica MATCH, y `CompareWith` da una diferencia de Membership ante cualquier ambigüedad (`:151-155`);
- además, el barrido del piloto no puede repetir claves (`PushBackPilotIntentScan.cs:47-48`).

Así, `DraftStaleBase.Create(plan.ExpectedBase, plan.ExpectedVariableRegistryText)` (`DraftStaleBase.cs:80`, pública) reconstruye una base equivalente para la comparación de MUTATE.

**H-19. `ExpectedVariableRegistryText` siempre es null en el piloto.** `BuildStaleBase` y `Analyze` crean la base con `DraftStaleBase.Create(entries)`, sin registro (`PushBackPilotIntentReader.cs:145`; comentario en `:62`: «the variable registry is not read»). La comparación del registro en MUTATE es hoy vacía: hay que declararlo, no ignorarlo.

### `ComposeForWrite` (`PushBackPilotMaterializer.cs:196-234`)

**H-20.** El cuerpo:
- **(6)** rechaza NaN, ±∞, 0 y los negativos (`:205-208`);
- **(1)** copia el **inicial** y le pone la escala del actual (`:212-213`), y compara con `current` tal cual (`:214-215`);
- vuelve a materializar desde `initial.Origin` (`:217-219`);
- **(3)** exige `initial == fresh.State` con el comparador del piloto (`:221-224`);
- y aplica la escala (`:227`).

**El menor M-1 sigue abierto.** No hay ninguna guarda de fuente sobre este cuerpo: `git grep` no encuentra `Copy(current)` ni `neutralInitial` en `tests/`. La prueba `CarryOver1_…` (`WorkspaceDraftPilotMaterializationTests.cs:767`) no distingue la forma antigua de la nueva.

**H-21. `ComposeForWrite` tiene un solo llamador**, el planificador, y solo después de `CompareWith`. Lo fija `ComposeForWriteIsCalledOnlyByThePlannerAndOnlyAfterTheComparisonOfTheBase` (`WorkspacePilotPrepareGuardTests.cs:156-190`).

---

## 3. La vía de lectura y la StaleBase

**H-22. `DraftStaleBase`** (`DraftStaleBase.cs:57-170`):
- `DraftStaleBaseEntry(Key, Membership, View, Section, EnvelopeText)`, **sin cuenta de referencias, por diseño** (INV-29, `:21-25`);
- `Create` es pública e independiente del orden; descarta NotMember y anula el texto de los Blocking (`:80-110`);
- `CompareWith` trabaja sobre la unión de claves, ordinal y sin normalizar, con el registro al final (`:141-169`);
- no hay operador de igualdad: la equivalencia se mide con `CompareWith(...).IsMatch` en los dos sentidos.

**H-23. `BuildStaleBase(scan)`** es `Analyze(scan).StaleBase` (`PushBackPilotIntentReader.cs:64-68`). **`Read(scan, catalog)`** usa el mismo `Analyze` (`:79`), y su `StaleBase` y la de su `DraftIntentRead` son la misma instancia (`:93-95`).

`Analyze` (`:113-146`):
- un `RackId` en blanco da una base vacía (`:116-117`);
- llama a `RackSiblingMembership.Classify(scan.Facts, scan.RackId, null, false)` (`:119`);
- Redraw y Erase pasan a **Mutable** (`:131-134`), ReadOnly a ReadOnly y BlockingUnreadable a Blocking sin texto (`:135-140`).

La igualdad entre `Read(...).StaleBase` y `BuildStaleBase(...)` la fija `TheBaseOfTheResultIsTheOneOfBuildStaleBaseWhetherOrNotTheReadIsAdmitted` (`WorkspaceDraftPilotIntentStaleBaseTests.cs:284-297`).

**H-24. La entrada pura `PushBackPilotIntentScan`** (`PushBackPilotIntentScan.cs:24-99`):
- copia lo que recibe;
- **lanza si algún hecho trae `IsSelectedSource`** (`:51-52`): la lectura caller-owned debe barrer sin origen seleccionado;
- exige `CompletenessEvidence` no vacía (`:68-69`), que solo se comprueba como no vacía en AUTH-13 (`RackAuthoredComparator.FourKinds.cs:52`) y no afecta a la base.

**H-25. Hoy existe un solo camino de lectura de producción**, el de las seis lecturas del host:
1. `WorkspaceHostCore` llama a `_editIntentReader!.ReadIntent(...)` (`WorkspaceHostCore.cs:313`, `:394`, `:472`, `:556`, `:638`, `:719`).
2. `WorkspaceNavigationHost.PilotIntentReader.ReadOnActiveDocument` (`src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:220-256`) hace, en orden:
   - **la puerta de reposo** (`CommandInProgress` o `!IsQuiescent` da Busy, `:225-226`);
   - el catálogo, fuera del bloqueo (`:228-230`);
   - `using LockDocument` (`:232`);
   - la autorización (`:234-236`);
   - la revalidación y el origen (`:238-240`);
   - y `RackPilotIntentAdapter.Read` (`:242-248`).
3. `RackPilotIntentAdapter.Read` (`src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:94-142`) llama a `RackSiblingScan.CaptureDrawing(document)` (`:116`), construye el `PushBackPilotIntentScan` con la evidencia `"RackSiblingScan/CaptureDrawing"` (`:71`, `:120`) y llama a `PushBackPilotIntentReader.Read` (`:121`).
4. `CaptureDrawing` es `Traverse(document, ObjectId.Null, null)` (`src/RackCad.Plugin/Views/RackSiblingScan.cs:95-96`), con su **propio** `LockDocument` y `StartTransaction` (`:112-113`) y su `Commit()` (`:163`).

**El catálogo se carga en el Plugin y no vuelve al host:** `EditIntentReadResult` solo lleva `Status`, `Reason` y `PilotRead` (`WorkspaceEditIntent.cs:34-63`).

**H-26. `PushBackPilotIntentResult`** (`PushBackPilotIntentResult.cs:25-60`):
- **siempre** lleva `StaleBase`, también sin admisión;
- `Read` es un `DraftIntentRead`;
- `OriginKey`, `Detail` y `ClassicEditorHint` (este solo sin admisión);
- y `PreviewWitness`.

### Lo que haría falta para R-T4-01 y F4-E26, medido

**H-27. Del lado de Application, la entrada pura ya basta.** Una lectura caller-owned que produzca los mismos `Facts` y `RawEnvelopes` y construya `PushBackPilotIntentScan(rackId, originKey, facts, raw, evidence)` obtiene la misma `DraftStaleBase` con `BuildStaleBase` o `Read`.

Lo que falta es del Plugin: una variante de `Traverse` que reciba la transacción del llamador, sin bloqueo, transacción ni `Commit`. Las guardas de cuenta del barrido la restringen:
- 1 `rawEnvelopes.Add(key, json)`, 1 `RackBlockData.Read(` y 1 `new ReadOnlyDictionary…(rawEnvelopes)` (`WorkspaceEditIntentGuardTests.cs:79-85`);
- 1 `new RackSiblingDrawingScan(` y 1 `RawEnvelopes =` (`:109-113`);
- las firmas exactas de `Capture`, `CaptureDrawing` y `Traverse` (`:93-99`);
- 1 `.Commit(` en el archivo (`:121-128`).

La forma que mantiene F4-E26 **por construcción** es un único núcleo de recorrido sobre una transacción, llamado por `CaptureDrawing` (con su bloqueo y su transacción) y por la variante caller-owned. Las firmas y cuentas de esas guardas cambian: es un cambio declarado.

**H-28. Qué de la equivalencia es comprobable en Core:**
- la capa pura: el mismo escaneo da la misma base, ya fijado (H-23);
- y por guarda de fuente, que los tres caminos (Comprobar ahora, O-04 en contexto de comando y caller-owned) pasan por el mismo núcleo de recorrido, la misma construcción de `PushBackPilotIntentScan` y `PushBackPilotIntentReader.BuildStaleBase` o `Read`.

La equivalencia «para el mismo dibujo» solo se puede medir en el host (Smoke-2).

**H-29. Los dobles del lector en pruebas:**
- `EditIntentFakeReader` registra las llamadas y responde con `EditIntentKit.Admitted`, la lectura real del piloto sobre un escaneo en memoria con `PilotKit.Catalog` (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs:17-61`);
- `PilotEditAuditedReader` cuenta las lecturas y tiene un `InsideRead` (`tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs:37-56`);
- el catálogo real de las pruebas es `JsonRackCatalogProvider.FromBaseDirectory().Load()` (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs:32-34`);
- los kits son `IntentKit` (`WorkspaceDraftPilotIntentStaleBaseTests.cs:21`), `PrepareKit` (`WorkspacePilotPrepareTests.cs:22`), `DraftKit` (`WorkspaceDraftSessionTests.cs:134`) y `WorkspaceHostRig` (`WorkspaceHostTestSupport.cs:110`).

---

## 4. El host del Workspace

**H-30. Operaciones públicas del host core sobre el piloto, hoy:**

| Operación | Ubicación | Lectura | Notas |
|---|---|---|---|
| `TryBeginPilotEdit` | `WorkspaceHostCore.cs:282-360` | sí | |
| `TryCheckPilotEdit` | `:367-430` | sí | solo Unknown(CommitThrew), `:390`; `CheckNow` en `:412`; su Conflict **no** lleva el diagnóstico de admisión, con la limitación de F4-E54 asignada a T4B y la nota de F4-E55 |
| `TryReloadPilotEdit` | `:440-514` | sí | |
| `TryConfirmPilotBase` | `:528-595` | sí | |
| `TryKeepPilotEdit` | `:602-680` | sí | Waiting en Applying, `:633` |
| `TryDiscardAndReloadPilotEdit` | `:687-761` | sí | Waiting en Applying, `:716` |
| `DiscardPilotEdits(SessionId)` | `:769-785` | no | |
| `NotePilotEdit(SessionId)` | `:792` | no | `_knownClean.Remove` |

**Las implementaciones explícitas del puerto** (`:802-1136`):
- las seis lecturas, cada una con `_pilotReadInProgress` en `try`/`finally` (`:802-886`);
- `GetField`, `SetPendingText`, `CommitField`, `CrossBoundary`, `DiscardRack`, `DiscardDocument`, `GetRacksAtRisk`, `TakeLossNotices` y `TakeCloseNotices`.

**`CrossBoundary` solo conoce** TabChange y RackChange, que bloquean, y los cuatro tipos que no bloquean. Cualquier otro valor da Aborted con «La frontera no se conoce.» (`:969-983`), y `PilotBoundaryKind` **no tiene `Update`** (`IWorkspacePilotEditPort.cs:167-175`). En el núcleo, `DraftBoundary.Update` existe y bloquea (`DraftSession.cs:80`, `:90-91`).

**H-31. No existe ninguna operación `TryUpdate`/`Apply`, ni ningún llamador de `TryBeginApply` o `CompleteApply` en `src/`.** Los únicos llamadores están en pruebas.

**Tampoco existe la marca de host «actualización en curso» de R-T4-07:**
- `DiscardRack` llama a `_drafts.DiscardByUser` sin mirar Applying (`WorkspaceHostCore.cs:1025`);
- `DiscardDocument` llama a `DiscardPilotEdits`, que llama a `DiscardAllByUser` (`:1046`, `:776`);
- las dos operaciones del registro desligan aunque el borrador esté en Applying (`DraftRegistry.cs:262-286`);
- la guarda prohíbe que esas operaciones del registro nombren la ranura (`WorkspaceLifecycleGuardTests.cs:698-706`), así que la comprobación de R-T4-07 va en el host, por ejemplo con `draft.Status == Applying` o con `_drafts.HasUpdateInProgress`;
- `EditDiscardStatus` es { Rejected, Discarded, Failed, Busy } (`WorkspaceEditIntent.cs:380-393`), sin un valor propio para Applying.

**H-32. La compuerta `PassGate`** (`WorkspaceHostCore.cs:1340-1374`):
- sin lector, Unavailable;
- sin documento, Waiting;
- con un modal RackCad, Waiting;
- **sin reposo, Waiting** (`:1361-1365`);
- y si la autorización falla, Rejected.

En contexto de comando (C-F4-02), la compuerta y el lector del Plugin responden Waiting o Busy (H-25). R-T4-06 pide otra vía sin la puerta de reposo.

**H-33. Dónde podría entrar un `Update`, según las guardas vigentes:**
- **Puerto:** las 15 operaciones son una **cota inferior** (`WorkspacePilotEditGuardTests.cs:100-102`). La autoprueba `ThePortGuardAllowsWhatTheNextTaskWouldAddAnUpdateAtTheEndOfTheKindsAndItsOperation` ya admite `Update` al final de `PilotBoundaryKind` y una operación `PilotFieldResult Update(WorkspaceRequest origin, string rackId)` (`:152-160`).
- **Host:** `TheHostGuardAllowsWhatTheNextTaskWouldAddAnUpdateOperationThatWrites` (`:354-362`) inyecta un `IWorkspacePilotEditPort.Update` con `StartTransaction`, `Commit` y `AppendEntity` y comprueba que `HostViolations` y `WriteViolations` no lo ven.
  - **Pero** la guarda de archivo entero `TheFilesOfTheLifecycleWriteNothingAndHoldNoObjectNorTransactionOfTheHost` prohíbe `StartTransaction`, `LockDocument`, `.Commit(`, `RackBlockData.Write`, `AppendEntity` y similares en **todo** `WorkspaceHostCore.cs`, `WorkspaceController.cs`, `WorkspaceEditIntent.cs`, `DraftRegistry.cs` y en otros archivos (`WorkspaceLifecycleGuardTests.cs:667-683`).
  - Y `TheThreeFilesOfTheApplicationAre…` prohíbe `Transaction` y `\bCommit\s*\(` **incluso en comentarios** en `WorkspaceHostCore.cs`, `IWorkspaceEditIntentReader.cs` y `WorkspaceEditIntent.cs` (`WorkspaceEditIntentGuardTests.cs:1002-1017`).
  - **Conclusión medida:** un `Update` del host solo puede delegar en un puerto. La MUTATE no cabe en el host core ni en el controlador.
- **Drafts:** el registro solo vive en el host core:
  - una sola `new DraftRegistry`, por guarda (`WorkspaceEditIntentGuardTests.cs:548-550`);
  - `OpenDrafts` es `internal` (`WorkspaceHostCore.cs:265`);
  - y `InternalsVisibleTo` solo incluye `RackCad.Tests` (`src/RackCad.Application/RackCad.Application.csproj:11`).

  Por tanto, **`TryBeginApply` y `CompleteApply` solo se pueden orquestar desde el host core**, o desde código de Application al que el host entregue el borrador.
- **El lector:** `IWorkspaceEditIntentReader` debe tener exactamente un método, `ReadIntent`, y no nombrar `Draft`, `Navigation`, `PushBackPilot`, `Document` ni `Autodesk` (`WorkspaceEditIntentGuardTests.cs:1020-1027`).
- **Los archivos de `Application/Workspace/`:** solo `WorkspaceHostCore.cs`, `IWorkspaceEditIntentReader.cs` y `WorkspaceEditIntent.cs` pueden nombrar `PushBackPilot*` (`:512-523`).
- **`Plugin/Workspace/`:** tiene exactamente 9 archivos y ninguno con «Intent» en el nombre (`:989`, `:991`).
- **Drafts:** el conjunto **exacto** de archivos de `Drafts/` lo fija `WorkspaceDraftBoundaryTests.cs:46`, así que T4B no puede añadir un archivo ahí.

**H-34. La marca «conocido limpio»:**
- `_knownClean` (`WorkspaceHostCore.cs:66`);
- `NotePilotEdit` (`:792`);
- la cota inferior de `_knownClean.Remove(` es 12 (`WorkspaceLifecycleGuardTests.cs:449`);
- `PortMutatingOperations` tiene 8 cuerpos (`:463-474`);
- `ChangesADraft` **no** reconoce `.TryBeginApply(` ni `.CompleteApply(` (`:476`).

Un `Update` del host tiene que entrar en esa lista, y el patrón tiene que ampliarse (R-T4-07: «Actualizar llama a `NotePilotEdit`»).

**H-35. La vista del campo** (`ViewOfPilotDraft`, `WorkspaceHostCore.cs:1154-1217`):
- Applying da `PilotFieldState.Applying` con `canEdit = false` (`:1183-1184`, `:1226`);
- `unknownOrigin`, `conflictText` y `lastCause` se rellenan (`:1206-1216`);
- **`Warnings` no viaja en `PilotFieldView`** (`IWorkspacePilotEditPort.cs:197-210`).

**H-36. Los dobles del puerto** que habría que extender si el puerto gana operaciones:
- `CommandAdvisorPortDouble` (`tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs:17`);
- `ForwardingRiskPort` (`:1107`);
- `WorkspacePilotEditPortDouble`, de la UI (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:25`, 356 líneas; `Reads()` ya cuenta las seis lecturas en `:65-67`, así que M-9 está cerrado);
- la compuerta, que reenvía (`WorkspacePilotSupport.cs:153-204`);
- y el adaptador del controlador, que solo reenvía (`src/RackCad.Plugin/Workspace/WorkspaceController.cs:278-317`).

**H-37. Las clases de prueba de Core en `tests/RackCad.Tests/Workspace/`** que tocan este núcleo. Las cuentas son métodos `[Fact]`/`[Theory]`, sin contar las filas `InlineData`:

| Archivo | Métodos | Qué fija |
|---|---|---|
| `WorkspaceDraftApplyTests.cs` | 37 | la ranura, `CompleteApply` y `CheckNow` |
| `WorkspaceDraftSessionTests.cs` | 55 | el borrador y `DraftKit` |
| `WorkspaceDraftRegistryTests.cs` | 32 | el registro y la exención por nombre de `DraftStaleBase` |
| `WorkspaceDraftRegistryContainmentTests.cs` | 19 | |
| `WorkspaceDraftBoundaryTests.cs` | 33 | el conjunto exacto de `Drafts/` y la pureza |
| `WorkspaceDraftStaleBaseTests.cs` | 39 | |
| `WorkspaceDraftPilotIntentStaleBaseTests.cs` | 22 | |
| `WorkspaceDraftPilotIntentReadTests.cs` | 63 | |
| `WorkspaceDraftPilotIntentGuardTests.cs` | 13 | la pureza del lector |
| `WorkspaceDraftPilotMaterializationTests.cs` | 41 | `LegacyDeltas` y los arrastres |
| `WorkspaceDraftPilotOwnershipTests.cs` | 15 | el copiador que lanza |
| `WorkspaceDraftPilotStateTests.cs` | 27 | `PilotKit` |
| `WorkspacePilotPrepareTests.cs` | 66 | el PREPARE, el plan y la reconciliación |
| `WorkspacePilotPrepareGuardTests.cs` | 17 | |
| `WorkspaceEditIntentHostTests.cs` | 58 | |
| `WorkspaceEditIntentGuardTests.cs` | 52 | |
| `WorkspacePilotEditHostTests.cs` | 59 | |
| `WorkspacePilotEditGuardTests.cs` | 34 | |
| `WorkspaceLifecycleHostTests.cs` | 69 | |
| `WorkspaceLifecycleGuardTests.cs` | 42 | |
| `WorkspaceHostHardeningGuardTests.cs` | 34 | |
| `WorkspacePilotReconciliationHostTests.cs` | 20 | de T4A2 |
| `WorkspacePilotReconciliationGuardTests.cs` | 22 | de T4A2 |
| `WorkspaceCommandAdvisorTests.cs` | 54 | |

**La línea base de CI** del último GREEN (F4-E56) es: filtro de Core 2923/2923 y filtro de UI 601/601 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:1927`).

---

## 5. C-F4-05 (`eraseByKind` nulo) y `LegacyDeltas`

**H-38. `eraseByKind` en el código.**
- Es un `Func<RackEmbedDocument, bool>` que el barrido aplica por hecho: `envelope != null && eraseByKind != null && eraseByKind(envelope)` (`src/RackCad.Plugin/Views/RackSiblingScan.cs:152`).
- `CaptureDrawing` pasa **`null`** (`:96`), así que en el piloto `EraseByKind` es siempre false.
- `Classify` convierte `EraseByKind` en Erase y su falta en Redraw (`src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:125-128`, `:146-148`).
- **En el piloto nunca hay miembros Erase.** Es coherente con la regla del plan de no borrar nunca una vista (H-14 (e); `WorkspacePilotPrepareGuardTests.cs:246`).

**H-39. Los supervivientes y H-06.**
- `IsSurvivor` es «Redraw y `LayoutReferenceCount >= 1`» (`RackSiblingMembership.cs:64`).
- `RackSiblingRedrawPlan.Create` da `DeferToFirstPlacement` solo con `survivors == 0 && erase.Count != 0` (`src/RackCad.Application/Views/Redraw/RackSiblingRedrawPlan.cs:91-95`).

Con `eraseByKind` nulo esa rama es **inalcanzable**: un rack con miembros Redraw y cero supervivientes daría `MutateNow` en la costura de I-55. Por eso C-F4-05 («No aplicado, con su causa visible en T4B y sin escrituras», `docs/initiatives/I-64-f4-gate-contract-package.md:61`) **exige una comprobación explícita propia**, y los datos para hacerla son:
- **no** están en la base ni en el plan: `DraftStaleBaseEntry` no lleva cuentas por diseño (INV-29, `DraftStaleBase.cs:21-25`);
- **sí** están en los hechos del barrido: `RackSiblingScanFact.LayoutReferenceCount` (`RackSiblingMembership.cs:25`, `:48`), que llegan a Application dentro de `PushBackPilotIntentScan.Facts` (`PushBackPilotIntentScan.cs:89`).

RACKEDITAR Push Back cuenta los supervivientes por bloques, con el mensaje «no se elimino el ultimo vinculo» (`src/RackCad.Plugin/RackPushBackCommands.cs:348-356`). La diferencia se declara (C-F4-05).

**H-40. `RackSiblingRedrawRun.Execute`:**
- convierte toda excepción de `Mutate` en `Discarded` (`src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs:92-100`): es H-02 de F4 y lo que INV-30 corrige;
- exige un `RackSiblingMembershipSnapshot` y unidades `RackSiblingPreparedUnit<TUnit>` por miembro (`:30-90`);
- y los fallos de `Post` se reducen a `Committed` con el diagnóstico `POST_FAILED` (`:110-118`).

El plan del piloto son textos por clave, no unidades por miembro. Reutilizar la costura exige un adaptador o una frontera de `Commit()` aditiva (INV-30, «sin cambiar la costura para Insertar»).

**H-41. `LegacyDeltas` (decisión 3 del Owner).**
- No es código de producto: es un diccionario de **pruebas** (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs:706-731`).
- Su comentario: «The exact canonicalization an older drawing gets when the pilot writes it (T2H-07)».
- Tiene seis fixtures antiguos, `pb-10` a `pb-15`, con sus deltas JSON exactos (por ejemplo `FirstLevelDatum: <absent> -> 1` o `LegacyHighEndBeamPeralte: 5 -> 3.5`).
- Lo comprueban tres pruebas:
  - `EveryLegacyFixtureHasItsExpectedDelta` (`:735-738`);
  - `AnOlderDrawingComesBackWithExactlyTheCanonicalizationOfTheApplicationPath` (`:741-749`);
  - `EditingTheScaleOfAnOlderDrawingAddsOnlyTheEditedFieldToTheCanonicalization` (`:751-762`).
- Las tres pasan por `RoundTrip`, que hace `Admitted`, copia, `WriteFrom(initial, current)` (`:654-660`).
- **Significado:** al escribir un dibujo antiguo, el texto saliente del plan (`DesignText` y `EnvelopeText`, H-15) lleva esa canonicalización además de la escala. Se acepta y se declara en T4B (`docs/initiatives/I-64-f4-t4-gate-contract-package.md:182`, `:538`).
- **Consecuencia para POST:** la expectativa de POST ya incluye la canonicalización, porque es el texto compuesto. El byte a byte compara contra el texto canonicalizado, no contra el original del dibujo.

---

## 6. Pruebas y guardas que fijan hoy este núcleo, y que T4B mantiene o amplía

**H-42. `WorkspacePilotPrepareGuardTests.cs`:**
- `Forbidden` (`:30-52`) prohíbe en el archivo del plan las transacciones, los bloqueos, `Commit`, `Database`, `Autodesk`, el Plugin y la UI, el host (`IWorkspace*`, `WorkspaceHost*`, `EditIntent*`), **toda mutación de un borrador, incluidos `TryBeginApply(` y `CompleteApply(`** (`:47`), `async`, `File` y la reflexión. **La orquestación de T4B no puede vivir en `PushBackPilotUpdatePlan.cs`.**
- El censo de `Compose`: una sola llamada, en `ComposeMemberEnvelope` (`:102-152`).
- `ComposeForWrite` solo tras `CompareWith` (`:156-190`).
- El orden de `Prepare` (`:195-227`).
- Los miembros salen solo de los Mutable de la relectura y nunca hay `Erase` (`:230-251`).
- R-3 en el núcleo (`:259-384`), con las formas de M-4 ya cubiertas en T4A2 (`:257-266`).
- Los builders no necesitan el Plugin (`:389-405`).
- Sin `ExecuteInCommandContextAsync`, `async` ni `Task` en `DraftSession.cs`, el plan, el puerto, el soporte, el intento y el materializador (`:408-430`).

**H-43. `WorkspaceEditIntentGuardTests.cs`, las lecturas y el antialias:**
- **D-1:** exactamente 6 `var pilot = read.PilotRead;` y 6 `.ReadIntent(` en el host core (`:727-728`), y solo el host core llama a `.ReadIntent(` (`:621-623`).
- `BaseOrReadUse` (`:647`), con M-2 ya incorporado.
- `AllowedConsumers` solo admite `.Begin(`, `.DiscardAndReload(`, `.ConfirmBase(`, `.KeepMineOnCurrentVersion(` y `.CheckNow(` (`:651`): **ni `CompleteApply` ni `Prepare` ni `From`**.
- La expresión del diagnóstico de admisión aparece exactamente una vez en el núcleo (M-3, `:860-868`).
- `PilotForms`: en el host, `pilot` solo aparece como `var pilot = read.PilotRead;`, `pilot == null` y `pilot.(StaleBase|Read|PreviewWitness|Detail|ClassicEditorHint)` (`:823-841`). `Use(pilot);` es violación (`:847`): **`Prepare(input, pilot, catalog)` en el host rompería esta guarda.**
- La contención como Failed cubre 5 cuerpos (`:583-596`).
- Nada del intento se llama desde el drenaje ni desde el Idle (`:599-640`).

**H-44. `WorkspacePilotEditGuardTests.cs`:**
- `Operations` (15) y `ReadOperations` (6) (`:44-64`);
- la marca de lectura, con cota inferior de 6 (`:366-388`);
- `WriteViolations` por cuerpo (`:693-733`);
- las autopruebas que admiten un `Update` (`:152-160`, `:354-362`, `:781-793`); ver H-33 sobre la contradicción con la guarda de archivo entero.

**H-45. `WorkspaceLifecycleGuardTests.cs`:**
- la marca «conocido limpio» (`:440-531`);
- ninguna API de escritura en todo el archivo del host core, del controlador, del intento y del registro (`:667-683`);
- las operaciones explícitas del registro no nombran la ranura (`:698-706`).

**H-46. `WorkspaceHostHardeningGuardTests.cs`:**
- la tabla de 30 literales con sus cuentas exactas (`:421-455`, `:470-490`), que una operación nueva con textos nuevos amplía;
- las subcadenas prohibidas en los textos (`Transaction|Database|Commit\(|LockDocument|…`, `:466-468`);
- la cola exacta de `PilotFieldView` (`:388-409`).

**H-47. `WorkspaceDraftBoundaryTests.cs`:**
- el conjunto exacto de `Drafts/` (`:46`);
- la prohibición de `Transaction`, `LockDocument`, `Database`, `\bCommit\s*\(`, `Debug.` y otras en todo `Drafts/` (`:107-116`): la inyección Debug de INV-30 no puede ir en `DraftSession.cs`;
- toda operación pública devuelve un valor (`:528-540`).

`WorkspaceDraftRegistryTests.cs:121-142`: ninguna operación pública de `Drafts` recibe una `DraftStaleBase` sola, salvo `CheckNow`, `ConfirmBase` y la propia `DraftStaleBase`.

**H-48. Las pruebas de comportamiento que T4B debe mantener en verde o cambiar declarándolo:**
- en `WorkspaceDraftApplyTests.cs`: `:128`, `:159`, `:219`, `:261`, `:484`, `:544`, `:565`, `:591` y `:702`;
- `WorkspaceDraftPilotOwnershipTests.cs:417-458`, que cambia si se corrige (4) (H-8);
- `WorkspacePilotPrepareTests.cs:1747-1778`, que cambia si se corrige M-5;
- y `WorkspacePilotPrepareTests.cs:1855-1899`, sobre INV-10 en el plan.

  Según su comentario (`:1857-1859`), el lector no admite sobres con miembros JSON desconocidos, así que la relectura de esa prueba se construye a mano.

---

## Implicaciones para el contrato de T4B

### Archivos de Application que T4B tocaría (el Plugin es del mapa 1)

**`src/RackCad.Application/Workspace/Drafts/DraftSession.cs`:**
- `CompleteApply` robusto ante un copiador o un comparador que lanzan (H-8, H-9). Por ejemplo, copiar antes de liberar la ranura y, si la copia falla, cerrar como Unknown(PostReread) sin dejar escapar la excepción; o el contrato que se fije.
- El XML de M-10 (H-11).
- Restricciones: sin un archivo nuevo en `Drafts/` (H-47) y sin `Transaction`, `Commit(` ni `Debug.` en el texto.

**`DraftApplyOutcome.cs`,** solo si se decide que `AppliedPostRereadFailed` lleve avisos (R-7).

**`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs`:**
- M-5 y M-6;
- y, si se decide aquí, un comparador puro de POST frente a `PostExpectation`, sin nombres prohibidos por H-42.

**Un archivo nuevo en `Systems/PushBack/Pilot/`, si la orquestación pura va en Application.** No cabe en el plan (H-42), ni en `Workspace/` si nombra el piloto (H-33), ni en `Drafts/` (H-47). Necesitaría su propia guarda de pureza.

**`WorkspaceHostCore.cs`, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs` y `WorkspaceEditIntent.cs`,** si T4B incluye ya la operación del host (R-2). En ese caso también los tres dobles (H-36) y el controlador, solo para reenviar.

**Pruebas y guardas:** las de H-42 a H-48, con cambios declarados en D-1 (6 → 7 si el host gana una lectura), `AllowedConsumers` y `PilotForms` (si el host consume la relectura de POST o pasa el resultado a `Prepare`), `ChangesADraft` y `PortMutatingOperations`, `Operations` (15 → 16), la tabla de literales (30 → 30 + n), y `WorkspaceDraftPilotOwnershipTests.cs:417-458` si se corrige (4).

### API nueva que haría falta, como propuesta medida y no decidida

1. **Un puerto de mutación inyectable**, al estilo de `SetEditIntentReader` (`WorkspaceHostCore.cs:274-275`), porque el lector no puede ganar métodos (H-33). Sus operaciones, como valores:
   - la lectura caller-owned, que da la entrada pura o el resultado del piloto;
   - la comprobación de destino;
   - las unidades por miembro con el `EnvelopeText` del plan;
   - el `Commit`, con la clasificación «lanzó» frente a «retornó»;
   - y POST, que da avisos y la relectura.
2. **La comparación de MUTATE:** `DraftStaleBase.Create(plan.ExpectedBase, plan.ExpectedVariableRegistryText).CompareWith(rereadBase)`, que es API pública existente (H-18). Con diferencias, `NotAppliedConflict(differences)` y cero unidades.
3. **La comprobación de C-F4-05** sobre los `Facts` de la lectura caller-owned (los Mutable con `LayoutReferenceCount >= 1`), con `NotAppliedPreflight(causa)` y cero escrituras (H-39).
4. **El comparador de POST:**
   - la relectura de POST admitida;
   - por cada clave de `PostExpectation.Envelopes`, la entrada Mutable de la base de POST con `EnvelopeText` ordinal igual;
   - la escala, con `Equals`;
   - si coincide, `Applied(postRead)`, y si no, `AppliedPostRereadFailed(causa)`.
5. **La clasificación por frontera (INV-30):** una excepción antes de `Commit()` da `NotAppliedFailure` o `NotAppliedPreflight`, y una en `Commit()` da `CommitThrew`.
6. **La disciplina de la ranura (INV-F4T1-20):**
   - `Prepare(From(draft), null, null)` para NothingToApply, PendingText y NotApplicable **antes** de `TryBeginApply` (H-13);
   - `TryBeginApply`, y si da `AnotherUpdateInProgress`, sin efecto;
   - después `From(draft)` bajo Applying, con `ConsideredStatus` como el anterior (H-12);
   - y `CompleteApply` en `finally`, exactamente una vez.

### Comprobable en Core con dobles, sin AutoCAD

- La tabla de `CompleteApply` y su robustez (H-6, H-8 y H-9).
- El orden de la ranura y `AnotherUpdateInProgress`.
- Que Error y Descartar no liberan la ranura.
- INV-06 (Conflict con cero unidades), INV-12 (un fallo en la unidad k da cero escrituras simuladas) e INV-30 (con un puerto cuyo `Commit` lanza).
- C-F4-05 (cero supervivientes da NotApplied sin escrituras).
- POST: coincidencia frente a diferencia de un byte, de la escala o de una clave.
- INV-10 en el plan: el sobre por vista se compone sobre el releído (ya en `WorkspacePilotPrepareTests.cs:1855-1899`).
- `LegacyDeltas` en el texto saliente.
- F4-E26 en la capa pura y por guarda (H-28).
- R-T4-07 en el host, si entra en T4B: un descarte sobre un Applying no actúa.

### Solo comprobable en el host

- Que la vía de escritura y `SyncName` conserven los bytes y los handles (H-17, R-8).
- La equivalencia de la lectura «para el mismo dibujo».
- Una sola transacción en MUTATE y `TopTransaction == null`.
- Undo.
- El contexto de comando.

---

## Riesgos y ambigüedades

- **R-1. Deriva de líneas en el paquete de T4** (ver el aviso de la cabecera). El contrato de T4B debe citar `DraftSession.cs:511-515` y `WorkspaceHostCore.cs:1154-1217`.
- **R-2. ¿T4B incluye la operación del host?** R-T4-06 y R-T4-07 son de T4C, pero dicen que la ranura y el cerrojo «son oráculo de T4B y T4C». Los borradores solo son accesibles desde el host core (H-33), así que probar la orquestación con un borrador real en Core exige un punto de entrada en el host, o un orquestador de Application que reciba el borrador del host. Hay que decidirlo antes de emitir.
- **R-3. El catálogo.** `Prepare` lo necesita, el host no lo tiene y el lector no lo devuelve (H-25). Hace falta que el puerto de mutación devuelva el catálogo junto con la relectura, o que el Plugin llame al planificador.
- **R-4. Las autopruebas de `WorkspacePilotEditGuardTests.cs:354-362`** admiten un `Update` que escribe en el host, mientras que `WorkspaceLifecycleGuardTests.cs:667-683` y `WorkspaceEditIntentGuardTests.cs:1002-1017` lo prohíben en todo el archivo. Es una contradicción latente: el contrato debe fijar que el host solo delega.
- **R-5. D-1, `AllowedConsumers` y `PilotForms`** (H-43) no admiten que el host pase el resultado del piloto a `Prepare` ni la relectura de POST a `CompleteApply`. Hay que declarar el cambio, con autopruebas en negativo, o hacer que esos consumos ocurran fuera del host.
- **R-6. F4-E25 (4):** la prueba vigente exige la excepción (H-8). Corregirla es un cambio declarado de una prueba existente. También hay que decidir qué pasa con un comparador que lanza en `Status` tras liberar la ranura (H-9).
- **R-7. «Los avisos de N-5»** (R-T4-04, paquete `:542`) es ambiguo:
  - la única N-5 del paquete trata de `Kept` (`:639`);
  - lo más probable es que se refiera al arrastre de F4-E16, «los avisos de POST visibles aunque falle su relectura» (`f4-dispositions.md:231`);
  - `AppliedPostRereadFailed` solo lleva una causa (`DraftApplyOutcome.cs:84`), y `PilotFieldView` no lleva avisos (H-35).

  Si los avisos deben sobrevivir, falta API.
- **R-8. El byte a byte de POST depende** de que las claves (handles) de los miembros no cambien al redibujar en sitio y de que la escritura y `SyncName` no reserialicen el sobre. No se puede medir en Core, y es la parada que prevé R-T4-04.
- **R-9. `ExpectedVariableRegistryText` siempre es null en el piloto** (H-19). La comparación del registro es vacía: hay que declararlo.
- **R-10. `ExpectedBase` no lleva la ambigüedad** (H-18). Es benigno para un Ready, pero la reconstrucción con `Create` debe declararse equivalente solo bajo un Ready.
- **R-11. `NotAppliedConflict` no lleva causa** (H-6). Con un Conflict de MUTATE, `LastApplyCause` queda null, mientras que D-13 pide «la causa y, en Conflicto, qué cambió». Solo hay diferencias.
- **R-12. Sobres con miembros JSON desconocidos:** el lector no los admite (H-48). La relectura de POST de ese rack daría NotAdmitted y por tanto Unknown(PostReread). INV-10 sobre desconocidos solo se prueba con relecturas hechas a mano.
- **R-13. La marca de obsolescencia del drenaje durante la mutación propia** marca todos los borradores de la sesión, incluido el de B (`WorkspaceHostCore.cs:174-175`, `:1321-1325`). Es R-T4-08 e INV-28 en T4C, y los dobles de T4B no la ejercitan.
- **R-14. `TryBeginApply` acepta Clean** (H-2). «NothingToApply antes de la ranura» debe ser una guarda o una prueba del llamador, no del núcleo.
- **R-15. R-T4-07:** los descartes actúan hoy sobre un Applying (H-31), y `EditDiscardStatus` no tiene un valor para ese caso. La comprobación va en el host, porque el registro no puede nombrar la ranura.
- **R-16. La variante caller-owned del barrido** choca con las guardas de cuenta y de firma de `RackSiblingScan.cs` (H-27). Conviene un núcleo de recorrido único, con un cambio declarado de esas guardas.
- **R-17. La puerta de reposo** del host y del lector bloquea el contexto de comando (H-32). Hace falta otra vía (R-T4-06, de T4C), y su relación con la equivalencia de F4-E26 (H-28) debe estar en el contrato.
- **R-18. C-F4-05 no está en la costura:** con `eraseByKind` nulo nunca se llega a `DeferToFirstPlacement` (H-39), así que la comprobación de supervivientes debe ser nueva y explícita, y salir de los hechos, no de la base.
- **R-19. La costura de I-55 no encaja directamente** con el plan, que son textos por clave (H-40). Hay que elegir entre un adaptador sobre `RackSiblingRedrawRun`, con la frontera de `Commit()` aditiva, y una ejecución propia del piloto. Insertar no puede cambiar (INV-30).
- **R-20. Los menores siguen abiertos en el código:** M-1 (sin guarda de `ComposeForWrite`), M-5, M-6 y M-10. D-2 y D-3 están anotados. M-2, M-3, M-4 y M-9 están cerrados en T4A2.
