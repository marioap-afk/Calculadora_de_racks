# Revisión adversarial independiente: paquete de F4-T3 (v1)

**Objeto:** `I-64-f4-t3-gate-contract-package.md` (v1), contra la orden del Owner `owner-f4t3-authorization.md`.
**Repositorio:** worktree `architecture/workspace-persistente-rackcad` en `94b4d468`, solo lectura. No se ejecutó nada que cambie el estado, ni dotnet ni AutoCAD.

## Veredicto: EMITIBLE TRAS CORRECCIONES

No hay ningún hallazgo bloqueante:
- la descomposición es legítima;
- la semántica del núcleo que expone el puerto es correcta;
- no hay ningún camino de escritura authored.

Hay que corregirlo antes de emitir por cuatro motivos:
- falta un paso que la orden exige antes de emitir: el mapa del ciclo K = 1;
- se pierden arrastres ya asignados a T3, y uno de ellos es un camino de pérdida silenciosa;
- el puerto no cubre lo que T3B necesitará. Eso contradice el objetivo declarado de evitar otra ronda de guardas;
- el envoltorio del asesor no tiene, tal como está descrito, los datos necesarios para hacer lo que promete.

**Recuento:** 0 BLOQUEANTE, 9 IMPORTANTE y 12 MENOR.

---

## Hallazgos

### I-1. IMPORTANTE — Falta el mapa del ciclo de materialización K = 1 que la orden exige antes de emitir

**Dónde:**
- paquete §2 y §3 (D-T3-01), y D-T3-02 (`PilotBoundaryKind`);
- en el código: `src/RackCad.Application/Workspace/BrowserSession.cs:48-49`, `:71-72` y `:78`, y `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:211-310`.

**Problema:**
- El paso 3 de la orden («map the K=1 materialization lifecycle») tiene que hacerse antes de emitir T3. El §2 mapea la UI (H-16) y el texto pendiente (H-01 a H-03), pero no el ciclo K = 1. El §7 lo deja para el paquete de T3B.
- El puerto de T3A ya fija el conjunto de fronteras, y ese conjunto depende de ese ciclo. En el código, la superficie viva es siempre la pestaña activa (`LiveSurfaceRackId => _activeRackId`), así que desmaterializar coincide con cambiar la pestaña activa. Eso pasa por dos vías:
  - por un gesto del usuario en el panel, que es TabChange y bloquea;
  - por la sincronización automática `ApplySelectionContext` de una selección Uno(R) hecha en el dibujo. Esa vía no es una acción del usuario dentro del panel, así que es no bloqueante (D-10: «iniciadas por el usuario dentro del panel»).
- El paquete no dice qué frontera cruza cada transición real ni en qué orden: frontera bloqueante y luego desmaterialización, o solo desmaterialización. Tampoco lo dice para el cierre de una pestaña, la activación por selección, el cambio de documento y ocultar el panel. Sin ese mapa no se puede decidir si `PilotBoundaryKind` y sus reglas bastan.

**Corrección:**
- Añadir al §2 el mapa medido: `BrowserSession` (`MaxLiveSurfaces`, `LiveSurfaceRackId`, `Activate`, `Pin`, `Close` y `ApplySelectionContext`) y `WorkspaceBrowserViewModel` (`SyncSurface`, `StartSurface`, `Materialize` y `ClearSurface`).
- Añadir una tabla «transición real → frontera del puerto → bloquea o no → quién la llama».
- Fijar como regla que la sincronización automática de selección nunca cruza una frontera bloqueante y que conserva el pendiente.

### I-2. IMPORTANTE — Arrastre omitido de F4-E35: reentrada durante la lectura de la recarga, un camino de pérdida silenciosa

**Dónde:**
- paquete D-T3-02 (`TryReload` «delega» sin cambios) y §4 «Lo que no cambia»;
- en el código: `WorkspaceHostCore.cs:451-476`;
- origen: F4-E35, «Arrastres a F4-T3: … la reentrada durante la lectura de la recarga», y `F4-T2B2B-LIFECYCLE/1/R20261112T201718Z-060d/independent-review.md:183`.

**Problema:**
- `TryReloadPilotEdit` comprueba `NeedsLazyReload` (paso 6), lee con el documento bloqueado (paso 7) y llama a `DiscardAndReload`, que vacía el pendiente (`DraftSession.cs:420`), sin volver a comprobar.
- Si algo bombea mensajes durante la lectura (el bloqueo o el lector) y en ese intervalo llega una pulsación, el diseño de T3 la convierte en `SetPendingText` (§7: «cada pulsación → SetPendingText»). El borrador pasa a tener pendiente y la recarga lo borra en silencio.
- IC-08 hace frecuente el marcado «posiblemente obsoleto» (grueso por sesión), y la recarga perezosa al activar una pestaña es justo la ruta de T3B.
- El paquete no menciona este arrastre.

