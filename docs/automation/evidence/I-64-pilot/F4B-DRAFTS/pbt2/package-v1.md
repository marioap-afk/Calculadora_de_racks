> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`, `<datos-locales-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T2 (aplicar, cerrar y escribir separados; los dos modos), v1

**Estado:** v1, borrador para la revisión independiente adversarial. **No se emite.** Antes de emitirse se vuelve a medir sobre el cierre de PB-T1c (§6.0, «Anclajes»). No declara GATE PASS.

**Lo que pide el Owner** ([OD], fila de PB-T2 del §5 de [P], literal):

> ACCEPT IN PRINCIPLE. Separate: "apply edits to current editing state" from: "close editor" and from: "write authored DWG state". Workspace mode must return edited state to DraftSession. Classic RACKEDITAR may continue its compatible existing outer workflow. Avoid two independent implementations of field validation.

**El contenido que [P] asigna a PB-T2** (§5, `:504`):
- separar «aplicar al estado», «cerrar» y «escribir»;
- el modo explícito (§4 de [P]), con la guarda de «sin camino clásico»;
- el dueño de los subdiálogos lo decide el host;
- una sola validación de campos;
- la prueba UI-STA de OB-01.3;
- el paso 4 de D-PB-06 en WORKSPACE, que PB-TC dejó pendiente (OB-06.3).

**Lo que añaden las decisiones posteriores:** D-PB-07 (Owner, 2026-11-19, «BOM y biblioteca (Recomendado)»): en WORKSPACE el editor ofrece «BOM» y «Guardar en biblioteca», con dos costuras de prueba nuevas y cuatro comprobaciones en la UI-STA de OB-01.3; el aviso de D-PB-08 en WORKSPACE (F4B-E06 y F4B-E09); D2 (F4B-E11), condiciones 4 y 10 y el límite, arrastradas por el §10 del paquete de PB-T1.

**Lo que propone este paquete:**
- **Dos subtareas secuenciales**, cada una con su cadena y su presupuesto de 1/3 (OB-10.1), sin rebase entre ellas y con delegaciones sucesivas (R-PB-09):
  - **PB-T2a — el puerto al borrador** (`F4B-PBT2A-DRAFT-STATE-PORT`): en Application, sin la ventana. El puerto por el que el editor devuelve su estado al `DraftSession`, su adaptador y el lanzamiento tipado del modo WORKSPACE.
  - **PB-T2b — la ventana en dos modos** (`F4B-PBT2B-WORKSPACE-MODE-WINDOW`): W y un archivo nuevo de UI. El modo, la carga desde el estado, «Aplicar al borrador», «Cerrar», la ocultación, la guarda, las dos costuras y las pruebas heredadas.
- **CLASSIC no cambia:** los constructores, los cuatro sitios, WX, WC, el Plugin y los literales de T1B quedan igual. RACKEDITAR sigue con su flujo exterior (la frase del Owner).
- **WORKSPACE** se obtiene solo con la factoría tipada `RackPushBackSystemWindow.ForDraft(PushBackDraftEditorLaunch)`. Carga una **copia** del estado del borrador, sin pasar por un diseño. «Aplicar al borrador» aplica al estado de edición con **el mismo paso** que «Actualizar» (extraído de `RequestDraw`, sin cambiar su comportamiento) y devuelve una copia por el puerto. No cierra la ventana ni escribe en el DWG.
- **Cuatro decisiones nuevas para el Owner** (§8.2): el ciclo aplicar/cerrar, la compuerta de un diseño bloqueado, el alcance de «una sola validación» y la emisión condicional.

**Condiciones para emitir** (§6.0):
1. PB-T1b y PB-T1c cerradas;
2. las respuestas del Owner a Q-PBT2-1 a Q-PBT2-4;
3. la remedición sobre el cierre de PB-T1c, sin cambio de una premisa material (§6.0, con la definición de L-6 del paquete de PB-T1);
4. la revisión independiente de este paquete y la comprobación acotada de cada subtarea;
5. las celdas vigentes, el `config.toml` igual a la Snapshot J (F4B-E19) y el binario acreditado;
6. la custodia.

**Base de la medición:** `2f5d3dec` de la rama `architecture/workspace-persistente-rackcad`, en solo lectura y por objetos git desde el checkout principal. `main` sigue en `812baf87`. El detalle y las órdenes están en `pbt2-measurements.md` (apartados A a P). **Marca [T1c]:** toda ancla de W, WC o WX es de `2f5d3dec` (la ventana de `b5c09f0b`, diff vacío, registro A-3) y hay que volver a medirla sobre el cierre de PB-T1c, que aplica D-PBT1-13.

## 0. Alias

- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`; **WC** = `….Composite.cs`; **WX** = `….xaml`.
- **SES** = `src/RackCad.UI/Editor/RackEditorSession.cs`; **RID** = `src/RackCad.UI/Editor/RackEditorIdentity.cs`; **EM** = `src/RackCad.UI/Editor/EditorModules.cs`.
- **RPC** = `src/RackCad.Plugin/RackPushBackCommands.cs`; **RMC** = `src/RackCad.Plugin/RackMenuCommands.cs`.
- **PES**, **PESC**, **POG** = `src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs`, `…Comparer.cs`, `PushBackEditorOutputGate.cs`.
- **DS**, **DR** = `src/RackCad.Application/Workspace/Drafts/DraftSession.cs`, `DraftRegistry.cs`.
- **T-SG** = `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs`; **T-PG** = `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`; **T-CL** = `tests/RackCad.UI.Tests/PushBackClassicCharacterizationCloseTests.cs`.
- **S0** = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`; **SC1** = `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`.
- **[P]** = el paquete de la fase v3.1; **[PT1]** = el paquete de PB-T1 v2.7 (con lo que corrija PB-T1c); **[OD]**, **[DP]**, **[OD2]** como en [PT1] §1; **[OPB7]** = las decisiones del Owner del 2026-11-19 sobre D-PB-07 y la `ExtensionData` anidada.
- **S** = el estado del borrador (`DraftSession.CurrentDraftState`); **W0** = el estado de la ventana justo después de cargar S en WORKSPACE.

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | La fila de PB-T2 (literal arriba). D-PB-01 (los dos modos, «ZERO authored DWG writes», «The mode must be explicit», «Do not let a Workspace invocation accidentally execute the classic drawing-write path»), D-PB-06, D-PB-08, D-PB-10, §7.3 (T1B) y §7.4 (RACKEDITAR) |
| [P] (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1) | §4 (los dos modos y su tabla, `:477-493`); OB-01.1 a OB-01.6; OB-06.3; OB-07.1; OB-08.2; OB-10.1; OB-T1B.1 a OB-T1B.3; OB-RE.1; H-PB-30 a H-PB-33; R-PB-09 y R-PB-10; la fila de PB-T2 del §5 (`:504`) |
| [OPB7] (`F4B-DRAFTS/coordinator/owner-f4b-dpb07-extensiondata-decisions.md`, a custodiar; las vueltas, `F4B-DRAFTS/pbfull/owner-return-dpb07.md` y `owner-return-extensiondata.md`) | **Pregunta 1:** «BOM y biblioteca (Recomendado)», con sus obligaciones para PB-T2. **Pregunta 2:** «Basta así (Recomendado)»: el panel rechaza todo campo desconocido (AUTH-13) y la pérdida de RACKEDITAR queda como limitación anterior |
| Proposal V5 D-15 (`docs/initiatives/I-64-proposal-v5.md:557-582`) | Punto 2: «la superficie emite la intención de Actualizar hacia su host. El host clásico la interpreta como hoy». Punto 3: ida y vuelta del estado sin la ventana. Punto 4: «el host decide el dueño» |
| D-16 e INV-19 (`:586-593`) | Sin cambio observable en RACKEDITAR |
| ADR-0029 D7, D8, D12 y D13 | Una política de cierre; dirty por ámbito; sin modelo paralelo; la caracterización no se edita para que pase |
| [PT1] (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`) | D-PBT1-02 a D-PBT1-13 (el estado y la ventana sobre él), D-PBT1-15 (D2), §4 (guardas), §5.5 (censo) y §10 (A-1 y A-2) |
| [OD2] (`F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`, F4B-E11) | D2: las dos referencias compartidas, las diez condiciones y el «IMPORTANT LIMIT» |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | C08 (una replanificación por la comparación literal) y C11 (los sustitutos solo con la firma exacta) |
| F4B-E19 (`F4B-DRAFTS/coordinator/owner-config-snapshot-j-decision.md`) | La Snapshot J es la línea base del Controller (P-01) |
| F4B-E21 (`F4B-DRAFTS/coordinator/owner-f4b-opaque-collections-decision.md`) | La sesión pone las ocho colecciones opacas del contrato, byte a byte |
| F4B-E22 (`F4B-DRAFTS/coordinator/owner-pbt1b-rework-decisions.md`) | Las lecciones M10 (lista cerrada de ayudantes) y M14 (`build-plugin` omitido en el RED); los menores de producto M11 y M15 arrastrados a PB-T2 y PB-T4 |
| F4B-E06 y F4B-E09 (`f4b-dispositions.md`) | Las obligaciones de PB-T2: el paso 4 de D-PB-06 y el aviso de D-PB-08 en WORKSPACE |
| Los cierres de PB-TC, PB-TC2, PB-T1a, PB-T1b y PB-T1c (`…/task-close-package.md`) | Las redes que siguen en verde; CT-PBTC-02, 05 y 07 como oráculos del paso 4 y del aviso |
| T1B (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md`) | 55 casos de UI y 10 de Core, y sus guardas de texto |
| `docs/AUTOMATION_PLAN.md` §16.3, §16.5, §16.8 a §16.11; `docs/automation/agent-execution/README.md` §4 y §10; `routing.md`; `model-catalog.md` | El protocolo, A1-A8, el RED de la cadena, las 14 comprobaciones, los términos, las paradas y las celdas |

## 2. Hechos medidos en `2f5d3dec`

### 2.1 Hoy, aplicar, cerrar y escribir van juntos [T1c]

- **HT2-01. «Aplicar al estado de edición» no es un paso propio.** Cada gesto escribe en el estado y recalcula (`Recompute`, W:616-723; `ReadInputs`, W:833-890; desde PB-T1c, `ReadInputs` escribe en `editable`, D-PBT1-13 (5)). Lo único que no llega al estado sin un clic es la **edición de módulo escenificada**. `RequestDraw` la confirma (W:3386-3389), y cerrar sin petición la pierde después de preguntar (`PendingWork`, W:3457-3459).
- **HT2-02. `RequestDraw` (W:3363-3419) junta en un manejador** las guardas (W:3365, W:3371), la confirmación de módulos (W:3386-3389), el recálculo (W:3391), la validez (W:3392-3396), la compuerta de salida (W:3400-3403), el nombre y el modelo (W:3405-3406), la anotación de la intención (`RequestUpdate` W:3409 o `RequestInsert` W:3414) y el cierre (W:3418). **Escribir no lo hace la ventana:** lo hace quien la abrió, después de `ShowModalWindow` (HT2-09).
- **HT2-03. «Insertar varias…»** (W:3334-3361) hace lo mismo y además abre el diálogo de lote (W:3351). No mira `canInsertInAutoCad` (T-SG CT-23, `:357-379`).
- **HT2-04. La intención se anota en la sesión** (SES:138-195): `EnsureId`, `UpdateOnly`, `InsertView`/`InsertSection`, `InsertionRequest` e `InsertRequested = true`, y se lanza `InsertRequestedRaised`, que nadie escucha (T-SG `:251-264`).
- **HT2-05. Cerrar** (`Close_Click`, W:3421 → `OnClosing`, W:3435-3444): solo pregunta si no hay petición y hay un módulo escenificado (W:3437; W:3457-3459). Escape, la X y Alt+F4 pasan por el mismo sitio (comentario de W:3423-3434; ADR-0029 D7).
- **HT2-06.** «Restaurar» (W:3307-3330) recarga el último diseño válido. No escribe.
- **HT2-07.** «BOM» (W:3463-3479) y «Guardar en biblioteca» (W:3481-3515) pasan la misma compuerta de salida (W:3473, W:3493) y no anotan la petición («Saving never flags an insert», W:3505-3506).
- **HT2-08. `UpdateButtons`** (W:3563-3610): «Actualizar» = `canInsertInAutoCad && canOutput && isEditingExisting`; «Insertar» y los cuatro por vista = `canInsertInAutoCad && canOutput`; «BOM» = `canOutput`; «Guardar en biblioteca» = `currentInputsAreValid`. `InsertBatchButton` no está (registro B).

### 2.2 Cómo vuelve el estado al llamador

- **HT2-09. RACKEDITAR** (`EditPushBack`): `LoadExisting` (RPC:169), `ShowModalWindow` (RPC:170), `InsertRequested` (RPC:172), `DesignToInsert` y `SystemToInsert` (RPC:178-179), el GUID del sobre por delante del de la ventana (RPC:187), `system.Name = name;` (RPC:190), `UpdateOnly` (RPC:193 y siguientes) e `InsertionRequest.Views` (RPC:254).
- **HT2-10.** RPB lee `InsertionRequest` (RPC:50) y dibuja (RPC:57; `system.Name` en RPC:98). El menú devuelve la petición solo si `InsertRequested` (EM:131) y la despacha (RMC:131-143; `pushBack.System.Name` en RMC:135).
- **HT2-11. El estado editable nunca vuelve al llamador.** Solo vuelven el diseño, el sistema, el id, el nombre y las vistas de la petición (registro F-5). Por eso el modo WORKSPACE necesita un canal nuevo: el puerto de PB-T2a.

### 2.3 Dónde se valida cada campo

- **HT2-12. La regla de cada caja** es `NumericFieldValidation.Validate`, en UI (`src/RackCad.UI/Controls/NumericFieldValidation.cs:61-137`), compartida por todos los editores (registro J-1).
- **HT2-13. La compuerta de cada pasada** es `AllFieldsValid` sobre 21 cajas (W:727-755). La usan `Recompute` (W:625) y siete manejadores (registro J-2). Cinco cajas se validan en su propio manejador (registro J-3).
- **HT2-14. La validación de Application** (`PushBackAdvancedRackParameters.Validate`, llamada por el ensamblador) lanza, y la ventana lo convierte en un cálculo inválido (W:662-670; registro J-4).
- **HT2-15. La compuerta de salida** es `outputIsBlocked` (W:711; WC:803-807), que consultan «Actualizar», «Insertar», el lote, el BOM y la biblioteca (registro J-5). PB-T1c la delega en `PushBackEditorOutputGate` (D-PBT1-13 (7)).
- **HT2-16. Hay ya una segunda implementación de una regla de caja:** el campo de escala del panel valida con `PushBackPilotFieldAdapter.Validate` (`PushBackPilotFieldAdapter.cs:35-53`). Tiene los mismos textos y el mismo analizador que `AnnotationScaleBox` (WX:68-69: `Minimum="0" MinimumInclusive="False"`), pero escritos otra vez en Application (registro J-6). Es anterior a F4B.

### 2.4 Los subdiálogos y su dueño

- **HT2-17. Todos los subdiálogos los posee la ventana:** el configurador (`{ Owner = this }`, W:2466), las botas por poste (W:2761), la defensa (W:2905), la seguridad (W:3056), los topes (W:3087), el BOM (W:3478), el lote (`TryShow(this, …)`, W:3351) y el diálogo de guardado (`PromptSaveToLibrary(this, …)`, W:3498 → `dialog.ShowDialog(owner)`, `UiSupport.cs:81`). El aviso de descarte no tiene dueño WPF (H-108 de MB).
- **HT2-18. El dueño de la ventana lo pone quien la abre:** RPB y RACKEDITAR la abren sin dueño, con `ShowModalWindow` (RPC:47-48, :168-170; T-PG CT-24 prohíbe `Owner =` en RACKPUSHBACK). El menú y la biblioteca la abren con `{ Owner = context.Owner }` (EM:114, :121; T-SG CT-21).
- **HT2-19. Costuras:** hay costura para el configurador, la seguridad, la defensa, los topes y las botas (registro B). **No hay costura para el BOM ni para el diálogo de guardado**. En una ventana sin mostrar, el diálogo de guardado lanza al pedir su dueño, y `PushBackLibrarySaveGateTests.cs:95` usa esa excepción como señal. `PromptSaveToLibrary` crea antes la carpeta de biblioteca en el perfil (`UiSupport.cs:66-68`).

### 2.5 El piloto Workspace hoy, y lo que le falta para recibir el estado editado

- **HT2-20. Lo que hace:** el panel edita un solo campo, la escala (`PushBackPilotFieldAdapter`). El borrador es un `DraftSession<PushBackPilotDraftState>` nacido con el comparador y el copiador del piloto (`WorkspaceHostCore.cs:347-352`). El materializador rechaza el compuesto y la cola (PM:106, PM:108) y no compone nada que no sea la escala (PM:112). La escritura es PREPARE → MUTATE → POST de T4. Cuando un rack no entra, el panel muestra el texto de `ClassicEditorHint` y no abre nada (registro I).
- **HT2-21. Lo que le falta, y a quién le toca:**

