# I-64 — Paquete del contrato de F4-T4A2 («Comprobar ahora» y reconciliación)

**Estado:** v3. La prepara la sesión principal el 2026-11-14. No declara GATE PASS.
- La v1 pasó por una revisión adversarial independiente: EMITIBLE TRAS CORRECCIONES (1/5/16, `independent-review.md`).
- La v2 aplicó todos sus hallazgos (§10.1) con las decisiones delegadas del Owner (`F4-DRAFTS/coordinator/owner-f4t4a2-overnight-decisions.md`, pregunta 1: «Lo que recomiendes»).
- La re-revisión de la v2 dio EMITIBLE TRAS CORRECCIONES (1/1/11, `independent-rereview.md`) y recomendó, en lugar de una tercera revisión completa, una comprobación focalizada de los dos puntos graves. La v3 aplica todo (§10.2).

**Código base:** `bbd9a35e`, el producto de F4-T4A1. HEAD es `b9e1ed5a`, la custodia de su cierre; entre los dos no cambia nada en `src/` ni en `tests/`.

**Autorización:**
- la ratificación del cierre de F4-T4A1 y la autorización de F4-T4A2 del Owner (`F4-DRAFTS/coordinator/owner-f4t4a2-authorization.md`, literal);
- las decisiones del Owner para la noche (`owner-f4t4a2-overnight-decisions.md`): el diseño con la opción recomendada, la cadena hasta el cierre, ante un imprevisto parar esa línea y esperar, y después solo los paquetes de T1B y T4B.

F4-T4A2 empieza en 1 de 3.

**Mapas previos** (se custodian con el contrato): `map-1-ui-guards.md`, `map-2-plugin-read-and-c2-guards.md` y `map-3-core-states-freeze.md`.

## 1. Autoridades

- **La orden del Owner de F4-T4A2** (literal): objetivo, flujo, los diez puntos de «Comprobar ahora», las clases de resultado, «sin falso limpio», las opciones, la confirmación, `Authorize`, «Conservar mi borrador», la recarga, el texto pendiente, cero escrituras, las 18 pruebas, TOCTOU, el plan inmutable, el proceso, la cadena, la regla de copia y la deuda de controles.
- **El Freeze** (`docs/initiatives/I-64-proposal-v5.md`):
  - D-10 (`:420-439`): los estados, Dirty, «toda recarga sustituye StaleBase e InitialDraftState a la vez, desde una sola lectura», las fronteras bloqueables y no bloqueables, y O-01;
  - D-11 (`:457-458`) e INV-31 (`:769`): «No editable aquí, con el diagnóstico visible, la entrada rechazada y el editor clásico disponible», y «el estado Desconocido con «Comprobar ahora»»;
  - D-12 (`:463-493`): MATCH solo si todo es idéntico; «Comprobar ahora: relectura de solo lectura que deja el borrador en Conflicto o confirma su estado»; la reconciliación explícita («STALE / EXTERNAL CHANGE»), con «Descartar y recargar» y «Conservar mi borrador sobre la versión actual», «sin fusión automática»;
  - la tabla por frontera de D-13 (`:521-532`), incluido «Nunca se supone aplicado»; D-14 (`:547-553`); INV-06, INV-18 e INV-26.
- **IC-07 punto 2** (`docs/initiatives/I-64-implementation-clarifications.md:123-127`).
- **El paquete de F4-T4** (`docs/initiatives/I-64-f4-t4-gate-contract-package.md`): D-T4A1-01 a D-T4A1-03 (la tabla de `ConfirmBase` y el despacho `:320-323`) y las reglas R-A2-01 a R-A2-06 (`:513-524`).
- **El paquete de F4-T3** (`docs/initiatives/I-64-f4-t3-gate-contract-package.md`): la tabla de transiciones (`:73-83`; la fila `:79`, el descarte con confirmación: «ninguna frontera; Descartar o Cancelar»), la regla `:85` y D-T3-08.
- **Los cierres:** F4-T3C2 (C2-M-1 y C2-M-2), F4-T4A1 (`analysis.md` de `R20261114T043231Z-623b`: los 14 menores) y F4-E25 (el arrastre (5)).
- **Las disposiciones** F4-E31, F4-E43 y F4-E51 a F4-E53 (`F4-DRAFTS/f4-dispositions.md`).

## 2. Hechos medidos en `b9e1ed5a`

### 2.1 El campo, el panel y el navegador

