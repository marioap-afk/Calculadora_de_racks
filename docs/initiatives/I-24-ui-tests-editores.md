---
schema: rackcad-initiative/v1
id: I-24
title: Tests de editores (ViewModels y estados)
type: refactor
status: integrated
branch: refactor/ui-tests-editores
base_branch: main
priority:
size: S
depends_on: [I-15, I-20, I-21]
conflicts_with: []
context_packs:
  - architecture-kernel
  - ui-editors
  - system-selective
  - system-dynamic-flowbed
  - persistence
  - delivery-validation
  - documentation-governance
automation_state_path: docs/automation/state/I-24.yml
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

# I-24 — Tests de editores (ViewModels y estados)

## 1. Objetivo

Ampliar `tests/RackCad.UI.Tests` (el proyecto nace en I-14) con pruebas de **ViewModels** y de los
**límites reales de los editores ya migrados** —selectivo (I-20), dinámico (I-21), configurador de
cabecera y cama— cubriendo el cableado WPF que la suite pura `RackCad.Tests` (net8.0) **no puede
alcanzar** porque vive en `RackCad.UI` (net8.0-windows). Concretamente (hallazgo U3 del ROADMAP,
Fase 5, I-24), cubrir:

- **recomputación**, **selección** y **mutaciones estructurales** a través del ViewModel o la ventana
  real;
- **carga nueva vs. existente**, **inserción**, **actualización** y **round-trip aplicable**;
- **conservación de GUID, nombre, vista, sección y `UpdateOnly`** por la superficie pública que
  consume el Plugin;
- **rutas negativas** y **casos límite deterministas**.

Resultado verificable cuando: la solución compila; `RackCad.UI.Tests` gana pruebas nuevas que pasan
en el runner STA existente; `RackCad.Tests` **sigue verde** sin cambios (no se le añaden pruebas);
los builds Debug de UI (y de Plugin/solución cuando el entorno lo permite) quedan en 0 errores
propios; CI queda verde en los cuatro jobs sobre la punta publicada. **Ningún comportamiento
observable cambia**: es una iniciativa de pruebas más un único **seam interno** de prueba
(autorizado en §3), sin reglas nuevas.

## 2. Problema

La auditoría 2026-07 (hallazgo **U3**, "UI sin tests por construcción") y el ROADMAP colocan I-24 al
final de la pista de UI: I-14 creó el proyecto `RackCad.UI.Tests` con su runner STA, I-15 extrajo el
Editor Shell y lo adoptaron las cuatro ventanas ricas, e I-20/I-21 extrajeron el **estado propio** de
los editores selectivo/dinámico a `RackCad.Application` (testeado allí de forma pura y exhaustiva).
Queda **sin cobertura** el **cableado WPF** que une esos estados puros con las ventanas/VM reales:

- el **`RackFrameConfiguratorViewModel`** (God-ViewModel del configurador de cabecera, archivo
  caliente ~2,550 líneas) **no tiene ninguna prueba**: sus mutaciones estructurales (altas/bajas/
  división/combinación de horizontales, arreglos de celosía), su recomputación **síncrona** del
  modelo físico y sus rutas negativas (elevación duplicada, guarda de dos horizontales, combinación,
  división sin selección) son hoy inalcanzables por prueba;
- la **adopción del estado dinámico por su ventana** (`RackDynamicSystemWindow` sobre
  `DynamicFrontMatrix`/`DynamicEditorDesignAssembler`, I-21) no tiene una caracterización `load→build`
  análoga a la del selectivo (`SelectiveEditorStateAdoptionTests`, I-20);
- la **identidad round-trip a nivel de ventana** (carga nueva ⇒ GUID nuevo al insertar; carga
  existente ⇒ GUID preservado; `UpdateOnly` limpia vista/sección) sólo está probada en el helper puro
  del shell (`RackEditorSessionTests`/`RackEditorIdentityTests`) y en la adopción del shell
  (`EditorShellAdoptionTests`, que usa `Identity.Adopt` directo), **no** por las rutas `LoadForNew`/
  `LoadExisting`/`LoadDesignForNew` de las ventanas reales.

## 3. Alcance

