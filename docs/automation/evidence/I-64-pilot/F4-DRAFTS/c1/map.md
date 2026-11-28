# I-64 F4 — Read-only map for the Smoke-2 correction (C1 F4-E72, C2 F4-S2-01, C3 F4-S2-03)

> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<worktree-de-diagnostico>`, `<carpeta-de-copias-de-Smoke-2>`, `<carpeta-del-DLL-de-Smoke-2>`, `<build-diag1>`, `<build-diag2>`, `<directorio-de-H0>`). El resto no cambia.


Worktree `<worktree-de-I-64>`, HEAD `d4a24790`.
`git diff --stat b88dabf2 HEAD -- src tests` is empty: every line number below is valid on the product `b88dabf2`.
Abbreviations: `TW/` = `tests/RackCad.Tests/Workspace/`, `EBC` = `src/RackCad.Application/Workspace/EventBridgeCore.cs`,
`BRIDGE` = `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`, `CTL` = `src/RackCad.Plugin/Workspace/WorkspaceController.cs`,
`FRZ` = `docs/initiatives/I-64-proposal-v5.md`, `ICL` = `docs/initiatives/I-64-implementation-clarifications.md`,
`T4CP` = `docs/initiatives/I-64-f4-t4c-gate-contract-package.md`. Nothing was built, run or written in the repository.

## 0. Facts from the diagnostic material (inputs, not opinions)

- `diag2.patch` changes three behaviours: (1) `system.Name = embed.Name;` after `PushBackPilotSystemRebuild.Build` in `RackPilotUpdateWriter.PrepareUnits`; (2) `IsAssociativeBookkeeping(DBObject o) => o is AssocAction || o is AssocDependency` with an early `return` in the five object handlers, **outside** `ContainSession`; (3) per-bridge (not per-session) fields `_undoCommandRunning`/`_undoTouchedDefinition`, set in `CommandWillStart` for `U|UNDO|REDO|MREDO`, any `o is BlockTableRecord` (type only, no `IsLayout`/xref/anonymous test) during it, and on `CommandEnded|Cancelled|Failed` an `AcApplication.Idle += once` whose body runs `doc?.Editor.Regen()` **inside the Idle callback**.
- Trace `<carpeta-de-copias-de-Smoke-2>\diag\trazas\rackcad-20261116.log.diag2` (39 670 lines, read-only):
  - after Actualizar the drain is own-only: `:7031 drain ... ops=[InvalidateNavigationCaches]`;
  - `U`: `:7032` CWS(U) … `:22905` CommandEnded(U), `:22906 undo.end cmd=U touchedDefinition=True`, regen `:22908`→`:22912` (01:56:01.008→02.407, ≈1.4 s), then `:22913 drain ... ops=[InvalidateNavigationCaches,MarkDraftsPossiblyStale]` (the foreign U still marks, FRZ:237-238). Same for `REDO` (`:22914`…`:39647`);
  - **the programmatic regen itself raises `AssocNetwork` ObjectModified** (`skip.assoc` at `:22909-22911` and `:39643-39645`): without C2 a C3 regen would enqueue a foreign hint and mark again → **C3 depends on C2**;
  - during `U` the bridge saw 15 872 object events; non-entity classes: 8 533 `ObjectModified BlockTableRecord`, 139 `ObjectErased BlockTableRecord`, 113 `ObjectUnappended BlockTableRecord`, plus DBDictionary/Xrecord (envelopes). The trace does not say which BTR is a layout; **an undo of a LINE was not exercised** (so whether `U` of a LINE raises `ObjectModified` on `*Model_Space` is NOT measured).
  - order in diag2: regen (in the Idle callback) **before** the drain continuation. A design that regenerates inside the drain reverses that order: not validated in AutoCAD.

---

## 1. C1 — F4-E72, the rack name in the pilot Actualizar

### 1.1 Places that change
- `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs:118` `var system = PushBackPilotSystemRebuild.Build(member.DesignText, _catalog);` — the system goes to the three `PrepareRedraw` at `:123`, `:126`, `:129-135` with `Name == null` (the resolver does not set it; `PushBackResolver.cs` has no `Name`).
- Classic reference: `src/RackCad.Plugin/RackPushBackCommands.cs:187-190` `var name = string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName; … system.Name = name;` (raw, untrimmed).
- The label rule is pure Application: `src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs:91-94` (`if (system.DrawRackName && !string.IsNullOrWhiteSpace(system.Name)) … system.Name.Trim()`, also `:156`, `:256`), used by `src/RackCad.Application/Systems/PushBack/PushBackSystemLateralBuilder.cs:213`. `system.Name` is not used by any other Push Back builder/draw service (grep).
- The envelope is already in the plan: `PushBackPilotMemberPlan.EnvelopeText` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs:166`); `PushBackPilotUpdateRun.DesiredNamesOf` already deserializes it with `RackEmbedStore` (`PushBackPilotUpdateRun.cs:196-201`).

### 1.2 Callers of `PushBackPilotSystemRebuild.Build` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs:24-36`)
- `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs:118` (the one that draws);
- `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs:214` (only `IsComposite`, for the cut names; the name is irrelevant there);
- `TW/WorkspacePilotUpdateRunTests.cs:897-902` (pins the 2-argument signature and its three throws).

