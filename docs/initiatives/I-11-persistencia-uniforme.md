---
schema: rackcad-initiative/v1
id: I-11
title: Persistencia uniforme
type: architecture
status: in-progress
branch: architecture/persistencia-uniforme
base_branch: main
priority:
size: M
depends_on: [I-02]
conflicts_with: [I-03, I-08]
context_packs:
  - persistence
  - architecture-kernel
  - autocad-plugin
  - system-dynamic-flowbed
  - delivery-validation
automation_state_path: docs/automation/state/I-11.yml
decision_paths:
  - docs/automation/decisions/I-11.md
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-11 — Persistencia uniforme

## 1. Objetivo

Uniformar el tratamiento de los documentos persistidos de RackCad de modo que **todos** los payloads
de proyecto/diseño lleven versión de esquema explícita y **preserven los campos JSON desconocidos** al
cargar, editar, duplicar, guardar o re-serializar. En concreto (hallazgos D1 y D3 de la auditoría):

- **D1 — payloads sin versionar**: `FlowBedConfiguration` (cama) y `LargueroDesign` (larguero) hoy se
  serializan como **POCOs planos del Domain**, sin `SchemaVersion` ni `SchemaGuard`, a diferencia del
  resto (`RackProjectDocument` 2.0, `RackFrameProjectDocument` 1.0, `SelectivePalletDesignDocument` 1.0,
  `DynamicRackSystemDocument`). Se crean documentos de persistencia versionados `FlowBedDocument` y
  `LargueroDocument` en `RackCad.Application/Persistence`, **planos** (sin nodo envolvente nuevo), con
  `CurrentSchemaVersion`, mapeo explícito `FromDomain`/`ToDomain` y **fallback legacy** (un JSON plano
  antiguo sin `SchemaVersion` sigue cargando).
- **D3 — pérdida de metadata desconocida**: al reconstruir un documento en cualquier ruta de re-guardado
  se descartan las propiedades JSON que este build no conoce (un archivo escrito por una versión futura
  pierde sus campos aditivos al reabrirse y volver a guardarse). Se preservan los campos desconocidos en
  los **cuatro límites propiedad de I-11** — `RackEmbedDocument`, `RackProjectDocument`, `FlowBedDocument`
  y `LargueroDocument` — mediante `JsonExtensionData` (`Dictionary<string, JsonElement>`) más el acarreo
  del documento fuente donde una ruta reconstruye el modelo.

Resultado verificable: los formatos nuevos son legibles por builds anteriores (los campos aditivos —
`SchemaVersion` y la metadata extendida — son ignorables por `System.Text.Json`), los formatos legacy
siguen abiertos, y el JSON conocido, los nombres/casing de propiedades, los enums persistidos, el GUID,
la geometría, el BOM y las reglas de validación actuales **no cambian**.

## 2. Problema

`RackProjectDocument` selecciona el payload por `Kind`. Cinco payloads: `Header` y `DynamicSystem`
(documentos versionados), `SelectiveRack` (documento versionado), y **`FlowBed`/`Larguero`, que son los
POCOs del Domain serializados directamente** (`RackProjectDocument.FlowBed : FlowBedConfiguration`,
`RackProjectDocument.Larguero : LargueroDesign`). La cama, además, se embebe en el DWG a través de
`FlowBedConfigurationStore` → `RackEmbedDocument.Design`.

Dos consecuencias:

1. **Sin versión ni guarda** en cama y larguero: no hay `SchemaGuard` que rechace un archivo escrito por
   un build más nuevo (major superior); el resto de documentos sí lo tiene.
2. **Reconstrucción destructiva**: cada `Deserialize → (editar) → Serialize` crea documentos nuevos que
   descartan cualquier propiedad JSON no mapeada. `RackEmbedStore`, `RackProjectDocument`,
   `FlowBedConfigurationStore` y las rutas de re-estampado/edición pierden metadata desconocida.

La auditoría (D1/D3) y ROADMAP fila I-11 piden: `FlowBedDocument`/`LargueroDocument` versionados con
lectura legacy; versión (de app o esquema) en el envelope del Xrecord; y preservar campos desconocidos
al re-guardar.

## 3. Alcance

Estrictamente el alcance de I-11 en ROADMAP, sin ampliaciones laterales:

- **Documentos versionados** `FlowBedDocument` y `LargueroDocument` en `RackCad.Application/Persistence`:
  sellados, `CurrentSchemaVersion`, escriben `SchemaVersion`, **planos** (mismos nombres y casing que hoy
  producen `FlowBedConfiguration`/`LargueroDesign`), `FromDomain`/`ToDomain` explícitos, fallbacks legacy
  conservadores, y `JsonExtensionData` para campos desconocidos. **No** convierten a `FlowBedConfiguration`
  ni `LargueroDesign` (Domain) en DTOs; **no** introducen tipos JSON en Domain.
- **`RackProjectDocument`**: sus slots `FlowBed`/`Larguero` pasan a tipar `FlowBedDocument`/`LargueroDocument`
  (siguen siendo un objeto plano en disco, ahora con `SchemaVersion`); gana `JsonExtensionData` para los
  campos desconocidos de nivel wrapper. Se conservan el schema `2.0`, el discriminador `Kind`, el orden y
  casing PascalCase de las claves, y la escritura de los slots nulos.
- **`FlowBedConfigurationStore`**: conserva su API pública (`Serialize(FlowBedConfiguration)` /
  `Deserialize(string) → FlowBedConfiguration`, tolerante); ahora escribe el `FlowBedDocument` plano
  versionado, lee JSON plano legacy, aplica `SchemaGuard` y mantiene su tolerancia frente a junk/documentos
  degenerados. Ofrece una ruta interna documento-completo (con `ExtensionData`) para los consumidores que
  la necesiten, sin romper a los que solo requieren `FlowBedConfiguration`.
- **`RackProjectStore` + `RackProject` + `SystemRegistry.Default`**: adaptan la escritura/construcción de
  cama y larguero para usar `FlowBedDocument`/`LargueroDocument` **sin introducir un `switch` nuevo por
  Kind** (se conserva el despacho por `SystemRegistry` de I-08). `RackProject` gana un **documento fuente
  privado** (vive en Persistence) que permite preservar la metadata desconocida de nivel wrapper y de
  payload en una carga/guardado.
- **`RackEmbedDocument`**: se formaliza su versionado (`CurrentSchemaVersion`, guarda de versión coherente
  con `SchemaGuard`) conservando lectura legacy, y gana `JsonExtensionData` para preservar los campos
  desconocidos del envelope. La guarda de versión se aplica de modo que **no rompe el escaneo de bloques
  del dibujo** (un bloque ajeno/ilegible se sigue omitiendo, no aborta el listado).
- **Rutas de re-guardado**: se revisan `RackEnvelopeRestamp`, `RackCamaCommands`/`RackFlowBedWindow` y la
  biblioteca para que conserven la metadata desconocida sin rediseñar UI, con overloads mínimos, un campo
  de documento fuente o un helper de merge explícito.

**Compatibilidad obligatoria (preservar de forma verificable)**: JSON conocido, nombres y casing de
propiedades, enums persistidos (incluidos los nombres de `RackSystemKind` y `FlowBedType`), GUID de
identidad, geometría reconstruida, BOM de cama y larguero, reglas de validación laxas actuales
(`IsUsableFlowBed` = solo `LaneDepth`; `IsUsableLarguero` = solo `BeamProfileId`), el Xrecord existente
(su clave `DictKey`, ubicación, `chunk 255`, `DxfCode.Text`, definición vs. referencia y formato del
`ResultBuffer`), y el comportamiento observable de `SystemRegistry`.

## 4. Fuera de alcance

Explícitamente NO se toca (pertenece a otras iniciativas o excede D1/D3):

- **`IRackKindHandler`, `KindHandlerRegistry` ni el despacho por Kind del Plugin** — es **I-10**
  (`architecture/kind-handlers`, en curso). I-11 no implementa handlers ni migra RACKEDITAR/RACKBOMTOTAL.
- **`RackEnvelopeRestamp.cs`, `RackMenuCommands.cs`, `RackInventarioCommands.BomTotal.cs`** — los tres
  archivos que I-10 modifica. I-11 preserva la metadata del envelope **desde el tipo** `RackEmbedDocument`
  (su `JsonExtensionData` fluye por el `RestampEnvelope` existente sin editar ese archivo), evitando el
  solape. Ver §6 (frontera con I-10).
