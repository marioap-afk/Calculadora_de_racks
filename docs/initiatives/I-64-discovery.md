# I-64 — Discovery F0-D1 (ID30, Persistent RackCad Workspace)

> **Qué es este documento.** El informe de Discovery acotado de F0, tarea D1, autorizado por el Coordinator (orden F0-D1,
> CD-I64-F0-D1-01/02). **No es** Proposal, Freeze, revisión del Architect, consenso ni GATE PASS. No cambia producto, pruebas ni
> normas. `IMPLEMENTATION AUTHORIZATION = NO` hasta Coordinator = AGREED y Architect = AGREED sobre el mismo Freeze.
>
> **Fuentes de la unidad:** [brief del Owner](I-64-owner-brief.txt) («DISCOVERY REQUIRED» 1-18 y «FIRST RESPONSE REQUIRED»),
> [confirmación D0-R3](I-64-coordinator-confirmation-d0-r3.txt), [contrato](I-64-workspace-persistente-rackcad.md) y
> [evidencia](../automation/evidence/I-64-evidence.md). Autoridades por referencia: [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md)
> §§2-5, [WORKFLOW](../WORKFLOW.md) §§2-4 y 11.4, [AGENTS](../../AGENTS.md), [PROMPT_TEMPLATES](PROMPT_TEMPLATES.md) §A.

## 0. Método, base y etiquetas

- **Base de lectura:** `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`, que es también la base de la rama
  `architecture/workspace-persistente-rackcad`. Toda ruta `src/`, `tests/` y `docs/` de este informe se leyó en ese árbol. Las ramas
  paralelas se leyeron solo por su referencia remota y solo como hechos de coordinación (§10); sus tips constan en la evidencia.
- **Quién y cómo:** trabajo directo de la sesión responsable (lectura estática de código, pruebas, ADR y documentos). Sin Controller,
  Worker, Architect ni subagentes. Sin AutoCAD, sin builds, sin pruebas ejecutadas, sin prototipos ni instalaciones.
- **Documentación externa:** referencia de Autodesk para AutoCAD 2025 y Microsoft Learn (§9.1); y los **metadatos** de los
  ensamblados gestionados instalados de AutoCAD 2025, leídos con `System.Reflection.Metadata` sin cargarlos ni ejecutarlos (hashes en la
  evidencia).
- **Etiquetas por afirmación:** `MEASURED` (leído o contado en la base o en el host), `RECONSTRUCTED` (leído en un documento, ADR,
  registro o fuente externa), `INFERENCE` (deducción razonada sin verificación directa) y `UNKNOWN`. Una lectura estática **no
  acredita** que un comportamiento se ejecute así en AutoCAD: lo conductual sin prueba queda `INFERENCE` o `UNKNOWN`.
- **Prioridad de lectura:** se siguió el orden propuesto (1-2 ventanas y bloqueo, 3 PaletteSet, 4-5 RACKEDITAR y editores, 6 eventos,
  8-9 RackId y RACKLISTA, 15 stale). El orden no reduce la cobertura: las dieciocho preguntas se responden en §2.
- **Numeración:** `Qnn` es la pregunta nn de «DISCOVERY REQUIRED» del brief; `FR-nn` es el punto nn de «FIRST RESPONSE REQUIRED».

## 1. Hallazgos principales

1. **Todo es modal y síncrono.** El Plugin abre todas sus ventanas con `Application.ShowModalWindow` (20 sitios) y no usa ni
   `ShowModelessWindow` ni `PaletteSet` (`MEASURED`). La edición de un rack es **un solo comando**: elegir bloque, leer su payload, abrir
   la ventana modal y, al cerrarla, redibujar.
2. **El borrador ya existe, pero vive en la ventana.** Actualizar compromete los campos pendientes del editor, construye el diseño,
   llama a `RackEditorSession.RequestUpdate` y **cierra la ventana**; el Plugin dibuja después (`MEASURED`). Cerrar la ventana es hoy el
   disparador del commit: es el acoplamiento modal central que la workspace tiene que romper.
3. **No hay ninguna suscripción a eventos de AutoCAD** en el producto (`MEASURED`): `PluginInitializer` está vacío y no hay `+=` sobre
   eventos de documento, editor ni base de datos. La workspace introduce el primer puente de eventos; no hay suscripciones dispersas
   que consolidar.
4. **Actualizar no comprueba la base.** Las seis rutas de `RACKEDITAR` leen el payload del bloque elegido al abrir y, tras cerrar,
   redibujan **sin comparar** con lo leído al abrir (cinco releen antes las hermanas por GUID; Cama redibuja solo el bloque elegido).
   Los comparadores AUTH-13 se usan en Insertar,
   RACKPROYECTAR, lotes y BOM, **no** en Actualizar (`MEASURED`). La modalidad es lo que hoy impide cambios intermedios (`INFERENCE`).
   Con una workspace modeless ese hueco se vuelve una sobrescritura silenciosa posible (EXP-08 positiva, §13).
5. **No existe huella ni revisión** del estado authored persistido (`MEASURED`). Hay piezas reutilizables para detectar
   obsolescencia: comparadores tipados AUTH-13 (cinco kinds; Cama unsupported) y la relectura de hermanas; ninguna es todavía un
   comparador «base vs. actual» (§9.7).
6. **La fundación de inventario existe y es pura en parte.** `RackListBuilder` agrupa sobres por GUID (Application, con pruebas) y
   `RackProjectionSelectionFilter` convierte una selección de AutoCAD en racks lógicos (Application, con pruebas). La travesía física
   (`RackBlockFinder.ScanEnvelopes`) deserializa **el sobre completo con su diseño** de cada definición (`MEASURED`): no es un índice ligero.
7. **PaletteSet está soportado por la API de AutoCAD 2025** (documentación de Autodesk y metadatos instalados: `AddVisual(name,
   control[, resize])`, `KeepFocus`, `Dock`, constructor con `toolID`). **Las superficies reales no son alojables tal cual:** los seis
   editores son subclases de `Window` y varios de sus diálogos anidados usan `Owner = this`. El shell visual común (`RackEditorVisualShell`) sí es
   un `Control` alojable. La integración en AutoCAD **no está probada** (`UNKNOWN`).
8. **Materialidad:** M-07 activado (ya lo fija el arquetipo NEW ARCHITECTURE); M-04, M-05, M-06 y M-08 activados; M-01, M-02 y M-03
   no activados bajo las condiciones de §12. EXP-09 se ejecutó solo para M-08 y lo resolvió como activado: ADR-0029 y ADR-0010
   delimitan su alcance por ventana `Window` y por `RACKEDITAR`, así que la workspace necesita ADR propio o enmienda antes de
   implementar.
9. **Intersecciones:** I-63 comparte la enumeración de racks lógicos por RackId (frontera escrita en su contrato); I-52 comparte el
   recorrido `RACKEDITAR` → Actualizar → redibujo en su producto planificado y **el entorno de host**: su política de `TRUSTEDPATHS` excluye
   rutas de RackCad, lo que choca con un smoke temprano de ID30 en la misma máquina y perfil sin coordinación (§10).
10. **Trampa de vocabulario:** ADR-0032 llama «comprometido» al estado **interno** del editor; el brief llama «committed base» al
    authored del DWG. El Freeze tiene que separar los dos términos (§4.3).

## 2. Matriz Q01..Q18

