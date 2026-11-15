# I-64 F4-T4C — Mapa 3: señales de mutación propia, ciclo de vida con Applying, UI de Actualizar y diagnóstico diferido de T4A2

**Base de medición:** `b15072ff` (custodia del cierre de T4B; solo toca `docs/`). El producto es el de `4314c180` (GREEN de T4B). Todas las anclas son `ruta:línea` relativas al repositorio y medidas en `b15072ff`.

**Modo:** solo lectura. No se compiló, no se ejecutó ninguna prueba y no se abrió AutoCAD.

**Convenciones:**
- **[M]** medido, con su ancla;
- **[I]** inferencia, marcada como tal;
- **[H]** solo lo puede probar el host (AutoCAD real), es decir, Smoke-2 o una medición en host de T4C.

**Abreviaturas:** `FRZ` = `docs/initiatives/I-64-proposal-v5.md`; `F4P` = `docs/initiatives/I-64-f4-gate-contract-package.md`; `T4P` = `docs/initiatives/I-64-f4-t4-gate-contract-package.md`; `T4BP` = `docs/initiatives/I-64-f4-t4b-gate-contract-package.md`; `A2P` = `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md`; `IC` = `docs/initiatives/I-64-implementation-clarifications.md`; `TW` = `tests/RackCad.Tests/Workspace/`; `UW` = `tests/RackCad.UI.Tests/Workspace/`; `CORE` = `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`; `DS` = `src/RackCad.Application/Workspace/Drafts/DraftSession.cs`; `DR` = `src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs`; `BRIDGE` = `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`; `EBC` = `src/RackCad.Application/Workspace/EventBridgeCore.cs`; `CTRL` = `src/RackCad.Plugin/Workspace/WorkspaceController.cs`; `FVM` = `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`; `BVM` = `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`.

---

## 0. Resumen

- **Hoy no existe ningún ámbito de mutación propia ni ninguna supresión** de pistas, ni por sesión ni por miembros. Un Actualizar del piloto, cuando T4C lo cablee tal cual, marcaría como «posiblemente obsoletos» **el propio rack y todos los borradores del piloto de la sesión** en el drenaje siguiente (H-M3-10). Es la violación de INV-08 e INV-28 que IC-08 encargó cerrar.
- **Las dos vías de C-F4-04 chocan con hechos medidos:**
  - la vía A (ámbito por sesión y miembros, cerrado con el fin del propio comando) choca con la coalescencia por tipo (`WorkspaceSession.cs:25-26`) y con el marcado grueso por sesión;
  - la vía B (pista con handle) choca con «coste constante» del Freeze (`FRZ:217-218`), con IC-04 y con una guarda que prohíbe `handle` en `WorkspaceSession.cs` (`TW/WorkspaceNavigationEchoTests.cs:325-340`).
- **`DiscardRack`, `DiscardDocument` y `DiscardByUser` sí actúan hoy sobre un borrador en Applying:** lo desregistran, y la ranura no se libera antes de tiempo (§2.1). R-T4-07 sigue abierto en esa parte.
- **No hay botón ni gesto de Actualizar en la UI** del panel. `PilotBoundaryKind` no tiene `Update`. Hay **guardas que impiden** que el puerto del panel tenga una operación con «Update» en el nombre y que el adaptador del controlador haga otra cosa que reenviar literalmente (§3).
- **El diagnóstico diferido de T4A2** es llevar el diagnóstico de admisión y la pista del editor clásico a la vía `TryCheckNow`. Hoy no viaja (`EditCheckOutcome` no lo lleva), y la UI **retira** el diagnóstico persistente ante `Recovered` y `Conflict` (§4).

---

## 1. La mutación propia (C-F4-04, D-04, INV-08, INV-28, IC-08, R-T4-08)

### 1.1 Qué escucha el puente

**H-M3-01 [M].** El único suscriptor es `BRIDGE` (`TW/WorkspaceBridgeSourceGuardTests.cs:102-123` exige un único archivo suscriptor y cada fuente gobernada enganchada una sola vez).
- **Las fuentes gobernadas** (`TW/WorkspaceBridgeSourceGuardTests.cs:79-84`; `src/RackCad.Application/Workspace/BridgeSource.cs:11-36` y `:50-64`):
  - de la colección de documentos: `DocumentCreated`, `DocumentActivated` y `DocumentToBeDestroyed`;
  - por documento con sesión: `BeginDocumentClose`, `ImpliedSelectionChanged`, `CommandWillStart`, `CommandEnded`, `CommandCancelled`, `CommandFailed` y `EnteringQuiescentState` (en el `Editor`);
  - de la base de cada documento: `ObjectAppended`, `ObjectErased`, `ObjectModified`, `ObjectUnappended` y `ObjectReappended`;
  - `Idle`, solo mientras alguna sesión tiene pistas pendientes (`EBC:517-536`).
- **Ningún manejador lee sus argumentos:**
  - los de objeto y de comando llaman `_core.OnSourceEvent(a.Session, a.Source)` (`BRIDGE:198-260`; por ejemplo `ObjectModified` en `BRIDGE:240-246` y `CommandEnded` en `BRIDGE:198-204`);
  - no leen `e.DBObject`, `e.GlobalCommandName` ni ningún handle.

### 1.2 Cómo se atribuyen las pistas

**H-M3-02 [M].** La identidad que viaja es **solo la sesión** de la conexión (`a.Session`, `src/RackCad.Application/Workspace/BridgeSource.cs:81`). La firma del núcleo es `OnSourceEvent(SessionId session, BridgeSource source)` (`EBC:306`). Confirma el `H-08` de F4 (`F4P:27`): «las pistas no llevan rack ni handle».

**H-M3-03 [M]. La traducción a pistas** (`EBC:315-338`):
- `CommandWillStart` y `EnteringQuiescentState` solo cambian el reposo en memoria (`EBC:317-320`, `:437-442`);
- `CommandEnded`, `CommandCancelled` y `CommandFailed` dan `HintKind.CommandEnded` (`EBC:324-328`);
- las cinco fuentes de objeto dan `HintKind.DatabaseObjectChanged` (`EBC:329-335`);
- `ImpliedSelectionChanged` da `HintKind.ImpliedSelectionChanged` (`EBC:321-323`).

**H-M3-04 [M]. La coalescencia es por tipo y por sesión:** `HashSet<HintKind> _pending` (`src/RackCad.Application/Workspace/WorkspaceSession.cs:25-26`). N pistas de un tipo son una sola marca, y una pista propia es indistinguible de una ajena del mismo tipo ya pendiente.

### 1.3 El drenaje en Idle

**H-M3-05 [M]. Cuándo drena:**
- solo la sesión **activa**, en reposo, sin modal RackCad y con el panel visible (`EBC:499-515`; la condición de reposo está en `:509`);
- `TryDrain` vacía `_pending` entero en cada drenaje (`WorkspaceSession.cs:40-59`, `:57`);
- el callback de `Idle` solo programa una continuación con `BeginInvoke` a prioridad `Background` (`BRIDGE:42-44`, `:261-276`; `src/RackCad.Application/Workspace/WorkspaceIdleScheduler.cs:60-108`);
- la continuación hace la pasada y la comprobación Debug de `TopTransaction` (`BRIDGE:280-302`);
- `CanDrainNow` repite la elegibilidad y es falso con un cierre permitido pendiente (`EBC:413-425`).

**H-M3-06 [M]. Qué produce cada pista:**
- `DatabaseObjectChanged` o `CommandEnded` producen `InvalidateNavigationCaches` y `MarkDraftsPossiblyStale` (`WorkspaceSession.cs:52-56`);
- `ImpliedSelectionChanged` produce `ReadImplicitSelection` y `RecomputeSelectionContext` (`:47-51`);
- el conjunto de `DrainOperation` está fijado exacto en cuatro valores, y cada `HintKind` debe dar operaciones no vacías (`TW/WorkspaceHintDrainTests.cs:93-112`);
- las pistas de objeto dan exactamente `[Invalidate, Mark]` (`TW/WorkspaceBridgeHintAndDrainTests.cs:106-117`).

### 1.4 `MarkDraftsPossiblyStale`

**H-M3-07 [M]. Dónde se marca:**
- en `CORE:172-176`, **antes** de los retornos tempranos de `OnDrained`, se quita la marca «conocido limpio» y, si el drenaje lleva la operación, se llama a `MarkPilotDraftsPossiblyStale(outcome.Session)`;
- este método marca **todos** los borradores del piloto de la sesión (`CORE:1462-1466`);
- la forma y el orden los fija `TW/WorkspaceLifecycleGuardTests.cs:560-575` y `:578-586`.

**H-M3-08 [M]. `DraftSession.MarkPossiblyStale` no tiene guarda de estado** (`DS:408-414`): marca también un borrador en Applying.
- La marca solo la limpian `Rebase` (`DS:489-500`, `:493`), y por tanto el `Applied` de POST (`DS:694`), y `ConfirmBase` con MATCH en Clean o Dirty (`DS:754`).
- Un Clean marcado tiene `NeedsLazyReload` (`DS:306`): la caja queda de solo lectura (`FVM:579-586`) y «Editar» relee con `TryReload` (`FVM:296-299`).

### 1.5 Lo que hizo T2 con «la parte de marcado de C-F4-04»