- **Draw Services** (`*DrawService`, `ViewBlockDraw`, `BlockPlacement`, `RackCatalogLoader`,
  `SystemBlockWriter`, `FlowBedDrawService`) — I-16, integrada; no se tocan.
- **`RackBlockData`** y el **formato físico del Xrecord** (`DictKey`, `RACKCAD_SELECTIVE`, chunk 255,
  `DxfCode.Text`, definición vs. referencia, `ResultBuffer`). Solo se toca si una prueba demuestra
  necesidad estricta; si eso exigiera cambiar el formato físico → **detenerse** y reportar gate de alcance.
- **Reglas de negocio, validaciones laxas, geometría y BOM**: sin cambios.
- **UI**: sin rediseño (solo, si es imprescindible, acarreo interno del documento fuente sin cambiar la
  experiencia).
- **Formatos legacy**: no se eliminan ni se migran destructivamente.
- **I-03** (escritura atómica, logging, archivos ilegibles): fuera; no se implementa escritura atómica ni
  logging aquí.
- **I-12** (centralización de `<Version>`, `PackageContents.xml`, publish/releases): no se edita
  `Directory.Build.props`, `PackageContents.xml`, deploy ni la estrategia global de versión. **No se crea
  una segunda fuente de verdad de versión de aplicación**: I-11 usa exclusivamente `SchemaVersion` (opción
  "versión de esquema" autorizada por el objetivo), salvo que una `ApplicationVersion` pueda derivarse de
  metadata ya existente sin duplicar la verdad.
- **Paquetes NuGet nuevos, catálogos, bloques DWG, deploy.**
- **`docs/HANDOFF.md` y `docs/ROADMAP.md`**: no se editan antes de la integración (WORKFLOW §4.5.4).

## 5. Contexto requerido

