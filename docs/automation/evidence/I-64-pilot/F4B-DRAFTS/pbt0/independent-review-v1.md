# I-64 F4B — Revisión adversarial independiente del paquete del contrato de PB-T0 (bloque C), v1

**Revisor:** independiente, adversarial, solo lectura. **Fecha:** 2026-11-17.
**Veredicto:** EMITIBLE TRAS CORRECCIONES — **0 bloqueantes, 4 importantes, 21 menores** (§5).

Notación: `C1:n` es la línea `n` del paquete revisado (`I-64-f4b-pbt0-gate-contract-package.md`, v1, 624 líneas). **W**, **WC**, **WX**, **CS**, **DA** y **WLC** son los alias de C1:35. `<worktree>` es el worktree de la rama `architecture/workspace-persistente-rackcad`.

## 1. Objeto, método y lo que no se pudo verificar

**Objeto.** El paquete C1, contra:
- el paquete de la fase v3.1 (**[P]**), sobre todo §6.1, §6.2, §7.2, §7.3, OB-06.3b, OB-06.4 y OB-06.5;
- la orden literal del Owner (**[OD]**);
- el dato nuevo que la sesión envió durante la revisión: las tres decisiones de procedimiento del Owner del 2026-11-17 (`owner-f4b-procedure-decisions.md`, en la carpeta del paquete). Según la instrucción recibida, que C1 siga el §7.2 viejo **no se cuenta como defecto**; lo que hay que cambiar está en el §3;
- los mapas MA y MB, usados solo como índice;
- el código y las pruebas de `<worktree>` en `67e28452`. `git diff --stat 5ccaa770 67e28452 -- src tests` sale vacío, así que el producto es el de `5ccaa770`.

**Método.**
- Lectura del código y de las pruebas con `git show`, `git grep`, `git ls-tree` y `git log`, y con la herramienta de lectura.
- Tres scripts de Python de solo lectura en la carpeta del informe:
  - `pbt0r_fixtures.py` y `pbt0r_safety.py` leen el corpus `Fixtures/I64`;
  - `pbt0r_blast.py` estima qué pruebas existentes ejercen lo que mutan los mutantes.
- Comparación de la copia de [P] y de [OD] de la carpeta del paquete con las custodiadas en `67e28452`.
- Sin compilar, sin ejecutar pruebas, sin AutoCAD y sin escribir nada en el worktree.

**Lo que no se pudo verificar** (todo se razona sobre el código):
- que cada mutante compile sin avisos (se razona por las firmas y por las reglas del compilador);
- que cada mutante discrimine su CT por aserción (se razona por el flujo del código);
- los valores de base que fijará cada CT;
- el número exacto de pruebas existentes que romperá el RED (solo una estimación con cotas, §2.3);
- el comportamiento de la CI.

## 2. Revisión por puntos

### 2.1 Cobertura del §6.2 de la fase

| Fila de [P] §6.2 | CT de C1 | Juicio |
|---|---|---|
| Propiedad del estado | 01 (pb-20, pb-21, pb-31, X-01) y 02 (los 17 y X-01) | **Cubierta.** La segunda pasada existe (W:3351) y CT-02 la compara con la primera. Los 14 diseños de un sentido no llevan CT-01 porque C1 los da por cubiertos con WLC (C1:124). Pero WLC compara la ventana **con el piloto**, no con el sobre (WLC:84-91, :244-245). La cobertura frente al sobre solo sale de una deducción de dos pruebas: WLC más la identidad piloto↔sobre (`WorkspaceDraftPilotMaterializationTests.cs:682` y `LegacyDeltas`, `:706`; MB H-033). Es defendible, pero C1 no cita el segundo eslabón (M-13) |
| Módulos | 03, y la mitad Core citada (D-PBT0-07) | **Cubierta en UI.** En Core, `PushBackModuleAdoptionTests` y `PushBackCompositeModuleSequenceTests` fijan la reconciliación por `ModuleId`, pero ninguna fija el caso de `Kind` incompatible. `git grep "Incompatible"` solo lo encuentra en `RackModuleEditSessionTests` y `DynamicHeaderBatchReconciliationTests`, que son genéricas (M-14) |
| Cola aparcada | 06, más los ids aparcados de pb-20 en CT-07 | **Cubierta.** H-14 confirmado (WC:838-842) |
| A/B | 07 | **Cubierta** |
| Añadir/quitar frentes | 08, 09 y 10 | **Cubierta.** H-15 confirmado. La regla del último vínculo (`RackPushBackCommands.cs:348-356`, C1:68) no tiene CT ni figura en el §4.3 (M-19) |
| Añadir/quitar módulos y H-PB-28 | 04 y 05 | **Cubierta.** H-10 confirmado: `Reset(editorInputs)` (DA:300-303) actúa sobre los inputs de esa pasada, el portador `advanced` (W:395-400, W:1592-1607, W:1628-1634) no se toca y `ClearModuleCommit()` tras aceptar (W:682) hace que la pasada de Actualizar vuelva a aplicar el portador. Alcance de CT-05: M-07 |
| Renombrar | 11 | **Cubierta.** `Name_Changed` ya recorta (W:1201-1205). MU-10 discrimina porque `RackEditorIdentity.SetName` no recorta (`src/RackCad.UI/Editor/RackEditorIdentity.cs:36-39`) |
| Borrado de vistas (decisión pura) | 10 | **Cubierta.** El cálculo coincide con RACKEDITAR: `BuildFrom` resuelve con `PushBackResolver` (DA:471) y los cortes con `PushBackSystemLateralBuilder.Cortes` (`RackPushBackCommands.cs:269`) |
| Subdiálogos | 12 a 16 | **Cubierta,** con observaciones: CT-13 (M-03), las costuras de CT-14 a CT-16 (I-04) y CT-12/CT-16 (M-18) |
| Dependencias de catálogo y (c)/(d)/nuevas | 17 y T-2 | **Cubierta** |
| `DimensionStyleName` | 18 | **Cubierta, y el recorte es justo.** `PushBackDimensionLegacyCharacterizationTests.cs:249-251` ya fija el estilo nombrado con los constructores que usa el Plugin (`:120-142`). Lo nuevo de CT-18 es el nulo y los estilos del corpus por `BuildFrom`, que cumple OB-06.1 |
| Redondeo campo a campo | 19 y 20 | **Cubierta.** F1-F11 = los campos con decimales de MB H-029/H-030 (W:819, :837-839, :845; W:797-801, :804). G1-G9 = MB H-079. Precisiones en M-09 |
| Efecto del redondeo | 21 | **Incompleta para OB-06.4:** I-03 |
| Censo control → prueba | T-1 | **Cubierta,** como tabla del informe, que es lo que pide [P]:588 |