### 1.3 Options
- (a) Plugin only: one line in the writer (diag2-validated). Untestable (ADR-0003); only a text guard on the writer.
- (b) **Application**: a new pure member of `PushBackPilotSystemRebuild`, e.g. `BuildForMember(PushBackPilotMemberPlan member, RackCatalog catalog)` = `Build(member.DesignText, catalog)` + `Name = envelope.Name` (deserialized from `member.EnvelopeText`, raw, like RACKEDITAR); the writer calls it at `:118`. `Build(string, RackCatalog)` stays for `PushBackPilotUpdateRun.cs:214` and the existing test.
- **Recommendation: (b).** The RED is a Core assertion, the writer stays a primitive caller, and no existing guard has to change.

### 1.4 Existing guards that apply (none fails with (b) if respected)
- Purity of `RebuildPath` (it is in `PureFiles`, `TW/WorkspacePilotUpdateGuardTests.cs:28`): RAW text, comments included, may not match `Transaction`, `\bDatabase\b`, `Autodesk`, `RackCad\.Plugin`, `\bTask\b|Task\.|ValueTask`, `Dispatcher`, `System\.IO|\bFile\.|\bStream\b`, `\btypeof\s*\(` … (`:56-71`, enforced at `:90-101`, self-test `:104-115`). `RackEmbedStore` is allowed (it is `RackCad.Application.Persistence`).
- `TW/WorkspacePilotUpdateGuardTests.cs:118-124` no file of the pilot folder contains the word `Transaction`.
- Writer census: `:215-…` `WriterStructureViolations` (`Src.Count(prepare, @"\bPrepareRedraw\s*\(") != 3`), `:198-201` PrepareRedraw only in PrepareUnits, `:261` `Assert.Equal(3, Src.Count(Writer(), @"\bclass\s+RackPilot\w+"))`, `:189-190` banned `RackViewBaseName`/`BlockNaming` in the writer.
- D-T4C-14 G-1 (`TW/WorkspacePilotUpdateIntegrationGuardTests.cs:289-311`) and G-9 writer counts `[WriterPath] = { RedrawInTransaction=3, SystemBlockWriter=2, Commit=1, StartTransaction=1, StartOpenCloseTransaction=1, SyncName=1 }` (`:681-686`): unchanged by a call rename.
- `TW/WorkspacePilotUpdateGuardTests.cs:848-866` the three test files `WorkspacePilotUpdateRunTests.cs`, `WorkspacePilotUpdateHostTests.cs`, `WorkspacePilotUpdateScopeDouble.cs` may not construct or compose an envelope: the RED must use the kit texts (`PrepareKit.Lateral(post, fixture, name)` / `PrepareKit.Frontal(fixture, section, name)`, `TW/WorkspacePilotPrepareTests.cs:26-33`) and only `Deserialize` them, as `TW/WorkspacePilotUpdateRunTests.cs:871-875` already does.
- Equivalence fact: diag2 Actualizar succeeded with the name set (the `WriteUnit` payload check `RackPilotUpdateWriter.cs:267-268` did not throw), label «PB G» kept.

---

## 2. C2 — F4-S2-01, associative bookkeeping counted as a drawing change

