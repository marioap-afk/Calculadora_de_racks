---
schema: rackcad-initiative/v1
id: I-15
title: Editor Shell
type: architecture
status: review-ready
branch: architecture/editor-shell
base_branch: main
priority:
size: M
depends_on: [I-08, I-14]
conflicts_with: [I-14]
context_packs:
  - architecture-kernel
  - ui-editors
  - persistence
  - delivery-validation
automation_state_path: docs/automation/state/I-15.yml
decision_paths: []
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

# I-15 — Editor Shell

## 1. Objetivo

Concentrar en una infraestructura compartida de `RackCad.UI` los patrones que hoy cada ventana de
editor clona (catálogo, identidad GUID+nombre, recomputación coalescida y contrato de
inserción/actualización) y hacer que el **menú principal** y la **biblioteca de diseños** consuman un
**registro explícito de módulos de editor** en lugar de las ~19 propiedades de payload y los handlers
por sistema que crecen O(N). Concretamente:

- `RackEditorSession<TDesign, TSystem>`: sesión compartida que reúne catálogo, identidad
  (`RackEditorIdentity`), recomputación coalescida (`RecomputeGate`) y el bloque del contrato de
  inserción/actualización (`InsertRequested`/`UpdateOnly`/`InsertView`/`InsertSection` +
  `RackInsertionRequest`);
- `IRackEditorModule` + `EditorModuleRegistry`: registro **explícito, sin reflexión** (mismo patrón que
  el `SystemRegistry` de I-08), en orden estable, que resuelve por `RackSystemKind`, rechaza duplicados
  y despacha la apertura desde el menú y desde la biblioteca;
- `RackInsertionRequest`: el contrato tipado de inserción (jerarquía por Kind) que el menú produce vía
  los módulos y que el host del Plugin consume, reemplazando el racimo de propiedades por-sistema.

El shell no es andamiaje aparte: **las cuatro ventanas ricas de editor (selectivo, dinámico, cama,
cabecera) lo adoptan** para esas cuatro capacidades compartidas, eliminando la duplicación real que hoy
tienen inline. Lo que queda para I-20/I-21 es sólo la extracción del estado PROPIO de cada editor (la
matriz por fondo y `BuildSystem`, el `Recompose`/módulos), no las capacidades del shell.

El resultado es verificable cuando: la solución compila; `RackCad.Tests` sigue verde; el proyecto
`RackCad.UI.Tests` cubre sesión, registro, orden/unicidad de módulos, despacho de menú y biblioteca,
recomputación coalescida, identidad y contrato de inserción **y la adopción del shell por las ventanas
reales (pruebas STA)**, y pasa; los builds Debug de UI y Plugin quedan en 0 errores propios; y CI queda
verde sobre la punta publicada. El comportamiento observable (dibujo, BOM, GUID, edición multivista,
persistencia, formatos en disco, etiquetas y orden del menú) es idéntico al vigente.

## 2. Problema

La auditoría 2026-07 (recomendación 9 «Editor Shell», hallazgos E3, E5, U1 parcial) y el ROADMAP
dividen ese trabajo en tres: controles (I-14, integrada), **shell (I-15)** y extracción del estado por
editor (I-20/I-21). Hoy, antes de I-15:

- **E5 — `RackMainMenuWindow` crece O(N) por tipo.** El menú expone ~19 propiedades de payload
  (`ConfigurationToInsert`, `DynamicSystemToInsert`/`DynamicDesignToInsert`/`DynamicRackId`/…,
  `FlowBedToInsert`/…, `SelectiveSystemToInsert`/…, más los tres `*SourceProjectToInsert`/
  `*SourceDocumentToInsert` de I-11) y cinco handlers `Design*_Click` de cuerpo casi idéntico, más un
  `OpenDesignLibrary_Click` con un switch de cinco ramas por `RackSystemKind`. Añadir un sistema exige
  editar esta ventana en tres puntos (propiedades, handler, rama de biblioteca) — es exactamente el
  archivo caliente que I-15 debe encoger, y el punto donde los agentes paralelos colisionan.
