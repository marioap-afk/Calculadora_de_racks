# I-64 F4-T1B (intento 1): revisión adversarial independiente del código

**Veredicto:** ACEPTABLE CON MENORES, con la condición de resolver antes del cierre las tres IMPORTANTES (§3 y §4).

**Recuento:**

| Gravedad | Hallazgos |
|---|---|
| Bloqueantes | 0 |
| Importantes | 3 |
| Menores | 10 |

**Lo que se revisa:**
- la base `0a6f84c298d0f542bbfdedabd82946490196e23f` (ChainBaseSha = BaseSha);
- el RED `92f5f5dbed597bc0e5a9ab57e83ff8a59459933e`;
- el GREEN `1d36d0065ed3a469004f571ad8e3bc0fdc0a0d02`;
- la rama `architecture/workspace-persistente-rackcad`.

**Abreviatura de rutas:** `<run>/` = `artifacts/orchestration/I-64/F4-T1B-PILOT-CHARACTERIZATION/1/R20261114T214644Z-c077/`.

---

## 1. Objeto y método

### 1.1 Lo que se leyó, entero o en las partes pertinentes

- **Contrato:** `docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json`, con INV-F4T1B-01..16 y las 27 paradas.
- **Paquete v5:** `docs/initiatives/I-64-f4-t1b-gate-contract-package.md`, entero.
- **Autorización del Owner:** `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t1b-authorization.md`.
- **Entrega del Worker:** `<run>/worker-handoff.json`, con Evidence[0..11], UnexpectedFindings, KnownLimitations y Deviations. También el prompt de la delegación, `<run>/prompt.md`, en sus pasos 0 a 5 y en sus reglas de pruebas y de limpieza.
- **Diffs:**
  - `git diff 0a6f84c2 92f5f5db -- src`, entero (14 archivos);
  - `git diff 92f5f5db 1d36d006`;
  - `git diff 0a6f84c2 1d36d006`.
- **Las siete clases nuevas**, enteras, en `1d36d006`; son idénticas en `92f5f5db`.
- **El producto en la base**, en las regiones que tocan los mutantes y los CT:
  - `RackPushBackSystemWindow.xaml(.cs)`;
  - `RackViewBatchDialog.xaml(.cs)` y `RackViewBatchDialogPresenter.cs`;
  - `UiSupport.cs` y `Shell/RackEditorVisualShell.cs`;
  - `RackPushBackCommands.cs` y `PushBackBedSpan.cs`;
  - las tres fachadas.
- **Los ayudantes existentes:** `StaTestRunner.cs`, `EditorDiscardPromptCollection.cs` y `EditorWindowTestSupport.cs`.
- **Artefactos de `<run>/`:**
  - los 45 `tests/m*/mutant.patch` con sus `ui.trx` y `core.trx`;
  - `tests/{b1..b4, cr0, r1, rf, gf, g1..g4}`;
  - `red-ci-ui/ui.trx`, `red-ci-core/core.trx`, `ci-ui/ui.trx` y `ci-core/core.trx`;
  - los registros de build `tests/{b0,rb,gb}`;
  - `tests/batch2.log`, `tests/batch3.log`, `tests/mutant-locs.txt` y `tests/ms1..ms3/out.txt` (las salidas de la medición).

### 1.2 Cómo se comprobó

- Solo con `git show`, `git diff`, `git log`, `git cat-file` y `git rev-parse`, con `-C` sobre el worktree.
- Con lectura de archivos.
- Con análisis en memoria de los TRX y de los parches: Python por la entrada estándar, sin crear archivos. En particular:
  - el criterio mecánico de INV-F4T1B-02 con la expresión del paquete;
  - la comparación línea a línea de la unión de los parches con el diff del RED;
  - el SHA-256 de cada blob.

### 1.3 Incidencias de método propias, declaradas por transparencia

- **Un `git status --short` al inicio**, sobre el worktree. No está en la lista permitida:
  - no altera contenido, aunque puede refrescar la caché de estado del índice;
  - devolvió un árbol limpio.
- **Un archivo temporal y un directorio vacío** creados por error fuera del worktree y del repositorio. Se borraron en el acto, sin relación con la entrega.
- **Nada más se escribió**, salvo este informe.

### 1.4 Lo que no se pudo verificar

- **El estado de los jobs de GitHub Actions** de los dos SHA:
  - `build-ui` en success y `build-plugin` omitido en el RED;
  - 4/4 en el GREEN;
  - la ausencia de `blame-hang` en los registros.

  Solo hay TRX descargados, y no contienen el SHA: que correspondan a `92f5f5db` y `1d36d006` es un dato de custodia de la sesión.
- **La compilación de los mutantes del Plugin** solo se evidencia con `<run>/tests/rb/build-plugin.log`, porque `build-plugin` queda omitido en el RED.
- **Las mediciones del paso 1**, salvo por sus salidas `ms1..ms3`.
- **El comportamiento real de WPF** más allá del código y de los TRX: no se ejecutó nada, por orden.