**H-M3-09 [M].** F4-E26 asignó a F4-T2B2 «la parte de marcado de C-F4-04» (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:476`). La cumplió D-T2B2-11 (`docs/initiatives/I-64-f4-t2b2-gate-contract-package.md:305-313`), que escribió IC-08:
- **el marcado:** grueso por sesión, «porque las pistas no llevan rack» (`IC:136-137`); es solo una pista, sin Conflicto y sin lectura (`IC:139-141`);
- **el origen de la pista:** cualquier cambio de base de datos y cualquier `CommandEnded`, también de comandos que no modifican nada (`IC:143`);
- **la obligación** (`IC:145-151`): «Las pistas que produce la mutación propia de Actualizar no marcan los borradores». El mecanismo debe respetar la agrupación por sesión y por tipo y que «se drenan después del ámbito», y no puede suprimir `InvalidateNavigationCaches`. El oráculo es INV-28.

T4B no fijó el mecanismo: lo pasó a T4C (`T4BP:59`, `T4BP:254`, `T4BP:818`).

### 1.6 Qué haría hoy un Actualizar sin ámbito

**H-M3-10 [I, a partir de H-M3-03 a H-M3-08].**
- **La secuencia:**
  1. Las escrituras de MUTATE y POST, la importación de biblioteca y el `CommandEnded` del comando interno encolan `DatabaseObjectChanged` y `CommandEnded` en la sesión.
  2. Al volver al reposo, el drenaje produce `MarkDraftsPossiblyStale`.
  3. `MarkPilotDraftsPossiblyStale` marca el borrador recién rebasado del rack A y todos los B de la sesión.
- **Contra qué va:**
  - contra INV-08 (`FRZ:746`): las pistas propias se tratan como externas;
  - contra INV-28 (`FRZ:766`) y contra el texto de D-04 «Un Actualizar del rack A no marca los borradores de otros racks» (`FRZ:242-243`);
  - y contra el oráculo de IC-08 (`IC:151`).
- **Lo que no rompe:** la comparación de B. La StaleBase de B solo guarda entradas de sus miembros («what is not a member does not appear», `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:58-63`), así que una comprobación de B daría MATCH. La violación es de marcado, no de conflicto.

### 1.7 ¿Existe algún concepto de ámbito o de supresión?

**H-M3-11 [M]. No.**
- `git grep` de `OwnMutation`, «own mutation», `suppress` y «mutación propia» en `src/` no da nada del Workspace.
- No hay ningún miembro en `WorkspaceSession`, `EBC`, `CORE` ni `BRIDGE` que abra o cierre una ventana.
- El `H-17` de T4 lo confirma (`T4P:160`).

**H-M3-12 [M]. El único precedente de «efecto propio no cuenta» es `NavigationEcho`** (`src/RackCad.Application/Workspace/NavigationEcho.cs:7-96`):
- funciona por contenido y por sesión: compara como conjunto los handles escritos con los leídos en el drenaje;
- declara expresamente «no token of the next event, no flag, no time window and no timer» (`:12-13`);
- se consume en `CORE:211`.

Lo vive el núcleo del host, no la sesión. La sesión tiene prohibido llevar eco o handles (`TW/WorkspaceNavigationEchoTests.cs:325-340`).

### 1.8 Las ventanas que piden las autoridades (no coinciden)

**H-M3-13 [M]. Las ventanas:**

| Autoridad | Ventana | Ancla |
|---|---|---|
| Freeze, D-04 (O-15) | «se indexa por (sesión, miembros del rack) y cubre **desde PREPARE hasta la relectura de POST**» | `FRZ:241-243` |
| INV-08 | «ámbito por (sesión, miembros) **de PREPARE a POST**» | `FRZ:746` |
| F4, C-F4-04 | «El `CommandEnded` del propio Actualizar llega **después** de la relectura de POST». Opciones: «se cierra con el **fin del propio comando**, no tras POST», o la pista con handle. «Se registra como aclaración IC o A-n» | `F4P:60` |
| IC-08 | «se drenan **después** del ámbito» y no se puede suprimir `InvalidateNavigationCaches` | `IC:148-150` |
| T4, R-T4-08 | «activo **solo en la ventana de mutación** y liberado en toda salida; no oculta otro rack; se fija una de las dos vías de C-F4-04, con una IC o con una A-n» | `T4P:562-566` |
| Orden del Owner para T4C | «session-bound; rack/member-bound; **active only around the actual authored operation**; released on success / exception / cancellation; unable to mask another rack» | orden de preparación de T4C, apartado OWN-MUTATION |

**[I].**
- La ventana del Freeze (hasta la relectura de POST) deja fuera, por construcción, el `CommandEnded` del comando interno, si `F4P:60` es cierto.
- La de C-F4-04 es más ancha.
- La del Owner («only around the actual authored operation») es más estrecha que ambas.
- Hace falta decidir y registrar los bordes (IC o A-n, `T4P:566`).

### 1.9 Vía A: un ámbito indexado por (sesión, miembros) que se cierra con el fin del propio comando

**Qué haría falta [I]:**
1. **Registrar las pistas propias aparte, no suprimirlas.**
   - Como `_pending` coalesce por tipo (H-M3-04), una bandera «suprimir el marcado en el próximo drenaje» también se tragaría:
     - las pistas externas **ya pendientes** al abrir la ventana;
     - las que lleguen **después** del fin del comando y antes del drenaje.
   - Ejemplos: un `U`/`UNDO` inmediato (`FRZ:237-238`), o una edición de B con un comando.
   - Hace falta un segundo conjunto de pistas «propias» en la sesión, o una instantánea de lo pendiente al abrir. Al drenar:
     - las propias dan `InvalidateNavigationCaches` (obligatorio por IC-08), y también la selección si hubo `ImpliedSelectionChanged`;
     - no dan `MarkDraftsPossiblyStale`, salvo que también haya pistas externas.
2. **Los bordes.**
   - **Apertura.** O antes de `ExecuteInCommandContextAsync`, en la operación del puerto, para cubrir el `*Cancel*` y el `CommandWillStart` del comando interno, o dentro del callback, bajo el bloqueo y antes de `TryUpdatePilot`, para cubrir la importación de `PrepareUnits`, que es parte de PREPARE (`T4BP:325`).
   - **Cierre.** Con el `CommandEnded`, `CommandCancelled` o `CommandFailed` del comando interno, con respaldo en `EnteringQuiescentState`. Nunca al final del callback, por `F4P:60`, ni en el drenaje, por IC-08.
3. **Reconocer «el propio comando».**
   - Los manejadores no leen `e.GlobalCommandName` (H-M3-01). O se lee en el manejador (es un dato en memoria del argumento, no del DWG, pero cambia la línea del manejador y la firma de `OnSourceEvent`), o se infiere «el primer fin de comando de esa sesión tras abrir la ventana». La inferencia vale si la ventana se abre dentro del callback y no hay comandos anidados (**[H]**).
4. **Liberarlo en toda salida.**
   - En el éxito, el fin de comando cierra la ventana.
   - En una excepción del callback o de T4B (T4B es total, `T4BP:297`), en un rechazo antes de la ranura, y si la tarea de `ExecuteInCommandContextAsync` se cancela, falla o nunca ejecuta el callback, hace falta un cierre explícito del adaptador de T4C en su `finally` o en la continuación de la tarea.
   - Sin él, la ventana podría quedar abierta y tragar pistas ajenas.
5. **Que no oculte otro rack.**
   - Con pistas sin rack y marcado grueso, la ventana es de sesión en la práctica, aunque se indexe por miembros. Durante ella, una escritura ajena sobre B quedaría sin marcar.
   - Las únicas escrituras posibles mientras el comando interno tiene el bloqueo son los reactores de otros complementos (D-12 H-LOCK los deja en `UNKNOWN`, `FRZ:494-498`) y AutoCAD mismo.
   - Es un **residual que habría que declarar**. Marcar de forma conservadora a los B durante la ventana violaría INV-28 (`FRZ:242-243`, `:766`).

**Pros:**
- coste constante;
- ninguna lectura en los manejadores;
- casa con IC-08 y con la sesión sin handles;
- se prueba en Core con la fuente simulada (`TW/WorkspaceBridgeHintAndDrainTests.cs`).

**Contras:**
- «rack/member-bound» queda nominal;
- el residual de los reactores ajenos;
- depende del orden real de los eventos (**[H]**, §1.11).

**Guardas que tocaría [M]:**
- la superficie pública exacta de `EventBridgeCore` (`TW/WorkspaceBridgeFacadeSourceGuardTests.cs:204-224`): un miembro nuevo del núcleo es un cambio declarado;
- `DrainOnlyRunsTheOperationsAllowedInF1` (`TW/WorkspaceHintDrainTests.cs:93-112`), si se añade un `HintKind` o un `DrainOperation`;
- `ObjectChangesDrainOnlyTheCacheInvalidationAndTheStaleMarking` (`TW/WorkspaceBridgeHintAndDrainTests.cs:106-117`), si cambia la salida;
- la forma del drenaje (`TW/WorkspaceLifecycleGuardTests.cs:560-586`), si el host decide el marcado;
- las líneas de los manejadores (`TW/WorkspaceIdleSchedulingGuardTests.cs:64-104`; `TW/WorkspaceBridgeSourceGuardTests.cs:182-189`), si se lee el nombre del comando;
- los nombres de tipo prohibidos en `RackCad.Application.Workspace`: `Draft`, `Editor`, `Subscription` y `Tab`, y en propiedades de `WorkspaceSession` también `Handler` (`TW/WorkspaceModelBoundaryTests.cs:93-106`).

### 1.10 Vía B: la pista de objeto lleva el handle para atribuirla a miembros

**Qué haría falta [I]:**
1. **Leer el objeto en el manejador.** Los manejadores tendrían que leer `e.DBObject.ObjectId.Handle` y, para las entidades dentro del BTR de una vista, subir por `OwnerId`, y en los Xrecord del diccionario de extensión, dos niveles. Así se llega a la clave del miembro, que es el handle de la definición (`src/RackCad.Plugin/Views/RackSiblingScan.cs:137`).
2. **Transportar conjuntos de handles** de la pista a la sesión y al drenaje, y marcar solo los borradores cuyos miembros intersecten.

**Choques medidos [M]:**
- **El Freeze.** «Filtran por tipo con coste constante y anotan una pista con la identidad de la sesión» (`FRZ:217-218`). Un conjunto de handles no es de coste constante.
- **IC-04.** «Los manejadores de notificación … no pueden leer el DWG: solo encolan o agrupan pistas» (`IC:77`). D-04: «No leen ni escriben la base de datos» (`FRZ:217`). Subir por `OwnerId` es leer el modelo del objeto abierto. **[I]:** el handle del propio objeto es un dato de identidad en memoria, en la frontera.
- **La guarda de la sesión.** `TW/WorkspaceNavigationEchoTests.cs:325-340` prohíbe `[Hh]andle` en `WorkspaceSession.cs`: es un cambio declarado.
- **Las escrituras propias tocan objetos que no son miembros:**
  - la importación de biblioteca de `PrepareUnits` (el residuo OM-5, `T4BP:325`);
  - la purga, que borra BTR de basura (`T4BP:220-227`);
  - las definiciones anidadas `_<Rol>_<n>` (`T4BP:413`);
  - y el diccionario y el Xrecord de `RackBlockData`.

  Atribuir por miembros dejaría todas esas pistas como «externas» y volvería a marcar la sesión entera. **[I]:** la vía B necesita además saber qué objetos tocó la corrida, y eso solo se sabe leyendo.

**Pros:** es la única forma de no ocultar un cambio ajeno de B durante la ventana.

**Contras:** los choques de arriba. Además cambia la granularidad del marcado, que hoy es grueso por sesión (IC-08), lo que exigiría una IC o una A-n nuevas.

### 1.11 Lo que solo el host puede probar (mutación propia)

**[H]:**
- **El orden real** entre el `CommandWillStart` de `EXECUTEFUNCTION`, el callback, los `ObjectModified` y `ObjectAppended` de la importación, del `Seal` (`Commit`) y de la purga y `SyncName` de POST, la relectura de POST (`CaptureDrawing`), el `CommandEnded`, `CommandCancelled` o `CommandFailed` y el `EnteringQuiescentState`.
  - Hay que medir si `F4P:60` («el `CommandEnded` llega después de la relectura de POST») se cumple.
  - Y si las notificaciones de objetos abiertos en transacción llegan al confirmar o al modificar.
- Si el `*Cancel*` previo produce `CommandCancelled` y `ImpliedSelectionChanged` **antes** de que empiece `EXECUTEFUNCTION`, fuera de una ventana abierta en el callback.
- Si `EnteringQuiescentState` llega siempre **después** del `CommandEnded` propio (si no, un drenaje podría correr entre los dos).
- Si en algún momento del callback AutoCAD bombea el despachador de WPF, de modo que una continuación de drenaje en `Background` pueda correr dentro.
- La frecuencia del marcado y el coste de las recargas (`IC:153`).
- Que tras Actualizar A el borrador B no queda marcado, y que un `U` inmediato sí marca. Es un objetivo mayor de Smoke-2 en la orden.

---

## 2. El ciclo de vida con un Applying (R-T4-07)

### 2.1 Los descartes sobre un Applying, hoy

**H-M3-14 [M]. Las tres operaciones:**
- **`IWorkspacePilotEditPort.DiscardRack`** (`CORE:1156-1178`): autoriza; solo mira `_pilotReadInProgress` (`:1161`); llama `_drafts.DiscardByUser(session, rackId)` (`:1166`) y `NotePilotEdit` (`:1167`). **No mira si el borrador está en Applying.**
- **`IWorkspacePilotEditPort.DiscardDocument`** (`CORE:1180-1196`): igual, con `_pilotReadInProgress` (`:1185`), y delega en `DiscardPilotEdits` (`CORE:770-786`), que llama `DiscardAllByUser`.
- **`DraftRegistry.DiscardByUser`** (`DR:262-273`) y `DiscardAllByUser` (`DR:276-286`): no miran el estado. Quitan el borrador y llaman `Detach()`, que solo pone `_registered = false` (`DS:796-799`).
  - **Nunca liberan la ranura:** `ReleaseApplySlot` solo la llama `CompleteApply` (`DR:363-367`; comentario «discarding or retiring never frees it»).
  - Al cerrar, `CompleteApply` devuelve `ClosedDetached` (`DS:541-546`) y el host responde `EditUpdateOutcome.ClosedDetached()` (`CORE:869`), sin `NotePilotEdit`.

**H-M3-15 [M]. Ese comportamiento está caracterizado por pruebas:**
- `NoExplicitOperationFreesTheSingleApplyingSlotOfTheRegistry` (`TW/WorkspaceDraftRegistryContainmentTests.cs:383-411`) afirma para `DiscardByUser`, `DiscardAllByUser`, `RetireKnownSafe` y `RetireConservative` que el borrador en Applying queda **desregistrado** (`:407`) y que la ranura sigue ocupada (`:409-410`);
- `ADraftThatWasDiscardedDuringTheRunIsClosedDetachedAndTheSlotIsFree` (`TW/WorkspacePilotUpdateHostTests.cs:368-382`) descarta durante `WriteUnit` y da `ClosedDetached` **después de una escritura**.

**[I]:** si R-T4-07 se implementa en el registro, cambia la primera; con una marca del host, ninguna de las dos.

**H-M3-16 [M]. Los caminos de la UI que llegan a esos descartes:**
- `CloseTab`: un borrador no protegido llama `DiscardRack` sin preguntar (`BVM:286-300`); uno protegido pregunta (`BVM:280-284`);
- `DiscardField` (`BVM:313-350`);
- `RequestDocumentDiscard` y `ConfirmDiscard` (`BVM:358`, `:410`);
- `RACKPANELDESCARTAR`, por `WorkspaceDocumentDiscardCommand` (`src/RackCad.Application/Workspace/WorkspaceDocumentDiscardCommand.cs:146`; el comando está en `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:38-39`).

**[I]:**
- Un Applying siempre viene de un Dirty: `TryUpdatePilot` para NothingToApply antes de la ranura (`CORE:825-834`). Por eso `IsProtected` es verdadero (`CORE:1316`) y `CloseTab` y `DiscardField` preguntan.
- Con un callback síncrono, solo un bombeo anidado haría alcanzables esos caminos durante la corrida (**[H]**).

### 2.2 Las demás operaciones del puerto frente a un Applying

**H-M3-17 [M].**

| Operación | Ante un Applying | Ancla |
|---|---|---|
| `TryBeginEdit` | `AlreadyOpen` (el borrador existe) | `CORE:311` |
| `TryCheckNow` | `NotApplicable` (no es Unknown(CommitThrew)) | `CORE:391-392` |
| `TryReload` | `NotNeeded` (`NeedsLazyReload` exige Clean) | `CORE:470`, `DS:306` |
| `TryConfirmBase` | `NotApplicable` | `CORE:552-554` |
| `TryKeepMine` | `Waiting` («El borrador se está aplicando…») | `CORE:634` |
| `TryDiscardAndReload` | `Waiting` | `CORE:717` |
| `SetPendingText` y `CommitField` | `NotEditable` (`CanEdit` falso en Applying) | `CORE:1056`, `:1082`, `:1324-1325`; `DS:331-332` |
| `CrossBoundary`, bloqueante | `Proceeded` sin pendiente; `Kept` con pendiente | `DS:383-386` |
| `GetRacksAtRisk` | el rack, si es Dirty | `CORE:1199-1241`; `DR:192-195` |
| `DiscardRack` y `DiscardDocument` | **lo desregistran** | H-M3-14 |

**H-M3-18 [M]. `EnterError` durante la corrida** queda diferido y se aplica tras la clasificación (`DS:773-788`, `:550-559`). No tiene llamador de producto (`git grep EnterError -- src`: solo la definición). `DraftRegistry.HasUpdateInProgress` (`DR:198`) tampoco tiene llamador de producto.

### 2.3 CLOSE, CLOSEALL, QUIT y el veto

**H-M3-19 [M]. La cadena del veto:**
1. `BeginDocumentClose` está enganchado por sesión durante toda su vida (`BRIDGE:177-183`; `EBC:357-375`).
2. El asesor es el relevo (`src/RackCad.Application/Workspace/WorkspaceCloseAdvisorRelay.cs:58-72`) sobre `IWorkspaceLifecycleAdvisor.AdviseClose` del host (`CORE:1378-1400`).
3. El host usa `DraftClosePolicy.EvaluateClose` (`src/RackCad.Application/Workspace/Drafts/DraftClosePolicy.cs:44-50`), que veta si hay un borrador Dirty o con texto pendiente (`DR:192-195`).

**[I]:** un Applying viene de un Dirty, su `IsDirty` sigue siendo verdadero y **se veta**.
- Tras Applied, el borrador es Clean y el cierre procede.
- Tras NotApplied, CommitThrew o Unknown(PostReread), sigue Dirty (no se rebasa) y se veta.

**[H]:**
- si puede llegar un `BeginDocumentClose` mientras el callback corre (bombeo anidado), o mientras `ExecuteInCommandContextAsync` está encolado;
- si `CLOSEALL` o `QUIT` encolados descartan el callback pendiente;
- el orden respecto del aviso de guardar (`FRZ:615`, `INFERENCE`).

### 2.4 La retirada de sesión durante una actualización

**H-M3-20 [M]. La retirada:**
- `OnDocumentToBeDestroyed` retira la sesión y avisa al asesor (`EBC:268-296`);
- `SessionDestroyed` (`CORE:1415-1439`) llama `RetireKnownSafe` o `RetireConservative` (`DR:292-352`). Ninguna de las dos mira Applying: desregistran y no liberan la ranura (H-M3-15);
- la corrida termina igual en `CompleteApply` y responde `ClosedDetached`;
- `Authorize` contra una sesión destruida da `RejectedSessionDestroyed` (`src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs:39-46`), y la puerta de `TryUpdatePilot` la rechaza **antes** de la ranura (`CORE:815-816`, `:925-929`).

**[I]:** si el documento se destruye con el callback encolado y sin empezar, `TryBeginApply` no llega a ejecutarse y no hay fuga de la ranura. Una marca del host de «actualización pedida», puesta antes del callback, sí podría fugarse (§5.2).

### 2.5 Dónde se llama hoy a `NotePilotEdit`

**H-M3-21 [M].**
- **Definición:** `CORE:793`, de forma exacta fijada por `TW/WorkspaceLifecycleGuardTests.cs:550-557`.
- **Llamadores:**
  - `TryUpdatePilot`: `CORE:874` (ClosedInError) y `:896` (Conflict, CommitThrew, Applied, AppliedWithWarnings y AppliedPostRereadFailed). Cada una va seguida de `_knownClean.Remove(request.Session)`, literal (`:875`, `:897`), aunque `NotePilotEdit` ya es ese `Remove`;
  - `SetPendingText`: `:1061`; `CommitField`: `:1088`; `CrossBoundary`: `:1139`; `DiscardRack`: `:1167`.
- **No lo llama en:** NotAppliedAdmission, NotAppliedPreflight y NotAppliedFailure (`:892-898`), ni en `ClosedDetached` (`:869`) o `!closed.Accepted` (`:870`).
- **Un comentario obsoleto:** `CORE:790-791` dice «Until the panel edits drafts (F4-T3, F4-T4) nothing calls it».
- **La guarda del mismo nombre:** `PortMutatingOperations` lleva `TryUpdatePilot` al final (`TW/WorkspaceLifecycleGuardTests.cs:463-477`), y `ChangesADraft` reconoce `TryBeginApply` y `CompleteApply` (`:480`).
- **Estado de R-T4-07:** la parte de «Actualizar llama a `NotePilotEdit`» está hecha por T4B (`T4BP:253`). Queda la parte de los descartes sobre Applying.

### 2.6 IC-04 y su tiempo

**H-M3-22 [M].** La redacción aceptada de IC-04 (`IC:73-85`):
- **los manejadores** solo encolan;
- **la lectura de resumen** solo después del drenaje, como operación aparte;
- **nunca** dentro de `ObjectModified`, `ObjectErased`, `ImpliedSelectionChanged` ni `CommandEnded`, ni de un callback de Idle o de evento «simplemente porque se disparó» (`IC:81`).

**Cómo se cumple hoy** (F4-T2C):
- el callback de Idle solo programa (`BRIDGE:263`, `:273-276`);
- la pasada y las lecturas corren en la continuación (`BRIDGE:280-284`);
- el resumen se lee solo en la ventana del aviso del drenaje (`CTRL:236-244`; `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:117-118`).

**[I]:**
- Las lecturas de la corrida de Actualizar (`ReadForPrepare`, `Reread` y la relectura de POST) no son lecturas de manejador: las autoriza R-T4-06 dentro del contexto de comando y sin puerta de reposo (`T4P:552-553`).
- Después de un Actualizar, el drenaje de sus pistas lee la selección, por el `*Cancel*`, y puede disparar la lectura de un resumen. Las dos deben ocurrir en la continuación, **después** del `CommandEnded` y del regreso del callback de Idle.

**[H]:** la temporización de pila en el host (D-T2C-05, `docs/initiatives/I-64-f4-t2c-gate-contract-package.md:204-218`, `:266`) y la latencia de `Background` tras Actualizar. Es el «IC-04 timing» de Smoke-2.

### 2.7 Opciones para lo que queda de R-T4-07

| Opción | Qué hace | Pros | Contras |
|---|---|---|---|
| (a) Marca del host | `DiscardRack` y `DiscardDocument` responden `Busy` (o `Waiting`) si `_drafts.HasUpdateInProgress` o si el borrador está en Applying, como `_pilotReadInProgress` (R-7 de `docs/automation/evidence/I-64-pilot/F4-T4A1-PREPARE-CORE/coordinator/map-3-host-seams-guards.md:365`) | No cambia `DR` ni H-M3-15; se prueba en Core | Textos nuevos (la tabla de literales del núcleo); `HostViolations` exige `Authorize` y `_pilotReadInProgress` en estas operaciones (`TW/WorkspacePilotEditGuardTests.cs:262-268`), que es una adición compatible |
| (b) En el registro | `DiscardByUser` y `DiscardAllByUser` no tocan un Applying | Cubre también otros llamadores | Cambia la prueba caracterizada `TW/WorkspaceDraftRegistryContainmentTests.cs:383-411`; la retirada por destrucción no se puede negar |
| (c) Las dos | — | — | La suma de los contras |

La retirada por destrucción (`RetireKnownSafe` y `RetireConservative`) no puede negarse. R-T4-07 no la nombra (`T4P:560`).

---

## 3. La UI de Actualizar (R-T4-09)

### 3.1 ¿Existe ya un botón o un gesto?

**H-M3-23 [M]. No.**
- `git grep -i actualizar` en `src/RackCad.UI/Workspace` no da nada. El único «actualizar» del Workspace son dos motivos de `UpdateGate` en `CORE:915` y `:921`.
- **El VM del campo (`FVM`) tiene estos gestos:** `BeginEdit` (`:287`), `CommitPending` (`:338`), `Rematerialize` (`:376`), `CheckNow` (`:397`), `RequestDiscardAndReload` (`:446`), `RequestKeepMine` (`:449`), `ConfirmChoice` (`:455`) y `CancelChoice` (`:505`).
- **El XAML del campo tiene estos botones:** Editar, Recargar, Descartar, Comprobar ahora, Descartar y recargar, Conservar mi borrador, Sí y No (`src/RackCad.UI/Workspace/WorkspacePanelView.xaml:193-265`).
- **El código subyacente tiene un manejador por botón,** todos dentro de `Contained` (`src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs:83-111`, `:197-207`).
- **`IWorkspacePilotEditPort`** tiene 15 operaciones (`src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:26-102`) y ninguna de actualización.

**H-M3-24 [M]. Una guarda prohíbe hoy cualquier método con «Update» en el nombre del puerto:** `Assert.DoesNotContain(typeof(IWorkspacePilotEditPort).GetMethods(), m => m.Name.Contains("Update", …))` (`TW/WorkspaceEditIntentHostTests.cs:1012`). Añadir la operación es un cambio declarado.

**H-M3-25 [M]. Los implementadores del puerto son seis tipos,** no cinco como dice `T4BP:114-118`, porque falta el propio núcleo:
- el núcleo (`CORE:38`);
- la compuerta (`src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:153`);
- el adaptador del controlador (`CTRL:278`);
- `CommandAdvisorPortDouble` y `ForwardingRiskPort` (`TW/WorkspaceCommandAdvisorTests.cs:17`, `:1107`);
- el doble de la UI (`UW/WorkspacePilotEditPortDouble.cs:25`).

### 3.2 La frontera `PilotBoundaryKind.Update`

**H-M3-26 [M]. La frontera:**
- **en el puerto no existe:** `PilotBoundaryKind` es `TabChange`, `RackChange`, `DocumentChange`, `PanelHide`, `Dematerialize` y `DocumentClose` (`src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:166-175`), un prefijo ordenado que admite valores al final (`TW/WorkspacePilotEditGuardTests.cs:79-84`);
- **en el núcleo sí existe:** `DraftBoundary.Update`, que es bloqueante (`DS:76-92`);
- **en el host:** `CrossBoundary` solo trata `TabChange` y `RackChange` como bloqueantes, y un valor desconocido da `Aborted` («La frontera no se conoce.», `CORE:1110-1124`);
- **en T4A1:** para PendingText, «antes se cruza la frontera Update» (`T4P:401`). La comprobación previa de `TryUpdatePilot` responde `PendingText` con `PendingTextReason` («Hay texto pendiente: confírmalo antes de actualizar.», `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs:57`, y `CORE:831`).

**H-M3-27 [M].** Al salir de la caja, `OnBrowserFieldBoxLostFocus` llama `CommitPending` (`src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs:170`), que cruza la frontera `TabChange` (`CORE:1084`). **[I]:** el clic en un botón enfocable mueve el foco antes del `Click`, así que el texto pendiente válido se confirma por esa vía antes de Actualizar, y el inválido queda con su mensaje. Si el botón no toma el foco, eso no ocurre. Es una pregunta de UI (Q-M3-08).

### 3.3 La presentación de Applying y de los avisos

**H-M3-28 [M]. Applying:**
- la vista de Applying da `PilotFieldState.Applying` (`CORE:1367`);
- el VM lo nombra «Aplicando» (`FVM:857`);
- en Applying no se ofrece «Comprobar ahora» (`FVM:592-607`), no es editable (`CanEdit`, `CORE:1324-1325`) y no se ofrece «Editar» (`FVM:817-818`).

**[I]:** Applying solo existe entre `TryBeginApply` y `CompleteApply`, dentro del callback (`CORE:837`, `:863-866`). Si el gesto es síncrono, la UI de WPF no repinta durante la corrida, y «Aplicando» puede no verse nunca. El Freeze pide que la pestaña lo muestre (`FRZ:545`). **[H]**.

**H-M3-29 [M]. Los avisos no viajan en la vista del campo:**
- `PilotFieldView` no tiene `Warnings` (`src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:197-262`);
- `ViewOfPilotDraft` no los copia (`CORE:1355-1357`);
- la cola del constructor está fijada exacta: `errorReason`, `unknownOrigin`, `conflictText` y `lastCause`, y añadir un parámetro hace saltar la guarda (`TW/WorkspaceHostHardeningGuardTests.cs:363-406`, autoprueba `:399`);
- los avisos están en `draft.Warnings` (`DS:321`, `:697`) y en `EditUpdateOutcome.Warnings` (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:451-452`).

