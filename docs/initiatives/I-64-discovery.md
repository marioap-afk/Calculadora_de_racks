# I-64 — Discovery F0 (ID30, Persistent RackCad Workspace)

> **Qué es este documento.** El informe de Discovery acotado de F0, autorizado por el Coordinator: tarea D1 (orden F0-D1,
> CD-I64-F0-D1-01/02) y su corrección D1-R1 (orden F0-D1-R1, revisión con CD-I64-D1-01/02 y hallazgos C64-D1-01..07). **No es**
> Proposal, Freeze, revisión del Architect, consenso ni GATE PASS. No cambia producto, pruebas ni normas.
> `IMPLEMENTATION AUTHORIZATION = NO` hasta Coordinator = AGREED y Architect = AGREED sobre el mismo Freeze.
>
> **Versión:** D1-R1. Es un documento mutable: esta versión corrige en su sitio las afirmaciones revisadas y documenta cada cambio por
> ID en §17. La versión D1 queda en el historial de Git.
>
> **Fuentes de la unidad:** [brief del Owner](I-64-owner-brief.txt) («DISCOVERY REQUIRED» 1-18 y «FIRST RESPONSE REQUIRED»),
> [confirmación D0-R3](I-64-coordinator-confirmation-d0-r3.txt), [contrato](I-64-workspace-persistente-rackcad.md) y
> [evidencia](../automation/evidence/I-64-evidence.md). Autoridades por referencia: [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md)
> §§2-5, [WORKFLOW](../WORKFLOW.md) §§2-4 y 11.4, [AGENTS](../../AGENTS.md), [PROMPT_TEMPLATES](PROMPT_TEMPLATES.md) §A.

## 0. Método, base y etiquetas

- **Base de lectura:** `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`, base de la rama
  `architecture/workspace-persistente-rackcad`, sin cambios durante D1 ni D1-R1. Toda ruta `src/`, `tests/` y `docs/` se leyó en ese
  árbol. Las ramas paralelas se leyeron solo por referencia remota y solo como hechos de coordinación (§10); sus tips constan en la
  evidencia.
- **Quién y cómo:** trabajo directo de la sesión responsable: lectura estática de código, pruebas, ADR y documentos, más documentación
  oficial. Sin Controller, Worker, Architect ni subagentes. Sin AutoCAD, builds, pruebas ejecutadas, prototipos, sondas ni instalaciones.
- **Documentación externa:** referencia de Autodesk (rutas 2025 y una 2022, señalada), Microsoft Learn y los metadatos de los
  ensamblados de AutoCAD 2025 instalados, leídos sin cargarlos (§9.1; hashes en la evidencia).
- **Etiquetas por afirmación:** `MEASURED` (leído o contado en la base o en el host), `RECONSTRUCTED` (leído en un documento, ADR,
  registro o fuente externa), `INFERENCE` (deducción razonada sin verificación directa) y `UNKNOWN`. Una lectura estática **no acredita**
  que un comportamiento se ejecute así en AutoCAD: lo conductual sin prueba queda `INFERENCE` o `UNKNOWN`, con prueba propuesta,
  responsable y fase (§15).
- **Numeración:** `Qnn` es la pregunta nn de «DISCOVERY REQUIRED» del brief; `FR-nn` es el punto nn de «FIRST RESPONSE REQUIRED»;
  `C64-D1-nn` son los hallazgos de la revisión del Coordinator.

## 1. Hallazgos principales

1. **Todo es modal y síncrono.** El Plugin abre todas sus ventanas con `Application.ShowModalWindow` (20 sitios) y no usa
   `ShowModelessWindow` ni `PaletteSet` (`MEASURED`). La edición de un rack es **un solo comando**. Además, AutoCAD **no dispara
   eventos** mientras muestra un diálogo modal (guía oficial de manejadores de eventos, `RECONSTRUCTED`).
2. **El borrador vive en la ventana; el commit lo hace el host después del cierre, interpretando una intención explícita.** Actualizar
   compromete los campos pendientes, construye el diseño, registra la intención (`InsertRequested`, `UpdateOnly`) y cierra la ventana.
   Después el Plugin lee esa intención y escribe. Cerrar **sin** intención no escribe nada. En Selectivo hay una segunda intención,
   `BindingIntent`, que va a otra ruta de escritura (§3.4) (`MEASURED`). La workspace tiene que separar «intención de Actualizar» de
   «cerrar la superficie».
3. **No hay ninguna suscripción a eventos de AutoCAD** en el producto (`MEASURED`): la workspace introduce el primer puente.
4. **Actualizar no compara con la base.** Las seis rutas de `RACKEDITAR` leen el payload del bloque elegido al abrir y, tras el cierre,
   escriben sin comparar con lo leído (cinco releen antes las hermanas por GUID; Cama redibuja solo el bloque elegido). Los comparadores
   AUTH-13 se usan en Insertar, RACKPROYECTAR, lotes y BOM, **no** en Actualizar (`MEASURED`). La ruta de vínculos de Selectivo sí tiene
   un precedente de relectura acreditada en la misma transacción de escritura (`RegistryCommit`, `MutationDestinationBinding`) (§9.7).
5. **Los fallos de Actualizar no son atómicos ni siempre visibles** (C64-D1-01, EXP-06 positiva):
   - cada vista confirma su propia transacción;
   - la importación de bloques escribe fuera de ella;
   - un `Failure` puede llegar después de un commit;
   - los bucles cuentan solo los éxitos, y el mensaje final puede anunciar «actualizado» con un subconjunto redibujado.
   Es un riesgo derivado del código, no observado en AutoCAD (§9.12).
6. **La travesía de inventario lee y analiza sobres, no diseños.** `RackBlockFinder.ScanEnvelopes` recompone el texto del Xrecord y
   lo deserializa como `RackEmbedDocument`; `Design` queda como **cadena JSON**, sin materializar el diseño tipado ni resolver nada
   (C64-D1-02). La agrupación pura por GUID ya existe en Application (`RackListBuilder`, orientada a presentación); lo que no existe es
   un contrato común de captura, admisión y agrupación para los consumidores nuevos (§9.4). Coste y retención: `UNKNOWN`.
7. **PaletteSet está soportado por la API de AutoCAD 2025**, según la documentación de Autodesk y los metadatos de `acmgd.dll`. Las
   superficies reales no son alojables tal cual: los seis editores son subclases de `Window` y varios de sus diálogos usan
   `Owner = this`. El shell común (`RackEditorVisualShell`) sí es un `Control` alojable. La integración en AutoCAD no está probada
   (`UNKNOWN`).
8. **Materialidad:** NEW ARCHITECTURE y M-04..M-07 confirmados por el Coordinator. M-01 **activado**: cambian los dueños del borrador,
   dirty, selección, cierre, aceptación de Actualizar y comprobación de base. M-08 activado, con el delta exacto de ADR-0010 y ADR-0032
   identificado. M-02 y M-03 quedan `UNKNOWN` dependientes de decisiones de diseño y se tratan como activados (EXP-09, §12).
9. **Autoridad de pertenencia ambigua para un consumidor nuevo** (EXP-02 positiva). Hay dos reglas gobernadas de pertenencia de vistas
   a un rack, cada una con alcance propio: `FindRackBlocks` para Actualizar y `RackSiblingMembership` para Insertar (I-55 Proposal V5
   §3.7). Cuál aplica a la workspace es una decisión que se eleva; no hay contradicción (EXP-01 negativa).
10. **Candidato a fundación común con I-63** en la enumeración lógica de racks. Lo registran ambas iniciativas y la consulta al Master
    está preparada por I-63, pero **no enviada**. I-64 no lo diseña y lo eleva (§10.2, §16).
11. **Trampa de vocabulario:** ADR-0032 llama «comprometido» al estado interno del editor; el brief llama «committed base» al authored
    del DWG (§4.3).

## 2. Matriz Q01..Q18

| Q | Respuesta | Ruta / símbolo | Autoridad / prueba | Etiqueta |
|---|---|---|---|---|
| Q01 Arquitectura de ventanas | 31 clases con `: Window` y 19 XAML con raíz `Window`, sin herencia indirecta; 20 aperturas `ShowModalWindow` en el Plugin; el menú abre editores con `ShowDialog` anidado; ni `ShowModelessWindow` ni `PaletteSet`; solo 4 tipos alojables (`Control`/`ContentControl`) | `src/RackCad.Plugin/*Commands*.cs`, `src/RackCad.UI/Editor/EditorModules.cs`, §7.3 | ADR-0029 D1, ADR-0019 | MEASURED (búsqueda textual con límites, §7.3) |
| Q02 Dónde bloquea | Toda la edición (`ShowModalWindow`); diálogos anidados; jig de Insertar tras el cierre; RACKLISTA modal y luego zoom. Durante el modal no hay eventos | `RackMenuCommands.RackEditar`, `EditX`, `BlockPlacement` | guía oficial de eventos | MEASURED (código) / RECONSTRUCTED (eventos) |
| Q03 PaletteSet | API soportada y presente; superficies no alojables tal cual; integración sin probar | §9.1 | Autodesk 2025; metadatos de `acmgd.dll`; Microsoft Learn | RECONSTRUCTED + MEASURED (API) / UNKNOWN (integración) |
| Q04 Ciclo de vida de RACKEDITAR | `GetEntity` → sobre de la definición → `KindHandlerDispatch` → `EditX` → `ShowModalWindow` → intención (`InsertRequested`/`UpdateOnly`/`BindingIntent`) → relectura de hermanas → `RedrawInPlace` por vista → renombrado → borrado de fantasmas → un `Regen` | §3.2, §6 | ADR-0010, ADR-0042; guardas de fuente; sin prueba conductual del Plugin | MEASURED |
| Q05 Reutilización de editores | Reutilizable: shell (`Control`, 4 editores), `RackEditorSession` (5), estados de Application, `EditorClosePolicy`. Obstáculos: `Window` raíz, intención leída tras el cierre, `Owner = this`, code-behind de 3 508-3 713 líneas | §3.3, §9.6 | ADR-0019, ADR-0029; `RackEditorSessionTests` | MEASURED |
| Q06 Eventos actuales | Ninguno en producto; la API ofrece los necesarios con restricciones de uso | §9.2 | metadatos; guía oficial de eventos | MEASURED / RECONSTRUCTED |
| Q07 APIs de selección usadas | `GetEntity` y `GetSelection`; nunca `SelectImplied`, `SetImpliedSelection` ni `ImpliedSelectionChanged` | `RackCommandSupport.PickRackBlock`, `RackProjectionCommandPort.Capture` | — | MEASURED |
| Q08 Rutas de escaneo RackId/miembros | `ScanEnvelopes` (8 llamadores; lee y analiza sobres, `Design` como cadena), `RackSiblingScan` (3; además referencias por definición y capas de sus entidades), `FindRackBlocks`; dos reglas de pertenencia (§9.4) | §5, §7 | ADR-0009; I-55 V5 §3.7/§4.1; `RackListBuilderTests` | MEASURED |
| Q09 RACKLISTA reutilizable | `RackListBuilder` (puro, presentación), `RackListRow`/`RackListWindow` (UI modal), copias = máximo de referencias directas, `ZoomToRack` privado | §9.4 | `RackListBuilderTests` | MEASURED |
| Q10 Búsqueda de vistas | `RackViewCodec.Decode` → `RackViewAddress`; `FindFirstModelSpaceReference`; `RackViewAvailability` en los redibujos | §9.5 | ADR-0010/0042/0044; `SharedViewCodecTests` | MEASURED |
| Q11 Zoom/selección | Solo `ZoomToRack` (privado). Sin selección programática, resaltado ni localización por vista | `RackInventarioCommands.ZoomToRack` | sin prueba | MEASURED |
| Q12 Ciclo de vida de documentos | Sin estado por documento; `MdiActiveDocument` por comando; cachés de proceso | §9.3 | — | MEASURED |
| Q13 Undo/redo/Actualizar | Sin marcas de undo; mutaciones dentro de un comando; una transacción por vista; matriz de fallos | §9.3, §9.12 | código; guías oficiales | MEASURED (código) / INFERENCE (undo) |
| Q14 Lectura y snapshots authored | Stores por kind; `SelectiveEditorOpen`; `SelectiveAuthoredAuthority`; AUTH-13; la apertura de RACKEDITAR lee solo el sobre elegido | §4, §9.7 | FOUNDATIONS; §11 | MEASURED |
| Q15 Huella para stale-state | No existe; precedente de relectura acreditada en la ruta de vínculos; conjuntos de lectura y escritura por sistema; riesgos de normalización | §9.7, §9.13, §9.14 | `RegistryCommitAccreditationTests` | MEASURED (ausencia y precedente) / INFERENCE (alternativas) |
| Q16 Operaciones caras a mantener lazy | Lectura y análisis de sobres, materialización del diseño, resolver, previews, BOM, `Regen`, árbol WPF: se separan sus etapas | §9.8 | — | MEASURED (estructura) / UNKNOWN (coste) |
| Q17 Hotspots con iniciativas activas | I-52 (sin delta), I-63 (Discovery con candidato común), I-62 (proceso) | §10 | WORKFLOW §7 | MEASURED / RECONSTRUCTED |
| Q18 Estado de I-52 y nativo/ObjectARX | Host no iniciado; política documentada frente a configuración leída; ARX y observador de investigación fuera del producto; resultado de una fila histórica, no ley general | §10.1 | registro de I-52 §§198, 247-248 | RECONSTRUCTED |

