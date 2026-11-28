# Re-revisión acotada: paquete de F4-T3, v2 (emisión de F4-T3A-EDIT-PORT)

**Objeto:**
- el paquete v2;
- la orden de división del Owner (`owner-f4t3-decomposition-decision.md`);
- la tabla de disposición del §9.

**Repositorio:** `94b4d468`, solo lectura. No se ejecutó nada.

## Veredicto para emitir T3A: EMITIBLE TRAS CORRECCIONES

Las correcciones que faltan son de texto: no cambian el diseño ni el código de T3A. Quedan dos IMPORTANTE:
- **N-1:** la disposición de un residual se apoya en una afirmación falsa sobre `PassGate`. Puede llevar al Worker a cambiar una semántica congelada.
- **N-2:** una regla vinculante de T3B contradice la letra del Freeze (D-06).

Los nueve menores conviene arreglarlos en la misma pasada. Ninguno bloquea.

**Recuento de hallazgos nuevos:** 0 BLOQUEANTE, 2 IMPORTANTE y 9 MENOR.

## Estado de los hallazgos de la revisión de la v1

| Hallazgo | Estado | Comprobación |
|---|---|---|
| I-1 (mapa K = 1) | **Cerrado** | H-07 y H-08 están medidos y cuadran con el código (`BrowserSession.cs:49`, `:71-72`, `:78-113`; `WorkspaceBrowserViewModel.cs:211-272`). La tabla de transiciones y su regla son correctas, salvo el criterio de «protegida» (N-2) |
| I-2 (reentrada en la recarga) | **Cerrado** | La marca, en la implementación explícita del puerto, envuelve las tres lecturas sin tocar los cuerpos de T2B2A ni de T2B2B. Una pulsación durante la lectura da `Busy` y nunca llega a `DiscardAndReload`, que solo actúa si el borrador está Limpio y sin pendiente en el paso 6. Detalles en M-a |
| I-3 (lecturas desde avisos) | **Cerrado** | La profundidad dentro de `NotifyStateChanged` cubre las cuatro vías de `StateChanged` (H-06) y la ventana del drenaje. Es compatible con las guardas del cuerpo (`WorkspacePaletteHostSourceGuardTests.cs:465-470`: `StateChanged?.Invoke()`, `catch` y `LogFault`; `WorkspaceBrowserHostGuardTests.cs:203`: sin `DrainNotice`) y con 3 `+=`. Matiz en M-g |
| I-4 (datos del envoltorio) | **Parcial** | Están fijados en R-C2 el nombre del documento, el aviso de pérdida diferido a `DocumentActivated`, la instrucción de activar el documento vetado y el acceso sin `Authorize`. Falta decir de dónde saca el envoltorio la pérdida sin consumir la cola, y de dónde salen los nombres de los racks (M-f) |
| I-5 (veto en el panel; Error) | **Cerrado** | La cola de veto está en el host. El Error de la UI con «Recargar» desde el borrador es una lectura válida de D-08: falla la superficie, no el borrador |
| I-6 (texto vivo) | **Cerrado** | El invariante, R-B4 y la prueba de host están. El envoltorio no necesita vaciar texto vivo |
| I-7 (TOCTOU) | **Cerrado** | R-B8 y R-C1 |
| I-8 (guardas permanentes) | **Cerrado** | Enums como prefijo y autoprueba de T4. Redacción de la guarda de escritura en M-e |
| I-9 (arrastres) | **Parcial** | La tabla está, pero la fila «`BeginDocumentClose` sin enganchar» es falsa (N-1) |
| M-1 a M-12 | **Cerrados** | M-7 deja una redacción de guarda que no se puede implementar tal cual (M-i) |

## Hallazgos nuevos

### N-1. IMPORTANTE — La disposición del enganche fallido de `BeginDocumentClose` se apoya en un hecho falso y empuja a cambiar una semántica congelada

**Dónde:**
- paquete D-T3-09, fila «`BeginDocumentClose` sin enganchar»: «una sesión degradada no admite intención nueva (`PassGate` responde `Unavailable`)»;
- en el código: `WorkspaceHostCore.cs:619-653`, `EventBridgeCore.cs:84-89` y `:460-476`;
- el Freeze: D-04 («La edición sigue permitida en modo degradado»).

