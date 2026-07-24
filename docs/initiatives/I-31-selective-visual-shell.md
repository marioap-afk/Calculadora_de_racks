---
schema: rackcad-initiative/v1
id: I-31
title: Migración del editor Selectivo al shell visual común
type: refactor
status: validating
branch: refactor/selective-visual-shell
base_branch: main
priority:
size:
depends_on: [I-30]
conflicts_with: [I-18]
context_packs:
  - ui-editors
  - architecture-kernel
  - delivery-validation
  - documentation-governance
automation_state_path: docs/automation/state/I-31.yml
decision_paths:
  - docs/adr/0019-shell-visual-de-editores-por-composicion.md
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

# I-31 — Migración del editor Selectivo al shell visual común

> Alcance autorizado por `docs/ROADMAP.md` (Fase 5, fila I-31) y por el ADR-0019 **ya aceptado**
> (migración progresiva Dinámico → **Selectivo** → Push Back). Segundo eslabón de la secuencia
> obligatoria **I-30 → I-31 → reanudación de I-18**. Este contrato NO amplía el ROADMAP ni reabre el ADR.

## 1. Objetivo

`RackSelectiveWindow` se **compone realmente sobre `RackEditorVisualShell`** (I-30) por composición y
slots, tal como ya lo hace `RackDynamicSystemWindow`, **sin cambio de dibujo, BOM, GUID, persistencia,
handlers ni comportamiento del Selectivo**. La segunda composición exterior propia del Selectivo (grid
principal 342 px + scroll exterior + disposición independiente de matriz/preview/status + barra
inferior propia) desaparece y su lugar lo toman los slots del shell y el contrato de tamaño común
`EditorShellWindowStyle`.

Verificable al cerrar:

1. La raíz real de la ventana es `RackEditorVisualShell` (`window.Content is RackEditorVisualShell`,
   idéntico al campo `Shell`).
2. Los **44 `x:Name`** del Selectivo siguen resolviendo, cada control queda en su slot correcto y no
   queda ninguna composición exterior duplicada.
3. Tamaño inicial/mínimo, fondo y tipografía provienen de los tokens del shell
   (`EditorShellWindowStyle`); la paleta reutiliza los tokens `Shell*Brush` (sin duplicar hex).
4. Geometría, BOM, GUID, nombre, round-trip, metadata I-11, seguridad, previews e inserción/
   actualización quedan **idénticos** (fijados por las pruebas STA por handler real y de estado).

## 2. Problema

Tras I-30, **Selectivo es el único de los dos editores grandes ya extraídos (I-20/I-21) que sigue
fuera del shell**. Medido sobre `main` = `40a2c8e`:

- `RackSelectiveWindow.xaml` (307 líneas) reimplementa a mano lo que el shell ya provee: un `Grid`
  raíz de dos filas × tres columnas con un panel lateral de **ancho fijo 342 px** dentro de su propio
  `ScrollViewer`, una disposición independiente de matriz (arriba) + preview frontal (abajo), el
  bloque de estado (`SummaryText`/`StatusText`) **al fondo del scroll del panel** y una barra de
  acciones inferior propia (`StackPanel` `HorizontalAlignment=Right`).
- Sus superficies usan hex incrustados (`#EEF2F6` de fondo de ventana, `White`/`#D8DEE6` de las
  tarjetas, `#0E1B2A` del preview, `Segoe UI`) que **ya existen como tokens** del shell
  (`ShellWindowBackgroundBrush` `#EEF2F6`, `ShellSurfaceBrush` `#FFFFFF`, `ShellBorderBrush` `#D8DEE6`,
  `ShellPreviewBackgroundBrush` `#0E1B2A`, `ShellFontFamily` `Segoe UI`, `ShellPanelPadding` `14`).
- Divergencia con el Dinámico ya migrado: distinto ancho de panel (342 vs 430 por token), distinta
  disposición del status, tamaños hardcodeados (`Height=740 Width=1300 MinHeight=600 MinWidth=1060`)
  frente al contrato común (`1280×720`, mín. `1120×672`). Esa divergencia es exactamente la que el
  Owner rechazó en el gate de I-18 (PB-VAL-01) y la que el shell existe para cerrar.

`ADR-0019` (aceptado) y `ARCHITECTURE.md` §7.3 declaran el shell como arquitectura vigente y ordenan
migrar el Selectivo **después** del Dinámico. La brecha es de **adopción**, no de diseño.

## 3. Inventario comparativo (auditoría en lectura)

