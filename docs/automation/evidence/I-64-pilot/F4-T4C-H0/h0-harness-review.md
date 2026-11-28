# I-64 F4-T4C-H0: revisión adversarial independiente del arnés

**Veredicto: APTO TRAS CORRECCIONES.** Hay 16 hallazgos: 0 BLOQUEANTES, 5 IMPORTANTES y 11 MENORES (tabla del §7).

**Qué se revisó:**
- el parche `h0-harness.patch`: 1416 líneas, 7 archivos modificados y 4 nuevos. Su SHA-256 es `56a8b53b…71aed6f`, el mismo que declara la nota;
- la nota `h0-harness-notes.md`;
- el encargo `_harness-brief.md`;
- D-T4C-16 del paquete de F4-T4C, con su tabla de 12 filas, y las decisiones a las que remite (D-T4C-02 a D-T4C-05, D-T4C-09 y D-T4C-15);
- el código de la base `b15072ff` que el parche toca o llama.

**Método, solo lectura:**
- **El parche y el worktree.** Leí los archivos del worktree desechable y comparé su `git diff` con el parche. La parte de los archivos con seguimiento coincide exactamente, y el contenido de los 4 archivos nuevos está en el parche.
  - Usé `git diff` y `git status`. `git status` puede refrescar la caché de estado del índice del worktree, pero no cambia ningún contenido.
- **Release.** Escribí una comprobación propia, independiente del script del autor: `review_release_check.py`, en la carpeta de notas.
- **El ensamblado de AutoCAD.** Leí los metadatos y el IL del `AcCoreMgd.dll` instalado de AutoCAD 2025 (versión de archivo 25.0.171) con `System.Reflection.Metadata`, mediante `review-il-*.ps1` en la misma carpeta. Ninguno de estos scripts ejecuta código de AutoCAD.
- **Lo que no hice:** no compilé, no ejecuté pruebas, no abrí AutoCAD y no escribí nada en el worktree ni en ningún repositorio.
- **La notación:** `patch Lnnn` es una línea de `h0-harness.patch`. `archivo:nnn` es una línea de la base `b15072ff`, salvo que se diga otra cosa.

---

## 1. Seguridad en el host real

### 1.1 Escrituras y estado persistente

**El arnés solo escribe en `RackLog`.**
- `H0Trace.Log` llama a `RackLog.Warning("[H0]", …)` (patch L636).
- `RackDiagnosticsLog.Write` hace un `File.AppendAllText` por línea en `%AppData%\RackCad\logs\rackcad-<yyyyMMdd UTC>.log` y se traga cualquier fallo (`RackDiagnosticsLog.cs:42-51`).

**No escribe en ningún otro sitio:**
- no toca el registro de Windows, el perfil de AutoCAD, `settings.json` ni variables del sistema;
- no abre ninguna transacción;
- solo lee:
  - su propio DLL, para el SHA-256 (patch L1203-1217);
  - la memoria del núcleo (`GetField`, que es «Memory only», `WorkspaceHostCore.cs:1029-1041`);
  - hechos del host (patch L685-720).

**Lo que escribe el camino del producto que el arnés ejercita** (T4B, sin cambios):
- el dibujo abierto, y nada más;
- **lee** los catálogos de la carpeta de salida y la biblioteca de bloques:
  - la biblioteca sale del override de `settings.json` (`BlockLibrary.cs:19-22`), porque los `assets/catalogs` del worktree no traen ningún `.dwg`;
  - se abre con `OpenForReadAndAllShare` (`BlockLibraryDatabaseCache.cs:38`).
- **La única escritura de perfil del producto,** latente y previa a H0: `UserSettingsStore.Load` pone en cuarentena un `settings.json` corrupto como `.bad` (`UserSettings.cs:110-111`). No la activa el arnés.

**El estado persistente que puede dejar AutoCAD**, y que §4 de la nota no trata:
- **el aviso de seguridad de `NETLOAD`.** Con `SECURELOAD` activo y la carpeta de salida fuera de las rutas de confianza, aparece un aviso. Si se elige «Cargar siempre», la carpeta queda registrada como de confianza en el perfil (H-05);
- **un cierre abrupto.** El `Debug.Assert` de la fila 12 termina el proceso con `FailFast`. Eso puede dejar:
  - informes de error de Windows o de Autodesk, que se ofrecen para enviar;
  - archivos de recuperación;
  - el trabajo sin guardar de **cualquier otro dibujo abierto**, que se pierde (H-05).

El MRU, los `.dwl` y el autoguardado son normales y no afectan al original.

