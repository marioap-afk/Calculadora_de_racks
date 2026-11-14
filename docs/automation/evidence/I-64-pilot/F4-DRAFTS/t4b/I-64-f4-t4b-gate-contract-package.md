# I-64 — Paquete del contrato de F4-T4B (MUTATE y POST del Actualizar del piloto)

**Estado:** v3. La prepara la sesión principal el 2026-11-14. No declara GATE PASS y **no se emite**: solo se prepara y se revisa (`owner-f4t4a2-overnight-decisions.md`, pregunta 4: «Solo paquetes»).
- **v1:** revisión adversarial independiente, **NO EMITIBLE (4/8/14)** (`independent-review.md`; la v1 se conserva en `_package-v1.md`).
- **v2:** rediseñó el puerto y la operación del host. Su re-revisión focalizada dio **EMITIBLE TRAS CORRECCIONES (0/7/14)** (`independent-rereview.md`, citado como RR).
- **v3:** aplica todos los hallazgos de esa re-revisión (§10.2). Son texto y decisiones, sin rediseño.
- Antes de emitir conviene una comprobación dirigida de la v3, o la que pida el Owner si cambia alguna de las decisiones del §9 (I-11 a I-14).

**Precondiciones de emisión, que hoy no se cumplen:**
- F4-T4A2 cerrada; hoy está en STOP `4b2a`, a la espera del Owner.
- F4-T1B emitida, ejecutada y cerrada (R-T4-12); hoy su paquete está en v4.
- Una orden del Owner que autorice la emisión y decida el §9.

**Código base:** `c3ae1c97`, con el producto de T4A2 en `ae0a7b40`. Si T4A2 o T1B cambian producto, se vuelve a medir (`T4P:177`).

**Mapas:** `map-1-plugin-write-path.md` (M1), `map-2-core-apply-plan.md` (M2) y `map-3-carries-guards-rules.md` (M3). La revisión se cita como IR.

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
- **El paquete de F4-T1B**, cuando se emita: su clausura y CT-24 a CT-27.

## 2. Hechos medidos en `c3ae1c97`

Los H-01 a H-13 de la v1 se sostienen (IR, «Lo comprobado»). Aquí van resumidos, con las correcciones y los hechos que añadió la revisión.

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
    PushBackPilotScanValues Post()

IPushBackPilotMutationScope : IDisposable      // Dispose sin Seal = abortar
    PushBackPilotScanValues Reread()
    PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText)
    void Seal()
