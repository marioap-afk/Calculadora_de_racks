# I-64 — Re-revisión adversarial independiente del paquete de F4-T4 (v2)

**Objeto:** `I-64-f4-t4-gate-contract-package.md` v2. Se contrasta con `_package-v1.md`, con su revisión `independent-review.md` (1/12/14), con la orden literal `owner-f4t4-preparation-authorization.md` y con las cuatro decisiones de `owner-f4t4-decomposition-decision.md`, que no se reabren. Los mapas (`map-1-legacy-write-i55-undo.md`, `map-2-drafts-freeze.md` y `map-3-host-seams-guards.md`) se usaron solo como pistas.

**Código revisado:** rama `architecture/workspace-persistente-rackcad`, HEAD `7297e1f1`, con el código de `4e34bb56`. `git diff 4e34bb56 7297e1f1 -- src tests` está vacío.

**Método:** solo lectura, con `git grep`, `git ls-files`, `git show` y lectura de archivos. Sin build, sin pruebas, sin AutoCAD y sin subagentes. Cada cita `archivo:línea` está comprobada sobre el worktree.

## Veredicto

| Objeto | Veredicto |
|---|---|
| **Descomposición de F4-T4** (A1, A2, B y C) | **EMITIBLE.** Aplica con fidelidad las cuatro decisiones del Owner (ver la tabla) |
| **Contrato de F4-T4A1-PREPARE-CORE** | **EMITIBLE TRAS CORRECCIONES.** N-1 hace que el contrato, tal como está, no se pueda cumplir dentro de su `AllowedWriteScope`. Se corrige con un cambio de alcance y una guarda declarada, sin rediseño y sin el Owner. N-2, N-3 y N-4 son decisiones de redacción y de alcance. Basta con que la sesión compruebe N-1 a N-4 en la v3 antes de emitir; no hace falta otra ronda completa |

**Recuento de hallazgos nuevos:** 1 BLOQUEANTE, 3 IMPORTANTE y 11 MENOR.

**Estado de la v1 (27 hallazgos):**
- 18 resueltos, cuatro de ellos por decisión del Owner;
- 9 parcialmente resueltos;
- ninguno sin resolver.

La tabla del §9 de la v2 dice «Aceptado» en todos, pero en cinco casos promete más de lo que entrega: B-1, I-2, I-7, M-5 y M-6.

**Vuelta al Owner antes de emitir T4A1:** ninguna condición de la lista «Return to Owner only if…» (`owner-f4t4-preparation-authorization.md:229-237`) obliga a volver:
- la autoridad de escritura no cambia;
- PREPARE sigue siendo de solo lectura, porque Application solo referencia Domain (`src/RackCad.Application/RackCad.Application.csproj:6`);
- no entra trabajo de F5;
- Undo se mide en T4C.

Con dos matices:
- **N-4:** si la sesión quisiera que el piloto borre vistas fantasma preexistentes, eso sí sería una decisión material del Owner, por «applying ONLY pilot-owned changes» (`:61`). La corrección propuesta lo evita: rechazo tipado y ninguna escritura.
- **N-2:** cambiar INV-F4T1-11 es una disposición de la sesión que hay que registrar. No es materia del Owner, porque D-12 admite la lectura.

---

## Estado de los 27 hallazgos de la v1