- **E3 — cada editor clona la tubería.** Selectivo, dinámico, cama, cabecera y larguero repiten, cada
  uno a su manera, la carga de catálogo (`UiSupport.LoadCatalogSafe`), la generación de identidad
  (`currentId`/`currentName` + `Guid.NewGuid()` inline en tres ventanas), la recomputación coalescida
  (tres mecanismos distintos: scope `IDisposable` en selectivo, `Dispatcher.BeginInvoke` en cabecera,
  llamada directa en los demás) y el contrato de inserción (`RequestDraw(view, updateOnly)` casi
  idéntico en tres ventanas). No hay un hogar compartido para estos cuatro conceptos.
- **U1 parcial.** El documento del editor vive en campos privados de la Window; la sesión compartida y
  el contrato tipado son el primer paso para poder extraer ese estado (I-20/I-21) sin reescribir cada
  ventana.

## 3. Alcance

Autorizado por el ROADMAP (Fase 3, I-15) y por el objetivo §7.2/§7.3 de `ARCHITECTURE.md`:

1. Crear la infraestructura del shell en `src/RackCad.UI/Editor/`, separando la lógica pura y testeable
   de la vista WPF:
   - `RackInsertionRequest` (base abstracta) + los cuatro subtipos por Kind (`HeaderInsertionRequest`,
     `DynamicInsertionRequest`, `FlowBedInsertionRequest`, `SelectiveInsertionRequest`), que llevan
     **exactamente** los datos que hoy el menú pasa a cada `Draw*` del Plugin.
   - `RackEditorIdentity` (Id + Name + `EnsureId()` con fábrica de GUID inyectable), centralizando el
     idiom `if (string.IsNullOrWhiteSpace(currentId)) currentId = Guid.NewGuid().ToString();`.
   - `RecomputeGate` (modelo scope-defer del selectivo) y `RecomputeDebouncer` + `IRecomputeScheduler`
     (modelo async de la cabecera), los dos sabores de recomputación coalescida vigentes, con
     scheduler inyectable para pruebas y un adaptador `DispatcherRecomputeScheduler` para producción.
   - `RackEditorLaunchContext` (Owner + CanInsertInAutoCad + DimensionStyles) e `IRackEditorModule` +
     `EditorModuleRegistry` (registro explícito ordenado, sin reflexión), con `ResolveForLibrary`.
   - `RackEditorSession<TDesign, TSystem>` que compone catálogo (inyectable, por defecto
     `UiSupport.LoadCatalogSafe`), identidad, `RecomputeGate` y el bloque del contrato de
     inserción/actualización (`RequestInsert`/`RequestUpdate` con la normalización
     `updateOnly ? null : view` / `updateOnly ? -1 : section`).
2. **Conectar el shell a consumidores reales de producción** (adaptación mínima o composición, sin
   extraer todavía las matrices, los modelos internos ni la lógica propia de selectivo/dinámico — eso es
   I-20/I-21):
   - `RackSelectiveWindow` adopta `RackEditorSession` para catálogo, identidad (`RackEditorIdentity`),
     recomputación coalescida (`RecomputeGate` reemplaza su `RecomputeDeferral`/depth/pending inline) y el
     contrato de inserción/actualización (`RequestInsert`/`RequestUpdate`);
   - `RackDynamicSystemWindow` y `RackFlowBedWindow` adoptan `RackEditorSession` para catálogo, identidad
     y contrato de inserción (su recomputación es directa hoy, no coalesce);
   - `RackFrameConfiguratorWindow` adopta `RecomputeDebouncer` + `DispatcherRecomputeScheduler` para la
     coalescencia asíncrona de su preview.
   Las propiedades públicas que el Plugin lee (`InsertRequested`, `SystemToInsert`, `RackId`, …) pasan a
   ser getters sobre la sesión; el estado propio de cada editor (matriz por fondo, `BuildSystem`,
   `Recompose`, módulos) queda intacto para I-20/I-21.