## 3. DC-01 — Comportamiento observable actual

### 3.1 Ventanas y comandos

- 37 registros `CommandMethod`: 19 comandos y 18 alias (RACKSECCION no tiene alias), todos sin `CommandFlags` (`MEASURED`). Por defecto
  son comandos de contexto de documento (`INFERENCE` de la API).
- `RACKCAD` muestra el menú con `ShowModalWindow`; el menú abre los editores con `ShowDialog` (`EditorModules`) y devuelve un
  `InsertionRequest` que el Plugin despacha **después** de cerrar el menú (`MEASURED`).
- `RACKEDITAR` y `RACKLISTA` también son modales; RACKLISTA solo hace zoom después de cerrar su ventana (`MEASURED`).
- Los editores no interactúan con AutoCAD mientras están abiertos: no hay `StartUserInteraction` y los jigs de Insertar corren tras el
  cierre (`MEASURED`).

### 3.2 Los seis recorridos de edición

| Sistema | Ventana | Creación | Lectura al abrir | Actualizar | Insertar | Cierre con pendiente | Shell | `RackEditorSession` | Estado en Application | AUTH-13 |
|---|---|---|---|---|---|---|---|---|---|---|
| Selectivo | `RackSelectiveWindow` | `RACKSELECTIVO`/`RS`, menú | sobre elegido; registro de variables y entradas de escaneo; `SelectiveEditorOpen` (authored + efectivo); `LinkedPropertyOptions` | `FindRackBlocks` → frontal/lateral/planta `RedrawInPlace` → renombrado → fantasmas → `Regen` único; `LinkedPropertyReconciler` con el registro leído **al abrir** | `SelectiveInsertPort` (escaneo de hermanas, gate de propiedades, prepare, redibujo de hermanas atómico, lote) | sin confirmación | sí | sí | `SelectiveEditorState` | en Insertar |
| Dinámico | `RackDynamicSystemWindow` | `RACKSISTEMADINAMICO`/`RSD`, menú | sobre elegido (`RackProjectStore`) | `FindRackBlocks` → `PreflightInnerSources` → redibujo por vista → renombrado → fantasmas → `Regen` | `RackUnsupportedSiblingInsert.TryAuthorize` + refresco por vista + lote | sin confirmación | sí | sí | piezas (`DynamicEditor*`) | en Insertar |
| Push Back | `RackPushBackSystemWindow` | `RACKPUSHBACK`/`RPB`, menú | sobre elegido (`RackProjectStore`) | como Dinámico, con chequeo previo de kind y descriptor | como Dinámico | **sí** (`EditorPendingWork`) | sí | sí | `PushBackEditorState`, `PushBackCompositeEditorState` | en Insertar |
| Cantilever | `RackCantileverWindow` | `RACKCANTILEVER`/`RCT`, menú | sobre elegido (`RackProjectStore`) | chequeos previos de kind, preflight y descriptor; transacción propia por vista **sin importación**; mensaje por vista fallida | como Dinámico | sin confirmación | sí | sí | estados de componentes | en Insertar |
| Cabecera | `RackFrameConfiguratorWindow` (+ ViewModel de 2 305 líneas) | `RACKCABECERA`/`RCB`, `QUICKCABECERA`/`QCB`, menú | sobre elegido (`RackProjectStore.Header`) | `FindRackBlocks` lateral + planta → preflight → redibujo (sin borrado de fantasmas) | como Dinámico | **sí** (`OnClosing`) | no | no (ViewModel propio) | — | en Insertar |
| Cama | `RackFlowBedWindow` | `QUICKCAMA`/`QCM`, menú | sobre elegido (`FlowBedConfigurationStore`, con su documento fuente) | `RedrawInPlace` **solo del bloque elegido** | no ofrece vistas adicionales | sin confirmación | no | sí | — | **unsupported** (`Unreadable`) |

Fuente: `MEASURED` en `src/RackCad.Plugin/Rack*Commands.cs` y en las ventanas de `src/RackCad.UI/Systems/` y `RackFrames/`. El cierre
está caracterizado por `RichEditorCloseContractTests` (`TheFourEditorsWithoutScopeCloseWithoutAsking`,
`OnlyTheEditorsWithADeclaredScopeInterceptTheirClosing`, `NoCloseRouteMaterialisesAnything`) (`MEASURED`).

**Diferencias y huecos explícitos (`MEASURED`):**

- Solo dos de seis editores tienen ámbito dirty; ADR-0029 D7 lo fija como objetivo con adopción gradual (D13).
- Ninguna ruta de Actualizar consulta AUTH-13 ni compara con lo leído al abrir.
- Selectivo es el único que reconcilia vínculos de variables, con el registro leído **antes** de abrir la ventana.
- Cabecera no usa `RackEditorSession` ni el shell; Cama no usa el shell y su comparador AUTH-13 devuelve `Unreadable`.
- Identidad en Actualizar: predicado histórico «vacío → GUID de la ventana» (Selectivo, Dinámico, Push Back, Cantilever) o
  `Guid.NewGuid()` (Cabecera con sobre sin Id). Es decir, Actualizar **asigna identidad a un sobre legacy sin Id**.

### 3.3 Dónde vive el borrador y qué es un commit

- El estado editable vive en la ventana: campos del code-behind, `RackEditorSession<TDesign,TSystem>` y, según el sistema, un estado
  puro de Application (`MEASURED`).
- **Intención explícita.** `RequestDraw` → `CommitPendingEditors` (frontera de ADR-0032) → `BuildSystem` → `session.RequestUpdate` o
  `RequestInsert` (fija `InsertRequested`, `UpdateOnly`, vistas e `InsertionRequest`) → `InsertRequestedRaised` → `Close()`
  (`MEASURED`, `RackSelectiveWindow.RequestDraw`; contrato en `RackEditorSessionTests.RequestUpdate_NullsViewAndSection_KeepsExistingId`).
- **Interpretación del host.** Tras `ShowModalWindow`, el Plugin lee la intención; sin ella, retorna sin escribir (`if
  (!window.InsertRequested) return;` en las seis `EditX`) (`MEASURED`). Cancelar es cerrar sin intención: el borrador se pierde, salvo
  confirmación en Push Back y Cabecera, y nada se escribe (`RichEditorCloseContractTests.NoCloseRouteMaterialisesAnything`).
- **Consecuencia** (`INFERENCE`): en una workspace la intención tiene que llegar al host **sin** cerrar la superficie, y cerrar una
  pestaña no puede equivaler a intención.

### 3.4 La bifurcación `BindingIntent` de Selectivo

En `EditSelective`, **antes** del chequeo de `InsertRequested`, un `BindingIntent` no nulo desvía a `ApplyBinding` y retorna
(`MEASURED`):

1. `SelectiveBindingIntentPreflight.Run(intent, registry, entries)`, con el registro y las entradas leídos al abrir.
2. `ProjectVariableMutationExecutor.Execute(document, plan)`, cuyo diseño es PREPARE / MUTATE / POST:
   - PREPARE: `FindRackBlocks` por rack, `MutationDestinationBinding.Bind` (aborta si las vistas del dibujo ya no coinciden con el plan),
     resolución e importación **fuera** de la transacción;
   - MUTATE: **una sola transacción** para el registro y todas las vistas, con relectura del registro y del escaneo, acreditada por
     `RegistryCommit.Prepare` (aborta sin escribir si la relectura no es una autoridad usable);
   - POST: purga y un `Regen`.
3. Mensaje: «vínculo actualizado; se redibujaron N vista(s)» o el error.

Diferencias con Actualizar: es atómica entre vistas y relee y acredita antes de escribir. Su `catch` devuelve `Aborted` también si el
`Regen` posterior al commit lanza (`MEASURED`): un fallo posterior al commit se informaría como «no aplicado». No se absorbe en el
recorrido común.

## 4. DC-02 — Autoridades, ADR y Freeze que las gobiernan

### 4.1 Dueños de reglas y valores

