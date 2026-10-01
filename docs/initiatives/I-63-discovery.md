# I-63 — Focused Discovery (F0-DISCOVERY R1) — ID20, Computed Parameters & Project Summary

> **Estado:** ronda R1 del Discovery, que corrige la ronda R0 según la revisión del Coordinator (CD-I63-F0-R1-01: CHANGES REQUIRED)
> y completa las expansiones autorizadas (CD-I63-F0-R1-02). **No** es Proposal, Freeze, revisión del Architect ni GATE PASS.
> - Lo hizo **directamente** la sesión principal responsable, sin Controller, Worker, Architect, subagente ni otro proceso de IA y
>   sin delegación §16.
> - La ronda R0 se conserva en Git: commit `dbfe1150007b7ea6819df7277e35666fe67af056`, blob `d9426070818eaa14ceb976fca474248160fbf803`.
> - La elección del contrato compartido con I-64, la Proposal y el Freeze están **detenidos** mientras se coordina con el Master
>   (CD-I63-F0-R1-03).

## 0. Identidad, base y método

- Unidad I-63 (ID funcional ID20). Rama `architecture/parametros-calculados-resumen-proyecto`.
  [Contrato](I-63-parametros-calculados-resumen-proyecto.md); [decisiones](../automation/decisions/I-63.md);
  [evidencia](../automation/evidence/I-63-evidence.md).
- **Base de R1:** `dbfe1150`. Fuera de `docs/` es idéntica a `origin/main` `819955d61a6da4c811a11fbd11b5dca13f634b7c` (preflight
  2026-10-01T17:16:34Z, sin rebase), así que todo lo dicho sobre código vale para `main`.