Autorizado por el ROADMAP (Fase 5, I-24: "Tests de ViewModels y estados de editor sobre
`tests/RackCad.UI.Tests`") y por este objetivo. **Todo el código nuevo de prueba vive en
`tests/RackCad.UI.Tests`.** No se añaden pruebas a `RackCad.Tests` (evitar duplicar las pruebas puras
exhaustivas ya existentes de `SelectiveEditorState`/`DynamicFrontMatrix`/`DynamicEditorDesignAssembler`).

1. **`RackFrameConfiguratorViewModel`** (headless, sin Dispatcher): construcción desde el diseño por
   defecto (`HardcodedStandardRackFrameService().CreateDefault()`); mutaciones estructurales
   (`AddCommonSegment`, `AddHorizontalSegment`, `DuplicateSelectedHorizontal`,
   `DeleteSelectedHorizontal`, `SplitSelectedSegment`, `CombineSelectedSegments`); arreglos de
   bracing (`ApplyDoubleBracingToSelection`/`ApplyNoBracingToSelection`); recomputación **síncrona**
   del modelo físico (`Configuration.Members` tras cada edición); BOM (`BuildBom`); persistencia
   round-trip a través del VM (`SaveProjectTo`/`LoadProjectFrom` a una ruta temporal); y rutas
   negativas (elevación duplicada rechazada, guarda de mínimo dos horizontales, división/combinación
   guardadas, parse de dimensiones inválidas ignorado).

2. **Ventana dinámica** (`RackDynamicSystemWindow`, STA):
   - **Caracterización `load→build` con firma REALMENTE COMPLETA del dibujo** producido por Application: todos
     los cortes de `DynamicSystemLateralBuilder` (cada uno con su índice de corte), el frontal de salida
     (`DynamicSystemFrontalBuilder`+`DynamicRackEnd.Exit`), el frontal de entrada (`Entrance`) y la planta
     (`DynamicSystemPlantaBuilder`), **TODA instancia —bloques estructurales E decoraciones `Annotation`/
     `Dimension`—** (rol, `PieceId`, bloque, vista, inserción, anclaje, rotación, ambos mirrors, parámetros
     dinámicos, y para anotaciones/cotas también `Text`, `DimensionOffset` y `DimensionStyleName`), ordenada
     determinísticamente. Las anotaciones que dependen del **nombre visible** (p. ej. `DrawRackName`) se
     reconcilian **normalizando el `Name`**: para comparar `request.Design` con `request.System` se resuelve el
     diseño y se asigna al sistema resuelto el mismo `Name` de `request.System` **antes** de construir la firma
     (la ventana fija ese nombre en el sistema tras resolver). El **diseño representativo es NO default**: 3
     frentes, niveles distintos por frente, tarima/postes/altura no default, **valores NO default por celda/
     larguero** —frente/alto/peso de tarima, claro y `BeamLengthOverride`— inyectados en la celda de matriz de un
     frente+nivel no trivial, **y opciones de anotación NO default** (`NumberFronts`/`NumberLevels`/
     `DrawRackName`/`AnnotationScale`/`Dimensions`). Como la ventana normaliza entradas crudas al construir
     (resuelve el peralte del larguero IN/OUT y el intermedio en `Recompose`), la fidelidad se fija sobre el
     **diseño propio de la ventana** (punto fijo del doble build: cargar → `designA`, recargar `designA` →
     `designB`, firma completa —incluidas anotaciones— idéntica), por `LoadExisting` y por `LoadDesignForNew`;
     se verifica **expresamente** que los valores no default por celda **y las opciones de anotación** estén
     presentes en `designA` y sigan presentes en `designB`. Incluye una **prueba de sensibilidad**: dos diseños
     con igual número de frentes/módulos/longitud/altura pero una pieza dibujada distinta (una bota de seguridad
     añadida) producen **la misma firma débil de 4 agregados** pero **firma completa distinta**.
   - **Inserción/actualización por los handlers REALES de la ventana** (Click WPF real vía `RaiseEvent`
     sobre `InsertLateralButton`/`UpdateButton`/`InsertEntranceButton`, que recorren
     `*_Click`→`RequestDraw`→validación→`Recompose`→`SetModel`→sesión→payload→`Close`), **no** llamando a
     `session.RequestInsert/RequestUpdate`: dinámico nuevo inserta lateral; dinámico existente actualiza e
     inserta frontal de entrada. Tras cada acción se verifica `InsertRequested`, GUID, **nombre**, vista,
     sección, `UpdateOnly`, el **tipo concreto** de `InsertionRequest`, la **correspondencia estricta** del
     payload (la firma completa —incluidas anotaciones/cotas— construida desde `request.Design` —resolviéndolo y
     normalizando el `Name` a `request.System.Name`— iguala la construida directamente desde `request.System`,
     con una sobrecarga `FullDrawingSignature(DynamicRackSystem)` que no re-resuelve el sistema recibido), y la
     metadata de origen (I-11) preservada.
   - Se conserva aparte una prueba pura de `LoadExisting` (adopta GUID+nombre) que aporta cobertura distinta.

3. **Ventana selectiva** (`RackSelectiveWindow`, STA): inserción/actualización por los handlers REALES
   (Click WPF real sobre «Insertar frontal»/`UpdateButton`/`InsertLateralButton`, que recorren
   `RequestDraw`→`ConfirmPendingCellEdits`→`BuildSystem`→sesión→payload→`Close`): selectivo nuevo inserta
   frontal (GUID fresco); selectivo existente actualiza (GUID+nombre preservados, vista nula) e inserta una
   vista ligada (lateral). Verifica `InsertRequested`, GUID, **nombre**, vista, `UpdateOnly`, tipo concreto de
   `InsertionRequest` y la **correspondencia estricta** del payload por firma del dibujo resuelto (frontal de
   todos los fondos + planta + cortes laterales, **TODA instancia incluidas anotaciones y dimensiones** con
   `Text`/`DimensionOffset`/`DimensionStyleName`) construida desde `request.Design` (`SelectiveGeometryResolver`,
   normalizando `resolved.Name = request.System.Name`) vs. desde `request.System`. Se conserva aparte la prueba
   pura de `LoadExisting`. **No** se duplica la caracterización `load→build` ya existente (`SelectiveEditorStateAdoptionTests`).

4. **Ventana de cama** (`RackFlowBedWindow`, STA): inserción por el handler REAL
   (`InsertInAutoCad_Click`→`ReadConfig`→sesión→payload→`Close`): cama nueva y cama existente, verificando
   `InsertRequested`, GUID, **nombre**, `view == null`, `section == -1`, `UpdateOnly == false`, el tipo
   concreto `FlowBedInsertionRequest` y los **valores concretos** del `FlowBedConfiguration` producido por el
   handler (tipo, `LaneDepth`, `PalletDepth`, `RollerId`, `RollerPitchOverride`) contra el fixture, más la
   **metadata de origen `SourceDocument` I-11 transportada** por el handler real. Se conserva aparte la
   identidad round-trip pura (`LoadForNew`/`LoadExisting`) y la exposición de `SourceFlowBedToInsert`.

### Cambio de producción autorizado (único seam interno)

Para caracterizar la adopción del estado dinámico por su ventana (punto 2) se autoriza **un solo**
cambio de producción, **sin reglas nuevas**:

- En `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs`, añadir un seam **interno** de prueba
  `internal DynamicRackDesign BuildDesignForTest(out bool ok)` que **reenvía** al método privado
  existente `Recompose()` y devuelve el campo privado `design` que éste ya construye:
  `{ ok = Recompose(); return design; }`. Es el equivalente exacto del seam ya presente en el
  selectivo (`RackSelectiveWindow.BuildDesignForTest`, I-20, línea 176) y **no** introduce lógica de
  producto: no calcula geometría, no cambia el flujo de inserción, no se usa en producción y sólo es
  visible para `RackCad.UI.Tests` vía el `InternalsVisibleTo` ya existente. Cualquier necesidad de un
  seam adicional (o de tocar reglas) es motivo de **detención por alcance** (§12).

## 4. Fuera de alcance

- Cambiar **XAML**, apariencia, geometría, BOM, persistencia, handlers, Draw Services, catálogos,
  bloques, reglas de producto o cualquier función visible. Si probar algo exigiera un cambio
  funcional, **detenerse con gate de alcance** (§12).
- Añadir pruebas a `RackCad.Tests` o **duplicar** las pruebas puras exhaustivas de
  `SelectiveEditorState`/`DynamicFrontMatrix`/`DynamicEditorDesignAssembler`/`RackEditorSession`/
  `RackEditorIdentity`.
- Migrar el `RackFrameConfiguratorWindow` al Editor Shell (usa su propio mecanismo de inserción por
  banderas; migrarlo es trabajo de otra iniciativa), o migrar el larguero.
- Corregir hallazgos ajenos "de paso": la **entrada obsoleta de I-21 en
  `docs/initiatives/README.md`** (declara la iniciativa "No integrada" cuando ya está integrada) y las
  **lagunas de cobertura pura** en `RackCad.Tests` (p. ej. `DynamicFrontMatrix.ApplyScope` para los
  alcances `Level`/`Front`) se **registran** como hallazgos (§14, `docs/ideas-futuras.md`), no se
  corrigen aquí.
- Dependencias NuGet nuevas (política cero-NuGet, ADR-0003) o paquetes de test nuevos. El proyecto
  `RackCad.UI.Tests` ya trae xunit + Test SDK + coverlet.
- Cualquier seam de producción distinto del único autorizado en §3.

## 5. Contexto requerido

- `AGENTS.md` (dirección de dependencias Domain←Application←UI←Plugin, definición de terminado,
  archivos calientes) y `docs/WORKFLOW.md` (ciclo de iniciativa, cierre §5).
- `docs/ROADMAP.md` (I-24: Fase 5, depende de I-15/I-20; el objetivo añade la adopción dinámica de
  I-21) y `docs/ARCHITECTURE.md` §5/§7.3 (UI y estado de editor).
- Context Packs: `architecture-kernel`, `ui-editors`, `system-selective`, `system-dynamic-flowbed`,
  `persistence`, `delivery-validation`, `documentation-governance`.
- Precedentes de prueba: `tests/RackCad.UI.Tests/StaTestRunner.cs` (runner STA compartido),
  `EditorShellAdoptionTests.cs`, `SelectiveEditorStateAdoptionTests.cs`, `RackEditorSessionTests.cs`,
  `RackEditorIdentityTests.cs`; suites puras `tests/RackCad.Tests/SelectiveEditorStateTests.cs`,
  `DynamicFrontMatrixTests.cs`, `DynamicEditorDesignAssemblerTests.cs` (para delimitar la no
  duplicación).
- Código: `src/RackCad.UI/RackFrameConfiguratorViewModel.cs`, `RackFrameConfiguratorWindow.xaml.cs`,
  `RackDynamicSystemWindow.xaml.cs`, `RackSelectiveWindow.xaml.cs`, `RackFlowBedWindow.xaml.cs`,
  `src/RackCad.UI/Editor/*` (shell), `src/RackCad.Application/RackFrames/*` (configurador),
  `src/RackCad.Application/Systems/Dynamic*`/`Selective*` (estado puro y resolvers).

## 6. Dependencias

- **Integradas requeridas:** I-15 (`architecture/editor-shell`), I-20 (`refactor/selective-editor-state`)
  e I-21 (`refactor/dynamic-editor-state`), todas en `main` (`9a895e4`, Merge I-20). También I-14
  (proyecto `RackCad.UI.Tests` + runner STA) está en `main`. El ROADMAP declara la dependencia de I-15
  e I-20; I-21 se añade porque el objetivo cubre explícitamente la adopción del estado **dinámico** por
  su ventana, y está integrada.
- **Conflictos declarados:** ninguno (ROADMAP, columna "se estorba con" vacía).
- **Trabajo paralelo observado (no conflictivo):** durante el preflight se detectó una sesión de
  **I-22** (`refactor/safety-placement`, reclamo `0f39d9d` sobre `9a895e4`) recién publicada. I-22 no
  es conflicto declarado de I-24; toca producción del selectivo/seguridad, mientras I-24 sólo añade
  pruebas y un seam en el **dinámico** (archivo distinto). Para minimizar fricción de integración, I-24
  **no** añade seams al archivo caliente `RackSelectiveWindow.xaml.cs` (reutiliza su seam existente).
- **Entradas del dueño:** ninguna. No hay `docs/automation/decisions/I-24.md` ni `OWNER-DECISIONS.md`.
  Al ser una iniciativa exclusivamente de pruebas y seams sin comportamiento, `requires_autocad: false`
  y `requires_owner_validation: false` (§10).

## 7. Archivos esperados

Crear (pruebas, `tests/RackCad.UI.Tests/`):

- `EditorWindowTestSupport.cs` — helper interno de prueba: dispara el Click WPF **real** de un botón
  (por `x:Name` o por contenido) vía `RaiseEvent(ButtonBase.ClickEvent)` y setea texto en un `TextBox`
  nombrado. No es producción; sólo localiza el botón existente y levanta su evento.
- `RackFrameConfiguratorViewModelTests.cs` — VM headless: mutaciones estructurales (incl.
  `AddHorizontalSegment`), recomputación síncrona, bracing, selección múltiple, BOM, persistencia
  round-trip, rutas negativas (elevación duplicada, guarda de dos horizontales, división sin selección,
  dimensiones inválidas ignoradas).
- `DynamicEditorWindowTests.cs` — STA: caracterización `load→build` con **firma completa del dibujo**
  (cortes laterales + frontal salida/entrada + planta, por instancia) sobre un diseño no default, por
  punto fijo del doble build (`LoadExisting` y `LoadDesignForNew`); prueba de sensibilidad firma
  débil vs. completa; e inserción/actualización por los **handlers reales** de la ventana.
- `SelectiveEditorWindowTests.cs` — STA: inserción/actualización por los **handlers reales** (frontal
  nuevo, actualizar/lateral existentes) + identidad pura de `LoadExisting`.
- `FlowBedEditorWindowTests.cs` — STA: inserción por el **handler real** (cama nueva/existente, metadata
  de origen I-11) + identidad round-trip pura y exposición de `SourceFlowBedToInsert`.

Modificar (producción, único seam autorizado en §3):

- `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs` — añadir el seam interno `BuildDesignForTest`.

Modificar (documentación):

- `docs/initiatives/I-24-ui-tests-editores.md` (este contrato), `docs/initiatives/README.md` (índice,
  **sólo** añadir la entrada de I-24; no tocar la de I-21), `docs/automation/state/I-24.yml` (estado
  versionado), `docs/ideas-futuras.md` (registro de hallazgos, §14).

No se espera modificar el XAML, `RackFrameConfiguratorViewModel`/`RackSelectiveWindow`/`RackFlowBedWindow`
ni ningún archivo de Domain/Application/catálogos/persistencia/DrawServices/deploy/`.github/workflows`.
Una desviación material obliga a detenerse (§12).

## 8. Fases

1. **Reclamo y contrato.** Rama + worktree desde `origin/main`, commit de reclamo + push, contrato
   desde `TEMPLATE.md`, índice y estado versionado. (Evidencia: rama remota aceptada, este archivo.)
2. **Seam dinámico + pruebas de ventanas.** Añadir el seam `BuildDesignForTest` al dinámico; escribir
   `DynamicEditorWindowTests`, `SelectiveEditorWindowTests`, `FlowBedEditorWindowTests` (STA).
   (Evidencia: suite UI verde.)
3. **Pruebas del ViewModel del configurador.** `RackFrameConfiguratorViewModelTests` (headless).
   (Evidencia: suite UI verde.)
4. **Gates automatizados.** `RackCad.Tests` (sin cambio), `RackCad.UI.Tests` (con las nuevas), builds
   Debug de UI/Plugin/solución, CI de la rama sobre el SHA publicado. (Evidencia: §14.)
5. **Cierre de sesión.** Revisión de diff, commits lógicos, push de la rama, estado versionado,
   hallazgos registrados. (Evidencia: §14.)

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug` — suite completa verde, **sin
  regresión** (I-24 no la toca).
- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug` — suite UI verde con las
  pruebas nuevas, en el runner STA existente.
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` — 0 errores, 0 advertencias propias.
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` — 0 errores propios (sólo los
  `MSB3277` conocidos; requiere las referencias de AutoCAD 2025 del entorno).
- `dotnet build RackCad.sln -c Debug` — build completo de la solución.
- CI: los cuatro jobs (Tests, Build UI, UI Tests, Build Plugin sin AutoCAD) en verde sobre la punta
  publicada de la rama.

## 10. Validacion manual

**No aplica.** I-24 es una iniciativa exclusivamente de pruebas y de un seam interno **sin
comportamiento**: no cambia dibujo, geometría, BOM, persistencia, handlers, catálogos ni la
UI/apariencia. Por eso `requires_autocad: false` y `requires_owner_validation: false`. Si el diff real
invalidara esa premisa (cualquier cambio de comportamiento o de superficie visible), **no** se declaran
los gates cerrados: se detiene y se explica la contradicción (§12). La cobertura se sostiene con las
suites automatizadas y el CI verde de la rama.

## 11. Criterios de aceptacion

- `tests/RackCad.UI.Tests` gana pruebas nuevas —del `RackFrameConfiguratorViewModel`, de la adopción
  del estado dinámico por `RackDynamicSystemWindow`, y de la identidad/inserción round-trip de las
  ventanas selectiva/cama— que **pasan** en el runner STA existente, son **deterministas** (sin
  `Sleep`, sin temporización frágil, sin píxeles/screenshots, sin depender del orden global) y cada una
  **indica qué regresión observable detectaría**.
- Las pruebas **no duplican** las pruebas puras exhaustivas de `RackCad.Tests`.
- El único cambio de producción es el seam interno `BuildDesignForTest` del dinámico, que **reenvía** a
  `Recompose` sin reglas nuevas y no se usa en producción.
- `RackCad.Tests` sigue **verde sin cambios**; `RackCad.UI.Tests` verde; builds Debug de UI (y
  Plugin/solución donde el entorno lo permite) en 0 errores propios; CI verde en los cuatro jobs.
- La dirección de dependencias se conserva (las pruebas UI no referencian AutoCAD; el seam vive en UI)
  y no hay dependencias NuGet nuevas.

## 12. Condiciones para detenerse

- Que cubrir un comportamiento exija un cambio funcional (tocar XAML, geometría, BOM, persistencia,
  handlers, Draw Services, catálogos o reglas de producto): **detenerse con gate de alcance**.
- Que se necesite un seam de producción **distinto** del único autorizado en §3, o que el seam
  autorizado no pueda mantenerse como reenvío sin reglas.
- Que el diff real invalide la premisa "sólo pruebas y seam sin comportamiento": no declarar
  `requires_autocad`/`requires_owner_validation` cerrados; detenerse y explicar.
- Que aparezca en `origin` otra rama tocando de forma incompatible los mismos archivos de prueba o el
  dinámico: no sobrescribir; entregar evidencia.
- Cualquier necesidad de un paquete NuGet nuevo (producto o test).

## 13. Estado versionado y entrega del Pull Request

Estado canónico en `docs/automation/state/I-24.yml`. La automatización está pausada
(`automation.enabled: false`): el ejecutor es manual y mantiene ese archivo al cierre de la sesión. No
se abre un segundo Pull Request ni se activa auto-merge. Al ser una iniciativa de pruebas sin gates de
AutoCAD ni owner-validation, tras CI verde el estado queda `review-ready` con `gate: none`; la
integración a `main` (`git merge --no-ff`, WORKFLOW §4.5) se realiza en la sesión de integración, no en
esta rama. `docs/HANDOFF.md` §8-12 y el estado en `docs/ROADMAP.md` se actualizan **sólo** en la sesión
de integración (último commit de la rama), nunca desde esta rama.

## 14. Evidencia final

Se completa al cierre de la sesión: commits lógicos con trailer de procedencia, archivos creados/
modificados, matriz de cobertura nueva y rutas negativas, seam de producción con justificación,
resultados de `dotnet test` (suite completa y UI) y de los builds, evidencia de CI sobre el SHA
publicado, SHA base y punta de la rama, confirmación del push, y confirmación de que `main` no fue
modificada. **Conteos tras la corrección de revisión:** `RackCad.Tests` **913/913** (sin cambios;
I-24 no toca esa suite) y `RackCad.UI.Tests` **168/168** (139 base + **29 nuevas**: 13 del
configurator VM, 8 del dinámico, 4 del selectivo, 4 de la cama). Builds Debug de UI/Plugin/solución
sin errores propios.

Hallazgos registrados en `docs/ideas-futuras.md` (fuera del alcance de I-24, sin corregir):
(1) la **entrada obsoleta de I-21** en `docs/initiatives/README.md` (la declara "No integrada" cuando
ya está integrada en `main`); (2) una **laguna de cobertura pura comprobada** en `RackCad.Tests`:
`DynamicFrontMatrixTests` prueba los alcances `Cell`/`All`/`Selected` de `DynamicFrontMatrix.ApplyScope`
pero **no** los alcances `Level` ni `Front` (ambos existen en `DynamicRackCellScope`). Las demás
lagunas que una revisión previa había supuesto (archivo dedicado de `DynamicEditorSafety`/
`DynamicEditorCell`, guardas de `RestoreHeaderFondos`, ramas de `AnnotationScale`) se **descartaron al
verificarlas contra los archivos reales**: `DynamicEditorCellTests.cs` y `DynamicEditorSafetyTests.cs`
sí existen y cubren esas clases.
