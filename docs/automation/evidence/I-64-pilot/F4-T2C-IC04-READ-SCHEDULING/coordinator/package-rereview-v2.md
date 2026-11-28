# Re-revisión acotada: paquete del contrato de F4-T2C (v2)

**Objeto:** `I-64-f4-t2c-gate-contract-package.md` v2, con su tabla de disposición (§9).
**Repositorio:** worktree `architecture-workspace-persistente-rackcad`, HEAD `92ad237c`. Solo lectura: sin builds, pruebas ni AutoCAD.

## Veredicto

**EMITIBLE.**

Los 2 bloqueantes y los 7 importantes de la revisión de la v1 están cerrados, y las correcciones no introducen ningún defecto bloqueante ni importante.

Hay 7 hallazgos nuevos, todos MENORES. Recomiendo aplicar N-1, N-3 y N-4 al texto del contrato antes de emitirlo. Son cambios de una línea cada uno y no exigen otra revisión.

Recuento: **0 BLOQUEANTES, 0 IMPORTANTES, 7 MENORES.**

## Estado de los hallazgos de la v1

| Hallazgo | Estado | Comprobación |
|---|---|---|
| B-1 (`RackLog.Warning` frente a `NoHostFileLogsAMeasurementAsAWarning`) | **Cerrado** | D-T2C-05 no añade ninguna línea de registro de medidas. La comprobación Debug registra con `LogFault` y, a través de él, con `RackLog.Exception`. Eso está permitido en el puente: `TheNewPluginFilesLogOnlyFaultsAndOnlyInsideAFaultMethod` excluye `BridgePath` (`WorkspaceHostSourceGuardTests.cs:497`), y ninguna guarda cuenta los `LogFault(` del puente. Solo C2 exige uno concreto en `RaiseCommandStateChanged`, y ese no cambia |
| B-2 (el contador hace saltar siempre el `Debug.Assert`, que con `FailFast` termina AutoCAD) | **Cerrado** | No hay contador ni `Debug.Assert`, y G-7 prohíbe `Debug.Assert` y `Debug.Fail` en el puente. El §6 avisa de las aserciones que ya existen. Queda un matiz en N-5 |
| I-1 (bucles modales y secuencia de cierre) | **Cerrado** | (a) `Run` desmarca, vuelve a evaluar `canRunNow()` y solo entonces llama a la pasada (D-T2C-02, G-4). Una continuación no elegible ya no llama a `_core.OnIdle()` ni olvida la secuencia. (b) Hay condición de cierre pendiente (D-T2C-03). (c) Los riesgos pasan a Smoke-2 y (d) el arrastre a T3 está registrado (D-T2C-08). La prueba «Allow → continuación → destrucción → `closeSequenceIntact` true» discrimina: con el diseño de la v1, fallaría. Queda un borde: N-1 |
| I-2 (punto 13 sin comprobación tras el resumen) | **Cerrado** | La comprobación Debug va después de `OnIdle()`, en la continuación, y lee `MdiActiveDocument?.TransactionManager.TopTransaction`. No tiene `.Database`, así que respeta `WorkspaceBridgeFacadeSourceGuardTests.cs:127-128`. `TopTransaction` no casa con `StartTransaction`, que es lo que prohíben `TheBridgeReadsAndWritesNothingOfTheDrawing` (`:124-126`) y `WriteApis` (`WorkspaceLifecycleGuardTests.cs:526-528`). Las variables locales no las ven las guardas de campos (`WorkspaceBridgeSourceGuardTests.cs:205-214`; `WorkspaceHostSourceGuardTests.cs:460`). G-9 fija presencia, no orden |
| I-3 (J contra el host vigente) | **Cerrado** | J afirma `EventBridgeCore.IsDegraded == false` y, en el host, que la sesión queda fallida, como hoy (H-04). La variante del resumen se contiene como `Failed` |
| I-4 (G-1 como lista blanca) | **Cerrado** | G-1 es negativa y no fija qué más puede hacer un manejador. Cumple la regla I-1 |
| I-5 (cableado sin guarda) | **Cerrado** | G-10 fija la construcción única, `_core.CanDrainNow`, la continuación con `OnIdle` y `SetPost` con `BeginInvoke(Background)`. Hay un matiz de precisión en N-3 |
| I-6 (huecos de la excepción de `Dispatcher`) | **Cerrado** | La expresión del puente mantiene prohibidos `\w*Timer\b`, `DispatcherFrame`, `PushFrame`, `\bYield\b`, `InvokeAsync`, `(?:_dispatcher\|CurrentDispatcher)\s*\.\s*Invoke\b` y `System\.Windows(?!\.Threading\b)`. Comprobé que no da falsos positivos sobre el puente previsto: `_dispatcher.BeginInvoke`, `Dispatcher.CurrentDispatcher`, `DispatcherPriority`, `Drained?.Invoke(` y `using System.Windows.Threading;` pasan. G-3 la fija literalmente y la prueba por copia |
| I-7 (smoke mal calibrado, K = 1) | **Cerrado** | Los disparadores ya materializan de verdad: otro rack seleccionado, cambio de documento. R-T2C-01 va al Owner y la temporización en el host queda como limitación para Smoke-2. Un matiz en N-6 |