### 2.2 Cada CT

**Determinismo y reglas.**
- **Catálogo.** No depende de la máquina: sale de la carpeta del ensamblado (`UiSupport.LoadCatalogSafe` → `JsonRackCatalogProvider.FromBaseDirectory()`, `src/RackCad.UI/UiSupport.cs:84-94`). La caché se comparte entre pruebas, pero ningún CT muta el catálogo.
- **Aviso de cierre.** El suelo del aviso de `StaTestRunner.cs:57` responde «sí», así que ningún cierre en `finally` abre un `MessageBox`. Las clases de T1B ya van en `EditorDiscardPromptCollection`, y meter las seis nuevas es seguro.
- **Recálculo síncrono.** `RecomputeGate.Request()` ejecuta en el acto, sin dispatcher (`src/RackCad.UI/Editor/RecomputeGate.cs:35-44`).
- **CT-14 necesita la ventana mostrada.** `new SafetyPerPostWindow(…) { Owner = this }` (W:2716-2721) lanza si la ventana no se ha mostrado. El patrón fuera de pantalla ya existe en `PushBackBootUiFlowTests.cs:38-50`.
- **BOM.** La lectura de D-PBT0-06 (C1:97) es defendible:
  - cada `Recompute` construye el BOM (`PushBackBomBuilder.Build`, DA:472), así que prohibir *calcularlo* haría imposible cualquier prueba de UI;
  - INV-F4T1B-06 prohíbe exportarlo y el gesto `Bom_Click`, no el cálculo.

  Pero la regla de [P]:594 se copia «en su texto», y C1 la reescribe sin dejar la lectura en el invariante (M-12).

**Defectos encontrados:**
- **Costuras fuera de la lista cerrada (I-04).** INV-PBT0-10 (C1:505-507) solo admite las costuras de H-03 (C1:41) y los ayudantes de H-18. CT-14 a CT-16 necesitan otras:
  - `BootSectionForTest` (W:2661), `DefenseSectionForTest` (W:2801) y `RearTopeSectionForTest` (W:2972);
  - el `Button` y el `ModeBox` públicos de las secciones (`PushBackBootSection.cs:252-254`);
  - para «fijar el poste 0» en CT-14, las costuras internas `SafetyPerPostWindow.SetForTest` y `BuildResultForTest` (`src/RackCad.UI/SelectiveSafetyWindow.cs:1043`, `:1052`). Las usan las pruebas existentes (`PushBackSafetyPerPostFlowTests.cs:246-251`), y C1 no las nombra en ningún sitio.
- **La clase (a) está invertida (M-01).** C1 define `Delta(Sobre, Salida)` como «`ruta: x -> y`» (C1:107) y toma «la regla numérica de WLC:213-229» (C1:109). Esa regla comprueba `primero == redondeo(segundo)`, porque en WLC el primero es la ventana (WLC:213-223). Aplicada a `Sobre -> Salida`, una línea de redondeo real (`61.23456 -> 61.235`) sale `OUTSIDE`, y el filtro de D-PBT0-03 no la retira.
- **CT-02 se contradice (M-08).** Pide la precondición `UpdateButton.IsEnabled` (C1:117) y a la vez fija los resultados `BLOQUEADO` e `INVÁLIDO` (C1:130), en los que el botón está deshabilitado.
- **CT-21 no es del todo determinista ni portable (M-10).** No define el orden de las listas K1, K4 y K5, ni la regla para emparejar las «instancias homólogas». Además, K4′ («mayor que 0», C1:322) es una igualdad exacta de coma flotante en una prueba Core que corre en Ubuntu, contra INV-PBT0-11 (C1:508) y R-11.
- **Falta el GUID de `newIdFactory` (M-11).** [P]:598 lo pone en la regla de determinismo, y C1:114 y C1:508 no lo copian.

