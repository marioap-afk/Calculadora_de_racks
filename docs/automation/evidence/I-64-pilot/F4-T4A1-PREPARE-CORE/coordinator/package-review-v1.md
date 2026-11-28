# I-64 — Revisión adversarial independiente del paquete de F4-T4 (v1)

**Objeto:** `I-64-f4-t4-gate-contract-package.md` v1, con la orden literal `owner-f4t4-preparation-authorization.md` y los tres mapas (`map-1-legacy-write-i55-undo.md`, `map-2-drafts-freeze.md`, `map-3-host-seams-guards.md`).
**Código revisado:** rama `architecture/workspace-persistente-rackcad`, HEAD `7297e1f1` (código de `4e34bb56`). Solo lectura: sin build, sin pruebas, sin AutoCAD y sin subagentes.

## Veredicto

| Objeto | Veredicto |
|---|---|
| **Descomposición de F4-T4** (T4A1, T4A2, T4B y T4C condicional) | **EMITIBLE TRAS CORRECCIONES** |
| **Contrato de F4-T4A1-PREPARE-CORE** | **EMITIBLE TRAS CORRECCIONES**. El bloqueante B-1 se corrige en la v2 sin rediseño ni vuelta al Owner. Por el número de importantes que tocan el planificador y el puerto (I-1 a I-7), conviene re-revisar la v2 antes de emitir, como en T3B y T3C |

**Recuento:** 1 BLOQUEANTE, 12 IMPORTANTE y 14 MENOR.

**Ninguna condición de la lista del Owner obliga a volver antes de T4A1:**
- la autoridad de escritura congelada no cambia;
- T4 se puede separar en un PREPARE de solo lectura y una mutación;
- no entra trabajo de F5;
- Undo sigue sin medir;
- la semántica heredada no choca con el piloto (ver M-11).

Hay tres avisos al Owner recomendados (§ «Decisiones para el Owner»).

---

## Hallazgos

### B-1 (BLOQUEANTE). El `Prepare` de D-T4A1-03 no se puede usar donde lo exige R-T4B-02

**Referencia:** paquete §4, D-T4A1-03 (resultados 1 a 3) frente a §6, R-T4B-02. Código: `DraftSession.cs:285` (`Status => _explicitStatus ?? …`) y `:467-483` (`TryBeginApply` pone `_explicitStatus = Applying` y rechaza el texto pendiente).

**El problema:**
- R-T4B-02 fija «`TryBeginApply` antes de PREPARE», con PREPARE «repetido dentro, con el planificador de T4A1».
- Después de `TryBeginApply`, `draft.Status` es `Applying`, así que el resultado 1 de `Prepare` («su estado no es Clean ni Dirty derivado») responde **siempre NotApplicable**. El resultado 2 (PendingText) es inalcanzable y el 3 (NothingToApply, «está Clean») no se puede evaluar por `Status`.
- El entregable central de T4A1 sale inservible para su único consumidor, y T4B tendría que reabrir un archivo cerrado de T4A1.

**Lo que falta además:**
- **La correspondencia entre los resultados de `Prepare` y los ocho `DraftApplyOutcomeKind`**, que fija `WorkspaceDraftApplyTests.cs:591`. T4B la necesita para `CompleteApply`: NotAdmitted → NotAppliedAdmission, Conflict → NotAppliedConflict, Invalid → NotAppliedPreflight o NotAppliedFailure, y NothingToApply → ¿cuál?
- **El punto de llamada de T4B.** La firma `Prepare(draft, reread, catalog)` exige a la vez el `DraftSession` (namespace `RackCad.Application.Workspace.Drafts`) y el `RackCatalog`.
  - El catálogo solo lo carga el Plugin (`RackPilotIntentAdapter.LoadCatalog`) y no viaja en `PushBackPilotIntentResult`.
  - El adaptador de `Plugin/Views` no puede nombrar `RackCad.Application.Workspace` (`WorkspaceBridgeSourceGuardTests.cs:361-366`).
  - Por tanto, el llamador solo puede ser el host core (o `Plugin/Workspace`) con el catálogo recibido del escritor del Plugin. Hay que fijarlo ahora, para que la firma sea consumible.

**Corrección:**
- `Prepare` opera sobre una instantánea explícita: StaleBase, `InitialDraftState`, `CurrentDraftState`, `HasPendingText`, `IsDirty` y el estado previo al intento (`StatusBeforeAttempt`). Otra opción es que admita `Applying` cuando la ranura la tiene la propia operación y decida con `IsDirty` y `StatusBeforeAttempt`, nunca con `Status`.
- Se añade a la v2 la tabla de resultados de `Prepare` → `DraftApplyOutcomeKind` y se declara el punto de llamada y el flujo del catálogo en T4B.

### I-1 (IMPORTANTE). El orden de `Prepare` pone NotAdmitted antes de Conflict, contra INV-06 y el precedente medido