3. Migrar `RackMainMenuWindow` a consumir `EditorModuleRegistry`: los cinco handlers `Design*_Click`
   pasan a delegar en un único lanzador registry-driven; `OpenDesignLibrary_Click` resuelve el módulo
   por `registry.ResolveForLibrary(entry, project)`; el menú expone **una** propiedad
   `InsertionRequest` (más `InsertRequested` derivado) en lugar de las ~19 propiedades por sistema. Las
   etiquetas, subtítulos, orden de botones y textos de error se conservan **verbatim** (el XAML del
   menú no cambia).
4. Adaptar el único consumidor del payload del menú en el Plugin
   (`RackMenuCommands.RackCad()`) para leer `menu.InsertionRequest` y despachar por Kind a los
   **mismos** `Draw*` con los **mismos** argumentos (transcripción 1:1, sin cambio de dibujo).
5. Crear pruebas en `tests/RackCad.UI.Tests` proporcionales al riesgo: unitarias de sesión, registro
   (orden/unicidad/lookup), despacho de biblioteca (con la regla de fallback de cabecera), identidad,
   recomputación coalescida (defer y debounce) y contrato de inserción; **más pruebas de integración que
   construyen las ventanas reales (STA) y verifican que adoptan el shell** (identidad vía `LoadExisting`,
   contrato de inserción y debouncer respaldados por la sesión).

## 4. Fuera de alcance

- **Migrar el estado interno de Selectivo o Dinámico (I-20/I-21).** Las ventanas SÍ adoptan el shell para
  las cuatro capacidades compartidas (catálogo, identidad, recomputación coalescida, contrato de
  inserción/actualización) — eso es el objetivo de esta iniciativa. Lo que NO se extrae aquí es el estado
  PROPIO de cada editor: la matriz por fondo y `BuildSystem` del selectivo, el `Recompose`/módulos del
  dinámico, los modelos internos. Esa extracción a clases puras (testeables sin WPF) es I-20/I-21; el
  larguero, sin identidad ni inserción, no adopta la sesión (sólo comparte el catálogo vía `UiSupport`).
- Rediseñar ventanas o cambiar la apariencia/identidad visual vigente.
- Implementar Push Back (I-18) u otro sistema nuevo.
- Ampliar I-14 (los controles comunes) o migrar ventanas a esos controles.
- Modificar los Draw Services del Plugin (I-16) o la geometría, resolvers, builders o BOM.
- Cambiar formatos persistidos, DTO, envelope, Xrecord o el round-trip (I-11 intacto: los
  `*SourceProjectToInsert`/`*SourceDocumentToInsert` se transportan tal cual).
- Extender el `KindHandlerRegistry` del Plugin (I-10) o `SystemRegistry` (I-08): el registro de módulos
  de editor es de UI y distinto de ambos.
- Dependencias NuGet de producto (política cero-NuGet, ADR-0003) y paquetes de test nuevos.

## 5. Contexto requerido

- `AGENTS.md` (convenciones, dirección de dependencias, definición de terminado, dependencias).
- `docs/WORKFLOW.md` (ciclo de iniciativa, archivos calientes, cierre) y `docs/ROADMAP.md` (I-15, sus
  dependencias I-08/I-14 y su estorbo I-14).
- `docs/ARCHITECTURE.md` §4 (identidad/round-trip/BOM/catálogos), §5 (UI) y §7.2/§7.3 (registros y
  Editor Shell objetivo).
- `docs/auditoria-arquitectura-2026-07.md` §4.1 (registros) y §4.2 (patrón Editor Shell), hallazgos
  E3/E5/U1.
- Context Packs: `architecture-kernel`, `ui-editors`, `persistence`, `delivery-validation`.
- Precedentes de registro explícito: `src/RackCad.Application/Systems/SystemRegistry*.cs` (I-08) y
  `src/RackCad.Plugin/KindHandlers/*` (I-10).
