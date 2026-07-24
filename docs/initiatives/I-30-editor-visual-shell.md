---
schema: rackcad-initiative/v1
id: I-30
title: Fundación del shell visual común de editores
type: architecture
status: in-progress
branch: architecture/editor-visual-shell
base_branch: main
priority:
size:
depends_on: [I-14, I-15, I-20, I-21, I-24]
conflicts_with: [I-31, I-18]
context_packs:
  - ui-editors
  - architecture-kernel
  - delivery-validation
  - documentation-governance
automation_state_path: docs/automation/state/I-30.yml
decision_paths:
  - docs/adr/0019-shell-visual-de-editores-por-composicion.md
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-30 — Fundación del shell visual común de editores

> Alcance autorizado por `docs/ROADMAP.md` (Fase 5, fila I-30) con la secuencia obligatoria
> **I-30 → I-31 → reanudación de I-18**. Este contrato NO amplía el ROADMAP.

## 1. Objetivo

Un **shell visual común** (`RackEditorVisualShell`) que fije la composición, los tokens y el
comportamiento visual de los editores ricos, **más la migración real de `RackDynamicSystemWindow`**
como primera adopción y prueba de que el shell sirve para un editor completo.

Verificable al cerrar:

1. Existe el shell con sus slots, su status presenter y su action bar, con pruebas propias.
2. `RackDynamicSystemWindow` se compone sobre el shell **sin cambio de dibujo, BOM, GUID,
   persistencia ni handlers**, y sus pruebas vigentes siguen pasando.
3. El shell **no conoce** `RackSystemKind` ni ningún sistema concreto.
4. El ADR de composición visual queda aceptado por el Owner antes de implementar producción.

## 2. Problema

La auditoría de esta iniciativa (§3) midió sobre `main` = `8a1bce5`:

- **Los tres editores ricos no adoptan NINGÚN control de I-14.** `NumericField`, `CatalogCombo` y
  `RackDialogWindow` tienen **cero** consumidores en todo `src/RackCad.UI`. Los `PreviewCanvas` que
  aparecen en las tres ventanas son un `<Canvas x:Name="PreviewCanvas">` homónimo, **no** el control
  `RackCad.UI.Controls.PreviewCanvas`. `SelectionMatrix` solo lo adoptaron los tres diálogos de
  seguridad (I-22).
- **43 brushes privados duplicados**: 16 en el selectivo, 17 en el dinámico, 10 en la cama —
  mientras `PreviewPalette` ya define 8 compartidos y `Themes/AppStyles.xaml` solo aporta 6 claves
  de estilo con hex incrustados. No existen tokens de tamaño, tipografía ni espaciado.
- **Cada ventana reimplementa la misma matriz imperativa** (`RenderMatrix`/`MatrixGrid` en el
  selectivo, `RenderFrontMatrix`/`DynamicMatrixGrid` en el dinámico): Grid + `Border` + `TextBlock`,
  con su propia convención de selección primaria/múltiple y sus propios pinceles.
- **Cada ventana reimplementa su preview**: `Canvas` plano + `Map()`/escala privadas, en vez de
  `PreviewProjection` + `PreviewCanvasPainter` + `PreviewPalette`, que ya existen.
- Volumen actual: `RackSelectiveWindow` 307 XAML + 2 205 C#; `RackDynamicSystemWindow` 403 + 2 848;
  `RackFlowBedWindow` 138 + 516.

`docs/ARCHITECTURE.md` §7.3 ya declara este shell como arquitectura objetivo y enumera exactamente
esos controles: la brecha no es de diseño, es de **adopción**. I-14 entregó piezas; I-15 entregó la
sesión; I-20/I-21 sacaron el estado a Application. Falta la capa que las componga.

## 3. Inventario comparativo (auditoría en lectura)

`Selectivo` y `Dinámico` medidos en `main` = `8a1bce5`; `Push Back` leído **solo** desde
`origin/feature/push-back` = `b2d9e9d` con `git show` (sin checkout, sin merge, sin commit).