### 1.2 Cuelgues, transacciones, bloqueos y excepciones

**El bloqueo y las transacciones:**
- **el bloqueo exterior** (patch L1040-1062) va en un `using`, y el par `EnterOuterLock`/`LeaveOuterLock` está equilibrado en `try`/`finally`. No queda ningún bloqueo abierto en ningún camino;
- **el arnés no abre ninguna transacción.** Las de T4B no cambian (§3).

**Todo callback y todo manejador es total:**
- **el callback de `ExecuteInCommandContextAsync`** (patch L995-1026):
  - `catch` general, y el `finally` con su propio `try`;
  - devuelve `Task.CompletedTask`;
  - **es imprescindible:** el trampolín nativo `myCallback` de AutoCAD **no tiene ninguna región de excepción** (IL, §5). Una excepción del callback cruzaría a código nativo;
- **la continuación cruda** (patch L963-977) y la planificada (patch L1090-1123) son totales;
  - AutoCAD invoca la cruda dentro de un `catch` que abre su diálogo de excepción (`CerOrShowExceptionDialog`, §5). Que sea total evita ese diálogo;
- **los fotogramas** (patch L1144-1158), **la sonda `Background`** (patch L1068-1086) y **los clics** (`Contained`, patch L1383-1406) están contenidos;
- **en los manejadores del puente,** `H0Event` va **fuera** de `ContainSession` (patch L311-423), pero su cuerpo es total (patch L446-461).
  - El único escape posible es un `TypeLoadException` en la compilación JIT si se cargara un juego de DLL mezclado. Ver H-15.

**Los argumentos de `H0Trace.Log` se evalúan fuera de su `try`**, por ejemplo el `new StackFrame(1, false)` de patch L535. Todos son lecturas puras que en la práctica no lanzan. Riesgo despreciable.

**No hay esperas activas** (`Wait`, `Result` o bucles), y `GetResult()` es un método vacío (§5).
- Los fotogramas se desenganchan al completar o al tope de 30 s o 1800 fotogramas (patch L1152).
- **No hay cuelgue posible, pero sí lentitud:** cada evento `Object*` hace una escritura síncrona en disco (H-08).

**El camino de respaldo sin contexto de sincronización** (patch L884-893):
- si `FromCurrentSynchronizationContext` falla, el planificador es `Default`;
- entonces `OnCompleted` corre en un hilo del pool y llama a `ContextFacts()` (patch L1115): `MdiActiveDocument`, `Editor.IsQuiescent`, `TopTransaction` y `LockMode`, es decir, **API nativa de AutoCAD fuera del hilo principal**;
- `Safe` no protege de una violación de acceso, que en .NET no se puede capturar;
- es justo el caso «No hay contexto» que la fila 10 prevé (H-03).

**El filtro de excepción de `Traverse`** (patch L284) corre en la primera pasada, **antes** de los `Dispose` de sus `using`, es decir, con el bloqueo anidado y la transacción de lectura aún abiertos.
- Solo formatea la excepción y escribe en disco: no llama a AutoCAD y devuelve `false`.
- No cambia nada.

### 1.3 Un doble clic, la tecla Enter y un clic fuera de reposo

**Un doble clic es peligroso para la validez, no para el dibujo.**
- **La llamada nunca informa de un rechazo nativo.** El IL de `ExecuteInCommandContextAsync` llama a `beginExecuteInCommandContext` y **descarta su estado de error con `pop`** (§5). Así, una segunda petición con la primera pendiente tiene dos desenlaces posibles:
  - **AutoCAD la rechaza en silencio.** Su resultado nunca se completa, y aparece un «No» falso en la fila 2, que es el camino de C05;
  - **o la encola.** Entonces se crea otro `EXECUTEFUNCTION` y otra entrada de Deshacer, y se contaminan las filas 6 y 7.
- **`H0Trace.Phase` es global** (patch L600-607). Una segunda sonda reescribe la fase con la que se etiquetan los eventos del puente de la primera, y eso afecta a las filas 1, 3, 4 y 5.
- **El dibujo está a salvo:** la ranura de T4B (`TryBeginApply`) y la comprobación previa impiden una escritura concurrente. La segunda corrida da `AnotherUpdateInProgress` o `NothingToApply`.

Ver H-01.