### 2.3 La tabla cerrada de mutantes

| MU | ¿Existe la línea? | ¿Compila sin avisos nuevos? | ¿Discrimina su CT en aislado? (razonado) |
|---|---|---|---|
| 01 | W:315 sí | Sí: `PalletTolerance` tiene setter (`PushBackEditorInputs.cs:23`) | CT-01: pb-20 y X-01 son compuestos (`PushBackDesign.cs:70`) → +1 en la salida |
| 02 | W:3365 sí, fuera de las guardas W:3325-3363 | Sí (`DynamicRackDesign.cs:36`) | CT-02: `S1` es una cadena tomada antes; la petición sale con +1 |
| 03 | W:2463 sí | Sí | CT-03: el paso 2 deja el delta vacío |
| 04 | DA:211-213 sí | Sí; `standardRestore` sigue en uso (DA:229, :300, :315) | CT-04: sin reconstrucción, la copia del baseline conserva el módulo personalizado y la reconciliación sin intenciones no lo toca (`RackModuleReconciliation.cs:148`) |
| 05 | DA:300 sí; alt. W:2499 sí | Sí | CT-05: en A1 no se reinicia. La alternativa discrimina en A2 |
| 06 | WC:839-840 sí | Sí (`Take(0)` de `System.Linq`) | CT-06: cola aparcada vacía al cargar |
| 07 | CS:453 sí; alt. CS:320-323 sí | Sí. `OtherSideHasAnyFront` queda sin uso, pero IDE0051 no se aplica en el build (`.editorconfig:11`) | CT-07. **Su radio es enorme** (I-02) |
| 08 | CS:524 sí | Sí | CT-08 y CT-09 (`AddFront_Click` → `SetSlotCount`, W:1137-1138). **Radio enorme** (I-02) |
| 09 | `RackViewAvailability.cs:111` sí | Sí | CT-10: el poste que desaparece pasa de `VariantNotPresent` a `Available` |
| 10 | W:3365 sí | Sí | CT-11 |
| 11 | W:1946 sí; `copy` está en ámbito (W:1916) | Sí; `edited` sigue en uso | CT-12 |
| 12 | W:2626 sí | Sí; `Authorize` copia sin pereza (`PushBackSafetyAuthority.cs:94-123`) | CT-13, **por un camino distinto del que describe C1** (M-03) |
| 13 | W:2642 sí | Sí | CT-14 |
| 14 | W:2638 sí | Sí; CS0219 solo aplica a una constante, como dice C1 | CT-15 |
| 15 | W:2653 sí | Sí (`else { }`) | CT-16 |
| 16 | W:407 sí; alt. W:387 sí; `DimensionDetail.Standard` existe (`DimensionDetail.cs:16`) | Sí | CT-17, c-10 (7 → 3 frente a 7 → 2) |
| 17 | `DynamicViewDecorations.cs:402` sí | Sí | CT-18 |
| 18 | W:406 sí; `SetNumber(double?, string format)` (`NumericField.cs:131`) | Sí | CT-19, F5 |
| 19 | W:819 sí, tercer argumento | Sí | CT-20, G2 |
| 20 | `DynamicViewDecorations.cs:329` sí | Sí | CT-21: en un rack de un sentido, `Scale(system)` es el único consumidor de la escala. **La alternativa es inútil:** `PushBackSideAnnotations` devuelve vacío si el rack no es compuesto (`PushBackSideAnnotations.cs:50-53`, `:74-77`), y CT-21 solo usa pb-08, que es de un sentido (M-02) |

- **Clase y reversión.** Todos son de comportamiento, en archivos que el paquete permite. Ninguno toca `Owner`, `ShowDialog`, las comprobaciones de nulo de las costuras (W:1938, :2615, :2722, :2849, :2981, :3018 confirmadas), el constructor ni las guardas de `RequestDraw`. Ninguno añade `.RequestUpdate(` ni los literales de T-SG. La reversión con `git restore --source=BaseSha` es exacta.
- **CT sin mutante:** ninguno. Los 21 tienen al menos un mutante que los discrimina en aislado. Las anclas sin mutante quedan para T-6.

**Las pruebas existentes que rompería el RED (R-02).** Es la debilidad mayor del paquete.
- **MU-07** cambia `SetSlotPresent`, y esa función no solo la usa el gesto. También la usan:
  - la carga de cualquier compuesto o diseño con `AbsentSlots` (CS:1003-1008);
  - `AlignSlotGrid` (CS:568-573);
  - todo crecimiento en un compuesto (CS:542).

  El resultado es que un lado B declarado ya no puede quedar entero en blanco en ningún camino, que es la regla D-PB-08.
- **MU-08** altera todo crecimiento por `SetSlotCount`, que usan los ayudantes de construcción de racks de las pruebas.
- **Cotas** (`pbt0r_blast.py` y `git grep`):
  - `SetSlotCount(` aparece en 52 archivos de prueba con 699 métodos;
  - `SetSlotPresent(` aparece en 58 archivos con 842 métodos;
  - la costura `SafetyDialog =` (MU-12 a MU-15) aparece en 6 archivos con 89 métodos;
  - `ConfirmModuleButton` (MU-03) aparece en 29 métodos de UI;
  - las cargas con `LoadExisting` en UI (MU-01 y MU-06) son 66 métodos.