**Corrección**, en T3A y sin tocar `Drafts/`:
- **(a)** una bandera de «lectura del piloto en curso» en la implementación del puerto, puesta alrededor de las tres operaciones que leen. Con ella activa, `SetPendingText`, `CommitField`, `CrossBoundary` bloqueante y los descartes responden `Waiting` (o el valor equivalente) sin efecto;
- **(b)** o volver a comprobar `NeedsLazyReload` después de la lectura y antes de `DiscardAndReload`. Esta opción cambia el cuerpo de la recarga y obliga a declarar el cambio de `WorkspaceLifecycleGuardTests.cs:444-458` (AssertInOrder de la recarga).

En los dos casos: una prueba con un lector simulado que, dentro de `ReadIntent`, llama a `SetPendingText` y comprueba que el pendiente sobrevive o que se rechaza de forma visible, más un oráculo de Smoke-2.

### I-3. IMPORTANTE — Ninguna defensa en ejecución impide que las lecturas del puerto se disparen desde una pila de callback o desde el drenaje

**Dónde:**
- paquete D-T3-02 («leen … en una acción del usuario») y §7 (rematerializar → `GetField`, recarga perezosa al activar);
- en el código: `WorkspaceController.cs:129-171`;
- origen: F4-E33, «A F4-T3: … la defensa frente al drenaje», y `F4-T2B2A-INTENT-WIRING/1/…/independent-review.md:285-287`;
- la orden: «Do not reintroduce DWG reads into callback stacks».

**Problema:**
- `StateChanged` se emite dentro de callbacks de AutoCAD: `DocumentActivated`/`DocumentToBeDestroyed` (vía `ActiveSessionChanged`), `CommandWillStart`/`EnteringQuiescentState` (vía `CommandStateChanged`) y la ventana de aviso del drenaje.
- Si T3B reacciona a `StateChanged` rematerializando la pestaña activa, y esa pestaña tiene un borrador Limpio marcado, llamará a `TryReload` o `TryBeginEdit` dentro de esa pila. Eso es `LockDocument` y una lectura del DWG dentro de `DocumentActivated` o del drenaje, justo lo que T2C eliminó.
- `PassGate` comprueba reposo, modal y autorización, pero no si se está dentro de un callback.
- El paquete no recoge el arrastre ni fija una regla para T3B.

**Corrección:**
- En T3A, una compuerta en ejecución: el controlador marca una profundidad de «aviso en curso» alrededor de `NotifyStateChanged` cuando lo invoca desde un manejador del puente. Es memoria pura, sin `Dispatcher` (que `WorkspacePaletteHostSourceGuardTests.cs:488-497` prohíbe en el controlador) y sin `+=`. Mientras esa marca está activa, las tres operaciones que leen responden `Waiting`.
- Una regla vinculante para T3B: solo un gesto explícito del usuario dispara la intención, «Comprobar ahora» o la recarga, nunca un manejador de `StateChanged`.
- Una prueba que llame a la intención desde un `StateChanged` simulado y espere cero lecturas.

### I-4. IMPORTANTE — El envoltorio del asesor (D-T3-08) no tiene los datos que necesita, y el aviso de pérdida irá a una línea de comandos que no se verá

**Dónde:**
- paquete D-T3-08;
- en el código: `WorkspaceSession.cs:16-23` (sin nombre), `IWorkspaceLifecycleAdvisor.cs:19-20` («The advisor must not call the bridge back»), `WorkspaceHostCore.cs:580` (`internal LossNotices`), `RackCad.Application.csproj:11` (`InternalsVisibleTo` solo a las pruebas) y `EventBridgeCore.cs:287-289` (`Destroy` antes de `TellAdvisorTheSessionEnded`).

**Problema:**
- **(a) «Nombra el documento».** El asesor solo recibe un `SessionId`, y `WorkspaceSession` no tiene nombre. Nombrar el documento exige preguntar al puente (`TryGetSession` → `NativeInstanceId`) y recorrer `DocumentManager` dentro de `BeginDocumentClose`. En `SessionDestroyed` ya no es posible, porque la sesión se destruyó antes de llamar al asesor. El paquete no dice cómo, y el Worker tendría que inventarlo.
- **(b) «Si la cola recibió un aviso nuevo, sin consumirla».** El Plugin no puede leer `internal LossNotices`, y el puerto solo ofrece `TakeLossNotices`, que la consume. Así descrito, no se puede implementar.
- **(c) Destino del aviso de pérdida.** Se escribe «en la línea de comandos del documento activo si existe» dentro de `DocumentToBeDestroyed`. En ese instante el documento activo suele ser el que se destruye, y el historial de órdenes es de cada documento, así que el aviso desaparece con él. D-17 exige que la pérdida sea visible «en la línea de comandos». Además, escribir en `DocumentToBeDestroyed` amplía la excepción acotada de D-04 que T2B2B declaró para la retirada (`I-64-f4-t2b2-gate-contract-package.md:203`), y el paquete no lo declara.
- **(d) `GetRacksAtRisk` y `GetField`.** Ninguna de las dos dice si autoriza. El veto de CLOSEALL o QUIT puede ser de un documento no activo: si autorizan, el mensaje del veto sale vacío.
- **(e) El mensaje del veto.** Nombra `RACKPANELDESCARTAR`, pero el comando solo actúa sobre el documento activo. Para un documento vetado que no es el activo, el mensaje debe decir que hay que activarlo primero.

