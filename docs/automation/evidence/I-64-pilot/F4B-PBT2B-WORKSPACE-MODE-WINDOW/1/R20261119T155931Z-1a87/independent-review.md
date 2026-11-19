# I-64 F4B PB-T2b — Revisión adversarial independiente del código (intento 1)

**Objeto:** `git diff 59839c7148fa101c2f95e1f59b36e6b052216680 03b16c88c01ebfd29a642fc6a9324eb76926bcba`, con el RED `a3b86134227e716825c4fbf72c4efba03b819351`, la reparación del RED (RR) `6bc3497fc5962d46a3a8541f34004bfd8a7848be` y el GREEN `03b16c88c01ebfd29a642fc6a9324eb76926bcba`. WorkRunId `R20261119T155931Z-1a87`.

**Método:** solo lectura. El código y los documentos, por objetos git desde el checkout principal (`git show`, `diff`, `grep`, `ls-tree`). La entrega, en la copia `rv/` (handoff, relevo, TRX, logs, parches). Guiones de Python propios, de solo lectura, escritos con la herramienta de archivos y lanzados con `python -I`. No se compiló ni se ejecutó ninguna prueba, ni se leyó el worktree de la rama.

Abreviaturas: **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`; **AC** = `tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs`; **MT** = `tests/RackCad.UI.Tests/PushBackWorkspaceModeTests.cs`; **SUP** = `tests/RackCad.UI.Tests/PushBackWorkspaceSupport.cs`; **SG** = `tests/RackCad.Tests/PushBackWorkspaceModeSourceGuardTests.cs`; **SR** = `tests/RackCad.Tests/PushBackWorkspaceSharedReferenceGuardTests.cs`. Las líneas son de `03b16c88` salvo que se diga otra cosa.

## Veredicto

**ACEPTABLE CON MENORES.**

**Recuento: B 0 / I 0 / M 6. Más un hallazgo OWNER (OWNER-01).**

En resumen:
- El producto sigue el paquete v2.8 casi carácter a carácter: el modo, la factoría, el constructor privado y su orden de carga, la extracción, «Aplicar», «Cerrar», la guarda, los rótulos y las dos costuras.
- CLASSIC no cambia.
- D2 se respeta.
- El RED, la RR y el GREEN cumplen el régimen, y la CI 4/4 tiene los recuentos previstos.
- Los menores son de fidelidad o de robustez de las pruebas y de la evidencia. Ninguno cambia un comportamiento ni deja una línea esperada sin cumplir.

## Tabla de hallazgos

| Id | Sev. | Dónde (sha) | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-01 | M | SG:238, :328-329, :343-344 (`03b16c88`); W:3266 y W:3348 | **Tres comprobaciones de CT-PBT2B-08 no prueban lo que dicen.**<br>(a) **R11b:** la autoprueba quita la PRIMERA aparición de `if (!ApplyEditsToEditingState(…)) return;` del archivo. Esa aparición está en `ApplyToDraft` (W:3266), no en `RequestDraw` (W:3348), que es lo que fija el §5.3 («la llamada … quitada de `RequestDraw`»). Da `falla`, pero por quitarla de `ApplyToDraft`.<br>(b) **R6:** la autoprueba añade `bool workspace = false` al constructor PÚBLICO. Eso rompe a la vez la cláusula de la firma literal, así que `autoprueba-R6=falla` saldría aunque la cláusula del parámetro booleano no existiera.<br>(c) **R5:** cuenta las asignaciones con `(?<![\w.])mode\s*=`, que no ve `this.mode = …`. Como `mode` es `readonly`, esa forma solo compila en otro constructor, pero se saltaría el «sin otra asignación» del §5.3.<br>Las reglas son correctas por inspección: R11 compara conjuntos y vería la retirada en `RequestDraw`, y la expresión de R6 casa `bool workspace =`. No hay pérdida de cobertura sobre el GREEN; la evidencia de las autopruebas está mal dirigida en dos casos | En la próxima corrección que toque SG, o en PB-T2c si el régimen lo permite: anclar R11b en `void RequestDraw(`, inyectar R6 en el constructor privado y contar también `this.mode =` en R5. No es motivo de rework por sí solo |
| M-02 | M | AC:521-541 y AC:570-574 frente a `59839c71`:`tests/RackCad.UI.Tests/PushBackWindowOnStateTests.cs:578-587` | **El `Vigia` de CT-PBT2B-05 captura con menos frecuencia que su original.**<br>- El §5.3 pide capturar «justo tras la carga y tras cada paso, clics incluidos», con el `Vigia` de CT-PBT1C-02. Allí `Correr` captura tras CADA paso del gesto.<br>- Aquí `calculo-invalido` (tres acciones: el conmutador, la casilla y `SetNumberR(…, 100000)`) y `bloqueante` (`SetGap(-1)` y el `LostFocus`) son una sola lambda, y se captura una vez al final.<br>- **El riesgo:** una referencia de D2 adoptada por un recálculo intermedio y mutada por el paso inválido siguiente quedaría fuera, que es justo la condición 4 de D2.<br>- **Por qué es menor:** el camino de código es el mismo `Recompute` que CT-PBT1C-02 ya vigila paso a paso en CLASSIC, y «paso = gesto» es una lectura posible del §5.3 | Para PB-T2c y siguientes: un gesto con varios pasos se captura tras cada uno, como `Correr` |
| M-03 | M | Handoff `Deviations[1]`; `rv/tests/g0/diag04*.log`; `TestsExecuted` 138-142 | **Las cinco corridas de diagnóstico no siguen el régimen de INV-PBT2B-10.** Son corridas temporales (la clase de prueba instrumentada, fuera de todo commit) y:<br>- no guardan TRX, solo log;<br>- al menos `diag04` y `diag04b` terminan en un `Assert.Equal` modificado (cadenas unidas con ` ## `), no en `Assert.Fail`;<br>- el filtro, `FullyQualifiedName~PushBackWorkspaceApplyCloseTests.CT_PBT2B_04`, no es literalmente uno del contrato, aunque es un subconjunto de uno: no corrió ninguna prueba fuera de los filtros del contrato.<br>Están declaradas y no tienen efecto en ningún commit, en el producto ni en la CI. El juicio completo, con 16.8, RR y el árbol, está en el apartado «Las corridas de diagnóstico» | Registrar en las disposiciones. En adelante, una corrida temporal de diagnóstico guarda TRX y termina en `Assert.Fail` |
| M-04 | M | `rv/inv/check_inv.py:222-226`; `rv/inv/check-inv-final.txt` («base=27») | **La evidencia de INV-PBT2B-06 cuenta 27 costuras, y el contrato dice 41.** La expresión del guion solo casa declaraciones `internal` de una línea. No ve, por ejemplo, `CurrentPreviewPlan`, `SelectMatrixCell`, `PendingWork`, `LateralViewOptions` ni la de WC. El handoff no concilia la diferencia.<br>**En lo material se cumple.** Medido con `git show` en `59839c71`, W tiene 40 declaraciones `internal` y WC tiene 1, `CompositeState`: son las 41. En `03b16c88` siguen todas. `PendingWork` cambia de cuerpo con el mismo nombre y el mismo tipo, WC no cambia, y solo se añaden `ModeForTest`, `BomDialog` y `LibraryPathDialog` | Que la verificación lo mida sobre las 41. Basta con una nota |
| M-05 | M | SUP:328-336; MT:183-184; AC:342 y :351 | **Un centinela compartido en `diseno-igual`.** `DisenoDe` y `DisenoDeVentana` devuelven el mismo texto, `<sin diseno>`, cuando no hay diseño. Si fallaran los dos, `diseno-igual` daría `True`. El §5.3 escribe `Persist(w.LastComputation.Design)`, que habría dado una línea «EXCEPCION».<br>Con los datos de hoy no se ejerce, porque todas las filas con `diseno-igual` son válidas, y ninguna línea esperada pasa por ahí | En PB-T2c, que reutiliza SUP sin editarlo: no comparar dos centinelas, o leer antes `valido` |
| M-06 | M | §5.0 del paquete (`Sondeo`) frente al §5.3; SUP:275-281; AC:88 | **El texto del paquete no es coherente sobre la sonda.**<br>- El §5.0 dice que «la primera línea de cada ventana de WORKSPACE es `modo=<w.ModeForTest>`».<br>- Las listas literales del §5.3 solo la llevan en CT-03 (pasada 1) y como campo de CT-01 (ii); CT-02, 04, 05, 06 y 07 y las pasadas 2 y 3 de CT-03 no.<br>- El Worker siguió las listas literales, como manda C07. Su `Sondeo` solo deja `modo=` cuando el modo no es `Workspace`, y para la prueba sin más gestos. No lo declaró como lectura.<br>Sin efecto en esta entrega: en el RED todas fallan por la línea `modo=Classic` | Que la sesión aclare el §5.0 antes de PB-T2c, que reutiliza `Sondeo` |
| OWNER-01 | OWNER | W:3427-3433 (código literal de D-PBT2-07) | **Con un módulo escenificado y otros cambios sin aplicar, «Cerrar» solo hace la pregunta del módulo.** Contestar «Sí» pierde también lo no aplicado al borrador, sin nombrarlo. Ver el apartado OWNER | Informar al Owner. No es un defecto del Worker |

## Las corridas de diagnóstico (el punto que se suma al 7)

**Lo que declara el Worker** (`Deviations[1]`):
- cinco corridas locales sobre CT-PBT2B-04, con el GREEN aplicado en el árbol y una instrumentación temporal de AC;
- revertida cada vez con una copia de seguridad, fuera de todo commit.

**Lo que se pudo comprobar:**
- **Línea de tiempo** (los TRX y los commits):
  - el commit RED, a las 17:00:18Z; su CI empieza a las 17:01:26Z, así que el push es anterior;
  - el GREEN, aún sin commit, corre en `g0` a las 17:09-17:10Z, y CT-04 falla con `descartes=0` en lugar de 1 (`rv/tests/g0/PushBackWorkspaceApplyCloseTests.trx`);
  - después, las cinco corridas `diag04*`: sus logs no llevan hora, pero el orden se deduce;
  - el criterio del RED sobre la reparación (`rrl`, `rru` y `rrf`), de 17:19Z a 17:46Z;
  - el commit RR, a las 17:47:58Z, con su CI a las 17:48:48Z;
  - el GREEN reaplicado (`g1u` desde 17:49Z) y su commit a las 18:16:20Z.
- **Qué hacía la instrumentación** (los logs):
  - `diag04` compara cadenas unidas con ` ## ` (AC:471, la misma línea que el `Assert.Equal` de CT-04 en `a3b86134`);
  - `diag04b` añade una línea `DIAG pending=… tol=… asked=…`;
  - `diag04c` y `diag04d` vuelcan el log en el mensaje, con `DIAG may=… q=…`;
  - `diag04e` añade contadores `D0..D3`.
- **Ningún rastro en los commits:**
  - `git diff a3b86134 6bc3497f` = 21 líneas añadidas en las dos clases que añadió el RED: 7 veces el comentario y `Sta(() => { });`. Ninguna línea quitada;
  - el multiconjunto de literales de cadena es idéntico entre `RedSha` y `RedRepairSha`: 267 en AC y 97 en MT;
  - AC y MT son idénticas byte a byte en `6bc3497f` y en `03b16c88`;
  - ningún `DIAG`, `Console.Write`, `Debug.Write` ni `Paso1_Temporal` en las seis clases nuevas de los tres commits;
  - `git diff --name-only 6bc3497f 03b16c88` = solo W.
- **El GREEN no cambió tras el diagnóstico:**
  - `rv/rr/green-wip.patch` (el GREEN apartado) solo toca W;
  - su blob destino, `da66eba0`, es el W comprometido en `03b16c88`;
  - sus líneas cambiadas son las de `git diff 6bc3497f 03b16c88`.
- **Contención:** en esas corridas, el aviso vigente era el suelo `_ => true` de `StaTestRunner` (el propio defecto que se diagnosticaba; `59839c71`:`tests/RackCad.UI.Tests/StaTestRunner.cs:57`). No podía salir un `MessageBox` real, y nada corrió después del ámbito en ese proceso.

**El juicio:**
- **16.8: conforme.**
  - La regla se mide sobre commits: «el GREEN no toca `RT` ∪ `ChainRedFiles`», y con una reparación, desde `RedRepairSha`.
  - El único cambio de `tests/` tras el RED es el commit RR, que es la excepción de F4B-E35.
  - La instrumentación nunca entró en el historial.
  - El mismo régimen admite corridas temporales que tocan pruebas fuera de todo commit (INV-PBT2B-10), así que tocar `tests/` se lee sobre commits, no sobre el árbol de trabajo.
- **RR (F4B-E35): conforme.**
  - Hay una sola reparación (RR-1). El diagnóstico no es una segunda: no hay commit.
  - El error se vio tras el push del RED y se reparó antes del GREEN.
  - RR-2, RR-3, RR-5 y RR-6 se cumplen (apartado RED).
  - La regla no regula el diagnóstico, y el Worker lo declaró.
- **La limpieza del árbol: conforme en lo verificable.** Los tres commits solo llevan las 8 rutas, sin restos, y el relevo registra `CleanTree: true` en la entrada.
  - **No verificable:** dónde estuvo la copia de seguridad y el momento de cada reversión.
- **INV-PBT2B-10: hallazgo M-03** (arriba): sin TRX, no todas terminan en `Assert.Fail`, y un filtro de método en lugar del filtro del contrato. **Gravedad: menor.**

## Tabla diseño → código

| Diseño | Código (`03b16c88`) | ¿Conforme? |
|---|---|---|
| D-PBT2-04 el tipo | `src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs`: un `namespace` de bloque, el `enum` y un comentario de documentación. Sin comentarios, el texto es exacto (CT-09, `solo-el-enum=True`) | Sí |
| D-PBT2-04 el campo y la costura | `private readonly PushBackEditorMode mode;` (W:88); `ModeForTest => mode` (W:198). Una sola asignación, en W:175 | Sí |
| D-PBT2-04 la factoría y el constructor | `ForDraft` (W:296-300), con `ArgumentNullException(nameof(launch))`; el constructor privado `: this(canInsertInAutoCad: false)` (W:172-190). Sin dueño y sin mostrar | Sí |
| D-PBT2-05 la carga | W:175-189, **en el orden literal**: modo, puerto, `Copy`, `BuildInputs`, `UpdateCompositeEnabled`, `sourceProject`, `isEditingExisting`, `Adopt`, `SyncName`, las tres anulaciones, `LoadFromModel` y `appliedState`, y por último `ApplyModeChrome` | Sí |
| D-PBT2-05 las cargas públicas | La guarda es la primera sentencia de `LoadNew` (W:308), `LoadDesignForNew` (W:330) y `LoadExisting` (W:347). El constructor encadenado corre `LoadNew` con `mode` todavía en `Classic` | Sí |
| D-PBT2-06 (1) la extracción | `ApplyEditsToEditingState` (W:3372-3389) es W:3270-3287 de la base, en el mismo orden y con el mismo comentario I-42; solo `return` pasa a `return false`/`!…`. `RequestDraw` (W:3323-3365): primero la guarda, después las dos guardas de hoy sin cambio, la llamada con los dos literales de hoy y el resto, con `SyncName()` | Sí |
| D-PBT2-06 (2) «Aplicar» | `ApplyToDraft` (W:3264-3282) es literal, sin `SetName` ni `SyncName`. `HasUnappliedDraftChanges` (W:3439-3441) es literal. `Update_Click` (W:3253-3262) es el cuerpo de R9 | Sí |
| D-PBT2-07 «Cerrar» | `PendingWork` (W:3427-3433) es literal. `OnClosing` (W:3405-3414) no cambia. En CLASSIC, `When(false, …)` devuelve `None`, como hoy (`src/RackCad.UI/Shell/EditorClosePolicy.cs:33`) | Sí |
| D-PBT2-08 la guarda | `RefuseInWorkspace` (W:3316-3321) es literal. Es la primera sentencia de `RequestDraw` (W:3325) y de `InsertBatch_Click` (W:3286), antes de la confirmación de módulos | Sí |
| D-PBT2-09 los rótulos y la ocultación | `ApplyModeChrome` (W:3634-3641) oculta los seis botones con un `foreach`, que equivale a seis sentencias. El bloque final de `UpdateButtons` (W:3620-3625) es literal. Ningún otro código toca `Visibility` ni `Content` de esos botones (`git grep`) | Sí |
| D-PBT2-09 los once literales | W:3445-3455, cada uno una vez, idénticos a la tabla (R10 en verde) | Sí |
| D-PBT2-10 las dos costuras | `BomDialog` (W:3459) y su ramal, delante de S-7 (W:3476-3482); `LibraryPathDialog` (W:3485) y el ternario, con S-9 como segundo término (W:3504-3506). `EnsureId` no se mueve | Sí |
| D-PBT2-11 una sola validación | «Aplicar» pasa por el mismo paso; R11 y CT-05 lo fijan. El puerto no valida | Sí (ver M-01 a) |
| D-PBT2-12 el dueño | Los subdiálogos siguen con `Owner = this`. `ForDraft` no pone dueño (CT-01, `dueno-de-la-ventana=<null>`) | Sí |
| Q-PBT2-1 (A) | Aplica sin cerrar (CT-04 `abierta=True`); «Cerrar» pregunta (CT-06) | Sí (ver OWNER-01) |
| Q-PBT2-2 (A) | El bloqueo rechaza como «Actualizar» (CT-05 `bloqueante … llamadas=0`); el botón se deshabilita con `!canOutput` | Sí |
| Q-PBT2-3 (A) | CT-PBT2B-10, con la llamada literal del §5.3 | Sí |

**¿Cambia algo de CLASSIC?** No. Lo que llega a CLASSIC es:
- tres campos con sus valores por defecto (`Classic`, `null` y `null`);
- guardas que devuelven `false` o no lanzan;
- `Update_Click`, que llama a `RequestDraw(null, -1, updateOnly: true)` como hoy;
- la extracción, literal;
- `PendingWork`, que devuelve lo mismo;
- `UpdateButtons`, que solo añade el bloque de WORKSPACE;
- las dos costuras, nulas.

Los constructores públicos, WX, WC, Application, el Plugin, `LoadFromModel`, `ReadInputs`, `Recompute` y `OnClosing` no cambian (`git diff` vacío en esas rutas y en esos rangos). INV-PBT2B-03: los 22 hunks de `-U0` caen en miembros de la lista o son inserciones entre miembros. Los comprobé uno a uno contra el texto de `59839c71`; las inserciones están en W:86, :91, :92, :162, :168, :265, :3246, :3305, :3347, :3365 y :3499, todas entre dos miembros. Ninguna declaración nueva queda fuera de la lista de C13, y el único constructor nuevo es el privado.

**¿Algo de WORKSPACE cambia sin declararse?** Solo OWNER-01. El resto está declarado:
- N-PBT2-01;
- el nombre tras la biblioteca (L-PBT2-12);
- `FrontalSideBox`;
- «Restaurar»;
- la disposición del configurador;
- los once literales.

## D2 y su límite

- **Ninguna línea añadida muta el rack resuelto ni el informe de módulos, ni asigna el `Name` de un sistema.**
  - Las líneas añadidas en W no contienen ningún token de G-L1 en posición de escritura ni `.Name =`.
  - La única `SyncName()` nueva (W:183) asigna `editable.Name`, el nombre del estado y no de un sistema, y la prescribe D-PBT2-05.
  - `session.System == null` (W:3380) es una lectura trasladada.
- **La ventana carga una copia y devuelve una copia:**
  - `editable = Copy(launch.State)` (W:177);
  - `draftPort.ApplyToDraft(Copy(editable))` (W:3273), y el núcleo vuelve a copiar;
  - `appliedState = Copy(editable)` (W:188 y W:3280) no se muta nunca: solo se compara;
  - la ventana no guarda `launch` ni S; guarda el puerto y el proyecto fuente, como prescribe D-PBT2-05.
  - CT-02 da `mismo-objeto=False`; CT-04 da `compartidos-fuera-de-D2=0` tras «Aplicar»; CT-05 da `huellas-iguales=True`.
- **CT-PBT2B-09:**
  - las reglas, `Clean`, `Collapse`, `Members`, `Rfind`, `Aliases`, `Violations` e `Inject` son **idénticas** a G-L1c (`59839c71`:`tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs:31-172`; `diff` vacío);
  - las 15 autopruebas (a, b, c, e, f1-f3, g1, g2, h1, h2, i, j, k1 y k2) son idénticas, sin la (d), propia de la ventana;
  - `solo-el-enum` compara el texto exacto.
- **CT-PBT2B-05 y el `Vigia`:**
  - la clase es la de CT-PBT1C-02, ampliada como pide el §5.3 a `draft.CurrentDraftState` y `draft.InitialDraftState`;
  - la huella es la de PB-T1b (`95373af6`:`tests/RackCad.Tests/PushBackEditableStateCopyTests.cs:1080`), copiada sin cambios, como `AllObjects`;
  - la lectura final va tras «Aplicar» y antes de cerrar;
  - lo que se deja es la frecuencia de captura dentro de los gestos de varios pasos: M-02.
- **INV-PBT2B-07c y 14:** CT-PBT1C-02 y 04 y CT-PBT1B-14 y 15 no se tocan y están en verde en la CI del GREEN. No hay ninguna medición que encuentre una mutación, así que C17 no se dispara.

## Los CT

| CT | ¿Fija lo del §5.3? | Momento y gestos | Notas |
|---|---|---|---|
| 01 (MT:56-142) | Sí: (i) tres líneas CLASSIC, con las dos ventanas públicas registradas; (ii) la línea con la sonda como primer campo; (iii) las tres cargas lanzan, y después `modo=Workspace descartes=0`; (iv) `ForDraft(null)` | (ii) se lee antes de (iii) | — |
| 02 (MT:186-291) | Sí: el corpus C18, N-PBT2-01a, las tres filas de selección, el nombre y el estado inválido | `diseno-igual` tras `Recalcular`; `nombre` sale de `estado.Name`, leído antes de abrir; los índices de `seleccion` salen de `Sel(entrada)` y el lado es literal; el orden de `estado-invalido` es el del §5.3 | Las ramas (todas `iguales=True`) y `->Frontal, Planta` salen del paso 1 (`rv/tests/p1/p1.trx`, corrido sobre `BaseSha` a las 16:10Z, antes del RED). M-05 |
| 07 (MT:295-367) | Sí | `bom-es-el-calculado` se compara dentro de la costura y la línea se arma justo tras «BOM»; `ArchivoTemporal` se borra antes del clic | — |
| 03 (AC:257-317) | Sí: las tres listas literales, byte a byte | `borrador-igual` contra un `antes` tomado antes de abrir; 4.5 sobre 4; los gestos de la pasada 2 son copia de CT-PB0-12, 13, 15 y 16 (lo comparé con los originales en `59839c71`); `archivo` con un borrado previo; `campo-desconocido` sale del texto del archivo | Pulsa todos los botones de la barra (Restore, BOM, biblioteca, Update, los seis de inserción y Close; WX:749-790) |
| 04 (AC:322-478) | Sí | `actual` y `c` se toman tras `SetNumberR(…, 5)` y justo antes del clic; cada línea va tras su «Aplicar»; 4.5, 3 y 5 cambian el estado | El nombre tras la biblioteca: «No» y después «Sí», con `descartes=1` |
| 05 (AC:483-662) | Sí, con los tres gestos (`calculo-invalido` es el refuerzo de F-03, copiado de `PushBackStateNetSupport.cs:210-212`) y la cuarta ventana | `huellas-iguales` se lee al final, tras «Aplicar» | M-02 |
| 06 (AC:667-779) | Sí, con cinco ventanas | `SetNumberR(…, 4.5)`; «Sí» y `Cerrar` en el `finally` | — |
| 08 (SG) | Sí, las once reglas y las doce autopruebas; un miembro ausente da `False` sin lanzar (en el RED: R3 `True`, el resto `False`) | Es texto | M-01 |
| 09 (SR) | Sí | Es texto | Pasa en el RED y en el GREEN |
| 10 (`PushBackFieldValidationConcordanceTests`) | Sí, con los diez textos, la llamada literal y la autoprueba con el mínimo inclusivo | En STA y con la cultura invariante | — |

**Sin aserciones tautológicas** en lo que se ejerce. M-05 es el único centinela que podría serlo, y no se ejerce.

**Ningún gesto inerte sostiene una línea:**
- toda edición que debe llegar al borrador es `SetNumberR` con un valor distinto del de la caja (4.5 sobre 4, 3 sobre 8, 5 sobre 4.5);
- `campo-en-error` no eleva `LostFocus`, a propósito, como dice el §5.3.

**La sonda va antes de cualquier gesto** en todas las ventanas de WORKSPACE: MT:102, :164 y :317; AC:88, :149, :330, :378, :403, :569, :594 y :674.

**Cada prueba acaba en un solo `Assert.Equal`**, con las clases `public sealed`, en la colección y en STA.

## El RED y la reparación del RED

- **El RED** (`a3b86134`): en W, solo las cuatro declaraciones exactas de la costura inerte del §5.3, más dos líneas en blanco y ninguna línea quitada. Además, el archivo del modo y las seis clases.
- **Su CI** (`rv/red-ci-*`, run 44572405777):
  - Core: 19094, con 1 `Failed` (CT-08, `Assert.Equal`);
  - UI: 2447 resultados, 7 `Failed` (CT-01 a 07, todos `Assert.Equal`; la primera línea de la traza está en la clase; `Actual` = `modo=Classic`, o la línea de (ii) en CT-01) y 21 `NotExecuted`;
  - CT-09 y 10 en `Passed`; ninguna prueba existente falla;
  - jobs: Tests y UI Tests en `failure`, Build UI en `success`, Build Plugin `skipped`.
  - Se cumple C04: ningún método pasa ni falla por excepción.
- **La RR (`6bc3497f`):**
  - **RR-1:** un solo commit, hijo directo de `RedSha`, después del push del RED y antes del GREEN.
  - **RR-2:** solo AC y MT, dos clases que añadió el RED.
  - **RR-3:**
    - las 15 entradas de la lista cerrada del relevo (11 de AC y 4 de MT) siguen byte a byte, con las mismas cuentas en `RedSha` y en `RedRepairSha`;
    - los literales no cambian y los primeros argumentos de `Assert.Equal` tampoco (`esperadas` en los siete);
    - ninguna cadena `campo=` cambia: **sin indicios**.
  - **RR-3, el contenido:** la reparación solo adelanta la creación del hilo STA, con `Sta(() => { })`, a la sustitución del aviso: es el «cómo» de un paso que el paquete ya prescribe («el aviso de descarte siempre sustituido»). La causa es real:
    - `EditorDiscardPrompt.confirm` es un estático de proceso (`src/RackCad.UI/Shell/EditorClosePolicy.cs:51`, `:69`);
    - el primer `Sta` lo pisa con el suelo permanente `_ => true` (`StaTestRunner.cs:57`);
    - `g0` lo muestra: CT-04, la primera en correr, da `descartes=0`.
  - **RR-4:**
    - la CI 44581043462 da lo mismo que la del RED (19094/1 y 2447/7, con los mismos métodos);
    - los TRX locales `rrl` y `rru` fallan enteros en los tres filtros con `ExpectRed`, y el resto pasa.
  - **RR-5:** el título lleva «RED repair» y el cuerpo nombra la tarea; `Deviations[0]` declara el SHA, las pruebas, la sentencia, el error y el diff (`rv/rr/red-repair.diff`, idéntico a `git diff a3b86134 6bc3497f`).
  - **RR-6:** `RedRepairSha..CurrentSha` = solo W.
  - **El trailer:** `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>` en los tres commits.
  - Los SHA-256 de los seis TRX de CI copiados coinciden con los del relevo.
- **Una salvedad, no verificable:** los TRX locales de cada fase se tomaron sobre el árbol ANTES de su commit:
  - `r1` a `r4`, de 16:30Z a 16:58Z, antes del RED (17:00Z);
  - `rrl`, `rru` y `rrf`, de 17:19Z a 17:46Z, antes de la RR (17:47Z);
  - `g1` a `g3`, antes del GREEN (18:16Z).

  `TreeSha` es `null` en todas, como admite INV-PBT2B-11, así que su árbol no se puede atar al SHA. La CI de cada SHA exacto lo cubre.

## El GREEN y la estabilidad

- **La CI 44586045707** (`rv/ci-*`), los 4 jobs en `success`:
  - Core: 19094 `Passed`;
  - UI: 2447 resultados, 2426 `Passed`, 21 `NotExecuted` y 0 `Failed`.

  Es la línea base 19092/2439 más 2 y más 8.
- **En local:**
  - tres corridas en verde de los filtros con `ExpectRed` (`g2`);
  - las huellas de PB-T1a, tres veces 14/14 (`g3`);
  - Core completo, 19094 (`g1f`);
  - los 38 filtros, con su mínimo.
- **El orden y el estado estático:**
  - con la RR, el suelo del aviso ya no puede pisar la sustitución;
  - las dos clases de UI están en `EditorDiscardPromptCollection`, que tiene `DisableParallelization = true` (`EditorDiscardPromptCollection.cs:27`), así que ninguna otra clase corre a la vez;
  - `ArchivoTemporal` se comparte entre CT-03, 04, 05 y 07, pero dentro de la misma colección serializada, y se borra antes de cada guardado y en el `finally`.
- **La cultura:** `S0.Sta` la fija invariante. Las líneas no formatean números salvo enteros.
- No veo otra dependencia de orden.

## Contención

- **No se muestra ninguna ventana:** `ForDraft`, `S0.Ventana()` y los constructores públicos no llaman a `Show`.
- **No hay ningún modal real:** el aviso está sustituido; el BOM, la biblioteca, el configurador, la seguridad, la defensa y los topes van por sus costuras; ninguna prueba nueva llega a `PromptSaveToLibrary`; «Insertar» y el lote se niegan en WORKSPACE antes del diálogo.
- **No hay aserciones dentro de las costuras.**
- **La única escritura de las pruebas nuevas es `ArchivoTemporal`.**
- **En local, en UI:**
  - solo los filtros del contrato, más 6 corridas con el filtro de método de CT-04 (5 de diagnóstico y `g1-solo04`) y 1 con `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspace`;
  - ese último selecciona 7, la unión exacta de dos filtros del contrato (no hay otra clase con ese prefijo en `03b16c88`);
  - nunca la suite completa.
- **El Plugin, WX, WC y AutoCAD:** sin cambios (`git diff` vacío). Los builds del Plugin salieron a la carpeta de artefactos.

## Hallazgos OWNER

**OWNER-01. «Cerrar» con un módulo escenificado y cambios sin aplicar.**
- **Lo que aprobó el Owner** (F4B-E28, Q-PBT2-1, A; `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt2-decisions.md:13` en `03b16c88`): «Al cerrar con algo sin aplicar, el editor pregunta: «Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?»».
- **Lo que hace el código** (literal de D-PBT2-07; W:3427-3433): si además hay una edición de módulo escenificada, solo hace la pregunta del módulo («Hay cambios de módulo sin confirmar que se perderán al cerrar…»).
- **La consecuencia:** al contestar «Sí», la ventana se cierra y el borrador pierde también lo editado y no aplicado, por ejemplo la tolerancia, sin que la pregunta lo diga.
- No es un defecto del Worker, que sigue el paquete al pie de la letra. Tampoco está en el §8.4.
- CT-06 solo ejerce el módulo solo (ventana 4).
- **La propuesta:** informarlo al Owner y, si quiere otra cosa, decidirlo en PB-T4 o en otra versión del paquete.

No encuentro ninguna otra diferencia observable sin declarar, más allá de N-PBT2-01, el nombre tras la biblioteca y los once literales del §8.4.

## Lo no verificado

- **Nada se compiló ni se ejecutó.** Los resultados de las pruebas salen de los TRX copiados, cuyos SHA-256 de CI coinciden con el relevo.
- **El árbol de las corridas locales:** su `TreeSha` es `null`, y corrieron antes de cada commit.
- **Las corridas de diagnóstico:**
  - el instante exacto de cada una y de cada reversión: los logs no llevan hora;
  - dónde estuvo la copia de seguridad.
- **La prueba de limpieza del perfil:** `cleanup/` no está en la copia. Me fío de lo que dice el handoff: solo cambia `RackFrameConfigurator.layout`, atribuido a los filtros existentes (INV-PBT2B-10).
- **El estado de los jobs de CI:** sale de `RemoteFacts` del relevo, no de GitHub.
- **Que CT-02 cubra todos los diseños que pudieran traer seguridad no autorizada:** depende del paso 1, cuyo TRX vi. No comprobé la medición por mi cuenta.
- **Que un recálculo intermedio de `calculo-invalido` adopte una línea base nueva** (el supuesto de M-02): sale de leer el código, sin ejecutarlo.
- **Fuera del encargo, para la sesión:** el relevo registra `config.toml` «SNAPSHOT K» a la salida y a la entrada, y P-01 nombra «today Snapshot J». No lo comprobé.
- **Dos incidencias de procedimiento de esta revisión, sin efecto:**
  - lancé una vez `python -I -c "pass"`, lo que incumple la regla de no usar `-c`;
  - escribí un archivo temporal en el directorio temporal del sistema, fuera del scratchpad, y lo borré en el acto.

  Ninguna tocó el repositorio ni el worktree.
