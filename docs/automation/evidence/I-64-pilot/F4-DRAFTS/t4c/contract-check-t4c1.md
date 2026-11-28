# I-64 F4-T4C1-HOST — Comprobación del borrador del contrato

**Veredicto:** EMITIBLE TRAS CORRECCIONES. **Recuento:** 0 bloqueantes, 2 importantes y 4 menores (§3 y §4).

## 1. Objeto, método y límites

**Objeto:**
- el borrador `contract-draft.json`, con su `AuthorityRevision` provisional en ceros;
- su generador, `make_t4c1_contract.py`;
- frente a la parte [1] del paquete v5 de F4-T4C (`docs/initiatives/I-64-f4-t4c-gate-contract-package.md`, custodiado en `acffd79e`; citado como `P5:n`, en su copia de trabajo) y a la autorización de emisión del Owner.

**Método:**
- **el JSON:** sus alcances, sus invariantes, sus `RequiredTests` y sus paradas;
- **los alcances, con `git ls-files`:**
  - existencia;
  - duplicados;
  - un permitido bajo un prohibido;
  - los implementadores y dobles de cada interfaz que cambia;
  - las guardas que miran las carpetas tocadas;
- **las 23 clases:**
  - `git grep "class <Clase>"`;
  - recuento en el TRX de la CI de la base `43668817337` (`artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012820Z-7a4a/ci-core/core.trx` y `ci-ui/ui.trx`), con el script de solo lectura por clase de M4.

No se compiló, no se ejecutó ninguna prueba y no se escribió nada en el worktree.

**Dos desviaciones de procedimiento de esta comprobación:**
- una comprobación de los alcances se hizo con un script de Python pasado por la entrada estándar, no escrito como archivo;
- se creó por error un archivo temporal con la salida de `git ls-files` fuera de la carpeta de trabajo, y se borró en el acto.

Ninguna de las dos escribió en el worktree.

**Lo que no se pudo verificar:** que T4C1 compile, y el detalle de todas las guardas de cuerpo de la reconciliación.

---

## 2. Juicio de las decisiones de la sesión

### 2.1 `AllowedWriteScope` (30 rutas)

**Basta.** Coincide con las rutas marcadas [1] y [1, 2] del §6 del paquete. Se comprobó cada necesidad de compilación y de guarda de la parte [1]:
- **los seis implementadores de `IWorkspacePilotEditPort`,** todos en el permitido:
  - el núcleo;
  - la compuerta (`WorkspacePilotSupport.cs`);
  - el adaptador de `CTL`;
  - los dos dobles de `TW/WorkspaceCommandAdvisorTests.cs`;
  - el doble de UI.
- **los dos de `IWorkspaceBridgeLink`:** `CTL` y `TW/WorkspaceHostTestSupport.cs`, los dos permitidos;
- **ningún archivo de `src/RackCad.UI/` implementa** el puerto ni la `IWorkspaceBridgeLink`, ni hace un `switch` exhaustivo sobre `PilotBoundaryKind` o `EditUpdateStatus`. La UI solo consume, así que T4C1 no tiene que tocarla;
- **la carpeta del piloto:** las guardas que la recorren no se rompen con el archivo nuevo, que no debe contener «Transaction», ni con el recuento de al menos 11 archivos (`TW/WorkspacePilotUpdateGuardTests.cs:117-124`). Tampoco con la búsqueda de `RackEmbedComposer.Compose`, que se limita a la carpeta (`TW/WorkspacePilotPrepareGuardTests.cs:145`);
- **las filas [1] de D-T4C-20 (1 a 11, 13 a 17, 19, 20 y 22)** caen en archivos permitidos o en archivos nuevos;
- **las guardas que no cambian** siguen satisfechas con la parte [1]:
  - los literales de caso y el cuerpo de `SetQuiescent`;
  - las del manejador de `CommandWillStart` (`TW/WorkspaceHostC2SourceGuardTests.cs:62-90`, `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:132-142`, `TW/WorkspaceIdleSchedulingGuardTests.cs:64-104` y la lista de llamadas prohibidas de `TW/WorkspaceBridgeSourceGuardTests.cs:138-144`, que no nombra `GlobalCommandName`);
  - la de los nueve archivos de `Plugin/Workspace`;