| Materia | Dueño (símbolo) | Gobierna | Etiqueta |
|---|---|---|---|
| Identidad del rack | `RackEmbedDocument.Id` (GUID) | ADR-0009; FOUNDATIONS «Rack Identity» | MEASURED |
| Identidad de vista | sobre `(Id, View, Section)`; `RackViewCodec` | ADR-0010, ADR-0042, ADR-0044 | MEASURED |
| Actualizar / Insertar | `EditX` + `IRackKindHandler.Edit` | ADR-0010, complementado por ADR-0042 | MEASURED |
| Pertenencia de vistas a un rack | `FindRackBlocks` (Actualizar, ejecutor de variables) y `RackSiblingMembership.Classify` (Insertar, RACKPROYECTAR) | I-09 y Rack Identity; Freeze I-55 V5 §3.7 y §4.1 | MEASURED (§9.4, EXP-02) |
| Authored lógico por RackId | `SelectiveAuthoredAuthority` (Selectivo); `RackAuthoredComparatorPorts` (AUTH-13) | ADR-0034 + Freeze I-48; Freeze I-58 + ADR-0044 | MEASURED |
| Authored → effective | `SelectiveEffectiveDesignResolver` | ADR-0034 + Freeze I-48 | MEASURED |
| Variables de proyecto y su relectura acreditada | `ProjectVariablesDocument`; `RegistryCommit`; `MutationDestinationBinding` | ADR-0034 + Freeze I-48 | MEASURED |
| Propiedades personalizadas | `CustomPropertiesDocument`, `RackCustomPropertiesAuthority`; acarreo por `RackEmbedComposer` | ADR-0039 | MEASURED |
| Nombre lógico de racks nuevos | `RackLogicalNameAllocator` vía `RackNewRackName` | Freeze I-60 | MEASURED |
| Composición visual de editores | `RackEditorVisualShell` | ADR-0019 | MEASURED |
| Contrato funcional de ventanas | `EditorClosePolicy`, `EditorPendingWork`, `EditorAction*` | ADR-0029 | MEASURED |
| Pendiente frente a comprometido en el editor Selectivo | `SelectiveEditorState`; commit en dos fases | ADR-0032 | MEASURED |
| Sesión de editor | `RackEditorSession<TDesign,TSystem>` | I-15 (sin ADR propio) | MEASURED |
| Despacho por kind en el Plugin | `KindHandlerRegistry` / `KindHandlerDispatch` | I-10 | MEASURED |
| AutoCAD solo en Plugin | — | ADR-0006; `UiSystemBoundaryGuardTests` | MEASURED |

### 4.2 Restricciones de ADR que condicionan el diseño

- **ADR-0029 D11:** se adopta o evoluciona la infraestructura existente; **un segundo modelo paralelo está prohibido**. El borrador de
  la workspace parte de `RackEditorSession`, `EditorPendingWork` y los estados de Application; la comprobación de base, de los
  precedentes `RegistryCommit` y `MutationDestinationBinding` (`RECONSTRUCTED`).
- **ADR-0029 D7/D8:** ningún cierre pierde cambios en silencio; dirty pertenece a un ámbito (`RECONSTRUCTED`).
- **ADR-0029 D13:** caracterizar antes de migrar (`RECONSTRUCTED`).
- **ADR-0019 D2/D5:** sin herencia de `Window`; adoptar el shell no obliga a sustituir controles (`RECONSTRUCTED`).
- **ADR-0006:** host de paleta, puente de eventos y llamadas a AutoCAD en el Plugin (`RECONSTRUCTED`).
- **ADR-0044 §8:** `Unknown`/`Unreadable` nunca se vuelve ausencia o éxito (`RECONSTRUCTED`).

### 4.3 Trampa de vocabulario

ADR-0032 D5/D6 llama **«comprometido»** al estado interno del editor y **«commit»** al paso de un texto pendiente a ese estado, y lista
el «cambio de fondo visible» como frontera transaccional. El brief llama **«committed base»** al authored persistido en el DWG y reserva
el commit a Actualizar (`RECONSTRUCTED`). El Freeze debe fijar un glosario con tres niveles —texto pendiente, estado del borrador y
authored persistido— y decidir si cambiar de pestaña es frontera de compromiso **interno** (§12, M-08) (`INFERENCE`).

## 5. DC-03 — Persistencia, DTO y legacy

- **Sobre:** `RackEmbedDocument` (`SchemaVersion` 1.0, `Kind`, `View`, `Section`, `Id`, `Name`, `Design` como **cadena** JSON del kind,
  `CustomProperties` como JSON crudo, `ExtensionData`). Se guarda en el Xrecord `RACKCAD_SELECTIVE` del diccionario de extensión de
  **cada definición**, troceado en cadenas de 255 caracteres (`RackBlockData`) (`MEASURED`).
- **Diseños por kind:** `SelectivePalletDesignStore`; `RackProjectStore` (Dinámico, Push Back, Cantilever, Cabecera);
  `FlowBedConfigurationStore` (Cama) (`MEASURED`).
- **Lectura tolerante:** `RackEmbedStore.Deserialize` devuelve `null` ante JSON inválido o MAJOR futuro (`MEASURED`); un sobre legacy sin
  `Id` no permite inferir hermanas (FOUNDATIONS); `RackEnvelopeIdProbe` sondea el `Id` con `JsonDocument` para diagnóstico (`MEASURED`).
- **Preservación al redibujar:** cada vista se reescribe con **su propio** sobre como fuente (`RackEmbedComposer.Compose(source, …)`),
  que hereda `ExtensionData`, la versión de schema sin degradarla y `CustomProperties`; en Dinámico, Push Back, Cantilever y Cabecera,
  `PreflightInnerSources` aborta el Actualizar entero ante un diseño interior de MAJOR futuro o de otro kind y conserva el `RackProject`
  interior por vista (`MEASURED`). Un borrador tiene que llevar esas fuentes por vista, o releerlas en el commit, hasta escribir
  (`INFERENCE`).
- **Identidad legacy:** Actualizar estampa un GUID en un sobre sin Id (§3.2). Es significado persistido que la workspace debe decidir si
  conserva (§12, M-02).
- **Fuera del sobre:** variables (`RACKCAD_PROJECT`) y propiedades de proyecto (`RACKCAD_CUSTOM_PROPERTIES`) en el NOD (`RECONSTRUCTED`).
- **Impacto previsto:** el brief limita V1 a memoria. Las alternativas A, B y D de §9.7 no cambian el schema; la C sí (M-02).

## 6. DC-04 — Camino entrada → estado → persistencia → dibujo

```text
RACKEDITAR (comando, contexto de documento; sin eventos mientras dura el modal)
  PickRackBlock: Editor.GetEntity -> BlockReference.BlockTableRecord -> RackBlockData.Read -> RackEmbedStore.Deserialize
  KindHandlerDispatch.TryResolve(kind) -> IRackKindHandler.Edit -> Rack<X>Commands.Edit<X>
    store.Deserialize(embed.Design)                 [solo el sobre elegido; Selectivo: + registro y entradas de escaneo]
    new <X>Window(canInsertInAutoCad: true).LoadExisting(...)
    Application.ShowModalWindow(window)             [borrador en la ventana]
      Actualizar: CommitPendingEditors -> BuildSystem -> session.RequestUpdate -> Close()
    Selectivo: BindingIntent != null -> ApplyBinding -> ProjectVariableMutationExecutor (una transacción) -> return
    if !InsertRequested: return                     [sin intención: sin escritura]
    UpdateOnly ? FindRackBlocks(document, id)       [travesía del BlockTable]
               : TryAuthorize / InsertPort          [escaneo de hermanas + AUTH-13 + prepare]
    por cada vista:
      importar bloques (fuera de la transacción) -> LockDocument + transacción + commit -> purga [best effort]
      SyncName (transacción propia, best effort)
    EraseViewBlocks(fantasmas) si sobrevive alguna vista (transacción propia)
    Editor.Regen()                                  [uno al final, fuera del try por vista]
    Insertar: lote de vistas / jig (Editor.Drag)
```

`MEASURED` en el código citado. La selección de AutoCAD hacia racks lógicos existe solo en RACKPROYECTAR: `GetSelection` →
`RackProjectionSnapshotReader.Read` → `RackProjectionSelectionFilter.Build` → `RackPhysicalSelection.RackGroups` (`MEASURED`).

## 7. DC-05 — Consumidores, lectores e inventario (EXP-04)

### 7.1 Travesías y lectores del sobre

- **`RackBlockFinder.ScanEnvelopes`** (definiciones primero; omite layouts, anónimas y xref; recompone el texto del Xrecord y lo
  deserializa como `RackEmbedDocument`, con `Design` como cadena; conteo opcional de referencias directas), con 8 llamadores:
  `CustomPropertiesExecutor`, `ProjectVariableMutationExecutor`, `RackCommandSupport.FindRackBlocks`, RACKBOMTOTAL, RACKLISTA,
  `RackSelectivoCommands`, `RackVariablesCommands` y `RackNewRackName` (`MEASURED`). Cada uno aplica su política de validez (comentario
  en `RackBlockFinder`).
- **`RackSiblingScan.Traverse`** (además, por definición: referencias directas con su dueño y su capa, y las capas de todas sus
  entidades; marca xref; sondea el Id de sobres no interpretables), con 3 llamadores: `RackSelectivoInsertIntegration`,
  `RackUnsupportedSiblingInsert` y `RackProjectionSnapshotReader` (`MEASURED`).
- **Corrección de D1:** ambos usan `directOnly: true`, y según la referencia de Autodesk (2022, sin verificar en 2025) `forceValidity`
  solo aplica con `directOnly: false`. La diferencia de coste no viene de ese parámetro; viene del recorrido adicional de referencias y
  entidades (`MEASURED` como estructura; coste `UNKNOWN`).

### 7.2 Lo que lee y escribe cada `EditX` (inventario verificable)

| Sistema | Miembros de la ventana leídos por el Plugin | Lecturas del dibujo | Escrituras |
|---|---|---|---|
| Selectivo | `BindingIntent`, `DesignToInsert`, `SystemToInsert`, `InsertRequested`, `UpdateOnly`, `InsertView`, `InsertionRequest`, `LinkedPropertyFinalStates`, `RackId`, `RackName` | sobre elegido; registro y entradas; estilos de cota; hermanas (`FindRackBlocks` o escaneo de Insertar); catálogo; biblioteca | definiciones y sobres por vista; definiciones importadas; nombres de bloque; definiciones y referencias fantasma; registro (solo `BindingIntent`) |
| Dinámico | `DesignToInsert`, `SystemToInsert`, `InsertRequested`, `UpdateOnly`, `InsertionRequest`, `RackId`, `RackName` | sobre elegido; estilos; hermanas; `RackProject` interior por vista; catálogo; biblioteca | por vista; importadas; nombres; fantasmas |
| Push Back | ídem Dinámico | ídem | ídem |
| Cantilever | `DesignToInsert`, `LineToInsert`, `ComponentInsertion`, `InsertRequested`, `UpdateOnly`, `InsertionRequest`, `RackId`, `RackName` | sobre elegido; hermanas; interiores; catálogo de secciones (fábrica de geometría) | por vista (sin importación); nombres; fantasmas |
| Cabecera | `Configuration`, `InsertRequested`, `UpdateOnly`, `InsertViews`, `InsertAddress`, `RackId` | sobre elegido; hermanas; interiores; catálogo; biblioteca | lateral y planta; importadas; nombres (sin fantasmas) |
| Cama | `FlowBedToInsert`, `InsertRequested`, `RackId`, `RackName` (no lee `UpdateOnly`) | sobre elegido con su documento fuente; catálogo; biblioteca | solo la definición elegida; importadas; nombre |