- **Estimación:** la unión del RED será **del orden de cientos de resultados**, frente a los 3 de T1B ([T1B-X]:36).
- **El criterio mecánico sigue siendo viable,** porque solo mira los TRX de los dos filtros nuevos. Lo que no es viable es lo siguiente:
  - **la atribución tal como está escrita (I-02).** C1:421 pide atribuir por la corrida aislada de cada mutante, pero INV-PBT0-15 solo corre los dos filtros (C1:517) e INV-PBT0-20 prohíbe correr en local las suites completas de UI (C1:533). Atribuir así las existentes de UI es imposible. Al Worker le quedan tres salidas: desviarse, parar por S-03 o correr la suite completa de UI, que reproduce F4-E59/I-02 y haría fallar la prueba de la limpieza;
  - **la regla de la excepción (I-01).** C1:418 invalida el mutante que haga fallar «una prueba» por excepción, «solo o en la conjunción», y C1:482 la juzga sobre el RED, donde corren también las existentes. Con cientos de existentes rotas, alguna fallará por excepción (`First`/`Single` sobre una selección que ya no existe, por ejemplo). MU-06, MU-08, MU-12, MU-13, MU-14 y MU-15 no tienen alternativa, así que eso lleva a PBT0-C10. T1B formulaba la regla solo sobre las pruebas **nuevas** ([T1B]:254, «todas las pruebas nuevas fallan por aserción»).

### 2.4 El contrato

- **`AllowedWriteScope`** (C1:436-440). Las 17 rutas bastan. Sobra `PushBackSideAnnotations.cs`, que solo sirve a una alternativa inútil (M-02). Los diez archivos de prueba y los nombres de clase son coherentes con `RequiredTests`.
- **`ForbiddenWriteScope`** (C1:442-455).
  - **`src/`.** El método reproduce el de T1B. Lo comprobé con `git ls-tree`: en `Application/Systems/` solo `PushBack/` tiene un subdirectorio (`Pilot/`), y `Dynamic/` y `Shared/` no tienen ninguno. En `UI/Systems/PushBack/` hay diez archivos, ocho de ellos no permitidos. La raíz que lista C1:455 coincide con la de `5ccaa770`.
  - **`tests/`.** Es una lista a mano, como en T1B, no exhaustiva. La cierran el `AllowedWriteScope` exacto e INV-PBT0-01. Cubre T1B (las siete clases), los ayudantes y `Fixtures/`.
  - **Defectos.** Los cinco archivos de C1:453 van sin carpeta de proyecto (M-16), y el «método de `make_t1b_contract.py`» (C1:444) cita un script que no está versionado (M-06).
- **Invariantes.** Casi todos se verifican mecánicamente con git o con el TRX. Son de juicio INV-PBT0-04 (medido, no inferido) e INV-PBT0-19 (no duplicación), como en T1B. INV-PBT0-10 contradice a los CT (I-04).
- **`RequiredTests`** (C1:457-464):
  - **Core:** 3 (CT-10, CT-18 y CT-21);
  - **UI:** 18 = State (01, 02, 11) + Module (03-05) + Composite (06-09) + Subdialog (12-16) + Catalog (17) + RoundingLoss (19, 20);
  - **colisiones:** ninguna. `git grep PushBackFull` en `src` y `tests` da 0 (H-21), y el filtro de Core no contiene «UI.». El ayudante no tiene pruebas.
- **Paradas.** Con el §7.2 viejo, C08, C11 y C12 lo cubrían. Con las decisiones del Owner cambian (§3).
- **Celdas.** Siguen vigentes hasta el 2026-12-29 (`model-catalog.md:14`; Sonnet en `:48-61` y gpt-6-luna en `:94-108`, las dos con verificación del 2026-09-30, en `:51` y `:97`). Dos imprecisiones (M-05):
  - C1:600 dice que las dos celdas de [T1B-C] están verificadas el 2026-09-30, pero el Controller lleva `VerifiedOn` 2026-11-11 en [T1B-C]:513;
  - el texto de P-01 que se copia de [T1B-C] nombra los Snapshots «A, B, C o D», y T1B corrió con el Snapshot I.

### 2.5 Guardas existentes

Ninguna guarda falla con los archivos nuevos ni con los mutantes:

| Guarda | Resultado |
|---|---|
| Un solo `namespace` (`NamespaceFolderGuardTests.cs:211-245`) | Exige una declaración por archivo que empiece en la columna 0 e igual a la raíz del proyecto |
| INV-14 y los literales partidos (`StructuralAnalysisSourceGuardTests.cs:262-268`, `:416-419`, `:430-443`; `…HardeningGuardTests.cs:85-92`) | La expresión `python\|bentley` no distingue mayúsculas. Ni el código ni los comentarios pueden usar esas palabras |
| El corpus de WLC (WLC:323-332) | Se cumple sin fixtures nuevos |
| G-10 (`WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`) | Ningún mutante añade `.RequestUpdate(` |
| T-SG (MB H-129) | MU-11 toca `StageHeaderConfiguration`, no `return window.Configuration ?? copy;` |
| T-KB, T-OW y T-RS | No hay mutantes en el XAML |
| `I55DeferredProductFixCharacterizationTests.cs:8-15` | Sin cambio |
| `PushBackModuleEditorCharacterizationTests.cs:382-400` | `RequestStandardRestore()` y `StandardRestoreRequested` siguen presentes con MU-05 y con su alternativa |
| `BlankFrontDesviadorHandoffTests.cs:312-318` | Sin cambio |
| `ProductPolicyIndependenceGuardTests.cs:8-18` y `WorkspacePilotPrepareGuardTests.cs:388-404` | MU-09 no añade ningún token prohibido |