**H-01. El VM del campo** (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`, 461 líneas):
- tres botones:
  - «Editar» (`BeginEdit`, la única entrada que lee: `TryReload` si el borrador es Clean y posiblemente obsoleto, `TryBeginEdit` en otro caso, `:191-239`);
  - «Recargar» (`Rematerialize`, **solo memoria**, `:280-294`);
  - «Descartar», el del Error, que lleva el navegador;
- **no hay «Comprobar ahora»**: ningún archivo de la UI nombra `TryCheckNow`, `TryConfirmBase`, `TryKeepMine` ni `TryDiscardAndReload`, ni lee `UnknownOrigin`, `ConflictText` ni `LastCause` (map-1 §7.3, map-2 C.1);
- un borrador en Conflict con `canEdit` es editable, y lo fija `WorkspacePilotFieldViewModelTests.cs:1002-1014`;
- `EditableOf` (`:344-349`) no mira el diagnóstico de la superficie;
- `IsMine` (`:300-301`) descarta el resultado de una lectura si la superficie cambió durante ella (`:205-212`);
- cada gesto empieza con `ClearNotices` (`:313-319`), que también borra `_diagnostic` y `_hint`, incluido escribir en la caja (`:80`);
- `CommitPending` (`:242-277`) sale sin hacer nada si no hay pendiente o la caja no es editable, y si no, llama a `ClearNotices`; el código subyacente lo llama al perder el foco (`WorkspacePanelView.xaml.cs:158`);
- toda excepción de un gesto se contiene como el Error del campo (`EnterFailure`, `:405-416`).

**H-02. La única confirmación de la UI** es el hueco en línea del navegador:
- `WorkspaceBrowserViewModel.cs`: `ConfirmationKind` (`:57-63`), `Ask`, `ConfirmDiscard` y `CancelDiscard`;
- está en `BrowserConfirmationPanel`, **fuera** de la superficie;
- una prueba de vista fija ahí exactamente dos botones, «Descartar» y «Cancelar» (`WorkspacePilotCloseSyncViewTests.cs:168-194`), y una petición nueva sustituye a la pendiente (`WorkspacePilotCloseSyncTests.cs:424-451`);
- no hay abstracción de diálogo, y la UI tiene prohibidos `Window`, `MessageBox`, `ShowDialog` y hasta `EnterPanelModal` (map-1 §4.2).

**H-03. La forma pública del navegador** está fijada por texto a 33 firmas exactas (`WorkspacePilotCloseSyncGuardTests.cs:376-470`). Las del VM del campo y del VM del panel no están fijadas (map-1 §8).

**H-04. La guarda `:849`** (`WorkspacePilotCloseSyncGuardTests.cs:849-867`) prohíbe en el navegador `TryBeginEdit|TryReload|TryCheckNow`, pero **no** las tres operaciones de la reconciliación (map-1 §1.1, map-3 B).

**H-05. Las guardas del XAML y del código subyacente** (map-1 §3, §9; la revisión de la v1, §3, comprobó que el diseño no choca con ninguna):
- **G-04** es una cota inferior (`>= 3` botones) sobre el elemento `BrowserSurface` completo (`WorkspaceBrowserHostGuardTests.cs:301-347`);
- **su gemela** exige exactamente uno de `Content="Editar"`, `"Recargar"`, `"Volver a leer"` y `"Descartar"`, con la comilla de cierre, desde la superficie hasta «Navegación» (`WorkspacePilotCloseSyncGuardTests.cs:722-741`);
- **M-6 de T3B2** (`:797-839`):
  - todo el XAML sin `KeyUp` y con exactamente un `PreviewKeyDown=`;
  - todo el código subyacente sin `e.KeyboardDevice`, `Keyboard` ni `.Focus(`;
  - ningún `IsDefault=` ni `IsCancel=` en todo el XAML (`:694-709`);
- **en la sección del navegador:** sin tamaños fijos;
- **en los VM** (`:473-502`; `WorkspacePilotSurfaceGuardTests.cs:325-335`): sin `Dispatcher`, `Task`, temporizadores, `+=`, `event`, `delegate` ni estado estático;
- **vocabulario prohibido en el `.cs` y el `.xaml` de la UI, también en las cadenas:**
  - `Metric|Provider|\bBom|ProjectSummary|TotalRacks|Aggregat|ComputedParameter`, sin distinguir mayúsculas (`WorkspacePaletteHostSourceGuardTests.cs:583-601`);
  - en el XAML, `editor clásico` (`WorkspaceBrowserHostGuardTests.cs:256-264`).

**H-06. Las reglas de quién lee** (`WorkspacePilotSurfaceGuardTests.cs:56-104`):
- `TryBeginEdit`, `TryReload` y `TryCheckNow` nunca aparecen en cuerpos de presentación (`Present|Refresh|Show|Clear|...`), constructores ni *setters*;
- `TryCheckNow` nunca aparece en los gestos de T3B1;
- son reglas por cuerpo y no siguen las llamadas: un cuerpo de presentación que llamara a un método que lee no las incumple;
- una autoprueba (`:134-141`) **prevé** un miembro nuevo `CheckNow()` del VM del campo;
- T15 (`WorkspacePilotFieldViewModelTests.cs:1115-1160`) exige cero `TryCheckNow` en la presentación.

**H-07. El doble de la UI** (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs`, map-1 §6):
- `Reads()` (`:61`) cuenta solo `TryBeginEdit`, `TryCheckNow` y `TryReload`, y el resumen de `ReadHook` (`:63`) dice lo mismo (M-9);
- `TryCheckNow` responde siempre Unavailable, sin respondedor;
- `View(...)` no tiene parámetros para `unknownOrigin`, `conflictText` ni `lastCause`;
- 30 aserciones usan `Reads()`, y ninguna llama hoy a una operación de la reconciliación.

**H-08. No hay tabla de textos de la UI.** Los textos de la UI se fijan aserción por aserción (map-1 §5). La tabla de 30 filas (`WorkspaceHostHardeningGuardTests.cs:421-454`) es del host.

### 2.2 El host, el lector y el núcleo

**H-09. Las clases del Owner no existen como identificadores** (map-3 §0). La correspondencia con los valores existentes es la del §3.

**H-10. `TryConfirmPilotBase`** (`WorkspaceHostCore.cs:528-592`) sigue los pasos y textos de map-3 A.5:
- con MATCH responde `Confirmed(pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic)` (`:579`), sin la pista del editor clásico;
- con diferencias responde `Conflict(DescribeDifferences(...))` **sin** el diagnóstico, aunque la relectura no esté admitida (`:582`). Lo fija `WorkspacePilotPrepareTests.cs:993` con `EditIntentKit.MemberBecameBlocking` (`WorkspaceEditIntentHostTests.cs:59-60`), cuya relectura no está admitida por la regla 7: `UnreadableDiagnostic` (`PushBackPilotIntentReader.cs:192-194`, constante `:35`).

**H-11. Un rack borrado del dibujo** da una lectura `Read` con base vacía y NotAdmitted(`OriginMissingDiagnostic`) (map-3 A.2). Por H-10, «Comprobar ahora» lo presenta hoy como un Conflicto más, sin diagnóstico. «Conservar» y «Descartar y recargar» responden NotAdmitted, con el diagnóstico y la pista, y no cambian el borrador. Un sobre ilegible o de versión futura sigue el mismo camino, con `UnreadableDiagnostic`.

**H-12. En Conflict, `PilotFieldView.IsProtected` es `draft.IsDirty`** (`WorkspaceHostCore.cs:1163-1178`), o verdadero si `IsDirty` lanza. Dirty es «CurrentDraftState ≠ InitialDraftState, **o** hay texto pendiente» (D-10 `:422`; `DraftSession.cs:298`). La vista no tiene `IsDirty`. `ConflictText` solo se rellena en Conflict, y `UnknownOrigin` solo en Unknown.

**H-13. «Conservar» copia siempre la escala del estado actual** (`PushBackPilotMaterializer.KeepPilotEditsOn`, `:242-251`), y el host lo admite en cualquier Conflict (`WorkspaceHostCore.cs:631`). En un Conflict cuyo estado actual no tiene la escala editada, y con un cambio externo de la escala, «Conservar» devolvería la escala anterior sobre la versión nueva. Pasa sin trabajo propio y también con **solo** texto pendiente, porque el pendiente no está en el estado actual. Ninguna prueba cubre esos casos.

**H-14. El host no tiene «cancelar»:** `TryKeepPilotEdit` pasa `confirmed: true` (`:654`) y `TryDiscardAndReloadPilotEdit` no pide confirmación. La confirmación y la cancelación son de la UI.

**H-15. El texto pendiente** (map-3 C):
- ninguna de las tres operaciones lo lee ni cruza una frontera;
- `ConfirmBase` y `CheckNow` no lo tocan, `KeepMine` lo conserva tal cual y `DiscardAndReload` lo vacía;
- el Freeze clasifica como **bloqueables** solo cambiar de pestaña o de rack y Actualizar (D-10 `:434-435`). La tabla de T3 trata el descarte con confirmación como «ninguna frontera» (`I-64-f4-t3-gate-contract-package.md:79`);
- las pruebas existentes (`WorkspacePilotPrepareTests.cs:1015` y `:1100`) fijan que un pendiente inválido no bloquea, en el host, ni Conservar ni Descartar y recargar.

**H-16. La vía de lectura** (map-2 C):
- **`PassGate`** (`WorkspaceHostCore.cs:1337-1371`):
  - sin lector, Unavailable;
  - sin documento, con un modal de RackCad o con el documento sin reposo, Waiting;
  - una sesión que no es la del documento activo, Rejected, con la forma real `if (Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted) { reason = "La petición no es del documento activo."; return EditGate.Rejected; }` (`:1364-1367`, **el literal**, no la constante `NotThisDocumentReason`).
  - Su orden lo fija `WorkspaceEditIntentGuardTests.cs:562-580`, pero la comparación con `Accepted` solo por presencia.
- **El lector del Plugin** (`WorkspaceNavigationHost.cs:196-254`):
  - comando en curso o editor sin reposo: Busy;
  - `using (document.LockDocument())` alrededor de la autorización (`var decision = _authorize(new WorkspaceRequest(session), document.UnmanagedObject.ToInt64()); if (decision != RequestDecision.Accepted) return EditIntentReadResult.Rejected(...)`, `:234-236`), la revalidación y la lectura;
  - dos recorridos, cada uno en su propio `using` de bloqueo y de transacción;
  - todos los objetos se abren `ForRead`; no hay ningún `ForWrite`, y el único `Commit` es el de una transacción de lectura (`RackSiblingScan.cs:163`);
  - lo fijan las guardas de map-2 C.6.
- **T4A2 no cambia nada de esta vía.**

**H-17. C2-M-1 y C2-M-2 se reproducen** (map-2 B.3):
- las guardas actuales (`WorkspaceCommandAdvisorGuardTests.cs:99-129` y `:131-164`) solo buscan presencias, con autopruebas sintéticas;
- las **15 mutaciones** del archivo real (M1a a M1f, M2a a M2f y M3a a M3c) pasan las dos guardas;
- las formas exactas del §4 coinciden con `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:43-49` y `:60-69`;
- las cuentas que una guarda endurecida debe conservar son las de map-2 B.2.

**H-18. El corpus canónico** son los ocho diseños `pb-01-base`, `pb-02-modulos`, `pb-03-fondos`, `pb-04-blanco`, `pb-05-topes`, `pb-06-seguridad`, `pb-07-combinado` y `pb-08-redondeo` (`WorkspaceDraftPilotStateTests.cs:37-40`, `PilotKit`, interno de RackCad.Tests).
- Llevan `\r\n` escapado dentro del JSON del sobre.
- Toda comparación de Core los normaliza de CRLF a LF, porque Core corre en Linux.
- `RackCad.UI.Tests` corre en `windows-latest` (CI `:217-241`), y su caracterización también normaliza (`WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:57`).
- El arrastre (5) de F4-E25 sigue pendiente.

**H-19. El identificador `pilot` en el núcleo del host** aparece solo en estas formas, medidas (32 en total; quitadas estas formas no queda ninguno, y fuera de los seis cuerpos de lectura no aparece):

| Forma | Total | Intención | Comprobación | Recarga | Confirmación | Conservar | Descartar y recargar |
|---|---|---|---|---|---|---|---|
| `var pilot = read.PilotRead;` | 6 | 1 | 1 | 1 | 1 | 1 | 1 |
| `pilot == null` | 6 | 1 | 1 | 1 | 1 | 1 | 1 |
| `pilot.StaleBase` | 2 | 0 | 1 | 0 | 1 | 0 | 0 |
| `pilot.Read` | 6 | 1 | 0 | 1 | 2 | 1 | 1 |
| `pilot.PreviewWitness` | 4 | 1 | 0 | 1 | 0 | 1 | 1 |
| `pilot.Detail` | 4 | 1 | 0 | 1 | 0 | 1 | 1 |
| `pilot.ClassicEditorHint` | 4 | 1 | 0 | 1 | 0 | 1 | 1 |

**H-20. Prohibiciones también en los comentarios:** `WorkspaceEditIntent.cs`, `WorkspaceHostCore.cs` y `IWorkspaceEditIntentReader.cs` (`WorkspaceEditIntentGuardTests.cs:30-32`) no pueden contener `Transaction`, `Database`, `Commit(`, `ComputedParameter`, `Metric`, `ProjectSummary`, `TotalRacks`, `Aggregat`, `Provider` ni `Bom`, ni siquiera en un comentario (`WorkspaceEditIntentGuardTests.cs:935-945`).

**H-21. `TryCheckNow`** (`TryCheckPilotEdit`, `WorkspaceHostCore.cs:365-428`) responde `Conflict(DescribeDifferences(...))` sin diagnóstico, aunque la relectura no esté admitida (`:408-423`). `EditCheckOutcome` no lleva diagnóstico (`WorkspaceEditIntent.cs:237-280`). Extenderlo rompería guardas existentes: `CheckBody` exige cero `pilot.Read` y cero del patrón del diagnóstico (`WorkspaceEditIntentGuardTests.cs:681`, `:696-697`).

## 3. Las clases del Owner sobre los nombres de la arquitectura

El Freeze tiene **un** resultado para cualquier diferencia: Conflicto (D-12, INV-06). T4A2 no crea estados ni valores nuevos de enumerado en Application ni en el puerto. Distingue las clases del Owner así, y su contrato las enumera.

**«Comprobar ahora» por `TryConfirmBase`** (Clean, Dirty y Conflict):

| Clase del Owner | Respuesta del host | Vista y superficie | Texto (T-A2) |
|---|---|---|---|
| **UNCHANGED** | `Confirmed`, `AdmissionDiagnostic` null | la vista releída en Clean o Dirty; `IsPossiblyStale` false | A2-01 |
| UNCHANGED con el borrador ya en Conflict | `Confirmed`, `AdmissionDiagnostic` null | sigue en Conflict | A2-02 |
| **EXTERNAL_CHANGE** | `Conflict`, sin diagnóstico | Conflict con `IsProtected` false: sin trabajo propio | A2-04 |
| **CONFLICT** | `Conflict`, sin diagnóstico | Conflict con `IsProtected` true | A2-05, o A2-05b si hay pendiente |
| **NOT_FOUND** (sin borrador) | `NotFound` | sin borrador | prefijo `Sin borrador: ` y A2-08 |
| **NOT_FOUND / NO_LONGER_EDITABLE** (rack borrado, ya no editable, ilegible o de versión futura) | `Confirmed` o `Conflict` **con** `AdmissionDiagnostic` y `ClassicEditorHint` (D-T4A2-04) | el diagnóstico y la pista persisten; la caja no se edita (D-T4A2-06) | A2-03 o A2-06 |
| **READ_ERROR** | `Failed` | sin cambio | prefijo `Fallo: ` y A2-08 |
| **UNKNOWN** (no se leyó) | `Unavailable`, `Waiting`, `Rejected`, `NotApplicable` y cualquier otro valor, también el valor por defecto | sin cambio | prefijo de su estado y A2-08 |

**«Comprobar ahora» por `TryCheckNow`** (Unknown con `CommitThrew`):

| Clase del Owner | Respuesta del host | Texto (T-A2) |
|---|---|---|
| **UNCHANGED** (el commit no ocurrió) | `Recovered`: el borrador recupera el estado de antes del intento | A2-07 |
| **EXTERNAL_CHANGE / CONFLICT** | `Conflict` | A2-09, un texto neutro: tras un commit que lanzó, la diferencia puede venir del propio commit, y «nunca se supone aplicado» (D-13) |
| **NOT_FOUND / NO_LONGER_EDITABLE** | **limitación declarada:** `Conflict` sin diagnóstico (H-21); la elección posterior lo revela con NotAdmitted (A2-32) | A2-09 |
| **READ_ERROR** | `Failed` | prefijo `Fallo: ` y A2-08 |
| **UNKNOWN** | `Unavailable`, `Waiting`, `Rejected`, `NotFound`, `NotApplicable` y cualquier otro valor | prefijo y A2-08 |

Llevar el diagnóstico a la vía `TryCheckNow` se asigna a F4-T4B, que produce el estado Unknown y toca el host (§8).

**Sin falso limpio:**
- A2-01 sale **solo** de `Confirmed` con `AdmissionDiagnostic` null y con la vista releída en Clean o Dirty;
- A2-07 sale **solo** de `Recovered`;
- cualquier otro estado, el valor por defecto, una excepción del puerto o un valor desconocido del enumerado nunca da A2-01 ni A2-07, ni un estado más limpio que el anterior.

## 4. Decisiones de F4-T4A2

### D-T4A2-01. «Comprobar ahora» en el VM del campo

**Miembros nuevos del VM del campo:** `public bool ShowCheckButton` y `public void CheckNow()`.

**Cuándo se ofrece:** sin fallo de la superficie, sin una elección pendiente, con borrador, en una pestaña **fijada** (`!_isPreview`), y con el estado:
- Clean, Dirty o Conflict: despacha `TryConfirmBase`;
- Unknown con `UnknownOrigin == CommitThrew`: despacha `TryCheckNow`.

En cualquier otro estado no se ofrece: sin borrador, Applying, Error, Undecidable, o Unknown con PostReread o sin origen. Con un diagnóstico persistente (D-T4A2-06) **sí** se ofrece: comprobar otra vez es la forma de salir de él. Una pestaña de vista previa se fija primero con «Editar» o «Fijar», como hoy (D-T4A2-11).

**`CheckNow()`:**
1. Sin puerto, sin superficie o sin la condición de oferta, no hace nada.
2. `ClearNotices()`. **No** toca el diagnóstico persistente (D-T4A2-06).
3. Lee la vista (`GetField`) y despacha **una** operación con `new EditIntentRequest(session, rack, null, null)`, como `BeginEdit`.
4. Si la superficie cambió durante la lectura (`IsMine`), solo muestra el aviso existente de `:209`.
5. Presenta el resultado según el §3 y T-A2, vuelve a leer la vista de memoria (`Render`) y llama a `_changed`.
   - **Solo un resultado que releyó el dibujo sustituye el diagnóstico persistente:** `Confirmed` o `Conflict` de `TryConfirmBase`, que ponen el suyo o ninguno, y `Recovered` o `Conflict` de `TryCheckNow`, que lo retiran.
   - Con cualquier otro estado o con una excepción, el diagnóstico anterior se conserva y la caja sigue de solo lectura: un error nunca deja la superficie más limpia (§3).
6. Toda excepción se contiene como el Error del campo (`EnterFailure`).

**Presentación de `TryConfirmBase`.** La vista es la releída de memoria tras la operación:
- `Confirmed` sin diagnóstico, con la vista en Clean o Dirty: A2-01;
- `Confirmed` sin diagnóstico, con la vista en Conflict: A2-02;
- `Confirmed` sin diagnóstico, con la vista en otro estado (el borrador cambió durante la comprobación): A2-35 si la vista sigue teniendo borrador, y A2-37 si ya no lo tiene;
- `Confirmed` con diagnóstico: el diagnóstico persistente (D-T4A2-06) y A2-03;
- `Conflict` con diagnóstico: el diagnóstico persistente y A2-06;
- `Conflict` sin diagnóstico: A2-05b si la vista releída tiene pendiente, A2-05 si tiene `IsProtected` y no tiene pendiente, A2-04 en otro caso;
- cualquier otro estado: el prefijo de T-A2 por estado con la razón (o «sin detalle.»), y A2-08.

**Presentación de `TryCheckNow`:**
- `Recovered`: A2-07;
- `Conflict`: A2-09;
- cualquier otro estado: el prefijo y A2-08.

**Explícito:** `TryConfirmBase` y `TryCheckNow` aparecen en la UI **solo** en el cuerpo de `CheckNow`, una vez cada uno. `CheckNow` solo lo llama el manejador de su botón en el código subyacente. Ninguna presentación, *setter*, constructor, el navegador ni el panel comprueba. Lo fijan las reglas 1 y 2 de D-T4A2-07.

### D-T4A2-02. Las opciones de reconciliación (sin una cuarta política)

**Las del Freeze**, y nada más:
- **«Descartar y recargar»** (`TryDiscardAndReload`): sustituye StaleBase, InitialDraftState y CurrentDraftState desde una sola lectura y vacía el pendiente (D-12; el núcleo de F4-T4A1);
- **«Conservar mi borrador»** (`TryKeepMine`): StaleBase e InitialDraftState salen de una sola lectura. Del borrador solo sale la escala de anotación, y todo lo authored que el piloto no edita sale de la versión nueva (IC-07 punto 2; `KeepPilotEditsOn`);
- **cancelar** («No»): no llama al puerto y no cambia nada.

**Miembros nuevos del VM del campo:** `ShowDiscardAndReloadButton`, `ShowKeepMineButton`, `RequestDiscardAndReload()` y `RequestKeepMine()`.

**Cuándo se ofrecen:** siempre en una pestaña fijada, sin fallo, sin diagnóstico persistente y sin una elección pendiente.
- **«Descartar y recargar»:** en Conflict, y en Unknown con `UnknownOrigin == PostReread`. Es la fila «aplicado; recarga para seguir editando» de D-13, que usa la misma operación.
- **«Conservar mi borrador»:** **solo** en Conflict, con `IsProtected` verdadero y **sin texto pendiente** (`PendingText == null`).

**Por qué «Conservar» exige un cambio comprometido y ningún pendiente** (decisión delegada, declarada):
- Sin pendiente, `IsProtected` en Conflict es «el estado actual difiere del inicial» (H-12). Como el piloto solo edita la escala, es «la escala fue editada», salvo si `IsDirty` lanzó: en ese caso se ofrece, con la confirmación.
- En cualquier otro caso, «Conservar» solo podría poner la escala anterior sobre la versión nueva, es decir, revertir en silencio un cambio externo (H-13). La orden lo prohíbe. Sin trabajo propio, «Descartar y recargar» no pierde nada.
- Con pendiente, la superficie dice A2-05b: hay que aplicar o corregir el pendiente para poder conservar, o elegir «Descartar y recargar».
- El host no cambia. La UI es el único llamador de `TryKeepMine` (INV-02), y su oferta es la autoridad.

**En Dirty con «posiblemente obsoleto»** no se ofrece ninguna de las dos: se ofrece «Comprobar ahora», que confirma o deja el borrador en Conflict, y la elección sigue al Conflicto. Es el flujo de la orden: comprobar, clasificar, presentar y, si hace falta, elegir.

**El rótulo de Unknown(PostReread):** D-T4A1-03 decía «Recargar» (`I-64-f4-t4-gate-contract-package.md:322`). T4A2 usa «Descartar y recargar», con su propio texto de confirmación (A2-23), porque la gemela de G-04 exige un solo `Content="Recargar"` y ese es el botón de memoria del Error (`WorkspacePilotCloseSyncGuardTests.cs:738`). Es una desviación de rótulo declarada, no de operación.

### D-T4A2-03. La confirmación Sí/No, en línea en el campo

**Miembros nuevos del VM del campo:** `HasChoice`, `ChoiceText`, `ConfirmChoice()` y `CancelChoice()`.

**El hueco propio del campo:**
- Guarda una sola elección pendiente: su tipo, la sesión y el rack de la superficie, y la **instantánea** de la vista que la justificó (`State`, `IsProtected`, `PendingText`, `CommittedText` y `UnknownOrigin`).
- El tipo es un enumerado **privado** del VM, `{ None, DiscardAndReload, KeepMine }`; no es un valor del puerto (C06).
- No reutiliza el hueco del navegador: sus botones están fijados como «Descartar» y «Cancelar» (H-02), y el navegador no puede nombrar estas operaciones (INV-02).

**El ciclo:**
- **`RequestDiscardAndReload()` y `RequestKeepMine()`:**
  - leen la vista con `GetField`, que es memoria del puerto y no lee el dibujo, y no llaman a ninguna otra operación del puerto: ni a una lectura ni a una reconciliación;
  - solo si su oferta sigue vigente, fijan la elección, su instantánea y su texto (A2-20 a A2-23, según el estado, el trabajo propio y el pendiente);
  - sin oferta vigente no hacen nada.
- **`ConfirmChoice()` («Sí»):**
  1. sin elección pendiente, no hace nada;
  2. toma la elección y la retira;
  3. vuelve a leer la vista de memoria y la compara con la instantánea: la misma superficie y los mismos `State`, `IsProtected`, `PendingText`, `CommittedText` y `UnknownOrigin`. Si algo difiere, A2-34 y **ninguna** llamada;
  4. llama **una** vez a `TryDiscardAndReload` o a `TryKeepMine`, según el tipo, con `new EditIntentRequest(session, rack, null, null)`;
  5. presenta el resultado:
     - `Reloaded`, A2-30, **solo** si la elección era Descartar y recargar;
     - `Kept`, A2-31, **solo** si era Conservar;
     - `NotAdmitted`, A2-32, con el diagnóstico y la pista persistentes (D-T4A2-06);
     - cualquier otro estado, o un `Reloaded` o `Kept` que no corresponde a la elección: el prefijo de elección y A2-33;
     - si la superficie cambió durante la lectura, el aviso de `:209`;
  6. `Render` y `_changed`; toda excepción se contiene como Error del campo.
- **`CancelChoice()` («No»):** retira la elección, pone A2-36 y **no llama al puerto**.
- **La elección pendiente se retira** también:
  - con cualquier gesto nuevo del campo (lo hace `ClearNotices`);
  - con un cambio de superficie (`Show` de otra, `Clear`) y con `Fail`, en silencio, sin A2-34;
  - y en `Apply`, si la vista nueva ya no ofrece esa elección, por ejemplo tras un descarte del navegador: entonces pone A2-34. La oferta que se comprueba ahí es la de D-T4A2-02 **sin** la cláusula «sin una elección pendiente», para que la elección no se retire a sí misma.

**Mientras hay una elección pendiente:**
- **la caja es de solo lectura** (`EditableOf` lo exige), y no se ofrece ningún otro gesto del campo: ni «Editar», ni «Comprobar ahora», ni las opciones; solo «Sí» y «No»;
- así, perder el foco no compromete nada (`CommitPending` sale sin hacer nada con la caja de solo lectura, H-01), y el clic en «Sí» o «No» no se pierde por `LostFocus`;
- un pendiente que había antes de pedir la elección se queda como estaba.

**En el XAML,** dentro de `BrowserFieldPanel`:
- un `Border` `BrowserFieldChoicePanel`, visible con `HasChoice`, con `BrowserFieldChoiceText`, `BrowserFieldChoiceYesButton` (`Content="Sí"`) y `BrowserFieldChoiceNoButton` (`Content="No"`), en ese orden;
- sin `IsDefault` ni `IsCancel`, sin manejo de teclado y sin tamaños fijos;
- cada botón con su `AutomationProperties.Name` de T-A2 y su manejador `Click` en el código subyacente:
  - `private void OnBrowserFieldChoiceYesClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.Field.ConfirmChoice());`;
  - `private void OnBrowserFieldChoiceNoClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.Field.CancelChoice());`.

**Pruebas de inversión (orden del Owner):**
- **de VM:** «Sí» llama exactamente una vez a la operación elegida, con la sesión y el rack de la superficie, y **cero** veces a la otra. «No» no llama a nada y el borrador del doble queda igual. Las dos, para cada opción;
- **de vista:** un clic real en «Sí» y en «No» da los mismos resultados; también se fijan el orden, los textos y los nombres de automatización de los dos botones;
- **de fuente:** las reglas 2 y 3 de D-T4A2-07, con autopruebas en negativo que **intercambian** los dos y deben fallar.

Si Sí y No se intercambiaran en el VM, en el XAML o en el código subyacente, al menos una de esas pruebas falla.

### D-T4A2-04. El Conflicto y la confirmación llevan el diagnóstico y la pista (cambio declarado de D-T4A1-03)

Para que NOT_FOUND / NO_LONGER_EDITABLE no se confunda con un cambio externo, y para ofrecer el editor clásico con el diagnóstico (INV-31):

**En `WorkspaceEditIntent.cs`:**
- `EditConfirmOutcome` gana la propiedad `string? ClassicEditorHint`, null salvo junto a un `AdmissionDiagnostic`;
- las fábricas pasan a `Confirmed(string? admissionDiagnostic = null, string? classicEditorHint = null)` y `Conflict(IReadOnlyList<string> differences, string? admissionDiagnostic = null, string? classicEditorHint = null)`. Los parámetros son opcionales, y las llamadas existentes de uno o ningún argumento no cambian;
- los resúmenes de `AdmissionDiagnostic` y de `ClassicEditorHint` dicen «set for a Confirmed or a Conflict whose read was not admitted».

**En `TryConfirmPilotBase`,** justo después de `var confirm = draft.ConfirmBase(pilot.StaleBase);`, exactamente:

`var admission = pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic;`

`var hint = admission == null ? null : pilot.ClassicEditorHint;`

y `Confirmed(admission, hint)` y `Conflict(DescribeDifferences(confirm.Differences), admission, hint)`.

**Cuentas resultantes:**
- la expresión del diagnóstico sigue una sola vez en el cuerpo y en el núcleo, así que las cuentas existentes de la guarda del uso de la base no cambian: 2 de `pilot.Read` y 1 del patrón;
- `pilot.ClassicEditorHint` pasa de 4 a 5 en el núcleo, con 1 en la confirmación (H-19).

**Prueba existente que cambia, declarada:** `WorkspacePilotPrepareTests.cs:993` pasa de `Assert.Null(outcome.AdmissionDiagnostic)` a `Assert.Equal(PushBackPilotIntentReader.UnreadableDiagnostic, outcome.AdmissionDiagnostic)`. Es la única línea que cambia en ese archivo: el nombre de la prueba (`:978`) **no** cambia, para que la población de pruebas del RED y del GREEN sea comparable.

**No cambia nada más del host:** ni otro texto, ni la tabla de 30 filas, ni otra operación, ni `TryCheckPilotEdit` (H-21, §3).

**M-8 (documentación de `WorkspaceEditIntent.cs`):**
- el resumen de `EditConfirmStatus.Waiting` deja de decir «or the draft was being applied»;
- el de `EditConfirmStatus.NotApplicable` deja de decir «nothing was read» sin matiz: un borrador en Applying responde NotApplicable antes de leer, y uno que entra en Applying o sale del registro durante la lectura responde NotApplicable después de leer;
- la asimetría con Conservar y Descartar y recargar, que responden Waiting, se acepta y se declara: la UI presenta las dos como «no se comprobó» (A2-08).

Los comentarios nuevos respetan H-20.

### D-T4A2-05. El texto pendiente (prueba 13 de la orden)

Por D-10, por la fila `:79` de la tabla de T3 y por H-15:
- **«Comprobar ahora»** no es una transición: conserva el pendiente **byte a byte**, sin comprometerlo ni repararlo;
- **«Conservar mi borrador» no se ofrece con pendiente** (D-T4A2-02). Un pendiente, válido o inválido, impide esa transición, y el texto se queda en su caja con su mensaje (A2-05b). Es la regla «If the user action is blockable and the pending text prevents the transition: abort and preserve text» de la orden, aplicada en la oferta;
- **«Descartar y recargar»** es un descarte explícito, que en T3 no es una frontera. Pierde el pendiente **solo** después de una confirmación cuyo texto lo dice (A2-20);
- **«No»** no toca nada;
- **el compromiso al salir de la caja** (`LostFocus`, T3B1) sigue igual y es bloqueable por su cuenta: un pendiente válido se compromete antes de que actúe el botón que se pulsa, y uno inválido se queda con su mensaje. Por eso el «byte a byte» de «Comprobar ahora» se prueba en el VM con un pendiente **inválido**, y el compromiso de un pendiente válido al perder el foco es el comportamiento existente.

Las pruebas demuestran que el pendiente inválido no se descarta ni se normaliza en silencio en ninguna acción, y que «Conservar» no está disponible mientras haya uno.

### D-T4A2-06. El diagnóstico persistente, los avisos de INV-31 y la tabla de D-13

**El diagnóstico persistente de la superficie** (D-11, INV-31):
- `CheckNow` (con `Confirmed` o `Conflict` con diagnóstico) y `ConfirmChoice` (con `NotAdmitted`) guardan el diagnóstico y la pista en un campo propio de la superficie, distinto de `_diagnostic` y `_hint` de `BeginEdit`;
- `NotEditableText` y `ClassicEditorHintText` muestran el de `BeginEdit` si existe y, si no, el persistente;
- **mientras existe, la caja es de solo lectura** (`EditableOf` lo exige), no se ofrece «Editar» ni la apertura por gesto, y no se ofrecen las opciones; sí se ofrece «Comprobar ahora»;
- **`ClearNotices` nunca lo toca**: ni una tecla ni otro gesto lo quitan;
- **solo lo sustituye un resultado que releyó el dibujo** (D-T4A2-01, paso 5): `Confirmed` o `Conflict` de `TryConfirmBase`, que ponen el suyo o ninguno, y `Recovered` o `Conflict` de `TryCheckNow`, que lo retiran. Un `Waiting`, `Rejected`, `Unavailable`, `NotFound`, `NotApplicable`, `Failed` o una excepción lo conservan;
- también lo quitan un `Reloaded` o un `Kept`, un cambio de superficie (`Show` de otra), `Clear`, y `Apply` cuando la vista ya no tiene borrador, por ejemplo tras un descarte del navegador o de RACKPANELDESCARTAR;
- **limitación declarada:** es memoria de la superficie, como el diagnóstico de `BeginEdit` de T3B1. Al volver a la pestaña, el borrador vuelve a mostrarse editable, y la admisión de Actualizar en F4-T4B lo rechaza (D-T4A1-04, NotAdmitted antes de escribir). Smoke-2 lo comprueba (S2-R-05).

**`ReconcileText`** (miembro nuevo del VM del campo), mostrado en `BrowserFieldReconcileText`:
- en Conflict: A2-10, `Qué cambió: ` más `ConflictText` (o «sin detalle.»), y ` Causa: ` más `LastCause` si no está en blanco. Con texto pendiente se añade un espacio y A2-14, para que la desaparición de «Conservar» al teclear tenga su explicación, salvo si hay un diagnóstico persistente o una elección pendiente: entonces la caja es de solo lectura y A2-14 no se añade;
- en Unknown con CommitThrew: A2-11, «Estado desconocido; comprueba.»;
- en Unknown con PostReread: A2-12, «Aplicado; recarga para seguir editando.»;
- vacío en cualquier otro caso.

`StateText` no cambia: «Conflicto», «Desconocido» y los demás nombres siguen como hoy (`WorkspacePilotFieldViewModelTests.cs:1016-1029`).

**Fuera de T4A2:** «Aplicado con avisos» y la presentación de Applying son de la UI de Actualizar (R-T4-09, F4-T4C). Solo T4B produce esos estados.

### D-T4A2-07. Guardas

**Guarda nueva, `WorkspacePilotReconciliationGuardTests.cs`** (Core). Cada regla lee el texto real y tiene autopruebas en negativo **por un solo `Replace` sobre ese texto**, precedido de `Assert.Contains(from, real)`.

1. **Quién lee, por token, en todos los `.cs` de `src/RackCad.UI/`:**
   - `\bTryConfirmBase\b` y `\bTryCheckNow\b` aparecen solo en el VM del campo, exactamente una vez cada uno, dentro del cuerpo de `CheckNow`;
   - `\bTryKeepMine\b` y `\bTryDiscardAndReload\b` aparecen solo en el VM del campo, exactamente una vez cada uno, dentro del cuerpo de `ConfirmChoice`;
   - ningún otro archivo de la UI nombra ninguno de los cuatro: el navegador, el panel, la navegación y el código subyacente quedan fuera. Esto amplía H-04 sin tocar `:849`;
   - autopruebas: `TryKeepMine` en `RequestKeepMine`, `TryConfirmBase` en `Render`, cualquiera de los cuatro en el navegador, y `nameof(TryKeepMine)`.
2. **Quién llama a los gestos nuevos, por token, en todos los `.cs` de `src/RackCad.UI/`:**
   - `\b(?:CheckNow|ConfirmChoice|CancelChoice|RequestDiscardAndReload|RequestKeepMine)\b` solo aparece en su declaración `public void X()` del VM del campo, una vez cada uno, y en su manejador exacto del código subyacente, una vez cada uno: `private void OnBrowserFieldCheckClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.Field.CheckNow());`, y lo mismo para `OnBrowserFieldDiscardAndReloadClick` con `RequestDiscardAndReload`, `OnBrowserFieldKeepMineClick` con `RequestKeepMine`, `OnBrowserFieldChoiceYesClick` con `ConfirmChoice` y `OnBrowserFieldChoiceNoClick` con `CancelChoice`;
   - `\bCheckNow` no coincide con `TryCheckNow`, que tiene otro límite de palabra;
   - autopruebas: `CheckNow();` dentro de `Show`, `this.ConfirmChoice();` dentro del *setter* de `Text`, `Action a = CheckNow;`, una llamada desde el navegador y el **intercambio** de `ConfirmChoice` y `CancelChoice` entre los dos manejadores.
3. **El XAML de los cinco botones nuevos** (tabla de botones de T-A2):
   - cada botón aparece una vez, dentro de `BrowserFieldPanel`, con su `x:Name`, su `Content`, su `AutomationProperties.Name` y su `Click` exactos de la tabla;
   - «Comprobar ahora», «Descartar y recargar» y «Conservar mi borrador» llevan su propio `Visibility`, enlazado como dice la tabla;
   - Sí y No van en `BrowserFieldChoicePanel`, en ese orden, **sin** `Visibility` propio: el `Visibility` enlazado a `Browser.Field.HasChoice` es el del panel;
   - autopruebas: el intercambio de los `Click` de Sí y No, el de sus `AutomationProperties.Name`, y el intercambio de los `Click` de «Comprobar ahora» y «Conservar mi borrador».
4. **Sin falso limpio:**
   - el texto de A2-01 vive en **una** constante privada del VM del campo, referenciada una sola vez, dentro del caso `EditConfirmStatus.Confirmed` de la presentación de `CheckNow`;
   - en `src/RackCad.UI/Workspace/`, en `.cs` y en `.xaml`, con comparación **ordinal** (distingue mayúsculas), fuera de esa constante no aparece ni la subcadena `Sin cambios`, ni la segunda mitad de A2-01 (`el dibujo coincide con la base del borrador`), ni la concatenación `"Sin\s*"\s*\+\s*"\s*cambios`. La regla se limita a esa carpeta porque fuera de ella hay textos ajenos legítimos, como «Cabecera sin cambios.» (`RackDynamicSystemWindow.xaml.cs:797`);
   - lo mismo para A2-07, con el caso `EditCheckStatus.Recovered`;
   - autopruebas: una segunda referencia, la constante en el caso `Failed`, y `"Sin " + "cambios"` en otro caso;
   - **la defensa de fondo** es la teoría de comportamiento del §5.2, fila 8, sobre todos los valores: una guarda de texto no puede ver cualquier partición de la cadena.
5. **`Authorize` por forma exacta:**
   - en `PassGate`: `if (Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted) { reason = "La petición no es del documento activo."; return EditGate.Rejected; }`, con espacios flexibles, como única aparición de `Authorize(` en su cuerpo;
   - en `WorkspaceNavigationHost.cs`, **dentro de la clase anidada `PilotIntentReader`** (`:196-254`), toda aparición de `_authorize(` sigue la forma `var decision = _authorize(new WorkspaceRequest(session), document.UnmanagedObject.ToInt64());`, seguida de `if (decision != RequestDecision.Accepted)` y del Rejected. Hoy es una sola (`:234`), pero no se exige unicidad, para no congelar una lectura nueva de F4-T4B en esa clase. Las otras dos llamadas del archivo (`:120`, la lectura del resumen, y `:165`, la navegación) quedan fuera de la regla;
   - autopruebas, inyectadas dentro de `ReadOnActiveDocument`: `==` en lugar de `!=`, otro `RequestDecision`, `default` u otra sesión, el **documento** equivocado (`document.UnmanagedObject.ToInt64()` sustituido por `0L`), el resultado ignorado y `|| true`. Y una positiva: el archivo real pasa con las llamadas de `:120` y `:165`.

**C2-M-1** (`WorkspaceCommandAdvisorGuardTests.cs`):
- la guarda de `ConfirmOnCommandLine` fija exactamente `var answer = document.Editor.GetKeywords(options);` y `return answer.Status == PromptStatus.OK && string.Equals(answer.StringResult, "Si", System.StringComparison.Ordinal);`, con `if (document == null) return false;` como único otro `return`;
- sus autopruebas son M1a a M1f de map-2 B.3, cada una por un `Replace` del archivo real, y cada una debe fallar;
- se conservan las cuentas de map-2 B.2: un solo `GetKeywords`, dentro de `ConfirmOnCommandLine`, y ningún identificador `Result`.

**C2-M-2** (mismo archivo):
- la guarda del cuerpo de `RackPanelDiscard` fija exactamente `var controller = WorkspaceController.Existing;` y la llamada completa `WorkspaceDocumentDiscardCommand.Run(controller?.PilotEdit, () => controller?.Port.GetSnapshot().ActiveSession, session => controller.Port.Authorize(new WorkspaceRequest(session)) == RequestDecision.Accepted, ConfirmOnCommandLine, WriteToCommandLine);`, con espacios flexibles;
- sus autopruebas son M2a a M2f y M3a a M3c de map-2 B.3, cada una por un `Replace` del archivo real, y cada una debe fallar;
- las autopruebas sintéticas existentes se pueden conservar o sustituir, si cada caso que cubrían sigue cubierto.

**Sin cambio de producto en el Plugin.**

**M-2** (`WorkspaceEditIntentGuardTests.cs`):
- `BaseOrReadUse` **añade** la alternativa `\bpilot\s*[?!]?\s*\.\s*Read\b` y **conserva** la de `result` (`\b(?:pilot|result)\.Read\b`, `:645`), para que la autoprueba existente `var state = result.Read;` (`:773`) siga fallando;
- **una regla nueva**: en el núcleo del host, el identificador `pilot` solo aparece en las formas de H-19: `var pilot = read.PilotRead;`, `pilot == null` y `pilot\.(?:StaleBase|Read|PreviewWitness|Detail|ClassicEditorHint)\b`, sin espacios ni `?`/`!`, con el límite de palabra tras el miembro para que un `pilot.ReadX` no se quite a medias. Quitadas esas formas, no queda ningún `\bpilot\b`. La regla no fija cuentas: las cuentas por cuerpo ya las fijan las guardas existentes;
- autopruebas: `var p = pilot; var d = p.Read.Admission.Diagnostic;`, `pilot?.Read.IsAdmitted`, `pilot!.Read`, `Use(pilot)`, `(pilot).Read` y `pilot.Other`.

**M-3:** `Assert.Equal(1, Src.Count(Core(), AdmissionDiagnosticPattern))` en todo el núcleo.

**M-4** (`WorkspacePilotPrepareGuardTests.cs`):
- la guarda del núcleo de R-3 reconoce `this.ErrorReason =`, `ErrorReason ??=`, `ErrorReason +=` y una deconstrucción que asigna `ErrorReason`;
- con una autoprueba en negativo por cada forma fuera del estado Error.

**M-9** (el doble de la UI):
- `Reads()` incluye `ConfirmBaseOp`, `KeepMineOp` y `DiscardAndReloadOp`, y se actualiza el resumen de `ReadHook`;
- se añaden `CheckNowResponder` (por defecto, la respuesta actual) y los tres parámetros opcionales de `View(...)`;
- ninguna aserción existente cambia.

**D-1 sin cambio:** T4A2 no añade lecturas, así que la igualdad de 6 se mantiene.

### D-T4A2-08. El arrastre (5) de F4-E25

Es una prueba nueva de `RackCad.UI.Tests`, `WorkspaceDraftPilotCanonicalLineEndingTests.cs`, sobre los **ocho diseños canónicos de H-18, nombrados de forma explícita**: `PilotKit` no se alcanza desde RackCad.UI.Tests, y la carpeta de las fijaciones también tiene diseños antiguos.

Para cada uno:
- el documento persistido que da el piloto sin edición (`Materialize`, `ComposeForWrite` y `BuildDesign` con el mismo catálogo, como en la caracterización vecina) es **byte a byte** igual al diseño interior del sobre, **sin normalizar** los saltos de línea;
- la prueba también comprueba que ese diseño contiene `\r\n`, para que no sea vacua en Windows.

Lee los archivos por ruta desde la raíz del repositorio, como su vecina. Si la igualdad byte a byte falla en Windows, es la parada C09: un hallazgo de caracterización, y la aserción no se debilita.

### D-T4A2-09. TOCTOU y ningún plan

- T4A2 no produce ningún plan ni autorización de escritura: el resultado de «Comprobar ahora» es informativo.
- T4B relee dentro de MUTATE y compara con `plan.ExpectedBase` (R-T4-02). Que T4A2 pase no elimina esa comprobación.
- El único tipo de valor que cambia (`EditConfirmOutcome`) solo lleva estado, cadenas y listas de cadenas: ningún `DBObject`, `ObjectId`, `Transaction`, `Document` ni objeto mutable del host.
- El plan de F4-T4A1 no cambia.

### D-T4A2-10. Los menores de F4-T4A1 (orden «T4A1 MINORS»)

| Id | Superficie real | Dueño | Tratamiento |
|---|---|---|---|
| M-1 | `ComposeForWrite`, la composición para escribir | **F4-T4B** | Es código de escritura, y T4B es su primer llamador real. Va como cambio declarado del paquete de T4B: guarda de fuente con autoprueba sobre `Copy(current)`. Prioridad del Owner |
| M-2 | la guarda antialias de la lectura del host | **F4-T4A2** | D-T4A2-07. Es la prueba estática de que «Comprobar ahora» usa solo la vía autorizada y la StaleBase existente (puntos 3 y 7 de la orden) |
| M-3 | la exención del diagnóstico de admisión | **F4-T4A2** | D-T4A2-07 |
| M-4 | la guarda de R-3 en el núcleo | **F4-T4A2** | D-T4A2-07. R-3 sostiene «sin falso limpio»: el motivo de Error solo va con Error |
| M-5 | el texto del plan para un descriptor no decodificable | **F4-T4B** | El usuario del plan |
| M-6 | el orden del catálogo nulo frente al Conflict | **F4-T4B** | Ídem |
| M-7 | `ExpectedVariableRegistryText` | aceptado (F4-E53) | Sin cambio |
| M-8 | la documentación de `EditConfirmStatus.Waiting` | **F4-T4A2** | D-T4A2-04: se corrigen los resúmenes y se declara la asimetría |
| M-9 | `Reads()` del doble de la UI | **F4-T4A2** | D-T4A2-07 |
| M-10 | el XML de la sobrecarga de Conservar | **F4-T4B** | T4A2 no toca `DraftSession.cs`; T4B sí, por el arrastre (4) de F4-E25 |
| M-11 | el RED no inerte | aceptado (F4-E53) | La lección se aplica al RED de T4A2 (INV-14) |
| D-1 | la igualdad exacta de 6 lecturas | **F4-T4B / F4-T4C** | Sin cambio en T4A2, que no añade lecturas |
| D-2 | los hash del plan | endurecimiento posterior | Ninguna tarea los posee, y no se usan como clave |
| D-3 | la rama defensiva inalcanzable | endurecimiento posterior | Ídem |

### D-T4A2-11. Las clases de prueba y la vista previa

- **Las clases de prueba nuevas:**
  - en `RackCad.Tests`, con espacio de nombres de archivo `namespace RackCad.Tests;`;
  - en `RackCad.UI.Tests`, con el espacio de nombres de bloque `namespace RackCad.UI.Tests`;
  - todas `public class`, con nombres que empiezan por `Workspace`, para que las seleccionen los filtros de `RequiredTests`.
- **La vista previa:** «Comprobar ahora» y las opciones no se ofrecen en una pestaña de vista previa. Una pestaña con borrador y trabajo propio nunca lo es (D-14); un borrador limpio en una vista previa se comprueba después de fijarla.

### Tabla T-A2 (cerrada): los textos de la UI de T4A2

| Id | Texto |
|---|---|
| A2-01 | `Sin cambios: el dibujo coincide con la base del borrador.` |
| A2-02 | `La base vuelve a coincidir con el dibujo, pero el borrador sigue en conflicto: elige cómo reconciliarlo.` |
| A2-03 | `La base coincide con el dibujo, pero el rack ya no es editable aquí.` |
| A2-04 | `El dibujo cambió fuera del panel y el borrador no tiene cambios propios: «Descartar y recargar» lo pone al día.` |
| A2-05 | `El dibujo cambió fuera del panel y el borrador tiene cambios propios: elige «Conservar mi borrador» o «Descartar y recargar».` |
| A2-05b | `El dibujo cambió fuera del panel y el borrador tiene texto pendiente: aplícalo o corrígelo para poder conservarlo, o elige «Descartar y recargar».` |
| A2-06 | `El dibujo cambió y el rack ya no es editable aquí: no se puede recargar ni conservar el borrador.` |
| A2-07 | `El cambio no llegó a aplicarse: el borrador recupera el estado que tenía antes.` |
| A2-08 | ` No se comprobó.` (sufijo, con su espacio inicial) |
| A2-09 | `El dibujo no coincide con la base anterior al intento: el borrador pasa a conflicto y no se supone aplicado.` |
| A2-10 | `Qué cambió: ` y ` Causa: ` |
| A2-11 | `Estado desconocido; comprueba.` |
| A2-12 | `Aplicado; recarga para seguir editando.` |
| A2-14 | `Con texto pendiente no se puede conservar el borrador: aplícalo o corrígelo antes.` |
| A2-20 | `¿Descartar los cambios sin aplicar del borrador y recargarlo desde el dibujo?`, y con pendiente ` También se pierde el texto pendiente.` |
| A2-21 | `¿Recargar el borrador desde el dibujo? No tiene cambios propios que perder.` |
| A2-22 | `¿Conservar tu escala de anotación (` + el texto comprometido, o `sin valor` si es null, + `) sobre la versión actual del dibujo? El resto del rack se toma del dibujo y tu escala sustituye a la del dibujo.` |
| A2-23 | `¿Recargar el borrador desde el dibujo? El cambio ya se aplicó y el borrador vuelve a leerse del dibujo.` (Unknown con PostReread) |
| A2-30 | `Borrador recargado desde el dibujo.` |
| A2-31 | `Borrador conservado sobre la versión actual del dibujo.` |
| A2-32 | `No se aplicó la elección: el borrador se conserva tal como estaba.` |
| A2-33 | ` No se hizo nada.` (sufijo) |
| A2-34 | `La situación del borrador cambió; no se hizo nada.` |
| A2-35 | `Se comprobó, pero el borrador cambió durante la comprobación; vuelve a comprobarlo.` |
| A2-37 | `Se comprobó, pero el borrador ya no existe.` |
| A2-36 | `Elección cancelada: el borrador no cambió.` |

**El texto de las elecciones:** A2-20 sale en Conflict con trabajo propio o con pendiente; A2-21 en Conflict sin trabajo propio ni pendiente; A2-22 con Conservar; A2-23 en Unknown con PostReread.

**Prefijos de la comprobación** (`EditConfirmStatus` y `EditCheckStatus`), los de `Describe` (`:452-457`) más uno:
- `No disponible: `, `En espera: `, `Rechazado: `, `Sin borrador: `;
- `No aplicable: `, que es el nuevo;
- `Fallo: `;
- y por defecto `No se pudo comprobar: `.

**Prefijos de la elección** (`EditIntentStatus`):
- `No disponible: `, `En espera: `, `Rechazado: `, `Sin borrador: ` y `Fallo: `;
- `No aplicable: ` para `NotNeeded`;
- y por defecto `No se aplicó la elección: `.

**Botones del XAML,** con su `Content`, su `AutomationProperties.Name`, su `Click` y su `Visibility` (con el convertidor `FlagToVisibility` de sus vecinos):

| Botón | `Content` | `AutomationProperties.Name` | `Click` | `Visibility` enlazada a |
|---|---|---|---|---|
| `BrowserFieldCheckButton` | `Comprobar ahora` | `Comprobar el borrador ahora` | `OnBrowserFieldCheckClick` | `Browser.Field.ShowCheckButton` |
| `BrowserFieldDiscardAndReloadButton` | `Descartar y recargar` | `Descartar el borrador y recargarlo` | `OnBrowserFieldDiscardAndReloadClick` | `Browser.Field.ShowDiscardAndReloadButton` |
| `BrowserFieldKeepMineButton` | `Conservar mi borrador` | `Conservar el borrador sobre la versión actual` | `OnBrowserFieldKeepMineClick` | `Browser.Field.ShowKeepMineButton` |
| `BrowserFieldChoiceYesButton` | `Sí` | `Confirmar la elección` | `OnBrowserFieldChoiceYesClick` | el panel `BrowserFieldChoicePanel`, con `Browser.Field.HasChoice` |
| `BrowserFieldChoiceNoButton` | `No` | `Cancelar la elección` | `OnBrowserFieldChoiceNoClick` | ídem |

`BrowserFieldChoiceText` enlaza `Browser.Field.ChoiceText`, y `BrowserFieldReconcileText` enlaza `Browser.Field.ReconcileText`.

Ningún texto lleva el vocabulario prohibido de H-05.

## 5. Trazabilidad

### 5.1 Los diez puntos de «Comprobar ahora»

| # | Punto | Evidencia |
|---|---|---|
| 1 | Acción explícita del usuario | Reglas 1 y 2 de D-T4A2-07; T15 |
| 2 | Valida la sesión y el documento activo | `PassGate` (H-16) y la regla 5; prueba 14 y 15 |
| 3 | Solo por la vía de lectura autorizada | Las reglas existentes de map-2 C.6; M-2 y M-3; la regla 1 |
| 4 | En reposo y sin modal | `PassGate` (H-16); `WorkspacePilotPrepareTests.cs:605` |
| 5 | Con el bloqueo del documento | El lector del Plugin (H-16), sin cambio; sus guardas de map-2 C.6 |
| 6 | Relee lo authored | Una lectura por operación (`WorkspaceEditIntentGuardTests.cs:745-768`) |
| 7 | Compara con la StaleBase existente | `.ConfirmBase(pilot.StaleBase)` y `.CheckNow(pilot.StaleBase)`, con la guarda del uso de la base y M-2 |
| 8 | Resultado determinista | El §3 y sus pruebas |
| 9 | Cero mutaciones authored | `WriteViolations` (`WorkspacePilotEditGuardTests.cs:736-769`) y las guardas de la UI |
| 10 | Transacciones y objetos cerrados | El lector y los recorridos con `using` (H-16), sin cambio; Smoke-2 |

### 5.2 Las 18 pruebas de la orden

| # | Escenario | Core (host) | UI (VM y vista) |
|---|---|---|---|
| 1 | Sin cambios | existentes `WorkspacePilotPrepareTests.cs:942`, `:187` y `:1000` | A2-01 solo con Confirmed y la vista en Clean o Dirty; A2-02 en Conflict |
| 2 | Cambio externo del campo del piloto | **nueva:** Confirm con un cambio solo del texto de la escala da Conflict | A2-04, A2-05 y A2-05b según el trabajo propio y el pendiente |
| 3 | Cambio externo de algo que el piloto no edita | **nueva:** Confirm con un cambio solo de otro campo da Conflict | ídem |
| 4 | Los dos | **nueva** para Confirm; Conservar existente (`:1015`) | ídem |
| 5 | Rack borrado | **nuevas:** base vacía y NotAdmitted(`OriginMissingDiagnostic`). Confirm da Conflict **con** el diagnóstico y la pista; Conservar y Descartar dan NotAdmitted sin cambio | A2-06, el diagnóstico y la pista persistentes, la caja de solo lectura y sin opciones; una tecla no los borra |
| 6 | Ya no editable | **nuevas**, en una prueba nueva (sin tocar `:963`): con base igual, Confirmed con el diagnóstico y la pista; con base distinta, Conflict con los dos | A2-03 y A2-06; un borrador limpio «posiblemente obsoleto» sigue de solo lectura; una comprobación que responde Waiting o lanza conserva el diagnóstico |
| 7 | Sobre ilegible o de versión futura | `:993` cambiada (D-T4A2-04); **nuevas:** Conservar y Descartar con `UnreadableDiagnostic` | ídem |
| 8 | Excepción de lectura o de comparación | existentes `:833` y `:885`; **nuevas:** `default(EditIntentReadResult)` y una excepción dentro de `ConfirmBase` dan Failed sin cambio | teorías sobre todos los valores de `EditConfirmStatus` y de `EditCheckStatus`, el valor por defecto, `(EditConfirmStatus)99` y una excepción del puerto: nunca A2-01 ni A2-07, y el estado no cambia |
| 9 | La recarga sustituye base e inicial juntas | existentes (`WorkspaceDraftSessionTests.cs:712`, `WorkspacePilotPrepareTests.cs:1100`) | «Sí» de Descartar y recargar llama una vez a `TryDiscardAndReload` y cero a `TryKeepMine` |
| 10 | Conservar mantiene solo lo del piloto | existentes `WorkspaceDraftPilotMaterializationTests.cs:535`, `:872` y `:902`; `WorkspacePilotPrepareTests.cs:1015` | «Conservar» no se ofrece sin un cambio comprometido ni con pendiente (D-T4A2-02) |
| 11 | Conservar adopta lo authored vigente | existentes `:872`, `:902`, `:568` y `:608` | — |
| 12 | Cancelar no toca nada | núcleo existente (`NotConfirmed`) | «No» de las tres elecciones: cero llamadas, borrador igual |
| 13 | Pendiente inválido y reconciliación bloqueable | **nuevas:** las tres operaciones del host con pendiente inválido (H-15) | Conservar no se ofrece con pendiente (A2-05b); Comprobar lo conserva byte a byte; Descartar lo pierde solo tras la confirmación que lo nombra |
| 14 | SessionId equivocado | **nueva:** una sesión que nunca existió da Rejected en las tres, sin lectura ni cambio | Rechazado y A2-08 o A2-33 |
| 15 | Documento activo equivocado | existente `:605`, ampliada en una prueba nueva con base, inicial, actual y pendiente intactos | ídem |
| 16 | Cero escrituras | `WriteViolations` y la vía de H-16 | `DrawingReach` y las guardas de la UI; el doble no tiene escritura |
| 17 | Inversión de Sí/No | — | D-T4A2-03: VM, vista y fuente |
| 18 | Inversión de `Authorize` y sesión o documento equivocados | C2-M-2 y la regla 5; existentes `WorkspaceHostAuthorizationTests.cs:25-80` | — |

**Dónde van las pruebas nuevas:**
- **Core:** `WorkspacePilotReconciliationHostTests.cs`, con el host real y el lector doble (`EditIntentKit`).
- **UI:**
  - `WorkspacePilotReconciliationTests.cs`, con el VM sobre el doble;
  - `WorkspacePilotReconciliationViewTests.cs`, con la vista real.

## 6. Contrato propuesto de F4-T4A2 (a fijar en la emisión)

**`AllowedWriteScope`** (lista blanca exacta, 15 rutas):
- **Application:** `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` y `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`.
- **UI:** `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`, `WorkspacePanelView.xaml` y `WorkspacePanelView.xaml.cs`.
- **UI.Tests** (`tests/RackCad.UI.Tests/Workspace/`): `WorkspacePilotEditPortDouble.cs` y tres nuevos, `WorkspacePilotReconciliationTests.cs`, `WorkspacePilotReconciliationViewTests.cs` y `WorkspaceDraftPilotCanonicalLineEndingTests.cs`.
- **Tests** (`tests/RackCad.Tests/Workspace/`):
  - `WorkspaceCommandAdvisorGuardTests.cs`, `WorkspaceEditIntentGuardTests.cs` y `WorkspacePilotPrepareGuardTests.cs`;
  - `WorkspacePilotPrepareTests.cs`, solo la línea `:993`, sin cambiar el nombre de la prueba;
  - dos nuevos, `WorkspacePilotReconciliationHostTests.cs` y `WorkspacePilotReconciliationGuardTests.cs`.

**`ForbiddenWriteScope`** por raíces, como en F4-T4A1:
- `docs/` y `.github/`;
- la clausura de T1B, incluidos `src/RackCad.UI/Themes/` y `src/RackCad.UI/Controls/` (R-A2-06), y todo `src/RackCad.UI/` salvo los tres archivos permitidos de `Workspace/`;
- los demás archivos de `src/RackCad.UI/Workspace/`, entre ellos el navegador (`WorkspaceBrowserViewModel.cs`) y el panel (`WorkspacePanelViewModel.cs`);
- todo el Plugin, sin excepción;
- las carpetas de Application salvo `Workspace/`, y dentro de `Workspace/` todo salvo los dos archivos permitidos, con `Drafts/` completa;
- las guardas y las pruebas fuera del alcance, entre ellas `WorkspacePilotCloseSyncGuardTests.cs`, `WorkspacePilotSurfaceGuardTests.cs`, `WorkspacePilotEditGuardTests.cs`, `WorkspaceHostHardeningGuardTests.cs` y `WorkspacePilotFieldViewModelTests.cs`;
- las raíces de las pruebas.

**Invariantes:**
- **INV-01, cero escrituras authored** en «Comprobar ahora», Descartar y recargar, Conservar y «No». Lo sostienen las guardas existentes de H-16 y de la UI, y la regla 1 de D-T4A2-07.
- **INV-02, acción explícita:** la última frase de D-T4A2-01 y las reglas 1 y 2 de D-T4A2-07. La UI es el único llamador de `TryKeepMine` y `TryDiscardAndReload`.
- **INV-03, el despacho** de D-T4A2-01.
- **INV-04, las clases y sin falso limpio,** con las dos tablas del §3 y la limitación declarada de la vía `TryCheckNow`.
- **INV-05, la confirmación** de D-T4A2-03: la instantánea, la revalidación y la caja de solo lectura con una elección pendiente, más las pruebas de inversión.
- **INV-06, «Conservar»** solo en Conflict, con un cambio comprometido y sin pendiente; sin reversión en silencio.
- **INV-07, la recarga y Conservar** solo por las operaciones del host de F4-T4A1, sin cambiar su semántica.
- **INV-08, el texto pendiente** de D-T4A2-05.
- **INV-09, el diagnóstico persistente** de D-T4A2-06: la caja de solo lectura mientras existe, y la limitación declarada.
- **INV-10, el único cambio del host** es D-T4A2-04, con sus formas exactas, más los resúmenes de M-8 y H-20.
- **INV-11, C2-M-1 y C2-M-2,** con las 15 mutaciones fallando y sin cambio de producto en el Plugin.
- **INV-12, `Authorize` por forma exacta** (regla 5), sin unicidad en el lector.
- **INV-13, M-2, M-3, M-4 y M-9** como cambios declarados; D-1 sin cambio.
- **INV-14, la regla RED:**
  - el esqueleto lleva los miembros públicos nuevos del VM del campo con cuerpos **todos** inertes (falsos, vacíos o sin efecto), y la propiedad y los parámetros nuevos de `EditConfirmOutcome` sin uso en el host;
  - fallan solo pruebas nuevas o la cambiada, por aserción;
  - el XAML y el código subyacente pueden ir en el RED o en el GREEN.
- **INV-15, el arrastre (5)** de D-T4A2-08.
- **INV-16, TOCTOU** (D-T4A2-09).
- **INV-17, los límites:**
  - sin AutoCAD;
  - no cambian la vía de lectura, el lector, `RackSiblingScan`, el navegador, el panel ni ningún archivo del Plugin;
  - sin `Window`, diálogo, `EnterPanelModal`, teclado, foco, `Dispatcher`, `Task`, `+=` ni `event` nuevos;
  - ningún texto nuevo del host;
  - ninguna semántica de MUTATE, POST, Undo ni de la UI de Actualizar.

**`RequiredTests`** (líneas base de la CI del GREEN de F4-T4A1, `43554278196`):
- `RackCad.Tests.Workspace`: `MinSelected` 2843, `ExpectRed` true;
- `RackCad.UI.Tests.Workspace`: `MinSelected` 458, `ExpectRed` true.

**`ExpectedEvidence`, además de la habitual:** la cláusula de deuda de controles de la orden. Un sustituto local de la tarea solo se reutiliza con la firma EXACTA de sus condiciones aceptadas: PD-F4-01 para nc2, PD-F4-04 o PD-F4-05 para nc1, y PD-F4-06. Nunca se infiere un sustituto porque falle la misma comprobación. Una firma nueva para esa línea y vuelve al Owner (decisión de la noche, pregunta 3).

**`StopConditions`,** además de las S y P comunes:
- **C01:** un archivo fuera del alcance, o un archivo nuevo distinto de los cinco declarados;
- **C02:** una guarda o prueba existente cambiaría fuera de lo declarado en D-T4A2-04 y D-T4A2-07;
- **C03:** una escritura authored, una transacción, un bloqueo, o una semántica de MUTATE, POST, Undo o Actualizar;
- **C04:** haría falta cambiar el navegador (su forma de 33 firmas), el panel, el Plugin, la vía de lectura, `Themes/` o `Controls/` de la clausura de T1B, o algún archivo de `Drafts/`;
- **C05:** haría falta un `Window`, un diálogo, `EnterPanelModal`, un segundo `PreviewKeyDown=`, `KeyUp`, `IsDefault`/`IsCancel`, teclado o foco;
- **C06:** haría falta, en `src/RackCad.Application/`, un estado o un valor de enumerado nuevo o un texto nuevo del host, o una cuarta opción de reconciliación en cualquier capa;
- **C07:** la tarea evalúa Long-horizon;
- **C08:** la regla de copia de la planificación: una replanificación reforzada, y la segunda falla es STOP;
- **C09:** la igualdad byte a byte del arrastre (5) falla en Windows.

**`attempts`:** 1 de 3.

## 7. Escenarios de reconciliación para Smoke-2 (se ejecutan después de cerrar T4B)

Con racks reales de Push Back, en el documento activo y sin otro comando en curso. En cada uno se comprueba que el historial de deshacer y el estado de modificación del dibujo no cambian.

| Id | Preparación | Esperado |
|---|---|---|
| S2-R-01 | Borrador limpio, sin cambios en el dibujo | «Comprobar ahora» da A2-01 |
| S2-R-02 | Borrador limpio; RACKEDITAR cambia un módulo del mismo rack | Conflicto con A2-04 y el detalle; solo «Descartar y recargar»; «Sí» lo pone al día |
| S2-R-03 | Borrador con la escala editada; RACKEDITAR cambia un módulo | A2-05; «Conservar» y «Sí» mantienen la escala y adoptan el módulo; «No» no cambia nada |
| S2-R-04 | Borrador con la escala editada; RACKEDITAR cambia la escala | A2-05; «Conservar», con su confirmación explícita, mantiene la del borrador |
| S2-R-05 | Rack borrado del dibujo | A2-06 con «No editable aquí: el origen ya no está en el dibujo» y la pista; la caja no se edita; al volver a la pestaña, la limitación declarada de D-T4A2-06 |
| S2-R-06 | Rack convertido en no editable (A/B, referencia externa) | A2-03 o A2-06 con su diagnóstico y la pista |
| S2-R-07 | Comando en curso o modal de RackCad abierto | «En espera: …», sin lectura |
| S2-R-08 | Cambio de documento activo con la superficie de otro | «Rechazado: …» |
| S2-R-09 | Texto pendiente inválido en la caja y conflicto | Comprobar lo conserva; «Conservar» no se ofrece (A2-05b); «Descartar y recargar» avisa de que se pierde; el foco al pulsar «Sí» o «No» no pierde el clic |
| S2-R-10 | Desconocido (después de T4B, con la inyección Debug) | A2-11 y «Comprobar ahora» con A2-07 o A2-09; PostReread: A2-12 y «Descartar y recargar» con A2-23 |
| S2-R-11 | Borrador modificado y «posiblemente obsoleto» | «Comprobar ahora» da A2-01 y quita la marca |
| S2-R-12 | Solo texto pendiente, sin cambio comprometido, y RACKEDITAR cambia la escala | «Conservar» no se ofrece; ninguna acción devuelve la escala anterior |

## 8. Lo que queda para F4-T4B y F4-T4C

- **F4-T4B:**
  - M-1, M-5, M-6 y M-10 (D-T4A2-10);
  - el diagnóstico de admisión en la vía `TryCheckNow` (§3);
  - la relectura dentro de MUTATE contra `plan.ExpectedBase` y la relectura caller-owned;
  - D-1, si añade una lectura.
- **F4-T4C:** «Aplicado con avisos» y Applying en la UI de Actualizar (R-T4-09), y D-1 si añade una lectura.
- **Smoke-2:** los escenarios del §7 y los de la orden.

## 9. Interpretaciones declaradas que se comunican al Owner

- **La prueba 13** (D-T4A2-05):
  - ninguna acción de la reconciliación es una frontera bloqueable en el Freeze;
  - el bloqueo por pendiente se aplica a «Conservar» en su oferta;
  - «Descartar y recargar» es un descarte explícito con confirmación.
- **«Conservar»** se ofrece solo con un cambio comprometido y sin pendiente (D-T4A2-02).
- **El Conflicto y la confirmación** llevan el diagnóstico y la pista (D-T4A2-04), como cambio declarado del host de F4-T4A1.
- **Limitaciones:**
  - en la vía `TryCheckNow`, NOT_FOUND se ve como Conflicto hasta la elección (§3);
  - el diagnóstico persistente es memoria de la superficie (D-T4A2-06).
- **El rótulo «Descartar y recargar»** en Unknown(PostReread) (D-T4A2-02).

## 10. Disposición de las revisiones

### 10.1 La revisión de la v1 (EMITIBLE TRAS CORRECCIONES, 1/5/16)

| Id | Disposición |
|---|---|
| B-1 | **Aceptado.** La regla nueva de M-2 usa las formas medidas de H-19 y conserva la alternativa de `result` |
| I-1 | **Aceptado.** La forma real de `PassGate` con el literal (H-16, regla 5), la mutación del documento y sin unicidad en el lector |
| I-2 | **Aceptado.** Las reglas 1 y 2 van por token en toda la UI, con las autopruebas pedidas |
| I-3 | **Aceptado, más estricto:** «Conservar» solo con un cambio comprometido y sin pendiente (D-T4A2-02), con A2-05b, A2-22 honesto, la prueba de UI y S2-R-12 |
| I-4 | **Aceptado, opción (a):** el diagnóstico persistente con la caja de solo lectura y la pista del editor clásico, que lleva el host (D-T4A2-04 y D-T4A2-06), más la limitación declarada |
| I-5 | **Aceptado:** la tabla de `TryCheckNow` (§3), A2-09 neutro, la limitación declarada y la asignación a F4-T4B |
| M-01 | Aceptado: `UnreadableDiagnostic` (D-T4A2-04) |
| M-02 | Aceptado: una constante y su referencia única, sin otra «Sin cambios», y las teorías ampliadas (regla 4, §5.2 fila 8) |
| M-03 | Aceptado: A2-01 exige la vista en Clean o Dirty |
| M-04 | Aceptado: A2-23 propio y el rótulo declarado (D-T4A2-02) |
| M-05 | Aceptado: A2-09 |
| M-06 | Aceptado: con una elección pendiente, la caja es de solo lectura y `LostFocus` no la retira (D-T4A2-03); el compromiso previo, declarado (D-T4A2-05); S2-R-09 |
| M-07 | Aceptado: la instantánea, la correspondencia del resultado con la elección, cero llamadas a la otra operación y la retirada en `Apply` |
| M-08 | Aceptado: los prefijos de la elección, A2-08 y A2-33 coherentes, y el respaldo «sin valor» de A2-22 |
| M-09 | Aceptado (regla 5) |
| M-10 | Aceptado: el alcance en `src/RackCad.UI/` y los nombres de automatización atados, con su autoprueba |
| M-11 | Aceptado: el enumerado privado del VM (D-T4A2-03) y C06 limitado a Application, salvo la cuarta opción |
| M-12 | Aceptado: los ocho nombres (H-18, D-T4A2-08) y la parada C09 |
| M-13 | Aceptado: el resumen de NotApplicable y H-20 |
| M-14 | Aceptado: la fila `:79` citada y el §9 |
| M-15 | Aceptado (D-T4A2-11) |
| M-16 | Aceptado: §5.1, la cláusula de deuda de controles y S2-R-11 y S2-R-12 |

### 10.2 La re-revisión de la v2 (EMITIBLE TRAS CORRECCIONES, 1/1/11)

La re-revisión comprobó que 20 de las 22 disposiciones resuelven su hallazgo y que las cuentas de H-19 son exactas. Recomendó, para los dos puntos graves, una comprobación focalizada en lugar de una tercera revisión completa.

| Id | Disposición |
|---|---|
| R-B1 | **Aceptado.** La regla 5 en el Plugin queda limitada a la clase `PilotIntentReader`, con las mutaciones dentro de `ReadOnActiveDocument` y una autoprueba positiva sobre `:120` y `:165` |
| R-I1 | **Aceptado.** `ClearNotices` nunca toca el diagnóstico persistente, y solo lo sustituye un resultado que releyó (D-T4A2-01 paso 5, D-T4A2-06), con la prueba de Waiting y de una excepción (§5.2 fila 6) |
| R-M1 | Aceptado: `Apply` lo retira cuando la vista no tiene borrador (D-T4A2-06) |
| R-M2 | Aceptado: la oferta de `Apply` sin la cláusula de la elección pendiente; `Fail` y `Clear` en silencio (D-T4A2-03) |
| R-M3 | Aceptado: `Request*` lee con `GetField` y no llama a ninguna otra operación (D-T4A2-03) |
| R-M4 | Aceptado: H-20 nombra `IWorkspaceEditIntentReader.cs` |
| R-M5 | Aceptado: el límite de palabra tras el miembro (M-2) |
| R-M6 | Aceptado: la concatenación concreta, la segunda mitad de A2-01 y la defensa de fondo declarada (regla 4) |
| R-M7 | Aceptado: los nombres de los manejadores y la regla 3 sobre los cinco botones (T-A2) |
| R-M8 | Aceptado: A2-35 |
| R-M9 | Aceptado: la fila 6 dice «en una prueba nueva, sin tocar `:963`» |
| R-M10 | Aceptado: la prueba de `:993` no se renombra |
| R-M11 | Aceptado: A2-14 en `ReconcileText` con pendiente (D-T4A2-06) |

**Comprobación focalizada de la v3** (el mismo revisor, §5 de `independent-rereview.md`): **EMITIBLE**. R-B1 y R-I1 están resueltos y no hay bloqueantes ni importantes nuevos. Sus cuatro menores se aplican en la emisión:

| Id | Disposición |
|---|---|
| F-M1 | Aceptado: la regla 4 es ordinal y se limita a `src/RackCad.UI/Workspace/` |
| F-M2 | Aceptado: A2-14 no se añade con un diagnóstico persistente ni con una elección pendiente |
| F-M3 | Aceptado: A2-37 cuando la vista releída ya no tiene borrador |
| F-M4 | Aceptado: Sí y No sin `Visibility` propio; el del panel (regla 3) |
