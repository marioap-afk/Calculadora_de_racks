# Re-revisión independiente y focalizada — Paquete del contrato de F4-T1B (v2)

**Objeto:** `I-64-f4-t1b-gate-contract-package.md` v2 (citado como «paquete :línea»), contrastado con la v1 (`_package-v1.md`), con la revisión de la v1 (`independent-review.md`, «IR») y con el código en `c3ae1c970c908ef541840fc583680b86376cae0b`.

**Método:**
- Solo lectura: no se compiló ni se ejecutó ninguna prueba, y no se escribió nada en el repositorio.
- Todas las rutas son relativas al repositorio.
- Lo que es comportamiento de WPF o de xUnit y no está medido en el repositorio se marca **(WPF)** o **(xUnit)**.
- La disposición del autor («Aceptado») no se ha tomado como prueba: cada corrección se ha comprobado contra el código.

## Veredicto

**EMITIBLE TRAS CORRECCIONES — 1 bloqueante / 7 importantes / 11 menores.**

**Lo que la v2 resuelve de verdad:**
- B-2: la escritura de los mutantes ya está dentro del alcance.
- I-4, I-5, I-9, I-10, I-11 e I-13.
- Casi todos los menores.
- La idea de D-T1B-03 sigue siendo la correcta:
  - las 14 rutas de `MutantScope` existen;
  - todas las filas de la tabla caen en archivos de `MutantScope`;
  - ningún mutante previsto rompe la carga de la ventana;
  - salvo uno (R-M-1), todos compilan tal como están escritos.

**Lo que impide emitirla tal como está:**
- **R-B-1.** La realizabilidad se demuestra **por CT**, pero el criterio (`Failed == Selected`) y la parada F4T1B-C04 son **por método**, o por fila de `Theory`. La v1 tenía C04 por CT (`_package-v1.md:426`). Hay subpuntos obligatorios que ninguna clase de mutante permitida puede hacer fallar si el Worker los separa en métodos propios. Es el mismo defecto que B-1, ahora condicionado a cómo el Worker reparta los métodos.
- **Cuatro importantes** pueden llevar a REWORK, a STOP o a una CI del RED inverificable sin que el Worker se equivoque:
  - R-I-1: el criterio del TRX rechaza fallos de aserción legítimos;
  - R-I-2: dos filas pueden colgar la CI del RED;
  - R-I-3: las reglas (a) y (b), leídas al pie de la letra, prohíben filas de la propia tabla;
  - R-I-5: el respaldo de CT-04 deja operaciones vivas después de la prueba.
- **Otras tres importantes** son lagunas de especificación con una corrección concreta:
  - R-I-4: el control positivo de los gestos;
  - R-I-6: el configurador sin cerrar;
  - R-I-7: el ancla de INV-01 en las correcciones.

Ninguna exige rediseñar la tarea, y todas se corrigen en el texto del paquete.

## Estado de los hallazgos de la v1