**[I]:** o el VM los guarda como aviso del gesto, a partir del resultado, o se amplía la vista con un cambio declarado. El Freeze pide «avisos visibles en el panel, no solo en el registro» (`FRZ:528`; INV-31, `FRZ:769`).

**H-M3-30 [M]. Los textos que ya existen para el Desconocido:** «Estado desconocido; comprueba.» (A2-11, `FVM:56`) y «Aplicado; recarga para seguir editando.» (A2-12, `FVM:57`). Los usa `ReconcileText` (`FVM:777-795`). T4A2 dejó «Aplicado con avisos» y Applying para la UI de Actualizar (`A2P:359`, `:660`).

**H-M3-31 [M]. M-03 de T4B:** cualquier `Status == Error` tras la corrida responde `ClosedInError`, sin la clasificación, los avisos ni las diferencias (`CORE:872-877`). Pasa a T4C como «mostrar la clasificación de un ClosedInError» (`docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/task-close-package.md:119`, `:131`).

### 3.4 La «selección implícita que cancela `EXECUTEFUNCTION`»

**H-M3-32 [M]. Lo que midió Smoke-NAV:**
- **en la traza:** `ExecuteInCommandContextAsync` ejecuta el callback en el comando interno `EXECUTEFUNCTION` (`docs/automation/evidence/I-64-pilot/F2-SMOKE-NAV/R20261110T064058Z-c883/logs/diag-all-racklog-raw.txt:2`, «callback enter isAppCtx=False cmdNames=EXECUTEFUNCTION»);
- **en el diagnóstico:** dos `*Cancel*` vacían la selección implícita (`docs/automation/evidence/I-64-evidence.md:5912`; `docs/automation/evidence/I-64-overnight/overnight-dispositions.md:74`; `docs/automation/evidence/I-64-pilot/F2-T2-HOST-C1/delta-close-package.md:7`);
- **lo que retiró IC-01:** solo la navegación (`IC:17`). Actualizar conserva el contexto de comando (`T4P:547-550`, orden del Owner de T4C).