| Q | Respuesta | Ruta / símbolo | Autoridad / prueba | Etiqueta |
|---|---|---|---|---|
| Q01 Arquitectura de ventanas | 30 clases con `: Window` (grep textual); 20 aperturas por `Application.ShowModalWindow` en el Plugin; el menú `RACKCAD` abre los editores con `ShowDialog` anidado; cero `ShowModelessWindow` y cero `PaletteSet` | `src/RackCad.Plugin/*Commands*.cs`, `src/RackCad.UI/Editor/EditorModules.cs` | ADR-0029 (D1 censo por tipo), ADR-0019 | MEASURED |
| Q02 Dónde bloquea | Durante toda la edición: `ShowModalWindow` del editor; además los diálogos anidados (`ShowDialog`), el jig de Insertar tras cerrar (`Editor.Drag`) y RACKLISTA (modal y luego zoom) | `RackMenuCommands.RackEditar`, `Rack*Commands.EditX`, `BlockPlacement.PlaceBlockWithJigStatus` | — | MEASURED (código); el bloqueo efectivo de AutoCAD es INFERENCE de la semántica modal |
| Q03 PaletteSet | API soportada y presente; los editores no son alojables tal cual; integración sin probar | §9.1 | Autodesk 2025 (referencia), metadatos de `acmgd.dll` instalado; Microsoft Learn | RECONSTRUCTED + MEASURED (API) / UNKNOWN (integración) |
| Q04 Ciclo de vida de RACKEDITAR | `GetEntity` → payload de la definición → `KindHandlerDispatch` → `EditX` → `ShowModalWindow` → `InsertRequested/UpdateOnly` → relectura de hermanas → `RedrawInPlace` por vista → un `Regen` | §3.2 | ADR-0010, ADR-0042; guardas `KindHandlerGuardSourceTests`; sin prueba conductual del Plugin | MEASURED |
| Q05 Reutilización de editores | Reutilizable: shell (`Control`, 4 editores), `RackEditorSession` (5), estados de Application, `EditorClosePolicy`. Obstáculos: `Window` como raíz, `Close()` como commit, diálogos con `Owner = this`, code-behind de 3 508-3 713 líneas | §3.3, §9.6 | ADR-0019, ADR-0029, I-15/I-20/I-21 | MEASURED |
| Q06 Eventos actuales | Ninguno en producto; la API ofrece los necesarios | §9.2 | metadatos de `accoremgd.dll`/`acdbmgd.dll` | MEASURED |
| Q07 APIs de selección usadas | `GetEntity` (RACKEDITAR, RACKLAYOUT, RACKRELLENAR, RACKPROPIEDADES) y `GetSelection` (RACKDUPLICAR, RACKPROYECTAR); nunca `SelectImplied`, `SetImpliedSelection` ni `ImpliedSelectionChanged` | `RackCommandSupport.PickRackBlock`, `RackProjectionCommandPort.Capture` | — | MEASURED |
| Q08 Rutas de escaneo RackId/miembros | `RackBlockFinder.ScanEnvelopes` (8 llamadores), `RackSiblingScan.Traverse` (3), `FindRackBlocks`; referencia → definición por `BlockTableRecord`; definición → referencias por `GetBlockReferenceIds` | §5, §7 | Rack Identity (ADR-0009); `RackEmbedDocumentTests`, `I58F2ReaderTests` | MEASURED |
| Q09 RACKLISTA reutilizable | `RackListBuilder` (puro), `RackListRow`/`RackListWindow` (UI modal), conteo de copias = máximo de referencias directas, `ZoomToRack` privado | §9.4 | `RackListBuilderTests` | MEASURED |
| Q10 Búsqueda de vistas | `RackViewCodec.Decode(kind, view, section)` → `RackViewAddress`; `FindFirstModelSpaceReference` (frontal primero); `RackViewAvailability` en los redibujos | §9.5 | ADR-0010, ADR-0042, ADR-0044; `RackViewCodec` con 11 archivos de prueba | MEASURED |
| Q11 Zoom/selección | Un solo helper: `ZoomToRack` (privado; centra la vista actual en la extensión de la primera referencia, planta XY asumida). Sin helper de selección, resaltado ni localización por vista | `RackInventarioCommands.ZoomToRack` | sin prueba | MEASURED |
| Q12 Ciclo de vida de documentos | Sin estado por documento; cada comando usa `MdiActiveDocument`; cachés de proceso (catálogo, biblioteca de bloques) | §9.3 | — | MEASURED |
| Q13 Undo/redo/Actualizar | Sin marcas de undo explícitas; toda mutación ocurre dentro de un comando; Actualizar confirma una transacción por vista | §9.3 | código; documentación de Autodesk sobre contexto de aplicación | MEASURED (código) / INFERENCE (agrupación de undo) |
| Q14 Lectura y snapshots authored | Stores por kind; `SelectiveEditorOpen`; `SelectiveAuthoredAuthority`; `RackAuthoredComparatorPorts` (AUTH-13); la apertura de RACKEDITAR lee solo el sobre elegido | §4, §9.7 | FOUNDATIONS «Authored vs Effective» y «AUTH-13»; I-58 | MEASURED |
| Q15 Huella para stale-state | No existe; cuatro alternativas con límites | §9.7 | FOUNDATIONS «Project Variables» (sin control de concurrencia) | MEASURED (ausencia) / INFERENCE (alternativas) |
| Q16 Operaciones caras a mantener lazy | Escaneos completos con deserialización de diseños, resolver, previews, BOM, `Regen` completo, construcción del árbol WPF del editor | §9.8 | — | MEASURED (estructura) / UNKNOWN (latencia y memoria) |
| Q17 Hotspots con iniciativas activas | I-52, I-63 e I-62; archivos calientes de WORKFLOW §7 | §10 | WORKFLOW §7 | MEASURED / RECONSTRUCTED |
| Q18 Estado de I-52 y nativo/ObjectARX | Host no iniciado; ARX y observador de investigación con reactores, fuera del producto; medición de entrega incompleta de marcadores tras un abort | §10.1 | registro de I-52 §§198, 247-248 | RECONSTRUCTED |

## 3. DC-01 — Comportamiento observable actual

### 3.1 Ventanas y comandos

- 37 registros `CommandMethod`: 19 comandos y 18 alias (RACKSECCION no tiene alias), todos sin `CommandFlags` (`MEASURED`: no hay
  ninguna aparición de `CommandFlags` en `src/`). Por defecto son comandos de contexto de documento (`INFERENCE` de la API).
- `RACKCAD` muestra el menú con `ShowModalWindow`; el menú abre los editores con `ShowDialog` (`EditorModules`) y devuelve un
  `InsertionRequest` que el Plugin despacha **después** de cerrar el menú (`MEASURED`).
- `RACKEDITAR` y `RACKLISTA` también son modales; RACKLISTA solo hace zoom después de cerrar su ventana (`MEASURED`).
- Los editores no interactúan con AutoCAD mientras están abiertos: no hay `StartUserInteraction` y los jigs de Insertar corren tras el
  cierre (`MEASURED`).

### 3.2 Los seis recorridos de edición

| Sistema | Ventana | Creación | Lectura al abrir | Actualizar | Insertar | Cierre con pendiente | Shell | `RackEditorSession` | Estado en Application | AUTH-13 |
|---|---|---|---|---|---|---|---|---|---|---|
| Selectivo | `RackSelectiveWindow` | `RACKSELECTIVO`/`RS`, menú | sobre elegido; `ReadProjectVariables`; `SelectiveEditorOpen`; `LinkedPropertyOptions` | `FindRackBlocks` → frontal/lateral/planta `RedrawInPlace` → borra vistas fantasma → `Regen` único; `LinkedPropertyReconciler` con el registro leído **al abrir** | `SelectiveInsertPort` (escaneo de hermanas, gate de propiedades, prepare, redibujo de hermanas, lote) | sin confirmación | sí | sí | `SelectiveEditorState` | en Insertar |
| Dinámico | `RackDynamicSystemWindow` | `RACKSISTEMADINAMICO`/`RSD`, menú | sobre elegido (`RackProjectStore`) | `FindRackBlocks` → `PreflightInnerSources` → redibujo por vista → fantasmas → `Regen` | `RackUnsupportedSiblingInsert.TryAuthorize` + lote | sin confirmación | sí | sí | piezas (`DynamicEditor*`) | en Insertar |
| Push Back | `RackPushBackSystemWindow` | `RACKPUSHBACK`/`RPB`, menú | sobre elegido (`RackProjectStore`) | como Dinámico | como Dinámico | **sí** (`EditorPendingWork`) | sí | sí | `PushBackEditorState`, `PushBackCompositeEditorState` | en Insertar |
| Cantilever | `RackCantileverWindow` | `RACKCANTILEVER`/`RCT`, menú | sobre elegido (`RackProjectStore`) | preflight y redibujo por vista | como Dinámico | sin confirmación | sí | sí | estados de componentes | en Insertar |
| Cabecera | `RackFrameConfiguratorWindow` (+ ViewModel de 2 305 líneas) | `RACKCABECERA`/`RCB`, `QUICKCABECERA`/`QCB`, menú | sobre elegido (`RackProjectStore.Header`) | `FindRackBlocks` lateral + planta → preflight → redibujo | como Dinámico | **sí** (`OnClosing`) | no | no (ViewModel propio) | — | en Insertar |
| Cama | `RackFlowBedWindow` | `QUICKCAMA`/`QCM`, menú | sobre elegido (`FlowBedConfigurationStore`) | `RedrawInPlace` **solo del bloque elegido** | no ofrece vistas adicionales | sin confirmación | no | sí | — | **unsupported** |

Fuente: `MEASURED` en `src/RackCad.Plugin/Rack*Commands.cs` (`EditSelective`, `EditDynamic`, `EditPushBack`, `EditCantilever`,
`EditCabecera`, `EditCama`) y en las ventanas de `src/RackCad.UI/Systems/` y `RackFrames/`.

