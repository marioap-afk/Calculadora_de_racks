# I-63 — Focused Discovery (F0-DISCOVERY R2) — ID20, Computed Parameters & Project Summary

> **Estado:** ronda R2 del Discovery. Aplica las dos precisiones localizadas que pidió la revisión de R1 (CD-I63-F0-R2-01:
> CHANGES REQUIRED limitado a R63-R1-01 y R63-R1-02; ejecución autorizada en CD-I63-F0-R2-02). Lo demás es R1 sin cambios de fondo.
> **No** es Proposal, Freeze, revisión del Architect ni GATE PASS.
> - Lo hizo **directamente** la sesión principal responsable, sin Controller, Worker, Architect, subagente ni otro proceso de IA y
>   sin delegación §16.
> - Rondas anteriores conservadas en Git:
>   - R0: commit `dbfe1150007b7ea6819df7277e35666fe67af056`, blob `d9426070818eaa14ceb976fca474248160fbf803`;
>   - R1: commit `852932735aca5b03c13a17064a67347ec8c78952`, blob `5224349b39bfd18e3e17f9275e539404531e4b6a`.
> - **Actualización posterior a R2.** No forma parte de la ronda revisada.
>   - R2 fue **aceptada** sobre `23eeefc5` (blob `df74e7fe…`; CD-I63-F0-R2-04), sin ningún REQUIRED documental pendiente.
>   - Después, el **Owner decidió P-14**: no se crea ni se exige un contrato común de inventario o enumeración con I-64. Cada
>     iniciativa implementa su propio mecanismo, y la duplicación queda aceptada y diferida a una futura iniciativa arquitectónica.
>   - Las consultas al Master quedan **superadas y sin enviar**. La decisión del Owner prevalece, para I-63, sobre `MASTER-I63-I64-01`,
>     que I-64 registró en su rama (§24).
>   - La salida hacia el diseño espera P-01..P-05 (Owner) y una orden del Coordinator para la Proposal y el Architect.
>   - La historia de rondas anteriores (§§9, 21 y 23) se conserva y se anota; no se borra.

## 0. Identidad, base y método

- Unidad I-63 (ID funcional ID20). Rama `architecture/parametros-calculados-resumen-proyecto`.
  [Contrato](I-63-parametros-calculados-resumen-proyecto.md); [decisiones](../automation/decisions/I-63.md);
  [evidencia](../automation/evidence/I-63-evidence.md).
- **Base de R1:** `dbfe1150`. **Base de R2:** `85293273`. Fuera de `docs/`, las dos son idénticas a `origin/main`
  `819955d61a6da4c811a11fbd11b5dca13f634b7c` (preflights 2026-10-01T17:16:34Z y 19:15:18Z, sin rebase), así que todo lo dicho sobre
  código vale para `main`.
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
  - R1: dos pruebas nuevas para preguntas nuevas, 9/9, compiladas sobre `dbfe1150` (§8);
  - R2: ninguna. Las correcciones son de redacción y razonamiento, y CD-I63-F0-R2-02 no las pide. Las pruebas existentes que se
    citan nuevas en R2 se **leyeron**, no se ejecutaron.

## 1. Resumen (R1, con las precisiones de R2)

1. **Identidad lógica decidida; población abierta.** El mandato ya fija que N vistas o colocaciones del mismo RackId aportan **un**
   rack a `Project.TotalRacks`.
   - Lo que queda abierto es la **población admitida** (definida o colocada; ámbitos de colocación; casos incompletos) y sus
     **diagnósticos** (§20, P-01..P-04).
   - **R2:** hay cinco casos distintos, y cada consumidor actual los trata de forma propia (§16.1):
     - identidad ausente o no recuperable;
     - identidad recuperable con un sobre ilegible;
     - `Kind` ausente;
     - `Kind` desconocido;
     - fallo de una métrica con identidad conocida.
   - RACKLISTA filtra el `Kind` **ausente**, pero **lista** el desconocido con su *token* como etiqueta.
2. **Ya existe agrupación pura en Application.**
   - `RackListBuilder.Build` agrupa por `Id` con `OrdinalIgnoreCase` `[MEASURED]`.
   - `RackPhysicalSelection.Classify` forma los grupos de selección con el mismo comparador `[MEASURED]`.
   - Lo **no acreditado** es un contrato neutral **común** de captura, población, admisión y agrupación útil para ID20 e I-64 (§7.2).
3. **Hay dos capturas en el Plugin.**
   - `ScanEnvelopes` omite xref y no distingue el tipo de colocación. Su coste no está medido.
   - `RackSiblingScan`, de I-55, separa las referencias cuyo dueño es un *layout* de las anidadas. Esa marca **no** distingue Model
     Space de Paper Space, porque ambos son *layouts*. También marca xref y tantea el `Id` en sobres ilegibles; ese `Id` tanteado
     solo sirve para **bloquear** una operación, nunca para dar pertenencia ni *authored* (§16.1).
   - Cada consumidor aplica después su propia política (§7.2, §17).
4. **Las cifras de RACKLISTA y RACKBOMTOTAL son magnitudes distintas sobre poblaciones distintas, no una métrica duplicada.**
   - RACKLISTA: racks lógicos **definidos** (filas).
   - RACKBOMTOTAL: racks **cotizados** (`ConsolidatedBom.Racks`), y su ventana muestra `Racks.Count` y `Σ max(1, Copies)`.
   - La duplicación solo aparecería si ID20 definiera `TotalRacks` con una de esas poblaciones sin reconciliarla (§7.2, M-01).
5. **Fases por dependencia real, no por prefijo.**
   - Contar RackIds sobre una instantánea de sobres **no** exige resolver diseños ni catálogos. Eso no prueba que la travesía de
     metadatos sea barata ni segura: no está medida (§15).
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
9. **Coordinación con el Master:**
   - las dos consultas del Coordinator, la original y la actualizada tras R1, están entregadas al Owner;
   - esta sesión **no** envió ninguna, porque no tiene canal con el Master;
   - **no** se ha recibido respuesta (§21);
   - I-64 confirmó por canal entre sesiones que no diseña ese mínimo común;
   - **posterior a R2:** el Owner resolvió P-14 sin contrato común; las consultas quedan superadas y no hace falta respuesta del
     Master (§24).
10. **R2: referencia directa ≠ instancia alcanzable.**
    - La multiplicidad física no cambia la deduplicación por RackId.
    - Si la población elegida es la **colocada**, saber si existe una colocación pertinente **sí** puede cambiar la admisión y, por
      tanto, el total.
    - Hoy `DirectReferenceCount > 0` no distingue una referencia dentro de una definición contenedora que nunca se colocó (§17, §18).

