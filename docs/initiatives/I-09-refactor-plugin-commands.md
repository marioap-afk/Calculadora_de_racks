---
schema: rackcad-initiative/v1
id: I-09
title: Partición de RackFrameCommands y helpers del Plugin
type: refactor
status: completed
branch: refactor/plugin-commands
base_branch: main
priority:
size: M
depends_on: [I-02]
conflicts_with: [I-10, I-16]
context_packs:
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

# I-09 — Partición de RackFrameCommands y helpers del Plugin

## 1. Objetivo

Reestructurar la superficie de comandos del Plugin **sin cambiar comportamiento**: partir la clase
`RackFrameCommands` (hoy repartida en 12 archivos con helpers estáticos cruzados) en clases por área;
promover a tipos reutilizables los helpers de bloques, clonación, capas y transacciones
(`RackBlockFinder`, `RackCloner`, `LayerHelper` y un helper `InDocumentTransaction`); y unificar en un
único punto el escaneo de envelopes hoy triplicado. El resultado es un diff mecánico revisable que
preserva exactamente comandos, geometría dibujada, BOM, persistencia, identidad/round-trip y UX. Como
I-09 refactoriza infraestructura AutoCAD del Plugin y **no cambia los planes puros de Application**, la
equivalencia se sostiene con un inventario antes/después de todos los `[CommandMethod]` (comandos,
alias, prompts, keywords y mensajes idénticos), la suite existente completa, los builds Debug de UI y
Plugin y una revisión mecánica del refactor, no con planes golden nuevos.

## 2. Problema

`RackFrameCommands` es un archivo caliente (WORKFLOW §7): una clase repartida en 12 archivos
(`RackFrameCommands.cs` + `.Aliases`, `.BomTotal`, `.Cabecera`, `.Duplicar`, `.Dynamic`, `.Fill`,
`.FlowBed`, `.Help`, `.Layout`, `.List`, `.Selective`) con helpers estáticos cruzados que cada
iniciativa del Plugin debe tocar. El escaneo de referencias de bloque para leer el envelope embebido
por GUID aparece repetido entre comandos (`RACKLISTA`, `RACKBOMTOTAL`, `RACKLAYOUT` y las rutas de
edición/restamp), la búsqueda y clonación de bloques y el manejo de capas se resuelven en línea, y
cada comando abre y gestiona su propia transacción de documento. Esa duplicación es la causa de que
I-10 e I-16 declaren estorbo con esta área y de que la lista de archivos calientes no encoja. La
auditoría 2026-07 lo registró como P2 y P5.

## 3. Alcance

Estrictamente el alcance de I-09 en ROADMAP, sin ampliaciones laterales:

- **Partir `RackFrameCommands` en clases por área** (una por familia de comandos), conservando
  exactamente los nombres de comando (`CommandMethod`), sus alias y su firma de invocación.
- **Promover helpers a tipos reutilizables** hoy inline o estáticos cruzados:
  - `RackBlockFinder`: localización de definiciones y referencias de bloque.
  - `RackCloner`: clona definiciones **únicamente para copias independientes**; las copias enlazadas
    siguen creando referencias a la definición existente. Preserva las **dos políticas vigentes** de
    nombres únicos, layers, rotación, escala y restamp **sin unificarlas semánticamente**.
  - `LayerHelper`: creación y selección de capas.
- **Introducir un helper `InDocumentTransaction`** que encapsule el patrón repetido de apertura,
  commit y disposición de la transacción de documento, **solo en operaciones simples y equivalentes**:
  ningún `DBObject` escapa de la transacción; no engloba `Regen`, purgas posteriores al commit ni
  flujos de varias fases; y no cambia locks, `OpenMode`, `forceValidity` ni el orden de commit.
- **Unificar el escaneo de envelopes** (lectura del `RackEmbedDocument` por GUID sobre las
  referencias de bloque) en un único punto consumido por los comandos que hoy lo triplican.
- **Preservar exactamente el comportamiento observable**: comandos, alias, geometría dibujada, BOM
  (plano y por componentes, consolidado por GUID), persistencia y round-trip de identidad, y UX
  (prompts, keywords, mensajes, selección y capas resultantes).
- **Red de equivalencia (sin planes golden nuevos)**: la evidencia es un inventario antes/después de
  todos los `[CommandMethod]` (comandos y alias idénticos; prompts, keywords y mensajes preservados),
  la suite existente completa, los builds Debug de UI y Plugin, y una revisión mecánica de scans,
  clonación/restamp, layers, transacciones, purgas y `Regen`. Se añaden tests nuevos **solo si** se
  extrae lógica pura realmente testeable sin AutoCAD.