| Concepto | Selectivo | Dinámico | Push Back | Compartible |
|---|---|---|---|---|
| Identidad | Sección "Rack" en panel izquierdo; nombre en `TextBox` crudo; GUID no destacado | Nombre + GUID en el panel izquierdo, controles crudos | Sección "Rack" con `NameBox` + `GuidText` («se asigna al insertar»), arriba del panel | **Parcial** — slot `SidebarHeader` **neutral y opcional**: el shell reserva un encabezado pero no obliga a mostrar GUID; la identidad sigue en `RackEditorSession.Identity` |
| Panel lateral | `ScrollViewer` de ancho fijo 342 px, secciones por `TextBlock` con estilos `SectionTitle`/`FieldLabel` | Columna izquierda propia, secciones ad-hoc | `ScrollViewer` 430 px con `GroupBox`: Rack, Anotaciones, Estructura, Tarima, Seguridad, Resumen, Mensajes | **Sí** — slot `SidePanelContent` con scroll y ritmo vertical por tokens; el CONTENIDO sigue siendo de cada sistema |
| Matriz | `RenderMatrix()` imperativa sobre `MatrixGrid`; `cellBorders` por (bay, level); pinceles propios | `RenderFrontMatrix()` sobre `DynamicMatrixGrid`; tarjetas `Border`+`TextBlock`; clic simple/Ctrl+clic; ámbar `#FFD166` primaria, azul `#5B8DEF` incluida | Matriz de tarjetas equivalente + `SelectionMatrix` (I-14) relegada a herramientas masivas | **Parcial** — el *contenedor*, la convención de selección y el estilo de tarjeta son compartibles; el **contenido de la celda es por sistema** (DataTemplate) |
| Preview | `Canvas` plano `#0E1B2A` + `Map()` propio + 16 brushes | `Canvas` plano `#0E1B2A` + `mapScale/mapOffsetX/mapBottomY` + 17 brushes + métodos por rol | `Canvas` plano + `PushBackPreviewModel` semántico (primitivas rol/pieza) + `PreviewProjection` + `PreviewCanvasPainter` + `PreviewPalette` | **Sí (el marco)** — slot `PreviewContent` con superficie, encabezado de vista y leyenda; la interpretación geométrica es por sistema |
| Acciones | Fila inferior propia (`Grid.Row=1`) | Botonera propia | Barra inferior con acciones (restaurar/BOM/biblioteca · actualizar · 4 vistas · cerrar) con tooltips y motivo | **Sí** — `ActionBar` común con **categorías neutrales** (leading/secondary/primary/trailing); el editor decide qué ofrece |
| Estado/errores | `TextBlock` de estado vía `UiSupport.SetStatus` | Ídem | Ídem + hint del preview con ⚠ para modelo obsoleto + motivos por acción deshabilitada | **Sí** — `StatusPresenter` con **severidades** (info/éxito/advertencia/error) |
| Scroll/resize | 1300×740, mín. 1060×600; scroll solo en panel | Similar, con `MaxHeight` en la matriz | 1280×720, mín. 1120×640; scroll en panel y matriz | **Sí** — tokens de tamaño mínimo y política de scroll por zona |
| Sesión | `RackEditorSession` + `SelectiveEditorState` (I-20) | `RackEditorSession` + `DynamicFrontMatrix` + `DynamicEditorDesignAssembler` (I-21) | `RackEditorSession` + `PushBackEditorState` + assembler | **Ya compartido** — el shell **no** la sustituye ni la envuelve |

### 3.1 Componentes de I-14 reutilizables y no adoptados

| Componente | Estado en `main` | Acción en I-30 |
|---|---|---|
| `SelectionMatrix` / `SelectionMatrixModel` | adoptado **solo** por los 3 diálogos de seguridad | disponible; el shell no lo impone como matriz principal |
| `NumericField` / `NumericFieldValidation` | **cero** consumidores | **fuera de alcance**: I-30 NO migra las entradas numéricas del dinámico; los `TextBox`/handlers actuales se alojan tal cual en los slots. Adopción posterior por migración separada |
| `CatalogCombo` | **cero** consumidores | **fuera de alcance**: ídem para los combos de catálogo; se conservan los controles existentes |
| `PreviewCanvas` (control) + `PreviewProjection` + `PreviewPalette` | controles listos; ninguna ventana usa el CONTROL | **candidato de adopción SOLO si** una prueba fija equivalencia completa de proyección y contenido (§9); si no la fija, el `Canvas` actual se aloja tal cual |
| `RackDialogWindow` | **cero** consumidores | **no** se adopta por herencia (ver ADR): el shell compone |
| `PreviewCanvasPainter` | `internal`, usado por el dinámico vía campos privados | se conserva como primitiva de dibujo |

### 3.2 Duplicaciones, dependencias indebidas y seams

- **Duplicaciones**: 43 brushes privados; 3 matrices imperativas; 3 proyecciones de preview; 3
  barras de acciones; 3 convenciones de estado.
- **Dependencias indebidas**: `RackSystemKind` aparece 37 veces en `src/RackCad.UI`, pero solo **1**
  dentro de un editor rico (`RackDynamicSystemWindow.xaml.cs:2631`, guarda de carga de biblioteca);
  el resto vive en `Editor/` (registro de módulos) y `RackMainMenuWindow`. **El shell debe quedar en
  cero.**