**Corrección:**
- **(a)** fijar la vía exacta: el envoltorio resuelve el nombre en `AdviseClose`, antes de delegar, con `TryGetSession` del puente (consulta pura) y el recorrido de `DocumentManager`, y lo declara como lectura de memoria permitida por P-03. Alternativa: que el envoltorio lleve un mapa en memoria sesión → nombre, rellenado en `AdviseClose`;
- **(b)** añadir al puerto una lectura que no consuma (por ejemplo, `int LossNoticeCount` o `PeekLossNotices()`), o hacer que el envoltorio calcule la pérdida con `GetRacksAtRisk` justo antes de delegar `SessionDestroyed`;
- **(c)** diferir la escritura del aviso de pérdida al siguiente `DocumentActivated` (el relevo `OnActiveSessionChanged` del controlador), sin `Dispatcher`, y declarar la escritura en el callback como ampliación acotada. Con QUIT solo queda RackLog, y se declara;
- **(d)** decir de forma explícita «sin `Authorize`, solo memoria» para `GetRacksAtRisk` y `GetField`, y justificarlo frente a INV-22, porque `GetSelectionContext` sí autoriza;
- **(e)** añadir al mensaje del veto la instrucción de activar el documento.

### I-5. IMPORTANTE — El puerto no tiene canal para el aviso del veto en el panel ni operaciones para el estado Error, que T3B necesitará

**Dónde:**
- paquete D-T3-02 y §7 («los avisos del veto y de la pérdida en el panel (`TakeLossNotices`)», «el estado Error con Recargar o Descartar»);
- el Freeze: D-17 («el mensaje del veto, en la línea de comandos y en el panel»), P-03 («aviso encolado del panel»), D-08 (Error: «conserva su borrador y ofrece Recargar o Descartar») e INV-25;
- en el código: `WorkspaceHostCore.cs:454` y `DraftSession.cs:291`, `:601-616`.

**Problema:**
- **(a)** `TakeLossNotices` solo trae pérdidas. Un veto no deja nada que el panel pueda consumir, y el envoltorio no notifica al panel. El §7 promete un aviso del veto en el panel que el puerto no puede dar.
- **(b)** Ninguna operación del puerto lleva un borrador a Error (`EnterError`), así que `GetField` nunca verá Error en el producto.
- **(c)** «Recargar» desde Error es imposible: `TryReloadPilotEdit` devuelve `NotNeeded` si el borrador no está Limpio (`NeedsLazyReload`).
- El paquete afirma que el puerto cubre T3B «sin otra ronda de guardas». No es así.

**Corrección:**
- **(a)** una cola de avisos de veto, escrita por el envoltorio o por el host en memoria (P-03), y su `Take…` en el puerto, o generalizar la cola a «avisos de cierre»;
- **(b) y (c)** decidir ahora si el Error de D-08 es solo de la UI o también del borrador. Si es del borrador, añadir `EnterError` y una «Recargar desde Error» que use `DiscardAndReload` (declarando el cambio de la cota `.DiscardAndReload(`). Si es solo de la UI, fijar que «Recargar» es `DiscardRack` seguido de `TryBeginEdit` y declarar su semántica de pérdida: si la intención espera o falla, el borrador ya no existe.

### I-6. IMPORTANTE — Texto vivo fuera del borrador: un `SetPendingText` rechazado deja en la caja texto que ninguna frontera protege, y la obligación 15 no figura como invariante

**Dónde:**
- paquete D-T3-02 (`SetPendingText` exige `Authorize` y `CanEdit`; las no bloqueantes no capturan nada), D-T3-08 y §7;
- origen: la fila de T3 del paquete de F4 (`I-64-f4-gate-contract-package.md:46`: «texto pendiente vivo copiado al borrador **antes** de toda frontera no bloqueable, también `BeginDocumentClose`»), la obligación 15 (`:83`) y T2B2 `:281` («envolverlo para vaciar el texto vivo antes del consejo»);
- la orden: «invalid pending text must be captured and retained exactly».

**Problema:**
- El diseño confía en que cada pulsación ya está en el borrador. Si `SetPendingText` se rechaza (sesión no activa en la ventana de un cambio de documento, `CanEdit` falso o borrador inexistente), el texto queda solo en la caja.
- `CrossBoundary` no bloqueante es un no-op que no captura nada. En la siguiente desmaterialización ese texto se pierde sin aviso. Si no hay borrador, el veto ni siquiera lo ve.
- El paquete tampoco dice por qué el envoltorio no necesita vaciar el texto vivo antes de `AdviseClose`, que era su razón de ser en T2B2.