**Referencia:** D-T4A1-03, resultados 4 y 5. Proposal `:744` (INV-06: «diferencia en atribución, en un sobre o en el registro → No aplicado — Conflicto»). Pruebas `WorkspaceDraftPilotIntentStaleBaseTests.cs:552` y `:571`, donde una relectura NotAdmitted porque un miembro quedó Blocking, o por AUTH-13 Divergent, va a **Conflicto**, y `:605`, donde una relectura NotAdmitted solo por el catálogo, con la misma base, se **recupera**.

**El problema:**
- Un cambio externo que vuelve un miembro ilegible o divergente cambia la base y quita la admisión.
- Con el orden propuesto se clasifica como Admisión: el borrador no pasa a Conflicto, el usuario no ve «qué cambió» y puede reintentar sin fin.
- Cero escrituras, pero la clasificación congelada se incumple.

**Corrección:** comparar primero.
- Si la base difiere, Conflict, con independencia de la admisión.
- NotAdmitted solo con MATCH.
- Declarar también que NothingToApply (Clean) va antes de la comparación y que descarta una relectura ya hecha.

### I-2 (IMPORTANTE). El plan es demasiado delgado para la obligación del Owner y no permite verificar POST

**Referencia:** orden del Owner, T4A: «validation immediately before mutation» y «building an immutable mutation plan». D-13, pasos 1 y 3. D-T4A1-03, «Lo que no está en el plan». R-T4B-06.

**El problema:**
- **Parte de lo que el plan deja fuera es Application pura**, y la justificación «necesitan el dibujo **o el builder**» no se sostiene para ello:
  - **la construcción del sistema** con el builder existente (D-13 paso 1: «Si falla, no hay escritura y el campo muestra el error»). `PushBackSystemLateralBuilder` y los ensambladores están en `src/RackCad.Application/Systems/PushBack/`;
  - **el diseño saliente serializado** (`BuildDesign` y `RackProjectStore.Serialize`);
  - **los sobres salientes por vista** (INV-10: `RackEmbedComposer.Compose` en `src/RackCad.Application/Persistence/` y `WithSourceMetadataFrom` sobre el proyecto interior deserializado de cada sobre);
  - **Redraw/Erase recalculado desde el `CurrentDraftState`** con la disponibilidad lateral (`RackViewAvailability` en `src/RackCad.Application/Systems/Shared/`), a partir de `View`/`Section` de cada miembro.
- Todo eso se puede probar en Core. Pasarlo a T4B agranda la tarea más amplia (I-10).
- **Lo que sí necesita el dibujo:** las capas bloqueadas, las piezas faltantes, la importación y los supervivientes (los recuentos de referencias de layout no están ni en la StaleBase ni en `PushBackPilotIntentResult`).
- **Falta la expectativa de POST.**
  - El Owner pide que Dirty pase a false solo desde el estado posterior **verificado**.
  - `CompleteApply(Applied(postRead))` (`DraftSession.cs:537-557`) rebasa y deja Limpio con lo que diga la relectura, sin compararlo con lo que se quiso escribir.
  - Si la escritura pierde la escala en silencio, el borrador queda Limpio con el valor viejo y la edición desaparece.
  - El plan debe llevar el estado o los sobres esperados, para que T4B compare la relectura de POST y clasifique la discrepancia (con avisos o como Desconocido), sin dar por bueno lo que el comando devolvió.

**Corrección:**
- El plan de T4A1 incluye el sistema construido (o el error tipado de construcción), el diseño saliente serializado, los sobres salientes por vista (INV-10), la división Redraw/Erase recalculada y la expectativa de POST.
- Quedan para T4B solo las capas, las piezas faltantes, la importación y los supervivientes. Hay que declarar que el planificador vive fuera de `Application/Workspace`, porque `WorkspaceHostSourceGuardTests.cs:373-378` prohíbe ahí `RackEmbedComposer`.

### I-3 (IMPORTANTE). «Conservar mi borrador» en un solo paso no es posible con la API actual

**Referencia:** D-T4A1-02, `TryKeepMine` («en un solo paso… Si algo falla, nada cambia»). `DraftSession.cs:428-443` (`KeepMineOnCurrentVersion` rebasa y conserva `_current`), `:330-335` (`SetCurrentDraftState` copia y asigna) y `PushBackPilotMaterializer.cs:223-232`. F4-E24, menor «el rebase no es atómico».

**El problema:**
- La secuencia es `KeepMine`, luego `KeepPilotEditsOn` y luego `SetCurrentDraftState`.
- Si el copiador lanza en el tercer paso, queda un estado intermedio observable: la StaleBase y el inicial nuevos con el `CurrentDraftState` viejo, que conserva la capa no pilotada de la versión anterior. El borrador queda Dirty, y `ComposeForWrite` lo rechazaría con un motivo confuso.
- No hay escritura indebida, pero la promesa del contrato es falsa.
- **La guarda de la base no lo admite.** Si el host pasa `pilot.Read.InitialState` a `KeepPilotEditsOn`, eso es un uso de `.InitialState` que `WorkspaceEditIntentGuardTests.cs:635-663` no admite. D-T4A1-06 no lo declara.

