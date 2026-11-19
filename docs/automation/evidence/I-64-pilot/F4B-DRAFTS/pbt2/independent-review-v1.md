# I-64 F4B — Revisión adversarial independiente del paquete v1 del contrato de PB-T2

**Veredicto: NECESITA v2 — 0 BLOQUEANTES / 8 IMPORTANTES / 16 MENORES.**

El diseño cumple la fila de PB-T2 del [OD] y ninguna de mis pruebas lo tumba:
- CLASSIC no cambia;
- WORKSPACE se pide con una factoría tipada, devuelve una copia al `DraftSession` por un puerto, no cierra ni escribe, y no tiene camino a la petición clásica;
- las dos costuras de D-PB-07 dejan en verde S-7, S-9 y CT-22.

Las guardas de T1B y de texto siguen en verde sin sustituir ninguna, por lectura del texto real de `2f5d3dec` (§3).

Pero el paquete no se puede emitir tal cual. Hay ocho defectos que hay que corregir en una v2:
- una guarda de D2 copiada con la lista de autopruebas que la corrección de PB-T1b ya amplió (`a18ba345`);
- el texto de las correcciones (16.3) superado por la regla del Owner F4B-E25, y las `Authorities` sin declarar;
- una UI-STA y una guarda de texto que no cubren todos los gestos ni todo el `partial` de la ventana;
- la «validación única» sin ninguna guarda que discrimine;
- un plan B mal situado y que contradice los invariantes de PB-T2a;
- dos arrastres de PB-T1b (A-7 y A-9) sin decidir;
- una normalización del nombre en «Aplicar» que ensucia el borrador sin ninguna edición;
- Q-PBT2-1, planteada al Owner con una justificación que contradice la lectura de ADR-0029 D8 que está en el código.

**Decisiones para el Owner:**
- las cuatro del paquete son reales y ninguna estaba ya decidida: Q-PBT2-1 (reformulada, I-08), Q-PBT2-2, Q-PBT2-3 (con su opción B completada, M-16) y Q-PBT2-4;
- no encuentro ninguna decisión material nueva;
- se le informa, sin preguntar, de N-PBT2-01 y de los literales exactos (M-03).

---

## §1. Método y límites