## 4. Fuera de alcance

- **I-10 (`architecture/kind-handlers`)**: no se introduce `IRackKindHandler` ni el
  `KindHandlerRegistry` del Plugin, ni se despacha por registro. Los switches y despachos por `Kind`
  actuales se conservan tal cual.
- **I-16 (`refactor/draw-services`)**: no se crea el `ViewBlockDrawService` genérico, no se colapsan
  los `*DrawService`, no se extrae `BlockPlacementService` ni se uniforma `regen`. Los DrawServices
  quedan intactos.
- **Cualquier cambio funcional**: ninguna geometría nueva, comando nuevo, cambio de BOM, de
  persistencia/schema, de catálogos ni de UX; ningún ajuste de comportamiento «de paso».
- **Unificar semánticamente las políticas de clonación**: las dos políticas de nombres únicos, layers,
  rotación, escala y restamp se conservan; `RackCloner` solo extrae el código común, no fusiona
  comportamientos.
- Cambios bajo `src/RackCad.Domain`, `src/RackCad.Application`, `src/RackCad.UI`,
  `assets/catalogs`, `deploy` o el contenido de `blocks-library.dwg`.
- Actualizar `docs/HANDOFF.md` o `docs/ROADMAP.md` (solo en la sesión de integración, WORKFLOW §4.5).
- Registros o contratos compartidos nuevos (trabajo de I-08).
- Merge, auto-merge, integración o limpieza de rama/worktree.

## 5. Contexto requerido

- Fuentes globales: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md` y
  `docs/ARCHITECTURE.md` (§§2-5; identidad y round-trip §4.1; BOM §4.3; adaptación AutoCAD §5).
- Context Packs `autocad-plugin` (comandos, transacciones, dibujo y Xrecords) y `delivery-validation`
  (build, CI y validación) con sus guías requeridas.
- Código: `src/RackCad.Plugin/RackFrameCommands.cs` y sus parciales; `src/RackCad.Plugin/Systems/`
  (`RackBlockData.cs`, `SystemBlockWriter.cs`, los `*DrawService` como consumidores, sin modificarlos);
  el sobre `RackEmbedDocument`/`RackEmbedStore` en Application (lectura para entender el formato; no se
  modifica).
- La suite existente `tests/RackCad.Tests` como red de regresión; los tests de plan puros de
  Application permanecen sin cambios (I-09 no toca esa capa).

No se cargan Context Packs de UI, selectivo ni dinámico como objetivo de edición: el alcance no
cambia esas capas.

## 6. Dependencias

- **Depende de I-02** (`feature/dinamico-modular`), integrada el 2026-07-17; sus comandos dinámicos ya
  viven en `RackFrameCommands.Dynamic.cs`.
- **Conflictos que deben permanecer inactivos**: I-10 (`architecture/kind-handlers`) e I-16
  (`refactor/draw-services`) tocan la misma área del Plugin. ROADMAP serializa la pista B:
  I-09 → I-16 → I-10. En el reclamo se verificó que ninguna de esas ramas existe en `origin`.
- Convive con I-07 (`docs/adr-retroactivos`): distinta capa (docs), sin archivos calientes
  compartidos.
- Sin entrada del dueño requerida para abrir la iniciativa.

## 7. Archivos esperados

Nuevos (todos en `src/RackCad.Plugin`, salvo los docs):

- Clases de comandos por área extraídas de los parciales de `RackFrameCommands`.
- `RackBlockFinder`, `RackCloner`, `LayerHelper` y el helper `InDocumentTransaction` (verificado: no
  existen hoy).
- Un único escáner de envelopes consumido por los comandos.
- Este contrato: `docs/initiatives/I-09-refactor-plugin-commands.md`.

Modificados:

- Los parciales de `RackFrameCommands` (redistribución mecánica; comandos y alias no cambian de nombre
  ni de firma).
- `docs/initiatives/README.md` para enlazar este contrato.
- Tests en `tests/RackCad.Tests` **solo si** se extrae lógica pura realmente testeable sin AutoCAD; no
  se añaden planes golden nuevos (los planes puros de Application no cambian).

No se esperan cambios bajo `src/RackCad.Domain`, `src/RackCad.Application` (más allá de su lectura),
`src/RackCad.UI`, `assets/catalogs`, `deploy`, `docs/HANDOFF.md` ni `docs/ROADMAP.md`. Una desviación
material obliga a detenerse.

## 8. Fases

- [x] F0. Reclamo atómico: rama + worktree desde `origin/main`, commit vacío de reclamo y primer push
  aceptado sin force; baseline verde de `origin/main` verificada.
- [x] F1. Publicar este contrato detallado en la rama.
- [x] F2. Extraer los helpers reutilizables (`RackBlockFinder`, `RackCloner`, `LayerHelper`,
  `InDocumentTransaction`) sin cambiar las llamadas de comando; verde en cada paso. `RackCloner`
  conserva las dos políticas de clonación; `InDocumentTransaction` solo envuelve operaciones simples y
  equivalentes.
- [x] F3. Unificar el escaneo de envelopes en un único escáner y migrar los comandos que hoy lo
  duplican.
- [x] F4. Partir `RackFrameCommands` en clases por área conservando comandos, alias y firmas.
- [x] F5. Verificar equivalencia: inventario antes/después de todos los `[CommandMethod]` (comandos,
  alias, prompts, keywords y mensajes), revisión mecánica de scans, clonación/restamp, layers,
  transacciones, purgas y `Regen`, suite existente completa y builds Debug de UI y Plugin. Tests
  nuevos solo si se extrajo lógica pura sin AutoCAD.
- [ ] Paso posterior. Preparación de integración manual (rebase final, CI, muestreo de validación,
  HANDOFF/ROADMAP como último commit) y merge por el dueño.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
git diff origin/main --check
```

