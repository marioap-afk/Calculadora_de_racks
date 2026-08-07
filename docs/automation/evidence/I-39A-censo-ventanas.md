# I-39A — Censo por tipo de las ventanas WPF y taxonomía A/B/C/D

> Evidencia medida sobre la base de I-39A ([contrato](../../initiatives/I-39A-contrato-funcional-piloto-editor-acotado.md),
> [ADR-0029](../../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md),
> [decisiones](../decisions/I-39.md)). La unidad del censo es el **tipo**, no el archivo y nunca un
> `x:Name` (ADR-0029 D1). Registro factual del árbol; no incluye conteos de pruebas ni hashes, que
> viven en `docs/HANDOFF.md` §12.

## 1. Método, y por qué los otros dos no sirven

El censo se obtiene por **reflexión sobre el ensamblado de producto**: toda clase **concreta** que
sea asignable a `System.Windows.Window` en `RackCad.UI`. Lo fija
`tests/RackCad.UI.Tests/WindowCensusGuardTests.cs`, que asevera contención en **los dos sentidos**
—ninguna ventana real fuera del censo, ninguna ventana censada que no exista—, de modo que el
conjunto declarado **es** el conjunto real.

Los dos métodos alternativos se midieron y los dos mienten:

- **Por nombre de archivo.** `SafetyPerPostWindow` está declarada dentro de
  `src/RackCad.UI/SelectiveSafetyWindow.cs:903`. Un censo guiado por nombres de archivo la deja
  fuera de toda regla sin avisar. Es el único archivo del proyecto con dos clases `Window`; los
  demás archivos multi-clase declaran tipos auxiliares que no derivan de `Window`
  (`TopeResult`, `ParrillaResult`, `DesviadorResult`, `LayoutResult`, `FillResult`, `Row`,
  `HeaderPreset`, `DialogActionBar`).
- **Por `x:Name`.** El **tipo** `RackCad.UI.Controls.PreviewCanvas` tiene **un** consumidor
  productivo —`StructuralSections/StructuralSectionPreview.cs:21`, por herencia— y **cero**
  ocurrencias en XAML. Otras diez ventanas dibujan sobre un `System.Windows.Controls.Canvas`
  ordinario cuyo `x:Name` es `PreviewCanvas`. Un censo por nombre contaría once consumidores donde
  hay uno.

La reflexión, además, captura sin esfuerzo las ventanas construidas enteramente en C# y cualquier
profundidad de derivación.

## 2. Resultado

**29 clases concretas** derivadas de `Window`: **28 productivas** y **1 de infraestructura**. Todas
viven en `src/RackCad.UI`; ninguna en `RackCad.Plugin`, `RackCad.Application`, `RackCad.Domain` ni
`RackCad.Catalogs`.

| Arquetipo | Cantidad |
|---|---|
| A — editor rico de sistema | 6 |
| B — editor acotado con preview | 6 |
| C — diálogo de configuración transaccional | 10 |
| D — ventana utilitaria | 6 |
| Infraestructura (no producto) | 1 |

## 3. A — Editor rico de sistema

Varios ámbitos de edición, estructura agregada, preview principal, inserción o actualización en
AutoCAD, posible persistencia, sesión o identidad de rack y recomputación compleja. **La ausencia de
matriz no saca a una ventana de este arquetipo.**

| Clase | Declaración | Composición | Shell |
|---|---|---|---|
| `RackSelectiveWindow` | `Systems/Selective/RackSelectiveWindow.xaml.cs:35` | XAML | `RackEditorVisualShell` |
| `RackDynamicSystemWindow` | `Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:34` | XAML | `RackEditorVisualShell` |
| `RackPushBackSystemWindow` | `Systems/PushBack/RackPushBackSystemWindow.xaml.cs:40` | XAML | `RackEditorVisualShell` |
| `RackCantileverWindow` | `Systems/Cantilever/RackCantileverWindow.xaml.cs:31` | XAML | `RackEditorVisualShell` |
| `RackFlowBedWindow` | `Systems/FlowBed/RackFlowBedWindow.xaml.cs:22` | XAML | ninguno |
| `RackFrameConfiguratorWindow` | `RackFrames/RackFrameConfiguratorWindow.xaml.cs:17` | XAML | ninguno |

**Discutibles registrados.** `RackFlowBedWindow` es A por rol —sesión, preview, BOM, inserción y
actualización en AutoCAD— y B por tamaño y complejidad; se clasifica **por rol**.
`RackFrameConfiguratorWindow` edita un subensamble y no una línea, pero su escala, su árbol, sus
mutaciones estructurales y sus inserciones multivista la sitúan en **A**. Ninguna de las dos está
migrada al shell visual: es trabajo de I-39B.