H-19 (C1:75-80) no lista las cinco últimas filas (M-17).

### 2.6 Lecciones de T1B

- **El fallo estructural de nc2.** R-01 estaba bien planteado con el §7.2 viejo: el modo de fallo es del Controller, no del paquete, y la única salida era el Owner, que ya decidió. **Riesgo residual:** en F4-T3B1, nc2 dio la firma de PD-F4-01 «con Ci y Trailer en fail además» (`f4-dispositions.md:1227`). Con la decisión del Owner («cualquier otra firma o variación vuelve a ti»), eso ya no es la firma exacta (§3, OD-3).
- **La atribución de las existentes rotas.** No está bien aplicada (I-02).
- **F4-E59.** La regla de I-02 (sin suites completas de UI en local) y la de I-03 (sin sondas con entradas válidas en caminos con diálogo o escritura) están en INV-PBT0-20. Pero C1:421 empuja a incumplir la primera.
- **MAX_PATH.** Bien aplicada: sin copias de árboles y mutantes en sitio.
- **Teclado y foco.** Ningún CT usa gestos de teclado ni foco real (`LostFocus` por `RaiseEvent`), pero C1 no lo declara. Tampoco copia la regla de INV-F4T1B-04: «el código de medición es temporal y no entra en ningún commit» (M-21).
- **La adenda de anclas.** T-6 es obligatoria y la tabla da línea y parche. Bien aplicada.

### 2.7 Anclas

Comprobé unas 110 anclas `ruta:línea` de C1, y casi todas son exactas:
- **la ventana:** W:128-153, :157-185, :247-256, :303-427, :601-702, :812-857, :1493-1529, :1592-1634, :1938-1946, :2002, :2410, :2454-2501, :2534-2659, :2700-2722, :3028, :3292-3379, :3417 y :3487; WC:43 y :838-842; WX:569;
- **DA:** :204, :211-213, :229-231, :300-303, :315-317 y :466-474;
- **CS:** :316-323, :428-437, :453 y :524;
- **los cortes y RACKEDITAR:** `RackPushBackCommands.cs:268-271`, `:324-333` y `:348-356`; `RackViewAvailability.cs:106-115` y `:136-146`; `RackViewCodec.cs:115`; `RackEmbedDocument.cs:20` y `:29`;
- **el resto de Application:** `PushBackEditorComputation.cs:47-67`, `HeaderRunPlan.cs:31`, `DynamicRackSystemResolver.cs:246`, `DynamicViewDecorations.cs:117`, `:206`, `:285`, `:328-329` y `:402`, `PushBackSideAnnotations.cs:55` y `:79`, `PushBackEditorState.Load.cs:192-210` y `:229-232`, `DynamicRackLevelGeometry.cs:172-174` y `NumericField.cs:131-136`;
- **las pruebas:** StaTestRunner, el ayudante y la colección; las guardas de H-19; WLC; `PushBackDimensionLegacyCharacterizationTests.cs:35`, `:44`, `:52` y `:249-251`; `BlankFrontBoundaryTests.cs:223`, `:235` y `:310`; `PushBackDimensionViewsWindowTests.cs:309`; `PushBackViewTests.cs:152` (Core); `PushBackCompositeWindowTests.cs:82`, `:610`, `:1121` y `:1366`;
- **la documentación:** [T1B] :246-374 y :486-622; [T1B-C] :301-313, :328-345, :371-439 y :481-515; [T1B-X] :56-57 y :85-136; `f4-dispositions.md:2047`; `AUTOMATION_PLAN.md` §16.5, §16.8, §16.9 y §16.11; `model-catalog.md`; Proposal V5:557; ADR-0029:172-180; [OD]:157-177 y :259-268;
- **el corpus (H-13):** pb-01, 05, 06, 07, 08, 20, 21 y 31, comprobado con los scripts.

**Desviaciones:**
- **Todas las anclas de [P] apuntan a la copia de la carpeta del paquete** (C1:5, C1:23-26, C1:92, C1:97, C1:607 y C1:624). La copia custodiada en `docs/initiatives/I-64-f4b-push-back-completo-package.md` (`67e28452`) tiene cuatro líneas más al principio: la nota de redacción y la de ubicación. Contra la autoridad que nombrará el contrato, todas quedan desplazadas en +4. Por ejemplo, el §6.2 está en `:561-605`. [OD] es idéntica (M-04).
- **Desplazamientos menores:**
  - CS:294-327: el método termina en `:324`;
  - CS:522-545: termina en `:544`;
  - W:2715-2721: la construcción empieza en `:2716`;
  - `HeaderBlockInstance.cs:59-101` existe, pero H-16 omite `MirroredX`, `MirroredY` y `DynamicParameters` (I-03);
  - el `VerifiedOn` del Controller (M-05).
