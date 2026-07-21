---
schema: rackcad-initiative/v1
id: I-08
title: Registro de sistemas en Application (SystemRegistry)
type: architecture
status: completed
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

de modo que **mueren los switches por `RackSystemKind`** y **el enum paralelo `RackDesignKind` se
elimina por completo** (hallazgo E1 de la auditoría); `RackDesignKind` **no** se conserva como capa de
compatibilidad interna. **Sin cambio de comportamiento observable**: la compatibilidad obligatoria se
aplica a JSON persistido, `RackSystemKind` y sus nombres de enum, las propiedades del wrapper, IDs,
**etiquetas visibles**, fallback legacy y comportamiento observable. `RackDesignLibraryEntry` y
`RackMainMenuWindow` se migran mínimamente a `RackSystemKind` o al descriptor canónico. La equivalencia
se sostiene con una **caracterización previa** (F1) más los tests de round-trip y por-tipo existentes
(`RackProjectStoreTests`, `RackProjectStorePerKindTests`, `RackDesignLibraryTests`) y los del registro.

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
- `RackDesignLibrary` deriva tipo y etiqueta desde el registro/descriptor. **`RackDesignLibrary.MapKind`
  y el switch sobre `RackDesignKind` desaparecen**, y **el enum `RackDesignKind` se elimina por completo
  del código** (no queda como enum ni como capa de compatibilidad interna). `RackDesignLibraryEntry.Kind`
  se migra a `RackSystemKind` (o al identificador canónico del descriptor) y su etiqueta pasa a ser una
  consulta al descriptor que **devuelve exactamente** las mismas cadenas visibles.
- Migración **mínima** de los consumidores del enum eliminado — `RackDesignLibraryEntry` y
  `RackMainMenuWindow.xaml.cs` (rama de carga por tipo) — a `RackSystemKind`/descriptor, **sin
  rediseñar la UI y sin implementar un `EditorModuleRegistry`** ni ningún shell de editor.
  `RackDesignLibraryWindow` mantiene el binding a su propiedad de etiqueta (mismo nombre de propiedad,
  mismas cadenas).

**Compatibilidad obligatoria (preservar de forma verificable)**: el JSON persistido, `RackSystemKind`
y sus nombres de enum, las propiedades del wrapper (`RackProjectDocument`), los IDs, las **etiquetas
visibles**, el fallback legacy y el comportamiento observable de `RackProject`, `RackProjectStore`,
`RackDesignValidation` y `RackDesignLibrary`. Las etiquetas visibles se conservan **exactamente**:
`Cabecera`, `Sistema dinámico`, `Selectivo`, `Cama de rodamiento`, `Larguero`. **`RackDesignKind` no
forma parte de esta lista: se elimina** (su tipo desaparece de la API; `RackDesignLibraryEntry.Kind`
se migra a `RackSystemKind`/descriptor). Trampas concretas detectadas en la auditoría, cada una
cubierta por caracterización/round-trip/golden:

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
  Plugin —un **tercer** vocabulario, distinto de `RackSystemKind` (y del `RackDesignKind` que I-08
  elimina)— y quedan para I-10. No se unifican con el registro en esta iniciativa. (Nota: `Larguero`
  **no** tiene discriminador de embed — no se dibuja bloque; no asumir que todo Kind tiene handler de
  embed.)
- **`src/RackCad.Application/Persistence/RackListBuilder.cs`**: aunque vive en Application/Persistence,
  su `KindLabel` conmuta sobre los **strings del embed** (`RackEmbedDocument.Kind`), no sobre
  `RackSystemKind`; pertenece al mundo embed/lista-de-racks (I-10) y **no** se absorbe en el registro.
- **Rediseño de UI** más allá de la migración mínima del consumidor de biblioteca.
- **Implementar `EditorModuleRegistry`, `IRackEditorModule` o cualquier shell/registro de módulos de
  editor** — es territorio de I-15, fuera de I-08.
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

> Fases **de la iniciativa**. Todas ejecutadas en la rama (evidencia en §14); la integración es
> sesión aparte del dueño.

- **F0 (esta sesión)**: auditoría de estado + reclamo atómico + contrato publicado. **Sin código.**
- **F1 — Caracterización ANTES del refactor** (red de seguridad): tests golden/round-trip que fijan el
  comportamiento **actual** y deben pasar **antes** de tocar código de producción. Cubren **como
  mínimo**:
  1. serialización exacta de los cinco nombres de `RackSystemKind` (`Selective`, `PalletFlow`,
     `SelectiveRack`, `Cama`, `Larguero`);
  2. schema y nombres de payload existentes (wrapper `2.0`, cabecera legacy `1.0`);
  3. lectura legacy **sin `kind`** (carga como cabecera);
  4. `kind` **sin payload** (error claro);
  5. `kind` **desconocido o inválido**;
  6. reconstrucción física de cabeceras (`RefreshPhysicalModel`);
  7. reglas actuales de validación por tipo (`IsUsable*`, incluidos los criterios laxos);
  8. etiquetas y **precedencia de nombres** de `RackDesignLibrary`;
  9. omisión **individual** de archivos ilegibles (no aborta el listado).