- **ninguna prueba existente descarta por el puerto un borrador en Applying,** así que el `Busy` de D-T4C-10 no rompe `TW/WorkspacePilotEditHostTests.cs`, que no está en el permitido;
- **el valor desconocido** `(PilotBoundaryKind)99` sigue dando `Aborted`.

**No sobra nada.** Cada ruta la justifica una decisión [1] o una fila [1] de D-T4C-20.

**Ningún permitido cae bajo un prohibido:** sin prefijos ni rutas iguales. Siete rutas son nuevas, y las 23 modificadas existen.

### 2.2 `ForbiddenWriteScope` (107 entradas)

**La cuenta es correcta:** 70 − 4 + 7 + 10 + 11 + 13 = 107, sin duplicados y con todas las entradas existentes.

Conservar `src/RackCad.UI/` entero es **más estricto** que la partición del §6, y es correcto para una parte sin UI. La sonda nc4 sigue discriminando, porque `src/RackCad.UI/Systems/PushBack/` cae bajo el prohibido.

**Lo que protege:**
- **los 18 `*Commands*.cs`:** las 18 entradas F08 a F25; `git ls-files` no da ningún otro;
- **F28** (`RackPilotIntentAdapter.cs`);
- **la costura de I-55:** `src/RackCad.Plugin/Systems/`, `src/RackCad.Application/Views/Redraw/` y `RackSelectivoInsertIntegration.cs`;
- **de T1B:**
  - G1, G2, G3 y G6, por `src/RackCad.UI/`;
  - G5, por `src/RackCad.Plugin/Systems/`, `KindHandlers/` y `RackPushBackCommands.cs`;
  - las seis clases de UI y la de Core;
  - los fragmentos compartidos (`RackMenuCommands.cs`, `RackViewBatchProducts.cs` y `RackViewPlacement.cs`).

**Lo que no protege:** **G4** (K-01).

### 2.3 Invariantes y paradas

- **Los invariantes** siguen la numeración del §6, sin 11 ni 12, y su texto es fiel, salvo:
  - INV-01 (K-02);
  - INV-09 (K-04).
- **Las paradas** C01 a C14 reproducen el §6. C15, la replanificación, viene de F4-E31 y F4-E43, como en las cadenas anteriores. **Son fieles.**

### 2.4 `RequiredTests` (26 filas)

**Las 23 clases** están declaradas una sola vez cada una, con `git grep -E "class <Clase>\b"`. Sus cuentas coinciden **exactamente** con el TRX de la base:

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

**Los filtros:**
- **Core:** `RackCad.Tests.Workspace` = 3103;
- **UI:** `RackCad.UI.Tests.Workspace` = 601 y `RackCad.UI.Tests.PushBackClassicCharacterization` = 55.

Cada filtro de clase selecciona una sola clase.

**`ExpectRed`** es verdadero solo en Workspace de Core y falso en UI. Es coherente con una parte que solo cambia el doble de UI con cuerpos inertes.

---

