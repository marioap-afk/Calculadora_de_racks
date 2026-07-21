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
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: false
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
  `RackProjectStore.cs` (acarreo del documento fuente; preservar `ExtensionData` y versión sin degradar),
  `RackProject.cs` (campo de documento fuente + `WithSourceMetadataFrom` para el sidecar de UI),
  `FlowBedConfigurationStore.cs` (escritura versionada + ruta documento), `RackEmbedDocument.cs`
  (`CurrentSchemaVersion` + `JsonExtensionData` + guarda tolerante en `RackEmbedStore`), `SchemaGuard.cs`
  (delega la legibilidad en `SchemaVersionPolicy`).
- `src/RackCad.Application/Systems/SystemRegistry.Default.cs` (mapeo cama/larguero vía documentos + guarda).
- `src/RackCad.Plugin/`: `RackCamaCommands.cs`, `RackSelectivoCommands.cs`, `RackDinamicoCommands.cs`,
  `RackCabeceraCommands.cs` — cada `Build*Payload` compone el envelope vía `RackEmbedComposer`; el redibujo
  multivista conserva la metadata **del Embed de cada bloque**; una vista nueva insertada durante la edición
  hereda el envelope iniciador. **No** tocan los tres archivos de I-10.
- `src/RackCad.UI/`: `RackFlowBedWindow.xaml.cs`, `RackLargueroWindow.xaml.cs` (campo privado + overload que
  recibe el `RackProject` fuente; el guardado usa `WithSourceMetadataFrom`), `RackMainMenuWindow.xaml.cs`
  (transporta el proyecto fuente al editor). Sin rediseño de UI.

**Frontera de la biblioteca dinámico/cabecera**: la edición del dinámico reconstruye un `RackProjectDocument`
(vía `RackProjectStore`) y adopta el mismo `WithSourceMetadataFrom` (probado a nivel de store). La cabecera
guarda por `RackFrameProjectStore`/`RackFrameProjectDocument` (otro store, sin `RackProjectDocument`), así que
la condición "reconstruye `RackProjectDocument`" **no** se cumple en su ruta de biblioteca UI; el mecanismo se
prueba a nivel de store para su wrapper. Cablear esas dos ventanas de UI queda para cuando su guardado se
toque (una línea `WithSourceMetadataFrom` en dinámico; `RackFrameProjectDocument` necesitaría además
`JsonExtensionData` para la cabecera).

**Limitación explícita**: `JsonExtensionData` **no** es preservación recursiva de todos los DTO anidados. Se
preservan los campos desconocidos SOLO en los cuatro límites de I-11 (`RackEmbedDocument`,
`RackProjectDocument`, `FlowBedDocument`, `LargueroDocument`) y la versión de esos documentos. El diseño
interno type-specific del envelope (selectivo/dinámico/cabecera) se re-serializa desde el modelo reabierto y
**no** conserva campos desconocidos anidados propios.

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

- **AutoCAD: NO requerido** (`requires_autocad: false`; sin ✋ en ROADMAP). I-11 **no** cambia
  `RackBlockData` ni el formato físico del Xrecord; solo añade campos aditivos (ignorables) al JSON del
  envelope/payload y preserva desconocidos. No se declara AutoCAD validado.
- **Owner-validation: SÍ** (`requires_owner_validation: true`), por ser persistencia (área caliente) y
  tocar el contenido embebido. Checklist para el dueño (solo preparar; no ejecutar):

  **Biblioteca**:
  1. abrir un archivo legacy de cama sin `SchemaVersion` y uno de larguero legacy; guardarlos de nuevo;
     confirmar que los valores conocidos se conservan;
  2. inyectar un campo desconocido en el wrapper y en el payload (cama/larguero); abrir, editar un campo
     conocido, guardar; confirmar que el campo desconocido permanece.

  **DWG/envelope** (si aplica):
  3. insertar/abrir una cama; usar/inyectar un envelope con propiedad desconocida; editar nombre/config;
     confirmar GUID estable en edición y GUID nuevo al duplicar; confirmar que los campos desconocidos
     sobreviven; confirmar RACKLISTA/RACKEDITAR/BOM sin cambios visibles y que el Xrecord usa la misma
     clave y el rack reabre correctamente.

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