- **Ancla no verificable:** `make_t1b_contract.py` (M-06).

### 2.8 La vuelta al Owner

Ninguna decisión **nueva** de producto ni de procedimiento tiene que ir al Owner antes de emitir:
- las decisiones D-PBT0-01 a D-PBT0-07 caben en [P] §6.2 y en el patrón de T1B;
- la lectura del BOM es una interpretación declarada y defendible;
- R-01 y la operación del bloque M ya los resolvió el Owner (§3);
- las correcciones que propone esta revisión (§4) no deciden nada nuevo: restringir la regla de la excepción a los métodos nuevos, atribuir por análisis declarado como en T1B, reubicar mutantes, ampliar K4 y completar la lista de costuras. Quedan dentro de [P]:601 y de [T1B] D-T1B-03.

**Lo que sí debe estar antes de emitir:**
- la custodia de `owner-f4b-procedure-decisions.md`, con su anotación en `f4b-dispositions.md`;
- los cambios del §3.

Las firmas de los sustitutos deben copiarse **literalmente** de sus fuentes. Si una decisión del Owner se cita por nombre de check, se generaliza, y eso volvería a ser una decisión sin autoridad.

## 3. Lo que debe cambiar por las decisiones del Owner del 2026-11-17

Estos cambios no cuentan en el recuento B/I/M.

| Id | Dónde | Cambio |
|---|---|---|
| OD-1 | C1:14, §1 (C1:18-31) | Custodiar `owner-f4b-procedure-decisions.md` (por ejemplo, en `F4B-DRAFTS/coordinator/`) y anotarlo en `f4b-dispositions.md`. Añadirlo a las autoridades: sustituye a [P] §7.2 solo en (i) la replanificación y (ii) los sustitutos de nc1 y nc2. La pregunta de [P] §10 queda cerrada. Entra en la `AuthorityRevision` |
| OD-2 | PBT0-C08 (C1:589) e INV-PBT0-23 (C1:543) | Un rechazo de la planificación **solo** por la comparación literal de las colecciones permite **una** replanificación con el prompt reforzado, sin gastar intento. Un segundo rechazo de fidelidad es STOP y **vuelve al Owner**, no al Coordinator como decía F4T1B-C08. Cualquier otro rechazo de la planificación (A1-A8 u otra causa) sigue siendo STOP y vuelve al Owner, porque la decisión no lo extiende |
| OD-3 | PBT0-C11 (C1:592) e INV-PBT0-23 (C1:544) | Un nc1 con la firma **exacta** de PD-F4-04 → el sustituto de la señal de identidad, copia de `F4T3C2-NC1-IDENTITY-SIGNAL-01` con solo el identificador y el ámbito cambiados. Un nc2 con la firma **exacta** de PD-F4-01 → el sustituto de lista blanca exacta, copia de `F4T1B-NC2-EXACT-WHITELIST-01` con solo el identificador y el ámbito cambiados. En los dos casos se conserva la salida ordinaria, se registra `SATISFIED_UNDER_<id>` y se anota cada uso. Las firmas se transcriben literales de su fuente: PD-F4-01 de `f4-dispositions.md:1103-1107`, más «ninguna otra comprobación distinta de la verificación real» (`:1540`); PD-F4-04 de `owner-f4t3-authorization.md`, tal como se aplicó en `:1656-1660`. **Cualquier otra firma o variación vuelve al Owner antes de un sustituto:** PD-F4-05, PD-F4-06, nc2 con Ci o Trailer en fail además (el caso de `:1227`), cualquier fallo de nc3, o un nc1 cuya primera comprobación en fail no sea la de la firma |
| OD-4 | `ExpectedEvidence` (C1:572) | Quitar «sin sustitutos» y remitir a OD-3. **No copiar** el texto de [T1B-C]:368 («PD-F4-04 o PD-F4-05 para nc1, PD-F4-06 para Ci o Trailer»), porque el Owner no extendió PD-F4-05 ni PD-F4-06 |
| OD-5 | R-01 (C1:607) y §8 (C1:620-624) | La pregunta heredada está respondida. R-01 pasa a ser: «nc2 con la firma exacta de PD-F4-01 → sustituto; con cualquier variación → Owner» |
| OD-6 | INV-PBT0-20 (C1:536) e INV-PBT0-24 (C1:546) | El bloque M lo opera la sesión por control de pantalla. Hay que declarar que **no se opera mientras el Worker corre en local los filtros de UI**: CT-14 muestra una ventana, y las corridas exigen que nadie use el equipo |