| Hallazgo | Estado | Dónde en la v2 | ¿El §9 dice la verdad? |
|---|---|---|---|
| B-1 Prepare inservible bajo Applying | **Parcialmente resuelto.** Lo bloqueante está cerrado: la instantánea con `ConsideredStatus` sirve bajo Applying. Quedan dos restos | D-T4A1-04 | En parte. «NothingToApply → sin `TryBeginApply`» choca con R-T4-06 (N-8). «El estado derivado» y la operación interna del host quedan sin fijar (N-7) |
| I-1 NotAdmitted antes de Conflict | **Resuelto** | D-T4A1-04, resultados 3-5 | Sí. Coincide con `WorkspaceDraftPilotIntentStaleBaseTests.cs:551-615` |
| I-2 Plan delgado y sin expectativa de POST | **Parcialmente resuelto** | D-T4A1-04 | No del todo. La composición de los sobres rompe un censo exacto (N-1). La regla de Redraw/Erase parte de un hecho falso (N-4). La representación del plan no garantiza la inmutabilidad (N-9) |
| I-3 «Conservar» no atómico | **Resuelto** | D-T4A1-02 | Sí. Hay detalles en N-13 |
| I-4 R-3 no discriminado | **Resuelto** | D-T4A1-06 | Sí |
| I-5 La vista no basta para A2 | **Parcialmente resuelto** | D-T4A1-03 | En parte. Faltan los tipos de los dos campos nuevos, y `UnknownOrigin` no puede ser `DraftUnknownOrigin`. La «causa del último intento» que pedía I-5 no entra, aunque R-A2-02 la exige (N-6) |
| I-6 Dobles de Core omitidos | **Resuelto** | §5; D-T4A1-03 | Sí. La cuenta y el patrón de los dobles están en N-10 |
| I-7 Guardas no extendidas | **Parcialmente resuelto** | D-T4A1-07 | No del todo. Falta la guarda de la marca «conocido limpio» y la de contención de excepciones, y su archivo no está en el alcance (N-3) |
| I-8 Lector inválido en contexto de comando | **Resuelto** | H-08; R-T4-01; R-T4-06 | Sí. La precisión sobre el tercer lector está en N-15 |
| I-9 O-04 frente a D-12 | **Resuelto por decisión del Owner (4)** | §3; R-T4-01 | Sí, aplicada con fidelidad |
| I-10 T4B demasiado amplia | **Resuelto por decisión del Owner (2)** | §3; §7 | Sí. T4B y T4C tienen el contenido de la opción elegida, y la medición en host es el primer paso de T4C (R-T4-05) |
| I-11 División A1/A2 | **Resuelto por decisión del Owner (1)** | §3 | Sí, con el efecto de presupuesto registrado en F4-E51 |
| I-12 Arrastres (2) y (5) | **Resuelto** | H-05; D-T4A1-02; R-A2-05 | Sí |
| M-1 Literal del arrastre (1) | **Resuelto** | D-T4A1-05 (1) | Sí para (1). Las notas sobre (3) y (6) están en N-12 |
| M-2 Igualdad del plan | **Parcialmente resuelto** | D-T4A1-04 | En parte. Un plan con `RackEmbedDocument` sería mutable y compartiría `ExtensionData` (N-9) |
| M-3 Clave del origen | **Parcialmente resuelto** | D-T4A1-04, resultado 6 | En parte. La regla es ambigua (N-9) |
| M-4 Sobres de la relectura | **Resuelto** | D-T4A1-04 | Sí. Es coherente con `I-64-f4-t2-gate-contract-package.md:152`: la relectura de MUTATE solo compara |
| M-5 Cuatro interpretaciones de `ConfirmBase` | **Parcialmente resuelto** | D-T4A1-01 | No. Declara la lectura «confirma su estado», pero no declara que contradice INV-F4T1-11 (N-2) |
| M-6 Operaciones poco especificadas | **Parcialmente resuelto** | D-T4A1-03 | No. «Reloaded, nuevo, al final» es imposible: ya existe (N-5) |
| M-7 Sinónimos de literales | **Resuelto** | D-T4A1-03, «Textos» | Sí. La guarda de los textos nuevos está en N-11 |
| M-8 Confirmaciones y despacho | **Resuelto** | D-T4A1-03; R-A2-02 | Sí |
| M-9 Reubicación de T1B | **Resuelto** | §3.1 | Sí. El enunciado de la clausura está en N-14 |
| M-10 Importación fuera de PREPARE | **Resuelto** | §3.2 | Sí |
| M-11 `LegacyDeltas` | **Resuelto por decisión del Owner (3)** | §3; R-T4-03; §8 | Sí |
| M-12 Huecos de T4B | **Resuelto** | R-T4-02, R-T4-04, R-T4-07 y R-T4-09 | Sí |
| M-13 C2-M-1 y C2-M-2 en A2 | **Resuelto** | R-A2-04 | Sí |
| M-14 Nota de custodia | **Parcialmente resuelto** | §8 | En parte. No recoge los cuatro puntos literales que pide la orden (N-15) |

**Las decisiones del Owner, comprobadas:**
- **1 (A1/A2):** coincide con la opción elegida. A2 recibe además el arrastre (5), que es compatible con ella.
- **2 (T4B + T4C):** el reparto de §3 y §7 coincide con el texto de la opción, con la medición en host como primer paso y la A-n.
- **3 (`LegacyDeltas`):** se declara en T4B (R-T4-03) y se observa en Smoke-2 con un dibujo antiguo (§8).
- **4 (lectura caller-owned):** va en T4B (R-T4-01), con O-04 en T4A y la autoridad de D-12 en MUTATE (§3).