**Corrección:** elegir una de dos.
- Una operación aditiva del núcleo que reciba el estado actual sustituto y haga todas las copias antes de asignar. Es un cambio declarado del archivo de F4-T1.
- Declarar el residual y su orden: `KeepPilotEditsOn` antes que `KeepMine`, con una prueba del copiador que lanza.

En las dos, hay que declarar el consumidor nuevo en la guarda de la base, o usar `draft.InitialDraftState` tras el rebase.

### I-4 (IMPORTANTE). La prueba de R-3 que propone D-T4A1-05 no discrimina el mutante

**Referencia:** D-T4A1-05. `WorkspaceHostCore.cs:908-909`. `DraftSession.cs:308`, `:314`, `:456`, `:503-512` y `:601-616`. El cierre de F4-T3C1 («R-3: … no está discriminado»).

**El problema:**
- En todo estado alcanzable, también con las tres operaciones nuevas, `ErrorReason != null` implica `Status == Error`:
  - solo lo asignan `EnterError` y la razón diferida, las dos junto con Error;
  - `Rebase` lo limpia;
  - desde Error no hay `TryBeginApply`, `KeepMine`, `CheckNow` ni `ConfirmBase`.
- La razón diferida vive en otro campo (`DeferredErrorReason`), y el host lee `draft.ErrorReason`, que en Aplicando es null.
- Por tanto, las tres afirmaciones propuestas (Error con motivo; tras `DiscardAndReload` sin motivo; otro estado con razón diferida sin motivo) **pasan también** con el mutante que quita `state == PilotFieldState.Error ?`.
- R-3 seguiría abierto bajo la apariencia de cerrado, y el Owner lo pidió como cambio declarado del contrato.

**Corrección:**
- Una guarda por cuerpo sobre la expresión de `ViewOfPilotDraft`, con su autoprueba en negativo, como cambio declarado.
- Opcionalmente, un invariante del núcleo (guarda o prueba) de que `ErrorReason` solo se asigna junto con `_explicitStatus = DraftStatus.Error` o con null.

### I-5 (IMPORTANTE). El puerto que T4A1 «cierra» no da a T4A2 lo que su UI necesita

**Referencia:** §3.1 («T4A2 añade la superficie de UI sobre un puerto ya cerrado»). D-13, tabla por frontera («la causa y, en Conflicto, qué cambió»; Desconocido tras la relectura de POST: «recarga para seguir editando»). INV-31 (Desconocido con «Comprobar ahora»). `IWorkspacePilotEditPort.cs:165-218` y `WorkspaceHostCore.cs:861-911`.

**El problema:**
- `PilotFieldView` no lleva ni el origen de Desconocido ni las diferencias o la causa del Conflicto.
- **Sin el origen**, la UI no puede elegir entre «Comprobar ahora» (`TryCheckNow`, solo CommitThrew) y «Recargar» (`TryDiscardAndReload`, PostReread).
- **Sin las diferencias**, el Conflicto pierde «qué cambió» al cambiar de pestaña y volver: solo lo trae la respuesta inmediata de `TryConfirmBase`.
- Si T4A2 lo descubre, reabre el puerto y la vista de T4A1.

**Corrección:**
- Decidirlo en T4A1: el origen de Desconocido, las diferencias en el formato `clave:Tipo` de `DescribeDifferences` y la causa del último intento en la vista.
- Declarar el cambio de la guarda de `PilotFieldView`: un solo constructor con `errorReason` como último parámetro (`WorkspaceHostHardeningGuardTests.cs:363-369`).
- Fijar la regla de despacho de las dos operaciones de «Comprobar ahora».

### I-6 (IMPORTANTE). El alcance de T4A1 omite dos implementadores del puerto en Core

**Referencia:** D-T4A1-02, reenvíos («El doble de UI … implementa las tres operaciones»). D-T4A1-06 («UI: solo el doble»). `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs:17` (`CommandAdvisorPortDouble : IWorkspacePilotEditPort`) y `:1104` (`ForwardingRiskPort : IWorkspacePilotEditPort`).

**El problema:**
- Añadir tres miembros a la interfaz rompe la compilación de `RackCad.Tests` si esos dos dobles no cambian, y el contrato no los incluye en el `AllowedWriteScope`.
- Ese archivo es además la superficie de C2-M-8 (`:372-392`).

**Corrección:**
- Incluir `WorkspaceCommandAdvisorTests.cs` en el alcance de T4A1, con los dos dobles: respuestas inertes, sin cambiar ninguna aserción.
- Pasar C2-M-8 a T4A1, porque T4 sí toca esa superficie («Fix them where T4 touches those guard surfaces»).

### I-7 (IMPORTANTE). Las guardas existentes no cubren las operaciones nuevas, y el contrato no obliga a extenderlas

