# I-64 F4-T4C — Comprobación dirigida del paquete v5, tras la sonda de host T4C-H0

**Veredicto:** EMITIBLE. **Recuento:** 0 bloqueantes, 0 importantes y 4 menores (§7 y §8). La emisión sigue pendiente de la ratificación de O-17 por el Owner y de su orden de emisión (§9 del paquete).

## 1. Objeto, método y límites

**Objeto:**
- el paquete v5;
- su diferencia con `_package-v4.md`;
- frente a:
  - el resultado de H0 (`h0/h0-result.md`, citado como H0R);
  - la traza redactada (`h0/custody/h0-trace.txt`, citada como H0T);
  - las notas del arnés (`h0/custody/h0-harness-notes.md`);
  - la decisión del Owner sobre el C12 (`owner-f4t4c-h0-c12-decision.md`);
  - la respuesta de I-52 (`h0/i52-reply.md`).

**Base:** el worktree está en `44fe029d`, y `git diff b15072ff 44fe029d -- src tests` sale vacío.

**Método:**
- **la custodia:** la traza y el resultado de la carpeta de trabajo son idénticos, byte a byte, a los de `44fe029d:docs/automation/evidence/I-64-pilot/F4-T4C-H0/`;
- **la secuencia de eventos de las cinco sondas** se extrajo de la traza con `grep`;
- **el código:** el manejador de `CommandWillStart` del puente (`src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs:191-197`) y las guardas que lo fijan.

No se compiló, no se ejecutó ninguna prueba y no se escribió nada en el worktree.

**Notación:** `P5:n` es la línea n del paquete v5, que está fuera del repositorio.

**Lo que no se pudo verificar:**
- las observaciones de Undo y Redo de las filas 6 y 7, que vienen de la operación y no de la traza;
- los hechos de IL de H-50, más allá de su texto en las notas del arnés (§1, `:25-36`).

---

## 2. Los hechos H-39 a H-52, frente a la traza y el resultado

**La secuencia es idéntica en las cinco sondas** (P1 `:32-46`, P2 `:60-75`, P3 `:88-111`, P4 `:127-823` y P5 `:1061-1757`):
1. `call.returned`, con `IsCompleted=False`;
2. dos `EnteringQuiescentState`;
3. `CommandWillStart` con `cmd=EXECUTEFUNCTION`;
4. `callback.enter`, con `callReturnedBefore=True`;
5. `callback.exit`;
6. `CommandEnded(EXECUTEFUNCTION)`;
7. `EnteringQuiescentState`;
8. `continuation.raw`;
9. `continuation.ran`.

**No aparece ningún `CommandCancelled` ni `CommandFailed`,** y ningún `CommandWillStart` ajeno entre la vuelta de la llamada y el callback.

**Los únicos comandos ajenos de la traza** son RACKPANEL, U, REDO y CLOSE, todos fuera del vuelo (`:16`, `:831`, `:1043`, `:1765`, `:1974`, `:2186`). Las lecturas del panel no dan ningún `CommandWillStart`.