- **Global**: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/ARCHITECTURE.md`,
  `docs/auditoria-arquitectura-2026-07.md` (D1/D3) y este contrato.
- **Context Packs**: [`persistence`](../context-packs/persistence.md),
  [`architecture-kernel`](../context-packs/architecture-kernel.md),
  [`autocad-plugin`](../context-packs/autocad-plugin.md),
  [`system-dynamic-flowbed`](../context-packs/system-dynamic-flowbed.md),
  [`delivery-validation`](../context-packs/delivery-validation.md).
- **Contratos previos**: [`I-08`](I-08-system-registry.md) (despacho por `SystemRegistry`, que I-11
  conserva), [`I-09`](I-09-refactor-plugin-commands.md) (`RackEnvelopeRestamp`),
  [`I-16`](I-16-refactor-draw-services.md) (Draw Services, ya no se tocan),
  [`I-10`](I-10-kind-handlers.md) (frontera de Plugin, en curso).
- **Código a leer antes de editar**: `Persistence/RackProjectDocument.cs`, `RackProjectStore.cs`,
  `RackProject.cs`, `RackEmbedDocument.cs`, `FlowBedConfigurationStore.cs`, `SelectivePalletDesignDocument.cs`,
  `DynamicRackSystemDocument.cs`, `RackFrameProjectDocument.cs`, `SchemaGuard.cs`, `RackDesignValidation.cs`,
  `RackDesignLibrary.cs`; `Systems/SystemRegistry.Default.cs`, `SystemDescriptor.cs`;
  `Domain/Systems/FlowBedConfiguration.cs`, `LargueroDesign.cs`; `Plugin/RackCamaCommands.cs`,
  `RackEnvelopeRestamp.cs`; `UI/RackFlowBedWindow.xaml.cs`, `RackLargueroWindow.xaml.cs`,
  `RackMainMenuWindow.xaml.cs`; y las pruebas de persistencia existentes.

## 6. Dependencias y frontera con I-10

- **Depende de**: I-02 (integrada 2026-07-17). I-08 (integrada 2026-07-21) aporta el `SystemRegistry` que
  I-11 conserva.
- **Conflictos declarados en ROADMAP ("se estorba con")**: I-03 (`refactor/fallos-silenciosos`,
  **sin rama remota** al reclamar → inactiva) e I-08 (**integrada**, sin rama activa).
- **I-10 integrada** (`architecture/kind-handlers`, merge `c9f2d61` en `main` durante la corrida): I-11 se
  **rebaseó** sobre ese `main` sin conflictos (conjuntos de archivos disjuntos). Los tres archivos que I-10
  modifica (`RackMenuCommands.cs`, `RackInventarioCommands.BomTotal.cs`, `RackEnvelopeRestamp.cs`, todos en
  Plugin) quedan **fuera del alcance de I-11**. La preservación de metadata del envelope se logra **desde el
  tipo `RackEmbedDocument`** (Application), cuyo `JsonExtensionData` fluye por el `RestampEnvelope` de I-10
  (que sigue reserializando el MISMO objeto) sin editarlo; el `CamaKindHandler` de I-10 despacha a
  `RackCamaCommands.EditCama` (firma intacta). Si continuar exigiera handlers de I-10 o modificar sus tres
  archivos → **detenerse** (§12).
- Sin decisiones ni entradas del dueño previas.

## 7. Archivos esperados

**Crear** (en `src/RackCad.Application/Persistence`):

- `FlowBedDocument.cs`, `LargueroDocument.cs` (documentos versionados con `JsonExtensionData`).
- `SchemaVersionPolicy.cs` (política central: legibilidad por MAJOR + versión de escritura sin degradar).
- `RackEmbedComposer.cs` (fábrica pura de envelope que hereda `ExtensionData` y versión desde un `source`).
- Pruebas nuevas en `tests/RackCad.Tests/`.

**Modificar**:

- `src/RackCad.Application/Persistence/`: `RackProjectDocument.cs` (tipos de slot + `JsonExtensionData`),
  `RackProjectStore.cs` (acarreo del documento fuente; preservar `ExtensionData` y versión sin degradar;
  `TryDeserialize`/`IsReadable` para leer el diseño interior sin ocultar un MAJOR incompatible),
  `RackProject.cs` (documento fuente + `WithSourceMetadataFrom` y `WithSourceFlowBed`),
  `FlowBedConfigurationStore.cs` (escritura versionada + ruta documento), `RackEmbedDocument.cs`
  (`CurrentSchemaVersion` + `JsonExtensionData` + guarda tolerante en `RackEmbedStore`), `SchemaGuard.cs`
  (delega la legibilidad en `SchemaVersionPolicy`).
- `src/RackCad.Application/Systems/SystemRegistry.Default.cs` (mapeo cama/larguero vía documentos + guarda).
- `src/RackCad.Application/Persistence/InnerSourceResolution.cs` (**crear**): resultado discriminado
  `Success`/`BenignFallback`/`IncompatibleMajor`/`WrongKind` + `InnerSourcePreflightResult`.
  `RackProjectStore.ResolveInnerSource(designJson, expectedKind, initiating)` y `PreflightInnerSources(...)` puros;
  `RackProject.SourceFlowBedDocument` (accesor para el sidecar de cama).
- `src/RackCad.Plugin/`: `RackCommandSupport.cs` (`PreflightInnerSources` — envoltorio delgado del preflight puro),
  `RackCamaCommands.cs`, `RackSelectivoCommands.cs`, `RackDinamicoCommands.cs`, `RackCabeceraCommands.cs`. Cada
  `Build*Payload` compone el envelope vía `RackEmbedComposer`; el redibujo multivista conserva la metadata **del
  Embed de cada bloque**; una vista nueva hereda el envelope iniciador. **Dos límites independientes por bloque**:
  además del envelope, el diseño interior de dinámico y cabecera es a su vez un `RackProjectDocument`. `EditDynamic`/
  `EditCabecera` hacen un **preflight de TODAS las vistas ligadas antes de redibujar**: un MAJOR interior
  incompatible o un Kind interior incorrecto **abortan la edición completa** con mensaje visible (sin
  actualización parcial); en caso seguro, `Build*Payload` re-serializa el diseño con `WithSourceMetadataFrom` del
  proyecto resuelto por bloque (propio, o el iniciador en fallo benigno). El interior del selectivo es un
  `SelectivePalletDesignDocument` (no es uno de los cuatro límites; solo su envelope se preserva).
- `src/RackCad.Plugin/RackMenuCommands.cs` (**transporte mínimo, I-10 integrada**; sin cambiar handlers): la
  inserción desde biblioteca pasa el proyecto/documento fuente a `DrawDynamicView`/`BuildCamaPayload`/
  `RackCabeceraCommands.DrawAndPlace`.
- `src/RackCad.UI/`: `RackFlowBedWindow.xaml.cs` (`LoadForNew`/`LoadExisting` reciben el `RackProject` fuente **o**
  el `FlowBedDocument` fuente; guarda con `WithSourceMetadataFrom`/`WithSourceFlowBed`; expone
  `SourceFlowBedToInsert`), `RackLargueroWindow.xaml.cs`, `RackSelectiveWindow.xaml.cs` (`LoadForNew` recibe el
  proyecto fuente; `SaveToLibrary` usa `WithSourceMetadataFrom`), `RackDynamicSystemWindow.xaml.cs` (campo
  `sourceProject` fijado por `LoadDesignForNew`/`LoadExisting`/`OpenSystem_Click`; `SaveSystem_Click` usa
  `WithSourceMetadataFrom`; expone `SourceProjectToInsert`), `RackMainMenuWindow.xaml.cs` (transporta el proyecto
  fuente a los editores y **expone** `DynamicSourceProjectToInsert`/`FlowBedSourceDocumentToInsert`/
  `ConfigurationSourceProjectToInsert` para la inserción biblioteca→DWG). Sin rediseño de UI.

**Frontera de la cabecera de biblioteca (gate `owner-decision` — RESUELTO, aprobado)**: la biblioteca de
cabecera guarda por `RackFrameProjectStore`/`RackFrameProjectDocument` (un DTO **distinto**, esquema 1.0,
cabecera desnuda), que **no** es uno de los cuatro límites de I-11 y por diseño **nunca** produce un
`RackProjectDocument`. Preservar `RackFrameProjectDocument` sería un **quinto** límite (exigiría añadirle
`JsonExtensionData`). El dueño **aprobó excluir** `RackFrameProjectDocument` y la persistencia de cabecera
desnuda vía `RackFrameProjectStore` del alcance de I-11 (evidencia:
[`docs/automation/decisions/I-11.md`](../automation/decisions/I-11.md)); por eso
`requires_owner_decision: false`. I-11 queda limitada a los cuatro límites documentados. La preservación de
campos desconocidos en `RackFrameProjectDocument` se registra como **deuda/iniciativa posterior** (no se
cancela permanentemente). La cabecera **embebida** en el dibujo sí reconstruye un `RackProjectDocument`
interior y **sí** se preserva (arriba).

**Limitación explícita**: `JsonExtensionData` **no** es preservación recursiva de todos los DTO anidados. Se
preservan los campos desconocidos SOLO en los cuatro límites de I-11 (`RackEmbedDocument`,
`RackProjectDocument`, `FlowBedDocument`, `LargueroDocument`) y la versión de esos documentos. El diseño
interno type-specific de un `RackProjectDocument` (p. ej. el `DynamicRackSystemDocument`, la cabecera, o el
`SelectivePalletDesignDocument` del selectivo) se re-serializa desde el modelo reabierto y **no** conserva
campos desconocidos anidados **debajo** de esos DTO; los cuatro límites (incluido el wrapper `RackProjectDocument`
de nivel superior) sí.

Una **desviación material** —tocar un `*DrawService`, `RackBlockData`, el formato físico del Xrecord,
`RackEnvelopeRestamp`/`RackMenuCommands`/`RackInventarioCommands`, catálogos o geometría— obliga a
**detenerse** (§12).

## 8. Fases

- **F0 — Auditoría + reclamo**: estado real de main/I-10/I-11, reclamo atómico desde `origin/main`
  (`c5a4082`), `Claim-Id`, primer push aceptado sin force. **Sin código.**
- **F1 — Contrato**: este archivo, único cambio del primer commit no vacío; PR draft.
- **F2 — Caracterización que debe fallar antes del cambio**: pruebas que fijan el comportamiento actual y
  demuestran la ausencia de versionado/preservación (round-trip de campos conocidos; legacy plano sin
  `SchemaVersion`; `{}`/junk tolerante; Pushback `PalletDepth = 0`; MAJOR futuro rechazado; **campo
  desconocido sobrevive deserialize → editar campo conocido → serialize** en `FlowBedDocument`,
  `LargueroDocument`, `RackProjectDocument` y `RackEmbedDocument`; invariantes de GUID, geometría y BOM).
  Las pruebas de preservación deben **fallar** antes de F3.
- **F3 — Implementación**: la solución más pequeña que satisface F2 (documentos versionados; preservación
  de metadata; store/registry; envelope; rutas de re-guardado).
- **F4 — Validación local**: `git diff --check`, suite completa, builds sln/UI/Plugin, búsquedas de
  regresión (cero `RackDesignKind`, `SystemRegistry` presente, sin `IRackKindHandler` nuevo, sin cambios
  en Draw Services/HANDOFF/ROADMAP/catálogos, clave del Xrecord intacta).
- **F5 — Rebase final, push, PR, CI por SHA**: rebase sobre `origin/main` si avanzó (solo conflictos de
  I-11; `--force-with-lease`), PR draft único, CI verde sobre el HEAD final.

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` **verde** (toda la suite).
- **Caracterización de preservación (F2) fallando antes de F3** y verde después; no se debilitan pruebas.
- No se cambian expectativas golden existentes salvo **adiciones intencionales de `SchemaVersion`**
  (`SystemKindPersistenceCharacterizationTests` verifica claves de wrapper, no el interior de FlowBed/Larguero).
