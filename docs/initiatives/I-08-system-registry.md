---
schema: rackcad-initiative/v1
id: I-08
title: Registro de sistemas en Application (SystemRegistry)
type: architecture
status: claimed
branch: architecture/system-registry
base_branch: main
priority:
size: M
depends_on: [I-02]
conflicts_with: [I-10, I-11]
context_packs:
  - architecture-kernel
  - persistence
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-08 — Registro de sistemas en Application (SystemRegistry)

## 1. Objetivo

Introducir un **descriptor de sistema** y un **`SystemRegistry`** en `RackCad.Application` que
centralicen, en un solo lugar, el conocimiento hoy disperso sobre cada `RackSystemKind`. Los
consumidores de Application pasan a consultar el registro:

- `RackProjectStore` (serialización, construcción y validación por tipo),
- `RackDesignValidation` (predicados "usable" por tipo, que el registro invoca),
- `RackDesignLibrary` (mapeo de tipo y etiqueta de biblioteca),

de modo que **mueren los switches por `RackSystemKind` y el enum paralelo `RackDesignKind`**
(hallazgo E1 de la auditoría). **Sin cambio de comportamiento observable**: mismos formatos JSON en
disco, mismos IDs, nombres y etiquetas, mismo fallback legacy y mismas APIs públicas. La equivalencia
se sostiene con los tests de round-trip y por-tipo existentes (`RackProjectStoreTests`,
`RackProjectStorePerKindTests`, `RackDesignLibraryTests`) más los tests de equivalencia del registro.

## 2. Problema

El conocimiento de "qué sistemas existen y cómo se comportan" está repartido hoy en cinco
ramificaciones por tipo y un segundo enum que duplica al canónico:

| Punto | Archivo | Qué decide |
|---|---|---|
| `RackProjectStore.Serialize` (if/else por `project.Kind`) | `Persistence/RackProjectStore.cs` (~33-55) | qué payload se escribe |
| `RackProjectStore.BuildProject` (`switch document.Kind`) | `Persistence/RackProjectStore.cs` (~124-160) | qué `RackProject` se construye |
| `RackProjectStore.ValidateProject` (`switch project.Kind`) | `Persistence/RackProjectStore.cs` (~203-210) | qué predicado `IsUsable*` aplica |
| `RackDesignLibrary.MapKind` (`switch RackSystemKind`) | `Persistence/RackDesignLibrary.cs` (~94-103) | `RackSystemKind` → `RackDesignKind` |
| `RackDesignLibraryEntry.KindLabel` (`switch RackDesignKind`) | `Persistence/RackDesignLibrary.cs` (~41-49) | etiqueta de biblioteca |

El **enum paralelo `RackDesignKind`** (`Cabecera`, `Dinamico`, `Selectivo`, `Cama`, `Larguero`)
duplica a `RackSystemKind` (`Selective`, `PalletFlow`, `SelectiveRack`, `Cama`, `Larguero`). Dar de
alta un `Kind` nuevo obliga hoy a editar todos esos puntos. La meta de Fase 2 del ROADMAP es "alta de
un Kind sin tocar stores/switches".

## 3. Alcance

Limitado a `RackCad.Application` (persistencia y sistemas) y a `RackCad.Domain` **solo** si el
descriptor exige metadatos junto al enum canónico (sin renombrar ni reordenar miembros), más la
**adaptación mínima necesaria del consumidor de biblioteca en la UI**:

- Descriptor de sistema por `RackSystemKind` + `SystemRegistry` en Application (una entrada por Kind:
  nombre persistido, etiqueta de biblioteca, predicado "usable", selector de payload).
- `RackProjectStore` consume el registro en serialización, construcción y validación por tipo.
- `RackDesignValidation` conserva sus criterios "usable" por tipo; el registro los invoca (sin
  cambiar qué considera usable cada Kind).
- `RackDesignLibrary` deriva tipo y etiqueta desde el registro reemplazando **solo el cuerpo** de los
  switches `MapKind` y `KindLabel`. **Enfoque preferido (sin tocar UI)**: se conserva el tipo público
  `RackDesignKind` y las propiedades `RackDesignLibraryEntry.Kind`/`KindLabel` como superficie de
  compatibilidad respaldada por el registro — el enum deja de ser *fuente de verdad* pero sigue siendo
  API pública que la UI consume. Retirar el tipo `RackDesignKind` es un cambio de API pública y
  dispara la condición de §12.