Los menores M-1 a M-10 de la v1 están aplicados como dice el §9. M-4 queda sin objeto.

## Comprobaciones pedidas

### 1. Condición de cierre pendiente de `CanDrainNow` frente a las guardas de F4-T2B2B

- **Leer `_lastAllowedClose == _active` no rompe ninguna guarda existente:**
  - `\b_lastAllowedClose\s*=\s*(?!null\b|session\b)\w` (`WorkspaceLifecycleGuardTests.cs:97`) no casa con `==` ni con `!=`: tras el primer `=` viene otro `=`, que no es `\w`;
  - los recuentos de 13 olvidos (`:99`) y de una asignación `= session` (`:96`) no cambian;
  - `ForgetsBeforeAnyBranch` (`:148-159`) no se ve afectado, porque `OnIdle` no usa la condición y su olvido sigue justo tras el retorno por no iniciado;
  - `TheQueriesAndTheAssignmentOfTheAdvisorNeverForgetTheCloseSequence` (`:177-190`) solo fija cuatro consultas concretas, y `CanDrainNow` no está entre ellas.
- **Hay una trampa** (N-4): `:103` prohíbe `_lastAllowedClose[^;]*_states` en una misma sentencia.

### 2. Pruebas existentes y comportamiento cerrado

- Ninguna prueba existente llama a `CanDrainNow`, que es nuevo.
- Las pruebas del ciclo de vida y del drenaje llaman a `core.OnIdle()` directamente (por ejemplo, `WorkspaceHostRig.Idle`), y su semántica no cambia.
- El veto, la retirada, el marcado IC-08 y `NotePilotEdit` no se tocan.
- Hay un efecto neto: entre un Allow y la destrucción ya no hay pasadas, y antes las pasadas de Idle olvidaban. Por eso habrá más secuencias «intactas» y `RetireKnownSafe`.

  Es coherente con D-T2B2-10 («nada ocurrió entre medias») y seguro: la marca se calculó en el propio `AdviseClose`, y el marcado «posiblemente obsoleto» no influye en la política de cierre (`DraftClosePolicy.EvaluateClose` solo mira `RacksAtRiskOf`).

### 3. Bloqueo de los drenajes

No hay bloqueo indefinido en la práctica, pero hay un retraso residual (N-2). Tras un cierre cancelado, `_lastAllowedClose == _active` dura hasta la siguiente entrada del núcleo.
- Si el cierre era el comando CLOSE, `CommandCancelled` o `CommandEnded` y `EnteringQuiescentState` olvidan enseguida.
- Si no hubo comando, cosa que está por medir, las pistas previas al cierre no se drenan hasta la siguiente interacción con el dibujo o hasta mostrar u ocultar la paleta. Un clic que solo cambia de pestaña o recarga en la paleta no cuenta como entrada del núcleo.

Mientras dura, Idle queda enganchado y cada tic es una consulta en memoria. Las lecturas que pide el usuario siguen funcionando.

### 4. La comprobación Debug frente a las guardas del puente

No hay ningún choque:
- `MdiActiveDocument` ya se usa en `Start`, y ninguna guarda la prohíbe en el puente; solo en la paleta y en el catálogo;
- `TransactionManager` no lo prohíbe nadie;
- `LogFault`: ver B-1;
- la línea no lleva `.Database`.

Necesita `using System.Diagnostics;` para `[Conditional]`, y ninguna guarda del puente lo prohíbe.

---

## Hallazgos nuevos

### N-1 (MENOR). La condición de cierre pendiente solo cubre la sesión activa

- **Sección:** D-T2C-03 («falso mientras haya un cierre permitido pendiente de la sesión activa (`_lastAllowedClose == _active`)»).
- **Problema:** si `BeginDocumentClose` se permite para un documento **no activo** y su pregunta de guardar bombea una continuación pendiente mientras el activo es elegible, `CanDrainNow` es verdadero. La pasada llama a `_core.OnIdle()`, que olvida `_lastAllowedClose` (`EventBridgeCore.cs:386`), y la destrucción del documento no activo sale «no intacta».

  El efecto va en la dirección segura (`RetireConservative`), pero contamina la medición de «falsos inciertos» de Smoke-2, que la regla de F4-E34 (`f4-dispositions.md:790`) devuelve al Owner. La exposición es estrecha: depende de que AutoCAD no active antes el documento que cierra (F4-E35 lista «el documento vetado no activo»).