- Build Debug de `RackCad.sln`, `RackCad.UI` y `RackCad.Plugin` con **0 errores** (solo los `MSB3277`
  conocidos del Plugin), **0 advertencias propias** en UI. Sin paquetes nuevos.
- CI verde sobre el commit final (`requires_ci: true`): jobs Tests, Build UI, Build Plugin without AutoCAD.

## 10. Validación manual

- **AutoCAD: SÍ requerido (`requires_autocad: true`) — APROBADO por el Owner (2026-07-21).** La matriz
  obligatoria de abajo fue **ejecutada por el Owner** en AutoCAD 2025 (NETLOAD del DLL Debug del worktree,
  código `eea1c11`), quien confirmó explícitamente que **todos los escenarios pasan** (incluidos B5, B6 y S7).
  Evidencia: [`docs/automation/evidence/I-11-autocad-validation.md`](../automation/evidence/I-11-autocad-validation.md).
  El gate vigente pasa a **`owner-validation`**. `requires_autocad` se **conserva** en `true` (la matriz sigue
  siendo el criterio para cualquier re-validación tras un rebase).
- **Owner-validation: SÍ** (`requires_owner_validation: true`) — **pendiente** (revisión final del dueño e
  integración). No se declara aprobada aún. Las pruebas automatizadas cubren los **mecanismos** puros de
  Persistence (política de versión, `RackEmbedComposer`, `WithSourceMetadataFrom`/`WithSourceFlowBed`,
  `TryDeserialize`/`IsReadable`, y la preservación del interior dinámico/cabecera a nivel de store).