`Selectivo` medido en `main` = `40a2c8e`; `Dinámico` es la **referencia ya migrada** en ese mismo
commit; `Push Back` leído **solo** desde `origin/feature/push-back` = `b2d9e9d` con `git show` (sin
checkout, sin merge, sin rebase, sin commit).

| Concepto | Selectivo (a migrar) | Dinámico (migrado, referencia) | Compartible / acción I-31 |
|---|---|---|---|
| Raíz | `Grid` 2×3 con `Margin=16` | `shell:RackEditorVisualShell x:Name="Shell"` | La raíz pasa a ser el shell; se elimina el `Grid` externo |
| Ventana | `Height/Width/MinHeight/MinWidth/Background/FontFamily` hardcodeados | `Style="{DynamicResource EditorShellWindowStyle}"` | Aplicar el estilo compartido; borrar los sizes/fondo/tipografía a mano |
| Panel lateral | `ScrollViewer` propio, ancho **342 px** | `SidePanelContent`, el shell provee el scroll (ancho 430 por token) | `SidePanelContent`; se quita el scroll propio (lo da el shell) |
| Matriz | `MatrixGrid` + `FondoSelectorPanel` en un `Border` propio (fila 0 derecha) | `DynamicMatrixGrid` en `MatrixContent` | `MatrixContent` (incluye el selector de fondo, propio del Selectivo) |
| Preview | `Canvas #0E1B2A` + radios Frontal/Lateral + leyenda, `Border` propio (fila 2 derecha) | `Canvas` en `PreviewContent` (fondo por token) | `PreviewContent`; el `Canvas` y el painter actuales se alojan tal cual |
| Estado | `SummaryText`/`StatusText` al fondo del scroll del panel | `SummaryText`/`StatusText` en `StatusContent` (banda fija fuera del scroll) | `StatusContent` (se elimina la disposición independiente del status) |
| Acciones | `StackPanel` inferior propio, 7 botones a la derecha | 4 categorías neutrales del shell | Categorías neutrales (§Acciones), **sin reordenar** |
| Selección | **1 celda** (`selBay`/`selLevel`) + alcance Celda/Nivel/Frente/Todas | **multiselección** (Ctrl+clic, «Seleccionadas») | **Se conserva la selección simple** (ver §3.3) |
| Sesión/estado | `RackEditorSession` + `SelectiveEditorState` (I-20) | `RackEditorSession` + `DynamicFrontMatrix`/assembler (I-21) | **Ya compartido**; el shell no lo sustituye |

### 3.1 `x:Name`, handlers y controles específicos que se conservan

- **44 `x:Name`** en el XAML del Selectivo (todos deben seguir resolviendo tras el reparent):
  `NameBox`; anotaciones `DrawBasePlateCheck`/`NumberFrontsCheck`/`NumberLevelsCheck`/
  `DrawRackNameCheck`/`DrawPalletsCheck`/`AnnotationScaleBox`/`DimensionsBox`/`DimStyleBox`;
  seguridad `SafetyButton`; cabecera `PostBox`/`PostPeralteBox`/`PostSelectBox`/`CustomizePostButton`/
  `PostPeralteOverrideBox`/`PostCabeceraStatus`; tramo `BayCountBox`; tolerancias `ToleranceBox`/
  `ClearanceBox`/`FloorRiseBox`/`FondoBox`/`CabeceraFondoBox`/`FondosBox`/`SeparatorsSection`/
  `SeparatorsHost`; editor de celda `CellHeader`/`CellBeamBox`/`FrenteBox`/`PalletCountBox`/`AltoBox`/
  `BeamPeralteCombo`/`BeamLenBox`/`ClearBox`; estado `SummaryText`/`StatusText`; matriz
  `FondoSelectorPanel`/`FondoSelectorBox`/`MatrixGrid`; preview `PreviewFrontalRadio`/
  `PreviewLateralRadio`/`PreviewHint`/`PreviewCanvas`; acciones `UpdateButton`/`InsertLateralButton`/
  `InsertPlantaButton`.
- **31 handlers** referenciados por el XAML (todos existen en el `.cs`): `DrawToggle_Changed`,
  `Dimensions_Changed`, `Safety_Click`, `Post_Changed`, `GlobalScalar_LostFocus`/`_KeyDown`,
  `PostSelect_Changed`, `CustomizePost_Click`, `PostPeralteOverride_LostFocus`/`_KeyDown`,
  `ResetPost_Click`, `BayCount_LostFocus`/`_KeyDown`, `FondoDepth_LostFocus`, `Fondos_LostFocus`,
  `Update_Click`, `ApplyCell_Click`, `ApplyRow_Click`, `ApplyColumn_Click`, `ApplyAll_Click`,
  `FondoSelector_Changed`, `PreviewView_Changed`, `PreviewCanvas_SizeChanged`, `UpdateExisting_Click`,
  `ShowBom_Click`, `SaveToLibrary_Click`, `InsertFrontal_Click`, `InsertLateral_Click`,
  `InsertPlanta_Click`, `Close_Click`.
