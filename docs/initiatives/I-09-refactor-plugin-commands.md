---
schema: rackcad-initiative/v1
id: I-09
title: Partición de RackFrameCommands y helpers del Plugin
type: refactor
status: claimed
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
preserva exactamente comandos, geometría dibujada, BOM, persistencia, identidad/round-trip y UX,
respaldado por la suite verde y por tests golden de equivalencia de planes.

## 2. Problema

`RackFrameCommands` es un archivo caliente (WORKFLOW §7): una clase repartida en 12 archivos
(`RackFrameCommands.cs` + `.Aliases`, `.BomTotal`, `.Cabecera`, `.Duplicar`, `.Dynamic`, `.Fill`,
`.FlowBed`, `.Help`, `.Layout`, `.List`, `.Selective`) con helpers estáticos cruzados que cada
iniciativa del Plugin debe tocar. El escaneo de referencias de bloque para leer el envelope embebido
por GUID aparece repetido entre comandos (`RACKLIST`, `RACKBOMTOTAL`, `RACKLAYOUT` y las rutas de
edición/restamp), la búsqueda y clonación de bloques y el manejo de capas se resuelven en línea, y
cada comando abre y gestiona su propia transacción de documento. Esa duplicación es la causa de que
I-10 e I-16 declaren estorbo con esta área y de que la lista de archivos calientes no encoja. La
auditoría 2026-07 lo registró como P2 y P5.

## 3. Alcance

Estrictamente el alcance de I-09 en ROADMAP, sin ampliaciones laterales:

- **Partir `RackFrameCommands` en clases por área** (una por familia de comandos), conservando
  exactamente los nombres de comando (`CommandMethod`), sus alias y su firma de invocación.
- **Promover helpers a tipos reutilizables** hoy inline o estáticos cruzados: `RackBlockFinder`
  (localización de definiciones y referencias de bloque), `RackCloner` (clonación de definiciones,
  enlazada o independiente según la convención vigente) y `LayerHelper` (creación y selección de
  capas).
- **Introducir un helper `InDocumentTransaction`** que encapsule el patrón repetido de apertura,
  commit y disposición de la transacción de documento.
- **Unificar el escaneo de envelopes** (lectura del `RackEmbedDocument` por GUID sobre las
  referencias de bloque) en un único punto consumido por los comandos que hoy lo triplican.
- **Preservar exactamente el comportamiento observable**: comandos, alias, geometría dibujada, BOM
  (plano y por componentes, consolidado por GUID), persistencia y round-trip de identidad, y UX
  (prompts, mensajes, selección y capas resultantes).
- **Red de equivalencia**: apoyar el refactor en la suite completa y en tests golden de equivalencia
  de planes (snapshot del plan de bloques antes vs. después, patrón «ARRAY == plano» ya existente).

## 4. Fuera de alcance

- **I-10 (`architecture/kind-handlers`)**: no se introduce `IRackKindHandler` ni el
  `KindHandlerRegistry` del Plugin, ni se despacha por registro. Los switches y despachos por `Kind`
  actuales se conservan tal cual.
- **I-16 (`refactor/draw-services`)**: no se crea el `ViewBlockDrawService` genérico, no se colapsan
  los `*DrawService`, no se extrae `BlockPlacementService` ni se uniforma `regen`. Los DrawServices
  quedan intactos.
- **Cualquier cambio funcional**: ninguna geometría nueva, comando nuevo, cambio de BOM, de
  persistencia/schema, de catálogos ni de UX; ningún ajuste de comportamiento «de paso».
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
- Los tests golden existentes de equivalencia de planes como referencia del contrato de equivalencia.

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
- Tests golden nuevos en `tests/RackCad.Tests` que capturen la equivalencia de planes (sin alterar la
  intención de los tests existentes).

No se esperan cambios bajo `src/RackCad.Domain`, `src/RackCad.Application` (más allá de su lectura),
`src/RackCad.UI`, `assets/catalogs`, `deploy`, `docs/HANDOFF.md` ni `docs/ROADMAP.md`. Una desviación
material obliga a detenerse.

## 8. Fases

- [x] F0. Reclamo atómico: rama + worktree desde `origin/main`, commit vacío de reclamo y primer push
  aceptado sin force; baseline verde de `origin/main` verificada.
