---
schema: rackcad-initiative/v1
id: I-10
title: Registro de handlers por Kind en el Plugin (KindHandlerRegistry)
type: architecture
status: in-progress
branch: architecture/kind-handlers
base_branch: main
priority:
size: M
depends_on: [I-08, I-09]
conflicts_with: [I-09, I-16]
context_packs:
  - architecture-kernel
  - autocad-plugin
  - delivery-validation
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-10 — Registro de handlers por Kind en el Plugin (KindHandlerRegistry)

## 1. Objetivo

Introducir un contrato `IRackKindHandler` y un **registro explícito** `KindHandlerRegistry` dentro de
`RackCad.Plugin` que centralicen, en un solo lugar del Plugin, el despacho por el discriminador string
del sobre embebido `RackEmbedDocument.Kind`. Los consumidores del Plugin pasan a consultar el registro
en vez de repetir un `switch`/cadena `if` por `Kind`:

- `RackMenuCommands.RackEditar` (RACKEDITAR) — abre el editor correcto por tipo;
- `RackInventarioCommands.BuildRackBom` y `KindLabel` (RACKBOMTOTAL) — reconstruye el BOM por tipo y su
  etiqueta visible;
- `RackEnvelopeRestamp` (restamp de copias independientes, consumido por RACKDUPLICAR y RACKLAYOUT) —
  re-estampa la identidad interior específica del tipo.