**La tecla Enter vuelve a pulsar la sonda.**
- La paleta se construye con `KeepFocus = true` (`WorkspacePaletteHost.cs:52`), y su comentario dice que el teclado real queda para el smoke (`:21`).
- Un `Button` de WPF toma el foco de teclado al pulsarlo con el ratón, y **Enter o la barra espaciadora lo vuelven a pulsar**.
- El paso 7 de la nota dice «escribe `U` y Enter, con el teclado». Si el foco sigue en «H0: Actualizar (sonda)», el Enter lanza otra sonda en lugar del `U`, y la fila 6 queda falseada. Eso podría disparar un C02 falso. Ver H-02.

**Un clic fuera de reposo** no es peligroso en una copia, pero mide una situación que el producto nunca admitiría.
- El arnés no aplica `PassGate`: solo lo registra en `T0` (patch L830-835).
- `ExecuteInCommandContextAsync` cancela el comando en curso, y sus `*Cancel*` se confunden con los de la fila 4. Ver H-09.

### 1.4 Los pasos del operador (§4 de la nota)

**Las cuatro salvaguardas pedidas son correctas y necesarias:**
- la copia del dibujo, con el SHA-256 del original;
- cerrar sin guardar;
- una sola `acad.exe`;
- sin el bundle cargado.

**No bastan.** Faltan:
- **el foco:** hacer clic en la línea de comandos antes de escribir, y no pulsar Enter ni la barra espaciadora con el foco en el panel (H-02);
- **las variables de inyección de fallos del DLL Debug,** que la sesión tiene que comprobar ausentes (H-04);
- **`SECURELOAD`:** elegir «Cargar una vez» (H-05);
- **ningún otro dibujo abierto con cambios** (H-05);
- **el camino del cierre abrupto:** no enviar informes y no recuperar sobre el original (H-05);
- **las líneas de base y las variantes que piden las filas 4, 6 y 7** (H-11);
- **los datos de máquina que debe medir la sesión:** la biblioteca, la posición del registro y la comprobación del bundle (H-12).

---

## 2. Release intacto

**La afirmación es cierta.** Hay cuatro pruebas independientes.

1. **Por construcción:**
   - **los 4 archivos nuevos** empiezan con `#if DEBUG` y terminan con `#endif` (patch L579/L662, L669/L747, L754/L1289, L1296/L1416);
   - **los 7 modificados** solo añaden bloques `#if DEBUG … #endif`, o `#if DEBUG <variante> #else <línea original> #endif` en los 11 manejadores del puente (patch L310-424);
   - en esos manejadores, la línea original es **contexto** del diff, no una línea añadida;
   - el `try`/`catch when` de `Traverse` existe solo en Debug (patch L265-288).
2. **Mi comprobación independiente** (`review_release_check.py`):
   - **el parche no borra ninguna línea;**
   - **cada una de sus 1059 líneas añadidas** es `#if DEBUG`, `#else` o `#endif`, o cae dentro de una región que solo compila DEBUG. Lo confirma en los 11 archivos;
   - **ninguna línea añadida** queda en una rama `#else`.
3. **La configuración:**
   - ningún `.csproj` ni `Directory.Build.props` cambia;
   - no hay ningún `DefineConstants` propio, así que Release no define `DEBUG`;
   - el XAML no cambia: la sección de la sonda se construye en código (patch L1328-1373);
   - `Deterministic=true` (`Directory.Build.props:34`).
4. **Los DLL.** El build Release byte a byte de la nota, con los DLL idénticos, es coherente con todo lo anterior. No lo repetí, porque está prohibido compilar.

**Una advertencia que no cambia esto.** El DLL que se opera es **Debug**, y lleva comportamiento Debug de la base que Release no tiene:
- el `Debug.Assert` del drenaje (`WorkspaceController.cs:268-272`);
- la comprobación del puente tras el drenaje (`AutoCadEventBridge.cs:283`, `:289`);
- **tres puntos de inyección de fallos por variable de entorno** (H-04).

---

## 3. La lógica de T4B, sin tocar

**`RackPilotUpdateWriter`** (patch L40-254): 25 trazas, todas aditivas.
- **Sus argumentos son lecturas puras:** conteos, claves y `request.*`.
- **Dos leen el host:**
  - `writer.Begin` llama a `DocumentFacts(MdiActiveDocument)` (patch L53);
  - `BeginMutation.begin` y `Post.begin` leen una vez más `TopTransaction` (patch L119, L136). Son las mismas consultas, sin efectos, que T4B ya hace en `EnsureOwned`, `BeginMutation` y `Post`.
- **El orden no cambia:**
  - `Seal.commit.returned` queda entre `Commit()` y `FailSealIfRequested("after")` (patch L237-243);
  - las trazas de los `Dispose` van antes de la liberación (patch L190-192, L251-253).
- **No cambia** ninguna excepción, ningún retorno ni ninguna transacción.