- **F1.1 — Caracterización previa complementaria**: comportamiento actual de un `Kind` **numérico no
  definido** (p. ej. `999`): con cabecera válida → ruta histórica de cabecera (`Selective`); sin
  payload usable → error degenerado de cabecera; re-save → `"Kind":"Selective"`. Distinto del string
  desconocido (`"NoSuchKind"` → "no es un JSON valido"). El registro debe preservar esta ruta (usar
  `TryGet`, nunca `Get((RackSystemKind)999)`).
- **F2**: descriptor + `SystemRegistry` con los cinco Kinds y sus metadatos (nombre persistido,
  etiqueta, predicado "usable", selector de payload) + tests unitarios del registro. Sin tocar
  consumidores todavía.
- **F3**: `RackProjectStore` (Serialize/BuildProject/ValidateProject) consume el registro;
  caracterización + round-trip + por-tipo verdes **sin cambios en la salida JSON**.
- **F4**: `RackDesignLibrary` deriva tipo/etiqueta del registro; **se elimina `RackDesignKind`**;
  migración mínima de `RackDesignLibraryEntry` y `RackMainMenuWindow` a `RackSystemKind`/descriptor
  (etiquetas visibles intactas); `RackDesignLibraryTests` verde; build UI Debug con 0 errores.
- **F5**: auditoría final de la implementación completa, higiene, revisión de la superficie de API y
  cierre del contrato. La integración es sesión aparte, serializada y autorizada por el dueño
  (WORKFLOW §4.5).