**Problema:**
- `PassGate` solo comprueba cinco cosas: el lector (`Unavailable`), el documento activo, el modal, el reposo y `Authorize`. **No mira la degradación.**
- Una sesión cuyo `AttachSessionSources` falla en `BeginDocumentClose`:
  - queda degradada, pero viva en el registro, así que `Authorize` la acepta;
  - para ella `IsCommandActiveInActiveSession` es falso, así que solo cuenta `Editor.IsQuiescent`.
- Por tanto la intención **se admite**, y el borrador nuevo queda sin veto posible.
- El Freeze exige que la edición siga permitida en modo degradado. Un Worker que lea esa fila como requisito añadiría la comprobación de degradación a `PassGate` y caería en C06: cambia T2B2A y contradice D-04.

**Corrección:**
- Reescribir la fila con el hecho correcto: la edición se admite y los borradores de esa sesión no tienen veto. La pérdida sigue siendo visible por `DocumentToBeDestroyed`, el registro y la cola. Es un residual de Smoke-2.
- Si se quiere bloquear la edición, solo puede ser una A-n con el Owner, fuera de T3A: `EventBridgeCore.cs` no está en el alcance.

### N-2. IMPORTANTE — La regla de sincronización por selección amplía D-06 de «texto pendiente» a «protegida»

**Dónde:**
- paquete §2 (fila «Selección Uno(R)» de la tabla K = 1) y R-B6, que citan `P5:335-336`;
- el Freeze: `I-64-proposal-v5.md:335-336`.

**Problema:**
- El Freeze dice: «Si la pestaña activa tiene **texto pendiente**, esa pestaña sigue activa … y la preview de R se crea o reemplaza en segundo plano».
- El paquete aplica la retención a toda pestaña **protegida**, dirty incluida. Con una pestaña dirty sin pendiente, el Freeze manda activar o crear R. La pestaña anterior se desmaterializa con `Dematerialize` no bloqueante y su borrador se conserva, sin pérdida.
- La regla vinculante cambia un efecto congelado y cita como autoridad un texto que dice otra cosa.

**Corrección:**
- Alinear la tabla y R-B6 con la letra de D-06: solo el texto pendiente retiene la pestaña activa.
- Si se prefiere retener también lo dirty, llevarlo como A-n o aclaración al Owner.

No afecta al código de T3A, pero el paquete es la AuthorityRevision de las reglas de T3B.

### M-a. MENOR — La compuerta de reentrada no dice qué devuelven todos los tipos ni cubre las lecturas anidadas

**Dónde:** paquete D-T3-02.

**Problema:**
- «Toda operación que muta responde `Busy`», pero `PilotBoundaryResult` solo enumera `Proceeded`, `Aborted` y `Kept`, y `EditDiscardOutcome` no tiene `Busy`.
- Una intención disparada durante otra lectura, por un gesto bombeado, no se rechaza: es un bloqueo anidado, riesgo de Smoke-2 de F4-E33.

**Corrección:**
- Fijar que la frontera bloqueante con `Busy` aborta la transición y avisa.
- Añadir `Busy` al final de `EditDiscardStatus`. El prefijo ordenado de `WorkspaceLifecycleGuardTests.cs:663-672` lo admite, y `WorkspaceEditIntent.cs` está en el alcance.
- Hacer que las tres lecturas también respondan `Waiting` o `Busy` con una lectura en curso.
- Poner la marca en `try/finally`.

### M-b. MENOR — Los avisos de veto: «inciertos» como lista frente a marca, y avisos que quedan caducados

**Dónde:** paquete D-T3-02 (`PilotCloseView`: «los RackIds en riesgo y los inciertos») frente a D-T3-07 («la marca incierto»); guarda de `AdviseClose` (`WorkspaceLifecycleGuardTests.cs:311-312` prohíbe `_drafts`).

**Problema:**
- En el camino que lanza no hay lista de racks, y `AdviseClose` no puede recorrer `_drafts`, porque su guarda lo prohíbe.
- Un aviso de veto sobrevive al descarte o al cierre posterior de la sesión. Con el panel oculto, se mostraría después como si siguiera vigente.

**Corrección:**
- `PilotCloseView` lleva un indicador de «incierto», no una lista.
- Quitar el aviso de la sesión al descartarla o cuando el siguiente consejo dé Allow. Se puede insertar antes de `_knownClean.Add`, que la guarda solo exige justo antes del `return`.

### M-c. MENOR — La compuerta de avisos impide la recarga perezosa de O-01 en las activaciones por aviso

**Dónde:** paquete D-T3-06, R-B3, R-B5 y R-B10; el Freeze, D-10 O-01 («se recarga de forma perezosa al activarse»).

