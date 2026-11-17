# Revisión adversarial independiente del código — I-64 F4B-PBT0-CHARACTERIZATION, intento 1

- **Objeto:** `git diff f2eedd9a0d03c13edc0a92a0949d2c0b64bd78f4 28e38d8d51108c22305a5fc41ce84070a2efdbe7` (BaseSha = ChainBaseSha `f2eedd9a`; RED `8f75a89b`; GREEN `28e38d8d`), rama `architecture/workspace-persistente-rackcad`, leída con `git -C` y lectura de archivos, sin checkout, sin compilar y sin ejecutar pruebas.
- **Autoridades leídas en el GREEN:** el contrato `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json`, el paquete `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md` (en adelante **[PK]**), el paquete de la fase `docs/initiatives/I-64-f4b-push-back-completo-package.md` (**[P]**), el prompt del Worker y su entrega (`artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T185616Z-3c1b/`, carpeta ignorada por `.gitignore:3`), con sus parches y TRX.
- **Convención:** las citas de producto son de la BaseSha, que es byte a byte igual al GREEN en `src/`. «W» = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`. Las citas de pruebas son del GREEN (idénticas en el RED: `git diff 8f75a89b 28e38d8d -- tests` está vacío).

## 1. Veredicto

**REWORK — B/I/M = 0/1/6.**

El código entregado es técnicamente sólido: el alcance es exacto, los 20 mutantes son los de la tabla cerrada (todos en su forma principal), cada uno hace fallar en aislado el método de su fila por aserción, el RED conjunto falla 21/21 por aserción, el GREEN devuelve el producto a los bytes de la base y la CI del GREEN está en verde. Lo que impide aceptarlo **tal cual** es **I-01**: el Worker cambió la lista cerrada de casos de CT-PB0-17 (añadió c-16) para que MU-PB0-16 discriminara, cuando el contrato manda parar con PBT0-C10 (y, en su causa, PBT0-C07). Esa decisión no es del Worker.

**La salida mínima no es reescribir código.** Es una disposición del Owner o del Coordinador (OWNER-1):
- o ratifica c-16, y la representación «RECHAZADO» de c-12 y c-13, como enmienda de CT-PB0-17 y de la fila MU-PB0-16;
- o lo trata como PBT0-C10, es decir, como defecto del contrato.

Si se ratifica, lo que queda son menores. M-01 y M-02 solo se pueden corregir en un intento correctivo, porque las pruebas no se tocan después del RED; si no hay intento correctivo, basta con registrarlos.

## 2. Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| I-01 | IMPORTANTE (y OWNER-1) | **CT-PB0-17 se cambió fuera de la lista cerrada en lugar de parar.** Con c-01..c-15, MU-PB0-16 no discrimina, y su alternativa (W:387) tampoco. El Worker añadió c-16 (`Dimensions = 3`), y lo justifica con la cláusula del paso 3 del prompt. Pero la prueba original sí caracterizaba bien el comportamiento actual de sus casos (sin delta), así que la cláusula no aplica. El defecto estaba en el contrato: era PBT0-C10. La entrega declara `TriggeredStopConditions: []` y `Disposition: NONE`. | `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationCatalogTests.cs:60-62` (c-16) y `:144-151` (c-10, c-11 y c-16, todos sin delta en la base). `red-ci-ui/ui.trx`: CT-17 falla en el RED **solo** por la línea extra de c-16, `[SIN-ETIQUETA] $.PushBack.Structure.Dimensions:` (posición 24). Contrato: INV-PBT0-14 y PBT0-C10. Prompt, paso 3 (`prompt.md:172`) y regla de OpenQuestions (`:184`). Dictamen completo en el §5. | Disposición del Owner o del Coordinador (OWNER-1), registrada en `f4b-dispositions.md`. Si ratifica, el código no cambia. |
| M-01 | MENOR | **Fuga de una ventana en CT-17, caso c-13.** `Abrir` crea `Ventana()` y luego `LoadExisting` lanza `ArgumentException`. La prueba captura la excepción y sigue con `continue`, pero esa ventana nunca se cierra. Queda en `Application.Windows` durante todo el proceso de pruebas. Es inerte: nunca se mostró, no tiene HWND y la ventana no usa temporizadores ni `BeginInvoke`. Aun así, incumple la letra de INV-PBT0-07 («todas se cierran en finally; nada queda vivo»), toca el texto de PBT0-C09 y contradice la afirmación de la entrega. | `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs:126-133` (la ventana se crea en `:130` y `LoadExisting` se llama en `:131`, sin `try`). `PushBackFullEditorCharacterizationCatalogTests.cs:96-106`. La excepción sale de `src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs:473-474` (llamado desde `:45`). | En un intento correctivo, `Abrir` debe cerrar la ventana si `LoadExisting` lanza y volver a lanzar la excepción. Si no hay intento correctivo, registrarlo. El Coordinador puede elevarlo si lee PBT0-C09 de forma literal. |
| M-02 | MENOR | **Cita `archivo:línea` errónea en la desviación.** `DynamicRackSystemDocument.cs:322` está en `ToDomain()`, la ruta del sistema dinámico. Push Back deserializa con `ToDesign()` (`PushBackDesignDocument.cs:161`), y allí la normalización de `Dimensions` está en `:227`. El hecho se sostiene; la línea no (INV-PBT0-04 pide el `archivo:línea` medido). | `src/RackCad.Application/Persistence/DynamicRackSystemDocument.cs:196-227` (ToDesign: unidad en `:205`, escala en `:226`, Dimensions en `:227`), `:322` (ToDomain) y `:429-432` (`ValidDimensions`). La cita errónea está en `CatalogTests.cs:60-61` y en la entrega (Deviations[0], UnexpectedFindings[1], T-2 y M9). | En un intento correctivo, citar `:227` y `:429-432`. En cualquier caso, corregir el registro. |
| M-03 | MENOR (parte de OWNER-1) | **Segunda desviación:** c-12 y c-13 se fijan como `RECHAZADO <tipo>: <mensaje>`. El §4.1 no lo prevé: `Salida` solo contempla `NO-VÁLIDA: <StatusText>`. Es caracterización fiel y cabe en INV-PBT0-04 y OB-06.5 («una clase nueva se fija tal cual, sin STOP»), pero cambia la forma de lo que fija CT-17 por un hecho del §2 que no se sostiene (§5). Además, la entrega dice que lo fija «como CARGA-APPLICATION», y la prueba no lleva esa etiqueta: solo la lleva T-2. | `CatalogTests.cs:101-105` y `:146-147`. [PK] `:230` (Salida) y `:89-91` (H-05). | Ratificarla junto con I-01 y alinear el texto de la entrega. |
| M-04 | MENOR | **CT-08 y CT-09 fijan el estado del editor, no la salida.** El §4.2 pide el número de `Structure.Fronts` con su `IsActive` y `Composite.AbsentSlotsA/B` (de la salida). La prueba lee `CompositeState.SideA/SideB.Structure` y `AbsentSlotsOfA/B()`. Solo los cortes salen de `LastComputation`. Un defecto del ensamblador al copiar las ranuras ausentes al diseño (`PushBackCompositeEditorAssembler.cs:71-87`) no lo vería este CT, aunque CT-07 sí lee `design.CompositeOrDefault()`. | `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationCompositeTests.cs:55-63` frente a [PK] `:315` y `:321`. | Registrar. En un intento correctivo, leer también la salida. |
| M-05 | MENOR | **Aserción de las líneas (a)/(b) retiradas fuera de CT-18 a CT-20.** CT-01 y CT-17 asertan `lineas-retiradas(a/b)=[]` y `retiradas(a/b)=[]`. INV-PBT0-18 dice «solo CT-18 a CT-20 aseveran (a) o (b)», y el §4.1 pide que la **entrega** las liste. Hoy es inocuo: todas las listas están vacías y PB-TC no las llenaría. | `PushBackFullEditorCharacterizationStateTests.cs:78`, `:144`, `:147`, `:161` y `:164`; `CatalogTests.cs:118`. | Registrar. |
| M-06 | MENOR | **Evidencia de mutación incompleta.** MU-01 a MU-15 se corrieron en aislado contra el CT-17 anterior (sin c-16); lo declara KnownLimitations. No hay TRX ni salida custodiada de MU-16 con c-01..c-15 ni de la alternativa W:387: la no discriminación se sostiene solo por análisis, y la he comprobado yo en el código (§5). Las mediciones M1 a M9 tampoco tienen salida guardada: `tests/b0/measure-*.txt` solo trae CT-19, CT-20 y CT-21. | Directorios `tests/` de la entrega: no hay `m16a` ni `m16alt`. `worker-handoff.json`, KnownLimitations[6]. | Registrar. Por razonamiento, c-16 no cambia el resultado de MU-01 a MU-15: c-16 es `pb-01-base` con `Dimensions = 3`, y ninguno de esos mutantes toca esa ruta. |

**Observación sin número:** en CT-16, aceptar Seguridad sin cambios sobre `pb-05-topes` (que no tiene seguridad) añade a la salida `DefensePieceId` y una selección nueva de 28 hojas. Por el código, es el **portador de la familia de botas** que `ApplyBootSections` crea si no existe (W:2754-2763), y que el comentario declara intencional (I-42 S1G). Queda fijado por huella (`SubdialogTests.cs:272`), pero la entrega no lo menciona. No he podido verificar si dibuja algo o entra en el BOM.

## 3. Comparación de las listas de alcance

| Comprobación | Resultado |
|---|---|
| `git diff --name-only Base..GREEN` | **10 archivos**, exactamente las 10 rutas de prueba de AllowedWriteScope. |
| `git diff --name-only Base..RED` | **16 archivos**: el conjunto coincide exactamente con las 16 rutas de AllowedWriteScope. |
| `git diff --name-only RED..GREEN` | **6 archivos**: los 6 de MutantScope, todos en `src/`. El GREEN no toca `tests/`. |
| Coincidencias con ForbiddenWriteScope (265 entradas; por igualdad, prefijo de carpeta y patrón) | **0** en Base..RED, Base..GREEN y RED..GREEN. |
| `git diff Base GREEN -- src` | **Vacío**. Los ids de blob de los 6 archivos mutados son iguales en la base y en el GREEN, y distintos en el RED (`c4615c84`, `2a64f8d3`, `5f59ac3b`, `74e1a1ab`, `4752b394`, `205d3889`). Coinciden con la tabla de bytes de la entrega. |
| `tests/RackCad.Tests/Fixtures/` (H-PB-32, INV-PBT0-17) | Sin cambios. |
| Guardas, soporte y pruebas existentes | Ninguno modificado: solo se añaden archivos. |
| Commits | Exactamente un RED, con el título de INV-PBT0-02, y un GREEN. Ambos con el trailer de Sonnet 5.5, y el GREEN publicado en `origin`. |
| Construcciones nuevas | 21 `[Fact]`, sin `Theory` ni `Skip`. Un solo namespace igual a la raíz, `public sealed class`, y el ayudante `internal static`. Las 9 clases de UI van en `EditorDiscardPromptCollection`. Ningún uso de `HeaderConfiguratorPresenter`, `SafetyWindowDialog`, `MessageBox`, `SaveLibrary`, `Bom_Click`, `InsertBatch`, `DateTime`, `Random`, `Sleep`, `PushFrame`, teclado ni foco, ni literales prohibidos por INV-14. |
| Avisos de compilación (logs `tests/b0`, `red1` y `g5`) | Conteos idénticos por código: Core 66 xUnit, UI 28 xUnit y Plugin 104 líneas MSB3277. Ningún aviso CS, NU ni NETSDK. |

## 4. Tabla de los 20 mutantes (INV-PBT0-14, §5 de [PK])

Todos los parches coinciden línea a línea con su fila y con la línea medida en la base. `red-all.patch` contiene exactamente las mismas 38 líneas cambiadas que `git diff Base RED -- src`: los 20 mutantes y nada más. Ninguno toca una asignación de `Owner`, un `ShowDialog`, un `MessageBox` ni `EditorDiscardPrompt`; tampoco las comprobaciones de nulo de las costuras (W:1938, :2615, :2722, :2849, :2981, :3018), las guardas de `RequestDraw` (W:3325-3363) ni el constructor.

Las columnas «Discrimina en aislado» y «Por aserción» salen de los TRX `tests/mNN`. Por aserción significa que el mensaje no lleva prefijo de tipo y que la primera línea del StackTrace está bajo `tests/`.

| Fila | Archivo:línea (base) | Cambio aplicado frente a la tabla | Discrimina en aislado | Por aserción |
|---|---|---|---|---|
| MU-PB0-01 | W:315 | Idéntico (principal) | Sí: CT-01 | Sí |
| MU-PB0-02 | W:3365 | Idéntico | Sí: CT-02 | Sí |
| MU-PB0-03 | W:2463 | Idéntico | Sí: CT-03 (también fallan CT-04, CT-05 y CT-12) | Sí |
| MU-PB0-04 | DA:211-213 | Idéntico | Sí: CT-04 | Sí |
| MU-PB0-05 | DA:300 | Idéntico (principal, sin la alternativa W:2499) | Sí: CT-05 | Sí |
| MU-PB0-06 | WC:839-840 | Idéntico | Sí: CT-06 (también fallan CT-01, CT-02 y CT-03) | Sí |
| MU-PB0-07 | CS:320-323 | Idéntico (principal; R-02 no se re-estima) | Sí: CT-07 | Sí |
| MU-PB0-08 | W:1138 | Idéntico | Sí: CT-08 y CT-09 | Sí |
| MU-PB0-09 | RackViewAvailability.cs:111 | Idéntico | Sí: CT-10 (Core) | Sí |
| MU-PB0-10 | W:3365 | Idéntico | Sí: CT-11 | Sí |
| MU-PB0-11 | W:1946 | Idéntico | Sí: CT-12 | Sí |
| MU-PB0-12 | W:2626 | Idéntico | Sí: CT-13 | Sí |
| MU-PB0-13 | W:2642 | Idéntico | Sí: CT-14 (también fallan CT-13, CT-15 y CT-16) | Sí |
| MU-PB0-14 | W:2638 | Idéntico | Sí: CT-15 (también fallan CT-13, CT-14 y CT-16) | Sí |
| MU-PB0-15 | W:2653 | Idéntico (queda `else { }`) | Sí: CT-16 | Sí |
| MU-PB0-16 | W:407 | Idéntico (principal) | **Solo con c-16** (I-01). Con c-01..c-15, no; la alternativa W:387 tampoco (§5) | Sí |
| MU-PB0-17 | DynamicViewDecorations.cs:402 | Idéntico | Sí: CT-18 (Core) | Sí |
| MU-PB0-18 | W:406 | Idéntico | Sí: CT-19 | Sí |
| MU-PB0-19 | W:819 | Idéntico | Sí: CT-20 | Sí |
| MU-PB0-20 | DynamicViewDecorations.cs:329 | Idéntico | Sí: CT-21 (Core) | Sí |

**La conjunción.** En el RED (TRX de la CI y `tests/r0`), Core falla 3/3 y UI 18/18, todos por aserción en la línea del `Assert.Equal` final, sin NotExecuted ni Skipped.

Ningún mutante revierte ni neutraliza el código de otro. Los dos pares que comparten contexto, MU-02 con MU-10 y MU-16 con MU-18, quedan los dos aplicados en el RED. Sí hay **enmascaramiento de efectos**, aunque no anulación:
- MU-03 impide que «Confirmar» recalcule, así que en la conjunción CT-04 y CT-05 fallan antes de que se vea el efecto de MU-04 o MU-05. El aislado cubre a cada uno.
- CT-17 falla en el RED **únicamente** por c-16.

## 5. Dictamen razonado sobre las dos desviaciones de CT-17

### 5.1 Lo que he verificado en el código

1. **7 y −1 no llegan a W:407.**
   - Push Back deserializa su estructura con `DynamicRackSystemDocument.ToDesign()` (`PushBackDesignDocument.cs:161`).
   - Allí, `Dimensions = ValidDimensions(Dimensions)` (`DynamicRackSystemDocument.cs:227`), y `ValidDimensions` convierte cualquier valor fuera de `[None, Detailed]` en `None` (`:429-432`).
   - `RecoverInputs` copia el valor ya validado (`PushBackEditorState.Load.cs:207`), de modo que W:407 (`Math.Min(Detailed, Math.Max(0, d))`) solo recibe valores de 0 a 3 por la vía del documento.
   - **Conclusión:** el recorte es inalcanzable desde un sobre. La cita `:322` de la entrega es la de `ToDomain()` (M-02).
2. **La alternativa W:387 no discrimina.**
   - W:387 solo usa el literal `"kg"` si `pallet.WeightUnit` está en blanco.
   - `ToDesign` ya sustituye el blanco por `"kg"` (`:205`), y `"ton"` toma la rama `pallet.WeightUnit`.
   - El literal es inalcanzable, y cambiarlo a `"lb"` no mueve ningún caso.
3. **Lo mismo pasa con la escala:** `ToDesign` convierte `AnnotationScale ≤ 0` en 1,0 (`:226`), así que la sustitución de W:406 tampoco se alcanza con c-15.
4. **c-12 y c-13 ni siquiera llegan a la ventana.**
   - `PalletsDeep = 1` lo rechaza `RackProjectStore.ValidateProject` (`RackProjectStore.cs:343-353`).
   - `PalletTolerance = 0` hace lanzar a `Validate` del resolver (`DynamicRackSystemResolver.cs:473-474`) dentro de `LoadExisting`, antes de `RecoverInputs` (`Load.cs:199`) y de W:389.

### 5.2 ¿Caracterizaba bien el CT-17 original el recorte de W:407 que el §2 atribuye a la ventana?

No podía, y no por un defecto de la prueba sino del contrato:
- El §2 (H-05, [PK]:89-91) enumera «el recorte de `Dimensions` a `[0, Detailed]` en W:407» entre las sustituciones de `LoadFromModel`.
- CT-17 elige c-10 y c-11 (7 y −1) para alcanzarlo ([PK]:396), y la fila MU-PB0-16 ([PK]:539) se apoya en esos casos.
- Pero `Sobre(t)` y `Abrir(t)` pasan por el mismo almacén, que normaliza antes. Con los casos del contrato, la prueba fija **exactamente** el comportamiento actual de c-10 y c-11, que es no producir delta.

Es decir: la prueba original sí «caracteriza bien el comportamiento actual» de sus casos. Lo que falla es la hipótesis del contrato de que esos casos alcanzan W:407.

### 5.3 ¿Es c-16 una corrección admitida por el paso 3, o un cambio de contrato que debía ser STOP?

**Es un cambio de contrato que debía ser STOP.** Los textos exactos:

- **INV-PBT0-14:** «un mutante que no discrimina su CT en aislado, o que hace fallar un metodo nuevo por excepcion sin alternativa valida, es defecto del contrato y STOP (PBT0-C10); el Worker no inventa mutantes».
- **PBT0-C10:** «a mutant of the closed table does not discriminate its CT in its isolated run [...] and no admitted alternative works (contract defect)».
- **Prompt, paso 3:** «Si el mutante principal no discrimina su CT [...] y la alternativa tampoco vale: NO inventes otro; corrige la prueba solo si no caracteriza bien el comportamiento actual que el CT describe; si aun asi no hay mutante valido, parada PBT0-C10».
  - La condición de la cláusula, «no caracteriza bien», no se cumple: la prueba original fijaba lo que hay.
  - c-16 no corrige una aserción mal medida. **Añade un caso** a una lista que el prompt traslada «como receta, sin reconstruirlo» (`prompt.md:160`) y que [PK] declara cerrada (§4).
  - Además, en la base c-16 no ejerce ningún «recorte»: con 3, `Min(3, Max(0, 3))` no recorta nada. Fija que el límite válido pasa intacto. Es una prueba legítima, pero es **otra** prueba.
- **Prompt, regla de OpenQuestions:** «Si una ambiguedad cambia lo que se caracteriza: para con S-03 o PBT0-C07 y no reinterpretes el contrato». Aunque la cláusula del paso 3 se leyera como ambigua, añadir un caso cambia lo que se caracteriza.
- **PBT0-C07:** «the facts of section 2 of the package do not hold on the base in a way that changes a CT». El cuadro de H-05 (las sustituciones de W:384-392 como alcanzables por el documento, y «RecoverInputs ya normaliza PalletsDeep, tolerancia...») no se sostiene para los casos de CT-17, y eso **cambió** CT-17 de dos maneras: el caso c-16 y el `try/catch`.

**Dictamen:** la salida correcta era PBT0-C10, con PBT0-C07 como causa, y volver al Coordinador o al Owner. La lectura del Worker es plausible y la desviación está bien declarada. Técnicamente, c-16 es una buena prueba de límite. Pero la autorización para enmendar una lista cerrada no es suya. Por eso I-01 es IMPORTANTE y no BLOQUEANTE: se resuelve con una disposición, sin código.

### 5.4 La segunda desviación: el `try/catch` de c-12 y c-13

- **No era lo que el paquete preveía.** El §4.1 define `Salida` con `NO-VÁLIDA: <StatusText>` para los cálculos inválidos ([PK]:230). H-05 suponía que `RecoverInputs` normalizaría `PalletsDeep` y la tolerancia (`Load.cs:196`, `:199`; y W:388-389), lo que habría dado una línea de delta.
- **Lo que hay es otra cosa:** un rechazo del almacén y una excepción del resolver antes de la ventana. Por tanto, el CT sí cambia por un hecho del §2 que no se sostiene: es literalmente el supuesto de PBT0-C07.
- **Atenuante:** INV-PBT0-04 y OB-06.5 dicen que «una clase nueva se fija tal cual y se enumera en T-2, sin STOP». Con un producto que lanza, la única prueba que puede pasar en verde es la que captura la excepción.
- **Valoración:** es caracterización fiel y conservadora. Debe ratificarse junto con c-16 en la misma disposición (M-03). Tiene un efecto colateral real: la fuga de la ventana de c-13 (M-01).

## 6. Los 21 CT frente al §4 de [PK]

Clase y ubicación: las tres de Core (CT-10, CT-18 y CT-21) están en `tests/RackCad.Tests` y las 18 de UI en `tests/RackCad.UI.Tests`, con las clases exactas del paquete. Los valores fijados pasan en la base, así que están medidos en la base. Las anclas de T-6 están declaradas.

| CT | Valoración | Nota |
|---|---|---|
| 01 | Bien | Fija, por diseño, la composición, el delta filtrado (número de líneas, SHA-256 y contenedores con su clase y `archivo:línea`) y X-01 según el §4.1. La clase se asigna por contenedor, no por línea; la huella fija la lista exacta. Afectado por M-05. |
| 02 | Bien | 17 diseños y X-01. Tupla completa: habilitado, PETICIÓN, `UpdateOnly`, vistas y delta. |
| 03 | Bien | Los cuatro pasos y las cinco listas de la reconciliación. |
| 04 | Bien | El paso 2 (Fondos + 1) no tiene efecto observable; lo declara la entrega. |
| 05 | Bien | A0, A1 y A2 de los seis avanzados, en la salida y en los controles. Lleva el comentario «INCUMPLIMIENTO CARACTERIZADO» citando DA:296-299 (`ModuleTests.cs:164-168`). |
| 06 | Bien | Cola aparcada y salida en cada paso. |
| 07 | Bien | Lee `design.CompositeOrDefault()` de la salida, la casilla y la cola aparcada. |
| 08 | **Débil** | Fija el estado del editor en vez de `Structure.Fronts` y `Composite.AbsentSlots*` de la salida (M-04). |
| 09 | **Débil** | Igual que CT-08, más el `IsComposite` de la salida. |
| 10 | Bien | Replica la decisión de RACKEDITAR (`RackPushBackCommands.cs:269-271` y `:324-333`) con `design.IsComposite`. |
| 11 | Bien | Los tres textos, en una ventana cada uno. Que `SystemToInsert.Name` quede vacío no es un defecto: en RACKEDITAR lo asigna el Plugin (`RackPushBackCommands.cs:190`). |
| 12 | Bien | La costura no muta su argumento, el caso nulo está fijado y `PanelClear 44` es ancla. |
| 13 | Bien | (i) es un hallazgo de M-03 de [PK]; (ii) fija los ids y el orden. |
| 14 | Bien | MU-13 lo detecta también por líneas comunes a todo «Aceptar», pero el delta incluye las líneas `BotaPosts`. |
| 15 | Bien | — |
| 16 | Bien | El delta fija, por contenedores, la normalización no declarada (ver la observación del §2). |
| 17 | **No fija lo prometido sin la desviación** | Con c-01..c-15 ningún mutante de la tabla lo detecta (habría sido también PBT0-C04). Fija la desviación I-01 y la M-03. |
| 18 | Bien | Cuatro planes y cada corte (H-16: no existe un quinto plan). |
| 19 | Bien | — |
| 20 | Bien | — |
| 21 | Bien, con una advertencia para el Owner (OWNER-4) | La fórmula se aplica tal como la define el paquete (K1o, emparejamiento por orden y K3 exacto). |

**No duplicación:** CT-01 no incluye los 14 diseños de un sentido de WLC, y CT-18, CT-19 y CT-20 no repiten WLC, `T19_H1` ni la caracterización legacy. **Tautologías:** no he encontrado ninguna aserción que compare un valor consigo mismo ni que pase con cualquier producto. Los ecos de costura (`costura llamada=1` y `PanelClear 40`) son anclas declaradas.

## 7. El RED, el GREEN, la estabilidad y la contención

**El RED (TRX de la CI)**
- Core: 22 fallos, que son los 3 nuevos más 19 existentes. La unión de las corridas completas aisladas de Core por mutante de Application (m04f, m07f, m09f y m17f) es **idéntica** a esas 19. MU-05 y MU-20 no rompen ninguna existente.
- UI: 123 fallos, que son los 18 nuevos más 105 existentes, y 17 NotExecuted que ya están en la base.
- La atribución de T-8 por clase suma 105, y **todas** las roturas tienen un mutante plausible:
  - MU-13 y MU-14: botas y defensa;
  - MU-03 y MU-11: cabeceras y módulos;
  - MU-08: frentes;
  - MU-01: cargas de compuestos;
  - MU-12 y MU-15: Seguridad y topes;
  - MU-16 y MU-18: las dos filas de WLC, que lo confirman en su mensaje (`Dimensions: 2 -> 3` y `PalletHeight: 61.235 -> 61.23456`).
- Algunas existentes fallan por excepción («Sequence contains no elements»), lo que se admite para las existentes. No he podido comprobar cada atribución de UI por prueba (§9).

**El GREEN**
- CI: Core 19 070 de 19 070 y UI 2 404 Passed más 17 NotExecuted previos. Los 21 nuevos en verde.
- En local, tres corridas del filtro de UI: 18 de 18 cada una.

**Estabilidad**
- Cultura invariante fijada y restaurada en el hilo STA. Orden ordinal.
- Sin hora, sin aleatoriedad y sin rutas de la máquina: la raíz se busca por `RackCad.sln` y el catálogo se carga con `FromBaseDirectory`.
- Sin teclado ni foco. CT-14 se muestra fuera de pantalla y con `ShowActivated = false`. Ninguna prueba de UI bombea el dispatcher, así que no hay reentrada.
- La única ventana que queda viva es la de c-13 (M-01). La rejilla de botas de CT-14 se cierra con su dueño.

**Contención**
- Ningún gesto ejercido escribe:
  - «Actualizar» = `RequestDraw` + `Close`;
  - `OnClosing` pasa por `EditorClosePolicy`, con el suelo del aviso de la suite;
  - el configurador, la seguridad, los topes, la defensa y la rejilla de botas van siempre por costura.
- `RackLog` solo se usa desde `CorruptFile`, `UserTemplateStore` y `UserSettings`, que no están en estas rutas.
- Ningún proyecto de pruebas referencia el Plugin.

## 8. Hallazgos de producto declarados (UnexpectedFindings)

### 8.1 `PalletTolerance = 0` (c-13)

- **¿Está bien fijado?** Sí: tipo y mensaje exactos, con la cultura de UI invariante.
- **¿Es un defecto actual?** En RACKEDITAR **no hay cierre inesperado**:
  - `EditPushBack` llama a `LoadExisting` sin `try` (`RackPushBackCommands.cs:168-170`);
  - pero `RACKEDITAR` envuelve `handler.Edit` en `try/catch` → `RackCommandSupport.Report(ex)` (`RackMenuCommands.cs:156`, `:184` y `:186-188`).
- **Consecuencia:** ese rack no se puede reabrir. El producto nunca escribe 0, porque el resolver lo rechaza (`:473-474`), así que solo afecta a documentos ajenos o editados a mano. Es información para el Owner, de prioridad baja.

### 8.2 `pb-21-cola`: 667 hojas perdidas

- **¿Está bien fijado?** Sí: número de líneas, SHA-256 y contenedores, con su etiqueta.
- **¿Indica un posible defecto actual?** Sí, una **pérdida de datos al Actualizar en RACKEDITAR**. CT-02 prueba que Actualizar escribe la salida de la carga. Para un documento de un sentido que guarda la cola de B (GAP y B:\*) y el objeto `SideB` no presente, esa salida descarta la cola y el objeto.
  - La cola dormida solo vive en la sesión (`PushBackCompositeEditorAssembler.cs:52-57`; DA:279-287).
  - Choca con D-PB-03 ([P]:202-221), que exige un estado «lossless» para el «parked queue state».
  - No he verificado si existen documentos así en dibujos reales. La ventana actual no los produce (CT-06 y CT-07), pero el piloto tiene un motivo de rechazo propio para ellos.
- **Ver OWNER-3.**

### 8.3 H-PB-28 confirmado

- **¿Está bien fijado?** Sí: A0, A1 y A2 en dos diseños, en la salida y en los controles.
- **¿Es un defecto actual?** Sí, de RACKEDITAR:
  - tras «Restaurar estándar» y «Confirmar», la vista previa muestra los seis avanzados reiniciados;
  - pero los controles siguen con los valores cargados;
  - y Actualizar, o cualquier recálculo posterior, vuelve a escribir los valores anteriores.
- **Ver OWNER-2.**

## 9. Hallazgos OWNER

- **OWNER-1 (decisión de proceso, = I-01 y M-03).** Ratificar o rechazar la enmienda de CT-PB0-17:
  - c-16 como caso nuevo y la fila MU-PB0-16 apoyada en él;
  - c-12 y c-13 representados como `RECHAZADO <tipo>: <mensaje>`.

  Si se rechaza, es PBT0-C10, un defecto del contrato. Si se ratifica, conviene anotar también que W:387, W:406 y W:407 son inalcanzables desde un documento (§5.1), porque eso afecta a la enumeración de (c) para OB-06.5.
- **OWNER-2 (posible defecto actual).** H-PB-28 queda confirmado como defecto observable de RACKEDITAR (§8.3). [P]:118 dice que su corrección «sería una decisión nueva». Hay que decidir antes de PB-TC o PB-T1 si PB-T1 lo reproduce al extraer el estado o se corrige.
- **OWNER-3 (posible pérdida de datos actual y conflicto con D-PB-03).** Las 667 hojas de `pb-21-cola` desaparecen al Actualizar (§8.2). Con OB-06.5 ([P]:392-394), los casos alcanzables de T-2 obligan a volver al Owner antes del contrato de PB-T1:
  - la pérdida de la cola;
  - c-02, la sustitución (c) del poste inexistente por la ventana (W:390 y :836);
  - las cargas que rechazan (c-12 y c-13).
- **OWNER-4 (lectura de T-3 para OB-06.4).** En R7 (`PalletFront` 42,00044), `K4 = [planta]` y el máximo |Δ| de 106,988 que da la entrega no pueden ser un desplazamiento físico de un cambio de 0,00044. Salen de la regla de emparejamiento por orden de aparición dentro de cada grupo de firma cuando hay K1o (`RoundingEffectTests.cs:188-201`; [PK] N-02). R7 y R11 son «materiales» por K3: el BOM se compara exacto, y sus longitudes difieren unas diezmilésimas (`RoundingEffectTests.cs:222` y `:233-245`). Es conforme con la letra de OB-06.4 ([P]:387-391), pero el Owner debe decidir sabiéndolo.

## 10. Lo no verificado

- La relación entre las ejecuciones de CI (ids 44174025134 y 44175320836) y los commits, el estado de cada job (build-ui en success, build-plugin omitido) y la ausencia de blame-hang o timeout. No usé `gh`; solo leí los TRX, que no llevan el SHA.
- Las corridas locales y la prueba de limpieza de INV-PBT0-06 (datos del perfil del usuario). No la he vuelto a medir, y una medición de hoy no probaría el estado de entonces. Por el código, ningún camino ejercido escribe (§7).
- Que MU-01 a MU-15 den el mismo resultado con el CT-17 definitivo. Solo lo he razonado (M-06).
- La atribución de T-8 en UI prueba por prueba. Solo he comprobado la plausibilidad por clase y por mensaje; el contrato prohíbe correr la suite de UI completa en local.
- Si la selección nueva de CT-16 (el portador de botas) dibuja algo o entra en el BOM.
- Si existen en dibujos reales documentos como `pb-21-cola`.
- Que el máximo de 106,988 de R7 sea exactamente un artefacto de emparejamiento. Lo deduzco de la magnitud y de K1o; no lo he medido.
- No compilé ni ejecuté nada. Todo lo dinámico sale de los TRX y logs custodiados en la entrega.