**Qué leí:**
- el paquete v1, su registro `pbt2-measurements.md`, los dos guiones (`measure_pbt2.py`, `forbidden_pbt2.py`) y la salida de `forbidden_pbt2.py` en `_work/`;
- el encargo del redactor (`drafting-brief.md`);
- [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`, en `2f5d3dec`) entero;
- [OPB7] (`pbfull/owner-f4b-dpb07-extensiondata-decisions.md`) y su expediente (`pbfull/owner-return-dpb07.md`);
- [OD2] (`owner-d1-d2-decisions.md`, D2 entero) y `f4b-dispositions.md` (F4B-E06, E09, E20 a E22);
- en [P] v3.1: D-PB-01, D-PB-06, D-PB-07, D-PB-08, §4, §5 y §7.3;
- D-15 y D-16 de la Proposal V5, y ADR-0029 D7 y D8;
- AUTOMATION_PLAN §16.3 a §16.10;
- en el borrador v2.8 de PB-T1: D-PBT1-13, D-PBT1-15 «La condición 10», §5.3, §5.4, §6.3 y §10. Ese borrador **cambió mientras lo revisaba**: añadió V28-23 a V28-28 y los ids F4B-E23 a F4B-E26. Me baso en su estado al cierre de esta revisión;
- `pbt1c/anchors-report.md` y la simulación `pbt1c/sim/W-A.cs.txt`;
- la regla del Owner F4B-E25 (`pbt1b2/owner-f4b-correction-unit-change-decision.md`), que cita la v2.8.

**Cómo leí el repositorio:**
- solo por objetos git desde el checkout principal: `git -C <checkout-principal> show|grep|ls-tree|log|diff|rev-parse|cat-file`, sobre `2f5d3dec`, y para contrastar, sobre `a18ba345`, el GREEN de la corrección de PB-T1b, que ya está en el almacén;
- los volcados de lectura (`git show … >`) los escribí solo en `<carpeta-del-paquete>/_rv/`;
- no leí, no listé ni nombré el worktree de la rama;
- no compilé, no ejecuté pruebas y no usé AutoCAD;
- no usé Python, ni con `-c` ni de ningún otro modo.

**Límites:**
- todo es por lectura;
- lo que depende del texto de PB-T1c, que aún no existe, lo juzgo sobre D-PBT1-13 y la simulación del redactor de PB-T1c;
- los recuentos de la CI son los del paquete; no los volví a medir;
- la incidencia `python3 -c "pass"` que declara el redactor no tiene efecto en ninguna medición, y no la cuento como hallazgo.

## §2. El Owner

| Lo que pide | ¿Lo cumple el diseño? | Nota |
|---|---|---|
| [OD], PB-T2: separar aplicar al estado, cerrar y escribir | **Sí.** `ApplyEditsToEditingState`, extraído de `RequestDraw` sin cambiar su comportamiento, es el único paso de «aplicar al estado». «Aplicar al borrador» no cierra. Cerrar sigue en `OnClosing`. La escritura es del llamador clásico o de T4 | La guarda de texto no fija que el paso sea único (I-04) |
| «Workspace mode must return edited state to DraftSession» | **Sí**, por `IPushBackDraftStatePort` → `SetCurrentDraftState`, que copia (DS:345-350) | Un «Aplicar» sin editar puede ensuciar el borrador (I-07) |
| «Classic RACKEDITAR may continue its compatible existing outer workflow» | **Sí.** Constructores, cuatro sitios, Plugin, WX y WC sin cambio. Cada línea nueva de W está detrás de `mode == Workspace` o de una costura nula | — |
| «Avoid two independent implementations of field validation» | **En la ventana, sí, por construcción.** Fuera de ella, el campo de escala del panel ya duplica la regla (HT2-16, anterior a F4B). Q-PBT2-3 es una lectura legítima | Nada impide en la red que vuelva una segunda implementación (I-04) |
| OB-01.1b: explícito, tipado, inmutable, constructores sin cambio | **Sí.** `ForDraft(PushBackDraftEditorLaunch)`, el campo `readonly mode` y el constructor privado encadenado. La reasignación de un `readonly` en otro constructor de la clase es C# válido. El inicializador corre solo en el constructor encadenado | — |
| §4 de [P]: la guarda «sin camino clásico» | **Sí.** `RefuseInWorkspace()` va primero en `RequestDraw` y en `InsertBatch_Click`, los únicos miembros de W que llaman a `session.Request*` (comprobado: W:3354, :3409, :3414; WC no los llama) | R3 solo mira W (I-03) |
| OB-01.2 y OB-01.3 (parte de PB-T2) | **Casi.** La UI-STA (CT-03) pulsa la barra entera, incluidos los botones ocultos, y una edición. No pulsa `FrontalSideBox`, que sigue visible en un compuesto, ni los gestos que abren subdiálogos o tocan módulos | I-03 |
| OB-01.6: rótulo, tooltip y ocultación | **Sí**, en código y solo en WORKSPACE. El rótulo y el tooltip son los que declara OB-01.6 | El tooltip de `FrontalSideBox` queda falso (M-12) |
| OB-06.3: el paso 4 de D-PB-06 en WORKSPACE | **Sí, en el tramo de la ventana** (CT-08, con los oráculos de CT-PBTC-02 y CT-PBTC-05, que comprobé literalmente) | El tramo hasta el DWG no se arrastra a nadie (M-06) |
| OB-08.2: el aviso en los dos modos | **Sí** (CT-09, con las doce líneas literales de CT-PBTC-07, comprobadas) | — |
| [OPB7], pregunta 1: las dos costuras sin romper S-7, S-9 ni CT-22 | **Sí.** El literal de S-7 sigue tras el ramal de la costura. El de S-9 sigue en el segundo término de la condicional, y `IndexOf("UiSupport.PromptSaveToLibrary(this,")` sigue antes de `EnsureId` (T-SG:281-284, :294-298, :324-341) | — |
| [OPB7]: la UI-STA con las cuatro comprobaciones | **Sí.** Sin petición clásica; borrador igual y 0 llamadas hasta «Aplicar»; `id=GUID-PBT2-0001`; `"CampoFuturoPbt2"` en el archivo | `guid-en-archivo=False` no discrimina nada: el archivo nunca lleva el GUID (`RackProject.ForPushBack`). La comprobación que vale es `id=` |
| [OPB7]: el dueño de las dos ventanas hijas lo diseña PB-T2 | **Sí** (D-PBT2-12): la ventana, como en CLASSIC | Contradice la letra de [P] §4 y §5, que hay que enmendar (M-04) |
| [OPB7]: dos salidas a disco aceptadas | **Sí** | Hay una tercera, sin declarar: la disposición del configurador en el perfil (M-05) |
| D2 (F4B-E11): condiciones 2, 4 y 10 y el límite | **Sí en el diseño:** copias, huellas en CT-05 y C17 | La guarda copiada tiene una lista de autopruebas obsoleta (I-01). La condición 3 se podría probar ya en PB-T2a (M-09) |
| F4B-E22: M10 y M14 bien escritos desde el principio; M09, M11, M12 y M15, declarados o arrastrados | **M10 y M14, sí.** M11 y M15, también | **M09 y M12 (A-7 y A-9 de [PT1] §10, «Por decidir al emitir PB-T2») no aparecen** (I-06) |

## §3. Los hechos (anclas comprobadas en `2f5d3dec`)

**Lo que está fuera de la tabla:**
- `git diff 2f5d3dec a18ba345 -- src` sale vacío, así que se sostiene la premisa «el `src/` de `2f5d3dec` es el final de PB-T1b». La corrección solo tocó cinco archivos de prueba, con el mismo número de métodos (16 → 16);
- W, WC y WX tienen 3 700, 936 y 793 líneas.

| Ancla | Qué hay | Resultado |
|---|---|---|
| W:56-57, :63, :82 | `composite` es `readonly` con inicializador; `state => composite.Active` y `moduleOwner => composite.SideA` son **propiedades calculadas** | OK. Es lo que hace segura la reasignación de `editable` (ninguna referencia queda cacheada) |
| W:84, :101, :106-109, :114 | `safetySelections`, `generalPallet`, los cuatro portadores y `advanced` | OK. En la simulación de PB-T1c (`sim/W-A.cs.txt:84-114`) todos son reenvíos calculados |
| W:131-161 | dos constructores públicos; `LoadNew()` en :160 | OK |
| W:248-251, :255-264 | `BuildLibraryProjectForTest`; el contrato público | OK |
| W:270-289, :294-307, :311-324 | las tres cargas, con `Adopt` en :279, :305 y :322 | OK |
| W:385-440 | `LoadFromModel`; `Recompute()` en :439 | OK |
| W:616-723 | `Recompute`: `AllFieldsValid` (:625), `CommitCurrentCell` (:635), `outputIsBlocked` (:711) | OK |
| W:727-755 | `AllFieldsValid` y **21** cajas | OK (contadas) |
| W:833-890 | `ReadInputs` | OK |
| W:1087, :1130, :1198, :1211, :1343, :1420, :1506 | siete manejadores con `AllFieldsValid` | OK |
| W:1106, :1900, :2127; WC:193, :271 | las cinco cajas validadas en su manejador | OK |
| W:1234-1238 y WX:47 | `Name_Changed`, conectado a **`TextChanged`**, hace `SetName(NameBox.Text?.Trim())` | OK. Es dato de I-07 |
| W:1268-1284 | `ShowDimensionViews` | OK |
| W:2466, :2761, :2905, :3056, :3087, :3351, :3478, :3498 | los dueños de los subdiálogos | OK |
| W:1533, :1539, :1545, :1565, :2450, :2740, :3068 | las costuras de los subdiálogos | OK |
| W:3264, :3527 | `FrontalSideForTest`, `OutputIsBlockedForTest` | OK |
| W:3307-3330 | `Restore_Click`, con su guarda en :3309-3313 | OK |
| W:3332 | `Update_Click => RequestDraw(null, -1, updateOnly: true)` | OK |
| W:3334-3361 | `InsertBatch_Click`: confirma en :3336, el diálogo en :3351, `RequestInsertViews` en :3354, `Close()` en :3360 | OK |
| W:3363-3419 | `RequestDraw`: guardas en :3365 y :3371; confirma en :3386-3389; recalcula en :3391; validez en :3392-3396; compuerta en :3400-3403; nombre en :3405; `RequestUpdate` en :3409; `RequestInsert` en :3414; `Close()` en :3418 | OK |
| W:3421, :3435-3444, :3457-3459 | `Close_Click`, `OnClosing`, `PendingWork` | OK |
| W:3451-3455 | el comentario «comparar la sesion entera contra su apertura, que es el dirty global artificial que ADR-0029 D8 no quiere» | OK. Es dato de I-08 |
| W:3463-3479, :3481-3515 | `Bom_Click` (S-7 en :3478); `SaveLibrary_Click` (S-9 en :3498; `EnsureId` en :3503 a `Save` en :3508; «Saving never flags an insert» en :3505-3506) | OK |
| W:3552-3561, :3563-3610 | `OutputBlockedAfterRecompute`; `UpdateButtons`, que no toca `InsertBatchButton` | OK |
| WC:43, :46-58, :65-71, :85-96, :103-122 | la costura `CompositeState`, `InitializeCompositeSection`, `UpdateCompositeEnabled`, el aviso y `UpdateFrontalButtons` (solo `FrontalSideBox`) | OK |
| **WC:738-746** | `RefreshCompositePanel` | **DIFF menor:** el método sigue más allá de :746. También siembra `CentralSeparatorCheck`, `StructureOverrideBox`, `FondosFrenteLabel` y el texto de la estructura propuesta |
| WC:783-807, :813-934 (`UpdateCompositeEnabled` en :933) | el criterio de bloqueo y `LoadCompositeFromDesign` | OK |
| WC:445, :602-639; W:489 | `LoadSelectedFront` → `LoadCompositeCellPanel` → `ApplyMixedSideState`: con «Ambos lados», una caja mixta se pinta vacía | OK. Es dato de M-01 |
| WX:68-69, :750-790 | `AnnotationScaleBox` con `Minimum="0" MinimumInclusive="False"`; la barra; `FrontalSideBox` `Collapsed` por defecto; `CloseButton IsCancel` | OK |
| SES:106, :121, :124, :138, :145-170, :176-177, :179-195 | la sesión | OK |
| RID:36-39, :45-53, :61-65; ECP:16, :21-29, :33-34, :54, :57-72, :108 | la identidad y la política de cierre | OK |
| RPC:47-48, :50, :57, :98, :168-170, :172, :178-179, :186-187, :190, :193, :254; EM:114, :121, :131; RMC:131-143, :135 | los cuatro sitios y su lectura de la petición | OK |
| PES:40-52, :55, :95, :109-126, :130-169, :367-405, :454-465 | el estado de PB-T1b | OK. `AdoptCarriers` normaliza, entre otros, `DimensionViewsTouched = false` (:157) y la seguridad (:164-168); `BuildInputs` filtra con `AuthorizedSafety` (:399) |
| PESC:33-43, :63-136, :140-141, :403-441 (`Name` en :429) | el comparador, que **compara `Name`**, y el copiador, que «supone un destino fresco» | OK. Datos de I-05 e I-07 |
| POG:16-40 | `Diagnostics`, `IsBlocked`, `StatusOr` | OK |
| DS:44, :239, :284, :287, :298, :331-332, :345-350, :773; DR:143-167 (:145), :204 | el núcleo: `SetCurrentDraftState` copia y da `NotEditable` si no puede editar; `Begin` es público; `Discard` existe | OK. `DraftEditStatus` tiene solo `Updated` y `NotEditable` (DS:150-156) |
| `PushBackPilotFieldAdapter.cs:27`, `:29`, `:31`, `:35-53`; `NumericFieldValidation.cs:63`, `:64`, `:76`, `:102`, `:130-131` | las dos reglas de la caja de escala | OK (el texto «> X» está en :131 y su llamada, en :113) |
| `WorkspaceHostCore.cs:347-352`; PM:106, :108, :112, :126; `PushBackPilotIntentReader.cs:56`; `WorkspacePanelView.xaml:239`, `:241` | el piloto hoy | OK |
| `UiSupport.cs:64-82` (:66-68, :81); `RackProjectStore.cs:60`, `:71-95` | el diálogo de guardado crea la carpeta; I-11 solo en el nivel del proyecto | OK |
| T-SG :169-176, :217-231, :236-248, :251-264, :269-298, :303-319, :324-341, :344-352, :357-379 | la clausura y CT-16, CT-19, S-5 a S-9, CT-21, CT-22, CT-22b y CT-23, leídos con su `Body()` y su `Normalize()` | **OK: todas siguen en verde** con el diseño (detalle en §5) |
| T-PG :198-207 | CT-24 con `Lacks("Owner =")` en RACKPUSHBACK | OK (el Plugin no cambia) |
| **G-10** (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:725-767`) | `.RequestUpdate(` sobre **todo el producto** (`Src.AllProduct()`), con una lista cerrada de permitidos: el controlador, la compuerta, cuatro ventanas y el modelo de vista del campo | **DIFF de descripción:** el paquete dice «ninguna en otro archivo de `src/RackCad.UI`». También cubre Application: los archivos de PB-T2a no pueden contenerla (INV-PBT2A-05 ya lo prohíbe) |
| G9 :703-719 | solo el producto que nombra el piloto | OK. La fila del §4, «los archivos nuevos no nombran el piloto», es falsa para la clase de CT-13, que es una prueba y G9 no mira |
| I-55 :8-15; Fact3 :381-401; desviador :311-318 | literales de W | OK (el diseño no añade `forceRebuild` ni toca esos literales) |
| `PushBackRichPreviewTests.cs:349-367` | métodos `NonPublic` y tipos de campo | OK con los nombres nuevos |
| `WindowCensusGuardTests` | ningún `Window` nuevo | OK |
| WLC :322-333; RAM :83-92; `WorkspaceDraftBoundaryTests.cs:193`, `:204`; `WorkspaceDraftPilotIntentGuardTests.cs:232-244`; `NamespaceFolderGuardTests` | las restricciones | OK |
| `WorkspaceEditIntentHostTests.cs:1030-1036`; `WorkspaceLifecycleHostTests.cs:1477-1480` | los portadores del puerto del panel no exponen tipos de `Drafts` ni de `Systems`, salvo la excepción declarada de `TryUpdatePilot` (`:1039-1046`) | OK. Es dato de M-15 |
| `WorkspaceLifecycleGuardTests.cs:509-520`; `WorkspaceHostCore.cs:66`, `:832` | «toda operación que cambia un borrador acaba la marca de *known clean*» (`NotePilotEdit`) | OK. Es dato de M-15 |
| `RichEditorCloseContractTests.cs:69-86` (:77) | solo Push Back y la cabecera declaran `OnClosing`; «no se inventa un dirty global que el producto no tiene» | OK. El diseño mantiene `OnClosing`; es dato de I-08 |
| `PushBackLibrarySaveGateTests.cs:95`; `DialogWindowCharacterizationTests`; `EditorDiscardPromptCollection.cs:27-30` | la señal por excepción del diálogo de guardado; la ventana del BOM; la colección sin paralelismo | OK |
| **CT-PBT1B-14 corregida** (`a18ba345:tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs:238-253`) | **quince** autopruebas, con `k1` y `k2` («alias + molde») | **DIFF frente al paquete,** que nombra trece y dice que la versión cerrada «hoy no existe» (I-01) |
| `PushBackEditableStateCopyTests.cs:272`, `:1039` | `AllObjects` y `Fingerprint` | **DIFF [T1b-i2]:** en `a18ba345` están en `:278` y `:1080`, y `AllObjects` gana un parámetro `skip` (M-10) |
| `ci.yml:213-241`, `:254-258`; AUTOMATION_PLAN §16.9, n.º 9 | `ui-tests` con un tope de 25 minutos y `--blame-hang`; `build-plugin` con `needs: [tests, build-ui]`; el RED exige el job Core en `failure` | OK. Los dos RED tienen Core en `failure`: PB-T2a por CT-01 a 03 y PB-T2b por CT-11 |