### 10.1 Matriz AutoCAD obligatoria (NETLOAD del DLL Debug del worktree)

Para cada familia (**cama, selectivo, dinámico, cabecera**):

| # | Escenario | Verificación |
|---|---|---|
| 1 | Inserción fresca (QUICK*/menú) | Dibuja; RACKEDITAR reabre; el embed nuevo lleva `SchemaVersion` actual (sin metadata fabricada). |
| 2 | Edición multivista (Actualizar) | Con ≥2 vistas del mismo GUID, editar un campo conocido redibuja **todas**; GUID **estable**; nombre conservado. |
| 3 | Metadata desconocida por bloque | Inyectar un campo desconocido y una `SchemaVersion` menor-superior del mismo major en el **envelope** y en el **diseño interior** de UNA vista; editar y guardar; comprobar que esa vista conserva **sus** desconocidos+versión y otra vista los **suyos** (distintos) — sin mezclar ni degradar. |
| 4 | Vista nueva durante edición (Insertar) | La vista nueva hereda el **envelope y el diseño interior** del bloque iniciador (metadata+versión), no una versión fresca. |
| 5 | Duplicación (RACKDUPLICAR) | Copia con **GUID nuevo** y nombre nuevo; desconocidos del envelope sobreviven; original intacto. |
| 6 | Reapertura / round-trip | Reabrir el dibujo; RACKEDITAR reabre el sistema; BOM (RACKBOMTOTAL)/RACKLISTA sin cambios visibles; Xrecord con la **misma clave**; reabre bien. |

**Biblioteca (WPF):**