**Referencia:** D-T4A1-06 («si las operaciones nuevas lo exigen»). `WorkspacePilotEditGuardTests.cs`:
- `:43-63`: las 12 `Operations` y las 3 `ReadOperations`;
- `:239-244`: solo las lecturas nombran el lector;
- `:357-375`: la tabla de la marca de lectura;
- `:411-439`: la compuerta, donde esperan solo las tres lecturas y el resto es delegación pura;
- `:490-500`: los reenvíos del controlador;
- `:664-737`: `WriteViolations` por cuerpo.

Además, `WorkspaceEditIntentHostTests.cs:969-986` (los portadores sin tipos de Drafts ni de Systems), `WorkspaceEditIntentGuardTests.cs:592-610` (los nombres prohibidos en el drenaje y el Idle) y el comentario del puerto `IWorkspacePilotEditPort.cs:14-24` («only the three operations of the edit intention read the drawing»).

**El problema:**
- Todas estas guardas son cotas inferiores sobre los 12 nombres. Las tres operaciones nuevas no rompen nada, pero tampoco quedan protegidas.
- Nada fijaría, para ellas: el `Waiting` dentro de un aviso; la marca con `try`/`finally`; cero APIs de escritura por cuerpo; el reenvío literal; la ausencia de lecturas desde el drenaje y el Idle; y la comprobación de aplicabilidad **antes** de la lectura (el paso 6 de `TryCheckPilotEdit`).
- La «prueba de cero escrituras en PREPARE» de la orden descansa en estas guardas: las pruebas de Core con un lector simulado son vacuas, porque el host no tiene escritor.
- Con el texto actual, el Worker puede no extenderlas.

**Corrección:** la v2 enumera como obligación la extensión positiva de esas tablas a `TryConfirmBase`, `TryKeepMine` y `TryDiscardAndReload` (y a `EditConfirmOutcome` en los portadores), con sus autopruebas en negativo, y actualiza el comentario del puerto.

### I-8 (IMPORTANTE). H-08 no vale para la comprobación previa O-04 que T4B tiene que repetir dentro del contexto de comando

**Referencia:** H-08 («Vale para la comprobación previa (O-04)…»). R-T4B-02 (PREPARE repetido dentro del bloqueo único). `WorkspaceNavigationHost.cs:225-226` (`CommandInProgress` no vacío o el editor no en reposo → Busy). `WorkspaceHostCore.cs:1055-1058` (`PassGate` → Waiting). Medición de Smoke-NAV: el callback corre en `EXECUTEFUNCTION` (`I-64-evidence.md:5912`; `F2-SMOKE-NAV/.../diag-1-racklog.txt:2`).

**El problema:**
- D-13 pone O-04 dentro del bloqueo único, en contexto de comando.
- Ahí el lector existente responde Busy y la compuerta Waiting: no sirve para el PREPARE de T4B.
- Si O-04 se hace antes, en contexto de aplicación y con su propio bloqueo, se pierde el «un único `using LockDocument`» de D-13 paso 2 tal como lo recoge R-T4B-02.
- **Lectores distintos.** «Comprobar ahora» (`ConfirmBase`) y la relectura caller-owned de MUTATE usarán lectores distintos. Si para el mismo dibujo dan StaleBases distintas, un Conflicto de MUTATE se «limpiaría» con un MATCH de «Comprobar ahora» y volvería en cada Actualizar.

**Corrección:**
- Corregir H-08.
- Añadir a las reglas de T4B la vía de lectura en contexto de comando (sin la puerta de reposo, con la revalidación de sesión) o la decisión explícita sobre el bloqueo.
- Añadir un invariante de equivalencia: los dos lectores dan la misma `DraftStaleBase` para el mismo dibujo, con la misma `BuildStaleBase` (F4-E26).

### I-9 (IMPORTANTE). La «relectura autoritativa antes de actualizar» del Owner se reetiqueta como O-04

**Referencia:** orden del Owner, T4A, primera obligación. Paquete §3.3. D-12 (`:465-469`: la autoridad es la relectura dentro de MUTATE; `:480-481`: O-04 «No sustituye» a esa relectura).

**El problema:**
- El paquete cumple la primera obligación de T4A llamando autoritativa a una comprobación que el Freeze dice que no lo es.
- Justifica las reasignaciones con «la mecánica no se congela», pero esa frase es de D-13 y vale frente al Freeze, no frente al reparto que ordena el Owner.
- **La variante caller-owned de lectura (INV-39) es de solo lectura:**
  - un `Traverse` aditivo sin bloqueo, sin transacción y sin `Commit` propios, con su guarda;
  - el paquete de F4 v2 la tenía en T4A;
  - los mapas 2 (riesgo 2b) y 3 (R-1) señalan que cabe en T4A.
- Moverla a T4B agranda T4B (I-10) y deja a T4A sin la autoridad de D-12.