**Problema:**
- Tras un cambio de documento, la pestaña activa con un borrador Limpio marcado no se recarga hasta un gesto del usuario.
- Ese primer gesto da `AlreadyOpen` sin recargar. El usuario editaría sobre una base marcada.

**Corrección:** añadir a R-B3 y R-B5 dos cosas:
- en una activación por aviso, la superficie muestra «posiblemente obsoleto»;
- el primer gesto sobre un borrador Limpio marcado (`IsPossiblyStale` y Limpio) llama a `TryReload` antes de editar.

Declararlo como cumplimiento de O-01.

### M-d. MENOR — El AllowedWriteScope dice «13 archivos exactos» y la tabla lista 12

**Dónde:** paquete §5.

**Problema:** la cabecera y la tabla no coinciden. El encargo de esta revisión también dice 12.

**Corrección:** corregir la cifra antes de generar el contrato.

### M-e. MENOR — La guarda «T3A no añade operaciones de escritura authored» no dice cómo se compatibiliza con la autoprueba de T4

**Dónde:** paquete D-T3-10.

**Problema:** una guarda de texto no distingue T3A de T4. Si una autoprueba demuestra que T4 puede añadir una operación de Actualizar sin romperla, la guarda tampoco detecta esa operación en T3A.

**Corrección:** redactarla por cuerpo. Los cuerpos de las operaciones de T3A, nombradas una a una, y del decorador no nombran ninguna API de escritura (el patrón `WriteApis` de `WorkspaceLifecycleGuardTests.cs:533-535`). Las operaciones futuras quedan fuera de esa lista.

### M-f. MENOR — R-C1 y R-C2 piden datos que el puerto de T3A no da

**Dónde:** paquete §8.

**Problema:**
- «Por nombre si se conoce»: los nombres de las pestañas viven en la UI (`BrowserSessionRegistry` solo lo usa `WorkspaceBrowserViewModel`), y el estado del piloto no lleva nombre de rack. El comando y el envoltorio no tienen de dónde sacarlos.
- La pérdida en la línea de comandos: el envoltorio no puede leer la cola sin consumirla, porque `TakeLossNotices` es del panel.

**Corrección:**
- Fijar en R-C2 que el envoltorio calcula la pérdida con `GetRacksAtRisk(session)` antes de delegar `SessionDestroyed`. Funciona: es memoria sin `Authorize` y los borradores siguen en el host hasta la retirada.
- Fijar en R-C1 y R-C2 que se muestra solo el id, o añadir ya a `PilotRiskView` un nombre opcional. Así T3C no reabre el puerto.

### M-g. MENOR — La compuerta de avisos no es «imposible en ejecución» en todas las vías, y faltan los nombres de los enums

**Dónde:** paquete D-T3-06 y D-T3-02.

**Problema:**
- `WorkspacePaletteHost.cs:63` llama a `OnControllerStateChanged()` directamente, fuera de `NotifyStateChanged`. Hoy lo cubre `PassGate`, porque ocurre dentro de RACKPANEL con un comando activo, pero la frase del paquete es absoluta.
- Los enums de `State` (`PilotFieldView`), del resultado de `PilotFieldResult` y de la decisión de `PilotBoundaryResult` no tienen nombre. Un Worker que los anide con nombres simples (`Status`, `Decision`, `Kind`) cae en la trampa de H-10 (`WorkspaceDraftBoundaryTests.cs:441-452`, `WorkspaceBrowserBoundaryTests.cs:84-100`).

**Corrección:**
- Matizar la frase y nombrar esa vía como cubierta por `PassGate`; T3B la revisará cuando toque la paleta.
- Nombrar los enums, por ejemplo `PilotFieldState`, `PilotFieldStatus` y `PilotBoundaryDecision`. Ninguno contiene `Tab`, `Editor` ni `Draft`, ni coincide con una palabra de esos archivos.

### M-h. MENOR — Cerrar una pestaña limpia no termina su borrador

**Dónde:** paquete §2 (tabla K = 1: «Cerrar una pestaña limpia: ninguna; se cierra») y R-B7; el Freeze, D-10 («Vida: … hasta cerrar la pestaña (limpia o descartada)»).

**Problema:** si el borrador sigue en el registro, la siguiente intención sobre ese rack da `AlreadyOpen` con un estado viejo.

**Corrección:** cerrar una pestaña limpia llama a `DiscardRack`, que no tiene efecto en el DWG, y lo declara.

