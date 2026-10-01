# I-63 — Focused Discovery (F0-DISCOVERY) — ID20, Computed Parameters & Project Summary

> **Estado:** Discovery entregado para revisión del Coordinator. **No** es Proposal, Freeze, revisión del Architect ni GATE PASS.
> Lo hizo **directamente** la sesión principal responsable (orden CD-I63-F0-01, opción (a)), sin Controller, Worker, Architect,
> subagente ni otro proceso de IA y sin delegación §16. Las decisiones de diseño quedan abiertas (§16).

## 0. Identidad, base y método

- Unidad I-63 (ID funcional ID20). Rama `architecture/parametros-calculados-resumen-proyecto`; contrato
  [I-63-parametros-calculados-resumen-proyecto.md](I-63-parametros-calculados-resumen-proyecto.md).
- **Base leída:** `b557ea3be4adeb496c353d9915b38fffaf2d9316`. Fuera de `docs/` es idéntica a `origin/main`
  `819955d61a6da4c811a11fbd11b5dca13f634b7c`: `git diff --name-only 819955d6 b557ea3b` no lista nada fuera de `docs/`. Por eso todo
  lo dicho sobre código vale para `main`.
- **Fuentes de autoridad:** mandato del Owner ([copia](../automation/decisions/I-63-owner-mandate.original.txt)); AGENTS; WORKFLOW;
  [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §§3-5; [FOUNDATIONS](../FOUNDATIONS.md), solo como índice; ADR y Freezes de §9;
  código y pruebas citados por ruta y línea.
- **Clase de cada afirmación** (LIFECYCLE §4):
  - `[MEASURED]`: leído en el código o en Git, o ejecutado, en esta base.
  - `[RECONSTRUCTED]`: deducido de varias lecturas cuyo encadenamiento no se ejecutó.
  - `[INFERENCE]`: juicio sobre lo leído.
  - `[UNKNOWN]`: no acreditado.
- Las rutas `src/…` son relativas a la raíz. Un número de línea identifica la lectura en esta base, no un contrato.
- **Pruebas existentes ejecutadas** (no se escribió ninguna; solo para responder preguntas concretas, §7): ocho clases de Core,
  179/179 superadas, selección > 0 en cada clase.

## 1. Resumen de hallazgos

1. **El motor de I-49 tiene un solo namespace activo, `projectVariable`.** `Rack` y `Project` están reservados y nada productivo
   los crea `[MEASURED]`. Activarlos exige cambiar contratos consumidos: `SymbolNamespace`, la validez de clave de `SymbolId` (hoy
   exige un GUID) y la resolución del cualificador del binder (fija `ProjectVariable`). Además, persistir una referencia `Rack.*`
   cambiaría el formato de disco: un *token* de namespace desconocido no se puede leer (§3, §4).
2. **Ya existen fases implícitas en el Selectivo:**
   1. acreditar el registro;
   2. evaluar las variables (grafo y ciclos solo entre variables);
   3. evaluar cada propiedad vinculada (literal, referencia o fórmula, con consumidor de ámbito `Rack`);
   4. obtener el diseño *effective*;
   5. resolver la geometría;
   6. construir dibujo y BOM.

   Las fórmulas de propiedad **no son nodos del grafo**. Un símbolo `Rack.*` derivado del *effective* y leído desde una fórmula de
   propiedad crearía un ciclo entre fases que el grafo actual no ve (§14) `[RECONSTRUCTED]`.
3. **No hay una enumeración lógica de racks en Application.** El escaneo de definiciones es único, pero vive en el Plugin
   (`RackBlockFinder.ScanEnvelopes`), y cada consumidor aplica su propia política: RACKLISTA, RACKBOMTOTAL, nombre automático,
   variables, propiedades personalizadas y duplicación (§6) `[MEASURED]`.
4. **Ya hay tres «conteos de racks» con semánticas distintas** `[MEASURED]`:
   - RACKLISTA: definiciones agrupadas por GUID, incluidas las no colocadas; «copias» = máximo de referencias directas.
   - RACKBOMTOTAL: solo colocados y con autoridad, saltando los ilegibles con aviso.
   - Su ventana muestra **«N racks · M copias»** (`ConsolidatedBom.RackCount` / `TotalCopies`).

   Un `Project.TotalRacks` nuevo sería una **segunda autoridad** si no se reconcilia con estos (M-01; EXP-02) `[INFERENCE]`.
5. **«Racks lógicos» frente a «copias físicas» no está decidido.** Una referencia copiada en AutoCAD comparte definición y GUID, y
   el BOM la multiplica por `Copies` `[MEASURED]`. El mandato dice «racks lógicos, no BlockReferences»; falta decidir si las copias
   colocadas de un mismo GUID cuentan una vez o N (§16, D-OPEN-01) `[UNKNOWN]`.
6. **Las métricas de producto no tienen semántica común:**
   - «Frente», «nivel» y «posición» significan cosas distintas en Selectivo (bahía; niveles por bahía; celdas × tarimas, medio
     frente por ajuste geométrico), Dinámico (frente transversal con carriles, niveles y fondo por frente), Push Back (ranuras por
     lado, fondo efectivo por nivel), Cantilever (estaciones, intervalos y niveles de brazo, sin tarimas), Cabecera (un marco) y
     Cama (una cama).
   - No hay hoy una función que cuente posiciones de tarima de un rack completo. La matriz (§13) da la mayoría de celdas como
     `NeedsContract` `[MEASURED]`/`[INFERENCE]`.
7. **Autoridad entre vistas hermanas desigual** `[MEASURED]`:
   - el BOM compara las vistas hermanas solo en Selectivo; en los demás kinds aprueba la primera vista;
   - desde I-58 existen comparadores AUTH-13 para Dinámico, Push Back, Cantilever y Cabecera, que el BOM no consume;
   - la Cama no tiene comparador.
8. **Candidato a fundación común con I-64 (ID30).** I-64 declara un «inventario ligero de racks por dibujo»; ID20 necesita
   deduplicar por RackId; y en Application no existe la autoridad de enumeración que ambos necesitarían. Se registra para
   coordinarlo **vía Master** antes del diseño, conforme al mandato (§8) `[INFERENCE]`.
9. **Arquetipo:** M-01, M-05, M-06 y M-08 activados; M-02 condicional; M-07 sigue UNKNOWN, acotado a una decisión de diseño.
   Recomendación: mantener **NEW ARCHITECTURE provisional** hasta la Proposal (§10).

## 2. DC-01 — Comportamiento observable actual

| # | Comportamiento | Evidencia | Clase |
|---|---|---|---|
| 1 | Una referencia sin llaves `Rack` o `Project` da `ReservedName`, aunque exista una variable con ese nombre. La forma `palabra.miembro` (p. ej. `Rack.Frentes * 2`, `Project.TotalRacks`) da `UnknownNamespace`. Con llaves (`{Rack}`, `{Rack.Frentes}`) son nombres ordinarios de variable | `src/RackCad.Application/Expressions/ExpressionBinder.cs:220-222, 322-326`; `FunctionRegistry.cs:174-186`; `ExpressionBinderTests` (`UN_RESERVADO_SIN_LLAVES_…`, `LA_SINTAXIS_DE_NAMESPACE_DA_UNKNOWNNAMESPACE`, datos `"Rack.Frentes * 2"`, `"Project.TotalRacks"`); `ExpressionRoundTripTests.cs:136-139`, que usa los nombres `Rack` y `Rack.Frentes` como variables válidas | `[MEASURED]`, ejecutado §7 |
| 2 | El símbolo de ámbito `Rack` solo lo crean tablas sintéticas; consumidor de ámbito `Project` + símbolo `Rack` = `ScopeViolation` | `SymbolTable.cs:9-17`; `ExpressionBinder.cs:376-379`; `ExpressionSymbolModelTests` | `[MEASURED]` |
| 3 | Las fórmulas de las propiedades vinculables del Selectivo se enlazan con consumidor de ámbito `Rack` y solo pueden leer símbolos `projectVariable`. Su valor debe ser > 0 | `ProjectVariables/LinkedPropertyExpressionAuthoring.cs:127, 155`; `BindingInspection.cs:359-364` | `[MEASURED]` |
| 4 | Propiedades vinculables productivas: **dos**, `SelectiveVerticalClearance` y `SelectivePalletTolerance` (longitudes) | `ProjectVariables/SelectiveLinkedProperties.cs:186-214` | `[MEASURED]` |
| 5 | RACKLISTA: una fila por GUID (comparador `OrdinalIgnoreCase`), con nombre y kind tomados del primer valor no vacío entre las hermanas. Omite sin aviso los sobres sin `Id` o sin `Kind`. Lista también definiciones sin colocar (copias 0). «Copias» = máximo de referencias directas entre las definiciones de vista. Etiqueta Push Back con el *token* crudo | `Persistence/RackListBuilder.cs:54-73, 82-96`; `RackCad.Plugin/RackInventarioCommands.cs:50-73`; `RackCountInvariantCharacterizationTests`; `RackListBuilderTests` | `[MEASURED]` |
| 6 | RACKBOMTOTAL: lee el registro una vez y cuenta solo racks colocados. Aborta ante una definición colocada inclasificable, un kind sin handler, un rack bloqueado o una referencia de variable rota. Salta con aviso un rack sin autoridad o con *payload* ilegible. Multiplica por «copias» | `RackCad.Plugin/RackInventarioCommands.BomTotal.cs:47-232` | `[MEASURED]` (lectura) |
| 7 | La ventana del BOM total muestra «N racks · M copias». `RackCount` = racks listados tras saltos; `TotalCopies` = suma de `max(1, Copies)` | `Bom/ConsolidatedBom.cs:40-41`; `RackCad.UI/RackConsolidatedBomWindow.xaml.cs:24-25`; `ConsolidatedBomBuilderTests` | `[MEASURED]` |
| 8 | El BOM del Selectivo cuenta las instancias de bloque que producen los *builders* de vista (con las decoraciones apagadas), no un modelo de conteo separado | `Systems/Selective/SelectiveBomBuilder.cs:54-84` | `[MEASURED]` |
| 9 | La numeración de niveles dibujada en el frontal del Selectivo usa solo los niveles de la **primera** bahía | `Systems/Selective/SelectiveFrontalBuilder.cs:333-339` | `[MEASURED]` |
| 10 | El nombre lógico automático de un rack nuevo (I-60) es `«Prefijo» N`, con N uno por encima del máximo de su familia. El nombre no es identidad | `Persistence/RackLogicalNameAllocator.cs:9-19, 64-98` | `[MEASURED]` |

## 3. DC-02 — Dueños de reglas y valores

| Concepto | Dueño actual (símbolo) | Gobierno | Clase |
|---|---|---|---|
| Identidad de símbolo | `SymbolId` = `(SymbolNamespace, Key)`. La clave debe ser un GUID válido para cualquier namespace registrado | `Expressions/SymbolId.cs:85-108, 151-154`; ADR-0043 D5 | `[MEASURED]` |
| Namespaces | Conjunto cerrado de `SymbolNamespace`, hoy solo `ProjectVariable`. Tabla de *tokens* explícita | `SymbolId.cs:16-70` | `[MEASURED]` |
| Ámbitos | `SymbolScope.Project` (activo) y `SymbolScope.Rack` (reservado) | `SymbolTable.cs:13-17` | `[MEASURED]` |
| Contexto de operación | `ExpressionContext`: tabla de un *snapshot*, `FunctionRegistry.Productive`, `LengthUnits.Authority` y límites normativos. Se construye solo por `internal Create` | `Expressions/ExpressionContext.cs:60-77` | `[MEASURED]` |
| Del nombre a la identidad | `ExpressionBinder.Bind(syntax, context, consumerScope)`: único camino productivo. El cualificador `#…` resuelve siempre como `ProjectVariable` | `ExpressionBinder.cs:48-90, 317-383` (en 334) | `[MEASURED]` |
| Grafo y ciclos | `DependencyGraph` sobre las entradas de la tabla; `RegistryEvaluation` decide orden, ciclos y causas raíz | `Expressions/DependencyGraph.cs:16-20`; `RegistryEvaluation.cs:8-80` | `[MEASURED]` |
| Productores de contexto | `ProjectVariablesExpressionAdapter.From`, siempre con ámbito `Project`; las fábricas de autoría de propiedades vinculadas | `ProjectVariables/ProjectVariablesExpressionAdapter.cs:13-63`; `LinkedPropertyAuthoringContext.cs:62-89`; `LinkedPropertyExpressionAuthoring.cs:60-109` | `[MEASURED]` |
| *Effective* del Selectivo | `SelectiveEffectiveDesignResolver`, punto único | `Systems/Selective/SelectiveEffectiveDesignResolver.cs:11-31, 112-172`; ADR-0034 + Freeze de I-48 | `[MEASURED]` |
| *Authored* entre hermanas | `SelectiveAuthoredAuthority` (Selectivo). Los puertos AUTH-13 de `RackAuthoredComparatorPorts` (Dinámico, Push Back, Cantilever y Cabecera) son de I-58; la Cama no tiene | `ProjectVariables/SelectiveAuthoredAuthority.cs:84`; `Systems/Shared/RackAuthoredComparator.cs:65-86`; `RackAuthoredComparator.FourKinds.cs:14-27` | `[MEASURED]` |
| Autoridad para cotizar | `BomAuthoredAuthority`: compara las hermanas solo en Selectivo y aprueba la primera vista en los demás kinds | `Bom/BomAuthoredAuthority.cs:82-125` (104-110) | `[MEASURED]` |
| Identidad de rack | `RackEmbedDocument.Id` (GUID) | `Persistence/RackEmbedDocument.cs:49-50`; ADR-0009 | `[MEASURED]` |
| Identidad de vista | `RackEmbedDocument.View` y `Section` | `RackEmbedDocument.cs:40-47`; ADR-0010 | `[MEASURED]` |
| Escaneo de definiciones | `RackBlockFinder.ScanEnvelopes`: omite layouts, anónimos y xref; cuenta referencias directas con `directOnly: true, forceValidity: false` | `RackCad.Plugin/RackBlockFinder.cs:42-92` (66, 85) | `[MEASURED]` |
| Clasificación sin inventar RackId | `ProjectVariableScanProjection.Project` (sobre ilegible, sin `Id` o sin `Kind` → inclasificable) | `ProjectVariables/ProjectVariableScanProjection.cs:30-65` | `[MEASURED]` |
| Vocabularios de kind | *Token* del sobre (`selective`, `dynamic`, `cabecera`, `cama`, `pushback`, `cantilever`); `RackSystemKind`, cuyo miembro `Selective` es la **cabecera** y `SelectiveRack` el selectivo, e incluye `Larguero`; etiquetas en `RackListBuilder.KindLabel`, `IRackKindHandler.BomLabel` y `SystemDescriptor` | `RackEmbedDocument.cs:16-26`; `Domain/Systems/Shared/RackSystemKind.cs:4-30`; `Systems/Shared/SystemRegistry.Default.cs:25-37` | `[MEASURED]` |
| Agregación del BOM | `ConsolidatedBomBuilder.Build` (componentes × copias) | `Bom/ConsolidatedBom.cs:44-117` | `[MEASURED]` |
| Conteos de producto | **Sin dueño único** para frentes, niveles o posiciones de un rack o proyecto (§13) | búsquedas de §17 | `[MEASURED]` (ausencia, con los huecos de §17) |

## 4. DC-03 — Persistencia, campos, *legacy* y desconocidos

- **Sobre del rack** (`RackEmbedDocument`):
  - campos `SchemaVersion`, `Kind`, `View`, `Section`, `Id`, `Name`, `Design` y `CustomProperties` (JSON crudo);
  - `[JsonExtensionData]` preserva los campos desconocidos (`RackEmbedDocument.cs:35-75`) `[MEASURED]`;
  - una vista nula o vacía se lee como lateral en RACKLISTA (`RackListBuilder.cs:117-118`) `[MEASURED]`.
- **Identidad *legacy*:**
  - un sobre sin `Id` o sin `Kind` no es un rack para RACKLISTA (`RackListBuilder.cs:54-56`) ni para el BOM total
    (`ProjectVariableScanProjection.cs:37-42`) `[MEASURED]`;
  - el BOM total aborta si esa definición está colocada (`RackInventarioCommands.BomTotal.cs:84-96`), y RACKLISTA la omite sin
    avisar (`RackInventarioCommands.cs:53-56`) `[MEASURED]`;
  - FOUNDATIONS («Rack Identity», limitaciones) dice que un sobre *legacy* sin `Id` no permite inferir hermanas `[MEASURED]`.
- **Expresiones persistidas:**
  - `{"Node":"Reference","Namespace":<token>,"Id":<clave>}`; leer un *token* de namespace desconocido falla
    (`Persistence/PersistedBoundExpressionJson.cs:93-103, 170`) `[MEASURED]`;
  - las usan `ProjectVariablesStore` (`:277`), `SelectivePalletDesignStore` (`:127`) y `MutationPlan` (`:265`) `[MEASURED]`;
  - consecuencia: persistir `Rack.*` o `Project.*` haría ilegibles los documentos para las *builds* anteriores (fallo cerrado) →
    M-02 `[RECONSTRUCTED]`.
- **Dinámico *legacy*:** `RackProject.DynamicSystem`, sistema resuelto persistido, frente a `DynamicDesign`; el handler acepta
  ambos (`RackCad.Plugin/KindHandlers/DynamicKindHandler.cs:37-42`) `[MEASURED]`.
- **Custom Properties (ADR-0039):** V1 solo admite cadenas literales; `RACKCAD_CUSTOM_PROPERTIES` en el NOD y JSON crudo en el
  sobre (FOUNDATIONS «Custom Properties») `[MEASURED]`.
- **Lo que no se encontró:** ningún campo persistido de «métrica calculada», «resumen» o «conteo» en sobres, documentos de diseño
  ni NOD `[MEASURED]` (búsquedas de §17).

## 5. DC-04 — Del documento al *effective*, por sistema

Así compone hoy el BOM cada kind; las composiciones viven en los handlers del Plugin (`RackCad.Plugin/KindHandlers/*`).

| Kind (*token*) | Deserialización | Resolución | Variables de proyecto | Modelo resuelto | Clase |
|---|---|---|---|---|---|
| `selective` | `SelectivePalletDesignStore.Deserialize` | `SelectiveEffectiveDesignResolver.Resolve` → `SelectiveGeometryResolver.Resolve(design, catalog)` | **sí** (dos propiedades vinculables) | `SelectiveRackSystem` | `[MEASURED]` (`SelectiveKindHandler.BuildBom`) |
| `dynamic` | `RackProjectStore.Deserialize` | `DynamicRackSystemResolver(catalog).Resolve(DynamicDesign).System`, o `DynamicSystem` *legacy* | no | `DynamicRackSystem` | `[MEASURED]` (`DynamicKindHandler.cs:37-42`) |
| `pushback` | `RackProjectStore.Deserialize` | `PushBackResolver(catalog).Resolve(PushBackDesign)` | no | `PushBackSystem` (`Structure`, `Composite`) | `[MEASURED]` (`PushBackKindHandler.cs:40-50`) |
| `cantilever` | `RackProjectStore.Deserialize` | `CantileverLineEditorAssembler(sections).Build(CantileverLineDesign)`; el catálogo de secciones lo carga el Plugin (`StructuralSectionCatalogAccess.TryLoad`) | no | línea ensamblada (`IsValid`, `Bom`) | `[MEASURED]` (`CantileverKindHandler.cs:47-66`) |
| `cabecera` | `RackProjectStore.Deserialize(...)?.Header` | ninguna: la configuración es el modelo | no | `RackFrameConfiguration` | `[MEASURED]` (`CabeceraKindHandler.cs:35-39`) |
| `cama` | `FlowBedConfigurationStore.Deserialize` | `FlowBedLateralBuilder().Build(config, catalog)` | no | instancias de lateral | `[MEASURED]` (`CamaKindHandler.cs:37-46`) |

**Fases del Selectivo** (`SelectiveEffectiveDesignResolver.cs:132-172`) `[MEASURED]`:
1. acreditar el registro;
2. `RegistryEvaluation` de todas las variables;
3. cada `PropertyValues` en orden ordinal: `LinkedPropertyInspection` (literal, referencia o fórmula);
4. `authored.ToDomain()` + `WriteEffective`;
5. el consumidor resuelve la geometría.

Las reglas de derivación (`Domain/Systems/Selective/SelectivePalletDesign.cs:9-16`) fijan que la separación entre niveles depende de
la holgura vertical (`Clearance`) y que la altura del poste depende del nivel superior `[MEASURED]`.

## 6. DC-05 — Llamadores, consumidores y lectores

- **`RackBlockFinder.ScanEnvelopes`** lo llaman `CustomPropertiesExecutor`, `ProjectVariableMutationExecutor`, `RackCommandSupport`,
  `RackInventarioCommands` (RACKLISTA y RACKBOMTOTAL), `RackSelectivoCommands`, `RackVariablesCommands` y
  `Systems/Shared/RackNewRackName` (I-60) (`grep ScanEnvelopes(`) `[MEASURED]`.
  - Cada uno aplica su propia política de validez, colocación y agrupación (comentario `RackBlockFinder.cs:51-55`) `[MEASURED]`.
- **Agrupación por RackId:**
  - `RackListBuilder` (`GroupBy … OrdinalIgnoreCase`);
  - el diccionario de hermanas de RACKBOMTOTAL (`OrdinalIgnoreCase`, `BomTotal.cs:73`);
  - `PlanReadSet` (orden `OrdinalIgnoreCase`, `ProjectVariables/PlanReadSet.cs:313`);
  - `RackProjectionPipeline`, que **ordena** sus grupos con `Ordinal` (`Views/Placement/RackProjectionPipeline.cs:78-79`); el
    comparador con que se formaron esos grupos no se inspeccionó `[UNKNOWN]`.
- **Contratos del motor:**
  - consumidores productivos de `ExpressionContext`/`SymbolTable`: `ProjectVariables/*` (adaptador, autoría, *preflight*, *commit*,
    recuperación) y `SelectiveEffectiveDesignResolver`;
  - lectores persistidos de `SymbolNamespace`: `PersistedBoundExpressionJson`;
  - binder con consumidor `Rack`: `LinkedPropertyExpressionAuthoring` (`grep` de `ExpressionContext.Create`, `RegistryEvaluation.`
    y `SymbolScope.`) `[MEASURED]`.
- **Consumidores de conteos:**
  - `RackListRow`/`RackListWindow` (RACKLISTA);
  - `RackConsolidatedBomWindow` («N racks · M copias»);
  - exportadores CSV/XLSX del BOM consolidado (`ConsolidatedBomCsvExporter`, `ConsolidatedBomXlsxExporter`) `[MEASURED]`.
- **Huecos de búsqueda:** el Plugin no es comprobable en Core (ADR-0003). Los caminos que solo se ejecutan en AutoCAD se leyeron,
  no se ejecutaron `[MEASURED]` en la lectura, `[UNKNOWN]` en el comportamiento.

## 7. DC-06 — Pruebas y guardas

**Corrida focal de pruebas existentes** (sin editar fuentes):
- base `b557ea3b`, árbol limpio antes y después; SDK de usuario 8.0.423;
- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter <8 cláusulas FullyQualifiedName~RackCad.Tests.<Clase>>`;
- 2026-10-01, de 16:48:09Z a 16:49:16Z;
- TRX fuera del repositorio, SHA-256 `F5A9D461…DA21`.

| Clase | Seleccionadas | Resultado |
|---|---:|---|
| `ExpressionBinderTests` | 65 | superadas |
| `ExpressionSymbolModelTests` | 44 | superadas |
| `SelectiveBomAuthorityTests` | 31 | superadas |
| `ProjectVariableScanProjectionTests` | 17 | superadas |
| `RackListBuilderTests` | 10 | superadas |
| `RackEmbedDocumentTests` | 7 | superadas |
| `RackCountInvariantCharacterizationTests` | 3 | superadas |
| `ConsolidatedBomBuilderTests` | 2 | superadas |
| **Total** | **179** | **179 superadas, 0 fallos, 0 omitidas** |

**Guardas leídas, no ejecutadas:** `ExpressionRoundTripTests`, `ExpressionSyntaxGrammarTests`, `ExpressionSyntaxDiagnosticsTests`,
`ExpressionFunctionRegistryTests`, `ExpressionParserGuardTests`, `G11CandidateValidationTests` (mide tiempos con `Stopwatch` en la
cadena de I-49, `:306-309`) y las pruebas de AUTH-13 (`I58*`) que cita FOUNDATIONS.

**Huecos declarados** `[MEASURED]` (ausencia en las búsquedas de §17):
- ninguna prueba compara RACKLISTA con RACKBOMTOTAL sobre el mismo dibujo;
- ninguna fija la semántica «definido sin colocar», ni la de «copias» con un número de referencias distinto entre vistas;
- ninguna protege ciclos entre fases a través de un símbolo calculado, porque esos símbolos no existen;
- ninguna fija que `BomAuthoredAuthority` apruebe la primera vista en los kinds no selectivos;
- `RackCountInvariantCharacterizationTests.BomRepresentativeStillUsesOneAuthoredDocumentPerRack` es una **guardia de fuente**
  (busca texto en el código), no una prueba de comportamiento.

## 8. DC-07 — Archivos calientes e intersecciones activas

Ramas activas observadas el 2026-10-01 a las 16:40:05Z: I-52 `c1982b2a`, I-62 `85ae4324`, I-63 `b557ea3b` e I-64 `d8a02163`;
`main` `819955d6`.

| Iniciativa | Cruce textual | Cruce funcional | Autoridad compartida |
|---|---|---|---|
| **I-64 (ID30, Workspace)** | `docs/ROADMAP.md`: las dos filas van tras I-60; `git merge-tree` dio conflicto de contenido `[MEASURED]` (evidencia §9.4). Su rama solo cambia `docs/` (5 archivos) `[MEASURED]` | **No inspeccionado en código**: su rama no tiene producto. Su contrato declara un «navegador e inventario ligero de racks por dibujo» y «selección de AutoCAD → RackId lógico» `[MEASURED]`, lectura del contrato de su rama | **Candidato a fundación común:** enumeración lógica de racks del dibujo (identidad, *legacy*, colocado frente a definido, hermanas). No existe autoridad en Application (§6) `[INFERENCE]` |
| **I-62 (portabilidad del Principal)** | Fila en otra tabla de ROADMAP; `git merge-tree` sin conflicto `[MEASURED]`. Su rama solo cambia `docs/` `[MEASURED]` | Ninguno de producto esperado; **no inspeccionado** | Protocolo de ejecución delegada (`routing.md`, AUTOMATION_PLAN §16): afecta cómo delegará I-63, no a su producto `[INFERENCE]` |
| **I-52 (RACKMIRROR)** | Edita ROADMAP (su fila y la de I-57), HANDOFF y el índice de ADR (ADR-0036) `[MEASURED]`; no se midió el cruce con I-63 | **No inspeccionado.** Su rama no cambia `src/`, `tests/` ni `assets/` respecto de su base `[MEASURED]`. RACKMIRROR crea racks espejados, lo que podría influir en los conteos | Identidad de racks nuevos (AUTH-15) `[UNKNOWN]` |

Archivos calientes para el diseño futuro, según WORKFLOW §7:
- `src/RackCad.Plugin/*Commands*.cs`, incluidos `RackInventarioCommands*.cs`;
- `docs/ROADMAP.md`;
- `docs/HANDOFF.md`, solo al cerrar `[MEASURED]`.

## 9. DC-08 — Fundaciones consumidas o extendidas

### 9.1 Freeze final integrado de I-49 (acreditación)

- **Freeze vigente:** [`I-49-consensus-freeze-v6-a1-a2-a3-r1.md`](I-49-consensus-freeze-v6-a1-a2-a3-r1.md), blob `440ae61a…`, en el
  segundo padre del merge `a61850a6` y en `origin/main`.
  - Es el «Consensus Freeze **correctivo** V6 + A1 + A2 + A3-R2 + ADR-0043 R1»: `R1` es la primera revisión del freeze, no A3-R1.
  - El freeze anterior `…-v6-a1-a2-a3.md` (blob `cba59b7f…`) queda histórico `[MEASURED]`.
- **Los blobs que declara coinciden con los medidos en `origin/main`** `[MEASURED]`:
  - Proposal V6 `ef4db3aa…`;
  - A1 `d6201908…`;
  - A2 `49a92533…`;
  - A3-R2 `4da6ef3c…` (`I-49-proposal-v6-amendment-a3-multi-cause-dependency-failures.md`);
  - ADR-0043 aceptado y corregido `dd88bf06…`.
- ADR-0043 («Base exacta y frontera de decisión») registra esa base, e indica que A3 original y A3-R1 recibieron CHANGES REQUIRED
  `[MEASURED]`.
- **Correspondencia con el código** `[MEASURED]`:
  - ADR-0043 reserva `Rack.*`, `Project.*` y el ámbito `Rack` para ID20 «sin registrar» (`:368, 2031-2057`);
  - el código coincide: un solo namespace, palabras reservadas, `UnknownNamespace` y ningún símbolo `Rack` productivo (§2, filas
    1-2);
  - el consumidor de ámbito `Rack` de las fórmulas de propiedad **no** crea símbolos `Rack`, así que no contradice el ADR.

### 9.2 Fundaciones candidatas

| Fundación | Fuente | Código | Consumidores | Pruebas | Resultado DC-08 |
|---|---|---|---|---|---|
| Rack Identity | ADR-0009 (aceptado) | `RackEmbedDocument.Id`; agrupaciones de §6 | RACKLISTA, RACKBOMTOTAL, duplicación, variables, nombres | `RackEmbedDocumentTests`, `RackListBuilderTests`, `RackCountInvariantCharacterizationTests` (ejecutadas) | Coincide `[MEASURED]`. La entrada no dice si las copias de una definición son un rack o varios: es una decisión abierta (D-OPEN-01), no una discrepancia |
| View Identity | ADR-0010 + ADR-0009 | `View`/`Section`; `RackListBuilder.DescribeViews` | RACKLISTA, edición multivista | `RackCountInvariantCharacterizationTests` | Coincide `[MEASURED]` |
| Authored vs Effective | ADR-0034 + Freeze de I-48 | `SelectiveEffectiveDesignResolver`; `BomAuthoredAuthority` | BOM, dibujo, *preview* | `SelectiveBomAuthorityTests` (ejecutadas) | Coincide `[MEASURED]`; la autoridad estricta es solo del Selectivo, como dice la entrada. No es un veredicto de soporte para los demás sistemas |
| Project Variables | ADR-0034 + I-48 | `ProjectVariablesExpressionAdapter`; `UsableProjectVariablesRegistry` | motor, *effective* del Selectivo | `ProjectVariableScanProjectionTests` (ejecutadas); FOUNDATIONS cita más | Coincide en lo leído `[MEASURED]` |
| Expression Engine | ADR-0043 + Freeze correctivo R1 (§9.1) | `Expressions/*` | §6 | `ExpressionBinderTests`, `ExpressionSymbolModelTests` (ejecutadas) | Coincide `[MEASURED]`; **sin entrada en FOUNDATIONS**: su ausencia no prueba que la fundación no exista |
| Auto Rack Naming | [I-60-freeze.md](I-60-freeze.md) §3 | `RackLogicalNameAllocator`; `RackNewRackName` | caminos de creación | no ejecutadas aquí | Coincide en lo leído `[MEASURED]`; sin entrada en FOUNDATIONS. El nombre no es identidad |
| Shared View Foundation (AUTH-13; AUTH-08/12) | Freeze de I-58/I-59 + ADR-0044 | `RackAuthoredComparatorPorts` | sesiones de vista e inserción del Plugin; **no** el BOM | `I58*`/`I59*` según FOUNDATIONS; no ejecutadas | Coincide `[MEASURED]`; que el BOM no lo consuma no es una contradicción (no está declarado como consumidor) |
| Custom Properties (frontera) | ADR-0039 | `CustomPropertiesDocument`; sobre crudo | — | no ejecutadas | Ver §9.3 |

**Ninguna discrepancia consumida de clase A** (§11, EXP-01).

### 9.3 ADR-0039 D-16: delimitación

ADR-0039 §13 dice `[MEASURED]`:
- un consumidor futuro referencia por `CustomPropertyId` y lee de la autoridad;
- el ADR no fija la sintaxis de I-49 ni reclama `Rack`/`Project`;
- «la relación entre ID24 e ID20 se decide en ID20».

Las Custom Properties V1 son **cadenas literales** `[MEASURED]`, y el motor solo maneja `double` finitos
(`SymbolTable.cs:53-62`) `[MEASURED]`. Por tanto `[INFERENCE]`:
- una CustomProperty no se convierte en símbolo numérico sin un contrato nuevo, y las fórmulas de Custom Properties quedan fuera de
  alcance;
- la Proposal debe fijar que CustomProperty (metadato), ProjectVariable (*authored*, editable) y ComputedParameter (derivado, solo
  lectura) son autoridades disjuntas, y que ningún ComputedParameter se persiste como ProjectVariable ni como CustomProperty.

## 10. DC-09 y EXP-09 — Disparadores M y arquetipo

EXP-09, con el disparador, la pregunta, el área y la salida que fijó el Coordinator.

| ID | Estado | Evidencia | Clase |
|---|---|---|---|
| M-01 | **Activado** | Aparecería una autoridad nueva de métricas, con riesgo de **segunda autoridad** frente a `ConsolidatedBom.RackCount`/`TotalCopies` y a RACKLISTA (§2, filas 5-7) | `[INFERENCE]` sobre hechos `[MEASURED]` |
| M-02 | **Condicional** | Se activa si alguna expresión persiste referencias `Rack.*`/`Project.*` (*token* de namespace en el formato de disco, §4); no se activa si los parámetros solo existen en ejecución | `[RECONSTRUCTED]` |
| M-03 | No activado (a verificar en la Proposal) | Los nombres con llaves (`{Rack.Frentes}`) siguen siendo variables; las expresiones persistidas guardan `SymbolId`, no texto, así que los documentos existentes no cambian de significado | `[INFERENCE]` |
| M-04 | **UNKNOWN** | Hay que decidir cómo se presentan los estados Unavailable/NotApplicable y si un resumen aborta, salta o marca (las políticas de RACKLISTA y RACKBOMTOTAL difieren, §2) | `[UNKNOWN]` |
| M-05 | **Activado** | Ampliar `SymbolNamespace`, la validez de clave de `SymbolId` (hoy GUID) y la resolución del cualificador cambia contratos que consumen Project Variables, I-48 y el formato persistido | `[MEASURED]` |
| M-06 | **Activado** | Se añade o extiende el punto de símbolos built-in de I-49 y un punto de *providers* por kind | `[INFERENCE]` sobre el mandato y ADR-0043 |
| M-07 | **UNKNOWN acotado** | Hoy no hay ninguna abstracción de *provider*; la composición por kind está en los handlers del Plugin. Que sea M-07 depende del diseño: un catálogo cerrado y estático de métricas por kind (patrón `SelectiveLinkedProperties`) extiende puntos existentes, mientras que un registro o *framework* genérico sería mecanismo transversal nuevo. **Falta una decisión de diseño de la Proposal**, no de Discovery | `[UNKNOWN]` |
| M-08 | **Activado** | ADR-0043 fija hoy «exactamente un namespace activo» (`SymbolId.cs:8-14`; ADR D5/P25): activar `rack`/`project` modifica una afirmación de un ADR aceptado y exige un ADR nuevo o sucesor | `[MEASURED]` |

**Recomendación (la confirma el Coordinator):** M-01, M-05, M-06 y M-08 bastan para FOUNDATION EVOLUTION como mínimo. Con M-07
UNKNOWN, LIFECYCLE §3 obliga a mantener **NEW ARCHITECTURE provisional** hasta que la Proposal elija entre catálogo cerrado y
registro, y el Architect lo revise.

## 11. EXP-01..09

Las expansiones con disparador presente se **proponen**, no se ejecutan: solo EXP-09 estaba autorizada.

| EXP | Disparador | Evaluación |
|---|---|---|
| EXP-01 | Contradicción entre FOUNDATIONS, ADR, Freeze y código | **Negativo razonado.** §9 no halló contradicción en lo consumido. Que falten entradas para el Expression Engine y Auto Rack Naming es una ausencia, no una contradicción (CD-ID20-G0-06). La etiqueta de Push Back que falta en `RackListBuilder` la reconoce el propio código (`:92-94`) |
| EXP-02 | Autoridad ambigua | **Positivo; se propone.** Pregunta: ¿qué autoridad única cuenta racks (definidos o colocados; GUID o copias) y qué autoridad de etiqueta de kind usa un resumen? Área: `RackListBuilder`, `ConsolidatedBom`, `RackInventarioCommands*`, `ProjectVariableScanProjection`, `SystemRegistry`/`KindLabel`/`BomLabel`. Salida: tabla de semánticas por consumidor con la decisión que falta |
| EXP-03 | *Legacy* no localizable | **Negativo.** Los sobres sin `Id` o `Kind`, la vista nula y el `DynamicSystem` *legacy* se localizaron (§4) |
| EXP-04 | Contrato con consumidores a más de un salto | **Positivo; se propone.** Pregunta: inventario completo de lectores de `SymbolNamespace`, `SymbolId`, `SymbolScope` y JSON persistido, incluidos los futuros ID23 y la UI de I-48. Área: `Expressions/*`, `ProjectVariables/*`, `Persistence/*Store.cs`, UI de las propiedades vinculables. Salida: lista de lectores con su efecto ante un *token* nuevo |
| EXP-05 | Invariante sin prueba protectora | **Positivo; se propone.** Invariantes candidatos sin prueba: deduplicar por RackId con copias y sin colocar; ausencia de ciclo entre fases para un símbolo calculado; autoridad de hermanas por kind. Hacen falta pruebas nuevas en un gate autorizado |
| EXP-06 | Fallo silencioso o semántica no determinable | **Positivo; se propone.** RACKLISTA omite sin aviso los sobres sin `Id`/`Kind` y toma el primer kind sin detectar divergencia; el efecto de `forceValidity: false` sobre referencias borradas es `[UNKNOWN]`. Salida: decisión de fallo cerrado o estado visible para el resumen |
| EXP-07 | Rama activa sobre la misma autoridad | **Positivo; coordinación.** I-64 declara un inventario de racks por dibujo (§8). Se pide la coordinación vía **Master** que prevé el mandato; Discovery no asigna la propiedad |
| EXP-08 | Deuda preexistente que el cambio expondría | **Positivo; se registra.** Faltan la etiqueta de Push Back en RACKLISTA y la comparación de hermanas no selectivas en el BOM; RACKLISTA cuenta definiciones sin colocar y el BOM no. Un resumen por sistema las haría visibles. No se arreglan aquí |
| EXP-09 | M-07 UNKNOWN | **Ejecutada** (§10): matriz M-01..08; M-07 sigue UNKNOWN acotado a la decisión de catálogo cerrado frente a registro |

**¿Debió activarse alguna EXP que no se activó?** Candidata para la revisión del Coordinator: EXP-03 sobre la *legacy* de
**copias** (referencias anidadas en otros bloques; `directOnly`) quedó como negativa porque la regla está localizada, pero su
efecto en el conteo no se midió `[UNKNOWN]`.

## 12. Trazado de Q1..Q14 (mandato, «DISCOVERY REQUIRED»)

| Q | Respuesta | Sección |
|---|---|---|
| 1 | Contrato de símbolos: `SymbolId(namespace, key)` con clave GUID, un namespace, ámbitos `Project`/`Rack` y nombre de visualización no identitario `[MEASURED]` | §3, §2 |
| 2 | `ExpressionContext` inmutable por operación: tabla, funciones, unidades y límites; constructor interno `[MEASURED]` | §3 |
| 3 | Grafo: `DependencyGraph` solo entre símbolos de la tabla; las fórmulas de propiedad no son nodos; ciclos y causas raíz en `RegistryEvaluation` `[MEASURED]` | §3, §14 |
| 4 | Fases: implícitas (variables → propiedades → *effective* → geometría → dibujo/BOM) `[MEASURED]`; no hay fases nombradas | §5, §14 |
| 5 | *Effective* por sistema: resolutores puros en Application, compuestos en los handlers del Plugin; solo el Selectivo usa variables `[MEASURED]` | §5 |
| 6 | Conteos actuales: RACKLISTA, RACKBOMTOTAL y su resumen «N racks · M copias», con semánticas distintas `[MEASURED]` | §2, §6 |
| 7 | RACKLISTA reutilizable: `RackListBuilder` es puro, pero orientado a la presentación (etiquetas, «(sin nombre)», definidos) `[MEASURED]`. Reutilizarlo como autoridad de conteo exige decidir su semántica `[INFERENCE]` | §2, §16 |
| 8 | BOM: `ConsolidatedBomBuilder` reutilizable para piezas y componentes. **No** reutilizable como fuente de métricas de producto: cuenta instancias de dibujo, aprueba la primera vista en kinds no selectivos y salta racks `[MEASURED]`/`[INFERENCE]` | §2, §3 |
| 9 | Dedupe por RackId: `OrdinalIgnoreCase` en los consumidores leídos; *legacy* sin `Id` nunca recibe identidad `[MEASURED]`; copias pendientes (D-OPEN-01) `[UNKNOWN]` | §4, §6 |
| 10 | Métricas dispersas: alturas y fondos por kind, niveles y posiciones por celda, `StorageCapacity` de Push Back (capacidad de un marco, otro concepto), numeración dibujada `[MEASURED]` | §13 |
| 11 | Comunes de verdad: el kind del sistema y el conteo lógico de racks. Frentes, niveles, posiciones, altura y fondo **no** tienen semántica común entre kinds `[INFERENCE]` | §13 |
| 12 | Riesgos de ciclos: fórmula de propiedad → `Rack.*` *effective*; variable de proyecto → `Project.*` agregado `[RECONSTRUCTED]` | §14 |
| 13 | Cruce con ID30: candidato a fundación común en la enumeración lógica; textual en ROADMAP `[MEASURED]`/`[INFERENCE]` | §8 |
| 14 | 100 % Application/Domain: los *providers* y la agregación pueden ser puros si reciben instantáneas de sobres, conteos de referencias y catálogos ya cargados. El escaneo de BTR, las referencias y la carga de catálogos de secciones son del Plugin `[INFERENCE]` sobre §5-§6 | §5, §6 |

## 13. Matriz sistema × métrica candidata

Leyenda: `Supported` = existe hoy una autoridad única con semántica clara; `NotApplicable` = el concepto no existe en el sistema;
`Unavailable` = el concepto existe pero no hay dato fiable; `NeedsContract` = hay datos, pero falta decidir la semántica o la
autoridad. Unidades: pulgadas (`LengthUnits.Authority`), conteos enteros. **Soporte actual ≠ propuesta futura.**

| Métrica (candidata) | Selectivo | Dinámico | Push Back | Cantilever | Cabecera | Cama |
|---|---|---|---|---|---|---|
| **SystemKind** (*token* del sobre) | Supported: `selective` | Supported: `dynamic` | Supported: `pushback` | Supported: `cantilever` | Supported: `cabecera` | Supported: `cama` |
| **Frentes** | NeedsContract: `Bays` por fondo (`SelectiveRackSystem.cs:41,50`); la rejilla horizontal es compartida entre fondos | NeedsContract: `Fronts`, con `IsActive` (`DynamicRackFront.cs:99-113`) | NeedsContract: ranuras compartidas y lados A/B (`PushBackSystem.cs:18,44`; `PushBackCompositeSystem.cs:16-48`) | NeedsContract: `StationCount` e `IntervalCount` (`CantileverLineDesign.cs:339,367`), góndola sencilla o doble | NotApplicable | NotApplicable |
| **Niveles** | NeedsContract: varían por bahía (`SelectiveBay.Levels`, `SelectiveRackSystem.cs:125`); la numeración dibujada usa la primera bahía (§2, fila 9) | NeedsContract: `LoadLevels` por frente | NeedsContract: por frente y lado | NeedsContract: niveles de **brazo** (`LevelCount`, `:216`), no niveles de tarima | NotApplicable | NotApplicable |
| **Posiciones de tarima** | NeedsContract: `PalletCount` por celda + piso; medio frente por ajuste geométrico (`SelectiveFrontalBuilder.cs:351-353`) | NeedsContract: carriles (`PalletCount`) × niveles × `PalletsDeep` por frente `[INFERENCE]` | NeedsContract: fondo efectivo por nivel (`PushBackResolvedFront.PalletsDeep`, `PushBackSystem.cs:258-261`) | NotApplicable (cargas largas, sin tarimas) `[INFERENCE]` | NotApplicable | NotApplicable |
| **Altura** | NeedsContract: `Height` = bahía más alta (`SelectiveRackSystem.cs:16-17`). Es fuente segura después de resolver, nunca durante la evaluación de propiedades (§14) | NeedsContract: `Height` por frente; zonas de altura de cabecera | NeedsContract: estructura compartida | NeedsContract: altura de columna por estación (automática o manual) | Supported (candidato): `RackFrameConfiguration.Height` (`:17`) `[INFERENCE]` | NotApplicable |
| **Fondo** | NeedsContract: `DepthCount` + `FondoDepths` (`SelectiveRackSystem.cs:29,35`) | NeedsContract: `PalletsDeep` por frente | NeedsContract: fondo efectivo por nivel y lado | NotApplicable o NeedsContract (base o brazo) `[UNKNOWN]` | Supported (candidato): `RackFrameConfiguration.Depth` (`:18`) `[INFERENCE]` | NeedsContract: `LaneDepth` (`FlowBedConfiguration.cs:13`) |

**Agregados de proyecto:**
- `TotalRacks`: NeedsContract (D-OPEN-01/02).
- `RacksBySystem`: NeedsContract, porque depende de `TotalRacks`; la clave candidata es el *token* del sobre.
- `TotalFrentes` y `TotalPosiciones`: NeedsContract, sin semántica común entre kinds; agregarlos por kind evita equivalencias falsas
  `[INFERENCE]`.

**Primer conjunto candidato (PROPUESTA, no congelada):**
- Por rack: `SystemKind`.
- Por proyecto: `TotalRacks` lógico y `RacksBySystem`, con identidad, colocación y *legacy* decididas.
- Como métricas por kind, solo donde una autoridad resuelta sea inequívoca tras el contrato: Altura y Fondo de Cabecera; frentes del
  Selectivo como `Bays` del fondo 0, si el Owner acepta esa semántica.

Todo lo demás queda `NeedsContract` hasta la Proposal. **Sin identidad *legacy* inventada:** un sobre sin `Id` no cuenta como rack;
cuenta como «inclasificable» (estado de diagnóstico).

## 14. Símbolos, fases, grafo y riesgos de reentrada

- **Caso del mandato** (`Rack.Altura` → *effective* `Height` → `Height` *authored* = `Rack.Altura + 2`) `[RECONSTRUCTED]`:
  - la altura no es hoy una propiedad vinculable;
  - sí lo es `VerticalClearance` (§2, fila 4), y por las reglas de derivación la altura resuelta depende de ella (`SelectivePalletDesign.cs:9-16`);
  - una fórmula `VerticalClearance = Rack.Altura / 10` se evaluaría en la fase de propiedades (§5, paso 3), **antes** de que exista
    la altura;
  - ese ciclo cruza fases y `DependencyGraph` no lo ve, porque las fórmulas de propiedad no son nodos.
- **Agregados en variables** `[RECONSTRUCTED]`:
  - si `Project.TotalRacks` u otro agregado fuera legible desde una **variable de proyecto** (consumidor `Project`, fase previa a
    todos los racks), y un rack enlazara una propiedad a esa variable, el ciclo cruzaría proyecto → rack → agregado;
  - incluso sin ciclo, el valor del agregado depende de todos los racks: evaluarlo durante el registro supondría resolver el
    dibujo entero (rendimiento, §15).
- **Fronteras del modelo actual** `[MEASURED]`:
  - `ExpressionContext` es por operación y sin estado ambiente;
  - el cualificador fija `ProjectVariable`;
  - `BindingInspection` rechaza cualquier namespace distinto de `projectVariable` (`:359-364`);
  - un `Rack.*` productivo solo puede entrar por decisiones explícitas de contrato, no por accidente.
- **Hipótesis de fases para la Proposal** (no decidido):
  - (i) los *built-ins* previos a resolver: ninguno identificado;
  - (ii) parámetros posteriores a resolver: disponibles para consumidores posteriores, como el ID23 (cantidades de BOM en el
    contexto de un rack) y los resúmenes, y **no** para las fórmulas de propiedad;
  - (iii) agregados de proyecto: no disponibles para las variables de proyecto.
- La frontera que ya existe (los consumidores de ámbito `Rack` solo leen `projectVariable`) es la equivalente que menciona el
  mandato; la decisión es si se amplía y para qué consumidores `[INFERENCE]`.
- **Vía para ID23:** una tabla de símbolos de ámbito `Rack` construida *después* del *effective* de un rack concreto (su
  `SelectiveRackSystem` u homólogo), en un `ExpressionContext` propio y sin persistirlos como ProjectVariable. Es compatible con P26.1
  (el núcleo no nombra `VariableType`) `[INFERENCE]`.
- **Procedencia para ID28/ID29** (candidatos de la Proposal): `SymbolId`, *provider*/kind, autoridad de origen (§13), fase y
  RackId. Las causas raíz de `RegistryEvaluation` ya son el modelo de explicación existente `[INFERENCE]`.

## 15. Plan de caracterización de rendimiento (escenarios del mandato)

**Mediciones disponibles hoy** `[MEASURED]`:
- RACKLISTA y RACKBOMTOTAL recorren las definiciones **una vez** con `forceValidity: false`, deliberadamente barato (`RackBlockFinder.cs:51-55`);
- RACKBOMTOTAL resuelve cada rack aprobado completo y lee el registro una sola vez;
- `SelectiveBomBuilder` apaga las decoraciones porque un rack de 20 frentes × 4 fondos generaba miles de instancias por BOM (`:54-59`);
- `G11CandidateValidationTests` mide tiempos de la cadena de I-49.

**No hay ninguna medición de métricas de ID20 ni tiempos de comandos registrados en AutoCAD** `[UNKNOWN]`.

**Plan futuro** (necesita pruebas nuevas en un gate autorizado; sin cachés previos y sin tiempos inventados):

| Escenario | Instrumento propuesto | Variables |
|---|---|---|
| Métrica de un rack | prueba de Core con `Stopwatch` y conteo de llamadas a resolutores por *provider* | kind; tamaño del diseño |
| Agregación de proyecto | prueba de Core sobre instantáneas sintéticas de sobres (sin AutoCAD) | N = 1, 10, 100, 1000 racks |
| N racks y múltiples vistas | mismas instantáneas con 1-3 vistas y secciones laterales; comprobar un solo *resolve* por RackId | vistas y secciones por rack; copias |
| Lecturas repetidas | dos lecturas del resumen sobre la misma instantánea; contar resoluciones | sin caché frente a «resolver una vez por orden», el patrón de RACKBOMTOTAL |

## 16. Decisiones abiertas (insumos de la Proposal, no Freeze)

| ID | Decisión | Quién decide |
|---|---|---|
| D-OPEN-01 | Qué es «un rack» para `TotalRacks`: GUID lógico, copias colocadas o definido sin colocar; qué hacer con el sobre *legacy* sin `Id` | Owner (producto), con el Coordinator |
| D-OPEN-02 | Autoridad de enumeración lógica (nueva y pura, o adoptar una política existente) y su propiedad frente a I-64 | **Master Orchestrator** (fundación común) y Coordinator |
| D-OPEN-03 | Identidad de los símbolos calculados: *token* de namespace y forma de clave (`SymbolId` exige hoy un GUID); nombre visible frente a identidad | Proposal + Architect; ADR sucesor de ADR-0043 (M-08) |
| D-OPEN-04 | Disponibilidad por fase y consumidor: fórmulas de propiedad, variables de proyecto, ID23, resumen | Proposal + Architect |
| D-OPEN-05 | Semántica por kind de frentes, niveles, posiciones, altura y fondo (§13) | Owner (producto) |
| D-OPEN-06 | Autoridad de hermanas por kind para las métricas (`BomAuthoredAuthority` o puertos AUTH-13) y trato de la Cama | Proposal + Architect |
| D-OPEN-07 | Estados Unavailable, NotApplicable, Divergent o Unreadable y política de resumen parcial frente a abortar (M-04) | Proposal + Owner |
| D-OPEN-08 | Relación con el contador visible «N racks · M copias» de RACKBOMTOTAL: fuente única o convivencia declarada (M-01) | Owner + Coordinator |
| D-OPEN-09 | Clave de `RacksBySystem`: *token* del sobre frente a `RackSystemKind`; autoridad de etiqueta | Proposal |
| D-OPEN-10 | Persistir o no las referencias `Rack.*`/`Project.*` (M-02; compatibilidad hacia atrás) | Proposal + Architect + ADR |
| D-OPEN-11 | Frontera con CustomProperty según ADR-0039 D-16 (§9.3) | Proposal |
| D-OPEN-12 | Modelo neutral del resumen (totales, por sistema, por rack, diagnósticos) y unidades | Proposal |

**Preguntas reservadas al Coordinator:**
1. ¿Se eleva al Master D-OPEN-02 (EXP-07) antes de la Proposal?
2. ¿Qué EXP positivas (02, 04, 05, 06) se autorizan, y con qué límites?
3. ¿Se acepta el criterio de no versionar pruebas nuevas hasta que exista un gate que lo permita?

## 17. Búsquedas realizadas y huecos

**Búsquedas** (`grep` sobre `src/` y `tests/` en `b557ea3b`):
- `ScanEnvelopes(`, `RackListBuilder\.`, `ConsolidatedBom`, `BomAuthoredAuthority\.`;
- `GroupBy(... Id|RackId)`, `RackId, StringComparer`;
- `ExpressionContext.Create`, `SymbolTable.Create`, `RegistryEvaluation\.`, `SymbolScope\.`;
- `ReservedName|UnknownNamespace|ScopeViolation`, `Rack\.Frentes|"Rack"|Project\.Total|ID20`;
- `NumberFronts|NumberLevels`;
- `posiciones|PalletPositions|PositionCount|Capacity|TotalPallets|PalletSlots`;
- `RackCount|TotalCopies`;
- `class RackAuthoredComparatorPorts|RackAuthoredComparatorPorts\.`;
- `SystemRegistry\.`, `PersistedBoundExpressionJson`;
- `Stopwatch|Elapsed|Performance|Benchmark`.

**Huecos:**
- No se ejecutó ningún camino del Plugin (sin AutoCAD).
- No se inspeccionó el comparador de grupos de `RackProjectionPipeline`.
- No se midió el efecto de `forceValidity: false` ni de las referencias anidadas en el conteo de copias.
- No se leyó la UI de las propiedades vinculables más allá de su autoría.
- No se inspeccionó código de I-64, I-62 ni I-52: sus ramas no tienen producto.
- Ninguna afirmación funcional sobre ramas ajenas se presenta como medida.

## 18. Riesgos del mandato → estado tras Discovery

| Riesgo (orden G0 y mandato) | Estado |
|---|---|
| Contar vistas en vez de RackId | Mitigado en los consumidores actuales (agrupan por GUID) `[MEASURED]`; queda abierta la semántica de copias (D-OPEN-01) |
| *Legacy* sin identidad | Hoy no se cuenta como rack y no se inventa identidad `[MEASURED]`; falta decidir cómo se presenta (D-OPEN-07) |
| Hermanas divergentes o ilegibles | Comparación estricta solo en el Selectivo; la Cama no tiene (D-OPEN-06) |
| Aritmética duplicada | Real: tres conteos con semánticas distintas (EXP-02) |
| Fases y ciclos ocultos | Real entre fases; el grafo no los ve (§14) |
| Nombres visibles como identidad | El motor y RACKLISTA usan identidad; los nombres no se usan como clave `[MEASURED]` |
| Equivalencias falsas entre sistemas | Real (§13); se propone agregar por kind |
| Parciales como totales | RACKBOMTOTAL salta racks con aviso y su resumen «N racks» excluye los saltados `[MEASURED]` (D-OPEN-07/08) |
| Resoluciones repetidas | Sin medición; plan en §15 |
| Solapamiento con ID30 | Candidato a fundación común (§8, D-OPEN-02) |