**Corrección:** elegir una de dos.
- Llevar la variante caller-owned y su guarda de fuente a T4A1. Su uso dentro de la transacción sigue en T4B.
- Mantenerla en T4B y **avisar al Owner** de forma explícita: T4A entrega O-04, que no es la autoridad de D-12, y la autoridad sigue en MUTATE (T4B).

### I-10 (IMPORTANTE). El criterio de división es incoherente: T4B es claramente la tarea más amplia, y T4C se aplaza a una medición sin vehículo

**Referencia:**
- §3.1 divide T4A por amplitud (unos 22-25 archivos, con el precedente de T3C).
- §6, R-T4B-01 a R-T4B-12, más los mapas 1 y 3:
  - el adaptador con `ExecuteInCommandContextAsync` y el puente con el `Task`;
  - las excepciones de unas 15 guardas (mapa 3, «Guardas que cambian en T4B»);
  - la interfaz de escritor Application ↔ Plugin, con el censo de 9 archivos de `Plugin/Workspace`;
  - la lectura caller-owned;
  - la transacción con la frontera de `Commit()` y la inyección Debug (INV-30);
  - la guarda de las 3 fachadas de Push Back;
  - las unidades por vista y C-F4-05;
  - la importación;
  - POST con `SyncName`, la purga, `Regen` y la relectura;
  - los cambios del núcleo (F4-E25 (4) y N-5);
  - la marca de host de Aplicando;
  - el ámbito de mutación propia (C-F4-04 e IC-08 en `EventBridgeCore`/`WorkspaceSession`);
  - la UI de Actualizar y la frontera Update;
  - los avisos de INV-31;
  - C2-M-4 a C2-M-7.
- C-F4-02 del paquete de F4: «La medición en host (escrituras y UNDO) **va dentro de T4B**».

**El problema:**
- Con la misma vara que divide T4A, T4B ya excede a un Worker.
- La orden del Owner dispara T4C por amplitud («host/command wiring, Undo integration, or post-write reconciliation makes T4B too broad»), no solo si falla Undo. El mapeo, que es la evidencia, ya está hecho.
- **R-T4B-03 no se puede ejecutar tal como está:**
  - pide medir «antes de fijar el paquete» si un Actualizar da un paso de Undo, pero ese Actualizar no existe hasta T4B;
  - medir el retorno del callback exige código con `ExecuteInCommandContextAsync`, prohibido en todo `src` (`WorkspaceNavigationHostTests.cs:436`), y una sesión de AutoCAD;
  - ninguna tarea lo posee, y T4A no puede escribir;
  - contradice además C-F4-02.

**Corrección:** proponer ya, con esta evidencia, una división de T4B en dos tareas.
- **F4-T4B, MUTATE y POST,** en Application y `Plugin/Views`, probadas con guardas de fuente y puertos simulados:
  - la lectura caller-owned, si no va a T4A1 (I-9);
  - la transacción, la frontera de `Commit()` y la inyección;
  - las fachadas, las unidades y C-F4-05;
  - la importación;
  - POST y su relectura;
  - F4-E25 (4) y N-5.
- **F4-T4C, HOST, UNDO y POST-INTEGRATION:**
  - el contexto de comando y el puente con el `Task`;
  - la marca de Aplicando del host;
  - el ámbito de C-F4-04;
  - la UI de Actualizar y la frontera Update;
  - los avisos;
  - la medición en host como primer paso, con su parada (A-n de D-13 si el callback es diferido o Undo no es un paso).

Hay que medir el número de archivos de cada una, como se hizo con T4A. No es para ganar presupuesto: es la misma vara aplicada a la tarea más amplia.

### I-11 (IMPORTANTE). La división A1/A2 cambia el presupuesto y cita un precedente incompleto

**Referencia:** §3.1 («F4-T3C, con unos 28, se dividió por la misma razón»). F4-E38 (`owner-f4t3-decomposition-decision.md`), F4-E40 (`owner-f4t3b-decomposition-decision.md`) y F4-E47 (`owner-f4t3c-decomposition-decision.md`): «Orden del Owner … pregunta y respuesta literales». Orden de F4-T4: «Do not create T4C merely to gain attempt budget».

**El problema:**
- Desde F4-T3, **todas** las divisiones las decidió el Owner con pregunta literal; la sesión solo aportó la medición.
- A1/A2 duplica la capacidad de T4A (de 3 a 6 intentos), y la orden nombró T4A como una tarea.
- La orden permite «AT LEAST two», así que no hay prohibición. Pero el paquete presenta la división como disposición de la sesión, con un precedente que omite quién decidió.
- La justificación por capas es razonable (A2 es la UI sobre el puerto de A1), siempre que el puerto quede de verdad cerrado en A1 (I-5).

**Corrección:**
- Registrar la división en F4-E51 con su efecto de presupuesto y corregir la cita del precedente.
- Avisar al Owner o preguntarle con el mismo formato de F4-E40 y F4-E47. Si no, emitir T4A como una sola tarea.