- Adaptación mínima del único consumidor de biblioteca en UI —
  `RackMainMenuWindow.xaml.cs` (rama de carga por tipo) y el binding de etiqueta de
  `RackDesignLibraryWindow`— **solo** si la firma pública del mapeo/etiqueta cambia, preservando el
  texto exacto de las etiquetas.

**Preservar de forma verificable**: los formatos JSON en disco, `RackSystemKind` y sus miembros, IDs,
nombres, etiquetas, el fallback legacy y las APIs públicas de `RackProject`, `RackProjectStore`,
`RackDesignValidation` y `RackDesignLibrary`/`RackDesignLibraryEntry`. Trampas concretas detectadas en
la auditoría, cada una cubierta por round-trip/golden:

- El token `kind` se serializa como el **nombre** del miembro de `RackSystemKind` vía
  `JsonStringEnumConverter` (sin `JsonNamingPolicy`): `Selective`, `PalletFlow`, `SelectiveRack`,
  `Cama`, `Larguero`. Renombrar o reordenar miembros, o añadir una política de nombres, rompe archivos
  existentes en silencio (`SchemaGuard` solo detecta saltos de *major*).
- Versiones de schema: wrapper `2.0` (`RackProjectDocument`), cabecera legacy `1.0`
  (`RackFrameProjectDocument`); **sin `kind` → cabecera**. Preservar esos *majors* y el fallback.
- Propiedades DTO en PascalCase sin `[JsonPropertyName]`; `Cama` (`FlowBedConfiguration`) y `Larguero`
  (`LargueroDesign`) se serializan como **POCOs planos del Domain** (sus nombres de propiedad son
  tokens de disco).