## 4. B — Editor acotado con preview

Alcance funcional limitado, parámetros propios, preview, diagnóstico y un resultado o acción
acotada. **No se llama «editor de componente»**: sus consumidores no son necesariamente componentes
persistentes ni miembros del BOM (decisión 6 del Owner).

| Clase | Declaración | Composición | Shell |
|---|---|---|---|
| `CantileverColumnBaseWindow` | `Systems/Cantilever/Components/CantileverColumnBaseWindow.xaml.cs:26` | XAML | shell de componentes |
| `CantileverArmWindow` | `.../CantileverArmWindow.xaml.cs:24` | XAML | shell de componentes |
| `CantileverSeparatorWindow` | `.../CantileverSeparatorWindow.xaml.cs:24` | XAML | shell de componentes |
| `CantileverBraceWindow` | `.../CantileverBraceWindow.xaml.cs:22` | XAML | shell de componentes |
| `RackLargueroWindow` | `Systems/Larguero/RackLargueroWindow.xaml.cs:22` | XAML | ninguno |
| `StructuralSectionInspectorWindow` | `StructuralSections/StructuralSectionInspectorWindow.cs:24` | **code-only** | ninguno → **piloto de I-39A** |

**Discutible registrado.** `StructuralSectionInspectorWindow` explora un catálogo y no edita una
pieza del modelo de rack, lo que la acercaría a D; se clasifica en **B** porque tiene parámetros,
preview, diagnóstico y un resultado materializable, que es lo que el arquetipo describe. Su
`WindowStartupLocation` es `CenterScreen` y no recibe `Owner`, coherente con abrirse desde un comando
del Plugin.

**Anomalía de tamaño medida en las cuatro ventanas Cantilever.** Aplican
`EditorShellWindowStyle` —el contrato del editor **rico**— y declaran `Width`/`Height` locales sin
mínimos propios. En precedencia WPF el valor local gana al setter del estilo, pero `MinWidth` y
`MinHeight` vienen del estilo y **clampean**:

| Ventana | Declara | Abre realmente |
|---|---|---|
| `CantileverColumnBaseWindow`, `CantileverArmWindow` | `1000×700` | `1120×700` |
| `CantileverSeparatorWindow`, `CantileverBraceWindow` | `900×640` | `1120×672` |

Los cuatro `Width` y dos de los cuatro `Height` son **letra muerta**: contradicen el contrato de
tamaño sin producir el tamaño escrito. Es la evidencia que sostiene el texto normativo de ADR-0029
D9. Su corrección es de **I-39C**, cuando esas cuatro ventanas adopten el contrato del arquetipo B.

## 5. C — Diálogo de configuración transaccional

Modal que edita una copia o una selección, acepta o cancela, no administra una sesión completa de
rack y puede no tener preview. Las diez son **code-only**, usan `CenterOwner`, mergean `AppStyles.xaml`
a mano y cierran por `DialogResult`.

| Clase | Declaración |
|---|---|
| `SelectiveSafetyWindow` | `SelectiveSafetyWindow.cs:20` |
| `SafetyPerPostWindow` | `SelectiveSafetyWindow.cs:903` — **declarada dentro del archivo de otra ventana** |
| `SafetyTopeGridWindow` | `SafetyTopeGridWindow.cs:25` |
| `SafetyParrillaGridWindow` | `SafetyParrillaGridWindow.cs:29` |
| `SafetyGuiaEntradaGridWindow` | `SafetyGuiaEntradaGridWindow.cs:19` |
| `SafetyDesviadorGridWindow` | `SafetyDesviadorGridWindow.cs:21` |
| `SafetyDefensaGridWindow` | `SafetyDefensaGridWindow.cs:13` |
| `SelectiveSegmentsWindow` | `Systems/Selective/SelectiveSegmentsWindow.cs:19` |
| `RackWarehouseLayoutWindow` | `RackWarehouseLayoutWindow.cs:15` |
| `RackWarehouseFillWindow` | `RackWarehouseFillWindow.cs:15` |

**Discutibles registrados.** `RackWarehouseLayoutWindow` y `RackWarehouseFillWindow` son C por forma
—transaccionales, `SizeToContent`, acción primaria por defecto, resultado tipado— y D por
procedencia: las abre un comando del Plugin sin ventana padre WPF. Se clasifican **por forma**.
`SafetyDefensaGridWindow` es el único diálogo de rejilla que **no** adoptó `SelectionMatrix`, porque
la defensa no es una matriz booleana; la excepción es de producto y queda registrada.