**¿Siguen en verde todas las guardas de T1B sin sustituir ninguna? Sí, por lectura.**
- **CT-19:** ningún token nuevo, ni en W ni en `PushBackEditorMode.cs`.
- **S-7 y S-9:** sus subcadenas siguen, la de S-7 tras el ramal y la de S-9 en la condicional.
- **CT-22:** el `IndexOf` del prompt sigue antes de `EnsureId`.
- **CT-23:** la confirmación sigue antes del diálogo; `LastIndexOf("Close();")` sigue después de `RequestInsertViews`; y el cuerpo no contiene `canInsertInAutoCad`, porque la guarda es `RefuseInWorkspace()`.
- **T-CL y T-IN:** CLASSIC conserva los textos, el orden y el efecto. `PendingWork()` da `None` cuando no hay nada pendiente.
- **`RichEditorCloseContractTests`:** `OnClosing` sigue declarado.
- **G-10:** una sola llamada en W y ninguna en los archivos nuevos.

Las guardas de PB-T1c (CT-PBT1C-01 a 04) se vuelven a comprobar sobre su texto real, como dice el paquete. Por la letra de la v2.8, ninguna impide reasignar `editable` (CT-PBT1C-03 solo exige `EditableStateForTest => editable;` y los reenvíos sin almacenamiento).

## §4. La división

- **PB-T2a se puede entregar sola.**
  - Son dos archivos de Application, con un esqueleto inerte.
  - Tres CT fallan en el RED por aserción; la guarda pasa.
  - Su CI cumple §16.9: Core en `failure`, `ui-tests` y `build-ui` en `success`, `build-plugin` omitido y declarado.
  - No depende técnicamente de PB-T1c. El orden PB-T1c → PB-T2a viene de la regla de una sola delegación abierta (R-PB-09), y es correcto.
- **PB-T2b se puede entregar, con dos reservas:**
  - CT-10 no tiene líneas esperadas hasta la remedición sobre PB-T1c. Es aceptable, porque la sesión las fija antes de emitir y C18 lo cubre;
  - el tamaño frente al tope de 60 minutos del Worker (§16.4) no se ha valorado (M-13).
- **El plan B de R-PBT2-09 no es viable tal como está escrito (I-05):**
  - se decide en los anclajes de PB-T2b, cuando PB-T2a ya está cerrada, pero cambia PB-T2a;
  - añade `CopyInto` a `PushBackEditableStateComparer.cs`, un archivo de PB-T1b que INV-PBT2A-01 y D-PBT2-15 prohíben tocar;
  - copiar sobre un estado ya usado choca con el copiador, que «supone un destino fresco» (PESC:140-141), y necesitaría limpiar la marca de consumo de la cola y la autoridad de módulos, quizá con API nueva en CS o ES;
  - y le harían falta su propio censo y sus propias pruebas de alias (D2, condiciones 2 y 9).

  **Por lectura, el plan B no hará falta.** La simulación de PB-T1c deja `editable` como `readonly` con inicializador, y todo lo que cuelga de él es calculado (§3, W:56-114). Ninguna sección de seguridad, botas o topes cachea el estado: se construyen al abrir su diálogo (W:2575-2651). Y no hay suscripciones a eventos del estado.