**Diferencias y huecos explícitos (`MEASURED`):**

- Solo dos de seis editores piden confirmación antes de cerrar con trabajo pendiente; ADR-0029 D7 lo exige como objetivo, con
  adopción gradual (D13).
- Ninguna ruta de Actualizar consulta AUTH-13 ni compara con lo leído al abrir.
- Selectivo es el único que reconcilia vínculos de variables de proyecto, y lo hace con el registro leído **antes** de abrir la ventana.
- Cabecera no usa `RackEditorSession` ni el shell; Cama no usa el shell y su comparador AUTH-13 es unsupported.
- La identidad de Actualizar conserva el predicado histórico «vacío → GUID de la ventana» (Selectivo, Dinámico, Push Back,
  Cantilever) o `Guid.NewGuid()` (Cabecera con sobre sin Id).

### 3.3 Dónde vive hoy el borrador

- El estado editable vive en la ventana: campos del code-behind, `RackEditorSession<TDesign,TSystem>` y, según el sistema, un estado
  puro de Application (`MEASURED`).
- La frontera es `RequestDraw` → `CommitPendingEditors` → `BuildSystem` → `session.RequestUpdate|RequestInsert` →
  `InsertRequestedRaised` → `Close()` (`MEASURED` en `RackSelectiveWindow.RequestDraw`). El Plugin lee `InsertRequested`,
  `UpdateOnly`, `DesignToInsert` y `SystemToInsert` después de `ShowModalWindow`.
- Cancelar es cerrar sin solicitud: el borrador se pierde (salvo la confirmación de Push Back y Cabecera). Nada persiste entre
  aperturas del editor.

## 4. DC-02 — Autoridades, ADR y Freeze que las gobiernan

### 4.1 Dueños de reglas y valores

| Materia | Dueño (símbolo) | Gobierna | Etiqueta |
|---|---|---|---|
| Identidad del rack | `RackEmbedDocument.Id` (GUID) | ADR-0009; FOUNDATIONS «Rack Identity» | MEASURED |
| Identidad de vista | sobre `(Id, View, Section)`; `RackViewCodec` | ADR-0010, ADR-0042, ADR-0044 | MEASURED |
| Actualizar / Insertar | `EditX` + `IRackKindHandler.Edit` | ADR-0010, complementado por ADR-0042 | MEASURED |
| Authored lógico por RackId | `SelectiveAuthoredAuthority` (Selectivo); `RackAuthoredComparatorPorts` (AUTH-13) | ADR-0034 + Freeze I-48; Freeze I-58 + ADR-0044 | MEASURED |
| Authored → effective | `SelectiveEffectiveDesignResolver` | ADR-0034 + Freeze I-48 | MEASURED |
| Variables de proyecto | `ProjectVariablesDocument` (NOD `RACKCAD_PROJECT`) | ADR-0034 + Freeze I-48 | MEASURED |
| Propiedades personalizadas | `CustomPropertiesDocument`, `RackCustomPropertiesAuthority` | ADR-0039 | MEASURED |
| Nombre lógico de racks nuevos | `RackLogicalNameAllocator` (Application) vía `RackNewRackName` (Plugin) | Freeze I-60 (`docs/initiatives/I-60-freeze.md`) | MEASURED |
| Composición visual de editores | `RackEditorVisualShell` (`Control` lookless) | ADR-0019 | MEASURED |
| Contrato funcional de ventanas (dirty por ámbito, cierre, acciones) | `EditorClosePolicy`, `EditorPendingWork`, `EditorAction*` | ADR-0029 | MEASURED |
| Pendiente frente a comprometido en el editor Selectivo | `SelectiveEditorState`; commit en dos fases en las fronteras | ADR-0032 | MEASURED |
| Sesión de editor (identidad, recompute, Insertar/Actualizar) | `RackEditorSession<TDesign,TSystem>` | I-15 (sin ADR propio) | MEASURED |
| Despacho por kind en el Plugin | `KindHandlerRegistry` / `KindHandlerDispatch` | I-10 | MEASURED |
| AutoCAD solo en Plugin | — | ADR-0006; `UiSystemBoundaryGuardTests` | MEASURED |

### 4.2 Restricciones de ADR que condicionan el diseño

- **ADR-0029 D11:** ante una necesidad cubierta por infraestructura existente se adopta o se evoluciona; **un segundo modelo paralelo
  está prohibido**. Un `DraftSession` o un modelo de dirty de la workspace debe partir de `RackEditorSession`, `EditorPendingWork` y
  los estados de Application, o justificar su evolución (`RECONSTRUCTED`).
- **ADR-0029 D7/D8:** ningún camino de cierre pierde cambios en silencio; dirty pertenece a un ámbito, no a la ventana. Encaja con «una
  pestaña dirty nunca se reemplaza en silencio» del brief (`RECONSTRUCTED`).
- **ADR-0029 D13:** una migración fija primero el comportamiento con pruebas de caracterización (Enter, Escape, foco, tabulación,
  cierre) y después migra (`RECONSTRUCTED`).
- **ADR-0019 D2/D5:** sin herencia de `Window`; adoptar el shell no obliga a sustituir controles (`RECONSTRUCTED`).
- **ADR-0006:** el host de paleta, el puente de eventos y toda llamada a AutoCAD viven en el Plugin; la UI aporta el `Visual`
  (`RECONSTRUCTED`).

### 4.3 Trampa de vocabulario

ADR-0032 D5/D6 llama **«comprometido»** al estado interno del editor (por ejemplo `FondoMatrices[k].Depth`) y **«commit»** al paso de
un texto pendiente a ese estado, y lista el «cambio de fondo visible» como frontera transaccional. El brief llama **«committed base»** al
authored persistido en el DWG y reserva el commit a Actualizar (`RECONSTRUCTED`). El Freeze debe fijar un glosario con tres niveles
—texto pendiente, estado del borrador y authored persistido— y decidir si cambiar de pestaña es frontera de compromiso **interno**
(como el cambio de fondo) sin ser commit al DWG (`INFERENCE`).

## 5. DC-03 — Persistencia, DTO y legacy

- **Sobre:** `RackEmbedDocument` (`SchemaVersion` 1.0, `Kind`, `View`, `Section`, `Id`, `Name`, `Design` como JSON del kind,
  `CustomProperties` como JSON crudo, `ExtensionData`). Se serializa con `RackEmbedStore` y se guarda en el Xrecord `RACKCAD_SELECTIVE`
  del diccionario de extensión de **cada definición** de bloque, troceado en cadenas de 255 caracteres (`RackBlockData`) (`MEASURED`).
- **Diseños por kind:** `SelectivePalletDesignStore`; `RackProjectStore` para Dinámico, Push Back, Cantilever y Cabecera;
  `FlowBedConfigurationStore` para Cama (`MEASURED`).
- **Lectura tolerante:** `RackEmbedStore.Deserialize` devuelve `null` ante JSON inválido o un MAJOR futuro, para no abortar un escaneo
  completo (`MEASURED`). Un sobre legacy sin `Id` no permite inferir hermanas (FOUNDATIONS, `RECONSTRUCTED`); `RackEnvelopeIdProbe` es
  solo un sondeo diagnóstico (`MEASURED`).
- **Preservación al redibujar:** cada vista se reescribe con su **propio** sobre como fuente (por ejemplo
  `WrapSelectivePayload(..., fb.Embed)`), para conservar metadatos desconocidos por vista (I-11); en Dinámico, Push Back, Cantilever y
  Cabecera, `PreflightInnerSources` aborta el Actualizar entero ante un diseño interior de MAJOR futuro o de otro kind y conserva el
  `RackProject` interior de cada vista (`MEASURED`). Un borrador de la workspace tiene que llevar esas fuentes por vista hasta el commit
  (`INFERENCE`).
- **Fuera del sobre:** variables de proyecto (`RACKCAD_PROJECT`) y propiedades de proyecto (`RACKCAD_CUSTOM_PROPERTIES`) en el NOD
  (`RECONSTRUCTED` de FOUNDATIONS; símbolos presentes, `MEASURED`).
- **Impacto previsto:** el brief limita V1 a memoria y prohíbe una segunda autoridad persistente. Las alternativas A, B y D de §9.7 no
  exigen cambio de schema; la C sí (M-02). La restauración entre arranques queda fuera de alcance (`RECONSTRUCTED`).

## 6. DC-04 — Camino entrada → estado → persistencia → dibujo