```

**Los tipos de valor**, con la lista exacta de campos (RR R-M-7):

| Tipo | Campos |
|---|---|
| `PushBackPilotUpdateRequest` | `AuthorizedDocumentNativeId` (`long`, el `_probe.ActiveDocumentNativeId` que validó la puerta) y `RackId` (`string`) |
| `PushBackPilotScanValues` | `DocumentMatches` (`bool`); los hechos y los textos crudos del barrido sin origen, con los mismos tipos que consume `BuildStaleBase` y `RackSiblingMember`; `LockedLayerKeys` (las claves `Mutable` con capa bloqueada); el `Catalog` (solo en `ReadForPrepare`); `PreparedBlockMatches` (por clave, si el bloque preparado es el releído; solo en `Reread`); `Warnings` (solo en `Post`) |
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
- `TOP_TRANSACTION_OPEN`;
- `LOCKED_LAYER`;
- y las constantes que ya existen, `SeveralOriginsReason` y `NoOriginReason` (`PushBackPilotUpdatePlan.cs:271-273`).

**1. Antes de la ranura** (lo llama el host): `Prepare(From(draft), null, null)`.
- **Solo para** con NotApplicable, PendingText y NothingToApply.
- Cualquier otro resultado sigue adelante. Por ejemplo, el `Invalid(MissingInputReason)` de un Dirty sin pendiente es normal sin catálogo (RR R-M-6).

**2. Bajo Applying**, `using (var scope = port.Begin(request))`:

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

**(d) `BeginMutation()`.** Con un `RejectionCode`: NotAppliedPreflight(código). Si no, `using (var m = start.Scope)`:
- `Reread()` → la elección del origen otra vez, en puro, que tiene que dar la misma clave que el plan (si no, NotAppliedConflict) → `BuildStaleBase` → `DraftStaleBase.Create(plan.ExpectedBase, plan.ExpectedVariableRegistryText).CompareWith(...)`. Con una diferencia: **NotAppliedConflict y cero `WriteUnit`** (INV-06);
- **el destino:** las claves del plan son iguales a las `Mutable` releídas y todos los `PreparedBlockMatches` son verdaderos. Si no, NotAppliedPreflight;
- **la autoridad de MUTATE:** los supervivientes y las capas, otra vez;
- un `WriteUnit(key, plan.Envelope(key))` por miembro. Si uno lanza: NotAppliedFailure, sin `Seal` (INV-12);
- **`Seal()` en su propio `try`**, y la marca `sealedReturned = true` cuando vuelve.

**3. POST.** Siempre que `sealedReturned` es verdadero:
- **El origen de POST es `plan.OriginKey`** (RR R-I-6): la clave de la definición, que no cambia, aunque su texto ya sea el del plan.
- La corrida construye la relectura de POST con esa clave a partir de los valores de `Post()`.
- Después, el comparador del D-T4B-05.

**4. La clasificación por frontera, total:**

| Qué pasa | Resultado |
|---|---|
| Una excepción antes de `Seal`: en `Begin`, `ReadForPrepare`, `PrepareUnits`, `BeginMutation`, `Reread` o `WriteUnit` | NotAppliedFailure con su causa |
| Una excepción **en** `Seal` | **CommitThrew** |
| `Seal` vuelve y el `Dispose` de `m` lanza | **POST** (la fila «`Commit()` retornó» de D-13, `FRZ:527`), y el `Dispose` se convierte en un aviso (RR R-I-3) |
| `Post()` lanza, o falta su relectura | AppliedPostRereadFailed(causa, avisos) |
| El comparador o el constructor de un resultado lanzan después de `Seal` | AppliedPostRereadFailed(causa, avisos) |
| El `Dispose` del ámbito exterior lanza después de `Post` | Un aviso. El resultado no cambia |

**5. Los avisos:** las piezas faltantes de cada `WriteUnit`, los avisos de `Post` y los de los `Dispose`. Llegan siempre al resultado (D-T4B-06).

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

**Lo que hace el escritor ya no lo decide:** solo comprueba el documento (`document.UnmanagedObject.ToInt64() == AuthorizedDocumentNativeId`) y lee. El origen, los supervivientes y las capas los decide la corrida (D-T4B-02).

**El bloqueo:** el escritor **no toma `LockDocument`**. El único `using LockDocument` alrededor de PREPARE, MUTATE y POST es del adaptador de T4C (R-T4-06). Los bloqueos anidados de `CaptureDrawing`, `SyncName` y la purga ya existen (IR I-3).

**Lo que hace cada operación:**
- **`PrepareUnits`:** las `*DrawService.PrepareRedraw` de las fachadas, que importan antes de cualquier transacción (R-T4-03).
  - El sistema sale de la ayuda pura; el corte, de `RackViewCodec`.
  - El catálogo de `PrepareRedraw` es el suyo, distinto del de la lectura previa: se declara (IR M-4).
- **`BeginMutation`:** `TopTransaction == null`, o el rechazo. Después `StartTransaction` en `using`.
- **`Reread`:** la lectura caller-owned (D-T4B-04), con la identidad de la transacción comparada por `UnmanagedObject`.
- **`WriteUnit(key, envelopeText)`:** comprueba que `envelopeText` es el del plan para esa clave. Después, `RedrawInTransaction` de la fachada de esa clave (INV-10). Recoge `staleDefinitions` y `MissingInstances`.
- **`Seal`:** `transaction.Commit()`.
- **`Post`:**
  - `SyncName` con el nombre base del `Name` del sobre del plan y `RackViewBaseName` (IR M-5);
  - la purga con las `staleDefinitions`;
  - un `ApplyRegen`;
  - la relectura con `CaptureDrawing` sin cambios.
  - **Avisos** (I-1): un nombre releído distinto del esperado tras `SyncName`, y un `Regen` que lanza. La purga no informa, y queda declarada como limitación de la fila de D-13: solo va al registro (§9 I-5).
  - **El nombre esperado** (RR R-M-10) es `BlockNaming.SanitizeBlockName(RackViewBaseName.Linked…(base))` para la vista de cada clave, comparado con `OrdinalIgnoreCase`, como en `SyncName`. Con un `Name` en blanco no hay `SyncName` ni aviso.
  - Un nombre ocupado que `SyncName` se salta da el aviso de nombre distinto.

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

### D-T4B-04. La lectura caller-owned (R-T4-01, INV-39; IR I-6)

- **Dónde:** en `RackSiblingScan.cs`, un núcleo interno de recorrido que recibe `(Transaction, Database)`.
- **`Traverse`** conserva su firma, su bloqueo, su transacción y su único `Commit`, y llama al núcleo.
- **Las guardas de cuenta** (`TW/WorkspaceEditIntentGuardTests.cs:88-128`) se conservan.
- **Es un movimiento de código sin cambio de comportamiento del barrido de Insertar** (I-55, que validó OV-RED), del insertar de RACKEDITAR y de RACKPROYECTAR (§9 I-6). Lo cubren:
  - una **guarda nueva**: `Traverse` solo contiene el bloqueo, la transacción, la llamada al núcleo y el `Commit`, y el núcleo conserva en orden los pasos de hoy (`AssertInOrder` sobre las sentencias de `RackSiblingScan.cs:115-161`);
  - **Smoke-2**: un Insertar de Selectivo, un insertar desde RACKEDITAR Push Back y un RACKPROYECTAR.
- **`eraseByKind`** sigue nulo, con la misma `CompletenessEvidence`.
- **F4-E26 se cumple por construcción:** los tres caminos usan el mismo núcleo y la misma `BuildStaleBase`. La prueba en un dibujo real es de Smoke-2.
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
  - `PrepareRedraw(` solo en el cuerpo de `PrepareUnits`;
  - un `StartTransaction` en `BeginMutation`, después de comprobar `TopTransaction`;
  - un `Commit()` en `Seal`;
  - sin `LockDocument`, `Erase(`, `EraseViewBlocks`, `RackUnitsGuard`, `async`, `await`, `ExecuteInCommandContextAsync` ni `NavigationTargets`;
  - INV-14: la transacción en `using` y ningún campo `Transaction`, `DBObject`, `Document` ni `ObjectId` fuera de las clases de ámbito;
- **la variante de lectura caller-owned** (INV-39): sin `StartTransaction`, `LockDocument` ni `Commit`;
- **el movimiento de `Traverse`** (D-T4B-04);
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

`RackSiblingScan.cs` está **permitido**: no es la costura (IR I-3).

**`RequiredTests`** (IR I-4):

| Proyecto | Filtro | `MinSelected` | `ExpectRed` |
|---|---|---|---|
| Core | `FullyQualifiedName~RackCad.Tests.Workspace` (incluye las `WorkspacePilotUpdate*` y la guarda de INV-39) | la de la base + las nuevas | true |
| Core, **una fila por clase** (RR R-I-7) | `FullyQualifiedName~RackCad.Tests.<Clase>`, con `<Clase>` en: `CallerOwnedFacadeGuardTests`, `CallerOwnedTransactionGuardTests`, `RackSiblingRedrawUnitsTests`, `DebugFaultInjectionGuardTests`, `SiblingRedrawSeamGuardTests`, `RackSiblingMembershipTests`, `RackSiblingRedrawPlanTests`, `RackSiblingRedrawRunTests`, `PushBackClassicCharacterizationPluginGuardTests`, `PushBackPluginSourceGuardTests`, `PushBackRoundTripSourceGuardTests`, `RackProjectionCommandGuardTests`, `NamespaceFolderGuardTests`, `CustomPropertiesEnvelopeGuardTests`, `RackUnitsGuardSourceTests` e `I55G15Auth15ConsumerTests`. Cada nombre se confirma por `git grep "class <Clase>"` en la emisión | la de cada clase en la CI de la base | false |
| UI | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 601 | false |
| UI | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | la de T1B | false |

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
- **13:** los caminos clásicos, Insertar y RACKPROYECTAR no cambian. El movimiento de `Traverse` lo fija su guarda. Las pruebas de T1B y de I-55 siguen en verde.
- **14:** solo los cambios de guarda declarados, exactos y con su autoprueba en negativo.
- **15:** el RED con cuerpos inertes también en las fábricas, igualdades y reenvíos (M-11). Las pruebas existentes que cambian van en el RED. Ninguna prueba necesita AutoCAD.

**`ExpectedEvidence`:**
- todo lo de F4-T4A2: contrato, delegación, RED y GREEN con su push, CI exacta y entrega;
- además, en local:
  - el build Release de la solución, sin avisos;
  - **el build Debug del Plugin** con la orden del D-T4B-03, que lleva la salida a una carpeta temporal (sin avisos nuevos respecto a la base en Debug, IR I-5, RR R-M-14);
  - el mapa evidencia → invariante.

**Paradas:**
- **C01:** haría falta tocar la UI, el contexto de comando, `async` o `LockDocument` en el escritor.
- **C02:** haría falta tocar la costura de I-55 (H-14), las fachadas, los caminos clásicos, `RackPilotIntentAdapter.cs`, `IWorkspacePilotEditPort` o sus implementadores.
- **C03:** un cambio de guarda no declarado en el D-T4B-07 o el D-T4B-09.
- **C04:** el plan necesitaría llevar un objeto del host, o un objeto de AutoCAD tendría que vivir fuera de un ámbito.
- **C05:** la comparación de POST necesitaría normalizar algo.
- **C06:** las precondiciones de emisión dejan de cumplirse.
- **C07:** la regla de fidelidad de la planificación (F4-E31 y F4-E43).

## 7. Smoke-2

T4B no lo ejecuta, porque necesita T4C. Le pasa estas comprobaciones:
- que los bytes y los handles se conservan (R-T4-04);
- una transacción y `TopTransaction == null`;
- la equivalencia de las tres lecturas;
- R-09;
- la agrupación de Undo de la importación;
- S2-R-10 con `SEAL=before` y con `SEAL=after`;
- **una capa bloqueada** en un miembro del piloto: NotAppliedPreflight, sin residuo de importación (RR R-I-4);
- **el movimiento de `Traverse`:** Insertar de Selectivo, el insertar desde RACKEDITAR Push Back y RACKPROYECTAR (D-T4B-04).

## 8. Lo que queda para F4-T4C

- R-T4-05 a R-T4-10;
- la operación del puerto `IWorkspacePilotEditPort` con sus cinco implementadores, y **la puerta de reposo** antes de entrar en el contexto de comando (RR R-I-1);
- **el cableado de la instancia del puerto de actualización** (RR R-M-12). El núcleo no puede tener un campo de `Systems`, el puerto del panel no puede llevar tipos del piloto, el soporte no puede nombrar el piloto y el adaptador del controlador reenvía literalmente. **T4C necesitará un cambio de guarda declarado**, por ejemplo la opción B-2 (b) de la revisión de la v1 (un único campo con su autoprueba);
- la llamada de la UI;
- el diagnóstico de `TryCheckNow`;
- IC-08;
- C2-M-3 a C2-M-7 donde toque.

## 9. Interpretaciones declaradas que se comunican al Owner (decisiones para la emisión)

| # | Interpretación | Alternativa |
|---|---|---|
| I-1 | **«Única escritora»:** T4B crea la escritura authored, y T4C envuelve el **mismo** escritor con el contexto de comando y el bloqueo, sin una clase nueva de escritura | Que T4C no tenga escrituras propias |
| I-2 | `TryUpdatePilot` es **un método del núcleo**, sin llamador de producto en T4B, para que la ranura sea oráculo de T4B. La operación del puerto va en T4C. **T4C necesitará un cambio de guarda declarado para cablear la instancia del puerto** (§8, RR R-M-12) | La operación en el puerto ya en T4B, con los cinco implementadores y UI.Tests en el alcance |
| I-3 | El diagnóstico de `TryCheckNow` (`A2P:657`, F4-E55) **pasa a T4C**. Toca el VM y rompe M-3 o M-2. Esto **añade «Comprobar ahora» a T4C**, más allá de su fila (`T4P:173`) (IR M-13) | Mantenerlo en T4B, añadiendo la UI al alcance |
| I-4 | **La vía (IR I-8):** el piloto **no usa la vía implementable declarada de D-13** (la costura de I-55 con OV-RED). Usa una vía propia con la mecánica de los pasos 3 a 5, que D-13 admite («la mecánica no se congela»). **OV-RED no se transfiere**, y la primera prueba física del escritor del piloto es Smoke-2 (R-09). INV-30 se resuelve sin tocar Insertar | (a) la costura, con una frontera de `Commit()` aditiva en `SiblingRedrawTransaction.cs`, conservando sus cuentas, y un cambio declarado de `SiblingRedrawSeamGuardTests`. Reaprovecha OV-RED, pero toca el camino de Insertar. (b) la vía propia reutilizando el tipo `SiblingRedrawUnit` sin tocar su archivo |
| I-5 | **«Aplicado con avisos»:** las piezas faltantes (por unidad), un nombre distinto tras `SyncName` y un `Regen` que lanza dan avisos visibles. **Los fallos de la purga solo van al registro**, porque devuelve `void` y no se toca: es una limitación de la fila de D-13 | Tocar `LateralHeaderDrawer` para que informe, fuera del alcance |
| I-6 | **El núcleo de `Traverse`** es un movimiento sin cambio de comportamiento de un camino que el Owner validó (I-55, OV-RED). Lo fijan una guarda y Smoke-2 | Duplicar el recorrido, lo que rompe las cuentas y la equivalencia por construcción |
| I-7 | **`TopTransaction`:** se rechaza una ajena al entrar (`TOP_TRANSACTION_OPEN`) y, al salir, se comprueba registrando en el log, sin `Debug.Assert` | `Debug.Assert`, que terminaría AutoCAD |
| I-8 | **El `PushBackSystem`** se reconstruye con una ayuda pura de `Pilot/` a partir del texto de diseño del plan; el corte, con `RackViewCodec`. **Hay dos catálogos** (el de la lectura previa y el de `PrepareRedraw`), y se declaran | Ampliar el plan |
| I-9 | **El origen de MUTATE** lo elige **la corrida, en puro**, por el texto exacto del origen del borrador (la regla Build (b)). Cero da `NoOriginReason` y varios `SeveralOriginsReason`, los dos como Preflight. El documento se elige por `AuthorizedDocumentNativeId`. **El origen de POST es `plan.OriginKey`.** Es distinto de la elección de «Comprobar ahora», y R-T4-01 exige solo la equivalencia de la StaleBase. **Consecuencia** (RR R-M-11): la promesa de D-T4A2-06 (`A2P:349`) solo se cumple para las causas que no dependen del origen. Las reglas 2, 5 y 6 del lector pueden dejar un diagnóstico persistente en «Comprobar ahora» mientras Actualizar admite | Leer por el lector del host (D-1 6 → 7, `PilotForms` y el catálogo, que `EditIntentReadResult` no lleva) |
| I-10 | **El respaldo de `CompleteApply`:** si el cálculo del estado lanza en una rama NotApplied, el borrador queda en **Error** con su causa (`ClosedInError`), nunca en «aplicado» | Dejar la rama tal cual y que la excepción escape, que es lo de hoy y lo que F4-E25 (4) pide corregir |
| I-11 | **La puerta de `TryUpdatePilot`** (RR R-I-1, opción a): la propia, **sin reposo ni lector** (el documento activo, el modal y `Authorize`). **La puerta de reposo pasa a la operación del puerto de T4C**, antes del contexto de comando. Así T4C llama a `TryUpdatePilot` dentro del callback sin reescribir el oráculo de la ranura | (b) `PassGate` en T4B, con un cambio en T4C que reescribe el método |
| I-12 | **«`Seal` volvió y su `Dispose` lanzó»** va a POST, como la fila «`Commit()` retornó» de D-13 (`FRZ:527`), y el `Dispose` se convierte en un aviso. No va a CommitThrew, que daría un Conflicto falso con la escritura ya hecha (RR R-I-3) | CommitThrew |
| I-13 | **Las capas bloqueadas, en T4B**: antes de importar y en MUTATE, con `LOCKED_LAYER`, como pide el paso 3 de D-13 (RR R-I-4) | Pasarlas a T4C, que deja un residuo OM-5 evitable |
| I-14 | **«Varios orígenes»** da Preflight con `SeveralOriginsReason`, y no el NotAdmitted u OriginMissing engañoso del lector (RR R-I-5) | NotAdmitted |

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