### I-12 (IMPORTANTE). Dos arrastres de F4-E25 desaparecen sin disposición

**Referencia:** H-05 y D-T4A1-04 («Siguen pendientes tres arrastres»). F4-E25 (`f4-dispositions.md:450-455`): **a T4A** (1), (3) y (6), y también **(2)**, separar en variantes distintas el escenario (b) de «Conservar», y **(5)**, una aserción sin normalizar del corpus canónico en `RackCad.UI.Tests`. `F4-T2A-PILOT-STATE/task-close-package.md:55` («cinco menores … van a T4A»).

**El problema:** el hecho H-05 es incompleto, y dos arrastres dispuestos se pierden en silencio.

**Corrección:** asignar (2) a T4A1 (pruebas de Core de `WorkspaceDraftPilotMaterializationTests.cs`) y (5) a T4A2 (`RackCad.UI.Tests`), o disponerlos de nuevo de forma registrada.

---

### M-1 (MENOR). La redacción del arrastre (1) pierde la neutralización

D-T4A1-04 (1) dice «compara `current` con `initial` sin pasar `current` por `Copy`». F4-E25 dice «contra una copia de `initial` con la escala neutralizada». Leída al pie de la letra, la versión del paquete rechaza toda edición de la escala. Hay que copiar el literal de F4-E25.

### M-2 (MENOR). La igualdad «por valor» del plan no sale gratis

- Un `record` con colecciones `IReadOnlyList` compara las listas por referencia.
- `DraftStaleBase` es una clase sin `Equals`.
- Hay que fijar la igualdad explícita: las colecciones elemento a elemento en orden ordinal, y `ExpectedBase` por `CompareWith(...).IsMatch` o por sus entradas.

### M-3 (MENOR). La clave del origen y su texto pueden no corresponderse

- `DraftSession` no guarda la clave del origen.
- El `OriginKey` de la relectura sale de `NavigationTargets` y de `chosenReference` (`WorkspaceNavigationHost.cs:238-240`) y puede ser otra vista que aquella cuyo texto es `initial.Origin`.
- **Corrección:** el planificador deriva la clave del miembro cuyo `EnvelopeText` es `initial.Origin`, o rechaza la incoherencia.

### M-4 (MENOR). Los sobres por vista del plan salen «de la base»

- La orden pide separar con claridad la StaleBase de la relectura autoritativa, y D-10 dice que la StaleBase sirve «solo [para] detectar cambios externos».
- Con MATCH los textos son idénticos, pero el plan debe tomarlos de la relectura comprobada y nombrarlos así, no de la StaleBase.

### M-5 (MENOR). `ConfirmBase`: declarar cuatro interpretaciones

- **Conflicto con MATCH → estado derivado.** Es aceptable: no hay nada que reconciliar, y es análogo a O3-06 de `CheckNow`. Hay que declarar que es la lectura de «confirma su estado» y qué pasa con `LastApplyCause`, que `CheckNow` sí limpia (`DraftSession.cs:582`).
- **Limpio con diferencia → Conflicto,** en lugar de la recarga perezosa de O-01. Es el literal de D-12; hay que declararlo.
- **NotAdmitted con MATCH → Confirmed.** Es coherente con `WorkspaceDraftPilotIntentStaleBaseTests.cs:605`. Conviene devolver el diagnóstico de admisión, porque si no el usuario recibe «confirmado» y Actualizar falla después.
- **Desconocido tras la relectura de POST → NotApplicable.** Es correcto, porque comparar con la base anterior a la escritura daría un Conflicto falso sobre la escritura propia. La única salida es `TryDiscardAndReload`, y depende de I-5.

**Confirmado:** `ConfirmBase` nunca toca `_initial`, `_current` ni el pendiente, así que no limpia Dirty. Limpiar la marca con MATCH es seguro: ninguna prueba fija que solo `Rebase` la limpie (`WorkspaceDraftSessionTests.cs:612-705`).

### M-6 (MENOR). Las operaciones nuevas están poco especificadas

- qué `EditIntentStatus` devuelven `TryKeepMine` y `TryDiscardAndReload` (¿un `Kept` nuevo al final? ¿`Reloaded`? ¿`NotNeeded`?); el enumerado admite añadir al final (`WorkspaceEditIntentHostTests.cs:1020`);
- la aplicabilidad **antes** de leer, como el paso 6 de `TryCheckPilotEdit`;
- `_knownClean.Remove`, como en `WorkspaceHostCore.cs:414`, `:417` y `:493`;
- la redacción «Busy y Waiting»: las lecturas existentes responden Waiting con la marca (`:555`).

### M-7 (MENOR). La política de literales obliga a inventar sinónimos

La regla «no repiten literales de la tabla H-07» obliga a usar textos distintos para la misma condición, por ejemplo «El dibujo no está listo para leerse.» o el testigo sin clave. Es preferible declarar el cambio de las cuentas en `WorkspaceHostHardeningGuardTests.cs:398-428`, en línea con la orden de T3C2 («invariant-oriented guards»), y reutilizar `NoDraftReason`.