---

## 2. Análisis por punto

### Punto 1. Tabla cerrada (INV-F4T1B-14, F4T1B-C04 y C10)

`git diff 0a6f84c2 92f5f5db -- src` toca **exactamente** los 14 archivos de MutantScope y ningún otro. Cada cambio se contrastó con su fila, con la clase que su archivo permite y con las reglas (a) y (b).

| Mutante | Archivo (base) | Cambio | Clase y archivo | (a)/(b) |
|---|---|---|---|---|
| MU-01 | `.xaml:749` | `IsDefault="True"` en RestoreButton | comportamiento, permitido | Restore_Click no abre diálogo ni escribe |
| MU-02 | `.xaml.cs:3419` | «¿Deseas seguir?» | comportamiento | no toca `EditorDiscardPrompt`, solo el texto que recibe |
| MU-03 | `.xaml.cs:3381` | `Close_Click { }` | comportamiento | ver la nota (i) |
| MU-05 | `.xaml.cs:1204` | sin `Trim()` | comportamiento | sí |
| MU-06 | `.xaml.cs:3377-3378` | sin el `Close()` final | comportamiento | ninguna prueba existente usa `ShowDialog()` (lo comprueba `git grep`) |
| MU-07 | `.xaml.cs:3214/3227/3230/3251` | rotación 0→1→2→3→0 | comportamiento | sí |
| MU-08 T / O | `.xaml.cs:3327, 3333, 3354` / inicio de `RequestDraw` | « (x).» en las guardas 1 a 3 / confirmación al inicio | comportamiento | O añade un bloque: la confirmación original (3346-3349) queda en no-op. Equivale a «confirmar antes» |
| MU-09 T / E | `.xaml.cs:3532, 3540` / tras 3528 | tooltips « (x)» / `InsertBatchButton.IsEnabled = canOutput` | comportamiento | E **refuerza**: deshabilita la acción |
| MU-10 X / C | `.xaml:9` / `.xaml.cs:315` | foco a `FrontCountBox` / `rackName + "*"` | comportamiento | C altera el argumento de `LoadFromModel`, que es quien asigna `NameBox.Text` (`.xaml.cs:382`). Instancia la fila |
| MU-11 | `.xaml:102-107` | parejas etiqueta+campo de ToleranceBox y BeamDepthBox intercambiadas, conservando `x:Name` | comportamiento | sí |
| MU-12 | `.xaml:6` | `Title` + « (x)» | comportamiento | sí |
| MU-13 | `.xaml.cs` tras 2721 y 3017 | `WindowStartupLocation = Manual` antes de la costura | comportamiento | no toca `Owner` ni `ShowDialog` |
| MU-14 L / D / C | `Views/RackViewBatchDialog.xaml:5, 21/36, 22/35` | `CenterScreen` / IsDefault a «Agregar» / IsCancel a «Quitar» | comportamiento, permitido | `Add_Click` y `Remove_Click` solo mueven listas del presentador (`RackViewBatchDialog.xaml.cs:17-18`) |
| MU-15 V / D | `.xaml:20` / tras 37 | `0,0,7,4` / diccionario vacío fusionado en el shell | comportamiento | no retira recursos. El respaldo del shell se fusiona en `OnApplyTemplate` (`Shell/RackEditorVisualShell.cs:45-52`), no en el constructor |
| MU-16 | `.xaml.cs:2426, 2446` | `(Window)this` y `(window.Configuration) ?? copy` | textual | idéntico |
| MU-17 | `.xaml.cs:2618-2621` | `RequestRecompute()` al cancelar Seguridad | comportamiento | sí |
| MU-19 F / D / S | tras `.xaml.cs:3292` | tres métodos privados sin uso | textual | métodos, no campos ni funciones locales. La lambda de S no es CS8321. Sin avisos CS (`rb/build-ui*.log`) |
| MU-20 W / P / U | `.xaml.cs:2865, 3047, 3311, 3438` / presentador `:59` / `UiSupport.cs:81` | conversiones redundantes | textual | `TryShow(Window owner…)` y `PromptSaveToLibrary(System.Windows.Window owner…)` ya están tipados como `Window`: el cast es redundante |
| MU-21 | `Editor/EditorModules.cs:114, 121` | `(context.Owner)` | textual | idéntico |
| MU-22 W / C | `.xaml.cs:3458` / configurador `:604` | `(Window)this` / `this.SaveCurrentLayout()` | textual | idéntico |
| MU-23 | `.xaml.cs:3296` | sin la confirmación del módulo | comportamiento | no es una guarda. Ver la nota (ii) |
| MU-24 S / D | `RackPushBackCommands.cs:48` / `:57` | `(System.Windows.Window)window` / `RackPushBackCommands.DrawPushBackView(` | textual | misma sobrecarga y método estático; sin `this.` |
| MU-25 | `RackPushBackCommands.cs:279` | `regen: (false)` | textual | idéntico |
| MU-26 M / P / L | `RackMenuCommands.cs:136` / `RackViewBatchProducts.cs:94` / `RackViewPlacement.cs:39` | `global::…` / `regen: (false)` / `RackViewPlacement.PlaceHeader(` | textual | la cualificación no cambia la resolución de sobrecargas |
| MU-27 S / F / P / D | fachadas `:46`, `:44`, `:40` / tras `RackPushBackCommands.cs:462` | `bool regen = (true)` / método privado estático sin uso que llama a `PrepareRedraw` | textual | la constante sigue siendo constante. D compila sin avisos (`rb/build-plugin.log`: solo MSB3277) |
| MU-28 | `.xaml.cs:3395` | `RequestRecompute()` al inicio de `OnClosing` | comportamiento | sí |