- **Seams disponibles**: `RackEditorSession` (identidad/catálogo/recompute/inserción),
  `IRackEditorModule` + `EditorModuleRegistry`, los estados puros de Application, `UiSupport`, y los
  controles de I-14 sin adoptar.
- **Archivos calientes**: `RackDynamicSystemWindow.xaml(.cs)` (migra en I-30),
  `RackSelectiveWindow.xaml(.cs)` (**intacto** en I-30; migra en I-31),
  `Themes/AppStyles.xaml` (recibe tokens), `tests/RackCad.UI.Tests/**` (165 pruebas).

### 3.3 Conflicto esperado al rebasar I-18 después de I-31

`git diff --stat origin/main...origin/feature/push-back -- src/RackCad.UI tests/RackCad.UI.Tests`
= **3 786 inserciones en 16 archivos**, pero casi todas en archivos **nuevos**
(`RackPushBackSystemWindow.xaml(.cs)`, `PushBackMatrixCardModel`, `PushBackPreviewModel` y 6 suites).
Los únicos archivos compartidos que I-18 toca son `RackMainMenuWindow.xaml` (9 líneas),
`RackMainMenuWindow.xaml.cs` (4) y `EditorModuleRegistryTests.cs` (3).

Predicción: **el conflicto textual será mínimo; el conflicto real es semántico.** Tras I-31, Push
Back sería el único editor fuera del shell, y sus tres piezas propias (layout en tres zonas, matriz
de tarjetas, `PushBackPreviewModel`) quedarían solapadas con las del shell. I-30 **no** modifica
Push Back: se limita a dejar registrado el handoff.

## 4. Alcance

1. **Fundación**: `RackEditorVisualShell` (composición por slots), `StatusPresenter`, `ActionBar` y
   sus modelos puros; tokens en `Themes/AppStyles.xaml`.
2. **Migración VISUAL de `RackDynamicSystemWindow`** al shell, sin cambio de comportamiento. La
   migración **aloja los controles existentes tal cual dentro de los slots** (los `TextBox`,
   `ComboBox`, `CheckBox` y `Canvas` actuales del dinámico), preservando **exactamente** su parsing,
   sus eventos, su `LostFocus`, su recomputación y su comportamiento observable. **No** sustituye
   esos controles por los de I-14: adoptar el shell es reubicar y coordinar, no reemplazar la
   captura de datos.
3. Pruebas del shell y de la migración; gate visual y de Owner.

**Sustitución de controles de captura fuera de alcance.** Migrar las entradas numéricas del dinámico
a `NumericField` o sus combos a `CatalogCombo` **no** es parte de I-30: se hace, si se hace, en
migraciones posteriores separadas. La única excepción es una **adaptación mínima inevitable**,
**demostrada** (por qué no cabe alojar el control tal cual) y **escalada al Owner antes de
ejecutarse**, registrada en el estado.

### 4.1 Contrato visual y tokens

Tokens **con nombre** en `Themes/AppStyles.xaml` (hoy hay 6 claves con hex incrustados). Categorías
mínimas, sin fijar valores en este contrato (los fija la fase 1 a partir de los ya usados):

- **Tamaño**: ancho del panel lateral, alto mínimo del preview, tamaño inicial y mínimo de ventana.
- **Color**: superficies, borde, texto primario/secundario, y los estados **primaria** e
  **incluida** de la matriz — reconciliando los 43 brushes con `PreviewPalette`.
- **Tipografía**: familia, tamaño base, título de sección, etiqueta de campo.
- **Espaciado**: ritmo vertical de sección, padding de tarjeta, separación de la barra de acciones.
- **Sidebar / superficies / preview / acciones**: un token por decisión repetida hoy a mano.

### 4.2 Composición por slots

El shell expone slots como `ContentControl`/`ContentPresenter`; el editor concreto **inyecta**
contenido:

| Slot | Contenido | Opcional |
|---|---|---|
| `SidebarHeader` | encabezado NEUTRAL del panel (lo que el editor decida: nombre, identidad, etc.) | **sí** — el shell no obliga a mostrar GUID ni impone un contrato de identidad |
| `SidePanelContent` | secciones del sistema (scroll) | no |
| `MatrixContent` | superficie central de edición | **sí** — un editor sin matriz deja el slot vacío y el shell recoloca sin huecos |
| `PreviewContent` | superficie de vista previa + su selector | sí |
| `ActionBarContent` | acciones del sistema, clasificadas en las categorías neutrales (§4.5) | sí |
| `StatusContent` | mensajes; por defecto el `StatusPresenter` común | sí |