- **Tres juegos de etiquetas distintos** para los mismos Kinds — biblioteca ("Sistema dinámico",
  "Cama de rodamiento", …), sustantivos de error de `ValidateProject` ("el sistema dinámico", "la
  cama", …) y los del Plugin (I-10) — **no** deben unificarse; el descriptor lleva cada variante por
  separado.
- Precedencia del nombre en `RackDesignLibrary.List`: `Header?.Name ?? SelectiveRack?.Name ??
  Larguero?.Name ?? nombre-de-archivo`; `Cama`/`Dinamico` **omiten** a propósito el nombre de su
  payload. Un accesor del registro debe conservar esa asimetría (además del `OrderByDescending`
  por fecha y el salto silencioso de archivos ilegibles).

## 4. Fuera de alcance

Explícitamente NO se toca (pertenece a otras iniciativas o excede E1):

- **Handlers y switches del Plugin** que despachan por el discriminador string de `RackEmbedDocument`:
  `RackMenuCommands` (RACKEDITAR), `RackInventarioCommands.BomTotal` (RACKBOMTOTAL),
  `RackEnvelopeRestamp` (restamp) y RACKLAYOUT — pertenecen a **I-10** (`architecture/kind-handlers`).
- **Todos los `*DrawService`**, servicios de colocación de bloques, acceso a catálogo y las llamadas a
  `Regen` — pertenecen a **I-16** (`refactor/draw-services`).
- **`RackEmbedDocument` y sus discriminadores string** (`"selective"`, `"dynamic"`, `"cabecera"`,
  `"cama"`, y las vistas `"frontal"`/`"lateral"`/`"planta"`): son el vocabulario de round-trip del
  Plugin —un **tercer** vocabulario, distinto de `RackSystemKind` y de `RackDesignKind`— y quedan
  para I-10. No se unifican con el registro en esta iniciativa. (Nota: `Larguero` **no** tiene
  discriminador de embed — no se dibuja bloque; no asumir que todo Kind tiene handler de embed.)
- **`src/RackCad.Application/Persistence/RackListBuilder.cs`**: aunque vive en Application/Persistence,
  su `KindLabel` conmuta sobre los **strings del embed** (`RackEmbedDocument.Kind`), no sobre
  `RackSystemKind`; pertenece al mundo embed/lista-de-racks (I-10) y **no** se absorbe en el registro.
- **Rediseño de UI** más allá de la adaptación mínima del consumidor de biblioteca.
- **Cambios de geometría, BOM o catálogos.**
- **`docs/HANDOFF.md` y `docs/ROADMAP.md`**: no se editan antes de la integración (se actualizan como
  último commit de la rama en la sesión de integración, WORKFLOW §4.5.4).
- **Renombrar o reordenar `RackSystemKind`** (sus nombres se serializan a JSON).

## 5. Contexto requerido

- **Global**: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/AUTOMATION_PLAN.md` y este
  contrato.
- **Context Packs**: [`architecture-kernel`](../context-packs/architecture-kernel.md) (registros,
  contratos compartidos, capas) y [`persistence`](../context-packs/persistence.md) (DTO, stores,
  identidad, round-trip, legacy).
- **ADRs**: índice [`docs/adr/README.md`](../adr/README.md); ADR-0002 (secuencia dinámico-modular,
  aceptado con opción A) para el tratamiento de `PalletFlow`.
- **Código a leer antes de editar**:
  - `src/RackCad.Domain/Systems/RackSystemKind.cs` (enum canónico);
  - `src/RackCad.Application/Persistence/`: `RackProjectStore.cs`, `RackProject.cs`,
    `RackProjectDocument.cs`, `RackDesignValidation.cs`, `RackDesignLibrary.cs`, `SchemaGuard.cs`;
  - `tests/RackCad.Tests/`: `RackProjectStoreTests.cs`, `RackProjectStorePerKindTests.cs`,
    `RackDesignLibraryTests.cs`;
  - **solo lectura, para respetar el límite**: `Persistence/RackEmbedDocument.cs` y los consumidores
    de Plugin/UI citados en §4.

## 6. Dependencias

- **Depende de**: I-02 (integrada 2026-07-17) — el dinámico modular ya está en `main`, así que el
  descriptor cubre `PalletFlow` sobre el código real.
- **Conflictos que deben permanecer inactivos**: I-10 (`architecture/kind-handlers`) e I-11
  (`architecture/persistencia-uniforme`). Al reclamar, ninguna tiene rama remota (verificado:
  no existen `origin/architecture/kind-handlers` ni `origin/architecture/persistencia-uniforme`).
- No requiere decisiones ni entradas del dueño previas (`requires_owner_decision: false`).

## 7. Archivos esperados

**Crear**:

- El descriptor de sistema + `SystemRegistry` en `src/RackCad.Application/` (subcarpeta `Systems/` o
  `Persistence/`; nombre definitivo a fijar en la F1 de esta iniciativa).
- Tests del registro en `tests/RackCad.Tests/`.

**Modificar**:

- `src/RackCad.Application/Persistence/RackProjectStore.cs`, `RackDesignLibrary.cs`,
  `RackDesignValidation.cs` (según el diseño del registro).
- `src/RackCad.Domain/Systems/RackSystemKind.cs` solo si el descriptor requiere metadatos junto al
  enum (sin renombrar miembros).
- `src/RackCad.UI/RackMainMenuWindow.xaml.cs` y `RackDesignLibraryWindow.xaml`(`.cs`) — adaptación
  mínima del consumidor de biblioteca.

Una **desviación material** respecto a estos archivos —tocar un `*DrawService`, `RackEmbedDocument`,
un handler del Plugin, catálogos o geometría— obliga a **detenerse** (§12).

## 8. Fases

> Fases **de la iniciativa** (implementación futura). Esta sesión ejecuta únicamente la F0.

- **F0 (esta sesión)**: auditoría de estado + reclamo atómico + contrato publicado. **Sin código.**
- **F1**: descriptor + `SystemRegistry` con los cinco Kinds y sus metadatos (nombre persistido,
  etiqueta, predicado "usable", selector de payload) + tests unitarios del registro. Sin tocar
  consumidores todavía.
- **F2**: `RackProjectStore` (Serialize/BuildProject/ValidateProject) consume el registro; round-trip
  y por-tipo verdes **sin cambios en la salida JSON**.
- **F3**: `RackDesignLibrary` deriva tipo/etiqueta del registro; se retira `RackDesignKind` como
  fuente duplicada; `RackDesignLibraryTests` verde.
- **F4**: adaptación mínima del consumidor de biblioteca en UI; suite completa verde; build UI Debug
  con 0 errores.
- **F5**: cierre de la sesión de implementación (push de rama). La integración es sesión aparte,
  serializada y autorizada por el dueño (WORKFLOW §4.5).

Cada fase termina con evidencia revisable (diff mecánico + tests verdes citados en el cuerpo del
commit).

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` **verde** (toda la suite, no solo la nueva).
- Mantener verdes y ampliar `RackProjectStoreTests`, `RackProjectStorePerKindTests` y
  `RackDesignLibraryTests`; **añadir tests de equivalencia del registro** (mismo JSON antes/después
  por cada Kind — patrón golden ya usado en el repo, "ARRAY == plano").
- Build Debug de `src/RackCad.UI/RackCad.UI.csproj` con **0 errores** (la adaptación del consumidor de
  biblioteca lo exige). **No** requiere build de Plugin ni AutoCAD (`requires_plugin_build: false`,
  `requires_autocad: false`).
- CI verde sobre los commits de la rama (`requires_ci: true`).

## 10. Validación manual

- **No requiere AutoCAD** (I-08 no cambia el dibujo; sin ✋ en el ROADMAP). La equivalencia de
  persistencia queda cubierta por los round-trip/golden automatizados.
- `requires_owner_validation: true`: por tratarse de **persistencia (área caliente)**, antes de
  integrar el dueño confirma la revisión del diff del registro y de la equivalencia JSON por Kind.
- Checklist sugerido del dueño (opcional, no bloquea la implementación): (1) abrir desde la biblioteca
  un diseño guardado de cada Kind y verificar que carga con su etiqueta correcta; (2) confirmar que un
  archivo legacy **sin `kind`** sigue cargando como cabecera.

## 11. Criterios de aceptación

- Alta conceptual de un `Kind` nuevo **no** exige editar `RackProjectStore` ni `RackDesignLibrary`
  (basta registrar el descriptor) — demostrado con un test.
- `RackDesignKind` eliminado como fuente de verdad duplicada (o reducido a proyección derivada del
  registro), **sin** cambiar etiquetas ni la API pública que observa la UI.
- JSON en disco **idéntico** por cada Kind (round-trip golden), incluido el caso legacy sin `kind`.
- Suite completa verde + build UI Debug con 0 errores.
- Implementada ≠ integrada: la integración sigue siendo manual y autorizada por el dueño.

## 12. Condiciones para detenerse

- Aparece rama remota de **I-10 o I-11** (conflicto activo).
- El registro exigiría tocar `RackEmbedDocument`, un handler del Plugin, un `*DrawService`, catálogos
  o geometría → **excede alcance**.
- Sería inevitable un cambio de **formato JSON**, de nombre de miembro de `RackSystemKind`, de ID o de
  **etiqueta** → detenerse (rompe la preservación exigida).
- Retirar el tipo público `RackDesignKind` (lo consume la UI) o cambiar cualquier otra API pública de
  §3 → cambio de API pública fuera del enfoque preferido; detenerse para decisión del dueño.
- Endurecer los predicados "usable" (p. ej. exigir `PalletDepth` en la cama o longitud en el
  larguero) → cambia comportamiento; los criterios laxos son intencionales, no se tocan.
- Árbol sucio, operación Git en curso, o CI no diagnosticable con seguridad.
- Cualquier decisión reservada al dueño o cualquier ampliación lateral de alcance.

## 13. Estado versionado y entrega del Pull Request

Estado canónico del ejecutor: `docs/automation/state/I-08.yml` (se crea al **iniciar la
implementación**; no en esta fase de contrato). Campos inmutables copiados del reclamo:
`initiative: I-08`, `branch: architecture/system-registry`,
`claim_id: 6c4ec565-5ce7-4ff9-88a0-38b007cedcf2`. `state` esperado al abrir implementación:
`claimed` → `implementing`. No hay Pull Request abierto conocido; si se abre, será **draft** contra
`main`, uno solo, sin auto-merge. La incapacidad de actualizar el PR no bloquea commit, push ni el
estado versionado. **Merge automático prohibido.**

## 14. Evidencia final

De **esta** sesión (F0):

- Commits en la rama: reclamo vacío `24cfc22` + el commit de este contrato.
- Push: rama `architecture/system-registry` publicada (primer push aceptado = reclamo) + push del
  contrato. Sin `--force`.
- Sin cambios en `src/`, `tests/`, `assets/`, `deploy/` ni `.github/`.
- `main` intacta: `origin/main` = `08491523233ae5f483f904a025bbed0c2845e3a9`; CI verde sobre ese SHA.
- Claim-Id: `6c4ec565-5ce7-4ff9-88a0-38b007cedcf2`.
- Worktree: `D:\Documentos\Codex\Calculadora de racks-I-08-system-registry`.
- Implementación **NO iniciada**; sin merge ni auto-merge.
