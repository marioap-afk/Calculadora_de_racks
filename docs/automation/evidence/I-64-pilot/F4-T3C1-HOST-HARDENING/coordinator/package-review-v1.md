# I-64 F4-T3C: revisión adversarial independiente del paquete de contrato (v1)

- **Objeto:** `I-64-f4-t3c-gate-contract-package.md` v1, con sus cuatro mapas y la orden del Owner (`owner-f4t3c-decomposition-decision.md`).
- **Código:** el worktree de I-64, rama `architecture/workspace-persistente-rackcad`, HEAD `7895f397`. Comprobé que desde `23f67f56` solo cambia `docs/`.
- **Método:** solo lectura (`git show`, `git diff`, `git log`, `git grep` y lectura de archivos). No ejecuté builds ni pruebas y no abrí AutoCAD. Lo que depende de ejecutarlas, como `MinSelected`, no está comprobado.
- **Autoridades leídas:**
  - el paquete de F4-T3 (§8, R-C1 a R-C4, D-T3-09);
  - el paquete de F4-T3B2 (D-T3B2-03, D-T3B2-04, R-B2-5);
  - las órdenes del Owner `owner-f4t3b1-close-disposition.md` (F4-E43), `owner-f4t3-authorization.md` y `owner-f4t3a-close-decision.md`;
  - los paquetes de cierre de F4-T3A (§5), F4-T3B1 (§5) y F4-T3B2 (§5);
  - las revisiones independientes de F4-T3A (`d960`), F4-T3B1 intento 1 (`2130`, hallazgo 6) y F4-T3B2 (`4f9b`).

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Hay 1 hallazgo BLOQUEANTE, 4 IMPORTANTES y 10 MENORES.

El diseño de fondo es sólido:
- el desconocido explícito de `PilotRiskView`;
- las colas con `try`;
- `DropCloseNotice` dentro del primer `try`;
- el motivo del Error que cruza el puerto.

Ninguno de esos cuatro rompe las guardas de forma del puerto ni las de `SessionDestroyed`.

El bloqueante está en M-3: la contención que propone rompe una prueba cerrada de F3 que el paquete afirma que no existe. Los importantes son huecos que el Worker tendría que inventar:
- cómo llegar al `catch` inalcanzable;
- los textos exactos de las traducciones;
- dos textos de M-2;
- una parte del menor 6 que se aplaza a F7 sin decisión del Owner.

## Hallazgos

### B-1 (BLOQUEANTE). La contención de M-3 rompe una prueba cerrada de F3 y cambia su comportamiento; el paquete afirma lo contrario

**Evidencia:**
- **Paquete:**
  - §2.3 M-3: «El doble tiene `ThrowOnAlive`, **que hoy no usa ninguna prueba**»;
  - D-T3C1-07 M-3: «la única llamada a `IsSessionAlive` del VM va dentro de un `try`. Si lanza, esa sesión cuenta como viva […] y la presentación da el aviso»;
  - §4: «Ninguna prueba existente de comportamiento cambia»;
  - §5: la regla RED;
  - §7, punto 5.
- **La prueba de F3:** `tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserViewModelTests.cs:1006-1019`, `AFailureOfTheLivenessCheckReachesThePanelAsTheContainedErrorState`, introducida en el RED de F3-T2 (`1c379a23`).
  - Pone `summary.ThrowOnAlive` (`:1013`), llama a `panel.Refresh()` y exige que `panel.State` sea `WorkspacePanelState.Error` y que `ErrorMessage` contenga «boom-vida».
  - El panel es de F3, sin puerto del piloto (`Panel()`, `:37-44`).
- **Recorrido en el código:**
  - `WorkspacePanelViewModel.cs:183-195`: `Query` → `Present` → `Browser.Present`;
  - `WorkspaceBrowserViewModel.cs:621-640`: `PurgeDeadSessions` recorre `KnownSessions` también en F3 y llama a `IsSessionAlive` en `:630`.
  - Hoy la excepción sube a `Query`, que entra en `EnterError`.

**Problema:**
- Con el `try` propuesto, la excepción se queda en el navegador y el panel sigue en `Ready`, así que la prueba de F3 falla en el GREEN.
- La regla RED no lo detecta: en el RED el VM no cambia, así que el fallo aparece después, en la CI del GREEN.
- La prueba no está en el AllowedWriteScope, y cambiarla modifica un comportamiento cerrado de F3: un fallo de la vida llega al panel como su Error contenido.
- El hecho de §2.3 es falso.