`SidebarHeader` es **neutral y opcional**: el shell reserva un encabezado de panel pero no decide su
contenido; un editor puede poner nombre + GUID, solo nombre, o nada. La identidad y el GUID siguen
viviendo en `RackEditorSession`, no en el shell.

**Soporte de contenido opcional y editores sin matriz** es requisito, no cortesía: `RackFlowBedWindow`
(516 líneas, sin matriz) es el caso de prueba del slot vacío, aunque **no se migra** en I-30.

### 4.3 Frontera shell / sesión / estado puro

| Capa | Posee | Prohibido |
|---|---|---|
| `RackEditorVisualShell` (UI) | composición, tokens, slots, severidades, categorías de acción, scroll/resize | conocer sistemas, geometría, BOM, persistencia, `RackSystemKind` |
| `RackEditorSession` (I-15) | identidad, catálogo, recompute coalescido, contrato de inserción | pintar o decidir layout |
| Estados puros (I-20/I-21, Application) | estructura, selección, valores por celda, ensamblado del diseño | tipos WPF |

Reglas duras:

- **Prohibido que el shell dependa de `RackSystemKind`** (objetivo: 0 ocurrencias en sus archivos).
- **Prohibida cualquier rama por Selectivo, Dinámico o Push Back** dentro del shell.
- **Preferencia explícita** por composición, `ContentControl`, `ContentPresenter`, `DataTemplate` y
  modelos puros, frente a herencia profunda de ventanas.

### 4.4 Status presenter y severidades

Un presentador único con severidades `Info`, `Success`, `Warning`, `Error`, alimentado por un modelo
puro (mensaje + severidad), que absorbe el `UiSupport.SetStatus` de hoy. Debe distinguir «resultado
actual» de «referencia obsoleta» (el caso ⚠ que Push Back ya resuelve a mano).

### 4.5 Action bar: categorías, estilos y motivos

Categorías **neutrales** (posiciones/roles visuales, no nombres de sistema); **el editor concreto
decide qué acciones ofrece y en qué categoría las clasifica**:

- **`LeadingActions`** — extremo inicial de la barra;
- **`SecondaryActions`** — acciones de apoyo;
- **`PrimaryActions`** — acción(es) principal(es);
- **`TrailingActions`** — extremo final.

El shell **no** define qué acción va en cada categoría ni asume BOM/biblioteca/insertar/cerrar: eso
es del editor. Requisitos:

- estilos coherentes habilitado/deshabilitado (partiendo de `PrimaryButtonStyle`/`SecondaryButtonStyle`);
- **toda acción deshabilitada expone su motivo por tooltip**, sin excepción;
- la barra sobrevive al tamaño mínimo de ventana sin ocultarse.

## 5. Fuera de alcance

- **Selectivo**: `RackSelectiveWindow` no se toca — es I-31.
- **Push Back**: `feature/push-back` **solo lectura**; ni un commit, checkout, merge o rebase.
  El handoff se documenta, no se ejecuta.
- `RackFlowBedWindow`, `RackFrameConfiguratorWindow`, `RackLargueroWindow`: no migran.
- Geometría, resolvers, BOM, persistencia, catálogos, handlers y Plugin: **prohibido**, salvo
  adaptación mínima inevitable **previamente escalada al Owner** y registrada en el estado.
- `docs/HANDOFF.md` y `docs/ROADMAP.md`: los actualiza la sesión de integración.

## 6. Dependencias y conflictos

- Integradas requeridas: **I-14, I-15, I-20, I-21, I-24**.
- Conflicto por orden: **I-31** (I-30 primero) y la **reanudación de I-18** (espera la secuencia).
- Entrada del Owner requerida: **aceptación del ADR-0019** antes de implementar producción.

## 7. Archivos esperados

Previstos (crear salvo indicación):

- `src/RackCad.UI/Shell/RackEditorVisualShell.xaml(.cs)`;
- `src/RackCad.UI/Shell/EditorStatusPresenter.*`, `EditorActionBar.*` y sus modelos puros;
- `src/RackCad.UI/Themes/AppStyles.xaml` (**modificar**: añadir tokens sin romper claves vigentes);
- `src/RackCad.UI/RackDynamicSystemWindow.xaml(.cs)` (**modificar**: composición sobre el shell);
- `tests/RackCad.UI.Tests/` (añadir suites del shell y de la migración).