**`RackSiblingScan.Traverse`** (patch L265-288). El filtro devuelve `false`, así que la excepción viaja igual, y el `throw;` del `catch` es inalcanzable. Lo único nuevo es que el registro ocurre antes del `Dispose` (§1.2). La lectura de `held` llama a `DocumentFacts` con la transacción de la lectura ya abierta: es una consulta.

**`RackBlockRenamer.SyncName`** (patch L9-28): `request` y `held`, y el registro dentro del `catch` de hoy, **antes** de `RackLog.Exception`. La excepción se sigue tragando igual.

**El puente:**
- `H0Event` solo lee `TryGetSession`, `PendingHintCount`, `IsDegraded` e `IsCommandActiveInActiveSession` (patch L450-453);
- las cuatro son lecturas puras (`EventBridgeCore.cs:84-89`, `:103`, `:144`; `WorkspaceSessionRegistry.cs:23-27`; `WorkspaceSession.cs:29`), y **ninguna está entre las 13 entradas que olvidan `_lastAllowedClose`** (H-18). La regla de olvido no cambia;
- la única interacción nueva con el host en los `Object*` es `e.DBObject` (patch L377-423). Es el envoltorio del objeto que AutoCAD ya tiene abierto para la notificación: el arnés no lo abre ni lo libera, y solo lee `GetType().Name` y `ObjectId.Handle`. Riesgo residual bajo;
- `H0Drain` enumera `Operations` antes de `Drained`. Es puro.

**El controlador** (patch L510-536): `Attach`, que es total; la traza previa al Assert, sin tocarlo; y la línea de `panel.notice`, antes de `_noticeDepth.Enter()`. Ninguna cambia lo que se cablea ni el orden.

**La corrida misma** (patch L1046-1049): `new RackPilotUpdateWriter()` y `_host.TryUpdatePilot(request, writer)`, exactamente como lo llamaría `Execute`. Lo que se salta está declarado en la nota §8: la compuerta de avisos, `PassGate`, el billete y el ámbito propio. Tampoco cambia T4B.

**Conclusión:** las trazas no cambian el orden, las excepciones ni las transacciones de T4B.

---

## 4. La validez de cada medida (D-T4C-16, fila por fila)

| # | ¿Mide lo que dice? | Distorsión posible | ¿Falta algo para decidir? |
|---|---|---|---|
| 1 | **Sí.** El modo sale de las marcas `callbackEntered`/`callbackExited` en `call.returned` (patch L926-932), no de `IsCompleted`. Hay hora antes, al volver, al entrar y al salir; `ui.frames` por etapa | `IsCompleted=False` al volver es lo esperado **también en síncrono**: solo se pone a verdadero cuando corre el `Post` (§5). No es evidencia. Además, los fotogramas fuerzan el repintado continuo de WPF (H-16) | No |
| 2 | **Sí:** `continuation.raw` y `continuation.ran` | Una segunda petición puede producir un «No» falso (H-01). `getResultFault` siempre dirá `none` (H-07). `why=cap` llega a 1800 fotogramas o a 30 s, lo que ocurra antes; no significa «nunca» | Una regla de lectura (§5): con `syncCtx=null` en `callback.exit`, AutoCAD no completará el resultado **nunca**. Es un «No» determinista |
| 3 | **Sí:** `event.EnteringQuiescentState` en fase `requested` o `returned-awaiting-callback`. Cada evento crudo es lo que contaría la cota de D-T4C-09 | Ninguna material | No |
| 4 | **Sí:** `cmd=` en `CommandWillStart`, `Ended`, `Cancelled` y `Failed`, y su fase | Un clic fuera de reposo mete `*Cancel*` ajenos (H-09) | Una sonda con selección (PICKFIRST) y otra sin ella: los `*Cancel*` de F2 vaciaban PICKFIRST (H-11) |
| 5 | **Sí:** las fases del escritor, los `Object*` con tipo y handle, `lock.nested.*`, `run.outcome`, `callback.exit`, los eventos de comando, `drain` y `panel.notice`. El `Begin` del ámbito propio del producto corresponde aquí a `run.begin` | La escritura síncrona por evento alarga las fases (H-08). El repintado forzado cambia la cadencia de Idle y puede **mover** `drain` y `panel.notice` respecto del callback (H-16). El **orden** de los eventos de AutoCAD no cambia | No. Como complemento, los `Object*` del `U` y del `REDO` quedan en la traza: corroboran la fila 6 |
| 6 | Solo el operador | El Enter puede volver a pulsar la sonda en lugar del `U` (H-02) | La línea de base de Deshacer antes de cada sonda (H-11) |
| 7 | **Sí,** solo para el `EXECUTEFUNCTION` vacío: la sonda vacía no toma el bloqueo ni lee | — | La variante «bloqueo sin escritura», un Actualizar con el campo limpio que da `NothingToApply`. Es uno de los casos que nombra D-T4C-15 (H-11) |
| 8 | **Sí:** `callback.enter` con `activeIsAdmitted`, `cmd`, `quiescent`, `top`, `lockOthers` y `lockAll`, antes del bloqueo | — | No |
| 9 | **Sí:** `request`, `held` y `threw`, con `outerDepth` | `held` se escribe tras el `LockDocument` **y** el `StartTransaction`, así que su `top=OPEN` es la propia transacción. También salen líneas `outerDepth=0` de lecturas ajenas a H0, como la navegación: es ruido | Falta `released`, con el modo de bloqueo tras soltar cada anidado (H-10) |
| 10 | **Sí:** `syncCtx` en `T0`, `scheduler captured`, `continuation.ran onUiThread` | El respaldo `Default` lee AutoCAD fuera de hilo (H-03) | No |
| 11 | **Solo en parte.** «BEFORE» prueba que hubo bombeo; «AFTER» no prueba lo contrario | WPF aplaza `Background` mientras hay entrada de Win32 pendiente (H-06) | La regla: «No» solo si `row11 … AFTER` **y** `ui.frames inCallback=0` (prioridad Render) |
| 12 | **Sí:** `drain.assert-precheck` justo antes del Assert. La escritura síncrona sobrevive a `FailFast` | Una mutación más larga por las trazas aumenta un poco la exposición (H-08) | Las líneas sin `[H0]` del puente (`AutoCadEventBridge.IdleContinuation.OpenTransaction`) también cuentan, así que se custodian en la ventana de la corrida (H-12, H-13) |