**Corrección:**
- Fijar el invariante «la caja nunca contiene texto del usuario que el borrador no tenga».
- Elegir uno de dos mecanismos:
  - **(a)** un rechazo de `SetPendingText` obliga a T3B a restaurar la caja desde `GetField` y a avisar de forma visible;
  - **(b)** `CrossBoundary` no bloqueante acepta el texto vivo de la caja y lo guarda como pendiente sin `Authorize` y sin validar, solo si el usuario lo editó (nunca desde `Show`).
- Declarar en D-T3-08 que no hace falta vaciar el texto vivo por ese invariante, y probarlo.

### I-7. IMPORTANTE — `RACKPANELDESCARTAR` descarta lo que no confirmó (TOCTOU entre la lista y la confirmación)

**Dónde:**
- paquete D-T3-07;
- C-F4-03: «ante un destino ambiguo falla … sin efecto»;
- la orden: «must not silently select an ambiguous draft».

**Problema:**
- El comando lista los racks en riesgo, espera `GetKeywords` y llama a `DiscardDocument`, que descarta todos los borradores de la sesión.
- Mientras espera la respuesta, el panel (modeless) sigue editable: D-01 solo deshabilita las acciones que leen o escriben el DWG. Así, un borrador o un pendiente creado después de la lista se descarta sin haberse mostrado ni confirmado.
- Lo mismo afecta a «Descartar borradores de este documento» de T3B.
- Además, el plan no tiene la rama «sesión activa sin racks en riesgo» (solo con borradores limpios, o sin ninguno).

**Corrección:**
- Tras «Sí», volver a leer `GetRacksAtRisk`. Si el conjunto difiere del listado, no descartar nada y avisar. Alternativa: descartar exactamente los racks listados, con `DiscardRack` por rack.
- Fijar la rama sin riesgo: mensaje y descarte silencioso de los limpios, o ningún efecto, decidido.
- Añadir la misma regla a la acción del panel en el esbozo de T3B.

### I-8. IMPORTANTE — Las guardas nuevas del puerto repiten el modo de fallo I-1

**Dónde:**
- paquete D-T3-10, «Guardas nuevas»: «el puerto: … ningún método de escritura authored»;
- D-T3-02: `PilotBoundaryKind` «sin `Update`, que es de T4»;
- INV-21: «el puerto no expone escrituras salvo Actualizar y el descarte»;
- precedentes: F4-E33 (N-1, N-5), F4-E35 (M-1, M-2) y F4-E37.

**Problema:**
- Una guarda que prohíbe para siempre cualquier método de escritura en `IWorkspacePilotEditPort` obligará a T4 a cambiarla si Actualizar entra por este puerto.
- Un conjunto exacto de `PilotBoundaryKind` o del enum de estado impide añadir `Update`.
- Es el mismo modo de fallo que ya se repitió tres veces en F4. Según la regla del proyecto, un fallo repetido se devuelve antes de clasificarlo por analogía.

**Corrección:**
- Redactar cada guarda nueva acotada a T3A: los enums como prefijo ordenado, y la prohibición de escritura como «hasta T4» o limitada a nombres concretos.
- Declarar en el paquete si Actualizar irá por este puerto o por otro (H-15 del paquete de F4: «puertos nuevos para Actualizar y descartar»).
- Añadir una autoprueba de que T4 podría añadir `Update` sin romperla.

### I-9. IMPORTANTE — Arrastres de F4-E33 y residuales de T2B2B asignados a T3 que el paquete no dispone

**Dónde:**
- el §1 del paquete no cita F4-E33;
- `f4-dispositions.md:749-753` («A F4-T3: N-2 cuando `ReadPreviewWitness` tenga llamador; N-5 con sus propios cambios de guarda declarados (H-03); la compuerta de `ReadPreviewWitness` y la defensa frente al drenaje»);
- `I-64-f4-t2b2-gate-contract-package.md:204` (residual de `Stop()`: «queda para T3 o F7») y `:434` (fallo al enganchar `BeginDocumentClose`: «Se declara para F4-T3 y Smoke-2»).

**Problema:**
- La orden pide llevar los arrastres de forma explícita, y el paquete dice que trae todos. Faltan estos, y cada uno necesita una disposición escrita:
  - **N-5:** la cabecera de `WorkspaceEditIntentGuardTests.cs:16-17` sigue siendo inexacta. T3A sigue la vía del controlador, que está abierta, así que N-5 se puede cerrar como no aplicable, pero hay que escribirlo;
  - **N-2 y la compuerta de `ReadPreviewWitness`:** siguen siendo código oscuro por D-T3-04;
  - **la defensa frente al drenaje:** ver I-3;
  - **`BeginDocumentClose` sin enganchar:** una sesión sin veto posible en la que T3 permitirá editar;
  - **`Stop()`.**