| Falta | Tarea |
|---|---|
| Un canal por el que el editor devuelva su estado a un `DraftSession` | **PB-T2a** (el puerto) |
| Abrir la ventana sobre un estado y no sobre un diseño, y que su botón principal aplique al borrador | **PB-T2b** |
| Un `DraftSession<PushBackEditableState>` con el comparador y el copiador de PB-T1, y la materialización del estado desde la lectura (retira la copia del piloto, A-5) | PB-T4 |
| «Editar todo», la modalidad, el texto pendiente del panel y K = 1 (R-PB-10) | PB-T4 |
| Componer y escribir el diseño completo | PB-T3 |
| Quitar los rechazos A/B (OB-03.2) | PB-T3 o PB-T4 |

- **HT2-22. El núcleo admite ya el estado de PB-T1:** `DraftRegistry.Begin<TState>` es público (DR:143-167) y `SetCurrentDraftState` guarda una copia hecha con el copiador inyectado (DS:345-350). Una prueba puede nacer un `DraftSession<PushBackEditableState>` sin tocar el host (registro H-1).

### 2.6 El estado de PB-T1 sobre el que se diseña

- **HT2-23. PB-T1b** (en `2f5d3dec`, intento 2 de 3 en curso, que solo toca pruebas): `PushBackEditableState`, su comparador y su copiador (`Copy` sobre un estado nuevo, PESC:63-136), el mapa y `PushBackEditorOutputGate` (registro G). **No hay ninguna carga de un estado desde otro estado en sitio.**
- **HT2-24. PB-T1c** (no emitida; D-PBT1-13): `private readonly PushBackEditableState editable = new PushBackEditableState();` con `composite => editable.Editor`; los campos de portadores como reenvíos; las cargas delegadas, con `UpdateCompositeEnabled()` en la línea siguiente; `editable.AdoptCarriers(inputs, catalog);` como primera sentencia del `try` de `LoadFromModel`; `ReadInputs` que escribe en el estado; `SyncName()` tras cada `Adopt` y `SetName`; la costura `EditableStateForTest`.
- **HT2-25. Lo que de eso usa PB-T2b:** la copia del estado (`PushBackEditableStateComparer.Copy`), `BuildInputs`, la carga de `LoadFromModel` y `SyncName`. La reasignación de `editable` en un constructor es válida en C# aunque el campo sea `readonly`, porque es otro constructor de la misma clase. Hay que comprobar sobre el cierre de PB-T1c que ninguna guarda de PB-T1c la impide (R-PBT2-09).
- **HT2-26. M11 de PB-T1b** (arrastre a PB-T2): `LoadDesignEditor` y `RestoreEditor` con un diseño nulo mutan antes de lanzar. **PB-T2 no lo ejerce:** la carga de WORKSPACE no llama a `LoadDesignEditor`, y «Restaurar» conserva su guarda (W:3309-3313).

### 2.7 Las guardas que tocan estas zonas

El detalle está en el registro K, y su tratamiento, guarda por guarda, en el §4. Las que condicionan el diseño:
- **T-SG lee toda la clausura `src/RackCad.UI/Systems/PushBack`** (`:169-176`). Un archivo nuevo ahí entra en CT-19 (sin `KeyDown`, `.Focus(`, `DialogResult`…).
- **S-7, S-9 y CT-22** fijan los literales y el orden del BOM y de la biblioteca. **CT-23** fija `InsertBatch_Click`.
- **G-10** exige una sola `.RequestUpdate(` en W y ninguna en otro archivo de `src/RackCad.UI`.
- **El núcleo de borradores** no admite nombres de sistema ni tipos llamados `…Editor…` (`WorkspaceDraftBoundaryTests.cs:193`, `:204`). Por eso el puerto no va en `Drafts/`.
- **La reflexión de la vista previa** (`PushBackRichPreviewTests.cs:349-367`) mira los métodos y los tipos de campo no públicos de W.
- **El censo de ventanas** no admite un `Window` nuevo sin censar.

## 3. El diseño

### D-PBT2-01. Dos subtareas secuenciales (OB-10.1, R-PB-09)

**Por qué dividir:** son dos capas con dos regímenes de RED distintos: el esqueleto inerte en Application y la costura inerte en la ventana. El canal al borrador se prueba entero sin la ventana **antes** de que la ventana dependa de él (D-15, punto 3), como PB-T1b frente a PB-T1c. Así PB-T2b consume una API ya probada, y PB-T4 la heredará igual («consume the already proven full Push Back draft/state API», [OD] para PB-T4).

**La secuencia:** PB-T2a → PB-T2b, cada una desde la punta que deja cerrada la anterior, sin rebase entre tareas. PB-T2a solo cambia Application; PB-T2b no toca Application.

**No es material** (como L-3 del §8.3 de [PT1]): el contenido, el orden y las dependencias de la fase no cambian. Se informa al Owner en la emisión.

### D-PBT2-02. PB-T2a: el puerto al borrador (API exacta)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs`, con un solo `namespace RackCad.Application.Systems.PushBack` de bloque. **No va en `Drafts/`**: el núcleo no admite nombres de sistema ni tipos llamados `…Editor…` (§2.7).

```csharp
public enum PushBackDraftApplyStatus { NotEditable = 0, Applied = 1 }

public readonly struct PushBackDraftApplyResult
{
    public PushBackDraftApplyResult(PushBackDraftApplyStatus status, bool isDirty);
    public PushBackDraftApplyStatus Status { get; }
    public bool IsDirty { get; }
}

/// El único canal por el que el editor completo de Push Back, en modo WORKSPACE, devuelve su estado editado.
public interface IPushBackDraftStatePort
{
    PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited);
}

public sealed class PushBackDraftSessionPort : IPushBackDraftStatePort
{
    public PushBackDraftSessionPort(DraftSession<PushBackEditableState> draft);
    public PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited);
    public static PushBackDraftEditorLaunch LaunchFor(DraftSession<PushBackEditableState> draft, RackProject sourceProject);
}
```

**La semántica:**
- el constructor lanza `ArgumentNullException` con `draft` nulo y guarda la referencia en un campo privado `readonly`;
- `ApplyToDraft` lanza `ArgumentNullException` con `edited` nulo. Si no, llama **solo** a `draft.SetCurrentDraftState(edited)`, que guarda una copia hecha con el copiador del borrador (DS:345-350), y traduce el resultado: `Updated` → `Applied`; `NotEditable` → `NotEditable`. `IsDirty` es el del resultado del núcleo;
- `LaunchFor` lanza `ArgumentNullException` con `draft` nulo. Si no, devuelve `new PushBackDraftEditorLaunch(draft.CurrentDraftState, draft.RackId, sourceProject, new PushBackDraftSessionPort(draft))`.

**Por qué un resultado propio y no `DraftEditResult`:** el puerto del panel ya prohíbe que tipos de `Drafts` crucen sus valores (registro H-3). El de la ventana sigue el mismo criterio: la ventana solo conoce tipos de `Systems/PushBack`.

**Lo que el adaptador no hace:** no copia (copia el borrador); no valida (la validación es la de la ventana, D-PBT2-11); no llama a ninguna otra operación del borrador (`CrossBoundary`, `TryBeginApply`, `DiscardAndReload`, `KeepMineOnCurrentVersion`, `EnterError`, `MarkPossiblyStale`, `SetPendingText`).

### D-PBT2-03. PB-T2a: el lanzamiento tipado

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackDraftEditorLaunch.cs`.

```csharp
public sealed class PushBackDraftEditorLaunch
{
    public PushBackDraftEditorLaunch(PushBackEditableState state, string rackId, RackProject sourceProject, IPushBackDraftStatePort port);
    public PushBackEditableState State { get; }
    public string RackId { get; }
    public RackProject SourceProject { get; }
    public IPushBackDraftStatePort Port { get; }
}
```

- `state` nulo → `ArgumentNullException`; `rackId` nulo, vacío o en blanco → `ArgumentException`; `port` nulo → `ArgumentNullException`; `sourceProject` puede ser nulo.
- Guarda las referencias tal cual: **no copia**. Copia la ventana (D-PBT2-05).
- `SourceProject` es el proyecto interior del sobre, con sus metadatos I-11. Lo usa «Guardar en biblioteca» (`WithSourceMetadataFrom`). El host de PB-T4 lo saca de la lectura.
- Es el **argumento tipado** de OB-01.1b: el modo WORKSPACE no se pide con un booleano, sino con este tipo.

### D-PBT2-04. PB-T2b: el modo explícito

- **El tipo:** `src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs`, `public enum PushBackEditorMode { Classic = 0, Workspace = 1 }`, con un solo `namespace RackCad.UI.Systems.PushBack` de bloque. Entra en la clausura de T-SG y no lleva ningún token de CT-19.
- **El campo:** `private readonly PushBackEditorMode mode;` en W. Vale `Classic` (el valor por defecto) en los constructores de hoy, que no cambian ni un carácter (OB-01.1b). Solo lo asigna, una vez, el constructor privado de WORKSPACE. **Es inmutable durante la vida de la ventana.**
- **La factoría:** `public static RackPushBackSystemWindow ForDraft(PushBackDraftEditorLaunch launch)` → `ArgumentNullException` con `launch` nulo; si no, `new RackPushBackSystemWindow(launch)`.
- **El constructor privado:** `private RackPushBackSystemWindow(PushBackDraftEditorLaunch launch) : this(canInsertInAutoCad: false)`, con la carga de D-PBT2-05 en su cuerpo. La ventana que devuelve **no se muestra** y no tiene dueño: los pone el host (PB-T4, OB-01.4).
- **Quién elige el modo:** quien construye la ventana. Los cuatro sitios de hoy (registro F-1) eligen CLASSIC sin saberlo. En F4B, solo el «Editar todo» del panel (PB-T4) llamará a `ForDraft`.
- **La costura:** `internal PushBackEditorMode ModeForTest => mode;`.

### D-PBT2-05. PB-T2b: WORKSPACE carga una copia del estado, sin pasar por un diseño

**Por qué no por un diseño:** la cola aparcada no se persiste («dormir no persiste», R-PB-02). Cargar S pasando por su diseño perdería la cola, la sesión de módulos y la selección, que D-PB-03 exige conservar. Por eso la ventana adopta una **copia del estado** y solo vuelve a sembrar los controles.

**El cuerpo del constructor privado, en este orden** (el de `LoadExisting` en D-PBT1-13 (3)):

```csharp
mode = PushBackEditorMode.Workspace;
draftPort = launch.Port;
editable = PushBackEditableStateComparer.Copy(launch.State);   // el editor, la cola, la autoridad de módulos y los portadores
var inputs = editable.BuildInputs(assembler);                  // lo que la carga clásica habría adoptado
UpdateCompositeEnabled();                                      // el sitio de WC:933, como en LoadExisting
sourceProject = launch.SourceProject;
isEditingExisting = true;
session.Identity.Adopt(launch.RackId, editable.Name);
SyncName();
LoadFromModel(inputs, editable.Name);
appliedState = PushBackEditableStateComparer.Copy(editable);   // W0: la referencia de «sin aplicar»
ApplyModeChrome();                                             // D-PBT2-09
```

- **Los campos nuevos:** `private readonly IPushBackDraftStatePort draftPort;` y `private PushBackEditableState appliedState;`.
- **La reasignación de `editable`** ocurre en un constructor, así que el campo sigue siendo `readonly` (HT2-25). Pasa después de que el constructor encadenado haya hecho `LoadNew` sobre el estado inicial, que se descarta entero.
- **D2:** la copia comparte con S solo las dos referencias declaradas de cada lado (D-PBT1-05). La ventana no las muta (CT-PBT1C-04 sobre W y WC). Su primer recálculo vuelve a apuntar su línea base (`AcceptComputation`), sin tocar la de S.
- **M11 no aplica:** no se llama a `LoadDesignEditor` (HT2-26).
- **La normalización declarada N-PBT2-01.** `LoadFromModel` empieza por `editable.AdoptCarriers(inputs, catalog)` (D-PBT1-13 (4)), que vuelve a adoptar los portadores desde las entradas que el propio estado produce. Es idempotente salvo en dos casos, y los dos dejan **la misma salida**:
  - (a) si S trae `DimensionViewsTouched = true`, W0 queda con `DimensionViewsTouched = false` y con `LoadedDimensionViews` = la política efectiva (`DimensionViewPolicy.FromEditor(…)`);
  - (b) si S trae una selección de seguridad no autorizada, W0 no la trae (`BuildInputs` usa `assembler.AuthorizedSafety`).

  Es lo mismo que pasa hoy al reabrir con RACKEDITAR el diseño que ese estado produce. Lo fija CT-PBT2B-02. Si W0 difiriera de S en cualquier otra cosa, o la salida cambiara, es PBT2B-C15.
- **Las cargas públicas en WORKSPACE:** `LoadNew`, `LoadDesignForNew` y `LoadExisting` empiezan por `if (mode == PushBackEditorMode.Workspace) throw new InvalidOperationException(WorkspaceLoadRefusalMessage);`. La ventana del borrador solo lee el borrador (D-PB-01: «the editor reads from the draft»). El constructor encadenado llama a `LoadNew` cuando `mode` todavía vale `Classic`, así que no lanza.

### D-PBT2-06. «Aplicar al estado de edición», «Aplicar al borrador» y «escribir», separados

**(1) Aplicar al estado de edición: un solo paso, compartido.** Se extrae de `RequestDraw`, literal y en el mismo orden (W:3386-3403), a un ayudante privado:

```csharp
private bool ApplyEditsToEditingState(string invalidMessage, string blockedFallback)
{
    if (moduleOwner.ModuleSession.HasPendingChanges)
    {
        moduleOwner.CommitModuleEdits();
    }

    RequestRecompute(); // synchronous validate + build
    if (!currentInputsAreValid || session.System == null)
    {
        SetStatus(invalidMessage, true);
        return false;
    }

    return !OutputBlockedAfterRecompute(blockedFallback);
}
```

`RequestDraw` (CLASSIC) pasa a ser: `if (RefuseInWorkspace()) return;`, sus dos guardas de hoy (W:3365-3375) sin cambio, `if (!ApplyEditsToEditingState("Corrige los datos: no se puede insertar un modelo inválido.", "Corrige los datos: no se puede dibujar un modelo bloqueado.")) return;`, y después lo de hoy, de W:3405 al `Close()` de W:3418, con el `SyncName()` de PB-T1c. **Los textos, el orden y el efecto son los de hoy.** `InsertBatch_Click` no se refactoriza: CT-23 fija sus literales.

**(2) Aplicar al borrador (solo WORKSPACE):**

```csharp
private void ApplyToDraft()
{
    if (!ApplyEditsToEditingState(WorkspaceInvalidMessage, WorkspaceBlockedMessage)) return;
    session.Identity.SetName(NameBox.Text?.Trim());
    SyncName();
    if (!HasUnappliedDraftChanges())
    {
        SetStatus(WorkspaceNothingToApplyMessage, false);
        return;
    }

    var result = draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable));
    if (result.Status != PushBackDraftApplyStatus.Applied)
    {
        SetStatus(WorkspaceNotEditableMessage, true);
        return;
    }

    appliedState = PushBackEditableStateComparer.Copy(editable);
    SetStatus(WorkspaceAppliedMessage, false);
}

private bool HasUnappliedDraftChanges()
    => mode == PushBackEditorMode.Workspace && appliedState != null
       && !PushBackEditableStateComparer.Instance.Equals(editable, appliedState);
