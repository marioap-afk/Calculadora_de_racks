# I-64 — Paquete del contrato de F4-T4C (integración en el host del Actualizar del piloto)

**Estado:** v5; H0 hecho. **F4-T4C1-HOST se emite el 2026-11-15** (F4-E65) por la orden del Owner «Emitir T4C1» (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c1-emission-authorization.md`, literal). F4-T4C2-PANEL no está emitida.
- La prepara la sesión principal por la orden del Owner de preparación de F4-T4C: «PACKAGE PREPARATION + INDEPENDENT REVIEW ONLY» y «Do NOT emit the T4C contract yet».
- **v1:** la revisión adversarial independiente la juzgó **EMITIBLE TRAS CORRECCIONES (0/12/16)** (`independent-review-v1.md`, citada como IR). La v1 se conserva como `_package-v1.md`.
- **v2:** incorporó los 28 hallazgos (§10.1). Cambió el diseño de D-T4C-02, D-T4C-04 y D-T4C-09 y añadió D-T4C-20. Se conserva como `_package-v2.md`.
- **v3:** la re-revisión acotada de la v2 (`independent-review-v2.md`, citada como RR) da **EMITIBLE TRAS CORRECCIONES**. Tiene 0/7/8 hallazgos nuevos, y de los 28 anteriores da 26 resueltos y R-12 parcial, por N-02. La v3 incorpora los 15 hallazgos nuevos (§10.2), comprobando antes en el código cada hecho que alegan.
  - **Cambian** D-T4C-02, D-T4C-03, D-T4C-04, D-T4C-05, D-T4C-08 a D-T4C-14, D-T4C-16, D-T4C-17 y D-T4C-20.
  - **No cambian** D-T4C-01, D-T4C-06, D-T4C-07, D-T4C-15, D-T4C-18 ni D-T4C-19.
  - RR §7 no pide otra re-revisión de arquitectura. Basta con comprobar las siete correcciones importantes antes de volver al Owner.
  - Se conserva como `_package-v3.md`.
- **v4:** la comprobación dirigida de la v3 (`independent-check-v3.md`, citada como CK) da **EMITIBLE TRAS CORRECCIONES (0/1/2)**. Da por resueltos N-01 a N-15 y R-12. La v4 incorpora sus tres hallazgos (§10.3), sin cambio del diseño de base:
  - **C3-01:** V3 se redefine en D-T4C-09; para N-10, «cerrar en `Settle`» va como variante y como desviación expresa de C-F4-04; se ajustan la fila 5 de H0, C12 y O-02;
  - **C3-02:** el cierre por degradación pasa a `Degrade`, el punto único (D-T4C-09, D-T4C-17 y D-T4C-20);
  - **C3-03:** se precisa la caducidad dentro de `RequestUpdate` cuando el billete es de otro rack (D-T4C-02, D-T4C-03, D-T4C-05, D-T4C-11 y D-T4C-17).
  - El Owner dispuso el §9 de la v4: aceptó todas las recomendaciones de O-01 a O-15 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c-package-decisions.md`). La v4 está custodiada en `de27426b` (F4-E62 y la evidencia 205).
- **v5:** incorpora la sonda **T4C-H0**, custodiada en `44fe029d` (F4-E63 y la evidencia 206, en `docs/automation/evidence/I-64-pilot/F4-T4C-H0/`), y la **decisión del Owner sobre su C12**, «Completar o comando ajeno» (`owner-f4t4c-h0-c12-decision.md`, custodiada en `F4-DRAFTS/coordinator/` con la v5, F4-E64). Se conserva la v4 como `_package-v4.md`.
  - **Pasan de [H] a [M]** los hechos que H0 midió (§2.6, H-39 a H-52).
  - **Cambian** D-T4C-02 (la cota del billete), D-T4C-03, D-T4C-04 (la guarda para `ExecutionResult`), D-T4C-05, D-T4C-08 (solo un hecho medido), D-T4C-09 (la vía A con el orden medido y el comando ajeno), D-T4C-10, D-T4C-15 (Undo medido), D-T4C-16 (la tabla rellena), D-T4C-17 y D-T4C-20, además del §5, el §6, el §7, el §9 y el §10.4.
  - **No cambian** D-T4C-01, D-T4C-06, D-T4C-07, D-T4C-11 a D-T4C-14, D-T4C-18 ni D-T4C-19.
- Los mapas y los documentos de revisión se custodian en `I-64-pilot/F4-DRAFTS/t4c/` (F4-E62 y F4-E64). Las versiones v1 a v4 del paquete quedan solo como historia en la carpeta de trabajo de la sesión; la v4 está en la historia de Git de este archivo (`de27426b`).
- No declara ningún cierre. El Owner ratificó O-17 y O-17 bis («Ratificar»), dispuso O-18 aparte («Registrar aparte») y ordenó la emisión de T4C1 (F4-E65). IC-09 queda registrada en `docs/initiatives/I-64-implementation-clarifications.md`. T4C2 y Smoke-2 necesitan su propia orden (O-09).

**Base medida:** `b15072ff`, la custodia del cierre de T4B, que solo toca `docs/`. El producto es el de `4314c180`: `git diff 4314c180 b15072ff -- src tests` sale vacío (M1). La rama está ahora en `44fe029d`, y `git diff b15072ff 44fe029d -- src tests` también sale vacío: las anclas de `src` y `tests` siguen valiendo, y las de H0 se citan en `44fe029d`. Todas las anclas `ruta:línea` son relativas al repositorio y están medidas en `b15072ff`. La sesión ha vuelto a comprobar en el worktree las que fundan una decisión, también las nuevas de la v2.

**Mapas** (custodiados en `I-64-pilot/F4-DRAFTS/t4c/`; se citan como M1 a M4):
- M1, `map-1-t4b-seam-writer.md`: la costura de T4B, el escritor y la ausencia de una segunda vía;
- M2, `map-2-command-context-lock-undo.md`: el contexto de comando, el bloqueo y Undo;
- M3, `map-3-own-mutation-ui-lifecycle.md`: la mutación propia, el ciclo de vida, la UI y el diagnóstico de T4A2;
- M4, `map-4-t1b-seams-baseline.md`: T1B, el alcance prohibido y la línea base.

El censo de G-9 lo hace un script de solo lectura de la misma carpeta, `_v2_g9_census.py`.

**Abreviaturas:**
- `OO` = la orden del Owner de preparación de T4C (`I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c-preparation-authorization.md`, literal); se cita por sus apartados;
- `IR` = la revisión independiente de la v1; `RR` = la re-revisión de la v2; `CK` = la comprobación dirigida de la v3;
- `T4BP` = `docs/initiatives/I-64-f4-t4b-gate-contract-package.md`;
- `T4BC` = `docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/task-close-package.md`;
- `T4P` = `docs/initiatives/I-64-f4-t4-gate-contract-package.md`;
- `F4P` = `docs/initiatives/I-64-f4-gate-contract-package.md`;
- `FRZ` = `docs/initiatives/I-64-proposal-v5.md`;
- `ICL` = `docs/initiatives/I-64-implementation-clarifications.md`;
- `DSP` = `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md`;
- `A2P` = `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md`;
- `TW` = `tests/RackCad.Tests/Workspace/`; `UW` = `tests/RackCad.UI.Tests/Workspace/`;
- `CORE` = `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- `RUN` = `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs`;
- `HOSTRUN` = `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotHostRun.cs`, el archivo nuevo de T4C;
- `ADAPTER` = `src/RackCad.Plugin/Views/RackPilotUpdateCommandContext.cs`, el archivo nuevo de T4C;
- `WRITER` = `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`;
- `CTL` = `src/RackCad.Plugin/Workspace/WorkspaceController.cs`;
- `EBC` = `src/RackCad.Application/Workspace/EventBridgeCore.cs`;
- `DS` = `src/RackCad.Application/Workspace/Drafts/DraftSession.cs`; `DR` = `.../Drafts/DraftRegistry.cs`;
- `FVM` = `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`; `BVM` = `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`.

**Presupuesto propuesto:** 1 de 3 por tarea de producto (§6).

---

## 1. Autoridades

- **`OO`, que manda sobre todo lo demás:**
  - «PRIMARY OBJECTIVE» y «AUTHORITATIVE MUTATION LOGIC»: T4C integra T4B y no implementa un segundo algoritmo de mutación. «Do not reopen T4B merely because T4C integrates it»;
  - «EXECUTE IN COMMAND CONTEXT», «DOCUMENT LOCK», «FINAL REVALIDATION», «RUN-SCOPED WRITER» y «ONE ACTUALIZAR»;
  - «T4B I-01», «T4B M-01 / M-03 / M-06» y «T4A2 DIAGNOSTIC»;
  - «COMMIT FAILURE», «POST», «OWN-MUTATION» y «UNDO»;
  - **«T4C AUTOMATED TESTS»** (las 20 pruebas mínimas) y **«NO SECOND WRITE PATH»**;
  - **«T1B COMPATIBILITY»**: si T4C cambia un comportamiento caracterizado, STOP;
  - «PACKAGE REVIEW», **«RETURN BEFORE EMISSION»** y «SMOKE-2», que llama a Smoke-2 «the first physical validation of the new pilot-specific authored write path».
- **`T4P` §7:**
  - R-T4-05 (`:547-550`), R-T4-06 (`:552-558`), R-T4-07 (`:560`), R-T4-08 (`:562-566`), R-T4-09 (`:568-571`), R-T4-10 (`:573`), R-T4-11 (`:575-579`) y R-T4-13 (`:583`);
  - §8, Smoke-2 (`:585-587`).
- **`F4P`:** C-F4-02 (`:59`), C-F4-04 (`:60`), C-F4-06 (`:58`) y H-13 (`:32`).
- **`FRZ`:**
  - D-04: los manejadores (`:216-219`) y la mutación propia, O-15 (`:241-243`);
  - D-12, H-LOCK (`:494-498`);
  - D-13 (`:500-545`): el residuo OM-5 (`:504`), el paso 2 (`:510-511`), POST (`:519`), la tabla por frontera (`:521-532`), Undo (`:542-543`) y Aplicando (`:545`);
  - INV-08 (`:746`), INV-28 (`:766`), INV-30 (`:768`) e INV-32 (`:770`);
  - §12.1 (`:817-834`), con los momentos de los hitos en `:821-822` y la build del hito en `:828-830`; y §12.4 (`:877-898`).
- **`ICL`:** IC-01 (`:5-41`, con su alcance en `:17`), IC-04 en su redacción aceptada (`:73-85`) e IC-08 (`:131-153`, con la obligación en `:145-151`).
- **`T4BP`:** D-T4B-02, D-T4B-03, D-T4B-07 (`:546-605`) y D-T4B-09 (`:620-651`); §7 (`:794-809`); §8 (`:811-819`); §9, I-2, I-3, I-8, I-9 e I-11 (`:836-845`).
- **`T4BC`:**
  - el §5, I-01;
  - el §6, M-03: que T4C muestre la clasificación quedó abierto;
  - el §7: los arrastres M-01, M-03 y M-06, Smoke-2, H-1 y PD-F4-07.
- **`DSP`:** F4-E09 (`:102-109`); F4-E51 (`:1694`, `:1701` y `:1707`); y F4-E59 (`:2082` y `:2086`).
- **`A2P`:** el §8, con «D-1 si añade una lectura» (`:660`), y D-T4A2-06.
- **C2-M-3 a C2-M-7** (R-T4-13): su tabla está en `docs/automation/evidence/I-64-pilot/F4-DRAFTS/t4b/map-3-carries-guards-rules.md:156-162`.
- **F4-T1B**, la autoridad de compatibilidad: `docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/task-close-package.md` y CT-24 a CT-27 (`tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`).
- **Las decisiones del Owner sobre este paquete:**
  - la disposición del §9 de la v4 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c-package-decisions.md`): «H0 con dispensas» (O-01), «Vía A + IC-09» (O-02), «E-a + D-1» (O-03 y O-04) y «Aceptar todas» (O-05 a O-15);
  - **la decisión sobre el C12 de H0**, «Completar o comando ajeno» (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c-h0-c12-decision.md`, que se custodia con esta v5): el billete se libera con la compleción de `ExecutionResult`; como respaldo, si el callback no empezó, caduca con el primer `CommandWillStart` de la sesión de un comando que no sea `EXECUTEFUNCTION`, con el cierre del documento o con la retirada de la sesión; y las entradas en reposo dejan de contar.
- **T4C-H0:** su custodia en `44fe029d` (§2.6).

---

## 2. Hechos medidos

Aquí solo van los hechos que fundan una decisión. El detalle está en los mapas.

### 2.1 La costura de T4B

**H-01. La firma** es `public EditUpdateOutcome TryUpdatePilot(EditIntentRequest request, IPushBackPilotUpdatePort port)` (`CORE:805`).
- La fija por reflexión `TW/WorkspaceEditIntentHostTests.cs:1003-1010`.
- La misma prueba prohíbe cualquier método con «Update» en el nombre de `IWorkspacePilotEditPort` (`:1012`).
- Hoy no tiene llamador de producto, y nadie construye `RackPilotUpdateWriter` (M1 H-06).

**H-02. La forma del cuerpo** (`CORE:805-905`):
- **la puerta** (`:815-816`): cualquier valor distinto de `Open`, también `Waiting`, sale como `Rejected(reason)`;
- **la ranura** (`:837-843`);
- **la contención como `Failed`**, solo antes de la ranura (`:845-849`);
- **la corrida** (`:851-866`): el valor por defecto `NotAppliedFailure(UpdateNotRunReason)` (`:852`), su `catch` (`:858-862`) y `CompleteApply` en `finally` (`:863-866`);
- **la respuesta** (`:868-904`) queda **fuera de todo `try`** (M2 H2-40):
  - `ClosedDetached` (`:869`);
  - `!closed.Accepted` responde NotAppliedFailure sea cual sea `closed.Kind` (`:870`). Es `NotApplying`, que sale cuando el borrador no tiene la ranura (`DS:536-537`);
  - `ClosedInError` pierde la clasificación (`:872-877`).

**H-03. `UpdateGate`** (`CORE:908-933`) mira el documento activo (literal en `:915`), el modal de RackCad (literal en `:921`) y `Authorize` (`:925-929`).
- No mira el reposo ni el lector.
- Su constante de respaldo es `UpdateNotRunReason` (`CORE:48`).
- Son los tres textos de I-01.

**H-04. `EditUpdateOutcome`** (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`):
- 19 estados, con `Unavailable` inerte por defecto (`:381-384`);
- `Closed(...)` guarda las listas **sin copiarlas** (`:477-483`);
- `ClosedInError(reason)` no lleva clasificación, avisos ni diferencias (`:472`);
- `Failed(reason)` es «A failure of the host BEFORE the update began» (`:474`; M2 H2-39).

### 2.2 El escritor de T4B

**H-05. El puerto no tiene campos y lee el documento activo:**
- la clase del puerto es `RackPilotUpdateWriter` (`WRITER:32-38`);
- su comentario dice que nunca toma el bloqueo (`:24-27`);
- `Begin` resuelve `AcApplication.DocumentManager.MdiActiveDocument` (`:37`);
- el ámbito compara **una vez** el `UnmanagedObject` con `AuthorizedDocumentNativeId` (`:68-72`);
- `Begin` se puede llamar cualquier número de veces, aunque la corrida solo lo llama una vez (`RUN:248`).

**H-06. La guarda de T4B sobre el puerto** (`TW/WorkspacePilotUpdateGuardTests.cs`):
- la clase del puerto no puede nombrar `Transaction`, `DBObject`, `Document`, `ObjectId`, `Database` ni `PreparedViewRedraw`, ni declarar campos (`:185-188`);
- su negativa inyecta literalmente `private Document _document;` tras la llave de la clase (`:267-276`);
- el escritor no nombra `LockDocument`, `async`, `await` ni `ExecuteInCommandContextAsync` (`:171`);
- `Begin` tiene que devolver `return new RackPilotUpdateScope(` (`:216`);
- **ningún archivo de la carpeta del piloto** contiene la palabra «Transaction», ni siquiera en un comentario, y la carpeta tiene al menos 11 archivos (`:117-124`).

**H-07. Lo que escribe una corrida** (M2 H2-28 y H2-29):
- **lo authored:** una sola transacción poseída, con un `StartTransaction` y un `.Commit(` en todo el escritor (`TW/WorkspacePilotUpdateGuardTests.cs:208-213`);
- **el residuo OM-5:** hasta N `WblockCloneObjects` de la importación, sin transacción;
- **POST:** hasta una transacción de purga y hasta N `SyncName`;
- **el regen.**

**H-08. Qué toma bloqueo propio:**
- **solo `CaptureDrawing` y `SyncName`:** `src/RackCad.Plugin/Views/RackSiblingScan.cs:103` y `src/RackCad.Plugin/Drawing/RackBlockRenamer.cs:32`;
- **la importación y la purga no lo toman:** `git grep LockDocument` no da nada en `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs` ni en `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`;
- las dos dependen del bloqueo exterior de T4C.

### 2.3 Las guardas que acotan la integración

**H-09. El contexto de comando y la asincronía:**
- `ExecuteInCommandContextAsync` está prohibido en **todo `src`**, sin contar los comentarios (`TW/WorkspaceNavigationHostTests.cs:436`).
- `async` y `await` solo se prohíben en `Src.PluginFiles()` y `Src.ApplicationFiles()` (`:435`). Esas dos son `src/RackCad.Plugin/Workspace/` y `src/RackCad.Application/Workspace/` (`TW/WorkspaceHostSourceSupport.cs:60-62`).
- **`src/RackCad.Plugin/Views/` no entra en ninguna guarda de asincronía de carpeta.** El escritor tiene la suya, por archivo (`TW/WorkspacePilotUpdateGuardTests.cs:171`). La v1 decía «ninguna guarda» (IR M-04).
- **El censo de `Task` y `async` de hoy** en las carpetas del panel (IR R-11), que solo usa la navegación:
  - `src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs`: una línea con `Task` (`:94`);
  - `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`: dos (`:127`, `:148`);
  - `src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs`: cinco con `Task` y dos con `async` o `await` (`:194-231`; el `await` está en `:231`, RR N-14).
  
  Las dos carpetas Workspace no tienen ningún `async` ni `await` (`git grep -c`).
- **El precedente de las prohibiciones** de un archivo con la API es la regla del host de navegación: `.Wait(`, `.Result`, `GetAwaiter`, `GetResult`, `ConfigureAwait`, `Task.Run`, `Task.Factory`, `StartNew`, `ContinueWith` y `async void` (`TW/WorkspaceSourceGuardExceptionTests.cs:270`).

**H-10. Las fronteras de nombres:**
- ningún archivo del Plugin fuera de `src/RackCad.Plugin/Workspace/` nombra `AutoCadEventBridge`, `RackCad.Plugin.Workspace` ni `RackCad.Application.Workspace` (`TW/WorkspaceBridgeSourceGuardTests.cs:361-366`);
- nada fuera de las dos carpetas Workspace nombra `WorkspaceController`, `WorkspaceHostCore`, `WorkspaceHostPort` ni `WorkspaceSelectionReader` (`TW/WorkspaceHostSourceGuardTests.cs:155-166`).

**H-11. `src/RackCad.Plugin/Workspace/`:**
- tiene exactamente nueve archivos (`TW/WorkspacePaletteHostSourceGuardTests.cs:774-782`, entre otras cuatro guardas, M1 H-10);
- no guarda ningún objeto del host en un campo, y todo `LockDocument` va en un `using` (`TW/WorkspaceHostSourceGuardTests.cs:467-477`).

**H-12. El núcleo y el controlador:**
- ninguno de los dos nombra `ExecuteInCommandContextAsync`, `async`, `await`, `Task`, `Dispatcher` ni `Timer` (`TW/WorkspacePilotPrepareGuardTests.cs:420-429`);
- ninguno nombra API de escritura, `LockDocument` incluido (`TW/WorkspaceLifecycleGuardTests.cs:679-694`). Esa guarda mira también `AutoCadEventBridge.cs`;
- el adaptador del controlador solo reenvía cada operación de `Operations` (`TW/WorkspacePilotEditGuardTests.cs:509-519`) y se construye literal (`:506-507`);
- el controlador tiene tres `+=`, ningún `-=`, un `event`, y no nombra `Task` (`:526-529`).

**H-13. Los campos del núcleo y el precedente del setter:**
- ningún campo de instancia del núcleo puede ser de un tipo de `RackCad.Application.Systems*` (`TW/WorkspaceEditIntentHostTests.cs:1023-1043`);
- el precedente de asignación es `SetEditIntentReader` (`CORE:275-276`). Se asigna en el getter de `Navigation` (`CTL:105`), con su censo de dos archivos (`TW/WorkspaceEditIntentGuardTests.cs:490-505`);
- en `src/RackCad.Application/Workspace/` solo pueden nombrar el piloto el núcleo, la interfaz del lector y `WorkspaceEditIntent.cs` (`TW/WorkspaceEditIntentGuardTests.cs:509-523`).

**H-14. El puerto del panel:**
- tiene 15 operaciones (`TW/WorkspacePilotEditGuardTests.cs:44-61`), de las que seis son lecturas (`:64`);
- las que no son lecturas no nombran `PassGate`, `_probe` ni `_bridge` (`:248-253`), y las de memoria no autorizan (`:256-260`);
- las que mutan autorizan y miran `_pilotReadInProgress` (`:262-268`);
- **dos guardas se contradicen para T4C:**
  - la guarda del puerto ya admite `Update` al final de `PilotBoundaryKind` y una operación de actualización (`:151-160`);
  - la reflexión de `TW/WorkspaceEditIntentHostTests.cs:1012` prohíbe «Update» en cualquier nombre del puerto;
- los implementadores son seis, no cinco: `T4BP:114-120` olvida el núcleo (M1 H-09, M3 H-M3-25).

**H-15. Los tres VM del panel** no pueden tener `+=`, `-=`, `event`, `delegate`, `Dispatcher`, `Timer`, `Task`, `async`, `await`, `Keyboard` ni foco (`TW/WorkspaceHostHardeningGuardTests.cs:758-760`, `:779`). **Ningún canal de empuje hacia la UI es compatible con las guardas.**

**H-16. `PortMutatingOperations`** (`TW/WorkspaceLifecycleGuardTests.cs:463-477`) exige que cada cuerpo cambie un borrador (`:494`) y termine la marca de limpio (`:495`), y fija `Skip(5).Count() == 4` (`:540`). **Una operación que no cambia ningún borrador no puede entrar** (IR R-07 (a)).

**H-17. El estado de comando del núcleo del puente:**
- `SetQuiescent` (`EBC:437-442`) vuelve **antes** de cambiar nada si el valor no cambia (`:439`);
- sus dos casos están fijados literalmente: `case BridgeSource.CommandWillStart: return SetQuiescent(state, false);` y el de `EnteringQuiescentState` (`TW/WorkspaceHostC2SourceGuardTests.cs:208-209`; `TW/WorkspaceLifecycleGuardTests.cs:176-177`);
- el cuerpo de `SetQuiescent` no puede nombrar `EnqueueHint`, `SyncIdle`, `_source.`, `TryDrain`, `_active`, `_registry` ni `_log` (`TW/WorkspaceHostC2SourceGuardTests.cs:206`, `:210`);
- el cuerpo de `SessionState` (`EBC:28-40`) no puede contener `SessionId?`, `Allowed` ni `Close` (`TW/WorkspaceLifecycleGuardTests.cs:115-116`).

**H-18. La regla de olvido de la secuencia de cierre:**
- trece entradas de `EBC` que cambian su estado olvidan `_lastAllowedClose` exactamente una vez, antes de cualquier rama (`TW/WorkspaceLifecycleGuardTests.cs:113`, `:124-139` y `:167-173`);
- ningún nombre nuevo puede contener `Destroy`, `Detach`, `OnIdle` ni `Stop` (`:118-120`).

**H-19. Las superficies públicas:**
- la de `AutoCadEventBridge` es una lista exacta de nueve nombres (`TW/WorkspaceBridgeFacadeSourceGuardTests.cs:91-104`), y solo cuenta los miembros `public`;
- la de `EventBridgeCore`, también exacta (`:206-224`);
- el doble de `IWorkspaceBridgeLink` está en `TW/WorkspaceHostTestSupport.cs:53`.

**H-20. Los nombres de tipo en `RackCad.Application.Workspace`:**
- ningún tipo, anidados incluidos, puede llamarse con `Subscription`, `Editor`, `Draft` ni `Tab` (`TW/WorkspaceModelBoundaryTests.cs:94-104`);
- tampoco con `ProjectSummary`, `Metric`, `Provider`, `TotalRacks`, `Aggregat` ni `Bom`, ni ninguno de sus miembros públicos (`:84-90`).

### 2.4 El host y el panel

**H-21. El clic del panel corre en síncrono**, en el hilo de la UI y en contexto de aplicación (M2 H2-16):
- DIAG-1 mide `isAppCtx=True` al entrar;
- el sondeo de reposo es `Editor.IsQuiescent && !_bridge.IsCommandActiveInActiveSession` (`CTL:367-373`);
- **[I]** dentro de `EXECUTEFUNCTION` falla siempre;
- `PassGate` (`CORE:1481-1515`) comprueba el lector, el documento, el modal, el reposo y `Authorize`, con sus textos ya existentes;
- los avisos `StateChanged` salen de `CommandWillStart` y de `EnteringQuiescentState` (`CTL:217-219`, `:250-265`; `EBC:317-320`).

**H-22. El callback de `ExecuteInCommandContextAsync`** (M2 H2-10 a H2-12):
- **lo medido:** corre en `EXECUTEFUNCTION`, fuera del contexto de aplicación; vacía PICKFIRST con dos `*Cancel*` (`docs/automation/evidence/I-64-pilot/F2-SMOKE-NAV/R20261110T064058Z-c883/operator/session-log.md:10`, `:20`); y un `LockDocument` dentro no falló;
- **lo que hasta la v4 no estaba medido, y H0 midió** (§2.6): el callback es diferido (H-39); la llamada se completa siempre (H-40), aunque no devuelve un `Task` sino un `ExecutionResult` (H-50); y los `*Cancel*` no dan pistas (H-42).

**H-23. Undo:**
- `src` no tiene ninguna marca ni API de Undo, ningún `SendStringToExecute` y ningún `CommandFlags` (M2 H2-30);
- hay exactamente 40 `[CommandMethod(` (`tests/RackCad.Tests/SelectiveEditorOpenTests.cs:564-572`).

**H-24. Las pistas** (`EBC:306-338`; `src/RackCad.Application/Workspace/WorkspaceSession.cs:25-58`):
- solo llevan la sesión y se agrupan por tipo;
- `CommandCancelled` y `CommandFailed` encolan `CommandEnded` (`EBC:324-327`);
- `DatabaseObjectChanged` y `CommandEnded` producen `InvalidateNavigationCaches` y `MarkDraftsPossiblyStale`, e `ImpliedSelectionChanged` produce la lectura de la selección (`WorkspaceSession.cs:47-56`);
- el marcado alcanza a todos los borradores del piloto de la sesión (M3 H-M3-07);
- no existe ningún ámbito de mutación propia (M3 H-M3-11).

**H-25. Los descartes y el cierre:**
- `DiscardRack` y `DiscardDocument` solo miran `_pilotReadInProgress` (`CORE:1156-1168`, `:1180-1188`), y el registro desregistra el borrador sin mirar su estado (M3 H-M3-14). Lo caracterizan `TW/WorkspaceDraftRegistryContainmentTests.cs:383-411` y `TW/WorkspacePilotUpdateHostTests.cs:368-382`;
- `RACKPANELDESCARTAR` llama a `DiscardDocument` y presenta su `Busy` con el motivo (`src/RackCad.Application/Workspace/WorkspaceDocumentDiscardCommand.cs:146`, `:176-177`);
- **el cierre se veta** con un borrador en riesgo (`CORE:1378-1392`);
- `BVM` ya presenta un `Busy` de los descartes con su motivo (`BVM:945`).

**H-26. Los cambios del borrador que siguen admitidos fuera de Applying:** `SetPendingText` (`CORE:1045-1060`), `CommitField` (`:1074-1090`), `CrossBoundary` (`:1105-1133`), `TryKeepMine` y `TryDiscardAndReload` (M3 H-M3-17).

**H-27. La frontera bloqueante** (ADR-0032 D6):
- `DraftBoundary.Update` es bloqueante (`DS:87-92`);
- en `CrossBoundary` (`DS:377-404`):
  - sin texto pendiente, `Proceeded`;
  - si no se puede editar, `Kept`;
  - con algún texto inválido, `Aborted`;
  - **con texto válido, lo aplica y da `Proceeded`**;
- el puerto añade `Busy` con una lectura en curso (`src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:147-163`; `CORE:1127-1128`);
- hoy el núcleo solo traduce `TabChange` y `RackChange` (`CORE:1110-1124`, `:1131`).

**H-28. La UI:**
- `PilotBoundaryKind` no tiene `Update` (`IWorkspacePilotEditPort.cs:166-175`);
- la cola del constructor de `PilotFieldView` está fijada (M3 H-M3-29);
- los textos del Desconocido ya existen (`FVM:56-57`);
- el patrón `IsMine` está en `FVM:412-416`;
- el gesto `CheckNow` comprueba primero su botón (`FVM:397-399`);
- `src/RackCad.UI/Workspace/` tiene ocho archivos (`git ls-files`).

