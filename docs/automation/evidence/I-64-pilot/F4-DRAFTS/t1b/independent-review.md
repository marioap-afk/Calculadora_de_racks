# Revisión independiente y adversarial — Paquete del contrato de F4-T1B (v1)

**Objeto:** `I-64-f4-t1b-gate-contract-package.md` v1 (citado como «paquete :línea»), contrastado con el código y las autoridades en `c3ae1c970c908ef541840fc583680b86376cae0b`. Solo lectura: no se compiló, no se ejecutó ninguna prueba y no se tocó git. Todas las rutas son relativas al repositorio.

## Veredicto

**EMITIBLE TRAS CORRECCIONES — 2 bloqueantes / 13 importantes / 20 menores.**

- Los dos bloqueantes tienen una corrección concreta y no necesitan rediseñar la tarea:
  - B-1: a CT-26 le falta un mutante posible;
  - B-2: los campos de alcance prohíben la escritura que el RED exige.
- Tres importantes necesitan una decisión que el paquete no puede tomar solo, o una reformulación que vaya al Owner:
  - I-4: CT-27 frente a T4B;
  - I-5: el §8 dicta el alcance de T4B;
  - I-6: los efectos laterales del configurador.
- Recomendación: antes de emitir, una comprobación focalizada de la v2 sobre:
  - D-T1B-03 (B-1, B-2, I-1, I-2 e I-3);
  - I-6, I-7 e I-8.

**Sobre D-T1B-03 (pregunta 3), en resumen.** El núcleo de los «mutantes revertidos» es compatible con §16.8 y §16.9 (`docs/AUTOMATION_PLAN.md:594-599`, `:617`, `:619`, `:620`):
- `RT` son solo los siete archivos de prueba, y el GREEN toca solo `src/`, así que no toca `RT ∪ ChainRedFiles`;
- Scope se evalúa sobre el diff neto `BaseSha..CurrentSha`;
- el `RedPart` de Ci se cumple si el job Core falla;
- `build-plugin` queda omitido por `needs: [tests, build-ui]` (`.github/workflows/ci.yml:254-258`).

Es honesto respecto a D13 si se declara. El producto neto es idéntico a `BaseSha`, y el GREEN demuestra que las pruebas pasan sobre el producto real. Los mutantes solo demuestran que ninguna prueba es vacía.

Pero tal como está escrito tiene cuatro problemas:
- **no es realizable** para CT-26 (B-1);
- **choca con los campos del contrato** que el Worker y el Controller leen literalmente (B-2);
- **no prevé** que el RED rompa pruebas existentes ni que pueda colgar la CI (I-1, I-2);
- **no dice qué pasa en una corrección** (I-3).

«Failed == Selected» se puede verificar con los conteos del TRX por filtro, igual que hace ya `ci-filter-counts.json`. En cambio, «por aserción» todavía no es un criterio mecánico (I-1).

No veo una alternativa mejor que siga dentro del protocolo. Cualquier fallo del RED que no venga de `src/` estaría en `tests/`, y entonces formaría parte de `RT`, que el GREEN no puede tocar. La dispensa del Owner es la única salida fuera del protocolo, y sería más débil. Propongo mejoras dentro de la misma idea:
- ámbito por archivo exacto (B-1, B-2);
- mutantes textuales de comportamiento idéntico en los archivos compartidos (I-2);
- permiso explícito para que fallen pruebas existentes (I-1);
- regla para las correcciones (I-3);
- opcionalmente, un commit de mutantes separado del commit de pruebas (M-18).

---

## Bloqueantes

### B-1. CT-26 no puede fallar con ningún mutante de `MutantScope`: el RED de Core es irrealizable y la primera delegación acaba en STOP

**Evidencia:**
- paquete :253 (`MutantScope` = G1, G3, G5, `Shell/EditorClosePolicy.cs`, `Editor/EditorModules.cs`, `UiSupport.cs` y `Themes/AppStyles.xaml`);
- paquete :293 (CT-26 lee `case PushBackInsertionRequest` → `RackNewRackName.Resolve` → `RackViewBatchProducts.PushBack` → `RackViewBatchExecution.Run`, y `PlacePushBack(…, regen:false)`);
- paquete :209 (esos archivos se declaran fuera de la clausura);
- paquete :392 (`Failed == Selected` por filtro) y :426 (F4T1B-C04, segunda cláusula).

En el código, todo lo que CT-26 afirma vive en archivos fuera de `MutantScope`:
- `src/RackCad.Plugin/RackMenuCommands.cs:131-143`;
- `src/RackCad.Plugin/Views/RackViewBatchProducts.cs:74-94`;
- `src/RackCad.Plugin/Views/RackViewPlacement.cs:36-39`;
- `RackNewRackName` está en `src/RackCad.Plugin/Systems/Shared/`.

**Problema:**
- No existe ningún mutante «dentro de MutantScope» que haga fallar la prueba de CT-26, y CT-26 es obligatorio, de Core y con `ExpectRed` true.
- `Failed == Selected` en el filtro de Core es imposible, así que el Worker solo puede parar con F4T1B-C04.
- Es un STOP seguro, ya previsto por el propio paquete.