**Corrección:**
- Añadir una tabla de disposición de estos arrastres.
- Para el enganche fallido: o el puerto lo expone y la intención se rechaza (por ejemplo, el puente expone «sin protección de cierre»), o se declara de nuevo como residual de Smoke-2 con la decisión de producto.

### M-1. MENOR — Presupuesto y tamaño de T3A

**Dónde:** paquete D-T3-01 y C07.

**Problema:**
- La división es legítima por F4-E09 («cada tarea contratada por separado empieza en 1 de 3») y por los precedentes F4-E18 y F4-E29. El Owner aceptó la división de T2B2 de forma explícita.
- La orden fija «F4-T3, attempts 1/3». Avisar después de emitir le quita al Owner la ocasión de objetar.
- T3A abarca 19 rutas: puerto, comando, mensajes, modal, diez cambios de guarda y los arrastres de T2C y T2B2B. Corre el riesgo de que la planificación lo evalúe como Long-horizon, que es C07 y STOP.

**Corrección:**
- Avisar al Owner de la división y de la duplicación de intentos **antes** de emitir T3A.
- Valorar sacar los arrastres de guardas y pruebas (T2C 1 a 4, M-2, M-3, M-5 y D-1) a una subtarea propia, o justificar el horizonte Medium.

### M-2. MENOR — D-T3-03 es implementable, pero le falta la regla concreta y el dominio

**Dónde:** paquete D-T3-03 y H-03.

**Problema:**
- En .NET 8, `"R"` y `ToString()` dan la representación más corta que se recupera exacta, pero usan exponente desde 1E+15 y por debajo de 1E-05. `"F<n>"` no es la más corta: 0.1 se escribe «0.10000000000000001».
- El paquete no dice qué hacer fuera del dominio: `Annotations` nulo, valores ≤ 0 o no finitos. La carga repara ≤ 0 (`PushBackEditorState.Load.cs:206`), pero +Infinity pasa esa reparación.
- Mostrar `double.Epsilon` o `MaxValue` en coma fija da cadenas de unos 330 caracteres: se recuperan bien, pero el campo deja de ser usable.

**Corrección:**
- Fijar el algoritmo:
  1. `s = v.ToString("R", CultureInfo.InvariantCulture)`;
  2. separar la mantisa y el exponente;
  3. desplazar la coma en la cadena de dígitos, rellenando con ceros a la izquierda o a la derecha;
  4. quitar los ceros sobrantes;
  5. cambiar «.» por `CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator`.

  Es el mismo número decimal que `"R"`, así que `LocalizedNumberParser.TryDouble` (cultura actual primero) devuelve el mismo `double`.
- Precondición: finito y > 0. Si no se cumple, `CommittedText = null` con un motivo, y no hay pendiente.
- Aviso de guarda: en `WorkspacePilotText.cs` no se puede nombrar `IFormatProvider` (`WorkspaceHostSourceGuardTests.cs:546-553` y `WorkspaceModelBoundaryTests.cs:81-92`, con «Provider» sin distinguir mayúsculas).

### M-3. MENOR — D-T3-04 es seguro frente a INV-27, pero se desvía de T2B sin dar destino y tiene un coste de UX

**Dónde:**
- paquete D-T3-04;
- `I-64-f4-t2b-gate-contract-package.md:141` («el par … que T3 captura al mostrar la preview»);
- D-10 («con un aviso»).

**Problema:**
- No leer el valor y no aplicar el gesto con `NotSupplied` cumple INV-27: nunca se escribe sobre lo no visto.
- Pero se desvía de lo contratado en T2B («T3 captura») y deja «una tarea posterior» sin nombre.
- Un campo vacío en la preview, cuya primera pulsación se descarta sin aviso, puede parecer un defecto en Smoke-2.

**Corrección:**
- Nombrar el destino (F5 o F7) y registrar la desviación.
- Fijar para T3B que el campo de la preview no se muestra como vacío editable (marcador de solo lectura, «—»), y que el gesto descartado deja un indicio visible.

### M-4. MENOR — Trampas de guardas no declaradas, que un Worker puede pisar y disparar C02