## 2. Disposición de los hallazgos

### 2.1 R2 (R63-R1-01 y R63-R1-02)

La revisión de R1 (CD-I63-F0-R2-01) da R63-DISC-02..07 por satisfechos **dentro del alcance de Discovery**. Esa aceptación no elige
métricas, población, identidad de símbolos, persistencia ni fases. R63-DISC-01 quedaba pendiente solo de separar casos.

| ID | Corrección aplicada | Dónde |
|---|---|---|
| R63-R1-01.1 | `Kind` ausente y `Kind` desconocido dejan de tratarse como una sola exclusión de RACKLISTA. `Build` (`RackListBuilder.cs:46-79`) solo exige `Id` y `Kind` **no vacíos**: un *token* desconocido pasa el filtro y `KindLabel` (`:82-96`) lo muestra tal cual. Con hermanas en estados distintos se describe por grupo y por los filtros reales. RACKLISTA no se toca | §3 (fila 6), §7.2, §16.1 |
| R63-R1-01.2 | Un sobre ilegible no siempre carece de identidad. Se separan cinco casos por consumidor. El `Id` tanteado (`RackSiblingScan.cs:131`) solo bloquea en `RackSiblingMembership` (`:132-137`); no es identidad inventada, *authored* ni garantía de admisión | §5.1, §7.2, §8.2 (I-03, I-04), §16.1 |
| R63-R1-02.A | Se corrige «anidados no afecta a `TotalRacks`» (§18). La multiplicidad no cambia la deduplicación, pero, con la población **colocada**, la existencia de una colocación pertinente puede cambiar la admisión. Una referencia directa activa no es una instancia alcanzable. Se deja explícito el escenario de la definición contenedora que nunca se colocó | §17, §18 |
| R63-R1-02.B | El ejemplo de `GetBlockReferenceIds` se limita a una sola referencia directa: con k referencias, k elementos, sin multiplicar por N. `directOnly: false` **no** se propone como conteo de instancias. La marca *layout* de `RackSiblingScan` no separa Model Space de Paper Space. La página 2022 (corroborada por el Coordinator) sigue siendo evidencia documental, no verificación en AutoCAD 2025 | §1.3, §17 |
| R63-R1-02.C | P-01..P-03 sin consecuencias falsas: el predicado de colocación sustituye a «lo visible»; P-02 separa **admisión** y **clasificación**; P-03 distingue total exacto, identidades con cobertura no acreditada y métrica parcial o no disponible. Los oráculos I-05, I-06 e I-11 se alinean con esas decisiones abiertas. P-04 y P-05 siguen abiertos | §8.2, §20.1 |
| Estado de la consulta | Se corrige la redacción que podía leerse como transmitida («elevado al Master», «en el Master», «en coordinación»). Consta la consulta actualizada | §9.2, §11, §12, §13, §21, §23 |

### 2.2 R1 (R63-DISC-01..07; histórico)

| ID | Corrección aplicada | Dónde |
|---|---|---|
| R63-DISC-01 | Se retira la alternativa «1 frente a N» para el mismo RackId. D-OPEN-01 pasa a ser población, ámbitos de colocación, admisibilidad y diagnósticos. Se separan los tres tipos de exclusión (R2 los amplía a cinco casos más el de hermanas en estados distintos: §2.1). No se añade ninguna métrica de copias | §1.1, §16.1, §20 |
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
| 6 | RACKLISTA: una fila por RackId lógico **definido** (`OrdinalIgnoreCase`), colocado o no. Omite **sin aviso** tres tipos de sobre: ilegible (`Deserialize` = null, incluido el de una versión MAJOR superior), sin `Id` y con `Kind` vacío. Un `Kind` **no vacío y desconocido sí se lista**, con el *token* como etiqueta. Nombre y kind = primer valor no vacío entre las hermanas **que pasan el filtro**. Copias = máximo de referencias directas de esas mismas hermanas | `Persistence/RackListBuilder.cs:46-79, 82-96`; `RackCad.Plugin/RackInventarioCommands.cs:50-73`; `Persistence/RackEmbedDocument.cs:97-129`; `RackListBuilderTests.Build_IgnoresEnvelopesWithoutIdOrKind`, `KindLabel_UnknownKind_ReturnsRawKind_DisplayOnly` (leídas) | `[MEASURED]` |
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
  - **R2.** El tanteo solo acepta un `Id` de primer nivel, de tipo cadena y único; en cualquier otro caso devuelve null
    (`RackEnvelopeIdProbe.cs:9-31`; `RackEnvelopeIdProbeTests.Only_one_readable_top_level_id_is_attributable`, leída).
  - Su único uso de producto está en `RackSiblingMembership.Classify` (`:132-137`): un sobre ilegible, no xref, cuyo `Id` tanteado
    coincide con el rack objetivo se clasifica como `BlockingUnreadable`, es decir, **bloquea** la operación. En cualquier otro caso
    queda `NotMember` (`RackSiblingMembershipTests.UnreadableSameProbeBlocks`, `UnreadableOtherProbeIsNotMember`,
    `UnreadableWithoutProbeIsNotMember`, leídas) `[MEASURED]`.
  - Por tanto, un `Id` tanteado no es una identidad inventada ni autoridad *authored*, y no garantiza la admisión en ninguna población.
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
| RACKLISTA (`RackInventarioCommands.RackLista` + `RackListBuilder.Build`) | Definiciones de `ScanEnvelopes(includeReferenceCount: true)`: no layout, no anónimas, **no xref** | Omite **sin aviso** el sobre nulo (ilegible), sin `Id` o con `Kind` vacío. Un `Kind` no vacío y desconocido **pasa** y se etiqueta con su *token* (R2) | `Id`, `OrdinalIgnoreCase`; nombre y kind = primer valor no vacío entre las hermanas que pasan el filtro | Filas = racks lógicos **definidos** (colocados o no); `ViewCount`; copias = máximo de referencias directas de esas hermanas | `RackListWindow` (filas por nombre) |
| RACKBOMTOTAL (`RackInventarioCommands.BomTotal` + `ConsolidatedBomBuilder`) | Las mismas definiciones + el registro leído una vez | **Inclasificable** = sobre nulo, `Id` vacío o `Kind` vacío; la proyección **no conserva** el `Id` en ese caso (`ProjectVariableScanProjection.cs:37-42`). Inclasificable y colocado: **aborta** todo el comando, aunque otras hermanas del mismo rack sean legibles; sin colocar: salta. Solo colocados (copias > 0). Kind no vacío sin handler (se resuelve con el kind de la **primera** hermana): aborta. Sin autoridad: salta con aviso. Bloqueado: aborta. Variable rota: aborta. Diseño ilegible: salta con aviso (`BomTotal.cs:84-96, 123, 134, 155, 200, 213`) | Hermanas por RackId `OrdinalIgnoreCase`; autoridad `BomAuthoredAuthority` | `Racks` (cotizados); `RackCount = Racks.Count`; `TotalCopies = Σ max(1, Copies)`; BOM total | `RackConsolidatedBomWindow` («N racks · M copias»), CSV y XLSX |
| `FindRackBlocks` (`RackCommandSupport.cs:117-140`) | `ScanEnvelopes(false)` | Sobre no nulo con `Id` == rackId. **Tolera `Kind` ausente o desconocido**, porque no lo mira. Omite sin aviso el sobre ilegible aunque su `Id` sea recuperable (R2) | `OrdinalIgnoreCase` | Lista de definiciones de **un** rack | RACKEDITAR / Actualizar; ejecutor de variables |
| `RackSiblingScan` + `RackSiblingMembership` (I-55) | Definiciones no layout y no anónimas, **xref incluidas con marca** | Tantea el `Id` del sobre ilegible solo para **bloquear** (`BlockingUnreadable`), nunca para dar pertenencia. Distingue el dueño *layout* (Model o Paper Space, **sin** distinguirlos) del anidado. La pertenencia no depende del `Kind`; el *callback* `eraseByKind` del llamador solo elige `Erase` o `Redraw` (`RackSiblingScan.cs:76, 142`) (R2) | Por RackId, `OrdinalIgnoreCase` (en Application) | Pertenencia de las hermanas de un rack | Insertar; RACKPROYECTAR |
| `RackPhysicalSelection.Classify` | **Selección** del usuario, no el dibujo | Sin RackId: se ignora o se conserva para que falle el plan (OD-3) | Claves físicas `Ordinal`; grupos `OrdinalIgnoreCase` | Grupos de la selección | `RackProjectionPipeline` (RACKPROYECTAR) |
| `RackNewRackName` (I-60) | `ScanEnvelopes(false)` | Nombres no vacíos | — | Siguiente nombre de la familia (no es un conteo) | Creación de racks nuevos |
| Etiquetas de kind | `RackListBuilder.KindLabel` (sin Push Back); `IRackKindHandler.BomLabel`; `SystemDescriptor.LibraryLabel` | — | — | Texto de presentación | RACKLISTA, BOM y biblioteca |