```text
RACKEDITAR (comando, contexto de documento)
  PickRackBlock: Editor.GetEntity -> BlockReference.BlockTableRecord -> RackBlockData.Read -> RackEmbedStore.Deserialize
  KindHandlerDispatch.TryResolve(kind) -> IRackKindHandler.Edit -> Rack<X>Commands.Edit<X>
    store.Deserialize(embed.Design)                       [solo el sobre elegido; Selectivo: + registro de variables]
    new <X>Window(canInsertInAutoCad: true).LoadExisting(...)
    Application.ShowModalWindow(window)                   [AutoCAD bloqueado; el borrador vive en la ventana]
      Actualizar: CommitPendingEditors -> BuildSystem -> session.RequestUpdate -> Close()
    if !InsertRequested: return                           [cancelar = descartar]
    UpdateOnly ? FindRackBlocks(document, id)             [nueva travesía completa del BlockTable]
               : TryAuthorize / InsertPort (escaneo de hermanas + AUTH-13 + prepare)
    por cada vista: <X>DrawService.RedrawInPlace(...)      [LockDocument + transacción + commit POR VISTA]
    EraseViewBlocks(fantasmas) si sobrevive alguna vista
    Editor.Regen()                                        [uno al final]
    Insertar: lote de vistas / jig (Editor.Drag)
```

`MEASURED` en el código citado. La selección de AutoCAD hacia racks lógicos existe solo en RACKPROYECTAR:
`GetSelection` → `RackProjectionSnapshotReader.Read` → `RackProjectionSelectionFilter.Build` → `RackPhysicalSelection.RackGroups` por
RackId (`MEASURED`).

## 7. DC-05 — Consumidores, lectores e inventario de búsqueda

- **`RackBlockFinder.ScanEnvelopes`** (definiciones primero; lee y deserializa cada sobre; conteo de referencias opcional con
  `forceValidity: false`), con 8 llamadores: `CustomPropertiesExecutor`, `ProjectVariableMutationExecutor`,
  `RackCommandSupport.FindRackBlocks`, RACKBOMTOTAL, RACKLISTA, `RackSelectivoCommands` (línea 396), `RackVariablesCommands` y
  `RackNewRackName` (`MEASURED`).
- **`RackSiblingScan.Traverse`** (además recorre referencias con `forceValidity: true` y las entidades de cada definición para las capas),
  con 3 llamadores: `RackSelectivoInsertIntegration`, `RackUnsupportedSiblingInsert` y `RackProjectionSnapshotReader` (`MEASURED`).
- **`RackEditorSession`:** 5 editores (Cantilever, Dinámico, Cama, Push Back, Selectivo). **`RackEditorVisualShell`:** 4 (Cantilever,
  Dinámico, Push Back, Selectivo). **`EditorPendingWork`:** 1 (Push Back) (`MEASURED`).
- **`IRackKindHandler`:** consumido por RACKEDITAR (`Edit`), RACKBOMTOTAL (`BuildBom`, `OutputBlockedReason`) y las copias de
  RACKDUPLICAR/RACKLAYOUT (`RestampDesign`) (`MEASURED`).
- **AUTH-13:** consumido por Insertar (Selectivo, Dinámico, Push Back, Cantilever, Cabecera), RACKPROYECTAR, lotes de vistas y
  `BomAuthoredAuthority` (`MEASURED`).
- **Huecos de búsqueda:** el censo de ventanas se hizo por grep textual (`: Window`), no por tipo como exige ADR-0029 D1; la lectura de
  `Design` por los builders de cada sistema no se rastreó campo a campo porque la workspace no cambia esos DTO (`UNKNOWN` acotado).

## 8. DC-06 — Pruebas y guardas que protegen DC-01..05

- **UI (sin AutoCAD):** 39 archivos de `tests/RackCad.UI.Tests` ejercitan el contrato `LoadExisting` / `InsertRequested` / `UpdateOnly`
  de los editores (por ejemplo `EditorShellAdoptionTests`, `EditorViewBatchRequestTests`, `PushBackEditorWindowTests`,
  `FlowBedEditorWindowTests`, `CantileverEditorWindowTests`, `DynamicEditorWindowTests`) (`MEASURED`).
- **Core:** `RackListBuilderTests`, `KindDispatchTests`, `KindHandlerGuardSourceTests`, `RackProjectionCommandGuardTests`,
  `RackCountInvariantCharacterizationTests`, `I58F*`, `I60*`, y las pruebas de fundación que cita FOUNDATIONS (todas presentes, §11)
  (`MEASURED`).
- **Guardas de frontera:** `UiSystemBoundaryGuardTests` (UI sin AutoCAD), `NamespaceFolderGuardTests` y las guardas de fuente del
  Plugin (`MEASURED`).
- **Huecos:** no hay prueba conductual del recorrido `EditX` del Plugin (solo guardas de fuente; la validación real es del Owner en
  AutoCAD); no hay pruebas de eventos, de multi-documento ni de ventanas modeless porque no existen (`MEASURED`); ADR-0029 registró que
  UI.Tests no ejercitaba Enter, Escape, foco ni cierre por sistema, y no se verificó aquí cuánto se ha cubierto desde entonces (`UNKNOWN`).
- Recuentos estructurales de atributos de prueba: 5 926 en `RackCad.Tests` y 1 452 en `RackCad.UI.Tests` (`MEASURED` por grep). **No
  son** resultados de ejecución ni sustituyen ninguna clase de AGENTS.

## 9. Respuestas detalladas

### 9.1 Q03 — PaletteSet y opciones modeless

**Soporte de API documentado** (`RECONSTRUCTED`, referencia de Autodesk en la ruta `cloudhelp/2025`, consultada el 2026-10-01; las
páginas no muestran marcador de versión):

- `PaletteSet` es un contenedor modeless acoplable de controles personalizados; envuelve `AduiPaletteSet`.
- `AddVisual(string, Visual)` y `AddVisual(string, Visual, bool)` aloja WPF; la segunda decide si el contenido se ajusta al tamaño.
- Propiedades: `KeepFocus`, `Dock`, `DockEnabled`, `Visible`, `Style`, `Size`, `MinimumSize`, `RolledUp`, `Opacity`, entre otras.
- Eventos: `StateChanged`, `PaletteActivated`, `Focused`, `SizeChanged`, `Load`/`Save` (estado en XML).

**Superficie presente en el host** (`MEASURED`, metadatos de `acmgd.dll` de AutoCAD 2025 `R25.0.171.0.0`, sin cargarlo):
`PaletteSet(name)`, `PaletteSet(name, toolID)`, `PaletteSet(name, cmd, toolID)`, `AddVisual(name, control[, bResizeContentToPaletteSize])`,
`Add(name, control)` (WinForms), `KeepFocus`, `Visible`, `Dock`, `Style`. Además `Core.Application.ShowModelessWindow(...)` (tres
sobrecargas con ventana, en `accoremgd.dll`) como alternativa de ventana modeless no acoplable.

**Compatibilidad de las superficies reales** (`MEASURED` salvo donde se indica):

- Los seis editores son `Window`; `AddVisual` necesita un `Visual` hijo y una `Window` no puede ser hija de otro árbol (`INFERENCE` de
  WPF). Alojar un editor exige separar su contenido de su `Window`.
- `RackEditorVisualShell` ya es un `Control` y concentra la estructura de los cuatro editores ricos; su resolución de tokens tiene un
  respaldo para cuando el contenedor no aporta `AppStyles` (comentario y código en `OnApplyTemplate`).
- Varios diálogos anidados usan `Owner = this` o `ShowDialog(this)` (por ejemplo la ventana de BOM desde cada editor); en una paleta
  el `Owner` WPF no existe y habría que resolver el propietario de otra forma (`INFERENCE`).
- `Close()` es el disparador del commit (§3.3); una paleta no se cierra al actualizar.
- Microsoft Learn (actualizada el 2025-08-27): el contenido WPF en una ventana Win32 se aloja con `HwndSource`, en STA, y el teclado y
  la tabulación pasan por `IKeyboardInputSink`, cuyas implementaciones por defecto pueden no cubrir todos los mensajes de entrada en
  escenarios avanzados (`RECONSTRUCTED`). Riesgo de foco y teclado dentro de la paleta: `UNKNOWN` hasta el smoke.

**Integración probada en AutoCAD:** ninguna (`UNKNOWN`). Prueba propuesta: el smoke temprano de §9.10, sobre un DLL Debug de esta
rama, con paleta mínima, campo de texto, combo, lista y diálogo anidado.

**Opciones a evaluar en la Proposal (sin elegir):** (a) `PaletteSet` con `AddVisual`; (b) `ShowModelessWindow` con una `Window`
flotante; (c) paleta como navegador y ventana modeless para el editor. Criterios: foco y entrada de comandos, acoplamiento,
persistencia de posición (`toolID`), coste de migrar las superficies, multi-documento (una paleta es de aplicación, no de documento).