| Dónde | Trampa |
|---|---|
| `WorkspaceSourceGuardExceptionTests.cs:30-34` | El Fact admite solo `RACKPANEL`, y (6) solo cita `:36-61`. Hay que fijar la regla nueva de `DeclaresEntryPoint`: `RACKPANEL` exactamente una vez, `RACKPANELDESCARTAR` como mucho una, nada más. Con esa regla el Fact y el `void A() {}` de la teoría siguen valiendo |
| `WorkspacePaletteHostSourceGuardTests.cs:311-317` | Prohíbe `aun\s+no` y `todavia\s+no` (sin tilde y sin distinguir mayúsculas) en el archivo del comando. Los textos nuevos del veto y del comando deben evitarlos |
| `WorkspaceModelBoundaryTests.cs:94-104` y `WorkspaceBrowserBoundaryTests.cs` | «Tab» se busca como subcadena sin distinguir mayúsculas, así que «Editable», «Mutable», «Table» o «Stable» en un nombre de tipo fallan |
| `WorkspaceBrowserBoundaryTests.cs:84-100` y `WorkspaceDraftBoundaryTests.cs:441-452` | Un tipo nuevo, también anidado, del namespace `RackCad.Application.Workspace` cuyo nombre simple coincida con una palabra de `BrowserSession.cs` o de `Drafts/*.cs` (`Status`, `Kind`, `Decision`, `Change`…) rompe guardas de F3 y F4-T1 |
| `WorkspaceEditIntentGuardTests.cs:504-514` | Solo el core, `IWorkspaceEditIntentReader.cs` y `WorkspaceEditIntent.cs` pueden nombrar `PushBackPilot*`. `IWorkspacePilotEditPort.cs` y `WorkspacePilotText.cs` no |
| `WorkspaceEditIntentGuardTests.cs:821-833` | También aplica a `WorkspaceHostCore.cs`, con comentarios: nada de `Transaction`, `Database`, `Commit(` ni «provider» |
| `WorkspaceEditIntentHostTests.cs:990-1009` | El adaptador del piloto no puede ser un campo de instancia del host (fuera de alcance); hay que usar un campo estático o un local |
| `WorkspaceLifecycleGuardTests.cs:296` | `: IWorkspaceLifecycleAdvisor` debe seguir siendo la primera base, antes de `IWorkspacePilotEditPort` |
| `WorkspaceBridgeSourceGuardTests.cs:205-214` | Una propiedad `private static Document X => …` cuenta como campo prohibido |
| `WorkspaceHostSourceGuardTests.cs:503-514` | `RackLog.` solo dentro de `LogFault`, así que el ayudante de mensajes del comando no puede registrar con `RackLog` |

**Corrección:** añadir esta lista a los invariantes, como límites ya vigentes.

### M-5. MENOR — RequiredTests y la regla RED

**Dónde:** paquete §5 RequiredTests.

**Problema:**
- `CustomPropertiesCommandGuardTests` y `SelectiveEditorOpenTests` están en el namespace `RackCad.Tests`, no en `RackCad.Tests.Workspace`, así que el filtro no selecciona los cambios (7) y (8).
- «MinSelected = la línea base» no da número. T2C sí fijaba 1919 con su CI.
- Los arrastres que solo tocan guardas pasan en el RED. El Owner ya lo señaló al cerrar T2C, y el paquete no lo declara.

**Corrección:**
- Fijar MinSelected = 2071 (CI `43319785427` de `33e43f9f`).
- Añadir un filtro, o declarar que el esqueleto del RED ya lleva el atributo del comando.
- Declarar que los arrastres de guardas pasan en el RED y prueban su discriminación con autopruebas de mutación.

### M-6. MENOR — `WorkspacePilotText.cs` mezcla tres responsabilidades y el alcance es ambiguo

**Dónde:** paquete D-T3-06 («`WorkspacePilotText.cs` u otro archivo nuevo de `Application/Workspace`») frente al AllowedWriteScope exacto.

**Problema:** «u otro archivo nuevo» contradice la lista exacta, y eso es C01.

**Corrección:** fijar el archivo y el nombre de cada tipo (texto, plan del comando, contador del modal), sorteando las trampas de M-4.

### M-7. MENOR — `EnterPanelModal`

**Dónde:**
- paquete D-T3-06;
- `WorkspacePaletteHostSourceGuardTests.cs:493`, que obliga a conservar el `SetModalRackCadActive` público tal cual.

**Problema:**
- Cualquiera puede llamar al método público en paralelo al contador y desactivar el modal con ámbitos todavía abiertos.
- La afirmación «las guardas existentes siguen prohibiéndolas en … el Plugin» no vale para `ShowModalWindow`: `WorkspaceBridgeSourceGuardTests.cs:139-142` prohíbe `ShowModalDialog`, `ShowDialog` y `MessageBox`, pero no `ShowModalWindow`.

**Corrección:**
- Una guarda: solo `EnterPanelModal` llama a `SetModalRackCadActive`.
- Corregir la frase sobre `ShowModalWindow`.
- Una prueba de `Dispose` doble con anidamiento.

### M-8. MENOR — Exposición del puerto como el propio host y el getter de `PilotEdit`

**Dónde:** paquete D-T3-02 (implementación explícita en el host) y D-T3-05; Freeze D-02 («implementado en el Plugin»).

**Problema:**
- `PilotEdit` devuelve el objeto `WorkspaceHostCore`. Desde la UI se puede convertir a `IWorkspaceLifecycleAdvisor` y llamar a `AdviseClose`. Las guardas de la UI solo prohíben el nombre `WorkspaceHostCore`.
- Si el envoltorio usa el getter de `PilotEdit`, que accede a `Navigation`, crearía el host de navegación dentro de `BeginDocumentClose`.