**Notas:**
- **(i) MU-03 y el modal de CT-04.** Con MU-03 aplicado, CT-04 sigue cerrándose:
  - el botón `IsCancel` ejecuta además `DialogCancelCommand`;
  - el respaldo a `ApplicationIdle` llama a `Close()` directamente, no a `Close_Click`.

  La CI del RED terminó con los 2310 resultados de UI y los 18471 de Core, así que no hubo cuelgue.
- **(ii) MU-23 y la guarda.** Los fixtures de CT-23 (`IntentTests.cs:77-82`, `:106-107`) hacen la invalidez y el bloqueo independientes de la confirmación, y la ventana va **sin mostrar**. Así, una fuga hacia `TryShow` lanzaría en `Owner = owner`, lo que confirma la sonda de `ms3/out.txt`.

**Avisos del compilador.** Los registros Release de RED y GREEN tienen 0 errores, ningún CS (tampoco CS0162, 0168, 0169, 0219, 0414 ni 8321), ningún NU ni NETSDK, y solo MSB3277 en el Plugin, como en la línea base `b0`. `rb/build-ui.log` traía tres avisos xUnit (2× xUnit2013 y 1× xUnit2029), que desaparecen en `rb/build-ui2.log`. Es coherente con Deviations[2].

**Deviations[1]: varias variantes por fila o varios sitios.** Es **legítimo**:
- el paquete admite «las alternativas que la propia fila enumera» (D-T1B-03);
- el prompt pide «uno o más por fila aplicable» (paso 4);
- varios sitios dentro de una variante (MU-08 T sobre tres guardas, MU-09 T sobre dos tooltips, MU-20 W sobre cuatro sitios, MU-13 y MU-21 sobre dos) instancian la misma clase de cambio que la fila describe, sin introducir otro comportamiento;
- la conjunción se comprobó: los 65 resultados fallan en `red-ci-*`, sin anulación.

El único coste es de granularidad. Por ejemplo, `m08t` no prueba que cada texto de guarda se discrimine por separado, pero el criterio del contrato es por método y se cumple.

**Conclusión:** ningún mutante queda fuera de tabla, de MutantScope o de su clase, y ninguno viola (a) ni (b). C04, en su primer caso, no se activa.

### Punto 2. Prueba por mutante (INV-F4T1B-15)

- **Cobertura del RED.** Los 45 parches cubren 42 variantes distintas y tres repeticiones. `m01b`, `m15vb` y `m15db` son byte a byte iguales a `m01`, `m15v` y `m15d`. La unión de los 42 coincide **exactamente** con `git diff 0a6f84c2 92f5f5db -- src`:
  - los mismos 14 archivos;
  - los mismos multiconjuntos de líneas «−» y «+», también las vacías.

  No hay ningún cambio del RED sin corrida aislada, ni ningún parche ausente del RED.
- **Parches sobre la base.** Cada parche parte del blob de la BaseSha, según su línea `index`; por ejemplo, `205d3889` para el `.xaml.cs` y `75db0c6a` para `RackPushBackCommands.cs`. Cuando un archivo solo tiene un mutante, el blob resultante es el del RED; ocurre en ocho archivos.
- **Resultados de cada TRX aislado.** Seleccionan 55 resultados de UI y 10 de Core. Fallan al menos los métodos que INV-14 asigna a cada fila:
  - MU-02: 8, entre CT-02, CT-03 y CT-04;
  - MU-03: 12;
  - MU-06: 12;
  - MU-07: las 4 filas más el lado B;
  - MU-27 S/F/P: la fila de su fachada.

  Las líneas de las aserciones que fallan coinciden con las de los archivos finales:
  - `Close:212`, `:486` y `:600`;
  - `Intent:128`, `:131` y `:204`;
  - `Owner:58` y `:100`;
  - `Resource:33` y `:66`;
  - `SourceGuard:206`, `:209`, `:263` y `:371`;
  - `PluginGuard:174`, `:180` y `:264`.

  Es evidencia de que las corridas usaron la misma disposición de las pruebas.
