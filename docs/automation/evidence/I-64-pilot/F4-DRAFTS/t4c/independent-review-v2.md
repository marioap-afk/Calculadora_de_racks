# I-64 F4-T4C — Re-revisión acotada del paquete v2 (antes de emitir)

**Veredicto:** EMITIBLE TRAS CORRECCIONES. **Recuento de los hallazgos nuevos:** 0 bloqueantes, 7 importantes y 8 menores (§4 y §5).

## 1. Objeto, método y límites

**Objeto:** el paquete v2 de F4-T4C, frente a los 28 hallazgos de la revisión de la v1 (`independent-review-v1.md`, que aquí se cita como IR1).

**Base:** `b15072ff`, con el producto de `4314c180`.

**Método:**
- se leyó entero el paquete v2;
- todo lo que funda los diseños rehechos se comprobó en el código con lecturas y con `git grep`, `git show` y `git ls-files`. No se compiló, no se ejecutó ninguna prueba y no se escribió nada en el worktree;
- se ejecutó en solo lectura el script del censo de G-9 que cita el paquete, y su salida coincide con H-30;
- se midió aparte el censo de la lista de G-5 en las dos carpetas Workspace.

**Notación:** `P2:n` es la línea n del paquete v2, que está fuera del repositorio. El resto de las rutas son relativas al repositorio.

**Lo que no se pudo verificar:**
- todo lo marcado [H], que solo mide AutoCAD;
- que los cambios compilen;
- el detalle de todas las guardas que miran los cuerpos de la reconciliación (`TryKeepPilotEdit`, `TryDiscardAndReloadPilotEdit`) frente a la nueva comprobación del vuelo. Se revisaron `ReadGateViolations`, `HostViolations`, `PortMutatingOperations` y las cuentas por operación.

---

## 2. El cierre de los 28 hallazgos de IR1