Toda la tabla es `[MEASURED]` en el código citado.

**Conclusión de EXP-02** `[INFERENCE]`: RACKLISTA y RACKBOMTOTAL miden **magnitudes distintas sobre poblaciones distintas**, y que sus
cifras difieran no prueba una duplicación. La ambigüedad real es **qué población y qué admisión adopta `Project.TotalRacks`**: definida,
colocada o cotizable; qué ámbitos y qué predicado de colocación; qué diagnósticos. Esa decisión está pendiente (§20, P-01..P-04). No se autoriza
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
| I-03a (R2): una identidad ausente o no recuperable no recibe un id inventado ni se fusiona con otro rack; su efecto sobre el total sigue P-01..P-03 | `ProjectVariableScanProjectionTests` (proyección); `RackEnvelopeIdProbeTests` (tanteo nulo, leída); hueco para el resumen | Definición colocada con sobre ilegible y sin `Id` tanteable | Un resumen que le invente un id, la cuente como rack identificado o la omita sin dejar rastro |
| I-03b (R2): una identidad recuperable con un sobre ilegible no da *authored* ni admisión por sí sola, y no crea un segundo rack si coincide con hermanas legibles | `RackSiblingMembershipTests.UnreadableSameProbeBlocks` y `UnreadableOtherProbeIsNotMember` (pertenencia, no conteo; leídas); hueco para el resumen | Rack con frontal legible y lateral ilegible cuyo `Id` tanteado coincide | Contar dos racks, o tomar la vista ilegible como *authored*. Si cuenta cuando no tiene ninguna hermana legible depende de P-02 y P-03 |
| I-04a (R2): `Kind` ausente con `Id` es un caso propio | `RackListBuilderTests.Build_IgnoresEnvelopesWithoutIdOrKind` (RACKLISTA lo filtra; leída); hueco para el resumen | Sobre con `Id` y `Kind` vacío | Tratarlo como identidad ausente o como kind desconocido sin que P-02 lo decida |
| I-04b (R2): `Kind` desconocido con `Id` es un caso propio, distinto del ausente | `RackListBuilderTests.KindLabel_UnknownKind_ReturnsRawKind_DisplayOnly` (solo la etiqueta; leída); hueco para el resumen | Sobre con `Kind = "futuro"` | Suponer que hoy se excluye (RACKLISTA lo lista), o asignarle un kind conocido o un código numérico |
| I-05 (alineado en R2; candidato condicionado a P-03 y P-07): el fallo de una métrica de diseño o de capacidad no cambia `TotalRacks` por sí solo, ni lo vuelve parcial, si la identidad del rack incluido está acreditada | Hueco | Selectivo admitido en la población, con variable rota | `TotalRacks` baja, o se marca como parcial, al fallar `Frentes` o `Altura` |
| I-06 (alineado en R2): la población declarada se aplica de forma uniforme, y una exclusión deliberada por alcance no es parcialidad | Hueco | (1) Rack definido sin colocar; (2) rack referenciado solo dentro de una definición contenedora que nunca se colocó | Con P-01 y P-04 decididos, el rack cuenta o no según el predicado. Fallos: contar (2) como colocado cuando el predicado exige una instancia alcanzable, o marcar como parcial una exclusión por alcance |
| I-07: ningún parámetro calculado legible desde una fórmula de propiedad (o fase demostrada segura) | Parcial: hoy `BindingInspection` rechaza namespaces ajenos | Fórmula de `VerticalClearance` que lee `Rack.Altura` | Sin la regla, se evalúa con un valor anterior o entra en ciclo |
| I-08: un agregado `Project.*` que dependa de modelos resueltos no es legible desde variables de proyecto | Hueco | Variable `=Project.TotalPosiciones` + rack enlazado | Recursión o resolución completa del dibujo |
| I-09: `Rack.Frentes` (Selectivo) numérico en un contexto de rack para ID23 | Hueco | Rack de 4 frentes; `Rack.Frentes * 2` | Debe dar 8 con la semántica decidida (P-05); otro valor si se toma la bahía equivocada o la suma de fondos sin decidir |
| I-10: hermanas divergentes o ilegibles → estado explícito, no primera vista | Selectivo: `SelectiveBomAuthorityTests`; los demás kinds: los puertos AUTH-13 de I-58. Hueco en el resumen | Dinámico con dos vistas divergentes | Sin la regla, el valor depende del orden del `BlockTable` |
| I-11 (alineado en R2): un resultado con cobertura no acreditada no se presenta como total exacto | Hueco | Tres sobres colocados, uno ilegible: (a) sin `Id` recuperable; (b) con `Id` recuperable que coincide con otra vista legible; (c) con `Id` recuperable y sin ninguna vista legible | El valor esperado depende de si la identidad se conserva y de la población aprobada (P-01..P-03); **no** se fija «da 2» como resultado universal. Fallos: presentar como total exacto el caso (a), contar un rack de más en (b), o decidir (c) sin P-02 |
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
| I-64 | ROADMAP (filas adyacentes tras I-60) | **No inspeccionado en código**: no tiene producto. Su Discovery (§9.4) propone, como hipótesis, un índice runtime por documento (`RackId`, nombre, `SystemKind`, vistas, definiciones, conteo de referencias) alimentado por la misma travesía y sin retener `Design`. Su §10.2 dice que «ambas consumen la misma fundación integrada (Rack Identity, `RackListBuilder`)» y que una autoridad nueva de enumeración sería STOP y Master `[MEASURED]` (lectura de `cf034e59`) | Candidato a contrato compartido de hechos y agrupación. Consulta para el Master preparada y entregada al Owner, **no enviada** por esta sesión, sin respuesta (§21; redacción corregida en R2) |
| I-62 | ROADMAP (otra tabla) | Ninguno de producto; Discovery delta de proceso | Protocolo de ejecución delegada |
| I-52 | ROADMAP, HANDOFF, índice de ADR | No inspeccionado; su rama no tiene producto | Identidad de racks espejados (AUTH-15) `[UNKNOWN]` |

