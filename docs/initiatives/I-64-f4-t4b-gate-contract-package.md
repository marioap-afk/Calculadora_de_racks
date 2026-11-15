# I-64 — Paquete del contrato de F4-T4B (MUTATE y POST del Actualizar del piloto)

**Estado:** v7. La prepara la sesión principal el 2026-11-14, después del cierre de F4-T1B. No declara GATE PASS. **Se emite el 2026-11-14** por la autorización del Owner «Emitir con el §9 propuesto» (`owner-f4t4b-emission-authorization.md`, F4-E60), que aprueba las interpretaciones abiertas del §9 tal como se proponen.
- **v1:** revisión adversarial independiente, **NO EMITIBLE (4/8/14)** (`independent-review.md`; la v1 se conserva en `_package-v1.md`).
- **v2:** rediseñó el puerto y la operación del host. Su re-revisión focalizada dio **EMITIBLE TRAS CORRECCIONES (0/7/14)** (`independent-rereview.md`, citado como RR).
- **v3:** aplica todos los hallazgos de esa re-revisión (§10.2). Son texto y decisiones, sin rediseño.
- **v4:** revalidación contra la rama posterior a T1B (`7c29a5e5`). Lo pide la autorización de T1B: «After T1B closes: return to F4-T4B package v3. Re-run the relevant package validation/review against the post-T1B branch. Then incorporate T1B findings into T4B assumptions. Only then may T4B be emitted». La v4 incorpora:
  - lo que fija T1B (§2.3);
  - la categoría medida de la purga (§2.4, INV-16);
  - la decisión del Owner sobre el movimiento de `Traverse` (D-T4B-04, INV-17);
  - la contención de las corridas locales (INV-18).

  La disposición está en el §10.3.
- **v5:** aplica la comprobación independiente de la v4 (`independent-check-v4.md`, EMITIBLE TRAS CORRECCIONES, 1/4/13). Lo dispone el §10.4.
- **v6:** aplica la comprobación dirigida de la v5 (`independent-check-v5.md`, EMITIBLE TRAS CORRECCIONES, 0/2/6). Lo dispone el §10.5.
- **v7:** la comprobación de la v6 da **EMITIBLE (0/0/3)** (`independent-check-v6.md`). La v7 aplica sus tres menores de texto, sin otra revisión (§10.6).

**Precondiciones de emisión:**
- F4-T4A2 cerrada: **se cumple** (F4-E57).
- F4-T1B cerrada (R-T4-12): **se cumple** (F4-E59, custodia `7c29a5e5`).
- La validación determinista y la revisión contra `7c29a5e5`: **se cumple**. La v4 dio 1/4/13, la v5 0/2/6 y la v6 EMITIBLE con 0/0/3; las correcciones están en la v7 (§10.3 a §10.6).
- Una orden del Owner que autorice la emisión y decida lo que queda abierto del §9.

**Código base:** `7c29a5e5`.
- Su producto es el de `c3ae1c97`: `git diff c3ae1c97 7c29a5e5 -- src` sale vacío. Por eso los hechos del §2 medidos en `c3ae1c97` se sostienen sin volver a medirlos.
- Entre los dos no cambia nada en `src/`, y en `tests/` solo los siete archivos de T1B; el resto del diff es de `docs/`. Si cambia producto antes de la emisión, se vuelve a medir (`T4P:177`).

**Mapas:**
- `map-1-plugin-write-path.md` (M1), `map-2-core-apply-plan.md` (M2) y `map-3-carries-guards-rules.md` (M3);
- `map-4-traverse-t1b-coverage.md` (M4) y `map-5-purge-category.md` (M5), medidos en `7c29a5e5`.

La revisión se cita como IR.

**Abreviaturas:**
- `T4P` = `docs/initiatives/I-64-f4-t4-gate-contract-package.md`
- `A2P` = `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md`
- `FRZ` = `docs/initiatives/I-64-proposal-v5.md`
- `TW` = `tests/RackCad.Tests/Workspace/`

**Presupuesto:** 1 de 3.

## 1. Autoridades

- **`T4P`:**
  - §3: la fila de T4B (`:172`), el orden (`:175-177`) y las decisiones 3 y 4 (`:182-183`);
  - §3.1 y §3.2 (`:185-213`);
  - D-T4A1-04 (`:397-407`);
  - §7: R-T4-01 a R-T4-04, R-T4-11 a R-T4-13 y el oráculo de R-T4-06 (`:526-583`).
- **El Owner:**
  - `owner-f4t4a2-authorization.md`: TOCTOU (`:307-317`), el plan inmutable (`:320-334`) y «first and only task in F4 authorized to write authored DWG state» (`:395-400`);
  - `owner-f4t4-preparation-authorization.md` (`:126-147`);
  - `owner-f4t4-decomposition-decision.md`.
- **`FRZ`:**
  - D-12 (`:463-498`);
  - D-13 (`:500-545`), con la vía implementable (`:507-509`), «la mecánica no se congela» (`:506`), la tabla por frontera (`:521-532`) e INV-30 aditiva (`:533-535`);
  - INV-06 (`:744`), INV-10 (`:748`), INV-12 (`:750`), INV-14 (`:752`), INV-30 (`:768`) e INV-39 (`:777`);
  - M-02 (`:995`).
- **Las disposiciones** F4-E07, F4-E16, F4-E25, F4-E51 y F4-E53 a F4-E56.
- **IC-08** (`docs/initiatives/I-64-implementation-clarifications.md:145-150`): «obligación vinculante para F4-T4B» en el sentido anterior a F4-E51, y por eso pasa a T4C (R-T4-08, IR M-7).
- **`A2P`:** §8 y D-T4A2-06.
- **El Owner, después de la v3:**
  - `owner-f4t4-disposition-reverify-t1b-t4b.md`:
    - la vía propia del piloto: OV-RED no valida el escritor nuevo, y Smoke-2 es su primera validación física;
    - Application decide y el Plugin solo hace operaciones primitivas;
    - el puerto por ámbitos, «accepted in principle»;
    - el movimiento del barrido como refactorización;
    - la purga, «LOG ONLY» salvo que deje incierto lo authored («The package must prove which category applies»);
    - T4B es la escritura y su semántica, y T4C el contexto de ejecución;
    - la distinción A/B/C del fallo de commit;
  - `owner-f4t1b-authorization.md`: el orden posterior al cierre de T1B, y «Do not revert to the coarse I-55 seam»;
  - `owner-f4t4b-traverse-decision.md`: «Guardas exactas en T4B» (D-T4B-04).
- **F4-T1B**, CLOSED / PASS WITH LIMITATIONS (F4-E59): su paquete de cierre (`I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/task-close-package.md`), CT-24 a CT-27 y sus menores M-02 y M-03.

## 2. Hechos medidos en `c3ae1c97`

Los H-01 a H-13 de la v1 se sostienen (IR, «Lo comprobado»). Aquí van resumidos, con las correcciones y los hechos que añadió la revisión. En `7c29a5e5` el producto es el mismo. Los §2.3 y §2.4 son nuevos de la v4, medidos en `7c29a5e5`.

### 2.1 El núcleo

**H-01. La ranura** (`Workspace/Drafts/DraftSession.cs`):
- `TryBeginApply` (`:508-524`): `AnotherUpdateInProgress` está en `:515`.
- `CompleteApply` (`:531-554`) libera la ranura **antes** de clasificar.
- `Classify`: `Applied` y `AppliedWithWarnings` con relectura admitida dan Clean; sin relectura admitida dan Unknown(PostReread), y **se pierden los avisos**. `CommitThrew` da Unknown(CommitThrew).
- El comparador se evalúa en `Status` en **todas** las ramas (`:298-300`, `:564-604`) y en `ClosedDetached` (`:540`).

**H-02. F4-E25 (4) está abierto.** Lo fija la `Theory` de `TW/WorkspaceDraftPilotOwnershipTests.cs:417-458`, que mezcla las filas `discard` y `keep` (que deben seguir lanzando) con `applied` y `warnings`.

**H-03. `DraftApplyOutcome`** (`Drafts/DraftApplyOutcome.cs`):
- ya tiene `AppliedWithWarnings(postRead, warnings)`;
- `AppliedPostRereadFailed(cause)` no lleva avisos;
- `NotAppliedConflict` no lleva causa (`:69-74`).

**H-04. El plan** (`Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs`):
- es inmutable;
- no lleva el `PushBackSystem`, el extremo ni el lado, ni ningún `ObjectId`;
- `Prepare(input, pilot, catalog)` necesita el catálogo;
- la regla Build (b) elige el origen por texto exacto (`:342-345`);
- M-5 (`:379-381`) y M-6 (`:308-311`) siguen abiertos.

El tipo del estado del piloto es `PushBackPilotDraftState` (IR M-1).

**H-05. La admisión exige `OriginKey`** «chosen by the host» (`Pilot/PushBackPilotIntentScan.cs:84-85`); sin él, OriginMissing (`PushBackPilotIntentReader.cs:163-166`).
- Hoy el origen y la autorización salen de `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:232-248`.
- `NavigationTargets` no se puede nombrar fuera de las carpetas del panel (`TW/WorkspaceNavigationPrivacyGuardTests.cs:14-42`).
- El host no nombra `.InitialDraftState` (`TW/WorkspaceEditIntentGuardTests.cs:712`).

**H-06. Guardas del host** (IR M-14):
- en `WorkspaceHostCore.cs` y en los archivos del puerto, de la intención y del soporte, **ni en los comentarios** pueden aparecer `Transaction`, `\bDatabase\b` ni `\bCommit\s*\(` (`TW/WorkspacePilotEditGuardTests.cs:838-842`; `TW/WorkspaceEditIntentGuardTests.cs:1011`);
- `LockDocument` y `RackLog` son trampas del código (`TW/WorkspacePilotEditGuardTests.cs:812`);
- las API de escritura están prohibidas en archivos enteros (`TW/WorkspaceLifecycleGuardTests.cs:667-684`);
- **ningún campo del núcleo** puede ser de un tipo de `RackCad.Application.Systems*` (`TW/WorkspaceEditIntentHostTests.cs:1002-1020`);
- el constructor no cambia (`TW/WorkspaceEditIntentGuardTests.cs:555-559`).