```

- `Update_Click` pasa a `{ if (mode == PushBackEditorMode.Workspace) { ApplyToDraft(); return; } RequestDraw(null, -1, updateOnly: true); }`.
- **No cierra la ventana** (según Q-PBT2-1, opción A). No anota ninguna petición, no llama a `SetModel` y no lee `InsertRequested`.
- **«Sin cambios» no llama al puerto:** si el estado es igual al último aplicado (o a W0), el borrador no se toca. Así, abrir y aplicar sin editar deja S exacto, aunque N-PBT2-01 hubiera normalizado W0.
- **Lo que recibe el borrador es una copia** del estado de la ventana. El borrador la vuelve a copiar (DS:345-350). La ventana no guarda ninguna referencia a S ni al estado del borrador, salvo las dos de D2 que trae su copia.

**(3) Escribir:** sigue siendo de otro. En CLASSIC, del llamador tras `ShowModalWindow` (HT2-09 y HT2-10). En WORKSPACE, del «Actualizar» del panel (PREPARE → MUTATE → POST de T4, PB-T3). La ventana en WORKSPACE no tiene ningún camino a la escritura (D-PBT2-08).

### D-PBT2-07. «Cerrar»

- **El camino:** el de hoy. `Close_Click`, Escape, la X y Alt+F4 llegan a `OnClosing` (W:3435-3444), que no cambia. Hay una sola política de cierre (ADR-0029 D7).
- **`PendingWork` declara un ámbito más en WORKSPACE** (según Q-PBT2-1, opción A):

```csharp
internal EditorPendingWork PendingWork()
{
    var modules = EditorPendingWork.When(
        moduleOwner.ModuleSession != null && moduleOwner.ModuleSession.HasPendingChanges,
        "Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?");
    return modules.HasPendingWork ? modules : EditorPendingWork.When(HasUnappliedDraftChanges(), WorkspaceUnappliedQuestion);
}
```

  - En CLASSIC, `HasUnappliedDraftChanges()` es `false`: el resultado es el de hoy. La expresión del módulo queda literal.
  - En WORKSPACE, primero el módulo escenificado (el texto de hoy) y después «sin aplicar al borrador».
  - Es un ámbito declarado (ADR-0029 D8): la frontera del borrador. No es un dirty global de la sesión.
- **El borrador queda como estaba** si se cierra sin aplicar ([P] §4). Lo que no se aplicó se pierde.
- **Lo que no cuenta como «sin aplicar»:** un texto escrito en una caja sin salir de ella todavía no está en el estado, y se pierde sin preguntar, como en CLASSIC (H-095 de MB). Es una lectura declarada, L-PBT2-10.

### D-PBT2-08. La guarda «sin camino clásico» (OB-01.3)

- **El ayudante:**
  ```csharp
  private bool RefuseInWorkspace()
  {
      if (mode != PushBackEditorMode.Workspace) return false;
      SetStatus(WorkspaceRefusalMessage, true);
      return true;
  }
  ```
- **Dónde va, como primera sentencia:**
  - en `RequestDraw` (Actualizar en CLASSIC, Insertar y los cuatro botones por vista);
  - en `InsertBatch_Click`, delante de la confirmación de módulos, que CT-23 solo exige antes del diálogo.
- **Los únicos miembros de W que anotan una petición clásica** (`session.RequestUpdate(`, `RequestInsert(` y `RequestInsertViews(`) son esos dos, y los dos empiezan por la guarda. Lo fija CT-PBT2B-11.
- **No depende de `canInsertInAutoCad`:** OB-01.1b manda que el modo no sea un booleano. `InsertBatch_Click` ni siquiera lo mira (HT2-03). La ventana de WORKSPACE se construye con `false`, pero la guarda es el modo.
- **Lo que prueba la UI-STA (CT-PBT2B-03):** pulsa en WORKSPACE **todos** los botones de la barra, también los ocultos (el evento `Click` se eleva igual), y comprueba que `InsertRequested` sigue en `false` e `InsertionRequest` en `null`.
- **Lo que queda para PB-T4** (OB-01.3): la guarda de texto de que el abridor de «Editar todo» no llama a ninguna ruta clásica.

### D-PBT2-09. Rótulos y ocultación (OB-01.6)

**`ApplyModeChrome()`** (solo en WORKSPACE, una vez, al final del constructor privado):
- `UpdateButton.Content = WorkspaceApplyLabel;`;
- `Visibility.Collapsed` para `InsertButton`, `InsertLateralButton`, `InsertFrontalEntradaButton`, `InsertFrontalPosteriorButton`, `InsertPlantaButton` e `InsertBatchButton`.

**`UpdateButtons()`** no toca el código de hoy. Al final añade solo: `if (mode == PushBackEditorMode.Workspace) { UpdateButton.IsEnabled = canOutput; UpdateButton.ToolTip = outputIsBlocked ? WorkspaceApplyBlockedToolTip : currentInputsAreValid ? WorkspaceApplyToolTip : "Corrige los campos numéricos marcados."; }`. Según Q-PBT2-2, opción A, «Aplicar» exige lo mismo que «Actualizar».

**WX no cambia.** `FrontalSideBox` sigue visible en un compuesto: no escribe, y elige qué lado muestran los frontales de la vista previa (L-PBT2-4). En CLASSIC no cambia ningún rótulo (OB-01.6).

**Los literales** (constantes `private const string` en W, cada una una sola vez; los declara este paquete, como permite [OD] para el rótulo; L-PBT2-7):

| Constante | Texto |
|---|---|
| `WorkspaceApplyLabel` | `Aplicar al borrador` |
| `WorkspaceApplyToolTip` | `Guarda los cambios en el borrador. El dibujo cambia con «Actualizar» del panel.` |
| `WorkspaceApplyBlockedToolTip` | `Corrige el diagnóstico bloqueante: el rack no puede pasar al borrador así.` |
| `WorkspaceRefusalMessage` | `En el editor del borrador no se dibuja: aplica al borrador y actualiza desde el panel.` |
| `WorkspaceLoadRefusalMessage` | `La ventana del borrador solo carga el estado del borrador.` |
| `WorkspaceInvalidMessage` | `Corrige los datos: no se puede aplicar al borrador un modelo inválido.` |
| `WorkspaceBlockedMessage` | `Corrige los datos: no se puede aplicar al borrador un modelo bloqueado.` |
| `WorkspaceNothingToApplyMessage` | `No hay cambios que aplicar al borrador.` |
| `WorkspaceAppliedMessage` | `Cambios aplicados al borrador. El dibujo cambia con «Actualizar» del panel.` |
| `WorkspaceNotEditableMessage` | `El borrador no admite cambios ahora: cierra el editor y revisa el panel.` |
| `WorkspaceUnappliedQuestion` | `Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?` |

«Actualizar» es el rótulo del botón del panel (`WorkspacePanelView.xaml:239`).

### D-PBT2-10. BOM y biblioteca en WORKSPACE (D-PB-07): las dos costuras

**La decisión** ([OPB7], pregunta 1): los dos se ofrecen en WORKSPACE, con la misma compuerta que en CLASSIC (HT2-07). No hay código propio de WORKSPACE.

**Costura 1, el BOM:**
- `internal Action<RackCad.Application.Bom.BillOfMaterials, Window> BomDialog;`. Por defecto es nulo y se muestra la ventana real.
- En `Bom_Click`, justo delante de la línea S-7, que no cambia:
  ```csharp
  if (BomDialog != null)
  {
      BomDialog(lastComputation.Bom, this);
      return;
  }

  new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();
  ```
- La costura recibe el BOM calculado y **el dueño que usaría la ventana real**, sin construirla. WPF no admite como dueña una ventana que no se ha mostrado: con las ventanas de prueba, asignar `Owner = this` lanzaría.

**Costura 2, el diálogo de guardado:**
- `internal Func<Window, string, string> LibraryPathDialog;` (el dueño y el nombre propuesto → la ruta, o nulo si se cancela).
- En `SaveLibrary_Click`, la sentencia de W:3498 pasa a:
  ```csharp
  var path = LibraryPathDialog != null
      ? LibraryPathDialog(this, NameBox.Text)
      : UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema");
  ```
- El literal S-9 sigue. El orden de CT-22 (el `PromptSaveToLibrary(this,` antes de `EnsureId`, `SetName`, el proyecto y `Save`) se conserva. Con la costura, la prueba no crea la carpeta de biblioteca del perfil.

**Los literales de S-7, S-9 y CT-22 quedan en verde** (§4), así que la condición de [OPB7] («si no pueden quedar en verde, se vuelve al Owner antes del contrato») no se dispara.

**La identidad** ([OPB7]; el dato de H-PB-33): la ventana de WORKSPACE adopta el `RackId` del borrador (D-PBT2-05), así que `EnsureId` (W:3503) no acuña nada. El archivo de biblioteca no lleva el GUID ni el nombre (`RackProject.ForPushBack(design)`). Lo comprueba CT-PBT2B-03.

**Los campos desconocidos** ([OPB7], «el archivo de biblioteca con los campos desconocidos del rack (I-11)»): `WithSourceMetadataFrom(sourceProject)` conserva los del nivel del proyecto (`RackProjectStore.PreserveUnknownMetadata`, `RackProjectStore.cs:71-95`, llamado desde `Serialize` en `:60`). Los de la carga útil se pierden también en RACKEDITAR (limitación anterior, [OPB7] pregunta 2). La prueba lleva un campo desconocido en el nivel del proyecto (L-PBT2-6).

**Las salidas a disco**, declaradas y aceptadas por [OPB7]: la exportación del BOM y el archivo de biblioteca, que pueden reflejar valores no aplicados ni dibujados.

### D-PBT2-11. Una sola validación de campos

- **Dentro de la ventana, por construcción:** WORKSPACE no tiene ninguna validación propia.
  - Las cajas, `AllFieldsValid`, el cálculo y la compuerta de salida son los de CLASSIC (HT2-12 a HT2-15).
  - «Aplicar al borrador» pasa por **el mismo** `ApplyEditsToEditingState` que «Actualizar» (D-PBT2-06).
  - El puerto no valida nada (D-PBT2-02).
  - Lo prueba CT-PBT2B-05: para el mismo gesto, las dos ventanas dan la misma validez y el mismo bloqueo.
- **El campo de escala del panel** (HT2-16) es una copia de la misma regla, anterior a F4B. Según Q-PBT2-3, opción A, PB-T2 lo fija con una prueba de concordancia (CT-PBT2B-13) y lo deja para PB-T4. PB-T4 rehace ese campo sobre `PushBackEditableState` y deja una sola regla.
- **D2, condición 4:** ninguna validación muta las dos referencias compartidas, ni las de la ventana ni las del borrador (CT-PBT2B-05, con huellas profundas).

### D-PBT2-12. El dueño de los subdiálogos (D-15, punto 4)

- **La lectura:** D-15 (4) se escribió para una superficie alojada en el panel («la ventana clásica, o la ventana principal de AutoCAD en el panel»). D-PB-01 eligió otra forma: el editor completo **es una ventana** modal y a tamaño completo, la misma en los dos modos.
- **El diseño:**
  - en los dos modos, el dueño de cada subdiálogo es la propia ventana (`Owner = this`, HT2-17), con el código de hoy;
  - el host decide el dueño **de la ventana**. En CLASSIC lo deciden los cuatro sitios de hoy (HT2-18). En WORKSPACE, `ForDraft` no pone ninguno y lo decidirá PB-T4: la ventana principal de AutoCAD con `ShowModalWindow`, como RACKEDITAR, o la ventana del panel.
- **Por qué no un dueño inyectado para los subdiálogos:** con el editor modal abierto, su dueño está deshabilitado, y un subdiálogo de otro dueño quedaría detrás o sin modalidad. Además rompería los literales `Owner = this` de CT-16, CT-20, S-7 y S-9, y obligaría a volver al Owner (OB-T1B.3).
- **Las pruebas:** CT-PBT2B-07, para las dos ventanas hijas nuevas de D-PB-07, y las guardas de T1B, que no dependen del modo, para las demás. Si la revisión juzga que esta lectura es una decisión material, vuelve al Owner (L-PBT2-3).

### D-PBT2-13. El paso 4 de D-PB-06 y el aviso de D-PB-08 en WORKSPACE

- **El paso 4** (OB-06.3): CT-PBT2B-08. Con los oráculos de CT-PBTC-02 y CT-PBTC-05, un borrador cargado con los valores exactos o con el estilo nombrado pasa por la ventana en WORKSPACE, recibe un gesto neutro y se aplica. El estado aplicado lleva el valor exacto o el estilo, y su diseño también.
- **El aviso** (OB-08.2, «va en los dos modos»): CT-PBT2B-09, con las líneas de CT-PBTC-07, más una línea que aplica al borrador con el aviso visible.

### D-PBT2-14. D2 en PB-T2

| Condición | Qué hace PB-T2 |
|---|---|
| 4. «validation does not mutate either shared object» | CT-PBT2B-05: huellas profundas de las referencias de la ventana y del borrador durante los gestos de validación, el BOM, la biblioteca y la aplicación |
| 10. guardas | CT-PBT2A-04 (G-L1 sobre los dos archivos nuevos de PB-T2a) y CT-PBT2B-12 (sobre el archivo nuevo de PB-T2b). W y WC siguen bajo CT-PBT1C-04, sin tocarla |
| 2. sin alias fuera de D2 | CT-PBT2A-03 y CT-PBT2B-04: el estado del borrador no comparte con S ni con la ventana nada mutable fuera de las dos referencias |
| 3, 5, 6 y 7 | Siguen arrastradas a PB-T4, que conecta el `DraftSession` del panel (A-2) |
| 8. el ajuste de `Name` | WORKSPACE no tiene ningún escritor del Plugin. CT-PBT1B-15 sigue en verde sin tocarla |
| El límite | Las paradas PBT2A-C17 y PBT2B-C17 |

### D-PBT2-15. Lo que no hace PB-T2

- «Editar todo» en el panel, el host, la modalidad, el texto pendiente y K = 1: PB-T4 (OB-01.4, R-PB-10).
- La escritura completa, la barrera de PREPARE y «Conservar mi borrador»: PB-T3 y PB-T4.
- El piloto, `WorkspaceHostCore`, el tipo del borrador del panel y el campo de escala del panel: PB-T4.
- El núcleo de borradores, WX, WC, el Plugin, `EditorModules`, los constructores actuales y los archivos de PB-T1b: no se tocan.
- Quitar los rechazos A/B o cola (OB-03.2) y AUTH-13.
- Cualquier corrección de RACKEDITAR (OB-RE.1).

### La tabla del §4 de [P], con el diseño de PB-T2

| | CLASSIC | WORKSPACE |
|---|---|---|
| Quién la abre | RACKEDITAR, RPB y el menú, con los constructores de hoy | Solo `RackPushBackSystemWindow.ForDraft(launch)`; en F4B lo llamará el «Editar todo» del panel (PB-T4) |
| De dónde carga | `LoadExisting`, `LoadDesignForNew` o `LoadNew` | Una copia del estado del borrador, sin pasar por un diseño. Las cargas públicas lanzan |
| Botón principal | «Actualizar» e «Insertar», sin cambios | «Aplicar al borrador». Insertar, los cuatro por vista e «Insertar varias…» no se muestran |
| Qué hace | Aplica al estado de edición (el paso extraído), anota la intención y cierra. Escribe quien llamó | Aplica al estado de edición (el mismo paso) y devuelve una copia al borrador por el puerto. No cierra. Ninguna escritura en el DWG |
| Cerrar sin aplicar | Como hoy: solo pregunta por el módulo escenificado | Pregunta si hay cambios sin aplicar (Q-PBT2-1). El borrador queda como estaba |
| BOM y biblioteca | Como hoy | Los mismos botones, con la misma compuerta y el mismo dueño. Dos salidas a disco aceptadas ([OPB7]) |
| Dueño de los subdiálogos | La ventana | La ventana. El dueño de la ventana lo decide su host (D-PBT2-12) |
| Guardas | T1B en verde | CT-PBT2B-03 (UI-STA de OB-01.3) y CT-PBT2B-11 (texto). PB-T4 añade la guarda del abridor |

## 4. Las guardas de T1B y de texto (OB-T1B.3), una por una

**Conclusión:** el diseño deja en verde **todas** las guardas sin sustituir ninguna. OB-T1B.3 no obliga a volver al Owner. Las marcadas [T1c] se vuelven a comprobar sobre el cierre de PB-T1c.

| Guarda | Qué fija | ¿La toca PB-T2? | Cómo queda en verde |
|---|---|---|---|
| T-SG CT-16 (`:217-231`) | El configurador con `{ Owner = this }` y `window.Configuration ?? copy` | No | `ShowHeaderConfigurator` no cambia |
| T-SG CT-19 (`:236-248`) | Sin tokens de teclado, foco ni `DialogResult` en `Systems/PushBack` | Sí (código nuevo y un archivo nuevo en la clausura) | Ningún token en el código nuevo ni en `PushBackEditorMode.cs` (INV-PBT2B-04) |
| T-SG `InsertRequestedRaised` (`:251-264`) | Nadie se suscribe | No | Ningún `+=` nuevo |
| T-SG S-5 y S-6 (`:269-278`) | Dueños de defensa y topes | No | No se tocan |
| **T-SG S-7** (`:281-284`) | La línea del BOM | **Sí** | La línea sigue, byte a byte, tras el ramal de la costura (D-PBT2-10) |
| T-SG S-8 (`:287-291`) | El lote con `TryShow(this, …)` | No | `InsertBatch_Click` solo gana la guarda en su primera sentencia |
| **T-SG S-9** (`:294-298`) | `UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")` | **Sí** | El literal sigue, en el segundo término de la condicional |
| T-SG CT-21 (`:303-319`) | El host del menú, dos veces | No | `EditorModules` no se toca |
| **T-SG CT-22** (`:324-341`) | El orden de las cinco sentencias de la biblioteca | **Sí** | `IndexOf("UiSupport.PromptSaveToLibrary(this,")` sigue antes de `EnsureId`, que no se mueve (con el `SyncName()` de PB-T1c) |
| T-SG CT-22b (`:343-352`) | El configurador guarda su disposición al cerrarse | No | No se toca |
| **T-SG CT-23** (`:357-379`) | `InsertBatch_Click`: confirmar antes del diálogo, `RequestInsertViews` después, `Close()` al final y sin `canInsertInAutoCad` | **Sí** | La guarda va primero, no contiene `canInsertInAutoCad` ni `Close();`, y la línea de confirmación sigue antes del diálogo |
| T-PG CT-24 a CT-27 | RPB, RACKEDITAR, el menú y las fachadas | No | El Plugin no se toca |
| T-KB, T-OW, T-RS, foco, `AnnotationScaleBox` | WX y los dueños en CLASSIC | No | WX no se toca. La ocultación es solo de WORKSPACE y en código |
| T-CL (`PushBackClassicCharacterizationCloseTests`) | Los caminos de cierre en CLASSIC | Sí (`PendingWork`) | En CLASSIC, `PendingWork` da lo mismo: la expresión del módulo es literal y el segundo ámbito es `false` |
| T-IN (`PushBackClassicCharacterizationIntentTests`) | La intención de «Actualizar» e «Insertar» en CLASSIC | Sí (`RequestDraw`) | La extracción de D-PBT2-06 conserva el orden, los textos y el efecto |
| **G-10** (`WorkspacePilotUpdateIntegrationGuardTests.cs:725-767`) | Una sola `.RequestUpdate(` en W, ninguna en otro archivo de `src/RackCad.UI` | Sí | W conserva la suya en `RequestDraw`. El método nuevo se llama `ApplyToDraft` |
| G9 (`:703-719`) | Ningún archivo que nombre el piloto usa una API de escritura | No | Los archivos nuevos no nombran el piloto |
| I-55 (`I55DeferredProductFixCharacterizationTests.cs:8-15`) | Dos literales de W | No | No se tocan |
| Fact3 (`PushBackModuleEditorCharacterizationTests.cs:381-401`) | W sin `forceRebuild` | No | Ninguna línea nueva lo contiene |
| Desviador (`BlankFrontDesviadorHandoffTests.cs:311-318`) | Dos literales de W | No | No se tocan |
| Reflexión de la vista previa (`PushBackRichPreviewTests.cs:349-367`) | Ni `DrawModel` ni `RoleBrush`; un `EditorPreviewSurface`; ningún `PreviewCanvasPainter` | Sí (métodos y campos nuevos) | Los métodos nuevos se llaman `ApplyToDraft`, `ApplyEditsToEditingState`, `ApplyModeChrome`, `RefuseInWorkspace` y `HasUnappliedDraftChanges`. Los campos nuevos son de tipo `PushBackEditorMode`, `IPushBackDraftStatePort`, `PushBackEditableState`, `Action<…>` y `Func<…>` |
| Censo de ventanas (`WindowCensusGuardTests.cs`) | Ningún `Window` nuevo | No | No se crea ninguno |
| Corpus de WLC (`WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:323-333`) | Ningún `pb-*.json` nuevo en `Fixtures/I64` | Restricción | Los diseños nuevos se hacen en código |
| RAM Elements (`StructuralAnalysisRamElementsHardeningGuardTests.cs:84-92`) | Los literales de `tests/RackCad.Tests` | Restricción | Las pruebas nuevas no parten ninguna palabra de ese dominio |
| Núcleo de borradores (`WorkspaceDraftBoundaryTests.cs`, `WorkspaceDraftPilotIntentGuardTests.cs:233-244`) | `Drafts/` sin sistemas, con sus archivos esperados y sin nombres prohibidos | Restricción | `Drafts/` no se toca. El puerto va en `Systems/PushBack` (D-PBT2-02) |
| Las del puerto del panel (`WorkspaceEditIntentHostTests.cs:1020-1060`, `WorkspaceLifecycleHostTests.cs:1470-1480`, `WorkspacePilotEditGuardTests.cs:385`) | Los valores del puerto del panel, sin tipos de `Drafts` ni de `Systems`; dos interfaces del host core | No | El puerto del panel y el host core no se tocan |
| Espacio de nombres = carpeta (`NamespaceFolderGuardTests`) | Un `namespace` de bloque por archivo de `src/` | Restricción | INV-PBT2A-04 e INV-PBT2B-04 |
| El censo del piloto (HT1-21 de [PT1]) | Todo campo de instancia de su grafo | Restricción | Ningún campo nuevo en los tipos del censo. Los campos nuevos son de W |
| `PushBackLibrarySaveGateTests` (`:95`) | La compuerta de la biblioteca, que llega al diálogo real en una ventana sin mostrar | Sí (`SaveLibrary_Click`) | La costura es nula en esas pruebas: la rama real y su excepción no cambian |
| `DialogWindowCharacterizationTests` (`:91-157`) | La ventana del BOM compartida | No | `RackBomWindow` no se toca |
| Las costuras de la ventana (INV-PBT1C-06: 40 + `EditableStateForTest`) [T1c] | Su nombre y su tipo | Sí (añade tres) | Se conservan las 41 y se añaden `ModeForTest`, `BomDialog` y `LibraryPathDialog` |
| CT-PBT1C-01 a 04 [T1c] | La ventana sobre el estado, el censo en la ventana, la forma de W y G-L1 sobre W y WC | Sí (W) | `editable` sigue declarado como en D-PBT1-13 (1), y `ReferenceEquals(EditableStateForTest.Editor, CompositeState)` vale en los dos modos. `LoadFromModel` y `ReadInputs` no cambian. Ninguna línea nueva muta las dos referencias. **A comprobar sobre el texto real de PB-T1c** (R-PBT2-09) |
| CT-PBT1B-14 y 15 [T1b-i2] | G-L1 sobre Application y los escritores de `Name` del Plugin | No | Los archivos de su lista no se tocan. Los nuevos tienen su propia guarda (D-PBT2-14) |

## 5. La red de pruebas

### 5.0 Recetas y ayudantes

**Régimen común** (copiado en cada contrato, como el de [PT1] §5.2):
- clases `public sealed`;
- en UI, `[Collection(EditorDiscardPromptCollection.Name)]`, en el hilo STA y con la cultura invariante (`S0.Sta`);
- `[Fact]` que acaban en **un** `Assert.Equal(lo esperado, las líneas)`;
- «R», GUID fijo, orden ordinal;
- la regla «EXCEPCION» de [PT1] §5.0: cada paso que llama al API nuevo pasa por un ayudante privado que convierte una excepción en `EXCEPCION <Tipo>: <Mensaje>`;
- ventanas **sin mostrar**, con la carga dentro del `try` y `Cerrar` en `finally`;
- el aviso de descarte siempre sustituido (`EditorDiscardPrompt.Substitute`), con el sufijo ` descartes=<n>` en la última línea de cada ventana, leído tras cerrarla (D-PBTC2-08);
- sin teclado ni foco real; sin `MessageBox` real, configurador real, «Insertar» real, ventana real del BOM ni diálogo real de guardado.

**Las recetas** (en `PushBackWorkspaceSupport`, `internal static`, sin pruebas, para PB-T2b; copiadas como privadas en las clases de PB-T2a):
- **`Catalogo`** = `JsonRackCatalogProvider.FromBaseDirectory().Load()`, como `PushBackEditableStateParityTests.cs:41`.
- **`Pasada(s, frontal)`:** el ayudante de [PT1] §5.0, copiado.
- **`Estado(sobre)`:** `var s = new PushBackEditableState(); s.LoadNew(cat); Pasada; s.SeedDefaultSafety(cat); Pasada; s.LoadDesign(<diseño del sobre>, <nombre del sobre>, resolver); Pasada`. Es la secuencia de la ventana de [PT1] §5.0. El diseño es `new RackProjectStore().Deserialize(<sobre>.Design).PushBackDesign`. **X-01 (Core)** y **F-01 (Core)** son las recetas de [PT1] §5.0.
- **`Borrador(s)`:** con `registry = new DraftRegistry()`, `registry.Begin(new SessionId(64), "GUID-PBT2-0001", DraftIntentRead<PushBackEditableState>.Admitted(DraftStaleBase.Create(new[] { new DraftStaleBaseEntry("BLK-PBT2", DraftMembership.Mutable, "planta", null, "{}") }), s), PushBackEditableStateComparer.Instance, PushBackEditableStateComparer.Copy)`; devuelve el registro y su `Draft` (CT-PBT2A-01 usa el registro para `Discard`).
- **`PuertoContado`:** un `IPushBackDraftStatePort` privado de la prueba que cuenta las llamadas y delega en `new PushBackDraftSessionPort(draft)`.
- **`VentanaBorrador(draft, proyecto, puerto)`:** `RackPushBackSystemWindow.ForDraft(new PushBackDraftEditorLaunch(draft.CurrentDraftState, draft.RackId, proyecto, puerto))`, con su propio registro de `Closed` (como `S0.Ventana`).
- **`Sondeo(w)`, la sonda del modo (obligatoria, R-PBT2-07):** la primera línea de cada prueba de WORKSPACE es `modo=<w.ModeForTest>`. Si no es `Workspace`, la prueba **no hace ningún gesto más** y termina con su `Assert.Equal`. Así el RED no abre ningún modal real ni escribe en el perfil.
- **`ProyectoConDesconocido(sobre)`:** el JSON del proyecto interior del sobre, con `"CampoFuturoPbt2": { "v": 1 }` en la raíz, deserializado con `new RackProjectStore().Deserialize(...)`.
- **`ArchivoTemporal`:** `Path.Combine(Path.GetTempPath(), "rackcad-pbt2-0001", "biblioteca.rackcad.json")`. La carpeta se crea antes del gesto y se borra en el `finally`. **Es la única escritura en disco de la red** (INV-PBT2B-08). Las líneas nunca llevan la ruta.
- **`Huella(objeto)`:** `Fingerprint` y `AllObjects` de `PushBackEditableStateCopyTests` (`:1039`, `:272`), copiados de la versión **cerrada** de PB-T1b [T1b-i2].
- **`Igual(a, b)`** = `PushBackEditableStateComparer.Instance.Equals(a, b)`. **`DisenoDe(estado, frontal)`** = `Persist(Pasada(Copy(estado), frontal).Design)`.
- **De S0 y SC1, sin tocarlos:** `Fixture`, `CorpusNames`, `EditInner`, `Persist`, `Field`, `Check`, `Combo`, `Btn`, `StatusText`, `Click`, `SetNumberR`, `FilasF`, `VarianteDe`, `Num`, `Texto`, `Cargar` y `Aviso`.

### 5.1 Lo que sigue en verde sin tocarlo

T1B (55 de UI y 10 de Core), PB-T0 (16 y 3, con SUS-03 y SUS-04 en `Skip`), PB-TC (10), PB-TC2 (4), PB-T1a (14), PB-T1b (Core 13 + 1 + 2 y UI 1), PB-T1c (UI 2 y Core 2) [T1c], las familias `PushBack*` de UI, `RackCad.Tests.Workspace` (3 271) y `RackCad.UI.Tests.Workspace` (547), Cantilever (67), `BlankFront` (33), `WindowCensusGuardTests` (8), `RichEditor` (25), `PushBackRichPreviewTests`, `PushBackLibrarySaveGateTests`, `DialogWindowCharacterizationTests`, `I55DeferredProductFixCharacterizationTests` (3) y `NamespaceFolderGuardTests` (7). En PB-T2b, también PB-T2a.

### 5.2 PB-T2a: el puerto (lista cerrada CT-PBT2A-01 a 04)

**Régimen:** el RED es el **esqueleto inerte** y las dos clases nuevas, que fallan por aserción salvo la guarda CT-PBT2A-04, que pasa en el RED y discrimina por sus autopruebas (INV-F4T3A-13). El GREEN solo toca los dos archivos de `src/`.

**El esqueleto exacto:**
- `PushBackDraftApplyStatus`, `PushBackDraftApplyResult` e `IPushBackDraftStatePort`, completos;
- `PushBackDraftSessionPort`: el constructor no guarda nada ni comprueba nada; `ApplyToDraft` devuelve `new PushBackDraftApplyResult(PushBackDraftApplyStatus.NotEditable, false)`; `LaunchFor` devuelve `null`;
- `PushBackDraftEditorLaunch`: el constructor no asigna nada ni comprueba nada.

**Las clases** (`tests/RackCad.Tests/`, `namespace RackCad.Tests`): `PushBackDraftStatePortTests` (01, 02 y 03) y `PushBackDraftPortSharedReferenceGuardTests` (04).

| CT | Qué fija (las líneas) | Falla en el RED porque |
|---|---|---|
| **01** `CT_PBT2A_01_ThePortHandsACopyOfTheEditedStateToTheDraftAndNothingElse` | Sobre `Borrador(Estado(Fixture("pb-01-base")))`, con `antes = Copy(draft.CurrentDraftState)`: `inicio sucio=False`; con `e = Copy(draft.CurrentDraftState)` y `e.PalletTolerance = 4.5`, `ApplyToDraft(e)` → `aplicar estado=Applied sucio=True igual-al-editado=True mismo-objeto=False inicial-intacto=True` (`inicial-intacto` = `Igual(draft.InitialDraftState, antes)`); tras `e.PalletTolerance = 9.0` → `mutar-tras-aplicar sin-cambio=True`; `ApplyToDraft(Copy(antes))` → `volver estado=Applied sucio=False`; con `draft.EnterError("prueba")`, `ApplyToDraft(e)` → `error estado=NotEditable sin-cambio=True`; en otro borrador descartado con `registry.Discard` → `descartado estado=NotEditable`; `new PushBackDraftSessionPort(null)` → `EXCEPCION ArgumentNullException: …` con el parámetro `draft`; `ApplyToDraft(null)` → con `edited` | el esqueleto devuelve `NotEditable` y no lanza |
| **02** `CT_PBT2A_02_TheLaunchCarriesTheStateTheIdentityTheSourceAndThePortAndRefusesWhatIsMissing` | `new PushBackDraftEditorLaunch(s, "GUID-PBT2-0001", p, puerto)` → `lanzamiento estado-mismo=True id=GUID-PBT2-0001 proyecto-mismo=True puerto-mismo=True`; con el proyecto nulo → `proyecto=<null>`; `state` nulo, `rackId` nulo, `""` y `"   "`, y `port` nulo → una línea `EXCEPCION <Tipo>: <Mensaje>` cada una; `LaunchFor(draft, p)` → `launchfor estado-es-el-actual=True id=GUID-PBT2-0001 puerto-es-del-borrador=True`, donde la última comprueba que aplicar por ese puerto cambia ese borrador; `LaunchFor(null, p)` → `EXCEPCION ArgumentNullException` | el esqueleto no guarda ni comprueba nada |
| **03** `CT_PBT2A_03_ThePortRoundTripIsLosslessAndSharesNothingOutsideTheTwoDeclaredReferences` | Para los 17 de `Fixtures/I64`, X-01 y F-01, en el orden de [PT1] CT-PBT1B-02: `s = Estado(…)`; `d = Borrador(s)`; `otro = Copy(s)` con `otro.PalletTolerance = s.PalletTolerance + 1.0`, puesto con `d.SetCurrentDraftState(otro)` (llamada directa al núcleo, para que el borrador parta de algo distinto de `s`); `ApplyToDraft(Copy(s))` por el puerto; y la línea `<diseño> iguales=<Igual(d.CurrentDraftState, s)> diseno-igual=<DisenoDe(d.CurrentDraftState, A) == DisenoDe(s, A)> compartidos-fuera-de-D2=<n>`. `n` cuenta los objetos mutables que `AllObjects` encuentra a la vez en `d.CurrentDraftState` y en `s`, quitados los no nulos de las cuatro referencias de D2 y lo que alcanzan, como CT-PBT1B-07, parte 1. Lo esperado: `iguales=True diseno-igual=True compartidos-fuera-de-D2=0` en los 19 | el esqueleto no aplica, así que el borrador sigue en `otro` (`iguales=False`) |
| **04** `CT_PBT2A_04_NoFileOfThePortMutatesTheResolvedRackOrTheModuleReport` | **D2, condición 10.** G-L1 con las fichas y las reglas de CT-PBT1B-14, **copiadas de la versión cerrada de PB-T1b** (con la regla 1b sobre los alias, I01) [T1b-i2], sobre los dos archivos nuevos. Las autopruebas (a), (b), (c), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i) y (j), inyectadas en el texto de esos archivos. Las líneas: `reglas-sin-coincidencias=True` y `autoprueba-<x>=falla` por cada una | **no falla en el RED** (`ExpectRed` false) |

### 5.3 PB-T2b: la ventana en dos modos (lista cerrada CT-PBT2B-01 a 13)

**Régimen:** el RED es la **costura inerte** y las clases nuevas.
- Las de UI fallan por aserción gracias a la sonda del modo.
- CT-PBT2B-11 falla porque sus reglas no se cumplen.
- CT-PBT2B-12 y 13 pasan en el RED y discriminan por sus autopruebas.

El GREEN toca solo W y el archivo del modo.

**La costura inerte exacta** (en W, y nada más de W):
- `internal PushBackEditorMode ModeForTest => PushBackEditorMode.Classic;`;
- `public static RackPushBackSystemWindow ForDraft(PushBackDraftEditorLaunch launch) => new RackPushBackSystemWindow(canInsertInAutoCad: false);`;
- `internal Action<RackCad.Application.Bom.BillOfMaterials, Window> BomDialog;` e `internal Func<Window, string, string> LibraryPathDialog;`, declaradas y sin usar;
- `PushBackEditorMode.cs`, completo.

**Las clases:**
- en `tests/RackCad.UI.Tests/`: `PushBackWorkspaceSupport` (ayudante), `PushBackWorkspaceModeTests` (01, 02 y 07), `PushBackWorkspaceApplyCloseTests` (03 a 06), `PushBackWorkspaceCarryoverTests` (08 a 10) y `PushBackFieldValidationConcordanceTests` (13);
- en `tests/RackCad.Tests/`: `PushBackWorkspaceModeSourceGuardTests` (11) y `PushBackWorkspaceSharedReferenceGuardTests` (12).

| CT | Qué fija | Falla en el RED porque |
|---|---|---|
| **01** `CT_PBT2B_01_TheModeIsExplicitAndFixedAndClassicDoesNotChange` | **(i) CLASSIC:** `S0.Ventana()` con `Cargar(pb-01-base)`, `new RackPushBackSystemWindow()` y `new RackPushBackSystemWindow(false, null)`: `clasico modo=Classic boton=Actualizar ocultos=[]`, con los seis botones de inserción en `Visible`. **(ii) WORKSPACE:** `VentanaBorrador` sobre `Estado(pb-01-base)`: `modo=Workspace boton=Aplicar al borrador tooltip=<WorkspaceApplyToolTip> ocultos=[InsertButton,InsertLateralButton,InsertFrontalEntradaButton,InsertFrontalPosteriorButton,InsertPlantaButton,InsertBatchButton] visibles=[BomButton,SaveLibraryButton,RestoreButton,CloseButton] dueno-de-la-ventana=<null>`. **(iii)** En WORKSPACE, `LoadExisting(…)`, `LoadDesignForNew(…)` y `LoadNew()` → `EXCEPCION InvalidOperationException: La ventana del borrador solo carga el estado del borrador.` cada una, y después `modo=Workspace`. **(iv)** `ForDraft(null)` → `EXCEPCION ArgumentNullException` | la sonda da `Classic` |
| **02** `CT_PBT2B_02_WorkspaceLoadsACopyOfTheDraftStateWithoutGoingThroughADesign` | Para los 17, X-01 y F-01: `<diseño> iguales=<Igual(w.EditableStateForTest, draft.CurrentDraftState)> mismo-objeto=<ReferenceEquals(…)> diseno-igual=<DisenoDe(draft.CurrentDraftState, A) == Persist(w.LastComputation.Design)> id=GUID-PBT2-0001 nombre=<RackName> peticion=False`. Lo esperado es `iguales=True mismo-objeto=False diseno-igual=True` en todos, **salvo** los diseños que el paso 1 identifique con seguridad no autorizada, que llevan `iguales=False normalizacion=seguridad diseno-igual=True` (N-PBT2-01 (b)). **Fila N-PBT2-01 (a):** `Estado(pb-01-base)` con `DimensionsLateral = false` y `DimensionViewsTouched = true` → `N-PBT2-01a iguales=False tocado=True->False vistas=<a>-><b> diseno-igual=True`, con `<a>` y `<b>` medidos en el paso 1 | la sonda da `Classic` |
| **07** `CT_PBT2B_07_InWorkspaceTheWindowOwnsTheBomAndTheSaveDialog` | En WORKSPACE, con las dos costuras puestas: «BOM» → `BomDialog llamadas=1 dueno-es-la-ventana=True bom-es-el-calculado=True` (`ReferenceEquals(bom, w.LastComputation.Bom)`); «Guardar en biblioteca», con la costura devolviendo `null` → `LibraryPathDialog llamadas=1 dueno-es-la-ventana=True nombre=<NameBox.Text> archivo=False peticion=False`. Los demás subdiálogos los fijan las guardas de T1B, que no dependen del modo | la sonda da `Classic` |
| **03** `CT_PBT2B_03_NoWorkspaceGestureFlagsAClassicRequestAndBomAndLibraryLeaveTheDraftAndItsIdentityAlone` | **La UI-STA de OB-01.3, con las cuatro comprobaciones de [OPB7].** En una ventana de WORKSPACE sobre `Estado(pb-01-base)`, con `ProyectoConDesconocido(pb-01-base)` y `PuertoContado`, se guarda `antes = Copy(draft.CurrentDraftState)` y se hacen los gestos en este orden. Las líneas: `carga modo=Workspace peticion=False solicitud=<null> borrador-igual=True sucio=False llamadas=0 id=GUID-PBT2-0001`; para cada uno de los seis botones de inserción, con `Click` aunque estén ocultos, `<botón> visible=Collapsed peticion=False solicitud=<null> estado=<WorkspaceRefusalMessage> borrador-igual=True llamadas=0`; `BomButton dialogos=1 dueno-es-la-ventana=True peticion=False borrador-igual=True llamadas=0`; con la costura devolviendo `ArchivoTemporal`, `SaveLibraryButton dialogos=1 archivo=True campo-desconocido=True guid-en-archivo=False id=GUID-PBT2-0001 peticion=False borrador-igual=True llamadas=0`; `RestoreButton peticion=False borrador-igual=True llamadas=0`; `SetNumberR(ToleranceBox, 4)` → `ToleranceBox=4 peticion=False borrador-igual=True llamadas=0`; otra vez el BOM (`dialogos=2`) y la biblioteca (`dialogos=2 archivo=True id=GUID-PBT2-0001`), con `borrador-igual=True llamadas=0`; `UpdateButton aplicado=True peticion=False solicitud=<null> borrador-igual=False sucio=True llamadas=1 id=GUID-PBT2-0001 abierta=True`; y `CloseButton avisos=0 cerrada=True peticion=False solicitud=<null> llamadas=1`. `campo-desconocido` y `guid-en-archivo` leen el texto del archivo (`"CampoFuturoPbt2"` y `GUID-PBT2-0001`) | la sonda da `Classic` |
| **04** `CT_PBT2B_04_ApplyToDraftHandsTheEditedStateToTheDraftOnceAndKeepsTheWindowOpen` | Sobre `Estado(pb-20-ab)`: `carga llamadas=0 sucio=False`; `SetNumberR(ToleranceBox, 4)` → `editar aplicar-habilitado=True tooltip=<WorkspaceApplyToolTip>`; «Aplicar» → `aplicar estado=<WorkspaceAppliedMessage> llamadas=1 sucio=True igual-a-la-ventana=True mismo-objeto=False compartidos-fuera-de-D2=0 diseno-igual=True abierta=True peticion=False`; «Aplicar» sin cambios → `otra-vez estado=<WorkspaceNothingToApplyMessage> llamadas=1`; `SetNumberR(PalletsDeepBox, 3)` y «Aplicar» → `segunda llamadas=2 igual-a-la-ventana=True diseno-igual=True`; `draft.EnterError("prueba")`, `SetNumberR(ToleranceBox, 5)` y «Aplicar» → `error estado=<WorkspaceNotEditableMessage> llamadas=3 sin-cambio=True abierta=True`. `diseno-igual` = `DisenoDe(draft.CurrentDraftState, w.FrontalSideForTest) == Persist(w.LastComputation.Design)` | la sonda da `Classic` |
| **05** `CT_PBT2B_05_WorkspaceAndClassicShareOneValidationAndNeitherMutatesTheSharedReferences` | **Una sola validación, y D2 (4).** Tres gestos, cada uno en una ventana CLASSIC nueva (`S0.Ventana()` + `Cargar`) y en una de WORKSPACE nueva, seguidos del clic en `UpdateButton`: **campo-en-error** (`pb-01-base`, `Field(ToleranceBox).Text = "abc"`); **calculo-invalido** (`pb-01-base`, el refuerzo de F-03: `AdvancedModulesToggle`, `DerivedPostReinforcedCheck` y `SetNumberR(DerivedPostReinforcementHeightBox, 100000)`); **bloqueante** (`pb-20-ab`, `w.CompositeState.SetGap(-1)` y un `LostFocus` elevado en `AnnotationScaleBox`, como CT-PBT1B-01). Por gesto, dos líneas: `<g> clasico valido=<CurrentInputsAreValid> bloqueado=<OutputIsBlockedForTest> estado=<StatusText> peticion=<InsertRequested>` y `<g> borrador valido=<…> bloqueado=<…> estado=<StatusText> llamadas=<n>`. Lo esperado (según Q-PBT2-2, opción A): `campo-en-error clasico valido=False bloqueado=False estado=Corrige los datos: no se puede insertar un modelo inválido. peticion=False` y `campo-en-error borrador valido=False bloqueado=False estado=<WorkspaceInvalidMessage> llamadas=0`; lo mismo para `calculo-invalido`; y `bloqueante clasico valido=True bloqueado=True estado=El hueco (-1") no es una separacion fisica valida: debe ser cero o positivo. Corrigelo; no se interpreta como otro valor. peticion=False`, con la línea de `borrador` con el mismo `estado` y `llamadas=0`. **Huellas:** de los no nulos de las cuatro referencias de D2 de `draft.CurrentDraftState` y de `draft.InitialDraftState`, capturadas al cargar, y de las que adopta la ventana tras cada pasada, cada una desde su adopción (como CT-PBT1C-02, parte 2), a lo largo de los tres gestos de WORKSPACE, del BOM, de la biblioteca (con la costura devolviendo `null`) y de una aplicación válida: `huellas-iguales=True` | la sonda da `Classic` |
| **06** `CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied` | **Según Q-PBT2-1, opción A.** Con `EditorDiscardPrompt.Substitute(probe)` (copia de `PromptProbe` de T-CL), en ventanas de WORKSPACE nuevas sobre `Estado(pb-01-base)` salvo la última: `cerrar-sin-cambios avisos=0 cerrada=True borrador-igual=True`; `SetNumberR(ToleranceBox, 4)` y «Cerrar» con «No» → `cerrar-con-cambios-no avisos=1 pregunta=<WorkspaceUnappliedQuestion> cerrada=False`; después «Cerrar» con «Sí» → `cerrar-con-cambios-si avisos=2 cerrada=True borrador-igual=True llamadas=0`; editar, «Aplicar» y «Cerrar» → `cerrar-tras-aplicar avisos=0 cerrada=True llamadas=1`; una edición de módulo escenificada (la receta `StageModuleEdit` de T-CL) y «Cerrar» con «Sí» → `cerrar-con-modulo avisos=1 pregunta=Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar? cerrada=True`; y en CLASSIC (`S0.Ventana()`, `Cargar(pb-01-base)`, `SetNumberR(ToleranceBox, 4)`, «Cerrar») → `clasico-con-cambios avisos=0 cerrada=True peticion=False` | la sonda da `Classic` |
| **08** `CT_PBT2B_08_InWorkspaceTheDraftKeepsTheExactValuesAndTheNamedStyle` | **El paso 4 de D-PB-06 en WORKSPACE.** Por fila de `SC1.FilasF()`: `Estado(VarianteDe(fila))`, una ventana de WORKSPACE, un gesto neutro (`Check(w, "DrawRackNameCheck").IsChecked = !…`) y «Aplicar» → `<Fn> WORKSPACE aplicado=True peticion=False diseno=<Num(DisenoDe(draft.CurrentDraftState, A), fila.Path)>`, con los valores de CT-PBTC-02 (i): el exacto en diez filas (F1 48.00044, F2 3.00044, F3 4.00044, F5 1.50044, F6 4.00044, F7 42.00044, F8 60.00044, F9 1000.00044, F10 6.00044 y F11 100.00044) y `6` en F4. `pb-08-redondeo` → `pb-08-redondeo WORKSPACE aplicado=True peticion=False AnnotationScale=1.23456 PalletHeight=61.23456 sistema.AnnotationScale=1.23456`, con `sistema` = `w.LastComputation.System.Structure`. `pb-06-seguridad` y `pb-07-combinado` → `<diseño> WORKSPACE aplicado=True diseno=<estilo del diseño del borrador> sistema=<estilo de LastComputation.System> cotas>0=True estilos=[<estilos de las cotas de LastComputation>]`, con STANDARD-ISO y SIMPLEX, como CT-PBTC-05 | la sonda da `Classic` |
| **09** `CT_PBT2B_09_InWorkspaceTheSideBNoticeShowsExactlyAsInRackeditar` | **El aviso de D-PB-08 en WORKSPACE.** Los gestos de CT-PBTC-07, copiados, en ventanas de WORKSPACE sobre `Estado(pb-01-base)` y `Estado(pb-20-ab)`: sus doce líneas literales (`pb-01-base carga aviso=Collapsed compuesto=False` … `pb-20-ab todas las ranuras de B en blanco aviso=Visible compuesto=False`). Y una línea más, sobre `pb-01-base`: declarar B y «Aplicar» → `pb-01-base declarar B y aplicar aviso=Visible aplicar-habilitado=True llamadas=1 borrador-B-presente=True diseno-del-borrador-compuesto=False` | la sonda da `Classic` |
| **10** `CT_PBT2B_10_InWorkspaceNoControlIsTheSourceOfTruthOnTheWayToTheDraft` | **El censo en WORKSPACE.** Por cada fila del censo (§5.4), el gesto de esa fila en CT-PBT1C-02, copiado de su clase cerrada [T1c], sobre el mismo diseño, en una ventana de WORKSPACE nueva, y «Aplicar» → `<fila> aplicado=<b> igual-a-la-ventana=True diseno-igual=True`. Lo esperado de `aplicado` (`True`, o `False` con `estado=<WorkspaceNothingToApplyMessage>` si el gesto no cambia el estado) se fija en la remedición sobre el cierre de PB-T1c, fila por fila | la sonda da `Classic` |
| **11** (Core) `CT_PBT2B_11_NoWorkspacePathReachesTheClassicWritePath` | **La guarda de texto.** Sobre W sin comentarios, con los espacios colapsados (como T-SG): **R1** el cuerpo de `private void RequestDraw(` empieza por `{ if (RefuseInWorkspace()) return;`; **R2** lo mismo en `private void InsertBatch_Click(`; **R3** los miembros cuyo cuerpo contiene `session.RequestUpdate(`, `session.RequestInsert(` o `session.RequestInsertViews(` son exactamente esos dos; **R4** el cuerpo de `private void ApplyToDraft(` no contiene `RequestUpdate(`, `RequestInsert`, `RequestDraw(`, `Close();`, `SetModel(`, `InsertionRequest`, `InsertRequested` ni `ShowDialog`, y contiene una vez `draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable))`; **R5** `private readonly PushBackEditorMode mode;` una vez, y `mode = PushBackEditorMode.Workspace;` una vez, dentro de `private RackPushBackSystemWindow(PushBackDraftEditorLaunch launch)`, sin otra asignación a `mode`; **R6** las dos firmas públicas de hoy, literales; `public static RackPushBackSystemWindow ForDraft(PushBackDraftEditorLaunch launch)` presente; ningún parámetro de tipo `PushBackEditorMode` ni ningún `bool` cuyo nombre contenga `workspace`, `draft` o `borrador` (sin distinguir mayúsculas); **R7** en `Bom_Click`, `if (BomDialog != null)` va antes de la línea S-7; en `SaveLibrary_Click`, la sentencia del literal S-9 empieza por `var path = LibraryPathDialog != null ? LibraryPathDialog(this, NameBox.Text) :`; **R8** los cuerpos de `LoadNew`, `LoadDesignForNew` y `LoadExisting` empiezan por `{ if (mode == PushBackEditorMode.Workspace) throw new InvalidOperationException(WorkspaceLoadRefusalMessage);`; **R9** el cuerpo de `Update_Click` es `{ if (mode == PushBackEditorMode.Workspace) { ApplyToDraft(); return; } RequestDraw(null, -1, updateOnly: true); }`; **R10** las once constantes de D-PBT2-09, cada una una vez y con su literal. **Autopruebas en negativo**, una por regla, sobre el texto real con una inyección (quitar la guarda de `RequestDraw`; quitar la de `InsertBatch_Click`; un método nuevo con `session.RequestUpdate(ctx => null);`; `session.RequestUpdate(` dentro de `ApplyToDraft`; `mode = PushBackEditorMode.Classic;` en `UpdateButtons`; `bool workspace = false` en un constructor; el ramal del BOM movido detrás de S-7; la guarda de `LoadExisting` quitada; `Update_Click` que llama a `RequestDraw` en WORKSPACE; un literal cambiado). Las líneas: `regla-R<n>=True` y `autoprueba-R<n>=falla` | las reglas no se cumplen en la base (`ExpectRed` true) |
| **12** (Core) `CT_PBT2B_12_TheNewFileOfTheModeMutatesNoSharedReference` | **D2, condición 10.** G-L1, con las reglas de la versión cerrada de PB-T1b [T1b-i2], sobre `src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs`, con sus trece autopruebas inyectadas en ese texto: `reglas-sin-coincidencias=True` y `autoprueba-<x>=falla` | **no falla en el RED** (`ExpectRed` false) |
| **13** (UI) `CT_PBT2B_13_ThePanelScaleFieldAndTheWindowScaleBoxAcceptAndRejectTheSameTexts` | **Según Q-PBT2-3, opción A.** Para los textos `""`, `"   "`, `"abc"`, `"0"`, `"-1"`, `"0,5"`, `"0.5"`, `"2.5"`, `"1e3"` y `"1.234,5"`, la línea `[<texto>] iguales=True`, donde se comparan `new PushBackPilotFieldAdapter().Validate(PushBackPilotFieldAdapter.AnnotationScaleKey, texto)` (`IsValid` y `Message`) y `NumericFieldValidation.Validate(texto, false, false, 0.0, null, false, true)` (`!HasError` y `Message`), en la cultura invariante. Y la autoprueba `autoprueba [0] inclusivo iguales=False` (la regla de la ventana con `minimumInclusive: true`) | **no falla en el RED** (`ExpectRed` false; es una concordancia de dos reglas que ya existen) |

**El paso 1 de PB-T2b, sobre `BaseSha` y antes del RED** (una corrida temporal de Core, fuera de todo commit, que termina en `Assert.Fail`):
- para cada diseño del corpus de CT-02, la línea `<diseño> seguridad-autorizada=<SequenceEqual estructural de s.SafetySelections y assembler.AuthorizedSafety(s.SafetySelections)>`, que decide qué rama lleva cada diseño en CT-02;
- la línea `N-PBT2-01a vistas=<s.LoadedDimensionViews>-><DimensionViewPolicy.FromEditor(s.LoadedDimensionViews, true, s.DimensionsFrontal, false, s.DimensionsPlanta)>`, que da `<a>` y `<b>`.

Se transcriben en la entrega. **Si un diseño del corpus no puede llevar ninguna de las dos ramas, es PBT2B-C15.**

### 5.4 El censo control → prueba, actualizado

La columna nueva es «WORKSPACE»: el camino del control al borrador. Las dos primeras columnas son las de [PT1] §5.5.

| Control | Su efecto en la salida | El estado lo lleva (PB-T1c) | WORKSPACE (PB-T2b) |
|---|---|---|---|
| `DepthBox`, `ToleranceBox`, `PostPeralteBox`, `BeamDepthBox`, `AnnotationScaleBox` | CT-PB0-19 (SUS-03), CT-PBTC-01 a 03, CT-PBT1A-07 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-08 (exactos) |
| `WeightUnitBox`, `PalletsDeepBox` | CT-PB0-17, CT-PB0-04, CT-PBT1A-02, CT-PBT1B-17 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-04 (`PalletsDeepBox`) |
| `PostBox` | CT-PB0-17, CT-PBT1B-03 | CT-PBT1C-02 | CT-PBT2B-10 |
| `NumberFrontsCheck`, `NumberLevelsCheck`, `DrawRackNameCheck`, `DimensionsBox`, las tres de cotas | CT-PB0-17, `PushBackDimensionViewsWindowTests`, CT-PBT1A-01 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-02 (N-PBT2-01a) |
| `PositionsBox`, `LevelsBox`, `FondosBox`, `DepthStartBox` | CT-PBT1A-03 | CT-PBT1C-02 | CT-PBT2B-10 |
| `FirstLevelHeightBox` y las cuatro de la celda | CT-PB0-19, CT-PBTC-01 a 03, CT-PBT1A-07 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-08 |
| `CellInOutBeamBox`, `CellInOutPeralteBox`, `CellIntermediateBeamBox` | CT-PB0-17 | CT-PBT1C-02 | CT-PBT2B-10 |
| `CellIntermediatePeralteBox`, `CellFondoOverrideBox`, `CellDrawPalletCheck` | CT-PBT1A-04 | CT-PBT1C-02 | CT-PBT2B-10 |
| `CellBeamLengthOverrideBox` | CT-PB0-19, CT-PBTC2-04, CT-PBT1A-10 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-08 (F11) |
| `RearPeralteBox` | CT-PB0-17 | CT-PBT1C-02 | CT-PBT2B-10 |
| `GapBox` | CT-PB0-20, CT-PBTC2-01 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-05 (bloqueante) |
| `CentralSeparatorCheck`, `StructureOverrideBox`, `CellTopologyBox`, `RunDirectionBox`, `TopologyScopeBox`, `FrontCountBox` | CT-PBT1A-05 | CT-PBT1C-02 | CT-PBT2B-10 |
| `NameBox` | CT-PB0-11 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-02 (`nombre`) |
| Las cajas avanzadas | CT-PB0-05 (SUS-04), CT-PBTC-14, CT-PBTC2-03 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-05 (refuerzo) |
| `ModuleLengthBox`, `DerivedPostLineHeightBox` | CT-PB0-03, CT-PB0-04, CT-PBTC2-02, CT-PBT1A-06 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-06 (módulo escenificado) |
| `SideBPresentCheck`, `SideSelectorBox`, `FrontalSideBox` | CT-PB0-06 a 09, CT-PBT1A-09, CT-PBT1A-14 | CT-PBT1C-02 | CT-PBT2B-10; CT-PBT2B-09 |
| **Los botones de la barra** (`UpdateButton`, los seis de inserción, `BomButton`, `SaveLibraryButton`, `RestoreButton`, `CloseButton`) | T1B (T-IN, T-CL), S-7, S-9, CT-22, CT-23 | — | CT-PBT2B-01, 03, 04, 06 y 07; CT-PBT2B-11 |

**Huecos que quedan:** ninguno en la ventana. El campo de escala del panel queda para PB-T4 (Q-PBT2-3).

### 5.5 Trazabilidad

| Obligación | CT |
|---|---|
| [OD] «Separate: "apply edits to current editing state" from "close editor" and from "write authored DWG state"» | D-PBT2-06 y D-PBT2-07; CT-PBT2B-04, 06 y 11 (R3, R4, R9) |
| [OD] «Workspace mode must return edited state to DraftSession» | CT-PBT2A-01 y 03; CT-PBT2B-04 y 10 |
| [OD] «Classic RACKEDITAR may continue its compatible existing outer workflow» | INV-PBT2B-05; §5.1; CT-PBT2B-01 (i) y 06 (`clasico-con-cambios`) |
| [OD] «Avoid two independent implementations of field validation» | D-PBT2-11; CT-PBT2B-05 y 13; Q-PBT2-3 |
| OB-01.1b (explícito, tipado, inmutable, constructores sin cambio) | CT-PBT2B-01; CT-PBT2B-11 (R5, R6, R8) |
| OB-01.2 (cero escrituras desde WORKSPACE) | CT-PBT2B-03; CT-PBT2B-11 (R1 a R4) |
| OB-01.3, la parte de PB-T2 | CT-PBT2B-03 |
| OB-01.5 | §5.1; INV-PBT2B-05 |
| OB-01.6 | CT-PBT2B-01; CT-PBT2B-11 (R10) |
| [OPB7], pregunta 1: las dos costuras y las cuatro comprobaciones | D-PBT2-10; CT-PBT2B-03 y 07; CT-PBT2B-11 (R7); S-7, S-9 y CT-22 en verde |
| OB-06.3 (el paso 4 en WORKSPACE) | CT-PBT2B-08 |
| OB-08.2 (el aviso en los dos modos) | CT-PBT2B-09 |
| D-15, punto 3 (ida y vuelta del estado sin la ventana) | CT-PBT2A-03; CT-PBT2B-02 |
| D-15, punto 4 (el dueño) | D-PBT2-12; CT-PBT2B-01 (`dueno-de-la-ventana`) y 07 |
| D-PB-03 por el camino de la ventana (módulos, cola y A/B) | CT-PBT2B-02 (X-01, F-01, `pb-20-ab`) |
| D2, condición 2 | CT-PBT2A-03; CT-PBT2B-04 |
| D2, condición 4 | CT-PBT2B-05 |
| D2, condición 10 | CT-PBT2A-04; CT-PBT2B-12; CT-PBT1C-04 sin tocarla |
| D2, el límite | PBT2A-C17 y PBT2B-C17 |
| A-1 de [PT1] §10 | todo lo anterior |

## 6. Los contratos propuestos

### 6.0 Común a las dos subtareas

**Identidad:** `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; attempts 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true. La evidencia va en `docs/automation/evidence/I-64-pilot/<TaskId>/`. **Los `TaskId`** tienen 26 y 31 caracteres (MAX_PATH, R-T1-07 de [PT1]).

**Anclajes:** antes de emitir cada subtarea, la sesión vuelve a medir sobre su `BaseSha`, con las órdenes del registro, y lo deja escrito en el commit de emisión:
- **PB-T2a:** `git diff <cierre de PB-T1c> <BaseSha> -- src` vacío; las anclas del registro G y H; la API de `DraftSession` y `DraftRegistry`; las reglas cerradas de G-L1 de PB-T1b (para CT-PBT2A-04); los recuentos y la línea base de la CI.
- **PB-T2b:** `git diff <cierre de PB-T2a> <BaseSha> -- src/RackCad.UI` vacío; **todas las anclas [T1c] del §2 y del §4 sobre el texto real de PB-T1c**, en particular la declaración de `editable`, el primer enunciado de `LoadFromModel`, `SyncName`, las 41 costuras y lo que fijan CT-PBT1C-01 a 04; la tabla de gestos de CT-PBT1C-02 para CT-PBT2B-10, con lo esperado de cada fila; la línea base de la CI.

**La comprobación acotada** (como [PT1] §6.0): un revisor independiente, en solo lectura, sobre este paquete, la sección de la subtarea y la salida de sus anclajes. Comprueba que los anclajes se sostienen, que ningún hecho nuevo cambia un CT o el diseño y que no aparece ninguna decisión material nueva. Su resultado, EMITIBLE o NO EMITIBLE, se custodia en `F4B-DRAFTS/pbt2/`.

**Custodia:**
- en el commit de emisión de PB-T2a: este paquete (en `docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md`), el registro, los dos guiones de medición, la revisión independiente, la comprobación acotada, las decisiones del Owner de Q-PBT2-1 a 4, [OPB7] con sus dos vueltas, y el contrato y su generador `make_pbt2a_contract.py` en su `coordinator/`;
- en el de PB-T2b: su contrato y su generador, la salida de sus anclajes y su comprobación acotada;
- en `f4b-dispositions.md`: [OPB7], la división (D-PBT2-01), Q-PBT2-1 a 4, cada emisión y los arrastres del §10.

**La delegación (F4B-E21):** el Controller deja vacías las ocho colecciones opacas, y la sesión las pone byte a byte desde el contrato. La comparación byte a byte, A1-A8 y nc4 se hacen sobre la delegación final.

**`ForbiddenWriteScope`:** con el método de [PKC-C], calculado por el generador de cada contrato con `assert` (registro N):
- **`src/`:** 87 en PB-T2a y 60 en PB-T2b (medido en `2f5d3dec`);
- **`tests/`:** la lista curada de PB-T0 (18 entradas, con sus tres clases de Core); en `tests/RackCad.UI.Tests/`, la lista curada de [PKC2] (51 archivos y `Workspace/`), las clases de PB-TC2, PB-T1a, PB-T1b y PB-T1c y, en PB-T2b, las de PB-T2a;
- **la raíz:** las 17 entradas de [PKC2].

**`StopConditions` estándar** (17): S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 = «config.toml no longer matches the accepted Snapshot in force (today Snapshot J, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-J/snapshot-j.json)».

**Las de [DP], literales en cada contrato** (x = A o B): **PBT2x-C08**, **PBT2x-C11** y **PBT2x-C12**, con el texto de [PT1] §6.0 y sus fuentes, cambiando solo el prefijo. Ninguna aceptación puntual anterior de un control se extiende a PB-T2 (R-T1-08 de [PT1]).

**La parada del límite de D2, en cada contrato:** **PBT2x-C17**, con el texto de [PT1] §6.0 y el literal del Owner. El tiempo de vida incluye el de la ventana de WORKSPACE, que vive dentro del del borrador (L-5 de [PT1]).

**El RED de la cadena en una corrección (§16.8):** una corrección que toque `RT` ∪ `ChainRedFiles` exige otro RED. En PB-T2a vuelve a poner el esqueleto; en PB-T2b, la costura inerte. **Lección de PB-T1b (§16.3):** la delegación de corrección lleva una `AuthorityRevision` que contiene los commits de documentación de la sesión, para que `git diff --name-only AuthorityRevision BaseSha` no tenga rutas `EXTERNAL` ni con secciones `UNIT_CHANGE`.

**La CI del RED (lección M14 de PB-T1b):** en los dos RED, Core falla (por CT-PBT2A-01 a 03, o por CT-PBT2B-11), así que `build-plugin` sale **omitido**, por su dependencia de `tests` (registro O). `ExpectedEvidence` lo dice así. `RedPart` es `pass` con `tests` en `failure`, `build-ui` en `success` y `build-plugin` omitido; `ui-tests` sale en `success` en PB-T2a y en `failure` en PB-T2b.

**`EligibleCells`:** las de [PKC2] y [PT1]. Worker `anthropic/subagent/claude-sonnet-5-5` con effort `high`; Controller `openai/process/gpt-6-luna` con effort `high`. Vencen el **2026-12-29** (`model-catalog.md:14`, `:51`, `:97`). Se vuelven a comprobar en cada emisión y delegación (A7). nc4 va en la primera delegación de cada cadena; nc1 a nc3, tras la verificación.

**Invariantes comunes** (cada contrato los numera INV-PBT2x-07 a 12 y 14):
- **07. Las redes:** las del §5.1, las guardas del §4, el censo del piloto y las subtareas anteriores, en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra.
- **08. Las pruebas de UI:**
  - el régimen del §5.0 y la sonda del modo antes de cualquier gesto de WORKSPACE;
  - el BOM y «Guardar en biblioteca» **solo** por las costuras nuevas;
  - la única escritura en disco es `ArchivoTemporal`, borrado en el `finally`;
  - nunca `PromptSaveToLibrary`, un `RackBomWindow` mostrado, un `MessageBox` real, el configurador real ni «Insertar» real;
  - ninguna aserción dentro de una costura;
  - ningún fixture nuevo en `Fixtures/I64`.
- **09. Determinismo:** el GUID y la sesión fijos (`GUID-PBT2-0001`, `SessionId(64)`), la cultura invariante, «R» y el orden ordinal; ni hora ni aleatoriedad.
- **10. Las corridas locales:**
  - en UI, solo los filtros del contrato, **nunca la suite completa de UI en local**; la de Core, sí;
  - sin copias de árboles (MAX_PATH);
  - las corridas temporales terminan en `Assert.Fail(string.Join("\n", lineas))`, corren dentro de `EditorDiscardPrompt.Substitute` (IRC-M02), no entran en ningún commit y guardan TRX y log como artefactos;
  - el perfil no cambia.
- **11. La evidencia:** rutas relativas comprobadas en `worker-handoff.json`; `TreeSha` real (`git write-tree`) o null; el Worker no escribe en `docs/`; una sustitución se llamaría `SUS4A-0n` o `SUS4B-0n` y es C02.
- **12. Las reglas de F4B, y sin AutoCAD.** La verificación lleva las tres comprobaciones deterministas de F4B-E15.
- **14. Las dos referencias de D2:** ninguna copia nueva comparte un objeto mutable fuera de las dos referencias declaradas de cada lado, y ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado.

**Las propias comunes** (con el prefijo de cada contrato):
- **C01:** hacerlo bien exigiría escribir fuera del `AllowedWriteScope`.
- **C02:** habría que editar, saltar, reactivar o borrar una prueba o un ayudante existente, o tocar `InternalsVisibleTo`, los paquetes, un `.csproj` o `Fixtures/I64`. Una guarda del §4 que no queda en verde **vuelve al Owner** (OB-T1B.3).
- **C03:** aparece un cambio observable de RACKEDITAR, RPB o el menú frente a la base. STOP y vuelta al Owner (D-16, OB-RE.1).
- **C05:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, la ventana real del BOM, el diálogo real de guardado, «Insertar» real, el teclado o el foco real, o escribir en el perfil o en el disco fuera de `ArchivoTemporal`.
- **C06:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce y no es atribuible a ellas.
- **C07:** los hechos del §2 no se sostienen de un modo que cambia un CT o el diseño, o una línea esperada no se cumple en el GREEN. El Worker no ajusta lo esperado.
- **C09:** una prueba colgaría, dejaría viva una ventana, un modal o un dispatcher, escribiría en el perfil o dependería de rutas de una máquina.
- **C10:** hacerlo bien exigiría corregir algo fuera del alcance: el piloto, el panel, el host core, «Editar todo», la escritura del panel, el campo de escala del panel, una pérdida de T-2, M-07, `pb-21-cola`, AUTH-13 o una corrección de PB-TC o PB-TC2.

### 6.1 Contrato de PB-T2a — `F4B-PBT2A-DRAFT-STATE-PORT`

- **Objective** (ASCII): «Crear en Application el puerto por el que el editor completo de Push Back en modo WORKSPACE devuelve su estado editado al borrador (IPushBackDraftStatePort, PushBackDraftApplyResult y el adaptador PushBackDraftSessionPort sobre DraftSession<PushBackEditableState>) y el lanzamiento tipado de ese modo (PushBackDraftEditorLaunch), segun el paquete del contrato de PB-T2 v<n> y la lista cerrada CT-PBT2A-01..04. El RED es el esqueleto inerte y las pruebas nuevas, que fallan por asercion salvo la guarda G-L1; el GREEN implementa solo los dos archivos nuevos de Application. La ventana, el piloto, el host core, el nucleo de borradores, el Plugin y los archivos de PB-T1b no cambian; no se ejecuta AutoCAD. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1 (decision D2 del Owner, F4B-E11).»
- **`AllowedWriteScope`** (4): `src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs`, `src/RackCad.Application/Systems/PushBack/PushBackDraftEditorLaunch.cs`, `tests/RackCad.Tests/PushBackDraftStatePortTests.cs` y `tests/RackCad.Tests/PushBackDraftPortSharedReferenceGuardTests.cs`.
- **`ForbiddenWriteScope`:** 87 de `src/` y lo común del §6.0.
- **`RequiredTests`:**

  | Proyecto | Filtro (`FullyQualifiedName~RackCad.<proyecto>.`…) | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackDraftStatePortTests` | **3** | true |
  | Core | `PushBackDraftPortSharedReferenceGuardTests` | **1** | false |
  | Core | `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackSharedReferenceGuardTests` (PB-T1b) | 13, 1, 2 | false |
  | Core | `PushBackWindowOnStateSourceGuardTests`, `PushBackWindowSharedReferenceGuardTests` (PB-T1c) [T1c] | 1, 1 | false |
  | Core | `Workspace`, `NamespaceFolderGuardTests`, `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization` | 3 271, 7, 10, 3 | false |
  | UI | `PushBackWindowOnStateTests`, `PushBackEditableStateParityTests`, `PushBackStateNet` | 2, 1, 14 | false |
  | UI | `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackDeclaredCorrection` | 55, 16, 14 | false |
  | UI | `Cantilever` | 67 | false |

- **Invariantes propios:**
  - **INV-PBT2A-01. Alcance:** el diff dentro de las 4 rutas; en `src/`, solo los dos archivos nuevos. `git diff BaseSha..CurrentSha -- src/RackCad.UI src/RackCad.Plugin src/RackCad.Domain src/RackCad.Application/Workspace src/RackCad.Application/Systems/PushBack/Pilot` vacío, y ningún archivo existente de `src/RackCad.Application/Systems/PushBack/` cambia.
  - **INV-PBT2A-02. RED y GREEN:** el RED es el esqueleto exacto del §5.2 y las dos clases. El GREEN solo toca los dos archivos de `src/`. El criterio del RED, como INV-PBT1B-02: el filtro con `ExpectRed` da `Failed == Selected`, todos por aserción. La guarda pasa en el RED y en el GREEN, y sus autopruebas fallan sobre el texto inyectado.
  - **INV-PBT2A-03. La API:** exactamente la de D-PBT2-02 y D-PBT2-03. Ningún otro miembro público ni interno. **Lista cerrada de miembros privados:** el campo `readonly` del borrador en el adaptador y los campos de respaldo de las propiedades. Cualquier otro ayudante privado de producto es PBT2A-C13 (la lección M10).
  - **INV-PBT2A-04.** Ningún campo estático. Un solo `namespace RackCad.Application.Systems.PushBack` de bloque por archivo. Las pruebas, `namespace RackCad.Tests`.
  - **INV-PBT2A-05.** Los archivos nuevos de `src/` no contienen `RackCad.UI`, `System.Windows`, `Autodesk`, `Pilot`, `RackSystemKind`, `.RequestUpdate(`, `CrossBoundary`, `TryBeginApply`, `CompleteApply`, `DiscardAndReload`, `KeepMineOnCurrentVersion`, `EnterError`, `MarkPossiblyStale` ni `SetPendingText`.
  - **INV-PBT2A-06. Sin copia en el adaptador:** `ApplyToDraft` hace una sola llamada al borrador, `SetCurrentDraftState(edited)`.
- **`StopConditions`:** las 17 estándar, C08, C11, C12, C17, las comunes, y:
  - **PBT2A-C04:** un método de un filtro con `ExpectRed` pasa sobre el esqueleto, o falla por excepción y no por aserción.
  - **PBT2A-C13:** la implementación necesitaría tocar el núcleo de borradores, un archivo de PB-T1b, el piloto o el host core, o un miembro fuera de INV-PBT2A-03.
  - **PBT2A-C15:** CT-PBT2A-03 da `iguales=False`, `diseno-igual=False` o algo compartido fuera de D2. Es un hecho nuevo de PB-T1b: STOP y vuelta al Owner (OB-06.5).
- **`ExpectedEvidence`:** como el §6.2 de [PT1]:
  - antes del Worker: el contrato, la delegación (con las colecciones de F4B-E21), la comparación byte a byte, A1-A8 y nc4;
  - el diff del esqueleto y el RED con su CI (`tests` en `failure`, `ui-tests` y `build-ui` en `success`, `build-plugin` omitido, sin cuelgue);
  - la tabla «CT → la aserción que falla en el RED» y la salida de las autopruebas de la guarda;
  - el GREEN con su CI 4/4;
  - en local, el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED y el GREEN y tres corridas en verde del filtro con `ExpectRed` sobre el GREEN;
  - la entrega con el mapa CT → método y las rutas relativas comprobadas;
  - la verificación con las tres comprobaciones de F4B-E15, y nc1 a nc3.
- **Línea base de la CI:** la del cierre de PB-T1c. **Prevista** (registro M): Core 19 088 y UI 2 439, con 21 `Skipped`. **GREEN de PB-T2a:** Core + 4 = **19 092**; UI sin cambio, **2 439**. Se mide al emitir.

### 6.2 Contrato de PB-T2b — `F4B-PBT2B-WORKSPACE-MODE-WINDOW`

- **Objective** (ASCII): «Dar a la ventana de Push Back sus dos modos explicitos: CLASSIC sin cambio observable, y WORKSPACE solo por la factoria tipada ForDraft. Separar aplicar al estado de edicion, aplicar al borrador, cerrar y la escritura clasica. En WORKSPACE la ventana carga una copia del estado del borrador; Aplicar al borrador devuelve una copia por el puerto de PB-T2a y no cierra; Insertar y el lote no se muestran y ninguna ruta llega a la peticion clasica; BOM y Guardar en biblioteca siguen, con dos costuras de prueba nuevas; la validacion de campos es la misma en los dos modos. Todo segun el paquete del contrato de PB-T2 v<n>, con las decisiones del Owner de D-PB-07 y Q-PBT2-1..4, y la lista cerrada CT-PBT2B-01..13. El RED es la costura inerte y las pruebas nuevas; el GREEN toca solo W y el archivo nuevo del modo. El XAML, WC, Application, el Plugin, el piloto y los constructores actuales no cambian; no se ejecuta AutoCAD. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1 (decision D2 del Owner, F4B-E11).»
- **`AllowedWriteScope`** (9): W; `src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs`; `tests/RackCad.UI.Tests/PushBackWorkspaceSupport.cs`, `PushBackWorkspaceModeTests.cs`, `PushBackWorkspaceApplyCloseTests.cs`, `PushBackWorkspaceCarryoverTests.cs` y `PushBackFieldValidationConcordanceTests.cs`; `tests/RackCad.Tests/PushBackWorkspaceModeSourceGuardTests.cs` y `PushBackWorkspaceSharedReferenceGuardTests.cs`.
- **`ForbiddenWriteScope`:** 60 de `src/` (WC y WX incluidos) y lo común.
- **`RequiredTests`** (sin un filtro global `PushBack` de UI, para que ningún filtro sin `ExpectRed` contenga una clase nueva):

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | UI | `PushBackWorkspace` (CT-01 a 10) | **10** | true |
  | UI | `PushBackFieldValidationConcordanceTests` (CT-13) | **1** | false |
  | Core | `PushBackWorkspaceModeSourceGuardTests` (CT-11) | **1** | true |
  | Core | `PushBackWorkspaceSharedReferenceGuardTests` (CT-12) | **1** | false |
  | Core | `PushBackDraftStatePortTests`, `PushBackDraftPortSharedReferenceGuardTests` (PB-T2a) | 3, 1 | false |
  | Core | `PushBackWindowOnStateSourceGuardTests`, `PushBackWindowSharedReferenceGuardTests`, `PushBackSharedReferenceGuardTests` [T1c] | 1, 1, 2 | false |
  | Core | `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackModuleEditorCharacterizationTests`, `I55DeferredProductFixCharacterizationTests`, `NamespaceFolderGuardTests`, `Workspace` | 13, 1, 10, 3, 15, 3, 7, 3 271 | false |
  | UI | `PushBackWindowOnStateTests`, `PushBackEditableStateParityTests`, `PushBackStateNet` | 2, 1, 14 | false |
  | UI | `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackDeclaredCorrection` | 55, 16, 14 | false |
  | UI | `PushBackCompositeWindowTests`, `PushBackCompositeLifecycleWiringTests`, `PushBackCompositeLoadExistingModulesTests`, `PushBackBothModeCellBeamOverrideTests`, `PushBackModuleEditorWindowTests`, `PushBackAdvancedRackParametersWindowTests` | 52, 7, 3, 10, 13, 9 | false |
  | UI | `PushBackRichPreviewTests`, `PushBackLibrarySaveGateTests`, `DialogWindowCharacterizationTests` | los medidos en la emisión | false |
  | UI | `BlankFront`, `WindowCensusGuardTests`, `RichEditor`, `Workspace`, `Cantilever` | 33, 8, 25, 547, 67 | false |

- **Invariantes propios:**
  - **INV-PBT2B-01. Alcance:** dentro de las 9 rutas, y en `src/` solo W y el archivo del modo. `git diff BaseSha..CurrentSha -- src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin src/RackCad.UI/Editor src/RackCad.UI/Controls src/RackCad.UI/Workspace src/RackCad.UI/Shell` vacío.
  - **INV-PBT2B-02. RED y GREEN:** el RED son las clases nuevas y, en W, solo la costura inerte del §5.3. El GREEN solo toca W y el archivo del modo. El criterio del RED, como INV-PBT1A-02, con `Failed == Selected` en cada filtro con `ExpectRed`. CT-12 y CT-13 pasan en el RED y en el GREEN, y sus autopruebas fallan sobre lo inyectado.
  - **INV-PBT2B-03. Dónde cambia W (lista cerrada, la lección M10):** cada hunk de `git diff -U0 BaseSha..CurrentSha -- W` cae, por su rango en `BaseSha`, en un miembro de esta lista:
    - los modificados: `LoadNew`, `LoadDesignForNew`, `LoadExisting`, `Update_Click`, `InsertBatch_Click`, `RequestDraw`, `PendingWork`, `Bom_Click`, `SaveLibrary_Click` y `UpdateButtons`;
    - **o** es una declaración nueva de esta lista, la única admitida: los campos `mode`, `draftPort`, `appliedState`, `BomDialog` y `LibraryPathDialog`; `ModeForTest`; `ForDraft`; el constructor privado; los ayudantes privados `ApplyModeChrome`, `RefuseInWorkspace`, `ApplyEditsToEditingState`, `ApplyToDraft` y `HasUnappliedDraftChanges`; y las once constantes de D-PBT2-09.

    Cualquier otro miembro o ayudante es PBT2B-C13.
  - **INV-PBT2B-04. Los literales y las cuentas, sobre `CurrentSha`:**
    - S-7, S-9, las cinco sentencias de CT-22 en su orden y los literales de CT-23 siguen byte a byte;
    - el texto del módulo de `PendingWork` sigue;
    - las dos firmas públicas de los constructores no cambian;
    - W tiene una sola `.RequestUpdate(`;
    - ninguna línea añadida de W o del archivo del modo contiene `forceRebuild`, `KeyDown`, `PreviewKeyDown`, `InputBindings`, `KeyBinding`, `TabIndex`, `IsTabStop`, `KeyboardNavigation`, `.Focus(` ni `DialogResult`;
    - ninguna línea con `SetNumber(` se añade ni se quita;
    - `LoadFromModel`, `ReadInputs`, `Recompute` y `OnClosing` no cambian;
    - el archivo del modo declara un solo `namespace RackCad.UI.Systems.PushBack` de bloque.
  - **INV-PBT2B-05. CLASSIC sin cambio:** T1B, PB-T0, PB-TC, PB-TC2, PB-T1a (con las huellas de CT-PBT1A-14 y F-03), PB-T1b y PB-T1c siguen en verde, en local y en la CI.
  - **INV-PBT2B-06. Las costuras:** las 41 de hoy [T1c] con su nombre y su tipo; solo se añaden `ModeForTest`, `BomDialog` y `LibraryPathDialog`.
  - **INV-PBT2B-07c. D2:** CT-PBT2B-05 y 12 en verde. CT-PBT1C-02 y 04 y CT-PBT1B-14 y 15 siguen en verde sin tocarlas. Ninguna línea añadida asigna el `Name` de un sistema ni muta una línea base o un informe.
  - **INV-PBT2B-08. La única escritura en disco** de la red es `ArchivoTemporal`. Ninguna prueba llama a `PromptSaveToLibrary`, ni muestra un `RackBomWindow`, ni toca el perfil.
- **`StopConditions`:** las 17 estándar, C08, C11, C12, C17, las comunes, y:
  - **PBT2B-C04:** un método de un filtro con `ExpectRed` pasa sobre la costura inerte, o falla por excepción.
  - **PBT2B-C13:** mantener un gesto de CLASSIC sin cambio, o hacer el de WORKSPACE, exigiría tocar un miembro fuera de INV-PBT2B-03, WX, WC, Application o el Plugin, o cambiar `LoadFromModel`, `ReadInputs`, `Recompute` o el orden de una carga.
  - **PBT2B-C14:** cambia una huella de CT-PBT1A-14 o falla una prueba de CLASSIC. STOP (C03); el Worker no ajusta ni la huella ni el producto.
  - **PBT2B-C15:** CT-PBT2B-02 da una diferencia fuera de N-PBT2-01, `diseno-igual=False`, o un diseño sin rama en el paso 1; o CT-PBT2B-08 no da el exacto o el estilo. Es un hecho nuevo: vuelve al Owner (OB-06.5, D-PB-03).
  - **PBT2B-C16:** un gesto necesitaría un modal real, el perfil o el disco fuera de `ArchivoTemporal`, o la sonda del modo no basta para evitarlo.
  - **PBT2B-C18:** el texto de PB-T1c impide el diseño. Por ejemplo, una guarda de PB-T1c prohíbe reasignar `editable` en un constructor, o la tabla de gestos de CT-PBT1C-02 no se puede copiar. La sesión decide antes de emitir con el plan B de R-PBT2-09; si ocurre durante el Worker, STOP.
- **`ExpectedEvidence`:** como el §6.1, y además:
  - el paso 1 (las ramas de CT-02 y los valores de N-PBT2-01a);
  - el RED con su CI (`tests` y `ui-tests` en `failure`, `build-ui` en `success`, `build-plugin` omitido);
  - la salida de INV-PBT2B-03 y 04 sobre el diff;
  - las autopruebas de CT-11 a 13;
  - en local, la clase de huellas de PB-T1a tres veces en verde sobre el GREEN.
- **Línea base de la CI:** la del cierre de PB-T2a, **prevista** en Core 19 092 y UI 2 439. **GREEN de PB-T2b:** Core + 2 (CT-11 y CT-12) = **19 094**; UI + 11 (CT-01 a 10 y 13) = **2 450**, con 21 `Skipped` y 2 429 `Passed`. Si Q-PBT2-3 no es la opción A, UI + 10 = 2 449.

## 7. Riesgos y lecciones aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-PBT2-01 | **Sin latitud para el Worker** | Las APIs, los ayudantes, el cuerpo del constructor, los literales, las líneas esperadas y las ramas del paso 1 están en el paquete; las listas de miembros son cerradas |
| R-PBT2-02 | **Corridas temporales con `Assert.Fail`** | INV-10; el paso 1 de PB-T2b |
| R-PBT2-03 | **El aviso de descarte sustituido** | §5.0 e INV-08; `PromptProbe` en CT-06 |
| R-PBT2-04 | **Nunca la suite completa de UI en local** | INV-10 |
| R-PBT2-05 | **MAX_PATH** | `TaskId` de 26 y 31 caracteres; `ArchivoTemporal` corto; sin copias de árboles |
| R-PBT2-06 | **El tamaño del contrato frente a la salida del Controller** (F4B-E21) | La sesión pone las ocho colecciones opacas. PB-T2a es pequeño (4 rutas, unas 25 autoridades); PB-T2b, de un tamaño parecido al de PB-T1c más sus 13 CT. Se mide el tamaño al generar |
| R-PBT2-07 | **Un modal real en el RED colgaría la CI** (la suite de UI usa un hilo STA y un `MessageBox` real por defecto, `ci.yml:213-221`) | Ventanas sin mostrar (un dueño sin mostrar lanza en vez de abrir); las dos costuras; la sonda del modo antes de cualquier gesto; `--blame-hang`; PBT2B-C16 |
| R-PBT2-08 | **Escribir en el perfil:** `PromptSaveToLibrary` crea la carpeta de biblioteca (`UiSupport.cs:66-68`) | Ninguna prueba la llama (la costura y la sonda); INV-PBT2B-08 |
| R-PBT2-09 | **PB-T1c no está emitida** | Todas las anclas [T1c] se vuelven a medir. **Plan B**, si PB-T1c impide reasignar `editable`: PB-T2a añade a Application `PushBackEditableStateComparer.CopyInto(source, target)` (la copia en sitio sobre un estado ya usado, con la cola y la autoridad limpiadas antes) y la ventana la usa en lugar de reasignar. Cambia el alcance de PB-T2a y vuelve a la comprobación acotada (C18) |
| R-PBT2-10 | **El punto fijo de la carga** (W0 = S salvo N-PBT2-01) se razona por lectura | CT-PBT2B-02 lo mide en la CI; el paso 1 fija las ramas; PBT2B-C15 |
| R-PBT2-11 | **El coste de `Equals`** (M15 de PB-T1b) en «Aplicar» y «Cerrar» | Una o dos comparaciones por clic. Se mide en Smoke-PB; el `IsDirty` caliente es de PB-T4 |
| R-PBT2-12 | **Una lista cerrada de miembros sin sus ayudantes** (M10 de PB-T1b) | INV-PBT2A-03 e INV-PBT2B-03 nombran los ayudantes privados; cualquier otro es C13 |
| R-PBT2-13 | **La CI del RED con Core en `failure`** (M14) | §6.0: `build-plugin` omitido en los dos RED |
| R-PBT2-14 | **Una corrección con rutas `UNIT_CHANGE` ya cambiadas en la base** (§16.3) | §6.0: la `AuthorityRevision` de la corrección contiene los commits de la sesión |
| R-PBT2-15 | **nc1 no satisface el sustituto**, como en PB-TC y PB-TC2 | C11 al Owner; ninguna aceptación anterior se extiende |
| R-PBT2-16 | **Las celdas vencen el 2026-12-29**, con PB-T1c y dos cadenas por delante | A7 en cada emisión y delegación |
| R-PBT2-17 | **La validación del campo de escala del panel sigue duplicada** hasta PB-T4 | CT-PBT2B-13 detecta una divergencia; Q-PBT2-3; arrastre a PB-T4 |
| R-PBT2-18 | **«Restaurar» en WORKSPACE** vuelve al último cálculo válido de la ventana, que con un estado inválido sería el de `LoadNew` | Igual que RACKEDITAR con un diseño inválido. Los borradores del panel nacen de lecturas admitidas (PB-T4). Se declara (L-PBT2-9) |
| R-PBT2-19 | **`FrontalSideBox` queda sola** en la barra de WORKSPACE de un compuesto | No escribe y elige el lado de la vista previa; L-PBT2-4. La revisión puede pedir ocultarlo (sería otra línea de `ApplyModeChrome`) |
| R-PBT2-20 | **El modelo paralelo interino** (la carga del piloto, A-5) sigue hasta PB-T4 | Sin cambio en PB-T2 |

## 8. Las decisiones del Owner

### 8.1 Ya tomadas, que este paquete aplica

- **[OD], fila de PB-T2** (literal, al principio) → D-PBT2-06, D-PBT2-07 y D-PBT2-11.
- **[OD] D-PB-01:** «The mode must be explicit», «Do not let a Workspace invocation accidentally execute the classic drawing-write path» y «If changing the button label in Workspace mode improves clarity, that is allowed only if the package declares it and RACKEDITAR mode remains unchanged» → D-PBT2-04, D-PBT2-08 y D-PBT2-09.
- **[OPB7], pregunta 1** (2026-11-19, literal: «BOM y biblioteca (Recomendado)») → D-PBT2-10, CT-PBT2B-03 y 07. Las cuatro comprobaciones de la UI-STA están en CT-PBT2B-03, y las dos costuras dejan en verde S-7, S-9 y CT-22, así que no hay vuelta.
- **[OPB7], pregunta 2** («Basta así (Recomendado)») → L-PBT2-6: el campo desconocido de la prueba es del nivel del proyecto.
- **[OD2] D2** (F4B-E11: «ACCEPT DECLARED EXCEPTION»; el límite, «STOP and reopen L-1») → D-PBT2-14, C17 y CT-PBT2A-03 y 04 y CT-PBT2B-04, 05 y 12.
- **[OD] D-PB-10** («Each separately contracted PB task starts at: 1 / 3») → cada subtarea en 1/3.
- **[OD] §7.3** («Do not edit T1B characterization tests merely because F4B changes a behavior») → §4, sin sustituciones.
- **F4B-E06 y F4B-E09:** las obligaciones del paso 4 y del aviso en WORKSPACE → CT-PBT2B-08 y 09.
- **[DP] F4B-E03** → C08 y C11. **F4B-E19** → P-01 con la Snapshot J. **F4B-E21** → la delegación del §6.0.
- **F4B-E22:** las ratificaciones de M10 y M14 valían solo para PB-T1b. Este paquete escribe su texto bien desde el principio (INV-PBT2x-03 y §6.0). M11 y M15 están declarados (HT2-26, R-PBT2-11).

### 8.2 Las decisiones nuevas para el Owner

**Q-PBT2-1. El ciclo «Aplicar al borrador» / «Cerrar» en WORKSPACE (material).** [OD] pide separar aplicar de cerrar, y en CLASSIC «Actualizar» hace las dos cosas. En WORKSPACE hay que elegir qué hace el botón y qué hace «Cerrar» con lo no aplicado. En CLASSIC no cambia nada.
- **header:** `PBT2 cierre`
- **question:** «PB-T2: en el editor abierto desde el panel, «Aplicar al borrador» guarda lo editado en el borrador sin tocar el dibujo. ¿Qué pasa al pulsarlo y al cerrar? Hoy, en RACKEDITAR, «Actualizar» guarda y cierra, y «Cerrar» descarta sin preguntar (salvo un módulo sin confirmar).»
- **options:**

  | label | description |
  |---|---|
  | Aplica sin cerrar; Cerrar avisa (Recomendado) | La ventana sigue abierta tras aplicar: se puede seguir editando, ver el BOM o guardar en biblioteca. Si al cerrar queda algo sin aplicar, pregunta antes de perderlo. |
  | Aplica y cierra, como Actualizar | Un solo paso, igual que RACKEDITAR. Para seguir editando hay que volver a abrir «Editar todo». Cerrar descarta sin preguntar, como hoy. |
  | Aplica sin cerrar; Cerrar no avisa | Como el clásico al cerrar: lo no aplicado se pierde sin preguntar. |

- **Lo que cambia con cada una:**
  - **(A)** es el diseño de D-PBT2-06 y D-PBT2-07, con CT-06 tal cual.
  - **(B):** `ApplyToDraft` termina con `Close()`; `PendingWork` no gana ámbito; CT-03, 04 y 06 se rehacen, porque el BOM y la biblioteca después de aplicar ya no existen.
  - **(C):** `PendingWork` no cambia; CT-06 espera `avisos=0` sin preguntar.
- **Por qué (A):** separar los dos pasos crea una forma nueva de perder trabajo: aplicar se puede confundir con guardar y cerrar. Preguntar por un ámbito declarado (la frontera del borrador) es lo que ADR-0029 D8 admite. ADR-0029 D8 no quiere un dirty global de la sesión: este no lo es.

**Q-PBT2-2. La compuerta de «Aplicar al borrador» con un diagnóstico bloqueante (material).**
- **header:** `PBT2 bloqueo`
- **question:** «PB-T2: ¿«Aplicar al borrador» acepta un diseño con un diagnóstico bloqueante (por ejemplo, una celda que no cabe)? En RACKEDITAR, «Actualizar», el BOM y la biblioteca lo rechazan con el motivo.»
- **options:**

  | label | description |
  |---|---|
  | Lo rechaza, como Actualizar (Recomendado) | El borrador nunca guarda un diseño que no se puede dibujar: se corrige en la ventana. Es la misma compuerta en los dos modos. |
  | Lo acepta | El borrador puede guardar un diseño a medias; el «Actualizar» del panel lo rechazará al escribir (PB-T3). Los campos inválidos se siguen rechazando. |

- **Lo que cambia con (B):** `ApplyEditsToEditingState` deja de mirar la compuerta de salida en WORKSPACE, y entonces deja de ser el mismo paso. El tooltip bloqueado desaparece. CT-05 espera `llamadas=1` en `bloqueante`, y PB-T3 hereda bloquear un borrador bloqueado.
- **Por qué (A):** es «el mismo paso» en los dos modos y cumple la frase de la validación única. Con la ventana abierta (Q-PBT2-1, A), el usuario corrige y aplica.

**Q-PBT2-3. El alcance de «una sola validación de campos» (material: es una lectura de la frase del Owner).**
- **header:** `PBT2 valid.`
- **question:** «PB-T2: pides una sola validación de campos. La ventana usará la misma en los dos modos. Pero el campo de escala del panel valida con su propia copia de la regla (mismos textos y misma regla > 0). ¿Lo unifico ya o al conectar «Editar todo»?»
- **options:**

  | label | description |
  |---|---|
  | En PB-T4, con prueba ya (Recomendado) | PB-T2 añade una prueba de que las dos dan lo mismo. PB-T4, que rehace ese campo sobre el estado nuevo, deja una sola. |
  | Ya en PB-T2 | Se mueve la regla común de las cajas a Application para que el panel la use. Toca el control numérico que comparten todos los editores, sin cambiar ningún texto. |
  | No hace falta | Son la misma regla; no se toca el panel. |

- **Lo que cambia con cada una:**
  - **(B):** PB-T2a gana el traslado de `NumericFieldValidation` (UI) a Application, con un reenvío en UI y el adaptador del piloto como consumidor. Cambian su alcance, su RED y su contrato, y Cantilever y todos los editores entran en la red.
  - **(C):** CT-PBT2B-13 sale.
- **Por qué (A):** PB-T2 no crea ninguna validación nueva. El duplicado es anterior a F4B y está en el campo que PB-T4 rehace. Mover un control compartido tiene un radio que [P] ya trató con cuidado en OB-06.6.

**Q-PBT2-4. La emisión condicional (procedimiento).**
- **header:** `PBT2 emisión`
- **question:** «PB-T2 se divide en PB-T2a (el puerto al borrador) y PB-T2b (la ventana en dos modos). ¿Puedo emitir cada una sin volver a ti, si su revisión da 0 bloqueantes y 0 importantes sin resolver y la remedición sobre PB-T1c no cambia nada material (como hiciste con PB-TC2 y PB-T1)?»
- **options:**

  | label | description |
  |---|---|
  | Sí, con esa condición (Recomendado) | Vuelvo a ti por una decisión nueva, un control con firma no prevista, un intento 3 o un efecto material sin medir. |
  | No, vuelve antes de cada emisión | Te presento cada contrato antes de emitirlo. |

- **Por qué se pregunta:** [OD] dio a PB-T2 «ACCEPT IN PRINCIPLE», sin la autorización condicional que F4B-E09 (pregunta 4) dio a PB-TC2 y PB-T1.

### 8.3 Lo que no es decisión (lecturas declaradas)

- **L-PBT2-1. La división** en PB-T2a y PB-T2b (D-PBT2-01): la prevén OB-10.1 y D-PB-10 («y sus subtareas si se dividen»).
- **L-PBT2-2. N-PBT2-01:** la carga en WORKSPACE normaliza dos portadores sin cambiar la salida, igual que reabrir con RACKEDITAR. No es una pérdida de un valor del diseño (OB-06.5 habla de pérdidas). Si CT-02 muestra otra cosa, C15.
- **L-PBT2-3. El dueño de los subdiálogos** (D-PBT2-12): la ventana, en los dos modos; el host decide el dueño de la ventana. Si la revisión lo juzga material, vuelve al Owner.
- **L-PBT2-4. `FrontalSideBox`** sigue visible en WORKSPACE: no escribe.
- **L-PBT2-5. La prueba de la biblioteca escribe** el archivo en una carpeta temporal propia: lo exige «el archivo de biblioteca con los campos desconocidos» de [OPB7].
- **L-PBT2-6. El campo desconocido** de la prueba es del nivel del proyecto, el que I-11 conserva hoy. AUTH-13 hace que el panel no abra hoy un rack con campos desconocidos ([OPB7], pregunta 2). La prueba usa un lanzamiento fabricado y prueba la ventana, no el panel.
- **L-PBT2-7. Los textos nuevos** (D-PBT2-09) los declara el paquete, como permite [OD] para el rótulo. Si la revisión los juzga material, vuelven al Owner.
- **L-PBT2-8. Las cargas públicas lanzan en WORKSPACE:** es un error de programación, y ningún gesto llega ahí.
- **L-PBT2-9. «Restaurar»** sigue en WORKSPACE, con su semántica de hoy (R-PBT2-18).
- **L-PBT2-10. El texto escrito sin salir de la caja** no cuenta como «sin aplicar» al cerrar, como en CLASSIC (H-095).

**Decisiones materiales nuevas: las cuatro del §8.2.** Ninguna otra.

## 9. Límites

Lo que no se pudo medir está en el registro, apartado P. En resumen:
- **Nada se compiló ni se ejecutó.** Las líneas esperadas salen de oráculos existentes (CT-PBTC-02, 05 y 07, CT-PBT1B-12, T-CL y T-SG) o son literales. Las ramas del paso 1 las mide el Worker.
- **PB-T1c no existe:** las anclas de W, WC y WX son de `2f5d3dec` [T1c]. Los gestos de CT-PBT1C-02 (para CT-PBT2B-10) y el texto de sus guardas se fijan en la remedición.
- **PB-T1b está en su intento 2:** las reglas de G-L1 que copian CT-PBT2A-04 y CT-PBT2B-12 son las de su versión cerrada, que hoy no existe [T1b-i2].
- **Por lectura, sin medir:**
  - que `Close()` sobre una ventana sin mostrar pase por `OnClosing` (las pruebas de PB-TC2 y PB-T1a lo usan con el aviso sustituido);
  - el punto fijo de la carga en WORKSPACE;
  - los valores de N-PBT2-01a;
  - si algún diseño del corpus trae seguridad no autorizada;
  - el coste por llamada de `Equals`.
- **Los recuentos son estáticos** (registro M). Los `MinSelected` vienen de los contratos anteriores, y la línea base de la CI está **prevista**, no medida.
- **No medido aquí:** el `config.toml` frente a la Snapshot J, el binario acreditado y la vigencia de las celdas. Los comprueba la sesión al emitir.
- **El worktree de la rama no se leyó** (otro proceso trabajaba en él). Toda lectura fue por objetos git desde el checkout principal.
- **Una incidencia de procedimiento** (registro, cabecera): una orden incluyó `python3 -c "pass"`, sin efecto. Incumple la regla. No se repitió.

## 10. Arrastres que salen de PB-T2

| Id | Qué | Va a |
|---|---|---|
| A2-1 | Abrir la ventana con `ForDraft(PushBackDraftSessionPort.LaunchFor(draft, proyecto))`; poner su dueño (la ventana principal de AutoCAD o la del panel) y mostrarla modal (OB-01.4); sacar el proyecto interior de la lectura | PB-T4 |
| A2-2 | La guarda de texto de OB-01.3 sobre el abridor de «Editar todo» | PB-T4 |
| A2-3 | Confirmar o rechazar el texto pendiente del panel antes de abrir, y K = 1 mientras el editor está abierto (R-PB-10) | PB-T4 |
| A2-4 | El borrador del panel como `DraftSession<PushBackEditableState>`, con el comparador de PB-T1 (A-2) y el coste de `IsDirty` (M15) | PB-T4 |
| A2-5 | D2, condiciones 3, 5, 6 y 7, sobre el `DraftSession` del panel (A-2), sin cambio | PB-T4 |
| A2-6 | Según Q-PBT2-3 (A): dejar una sola regla para el campo de escala del panel. CT-PBT2B-13 sigue en verde mientras existan las dos | PB-T4 |
| A2-7 | Según Q-PBT2-2: si el Owner elige (B), el PREPARE de PB-T3 rechaza un borrador bloqueado con `PushBackEditorOutputGate` (ya en A-2) | PB-T3 |
| A2-8 | M11 de PB-T1b: si un host llama a `LoadDesignEditor` o `RestoreEditor` fuera de la ventana, debe guardarse del diseño nulo | PB-T4 |
| A2-9 | El resumen y «Modificado» del panel tras cerrar «Editar todo» ([P] §5, fila de PB-T4) | PB-T4 |
| A2-10 | La nota de [OPB7] para el paquete de la fase (la línea ~314 de [P] y la promesa de OB-05.2) | la sesión, en la próxima versión de [P] |