| Hecho | Comprobación |
|---|---|
| H-39, diferido | Sí. `:127`, «DEFERRED (the call returned before the callback ran)», y `:134`, `callReturnedBefore=True`. Los retrasos de 49,6 a 145,8 ms coinciden con `h0-session.json`. Las dos anclas no son las de los fotogramas, que están en `ui.frame.first` (`:34`, `:129`), pero el hecho se sostiene |
| H-40, compleción | Sí. `continuation.raw` en las cinco sondas, y `:823`: `IsCompleted=True`, `getResult=returned(no-op)`, `thread=1`, `onPanelThread=True`, `syncCtx=…DispatcherSynchronizationContext` |
| H-41, dos reposos previos | Sí. `:35-36`, con `cmdActiveInActiveSession=False`; `:37` es el primer `CommandWillStart`, y es el de `EXECUTEFUNCTION` |
| H-42, sin pistas de los `*Cancel*` | Sí, porque no hay ningún evento de comando antes de `:37` |
| H-43, el orden | Sí: `:37`, `:39-42` y, en P4, `:817-819`. `CommandEnded` va después de `callback.exit` |
| H-46, la entrada en el callback | Sí. `:134`, con `activeIsAdmitted=True`, `top=null`, `cmd='EXECUTEFUNCTION'`, `quiescent=False` y `lockAll=Write` |
| H-48, sin bombeo | Sí: `:826-827`, «AFTER the callback exited» |
| H-49, la aserción | Sí: `:829`, `top=null` |
| H-50, `ExecutionResult` | Coincide con las notas del arnés: `:25`, `:31-36`. Incluye el rechazo nativo que no lanza ni se completa, el trampolín sin región de excepción, el `Post` al contexto de dentro del comando y la ranura única. En `:39` se ve que el contexto dentro del callback es `Autodesk.AutoCAD.Runtime.SynchronizationContext` |
| H-44, H-45 y H-51 | Coinciden con H0R `:40-41` y `:50-53` |
| H-52, las desviaciones | Coinciden con H0R `:56-60` y con la respuesta de I-52. `h0-session.json` aún dice «no reply» para I-52, y queda superado por la respuesta (M5-02) |

**Conclusión:** los hechos que pasan a [M] coinciden con la traza y con el resultado.

---

## 3. D-T4C-02 con la regla nueva del Owner

**La regla, tal como se aplica:**
- **la liberación normal** es `Completed()`, desde la continuación de `OnCompleted`;
- **la del respaldo,** con el callback sin empezar:
  - el primer `CommandWillStart` de la sesión de un comando que no sea `EXECUTEFUNCTION`;
  - el cierre del documento o la retirada de la sesión, por `SessionDestroyed`;
  - la degradación;
- **el reposo** deja de contar.

Es la letra de la decisión del Owner.

**Caminos sin liberar:** ninguno.
- **El rechazo nativo,** que no lanza ni se completa, lo cubre el primer comando ajeno, el cierre o la retirada. Cualquier comando del usuario lo es.
- **`RACKPANELDESCARTAR` y `CLOSE`** son comandos ajenos. Su `CommandWillStart` anota la cuenta antes de que el comando llame a `DiscardDocument`, o antes de `BeginDocumentClose`. Así que ni el descarte ni el cierre quedan bloqueados más allá del comando con que se piden (P5, D-T4C-10). Se comprobó en el código que `NoteForeignCommandStart` va después de `OnSourceEvent` y de la decisión de `RaiseCommandStateChanged`, dentro de `ContainSession`. La caducidad perezosa lo ve en la siguiente comprobación: la del propio `DiscardDocument`.
- **Un billete empezado** tiene su llamada terminal: operaciones totales y el `finally` exterior. Si faltara, la defensa de N-01 lo cubre con el mismo disparador. **Un billete terminado sin compleción** se libera con el primer comando ajeno. **La seguridad de N-01 se mantiene.**

**Caducidades espurias:**
- **Un `CommandWillStart` ajeno entre la vuelta y el callback** es posible en teoría, en una ventana de 50 a 150 ms (H-39). Por ejemplo, un comando lanzado por un reactor de otro complemento o por la cinta.
- **H0 no lo vio:** la tecla va a la paleta, y los `*Cancel*` no dan `CommandWillStart`.
- **Si ocurre, es seguro:** `Admit` ve el billete caducado, no hay ninguna escritura y se presenta «no llegó a ejecutarse».
- **Si fuera sistemático, es C12,** y el paquete lo dice (C12, «un `CommandWillStart` ajeno sistemático antes del callback»).

**El callback tardío tras la caducidad:**
- el testigo viejo hace que `Admit` dé falso, que `NotRun` se ignore y que `Completed()` no haga nada;
- el `EXECUTEFUNCTION` vacío deja su paso de Undo (H-45, D-T4C-15);
- la prueba R-01 de D-T4C-17 lo cubre.