- **De paso:** escrita tal cual, la expresión es verdadera cuando los dos valores son null. Es inocuo porque, sin sesión activa, `CanDrainNow` ya es falso, pero se presta a confusión.
- **Corrección:** cambiar la condición a `_lastAllowedClose.HasValue` (cualquier cierre permitido pendiente). El bloqueo sigue acotado: lo levanta la destrucción, que olvida primero (`:271-272`), o cualquier entrada. La prueba del cierre pendiente necesita una variante con un documento no activo.

### N-2 (MENOR). Retraso de los drenajes tras un cierre cancelado sin comando

- **Sección:** D-T2C-03 y D-T2C-08.
- **Problema:** es lo descrito en la comprobación 3. Antes de T2C, el tic de Idle siguiente olvidaba y drenaba; ahora hay que esperar a una entrada del núcleo. No es un bloqueo indefinido, pero cambia la latencia observable del panel.
- **Corrección:** registrarlo como residual en D-T2C-08 y añadirlo al punto «el cierre cancelado» de Smoke-2. La prueba «falso … hasta la entrada siguiente» ya lo fija en Core.

### N-3 (MENOR). Falta precisión en G-10 y G-2

- **G-10, `SetPost`:** «Exactamente una llamada a `SetPost(` en `src`» cuenta también la declaración `public void SetPost(Action<Action> post)` de `WorkspaceIdleScheduler.cs`. Hay que contar llamadas con el punto delante: `\.SetPost\s*\(`.
- **G-10, la continuación:** D-T2C-04 admite `() => RunIdleContinuation()` «o equivalente». En ese caso el argumento del constructor no contiene `OnIdle`. G-10 tiene que seguir el método intermedio, o apoyarse explícitamente en el recuento de G-2: `(?<![\w.])OnIdle\b` exactamente 2 veces.
- **§5:** «Un post que ejecuta en el acto … haría fallar B y C» vale para el post doble del arnés. El post real del Plugin no lo ejerce Core; lo acreditan G-3 y G-10. Hay que redactarlo así para no sobrevender la prueba.

### N-4 (MENOR). Trampas que H-18 no lista

- `WorkspaceLifecycleGuardTests.cs:103` prohíbe `_lastAllowedClose[^;]*_states` y `_states[^;]*_lastAllowedClose` en una misma sentencia. Si `CanDrainNow` se escribe como una sola expresión (`=> _lastAllowedClose != _active && _states.TryGetValue(…)`), la guarda salta. La condición del cierre tiene que ir en su propia sentencia, o la elegibilidad en el método privado.
- La autoprueba de G-5, «no asigna `_lastAllowedClose =`», tiene que usar `_lastAllowedClose\s*=(?!=)`; si no, salta con la comparación `==` que el propio diseño pide.
- En el cambio declarado (2), la metaguarda exige el literal `string.Equals(path, PaletteHostPath, StringComparison.Ordinal)` (`WorkspacePaletteHostSourceGuardTests.cs:646`). Convertir la elección de regla en un `switch` lo rompería.

### N-5 (MENOR). Las aserciones `TopTransaction` existentes ya son alcanzables en bucles anidados

- **Sección:** §6, aviso previo, y D-T2C-08.
- **Problema:** `WorkspaceController.cs:191` y `WorkspaceSelectionReader.cs:115` corren ahora donde corra la continuación. Si un bucle anidado sin comando (un complemento con ventana no modal que abre una transacción y muestra un `MessageBox`) coincide con un estado en memoria elegible, puede haber una transacción ajena abierta:
  - en el DLL Debug, una de esas aserciones terminaría AutoCAD;
  - la comprobación nueva lo registraría como fallo del drenaje, un falso positivo.

  La exposición es pequeña, porque dentro de los comandos y de las ventanas modales de RackCad el reposo en memoria es falso.
- **Corrección:** añadirlo a los riesgos de Smoke-2. En el texto del registro de la comprobación Debug, decir que la transacción puede no ser del panel.

### N-6 (MENOR). El disparador 4 del §6 puede inducir a error

- **Problema:** «ocultar el panel, provocar cambios y volver a mostrarlo: la reconciliación carga después». Si la superficie era Ready y el rack es el mismo, mostrar el panel reconcilia la selección pero **no** vuelve a leer el resumen (H-20, R-T2C-01).
- **Corrección:** precisar «seleccionar otro rack mientras está oculto; al mostrar, aparece el contexto nuevo y su pestaña». Dejar dicho que un resumen Ready del mismo rack no se refresca.

### N-7 (MENOR). H-18 es más estricto que la guarda real

H-18 dice que `Application/Workspace` prohíbe las subcadenas «también en comentarios». En realidad, `WorkspaceModelBoundaryTests.cs:39-48` lee `CodeOnly`, sin comentarios. La versión «en comentarios» solo existe para los tres archivos de `WorkspaceEditIntentGuardTests.cs:822-832`.

Ser más estricto es inocuo, pero una guarda nueva que lo copiara literalmente fijaría algo que el repositorio no fija. Basta corregir la frase.