- Código consumido/adaptado: `src/RackCad.UI/RackMainMenuWindow.xaml{,.cs}`,
  `RackDesignLibraryWindow.xaml.cs`, las cinco ventanas de editor (`RackSelectiveWindow`,
  `RackDynamicSystemWindow`, `RackFlowBedWindow`, `RackFrameConfiguratorWindow`, `RackLargueroWindow`),
  `UiSupport.cs`; `src/RackCad.Application/Persistence/RackProject.cs`, `RackDesignLibrary.cs`,
  `RackProjectStore.cs`; `src/RackCad.Plugin/RackMenuCommands.cs` y las firmas `Draw*` de
  `RackCabeceraCommands`/`RackDinamicoCommands`/`RackCamaCommands`/`RackSelectivoCommands`.

## 6. Dependencias

- **Integradas requeridas:** I-08 (`SystemRegistry`, integrada 2026-07-21) e I-14 (controles + proyecto
  `RackCad.UI.Tests`, integrada 2026-07-21). Ambas están en `origin/main`.
- **Estorbos que deben permanecer inactivos:** I-14 (misma capa de UI) — ya integrada, no en curso.
  I-12 (versionado real) e I-19 (validador de catálogos) **también se integraron** a `origin/main`
  (2026-07-21) mientras I-15 estaba en curso; esta rama fue **rebasada sobre esa punta** (`origin/main` =
  `646614d`, Merge I-19 sobre I-12). La reconciliación fue mecánica y de solo-infra: la centralización de
  `LangVersion`/`Nullable` de I-12 en `Directory.Build.props` (los csproj de I-15 dejan de declararlas
  localmente) y las entradas de índice + hallazgos de I-19 en docs. **I-15 no incorpora ningún cambio
  funcional de I-19 en el código de UI** — solo la reconciliación natural del rebase.
- **Entradas del dueño:** ninguna decisión requerida para arrancar. La validación en AutoCAD del path
  de inserción del menú es responsabilidad del dueño al cierre (gate abierto, §10).

## 7. Archivos esperados

Crear (producto, `src/RackCad.UI/Editor/`):

- `RackInsertionRequest.cs` (base + cuatro subtipos por Kind);
- `RackEditorLaunchContext.cs`;
- `RackEditorIdentity.cs`;
- `RecomputeGate.cs`, `RecomputeDebouncer.cs`, `IRecomputeScheduler.cs`, `DispatcherRecomputeScheduler.cs`;
- `RackEditorSession.cs` (`RackEditorSession<TDesign, TSystem>` + `RackInsertionContext<TDesign, TSystem>`);
- `IRackEditorModule.cs`, `EditorModuleRegistry.cs`;
- `EditorModules.cs` (los cinco módulos concretos que adaptan las ventanas existentes).

Crear (pruebas, `tests/RackCad.UI.Tests/`):

- `RackEditorIdentityTests.cs`, `RecomputeGateTests.cs`, `RecomputeDebouncerTests.cs`,
  `RackEditorSessionTests.cs`, `EditorModuleRegistryTests.cs`, `RackInsertionRequestTests.cs`;
- `EditorShellAdoptionTests.cs` (integración STA: las ventanas reales adoptan el shell).

Modificar (acotado, sin cambio de comportamiento observable):

- Adopción del shell (getters sobre la sesión; estado interno intacto): `RackSelectiveWindow.xaml.cs`,
  `RackDynamicSystemWindow.xaml.cs`, `RackFlowBedWindow.xaml.cs` (sesión: catálogo + identidad +
  recompute/insert) y `RackFrameConfiguratorWindow.xaml.cs` (debouncer);