| Id | Estado | Comprobación |
|---|---|---|
| R-01 | **Resuelto en el caso señalado** | El billete caduca con la primera entrada en reposo si el callback no empezó, con un testigo y la prueba del modo «nunca» (P2:389-398, P2:1123). **El matiz es legítimo:** el siguiente drenaje puede no llegar sin pistas. Quedan dos huecos nuevos: el billete ya empezado (N-01) y la sesión degradada (N-09) |
| R-02 | Resuelto | El cerrojo terminal, el canal único del fallo síncrono, la regla de `InFlight` falso con `Unavailable` y el consumo por superficie, con pruebas HR, RQ y UI (P2:370-374, P2:440-448, P2:485-500, P2:1124-1126) |
| R-03 | **Resuelto en lo esencial** | `Busy` o `Waiting` durante el vuelo en las cuatro operaciones que cambian el borrador, con prueba RQ (P2:685-694). La afirmación «el resto no puede cambiar un Dirty» es falsa para `TryConfirmBase` (N-08) |
| R-04 | Resuelto | Hay un objetivo opaco, `IPushBackPilotHostTarget`, y Application no ve el `Document` (P2:475-481, P2:511-516). Queda un matiz de redacción en INV-19 (N-11) |
| R-05 | Resuelto | El primer `CommandWillStart` cierra la ventana, aunque el reposo no cambie, porque el cierre va antes de la comparación de `EBC:439` (P2:722-726). **El matiz es legítimo:** respeta los literales fijados en `TW/WorkspaceHostC2SourceGuardTests.cs:208-209` y la regla del cuerpo de `:206-210`. Deja dos casos de mapeo: N-07 y N-10 |
| R-06 | Resuelto | H0 lo mide en la fila 4, que activa C12, y O-02 recoge V1, V2 o una A-n (P2:747-753, P2:1046-1047). El argumento de V2 se sostiene |
| R-07 | Resuelto | Las cuatro partes. (a) La aserción va aparte (P2:1233). (b) El cierre va dentro de `SetQuiescent`. (c) La regla de olvido pasa a 15, en las dos cuentas de `TW/WorkspaceLifecycleGuardTests.cs:113` y `:167`. (d) Los reenviadores son `internal`. Hay inventarios nuevos incompletos, en N-02, N-03, N-05 y N-06 |
| R-08 | Resuelto | El planificador se captura antes de la llamada, con la `ContinueWith` sobre él y las prohibiciones del precedente (P2:507-509, P2:533-548). **El matiz es legítimo,** y mejora la propuesta |
| R-09 | Resuelto | La frontera sigue H-27, con prueba UI (P2:808-814, P2:1128). **El matiz de `Kept` es legítimo:** en una frontera bloqueante solo sale si no se puede editar (`IWorkspacePilotEditPort.cs:154-159`), y la comprobación previa no aplicaría |
| R-10 | Resuelto | Las tres dispensas, el arnés revisado en un worktree desechable, la custodia, el operador, el dibujo en copia y la tabla de resultados (P2:1005-1065). Falta una cuarta cláusula de §12.1 (N-12) |
| R-11 | Resuelto | El censo por archivo y la guarda con su cuenta (P2:146-151, P2:417-418, INV-03). Hay un ancla imprecisa (N-14) |
| R-12 | **Parcial** | G-9, G-10 y las cuentas de G-6 están bien pensados, pero **G-6 no se puede satisfacer** tal como está escrito (N-02) |
| M-01 | Resuelto | `MarkDraftsPossiblyStale` solo con `DatabaseObjectChanged` o `CommandEnded` ajenos, con prueba OM (P2:714, P2:1114) |
| M-02 | Resuelto | `\.EnterError\s*\(` en Application y en Plugin no coincide con la definición, que no lleva punto, ni con el método privado de la UI (P2:883) |
| M-03 | Resuelto | `NotApplying` da `ClosedInError` con su clasificación, y nunca un éxito (P2:568, P2:882). Queda un matiz de documentación (N-13) |
| M-04 | Resuelto | Las dos anclas están corregidas (H-09, H-33) |
| M-05 | Resuelto | **El matiz es legítimo:** R-T4-13 obliga solo «donde se toquen esas superficies» (P2:886-893) |
| M-06 | Resuelto | F28 está prohibida de forma expresa, las 65 cuadran y se enumeran los cinco archivos de UI, que se comprobaron con `git ls-files` (P2:1355-1371) |
| M-07 | Resuelto | `BVM` sale del permitido (P2:1322) |
| M-08 | Resuelto | La prueba 12 está reformulada (P2:1109) |
| M-09 | Resuelto | El orden de 3 (b) y el caso 3 (c) del id reciclado (P2:1100) |
| M-10 | **Resuelto en el corredor** | `OnCallback` es total (P2:500, P2:1129). El núcleo no lo es: N-01 |
| M-11 | Resuelto | El gesto comprueba su botón, y el paso de Undo vacío va a H0 y a Smoke-2 (P2:807, P2:983, P2:1053) |
| M-12 | Resuelto | P2:400-403 |
| M-13 | Resuelto | **El matiz es legítimo:** los tres textos de I-01 se quedan en `RUN` por `T4BC` §5, y los nuevos van a `HOSTRUN`. Se añade C14 (P2:577-581, P2:1481-1484) |
| M-14 | **Resuelto, pero introduce N-03** | G-3 amplía la lista de tipos y la reflexión sobre los anidados (P2:909-914) |
| M-15 | **Resuelto, con una afirmación falsa** | T4C no añade lecturas, pero la guarda que el paquete cita como «que no cambia» sí tiene que cambiar (N-05) |
| M-16 | Resuelto | P2:1413-1416 |

**Recuento:**
- **resueltos:** 26, de ellos 5 solo en lo esencial o con un hallazgo nuevo derivado (R-01, R-03, M-10, M-14 y M-15);
- **parcial:** 1, R-12;
- **sin resolver:** 0.

Los seis matices son legítimos.

---

## 3. Los tres diseños rehechos

### 3.1 D-T4C-02: el billete en vuelo, su cota y el cerrojo terminal

**Lo que se sostiene:**
- el testigo anula un callback tardío;
- **la caducidad** guarda `Failed(HostNotRunReason)` una sola vez, y una corrida que no empezó nunca escribe;
- **la lectura de la cuenta** es en memoria y a coste constante (`EBC:437-442`);
- **el canal único:** un fallo síncrono solo da el valor de inicio;
- **el cerrojo:** una llamada que lanza después de un callback síncrono ya no se convierte en `Failed`.

**Los defectos nuevos:**
- **Un billete empezado no tiene liberación garantizada** (N-01).
  - La caducidad excluye por diseño un billete cuyo callback ya empezó (P2:390). `Admit` lo marca como empezado (P2:471).
  - Después, la única liberación es el `finally` de `Execute` o `NotRun` (P2:380-385).
  - Si `NotRun` del núcleo lanza, o si una excepción del `finally` del corredor impide llamarlo, el `catch` total del corredor se la traga (P2:500), y el billete queda puesto para siempre. Es el caso de `Retire` con un escritor nulo después de que `CreateWriter` falle.
  - La prueba de M-10 usa un registrador doble (P2:1086, P2:1129), así que no lo ve.
  - Vuelve la trampa de R-01: `Busy` en los descartes y el veto de cierre.