**[I], su efecto en el panel:**
1. El vaciado encola `ImpliedSelectionChanged`. El drenaje lee la selección (`CORE:197-215`) y da un contexto sin rack.
2. `BrowserSession.ApplySelectionContext` no cambia nada ante un contexto sin `RackId` (`src/RackCad.Application/Workspace/BrowserSession.cs:88-90`): la pestaña fijada sigue activa.
3. Se pierde la selección del usuario, y la navegación ve «sin selección».
4. Actualizar no usa la selección: `TryUpdatePilot` no nombra `ChosenReference` ni `Witness` (`TW/WorkspaceEditIntentGuardTests.cs:1098-1111`).

**[H]:** si los `*Cancel*` producen además `CommandCancelled`; su orden respecto de `CommandWillStart`; y si cancelan algo más (pinzamientos o un comando transparente).

### 3.5 K = 1 tras POST

**H-M3-33 [M]. La superficie única:**
- `BrowserSession.MaxLiveSurfaces = 1` (`src/RackCad.Application/Workspace/BrowserSession.cs:49`; `BVM:155`);
- el campo se pinta desde la memoria del puerto (`FVM:557-577`) y sigue a la superficie viva (`BVM:801-814`);
- el drenaje solo invalida la caché de resúmenes (`src/RackCad.Application/Workspace/BrowserRackSummaries.cs:85-98`): una superficie Ready no se vuelve a leer (residual R-T2C-01, `f4-dispositions.md:919`).