---

## Hallazgos nuevos

### N-1 (BLOQUEANTE). El planificador de D-T4A1-04 añade un llamador a un censo exacto de `RackEmbedComposer.Compose` que está fuera del `AllowedWriteScope`

**Referencia:** v2 D-T4A1-04 («por cada miembro Mutable, `RackEmbedComposer.Compose` sobre el sobre releído»), H-11 y §5.

**Código:**
- `tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs:69-75` (`TGrd02_CensoDeCompose…`) compara **con igualdad exacta** todas las llamadas a `RackEmbedComposer.Compose` de `src/`, Application incluida, con `ExpectedComposeCensus` (`:191-203`).
- `:85-107` exige además que el primer argumento sea un **parámetro** de tipo `RackEmbedDocument` del miembro que contiene la llamada.
- El comentario de `:186-190` recoge el precedente de I-55 G16, que añadió una entrada declarada.

**El problema:**
- Cualquier llamada nueva en `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs` pone en rojo esa prueba, que es de I-54 y está fuera del filtro `RackCad.Tests.Workspace` pero dentro de la CI.
- El archivo de la prueba no está en el `AllowedWriteScope` exacto de §5. El Worker solo puede elegir entre una CI en rojo y una escritura fuera de alcance: el intento falla de las dos maneras.
- El compositor puro que ya figura en el censo, `RackViewEnvelopeComposition.Compose` (`src/RackCad.Application/Views/Preparation/RackViewEnvelopeComposition.cs:10-35`), no es una salida. Exige un `RackPreparedView<T>` con constructor `internal` (`src/RackCad.Application/Systems/Shared/RackViewPreparation.cs:21`), que solo produce la tubería de preparación.
- H-11 comprobó la guarda de capas de `WorkspaceHostSourceGuardTests.cs:373-378`, pero no este censo.

**Corrección:**
- Añadir `tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs` al `AllowedWriteScope`, con el cambio declarado del censo: **una** entrada nueva con la ruta del archivo del plan, el nombre del miembro y su parámetro.
- Clasificarla como «rack existente que compone con su sobre real (el releído)», según D-21 de I-54.
- Obligar a que la llamada viva en un miembro propio con un parámetro `RackEmbedDocument source`, por `:105`.
- Alternativa: sacar la composición de los sobres del plan y dejarla en T4B. Es peor, porque deshace I-2.

### N-2 (IMPORTANTE). `ConfirmBase` saca de Conflicto con un MATCH: contradice INV-F4T1-11 sin declararlo

**Referencia:** v2 D-T4A1-01 («Si estaba en Conflict, se quitan el estado explícito… y el estado vuelve al derivado») y la disposición de M-5 en §9.

**Código:**
- INV-F4T1-11 del contrato aceptado de F4-T1 dice: «Conflicto conserva el borrador y solo se sale con DiscardAndReload o con KeepMineOnCurrentVersion confirmado» (`docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/gate-contract-attempt3.json:213`).
- La prueba `AfterCheckNowFoundAConflictTheDraftOnlyLeavesThroughTheReconciliationActions` (`tests/RackCad.Tests/Workspace/WorkspaceDraftApplyTests.cs:701-716`) lo fija por su nombre.
- D-12 solo dice «deja el borrador en Conflicto o confirma su estado» (`docs/initiatives/I-64-proposal-v5.md:486`).

**El problema:**
- La v2 añade una tercera salida del Conflicto, sin reconciliación. M-5 la consideró aceptable siempre que se declarase, y la v2 declara la lectura de D-12, pero no el cambio de un invariante aceptado de una tarea cerrada.
- Un verificador que contraste A1 con INV-F4T1-11 lo rechazaría.
- Quedan además sin fijar tres cosas:
  - Conflict con una diferencia nueva: ¿se sustituyen `ConflictDifferences`? ¿Se conserva `LastApplyCause`?
  - La marca de posiblemente obsoleto en un Limpio que pasa a Conflicto.
  - `LastApplyCause` en Clean o Dirty con MATCH.