- **La caducidad cierra el ámbito con `CloseOwnMutation`** (P2:394, P2:727). Pero G-10 solo admite ese llamador en la clase de corrida y en los reenvíos (P2:953), y la fila 5 dice que `ExpireStaleUpdate` solo nombra `QuiescentEntriesOf` (P2:1230).
  - Las tres reglas se contradicen.
  - Además, `TakeUpdateProgress`, una operación «de memoria» que llama la presentación dentro de los avisos, terminaría mutando el puente y olvidando `_lastAllowedClose`.
  - El cierre es inalcanzable, porque `Begin` va después de `Admit` (N-04).
- **La cuenta de entradas en reposo** no avanza con la activación: `OnDocumentActivated` fija `Quiescent` directamente (`EBC:223-241`). Tampoco avanza en una sesión degradada, porque `OnSourceEvent` vuelve antes (`EBC:312`). P2:397 promete «o la siguiente activación» (N-09).
- **Una nueva aceptación del mismo rack** caduca el billete viejo, guarda `Failed` y lo vacía en el acto, porque «una nueva aceptación del mismo rack» limpia el almacén (P2:374). Ese resultado no se presenta nunca (N-15).

### 3.2 D-T4C-04: el objetivo opaco y las primitivas

**Lo que se sostiene:**
- el `Document` queda en `Target`;
- la clase `AutoCadProjectionWriteScopeFactory` (`src/RackCad.Plugin/Views/RackProjectionWriteScope.cs:17-21`) sirve de precedente para el campo único;
- `TaskScheduler.FromCurrentSynchronizationContext()` lanza sin contexto, de modo que no se pide nada;
- la continuación vuelve al hilo de la UI;
- `OnCallback` es total y respeta el cerrojo;
- el orden objetivo → `Admit` → bloqueo → ámbito → escritor → `Execute` es correcto, y para el id reciclado.

**Los defectos nuevos:**
- **G-3, por reflexión sobre los tipos anidados de `HOSTRUN`, prohíbe los campos de tipo `IPushBackPilotHostRun`** (P2:911-913). El corredor tiene que retener `run` entre `Request` y el callback diferido:
  - si va en un campo del corredor, lo viola;
  - si va en una clausura, el compilador genera un tipo anidado con ese campo, y la reflexión «con sus tipos anidados» también lo ve;
  - la guarda no se puede satisfacer (N-03).
- **`Lock()` devuelve el `DocumentLock` al corredor como `IDisposable`.** INV-19 dice «ningún objeto de AutoCAD cruza» (P2:1450). La orden solo nombra `DBObject`, `Transaction` y `Document`, así que el fondo vale, pero la redacción sobra (N-11).

### 3.3 D-T4C-09: el cierre del ámbito propio y el residual

**Lo que se sostiene:**
- el cierre en `SetQuiescent` antes de la comparación;
- los literales de caso intactos;
- `SessionState` sin `Close`, `Allowed` ni `SessionId?`;
- la regla de olvido a 15 en sus dos cuentas;
- los reenviadores `internal`, que no tocan la superficie pública de nueve nombres;
- un comando del usuario ya no se registra como propio;
- `ImpliedSelectionChanged` no marca.

**Los defectos nuevos:**
- **«Un marcado de más, nunca un enmascaramiento»** (P2:731) y la fila 5 de H0, «Con un comando anidado … se declara el marcado de más de POST» (P2:1050), aceptan como residual algo que viola el oráculo de IC-08, INV-28 y S2-14:
  - un cierre temprano **sistemático** marca B después de cada Actualizar. Puede venir de un comando anidado o de un `CommandWillStart` del propio `EXECUTEFUNCTION` que llegue después de `Begin`;
  - es el mismo defecto que R-06 en la v1 (N-07).
- **Si faltan a la vez el `CommandEnded` y el `EnteringQuiescentState` del comando interno,** solo cierra el siguiente `CommandWillStart`. Una escritura sin comando sobre otro rack antes de ese comando (por ejemplo, desde la paleta de Propiedades) queda enmascarada. La fila 5 lo da por cubierto (P2:1049) (N-10).

---

## 4. D-T4C-20: ¿está completo el inventario?

**No.**
- **G-6 no se puede satisfacer** (N-02). La regla lleva la lista de G-5, que incluye `CommandMethod` (P2:923, P2:926), y `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:22` y `:38` tienen dos `[CommandMethod(`. El archivo no está entre las excepciones, que se midieron:
  - `WorkspaceNavigationEffects.cs`: 2 y 2;
  - `WorkspaceNavigationFacts.cs`: 1 y 1;
  - `WorkspaceNavigationHost.cs`: 2;
  - `WorkspaceSelectionReader.cs`: 1 y 1.

  Esas cuatro coinciden con P2:927-930, pero falta la quinta.