**H-29. La identidad nativa** de un documento puede reciclarse después de destruirlo (`WorkspaceSession.cs:18`).

**H-30. G-9:** solo cuatro archivos de `src/RackCad.Plugin/` y `src/RackCad.Application/` nombran el piloto y usan alguna API de escritura o de bloqueo. Censo de `_v2_g9_census.py`, sin comentarios:
- `WRITER`;
- `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`: un `LockDocument` y un `StartTransaction`, de solo lectura (`:160-161`);
- `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`: dos `LockDocument` (`:163`, `:232`);
- ningún archivo de Application.

**H-31. La línea base de `4314c180`,** de la CI `43668817337` (M4 §4; IR §2.8 recontó los filtros y tres clases):
- Core: 18684/18684, con `RackCad.Tests.Workspace` = 3103;
- UI: 2310 resultados, 2293 en verde y 17 omitidas, con `RackCad.UI.Tests.Workspace` = 601;
- T1B: 55 de UI y 10 de Core;
- `CallerOwnedFacadeGuardTests`: 81.

**H-32. T1B** (M4 §2):
- ninguna guarda de T1B lee las superficies de T4C;
- la única que alcanza todo `src` es CT-19 (`InsertRequestedRaised +=`);
- CT-27 (1) congela las firmas de las fachadas que consume el escritor.

**H-33. Las reglas de host para un hito:**
- los momentos de los hitos (`FRZ:821-822`);
- la build del hito, con «UI Full local» y la CI exacta (`FRZ:828-830`; `docs/guias/validacion-manual-autocad.md:427-428`);
- el Owner ejecuta (`FRZ:833`);
- **los datos de máquina los mide la sesión** y el Owner observa (`docs/automation/evidence/I-64-pilot/F1-SMOKE-1/R20261107T171052Z-6862/preflight/baseline-verification-session.txt:15-16`);
- el precedente de un build de diagnóstico sin commit, con la fuente restaurada y los SHA-256 de los dibujos comprobados (`…/F2-SMOKE-NAV/R20261110T064058Z-c883/operator/session-log.md:18-19`);
- **§12.1 carga «el DLL Debug del worktree de I-64»** (`FRZ:830`; RR N-12).

**H-34. Los comandos de `Plugin/Workspace`:** `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs` tiene dos `[CommandMethod(`, `RACKPANEL` (`:22`) y `RACKPANELDESCARTAR` (`:38`). Entran en el censo de 40 (H-23) (RR N-02).

**H-35. «Conservar» y «Comprobar ahora» sobre un Dirty:** `DraftSession.ConfirmBase` admite Clean, Dirty y Conflict, y con una diferencia pasa el borrador a Conflict (`DS:743-760`). Así que `TryConfirmBase` sí puede cambiar un Dirty (RR N-08).

**H-36. La activación y el modo degradado** (RR N-09):
- `OnDocumentActivated` fija `state.Quiescent` directamente, sin pasar por `SetQuiescent` (`EBC:223-241`, `:240`);
- `OnSourceEvent` vuelve antes con una sesión degradada (`EBC:312`), así que una sesión degradada no recibe ninguna pista ni ningún cambio de reposo;
- `EBC.IsDegraded(SessionId)` existe y es pública (`EBC:144`);
- `Authorize` no rechaza una sesión degradada (`src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs:39-46`).

**H-37. La guarda de las cuentas por operación** (`TW/WorkspaceEditIntentGuardTests.cs:688-700`) exige cero diagnósticos de admisión en `IntentBody()`, `CheckBody()`, `KeepBody()` y `DiscardAndReloadBody()` (`:699-700`). Un diagnóstico en la comprobación la rompe (RR N-05).