**[I]:**
- Tras POST, `Classify(Applied)` rebasa el borrador (`DS:679-699`), y una sola superficie, la activa, se repinta desde la memoria.
- El riesgo de K = 1 es el cambio de pestaña por la selección: con la selección vaciada no ocurre (§3.4). Con una selección de otro rack hecha durante la corrida, solo la retiene el texto pendiente (`BVM:697-711`), que no existe en Applying.
- Hace falta la comprobación `IsMine` del gesto, como en `CheckNow` (`FVM:412-416`), por si la superficie cambia durante la corrida.

### 3.6 Las guardas de UI que acotan esa superficie

**H-M3-34 [M].**

| Guarda | Qué fija hoy | Efecto para Actualizar |
|---|---|---|
| `TW/WorkspacePilotCloseSyncGuardTests.cs:844-867` (`:849`) | En el VM del navegador, ninguno de `TryBeginEdit`, `TryReload` y `TryCheckNow`. En el navegador y en el código subyacente, `DrawingReach` (`Transaction`, `Database`, `.Commit(`, `SendStringToExecute`, `CommandMethod`, `ExecuteInCommandContextAsync` y `Autodesk`), `FocusReach` y `DialogReach` | La operación nueva no está en `IntentionReads`: hace falta añadirla. El código subyacente no puede nombrar AutoCAD ni enfocar |
| `TW/WorkspacePilotSurfaceGuardTests.cs:278-292` | En `src/RackCad.UI/Workspace`, sin `Task`, `async`, `await`, `Dispatcher` ni `Timer`, salvo el VM de navegación | El gesto de Actualizar del campo tiene que ser síncrono |
| `TW/WorkspacePilotSurfaceGuardTests.cs:54-92` | Las presentaciones (`Present`, `Refresh`, `Show`, `Clear`, …), los constructores y los *setters* no nombran las tres lecturas | Cota inferior: la operación nueva necesita su regla |
| `TW/WorkspacePilotReconciliationGuardTests.cs:62-101` (regla 1) | Las cuatro lecturas de la reconciliación, solo en su gesto del VM del campo | Cota inferior: no ve una quinta |
| `TW/WorkspacePilotReconciliationGuardTests.cs:155-195` (regla 2) | Los cinco gestos, cada uno con su manejador exacto | Ídem |
| `TW/WorkspacePilotReconciliationGuardTests.cs:256-...` (regla 3) | Los cinco botones del XAML, con nombre, texto, `AutomationProperties`, `Click` y `Visibility` | Ídem |
| `TW/WorkspacePilotCloseSyncGuardTests.cs:438-451` | La forma pública del VM del navegador: **exactamente 33 firmas** | Un gesto en el navegador es un cambio declarado; uno en el VM del campo no lo es |
| `TW/WorkspacePilotEditGuardTests.cs:44-62`, `:64` | 15 operaciones, cota inferior; seis lecturas | Hay que declarar la nueva en la lista que corresponda (§3.7) |

### 3.7 ¿Por dónde llegaría el clic de Actualizar al Plugin?

**H-M3-35 [M]. La cadena de hoy para cualquier operación del puerto:**
1. el clic, con `Contained` (`src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs:197-207`);
2. el gesto del VM del campo;
3. `IWorkspacePilotEditPort`, que es el `PilotEditAdapter` del controlador (`CTRL:278-318`);
4. `WorkspacePilotEditGate` (`src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:153-204`);
5. la implementación explícita de `WorkspaceHostCore`.