**La doble presentación:** no la hay.
- Un billete no terminado que caduca guarda `Failed(HostNotRunReason)` una sola vez, y un billete terminado solo se libera.
- La compleción no guarda nada si el billete está terminado.
- El canal único del fallo síncrono se mantiene.

**Las pruebas y las guardas** siguen siendo coherentes:
- la regresión de C12: las dos entradas en reposo y el `CommandWillStart` de `EXECUTEFUNCTION` no caducan el billete;
- la compleción libera una vez;
- el comando ajeno, en el puente, con su condición y sus negativas;
- N-01, con el comando ajeno;
- las filas 5, 14, 15, 17 y 22 de D-T4C-20.

**La regla de olvido pasa a 16.** `NoteForeignCommandStart` olvida `_lastAllowedClose`, como ya hace `OnSourceEvent` en el mismo manejador (`EBC:309`), así que no cambia la semántica de la secuencia de cierre.

**O-07 sigue siendo segura.**

**Un matiz:** la regla del Owner dice «si el callback no empezó». La v5 extiende el disparador a un billete empezado que no entró en `Execute` y a uno terminado sin compleción (P5:482). Es la defensa de N-01, que ya se aceptó, y no caduca nada que la regla quiera conservar: un billete en `Execute` nunca caduca. Pero va más allá de la letra (M5-01).

---

## 4. D-T4C-04 con `ExecutionResult`

**La guarda reescrita es satisfacible:**
- exactamente un `SynchronizationContext.Current` antes de la llamada;
- un `.OnCompleted(` en la rama de `IsCompleted` falso;
- un `.Post(` en la única ayuda del salto;
- como mucho un `IsCompleted` y como mucho un `GetResult()`, este dentro de un `try`;
- las prohibiciones del precedente, más `TaskScheduler`.

El adaptador devuelve `Task.CompletedTask` en su lambda, y ninguna prohibición lo alcanza. `\.Result\b` no coincide con `ExecutionResult`.

**Es discriminante:** las negativas son una segunda `OnCompleted`, un `GetResult()` fuera de un `try`, un `Task.Factory.StartNew`, una `ContinueWith`, una continuación sin `try` y un `throw` en un `catch`.

**La continuación sigue siendo total.** Los tres puntos por los que una excepción podría salir hacia AutoCAD van envueltos en un `try` que registra y no relanza:
- la lambda del callback, que el trampolín invoca sin región de excepción (notas `:32`);
- la continuación cruda, que AutoCAD invoca dentro de un `catch` con diálogo;
- la acción publicada.

El corredor también es total. Si el `Post` falla, el billete lo libera la caducidad. **La ranura única de `OnCompleted`** queda respetada: se llama una sola vez.

---

## 5. D-T4C-09, D-T4C-15 y Smoke-2

**D-T4C-09 es coherente con el orden medido:**
- `CommandWillStart(EXECUTEFUNCTION)` llega antes del callback y de `Begin`;
- todos los `Object*` van dentro del callback;
- `CommandEnded(EXECUTEFUNCTION)` llega después de la relectura de POST y cierra la ventana como propio;
- `EnteringQuiescentState` y la compleción vienen después.

N-07, N-10 y los `*Cancel*` no se activan. **El residual queda dentro del comando interno.**

**D-T4C-15 es coherente con un solo paso de Undo,** y no afirma el cumplimiento antes de Smoke-2. Un matiz sobre el paso vacío está en M5-03.

**§7, Smoke-2, recoge:**
- el paso vacío;
- Undo y Redo, con Redo inmediatamente después de Undo;
- el comportamiento de las lecturas del panel sobre la pila de Rehacer, que se declara;
- el nombre en la lista;
- la cota nueva;
- el diferido y «Aplicando».

---