**Las tres preguntas del encargo:**
- **el `BeginInvoke` de la fila 11** solo registra. No crea ninguna reentrada de producto, pero mide una sola clase de prioridad (H-06);
- **las trazas dentro de los manejadores** no reordenan nada. Todo es síncrono en el hilo principal, y el orden lo fija AutoCAD; las trazas solo alargan (H-08);
- **el salto al planificador** reproduce lo que el producto tendrá que hacer: `OnCompleted` y después el planificador capturado. La fila 10 mide justo ese hilo.

---

## 5. El hecho nuevo: `DocumentCollection.ExecutionResult`

La nota acierta: el resultado no es un `Task`. El IL del `AcCoreMgd.dll` instalado (25.0.171) añade hechos que importan.

| Miembro | IL observado | Consecuencia |
|---|---|---|
| `ExecuteInCommandContextAsync` | `newobj ExecutionResult`, `GCHandle.Alloc`, `call beginExecuteInCommandContext` **y `pop`** | **El estado nativo se descarta.** Un rechazo de AutoCAD no lanza y no completa nunca: un callback que no corre es indistinguible de uno diferido hasta que vence la cota |
| `<Module>::myCallback` (el trampolín) | Invoca el `Func<object,Task>`. Después hace `Task.ContinueWith(Action<Task,object>, object, TaskContinuationOptions)` con `ldc.i4 0x00080000` (`ExecuteSynchronously`). **No tiene ninguna región de excepción** | La continuación corre **en línea**, en el mismo hilo, justo al volver el callback. Una excepción del callback cruzaría a código nativo |
| `ExecutionResult.Continuation` | `SynchronizationContext.get_Current` → `ldnull; ldftn Callback; newobj SendOrPostCallback` → `callvirt Post`, **sin comprobar nulo** | La compleción se **publica** en el contexto de sincronización que haya **dentro del contexto de comando**. Si es nulo, salta un `NullReferenceException` en una tarea que nadie observa, y **el resultado no se completa jamás** |
| `ExecutionResult.Callback`, el de instancia | `m_completed = true`; si `m_continuation != null`, la invoca. Hay un `try` cuyo `catch` llama a `UnhandledExceptionFilter.CerOrShowExceptionDialog` | `IsCompleted` solo pasa a verdadero cuando corre ese `Post`. Una excepción de la continuación abre el diálogo de error de AutoCAD |
| `OnCompleted(Action)` | `stfld m_continuation` | **Una sola ranura:** una segunda llamada sustituye a la primera |
| `GetAwaiter()` / `GetResult()` | `return this` / `ret` | **No hay canal de fallo:** `GetResult` no hace nada |