### 9.2 Q06 — Modelo de eventos de AutoCAD

- **Estado actual:** cero suscripciones (`MEASURED`). No hay listeners dispersos que consolidar; el riesgo es crearlos.
- **API disponible** (`MEASURED`, metadatos de `accoremgd.dll` y `acdbmgd.dll`):
  - `Document.ImpliedSelectionChanged`, `CommandWillStart`, `CommandEnded`, `CommandCancelled`, `CommandFailed`, `BeginDocumentClose`,
    `CloseWillStart`;
  - `Editor.SelectionAdded`, `SelectionRemoved`, `EnteringQuiescentState`, `LeavingQuiescentState`, `IsQuiescent`;
  - `DocumentCollection.DocumentActivated`, `DocumentToBeActivated`, `DocumentToBeDeactivated`, `DocumentBecameCurrent`,
    `DocumentCreated`, `DocumentToBeDestroyed`, `DocumentDestroyed`, `DocumentLockModeChanged`, `IsApplicationContext`,
    `ExecuteInCommandContextAsync`, `ExecuteInApplicationContext`;
  - `Database.ObjectModified`, `ObjectErased`, `ObjectAppended`, `ObjectUnappended`, `ObjectReappended`, `BeginSave`, `SaveComplete`;
  - `Application.Idle`.
- **Undo/redo:** el filtro de metadatos no encontró un evento gestionado específico de undo en esas clases (`UNKNOWN`). Candidatos:
  `CommandEnded` de `UNDO`/`REDO` y `ObjectUnappended`/`ObjectReappended`/`ObjectModified` (`INFERENCE`).
- **Hecho de I-52:** en su campaña de host, tras un abort de transacción, el reactor de objeto entregó `cancelled` pero no los
  marcadores de undo, modificación ni cierre (fila `04NO-S`, UNKNOWN estructural; registro de I-52 §198, `RECONSTRUCTED`). Es un
  indicio medido de que la entrega de notificaciones de objeto no puede suponerse completa.
- **Consecuencia para el diseño** (`INFERENCE`): un puente central puede usar eventos solo como **pistas de invalidación**; la autoridad
  de «está obsoleto» tiene que ser una relectura en el momento de Actualizar. Riesgos que el Architect debe impugnar (brief): bucles
  entre selección de AutoCAD y de RackCad, reentrada durante un comando activo, eventos durante el propio Actualizar y suscripciones
  por pestaña.

### 9.3 Q12/Q13 — Documentos, undo y contexto de ejecución

- **Estado por documento:** no existe (`MEASURED`). Cada comando usa `MdiActiveDocument`. Los datos que dependen del documento (estilos
  de cota, registro de variables, propiedades) se leen por comando y se inyectan en la ventana antes de abrirla (`MEASURED`).
- **Cachés de proceso:** `JsonRackCatalogProvider` (por directorio y firma de archivos) y `BlockLibraryDatabaseCache` (una base lateral
  por ruta, `mtime` y tamaño). Son independientes del documento (`MEASURED`): pueden compartirse entre sesiones de documento si se
  respetan sus reglas de invalidación (`INFERENCE`).
- **Bloqueo de documento:** 40 sitios con `LockDocument()`, todos dentro de comandos (`MEASURED`). Autodesk documenta que el bloqueo es
  obligatorio al interactuar desde un diálogo modeless, al acceder a un documento que no es el actual y en comandos con `Session`
  (`RECONSTRUCTED`, «Lock and Unlock a Document (.NET)», ruta 2025). La guía ObjectARX 2022 añade que en contexto de aplicación no se
  pueden usar las funciones `acedCommand*` (`RECONSTRUCTED`, versión 2022: **no** verificada para 2025).
- **Undo:** no hay marcas explícitas (`MEASURED`). Como toda mutación ocurre dentro de un comando, el undo se agrupa por comando
  (`INFERENCE`, no ejecutado). Actualizar confirma una transacción por vista (`MEASURED`; el propio `SystemBlockWriter` documenta que un
  fallo en la vista *k* deja confirmadas las anteriores) y existe una costura con transacción del llamador (`RedefineInTransaction`,
  I-47 G9) (`MEASURED`).
- **Implicaciones para una workspace** (`INFERENCE`): Actualizar e Insertar desde la paleta necesitan contexto de comando
  (`ExecuteInCommandContextAsync` o un comando interno) para conservar la agrupación de undo y poder lanzar jigs; un UNDO del usuario
  después de un Actualizar vuelve obsoleto cualquier borrador basado en el estado posterior; cambiar de documento con borradores
  abiertos exige sesiones por documento y no mezclar RackIds. Comportamiento real en AutoCAD: `UNKNOWN`.

### 9.4 Q08/Q09 — Inventario de racks

- **Piezas puras:** `RackListBuilder.Build(embeds)` agrupa por GUID, exige `Id` y `Kind`, toma el primer `Name` no vacío y describe las
  vistas; `RackProjectionSelectionFilter.Build` agrupa una selección por RackId (`MEASURED`, ambos con pruebas).
- **Piezas del Plugin:** `ScanEnvelopes` deserializa el diseño completo de cada definición; RACKLISTA cuenta copias con el máximo de
  referencias directas (`forceValidity: false`) (`MEASURED`).
- **Políticas distintas por consumidor:** RACKLISTA y RACKBOMTOTAL exigen `Id` y `Kind`; `FindRackBlocks` tolera un `Kind` ausente; la
  etiqueta de Push Back falta en `RackListBuilder.KindLabel` (hueco conocido y declarado en el código) (`MEASURED`).
- **Candidato** (`INFERENCE`): un índice runtime por documento con `RackId`, `LogicalName`, `SystemKind`, vistas (direcciones
  decodificadas), definiciones y conteo de referencias, alimentado por la misma travesía, **sin** retener `Design` deserializado, e
  invalidado por pistas del puente y refrescado bajo demanda. Coste de leer metadatos sin el diseño completo: `UNKNOWN` (el payload es un
  solo JSON troceado; leer `Id`/`Kind`/`View`/`Section`/`Name` exige recomponer y analizar el texto).
- **Frontera con I-63:** su contrato atribuye a ID20 la semántica de métricas, los providers y la agregación, y a ID30 el inventario
  runtime de navegación; ambos cuentan racks lógicos por RackId (§10.2).

### 9.5 Q10/Q11 — Localizar, centrar y seleccionar

- **Hoy:** `ZoomToRack` (privado) centra la vista actual sobre `GeometricExtents` de la primera referencia en Model Space, con la
  frontal primero y un margen del 10 %, suponiendo planta XY. Falla con un mensaje si la extensión no es válida (`MEASURED`).
- **No existe:** selección programática (`SetImpliedSelection`), resaltado, localización de una vista concreta, ni manejo de varias
  copias o de referencias en layouts (`MEASURED`).
- **Disponible en la API:** `Editor.SetImpliedSelection`, `SelectImplied`, `GetCurrentView`/`SetCurrentView` (este último ya usado)
  (`MEASURED` metadatos).
- **Preguntas para el Freeze** (`INFERENCE`): qué significa Seleccionar cuando un rack tiene N vistas y M copias (todas las referencias,
  solo las visibles, solo Model Space); Centrar con varias copias dispersas; qué hacer en espacio papel; evitar que la propia selección
  programática vuelva por `ImpliedSelectionChanged` como cambio del usuario.

### 9.6 Q05 y FR-22 — Patrón de migración de editores

| Alternativa | Qué reutiliza | Riesgos principales | Etiqueta |
|---|---|---|---|
| A. Alojar los controles/ViewModels actuales en la workspace | shell (`Control`), `RackEditorSession`, estados de Application | los editores son `Window`: hay que extraer su contenido; diálogos con `Owner = this`; `Close()` como commit | INFERENCE |
| B. Extraer superficies de editor reutilizables | el shell ya separa estructura y contenido; estados puros | trabajo por editor sobre 3 500 líneas de code-behind; caracterización previa (ADR-0029 D13) | INFERENCE |
| C. Capa adaptadora alrededor de las ventanas | las ventanas tal cual, abiertas modeless | dos ventanas flotantes por rack, sin pestañas reales; el contrato «cerrar = commit» persiste | INFERENCE |
| D. Migración escalonada | cualquiera de las anteriores por sistema | coexistencia de rutas; obliga a mantener `RACKEDITAR` y su comportamiento | INFERENCE |

Hechos que inclinan la decisión (`MEASURED`): cuatro editores ya comparten el shell y cinco la sesión; Cabecera y Cama son los más
lejanos (sin shell; Cabecera sin sesión); Push Back es el único con dirty por ámbito. El brief excluye seis reescrituras paralelas y
exige mantener `RACKEDITAR` usable. **No se elige aquí** una alternativa.