### M-8 (MENOR). Confirmaciones y despacho de la UI

- `TryDiscardAndReload` sobre un Dirty descarta ediciones: T4A2 debe pedir confirmación, igual que el cierre Dirty de D-14. El paquete solo la pide de forma explícita para «Conservar».
- Con dos operaciones de «Comprobar ahora», hay que fijar el despacho por estado y origen (ligado a I-5).

### M-9 (MENOR). La reubicación de T1B es legítima, pero hay que fijar tres cosas

**Por qué es legítima:** F4-E18 fue una disposición de la sesión, y su motivo («como muy tarde antes de T4A», porque el T4A original tocaba las fachadas) ya no se aplica a A1 ni a A2.

**Lo que hay que fijar:**
- la clausura dice «los tipos de estado y ensambladores de `src/RackCad.Application/Systems/PushBack/**`»: hay que declarar que `Systems/PushBack/Pilot/` (el materializador y el archivo nuevo del plan) queda fuera;
- el disparador cambia de «modificar un archivo de la clausura» a «usar las fachadas para escribir»: hay que enunciarlo como regla nueva en F4-E51;
- F4-E51 tiene que estar registrada antes de emitir.

### M-10 (MENOR). La importación de biblioteca fuera de PREPARE es legítima

**Por qué:** D-13 congela el residuo OM-5, la importación fuera de la transacción authored y la comprobación O-04 antes de importar. La posición temporal no cambia.

**Lo que hay que declarar al Owner:**
- un Conflicto que solo detecte MUTATE deja residuo, como admite D-13;
- la agrupación de Undo de la importación en el contexto de comando se mide en Smoke-2.

### M-11 (MENOR). `LegacyDeltas` no obliga a volver al Owner, pero conviene cerrarlo ya

**Por qué no obliga:**
- T2H-07 ya la aceptó («igual que la ruta de Application de RACKEDITAR», con Smoke-2 como resultado esperado).
- Es una migración desde la base, no una copia vieja del borrador. Por ejemplo, el peralte heredado pasa a `HighEndBeamPeraltes` (`WorkspaceDraftPilotMaterializationTests.cs:706-730`).
- Así que no choca con «Do NOT rebuild … from stale draft copies».

**Por qué conviene cerrarlo:** la condición de R-T4B-07 («la presenta al Owner») deja abierta una decisión que, si fuese «los antiguos son No editable aquí», cambiaría los resultados del `Prepare` de A1.

**Corrección:** cerrarla ahora con un aviso al Owner, o que el plan exponga una marca de «canonicaliza un dibujo antiguo», para que cualquier decisión encaje sin reabrir A1.

### M-12 (MENOR). Huecos de las reglas de T4B

- **Una marca de host de «actualización en curso».** `DiscardRack` y `DiscardDocument` solo miran `_pilotReadInProgress` (`WorkspaceHostCore.cs:727` y `:751`), y `DiscardByUser` desliga un borrador Aplicando (mapa 3, R-7).
- **`NotePilotEdit`** en Actualizar (F4-E35).
- **La UI:** el botón Actualizar, `PilotBoundaryKind.Update` (punto 8 de Smoke-2) y la presentación de Aplicando y de los avisos.
- **La selección implícita.** `EXECUTEFUNCTION` la cancela con dos `*Cancel*` (Smoke-NAV): ver su efecto en la sincronización K = 1 tras POST.
- **`MutationDestinationBinding`** usa tipos de Selectivo: para Push Back es un «patrón», y el MATCH completo de la base ya cubre la pertenencia.
- **C-F4-05** con `eraseByKind` nulo da `MutateNow` cuando supervivientes y borrados son cero (`RackSiblingRedrawPlan.cs:91-95`).

### M-13 (MENOR). C2-M-1 y C2-M-2 en T4A2

T4A2 no toca `WorkspacePanelCommand.cs`. Corregirlos ahí es aceptable como corrección solo de guarda («van primero», F4-E50), pero hay que decir que es una elección, y que los `RequiredTests` de A2 incluyen el proyecto Core.

### M-14 (MENOR). Falta la nota de custodia

La orden pide registrar la nota de custodia legible en el **siguiente** commit documental útil, sin reescribir F4-E50, con los cuatro puntos del script auxiliar. El commit que custodie este paquete y F4-E51 es ese commit, y el paquete no lo menciona.

---

## Decisiones para el Owner

Ninguna **obliga** a volver antes de emitir T4A1 según la lista de la orden. Se recomiendan tres avisos, ninguno bloqueante si se aplican las correcciones:
1. **La división A1/A2 y su efecto de presupuesto** (I-11), con aviso o pregunta como en F4-E40 y F4-E47.
2. **O-04 frente a la autoridad de D-12** (I-9): T4A entrega O-04, y la autoridad de D-12 sigue en MUTATE, salvo que la variante caller-owned pase a T4A1.
3. **`LegacyDeltas`** (M-11), para cerrar ya la condición de R-T4B-07.