**Corrección propuesta.** Contener solo la construcción:
- en el constructor del VM del panel, `try { Present(InitialSnapshot(), null); } catch (Exception ex) { EnterError(ex); }`;
- el `Refresh` de F3 sigue igual y la prueba de `:1006-1019` sigue pasando;
- la guarda `ThePanelViewModelOnlyChangesTheDocumentationOfPilotEdit` (`WorkspacePilotCloseSyncGuardTests.cs:288-299`) sigue pasando, porque mira la firma, la construcción del navegador y la ausencia de llamadas a las colas.

Eso exige:
- ampliar el alcance de `WorkspacePanelViewModel.cs`, que hoy es «solo la documentación», al constructor;
- corregir el hecho de §2.3, §4 y §7.5;
- rehacer la prueba de M-3: con el puerto del piloto, un veto en cola y `ThrowOnAlive`, la construcción no lanza (`Record.Exception`) y el panel queda en Error con el motivo.

La otra salida es aplazar la purga de los vetos fuera de la primera presentación, como sugería la revisión `4f9b`. Si se quiere de verdad que la sesión «cuenta como viva», hace falta una decisión del Owner sobre F3 y un cambio declarado de esa prueba.

### I-1 (IMPORTANTE). El `catch` externo de `GetRacksAtRisk` es inalcanzable y el paquete pide probarlo sin decir cómo

**Evidencia:**
- **El host:** `WorkspaceHostCore.cs:765-807`.
  - El `try` interno (`:770-777`) y el de cada borrador (`:790-797`) se tragan los fallos del comparador.
  - Al `catch` externo (`:802-806`) solo se llega si lanzan `_drafts.RackIdsOf` o `_drafts.Find<…>`.
- **El registro:** `DraftRegistry.cs`.
  - `Find` (`:169-175`) solo lanza con un `rackId` vacío y devuelve `entry as …`.
  - `Begin` rechaza el `rackId` vacío (`:145`).
  - `RackIdsOf` (`:186-189`) no lanza.
- **Lo dicen las autoridades:** el paquete de F4-T3B2 (`:94`, «Hoy es inalcanzable») y la revisión de F4-T3A (`d960`, hallazgo 4, «Hoy también es inalcanzable»).
- **El truco de `GetField` no sirve aquí:** el precedente de `WorkspacePilotEditHostTests.cs:1463-1485` llega al `catch` de `GetField` con un copiador nulo por `OpenDrafts`, pero en `GetRacksAtRisk` ese mismo borrador cae en los `try` internos y sale incierto.
- **Lo que pide el paquete:** §4, primera viñeta: «el `catch` de `GetRacksAtRisk` da `IsKnown` falso».

**Problema:**
- El Worker tendría que inventar una costura, sea reflexión o un gancho en el producto, y D-T3C1-01 dice que el host no cambia la lógica.
- Además, el paquete presenta el endurecimiento como si cerrara un fallo observable. En el producto el desconocido es hoy latente, y eso no se dice.

**Corrección propuesta:**
- Declarar que el `catch` es hoy inalcanzable y que el endurecimiento es estructural.
- Fijar la cobertura, con una de dos vías:
  - (a) una prueba que pone `_drafts` a null por reflexión, con el precedente de `FieldInfo.SetValue` en `WorkspaceDraftPilotStateTests.cs:1182`;
  - (b) una guarda por cuerpo de que el `catch` externo es `Report(…); return default;`, más la prueba de que `default(PilotRiskView).IsKnown` es falso.
- Prohibir de forma expresa cualquier costura nueva en el producto.

### I-2 (IMPORTANTE). D-T3C1-05 no fija los textos ni la lista exacta de literales, y no dice qué identificadores y qué literales en inglés deben quedarse

**Evidencia:**
- **El paquete no da textos cerrados:** D-T3C1-05 da «Por ejemplo» y «y los de la intención, la comprobación y la recarga».
- **H-07 da rangos, no literales:** cita «299-504».
- **Los literales que caen en ese rango:** en `WorkspaceHostCore.cs`, `:299`, `:318`, `:320`, `:322`, `:326`, `:350`, `:373`, `:377`, `:387`, `:389`, `:399`, `:401`, `:403`, `:407`, `:420`, `:456`, `:466`, `:467`, `:477`, `:479`, `:481`, `:485`, `:502` y `:504`. Son unos 16 textos distintos.
- **Las pruebas sí piden textos exactos:** §4 pide pruebas con «los motivos en español» de `GetField`, `SetPendingText` y `CrossBoundary`.
- **Identificadores fijados por guardas:**
  - `WorkspacePilotEditGuardTests.cs:385` contiene el texto `…Waiting(PilotReadInProgressReason);`;
  - `:428` y `:448-450` exigen `.Waiting(InsideNoticeReason)`.
  - Si el Worker renombra esas constantes al traducir, esas guardas fallan.