## §5. Las pruebas

**Discriminación, CT por CT:**

| CT | ¿Discrimina? | Nota |
|---|---|---|
| A-01 | Sí, contra el esqueleto | `mutar-tras-aplicar` y `volver` prueban más el copiador del núcleo que el puerto. Bien igual |
| A-02 | Sí | Los tipos y los mensajes de excepción de `rackId` y de `ForDraft(null)` no están fijados: el Worker tendría latitud (M-07) |
| A-03 | Sí contra el esqueleto, aunque sobre todo vuelve a probar `Copy` | Le faltan las huellas de D2 alrededor de `ApplyToDraft` (M-09) |
| A-04 | Por sus autopruebas | La lista es obsoleta (I-01) |
| B-01 | Sí | — |
| B-02 | Sí: es el punto fijo de la carga | Le faltan filas de reapertura con una selección no por defecto (M-01) y la normalización del nombre (I-07). Incluye `pb-21-cola`, que está fuera de la garantía (M-02) |
| B-03 | Sí, para la barra y para las cuatro comprobaciones de [OPB7] | No cubre todos los gestos visibles (I-03). `guid-en-archivo` no discrimina (§2) |
| B-04 | Sí | — |
| B-05 | Discrimina el resultado, no la implementación | Una segunda validación equivalente pasaría (I-04) |
| B-06 | Sí | El `finally` tiene que contestar «Sí» antes de cerrar, como `CloseQuietly` de T-CL (`:126`) (M-08) |
| B-07 | En parte por construcción | `dueno-es-la-ventana` es el argumento que la costura recibe. Lo completan S-7 y S-9 |
| B-08 y B-09 | Sí | Sus oráculos, CT-PBTC-02, 05 y 07, coinciden literalmente con el código de `2f5d3dec` |
| B-10 | Sin definir hasta PB-T1c | Declarado |
| B-11 | Sí, con autopruebas | R3 solo mira W (I-03). No hay ninguna regla de validación única (I-04). En el RED, un miembro ausente tiene que dar `False`, no una excepción (M-08) |
| B-12 | Formal: un `enum` no puede mutar nada | M-09 |
| B-13 | Sí, para una divergencia entre las dos reglas existentes | Los parámetros (`false, false, 0.0, null, false, true`) coinciden con `NumericFieldValidation.Validate` (:76) y con WX:68-69 |

**El RED y la CI:**
- los dos RED se pueden cumplir con `ci.yml`, porque Core falla en los dos (§16.9, n.º 9). Que `build-plugin` salga omitido está escrito bien desde el principio (M14);
- en PB-T2b, la sonda del modo hace que ninguna prueba de UI haga un gesto sobre la ventana CLASSIC que devuelve la `ForDraft` inerte.

**Modales, perfil y AutoCAD:**
- las ventanas no se muestran;
- el BOM y el diálogo de guardado van por costura, y el aviso de descarte, sustituido;
- la única escritura es `ArchivoTemporal`;
- no hay AutoCAD;
- la rejilla de botas por poste no se puede ejercer sin mostrar la ventana (`S0.AbrirFueraDePantalla`, CT-PB0-14), y hay que declararlo (I-03).

## §6. Las preguntas al Owner

- **Q-PBT2-1. Material, real y sin decidir.**
  - [OD] pide separar; no dice si «Aplicar» cierra ni si «Cerrar» avisa.
  - D-15 (2) deja la reacción a la intención en manos del host.
  - **Está mal planteada** (I-08).
- **Q-PBT2-2. Material y real.**
  - El «blocking authored write» de [OD], en la fila de PB-T3, habla de la escritura, no del borrador.
  - Las opciones están bien descritas.
- **Q-PBT2-3. Es una lectura de una frase del Owner, y preguntarla es prudente.**
  - La opción (B) omite que toca `Pilot/` y sus guardas, que D-PBT2-15 deja para PB-T4 (M-16).
- **Q-PBT2-4. Procedimiento, y hace falta.**
  - F4B-E09 (pregunta 4) solo cubre PB-TC2 y PB-T1.
- **¿Alguna estaba ya decidida?** No: ni en [OD] (D-PB-01 y la fila de PB-T2), ni en [OPB7] (que excluye expresamente el dueño, los rótulos y los subdiálogos de edición, §5 del expediente).
- **¿Falta alguna?** Ninguna material:
  - **los 11 literales:** [OD] solo permite declarar el rótulo; los demás son textos de estado corrientes, y la pregunta de cierre pertenece a Q-PBT2-1. Basta con dárselos al Owner literales (M-03);
  - **N-PBT2-01:** no cambia la salida y es lo mismo que reabrir con RACKEDITAR. Pero hay un precedente (F4B-E20, la normalización de `pb-15`): se informa (M-03);
  - **el dueño de los subdiálogos:** es una lectura correcta de D-15 (4), escrito para una superficie alojada; solo hay que enmendar [P] (M-04);
  - **la normalización del nombre:** hay que corregirla, no preguntarla (I-07).

## §7. Los contratos

**Lo que es mecánico y se puede cumplir:**
- **`AllowedWriteScope`:** sí.
- **`ForbiddenWriteScope`:** sí. Los 87 y 60 de `src/` salen del algoritmo `cover()`, que revisé en `forbidden_pbt2.py`: sin rutas sin cubrir y sin nuevas que ya existan.
- **Los invariantes:** sí.
- **La lista cerrada de INV-PBT2B-03:** nombra todos los ayudantes privados que usa el código del paquete (`ApplyModeChrome`, `RefuseInWorkspace`, `ApplyEditsToEditingState`, `ApplyToDraft`, `HasUnappliedDraftChanges`, los cinco campos y las once constantes). No hacen falta `using` nuevos: W ya importa `RackCad.Application.Systems.PushBack`, y `BillOfMaterials` va cualificado. **La lección M10 está bien aplicada.**
- **INV-PBT2A-03:** suficiente.

**Lo que no se ajusta a AUTOMATION_PLAN §16:**
- falta la colección `Authorities`, con su ruta, sección y clase (§16.5), y falta decir qué rutas son `UNIT_CHANGE`;
- el texto sobre las correcciones (§16.3) está superado por F4B-E25 (I-02);
- el tope de 60 minutos del Worker (§16.4) no se valora (M-13).

**El tamaño:** cabe con F4B-E21, porque las ocho colecciones opacas, `Objective` incluida, las pone la sesión.

**`RequiredTests`:** tres filtros de PB-T2b quedan «medidos en la emisión» (M-11).