**H-38. La documentación de `ClosedInError`** dice «the draft is now in Error» (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:419-420`) (RR N-13).

### 2.5 Correcciones a los mapas, a `T4BP` y a la v1

| Dónde | Lo que dice | Lo medido |
|---|---|---|
| M4 §0 y R-10; v1 | La guía exige «UI Full local» en `:424-425` (M4) o en `:428` (v1) | La frase va de `docs/guias/validacion-manual-autocad.md:427` a `:428` (IR M-04) |
| `T4BP:389` | «Los bloqueos anidados de `CaptureDrawing`, `SyncName` y la purga ya existen» | La purga no toma bloqueo (H-08) |
| `T4BP:114-120` | Cinco implementadores de `IWorkspacePilotEditPort` | Seis, con el núcleo (H-14) |
| `T4BP:81-83` | `DS:508-524` y `:515` | `DS:510-526` y `:517` (M3 H-M3-43) |
| M1 H-10 y M3 H-M3-24 | La guarda que admite `Update` y la que lo prohíbe, por separado | Se contradicen para T4C (H-14) |
| v1 H-09 | «`Plugin/Views` no entra en ninguna guarda de asincronía» | Ninguna de carpeta. El escritor tiene la suya (IR M-04) |
| v1 D-T4C-02 e INV-03 | «Ningún `Task` en las carpetas del panel; el único archivo con `Task` es el adaptador» | La navegación usa `Task` y `async` (H-09; IR R-11) |
| v1 D-T4C-02 | `RequestUpdate` en `PortMutatingOperations` | No se puede satisfacer (H-16; IR R-07 (a)) |
| v1 D-T4C-11 | «Con texto pendiente, `Kept`» | Con texto válido, `Proceeded` tras aplicarlo (H-27; IR R-09) |
| v1 D-T4C-13 | «`git grep EnterError` en `src` = solo su definición» | La UI tiene un `EnterError` privado, ajeno (`src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs:83`, `:292`; IR M-02) |
| v1 §5 | «R-T4-13: T4C no toca esas superficies» | Toca la de C2-M-5 (IR M-05; D-T4C-13) |
| v1 §6 | «Las demás 64 entradas de T4B siguen» | Son 65, F28 incluida (IR M-06) |
| v1 D-T4C-17 | «No hay otros» cambios de guarda | El inventario estaba incompleto (IR R-07). Lo completa D-T4C-20 |
| v2 D-T4C-02 | La caducidad «o la siguiente activación del documento» | La activación no cuenta como entrada en reposo (H-36; RR N-09) |
| v2 D-T4C-08 | «El resto no puede cambiar un Dirty», `TryConfirmBase` incluida | `TryConfirmBase` sí puede pasarlo a Conflict (H-35; RR N-08) |
| v2 D-T4C-12 | «La cuenta de lecturas por operación no cambia» (`:688-700`) | Cambia en `:699-700` (H-37; RR N-05) |
| v2 D-T4C-14 | G-6 con la lista de G-5 y cuatro excepciones | Falta `WorkspacePanelCommand.cs`, con sus dos `[CommandMethod(` (H-34; RR N-02) |
| v2 §6 | El prohibido incluía «las guardas que no cambian», tres de ellas en archivos del permitido | El prohibido solo lleva archivos que no están en el permitido (RR N-06) |
| v1 a v4, D-T4C-04 | `ExecuteInCommandContextAsync` devuelve un `Task`, con una `ContinueWith` | Devuelve `DocumentCollection.ExecutionResult` (H-50) |
| v4, D-T4C-02 | La cota del billete por la primera entrada en reposo | H0 la invalidó (fila 3, H-41). La sustituye la regla del Owner |

### 2.6 Lo que midió T4C-H0 (de [H] a [M])

**La sonda.** T4C-H0 se hizo el 2026-11-15, según O-01 («H0 con dispensas»). Su custodia está en `44fe029d`, en `docs/automation/evidence/I-64-pilot/F4-T4C-H0/` (F4-E63 y la evidencia 206):
- `H0R` = `h0-result.md`, el resultado fila por fila;
- `H0T` = `h0-trace.txt`, la traza de 2192 líneas;
- `H0N` = `h0-harness-notes.md`, las notas del arnés.

Sus cinco corridas son P1 y P2 (el contexto vacío, sin selección y con ella), P3 (Actualizar con el borrador limpio) y P4 y P5 (Actualizar con un cambio, `Applied`) (`H0R:21-29`). Es un diagnóstico, no una validación: nada cuenta como evidencia de cumplimiento, y Smoke-2 sigue siendo la primera validación física. Las desviaciones del procedimiento están en H-52.

**H-39. El callback es diferido** (fila 1, `H0R:35`). Lo es en las cinco sondas: la llamada vuelve con `IsCompleted=False` y el callback corre entre 49,6 y 145,8 ms después. La UI pinta uno o dos fotogramas mientras tanto (`H0T:127`, `:134`, `callReturnedBefore=True`).

**H-40. La llamada se completa siempre** (filas 2 y 10, `H0R:36`, `:44`). `OnCompleted` llega en las cinco, y la continuación corre en el hilo de la UI (`thread=1`, `onPanelThread=True`; `H0T:823`). El contexto de sincronización al pedir es `DispatcherSynchronizationContext`.

**H-41. Dos entradas en reposo antes del callback** (fila 3, `H0R:37`). Llegan dos `EnteringQuiescentState` entre la vuelta de la llamada y el `CommandWillStart` de `EXECUTEFUNCTION`, en las cinco sondas. Llegan con el reposo ya puesto (`cmdActiveInActiveSession=False`, `H0T:35-36`). El `CommandWillStart` de `EXECUTEFUNCTION` es el primero tras la llamada (`H0T:37`).

**H-42. Los `*Cancel*` no dan pistas** (fila 4, `H0R:38`). Se ven dos en la línea de comandos por sonda, pero el puente no recibe `CommandCancelled` ni `CommandEnded` antes de `EXECUTEFUNCTION`.

**H-43. El orden de los eventos** (fila 5, `H0R:39`): `CommandWillStart(EXECUTEFUNCTION)` → el callback, con el bloqueo exterior → los `Object*` de la importación y de MUTATE → `Seal` → POST (la purga, `SyncName`, el regen y la relectura) → la salida del callback → `CommandEnded(EXECUTEFUNCTION)` → `EnteringQuiescentState` (`H0T:37`, `:41-42`). **`F4P:60` se cumple:** `CommandEnded` llega después de la relectura de POST. No hay comandos anidados.

**H-44. Un solo paso de Undo** (fila 6, `H0R:40`). La lista de Deshacer muestra **una** entrada «Executefunction» por Actualizar. Un `U` lo deshace entero, y un `REDO` **inmediato** lo rehace entero.

**H-45. El paso vacío existe** (fila 7, `H0R:41`). P1, P2 y P3, sin escritura, dejan cada una su propia entrada «Executefunction» en Deshacer.

**H-46. El estado al entrar en el callback** (fila 8, `H0R:42`; `H0T:134`): `MdiActiveDocument` es el admitido; `TopTransaction` es nula; `CommandInProgress` es `EXECUTEFUNCTION`; y el documento no está en reposo.

**H-47. Los bloqueos anidados funcionan** (fila 9, `H0R:43`). `CaptureDrawing`, dos veces, y `SyncName`, con dos claves, toman y liberan su bloqueo bajo el exterior.

**H-48. WPF no bombea dentro del callback** (fila 11, `H0R:45`). Las sondas `Background` y `Normal` corren **después** de salir del callback (`H0T:826-827`).

**H-49. El `Debug.Assert` del drenaje no salta** (fila 12, `H0R:46`): `drain.assert-precheck top=null` en todos los drenajes (`H0T:829`).

**H-50. `ExecuteInCommandContextAsync` devuelve `DocumentCollection.ExecutionResult`, no un `Task`** (O-H0-4, `H0R:53`; `H0N` §1). Los hechos del IL de `AcCoreMgd` 25.0.171, leídos por la revisión del arnés:
- tiene `IsCompleted`, `GetAwaiter()`, `GetResult()` y `OnCompleted(Action)`. `GetResult` no hace nada: **no hay canal de fallo**;
- **un rechazo nativo no lanza y nunca completa**;
- el trampolín nativo invoca el callback **sin ninguna región de excepción**. Una excepción del callback cruzaría a código nativo, así que el callback tiene que ser total;
- la compleción se publica con el `SynchronizationContext.Current` **de dentro del contexto de comando**. Sin contexto, nunca se completa;
- `OnCompleted` tiene una sola ranura: una segunda llamada sustituye a la primera. Una continuación que lanza abre el diálogo de error de AutoCAD.

**H-51. Las observaciones** (`H0R:50-54`):
- **O-H0-1:** sin el ámbito de T4C, el drenaje tras cada Actualizar lleva `[InvalidateNavigationCaches, MarkDraftsPossiblyStale]`, y el campo queda «posiblemente obsoleto». Es la línea base que corrige D-T4C-09. `U` y `REDO` también marcan, como deben;
- **O-H0-2:** las lecturas del panel («Comprobar ahora» y «Editar») dejan entradas «Group of commands» en Deshacer y **vacían la pila de Rehacer**. Un «Comprobar ahora» entre `U` y `REDO` da «Nothing to redo». No es de T4C;
- **O-H0-3:** `EXECUTEFUNCTION` vacía PICKFIRST, lo que se aceptó en O-15. La pestaña fijada se mantiene.

**H-52. Las desviaciones del procedimiento de H0** (`H0R:56-60`), que se declaran al Owner (§9):
- **el operador fue la sesión, no el Owner,** por la instrucción del Owner («Crealo tú controlando autocad, no pidas autorización»). D-T4C-16 decía «el Owner opera», y su dispensa 2 decía que eso no se dispensaba;
- **el dibujo es sintético:** lo creó la sesión con RACKPUSHBACK y RACKEDITAR, y no es un dibujo real del Owner;
- **el RELEASE de I-52** se pidió antes de abrir AutoCAD y la respuesta llegó después de la sonda (`F4-DRAFTS/t4c/h0/i52-reply.md`, literal). I-52 dice «SIN OBJECIÓN», que no tenía ni tiene prevista ninguna sesión de host, y que **no emite RELEASE**: no concede ventanas de host a otras iniciativas, y la autorización viene del Owner según el §12.1. No hubo solapamiento. **Regla para el futuro:** con I-52 basta con avisar del inicio y del cierre.

---

## 3. El reparto

| Pieza | T4B, sin cambios | T4C |
|---|---|---|
| Stale, Conflict, los valores que ganan, la secuencia de MUTATE y la clasificación de POST | **Sí:** `RUN` y `TryUpdatePilot` | — |
| La operación del panel y su valor de inicio, el billete en vuelo y su cota, el almacén del resultado y el canal de finalización | — | **Sí** (D-T4C-02, D-T4C-03) |
| El contexto de comando, el objetivo opaco, el bloqueo y la revalidación del documento | — | **Sí** (D-T4C-04, D-T4C-08) |
| La creación por corrida del escritor | El escritor y sus ámbitos | Su construcción con el documento exacto, de un solo uso (D-T4C-07) |
| Los fallos del host | — | **Sí**, sin reclasificar los de T4B (D-T4C-05) |
| El ámbito de mutación propia, R-T4-07, la UI, el diagnóstico de T4A2 y los arrastres | — | **Sí** (D-T4C-09 a D-T4C-13) |
| Undo | Una transacción authored | La frontera de comando y su medición (D-T4C-15, D-T4C-16) |

---

## 4. Decisiones de F4-T4C

### D-T4C-01. T4C no reimplementa nada de T4B

*Sin cambios en la v2.*

**Qué hace T4C, y solo eso:**
- crea el adaptador de host de cada corrida;
- entra en el contexto de comando;
- bloquea el documento exacto;
- llama **una vez** a `TryUpdatePilot`;
- traduce los fallos del host;
- guarda y presenta el `EditUpdateOutcome`.

**Lo que queda en T4B y en Application:** Stale, Conflict, los valores que ganan, la secuencia de MUTATE y la clasificación de POST.

**Por qué:**
- es la letra de `OO`, «AUTHORITATIVE MUTATION LOGIC»;
- `TryUpdatePilot` ya contiene la puerta propia, el borrador, la comprobación previa, la ranura y la corrida total (H-02). La lectura de O-04 ya va dentro, por `ReadForPrepare` (M2 H2-18);
- R-T4-06 pide O-04 en contexto de comando, sin la puerta de reposo (`T4P:552-553`). Se cumple al llamar a `TryUpdatePilot` dentro del callback, como prevé `T4BP:557`.

**Consecuencias:**
- **ningún archivo de T4C** nombra `PushBackPilotUpdateRun.Execute`, `Precheck`, `TryBeginApply`, `CompleteApply`, `Rebase` ni `MarkPossiblyStale`, ni ninguna primitiva de escritura (D-T4C-14);
- **el catálogo:** T4C no lo unifica ni lo saca del bloqueo. Las dos cargas por corrida son las declaradas por T4B (§9 I-8 de `T4BP:842`). Unificarlas cambiaría firmas que congela CT-27 (1): es DV-01 de M4;
- **el riesgo de E/S** de cargar el catálogo bajo el bloqueo (M2 R2-07) se declara.

**Anclas:** `CORE:805-905`; `T4BP:557`; `OO`.

### D-T4C-02. El modelo tolerante: el valor de inicio, el billete en vuelo con su cota y el almacén del resultado

*Cambia en la v2: IR R-01, R-03, R-07 (a), R-11 y M-12. Cambia en la v3: RR N-01, N-04, N-09 y N-15. Cambia en la v4: CK C3-03. Cambia en la v5: la cota del billete es la regla del Owner, «Completar o comando ajeno», tras la fila 3 de H0.*

**H0 lo midió** (§2.6): el callback es **diferido** en las cinco sondas, y la llamada **se completa siempre** (H-39, H-40). `Task` está prohibido en el núcleo, en el controlador y en los VM (H-12, H-15). Por eso nada en la cadena del panel espera a la corrida, y el modelo tolerante sigue siendo necesario: el caso diferido es el real.

**Dos operaciones nuevas del puerto del panel**, al final de `IWorkspacePilotEditPort`:
- **`EditUpdateStart RequestUpdate(EditIntentRequest request)`.** Responde al instante con un valor de inicio:
  - `Accepted`;
  - `Waiting(reason)`: el reposo, el modal, sin documento o dentro de un aviso;
  - `Rejected(reason)`: la sesión;
  - `Busy(reason)`: una lectura del piloto o una actualización en vuelo;
  - `Unavailable(reason)`: sin lector o sin ejecutor asignado, o **una sesión degradada** (RR N-09), en la que el billete no tendría cota ni el ámbito propio recibiría eventos (H-36);
  - `Failed(reason)`: no se pudo pedir el contexto de comando, o **la actualización anterior del mismo rack acaba de caducar** en esta misma llamada (RR N-15; CK C3-03, abajo).

  `EditUpdateStart` es un `struct` nuevo de `WorkspaceEditIntent.cs`, con `Unavailable` por defecto.
- **`EditUpdateProgress TakeUpdateProgress(WorkspaceRequest origin, string rackId)`.** Es una operación de memoria. Devuelve:
  - `InFlight`, un `bool`;
  - `Outcome`, un `EditUpdateOutcome`, que es `Unavailable` si no hay ninguno.

  **Consume** el resultado que devuelve, como `TakeLossNotices` y `TakeCloseNotices`. Solo la llama el VM del campo de la superficie viva de ese rack (D-T4C-03).

**El almacén del resultado:**
- vive en la memoria del núcleo, por (sesión, rack);
- **cada petición aceptada deja como mucho un resultado:** el de `TryUpdatePilot`, tal cual, o el fallo del host antes de la mutación (D-T4C-05);
- **un fallo síncrono de la petición** no deja nada en el almacén. Viaja **solo** por el valor de inicio `Failed`. Así hay un solo canal (IR R-02 (b));
- lo vacían `TakeUpdateProgress` y la destrucción de la sesión. **Una nueva aceptación no lo vacía** (RR N-15). El gesto recoge y presenta antes lo pendiente (D-T4C-11, paso 1). La caducidad que ocurre dentro de `RequestUpdate` sale por el valor de inicio si el billete es del mismo rack, y se guarda bajo su propio rack si es de otro (CK C3-03).

**El billete en vuelo:**
- vive en la memoria del núcleo, por sesión;
- lleva:
  - su rack y un **testigo** único de la corrida;
  - la marca «empezado», que pone `Admit`; la marca «en `Execute`»; y la marca «terminada», que ponen `Execute` y `NotRun` al guardar su resultado;
  - la cuenta de **comandos ajenos** de la sesión al aceptar (D-T4C-09, `ForeignCommandStartsOf`). Las entradas en reposo ya no cuentan.
- **Se pone** al aceptar, antes de llamar al ejecutor.
- **Se libera con la compleción del `ExecutionResult`** (la decisión del Owner sobre C12): el corredor llama a la quinta operación de la corrida, `Completed()`, desde la continuación de `OnCompleted` (D-T4C-04). H0 midió que la compleción llega siempre, en el hilo de la UI, después del callback (H-40).
  - Un billete **terminado** se libera sin más: su resultado ya está en el almacén.
  - Un billete **no terminado** al completarse, que es inalcanzable porque la compleción solo llega después de invocar el callback (H-50), guarda `Failed(HostNotRunReason)` y se libera.
- **También se libera,** como respaldo, con la caducidad (abajo), con un fallo síncrono de la petición y con la destrucción de la sesión.
- **`InFlight`**, para la presentación, es «hay billete y no está terminado». Entre `Execute` y la compleción, el resultado ya se puede recoger, y un segundo `RequestUpdate` sigue recibiendo `Busy`. H0 midió que ese intervalo es de unos milisegundos: la compleción es un mensaje que se publica al salir del callback (`H0T:817`, `:823`).
- **La terminación de un billete empezado está garantizada** (RR N-01), en tres capas:
  1. **Las cinco operaciones de la corrida del núcleo son totales.** `Admit`, `Begin`, `Execute`, `NotRun` y `Completed` nunca lanzan. `Execute` y `NotRun` marcan el billete como terminado, en su `finally`, si el testigo es el vigente;
  2. **el corredor garantiza**, con un `finally` exterior propio, **exactamente una** llamada a `Execute` o a `NotRun` después de un `Admit` verdadero (D-T4C-04);
  3. **la defensa en profundidad:** la caducidad de abajo también alcanza a un billete empezado que no ha entrado en `Execute`, y a uno terminado cuya compleción no llegó.
- **Mientras está puesto**, un segundo `RequestUpdate` de la sesión responde `Busy`, aunque el callback no haya corrido. Cubre la ventana que la ranura no cubre (M2 H2-41, M3 H-M3-44).
- **Las operaciones del núcleo, testigo en mano:** `Admit`, `Begin`, `Execute`, `NotRun` y `Completed` llevan el testigo de su corrida. Si no es el del billete vigente, no hacen nada.

**La cota del billete: «Completar o comando ajeno»** (IR R-01; la decisión del Owner sobre C12). Sin cota, un callback que no corre y una compleción que no llega dejarían el documento sin poder actualizarse, descartarse ni cerrarse sin perder el trabajo (H-25). Y los dos casos son posibles: un rechazo nativo de `ExecuteInCommandContextAsync` no lanza y nunca completa, y sin contexto de sincronización dentro del contexto de comando la compleción tampoco llega (H-50).
- **La v4 contaba las entradas en reposo, y H0 la invalidó** (fila 3, C12): AutoCAD entra en reposo dos veces entre la vuelta de la llamada y el `CommandWillStart` de `EXECUTEFUNCTION`, en las cinco sondas (H-41). Con esa regla, cada Actualizar caducaría antes de correr. La traza muestra además que esas dos entradas llegan con el reposo ya puesto (`cmdActiveInActiveSession=False`, `H0T:35-36`). Cualquier regla basada en el reposo es frágil.
- **La regla del Owner:**
  - **la liberación normal es la compleción** (arriba);
  - **el respaldo:** si el callback **no ha empezado**, el billete caduca con el **primer `CommandWillStart` de la sesión de un comando que no sea `EXECUTEFUNCTION`**, con el cierre del documento o con la retirada de la sesión;
  - **las entradas en reposo dejan de contar.**
- **La defensa en profundidad de N-01**, con el mismo disparador: un billete empezado que no entró en `Execute`, o uno terminado cuya compleción no llegó, también caduca con el primer comando ajeno posterior a su `Admit`. Ese comando solo puede empezar cuando `EXECUTEFUNCTION` ya terminó, y con él el callback (H-43). Un billete en `Execute` nunca caduca.
- **El comando ajeno lo anota el puente** (D-T4C-09): el manejador de `CommandWillStart` lee `e.GlobalCommandName`, un dato en memoria del argumento del evento y no del DWG, y si no es `EXECUTEFUNCTION` llama a `EBC.NoteForeignCommandStart(SessionId)`. El núcleo lee la cuenta con `ForeignCommandStartsOf(SessionId)`.
- **El cierre del documento y la retirada de la sesión** llegan al núcleo por `SessionDestroyed` (`CORE:1415-1439`), que quita el billete y el almacén de la sesión. Un cierre vetado no cierra nada, y su comando (`CLOSE`, `CLOSEALL` o `QUIT`) ya es un comando ajeno.
- **También caduca** si la sesión pasa a modo degradado antes de que el callback empiece: `ForeignCommandStartsOf` devuelve nulo para una sesión degradada o desconocida (D-T4C-09; RR N-09).
- **Cuándo se comprueba**, de forma perezosa, en memoria y a coste constante: al principio de `RequestUpdate`, de `TakeUpdateProgress` y de cada operación que responda `Busy` por el vuelo (D-T4C-08 y D-T4C-10). La comprueba una ayuda única del núcleo, `ExpireStaleUpdate`, que solo lee `_bridge.ForeignCommandStartsOf` y nunca muta el puente.
- **Al caducar:**
  - un billete **terminado** solo se libera: su resultado ya está guardado;
  - uno **no terminado** guarda una sola vez `Failed(HostNotRunReason)`, **bajo el rack del billete**. Si la caducidad ocurre dentro de `RequestUpdate`, depende del rack (RR N-15; CK C3-03):
    - **el billete es del mismo rack que la petición:** ese `Failed` sale **por el valor de inicio**, la petición nueva no se acepta y no se guarda nada. El usuario vuelve a pulsar;
    - **el billete es de otro rack:** su `Failed` se guarda bajo **su** rack, como en cualquier otra caducidad, y la petición sigue: se evalúa y, si pasa, se acepta. El resultado del otro rack lo presentará su superficie cuando vuelva a estar viva. Desde el producto es casi inalcanzable, porque el gesto llama antes a `TakeUpdateProgress`, cuya caducidad ya lo guarda bajo su rack;
  - se quita el billete;
  - **el ámbito propio no se cierra aquí** (RR N-04). Es inalcanzable que esté abierto: sin un callback empezado no hubo `Begin`; y en los casos de N-01, el `CommandWillStart` ajeno que dispara la caducidad ya lo cerró dentro de `SetQuiescent`, que cierra con cualquier inicio de comando (D-T4C-09). Una guarda fija que `ExpireStaleUpdate` no nombra `CloseOwnMutation`. `TakeUpdateProgress` no muta el puente ni olvida `_lastAllowedClose`.
- **Un callback tardío** con el testigo viejo no hace nada.
- **Cualquier comando del usuario** es un comando ajeno. En el peor caso, el vuelo dura hasta el siguiente comando de la sesión, su cierre o su retirada. La activación del documento no cuenta.
- **Si un comando ajeno empezara antes que un callback legítimo** (por ejemplo, un comando tecleado antes de que AutoCAD saque el `EXECUTEFUNCTION` de su cola), la actualización se presenta como «no llegó a ejecutarse», sin ninguna escritura, y el callback tardío no hace nada. Es seguro. H0 no lo vio: el `CommandWillStart` de `EXECUTEFUNCTION` fue siempre el primero tras la llamada (H-41).

**«ONE ACTUALIZAR» y la ranura** (IR M-12).
- La ranura de T4B sigue siendo el oráculo de T4B: `AnotherUpdateInProgress` dentro de `TryUpdatePilot` (`T4P:555-558`) no cambia, y sus pruebas de T4B siguen.
- **El host responde `Busy` antes**, en `RequestUpdate`, mientras haya un billete.
- Para el gesto del usuario, «un segundo Actualizar» recibe `Busy`. `AnotherUpdateInProgress` solo lo puede ver una corrida que entra mientras otra tiene la ranura, que en una pila síncrona no ocurre. Se declara como la lectura de R-T4-06 en esta integración.

**Las demás fuentes de una segunda corrida** (`OO`, «ONE ACTUALIZAR»):
- **el Idle y el drenaje:** se amplía la guarda «nada desde el drenaje ni el Idle» (`TW/WorkspaceEditIntentGuardTests.cs:598-641`):
  - `RequestUpdate` y `TryUpdatePilot` no aparecen en `OnDrained` del núcleo, en `OnIdle` del núcleo del puente ni en el del puente;
  - tampoco en `OnDrained`, `NotifyStateChanged`, `OnCommandStateChanged` ni `OnActiveSessionChanged` del controlador, ni en `OnDrained` del host de navegación;
  - así se cierra M1 R-07;
- **el puente:** los manejadores siguen encolando y nada más (`TW/WorkspaceIdleSchedulingGuardTests.cs:64-104`, sin cambios). El de `CommandWillStart` anota además, en memoria, el comando ajeno: no lee el DWG ni pide ninguna corrida;
- **la relectura de POST:** está dentro de la corrida;
- **las notificaciones de mutación propia:** solo invalidan (D-T4C-09);
- **el censo de llamadores** (G-10 de D-T4C-14):
  - `.RequestUpdate(` solo aparece en el gesto `Update()` del VM del campo y en los dos reenvíos (`CTL` y la compuerta);
  - `.TryRequest(` solo aparece en `RequestUpdate` del núcleo.

**La asincronía** (IR R-11): **ningún `Task`, `async` ni `await` nuevos** fuera de `ADAPTER`.
- La navegación conserva sus excepciones exactas, con el censo por archivo de H-09. Lo fija una guarda nueva, con una cuenta por archivo.
- Ni `Dispatcher`, ni `Timer`, ni `.Result`, ni `.Wait(`, ni `GetAwaiter` en ninguna parte nueva.

**Cambios de guarda declarados:** las filas 2, 4 a 8 y 10 de D-T4C-20.

**Anclas:** H-12, H-14 a H-16, H-21, H-22, H-25 y H-39 a H-43; `OO`, «ONE ACTUALIZAR»; la decisión del Owner sobre C12.

### D-T4C-03. El canal de finalización hacia el panel

*Cambia en la v2: IR R-02 (c) y (d). Cambia en la v3: RR N-15. Cambia en la v4: CK C3-03. Cambia en la v5: el aviso tras el comando interno, medido por H0.*

**Las opciones:**

| Opción | Cómo | Guardas que toca | A favor | En contra |
|---|---|---|---|---|
| **F-1. Extracción** | El panel llama a `TakeUpdateProgress` justo después de `Accepted` y en cada repintado que ya provoca el aviso `StateChanged` (H-21) | Las de D-T4C-02. Una regla nueva de la superficie: la presentación del campo puede llamar a `TakeUpdateProgress` y a ninguna lectura (`TW/WorkspacePilotSurfaceGuardTests.cs:54-92`) | Ningún mecanismo nuevo: sin evento, suscripción, temporizador, `Task` ni `Dispatcher`. Si el callback es síncrono, el resultado ya está en la primera llamada | Depende de que llegue un aviso después del comando interno. **H0 midió que llega siempre** (H-43). Si no llegara, el resultado esperaría al siguiente repintado |
| F-2. Empuje al VM | Un delegado o un evento desde el núcleo o el controlador | Rompe `TW/WorkspaceHostHardeningGuardTests.cs:758-760` (`delegate`, `event`, `+=`) y la cuenta de tres `+=` del controlador (`TW/WorkspacePilotEditGuardTests.cs:526`) | Inmediato | Un mecanismo nuevo en tres capas guardadas |
| F-3. Resultado síncrono | `RequestUpdate` devuelve el `EditUpdateOutcome` | Ninguna nueva | La más simple | Solo vale si el callback es síncrono. Contradice la dirección de diseño, y un callback diferido la convierte en una A-n |
| F-4. Sondeo | Un temporizador o el `Dispatcher` | Prohibido (H-15) | — | — |

**La recomendación es F-1.** Es la única compatible con todas las guardas, tolera los tres casos y su respaldo es el aviso que ya existe.

**Las reglas de la presentación**, para que sea exacta y única (IR R-02):
1. **El resultado se presenta una vez.** Lo presenta la primera llamada a `TakeUpdateProgress` que lo encuentra, venga del gesto o de un repintado.
2. **El repintado anidado** (IR R-02 (c)). En el caso síncrono, `CommandWillStart` y `EnteringQuiescentState` dan `NotifyStateChanged` dentro de la propia llamada (H-21). El repintado de la superficie puede consumir el resultado antes que el gesto.
   - **La regla:** `InFlight` falso con `Unavailable` **no toca** el último resultado presentado ni inventa uno.
   - **El indicador «Aplicando»** sale solo de `InFlight` o del estado Applying del campo. Cuando los dos son falsos, el campo se pinta desde su estado (`GetField`), que es la autoridad.
3. **El consumo va por superficie** (IR R-02 (d)).
   - Solo lo consume el VM del campo de la superficie viva, para su propio rack y después de `IsMine`.
   - Si la superficie cambió antes de recogerlo, el resultado sigue en el almacén hasta que la superficie de ese rack vuelva a estar viva. Ese es el sentido de «sigue guardado».
4. **El valor de inicio que no es `Accepted`** se presenta en el gesto, y nunca deja un resultado en el almacén (D-T4C-02). Eso vale también para el `Failed(HostNotRunReason)` de una caducidad ocurrida dentro de `RequestUpdate` cuando el billete es **del mismo rack** (RR N-15). Si es de otro rack, ese `Failed` va al almacén, bajo su rack, y lo presenta su superficie (CK C3-03).
5. **Una nueva petición nunca borra un resultado sin presentar.** El gesto recoge antes lo pendiente del rack, y si encuentra un resultado, lo presenta y no pide nada (D-T4C-11, paso 1).

**Lo que midió H0** (H-43, H-39): después del comando interno llega siempre `EnteringQuiescentState`, que da `NotifyStateChanged` y el repintado que recoge el resultado. El callback es diferido, así que el `TakeUpdateProgress` del gesto, justo después de `Accepted`, ve `InFlight` verdadero y presenta «Aplicando». El resultado lo presenta el repintado posterior. F-1 queda confirmado en el caso real; el caso síncrono no se dio.

**Anclas:** H-15, H-21; M2 Q2-03.

### D-T4C-04. El adaptador de contexto de comando: un corredor puro, un objetivo opaco y unas primitivas en `Plugin/Views`

*Cambia en la v2: IR R-02 (a), R-04, R-08, M-10 y M-14. Cambia en la v3: RR N-01, N-03 y N-11. Cambia en la v5: `ExecutionResult` en lugar de `Task` (H-50), la quinta operación `Completed` y la guarda del adaptador reescrita.*

**La forma.** El adaptador tiene dos capas:
- **el corredor puro**, en Application, que fija el orden y el cerrojo terminal, y se prueba en Core con dobles;
- **las primitivas y el objetivo**, en `ADAPTER`.

El precedente es `WorkspaceNavigationRunner`, que toca el host solo por un puerto de efectos y se prueba simulado (`src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs:103-108`).

**Por qué hacen falta las dos:**
- ningún proyecto de prueba referencia el Plugin (M2 H2-33);
- sin el corredor puro, las pruebas 1 a 11 y 17 a 19 de `OO` solo serían guardas de texto.

**(a) El corredor y sus contratos**, en `HOSTRUN`, en el espacio de nombres del piloto. No nombra el Workspace ni AutoCAD, y respeta las dos restricciones de nombres de H-06 y H-20 (IR M-16): ni la palabra «Transaction», ni siquiera en un comentario. Contiene:
- **`IPushBackPilotUpdateHost`**, el ejecutor: `bool TryRequest(long documentNativeId, IPushBackPilotHostRun run, out string failure)`;
- **`IPushBackPilotHostRun`**, el delegado síncrono de una corrida, en forma de interfaz para poder doblarlo. Lo implementa el núcleo, una instancia por petición, con su testigo. Tiene cuatro operaciones síncronas, **todas totales**: nunca lanzan (RR N-01):
  - `bool Admit(out string reason)`, que además marca el billete como empezado;
  - `void Begin()`;
  - `void Execute(IPushBackPilotUpdatePort port)`;
  - `void NotRun(PushBackPilotHostFailure kind, string cause)`, con `PushBackPilotHostFailure` = `Rejected` o `Failed`;
  - **`void Completed()`**, nueva en la v5: la compleción del `ExecutionResult`, que libera el billete (D-T4C-02);
- **`IPushBackPilotHostTarget`**, el **objetivo opaco** (IR R-04). Lo implementa el Plugin, y Application nunca ve un `Document`:
  - `IDisposable Lock()`;
  - `IPushBackPilotUpdatePort CreateWriter()`;
  - `void Retire(IPushBackPilotUpdatePort writer)`;
- **`IPushBackPilotHostPrimitives`**, las primitivas:
  - `void EnterCommandContext(Action callback, Action completed)`. La compleción no lleva fallo, porque `ExecutionResult` no tiene canal de fallo (H-50);
  - `bool TryOpenTarget(long documentNativeId, out IPushBackPilotHostTarget? target, out string reason)`;
- **`PushBackPilotHostRunner`**, que existe una vez por petición. **Solo lo crea su `Request` estático**, y `PushBackPilotHostRunner.Request(` solo aparece en `TryRequest` de `ADAPTER`. Ningún tipo de larga vida lo guarda (G-3; RR N-03). El corredor sí retiene `run`, el id y su estado entre `Request` y el callback diferido, en sus campos o en las clausuras que genera el compilador: es su función, y vive lo que vive la petición;
- **`PushBackPilotHostTexts`**, los textos nuevos de T4C (D-T4C-05).

**El cerrojo terminal del corredor** (IR R-02 (a), M-14). Tiene tres estados: `Pending`, `Started` y `Ended`.
- **`Request`** llama **una vez** a `EnterCommandContext`, dentro de un `try`:
  - si la llamada lanza y el callback **no** ha empezado, el estado pasa a `Ended` y `Request` devuelve falso con la causa. **No llama a `NotRun`**: es el canal único de D-T4C-02;
  - si la llamada lanza **después** de un callback síncrono que ya empezó, solo se registra, y `Request` devuelve verdadero. Nunca hay un `Failed` después de una escritura.
- **`OnCallback`**: si el estado no es `Pending`, solo se registra y no hace nada. Si lo es, pasa a `Started` y sigue en este orden:
  1. **El documento:** `TryOpenTarget(admitido)` lo resuelve por su id nativo, comprueba que no está cerrado y que sigue siendo el activo. Si no, `NotRun(Rejected, DocumentMismatchReason)`, la constante que ya tiene la corrida (`RUN:61`).
  2. **La sesión:** `run.Admit`, en memoria. Comprueba que el testigo es el vigente, que la sesión vive y está autorizada contra el documento activo y que no hay modal. Si no, `NotRun(Rejected, motivo)`, que el núcleo ignora si el testigo ya no vale. Este paso para también un id nativo reciclado (H-29).
  3. **El bloqueo:** `using (target.Lock())`, de **ese** documento.
  4. **El ámbito de mutación propia:** `run.Begin()` lo abre (D-T4C-09).
  5. **El escritor de ese documento:** `target.CreateWriter()`.
  6. **La corrida:** `run.Execute(writer)`, **una vez**, que llama a `TryUpdatePilot`.
  7. **La liberación, en `finally`:** `target.Retire(writer)`, **solo si el escritor llegó a crearse** (RR N-01), y el `Dispose` del bloqueo, cada uno en su propio `try`.
     - Si la excepción salta **antes** de `Execute`, `NotRun(Failed, …)`.
     - Si salta **después**, solo se registra y nunca se reclasifica (D-T4C-05).
  8. **El `finally` exterior** (RR N-01). Desde un `Admit` verdadero, un `finally` que envuelve los pasos 3 a 7 garantiza **exactamente una** llamada terminal: `Execute`, o `NotRun(Failed, …)` si `Execute` no llegó a llamarse por cualquier motivo, también una excepción de `Retire` o del `Dispose`. Una marca local, «terminal hecha», impide la segunda. Antes de `Admit`, los rechazos de los pasos 1 y 2 ya son esa llamada terminal.
- **`OnCompleted()`**, la compleción: llama **una vez** a `run.Completed()`, con una marca local que impide la segunda. Si el estado era `Pending`, pasa a `Ended`, y `Completed` guarda `Failed(HostNotRunReason)` en el núcleo. Es inalcanzable, porque la compleción solo llega después de invocar el callback (H-50), pero queda cubierto.
  - **Lo que la compleción no cubre lo cubre la caducidad** de D-T4C-02: un rechazo nativo no lanza y nunca completa, y sin contexto de sincronización dentro del contexto de comando la compleción tampoco llega (H-50).
  - `Request` sigue capturando la excepción síncrona de la llamada, pero H0 muestra que un rechazo nativo no la produce.
- **`OnCallback` y `OnCompleted` son totales** (IR M-10): todo, también `NotRun`, `Completed` y el registro, va dentro de un `try` cuyo `catch` registra y no relanza. **Ninguna excepción sale hacia AutoCAD.** Es obligatorio: el trampolín nativo invoca el callback sin ninguna región de excepción, y una continuación que lanza abre el diálogo de error de AutoCAD (H-50).

**La pureza del corredor.** No nombra `Task`, `async`, `await`, `Dispatcher`, `Timer`, `Autodesk`, `Database` ni `LockDocument`, ni usa reflexión ni archivos. Lo fija una guarda nueva, a la manera de `TW/WorkspacePilotUpdateGuardTests.cs:57-125`.

**(b) Las primitivas y el objetivo, en `ADAPTER`:**
- **`internal sealed class RackPilotUpdateCommandContext : IPushBackPilotUpdateHost`**, sin campos. `TryRequest` delega en `PushBackPilotHostRunner.Request(…, new Primitives(), …)`.
- **La clase anidada `Primitives`**, sin campos:
  - **el planificador primero** (IR R-08): `var context = SynchronizationContext.Current;` va **antes** de la llamada. Es el planificador capturado. Si es nulo, no se pide nada y el resultado es el valor de inicio `Failed`. H0 midió `DispatcherSynchronizationContext` (H-40);
  - **la única llamada de producto** a `AcApplication.DocumentManager.ExecuteInCommandContextAsync`, con una lambda que llama a `callback()` dentro de un `try` total y devuelve `Task.CompletedTask`. Devuelve un `DocumentCollection.ExecutionResult`, **no un `Task`** (H-50);
  - **la compleción, con una sola `OnCompleted`** (H-50; O-H0-4):
    - si `result.IsCompleted` ya es verdadero, se salta directamente al planificador capturado. H0 nunca lo vio (H-39), pero `OnCompleted` no se llamaría;
    - si no, `result.OnCompleted(…)` **una sola vez**, porque la ranura es única;
    - **el salto al planificador capturado** es `context.Post(_ => completed(), null)`, en una única ayuda que usan las dos ramas. No usa `Task.Factory.StartNew` ni `ContinueWith`, que siguen prohibidos. El arnés de H0 sí usó `Task.Factory.StartNew(…, scheduler)`, pero era un diagnóstico (`H0N` §1);
    - **la continuación cruda** que recibe `OnCompleted` es total: un `try` alrededor del `Post`, que registra y no relanza. Si el `Post` falla, la compleción no llega y la cubre la caducidad;
    - **la acción publicada** es total: un `try` alrededor de `completed()`. `result.GetResult()` no aporta nada, porque no tiene canal de fallo. Si se llama, es como mucho una vez y **solo dentro de ese `try`**;
    - H0 midió que la continuación corre en el hilo de la UI, después del callback (H-40). El salto al contexto capturado lo garantiza también si AutoCAD publicara la compleción en otro hilo;
  - `TryOpenTarget` recorre `DocumentManager` por `UnmanagedObject.ToInt64()` y exige `== MdiActiveDocument`. El precedente es `FindDocumentOrNull` (`src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs:335-343`).
- **La clase anidada `Target : IPushBackPilotHostTarget`:**
  - **un único campo**, `private readonly Document _document;`;
  - `Lock()` devuelve `_document.LockDocument()`;
  - `CreateWriter()` devuelve `new RackPilotUpdateWriter(_document)`;
  - `Retire(writer)` llama al `Retire()` del escritor (D-T4C-07);
  - el precedente es `AutoCadProjectionWriteScopeFactory(Document)` (`src/RackCad.Plugin/Views/RackProjectionWriteScope.cs:17-21`).
- **Es agnóstico del Workspace:**
  - no nombra `RackCad.Application.Workspace` ni `RackCad.Plugin.Workspace` (H-10);
  - solo nombra los tipos del piloto, el escritor y AutoCAD;
  - **recibe el id nativo del documento admitido** y el delegado síncrono de la corrida.
- **Es la única excepción por archivo** a la guarda de `ExecuteInCommandContextAsync` de todo el producto. Lo piden C-F4-02 (`F4P:59`) y R-T4-05 (`T4P:547-550`).

**INV-19** (IR R-04; RR N-11): ningún `DBObject`, `Transaction` ni `Document` cruza la interfaz de Application, y ninguna firma de Application nombra un tipo de AutoCAD. El corredor solo ve el objetivo opaco. El `DocumentLock` lo recibe como `IDisposable`, un tipo de .NET, y solo lo libera. Es la letra de `OO`, «No AutoCAD DBObject, Transaction or Document may escape through the Application-facing interface».

**El bloqueo, frente a R-T4-06.**
- El «único `using LockDocument`» es el `using` del corredor sobre `target.Lock()`. Abarca todo `TryUpdatePilot`: PREPARE, MUTATE y POST, la ranura y `CompleteApply`.
- El objetivo devuelve el bloqueo y no lo envuelve. El precedente de un ámbito que posee el `DocumentLock` es `src/RackCad.Plugin/Views/RackProjectionWriteScope.cs:23-37`.
- **Se declara** como la forma de R-T4-06 en esta integración.
- T4C no abre ninguna transacción alrededor de la corrida.

**La puerta de reposo** se evalúa en `RequestUpdate`, en el contexto de aplicación del clic y **antes** de pedir el contexto de comando. Es `PassGate` sin cambios (`CORE:1481-1515`), con sus textos de hoy. Dentro del callback solo quedan `Admit` y la `UpdateGate` de T4B, porque el reposo fallaría siempre dentro de `EXECUTEFUNCTION` (H-21).

**La guarda del adaptador** (IR R-08), reescrita en la v5 para `ExecutionResult` (H-50; O-H0-4). En su archivo:
- **exactamente una aparición** de:
  - `SynchronizationContext.Current`, antes de `ExecuteInCommandContextAsync`;
  - `ExecuteInCommandContextAsync`;
  - `.OnCompleted(`, en la rama en que `IsCompleted` es falso;
  - `.Post(`, sobre el contexto capturado, en la única ayuda del salto;
  - `.LockDocument()`, en `Lock`;
  - `new RackPilotUpdateWriter(`, en `CreateWriter`;
- **como mucho una** lectura de `.IsCompleted`, y como mucho un `GetResult()`, **solo dentro de un `try`**;
- **los campos:** ninguno, salvo el `_document` exacto de `Target`;
- **ninguna aparición** de las prohibiciones del precedente (H-09), que se mantienen, salvo `GetResult`, que queda como arriba:
  - `.Wait(`, `.Result`, `GetAwaiter`, `ConfigureAwait`;
  - `Task.Run`, `Task.Factory`, `StartNew`, `ContinueWith`, `async void`;
  - y además `Thread`, `ThreadPool`, `Timer`, `Dispatcher`, `TaskScheduler`, `async` y `await`;
  - `StartTransaction`, `StartOpenCloseTransaction`, `.Commit(` y `TopTransaction`;
  - ni la lista de escritura de D-T4C-14.
- **la totalidad, por texto:**
  - la lambda del callback, la continuación cruda de `OnCompleted` y la acción publicada empiezan con un `try` cuyo `catch` registra y no contiene `throw`;
  - el corredor (`HOSTRUN`) cumple lo mismo en `OnCallback` y `OnCompleted`.

Cada regla lleva su negativa: una segunda `OnCompleted`, un `GetResult()` fuera de un `try`, un `Task.Factory.StartNew`, un `ContinueWith`, una continuación sin `try` y un `throw` en un `catch`.

**La continuación sigue siendo total.** Hay tres puntos por los que una excepción podría salir hacia AutoCAD:
- la lambda del callback;
- la continuación cruda, que AutoCAD invoca dentro de un `catch` que abre su diálogo de error;
- la acción publicada.

Los tres envuelven todo en un `try` que registra y no relanza, y el corredor también es total. Si el salto falla, no se pierde nada: el billete lo libera su caducidad, y el resultado de la corrida, si la hubo, ya está en el almacén.

**Cambios de guarda declarados:** las filas 1, 3 y 13 de D-T4C-20.

**Lo que H0 confirmó del orden** (H-46, H-47): al entrar en el callback, el documento activo es el admitido y `TopTransaction` es nula, y los bloqueos anidados de `CaptureDrawing` y `SyncName` funcionan bajo el exterior. El orden objetivo → `Admit` → bloqueo → corrida se sostiene en el host.

**Anclas:** H-09 a H-11, H-21, H-22, H-29, H-39, H-40, H-46, H-47 y H-50; `T4P:547-553`; `F4P:59`; M2 §1.5 y §3.4.

### D-T4C-05. La traducción de los fallos del host, sin tocar el modelo de T4B

*Cambia en la v2: IR R-02 (a) y (b), M-03 y M-13. Cambia en la v3: RR N-13 y N-15. Cambia en la v4: CK C3-03. Cambia en la v5: la compleción sin canal de fallo y el rechazo nativo que no lanza (H-50).*

| Frontera (`OO`, «COMMIT FAILURE») | Quién decide | Estado que llega al panel | Lo que hace T4C |
|---|---|---|---|
| **La petición falla en síncrono** antes del callback, o **la actualización anterior del mismo rack caduca** dentro de `RequestUpdate` | T4C | El valor de inicio `Failed(reason)`, sin nada en el almacén | Cero corridas de T4B. El billete se quita. En la caducidad, la petición nueva no se acepta (RR N-15). Si el billete caducado es **de otro rack**, su `Failed` va al almacén bajo su rack, como en la fila siguiente, y la petición sigue (CK C3-03) |
| **Un fallo del host antes de la mutación:** el callback no corre (también un rechazo nativo, que no lanza y nunca completa, H-50), el billete caduca por un comando ajeno o por la degradación, el documento cambió o se cerró, la sesión no se admite, el bloqueo lanza | T4C | `Rejected(reason)` o `Failed(reason)` en el almacén, con las fábricas que ya existen (H-04) | Cero corridas de T4B. El borrador queda como estaba: no se toma la ranura, ni se llama a `CompleteApply` ni a `NotePilotEdit` (M2 H2-42). El billete se quita y el ámbito propio se cierra |
| Antes de la mutación, dentro de T4B: la puerta, el borrador, la comprobación previa o la ranura | T4B | `Rejected`, `NoDraft`, `NothingToApply`, `PendingText`, `NotApplicable`, `AnotherUpdateInProgress`, `Refused` o `Failed` | Lo guarda tal cual |
| La mutación o una unidad antes del commit | T4B | `NotAppliedConflict`, `NotAppliedAdmission`, `NotAppliedPreflight` o `NotAppliedFailure` | Lo guarda tal cual |
| El commit falla, o la persistencia es desconocida | T4B | `CommitThrew`: Unknown(CommitThrew) | Lo guarda tal cual. **Nunca lo convierte en fallo ni en éxito** |
| El commit vuelve | T4B | `Applied` | Lo guarda tal cual |
| El commit vuelve con un aviso de POST | T4B | `AppliedWithWarnings`, o `AppliedPostRereadFailed`, que es Unknown(PostReread) | Lo guarda tal cual, con sus avisos |
| Un error diferido o un descarte durante la corrida | T4B | `ClosedInError` con su clasificación (M-03, D-T4C-13) o `ClosedDetached` | Lo guarda tal cual |
| `NotApplying` después de la corrida (`CORE:870`), hoy inalcanzable | T4B, con M-01 de T4C | `ClosedInError` con la clasificación de `closed.Kind` y `UpdateNotClosedReason`. **Nunca un éxito**, aunque el sello volviera (IR M-03). En este caso el borrador no está en Error, así que la documentación del valor (H-38) se amplía en el mismo cambio de M-03: «the draft is now in Error, or it did not accept the close of the update» (RR N-13) | Lo guarda tal cual |
| **Una excepción del host después de que el callback empezó:** la de la llamada misma, el `Dispose` del bloqueo, la continuación de `OnCompleted` o el salto al contexto capturado. `ExecutionResult` no tiene canal de fallo (H-50) | T4C | El resultado ya guardado | Solo la registra. **Nunca reclasifica** (el cerrojo de D-T4C-04). Si se pierde la compleción, el billete lo libera su caducidad |
| Una excepción que escapa de `TryUpdatePilot` | — | Imposible por construcción tras D-T4C-13 (M-01) | Defensa: se registra y **no se guarda ningún resultado**. El panel repinta desde el estado del borrador, que es la autoridad (D-T4C-03, regla 2). Unknown se ve como «Estado desconocido; comprueba.». Nunca da `Failed` ni `Applied` |

**POST** (`OO`, «POST»):
- T4C nunca toca el estado del borrador;
- Dirty pasa a false solo por la clasificación de T4B tras la relectura de POST;
- la relectura corre dentro del callback y del bloqueo, con el ámbito propio abierto. Nada de T4C cierra el contexto antes de que vuelva `TryUpdatePilot`.

**Los textos** (IR M-13):
- **los tres de I-01** van a `RUN`, como dispuso el Owner (`T4BC` §5: «los tres textos pasan a constantes de `PushBackPilotUpdateRun`»): `UpdateNotRunReason`, `NoActiveDocumentReason` y `ModalOpenReason`, sin cambiar su redacción;
- **los nuevos de T4C** van al archivo nuevo de T4C, `HOSTRUN`, en `PushBackPilotHostTexts`, y no a `RUN`: `UpdateInFlightReason`, `HostNotRunReason`, `HostRequestFailedPrefix`, `LockFailedPrefix`, `HostUnavailableReason` y `UpdateNotClosedReason`. Su redacción se fija en la emisión;
- **ningún literal nuevo va en `CORE`;**
- la parada C14 protege `RUN` de todo cambio fuera de I-01 y M-01 (§6).

**Anclas:** H-02, H-04; `FRZ:521-532`; M2 H2-39 a H2-42; `OO`, «COMMIT FAILURE» y «POST».

### D-T4C-06. El cableado: un ejecutor sin estado, asignado una vez

*Sin cambios de diseño en la v2. Su cambio de guarda es la fila 3 de D-T4C-20.*

**Las opciones que dan los mapas** (M1 W-1 a W-4; M3 §3.7 (a) a (c)):

| Opción | Qué es | Choques |
|---|---|---|
| W-1 | Un campo `IPushBackPilotUpdatePort?` en el núcleo, es decir, B-2 (b) | **Retiene el escritor** todo el proceso, contra «not retained … not cached» (`OO`). Descartada |
| W-2 / M3 (a) | Un campo del núcleo con una fábrica o un ejecutor, asignado por un setter | Un campo de larga vida: ¿es «a new persistent host authority»? (§9 O-03) |
| W-3 / M3 (b) | Sin campo: el adaptador del controlador intercepta la operación y llama a `TryUpdatePilot` con un delegado | Rompe el reenvío literal (`TW/WorkspacePilotEditGuardTests.cs:509-519`) y se salta la compuerta de avisos |
| M3 (c) | Un decorador de `Plugin/Views` sobre la compuerta | Cambia la construcción literal (`:506-507`) y lleva tipos del Workspace a `Plugin/Views` (H-10) |
| W-4 | Un puerto del panel aparte | Contradice `T4BP:814` y añade otra superficie |

**La recomendación es W-2, en la forma E-a.**
- **El campo:** `private IPushBackPilotUpdateHost? _updateHost;` en el núcleo.
- **El setter:** `public void SetPilotUpdateHost(IPushBackPilotUpdateHost host)`, a la manera de `SetEditIntentReader` (`CORE:275-276`).
- **Lo asigna el controlador** en el getter de `Navigation`, justo después de `_host.SetEditIntentReader(...)` (`CTL:105`), con `new RackPilotUpdateCommandContext()`. Comparte el ciclo de vida del lector. Si el lector falta, `PassGate` ya responde `Unavailable`.
- **El ejecutor no guarda nada:** ni documento, ni escritor, ni transacción, ni corrida. Lo fija la guarda «sin campos» del adaptador. El escritor sigue siendo de una corrida (D-T4C-07).
- **La interfaz** se declara en el espacio de nombres del piloto y no en `WorkspaceEditIntent.cs`. Así el adaptador de `Plugin/Views` puede implementarla sin nombrar el Workspace (H-10).

**El precio:** el campo es de un tipo de `Systems`, y hace falta un cambio declarado de la guarda de campos.

**La alternativa E-b** evita ese cambio:
- la interfaz va en `WorkspaceEditIntent.cs`;
- un reenviador anidado en `CTL` la implementa y llama a las primitivas;
- cuesta una indirección más en el controlador, con sus guardas;
- el campo de larga vida sigue existiendo.

Se pregunta en el §9, O-03.

**Anclas:** H-12, H-13; M1 §2; M3 §3.7.

### D-T4C-07. El escritor del documento exacto y de un solo uso (M1 D-1)

*Cambia en la v2: el escritor lo crea el objetivo (IR R-04).*

**El cambio, en `WRITER`:**
- **la clase del puerto** pasa a `RackPilotUpdateWriter(Document document)`, con dos campos:
  - `private readonly Document _document;`;
  - la marca de un solo uso, `private bool _spent;`;
- **`Begin`:**
  - lanza `InvalidOperationException` si `_spent` ya es verdadero;
  - si no, pone `_spent = true` y devuelve `return new RackPilotUpdateScope(_document, request);`;
- **`internal void Retire() => _spent = true;`**, que el objetivo llama en toda salida;
- **la comparación** del id nativo en el constructor del ámbito se queda como segunda línea (`WRITER:68-72`; M1 R-10);
- **`MdiActiveDocument`** desaparece del escritor.
- **Lo construye solo** `Target.CreateWriter()` de `ADAPTER` (G-3).

**Una precisión a la dirección de la sesión.** «Un único campo de solo lectura» y «una marca de un solo uso» son dos campos, y la guarda admite exactamente esas dos declaraciones. La forma de un solo campo, un `Document?` consumido por `Begin`, no es de solo lectura (§9, O-04).

**El cambio declarado de la guarda de T4B** (D-T4C-20, fila 13):
- `WriterProhibitionViolations` (`TW/WorkspacePilotUpdateGuardTests.cs:185-188`) quita del texto del puerto exactamente:
  - el constructor;
  - las dos declaraciones;
  - las líneas de `Begin` y `Retire`;
- y aplica al resto **la misma** expresión de hoy;
- **la negativa** de `:267-276` sigue valiendo sin cambios. Inyecta `private Document _document;`, sin `readonly`, y esa línea no coincide con la declaración que se quita (IR §2.4 lo confirma);
- **negativas nuevas:** un segundo campo `Document`, un campo `Transaction`, un campo `static` y un `Begin` sin la comprobación de `_spent`.

**Por qué no es «reopen T4B».**
- `OO` asigna a T4C el «wiring of the run-scoped primitive writer», con dos exigencias que el escritor de hoy no cumple:
  - **«receives only the exact current document/context»**, porque hoy lo resuelve él mismo (H-05);
  - **«cannot be reused after run completion»**, porque hoy `Begin` se puede repetir.
- El cambio solo toca cómo nace el puerto. No toca:
  - los dos ámbitos ni sus operaciones;
  - MUTATE, POST ni la corrida;
  - las fachadas, CT-27 ni el núcleo de `Traverse`.
- Las guardas de D-T4B-09 siguen todas, salvo la línea declarada.
- La parada C14 protege `WRITER` de todo cambio fuera de estas líneas.

**La alternativa D-3** no toca `WRITER`:
- una clase por corrida, en el archivo del adaptador, que implementa `IPushBackPilotUpdatePort` y llama a `new RackPilotUpdateScope(document, request)`, cuyo constructor es `internal` (`WRITER:68`);
- deja `RackPilotUpdateWriter` sin uso, con su `MdiActiveDocument`;
- da un segundo implementador de producto, y G-2 tendría que nombrar los dos.

D-4, sin cambio y con la comparación como única defensa, no cumple la letra de `OO`.

**Anclas:** H-05, H-06; M1 §3; M2 H2-24 y H2-25.

### D-T4C-08. La revalidación final, sin TOCTOU

*Cambia en la v2: IR R-03 y M-09. Cambia en la v3: RR N-08. Cambia en la v5: la `TopTransaction` al entrar, medida por H0.*

**Ningún resultado de T4A2 autoriza.** `TryUpdatePilot` no nombra `ChosenReference`, `Witness`, `PilotRead`, `.StaleBase` ni el lector (`TW/WorkspaceEditIntentGuardTests.cs:1098-1111`). Sigue así.

| Condición que puede cambiar entre la aceptación y MUTATE | Al aceptar (`RequestUpdate`, contexto de aplicación) | En el callback, antes del bloqueo (corredor) | Dentro de `TryUpdatePilot` (T4B) | MUTATE (T4B, la autoridad) |
|---|---|---|---|---|
| La sesión viva y autorizada | `Authorize` y `PassGate` | `Admit`: la sesión no está destruida, y `Authorize` va contra el documento activo | `UpdateGate`, con `Authorize` (`CORE:925-929`) | — |
| El documento | `PassGate`: hay un documento activo; se captura el id admitido | `TryOpenTarget`: por el id, no cerrado y `== MdiActiveDocument`. Un id reciclado lo para `Admit` (H-29) | `UpdateGate`, con el documento activo | La comparación del ámbito (`WRITER:71`) y el Preflight `DocumentMismatchReason` de la corrida |
| El modal de RackCad | `PassGate` | `Admit` | `UpdateGate` (`CORE:919-923`) | — |
| El reposo | `PassGate`, el único sitio posible (H-21) | — | — | — |
| Una lectura del piloto en curso | `_pilotReadInProgress` → `Busy` | `Admit` | — | — |
| Un aviso en curso | La compuerta → `Waiting` | — | — | — |
| **El billete en vuelo** | Se pone; si ya había uno, `Busy` | `Admit`: el testigo sigue siendo el vigente y el billete no ha caducado | — | — |
| **El borrador de ese rack** | La UI cruza antes la frontera Update (D-T4C-11) y deja el campo sin texto pendiente | **No puede haber cambiado:** durante el vuelo, el núcleo responde `Busy` a toda operación que lo cambia (abajo) | `NoDraft`, `Precheck` (`PendingText`, `NothingToApply`, `NotApplicable`) | — |
| La ranura | — | — | `TryBeginApply` (`CORE:837`) | — |
| El dibujo: otro comando, UNDO o RACKEDITAR | — | — | La lectura de PREPARE | **La relectura de MUTATE:** el origen, la StaleBase, el destino, los supervivientes y las capas (M1 H-27) |
| Una `TopTransaction` ajena | — | H0 midió que es nula al entrar (H-46) | — | `BeginMutation` → `TOP_TRANSACTION_OPEN` |

**La política del borrador durante el vuelo** (IR R-03). El Actualizar aplica el estado del clic, y no uno que el usuario no tenía al pulsar.
- **Mientras el rack tiene un billete en vuelo,** el núcleo responde `Busy` o `Waiting`, con `UpdateInFlightReason` y sin cambiar nada, en estas operaciones:
  - `SetPendingText` y `CommitField`: `PilotFieldResult.Busy`;
  - `TryKeepMine` y `TryDiscardAndReload`: `EditIntentOutcome.Waiting`;
  - **`TryConfirmBase`**: `EditConfirmOutcome.Waiting`, que ya existe (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:353`), antes de su relectura (`CORE:548-556`) (RR N-08). Con una diferencia, `ConfirmBase` pasaría un Dirty a Conflict (H-35). El desenlace sería seguro, porque la comprobación previa de T4B daría NotApplicable, pero el borrador cambiaría;
  - `DiscardRack` y `DiscardDocument` (D-T4C-10).
- **Cada una comprueba primero** la caducidad (`ExpireStaleUpdate`, D-T4C-02).
- **`CrossBoundary` no necesita la marca.** Sin texto pendiente da `Proceeded` sin cambiar nada (H-27), y durante el vuelo no puede haber texto pendiente, porque `SetPendingText` responde `Busy` y el gesto consumió el de antes del clic.
- **El resto no puede cambiar un Dirty:** `TryReload` solo actúa sobre un Clean, `TryCheckNow` sobre un Unknown(CommitThrew) y `TryBeginEdit` responde `AlreadyOpen` (M3 H-M3-17). La v2 contaba aquí `TryConfirmBase`, y era falso (RR N-08).
- **La alternativa,** una huella del borrador en el billete que `Admit` compara, se descarta. Toca menos operaciones, pero deja que el usuario edite algo que después no se aplica.
- **Las pruebas,** en RQ: cada una de las cinco operaciones durante el vuelo responde `Busy` o `Waiting`, el borrador no cambia, y después de la salida vuelven a admitirse. `TryConfirmBase` se prueba con una relectura que difiere, y el borrador sigue Dirty.

**Anclas:** H-02, H-03, H-21, H-26, H-27 y H-29; M1 H-27 y §7.

### D-T4C-09. El ámbito de mutación propia: la vía A, con su residual declarado y una IC nueva

*Cambia en la v2: IR R-05, R-06, R-07 (b) a (d) y M-01. Cambia en la v3: RR N-04, N-07, N-09 y N-10. Cambia en la v4: CK C3-01 y C3-02. Cambia en la v5: la vía A queda confirmada con el orden medido por H0; V1, V2, V3 y el cierre en `Settle` pasan a respuestas declaradas para Smoke-2; y el manejador de `CommandWillStart` anota el comando ajeno.*

Lo exigen C-F4-04 (`F4P:60`), IC-08 (`ICL:145-151`), INV-08 (`FRZ:746`), INV-28 (`FRZ:766`) y R-T4-08 (`T4P:562-566`). El Owner eligió la vía A con IC-09 (O-02, «Vía A + IC-09»).

**Lo que midió H0** (§2.6):
- **la línea base sin el ámbito** (O-H0-1, H-51): tras cada Actualizar, el drenaje marca el borrador y el campo queda «posiblemente obsoleto». Es justo lo que corrige esta decisión;
- **el orden** (H-43): `CommandWillStart(EXECUTEFUNCTION)` llega **antes** del callback, y por tanto antes de `Begin`. Dentro del callback van todos los `Object*` de la importación, de MUTATE y de POST, y la relectura. Después de salir llegan `CommandEnded(EXECUTEFUNCTION)` y `EnteringQuiescentState`. No hay comandos anidados;
- **los `*Cancel*`** no dan ninguna pista (H-42).

**Con ese orden, la vía A tal como está** abre la ventana en `Begin`, registra como propio todo lo que escribe la corrida, y la cierra con el `CommandEnded` de `EXECUTEFUNCTION`, que cuenta como propio. Ninguna de las cuatro respuestas preparadas hace falta (abajo).

**Las pistas propias se registran aparte.** No se suprimen las ajenas pendientes.
- **`WorkspaceSession`** gana un segundo conjunto, `_ownPending`, también agrupado por tipo y de coste constante, con `EnqueueOwnHint(HintKind)`.
- **`SessionState` de `EBC`** gana dos campos: `OwnMutationOpen`, un `bool`, y `ForeignCommandStarts`, un `int`, la cuenta de comandos ajenos para la cota del billete (D-T4C-02). Ninguno contiene `Close`, `Allowed` ni `SessionId?` (H-17). La v4 llevaba aquí `QuiescentEntries`, que ya no existe: las entradas en reposo dejan de contar.
- **Mientras `OwnMutationOpen` es verdadero,** `EBC.OnSourceEvent` encola en `_ownPending` las pistas `DatabaseObjectChanged` y `CommandEnded`.
- **`ImpliedSelectionChanged`** sigue en `_pending`, porque nunca marca.

**Al drenar** (`WorkspaceSession.TryDrain`; IR M-01):
- lo propio da solo `InvalidateNavigationCaches`, como exige IC-08;
- `MarkDraftsPossiblyStale` sale **solo si `_pending` contiene `DatabaseObjectChanged` o `CommandEnded`**. Un `ImpliedSelectionChanged` pendiente, que siempre está tras el vaciado de PICKFIRST (O-H0-3), no marca;
- `PendingHintCount` cuenta los dos conjuntos, para que el Idle se enganche;
- `DrainOperation` no cambia.

**La apertura:** `run.Begin()`, en el callback, bajo el bloqueo y **antes** de `TryUpdatePilot`, por `IWorkspaceBridgeLink.OpenOwnMutation(session)`. Cubre la importación de PREPARE (`T4BP:325`), MUTATE, POST y la relectura, que H0 midió todas dentro del callback (H-43).

**El cierre**, idempotente. Tiene una cota estructural (IR R-05):
- **con el primer `CommandEnded`, `CommandCancelled` o `CommandFailed`** de la sesión tras abrirlo, que se registra como propio y cierra. H0 midió que es el de `EXECUTEFUNCTION`, después de la relectura de POST (H-43);
- **dentro de `SetQuiescent`, en las dos transiciones de estado de comando** (IR R-07 (b)):
  - `SetQuiescent` pone `state.OwnMutationOpen = false` **antes** de su comparación de `:439`;
  - así cierra con el **primer `CommandWillStart`** de la sesión tras `Begin`, aunque el reposo no cambie, y también con `EnteringQuiescentState`;
  - los dos literales de caso no cambian (H-17), y el cuerpo de `SetQuiescent` solo toca ese campo de `state`. No hace falta tocar `TW/WorkspaceHostC2SourceGuardTests.cs`;
  - **en la v5, `SetQuiescent` ya no suma nada.** La cuenta de la cota la lleva `NoteForeignCommandStart` (abajo);
- **explícito**, por `CloseOwnMutation(session)`, en `NotRun` y solo ahí. **La caducidad no lo cierra** (RR N-04): es inalcanzable que esté abierto, como explica D-T4C-02;
- **con la destrucción** de la sesión;
- **con la entrada en modo degradado** (RR N-09; CK C3-02): **`Degrade`** (`EBC:478-488`), el punto único de la degradación, pone también `OwnMutationOpen = false`. Llegan a `Degrade` seis entradas, por `Fault` (`EBC:491-495`; desde `:140`, `:246`, `:345`, `:399`, `:433` y `:472`). Una sesión degradada no recibe eventos (H-36), así que ninguno la cerraría después. Ninguna guarda fija el cuerpo de `Degrade`: las de `TW/WorkspaceLifecycleGuardTests.cs:216` y `:230` miran otros cuerpos;
- nunca al final del callback, por `F4P:60`, ni en el drenaje, por IC-08. La variante «cerrar en `Settle`», que sería una desviación de C-F4-04, queda solo como respuesta declarada para Smoke-2 (abajo).

**Con la cota estructural,** un comando posterior del usuario nunca se registra como propio. Su `CommandWillStart` cierra la ventana antes de que encole nada.

**El comando ajeno, para la cota del billete** (D-T4C-02; la decisión del Owner sobre C12):
- **el manejador de `CommandWillStart`** de `AutoCadEventBridge` sigue igual en lo que ya hace: llama a `_core.OnSourceEvent(a.Session, a.Source)` y, si cambia el estado, a `RaiseCommandStateChanged()`, dentro de `ContainSession`;
- **y además, en la misma lambda:** si `e.GlobalCommandName` no es `EXECUTEFUNCTION` (sin distinguir mayúsculas), llama a `_core.NoteForeignCommandStart(a.Session)`;
- leer `e.GlobalCommandName` es leer un dato en memoria del argumento del evento, de coste constante. **No lee el DWG**: IC-04 (`ICL:77`) y `FRZ:216-219` quedan intactos. El arnés de H0 ya lo leía en su traza (`cmd=EXECUTEFUNCTION`, `H0T:37`);
- **las guardas del manejador siguen satisfechas** y no cambian:
  - sigue contenido (`TW/WorkspaceBridgeSourceGuardTests.cs:182-189`);
  - no nombra nada de `HandlerForbidden` (`TW/WorkspaceIdleSchedulingGuardTests.cs:64-90`);
  - la decisión `if (_core.OnSourceEvent(a.Session, a.Source)) RaiseCommandStateChanged();` sigue dentro del manejador contenido, y la cuenta de dos `RaiseCommandStateChanged();` no cambia (`TW/WorkspaceHostC2SourceGuardTests.cs:62-90`);
- **`NoteForeignCommandStart`**, una entrada nueva de `EBC`, suma uno a `ForeignCommandStarts`. Con una sesión degradada o desconocida no hace nada.

**Un cierre temprano no es inocuo** (RR N-07). Si la ventana se cerrara antes del último `Object*` propio, lo que quedara de POST se trataría como ajeno y marcaría B. Un cierre temprano **sistemático** violaría el oráculo de IC-08 (`ICL:151`), INV-28 y S2-14.
- **H0 no lo activó** (fila 5, H-43): no hay comandos anidados, y el `CommandWillStart` de `EXECUTEFUNCTION` llega antes de `Begin`. No se activa C12.
- **V3 queda como respuesta declarada para Smoke-2**, por si allí apareciera un cierre temprano sistemático. Es una ventana en dos fases (CK C3-01): mientras dura el callback, ningún evento la cierra, y `SettleOwnMutation(SessionId)`, al final de `Execute`, restablece las reglas normales. Si hiciera falta, es una A-n al Owner.

**La falta del fin de comando y de la entrada en reposo** (RR N-10). **H0 no la activó** (fila 5): los dos llegan después del callback en las cinco sondas (H-43). La variante **«cerrar en `Settle`»** queda como respuesta declarada para Smoke-2. Sería una **desviación expresa de C-F4-04** (`F4P:60`), y solo el Owner podría admitirla.

**La superficie nueva:**
- **`EBC`:**
  - `OpenOwnMutation(SessionId)`, `CloseOwnMutation(SessionId)` y `NoteForeignCommandStart(SessionId)` cambian su estado. Olvidan `_lastAllowedClose` exactamente una vez, antes de cualquier rama, y entran en la regla de olvido, que pasa de 13 a **16** entradas (H-18; IR R-07 (c));
  - `int? ForeignCommandStartsOf(SessionId)` es una lectura y no olvida. **Devuelve nulo para una sesión degradada o desconocida** (RR N-09), con `IsDegraded` (`EBC:144`). Así `RequestUpdate` responde `Unavailable` y un billete no empezado caduca (D-T4C-02). Sustituye a `QuiescentEntriesOf` de la v4;
  - ningún nombre contiene `Destroy`, `Detach`, `OnIdle` ni `Stop`;
  - V2, V3 o el cierre en `Settle`, si Smoke-2 los pidiera, añadirían un miembro más, con su olvido;
- **`IWorkspaceBridgeLink`:** `OpenOwnMutation`, `CloseOwnMutation` y `ForeignCommandStartsOf`, que implementan el `BridgeLink` del controlador (`CTL:321`) y el doble `WorkspaceSimulatedBridgeLink` (`TW/WorkspaceHostTestSupport.cs:53`). `NoteForeignCommandStart` no va en el enlace: solo la llama el manejador del puente;
- **`AutoCadEventBridge`:** reenviadores **`internal`**, y la sentencia nueva del manejador de `CommandWillStart`. Su superficie pública de nueve nombres no cambia (H-19; IR R-07 (d));
- **los demás manejadores no cambian:** no leen handles ni el DWG. IC-04 queda intacta;
- la sesión no lleva handles (`TW/WorkspaceNavigationEchoTests.cs:325-340`, sin cambios).

**El residual que se declara:**
- **lo ajeno dentro de la ventana:** las pistas no llevan rack (H-24). Durante la ventana, una escritura ajena sobre otro rack quedaría sin marcar. H0 midió que la ventana va del `Begin` al `CommandEnded` de `EXECUTEFUNCTION`, todo dentro del comando interno (H-43). Esa escritura solo podría venir de un reactor de otro complemento, que D-12 H-LOCK ya deja en `UNKNOWN` (`FRZ:494-498`), o de AutoCAD mismo;
- **«rack/member-bound»** queda nominal. El ámbito guarda el rack para el registro, pero no puede atribuir las pistas.

**Los `*Cancel*` previos** (IR R-06). **H0 no los activó** (fila 4, H-42): el puente no recibe ninguna pista de ellos, solo las dos entradas en reposo de la fila 3, que no marcan. **V1 y V2 no hacen falta** y quedan como respuestas declaradas para Smoke-2:
- V1 abriría la ventana completa en la aceptación;
- V2 abriría una antesala desde la aceptación hasta `Begin`, solo para las pistas `CommandEnded`.

**Lo que no se enmascara:**
- lo ya pendiente al abrir;
- todo lo posterior al cierre: un `U` inmediato, un RACKEDITAR de otro rack o cualquier comando del usuario. H0 midió que `U` y `REDO` marcan, como deben (O-H0-1).

**Por qué fue una decisión del Owner** (§9, O-02, ya decidida):
- `OO` exige «unable to mask another rack», y la vía A deja el residual;
- la vía B, la pista con handle, choca con el «coste constante» de `FRZ:217-218`, con IC-04 (`ICL:77`) y con la guarda de `WorkspaceSession` (`TW/WorkspaceNavigationEchoTests.cs:325-340`);
- además, la vía B vuelve a marcarlo todo por las escrituras propias sobre objetos que no son miembros (M3 R-M3-06).

**La IC nueva:** IC-09, «El ámbito de mutación propia del Actualizar del piloto». Este es su texto. La sesión lo registra en la custodia de la emisión, porque `docs/` está prohibido al Worker:

> Las pistas `DatabaseObjectChanged` y `CommandEnded` de una sesión que llegan mientras está abierto el ámbito de mutación propia de su Actualizar se registran aparte y solo invalidan las cachés de navegación. Las pistas ajenas ya pendientes conservan su efecto. El ámbito se abre en el contexto de comando, bajo el bloqueo y antes de PREPARE. Se cierra con el primer fin de comando de la sesión, que cuenta como propio; con el primer inicio de comando o la primera entrada en reposo de la sesión; con el cierre explícito del adaptador si la corrida no llega a ejecutarse; con la degradación de la sesión; y con su destrucción. Residual declarado: una escritura ajena sobre otro rack durante la ventana queda sin marcar. Concreta D-04 (O-15), C-F4-04, INV-08 e INV-28 sin cambiar el Freeze.

Si Smoke-2 pidiera V1, V2, V3 o el cierre en `Settle`, la IC añadiría la antesala, las dos fases o ese cierre, este último como desviación expresa de C-F4-04 y con una decisión del Owner.

**Cambios de guarda declarados:** las filas 14 a 17 y 22 de D-T4C-20.

**Anclas:** H-17 a H-19, H-24, H-42, H-43 y H-51; M3 §1.

### D-T4C-10. R-T4-07: una marca del host (M3 §2.7, opción (a))

*Cambia en la v2: va con la cota del billete (IR R-01) y sin `BVM` (IR M-07). Cambia en la v3: la cota cubre también el billete empezado (RR N-01). Cambia en la v5: la cota es «Completar o comando ajeno».*

**El cambio:**
- `DiscardRack` responde `Busy(UpdateInFlightReason)` si el borrador de ese rack está en Applying o si ese rack tiene un billete en vuelo;
- `DiscardDocument` responde `Busy` si la sesión tiene un billete en vuelo o algún borrador en Applying;
- **los dos:**
  - comprueban primero la caducidad (`ExpireStaleUpdate`);
  - lo miran después de `Authorize` y de `_pilotReadInProgress`, como pide `HostViolations` (`TW/WorkspacePilotEditGuardTests.cs:262-268`).

**Por qué la cota hace seguro (a)** (IR §2.9). Sin ella, un billete colgado convertiría el descarte en imposible y, con el veto de cierre (H-25), el documento no se podría cerrar sin perder el trabajo. Con la cota de D-T4C-02, «Completar o comando ajeno», el `Busy` dura:
- **normalmente,** hasta la compleción del `ExecutionResult`, que H0 midió que llega siempre, unos milisegundos después de salir del callback (H-40);
- **como mucho,** hasta el primer comando ajeno de la sesión, su cierre o su retirada, **también si el callback ya empezó** (RR N-01). En ese caso lo cubren primero la llamada terminal que garantiza el corredor y la compleción, y si faltaran, la caducidad del billete empezado o terminado. Solo un billete en `Execute` no caduca, y `Execute` es total.

**Un `RACKPANELDESCARTAR`** con el billete colgado es él mismo un comando ajeno: su `CommandWillStart` caduca el billete antes de que el comando llame a `DiscardDocument`, así que el descarte se admite. **Un cierre** vetado por el borrador Dirty también es un comando ajeno (`CLOSE`), así que el siguiente intento ya no encuentra `Busy`.

**Lo que no cambia:**
- **`DraftRegistry` y sus pruebas caracterizadas** (`TW/WorkspaceDraftRegistryContainmentTests.cs:383-411`);
- **la retirada por destrucción**, que no se puede negar y que R-T4-07 no nombra;
- **el caso `ClosedDetached` de T4B** (`TW/WorkspacePilotUpdateHostTests.cs:368-382`): sigue en verde, porque llama al registro y no a la operación del puerto.

**La presentación del `Busy` no necesita cambios.** `BVM` ya presenta el motivo del puerto (`BVM:945`), y `RACKPANELDESCARTAR`, también (H-25). `BVM` sale del alcance permitido (IR M-07).

**Anclas:** H-25; `T4P:560`; M3 §2.

### D-T4C-11. La UI de Actualizar (R-T4-09)

*Cambia en la v2: IR R-02 (c), R-09 y M-11. Cambia en la v3: RR N-15. Cambia en la v4: CK C3-03.*

**El botón:**
- «Actualizar» en el campo del piloto, en `WorkspacePanelView.xaml`, con `AutomationProperties.Name` y un manejador en el código subyacente dentro de `Contained`, como los demás (M3 H-M3-23);
- **se ofrece solo** en Dirty, también si está «posiblemente obsoleto», sin `InFlight` y fuera de Applying;
- con un diagnóstico persistente también se ofrece, y la corrida decide (M3 H-M3-42);
- no se ofrece en Clean, Conflict, Unknown, Error ni en la vista previa.

**El gesto `Update()` del VM del campo**, que es síncrono y sin mecanismos nuevos (H-15):
1. **Comprueba primero su botón** (`if (!ShowUpdateButton) return;`), como `CheckNow` (`FVM:397-399`). Así un clic ya encolado tras una corrida síncrona no pide otra entrada en el contexto de comando (IR M-11). **Después recoge lo pendiente** con `TakeUpdateProgress`: si encuentra un resultado, lo presenta y no pide nada. Una petición nueva nunca deja sin presentar un resultado anterior (RR N-15).
2. **Cruza la frontera `PilotBoundaryKind.Update`** (IR R-09; H-27):
   - el valor se añade **al final** del enumerado, como admite su guarda (`TW/WorkspacePilotEditGuardTests.cs:151-160`);
   - `CrossBoundary` lo traduce a `DraftBoundary.Update`, que es bloqueante (`CORE:1131`).
   - **`Proceeded`:** no había texto pendiente, o el válido se confirmó. Sigue al paso 3;
   - **`Aborted`:** hay un texto inválido o no hay autorización. Se presenta el mensaje del campo y no se pide nada;
   - **`Kept`:** el borrador no se puede editar. No se pide nada. La documentación general del valor dice que la transición sigue (`IWorkspacePilotEditPort.cs:155-159`), pero en Actualizar el botón no se ofrece en esos estados, así que `Kept` solo sale de una carrera, y no se pide;
   - **`Busy`:** hay una lectura en curso. No se pide nada y se presenta el motivo.
3. `RequestUpdate`.
4. Si es `Accepted`, `TakeUpdateProgress` en el acto. Presenta el resultado si ya llegó, o «Aplicando» si sigue en vuelo. Si `InFlight` es falso y no hay resultado, porque lo presentó un repintado anidado, no toca nada (D-T4C-03, regla 2).
5. Cualquier otro valor de inicio se presenta con su causa. También `Failed(HostNotRunReason)`, cuando la actualización anterior **de este mismo rack** acaba de caducar: el usuario vuelve a pulsar. La de otro rack no llega por aquí: va al almacén de ese rack (CK C3-03).

**La presentación de cada resultado**, siempre a partir del `EditUpdateOutcome` y sin reclasificarlo:
- **Applied:** aplicado;
- **AppliedWithWarnings:** aplicado, con los avisos visibles;
- **NotAppliedConflict:** con sus diferencias;
- **NotApplied\***: con su causa;
- **CommitThrew:** el texto A2-11 que ya existe (`FVM:56`);
- **AppliedPostRereadFailed:** el texto A2-12 (`FVM:57`);
- **ClosedInError:** con su clasificación (M-03, D-T4C-13);
- **Rejected, Failed y los demás:** con su causa.

**Unknown nunca se presenta como fallo ni como éxito.**

**`IsMine`.** Como en `CheckNow` (`FVM:412-416`): si la superficie cambió entre el clic y la presentación, `PresentSurfaceChange`, y el resultado sigue en el almacén (D-T4C-03, regla 3).

**K = 1** (M3 H-M3-33). Tras POST, la única superficie viva se repinta desde la memoria del puerto.

**Los avisos.** El VM los guarda como aviso del gesto y **no cambia la cola fijada de `PilotFieldView`** (M3 H-M3-29). Se declara un límite: si la superficie cambia después de presentar el aviso, el aviso se pierde. Ampliar la vista es la alternativa del §9, O-14.

**PICKFIRST.** Se acepta el vaciado que provoca `EXECUTEFUNCTION` (H-22):
- Actualizar no usa la selección (`TW/WorkspaceEditIntentGuardTests.cs:1098-1111`);
- la pestaña fijada no cambia ante un contexto sin rack (M3 §3.4);
- restaurarla sería una escritura de selección (§9, O-15).

**Los textos nuevos** son constantes del VM, con su prueba de tabla.

**Cambios de guarda declarados:** la fila 21 de D-T4C-20.

**Anclas:** H-15, H-27 y H-28; `T4P:568-571`; M3 §3.

### D-T4C-12. El diagnóstico diferido de T4A2 (M3 §4)

*Cambia en la v2: IR M-15. Cambia en la v3: RR N-05.*

**La opción (a) de M3 §4.3:**
- **`EditCheckOutcome`** gana `AdmissionDiagnostic` y `ClassicEditorHint` opcionales, como `EditConfirmOutcome`. No gana valores de `EditCheckStatus`, cuya guarda lo prohíbe (`TW/WorkspaceEditIntentHostTests.cs:1056`);
- **`TryCheckPilotEdit`** los rellena a partir de **la misma lectura** que ya hace, **antes** de `CheckNow`, en su propio `try`, que da `null` si algo falla;
- **`PresentCheck`** los guarda como diagnóstico persistente, en lugar de retirarlo ante `Recovered` o `Conflict`.

**D-1 no cambia** (`A2P:660`, «D-1 si añade una lectura»; IR M-15). T4C no añade ninguna lectura del dibujo ni ninguna llamada al lector:
- el diagnóstico sale de la lectura que `TryCheckPilotEdit` ya hace;
- la corrida de Actualizar lee por `ReadForPrepare`, que es de T4B.

**Pero la guarda de las cuentas por operación sí cambia** (RR N-05; H-37). La v2 decía que `TW/WorkspaceEditIntentGuardTests.cs:688-700` no cambiaba, y era falso:
- `:699-700` exige cero diagnósticos de admisión en `CheckBody()`, y D-T4C-12 pone uno;
- el diagnóstico nombra el resultado de la lectura que ya existe (`pilot.Read`), así que también sube su cuenta textual en ese cuerpo (`:682-685`).

Lo que no cambia es el número de **lecturas del dibujo**, que sigue siendo una por operación: no hay ninguna llamada nueva al lector. Los cambios van en la fila 12 de D-T4C-20.

**Separado de la clasificación:**
- `Recovered` y `Conflict` siguen saliendo **solo** de `StaleBase.CompareWith` (`DS:720-731`);
- un fallo del diagnóstico nunca convierte en `Failed` una respuesta cuyo `CheckNow` ya cambió el borrador;
- el diagnóstico solo afecta a la caja y a las opciones, nunca al estado del borrador ni al DWG;
- es «A diagnostic failure must not alter authored state» de `OO`.

**Cambios de guarda declarados:** la fila 12 de D-T4C-20, más la tabla H-19 de `A2P:135-145`. La regla de UI de D-T4A2-06 cambia de «retira ante `Recovered` o `Conflict`» a «retira salvo que la respuesta traiga diagnóstico».

**La limitación que se declara:** la regla del origen de MUTATE difiere de la de «Comprobar ahora» (`T4BP:843`). Puede quedar un diagnóstico persistente mientras Actualizar admite (M3 R-M3-16).

**Anclas:** M3 H-M3-39 a H-M3-42; `A2P` §8.

### D-T4C-13. Los arrastres I-01, M-01, M-03 y M-06, y C2-M-3 a C2-M-7

*Cambia en la v2: IR M-02, M-03 y M-05. Cambia en la v3: RR N-13.*

| Arrastre | Dueño exacto | ¿Lo cierra T4C? | Prueba o guarda discriminante, con su negativa |
|---|---|---|---|
| **I-01:** los tres textos del host | `CORE:48` (`UpdateNotRunReason`) y los literales de `CORE:915` y `:921` | **Sí.** Pasan a constantes de `RUN` (`NoActiveDocumentReason`, `ModalOpenReason` y `UpdateNotRunReason`), sin cambiar la redacción. **No se amplía** a una limpieza de localización | La guarda de textos (`TW/WorkspaceEditIntentGuardTests.cs:1101-1111`) se extiende a `UpdateGate` y a los cuerpos nuevos (`RequestUpdate`, `TakeUpdateProgress`, `ExpireStaleUpdate` y la clase de corrida): ninguna asignación `reason = "…"` ni ningún literal en una fábrica de `EditUpdateOutcome`, `EditUpdateStart` o en los `Busy` nuevos. Negativas: `reason = "x";` en `UpdateGate`, y `Closed(EditUpdateStatus.NotAppliedFailure, "x", …)` en el cuerpo |
| **M-01:** la totalidad defensiva | (1) El `catch` exterior de `Execute` (`RUN:144-147`); (2) `!closed.Accepted` (`CORE:870`); (3) el respaldo y el `catch` del host (`CORE:852`, `:858-862`); (4) la respuesta fuera de `try` (`CORE:868-904`, M2 H2-40); (5) la capa nueva de T4C | **Sí, en los cinco** | (1) `Execution` se crea fuera del `try`, y el `catch` aplica la misma precedencia que el de `Run` (`_sealReturned`, `_writeStarted`). Así nunca da NotAppliedFailure tras un `WriteUnit` o un `Seal`. (2) Con `closed.Kind` de Applied\* o CommitThrew, `ClosedInError` con la clasificación y `UpdateNotClosedReason`, **nunca un éxito**: el borrador no se rebasó (IR M-03; `DS:536-537`). La documentación del valor se amplía, porque el borrador no está en Error (H-38; RR N-13). Con cualquier otro valor de `Kind`, se mantiene NotAppliedFailure. (3) Queda como respaldo muerto de una corrida total, y la guarda fija la totalidad de (1). (4) El estado se calcula antes, y la construcción de la respuesta va en un `try` con el respaldo `Closed(status, LastApplyCause, [], [])`. Las cuatro son inalcanzables hoy y van por guarda de fuente, con negativas: el `catch` sin consultar el sello, y `:870` con NotAppliedFailure o Applied fijos. (5) Las pruebas de Core del corredor: un `Dispose` del bloqueo o una llamada que lanza después del callback no cambian el resultado guardado (pruebas 14, 15 y R-02 de D-T4C-17) |
| **M-03:** `ClosedInError` sin clasificación | `CORE:872-877`; `WorkspaceEditIntent.cs:472` | **Sí** | `ClosedInError(reason, classification, cause, differences, warnings)` lleva el estado de 7d calculado a partir de `closed.Kind`. Se extiende `TW/WorkspacePilotUpdateHostTests.cs:383-397`: con Applied y un error diferido, la clasificación es Applied y los avisos llegan. Dos variantes: Conflicto con diferencias, y CommitThrew con el desconocido visible. **T4C no añade ningún llamador de producto de `DraftSession.EnterError`.** La guarda: `\.EnterError\s*\(` no aparece en `src/RackCad.Application/` ni en `src/RackCad.Plugin/`. El `EnterError` privado del VM del panel queda fuera, porque es ajeno y no lleva punto (`src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs:83`, `:292`; IR M-02). El bloque diferido congelado de `DS:555-559` no se toca. `ClosedDetached` queda fuera: con D-T4C-10 solo lo alcanza la destrucción de la sesión, que no tiene panel |
| **M-06:** las pruebas débiles y `Closed` sin copia | `TW/WorkspacePilotUpdateRunTests.cs:691-705`, `:753-777`, `:800-810` y `:826-835`; `TW/WorkspacePilotUpdateHostTests.cs:399-414` y `:435-458`; `WorkspaceEditIntent.cs:477-483` | **Sí** | (a) Dos corridas sobre el **mismo** doble. (b) La causa empieza por `PostEnvelopeDiffersPrefix + "B2"`. (c) La escala: si la rama `PostScaleDiffersReason` (`RUN:172-173`) se puede alcanzar con los sobres iguales, la prueba lo exige; si no, la rama se declara inalcanzable y una prueba lo fija (M1 P-08). (d) Los nombres deseados con un rack compuesto y un corte posterior. (e) `TheAnswerOfAnUpdateAlwaysComesFromTheClose…` gana una fila por clase de desenlace. (f) `Closed` copia al recibir, y la prueba muta la lista de origen después |

**C2-M-3 a C2-M-7** (R-T4-13, «donde se toquen esas superficies»; IR M-05):

| Id | Superficie | Decisión |
|---|---|---|
| C2-M-3 y C2-M-4 | `TW/WorkspaceCommandAdvisorGuardTests.cs` | **Se difieren.** T4C no toca ese archivo. El de los dobles, `TW/WorkspaceCommandAdvisorTests.cs`, es otro |
| C2-M-5 | Nombres y comentarios desfasados en seis archivos de guarda | **Se cierra solo en el archivo que T4C toca:** el nombre de `TW/WorkspaceLifecycleGuardTests.cs:295`, según su definición en T3C2. Las otras cinco superficies se difieren, porque T4C no toca esos archivos |
| C2-M-6 | `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:18`, `:73` | **Se difiere.** El mapa de T4B la señala como «superficie de comando, T4C», pero T4C no necesita ese archivo, y R-T4-13 solo obliga donde se toca |
| C2-M-7 | `BVM:356` | **Se difiere.** `BVM` sale del alcance (D-T4C-10) |

**Anclas:** H-02, H-03 y H-04; `T4BC` §5 a §7; M1 §6; `docs/automation/evidence/I-64-pilot/F4-DRAFTS/t4b/map-3-carries-guards-rules.md:156-162`.

### D-T4C-14. «NO SECOND WRITE PATH»: una guarda de todo el producto

*Cambia en la v2: IR R-12 y M-14. Cambia en la v3: RR N-02 y N-03.*

Va en un archivo nuevo, `TW/WorkspacePilotUpdateIntegrationGuardTests.cs`. Su contenido es el de M1 §5.4 ampliado. Cada fila lleva su negativa sobre el texto real.

**G-1. El censo de las primitivas de Push Back.** `PushBack(System|Frontal|Planta)DrawService` con `.PrepareRedraw(` o `.RedrawInTransaction(` solo aparecen en `WRITER`, en `PrepareUnits` y `WriteUnit`.
- Negativa: inyectarlas en el adaptador y en `CTL`.

**G-2. Un solo implementador de producto de `IPushBackPilotUpdatePort`:** `RackPilotUpdateWriter`.
- Negativa: una clase `: IPushBackPilotUpdatePort` en otro archivo.

**G-3. Ningún tipo de larga vida guarda nada de una corrida** (IR M-14; RR N-03).
- **Una sola construcción del escritor**, `new RackPilotUpdateWriter(`, en `CreateWriter` del objetivo de `ADAPTER`.
- **Una sola construcción del corredor**, `new PushBackPilotHostRunner(`, en `HOSTRUN`, dentro de su `Request` estático. `PushBackPilotHostRunner.Request(` solo aparece en `TryRequest` de `ADAPTER`.
- **Los tipos de larga vida** son el núcleo (`WorkspaceHostCore`), el controlador (`CTL`, con sus anidados), y el ejecutor `RackPilotUpdateCommandContext` y `Primitives` de `ADAPTER`. Ninguno tiene un campo, estático o de instancia, de tipo `IPushBackPilotUpdatePort`, `RackPilotUpdateWriter`, `PushBackPilotHostRunner`, `IPushBackPilotHostRun` ni `IPushBackPilotHostTarget`.
  - En el núcleo se comprueba por reflexión, sobre sus campos propios y los de sus tipos anidados **con nombre**. Los que genera el compilador, cuyo nombre empieza por `<`, quedan fuera.
  - En `CTL` y en `ADAPTER` se comprueba por texto, porque ninguna prueba referencia el Plugin.
- **Quedan exentos,** porque viven lo que vive una petición:
  - el corredor por petición, que retiene `run`, el id y su estado entre `Request` y el callback diferido;
  - `Target`, con su único `_document`;
  - las clases de clausura que genera el compilador.
  
  La v2 los incluía, y la guarda era imposible de satisfacer (RR N-03).
- Negativas:
  - un campo de cada uno de esos tipos en el núcleo, en `CTL` y en el ejecutor;
  - un campo estático `PushBackPilotHostRunner` en `HOSTRUN`;
  - un `new PushBackPilotHostRunner(` fuera de `Request`;
  - una segunda construcción del escritor.

**G-4. Un solo llamador de producto de `TryUpdatePilot(`:** el `Execute` de la clase de corrida del núcleo, con una sola llamada. `PushBackPilotUpdateRun.Execute(` solo aparece en `CORE`.

**G-5. El adaptador y el corredor sin API de escritura authored.** No nombran:
- `RedrawInTransaction`, `RedrawInPlace`, `DrawAndPlace` ni `SystemBlockWriter`;
- `RackBlockData.Write`, `ForWrite`, `UpgradeOpen` ni `AppendEntity`;
- `.Commit(`, `StartTransaction` ni `StartOpenCloseTransaction`;
- `SyncName`, `PurgeUnreferenced`, `EnsureForPlan`, `WblockCloneObjects`, `SetAt(` ni `.Erase(`;
- `SendStringToExecute` ni `CommandMethod`;
- ni los caminos clásicos: `RackPushBackCommands`, `EditPushBack`, `PushBackKindHandler`, `RackMenuCommands`, `RackViewBatch*`, `SiblingRedraw*`, `AutoCadSiblingRedrawPort`, `RackSelectivoInsertIntegration` ni `RackCommandSupport`.

**G-6. Las dos carpetas Workspace**, con una regla única que lleva la lista de G-5 y `LockDocument`. Las excepciones son las de hoy, por archivo exacto y con su cuenta (M1 H-20, que la emisión vuelve a medir):
- `WorkspaceNavigationEffects.cs`: dos `StartTransaction` y dos `LockDocument`;
- `WorkspaceNavigationFacts.cs`: uno y uno;
- `WorkspaceNavigationHost.cs`: dos `LockDocument`;
- `WorkspaceSelectionReader.cs`: uno y uno;
- **`WorkspacePanelCommand.cs`: dos `[CommandMethod(`**, `RACKPANEL` y `RACKPANELDESCARTAR` (H-34). La v2 la olvidaba, y G-6 era imposible de satisfacer (RR N-02). Se conserva `CommandMethod` en la regla, en lugar de quitarlo, para que un comando nuevo en la carpeta salte también aquí y no solo en el censo de 40.

Fuera de esas cuentas, nada. La v1 dejaba la excepción sin enumerar (IR R-12). Las negativas son una por excepción, cada una con su cuenta más uno: por ejemplo, un tercer `[CommandMethod(` en `WorkspacePanelCommand.cs`. Además, un `[CommandMethod(` en `WorkspaceController.cs`.

**G-7. Las llamadas de escritura del puerto en Application** (`.PrepareUnits(`, `.BeginMutation(`, `.WriteUnit(`, `.Seal(` y `.Post(`) solo aparecen en `RUN`.

**G-8, opcional.** El conjunto de archivos de `src` con `.Commit(` no crece. Es frágil (M1), y la revisión decide si entra.

**G-9. El piloto y la escritura, en todo el Plugin y en Application** (IR R-12). Ningún archivo de `src/RackCad.Plugin/` ni de `src/RackCad.Application/` que nombre el piloto (`PushBackPilot\w*`, `RackPilot\w*` o el espacio de nombres del piloto) usa la API de G-5 ni `LockDocument`. Las únicas excepciones, con sus cuentas exactas (H-30):
- `WRITER`, con las cuentas de hoy;
- `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`: un `LockDocument` y un `StartTransaction` (`:160-161`);
- `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`: dos `LockDocument`;
- `ADAPTER`: un `LockDocument`.

Negativas:
- un archivo nuevo del Plugin que nombre `RackPilot…` y llame a `RedrawInPlace`;
- uno que llame a `RackBlockData.Write`;
- un `ForWrite` en `RackPilotIntentAdapter.cs`;
- un segundo `LockDocument` en `ADAPTER`.

**G-10. El censo de los llamadores del vuelo:**
- `.RequestUpdate(` solo en `Update()` de `FVM`, en el reenvío de `CTL` y en el de la compuerta;
- `.TryRequest(` solo en `RequestUpdate` del núcleo;
- `.OpenOwnMutation(` y `.CloseOwnMutation(` solo en la clase de corrida del núcleo y en los reenvíos. En particular, `ExpireStaleUpdate` y `TakeUpdateProgress` no nombran ninguno de los dos (RR N-04). Negativa: `CloseOwnMutation` inyectado en `ExpireStaleUpdate`.

**Anclas:** M1 §5; H-30.

### D-T4C-15. Undo

*Cambia en la v2: IR M-11. Cambia en la v5: Undo medido en el diagnóstico de H0.*

**La frontera medida** (M2 §4, H-07; H0, H-43 y H-44):
- un Actualizar es **un** comando interno, `EXECUTEFUNCTION`, que contiene:
  - la importación sin transacción;
  - las lecturas confirmadas;
  - **una** transacción authored;
  - la purga y los `SyncName` en sus transacciones;
  - el regen;
- no hay ninguna marca ni API de Undo (H-23);
- **T4C no añade ninguna:** ni `StartUndoMark` u otra API de Undo, ni `SendStringToExecute`, ni `[CommandMethod(`, ni `CommandFlags`. La guarda de D-T4C-14 y el censo de 40 lo fijan.

**Una lectura que se declara.** El renombrado y la purga son POST (`FRZ:519`), y la importación es residuo OM-5 (`FRZ:504`). Por tanto hay **una** transacción authored, y la condición «more than one authored mutation transaction» no se activa (O-12, aceptada por el Owner).

**Lo que midió H0, en el diagnóstico** (H-44, H-45):
- **un solo paso de Undo:** la lista de Deshacer muestra **una** entrada «Executefunction» por Actualizar, que agrupa la importación, MUTATE, la purga y los `SyncName`;
- **Undo y Redo completos:** un `U` deshace el Actualizar entero, y «Comprobar ahora» da Conflicto en las dos vistas; un `REDO` **inmediato** lo rehace entero;
- **el paso vacío existe:** un `EXECUTEFUNCTION` sin escritura (P1, P2 y P3: el contexto vacío y un `NothingToApply`) deja su propia entrada «Executefunction» en Deshacer.

**Lo que eso significa:**
- **la condición «Undo requires a material architecture change» no se activa:** el contexto de comando da un solo paso. No hace falta la A-n de D-13 hacia un comando registrado;
- **no es todavía el cumplimiento.** H0 midió el escritor de T4B con un arnés, sobre un dibujo sintético. **Smoke-2 lo confirma con el producto** (S2-05, `FRZ:885`), y no hay ninguna afirmación de cumplimiento antes (`OO`, «UNDO»).

**Lo que se declara para Smoke-2:**
- **el paso vacío** (H-45): un Actualizar que no llega a importar deja una entrada «Executefunction» vacía en Deshacer. Son los casos de `NothingToApply`, de un rechazo dentro del callback antes de la importación de PREPARE y de un billete caducado. **Un Conflict no deja un paso vacío** (CK5 M5-03): sale en MUTATE, después de la importación de `PrepareUnits`, así que su paso de Undo lleva el residuo OM-5 y lo prueba solo AutoCAD (Smoke-2). Es comportamiento de AutoCAD con `ExecuteInCommandContextAsync`, y T4C no lo evita. Se informa al Owner como observación; no es una condición de devolución;
- **las lecturas del panel vacían la pila de Rehacer** (O-H0-2, H-51): «Comprobar ahora» y «Editar» dejan entradas «Group of commands» en Deshacer, y un «Comprobar ahora» entre `U` y `REDO` da «Nothing to redo». No es de T4C, sino de las lecturas del panel de F4-T3 y F4-T4A. En Smoke-2, Redo se prueba **inmediatamente** después de Undo, y este comportamiento se declara.

**Lo que prueban las pruebas unitarias:**
- una entrada en el contexto, un bloqueo y una corrida de T4B por Actualizar;
- una transacción authored, por las guardas de T4B que no cambian;
- ninguna API de Undo en el producto.

**Lo que sigue siendo solo de AutoCAD,** y va a Smoke-2:
- el paso único, Undo, Redo y la agrupación de la importación, con el producto;
- el paso propio de un NotApplied con residuo OM-5;
- el nombre en la lista de Undo, que H0 vio como «Executefunction».

**Si Smoke-2 diera más de un paso,** sería una A-n hacia el comando registrado de D-13 (`FRZ:542-543`), y una devolución material:
- cambia el censo de 40;
- añade `SendStringToExecute`, prohibido en varias guardas;
- según dónde se registre, toca los `*Commands*.cs` o T1B (M4 DV-02).

Es la parada C02.

**Anclas:** H-07, H-23, H-43 a H-45 y H-51; `T4P:573`; M2 §4 y §5.

### D-T4C-16. La sonda de host T4C-H0, antes del producto (R-T4-05, «primer paso»)

*Cambia en la v2: IR R-06, R-10 y M-11. Cambia en la v3: RR N-07, N-10 y N-12. Cambia en la v4: la fila 5 (CK C3-01). Cambia en la v5: **H0 está hecho.** La tabla lleva el resultado medido y la acción que se tomó.*

**El hecho:**
- R-T4-05 pone la medición del callback y de Undo como primer paso de T4C (`T4P:550`; `DSP:1701`);
- el Owner eligió «H0 con dispensas» (O-01), con las tres dispensas de abajo;
- **H0 se hizo el 2026-11-15** y está custodiado en `44fe029d` (F4-E63 y la evidencia 206; §2.6).

**Las tres dispensas expresas del Owner** (IR R-10), tal como se pidieron:
1. **Frente a `OO`, que llama a Smoke-2 «the first physical validation».** H0 ejercita la vía de escritura del piloto antes de Smoke-2. La dispensa la declara una **medición de diagnóstico**, no una validación. Smoke-2 sigue siendo la primera validación del producto, y nada de H0 cuenta como evidencia de cumplimiento.
2. **Frente a §12.1,** en cuatro cláusulas:
   - H0 no es un hito de `FRZ:821-822`;
   - su build no es la build completa de un hito, con «UI Full local» y la CI exacta (`FRZ:828-829`);
   - **el DLL Debug no sale del worktree de I-64**, que §12.1 exige (`FRZ:830`), sino del worktree desechable (RR N-12). Se cargó ese DLL, con su SHA-256 medido por la sesión (`H0R:5-19`);
   - el Owner opera, como en `FRZ:833`. **No se cumplió:** ver las desviaciones.
3. **Frente a F4-E59** (`DSP:2086`), que prohíbe las sondas con entradas válidas en caminos de escritura. La dispensa admite H0 como sesión de host, sobre una copia desechable.

**El arnés y su custodia,** como se hicieron (`H0N`; la revisión y la re-revisión del arnés, en la misma carpeta):
- la sesión escribió el arnés sobre `b15072ff`, en un worktree desechable, sin commit y sin rama;
- lo revisó un revisor independiente, y la segunda versión quedó «APTO PARA OPERAR»;
- se custodiaron el parche, su SHA-256, el SHA de la base, los SHA-256 de los DLL, el procedimiento, la traza, los datos medidos y el resultado;
- la sonda corrió sobre una copia del dibujo, que se cerró sin guardar, y el SHA-256 del original no cambió (`H0R:5-19`).

**Las desviaciones del procedimiento** (H-52), que se declaran al Owner (§9):
- **el operador fue la sesión, no el Owner,** por la instrucción del Owner («Crealo tú controlando autocad, no pidas autorización»). Contradice la cuarta cláusula de la dispensa 2;
- **el dibujo es sintético,** creado por la sesión con RACKPUSHBACK y RACKEDITAR, y no un dibujo real del Owner;
- **el RELEASE de I-52** se pidió. I-52 respondió después de la sonda: «SIN OBJECIÓN», sin ninguna sesión de host y sin emitir RELEASE, porque no concede ventanas a otras iniciativas (`F4-DRAFTS/t4c/h0/i52-reply.md`).

**El resultado, fila por fila** (`H0R:31-46`):

| # | Medida | Resultado medido | Acción tomada |
|---|---|---|---|
| 1 | El momento del callback | **Diferido** en las cinco sondas, de 49,6 a 145,8 ms después de volver (H-39) | Ninguna: F-1 tolera los dos casos, y el diferido es el real |
| 2 | ¿Se completa siempre? | **Sí**, en las cinco, en el hilo de la UI (H-40). La compleción es la de un `ExecutionResult`, no la de un `Task` (H-50) | La compleción es la liberación normal del billete (D-T4C-02). La guarda del adaptador se reescribe para `OnCompleted` (D-T4C-04) |
| 3 | ¿Llega una entrada en reposo **antes** del callback? | **Sí, en las cinco:** dos `EnteringQuiescentState` antes del `CommandWillStart` de `EXECUTEFUNCTION` (H-41) | **C12, resuelto por el Owner** con «Completar o comando ajeno» (D-T4C-02). Las entradas en reposo dejan de contar |
| 4 | ¿Los `*Cancel*` producen `CommandCancelled` o `CommandEnded` antes de `EXECUTEFUNCTION`? | **No** (H-42) | Ninguna. V1 y V2 quedan declaradas para Smoke-2 (D-T4C-09) |
| 5 | El orden de los eventos y `F4P:60` | `CommandWillStart(EXECUTEFUNCTION)` → el callback, con la importación, MUTATE, POST y la relectura → `CommandEnded` → `EnteringQuiescentState`. **`F4P:60` se cumple.** No hay comandos anidados (H-43) | Ninguna: es lo que prevé D-T4C-09. Ni el cierre temprano sistemático (N-07) ni la falta de los dos eventos (N-10) se activan. V3 y el cierre en `Settle` quedan declarados para Smoke-2 |
| 6 | Un solo paso de Undo, Undo y Redo | **Un solo paso,** con Undo y Redo completos (H-44) | Ninguna. Smoke-2 lo confirma con el producto (D-T4C-15) |
| 7 | El paso vacío | **Existe** (H-45) | Se declara para Smoke-2 y se informa al Owner (D-T4C-15) |
| 8 | El estado al entrar en el callback | El documento admitido, `TopTransaction` nula, `CommandInProgress=EXECUTEFUNCTION` y sin reposo (H-46) | Ninguna |
| 9 | Los bloqueos anidados | **Funcionan** (H-47) | Ninguna |
| 10 | El contexto de sincronización y la continuación | `DispatcherSynchronizationContext` al pedir, y la continuación en el hilo de la UI (H-40) | Ninguna |
| 11 | ¿Bombea WPF dentro del callback? | **No** (H-48) | Ninguna |
| 12 | El `Debug.Assert` del drenaje | **No salta** (H-49) | Ninguna |

**Lo que el resultado dejó al paquete:**
- todos los hechos [H] de la tabla pasan a [M] (§2.6);
- **una sola fila dio C12, la 3,** y el Owner la resolvió antes de esta v5. Ninguna fila dio C02 ni C05;
- además, H-50 obligó a reescribir la guarda del adaptador, y O-H0-2 pasa a Smoke-2.

**Anclas:** H-22, H-23, H-33 y H-39 a H-52; `T4P:550`; `FRZ:817-834`; la decisión del Owner sobre C12.

### D-T4C-17. Las pruebas automatizadas y la contención

*Cambia en la v2: IR R-01 a R-03, R-05, M-08 a M-11. Cambia en la v3: RR N-01, N-08, N-09 y N-15. Cambia en la v4: CK C3-02 y C3-03. Cambia en la v5: las pruebas de la cota siguen la regla del Owner, y se añaden las regresiones de lo que midió H0.*

**Los dobles nuevos**, en `TW/WorkspacePilotUpdateHostDouble.cs`:
- **las primitivas y el objetivo,** con estos modos:
  - síncrono y diferido;
  - nunca, con el `ExecutionResult` sin completar, que es lo que da un rechazo nativo (H-50);
  - **el orden medido por H0** (H-41, H-43): dos `EnteringQuiescentState` y el `CommandWillStart` de `EXECUTEFUNCTION` antes del callback, y `CommandEnded` y `EnteringQuiescentState` después;
  - lanza al pedir, antes y después de un callback síncrono;
  - lanza al bloquear y lanza al liberar el bloqueo;
  - documento distinto, cerrado y con id reciclado;
  - callback repetido;
- **el ejecutor,** que cuenta las llamadas a `TryRequest`;
- **un registrador de la corrida,** con la opción de que `NotRun` o el registro lancen.

El doble del ámbito de T4B, `TW/WorkspacePilotUpdateScopeDouble.cs`, se consume sin cambios. El doble del puente, `WorkspaceSimulatedBridgeLink` (`TW/WorkspaceHostTestSupport.cs:53`), gana los tres miembros nuevos.

**Las clases nuevas empiezan por `Workspace`**, para entrar en el filtro de prefijo (`T4BP:714`):
- `WorkspacePilotUpdateHostRunTests` (HR): el corredor, con las primitivas dobles;
- `WorkspacePilotUpdateRequestTests` (RQ): el núcleo real (`TW/WorkspaceHostTestSupport.cs`), con el ejecutor doble y el doble del ámbito;
- `WorkspaceOwnMutationTests` (OM): `EBC` y la sesión, con la fuente simulada;
- `WorkspacePilotUpdateIntegrationGuardTests` (IG).

| # (`OO`) | Prueba | Archivo y doble | Qué prueba exactamente |
|---|---|---|---|
| 1 | El documento correcto se admite | HR | Con el activo igual al admitido: `TryOpenTarget` una vez, y `Lock` y `CreateWriter` una vez sobre ese objetivo; `Execute` una vez |
| 2 | El documento equivocado se rechaza antes de crear el escritor | HR y RQ | Con un documento distinto o cerrado, `TryOpenTarget` falla: `Lock` 0, `CreateWriter` 0, `Execute` 0 y `NotRun(Rejected, DocumentMismatchReason)` una vez. En RQ, el resultado es `Rejected`, el billete se quita y el borrador no cambia |
| 3 | Una sesión cerrada se rechaza | RQ y HR | (a) Al aceptar: `Rejected` y cero `TryRequest`. (b) Destruida entre la aceptación y el callback: `TryOpenTarget` falla primero (IR M-09), `Lock` 0 y cero `Begin` del puerto de T4B, sin billete residual ni resultado guardado. (c) **Un id nativo reciclado** (H-29): `TryOpenTarget` encuentra el documento nuevo y `Admit` lo para, porque el testigo ya no vale. `Lock` 0 |
| 4 | `ExecuteInCommandContextAsync` una vez | HR, RQ e IG | `EnterCommandContext` una vez por petición y `TryRequest` una vez por aceptación. La guarda de D-T4C-04: una llamada textual en el producto |
| 5 | El bloqueo se toma una vez, del documento de destino | HR e IG | `Lock` una vez, sobre el objetivo resuelto por el id admitido, y antes de `Begin`, `CreateWriter` y `Execute`. La guarda: un `.LockDocument()` |
| 6 | El escritor se crea una vez por corrida | HR | `CreateWriter` una vez. Con el callback repetido por el doble, la cuenta sigue en uno |
| 7 | El escritor se libera en el éxito | HR | `Retire` una vez, después de `Execute` |
| 8 | El escritor se libera en el fallo | HR | Con un `Execute` que lanza, o con un fallo después de `CreateWriter`: `Retire` una vez |
| 9 | T4B se invoca exactamente una vez | RQ | El `Begin` del puerto de T4B una vez por aceptación, también con dos clics y con el callback repetido |
| 10 | Un fallo del contexto antes de la admisión no ejecuta T4B | HR y RQ | La llamada lanza antes del callback, o no llega ni el callback ni la compleción, como en un rechazo nativo (H-50): `Execute` 0 y cero `Begin` del puerto. El estado y la causa del borrador, y la marca de limpio, quedan sin cambios. En el primer caso solo hay valor de inicio `Failed` y nada en el almacén. En el segundo, el billete caduca con el primer comando ajeno y guarda `Failed(HostNotRunReason)` una vez |
| 11 | Un fallo del bloqueo no muta nada | HR | `Lock` lanza: `Begin` 0, `CreateWriter` 0, `Execute` 0 y `NotRun(Failed, LockFailedPrefix…)` |
| 12 | Con un Conflict de T4B, el host no escribe nada más | RQ y HR | Conflicto en el doble del ámbito: cero `WriteUnit`; ningún segundo `CreateWriter` ni `Execute`, ni ninguna escritura después de `Execute` (IR M-08); y el resultado es NotAppliedConflict con sus diferencias |
| 13 | El fallo de T4B se conserva | RQ | NotAppliedFailure, Preflight y Admission: `Status` y `Reason` idénticos a los de `TryUpdatePilot` |
| 14 | El desconocido de T4B se conserva | RQ y HR | CommitThrew (el `Seal` lanza) y AppliedPostRereadFailed, idénticos. Un `Dispose` del bloqueo que lanza después no los cambia (M-01, punto 5) |
| 15 | Applied se conserva | RQ | Idéntico. Clean solo por la clasificación de T4B |
| 16 | AppliedWithWarnings se conserva | RQ | Idéntico, con los avisos iguales y copiados (M-06 (f)) |
| 17 | El ámbito de mutación propia se abre y se cierra una vez | OM, HR y RQ | Se abre una vez, en `Begin`. Se cierra con el `CommandEnded`, y un segundo no tiene efecto; o con el primer `CommandWillStart`, aunque el reposo no cambie (IR R-05); o con `EnteringQuiescentState`; o con `NotRun` después de `Begin`; o con la destrucción. No se abre si el fallo es anterior al bloqueo. Al drenar, lo propio da solo la invalidación, y B no se marca (INV-28). Una pista ajena ya pendiente sí marca, y un `U` posterior también. **Sin el `CommandEnded` del comando interno, un comando posterior marca B** (IR R-05). Un `ImpliedSelectionChanged` ajeno no marca (IR M-01) |
| 18 | Un Actualizar concurrente se rechaza | RQ | Con el doble diferido, el segundo `RequestUpdate` da `Busy`, `TryRequest` sigue en uno, y tras terminar se acepta otro |
| 19 | No se retiene ningún escritor tras la corrida | HR, RQ e IG | `Retire` en toda salida. Por reflexión, ningún campo de los tipos de G-3 en el núcleo, sus anidados ni `HOSTRUN`. Y G-3 |
| 20 | Ninguna escritura del DWG fuera de la vía de T4B | IG | G-1 a G-7, G-9 y G-10, cada una con su negativa |

**Las pruebas que añade la revisión:**

| Hallazgo | Archivo | Qué prueba |
|---|---|---|
| R-01, la cota | RQ y HR | El modo «nunca»: tras un `CommandWillStart` **ajeno** simulado, `TakeUpdateProgress` da `Failed(HostNotRunReason)` una vez, y los descartes y `RequestUpdate` vuelven a admitirse. Un callback tardío con el testigo viejo da `Lock` 0 y `Execute` 0, y no deja ningún segundo resultado. Con la retirada de la sesión, el billete y el almacén desaparecen |
| **H0, fila 3: las entradas en reposo no cuentan** | RQ y OM | Con el orden medido, las dos `EnteringQuiescentState` y el `CommandWillStart` de `EXECUTEFUNCTION` antes del callback **no** caducan el billete, y la corrida se ejecuta. Es la regresión de C12 |
| **La compleción libera** | RQ y HR | Tras `Execute`, el billete sigue puesto (`Busy`, `InFlight` falso) hasta `Completed()`, que lo libera una vez. Una segunda compleción no hace nada. Una compleción sin callback (inalcanzable) guarda `Failed(HostNotRunReason)` |
| **El comando ajeno, en el puente** | IG | En el bloque de `CommandWillStart` de `AutoCadEventBridge`, `NoteForeignCommandStart` se llama una vez, dentro de `ContainSession`, condicionada a que `GlobalCommandName` no sea `EXECUTEFUNCTION`, y después de la decisión de `RaiseCommandStateChanged`. Ningún otro bloque la nombra. Negativas: la llamada sin la condición, y la llamada en otro bloque |
| R-02 (a) | HR | La llamada lanza después de un callback síncrono que escribió: ningún `NotRun`, `Request` verdadero, y el resultado guardado es el de T4B |
| R-02 (b) | RQ | La llamada lanza antes del callback: valor de inicio `Failed` y almacén vacío, con una sola presentación |
| R-02 (c) y (d) | UI | Un repintado anidado consume el resultado, y el gesto ve `InFlight` falso con `Unavailable` y no toca lo presentado. El consumo solo lo hace la superficie de ese rack |
| R-03 | RQ | Las cinco operaciones de D-T4C-08 durante el vuelo, `TryConfirmBase` incluida: `Busy` o `Waiting`, y el borrador sin cambios |
| R-09 | UI | La frontera: `Proceeded` (con texto válido y sin texto) pide; `Aborted`, `Kept` y `Busy` no piden |
| M-10 | HR | Con `NotRun`, `Execute` o el registro lanzando, `OnCallback` nunca lanza |
| M-11 | UI | Un segundo `Update()` con el botón ya oculto no llama al puerto |
| **N-01, la liberación del billete empezado** | **RQ, con el núcleo real** y el doble de las primitivas | Con `Lock`, `Begin`, `CreateWriter` o `Retire` lanzando, el billete se quita siempre, el ámbito se cierra si se abrió, y un `RequestUpdate` posterior se acepta. Con `CreateWriter` lanzando, `Retire` no se llama sobre un escritor nulo. Con un doble que llama a `Admit` y vuelve sin llamada terminal, el billete caduca con el siguiente comando ajeno |
| N-01 | HR, con un registrador doble | Después de un `Admit` verdadero, exactamente una llamada a `Execute` o a `NotRun`, también si `Retire` o el `Dispose` lanzan |
| N-08 | RQ | `TryConfirmBase` durante el vuelo, con una relectura que difiere: `Waiting`, y el borrador sigue Dirty |
| N-09 | RQ | Con una sesión degradada, `RequestUpdate` responde `Unavailable` y cero `TryRequest`. Si se degrada después de aceptar y antes del callback, el billete caduca. En OM, la degradación cierra el ámbito: **se provoca con un fallo dentro de `OnSourceEvent`** (`EBC:343-346`), no con `ReportFault`, para discriminar el punto único `Degrade` (CK C3-02) |
| N-15 | RQ y UI | La caducidad dentro de `RequestUpdate` de un billete **del mismo rack** da el valor de inicio `Failed(HostNotRunReason)`, no acepta la petición y no guarda nada. El gesto presenta antes lo pendiente y no pide |
| C3-03 | RQ | La caducidad dentro de `RequestUpdate` de un billete **de otro rack**: su `Failed` queda en el almacén bajo su rack, la petición del rack nuevo se acepta, y `TakeUpdateProgress` del otro rack devuelve ese `Failed` una vez |

**Además**, en RQ y en las clases de T4B extendidas:
- `TakeUpdateProgress` consume y va por clave;
- D-T4C-10;
- la frontera Update en el núcleo;
- I-01, M-01 (por guarda), M-03 y M-06.

**En UI**, en `UW/WorkspacePilotFieldViewModelTests.cs` y `UW/WorkspacePilotFieldViewTests.cs`, con el doble de UI extendido:
- el gesto y su orden: botón, frontera, petición y recogida;
- «Aplicando» con `InFlight`;
- cada clase de resultado;
- `IsMine`;
- la visibilidad del botón;
- el diagnóstico de T4A2, en `UW/WorkspacePilotReconciliationTests.cs`.

**Las guardas nuevas, todas con su autoprueba en negativo:**
- la pureza del corredor;
- el adaptador (D-T4C-04);
- G-1 a G-10;
- el censo de `Task` y `async` (D-T4C-02);
- el censo del setter;
- I-01, M-01 y «sin llamador de `EnterError`».

**Los cambios de guarda declarados** están todos en D-T4C-20.

**La contención** (F4-E59, `DSP:2082`, `:2086`; INV-F4T4B-18):
- en local, solo los filtros de Core, la suite completa de Core en Debug y los filtros de UI de `RequiredTests`;
- **nunca suites completas de UI en local;**
- ninguna sonda con entradas válidas en caminos con diálogo o escritura, tampoco las confirmaciones del panel;
- ninguna prueba escribe fuera de memoria ni usa la configuración real.

H0 no es una sonda del Worker: va por su dispensa (D-T4C-16).

**Anclas:** `OO`, «T4C AUTOMATED TESTS»; M2 §5.1; M4 §5.

### D-T4C-18. La compatibilidad con T1B

*Sin cambios en la v2.*

**Lo que T4C no toca:**
- los 14 archivos de MutantScope;
- las tres fachadas;
- `KindHandlers/`;
- `src/RackCad.UI/Systems/PushBack/**`;
- los 18 `*Commands*.cs`.

Todos van al prohibido (§6).

**Los archivos nuevos** no suscriben `InsertRequestedRaised` (CT-19) y no añaden `[CommandMethod(`.

**Ningún comportamiento caracterizado cambia** (M4 §2.3). Las pruebas de T1B, 55 de UI y 10 de Core, siguen en verde y **sin cambios**.

**Si algo lo exigiera, es STOP y se vuelve al Owner** (`OO`, «T1B COMPATIBILITY»; parada C03). Los tres caminos conocidos son:
- DV-01, el catálogo en las fachadas;
- DV-02, el comando registrado por Undo;
- poner código bajo `src/RackCad.UI/Systems/PushBack/`.

**Anclas:** H-32; M4 §1 a §3.

### D-T4C-19. La división de T4C

*Cambia en la v2: el estado intermedio (IR §2.8).*

**La propuesta** son tres pasos en serie:
1. **T4C-H0**, la sonda, sin producto (D-T4C-16).
2. **T4C1-HOST:**
   - Application: la operación, el ejecutor, el corredor, el billete y su cota, el almacén, el ámbito propio, R-T4-07 y la política de D-T4C-08;
   - Plugin: el adaptador, el escritor D-1 y el cableado;
   - I-01, M-01, M-03 en el tipo, M-06, D-T4C-14 y D-T4C-20;
   - el doble de UI, con las dos operaciones nuevas inertes.
3. **T4C2-PANEL:**
   - el botón, la frontera, la presentación y M-03 en la UI;
   - el diagnóstico de T4A2, con su parte de núcleo.

**Por qué:**
- **H0 es el «primer paso»** de R-T4-05, y sus resultados condicionan D-T4C-02 a D-T4C-04 y D-T4C-09;
- **la división sigue las capas y el riesgo:** el contexto de comando y la mutación propia son el riesgo mayor y llevan las 20 pruebas; la UI tiene su propia contención (F4-E59);
- **el precedente:** T4B dejó su método sin llamador de producto.

**El estado intermedio es seguro** (IR §2.8). Tras T4C1, `RequestUpdate` no tiene gesto, así que nunca se abre un ámbito ni se pone un billete, y D-T4C-10 no es alcanzable.

**En contra:**
- dos contratos y dos revisiones;
- Smoke-2 espera a las dos.

**La alternativa** es H0 más una única tarea de producto, de unas 40 rutas. Es una decisión del Owner (§9, O-09). El §6 marca cada ruta con la parte a la que va, de modo que vale para las dos formas.

### D-T4C-20. El inventario completo de los cambios de guarda

*Nueva en la v2: IR R-07. Cambia en la v3: RR N-02 a N-06, N-08 y N-09. Cambia en la v4: CK C3-01 (filas 14 y 15) y C3-02 (fila 17). Cambia en la v5: la cota por comando ajeno (filas 5, 14, 15 y 17), la guarda del adaptador para `ExecutionResult` (fila 19) y la fila nueva 22.*

**Cambian**, cada uno exacto y con su autoprueba en negativo:

| # | Guarda | Cambio | Negativa | Decisión | Parte |
|---|---|---|---|---|---|
| 1 | `TW/WorkspaceNavigationHostTests.cs:436` | De «vacío» a «exactamente `[ADAPTER]`», con una aparición | La API inyectada en `WorkspaceNavigationHost.cs`; una segunda llamada en `ADAPTER` | D-T4C-04 | [1] |
| 2 | `TW/WorkspaceEditIntentHostTests.cs:1012` | De «ningún método con Update» a «exactamente `RequestUpdate` y `TakeUpdateProgress`» | Un tercer método con «Update» | D-T4C-02 | [1] |
| 3 | `TW/WorkspaceEditIntentHostTests.cs:1023-1043` | Exactamente un campo de `Systems`: `_updateHost`, de tipo `IPushBackPilotUpdateHost` | Un segundo campo de `Systems`; un campo `IPushBackPilotUpdatePort` | D-T4C-06 | [1] |
| 4 | `TW/WorkspaceEditIntentHostTests.cs:982`, el censo de portadores | Ganan `EditUpdateStart` y `EditUpdateProgress` | Un portador sin censar | D-T4C-02 | [1] |
| 5 | `TW/WorkspacePilotEditGuardTests.cs:44-61`, `:64`, `:248-253`, `:256-260` y `:262-268` | `Operations` gana las dos al final. `RequestUpdate` entra en `MutatingOperations` y en `UpdateOperations`, nueva, la única que puede consultar el reposo y el ejecutor fuera de las lecturas. `TakeUpdateProgress` entra en `MemoryOperations`. `ExpireStaleUpdate`, la ayuda que consulta `_bridge.ForeignCommandStartsOf`, tiene su propia regla: solo nombra ese miembro del puente, y nunca `CloseOwnMutation`, `OpenOwnMutation`, el lector, el sondeo ni `PassGate` (RR N-04). Las cinco operaciones de D-T4C-08 y los dos descartes comprueban el vuelo después de `Authorize` y de `_pilotReadInProgress` | `RequestUpdate` sin `Authorize`; `TakeUpdateProgress` que autoriza; la ayuda que nombra `_probe` o `CloseOwnMutation` | D-T4C-02, D-T4C-08 | [1] |
| 6 | `GateViolations` (`TW/WorkspacePilotEditGuardTests.cs:427-455`) | La compuerta responde `Waiting` a `RequestUpdate` dentro de un aviso y solo reenvía `TakeUpdateProgress` | `RequestUpdate` reenviada dentro de un aviso | D-T4C-02 | [1] |
| 7 | `ControllerViolations` (`:509-519`) | El reenvío literal de las 17 operaciones | Un reenvío que hace algo más | D-T4C-02 | [1] |
| 8 | `PortMutatingOperations` (`TW/WorkspaceLifecycleGuardTests.cs:463-477`, `:540`) | **No cambia** (IR R-07 (a)). Una aserción nueva, aparte: el cuerpo de `RequestUpdate` no coincide con `ChangesADraft` y no nombra `TryUpdatePilot` | `.TryBeginApply(` inyectado en `RequestUpdate` | D-T4C-02 | [1] |
| 9 | El censo del setter, a la manera de `TW/WorkspaceEditIntentGuardTests.cs:490-505` | Nuevo, para `SetPilotUpdateHost`: una llamada en el getter, después del lector, y solo `CORE` y `CTL` | Una llamada en el constructor | D-T4C-06 | [1] |
| 10 | `TW/WorkspaceEditIntentGuardTests.cs:598-641` | «Nada desde el drenaje ni el Idle», con `RequestUpdate` y `TryUpdatePilot` en los cuerpos del controlador y del host de navegación | La llamada inyectada en `OnDrained` del controlador | D-T4C-02 | [1] |
| 11 | `TW/WorkspaceEditIntentGuardTests.cs:1101-1111` | Los textos de I-01, extendidos a los cuerpos nuevos | D-T4C-13 | D-T4C-13 | [1] |
| 12 | `TW/WorkspaceEditIntentGuardTests.cs:682-685`, `:821-870` (M-2 y M-3) y **`:699-700`** (RR N-05) | La expresión del diagnóstico, una vez más, en el cuerpo de la comprobación. En `:699-700`, `CheckBody()` sale del bucle de cero y pasa a exactamente uno; `IntentBody()`, `KeepBody()` y `DiscardAndReloadBody()` siguen en cero | Una tercera aparición en el núcleo; una segunda en `CheckBody()`; una en `KeepBody()` | D-T4C-12 | [2] |
| 13 | `TW/WorkspacePilotUpdateGuardTests.cs:185-188` | El puerto admite exactamente sus dos campos, el constructor, y las líneas de `Begin` y `Retire` | `:267-276` sin cambios, más las cuatro de D-T4C-07 | D-T4C-07 | [1] |
| 14 | `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:206-224` | La superficie de `EBC` gana cuatro nombres: `OpenOwnMutation`, `CloseOwnMutation`, `NoteForeignCommandStart` y `ForeignCommandStartsOf`. Uno más solo si Smoke-2 pidiera V2, V3 o el cierre en `Settle` | Un nombre más sin declarar | D-T4C-02, D-T4C-09 | [1] |
| 15 | `TW/WorkspaceLifecycleGuardTests.cs:113`, `:124-139` y `:167` | La regla de olvido pasa de 13 a **16** entradas: `OpenOwnMutation`, `CloseOwnMutation` y `NoteForeignCommandStart`. Se corrige además el nombre de `:295` (C2-M-5) | Una entrada nueva sin olvido, o con el olvido después de una rama | D-T4C-09, D-T4C-13 | [1] |
| 16 | La regla nueva del drenaje (OM) | Lo propio da solo la invalidación, y la marca sale solo de `DatabaseObjectChanged` o `CommandEnded` ajenos | Una supresión de lo ajeno; un marcado por `ImpliedSelectionChanged` | D-T4C-09 | [1] |
| 17 | La regla nueva de `SetQuiescent`, de `Degrade` y de `NoteForeignCommandStart` (OM) | El cierre, antes de la comparación de `SetQuiescent`, que ya no suma nada; el cierre en `Degrade`, el punto único de la degradación (RR N-09; CK C3-02); la cuenta de comandos ajenos en `NoteForeignCommandStart`, que no hace nada con una sesión degradada; y `ForeignCommandStartsOf` nulo para una sesión degradada | El cierre después de la comparación; una degradación por `OnSourceEvent` que deja el ámbito abierto; una entrada en reposo que suma a la cota | D-T4C-02, D-T4C-09 | [1] |
| 18 | `TW/WorkspacePilotSurfaceGuardTests.cs:54-92` | La presentación del campo puede llamar a `TakeUpdateProgress` y a ninguna lectura | Una lectura en la presentación | D-T4C-03 | [2] |
| 19 | Las nuevas de IG | La pureza del corredor, el adaptador, G-1 a G-10 (G-3 limitada a los tipos de larga vida, RR N-03; G-6 con `WorkspacePanelCommand.cs`, RR N-02), el censo de `Task` y `async`, I-01, M-01 y `EnterError` | Una por regla, con las de G-3 y G-6 ajustadas a sus correcciones | D-T4C-02, D-T4C-04, D-T4C-13, D-T4C-14 | [1] |
| 20 | Los dobles: `TW/WorkspaceCommandAdvisorTests.cs:17`, `:1107`; `UW/WorkspacePilotEditPortDouble.cs:25`; `TW/WorkspaceHostTestSupport.cs:53` | Implementan los miembros nuevos | — | D-T4C-02, D-T4C-09 | [1] |
| 21 | Las reglas de la reconciliación (`TW/WorkspacePilotReconciliationGuardTests.cs:62-101`, `:155-195` y desde `:256`) y `IntentionReads` (`TW/WorkspacePilotCloseSyncGuardTests.cs:844-867`) | El gesto nuevo, con su manejador y su botón exactos; `RequestUpdate` fuera del VM del navegador | Un segundo llamador | D-T4C-11 | [2] |
| 22 | La guarda nueva del manejador de `CommandWillStart` (IG), nueva en la v5 | `NoteForeignCommandStart` una vez, en el bloque de `CommandWillStart`, dentro de `ContainSession`, condicionada al nombre del comando y después de la decisión de `RaiseCommandStateChanged`. **Las guardas existentes del manejador no cambian y siguen satisfechas:** `TW/WorkspaceBridgeSourceGuardTests.cs:182-189`, `TW/WorkspaceIdleSchedulingGuardTests.cs:64-90` y `TW/WorkspaceHostC2SourceGuardTests.cs:62-90` | La llamada sin la condición; la llamada en otro bloque; una lectura del DWG en el manejador | D-T4C-02, D-T4C-09 | [1] |

**No cambian.** Su protección depende de dónde viven (RR N-06).

**(a) En archivos que T4C no toca: van al prohibido** (§6), por su archivo exacto:
- **los literales de caso de reposo**, en `TW/WorkspaceHostC2SourceGuardTests.cs:202-210`: el cierre va en el cuerpo de `SetQuiescent` (H-17);
- **los nombres de tipo**, en `TW/WorkspaceModelBoundaryTests.cs:84-104` y `TW/WorkspaceBrowserBoundaryTests.cs:287-295`;
- **la sesión sin handles**, en `TW/WorkspaceNavigationEchoTests.cs:325-340`;
- **el drenaje de F1**, en `TW/WorkspaceHintDrainTests.cs:93-112`, y **lo ajeno**, en `TW/WorkspaceBridgeHintAndDrainTests.cs:106-117`;
- **los manejadores**, en `TW/WorkspaceIdleSchedulingGuardTests.cs:64-104`, y **la frontera del puente**, en `TW/WorkspaceBridgeSourceGuardTests.cs:361-366`;
- **la asincronía de las carpetas**, en `TW/WorkspaceHostSourceGuardTests.cs:423-443`, `TW/WorkspaceSourceGuardExceptionTests.cs:262-271` y `TW/WorkspacePaletteHostSourceGuardTests.cs:554-581`;
- **el núcleo y el controlador sin `Task`**, en `TW/WorkspacePilotPrepareGuardTests.cs:407-430`;
- **los VM sin mecanismos**, en `TW/WorkspaceHostHardeningGuardTests.cs:758-779`;
- **los nueve archivos** de `Plugin/Workspace`, en `TW/WorkspacePaletteHostSourceGuardTests.cs:774-782`.

**(b) En archivos del permitido, porque cambian por otras filas:** no pueden ir al prohibido, porque entonces la tarea no podría hacer esas filas. La v2 los ponía en los dos sitios (RR N-06). Son:
- `TW/WorkspaceLifecycleGuardTests.cs`, con los literales de `:176-177` y la regla sin escrituras de `:679-694` (cambia por las filas 8 y 15);
- `TW/WorkspaceBridgeFacadeSourceGuardTests.cs`, con la superficie pública de `AutoCadEventBridge` de `:91-104` (cambia por la fila 14);
- `TW/WorkspacePilotUpdateGuardTests.cs`, con la carpeta del piloto sin «Transaction» de `:117-124` (cambia por la fila 13).

**En esos archivos, la protección es por línea:** la entrega lista sus cambios de guarda (§6, `ExpectedEvidence`), la revisión los compara con este inventario, y todo lo que no esté en él es la parada C07.

**Cualquier otra guarda que tuviera que cambiar es la parada C07.**

---

## 5. Trazabilidad

| Autoridad | Dónde |
|---|---|
| `OO`, «AUTHORITATIVE MUTATION LOGIC» | D-T4C-01, D-T4C-14 |
| `OO`, «EXECUTE IN COMMAND CONTEXT» / R-T4-05 / C-F4-02 | D-T4C-04, D-T4C-16, §9 O-05 |
| R-T4-05, «el primer paso de T4C es medir en host» | D-T4C-16: H0 hecho; §2.6; §9 O-01, O-16 y O-17 |
| `OO`, «DOCUMENT LOCK» / R-T4-06 | D-T4C-04, D-T4C-08 |
| `OO`, «FINAL REVALIDATION» | D-T4C-08 |
| `OO`, «RUN-SCOPED WRITER» | D-T4C-04 (el objetivo), D-T4C-06, D-T4C-07 |
| `OO`, «ONE ACTUALIZAR» | D-T4C-02, D-T4C-11 |
| `OO`, «COMMIT FAILURE» y «POST» | D-T4C-04 (el cerrojo), D-T4C-05 |
| `OO`, «OWN-MUTATION» / R-T4-08 / C-F4-04 / IC-08 / INV-08 / INV-28 | D-T4C-09, IC-09, §9 O-02 |
| R-T4-06, «un segundo Actualizar recibe `AnotherUpdateInProgress`» | D-T4C-02 |
| R-T4-07 | D-T4C-10, con la cota de D-T4C-02 |
| R-T4-09 / INV-31 | D-T4C-03, D-T4C-11 |
| `OO`, «UNDO» / R-T4-10 / D-13 / INV-32 | D-T4C-15, D-T4C-16 |
| `OO`, «T4A2 DIAGNOSTIC» / `T4BP` §9 I-3 / `A2P:660` | D-T4C-12 |
| `OO`, «T4B I-01» y «M-01 / M-03 / M-06» | D-T4C-13 |
| `OO`, «NO SECOND WRITE PATH» | D-T4C-14 |
| `OO`, «T4C AUTOMATED TESTS» (1 a 20) | D-T4C-17 |
| `OO`, «T1B COMPATIBILITY» | D-T4C-18 |
| R-T4-11 | D-T4C-01: T4C no aplica ni borra nada por sí mismo |
| R-T4-13 (C2-M-3 a C2-M-7) | D-T4C-13: C2-M-5 se cierra en el archivo que se toca, y las demás se difieren con su motivo |
| F4-E59 | D-T4C-17, INV-F4T4C-18, y la dispensa de H0 |

---

## 6. Contrato propuesto de F4-T4C (a fijar en la emisión)

**Identidad:**
- **El producto:** `F4-T4C-HOST-UNDO`, I-64, F4. Si se divide (D-T4C-19), son `F4-T4C1-HOST` y `F4-T4C2-PANEL`.
- **El presupuesto:** 1 de 3 por tarea.
- **Las celdas:** las de F4-T4B, con nc4 en la primera delegación y nc1 a nc3.
- **La sonda nc4:** la copia que amplía `AllowedWriteScope` con `src/RackCad.UI/Systems/PushBack/`. La de T4B, `src/RackCad.UI/`, ya no discrimina, porque parte de la UI pasa a estar permitida (M4 §3.3).
- **T4C-H0** no era una tarea de contrato del Worker, y **ya está hecha** (D-T4C-16; custodia `44fe029d`).

**`AllowedWriteScope`**, por ruta exacta. Las marcas indican la parte a la que va cada ruta: [1] es T4C1, [2] es T4C2, y [1, 2] va a las dos.

**Application:**
- Nuevo: `HOSTRUN` [1].
- Modificados:
  - `RUN` [1], solo I-01 y M-01;
  - `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` [1, 2];
  - `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` [1, 2];
  - `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs` [1];
  - `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs` [1];
  - `src/RackCad.Application/Workspace/IWorkspaceBridgeLink.cs` [1];
  - `src/RackCad.Application/Workspace/EventBridgeCore.cs` [1];
  - `src/RackCad.Application/Workspace/WorkspaceSession.cs` [1].

**Plugin:**
- Nuevo: `ADAPTER` [1].
- Modificados:
  - `WRITER` [1], solo D-T4C-07;
  - `src/RackCad.Plugin/Workspace/WorkspaceController.cs` [1];
  - `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs` [1], solo los tres reenviadores `internal` y la sentencia del comando ajeno en el manejador de `CommandWillStart` (D-T4C-09).

**UI:** `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`, `WorkspacePanelView.xaml` y `WorkspacePanelView.xaml.cs` [2]. **`BVM` sale** (IR M-07; D-T4C-10).

**Pruebas de Core, nuevas:**
- `TW/WorkspacePilotUpdateHostRunTests.cs` [1];
- `TW/WorkspacePilotUpdateRequestTests.cs` [1];
- `TW/WorkspaceOwnMutationTests.cs` [1];
- `TW/WorkspacePilotUpdateIntegrationGuardTests.cs` [1];
- `TW/WorkspacePilotUpdateHostDouble.cs` [1].

**Pruebas de Core, modificadas:**
- `TW/WorkspaceNavigationHostTests.cs` [1];
- `TW/WorkspaceEditIntentHostTests.cs` [1];
- `TW/WorkspacePilotEditGuardTests.cs` [1];
- `TW/WorkspaceEditIntentGuardTests.cs` [1, 2];
- `TW/WorkspaceLifecycleGuardTests.cs` [1];
- `TW/WorkspacePilotUpdateGuardTests.cs` [1];
- `TW/WorkspacePilotUpdateHostTests.cs` [1];
- `TW/WorkspacePilotUpdateRunTests.cs` [1];
- `TW/WorkspaceBridgeFacadeSourceGuardTests.cs` [1];
- `TW/WorkspaceCommandAdvisorTests.cs` [1], por los dos dobles del puerto;
- `TW/WorkspaceHostTestSupport.cs` [1];
- `TW/WorkspacePilotReconciliationGuardTests.cs` [2];
- `TW/WorkspacePilotSurfaceGuardTests.cs` [2];
- `TW/WorkspacePilotCloseSyncGuardTests.cs` [2].

`TW/WorkspaceBridgeHintAndDrainTests.cs` sale del alcance permitido. Lo propio va en el archivo nuevo OM, y lo ajeno no cambia.

**Pruebas de UI:**
- `UW/WorkspacePilotEditPortDouble.cs` [1, 2];
- `UW/WorkspacePilotFieldViewModelTests.cs` [2];
- `UW/WorkspacePilotFieldViewTests.cs` [2];
- `UW/WorkspacePilotReconciliationTests.cs` [2].

**`ForbiddenWriteScope`.** La plantilla son las 70 entradas de T4B (M4 §3.1). Cambian cinco, siguen 65 y se añaden las de abajo (IR M-06).

**Las cinco que cambian:**
- **F01, `src/RackCad.UI/`,** se parte en:
  - `src/RackCad.UI/Systems/`, `Controls/`, `Shell/`, `Themes/`, `Editor/`, `Preview/`, `Views/`, `RackFrames/` y `StructuralSections/`;
  - la clausura de T1B, de G1 a G6, por entrada explícita;
  - **los cinco archivos de `src/RackCad.UI/Workspace/` que no están permitidos:** `WorkspaceBrowserViewModel.cs`, `WorkspaceMeasurementRow.cs`, `WorkspaceNavigationViewModel.cs`, `WorkspacePanelState.cs` y `WorkspacePanelViewModel.cs`;
  - el resto de `src/RackCad.UI/` fuera de lo permitido.
- **F03, `src/RackCad.Plugin/Workspace/`,** se parte en sus siete archivos no permitidos: `WorkspaceHostPort.cs`, `WorkspaceNavigationEffects.cs`, `WorkspaceNavigationFacts.cs`, `WorkspaceNavigationHost.cs`, `WorkspacePaletteHost.cs`, `WorkspacePanelCommand.cs` y `WorkspaceSelectionReader.cs`.
- **F37 y F38** pasan al permitido.
- **F41, `tests/RackCad.UI.Tests/`,** se parte:
  - las seis clases de UI de T1B;
  - `StaTestRunner.cs`, `EditorWindowTestSupport.cs` y `EditorDiscardPromptCollection.cs`;
  - el `.csproj`;
  - el resto fuera de lo permitido.

**Siguen las otras 65,** con **F28, `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`,** prohibida de forma expresa. T4C no la necesita: la corrida lee dentro de su callback, por `ReadForPrepare` (D-T4C-01), y la orden dice «Do not migrate unrelated Workspace reads/navigation into command context».

**Se añaden:**
- lo que T4B tenía permitido y T4C no necesita:
  - `src/RackCad.Plugin/Views/RackSiblingScan.cs`;
  - `src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotUpdatePort.cs`, `PushBackPilotSystemRebuild.cs` y `PushBackPilotUpdatePlan.cs`;
  - `src/RackCad.Application/Workspace/Drafts/DraftSession.cs`, `DraftApplyOutcome.cs` y `DraftRegistry.cs`;
  - `TW/WorkspacePilotUpdateScopeDouble.cs`;
- `TW/WorkspaceDraftRegistryContainmentTests.cs`;
- `src/RackCad.Plugin/Views/RackViewBatchExecution.cs` y `RackViewBatchProductSession.cs`, el hueco de M4 §3.4;
- **las guardas que no cambian y viven en archivos que T4C no toca**, del apartado (a) de D-T4C-20, cada una por su archivo exacto.

**El prohibido no contiene ningún archivo del permitido** (RR N-06). Las guardas que no cambian dentro de archivos permitidos las protege la comparación de la entrega con D-T4C-20, y su violación es C07.

**El control real sigue siendo la lista blanca exacta.** Por PD-F4-07, la revisión compara las dos listas y no se fía de un Scope en verde.

**`RequiredTests`**, con las cuentas de `4314c180` (H-31):

| Proyecto | Filtro | `MinSelected` | `ExpectRed` |
|---|---|---|---|
| Core | `FullyQualifiedName~RackCad.Tests.Workspace`, por prefijo del nombre de clase. Las clases nuevas empiezan por `Workspace` | **3103** (el suelo; la entrega declara las nuevas) | true |
| Core, una fila por clase (`FullyQualifiedName~RackCad.Tests.<Clase>`) | Las 21 de T4B (M4 §4.3), con `CallerOwnedFacadeGuardTests` en **81**, más `SelectiveEditorOpenTests` (33) y `CustomPropertiesCommandGuardTests` (48), por el censo de comandos (DV-02) | La de cada una | false |
| UI | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 601 | false en [1]; true en [2] |
| UI | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 | false |

**Las cuentas de la fila por clase**, de M4 §4.3 y §4.4:

| Clase | Cuenta | Clase | Cuenta |
|---|---|---|---|
| `CallerOwnedFacadeGuardTests` | 81 | `CallerOwnedTransactionGuardTests` | 15 |
| `RackSiblingRedrawUnitsTests` | 2 | `DebugFaultInjectionGuardTests` | 1 |
| `SiblingRedrawSeamGuardTests` | 2 | `RackSiblingMembershipTests` | 12 |
| `RackSiblingRedrawPlanTests` | 2 | `RackSiblingRedrawRunTests` | 6 |
| `PushBackClassicCharacterizationPluginGuardTests` | 10 | `PushBackPluginSourceGuardTests` | 11 |
| `PushBackRoundTripSourceGuardTests` | 24 | `RackProjectionCommandGuardTests` | 34 |
| `NamespaceFolderGuardTests` | 7 | `CustomPropertiesEnvelopeGuardTests` | 22 |
| `RackUnitsGuardSourceTests` | 18 | `I55G15Auth15ConsumerTests` | 5 |
| `SingleScanInsertGuardTests` | 5 | `RackViewBatchDriverGuardTests` | 4 |
| `G16FirstViewCorrectiveTests` | 61 | `RackSiblingCustomPropertiesGateTests` | 2 |
| `RackEnvelopeIdProbeTests` | 5 | `SelectiveEditorOpenTests` | 33 |
| `CustomPropertiesCommandGuardTests` | 48 | | |

Cada nombre se confirma con `git grep "class <Clase>"` en la emisión.

**Restricciones vigentes que la emisión cita al Worker** (IR M-16):
- `HOSTRUN` no contiene la palabra «Transaction», ni en un comentario (H-06);
- ningún tipo nuevo de `RackCad.Application.Workspace`, anidados incluidos, se llama con `Subscription`, `Editor`, `Draft`, `Tab`, `Metric`, `Provider` ni los demás de H-20;
- el cuerpo de `SessionState` no contiene `Close`, `Allowed` ni `SessionId?` (H-17).

**Invariantes INV-F4T4C-NN:**
- **01. T4B es la única autoridad de la mutación.** `TryUpdatePilot` se llama solo desde la corrida del núcleo, una vez por corrida admitida. T4C no decide Stale, Conflict, los valores, la secuencia ni POST.
- **02. Un Actualizar explícito da como mucho una corrida de T4B.**
  - El billete en vuelo, por sesión y con su testigo, va desde la aceptación hasta toda salida.
  - Se libera con la compleción del `ExecutionResult`. Como respaldo, caduca con el primer `CommandWillStart` de la sesión de un comando que no sea `EXECUTEFUNCTION`, con el cierre del documento o con la retirada de la sesión, si el callback no empezó o no terminó; y si la sesión se degrada antes del callback. **Las entradas en reposo no cuentan** (la decisión del Owner sobre C12).
  - Un billete empezado tiene garantizada exactamente una llamada terminal (RR N-01).
  - Un segundo pedido da `Busy`.
  - Ningún Idle, drenaje, manejador ni pista propia pide otro.
- **03. Ningún `Task`, `async` ni `await` nuevo fuera de `ADAPTER`.** La navegación conserva sus excepciones exactas, con el censo por archivo de H-09. No hay `Dispatcher`, `Timer`, `.Result`, `.Wait(` ni `GetAwaiter` en ninguna parte nueva. El adaptador solo tiene una `OnCompleted` sobre el `ExecutionResult`, con el salto al contexto capturado, y su `Task` es el `Task.CompletedTask` que devuelve el callback (IR R-11; H-50).
- **04. `ExecuteInCommandContextAsync`** aparece una vez y solo en `ADAPTER`. La navegación sigue en contexto de aplicación (IC-01).
- **05. Un bloqueo** sobre el objetivo resuelto por el id admitido, en el orden objetivo → `Admit` → bloqueo → ámbito propio → escritor → `Execute` → `Retire` → desbloqueo. El documento bloqueado es el que recibe el escritor.
- **06. El escritor** nace tras la admisión y dentro del bloqueo, una vez por corrida. Recibe solo ese documento, no vive en ningún campo, es de un solo uso, se retira en toda salida, y su ámbito sigue comparando el id.
- **07. Un fallo del host antes de la mutación** da `Rejected` o `Failed`, por un solo canal: el valor de inicio o el almacén. Cero T4B, el borrador sin cambios, el billete quitado y el ámbito cerrado. Una vez empezado el callback, nada reclasifica, Unknown nunca se traduce, y ninguna excepción sale hacia AutoCAD.
- **08. El reposo se comprueba antes de entrar,** y dentro solo `Admit`, `UpdateGate` y T4B. Durante el vuelo, el borrador del rack no cambia (D-T4C-08). La relectura de MUTATE sigue siendo la autoridad.
- **09. El ámbito propio sigue IC-09:**
  - registra lo propio aparte, y lo propio solo invalida;
  - conserva lo ajeno pendiente;
  - se cierra con el primer fin o inicio de comando, la entrada en reposo, `NotRun` o la destrucción;
  - los manejadores solo encolan y la sesión no lleva handles;
  - el residual queda declarado.
- **10. R-T4-07:** `Busy` en los descartes, con la cota del billete y el registro sin cambios.
- **11. La UI:**
  - el botón solo en Dirty, y el gesto que comprueba su botón;
  - la frontera según H-27;
  - «Aplicando» solo con `InFlight` o Applying;
  - cada clase presentada una vez y sin reclasificar;
  - `IsMine`, K = 1, los avisos visibles y el VM sin mecanismos nuevos.
- **12. El diagnóstico de T4A2,** separado de la clasificación. Su fallo da `null`, nunca cambia el estado, y T4C no añade ninguna lectura.
- **13. Los arrastres,** según D-T4C-13. Ningún llamador de producto de `DraftSession.EnterError`, y el bloque diferido sin tocar.
- **14. Sin segunda vía de escritura:** G-1 a G-7, G-9 y G-10.
- **15. Undo:** ninguna API de Undo, ningún `SendStringToExecute`, `[CommandMethod(` ni `CommandFlags` nuevos, y ninguna afirmación antes de Smoke-2.
- **16. T1B:** `git diff --name-only BaseSha..CurrentSha` no toca la clausura, las fachadas, `KindHandlers/` ni los `*Commands*.cs`, y las pruebas de T1B siguen en verde y sin cambios.
- **17. Solo los cambios de guarda de D-T4C-20,** exactos y con su autoprueba en negativo.
- **18. La contención de F4-E59** (D-T4C-17).
- **19. Ningún `DBObject`, `Transaction` ni `Document` cruza la interfaz de Application, y ninguna firma de Application nombra un tipo de AutoCAD** (IR R-04; RR N-11). El corredor solo ve `IPushBackPilotHostTarget`, una interfaz opaca que implementa `ADAPTER`. El `DocumentLock` lo recibe como `IDisposable` y solo lo libera. `Document` solo aparece en `Plugin/Views`.
- **20. El RED** lleva cuerpos inertes, también en las fábricas, los reenvíos, el objetivo y las primitivas. Las pruebas existentes que cambian van en el RED, y ninguna prueba necesita AutoCAD.

**`ExpectedEvidence`:**
- lo de T4B: el contrato, la delegación, el RED y el GREEN con su push, la CI exacta y la entrega;
- el build Release de la solución, sin avisos;
- el build Debug del Plugin, con salida temporal;
- el mapa evidencia → invariante;
- qué suites corrieron en local (INV-F4T4C-18);
- en la entrega, la lista de los cambios de guarda aplicados frente a los de D-T4C-20.

**Paradas:**
- **C01:** haría falta decidir Stale, Conflict, los valores, la secuencia o POST fuera de T4B, o duplicar su política.
- **C02:** haría falta más de una transacción authored, una API de Undo o un comando registrado. Es una A-n de D-13.
- **C03:** haría falta cambiar un comportamiento caracterizado por T1B, o tocar la clausura, las fachadas, `KindHandlers/` o los `*Commands*.cs`.
- **C04:** `ExecuteInCommandContextAsync` fuera del archivo exacto; un `Task` o un `async` nuevos fuera de `ADAPTER`; o una espera bloqueante, un hilo o un planificador distinto del capturado en el adaptador.
- **C05:** el bloqueo o el contexto no pueden envolver la corrida. Por ejemplo:
  - una `TopTransaction` abierta al entrar;
  - un callback sin fin de comando, sin inicio de comando y sin entrada en reposo posteriores, de modo que el ámbito no se cierra;
  - la falta de un contexto de sincronización;
  - otro documento activo dentro del callback.

  H0 no encontró ninguno de estos casos (filas 5, 8 y 10). Quedan como paradas por si la tarea o Smoke-2 los encuentran.
- **C06:** un escritor, una corrida, un objetivo o un estado retenidos fuera de lo declarado.
- **C07:** un cambio de guarda fuera de D-T4C-20.
- **C08:** una segunda vía de escritura.
- **C09:** el host traduce Unknown, o reclasifica un desenlace ya decidido.
- **C10:** el ámbito propio necesitaría suprimir lo ajeno o la invalidación, o llevar handles, sin una decisión del Owner.
- **C11:** un llamador de producto de `DraftSession.EnterError`, o un cambio de `DS:555-559`.
- **C12:** un hecho medido, o las precondiciones de emisión, contradicen el paquete.
  - **H0 dio C12 en una sola fila, la 3:** una entrada en reposo precede al callback. **Está resuelto** por la decisión del Owner, «Completar o comando ajeno» (D-T4C-02).
  - **H0 no activó** ninguno de los demás casos (filas 4 y 5):
    - los `*Cancel*` que encolan una pista antes de la ventana (IR R-06);
    - un cierre temprano sistemático del ámbito (RR N-07);
    - la falta a la vez del fin del comando interno y de su entrada en reposo (RR N-10).
  - **Siguen siendo paradas** si la tarea o Smoke-2 los encuentran. Cada caso vuelve a O-02 con su respuesta declarada o con una A-n: V1 o V2 para los `*Cancel*`; V3 para el cierre temprano sistemático; y el cierre en `Settle`, como desviación expresa de C-F4-04, para la falta de los dos eventos (CK C3-01).
  - **Es C12 también** cualquier comportamiento de la compleción o de la cota que contradiga D-T4C-02. Por ejemplo, un `CommandWillStart` ajeno sistemático antes del callback, que caducaría cada Actualizar.
- **C13:** se viola la contención de F4-E59.
- **C14:** hace falta cambiar algo fuera de las líneas declaradas (IR M-13):
  - de `RUN`, más allá de I-01 y M-01;
  - de `WRITER`, más allá de D-T4C-07;
  - de `TryUpdatePilot` o `UpdateGate`, más allá de I-01, M-01 y M-03.

**Precondiciones de emisión:**
- T4C-H0 medida: **se cumple** (custodia `44fe029d`), con su C12 resuelto por el Owner;
- las revisiones incorporadas: **se cumple** hasta la v4 (§10.1 a §10.3), y la v5 incorpora H0 (§10.4);
- la disposición del §9 por el Owner: **se cumple** para O-01 a O-16. Faltan su ratificación de las desviaciones de H0 (O-17, que incluye la interpretación de M5-01), la observación O-18 y la orden de emisión (§9);
- **la custodia** de la decisión sobre el C12, de la respuesta de I-52 (`F4-DRAFTS/t4c/h0/i52-reply.md`, que supera el «no reply» de `h0-session.json` de `44fe029d`), de la comprobación de la v5 y de esta v5: va en el commit de documentación que acompaña a esta versión (CK5 M5-02).

---

## 7. Smoke-2

Empieza solo después de cerrar el producto de T4C (`OO`, «SMOKE-2»). Lleva:

**De la orden:**
- un rack Push Back real y un Actualizar real;
- el resultado authored correcto;
- un Undo lógico, su restauración y el Redo, probado inmediatamente después del Undo;
- el conflicto;
- «cancel», con la definición de O-13;
- la discordancia de documento;
- la contención de la mutación propia, con otro rack no enmascarado;
- CLOSE, CLOSEALL y QUIT, también con una actualización en vuelo;
- la temporización de IC-04;
- los casos de navegación con racks representativos.

**De T4B** (`T4BP:794-809`):
- los bytes y los handles;
- una transacción y `TopTransaction == null`;
- las tres lecturas;
- R-09;
- el Undo de la importación;
- S2-R-10 con `SEAL=before` y `SEAL=after`;
- la capa bloqueada;
- la purga;
- los llamadores A a D.

**De T4C:**
- un `U` inmediato después de Actualizar A marca la sesión;
- un RACKEDITAR de B después de Actualizar A marca B;
- B no queda marcado por el propio Actualizar (S2-14, `FRZ:894`);
- el vaciado de PICKFIRST;
- si «Aplicando» se ve;
- `Busy` al descartar durante el vuelo, y su fin con la compleción o con un comando ajeno (la cota de D-T4C-02);
- el paso de Undo vacío de un Actualizar sin escritura;
- el `Debug.Assert` del drenaje (`CTL:267-273`).

**De H0** (§2.6), para confirmar con el producto lo que se midió con el arnés:
- **Undo y Redo, con Redo inmediatamente después de Undo** (H-44). Entre los dos no se hace ninguna lectura del panel, porque «Comprobar ahora» y «Editar» vacían la pila de Rehacer (O-H0-2, H-51). Ese comportamiento de las lecturas del panel se declara en Smoke-2, y no es de T4C;
- **el paso vacío** (H-45): se observa y se declara;
- **un solo paso de Undo con el producto,** con su nombre en la lista («Executefunction» en H0);
- **el orden de los eventos y la ventana del ámbito propio** (H-43): B no se marca, y lo propio solo invalida. Es lo que O-H0-1 midió sin el ámbito;
- **la cota del billete con la regla del Owner:** un Actualizar normal se libera con la compleción; las dos entradas en reposo previas no lo caducan; y un comando ajeno tecleado durante el vuelo, si se llega a provocar, lo caduca sin escribir;
- **el diferido y «Aplicando»:** H0 midió uno o dos fotogramas de la UI entre la vuelta de la llamada y el callback (H-39);
- **las respuestas declaradas,** por si Smoke-2 encuentra lo que H0 no activó: V1 o V2 (los `*Cancel*`), V3 (un cierre temprano sistemático) y el cierre en `Settle` (la falta de los dos eventos), cada una con su C12 y su vuelta al Owner;
- **un dibujo real del Owner,** que H0 no usó (H-52), y **el Owner como operador**, según C-F4-06.

**Del resto:**
- S2-01 a S2-22;
- S2-R-01 a S2-R-12;
- la repetición de SN-06c y de la parte de pestañas de SN-10;
- los dibujos del Owner de C-F4-06.

El procedimiento se fija tras T4C (`T4P:587`). La build del hito tiene que resolver antes DV-05 (§9, O-08).

---

## 8. Lo que queda fuera de T4C

- **El catálogo:** su unificación (DV-01) y la carga fuera del bloqueo (M2 Q2-08). Se declaran.
- **`ClosedDetached` con clasificación:** inalcanzable desde la UI con D-T4C-10.
- **La restauración de la selección implícita** tras Actualizar.
- **La vía B del ámbito propio,** si el Owner acepta la vía A.
- **C2-M-3, C2-M-4, C2-M-6, C2-M-7, y C2-M-5 fuera de `TW/WorkspaceLifecycleGuardTests.cs`:** se difieren (D-T4C-13).
- **M-05, M-07 y M-09** de T4B: siguen en ideas futuras (`T4BC` §7).
- **H-1** (la suite de UI escribe en el perfil) y **PD-F4-07**: siguen como deudas.

---

## 9. Decisiones materiales para el Owner antes de emitir

**El estado en la v5:**
- **O-01 a O-15 están decididas.** El Owner aceptó todas las recomendaciones del §9 de la v4 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c-package-decisions.md`). Las filas conservan su texto y llevan el resultado de H0 donde cambia algo: O-01, O-02 y O-07.
- **O-16 está resuelta:** el C12 de H0, por la decisión del Owner «Completar o comando ajeno».
- **O-17 es lo único que queda para el Owner:** ratificar las desviaciones de H0.
- Después hace falta la orden de emisión de T4C1, y luego la de T4C2 (O-09).

| # | Hecho | Opciones | Recomendación de la sesión | Condición de «RETURN BEFORE EMISSION» que toca |
|---|---|---|---|---|
| **O-01** | Antes de H0: el momento del callback, la compleción de la llamada, los `*Cancel*` y el paso único de Undo no estaban medidos (H-22, H-23). R-T4-05 los pone como primer paso, la sesión no opera AutoCAD y Smoke-2 va después del producto | (a) **T4C-H0** (D-T4C-16), con **tres dispensas expresas:** Smoke-2 sigue siendo «the first physical validation», y H0 es un diagnóstico; §12.1, sin build de hito y **con el DLL Debug del worktree desechable en lugar del «worktree de I-64» que exige `FRZ:830`** (RR N-12); y F4-E59. Lleva un arnés revisado en un worktree desechable, con su custodia; datos de máquina medidos por la sesión; dibujo en copia, cerrado sin guardar y con el hash del original comprobado; y la tabla resultado → acción. (b) Instrumentación Debug dentro de T4C1, y la sonda sobre el adaptador real antes de T4C2. (c) Todo en Smoke-2 | **(a)**, con las tres dispensas y una ventana de host propia (RELEASE de I-52). Cualquier fila con C02, C05 o C12 vuelve antes de emitir. **Decidida:** «H0 con dispensas». **Hecha** el 2026-11-15 (D-T4C-16; custodia `44fe029d`): ninguna fila dio C02 ni C05, y la fila 3 dio C12 (O-16). Las desviaciones van a O-17 | «Undo requires a material architecture change» y «DocumentLock/command context cannot wrap the T4B run cleanly»: **no activadas, ahora por la medida** (filas 2, 5, 6 y 8 a 10). «any other material product decision»: decidida |
| **O-02** | Las pistas no llevan rack (H-24), y `OO` exige «unable to mask another rack» | (A) La vía A con IC-09: lo propio aparte; el cierre con cota estructural (el primer fin o inicio de comando, o la entrada en reposo); y el residual de una escritura ajena durante la ventana (D-T4C-09). Las respuestas preparadas para la parada C12, según lo que mida H0: **con los `*Cancel*` previos,** V2 (antesala solo para `CommandEnded`), V1 (abrir en la aceptación) o una A-n; **con un cierre temprano sistemático** (RR N-07), por un comando anidado o por un `CommandWillStart` posterior a `Begin`, que marcaría B en cada Actualizar, V3 (dos fases: mientras dura el callback **ningún** evento cierra la ventana, y `Settle` restablece las reglas normales) o una A-n; **sin fin de comando ni entrada en reposo** del comando interno (RR N-10), que V3 no corrige, **el cierre en `Settle`**, que es una **desviación expresa de C-F4-04** (`F4P:60`, «se cierra con el fin del propio comando, no tras POST»), o una A-n (CK C3-01). (B) La pista con handle: choca con `FRZ:217-218`, con IC-04 y con `TW/WorkspaceNavigationEchoTests.cs:325-340`, y vuelve a marcarlo todo por lo propio que no es miembro | **(A)**, con su residual aceptado de forma expresa y la IC-09. **V2** si H0 confirma los `*Cancel*`. **V3** si confirma un cierre temprano sistemático. Si confirma la falta sistemática de los dos eventos, **el cierre en `Settle`**, solo si el Owner admite la desviación de C-F4-04; si no, una A-n. Solo un caso aislado se declara como residual. **Decidida:** «Vía A + IC-09». **H0 no activó ninguna respuesta** (filas 4 y 5): V1, V2, V3 y el cierre en `Settle` no hacen falta, y quedan declarados para Smoke-2 (D-T4C-09) | «any other material product decision»: decidida |
| **O-03** | El núcleo, que vive todo el proceso, guarda un ejecutor sin estado (D-T4C-06). El escritor sigue siendo de una corrida | (E-a) Un campo de tipo `Systems`, con el cambio declarado de la guarda de campos. (E-b) La interfaz en `WorkspaceEditIntent.cs` y un reenviador en `CTL`, sin ese cambio. (W-3) Sin campo, rompiendo el reenvío literal y la compuerta | **(E-a).** La sesión no lo lee como «a new persistent host authority»: no guarda documento, escritor, transacción, corrida ni objetivo, y cumple «not retained by Workspace controller». Pero el Owner decide la lectura | **«the pilot-specific writer needs a new persistent host authority»** (posible, según la lectura) |
| **O-04** | El escritor de hoy lee el documento activo y se puede reutilizar (H-05). Su guarda prohíbe campos (H-06) | (D-1) Un constructor con el `Document`; dos campos, el documento de solo lectura y la marca de un solo uso; `Retire`; y un cambio declarado de `WRITER` y de `TW/WorkspacePilotUpdateGuardTests.cs:185-188`, protegido por C14. (D-1') Un solo campo `Document?` consumido. (D-3) Un puerto por corrida en el adaptador, con `WRITER` sin uso | **(D-1).** No es «reopen T4B»: cumple «receives only the exact current document» y «cannot be reused», que `OO` asigna a T4C, sin tocar MUTATE ni POST | «any other material product decision», por un cambio en un archivo y en una guarda de T4B |
| **O-05** | C-F4-02 manda **avisar** al Owner de que se reintroduce `ExecuteInCommandContextAsync` (`F4P:59`). Hoy está prohibida en todo `src` (H-09) | Avisar y admitir la excepción por un solo archivo exacto; o un comando registrado desde el principio (D-13, A-n) | **El aviso:** se reintroduce en un único archivo, `ADAPTER`, solo para Actualizar, con el planificador del hilo de la UI y sin ninguna espera. La navegación sigue según IC-01. Vuelve el vaciado de PICKFIRST (`*Cancel*` x2), solo en Actualizar | — (aviso obligatorio) |
| **O-06** | La UI no puede recibir nada por empuje (H-15) | F-1 extracción, F-2 empuje o F-3 resultado síncrono (D-T4C-03) | **F-1**, con `TakeUpdateProgress`, el aviso `StateChanged` existente y las cinco reglas de presentación única | Si H0 muestra que no llega ningún aviso después del comando interno: «DocumentLock/command context cannot wrap the T4B run cleanly» |
| **O-07** | Los descartes actúan hoy sobre un Applying (H-25). Un `Busy` sin cota haría el documento imposible de descartar y de cerrar sin perder el trabajo (IR R-01) | (a) La marca del host, `Busy`, **con la cota del billete**, con `Failed(HostNotRunReason)` y un testigo que anula el callback tardío. En la v4, la cota era la primera entrada en reposo; **en la v5 es la regla del Owner** (O-16). (b) El registro, que cambia una prueba caracterizada de T2B2B. (c) Las dos | **(a) con la cota.** Sin la cota, (a) no es seguro. **Decidida.** **Sigue siendo segura con la regla nueva:** el billete se libera con la compleción, que H0 vio siempre (H-40); y como respaldo, con el primer comando ajeno de la sesión, su cierre o su retirada. Un `RACKPANELDESCARTAR` o un `CLOSE` son ellos mismos comandos ajenos, así que el descarte y el cierre nunca quedan bloqueados más allá del comando con que el usuario los pide (D-T4C-10). Un billete empezado tiene garantizada su llamada terminal (RR N-01), y en una sesión degradada `RequestUpdate` responde `Unavailable` y no pone billete | — |
| **O-08** | DV-05 de M4: la guía (`docs/guias/validacion-manual-autocad.md:427-428`) y `FRZ:828-830` exigen la suite completa de UI en local para la build del hito, y F4-E59 la prohíbe mientras siga H-1 | Una dispensa (la CI exacta y los filtros locales); aceptar H-1 declarada; o resolver antes H-1 | Una dispensa expresa del Owner para Smoke-2, o resolver H-1 en una tarea aparte. **Es para antes de Smoke-2 y no bloquea la emisión** | — |
| **O-09** | El producto toca unas 40 rutas, en tres capas (§6) | H0 + T4C1 + T4C2; o H0 + una sola tarea | **H0 + T4C1 + T4C2** (D-T4C-19). El estado intermedio es seguro | — |
| **O-10** | El diagnóstico de T4A2 obliga a cambiar M-2 y M-3 y la regla de UI de D-T4A2-06 (D-T4C-12) | La opción (a) de M3 §4.3; o dejar la limitación declarada a F5 | **(a)**, porque `OO` lo asigna a T4C, y sin ninguna lectura nueva | — |
| **O-11** | M-03: dar la clasificación cambia la fábrica `ClosedInError` (D-T4C-13) | Ampliar `ClosedInError`; o solo un texto | **Ampliarla**, sin ningún llamador de `DraftSession.EnterError` y con `ClosedDetached` fuera | — |
| **O-12** | Una corrida modifica el DWG en varias transacciones, pero lo authored va en una (H-07) | Leer el renombrado y la purga como POST (`FRZ:519`); o como authored | **Como POST**: una transacción authored | «more than one authored mutation transaction is required» (no activada con esta lectura) |
| **O-13** | «cancel» de `OO` no está definido (M2 Q2-05) | (i) El comando interno cancelado antes del callback, también con Esc justo después del clic: fallo del host y cero escrituras. (ii) El «No» de una confirmación. (iii) Esc durante la corrida, que no tiene punto de cancelación | **(i)**, y (iii) declarada como inexistente | — |
| **O-14** | Los avisos no viajan en `PilotFieldView` (H-28) | Un aviso del gesto en el VM, que se pierde si la superficie cambia después de presentarlo; o ampliar la vista con un cambio declarado | **El aviso del gesto** | — |
| **O-15** | `EXECUTEFUNCTION` vacía PICKFIRST (H-22) | Aceptarlo; o restaurar la selección, que es una escritura de selección nueva | **Aceptarlo**, en Smoke-2. H0 lo confirmó (O-H0-3) | — |
| **O-16** | **El C12 de H0, fila 3** (H-41): AutoCAD entra en reposo dos veces entre la vuelta de la llamada y el callback, en las cinco sondas. Con la cota de la v4, cada Actualizar caducaría antes de correr | «Completar o comando ajeno»; «Contar desde el comando interno», en la que la caducidad por reposo solo cuenta después del `CommandWillStart` y del `CommandEnded` de `EXECUTEFUNCTION`; o «A-n: comando registrado» | **Resuelta por el Owner:** «Completar o comando ajeno» (`owner-f4t4c-h0-c12-decision.md`). El billete se libera con la compleción de `ExecutionResult`; como respaldo, si el callback no empezó, caduca con el primer `CommandWillStart` de la sesión de un comando que no sea `EXECUTEFUNCTION`, con el cierre del documento o con la retirada de la sesión; y las entradas en reposo dejan de contar. La v5 lo aplica en D-T4C-02, D-T4C-04, D-T4C-09, D-T4C-10, D-T4C-17 y D-T4C-20 | «any other material product decision»: **resuelta** |
| **O-17** | **Las desviaciones del procedimiento de H0** (H-52): **el operador fue la sesión, no el Owner**, por la instrucción del Owner («Crealo tú controlando autocad, no pidas autorización»), contra la cuarta cláusula de la dispensa 2; **el dibujo es sintético**, creado por la sesión con el producto clásico; y **el RELEASE de I-52** se pidió y I-52 respondió después de la sonda, «SIN OBJECIÓN» y sin emitir RELEASE, por su política de no conceder ventanas a otras iniciativas | Ratificarlas como están; o repetir H0, o alguna de sus filas, con el Owner como operador, un dibujo real y el RELEASE de I-52 | **Ratificarlas.** El resultado es de diagnóstico y no cuenta como cumplimiento; Smoke-2 lo repite todo con el producto, un dibujo real del Owner y el Owner como operador (§7). Ninguna fila depende del dibujo: el callback, la compleción, el orden, Undo y los bloqueos son del host. La sesión de I-52 estaba inactiva, sin ninguna `acad.exe`, y no hubo solapamiento | — (ratificación del procedimiento) |
| **O-17 bis** (CK5 M5-01) | **La interpretación de la regla del C12:** D-T4C-02 aplica el respaldo del comando ajeno también a un billete empezado que no entró en `Execute` y a uno terminado sin compleción. Es la defensa de N-01, que la decisión del Owner no nombra | Ratificarla como interpretación; o limitar el respaldo, literalmente, al billete cuyo callback no empezó | **Ratificarla.** No contradice la regla, solo cierra los dos caminos que la letra no cubre; sin ella, un billete empezado que no completa quedaría sin cota | — (ratificación de la interpretación) |
| **O-18** (CK5 M5-04) | **Observación, fuera del alcance de T4C:** las lecturas del panel de F4-T3 y F4-T4A («Editar» y «Comprobar ahora») dejan entradas «Group of commands» en Deshacer y vacían la pila de Rehacer (O-H0-2, H-51). Es un efecto visible de unas lecturas que se suponían sin efecto, y toca S2-05 y la obligación de IC-02 de observar Deshacer y Rehacer | Registrarla como candidata a ideas futuras o a una corrección aparte de T3 y T4A; o tratarla dentro de T4C | **Registrarla aparte,** sin cambiar el alcance de T4C. Smoke-2 la declara y prueba Redo inmediatamente después de Undo | — (observación) |

**Lo que IR §4 pedía llevar al Owner y el diseño de la v2 resuelve sin fila nueva:**
- **la política del borrador que cambia durante el vuelo** (IR R-03): `Busy` en las operaciones que cambian ese borrador (D-T4C-08). El usuario no puede editar lo que no se va a aplicar, y la UI ya deshabilitaba la caja;
- **la lectura de INV-19** (IR R-04): con el objetivo opaco, ningún `Document` cruza Application, así que INV-19 se cumple literalmente;
- **la superficie de `RUN` para los textos** (IR M-13): en `RUN` solo quedan los tres de I-01, por la disposición del Owner (`T4BC` §5). Los nuevos van a `HOSTRUN`, y C14 protege `RUN`.

**El estado de las condiciones de «RETURN BEFORE EMISSION»**, con lo medido:

| Condición | Estado |
|---|---|
| T4B no se puede invocar sin duplicar la política | **No activada.** `TryUpdatePilot` se llama tal cual (D-T4C-01) |
| Hace falta más de una transacción authored | **No activada**, con la lectura de O-12 |
| T1B tiene que cambiar | **No activada** (D-T4C-18) |
| El bloqueo y el contexto no envuelven la corrida | **No activada, por la medida de H0:** la compleción llega siempre (fila 2), el orden es el previsto (fila 5), el documento es el admitido con `TopTransaction` nula (fila 8), los bloqueos anidados funcionan (fila 9) y hay contexto de sincronización (fila 10). El C12 de la fila 3 tocaba la cota, no el envoltorio, y está resuelto (O-16) |
| Undo exige un cambio material | **No activada, por la medida de H0** (fila 6): un solo paso, con Undo y Redo completos. Smoke-2 lo confirma con el producto |
| El escritor necesita una autoridad persistente del host | **Decidida por el Owner:** eligió «E-a + D-1» (O-03 y O-04). La descripción de la opción le dejaba a él decidir si el ejecutor sin estado contaba como autoridad persistente, y lo eligió con esa lectura sobre la mesa. H0 no cambia nada aquí |
| Cualquier otra decisión material de producto | **Decidida o resuelta:** O-01, O-02 y O-07 por la disposición del Owner del §9 de la v4, y el C12 de H0 por su decisión (O-16). **Queda abierta solo la ratificación de las desviaciones de H0 (O-17)**, que es de procedimiento y no de producto |

---

## 10. Disposición de las revisiones

### 10.1 La revisión independiente de la v1 (EMITIBLE TRAS CORRECCIONES, 0/12/16)

Antes de aceptar cada hallazgo, la sesión comprobó en el código el hecho que alega. **Los 28 se sostienen.** No se rechaza ninguno; seis se aceptan con matiz.

| Hallazgo | Disposición | Comprobación de la sesión | Dónde queda en la v2 |
|---|---|---|---|
| R-01. El billete sin cota | **Aceptado con matiz.** La cota es la primera entrada en reposo de la sesión desde la aceptación, contada en `SessionState`, en lugar del «siguiente drenaje», que puede no llegar sin pistas. La protege el testigo | El veto (`CORE:1378-1392`); el descarte por comando (`WorkspaceDocumentDiscardCommand.cs:146`, `:176-177`) | D-T4C-02, D-T4C-09, D-T4C-10, D-T4C-16 fila 2, D-T4C-17, O-07 |
| R-02. La presentación no es exacta ni única | **Aceptado.** El cerrojo terminal, el canal único, la regla de `InFlight` falso con `Unavailable`, y el consumo por superficie | Los avisos dentro de la llamada (`CTL:217-219`, `:250-265`; `EBC:317-320`) | D-T4C-02, D-T4C-03, D-T4C-04, D-T4C-05, D-T4C-17 |
| R-03. El TOCTOU del borrador | **Aceptado, con la segunda opción:** `Busy` durante el vuelo, y no la huella | Las operaciones admitidas (`CORE:1045-1060`, `:1074-1090`, `:1105-1133`) | D-T4C-08, D-T4C-17, §9 (resuelto) |
| R-04. Un `Document` cruza Application | **Aceptado.** El objetivo opaco `IPushBackPilotHostTarget`, en `ADAPTER` | La interfaz genérica de la v1 | D-T4C-04, D-T4C-07, INV-19, §9 (resuelto) |
| R-05. El ámbito sin cota estructural | **Aceptado con matiz.** El cierre va dentro de `SetQuiescent`, **antes** de su comparación de `:439`, para que el primer `CommandWillStart` cierre aunque el reposo no cambie | `EBC:317-327`, `:437-442` | D-T4C-09, D-T4C-17 prueba 17, D-T4C-20 fila 17 |
| R-06. Los `*Cancel*` frente a INV-28 | **Aceptado.** Medida en H0, la parada C12, y las respuestas V1, V2 o A-n en O-02 | `EBC:324-327`; `ICL:145-151`; `FRZ:766`, `:894`; `…/session-log.md:10` | D-T4C-09, D-T4C-16 fila 4, C12, O-02 |
| R-07. El inventario de guardas y el alcance | **Aceptado, en sus cuatro partes:** (a) una aserción propia, con `PortMutatingOperations` sin cambios; (b) el cierre dentro de `SetQuiescent`, con los literales intactos, sin añadir el archivo de C2; (c) la regla de olvido, de 13 a 15; (d) los reenviadores `internal` | `TW/WorkspaceLifecycleGuardTests.cs:486-497`, `:540`, `:113`, `:124-139`, `:167`, `:176-177`; `TW/WorkspaceHostC2SourceGuardTests.cs:206-210`; `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:91-104` | D-T4C-09, D-T4C-20 |
| R-08. El planificador y las prohibiciones del adaptador | **Aceptado con matiz.** El planificador se captura **antes** de la llamada, de modo que, sin contexto de sincronización, no se pide nada | `TW/WorkspaceSourceGuardExceptionTests.cs:270` | D-T4C-04 |
| R-09. La frontera Update | **Aceptado con matiz.** `Kept` no pide: el valor dice en general que la transición sigue, pero en Actualizar solo sale de una carrera | `DS:87-92`, `:377-404`; `IWorkspacePilotEditPort.cs:147-163` | D-T4C-11, D-T4C-17 |
| R-10. H0: sus choques, su custodia y el operador | **Aceptado.** Las tres dispensas, el arnés revisado en un worktree desechable, la custodia, el operador, la copia cerrada sin guardar y la tabla resultado → acción | `FRZ:821-822`, `:828-833`; `DSP:2086`; `…/baseline-verification-session.txt:15-16`; `…/session-log.md:18-19` | D-T4C-16, O-01 |
| R-11. INV-03 falso | **Aceptado.** Ningún `Task` ni `async` nuevo, con el censo por archivo de la navegación | `IWorkspaceNavigationPort.cs:94`; `WorkspaceNavigationHost.cs:127`, `:148`; `WorkspaceNavigationViewModel.cs:194-206`; `git grep -c` | H-09, D-T4C-02, INV-03 |
| R-12. «NO SECOND WRITE PATH» en el Plugin | **Aceptado.** G-9 con el censo, G-6 con las cuentas exactas y G-10 | El censo de `_v2_g9_census.py` (H-30) | D-T4C-14 |
| M-01. La regla de drenaje | **Aceptado** | `WorkspaceSession.cs:47-56` | D-T4C-09, D-T4C-20 fila 16 |
| M-02. La guarda de `EnterError` | **Aceptado.** `\.EnterError\s*\(` en Application y en Plugin | `WorkspacePanelViewModel.cs:83`, `:292` | D-T4C-13 |
| M-03. `NotApplying` tras el sello | **Aceptado.** `ClosedInError` con la clasificación, nunca un éxito | `DS:536-537` | D-T4C-05, D-T4C-13 |
| M-04. Dos anclas inexactas | **Aceptado** | `docs/guias/validacion-manual-autocad.md:427-428`; `TW/WorkspacePilotUpdateGuardTests.cs:171` | H-09, H-33, §2.5 |
| M-05. R-T4-13 | **Aceptado con matiz.** C2-M-5 se cierra solo en `TW/WorkspaceLifecycleGuardTests.cs`; C2-M-6 se difiere, porque T4C no toca su archivo y la asignación del mapa de T4B no es una orden | `…/t4b/map-3-carries-guards-rules.md:156-162` | D-T4C-13, §5, §8 |
| M-06. El prohibido | **Aceptado.** F28 prohibida de forma expresa, 65 entradas que siguen y los cinco archivos de UI enumerados | `git ls-files src/RackCad.UI/Workspace` | §6 |
| M-07. `BVM` sobra | **Aceptado.** Sale | `BVM:945` | D-T4C-10, §6 |
| M-08. La prueba 12 frente a la 7 | **Aceptado** | — | D-T4C-17 |
| M-09. El orden de 3 (b) y el id reciclado | **Aceptado** | `WorkspaceSession.cs:18` | D-T4C-08, D-T4C-17 prueba 3 |
| M-10. Ninguna excepción hacia AutoCAD | **Aceptado.** `OnCallback` es total | — | D-T4C-04, D-T4C-17 |
| M-11. El paso de Undo vacío y el doble clic | **Aceptado** | `FVM:397-399` | D-T4C-11, D-T4C-15, D-T4C-16 fila 7, §7 |
| M-12. R-T4-06 y la ranura | **Aceptado.** La ranura sigue siendo el oráculo de T4B, y el host responde `Busy` antes | `T4P:555-558` | D-T4C-02 |
| M-13. Los textos en `RUN` y una parada | **Aceptado con matiz.** Los tres de I-01 se quedan en `RUN` por la disposición del Owner, y los nuevos van a `HOSTRUN`. Se añade C14 | `T4BC` §5 | D-T4C-05, §6 C14, §9 (resuelto) |
| M-14. El corredor en un campo y el cerrojo | **Aceptado** | — | D-T4C-04, D-T4C-14 G-3 |
| M-15. D-1 y las lecturas | **Aceptado.** T4C no añade ninguna lectura | `A2P:660` | D-T4C-12 |
| M-16. Dos restricciones vigentes | **Aceptado**, más la de `SessionState` y la lista de nombres de H-20 | `TW/WorkspacePilotUpdateGuardTests.cs:117-124`; `TW/WorkspaceModelBoundaryTests.cs:84-104` | H-06, H-17, H-20, §6 |

**Recuento:** 28 aceptados, de ellos 6 con matiz (R-01, R-05, R-08, R-09, M-05 y M-13), y 0 rechazados.

### 10.2 La re-revisión de la v2 (EMITIBLE TRAS CORRECCIONES, 0/7/8)

RR §2 da 26 de los 28 anteriores por resueltos y R-12 por parcial, por N-02. Considera legítimos los seis matices de la v2 y no pide otra re-revisión de arquitectura (RR §7).

Antes de aceptar cada hallazgo nuevo, la sesión comprobó en el código el hecho que alega. **Los 15 se sostienen.** No se rechaza ninguno; cuatro se aceptan con matiz.

| Hallazgo | Disposición | Comprobación de la sesión | Dónde queda en la v3 |
|---|---|---|---|
| N-01. La liberación del billete empezado | **Aceptado con matiz.** Las tres partes de RR: las operaciones totales del núcleo, el `finally` exterior del corredor con una sola llamada terminal, y la prueba RQ con el núcleo real. Se añade una defensa más: la caducidad del billete empezado que no entró en `Execute`, con la primera entrada en reposo posterior a su `Admit`. Además, `Retire` solo se llama si el escritor llegó a crearse | El diseño de la v2 (`Admit` marca el billete; el `catch` total) | D-T4C-02, D-T4C-04 (pasos 7 y 8), D-T4C-10, D-T4C-17, INV-02, O-07 |
| N-02. G-6 imposible por `WorkspacePanelCommand.cs` | **Aceptado con matiz:** la excepción exacta, con dos `[CommandMethod(`, **conservando** `CommandMethod` en G-6, para que un comando nuevo en la carpeta salte también aquí. Así R-12 queda resuelto | `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:22`, `:38` (H-34) | D-T4C-14 G-6, D-T4C-20 fila 19 |
| N-03. G-3 imposible por los anidados de `HOSTRUN` | **Aceptado.** G-3 se limita a los tipos de larga vida, con la regla de que solo `Request`, desde `TryRequest`, crea el corredor | El diseño de la v2 | D-T4C-04, D-T4C-14 G-3 |
| N-04. `CloseOwnMutation` en la caducidad | **Aceptado, con la primera opción:** el cierre sale de la caducidad, por inalcanzable, y una guarda fija que `ExpireStaleUpdate` y `TakeUpdateProgress` no lo nombran | El diseño de la v2; con N-01, la entrada en reposo que dispara la caducidad del billete empezado ya cerró el ámbito en `SetQuiescent` | D-T4C-02, D-T4C-09, D-T4C-14 G-10, D-T4C-20 fila 5 |
| N-05. `:699-700` sin declarar | **Aceptado.** La fila 12 lo incluye, y se corrige la afirmación de D-T4C-12 | `TW/WorkspaceEditIntentGuardTests.cs:688-700` (H-37) | D-T4C-12, D-T4C-20 fila 12, §2.5 |
| N-06. El prohibido con archivos del permitido | **Aceptado.** El prohibido solo lleva archivos que no están en el permitido. En los compartidos, protegen D-T4C-20 y C07 | Los tres archivos que nombra RR están en el permitido de §6 | D-T4C-20 (a) y (b), §6 |
| N-07. El cierre temprano sistemático | **Aceptado.** Pasa a C12, con V3 (dos fases) o una A-n en O-02. Solo un caso aislado se declara | `ICL:151` | D-T4C-09, D-T4C-16 fila 5, C12, O-02 |
| N-08. `TryConfirmBase` sobre un Dirty en vuelo | **Aceptado, con la primera opción:** `Waiting` en la lista de D-T4C-08 | `DS:743-760` (H-35); `EditConfirmOutcome.Waiting` en `WorkspaceEditIntent.cs:353` | D-T4C-08, D-T4C-17, §2.5 |
| N-09. La activación y el modo degradado | **Aceptado con matiz.** Se corrige la promesa. Para la sesión degradada no se añade un miembro más: `QuiescentEntriesOf` devuelve nulo con `IsDegraded`, `RequestUpdate` responde `Unavailable`, un billete no empezado caduca, y `ReportFault` cierra el ámbito (en la v4, `Degrade`, el punto único: CK C3-02) | `EBC:144`, `:223-241`, `:312`; `WorkspaceSessionRegistry.cs:39-46` no rechaza una sesión degradada (H-36) | D-T4C-02, D-T4C-09, D-T4C-17, D-T4C-20 fila 17 |
| N-10. Sin fin de comando ni entrada en reposo | **Aceptado:** C12, y no se declara en IC-09 sin el Owner | El diseño de la v2 | D-T4C-09, D-T4C-16 fila 5, C12, O-02 |
| N-11. La redacción de INV-19 | **Aceptado,** con la primera opción: la letra de `OO` y ningún tipo de AutoCAD en las firmas | `OO`, «RUN-SCOPED WRITER» | D-T4C-04, INV-19 |
| N-12. La cuarta cláusula de §12.1 | **Aceptado** | `FRZ:830` | H-33, D-T4C-16, O-01 |
| N-13. `ClosedInError` para `NotApplying` | **Aceptado, con la segunda opción:** se amplía la documentación del valor en el mismo cambio de M-03 | `WorkspaceEditIntent.cs:419-420` (H-38) | D-T4C-05, D-T4C-13 |
| N-14. El ancla del `await` | **Aceptado** | `WorkspaceNavigationViewModel.cs:231` | H-09 |
| N-15. La nueva aceptación vacía el `Failed` | **Aceptado con matiz:** las dos propuestas de RR a la vez. La caducidad dentro de `RequestUpdate` sale por el valor de inicio y no acepta la petición. La nueva aceptación ya no vacía el almacén, y el gesto recoge antes lo pendiente | El diseño de la v2 | D-T4C-02, D-T4C-03 (reglas 4 y 5), D-T4C-05, D-T4C-11 (pasos 1 y 5), D-T4C-17 |

**Recuento:** 15 aceptados, de ellos 4 con matiz (N-01, N-02, N-09 y N-15), y 0 rechazados.

**El §9 que pide RR §6:**
- N-07 está en O-02 y en C12;
- N-12 está en O-01;
- la recomendación de O-07 se ha vuelto a revisar con N-01, y sigue siendo segura.

### 10.3 La comprobación dirigida de la v3 (EMITIBLE TRAS CORRECCIONES, 0/1/2)

CK §2 da por resueltos N-01 a N-15 y R-12, y considera legítimos los cuatro matices de la v3. Sus tres hallazgos no piden otra revisión de arquitectura (CK §6).

Antes de aceptar cada hallazgo, la sesión comprobó en el código o en el paquete el hecho que alega. **Los tres se sostienen y se aceptan sin matiz.**

| Hallazgo | Disposición | Comprobación de la sesión | Dónde queda en la v4 |
|---|---|---|---|
| C3-01 (IMPORTANTE). V3 no cubre el comando anidado ni N-10, y «cerrar al final del callback» choca con C-F4-04 | **Aceptado.** V3 se redefine: mientras dura el callback, **ningún** evento cierra la ventana, y `Settle` restablece las reglas normales. Así cubre el `CommandWillStart` tardío y el comando anidado. Para N-10, la variante es «cerrar en `Settle`», presentada como **desviación expresa de C-F4-04**. La regla «nunca al final del callback, por `F4P:60`» lleva ahora «salvo la variante de O-02» | El texto de la v3: V3 dejaba cerrar el fin de comando durante el callback, y tras `Settle` volvían las mismas reglas; `F4P:60` | D-T4C-09 (V3, N-10 y la regla de cierre), IC-09, D-T4C-16 fila 5, C12, O-02, D-T4C-20 filas 14 y 15 |
| C3-02 (MENOR). El cierre por degradación en `ReportFault` | **Aceptado.** El cierre va en `Degrade`, el punto único. La prueba OM provoca la degradación con un fallo dentro de `OnSourceEvent` | `Degrade` (`EBC:478-488`) y `Fault` (`:491-495`), con seis entradas (`:140`, `:246`, `:345`, `:399`, `:433` y `:472`); ninguna guarda fija el cuerpo de `Degrade` (`TW/WorkspaceLifecycleGuardTests.cs:216` y `:230` miran otros cuerpos) | D-T4C-09, D-T4C-17 (fila N-09), D-T4C-20 fila 17 |
| C3-03 (MENOR). La caducidad dentro de `RequestUpdate` con el billete de otro rack | **Aceptado.** Si el billete caducado es de otro rack, su `Failed` se guarda bajo su rack y la petición sigue. Si es del mismo, sale por el valor de inicio | El billete es por sesión (D-T4C-02) | D-T4C-02, D-T4C-03 regla 4, D-T4C-05, D-T4C-11 paso 5, D-T4C-17 (fila C3-03) |

**Recuento:** 3 aceptados, sin matiz, y 0 rechazados.

**El §9:** O-02 queda corregido según C3-01, con la alternativa para N-10 como desviación expresa de C-F4-04. O-07 sigue siendo seguro (CK §4).

### 10.4 La incorporación de T4C-H0 (v5)

**Las entradas:**
- el resultado de H0, fila por fila, con sus observaciones O-H0-1 a O-H0-5 y sus desviaciones (`H0R`);
- las notas del arnés, con los hechos de `ExecutionResult` (`H0N` §1);
- la traza (`H0T`);
- la decisión del Owner sobre el C12 de la fila 3 («Completar o comando ajeno»);
- la disposición del Owner sobre el §9 de la v4, que sigue vigente.

La custodia está en la rama: la v4 en `de27426b` y H0 en `44fe029d`. La sesión comprobó en la rama los archivos de `docs/automation/evidence/I-64-pilot/F4-T4C-H0/` que cita la v5, y que `git diff b15072ff 44fe029d -- src tests` sale vacío.

| Entrada | Qué cambia en la v5 | Dónde queda |
|---|---|---|
| Filas 1, 2 y 10: el callback diferido, la compleción siempre y en el hilo de la UI | [H] → [M]. El modelo tolerante se confirma en el caso real, y la compleción pasa a ser la liberación normal del billete | §2.6 (H-39, H-40), D-T4C-02, D-T4C-03, D-T4C-16 |
| Fila 3: dos entradas en reposo antes del callback, C12 | **C12 resuelto por el Owner:** la cota «Completar o comando ajeno» sustituye a la de las entradas en reposo. Se añade la cuenta de comandos ajenos en el puente (`NoteForeignCommandStart`, `ForeignCommandStartsOf`) y se retira `QuiescentEntriesOf` | §2.6 (H-41), D-T4C-02, D-T4C-09, D-T4C-10, D-T4C-17, D-T4C-20 (filas 5, 14, 15, 17 y 22), §6 (INV-02, C12), §9 (O-07, O-16) |
| Fila 4: sin pistas de los `*Cancel*` | V1 y V2 no hacen falta, y quedan declaradas para Smoke-2 | §2.6 (H-42), D-T4C-09, §6 C12, §9 O-02 |
| Fila 5: el orden de los eventos y `F4P:60` | La vía A se confirma con el orden medido. V3 y el cierre en `Settle` no hacen falta, y quedan declarados para Smoke-2 | §2.6 (H-43), D-T4C-09, §6 C12, §7, §9 O-02 |
| Filas 6 y 7: un solo paso de Undo, Undo y Redo completos, y el paso vacío | Undo medido en el diagnóstico, y Smoke-2 lo confirma con el producto. El paso vacío se declara | §2.6 (H-44, H-45), D-T4C-15, §7 |
| Filas 8, 9, 11 y 12: el estado al entrar, los bloqueos anidados, sin bombeo y sin Assert | [H] → [M]. Ninguna acción | §2.6 (H-46 a H-49), D-T4C-04, D-T4C-16 |
| O-H0-4: `ExecutionResult`, no un `Task` | La guarda del adaptador se reescribe: una sola `OnCompleted` con el salto al contexto capturado, las prohibiciones del precedente y `GetResult` solo dentro de un `try`. La totalidad de la continuación se comprueba. La compleción ya no lleva fallo, y la quinta operación de la corrida es `Completed` | §2.6 (H-50), D-T4C-04, D-T4C-05, §6 INV-03 |
| O-H0-1, O-H0-2 y O-H0-3 | La línea base de la mutación propia confirma D-T4C-09. Las lecturas del panel que vacían Rehacer y Redo inmediatamente después de Undo van a Smoke-2. PICKFIRST confirma O-15 | §2.6 (H-51), D-T4C-09, D-T4C-15, §7, §9 O-15 |
| O-H0-5: el paso opcional 10 no se hizo | Ninguna: las filas 1 y 11 no lo necesitan | §2.6 |
| Las desviaciones: el operador, el dibujo y el RELEASE de I-52 | Se declaran, y se pide su ratificación | §2.6 (H-52), D-T4C-16, §7, §9 O-17 |

**Lo que de §10.1 a §10.3 queda superado por H0:**
- **la cota por entradas en reposo** de R-01 (§10.1), N-01 y N-09 (§10.2): ahora va por comando ajeno. La liberación garantizada de N-01 y el nulo de una sesión degradada de N-09 se conservan;
- **la caducidad dentro de `RequestUpdate`** de C3-03 (§10.3) se conserva igual, con el disparador nuevo;
- **las respuestas de N-07, N-10 y C3-01** se conservan como respuestas declaradas para Smoke-2, porque H0 no las activó.

**Lo que se señala de la medición, sin cambiar la decisión del Owner.** Las dos entradas en reposo de la fila 3 llegan con el reposo ya puesto (`H0T:35-36`). Una cota que contara solo los cambios de estado quizá no las habría contado. Aun así, la regla del Owner es más robusta, porque no depende del estado del reposo que lleva el núcleo, y es la que rige.

### 10.5 Lo siguiente

- **La vuelta al Owner** con el §9: solo queda la ratificación de O-17.
- **Después, la emisión de T4C1** (O-09), y luego la de T4C2.