**Corrección:**
- Añadir esos tres archivos a `MutantScope` por ruta exacta, solo con mutantes textuales de comportamiento idéntico (M-18) y revertidos en el GREEN. Siguen fuera de la clausura.
- Exigir en la emisión, y no solo en la entrega, una tabla «método CT → archivo → mutante previsto» que demuestre que cada método obligatorio tiene al menos un mutante dentro del ámbito.

### B-2. Los campos del contrato prohíben las escrituras que el RED exige

**Evidencia:**
- paquete :368 (`AllowedWriteScope` = los siete archivos), :370 (`MutantScope` no es un campo del esquema), :373 (`src/` completo «en el diff neto») y :387;
- `docs/automation/agent-execution/schemas/gate-contract.schema.json:29`: `ForbiddenWriteScope` es una lista de rutas y no admite el calificativo «en el diff neto»;
- el contrato base del Worker escribe literalmente «prohibido <ForbiddenWriteScope>» y «nada fuera del alcance permitido» (`docs/initiatives/PROMPT_TEMPLATES.md:158`, `:164`);
- modo de fallo «Éxito declarado sin commit … `BLOCKED/STOP` si hay escritura fuera de alcance» (`docs/AUTOMATION_PLAN.md:641`).

Hay dos precedentes de esta misma cadena:
- el Worker de F4-T4A1 paró porque el contrato le obligaba a tocar algo fuera de alcance (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:1768` y siguientes);
- el Controller de F4-T4A2 leyó una desviación declarada como prohibida y dio STOP (`f4-dispositions.md:1932-1933`).

**Problema:**
- El RED escribe en `src/`, que el Worker recibe como prohibido y fuera de su lista blanca.
- Que el Worker obedezca el contrato base, o que el Controller lea los commits de `src/` del RED como «escritura fuera de alcance», da un STOP que el paquete mismo provoca.
- La salvedad vive solo en un invariante en prosa, que compite con dos campos estructurados que dicen lo contrario.

**Corrección:**
- `AllowedWriteScope` = los siete archivos de prueba **más** los archivos exactos de `MutantScope`, con la marca «solo mutante temporal, neto cero» en el invariante.
- `ForbiddenWriteScope` por raíces de `src/`, **excluidos** esos archivos, como hizo F4-T4A2 por raíces.
- INV-F4T1B-01 conserva `git diff BaseSha..CurrentSha -- src` vacío. Ese es el control real, y además atrapa un cambio de producto que quedara escondido en un archivo de `MutantScope`. Se añade que `git diff --name-only BaseSha..CurrentSha -- tests/` = los siete archivos.
- Una frase explícita para el Controller: «las escrituras en `src/` del commit RED, dentro de `MutantScope` y revertidas por el GREEN, no son escritura fuera de alcance».
- nc2 y nc4 no se ven afectados, porque trabajan sobre el diff neto.

---

## Importantes

### I-1. El RED romperá pruebas existentes, y el contrato no lo admite ni define «por aserción»

**Evidencia.** Varios hechos que T1B tiene que mutar ya los fija una prueba existente:
- `CenterOwner` (`tests/RackCad.UI.Tests/RichEditorCharacterizationTests.cs:277`);
- «Cerrar» con `IsCancel` (`tests/RackCad.UI.Tests/RichEditorCloseContractTests.cs:48`);
- los tokens locales (`tests/RackCad.UI.Tests/PushBackActionBarRhythmTests.cs:45-47`);
- la ausencia de `IsDefault` (`RichEditorCharacterizationTests.cs:92`).

La plantilla de evidencia de F4-T4A2 dice «las existentes pasan y solo las nuevas o la cambiada fallan, por asercion» (`docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/coordinator/gate-contract.json:380`). El perfil del Worker repite «pruebas que fallan por asercion» (`docs/initiatives/PROMPT_TEMPLATES.md:184`). El paquete no trae `ExpectedEvidence` y dice «por aserción, no por compilación ni por excepción de infraestructura» (:392) sin un criterio mecánico. Tampoco excluye `Skip` en las pruebas nuevas.

**Problema:**
- Si la emisión reutiliza la plantilla de T4A2, el RED la incumple por construcción, con riesgo de REWORK o STOP.
- «Excepción de infraestructura» no es comprobable: un mutante que haga lanzar al producto no cae ni de un lado ni del otro.

**Corrección:**
- Declarar que en el RED **pueden fallar pruebas existentes, solo por los mutantes**, enumeradas en la tabla de mutantes, y que el GREEN las devuelve a verde (INV-F4T1B-12).
- **Criterio mecánico:** en el TRX del RED, cada resultado de los filtros de T1B tiene `outcome = Failed` y una excepción de tipo `Xunit.Sdk.*`, con un mensaje que empieza por `Assert.`. No queda ninguno `NotExecuted` ni `Skipped`. Además, `Failed == Selected == Total` y `Selected ≥ MinSelected`.
- Prohibir `Skip` en las clases nuevas.
- Redactar `ExpectedEvidence` con estas frases.

### I-2. Hay mutantes que compilan y cuelgan la CI; `MutantScope` es demasiado amplio

**Evidencia:**
- `tests/RackCad.UI.Tests/PushBackLibrarySaveGateTests.cs:89-95` llega a `UiSupport.PromptSaveToLibrary`. Confía en que `dialog.ShowDialog(owner)` (`src/RackCad.UI/UiSupport.cs:81`) lance porque el dueño nunca se mostró.
  - Un mutante que quite el dueño, en `RackPushBackSystemWindow.xaml.cs:3458` o en `UiSupport.cs:81`, abre un `SaveFileDialog` real.
- Pasa lo mismo si se retira un `{ Owner = this }` (`RackPushBackSystemWindow.xaml.cs:2865`, `:3047`, `:3438`) o se invierte la comprobación de nulo de una costura (`:2722`, `:3018`). En los dos casos aparece un `ShowDialog` real.
- Si `Shell/EditorClosePolicy.cs:112` se salta la costura, aparece un `MessageBox` real (`tests/RackCad.UI.Tests/StaTestRunner.cs:45-57`).
- `ci.yml:239-244`: `--blame-hang` mata el testhost a los 5 minutos y el TRX queda incompleto, así que el RED no se puede verificar.
- `MutantScope` (paquete :253) incluye archivos que comparten Selectivo y Dinámico: `Shell/EditorClosePolicy.cs`, `UiSupport.cs`, `Themes/AppStyles.xaml` y todo G3.

**Corrección:**
- (a) En los archivos compartidos y en el Plugin, solo **mutantes textuales de comportamiento idéntico**: argumento con nombre, paréntesis o conversión redundantes, o una expresión equivalente. Bastan para romper las guardas de fuente sin cambiar nada observable.
- (b) Mutantes de comportamiento solo en G1 y en `Views/RackViewBatchDialog.xaml`. Nunca sobre `Owner`, `ShowDialog`, `MessageBox`, `EditorDiscardPrompt`, la comprobación de nulo de una costura ni un camino de `Close()` que pueda dejar un modal abierto.
- (c) La CI del RED termina sin `blame-hang` ni timeout; si no, es REWORK.
- (d) El respaldo del shell de CT-15 se puede mutar en el XAML de G1, añadiendo un diccionario a `Shell.Resources`. No hace falta tocar `AppStyles.xaml`.
- Reducir `MutantScope` a los archivos exactos que exige la tabla de B-1.

### I-3. No hay regla RED para una corrección

**Evidencia:**
- `docs/AUTOMATION_PLAN.md:594-599`: exige RED si el diff toca `ChainRedFiles`;
- `:619`: el job Core debe fallar;
- `docs/automation/agent-execution/README.md:199-205`;
- precedente de T4A2: el intento 2 pasó Core a `ExpectRed` false (`f4-dispositions.md:1909`) y aceptó un RED con Core en verde (`:1927`).

El paquete solo describe la primera delegación (:234, :388-394).

**Problema:**
- Toda corrección de T1B toca alguna de las siete pruebas, así que exige otro RED.
- No se sabe si se vuelven a aplicar los mutantes, si sigue valiendo `Failed == Selected`, ni cómo falla Core si solo cambian pruebas de UI.

**Corrección:**
- «Toda corrección que toque los siete archivos lleva un RED nuevo con los mutantes vueltos a aplicar según la tabla. `Failed == Selected` vale para los filtros con `ExpectRed` del contrato de la corrección, y el GREEN es otra vez su inverso exacto.»
- Si solo cambian pruebas de UI, hay dos salidas:
  - Core pasa a `ExpectRed` false de forma declarada en el contrato de la corrección (como en F4-E55);
  - o se vuelven a aplicar los mutantes del Plugin para que el job Core falle.

### I-4. CT-27 congela un hecho que T4B tiene encargado cambiar

**Evidencia:**
- paquete :294 («no hay ningún llamador de producción de los caller-owned de Push Back fuera de las fachadas») y :440 («Las pruebas de T1B deben seguir en verde en T4B»);
- en `docs/initiatives/I-64-f4-t4-gate-contract-package.md`:
  - T4B es dueña de «la transacción atómica, … las fachadas, las unidades» (`:172`);
  - T1B va antes de la primera tarea que «use las fachadas de Push Back para escribir» (`:187`);
  - R-T4-02: las unidades van dentro de la transacción de MUTATE (`:532`);
- D-15, punto 5 (`docs/initiatives/I-64-proposal-v5.md:569`).

**Problema:**
- T4B añadirá el primer llamador de producción de `PrepareRedraw` y `RedrawInTransaction` de Push Back, así que CT-27 se pondrá en rojo por diseño.
- T4B tendría que editar o saltarse una caracterización de T1B, cosa que D13 prohíbe. El propio §8 se contradice.

**Corrección:**
- Limitar CT-27 a lo que protege D-16:
  - las firmas públicas exactas y la existencia de los caller-owned;
  - que los **caminos clásicos** (`RackPushBackCommands.cs`, `KindHandlers/PushBackKindHandler.cs`, `RackMenuCommands.cs` y `Views/RackViewBatchProducts.cs`) no llaman a los caller-owned de Push Back.
- «Sin ningún llamador en producción» queda como hecho del §2 para T4B, no como prueba.

### I-5. El §8 dicta el `ForbiddenWriteScope` de T4B y convierte en material tocar la clausura

**Evidencia:**
- paquete :211, :213 y :440;
- F4-E51: T1B va antes de la primera tarea que **modifique** un archivo de la clausura (`f4-dispositions.md:1731-1734`). La premisa es que T4B podrá tocarla con la caracterización en verde;
- R-A2-06: «si A2 necesitara `Themes/` o `Controls/` …, es STOP: T1B iría antes» (`docs/initiatives/I-64-f4-t4-gate-contract-package.md:524`);
- T4B es dueña de «las fachadas» (`:172`), que son G5 y están dentro de la clausura.

**Problema:**
- T1B decide el contrato de otra tarea y contradice el paquete de F4-T4.
- El argumento para dejar fuera los fragmentos compartidos (:211: «si entraran, T4B no podría tocar nada») descansa sobre esa misma regla inventada.

**Corrección:**
- Sustituir el §8 por: «T1B fija la clausura (§3) y sus pruebas. El paquete de T4B decide su alcance. Si T4B toca un archivo de la clausura, las pruebas de T1B siguen en verde (INV-19). Si una tuviera que cambiar, es un cambio funcional y se declara al Owner (D13).»
- Si la sesión quiere la regla estricta, debe ir como interpretación del §9 para el Owner, señalando que choca con «las fachadas» de T4B.

### I-6. El configurador real deja dos salidas: escribir en el perfil o fugar un manejador del dispatcher compartido

**Evidencia:**
- `src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs`:
  - el constructor se suscribe a `Dispatcher.UnhandledException` (`:73`);
  - solo se desuscribe en `OnClosed`, que además escribe la disposición (`:602-606`);
  - el manejador marca `e.Handled = true` (`:756-760`);
- `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:2426` lo construye con `Owner = this`, y `:2437-2446` es la costura;
- en el paquete: CT-13 S-2 (:280), CT-16 (:283), INV-F4T1B-06 (:406-408) y F4T1B-C03 (:425);
- la suite ya lo cierra, y por tanto ya escribe en `%LOCALAPPDATA%` (`tests/RackCad.UI.Tests/PushBackHeaderAuthorityWindowTests.cs:68-74`).

**Problema:**
- CT-13 (S-2) y CT-16 son obligatorios y construyen el configurador real.
- **Si no se cierra:** el manejador queda en el dispatcher STA de todo el proceso y se traga las excepciones asíncronas de las demás pruebas, lo que oculta regresiones.
- **Si se cierra:** escribe en el perfil, lo que va contra INV-06, y el Worker para con C03.
- Respuesta a la pregunta 4: **no está medido** si cerrar la ventana Push Back cierra una ventana poseída que nunca se mostró, lo que dispararía su `OnClosed`.
  - Win32 destruye al cerrar el dueño solo las ventanas poseídas que tienen HWND.
  - Una ventana nunca mostrada no tiene HWND, y lo que hace WPF con ella no está fijado en el repositorio.

**Corrección.** Decidirlo en el paquete, o llevarlo al Owner en el §9:
- (a) **recomendada:** permitir cerrar el configurador en esas dos pruebas, declarado como la misma escritura que la suite ya hace en cada CI, no como una nueva;
- (b) S-2 y la línea `window.Configuration ?? copy` (`:2446`) por guarda de fuente. CT-16 quedaría solo para la medición de R-8, y se declara que hay ejecución pendiente para F5.

En los dos casos:
- se lee `Owner` y `WindowStartupLocation` al entrar en la costura;
- si se quiere medir lo que hace WPF al cerrar el dueño, se hace con una `Window` sonda privada de la prueba, nunca con el configurador.

### I-7. Gestos de teclado sin medir: riesgo de caracterización falsa y de STOP; Enter con un oráculo ciego

**Evidencia:**
- paquete CT-01 (:268), CT-02 (:269) y CT-03 (:270);
- `tests/RackCad.UI.Tests/EditorWindowTestSupport.cs:110-111`;
- en `tests/` no hay ningún uso de `AccessKeyManager`, `AutomationPeer`, `IInvokeProvider` ni `PushFrame` (búsqueda en el árbol);
- `AGENTS.md:330-333` (oráculo ciego).

Comportamiento de WPF que no está medido en el repositorio:
- **el ámbito de una tecla de acceso** de un elemento es, por defecto, su `PresentationSource`. `AccessKeyManager.ProcessKey(window, "\x1B", false)` puede no encontrar el botón y no hacer nada. `ProcessKey(null, …)` depende de la ventana activa;
- **`ButtonAutomationPeer`** encola el clic con `BeginInvoke` (prioridad Input), así que hace falta bombear el dispatcher;
- **un `KeyDown` elevado con `RaiseEvent`** no pasa por `InputManager`. Nunca puede activar `IsDefault` ni `IsCancel`, de modo que la parte de ejecución de CT-01 no puede fallar si aparece un botón por defecto.

**Problema:**
- Si el gesto no llega al botón, el Worker fijaría «Escape no cierra» (INV-04: «se fija lo que se mida»). Sería una caracterización falsa.
- O pararía con C01, porque no hay una vía pública sin costura nueva.
- CT-01 no prueba nada sobre la ruta real de Enter.

**Corrección:**
- (1) **Control positivo obligatorio:** el mismo gesto, sin trabajo pendiente, cierra la ventana. Si no la cierra, el gesto no es válido; no es un hecho del producto.
- (2) Escape con `ProcessKey(PresentationSource.FromVisual(w), "\x1B", false)` sobre la ventana mostrada, a medir.
- (3) Enter por la misma ruta, con `"\r"`, más la comprobación estructural. La parte con `RaiseEvent(KeyDown)` no cuenta como evidencia de Enter.
- (4) El peer se bombea por prioridad dentro de la colección, sin esperas fijas.
- (5) Un respaldo declarado: si no hay ningún gesto medible, CT-02 se reduce a CT-03 (`OnClick`) más la declaración existente. Va a `KnownLimitations` sin STOP.

### I-8. CT-04 (`ShowDialog`) puede colgar el job de UI

**Evidencia:**
- paquete :271 y :413;
- `ci.yml:222` y `:239-244`;
- cada `Close()` vuelve a preguntar mientras haya trabajo pendiente (`RackPushBackSystemWindow.xaml.cs:3395-3404`);
- `StaTestRunner.cs:78-99`: `Invoke` es síncrono.

**Problema:**
- Un «cierre de respaldo» que llama a `Close()` mientras la sustitución sigue respondiendo «No» lo cancela `OnClosing`.
- `ShowDialog` no vuelve, y a los 5 minutos `blame-hang` mata el testhost.
- Las pruebas que quedaban, entre ellas la colección no paralela, no corren.

**Corrección.** Especificar el respaldo:
- se encola antes de `ShowDialog`, con una prioridad más baja que las acciones programadas (`ContextIdle` o `ApplicationIdle`);
- primero cambia la sustitución a «Sí» y cancela la edición de módulo;
- después llama a `Close()`;
- las aserciones van después, y no se usan temporizadores.

Añadir además a INV-F4T1B-07 que, con la ventana mostrada, no se invoca ningún gesto que abra un modal real sin costura:
- BOM (`:3438`);
- «Insertar varias…» con entradas válidas (`:3311`);
- «Guardar en biblioteca» (`:3458`);
- el configurador sin presentador (`:2443`);
- Seguridad sin costuras.

### I-9. La «secuencia estructural» de CT-11 caracteriza el ayudante, no la tabulación

**Evidencia:**
- paquete :278 («en orden de documento (árbol lógico y slots del shell)»);
- `EditorWindowTestSupport.cs:159-170` recorre los slots en este orden: lateral → matriz → vista previa → estado → acciones;
- la plantilla, en cambio, pone `PART_ActionBar` como primer hijo del `DockPanel`, antes de `PART_StatusHost` y `PART_SidebarScroll` (`src/RackCad.UI/Themes/Generic.xaml:151-174`);
- el propio H-10 deduce que la barra de acciones va primero (paquete :102).

**Problema:** fijar esa secuencia fija el orden del recorrido de la prueba, y no el orden de tabulación de WPF, que sigue el árbol visual. Sería una caracterización falsa en verde.

**Corrección.** La parte obligatoria tiene dos piezas:
- el orden de las paradas de tabulación **dentro de cada slot**, que no depende de la composición;
- el **orden de las zonas** medido en la plantilla aplicada: el índice de `PART_ActionBar`, `PART_StatusHost`, la rejilla lateral y `PART_WorkArea` en el `DockPanel`. No necesita foco.

El recorrido efectivo con `MoveFocus` se fija solo si es estable también en la CI (I-12). OD-9 queda condicionado a esa medición.

### I-10. CT-23 tiene un camino de ejecución, y el paquete lo resuelve por fuente

**Evidencia:**
- `RackPushBackSystemWindow.xaml.cs:3294-3299`: confirma el módulo escenificado, recalcula y, si la entrada es inválida o la salida está bloqueada, vuelve **sin diálogo**. Nunca comprueba `canInsertInAutoCad`;
- paquete :290 («No hay costura que permita ejercerlo») y :414-416 (INV-F4T1B-08).

**Corrección:**
- CT-23 en ejecución, con `new RackPushBackSystemWindow()` (sin AutoCAD), un módulo escenificado y un campo inválido. Al pulsar `InsertBatchButton`:
  - `PendingWork()` queda vacío;
  - no aparece el texto de AutoCAD;
  - `StatusText` muestra el error del campo;
  - no hay `Closed` ni petición.
- La variante con la salida bloqueada debe dar su diagnóstico.
- La guarda de fuente queda solo para el tramo diálogo → `RequestInsertViews` → `Close()`.
- OD-2 pasa a estar medido.

### I-11. Falta un camino de cierre: el texto sin comprometer y las ediciones fuera del ámbito declarado

**Evidencia:**
- `RackPushBackSystemWindow.xaml.cs:3406-3419`: solo cuenta el módulo escenificado;
- `:1207`: los campos comprometen en `LostFocus`;
- ADR-0029 D7: «Escape no puede provocar pérdida silenciosa de cambios» (`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md:121-123`);
- el riesgo R-11 del mapa 1 lo identificó;
- CT-02 (paquete :269) solo cubre «sin trabajo pendiente» y el módulo escenificado.

**Problema:** es la diferencia de comportamiento más relevante para el piloto, porque el texto pendiente es el eje de F4-T3. Además, Escape (que no mueve el foco) y el clic de ratón (que sí lo mueve y compromete) podrían tratar distinto el mismo texto, en contra de «la misma política» de D7.

**Corrección:**
- Un CT obligatorio en `CloseTests`: con un `NumericField` cambiado por `SetNumber` sin `LostFocus`, y con una edición ya aplicada de la matriz:
  - Escape (gesto de I-7) y `Close()` cierran sin preguntar y sin petición;
  - el texto no se compromete (`RecomputePassesForTest` no cambia).
- En D-T1B-06, la parte aplicada es D8, por diseño. Lo del texto de Escape es candidato a deuda D7 si se mide.
- Ajustar `MinSelected`.

### I-12. «Estable» se define solo en local

**Evidencia:**
- paquete :402 («tres corridas locales consecutivas … con el mismo resultado»);
- el criterio del repositorio dice que el foco efectivo es frágil (`tests/RackCad.UI.Tests/RichEditorContractTests.cs:58-59`);
- la CI corre en `windows-latest` (`ci.yml:217`), donde la activación y el foco no están medidos;
- afecta a CT-04, CT-10 (foco efectivo) y CT-11 (recorrido efectivo).

**Problema:** una prueba estable en local y roja en la CI pone en rojo el GREEN, da REWORK y consume intentos.

**Corrección:**
- Estos puntos solo se fijan si pasan también en la CI del GREEN, y en la del RED fallan por aserción.
- En local: «tres corridas consecutivas **en verde**», no «con el mismo resultado».
- Si no se cumple, va a la limitación declarada y queda como candidato a Smoke-2, sin STOP. Se omiten del mínimo, como ya hace el paquete.

### I-13. F4T1B-C05 es una parada demasiado amplia

**Evidencia:**
- paquete :427;
- `docs/AUTOMATION_PLAN.md:619`: una CI roja da REWORK;
- precedente de STOP falso: `f4-dispositions.md:1932-1933`.

**Problema:** cualquier fallo del GREEN fuera de las pruebas nuevas, sea una inestabilidad ajena o una interferencia corregible de las pruebas nuevas (un aviso sin restaurar, ventanas o un manejador fugados), se convierte en STOP y detiene la línea.

**Corrección:**
- **C05 solo si el fallo se reproduce** en una segunda corrida de la CI y no es atribuible a las pruebas nuevas.
- **Una interferencia atribuible** es REWORK por Ci o Tests.
- **Si no se reproduce**, es una reejecución por BLOCKED dentro de los topes de §16.8.

---

## Menores

### M-1. Recuentos y regla de la clausura

**Evidencia** (`git ls-tree`):
- G3 tiene **10** archivos, no 11: los dos del configurador, `SelectiveSafetyWindow.cs`, `SafetyDefensaGridWindow.cs`, `SafetyTopeGridWindow.cs`, los dos de `RackBomWindow`, los dos de `Views/RackViewBatchDialog` y `Views/RackViewBatchDialogPresenter.cs`;
- G6 repite `src/RackCad.UI/Views/**`, que ya está en G3;
- «dependencias directas» deja fuera tres dependencias transitivas de la propia clausura:
  - `src/RackCad.UI/PreviewCanvasPainter.cs`, usado por `Controls/PreviewCanvas.cs`, `Controls/PreviewProjection.cs` y `Preview/EditorPreviewSurface.cs`;
  - `src/RackCad.UI/ObservableObject.cs`, usado por `RackFrames/*`;
  - `src/RackCad.UI/EnumDisplayConverter.cs`, usado por `RackFrames/RackFrameConfiguratorWindow.xaml`.
- F4-T4A2 ya prohibía por nombre `ObservableObject.cs` y `UiSupport.cs`.

**Corrección:** fijar G6 por `ls-tree` ya, no «en la emisión», y declarar la regla: directas más transitivas dentro de `src/RackCad.UI/`.

### M-2. CT-24 cita mal el producto

El código usa `window.InsertionRequest is PushBackInsertionRequest request` (`src/RackCad.Plugin/RackPushBackCommands.cs:50`), no `as`. Una guarda literal fallaría sobre la base. Copiar el texto del producto (INV-09).

### M-3. CT-08

**Evidencia:** el cuarto texto es normalmente el diagnóstico compuesto. «Corrige los datos: no se puede dibujar un modelo bloqueado.» solo cubre «el caso imposible» (`RackPushBackSystemWindow.xaml.cs:3507`, `:3512-3520`).

**Corrección:**
- fijar el diagnóstico del escenario concreto;
- fijar también que las guardas 1 y 2 (`:3325-3335`) vuelven **sin** confirmar el módulo, y que las 3 y 4 lo confirman antes (`:3346-3349`). Se observa con `PendingWork()`.

### M-4. CT-14

Las opciones del lote se construyen dentro de `InsertBatch_Click` (`:3300-3310`) y no se ven sin el diálogo. «Con los argumentos de la ventana» no es realizable sin replicarlos. Reformular: propiedades del tipo con `RackSystemKind.PushBack` y opciones representativas; los argumentos reales quedan en la guarda de CT-23.

### M-5. CT-05

**Corrección:**
- Añadir la variante con la ventana mostrada. Con HWND, el `Close()` de WPF pasa por `WM_CLOSE`, el mismo mensaje que X y Alt+F4 tras `SC_CLOSE` (comportamiento de WPF, a medir). Refuerza I-4 del §9.
- `RackName` es el texto **recortado** (`:1203`).

### M-6. Escrituras del llamador incompletas

**Evidencia:**
- los tres llamadores mutan `System.Name` del objeto que entrega la ventana (`RackPushBackCommands.cs:98` y `:190`; `RackMenuCommands.cs:135`);
- RACKEDITAR prepara el lote (`PrepareAll`) antes del primer `RedrawInPlace` (`RackPushBackCommands.cs:248-260`).

**Corrección:** añadirlo a CT-24, CT-25 y CT-26.

### M-7. Tabla de clases frente a la regla de la colección

La tabla de D-T1B-05 (paquete :305) pone solo `CloseTests` en la colección. La regla (:313) también alcanza a:
- `OwnerTests`, porque CT-13 muestra la ventana;
- `IntentTests`, porque CT-16 la muestra;
- `KeyboardTests`, si mide el foco efectivo.

**Corrección:** decirlo clase por clase.

### M-8. Filtros y saltos de línea

**Corrección:**
- Fijar el espacio de nombres raíz del proyecto (`RackCad.UI.Tests` o `RackCad.Tests`), del que dependen los filtros (:312).
- Las guardas de Core corren en Ubuntu con LF, y en local con `core.autocrlf=true` (`.gitattributes:4`). Normalizar de CRLF a LF, como hace el precedente (`docs/initiatives/I-64-f4-t4a2-gate-contract-package.md:131`).

### M-9. Paradas y controles

**Evidencia:**
- C08 (:430) cita como «regla de F4-T4A2» algo que allí no existe: F4T4A2-C08 es la regla de fidelidad de la planificación con una replanificación (`…/F4-T4A2-PREPARE-UI/coordinator/gate-contract.json:486`), y `config.toml` es P-01 común;
- el paquete solo nombra nc1..nc3 (:366), pero nc4 va en la primera delegación (`docs/automation/agent-execution/README.md:232`; `f4-dispositions.md:1755`).

**Corrección:** escribir C08 con su texto literal y añadir nc4.

### M-10. Falta `ExpectedEvidence`

La de F4-T4A2 exigía un build Release local de la solución con el Plugin incluido, también con el RED. Aquí compilaría los mutantes del Plugin y cerraría el «la CI del RED no los compila» de D-T1B-03. Recomendación: incluirla, con las frases de I-1.

### M-11. OD-2 y OD-3

**Evidencia:** D7 trata de los caminos de cierre, y OD-2 no cierra. El mismo patrón de «confirmar y no salir» existe en las guardas 3 y 4 de `RequestDraw` (`:3346-3363`), por decisión de I-40.

**Corrección:** la regla de OD-2 y OD-3 es D6 (efecto declarado de la acción), no D7, y hay que mencionar que `RequestDraw` hace lo mismo.

### M-12. OD-4 «INCUMPLIMIENTO»

Se observa `owner == null` en la costura (`Shell/EditorClosePolicy.cs:112`). Pero WPF, cuando no recibe dueño, toma la ventana activa (comportamiento del framework, no medido). Calificarlo como «sin dueño WPF explícito; el dueño efectivo, solo en el host».

### M-13. OD-7, OD-8 y OD-9

**Corrección:**
- OD-7 y OD-9 son inferencias: se registran solo si se miden (INV-04).
- OD-8: D9 no habla de accesibilidad. Es una observación, no una regla.

### M-14. D-T1B-01

«S6 y S7 no cierran, en ejecución» (paquete :226) solo se puede ejercer en sus caminos bloqueados, que ya están cubiertos. El camino libre llega a un modal o a una escritura. Declararlo.

### M-15. §1

**Corrección:**
- La fila de F4 tiene la columna «Escrituras authored», no «Mutación del DWG», que es la del paquete de T2 (`docs/initiatives/I-64-f4-gate-contract-package.md:41`, `:44`).
- Omite «Antes de tocar el estado del piloto». Hay que citarla como sustituida por F4-E18 y F4-E51.

### M-16. §8, obligación de F5

Sustituir una guarda de fuente por ejecución debe seguir el patrón de I-39:
- la guarda vieja queda con `Skip = BaseEvidence`, más la evidencia versionada;
- la prueba nueva va en una clase de contrato, sin borrar nada.

### M-17. Efectos locales

Mostrar ventanas en local roba el foco al usuario: las teclas pueden ir a la ventana de prueba y alterar el resultado.

**Corrección:**
- `ShowActivated = false` donde no se mide el foco;
- las tres corridas locales, fuera de la sesión de uso del Owner, y declarado.

### M-18. Forma del RED

**Corrección:**
- Mensaje del commit RED: «RED con mutantes deliberados, revertidos en el GREEN».
- Ese SHA nunca sirve para un smoke ni para la validación del Owner.
- **Opcional:** commit de mutantes, después el commit de pruebas (que es el `RedSha`), después el revert. `RT` y la regla del GREEN se cumplen igual, y la revisión es más clara.

### M-19. INV-F4T1B-04

«Mismo resultado» admite un fallo estable. Debe decir «en verde» (véase I-12).

### M-20. CT-20, S-1

El dueño nulo del aviso ya se observa en ejecución en CT-02. La guarda de fuente de S-1 sobra según INV-08: quitarla, o marcarla como redundante.

---

## Lo comprobado y correcto (breve)

**Hechos del §2:** son exactos en lo esencial.
- **Ventana y sitios:**
  - tamaños 792, 3630 y 911;
  - los 4 sitios de apertura y ninguno más (`git grep`);
  - los constructores y `LoadNew`;
  - el contrato público `:247-256`.
- **Ausencias:**
  - 0 `DialogResult` y 0 manejadores de teclado, `TabIndex`, `IsTabStop`, `KeyboardNavigation` o `.Focus(` en **los 10** archivos de G1;
  - `InsertRequestedRaised` sin suscriptores en `src/`;
  - nada de foco ni de tabulación en `Shell/`, `Themes/` ni `Controls/`.
- **Cierre:**
  - `OnClosing` y `PendingWork` con el texto exacto;
  - `MayClose` → `Confirm(question)` sin dueño, y el configurador con `this` (`:193`);
  - el suelo de `StaTestRunner.cs:57`;
  - `DisableParallelization` en la colección;
  - el clic del ayudante con `RaiseEvent`.
- **Barra y guardas:**
  - `UpdateButtons`, con `InsertBatchButton` fuera;
  - los tooltips;
  - los textos de las guardas;
  - `InsertBatch_Click` sin comprobar `canInsertInAutoCad`.
- **Diálogos y recursos:**
  - los `Owner = this` y sus costuras (`:2410`, `:2700`, `:3028`), con `RackViewBatchDialog` (`CenterOwner`, `IsDefault` e `IsCancel`);
  - los tokens y el respaldo del shell;
  - `PromptSaveToLibrary`, que crea la carpeta antes del diálogo.
- **Escrituras y fachadas:**
  - la escritura en `OnClosed` del configurador;
  - las firmas de las tres fachadas, con sus caller-owned sin llamador en producción (los únicos llamadores son de Selectivo y Dinámico).
- **CI:** `build-plugin` depende de `tests` y de `build-ui`.

**La clausura:**
- G1 = 10, G2 = 43 (17 + 9 + 2 + 12 + 3), G4 = 52 (60 − 8 de `Pilot/`) y G5 = 5 son correctos;
- las rutas declaradas en F4-T2 existen con su nombre.

**Los filtros:** ningún nombre existente contiene `PushBackClassic`, así que hoy seleccionan 0 pruebas. `MinSelected` 21 y 4 cuadran con los CT obligatorios por proyecto.

**Viabilidad sin paquetes:** `AccessKeyManager` y `ButtonAutomationPeer`/`IInvokeProvider` vienen en el marco WPF compartido. Es posible leer `Owner` y `WindowStartupLocation` al entrar en las costuras sin mostrar el subdiálogo, siempre que la ventana Push Back esté mostrada.

**Mecánica del RED:** compatible con §16.8 y §16.9 en su mecánica básica (véase el veredicto). Las alternativas descartadas en D-T1B-03 se descartan con razón.

**Ámbito:** el paquete no inventa alcance en las CT de la ventana y respeta D13 en lo que fija. Los fallos de ámbito están en el §8 (I-5) y en CT-27 (I-4).