## 4. Tabla de hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| I-01 | IMPORTANTE | La regla de la excepción vale para «una prueba», «solo o en la conjunción», y se juzga sobre el RED, así que alcanza a las existentes. Con la unión prevista (I-02), invalidaría mutantes sin alternativa (06, 08, 12-15) y llevaría a PBT0-C10 | C1:418, C1:482, C1:591, C1:609; [T1B]:254 | Restringirla a **los métodos de los filtros nuevos**. Para las existentes, que solo cuenten el cuelgue y el timeout (la regla (c) de T1B) |
| I-02 | IMPORTANTE | La atribución «por la corrida aislada» es imposible para las existentes de UI, porque INV-PBT0-15 solo corre los filtros e INV-PBT0-20 prohíbe las suites completas de UI en local. Empuja a repetir F4-E59/I-02. MU-07 y MU-08 rompen además cientos de pruebas existentes (§2.3) | C1:421, C1:517, C1:533, C1:557, C1:608; CS:542, :568-573, :1003-1008 | (a) Atribuir **por análisis declarado**, como [T1B]:257, con corrida aislada de la **suite completa de Core** por mutante y la de UI solo en la CI. (b) Reducir el radio: MU-08 → W:1138 (`state.Structure.Count + 1` → `+ 2`), que discrimina CT-08 y CT-09 (CT-10 ya tiene MU-09); MU-07 → su alternativa CS:320-323 como principal, mucho más estrecha. (c) Dar en R-02 la estimación |
| I-03 | IMPORTANTE | K1-K5 de CT-21 ignoran `DynamicParameters` (largos y alturas de los bloques dinámicos que dibuja el Plugin). Un cambio de geometría hecho solo por parámetro no entra en «material» ni en el máximo \|Δ\|, así que el dato que va al Owner por OB-06.4 puede infravalorar el efecto | C1:69, C1:316-324; `HeaderBlockInstance.cs:86`; MA H-069 (parámetros dinámicos) | K4 y K4′ deben incluir la diferencia de cada `DynamicParameters[clave]` de la instancia homóloga, y K1 el conjunto de claves de parámetros. Si se quiere, también `MirroredX` y `MirroredY` en K1 |
| I-04 | IMPORTANTE | INV-PBT0-10 cierra las costuras a H-03, pero CT-14 a CT-16 necesitan `*SectionForTest` y CT-14 necesita las costuras internas `SafetyPerPostWindow.SetForTest` y `BuildResultForTest`, que C1 no nombra. Un Worker estricto pararía por PBT0-C01; uno laxo incumpliría el invariante | C1:41, C1:237-253, C1:505-507; W:2661, :2801, :2972; `SelectiveSafetyWindow.cs:1043`, `:1052` | Añadir a H-03 y a T-7 esas costuras y los miembros públicos de las secciones (`Button`, `ModeBox`), con su uso en CT-14 a CT-16 |
| M-01 | MENOR | La regla de la clase (a), copiada de WLC, está invertida para `Delta(Sobre, Salida)` | C1:107-112; WLC:213-223 | Para `x -> y`, (a) es `y == redondeo(x)`. Escribirlo así y no copiar `ClassOf` |
| M-02 | MENOR | La alternativa de MU-20 no puede discriminar CT-21, porque `PushBackSideAnnotations` solo actúa en compuestos y CT-21 usa pb-08. `PushBackSideAnnotations.cs` sobra en `MutantScope` y en `AllowedWriteScope` | C1:382, C1:407, C1:440 | Quitar la alternativa y el archivo (16 rutas), o añadir a CT-21 una fila compuesta (pb-20) |
| M-03 | MENOR | En CT-13, quitar `DEFENSA_MONTACARGAS` de la lista devuelta no la quita: la sección la recrea (W:2555-2558, :2927-2959) con `Side` y `AuthoredSide` en `None`, y `SafetySelections.Count` sigue en 2. MU-12 discrimina solo por esos dos campos (1/3 frente a 0/0) | C1:231-235; `PushBackSafetyAuthority.cs:239-255` | Declarar el mecanismo y fijarlo como hallazgo de caracterización. Para la «quita declarada», usar una familia que sí viaje en la lista |
| M-04 | MENOR | Las anclas de [P] apuntan a la copia de la carpeta del paquete. Contra la custodiada están desplazadas +4 | C1:5, C1:23-26, C1:92, C1:97, C1:607, C1:624 | Reanclar a `docs/initiatives/I-64-f4b-push-back-completo-package.md` |
| M-05 | MENOR | Anclas imprecisas y celdas: CS:294-327 (→ `:324`), CS:522-545 (→ `:544`), W:2715 (→ `:2716`). C1:600 contradice a [T1B-C]:513 (`VerifiedOn` 2026-11-11). P-01 nombra «Snapshot A, B, C or D» | C1:41, C1:53, C1:67, C1:576, C1:597-600 | Corregir. Fijar en P-01 el Snapshot vigente aceptado |
| M-06 | MENOR | `make_t1b_contract.py` no está en el repositorio: es un ancla no verificable | C1:444 | Describir el algoritmo en el paquete o custodiar el script con la emisión |
| M-07 | MENOR | CT-05 no dice que A0-A2 fijen **solo** los seis campos. pb-07 lleva `DimensionStyle` SIMPLEX: si se fija la salida entera, PB-TC rompería CT-05, contra OB-06.3b. Además pb-07 solo tiene un avanzado no por defecto, y pb-06 tiene cuatro (150, 18, 3 y sin refuerzo) | C1:63, C1:158-165; fixtures (`pbt0r_fixtures.py`) | Restringir A0-A2 a los seis campos. Usar pb-06, o pb-06 y pb-07 |
| M-08 | MENOR | CT-02 exige `UpdateButton.IsEnabled` y a la vez fija `BLOQUEADO` e `INVÁLIDO`, con el botón deshabilitado | C1:117, C1:128-131 | Fijar `IsEnabled` como parte de la tupla y pulsar solo si está habilitado, o declararlo |
| M-09 | MENOR | En CT-20, «el primer módulo con longitud» es ambiguo (en pb-31 todos tienen `Length` y M3 ya es manual). Las bases inventadas de G4-G6 sobre pb-01 (150, 30) pueden dar `NO-VÁLIDA` y perder la fila | C1:304-306 | Nombrar el `ModuleId`. Para G5 y G6, usar los valores reales de pb-06 (150 y 18) |
| M-10 | MENOR | CT-21: sin orden de las listas K, sin regla de emparejamiento de homólogas, y K4′ con umbral 0 en Core (Ubuntu). La T-3 incluye «material» y INV-22 dice «no juzga» | C1:316-325, C1:364, C1:508, C1:541 | Orden ordinal de las claves. Emparejar por índice en `Flatten().Instances` cuando K1 coincide. K4′ solo informativo (o con ε 1e-9). Decir que «material» es la fórmula mecánica |
| M-11 | MENOR | La regla de determinismo de [P]:598 incluye el GUID de `newIdFactory`, y C1 no la copia | C1:114, C1:508 | Copiarla, o declarar que ningún flujo acuña un GUID y asertar `RackId` |
| M-12 | MENOR | [P]:594 se copia «en su texto», pero la lectura del BOM solo está en D-PBT0-06 | C1:97, C1:493-497 | Poner en INV-PBT0-06 el texto de [P]:594 con la lectura declarada |
| M-13 | MENOR | «No duplica WLC» se apoya en una deducción de dos pruebas que C1 no cita | C1:82, C1:124 | Citar `WorkspaceDraftPilotMaterializationTests.cs:682` y `LegacyDeltas`, y MB H-033 |
| M-14 | MENOR | En la mitad Core de «Módulos», el caso de `Kind` incompatible no lo fija ninguna prueba de Push Back | C1:85, C1:98 | Citar `RackModuleEditSessionTests`, o hacer que CT-03 o CT-04 fijen un `Incompatible` |
| M-15 | MENOR | R-09 aplica OB-T1B.3 por analogía a las pruebas de PB-T0 | C1:615 | Dejar la regla al paquete de PB-T1 y quitar la cita |
| M-16 | MENOR | `ForbiddenWriteScope` nombra cinco archivos sin carpeta de proyecto | C1:453 | Rutas completas |
| M-17 | MENOR | H-19 no lista todas las guardas de texto sobre la ventana y sus archivos | C1:75-80; §2.5 | Añadir T-KB, T-OW, T-RS, `I55DeferredProductFix…`, `PushBackModuleEditorCharacterizationTests`, `BlankFrontDesviadorHandoffTests` y `ProductPolicyIndependenceGuardTests` |
| M-18 | MENOR | CT-12 no dice cómo clonar (no hay `Clone` en Domain). CT-16 no dice qué devuelve `SafetyDialog` | C1:222, C1:251 | Usar `RackFrameProjectStore.DeepCopy` (`Persistence/RackFrameProjectStore.cs:87`). CT-16: «la lista recibida sin cambios» |
| M-19 | MENOR | La regla del último vínculo se cita en H-15, pero ni CT-10 ni el §4.3 la cubren | C1:68, C1:198-209, C1:328-335 | Añadirla al §4.3: M-2 y las guardas MA H-121 |
| M-20 | MENOR | La precondición 2 lista como pendiente una custodia que ya está en `67e28452`, y le faltan las decisiones del Owner y este paquete | C1:14 | Actualizar (OD-1) |
| M-21 | MENOR | Faltan dos lecciones de T1B: «el código de medición es temporal y no entra en ningún commit» y la declaración de que no hay gestos de teclado ni foco real | C1:488-491; [T1B-C]:332 | Añadirlas a INV-PBT0-04 |