**Corrección:** elegir una de dos.
- **(a)** En Conflict, `ConfirmBase` con MATCH responde Confirmed **sin salir de Conflicto**: la salida sigue siendo una reconciliación explícita. Cumple INV-F4T1-11 sin cambiarlo.
- **(b)** Declarar en F4-E51 el cambio de INV-F4T1-11, con una nueva salida «MATCH de Comprobar ahora», su motivo (la base vuelve a coincidir y no hay nada que reconciliar) y una prueba nueva. Anotar que el nombre de `WorkspaceDraftApplyTests.cs:702` queda desfasado y se conserva por alcance.

En los dos casos hay que fijar los tres detalles de arriba.

### N-3 (IMPORTANTE). D-T4A1-07 no recoge todas las guardas que fijan las operaciones del puerto, y una de ellas está fuera del alcance

**Referencia:** v2 D-T4A1-07, D-T4A1-03 («quita `_knownClean` cuando cambia el borrador») y §5.

**Código:**
- **`tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:438-511`:**
  - el comentario de `:446-447` exige que «a new operation that changes a draft must call NotePilotEdit or remove the mark itself (guard below)»;
  - pero la guarda recorre una lista **fija** de cinco operaciones (`PortMutatingOperations`, `:462-469`), y su `ChangesADraft` (`:471`) ni siquiera reconoce `.KeepMineOnCurrentVersion(` ni `.ConfirmBase(`.
- **`tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:575-589`:** la contención de excepciones como Failed solo recorre los cuerpos de `TryBeginPilotEdit` y `TryCheckPilotEdit`.

**El problema:**
- La obligación de la v2 deja sin guarda dos invariantes que la propia v2 exige a las tres operaciones nuevas: el fin de la marca y la contención como Failed sin degradar la sesión.
- `WorkspaceLifecycleGuardTests.cs` no está en el `AllowedWriteScope`, así que el Worker no puede extenderla aunque quiera.
- Es el mismo defecto de I-7.

**Corrección:**
- Añadir `WorkspaceLifecycleGuardTests.cs` al alcance.
- Extender `PortMutatingOperations` a `TryKeepMine`, `TryDiscardAndReload` y `TryConfirmBase`, y `ChangesADraft` a `.KeepMineOnCurrentVersion(` y `.ConfirmBase(`, con su autoprueba en negativo.
- Extender `:575-589` a los tres cuerpos nuevos del host.
- Enumerarlas en D-T4A1-07.

### N-4 (IMPORTANTE). La regla de Redraw/Erase parte de un hecho falso y deja abierta una escritura que el piloto no posee

**Referencia:** v2 D-T4A1-04: «recalculada desde el `CurrentDraftState` con `RackViewAvailability`. Si Push Back no tiene hechos de disponibilidad, todo es Redraw, declarado».

**Código:**
- Push Back **sí** tiene hechos: `PushBackViewAvailabilityFacts` (`src/RackCad.Application/Systems/Shared/RackViewAvailability.cs:94`).
- RACKEDITAR los usa:
  - los construye en `src/RackCad.Plugin/RackPushBackCommands.cs:269-271`;
  - marca obsoleto un lateral cuyo poste ya no existe (`:322-333`);
  - lo borra si quedan supervivientes (`:348-356`).
- D-13 incluye «los borrados de fantasmas» en lo authored atómico (`docs/initiatives/I-64-proposal-v5.md:502`).
- La orden del Owner pide «applying ONLY pilot-owned changes» (`owner-f4t4-preparation-authorization.md:61`).
- El paquete de F4-T2 dice que «T4 nunca arma un diseño directamente sobre un estado del borrador» (`docs/initiatives/I-64-f4-t2-gate-contract-package.md:153`).

**El problema:**
- Con los hechos reales, un dibujo con un lateral fantasma **preexistente** da un Erase en el plan, y T4B borraría una vista por un cambio de escala. Es una escritura authored que el piloto no posee.
- La v2 lo despacha con una condición que no se da.
- «Desde el `CurrentDraftState`» copia la letra de D-13 (`:472`, `:513`), pero el sistema que hay que evaluar es el del diseño saliente (`ComposeForWrite` y `BuildDesign`), no el estado del borrador.

**Corrección:**
- Calcular la disponibilidad sobre el sistema resuelto del diseño saliente, con `PushBackViewAvailabilityFacts`.
- **Si el conjunto Erase no está vacío, Prepare da Invalid** (fuera del piloto, con la indicación de RACKEDITAR) y no hay escritura. Así el piloto solo redibuja vistas existentes.
- Si se prefiere que el piloto borre fantasmas, es una decisión material que va al Owner.