- **Literales en inglés que deben quedarse:**
  - `WorkspaceLifecycleGuardTests.cs:405-408` exige que `DescribeLoss` (`WorkspaceHostCore.cs:977-988`) contenga «Uncertain»;
  - `:1050` («(variable registry)», que cruza el puerto en `EditCheckOutcome.Conflict`) no está ni dentro ni fuera de H-07.
  - Una guarda nueva que prohíba «el inglés» de forma genérica chocaría con los dos.
- **Textos duplicados tras la traducción:**
  - `WorkspacePilotFieldViewModel.cs:432-433` antepone «hay una lectura en curso.» y «el campo no se puede editar ahora.» al motivo;
  - `WorkspaceBrowserViewModel.cs:909` hace lo mismo con «hay una lectura en curso.».
  - Saldría «El texto no se aplicó: el campo no se puede editar ahora. El campo no se puede editar ahora.».

**Corrección propuesta:**
- Una tabla cerrada «literal en inglés → texto exacto en español», que sea también la lista de la guarda nueva.
- Conservar los identificadores `PilotReadInProgressReason`, `NotThisDocumentReason`, `NoDraftReason` e `InsideNoticeReason`.
- Excluir de forma expresa `DescribeLoss` (registro) y `DescribeDifferences`.
- Decidir la duplicación: quitar el prefijo redundante de la UI o aceptarlo como limitación declarada.

### I-3 (IMPORTANTE). Una parte del menor 6 asignado a F4-T3C, los motivos del Plugin que muestra el campo, se aplaza a F7 sin decisión del Owner

**Evidencia:**
- **Las órdenes:**
  - el cierre de F4-T3B1 (§5): «Menor 6 […] → F4-T3C: los motivos del host en inglés»;
  - la revisión `2130` (hallazgo 6) pone de ejemplo «En espera: A command is in progress.»;
  - F4-E43: T3C es dueña de «carried minor limitations assigned to T3C».
- **Los motivos en inglés de la lectura del piloto en el Plugin:**
  - `WorkspaceNavigationHost.cs:224`, `:226`, `:236` y `:251`, este último con `read.FailureReason`;
  - `RackPilotIntentAdapter.cs:80` («The catalog is not available.»), `:112`, `:118`, `:127`, `:157` y `:174`.
- **Cómo llegan al usuario:** pasan por `WorkspaceHostCore.cs:318-322` y `:399-403` (`read.Reason ?? …`) y se muestran en `Describe` (`WorkspacePilotFieldViewModel.cs:439-450`).
- **El paquete los aplaza:** D-T3C1-05 y D-T3C1-08 los declaran «limitación para F7».

**Problema:**
- La orden del Owner de dos subtareas deja a T3C1 sin Plugin, pero T3C2 sí toca el Plugin.
- Aplazar a F7 una parte de un menor que el Owner asignó a T3C es una decisión de alcance que el paquete no puede tomar sola.
- Los de F3 (resumen y navegación) sí quedan fuera con razón, porque no son del campo.

**Corrección propuesta:** asignarlos a T3C2 con una regla nueva (R-C14), o pedir y citar una decisión del Owner que los aplace.

### I-4 (IMPORTANTE). Los casos límite de M-2 dejan sin texto y sin mecanismo la retención con «abrió en segundo plano» y la confirmación sin lectura

**Evidencia:**
- **La retención:** `WorkspaceBrowserViewModel.cs:657-660` tiene dos textos.
  - `:659` es el de `Created` o `Replaced`, «La selección abrió R en segundo plano; la pestaña activa conserva su texto pendiente.».
  - `:660` es el de la pestaña ya abierta.
  - El paquete solo da el sustituto del de `:660`.
  - El caso real de `GetField` que lanza, `AFieldThatThrowsRetainsTheActiveTab` (`WorkspacePilotCloseSyncTests.cs:1179-1189`), sale por `:659`.