**H-M3-36 [M]. Las restricciones de esa cadena:**
- **el adaptador del controlador solo reenvía**, literalmente, cada operación de `Operations` (`TW/WorkspacePilotEditGuardTests.cs:509-519`), y se construye exactamente sobre la compuerta (`:506-507`);
- **el controlador** no nombra `Task`, `async`, `Dispatcher` ni `Timer` (`:529`), tiene tres `+=` (`:526`), y no puede usar `LockDocument`, `StartTransaction` ni `.Commit(` (`TW/WorkspaceLifecycleGuardTests.cs:684-695`, que mira también `BRIDGE`);
- **la compuerta** responde `Waiting` dentro de un aviso para las seis lecturas, y para el resto **solo reenvía** (`TW/WorkspacePilotEditGuardTests.cs:427-456`);
- **en el núcleo**, las operaciones que no son lecturas no pueden nombrar `PassGate`, `_probe`, `_bridge` ni el lector (`TW/WorkspacePilotEditGuardTests.cs:248-253`);
- **el núcleo no puede tener campos de tipos de `RackCad.Application.Systems*`** (`TW/WorkspaceEditIntentHostTests.cs:1024-1043`), ni cambiar el constructor (`T4BP:112`);
- **`TryUpdatePilot`** es público, recibe el puerto como parámetro (`CORE:805`; `TW/WorkspaceEditIntentHostTests.cs:1004-1021`) y no puede contener `PassGate` (`TW/WorkspaceEditIntentGuardTests.cs:1098-1111`);
- **en todo `src`**, `ExecuteInCommandContextAsync` está prohibido (`TW/WorkspaceNavigationHostTests.cs:436`), y en `Plugin/Workspace` y `Application/Workspace` también `async` y `await` (`:430-435`; `TW/WorkspaceHostSourceGuardTests.cs:424-440`);
- **en el escritor** están prohibidos `async`, `await` y `ExecuteInCommandContextAsync` (`TW/WorkspacePilotUpdateGuardTests.cs:171`);
- **R-T4-05 pone el adaptador en un archivo nuevo** de `Plugin/Views` (`T4P:548`). No hay ninguna guarda de `async` sobre `Plugin/Views`.

**[I], consecuencia:** la implementación del núcleo no puede entrar en contexto de comando ni crear el escritor. El clic tiene que llegar al adaptador de `Plugin/Views` con el adaptador del controlador o con el núcleo como intermediarios. Esto exige un cambio de guarda declarado (`T4BP:815`).

**Opciones [I]:**

| Opción | Cómo | Pros | Contras |
|---|---|---|---|
| (a) Ejecutor asignado al núcleo | Una interfaz de `Application/Workspace` (o, como ya se tiene, de uno de los tres archivos que pueden nombrar el piloto, `TW/WorkspaceEditIntentGuardTests.cs:512-536`), asignada por *setter* como `SetEditIntentReader` (`CORE:275-276`). La implementación explícita del núcleo aplica la puerta de reposo y delega en el ejecutor; el ejecutor (`Plugin/Views`) entra en contexto, bloquea, crea el escritor y llama a `host.TryUpdatePilot(request, writer)` | Conserva el reenvío literal del controlador y la compuerta | Un campo nuevo del núcleo; si su firma nombra `IPushBackPilotUpdatePort`, choca con la guarda de campos (`:1024-1043`), salvo la opción B-2 (b) de la revisión de la v1 de T4B; un campo persistente en el núcleo puede leerse como «una autoridad persistente del host» (§9) |
| (b) Excepción en el controlador | El adaptador del controlador no reenvía esa operación a la compuerta, sino al adaptador de `Plugin/Views`, que llama a `TryUpdatePilot` | El núcleo no gana campos | Rompe el reenvío literal (`TW/WorkspacePilotEditGuardTests.cs:509-519`) y se salta la compuerta de avisos (hay que reproducirla) |
| (c) Envoltorio del puerto | El controlador expone un decorador de `Plugin/Views` sobre la compuerta | La compuerta sigue | Cambia `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));` (`TW/WorkspacePilotEditGuardTests.cs:506-507`) |

### 3.8 La puerta de reposo

**H-M3-37 [M]. Lo que dejó T4B:**
- `UpdateGate` solo mira el documento activo, el modal RackCad y `Authorize` (`CORE:908-933`); no mira el reposo ni el lector, y lo fija `TheGateHasNoRestAndNoReaderSoACommandInProgressAndAMissingReaderDoNotStopTheUpdate` (`TW/WorkspacePilotUpdateHostTests.cs:96-113`);
- «La puerta de reposo pasa a la operación del puerto de T4C, antes de entrar en el contexto de comando» (`T4BP:557`, `:845`);
- **la puerta de reposo existente** es `PassGate` (`CORE:1481-1515`): lector asignado, documento activo, sin modal, `_probe.IsActiveDocumentQuiescent` y `Authorize`;
- **el sondeo** `IsActiveDocumentQuiescent` combina `Editor.IsQuiescent` y `!IsCommandActiveInActiveSession`, que se apaga con `CommandWillStart` (`CTRL:367-374`; `EBC:84-89`).

**[I]:**
- Dentro de `EXECUTEFUNCTION`, `PassGate` respondería `Waiting` (`docs/automation/evidence/I-64-pilot/F4-T4A1-PREPARE-CORE/coordinator/map-3-host-seams-guards.md:353`). Por eso la puerta de reposo se evalúa antes de llamar a `ExecuteInCommandContextAsync`, en el contexto de aplicación del clic, y dentro solo queda `UpdateGate`.
- Una operación del puerto que use `PassGate` tiene que entrar en `ReadOperations` o en una lista nueva declarada (`TW/WorkspacePilotEditGuardTests.cs:64`, `:248-253`). Así también obtiene el `Waiting` de la compuerta dentro de un aviso.

**H-M3-38 [M]. I-01 de T4B:** los textos de `UpdateGate` (`CORE:915`, `:921`) y `UpdateNotRunReason` (`CORE:48`) pasan a constantes de `PushBackPilotUpdateRun`, y la guarda de textos se extiende a `UpdateGate` (`docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/task-close-package.md:91-97`). Es la superficie de la puerta que T4C toca.

### 3.9 Síncrono o diferido

**[H]:**
- si el callback corre antes de que vuelva `ExecuteInCommandContextAsync` cuando se llama desde un clic de la paleta. Es el primer paso de T4C (`T4P:550`).
- La traza de Smoke-NAV no lo distingue: aquella versión usaba `await` (`git show 7bdcbfd9:src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`, líneas 56-69), y los registros ponen «callback exit» antes de «ExecuteAsync end» (`.../diag-all-racklog-raw.txt:1-7`).
- Si es diferido, la cadena síncrona del puerto no puede devolver el resultado, porque `Task` y `async` están prohibidos en la UI, en el controlador y en las carpetas Workspace (H-M3-36). Haría falta un canal de finalización: es el riesgo R-2 de `.../map-3-host-seams-guards.md:350-354` y la A-n de `T4P:550`.

---

## 4. El diagnóstico diferido de T4A2 (§9 I-3 de T4B; `A2P` §8 y D-T4A2-06)

### 4.1 Qué es

**H-M3-39 [M]. El caso:**
- «Comprobar ahora» sobre un Unknown(CommitThrew) va por `TryCheckNow` (`FVM:409-419`) y llega a `TryCheckPilotEdit` (`CORE:368-431`);
- lee una vez (`:395`) y entrega `pilot.StaleBase` a `CheckNow` (`:413`);
- `CheckNow` decide `Recovered` o `Conflict` **solo** por la StaleBase (`DS:714-732`);
- el host responde `EditCheckOutcome.Recovered()` o `EditCheckOutcome.Conflict(differences)` (`CORE:414-424`) **sin el diagnóstico de admisión**, aunque la relectura no esté admitida;
- `EditCheckOutcome` no tiene campo para él (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:237-280`). En cambio, `EditConfirmOutcome` lleva `AdmissionDiagnostic` y `ClassicEditorHint` (`:320-374`, `:346-349`).

Es la limitación declarada de T4A2 (`A2P:149`, `:174`, `:672`): en la vía `TryCheckNow`, NOT_FOUND o NO_LONGER_EDITABLE se ven como Conflicto, y solo la elección posterior lo revela con NotAdmitted (A2-32). `A2P:178` la asignó a T4B, y T4B la pasó a T4C (`T4BP:255`, `:616`, `:837`).

**H-M3-40 [M]. La UI hoy:** `PresentCheck` **retira** el diagnóstico persistente ante `Recovered` o `Conflict` (`FVM:712-727`, `:714-718`, según D-T4A2-01 paso 5 y D-T4A2-06, `A2P:203`, `:347`). Si el rack ya no es editable, «Comprobar ahora» deja la superficie sin el diagnóstico y con A2-09 neutro (`FVM:48`, `:736`).

### 4.2 Qué falta

**[M], las piezas:**
- un portador del diagnóstico en `EditCheckOutcome`;
- su relleno en `TryCheckPilotEdit` a partir de la misma lectura;
- que `PresentCheck` lo guarde como diagnóstico persistente;
- los textos, o la reutilización de A2-03 y A2-06;
- y el respondedor del doble de la UI (`UW/WorkspacePilotEditPortDouble.cs`, `CheckNowResponder`).

**H-M3-41 [M]. Las guardas que lo impiden hoy** (cambios declarados obligatorios):
- `CheckBody`: cero `pilot.Read` (`TW/WorkspaceEditIntentGuardTests.cs:682-685`);
- M-3: la expresión del diagnóstico, exactamente una vez en todo el núcleo y cero en `CheckBody` (`:861-870`);
- las cuentas por operación: cero en las demás (`:688-700`);
- las formas de `pilot` en el núcleo (M-2, `:821-857`), y la tabla H-19 de `A2P:135-145`, que hoy tiene `pilot.ClassicEditorHint` con cero usos en la comprobación;
- `EditCheckStatus` no puede ganar valores (`TW/WorkspaceEditIntentHostTests.cs:1056`). Habría que añadir propiedades, como en `EditConfirmOutcome`, y no un estado;
- las reglas de UI de D-T4A2-07 (`A2P:361-388`) y las pruebas de `UW/WorkspacePilotReconciliationTests.cs` sobre `PresentCheck`.

### 4.3 Dónde se cablearía

**Opciones [I]:**

| Opción | Dónde | Pros | Contras |
|---|---|---|---|
| (a) Host y UI | `EditCheckOutcome` gana `AdmissionDiagnostic` y `ClassicEditorHint` opcionales; `TryCheckPilotEdit` los rellena como `TryConfirmPilotBase` (`CORE:576-586`); `PresentCheck` los guarda | Simétrico con D-T4A2-04; cierra la limitación | Rompe M-2 y M-3 (cambio declarado, `T4BP:837`) y cambia la regla de D-T4A2-06 de la UI |
| (b) Solo la UI | No hay forma: la UI no recibe el diagnóstico | — | No cumple |
| (c) Por Actualizar | El NotAppliedAdmission de la corrida lleva el diagnóstico en `LastApplyCause` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs:306`) | No toca `TryCheckNow` | Un Unknown(CommitThrew) no ofrece Actualizar; no cubre la vía |