- **Falta un cambio:** D-T4C-12 pone la expresión del diagnóstico en el cuerpo de `TryCheckPilotEdit`. Eso rompe `TW/WorkspaceEditIntentGuardTests.cs:699-700`, que exige cero en `CheckBody()`. Y P2:861 dice justo que esa prueba, `:688-700`, «no cambia». La fila 12 solo cubre `:682-685` y `:821-870` (N-05).
- **El prohibido contradice el permitido** (N-06). P2:1381 pone en el prohibido «las guardas que no cambian … cada una por su archivo exacto». Tres de esos archivos también están en el permitido, porque cambian por otras filas:
  - `TW/WorkspaceLifecycleGuardTests.cs` (filas 8 y 15);
  - `TW/WorkspaceBridgeFacadeSourceGuardTests.cs` (fila 14);
  - `TW/WorkspacePilotUpdateGuardTests.cs` (fila 13).
- **G-3 y la caducidad** (N-03, N-04): §3.
- **Las negativas:** todas las filas declaradas la tienen, salvo la 20, la de los dobles, donde no aplica. Las negativas de G-6 y G-3 deben ajustarse a sus correcciones.

---

## 5. Los hallazgos nuevos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| N-01 | IMPORTANTE | **Un billete empezado no tiene liberación garantizada.** La caducidad excluye los empezados, y si `NotRun` del núcleo lanza, o el `finally` del corredor salta su llamada, el `catch` total se traga la excepción y el billete queda puesto. Es la trampa de R-01 por otro camino | P2:380-390, P2:471, P2:496-500, P2:1086, P2:1129 | (1) Las cuatro operaciones de la corrida del núcleo son totales y quitan el billete en su `finally`. (2) El corredor garantiza, con un `finally` exterior propio, exactamente una llamada a `Execute` o a `NotRun` después de un `Admit` verdadero. (3) Una prueba RQ con el núcleo real: `Lock`, `CreateWriter`, `Retire` y `Begin` lanzan, y el billete se quita siempre |
| N-02 | IMPORTANTE | **G-6 no se puede satisfacer:** `WorkspacePanelCommand.cs` tiene dos `[CommandMethod(`, que están en la lista de G-5, y falta entre las excepciones exactas | `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:22`, `:38`; P2:923-932 | Añadir la excepción exacta, `WorkspacePanelCommand.cs` con 2 `CommandMethod`, o quitar `CommandMethod` de G-6, ya que el censo de 40 lo cubre. Ajustar la negativa |
| N-03 | IMPORTANTE | **G-3, por reflexión sobre los tipos anidados de `HOSTRUN`, no se puede satisfacer:** el corredor diferido tiene que retener `run` en un campo o en una clausura generada por el compilador | P2:482, P2:485-499, P2:909-914 | Limitar G-3 a los tipos de larga vida: el núcleo, `CTL`, el ejecutor de `ADAPTER` y `Primitives`. El corredor por petición y los tipos generados por el compilador quedan exentos. Añadir la regla «el corredor solo se crea dentro de `TryRequest`, y ningún tipo de larga vida lo guarda» |
| N-04 | IMPORTANTE | **Contradicción en la caducidad:** D-T4C-02 y D-T4C-09 cierran el ámbito con `CloseOwnMutation`, y G-10 y la fila 5 lo prohíben. Además, la presentación mutaría el puente. Ese cierre es inalcanzable | P2:394, P2:727, P2:953, P2:1230 | Quitar el cierre de la caducidad y fijar con una guarda que `ExpireStaleUpdate` no nombra `CloseOwnMutation`. O declararlo en G-10 y en la fila 5, con su efecto sobre `_lastAllowedClose` |
| N-05 | IMPORTANTE | **Un cambio de guarda sin declarar en T4C2:** `TW/WorkspaceEditIntentGuardTests.cs:699-700` exige cero diagnósticos en `CheckBody()`, y el paquete afirma que esa prueba no cambia | P2:861, P2:1237 | Añadir `:699-700` a la fila 12, con su negativa, y corregir P2:861 |
| N-06 | IMPORTANTE | **El prohibido incluye archivos del permitido:** tres archivos de «las guardas que no cambian» se modifican por otras filas. Con el prohibido por encima, la tarea no podría hacer las filas 8 y 13 a 15 | P2:1248-1259, P2:1336-1340, P2:1381 | Al prohibido solo los archivos que no están en el permitido. En los compartidos, la protección por línea es la comparación de la entrega con D-T4C-20 y C07, y debe decirse así |
| N-07 | IMPORTANTE | **Un cierre temprano sistemático del ámbito se declara «marcado de más»:** marca B después de cada Actualizar, y eso viola el oráculo de IC-08, INV-28 y S2-14. Es el mismo defecto que R-06 | P2:731, P2:1050; `docs/initiatives/I-64-implementation-clarifications.md:151` | En la fila 5 de H0, un comando anidado o un `CommandWillStart` posterior a `Begin` antes del último `Object*` propio da C12, con una variante o una A-n en O-02. Solo un caso aislado se puede declarar |
| N-08 | MENOR | **`TryConfirmBase` puede pasar a Conflict un Dirty en vuelo** (`DS:743-760`). El paquete dice que el borrador no cambia. El desenlace es seguro, porque la comprobación previa da NotApplicable | P2:692, P2:1429 | Ponerlo en la lista `Waiting` de D-T4C-08, u ocultar «Comprobar ahora» con `InFlight` y corregir INV-08 |
| N-09 | MENOR | **La activación no cuenta como entrada en reposo,** y una sesión degradada no cuenta nunca. En esa sesión, el billete no tiene cota y el ámbito no se cierra por eventos | `EBC:223-241`, `:312`; P2:397 | Corregir la promesa de P2:397. Para una sesión degradada, `RequestUpdate` responde `Unavailable` o `Rejected` (`IsDegraded`, `EBC:144`), o se declara |
| N-10 | MENOR | **Sin `CommandEnded` ni `EnteringQuiescentState`,** una escritura sin comando sobre otro rack antes del siguiente comando queda enmascarada, y la fila 5 lo da por cubierto | P2:1049 | Que ese resultado de H0 dé C05 o C12, o declararlo en IC-09 |
| N-11 | MENOR | **INV-19 dice «ningún objeto de AutoCAD»,** y el `DocumentLock` cruza como `IDisposable` | P2:476, P2:1450 | Reducirlo a «ningún `DBObject`, `Transaction` ni `Document`, ni ningún tipo de AutoCAD en las firmas», o envolver el bloqueo en `ADAPTER` |
| N-12 | MENOR | **§12.1 carga «el DLL Debug del worktree de I-64»,** y H0 lo construye en un worktree desechable | `docs/initiatives/I-64-proposal-v5.md:830`; P2:1007, P2:1013 | Añadir esa cláusula a la dispensa 2 |
| N-13 | MENOR | **`ClosedInError` para `NotApplying`** contradice su documentación («the draft is now in Error»). El caso es inalcanzable | `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:419-420`; P2:568 | Declararlo, o ajustar la documentación del valor |
| N-14 | MENOR | **Ancla:** el `await` de `WorkspaceNavigationViewModel.cs` está en `:231`, fuera del intervalo `:194-206` | P2:149 | Citar `:194-231` |
| N-15 | MENOR | **Una nueva aceptación del mismo rack** caduca el billete viejo y vacía en el acto el `Failed` recién guardado, que no se presenta | P2:374, P2:391-395 | Devolverlo como valor de inicio, o no vaciarlo en esa aceptación |