| # | Escenario | Verificación |
|---|---|---|
| B1 | Cama/larguero legacy | Abrir legacy sin `SchemaVersion`, editar, guardar; valores conocidos conservados y archivo versionado. |
| B2 | Metadata desconocida | Inyectar desconocido en wrapper y payload; abrir, editar, guardar; desconocido + versión no degradada permanecen. |
| B3 | Dinámico/selectivo de biblioteca | Abrir un diseño con metadata desconocida en el wrapper, editar y guardar; el wrapper conserva metadata+versión (Open/Save conservan el proyecto fuente). |
| B4 | Cama DWG → biblioteca | Editar una cama del dibujo (con `FlowBedDocument` de versión menor-superior + desconocido) y guardarla en biblioteca; el archivo conserva `SchemaVersion` y el desconocido del `FlowBedDocument`. |
| B5 | **Dinámico biblioteca → DWG** | Archivo con wrapper interior 2.x futuro-minor + desconocido; abrir desde biblioteca e **insertar** en el dibujo; inspeccionar el `Design` embebido; la versión y el desconocido permanecen. |
| B6 | **Cama biblioteca → DWG** | Archivo con `FlowBedDocument` 1.x futuro-minor + desconocido; abrir desde biblioteca e **insertar**; el `Design` embebido conserva ambos. |

**Protección de esquema interior:**

| # | Escenario | Verificación |
|---|---|---|
| S7 | **MAJOR interior incompatible** | Dos vistas del mismo GUID, una con el `RackProjectDocument` interior de un MAJOR incompatible; **RACKEDITAR NO** debe reescribir **ninguna** vista ni hacer actualización parcial; debe mostrar un **diagnóstico visible** ("...version mas nueva...; No se modifico ningun bloque."). |

## 11. Criterios de aceptación

- `FlowBedDocument` y `LargueroDocument` versionados, planos, con lectura legacy y `SchemaGuard`.
- Campos desconocidos preservados en los cuatro límites (`RackEmbedDocument`, `RackProjectDocument`,
  `FlowBedDocument`, `LargueroDocument`) en una carga/guardado, con test que falla sin el cambio.
- JSON conocido, nombres/casing, enums, GUID, geometría, BOM y validaciones **idénticos**; formatos legacy
  siguen abiertos; la clave del Xrecord no cambia.
- Sin `switch` nuevo por Kind; `SystemRegistry` de I-08 conservado; sin `RackDesignKind`; sin handlers de
  I-10; sin cambios en Draw Services.
- Suite verde + builds UI/Plugin Debug con 0 errores. Implementada ≠ integrada.

## 12. Condiciones para detenerse

- Rama remota de I-11 reclamada por otra sesión, o worktree de I-11 con otra sesión activa.
- La implementación exigiría handlers de I-10 o modificar sus tres archivos, o un conflicto semántico con
  I-10 no resoluble sin incorporar su arquitectura.
- Sería inevitable cambiar Draw Services, geometría, BOM, reglas de negocio, `RackBlockData` o el formato
  físico del Xrecord; o una migración destructiva.
- Un cambio rompería la compatibilidad obligatoria (JSON conocido, nombres/casing, enums, GUID, clave del
  Xrecord, fallback legacy, comportamiento observable).
- Tres intentos fallidos de pruebas/build/CI; CI no disponible para el SHA final; árbol sucio del usuario
  que impide el worktree; falta de permisos/autenticación GitHub; requisito de validación humana.

## 13. Estado versionado y entrega del Pull Request

Ejecución **dirigida** (no por ejecutor nocturno; la automatización está pausada). No se crea
`docs/automation/state/I-11.yml` (precedente I-08/I-10). Identidad del reclamo: `initiative: I-11`,
`branch: architecture/persistencia-uniforme`, `claim_id: a1b01b5c-f8ce-432f-9c08-c0136a49a3ca`. Se abre un
**único** PR **draft** contra `main`, sin auto-merge. **Merge automático prohibido.** `status`/`completed`
no significan integrada.

## 14. Evidencia final

Commits de la rama, archivos, diseño de compatibilidad, formatos legacy cubiertos, límites de preservación,
pruebas y builds ejecutados, PR (número/URL/draft), CI por SHA, solape con I-10 y validación manual
pendiente se registran en el reporte de cierre de la sesión y en el cuerpo de los commits. `main` **no** se
modifica; la integración es operación manual del dueño (rebase final, CI, owner-validation, HANDOFF §8-12 +
ROADMAP, merge `--no-ff`).