`MEASURED` (`grep window.<Miembro>` por archivo y lectura de cada `EditX`). Otros consumidores de los contratos de edición:
`IRackKindHandler` (RACKEDITAR, RACKBOMTOTAL y las copias de RACKDUPLICAR/RACKLAYOUT); `RackEditorSession` (5 editores);
`RackEditorVisualShell` (4); `EditorPendingWork` (1); AUTH-13 (Insertar, RACKPROYECTAR, lotes, `BomAuthoredAuthority`) (`MEASURED`).
**Hueco:** la lectura campo a campo del `Design` por los builders de cada sistema no se rastreó; la workspace no la cambia, pero una
huella que incluyera `Design` dependería de su forma (§9.14) (`UNKNOWN` acotado).

### 7.3 Censo de ventanas por tipo

31 clases declaran `: Window` (o `: System.Windows.Window`) directamente; ninguna hereda de otra ventana del producto; los 19 XAML con
raíz `Window` corresponden a 19 de esas clases (`x:Class`); el resto se construyen en código. Hay 4 tipos alojables: `StructuralSectionPicker`,
`EditorStatusPresenter`, `RackBoundedEditorShell` y `RackEditorVisualShell` (`MEASURED`). **Límites:** búsqueda textual sin build, sin
resolver alias, compilación condicional ni tipos de otros ensamblados. No sustituye el censo por tipo compilado de ADR-0029 D1.

## 8. DC-06 — Pruebas, guardas y obligaciones (EXP-05)

### 8.1 Protección existente

- **UI (sin AutoCAD):** 39 archivos ejercitan `LoadExisting` / `InsertRequested` / `UpdateOnly`; `RackEditorSessionTests` fija el
  contrato de la sesión; `RichEditorCloseContractTests` y `EditorClosePolicyTests` fijan el cierre de los seis editores ricos (`MEASURED`).
- **Core:** las pruebas con aserción concreta de §11 (`MEASURED`).
- **Guardas de fuente:** `UiSystemBoundaryGuardTests`, `NamespaceFolderGuardTests`, `KindHandlerGuardSourceTests` y las del Plugin.
  Protegen fronteras estáticas, no comportamiento (AGENTS, «Guardia de fuente») (`MEASURED`).
- **Huecos:** ninguna prueba conductual de `EditX` del Plugin (la valida el Owner en AutoCAD); ninguna de eventos, multi-documento ni
  modeless porque no existen (`MEASURED`).
- Recuentos estructurales (5 926 atributos de prueba en `RackCad.Tests`, 1 452 en `RackCad.UI.Tests`) **no son** ejecuciones.

### 8.2 Matriz de obligaciones (EXP-05)

Diseño documental de escenarios: **ninguno se ejecutó ni tiene PASS**. Las clases de evidencia no se sustituyen entre sí.

| # | Invariante (brief o ADR) | Estímulo negativo | Resultado esperado | Clase de evidencia | Fase / responsable |
|---|---|---|---|---|---|
| O-01 | Una workspace ociosa no resuelve, redibuja ni recalcula | workspace abierta con N racks; eventos de comandos ajenos | cero llamadas a resolver y regen | Core con puente simulado; medición en host | F1, F7 / Worker y Owner |
| O-02 | Ninguna suscripción por pestaña | abrir 10 pestañas | una sola suscripción por evento | UI/Core con fuente de eventos simulada; guarda de fuente complementaria | F1, F3 / Worker |
| O-03 | Una pestaña dirty nunca se reemplaza en silencio | preview dirty y seleccionar otro rack | se conserva y se pregunta | UI (STA) | F3, F4 / Worker |
| O-04 | Cambiar de pestaña o rack no escribe | editar, cambiar y comprobar el puerto del host | cero escrituras | UI con puerto de host simulado; smoke | F4 / Worker y Owner |
| O-05 | Un borrador obsoleto nunca sobrescribe un estado más nuevo | base X, cambio externo a Y, Actualizar | conflicto o reconciliación explícita, cero escrituras | Core (comparación); host (OV) | F4 / Worker y Owner |
| O-06 | Documentos aislados | dos documentos con el mismo RackId (COPY entre DWG) | sesiones separadas | Core (claves por documento); host | F1, F6 / Worker y Owner |
| O-07 | Sin bucle de selección | la workspace fija la selección implícita y vuelve el evento | sin renavegación | Core/UI con puente simulado; smoke | F1, F2 / Worker y Owner |
| O-08 | Foco y teclado no capturados | escribir en la paleta y volver a la línea de comandos | AutoCAD recibe la entrada | solo host (smoke) | F1 / Owner |
| O-09 | Sin reentrada durante el propio Actualizar | eventos de modificación durante el commit | ignorados o coalescidos | Core con puente simulado | F4 / Worker |
| O-10 | Cancelar o cerrar sin intención no escribe | cerrar sin intención | cero escrituras | **existente**: `RichEditorCloseContractTests.NoCloseRouteMaterialisesAnything` (UI) | conservar |
| O-11 | Metadatos y desconocidos por vista preservados en un Actualizar desde la workspace | sobres con `ExtensionData` distintos por vista | cada vista conserva los suyos | **existente** para el composer (`PersistenceReopenPreservationTests.Composer_TwoViews_KeepTheirOwnExtensionData`); nueva para el recorrido | F4 / Worker |
| O-12 | Un fallo no se informa como éxito y un commit no se informa como «no aplicado» | fallo inyectado antes, dentro y después del commit (puerto simulado) | resultado que distingue las cuatro clases de §9.12 | Core con escritor simulado | F4, si el Freeze lo exige / Worker |

## 9. Respuestas detalladas

### 9.1 Q03 — PaletteSet y opciones modeless

**Soporte de API documentado** (`RECONSTRUCTED`, referencia de Autodesk en la ruta `cloudhelp/2025`, consultada el 2026-10-01):
- `PaletteSet` es un contenedor modeless acoplable que envuelve `AduiPaletteSet`.
- `AddVisual(string, Visual[, bool])` aloja WPF.
- Tiene `KeepFocus`, `Dock`, `DockEnabled`, `Visible`, `Style`, `Size` y `MinimumSize`.
- Eventos: `StateChanged`, `PaletteActivated`, `Focused`, `SizeChanged`, `Load` y `Save`.

**Superficie presente en el host** (`MEASURED`, metadatos de `acmgd.dll` de AutoCAD 2025 `R25.0.171.0.0`):
- constructores `PaletteSet(name)`, `PaletteSet(name, toolID)` y `PaletteSet(name, cmd, toolID)`;
- `AddVisual(name, control[, bResizeContentToPaletteSize])` y `Add(name, control)` (WinForms);
- `KeepFocus`, `Visible`, `Dock` y `Style`;
- además, `Core.Application.ShowModelessWindow(...)` en `accoremgd.dll`.

**Compatibilidad de las superficies reales:**
- Los editores son `Window` y hay que separar su contenido para alojarlo (`MEASURED` + `INFERENCE` de WPF).
- El shell ya es un `Control` y resuelve sus tokens aunque el contenedor no aporte `AppStyles` (`MEASURED`).
- Varios diálogos usan `Owner = this` (`MEASURED`).
- La intención se lee tras el cierre (§3.3) (`MEASURED`).
- Microsoft Learn (actualizada el 2025-08-27) explica el alojamiento con `HwndSource` en STA y el teclado por `IKeyboardInputSink`, cuyas
  implementaciones por defecto pueden no cubrir todos los mensajes (`RECONSTRUCTED`).

**Integración probada:** ninguna (`UNKNOWN`; prueba: smoke de §9.10; responsable: Owner; fase: tras F1).

**Opciones para la Proposal (sin elegir):** (a) `PaletteSet` con `AddVisual`; (b) `ShowModelessWindow`; (c) paleta como navegador y
ventana modeless como editor.

### 9.2 Q06 — Modelo de eventos de AutoCAD

- **Estado actual:** cero suscripciones (`MEASURED`).
- **API disponible** (`MEASURED`, metadatos):
  - `Document`: `ImpliedSelectionChanged`, `CommandWillStart`, `CommandEnded`, `CommandCancelled`, `CommandFailed`, `BeginDocumentClose`;
  - `Editor`: `SelectionAdded`, `SelectionRemoved`, `EnteringQuiescentState`, `LeavingQuiescentState`, `IsQuiescent`;
  - `DocumentCollection`: `DocumentActivated`, `DocumentToBeActivated`, `DocumentToBeDeactivated`, `DocumentBecameCurrent`,
    `DocumentCreated`, `DocumentToBeDestroyed`, `DocumentDestroyed`, `DocumentLockModeChanged`, `IsApplicationContext`,
    `ExecuteInCommandContextAsync`, `ExecuteInApplicationContext`;
  - `Database`: `ObjectModified`, `ObjectErased`, `ObjectAppended`, `ObjectUnappended`, `ObjectReappended`, `BeginSave`, `SaveComplete`;
  - `Application.Idle`.
- **Reglas oficiales** («Guidelines for Event Handlers (.NET)», ruta 2025, `RECONSTRUCTED`):
  - el orden de los eventos y de las operaciones sobre objetos no está garantizado;
  - no se ejecutan funciones interactivas ni diálogos desde un manejador;
  - no se modifica el objeto que disparó el evento, aunque se puede leer;
  - no se hacen acciones que vuelvan a disparar el mismo evento;
  - **no se disparan eventos mientras AutoCAD muestra un diálogo modal**.
- **Undo/redo:** no se encontró un evento gestionado específico (`UNKNOWN`). Alternativas: `CommandEnded` de `UNDO`/`REDO` y los eventos
  de objeto. Prueba: host. Responsable: Worker con smoke del Owner. Fase: F1/F4. No bloquea si la autoridad es la relectura (abajo).
- **Hecho de I-52, acotado:** en **una** fila (`04NO-S`) de **una** campaña histórica (V35-A3), con reactores **nativos** de objeto, tras un
  abort se observó `cancelled` y no los marcadores de undo, modificación ni cierre (registro de I-52 §198, `RECONSTRUCTED`). No se
  generaliza a los eventos gestionados ni a otros escenarios. Apoya un diseño conservador, no una ley.
- **Consecuencia** (`INFERENCE`): los eventos sirven como pistas de invalidación; la autoridad de obsolescencia es una relectura
  acreditada antes de escribir (§9.7).

### 9.3 Q12/Q13 — Documentos, undo y contexto de ejecución

- **Estado por documento:** no existe; cada comando usa `MdiActiveDocument`; los datos dependientes del documento se leen por comando
  (`MEASURED`).
- **Cachés de proceso:** `JsonRackCatalogProvider` (directorio y firma) y `BlockLibraryDatabaseCache` (base lateral por ruta, `mtime` y
  tamaño), independientes del documento (`MEASURED`).
- **Bloqueo:** 40 sitios con `LockDocument()`, todos dentro de comandos (`MEASURED`). Autodesk exige bloquear desde un diálogo modeless,
  al acceder a otro documento y en comandos `Session` («Lock and Unlock a Document (.NET)», ruta 2025, `RECONSTRUCTED`). La guía
  ObjectARX 2022 indica que en contexto de aplicación no se usan las funciones `acedCommand*` (`RECONSTRUCTED`, 2022, no verificada para 2025).