Cada fase termina con evidencia revisable (diff mecánico + tests verdes citados en el cuerpo del
commit).

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` **verde** (toda la suite, no solo la nueva).
- **La caracterización de F1 debe pasar ANTES del refactor** y permanecer verde después (es la red que
  demuestra la equivalencia). Mantener verdes y ampliar `RackProjectStoreTests`,
  `RackProjectStorePerKindTests` y `RackDesignLibraryTests`; **añadir tests de equivalencia del
  registro** (mismo JSON antes/después por cada Kind — patrón golden ya usado en el repo,
  "ARRAY == plano").
- Build Debug de `src/RackCad.UI/RackCad.UI.csproj` con **0 errores** (la adaptación del consumidor de
  biblioteca lo exige). **No** requiere build de Plugin ni AutoCAD (`requires_plugin_build: false`,
  `requires_autocad: false`).
- CI verde sobre los commits de la rama (`requires_ci: true`).

## 10. Validación manual

Dos conceptos distintos:

- **AutoCAD: NO requerido** (`requires_autocad: false`; sin ✋ en el ROADMAP). I-08 no cambia dibujo,
  BOM ni edición; la equivalencia de persistencia y de etiquetas queda cubierta por los round-trip/
  golden y los tests del registro. **No se declara AutoCAD validado.**
- **Owner-validation: SÍ requerida** (`requires_owner_validation: true`). Por tratarse de
  **persistencia (área caliente)** y de un cambio de UI, antes de integrar el dueño revisa el diff y,
  como comprobación de confianza (no un gate de AutoCAD), recorre este checklist breve, limitado al
  comportamiento afectado por I-08:
  1. abrir la biblioteca de diseños;
  2. confirmar las cinco etiquetas exactas (`Cabecera`, `Sistema dinámico`, `Selectivo`,
     `Cama de rodamiento`, `Larguero`);
  3. abrir un diseño guardado de cada tipo disponible;
  4. confirmar que se abre el mismo editor correcto que antes;
  5. abrir una cabecera legacy **sin `Kind`** y confirmar que carga como cabecera;
  6. confirmar que no cambió dibujo, BOM ni la edición posterior.

## 11. Criterios de aceptación

- Alta conceptual de un `Kind` nuevo **no** exige editar `RackProjectStore` ni `RackDesignLibrary`
  (basta registrar el descriptor) — demostrado con un test.
- **`RackDesignKind` eliminado por completo del código** (no queda como enum ni como capa de
  compatibilidad); `RackDesignLibraryEntry` y `RackMainMenuWindow` migrados a `RackSystemKind`/descriptor,
  **conservando exactamente** las etiquetas visibles (`Cabecera`, `Sistema dinámico`, `Selectivo`,
  `Cama de rodamiento`, `Larguero`), sin rediseño de UI ni `EditorModuleRegistry`.
- La **caracterización (F1) pasa antes del refactor** y sigue verde después.
- JSON en disco **idéntico** por cada Kind (round-trip golden), incluido el caso legacy sin `kind`.
- Suite completa verde + build UI Debug con 0 errores.
- Implementada ≠ integrada: la integración sigue siendo manual y autorizada por el dueño.

## 12. Condiciones para detenerse

- Aparece rama remota de **I-10 o I-11** (conflicto activo).
- El registro exigiría tocar `RackEmbedDocument`, un handler del Plugin, un `*DrawService`, catálogos
  o geometría → **excede alcance**.
- Sería inevitable un cambio de **formato JSON**, de nombre de miembro de `RackSystemKind`, de ID o de
  **etiqueta** → detenerse (rompe la preservación exigida).
- Un cambio que rompa la **compatibilidad obligatoria** (JSON persistido, nombres de `RackSystemKind`,
  propiedades del wrapper, IDs, **etiquetas visibles**, fallback legacy, comportamiento observable), o
  que exija **rediseñar la UI** o **implementar `EditorModuleRegistry`**/un shell de editor → detenerse.
  (Eliminar `RackDesignKind` es **requisito** de I-08, no motivo de parada; migrar sus consumidores a
  `RackSystemKind`/descriptor es parte del alcance.)
- Endurecer los predicados "usable" (p. ej. exigir `PalletDepth` en la cama o longitud en el
  larguero) → cambia comportamiento; los criterios laxos son intencionales, no se tocan.
- Árbol sucio, operación Git en curso, o CI no diagnosticable con seguridad.
- Cualquier decisión reservada al dueño o cualquier ampliación lateral de alcance.

## 13. Estado versionado y entrega del Pull Request

Ejecución **dirigida** (no por el ejecutor nocturno): no se creó `docs/automation/state/I-08.yml` ni se
abrió Pull Request. Identidad del reclamo: `initiative: I-08`, `branch: architecture/system-registry`,
`claim_id: 6c4ec565-5ce7-4ff9-88a0-38b007cedcf2`. Si en el cierre se adopta el flujo de automatización,
el estado versionado se crea entonces con `state: completed`; si se abre PR, será **draft** contra
`main`, uno solo, sin auto-merge. **Merge automático prohibido.**

## 14. Evidencia final

**Implementación completa (F0–F5), aún sin integrar.** El trabajo vive solo en la rama
`architecture/system-registry`; `main` **no fue modificada**. Claim-Id
`6c4ec565-5ce7-4ff9-88a0-38b007cedcf2`. Worktree
`D:\Documentos\Codex\Calculadora de racks-I-08-system-registry`.

- **F0–F1 / F1.1**: reclamo atómico, contrato (y su corrección) y la caracterización previa que congela
  el comportamiento actual — wire format PascalCase (`SchemaVersion`, `Kind`, `Header`, `DynamicSystem`,
  `SelectiveRack`, `FlowBed`, `Larguero`), schema `2.0`, cinco nombres de enum, nulos escritos, legacy
  sin `Kind`, reconstrucción física, `kind` sin payload, degenerado, versión futura, string desconocido,
  `Kind: 999`, reglas laxas de cama/larguero, etiquetas/precedencia/orden/tolerancia.
- **F2**: `SystemDescriptor` + `SystemRegistry` (puro, sin reflexión ni escaneo) con los cinco Kinds en
  orden de declaración y sus etiquetas verbatim; `SystemRegistry.Default` como única fuente.
- **F3**: `RackProjectStore` delega escritura/construcción/validación en el descriptor del registro;
  mueren el `if/else` de `Serialize` y los `switch` de `BuildProject`/`ValidateProject`; un `Kind`
  desconocido conserva el fallback histórico a cabecera (`TryGet`, nunca `Get((RackSystemKind)999)`);
  `RequirePayload` y el sustantivo de validación preservados.
- **F4**: `RackDesignLibrary` deriva Kind (`RackSystemKind`) y etiqueta del descriptor; **`RackDesignKind`
  y `MapKind` eliminados por completo** (búsqueda global = cero); `RackMainMenuWindow` compara
  `RackSystemKind`; un `Kind` no registrado se omite (tolerante). Sin `EditorModuleRegistry` ni shell.
- **F5**: auditoría funcional y estática (el registro tiene los cinco Kinds; sin `switch`/cadena por
  `RackSystemKind` en el store ni en la biblioteca; sin segundo registro manual; Plugin,
  `RackEmbedDocument`, `RackListBuilder`, DrawServices, geometría, BOM y catálogos fuera del cambio),
  revisión de la superficie de API (sin cambios de visibilidad: reducir los seams inyectables exigiría
  un `InternalsVisibleTo` inexistente) y este cierre del contrato.

**Equivalencia verificada (método):** suite `RackCad.Tests`, build Debug de UI (sin advertencias
propias) y build Debug del Plugin (sin errores; solo los `MSB3277` conocidos de AutoCAD) y CI de la
rama (Tests + Build UI + Build Plugin without AutoCAD) en **verde** sobre el commit final; toda la
caracterización F1–F1.1 permanece verde tras el refactor; el diff acota los cambios de producto a
`RackProjectStore`, `RackDesignLibrary`, `RackMainMenuWindow` y los tipos nuevos del registro.

**Pendiente (fuera de esta fase):** la integración es operación manual del dueño — rebase final sobre
`main`, CI de `main`, owner-validation (§10; AutoCAD no requerido), actualización de `HANDOFF.md`
§8–12 y `ROADMAP.md`, y merge `--no-ff`. `status: completed` **no** significa integrada. Los conteos de
pruebas y los hashes canónicos viven en `docs/HANDOFF.md` (§12), no aquí.