**Si se fija T4C ahora** (I-10), está dentro del criterio del Owner y basta con informar.

**Lo que volvería al Owner más adelante:** la medición en host (callback diferido o Undo en más de un paso). Es una A-n material de D-13 y C-F4-02, y su vehículo hay que fijarlo (I-10).

## Lo comprobado

**Núcleo de borradores** (`src/RackCad.Application/Workspace/Drafts/`):
- `DraftSession.cs:220-629`:
  - los tres estados (`:266`, `:269` y `:272`);
  - `IsDirty`, `Status`, `NeedsLazyReload` y `CanEdit` (`:283-317`);
  - `DiscardAndReload` y `KeepMineOnCurrentVersion` (`:407-443`) y `Rebase` (`:446-457`);
  - `TryBeginApply` (`:467-483`), `CompleteApply` y `Classify` (`:490-565`);
  - `CheckNow` (`:571-589`) y `EnterError` (`:601-616`).
- `DraftStaleBase.cs:60-171`: `Create` y `CompareWith`, ordinal y sin normalización.

**Piloto** (`src/RackCad.Application/Systems/PushBack/Pilot/`):
- `PushBackPilotMaterializer.cs` completo: `ComposeForWrite`, con los arrastres (1), (3) y (6) pendientes, y `KeepPilotEditsOn`.
- `PushBackPilotIntentResult.cs`: sin catálogo; lleva la base siempre.
- `PushBackPilotIntentReader.cs:55-160`.

**Host** (`WorkspaceHostCore.cs`):
- campos y literales (`:38-75`);
- el drenaje (`:164-220`);
- `TryBeginPilotEdit`, `TryCheckPilotEdit` y `TryReloadPilotEdit` (`:280-512`);
- el puerto explícito (`:553-762`);
- `ViewOfPilotDraft` y R-3 (`:861-911`);
- `PassGate` (`:1034-1068`).

**Puerto:** el comentario de reglas de `IWorkspacePilotEditPort.cs:14-24`.

**Plugin:**
- el lector `WorkspaceNavigationHost.cs:218-252`, con Busy si hay comando;
- `RackSiblingScan.cs:112-113` y `:163`;
- `RackPushBackCommands.cs:435-461`, con `BuildPushBackPayload` sobre primitivas de Application;
- los implementadores del puerto (`git grep`), con los dos dobles de Core omitidos (I-6).

**Guardas:**
- `WorkspaceEditIntentGuardTests.cs:540-700`: el orden, una lectura y `AllowedConsumers`;
- `WorkspacePilotEditGuardTests.cs:30-520`: operaciones como cota inferior, lecturas, compuerta y controlador;
- `WorkspaceHostHardeningGuardTests.cs:355-480`: `PilotFieldView` y los 28 literales con su cuenta;
- `WorkspaceEditIntentHostTests.cs:960-1030`: `EditCheckStatus` exacto y los portadores;
- `WorkspaceDraftBoundaryTests.cs:190-300` y `:500-540`: nombres, pureza y valor de retorno;
- `WorkspaceDraftPilotIntentGuardTests.cs:1-120`;
- `WorkspaceCommandAdvisorGuardTests.cs:405-445`: la lista blanca de llamadas;
- `WorkspaceNavigationHostTests.cs:425-437`;
- las pruebas `WorkspaceDraftPilotIntentStaleBaseTests.cs:540-625` y `WorkspaceDraftPilotMaterializationTests.cs:670-740` (`LegacyDeltas`).

**Autoridades:**
- Proposal V5 `:400-560` (D-10 a D-13), `:744-749` y `:768-770`;
- IC-08 (`I-64-implementation-clarifications.md:145-151`);
- el paquete de F4 v2 (§1-§7);
- el paquete de T2 (`:20-80` y `:130-200`, con T2H-07, D-T2-03 y la clausura);
- `f4-dispositions.md`: F4-E18, F4-E24, F4-E25, F4-E38, F4-E40, F4-E47 y F4-E50;
- las órdenes `owner-f4t3-authorization.md` y `owner-f4t3c2-authorization.md`;
- los cierres de F4-T3C1 (R-3) y F4-T3C2 (C2-M-1 a C2-M-8).

**Hechos de host y de CI:**
- `EXECUTEFUNCTION` (`I-64-evidence.md:5912` y el registro de Smoke-NAV);
- la CI compila el Plugin sin AutoCAD (`.github/workflows/ci.yml:254`).

**Hechos del paquete que se confirman:**
- H-01 a H-04, H-06, H-07, H-09, H-12 y H-14 a H-16;
- H-02, con la precisión de que la semántica es solo para Dirty;
- H-05 incompleto (I-12) y H-08 parcialmente falso (I-8).