## 6. D — Ventana utilitaria

Navegación, consulta, ayuda, listas o BOM, sin contrato transaccional de edición.

| Clase | Declaración | Composición |
|---|---|---|
| `RackMainMenuWindow` | `RackMainMenuWindow.xaml.cs:20` | XAML |
| `RackDesignLibraryWindow` | `RackDesignLibraryWindow.xaml.cs:13` | XAML |
| `RackBomWindow` | `RackBomWindow.xaml.cs:9` | XAML |
| `RackConsolidatedBomWindow` | `RackConsolidatedBomWindow.xaml.cs:11` | XAML |
| `RackListWindow` | `RackListWindow.xaml.cs:13` | XAML |
| `RackCommandHelpWindow` | `RackCommandHelpWindow.cs:14` | **code-only** |

**Discutible registrado.** `RackMainMenuWindow` es D por interfaz —lista de botones, sin preview y
sin transacción— pero funciona como **infraestructura de navegación**: es el host del registro de
módulos de I-15 y el único punto WPF que provee `Owner` a los editores. Se clasifica en **D** y la
observación queda escrita.

## 7. Infraestructura

| Clase | Declaración | Estado |
|---|---|---|
| `RackDialogWindow` | `Controls/RackDialogWindow.cs:33` | **cero subclases productivas** |

Es chrome compartido, no una ventana que el usuario abra, y no cuenta como producto (decisión 7 del
Owner). Nació en I-14 con patrón strangler y **ningún** diálogo lo adoptó: los diez del arquetipo C
reconstruyen a mano el par Aceptar/Cancelar que esta base ya ofrece. Su papel futuro en el arquetipo
C es una decisión de **I-39D**.

## 8. Hallazgos adyacentes registrados, no corregidos en I-39A

Ninguno bloquea I-39A; todos quedan asignados.

1. **`RackPushBackSystemWindow` es la única de las nueve ventanas de rack sin `IsCancel`** en su
   botón Cerrar (`Systems/PushBack/RackPushBackSystemWindow.xaml:475`): **Escape no la cierra**. Es
   además la única con cambios pendientes explícitos —`RackModuleEditSession.HasPendingChanges`,
   local al editor de módulo—, que **ningún `Closing` consulta**. → **I-39B**, después de fijar la
   política de Escape.
2. **`EditorAction`, `EditorActionBar`, `EditorStatus`, `EditorStatusPresenter` y sus cuatro
   severidades: cero consumidores productivos.** Las ventanas migradas ponen botones crudos y
   replican a mano `ToolTipService.ShowOnDisabled`. → adopción gradual, I-39B/C/D.
3. **`EditorActions.Button` no fija `IsDefault` ni `IsCancel`** (`Shell/EditorAction.cs:41-66`), así
   que hoy no puede describir la acción por defecto ni la de cancelación de una ventana. → evolución
   registrada para **I-39C**.
4. **Fugas de capa en infraestructura transversal**: `Editor/RecomputeGate.cs:2` declara
   `using RackCad.UI.Systems.Selective;`, y `RecomputeDebouncer` y `DispatcherRecomputeScheduler`
   declaran `using RackCad.UI.RackFrames;`, en los tres casos solo para resolver un `<see cref>` de
   documentación. Ninguna guarda vigente los cubre. → registrado, **no** se corrige en I-39A
   (decisión 32 del Owner).
5. **Tres chromes coexisten**: estilo compartido sobre shell templated; `Width`/`Height` literales en
   XAML; y chrome ensamblado a mano en el constructor de C#, repetido en once ventanas code-only. →
   I-39B/C/D.
6. **`UiSupport.TryOptionalNum` tiene tres consumidores** y ninguno es de Cantilever, que valida por
   `NumericField`. → sin asignar; no es deuda de I-39A.

## 9. Trazabilidad

- Guarda que mantiene vivo el censo: `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs`, verificada
  **en rojo** bajo dos infracciones inyectadas: una ventana real retirada del censo y una ventana
  fantasma declarada.
- Contrato: [`../../initiatives/I-39A-contrato-funcional-piloto-editor-acotado.md`](../../initiatives/I-39A-contrato-funcional-piloto-editor-acotado.md)
- ADR: [`../../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md`](../../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md)
- Decisiones: [`../decisions/I-39.md`](../decisions/I-39.md)