- **Handlers sensibles**: el botón **«Insertar frontal» NO tiene `x:Name`** (se localiza por
  `Content`); su texto exacto debe conservarse (lo usa `SelectiveEditorWindowTests`). El botón
  **«Recalcular tramo» (sidebar)** deliberadamente **no** es `IsDefault` (evita que Enter en cualquier
  caja revierta lo tecleado): esa ausencia se conserva. El constructor lee `UpdateButton.ToolTip`/
  `InsertLateralButton.ToolTip`/`InsertPlantaButton.ToolTip` justo tras `InitializeComponent()` y
  `UpdateInsertButtons()` los intercambia por el motivo de indisponibilidad; esa lógica no se toca.

### 3.2 Archivos calientes, riesgos de recorte y frontera

- **Archivo caliente**: `RackSelectiveWindow.xaml` (se reescribe la composición exterior).
  `RackSelectiveWindow.xaml.cs` (~2 205 líneas, hotspot de WORKFLOW §7) **se mantiene intacto** salvo
  adaptación mínima inevitable (no se prevé ninguna). El `.cs` no navega el árbol visual/lógico
  (`.Parent`/`VisualTreeHelper`/`LogicalTreeHelper` no aparecen), así que reparentar en slots es seguro.
- **Riesgo de recorte**: el shell ya resolvió el layout mínimo en I-30 (`ShellMinHeight` 672, work-area
  con `ClipToBounds`, sidebar con scroll, action bar `WrapPanel`). El Selectivo hereda esa robustez al
  aplicar `EditorShellWindowStyle`; las pruebas lo fijan mostrando la ventana real al mínimo.
- **Frontera dura**: el shell solo controla presentación. `SelectiveEditorState` sigue siendo la
  autoridad del estado y `RackEditorSession` la de identidad, recomputación e inserción. **Cero
  `RackSystemKind`** y **cero ramas por sistema** en el shell (invariante de I-30, no se altera).

### 3.3 Comportamiento de selección (confirmado en lectura — discrepancia documental registrada)

**`main` NO implementa multiselección en la matriz principal del Selectivo.** El editor selectivo usa
**selección de una sola celda** (`state.SelBay`/`state.SelLevel`) y aplica por **alcance** con cuatro
botones «Aplicar a:» **Celda / Nivel / Frente / Todas** (`ApplyCell_Click`/`ApplyRow_Click`/
`ApplyColumn_Click`/`ApplyAll_Click`). No hay Ctrl+clic, ni botón «Seleccionadas», ni conjunto de
celdas seleccionadas (grep de `Ctrl`/`Keyboard.Modifiers`/`SelectedCells`/`Seleccionad` en el `.cs` =
**0 coincidencias**). El **Dinámico sí** tiene multiselección (Ctrl+clic + «Seleccionadas»). **I-31
conserva la selección simple del Selectivo tal cual y NO agrega multiselección**: sería una capacidad
nueva, fuera de una migración puramente visual. Esta divergencia Selectivo↔Dinámico queda registrada
como decisión consciente, no como defecto a corregir aquí.

### 3.4 Decisiones de composición registradas

- **Status en `StatusContent`**: `SummaryText`+`StatusText` pasan a la banda de estado fija (como el
  Dinámico), eliminando la disposición independiente del status. `SetStatus` conserva
  `StatusText.BringIntoView()` para el caso de error; fuera del scroll del panel esa llamada es un
  **no-op inofensivo** (el status ya es siempre visible). Su comentario en el `.cs` (~línea 2197)
  describe la ubicación previa; se deja **sin tocar** para mantener la migración exclusivamente XAML
  (artefacto cosmético conocido, adoptable por una limpieza posterior del `.cs`).
- **Paleta por token**: superficies/borde/fondo de preview usan `Shell*Brush`; la **leyenda del
  preview** (swatches `#3DC986`/`#E08A2B`/`#B7C3CF`/`#2E9C66`) y el **painter** conservan sus colores
  actuales (son proyección específica del sistema; no se tokenizan para no arriesgar desalineación con
  `PreviewCanvasPainter`).