### N-5 (MENOR). `Reloaded` ya existe: no puede ser «nuevo, al final»

- **Referencia:** v2 D-T4A1-03 (`TryDiscardAndReload`: «el resultado nuevo es **Reloaded**, al final») y la disposición de M-6.
- **Código:**
  - `EditIntentStatus.Reloaded` ya es el último miembro (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:123-124`);
  - la fábrica `EditIntentOutcome.Reloaded` está en `:188-189`;
  - el host la usa en `WorkspaceHostCore.cs:494` y la UI en `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:218`.
  - Hay además dos guardas de prefijo de 10 que la v2 no cita: `WorkspaceLifecycleGuardTests.cs:805-816` y `WorkspaceLifecycleHostTests.cs:1488-1490`. Admiten `Kept` en la posición 10.
- **El problema:** un miembro duplicado no compila. Un Worker que obedezca «nuevo» puede inventar otro nombre y desalinear el despacho de A2.
- **Corrección:** «`TryDiscardAndReload` reutiliza `EditIntentStatus.Reloaded` (existente); solo `Kept` es nuevo, en la posición 10». Hay que citar las tres guardas de prefijo.

### N-6 (MENOR). Los dos campos nuevos de `PilotFieldView` no tienen tipo, y falta la causa

- **Referencia:** v2 D-T4A1-03 (`UnknownOrigin`: null, CommitThrew o PostReread; `ConflictText`) y R-A2-02 («El Conflicto muestra la causa y qué cambió»).
- **Código:**
  - El puerto no puede nombrar `Draft…`: `DraftOrPilotNames` (`WorkspacePilotEditGuardTests.cs:73`, `:99`) y la reflexión de `:171-197`.
  - Tampoco puede declarar un tipo cuyo nombre sea una palabra de `Drafts/`: `UnknownOrigin` lo es (`DraftSession.cs:297`; `WorkspacePilotEditGuardTests.cs:747-754` y `:762`).
  - Las listas del puerto nunca son null, ni siquiera en el valor por defecto (`IWorkspacePilotEditPort.cs:15`).
- **El problema:**
  - `DraftUnknownOrigin?` es la elección obvia y no pasa las guardas.
  - No se sabe si `ConflictText` es una cadena o una lista.
  - La «causa del último intento» (`LastApplyCause`, `DraftSession.cs:303`), que pedía la corrección de I-5, no entra, aunque R-A2-02 exige mostrar «la causa».
- **Corrección:**
  - Fijar un enumerado del puerto, por ejemplo `PilotUnknownOrigin { None, CommitThrew, PostReread }` con None como valor por defecto, o un `string?`.
  - Fijar `ConflictText` como `IReadOnlyList<string>` nunca null, o como cadena.
  - Decidir ahora si `LastApplyCause` entra en la vista, o declarar que llegará con T4C mediante otro cambio declarado de la guarda.

### N-7 (MENOR). La instantánea: «estado derivado», la base en el host y una operación sin definir

- **Referencia:** v2 D-T4A1-04 (`ConsideredStatus`: «el estado derivado, o `StatusBeforeAttempt`»; «una operación interna del host para T4B»).
- **El problema:**
  - **«Derivado».** El propio paquete llama «derivado» a la parte Clean/Dirty sin el estado explícito (H-01; D-T4A1-01, «vuelve al derivado»; `DraftSession.cs:285`). Leído así, un Conflicto entra como Dirty y el resultado 1, NotApplicable, es inalcanzable.
  - **La base en el host.** Si el host construye la instantánea, nombra `draft.StaleBase`, y `BaseOrReadUse` (`WorkspaceEditIntentGuardTests.cs:635-648`) lo rechaza salvo que la sentencia exacta se admita. «El planificador» no la describe.
  - **La operación interna.** «Una operación interna del host para T4B» no tiene firma. El host no tiene catálogo, porque solo lo carga el lector del Plugin (`WorkspaceNavigationHost.cs:228`), y la operación no tiene llamador en A1: sería código muerto sin guardas.
- **Corrección:**
  - `ConsideredStatus` = `Status` (el efectivo) fuera de Applying, y `StatusBeforeAttempt` bajo Applying.
  - Una fábrica de la instantánea en el archivo del plan, por ejemplo `PushBackPilotPrepareInput.From(DraftSession<PushBackPilotDraftState>)`, para que el host no nombre `.StaleBase`. `Systems/PushBack/Pilot` ya puede nombrar Drafts (`PushBackPilotIntentResult.cs:3`).
  - Quitar la operación interna de A1 o fijar su firma.

### N-8 (MENOR). NothingToApply bajo Applying no tiene salida en la tabla

- **Referencia:** v2 D-T4A1-04, tabla («NothingToApply → sin `TryBeginApply`»), frente a R-T4-06 («`TryBeginApply` antes de PREPARE»).
- **Código:** `TryBeginApply` acepta un Clean (`DraftSession.cs:471`), y `ConsideredStatus` vale `StatusBeforeAttempt`.
- **El problema:** si T4B sigue R-T4-06, toma la ranura en un Clean y Prepare da NothingToApply. La tabla dice «sin `TryBeginApply`», ninguno de los ocho resultados le sirve y la ranura tiene que liberarse exactamente una vez.
  - PendingText y NotApplicable no tienen este problema: `TryBeginApply` los rechaza (`:471-473`).
- **Corrección:** fijar que T4B decide NothingToApply (Clean) **antes** de `TryBeginApply`, y que bajo Applying es inalcanzable; o asignarle un resultado de cierre.

### N-9 (MENOR). La representación del plan no garantiza la inmutabilidad y deja ambigüedades

- **Referencia:** v2 D-T4A1-04 (los sobres por vista; «el diseño saliente serializado»; la clave del origen, M-2 y M-3).
- **Código y problema:**
  - **Los sobres.** `RackEmbedComposer.Compose` devuelve un `RackEmbedDocument` mutable que **comparte** el `ExtensionData` del sobre fuente (`RackEmbedComposer.cs:48-59`). Un plan que lo guarde no es inmutable, y la expectativa de POST tiene que compararse por texto.
  - **El diseño.** `WithSourceMetadataFrom` cambia por vista (`RackPushBackCommands.cs:278`, `:296`, `:335` con `preflight.ResolvedByBlock[...]`), así que el diseño serializado es uno **por vista**, no uno solo.
  - **La serialización.** La que cita la v2 existe como primitivas de Application:
    - `RackProjectStore.Serialize` sobre `RackProject.ForPushBack(design).WithSourceMetadataFrom(innerSource)` y luego `RackEmbedStore.Serialize` (`RackPushBackCommands.cs:456-460`);
    - con la comprobación previa del proyecto interior, `RackProjectStore.PreflightInnerSources` (`src/RackCad.Application/Persistence/RackProjectStore.cs:228`; uso en `RackPushBackCommands.cs:221-226`).
    - `BuildPushBackPayload` es del Plugin (`:439-461`) y el planificador no puede llamarlo.
  - **`Compose`** recibe también `view` (`RackEmbedComposer.cs:43-44`), que la v2 omite al decir «el `id`, el `name` y la `section`».
  - **M-3.** No queda claro si Invalid se da cuando ningún miembro tiene `EnvelopeText == initial.Origin` o cuando difiere de `reread.OriginKey` (`PushBackPilotIntentResult.cs:50`). Lo segundo da Invalid falsos si la relectura eligió otro origen.
- **Corrección:**
  - El plan guarda, por clave y en orden ordinal, el texto serializado de cada sobre y de cada diseño.
  - `PreflightInnerSources` abortado da Invalid.
  - `view` sale del sobre releído.
  - Invalid solo si ningún miembro Mutable tiene ese texto.

### N-10 (MENOR). Los dobles: la cuenta y el patrón

- **Referencia:** v2 D-T4A1-03 («los **cuatro** implementadores de pruebas… responden de forma inerte o con un respondedor opcional»).
- **Código:**
  - Son **tres**: `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:25`, `WorkspaceCommandAdvisorTests.cs:17` y `:1104`.
  - `CommandAdvisorPortDouble` registra toda operación no permitida **y lanza**, para que «el conjunto exacto de operaciones» sea el oráculo de cero escrituras (`:13-15`, `:47-53`).
- **Corrección:**
  - Corregir la cuenta.
  - Prescribir `Refuse<T>(nombre, request.Session)` para `CommandAdvisorPortDouble`, el reenvío a `_calls` para `ForwardingRiskPort` y el respondedor opcional solo para el doble de UI.

### N-11 (MENOR). Los textos de T-A1 quedan fuera de la guarda de literales

- **Referencia:** v2 D-T4A1-03 («Textos» y tabla T-A1).
- **Código:** `LiteralViolations` comprueba G-04 y la cuenta solo sobre las filas de su `Table` (`WorkspaceHostHardeningGuardTests.cs:449-455`) y fija 28 filas (`:447`, `:468-470`).
- **El problema:** si A1-01..A1-03 no entran en la tabla, nada fija que estén en español, que aparezcan las veces previstas ni que no lleven una subcadena de G-04.
- **Corrección:** declarar el paso de 28 a 31 filas, con su original en inglés de referencia, y las cuentas nuevas de los literales reutilizados.

### N-12 (MENOR). Hechos y literales inexactos

- **H-03.** Dice que nada del producto llama a `KeepMineOnCurrentVersion` ni a `DiscardAndReload`. `DiscardAndReload` **sí** se llama (`WorkspaceHostCore.cs:489`), y también `CheckNow` (`:410`).
- **H-12.** El Actualizar llega hasta `RackPushBackCommands.cs:384`, no hasta `:367`.
- **D-T4A1-05 (6).** Añade «infinita» al literal de F4-E25 (`f4-dispositions.md:453`: «NaN, 0 o negativa»). Es coherente con la validación del campo («número finito mayor que cero», `WorkspaceHostHardeningGuardTests.cs:426`), pero hay que declararlo como ampliación.
- **D-T4A1-05 (3).** Parafrasea el literal y pierde «antes de componer, por si cambia el catálogo». Además no dice con qué comparador: debe ser el del piloto.

### N-13 (MENOR). «Conservar»: el orden en el host y el cambio frente a INV-F4T1-04

- **Referencia:** v2 D-T4A1-02 y D-T4A1-03 (`TryKeepMine`: «calculado antes»).
- **Código:**
  - `KeepPilotEditsOn` lanza con null (`PushBackPilotMaterializer.cs:225`).
  - `DraftIntentRead.InitialState` es null si la lectura no está admitida (`DraftSession.cs:41-42`, `:50-51`).
  - INV-F4T1-04 y D-12 dicen que «Conservar» conserva el `CurrentDraftState` (`gate-contract-attempt3.json:206`; Proposal `:489`).
- **El problema:**
  - Si la sustitución se calcula antes de mirar la admisión, una relectura no admitida da Failed en lugar de NotAdmitted.
  - La sobrecarga nueva **sustituye** el actual. Es coherente con IC-07 punto 2 (`docs/initiatives/I-64-implementation-clarifications.md:125-127`), pero no está declarado.
  - «Hace todas las copias (la base…)»: la base no se copia, porque es inmutable.
- **Corrección:**
  - El orden en el host: admisión, luego `KeepPilotEditsOn` y luego la sobrecarga.
  - Declarar la relación con INV-F4T1-04 e IC-07.
  - Corregir la redacción.

### N-14 (MENOR). La clausura de T1B queda enunciada a medias, y A2 no la prohíbe

- **Referencia:** v2 §3.1 y §6.
- **Código:** la clausura completa es más amplia (`docs/initiatives/I-64-f4-t2-gate-contract-package.md:59-65`):
  - `src/RackCad.UI/Systems/PushBack/**`;
  - Controls, Shell, Themes, Editor y Preview de la UI;
  - los subdiálogos;
  - las fachadas de escritura de Push Back del Plugin.
- **El problema:**
  - §3.1 la reduce a la ventana, sus tipos de estado y sus ensambladores.
  - A2 añade botones al panel. Si toca `Themes/` o `Controls/`, dispara la regla de §3.1 antes de T4B.
  - R-A2-01 prohíbe al navegador nombrar `TryCheckNow`, pero no las tres operaciones nuevas, como hace hoy `WorkspacePilotCloseSyncGuardTests.cs:849` con las lecturas.
- **Corrección:**
  - Citar la clausura por referencia a ese §3.
  - Declarar que el `ForbiddenWriteScope` de A2 la incluye.
  - Extender R-A2-01 a las tres operaciones nuevas.

### N-15 (MENOR). Precisiones de §7 y §8

- **El cerrojo de T1.** La orden exige que «The lock already proved in T1 must remain effective» y que «No second Actualizar may start while one is Applying» (`owner-f4t4-preparation-authorization.md:132-133`). R-T4-06 habla de la ranura, pero no cita INV-F4T1-20 (`gate-contract-attempt3.json:222`) ni la negativa `AnotherUpdateInProgress` (`DraftSession.cs:474`) como oráculo de T4B y T4C.
- **El tercer lector.** R-T4-01 fija la equivalencia entre el lector de «Comprobar ahora» y el caller-owned. Debe incluir también el lector de O-04 en contexto de comando de R-T4-06, que es un tercer camino de lectura.
- **La nota de custodia.** §8 la sitúa bien, pero la orden pide «State only» cuatro puntos (`:24-30`), y la v2 no los recoge literalmente.

---

## Lo comprobado

**Núcleo** (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs`):
- los tres estados (`:266-272`), `IsDirty`, `Status` y `NeedsLazyReload` (`:283-291`);
- `DiscardAndReload` y `KeepMineOnCurrentVersion` (`:407-443`), `Rebase` (`:446-457`);
- `TryBeginApply` y `CompleteApply` (`:467-513`), `Classify` (`:515-565`), `CheckNow` (`:571-589`) y `EnterError` (`:601-616`).

**Guardas del núcleo:** admiten `ConfirmBase` y la sobrecarga: los cinco archivos exactos (`WorkspaceDraftBoundaryTests.cs:23-51`), los nombres (`:204-239`), los tipos de las firmas (`:490-524`) y el valor de retorno (`:528-540`).

**Piloto:**
- `PushBackPilotMaterializer.cs` completo, con `ComposeForWrite` (`:189-215`) y `KeepPilotEditsOn` (`:223-232`);
- `PushBackPilotIntentResult.cs:44-59`;
- las guardas de pureza de `WorkspaceDraftPilotIntentGuardTests.cs:16-22`, que solo valen para tres archivos;
- `RackCad.Application.csproj:6`: Application solo referencia Domain.

**Host:**
- las tres lecturas (`WorkspaceHostCore.cs:280-512`) y sus implementaciones explícitas (`:553-593`);
- `ViewOfPilotDraft` (`:861-911`) y `PassGate` (`:1034-1068`).

**Puerto y tipos:**
- `IWorkspacePilotEditPort.cs` completo;
- `WorkspaceEditIntent.cs:77-125` y `:195-220`.

**Guardas del puerto y del host:**
- `WorkspacePilotEditGuardTests.cs:43-439`, `:490-500`, `:664-737` y `:747-829`;
- `WorkspaceEditIntentGuardTests.cs:530-663`;
- `WorkspaceEditIntentHostTests.cs:957-1023`;
- `WorkspaceHostHardeningGuardTests.cs:361-473`;
- `WorkspaceLifecycleGuardTests.cs:438-616` y `:805-825`.

**Dobles:** `WorkspaceCommandAdvisorTests.cs:13-76`, `:372-392` y `:1104-1140`, y `WorkspacePilotEditPortDouble.cs:25-121`.

**Dependencias del planificador:**
- `RackEmbedComposer.cs:34-60` y `RackProject.cs:75-79`;
- `PushBackCompositeEditorAssembler.cs:29-66` y `RackViewAvailability.cs:94-150`;
- `RackViewCodec.Decode` (`RackViewCodec.cs:115`) y `RackProjectStore.PreflightInnerSources` (`:228`);
- el censo de I-54 (`CustomPropertiesEnvelopeGuardTests.cs:62-307`) y `WorkspaceHostSourceGuardTests.cs:373-378`.

**La vía heredada:** `RackPushBackCommands.cs:195-461`, con su disponibilidad, sus borrados y `BuildPushBackPayload`.

**Autoridades:**
- Proposal V5 D-10 a D-14 (`:408-553`);
- el paquete de F4-T2 (`:44-80` y `:140-160`);
- F4-E18, F4-E24, F4-E25 y F4-E26 (`f4-dispositions.md:253-487`);
- INV-F4T1-01 a 27 (`gate-contract-attempt3.json:203-229`);
- IC-07;
- los cierres de F4-T3C1 (R-3) y F4-T3C2 (C2-M-1 a C2-M-8, con su revisión independiente).

**Hechos de la v2 que se confirman:**
- H-01, H-02, H-04, H-06 a H-11 y H-13 a H-18;
- H-03 y H-12 con las precisiones de N-12.