- `src/RackCad.UI/RackMainMenuWindow.xaml.cs` (consume el registro; una sola propiedad `InsertionRequest`);
- `src/RackCad.Plugin/RackMenuCommands.cs` (lee `InsertionRequest` y despacha por Kind);
- `src/RackCad.UI/RackCad.UI.csproj` (`InternalsVisibleTo` para el proyecto de pruebas) y
  `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` (copia de catálogos junto al binario de pruebas);
- `docs/initiatives/README.md` (índice), `docs/automation/state/I-15.yml` (estado),
  `docs/ideas-futuras.md` (hallazgo fuera de alcance).

No se espera modificar el XAML del menú ni de los editores, `RackLargueroWindow` (sin identidad ni
inserción), el estado propio de los editores (matriz/`BuildSystem`/`Recompose`), Domain/Application,
Plugin fuera de `RackMenuCommands`, catálogos, `deploy/` ni `.github/workflows`. Una desviación material
obliga a detenerse (§12).

## 8. Fases

1. **Reclamo y contrato.** Rama + worktree desde `origin/main`, commit de reclamo + push, contrato
   creado desde `TEMPLATE.md`, índice y estado versionado. (Evidencia: rama remota aceptada, este
   archivo.)
2. **Infra pura del shell.** `RackInsertionRequest`, `RackEditorIdentity`, `RecomputeGate`,
   `RecomputeDebouncer` + scheduler, `RackEditorSession`, con sus pruebas. (Evidencia: suite de UI
   verde.)
3. **Registro y módulos.** `IRackEditorModule`, `EditorModuleRegistry`, los cinco módulos concretos y
   sus pruebas de registro/despacho. (Evidencia: suite de UI verde.)
4. **Wire del menú y del Plugin.** `RackMainMenuWindow` consume el registro; `RackMenuCommands.RackCad`
   lee `InsertionRequest`. (Evidencia: builds Debug de UI y Plugin en 0 errores propios.)
5. **Gates automatizados.** Suite completa + suite de UI + build de la solución + CI de la rama sobre
   el SHA publicado. (Evidencia: §14.)
6. **Cierre de sesión.** Revisión de diff, commits lógicos, push de la rama, estado versionado y gates
   manuales documentados como abiertos. (Evidencia: §14.)

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` — suite completa verde (sin regresión).
- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` — nuevas pruebas del shell verdes.
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` — 0 errores, 0 advertencias propias.
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` — 0 errores propios (solo los
  `MSB3277` conocidos de AutoCAD; requiere AutoCAD 2025 para el build completo del Plugin).
- `dotnet build RackCad.sln -c Debug` — build completo de la solución.
- CI: los cuatro jobs (Tests, Build UI, Build Plugin sin AutoCAD, UI tests) en verde sobre la punta
  publicada de la rama.

## 10. Validacion manual

- **AutoCAD (gate abierto).** I-15 cambia el consumidor del payload del menú en el Plugin
  (`RackMenuCommands.RackCad`) de un racimo de propiedades a un único `RackInsertionRequest` despachado
  por Kind. El dibujo resultante es idéntico (mismos `Draw*`, mismos argumentos), pero el path de
  inserción del menú toca el Plugin, así que el dueño debe validar en AutoCAD que **RACKCAD → cada
  opción → Insertar** dibuja igual que antes para los cinco sistemas (cabecera, dinámico, cama,
  selectivo; larguero no inserta) y que **abrir desde la biblioteca** cada tipo reabre el editor y su
  inserción funciona (incluida la preservación de metadatos I-11). No se declara aprobado en esta
  corrida (`requires_autocad: true`, gate abierto).
- **AutoCAD — round-trip de los editores adoptados (gate abierto).** Las cuatro ventanas ricas ahora
  toman identidad e inserción de la sesión. El dueño valida además los comandos directos
  (`RACKSELECTIVO`/`RACKDINAMICO`/`RACKCAMA` → Insertar) y `RACKEDITAR` sobre un rack ya dibujado
  (Actualizar en sitio y Insertar vista enlazada): el GUID se conserva, las vistas se ligan y el dibujo
  es idéntico. La coalescencia del preview del configurador y del selectivo debe verse fluida como antes.