Es un refactor **sin cambio de comportamiento observable** salvo **una** excepción autorizada: cuando no
exista handler registrado para un `Kind`, el error debe ser **visible/diagnosticable** (hallazgo E2 de la
auditoría; ARCHITECTURE §7.2: "Un `Kind` desconocido debe producir un error visible, no una omisión
silenciosa"). La equivalencia se sostiene con **caracterización previa** (donde exista una costura pura),
**inventario mecánico** de comandos/alias/mensajes/constantes, la **suite existente completa**, los
**builds Debug** de UI y Plugin y el **CI de rama**, exactamente como en I-09 (el registro vive en el
Plugin, que referencia AutoCAD; el harness `RackCad.Tests` no lo ejecuta).

## 2. Problema

El "qué tipos de rack existen y cómo se editan/inventarían/re-estampan" está repartido hoy en cuatro
ramificaciones por `Kind` dentro del Plugin, cada una repitiendo la lista de los cuatro tipos:

| Punto | Archivo | Qué decide |
|---|---|---|
| `RackMenuCommands.RackEditar` (`switch (embed.Kind)`) | `RackMenuCommands.cs` (~87-104) | qué editor abre RACKEDITAR + error visible por defecto |
| `RackInventarioCommands.BuildRackBom` (`switch (embed.Kind)`) | `RackInventarioCommands.BomTotal.cs` (~119-150) | cómo se reconstruye el BOM por tipo |
| `RackInventarioCommands.KindLabel` (`switch (kind)`) | `RackInventarioCommands.BomTotal.cs` (~158-168) | etiqueta visible del BOM ("Selectivo"/"Dinámico"/"Cabecera"/"Cama") |
| `RackEnvelopeRestamp.RestampDesign` (cadena `if`) | `RackEnvelopeRestamp.cs` (~30-67) | re-estampa la identidad interior (selectivo: Id+Name; cabecera: Header.Name) |

Dar de alta un `Kind` nuevo obliga hoy a editar todos esos puntos. La meta de Fase 2 del ROADMAP es
"alta de un Kind sin tocar stores/switches". La pista B del Plugin se serializa `I-09 → I-16 → I-10`; I-09
e I-16 ya están integradas y dejaron estos despachos intactos y reservados para I-10.

## 3. Alcance

Estrictamente el alcance de I-10 en ROADMAP ("`IRackKindHandler` + registro en el **Plugin**;
RACKEDITAR/RACKBOMTOTAL/RACKLAYOUT/restamp despachan por registro; Kind no registrado = error visible"),
sin ampliaciones laterales:

- **`IRackKindHandler`** en `RackCad.Plugin`: contrato mínimo cuyas operaciones se derivan de los call
  sites reales — `Edit`, `BuildBom`, `BomLabel`, `RestampDesign`, más la clave `Kind` que el handler
  declara. No un contrato genérico sobredimensionado.
- **`KindHandlerRegistry`** en `RackCad.Plugin`: registro explícito, inmutable, **sin reflexión ni
  escaneo de assemblies**, con orden estable y documentado, que rechaza handlers nulos, claves vacías y
  claves duplicadas. Lookup **no throwing** (`TryGet`), con un lookup case-insensitive adicional
  (`TryGetIgnoreCase`) para el único consumidor case-insensitive (restamp).
- **Cuatro handlers explícitos**, fachadas delgadas hacia los métodos/stores/resolvers/builders/Draw
  Services existentes: `selective`, `dynamic`, `cabecera`, `cama`.
- **Un único composition root / registro por defecto** explícito (`KindHandlerRegistry.Default`).
- **Migración de los consumidores**: RACKEDITAR, RACKBOMTOTAL (BuildRackBom + KindLabel) y el restamp
  (`RackEnvelopeRestamp`, consumido transitivamente por RACKDUPLICAR y RACKLAYOUT) despachan por el
  registro. RACKLAYOUT **no tiene switch propio por Kind**: su única dependencia de `Kind` es el restamp
  de copias independientes, que ya delega en `RackEnvelopeRestamp`; migrar el restamp satisface
  "RACKLAYOUT despacha por registro" de forma transitiva. **No** se traslada el planner al handler ni se
  recalcula la huella desde el diseño.
- **Error visible ante handler ausente**: RACKEDITAR conserva la forma exacta del mensaje actual de tipo
  no reconocido (`"\nRackCad: tipo de rack no reconocido (" + embed.Kind + ")."`).
- **Preservar exactamente el comportamiento observable** de los cuatro consumidores (§ "Invariantes").

## 4. Fuera de alcance

Explícitamente NO se toca:

- **`RackListBuilder` (`src/RackCad.Application/Persistence/RackListBuilder.cs`) y RACKLISTA.** Aunque el
  contrato de I-08 § "Fuera de alcance" apunta `RackListBuilder.KindLabel` hacia I-10, su despacho conmuta
  sobre los strings del embed **en la capa Application**. El registro `KindHandlerRegistry` vive en el
  **Plugin**, y **Application nunca puede depender del Plugin** (dirección de dependencias
  Domain←Application←UI←Plugin, ARCHITECTURE §1/§2). El ROADMAP I-10 **no** lista RACKLISTA entre los
  despachos que migran. Reconciliación ROADMAP↔I-08: `RackListBuilder` queda **intacto y fuera de
  alcance**; unificarlo pertenecería a un registro/descriptor de Application (territorio de I-08/I-15,
  no de I-10). Además, sus etiquetas ("Sistema dinámico", "Cama de rodamiento") **difieren** de las del
  BOM del Plugin ("Dinámico", "Cama"): son dos de los "tres juegos de etiquetas distintos" de I-08 y **no
  se unifican**.
- **`RackEmbedDocument` y sus constantes de discriminador** (`selective`/`dynamic`/`cabecera`/`cama`,
  vistas `frontal`/`lateral`/`planta`): son el vocabulario de round-trip; el registro las **consume**, no
  las redefine ni las mueve.
- **`SystemRegistry`, `RackProjectStore`, `RackDesignValidation`, `RackDesignLibrary`** (Application,
  I-08): el registro del Plugin **no** se convierte en su reemplazo ni al revés. `SystemRegistry` sigue en
  Application para persistencia, validación y biblioteca; `KindHandlerRegistry` se ocupa de las
  operaciones AutoCAD/Plugin. Son registros de **capas distintas** con vocabularios distintos
  (`RackSystemKind` vs. string del embed).
- **`Larguero`**: no tiene discriminador de embed ni se dibuja como bloque de rack; **no** tiene handler y
  **no** se registra (no asumir que todo Kind tiene handler).
- **Los `*DrawService`, `ViewBlockDraw`, `BlockPlacement`, `RackCatalogLoader`, `SystemBlockWriter`**
  (I-16, integrada): no se re-generalizan ni se tocan.
- **Geometría, BOM funcional (conteos/recetas), GUID, formatos persistidos, mensajes existentes, aliases,
  comandos, catálogos y compatibilidad legacy**: sin cambios (salvo el error visible autorizado).
- **`RackCad.Domain`, `RackCad.Application` (más allá de leer), `RackCad.UI`, `assets/catalogs`,
  `deploy`.** No se mueve lógica de negocio a Application ni se crea un proyecto de tests del Plugin.
- **Sistemas nuevos, paquetes NuGet, reflexión/assembly scanning.**
- **`docs/HANDOFF.md`, `docs/ROADMAP.md`, `docs/initiatives/README.md`**: solo se tocan en la sesión de
  integración (WORKFLOW §4.5), como último commit de la rama.
- Merge, auto-merge, integración o limpieza fuera de las condiciones de la Fase 7.

## 5. Contexto requerido

- **Global**: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`,
  `docs/ARCHITECTURE.md` (§§1-2 capas/dependencias; §4.1 identidad/round-trip; §4.3 BOM; §7.2 registros),
  `docs/AUTOMATION_PLAN.md` y este contrato.
- **Context Packs**: [`architecture-kernel`](../context-packs/architecture-kernel.md) (registros y
  capas), [`autocad-plugin`](../context-packs/autocad-plugin.md) (comandos, transacciones, Xrecords) y
  [`delivery-validation`](../context-packs/delivery-validation.md) (build, CI, validación).
- **ADRs**: índice [`docs/adr/README.md`](../adr/README.md); ADR-0003 (referencias AutoCAD para CI) para
  el build del Plugin sin AutoCAD.
- **Contratos previos**: [`I-08`](I-08-system-registry.md) (frontera con `SystemRegistry`;
  `RackListBuilder` y `RackEmbedDocument` deferidos), [`I-09`](I-09-refactor-plugin-commands.md) (helpers
  y método de equivalencia mecánica que I-10 reutiliza), [`I-16`](I-16-refactor-draw-services.md)
  (Draw Services, ya no se tocan).
- **Código a leer antes de editar**:
  - `src/RackCad.Application/Persistence/RackEmbedDocument.cs` (constantes de Kind, solo lectura);
  - `src/RackCad.Plugin/RackMenuCommands.cs`, `RackInventarioCommands.BomTotal.cs`,
    `RackInventarioCommands.cs`, `RackEnvelopeRestamp.cs`, `RackDuplicarCommands.cs`,
    `RackLayoutCommands.cs`;
  - los entry points de edición `internal static void Edit{Selective,Dynamic,Cabecera,Cama}(...)` en
    `RackSelectivoCommands.cs`, `RackDinamicoCommands.cs`, `RackCabeceraCommands.cs`, `RackCamaCommands.cs`;
  - solo lectura, para respetar el límite: `RackListBuilder.cs` (Application).

## 6. Dependencias

- **Depende de**: I-08 (integrada 2026-07-21) e I-09 (integrada 2026-07-20), ambas en `main`. La
  serialización de la pista B I-09 → I-16 → I-10 está completa salvo I-10.
- **Conflictos declarados en ROADMAP ("se estorba con")**: I-09 e I-16, ambas **integradas** (sin rama
  remota activa). Al reclamar se verificó que no existe `origin/architecture/kind-handlers` y que I-08,
  I-09 e I-16 están contenidas en `main`. I-11 (`architecture/persistencia-uniforme`) no tiene rama
  remota.
- **Convive con** I-07 (`docs/adr-retroactivos`, worktree hermano): distinta capa (docs), sin archivos
  calientes compartidos.
- Sin entrada del dueño requerida para abrir la iniciativa.

## 7. Archivos esperados

**Crear** (todos en `src/RackCad.Plugin`, salvo el contrato):

- `IRackKindHandler` (contrato mínimo);
- `KindHandlerRegistry` (registro explícito + composition root `Default`);
- cuatro handlers: `SelectiveKindHandler`, `DynamicKindHandler`, `CabeceraKindHandler`, `CamaKindHandler`;
- probablemente bajo una carpeta clara (p. ej. `src/RackCad.Plugin/KindHandlers/`; forma final a fijar en
  la implementación);
- este contrato: `docs/initiatives/I-10-kind-handlers.md`.

**Modificar** (todos en `src/RackCad.Plugin`):

- `RackMenuCommands.cs` (RACKEDITAR consulta el registro; conserva el mensaje de tipo no reconocido);
- `RackInventarioCommands.BomTotal.cs` (BuildRackBom + KindLabel consultan el registro);
- `RackEnvelopeRestamp.cs` (restamp consulta el registro con lookup case-insensitive).

`docs/initiatives/README.md` se enlaza en la sesión de integración (archivo caliente afín a HANDOFF/
ROADMAP). **No** se esperan cambios bajo `src/RackCad.Domain`, `src/RackCad.Application`,
`src/RackCad.UI`, `assets/catalogs`, `deploy`, ni en `docs/HANDOFF.md`/`docs/ROADMAP.md` fuera de la
integración. Una **desviación material** —tocar un `*DrawService`, `RackEmbedDocument`, `RackListBuilder`,
`SystemRegistry`, catálogos o geometría— obliga a **detenerse** (§12).

## 8. Fases

- **F0. Reclamo atómico**: rama + worktree desde `origin/main` (`c5a4082`), commit vacío de reclamo
  (`Claim-Id`) y primer push aceptado sin force; base verificada (I-08/I-09/I-16 en `main`; sin estorbo
  activo).
- **F1. Contrato + caracterización previa**: este contrato publicado (primer commit no vacío, solo el
  contrato) + caracterización de los invariantes (§ "Invariantes") por inventario mecánico; tests de
  caracterización solo donde exista una costura pura razonable en Application (`RackCad.Tests`).
- **F2. Contratos y registro** (`IRackKindHandler`, `KindHandlerRegistry`, cuatro handlers, `Default`),
  sin migrar consumidores todavía; build verde.
- **F3. Migración de consumidores** por etapas revisables: RACKEDITAR → RACKBOMTOTAL → restamp; verificar
  RACKDUPLICAR y RACKLAYOUT (consumidores del restamp). Eliminar solo los switches sustituidos.
- **F4. Verificación completa**: suite + builds Debug (solución/UI/Plugin) + inventario mecánico
  (comandos, alias, mensajes, constantes, GUID, sin switch por Kind restante en alcance, sin AutoCAD
  fuera del Plugin, sin dependencias nuevas).
- **F5. CI por SHA exacto** verde (Tests, Build UI, Build Plugin without AutoCAD) + cierre del contrato.

Cada fase cierra con commit + push de la rama (respaldo, WORKFLOW §4.3) y evidencia revisable.

## 8.1 Invariantes a preservar (caracterización)

- **RACKEDITAR**: los cuatro `case` en orden actual (selective, dynamic, cabecera, cama); prompts,
  cancelaciones y mensajes existentes; el error visible por defecto **verbatim**
  (`"\nRackCad: tipo de rack no reconocido (" + embed.Kind + ")."`); el chequeo previo de
  `embed == null || Design` vacío; comparación **case-sensitive** (semántica del `switch` C#, ordinal).
- **RACKBOMTOTAL**: agrupamiento por GUID; conteo de copias = MAX de referencias; consolidación
  (`ConsolidatedBomBuilder`); el `continue` para `Kind` nulo/vacío; el **skip best-effort** cuando el BOM
  no se puede reconstruir (payload ilegible → `null`) **y** cuando el `Kind` no tiene handler (foráneo →
  `null`) — ambos conservan el skip actual, no se convierten en error; el `try/catch` que devuelve `null`;
  las etiquetas visibles del BOM **verbatim** ("Selectivo"/"Dinámico"/"Cabecera"/"Cama") y el
  `default: return kind ?? string.Empty` de `KindLabel`; comparación **case-sensitive** (switch).
- **Restamp** (`RackEnvelopeRestamp`): `RestampEnvelope` genera **un solo** GUID por copia y fija
  `Name`; la identidad interior se re-estampa solo para selectivo (Id+Name) y cabecera (Header.Name);
  dinámico y cama **no** tienen identidad interior (no-op); el `if (string.IsNullOrEmpty(designJson))`
  temprano; el **fallback best-effort** (diseño ilegible → se devuelve intacto); comparación
  **case-insensitive** (`OrdinalIgnoreCase`, preservada por `TryGetIgnoreCase`).
- **RACKDUPLICAR / RACKLAYOUT**: consumen `RestampEnvelope`; una sola generación de GUID por copia
  independiente; RACKLAYOUT conserva la búsqueda por GUID de la planta, extents reales, rotación,
  `Scale3d`, offsets, labels, enlazado/independiente, mensajes y su único `Regen`.
- **General**: mismos `[CommandMethod]` y aliases; cero comandos duplicados; sin cambios en catálogos,
  geometría, BOM funcional, GUID ni formatos persistidos; ninguna referencia AutoCAD fuera del Plugin;
  ninguna dependencia nueva; ningún `Regen` nuevo.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
git diff origin/main --check
```

- La suite existente completa permanece **verde** en cada fase (Domain + Application, sin AutoCAD).
- **El registro no es testeable desde `tests/RackCad.Tests`**: vive en el Plugin, que referencia AutoCAD
  (`IRackKindHandler.Edit` toma `Document`/`ObjectId`), y `RackCad.Tests` solo referencia Domain +
  Application. **No** se crea un proyecto de tests del Plugin ni se mueve lógica a Application para
  testearla (deformaría el diseño). La verificación del registro es por **inventario mecánico + build +
  revisión de código**, como I-09/I-16. Se comprueba por código que las cuatro constantes embebidas están
  representadas y que `Larguero` no se registra.
- Build Debug de UI y Plugin con **0 errores**; los `MSB3277` conocidos de las referencias de AutoCAD no
  cuentan. Si AutoCAD bloquea el DLL, se compila a un output temporal y se reporta el bloqueo.
- CI de rama **verde** (Tests, Build UI, Build Plugin without AutoCAD) verificado por **SHA exacto**.

## 10. Validación manual

- **AutoCAD: NO requerido** (`requires_autocad: false`; ROADMAP **no** marca I-10 con ✋). I-10 no cambia
  la superficie de dibujo (los handlers son fachadas delgadas hacia el código de edición/BOM/restamp
  **sin cambiar**), ni geometría, BOM funcional, GUID ni persistencia. Como I-09 (refactor del Plugin que
  preserva comportamiento), la equivalencia se sostiene con inventario mecánico, builds y CI. **No se
  declara AutoCAD validado.**
- **Owner-validation: NO requerida** (`requires_owner_validation: false`), por analogía directa con I-09:
  no toca persistencia (área caliente) ni UI; el único cambio observable nuevo (error visible ante handler
  ausente) no puede dispararse con datos reales porque los cuatro Kinds embebidos siempre están
  registrados. El muestreo humano en AutoCAD queda disponible al integrar (ROADMAP principio 4), pero no
  es gate de la implementación.

## 11. Criterios de aceptación

- `IRackKindHandler` + `KindHandlerRegistry` existen en `RackCad.Plugin`; el registro es explícito,
  inmutable, sin reflexión, con orden estable, y rechaza handlers nulos/claves vacías/duplicadas.
- Cuatro handlers (`selective`, `dynamic`, `cabecera`, `cama`) registrados en `Default`; `Larguero` no
  registrado (verificado por código).
- RACKEDITAR, RACKBOMTOTAL (BuildRackBom + KindLabel) y el restamp despachan por el registro; los switches
  sustituidos se eliminan; no queda ningún `switch`/cadena por `Kind` del embed dentro del alcance
  acordado (RACKLISTA/`RackListBuilder` quedan intactos por capas — §4).
- Handler ausente produce un **error visible** en RACKEDITAR, conservando la forma del mensaje actual.
- **Todas las invariantes de §8.1 preservadas**, verificadas por inventario mecánico: comandos, alias,
  prompts, keywords, mensajes, constantes de Kind/View, etiquetas de BOM, puntos de generación de GUID,
  case-sensitivity por consumidor, skips best-effort, y el patrón de `Regen`.
- Suite completa + builds Debug de UI y Plugin sin fallos ni errores/advertencias propias; CI de rama
  verde por SHA exacto.
- Sin cambios funcionales ni cambios bajo Domain, Application, UI, `assets/catalogs`, `deploy`, ni en
  `docs/HANDOFF.md`/`docs/ROADMAP.md`/`docs/initiatives/README.md` fuera de la sesión de integración.
- Implementada ≠ integrada: la integración se rige por la Fase 7 del proceso.

## 12. Condiciones para detenerse

- La migración exigiría tocar `RackEmbedDocument`, `RackListBuilder`, `SystemRegistry`, un `*DrawService`,
  catálogos o geometría → excede alcance.
- Application tendría que depender del registro del Plugin (p. ej. para RACKLISTA) → viola la dirección de
  dependencias; se deja intacto y se documenta fuera de alcance.
- Un paso exigiría cambiar geometría, BOM funcional, GUID, un formato persistido, un mensaje existente, un
  alias o un comando (salvo el error visible autorizado) → cambio de comportamiento, fuera de alcance.
- Sería inevitable introducir reflexión/assembly scanning, un paquete NuGet o un sistema nuevo.
- Aparece en `origin` una rama de I-11 u otra iniciativa que estorbe, o una segunda sesión sobre este
  worktree/rama.
- `origin/main` avanza con conflictos semánticos; árbol sucio; operación Git en curso; o CI no
  diagnosticable con seguridad.
- El inventario mecánico detecta una divergencia observable no explicable por un cambio puramente
  mecánico (o el error visible autorizado).

## 13. Estado versionado y entrega del Pull Request

Iniciativa **manual**: la automatización permanece **deshabilitada** (HANDOFF §4: sin ejecutor nocturno ni
horarios). No se crea `docs/automation/state/I-10.yml` ni Pull Request en esta fase. Identidad del
reclamo: `initiative: I-10`, `branch: architecture/kind-handlers`,
`claim_id: 34935fa5-1a2b-4446-9c9d-1c21bbc0f634`. El reclamo y el respaldo son la rama remota
`architecture/kind-handlers`; cada sesión cierra con commit + push. `completed` no significa integrada.
**Merge automático prohibido.** Git y los resultados verificables prevalecen sobre cualquier campo
`status`.

## 14. Evidencia final

Se completará al cerrar la implementación: Claim-Id, worktree, SHAs por fase, diseño final de la frontera
`SystemRegistry`/`KindHandlerRegistry`, handlers registrados, consumidores migrados, switches conservados
y motivo, pruebas/builds ejecutados, advertencias conocidas frente a errores propios, CI por SHA exacto
(run + jobs), validación manual requerida o no con fundamento, y confirmación de que no se cambió
geometría, BOM funcional, GUID, persistencia, comandos, aliases, catálogos ni compatibilidad legacy salvo
el error visible autorizado para handler ausente. `main` no se modifica fuera de la Fase 7.