- **`PreviewCanvas` (control I-14) NO se adopta**: no existe prueba de equivalencia completa de
  proyección y contenido frente al `Canvas`+`Map()`/painter actuales; se aloja el `Canvas` tal cual
  (misma decisión que I-30 para el Dinámico).

### 3.5 Efecto sobre I-18 (handoff, no ejecución)

Tras integrar I-31, **Push Back será el único editor fuera del shell**. Leído en `b2d9e9d`,
`RackPushBackSystemWindow.xaml` **imita a mano** la estructura del shell (`DockPanel` + `ScrollViewer`
`Dock=Left Width=430` + `Grid x:Name="WorkArea"` + barra de acciones inferior) con tamaños
hardcodeados `Height=720 Width=1280 MinHeight=640 MinWidth=1120` (**mín. 640 pre-672**) y **ya adopta
`NumericField`/`CatalogCombo`**. Su migración futura al shell es un **reparent de layout** (como
Selectivo/Dinámico) que **no** necesita adoptar controles de captura (ya adoptados) y que subiría su
mínimo a 672. El conflicto textual al rebasar I-18 es mínimo (archivos nuevos; compartidos:
`RackMainMenuWindow.xaml(.cs)` y `EditorModuleRegistryTests.cs`); el trabajo real es semántico. **I-31
solo deja registrado este handoff**; no toca `feature/push-back`.

## 4. Alcance

1. Reescribir la **composición exterior** de `RackSelectiveWindow.xaml` para consumir
   `RackEditorVisualShell` por los slots `SidePanelContent`, `MatrixContent`, `PreviewContent`,
   `StatusContent` y las categorías `LeadingActions`/`SecondaryActions`/`PrimaryActions`/
   `TrailingActions`; aplicar `Style="{DynamicResource EditorShellWindowStyle}"` y los tokens
   estructurales comunes; **alojar los controles existentes tal cual** en los slots.
2. **Suite estructural/STA** de la migración Selectiva (patrón de `DynamicShellMigrationTests`).
3. Contrato, estado versionado y entrada de índice de I-31.

**Preferencia por adaptación exclusivamente XAML.** No se modifica `RackSelectiveWindow.xaml.cs` salvo
una adaptación mínima inevitable, demostrada y sin cambio funcional (no se prevé ninguna). No se
sustituyen entradas por `NumericField`/`CatalogCombo` como efecto colateral.

## 5. Fuera de alcance

- **Multiselección** de la matriz principal del Selectivo (§3.3): no se agrega.
- **Push Back** / `feature/push-back`: **solo lectura**; ni un commit, checkout, merge o rebase. El
  handoff se documenta, no se ejecuta. **I-18, I-23, I-25 y PB-VAL-06**: no se implementan.
- Geometría, resolvers, BOM, persistencia, GUID, comandos, handlers, seguridad, catálogos, Domain,
  Application y Plugin: **prohibido** tocarlos.
- Sustitución de controles de captura por los de I-14; adopción del control `PreviewCanvas`;
  tokenización del painter/leyenda del preview.
- Dependencias NuGet nuevas.
- `docs/HANDOFF.md` y `docs/ROADMAP.md`: los actualiza la sesión de integración (tras aprobación del
  Owner), no esta corrida.

## 6. Dependencias y conflictos

- Integrada requerida: **I-30** (fundación del shell + Dinámico migrado). Transitivamente I-14/I-15/
  I-20/I-22/I-24 (integradas).
- Conflicto por orden: la **reanudación de I-18** espera la integración de I-31.
- Owner: **ADR-0019 ya aceptado** cubre esta migración; **no** se requiere una decisión nueva
  (`requires_owner_decision: false`). Sí se requiere **owner-validation** + **AutoCAD** al cerrar.

## 7. Archivos esperados

Modificar:

- `src/RackCad.UI/RackSelectiveWindow.xaml` — composición sobre el shell + `EditorShellWindowStyle`.

Crear:

- `tests/RackCad.UI.Tests/SelectiveShellMigrationTests.cs` — suite estructural/STA de la migración.
- `docs/initiatives/I-31-selective-visual-shell.md` (este contrato), `docs/automation/state/I-31.yml`,
  entrada en `docs/initiatives/README.md`, y una nota de corrida en `docs/automation/runs/`.

**Hotspots que NO deben aparecer en el diff de I-31**: `RackSelectiveWindow.xaml.cs` (salvo adaptación
mínima inevitable, que no se prevé), `RackDynamicSystemWindow.*`, `RackPushBackSystemWindow.*`,
`RackFlowBedWindow.*`, el shell (`src/RackCad.UI/Shell/**`, `Themes/**`), Plugin, Application/Domain de
geometría/BOM/persistencia, catálogos.