---

## 6. El §9

- **Están las decisiones de IR1:**
  - O-01, con las tres dispensas;
  - O-02, con la cota, V1, V2 o una A-n;
  - O-07, con la cota;
  - los tres «resueltos sin fila»: R-03 por diseño, R-04 y M-13, que se aceptan.
- **Faltan:**
  - **en O-02 y C12:** la consecuencia de un cierre temprano sistemático (N-07);
  - **en O-01:** la cuarta cláusula de §12.1 (N-12).
- **Sobre la recomendación O-07 (a) «con la cota»:** sigue sin ser del todo segura mientras no se corrija N-01.

---

## 7. Veredicto

**EMITIBLE TRAS CORRECCIONES.** Los hallazgos nuevos son 0/7/8. De IR1: 26 resueltos, 1 parcial y 0 sin resolver.

**Los tres diseños rehechos se sostienen en su arquitectura.** Los defectos nuevos son locales:
- una liberación (N-01);
- tres guardas imposibles o contradictorias (N-02 a N-04);
- un inventario y un alcance incompletos (N-05, N-06);
- un mapeo de H0 (N-07).

**Ninguno exige otra re-revisión de arquitectura.** Basta con comprobar las siete correcciones importantes antes de volver al Owner.

**Lo que tiene que volver al Owner:** el §9 de la v2, con N-07 en O-02 y C12, y N-12 en O-01.