**El arnés lo trata bien:**
- llama a `OnCompleted` **una sola vez**, y solo si el resultado no estaba completo (patch L940-951);
- si ya lo estaba, salta directamente al planificador (patch L942-947). Es correcto, porque en ese caso `Callback` ya corrió con la ranura vacía;
- entre leer `IsCompleted` y enganchar no hay bombeo: la escritura en disco no bombea mensajes, así que no hay carrera;
- lee `IsCompleted` dentro de `Safe` (patch L925, L931 y L1111);
- llama a `GetResult()` dentro de un `try` (patch L1099-1107). Es inocuo, pero su `getResultFault` es siempre `none` (H-07);
- el callback y la continuación son totales, como exigen el trampolín y el `catch` de `Callback`.

**Lo que la nota tiene que añadir:**
- **cómo se lee la traza:**
  - `IsCompleted=False` al volver la llamada no distingue síncrono de diferido;
  - el `syncCtx` de `callback.exit` predice la fila 2: si es nulo, la respuesta es «No», de forma determinista;
  - `continuation.raw` corre en el hilo del contexto publicado;
- **las consecuencias para el paquete,** que no son defectos de H0 y se deciden fuera:
  - D-T4C-04, «si la llamada lanza y el callback no ha empezado»: un rechazo nativo **no lanza**;
  - la `OnCompleted(fault)` del corredor no recibe nunca un fallo;
  - D-T4C-05, «la excepción … del `Task`», no tiene canal;
  - la compleción depende del contexto de sincronización **dentro** del contexto de comando, no del de la petición;
  - el producto tendrá que enganchar `OnCompleted` exactamente una vez, con una continuación total;
- **una condición de validez:** estos hechos valen para la versión de archivo 25.0.171. Si AutoCAD cambia de versión antes de la sesión, hay que volver a leerlos.

---

## 6. La privacidad de la traza (repositorio público)

**Lo que lleva cada línea `[H0]`:**
- la hora UTC, un número de secuencia, la hora monótona y el id del hilo;
- la fase;
- los ids nativos de documento, que son direcciones de memoria del proceso;
- los ids de sesión, el pid y el nombre del DLL, sin ruta (patch L806);
- el SHA-256 y la versión;
- los handles y el id del rack;
- los nombres de comando (`cmd=`);
- los tipos .NET de los objetos;
- los textos del resultado: `reason`, `differences`, `warnings`, `lastCause` y `error`;
- las excepciones completas.

**Hay que redactar antes de custodiar:**
1. **Todo lo que no es de la ventana de la sonda.** El archivo diario es compartido: lleva entradas sin `[H0]` de otras sesiones del mismo día UTC, y pueden incluir mensajes con nombres de dibujos o rutas.
   - Se custodian solo las líneas desde la posición medida antes de arrancar (H-12): las `[H0]` y las sin `[H0]` que sirven para las filas 9 y 12, redactadas.
2. **Las rutas dentro de las excepciones** (`exception.ToString()`, patch L644-645):
   - las rutas de fuente del PDB Debug (`<worktree>\src\…`);
   - las de los catálogos, de `settings.json` y de la biblioteca de bloques. La del override puede estar bajo el perfil del usuario y llevar su nombre.
   - Se sustituyen por `<worktree>`, `<appdata>` y `<biblioteca>`.
3. **Los textos libres del resultado y del campo,** que pueden nombrar el rack, el proyecto, un cliente o bloques propios del dibujo. Se revisan línea a línea. Si el id del rack no es un GUID sino un nombre legible, también.
4. **Las observaciones del Owner.** Las transcripciones de la línea de comandos y los nombres de la lista de Deshacer pueden incluir el nombre o la ruta del dibujo.
   - Si hay capturas, solo de AutoCAD, recortadas y sin la barra de tareas ni otras ventanas.
5. **Opcional:** normalizar los ids nativos y el pid a etiquetas (`D1`, `P1`). No son datos personales, pero no aportan nada fuera de la corrida.

**Lo que no hace falta redactar:**
- los handles;
- los nombres de comando;
- los tipos;
- las horas UTC;
- los nombres de bloque del catálogo versionado, que ya son públicos.

**La custodia, además del registro:**
- **la nota** `h0-harness-notes.md` lleva rutas con el nombre corto del usuario, así que hay que redactarla;
- **el parche** no tiene ninguna ruta local;
- **los registros de build** solo llevan rutas de la carpeta de salida.

---

## 7. Tabla de hallazgos