### 9.7 Q14/Q15 y FR-19 — Obsolescencia (stale) y su detección

**Hechos** (`MEASURED`):

- No hay revisión, huella ni contador en el sobre ni en el diseño.
- Actualizar no compara la base con el estado actual (§3.2).
- El registro de variables que usa Selectivo al reconciliar es el leído **al abrir**.
- Los comparadores AUTH-13 producen `Single`/`Divergent`/`Unreadable` sobre authored tipado (Cama unsupported; con `PostPeralte` global
  0 pueden dar `Divergent` conservador, según FOUNDATIONS).
- FOUNDATIONS declara que el registro de variables no ofrece control general de concurrencia entre preflight y commit.
- Selectivo abre el editor con **dos** entradas: el authored leído (`saved`) y el diseño **efectivo** (`open.Design`); edita sobre el
  efectivo y, al Actualizar, `LinkedPropertyReconciler` produce el authored final desde `saved` (`MEASURED` en `EditSelective`). La base de
  comparación de un borrador tiene que ser el **authored** (con sus vínculos), nunca el efectivo ni la geometría resuelta (FOUNDATIONS
  «Authored vs Effective»; `INFERENCE` para el diseño).

**Qué cuenta como cambio externo** (`INFERENCE`): otra edición del mismo rack (RACKEDITAR, RACKPROYECTAR, RACKDUPLICAR sobre el origen,
una futura RACKMIRROR), un UNDO/REDO, una mutación de variables de proyecto que cambia el **efectivo** sin tocar el authored, cambios de
propiedades personalizadas, el alta o baja de vistas hermanas, un renombrado o un COPY que comparte definición.

| Alternativa | Idea | Límites | Materialidad |
|---|---|---|---|
| A. Comparación authored tipada | guardar la base authored del borrador y, al Actualizar, releer las hermanas y comparar con AUTH-13 | Cama sin comparador; `Divergent` conservador; no detecta cambios fuera del authored (vistas, nombre, propiedades, registro) salvo que se añadan | sin schema |
| B. Huella de la base | hash de los sobres de las hermanas (texto normalizado), del conjunto de miembros y de la revisión del registro y propiedades relevantes | define qué normaliza; falsos positivos por metadatos irrelevantes; coste de la travesía | sin schema |
| C. Revisión persistida | contador o huella escrita en el sobre en cada commit | cambio de schema y de significado persistido; builds anteriores no lo mantienen | **M-02**; decisión Owner/ADR |
| D. Invalidación por eventos | marcar obsoleto por pistas del puente | entrega no garantizada (indicio de I-52 §198); nunca suficiente sola | solo como pista |

**Límite de la relectura** (`INFERENCE`): si la relectura, la comparación y la escritura ocurren bajo **el mismo** `LockDocument` y la
misma transacción en el hilo principal de AutoCAD, no hay ventana intra-proceso entre comprobar y escribir. Hoy Actualizar abre una
transacción por vista (§9.3), así que esa atomicidad no existe todavía. Algoritmo y schema: **no se deciden aquí**.

### 9.8 Q16, FR-20 y FR-23 — Trabajo caro y estrategia lazy

**Caro y por ello candidato a diferirse** (`MEASURED` como estructura; latencia y memoria `UNKNOWN`):

- la travesía completa del `BlockTable` con deserialización de cada diseño (`ScanEnvelopes`) y, peor, la de hermanas
  (`RackSiblingScan`: referencias con `forceValidity: true` y entidades de cada definición);
- el resolver de cada sistema, los builders de preview y el BOM (este último solo bajo demanda en los editores);
- la construcción del editor WPF (code-behind de 547 a 3 713 líneas, con rejillas creadas en código);
- `Editor.Regen()` del dibujo completo en cada Actualizar;
- la carga del catálogo (cacheada por firma, pero con E/S para calcular la firma) y la base lateral de la biblioteca (cacheada).

**Estrategia candidata** (`INFERENCE`): registros de pestaña ligeros (RackId, documento, nombre, estado dirty y stale), materialización
del editor solo para la pestaña activa, liberación de árboles WPF de pestañas inactivas conservando el borrador en un estado puro, un solo
puente de eventos y ninguna transacción ni objeto de AutoCAD retenido fuera de una operación. Invariante del brief: una workspace ociosa
no resuelve, redibuja ni recalcula racks.

**Mediciones propuestas** (sin baseline previa, sin objetivos inventados): las nueve del brief (workspace cerrada; abierta ociosa;
latencia de selección; cambio de rack; cambio de pestaña; 10 pestañas ligeras; inventario grande; selección grande; cambio de documento),
con la herramienta, el DWG de prueba del Owner y la máquina registrados en la evidencia. Antes de medir hay que decidir el instrumento
(cronómetro en código de diagnóstico o medición manual), algo que excede D1.

### 9.9 FR-17, FR-18 y FR-21 — Modelos de sesión, pestañas y multiselección (candidatos, sin congelar)

```text
WorkspaceController (aplicación, Plugin)              hipótesis de nombres del brief, no congelados
  AutoCADEventBridge (uno; pistas, nunca autoridad)
  WorkspaceDocumentSession (una por Document)
    RackInventory   (índice ligero; invalidar -> refrescar bajo demanda)
    SelectionContext (0 | 1 rack | Selection(N))
    OpenTabs / ActiveTab (registros ligeros; preview y fijadas)
    DraftSessions[RackId] (base authored + revisión base, borrador, dirty, stale)
```

- **DraftSession** (`INFERENCE`): evolución de `RackEditorSession` y de los estados puros existentes (ADR-0029 D11), no un modelo
  paralelo; la base guarda lo necesario para la alternativa de stale elegida; dirty sigue ADR-0029 D8.
- **Multiselección:** `RackProjectionSelectionFilter` ya produce grupos por RackId a partir de una selección (`MEASURED`); un contexto
  «Selection (N)» puede construirse sobre él sin abrir N pestañas (`INFERENCE`).
- **Pestañas:** preview reemplazable, fijada por doble clic o edición; una pestaña dirty nunca se reemplaza en silencio (brief).

### 9.10 FR-24 — Smoke temprano del Owner (propuesta)

Tras F1, sobre el DLL Debug del worktree, en un perfil y momento acordados con I-52 (§10.1):

1. abrir la workspace y comprobar que AutoCAD sigue usable (zoom, pan, comandos ordinarios, entrada por teclado en la línea de comandos);
2. seleccionar un rack en el dibujo y ver que la workspace lo sigue; seleccionar dos o más y ver «Selection (N)»;
3. escribir en un campo de la paleta y volver al dibujo sin capturas de foco ni teclas perdidas;
4. cambiar de documento con la workspace abierta y comprobar que el contexto no se mezcla;
5. cerrar la workspace sin modificar el dibujo.

Sin editores integrados todavía, conforme al brief.

### 9.11 FR-25 — Absorción de ID3

ID3 Foundation (navegar varios racks, conservar borradores, sesión persistente) cae dentro de F2-F4. La edición por lotes
(propiedades comunes, validación de lote, mutación atómica multi-rack) queda como entrega posterior salvo que el Freeze la incluya. El
cierre clasificará ID3 como `COMPLETE` o `PARTIAL` con residual exacto (brief).

## 10. DC-07 — Archivos calientes e intersecciones (EXP-07)

Archivos calientes de [WORKFLOW](../WORKFLOW.md) §7 que ID30 probablemente toque (`INFERENCE`): los tres editores grandes
(`RackSelectiveWindow` 3 713 líneas, `RackPushBackSystemWindow` 3 630, `RackDynamicSystemWindow` 3 508; la tabla de §7 cita cifras
anteriores), `RackFrameConfiguratorViewModel` (2 305), y `src/RackCad.Plugin/*Commands*.cs` (al menos `RackMenuCommands` e
`RackInventarioCommands`) (`MEASURED` los tamaños).

### 10.1 I-52 (RACKMIRROR)