## 8. Fases

1. **Preflight + reclamo** — gates de WORKFLOW verdes; worktree desde `origin/main`; rama; commit de
   reclamo; primer push.
2. **Auditoría + contrato** — inventario comparativo, `x:Name`/handlers, selección confirmada, este
   contrato + estado + índice.
3. **Migración XAML** — `RackSelectiveWindow.xaml` se compone sobre el shell; `.cs` intacto.
4. **Pruebas** — `SelectiveShellMigrationTests`; suites vigentes verdes; filtros dirigidos.
5. **Cierre** — revisión del diff, commits, estado a `validating`, push; DLL Debug del SHA exacto para
   el Owner.

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug`
- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug`
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug`
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug`
- `dotnet build RackCad.sln -c Debug`
- Filtros dirigidos (cada uno descubre ≥1 prueba): Selectivo, Dinámico, shell I-30, estado de editor,
  persistencia, handlers, goldens, validador I-19.
- La suite nueva es **estructural/semántica**, nunca comparación de píxeles: raíz = shell; los 44
  `x:Name` resuelven; cada zona aparece una sola vez y en su slot; sin composición exterior duplicada;
  tamaño/mínimo por tokens; sidebar con scroll común; selector de fondo; matrices dentadas por fondo;
  selección y aplicación por alcance; recomputación; preview frontal y lateral; insertar frontal/
  lateral/planta y actualizar por **handlers reales**; BOM y biblioteca sin alterar; GUID/nombre/
  metadata/round-trip; habilitado/deshabilitado + tooltips; ventana real mostrada al mínimo sin recorte
  ni solape (preview/status/action bar); Dinámico sin regresión.
- Regresión obligatoria: goldens (dinámico/selectivo), persistencia, handlers y validador I-19 **sin
  cambios**.

## 10. Validación manual

Requerida (✋ en ROADMAP). El Owner valida en **AutoCAD 2025** sobre el **DLL Debug construido desde el
SHA exacto** posterior al rebase final (si lo hubiera), comprobando que el editor selectivo migrado
conserva dibujo, BOM, identidad y round-trip, y que la interfaz cumple el contrato visual y queda
alineada con el Dinámico. Sin esa validación la iniciativa no se integra.

## 11. Criterios de aceptación

1. `RackSelectiveWindow` compuesto sobre `RackEditorVisualShell`; raíz = shell.
2. Los 44 `x:Name` resuelven; cada control en su slot correcto; sin composición exterior duplicada.
3. Tamaño inicial/mínimo, fondo y tipografía por `EditorShellWindowStyle`/tokens; paleta por
   `Shell*Brush` sin hex duplicado.
4. Sin cambio de geometría/BOM/GUID/persistencia/handlers/seguridad; selección simple conservada.
5. Suite nueva verde + suites vigentes verdes; goldens y validador intactos; ningún filtro con cero
   pruebas.
6. Diff confinado a UI (solo `RackSelectiveWindow.xaml`), pruebas y documentación de I-31.
7. `feature/push-back` intacta; sin ramas por sistema ni `RackSystemKind` en el shell.
8. DLL Debug validado por el Owner en AutoCAD 2025.

## 12. Condiciones para detenerse

- La migración exigiría tocar geometría, BOM, persistencia, handlers, seguridad o Plugin.
- Aparece necesidad de tocar `feature/push-back`, o de implementar I-18/I-23/I-25/PB-VAL-06.
- Una prueba vigente exige reescritura de comportamiento (no de nombres de contenedor).
- Se requeriría una adaptación del `.cs` que no sea mínima, inevitable y sin cambio funcional.
- `origin/main` avanza con cambios en los hotspots declarados (rebase y re-evaluación).
- El alcance crece más allá de la migración visual del Selectivo.

## 13. Estado versionado y entrega del Pull Request

Estado canónico: [`docs/automation/state/I-31.yml`](../automation/state/I-31.yml). Sin Pull Request
(modo `manual-git-only`). El merge automático está prohibido. Al cerrar la corrida el estado queda
`state: validating`, con los gates `autocad` y `owner-validation` pendientes. `completed`/integrada es
manual y posterior a la aprobación del Owner.

## 14. Evidencia final

Se completa al cerrar la corrida (commits, archivos, pruebas, builds, DLL Debug del SHA exacto,
`AssemblyInformationalVersion`, CI del HEAD publicado, y confirmación de que `main` y `feature/push-back`
no fueron modificadas). Registro de corrida en `docs/automation/runs/`.