## §8. Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **I-01** | IMPORTANTE | CT-PBT2A-04 y CT-PBT2B-12 cierran la lista de autopruebas de G-L1 en **trece**: (a) a (j), sin la (d). La corrección de PB-T1b añadió (k1) y (k2), «alias + molde», precisamente para discriminar I01. La v2.8 las da por comunes: quince. Copiar «la versión cerrada» y a la vez exigir trece es contradictorio, y sin (k1) y (k2) la regla 1b sobre los alias no se autoprueba: es el mismo modo de fallo que PBT1B-IR-I01. El §9 y el registro P dicen que esa versión «hoy no existe», y ya existe | `a18ba345:tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs:238-253`; v2.8, D-PBT1-15 «La condición 10» («Comunes a las dos guardas, quince») y V28-20; paquete §5.2, CT-04, y §5.3, CT-12 | Nombrar las quince, con `autoprueba-k1=falla` y `autoprueba-k2=falla`. Copiar las reglas y `Rebind` (V28-27) de `a18ba345` o de su cierre (F4B-E26). Actualizar §9, el registro P y el ancla de `Fingerprint` y `AllObjects` (M-10) |
| **I-02** | IMPORTANTE | El §6.0 (lección de PB-T1b) propone que la delegación de corrección lleve «una `AuthorityRevision` que contiene los commits de documentación de la sesión». Eso choca con A6, que exige la `AuthorityRevision` del contrato salvo que se reemita, y está superado por la regla del Owner **F4B-E25**: una ruta `UNIT_CHANGE` cambiada solo por commits RED o GREEN de un Worker de la misma cadena, dentro de su alcance y sin commits de la sesión, no incumple 16.3; las `EXTERNAL`, nunca. Además, los contratos no declaran `Authorities` por ruta, sección y clase (§16.5), ni qué rutas son `UNIT_CHANGE`. PB-T1a y PB-T1c declaran W y WC como `UNIT_CHANGE`. El §1 tampoco tiene F4B-E23 a E26 ([OPB7] = E23 y E24) | AUTOMATION_PLAN §16.3, §16.5 (A6); `pbt1b2/owner-f4b-correction-unit-change-decision.md`; v2.8, V28-22 a V28-25 y §6.3 («W y WC son `UNIT_CHANGE`… F4B-E25»); paquete §6.0 y §1 | Quitar la «lección» y citar F4B-E25 como autoridad en §1 y §6.0. Declarar en cada contrato las `Authorities` con su clase: en PB-T2b, W como `UNIT_CHANGE`, como en PB-T1c; en PB-T2a, decir si los dos archivos nuevos lo son. Referir [OPB7] como F4B-E23 y F4B-E24 |
| **I-03** | IMPORTANTE | OB-01.3 pide demostrar que «ningún gesto» de WORKSPACE anota una petición clásica. CT-03 pulsa la barra y una caja. No pulsa `FrontalSideBox`, visible en un compuesto, ni los gestos que abren subdiálogos (seguridad, defensa, topes, configurador) o tocan módulos. El complemento estático, R3, solo mira W, aunque WC es el mismo `partial` y está en la clausura. El paquete no explica por qué R3 más CT-03 cubren el resto | OB-01.3 de [P] (:172); paquete D-PBT2-08 y CT-11 R3; WC:227 (WC solo usa `session.Recompute`); `S0.cs:136-154` (las botas por poste exigen la ventana mostrada) | Extender R3 a todos los `.cs` de `src/RackCad.UI/Systems/PushBack` (como `ClassicWindowSources`), con su autoprueba en WC. Añadir a CT-03 una pasada sobre `pb-20-ab` con `FrontalSideBox` y los gestos que ya tienen costura (`SafetyDialog`, `DefenseDialog`, `RearTopeDialog`, `HeaderConfiguratorDialog`) y la confirmación y cancelación de módulos. Declarar las botas por poste como no ejercibles sin mostrar, cubiertas por R3 |
| **I-04** | IMPORTANTE | La frase del Owner «Avoid two independent implementations of field validation» no tiene ninguna guarda que discrimine. CT-05 compara resultados: una segunda validación equivalente en `ApplyToDraft` pasaría. R4 no exige que `ApplyToDraft` llame a `ApplyEditsToEditingState`, y nada fija que `RequestDraw` lo siga llamando. La trazabilidad lo da por cubierto con CT-05 y CT-13 | Paquete D-PBT2-11, §5.5 y CT-11 R1 a R10 | Añadir **R11** a CT-11, con su autoprueba: `ApplyEditsToEditingState(` aparece exactamente en `RequestDraw` y en `ApplyToDraft`, y ni `ApplyToDraft` ni `HasUnappliedDraftChanges` contienen `AllFieldsValid(`, `OutputBlockedAfterRecompute(` ni `currentInputsAreValid` |
| **I-05** | IMPORTANTE | El plan B de R-PBT2-09 no es viable tal como está escrito (§4): se decide en los anclajes de PB-T2b pero cambia PB-T2a, ya cerrada; toca un archivo de PB-T1b que INV-PBT2A-01 y D-PBT2-15 prohíben; y la copia en sitio necesita limpiar la marca de consumo de la cola y la autoridad, contra lo que supone el copiador | PESC:138-155 («The target is a fresh state»); paquete R-PBT2-09, PBT2B-C18, INV-PBT2A-01 y §6.0 «Anclajes» | Pasar la comprobación «ninguna guarda de PB-T1c impide reasignar `editable`» a los **anclajes de PB-T2a** (PB-T1c ya está cerrada al emitirla). Si falla, STOP y una v2 con una subtarea propia y su censo, no «plan B dentro de PB-T2a». O quitar el plan B y dejar solo la parada |
| **I-06** | IMPORTANTE | A-7 (M09: `Ownership.Of` decide por la tabla) y A-9 (M12: `SameAs` y `NaN`) del §10 de [PT1] están «Por decidir al emitir PB-T2». F4B-E22 manda declarar o arrastrar los menores de producto M09, M11, M12 y M15. El paquete solo trata M11 y M15 (§8.1, HT2-26, R-PBT2-11) | v2.8, §10, A-7 y A-9; F4B-E22 (`owner-pbt1b-rework-decisions.md`, pregunta 1) | Decidir cada uno en §8.1 y §10: declararlo como límite o arrastrarlo a PB-T3 (A-7, por el mapa de D-PB-05) y a PB-T4 (A-9, por `IsDirty`), con su ancla en `a18ba345` |
| **I-07** | IMPORTANTE | `ApplyToDraft` hace `SetName(NameBox.Text?.Trim())` y `SyncName()` antes de comparar. El comparador compara `Name` (PESC:429). Si el borrador trae un nombre nulo o con espacios en los extremos (racks antiguos: AUTH-15 admite nombres en blanco), un «Aplicar» sin ninguna edición cambia `null` → `""` o recorta el nombre, llama al puerto y **ensucia el borrador** (`IsDirty`, DS:298). Contradice «abrir y aplicar sin editar deja S exacto» (D-PBT2-06 (2)). No está en N-PBT2-01, y CT-02 y CT-04 usan fixtures con nombre. El `SetName` sobra: `Name_Changed` va en `TextChanged` y ya recorta y sincroniza cada cambio de la caja | W:1234-1238; WX:47; PESC:429; paquete D-PBT2-06 (2) y N-PBT2-01 | Quitar `SetName` y `SyncName` de `ApplyToDraft` (y de R4), o declararlo como N-PBT2-01 (c). En los dos casos, una fila de CT-02 y CT-04 con un nombre nulo y otra con un nombre con espacios, con `llamadas=0` tras «Aplicar» sin editar |
| **I-08** | IMPORTANTE | Q-PBT2-1 está mal planteada en tres puntos. (1) «Por qué (A)» dice que el ámbito nuevo «no es un dirty global». La lectura de ADR-0029 D8 que está en el código dice lo contrario: comparar toda la sesión con su apertura «es el dirty global artificial que ADR-0029 D8 no quiere», y el contrato de las ventanas ricas, que «no se inventa un dirty global». (A) introduce, solo en WORKSPACE, el primer dirty de todo el estado de este editor. Puede ser legítimo con una frontera explícita, pero el Owner tiene que verlo. (2) La descripción de (A), «Si al cerrar queda algo sin aplicar, pregunta», omite L-PBT2-10: un texto escrito sin salir de la caja se pierde sin preguntar con Escape, Alt+F4 o la X, aunque no con un clic en «Cerrar». (3) No da el literal de la pregunta nueva | W:3451-3455; `RichEditorCloseContractTests.cs:77`; ADR-0029 D7 y D8; paquete Q-PBT2-1 y L-PBT2-10 | Reescribir el «por qué» con la tensión con D8 dicha a las claras. Añadir a (A) la excepción de la caja sin confirmar, y a (B) y (C), que con un paso explícito de aplicar Escape perdería cambios sin avisar (D7). Poner el literal de `WorkspaceUnappliedQuestion` |
| M-01 | MENOR | El punto fijo de la carga se razona solo sobre los portadores. CT-02 carga estados con la selección por defecto. La reapertura real de PB-T4 trae la selección del borrador: «Ambos lados» con celdas mixtas, el lado B activo, otra celda o una selección múltiple. El primer `Recompute` hace `CommitCurrentCell` sobre esa selección. Por lectura queda idempotente, porque `ApplyMixedSideState` pinta vacías las cajas mixtas, pero no se prueba | W:635, :774-788, :489; WC:445, :602-639; PESC:63 («with the same selection») | Añadir a CT-02 filas con `pb-20-ab` en «Ambos lados» y una celda distinta en A y B, con el lado B activo, y con una celda ≠ (0,0) |
| M-02 | MENOR | CT-02 incluye `pb-21-cola`, fuera de la garantía (F4B-E09, pregunta 3; D-PBT1-10) y que el Workspace rechaza en todo F4B. Aun así, PBT2B-C15 mandaría al Owner cualquier diferencia en ese diseño | F4B-E09; paquete CT-02 y PBT2B-C15 | Excluirlo de CT-02 o fijar su línea, y exceptuarlo de C15 |
| M-03 | MENOR | L-PBT2-7 extiende el permiso de [OD], que es para el rótulo, a otros diez textos. N-PBT2-01 es una normalización conocida de la carga, y su precedente (F4B-E20) pasó por el Owner | [OD]:47-49; F4B-E20 | No es una pregunta: en la vuelta de Q-PBT2-1 a 4, informar de N-PBT2-01 y dar el anexo con los once literales exactos |
| M-04 | MENOR | D-PBT2-12 (dueño = la ventana en los dos modos) contradice la letra de [P] §4 («El que decide el host») y §5. La lectura es correcta: D-15 (4) era para una superficie alojada, y [OPB7] delega el diseño | [P]:488, :504; Proposal V5, D-15 (4) | Añadir la enmienda de [P] §4 y §5 a A2-10 |
| M-05 | MENOR | En WORKSPACE, abrir el configurador de cabecera escribe su disposición en el perfil al cerrarse. Es una tercera salida a disco, no aceptada por [OPB7] (su §5: «no se pregunta aquí»), y D-PBT2-10 no la declara | H-PB-33; T-SG:344-352 (CT-22b); `owner-return-dpb07.md` §5 | Declararla como L-PBT2-11 (preferencia de pantalla, no dato del rack) |
| M-06 | MENOR | OB-06.3 queda cerrada solo en el tramo de la ventana. Probar en el Workspace que el valor llega al DWG (PREPARE → MUTATE → POST) no se arrastra a nadie | [OD]:144-147; [P] OB-06.3 | Arrastre a PB-T3, PB-T4 y Smoke-PB |
| M-07 | MENOR | No están fijados el tipo y el mensaje de las excepciones: `rackId` nulo, vacío o en blanco, `state` y `port` nulos (CT-A-02), y `ForDraft(null)` (CT-B-01 (iv)). En el RED, leer `LaunchFor(...)`, que da nulo, tiene que ir dentro del envoltorio «EXCEPCION» | DR:145 («A rack id is required.») como modelo | Fijar las sentencias exactas (por ejemplo, `new ArgumentException("A rack id is required.", nameof(rackId))`) y las líneas esperadas |
| M-08 | MENOR | En el RED, CT-11 tiene que dar `regla-R<n>=False` sin lanzar cuando falta un miembro (`ApplyToDraft`, el constructor privado). Si lanza, es C04. Además, R3 necesita un partidor de miembros con nombre. Y el `finally` de CT-06 tiene que poner la sonda en «Sí», como `CloseQuietly` de T-CL | T-SG `Body()` (`:179-203`); `PushBackSharedReferenceGuardTests.Members`; T-CL:47-65 y :126 | Escribirlo en CT-11 y en CT-06 |
| M-09 | MENOR | CT-B-12 es formal: guarda un `enum`. D2, condición 3 («DraftSession editing does not mutate either shared object»): PB-T2a crea el primer camino de edición de un `DraftSession<PushBackEditableState>` y no lo prueba con huellas | [OD2], condición 3; paquete D-PBT2-14 | Añadir a CT-A-03 las huellas de las cuatro referencias de D2 antes y después de `ApplyToDraft`. En CT-B-12, comprobar además que el archivo solo declara el `enum` |
| M-10 | MENOR | Hechos imprecisos: la descripción de G-10; el tramo WC:738-746; la fila de G9 («no nombran el piloto»); las anclas de `Fingerprint` y `AllObjects`, que en `a18ba345` cambian de línea y de firma | §3 | Corregir el §2.7, el §4 y los registros C y K |
| M-11 | MENOR | En PB-T2b, `PushBackRichPreviewTests`, `PushBackLibrarySaveGateTests` y `DialogWindowCharacterizationTests` quedan «medidos en la emisión». `ExpectedEvidence` no pide el `Trailer` de `worker-handoff.json`, la mitad (b) de M14, que PB-T1c sí pide | v2.8, V28-17; registro M | Fijarlos en la remedición y añadir el `Trailer` |
| M-12 | MENOR | En WORKSPACE, `FrontalSideBox` sigue visible con el tooltip «De qué lado son los dos cortes frontales de al lado», pero esos botones están ocultos | WX:776-780 | Cambiar el tooltip, que es un literal declarado, o decir en L-PBT2-4 que queda así |
| M-13 | MENOR | PB-T2b tiene que hacer en una sola invocación: el producto de W, siete clases, trece CT (copias de CT-PBT1C-02, CT-PBTC-02, 05 y 07, dos motores de G-L1), el paso 1 y tres corridas locales. Su tamaño frente al tope de 60 minutos del Worker no se ha valorado | AUTOMATION_PLAN §16.4 | Valorarlo, o separar CT-08 a 10 en una PB-T2c |
| M-14 | MENOR | Con un W0 inválido, «Restaurar» vuelve al rack nuevo por defecto del `LoadNew` encadenado, y «Aplicar» lo llevaría al borrador. En RACKEDITAR pasa igual, pero aquí cae en el borrador | W:3309-3328; paquete R-PBT2-18 | Anular `lastComputation` en el constructor privado antes de `LoadFromModel`, o llevarlo a la vuelta al Owner como dato de L-PBT2-9 |
| M-15 | MENOR | A2-1 manda a PB-T4 `ForDraft(PushBackDraftSessionPort.LaunchFor(...))` tal cual. Así, PB-T4 cambiaría el borrador sin pasar por el host, que exige acabar la marca de *known clean* en cada cambio, y sus portadores no pueden exponer tipos de `Systems` ni de `Drafts`, salvo una excepción declarada como `TryUpdatePilot` | `WorkspaceLifecycleGuardTests.cs:509-520`; `WorkspaceHostCore.cs:66`, `:832`; `WorkspaceEditIntentHostTests.cs:1030-1046` | Reescribir A2-1: PB-T4 envuelve el puerto en el host (`NotePilotEdit`) y declara la excepción de portador. O admitir en el puerto una devolución de llamada tras aplicar |
| M-16 | MENOR | La opción (B) de Q-PBT2-3 omite que toca `Pilot/` y sus guardas, que D-PBT2-15 y [P] dejan para PB-T4 | Paquete Q-PBT2-3 y D-PBT2-15 | Decirlo en su descripción |