| Hallazgo | Estado | Comprobación |
|---|---|---|
| B-1. CT-26 sin mutante posible | **Parcial** | Resuelto para CT-26: `RackMenuCommands.cs`, `Views/RackViewBatchProducts.cs` y `Views/RackViewPlacement.cs` están en `MutantScope`, solo para mutantes textuales, y sus filas compilan. La tabla pedida «**método** CT → archivo → mutante» (IR :68) se hizo **por CT**, y C04 pasó a ser por método. Hay subpuntos que no se pueden matar → R-B-1 |
| B-2. Los campos prohibían las escrituras del RED | **Resuelto** | Allowed tiene 7 + 14 rutas, y Forbidden va por raíces sin esas 14. Scope (`docs/AUTOMATION_PLAN.md:617`, #7) mide el diff neto y nc2 (`docs/automation/agent-execution/README.md:229-230`) trabaja sobre él, así que los dos son coherentes. En las correcciones queda el ancla a `BaseSha` → R-I-7 |
| I-1. Pruebas existentes rotas en el RED y «por aserción» | **Parcial** | Se admiten las pruebas existentes rotas y se prohíbe `Skip`. El criterio mecánico, tal como está escrito, rechaza las aserciones con mensaje de usuario y no puede leer el tipo de la excepción en el TRX → R-I-1 |
| I-2. Mutantes que cuelgan la CI | **Parcial** | Se adoptan las reglas (a) a (c) y la clase permitida por archivo. La regla (b) no cubre los mutantes que desvían un gesto hacia un modal ni los que debilitan una guarda (R-I-2), y contradice filas de la tabla (R-I-3) |
| I-3. Sin regla RED para las correcciones | **Parcial** | La regla existe. Falla el ancla `BaseSha` frente a `ChainBaseSha`, no cubre el caso del RED vigente nulo y no dice qué mutantes se vuelven a aplicar → R-I-7 |
| I-4. CT-27 frente a T4B | **Resuelto** | CT-27 queda limitado a las firmas y a los cuatro caminos clásicos. Los caller-owned de otros sistemas se llaman desde `ProjectVariableMutationExecutor.cs` y `RackSelectivoInsertIntegration.cs`, fuera de esos cuatro, así que la guarda pasa sobre la base. `KindHandlers/PushBackKindHandler.cs` solo despacha `Edit` y el BOM |
| I-5. El §8 dictaba el alcance de T4B | **Resuelto** | El §8 se reescribió, y el §3 aclara para qué sirve la clausura |
| I-6. Los efectos del configurador | **Parcial (lo esencial, resuelto)** | Cerrar dentro de la costura funciona y es la misma escritura (detalle abajo). Quedan dos caminos que dejan el configurador sin cerrar → R-I-6 |
| I-7. Gestos de teclado sin medir | **Parcial** | `ProcessKey(PresentationSource, "\x1B", false)` y el peer son API pública válida, sin infraestructura nueva. El control positivo no está definido para Enter y admite leerse como una rama en ejecución → R-I-4 |
| I-8. CT-04 puede colgar la CI | **Parcial** | El respaldo garantiza el retorno de `ShowDialog` si se ejecuta. Faltan las operaciones que sobreviven a la prueba y las excepciones; con el mutante previsto de CT-04, ese caso se da seguro en el RED → R-I-5 |
| I-9 | **Resuelto** | Se fija el orden por slot y el de las zonas. Que la zona sea mutable es R-B-1 |
| I-10 | **Resuelto** | CT-23 va en ejecución. El riesgo de cuelgue de su mutante está en R-I-2 |
| I-11 | **Resuelto** | Están CT-28 y OD-13. `MinSelected` UI = 22 cuadra con los CT obligatorios, salvo con el respaldo de CT-02 → R-M-4 |
| I-12 | **Resuelto en lo esencial** | Queda el residual de una inestabilidad que solo aparezca en la CI del GREEN → R-M-11 |
| I-13 | **Resuelto** | C05 queda acotada |
| M-1 a M-20 | **Resueltos, salvo M-10 y M-17** | G6 coincide archivo por archivo con `closure-measured.json` (17), y G2 suma 43 (17 + 9 + 2 + 12 + 3). <br>• M-10: falta «sin avisos nuevos» → R-M-2. <br>• M-17: el §10 lo da por aplicado, pero la cláusula de las corridas locales no está en el cuerpo → R-M-3 |

## Respuestas a las comprobaciones pedidas

### 1. D-T1B-03 y el contrato

**¿Compilan los mutantes?** Todas las filas compilan, salvo `ShowDialog((Window)owner)` en `UiSupport.cs` (R-M-1).
- `(Window)this` y `TryShow((Window)this` compilan porque `.xaml.cs` importa `System.Windows` (`:6`).
- `ShowModalWindow((System.Windows.Window)window)` y `regen: (false)` compilan.
- Los valores por defecto entre paréntesis compilan: `string payloadJson = (null)` y `bool regen = (true)` son expresiones constantes (`src/RackCad.Plugin/Systems/PushBack/PushBackFrontalDrawService.cs:26-27`, `:44`).
- La suscripción muerta a `session.InsertRequestedRaised` compila (`src/RackCad.UI/Editor/RackEditorSession.cs:124`).
- Los mutantes textuales no producen avisos CS: un miembro privado sin uso solo da IDE0051, que no se aplica en el build.

**¿Rompe alguno la carga de la ventana?** Ninguno de los previstos:
- `IsDefault`;
- `ElementName` apuntando a otro control existente;
- intercambiar dos controles del lateral conservando su `x:Name`;
- `Title`;
- el valor de `ActionGap`;
- un diccionario fusionado en `Shell.Resources`.

El riesgo está en las alternativas que el texto no prohíbe, como retirar recursos del XAML o actuar en el constructor (R-I-2).

**¿Cuelga alguno la CI?** Dos casos (R-I-2):
- CT-01, si `IsDefault` cae en BOM, Biblioteca o «Insertar varias…»;
- CT-23, en su variante bloqueada, si el bloqueo lo produce la edición de módulo y la ventana está mostrada.

CT-04 no cuelga, pero deja operaciones vivas que pueden tumbar el testhost (R-I-5).

**¿Hacen fallar las pruebas por aserción?** Por CT, sí. Por método, no siempre. Las partes que preguntaba la orden:

| Parte | ¿Puede fallar por mutante? |
|---|---|
| CT-11 (2), orden de zonas | Solo con una plantilla completa redefinida en el XAML, o con un reordenamiento en `.xaml.cs` que «del mismo archivo» impide. No está en la tabla |
| CT-15, «el shell no activó su respaldo» | Sí, si el oráculo es «`Shell.Resources.MergedDictionaries` vacío»; no, si es por identidad (R-M-6) |
| CT-19, `InsertRequestedRaised +=` | Sí, con una suscripción muerta en `.xaml.cs` (no está en la tabla) |
| CT-21 | Sí: `Owner = (context.Owner)` |
| CT-12 | Sí con `Title`. Su subpunto «`Owner` en `null`», **no**: la regla (b) prohíbe tocar `Owner` → R-B-1 |
| CT-24, «sin `Owner =`», y CT-27 (2) | **No** como métodos separados → R-B-1 |

**Alcance y Controller:**
- Allowed, Forbidden, el esquema (arrays de cadenas, `docs/automation/agent-execution/schemas/gate-contract.schema.json:28-29`), Scope y nc2 son coherentes en el intento 1.
- La frase para el Controller solo cubre Scope, que ya se evalúa sobre el diff neto, así que ahí es redundante. No cubre el vector real de STOP falso, que es C04 juzgando cada mutante contra el texto literal de (a) y (b) → R-I-3.

**El criterio del TRX** es comprobable en el prefijo del mensaje, pero no en el tipo, y rechaza los mensajes de usuario → R-I-1. Los mensajes reales de xUnit 2.9.2 tienen esta forma, custodiada en el repositorio: `Assert.Equal() Failure: Values differ` (`docs/automation/evidence/I-58-f1/raw/I58-F1-red.trx:11`).

**La regla de correcciones** es coherente con `RT` y `ChainRedFiles` de §16.8: `RT` es solo `tests/` (`docs/AUTOMATION_PLAN.md:594-599`), y el GREEN de una corrección solo revierte mutantes. Pero su ancla es incorrecta → R-I-7.

### 2. I-6, el configurador (opción a)

**Cerrarlo dentro de `HeaderConfiguratorPresenter` es correcto:**
- La costura es `Action<RackFrameConfiguratorWindow>` (`src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:2410`).
- Producción lee `window.Configuration ?? copy` **después** de la costura (`:2446`), y `Configuration => ViewModel.Configuration` (`src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs:89`).
- `OnClosed` no suelta el ViewModel (`:602-616`).
- Es exactamente el patrón existente `QuickConfigAt`, que cierra dentro de la costura (`tests/RackCad.UI.Tests/PushBackHeaderAuthorityWindowTests.cs:68-75`), y lo verifica `:100-106` en cada CI.
- **(WPF)** `Close()` sobre una ventana nunca mostrada ejecuta `OnClosing` y `OnClosed`. El precedente lo confirma.

**Elimina el manejador fugado:** `OnClosed` desuscribe `Dispatcher.UnhandledException` (`:606`).

**Es la misma escritura** que la suite ya hace, en ruta y en contenido, **siempre que el configurador nunca se muestre**:
- `SaveCurrentLayout` guarda los `ActualWidth` y `ActualHeight` (`:670-690`), que valen 0 sin medición;
- es lo mismo que escriben las pruebas PBH.

**Quedan dos caminos que dejan el configurador sin cerrar** (R-I-6):
- una aserción dentro de la costura (el RED la dispara seguro);
- el aviso propio del configurador (`ConfirmDiscard`, `:182-194`, `:210-218`) contestado «No».

### 3. I-7, los gestos

**`AccessKeyManager.ProcessKey(object scope, string key, bool isMultiple)` es API pública** **(WPF)**:
- el primer argumento es `object`, así que admite un `PresentationSource`;
- `IsCancel` registra «\x1B» y `IsDefault` registra «\r» (fuente de `Button`);
- `EditorActionBar` es un `WrapPanel` sin manejadores de ámbito (`src/RackCad.UI/Shell/EditorActionBar.cs:15`), y en `src/` no hay ningún manejador de `AccessKey`;
- con la ventana mostrada, el ámbito por defecto es su `HwndSource` y la llamada llega a `CloseButton` (`.xaml:789`).

**Tres matices:**
- el valor de vuelta significa «hay más coincidencias», no «se procesó»;
- `FromVisual` devuelve `null` si la ventana no está mostrada, y con `null` el ámbito pasa a ser la ventana activa;
- no recorre los manejadores de teclado del elemento con foco (R-M-8).

**`ButtonAutomationPeer` + `IInvokeProvider.Invoke` es realizable sin paquetes ni P/Invoke:**
- `UseWPF` aporta `UIAutomationProvider` (`tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:5-6`);
- **(WPF)** el clic se encola con `BeginInvoke(Input)` y llega a `Button.OnClick`, que incluye el `DialogCancelCommand` de `IsCancel`;
- bombear con un `Dispatcher.Invoke(…, Background)` en el mismo hilo, o con `PushFrame`, es API pública;
- **(WPF)** con el botón deshabilitado lanza `ElementNotEnabledException` (R-I-1).

**El control positivo y el respaldo** tienen dos defectos (R-I-4):
- no están definidos para Enter;
- admiten leerse como una rama en tiempo de ejecución.

### 4. I-8, CT-04 y `ShowDialog`

**El bucle anidado dentro de `StaTestRunner.Run` es seguro:**
- el cuerpo corre como una operación del dispatcher STA (`tests/RackCad.UI.Tests/StaTestRunner.cs:83`);
- el anidamiento está permitido;
- la excepción se captura y se relanza (`:84-98`);
- la colección `DisableParallelization` impide la concurrencia.

**Dos riesgos:**
- el `Invoke` entre hilos no tiene timeout;
- lo que sobrevive a la prueba se ejecuta en el `Dispatcher.Run()` de nivel superior (`:61`), donde una excepción no tiene manejador.

**El respaldo garantiza que `ShowDialog` vuelve si se ejecuta y no lanza:** con la sustitución en «Sí», `MayClose` es verdadero. **Con lo escrito no está garantizado:**
- si una acción cierra la ventana antes, el respaldo queda vivo;
- si el respaldo o una acción lanza, se pierde la excepción o se tumba el testhost, según la API;
- el mutante previsto de CT-04 produce justamente ese cierre anticipado.

Detalle en R-I-5.

### 5. Problemas nuevos

Son R-B-1, R-I-1 a R-I-7 y R-M-1 a R-M-11:
- la clausura del §3 coincide con `closure-measured.json`;
- `MinSelected` 22 y 4 cuadran, salvo con el respaldo de CT-02 (R-M-4);
- CT-28 está bien trazado;
- las paradas solo tienen el problema de C04 por método (R-B-1, R-I-3).

---

## Bloqueante

### R-B-1. La tabla prevista demuestra un mutante por CT, pero el criterio y la parada son por método o por fila: hay subpuntos obligatorios que no se pueden matar

**Evidencia:**
- **La tabla y su regla:** la tabla es «CT → archivo → mutante» (paquete :285-314), y «el Worker puede usar otro mutante **del mismo archivo** si mata el mismo método» (:285).
- **El criterio:** `Failed == Selected == Total` (:470).
- **La parada:** C04, «un **método** obligatorio sin ningún mutante válido que lo detecte» (:536). En la v1, C04 era por CT (`_package-v1.md:426`).
- **Nada limita el reparto:** D-T1B-04 pide «al menos un método propio» por CT (:331) y no limita cuántos.
- **(xUnit)** Cada fila `InlineData` de una `Theory` es un resultado distinto en el TRX.

**Subpuntos obligatorios que no se pueden matar** si van en su propio método o en su propia fila:

| Subpunto | Por qué no se puede matar |
|---|---|
| CT-12, «`Owner` en `null`» (:348) | Solo una asignación lo cambia, y la regla (b) prohíbe tocar `Owner` (:278) |
| CT-24, «sin `Owner =`» (:360; `src/RackCad.Plugin/RackPushBackCommands.cs:47-48`) | El archivo admite solo mutantes textuales, y la lista cerrada de (a) (:276) no incluye ninguna mutación que **añada** texto |
| CT-27 (2), «los caminos clásicos no llaman a los caller-owned» (:363) | `src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs` **no está** en `MutantScope`. Los otros tres archivos admiten solo mutantes textuales, y (a) no permite añadir una llamada |
| CT-11 (2), el orden de zonas (:347) | Vive en `src/RackCad.UI/Themes/Generic.xaml:151-174`, fuera de `MutantScope`. Desde la fila de `.xaml` exigiría redefinir la plantilla entera; un reordenamiento en `.xaml.cs` no es «del mismo archivo» |
| CT-07 en `Theory` por botón | El mutante previsto, «dos índices intercambiados» (`.xaml.cs:3214-3251`), mata 2 de las 4 filas |

Hay casos parecidos en CT-10: el texto en `LoadNew`/`LoadExisting` se mutaría en `.xaml.cs`, pero la fila es `.xaml`.

**Problema:** si el Worker separa esos subpuntos, el filtro no puede cumplir `Failed == Selected` y C04 es STOP seguro. Es la misma clase de defecto que B-1, y la propia v2 lo agravó al llevar C04 a nivel de método.

**Corrección:**
1. **Declarar la granularidad.** `Failed == Selected` y la tabla de la entrega van por resultado del TRX (método o fila).
2. **Listar en el paquete los subpuntos sin mutante posible:** como mínimo CT-12 «`Owner` en `null`», CT-24 «sin `Owner =`», CT-27 (2) y CT-11 (2). Exigir que cada uno vaya **en el mismo método** que un subpunto con mutante de la tabla (un «ancla»). La entrega declara qué aserciones quedan sin mutante propio.
3. **`Theory`**, solo si cada fila tiene su mutante. Si no, `Fact`.
4. **Sustituir** «otro mutante del mismo archivo» por «otro mutante de cualquier archivo de `MutantScope`, con la clase que ese archivo permite».
5. **Ampliar (a)** con «código muerto sin uso (un miembro privado nunca llamado) y cualificación redundante (`this.`, espacio de nombres, `global::`)». Así, las guardas de ausencia de los archivos textuales pasan a poder matarse, salvo `PushBackKindHandler.cs`, que queda anclado.

## Importantes

### R-I-1. El criterio mecánico del TRX rechaza fallos de aserción legítimos y no puede leer el tipo de la excepción

**Evidencia:**
- **El criterio** (paquete :468): «una excepción de tipo `Xunit.Sdk.*` y un mensaje que empieza por `Assert.`».
- **El TRX no guarda el tipo.** **(xUnit)** xunit.runner.visualstudio 2.8.2 escribe en `<Message>` el mensaje tal cual cuando la excepción es de `Xunit.Sdk`, y `«<Tipo> : <mensaje>»` en cualquier otro caso:
  - `docs/automation/evidence/I-58-f1/raw/I58-F1-stimulus.trx:17` («System.InvalidOperationException : …»);
  - frente a `docs/automation/evidence/I-58-f1/raw/I58-F1-red.trx:11` («Assert.Equal() Failure: Values differ»).
- **Un mensaje de usuario sustituye al texto `Assert.X() Failure`** **(xUnit 2.5 y posteriores)**:
  - `tests/RackCad.Tests/NamespaceFolderGuardTests.cs:134` (`Assert.True(offenders.Count == 0, "Declaración…")`) produce `<Message>Declaración de namespace no canónica:` en `docs/automation/evidence/I-58-f2/rejected-39bb0b14/core-full.trx:12362`;
  - `Assert.Fail(msg)` hace lo mismo.
- **El repositorio usa mensajes de usuario a menudo:** hay 253 `Assert.True` o `Assert.False` con mensaje en `tests/RackCad.UI.Tests`, entre ellos en las clases de I-39 que el paquete manda seguir («se siguen las convenciones de I-39», :386). Ejemplo: `tests/RackCad.UI.Tests/RichEditorCloseContractTests.cs:106`.
- **Las precondiciones no aseveradas fallan por excepción.** Con todos los mutantes aplicados a la vez, la de CT-09 (`InsertBatchButton.IsEnabled = canOutput`) deshabilita el botón que pulsa CT-23. Si se pulsa por el peer, `IInvokeProvider.Invoke` lanza `ElementNotEnabledException` **(WPF)**.

**Problema:** una aserción correcta con mensaje, o una precondición no aseverada, da un RED que incumple INV-F4T1B-02, y por tanto REWORK. El tipo de la excepción no se puede comprobar en el TRX.

**Corrección:**
- **Redactar el criterio sobre lo que el TRX contiene:**
  - `outcome = Failed`;
  - un `<Message>` **sin** el prefijo `«<Tipo> : »`, que el runner pone a toda excepción ajena a `Xunit.Sdk`;
  - la primera línea de `<StackTrace>` en un archivo de `tests/`.
- **Alternativa:** prohibir el mensaje de usuario en `Assert.True`, `Assert.False` y `Assert.Fail` de las clases nuevas, y declararlo como desviación de la convención de I-39.
- **Añadir dos reglas:**
  - toda precondición es una aserción (`Assert.True(button.IsEnabled)` antes del peer; `Assert.NotNull(PresentationSource.FromVisual(w))`);
  - el criterio se comprueba con **todos** los mutantes aplicados a la vez, porque el RED es su conjunción.

### R-I-2. La regla (b) no cubre los mutantes que desvían un gesto a un modal sin costura ni los que rompen la carga: dos filas pueden colgar la CI del RED

**Evidencia:**
- **La fila de CT-01** (:289) dice «`IsDefault="True"` en un botón de acción». CT-01 necesita la ventana mostrada para `ProcessKey`. Con el mutante, Enter pulsa ese botón:

  | Botón | Lo que abre |
  |---|---|
  | `BomButton` (`.xaml:754`) | `new RackBomWindow(…) { Owner = this }.ShowDialog()` (`.xaml.cs:3438`) |
  | `SaveLibraryButton` (`.xaml:756`) | `PromptSaveToLibrary`, que crea la carpeta (`src/RackCad.UI/UiSupport.cs:68`) y abre `SaveFileDialog.ShowDialog(owner)` (`:81`) |
  | `InsertBatchButton` (`.xaml:782`), siempre habilitado | `TryShow`, que abre `ShowDialog` (`src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs:52-63`) |

  La regla (b) prohíbe **tocar** un `ShowDialog`, pero no **desviar** un gesto hacia él. Resultado: `blame-hang` y un TRX incompleto.
- **La fila de CT-23** (:309), «`InsertBatch_Click` sin confirmar el módulo», junto con «lo mismo con la salida bloqueada» (:359):
  - si el bloqueo del escenario lo produce la **edición de módulo** escenificada, el mutante deja la salida sin bloquear y el clic llega a `TryShow` (`.xaml.cs:3294-3311`);
  - con la ventana mostrada, eso cuelga; sin mostrar, `Owner = owner` lanza y el fallo es por excepción;
  - el paquete no dice si CT-23 muestra la ventana.
- **Retirar un recurso del XAML**, por ejemplo una alternativa de CT-15 que quite el `AppStyles` fusionado (`.xaml:10-14`), hace que cada `StaticResource` de la ventana falle en `InitializeComponent` (`.xaml.cs:131`). Son fallos por excepción en todas las pruebas que construyen la ventana, y la regla no lo prohíbe.

**Corrección:**
- **Ampliar (b):** «…ni desvía un gesto (`IsDefault`, `IsCancel` o una tecla de acceso) a un botón cuyo manejador abre un diálogo o escribe; ni retira o debilita una guarda que hoy separa una prueba de un modal sin costura o de una escritura (la validez, el bloqueo, `canInsertInAutoCad`); ni actúa en el constructor, ni retira recursos del XAML».
- **Fijar la fila de CT-01** en `IsDefault="True"` sobre `CloseButton` o `RestoreButton` (`.xaml:749`).
- **Fijar CT-23** con la ventana **sin mostrar** y con un bloqueo independiente de la edición de módulo (como `StageBlockingEdit`, `tests/RackCad.UI.Tests/PushBackOutputGateAfterRecomputeTests.cs:96-97`). Así, cualquier escape hacia `TryShow` lanza en `Owner = owner` en lugar de colgar, que es la misma señal que ya usa `tests/RackCad.UI.Tests/PushBackLibrarySaveGateTests.cs:89-95`.

### R-I-3. Las reglas (a) y (b), leídas literalmente, prohíben filas de la propia tabla: el mismo vector de STOP falso que en F4-T4A2

**Evidencia:**
- **La regla (b)** (:282) dice «nunca toca… un camino de `Close()` que **pueda** dejar un modal abierto». Contradice tres filas:
  - CT-02 y CT-03, «`Close_Click` sin efecto» (:290; `.xaml.cs:3381`);
  - CT-06, «sin el `Close()` de `RequestDraw`» (:293; `:3378`).

  En los dos casos es un camino de cierre, y en producción la ventana **es** modal (`RackPushBackCommands.cs:48`; `src/RackCad.UI/Editor/EditorModules.cs:115`), igual que en CT-04. La fila de CT-04 (:291) toca `OnClosing` directamente.
- **La lista cerrada de (a)** (:276) no recoge «una cualificación redundante» (CT-26, :312) ni «`this.SaveCurrentLayout()`» (CT-22, :308).
- **C04 convierte en STOP** «una clase que su archivo no permite o que viola las reglas (a) a (c)» (:536), y el Controller juzga cada fila de la tabla de la entrega. Precedente: el Controller leyó como prohibida una desviación declarada y dio STOP (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:1932-1934`).
- **La frase para el Controller** (:473) solo trata Scope, que ya mide el diff neto. No dice nada sobre C04.

**Corrección:**
- **Reescribir (b) en términos observables:** «…ni un camino de cierre de modo que una prueba de la suite, las de T1B incluidas y contando el respaldo de CT-04, deje un modal abierto».
- **Ampliar (a)** como en R-B-1.
- **Añadir para el Controller:** «las filas de la tabla prevista son conformes a (a)-(c) por construcción. C04 solo se juzga sobre los mutantes que el Worker declare fuera de ella, contra el texto de (a)-(c)».

### R-I-4. El control positivo no está definido para Enter y admite leerse como una rama en ejecución

**Evidencia:**
- **La regla** (:333): «el mismo gesto, sin trabajo pendiente, cierra la ventana… Si no la cierra, el gesto no es válido… y se va al respaldo».
- **CT-01 es un gesto de teclado** cuyo hecho es justamente que Enter **no** cierra: no hay ningún `IsDefault` (`.xaml:749-789`). Su control positivo no puede pasar nunca, y CT-01 no tiene respaldo declarado (:337). Queda la duda entre C01 y C04, o una prueba vacía.
- **Si el Worker programa «si falla el control → respaldo» como una rama**, falla en las dos direcciones:
  - en el RED, el mutante `Close_Click` sin efecto hace fallar el control, la prueba toma el respaldo y **pasa**, así que no se cumple `Failed == Selected`;
  - en el GREEN, ocultaría una regresión del gesto.
- **CT-04 dice «por Escape»** (:340) y no dice qué hacer si Escape tomó el respaldo.

**Corrección:**
1. **El control positivo es una medición sobre `BaseSha`** que decide el diseño antes del RED. En la prueba versionada es una aserción incondicional, sin ramas.
2. **Para Enter, el control es la misma ruta:** `ProcessKey` sobre el mismo `PresentationSource` llega con «\x1B» a un botón `IsCancel`, que es el control de CT-02. Opcionalmente, una `Window` sonda privada de la prueba, con un botón `IsDefault`, demuestra que «\r» por esa ruta llega a un `IsDefault`.
3. **Con el respaldo de Escape**, CT-04 y CT-28 usan solo el peer sobre «Cerrar». CT-28 ya lo dice.

### R-I-5. CT-04: el respaldo no cubre las operaciones que sobreviven a la prueba ni las excepciones de las acciones programadas

**Evidencia:**
- **La fila de CT-04** (:291), «`OnClosing` sin la rama `!InsertRequested`», admite dos lecturas, y las dos fallan:
  - **«nunca pregunta»:** el primer «Cerrar» cierra el modal y `ShowDialog` vuelve con el respaldo todavía encolado en `ApplicationIdle`;
  - **«pregunta siempre»:** CT-04 no cambia (`InsertRequested` es falso) y no falla.
- **La operación sobrante se ejecuta** en el `Dispatcher.Run()` de nivel superior del hilo STA (`tests/RackCad.UI.Tests/StaTestRunner.cs:61`) o dentro del bucle anidado de la prueba siguiente.
  - «Cambia la sustitución a "Sí"» hecho con `EditorDiscardPrompt.Substitute` (`src/RackCad.UI/Shell/EditorClosePolicy.cs:57-74`), un guardar-y-restaurar no reentrante, puede pisar la sustitución de la prueba siguiente de la colección.
- **Las excepciones** **(WPF)**:
  - **con `Dispatcher.BeginInvoke(Delegate, …)`**, la semántica heredada, una excepción de una acción sale del bucle anidado y termina `ShowDialog` con la ventana abierta y el respaldo vivo;
  - **fuera de la prueba**, la misma excepción no tiene manejador en el hilo STA: se cae el testhost y el TRX queda truncado;
  - **con `InvokeAsync`**, la excepción queda en la `Task` de la operación y nadie la observa. Es un escenario medio ejecutado que puede pasar.
  - Lo mismo vale para un `Assert` dentro de la sustitución, que corre dentro de `OnClosing`.

**Corrección:** especificar en CT-04 y en INV-F4T1B-07:
1. Cada acción programada captura su excepción en una variable local, y la prueba la relanza cuando `ShowDialog` vuelve.
2. El respaldo es `try { bandera = true; cancelar módulo } finally { if (la ventana sigue abierta) Close(); }`. Nunca lanza y conmuta una bandera local que lee la sustitución de la prueba, en lugar de llamar otra vez a `Substitute`.
3. Cuando `ShowDialog` vuelve, se llama a `Abort()` sobre toda `DispatcherOperation` que siga `Pending`.
4. No hay aserciones dentro de los delegados que ejecuta el producto (la sustitución y las costuras): se captura dentro y se asevera fuera.
5. La fila de CT-04 pasa a un mutante con efecto definido, por ejemplo el texto de la pregunta, que cambia el texto o la cuenta medidos.

### R-I-6. I-6: cerrar el configurador es correcto, pero quedan dos caminos que lo dejan sin cerrar

**Evidencia** (además de la respuesta 2):
- **Camino 1: una aserción dentro de la costura.** CT-16 lee `Owner` y la ubicación «al entrar en la costura» (:352). Si esa aserción va dentro del presentador y falla, la excepción sale antes del `Close()`.
  - En el RED eso es seguro si el mutante de ubicación de CT-13 alcanza a S-2.
  - El manejador con `e.Handled = true` (`src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs:756-760`) queda en el dispatcher compartido durante el resto de la corrida del RED.
  - Altera el resultado de pruebas posteriores, y con él la atribución de las pruebas existentes rotas a cada mutante (INV-02).
- **Camino 2: el aviso propio del configurador.** El configurador pregunta en su `OnClosing` por `EditorDiscardPrompt.Confirm(…, this)` cuando `HasUnsavedManualEdits` (`:182-194`, `:210-218`).
  - Dentro de la colección, una sustitución de prueba que conteste «No» cancela el cierre.
  - Una sustitución que cuente añade una pregunta a la cuenta.

**Corrección:** en CT-16:
- capturar `Owner` y `WindowStartupLocation` en variables locales dentro del presentador;
- `try { gesto } finally { window.Close(); }` también dentro;
- suscribirse a `Closed` y aseverar fuera;
- la sustitución contesta «Sí» a la pregunta del configurador, que tiene otro texto, y solo cuenta la de Push Back;
- añadir «el configurador nunca se muestra» como condición de la excepción de INV-F4T1B-06.

### R-I-7. INV-F4T1B-01 y el GREEN se anclan a `BaseSha`, que en una corrección es el GREEN anterior

**Evidencia:**
- **El paquete** (INV-01 :462-463, D-T1B-03 :257-258, `ExpectedEvidence` :517): «`git diff BaseSha..CurrentSha -- src` vacío» y «`--name-only BaseSha..CurrentSha` = exactamente los siete».
- **El precedente:** en el intento 2 de F4-T4A2, `BaseSha` f8ccef46 ≠ `ChainBaseSha` 4eb7e924 (`docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/delegation.json` frente a `…/1/R20261114T144612Z-2d7d/delegation.json`). §16.8 define `ChainBaseSha` aparte (`docs/AUTOMATION_PLAN.md:594`).
- **Dos correcciones que el texto haría imposibles:**
  - una que cambie 2 de las 7 pruebas incumple «exactamente los siete»;
  - una que tenga que revertir un mutante residual del GREEN anterior, que es precisamente un REWORK por INV-01, no puede dejar vacío `BaseSha..CurrentSha -- src`.
- **Casos que la regla no cubre:**
  - §16.8 exige RED también cuando `ChainRedSha` es `null`, es decir, con el RED anterior no acreditado, aunque no se toquen pruebas (`docs/AUTOMATION_PLAN.md:597-598`). La regla solo cubre «toda corrección que toque los siete archivos» (:319);
  - «con los mutantes aplicados otra vez» frente a «Core en `ExpectRed` false» (:321) deja sin decir qué mutantes se reaplican. Si se reaplican todos, Core falla de todos modos. Y `Failed == Selected` en UI exige reaplicar **todos** los de UI, también los de pruebas no cambiadas.

**Corrección:**
- **Anclar las comprobaciones de producto a `ChainBaseSha`:**
  - «`git diff ChainBaseSha..CurrentSha -- src` vacío»;
  - «`--name-only ChainBaseSha..CurrentSha` = los siete»;
  - «`BaseSha..CurrentSha` ⊆ las 21 rutas de Allowed».
- **Redactar la regla de correcciones:** «toda corrección que exija RED según §16.8 (`ChainRedSha` nulo, o que toque `ChainRedFiles`) vuelve a aplicar todos los mutantes de los filtros con `ExpectRed`. Core pasa a `ExpectRed` false solo si no se reaplican los del Plugin, y se declara».

## Menores

### R-M-1. Una fila de CT-20 no compila tal cual

`ShowDialog((Window)owner)` en `UiSupport.cs` (:306) no compila, porque `src/RackCad.UI/UiSupport.cs:1-9` no importa `System.Windows` y la firma usa el nombre completo (`:64`). El resultado sería `build-ui` en `failure` en el RED.

**Corrección:** `(System.Windows.Window)owner`.

### R-M-2. Falta «sin avisos nuevos»

`build-plugin` compila la solución en Release con `-NoWarnings` (`.github/workflows/ci.yml:312`; `eng/ci/verify-autocad-references.ps1:110-111` y `:601`). La solución incluye los dos proyectos de prueba (`RackCad.sln:16`, `:20`), así que un aviso CS en una prueba nueva pone el GREEN en 3/4.

El precedente lo exigía también con el esqueleto del RED (`docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/coordinator/gate-contract.json`, `ExpectedEvidence`).

**Corrección:** añadirlo a INV-12 y a `ExpectedEvidence`, también para los mutantes (sin CS0162 ni CS0219).

### R-M-3. M-17 no está aplicado

El §10 (:596) da M-17 por aplicado, pero la cláusula «las corridas locales, fuera de la sesión de uso del Owner» no aparece en INV-04, INV-07 ni en `ExpectedEvidence`.

### R-M-4. `MinSelected` UI = 22 frente al respaldo de CT-02

El 22 supone que CT-02 tiene un método propio. Con su respaldo («se reduce a CT-03 más la declaración», :338) y un método por CT, que es justo hacia donde empuja el anclaje de R-B-1, salen 21 y Tests falla.

**Corrección:** declarar que con el respaldo CT-02 conserva su método (`CloseButton` como único `IsCancel`, con su mutante), o bajar `MinSelected` a 21.

### R-M-5. CT-18 no tiene fila en la tabla

CT-18 (:354) es opcional, pero si se escribe entra en el filtro de UI y tiene que fallar en el RED. Le falta fila en la tabla.

**Corrección:** darle una, por ejemplo `SaveLibrary_Click` marcando la identidad antes de la guarda de validez, en `.xaml.cs`; o excluirlo del filtro.

### R-M-6. El oráculo del respaldo del shell en CT-15

El mutante previsto, un diccionario en `Shell.Resources`, solo hace fallar un oráculo del tipo «`Shell.Resources.MergedDictionaries` vacío». No haría fallar uno por identidad del tipo «no contiene `ShellResources.Shared`» (`src/RackCad.UI/Shell/RackEditorVisualShell.cs:45-53`).

**Corrección:** fijar el oráculo.

### R-M-7. Valor local frente a valor efectivo en CT-11 (3)

«Ningún valor adjunto» (:347) tiene que referirse al valor **local** (`ReadLocalValue`, o `BaseValueSource.Local`). Los valores efectivos incluyen los estilos por defecto del tema de WPF.

Hay que decir además dónde vive el código de la medición local del foco y de la tabulación (CT-10, CT-11). Si va en los siete archivos, entra en el filtro y tiene que fallar en el RED.

### R-M-8. Lo que `ProcessKey` no recorre

**(WPF)** `ProcessKey` no recorre los manejadores de teclado del elemento con foco. Por ejemplo, un `CatalogCombo` abierto consume Escape (`PostBox`, `.xaml:118`, y los `PieceBox` de las secciones).

**Corrección:** declarar en D-T1B-07 y en `KnownLimitations` que el gesto caracteriza la ruta de la tecla de acceso, no el enrutado desde el foco. Y no usar como oráculo el valor de vuelta de `ProcessKey`.

### R-M-9. El coste de la tabla de mutantes

Medir «las pruebas existentes que rompe» **cada** mutante (:472) exige una suite de UI completa por mutante, unas 30.

**Corrección:** la detección por mutante se mide con los filtros de T1B. Las pruebas existentes rotas se miden como unión, con todos los mutantes aplicados, y se atribuyen por análisis declarado.

### R-M-10. «T1B no modifica la clausura» (:227)

El RED sí la modifica: G1, `EditorModules.cs` de G2, el configurador y el diálogo de lote de G3, G5 y `UiSupport.cs` de G6.

**Corrección:** decir «en neto», para que la regla de orden de F4-E51 no se lea contra la propia T1B.

### R-M-11. Residual de I-12

Si CT-04, o una medición promovida a prueba, pasa en local y falla solo en la CI del GREEN, la única salida es un REWORK, porque el GREEN no puede tocar `RT`.

**Corrección:** declararlo como riesgo aceptado, o dejar CT-04 fuera de la primera entrega y que se incluya en una corrección si se mide estable.

## Lo comprobado y correcto (breve)

**`MutantScope` y la tabla:**
- las 14 rutas de `MutantScope` existen;
- todas las filas de la tabla caen en ellas;
- la suma es 14.

**La clausura:**
- G6 del §3 coincide con `closure-measured.json`, los 17 archivos y sus motivos;
- G1, G2, G3, G4 y G5 cuadran;
- el NOPROP de `EditorModules.cs` está declarado.

**El protocolo:**
- `RT` es solo `tests/` (`docs/AUTOMATION_PLAN.md:594`), así que el GREEN que revierte mutantes no toca `RT ∪ ChainRedFiles`;
- Scope es neto (`:617`);
- nc2 y nc4 no se ven afectados por las 14 rutas de `src/` en Allowed (`docs/automation/agent-execution/README.md:229-234`).

**La CI:**
- `build-plugin` queda omitido en el RED por su `needs`;
- `tests` falla en el RED por los mutantes textuales del Plugin, sin compilar el Plugin;
- el build Release local los compila.

**`StaTestRunner`** relanza con `ExceptionDispatchInfo` (`:97`): el TRX ve la excepción original, sin envoltorio, y eso hace viable el criterio del TRX una vez corregido.

**Las fachadas:** sus caller-owned no tienen llamadores en los cuatro caminos clásicos, y la guarda de CT-27 (2) pasa sobre la base.