- La suite existente completa permanece verde en cada fase (no solo los tests nuevos).
- Build Debug de UI y Plugin con 0 errores; los `MSB3277` conocidos de las referencias de AutoCAD no
  cuentan.
- CI de rama verde (Tests, Build UI, Build Plugin without AutoCAD) al cerrar cada sesión.
- Equivalencia sin planes golden nuevos: inventario antes/después de los `[CommandMethod]` con
  comandos, alias, prompts, keywords y mensajes idénticos, más una revisión mecánica de scans,
  clonación/restamp, layers, transacciones, purgas y `Regen`. Cualquier diferencia observable detiene
  la fase. Se añaden tests solo si se extrae lógica pura realmente testeable sin AutoCAD.

## 10. Validación manual

I-09 no está marcada con ✋ en ROADMAP. La equivalencia de comportamiento se sostiene con el inventario
antes/después de los `[CommandMethod]`, la revisión mecánica del refactor y la suite existente; la
validación humana en AutoCAD queda en **muestreo** al integrar (ROADMAP, principio 4). No se requiere
una validación NETLOAD completa como gate de la implementación. Si un cambio dejara de ser
mecánicamente equivalente, se convierte en cambio de comportamiento y sale del alcance (detenerse, no
«arreglar de paso»).

## 11. Criterios de aceptación

- Todos los comandos y alias existentes conservan nombre, firma y comportamiento observable (prompts,
  keywords y mensajes incluidos), verificado con un inventario antes/después de los `[CommandMethod]`.
- `RackBlockFinder`, `RackCloner`, `LayerHelper` e `InDocumentTransaction` existen como tipos
  reutilizables y son el único sitio de su lógica; los comandos los consumen.
- `RackCloner` clona definiciones solo para copias independientes; las copias enlazadas siguen creando
  referencias a la definición existente; conserva las dos políticas vigentes de nombres únicos, layers,
  rotación, escala y restamp sin unificarlas semánticamente.
- `InDocumentTransaction` solo envuelve operaciones simples y equivalentes; ningún `DBObject` escapa de
  la transacción; no engloba `Regen`, purgas posteriores al commit ni flujos de varias fases; no cambia
  locks, `OpenMode`, `forceValidity` ni el orden de commit.
- El escaneo de envelopes vive en un único punto consumido por los comandos que antes lo triplicaban.
- `RackFrameCommands` queda partido en clases por área; ningún archivo concentra la clase completa.
- La suite existente completa, el build Debug de UI y el build Debug de Plugin terminan sin fallos ni
  errores/advertencias propias; no se añaden planes golden nuevos y solo se agregan tests si se extrae
  lógica pura testeable sin AutoCAD.
- No hay cambios funcionales ni cambios bajo Domain, Application, UI, `assets/catalogs` o `deploy`; ni
  en `docs/HANDOFF.md`/`docs/ROADMAP.md` fuera de la sesión de integración.

## 12. Condiciones para detenerse

- Un paso exige un cambio de comportamiento observable (geometría, BOM, persistencia, comandos o UX).
- La partición o la promoción de helpers obligaría a introducir `IRackKindHandler`/registro (I-10) o a
  tocar los `*DrawService` (I-16).
- `RackCloner` no puede extraer el código común sin fusionar las dos políticas de clonación, o
  `InDocumentTransaction` tendría que envolver `Regen`, purgas post-commit, flujos multifase o alterar
  locks/`OpenMode`/`forceValidity`/orden de commit.