- **El mecanismo de la retención:** `KeepsActive` (`:664-676`) devuelve un `bool` y no distingue «tiene pendiente» de «no se pudo comprobar».
- **La confirmación de cierre:** `ConfirmationTextOf` (`:995-1007`) compone el texto solo con el tipo de confirmación. Para el caso de `:1000` hace falta recordar, en `CloseTab` (`:277-280`), que `ReadField` devolvió null.

**Problema:**
- El Worker tiene que inventar un texto visible y dos cambios de estado privado: el resultado de `KeepsActive` y una marca de la confirmación.
- El texto propuesto «¿Descartar su borrador y cerrarla?» da por hecho que hay borrador, y eso es justo lo que no se pudo comprobar.

**Corrección propuesta:**
- Dar el texto de `:659` para el caso «no se pudo comprobar».
- Fijar el mecanismo: un resultado de tres valores u `out`, y una marca privada que pone `Ask` y quita `RemoveConfirmation`.
- Usar una pregunta neutral, por ejemplo «¿Descartar lo que tenga y cerrarla?».

### m-1 (MENOR). G-02 dice algo falso: la guarda del puerto no lee los comentarios

**Evidencia:**
- `WorkspacePilotEditGuardTests.cs:30` y `:99`: `Port()` es `Src.Code`.
- `WorkspaceHostSourceSupport.cs:29-39`: `CodeOnly` quita `//` y `/* */`.
- `:73`: la expresión `\bDraft\w*` distingue mayúsculas.
- El puerto ya dice «draft» en la documentación: `IWorkspacePilotEditPort.cs:93` y `:128`.
- La única comprobación del puerto con comentarios es `:786-787` (`Src.Text`), y no busca `Draft`.

**Problema:** el aviso de G-02 y D-T3C1-06 sobre «draft» en la documentación no tiene fundamento.

**Corrección:** corregir el hecho.

### m-2 (MENOR). G-04 está incompleta

**Evidencia:**
- `WorkspacePilotEditGuardTests.cs:756-758`:
  - `CodeTraps` incluye `LockDocument`;
  - `NameTraps`, sin distinguir mayúsculas, incluye `Aggregat`, `\bBom`, `TotalRacks` y `ProjectSummary`.
- `:782-788`: `Src.Text` de cuatro archivos (también `WorkspaceEditIntent.cs`), sin distinguir mayúsculas e incluyendo `ComputedParameter`.
- `WorkspaceBridgeSourceGuardTests.cs:362-372`: aplica `I63Concepts` a todo `Application/Workspace`.

**Problema:** al no distinguir mayúsculas, «metrica» sin tilde dispararía `Metric`.

**Corrección:** completar la lista y decir que se busca sin distinguir mayúsculas y por subcadena.

### m-3 (MENOR). M-6 cambia sin necesidad unas guardas que la autoridad condiciona («si las necesita»), y su referencia es ambigua

**Evidencia:**
- **La condición:** el cierre de F4-T3B2 (§5, M-6) dice «F4-T3C las cambia de forma declarada si las necesita».
- **Ninguna subtarea las necesita:** el §2.4 y D-T3C1-08 del propio paquete dicen que T3C1 no toca ni el XAML ni el teclado, y R-C13 dice que T3C2 no abre ningún subdiálogo.
- **Lo que se pierde:**
  - G:450 prohíbe hoy en todo el code-behind `e.SystemKey`, `e.ImeProcessedKey`, `e.IsDown`, `e.IsToggled` y `KeyUp`;
  - la G-05 de F4-T3B1 (`WorkspacePilotSurfaceGuardTests.cs:280-283`) no cubre esas prohibiciones, así que estrechar G:450 las pierde fuera del manejador.
- **La referencia ambigua:** «G-05» choca con la G-05 del §2.5, que es la del host.
- **Los botones sin nombre:** los botones de las filas (`WorkspacePanelView.xaml:169` y `:172`) no tienen `x:Name`, así que `TagOf` no los encuentra.

**Corrección:** dejar M-6 sin cambio en T3C, declarado como «no necesitado». Si se mantiene, nombrar los botones y cómo se localizan, y citar «la G-05 de F4-T3B1».

### m-4 (MENOR). El arreglo de M-4 toma por llamada la declaración de un método que devuelve un array

**Evidencia:** `WorkspacePilotCloseSyncGuardTests.cs:46`. Con «el token empieza por un carácter de palabra», en `string[] Name(` la racha final es «]», así que deja de contar como declaración.

**Corrección:** añadir autopruebas con `string[] Name(` (declaración) y `List<int> Name(` (declaración), además de `=>`, `?` y `a > Name(`.