## 3. Los hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| K-01 | IMPORTANTE | **G4 de la clausura de T1B no está en el prohibido.** Son los 52 archivos de `src/RackCad.Application/Systems/PushBack/` fuera de `Pilot/`, todos en la raíz de esa carpeta. T4C1 edita justo al lado, en `Pilot/`, con `RUN` y `HOSTRUN`. La raíz no se puede prohibir, porque cubriría el permitido. Con PD-F4-07, el Controller puede dar Scope en verde ante una lista blanca recortada, así que solo INV-16, C03 y la comparación manual de la revisión los protegen. El mismo hueco venía de la plantilla de T4B | `docs/initiatives/I-64-f4-t1b-gate-contract-package.md:219`; `git ls-files src/RackCad.Application/Systems/PushBack \| grep -v /Pilot/` = 52; el `ForbiddenWriteScope` del borrador | El generador añade los 52 por ruta exacta, desde `git ls-files` sin `Pilot/` y con la cuenta afirmada. Quedan 159 entradas. Si se prefiere no hacerlo, se declara como limitación heredada de T4B, con la comprobación de INV-16 por la revisión |
| K-02 | IMPORTANTE | **INV-01 dice que `TryUpdatePilot` se llama «desde la corrida del núcleo (HOSTRUN)».** La corrida del núcleo es la clase anidada de `WorkspaceHostCore` que implementa `IPushBackPilotHostRun`. `HOSTRUN` es el corredor: no nombra el Workspace y no puede llamar a `TryUpdatePilot`. Es la letra de G-4 y de la guarda de pureza del corredor. El texto invierte quién llama a T4B | P5:567 (el corredor no nombra el Workspace); P5:1078 (G-4); `Invariants[0]` del borrador y `make_t4c1_contract.py` (`INV`) | «…solo desde el `Execute` de la clase de corrida anidada en `WorkspaceHostCore`, una vez por corrida admitida; `HOSTRUN` solo la invoca por `IPushBackPilotHostRun`» |
| K-03 | MENOR | **La frontera `PilotBoundaryKind.Update` no está asignada a ninguna parte.** D-T4C-19 asigna «la frontera» a T4C2 (P5:1360), pero `IWorkspacePilotEditPort.cs` solo está marcado [1] en el §6, y D-T4C-17 prueba en RQ «la frontera Update en el núcleo» (P5:1293). El objetivo del borrador no lo dice: con el borrador tal cual, T4C2 no podría añadir el valor | §6 del paquete; `Objective` del borrador | Decir en el objetivo de T4C1 que T4C1 añade `PilotBoundaryKind.Update` al final y su traducción en `CrossBoundary`, con la prueba RQ. O marcar `IWorkspacePilotEditPort.cs` como [1, 2] para T4C2 |
| K-04 | MENOR | **INV-09** omite el cierre del ámbito en `Degrade` (D-T4C-09; fila 17). Conserva «los manejadores solo encolan», aunque el de `CommandWillStart` también anota `NoteForeignCommandStart`. El mismo texto está en el §6 | P5, §6 INV-09; `Invariants` del borrador | Añadir «la degradación (`Degrade`)» a los cierres, y precisar: «los manejadores solo encolan; el de `CommandWillStart` anota además, en memoria, el comando ajeno» |
| K-05 | MENOR | **`Authorities` cita `owner-f4t4c1-emission-authorization.md`,** que todavía no está en el repositorio (`acffd79e`). El propio `gate-contract.json` tampoco, que es lo esperado | `git ls-files`; `Authorities` del borrador | Custodiar antes la autorización, y fijar `AuthorityRevision` al commit que la contiene |
| K-06 | MENOR | **El resto de la carpeta `Pilot/`** depende solo de la lista blanca: `PushBackPilotDraftState.cs`, `PushBackPilotFieldAdapter.cs`, `PushBackPilotIntentResult.cs` y `PushBackPilotStateComparer.cs`. Son archivos cerrados de T2 y T3, junto a los que T4C1 edita | `git ls-files src/RackCad.Application/Systems/PushBack/Pilot` = 11; el prohibido lleva 6 | Añadirlos por ruta exacta, que cuesta cuatro entradas. O dejarlos a la lista blanca, de forma declarada |

---

## 4. Veredicto

**EMITIBLE TRAS CORRECCIONES. B/I/M = 0/2/4.**

**Lo que está bien:**
- **el `AllowedWriteScope`** basta para la parte [1], no le sobra nada y no cae bajo ningún prohibido;
- **las particiones** protegen los `*Commands*.cs`, F28 y la costura de I-55, y de T1B todo menos G4;
- **las cuentas de `RequiredTests`** salen exactas del TRX de la base;
- **los invariantes y las paradas** son fieles al §6, salvo K-02 y K-04.

**Antes de emitir:** corregir K-01 y K-02 en el generador, y custodiar la autorización (K-05). K-03, K-04 y K-06 se pueden corregir en la misma pasada, sin otra comprobación.