| Cruce | Publicado | Planificado | Propietario | Frontera / coordinación |
|---|---|---|---|---|
| Producto: `RACKEDITAR` → Actualizar → redibujo | 0 archivos bajo `src/`, `tests/` y `assets/` en su rama (`MEASURED`) | su contrato recorre «espejo → guardar → reabrir → RACKEDITAR → Actualizar → redibujo» (`RECONSTRUCTED`); su contrato de autoridad CT-21D estudia las costuras de dibujo de Selectivo | I-52 su producto; I-64 el hosting y el borrador | coordinar cuando cualquiera de las dos toque `EditX` o las costuras de redibujo; I-52 debe saber que la workspace exigirá detección de stale ante una mutación externa como la suya |
| Entorno de host | política de `TRUSTEDPATHS` con una sola entrada no recursiva (su `declaredSet`) y **sin ruta de RackCad**; ACL parte 1 no aplicada; S1-A..S4 no elegibles; host no iniciado (§§247-248, `RECONSTRUCTED`) | sus corridas de host | I-52 / CAD manager | un smoke de ID30 cargaría un DLL de RackCad fuera de su lista de confianza: acordar perfil, máquina o momento. ID30 **no** cambia `TRUSTEDPATHS`, `SECURELOAD` ni ACL |
| Eventos y nativo | ARX de investigación con `AcDbDatabaseReactor`, `AcDbObjectReactor`, `AcEditorReactor`, `AcApDocManagerReactor` y observador gestionado de `CommandEnded`, todo bajo `eng/research` y fuera del producto (`MEASURED` por referencia remota) | — | I-52 | no cargar ID30 en sesiones de host de I-52; usar su hallazgo de entrega (§9.2) solo como hecho |
| Documentos | `ROADMAP`, `HANDOFF` (+393 líneas) e índice de ADR | rebase diferido hasta después del host | — | ventanas de ROADMAP por acuse (ya practicadas) |

### 10.2 I-63 (ID20)

| Cruce | Publicado | Planificado | Propietario | Frontera / coordinación |
|---|---|---|---|---|
| Enumeración de racks lógicos por RackId | solo docs (bootstrap) | su alcance: conteo de racks lógicos por RackId, legacy según la fundación vigente; modelo neutral ProjectSummary «consumible después por ID30» (`RECONSTRUCTED`) | contrato de I-63: ID20 semántica/providers/agregación; ID30 inventario runtime de navegación y UI | ambas consumen la misma fundación integrada (Rack Identity, `RackListBuilder`). Si una de las dos quisiera crear una autoridad nueva de enumeración: STOP y Master |
| Superficie de UI | — | ID30 prepara alojamiento para el resumen de ID20, sin implementarlo (brief) | I-64 aloja, I-63 aporta el modelo | sin consumo de código no integrado |

### 10.3 I-62

Solo documentos de proceso (bootstrap). Cruce: escritura de ROADMAP y, a futuro, posibles cambios en `routing.md`, el catálogo o el
protocolo que afecten al enrutamiento de los gates de I-64 (`RECONSTRUCTED`). Coordinación: releer esas autoridades en `MainSha` al
emitir cada contrato de gate.

**Salida de EXP-07:** activada por evidencia de DC-07 (cruce de contrato con I-63 en la enumeración y con I-52 en el recorrido de
edición y en el entorno de host). No se diseñó ninguna fundación compartida. No se contactó a las otras sesiones en D1: las fronteras
escritas bastan para esta fase y la decisión sobre una base común, si aparece, es del Master.

## 11. DC-08 — Fundaciones del brief verificadas en la base

| Fundación | Fuente | Símbolos (presentes) | Pruebas protectoras (presentes) | Resultado |
|---|---|---|---|---|
| Rack Identity | ADR-0009 | `RackEmbedDocument`, `RackEmbedStore`, `RackEmbedComposer` | `RackEmbedDocumentTests`, `RackListBuilderTests`, `RackDuplicationPlanTests`, `SelectiveDuplicationFailClosedTests` | coincide |
| View Identity | ADR-0010 (+ ADR-0042) | `RackViewCodec`, `RackViewAddress` | `PersistenceReopenPreservationTests`, `SelectiveAuthoredBindingTests`, `DimensionViewsRestampTests` | coincide |
| Authored vs Effective | ADR-0034 + Freeze I-48 | `SelectiveAuthoredAuthority`, `SelectiveEffectiveDesignResolver` | `SelectiveEffectiveDesignResolverTests`, `SelectiveBomAuthorityTests`, `LinkedPropertyKernelTests` | coincide; autoridad estricta solo en Selectivo |
| Project Variables / Expressions | ADR-0034, Freeze I-48; ADR-0043 | `ProjectVariablesDocument`, `ProjectVariablesWorkspace`, `ProjectVariablesStore`; `ExpressionParser`, `ExpressionEvaluator`, `SymbolTable` | `ProjectVariablesDocumentTests`, `ProjectVariablesWorkspaceTests`, `ProjectVariablesConformanceTests`, `RegistryCommitAccreditationTests` | coincide; sin entrada FOUNDATIONS para el motor de expresiones |
| Linked Properties | Freeze I-48 | `SelectiveLinkedProperties`, `SelectiveLinkedPropertyKernel`, `LinkedPropertyEditSession`, `LinkedPropertyReconciler` | `LinkedPropertyEditSessionTests`, `LinkedPropertyReconcilerTests` | coincide |
| Custom Properties | ADR-0039 | `CustomPropertiesDocument`, `RackCustomPropertiesAuthority`, `CustomPropertiesWorkspace` | `CustomPropertiesAuthorityTests`, `CustomPropertiesCommitTests` | coincide |
| DimensionViews | ADR-0035 | `DimensionViewPolicy` | `DimensionViewPolicyTests`, `DimensionViewsLegacyJsonTests` | coincide |
| Shared View Foundation | ADR-0044; Freezes I-57/58/59 | `RackAuthoredComparatorPorts`, `RackSourcePlacementCaptureAdapter`, `RackSourceTransformClassifier`, `RackViewPreparationAdapter` | `I58F2ReaderTests`, `I58F3SeamTests`, `SharedViewFoundationF6Tests`, `I59F3ConsumerIntegrationTests` | coincide; FOUNDATIONS solo tiene entradas para AUTH-08/12/13 |
| Auto Rack Naming | Freeze I-60 | `RackLogicalNameAllocator`, `RackNewRackName` | `I60RackLogicalNameTests`, `I60BatchCreationNamingTests` | coincide; **sin entrada FOUNDATIONS** |

Método: presencia de cada símbolo y de cada clase de prueba en la base (`MEASURED`) y lectura de las rutas que usa la workspace
(§§3-9). El **comportamiento** de cada fundación no se re-ejecutó (no se corrió ninguna prueba): la coincidencia es estructural y de
lectura. No hay contradicción entre FOUNDATIONS, ADR, Freeze y código en lo que consume ID30 (EXP-01 negativa, §13). Las ausencias de
entrada en FOUNDATIONS son registro descriptivo incompleto, no discrepancia.

## 12. DC-09 — Disparadores M-01..M-08

| M | Resultado | Evidencia |
|---|---|---|
| M-01 segunda autoridad | **No activado, con condiciones**: el borrador ya es transitorio (vive en la ventana) y pasaría a vivir más; el inventario es una caché de hechos del sobre. Se activaría si el inventario o el borrador se leyeran como verdad sin relectura, o si se persistieran | brief «KEY ARCHITECTURAL BOUNDARY»; §3.3, §9.4; candidatos a invariante del Freeze |
| M-02 schema/persistencia | **No activado** con las alternativas A, B o D de stale; **activado** si se elige C | §5, §9.7 |
| M-03 trato de documentos existentes fuera de lo pedido | **No activado**: lo pedido incluye conservar `RACKEDITAR` y no mutar en vivo | brief |
| M-04 qué falla y cómo | **Activado** (pedido): Actualizar puede negarse por stale o conflicto donde hoy redibuja; la atomicidad entre vistas queda por definir | §9.3, §9.7 |
| M-05 contrato consumido por otros kinds | **Activado**: `IRackKindHandler.Edit`, el contrato `RackEditorSession` (`InsertRequestedRaised` → cerrar) y `LoadExisting`/`InsertRequested` de las ventanas los consumen los seis kinds | §3, §7 |
| M-06 punto de extensión | **Activado**: alojamiento por sistema de editores y superficies futuras (ID20, ID28/29, propiedades, variables) | brief «PREPARES BUT DOES NOT IMPLEMENT» |
| M-07 framework o mecanismo transversal | **Activado**: host de workspace, puente central de eventos, sesiones por documento e inventario runtime | brief; arquetipo NEW ARCHITECTURE |
| M-08 ADR aceptado | **Activado** (EXP-09, abajo) | ADR-0029 D1/D9, ADR-0010 |

