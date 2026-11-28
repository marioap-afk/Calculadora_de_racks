# I-64 F4-T4C-H0: notas del arnés de la sonda de host

**Qué es:** un arnés de diagnóstico, solo en Debug. **No es producto**, no se hace commit, no se integra y no cuenta como evidencia de cumplimiento (D-T4C-16; O-01 «H0 con dispensas»).

**Estado:** segunda versión, con las correcciones de la revisión independiente (`h0-harness-review.md`: APTO TRAS CORRECCIONES, 0/5/11). Ver §9.

**Las rutas:**
- **la carpeta de notas** es `%TEMP%\claude\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\t4c\h0\`. Esta nota no lleva rutas con el nombre del usuario (H-13);
- **el worktree desechable:** `<worktree-desechable>`, desacoplado en `b15072ff4d30e72ed820df8a40bada84729fc331`, sin commit, sin rama y sin stash;
- **la salida del build:** `<salida>`.

**El parche:** `<carpeta de notas>\h0-harness.patch`.
- Tiene 1820 líneas y 11 archivos: 7 modificados y 4 nuevos.
- Su SHA-256 es `f2a48bec64704ebbc2a99198051ea8417c1c156a2c7895a6d45fd2f7ad2672f5`.
- `git diff` a secas no incluye los archivos sin seguimiento. Por eso el parche es `git -C <worktree-desechable> diff` más un `git diff --no-index -- /dev/null <archivo>` por cada archivo nuevo, sin tocar el índice.
- `git apply --check` aplica limpio sobre una copia de `b15072ff`, y `-R` sobre el worktree también.
- No contiene rutas locales.

**Lo que no se hizo:** no se abrió AutoCAD y no se ejecutó ninguna prueba. El checkout principal y el worktree de I-64 no se tocaron.

---

## 1. `ExecuteInCommandContextAsync` no devuelve un `Task`

**El tipo.** La llamada devuelve `DocumentCollection.ExecutionResult`, un awaitable propio de AutoCAD con `IsCompleted`, `GetAwaiter()`, `GetResult()` y `OnCompleted(Action)`. Lo medí al compilar contra el `AcCoreMgd.dll` 25.0.0 de NuGet: el primer build dio CS0029.

**Los hechos del IL** los leyó la revisión independiente (§5 de `h0-harness-review.md`) en el `AcCoreMgd.dll` instalado, versión de archivo **25.0.171**:

| Miembro | Lo que hace | Consecuencia |
|---|---|---|
| `ExecuteInCommandContextAsync` | Llama a `beginExecuteInCommandContext` y **descarta su estado** (`pop`) | Un rechazo nativo no lanza y nunca completa. Un callback que no corre es indistinguible de uno diferido hasta la cota |
| El trampolín nativo `myCallback` | Invoca el callback y engancha `ContinueWith(…, ExecuteSynchronously)`, **sin ninguna región de excepción** | La continuación corre en línea al volver el callback. Una excepción del callback cruzaría a código nativo: el callback tiene que ser total |
| `ExecutionResult.Continuation` | Publica la compleción con `Post` en el `SynchronizationContext.Current` **de dentro del contexto de comando**, sin comprobar nulo | Sin contexto, salta un `NullReferenceException` que nadie observa, y **el resultado no se completa jamás** |
| `ExecutionResult.Callback` | Pone `m_completed` e invoca la continuación dentro de un `catch` que abre el diálogo de error de AutoCAD | `IsCompleted` solo pasa a verdadero cuando corre ese `Post`. Una continuación que lanza abre el diálogo |
| `OnCompleted(Action)` | `stfld m_continuation` | Una sola ranura: una segunda llamada sustituye a la primera |
| `GetAwaiter()` / `GetResult()` | `return this` / `ret` | **No hay canal de fallo:** `GetResult` no hace nada |

**Cómo lo usa el arnés:**
- «`Task.IsCompleted`» del encargo es `ExecutionResult.IsCompleted`;
- engancha `OnCompleted` **una sola vez**, y solo si el resultado no estaba completo. La continuación cruda (`continuation.raw`) salta al planificador capturado (`continuation.scheduled`, `continuation.ran`);
- si ya estaba completo, programa la continuación directamente (`continuation.not-needed`);
- `GetResult()` se llama por completar el patrón. Su campo es `getResult=returned(no-op)`; la versión anterior decía `getResultFault`, que era engañoso (H-07).

**Las reglas de lectura** (H-07):
- `IsCompleted=False` en `call.returned` es lo esperado **también en síncrono**. El modo lo dicen `callbackEntered`/`callbackExited`, no `IsCompleted`;
- `callback.exit row2-prediction=` predice la fila 2: con `NEVER (no SynchronizationContext…)`, la respuesta es «No», de forma determinista;
- `continuation.raw` corre en el hilo del contexto publicado; `thread=` y `onPanelThread=` lo dicen;
- `harness.attach acCoreMgdFileVersion=` tiene que ser 25.0.171. Si no lo es, hay que volver a leer estos hechos antes de interpretar.

**Las consecuencias para el paquete**, que no son defectos de H0 y se deciden fuera:
- D-T4C-04 supone «una sola `ContinueWith(…, scheduler)` sobre el `Task` devuelto», y su guarda exige un `ContinueWith(` y prohíbe `GetAwaiter`/`GetResult`. No se puede escribir así;
- «si la llamada lanza y el callback no ha empezado»: un rechazo nativo **no lanza**;
- la `OnCompleted(fault)` del corredor no recibe nunca un fallo, y la excepción «del `Task`» de D-T4C-05 no tiene canal;
- la compleción depende del contexto de sincronización **de dentro** del contexto de comando, no del de la petición;
- el producto tendrá que enganchar `OnCompleted` exactamente una vez, con una continuación total.

---

## 2. Los archivos y por qué

Todo lo nuevo está bajo `#if DEBUG`. Los cuatro archivos nuevos están envueltos enteros.

| Archivo | Cambio | Por qué |
|---|---|---|
| `src/RackCad.Application/Diagnostics/H0Trace.cs` (nuevo) | El sumidero de la traza: `RackLog.Warning` con contexto `[H0]`, secuencia de 7 dígitos, hora monótona (`Stopwatch`, ms con 3 decimales), hora UTC con 7 decimales, hilo y fase. Además: la profundidad del bloqueo exterior; `LogAndContinueSearch` para filtros de excepción, que devuelve `false`; y **el tope de los `Object*`** (H-08): 200 líneas por segmento y, para el resto, un resumen por fuente y tipo (`event.Object.suppressed`) justo antes de la línea que cierra el segmento | Una sola forma de línea. Application lo pueden usar el Plugin y la UI |
| `src/RackCad.Plugin/Diagnostics/H0HostTrace.cs` (nuevo) | Lee los hechos del host sin abrir nada y nunca lanza: id nativo, `CommandInProgress`, `IsQuiescent`, `TopTransaction`, `LockMode(false/true)`, `IsApplicationContext`, `SynchronizationContext.Current`, y el tipo y el handle de un `DBObject`. Incluye la traza de los bloqueos anidados y la línea previa al Assert del drenaje | Filas 8, 9 y 12 |
| `src/RackCad.Plugin/Diagnostics/WorkspaceH0Harness.cs` (nuevo) | El arnés: los dos ganchos, la guarda de vuelo (H-01), el rechazo fuera de reposo (H-09), T0, la llamada, el callback, la corrida, las continuaciones (H-03), las dos sondas de WPF (H-06), los fotogramas, los ensamblados cargados (H-15) y las variables de inyección (H-04) | Los pasos 1 y 2 del encargo |
| `src/RackCad.UI/Diagnostics/WorkspacePanelViewH0.cs` (nuevo) | `WorkspaceH0Hooks` (los dos delegados y `CountFrames`) y la sección «Sonda H0» del panel, construida en código: los dos botones, **que no toman el foco** (H-02), la casilla «Contar fotogramas de WPF (fila 1)» (H-16) y una línea de estado | La UI no puede nombrar AutoCAD. El XAML del producto no cambia |
| `src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs` | `#if DEBUG AttachH0Probe(); #endif` al final del constructor | Monta la sección |
| `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs` | `H0Session` y `H0Rack`, `internal` y de solo lectura | La sesión y el rack exactos de la superficie del panel |
| `src/RackCad.Plugin/Workspace/WorkspaceController.cs` | (1) `WorkspaceH0Harness.Attach(_host, _bridge)`, el accesor solo-Debug al núcleo, en un `try` que registra con `RackLog` si falla (H-15). (2) `panel.notice` en `NotifyStateChanged`. (3) `drain.assert-precheck` antes de `AssertNoOpenTransaction()`. **El `Debug.Assert` no se toca** | Ganchos, fila 5 y fila 12 |
| `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs` | En los 11 manejadores existentes, un gemelo `#if DEBUG … #else <línea original> #endif`. La versión Debug escribe una línea por evento **antes** del núcleo, dentro de su propio `try` (H-15), y después hace lo de hoy. Los `Object*` pasan por el tope de H-08. Además, `drain` registra cada resultado de la pasada de Idle | Filas 3, 4 y 5 |
| `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs` | El inicio y el fin de cada fase: `Begin`, `ReadForPrepare`, `PrepareUnits` por miembro, `BeginMutation`, `Reread`, `WriteUnit`, `Seal`, `Post` (purga, `SyncName`, regeneración y relectura) y los dos `Dispose`. Ninguna sentencia de T4B cambia | Fila 5 |
| `src/RackCad.Plugin/Views/RackSiblingScan.cs` | En `Traverse` (`CaptureDrawing`): `request`, `held`, `threw` (un filtro que devuelve `false`) y **`released`** (en un `finally` solo de Debug, tras los dos `using`, H-10) | Fila 9 |
| `src/RackCad.Plugin/Drawing/RackBlockRenamer.cs` | En `SyncName`: `request`, `held`, **`released`** (H-10) y `threw-and-caught` en su `catch` de hoy | Fila 9 |

**No cambia la lógica de T4B ni del producto:**
- no se toca `RUN`, el núcleo ni el `Debug.Assert`;
- el escritor es el de la base, `new RackPilotUpdateWriter()`;
- no existe billete, cota, `Busy`, canal F-1 ni ámbito propio del producto.

---

## 3. Qué fila de D-T4C-16 mide cada traza

**Las fases.** Cada línea lleva `phase=P<n>:<fase>`. Las fases son `requested`, `returned-awaiting-callback`, `in-callback`, `callback-done` y `task-completed`, o `task-completed-NO-CALLBACK` si el callback no corrió. Con la guarda de vuelo (H-01) solo hay una sonda a la vez, así que la fase no se reetiqueta.

| Fila | Trazas | Cómo se lee |
|---|---|---|
| 1. Momento del callback y si la UI pinta en medio | `T0`, `scheduler`, `call.before`, `call.returned`, `callback.enter`, `callback.exit`, `ui.frame.first`, `ui.frames` | `call.returned` dice `SYNCHRONOUS`, `DEFERRED` o `CALLBACK-STILL-RUNNING`; `IsCompleted` no decide (§1). `ui.frames` cuenta los fotogramas de WPF del hilo del panel por etapa. Con la casilla apagada, sale `ui.frames … off` |
| 2. ¿Se completa siempre? | `callback.exit row2-prediction`, `continuation.attached` o `continuation.not-needed`, `continuation.raw`, `continuation.scheduled`, `continuation.ran`, `probe.flight-released why=completed`, `probe.abandoned` | «Sí» si sale `continuation.ran`. Con `row2-prediction=NEVER`, es «No» determinista. `probe.abandoned` sale cuando un clic posterior encuentra la sonda sin completar tras 2 minutos. `ui.frames why=cap` (30 s) **no** significa «nunca» |
| 3. ¿Entrada en reposo antes del callback? | `event.EnteringQuiescentState` | Busca una con fase `requested` o `returned-awaiting-callback` |
| 4. Los `*Cancel*` antes de `EXECUTEFUNCTION` | `event.CommandWillStart cmd=…`, `event.CommandCancelled`, `event.CommandEnded`, `event.CommandFailed`, `event.ImpliedSelectionChanged` | Su `cmd=` y su fase respecto de `callback.enter`. Se comparan las sondas con y sin selección (§4). Las sondas fuera de reposo ya no existen (H-09) |
| 5. El orden completo | Los eventos; `event.Object*`; `event.Object.suppressed`; las fases `writer.*`; `lock.nested.*`; `run.outcome`; `callback.exit`; `drain`; `panel.notice` | Se ordena por `#`. `writer.ReadForPrepare.end definitionKeys=[…]` da los handles del rack, para comparar con el `h=` de los `Object*`. `event.Object.suppressed` está donde termina su segmento y cuenta lo no escrito por tipo |
| 6. Un paso de Undo, y Undo/Redo | — | **Solo el operador** (§4). Los `Object*` del `U` y del `REDO` corroboran |
| 7. Undo vacío | Las sondas `P<n>(empty)` y el Actualizar con el campo limpio (`run.outcome status=NothingToApply`, con el bloqueo tomado) | El paso de Undo lo ve el operador, contra su línea de base |
| 8. Al entrar en el callback | `callback.enter` | `activeIsAdmitted`, `isAppCtx`, `active:doc=… cmd='…' quiescent=… top=…`, `lockOthers` y `lockAll` |
| 9. La reentrada del bloqueo | `lock.outer.*`; `lock.nested.CaptureDrawing.request/held/released/threw`; `lock.nested.SyncName.request/held/released/threw-and-caught` | `outerDepth=1` marca lo anidado bajo el exterior. `held` significa que funcionó, y `threw` que lanzó. **El `top=OPEN` de `held` es la propia transacción de esa lectura** (H-10). `released` da los modos de bloqueo tras soltarlo. Las líneas con `outerDepth=0` son lecturas ajenas a H0, como la navegación: ruido |
| 10. El contexto de sincronización | `T0 … syncCtx=…`, `scheduler captured` o `NOT-CAPTURED`, `continuation.ran thread=… onPanelThread=…` | Fuera del hilo del panel, el arnés no lee AutoCAD (H-03) |
| 11. ¿Bombea WPF dentro del callback? | `row11.background-*`, `row11.normal-*`, `ui.frames inCallback=` | **«No»** solo si `row11.background-ran … AFTER`, **y** `row11.normal-ran … AFTER`, **y** `ui.frames inCallback=0`, con `callback.enter sameThreadAsClick=True` (H-06). **«Sí»** si cualquiera dice `BEFORE`, o si `inCallback>0` en el mismo hilo |
| 12. El Assert del drenaje | `drain.assert-precheck` | Se escribe **antes** del Assert. Cuentan también las líneas sin `[H0]` `AutoCadEventBridge.IdleContinuation.OpenTransaction` |

**Otras trazas:**
- `harness.attach` (el pid, el `Stopwatch` y `acCoreMgdFileVersion`);
- `harness.attach.assembly`, `harness.attach.assembly.summary` y `T0.assembly`: la ubicación y el SHA-256 de cada ensamblado RackCad cargado, y `folders=1` (H-15);
- `harness.attach.injection` y `T0 … injection=` (H-04);
- `T0.refused`, `T0.refused-in-flight` y `T0.refused-not-quiescent`;
- `probe.flight-released`;
- `T0.field` y `run.field-after`;
- `run.outcome`: el `EditUpdateOutcome` completo.

---

## 4. Cómo se usa

### 4.1 Lo que prepara y mide la sesión (no el Owner)

**El host:**
- una sola `acad.exe`;
- RELEASE de I-52;
- la copia desechable del dibujo, con el SHA-256 del original medido antes.

**El bundle tiene que estar ausente** (H-12). Se comprueba en lectura:
- las carpetas `ApplicationPlugins` de `%AppData%\Autodesk`, `%ProgramData%\Autodesk` y `%ProgramFiles%\Autodesk`;
- los restos del Autoloader en el registro (HKCU, `Applications`/`Loaded`/`AssemblyMap`).

Un RackCad ya cargado haría que el Plugin Debug se uniera por identidad a otro `RackCad.Application`. Se vería en `harness.attach.assembly`.

**Las variables de inyección** (H-04): `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT`, `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL` y `RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT`.
- Tienen que estar ausentes a nivel de usuario, a nivel de máquina y en el proceso que lanza `acad.exe`.
- En la traza, `harness.attach.injection` tiene que decir `none armed in the process`. Con un `ARMED`, la corrida no vale.

**El DLL:** `<salida>\RackCad.Plugin.dll`, con el SHA-256 del §6, medido por la sesión.
- Tras `RACKPANEL`, `harness.attach.assembly.summary` tiene que decir `loaded=4 … folders=1`, con las cuatro ubicaciones en `<salida>` y los SHA-256 del §6.
- La comprobación se repite en `T0.assembly`.

**La biblioteca de bloques** (H-12): su ruta resuelta (el override de `settings.json`, guía §7.1) y su SHA-256.

**El registro** (H-12):
- antes de arrancar: el nombre del archivo del día UTC y su longitud en bytes. Se custodia solo desde esa posición;
- si la sesión cruza las 00:00 UTC, hay un segundo archivo;
- se ordena por `#`, que tiene 7 dígitos;
- un hueco en `#` es una línea perdida;
- **no se abre el registro en exclusiva mientras se opera:** `RackLog` se traga los fallos de escritura.

**`AcCoreMgd`:** `harness.attach acCoreMgdFileVersion=25.0.171`. Si es otra, ver §1.

**Al terminar:**
- el SHA-256 del original;
- los restos en lectura: los archivos de recuperación y los informes de error de Windows y de Autodesk, si hubo un cierre abrupto.

### 4.2 Los pasos del operador, en orden

**Las reglas de protección** (H-05):
1. Si al cargar aparece un aviso de seguridad, elige **«Cargar una vez»**, nunca «Cargar siempre».
2. Ten abierta **solo la copia**: ningún otro dibujo.
3. Si AutoCAD se cierra de golpe, **no envíes ningún informe** y, al volver a abrir, **no recuperes nada sobre el original**.

**Las reglas del foco y del teclado** (H-02):
- antes de escribir `U` o `REDO`, haz clic en la línea de comandos. El primer clic puede solo activar la ventana, así que comprueba que el cursor está allí;
- **nunca pulses Enter ni la barra espaciadora con el foco en el panel.** Los botones H0 ya no toman el foco, pero otros controles del panel sí.

**Las reglas de la sonda:**
- pulsa las sondas solo con la línea de comandos libre: fuera de reposo, la sonda no pide nada (H-09);
- una sola sonda a la vez: un segundo clic no pide nada y lo dice (H-01);
- espera a que el recuadro diga «terminado», **hasta 2 minutos** (H-08). Si dice «abandonada», para y avisa a la sesión.

**Antes de cada sonda, la línea de base** (H-11): abre la lista desplegable de Deshacer y anota las entradas de arriba, o su número. Después de la sonda, anota qué entradas nuevas aparecieron y sus nombres.

**Los pasos:**
1. Abre AutoCAD y abre **la copia** del dibujo.
2. Escribe `NETLOAD` y elige `<salida>\RackCad.Plugin.dll`.
3. Escribe `RACKPANEL`. Debajo del navegador de racks aparece el recuadro «Sonda H0», con dos botones y la casilla «Contar fotogramas de WPF (fila 1)», que está marcada.
4. **Contexto vacío, sin selección** (filas 7 y 4):
   1. con el cursor en la línea de comandos, pulsa Esc dos veces;
   2. toma la línea de base de Deshacer;
   3. pulsa «H0: contexto vacío». Espera a «terminado» y anota.
5. **Contexto vacío, con selección** (fila 4):
   1. haz clic sobre un objeto cualquiera del dibujo, de modo que muestre sus pinzamientos, sin ejecutar ninguna orden;
   2. toma la línea de base de Deshacer;
   3. pulsa «H0: contexto vacío». Espera a «terminado» y anota si la selección desapareció.
6. **Actualizar con el campo limpio** (fila 7, bloqueo sin escritura):
   1. abre el rack piloto en el navegador sin cambiar nada;
   2. toma la línea de base de Deshacer;
   3. pulsa «H0: Actualizar (sonda)». Espera a «terminado» y anota.
7. **Actualizar con cambios** (filas 1 a 6 y 8 a 12):
   1. cambia «Escala de anotación» a otro valor válido y pulsa Enter en ese cuadro: el campo queda con cambios;
   2. toma la línea de base de Deshacer;
   3. pulsa «H0: Actualizar (sonda)» **una vez**. No toques nada hasta «terminado»;
   4. anota si el recuadro dijo «en vuelo», si el dibujo cambió, qué salió en la línea de comandos (por ejemplo, `*Cancel*`) y las entradas nuevas de Deshacer.
8. **Undo y Redo** (fila 6):
   1. haz clic en la línea de comandos, escribe `U` y pulsa Enter. Anota si se deshizo todo el Actualizar de una vez o solo una parte;
   2. haz clic en la línea de comandos, escribe `REDO` y pulsa Enter. Anota si volvió todo.
9. **Repetición** (filas 4 y 5, «en cada Actualizar»): vuelve a dejar el campo con cambios (si el panel pide «Recargar», recarga antes) y repite el paso 7 una o dos veces.
10. **Opcional, para comparar** (H-16): desmarca «Contar fotogramas de WPF» y repite el paso 7 una vez.
11. **Si en cualquier momento AutoCAD se cierra de golpe**, anota el paso. Es la fila 12. Aplica la regla de protección 3.
12. **Al final**, cierra el dibujo **sin guardar** y cierra AutoCAD.

---

## 5. Dónde queda la traza y la privacidad

**El archivo:**
- `%AppData%\RackCad\logs\rackcad-<yyyyMMdd>.log`, con la fecha **UTC** (`RackLog.LogDirectory`);
- las líneas del arnés tienen la forma `[<hora UTC ms>] WARN [H0]: #<7 dígitos> t=<ms monótonos> utc=<HH:mm:ss.fffffff> thr=<hilo> phase=<fase> <etiqueta> <detalle>`;
- las excepciones se pliegan en una línea con ` || `;
- el mismo archivo recibe entradas sin `[H0]` del Plugin y de otras sesiones del día.

**Las reglas de privacidad** (H-13; review §6), antes de custodiar en el repositorio público:
1. **Solo la ventana de la sonda:** desde la posición medida antes de arrancar (§4.1). Van las líneas `[H0]` y las sin `[H0]` que sirven para las filas 9 y 12, redactadas.
2. **Las rutas dentro de las excepciones y de `harness.attach*`:**
   - las rutas del PDB Debug se sustituyen por `<worktree>`;
   - `%AppData%` y el perfil del usuario, por `<appdata>`;
   - la biblioteca de bloques, por `<biblioteca>`.
3. **Los textos libres:** el resultado (`reason`, `differences`, `warnings`, `lastCause`, `error`) y el campo pueden nombrar el rack, el proyecto, un cliente o bloques propios del dibujo. Se revisan línea a línea, y también el id del rack si no es un GUID.
4. **Las observaciones del Owner:** las transcripciones de la línea de comandos y los nombres de Deshacer pueden llevar el nombre o la ruta del dibujo. Las capturas, si las hay, solo de AutoCAD, recortadas, sin la barra de tareas ni otras ventanas.
5. **Opcional:** normalizar los ids nativos y el pid a etiquetas (`D1`, `P1`).

**Lo que no hace falta redactar:** los handles, los nombres de comando, los tipos, las horas UTC y los nombres de bloque del catálogo versionado.

**En la custodia:**
- esta nota ya no lleva rutas con el nombre del usuario;
- el parche no tiene rutas locales;
- los registros de build solo llevan rutas de `<worktree-desechable>` y `<salida>`.

---

## 6. El build

**La orden** (el SDK de usuario, 8.0.423):

```
%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -p:UseAutoCADNuGetReferences=true -o <salida> --no-incremental
```

- **El resultado:** «Compilación correcta», **0 avisos, 0 errores**. El registro es `<carpeta de notas>\build-debug.log`.
- **La versión estampada:** `1.0.0+b15072ff4d30e72ed820df8a40bada84729fc331`.
- **Los SHA-256 de la primera versión** (Plugin `0A51DA3D…`) quedan **obsoletos**.

| DLL en `<salida>` | Bytes | SHA-256 |
|---|---|---|
| `RackCad.Plugin.dll` | 358400 | `611808426BAA9A022414A18D1E736DE015626DC127F4EC5FA0FDF6FE1FDAE790` |
| `RackCad.Application.dll` | 2671616 | `850275667364075D058211A98644E6C64322C7BF6BA2BC7F1EDC81EA02907EE2` |
| `RackCad.UI.dll` | 1119232 | `13778C8111D92ED8D87DD637EC0C8A66846CFFF8988F92B2D822BD0E1F7DFD4C` |
| `RackCad.Domain.dll` | 125952 | `0033C322019C48246FB76E1473A86987F0BBC600E5B1C1015466154C199D5F63` |

**Lo que no se ejecutó:** ninguna prueba.

**Las guardas que fallarían:** varias guardas de Core leen el texto de `src` sin mirar `#if` y fallarían sobre este árbol, como es de esperar de un arnés que no se integra. Por ejemplo, la prohibición de `ExecuteInCommandContextAsync` en todo `src` y las fronteras de nombres de H-10 del paquete.

---

## 7. Por qué Release no cambia (repetido tras las correcciones)

1. **Por construcción.**
   - Cada línea nueva está dentro de un `#if DEBUG`.
   - Los archivos nuevos están envueltos enteros.
   - En los manejadores del puente, el `#else` lleva la línea original byte a byte.
   - El `try`, el `catch … when` y los `finally` de `Traverse` y de `SyncName` solo existen en Debug.
2. **Los fuentes efectivos.**
   - **El método:** `<carpeta de notas>\effective_release_diff.py` preprocesa sin `DEBUG` cada archivo tocado y lo compara con `git show b15072ff:<ruta>`.
   - **El resultado:** **11 de 11 IDÉNTICOS**, y los 4 nuevos quedan con 0 líneas efectivas.
   - **La salida:** `effective-release-diff.out`.
3. **Byte a byte.**
   - **Los dos builds:** Release de la base (`git archive b15072ff`) y Release del árbol corregido.
   - **Las opciones de los dos:** `-c Release -p:UseAutoCADNuGetReferences=true -p:DebugType=none -p:DebugSymbols=false -p:SourceRevisionId=b15072ff4d30e72ed820df8a40bada84729fc331 --no-incremental`.
   - **Los registros:** `build-release-base.log` y `build-release-h0.log`.
   - **El resultado:** los cuatro DLL son **idénticos**. Ninguno contiene `WorkspaceH0Harness`, `H0Trace` ni `WorkspaceH0Hooks`.
   - **La limpieza:** la carpeta temporal `<salida>\_relcheck` se borró después.

| DLL Release | SHA-256 (igual en base y en arnés) |
|---|---|
| `RackCad.Plugin.dll` | `4CCA677884A89EB7E5A5F9E96F86EB412D8A42146B57CDE43057A1F4D5F5E792` |
| `RackCad.Application.dll` | `4260DFAC77C52E0CA02FED8A4DFA96716EF72B698B0AD1B670A8659CB624437B` |
| `RackCad.UI.dll` | `0E5CC5CED16022BC74A34E99663A0D2CE58BC9044929EC7AD8511EBE20B4BAE8` |
| `RackCad.Domain.dll` | `3AD45EE7E26307D28545B84AD9625D323C339623C0FE6ED72F90C7B90BBDAEC3` |

---

## 8. Lo que el arnés NO mide, y lo que observa el operador

**Solo el operador:**
- **la fila 6:** si un Actualizar es un solo paso de Undo, y si `U` y `REDO` lo deshacen y lo rehacen enteros;
- **la fila 7:** si el contexto vacío, o el Actualizar sin escritura, deja un paso de Undo, contra la línea de base;
- **la fila 12:** el `Debug.Assert` del drenaje (`CTL:267-273`) no se toca. Si falla sin depurador, .NET termina el proceso con `Environment.FailFast`: **AutoCAD se cierra de golpe**. La traza escribe antes `drain.assert-precheck top=OPEN`;
- **qué ve en pantalla:** los `*Cancel*`, si el dibujo se actualizó y los nombres de Deshacer.

**Lo que no existe en la base y no se mide** (el diseño de T4C):
- el billete, la cota y `Busy`;
- F-1;
- `PassGate` como puerta. El arnés solo aplica su regla de reposo para no pedir nada fuera de reposo (H-09);
- el corredor;
- D-T4C-07;
- **el ámbito de mutación propia** (D-T4C-09). Sin él, se espera que el drenaje posterior marque el borrador como posiblemente obsoleto. Las filas 4 y 5 salen del **orden** de los eventos, no del marcado.

**Las perturbaciones, declaradas:**
- **la escritura síncrona.** Cada línea se escribe en el acto. Con el tope de H-08, como mucho 200 líneas de `Object*` por segmento, más un resumen. El **orden** de los eventos de AutoCAD no cambia, pero sí las horas absolutas;
- **los fotogramas** (H-16). `CompositionTarget.Rendering` fuerza el repintado de WPF mientras dura la sonda, hasta 30 s o 1800 fotogramas. Eso cambia la cadencia del bucle de mensajes (Idle y el despachador) y puede **desplazar** `drain` y `panel.notice` respecto del callback. El orden de los eventos de AutoCAD no cambia.
  - Las filas 1 y 5 se leen en la corrida con fotogramas, y `drain` y `panel.notice` se contrastan con la repetición sin fotogramas (paso 10).

**Otros límites:**
- `ui.frames` cuenta los fotogramas de WPF del panel, no los del lienzo de AutoCAD;
- si el callback corriera en otro hilo (`sameThreadAsClick=False`), las sondas de la fila 11 ya no prueban el bombeo. La traza lo dice;
- lo que el arnés añade por su cuenta: el handle (`h=`) de los `Object*`, leído de `ObjectId.Handle` sin abrir el objeto, y las ubicaciones de los ensamblados (H-15).

---

## 9. Correcciones tras la revisión

Una fila por hallazgo de `h0-harness-review.md`.

| Id | Gravedad | Estado | Lo que se hizo |
|---|---|---|---|
| H-01 | IMPORTANTE | **Corregido** | La guarda de vuelo: `Interlocked.CompareExchange` sobre el id de la sonda, compartida por los dos botones. Se pone antes de `T0` y se quita en la continuación planificada (`probe.flight-released why=completed`) o en todo camino que no deja continuación (`not-requested`, `call-threw`, `result-null`, `no-continuation`, `launch-threw` y otros). Un segundo clic registra `T0.refused-in-flight` y no pide nada. Si la sonda en vuelo pasa de 2 minutos, el clic siguiente registra `probe.abandoned` y libera la guarda, pero ese clic tampoco pide nada |
| H-02 | IMPORTANTE | **Corregido** | `Focusable = false` e `IsTabStop = false` en los dos botones y en la casilla nueva. En §4.2, las reglas del foco y del teclado: clic en la línea de comandos antes de `U` y `REDO`, y nunca Enter ni la barra espaciadora con el foco en el panel |
| H-03 | IMPORTANTE | **Corregido** | `continuation.raw` y `continuation.ran` leen AutoCAD solo si `probe.Dispatcher.CheckAccess()`. Si no, solo los hilos (`onPanelThread=False … AutoCAD is not read`). `DetachFrames` y el texto del panel van por `Dispatcher.BeginInvoke` cuando no se está en el hilo del panel |
| H-04 | IMPORTANTE | **Corregido** | `harness.attach.injection` registra, para cada una de las tres variables, si está puesta en el proceso, en el usuario y en la máquina, sin su valor. Con alguna puesta en el proceso, dice `ARMED … STOP`. `T0` repite `injection=`. En §4.1, la comprobación de la sesión |
| H-05 | IMPORTANTE | **Corregido** | Las tres reglas de protección del host en §4.2: «Cargar una vez», solo la copia abierta, y ni informes ni recuperación sobre el original. En §4.1, la sesión registra los restos y el SHA-256 del original |
| H-06 | MENOR | **Corregido** | La regla de lectura de la fila 11 (§3). Y la segunda sonda, `row11.normal-*`, a prioridad `Normal` |
| H-07 | MENOR | **Corregido** | `getResultFault` pasa a `getResult=returned(no-op)`. §1 recoge los hechos del IL, las reglas de lectura y las consecuencias para D-T4C-04 y D-T4C-05. `callback.exit` lleva `row2-prediction`, y `harness.attach` lleva `acCoreMgdFileVersion` |
| H-08 | MENOR | **Corregido** | El tope de 200 líneas de `Object*` por segmento, con el resumen `event.Object.suppressed` por fuente y tipo al cerrar el segmento. En §4.2, esperar a «terminado» hasta 2 minutos |
| H-09 | MENOR | **Corregido** | `T0.refused-not-quiescent`, con la regla de reposo de `PassGate` (`IsQuiescent` y sin comando activo para el puente), en los dos botones. No se pide nada |
| H-10 | MENOR | **Corregido** | `released`, con `lockOthers` y `lockAll`, tras los dos `using` de `CaptureDrawing` y de `SyncName`, en un `finally` solo de Debug. §3 documenta que el `top=OPEN` de `held` es la propia transacción |
| H-11 | MENOR | **Corregido** | Las variantes en §4.2: la línea de base de Deshacer antes de cada sonda, el contexto vacío con y sin selección, el Actualizar con el campo limpio (`NothingToApply` con bloqueo) y la repetición |
| H-12 | MENOR | **Corregido** | §4.1 lista lo que mide la sesión: la biblioteca, la posición del registro, el segundo archivo, el orden por `#` (ahora con 7 dígitos), los huecos, la ausencia del bundle y del Autoloader, las variables, los ensamblados y `acCoreMgdFileVersion` |
| H-13 | MENOR | **Corregido** | Las 5 reglas de privacidad en §5. Esta nota ya no lleva rutas con el nombre del usuario: usa `%TEMP%`, `%AppData%` y `<carpeta de notas>` |
| H-14 | MENOR | **Parcial. El resto es de proceso y no me toca** | Build nuevo, con los SHA-256 del §6, y la misma comprobación de Release del §7. Quedan para la sesión y el revisor la medición del SHA-256 por la sesión y la revisión focalizada del delta. La custodia registra el orden: build 1, revisión, correcciones, build 2 |
| H-15 | MENOR | **Corregido** | `harness.attach.assembly` y `T0.assembly` registran el nombre, la ubicación, el SHA-256 y la versión de cada ensamblado RackCad cargado. `…summary` da `folders=` y los que faltan. Cada llamada de traza del puente va en su propio `try`, y `Attach` va en un `try` del controlador que registra con `RackLog` |
| H-16 | MENOR | **Corregido** | Declarado en §8, con la lectura de las filas 1 y 5. Además, la casilla «Contar fotogramas de WPF (fila 1)» (`WorkspaceH0Hooks.CountFrames`) permite la repetición sin fotogramas del paso 10 |