Hotspots que **no** deben aparecer en el diff de I-30: `RackSelectiveWindow.*`,
`RackFlowBedWindow.*`, Plugin, Application de geometría/BOM/persistencia, catálogos.

## 8. Fases

1. **ADR + tokens en papel** — ADR-0019 propuesto; inventario de tokens derivado de los 43 brushes.
   Termina con la decisión del Owner. **Detiene la implementación.**
2. **Shell** — slots, status presenter, action bar y modelos puros, con pruebas propias; ninguna
   ventana migrada todavía.
3. **Tokens** — `AppStyles.xaml` gana los tokens; el shell los consume.
4. **Migración del Dinámico** — `RackDynamicSystemWindow` se compone sobre el shell; sus pruebas
   vigentes siguen verdes sin reescribirse salvo por nombres de contenedor.
5. **Cierre** — gates, evidencia, DLL Debug del SHA exacto y validación del Owner.

## 9. Pruebas y builds

- `dotnet test` (solución) y `tests/RackCad.UI.Tests` — las 165 pruebas UI vigentes siguen verdes.
- `dotnet build src/RackCad.UI -c Debug` y `src/RackCad.Plugin -c Debug`.
- Suites nuevas: composición de slots, slot vacío (editor sin matriz), severidades del status,
  categorías/tooltips/estados de la action bar, tokens resueltos, y **regresión estructural** del
  dinámico. Pruebas **estructurales y semánticas**, nunca comparación de píxeles.
- **Adopción del control `PreviewCanvas` en el shell SOLO si** una prueba fija la **equivalencia
  completa de proyección y contenido** frente al `Canvas`+`Map()` actual del dinámico (misma
  proyección mundo→lienzo y mismas primitivas dibujadas). Sin esa prueba, el `Canvas` actual se aloja
  tal cual en el slot y no se adopta el control.
- Regresión obligatoria: golden dinámico, golden Push Back y validador I-19 **sin cambios**.

## 10. Validación manual

Requerida (✋ en ROADMAP). El Owner valida en **AutoCAD 2025** sobre el **DLL Debug construido desde
el SHA exacto posterior al rebase final**, comprobando que el editor dinámico migrado conserva
dibujo, BOM, identidad y round-trip, y que la interfaz cumple el contrato visual. Sin esa validación
la iniciativa no se integra.

## 11. Criterios de aceptación

1. Shell con slots, status presenter y action bar, con pruebas propias verdes.
2. Cero ocurrencias de `RackSystemKind` y cero ramas por sistema en los archivos del shell.
3. `RackDynamicSystemWindow` compuesto sobre el shell, sin cambio de dibujo/BOM/GUID/persistencia.
4. Tokens en `AppStyles.xaml` consumidos por el shell.
5. Slot de matriz opcional demostrado por prueba.
6. Suites vigentes verdes; goldens y validador intactos.
7. ADR-0019 **aceptado**; DLL Debug validado por el Owner.
8. Diff sin Selectivo, sin Push Back, sin Plugin, sin geometría/BOM/persistencia/catálogos.

## 12. Condiciones para detenerse

- ADR-0019 sin decisión del Owner → **no se implementa producción** (estado actual).
- La migración del dinámico exigiría tocar geometría, BOM, persistencia, handlers o Plugin.
- Aparece necesidad de tocar `RackSelectiveWindow` (es I-31) o `feature/push-back`.
- Una prueba vigente exige reescritura de comportamiento, no de nombres.
- `origin/main` avanza con cambios en los hotspots declarados.
- El alcance crece más allá de fundación + Dinámico.

### 12.1 Partición obligatoria

| Iniciativa | Alcance | Condición |
|---|---|---|
| **I-30** (esta) | fundación del shell + migración de **Dinámico** | — |
| **I-31** | migración de **Selectivo** (`refactor/selective-visual-shell`, rama provisional) | **no puede reclamarse antes de cerrar I-30**; sin rama, worktree, contrato ni estado en esta corrida |
| **I-18** | Push Back: **solo lectura** y **handoff posterior** | se rebasa y reanuda **después** de I-31 |

## 13. Estado versionado y entrega del Pull Request

Estado canónico: [`docs/automation/state/I-30.yml`](../automation/state/I-30.yml). Sin Pull Request
(modo `manual-git-only`). El merge automático está prohibido. Tras publicar el ADR propuesto el
estado queda `state: waiting`, `gate: owner-decision`.

## 14. Evidencia final

Se completa al cerrar. Esta corrida deja: reclamo, auditoría, contrato, estado y ADR propuesto, sin
tocar producción, `docs/HANDOFF.md`, `docs/ROADMAP.md` ni `feature/push-back`.
