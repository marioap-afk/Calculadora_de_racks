# Comprobación focalizada independiente — Paquete del contrato de F4-T1B (v3)

**Objeto:** `I-64-f4-t1b-gate-contract-package.md` v3 (citado como «paquete :línea»), contrastado con la comprobación de la v2 (`independent-rereview.md`, «RR»), con la revisión de la v1 y con el código en `c3ae1c970c908ef541840fc583680b86376cae0b`.

**Método:**
- Solo lectura: no se compiló, no se ejecutó ninguna prueba y no se escribió nada en el repositorio.
- Las rutas son relativas al repositorio. Lo que es comportamiento de WPF, de xUnit o del compilador y no está medido en el repositorio se marca **(WPF)**, **(xUnit)** o **(C#)**.
- La palabra «Aceptado» del §10.2 no se ha tomado como prueba: cada corrección se ha comprobado contra el texto del paquete y contra el código.

## Veredicto

**EMITIBLE TRAS CORRECCIONES — 0 bloqueantes / 2 importantes / 11 menores.**

**Lo que la v3 resuelve de verdad:**
- R-I-1, R-I-2, R-I-3, R-I-5, R-I-6 y R-I-7, cada uno con algún residuo menor.
- El mecanismo de R-B-1:
  - la granularidad por resultado;
  - las anclas declaradas en el mismo método que un subpunto detectado;
  - la regla de las `Theory`;
  - los mutantes de cualquier archivo de `MutantScope`;
  - el código muerto en (a).

  Ya no queda ningún resultado obligatorio que no se pueda hacer fallar o anclar. El defecto bloqueante de la v2 desaparece.

**Lo que impide emitirla tal como está:**
- **V-I-1.** La lista de anclas está incompleta. Faltan subpuntos que ninguna clase permitida puede matar: el `Owner` de S-2, S-3 y S-4, el dueño nulo del aviso, «NameBox es un TextBox» y la ventana sonda de Enter. Se lee como cerrada, y C04 es STOP. Es el mismo vector de STOP por lectura literal que en F4-T4A2.
- **V-I-2.** CT-01 no tiene respaldo si el control de Escape falla sobre `BaseSha`. La v3 hace depender el control de Enter del de Escape, pero solo le da respaldo a CT-02. R-I-4 queda a medias.

Las dos se corrigen en el texto del paquete, sin rediseñar la tarea.

## Estado de R-B-1 y R-I-1 a R-I-7

| Hallazgo | Estado | Comprobación |
|---|---|---|
| R-B-1 | **Parcial** (el mecanismo, resuelto) | La granularidad (paquete :254), las anclas (:331-335), la regla de las `Theory` (:297), el «cualquier archivo» (:296), la regla (a) ampliada (:277-281) y C04 (:576) están. <br>El código muerto de CT-27 (2) compila sin avisos (respuesta 1). <br>**Falta:** la lista de anclas no recoge todos los subpuntos sin mutante posible (V-I-1). Además, la tabla prevista se razona mutante a mutante y dos de sus filas se anulan en la conjunción (V-M-1) |
| R-I-1 | **Resuelto** | El criterio coincide con lo que escribe xunit.runner.visualstudio 2.8.2. Funciona igual en `StaTestRunner.Run`, en los ayudantes privados y en `EditorWindowTestSupport` (respuesta 2). Residuo: cómo se mide «bajo `tests/`» y de qué TRX se cuenta (V-M-7) |
| R-I-2 | **Resuelto** | (b) prohíbe desviar gestos, debilitar guardas, actuar en el constructor y retirar recursos (:282-291). CT-01 queda en «Cerrar» o «Restaurar». CT-23 se ejerce sin mostrar la ventana y con un bloqueo independiente: `StageBlockingEdit` es el texto sin comprometer de `FondosBox` (`tests/RackCad.UI.Tests/PushBackOutputGateAfterRecomputeTests.cs:96-97`), no la edición de módulo. Ninguna prueba existente pulsa `InsertBatchButton` ni llama a `ShowDialog`. Residuo: «el constructor» es ambiguo (V-M-9) |
| R-I-3 | **Resuelto** | (b) en términos observables (:291), (a) ampliada y la frase para el Controller (:293, :508-510). Ninguna fila genérica admite una instancia peligrosa (respuesta 3). Residuo: «conformes a (c) por construcción» no tiene sentido, y C04 mezcla resultado y aserción (V-M-8) |
| R-I-4 | **Parcial** | Ya están: la medición sobre `BaseSha` con aserción incondicional (:354), el control de Enter por la ruta de «\x1B» (:356) y el peer con el respaldo de CT-04 y CT-28. El control de Enter **no** es un ancla: lo mata «Close_Click sin efecto» o retirar `IsCancel`. **Falta** el respaldo de CT-01 (V-I-2). La sonda opcional no la mata ningún mutante (V-I-1) |
| R-I-5 | **Resuelto** | Captura local, bandera leída por la sustitución, `Abort()` de las operaciones `Pending`, aserciones después y el mutante de texto (:368). Todo es realizable con API pública. Residuo: un `try/finally` sin `catch` sí lanza (V-M-6) |
| R-I-6 | **Resuelto** | Captura en locales y `try { gesto } finally { window.Close(); }` dentro del presentador, como `QuickConfigAt` (`tests/RackCad.UI.Tests/PushBackHeaderAuthorityWindowTests.cs:68-74`). <br>La sustitución distingue la pregunta del configurador (`src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs:193-195`). <br>«Nunca se muestra» es condición de INV-06 (:526-527). <br>Aparte, y no de R-I-6: el mutante previsto de CT-16 es débil (V-M-4) |
| R-I-7 | **Resuelto** | `ChainBaseSha` es un campo obligatorio de la delegación (`docs/automation/agent-execution/schemas/delegation.schema.json:122`, `:149`), así que el Controller lo conoce. <br>Es coherente con §16.8 (`docs/AUTOMATION_PLAN.md:594-599`) y con Scope (`:617`): el paquete mantiene `BaseSha..CurrentSha` ⊆ 21 rutas. <br>INV-01, el GREEN y `ExpectedEvidence` dicen lo mismo (:490-494, :504, :557). Residuo en las correcciones con Core en false (V-M-11) |

## Respuestas a las comprobaciones pedidas

### 1. R-B-1: realizabilidad por resultado

**Recorrido por CT.** Esta tabla recoge los subpuntos que, en un método propio, no mata ninguna fila de la tabla prevista; el resto de los subpuntos sí lo mata alguna fila.

| CT | Subpunto aislado | ¿Hay un mutante permitido? | Qué hace falta |
|---|---|---|---|
| CT-01 | El gesto de Enter (no cierra, no pide y no recalcula), con `IsDefault` en `CloseButton` | Sí, con `IsDefault` en `RestoreButton`: `Restore_Click` → `LoadFromModel` → `Recompute()` (`src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3288`, `:426`) | V-M-1 |
| CT-02, CT-03 | Caso «No» (sigue abierta, con pendiente y sin petición) | Solo aseverando la cuenta y el texto de la pregunta, o sin «Close_Click sin efecto». Por ejemplo, un `OnClosing` que ignore `e.Cancel` | V-M-1 |
| CT-02, CT-03 | El dueño `null` del aviso | **No.** Lo fija `EditorClosePolicy.MayClose` (`src/RackCad.UI/Shell/EditorClosePolicy.cs:112`, `:54`), fuera de `MutantScope`, y (b) prohíbe tocar `EditorDiscardPrompt` | Ancla (V-I-1) |
| CT-05 | Que no pregunte, los valores iniciales y `RackId` nulo | Sí: `PendingWork()` siempre pendiente, o los getters del contrato público (`.xaml.cs:247-256`), que no son el constructor | — |
| CT-07 | El GUID de `newIdFactory` y el lado B | Sí: el getter de `RackId`, o `FrontalSide_Changed` (`:3244`) | — |
| CT-09 | Las filas {AutoCAD} × {nuevo o RACKEDITAR} | Sí, con tres textos de tooltip distintos. `IsEnabled = canOutput` solo mata «con entradas inválidas» | — |
| CT-10 | «NameBox es un TextBox», aseverado sobre el propio control | **No:** es un `TextBox` con `TextChanged` (`.xaml:47`), y el code-behind usa `.Text` (`.xaml.cs:1204`). Ningún cambio de tipo compila | Ancla, o aseverarlo sobre el destino del binding |
| CT-12 | `Owner` en `null` | No | Ya es ancla |
| CT-13 | El `Owner` de S-3 y de S-4, también «aunque S-3 esté abierto» | **No:** `Owner = this` (`.xaml.cs:3016`, `:2721`), y (b) lo prohíbe | Ancla (V-I-1) |
| CT-15 | La fuente del diccionario y el `Style` resuelto | Sí: un segundo diccionario fusionado, o una redefinición local `BasedOn` de `EditorShellWindowStyle` | — |
| CT-16 | El `Owner` de S-2 | **No** (`.xaml.cs:2426`) | Ancla (V-I-1) |
| CT-16 | «Escenifica igualmente» | Solo con un gesto que sustituya la instancia | V-M-4 |
| CT-17 | Las selecciones no cambian | Sí: vaciar `safetySelections` en la rama de cancelar | — |
| CT-19 | Una fila por token | Sí: un método muerto por token en `.xaml.cs` | — |
| CT-20 | S-8, `Owner = owner` en el presentador | Sí: `Owner = (Window)owner`, porque el archivo importa `System.Windows` (`src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs:5`, `:59`). No está en la fila | V-M-10 |
| CT-23 | «No aparece el texto de AutoCAD» y «sin `Closed` ni petición» | Sí: añadir una guarda de `canInsertInAutoCad`, o un `Close()` en la rama de error | — |
| CT-24 | «No `Views`», si la guarda acota el cuerpo de `RackPushBack()` | Sí: una rama no alcanzable sin CS0162 dentro del cuerpo | — |
| CT-27 (2) | Una fila por archivo en una `Theory` | No para `PushBackKindHandler.cs` | `Fact` (ya dicho en :297) |

**¿Compila sin avisos el código muerto de CT-27 (2)?** Sí, si es un **método** privado:
- Las tres fachadas son `public sealed` con constructor implícito (`src/RackCad.Plugin/Systems/PushBack/PushBackSystemDrawService.cs:18-21`), y sus caller-owned son métodos de instancia `internal` sin sobrecargas:
  - `PrepareRedraw(Database, ObjectId, PushBackSystem, string, int = -1)` (`:58`);
  - `RedrawInTransaction(Database, Transaction, PreparedViewRedraw, out IReadOnlyCollection<ObjectId>)` (`:64`);
  - y las de las otras dos fachadas: `PushBackFrontalDrawService.cs:56`, `:62`; `PushBackPlantaDrawService.cs:51`, `:56`.
- Un ejemplo que compila: `private static void Dead() { _ = new PushBackPlantaDrawService().PrepareRedraw(null, ObjectId.Null, null, null); }`, con `out _` para `RedrawInTransaction`. `RackPushBackCommands.cs:5`, `:11` y `:14` importan los tipos.
- **(C#)** Un método privado sin uso no da ningún aviso CS. IDE0051 no se aplica: no hay `EnforceCodeStyleInBuild`, `TreatWarningsAsErrors` ni `NoWarn` en `Directory.Build.props`, `Directory.Build.targets` ni en ningún `.csproj`, y `.editorconfig:11-15` lo explica.
- `-NoWarnings` solo rechaza `warning (MSB|CS|NU|NETSDK)\d+` (`eng/ci/verify-autocad-references.ps1:110-113`). Los avisos CA, IDE y `xUnit*` de xunit.analyzers no lo rompen.
- **Pero sí lo rompen** **(C#)**:
  - un **campo** privado sin uso (CS0169) o asignado y nunca leído (CS0414);
  - una función local sin uso (CS8321);
  - una variable sin uso (CS0168, o CS0219 con una constante);
  - un `if (false)` (CS0162).

  La regla (a) dice «miembro privado» y solo nombra CS0162 y CS0219 (V-M-3).
- El Plugin solo se compila en el build Release local: la CI del RED no lo compila, porque `build-plugin` queda omitido y Core no referencia el Plugin. Por eso `ExpectedEvidence` (:560) es la única red para V-M-2 y V-M-3.

### 2. R-I-1: el criterio del TRX

**El prefijo:**
- **(xUnit)** El runner antepone `«<Tipo> : »` solo cuando el espacio de nombres de la excepción no es `Xunit.Sdk`.
- Las aserciones de xunit 2.9.2, también `Assert.Fail` y las que llevan mensaje de usuario, son de `Xunit.Sdk`.
- Evidencia versionada:
  - `docs/automation/evidence/I-58-f1/raw/I58-F1-red.trx:11` («Assert.Equal() Failure…», sin prefijo);
  - frente a `I58-F1-stimulus.trx:17` («System.InvalidOperationException : …»).

**La primera línea de la traza:**
- **(xUnit)** El runner filtra los marcos `Xunit.*`, así que la primera línea es el marco de la prueba que asevera: `I58-F1-red.trx:14` empieza en un ayudante de `tests/`.
- **Las dos condiciones se complementan.** `I58-F1-stimulus.trx:18` muestra una excepción **lanzada desde `tests/`** (primera línea en `tests/`) que solo el prefijo rechaza. Es el caso de los ayudantes de `EditorWindowTestSupport`, que lanzan `InvalidOperationException` (`tests/RackCad.UI.Tests/EditorWindowTestSupport.cs:24-25`, `:33-34`, `:104-105`). Se rechazan, y es correcto.

**Dentro de `StaTestRunner.Run`:**
- El cuerpo corre en el hilo STA y la excepción se captura en la lambda (`tests/RackCad.UI.Tests/StaTestRunner.cs:85-92`).
- Se relanza con `ExceptionDispatchInfo.Capture(captured).Throw()` (`:97`), que conserva el tipo (sin prefijo) y los marcos originales.
- La traza queda así:
  1. la lambda o el ayudante de la prueba, en `tests/`;
  2. `StaTestRunner.<Run>b__0` (`:87`);
  3. «--- End of stack trace from previous location ---»;
  4. `StaTestRunner.Run` (`:97`);
  5. el método de prueba.
- Lo confirma un TRX real de la CI de UI, no versionado (`artifacts/` está en `.gitignore:3`): `artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T090257Z-3038/red-ci-ui/ui.trx`, con la primera línea en `tests\RackCad.UI.Tests\…`.
- **(WPF)** Un `Dispatcher.Invoke(Action, prioridad)` anidado relanza desde la tarea de la operación y conserva la traza.

**Un riesgo que el criterio no ve:** una aserción dentro de una operación encolada por la propia prueba (un `BeginInvoke` o el bombeo de CT-03) mientras haya un configurador vivo. Su manejador de `Dispatcher.UnhandledException` marca `Handled = true` (`RackFrameConfiguratorWindow.xaml.cs:73`, `:756-760`), y la aserción desaparece. La regla «ningún `Assert` dentro de un delegado» (:360) solo nombra los delegados del producto (V-M-7).

### 3. R-I-2 y R-I-3: las reglas frente a la tabla

**Las filas frente a la letra de (a) y (b):** todas encajan, con dos tensiones que la frase para el Controller absorbe:
- **CT-01 con `CloseButton`:** su manejador, `Close()` → `OnClosing` → `MayClose`, puede abrir el aviso de descarte, un `MessageBox`. Eso choca con «no desvía un gesto hacia un botón cuyo manejador abre un diálogo» (:288). Véase V-M-1.
- **CT-23, «sin confirmar el módulo»:** debilita el bloqueo cuando este viene de la edición de módulo. Es inocuo, porque ninguna otra prueba pulsa «Insertar varias…» y la de CT-23 no muestra la ventana.

**¿Hay agujero por las filas genéricas?** No:
- las de CT-08, CT-09, CT-19, CT-26 y CT-27 (1) son clases textuales o de texto;
- las de CT-15, CT-11 y CT-17 no abren ningún modal;
- la de CT-14 actúa en un diálogo que ninguna prueba muestra.

**Lo que falta** es que la entrega diga qué fila instancia cada mutante. Sin eso, el Controller no sabe qué mutantes son «de la tabla» (V-M-8).

### 4. R-I-4 a R-I-6: el control positivo, CT-04 y CT-16

- **El control como medición previa más una aserción incondicional** es coherente con «toda prueba falla en el RED»: si el control cae en el RED, el método falla por aserción.
- **El control de Enter («\x1B» llega a `IsCancel`) no es un ancla:**
  - con «Close_Click sin efecto», si el control se observa por el cierre;
  - o retirando `IsCancel` de `CloseButton`, si se observa por el `Click`.

  Hay que ejercerlo antes del cierre, o sobre una segunda instancia.
- **La ventana sonda opcional** es código solo de prueba: ningún mutante de producto la cambia (V-I-1).
- **Si el control de Escape falla sobre `BaseSha`,** CT-01 se queda sin control y sin respaldo (V-I-2).
- **CT-04 es realizable:** `DispatcherOperation.Abort()`, una bandera local y la captura por acción. Un detalle: el respaldo escrito como `try { … } finally { … }` no «nunca lanza» (V-M-6).
- **CT-16 es realizable y suficiente para R-I-6.** El mutante de su fila solo mata «escenifica igualmente» si el gesto sustituye la instancia (V-M-4).

### 5. R-I-7

- **El ancla en `ChainBaseSha`:**
  - el Controller la conoce, porque está en la delegación;
  - no choca con Scope (#7), que sigue en `BaseSha`;
  - ni con la regla de §16.8: `RT` se calcula desde `ChainBaseSha`, así que en una corrección cubre los siete archivos.
- **Una corrección** que cambie 2 de las 7 pruebas cumple INV-01, y una que revierta un mutante residual también.
- **El único roce es con Core en `ExpectRed` false**, con precedente en F4-E56 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:1927`). Lo recoge V-M-11.

### 6. Coherencia global

**Los números cuadran:**
- 14 rutas de `MutantScope`, que existen todas;
- 21 = 7 + 14;
- `MinSelected` UI 22 = 24 − 2;
- Core 4;
- G3 10.

**Las referencias** cruzadas internas resuelven.

**El §10.2** coincide con la RR.

**Contradicciones menores** (V-M-11):
- INV-04 (:519) frente a CT-10 y CT-11 (:374-375);
- CT-17, que muestra la ventana y no lo dice el D-T1B-05 (:404);
- el «build-plugin omitido» del RED en las correcciones con Core en false.

---

## Importantes

### V-I-1. La lista de aserciones ancla está incompleta y se lee como cerrada

**Evidencia:**
- **El paquete** (:331-335): «algunos subpuntos obligatorios no tienen ningún mutante posible… Cada uno va en el mismo método…», y cuatro casos a continuación.
- **C04** (:576) da STOP a «un resultado obligatorio… que no detecta ningún mutante válido y que no es una aserción ancla declarada…».
- **Subpuntos obligatorios sin mutante posible que no están en la lista:**
  - **CT-13**, `Owner` de S-3 y de S-4 «aunque S-3 esté abierto» (:377). Las asignaciones `{ Owner = this }` están en `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3015-3017` y `:2716-2721`, y (b) prohíbe tocarlas (:284-285).
  - **CT-16**, `Owner` de S-2 (:380). La asignación está en `.xaml.cs:2426`.
  - **CT-02 y CT-03**, «el dueño `null`» del aviso (:366). Lo decide `src/RackCad.UI/Shell/EditorClosePolicy.cs:112`, fuera de `MutantScope`, y (b) prohíbe tocar `EditorDiscardPrompt` (:286).
  - **CT-10**, «NameBox es un `TextBox`» (:374), si se asevera sobre el control: `.xaml:47` lleva `TextChanged`, y `.xaml.cs:1204` usa `.Text`.
  - **La ventana sonda opcional de Enter** (:356), que es código solo de prueba.
- **El precedente:** el Controller ya leyó una desviación declarada como contraria al paquete y dio STOP (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:1932-1934`). Un ancla que el Worker declare fuera de la lista abre el mismo riesgo.

**Corrección:**
1. Añadir a la lista los cinco casos.
2. Añadir la regla general: «es ancla admisible toda aserción cuyo observable ningún mutante permitido por (a) y (b) puede cambiar, como un `Owner`, el dueño del aviso, el tipo de un control con manejadores, código solo de prueba o un archivo fuera de `MutantScope`. Va siempre declarada y en el mismo método que una aserción detectada. La lista no es exhaustiva».
3. Para la sonda: «si se escribe, va en el método de CT-01 como ancla; nunca en un método propio».

### V-I-2. CT-01 no tiene respaldo si el control de Escape falla sobre `BaseSha`

**Evidencia:**
- **El control de Enter** es «la misma ruta… con "\x1B"… (el control de CT-02)» (:356).
- **El respaldo** solo se declara para CT-02 (:366).
- **CT-01 es obligatorio** y su único contenido de ejecución es el gesto (:365).
- **INV-04:** «Un gesto de teclado vale solo con su control positivo» (:517).
- **Ninguna parada encaja:** C01 trata del producto o una costura, y C03 de AutoCAD, `MessageBox` o escrituras (:573-575).
- **Resultado:** si `ProcessKey(PresentationSource, "\x1B")` no cierra la ventana, el Worker tiene que inventar un respaldo, que es una desviación que el Controller puede leer como contraria al paquete, o parar sin una regla que lo diga. Es el caso que R-I-4 pedía cerrar («la duda entre C01 y C04»).

**Corrección:** añadir a CT-01: «Respaldo, decidido por la misma medición que el de CT-02:
- CT-01 conserva su método y fija solo la declaración: ningún `Button` `IsDefault` en el árbol, con el mutante `IsDefault` en `RestoreButton`;
- el gesto de Enter pasa a `KnownLimitations` y a D-T1B-07, sin STOP.

La sonda, si existe, no sustituye al control sobre la ventana de Push Back.»

## Menores

### V-M-1. La tabla prevista no es coherente en la conjunción, que es el RED

**Evidencia:**
- **«Close_Click sin efecto»** (fila de CT-02 y CT-03, :302) anula dos cosas:
  - **`IsDefault` en `CloseButton`** (fila de CT-01, :301): Enter llega a `Close_Click`, que ya no hace nada, así que «no cierra, no pide, no recalcula» pasa en el RED. Solo falla «ningún `IsDefault` en el árbol».
  - **Todo mutante de `OnClosing` para el caso «No»:** Escape y el peer llegan a `Close_Click` y nunca a `OnClosing`. Un método que solo asevere «sigue abierta, con trabajo pendiente y sin petición» pasa.
- **`CloseButton` choca con la letra de (b)** (:288): su manejador puede abrir el aviso de descarte.
- **El paréntesis de la fila** «ninguna prueba muestra la ventana como modal» (:302) es falso si CT-04 se promueve. El respaldo lo cubre.

**Corrección:**
- CT-01, solo en `RestoreButton`: `Restore_Click` → `LoadFromModel` → `Recompute()`, `.xaml.cs:3288` y `:426`.
- En CT-02 y CT-03, cada caso con trabajo pendiente asevera la cuenta y el texto de la pregunta.
- Corregir el paréntesis.
- Una frase: «la tabla se razona mutante a mutante; el Worker comprueba la conjunción».

### V-M-2. Una variante de la fila de CT-24 no compila

`this.DrawPushBackView(` da CS0176, porque `DrawPushBackView` es `internal static` (`src/RackCad.Plugin/RackPushBackCommands.cs:75`). La CI del RED no lo detectaría (respuesta 1).

**Corrección:** dejar solo `global::RackCad.Plugin.RackPushBackCommands.DrawPushBackView(` o `RackPushBackCommands.DrawPushBackView(`.

### V-M-3. La regla (a) del código muerto y la definición de «aviso»

- «Un miembro privado» (:281, :318, :326) tiene que ser «un **método** privado». Un campo da CS0169 o CS0414, y una función local CS8321.
- La lista de avisos prohibidos tiene que nombrar también CS0168 y CS8321, además de CS0162 y CS0219.
- INV-12 (:547) dice «sin avisos nuevos», pero el gate solo mira CS, MSB, NU y NETSDK (`eng/ci/verify-autocad-references.ps1:110-113`). Hay que decir si cuentan los de xunit.analyzers (`xUnit*`) y los CA.

### V-M-4. El mutante de CT-16 solo actúa con un gesto que sustituya la instancia

- `ShowHeaderConfigurator` devuelve `window.Configuration ?? copy` (`.xaml.cs:2446`). La instancia solo se sustituye con «Aplicar», «Restaurar estándar» o abrir un proyecto (`:2416-2421`).
- Con otro gesto, `return copy` es equivalente: `edited` sigue sin ser nulo y la escenificación ocurre igual (`:1938-1946`).

**Corrección**, una de dos:
- exigir el gesto `ApplySimpleConfiguration` con otra altura (el de `QuickConfigAt`) y aseverar la altura escenificada;
- o cambiar el mutante a «devuelve `null` si `window.DialogResult != true`», que mata OD-3 directamente y también CT-19.

### V-M-5. El mutante de CT-18 no mata lo que CT-18 describe

- La fila (:317) pone `EnsureId` antes de la guarda de validez.
- CT-18 (:382) fija «no marca inserción», y `EnsureId` no toca `InsertRequested`.

**Corrección:** que CT-18 asevere también `RackId` nulo, o cambiar de mutante.

### V-M-6. El respaldo de CT-04 sí puede lanzar

- `try { bandera = true; cancelar el módulo } finally { … Close(); }` (:368) propaga lo que lance el `try` y lo que lance el `Close()` del `finally`.
- **(WPF)** Con `BeginInvoke(Delegate)`, eso sale del bucle de `ShowDialog`.

**Corrección:** `catch (Exception ex) { capturada ??= ex; }` en los dos tramos, como en las demás acciones.

### V-M-7. Precisiones del criterio del TRX

**Las rutas de la traza:** «Bajo `tests/`» se mide relativo a la raíz del checkout y con cualquier separador. En la CI son `<unidad>:\a\<repo>\<repo>\tests\…`, y en local es la ruta absoluta del worktree.

**El prefijo:** se reconoce como nombre de tipo cualificado, `^(?:[A-Za-z_][\w`]*[.+])+[A-Za-z_][\w`]* : `, para no confundirlo con un mensaje de usuario que contenga « : ».

**De qué TRX se cuenta:** hay que decir si es la corrida local filtrada o el TRX completo de la CI filtrado por nombre. En el segundo, `Total` ≠ `Selected`.

**Los delegados:** la regla de :360 se extiende a los delegados que encola la propia prueba (respuesta 2).

### V-M-8. La frase para el Controller y la redacción de C04

**La frase para el Controller:**
- (c) es un resultado de la CI y no puede ser conforme «por construcción» (:293, :510).
- Corrección: «conformes a (a) y (b) por construcción; (c) se juzga siempre sobre la CI del RED».

**C04** (:576) mezcla «resultado» y «aserción».

Corrección: «…o un resultado del TRX que ningún mutante válido detecta en la conjunción; o una aserción sin mutante propio que no esté declarada como ancla en un método detectado».

**La tabla de mutantes de la entrega:** marca cada mutante con la fila prevista que instancia, o con «fuera de tabla».

### V-M-9. «No actúa en el constructor» es ambiguo

El constructor ejecuta:
- `InitializeComponent`, es decir, todo el XAML;
- `LoadNew`;
- `LoadFromModel`;
- `Recompute` y `UpdateButtons` (`.xaml.cs:128-153`, `:262-281`).

Leído así, la regla prohibiría las filas de CT-09, CT-10 y CT-12 y las alternativas de CT-05 y CT-10.

**Corrección:** «no edita el cuerpo del constructor ni hace que la construcción lance».

### V-M-10. Faltan dos mutantes en la tabla

- **El respaldo de CT-02** dice «con su mutante» (:366) y no tiene fila. Corrección: «retirar `IsCancel` de `CloseButton` o ponerlo en `RestoreButton`».
- **La fila de CT-20** (:319) nombra `RackViewBatchDialogPresenter.cs` pero no le da mutante. Corrección: `Owner = (Window)owner` (`src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs:59`).

### V-M-11. Coherencia

**Las clases y las ventanas:**
- **D-T1B-05** (:404) dice «CT-16 muestra la ventana», pero CT-17 también tiene que mostrarla: S-3 lleva `Owner = this` (`.xaml.cs:3016`), que lanza si la ventana nunca se mostró.
- **INV-04** (:519) admite el foco y la tabulación efectivos en las pruebas «si son estables», mientras que CT-10, CT-11 y :361 los excluyen siempre. Hay que alinear el texto.

**Las correcciones con Core en false:**
- `tests` pasa, así que `build-plugin` sí corre sobre el RED. Lo contradice «omitido» (:263, :553).
- §16.9 #9 pide literalmente «el job Core en `failure`» (`docs/AUTOMATION_PLAN.md:619`).

Corrección: declararlo en la regla de correcciones (:340) con la cita de F4-E56.

**La redacción del §3:** «Los mutantes del RED tocan archivos de G1, G2, G3, G5 y G6» (:226) omite los fragmentos compartidos del Plugin.

## Lo comprobado y correcto (breve)

**`MutantScope` y las rutas:**
- las 14 rutas de `MutantScope` existen;
- `RackViewBatchDialog.xaml` tiene `CenterOwner`, `IsCancel` «Cancelar» e `IsDefault` «Insertar cola» (`:5`, `:35-36`);
- hoy no hay ningún archivo `PushBackClassic*` en `tests/`.

**La base:**
- G1 no contiene ningún token de CT-19, y en `src/` no hay ningún `InsertRequestedRaised +=`. La guarda pasa sobre la base.
- `OnApplyTemplate` solo fusiona el respaldo del shell si `ShellZoneSpacing` no resuelve (`src/RackCad.UI/Shell/RackEditorVisualShell.cs:45-53`). El oráculo «`MergedDictionaries.Count == 0`» de CT-15 es medible y el mutante lo mata.

**La suite existente:**
- ninguna prueba llama a `ShowDialog`;
- ninguna pulsa `InsertBatchButton`.

Las filas de comportamiento que tocan los caminos de cierre no dejan ningún modal en la suite existente.

**CT-04 y CT-16:** las especificaciones nuevas usan solo API pública (`DispatcherOperation.Abort`, `ButtonAutomationPeer`, `AccessKeyManager.ProcessKey`), sin infraestructura nueva (INV-10).
