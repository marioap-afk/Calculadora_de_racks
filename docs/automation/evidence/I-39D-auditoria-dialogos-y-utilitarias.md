# I-39D — Auditoría de apertura: los diez diálogos del arquetipo C y las seis utilitarias del D

> Evidencia medida sobre `main` (I-39C ya integrada). Síntesis de once auditorías de solo lectura
> más **verificación puntual propia** de todo lo dudoso o contradictorio. Unidad de inventario: el
> **tipo** (ADR-0029 D1). Este documento **no decide** nada: describe lo que el código HACE hoy y
> separa explícitamente lo deseable de lo medido. Las contradicciones entre auditorías **no se
> promedian**: se resuelven contra el código y quedan escritas en §8.

---

## 1. Cuántas ventanas hay en C y en D, y si el censo de I-39A sigue siendo exacto

### 1.1 El conjunto

**C = 10, D = 6.** Verificado contra la guarda por reflexión, que es la fuente viva del censo y
asevera contención en los dos sentidos (`tests/RackCad.UI.Tests/WindowCensusGuardTests.cs:63-68`,
listas en `:46-51` y `:56-60`). Las 29 clases concretas derivadas de `Window` del ensamblado están
declaradas: 6 A + 6 B + 10 C + 6 D + 1 infraestructura.

| # | Arquetipo C — diálogo transaccional | Declaración |
|---|---|---|
| 1 | `SelectiveSafetyWindow` | `src/RackCad.UI/SelectiveSafetyWindow.cs:20` |
| 2 | `SafetyPerPostWindow` | `src/RackCad.UI/SelectiveSafetyWindow.cs:903` — dentro del archivo de otra ventana |
| 3 | `SafetyTopeGridWindow` | `src/RackCad.UI/SafetyTopeGridWindow.cs:25` |
| 4 | `SafetyParrillaGridWindow` | `src/RackCad.UI/SafetyParrillaGridWindow.cs:29` |
| 5 | `SafetyGuiaEntradaGridWindow` | `src/RackCad.UI/SafetyGuiaEntradaGridWindow.cs:19` |
| 6 | `SafetyDesviadorGridWindow` | `src/RackCad.UI/SafetyDesviadorGridWindow.cs:21` |
| 7 | `SafetyDefensaGridWindow` | `src/RackCad.UI/SafetyDefensaGridWindow.cs:13` |
| 8 | `SelectiveSegmentsWindow` | `src/RackCad.UI/Systems/Selective/SelectiveSegmentsWindow.cs:19` |
| 9 | `RackWarehouseLayoutWindow` | `src/RackCad.UI/RackWarehouseLayoutWindow.cs:15` |
| 10 | `RackWarehouseFillWindow` | `src/RackCad.UI/RackWarehouseFillWindow.cs:15` |

| # | Arquetipo D — utilitaria | Declaración |
|---|---|---|
| 1 | `RackMainMenuWindow` | `src/RackCad.UI/RackMainMenuWindow.xaml.cs:20` |
| 2 | `RackDesignLibraryWindow` | `src/RackCad.UI/RackDesignLibraryWindow.xaml.cs:13` |
| 3 | `RackBomWindow` | `src/RackCad.UI/RackBomWindow.xaml.cs:9` |
| 4 | `RackConsolidatedBomWindow` | `src/RackCad.UI/RackConsolidatedBomWindow.xaml.cs:11` |
| 5 | `RackListWindow` | `src/RackCad.UI/RackListWindow.xaml.cs:13` |
| 6 | `RackCommandHelpWindow` | `src/RackCad.UI/RackCommandHelpWindow.cs:14` |

### 1.2 Exactitud del censo: sí para C y D; con cuatro punteros caducos en A y B

Verifiqué **las dieciséis** declaraciones del documento del censo contra `main`: **las dieciséis son
exactas**, línea a línea. La deriva documental existe pero está **fuera de C y D**: cuatro punteros
de los arquetipos A y B se movieron y el documento nunca se refrescó (`git log` del censo: un solo
commit).

| Censo dice | Real en `main` |
|---|---|
| `RackFrameConfiguratorWindow.xaml.cs:17` (`I-39A-censo-ventanas.md:58`) | `:18` |
| `RackCantileverWindow.xaml.cs:31` (`:59`) | `:32` |
| `RackPushBackSystemWindow.xaml.cs:40` (`:61`) | `:42` |
| `StructuralSectionInspectorWindow.cs:24` (`:82`) | `:25` |

Además, **dos afirmaciones del censo ya no describen `main`** y ambas fueron consumidas por I-39C:

- §4, «anomalía de tamaño de las cuatro Cantilever»: **corregida**. Ya existe
  `BoundedEditorWindowStyle` con sus cuatro tokens (`src/RackCad.UI/Themes/AppStyles.xaml:62-65`
  y `:183-187`).
- §8 punto 2, «`EditorAction`… cero consumidores productivos»: **ya no es cierto globalmente**. Hay
  uno: `src/RackCad.UI/StructuralSections/StructuralSectionInspectorWindow.cs:223` y `:227`. Lo que
  **sí** sigue en cero es `EditorActionBar` fuera de la plantilla del shell A
  (`src/RackCad.UI/Themes/Generic.xaml:154` es su única instanciación) y `EditorStatusPresenter`
  (única mención en `src/RackCad.UI/Shell/RackEditorVisualShell.cs:95`, un comentario).

**Conclusión operativa.** El censo es citable como **clasificación** (la guarda lo mantiene vivo);
no es citable hoy como **puntero exacto** ni en su §4/§8. Refrescarlo es trabajo de minutos y
prerrequisito para que I-39D lo cite en su contrato.

---

## 2. Qué comparten de verdad los diez diálogos de C — y las excepciones, que importan más

### 2.1 Lo que comparten los diez, sin excepción (verificado)

1. **Son code-only.** Ninguno tiene `.xaml`; todo el árbol se arma en el constructor.
2. **Ninguno hereda de `RackDialogWindow`.** Cero subclases productivas en todo `src/`
   (`src/RackCad.UI/Controls/RackDialogWindow.cs:33`); la única subclase del repositorio vive en
   `tests/RackCad.UI.Tests/RackDialogWindowTests.cs:13`.
3. **Declaran `CenterOwner`**: `SelectiveSafetyWindow.cs:164` y `:926`, `SafetyTopeGridWindow.cs:87`,
   `SafetyParrillaGridWindow.cs:85`, `SafetyGuiaEntradaGridWindow.cs:68`,
   `SafetyDesviadorGridWindow.cs:117`, `SafetyDefensaGridWindow.cs:71`,
   `SelectiveSegmentsWindow.cs:49`, `RackWarehouseLayoutWindow.cs:50`,
   `RackWarehouseFillWindow.cs:47`.
4. **Mergean `Themes/AppStyles.xaml` a mano**, uno por instancia.
5. **Tienen exactamente una barra de acción cada uno, con `IsDefault` en la primaria e `IsCancel` en
   `Cancelar`**, y `Cancelar` **sin `Click` propio**: Escape y el botón recorren el mismo camino de
   WPF.