**Corrección:**
- Un adaptador anidado en el controlador que solo delegue, como `BridgeLink`, y registrar la desviación de D-02.
- El envoltorio usa el puerto del host directamente, nunca el getter.

### M-9. MENOR — Contención del envoltorio y semántica de las fronteras no editables

**Dónde:** paquete D-T3-08 y D-T3-02.

**Problema:**
- **(a)** No se dice qué devuelve el envoltorio si la delegación lanza, ni qué hace si su propio `catch` envuelve la decisión. Debe ser Veto, o volver a lanzar para que el core lo convierta en Veto.
- **(b)** Con un borrador no editable (Applying, Unknown o Error) y con pendiente, `DraftSession.CrossBoundary` bloqueante devuelve `Kept` (`DraftSession.cs:370-371`). El puerto no traduce ese caso.
- **(c)** `DraftSession.CanEdit` es privado (`:316`). La derivación desde `Status` y `IsRegistered` no está escrita.
- **(d)** `CommitField` necesita usar `CrossBoundary` con un valor de `DraftBoundary`, porque `Drafts/` no se toca. No se dice cuál.
- **(e)** No hay decisión sobre Esc (revertir el campo): el puerto no tiene `RemovePendingText`.

**Corrección:** fijar cada punto. Propuesta:
- (b) `Proceeded` con un indicador de «conservado, no editable»;
- (d) `TabChange`, documentado como mecánica;
- (e) «Esc no revierte en el piloto», declarado, o una operación `RevertField` con `NotePilotEdit`.

### M-10. MENOR — `Status = Unknown` por un comparador que lanza se confunde con el Desconocido de D-13

**Dónde:** paquete D-T3-02 (`GetField`), INV-31 («Desconocido con Comprobar ahora») y `WorkspaceHostCore.cs:375-376`.

**Problema:** T3B ofrecería «Comprobar ahora», y `TryCheckNow` respondería `NotApplicable`.

**Corrección:** un valor distinto (por ejemplo, `Indeterminate`) o un indicador `DirtyUndecidable`. Además, justificar por qué la edición se bloquea, ya que `DraftSession.SetPendingText` no depende del comparador. Bloquearla es conservador pero no obligatorio.

### M-11. MENOR — Coherencia documental

**Dónde:** paquete D-T3-02 y §1.

**Problema:**
- `TakeLossNotices` no lleva sesión, y eso contradice la frase general «la sesión va en `WorkspaceRequest` (INV-22)».
- `EditDiscardOutcome` y `EditDiscardStatus.Discarded` documentan «every draft of the session» (`WorkspaceEditIntent.cs:258-275`), lo que es falso para `DiscardRack`.
- La orden del Owner que cita el §1 (`F4-DRAFTS/coordinator/owner-f4t3-authorization.md`) no existe en `94b4d468`.

**Corrección:**
- Declarar la excepción de `TakeLossNotices` y su motivo (sesiones destruidas).
- Actualizar esa documentación respetando las guardas de texto.
- Custodiar la orden en el repositorio antes de fijar la AuthorityRevision.

### M-12. MENOR — Detalles del comando y del arrastre T2C-1

**Dónde:** paquete D-T3-07 y D-T3-09.

**Problema y corrección:**
- **(a)** El comando no debe crear el controlador (P-01). Fijar una guarda: `WorkspaceController.GetOrCreate()` aparece una sola vez en el archivo del comando, y `RACKPANELDESCARTAR` usa `IsCreated`.
- **(b)** La lista de racks por RackId (GUID) es ilegible en la línea de comandos. Decidir qué se muestra: el nombre si se conoce, y el id.
- **(c)** T2C-1 todavía deja escapar un alias del core: con `var c = _core; c.OnIdle()`, `(?<![\w.])OnIdle\b` no cuenta `.OnIdle` y `_core\.OnIdle` no casa. Contar `\.OnIdle\s*\(` con cualquier receptor (= 1) y fijar el conjunto de miembros del planificador que llama el puente (`OnIdleTick` y `SetPost`).
- **(d)** La orden pide evidencia dirigida en el host «where practical» si cambia el foco o el modal en código de AutoCAD. Decidir de forma explícita para T3A (`GetKeywords`, `WriteMessage` en `BeginDocumentClose`) si es practicable o una limitación de Smoke-2.

---

## Lo que comprobé y es correcto

**El alcance de la orden:**
- sin mutación authored ni Actualizar;
- solo `AnnotationScale`, con la clave del adaptador;
- distingue `CurrentDraftState` del pendiente;
- el pendiente cuenta para dirty, el riesgo y el veto;
- las cuatro fronteras no bloqueantes del Freeze, más el ciclo de vida, conservan el texto tal cual;
- las bloqueantes abortan con texto inválido y no auto-reparan;
- el descarte es por sesión o por rack, en memoria y con cero escrituras;
- el cierre ofrece Descartar o Cancelar (en T3B);
- el modal se activa antes del diálogo y se limpia en toda salida;
- M-1 a M-5, D-1 y los cuatro menores de T2C están recogidos;
- la cobertura 1-18 está repartida sin huecos de número;
- el oráculo crítico, la regla de nc1, el régimen de cierre y R-T2C-01 diferido (fuera de T3) están bien.