Canal con I-64: respondí a su consulta de frontera, sin compromisos (evidencia §11). I-64 adoptó la corrección de R63-DISC-02 y declaró
que no diseña el mínimo común.

### 9.3 Observación R2 (2026-10-01T19:15:18Z)

I-52 `88138f01`, I-62 `ea055591`, I-63 `85293273`, I-64 `c6c44828`, `main` `819955d6`. Coinciden con las puntas que observó el
Coordinator. Solo se revisaron los deltas de coordinación pertinentes; no se repitió `merge-tree`.

- **I-64** (`cf034e59..c6c44828`): solo cambia sus tres documentos.
  - Su Discovery §9.4 ya describe las dos reglas de pertenencia: `FindRackBlocks` omite sin aviso el sobre ilegible atribuible, y
    `RackSiblingMembership` falla cerrado con `BlockingUnreadable`. Eso coincide con el §16.1 de R2.
  - Mantiene el coste del índice como `UNKNOWN`.
  - Su §10.2 registra la consulta de I-63 como «preparada por su Coordinator, no enviada» `[MEASURED]` (lectura de `c6c44828`).
- **I-62** (`f25dd1d5..ea055591`): solo `docs/` (Proposal V1, paquete del Architect, borrador de ADR sucesor). Sin cruce de producto.
- **I-52** (`c1982b2a..88138f01`, avance *fast-forward*): solo añade `eng/research` y `eng/validation`.
  - Respecto de su base de fusión con `main` (`95690c28`), no cambia nada en `src/` ni en `tests/` (diff de tres puntos).
  - El diff de dos puntos contra `main` sí lista seis archivos de `src/` y `tests/`, pero son cambios de I-61 que I-52 aún no
    incorporó, no producto propio.
  - Cruce funcional con I-63: ninguno observado en esos deltas; su contenido no se inspeccionó entero.

### 9.4 Observación posterior a R2 (2026-10-01T19:54:59Z)

I-52 `d8078ef3`, I-62 `a1f5e003`, I-63 `23eeefc5`, I-64 `dcc16bed`, `main` `819955d6`. Las tres ramas ajenas avanzan por
*fast-forward* y ninguna cambia `src/` ni `tests/` respecto de `main` (diff de tres puntos).

- **I-64** (`c6c44828..dcc16bed`): Proposal V1 y paquete del Architect, solo documentos.
  - Su evidencia §13 registra una decisión del Master, `MASTER-I63-I64-01`, recibida por su Coordinator: fundación mínima común de
    *snapshot* lógico neutral por RackId, con **I-63 como autor inicial** del contrato puro común, y una F2 de I-64 que lo consume solo
    cuando esté integrado `[MEASURED]` (lectura de `dcc16bed`, resumen saneado de I-64).
  - **I-63 nunca recibió esa decisión**: la conoce solo por esa lectura.
  - La decisión posterior del Owner sobre P-14 prevalece para I-63 (§24).
- **I-62** (`ea055591..a1f5e003`): Proposal V2 y registro de su revisión; solo `docs/`.
- **I-52** (`88138f01..d8078ef3`): decisiones y evidencia de su campaña de host; solo `docs/`.

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
| M-07 | **Activado para planificar (UNKNOWN)** | No hay ninguna abstracción de *provider* en las rutas inspeccionadas; la composición por kind está en el Plugin; para un posible contrato compartido de captura y agrupación con I-64 hay una consulta al Master preparada y entregada al Owner, no enviada por esta sesión y sin respuesta (§21). **Posterior a R2:** el Owner decidió que no se exige ese contrato común (§24), así que la pregunta M-07 se limita al mecanismo propio de I-63. La pregunta no se cierra eligiendo una arquitectura | Master (contrato compartido) y después Proposal + Architect (catálogo o registro) |
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
| EXP-07 | **Cerrada por decisión del Owner** (posterior a R2) | Hasta R2: consulta al Master preparada y entregada al Owner (original y actualizada tras R1), **no enviada** por esta sesión y sin respuesta (§21). Después, el Owner decidió P-14 sin contrato común; las consultas quedan superadas y no hace falta respuesta (§24) |
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
| 7 | `RackListBuilder.Build` es una agrupación pura **reutilizable**; su admisión (exige `Id` y `Kind` no vacíos, no exige un kind conocido; omite sin aviso) y su orientación a presentación (nombre, etiqueta) hay que decidirlas antes de adoptarlo como base de `TotalRacks` `[MEASURED]`/`[INFERENCE]` | §7.2 |
| 8 | `ConsolidatedBomBuilder` sirve para el BOM. No es una fuente de métricas de producto: cuenta instancias de dibujo, la población «cotizable» y la autoridad de primera vista en kinds no selectivos `[MEASURED]`/`[INFERENCE]` | §7.2, §18 |
| 9 | Dedupe `OrdinalIgnoreCase` en todos los agrupadores leídos; sin RackId no hay grupo `[MEASURED]` | §7.2, §7.3 |
| 10 | Métricas dispersas: §14 `[MEASURED]` | §14 |
| 11 | Comunes de verdad: kind y racks lógicos por población. Las demás, por kind `[INFERENCE]` | §14 |
| 12 | Ciclos: riesgos de extensión futura por dependencias reales `[RECONSTRUCTED]` | §16.2 |
| 13 | ID30: el Discovery identificó un candidato a contrato compartido. **Posterior a R2:** el Owner decidió que no se exige; cada iniciativa tiene su propio mecanismo y la duplicación queda diferida | §9.2-9.4, §21, §24 |
| 14 | 100 % Application: *providers* y agregación puros, si reciben instantáneas de captura y catálogos cargados `[INFERENCE]` | §6, §17 |