### m-5 (MENOR). M-5 no dice qué es «exactamente» la forma pública

**Evidencia:**
- `WorkspaceBrowserViewModel.cs` tiene 41 líneas `public`:
  - 2 records fuera de la clase (`:13` y `:19`);
  - la clase (`:44`);
  - 5 de `StoredNotice` (`:69-83`);
  - 33 miembros de la clase exterior: el constructor, 18 propiedades (`:138-200`) y 14 métodos (`:211-580`).
- La guarda corre en Core, sin reflexión sobre la UI.

**Corrección:**
- Fijar la lista exacta de las 33 firmas.
- Fijar el método: quitar el cuerpo de `private sealed class StoredNotice` y comparar el conjunto exacto.
- Añadir su autoprueba.
- Declarar que esto fija también la superficie de F3 y de F4-T3B1, y que cualquier cambio de T3C2 será declarado.

### m-6 (MENOR). La prueba de M-7 está mal formulada

**Evidencia:**
- `CancelDiscard` (`:510-514`) y `DismissNotice` (`:517-525`) no tienen una excepción del puerto que inyectar.
- Cerrar la pestaña activa limpia cambia la superficie de forma legítima: `CloseNow` → `Interact` → `Rebuild`, y lo recorre `WorkspacePilotCloseSyncTests.cs:347-364`.

**Corrección:**
- En los cuatro gestos con excepción del puerto, afirmar que la superficie no entra en el Error de `ContainFailure`: `SurfaceStatusText` no empieza por «No se pudo leer el resumen:» con el mensaje del gesto.
- La guarda sigue cubriendo los seis.

### m-7 (MENOR). D-T3C1-03 pierde el aviso cuya conversión lanza, y cita mal una guarda

**Evidencia:**
- `WorkspaceHostCore.cs:812-816` desencola antes de convertir.
- «Lo tomado hasta el fallo» descarta justo el aviso que lanzó, la pérdida que señalaba la revisión `d960` (hallazgo 4).
- «Sin `Authorize`» es `WorkspacePilotEditGuardTests.cs:247-251`, no G-08.

**Corrección:** declarar esa pérdida, que solo deja rastro en el registro, o seguir con el siguiente aviso. Corregir también la referencia.

### m-8 (MENOR). El motivo del Error y el desconocido se documentan de más

**Evidencia:**
- `DraftSession.EnterError` no tiene llamadores en `src/`: solo lo llaman las pruebas, como `WorkspacePilotEditHostTests.cs:620` y `:676`.
- Un borrador en Error solo será alcanzable con F4-T4, y su motivo llega crudo, quizá un `ex.Message` en inglés.
- La regla del puerto «every exception is contained as a value with a reason» (`IWorkspacePilotEditPort.cs:21`) choca con el desconocido sin motivo de D-T3C1-08.
- El cierre de F4-T3A (§5, punto 4) pedía «el fallo de `GetRacksAtRisk` como incierto».

**Corrección:**
- Declarar que `ErrorReason` es latente hasta F4-T4 y que no se traduce.
- Ajustar la regla del puerto.
- Justificar en una línea que `IsKnown` falso es la forma de «incierto» sin lista.

### m-9 (MENOR). §6 omite censos y matices que T3C2 necesita

**Censos que faltan en R-C9:**
- `WorkspaceLifecycleGuardTests.cs:791-798`: 9 archivos de `Plugin/Workspace` y 45 pruebas en `WorkspaceHostSourceGuardTests`, dos veces con la de `WorkspacePaletteHostSourceGuardTests.cs:659`;
- `WorkspacePaletteHostSourceGuardTests.cs:763-777`: la lista exacta de los 9 archivos del Plugin.
- Los dos rompen si el emisor de R-C8 va a otro archivo del Plugin o si cambia el número de pruebas.

**Textos prohibidos:** la guarda de `:317` prohíbe también «no muestra» y «not shown».

**Prohibiciones del archivo del comando:** `WorkspaceHostSourceGuardTests.cs:65-73` prohíbe `LockDocument`, `StartTransaction`, `GetObject` y otros.

**R-C6 frente a R-C1:** R-C6 sustituye el «usa `IsCreated`» de R-C1 (paquete de F4-T3, `:425`) y debe declararse como cambio.

### m-10 (MENOR). Precisiones