6. **Cero `OnClosing` y cero `Closing +=`** en los diez (grep = 0). Los únicos dos `OnClosing` del
   producto son de arquetipo A (`RackFrames/RackFrameConfiguratorWindow.xaml.cs:196`,
   `Systems/PushBack/RackPushBackSystemWindow.xaml.cs:1849`).
7. **Cero foco inicial y cero `TabIndex`.** El orden de tabulación es el orden de `Children.Add`.
8. **Nunca deshabilitan un botón de la barra.** No hay una sola acción bloqueada que necesite motivo
   visible; los `IsEnabled` que existen son de campos y filas.
9. **Devuelven por propiedad tipada + `DialogResult`**, y ningún camino de cierre inserta, actualiza
   ni guarda: el efecto lo produce el llamador.
10. **Trabajan sobre copia.** Ninguno muta el estado del llamador antes del OK.
11. **Cero adopción de `EditorAction`/`EditorActionBar`/`EditorStatusPresenter`.**

### 2.2 Las excepciones — lo que **no** comparten

Esta es la parte que gobierna el plan, porque cada excepción es un punto donde una unificación
mecánica cambia producto.

| Excepción medida | Dónde | Por qué importa |
|---|---|---|
| **`SafetyDefensaGridWindow` no aplica `FontFamily` ni `Background`** — `grep` de ambos en el archivo devuelve **cero** | `SafetyDefensaGridWindow.cs:71-75` (solo `CenterOwner` + merge) frente a `SafetyDesviadorGridWindow.cs:117-120` | Es la única de las diez con el bloque de chrome incompleto: abre con la tipografía por defecto de `Window` y sin `WindowBackgroundBrush` (#F4F6F9, `AppStyles.xaml:8`). Cualquier chrome común **le cambia el aspecto**: corrección, no refactor |
| **La etiqueta primaria no es «Aceptar» en dos** | `RackWarehouseLayoutWindow.cs:124` («Colocar»), `RackWarehouseFillWindow.cs:103` («Calcular»); las otras ocho dicen «Aceptar» | `RackDialogWindow.CreateActionBar` tiene `acceptText = "Aceptar"` por defecto (`:53`): una adopción sin parámetro renombra la acción primaria de dos ventanas |
| **Concordancia de género cruzada en Todos/Ninguno** | «Todos»/«Ninguno» en `SafetyTopeGridWindow.cs:143,145`, `SafetyDesviadorGridWindow.cs:176,178`, `SafetyGuiaEntradaGridWindow.cs:94,96`; «Todas»/«Ninguna» en `SafetyParrillaGridWindow.cs:136,138` | Una fábrica que fije el literal cambia el texto visible de una de las cuatro |
| **Tres formas distintas de construir el mismo par** | línea única (7 ventanas); **fábrica local** `private Button Button(string, bool)` en `SafetyGuiaEntradaGridWindow.cs:135-143`; **inicializador multilínea** con `Padding` de Cancelar 16 en vez de 10 en `SafetyDefensaGridWindow.cs:92-107` | La cuenta honesta es **10 barras** en 10 ventanas, no «9 sitios idénticos» |
| **Guía da `Margin(0,0,8,0)` también a `Cancelar`** | fábrica en `SafetyGuiaEntradaGridWindow.cs:141` vs `SafetyTopeGridWindow.cs:149` y `SafetyParrillaGridWindow.cs:142`, sin `Margin` | 8 px de diferencia en el borde derecho. El precedente de I-39A es que **una ronda de validación manual se rechazó por un único defecto de espaciado** |
| **Un tercer botón de terminación** | `SelectiveSegmentsWindow.cs:102-107`, «Sin medio frente»: pone `DialogResult = true` **sin** `CommitFromControls` y con `Result` vacío | No es Aceptar ni Cancelar: es una **tercera semántica** («el frente no se parte»). Degradarla a `leading` decorativo o a Cancelar cambia lo que llega a `RackSelectiveWindow.xaml.cs:703` |
| **Un botón «Restaurar» sin línea base declarada** | `SelectiveSafetyWindow.cs:946-947`, «Todos por defecto»: pone todos los combos al índice 0 | D6 exige que `Restaurar` declare a qué línea base vuelve; vuelve al **valor neutro del control**, no al estado de apertura |
| **Cuatro de diez no tienen matriz booleana** | Tope/Parrilla/Guía/Desviador adoptan `SelectionMatrix`; `SafetyDefensaGridWindow` **no puede**: `SelectionMatrixModel` es `bool[,]` y el único acompañante que `SelectionMatrix` admite es un `TextBlock` de solo lectura, mientras la fila de la defensa lleva `TextBox` editables y casillas Auto acopladas (`SafetyDefensaGridWindow.cs:178-243`) | Excepción **de producto**, ya registrada en el censo `:126-128`. No es deuda |
| **El diagnóstico está en lados opuestos de la barra** | Tope: error **encima** de los botones (dock Bottom en orden `options 139 / buttons 154 / error 158`); Parrilla: `summary` **debajo** (`options 128 / summary 132 / buttons 147`) | Un slot de estado único mueve una de las dos bandas |
| **Dos no tienen banda de estado en absoluto** | `SafetyPerPostWindow` (`SelectiveSafetyWindow.cs:903-1013`) y `SafetyGuiaEntradaGridWindow` (cero `Foreground` de estado) | «No aplicable» legítimo (D8): ninguna tiene entrada que pueda ser inválida. No inventar carencia |
| **Tres rojos y un naranja, ninguno del token** | `Brushes.Firebrick` (#B22222) en `SelectiveSafetyWindow.cs:192`, `SafetyTopeGridWindow.cs:157`, `SafetyDesviadorGridWindow.cs:193`, `SafetyDefensaGridWindow.cs:82`, `SelectiveSegmentsWindow.cs:243,269,281`, `RackWarehouseLayoutWindow.cs:120`, `RackWarehouseFillWindow.cs:99`; **#B00020 a mano** en `SafetyParrillaGridWindow.cs:287`; **`Brushes.DarkOrange`** (#FF8C00) en `SafetyDesviadorGridWindow.cs:190` | El token de error es `ShellStatusErrorBrush` #B00020 (`AppStyles.xaml:38`) y el de aviso #B7791F (`:37`). Unificar **cambia el color en siete ventanas** |
| **Un único aviso no bloqueante en las dieciséis** | `SafetyDesviadorGridWindow.cs:190` | Es el caso exacto para el que se escribió `UiSupport.SetStatus(TextBlock, string, EditorStatusSeverity)` (`UiSupport.cs:43-48`), hoy con **un** consumidor productivo y de arquetipo A (`RackCantileverWindow.xaml.cs:447`) |
| **Bloqueadores modales solo en una de C** | `SafetyParrillaGridWindow.cs:341` y `:356`, dos `MessageBox` que **abortan** el cierre. Las otras nueve: cero `MessageBox`, cero `FileDialog` | Parte a C en dos mitades de coste muy distinto para caracterizar |
| **Tamaño: tres familias incompatibles** | ver §2.3 | Un token de mínimos común clamparía cuatro ventanas |
| **Una sin scroll y sin tope de crecimiento** | `SelectiveSegmentsWindow`: `Width=400` (`:47`), `SizeToContent.Height` (`:48`), `ResizeMode.NoResize` (`:50`), **cero `ScrollViewer`** en el archivo, y «+ Agregar tramo» sin tope | La altura crece sin límite y no hay redimensión ni desplazamiento |
| **Colisión de miembro real** | `SafetyGuiaEntradaGridWindow.cs:164` declara `private void Accept()` | Heredar de `RackDialogWindow` **oculta** su `protected virtual void Accept()` (aviso CS0114) y el `ok.Click` de `:99` seguiría llamando al privado, no a la política de la base |

### 2.3 Tamaño de C: tres familias, ningún estilo compartido

Ninguna de las 16 aplica `EditorShellWindowStyle` ni `BoundedEditorWindowStyle`. **No existe hoy
contrato de tamaño para C ni para D**: son 16 declaraciones independientes.

| Familia | Ventanas | Declaración |
|---|---|---|
| **Calculado a partir de los datos** (4) | Tope `Width = Math.Max(560, Math.Min(900, 260 + frentes*46))`, `Height = Math.Min(676, 296+niveles*30)`, min 540×336 (`:82-86`); Parrilla `Max(560,Min(920,300+frentes*52))` / `Min(716,356+niveles*30)`, min 520×376 (`:80-84`); Guía `Max(470,Min(1000,220+niveles*54))` / `Min(716,281+niveles*30)`, min 450×336 (`:63-67`); Desviador `Max(560,Min(1000,270+postes*46))` / `Min(716,366+niveles*30)`, min 540×366 (`:112-116`) | El **ancho** crece con la rejilla |
| **Fijo con mínimos completos** (3) | SelectiveSafety 480×540 min 400×300 (`:160-163`); PerPost 340×460 min 300×260 (`:922-925`); Defensa `780/670`×580 min 600×360 (`:67-70`) | — |
| **`SizeToContent.Height`** (3) | Segments 400 + `NoResize`, **sin mínimos** (`:47-50`); Layout 480, `MinWidth=440`, **sin `MinHeight`** (`:47-49`); Fill 500, `MinWidth=460`, **sin `MinHeight`** (`:44-46`) | Son las **tres únicas** ventanas del repositorio con `SizeToContent` |

En C, a diferencia de las cuatro Cantilever de I-39A, **`MinWidth` nunca es letra muerta** (el piso
del `Math.Max` ya lo supera en las cuatro calculadas) y **`MinHeight` sí clampea** en rejillas
pequeñas (Tope y Guía con `maxLevels ≤ 1`; Parrilla con `maxLevels = 0`). La anomalía de I-39A **no
se repite aquí**; la que sí hay es otra y es de Segments.

### 2.4 D: qué comparte y qué no

- **Ninguna de las seis tiene par Aceptar/Cancelar.** Las acciones son «Exportar Excel / Exportar
  CSV / Cerrar» (`RackBomWindow.xaml:150,155,160`; `RackConsolidatedBomWindow.xaml:114-116`), «Abrir
  / Refrescar / Cerrar» (`RackDesignLibraryWindow.xaml:55-57`), «Ir al rack / Cerrar»
  (`RackListWindow.xaml:62-63`), diez tarjetas de navegación + «Cerrar»
  (`RackMainMenuWindow.xaml:76-139, 168`) y solo «Cerrar» (`RackCommandHelpWindow.cs:47`). **El
  reproche del ADR a C no aplica a D.**
- **Las seis llevan `IsCancel` en «Cerrar»**; el defecto de Push Back no se repite.
- **`IsDefault` solo en dos**, y en ambas sobre una acción segura y contextual: «Ir al rack»
  (`RackListWindow.xaml:62`) y «Abrir» (`RackDesignLibraryWindow.xaml:55`). Su ausencia en las dos
  de BOM es **correcta**: «Exportar Excel» materializa un archivo.
- **Excepción dura:** `RackCommandHelpWindow.cs:47` declara `IsDefault = true, IsCancel = true` en
  **el mismo botón**. `EditorAction` **lanza `ArgumentException`** exactamente ante esa combinación
  (`src/RackCad.UI/Shell/EditorAction.cs:24-29`).
- **Foco inicial: solo dos lo declaran**, y ambas por código con el motivo escrito —
  `RackDesignLibraryWindow.xaml.cs:37-41` («Preselect the first design so Enter ('Abrir') works
  without a prior click») y `RackListWindow.xaml.cs:25-29`.
- **Tres delegan el color de estado** en `UiSupport.SetStatus` booleano
  (`RackMainMenuWindow.xaml.cs:78-83`, `RackDesignLibraryWindow.xaml.cs:66`,
  `RackListWindow.xaml.cs:55`) y **tres no tienen banda de estado**. En D **no hay deuda de pintura
  a mano**: la deuda de color es íntegramente de C.
- **Cero `OnClosing` en las seis.**
- **Tres de las tres XAML de consulta comparten líneas 9-19 byte-idénticas** (CenterOwner + Segoe UI
  + `WindowBackgroundBrush` + merge + `Grid Margin="16"`), y **tres coinciden exactamente en
  720×480 min 520×320** (`RackDesignLibraryWindow.xaml:5-8`, `RackBomWindow.xaml:5-8`,
  `RackListWindow.xaml:5-8`) — convergencia real, aprovechable sin cambiar un píxel.

---

## 3. Qué puede hacer `RackDialogWindow` por ellos y qué no

### 3.1 Lo que es, medido

Archivo de **132 líneas**. Superficie pública: **un constructor sin parámetros**. Todo lo demás es
`protected`: `SetStatus` (`:49`), `CreateActionBar` (`:53`), `Accept` (`:90`), `Cancel` (`:103`).
**Es utilizable exclusivamente por herencia**; no hay forma de componerla.

**Lo que aporta:**

- Cuatro asignaciones de chrome en el constructor (`:35-45`): `FontFamily = "Segoe UI"`,
  `CenterOwner`, merge de `AppStyles.xaml`, `Background` desde `WindowBackgroundBrush`. Es
  **exactamente** el bloque que las diez C repiten a mano.
- `Accept()`/`Cancel()` que fijan `DialogResult` **envuelto en `try/catch(InvalidOperationException)`
  con caída a `Close()`** (`:90-113`). Es el único comportamiento no trivial de la clase, y encaja
  con el patrón «validar y llamar a `base.Accept()`» que los diez `OnOk` ya practican.
- `CreateActionBar`, que produce `Aceptar(IsDefault)` + `Cancelar(IsCancel)` cableados.

**Lo que NO hace:**

- **No fija `Title`, `Width`, `Height`, `MinWidth`, `MinHeight`, `SizeToContent`, `ResizeMode` ni
  `Owner`.** No entrega ningún contrato de tamaño (D9).
- **No tiene plantilla, layout ni `Content`**: no coloca la barra que fabrica; la subclase debe
  insertar `bar.Panel` en su propio árbol. Los diez constructores manuales **sobreviven** a la
  adopción.
- **No tiene `OnClosing`**: `Alt+F4` y el botón de sistema **no atraviesan** `Accept`/`Cancel`. La
  frase de D7 «el cierre por botón, por Escape, por `Alt+F4` y por el botón de sistema atraviesa la
  misma política» **no la entrega esta clase**.
- **No entrega D6**: construye `Button` crudos, no consume `EditorAction`, así que no hay
  `DisabledReason` ni `ToolTipService.ShowOnDisabled`.

### 3.2 Dónde encaja y dónde no

`CreateActionBar` fabrica un `DockPanel(LastChildFill=false, Margin 0,12,0,0)`, ancla los `leading`
a la **izquierda** (`:66`) y pone los dos botones a la derecha con **`MinWidth = 96` y sin
`Padding`** (`:71`, `:75`; `cancel` con `Margin(8,0,0,0)`).

| Ventana C | ¿Encaja `CreateActionBar`? | Delta observable |
|---|---|---|
| `RackWarehouseLayoutWindow`, `RackWarehouseFillWindow` | **Casi entero**: dos botones exactos, validación en `OnOk` que no cierra al fallar | Etiqueta primaria («Colocar»/«Calcular») si no se pasa `acceptText`; `Padding` 16,3/10,3 → estilo (16,8 / 14,7); `Margin` de la barra 0,14 → 0,12 |
| `SelectiveSafetyWindow`, `SafetyDefensaGridWindow` | Dos botones; encaja salvo métricas | `Padding`, `MinWidth` |
| `SafetyPerPostWindow` | Tres botones: «Todos por defecto» iría como `leading` | **Se movería de la derecha a la izquierda** |
| Tope, Parrilla, Guía, Desviador | **No encaja de forma**: cuatro botones en un `StackPanel HorizontalAlignment=Right` | Todos/Ninguno **cruzan la ventana** hasta el borde izquierdo |
| `SelectiveSegmentsWindow` | No: tiene **tres** terminaciones, y la tercera cierra con éxito | «Sin medio frente» no tiene sitio en un modelo Aceptar/Cancelar |

Además, la firma `CreateActionBar(string acceptText = "Aceptar", string cancelText = "Cancelar",
params UIElement[] leading)` **impide la invocación natural**: `CreateActionBar(all, none)` no
compila; hay que reescribir las dos etiquetas para pasar un solo elemento. La única variación que la
base soporta es la más incómoda de invocar, y afecta a las cuatro rejillas.

### 3.3 El obstáculo estructural: herencia y contrato de tamaño por estilo son excluyentes

La base asigna `Background` y `FontFamily` como **valor local** en el constructor (`:37`, `:42-44`).
En precedencia WPF el valor local **gana al setter de un `Style`**. Los arquetipos A y B ya tienen
su contrato como estilo de ventana (`AppStyles.xaml:165` y `:183`), ambos con
`ShellWindowBackgroundBrush`. **Un futuro `DialogWindowStyle` para C no podría cambiar el fondo ni
la tipografía de una ventana que herede de esta base.** Y su token es `WindowBackgroundBrush`
#F4F6F9 (`:8`), que **ya no es el de A ni el de B** (#EEF2F6, `:23`).

### 3.4 Cobertura de la base: aparente, no real

`tests/RackCad.UI.Tests/RackDialogWindowTests.cs` tiene 5 hechos, y su doble **sobreescribe `Accept`
y `Cancel` sin llamar a `base`**: los cuerpos reales (`DialogResult`, el `catch`, el `Close` de
reserva) **nunca se ejecutan**. `SetStatus` y la sobrecarga con `leading` no se llaman nunca. El
único comportamiento propio de la clase está sin cobertura.

### 3.5 Respuesta a la pregunta: herencia, composición o ninguna

- **Herencia**: técnicamente posible para **cuatro** ventanas (las dos de almacén, `SelectiveSafety`,
  `Defensa`) con un delta observable acotado a métricas y etiqueta; **descartable** para las cuatro
  rejillas por forma y para `Segments` por semántica; **con colisión de miembro** en Guía.
- **Composición**: **imposible hoy** — no hay un solo miembro público utilizable. Habría que
  promover `CreateActionBar` a helper estático público, momento en el cual la clase deja de ser
  necesaria como ancestro.
- **Ninguna de las dos**: es la lectura honesta del conjunto. La forma correcta ya existe y está
  probada en el repositorio: `RackBoundedEditorShell` es un `Control` lookless con slots que mergea
  `ShellResources.Shared` solo si el token no resuelve, precisamente para servir a ventanas
  construidas en código.

**Marco normativo, textual:** ADR-0019 D2 prohíbe la herencia **para los editores ricos**, y al
descartar la alternativa escribe «Además `RackDialogWindow` nació para diálogos, no para editores
ricos». ADR-0029 cierra: «No adopta `RackDialogWindow` como ancestro de los **editores ricos** —eso
lo prohíbe ADR-0019 D2 y sigue vigente—; su posible papel en el arquetipo C es una decisión separada
de I-39D» (`docs/adr/0029-…md:228-230`). **El papel en C está explícitamente abierto y asignado a
I-39D.**

---

## 4. Qué unificar y qué dejar en paz

Criterio aplicado, los tres a la vez: **(a) al menos dos consumidores reales, (b) pertenece al
contrato funcional de C/D, (c) no cambia reglas de negocio.**

### 4.1 Unificar — pasa los tres filtros

| Candidato | Consumidores | Nota |
|---|---|---|
| **Bloque de chrome de ventana** (`CenterOwner` + Segoe UI + merge + fondo) | 10 en C + `RackCommandHelpWindow` | Cero regla de negocio. **Pero destapa** el defecto de `SafetyDefensaGridWindow` (§2.2): eso es cambio visible y va al Owner |
| **Claves de recurso muertas** — `MutedTextBrush` (`SafetyParrillaGridWindow.cs:288`), `AccentBrush`/`SubtleTextBrush`/`TextBrush` (`RackCommandHelpWindow.cs:28-30`). **Verificado: cero `x:Key` con esos nombres en todo `src/`** | 2 ventanas, 4 búsquedas | Hoy el código **simula** estar tematizado y siempre gana el literal de respaldo. Retirar el `TryFindResource` muerto conservando el color actual **no cambia un píxel** |
| **Aviso no bloqueante → severidad `Warning`** | 1 sitio (`SafetyDesviadorGridWindow.cs:190`) | Falla el filtro (a) por poco, pero es el caso exacto para el que se escribió `UiSupport.SetStatus(…, EditorStatusSeverity)`; **cambia el color** (#FF8C00 → #B7791F) ⇒ Owner |
| **Token de tamaño de la ventana de lista D** | 3 (Library, Bom, List: 720×480 min 520×320 idénticos) | Convergencia ya existente: tokenizar **no mueve nada** |
| **`Enter`/`Escape` declarados vía `EditorAction`** | 15 de 16 (la 16.ª es la excepción de `RackCommandHelpWindow`) | Desde I-39C `EditorActions.Button` transporta `IsDefault`/`IsCancel` (`EditorAction.cs:82-83`): el obstáculo técnico que el censo §8.3 registró **ha desaparecido** |

### 4.2 Dejar en paz — falla al menos un filtro

| Candidato | Filtro que falla | Evidencia |
|---|---|---|
| **Los cuatro mapeos `SafetySide` → `ComboBox`** | (c) **regla de negocio** | Tope y Desviador **no ofrecen «Ninguno»** porque la presencia la decide la matriz, y colapsan cualquier valor no-Left/Right a `Both` (`SafetyTopeGridWindow.cs:194-202`, `SafetyDesviadorGridWindow.cs:363-372`). Un conjunto único de opciones crearía un estado sin mapeo definido |
| **La base de índice del combo de bota** | (c) | `SelectiveSafetyWindow.cs:448` castea el índice al enum **directamente**; el orden del array de `:22` es dependencia no declarada que ningún compilador ni prueba vigila |
| **Encapsular el par `«Lado:»` + combo** | (b)/(c) | `tests/RackCad.UI.Tests/SafetyDialogTestSupport.cs:35-36` detecta el selector por el **literal** «Lado:», y es la única guarda viva de PB-003 y PB-006 |
| **`SelectionMatrix` en `SafetyDefensaGridWindow`** | (c) | Su celda no es booleana; exigiría evolucionar el control a celdas de captura — modelo nuevo, no adopción (D11) |
| **`EditorActionBar` en las tres de D con primaria antes que secundaria** | (c) | Orden fijo `leading→secondary→primary→trailing` (`Shell/EditorActionBar.cs:17-20`) frente a `RackDesignLibraryWindow.xaml:55-56`, `RackBomWindow.xaml:150-155`, `RackConsolidatedBomWindow.xaml:114-115`: **invierte los botones** |
| **Los diez botones-tarjeta del menú** | (c) | Su `Content` es un `StackPanel` de dos `TextBlock` sobre `ControlTemplate` propio (`RackMainMenuWindow.xaml:26-41, 76-79`); `EditorActions.Button` fuerza `Content = string`. Rompería dos pruebas verdes |
| **Política de cierre en C/D** | (b) | Ninguna tiene ámbito transaccional propio: Cancelar/Escape **ES** el descarte declarado del borrador y el llamador conserva su estado. D8 admite «no aplicable» — hay que **probarlo**, no sustituirlo |
| **`PromptSaveToLibrary`, `LoadCatalogSafe`, `ToOptions`, `TryOptionalNum`** | (a) | Cero llamadas desde C y desde D. No son duplicación pendiente: son helpers que estos arquetipos legítimamente no usan |
| **Los `Width`/`Height` calculados de las cuatro rejillas** | (c) | Adaptación al tamaño de la matriz, no descuido |
| **`SafetyPerPostWindow` a su propio archivo** | (b) | No rompe la guarda (refleja por tipo) pero deja obsoletas tres piezas de documentación normativa que citan ese archivo como prueba de por qué la unidad es el tipo |

### 4.3 Dos defectos de producto detectados por lectura — se **registran**, no se arreglan

1. **Diagnóstico obsoleto que no se limpia.** `SafetyDefensaGridWindow` asigna `error.Text` en `:320`
   y `:330` y **nunca lo borra** (cero `TextChanged` en el archivo); `SafetyDesviadorGridWindow` solo
   lo limpia con `showError = true` (`:315`) mientras la ruta viva pasa `showError: false` (`:233`).
   Cualquier chrome que limpie el estado al recomputar los **arregla como efecto colateral** ⇒ D13.
2. **`SelectionMatrixBulkBar` no fija `ToolTipService.ShowOnDisabled`** (`Controls/SelectionMatrixBulkBar.cs:117-121`,
   cero ocurrencias): el motivo de bloqueo **existe en el modelo y no es legible** con el botón
   apagado, pese a que `tests/…/SafetyGridBulkAdoptionTests.cs:244-245` lo asevera. Es la única
   cobertura de D6 en C y está a medias.

---

## 5. Qué es observable para el usuario (⇒ validación manual) y qué no

### 5.1 Observable — exige gate del Owner (D13)

| Cambio | Efecto medible |
|---|---|
| Adoptar `CreateActionBar` o `EditorActions.Button` | `Padding` 16,3/10,3 → el del estilo (16,8 / 14,7) en 10+1 ventanas; `MinWidth = 96` donde hoy no hay; `Margin` de barra 0,14 → 0,12 en las dos de almacén |
| `leading` de la base | «Todos/Ninguno» (4 rejillas) y «Todos por defecto» (PerPost) **cruzan a la izquierda** |
| `EditorActionBar` en D | **Invierte** primaria y secundaria en Library, Bom y ConsolidatedBom |
| Etiqueta primaria por defecto | «Colocar» y «Calcular» → «Aceptar» |
| Unificar la paleta de estado | **Siete** ventanas de #B22222 → #B00020; **una** de #FF8C00 → #B7791F |
| Completar el chrome de `SafetyDefensaGridWindow` | Tipografía y fondo cambian en esa ventana |
| `CenterOwner` → `CenterScreen` en las cinco de §6 | **Cambia dónde aparecen** cinco ventanas |
| Cualquier reordenamiento del árbol | Mueve el **foco inicial** y la tabulación: hoy son emergentes y divergen (Guía enfoca el botón «Todos», que reescribe toda la matriz; Tope un `CheckBox`; Parrilla otro `CheckBox`; Defensa y Desviador visitan la barra antes que el contenido) |
| Deshabilitar la primaria hasta que la entrada sea válida | Hoy **ninguna** lo hace: valida al pulsar |
| Definir `AccentBrush`/`SubtleTextBrush`/`TextBrush`/`MutedTextBrush` como tokens | Cambia el aspecto de dos ventanas de golpe |
| Fijar `Height`/`MinHeight` comunes | Rompe las tres de `SizeToContent` y clampea las cuatro calculadas |
| Estilar el botón «✕» de Segments (`:169-175`, **el único sin estilo compartido**; `AppStyles.xaml` no tiene estilo implícito de `Button`) | Cambia aspecto y ancho dentro de una ventana de 400 px |

### 5.2 No observable — verificable solo por pruebas y CI

- Retirar los cuatro `TryFindResource` muertos **conservando** el literal actual.
- Sustituir el bloque de chrome por una fuente única **cuando** la ventana ya asigna las cuatro
  propiedades (nueve de diez en C).
- Tokenizar 720×480 / 520×320 en las tres de D que ya coinciden.
- Cambiar el **canal** de construcción del botón conservando explícitamente `Content`, `Style`,
  `Padding`, `Margin`, `MinWidth`, `IsDefault` e `IsCancel` (el piloto ya demuestra la mitigación:
  fijar la propiedad después de construir, `StructuralSectionInspectorWindow.cs:231-232`).
- Escribir caracterización.

### 5.3 Lo que hoy **no** es verificable con las utilidades existentes

- **Cero `ShowDialog` y cero asignaciones de `Owner`** en todo `tests/`. `CenterOwner` es
  inverificable, y sin `Owner` WPF **degrada `CenterOwner` a `CenterScreen` en silencio**.
- **Cero pulsaciones de tecla** (`KeyEventArgs`, `Key.Enter`, `Key.Escape`, `Key.Tab` = 0). El
  patrón vigente y suficiente es leer `IsDefault`/`IsCancel`: hay que declarar que se caracteriza la
  **declaración del rol**, no el enrutado de la tecla.
- **Una sola aserción de camino de cierre en las dieciséis**:
  `tests/RackCad.UI.Tests/RackMainMenuStructuralSectionTests.cs:112` (`window.Closed += …`).
- **`MessageBox` y `SaveFileDialog` sin costura.** Reparto medido: `SafetyParrillaGridWindow` 2
  `MessageBox.Show`; `RackBomWindow` 4 + 2 `SaveFileDialog`; `RackConsolidatedBomWindow` 4 + 2;
  `RackMainMenuWindow` 3 + 1 `OpenFileDialog`. **Las otras doce: cero y cero.**

### 5.4 Cobertura actual, medida por construcción real

**Siete** de las dieciséis se construyen en pruebas; **nueve nunca**.

| Se construyen | Nunca se construyen |
|---|---|
| SelectiveSafety (4), Tope (6), Parrilla (1), Guía (6), Desviador (7), Defensa (8), MainMenu (13) | **SafetyPerPost, SelectiveSegments, WarehouseLayout, WarehouseFill, DesignLibrary, Bom, ConsolidatedBom, List, CommandHelp** |

Y **toda** la cobertura existente es funcional (modelo de matriz, celdas dormidas, resultado tipado,
alcance del bulk, vocabulario por sistema): **ninguna** aserción de teclado, cierre, foco,
tabulación, tamaño u ownership.

---

## 6. Qué ventanas NO pueden recibir `Owner`, y por qué

**Diez de dieciséis reciben `Owner` siempre**, en todos sus sitios de construcción, verificado uno a
uno: las ocho de C abiertas desde un editor rico o desde `SelectiveSafetyWindow`
(`RackSelectiveWindow.xaml.cs:700,1010`; `RackDynamicSystemWindow.xaml.cs:576`;
`RackPushBackSystemWindow.xaml.cs:1584,1606`; `SelectiveSafetyWindow.cs:533,547,595-597,610-612,635-651,667`)
más `RackDesignLibraryWindow` (`RackMainMenuWindow.xaml.cs:190`) y `RackBomWindow` (**ocho**
llamadores, los ocho con `{ Owner = this }`). Para ellas D9 **se cumple hoy**; una migración solo
debe no perderlo.

**Seis no pueden recibir `Owner`.** Todas comparten la misma causa estructural: su **único**
constructor vive en un comando de `RackCad.Plugin` y se muestran con
`AcApplication.ShowModalWindow`, sin ninguna ventana padre WPF viva.

| Ventana | Arq. | Único sitio de construcción | Comando |
|---|---|---|---|
| `RackWarehouseLayoutWindow` | C | `src/RackCad.Plugin/RackLayoutCommands.cs:84-85` | `RACKLAYOUT` / `RLY` |
| `RackWarehouseFillWindow` | C | `src/RackCad.Plugin/RackLayoutCommands.Fill.cs:72-73` | `RACKRELLENAR` / `RR` |
| `RackMainMenuWindow` | D | `src/RackCad.Plugin/RackMenuCommands.cs:27-30` | `RACKMENU` |
| `RackListWindow` | D | `src/RackCad.Plugin/RackInventarioCommands.cs:87-88` | `RACKLISTA` / `RL` |
| `RackConsolidatedBomWindow` | D | `src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs:114` | `RACKBOMTOTAL` / `RB` |
| `RackCommandHelpWindow` | D | `src/RackCad.Plugin/RackAyudaCommands.cs:18` | `RACKAYUDA` / `RA` |

**Por qué no es deuda sino excepción.** `Window.Owner` exige una instancia de
`System.Windows.Window`, y en ese punto no hay ninguna: la única fuente WPF de `Owner` del repositorio
es `RackMainMenuWindow` vía `RackEditorLaunchContext` (`Editor/RackEditorLaunchContext.cs:22-23`,
consumido en los catorce puntos de `Editor/EditorModules.cs`), y no está viva en estos flujos;
`AcApplication.MainWindow` **no se referencia en ningún punto de `src/`** y no es un `Window` de WPF;
y **no hay ni un uso de `WindowInteropHelper` en el repositorio**. D9 condiciona el `Owner` a que
**exista** una ventana padre WPF: **las seis lo cumplen por vacío**.

**Lo que sí es un defecto medido, y es otro.** **Cinco de esas seis declaran `CenterOwner` sin que
pueda existir `Owner`**: `RackWarehouseLayoutWindow.cs:50`, `RackWarehouseFillWindow.cs:47`,
`RackListWindow.xaml:9`, `RackConsolidatedBomWindow.xaml:9`, `RackCommandHelpWindow.cs:23`. Solo
`RackMainMenuWindow` declara `CenterScreen` (`RackMainMenuWindow.xaml:9`). En todo el repositorio
únicamente **dos** ventanas declaran `CenterScreen`, y son precisamente las dos que documentaron su
procedencia de comando: esa y `StructuralSectionInspectorWindow.cs:61` (justificada en el censo
`:87-89`).

**Cómo documentarlo en vez de forzarlo.** El texto de la excepción debe decir: *«esta ventana no
recibe `Owner` porque su único llamador es un comando de AutoCAD y no existe ventana padre WPF; su
ubicación se documenta con su motivo (D9)»*. Y **la corrección de `CenterOwner` a `CenterScreen` es
un cambio de ubicación observable de cinco ventanas ya validadas**: decisión del Owner, aislada, no
efecto colateral de una limpieza de chrome. Introducir `WindowInteropHelper` crearía un **segundo
modelo de ownership** junto al de `RackEditorLaunchContext`, que es lo que D11 prohíbe.

**Consecuencia directa para el plan:** las dos ventanas de almacén **no deben heredar de
`RackDialogWindow`**, cuyo constructor impone `CenterOwner` (`:36`). Heredarlo convertiría un defecto
hoy visible en una herencia invisible.

---

## 7. Plan de fases para I-39D, por riesgo creciente

Regla transversal, de D13: **cada fase caracteriza ANTES de tocar**, la caracterización no se edita
para que pase, y ningún cambio observable viaja dentro de una fase de adopción.

### Fase 0 — Higiene documental y de código muerto *(riesgo: nulo)*
**Se caracteriza antes:** nada; no hay comportamiento en juego.
- Refrescar el censo: cuatro punteros caducos (§1.2) y las dos afirmaciones consumidas por I-39C.
- Retirar los cuatro `TryFindResource` a claves inexistentes **conservando el literal actual**
  (`SafetyParrillaGridWindow.cs:288`, `RackCommandHelpWindow.cs:28-30`).
- Registrar como deuda declarada, sin tocar: `ShowOnDisabled` ausente en `SelectionMatrixBulkBar`;
  `IsDefault`+`IsCancel` juntos en `RackCommandHelpWindow.cs:47`; `error.Text` que no se limpia en
  Defensa y Desviador; `CenterOwner` sin `Owner` en cinco ventanas; `SelectiveSegmentsWindow` sin
  scroll ni tope de crecimiento.
**Salida:** censo citable; cero deltas visuales.

### Fase 1 — Caracterización de las siete ventanas sin bloqueadores modales *(riesgo: bajo)*
**Se caracteriza:** por ventana — `Content`, `Style`, `Padding`, `Margin`, `MinWidth`, `IsDefault` e
`IsCancel` de cada botón; el orden de los hijos del contenedor raíz (que **es** hoy el orden de
tabulación); `Width`/`Height`/`MinWidth`/`MinHeight` efectivos incluidos los calculados;
`WindowStartupLocation`; ausencia de `OnClosing`; `Result` tras cada camino; el **orden y el texto
exacto** de las validaciones (7 comprobaciones en Layout, 6 en Fill, primer-fallo-gana), que hoy es
su único contrato observable y no lo fija nada.
**Ventanas:** `RackListWindow` y `RackDesignLibraryWindow` (las dos mejores: su contrato de Enter ya
está **escrito en comentario** por el autor), después `SafetyPerPostWindow`,
`SelectiveSegmentsWindow`, `RackWarehouseLayoutWindow`, `RackWarehouseFillWindow` y
`RackCommandHelpWindow`.
**Utillaje nuevo, mínimo:** localizadores por **rol** (`IsDefault`/`IsCancel`, tipo) encima de
`EditorWindowTestSupport.Descendants` — **no** un decimocuarto recorrido de árbol; y un helper que
ejecute la aceptación tragando solo `InvalidOperationException` (patrón ya usado en
`StructuralSectionInspectorWindowTests.cs:71-81`, válido porque en las dieciséis `Result` se asigna
**antes** de `DialogResult`).
**No se hace:** simular teclas; introducir `ShowDialog`; añadir costuras dentro del código de
producto.

### Fase 2 — Chrome de ventana en una sola fuente, para las nueve que ya lo tienen completo *(riesgo: bajo-medio)*
**Se caracteriza antes:** `FontFamily`, `Background`, `WindowStartupLocation` y el diccionario
mergeado, ventana por ventana, más el tamaño efectivo de apertura.
- Nueve ventanas C con las cuatro asignaciones idénticas → una sola fuente, **sin heredar** de
  `RackDialogWindow` (§3.3: la herencia bloquea cualquier contrato de tamaño por estilo, y §6: impone
  `CenterOwner` a las dos de almacén).
- **`SafetyDefensaGridWindow` queda FUERA de esta fase**: completarle el chrome es cambio visible y
  va a la fase 5.
**Salida:** diff visual vacío verificable por caracterización.

### Fase 3 — Contrato de tamaño de C y D, solo donde ya converge *(riesgo: medio)*
**Se caracteriza antes:** el tamaño real de apertura de las dieciséis, incluidos los cuatro
calculados en sus extremos (`maxLevels = 0, 1, n`).
- **D**: tokenizar 720×480 / 520×320 (Library, Bom, List) — coincidencia exacta ya existente.
- **C**: definir el estilo hermano con **solo** `Background`, `FontFamily`, `FontSize`. **NO**
  `Width`/`Height`/`MinWidth`/`MinHeight`: cuatro ventanas calculan su tamaño de los datos y tres
  usan `SizeToContent.Height` y omiten `MinHeight` **a propósito**. Un token de mínimos común
  reproduciría en C la anomalía que I-39A midió en Cantilever y que I-39C acaba de cerrar.
**Salida:** contrato de tamaño de C declarado **con su excepción escrita**, no impuesto.

### Fase 4 — Piloto de `EditorActions.Button` en los diálogos de dos botones *(riesgo: medio)*
**Se caracteriza antes:** ya hecho en la fase 1 para las candidatas.
**Candidatas:** `RackWarehouseLayoutWindow` (`:124-126`), `RackWarehouseFillWindow` (`:103-105`),
`SafetyDefensaGridWindow` (`:92-107`). Dos botones, sin `x:Name`, sin acción deshabilitada, sin
tercer grupo, sin conflicto de orden.
**Se conserva explícitamente** `Padding`, `Margin` y etiqueta fijando la propiedad después de
construir (patrón del piloto, `StructuralSectionInspectorWindow.cs:231-232`).
**Fuera:** las cuatro rejillas (Todos/Ninguno), `SafetyPerPostWindow`, `SelectiveSegmentsWindow` (su
tercera terminación), `RackCommandHelpWindow` (lanzaría `ArgumentException` **en tiempo de
ejecución**, no de compilación) y los diez botones-tarjeta del menú.

### Fase 5 — Cambios observables, uno a uno, cada uno con su gate *(riesgo: alto — decisión del Owner)*
**Se caracteriza antes:** todo lo de las fases 1-2, más una foto del estado actual como línea base.
Cada punto es una decisión **independiente**, no un lote:
1. Completar el chrome de `SafetyDefensaGridWindow` (tipografía + fondo).
2. `CenterOwner` → `CenterScreen` en las cinco ventanas sin padre WPF posible.
3. Paleta de estado: #B22222 → #B00020 en siete ventanas; #FF8C00 → #B7791F en una.
4. Género cruzado «Ambos»/«Ambas», «Izquierda»/«Izquierdo».
5. Posición de Todos/Ninguno y de la banda de diagnóstico si se adopta una barra común.
6. Foco inicial determinista donde hoy recae sobre una acción de escritura masiva
   (`SafetyGuiaEntradaGridWindow`, botón «Todos» que ejecuta `model.SetAll(true)`).
7. `ShowOnDisabled` en `SelectionMatrixBulkBar`.
8. Limpieza del diagnóstico obsoleto en Defensa y Desviador.

### Fase 6 — Decidir el destino de `RackDialogWindow` *(riesgo: estructural)*
**Se caracteriza antes:** las tres rutas de su propia clase que **hoy nadie ejecuta** (`DialogResult`,
el `catch`, el `Close` de reserva), o se declara que no vale la pena escribirlas porque se retira.
Tres salidas posibles, con la evidencia ya sobre la mesa: **(a)** ancestro de las cuatro ventanas que
encajan; **(b)** hacerla componible (subir `CreateActionBar` a helper estático público que reciba
`EditorAction`, y dejar de asignar `Background`/`FontFamily` como valor local); **(c)** retirarla en
un paso propio, **después** de que su sustituto tenga adoptantes. Lo que la medición descarta es un
**ancestro único para las diez**: encajan cuatro y no encajan seis, y forzarlo degeneraría en
`protected virtual` por variación — literalmente la alternativa que ADR-0029 descartó.

### Fases fuera de alcance, declaradas
`MessageBox`/`SaveFileDialog` sin costura en `RackBomWindow`, `RackConsolidatedBomWindow`,
`RackMainMenuWindow` y `SafetyParrillaGridWindow`; unificación de los mapeos `SafetySide`; migración
de los botones-tarjeta del menú; `EditorStatusPresenter` (adoptarlo pone en rojo, a propósito,
`tests/RackCad.UI.Tests/RichEditorCharacterizationTests.cs:349`).

### Gate documental
**`docs/initiatives/` contiene hoy solo I-39A, I-39B e I-39C.** No existe contrato de I-39D. La fila
`I-39D` figura en `docs/ROADMAP.md` como dependiente de las tres (`:366-368`, «queda solo I-39D, con
la que se cierra la línea»), pero el contrato de la subiniciativa hay que escribirlo **antes** de
tocar código. Regla vigente del repositorio: no se reclama por cuenta propia.

---

## 8. Contradicciones entre las once auditorías, resueltas por medición

Se listan sin promediar. En cada caso se indica quién acierta y qué dice el código.

1. **Número de mapeos `SafetySide`.** «safety-rejillas-1» dice cuatro; «safety-rejillas-2» dice
   **cinco `ComboBox`**; «duplicacion» dice **cuatro `ComboBox` + dos helpers de texto = seis
   lugares**. **Medido: acierta «duplicacion».** Los `ComboBox` son cuatro
   (`SelectiveSafetyWindow.cs:22`+`:447-448`; `SelectiveSafetyWindow.cs:906`+`:972-973`,`:1004`;
   `SafetyTopeGridWindow.cs:27`+`:184-202`; `SafetyDesviadorGridWindow.cs:145-147`+`:363-382`) y los
   helpers de texto **dos**: `DesviadorSideName` (`SelectiveSafetyWindow.cs:623-631`) y `SideName`
   (`:985-994`). El quinto «ComboBox» de «safety-rejillas-2» es en realidad el primer helper.
2. **Bloqueadores modales de `RackMainMenuWindow`.** «cobertura-existente» dice «un `SaveFileDialog`
   y **seis** `MessageBox`»; «utilitarias-navegacion» dice **tres** `MessageBox` y un
   `OpenFileDialog`. **Medido: acierta «utilitarias-navegacion»** — tres `MessageBox.Show`
   (`:164`, `:202`, `:216`) y un **`OpenFileDialog`** (`:88`), no `SaveFileDialog`. El total real de
   las tres ventanas con exportación es **11 `MessageBox.Show` y 4 `SaveFileDialog`**, no 14 y 4.
3. **Cuántos sitios reconstruyen el par Aceptar/Cancelar.** «duplicacion» dice «9 sitios» y luego
   «11». **Medido: son 10** — una barra por ventana de C, en tres formas distintas (línea única ×7,
   fábrica local en Guía, inicializador multilínea en Defensa, y Guía además usa su fábrica también
   para Todos/Ninguno).
4. **«`EditorAction` con cero consumidores productivos».** El censo §8.2 lo dice y varias auditorías
   lo repiten; «utilitarias-navegacion» lo corrige. **Medido: hay uno**
   (`StructuralSectionInspectorWindow.cs:223`,`:227`). Siguen en cero `EditorActionBar` fuera de la
   plantilla del shell A y `EditorStatusPresenter`.
5. **Cobertura de pruebas por ventana.** Contar **menciones del nombre** en `tests/` da 16 de 16 con
   «cobertura»; contar **construcciones reales** (`new X(`) da **7 de 16**. La cifra correcta para
   D13 es la segunda: nueve ventanas nunca se construyen (§5.4). Cualquier métrica publicada debe
   declarar cuál cuenta.
6. **`SafetyDefensaGridWindow` y el chrome.** «safety-rejillas-2» y «duplicacion» afirman que omite
   `FontFamily` y `Background`; ninguna otra auditoría lo menciona. **Verificado y confirmado**:
   `grep` de ambos en el archivo devuelve **cero**.
7. **Anomalía de tamaño de las cuatro Cantilever.** Varias auditorías la citan como vigente (viene
   del censo §4). **Medido: ya no existe** — `BoundedEditorWindowStyle` y sus cuatro tokens están en
   `AppStyles.xaml:62-65` y `:183-187`, consumidos por los cuatro XAML de componente.
8. **Punto donde las once coinciden y lo verifiqué igualmente por ser la base del plan:** cero
   `OnClosing` en las dieciséis; cero subclases productivas de `RackDialogWindow`; quince de
   dieciséis declaran `CenterOwner`; y las tres únicas ventanas del repositorio con `SizeToContent`
   son de C.

---

## 9. Correcciones que la caracterización impuso a esta auditoría

La auditoría se hizo **leyendo**; la caracterización se hizo **construyendo las dieciséis ventanas**.
Cinco afirmaciones no sobrevivieron a la ejecución y se corrigen aquí en vez de dejarlas en pie. Es
exactamente la razón por la que ADR-0029 D13 exige caracterizar, y no auditar solamente.

| § | La auditoría decía | Lo que la ejecución mide | Consecuencia |
|---|---|---|---|
| 4.1 | «tres de D declaran 720×480 / 520×320» | **Dos**: biblioteca y lista. `RackBomWindow` es **740×520** y solo comparte los **mínimos** | Se tokeniza lo que converge de verdad: dos tamaños completos y tres mínimos |
| 5.1, 7 | «tres ventanas usan `SizeToContent`» | **Dos**: las de almacén. `SafetyPerPostWindow` declara su alto como las demás | El contrato de tamaño de C tiene **dos** excepciones, no tres |
| 2.2 | «completar el chrome de Defensa cambia tipografía **y** fondo» | Solo el **fondo**: `FontFamily` resuelve a Segoe UI igualmente por ser la predeterminada del sistema | El delta observable es **la mitad** del anunciado: hoy abre en blanco liso en vez del `#F4F6F9` compartido |
| 4.1 | «siete ventanas usan `Firebrick`» | **Seis** de las diez, más `SelectiveSegmentsWindow`. Parrilla y Guía **no** pintan aviso con color propio | La unificación de paleta afecta a siete archivos, no a ocho |
| 7 | «con los campos en su estado inicial, `Colocar` no produce resultado» | **Sí** lo produce: la ventana se autorrellena con valores válidos y la validación pasa | Lo que hay que caracterizar es el camino **inválido**, no el inicial |

Ninguna de las cinco cambia el plan de fases; todas cambian el **texto** de lo que hay que preservar,
que es justo lo que una caracterización sirve para descubrir antes de tocar nada.

## 10. Trazabilidad

- Caracterización: `tests/RackCad.UI.Tests/DialogWindowCharacterizationTests.cs` — las dieciséis
  construidas por primera vez en una prueba.
- Guarda viva del censo: `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs`.
- Contrato funcional: [`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md`](../../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md)
  (D1, D6, D7, D8, D9, D11, D12, D13 y «Lo que este ADR NO decide», `:228-230`).
- Censo: [`I-39A-censo-ventanas.md`](I-39A-censo-ventanas.md) — **refrescado por la fase 0 de I-39D**.
- ADR-0019 D2 y su descarte de la alternativa de herencia (`:46-47` y `:70-74`).
- Decisiones del Owner: [`../decisions/I-39.md`](../decisions/I-39.md).
- ROADMAP Fase 3, filas I-39, I-39A, I-39B, I-39C (`docs/ROADMAP.md:365-368`).