## 14. Matriz sistema × métrica (R63-DISC-05)

Leyenda de estado: `Supported` = fuente actual única y semántica clara; `NotApplicable`; `Unavailable`; `NeedsContract` = hay datos,
pero falta decidir la semántica o la autoridad. `NeedsContract` es un resultado válido de Discovery, no una exclusión. Cada celda indica
**fuente actual · pregunta pendiente · dependencia/fase · datos no fiables**.

| Métrica | Selectivo | Dinámico | Push Back | Cantilever | Cabecera | Cama |
|---|---|---|---|---|---|---|
| **SystemKind** (texto; **no** sirve para la vía numérica) | Supported · *token* `selective` · sin pregunta · instantánea · `Kind` ausente y `Kind` desconocido = dos casos propios (§16.1) | Supported (`dynamic`) | Supported (`pushback`) | Supported (`cantilever`) | Supported (`cabecera`) | Supported (`cama`) |
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
| `TotalRacks` | Instantánea de sobres (Id, Kind, View, Section) + hechos de colocación según la población (predicado de P-04; una referencia directa activa no es por sí sola una instancia alcanzable, §17) + completitud de la captura (sobres ilegibles, con o sin `Id` recuperable) | Política de población y admisión (P-01..P-04) | **Metadatos**: sin deserializar `Design` ni resolver. Coste y seguridad de la travesía existente **no medidos** | Resumen, I-64 (presentación) | Una sola captura por lectura | No | Ninguno: solo lectura | Alta, baja o cambio de identidad, de colocación o de legibilidad de un sobre |
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

Contar RackIds solo necesita lo primero. Eso no lo hace barato ni seguro por principio: la travesía actual deserializa cada sobre y
retiene la cadena `Design` de todos ellos durante el recorrido (`RackBlockFinder.cs:57-92`; I-64 §9.4 en `c6c44828`), y su coste no
está medido (§19).

## 16. Identidad, fases y símbolos

### 16.1 Admisión, identidad y lectura del sobre (R63-DISC-01; separada en R2 por R63-R1-01)

Hechos actuales por consumidor, todos `[MEASURED]` en el código citado en §3 (fila 6), §5.1 y §7.2. La última columna **no** es una
decisión: dice qué falta decidir y quién lo decide. Un sobre es **ilegible** cuando `RackEmbedStore.Deserialize` devuelve null:
JSON inválido o versión MAJOR superior (`RackEmbedDocument.cs:97-129`).

| Caso | RACKLISTA | RACKBOMTOTAL | `FindRackBlocks` | `RackSiblingScan` + `Membership` | Pendiente (sin decidir) |
|---|---|---|---|---|---|
| **A. Identidad ausente o no recuperable**: sobre legible con `Id` vacío, o sobre ilegible sin `Id` tanteable | Omitido sin aviso | Inclasificable: si está colocado, **aborta** todo el comando; si no, salta | No coincide con ningún rack | `NotMember` | Nunca se inventa identidad. Su efecto sobre el total y su diagnóstico: P-01 y P-03 (cobertura no acreditada si cae en la población admitida) |
| **B. Identidad recuperable con sobre ilegible**: el tanteo devuelve un `Id` único de primer nivel | Omitido sin aviso (no tantea) | Igual que A: no tantea y no conserva el `Id` | Omitido sin aviso (no tantea) | `BlockingUnreadable` si el `Id` tanteado es el del rack objetivo y la definición no es xref; si no, `NotMember`. Nunca es miembro ni *authored* | El `Id` tanteado no es identidad inventada, *authored* ni garantía de admisión. Si coincide con hermanas legibles, no crea otro rack. Sin ninguna hermana legible, admitirlo o no y cómo diagnosticarlo: P-02 y P-03, conforme a las fundaciones (Rack Identity; Freeze I-55 para la pertenencia) |
| **C. `Kind` ausente**: sobre legible, `Id` no vacío, `Kind` vacío | **Esa definición** se omite sin aviso. Si otra hermana del mismo RackId tiene `Kind`, el rack aparece solo con las vistas y copias de esas hermanas | Inclasificable (la proyección descarta el `Id`): si está colocado, **aborta** aunque otras hermanas sean legibles; si no, salta | Tolerado (coincide por `Id`) | Miembro según su RackId; el `Kind` no interviene | P-02 en sus dos partes: **admisión** y **clasificación**. Se evalúa por grupo: un rack sin kind en ninguna vista no es lo mismo que una vista sin kind de un rack que lo tiene en otra |
| **D. `Kind` desconocido**: no vacío y sin entrada en `KindLabel` ni handler | **Se lista**, con el *token* como etiqueta | Se proyecta como `Foreign` y se agrupa. Si el rack está colocado y la **primera** hermana tiene ese kind, `TryResolveAll` **aborta** todo el comando; si no está colocado, no se mira | Tolerado | Miembro según su RackId | P-02: admisión y clasificación (¿categoría explícita en `RacksBySystem`?). Nunca se le asigna un kind conocido ni un código numérico |
| **E. Fallo de una métrica con identidad conocida**: diseño ilegible, hermanas divergentes, variable rota, diseño bloqueado | No aplica (no lee `Design`) | Diseño ilegible o sin autoridad: salta con aviso. Variable rota o diseño bloqueado: **aborta** | No aplica | No aplica | Estado de esa métrica (P-07). Por sí solo **no** saca al rack de `TotalRacks` ni lo vuelve parcial si su identidad está acreditada (I-05, P-03) |
| **F. Hermanas en estados distintos** (mismo RackId) | El grupo se forma solo con las hermanas que pasan el filtro; nombre y kind = primer valor no vacío; si ninguna pasa, no hay fila | Se decide por definición: una hermana inclasificable y colocada aborta todo. Entre las legibles decide `BomAuthoredAuthority` | Devuelve las hermanas legibles con ese `Id` | Clasifica cada definición por separado | Se describe **por grupo** y por los filtros reales, nunca como conclusión universal sobre el rack; autoridad de hermanas por kind (P-06) |