- **Filas no aplicadas.** MU-04 y MU-18 no se aplican, y es coherente: CT-02 usa el gesto, y CT-18 no se escribe. Todas las demás filas tienen mutante.
- **Cronología.** En UTC, a partir de las horas de los TRX:

  | Momento | Hora |
  |---|---|
  | `m01..m28` | 22:11–22:36 |
  | commit RED | 22:47:37 |
  | commit GREEN | 22:48:49 |
  | `m01b`, `m15vb` y `m15db`, sobre el GREEN | 22:57–22:59 |

  - Los mensajes de `m01` y `m15d` son los anteriores al retoque: «Assert.Empty() Failure» en `Keyboard:69` y «Assert.Equal() Failure» en `Resource:66`.
  - Los de `m01b` y `m15db` son los finales: «Assert.DoesNotContain()» y «Assert.Empty()».
  - Confirma Deviations[2], pero el retoque afectó a **tres** métodos en dos archivos, no a «dos pruebas» (M-10).
- **Lotes.** `batch2.log` y `batch3.log` registran `restore-left=0` en cada uno de sus 40 mutantes. Las corridas son secuenciales, de 30 a 40 s cada una, sin solaparse.

### Punto 3. RED conjunto y criterio mecánico (INV-F4T1B-02)

**Criterio mecánico:**

| Filtro | Seleccionados | Failed | Infracciones | Mínimo |
|---|---|---|---|---|
| `red-ci-ui/ui.trx` | 55 | 55 | 0 | 22 |
| `red-ci-core/core.trx` | 10 | 10 | 0 | 4 |