- **H-03:** `ConfirmDiscard` ocupa `:396-507`. `:472` es solo el final de la rama del documento.
- **Regla RED:** decir si el RED ya trae `_known`, porque de eso depende que la guarda nueva de `_known` falle en el RED. Exigir `Record.Exception` en las pruebas de construcción, para que fallen «por aserción».
- **M-3:** no dice cómo se compone el aviso (`_takeFailure` y `PostPresentationNotice`, o cómo).
- **`MinSelected`:** los valores 2331 y 431 no se pueden comprobar aquí. F4-T3B2 usó 2300 y 352.

## Lo que comprobé y es correcto

**Hechos:**
- H-01, H-02, H-04, H-05 y H-08 coinciden con el código, con las líneas indicadas.
- Desde `23f67f56` solo cambia `docs/`.

**D-T3C1-01 (productores, consumidores y guardas):**
- El único productor de `default(PilotRiskView)` es el `catch` externo del host.
- El decorador (`WorkspacePilotSupport.cs:190`) y el adaptador (`WorkspaceController.cs:263`) solo delegan.
- El doble construye sus listas (`:262-268`), y todos los `RisksResponder` usan valores construidos.
- No hay puertos falsos en Core, y ninguna prueba compara `PilotRiskView` por valor.
- Los únicos consumidores son `RequestDocumentDiscard` y `ConfirmDiscard`, más T3C2 (R-C7).
- Un `private readonly bool _known` con `IsKnown => _known` pasa la guarda de forma (`WorkspacePilotEditGuardTests.cs:204-210`), la del puerto (`:73` y `:99`) y las trampas (`:756-788`).
- Las pruebas de `:1269-1285` y `:1307-1318` siguen pasando.

**D-T3C1-06:**
- El parámetro opcional compila con los usos de 9 argumentos: host `:606` y `:891`, doble `:104` y `:108`.
- Ninguna guarda fija la aridad del constructor.
- Ninguna prueba fija «Error: sin detalle.».

**D-T3C1-03 y D-T3C1-04:**
- Conservan G-07 (`WorkspaceLifecycleGuardTests.cs:376-389`: 2 `try`, 2 `catch`, `DropCloseNotice` fuera del orden), G-08 (`:401-403`) y las reglas de memoria (`WorkspacePilotEditGuardTests.cs:247-251`).
- Las mutaciones de `:313-327` y `:703-722` siguen inyectando dentro del cuerpo con `try`.
- La prueba `WorkspacePilotEditHostTests.cs:1209-1216` no depende del orden.

**H-07:**
- Ninguna prueba afirma los literales del host ni del support; las coincidencias son de dobles.
- No hay ninguna guarda de ASCII, y Application ya tiene literales con tilde (`PushBackPilotIntentReader.cs:23-56`).
- Las traducciones naturales no disparan las guardas de texto de host ni de UI, salvo lo dicho en m-2.

**D-T3C1-02 y D-T3C1-07 (textos que no se rompen):**
- M-1 y M-2 no rompen ningún texto fijado: `WorkspacePilotCloseSyncTests.cs:22`, `:625`, `:675`, `:1116`, `:1134` y `:1148`, y la vista en `:296` y `:305`.
- Los textos nuevos del desconocido no afirman nada que el VM no sepa.
- M-1 es compatible con la prueba de `:315-345`.

**M-4:** el arreglo no cambia el resultado actual de ninguna guarda, porque en la UI no hay llamadas sin calificar contadas tras `=>` ni tras `?`.

**G-10 y G-11:** sus conteos se conservan. `.IsSessionAlive(` sigue en 1, y lo cuentan también `WorkspaceBrowserHostGuardTests.cs:248` y G:269 y G:274.

**§2.4:**
- `EnterPanelModal` no tiene llamadores en `src/`.
- `DialogViolations` no cuenta `GetKeywords`.
- Ninguna autoridad concreta una obligación de código de modal o de foco para T3C.

**§6:**
- Las referencias de R-C8, R-C10 y R-C11 son correctas: `WorkspaceHostSourceGuardTests.cs:84-89`, `WorkspaceLifecycleGuardTests.cs:278-288` y `WorkspacePilotCloseSyncTests.cs:780-785`.
- R-C12 es coherente con R-C2.

**Las órdenes del Owner:** F4-E43 queda cubierta, salvo lo dicho en I-3:
- el comando y el aviso, en T3C2;
- el endurecimiento, en T3C1;
- el modal y el foco, en Smoke-2;
- los menores 4 y 6 de F4-T3A y M-1 a M-7 de F4-T3B2, en T3C1.