Pruebas existentes que fijan parte de estas filas (leídas en R2, no ejecutadas): `RackListBuilderTests.Build_IgnoresEnvelopesWithoutIdOrKind`
(C), `RackListBuilderTests.KindLabel_UnknownKind_ReturnsRawKind_DisplayOnly` (D, solo la etiqueta), `RackEnvelopeIdProbeTests` (A/B)
y `RackSiblingMembershipTests.Unreadable*` (A/B en la pertenencia). Para el resumen, todas son hueco (§8.2).

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
página de 2022, consultada el 2026-10-01; el Coordinator también la corroboró; la de 2025 no pudo verificarse):
- devuelve solo las referencias **activas**;
- `directOnly: true` excluye las referencias del bloque padre cuando el bloque está anidado;
- `forceValidity` «solo aplica si `directOnly` es false».

Es evidencia documental de 2022, no una verificación en AutoCAD 2025. Tampoco es un contador de instancias expandidas.

**Referencia directa frente a instancia alcanzable (R2)** `[INFERENCE]`:
- Las dos capturas trabajan con **referencias directas activas** a la definición del rack: `ScanEnvelopes` las cuenta
  (`RackBlockFinder.cs:84-86`) y `RackSiblingScan` las clasifica por dueño (`RackSiblingScan.cs:119-125`).
- Que exista una instancia del rack **alcanzable** desde Model Space o Paper Space es otra condición. No se deduce solo de
  `ReferenceCount > 0`.
- La marca de `RackSiblingScan` distingue un dueño *layout* de uno anidado. Model Space y Paper Space son ambos *layouts*, así que esa
  marca **no** los separa.
- `directOnly: false` **no** se propone como conteo de instancias: no está acreditado como tal.

| Caso | Salida actual | Diagnóstico | Límite de la prueba |
|---|---|---|---|
| Rack colocado en Model Space | Cuenta en `ScanEnvelopes`; `RackSiblingScan` la marca como referencia de *layout* | — | Plugin, solo en AutoCAD |
| Rack colocado en un layout de Paper Space | Cuenta como referencia directa en `ScanEnvelopes`; `RackSiblingScan` la marca como referencia de *layout*, **igual que en Model Space** | Ninguno | `[RECONSTRUCTED]`; escenario manual: rack en un layout, RACKLISTA y RACKBOMTOTAL |
| Definición contenedora con **una** referencia directa del rack, contenedor colocado N veces | La consulta con `directOnly: true` devuelve **una** referencia (la que vive dentro de la definición contenedora), no N; `RackSiblingScan` la marca como anidada | Ninguno | `[EXTERNAL]` + `[RECONSTRUCTED]`; escenario manual: contenedor ×3 |
| Definición contenedora con **k** referencias directas del rack, contenedor colocado N veces | k referencias directas. No se infiere una sola ni se multiplica por N | Ninguno | `[EXTERNAL]` + `[RECONSTRUCTED]`; escenario manual: contenedor con 2 racks, colocado ×3 |
| Referencia del rack dentro de una definición contenedora que **nunca se colocó** (escenario de razonamiento, R63-R1-02.A) | La referencia directa es activa, así que se cuenta: RACKLISTA muestra copias ≥ 1, RACKBOMTOTAL lo admite como colocado (copias > 0) y `RackSiblingScan` la marca como anidada. Sin embargo, ninguna instancia es alcanzable desde Model Space ni Paper Space | Ninguno | `[RECONSTRUCTED]` sobre el código + `[EXTERNAL]` («solo referencias activas»); `[UNKNOWN]` en el host. No se ejecutó AutoCAD ni se propone un recorrido nuevo; escenario manual propuesto: bloque contenedor con un rack dentro, sin insertar, y luego RACKLISTA y RACKBOMTOTAL |
| Referencia borrada | Excluida (solo referencias activas) | — | `[EXTERNAL]`; escenario manual: borrar, contar, deshacer y contar |
| Definición sin referencias | RACKLISTA la lista con 0 copias si pasa su filtro; el BOM la salta si es inclasificable y la excluye si no está colocada | Ninguno | `[MEASURED]` |
| Definición de una xref | `ScanEnvelopes` la omite; `RackSiblingScan` la incluye marcada | Ninguno | `[MEASURED]` (código) |
| Sobre ilegible, sin `Id`, sin `Kind` o con `Kind` desconocido | Casos A-D de §16.1, con la salida de cada consumidor (R2) | RACKLISTA: ninguno; BOM: mensaje y aborto si está colocado (inclasificable o kind sin handler) | `[MEASURED]` |
| Kinds divergentes entre hermanas | RACKLISTA toma el primero; el BOM usa el kind de la primera hermana para el handler | Ninguno | `[MEASURED]`; prueba futura posible en Core sobre `RackListBuilder` |
| `forceValidity: true` frente a `false` con `directOnly: true` | Sin diferencia según la documentación externa | — | `[EXTERNAL]`; el comentario del código que atribuye coste a `true` no se verificó en el host `[UNKNOWN]` |

## 18. EXP-08 — Deuda que un resumen podría exponer