| Id | Gravedad | Hallazgo | Evidencia | Corrección |
|---|---|---|---|---|
| H-01 | IMPORTANTE | **No hay ninguna guarda de vuelo.** Un doble clic, o el botón vacío con una sonda en vuelo, pide un segundo contexto. AutoCAD descarta el estado nativo, así que la segunda petición puede quedar sin completar nunca, que es un «No» falso en la fila 2 (C05), o crear otro `EXECUTEFUNCTION`, que contamina las filas 6 y 7. La fase global reetiqueta los eventos de la primera sonda | patch L818-877; L600-607; IL `pop` (§5) | Una marca estática `Interlocked`, puesta en `T0` y quitada en la continuación planificada (o con una línea `probe.abandoned` tras el tope). Un segundo clic registra `T0.refused-in-flight` y no pide nada |
| H-02 | IMPORTANTE | **Enter o la barra espaciadora vuelven a pulsar la sonda.** La paleta usa `KeepFocus = true` y el botón conserva el foco. Escribir «`U` y Enter» (paso 7) puede lanzar otra sonda en lugar del `U` y falsear la fila 6, con un C02 falso | patch L1375-1381; `WorkspacePaletteHost.cs:21`, `:52`; nota §4 paso 7 | `Focusable = false` e `IsTabStop = false` en los dos botones H0. En §4: «antes de escribir `U` o `REDO`, haz clic en la línea de comandos (el primer clic puede solo activar la ventana) y comprueba que el cursor está allí; nunca pulses Enter ni la barra espaciadora con el foco en el panel» |
| H-03 | IMPORTANTE | **El respaldo sin contexto de sincronización lee AutoCAD fuera del hilo principal.** Con el planificador `Default`, `OnCompleted` llama a `ContextFacts()` desde el pool. `Safe` no captura una violación de acceso. Es el caso «No hay contexto» de la fila 10 | patch L884-893, L983, L1110-1116 | En las continuaciones, leer el host solo si `probe.Dispatcher.CheckAccess()`; si no, registrar solo los hilos. `DetachFrames` y `report`, por `probe.Dispatcher.BeginInvoke` |
| H-04 | IMPORTANTE | **El DLL Debug obedece tres variables de inyección de fallos:** `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT`, `…_SEAL` y `RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT`. Ni la nota ni el arnés las comprueban. Un resto de una sesión anterior convertiría la corrida en un fallo inyectado e invalidaría las filas 5 y 6 | `RackPilotUpdateWriter.cs:217-218`, `:259`, `:292-294`, `:333`; `SiblingRedrawDebugFaultInjection.cs:7-11` | `harness.attach` registra para cada una si está puesta o no, sin su valor. La sesión comprueba que no existen ni a nivel de usuario ni de máquina, ni en el proceso que lanza `acad.exe` |
| H-05 | IMPORTANTE | **§4 no protege el estado del host:** «Cargar siempre» en el aviso de `SECURELOAD` deja la carpeta como de confianza en el perfil; otro dibujo abierto pierde sus cambios con el `FailFast` de la fila 12; y el cierre abrupto ofrece enviar informes y deja archivos de recuperación | nota §4 y §8; `WorkspaceController.cs:268-272` | Añadir a §4: (1) si aparece un aviso de seguridad, «Cargar una vez»; (2) solo la copia abierta, ningún otro dibujo; (3) si AutoCAD se cierra de golpe, no enviar ningún informe y, al volver a abrir, no recuperar nada sobre el original. La sesión registra los restos (recuperación e informes de error) y comprueba el SHA-256 del original |
| H-06 | MENOR | **La fila 11 es unilateral:** `Background` está en la clase de prioridades que WPF aplaza con entrada pendiente | patch L1074 | La regla en la nota: «No» solo con `AFTER` y `inCallback=0`. Opcional: una segunda sonda a prioridad `Normal` |
| H-07 | MENOR | **`GetResult()` está vacío,** así que `getResultFault` siempre dirá `none`. Además, la nota no recoge los hechos del IL del §5 | patch L1098-1107; §5 | Corregir la nota §1 y §3 (fila 2), con las reglas de lectura y las consecuencias para D-T4C-04 y D-T4C-05 |
| H-08 | MENOR | **Una escritura síncrona por cada `Object*`** (abrir, escribir y cerrar, más el antivirus): una importación con miles de eventos puede congelar AutoCAD segundos o minutos y alargar la mutación. El operador podría creer que se colgó. La regla de los «30 segundos» no basta | patch L377-423; `RackDiagnosticsLog.cs:42-48` | Limitar las líneas por evento en cada sonda (por ejemplo, las 200 primeras de cada fase del escritor) y sumar el resto por tipo y fase, escrito en cada frontera de fase. O, al menos, avisar en §4 y esperar a «terminado» con un tope de 2 minutos |
| H-09 | MENOR | **`T0` no rechaza fuera de reposo,** como sí haría `PassGate`. Una sonda así contamina la fila 4 | patch L830-844 | Rechazar con `T0.refused-not-quiescent` si `quiescent=False` o `bridgeCmdActive=True`, o declarar en la nota que esas sondas se descartan |
| H-10 | MENOR | **A la fila 9 le falta `released`,** y `held` mezcla el bloqueo con la transacción | patch L268-288, L9-18; L705-706 | Registrar `released` con `lockOthers` y `lockAll` tras cada `using` anidado. Documentar que el `top=OPEN` de `held` es la propia transacción |
| H-11 | MENOR | **Faltan variantes en el procedimiento:** la línea de base de Deshacer antes de cada sonda (filas 6 y 7); un Actualizar con el campo limpio, `NothingToApply` con bloqueo (fila 7, D-T4C-15); y sondas con y sin selección (fila 4) | nota §4; D-T4C-15; H-22 del paquete | Añadir esos pasos a §4 |
| H-12 | MENOR | **Datos de máquina que la sesión debe medir y no están en §4:** la ruta resuelta y el SHA-256 de la biblioteca (el override de `settings.json`; guía §7.1); la longitud del registro antes de arrancar; un segundo archivo si la corrida cruza las 00:00 UTC; la ordenación numérica de `#` (`D5` crece a 6 dígitos); los huecos de `#`, que revelan líneas perdidas, sin abrir el registro en exclusiva; y cómo se comprueba la ausencia del bundle: las carpetas `ApplicationPlugins` y los restos del Autoloader en el registro, en lectura | `BlockLibrary.cs:19-22`; `RackDiagnosticsLog.cs:47-51`; patch L626-635 | Añadirlo a la preparación de la sesión |
| H-13 | MENOR | **La privacidad,** como lista concreta (§6). La nota solo dice «revisarlo», y ella misma lleva rutas con el nombre corto del usuario | nota §5; patch L644-645 | Aplicar las 5 reglas del §6 a la traza, a las observaciones y a la nota |
| H-14 | MENOR | **El orden:** D-T4C-16 pide revisar «antes de construir», y el DLL se construyó antes. Con las correcciones habrá un build nuevo, y los SHA-256 de la nota §6 quedan obsoletos | D-T4C-16; nota §6 | Tras corregir: un build nuevo, el SHA-256 medido por la sesión, la misma comprobación de Release y una revisión focalizada del delta. La custodia registra el orden |
| H-15 | MENOR | **`H0Event` va fuera de `ContainSession`,** y `harness.attach` solo mide el SHA-256 del Plugin. Con un juego de DLL mezclado (un Application de Release), un `TypeLoadException` de JIT escaparía hacia AutoCAD desde un manejador | patch L311-423, L793-814 | `harness.attach` registra la ubicación y el SHA-256 de los cuatro ensamblados RackCad cargados. Opcional: envolver la llamada a `H0Event` en un `try` en el propio manejador |
| H-16 | MENOR | **`CompositionTarget.Rendering` fuerza el repintado continuo** hasta 30 s por sonda. Cambia la cadencia del bucle de mensajes (Idle y el despachador) y puede desplazar `drain` y `panel.notice`. El orden de los eventos de AutoCAD no cambia | patch L1129-1158 | Declararlo en la nota §8, con la lectura de las filas 1 y 5. Opcional: una repetición del Actualizar sin fotogramas, para comparar |

**Recuento:** 0 BLOQUEANTES, 5 IMPORTANTES y 11 MENORES.

---

## 8. Veredicto

**APTO TRAS CORRECCIONES.**

**Lo que está bien:**
- el arnés no puede dañar el original ni escribir fuera del dibujo abierto y de `RackLog`;
- Release es idéntico a la base;
- la lógica de T4B no cambia;
- cada fila de la tabla tiene su traza;
- el tratamiento de `ExecutionResult` es correcto.

**Antes de operar:**
- **H-01, H-02 y H-03,** en el código del arnés, sin tocar T4B;
- **H-04,** en el arnés y en la preparación de la sesión;
- **H-05,** en el procedimiento.

**Muy recomendados, porque deciden la lectura de las filas 2, 7 y 11:** H-06, H-07 y H-11.

**Después de las correcciones de código:**
- un build nuevo, con su SHA-256 medido por la sesión;
- la misma comprobación de Release;
- una revisión focalizada solo del delta (H-14).