## 6. Contradicciones con la v4 o con las decisiones del Owner, y condiciones de devolución

- **No se encontró ninguna contradicción** con O-01 a O-16.
- **O-16** aplica la decisión del Owner, con el matiz de M5-01.
- **O-17,** la ratificación de las desviaciones de H0, es la única decisión abierta. Es de procedimiento:
  - el operador fue la sesión, por instrucción del Owner;
  - el dibujo es sintético;
  - el RELEASE de I-52 llegó tarde, y con la política de no emitirlo.
- **Las condiciones de «RETURN BEFORE EMISSION» quedan sin activar,** ahora por la medida:
  - el envoltorio (filas 2, 5 y 8 a 10);
  - Undo (fila 6);
  - una transacción authored (O-12).

  La única que se activó, el C12 de la fila 3, está resuelta por el Owner.

---

## 7. Los hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| M5-01 | MENOR | **La regla aplicada va más allá de la letra del Owner:** extiende el respaldo del comando ajeno a un billete empezado que no entró en `Execute` y a uno terminado sin compleción. Es la defensa de N-01 y no contradice la regla, pero no está en el texto de la decisión | `owner-f4t4c-h0-c12-decision.md`; P5:480-482 | Declararlo como interpretación en O-16, o ponerlo con O-17 para la ratificación del Owner |
| M5-02 | MENOR | **La custodia está incompleta:** la decisión del Owner sobre el C12 está «todavía sin custodiar», y la respuesta de I-52 se cita como `F4-DRAFTS/t4c/h0/i52-reply.md` sin estar en el repositorio. `h0-session.json` dice «no reply». Las precondiciones de emisión no lo recogen, ni tampoco esta comprobación | P5:92, P5:383, P5:1664-1667; `44fe029d` (12 archivos, sin esos dos) | Añadir a las precondiciones la custodia de la decisión, de la respuesta de I-52, con una nota que supere el «no reply», y de esta comprobación |
| M5-03 | MENOR | **El paso vacío incluye «un Conflict»,** pero un Conflict sale en MUTATE, después de la importación de PREPARE. Su paso de Undo lleva el residuo OM-5 y no está vacío. Solo `NothingToApply`, los rechazos anteriores a la importación y un billete caducado dejan un paso vacío | P5:1146; `RUN` (PREPARE → `PrepareUnits` → MUTATE) | Separar el paso vacío del paso con residuo OM-5, que ya figura en «lo que solo prueba AutoCAD» |
| M5-04 | MENOR | **O-H0-2 no llega al Owner como observación:** las lecturas del panel de F4-T3 y F4-T4A («Editar» y «Comprobar ahora») dejan entradas «Group of commands» en Deshacer y vacían Rehacer. Es un efecto visible de unas lecturas que se suponían sin efecto, y toca S2-05 y la obligación de IC-02 de observar Deshacer y Rehacer. El paquete lo declara solo para Smoke-2 | P5:1147; H0R `:51`; `docs/initiatives/I-64-implementation-clarifications.md:55` | Llevarlo al §9 como observación para el Owner, por ejemplo como candidato a ideas futuras o a una corrección aparte de T3 y T4A, sin cambiar el alcance de T4C |

---

## 8. Veredicto

**EMITIBLE. B/I/M = 0/0/4.**

**Lo que queda bien:**
- los hechos de H0 coinciden con la traza;
- la regla «Completar o comando ajeno» no deja caminos sin liberar ni presentaciones dobles;
- la guarda del adaptador para `ExecutionResult` es satisfacible, discriminante y total;
- D-T4C-09 y D-T4C-15 son coherentes con el orden medido y con un solo paso de Undo.

**Los cuatro menores** se pueden corregir sin otra comprobación.

**Lo que tiene que volver al Owner:**
- la ratificación de O-17, con M5-01 si se elige ese camino;
- la observación de M5-04;
- y después, la orden de emisión de T4C1.