| Hallazgo | Disposición |
|---|---|
| Falta la etiqueta de Push Back en `RackListBuilder.KindLabel` | **No consumir** la etiqueta de RACKLISTA como identidad ni como texto del resumen; la clave es el *token* (P-09). No se arregla |
| `BomAuthoredAuthority` aprueba la primera vista en kinds no selectivos | **No consumir** como autoridad de métricas; usar la autoridad de hermanas por kind (decisión de Proposal; P-06) |
| RACKLISTA omite sin aviso los sobres ilegibles, sin `Id` o con `Kind` vacío, y lista los kinds desconocidos con su *token* (R2) | **Preservar** RACKLISTA tal cual; el resumen necesita su propia admisión y sus diagnósticos (P-02, P-03; §16.1) |
| Las poblaciones de RACKLISTA (definidos) y RACKBOMTOTAL (cotizados) difieren | **Preservar**; declarar la población de `TotalRacks` (P-01) y su relación con `Racks.Count` (P-08) |
| Referencias directas frente a instancias alcanzables (`directOnly: true`; corregido en R2) | **Dependencia de P-01 y P-04.** La multiplicidad física no cambia la deduplicación por RackId. Pero, si la población es la **colocada** en ámbitos admitidos, que exista una colocación pertinente **sí** puede cambiar la admisión y, por tanto, `TotalRacks`. Hoy `DirectReferenceCount > 0` no distingue una referencia dentro de una definición contenedora que nunca se colocó (§17). También afecta a cualquier magnitud física futura. No se corrige ni se propone un recorrido nuevo en esta orden |
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

### 20.1 De producto (Owner, con alternativas; corregida en R2)

Ninguna está aprobada. Todas parten de la regla del mandato que no se reabre: dentro de la población aprobada, un RackId aporta un rack
aunque tenga varias vistas o colocaciones. Las consecuencias son **condicionadas**: dicen qué pasaría con cada alternativa, no qué se
recomienda al Owner.

| ID | Pregunta | Alternativas | Consecuencias condicionadas |
|---|---|---|---|
| P-01 | Población de `TotalRacks` | (a) **definidos** en el dibujo (como RACKLISTA); (b) **colocados**, según el predicado de colocación que fije P-04; (c) **cotizables** (como el BOM) | (a) incluye definiciones sin ninguna referencia, por ejemplo racks borrados sin purgar. (b) depende del predicado: con el actual (una referencia directa activa) contaría un rack que solo está dentro de una definición contenedora que nunca se colocó (§17). Presencia y visibilidad son condiciones distintas: este predicado no trata la visibilidad. (c) mezcla la validez para cotizar (handler, autoridad, bloqueo) con el conteo: un rack que no se puede cotizar desaparecería del total |
| P-02a | **Admisión** de un rack con `Id` y `Kind` ausente, y de uno con `Kind` desconocido (pueden decidirse por separado; §16.1 C y D) | (a) se admite como rack identificado, con diagnóstico; (b) no se admite, con diagnóstico | (a) `TotalRacks` refleja todas las identidades acreditadas. (b) el rack queda fuera por regla de admisión, lo que no es por sí mismo parcialidad (P-03) |
| P-02b | **Clasificación** en `RacksBySystem` de los admitidos sin kind conocido | (a) categoría explícita (por ejemplo «kind ausente» o «kind desconocido»); (b) sin categoría | (a) la partición sigue sumando `TotalRacks`, aunque se admitan. (b) si se admiten, la partición no suma `TotalRacks` y la diferencia debe declararse. Para que la partición cuadre **no** hace falta excluir los kinds desconocidos: basta (a). Admisión y clasificación son decisiones separadas |
| P-03 | Qué se presenta en cada clase de resultado | Distinguir tres clases: (1) **total exacto** para la población declarada: todas las identidades incluidas están acreditadas; (2) **conteo de identidades conocidas con cobertura no acreditada**: por ejemplo, un sobre ilegible sin `Id` recuperable dentro de la población admitida; (3) **métrica agregada parcial o no disponible**: por ejemplo, la altura o la capacidad de algún rack. Alternativas para (2): (a) valor con su estado de cobertura y los diagnósticos; (b) sin valor (`Unavailable`) | Una exclusión deliberada por alcance (P-01, P-02a, P-04) **no** es por sí sola parcialidad. Que falle la altura o la capacidad **no** vuelve parcial `TotalRacks` si todas las identidades incluidas están acreditadas. Ningún valor parcial se presenta como total exacto. La UI no se decide aquí |
| P-04 | Ámbitos y predicado de colocación | Ámbitos: Model Space; Paper Space; referencias anidadas; xref. Predicado: (a) al menos una referencia directa activa (el actual); (b) al menos una instancia alcanzable desde un ámbito admitido | Fija el predicado de P-01 (b). Con (a), cuentan los racks que solo están dentro de una definición contenedora que nunca se colocó. Con (b), hace falta un recorrido que ninguna de las dos capturas actuales hace: `RackSiblingScan` solo distingue el dueño *layout* del anidado y no separa Model Space de Paper Space. No se propone desarrollarlo en esta ronda |
| P-05 | Semántica por kind de las métricas numéricas, empezando por los frentes del Selectivo | Fondo 0 (preferida) o fondo más largo; frentes vacíos sí o no | Define la vía ID23. La candidata numérica del Selectivo está admitida para diseño, **no** congelada |

### 20.2 De arquitectura (Proposal + Architect; ADR cuando corresponda)

- P-06: autoridad de hermanas por kind para las métricas;
- P-07: estados por métrica;
- P-08: relación con `ConsolidatedBom.Racks.Count`;
- P-09: clave de `RacksBySystem` (*token*);
- P-10: identidad de los símbolos (§16.3);
- P-11: persistencia de referencias (M-02);
- P-12: fases por consumidor;
- P-13: modelo neutral del resumen y unidades.

### 20.3 Reservadas al Master (historia) y resueltas por el Owner

P-14: contrato mínimo compartido con I-64, su responsable, consumidores y secuencia (§21). **Resuelta por el Owner después de R2**:
no se crea ni se exige un contrato común en esta iniciativa (§24).

## 21. Estado de la coordinación con el Master (EXP-07)

| Documento del Coordinator de I-63 | Bytes | SHA-256 | Estado |
|---|---:|---|---|
| `I63_I64_Consulta_Contrato_Compartido.txt` (original) | 4 227 | `e4ca7e8b…e935e930` | Entregada al Owner tras R1. **No enviada** al Master por esta sesión |
| `I63_I64_Consulta_Master_Actualizada_R1.txt` (actualización tras R1; **documento nuevo**, no sustituye al original) | 6 486 | `4c5f2843…915bb6c8d` | Preparada para que el Owner la traslade. **No enviada** por esta sesión |

- **Sin canal:** en R1, `ListAgents` no mostró al Master (evidencia §11.4). En R2 no se volvieron a enumerar sesiones (CD-I63-F0-R2-03).
- La consulta actualizada incorpora las dos capturas, la agrupación existente, la admisión y la colocación, y los límites revisados.
- **Estado: SIN RESPUESTA.** No se declara ningún envío ni respuesta, ni una solicitud «pendiente en el Master». EXP-07 y P-14 siguen
  pendientes de esta coordinación.