Las infracciones son 0 en los tres criterios:
- ningún Message con el prefijo `^(?:[A-Za-z_][\w`]*[.+])+[A-Za-z_][\w`]* : `;
- la primera línea de StackTrace siempre bajo `tests/`;
- ningún NotExecuted ni Skipped.

**Unión de pruebas existentes rotas.** Son exactamente tres, y cada una está bien atribuida según su mensaje:

| Prueba | Mensaje | Mutante |
|---|---|---|
| `RackCad.Tests.PushBackRoundTripSourceGuardTests.EditPushBack_…_EveryRedrawRegenFalse` | «Expected: 3, Actual: 2», la cuenta de `regen: false` | MU-25 |
| `RackCad.UI.Tests.RichEditorCharacterizationTests.TheDefaultActionOfEachRichEditorIsWhatItIsToday` | «Actual: System.Windows.Controls.Button: Restaurar» | MU-01 |
| `RackCad.UI.Tests.PushBackTopeSectionInSafetyTests.MovingControl_DoesNotChangeTopeGeometryOrBom` | un tope extra; la prueba fija `SafetyDialog = _ => null` y pulsa Seguridad | MU-17 |

- **Suites completas.** La UI tiene 2310 resultados (2236 en verde, 57 en rojo y 17 NotExecuted previos) y Core 18471 (18460 en verde y 11 en rojo). Coincide con `rf/` en local.
- **Observación para la reconciliación previa a T4B** (§8 del paquete). En la base, la `LastComputation` del fixture compuesto difiere de la que produce un recálculo. No es un defecto de la entrega, pero conviene mirarlo antes de que T4B dependa de esa ventana.

### Punto 4. GREEN = inverso exacto (INV-F4T1B-01)

- `git diff 0a6f84c2 1d36d006 -- src` está vacío: 0 bytes.
- `git diff --name-only 0a6f84c2 1d36d006` lista exactamente los siete archivos nuevos de prueba, todos dentro de las 21 rutas de AllowedWriteScope.
- `git diff --name-only 92f5f5db 1d36d006` lista solo los 14 de MutantScope. **Ninguno de `tests/`**.
- **Los blobs del GREEN son los de la base** en los 14 archivos. Por ejemplo, `RackPushBackSystemWindow.xaml.cs`: `205d3889…` en la base, `9b596914…` en el RED y `205d3889…` en el GREEN.
- **Los SHA-256**, de `git cat-file blob | sha256sum`, coinciden en los 14 archivos con los 16 primeros caracteres hexadecimales de Evidence[4].
- **Los commits:**
  - los padres son los esperados: GREEN → RED → base;
  - el título del RED contiene la frase exacta;
  - los dos commits llevan el trailer `Co-Authored-By: Claude Sonnet 5.5`.

### Punto 5. Calidad de la caracterización (D-T1B-04)

**El mapa CT → método.** Evidence[0] y los TRX coinciden en los 56 métodos distintos de los 65 resultados: no sobra ni falta ninguno. Cada CT obligatorio, y CT-04 promovido, tiene un método propio en la clase que manda D-T1B-05, con su colección.

| CT | Juicio |
|---|---|
| CT-01 | La ruta `ProcessKey` y el control positivo con Escape son correctos, y la sonda va en el mismo método. Pero la aserción de declaración (`Keyboard:69`) **precede** al gesto, así que MU-01 nunca ejerce las aserciones de «Enter no hace nada» (`:74-79`): quedan enmascaradas y no están declaradas como anclas (I-01) |
| CT-02 y CT-03 | Los cuatro casos son correctos: cuenta, texto, dueño nulo, «No» y «Sí». La precondición de IsEnabled y del peer se asevera (`Close:117-119`). Falta «INCUMPLIMIENTO CARACTERIZADO» en el dueño nulo, que es OD-4 de D9 (M-06) |
| CT-04 | Cumple el respaldo especificado: capturas locales, bandera, `ApplicationIdle`, `try`/`finally` que no lanza y `Abort()` de las operaciones pendientes, sin aserciones en los delegados (`Close:399-471`). Si `ShowDialog` lanzara, el `Abort` no corre, pero el respaldo encolado acaba cerrando la ventana: es aceptable |
| CT-05 | Correcto en las dos variantes, sin mostrar y mostrada. Solo `RackName` tiene mutante: las otras 10 aserciones del contrato inicial no están declaradas (I-01) |
| CT-06 y CT-07 | Correctos; el GUID sale de `newIdFactory` (`Intent:121`, `:140-141`) |
| CT-08 | Correcto. Las guardas 1 y 2 se ejercen elevando `ClickEvent` sobre botones **deshabilitados** en ese estado (`.xaml.cs:3526-3527`); esa precondición no se declara (M-08) |
| CT-09 | Las cuatro combinaciones con `IsEnabled`/`ToolTip` exactos, más el caso inválido, son correctos. Las cinco ventanas no se cierran (M-05) |
| CT-10, CT-11, CT-12 y CT-13 | Correctos. CT-13 `Owner:150` es redundante (M-07) |
| CT-14 | `Assert.Null(dialog.Owner)` en `Owner:179` es tautológico, porque el diálogo se construye directamente, y no está declarado (I-01 y M-07) |
| CT-15 | Correcto, por equivalencia |
| CT-16 y CT-19 a CT-22 | Correctos en el texto; su alcance y fragilidad están en M-03 |
| CT-17 | Correcto, con la costura `SafetyWindowDialog` y la ventana mostrada |
| CT-23 | En ejecución, correcto: la ventana va sin mostrar y la variante bloqueada usa un bloqueo independiente. En fuente, Evidence[2] declara «las cinco vistas fijas», pero solo se fijan tres (`SourceGuard:372-374`) (I-01) |
| CT-24 a CT-27 | Las aserciones de orden son débiles (M-02) y la guarda de CT-27 (2) es demasiado amplia (M-03) |
| CT-28 | Correcto. Evidence[2] declara como ancla «el campo conserva el valor tecleado», y **esa aserción no existe** (`Close:683-700`) (I-01) |

**Textos (INV-F4T1B-09).** Coinciden con el producto:
- la pregunta (`.xaml.cs:3419`);
- las guardas 1 a 3;
- el estado del campo, que es «Corrige los campos numéricos marcados: » más `NumericFieldValidation.NotANumberMessage`;
- los ocho tooltips, el título, «Insertar cola», «Cancelar» y la fuente de AppStyles.

El diagnóstico bloqueante es el de `PushBackBedSpan.DisabledReason` con valores medidos.

**Lo demás:**
- **Ramas (INV-04):** no hay ninguna rama del tipo «si falla el control, el respaldo».
- **Delegados:** ningún `Assert` va dentro de un delegado que ejecuta el producto, ni de uno encolado.
- **Guardas de fuente:**
  - normalizan CRLF a LF;
  - quitan los comentarios de C# y de XAML;
  - colapsan los espacios;
  - llevan la constante `TextGuard`.

### Punto 6. Contención (INV-05, -06, -07, -16, F4T1B-C09 y la orden del Owner)

**Lo que las pruebas versionadas no hacen.** Comprobado con `git grep` y leyendo el código:
- no instancian `RackFrameConfiguratorWindow` y no tocan `ConfigureModuleHeader`, `Bom_Click`, `SaveLibrary_Click`, `PromptSaveToLibrary` ni `RackProjectStore.Save`;
- `InsertBatch_Click` solo se ejerce con entrada inválida o salida bloqueada, y sin mostrar la ventana;
- Seguridad y las botas pasan siempre por su costura;
- no usan un `MessageBox` real: el aviso va sustituido, y las clases que lo sustituyen o muestran ventanas están en `EditorDiscardPromptCollection`.

**Caminos de fallo del RED.** Ninguno deja un modal ni se queda esperando:
- todas las ventanas mostradas se cierran en un `finally`;
- CT-04 con MU-03 y MU-28 sale por el respaldo;
- CT-23 lanzaría antes de cualquier diálogo.

**Las tres clases de escritura (INV-16) se distinguen bien:**
- **A**, solo por fuente;
- **B**, en ejecución sobre el contrato público;
- **C**, solo por fuente en RackCad.Tests.

**UnexpectedFindings[1]: la sonda temporal** (I-03).
- `ms3/out.txt` registra en P06_Batch «B3 ex=InvalidOperationException … Owner …». Es un `InsertBatch_Click` con entradas válidas sobre una ventana sin mostrar.
- Lanzó en `RackViewBatchDialogPresenter.cs:59`, antes de `ShowDialog`, y no escribió nada.
- Viola la letra de INV-06: «sin excepciones … ni InsertBatch_Click con entradas válidas».
- No provoca ninguno de los efectos que el Owner castiga con STOP: no se cuelga, no deja nada vivo, no escribe en el perfil, no necesita AutoCAD y no usa rutas de máquina.
- Además era innecesaria: la guarda de fuente de CT-23 ya fija la ausencia de `canInsertInAutoCad`.

**KnownLimitations[7]: las suites completas reescriben la disposición real** (I-02).
- **Lo que pasó:**
  - El prompt exige las suites completas en local: paso 4 (ii) y paso 5.
  - También exige la limpieza «DESPUÉS … idéntica a la de antes (si cambia algo: parada F4T1B-C09 con los dos valores)».
  - Las suites completas de UI cierran el configurador real, porque al menos diez clases existentes lo instancian.
  - La fecha del archivo de disposición real pasó de 16:42:01Z (paso 0) a 22:46:41Z y a 22:55:38Z (UTC). Coincide con las corridas `rf` (22:41–22:46Z) y `gf` (22:51–22:55Z).
- **Lo que hizo el Worker:**
  - No se detuvo. Volvió a acotar la prueba a sus corridas filtradas, lo que es coherente con el texto de INV-06 del paquete, «antes y después de la corrida local del filtro de UI».
  - Lo registró como limitación, no como desviación, con `TriggeredStopConditions` vacío.
- **El alcance:**
  - El contenido no cambió: el SHA-256 es idéntico.
  - Las pruebas de T1B no escriben; el contrato no se infringe.
  - Lo que sí ocurrió es una escritura en la configuración real del usuario durante la delegación, y la orden del Owner es «nunca la configuración real del usuario».
  - Otras escrituras posibles de la suite existente en el perfil no se midieron.

**Deviations[0]: el subshell en segundo plano** (M-09). No hay rastro de solapamiento ni de procesos vivos.

### Punto 7. Determinismo y entorno

- **GUID:** el de CT-07 sale de `newIdFactory`, y ningún CT asevera un GUID de `Guid.NewGuid`.
- **Cultura (INV-11):** CT-08 y CT-23 asertan un texto formateado con `CultureInfo.CurrentCulture` y `0.##` (`PushBackBedSpan.cs:357-365`) sin fijar la cultura. Hoy es estable porque los valores son enteros (596 y 404), y pasa en local y en la CI (M-04).
- **Orden entre pruebas:**
  - el hilo STA es compartido (`StaTestRunner`), y la colección deshabilita el paralelismo, así que no hay reentradas en los bombeos ni en el `ShowDialog`;
  - no queda estado estático, porque el aviso se restaura con `using`;
  - las ventanas sin cerrar de CT-09 quedan en `Application.Windows`, aunque sin HWND (M-05).
- **Foco y activación:**
  - `ProcessKey` usa como ámbito el `PresentationSource`, y el peer no depende del foco;
  - CT-04 se midió con y sin activación (`ms2/out.txt`: M05–M08, con dos avisos en los cuatro);
  - la prueba versionada usa `ShowActivated = false`.

  CT-04 es independiente de la activación, y es estable: b1–b3 y g1–g3, más la CI del GREEN.
- **Rutas y esperas:** no hay rutas de máquina, porque `RepoRoot` sube hasta `RackCad.sln`, ni esperas fijas o temporizadores.

### Punto 8. Honestidad de la entrega

**Verificado como cierto:**
- RED, GREEN y su alcance;
- los conteos 55/10 y la conjunción 65/65;
- el mapa CT → método;
- la tabla de bytes;
- los builds: 65, 14 y 2 avisos en Core, UI y Plugin, sin CS;
- las tres corridas en verde y la unión de las tres existentes rotas;
- la deuda OD-1 a OD-13;
- la clasificación de las escrituras;
- la falta de código de medición en los commits.

**Inexacto o no verificable:**
- **Evidence[2] presenta como exhaustiva una lista de anclas que no lo es.** Incluye una que no existe (CT-28) y otra que exagera (CT-23) (I-01). Además, sobre CT-09, afirma que ningún mutante cambia el tooltip de Actualizar, y MU-09 T lo cambia sin AutoCAD (`.xaml.cs:3533`) (M-10).
- **Evidence[1] y `mutant-locs.txt`** dan varias líneas desplazadas una o dos posiciones (M-01).
- **Deviations[2]** dice «dos pruebas», y fueron tres métodos (M-10).
- **El resultado de la limpieza** se presenta sin declarar que choca con la regla de parada del prompt (I-02).

---

## 3. Tabla de hallazgos

| Id | Gravedad | Contenido | Evidencia | Disposición propuesta |
|---|---|---|---|---|
| I-01 | IMPORTANTE | **La declaración de anclas es incompleta e inexacta.** El paquete dice que «la entrega declara todas las aserciones sin mutante propio», y F4T1B-C04, en su tercer caso, es una parada. Hay aserciones sin mutante propio que Evidence[2] no declara (ver el desglose bajo la tabla), y dos declaraciones son inexactas. Todas están en métodos que algún mutante detecta: no hay riesgo de código | `1d36d006`; ver el desglose | Leído al pie de la letra, activa C04. Es una **corrección documental**: completar la lista de anclas, sin tocar `tests/` ni `src/` y sin RED. La decide el Controller o el Coordinator antes del cierre |
| I-02 | IMPORTANTE | **Las suites completas en local reescribieron dos veces la fecha del archivo de disposición real del usuario**, con el mismo contenido. La prueba de limpieza se volvió a acotar en lugar de parar como pide el prompt, y se registró como limitación, no como desviación. La raíz está en que el prompt exige suites completas locales, que según H-18 siempre escriben ese archivo | Evidence[6], KnownLimitations[7], `<run>/prompt.md` pasos 4 (ii) y 5, y las horas de `rf/` y `gf/` | El Coordinator o el Owner ratifican que T1B no escribe (contrato) y lo registran como **desviación de la delegación**. En las correcciones y en T4B, no correr la suite completa de UI en local (solo en la CI) o declararlo antes. No exige cambio de código |
| I-03 | IMPORTANTE | **Una sonda temporal disparó `InsertBatch_Click` con entradas válidas**, contra la cláusula de INV-06 «sin excepciones». Quedó contenida: con la ventana sin mostrar, lanza en `Owner = owner` antes de `ShowDialog`; no escribe y no se versiona. Era innecesaria | UnexpectedFindings[1]; `ms3/out.txt` (P06_Batch) | El Coordinator o el Owner registran y aceptan la desviación. Para el futuro, prohibir en el prompt las sondas con entradas válidas. No invalida el código |
| M-01 | MENOR | **Referencias `archivo:línea` desplazadas.** Ver el desglose bajo la tabla | Evidence[1]; `mutant-locs.txt`; `git show 0a6f84c2:<archivo>` | Adenda documental |
| M-02 | MENOR | **Las aserciones de orden de las guardas del Plugin son débiles:** pasarían con otro comportamiento. `InOrder` busca la primera aparición posterior y no fija que `PrepareAll` vaya antes del **primer** `RedrawInPlace`. Tampoco fija que `RackViewBatchExecution.Run` va bajo `if (!window.UpdateOnly)`, que es la base `RackPushBackCommands.cs:369-371` (solo `Has("window.UpdateOnly")`). CT-24 no fija que `DrawPushBackView` vaya después de `ShowModalWindow` | `PluginGuardTests.cs:182-191, 249, 254-261, 201, 219-222` | Anotarlo para la reconciliación de T4B y para F5. No se puede tocar `tests/` sin RED, así que solo se refuerza si el Owner lo pide |
| M-03 | MENOR | **Alcance y fragilidad de las guardas de texto (INV-08):** ver el desglose bajo la tabla | `SourceGuardTests.cs:163-166, 230, 290, 297, 305-309, 348`; `PluginGuardTests.cs:209, 280, 371-382` | Incluirlo en la reconciliación previa a T4B (§8 del paquete) y en la sustitución de F5. KnownLimitations[3] ya lo declara en parte |
| M-04 | MENOR | **La cultura no se fija (INV-11)** al aseverar un diagnóstico formateado con `CurrentCulture` y `0.##` | `IntentTests.cs:39-40, 265, 431`; `PushBackBedSpan.cs:357-365` | Declararlo, o fijar la cultura en una corrección futura |
| M-05 | MENOR | **Ventanas sin cerrar.** CT-09 crea cinco ventanas sin `try`/`finally` ni `Close()`, contra la regla del prompt («cada ventana que crea una prueba se cierra en un finally») y la letra de INV-07. No se muestran, así que no tienen HWND, pero quedan en `Application.Windows` del hilo STA compartido | `IntentTests.cs:316-327, 335-347` | Aceptarlo como menor; corregirlo si hay una corrección |
| M-06 | MENOR | **Falta «INCUMPLIMIENTO CARACTERIZADO» en OD-4**, el aviso sin dueño WPF de D9 (INV-03) | `CloseTests.cs:186, 307` | Anotarlo |
| M-07 | MENOR | **Aserciones tautológicas o redundantes** | `OwnerTests.cs:150` (NotSame tras Same) y `:179` (Owner nulo de un diálogo construido a mano); `IntentTests.cs:403` (redundante con `:404`) | Anotarlo; declararlas como anclas (I-01) |
| M-08 | MENOR | **Precondición no declarada (R-I-1).** CT-08 alcanza las guardas 1 y 2 elevando `ClickEvent` sobre InsertButton y UpdateButton, que están deshabilitados en esos estados. Es una ruta que el usuario no puede recorrer | `IntentTests.cs:198, 217`; `.xaml.cs:3526-3527` | Declararlo como límite en la entrega |
| M-09 | MENOR | **El Worker lanzó un subshell en segundo plano**, contra el prompt (Deviations[0]). No hay solapamiento: las corridas son secuenciales y `restore-left=0` | `tests/batch3.log`; horas de los TRX | Registrarlo |
| M-10 | MENOR | **Redacción inexacta:** ver el desglose bajo la tabla | Deviations[2]; Evidence[2] | Adenda documental |

**Desglose de I-01.** Aserciones sin mutante propio que Evidence[2] no declara:
- **CT-05:** las 10 aserciones del contrato inicial (`CloseTests.cs:516-529`); MU-05 solo mata `RackName`, en `:547` y `:571`.
- **CT-07:** `InsertRequested`, `UpdateOnly` y el GUID (`IntentTests.cs:129-130, 140-141`).
- **CT-04:** el estado final (`CloseTests.cs:489-492, 507-510`).
- **CT-02 y CT-03:** «Sí» no confirma la edición (`CloseTests.cs:250-251, 370-371`).
- **CT-14:** `Owner` nulo (`OwnerTests.cs:179`).
- **CT-17:** las selecciones y las llamadas (`IntentTests.cs:368-369`).
- **CT-21:** dos `ShowDialog()` y la petición condicional (`SourceGuardTests.cs:315-318`).
- **CT-22 (a):** el orden `EnsureId` < `SetName` < proyecto < `Save` (`:337-340`).
- **CT-26:** los pasos que siguen al tercero (`PluginGuardTests.cs:284-290`).
- **CT-27:** las firmas de `DrawAndPlace`, `PrepareRedraw` y `RedrawInTransaction`, y los `Lacks` de los otros archivos (`:322-347, 380-381`).
- **CT-01:** lo que sigue a Enter, enmascarado por `:69` (`KeyboardTests.cs:74-79`).

Las dos declaraciones inexactas:
- CT-28 declara como ancla «el campo conserva el valor tecleado», y esa aserción no existe (`CloseTests.cs:683-700`);
- CT-23 declara «cinco vistas fijas», y solo se fijan tres (`SourceGuardTests.cs:372-374`).

**Desglose de M-01.** Referencias de Evidence[1] y `mutant-locs.txt` frente a la línea real en la base:

| Mutante | Dice | Real |
|---|---|---|
| MU-05 | `:1203` | `:1204` |
| MU-06 | `:3376` | `:3377-3378` |
| MU-10 C | `:314` | `:315` |
| MU-20 W | `:2863` y `:3045` | `:2865` y `:3047` |
| MU-24 S | `:47` | `:48` |
| MU-24 D | `:56` | `:57` |
| MU-22 C | `:602` | `:604` |

**Desglose de M-03.** Alcance y fragilidad de las guardas de texto:
- varias guardas leen archivos fijos con `Read(…)` en lugar de buscar por las raíces;
- CT-21 depende del orden de las clases en `EditorModules.cs`;
- CT-26 corta en el primer `break;`;
- CT-24 usa un `Lacks("Views")` muy amplio;
- CT-27 (2) prohíbe `PrepareRedraw(` y `RedrawInTransaction(` en archivos compartidos completos (`RackMenuCommands.cs` y `RackViewBatchProducts.cs`). Daría un falso positivo si T4B, u otro sistema, añadiera allí un llamador que no fuera de Push Back;
- no se quitan las cadenas, solo los comentarios.

**Desglose de M-10.** Redacción inexacta:
- Deviations[2] dice «dos pruebas», y fueron tres métodos: `K.EnterOnTheAccessKey…`, `R.TheWindowMerges…` y `R.TheShellDoesNot…`;
- Evidence[2], sobre CT-09, afirma que ningún mutante cambia el tooltip de Actualizar, y MU-09 T lo cambia sin AutoCAD (`.xaml.cs:3533`).

## 4. Veredicto

**ACEPTABLE CON MENORES.** Recuento: **0 bloqueantes, 3 importantes y 10 menores.**

**Lo que sostiene el veredicto.** El código de la entrega cumple el contrato en lo sustantivo:
- **la tabla cerrada:** ningún mutante queda fuera de ella ni viola (a) o (b);
- **la prueba por mutante:** cada cambio del RED tiene su corrida aislada sobre la base, y cada fila aplicable discrimina al menos lo que INV-14 le asigna;
- **el RED conjunto:** 55/55 y 10/10, con el criterio mecánico cumplido y la unión de tres existentes bien atribuida;
- **el GREEN:** es el inverso byte a byte;
- **la contención** de las pruebas versionadas y el determinismo.

**La condición antes del cierre.** Hay que resolver las tres IMPORTANTES, y ninguna exige rehacer el RED, el GREEN ni consumir otro intento:
- I-01, con una corrección documental de las anclas, sin tocar `tests/` ni `src/`. Si el Controller aplica C04 al pie de la letra sin esa corrección, la entrega se para por C04;
- I-02 e I-03, con una disposición expresa del Coordinator o del Owner sobre las dos desviaciones de contención, ya declaradas por el Worker y sin efecto persistente sobre el contenido.

Las MENORES se anotan para la reconciliación previa a T4B (§8 del paquete) y para F5.