**H-07. `IWorkspacePilotEditPort`** tiene cinco implementadores (IR B-1):
- la compuerta (`WorkspacePilotSupport.cs:153`);
- `src/RackCad.Plugin/Workspace/WorkspaceController.cs:278`;
- dos dobles de Core (`TW/WorkspaceCommandAdvisorTests.cs:17`, `:1107`);
- el doble de UI (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:25`).

Añadir una operación los obliga a cambiar a todos.

**H-08. El precedente de puerto con forma de ámbito** es `IRackProjectionWriteScope` con `RackProjectionMaterializationRun` (`src/RackCad.Application/Views/Placement/RackProjectionMaterialization.cs:153-168`, `:228-303`).

### 2.2 El Plugin

**H-09. Las fachadas:**
- `PrepareRedraw` importa (`Systems/Shared/ViewBlockDraw.cs:86`) y fija el `BlockId` en `PreparedViewRedraw` (`Systems/Shared/PreparedViewRedraw.cs:22-49`). Carga su propio catálogo (`ViewBlockDraw.cs:84`).
- `RedrawInTransaction` devuelve `staleDefinitions` (`:97-109`), que la purga necesita.
- Las piezas faltantes salen **en MUTATE**, en `LateralHeaderDrawOutcome.MissingInstances` (`src/RackCad.Plugin/Drawing/LateralHeaderDrawOutcome.cs:31-33`), no en la importación: `EnsureForPlan` solo cuenta las importadas.
- `CallerOwnedFacadeGuardTests.Facades` (`:49-56`) no incluye ninguna fachada de Push Back.

**H-10. El barrido:**
- `CaptureDrawing` → `Traverse` toma su propio bloqueo, su transacción y su `Commit` (`Views/RackSiblingScan.cs:112-163`).
- Lo usan Insertar de I-55 (`RackSelectivoInsertIntegration.cs:81`), el insertar de RACKEDITAR (`Views/RackUnsupportedSiblingInsert.cs:43`), RACKPROYECTAR (`Views/RackProjectionSnapshotReader.cs:127`) y el lector del piloto (`Views/RackPilotIntentAdapter.cs:116`).
- Las guardas de cuenta y de firma (`TW/WorkspaceEditIntentGuardTests.cs:88-128`) admiten extraer un núcleo interno.

**H-11. POST:**
- `SyncName` (`Drawing/RackBlockRenamer.cs:21-58`) y la purga (`LateralHeaderDrawer.cs:193-238`) devuelven `void` y se tragan las excepciones. `SyncName` además se salta en silencio un nombre ocupado.
- `ApplyRegen` es la autoridad única del regen.
- `RackBlockData.Write` trocea cada 255 caracteres y vuelve en silencio con un JSON vacío.

**H-12. Transacción ajena:** los precedentes rechazan una `TopTransaction` abierta con `TOP_TRANSACTION_OPEN` (`Views/RackViewBatchDriver.cs:52-53`; `Application/Views/Insertion/RackSiblingInsertRun.cs:117-118`).

**H-13. Debug:**
- La CI solo compila el Plugin en Release (`eng/ci/verify-autocad-references.ps1:197`, `:595`). Ningún job compila el código `#if DEBUG` del Plugin.
- Hay un precedente `[Conditional("DEBUG")]` en `Views/RackPilotIntentAdapter.cs:195`.

**H-14. La costura de I-55**, por ruta exacta:
- `src/RackCad.Plugin/Systems/Shared/{AutoCadSiblingRedrawPort,SiblingRedrawTransaction,SiblingRedrawUnits,SiblingRedrawDebugFaultInjection,PreparedViewRedraw}.cs`;
- `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs`;
- `src/RackCad.Application/Views/Redraw/**` (`ISiblingRedrawPort`, `RackSiblingMembership`, `RackSiblingRedrawPlan`, `RackSiblingRedrawRun`).

`PreparedViewRedraw.cs` se consume, pero no se modifica.

### 2.3 Lo que fija F4-T1B (en `7c29a5e5`)

**H-15. La línea base caracterizada.**
- Son 55 resultados de UI y 10 de Core (`PushBackClassicCharacterization*`), en verde en la CI `43650040527`.
- Son la línea base de compatibilidad del editor clásico de Push Back para T4B y T4C, sin respaldar sus rarezas (OD-1..OD-13).

**H-16. Las fachadas quedan congeladas.**
- CT-27 (1) (`tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs:322-354`) fija:
  - las firmas públicas exactas de las tres fachadas de Push Back;
  - la existencia de `PrepareRedraw` y `RedrawInTransaction`, con su firma `internal` exacta.
- Es lo que usa el escritor (D-T4B-03), y coincide con la parada C02.

**H-17. Los caminos clásicos no llaman a los caller-owned.**
- CT-27 (2) (`:366-382`) prohíbe `PrepareRedraw(` y `RedrawInTransaction(` en cuatro archivos enteros: `RackPushBackCommands.cs`, `KindHandlers/PushBackKindHandler.cs`, `RackMenuCommands.cs` y `Views/RackViewBatchProducts.cs`.
- El escritor nuevo (`Views/RackPilotUpdateWriter.cs`) queda fuera de esa guarda.
- T4B no puede añadir un llamador en esos cuatro archivos: todos están en el prohibido (M-03 de la revisión de T1B).

**H-18. RACKEDITAR.**
- CT-25 fija el orden posterior a `TryAuthorize`: `PrepareAll` < `RedrawInPlace` < `SyncName` < `EraseViewBlocks` < un `Regen()` < `RackViewBatchExecution.Run`, con `regen: false` por vista.
- Las guardas de orden de T1B son débiles, porque buscan la primera aparición posterior (M-02 de su revisión). T4B no se apoya en ellas para los caminos clásicos: esos caminos los protegen el prohibido (`*Commands*.cs`) y un `git diff` vacío en sus archivos.

**H-19. T1B no cubre el barrido** (M4).
- Ninguna de sus guardas lee `RackSiblingScan.cs`, `RackUnsupportedSiblingInsert.cs`, `RackSelectivoInsertIntegration.cs` ni `RackProjectionSnapshotReader.cs`; su contrato los puso en el prohibido.
- Ninguna prueba ejecuta `Traverse` (ADR-0003).
- **Lo protegen hoy guardas de texto parciales:**
  - `TW/WorkspaceEditIntentGuardTests.cs:74-128`: firmas, cuentas antiduplicación y un orden parcial;
  - `SingleScanInsertGuardTests` (`G9bInsertWiringGuardTests.cs:7-35`);
  - `RackViewBatchDriverGuardTests:37-50`;
  - `G16FirstViewCorrectiveTests:159-166`;
  - `RackProjectionCommandGuardTests:118-128`;
  - y las guardas del adaptador.
- **Nada automatizado fija:**
  - el cuerpo del recorrido, fuera de esas anclas;
  - los cuerpos de `Capture` y `CaptureDrawing`;
  - el orden bloqueo → transacción → recorrido → `Commit`.
- Nada en T1B revela una diferencia de comportamiento.

**H-20. Los cuatro llamadores del barrido** (M4 §2):

| Id | Camino | Llamada |
|---|---|---|
| A | Insertar de Selectivo; es el único con `eraseByKind` no trivial | `RackSelectivoInsertIntegration.cs:81` |
| B | El insertar de RACKEDITAR de Push Back, Dinámico, Cantilever y Cabecera, todos por el mismo `RackUnsupportedSiblingInsert.TryAuthorize` | `Views/RackUnsupportedSiblingInsert.cs:43` |
| C | RACKPROYECTAR | `Views/RackProjectionSnapshotReader.cs:127` |
| D | El lector del piloto | `Views/RackPilotIntentAdapter.cs:116` |

**H-21. Las cuentas de la base** (CI `43650040527`):
- **Core:** 18471 resultados; `RackCad.Tests.Workspace`, 2923.
- **UI:** 2310 resultados, 2293 en verde y 17 omitidas que ya lo estaban; `RackCad.UI.Tests.Workspace`, 601; `RackCad.UI.Tests.PushBackClassicCharacterization`, 55.
- Las de cada clase, en el §6.

**H-22. Las corridas locales** (F4-E59, reglas del Owner).
- La suite de UI existente escribe `RackFrameConfigurator.layout` en `LocalApplicationData` al cerrar el configurador real. Es la deuda H-1 de T1B.
- En una tarea con contención de perfil, las suites completas de UI solo corren en la CI.
- Ninguna sonda temporal ejerce con entradas válidas un camino con diálogo o escritura.

**Observación** (punto 3 de la revisión independiente de T1B): en la base, la `LastComputation` del fixture compuesto de UI difiere de la que da un recálculo. T4B no toca la UI; se anota.

### 2.4 La categoría de la purga (M5)

**H-23. Una sola implementación.**
- La entrada única **de los caminos de redibujo** es `SystemBlockWriter.PurgeAfterCommit` (`Systems/Shared/SystemBlockWriter.cs:147-150`). Reenvía a `LateralHeaderDrawer.PurgeUnreferenced` (`Drawing/LateralHeaderDrawer.cs:193-238`), que devuelve `void`.
- Hay dos llamadas directas más a `PurgeUnreferenced`, que no son de redibujo: `Drawing/BlockPlacement.cs:213` (la limpieza de un Insertar cancelado) y `RackCommandSupport.cs:274` (`EraseViewBlocks`, prohibido al piloto por R-T4-11).
- Las fachadas de Push Back **no** purgan: solo devuelven `staleDefinitions` (`PushBackSystemDrawService.cs:64-66`, `PushBackFrontalDrawService.cs:62-64` y `PushBackPlantaDrawService.cs:56-58`).

**H-24. Qué borra la purga.**
- **Las candidatas** son las definiciones que referenciaba el contenido **antiguo** de la vista, menos las que referencia el nuevo (`LateralHeaderDrawer.cs:97-113`, `:128-161` y `:177`).
- **La purga corre en su propia transacción** (`:202`), filtra por 0 referencias con `Database.Purge` (`:220`) y solo borra BTR de basura (`:221-227`).
- **Nunca toca** el BTR de la vista, su payload ni su identidad (`Systems/Shared/RackBlockData.cs:20-51`).
- **Sus excepciones:** captura `System.Exception`, registra con `RackLog.Exception` (que no lanza, `RackLog.cs:23-24`) y no relanza (`:233-237`).

**H-25. El orden.**
- Todos los llamadores purgan **después** del `Commit` authored y fuera de su `using`:

  | Llamador | Commit | Purga |
  |---|---|---|
  | `SystemBlockWriter` | `:87` | `:91` |
  | `LateralHeaderDrawService` | `:141` | `:145` |
  | `ProjectVariableMutationExecutor` | `:312` | `:316` |
  | el `post` de I-55 | `Systems/Shared/SiblingRedrawTransaction.cs:23` | `RackSelectivoInsertIntegration.cs:253` |

- `CallerOwnedTransactionGuardTests.cs:197-238` lo fija en **los dos escritores clásicos** que mira: el commit va antes de la purga, la purga antes del regen, y cada uno tiene una sola entrada. No mira al escritor del piloto: lo cubren las guardas de D-T4B-09.
- `StartTransaction` se anida si hay una `TopTransaction` abierta.

**H-26. Es categoría (b), solo registro, con ese orden.**
- Un fallo aborta solo la transacción de la purga. Deja lo authored, ya confirmado, más basura que los lectores de racks no ven (`Views/RackSiblingScan.cs:119-135`).
- **Si la purga compartiera la transacción authored, se anidara en ella o se intercalara, pasaría a (a).** Es el precedente anterior a `f2ab66bb` (M5 §2.3): entre dos `WriteUnit` podría borrar una definición que necesita la unidad siguiente.
- Ningún camino clásico convierte hoy un fallo de la purga en fallo de la operación.

## 3. El reparto entre T4B y T4C (D-T4B-01)

| Pieza | T4B | T4C |
|---|---|---|
| La lectura caller-owned (núcleo de `Traverse`) y F4-E26 por construcción | **Sí** | — |
| La corrida pura que **decide y clasifica** (D-12, INV-06, INV-12, INV-30, C-F4-05 y POST), probada en Core con un doble del ámbito | **Sí** | — |
| El escritor del Plugin, que solo hace operaciones primitivas dentro de ámbitos por corrida | **Sí** | — |
| `TryUpdatePilot`, un **método público del núcleo** (no una operación del puerto `IWorkspacePilotEditPort`), con la disciplina de la ranura | **Sí** | La operación del puerto, la compuerta, el adaptador del controlador, los dobles y la llamada de la UI |
| `CompleteApply` por rama (F4-E25 (4)), los avisos que sobreviven (F4-E16), M-1, M-5, M-6 y M-10 | **Sí** | — |
| Actualizar llama a `NotePilotEdit` y quita la marca de limpio (la parte de R-T4-07 que va con la operación, IR M-10) | **Sí** | El resto de R-T4-07 |
| El contexto de comando, el `using LockDocument` único, el ámbito de mutación propia (R-T4-08 e IC-08), la UI, Undo y K = 1 | — | **Sí** |
| El diagnóstico de `TryCheckNow` | — | **Sí** (§9 I-3) |

## 4. Decisiones de F4-T4B

### D-T4B-02. El puerto: ámbitos por corrida, y la corrida pura decide (IR B-3, B-4; RR R-I-3 a R-I-6, R-M-6 a R-M-8, R-M-13)

**`src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotUpdatePort.cs`** (nuevo). El puerto **solo devuelve valores del barrido**: no elige el origen ni cuenta supervivientes (RR R-I-5).

**Las operaciones:**

```
IPushBackPilotUpdatePort
    IPushBackPilotUpdateScope Begin(PushBackPilotUpdateRequest request)

IPushBackPilotUpdateScope : IDisposable
    PushBackPilotScanValues ReadForPrepare()
    PushBackPilotImportResult PrepareUnits(PushBackPilotUpdatePlan plan)
    PushBackPilotMutationStart BeginMutation()
    PushBackPilotPostValues Post(PushBackPilotPostRequest request)

IPushBackPilotMutationScope : IDisposable      // Dispose sin Seal = abortar
    PushBackPilotScanValues Reread()
    PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText)
    void Seal()
```

**Los tipos de valor**, con la lista exacta de campos (RR R-M-7):

| Tipo | Campos |
|---|---|
| `PushBackPilotUpdateRequest` | `AuthorizedDocumentNativeId` (`long`, el `_probe.ActiveDocumentNativeId` que validó la puerta) y `RackId` (`string`) |
| `PushBackPilotScanValues` | `DocumentMatches` (`bool`); los hechos y los textos crudos del barrido sin origen, con los mismos tipos que consume `BuildStaleBase` y `RackSiblingMember`; `LockedLayerKeys` (las claves `Mutable` con capa bloqueada); el `Catalog` (solo en `ReadForPrepare`); `PreparedBlockMatches` (por clave, si el bloque preparado es el releído; solo en `Reread`) |
| `PushBackPilotPostRequest` | `DesiredNames`: por clave, el nombre crudo que calcula la corrida con la regla por vista del clásico (D-T4B-03, `Post`). Una clave sin nombre deseado no se renombra y **no da aviso** |
| `PushBackPilotPostValues` | `ForeignScopeOpenAtPost` (`bool`); `Scan` (los mismos valores de barrido que `Reread`, leídos con `CaptureDrawing` después de la purga, de `SyncName` y del regen); `NamesByKey` (el nombre de la definición de cada clave, leído **después** de `SyncName`); `RegenFailure` (`string?`, el tipo y el mensaje si `ApplyRegen` lanzó). **Son hechos, no avisos:** los avisos los decide la corrida |
| `PushBackPilotImportResult` | `Ok` y `Cause` |
| `PushBackPilotMutationStart` | `Scope` (`IPushBackPilotMutationScope?`) y `RejectionCode` (`string?`; hoy solo `TOP_TRANSACTION_OPEN`). **Un rechazo es un valor, no una excepción** |
| `PushBackPilotUnitResult` | `MissingPieces` (`IReadOnlyList<string>`) |

**`WriteUnit(key, envelopeText)`** recibe el texto del plan, y el escritor comprueba que es el del plan para esa clave (RR R-M-13). Así el doble de Core ve qué texto va con qué clave: INV-10 en su parte de escritura.

**Comentarios:** ningún comentario del puerto, de la corrida ni de la ayuda contiene la palabra `Transaction`. `TOP_TRANSACTION_OPEN` en mayúsculas no la activa, porque la regla distingue mayúsculas de minúsculas. La guarda de pureza lee el texto **con** los comentarios (RR R-M-8).

**`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs`** (nuevo), puro y **total**: nunca lanza (RR R-I-3). Sus causas son constantes propias (RR R-M-6):
- NothingToApply, NotApplicable y PendingText bajo Applying;
- cero supervivientes;
- el destino;
- el documento equivocado;
- `TOP_TRANSACTION_OPEN`. Es el valor de una constante cuyo identificador **no contiene** `Transaction`, por la guarda de pureza: `ForeignScopeOpenCause = "TOP_TRANSACTION_OPEN"` (N-7 de la comprobación de la v6);
- `LOCKED_LAYER`;
- y las constantes que ya existen, `SeveralOriginsReason` y `NoOriginReason` (`PushBackPilotUpdatePlan.cs:271-273`).

**1. Antes de la ranura** (lo llama el host): `Prepare(From(draft), null, null)`.
- **Solo para** con NotApplicable, PendingText y NothingToApply.
- Cualquier otro resultado sigue adelante. Por ejemplo, el `Invalid(MissingInputReason)` de un Dirty sin pendiente es normal sin catálogo (RR R-M-6).

**2. Bajo Applying**, `scope = port.Begin(request)`, liberado en el `finally` exterior de (6):

**(a) `ReadForPrepare`**. Si `DocumentMatches` es falso: NotAppliedPreflight (documento). Si no, **la corrida elige el origen en puro** (RR R-I-5):
- es el único `Mutable` cuyo texto crudo es, byte a byte, `From(draft).InitialDraftState.Origin`, con la misma regla que Build (b);
- con cero coincidencias: NotAppliedPreflight(`NoOriginReason`); con varias: NotAppliedPreflight(`SeveralOriginsReason`), en lugar del OriginMissing engañoso del lector;
- con exactamente uno: `PushBackPilotIntentReader.Read` con esa clave da el resultado del piloto, y después `Prepare(From(draft), pilot, catalog)`.

Bajo Applying:
- NothingToApply, NotApplicable y PendingText dan NotAppliedPreflight con su causa;
- Conflict da NotAppliedConflict;
- NotAdmitted da NotAppliedAdmission;
- Invalid da NotAppliedPreflight.

**(b) Antes de importar** (RR R-I-4, M-3): los supervivientes, contados con `RackSiblingMember.IsSurvivor` sobre los hechos; con cero, NotAppliedPreflight. Y `LockedLayerKeys` ∩ los miembros del plan; si no está vacío, NotAppliedPreflight(`LOCKED_LAYER: …`). Ninguno de los dos casos deja residuo OM-5.

**(c) `PrepareUnits(plan)`:** la importación, con su residuo OM-5 declarado.

**(d) `BeginMutation()`.** Con un `RejectionCode`: NotAppliedPreflight(código). Si no, `m = start.Scope`, liberado en el `finally` de (6):
- `Reread()` → la elección del origen otra vez, en puro, que tiene que dar la misma clave que el plan (si no, NotAppliedConflict) → `BuildStaleBase` → `DraftStaleBase.Create(plan.ExpectedBase, plan.ExpectedVariableRegistryText).CompareWith(...)`. Con una diferencia: **NotAppliedConflict y cero `WriteUnit`** (INV-06);
- **el destino:** las claves del plan son iguales a las `Mutable` releídas y todos los `PreparedBlockMatches` son verdaderos. Si no, NotAppliedPreflight;
- **la autoridad de MUTATE:** los supervivientes y las capas, otra vez;
- un `WriteUnit(key, plan.Envelope(key))` por miembro. Si uno lanza: NotAppliedFailure, sin `Seal` (INV-12);
- **`Seal()` en su propio `try`**, y la marca `sealedReturned = true` cuando vuelve.

**3. POST.** Siempre que `sealedReturned` es verdadero:
- **El origen de POST es `plan.OriginKey`** (RR R-I-6): la clave de la definición, que no cambia, aunque su texto ya sea el del plan.
- La corrida calcula los `DesiredNames` del plan, llama a `Post(request)` y construye la relectura de POST con esa clave a partir de `PushBackPilotPostValues.Scan`.
- **La interpretación de POST es de la corrida** (Application, por la disposición del Owner, «Application owns … POST interpretation»):
  - el nombre esperado de cada clave **con nombre deseado** es `BlockNaming.SanitizeBlockName(desiredName)` (`src/RackCad.Application/BlockNaming.cs`), comparado con `NamesByKey` con `OrdinalIgnoreCase`, como en `SyncName`. Si difiere, da aviso; también si `SyncName` se saltó un nombre ocupado;
  - **una clave sin nombre deseado** no se compara: `SanitizeBlockName(null)` da `"Cabecera"` (`BlockNaming.cs:12-17`), y compararla daría un aviso falso;
  - un `RegenFailure` da aviso;
  - con `ForeignScopeOpenAtPost` verdadero, el escritor no ha purgado, ni renombrado, ni releído, y la corrida da AppliedPostRereadFailed(`TOP_TRANSACTION_OPEN`, avisos). Es imposible por construcción, porque `BeginMutation` rechaza una transacción ajena y `FRZ:518`; si ocurre, no hay POST normal.
- Después, el comparador del D-T4B-05.

**4. La clasificación por frontera, total:**

| Qué pasa | Resultado |
|---|---|
| Una excepción antes de `Seal`: en `Begin`, `ReadForPrepare`, `PrepareUnits`, `BeginMutation`, `Reread` o `WriteUnit`, y el `Dispose` de `m` (el aborto) no lanza | NotAppliedFailure con su causa |
| **El aborto falla:** `sealedReturned` es falso, **algún `WriteUnit` ya empezó** y el `Dispose` de `m` lanza | **CommitThrew** con la causa `ABORT_FAILED: …`, y por tanto Unknown(CommitThrew). No se afirma ningún rollback (la B del Owner) |
| El `Dispose` de `m` lanza sin `Seal` y **sin ningún `WriteUnit` empezado** | NotAppliedFailure con su causa |
| Una excepción **en** `Seal` | **CommitThrew** |
| `Seal` vuelve y el `Dispose` de `m` lanza | **POST** (la fila «`Commit()` retornó» de D-13, `FRZ:527`), y el `Dispose` se convierte en un aviso (RR R-I-3) |
| `Post()` lanza, o falta su relectura | AppliedPostRereadFailed(causa, avisos) |
| El comparador o el constructor de un resultado lanzan después de `Seal` | AppliedPostRereadFailed(causa, avisos) |
| El `Dispose` del ámbito exterior lanza | **La clasificación ya decidida no cambia.** Después de `Post` es un aviso. Fuera de POST, en NotApplied*, CommitThrew o `ABORT_FAILED`, se añade a la causa |

**5. Los avisos:** las piezas faltantes de cada `WriteUnit`, los que la corrida deriva de `PushBackPilotPostValues` y los de los `Dispose`. Llegan siempre al resultado (D-T4B-06).

**6. Las excepciones no se tapan.**
- La corrida no libera `m` ni `scope` con un `using`. Los libera en dos `finally` propios, el de `m` dentro del de `scope`. Cada uno captura la excepción del `Dispose` aparte de la del `WriteUnit`, la de `Seal` o la de `Post`, para poder aplicar la tabla.
- **La precedencia** (N-4 de la comprobación de la v5): se conserva la clasificación ya decidida y el `Dispose` solo añade su causa.
- **Hay dos salvedades declaradas** (N-8 de la comprobación de la v6):
  - **NotAppliedConflict no lleva causa** (`DraftApplyOutcome.cs:69-74`; D-T4B-06). La excepción de un `Dispose` sobre un Conflicto se descarta: es una limitación declarada, con su prueba;
  - **después de `Post`, un `Applied` pasa a `AppliedWithWarnings`** con el aviso del `Dispose`. Es la única salvedad a «no cambia», y las dos ramas dan Clean.
- **Los casos:**
  - un Conflict o un Preflight de MUTATE, con sus diferencias, sigue siéndolo aunque el `Dispose` de `m` lance;
  - si `Seal` lanzó y después el aborto también lanza, sigue siendo CommitThrew por `Seal`, con la causa del aborto añadida;
  - `ABORT_FAILED` solo se aplica cuando la clasificación provisional es NotAppliedFailure con algún `WriteUnit` empezado.
- **La prueba con el doble** cubre las tres filas del aborto (I-4 de la comprobación de la v4), el `Dispose` exterior que lanza fuera de POST y los dos casos de precedencia.

**Lo que prueban de verdad los dobles de Core** (INV-06, INV-11 e INV-12, INV-30):
- el origen con cero, uno o varios candidatos, y el texto del origen cambiado tras MUTATE que **termina en Applied**;
- los supervivientes y las capas;
- un `Seal` que vuelve con su `Dispose` que lanza, que va a POST;
- un comparador que lanza, que da AppliedPostRereadFailed.

**La ayuda pura del sistema** (IR M-5): `PushBackPilotSystemRebuild.cs`, como en la v2.

### D-T4B-03. El escritor del Plugin

`src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs` (nuevo) tiene **tres clases**, una por interfaz (RR R-M-9):
- **la del puerto**, sin ningún campo de AutoCAD. `Begin` devuelve **cada vez una instancia nueva** del ámbito;
- **las dos de ámbito**, que son las únicas con campos `Transaction`, `ObjectId`, `Document`, `PreparedViewRedraw` y `staleDefinitions`.

**Dónde viven los objetos de AutoCAD:** solo dentro de las instancias de ámbito, que se liberan en `Dispose` (INV-14 e INV-F4T4B-08). El puerto que conserve T4C no retiene ninguno.

**Lo que hace el escritor ya no lo decide:** solo comprueba el documento (`document.UnmanagedObject.ToInt64() == AuthorizedDocumentNativeId`), lee y hace las operaciones primitivas. El origen, los supervivientes, las capas, los nombres esperados y los avisos de POST los decide la corrida (D-T4B-02).

**El bloqueo:** el escritor **no toma `LockDocument`**. El único `using LockDocument` alrededor de PREPARE, MUTATE y POST es del adaptador de T4C (R-T4-06). Los bloqueos anidados de `CaptureDrawing`, `SyncName` y la purga ya existen (IR I-3).

**Lo que hace cada operación:**
- **`ReadForPrepare`:** `CaptureDrawing` sin cambios, más el catálogo, fuera de MUTATE y sin transacción poseída (N-9 de la comprobación de la v6). Así lee por el mismo núcleo que los demás caminos, y el censo de dos llamadas al núcleo se mantiene.
- **`PrepareUnits`:** las `*DrawService.PrepareRedraw` de las fachadas, que importan antes de cualquier transacción (R-T4-03).
  - El sistema sale de la ayuda pura; el corte, de `RackViewCodec`.
  - El catálogo de `PrepareRedraw` es el suyo, distinto del de la lectura previa: se declara (IR M-4).
- **`BeginMutation`:** `TopTransaction == null`, o el rechazo. Después `StartTransaction`. La transacción es un campo del ámbito de mutación, que la libera en su `Dispose`, y la corrida libera ese ámbito en su `finally`.
- **`Reread`:** la lectura caller-owned (D-T4B-04), con la identidad de la transacción comparada por `UnmanagedObject`.
- **`WriteUnit(key, envelopeText)`:** comprueba que `envelopeText` es el del plan para esa clave. Después, `RedrawInTransaction` de la fachada de esa clave (INV-10). Recoge `staleDefinitions` y `MissingInstances`.
- **`Seal`:** `transaction.Commit()`.
- **`Post(request)`** solo hace operaciones primitivas y devuelve hechos (INV-16). El orden está alineado con el clásico:
  1. **comprueba `TopTransaction == null`.** Si no se cumple, devuelve `ForeignScopeOpenAtPost = true` sin hacer nada más;
  2. **la purga, una vez:** `SystemBlockWriter.PurgeAfterCommit` con la **unión** de las `staleDefinitions` de todas las `WriteUnit`, después de que `Seal` haya vuelto y de que la corrida haya liberado el ámbito de mutación. Nunca `LateralHeaderDrawer.PurgeUnreferenced` directamente, ni `.Purge(`;
  3. `SyncName` con cada `DesiredNames[key]` de la petición. Una clave sin nombre deseado no se renombra;
  4. **la lectura de los nombres**, en una transacción propia **de solo lectura** (`StartOpenCloseTransaction`, sin `ForWrite`, `UpgradeOpen` ni escritura), que devuelve `NamesByKey`;
  5. un `ApplyRegen`; si lanza, `RegenFailure`;
  6. la relectura con `CaptureDrawing` sin cambios, **después** de la purga, para que el comparador observe el estado posterior.
- **La regla por vista de `DesiredNames`** es la del clásico (`RackPushBackCommands.cs:189`, `:282`, `:300-309` y `:339-342`), calculada en la corrida (Application):
  - `base` es `Name.Trim()` del sobre del plan, o nulo si está en blanco. Con `base` nulo no hay nombre deseado;
  - planta: `RackViewBaseName.LinkedPlanta(base)`;
  - lateral: `RackViewBaseName.LinkedLateral(base, postIndex)`;
  - corte: `RackViewBaseName.LinkedPushBackCut(base, end, side, system.IsComposite)`, con el extremo y el lado del corte de `RackViewCodec` y `IsComposite` de `PushBackPilotSystemRebuild`.
  - Lleva una prueba por vista y una con el nombre en blanco.
- **El orden purga → `SyncName` es el del clásico** (`SystemBlockWriter.cs:91` y después `RackPushBackCommands.cs:282`; `RackSelectivoInsertIntegration.cs:253-254`). R-T4-04 (`T4P:540`) y el paso 5 de D-13 (`FRZ:519`) lo enumeran al revés. Se declara (§9 I-15): `FRZ:519` remite a «la autoridad existente», y `FRZ:506` no congela la mecánica. El nombre de la vista no puede coincidir con el de una definición anidada, porque estas llevan siempre el sufijo `_<Rol>_<n>` (`HeaderInstanceGrouper.cs:96`).
- **La purga no da aviso ni fallo, y no cambia el desenlace:** es categoría (b) (§2.4, INV-16), y su fallo solo va al registro.

**Lo que nunca hace:**
- `Erase(` ni `EraseViewBlocks` (R-T4-11);
- `RackUnitsGuard`;
- `async`, `await` ni `ExecuteInCommandContextAsync`;
- nombrar `NavigationTargets`.

**La inyección Debug** (INV-30; IR I-5, M-9):
- **Variables de entorno:**
  - `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT=k`: la unidad k lanza;
  - `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL=before`: lanza **antes** del `Commit` real, sin escritura, y por tanto «Comprobar ahora» da MATCH y recupera;
  - `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL=after`: lanza **después**, con escritura, y por tanto Conflict.
- **El patrón:** los puntos de inyección son métodos `[Conditional("DEBUG")]` (el precedente de H-13), y solo la lectura de la variable va en `#if DEBUG`. Se declara frente a la regla «`#if DEBUG` sin `#else`».
- **La evidencia** (RR R-M-14): `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -p:UseAutoCADNuGetReferences=true -o <carpeta temporal fuera del repo>`.
  - La salida va a una carpeta temporal, para no pisar el DLL Debug que carga AutoCAD en Smoke-2.
  - Primero se mide la base en Debug. Si ya tiene avisos, el criterio es «sin avisos nuevos».

### D-T4B-04. La lectura caller-owned y el movimiento de `Traverse` (R-T4-01, INV-39; IR I-6; M4; decisión del Owner «Guardas exactas en T4B»)

**El núcleo.** Va en `RackSiblingScan.cs`, con esta firma: `internal static RackSiblingDrawingScan TraverseCore(Transaction transaction, Database database, ObjectId selectedDefinition, Func<RackEmbedDocument, bool> eraseByKind)`.
- **Es `internal`**, porque lo llama `RackPilotUpdateWriter.cs`, del mismo ensamblado; `RackSiblingScan` ya es `internal static class` (`:77`). La guarda (2) fija la visibilidad y la firma exactas. El nombre se fija en la emisión.
- La firma lleva **`selectedDefinition` y `eraseByKind`**, porque `Capture` los necesita (M4 §1.2).
- **Lo que recibe de hoy:** `:103-110` (los diccionarios y los almacenes locales), `:115-161` (la tabla y el bucle) y `:166-169` (la construcción única del resultado).
- **Lo que pasa al envoltorio:** `:111-114` (la línea en blanco, el bloqueo, la transacción y la `{`) y `:162-165` (la línea en blanco, el `Commit` y las llaves).

**`Traverse`** conserva su firma y queda **exactamente** así, en este orden, sin `try`/`catch` en el archivo:
1. `using (document.LockDocument())`;
2. `using (var transaction = document.Database.TransactionManager.StartTransaction())`;
3. una llamada `var scan = TraverseCore(transaction, document.Database, selectedDefinition, eraseByKind);`;
4. `transaction.Commit();`, después de la llamada y dentro de los `using`;
5. `return scan;`.

**La variante caller-owned** del escritor (`Reread`, D-T4B-03) llama al núcleo con `(transaction, database, ObjectId.Null, null)`, sin `StartTransaction`, `LockDocument` ni `Commit` (INV-39).

**La protección del movimiento es propia de T4B.** La condición (3) de la disposición, «characterization T1B protects the relevant existing behavior», no se cumple (H-19). Por decisión del Owner, se sustituye, y se declara, por esta:

1. **Equivalencia exacta del núcleo.**
   - **El literal:** `c3ae1c97:src/RackCad.Plugin/Views/RackSiblingScan.cs:103-110`, `:115-161` y `:166-169`, con **una única sustitución**: `document.Database.BlockTableId` → `database.BlockTableId` en `:115`.
   - **La normalización**, fija y declarada en la guarda:
     - CRLF → LF;
     - quitar los comentarios `//` hasta el fin de línea y los `/* … */`, fuera de los literales de cadena;
     - colapsar cada secuencia de espacios en blanco a un espacio;
     - recortar los extremos.
   - El cuerpo del núcleo, **sin las llaves del método**, normalizado, es **idéntico** al literal normalizado.
   - Lleva **autopruebas negativas**: inyectar `directOnly: false`, quitar `|| definition.IsAnonymous` o quitar `envelope != null &&` la hacen fallar.
2. **El envoltorio exacto:** el cuerpo de `Traverse` es el de arriba, en ese orden. La firma del núcleo es la declarada, con `internal static`.
3. **`Capture` y `CaptureDrawing`, congelados como texto exacto.**
   - `CaptureDrawing`: `Traverse(document, ObjectId.Null, null)`.
   - `Capture`: `Traverse(document, selectedDefinition, eraseByKind)`, seguido de `RackSiblingMembership.Classify(scan.Facts, rackId, originalSourceId, originalSourceIdIsAttributable)` con su construcción.
   - Hoy solo se fijan sus firmas (`TW/WorkspaceEditIntentGuardTests.cs:93-96`).
4. **Un censo del núcleo:**
   - exactamente dos llamadas, desde `Traverse` y desde `RackPilotUpdateWriter.cs`;
   - ningún otro archivo de `src/` lo nombra;
   - se conservan las cuentas antiduplicación de `TW/WorkspaceEditIntentGuardTests.cs:74-128`.
4b. **El resto del archivo, congelado** (I-2 de la comprobación de la v4).
   - **La región** son los dos miembros, `Traverse` y el núcleo, localizados por su firma; el núcleo va inmediatamente después de `Traverse` (N-6 de la comprobación de la v5).
   - El texto normalizado de `RackSiblingScan.cs` sin esos dos miembros, esté donde esté cada cosa, es **idéntico** al de `c3ae1c97` sin `:98-170`: los constructores y `MembershipFor` (`:16-28`, `:43-55`, `:72-73`), `Capture`, `CaptureDrawing` y **`AddLayer`** (`:172-179`), al que llama el núcleo y del que salen `Layers`, es decir, el preflight de capas de Insertar y el `LOCKED_LAYER` del piloto. Cualquier otro cambio en el archivo es C09.
5. **`RequiredTests`:** una fila por clase también para `SingleScanInsertGuardTests`, `RackViewBatchDriverGuardTests`, `G16FirstViewCorrectiveTests`, `RackSiblingCustomPropertiesGateTests` y `RackEnvelopeIdProbeTests` (§6).
6. **Smoke-2 cubre los cuatro llamadores** (H-20):
   - A;
   - B por Push Back, que representa por construcción a Dinámico, Cantilever y Cabecera porque comparten `TryAuthorize`. Se declara;
   - C;
   - D: «las tres lecturas» del piloto la incluyen.
7. **Evidencia opcional:** `git diff --color-moved=zebra --color-moved-ws=allow-indentation-change <BaseSha>..<GREEN> -- src/RackCad.Plugin/Views/RackSiblingScan.cs`.
8. **RED y GREEN** (m-5 de la comprobación de la v4).
   - **El movimiento va en el GREEN.**
   - **Fallan en el RED:** (1), (2), (4) y la parte de (4b) que exige la región nueva.
   - **Pasan ya en el RED, por construcción, y se declaran** como la extensión de `CallerOwnedFacadeGuardTests`:
     - (3), porque `Capture` y `CaptureDrawing` no cambian;
     - la igualdad de (4b) fuera de la región;
     - las negativas de INV-39 sobre la variante, que en el RED es un cuerpo inerte;
     - y las de D-T4B-09 que solo prohíben:
       - la pureza de la corrida, del puerto y de la ayuda;
       - los «sin …» del escritor;
       - la parte de campos de INV-14;
       - «sin `BlockNaming`, `RackViewBaseName` ni textos de aviso»;
       - el ámbito de mutación sin purga;
       - las trampas de censo de m-11;
       - la guarda de M-1;
       - y «`PrepareRedraw(` solo en `PrepareUnits`».
   - **Fallan en el RED** las positivas de D-T4B-09:
     - el `StartTransaction` de `BeginMutation`, el `StartOpenCloseTransaction` de `Post` y el `Commit()` de `Seal`;
     - `PurgeAfterCommit` en `Post` y la comprobación previa;
     - la guarda de orden de `Post`;
     - los `finally` de la corrida;
     - la inyección Debug.

**Por qué así.** El `AssertInOrder` de la v3 comprobaba una subsecuencia de anclas por su primera aparición (M4 §5):
- no detectaba sentencias insertadas, tokens cambiados ni duplicados en el mismo archivo;
- dejaba fuera `:103-110` y `:166-169`.

**Lo demás:**
- **`eraseByKind`** sigue nulo en el piloto, con la misma `CompletenessEvidence`.
- **F4-E26 se cumple por construcción:** los tres caminos de F4-E26 (`T4P:530`) usan el mismo núcleo y la misma `BuildStaleBase`: «Comprobar ahora»; la vía de O-04 en contexto de comando, que en T4B es `ReadForPrepare` bajo Applying; y la lectura caller-owned de `Reread`. POST lee con el mismo núcleo, por `CaptureDrawing`. La prueba en un dibujo real es de Smoke-2. Los cuatro llamadores del barrido (H-20) son cosa de INV-17.
- **La regla del origen de MUTATE** (D-T4B-02) difiere de la de «Comprobar ahora» (la referencia elegida y `NavigationTargets`). R-T4-01 exige la equivalencia de la StaleBase, no del método de elección (§9 I-9).
- **El núcleo de recorrido** devuelve también las capas por clave, que el barrido ya recoge (`RackSiblingScan.cs:61`, `:128-139`, `:155`).

### D-T4B-05. El comparador de POST

Es puro y está en la corrida. Con la relectura de POST admitida:
- el mismo conjunto de claves que `PostExpectation.Envelopes`;
- para cada clave, un `EnvelopeText` **ordinalmente** igual;
- la escala igual.

| Resultado del comparador | Desenlace |
|---|---|
| Coincide, sin avisos | `Applied(postRead)` |
| Coincide, con avisos | `AppliedWithWarnings(postRead, avisos)` |
| No coincide, o la relectura no está admitida | `AppliedPostRereadFailed(causa, avisos)`, que da Unknown(PostReread) |

**Sin normalizar nada.** La conservación de los bytes en el host se mide en Smoke-2. Si no se conservan, es STOP y la comparación no se debilita (R-T4-04).

### D-T4B-06. `CompleteApply` por rama y los avisos que sobreviven (IR I-2, M-12)

En `DraftSession.cs`, sin archivo nuevo en `Drafts/` y sin `Transaction`, `Commit(` ni `Debug.`:

| Rama | Comportamiento |
|---|---|
| `Applied` y `AppliedWithWarnings`, con la escritura confirmada | La copia se hace **antes** de liberar la ranura. Si el copiador o el comparador lanzan: Unknown(PostReread) con causa |
| `CommitThrew` | Unknown(CommitThrew) se conserva. Si el cálculo del estado lanza, se queda en esa rama con causa |
| NotApplied* | Se conserva la clasificación. Si el cálculo del estado lanza: **Error** con `ErrorReason` (respaldo definido, guardado por R-3 y M-4; §9 I-10). **Nunca** «aplicado» |
| Todas | La ranura se libera exactamente una vez, la excepción no escapa y el error diferido se sigue aplicando |

- **Una prueba por fila.**
- **La trampa de texto exacto** (RR R-M-2): el bloque del error diferido (`DraftSession.cs:550-551`) queda **idéntico byte a byte**, con su sangría, porque `TW/WorkspacePilotPrepareGuardTests.cs:303-306` lo busca literalmente. El respaldo Error usa `_explicitStatus = DraftStatus.Error;` seguido de `ErrorReason = …;` en el mismo bloque, como en `:268-294`.
- **`ClosedDetached`** no evalúa el comparador, porque `_explicitStatus` sigue en Applying (RR R-M-1). La fila «Todas» sigue valiendo.
- **La `Theory` de `TW/WorkspaceDraftPilotOwnershipTests.cs:417-425`** se divide: las filas `discard` y `keep` quedan intactas y siguen lanzando; las de `applied` y `warnings` pasan al comportamiento corregido. El cambio va en el commit RED.
- **F4-E16:** `AppliedPostRereadFailed(cause, warnings = null)` en `DraftApplyOutcome.cs`. `Classify` conserva `Warnings` también en la rama sin relectura admitida.
- **M-10:** el XML de la sobrecarga de Conservar.
- **El Conflicto de MUTATE** no lleva causa, solo diferencias. Se acepta expresamente: las diferencias son la causa (IR M-2).

### D-T4B-07. `TryUpdatePilot` en el núcleo (IR B-1, B-2, I-3, M-6, M-8, M-10; RR R-I-1, R-I-2, R-M-3 a R-M-5)

**La firma:** `public EditUpdateOutcome TryUpdatePilot(EditIntentRequest request, IPushBackPilotUpdatePort port)` en `WorkspaceHostCore.cs`.
- Es un método público y no una operación de `IWorkspacePilotEditPort`. El puerto es un parámetro.
- **Su firma es una excepción declarada a D-T2B2-06**, porque su segundo parámetro es de `Systems`. `TryUpdatePilot` **no** entra en el bucle de firmas de `TW/WorkspaceEditIntentHostTests.cs:970-979`; `EditUpdateOutcome` sí entra en los portadores (`:982`) (RR R-M-5).

**La puerta propia, sin reposo ni lector** (RR R-I-1, opción a; §9 I-11):
- el documento activo;
- ningún modal de RackCad;
- `Authorize` de la sesión, con la forma exacta de `PassGate`.

Su guarda sigue el patrón de `PassGateViolations` (`TW/WorkspacePilotReconciliationGuardTests.cs:526-541`). **La puerta de reposo pasa a la operación del puerto de T4C**, antes de entrar en el contexto de comando. Así T4C llama a `TryUpdatePilot` dentro del callback sin reescribirla.

**En orden:**
1. La puerta.
2. El borrador. Con el rack en blanco, `NoDraftReason`.
3. La comprobación previa de la corrida.
4. `TryBeginApply`.
5. La corrida, que es total.
6. **`CompleteApply(resultado de la corrida)` en `finally`, exactamente una vez.** Si la corrida no llegó a `Begin`, NotAppliedFailure con la causa.
7. `NotePilotEdit` y `_knownClean.Remove(request.Session)`, literal, en los resultados que cambian el borrador.

**Lo que no hace:**
- **No lee por el lector del host:** D-1 no cambia.
- **No nombra `pilot`.**
- **No usa `ChosenReference` ni `Witness`,** y no llama a `HasWitnessWithoutKey` (RR R-M-4).

**Textos:**
- las salidas tempranas usan **solo** las constantes del núcleo que ya existen (`NoDraftReason`), de modo que la tabla de 30 literales y sus cuentas no cambian (RR R-M-4);
- los rechazos y las causas son constantes de la corrida.

**`EditUpdateOutcome`** es un `struct`, en `WorkspaceEditIntent.cs`, con `Status`, `Reason`, `Differences` (textos) y `Warnings`. Su valor por defecto es **`Unavailable`**, inerte. Una prueba fija los nombres y el orden, a la manera de `TW/WorkspaceEditIntentHostTests.cs:1023-1047` (RR R-I-2).

**`EditUpdateStatus`, en este orden:**

| # | Situación | `Status` | `Reason` / `Differences` / `Warnings` |
|---|---|---|---|
| 0 | Valor por defecto | `Unavailable` | — |
| 1 | La puerta rechaza (sesión, documento o modal) | `Rejected` | el motivo de la puerta |
| 2 | Sin borrador, o rack en blanco | `NoDraft` | `NoDraftReason` |
| 3 | La comprobación previa: NothingToApply | `NothingToApply` | constante de la corrida |
| 4 | La comprobación previa: PendingText. La frontera Update de `T4P:401` es de T4C (R-T4-09) | `PendingText` | constante |
| 5 | La comprobación previa: NotApplicable | `NotApplicable` | constante |
| 6 | `TryBeginApply`: `AnotherUpdateInProgress` | `AnotherUpdateInProgress` | — |
| 7 | `TryBeginApply`: `NotRegistered`, `InvalidStatus` o `PendingText` | `Refused` | el nombre del rechazo |
| 8-15 | `CompleteApply` → `NotAppliedConflict`, `NotAppliedAdmission`, `NotAppliedPreflight`, `NotAppliedFailure`, `CommitThrew`, `Applied`, `AppliedWithWarnings` o `AppliedPostRereadFailed` | el mismo nombre | `Reason` = `LastApplyCause`; `Differences` = `DescribeDifferences(draft.ConflictDifferences)`, leídas después de `CompleteApply` (forma que no toca `BaseOrReadUse`); `Warnings` = `draft.Warnings` |
| 16 | `CompleteApply` → `ClosedDetached` | `ClosedDetached` | — |
| 17 | El respaldo de I-10 (el borrador queda en Error) | `ClosedInError` | `ErrorReason` |
| 18 | Una excepción **antes** de `TryBeginApply` (la guarda de contención) | `Failed` | el mensaje. Después de `TryBeginApply` no hay `Failed` suelto: el resultado sale siempre del `DraftApplyResult` |

`NotApplying` es imposible por construcción, y una prueba lo fija.

**Cambios de guarda declarados**, exactos y con autoprueba en negativo:
- **`PortMutatingOperations`** (`TW/WorkspaceLifecycleGuardTests.cs:463-474`): `TryUpdatePilot` se añade **al final**, para respetar el corte `Skip(5)` de `:527-534` (RR R-M-3);
- **`ChangesADraft`** (`:476`): gana `.TryBeginApply(` y `.CompleteApply(`;
- **el cuerpo de `TryUpdatePilot`** entra en la guarda de escrituras (a la manera de `ReconciliationBodies`, `TW/WorkspacePilotEditGuardTests.cs:686-691`);
- **la contención como Failed** (`TW/WorkspaceEditIntentGuardTests.cs:583-596`), solo antes de `TryBeginApply`;
- **el «nada desde el drenaje ni el Idle»** (`:599-640`);
- **la guarda de la puerta propia;**
- **el censo de portadores** (`TW/WorkspaceEditIntentHostTests.cs:982`).

### D-T4B-08. Los menores y los arrastres

| Id | Qué hace T4B |
|---|---|
| M-1 | Una guarda de `ComposeForWrite` con autoprueba `Copy(current)`. **El mapa de evidencia lo corrige la sesión en la custodia**: está en `docs/`, prohibido al Worker (IR I-3) |
| M-5 | `HasAddress` antes de la disponibilidad, con su texto propio. `TW/WorkspacePilotPrepareTests.cs:1758` y `:1772` cambian en el RED |
| M-6 | `catalog == null` al principio de `Build` |
| M-10, F4-E25 (4), F4-E16 | D-T4B-06 |
| D-1 | Sin cambio |
| Diagnóstico de `TryCheckNow` | A T4C (§9 I-3) |
| C2-M-3 a C2-M-7 | No se tocan |
| m-1, D2-1 y D2-2 de T4A2 | Los dispone el cierre de T4A2 |

### D-T4B-09. Las guardas

**Nuevas,** en `TW/WorkspacePilotUpdateGuardTests.cs`:
- **la pureza** de la corrida, del puerto y de la ayuda: sin `Transaction`, `Database`, `\.\s*Commit\s*\(`, `LockDocument`, `Autodesk`, `async`/`Task`, archivos ni reflexión;
- **el escritor, anclado por cuerpo** (IR M-11):
  - no nombra `RackBlockData.Read`, `BlockTableId` ni `GetBlockReferenceIds`: solo lee por `RackSiblingScan` (`CaptureDrawing` y el núcleo). Las cuentas antiduplicación de `TW/WorkspaceEditIntentGuardTests.cs:77-86` solo miran `RackSiblingScan.cs` (N-9);
  - `PrepareRedraw(` solo en el cuerpo de `PrepareUnits`;
  - un `StartTransaction` en `BeginMutation`, después de comprobar `TopTransaction`, y ninguno más en el archivo;
  - un `StartOpenCloseTransaction`, solo en el cuerpo de `Post`, para la lectura de nombres, sin `ForWrite` ni `UpgradeOpen` en el archivo;
  - un `Commit()` en `Seal`, y ninguno más en el archivo;
  - sin `LockDocument`, `Erase(`, `EraseViewBlocks`, `RackUnitsGuard`, `async`, `await`, `ExecuteInCommandContextAsync` ni `NavigationTargets`;
  - INV-14: el ámbito de mutación lo libera siempre la corrida, en su `finally`; su `Dispose` libera la transacción; y no hay ningún campo `Transaction`, `DBObject`, `Document` ni `ObjectId` fuera de las clases de ámbito;
  - sin `BlockNaming`, `RackViewBaseName` ni textos de aviso: la interpretación de POST es de la corrida (I-3 de la comprobación de la v4);
- **la variante de lectura caller-owned** (INV-39): sin `StartTransaction`, `LockDocument` ni `Commit`;
- **el movimiento de `Traverse`:** las guardas de D-T4B-04 (1) a (4b), con sus autopruebas negativas;
- **la purga** (INV-16):
  - `PurgeAfterCommit` solo en el cuerpo de `Post`, y una vez;
  - la clase del ámbito de mutación sin `PurgeAfterCommit`, `PurgeUnreferenced` ni `.Purge(`;
  - `Post` comprueba `TopTransaction` antes de purgar;
  - **el orden del cuerpo de `Post`,** con cuentas de una aparición, porque la primera aparición es débil (M-02 de T1B): la comprobación de `TopTransaction` < `PurgeAfterCommit` < `SyncName` < la lectura de nombres < `ApplyRegen` < `CaptureDrawing`;
- **las trampas de censo de todo `src/`** que alcanzan a los archivos nuevos (m-11), que se declaran:
  - ningún archivo nuevo construye ni compone un `RackEmbedDocument` (`CustomPropertiesEnvelopeGuardTests.cs:24-34`, `:69-75` y `:194-236`); una fábrica inerte del RED no puede romper esa clase, que va con `ExpectRed` false;
  - ninguno añade `[CommandMethod(` (`SelectiveEditorOpenTests.cs:565-572`);
  - ninguno nombra `Insunits` ni `RackUnitsGuard` (`RackUnitsGuardSourceTests.cs:87-101`);
- **la inyección Debug** (D-T4B-03);
- **M-1.**

**Extendida:** `CallerOwnedFacadeGuardTests.Facades`, con las tres fachadas de Push Back. Ya cumplen en la base, así que pasa en el RED y en el GREEN.

**Sin cambios:**
- `SiblingRedrawSeamGuardTests`, `DebugFaultInjectionGuardTests` y la costura de I-55;
- las pruebas de T1B.

## 5. Trazabilidad

| Regla | Dónde |
|---|---|
| R-T4-01 / INV-39 / F4-E26 | D-T4B-04, D-T4B-09 |
| R-T4-02 / INV-06 / INV-12 / INV-14 / INV-30 | D-T4B-02, D-T4B-03 |
| R-T4-03 / OM-5 / decisión 3 (`LegacyDeltas`) | D-T4B-02 (c), D-T4B-03. `LegacyDeltas` ya está en la expectativa del plan; Smoke-2 lo observa |
| R-T4-04 / C-F4-05 / F4-E25 (4) / F4-E16 | D-T4B-02, D-T4B-05, D-T4B-06 |
| INV-11 (la clasificación de D-13 en Core, `FRZ:749`) | D-T4B-02, la tabla de frontera y las pruebas de la corrida (RR R-M-13) |
| INV-10 en su parte de escritura | D-T4B-02, con `WriteUnit(key, envelopeText)`, que el doble de Core ve |
| Las capas bloqueadas (D-13, paso 3; O-05, `FRZ:513`, `:1103`; `T4P:407`) | D-T4B-02 (b) y MUTATE (RR R-I-4) |
| R-T4-06 (oráculo) / INV-F4T1-20 | D-T4B-07 |
| R-T4-11 | D-T4B-03 |
| TOCTOU y el plan inmutable | D-T4B-02: `ReadForPrepare`, y `Reread` dentro de MUTATE. D-T4B-03: los objetos solo en los ámbitos |
| La purga (disposición del Owner, M5) | §2.4, D-T4B-03, INV-16 y D-T4B-09 |
| El movimiento de `Traverse` (disposición y decisión del Owner, M4) | §2.3, D-T4B-04, INV-17, D-T4B-09 y §7 |
| La línea base de T1B (H-15 a H-18) | INV-13 y `RequiredTests` |
| Las reglas de F4-E59 para las corridas locales | INV-18 |

## 6. Contrato propuesto de F4-T4B (a fijar en la emisión)

**Identidad:**
- `F4-T4B-MUTATE-POST`, I-64, F4;
- 1 de 3;
- las celdas de F4-T4A2, con nc4 en la primera delegación y nc1..nc3.

**`AllowedWriteScope`**, por ruta exacta:
- **Application, archivos nuevos:**
  - `src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotUpdatePort.cs`;
  - `PushBackPilotUpdateRun.cs`;
  - `PushBackPilotSystemRebuild.cs`.
- **Application, archivos modificados:**
  - `.../Pilot/PushBackPilotUpdatePlan.cs`;
  - `src/RackCad.Application/Workspace/Drafts/DraftSession.cs` y `DraftApplyOutcome.cs`;
  - `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` y `WorkspaceEditIntent.cs`.
- **Plugin:** `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs` (nuevo) y `src/RackCad.Plugin/Views/RackSiblingScan.cs`.
- **Pruebas nuevas:** `TW/WorkspacePilotUpdateRunTests.cs`, `TW/WorkspacePilotUpdateHostTests.cs`, `TW/WorkspacePilotUpdateGuardTests.cs` y `TW/WorkspacePilotUpdateScopeDouble.cs`.
- **Pruebas modificadas:**
  - `tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs`;
  - `TW/WorkspaceDraftPilotOwnershipTests.cs` y `TW/WorkspacePilotPrepareTests.cs`;
  - `TW/WorkspacePilotPrepareGuardTests.cs`, `TW/WorkspaceLifecycleGuardTests.cs`, `TW/WorkspacePilotEditGuardTests.cs`, `TW/WorkspaceEditIntentGuardTests.cs` y `TW/WorkspaceEditIntentHostTests.cs`;
  - `TW/WorkspaceHostTestSupport.cs`.

**`ForbiddenWriteScope`**, por raíces y archivos exactos:
- `src/RackCad.UI/` y `tests/RackCad.UI.Tests/`;
- `src/RackCad.Plugin/Workspace/`;
- la costura de I-55 de H-14;
- la clausura de T1B (G1 a G6) y sus fragmentos compartidos;
- todo `*Commands*.cs`;
- `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`;
- `.../Pilot/PushBackPilotMaterializer.cs`;
- el resto de `src/RackCad.Application/` y de `src/RackCad.Plugin/` fuera de lo permitido;
- `src/RackCad.Domain/`, `.github/`, `docs/`, los `.csproj` y `tests/RackCad.Tests/Fixtures/`;
- las clases de prueba de T1B.

`RackSiblingScan.cs` está **permitido**, porque no es la costura (IR I-3). Los llamadores del barrido (H-20) quedan en el prohibido: el movimiento no los toca.

**`RequiredTests`** (IR I-4), con las cuentas de la base medidas en la CI `43650040527` (H-21):

| Proyecto | Filtro | `MinSelected` | `ExpectRed` |
|---|---|---|---|
| Core | `FullyQualifiedName~RackCad.Tests.Workspace`. Selecciona por **prefijo del nombre de clase**, porque los archivos de `TW` declaran `namespace RackCad.Tests;`: entran todas las clases `Workspace*`, incluidas las `WorkspacePilotUpdate*`, la guarda de INV-39 y `WorkspaceEditIntentGuardTests` | **2923** (el suelo de la base); la entrega declara las nuevas | true |
| Core, **una fila por clase** (RR R-I-7; D-T4B-04 (5)) | `FullyQualifiedName~RackCad.Tests.<Clase>`. Cada nombre se confirma por `git grep "class <Clase>"` en la emisión | la de la tabla siguiente | false |
| UI | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 601 | false |
| UI | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 | false |

Las clases de la fila por clase, con su cuenta en la base:

| Clase | Cuenta | Clase | Cuenta |
|---|---|---|---|
| `CallerOwnedFacadeGuardTests` | 48 | `CallerOwnedTransactionGuardTests` | 15 |
| `RackSiblingRedrawUnitsTests` | 2 | `DebugFaultInjectionGuardTests` | 1 |
| `SiblingRedrawSeamGuardTests` | 2 | `RackSiblingMembershipTests` | 12 |
| `RackSiblingRedrawPlanTests` | 2 | `RackSiblingRedrawRunTests` | 6 |
| `PushBackClassicCharacterizationPluginGuardTests` | 10 | `PushBackPluginSourceGuardTests` | 11 |
| `PushBackRoundTripSourceGuardTests` | 24 | `RackProjectionCommandGuardTests` | 34 |
| `NamespaceFolderGuardTests` | 7 | `CustomPropertiesEnvelopeGuardTests` | 22 |
| `RackUnitsGuardSourceTests` | 18 | `I55G15Auth15ConsumerTests` | 5 |
| `SingleScanInsertGuardTests` (v4) | 5 | `RackViewBatchDriverGuardTests` (v4) | 4 |
| `G16FirstViewCorrectiveTests` (v4) | 61 | `RackSiblingCustomPropertiesGateTests` (v4) | 2 |
| `RackEnvelopeIdProbeTests` (v4) | 5 | | |

**Nota sobre `CallerOwnedFacadeGuardTests`.** Está en la fila por clase con `ExpectRed` false y también en las pruebas modificadas, porque la extensión de `Facades` ya cumple en la base. Si su cuenta crece, `MinSelected` sigue siendo la de la base.

**Invariantes:**
- **01:** la relectura de MUTATE es la autoridad. Ningún resultado de PREPARE autoriza escribir.
- **02:** con una diferencia, NotAppliedConflict y cero `WriteUnit`.
- **03:** todo o nada, con la clasificación por frontera de la tabla del D-T4B-02. **La corrida es total y nunca lanza.** La costura de I-55 no cambia y sus pruebas siguen en verde.
- **04:** una transacción poseída. La relectura no abre transacción, no bloquea y no confirma. Una `TopTransaction` ajena da `TOP_TRANSACTION_OPEN`. Los objetos de AutoCAD viven solo en los ámbitos.
- **05:** la importación va antes de la transacción. C-F4-05 y las capas bloqueadas se comprueban antes de importar y en MUTATE. El origen se elige en puro, en la corrida. El residuo OM-5 queda declarado.
- **06:** POST sale de la relectura, se compara byte a byte y sin normalizar. Dirty pasa a false solo desde el estado verificado.
- **07:** nunca se borra una vista; solo se aplica lo que posee el piloto.
- **08:** el plan es inmutable y de valores.
- **09:** la disciplina de la ranura de D-T4B-07.
- **10:** D-T4B-06, rama por rama.
- **11:** M-1, M-5, M-6 y M-10.
- **12:** sin UI, contexto de comando, `async` ni `LockDocument` en el escritor ni en las carpetas Workspace.
- **13:** los caminos clásicos, Insertar y RACKPROYECTAR no cambian.
  - `git diff --name-only BaseSha..CurrentSha` no toca ningún archivo de los caminos clásicos ni de los llamadores del barrido (H-20), que están en el prohibido.
  - Las pruebas de T1B y de I-55 siguen en verde.
  - Que T1B siga en verde es necesario, pero no protege el movimiento de `Traverse` (H-19): lo protege INV-17.
- **14:** solo los cambios de guarda declarados, exactos y con su autoprueba en negativo.
- **15:** el RED con cuerpos inertes también en las fábricas, igualdades y reenvíos (M-11). Las pruebas existentes que cambian van en el RED. Ninguna prueba necesita AutoCAD. Ningún archivo nuevo construye ni compone un `RackEmbedDocument`, ni añade `[CommandMethod(` o `Insunits` (D-T4B-09).
- **16, la purga, categoría (b)** (§2.4):
  - corre una vez, en `Post`, con `PurgeAfterCommit` y la unión de las `staleDefinitions`;
  - después de que `Seal` vuelva, fuera del ámbito de mutación y con `TopTransaction == null`. Si no se cumple, `Post` no hace nada más y la corrida da AppliedPostRereadFailed(`TOP_TRANSACTION_OPEN`): es imposible por construcción y no hay POST normal;
  - nunca en `WriteUnit`, en `Seal`, dentro del ámbito de mutación ni entre dos `WriteUnit`;
  - no hay purga en NotApplied ni en CommitThrew;
  - no da aviso, fallo ni estado, y su fallo solo va al registro;
  - la relectura de POST va después;
  - un diseño que la comparta con la transacción authored, la anide o la intercale es STOP (C08).
- **17, el movimiento de `Traverse`,** es una refactorización sin cambio de comportamiento. Lo fijan:
  - las guardas exactas de D-T4B-04: el núcleo idéntico al literal de `c3ae1c97` con su única sustitución, la normalización declarada y las autopruebas negativas; el envoltorio exacto, con el núcleo `internal static`; `Capture` y `CaptureDrawing` exactos; el censo de dos llamadas; y el resto del archivo congelado, `AddLayer` incluido;
  - las clases de regresión del barrido, en verde;
  - y Smoke-2 de los cuatro llamadores.
- **18, la contención de las pruebas y de las corridas locales:**
  - ninguna prueba nueva escribe fuera de una ubicación temporal aislada ni usa la configuración real del usuario;
  - ninguna deja vivo un modal, un dispatcher ni estado persistente fuera de su sandbox;
  - las suites completas de UI solo corren en la CI; en local, solo los filtros de UI de `RequiredTests`;
  - ninguna sonda temporal ejerce con entradas válidas un camino con diálogo o escritura (F4-E59).

**`ExpectedEvidence`:**
- todo lo de F4-T4A2: contrato, delegación, RED y GREEN con su push, CI exacta y entrega;
- además, en local:
  - el build Release de la solución, sin avisos;
  - **el build Debug del Plugin** con la orden del D-T4B-03, que lleva la salida a una carpeta temporal (sin avisos nuevos respecto a la base en Debug, IR I-5, RR R-M-14);
  - el mapa evidencia → invariante;
  - qué suites se corrieron en local (INV-18);
  - opcional: el `git diff --color-moved` del movimiento de `Traverse` (D-T4B-04 (7)).

**Paradas:**
- **C01:** haría falta tocar la UI, el contexto de comando, `async` o `LockDocument` en el escritor.
- **C02:** haría falta tocar la costura de I-55 (H-14), las fachadas, los caminos clásicos, `RackPilotIntentAdapter.cs`, `IWorkspacePilotEditPort` o sus implementadores.
- **C03:** un cambio de guarda no declarado en el D-T4B-07 o el D-T4B-09.
- **C04:** el plan necesitaría llevar un objeto del host, o un objeto de AutoCAD tendría que vivir fuera de un ámbito.
- **C05:** la comparación de POST necesitaría normalizar algo.
- **C06:** las precondiciones de emisión dejan de cumplirse.
- **C07:** la regla de fidelidad de la planificación (F4-E31 y F4-E43).
- **C08:** la purga tendría que compartir la transacción authored, anidarse en ella o intercalarse (categoría (a)), o su fallo tendría que cambiar el desenlace.
- **C09:** el núcleo movido no puede ser idéntico al literal congelado salvo su sustitución declarada; o haría falta cualquier otro cambio en `RackSiblingScan.cs` fuera de los dos miembros `Traverse` y el núcleo; o el movimiento necesitaría cambiar de forma no declarada un llamador del barrido o una guarda de H-19.

## 7. Smoke-2

T4B no lo ejecuta, porque necesita T4C. Le pasa estas comprobaciones:
- que los bytes y los handles se conservan (R-T4-04);
- una transacción y `TopTransaction == null`;
- la equivalencia de las tres lecturas;
- R-09;
- la agrupación de Undo de la importación;
- S2-R-10 con `SEAL=before` y con `SEAL=after`;
- **una capa bloqueada** en un miembro del piloto: NotAppliedPreflight, sin residuo de importación (RR R-I-4);
- **el movimiento de `Traverse`:**
  - Insertar de Selectivo (A);
  - el insertar desde RACKEDITAR Push Back (B), que representa por construcción a Dinámico, Cantilever y Cabecera;
  - RACKPROYECTAR (C);
  - el lector del piloto (D) (D-T4B-04);
- **la purga:** tras un Actualizar, la vista queda sin definiciones obsoletas. El residuo de una purga que falla queda **declarado y no observado**: no hay inyección para ella, y una inyección en el escritor saldría de `Post` como AppliedPostRereadFailed, contra INV-16.

## 8. Lo que queda para F4-T4C

- R-T4-05 a R-T4-10;
- la operación del puerto `IWorkspacePilotEditPort` con sus cinco implementadores, y **la puerta de reposo** antes de entrar en el contexto de comando (RR R-I-1);
- **el cableado de la instancia del puerto de actualización** (RR R-M-12). El núcleo no puede tener un campo de `Systems`, el puerto del panel no puede llevar tipos del piloto, el soporte no puede nombrar el piloto y el adaptador del controlador reenvía literalmente. **T4C necesitará un cambio de guarda declarado**, por ejemplo la opción B-2 (b) de la revisión de la v1 (un único campo con su autoprueba);
- la llamada de la UI;
- el diagnóstico de `TryCheckNow`;
- IC-08;
- C2-M-3 a C2-M-7 donde toque.

## 9. Interpretaciones declaradas que se comunican al Owner (decisiones para la emisión)

**Lo ya decidido por el Owner** (`owner-f4t4-disposition-reverify-t1b-t4b.md` y `owner-f4t4b-traverse-decision.md`):
- **I-1:** «T4B = write implementation/semantics; T4C = execution-context integration» y «T4C must NOT implement a second authored mutation algorithm».
- **I-4:** «USE A PILOT-SPECIFIC WRITE PATH». OV-RED no valida la vía nueva, y Smoke-2 es su primera validación física.
- **I-5, solo en lo que toca a la purga:** «LOG ONLY unless…», con la categoría probada en el §2.4, que es la (b). La política de avisos de «aplicado con avisos» sigue siendo una interpretación de la sesión.
- **I-6:** autorizado como refactorización, con la condición (3) sustituida por la decisión del Owner (D-T4B-04).

**Compatibles con esas decisiones, aunque no decididas de forma expresa:** I-2 e I-11. Que T4C envuelva o llame a T4B encaja con `TryUpdatePilot` como método del núcleo y con la puerta propia.

**Quedan para la orden de emisión:** I-2, I-3, el resto de I-5 (los avisos), I-7, I-8, I-9, I-10, I-11, I-12, I-13, I-14 e I-15.

| # | Interpretación | Alternativa |
|---|---|---|
| I-1 | **«Única escritora»:** T4B crea la escritura authored, y T4C envuelve el **mismo** escritor con el contexto de comando y el bloqueo, sin una clase nueva de escritura | Que T4C no tenga escrituras propias |
| I-2 | `TryUpdatePilot` es **un método del núcleo**, sin llamador de producto en T4B, para que la ranura sea oráculo de T4B. La operación del puerto va en T4C. **T4C necesitará un cambio de guarda declarado para cablear la instancia del puerto** (§8, RR R-M-12) | La operación en el puerto ya en T4B, con los cinco implementadores y UI.Tests en el alcance |
| I-3 | El diagnóstico de `TryCheckNow` (`A2P:657`, F4-E55) **pasa a T4C**. Toca el VM y rompe M-3 o M-2. Esto **añade «Comprobar ahora» a T4C**, más allá de su fila (`T4P:173`) (IR M-13) | Mantenerlo en T4B, añadiendo la UI al alcance |
| I-4 | **La vía (IR I-8):** el piloto **no usa la vía implementable declarada de D-13** (la costura de I-55 con OV-RED). Usa una vía propia con la mecánica de los pasos 3 a 5, que D-13 admite («la mecánica no se congela»). **OV-RED no se transfiere**, y la primera prueba física del escritor del piloto es Smoke-2 (R-09). INV-30 se resuelve sin tocar Insertar | (a) la costura, con una frontera de `Commit()` aditiva en `SiblingRedrawTransaction.cs`, conservando sus cuentas, y un cambio declarado de `SiblingRedrawSeamGuardTests`. Reaprovecha OV-RED, pero toca el camino de Insertar. (b) la vía propia reutilizando el tipo `SiblingRedrawUnit` sin tocar su archivo |
| I-5 | **«Aplicado con avisos»:** las piezas faltantes (por unidad), un nombre distinto tras `SyncName` y un `Regen` que lanza dan avisos visibles, y los decide la corrida a partir de los hechos de `Post`. **Los fallos de la purga solo van al registro**, porque la purga es categoría (b) con su orden (§2.4, INV-16) | Tocar `LateralHeaderDrawer` para que informe, fuera del alcance |
| I-6 | **El núcleo de `Traverse`** es un movimiento sin cambio de comportamiento de un camino que el Owner validó (I-55, OV-RED). Lo fijan las guardas exactas de D-T4B-04 (1) a (4b) y Smoke-2, por la decisión del Owner | Duplicar el recorrido, lo que rompe las cuentas y la equivalencia por construcción |
| I-7 | **`TopTransaction`:** se rechaza una ajena al entrar (`TOP_TRANSACTION_OPEN`) y, al salir, se comprueba registrando en el log, sin `Debug.Assert` | `Debug.Assert`, que terminaría AutoCAD |
| I-8 | **El `PushBackSystem`** se reconstruye con una ayuda pura de `Pilot/` a partir del texto de diseño del plan; el corte, con `RackViewCodec`. **Hay dos catálogos** (el de la lectura previa y el de `PrepareRedraw`), y se declaran | Ampliar el plan |
| I-9 | **El origen de MUTATE** lo elige **la corrida, en puro**, por el texto exacto del origen del borrador (la regla Build (b)). Cero da `NoOriginReason` y varios `SeveralOriginsReason`, los dos como Preflight. El documento se elige por `AuthorizedDocumentNativeId`. **El origen de POST es `plan.OriginKey`.** Es distinto de la elección de «Comprobar ahora», y R-T4-01 exige solo la equivalencia de la StaleBase. **Consecuencia** (RR R-M-11): la promesa de D-T4A2-06 (`A2P:349`) solo se cumple para las causas que no dependen del origen. Las reglas 2, 5 y 6 del lector pueden dejar un diagnóstico persistente en «Comprobar ahora» mientras Actualizar admite | Leer por el lector del host (D-1 6 → 7, `PilotForms` y el catálogo, que `EditIntentReadResult` no lleva) |
| I-10 | **El respaldo de `CompleteApply`:** si el cálculo del estado lanza en una rama NotApplied, el borrador queda en **Error** con su causa (`ClosedInError`), nunca en «aplicado» | Dejar la rama tal cual y que la excepción escape, que es lo de hoy y lo que F4-E25 (4) pide corregir |
| I-11 | **La puerta de `TryUpdatePilot`** (RR R-I-1, opción a): la propia, **sin reposo ni lector** (el documento activo, el modal y `Authorize`). **La puerta de reposo pasa a la operación del puerto de T4C**, antes del contexto de comando. Así T4C llama a `TryUpdatePilot` dentro del callback sin reescribir el oráculo de la ranura | (b) `PassGate` en T4B, con un cambio en T4C que reescribe el método |
| I-12 | **«`Seal` volvió y su `Dispose` lanzó»** va a POST, como la fila «`Commit()` retornó» de D-13 (`FRZ:527`), y el `Dispose` se convierte en un aviso. No va a CommitThrew, que daría un Conflicto falso con la escritura ya hecha (RR R-I-3) | CommitThrew |
| I-13 | **Las capas bloqueadas, en T4B**: antes de importar y en MUTATE, con `LOCKED_LAYER`, como pide el paso 3 de D-13 (RR R-I-4) | Pasarlas a T4C, que deja un residuo OM-5 evitable |
| I-14 | **«Varios orígenes»** da Preflight con `SeveralOriginsReason`, y no el NotAdmitted u OriginMissing engañoso del lector (RR R-I-5) | NotAdmitted |
| I-15 | **El orden de `Post`** es purga → `SyncName`, el del clásico y el de I-55. R-T4-04 (`T4P:540`) y `FRZ:519` lo enumeran al revés, pero `FRZ:519` remite a «la autoridad existente» y `FRZ:506` no congela la mecánica | `SyncName` → purga, como la letra de R-T4-04 |

## 10. Disposición de las revisiones

### 10.1 La revisión de la v1 (NO EMITIBLE, 4/8/14)

| Hallazgo | Disposición |
|---|---|
| B-1. `Operations` 15 → 16 | **Aceptado.** `TryUpdatePilot` es un método del núcleo. La operación del puerto va a T4C, y se declaran los censos exactos (D-T4B-07) |
| B-2. El puerto como campo | **Aceptado, opción (a):** el puerto es un parámetro |
| B-3. Granularidad y estado entre llamadas | **Aceptado.** Ámbitos por corrida al estilo de RACKPROYECTAR; la corrida pura decide y clasifica; los objetos solo viven en los ámbitos; la tabla de excepciones; `Seal` en lugar de `Commit` |
| B-4. `ReadForPrepare` sin entradas | **Aceptado.** `Begin(request)` de valores, con la regla del origen por texto exacto y el documento autorizado; §9 I-9 |
| I-1. Avisos no observables | **Aceptado.** Las faltantes por `WriteUnit`, `SyncName` verificado por relectura y el `Regen`; la purga como limitación (§9 I-5) |
| I-2. Unknown(PostReread) en todas las ramas | **Aceptado.** Comportamiento por rama, con respaldo Error en NotApplied (§9 I-10) y una prueba por fila |
| I-3. Alcance incoherente | **Aceptado.** <br>• Sin `WorkspaceController`. <br>• La costura por ruta exacta y `RackSiblingScan` permitido. <br>• La clausura por grupos. <br>• Sin condicionales. <br>• El mapa de M-1 lo corrige la sesión. <br>• La política de bloqueo del escritor. <br>• UI.Tests, los `.csproj` y Fixtures en el prohibido. <br>• El soporte de pruebas en el permitido |
| I-4. `RequiredTests` | **Aceptado.** La guarda de INV-39 en el filtro 1 y los filtros de regresión con `ExpectRed` false, en Core y en UI |
| I-5. Debug sin compilar en la CI | **Aceptado.** `[Conditional("DEBUG")]` y el build Debug del Plugin como evidencia |
| I-6. El movimiento de `Traverse` | **Aceptado.** Declarado (§9 I-6), con su guarda y en Smoke-2 |
| I-7. Transacción ajena | **Aceptado.** `TOP_TRANSACTION_OPEN` |
| I-8. Abandono de la vía de D-13 | **Aceptado.** §9 I-4 reescrito con las alternativas |
| M-1 a M-14 | **Aceptados:** <br>• M-1: `PushBackPilotDraftState`; <br>• M-2: la correspondencia bajo Applying y el Conflicto sin causa aceptado; <br>• M-3: C-F4-05 antes de importar; <br>• M-4: dos catálogos declarados; <br>• M-5: la ayuda pura y el nombre base del plan; <br>• M-6: `EditUpdateOutcome` con su censo; <br>• M-7: IC-08 y las disposiciones antiguas, a T4C; <br>• M-8: sin literales nuevos; <br>• M-9: `SEAL=before/after` con nombres; <br>• M-10: `NotePilotEdit` y `ChangesADraft`; <br>• M-11: guardas por cuerpo; <br>• M-12: la `Theory` dividida y los cambios en el RED; <br>• M-13: el §9 I-3; <br>• M-14: H-06 precisado |

### 10.2 La re-revisión de la v2 (EMITIBLE TRAS CORRECCIONES, 0/7/14)

**De la v1:**
- **Resueltos:** B-1, y B-2 para T4B.
- **Parciales:** B-3 y B-4. El puerto por ámbitos es implementable y la corrida decide sobre valores reales; faltaban el origen en puro y su totalidad.
- **Parciales también:** I-4 e I-7. El resto, resuelto.

**Lo que confirmó la re-revisión:**
- ninguna guarda prohíbe nombrar `Systems` en el núcleo fuera de los campos;
- `[Conditional("DEBUG")]` no choca con `DebugFaultInjectionGuardTests`;
- `input.InitialDraftState.Origin` existe;
- el respaldo Error es alcanzable sin romper R-3 ni M-4.

| Hallazgo | Disposición en la v3 |
|---|---|
| R-I-1. La puerta de reposo frente a R-T4-06 | **Aceptado, opción (a):** la puerta propia, sin reposo ni lector, y el reposo en T4C (D-T4B-07; §9 I-11) |
| R-I-2. `EditUpdateOutcome` sin forma | **Aceptado.** La tabla de 19 estados en orden, con `Unavailable` por defecto, el `struct`, la prueba de nombres y orden, y las diferencias leídas después de `CompleteApply` |
| R-I-3. Corrida no total | **Aceptado.** La corrida es total: la marca de `Seal` devuelto; las excepciones posteriores van a POST o a AppliedPostRereadFailed; los `Dispose` se convierten en avisos; el host pasa siempre el resultado de la corrida; §9 I-12 |
| R-I-4. Las capas bloqueadas | **Aceptado, en T4B** (§9 I-13): `LockedLayerKeys` y `LOCKED_LAYER` antes de importar y en MUTATE, con Smoke-2 |
| R-I-5. El origen y los supervivientes en el escritor | **Aceptado.** El puerto solo devuelve valores; la corrida elige el origen, cuenta los supervivientes y comprueba las capas; «varios» es Preflight (§9 I-14); pruebas con 0, 1 y varios |
| R-I-6. El origen de POST | **Aceptado.** `plan.OriginKey`, con la prueba «texto del origen cambiado → Applied» |
| R-I-7. El filtro 2 | **Aceptado.** Una fila por clase, `FullyQualifiedName~RackCad.Tests.<Clase>`, con los nombres exactos que dio la re-revisión |
| R-M-1 a R-M-14 | **Aceptados:** <br>• R-M-1: `ClosedDetached`; <br>• R-M-2: el bloque diferido byte a byte y la forma del respaldo Error; <br>• R-M-3: `TryUpdatePilot` al final, con `_knownClean.Remove(request.Session)` literal; <br>• R-M-4: textos de las salidas tempranas, sin `ChosenReference`, `Witness` ni `HasWitnessWithoutKey`; <br>• R-M-5: la excepción de firma declarada; <br>• R-M-6: las constantes de las causas y la comprobación previa que solo para en tres tipos; <br>• R-M-7: los campos exactos y `PushBackPilotMutationStart`; <br>• R-M-8: comentarios sin `Transaction`; <br>• R-M-9: tres clases en el escritor; <br>• R-M-10: el nombre esperado saneado y sin distinguir mayúsculas; <br>• R-M-11: §9 I-9; <br>• R-M-12: el cableado de T4C (§8 y §9 I-2); <br>• R-M-13: INV-11 trazado y `WriteUnit(key, envelopeText)`; <br>• R-M-14: la orden del build Debug con salida temporal |

### 10.3 La revalidación tras T1B (v4)

**Las mediciones,** en `7c29a5e5` (M4 y M5), con solo lectura:

| Hallazgo | Disposición en la v4 |
|---|---|
| M4: T1B no protege el comportamiento relevante para el movimiento de `Traverse` (la condición (3) de la disposición), y ninguna prueba ejecuta el barrido | **Decisión del Owner, «Guardas exactas en T4B»:** D-T4B-04 (1) a (7), INV-17 y C09 |
| M4: la firma del núcleo nombraba solo `(Transaction, Database)`, y quedaban sin destino `:103-110` y `:166-169` | La firma lleva `selectedDefinition` y `eraseByKind`; las líneas van al núcleo (D-T4B-04) |
| M4: Smoke-2 no nombraba los insertar de Dinámico, Cantilever y Cabecera, ni D | Dinámico, Cantilever y Cabecera se declaran cubiertos por construcción con el de Push Back; D se nombra (§7) |
| M5: la purga es categoría (b) solo con su orden (en su propia transacción, después del commit y sin `TopTransaction`) | §2.4, el `Post` de D-T4B-03, INV-16, las guardas de D-T4B-09 y C08 |
| M5: el orden de `Post` (`SyncName` → purga) difería del clásico (purga → `SyncName`) | Alineado con el clásico y declarado (§9 I-15) |
| El cierre de T1B manda M-02 a M-09 a la reconciliación previa a T4B | M-02 y M-03 van a H-17 y H-18. M-04 a M-09 son de las pruebas de UI de T1B y no aplican a T4B: van a F5 |
| T1B: las fachadas congeladas (CT-27), las guardas débiles de orden (M-02 y M-03) y la línea base | H-15 a H-18, INV-13 y `RequiredTests` (55 y 10) |
| F4-E59: las reglas de las corridas locales | H-22 e INV-18 |

**Resultado de la v4:**
- la validación determinista `validate_t4b_package.py`: PASS 80/80 (`validation-v4.json`);
- la comprobación independiente (`independent-check-v4.md`): **EMITIBLE TRAS CORRECCIONES (1/4/13)**. Confirma que H-15 a H-26 son correctos, que la categoría (b) está probada, que el movimiento es compatible con todas las guardas existentes y que ningún cambio de T4B rompe T1B.

### 10.4 La comprobación de la v4 (EMITIBLE TRAS CORRECCIONES, 1/4/13)

| Hallazgo | Disposición en la v5 |
|---|---|
| B-1. El núcleo `private` y la llamada desde el escritor se excluyen | **Aceptado.** `internal static`, fijado por la guarda (2) (D-T4B-04) |
| I-1. El literal mal definido | **Aceptado.** El literal es `:103-110` + `:115-161` + `:166-169`, con una única sustitución en `:115`; las líneas `:111-114` y `:162-165` van al envoltorio; la normalización es fija (D-T4B-04 (1)) |
| I-2. El resto de `RackSiblingScan.cs` sin guarda (`AddLayer`) | **Aceptado.** D-T4B-04 (4b), el archivo congelado fuera de la región; C09 se amplía |
| I-3. El `Post` del escritor interpretaba POST | **Aceptado, sin excepción para el Owner.** `Post(PushBackPilotPostRequest)` recibe los nombres deseados de la corrida y devuelve hechos (`NamesByKey`, leídos en una transacción de solo lectura propia; `RegenFailure`; `ForeignScopeOpenAtPost`). Los avisos y el nombre esperado los decide la corrida (D-T4B-02 y D-T4B-03; D-T4B-09) |
| I-4. El aborto que falla tras un `WriteUnit` | **Aceptado.** Una fila nueva con CommitThrew(`ABORT_FAILED`), es decir, Unknown; sin `WriteUnit`, NotAppliedFailure; y la corrida separa las excepciones en un `finally` propio (D-T4B-02 (4) y (6)) |
| m-1 | **Aceptado.** Declarado en D-T4B-03 y como §9 I-15 |
| m-2 | **Aceptado.** La guarda de orden del cuerpo de `Post` (D-T4B-09) |
| m-3 | **Aceptado.** Con `TopTransaction` en `Post`, AppliedPostRereadFailed, imposible por construcción (D-T4B-02, INV-16) |
| m-4 | **Aceptado.** El residuo, declarado y no observado (§7) |
| m-5 | **Aceptado.** El movimiento va en el GREEN, y se declaran las guardas que pasan en el RED (D-T4B-04 (8)) |
| m-6 | **Aceptado.** H-23 y H-25 precisados |
| m-7 | **Aceptado.** F4-E26 son los tres caminos de lectura del piloto |
| m-8 | **Aceptado.** El prefijo de clase y el suelo de 2923 |
| m-9 | **Aceptado.** El diff entre las bases, precisado |
| m-10 | **Aceptado.** I-5 e I-6 alineados, y I-5 decidido solo en la purga |
| m-11 | **Aceptado.** Las trampas de censo, en D-T4B-09 y en INV-15 |
| m-12 | **Aceptado.** La transacción como campo del ámbito, que libera la corrida |
| m-13 | **Aceptado.** Una fila en el §10.3 |

**Resultado de la v5:**
- la validación determinista: PASS 94/94 (`validation-v5.json`);
- la comprobación dirigida (`independent-check-v5.md`): **EMITIBLE TRAS CORRECCIONES (0/2/6)**. B-1, I-1, I-2 e I-3 están resueltos, y 11 de los 13 menores también.

### 10.5 La comprobación dirigida de la v5 (EMITIBLE TRAS CORRECCIONES, 0/2/6)

| Hallazgo | Disposición en la v6 |
|---|---|
| N-1. `TopTransactionAtPost` choca con la guarda de pureza (`Transaction` sin `\b`) | **Aceptado.** El campo pasa a llamarse `ForeignScopeOpenAtPost` |
| N-2. El ámbito exterior en un `using` puede tapar una clasificación decidida | **Aceptado.** Los dos ámbitos se liberan en `finally` propios; la fila del `Dispose` exterior y la precedencia (D-T4B-02 (2) y (6)) |
| N-3. (d) seguía diciendo `using (var m …)` | **Aceptado.** `m = start.Scope`, liberado en el `finally` de (6) |
| N-4. La precedencia entre filas | **Aceptado.** Se conserva la clasificación ya decidida; `ABORT_FAILED` solo desde un NotAppliedFailure con algún `WriteUnit` empezado (D-T4B-02 (6)) |
| N-5. La regla por vista de `DesiredNames` y «sin nombre, sin aviso» | **Aceptado.** La regla del clásico por vista, en la corrida, con `IsComposite` de la ayuda pura y el corte de `RackViewCodec`; sin nombre deseado no hay `SyncName` ni comparación; una prueba por vista y una en blanco (D-T4B-03) |
| N-6. La región de (4b) | **Aceptado.** Los dos miembros localizados por firma, con el núcleo justo después de `Traverse` |
| El resto de m-5 | **Aceptado.** D-T4B-04 (8) declara también las guardas de D-T4B-09 que pasan y las que fallan en el RED |
| El resto de m-7 | **Aceptado.** Los tres caminos de F4-E26, tal como los define `T4P:530` |
| La precisión opcional de I-1 | **Aceptada.** El cuerpo sin las llaves del método |

**Resultado de la v6:**
- la validación determinista: PASS 99/99 (`validation-v6.json`);
- la comprobación de las filas (`independent-check-v6.md`): **EMITIBLE (0/0/3)**.

### 10.6 La comprobación de la v6 (EMITIBLE, 0/0/3)

| Hallazgo | Disposición en la v7 |
|---|---|
| N-7. El identificador de la constante `TOP_TRANSACTION_OPEN` | **Aceptado.** `ForeignScopeOpenCause`, sin la palabra `Transaction` |
| N-8. «Se conserva y el `Dispose` añade su causa» no vale para todas las formas | **Aceptado.** Dos salvedades declaradas: el Conflicto sin causa descarta la excepción del `Dispose`, y después de `Post`, `Applied` pasa a `AppliedWithWarnings` (D-T4B-02 (6)) |
| N-9. Cómo lee `ReadForPrepare`, y ninguna guarda contra un barrido propio del escritor | **Aceptado.** `ReadForPrepare` usa `CaptureDrawing` más el catálogo (D-T4B-03), y una guarda prohíbe `RackBlockData.Read`, `BlockTableId` y `GetBlockReferenceIds` en el escritor (D-T4B-09) |

El paquete se emite por la autorización del Owner (F4-E60), con el §9 aprobado.