- **Fuentes:** mandato del Owner ([copia](../automation/decisions/I-63-owner-mandate.original.txt)), en especial sus secciones RACK
  COUNTING, FIRST METRIC SET, EVALUATION PHASES, RACK INVENTORY COORDINATION WITH ID30 y DISCOVERY REQUIRED; AGENTS; WORKFLOW;
  [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §§3-5; [FOUNDATIONS](../FOUNDATIONS.md), solo como índice; ADR y Freezes de
  §10; código y pruebas por ruta y línea.
- Los documentos de I-64 e I-62 se leyeron como **hechos de coordinación**, no como fundaciones ni política integrada.
- **Clase de cada afirmación** (LIFECYCLE §4):
  - `[MEASURED]`: leído en el código o en Git, o ejecutado, en esta base.
  - `[RECONSTRUCTED]`: encadenamiento de lecturas que no se ejecutó.
  - `[INFERENCE]`: juicio sobre lo leído.
  - `[UNKNOWN]`: no acreditado.
  - `[EXTERNAL]`: documentación oficial de terceros; no es una ejecución en este equipo.
- Toda ausencia se limita a las rutas y búsquedas inspeccionadas (§22); no es una inexistencia universal.
- **Pruebas existentes ejecutadas** (sin escribir ninguna):
  - R0: ocho clases, 179/179;
  - R1: dos pruebas nuevas para preguntas nuevas, 9/9, compiladas sobre `dbfe1150` (§8).

## 1. Resumen R1

1. **Identidad lógica decidida; población abierta.** El mandato ya fija que N vistas o colocaciones del mismo RackId aportan **un**
   rack a `Project.TotalRacks`.
   - Lo que queda abierto es la **población admitida** (definida o colocada; ámbitos de colocación; casos incompletos) y sus
     **diagnósticos** (§20, P-01..P-03).
   - Falta de identidad, kind ausente o desconocido y fallo de una métrica son tres casos distintos (§16.1).
2. **Ya existe agrupación pura en Application.**
   - `RackListBuilder.Build` agrupa por `Id` con `OrdinalIgnoreCase` `[MEASURED]`.
   - `RackPhysicalSelection.Classify` forma los grupos de selección con el mismo comparador `[MEASURED]`.
   - Lo **no acreditado** es un contrato neutral **común** de captura, población, admisión y agrupación útil para ID20 e I-64 (§7.2).
3. **Hay dos capturas en el Plugin.** `ScanEnvelopes`, la barata, omite xref y no distingue el tipo de colocación. `RackSiblingScan`,
   de I-55, separa layout y anidada, marca xref y tantea el `Id` en sobres ilegibles. Cada consumidor aplica después su propia
   política (§7.2, §17).
4. **Las cifras de RACKLISTA y RACKBOMTOTAL son magnitudes distintas sobre poblaciones distintas, no una métrica duplicada.**
   - RACKLISTA: racks lógicos **definidos** (filas).
   - RACKBOMTOTAL: racks **cotizados** (`ConsolidatedBom.Racks`), y su ventana muestra `Racks.Count` y `Σ max(1, Copies)`.
   - La duplicación solo aparecería si ID20 definiera `TotalRacks` con una de esas poblaciones sin reconciliarla (§7.2, M-01).
5. **Fases por dependencia real, no por prefijo.**
   - Contar RackIds sobre una instantánea de sobres **no** exige resolver diseños ni catálogos.
   - Las métricas de capacidad o geometría sí lo exigen, en distinto grado (§15).
   - El ciclo VerticalClearance → altura → fórmula es un **riesgo de una extensión futura**, no un defecto que se pueda ejecutar hoy (§16.2).
6. **Compatibilidad precisa (prueba ejecutada en R1).** Hoy, un registro de variables que **contenga** una referencia
   `"Namespace":"rack"` se lee entero como `PresentButUnreadable`.
   - Los documentos que no contienen el *token* no cambian.
   - Lo que hay que diseñar es el caso de **un documento nuevo leído por una *build* antigua** (§5.2).
   - Mantener claves GUID para los símbolos nuevos sigue siendo posible (§16.3).
7. **Métrica numérica real para la vía ID23:** **frentes del Selectivo**.
   - Su fuente es el conteo estructural del diseño (`Bays.Count` y `BaysForFondo(k).Count`), que el resolver mantiene 1:1.
   - No depende del catálogo ni de las propiedades vinculables `[MEASURED]`.
   - Falta decidir el **producto**: cómo se cuentan los fondos con distinto número de frentes y si los frentes vacíos cuentan (§14,
     P-05). `SystemKind` es texto y no ejercita la vía numérica.
8. **Arquetipo:** NEW ARCHITECTURE como arquetipo de trabajo.
   - M-01 está activado como **creador** de una autoridad de métricas.
   - M-02, M-04 y M-07 cuentan como activados para la planificación: cada uno tiene evidencia, una pregunta acotada y quién decide (§11).
9. **Coordinación con el Master:** la consulta preparada por el Coordinator **no** se envió, porque esta sesión no tiene canal con el
   Master. Se entrega al Owner (§21). I-64 confirmó por canal entre sesiones que no diseña ese mínimo común.

## 2. Disposición de los hallazgos R63-DISC-01..07

| ID | Corrección aplicada | Dónde |
|---|---|---|
| R63-DISC-01 | Se retira la alternativa «1 frente a N» para el mismo RackId. D-OPEN-01 pasa a ser población, ámbitos de colocación, admisibilidad y diagnósticos. Se separan los tres tipos de exclusión. No se añade ninguna métrica de copias | §1.1, §16.1, §20 |
| R63-DISC-02 | Se corrigen las afirmaciones absolutas: sí hay agrupación pura; lo que falta acreditar es un contrato común. Se mapea productor → población → filtros → igualdad → magnitud → consumidor. Ninguna cifra distinta se trata como duplicación por sí sola | §1.2-1.4, §7.2 |
| R63-DISC-03 | Las fases se basan en dependencias reales, con una tabla por métrica: entradas, dependencias, fase mínima, consumidor, consistencia, catálogo y geometría, efectos e invalidación. La disponibilidad previa al *resolve* sigue siendo hipótesis | §15, §16.2 |
| R63-DISC-04 | Se separa el hecho (solo `projectVariable`; clave GUID) de las alternativas. Se precisa la compatibilidad en tres casos con una prueba ejecutada. No se decide persistencia ni ADR | §5.2, §16.3 |
| R63-DISC-05 | Se descarta `SystemKind` como prueba numérica y se documenta una métrica numérica real (frentes del Selectivo). La matriz distingue fuente actual, pregunta pendiente, dependencias o fase y datos no fiables, con una alternativa preferida y una segunda | §14 |
| R63-DISC-06 | NEW ARCHITECTURE se mantiene; se revisan M-01 (creador), M-02, M-04 y M-07. Cada UNKNOWN lleva evidencia, una pregunta acotada y quién decide | §11 |
| R63-DISC-07 | Se cierra el inventario del motor (EXP-04). Los grupos de `RackProjectionPipeline` se forman con `OrdinalIgnoreCase` (no solo se ordenan con `Ordinal`). DC-07 se actualiza con el Discovery de I-64 y el avance de I-62, conservando el *snapshot* R0 | §7.1, §7.3, §9 |

## 3. DC-01 — Comportamiento observable actual

| # | Comportamiento | Evidencia | Clase |
|---|---|---|---|
| 1 | Una referencia sin llaves `Rack` o `Project` da `ReservedName`, y la sintaxis `palabra.miembro` da `UnknownNamespace`. Con llaves (`{Rack}`, `{Rack.Frentes}`) son nombres ordinarios de variable | `Expressions/ExpressionBinder.cs:220-222, 322-326`; `FunctionRegistry.cs:174-186`; `ExpressionBinderTests` (R0); `ExpressionRoundTripTests.cs:136-139` | `[MEASURED]` |
| 2 | Ningún camino productivo crea un símbolo de ámbito `Rack`; consumidor `Project` + símbolo `Rack` = `ScopeViolation` | `SymbolTable.cs:9-17`; `ExpressionBinder.cs:376-379`; `ExpressionSymbolModelTests` (R0) | `[MEASURED]` |
| 3 | Las fórmulas de propiedad del Selectivo se enlazan con consumidor `Rack` y solo leen `projectVariable`; su valor debe ser > 0 | `ProjectVariables/LinkedPropertyExpressionAuthoring.cs:127, 155`; `BindingInspection.cs:359-364` | `[MEASURED]` |
| 4 | Propiedades vinculables productivas: `SelectiveVerticalClearance` y `SelectivePalletTolerance` | `ProjectVariables/SelectiveLinkedProperties.cs:186-214` | `[MEASURED]` |
| 5 | Un registro de variables cuyo JSON contiene una referencia con `"Namespace":"rack"` (clave GUID) se lee como `PresentButUnreadable` y sin documento | `tests/RackCad.Tests/G8PersistenceIntegrationContractTests.cs:171, 179-185` (ejecutada en R1) | `[MEASURED]` |
| 6 | RACKLISTA: una fila por RackId lógico **definido** (`OrdinalIgnoreCase`), colocado o no. Omite sin aviso los sobres sin `Id` o `Kind`. Nombre y kind = primer valor no vacío entre las hermanas. Copias = máximo de referencias directas | `Persistence/RackListBuilder.cs:46-79`; `RackCad.Plugin/RackInventarioCommands.cs:50-85` | `[MEASURED]` |
| 7 | RACKBOMTOTAL cotiza racks **colocados** con autoridad y sin bloqueo. Aborta o salta según el caso (§7.2). Su ventana muestra `ConsolidatedBom.RackCount` (= `Racks.Count`) y `TotalCopies` (= `Σ max(1, Copies)`) | `RackInventarioCommands.BomTotal.cs:47-232`; `Bom/ConsolidatedBom.cs:40-41`; `RackConsolidatedBomWindow.xaml.cs:24-25` | `[MEASURED]` |
| 8 | El BOM del Selectivo cuenta instancias de los *builders* de vista con las decoraciones apagadas | `Systems/Selective/SelectiveBomBuilder.cs:54-84` | `[MEASURED]` |
| 9 | La numeración de niveles dibujada usa solo la primera bahía | `Systems/Selective/SelectiveFrontalBuilder.cs:333-339` | `[MEASURED]` |
| 10 | El resolver del Selectivo crea **una bahía resuelta por cada bahía de diseño**, por fondo, sin filtrar. Una bahía sin niveles sigue siendo un frente vacío. Cada fondo conserva su propio número de frentes | `Systems/Selective/SelectiveGeometryResolver.cs:121-136, 212-228, 244-260` | `[MEASURED]` |
| 11 | Dinámico: un frente en blanco (`IsActive = false`) conserva su estructura y aporta **cero** niveles de carga. La autoridad es `DynamicFrontActivation.EffectiveLoadLevels`, sobre el diseño o sobre el frente resuelto | `Domain/Systems/Dynamic/DynamicRackFront.cs:38-45, 100-111`; `Application/Systems/Dynamic/DynamicFrontActivation.cs:24-40` | `[MEASURED]` |

## 4. DC-02 — Dueños de reglas y valores

| Concepto | Dueño actual | Gobierno | Clase |
|---|---|---|---|
| Identidad de símbolo | `SymbolId(Namespace, Key)`. El constructor rechaza namespaces no registrados y valida la clave como `projectVariable` (GUID) | `Expressions/SymbolId.cs:85-108, 151-154`; ADR-0043 | `[MEASURED]` |
| Namespaces | `SymbolNamespace`, hoy solo `ProjectVariable`, con tabla de *tokens* y comparador por namespace (`OrdinalIgnoreCase`) | `SymbolId.cs:16-70` | `[MEASURED]` |
| Ámbitos | `SymbolScope.Project` y `SymbolScope.Rack` (reservado) | `SymbolTable.cs:13-17` | `[MEASURED]` |
| Contexto de operación | `ExpressionContext` inmutable, por operación (construcción interna) | `ExpressionContext.cs:60-77` | `[MEASURED]` |
| Del nombre a la identidad | `ExpressionBinder.Bind`; el cualificador resuelve como `ProjectVariable` | `ExpressionBinder.cs:48-90, 334` | `[MEASURED]` |
| Grafo y evaluación | `DependencyGraph` y `RegistryEvaluation` sobre las entradas de la tabla | `DependencyGraph.cs:16-20`; `RegistryEvaluation.cs:8-80` | `[MEASURED]` |
| *Effective* del Selectivo | `SelectiveEffectiveDesignResolver` | `Systems/Selective/SelectiveEffectiveDesignResolver.cs:112-172`; ADR-0034 + I-48 | `[MEASURED]` |
| Niveles efectivos del Dinámico | `DynamicFrontActivation.EffectiveLoadLevels` | `DynamicFrontActivation.cs:24-40` | `[MEASURED]` |
| *Authored* entre hermanas | `SelectiveAuthoredAuthority` (Selectivo) y los puertos AUTH-13 (Dinámico, Push Back, Cantilever y Cabecera); la Cama sin puerto | `SelectiveAuthoredAuthority.cs:84`; `Systems/Shared/RackAuthoredComparator.cs:65-86`; `RackAuthoredComparator.FourKinds.cs:14-27` | `[MEASURED]` |
| Autoridad para cotizar | `BomAuthoredAuthority` (compara solo el Selectivo; en los demás kinds aprueba la primera vista) | `Bom/BomAuthoredAuthority.cs:82-125` | `[MEASURED]` |
| Identidad de rack y de vista | `RackEmbedDocument.Id`, `View` y `Section` | `Persistence/RackEmbedDocument.cs:40-50`; ADR-0009/0010 | `[MEASURED]` |
| Agrupación pura del dibujo | `RackListBuilder.Build` (`Id`, `OrdinalIgnoreCase`; exige `Id` y `Kind`) | `RackListBuilder.cs:54-57` | `[MEASURED]` |
| Agrupación pura de una selección | `RackPhysicalSelection.Classify` (claves físicas `Ordinal`; grupos por RackId `OrdinalIgnoreCase`, solo donde se observó un RackId) | `Systems/Shared/RackPhysicalSelection.cs:195-273` (227, 231) | `[MEASURED]` |
| Clasificación sin inventar RackId | `ProjectVariableScanProjection.Project` | `ProjectVariables/ProjectVariableScanProjection.cs:30-65` | `[MEASURED]` |
| Captura del dibujo | `RackBlockFinder.ScanEnvelopes` y `RackSiblingScan` (§17) | `RackCad.Plugin/RackBlockFinder.cs:57-92`; `RackCad.Plugin/Views/RackSiblingScan.cs:95-149` | `[MEASURED]` |
| Vocabularios de kind | *Token* del sobre; `RackSystemKind` (`Selective` = cabecera); tres familias de etiquetas | `RackEmbedDocument.cs:16-26`; `Domain/Systems/Shared/RackSystemKind.cs:4-30`; `SystemRegistry.Default.cs:25-37` | `[MEASURED]` |
| Conteos de producto por rack | Sin dueño único hoy para frentes, posiciones, altura y fondo de rack en las rutas inspeccionadas. Para niveles del Dinámico existe la autoridad por frente | búsquedas de §22 | `[MEASURED]` (en las rutas inspeccionadas) |

## 5. DC-03 — Persistencia, *legacy* y compatibilidad

### 5.1 Hechos

- **Sobre del rack:** `SchemaVersion`, `Kind`, `View`, `Section`, `Id`, `Name`, `Design`, `CustomProperties` y `[JsonExtensionData]`
  (`RackEmbedDocument.cs:35-75`) `[MEASURED]`.
- **Identidad *legacy*:** sin `Id` no hay rack en RACKLISTA ni en el BOM total; nunca se inventa (`ProjectVariableScanProjection.cs:37-42`)
  `[MEASURED]`.
  - `RackSiblingScan` tantea el `Id` aunque el sobre no se pueda deserializar (`RackEnvelopeIdProbe`; `RackSiblingScan.cs:131`) `[MEASURED]`.
- **Expresiones persistidas:** `{"Node":"Reference","Namespace":<token>,"Id":<clave>}`; leer un *token* desconocido falla
  (`Persistence/PersistedBoundExpressionJson.cs:93-103, 170`). Las usan `ProjectVariablesStore:277`, `SelectivePalletDesignStore:127` y
  `MutationPlan:265` `[MEASURED]`.
- **Ningún campo persistido de métrica, resumen o conteo** en las rutas inspeccionadas `[MEASURED]`.

### 5.2 Compatibilidad (R63-DISC-04), en tres casos

| Caso | Efecto | Clase |
|---|---|---|
| Documento antiguo (sin el *token* nuevo) leído por una *build* nueva | Sin cambio de significado, si la *build* nueva conserva exactamente las reglas de `projectVariable` (clave, comparación, ida y vuelta, cualificador) | `[INFERENCE]` |
| Documento nuevo que **no** contiene el *token* nuevo, leído por una *build* antigua | Se lee como hoy: el *token* nuevo solo existe donde se escribe | `[RECONSTRUCTED]` |
| Documento nuevo que **contiene** `"Namespace":"rack"` (o similar), leído por una *build* antigua | **Registro de variables:** el documento entero queda `PresentButUnreadable` (prueba ejecutada). La lectura del registro falla cerrada en sus consumidores (RACKBOMTOTAL no genera el listado: `BomTotal.cs:60-67`). **Diseño Selectivo** con una fórmula de propiedad que contenga el *token*: el convertidor lanzaría `JsonException`, el *store* la convierte en `InvalidOperationException` (`SelectivePalletDesignStore.cs:49-51`) y el rack quedaría «diseño ilegible» (`ProjectVariableScanProjection.cs:51-62`); no se ejecutó una prueba de ese caso | `[MEASURED]` registro / `[RECONSTRUCTED]` diseño |

Que un documento antiguo se vuelva ilegible solo por añadir código **no** está demostrado ni se afirma. La persistencia y el ADR no se
deciden en esta ronda.

## 6. DC-04 — Del documento al *effective*, por sistema

Sin cambios respecto de R0: composición en los handlers del Plugin; resolutores puros en Application; solo el Selectivo usa variables.

| Kind | Deserialización | Resolución | Variables | Modelo resuelto |
|---|---|---|---|---|
| `selective` | `SelectivePalletDesignStore` | `SelectiveEffectiveDesignResolver` → `SelectiveGeometryResolver(design, catalog)` | sí | `SelectiveRackSystem` |
| `dynamic` | `RackProjectStore` | `DynamicRackSystemResolver(catalog)` o `DynamicSystem` *legacy* | no | `DynamicRackSystem` |
| `pushback` | `RackProjectStore` | `PushBackResolver(catalog)` | no | `PushBackSystem` |
| `cantilever` | `RackProjectStore` | `CantileverLineEditorAssembler(sections)`; catálogo de secciones cargado por el Plugin | no | línea ensamblada |
| `cabecera` | `RackProjectStore(...).Header` | ninguna | no | `RackFrameConfiguration` |
| `cama` | `FlowBedConfigurationStore` | `FlowBedLateralBuilder(config, catalog)` | no | instancias de lateral |

Toda la tabla es `[MEASURED]` (`RackCad.Plugin/KindHandlers/*KindHandler.cs`, métodos `BuildBom`/`Build`). Las fases del Selectivo
(`SelectiveEffectiveDesignResolver.cs:132-172`) son: acreditar; `RegistryEvaluation`; inspeccionar las propiedades; `ToDomain` +
`WriteEffective`; resolver la geometría en el consumidor `[MEASURED]`.

## 7. DC-05 — Consumidores, lectores y autoridades por conteo

### 7.1 EXP-04 — Inventario de los contratos de símbolo

Todo `[MEASURED]` en las rutas buscadas (§22). ID23 es un consumidor **previsto**, no código existente.

| Frontera | Productores | Lectores | Restricción de ida y vuelta | Prueba existente |
|---|---|---|---|---|
| Namespace | `SymbolNamespaces` (tabla de *tokens*) | `SymbolId` (`IsRegistered`), `PersistedBoundExpressionJson` (lectura y escritura del *token*), `BindingInspection` (rechaza los que no son `projectVariable`), `ExpressionFormatter`/`CanonicalShape.Classify` (referencia directa solo para `ProjectVariable`, `ExpressionFormatter.cs:64`) | Un *token* desconocido hace ilegible el documento (§5.2) | `ExpressionSymbolModelTests`; `G8PersistenceIntegrationContractTests` (R1) |
| Clave | `SymbolId.ProjectVariable` en `ExpressionBinder`, `ProjectVariablesExpressionAdapter`, `LinkedPropertyAuthoringContext`, `LinkedPropertyExpressionAuthoring`, `LinkedPropertyEditSession`, `BindingInspection`, `PlanReadSet`, `ProjectVariableMutationPreflight`, `ProjectVariablesWorkspace`, `RecoveryAssessment`; `PersistedBoundExpressionJson` (lectura) | `SymbolId.IsValidProjectVariableKey`; `ExpressionLexer` (cualificador `#d` / `#{clave}`, `:356, 405-430`); `ExpressionFormatter.FormatQualifier` (`:215-219`) | La clave se conserva tal cual y se compara con el comparador del namespace; el cualificador escribe `Q(key)` | `ExpressionRoundTripTests`, `ExpressionFormatterTests`, `G8PersistenceIntegrationContractTests` (`not-a-guid`) |
| Ámbito | `ProjectVariablesExpressionAdapter` y `LinkedPropertyAuthoringContext` (entradas `Project`); `LinkedPropertyExpressionAuthoring` (entradas `Project`, consumidor `Rack`); `ProjectVariableDefinitionAuthoring` (consumidor `Project`) | `ExpressionBinder` (`ScopeViolation`) | El ámbito no se persiste en el árbol enlazado (el JSON solo lleva `Namespace` e `Id`) `[MEASURED]` (`PersistedBoundExpressionJson.cs:93-103`) | `ExpressionBinderTests` |
| Forma persistida | `ProjectVariablesStore`, `SelectivePalletDesignStore`, `MutationPlan` | los mismos | Lectura estricta de miembros (`RequireOnly`) | `G8PersistenceIntegrationContractTests` |
| UI vinculable (I-48) | — | `RackCad.UI/Controls/LinkedPropertyEditor.cs` (hospeda `LinkedPropertyEditSession`); `RackSelectiveWindow.xaml.cs:3015-3016` | La UI no toca `SymbolId` ni el namespace directamente; consume la sesión pura | `LinkedPropertyEditorControlTests`, `MultiPropertySessionTests`, `PalletToleranceEditorTests` (UI); `LinkedPropertyEditSessionTests` (Core) |

**Hueco:** ninguna prueba ejecutada fija la ida y vuelta de un *token* nuevo en `SelectivePalletDesignStore` (el caso de diseño de §5.2).

### 7.2 EXP-02 — Productores de conteos y etiquetas (R63-DISC-02)

| Productor | Población | Filtros y admisión | Igualdad y agrupación | Magnitud | Consumidor / presentación |
|---|---|---|---|---|---|
| RACKLISTA (`RackInventarioCommands.RackLista` + `RackListBuilder.Build`) | Definiciones de `ScanEnvelopes(includeReferenceCount: true)`: no layout, no anónimas, **no xref** | Sobre nulo, sin `Id` o sin `Kind`: omitido **sin aviso** | `Id`, `OrdinalIgnoreCase`; nombre y kind = primer valor no vacío | Filas = racks lógicos **definidos** (colocados o no); `ViewCount`; copias = máximo de referencias directas por vista | `RackListWindow` (filas por nombre) |
| RACKBOMTOTAL (`RackInventarioCommands.BomTotal` + `ConsolidatedBomBuilder`) | Las mismas definiciones + el registro leído una vez | Inclasificable y colocado: **aborta**. Inclasificable sin colocar: salta. Solo colocados (copias > 0). Kind sin handler: aborta. Sin autoridad: salta con aviso. Bloqueado: aborta. Variable rota: aborta. Ilegible: salta con aviso | Hermanas por RackId `OrdinalIgnoreCase`; autoridad `BomAuthoredAuthority` | `Racks` (cotizados); `RackCount = Racks.Count`; `TotalCopies = Σ max(1, Copies)`; BOM total | `RackConsolidatedBomWindow` («N racks · M copias»), CSV y XLSX |
| `FindRackBlocks` (`RackCommandSupport.cs:117-140`) | `ScanEnvelopes(false)` | Sobre no nulo con `Id` == rackId; **tolera `Kind` ausente** | `OrdinalIgnoreCase` | Lista de definiciones de **un** rack | RACKEDITAR / Actualizar; ejecutor de variables |
| `RackSiblingScan` + `RackSiblingMembership` (I-55) | Definiciones no layout y no anónimas, **xref incluidas con marca** | Tantea el `Id` aunque el sobre no se pueda leer; separa referencias de layout y anidadas | Por RackId (en Application) | Pertenencia de las hermanas de un rack | Insertar; RACKPROYECTAR |
| `RackPhysicalSelection.Classify` | **Selección** del usuario, no el dibujo | Sin RackId: se ignora o se conserva para que falle el plan (OD-3) | Claves físicas `Ordinal`; grupos `OrdinalIgnoreCase` | Grupos de la selección | `RackProjectionPipeline` (RACKPROYECTAR) |
| `RackNewRackName` (I-60) | `ScanEnvelopes(false)` | Nombres no vacíos | — | Siguiente nombre de la familia (no es un conteo) | Creación de racks nuevos |
| Etiquetas de kind | `RackListBuilder.KindLabel` (sin Push Back); `IRackKindHandler.BomLabel`; `SystemDescriptor.LibraryLabel` | — | — | Texto de presentación | RACKLISTA, BOM y biblioteca |

Toda la tabla es `[MEASURED]` en el código citado.

**Conclusión de EXP-02** `[INFERENCE]`: RACKLISTA y RACKBOMTOTAL miden **magnitudes distintas sobre poblaciones distintas**, y que sus
cifras difieran no prueba una duplicación. La ambigüedad real es **qué población y qué admisión adopta `Project.TotalRacks`**: definida,
colocada o cotizable; qué ámbitos de colocación; qué diagnósticos. Esa decisión está pendiente (§20, P-01..P-03). No se autoriza
migrar RACKLISTA, RACKBOMTOTAL ni las etiquetas para forzar igualdad.

### 7.3 Grupos de `RackProjectionPipeline` (cerrado)

`RackProjectionSelectionFilter.Build` → `RackPhysicalSelection.Classify`, que forma los grupos con un diccionario
`OrdinalIgnoreCase` (`RackPhysicalSelection.cs:231-263`). `RackProjectionPipeline` solo **ordena** esos grupos con `Ordinal`
(`Views/Placement/RackProjectionPipeline.cs:78-79`) `[MEASURED]`. Lo prueba
`SharedPhysicalFactsTests.AUTH07_DEDUPES_IN_STABLE_ORDER_AND_GROUPS_ONLY_OBSERVED_RACK_IDS` (ejecutada en R1): `RackA` y
`RACKA` forman un solo grupo, y una referencia sin RackId no se agrupa.

## 8. DC-06 — Pruebas, guardas y EXP-05

### 8.1 Corridas de pruebas existentes

| Ronda | Base compilada | Orden y filtro | Resultado |
|---|---|---|---|
| R0 | `b557ea3b` | ocho clases `FullyQualifiedName~` (detalle en la evidencia §10.3) | 179/179, selección > 0 por clase |
| R1 | `dbfe1150` (ensamblado `1.0.0+dbfe1150…`) | `FullyQualifiedName~…G8PersistenceIntegrationContractTests.UNKNOWN_OR_MALFORMED_EXPRESSION_PAYLOAD_IS_STRUCTURALLY_UNREADABLE` y `FullyQualifiedName~…SharedPhysicalFactsTests.AUTH07_DEDUPES_IN_STABLE_ORDER_AND_GROUPS_ONLY_OBSERVED_RACK_IDS` | 9/9 (8 casos de la *theory* + 1 *fact*); TRX SHA-256 `A1F5A8FA…60E9` |

Son pruebas focales para preguntas del Discovery, no evidencia Full ni de gate. Una primera corrida R1 con `--no-build` usó binarios de
`b557ea3b`; se repitió compilando y solo cuenta la segunda (evidencia §11).

### 8.2 EXP-05 — Invariante → oráculo

No se escriben pruebas ni se fabrica RED. Ubicación futura: Core (`tests/RackCad.Tests`), salvo que se indique otra.

| Invariante candidato | Prueba existente o hueco | Escenario que demostraría la falla | Fallo esperado de la alternativa incorrecta |
|---|---|---|---|
| I-01: N vistas o colocaciones de un RackId aportan 1 a `TotalRacks` | Parcial: `RackCountInvariantCharacterizationTests` (vistas, mayúsculas), `SharedPhysicalFactsTests.AUTH07` (selección). Hueco para `TotalRacks` | Instantánea con 3 definiciones de un rack (frontal, lateral ×2) y referencias 2/1/0 | Un conteo por definición o por referencia da 3 o 3+; el correcto da 1 |
| I-02: igualdad de RackId = la vigente (`OrdinalIgnoreCase`), sin normalizar | Parcial (las mismas) | `Rack-A` y `rack-a` | Un conteo `Ordinal` da 2 |
| I-03: un sobre sin `Id` no recibe identidad y produce un diagnóstico, no un rack | `ProjectVariableScanProjectionTests` (proyección); hueco para el resumen | Definición colocada sin `Id` | Un resumen que la omita en silencio o le invente un id |
| I-04: kind ausente o desconocido es un caso distinto de la falta de identidad | Hueco | Sobre con `Id` y `Kind` vacío; sobre con `Kind = "futuro"` | Mezclarlos en un solo «excluido» hace indistinguible el diagnóstico |
| I-05: el fallo de una métrica no saca al rack del conteo de identidad | Hueco | Selectivo con variable rota | `TotalRacks` baja al fallar `Frentes` |
| I-06: población declarada (definida o colocada) aplicada de forma uniforme | Hueco | Rack definido sin colocar | Según la decisión P-01, cuenta o no, y el diagnóstico lo dice |
| I-07: ningún parámetro calculado legible desde una fórmula de propiedad (o fase demostrada segura) | Parcial: hoy `BindingInspection` rechaza namespaces ajenos | Fórmula de `VerticalClearance` que lee `Rack.Altura` | Sin la regla, se evalúa con un valor anterior o entra en ciclo |
| I-08: un agregado `Project.*` que dependa de modelos resueltos no es legible desde variables de proyecto | Hueco | Variable `=Project.TotalPosiciones` + rack enlazado | Recursión o resolución completa del dibujo |
| I-09: `Rack.Frentes` (Selectivo) numérico en un contexto de rack para ID23 | Hueco | Rack de 4 frentes; `Rack.Frentes * 2` | Debe dar 8 con la semántica decidida (P-05); otro valor si se toma la bahía equivocada o la suma de fondos sin decidir |
| I-10: hermanas divergentes o ilegibles → estado explícito, no primera vista | Selectivo: `SelectiveBomAuthorityTests`; los demás kinds: los puertos AUTH-13 de I-58. Hueco en el resumen | Dinámico con dos vistas divergentes | Sin la regla, el valor depende del orden del `BlockTable` |
| I-11: resultado parcial marcado como parcial | Hueco | Un rack ilegible entre 3 | `TotalRacks = 2` sin marca de parcial |
| I-12: el formato de `projectVariable` (clave, ida y vuelta, cualificador) no cambia al añadir un namespace | `ExpressionRoundTripTests`, `G8PersistenceIntegrationContractTests` | Ida y vuelta de un árbol existente tras ampliar la tabla de *tokens* | Cambio de bytes o de igualdad |

## 9. DC-07 — Archivos calientes e intersecciones

### 9.1 *Snapshot* R0 (conservado; 2026-10-01T16:40:05Z)

I-52 `c1982b2a`, I-62 `85ae4324`, I-63 `b557ea3b`, I-64 `d8a02163`, `main` `819955d6`. Cruce textual medido: I-63 × I-64 = conflicto en
`docs/ROADMAP.md`; I-63 × I-62 = sin conflicto (`git merge-tree`, evidencia §9.4).

### 9.2 Observación R1 (2026-10-01T17:16:34Z)

I-52 `c1982b2a` (sin cambio), I-62 `f25dd1d5`, I-63 `dbfe1150`, I-64 `cf034e59`, `main` `819955d6`. Las ramas de I-62 e I-64 solo
cambian `docs/` respecto de `main` (7 y 6 archivos) `[MEASURED]`. No se repitió `merge-tree`.

| Iniciativa | Textual | Funcional | Autoridad compartida (hechos de coordinación) |
|---|---|---|---|
| I-64 | ROADMAP (filas adyacentes tras I-60) | **No inspeccionado en código**: no tiene producto. Su Discovery (§9.4) propone, como hipótesis, un índice runtime por documento (`RackId`, nombre, `SystemKind`, vistas, definiciones, conteo de referencias) alimentado por la misma travesía y sin retener `Design`. Su §10.2 dice que «ambas consumen la misma fundación integrada (Rack Identity, `RackListBuilder`)» y que una autoridad nueva de enumeración sería STOP y Master `[MEASURED]` (lectura de `cf034e59`) | Candidato a contrato compartido de hechos y agrupación; elevado al Master (§21) |
| I-62 | ROADMAP (otra tabla) | Ninguno de producto; Discovery delta de proceso | Protocolo de ejecución delegada |
| I-52 | ROADMAP, HANDOFF, índice de ADR | No inspeccionado; su rama no tiene producto | Identidad de racks espejados (AUTH-15) `[UNKNOWN]` |

Canal con I-64: respondí a su consulta de frontera, sin compromisos (evidencia §11). I-64 adoptó la corrección de R63-DISC-02 y declaró
que no diseña el mínimo común.

## 10. DC-08 — Fundaciones (y EXP-01 repetida)

- **Freeze de I-49** (sin cambio respecto de R0):
  - vigente: `I-49-consensus-freeze-v6-a1-a2-a3-r1.md`, blob `440ae61a…`; es el freeze **correctivo** R1 sobre V6 + A1 + A2 + A3-R2
    + ADR-0043;
  - los blobs declarados coinciden con `origin/main` (V6 `ef4db3aa…`, A1 `d6201908…`, A2 `49a92533…`, A3-R2 `4da6ef3c…`, ADR-0043
    `dd88bf06…`) `[MEASURED]`.
- **Fundaciones candidatas** (detalle R0 conservado en Git):
  - Rack Identity, View Identity, Authored vs Effective, Project Variables, Expression Engine, Auto Rack Naming y Shared View
    Foundation coinciden con su fuente, código y pruebas en lo leído `[MEASURED]`;
  - ni Expression Engine ni Auto Rack Naming tienen entrada en FOUNDATIONS, lo que no prueba que no existan;
  - captura añadida en R1: `RackSiblingScan`/`RackSiblingMembership` (I-55) es una **segunda captura gobernada**, alcance de I-55, y
    no contradice Rack Identity.
- **ADR-0039 D-16:**
  - las Custom Properties V1 son cadenas, y el motor solo usa `double` finitos `[MEASURED]`;
  - CustomProperty, ProjectVariable y ComputedParameter son autoridades disjuntas que la Proposal debe delimitar `[INFERENCE]`.
- **EXP-01 repetida con las fuentes ampliadas:** **negativa**.
  - Las capturas y agrupaciones nuevas son coherentes con ADR-0009 (identidad por GUID; igualdad `OrdinalIgnoreCase` en todos los
    agrupadores leídos).
  - `forceValidity: true` con `directOnly: true` no contradice ningún ADR; según la documentación externa, la marca no tiene efecto
    (§17) `[EXTERNAL]`.
  - Los Discoveries de I-64 e I-62 no son fundaciones y no se usan como fuente consumida.
  - No hay discrepancia consumida de clase A.

## 11. DC-09 y EXP-09 — Materialidad (R63-DISC-06)

Arquetipo de trabajo: **NEW ARCHITECTURE** (se mantiene). Que el diseño elija un catálogo estático no lo bajaría: LIFECYCLE §3 incluye
la creación de una autoridad o contrato transversal y M-01 como creador.

| ID | Estado para planificar | Evidencia | Pregunta acotada y quién decide |
|---|---|---|---|
| M-01 | **Activado (creador)** | No hay dueño único de métricas de rack o proyecto (§4); ID20 introduce una autoridad de métricas y agregación. Riesgo de modificador si `TotalRacks` coincide en población con `Racks` del BOM y no se reconcilia | Coordinator + Architect: relación con `ConsolidatedBom` (P-08) |
| M-02 | **Activado (rama pendiente)** | Persistir referencias `rack`/`project` cambia el formato y deja ilegibles los registros en *builds* antiguas (§5.2, prueba ejecutada) | Proposal + Architect + ADR: ¿se persisten referencias a símbolos calculados (necesario para ID23 persistido)? |
| M-03 | No activado (a verificar) | Los nombres con llaves siguen siendo variables; el árbol persiste `SymbolId`, no texto `[INFERENCE]` | Architect, en la revisión |
| M-04 | **Activado (UNKNOWN)** | Los consumidores actuales difieren: omiten, saltan con aviso o abortan (§7.2). Un resumen añade estados Unavailable/NotApplicable/Divergent | Owner + Proposal: política de parciales y diagnósticos (P-03, P-07) |
| M-05 | Activado | `SymbolNamespace`, la validez de `SymbolId` y el cualificador son contratos consumidos (§7.1) | Proposal + Architect |
| M-06 | Activado | Punto de extensión de símbolos built-in de I-49 y de *providers* por kind | Proposal + Architect |
| M-07 | **Activado para planificar (UNKNOWN)** | No hay ninguna abstracción de *provider* en las rutas inspeccionadas; la composición por kind está en el Plugin; un contrato compartido de captura y agrupación con I-64 está en consulta (§21). La pregunta no se cierra eligiendo una arquitectura | Master (contrato compartido) y después Proposal + Architect (catálogo o registro) |
| M-08 | Activado | ADR-0043 fija «exactamente un namespace activo»; activar `rack`/`project` modifica esa afirmación | ADR sucesor; Owner acepta |

**EXP-09: ejecutada; la pregunta M-07 sigue abierta.** Quién la resuelve: Master (frontera compartida), luego Proposal y Architect.

## 12. EXP-01..09: resultados

| EXP | Estado | Resultado |
|---|---|---|
| EXP-01 | Repetida; **negativa** | §10 |
| EXP-02 | **Ejecutada** | §7.2; decisiones P-01..P-03 y P-09 |
| EXP-03 | **No activada** | El camino *legacy* de las referencias está localizado (`ScanEnvelopes`, `RackSiblingScan`, `RackEnvelopeIdProbe`); su efecto incierto se trató en EXP-06 |
| EXP-04 | **Ejecutada** | §7.1, con un hueco de prueba (diseño Selectivo con *token* nuevo) |
| EXP-05 | **Ejecutada** (matriz; sin pruebas nuevas) | §8.2 |
| EXP-06 | **Ejecutada** | §17; lo que solo se verifica en AutoCAD queda UNKNOWN con escenario manual |
| EXP-07 | **En coordinación** | Consulta al Master preparada, no enviada (§21) |
| EXP-08 | **Ejecutada** | §18 |
| EXP-09 | **Ejecutada; M-07 abierto** | §11 |

## 13. Trazado de Q1..Q14

| Q | Respuesta R1 | Sección |
|---|---|---|
| 1 | `SymbolId(namespace, key)`, clave GUID para `projectVariable`, un namespace, ámbitos `Project`/`Rack`, nombre de visualización no identitario `[MEASURED]` | §4, §7.1 |
| 2 | `ExpressionContext` por operación, constructor interno `[MEASURED]` | §4 |
| 3 | El grafo cubre solo los símbolos de la tabla; las fórmulas de propiedad no son nodos `[MEASURED]` | §4, §16.2 |
| 4 | Fases implícitas del Selectivo `[MEASURED]`; fase mínima por métrica en §15 `[INFERENCE]` | §6, §15 |
| 5 | Resolutores puros en Application, composición en el Plugin `[MEASURED]` | §6 |
| 6 | Productores, poblaciones y magnitudes de §7.2 `[MEASURED]` | §7.2 |
| 7 | `RackListBuilder.Build` es una agrupación pura **reutilizable**; su admisión (exige `Kind`; omite sin aviso) y su orientación a presentación (nombre, etiqueta) hay que decidirlas antes de adoptarlo como base de `TotalRacks` `[MEASURED]`/`[INFERENCE]` | §7.2 |
| 8 | `ConsolidatedBomBuilder` sirve para el BOM. No es una fuente de métricas de producto: cuenta instancias de dibujo, la población «cotizable» y la autoridad de primera vista en kinds no selectivos `[MEASURED]`/`[INFERENCE]` | §7.2, §18 |
| 9 | Dedupe `OrdinalIgnoreCase` en todos los agrupadores leídos; sin RackId no hay grupo `[MEASURED]` | §7.2, §7.3 |
| 10 | Métricas dispersas: §14 `[MEASURED]` | §14 |
| 11 | Comunes de verdad: kind y racks lógicos por población. Las demás, por kind `[INFERENCE]` | §14 |
| 12 | Ciclos: riesgos de extensión futura por dependencias reales `[RECONSTRUCTED]` | §16.2 |
| 13 | ID30: candidato a contrato compartido, en el Master `[MEASURED]`/`[INFERENCE]` | §9.2, §21 |
| 14 | 100 % Application: *providers* y agregación puros, si reciben instantáneas de captura y catálogos cargados `[INFERENCE]` | §6, §17 |

## 14. Matriz sistema × métrica (R63-DISC-05)

Leyenda de estado: `Supported` = fuente actual única y semántica clara; `NotApplicable`; `Unavailable`; `NeedsContract` = hay datos,
pero falta decidir la semántica o la autoridad. `NeedsContract` es un resultado válido de Discovery, no una exclusión. Cada celda indica
**fuente actual · pregunta pendiente · dependencia/fase · datos no fiables**.

| Métrica | Selectivo | Dinámico | Push Back | Cantilever | Cabecera | Cama |
|---|---|---|---|---|---|---|
| **SystemKind** (texto; **no** sirve para la vía numérica) | Supported · *token* `selective` · sin pregunta · instantánea · sobre sin `Kind` = caso propio | Supported (`dynamic`) | Supported (`pushback`) | Supported (`cantilever`) | Supported (`cabecera`) | Supported (`cama`) |
| **Frentes** (numérico) | NeedsContract · `Bays.Count` y `BaysForFondo(k).Count` (1:1 con el resuelto, `SelectiveGeometryResolver.cs:121-136, 244-260`) · ¿fondo 0, el más largo o por fondo?; ¿cuentan los vacíos? · estructura del diseño, sin catálogo ni propiedades vinculables · diseño ilegible o hermanas divergentes = estado | NeedsContract · `Fronts` (`IsActive`) · ¿cuentan los frentes en blanco? · estructura del diseño · ídem | NeedsContract · ranuras compartidas, lados A/B · ¿por lado o compartidas? · resolución del compuesto · ídem | NeedsContract · `StationCount`/`IntervalCount` (`CantileverLineDesign.cs:339, 367`) · ¿estaciones, intervalos o caras? · diseño · ídem | NotApplicable | NotApplicable |
| **Niveles** | NeedsContract · `Levels` por bahía (`SelectiveRackSystem.cs:125`) · ¿máximo, por bahía o total? · diseño · ídem | NeedsContract · `DynamicFrontActivation.EffectiveLoadLevels` por frente (**fuente única** por frente) · agregación a nivel rack · diseño o resuelto · ídem | NeedsContract · por frente y lado · ídem · resolución · ídem | NeedsContract · niveles de **brazo** · no equivalen a niveles de tarima | NotApplicable | NotApplicable |
| **Posiciones de tarima** | NeedsContract · `PalletCount` por celda + piso; medio frente por ajuste geométrico · ¿se incluye el medio frente? · el medio frente exige geometría (longitud de tramo) · ídem | NeedsContract · carriles × niveles efectivos × `PalletsDeep` · confirmar la fórmula · resuelto · ídem | NeedsContract · fondo efectivo por nivel y lado · ídem · resolución · ídem | NotApplicable (cargas largas) `[INFERENCE]` | NotApplicable | NotApplicable |
| **Altura** | NeedsContract · `SelectiveRackSystem.Height` (posterior al *resolve*, catálogo) · ¿altura máxima del rack? · **posterior al resolve**; insegura durante la evaluación de propiedades (§16.2) · ídem | NeedsContract · `Height` por frente · agregación · resuelto | NeedsContract · estructura compartida · resuelto | NeedsContract · columna por estación · resuelto | Supported (candidato) · `RackFrameConfiguration.Height` · sin pregunta de agregación · configuración | NotApplicable |
| **Fondo** | NeedsContract · `DepthCount`, `FondoDepths` · ¿conteo o medida? · diseño o resuelto | NeedsContract · `PalletsDeep` por frente | NeedsContract · fondo efectivo por nivel y lado | `[UNKNOWN]` | Supported (candidato) · `Depth` | NeedsContract · `LaneDepth` |

**Métrica numérica para la vía ID23: Frentes del Selectivo.**
- **Preferida:** frentes del **fondo 0** (cara frontal), contando los vacíos; numérica, de diseño estructural, sin catálogo y sin
  dependencia de propiedades vinculables `[INFERENCE]` sobre §3, fila 10.
- **Segunda:** frentes del **fondo más largo** (la rejilla maestra del resolver, `SelectiveGeometryResolver.cs:138-145`).
- Ninguna está congelada; decide el Owner (P-05).
- El ejemplo del mandato `Rack.Frentes * 2` quedaría bien definido en un contexto de rack con cualquiera de las dos, una vez decidida
  la semántica.

**Subconjunto inicial de estudio** (no aprobado como suficiente): `SystemKind`, `TotalRacks`, `RacksBySystem` + `Frentes` del Selectivo
como métrica numérica de prueba de la vía ID23. No se asignan códigos numéricos a los kinds. No se elige la bahía 0, la altura máxima
ni la suma de niveles como decisión de producto.

## 15. Dependencias y fase mínima por métrica candidata (R63-DISC-03)

| Métrica | Entradas | Dependencias | Fase mínima (hipótesis, no habilitada) | Consumidor | Consistencia de la instantánea | Catálogo/geometría | Efectos laterales | Invalidación |
|---|---|---|---|---|---|---|---|---|
| `TotalRacks` | Instantánea de sobres (Id, Kind, View, Section) + hechos de colocación según la población | Política de población y admisión (P-01..P-03) | **Metadatos**: sin deserializar `Design` ni resolver | Resumen, I-64 (presentación) | Una sola captura por lectura | No | Ninguno: solo lectura | Alta, baja o cambio de identidad o de colocación |
| `RacksBySystem` | La de `TotalRacks` + *token* `Kind` | Ídem + clave de kind (P-09) | Metadatos | Resumen | Ídem | No | Ninguno | Ídem + cambio de kind |
| `SystemKind` (rack) | `Kind` del sobre representativo | Hermanas coherentes en `Kind` (hoy RACKLISTA no lo verifica) | Metadatos | Resumen | Por rack | No | Ninguno | Cambio de kind |
| `Frentes` (Selectivo) | `Design` deserializado (bahías por fondo) | Autoridad de hermanas (`SelectiveAuthoredAuthority`); semántica P-05 | **Diseño estructural**: deserializar sin resolver la geometría; no depende de las propiedades vinculables | Resumen; **ID23** en un contexto de rack | Por rack | No | Ninguno | Edición del diseño |
| `Niveles` (Dinámico) | `Design` (frentes) | `DynamicFrontActivation.EffectiveLoadLevels`; agregación P-05 | Diseño estructural (la autoridad admite el diseño sin resolver) | Resumen | Por rack | No | Ninguno | Edición |
| `Altura` (Selectivo) | *Effective* + catálogo | Resolver; propiedades vinculables (holgura) | **Posterior al resolve** | Resumen, ID23 | Por rack, con registro y catálogo de la misma lectura | Sí | Ninguno (resolver puro) | Diseño, variables o catálogo |
| Posiciones (Selectivo) | *Effective* (celdas) + geometría para el medio frente | Resolver; P-05 | Posterior al resolve (medio frente) | Resumen | Ídem | Sí (medio frente) | Ninguno | Ídem |
| Agregados de capacidad del proyecto | Todas las métricas por rack | Todas las anteriores | Posterior al resolve de cada rack incluido | Resumen | Una lectura de registro y catálogo por resumen, como RACKBOMTOTAL | Según la métrica | Ninguno | Cualquier cambio de los racks incluidos |

**Separación de costes** `[INFERENCE]`, sobre `RackBlockFinder.cs:51-55`, el §9.8 de I-64 y `SelectiveBomBuilder.cs:54-59`:
- recorrer metadatos (`BlockTable` + Xrecord + deserializar el sobre);
- deserializar `Design`;
- resolver el modelo (catálogo y geometría).

Contar RackIds solo necesita lo primero.

## 16. Identidad, fases y símbolos

### 16.1 Exclusiones distintas (R63-DISC-01)

| Caso | Hecho actual | Tratamiento pendiente |
|---|---|---|
| Sin identidad (`Id` vacío o sobre ilegible) | Excluido por RACKLISTA (sin aviso) y por el BOM (aborta si está colocado) `[MEASURED]` | Diagnóstico «inclasificable»; nunca se inventa identidad |
| Con `Id` y `Kind` ausente o desconocido | RACKLISTA lo excluye; `FindRackBlocks` lo tolera; el BOM aborta si el kind no tiene handler `[MEASURED]` | ¿Cuenta como rack lógico con kind desconocido? (P-02) |
| Fallo de una métrica (diseño ilegible, hermanas divergentes, variable rota) | El BOM salta o aborta según el caso `[MEASURED]` | Estado de esa métrica; **no** saca al rack de `TotalRacks` (I-05), si así se decide (P-07) |

### 16.2 Fases y ciclos por dependencias reales

- **Riesgo futuro** (no se puede ejecutar hoy porque no hay *built-ins*):
  - la holgura vertical es vinculable (§3, fila 4), y la altura resuelta depende de ella (`SelectivePalletDesign.cs:9-16`);
  - una fórmula de holgura que leyera una altura calculada se evaluaría antes de que exista;
  - `DependencyGraph` no ve ese ciclo, porque las fórmulas de propiedad no son nodos `[RECONSTRUCTED]`.
- **No todo `Project.*` depende del *effective*:** `TotalRacks` y `RacksBySystem` solo dependen de metadatos (§15); las agregaciones
  de capacidad sí dependen de cada rack resuelto `[INFERENCE]`.
- Un prefijo de namespace **no** decide la fase. Fase y ámbito se comprueban por métrica.
- La disponibilidad de una métrica de diseño estructural (por ejemplo `Frentes`) dentro de las fórmulas de propiedad sigue siendo
  **hipótesis**. No depende de las propiedades vinculables de hoy, pero una propiedad vinculable futura que cambiara la estructura
  rompería esa independencia: no se habilita.

### 16.3 Identidad de símbolos calculados (alternativas; no se decide)

| Alternativa | Contrato de namespace | Compatibilidad con las reglas de `projectVariable` | Riesgos |
|---|---|---|---|
| A. Clave GUID **preasignada y estable** por parámetro calculado (catálogo fijo), en un namespace nuevo | Validez de clave por namespace (hoy común) | Se conservan la forma `#d`/`#{}` y `Q(key)`; el lexer y el formatter ya manejan GUIDs | Hay que fijar los GUID como contrato; el nombre visible es aparte |
| B. Clave **textual estable** (por ejemplo `frentes`) en un namespace nuevo | Cambiar `IsValidProjectVariableKey` por una validez por namespace; revisar el lexer, el formatter y el cualificador | Riesgo de alterar el cualificador de `projectVariable` si no se separa por namespace | Más cambios en contratos consumidos (M-05) |

En ambas: nunca se deriva la identidad del nombre visible, no cambia en cada evaluación y se conservan **exactamente** las reglas de
`projectVariable`. La elección es de Proposal + Architect + ADR (M-08).

## 17. EXP-06 — Qué observa y qué omite cada consumidor

**Fuente externa** `[EXTERNAL]`: referencia oficial de Autodesk de `BlockTableRecord.GetBlockReferenceIds` (Managed Reference Guide,
página de 2022, consultada el 2026-10-01; no se verificó la de 2025):
- devuelve solo las referencias **activas**;
- `directOnly: true` excluye las referencias del bloque padre cuando el bloque está anidado;
- `forceValidity` «solo aplica si `directOnly` es false».

| Caso | Salida actual | Diagnóstico | Límite de la prueba |
|---|---|---|---|
| Rack colocado en Model Space | Cuenta en las dos capturas | — | Plugin, solo en AutoCAD |
| Rack colocado en un layout de Paper Space | Cuenta como referencia directa en `ScanEnvelopes`; `RackSiblingScan` la marca como layout | Ninguno | `[RECONSTRUCTED]`; escenario manual: rack en un layout, RACKLISTA y RACKBOMTOTAL |
| Rack anidado dentro de otro bloque colocado N veces | `directOnly: true` cuenta **una** referencia (la de dentro de la definición contenedora), no N; `RackSiblingScan` la marca como anidada | Ninguno | `[EXTERNAL]` + `[RECONSTRUCTED]`; escenario manual: contenedor ×3 |
| Referencia borrada | Excluida (solo referencias activas) | — | `[EXTERNAL]`; escenario manual: borrar, contar, deshacer y contar |
| Definición sin referencias | RACKLISTA la lista con 0 copias; el BOM la salta si es inclasificable y la excluye si no está colocada | Ninguno | `[MEASURED]` |
| Definición de una xref | `ScanEnvelopes` la omite; `RackSiblingScan` la incluye marcada | Ninguno | `[MEASURED]` (código) |
| Sobre sin `Id` / sin `Kind` | Ver §16.1 | RACKLISTA: ninguno; BOM: mensaje | `[MEASURED]` |
| Kinds divergentes entre hermanas | RACKLISTA toma el primero; el BOM usa el kind de la primera hermana para el handler | Ninguno | `[MEASURED]`; prueba futura posible en Core sobre `RackListBuilder` |
| `forceValidity: true` frente a `false` con `directOnly: true` | Sin diferencia según la documentación externa | — | `[EXTERNAL]`; el comentario del código que atribuye coste a `true` no se verificó en el host `[UNKNOWN]` |

## 18. EXP-08 — Deuda que un resumen podría exponer

| Hallazgo | Disposición |
|---|---|
| Falta la etiqueta de Push Back en `RackListBuilder.KindLabel` | **No consumir** la etiqueta de RACKLISTA como identidad ni como texto del resumen; la clave es el *token* (P-09). No se arregla |
| `BomAuthoredAuthority` aprueba la primera vista en kinds no selectivos | **No consumir** como autoridad de métricas; usar la autoridad de hermanas por kind (decisión de Proposal; P-06) |
| RACKLISTA omite sin aviso los sobres sin `Id`/`Kind` | **Preservar** RACKLISTA tal cual; el resumen necesita diagnósticos propios (P-03) |
| Las poblaciones de RACKLISTA (definidos) y RACKBOMTOTAL (cotizados) difieren | **Preservar**; declarar la población de `TotalRacks` (P-01) y su relación con `Racks.Count` (P-08) |
| Copias con `directOnly` (anidados ×1) | **Dependencia**: no afecta a `TotalRacks` (identidad), sí a cualquier magnitud física futura; fuera de esta orden |
| El BOM del Selectivo cuenta instancias de dibujo | **No consumir** como fuente de métricas de producto |

## 19. Rendimiento: plan (sin tiempos inventados)

- **Disponible** `[MEASURED]`:
  - una sola travesía de definiciones por orden en RACKLISTA y RACKBOMTOTAL;
  - RACKBOMTOTAL resuelve cada rack aprobado y lee el registro una vez;
  - las decoraciones se apagan para contar;
  - `G11CandidateValidationTests` cronometra la cadena de I-49.
- **No hay** mediciones de ID20 ni de comandos en AutoCAD `[UNKNOWN]`.
- **Plan futuro** (necesita pruebas nuevas en un gate autorizado):
  1. métrica de un rack por clase de fuente (metadatos, diseño estructural, posterior al *resolve*);
  2. agregación sobre instantáneas sintéticas de N = 1, 10, 100 y 1000 racks;
  3. N racks con varias vistas o secciones: comprobar un solo *resolve* por RackId y por lectura;
  4. lecturas repetidas: contar resoluciones con y sin «resolver una vez por orden».

  Se separa el coste de los metadatos del coste de la resolución (§15).

## 20. Decisiones pendientes

### 20.1 De producto (Owner, con alternativas)

| ID | Pregunta | Alternativas | Consecuencias |
|---|---|---|---|
| P-01 | Población de `TotalRacks` | (a) **definidos** en el dibujo (como RACKLISTA); (b) **colocados** (al menos una referencia activa en un ámbito admitido); (c) **cotizables** (como el BOM) | (a) cuenta racks dibujados y borrados sin purgar; (b) coincide con lo visible; (c) mezcla la validez del BOM con el conteo |
| P-02 | Kind ausente o desconocido con `Id` válido | (a) cuenta como rack con kind «desconocido» y diagnóstico; (b) no cuenta, con diagnóstico | (a) `TotalRacks` refleja la identidad; (b) `RacksBySystem` suma `TotalRacks` |
| P-03 | Diagnósticos y parciales | (a) siempre total + lista de diagnósticos; (b) total marcado «parcial» si hay exclusiones | Visibilidad frente a simplicidad |
| P-04 | Ámbitos de colocación admitidos | Model Space; Paper Space; anidados; xref | Afecta a P-01 (b) |
| P-05 | Semántica por kind de las métricas numéricas, empezando por los frentes del Selectivo | Fondo 0 (preferida) o fondo más largo; frentes vacíos sí o no | Define la vía ID23 |

### 20.2 De arquitectura (Proposal + Architect; ADR cuando corresponda)

- P-06: autoridad de hermanas por kind para las métricas;
- P-07: estados por métrica;
- P-08: relación con `ConsolidatedBom.Racks.Count`;
- P-09: clave de `RacksBySystem` (*token*);
- P-10: identidad de los símbolos (§16.3);
- P-11: persistencia de referencias (M-02);
- P-12: fases por consumidor;
- P-13: modelo neutral del resumen y unidades.

### 20.3 Reservadas al Master

P-14: contrato mínimo compartido con I-64, su responsable, consumidores y secuencia (§21).

## 21. Estado de la coordinación con el Master (EXP-07)

- Consulta: `I63_I64_Consulta_Contrato_Compartido.txt` del Coordinator de I-63 (4 227 bytes, SHA-256 `e4ca7e8b…e935e930`).
- **No enviada por esta sesión:** no tiene canal con el Master (sesiones visibles: I-64, I-62, I-52, ARC-02). Se entrega al Owner
  para que la haga llegar.
- **Estado: SIN RESPUESTA.** No se ha declarado ningún envío ni respuesta.
- Mientras se coordina, se detienen la elección del contrato compartido, la Proposal y el Freeze. No se adopta la propuesta de I-64 ni
  su código, y su caché o su inventario runtime no se tratan como autoridad de métricas.

## 22. Búsquedas y huecos

**Búsquedas R0** (en `src/` y `tests/`): `ScanEnvelopes(`, `RackListBuilder\.`, `ConsolidatedBom`, `BomAuthoredAuthority\.`,
`GroupBy(… Id|RackId)`, `ExpressionContext.Create`, `SymbolTable.Create`, `RegistryEvaluation\.`, `SymbolScope\.`,
`ReservedName|UnknownNamespace|ScopeViolation`, `NumberFronts|NumberLevels`, `posiciones|PalletPositions|Capacity…`,
`RackCount|TotalCopies`, `RackAuthoredComparatorPorts`, `SystemRegistry\.`, `PersistedBoundExpressionJson`,
`Stopwatch|Elapsed|Benchmark`.

**Búsquedas R1:**
- `SymbolNamespace`, `SymbolNamespaces\.`, `SymbolId\.ProjectVariable|new SymbolId(`, `SymbolScope\.`, `ExpressionFormatter\.`,
  `ExpressionBinder\.Bind`, `IsValidProjectVariableKey|IsDFormatGuid`, `PersistedBoundExpressionJson` (Application, UI, Plugin);
- `RackCad.Application.Expressions|LinkedPropertyEdit` (UI, Plugin);
- `GetBlockReferenceIds` (Plugin);
- `RackPhysicalSelection`, `RackSiblingScan`, `FindRackBlocks`, `DynamicFrontActivation`;
- `projectVariable` y `Namespace` en las pruebas.

**Huecos:**
- no se ejecutó nada del Plugin ni de AutoCAD;
- no se probó un *token* nuevo en el diseño Selectivo;
- no se inspeccionó código de I-64, I-62 ni I-52;
- la documentación de `GetBlockReferenceIds` es de 2022;
- no se inventariaron todos los consumidores de `DynamicFrontActivation` (32 archivos).

## 23. Riesgos del mandato → estado

| Riesgo | Estado R1 |
|---|---|
| Contar vistas en vez de RackId | Decidido por el mandato; los agrupadores leídos lo cumplen; falta la población (P-01) |
| *Legacy* sin identidad | Sin identidad inventada; falta el diagnóstico (P-03) |
| Hermanas divergentes o ilegibles | Autoridad por kind (P-06) |
| Aritmética duplicada | No demostrada: hay magnitudes distintas. El riesgo aparece si `TotalRacks` adopta una población sin reconciliar (P-08) |
| Fases y ciclos ocultos | Riesgo de extensión futura; tablas de dependencia (§15-16) |
| Nombres visibles como identidad | Excluido por el motor y los agrupadores |
| Equivalencias falsas | Métricas por kind; ningún código numérico para los kinds |
| Parciales presentados como totales | P-03 |
| Resoluciones repetidas | Los metadatos y la resolución se separan (§15, §19) |
| Solapamiento con ID30 | Consulta al Master (§21) |