### 2.1 The path today
- `BRIDGE:240-274` the five object handlers, each `ObjectEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));`.
- `EBC:346-353` `if (state.OwnMutationOpen) live.EnqueueOwnHint(HintKind.DatabaseObjectChanged); else live.EnqueueHint(HintKind.DatabaseObjectChanged);`.
- `src/RackCad.Application/Workspace/WorkspaceSession.cs:66-68` `var foreignChange = _pending.Contains(HintKind.DatabaseObjectChanged) || _pending.Contains(HintKind.CommandEnded); … if (foreignChange) ops.Add(DrainOperation.MarkDraftsPossiblyStale);`.
- Host: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` `OnDrained` → `MarkPilotDraftsPossiblyStale(outcome.Session)` (pinned by `TW/WorkspaceLifecycleGuardTests.cs:618`).

### 2.2 Authorities that frame it
- FRZ:216-218 handlers «no leen ni escriben la base de datos … **Filtran por tipo con coste constante** y anotan una pista» → a type filter in the handler is literally within the Freeze.
- FRZ:237-238 «`CommandEnded` de `U`, `UNDO`, `REDO` y `MREDO`, junto con los eventos de objeto, invalidan y marcan» → unaffected (U still marks through its foreign `CommandEnded`, trace `:22913`).
- ICL:143 (IC-08) «`MarkDraftsPossiblyStale` nace de **cualquier cambio de base de datos** y de cualquier `CommandEnded`» → the filter changes the letter of IC-08 for Assoc* objects: needs a declared IC (refinement), as IC-09 was.
- T4CP:887 (D-T4C-09) «los demás manejadores no cambian: no leen handles ni el DWG» → any object-handler change departs from that package sentence (declare it).
- ICL:77 (IC-04) handlers «no pueden leer el DWG: solo encolan o agrupan pistas»; D-T4C-09 (T4CP:866) ruled reading `e.GlobalCommandName` an in-memory event datum. A `e.DBObject is AssocAction` type test is the same family (FRZ:218); no ruling covers property reads of `e.DBObject` (relevant for C3).

### 2.3 Design (a) — Plugin only (diag2)
Change: the five object blocks become `handler = (s, e) => ContainSession(a, () => { if (!IsAssociativeBookkeeping(e.DBObject)) _core.OnSourceEvent(a.Session, a.Source); });` + `private static bool IsAssociativeBookkeeping(DBObject o)` (the diag2 early return outside `ContainSession` lets an exception reach AutoCAD; move it inside).
Existing guards that FAIL: **none found**. Checked and satisfied:
- `TW/WorkspaceBridgeSourceGuardTests.cs:183-189` (`Assert.Equal(GovernedEvents.Length, handlerLines.Count); … Contain(?:Session)?\(`), `:242-248` field guard (a `private static bool …(DBObject o)` ends in `(` and is skipped, `:214-215`), `:251-257` public surface, `:171-180` forbidden calls (no name hit);
- `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:107-117` (16/16/16 unchanged), `:120-129` (no `Regen`, `.Database` only on hookup lines), `:132-141`;
- `TW/WorkspaceIdleSchedulingGuardTests.cs:64-65` `HandlerForbidden` (`GetObject`, `OpenMode`, `\.Read\w*\s*\(`… not used), `:82` ≥16 case blocks, `:129` (the literal `ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));` must still exist somewhere for `Mutate`: it stays in ImpliedSelectionChanged `BRIDGE:195` and the three command ends `BRIDGE:215/222/229`);
- `TW/WorkspaceHostSourceGuardTests.cs:467-477` (`^\s*(?:private|…)\s+(?:static\s+)?(?:readonly\s+)?(?:Transaction|DBObject|…)\b[^(]*[;=]`: `private static bool` does not match);
- `TW/WorkspaceLifecycleGuardTests.cs:730-741` write APIs (bridge in the list) — none used.
Tests it allows: only source-text guards (ADR-0003).
Side effects: bookkeeping events no longer reach `EBC.OnSourceEvent`, so they **no longer forget `_lastAllowedClose`** (`EBC:315`) — a change to the ratified T2B2B close-sequence observation (D-T2B2-10), unmeasured whether AutoCAD raises Assoc events between an allowed close and `DocumentToBeDestroyed`.

### 2.4 Design (b) — classification in the handler, decision in Application
Change: `BRIDGE` object blocks: `handler = (s, e) => ContainSession(a, () => _core.OnObjectEvent(a.Session, a.Source, SubjectOf(e.DBObject)));` with a private, total (own try/catch → conservative value) classifier by type; a neutral enum in `src/RackCad.Application/Workspace/` (e.g. in `BridgeSource.cs` or `Hints.cs`: `Drawing`, `AssociativeBookkeeping`, and `BlockDefinition` for C3); `EBC.OnObjectEvent(SessionId, BridgeSource, <enum>)`: forget `_lastAllowedClose` first (keeps T2B2B), same checks as `OnSourceEvent`, bookkeeping → return without hint/SyncIdle; otherwise the existing own/foreign enqueue. `OnSourceEvent(SessionId, BridgeSource)` keeps its 2-param signature and its object cases (dozens of tests call it with object sources: `TW/WorkspaceOwnMutationTests.cs`, `TW/WorkspaceBridgeHintAndDrainTests.cs:22-53`…).
Existing guards that need a DECLARED change:
1. `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:216-236` `DeclaredCoreSurface()` → `Assert.Equal(DeclaredCoreSurface(), CoreSurface());` (reflection over public names): +`OnObjectEvent`.
2. `TW/WorkspaceLifecycleGuardTests.cs:114` `Assert.Equal(16, Src.Count(code, @"\b_lastAllowedClose\s*=\s*null\s*;"));` → 17; `:126-142` `ForgettingEntries` +1 entry; `:171` `Assert.Equal(16, ForgettingEntries.Length);` → 17; `:196-201` negative theory +1 `InlineData`.
Not affected if the 2-param `OnSourceEvent` is kept textually: `TW/WorkspaceLifecycleGuardTests.cs:133` and `TW/WorkspaceHostC2SourceGuardTests.cs:205` (both `BodyOf(…@"public\s+bool\s+OnSourceEvent\s*\(\s*SessionId\s+\w+\s*,\s*BridgeSource\s+\w+\s*\)")` — an extra/optional third parameter would make both FAIL with «member not found»).
Naming constraints met by construction: type names in `RackCad.Application.Workspace` may not match `Subscription|Editor|Draft` nor `Tab` (`TW/WorkspaceModelBoundaryTests.cs:94-105`, `TW/WorkspaceBrowserBoundaryTests.cs:283-293`; note `…Table…` contains «Tab»); every new Workspace type name becomes a banned word in the browser files (`TW/WorkspaceBrowserBoundaryTests.cs:84-100`); no `\.Write[A-Z]\w*\s*\(` in EBC/BRIDGE (`TW/WorkspaceIdleSchedulingGuardTests.cs:597-618`); no `Transaction|\bDatabase\b` in Application/Workspace (`TW/WorkspaceModelBoundaryTests.cs:41-49`).
Tests it allows (Core, simulated source): no hint/no Idle for bookkeeping; drawing events unchanged; own scope/command state untouched; close sequence still forgotten; host-level «a read that only raises bookkeeping leaves the draft unmarked».
- **Recommendation: (b).** Same validated behaviour as diag2, but the rule is Core-testable, T2B2B's «every input forgets» stays exact, and C3 reuses the same entry (one new EBC name instead of two separate mechanisms). Cost: 2 guard files, 4 assertions, all of the kind declared by T4C1 (D-T4C-20 rows 14-15). (a) is the zero-guard-change fallback if C3 is not done.
- A third design was considered and not recommended: an «own read scope» around panel reads (Application-testable) — it needs every read site bracketed (Plugin adapters and host core), masks foreign events during reads, and does not cover the Assoc events raised by the C3 regen (trace `:22909-22911`) or by other add-ins' locks.

---

## 3. C3 — F4-S2-03, no refresh after U/REDO of a redefinition

### 3.1 Authorities that frame it
- FRZ:228-232 (D-04) drain «Operaciones permitidas en el drenaje, **y solo estas**: 1. leer la selección … 2. recalcular … 3. invalidar cachés … 4. marcar borradores»; `src/RackCad.Application/Workspace/Hints.cs:14-21` «The only operations a drain may run in F1 (D-04)»; T4CP:845 «`DrainOperation` no cambia».
- FRZ:741 INV-03 «Ningún evento ni cambio de contexto escribe selección o vista»; FRZ:240 «ningún evento ni cambio de contexto produce una escritura de selección o de vista». The F1 guards classify `Regen` as a view write: `TW/WorkspaceBridgeSourceGuardTests.cs:137-142` (INV-F1-T2-03, «events never write selection or view», `…|UpdateScreen|Regen|…`), `TW/WorkspaceHostSourceGuardTests.cs:380-396` (INV-F1-T3A-08).
- FRZ:630/739 P-02/INV-01 «cero llamadas a … `Regen` … sin pistas pendientes» (a regen after U has a pending hint → not violated).
- FRZ:237-238 undo/redo «invalidan y marcan» (C3 adds a third effect).
- ICL:77-83 IC-04: nothing synchronous in a handler or «un callback de Idle»; «después del drenaje» is acceptable. diag2 ran the regen inside the Idle callback (`AcApplication.Idle += once`).
- Owner D5 (memory/`owner-f4t4c-decisions-d1-d6.md`): «volver si aparece decisión material nueva (… **Undo**, conflicto/stale, …)». The fix order (`owner-f4-smoke2-fix-order.md`) lists F4-S2-03 to fix and says «Las decisiones materiales que no cubran esas reglas se anotan para el Owner, sin parar el resto».

### 3.2 Trigger (common to both designs)
- Command family by `e.GlobalCommandName` (in-memory, D-T4C-09 precedent): `U`, `UNDO`, `REDO`, `MREDO`, case-insensitive (diag2 set).
- Object: `BlockTableRecord` that is not a layout (model/paper space), not from an xref (`IsFromExternalReference`/`IsDependent`), optionally not anonymous. The pilot's redrawn definitions are named (renamed by `RackBlockRenamer.SyncName`, `RackPilotUpdateWriter.cs:177-180`), so excluding anonymous still triggers for the pilot. Type-only (`is BlockTableRecord`, diag2) needs no property read but probably also fires on the model space BTR when undoing a LINE (NOT measured, §0). The property reads (`IsLayout`, …) of `e.DBObject` in a handler are not covered by FRZ:218 («por tipo») nor by the D-T4C-09 ruling → need an explicit declaration (IC) — fact, not a recommendation.
- Per session, not per bridge (diag2 used single bridge fields): state belongs in `EBC.SessionState` (`EBC:28-48`). Constraint: its body may not contain `SessionId?|Allowed|Close` (`TW/WorkspaceLifecycleGuardTests.cs:115-117`).
- End: the first `CommandEnded|Cancelled|Failed` of the session (diag2 refreshed on all three); a nested `CommandWillStart` resets (diag2: `DiagUndoStart` on every start).
- A classifier exception inside `ContainSession` degrades the session (`BRIDGE:384-401`) → the classifier must be total.

### 3.3 Design (a) — Plugin only
(a1) diag2 form (`AcApplication.Idle += once` + `Editor.Regen()` in `BRIDGE`). Existing tests that FAIL (10):
1. `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:107-117` `Assert.Equal(16, Src.Count(code, @"\+="));` / `-=` (17/17).
2. `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:120-129` `DoesNotMatch(…|Highlight|Zoom|Regen|SendStringToExecute)` (substring `Regen`).
3. `TW/WorkspaceBridgeSourceGuardTests.cs:102-123` `Regex.Matches(subscriber.Code, @"\." + name + @"\s*\+=").Count == 1, name + " must be hooked exactly once"` (Idle twice).
4. `TW/WorkspaceBridgeSourceGuardTests.cs:171-180` `HandlerForbiddenCallsFor(path)` (`\bRegen\b`, bridge has no exception).
5. `TW/WorkspaceSourceGuardExceptionTests.cs:130-140` `TheRealWorkspaceFilesPassTheRefinedGuard` (same regex).
6. `TW/WorkspaceHostSourceGuardTests.cs:383-396` `\b(?:…|Zoom\w*|Regen|UpdateScreen|…)\b` over Plugin+Application Workspace.
7. `TW/WorkspaceHostC2SourceGuardTests.cs:190-197` 16/16/16.
8. `TW/WorkspaceIdleSchedulingGuardTests.cs:506-523` G-6 `if (plus != 16) …`.
9. `TW/WorkspaceLifecycleGuardTests.cs:273-289` 16/16/16.
10. `TW/WorkspacePaletteHostSourceGuardTests.cs:377-394` 16/16/16.
(+ `TW/WorkspaceIdleSchedulingGuardTests.cs:546-560` G-7 and `TW/WorkspaceHostSourceGuardTests.cs:495-499` if any `RackLog.Warning` survives.) Posting with `_dispatcher.BeginInvoke` instead of `+=` trips `TW/WorkspaceIdleSchedulingGuardTests.cs:241-254` G-3 (`BeginInvoke appears … expected 1`) and G-1 `:64-65` (`BeginInvoke` in a case block).
(a2) through the existing continuation (flag set in the command-end block, `RunIdleContinuation` `BRIDGE:294-298` calls the regen after `OnIdle`): no new `+=`/`BeginInvoke`; G-9 (`:641-653`) and G-10 (`:775-…`) still pass (they check presence/order of `OnIdle` and the Debug check only). Still FAIL: items 2, 4, 5, 6 (the `Regen` word) unless an exact-file exception for `BRIDGE` is added to `HandlerForbiddenCallsFor` (`TW/WorkspaceBridgeSourceGuardTests.cs:163-168`), to `:383-396` and to the facade regex. Renaming the call to dodge the regexes (e.g. a helper without the word) would pass them literally while contradicting INV-F1-T2-03.
Tests (a) allows: text guards only; the trigger rule (LINE vs definition, REDO, coalescing, degraded session) is untestable.

### 3.4 Design (b) — decision in Application, execution in the drain
Change:
- `BRIDGE` `CommandWillStart` block: one more statement after the ratified one, e.g. `_core.NoteCommandStart(a.Session, e.GlobalCommandName);` (keeps `TW/WorkspacePilotUpdateIntegrationGuardTests.cs:1031-1046`: `NoteForeignCommandStart` once, order contain < decision < note, no `GetObject|StartTransaction|LockDocument|OpenMode|\.Database\b|\.Read\w*\s*\(`; keeps `TW/WorkspaceHostC2SourceGuardTests.cs:104-115`: no `EnqueueHint|OnIdle|AcApplication\.Idle|…` in that block). Object blocks: the C2 classifier gains `BlockDefinition`.
- `EBC`: `NoteCommandStart` (undo-family list, testable), `SessionState` fields, `OnObjectEvent` marks «definition touched» while the family runs, and the command-end case of `OnSourceEvent` (`EBC:330-345`) enqueues one new hint kind when both hold (own/foreign logic untouched).
- `src/RackCad.Application/Workspace/Hints.cs:7-21`: +1 `HintKind`, +1 `DrainOperation` (name without the word `Regen`, without `Navigation`/`Handle` — `TW/WorkspaceNavigationEchoTests.cs:325-340` bans them in `WorkspaceSession.cs` except `DrainOperation.InvalidateNavigationCaches`).
- `src/RackCad.Application/Workspace/WorkspaceSession.cs:52-72` `TryDrain` emits the op (coalesced; `PendingHintCount` counts it).
- `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` `OnDrained`: after `WorkspaceHostPolicy.MayDrain(conditions)` and before the selection read (whose early `return`s would skip it), call a port once for the session's document; setter like `SetPilotUpdateHost` (`WorkspaceHostCore.cs:289`). Port type: an interface or `Action<long>` whose namespace is **not** `RackCad.Application.Workspace` (a Plugin file outside `Plugin/Workspace` may not reference it: `TW/WorkspaceBridgeSourceGuardTests.cs:362-366` `AutoCadEventBridge|RackCad\.Plugin\.Workspace|RackCad\.Application\.Workspace`) and **not** `RackCad.Application.Systems*` (`TW/WorkspaceEditIntentHostTests.cs:1102-1120`, only `_updateHost` may be of `Systems`). `src/RackCad.Application/Views/Redraw/` is in the T4C2 forbidden scope; a new folder/namespace must satisfy `NamespaceFolderGuardTests`.
- Implementer in `src/RackCad.Plugin/Views/` (new file; `Plugin/Workspace` must stay at 9 files, `TW/WorkspaceNavigationHostTests.cs:714-718`): finds the document by native id, regenerates only the active one with `document.Editor.Regen()`, catches and logs (a throw inside `WorkspaceHostCore.OnDrained` ends in `Contain` → `_faulted.Add(session)`, `WorkspaceHostCore.cs:1991-1994`). If it names the pilot (`PushBackPilot\w*|RackPilot\w*`) G-9 applies (`TW/WorkspacePilotUpdateIntegrationGuardTests.cs:675-711`: `SystemBlockWriter`/`LockDocument` forbidden there); do not use `SystemBlockWriter`/`LockDocument` from `Plugin/Workspace` (G-6 `:589-640`, `TW/WorkspaceLifecycleGuardTests.cs:725-737`).
- `CTL`: wires the port (constructor or the `Navigation` getter after `_host.SetPilotUpdateHost(...)`; `TW/WorkspaceEditIntentGuardTests.cs:1154-1168` only censuses `SetPilotUpdateHost`). No `+=` (3 exactly: `TW/WorkspaceHostSourceGuardTests.cs:197`, `TW/WorkspaceHostC2SourceGuardTests.cs:137`, `TW/WorkspaceLifecycleGuardTests.cs:338`, `TW/WorkspaceNavigationHostTests.cs:708`, `TW/WorkspaceBrowserHostGuardTests.cs:148`, `TW/WorkspacePaletteHostSourceGuardTests.cs:420`, G-6 `:515`), no `Dispatcher|Timer|Task` (`TW/WorkspaceCommandAdvisorGuardTests.cs:341-346`, `TW/WorkspaceNavigationHostTests.cs:472-480`).
Existing guards that need a DECLARED change (on top of C2-b):
1. `TW/WorkspaceHintDrainTests.cs:93-113` `Assert.Equal(allowed.OrderBy(x => x), System.Enum.GetValues<DrainOperation>().OrderBy(x => x));` (+1 op); its `foreach (var kind in System.Enum.GetValues<HintKind>())` loop requires the new hint alone to drain non-empty ops ⊆ allowed.
2. `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:216-236` +`NoteCommandStart` (total +2 with C2).
3. `TW/WorkspaceLifecycleGuardTests.cs:114` 17→18, `:126-142` +1, `:171` 17→18, `:196-201` +1.
Not affected: `TW/WorkspaceBridgeHintAndDrainTests.cs:106-117` (`[InvalidateNavigationCaches, MarkDraftsPossiblyStale]` for ObjectModified+CommandEnded without undo), `:272-286` (`IHostEventSource` = Attach/Detach, EBC never takes/returns `SelectionContext`), `:288-296` (EBC ctor 2 params), `TW/WorkspacePilotEditGuardTests.cs:385` (host interfaces exactly two), all Regen-word guards (no `Regen` text in the Workspace folders — but see §6: the intent of INV-F1-T3A-08/INV-03 changes by design and must be declared, not dodged).
Tests it allows: the whole trigger rule in Core plus the host call with a port double.
- **Recommendation: (b), subject to the Owner decision of §6.** Per-session state, testable rule (incl. «U of a LINE → no regen»), the regen runs in the scheduled drain (IC-04, not in a handler nor in the Idle callback), coalesces, keeps every subscription/post census (16/16/16/1, controller 3), and the Freeze deviation is one explicit `DrainOperation` instead of a hidden bridge exception. Variant (b2): keep `DrainOperation` unchanged and let `CTL` take a «refresh requested» flag from the bridge after the drain — avoids `TW/WorkspaceHintDrainTests.cs:93-113` but adds an EBC query and is the same Freeze reading.

---

## 4. Paths the task would touch (recommended designs) and CI shape

### src
- C1: `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs`; `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`.
- C2: `src/RackCad.Application/Workspace/EventBridgeCore.cs`; `src/RackCad.Application/Workspace/BridgeSource.cs` (or `Hints.cs`) for the subject enum; `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`.
- C3: `src/RackCad.Application/Workspace/EventBridgeCore.cs`; `src/RackCad.Application/Workspace/Hints.cs`; `src/RackCad.Application/Workspace/WorkspaceSession.cs`; `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`; one new port file in a new non-Workspace, non-Systems Application folder (or none with `Action<long>`); one new file in `src/RackCad.Plugin/Views/`; `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`; `src/RackCad.Plugin/Workspace/WorkspaceController.cs`.
### tests — Core (`tests/RackCad.Tests`)
- changed: `TW/WorkspaceBridgeFacadeSourceGuardTests.cs`, `TW/WorkspaceLifecycleGuardTests.cs`, `TW/WorkspaceHintDrainTests.cs` (C3);
- added to: `TW/WorkspacePilotUpdateRunTests.cs`, `TW/WorkspacePilotUpdateGuardTests.cs` (C1);
- new: one C2 file and one C3 file (class names must start with `Workspace`, see filters), optionally a bridge text-guard file; `TW/WorkspaceHostTestSupport.cs` only if the host rig gets the port double.
### tests — UI (`tests/RackCad.UI.Tests`)
- none (no change under `src/RackCad.UI`; the port `IWorkspaceHostPort` and the edit port do not change).
### T4C2 contract shape (`docs/automation/evidence/I-64-pilot/F4-T4C2-PANEL/coordinator/gate-contract-attempt2.json`)
- Keys: `Schema, Unit, Gate, TaskId, Objective, AuthorityRevision, MainSha, Authorities(35), AllowedWriteScope(14), ForbiddenWriteScope(150), Invariants(18), RequiredTests(26), ExpectedEvidence(9), StopConditions(33), EligibleCells(2), RoutingEnforcement, CorrectionsAuthorized, IssuedBy, IssuedUtc`.
- Forbidden there and needed here (must move to Allowed): prefixes `src/RackCad.Plugin/`, `src/RackCad.Application/Systems/`, `src/RackCad.Application/Views/Redraw/` (only if used); files `EventBridgeCore.cs`, `WorkspaceSession.cs`, `TW/WorkspacePilotUpdateRunTests.cs`, `TW/WorkspacePilotUpdateGuardTests.cs`, `TW/WorkspaceBridgeFacadeSourceGuardTests.cs`, `TW/WorkspaceLifecycleGuardTests.cs`, `TW/WorkspaceHintDrainTests.cs`, `TW/WorkspaceHostTestSupport.cs`. `Hints.cs`, `BridgeSource.cs` were not listed; `WorkspaceHostCore.cs` was Allowed.
- RequiredTests filters: Core `FullyQualifiedName~RackCad.Tests.Workspace` (MinSelected 3421, ExpectRed true) + 23 Core filters with ExpectRed false (`CallerOwnedFacadeGuardTests` 81, `CallerOwnedTransactionGuardTests` 15, `RackSiblingRedrawUnitsTests` 2, `DebugFaultInjectionGuardTests` 1, `SiblingRedrawSeamGuardTests` 2, `RackSiblingMembershipTests` 12, `RackSiblingRedrawPlanTests` 2, `RackSiblingRedrawRunTests` 6, `PushBackClassicCharacterizationPluginGuardTests` 10, `PushBackPluginSourceGuardTests` 11, `PushBackRoundTripSourceGuardTests` 24, `RackProjectionCommandGuardTests` 34, `NamespaceFolderGuardTests` 7, `CustomPropertiesEnvelopeGuardTests` 22, `RackUnitsGuardSourceTests` 18, `I55G15Auth15ConsumerTests` 5, `SingleScanInsertGuardTests` 5, `RackViewBatchDriverGuardTests` 4, `G16FirstViewCorrectiveTests` 61, `RackSiblingCustomPropertiesGateTests` 2, `RackEnvelopeIdProbeTests` 5, `SelectiveEditorOpenTests` 33, `CustomPropertiesCommandGuardTests` 48); UI `FullyQualifiedName~RackCad.UI.Tests.Workspace` (601, ExpectRed true) and `~RackCad.UI.Tests.PushBackClassicCharacterization` (55, false). Here UI would be ExpectRed false (no UI change). The MinSelected values predate the T4C2 GREEN: re-measure on `b88dabf2` (its CI was 43758023582, per the session memory).
- The test namespace is `RackCad.Tests` / `RackCad.UI.Tests` (not `.Workspace`), so the Workspace filter selects by **class-name prefix**: a RED class not starting with `Workspace` is outside the ExpectRed filter. CI runs both full suites without filter (`.github/workflows/ci.yml:133`, `:241`).
- ExpectedEvidence[2]: «RED … pruebas que compilan contra cuerpos inertes; las existentes pasan y solo las nuevas o las cambiadas fallan, por aserción» → new members (`BuildForMember`, `OnObjectEvent`, `NoteCommandStart`, the op, the setter) go into the RED commit with inert bodies.

---

## 5. RED tests to write first (fail on `b88dabf2` by assertion with inert bodies; pass after)

C1
1. `WorkspacePilotUpdateRunTests.TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope` — member from `PrepareKit.Lateral(PrepareKit.ExistingPost(), "pb-01-base", "PB G")` (deserialize only): `BuildForMember(member, PilotKit.Catalog).Name == "PB G"`; for every member of `new PilotUpdateSetup().ExpectedPlan()` the rebuilt `Name` equals the deserialized envelope `Name`; blank envelope name → `IsNullOrWhiteSpace(Name)` (no label, like RACKEDITAR). Inert body = `Build(member.DesignText, catalog)` → `Name` null → fails.
2. `WorkspacePilotUpdateGuardTests.PrepareUnitsBuildsTheSystemOfEachMemberWithItsRackName` (writer text): `PrepareUnits` calls `PushBackPilotSystemRebuild.BuildForMember(member, _catalog)` exactly once and the writer names `PushBackPilotSystemRebuild.Build(` zero times; negative on the `b88dabf2` text. Fails on `b88dabf2` (writer still calls `Build`).

C2 (new class, e.g. `WorkspaceAssociativeBookkeepingTests`)
3. `AnAssociativeBookkeepingObjectEventQueuesNoHintAndHooksNoIdle` (theory over the 5 object sources): `PendingHintCount == 0`, `Source.Count(BridgeSource.Idle) == 0`, no faults. Inert `OnObjectEvent` (delegating to `OnSourceEvent`) → 1 hint → fails.
4. `AfterTheUpdateClosedItsScopeTheBookkeepingOfTheReReadDoesNotMarkTheDrafts`: `OpenOwnMutation`, drawing `ObjectModified`, `CommandEnded` (closes, own), 3 bookkeeping events, `OnIdle` → ops == `[InvalidateNavigationCaches]` (the Smoke-2 «Limpio (posiblemente obsoleto)» after Actualizar).
5. `AReadThatOnlyRaisesBookkeepingLeavesTheDraftUnmarked` (host rig): bookkeeping events after a clean draft → no drain carries `MarkDraftsPossiblyStale`; the draft is not possibly-stale.
6. Pins (pass in RED, protect ratified behaviour): drawing subject = exact effect of `OnSourceEvent` (incl. `TW/WorkspaceBridgeHintAndDrainTests.cs:106-117` list); bookkeeping still forgets the close sequence (`LastIntact == false`, like `TW/WorkspaceOwnMutationTests.cs:292-323`); bookkeeping neither opens/closes the own scope nor changes quiescence; degraded/unknown session → no effect.
7. Bridge text guard (e.g. `WorkspaceBridgeObjectSubjectGuardTests`): each of the 5 object case blocks is `handler = (s, e) => ContainSession(a, () => _core.OnObjectEvent(a.Session, a.Source, <classifier>(e.DBObject)));`, the classifier uses only `is` tests (+ the BTR flags of C3), has its own catch, names no `.Database|GetObject|OpenMode|\.Read\w*\(`; negatives. Fails on `b88dabf2`.

C3 (new class, e.g. `WorkspaceUndoDisplayRefreshTests`)
8. `AnUndoFamilyCommandThatTouchedABlockDefinitionDrainsOneDisplayRefresh` (theory `U`, `UNDO`, `REDO`, `MREDO`, `u`): CWS → `NoteCommandStart(s, name)` → `OnObjectEvent(ObjectModified, BlockDefinition)` → `CommandEnded` → `EnteringQuiescentState` → `OnIdle`: the new op exactly once, plus `InvalidateNavigationCaches` and `MarkDraftsPossiblyStale` (FRZ:237). Inert → op absent → fails.
9. `TwoUndosBeforeTheDrainCoalesceIntoOneRefresh`; `ACancelledOrFailedUndoThatTouchedADefinitionAlsoRefreshes` (diag2 behaviour; or the opposite, if decided) — RED.
10. `TheHostRunsTheRegenerationPortOnceForADrainThatCarriesTheOperation` (port double: 1 call with the session's native id; 0 calls for a drain without the op; 0 when `MayDrain` refuses: hidden panel, modal, command running; a throwing port is logged and does not fault the session if that is the decided rule) — RED.
11. Pins: `UndoingALineDrainsNoDisplayRefresh` (subject `Drawing`, incl. a layout BTR classified as `Drawing`); `ACommandOutsideTheUndoFamilyNeverRefreshes` (`EXECUTEFUNCTION`, `RACKEDITAR`, `BEDIT`, `REGEN`); `AnUndoWithoutDefinitionsNeverRefreshes` (Pan and Zoom); `ACommandStartDuringTheUndoEndsItsTracking`; `ADegradedSessionNeverRefreshes`; `AnUndoRightAfterTheUpdateIsForeignAndMarks` (IC-09, cf. `TW/WorkspaceOwnMutationTests.cs:198-210`).
12. Text guards: the `CommandWillStart` block adds exactly one `_core.NoteCommandStart(a.Session, e.GlobalCommandName);` after the ratified `NoteForeignCommandStart` line; no `Regen` in `Plugin/Workspace` or `Application/Workspace`; the Plugin/Views helper is the only panel-path file calling `Editor.Regen()` and `CTL` wires it once — fail on `b88dabf2` (helper absent).

---

## 6. Risks and ratified behaviour (facts)

- IC-09 / own scope: C2-b drops only bookkeeping events; own and foreign drawing hints are unchanged, so `TW/WorkspaceOwnMutationTests.cs` (incl. `:198-210` «an immediate Undo after the update marks») keeps passing. C2-a additionally stops bookkeeping events from forgetting `_lastAllowedClose` (T2B2B D-T2B2-10 observation changes); C2-b keeps it.
- «Every command marks» (ICL:143; `WorkspaceSession.cs:66`): unchanged by C1-C3; U/REDO keep marking (trace `:22913`, `:39647`). IC-08:143 «cualquier cambio de base de datos» changes letter for Assoc* objects (needs a declared IC).
- Ticket expiry by foreign `CommandWillStart` (`BRIDGE:206-207`, `EBC:476-487`, D-T4C-02/09): unchanged; U still counts as foreign.
- Drain operation set: C3-b adds one value to the frozen D-04 list (FRZ:228-232; `Hints.cs:14`; T4CP:845 «`DrainOperation` no cambia»). C3-a/b both change the intent of INV-03/INV-F1-T2-03/INV-F1-T3A-08 (Regen classified as a view write by the F1 guards).
- Undo: C3 changes what happens after U/UNDO/REDO/MREDO (FRZ:237-238), which is literally the «Undo» category of Owner rule D5. No Undo API is added (`TW/WorkspacePilotUpdateIntegrationGuardTests.cs:1076-1084` keeps passing; it scans `PushBackPilotHostRun.cs` and `RackPilotUpdateCommandContext.cs`). Fact: a REGEN command between U and REDO kills REDO; the programmatic `Editor.Regen()` kept REDO in diag2 (validated only in the Idle-callback position, before the drain).
- C2 also falls under D5's «conflicto/stale» category by its text; the Owner's fix order names F4-S2-01 explicitly.
- C3 depends on C2: the regen itself raises Assoc events (trace `:22909-22911`).
- Coverage: with (b) the regen happens only when the drain runs (panel visible, active session quiescent, no RackCad modal, no pending allowed close: `EBC:557-573`, `:431-443`); diag2's Idle hook ran regardless. The bridge exists only after RACKPANEL, so the refresh after U (also of classic RACKEDITAR, which redraws named definitions too) depends on the panel having been opened — the classic path was not measured.
- Cost: ≈1.4 s per regen on D1 (trace); the classifier touches `e.DBObject` on every object event (15 872 events during one U); the cost of creating those wrappers was not measured separately (diag2's ≈14 s Actualizar included DIAG logging).
- Precision of the C3 trigger: type-only vs `IsLayout`/xref/anonymous property reads (§3.2) is undecided and the LINE case is unmeasured; property reads in a handler need an explicit IC-04 reading.
- Exceptions in handlers degrade the session (`BRIDGE:384-401`); exceptions in the host drain fault the session (`WorkspaceHostCore.cs:1991-1994`): both new code paths need local containment.