- **owner-validation (gate abierto).** El dueño confirma que las etiquetas, el orden y los flujos del
  menú y de la biblioteca son idénticos.

## 11. Criterios de aceptacion

- Existe la infraestructura del shell en `src/RackCad.UI/Editor/` con lógica pura separada de la vista,
  sin referencias AutoCAD y sin duplicar reglas de Application.
- El `EditorModuleRegistry` es explícito (sin reflexión), de orden estable, rechaza Kinds duplicados,
  resuelve por Kind y resuelve la biblioteca preservando la regla de fallback de cabecera.
- `RackMainMenuWindow` consume el registro y expone **una** propiedad de payload
  (`InsertionRequest`) en lugar de las ~19 por sistema; el XAML (etiquetas, subtítulos, orden) no cambió.
- `RackMenuCommands.RackCad` dibuja exactamente igual que antes para los cinco casos.
- Las cuatro ventanas ricas (selectivo, dinámico, cama, cabecera) adoptan el shell para las capacidades
  compartidas: sus getters públicos leen la sesión, la generación de GUID y la recomputación coalescida
  ya no están inline, y `RackEditorSession`/`RackEditorIdentity`/`RecomputeGate`/`RecomputeDebouncer`
  tienen consumidores de producción (no sólo pruebas). El estado propio de cada editor no se migró.
- `RackCad.Tests` permanece verde; `RackCad.UI.Tests` cubre los ejes de riesgo del shell **y** la
  adopción por ventana real (STA), y pasa; los builds de UI, Plugin y solución quedan en 0 errores
  propios; CI verde en la rama.
- Ninguna ventana cambió de comportamiento observable (dibujo, BOM, GUID, persistencia, etiquetas); sólo
  cambió de dónde toma catálogo/identidad/recompute/insert (la sesión).
- La dirección de dependencias se conserva (UI no referencia AutoCAD; el shell no depende del Plugin) y
  no hay dependencias NuGet nuevas.

## 12. Condiciones para detenerse

- Que preservar el comportamiento exija migrar el estado interno de una ventana de editor (eso es
  I-20/I-21): detenerse antes de ampliar alcance.
- Que el wire del menú obligue a cambiar geometría, BOM, persistencia, formatos, Draw Services (I-16),
  `KindHandlerRegistry` (I-10) o `SystemRegistry` (I-08).
- Que el cambio del consumidor en el Plugin exija algo más que una transcripción 1:1 de los `Draw*`
  vigentes (p. ej. un nuevo método de handler): detenerse y reconsiderar el alcance.
- Que aparezca en `origin` otra rama tocando `RackMainMenuWindow`/`RackMenuCommands` de forma
  incompatible: no sobrescribir; entregar evidencia.
- Cualquier necesidad de un paquete NuGet nuevo (producto o test).

## 13. Estado versionado y entrega del Pull Request

Estado canónico en `docs/automation/state/I-15.yml`. La automatización está pausada
(`automation.enabled: false`): el ejecutor es manual y mantiene ese archivo al cierre de la sesión. No
se abre un segundo Pull Request ni se activa auto-merge. La integración a `main` es una sesión manual
posterior (WORKFLOW §4.5) y no forma parte de esta corrida; los gates `autocad` y `owner-validation`
quedan abiertos.

## 14. Evidencia final

Se completa al cierre de la sesión: commits lógicos con trailer de procedencia, archivos creados/
modificados, resultados de `dotnet test` (suite completa y UI), builds de UI/Plugin/solución, evidencia
de CI sobre el SHA publicado, SHA base y punta de la rama, confirmación del push, gates manuales
abiertos (AutoCAD + owner-validation) y confirmación de que `main` no fue modificada. El detalle vivo
del proyecto se actualiza en `docs/HANDOFF.md` §8-12 **solo** en la sesión de integración (último commit
de la rama), nunca desde esta rama en paralelo.