- **Undo:** sin marcas explícitas; agrupación por comando (`INFERENCE`, no ejecutado).
- **Transacciones de Actualizar:** una por vista, más importación previa, renombrado y borrado en transacciones propias (§9.12) (`MEASURED`).
- **Implicaciones** (`INFERENCE`):
  - Actualizar desde la workspace necesita contexto de comando para conservar la agrupación de undo y poder lanzar jigs;
  - un UNDO tras Actualizar vuelve obsoletos los borradores basados en el estado posterior;
  - hacen falta sesiones por documento.

### 9.4 Q08/Q09 — Inventario y pertenencia

- **Qué hace la travesía** (C64-D1-02, `MEASURED`). `ScanEnvelopes`, por cada definición no layout, no anónima y no xref, hace cinco cosas:
  1. abre el diccionario de extensión;
  2. lee las cadenas del Xrecord y las concatena;
  3. analiza el texto como `RackEmbedDocument`; `Design` queda como cadena y `CustomProperties` como `JsonElement`;
  4. aplica la puerta de versión;
  5. cuenta referencias directas si se pide.

  **No** llama al store del kind ni materializa el diseño tipado, y **no** resuelve. Retiene, por tanto, las cadenas `Design` de todos los
  sobres durante el recorrido. Los costes de cada etapa —lectura de bytes, análisis del sobre, materialización, resolución y retención—
  son `UNKNOWN`.
- **Agrupación pura existente:** `RackListBuilder.Build` agrupa por `Id` (`OrdinalIgnoreCase`), exige `Id` y `Kind`, toma el primer
  nombre no vacío y describe vistas; está orientada a presentación (`MEASURED`; `RackListBuilderTests.Build_GroupsViewBlocksOfTheSameRackIntoOneEntry`).
  `RackPhysicalSelection` (AUTH-06/07) agrupa una **selección** por RackId (`RECONSTRUCTED`, evidencia F4 de I-57).
- **Dos reglas de pertenencia** (EXP-02, `MEASURED`). Las dos tienen semántica distinta en los casos límite:

  | Regla | Consumidores | Xref | Sobre ilegible atribuible | Kind ausente | Gobierna |
  |---|---|---|---|---|---|
  | `FindRackBlocks` | Actualizar (cinco kinds), ejecutor de variables | excluido | omitido sin aviso | tolerado | I-09 + Rack Identity; Freeze I-55 V5 §3.7: «Actualizar y PVME conservan su política» |
  | `RackSiblingMembership.Classify` | Insertar, RACKPROYECTAR | `ReadOnly` | `BlockingUnreadable` (falla cerrado) | según hechos del barrido | Freeze I-55 V5 §4.1 (sobre los hechos de AUTH-06) |
- **Coste del índice** (`UNKNOWN`): no se concluye que la travesía actual sea apta o inútil como base. Medición futura propuesta: tiempo
  y memoria de cada etapa sobre los DWG reales del Owner, con N racks, en F2. No se crea otro enumerador ni analizador en D1.
- **Frontera con I-63:** la definición del mínimo común de captura, admisión y agrupación está elevada (§10.2).

### 9.5 Q10/Q11 — Localizar, centrar y seleccionar

- **Hoy:** `ZoomToRack` centra la vista sobre `GeometricExtents` de la primera referencia en Model Space (frontal primero, margen del 10 %,
  planta XY asumida) (`MEASURED`).
- **No existe:** selección programática, resaltado, localización de una vista ni manejo de varias copias o de espacio papel (`MEASURED`).
- **API:** `SetImpliedSelection`, `SelectImplied`, `GetCurrentView`/`SetCurrentView` (`MEASURED`).
- **Preguntas para el Freeze** (`INFERENCE`):
  - qué selecciona «Seleccionar» con N vistas y M copias;
  - qué hace «Centrar» con copias dispersas;
  - cómo evitar que la selección programática vuelva como cambio del usuario (O-07).

### 9.6 Q05 y FR-22 — Patrón de migración de editores

| Alternativa | Qué reutiliza | Riesgos principales | Etiqueta |
|---|---|---|---|
| A. Alojar controles/ViewModels actuales | shell, `RackEditorSession`, estados de Application | extraer contenido de `Window`; `Owner = this`; intención leída tras el cierre | INFERENCE |
| B. Extraer superficies reutilizables | el shell separa estructura y contenido | trabajo por editor sobre 3 500 líneas; caracterización previa | INFERENCE |
| C. Adaptador alrededor de las ventanas | ventanas tal cual, abiertas modeless | sin pestañas reales; el cierre sigue mezclado con la intención | INFERENCE |
| D. Escalonada | cualquiera por sistema | coexistencia de rutas; mantener `RACKEDITAR` | INFERENCE |

Hechos (`MEASURED`): cuatro editores comparten shell y cinco la sesión; Cabecera y Cama son los más lejanos; solo dos tienen ámbito dirty.
**No se elige** una alternativa.

### 9.7 Q14/Q15 y FR-19 — Obsolescencia (stale) y su detección

**Hechos** (`MEASURED`):

- No hay revisión, huella ni contador en el sobre ni en el diseño.
- Actualizar no compara la base con el estado actual (§3.2); Selectivo reconcilia con el registro leído al abrir.
- Selectivo abre con el authored (`saved`) y el **efectivo** (`open.Design`); edita sobre el efectivo y el reconciliador produce el
  authored final desde `saved`.
- **Precedente:** la ruta `BindingIntent` relee y acredita el registro y el escaneo en la **misma** transacción de escritura
  (`RegistryCommit.Prepare`) y verifica que las vistas del plan siguen siendo las del dibujo (`MutationDestinationBinding.Bind`),
  abortando sin escribir (§3.4). Protegido por `RegistryCommitAccreditationTests.EL_EXECUTOR_NO_APLICA_LA_MUTACION_SOBRE_EL_DOCUMENTO_CRUDO_DEL_RE_READ`.
- AUTH-13 da `Single`/`Divergent`/`Unreadable`; Cama devuelve `Unreadable` (`I58F1OracleChecks`, líneas 135-136). FOUNDATIONS declara
  que el registro de variables no tiene control general de concurrencia.

**Reglas para cualquier alternativa** (`INFERENCE`):
- La base de comparación es el **authored** (con vínculos), nunca el efectivo ni la geometría.
- Igualdad authored no es igualdad efectiva: un cambio de variable cambia el efectivo sin tocar el authored.
- Un evento recibido no es autoridad.
- Cama no hereda un comparador que hoy no existe.
- Una prueba que agrupe dos sobres con AUTH-13 no prueba un protocolo base-vs-actual futuro.

| Alternativa | Idea | Límites | Materialidad |
|---|---|---|---|
| A. Comparación authored tipada | base authored del borrador frente a la relectura de hermanas, comparada con AUTH-13 o su evolución | Cama sin comparador; `Divergent` conservador; no cubre vistas, nombre, propiedades ni registro salvo que se añadan | sin schema |
| B. Huella de la base | hash del texto de los sobres, del conjunto de miembros y de las dependencias relevantes | normalización no aprobada (§9.14) | sin schema |
| C. Revisión persistida | contador o huella escrita en el sobre en cada commit | cambio de schema y de significado; builds anteriores no lo mantienen | **M-02**; Owner/ADR |
| D. Invalidación por eventos | pistas del puente | entrega y orden no garantizados; nunca suficiente sola | solo pista |
| E. Evolucionar el precedente de vínculos | extender a Actualizar la relectura acreditada en la transacción de escritura | hoy solo cubre registro y destinos | sin schema; ADR-0029 D11 favorece evolucionar lo existente |

**Hipótesis acotada H-LOCK** (C64-D1-03, `INFERENCE`). Condiciones: AutoCAD 2025; RackCad como único complemento que escribe sobres de
rack; la comprobación y la escritura dentro del **mismo** bloqueo de documento, en contexto de comando y en **una** transacción; sin
importación intermedia ni funciones interactivas. En esas condiciones, otro comando de RackCad no puede intercalar una escritura del
mismo rack entre la comprobación y el commit.

No cubre cuatro casos:
- la importación de bloques, que hoy escribe antes de la transacción;
- la purga y el `Regen` posteriores al commit;
- manejadores de eventos de otros complementos que escriban durante la transacción (la guía oficial lo permite sobre objetos distintos
  del que dispara el evento);
- la reentrada por callbacks propios.

Verificación propuesta: en el host, un manejador de prueba que escriba durante el commit y un segundo comando lanzado desde la workspace.
Responsable: Worker (diseño) y Owner (host). Fase: F4. **No** se presenta como demostración universal.

### 9.8 Q16, FR-20 y FR-23 — Trabajo caro y estrategia lazy

Se separan las etapas (C64-D1-02; estructura `MEASURED`, coste `UNKNOWN`):

| Etapa | Dónde ocurre hoy | Para el inventario |
|---|---|---|
| Leer bytes del Xrecord y concatenar | todas las travesías | necesaria |
| Analizar el sobre (`RackEmbedDocument`) | `ScanEnvelopes`, `RackSiblingScan` | necesaria para `Id`/`Kind`/`View`/`Section`/`Name` |
| Retener la cadena `Design` | resultado de ambas travesías | evitable si no se necesita (`INFERENCE`) |
| Materializar el diseño tipado (store del kind) | solo al editar, en BOM o en Insertar | diferible a la pestaña activa |
| Resolver el sistema | editores, BOM, redibujos | diferible |
| Previews y BOM | editores | diferibles |
| Recorrer referencias, dueños, capas y entidades | `RackSiblingScan`; conteo directo en `ScanEnvelopes` | según la información de navegación que se exija |
| `Editor.Regen()` completo | cada Actualizar | uno por commit |
| Construir el árbol WPF (547 a 3 713 líneas de code-behind) | abrir el editor | solo la pestaña activa |

Estrategia candidata (`INFERENCE`): registros ligeros de pestaña; materialización solo de la pestaña activa; borradores en estado puro;
un solo puente; ningún objeto de AutoCAD retenido. Mediciones: las nueve del brief, con instrumento a decidir, sin baseline inventada
(responsable Worker y Owner; fases F2-F7).

### 9.9 FR-17, FR-18 y FR-21 — Modelos candidatos (sin congelar)

```text
WorkspaceController (aplicación, Plugin)              hipótesis de nombres del brief, no congelados
  AutoCADEventBridge (uno; pistas, nunca autoridad)
  WorkspaceDocumentSession (una por Document)
    RackInventory   (índice ligero; contrato mínimo común pendiente con I-63)
    SelectionContext (0 | 1 rack | Selection(N))
    OpenTabs / ActiveTab (registros ligeros; preview y fijadas)
    DraftSessions[RackId] (base authored + dependencias de la base, borrador, dirty, stale)
```

- **DraftSession:** evolución de `RackEditorSession` y de los estados existentes (ADR-0029 D11).
- **Multiselección:** sobre `RackProjectionSelectionFilter`, sin N pestañas (`INFERENCE`).
- **Pestañas:** preview reemplazable; una pestaña dirty nunca se reemplaza en silencio (brief).