## §9. Veredicto

**NECESITA v2 — 0 / 8 / 16.**

**Lo que no hay que rehacer:**
- el diseño de fondo: los dos modos, el puerto, la carga por copia, la guarda, las costuras y la ocultación;
- la división;
- las anclas de producto, que se sostienen salvo los detalles de M-10;
- las guardas de T1B, que siguen en verde sin sustituir ninguna.

**Lo que tiene que traer la v2:**
- I-01 a I-08, cada una con su corrección;
- los menores, aplicados en el texto.

**Después:**
- la remedición sobre el cierre de PB-T1c, que ya prevé el paquete;
- una comprobación acotada.

**Decisiones para el Owner:**
- **Q-PBT2-1**, reformulada según I-08;
- **Q-PBT2-2**;
- **Q-PBT2-3**, con su opción (B) completada (M-16);
- **Q-PBT2-4**.

Las cuatro son reales y ninguna estaba ya decidida. No hay ninguna decisión material nueva. Se le informa, sin preguntar, de N-PBT2-01 (precedente F4B-E20) y de los once literales exactos (M-03).

---

## §10. Recomprobación de la v2

**Veredicto: LISTO PARA EMITIR TRAS REMEDICIÓN — 0 BLOQUEANTES / 0 IMPORTANTES / 7 MENORES nuevos.**