**EXP-09 (solo M-08).** Pregunta registrada antes de investigar: «¿Alojar editores en una superficie modeless (paleta o ventana) y
lanzar Actualizar/Insertar desde ella modifica, reinterpreta o contradice ADR-0029, ADR-0019 o ADR-0010?». Área: decisión y alcance de
esos tres ADR y sus consumidores en `Shell/` y `Editor/`. Resultado: **activado**. ADR-0029 D1 define su censo por clases derivadas de
`Window` y D9 solo regula ownership y ubicación de **modales**; una superficie de paleta queda fuera del contrato sin una extensión de
alcance, y crear un contrato paralelo lo prohíbe D11. ADR-0010 decide Actualizar e Insertar «cuando un rack existente se abre mediante
`RACKEDITAR`»; abrirlo desde la workspace amplía ese alcance. ADR-0019 no se contradice (composición sin herencia de `Window`).
Consecuencia: el Freeze necesita ADR nuevo o enmienda **antes** de implementar (WORKFLOW §8). No es EXP-01: no hay contradicción entre
ADR y código.

**Arquetipo:** NEW ARCHITECTURE, fijado por el brief y coherente con M-07. Materialidad propuesta: M-04, M-05, M-06, M-07, M-08
(decide el Coordinator; el contrato conserva `materiality: [UNKNOWN]` hasta su revisión).

## 13. Expansiones EXP-01..09

| EXP | Resultado | Razón |
|---|---|---|
| EXP-01 contradicción FOUNDATIONS/ADR/Freeze/código | negativa | §11 sin contradicciones en lo consumido; las ausencias en FOUNDATIONS y las cifras antiguas de WORKFLOW §7 y ARCHITECTURE §7.3/§8 son descriptivas (hallazgos laterales en la evidencia) |
| EXP-02 autoridad ambigua | negativa por ahora | cada materia tiene dueño (§4.1); el riesgo es crear una segunda (M-01), no que hoy falte |
| EXP-03 persistencia o legacy no localizable | negativa | sobre, stores y legacy localizados (§5) |
| EXP-04 consumidores más allá de un salto | **positiva, cubierta por DC-05** | los contratos de edición los consumen seis kinds (§7); no requiere investigación extra en D1 |
| EXP-05 invariante sin prueba ni modo de probar | **positiva** | «idle no recalcula», foco y teclado en paleta y entrega de eventos no tienen prueba ni modo automatizado conocido sin AutoCAD; el modo propuesto es el smoke del Owner y mediciones (§9.8, §9.10). Ampliarla vuelve al Coordinator |
| EXP-06 semántica de fallo indeterminable o silenciosa | negativa | la semántica actual es determinable (transacción por vista, informe de conteos); su redefinición es materia del Freeze (M-04) |
| EXP-07 rama activa en la misma autoridad | **positiva y ejecutada** | §10 |
| EXP-08 deuda que el cambio empeoraría | **positiva** | Actualizar sin comprobación de base y con registro leído al abrir (§3.2, §9.7): con modeless se vuelve sobrescritura silenciosa. Los hechos ya constan; investigar más (p. ej., caracterizarlo con pruebas) vuelve al Coordinator |
| EXP-09 M en UNKNOWN | **positiva y ejecutada solo para M-08** | §12 |

**¿Qué EXP debió activarse y no se activó, y por qué?**

- **EXP-05 y EXP-08 no estaban autorizadas** en la orden, que solo autoriza EXP-07 y EXP-09. Se dejaron como positivas sin
  investigación adicional porque DC-01..06 ya aportan sus hechos; su ampliación es una propuesta para el Coordinator (§16).
- **EXP-06 se evaluó negativa**, aunque es discutible. Si el Coordinator considera la transacción por vista como «posible fallo
  silencioso» frente a un Actualizar que parece atómico al usuario, debería activarse. Pregunta: «¿qué ve el usuario cuando falla la
  vista k?». Área: `RedrawInPlace` y los mensajes de las seis `EditX`.
- **EXP-02 podría reclamarse para el puente de eventos:** hoy no existe dueño de «cambio externo de un rack». No se activó porque no
  hay dos candidatas ni una ausencia que bloquee una decisión existente; el dueño lo crearía el Freeze (M-07).

## 14. Plan de gates, smoke y enrutamiento I-61 (propuesta, no Freeze)

- **Gates del brief frente a LIFECYCLE §7:** F1 tiene resultado observable (workspace abierta, selección sincronizada, smoke); F2
  (navegación) y F3 (pestañas) también. Riesgo de gate sin resultado propio: un F4 limitado a «modelo de borrador». Conviene que F4
  termine con Actualizar desde la workspace y conflicto detectado, observable por el Owner. F7 mezcla conformidad (READY-06) con
  rendimiento y regresión; la conformidad no es un gate funcional.
- **Caracterización antes de migrar** (ADR-0029 D13): cada F5 por sistema empieza fijando Enter, Escape, foco, tabulación, cierre y
  Actualizar del editor actual.
- **Enrutamiento I-61** (sin delegación ni contrato de gate; celdas del catálogo verificadas el 2026-09-30, `STALE` desde el 2026-12-29):

  | Trabajo | Clase y effort semántico (routing.md) | Nivel y celda candidata |
  |---|---|---|
  | resto de F0 (Discovery, Proposal) | documentación y caracterización por la sesión responsable | sin delegación |
  | revisión del Architect | ARCHITECTURE_REVIEW, Deep | modo e independencia por declarar (SAME-SESSION ROLE o sesión separada) |
  | F1 host, puente y documento | implementación transversal a capas, larga: LONG_HORIZON_IMPLEMENTATION, Long-horizon | **Frontera**: ninguna celda Frontera con `write-commit-push` medida (Opus 5.5 subagente solo `read`/`tool-use`). Sin celda elegible: decide el Coordinator (routing.md §4: siguiente transporte, A-n `advisory` u Owner), o se parte F1 en tareas de horizonte menor |
  | caracterización previa por editor | CHARACTERIZATION, Balanced | Equilibrado: Worker Sonnet 5.5 `medium` |
  | F5 adaptadores por sistema (corta, transversal a capas) | ROUTINE_IMPLEMENTATION, Deep | Equilibrado: Worker Sonnet 5.5 `high` |
  | Controller (planificación y verificación) | CONTROLLER_*, Balanced | Eficiente con más effort: `gpt-6-luna`/`high` |

  Las dimensiones de cada delegación las puntúa el Controller de planificación con la fecha de la delegación; esta tabla es orientativa.
  El smoke y la validación del Owner son frontera (S-01/S-10). Antes de la primera invocación del Controller hace falta una línea base
  nueva de `config.toml` (su hash cambió respecto de I-61; ver evidencia) y crear el estado `docs/automation/state/I-64.yml`.

## 15. Huecos y preguntas abiertas

| Tema | Estado | Quién decide o cómo se cierra |
|---|---|---|
| Foco y teclado de WPF en `PaletteSet` en AutoCAD 2025 | UNKNOWN | smoke del Owner tras F1 |
| Evento gestionado de undo/redo | UNKNOWN | prueba en host o documentación adicional |
| Coste de leer metadatos del sobre sin el diseño completo | UNKNOWN | medición en F2 |
| Latencias y memoria (nueve escenarios del brief) | UNKNOWN | mediciones con instrumento a decidir |
| Alternativa de stale y si el registro de variables entra en la base | por decidir | Proposal → Architect + Coordinator; Owner si C |
| Semántica de Seleccionar y Centrar con varias vistas y copias | por decidir | Proposal; matriz OV |
| Si cambiar de pestaña es frontera de compromiso interno (ADR-0032) | por decidir | Proposal; ADR |
| ADR para workspace modeless (alcance de ADR-0029 y ADR-0010) | requerido | Architect + Coordinator; Owner acepta ADR |
| Smoke en la máquina donde I-52 prepara su host | coordinación | Owner / CAD manager con I-52 |
| D0-R1 y D0-R2 | no recibidas por esta sesión | Coordinator |

## 16. Siguiente alcance propuesto

1. **Revisión del Coordinator de este Discovery** (DC, EXP y la pregunta de §13).
2. Si se autoriza: **ampliaciones acotadas** de EXP-05/EXP-08. Por ejemplo, caracterizar con pruebas sin AutoCAD, y sin cambiar
   comportamiento, las piezas puras que usaría una comprobación de base (AUTH-13 sobre una base frente a un estado modificado; la
   reconciliación con un registro distinto del leído al abrir), y dejar para el host lo que solo AutoCAD puede mostrar. Es la forma de
   convertir en `MEASURED` parte de lo que aquí es `INFERENCE`, y exige autorización explícita para escribir pruebas.
3. **Proposal V1** con las definiciones de «PROPOSAL MUST DEFINE» del brief: host, sesiones, puente, inventario, stale, Actualizar,
   migración, fallo, smoke, coexistencia y fronteras de ID3. Incluye el borrador de ADR.
4. **Paquete del Architect** (PROMPT_TEMPLATES §C) sobre la Proposal completa, declarando modo de revisión e independencia.

Nada de lo anterior autoriza producto: `IMPLEMENTATION AUTHORIZATION = NO`.