### 9.10 FR-24 — Smoke temprano del Owner (propuesta)

Tras F1, sobre el DLL Debug del worktree de I-64:

1. AutoCAD sigue usable con la workspace abierta.
2. La selección de un rack se refleja; varios racks dan «Selection (N)».
3. Se escribe en la paleta y se vuelve al dibujo sin perder teclas.
4. Cambiar de documento no mezcla contextos.
5. Cerrar la workspace no modifica el dibujo.

Perfil y momento se acuerdan con el responsable de host de I-52 (§10.1). No se infiere que sea imposible en esa máquina.

### 9.11 FR-25 — Absorción de ID3

ID3 Foundation (navegar, conservar borradores, sesión persistente) cae en F2-F4. La edición por lotes queda posterior salvo Freeze. El
cierre clasificará ID3 como `COMPLETE` o `PARTIAL` con residual exacto.

### 9.12 Matriz de fallos de Actualizar (C64-D1-01, EXP-06)

Fases y efectos, sustentados en código (`MEASURED`). **No** son fallos inyectados ni observados en AutoCAD.

| Fase | Selectivo | Dinámico / Push Back | Cantilever | Cabecera | Cama | Qué queda escrito | Qué ve el usuario | Clase |
|---|---|---|---|---|---|---|---|---|
| Antes de la ventana (sobre ilegible, apertura bloqueada) | sí | sí | sí | sí | sí | nada | mensaje | fallo sin commit |
| Preflight (reconciliación, interiores, kind o descriptor, fábrica) | reconciliación | interiores; PB: kind y descriptor | kind, interiores, descriptor, fábrica | interiores | — | nada | «No se modificó ningún bloque» o el error | fallo sin commit |
| Importación de bloques (fuera de la transacción) | sí | sí | **no** | sí | sí | definiciones importadas, sin revertir; errores tragados y registrados | nada | escritura sin vista redibujada si falla después |
| Dentro de la transacción de una vista | `Failure`; esa vista se revierte | ídem | captura y mensaje por vista | ídem | ídem | nada de esa vista; importaciones previas sí | Cantilever lo informa; los demás no la nombran | fallo sin commit de esa vista |
| Entre vistas | transacción por vista | ídem | ídem | ídem | una sola vista | vistas anteriores confirmadas | conteo solo de éxitos | **commit parcial** |
| Tras el commit dentro de `RedrawInPlace` (antes de retornar) | `Failure` aunque confirmó | ídem | no aplica (captura propia) | ídem | ídem | la vista confirmada, sin contar ni renombrar | conteo menor; posible «no se pudo actualizar» | **fallo posterior al commit** |
| Purga posterior | best effort | ídem | — | ídem | ídem | definiciones anidadas huérfanas | nada (solo registro) | posterior al commit, sin efecto semántico |
| Renombrado (`SyncName`) | transacción propia, best effort | ídem | ídem | ídem | ídem | vista confirmada con nombre antiguo | nada (solo registro) | posterior al commit, invisible |
| Borrado de fantasmas | si sobrevive alguna vista | Din: sin mensaje si no sobrevive ninguna; PB: con mensaje | con mensaje | **no hay** | — | definiciones y referencias borradas en su transacción | mensaje si falla | commit independiente |
| `Regen` final | fuera del bucle | ídem | ídem | ídem | dentro de `RedrawInPlace` | todo lo anterior | «RackCad error: …» del comando | **fallo posterior al commit** |
| Faltantes de biblioteca | resultado con faltantes | ídem | — | ídem | ídem | geometría incompleta | **no se muestran** en Actualizar | escrito, invisible |
| Mensaje final | «sistema actualizado» si hubo éxitos o borrados | Din: solo cuenta redibujos (puede decir «no se pudo» tras borrar); PB: incluye borrados | incluye borrados | solo redibujos | éxito o error | — | anuncio parcial | — |

Distinciones pedidas: **desconocido** (excepciones fuera de lo leído; comportamiento real en AutoCAD), **fallo sin commit**, **commit
parcial** (entre vistas) y **fallo posterior al commit** (`Failure` tras confirmar y `Regen`). La ruta `BindingIntent` es atómica entre
vistas, pero informa «no aplicado» si el `Regen` posterior falla (§3.4). Insertar en Selectivo redibuja las hermanas en una sola
transacción (`RackSiblingRedrawRun`). **No se repara ni se impone una nueva atomicidad**; la semántica de fallo de Actualizar desde la
workspace es una decisión del Freeze (M-04).

### 9.13 Lecturas que condicionan Actualizar y escrituras por sistema (C64-D1-03)

| Componente leído | Sistemas | Cuándo se lee hoy | Tipo de cambio externo | Tratamiento candidato (pendiente) |
|---|---|---|---|---|
| Authored del sobre elegido (`Design`) | los seis | al abrir | autoría | conflicto |
| Vínculos y `PropertyValues` del authored | Selectivo | al abrir (`saved`) | autoría | conflicto |
| Registro de variables y entradas de escaneo | Selectivo | al abrir | dependencia transitiva del efectivo | relectura y re-resolución, o conflicto |
| Pertenencia de hermanas | cinco (no Cama) | al Actualizar | pertenencia | relectura; conflicto si cambió la pertenencia de la base |
| Sobre propio de cada vista (`View`, `Section`, `ExtensionData`, `CustomProperties`, versión) | cinco | al Actualizar | metadatos por vista | preservación |
| `RackProject` interior por vista | Dinámico, Push Back, Cantilever, Cabecera | al Actualizar | metadatos | preservación o aborto |
| `CustomProperties` del rack | los seis (acarreo, sin interpretar) | al Actualizar | autoría de metadatos | preservación |
| Catálogo CSV | los seis | al abrir y al Actualizar | datos de producto | relectura |
| Biblioteca de bloques | cinco (no Cantilever) | al Actualizar | geometría de piezas | relectura |
| Estilos de cota | Selectivo, Dinámico | al abrir | presentación | refresco |
| `Id` y `Name` del sobre | los seis | al abrir | identidad y nombre | conflicto (Id); conflicto o preservación (nombre) |
| Posición, copias, capas de referencias | ninguno en Actualizar (salvo borrar referencias de fantasmas) | — | geometría y referencias | solo navegación |

Escrituras (`MEASURED`): definiciones y sobres por vista; definiciones importadas; nombres de bloque; definiciones y referencias fantasma;
`Regen`. Cama escribe solo su definición; Cabecera no borra fantasmas; la ruta `BindingIntent` escribe además el registro.

### 9.14 Riesgos de normalización de una huella (C64-D1-03; ninguna aprobada)

`INFERENCE`, con apoyo en código `MEASURED`:
- **Campos desconocidos:** `ExtensionData` se conserva por vista; reordenarlo o reescribirlo cambia el texto sin cambiar el significado.
- **Versión:** `Compose` no degrada la versión y algunos documentos «promueven» su schema al serializar; una huella textual vería cambios
  sin cambio de autoría.
- **Null frente a ausencia:** `Compose` normaliza `CustomProperties` `Undefined`/`Null` a ausente.
- **Mayúsculas:** el sobre se lee sin distinguir mayúsculas y `Compose` rechaza claves duplicadas que solo difieren en ellas.
- **Tipos y formato numérico:** el JSON del diseño lo produce cada build; el formato de números o el orden de miembros pueden variar
  entre builds (`UNKNOWN`).
- **Identidad de miembros:** handle de la definición (estable en el DWG) frente a nombre de bloque (cambia con `SyncName`) frente a
  `ObjectId` (por sesión).
- **Metadatos por vista:** `View` y `Section` difieren por diseño entre hermanas y no pueden entrar en una huella común sin distinguirlas.
- **Tipado frente a crudo:** una comparación tipada ignora diferencias de representación que una huella cruda detectaría como conflicto
  falso.

## 10. DC-07 — Archivos calientes e intersecciones (EXP-07, delta D1-R1)

Archivos calientes de [WORKFLOW](../WORKFLOW.md) §7 que ID30 probablemente toque (`INFERENCE`):
- los tres editores grandes: `RackSelectiveWindow` (3 713 líneas), `RackPushBackSystemWindow` (3 630) y `RackDynamicSystemWindow` (3 508);
- `RackFrameConfiguratorViewModel` (2 305);
- `src/RackCad.Plugin/*Commands*.cs`.

Los tamaños son `MEASURED`.

**Delta respecto de D1** (`MEASURED`, por referencia remota). Tips exactos en la evidencia.
- I-52 sin cambios.
- I-63 avanzó tres commits, solo documentos: continuación de G0, su Discovery y su Discovery R1. El R1 detiene la elección del contrato
  compartido con I-64, su Proposal y su Freeze mientras se coordina con el Master, y registra que la consulta no se envió (`RECONSTRUCTED`).
- I-62 avanzó dos commits, solo documentos: su Discovery y su corrección R1.
- Ninguna rama paralela toca `src/`, `tests/` ni `assets/`.

### 10.1 I-52 (RACKMIRROR)

| Cruce | Publicado | Planificado | Propietario | Frontera / coordinación |
|---|---|---|---|---|
| Producto: `RACKEDITAR` → Actualizar → redibujo | 0 archivos de `src/`, `tests/`, `assets/` | su contrato recorre «espejo → … → RACKEDITAR → Actualizar → redibujo»; CT-21D estudia las costuras de Selectivo (`RECONSTRUCTED`) | I-52 su producto; I-64 hosting y borrador | una mutación de RACKMIRROR sería un cambio externo para un borrador abierto; coordinar si alguna toca `EditX` o las costuras de redibujo |
| Host: **política documentada** | `TRUSTEDPATHS` especificada con una entrada no recursiva sin ruta de RackCad, `SPECIFIED_NOT_APPLIED` (§247) | siguiente campaña: S1-A..S4 no elegibles (§248) | I-52 / CAD manager | — |
| Host: **configuración leída** | por la sesión de I-52, en el perfil `<<Unnamed Profile>>`: la entrada autorizada una vez y 18 entradas antiguas (§248; no leída por I-64) | — | I-52 | no se infiere imposibilidad de un smoke de I-64 en otro perfil o momento; se acuerda |
| Eventos y nativo | ARX y observador gestionado de `CommandEnded` de investigación, bajo `eng/research`, fuera del producto | — | I-52 | no cargar I-64 en sesiones de host de I-52; su fila histórica `04NO-S` es un hecho acotado (§9.2) |
| Consulta de convivencia | enviada; **en cola**; sin acuse ni respuesta al publicar | — | — | estado real en la evidencia |

### 10.2 I-63 (ID20)