- Los 24 hallazgos de la v1 están resueltos: los ocho importantes y los dieciséis menores.
- Las siete correcciones elegidas por el redactor son válidas.
- La división en tres subtareas se sostiene, y el RED de PB-T2c discrimina.
- Quitar `SetName` y `SyncName` de «Aplicar» no rompe ni la identidad ni el nombre del borrador. Queda un resto, por «Guardar en biblioteca» (N-01).
- Las cuatro preguntas son exactas, autocontenidas y con opciones reales. Ninguna estaba decidida y no falta ninguna.
- No hay regresiones ni decisiones materiales nuevas.

Los siete menores nuevos se aplican en el texto de emisión, después de la remedición sobre el cierre de PB-T1c que ya exige el paquete.

### 10.1 Método

**Qué leí:**
- el paquete v2 entero (V2-01 a V2-32);
- `v2-delta.md`;
- el registro, con el apartado Q nuevo.

**Cómo lo comprobé:**
- solo por objetos git desde el checkout principal, sobre la HEAD nueva `833ea6bf` (PB-T1c emitida, F4B-E27) y sobre `95373af6`;
- `git diff --name-only 95373af6 833ea6bf -- src tests` sale vacío, y el `src/` es igual al de `2f5d3dec`. Entre las dos solo cambian documentos de PB-T1c, entre ellos su contrato emitido;
- no leí el worktree, no compilé y no usé Python.

### 10.2 Los 24 hallazgos

| Hallazgo | ¿Resuelto? | Comprobación |
|---|---|---|
| I-01 | **Sí** | CT-PBT2A-04 y CT-PBT2B-09 nombran las quince autopruebas, (k1) y (k2) incluidas. Las copian de G-L1c (`95373af6`, igual a `a18ba345`). Mi ancla `:238-253` era imprecisa: el arreglo de autopruebas va en `:223-239`, como mide Q-3 |
| I-02 | **Sí** | Sale la «lección»; F4B-E25 se cita en §1 y §6.0, y cada contrato declara sus `Authorities` con su clase. W como `UNIT_CHANGE` en PB-T2b, y W y WC en PB-T2c, siguen el precedente del contrato emitido de PB-T1c (`F4B-PBT1C-…/coordinator/gate-contract.json:316-322`, sección descriptiva más F4B-E25). Comprobé en `833ea6bf` que existen las rutas de F4B-E23, E24 y E25 y la de F4B-E26 |
| I-03 | **Sí** | R3 mira toda la clausura, con su autoprueba en WC. Comprobé que en `833ea6bf` solo W:3354, :3409 y :3414 contienen los tres tokens. CT-03 gana dos pasadas, y las botas quedan declaradas. Queda un detalle del fixture de la pasada 2 (N-03) |
| I-04 (variante: `HasError`) | **Sí** | R11 tiene dos autopruebas: una validación propia (R11a) y la llamada quitada (R11b). Añadir `HasError` estrecha la regla y es correcto |
| I-05 (segunda opción) | **Sí** | Fuera el plan B. La comprobación de que se puede reasignar `editable` pasa a los anclajes de PB-T2a, con STOP y v3 si falla. Por la letra emitida de PB-T1c (`833ea6bf`, el paquete de PB-T1, CT-PBT1C-01, 03 y 04), nada lo impide |
| I-06 | **Sí** | A-7 se arrastra a PB-T3 (A2-12). A-9 queda declarada y va a PB-T4 (A2-13), con anclas. Es coherente con F4B-E26 («A PB-T2 y PB-T4 … M09, M11, M12 y M15») |
| I-07 (primera opción) | **Sí**, salvo el resto de N-01 | Véase 10.4 |
| I-08 | **Sí** | La tensión con D8, el literal y la excepción de Esc, la X y Alt+F4 están en la pregunta. Queda un matiz de redacción (N-04) |
| M-01 | Sí | Las filas `ambos-mixto`, `lado-B` y `celda-1-0`, por `EstadoDeVentana`. Por lectura, `ApplyMixedSideState` las deja idempotentes |
| M-02 | Sí | `pb-21-cola` sale del corpus C18 |
| M-03 | Sí | §8.4 |
| M-04 | Sí | A2-10 |
| M-05 | Sí | L-PBT2-11. Comprobé que `HeaderConfiguratorDialog` no crea la ventana (W:1978-1979) |
| M-06 | Sí | A2-11 |
| M-07 | Sí | Sentencias y mensajes literales. Los textos de .NET 8, «Value cannot be null. (Parameter '…')» y «… (Parameter 'rackId')», son los correctos |
| M-08 | Sí | CT-PBT2B-08 y CT-PBT2B-06 |
| M-09 | Sí | Huellas en CT-PBT2A-03; `solo-el-enum=True` |
| M-10 | Sí | §2.7, §4 y los registros C, K y Q. Comprobado: `Product() => Src.AllProduct()` y `RefreshCompositePanel` hasta WC:777 |
| M-11 (variante: fijados ya) | Sí | 9, 5 y 23 `Fact`, sin `Theory`. El `Trailer`, en la evidencia común |
| M-12 (segunda opción) | Sí | Declarado (L-PBT2-4) e informado (§8.4) |
| M-13 (se divide) | Sí | PB-T2c (véase 10.3). Q-5 da el dato: el Worker de PB-T1b tardó 70 minutos |
| M-14 (primera opción) | Sí | Comprobé que `SetModel` admite nulos (SES:127-131) y que todo lo que lee `lastComputation` fuera de los caminos guardados admite el nulo: `RenderPreview` (W:3144), `PlanFor` (W:3208), `DescribeFromLastComputation` (W:1729-1733) y la vista previa (W:3170-3201). `lastComputation = null` es una reasignación que G-L1 admite (`Rebind`). CT-02 gana la fila `estado-invalido` |
| M-15 (primera opción) | Sí | A2-1 reescrito: el envoltorio en el host y la excepción de portador |
| M-16 | Sí | Q-PBT2-3 (B) |

### 10.3 La división en PB-T2a, PB-T2b y PB-T2c

**Cada una se puede entregar:**
- **PB-T2a**, sin cambios de fondo.
- **PB-T2b** queda en W, el archivo del modo y seis clases, con siete CT de UI y dos de Core. Sus `RequiredTests` filtran por clase exacta, y sus recuentos (Core 19 094, UI 2 447) cuadran.
- **PB-T2c** es solo de pruebas: una clase y tres CT, con el régimen de PB-T1a. Los mutantes se revierten con `git restore --source=BaseSha`, el GREEN no toca `tests/` (§16.8), y `git diff BaseSha..CurrentSha -- src` queda vacío (INV-PBT2C-01).

**El RED de PB-T2c discrimina.** Las tres líneas que muta existen hoy, literales:
- `Keep` en W:3683-3684;
- `UpdateSideBEmptyNotice` en WC:95;
- la de `ApplyToDraft`, que la define el propio paquete.

Cada mutante, aislado, hace fallar por aserción el CT de su fila:
- **MU-T2C-01** quita la regla `Keep`, y los exactos llegan redondeados;
- **MU-T2C-02** deja el aviso en `Collapsed`;
- **MU-T2C-03** aplica `appliedState` y no el estado editado, así que `igual-a-la-ventana` sale falso en toda fila con `aplicado=True`.