### M-i. MENOR — La guarda nueva del modal, tal como está redactada, falla sobre la cadena de reenvío existente

**Dónde:** paquete D-T3-05 («ningún archivo del producto llama a `SetModalRackCadActive`, salvo el reenvío existente del controlador y el ámbito»).

**Problema:** hoy hay más de un reenvío:
- `WorkspaceHostCore.cs:101` y `:144` (`_bridge.SetModalRackCadActive`);
- `BridgeLink` del controlador (`WorkspaceController.cs:210`);
- `AutoCadEventBridge.cs:110` (`_core.SetModalRackCadActive`).

**Corrección:** redactarla sobre los dos puntos de entrada públicos:
- `WorkspaceController.SetModalRackCadActive`, al que nadie llama;
- `_host.SetModalRackCadActive`, al que solo llaman el reenvío literal y `EnterPanelModal`.

## Comprobado y correcto en la v2

**Los nombres nuevos frente a H-10:** `IWorkspacePilotEditPort`, `WorkspacePilotValueText`, `WorkspacePanelModalScope`, `WorkspaceNoticeDepth`, `WorkspacePilotEditGate`, `PilotFieldView`, `PilotFieldResult`, `PilotBoundaryResult`, `PilotBoundaryKind`, `PilotRiskView`, `PilotLossView` y `PilotCloseView`.
- Ninguno contiene «tab», «editor», «draft» ni «subscription». «EditPort» y «EditGate» no son «Editor».
- Ninguno coincide con una palabra de `BrowserSession.cs`, `BrowserSessionRegistry.cs` ni `Drafts/`.
- Ninguno nombra `PushBackPilot*`.

**El decorador y el contador en `Application/Workspace`:** ninguna guarda prohíbe allí un tipo que implemente un puerto, `IDisposable`, `Action` ni `Func`. Hay precedentes: `WorkspaceInstrumentation` con `using`, y `WorkspaceIdleScheduler` y `WorkspaceLazyInstance` con delegados. La guarda de implementador único es solo de `IWorkspaceHostPort`.

**El controlador:**
- la profundidad en `try/finally` dentro de `NotifyStateChanged` cumple las guardas de su cuerpo y el recuento de 3 `+=`, siempre que el contador use `++` o viva en Application;
- `\bevent\b` = 1 obliga a no usar la palabra en el adaptador anidado;
- `PilotEdit`, `EnterPanelModal` y el adaptador anidado públicos no rompen `PublicSurfaceLeaks` ni ninguna guarda de miembros;
- `SetEditIntentReader` sigue apareciendo una vez, fuera del constructor;
- `OnDrained` no nombra nada de la intención.

**El cuerpo de `AdviseClose` (`:309-325`):** el encolado en los caminos de Veto obliga a cambiar dos regex, `\.IsVeto\s*\)\s*return…Veto` y el `catch`, y está declarado. `AdviseCloseForbidden` sigue intacto si la cola nueva no se llama `_lossNotices` y no se usa `_drafts`. La secuencia de cierre de F4-T2B2B y `CanDrainNow` de T2C no cambian: es memoria del host y no hay ninguna entrada nueva del core del puente.

**Los arrastres:**
- **M-2 y D-1:** las cotas son coherentes.
- **T2C 1:** `\.OnIdle\s*\(` con cualquier receptor = 1 cuadra con el código actual, y `.OnIdleTick(` no cuenta. Los miembros llamados son exactamente `OnIdleTick` y `SetPost`.
- **T2C 2:** la forma aplanada del post coincide con `AutoCadEventBridge.cs:41`.
- **T2C 4:** `Contain("IdleContinuation.Pass", OnIdle)` sigue cumpliendo G-10. Las mutaciones que hay que actualizar son las declaradas, y el literal no aparece en ningún otro archivo de pruebas.

**RequiredTests:** MinSelected 2071 cuadra con la CI `43319785427`. Los archivos nuevos de prueba entran en el filtro `~RackCad.Tests.Workspace`.

**R-B y R-C frente al puerto de T3A:** todas las reglas tienen operación en el puerto, salvo los nombres y la fuente de la pérdida de M-f. Además, R-B6 necesita un cambio de `BrowserSession`, que es de T3B y está declarado como tal.

**La división en tres subtareas:** autorizada de forma explícita por el Owner. Las reglas de T3B y T3C no dejan a T3A con dependencias hacia delante.