- La falta de respuesta no reabre G0 ni impide las correcciones de R2.
- **Posterior a R2:** el Owner decidió P-14 (§24). Las dos consultas quedan **SUPERADAS / NO ENVIAR** y no hace falta respuesta del
  Master. Esta sección conserva su estado anterior como historia.
- Mientras no haya disposición del Master, siguen detenidas la elección del contrato compartido, la Proposal y el Freeze. No se adopta
  la propuesta de I-64 ni su código, y su caché o su inventario runtime no se tratan como autoridad de métricas.

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

**Lecturas R2** (sin búsquedas nuevas de alcance amplio): `RackListBuilder.cs`, `RackInventarioCommands.cs`,
`RackInventarioCommands.BomTotal.cs`, `ProjectVariableScanProjection.cs`, `ProjectVariableScanEntry.cs`, `RackEmbedDocument.cs`
(`Deserialize`), `RackEnvelopeIdProbe.cs`, `RackSiblingScan.cs`, `RackSiblingMembership.cs`, `RackCommandSupport.cs`
(`FindRackBlocks`) y `RackBlockFinder.cs`; nombres de pruebas en `RackListBuilderTests`, `RackEnvelopeIdProbeTests` y
`RackSiblingRedrawTests`; deltas de las ramas paralelas y el Discovery de I-64 §9.4 y §10.2 en `c6c44828`.

**Huecos:**
- no se ejecutó nada del Plugin ni de AutoCAD;
- no se probó un *token* nuevo en el diseño Selectivo;
- no se inspeccionó código de I-64, I-62 ni I-52;
- la documentación de `GetBlockReferenceIds` es de 2022;
- el escenario de la definición contenedora que nunca se colocó (§17) es razonamiento sobre el código, sin verificar en el host;
- no se inventariaron todos los consumidores de `DynamicFrontActivation` (32 archivos).

## 23. Riesgos del mandato → estado

| Riesgo | Estado (R1, con precisiones R2) |
|---|---|
| Contar vistas en vez de RackId | Decidido por el mandato; los agrupadores leídos lo cumplen; faltan la población y el predicado de colocación (P-01, P-04) |
| *Legacy* sin identidad | Sin identidad inventada; el `Id` tanteado no es identidad ni admisión (§16.1, caso B); faltan la admisión y el diagnóstico (P-02, P-03) |
| Hermanas divergentes o ilegibles | Autoridad por kind (P-06) |
| Aritmética duplicada | No demostrada: hay magnitudes distintas. El riesgo aparece si `TotalRacks` adopta una población sin reconciliar (P-08) |
| Fases y ciclos ocultos | Riesgo de extensión futura; tablas de dependencia (§15-16) |
| Nombres visibles como identidad | Excluido por el motor y los agrupadores |
| Equivalencias falsas | Métricas por kind; ningún código numérico para los kinds |
| Parciales presentados como totales | P-03: tres clases de resultado; una exclusión por alcance no es parcialidad |
| Resoluciones repetidas | Los metadatos y la resolución se separan (§15, §19) |
| Solapamiento con ID30 | Hasta R2: consulta al Master preparada, no enviada y sin respuesta (§21). **Posterior a R2:** el riesgo se reformula como **duplicación aceptada conscientemente** de dos mecanismos independientes (I-63, métricas; I-64, inventario runtime), diferida a una futura iniciativa arquitectónica separada (decisión del Owner, §24) |

## 24. Decisión del Owner sobre P-14 (posterior a R2)

**Fuente:** decisión explícita del Owner, «I-63 — OWNER DECISION: frontera con I-64 / P-14», pegada en el chat de la sesión responsable
el 2026-10-01 (texto literal y procedencia en la evidencia §13). No es una orden del Coordinator ni una decisión del Master.

**Decisión:**
- No se crea ni se exige en esta iniciativa un contrato común de inventario o enumeración entre I-63 e I-64.
- **I-63** puede implementar su propio mecanismo para lo que necesitan sus métricas: enumeración lógica, deduplicación por RackId,
  población y admisión definidas por sus contratos, `ProjectSummary` y agregaciones.
- **I-64** puede implementar el suyo, de forma independiente, para su inventario runtime, la navegación, la selección y la
  representación de sesión y UI.
- I-63 no consume código no integrado de I-64, e I-64 no es autoridad de las métricas de I-63.
- No se exige la misma estructura interna ni una fundación nueva compartida.
- La duplicación posible queda **aceptada conscientemente**.
- Una iniciativa arquitectónica separada podrá revisarla después, cuando ambas implementaciones existan (idealmente integradas):
  duplicación, divergencia semántica, rendimiento, mantenimiento, extracción de una fundación común o sustitución de un mecanismo por
  el otro. Esa revisión podrá mantener ambos, unificarlos, hacer que uno consuma al otro o extraer una tercera autoridad.

**Relación con `MASTER-I63-I64-01`:**
- I-64 registró en su rama (`dcc16bed`, evidencia §13) una decisión del Master con el reparto contrario: fundación común, I-63 como
  autor inicial y la F2 de I-64 dependiente de ese contrato (§9.4). I-63 nunca la recibió.
- Ante ese conflicto, el Owner eligió expresamente que **su decisión prevalece para I-63** (evidencia §13).
- I-63 informó a I-64 por el canal entre sesiones, con un aviso informativo sin peticiones. Qué implica para la Proposal V1 de I-64 lo
  deciden su Coordinator y el Owner; I-63 no emite órdenes a I-64.

**Consecuencias para I-63:**
- EXP-07 y P-14 dejan de bloquear el diseño (§12, §20.3).
- Las dos consultas preparadas para el Master quedan **SUPERADAS / NO ENVIAR**; no hace falta respuesta (§21).
- El hallazgo del solapamiento se conserva como historia (§§9, 21 y 23). El riesgo se reformula como duplicación conscientemente
  diferida a una futura iniciativa arquitectónica (§23).
- La frontera del mandato sigue igual: I-63 posee la semántica de métricas, los *providers* y la agregación; I-64 posee el inventario
  runtime, la navegación y la UI y sesión. Sin consumo de código no integrado en ninguno de los dos sentidos.
- Siguen pendientes las decisiones de producto P-01..P-05 (§20.1; Owner). Después, la Proposal y la revisión del Architect, conforme
  al workflow y con una orden del Coordinator.
- Esta decisión **no** autoriza producto. IMPLEMENTATION AUTHORIZATION = NO hasta que Coordinator y Architect acuerden el mismo Freeze.