## 5. Veredicto

**EMITIBLE TRAS CORRECCIONES — 0 BLOQUEANTES, 4 IMPORTANTES y 21 MENORES.**

- **Lo que está bien.** Los CT cubren todas las filas del §6.2. Los recortes son justos, con las citas de M-13 y M-14. Las anclas son exactas en su gran mayoría. Los 20 mutantes existen, compilan y discriminan su CT por el código.
- **Las cuatro importantes:**
  - I-01, la regla de la excepción;
  - I-02, la atribución y el radio de MU-07 y MU-08;
  - I-03, K sin `DynamicParameters`;
  - I-04, las costuras fuera de la lista cerrada.

  Las cuatro tienen corrección textual dentro de [P] y del patrón de T1B. Hay que resolverlas antes de emitir, porque la autorización condicional exige 0 importantes sin resolver.
- **Lo que debe incorporarse** (§3, OD-1 a OD-6): las tres decisiones del Owner del 2026-11-17, con las firmas exactas transcritas y sin generalizar por nombre de check.
- **Vuelta al Owner antes de emitir: ninguna,** siempre que las correcciones se hagan como se proponen. Las tres cuestiones de procedimiento abiertas ya las decidió el Owner.
- **Recomendación:** una comprobación focalizada de la v2 por el mismo revisor antes de emitirla.