| Cruce | Publicado | Planificado | Propietario | Frontera / coordinación |
|---|---|---|---|---|
| Enumeración lógica de racks | Discovery de I-63: candidato a fundación común con I-64 y coordinación vía Master | I-63 detuvo Proposal y Freeze mientras se coordina; consulta **preparada por su Coordinator, no enviada** (sin canal; irá por el Owner) | contrato de I-63: ID20 semántica, providers y agregación; ID30 inventario runtime de navegación y UI | **elevado**: el mínimo común (identidad e igualdad, pertenencia al snapshot, vistas hermanas, hechos de colocación, captura incompleta, diagnósticos) no se diseña aquí |
| Superficie de UI | — | ID30 prepara alojamiento para el resumen de ID20 | I-64 aloja, I-63 aporta el modelo | sin consumo de código no integrado |
| Consulta de frontera | enviada; **respondida** por la sesión de I-63 (en la evidencia, literal resumido) | — | — | lectura de I-63: un índice de navegación sin semántica de métricas cae del lado de ID30, sin conflicto con la frontera escrita |

### 10.3 I-62

Solo documentos de proceso. Su Discovery registra que el Discovery de I-64 fue directo y que ninguna delegación de I-64 está acreditada
(`RECONSTRUCTED`). Una rama no integrada no cambia el protocolo vigente (C64-D1-07): las autoridades de enrutamiento se leen en `MainSha`.

**Salida de EXP-07:**
- Con I-63 hay un candidato a fundación común en la enumeración, ya elevado.
- Con I-52 los cruces son de producto planificado y de entorno de host, con consulta pendiente.
- Con I-62 no hay cruce de producto.
- No se consume producto no integrado.

## 11. DC-08 — Contratos consumidos: fuente → regla → prueba → hueco (C64-D1-05)

| Contrato consumido | Fuente normativa | Símbolo / regla | Prueba y aserción protectora | Hueco |
|---|---|---|---|---|
| Hermanas por GUID | ADR-0009 | `RackListBuilder.Build` agrupa por `Id` sin distinguir mayúsculas; `FindRackBlocks` busca por GUID | `RackListBuilderTests.Build_GroupsViewBlocksOfTheSameRackIntoOneEntry`; `Build_IgnoresEnvelopesWithoutIdOrKind` | `FindRackBlocks` (Plugin) sin prueba conductual |
| Vista y su decodificación | ADR-0010/0042/0044 | `RackViewCodec.Decode`; `RackViewAvailability` | `SharedViewCodecTests.CT04V2_DecodeAndCanonicalEncodeMatchEveryCharacterizedRow`; `AvailabilityFailsClosedForWrongKindAndInvalidDecode` | — |
| Preservación por vista (desconocidos, versión, propiedades) | Freeze I-11; ADR-0039 | `RackEmbedComposer.Compose(source, …)` | `PersistenceReopenPreservationTests.Composer_TwoViews_KeepTheirOwnExtensionData`; `Composer_UpdatesIdentityViewSectionDesign_WithoutLosingMetadataOrDowngradingVersion`; `RackEmbedDocumentTests.PushBackEnvelope_PreservesUnknownFields_AndDoesNotDowngradeAHigherMinor`; `CustomPropertiesEnvelopeTests.TEnv10_PorElComposeDeUnBuildAnterior_ElMiembroSobrevive` | que cada `EditX` pase el sobre propio: solo guardas de fuente y Owner |
| Authored frente a efectivo | ADR-0034 + Freeze I-48 | `SelectiveEffectiveDesignResolver` | `SelectiveEffectiveDesignResolverTests.Prueba2_ElLITERAL_AUTHORED_NO_CAMBIA_AlResolver`; `Prueba2_CambiarLaVariableCambiaElEfectivo_SinTocarNadaDelRack` | autoridad estricta solo en Selectivo |
| Reconciliación de vínculos | Freeze I-48 | `LinkedPropertyReconciler.Reconcile` | `LinkedPropertyReconcilerTests.EL_DISENO_EDITADO_NO_PUEDE_PISAR_EL_LITERAL_CONGELADO_DE_UNA_PROPIEDAD_VINCULADA`; `UNA_PROPIEDAD_DESCONOCIDA_EN_EL_ESTADO_FINAL_SE_RECHAZA` | sin prueba con un registro distinto del leído al abrir (hoy inalcanzable por la modalidad) |
| Relectura acreditada antes de escribir | Freeze I-48 (G4B, R-01) | `RegistryCommit.Prepare`; `MutationDestinationBinding.Bind` | `RegistryCommitAccreditationTests.UN_RE_READ_CON_VARIABLEID_DUPLICADO_BLOQUEA_Y_NO_PRODUCE_NINGUN_DOCUMENTO`; `EL_EXECUTOR_NO_APLICA_LA_MUTACION_SOBRE_EL_DOCUMENTO_CRUDO_DEL_RE_READ` | solo en la ruta `BindingIntent` |
| AUTH-13 | Freeze I-58 + ADR-0044 | `RackAuthoredComparatorPorts` | `I58F1OracleChecks` (Cama → `Unreadable`, `Authored` nulo) | no consumido por Actualizar |
| Contrato Insertar/Actualizar de la sesión | I-15 (sin ADR) | `RackEditorSession.RequestUpdate` / `RequestInsert` | `RackEditorSessionTests.RequestUpdate_NullsViewAndSection_KeepsExistingId`; `RequestInsert_SetsContractEnsuresIdBuildsPayloadAndRaises` | la interpretación del host no tiene prueba conductual |
| Cierre sin pérdida silenciosa; ninguna ruta de cierre materializa | ADR-0029 D7/D8 | `EditorClosePolicy` | `EditorClosePolicyTests.NothingPendingMeansTheWindowMayCloseWithoutAsking`; `RichEditorCloseContractTests.NoCloseRouteMaterialisesAnything`; `TheFourEditorsWithoutScopeCloseWithoutAsking` | cuatro editores sin ámbito dirty (caracterizado) |
| Despacho por kind con error visible | I-10 | `KindHandlerRegistry` | `KindDispatchTests.TryGet_UnknownOrNullKind_ReturnsFalseAndNull` | — |
| Selección → grupos por RackId | Freeze I-55; AUTH-06/07 de I-57 | `RackProjectionSelectionFilter`; `RackPhysicalSelection` | `RackProjectionMaterializationRunTests.G15_A_ENTITIES_THAT_ARE_NOT_RACKS_ARE_IGNORED_WITH_A_NOTICE`; `G15_N_MANY_REFERENCES_OF_ONE_RACKID_SHARE_ONE_NEW_DEFINITION_AND_ONE_COMMIT` | — |
| Nombre lógico (solo lectura de `Name`) | Freeze I-60 | `RackEmbedDocument.Name` | `RackListBuilderTests.Build_NameIsFirstNonEmptyOfTheGroup_OrFallback` | I-64 no consume la asignación |

Consumo indirecto sin cambio: DimensionViews y la autoridad de propiedades personalizadas, a través de los editores y del acarreo del
sobre. Las clases y métodos citados existen en la base (`MEASURED`); **no** se ejecutaron. No hay contradicción en lo consumido (EXP-01
negativa, §13).

## 12. DC-09 — Disparadores M-01..M-08 (C64-D1-04)

### 12.1 M-01: dueños actuales y propuestos

| Materia | Dueño actual (`MEASURED`) | Dueño candidato | ¿Cambia? |
|---|---|---|---|
| Borrador | instancia de ventana modal + `RackEditorSession` + estado puro; vida = ventana | `DraftSession` por (documento, RackId); vida = sesión de workspace | sí |
| Dirty | ámbito en 2 editores; ninguno en 4 | ámbito por borrador (ADR-0029 D8) | sí |
| Contexto de rack actual | el comando, al inicio (`GetEntity`); no persiste | `SelectionContext` sincronizado | sí (nuevo) |
| Cierre | política de ventana | política de pestaña y de paleta | sí |
| Aceptación de Actualizar | el host tras `ShowModalWindow` | el puente de comandos de la workspace hacia el host | sí (disparador); la escritura sigue en el host |
| Comprobación de base | nadie (la modalidad la hace innecesaria) | compuerta de commit | sí (nuevo) |
| Authored persistido | sobre en el DWG | sin cambio | no |
| Pertenencia de hermanas | `FindRackBlocks` / `RackSiblingMembership` según operación | por decidir (EXP-02) | UNKNOWN |

**M-01 = ACTIVADO.**

### 12.2 M-02 y M-03 (EXP-09)

- **M-02. Pregunta registrada:** «¿la workspace cambia el significado persistido, el fallback legacy o la preservación de desconocidos?»
  Área: constructores de payload, `Compose`, ramas de identidad legacy y lectura tolerante.
  - Evidencia: si Actualizar desde la workspace reutiliza los mismos constructores y `Compose`, la preservación no cambia
    (`MEASURED` para las rutas actuales). Pero hoy Actualizar **estampa** un GUID en sobres sin Id (§3.2) y RACKLISTA oculta esos sobres:
    la workspace debe decidir si navega a ellos, si los rechaza o si conserva la estampa.
  - **Resultado: UNKNOWN dependiente de diseño, tratado como activado.** Decide: Coordinator + Architect; Owner si el trato legacy cambia
    de forma visible.
- **M-03. Pregunta registrada:** «¿la coexistencia con RACKEDITAR o el trato de documentos existentes cambian fuera de lo pedido?» Área:
  RACKEDITAR, escaneos y lectura tolerante.
  - Mientras un RACKEDITAR modal está abierto no hay eventos (`RECONSTRUCTED`). Tras su commit, un borrador de la workspace del mismo
    rack queda obsoleto, y eso entra en el alcance pedido.
  - Al abrir la workspace sobre un DWG existente solo hay lecturas (`INFERENCE` del diseño pedido).
  - Si el Freeze cambia el comportamiento de RACKEDITAR (comprobación de base, informe de fallos) o muestra de otra manera sobres
    ilegibles o de versión futura, sería un cambio fuera de lo pedido salvo decisión explícita.
  - **Resultado: UNKNOWN dependiente de diseño, tratado como activado.** Decide: Coordinator + Architect; Owner si cambia RACKEDITAR.

### 12.3 M-04..M-08

- **M-04, M-05, M-06 y M-07:** activados; confirmados por el Coordinator para la dirección pedida (CD-I64-D1-01).
- **M-08 (activado, conservador):** delta normativo exacto por ADR.
  - **ADR-0010, «Decisión»:** fija Actualizar e Insertar «cuando un rack existente se abre mediante `RACKEDITAR`». La workspace añade otra
    vía de apertura y, si se adopta, una precondición de base vigente que puede rechazar un Actualizar. Precedente: ADR-0042 complementó
    ADR-0010 con precondiciones de Insertar y una nota fechada. Modificación si la precondición alcanza al Actualizar de RACKEDITAR;
    complemento si solo cubre la vía de la workspace.
  - **ADR-0032 D6:** lista las fronteras transaccionales del editor Selectivo. Si cambiar de pestaña debe comprometer los campos
    pendientes en el borrador, se amplía esa lista.
  - **ADR-0029:** su censo D1 es por clases `Window` y D9 regula modales. Una superficie alojada queda fuera de ese censo, pero eso **no**
    es por sí solo una contradicción. Un ADR propio puede extender el contrato funcional a las superficies alojadas sin modificar
    ADR-0029.
  - **ADR-0019, ADR-0006 y ADR-0009:** sin delta.
  - **Distinción:** la workspace necesita **su ADR de arquitectura** antes de implementar (WORKFLOW §8), lo que no es M-08. M-08 se activa
    por las modificaciones posibles de ADR-0010 y ADR-0032 D6.