### 4.4 Cómo se mantiene separado de la clasificación de la mutación

**[M].**
- `TryCheckNow` no forma parte de la corrida de Actualizar: no toma la ranura, no escribe y su guarda de núcleo es otra (`CORE:368-431`).
- La clasificación Recovered o Conflict sale **solo** de `StaleBase.CompareWith` (`DS:720-731`), no de la admisión.

**[I], cómo mantener la separación:**
- calcular el valor del diagnóstico (lectura de propiedades de la misma lectura) **antes** de `CheckNow`, o en su propio `try` que dé `null`, para que un fallo del diagnóstico no convierta en `Failed` una respuesta cuyo `CheckNow` ya cambió el borrador;
- en la UI, que el diagnóstico persistente solo afecte a la caja y a las opciones (`FVM:579-586`, `:612`), nunca al estado del borrador ni a una escritura;
- la orden: «A diagnostic failure must not alter authored state». Ni `TryCheckNow` ni la UI escriben el DWG; «authored» aquí es el DWG. **[I]:** el estado del borrador que un fallo no debe alterar es el que ya decidió `CheckNow`.

### 4.5 Una limitación relacionada, de T4B

**H-M3-42 [M].** La regla del origen de MUTATE difiere de la de «Comprobar ahora». En consecuencia, «la promesa de D-T4A2-06 (`A2P:349`) solo se cumple para las causas que no dependen del origen»: las reglas 2, 5 y 6 del lector pueden dejar un diagnóstico persistente mientras Actualizar admite (`T4BP:843`).

**[I]:** decidir si Actualizar se ofrece con un diagnóstico persistente en la superficie (Q-M3-07).

---

## 5. La concurrencia y la única corrida por Actualizar

### 5.1 La ranura

**H-M3-43 [M]. La ranura y su disciplina:**
- **una sola ranura para todo el registro**, `_applying` (`DR:130`), en todas las sesiones;
- **`TryBeginApply`** (`DS:510-526`) la toma solo desde Clean o Dirty sin texto pendiente, y responde `AnotherUpdateInProgress` si está ocupada (`DS:517`);
- **`CompleteApply`** (`DS:533-560`) copia antes de liberar (`:541`), libera una sola vez (`:543-544`) y después clasifica;
- **`TryUpdatePilot`** la toma en `CORE:837` y la cierra en un `finally`, exactamente una vez (`CORE:863-866`), y lo fija `TW/WorkspaceEditIntentGuardTests.cs:1051-1096`;
- **el segundo Actualizar** recibe `AnotherUpdateInProgress` (`TW/WorkspacePilotUpdateHostTests.cs:180-195`).

**Desfase:** `T4BP:81-83` cita `DS:508-524` y `:515`, medidos en `c3ae1c97`. En `b15072ff` son `:510-526` y `:517`.

### 5.2 Las fuentes que podrían disparar una segunda corrida

**H-M3-44 [M / I].**

| Fuente | Hoy | Hueco |
|---|---|---|
| **Eventos de UI** (doble clic, Enter o Espacio sobre el botón, un segundo clic) | No hay botón. Con un callback síncrono, el hilo de la UI está bloqueado durante la corrida (**[I]**) | **Con un callback diferido** [H], el intervalo entre el clic y `TryBeginApply` (dentro del callback) no lo cubre la ranura: dos clics encolan dos `EXECUTEFUNCTION`. El segundo vería Clean tras Applied (NothingToApply), o Dirty tras un NotApplied, y correría otra vez. Haría falta un cerrojo de «actualización pedida» desde la operación del puerto hasta el fin del callback, liberado también si la tarea falla o no ejecuta el callback |
| **Idle y drenaje** | Prohibidos en el drenaje del núcleo, en `OnIdle` del núcleo del puente y en `OnIdle` del puente (`TW/WorkspaceEditIntentGuardTests.cs:599-611`, autoprueba con `TryUpdatePilot` en `:633`) | El `OnDrained` del controlador (`:615-617`) y el del host de navegación (`:612-613`) **no** incluyen `TryUpdatePilot`. `NothingOfTheLifecycleIsCalledFromTheIdlePassOfTheCoreOrTheBridge` (`TW/WorkspaceLifecycleGuardTests.cs:668-675`) tampoco. `RunIdleContinuation` (`BRIDGE:280-284`) y el planificador no tienen esa regla. Ninguna de ellas conoce el nombre de la operación nueva del puerto |
| **El puente de eventos** | Los manejadores solo encolan (`TW/WorkspaceIdleSchedulingGuardTests.cs:64-104`) | Sin hueco medido |
| **La relectura de POST** | Está dentro de la corrida (`Post` usa `CaptureDrawing`; `T4BP:400-406`), no llama al host y no puede volver a entrar | Sin hueco medido |
| **Las notificaciones de mutación propia** | Las pistas, tras el drenaje, solo marcan e invalidan (H-M3-06, H-M3-07); la presentación posterior pinta desde la memoria (`BVM:214-234`, `:801-814`); la recarga perezosa solo la dispara un gesto (`FVM:287-335`) | Si T4C conecta algo de presentación a la operación nueva, ninguna guarda lo vería (§3.6) |
| **Avisos del controlador durante el comando** | `CommandWillStart` y `EnteringQuiescentState` disparan `NotifyStateChanged` (`BRIDGE:191-224`; `CTRL:219`, `:250-265`); dentro del aviso, la compuerta responde `Waiting` a las lecturas (`src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:167-183`) | **[I]:** una notificación síncrona de `CommandWillStart` dentro del gesto entraría de nuevo en el VM del campo con el gesto a medias. **[H]** |

### 5.3 Lo que garantiza hoy la cadena

- **[M]:** la ranura impide dos corridas **simultáneas** de T4B en todo el proceso.
- **[I]:** no impide dos corridas **sucesivas** de un mismo gesto si el gesto encola dos veces. «One explicit user Actualizar must create one T4B run» necesita, además, una marca de una sola petición por gesto, o que el gesto sea síncrono de punta a punta (**[H]**).

**H-M3-45 [M], relacionado con la contención en el host:**
- la comprobación Debug del drenaje del controlador usa `Debug.Assert(TopTransaction == null)` (`CTRL:267-273`, llamada en `:227`), a diferencia del puente, que solo registra (`BRIDGE:288-302`);
- Smoke-2 corre con el DLL Debug (C-F4-06, `F4P:58`).

**[I]:** si un drenaje llegara a correr con la transacción de MUTATE abierta (por un bombeo anidado), el `Debug.Assert` podría detener AutoCAD. **[H]**.

---

## 6. Lo que solo el host puede probar (consolidado)

1. Si el callback de `ExecuteInCommandContextAsync` corre antes de que vuelva la llamada desde la paleta (`T4P:550`).
2. El orden `CommandWillStart` → callback → notificaciones de objeto (en `Commit` o al modificar) → relectura de POST → `CommandEnded`, `CommandCancelled` o `CommandFailed` → `EnteringQuiescentState` → drenaje. Y si se cumple `F4P:60`.
3. Si los `*Cancel*` previos producen `CommandCancelled` además del vaciado de la selección.
4. Si algo dentro del callback bombea el despachador de WPF: la importación de biblioteca, `Regen` o el bloqueo.
5. Si «Aplicando» llega a verse.
6. Un `BeginDocumentClose`, `CLOSEALL` o `QUIT` con un Actualizar encolado o en curso.
7. Que tras Actualizar A, B no queda marcado y A no queda «posiblemente obsoleto»; que un `U` inmediato sí marca; que un cambio de B justo después del Actualizar sí marca.
8. La temporización de IC-04 tras un Actualizar.
9. El `Debug.Assert` del drenaje del controlador con el DLL Debug.