MU-T2C-03 hace además fallar R4 de CT-PBT2B-08, en Core. Así se cumple el n.º 9 de §16.9: el job Core en `failure`.

Un solo mutante sobre el camino compartido del puerto basta para CT-PBT2C-03: el camino del control al estado ya lo discrimina CT-PBT1C-02 en la ventana.

**Las dependencias son correctas:**
- PB-T2c parte del cierre de PB-T2b; sus anclas [T2b] se miden ahí, y su alcance prohibido sube a 60;
- PB-T2b parte de PB-T2a y de PB-T1c;
- la reasignación de `editable` se comprueba antes de PB-T2a.

**Un menor:** la atribución prevista de MU-T2C-03 incluye CT-PBT2B-06, que probablemente no falle (N-07).

### 10.4 «Aplicar» sin `SetName` ni `SyncName` (I-07)

**No rompe nada que ya exista:**
- `Name_Changed` está en `TextChanged` (WX:47) y, con el `SyncName()` que PB-T1c pone justo después (D-PBT1-13 (8), INV-PBT1C-04), cada tecla deja el nombre, recortado, en la identidad y en `editable`. «Aplicar» lo lleva al borrador;
- la carga siembra `NameBox` bajo `suppressSync`, así que un nombre nulo o con espacios queda tal cual. Las filas nuevas de CT-02 y CT-04 lo fijan;
- el `RackId` no se toca, y `EnsureId` no acuña nada;
- CLASSIC conserva el `SetName` de `RequestDraw`.

**Un resto** (N-01): «Guardar en biblioteca» sigue llamando a `SetName(NameBox.Text?.Trim())`. Es un literal de CT-22 que no se puede quitar, y PB-T1c le añade `SyncName()` (W:3504). En WORKSPACE, con un nombre nulo o con espacios:
- guardar en biblioteca normaliza el nombre del estado de la ventana;
- «Cerrar» preguntará entonces por cambios sin aplicar sin que el usuario haya editado nada;
- «Aplicar» llevará ese nombre al borrador y lo ensuciará.

El borrador no cambia hasta «Aplicar», así que F4B-E23 sigue cumplida. Pero «El nombre del rack no se toca» (§8.4) y «abrir y aplicar sin editar deja S exacto, también con un nombre nulo o con espacios» (D-PBT2-06 (2)) solo valen si no se usa la biblioteca.

### 10.5 Las cuatro preguntas

- **Exactas y autocontenidas:**
  - **Q-PBT2-1** dice ya la tensión con D8, da el literal de la pregunta nueva, la excepción de Esc, la X y Alt+F4 en (A), y qué pierde Esc en (B) y (C). Comprobé la premisa «un clic en "Cerrar" confirma la caja»: los botones del tema no son `Focusable=False` y no hay ámbitos de foco (`AppStyles.xaml:99`, `:130`);
  - **Q-PBT2-2, Q-PBT2-3 y Q-PBT2-4** son correctas; Q-PBT2-3 (B) ya nombra `Pilot/`, y Q-PBT2-4, las tres subtareas.
  - Los cuatro textos tienen menos de 400 caracteres, y la recomendada va primero (Q-6).
- **Ya decididas:** ninguna. F4B-E26 y F4B-E27 no deciden nada de PB-T2.
- **Faltan:** ninguna. La división en tres, la anulación de `lastComputation` y la normalización del nombre por la biblioteca son lecturas o declaraciones, no decisiones materiales.
- **Un matiz de redacción** (N-04): «sería el primer aviso de cambios sin guardar de todo el editor» se puede leer como «el primer aviso del editor», pero ya existe el del módulo, que la misma pregunta menciona. Mejor: «el primer aviso sobre todo lo editado; hoy solo avisa de un módulo sin confirmar».

### 10.6 Regresiones y decisiones

- **Regresiones:** ninguna.
  - El diseño de fondo no cambia.
  - Las guardas de T1B siguen en verde: R3 y R11 solo añaden comprobaciones, y PBT2B-C13 admite el constructor privado y los 26 nombres.
  - CT-PBT1C-03 y 04, en su texto emitido, siguen sin impedir el diseño.
  - Los recuentos son coherentes: PB-T2a 19 092 / 2 439; PB-T2b 19 094 / 2 447; PB-T2c 19 094 / 2 450.
- **Decisiones materiales nuevas:** ninguna.

### 10.7 Hallazgos nuevos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| N-01 | MENOR | El resto de I-07 por «Guardar en biblioteca» (10.4) | W:3503-3504; T-SG CT-22 (`:324-341`); D-PBT1-13 (8); paquete §8.4 y D-PBT2-06 (2) | Declararlo (L-PBT2-12) y añadir a CT-PBT2B-04 una fila `nombre-nulo`, primero biblioteca (con la costura devolviendo nulo) y después «Cerrar»: `avisos=1`, y «Aplicar», `llamadas=1`. O decir que solo vale sin biblioteca |
| N-02 | MENOR | Las líneas esperadas no son del todo literales: las pasadas 2 y 3 de CT-PBT2B-03 tienen «…», y el sufijo ` descartes=<n>` de cada ventana de WORKSPACE no está fijado en CT-02 a CT-05. Con Q-PBT2-1 (A), cerrar en el `finally` una ventana con cambios sin aplicar ya pregunta (las pasadas 2 y 3 de CT-03, `error` de CT-04, `bloqueante` de CT-05) | Paquete §5.0 (el régimen) y §5.3 | Escribir las líneas enteras y su `descartes=<n>`, o la regla: 1 si al cerrar queda algo sin aplicar, si no 0 |
| N-03 | MENOR | La pasada 2 de CT-03 copia los gestos de CT-PB0-12, 13, 15 y 16 sobre `pb-01-base`, pero sus originales usan `pb-31-v2-modulos`, `pb-06-seguridad` y `pb-05-topes`. El de CT-PB0-12 elige el módulo `M3`, que también existe en `pb-01-base`, así que no se rompe. Pero «copiados» no basta para saber el fixture | `PushBackFullEditorCharacterizationSubdialogTests.cs:43`, `:110`, `:205`, `:242` y `:57` | Fijar el fixture de cada gesto (el del original o `pb-01-base`) |
| N-04 | MENOR | La redacción de Q-PBT2-1 (10.5) | Paquete §8.2 | La frase propuesta en 10.5 |
| N-05 | MENOR | `F4B-PBT2C-WORKSPACE-CARRYOVER` tiene 29 caracteres, no 30 | Paquete §6.0 | Corregir el número |
| N-06 | MENOR | La base de la v2 es `95373af6`, y la HEAD ya es `833ea6bf` (F4B-E27, PB-T1c emitida), con el mismo `src` y los mismos `tests` | `git diff --name-only 95373af6 833ea6bf -- src tests`, vacío | En la remedición: F4B-E27 al §1 y la base nueva |
| N-07 | MENOR | Para MU-T2C-03, la atribución prevista dice «CT-PBT2B-03, 04 y 06», pero CT-06 seguramente no falle: su línea `cerrar-tras-aplicar` no depende del estado aplicado | Paquete §5.4 | Dejar CT-03, CT-04 y CT-08 (Core) como previstos, y que la tabla de atribución recoja lo que de verdad falle |

### 10.8 Veredicto de la recomprobación

**LISTO PARA EMITIR TRAS REMEDICIÓN — 0 / 0 / 7.**

**Lo que falta antes de emitir:**
1. N-01 a N-07, aplicados en el texto;
2. la remedición sobre el cierre de PB-T1c (§6.0), con la comprobación de la reasignación de `editable` en los anclajes de PB-T2a;
3. las respuestas del Owner;
4. la comprobación acotada de cada subtarea.

**Decisiones para el Owner:**
- Q-PBT2-1, con el matiz de N-04;
- Q-PBT2-2;
- Q-PBT2-3;
- Q-PBT2-4.

No hay ninguna decisión material nueva. El §8.4 informa sin preguntar.