- [x] F1. Publicar este contrato detallado en la rama.
- [ ] F2. Extraer los helpers reutilizables (`RackBlockFinder`, `RackCloner`, `LayerHelper`,
  `InDocumentTransaction`) sin cambiar las llamadas de comando; verde en cada paso.
- [ ] F3. Unificar el escaneo de envelopes en un único escáner y migrar los comandos que hoy lo
  duplican.
- [ ] F4. Partir `RackFrameCommands` en clases por área conservando comandos, alias y firmas.
- [ ] F5. Añadir/afinar los tests golden de equivalencia de planes y correr la suite completa más los
  builds Debug de UI y Plugin.
- [ ] Paso posterior. Preparación de integración manual (rebase final, CI, muestreo de validación,
  HANDOFF/ROADMAP como último commit) y merge por el dueño.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
git diff origin/main --check
```

- La suite completa permanece verde en cada fase (no solo los tests nuevos).
- Build Debug de UI y Plugin con 0 errores; los `MSB3277` conocidos de las referencias de AutoCAD no
  cuentan.
- CI de rama verde (Tests, Build UI, Build Plugin without AutoCAD) al cerrar cada sesión.
- Tests golden de equivalencia de planes: el snapshot del plan de bloques antes vs. después debe ser
  idéntico; cualquier diferencia detiene la fase.

## 10. Validación manual

I-09 no está marcada con ✋ en ROADMAP. La equivalencia de comportamiento se garantiza por
construcción con la suite y los tests golden de equivalencia de planes; la validación humana en
AutoCAD queda en **muestreo** al integrar (ROADMAP, principio 4). No se requiere una validación
NETLOAD completa como gate de la implementación. Si un cambio dejara de ser mecánicamente equivalente,
se convierte en cambio de comportamiento y sale del alcance (detenerse, no «arreglar de paso»).

## 11. Criterios de aceptación

- Todos los comandos y alias existentes conservan nombre, firma y comportamiento observable.
- `RackBlockFinder`, `RackCloner`, `LayerHelper` e `InDocumentTransaction` existen como tipos
  reutilizables y son el único sitio de su lógica; los comandos los consumen.
- El escaneo de envelopes vive en un único punto consumido por los comandos que antes lo triplicaban.
- `RackFrameCommands` queda partido en clases por área; ningún archivo concentra la clase completa.
- La suite completa, el build Debug de UI y el build Debug de Plugin terminan sin fallos ni
  errores/advertencias propias.
- Los tests golden de equivalencia de planes confirman un plan de bloques idéntico antes vs. después.
- No hay cambios funcionales ni cambios bajo Domain, Application, UI, `assets/catalogs` o `deploy`; ni
  en `docs/HANDOFF.md`/`docs/ROADMAP.md` fuera de la sesión de integración.

## 12. Condiciones para detenerse

- Un paso exige un cambio de comportamiento observable (geometría, BOM, persistencia, comandos o UX).
- La partición o la promoción de helpers obligaría a introducir `IRackKindHandler`/registro (I-10) o a
  tocar los `*DrawService` (I-16).
- Aparece en `origin` la rama de I-10 (`architecture/kind-handlers`) o de I-16
  (`refactor/draw-services`): estorbo activo.
- `origin/main` avanza con conflictos semánticos, o aparece otra sesión activa sobre este worktree o
  esta rama.
- Un test golden detecta una divergencia de plan que no se explica por un cambio puramente mecánico.
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

F0 y F1 completas: rama y worktree creados desde `origin/main` con commit vacío de reclamo
(`Claim-Id: 5234fb6e-928b-4bc1-84b6-df9881159e96`) y push aceptado sin force; este contrato publicado
como primer commit no vacío. `main` no fue modificada: el trabajo vive solo en la rama
`refactor/plugin-commands`. Baseline de apertura: CI de `origin/main` verde en sus tres jobs (Tests,
Build UI, Build Plugin without AutoCAD). Las fases F2-F5 y la preparación de integración quedan
pendientes; esta fase no toca código. Los conteos de pruebas y hashes canónicos viven en
`docs/HANDOFF.md` (§12), no aquí.