---

## 7. Riesgos

| Id | Riesgo | Base |
|---|---|---|
| R-M3-01 | Sin ámbito, todo Actualizar marca A y todos los B de la sesión (INV-08, INV-28 e IC-08 incumplidos) | H-M3-10 |
| R-M3-02 | Una supresión de sesión por bandera se traga las pistas externas ya pendientes al abrir y las de un `U` o de otro comando antes del drenaje. Se enmascara otro rack o el propio deshacer | H-M3-04; `FRZ:237-238` |
| R-M3-03 | Cerrar la ventana al final del callback deja fuera el `CommandEnded` propio (`F4P:60`); cerrarla en el drenaje contradice IC-08 («se drenan después del ámbito») | H-M3-13 |
| R-M3-04 | Una ventana sin cierre en toda salida (excepción, cancelación, tarea que no ejecuta el callback) queda abierta y enmascara | §1.9 (4) |
| R-M3-05 | En la vía A, «rack/member-bound» es nominal: una escritura ajena sobre B durante la ventana queda enmascarada (reactores de otros complementos, `FRZ:494-498`). Habría que declararlo como residual | §1.9 (5) |
| R-M3-06 | En la vía B, las escrituras propias tocan objetos que no son miembros (OM-5, la purga y las anidadas): la atribución por miembros vuelve a marcarlo todo | §1.10 |
| R-M3-07 | El descarte durante Applying desregistra el borrador mientras la corrida sigue, y se responde `ClosedDetached` después de escribir (prueba `TW/WorkspacePilotUpdateHostTests.cs:368-382`) | H-M3-14, H-M3-15 |
| R-M3-08 | Con un callback diferido, un doble clic produce dos corridas sucesivas; la ranura no cubre el intervalo | H-M3-44 |
| R-M3-09 | Las guardas de «nada desde el drenaje ni el Idle» no conocen la operación nueva, y dos de sus cuerpos no incluyen ni `TryUpdatePilot` | H-M3-44 |
| R-M3-10 | Los avisos se pierden en un repintado (no están en `PilotFieldView`) | H-M3-29 |
| R-M3-11 | «Aplicando» puede no verse nunca | H-M3-28 |
| R-M3-12 | El cableado del clic exige romper el reenvío literal del controlador o añadir un ejecutor persistente al núcleo | H-M3-36 |
| R-M3-13 | Hay seis implementadores, no cinco: un censo del paquete que cuente cinco fallaría | H-M3-25 |
| R-M3-14 | Las anclas de `T4BP` para `DS` ya no casan con `b15072ff` | H-M3-43 |
| R-M3-15 | Para llevar el diagnóstico a `TryCheckNow` hay que cambiar M-2, M-3 y la regla de retirada de D-T4A2-06; si no se declaran con autoprueba, la guarda podría debilitarse en silencio | H-M3-41 |
| R-M3-16 | El diagnóstico persistente y la admisión de Actualizar pueden divergir (`T4BP:843`) | H-M3-42 |
| R-M3-17 | El `Debug.Assert` del drenaje del controlador con el DLL Debug de Smoke-2 | H-M3-45 |
| R-M3-18 | El vaciado de la selección por `EXECUTEFUNCTION` hace que el usuario pierda la selección tras cada Actualizar | H-M3-32 |

---

## 8. Preguntas abiertas

| Id | Pregunta |
|---|---|
| Q-M3-01 | ¿Qué vía de C-F4-04, con qué IC o A-n, y quién la decide? `F4P:60` dice «sesión», y la orden de T4C la nombra objetivo mayor de Smoke-2 |
| Q-M3-02 | ¿Dónde se abre la ventana (en la operación del puerto o dentro del callback) y dónde se cierra (en el fin del comando propio, en `EnteringQuiescentState` o en un cierre explícito del adaptador)? ¿Con qué respaldo si nunca llega el fin del comando? |
| Q-M3-03 | ¿Se conservan las pistas ya pendientes al abrir la ventana, con una instantánea o con un conjunto aparte? |
| Q-M3-04 | ¿Se acepta, como residual declarado, que una escritura ajena sobre B durante la ventana quede sin marcar? ¿O el Owner exige la vía B, con sus choques con `FRZ:217-218`, IC-04 y `TW/WorkspaceNavigationEchoTests.cs:325-340`? |
| Q-M3-05 | ¿Un Actualizar que no llega a escribir (rechazo, Preflight o Conflict) abre la ventana para su `CommandEnded`? |
| Q-M3-06 | R-T4-07: ¿marca del host (`Busy` o `Waiting`) o cambio en el registro? ¿Con qué texto, y con qué efecto sobre la tabla de literales? |
| Q-M3-07 | ¿Se ofrece Actualizar en Dirty «posiblemente obsoleto», con un diagnóstico persistente en la superficie, en una pestaña de vista previa o en Conflict? |
| Q-M3-08 | ¿`PilotBoundaryKind.Update` al final del enumerado y asignado a `DraftBoundary.Update`, o basta el `LostFocus` con `CommitField`? ¿El botón toma el foco? |
| Q-M3-09 | ¿Los avisos se muestran como aviso del gesto o en la vista (cambio declarado de `PilotFieldView`)? |
| Q-M3-10 | M-03: ¿se muestra la clasificación de un `ClosedInError`? |
| Q-M3-11 | Si el callback es diferido, ¿cuál es el canal de finalización hacia la UI? ¿Qué muestra el campo entre el clic y `TryBeginApply`? |
| Q-M3-12 | ¿Cómo llega el clic al Plugin: con un ejecutor asignado al núcleo, con una excepción del controlador o con un envoltorio? ¿Cuenta un ejecutor asignado una vez como «persistent host authority»? |
| Q-M3-13 | ¿Se restaura la selección implícita tras Actualizar, o se acepta su vaciado? |
| Q-M3-14 | Diagnóstico de `TryCheckNow`: ¿propiedades nuevas en `EditCheckOutcome`, y qué texto en la vía Unknown? ¿Se reutilizan A2-03 y A2-06? |
| Q-M3-15 | ¿Se cambia, o se neutraliza en el host, el `Debug.Assert` de `CTRL:267-273` antes de Smoke-2? |
| Q-M3-16 | ¿Cerrojo de «actualización pedida» por gesto? ¿Y, si existe, dónde vive (núcleo, ejecutor) y cómo se libera si la tarea no ejecuta el callback? |

---

## 9. Posibles condiciones de devolución

Hechos que podrían activar alguna de las condiciones de «RETURN BEFORE EMISSION» de la orden del Owner:

1. **«any other material product decision».**
   - **El enunciado.** Con pistas sin rack ni handle y el marcado grueso por sesión, no se pueden cumplir a la vez INV-28 («Actualizar A no marca B», `FRZ:242-243`, `:766`) y «unable to mask another rack» (orden de T4C) sin elegir entre dos cosas:
     - un residual declarado (vía A: una escritura ajena sobre B durante la ventana queda enmascarada);
     - o la vía B, que choca con el Freeze («coste constante», `FRZ:217-218`), con IC-04 (`IC:77`) y con una guarda vigente (`TW/WorkspaceNavigationEchoTests.cs:325-340`).
   - **Las ventanas** de `FRZ:241-243`, `F4P:60` y la orden («only around the actual authored operation») no coinciden. C-F4-04 exige IC o A-n.
2. **«DocumentLock/command context cannot wrap the T4B run cleanly»**, o una decisión material de arquitectura.
   - **Si el callback es diferido** (**[H]**), la cadena síncrona del puerto no puede devolver el resultado sin `Task` ni `async`, prohibidos en la UI, en el controlador y en las carpetas Workspace (H-M3-36).
   - Hace falta un canal de finalización: es la A-n de `T4P:550` y el R-2 del mapa de T4A1.
3. **«the pilot-specific writer needs a new persistent host authority».** La opción (a) de §3.7, un ejecutor asignado por *setter* y guardado en un campo del núcleo, es un objeto de larga vida en el host, aunque el escritor siga siendo de una corrida. Hay que decidir si se lee como autoridad persistente. Las opciones (b) y (c) lo evitan, a costa de romper guardas del controlador.
4. **Ninguna de las piezas de este mapa cambia un comportamiento caracterizado por T1B.**
   - **Lo que tocan:** el panel, el núcleo del Workspace, el puente y el registro. Ninguno de esos archivos es de la clausura de la ventana clásica.
   - **Lo que no es de T1B:** el cambio de la prueba de `TW/WorkspaceDraftRegistryContainmentTests.cs:383-411`, si R-T4-07 se lleva al registro, es una caracterización de F4-T2B2B, no de T1B. Se señala para que no se cambie en silencio.

---

## 10. Recuento

- **Hechos numerados:** 45 (H-M3-01 a H-M3-45).
- **Riesgos:** 18 (R-M3-01 a R-M3-18).
- **Preguntas abiertas:** 16 (Q-M3-01 a Q-M3-16).
- **Posibles condiciones de devolución:** 4.