**La descomposición en T3A y T3B:** coherente con F4-E09 y con los precedentes F4-E18 y F4-E29 (sin hablar de «6 intentos»). T4 queda detrás de T3B.

**La semántica que el puerto expone del núcleo** (`DraftSession.cs:338-389`), frente a D-10 y ADR-0032 D6:
- dos fases;
- todo o nada;
- ninguna mutación con un campo inválido;
- el pendiente se limpia tras aplicar;
- las no bloqueantes devuelven `Kept`;
- el gesto propio del campo como frontera transaccional, con el texto inválido que se queda en su caja y sin auto-reparación (ADR-0032 D6);
- mostrar nunca crea pendiente (D-T2-01).

**El resto del puerto:**
- Las no bloqueantes sin `Authorize` son seguras: no tienen efectos y no hay efecto entre sesiones.
- `DiscardRack` con `DiscardByUser` y `DiscardDocument` delegado en `DiscardPilotEdits` cumplen la tabla de veto de F4-E29: el descarte no depende del comparador, y el sucio desconocido participa en el veto.
- La edición en Applying, Unknown o Error queda rechazada por `CanEdit`.
- No hay ningún camino de escritura authored.

**D-T3-03:** implementable en .NET 8 (ver M-2). Todo `double` finito y > 0 se puede mostrar sin exponente y se recupera exacto.

**D-T3-04:** cumple INV-27 y la regla de T2B «`NotSupplied` no es sin cambios», con los reparos de M-3.

**P-03 y D-04:** permiten de forma explícita el aviso no modal en la línea de comandos dentro de `BeginDocumentClose`. El envoltorio delega sin cambiar la decisión. `_lastAllowedClose` y los 13 olvidos no cambian: el aviso solo se escribe tras un Veto, cuando la memoria ya es null. `EnterPanelModal` es una entrada que ya olvida, y la consecuencia es una retirada conservadora. `CanDrainNow` y el cierre pendiente de T2C no cambian.

**Llamar a un `internal static` de `WorkspacePanelCommand` desde el controlador:** ninguna guarda lo impide, y `OnlyTheCommandAdapterWritesToTheCommandLine` (`:84-89`) sigue valiendo.

**Los cambios de guarda declarados (1) a (10) existen en las líneas citadas:**
- `:282-283` y `:237-241` de `WorkspaceLifecycleGuardTests`;
- `:386`, `:459` y `:71`;
- los conteos de `[CommandMethod` en cuatro archivos;
- el censo `:55`;
- el conteo bruto 39 → 40;
- `HandlerForbiddenCalls` `:139-163`;
- las 45 pruebas de `WorkspaceHostSourceGuardTests`, fijadas por dos guardas.

Aflojar `:237-241` lo manda F4-E35. Los `+=` del controlador (3) y del puente (16) no cambian con el diseño. Ninguna guarda fija el conjunto de miembros públicos del controlador.

---

## Riesgos que solo resuelve un smoke con AutoCAD

1. `Editor.WriteMessage` dentro de `BeginDocumentClose` (veto) y de `DocumentToBeDestroyed` (pérdida):
   - ¿bombea mensajes y deja correr una continuación del dispatcher, con una lectura del DWG, dentro de la pila del cierre vetado?
   - ¿lanza con un editor en destrucción?
   - ¿el aviso se ve? El historial es de cada documento.
   - Su orden frente al aviso de guardar en CLOSE, CLOSEALL y QUIT.
2. `GetKeywords` en `RACKPANELDESCARTAR`:
   - la palabra clave «Sí» con tilde;
   - Escape y el valor por defecto;
   - el foco en la paleta mientras espera la respuesta;
   - el cambio de documento durante la respuesta (debe dar Rejected sin efecto);
   - la edición en el panel durante la respuesta (I-7).
3. La reentrada de mensajes durante `LockDocument` y la lectura de la intención y de la recarga (I-2), con el foco en el campo.
4. Lecturas disparadas desde `StateChanged` dentro de `DocumentActivated` o del drenaje, si T3B rematerializa reaccionando a avisos (I-3).
5. `EnterPanelModal` con `ShowModalWindow`, cuando exista un llamador:
   - el modal debe estar activo antes de que el diálogo esté activo;
   - debe limpiarse si AutoCAD cancela o lanza;
   - la interacción con `KeepFocus` de la paleta.
6. Que el veto cancele de verdad CLOSE, CLOSEALL, QUIT y el cierre desde la pestaña con el envoltorio nuevo, y el documento vetado que no es el activo (nombre y mensaje).
7. El coste de evaluar el comparador dos veces por intento de cierre: el consejo y la lista del mensaje.