**Arquetipo:** NEW ARCHITECTURE (brief), sin rebajar. **Materialidad propuesta:** M-01, M-04, M-05, M-06, M-07 y M-08 activados; M-02 y
M-03 `UNKNOWN` tratados como activados hasta las decisiones citadas.

## 13. Expansiones EXP-01..09 (D1-R1)

| EXP | Resultado | Razón y salida |
|---|---|---|
| EXP-01 | negativa | Las dos reglas de pertenencia están gobernadas con alcance propio y explícito (I-55 V5 §3.7: «Actualizar y PVME conservan su política»). El inventario de productores de I-57 cubre AUTH-06 sin listar `ScanEnvelopes`, pero I-57 declara como consumidores obligatorios a I-52 e I-55 y ADR-0044 decide reutilizar autoridades existentes. Ninguna contradicción consumida por I-64; ausencias en FOUNDATIONS y cifras antiguas de WORKFLOW §7 y ARCHITECTURE §7.3/§8 son descriptivas (hallazgos laterales) |
| EXP-02 | **positiva (condicional), escalada** | La regla consumida «pertenencia de vistas a un rack» tiene dos candidatas para un consumidor nuevo (§9.4). Cada una está acreditada en su alcance; cuál gobierna la workspace se eleva al Coordinator y Architect y forma parte del mínimo común con I-63. Para «cambio externo de un rack» no hay ambigüedad: es una capacidad nueva, negativa razonada |
| EXP-04 | **positiva, cerrada** | inventario por símbolo de §7.2 con huecos explícitos |
| EXP-05 | **positiva, cerrada como matriz** | §8.2; ningún PASS |
| EXP-06 | **positiva, cerrada** | matriz §9.12 |
| EXP-07 | **positiva, delta** | §10 |
| EXP-08 | **positiva, cerrada** | separación abajo |
| EXP-09 | **positiva** | M-08 (D1); M-02 y M-03 (D1-R1); M-01 resuelto como activado sin EXP-09 |
| EXP-03 | negativa | sobre, stores y legacy localizados |

**EXP-08 — deuda actual, exposición nueva, alcance y diferidos** (`MEASURED` para lo actual; `INFERENCE` para lo demás):
- **Deuda actual (existe hoy):**
  - commit parcial entre vistas;
  - `Failure` tras commit;
  - faltantes de biblioteca no mostrados;
  - renombrados fallidos invisibles;
  - mensaje de Dinámico que no cuenta borrados;
  - la ruta `BindingIntent` informa «no aplicado» si falla el `Regen` posterior.
- **Exposición nueva con modeless:**
  - base y registro antiguos que sobrescribirían en silencio cambios externos;
  - borradores que sobreviven a la ventana;
  - cierre sin ámbito dirty en cuatro editores, con pérdida de borradores al cerrar pestañas;
  - eventos durante la edición, que hoy no existen por la modalidad;
  - RACKEDITAR modal concurrente con borradores abiertos.
- **Alcance necesario para ID30:**
  - comprobación de base en el Actualizar de la workspace;
  - ámbito dirty por borrador;
  - política de cierre de pestañas;
  - resultado de fallo coherente en la vía de la workspace.
- **Diferidos posibles:** corregir las rutas históricas de RACKEDITAR y su atomicidad, y mostrar faltantes en RACKEDITAR.
- **Decisiones reservadas:**
  - si RACKEDITAR adopta la comprobación de base (M-03; Owner);
  - si el Actualizar de la workspace debe ser atómico entre vistas (M-04);
  - el trato de sobres sin Id (M-02/M-03);
  - la alternativa C de stale (Owner/ADR).

**¿Qué EXP debió activarse y no se activó, y por qué?**
- En D1 debieron activarse EXP-06 (la `Failure` posterior al commit no es determinable desde fuera) y EXP-02 (dos reglas de
  pertenencia). Se corrigió en D1-R1.
- En esta versión no queda ninguna sin evaluar. EXP-03 se mantiene negativa con razón.
- El candidato a fundación común no es una EXP de I-64: es una elevación al Master.

## 14. Plan de gates y enrutamiento por tarea (C64-D1-07; propuesta, no Freeze)

- **Gates del brief frente a LIFECYCLE §7:** F1, F2 y F3 tienen resultado observable. F4 debe terminar con Actualizar desde la workspace y
  conflicto detectado. F7 mezcla conformidad (READY-06) con rendimiento y regresión.
- **Caracterización antes de migrar** (ADR-0029 D13) en cada F5.
- **Tareas reales de F1** (el gate conserva su resultado funcional; las tareas no se fragmentan para rebajar riesgo). Clasificación por
  routing.md §§1-3, sin elegir ni sondear modelos:

  | Tarea de F1 | Resultado verificable | Clase y effort semántico | Observación |
  |---|---|---|---|
  | T-F1-1 Caracterización del cierre y la intención de los editores que se alojarán | pruebas que fijan lo actual | CHARACTERIZATION, Balanced | ADR-0029 D13 |
  | T-F1-2 Sesión por documento y modelo de contexto (Application, puro) | pruebas Core | implementación de pruebas y mecánica, Balanced | — |
  | T-F1-3 Puente central de eventos con fuente simulada y guarda de fuente | pruebas O-01, O-02, O-07, O-09 | implementación transversal a capas, corta, Deep | — |
  | T-F1-4 Host modeless (paleta o ventana) con contenido mínimo y sincronización de selección fina | DLL para el smoke | implementación transversal a capas, Deep; puede exigir horizonte largo | el Worker nunca usa AutoCAD; el smoke es del Owner |
  | Smoke temprano | O-08 y escenarios §9.10 | frontera S-01/S-10 | Owner |

  Si una tarea exige de verdad una celda no acreditada, queda **pendiente de decisión o de una sonda autorizada**; no se declara elegible
  por necesidad. Un enrutamiento `advisory` no acredita capacidades ni consumo desconocidos.
- **Precondiciones de la primera delegación:**
  - línea base nueva de `config.toml`;
  - `docs/automation/state/I-64.yml`;
  - autoridades leídas en `MainSha`.
- **I-62:** su rama no cambia el protocolo vigente.

## 15. Incógnitas y su tratamiento

| Tema | Estado | Prueba o cierre | Responsable | Fase |
|---|---|---|---|---|
| Foco y teclado de WPF en la superficie modeless | UNKNOWN | smoke O-08 | Owner | tras F1 |
| Evento gestionado de undo/redo | UNKNOWN | prueba en host de alternativas | Worker + Owner | F1/F4 |
| Coste por etapa de la travesía y del índice | UNKNOWN | medición sobre DWG del Owner | Worker + Owner | F2 |
| Latencias y memoria (nueve escenarios del brief) | UNKNOWN | mediciones con instrumento a decidir | Worker + Owner | F2-F7 |
| Validez de H-LOCK | INFERENCE | prueba de host con manejador y segundo comando | Worker + Owner | F4 |
| Regla de pertenencia para la workspace | escalada | decisión de diseño; mínimo común con I-63 vía Master | Coordinator, Architect, Master | antes del Freeze |
| Alternativa de stale y dependencias de la base | por decidir | Proposal → Architect + Coordinator; Owner si C | Coordinator | Proposal |
| Trato de sobres sin Id | por decidir | Proposal | Coordinator + Architect; Owner si visible | Proposal |
| Semántica de fallo del Actualizar de la workspace | por decidir | Proposal (M-04) | Coordinator + Architect | Proposal |
| Seleccionar y Centrar con varias vistas y copias | por decidir | Proposal; matriz OV | Coordinator | Proposal |
| ¿Cambiar de pestaña es frontera interna (ADR-0032 D6)? | por decidir | Proposal; ADR | Coordinator + Architect | Proposal |
| ADR propio y deltas de ADR-0010 y ADR-0032 | requerido | Architect + Coordinator; Owner acepta | Coordinator | antes de implementar |
| Convivencia del smoke con el host de I-52 | consulta en cola | respuesta de I-52; acuerdo con el CAD manager | Owner / I-52 | antes del smoke |
| D0-R1 y D0-R2 | no recibidas (historia) | no bloquean (C64-D1-07) | — | — |

Ninguna de estas incógnitas impide cerrar el Discovery. La regla de pertenencia y el mínimo común con I-63 sí impiden **congelar**
la arquitectura del inventario hasta que decidan el Coordinator y el Master.

## 16. Siguiente alcance propuesto

1. **Revisión del Coordinator** de D1-R1, de la escalada EXP-02 y de la elevación del mínimo común con I-63 al Master.
2. **Proposal V1** cuando se autorice. Debe cubrir «PROPOSAL MUST DEFINE» del brief, el borrador de ADR propio y los deltas de ADR-0010 y
   ADR-0032 D6. El inventario queda condicionado a la decisión del Master.
3. **Paquete del Architect** sobre la Proposal completa, con modo de revisión e independencia declarados.

Nada de lo anterior autoriza producto: `IMPLEMENTATION AUTHORIZATION = NO`.

## 17. Delta D1-R1 por ID

| ID | Disposición | Cambios en este documento |
|---|---|---|
| C64-D1-01 | **resuelto** | §1 (punto 5); §3.4 (`BindingIntent` y su fallo posterior al commit); §9.12 (matriz de fallos con las cuatro clases); EXP-06 positiva (§13); O-12 (§8.2) |
| C64-D1-02 | **resuelto** | §1 (punto 6), §2 (Q08, Q16), §7.1, §9.4 y §9.8: `Design` como cadena, sin diseño tipado ni resolución; etapas separadas; corrección sobre `forceValidity`; coste `UNKNOWN` con medición propuesta; sin nuevo enumerador |
| C64-D1-03 | **resuelto** | §9.7 (precedente E, reglas, hipótesis H-LOCK acotada con verificación); §9.13 (lecturas y escrituras por sistema); §9.14 (riesgos de normalización); Cama sin comparador heredado |
| C64-D1-04 | **resuelto** | §12: M-01 activado con mapa de dueños; M-02 y M-03 con EXP-09 (UNKNOWN tratado como activado); M-08 con delta exacto; distinción ADR propio / modificación; EXP-02 reevaluada |
| C64-D1-05 | **resuelto** | §3.3 (intención explícita interpretada por el host; cancelar no escribe); §3.4 (`BindingIntent`); §7.2 (EXP-04); §7.3 (censo por tipo y límites); §8.2 (EXP-05); §11 (trazas con aserción) |
| C64-D1-06 | **resuelto, con consulta pendiente** | §10: delta de ramas; política documentada frente a configuración leída; fila histórica acotada; consultas a I-52 (en cola) e I-63 (respondida); candidato común elevado |
| C64-D1-07 | **resuelto** | §14: tareas reales de F1 sin elegir modelos; celdas no acreditadas quedan pendientes; I-62 no cambia el protocolo; D0-R1/R2 como historia no bloqueante (§15) |