- Aparece en `origin` la rama de I-10 (`architecture/kind-handlers`) o de I-16
  (`refactor/draw-services`): estorbo activo.
- `origin/main` avanza con conflictos semánticos, o aparece otra sesión activa sobre este worktree o
  esta rama.
- El inventario de `[CommandMethod]` o la revisión mecánica detectan una divergencia observable
  (comando, alias, prompt, keyword, mensaje o comportamiento) que no se explica por un cambio puramente
  mecánico.
- Se descubre que un helper no puede unificarse sin alterar comportamiento: se anota en
  `docs/ideas-futuras.md` y se detiene.

## 13. Entrega e integración manual

Iniciativa manual: la automatización permanece **deshabilitada** (HANDOFF §4: no hay ejecutor nocturno
activo ni horarios programados). No se crea `docs/automation/state/I-09.yml` ni Pull Request en esta
fase. El reclamo y el respaldo de la iniciativa son la rama remota `refactor/plugin-commands`
(WORKFLOW §2 y §4.1); cada sesión cierra con commit + push de la rama, sin esperar aprobación. La
implementación podrá quedar `completed`, pero `completed` no significa integrada: el rebase final, el
merge `--no-ff`, el CI posterior de `main`, el muestreo de validación en AutoCAD y la limpieza segura
de rama y worktree son operaciones manuales del dueño, externas a este contrato. Nunca se abre un
segundo Pull Request para la iniciativa.

## 14. Evidencia final

**Implementación completa (F0-F5), aún sin integrar.** El trabajo vive solo en la rama
`refactor/plugin-commands`; `main` no fue modificada.

- **F0-F1**: reclamo atómico (`Claim-Id: 5234fb6e-928b-4bc1-84b6-df9881159e96`) y este contrato.
- **F2**: helpers `RackBlockFinder`, `RackCloner` (las dos políticas de nombres únicos quedan
  separadas, sin unificar), `LayerHelper` (unifica dos `EnsureLayer` byte-idénticos) e
  `InDocumentTransaction` (solo operaciones simples y equivalentes) extraídos como `internal static`.
- **F3**: escaneo de envelopes unificado en `RackBlockFinder.ScanEnvelopes` (un único recorrido del
  `BlockTable`); `FindRackBlocks`, RACKLISTA y RACKBOMTOTAL migrados conservando sus filtros,
  agregación de copias = máximo, y `directOnly:true`/`forceValidity:false`.
- **F4**: `RackFrameCommands` (clase única en 12 parciales) eliminado y partido en clases públicas por
  área (`RackMenuCommands`, `RackCabeceraCommands`, `RackSelectivoCommands`, `RackDinamicoCommands`,
  `RackCamaCommands`, `RackDuplicarCommands`, `RackInventarioCommands`, `RackLayoutCommands`,
  `RackAyudaCommands`) más los helpers compartidos `RackCommandSupport` y `RackEnvelopeRestamp`. Cada
  alias vive junto a su comando; el menú y RACKEDITAR despachan por `internal static`. Sin
  `[assembly: CommandClass]`: AutoCAD descubre las clases públicas.
- **F5**: higiene de comentarios (crefs colgantes a `RackFrameCommands` reescritos como tipo
  anterior), auditoría de alcance e inventario/equivalencia finales.

**Equivalencia verificada (método):** 26 `[CommandMethod]` con nombres idénticos, cero duplicados, 13
principales + 13 aliases con los mismos destinos; conjuntos idénticos de literales de código (prompts,
`SetRejectMessage`, keywords, defaults, mensajes, nombres de bloque), `case` por `Kind`, llamadas a
DrawServices y stores, colores ACI, `directOnly`/`forceValidity`, y conteos de `Regen`, `Guid.NewGuid`,
purgas y `catch`/`Report`; el diff normalizado de cuerpos entre `origin/main` y la rama se reduce al
andamiaje del archivo neto adicional. Suite `RackCad.Tests`, builds Debug de UI y Plugin y CI de rama
(Tests + Build UI + Build Plugin without AutoCAD) en verde; `git diff origin/main --check` limpio.
Cambios de producto limitados a `src/RackCad.Plugin`; documentación limitada a este contrato y su
índice; sin dependencias nuevas.

**Pendiente:** la preparación de integración (rebase final sobre `main`, CI de `main`, muestreo de
validación en AutoCAD, actualización de HANDOFF/ROADMAP y merge `--no-ff`) sigue siendo operación
manual del dueño, fuera de esta fase. Los conteos de pruebas y hashes canónicos viven en
`docs/HANDOFF.md` (§12), no aquí.
