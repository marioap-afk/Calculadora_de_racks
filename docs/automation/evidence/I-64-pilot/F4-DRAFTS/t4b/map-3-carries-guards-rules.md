# I-64 F4-T4B-MUTATE-POST — Mapa 3: reglas vinculantes, arrastres, menores y guardas

**Base medida:** `c3ae1c97` (custodia del intento 2 de F4-T4A2; producto `ae0a7b40`, sin cambios en `src/` ni `tests/` entre los dos). Lectura sin modificar nada: Read, Grep y `git show`/`grep`. Todas las citas `ruta:línea` son de esa base; las de revisiones antiguas (sobre `4e34bb56` o `bbd9a35e`) se volvieron a medir donde el archivo cambió.

**Estado de partida medido, relevante para emitir T4B:**
- **F4-T4A2 no está cerrada.** La verificación `R20261114T165017Z-4b2a` dio EXECUTION_BLOCKED / STOP (F4T4A2-C04). La línea espera la decisión del Owner con `attempts` 2 de 3 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:1932-1940`).
- **F4-T1B no se ha ejecutado.** Su paquete v3 está preparado y sin emitir (§4).
- **El Owner pidió «Solo paquetes»** de T1B y T4B: nada se emite sin su autorización (`F4-DRAFTS/coordinator/owner-f4t4a2-overnight-decisions.md:31-38`).

Abreviaturas de rutas:
- `T4P` = `docs/initiatives/I-64-f4-t4-gate-contract-package.md`
- `A2P` = `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md`
- `FRZ` = `docs/initiatives/I-64-proposal-v5.md`
- `F4P` = `docs/initiatives/I-64-f4-gate-contract-package.md`
- `DISP` = `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md`
- `COORD` = `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/`
- `EV` = `docs/automation/evidence/I-64-pilot/`
- `TW` = `tests/RackCad.Tests/Workspace/`

---

## 1. Reglas vinculantes

### 1.1 El paquete de F4-T4: descomposición (§3) y reglas de T4B/T4C (§7)

| Id | Texto resumido fiel | Fuente | Dueño |
|---|---|---|---|
| §3 fila T4B | «La transacción atómica, la lectura caller-owned (decisión 4 del Owner), las fachadas, las unidades, la importación y POST, probados con puertos simulados y guardas». Escrituras authored: **Sí** | `T4P:172` | B |
| §3 fila T4C | Contexto de comando, ámbito de mutación propia, UI de Actualizar y medición en host de Undo como primer paso; A-n si Undo no es un paso o el callback es diferido. Escrituras: «Sí, la del adaptador del host» | `T4P:173` | C |
| §3 orden | A1 → A2 → B → C. Cada tarea se cierra antes de emitir la siguiente y empieza en 1 de 3. Los paquetes de A2, B y C se fijan sobre el código real de la tarea anterior | `T4P:175-177` | B, C |
| Decisión 3 | Se acepta la canonicalización `LegacyDeltas`, como en Application. Se declara en T4B y se observa en Smoke-2 con un dibujo antiguo | `T4P:182`; `COORD/owner-f4t4-decomposition-decision.md:15-19` | B |
| Decisión 4 | La lectura caller-owned (INV-39) va en T4B, que la usa dentro de la transacción. T4A entrega O-04, la comparación, la clasificación y el plan. **La autoridad de D-12 sigue siendo la relectura dentro de MUTATE (T4B)** | `T4P:183`; `COORD/owner-f4t4-decomposition-decision.md:21-25` | B |
| §3.1 | T1B se ejecuta antes de la primera tarea que use las fachadas de Push Back para escribir o modifique un archivo de la clausura, «es decir, antes de T4B». `Systems/PushBack/Pilot/` queda fuera de la clausura | `T4P:185-197` | B |
| §3.2 | La importación de biblioteca pasa a T4B, fuera de PREPARE. El residuo OM-5 queda fuera de la transacción authored, y O-04 va antes de importar. **Declarado:** un Conflicto que solo detecte MUTATE deja residuo. La agrupación de Undo de la importación se mide en T4C y en Smoke-2 | `T4P:206-211` | B (medición en C) |
| §3.2 | «A T4B o T4C van INV-30, la parte de escritura de INV-10, INV-39, C-F4-05, las guardas de fachadas y la inyección Debug» | `T4P:213` | B o C (sin repartir) |
| D-T4A1-04 | Correspondencia `Prepare` → T4B: NotApplicable o PendingText sin `TryBeginApply`; NothingToApply se decide antes de `TryBeginApply` y no toma la ranura (bajo Applying, NotAppliedPreflight); Conflict → NotAppliedConflict; NotAdmitted → NotAppliedAdmission; Invalid → NotAppliedPreflight | `T4P:397-405` | B |
| D-T4A1-04 | «Fuera del plan, para T4B y T4C, porque necesitan el dibujo: las capas bloqueadas, las piezas faltantes, la importación y los supervivientes» | `T4P:407` | B y C |
| R-T4-01 | Lectura caller-owned (INV-39): variante aditiva de `CaptureDrawing`, sin bloqueo, transacción ni `Commit` propios, con su guarda. **Equivalencia** (F4-E26): para el mismo dibujo, los tres caminos (lector de «Comprobar ahora», O-04 en contexto de comando y la caller-owned) dan la misma `DraftStaleBase` con la misma `BuildStaleBase` | `T4P:528-530` | **B** |
| R-T4-02 | MUTATE: una transacción poseída por el paso de mutación. Dentro: relectura caller-owned, comparación con `plan.ExpectedBase` (autoridad de D-12) y comprobación de destino; después las unidades con los sobres del plan (INV-10) y `Commit()` en su propio `try`, con la inyección Debug de INV-30. Todo o nada; `TopTransaction == null` al salir. `MutationDestinationBinding` es un patrón de Push Back | `T4P:532-536` | **B** |
| R-T4-03 | La importación de biblioteca va antes de la transacción authored (§3.2). Se acepta `LegacyDeltas` | `T4P:538` | **B** |
| R-T4-04 | POST desde una relectura: `SyncName`, purga, un `Regen` y la relectura, comparada **byte a byte** con la expectativa de POST del plan. Si coincide: `CompleteApply(Applied(postRead))`, `CurrentDraftState := InitialDraftState`, pendiente vacío; Dirty pasa a false solo desde el estado verificado. Si no coincide o falla: `AppliedPostRereadFailed` → Unknown(PostReread), con los avisos de N-5. Si la vía de escritura no conserva los bytes, **STOP a la sesión** (no se debilita la comparación). `CompleteApply` robusto ante un copiador que lanza (F4-E25 (4)). C-F4-05 con `eraseByKind` nulo | `T4P:540-545` | **B** |
| R-T4-05 | Contexto de comando (C-F4-02): `ExecuteInCommandContextAsync` solo en el archivo exacto del adaptador, nuevo en `Plugin/Views`; las carpetas Workspace siguen sin `async`, `await` ni `.Result`; primer paso de T4C: medir en host el callback y el paso único de Undo | `T4P:547-550` | C |
| R-T4-06 | Un único `using LockDocument` alrededor de PREPARE, MUTATE y POST. O-04 dentro, en contexto de comando, con una vía de lectura sin la puerta de reposo y con revalidación de sesión. La ranura y el cerrojo de T1 (INV-F4T1-20) **son oráculo de T4B y T4C**: NothingToApply antes de la ranura; `TryBeginApply` antes de PREPARE y `CompleteApply` en `finally`, exactamente una vez por salida; un segundo Actualizar recibe `AnotherUpdateInProgress` (`DraftSession.cs:474`, hoy `:515`); Error y Descartar no liberan la ranura antes de tiempo | `T4P:552-558` | C (oráculo B y C) |
| R-T4-07 | `DiscardRack`, `DiscardDocument` y `DiscardByUser` no actúan sobre un Applying; Actualizar llama a `NotePilotEdit` (F4-E35) | `T4P:560` | C |
| R-T4-08 | Ámbito de mutación propia (C-F4-04): por sesión y miembros, activo solo en la ventana de mutación, liberado en toda salida, no oculta otro rack; una de las dos vías de C-F4-04 con IC o A-n | `T4P:562-566` | C |
| R-T4-09 | UI de Actualizar: botón, frontera `PilotBoundaryKind.Update`, presentación de Applying y avisos, selección implícita que cancela `EXECUTEFUNCTION`, K = 1 tras POST | `T4P:568-571` | C |
| R-T4-10 | Undo: un solo paso lógico; Undo restaura el previo y Redo el posterior. **Solo lo prueba Smoke-2** | `T4P:573` | C |
| R-T4-11 | Autoridades integradas: `RackSiblingMembership` e I-55, identidad y vistas; `NavigationTargets` solo para navegar; solo se aplican los cambios que posee el piloto; **nunca se borra una vista** (D-T4A1-04, 6 (e)) | `T4P:575-579` | **B** y C |
| R-T4-12 | T1B va antes de T4B (§3.1) | `T4P:581` | **B** |
| R-T4-13 | C2-M-3 a C2-M-7, donde se toquen esas superficies | `T4P:583` | **B** y C |
| §8 | Smoke-2 obligatorio, con los 30 puntos y dibujos reales, uno antiguo para `LegacyDeltas`; «el procedimiento se fija tras T4C» | `T4P:585-587` | Owner (tras C) |

**Hechos del paquete que condicionan T4B:** H-14 (la costura de I-55 es la vía de D-13; una excepción de `Commit()` da `Discarded` sin inyección Debug; `SiblingRedrawSeamGuardTests.cs:13-14` congela su archivo), `T4P:150-152`; H-15 (ninguna API de Undo ni de contexto de comando en el producto), `T4P:154-156`; H-17 (las pistas no llevan handle), `T4P:160`; H-18 (la importación escribe definiciones de bloque, OM-5), `T4P:162`.

### 1.2 Órdenes del Owner

| Id | Texto resumido fiel | Fuente | Dueño |
|---|---|---|---|
| TOCTOU | El resultado de PREPARE no es una autorización permanente de escritura; todo plan es consultivo hasta T4B. **«T4B MUST reread/revalidate again immediately before MUTATE.»** Que T4A2 pase no elimina las comprobaciones de obsolescencia de T4B | `COORD/owner-f4t4a2-authorization.md:307-317` | B |
| Plan inmutable | Si hay un plan para T4B: inmutable, solo valores planos; ningún `DBObject`, `ObjectId`, `Transaction`, `Document` ni objeto mutable del host dentro | `COORD/owner-f4t4a2-authorization.md:320-334` | B (consumidor) |
| Única escritora | «T4B remains the first and only task in F4 authorized to write authored DWG state.» La rotula «F4-T4B — MUTATE / POST / UNDO» | `COORD/owner-f4t4a2-authorization.md:395-400` | B |
| Smoke-2 | «Do not perform final Smoke-2 before T4B closes»; T4A2 prepara los escenarios de reconciliación | `COORD/owner-f4t4a2-authorization.md:403-408` | Owner |
| T4A1 MINORS | Los 14 menores siguen abiertos; cada uno a T4A2 si toca su superficie, a T4B «if mutation/write code owns it», o a endurecimiento posterior. Prioridad: el arrastre que también pasa con el código viejo, los huecos antialias (`pilot?.Read`, alias) y las formas de R-3 (`this.ErrorReason =`, `??=`) | `COORD/owner-f4t4a2-authorization.md:49-70` | A2 / B |
| Oráculo T4B | O la mutación se confirma como una operación lógica, o no queda ningún resultado parcial. El cerrojo de T1 sigue efectivo; ningún segundo Actualizar mientras uno está en Applying; toda salida libera exactamente una vez; Error/Descartar no liberan antes | `COORD/owner-f4t4-preparation-authorization.md:126-135` | B (y C) |
| POST | POST por relectura, nunca «la llamada volvió, así que está bien». Tras el éxito: relectura, nueva StaleBase, nuevo InitialDraftState, alinear CurrentDraftState, vaciar el pendiente; Dirty false solo desde el estado verificado | `COORD/owner-f4t4-preparation-authorization.md:136-147` | B |
| Eventos propios | Los eventos de la propia actualización no aparecen como conflicto externo; el ámbito es de sesión y de rack/miembros, solo durante la ventana de mutación y liberado en toda salida; no se ocultan otros racks | `COORD/owner-f4t4-preparation-authorization.md:148-157` | C |
| Undo | Un Actualizar explícito = un paso lógico de Undo; «Do not claim Undo compliance from unit tests alone» | `COORD/owner-f4t4-preparation-authorization.md:158-165` | C / Smoke-2 |
| Arrastre de T3C1 | `ErrorReason` solo se rellena/presenta en Error, como cambio declarado de T4 | `COORD/owner-f4t4-preparation-authorization.md:166-168` | Hecho en A1 (D-T4A1-06) y A2 (M-4) |
| Arrastre de T3C2 | Los 8 menores de T3C2 entran en la revisión del contrato de T4; prioridad a las guardas Sí/No y `Authorize`; no gastar un intento entero en comentarios | `COORD/owner-f4t4-preparation-authorization.md:169-176` | A2 (C2-M-1, 2), A1 (C2-M-8), B/C (C2-M-3..7) |
| Cierre de T4 | «T4 cannot close on automated tests alone»; Smoke-2 real antes de cerrar F4 | `COORD/owner-f4t4-preparation-authorization.md:178-179` | Owner |
| Descomposición | «T4B + T4C ya (Recommended)» | `COORD/owner-f4t4-decomposition-decision.md:9-13` | — |
| Noche | Tras cerrar A2: «Solo paquetes» de T1B y T4B, sin emitir | `COORD/owner-f4t4a2-overnight-decisions.md:31-38` | — |

### 1.3 El Freeze (`FRZ`) y el paquete de F4

| Id | Resumen fiel | Fuente |
|---|---|---|
| D-12, autoridad | Relectura **dentro de la transacción de MUTATE**, antes de escribir, a través de la misma transacción caller-owned (O3-02); no puede usar el barrido actual, que abre su transacción (`RackSiblingScan.cs:103`, hoy `:112-113`); variante aditiva dentro de las guardas caller-owned (O4-05, INV-39) | `FRZ:465-469` |
| D-12, comparación | (1) pertenencia por clave de la unión base ∪ relectura (MUTABLE/READ_ONLY/BLOCKING/NOT_MEMBER, con `View` y `Section`); Redraw/Erase **no** es base, se recalcula desde el CurrentDraftState; (2) texto crudo exacto de cada sobre; (3) en Selectivo, el registro de variables completo. MATCH solo si todo es idéntico; cualquier diferencia: Conflicto y cero escrituras (INV-06) | `FRZ:470-479` |
| D-12, O-04 | PREPARE repite la comparación en solo lectura **antes de importar**, para no dejar residuo ante un Conflicto evidente. **No sustituye** a la relectura de MUTATE | `FRZ:480-481` |
| D-12, otros | Sin normalización (`:482-483`); «Comprobar ahora» (`:486`); reconciliación explícita, sin fusión automática (`:487-493`); H-LOCK reformulada: importación en PREPARE, relectura y escrituras en la misma transacción bajo bloqueo y en contexto de comando; POST queda fuera (`:494-498`) | `FRZ:482-498` |
| D-13, resultado | Todo o nada sobre el authored del rack (definiciones de vista, sobres y borrados de fantasmas). Residuo admitido: biblioteca importada en PREPARE (OM-5). «La mecánica no se congela» | `FRZ:502-506` |
| D-13, vía | Costura de I-55 (`ISiblingRedrawPort`, `RackSiblingRedrawRun`, `SiblingRedrawTransaction`): 1) compromiso interno y builder; 2) petición ligada a la sesión en contexto de comando, `using` del bloqueo alrededor de PREPARE, MUTATE y POST; 3) PREPARE sin escrituras authored (O-04, pertenencia y admisión, unidades caller-owned con Redraw/Erase desde el CurrentDraftState, capas bloqueadas, **importación fuera de la transacción authored**, piezas faltantes); 4) MUTATE: una transacción; relectura y comparación, comprobación de destino (`MutationDestinationBinding`), unidades que no confirman ni abren transacción (`CallerOwnedFacadeGuardTests`), `Commit()`; `TopTransaction == null` (INV-TX-1); 5) POST: `SyncName`, purga, un `Regen` y relectura que da a la vez StaleBase e InitialDraftState; Current := Initial; Limpio | `FRZ:507-520` |
| D-13, fronteras | Fallo antes de `Commit()` → No aplicado (Conflicto, Admisión, Preflight o Fallo), residuo posible; `Commit()` lanzó → **Desconocido** con relectura obligatoria; retornó → Aplicado; retornó y POST falló o faltantes → **Aplicado con avisos**; retornó y la relectura de POST falló → Aplicado, borrador Desconocido. Desde Desconocido, «Comprobar ahora» recupera el estado previo si coincide con la StaleBase anterior; nunca se supone aplicado | `FRZ:521-532` |
| D-13, INV-30 | La costura convierte hoy toda excepción de `Mutate` en `Discarded`. El panel distingue la excepción en `Commit()` como Desconocido **sin cambiar** el contrato observable para Insertar (cambio aditivo) | `FRZ:533-535` |
| D-13, otros | Cantilever y Cama sin escritor caller-owned (`:536-539`); escritores caller-owned de Push Back sin uso en producto, primera prueba física en Smoke-2 (R-09, `:540-541`); Undo congelado como observable (O-11), alternativa `SendStringToExecute` por A-n (`:542-543`); un solo Actualizar a la vez, pestaña en Aplicando (`:545`) | `FRZ:536-545` |
| INV-10 | Desconocidos y metadatos por vista preservados en el Actualizar del panel. Oráculo Core: sobres con `ExtensionData` distintos por vista, cada vista conserva los suyos. Falla «un recorrido que reutiliza el sobre elegido». F4, F5 | `FRZ:748` |
| INV-12 | Todo o nada en el authored de la vía del panel. Core con escritor transaccional simulado: fallo en la unidad k → cero escrituras authored; host: S2-09. Falla «con commit por vista». F4 | `FRZ:750` |
| INV-14 | Ninguna transacción, bloqueo ni objeto de AutoCAD sobrevive a su operación; `StartTransaction`, `StartOpenCloseTransaction` y `LockDocument` solo en `using`; ningún campo `Transaction`/`DBObject`/`Document`; comprobación Debug de `TopTransaction == null` | `FRZ:752` |
| INV-30 | Clasificación por frontera de `Commit()` sin cambiar la costura para Insertar. Core: excepción antes de `Commit()` → No aplicado; en `Commit()` → Desconocido con relectura; las pruebas existentes de la costura en verde | `FRZ:768` |
| INV-32 | Un Actualizar del panel es un solo paso de UNDO; solo host (S2-05) | `FRZ:770` |
| INV-39 | La relectura de MUTATE usa la misma transacción caller-owned: no abre transacción, no bloquea y no confirma. Guarda de fuente de la costura caller-owned (patrón `CallerOwnedFacadeGuardTests`) ampliada a la variante de lectura; prueba de la costura: una sola transacción en MUTATE | `FRZ:777` |
| OM-5 | En el Freeze solo se cita (`FRZ:504`). Su definición es de I-55: «Bloques importados que sobreviven a fallos (vigente)» (`docs/initiatives/I-55-proposal-v3.md:1429`); en V5, «Sin cambio; en el modo 1 de §4.6 el abort no deja definicion de rack, solo biblioteca importada» (`docs/initiatives/I-55-proposal-v5.md:1043`) y «Las importaciones de biblioteca ya ocurrieron en PREPARE y no se revierten» (`:616`) | ver fuente |
| M-02 | NOT ACTIVATED mientras: sin revisión persistida (D-12); builders, escritores y `Compose` existentes con preservación por vista (D-13, INV-10); sin campos nuevos (D-22); la importación fuera del ámbito atómico no cambia el significado persistido (OM-5). Si una condición se rompe, M-02 se reabre | `FRZ:995` |
| Obligaciones | Variante de lectura caller-owned (O3-02, INV-39) y distinguir la excepción de `Commit()` sin cambiar Insertar (INV-30), ambas «Worker, F4» | `FRZ:1035`, `FRZ:1038` |
| C-F4-05 | Cero vistas supervivientes (H-06): No aplicado, con su causa visible en T4B y sin escrituras. Difiere de RACKEDITAR Push Back (que cuenta supervivientes por bloques): se declara | `F4P:61` |
| C-F4-02 / C-F4-04 | Contexto de Actualizar con `ExecuteInCommandContextAsync` por archivo exacto; ámbito de mutación propia por (sesión, miembros) | `F4P:59-60` |

---

## 2. Arrastres y menores

### 2.1 Lo que el paquete de F4-T4A2 deja a T4B (`A2P` §8, §3 y D-T4A2-10)

| Id | Origen | Superficie de código (en `c3ae1c97`) | Qué pide |
|---|---|---|---|
| §8 (a) | `A2P:655-656` | ver M-1, M-5, M-6, M-10 en §2.2 | Los cuatro menores de T4A1 de D-T4A2-10 |
| §8 (b) diagnóstico en `TryCheckNow` | `A2P:168-178`, `:657`; limitación declarada `:671-673`; F4-E55 `DISP:1913`; nota del intento 1 `EV/F4-T4A2-PREPARE-UI/1/R20261114T160452Z-872b/analysis.md:30` | Host: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:367-428` (el Conflict sin diagnóstico en `:420`). Tipo: `EditCheckOutcome`, `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:237-280` (fábrica `Conflict` en `:268`, sin `AdmissionDiagnostic`). UI: `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:712-740` (`PresentCheck` retira el diagnóstico persistente con `Conflict` y presenta A2-09) | Que un rack borrado, ya no editable o ilegible en la vía Unknown(CommitThrew) no se vea como un Conflicto más. **Rompe por diseño** las guardas de `CheckBody` y M-3 (§3, A-2 y A-5); y, si se factoriza en un auxiliar que recibe `pilot`, también la regla de M-2 (A-4) |
| §8 (c) | `A2P:658`; D-T4A2-09 `A2P:430-435` | — (código nuevo de T4B) | La relectura dentro de MUTATE contra `plan.ExpectedBase` y la relectura caller-owned |
| §8 (d) | `A2P:659`, D-T4A2-10 fila D-1 `A2P:452` | `TW/WorkspaceEditIntentGuardTests.cs:727-728` | D-1, **si** T4B añade una lectura del host (cambio declarado 6 → 7) |
| D-T4A2-06 | `A2P:349` | admisión de Actualizar (D-T4A1-04) | Limitación declarada: el diagnóstico persistente es memoria de la superficie; al volver a la pestaña, «la admisión de Actualizar en F4-T4B lo rechaza (NotAdmitted antes de escribir)». T4B debe garantizar ese rechazo |
| D-T4A2-06 | `A2P:359` | — | «Aplicado con avisos» y Applying son de la UI de Actualizar (T4C); «Solo T4B produce esos estados» |
| §7 S2-R-10 | `A2P:649` | — | El escenario Desconocido de Smoke-2 necesita la inyección Debug de T4B |

### 2.2 Los 14 menores de F4-T4A1

Asignación: `A2P:437-454` (D-T4A2-10) y `DISP:1877-1882` (F4-E54). Contenido: `EV/F4-T4A1-PREPARE-CORE/2/R20261114T043231Z-623b/analysis.md:26-41`, `independent-review.md` y `direct-review.md`. La orden de cierre (`COORD/owner-f4t4a1-close-decision.md:6-9`) los pasa a T4A2, «o de F4-T4B si es esta la primera que toca esa superficie».

| Id | Dueño | Superficie de código (re-medida en `c3ae1c97`) | Qué pide |
|---|---|---|---|
| **M-1** | **T4B** (prioridad del Owner) | `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs:196` (`ComposeForWrite`), `:212-215` (`var neutralInitial = PushBackPilotStateComparer.Copy(initial);` y la comparación con `current` tal cual). La prueba que no discrimina: `TW/WorkspaceDraftPilotMaterializationTests.cs:767` (`CarryOver1_…`) | Guarda de fuente sobre el cuerpo de `ComposeForWrite`: no aplica `Copy(` a `current`, copia `initial`, compara con `current` tal cual; con autoprueba en negativo por la mutación a `Copy(current)`. Corregir el mapa de evidencia (independiente `:52-63`; paquete `A2P:441`) |
| **M-5** | **T4B** | `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs:379-381` (decodifica y evalúa la disponibilidad sin mirar `HasAddress`) y `:285-287` (los textos de vista no disponible). Prueba que fija el texto engañoso: `TW/WorkspacePilotPrepareTests.cs:1758` y `:1772`. RACKEDITAR distingue el caso: `src/RackCad.Plugin/RackPushBackCommands.cs:230-236` | Comprobar `decoded.HasAddress` antes de la disponibilidad, con su texto propio («descriptor de vista inválido: usa RACKEDITAR»); ajustar la prueba de forma declarada (independiente `:110-121`) |
| **M-6** | **T4B** | `PushBackPilotUpdatePlan.cs:308` (`if (reread == null \|\| catalog == null)` antes de `CompareWith`, `:310-311`). Guarda del orden: `TW/WorkspacePilotPrepareGuardTests.cs:194-227`. Prueba de totalidad: `TW/WorkspacePilotPrepareTests.cs:1795-1816` | Dejar solo `reread == null` antes de comparar y mover `catalog == null` al principio de `Build`, para que un catálogo nulo con diferencias dé Conflict (independiente `:123-133`). La guarda de orden no lo impide; la prueba de totalidad sigue dando `MissingInputReason` |
| **M-10** | **T4B** | `src/RackCad.Application/Workspace/Drafts/DraftSession.cs:460-466` (XML de la sobrecarga, «if one of them throws the draft stays exactly as it was»), `:467`, `:480` (`_copy(keepOn(_copy(read.InitialState!), _current))`). Prueba con `Assert.Same`: `TW/WorkspacePilotPrepareTests.cs:411` (cita de la revisión, no re-medida) | Documentar la precondición «keepOn must not mutate its arguments», sin cambiar la fórmula. Va a T4B porque T4B toca `DraftSession.cs` por el arrastre (4) de F4-E25 (`A2P:450`) |
| **D-1** | **T4B / T4C** | `TW/WorkspaceEditIntentGuardTests.cs:719-729` (igualdad exacta: 6 `var pilot = read.PilotRead;` y 6 `.ReadIntent(` en el núcleo, `:727-728`) | Cambio declarado previsto si T4B o T4C añade una lectura del host (directa `:68`; análisis `:39`) |
| M-2 | T4A2 | hecho: `TW/WorkspaceEditIntentGuardTests.cs:647` y `:821-855` | — (T4B debe mantenerlo) |
| M-3 | T4A2 | hecho: `TW/WorkspaceEditIntentGuardTests.cs:857-868` | — (T4B lo rompe si lleva el diagnóstico a `TryCheckNow`) |
| M-4 | T4A2 | hecho: `TW/WorkspacePilotPrepareGuardTests.cs:257-335` (formas `this.`, `??=`, `+=`, deconstrucción) | — (T4B lo mantiene si toca `ErrorReason`) |
| M-7 | aceptado (F4-E53, `DISP:1835`) | `PushBackPilotUpdatePlan.cs` (`ExpectedVariableRegistryText`) | Sin cambio. T4B lo compara en MUTATE como parte de la base esperada |
| M-8 | T4A2 | hecho (resúmenes de `EditConfirmStatus`) | — |
| M-9 | T4A2 | hecho: `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs` (`Reads()`) | — (si T4B añade una operación de lectura al puerto, `Reads()` del doble debe contarla) |
| M-11 | aceptado (F4-E53) | — | Lección: el RED de T4B con cuerpos inertes también en fábricas, igualdades y reenvíos |
| D-2 | endurecimiento posterior | `PushBackPilotUpdatePlan.cs` (`GetHashCode`) | Ninguna tarea lo posee |
| D-3 | endurecimiento posterior | `PushBackPilotUpdatePlan.cs` (`UnreadableOriginReason`) | Ídem |

### 2.3 Los menores de F4-T4A2 (intentos 1 y 2)

| Id | Origen | Estado y dueño | Superficie | Qué pide o afecta a T4B |
|---|---|---|---|---|
| I-1 (importante) | `EV/F4-T4A2-PREPARE-UI/1/R20261114T160452Z-872b/independent-review.md:50-65` | **Resuelto** en el intento 2 (`…/2/R20261114T165017Z-4b2a/independent-review.md:38`) | `TW/WorkspacePilotReconciliationGuardTests.cs:571-584` | Ya no congela T4B: `inside >= 1` y `inFile - inside >= 1`. Es el «no-freeze» de `_authorize` (§2.6) |
| Nota de M-9 para T4B | intento 1, `analysis.md:30`; independiente `:129-133`; F4-E55 `DISP:1913` | **Asignada al paquete de T4B** | `TW/WorkspaceEditIntentGuardTests.cs:680-683`, `:698-699`, `:864-867` | «T4B tendrá que declarar esos cambios de guarda o factorizar la expresión», junto con I-1 |
| M-1..M-9 del intento 1 | `…/872b/analysis.md:16-28` | Resueltos en el intento 2 (dentro de T4A2) | VM, guardas y pruebas de T4A2 | Ninguno queda para T4B |
| D-1 (directa, intento 1) | `…/872b/direct-review.md:28` | Aceptado; «se anota por si F4-T4B cambia esas respuestas» | `WorkspacePilotFieldViewModel.cs` (`PresentChoice`, rama defensiva A2-33) | Solo si T4B cambia las respuestas `Reloaded`/`Kept` de las operaciones del host |
| D-2, D-3 (directa, intento 1) | `…/872b/direct-review.md:29-30` | Aceptados en T4A2 | VM | Ninguno |
| m-1 (re-revisión, intento 2) | `…/2/R20261114T165017Z-4b2a/independent-review.md:62-76` | **Sin disposición:** la línea de T4A2 está parada (F4-E56, `DISP:1932-1942`) | `WorkspacePilotFieldViewModel.cs:683`, `:722`, `:770` (salida temprana sin `_changed?.Invoke()`) | No es de T4B salvo que T4B toque el VM (p. ej. por el diagnóstico de `TryCheckNow`, que pasa por `PresentCheck`, `:712-727`) |
| D2-1, D2-2 (directa, intento 2) | `…/4b2a/direct-review.md:22-23` | Propuesto «se acepta», pendiente del cierre | VM; contrato del intento 2 (INV-13) | Ninguno |
| O-1..O-3 | `…/4b2a/independent-review.md:80-82` | Observaciones, no cuentan | regla 1, `ReconcileText`, INV-13 | Ninguno |
| Parada `4b2a` | `…/4b2a/analysis.md:1-40` | **Abierta**: espera al Owner (opciones a/b/c) | — | Bloquea «Cada tarea se cierra antes de emitir la siguiente» (`T4P:176`) |

**Conclusión medida:** ningún menor de T4A2 está asignado formalmente a T4B, salvo la nota de M-9 (diagnóstico de `TryCheckNow`) y la anotación de D-1 de la directa del intento 1. La asignación de m-1 la decide el cierre de T4A2, que aún no existe.

### 2.4 C2-M-3 a C2-M-7 (de F4-T3C2; R-T4-13)

Definición: `EV/F4-T3C2-COMMAND-ADVISOR/1/R20261113T215110Z-28eb/analysis.md:22-26` y `EV/F4-T3C2-COMMAND-ADVISOR/task-close-package.md:79-83`. Mapa en `EV/F4-T4A1-PREPARE-CORE/coordinator/map-2-drafts-freeze.md:375-379` (líneas de `4e34bb56`; re-medidas aquí).

| Id | Qué es | Superficie en `c3ae1c97` | ¿T4B la toca? |
|---|---|---|---|
| C2-M-3 | Siete guardas nuevas sin autoprueba en negativo; una mira `SessionDestroyed` en lugar de `RememberLoss` | Guardas en `TW/WorkspaceCommandAdvisorGuardTests.cs`: `:306`, `:341`, `:455`, `:468`, `:551`, `:568` (el cuerpo mirado es `SessionDestroyed`, `:576-583`) y `:587`. Producto: `src/RackCad.Application/Workspace/WorkspaceCloseAdvisorRelay.cs:134` (`RememberLoss`) | No, salvo que T4B toque el asesor de cierre |
| C2-M-4 | La guarda de los dos miembros del asesor congela «dos sentencias» | `TW/WorkspaceCommandAdvisorGuardTests.cs:498` (`Src.Count(body, @";") != 2`), prueba `:501-512` | Solo si T4B o T4C añade un miembro a `IWorkspaceLifecycleAdvisor` (cambio declarado previsible) |
| C2-M-5 | Nombres de prueba y comentarios de guardas desfasados | Nombres: `TW/WorkspaceLifecycleGuardTests.cs:295`, `TW/WorkspacePaletteHostSourceGuardTests.cs:321`, `TW/WorkspaceHostSourceGuardTests.cs:50`. Comentarios: `TW/WorkspaceBridgeSourceGuardTests.cs:50-52`, `TW/WorkspaceSourceGuardExceptionTests.cs:10-11` y `:279` | Solo si T4B edita esos archivos (T4 o F7, de forma declarada) |
| C2-M-6 | «the one writer of the command line of the panel» exagera | `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:18` y `:73` | No (es la superficie de comando, T4C) |
| C2-M-7 | La documentación R-4 del VM, cuando la lectura del riesgo lanza | `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:356` | No (UI del navegador, prohibida en T4A2; candidata a T4C) |
| C2-M-1, C2-M-2 | Guardas Sí/No y `Authorize` | Hechas en T4A2 (`TW/WorkspaceCommandAdvisorGuardTests.cs:132-164`, `:220-254`), pendientes del cierre de T4A2 | Mantener |
| C2-M-8 | Teoría de tres casos idénticos | Hecha en T4A1 (D-T4A1-08, `T4P:438-440`) | — |

### 2.5 Los arrastres de `ComposeForWrite` (F4-E25) y los de F4-T1

F4-E25: `DISP:449-459`.

| Arrastre | Estado medido | Dónde | Qué queda para T4B |
|---|---|---|---|
| (1) rechazo con `current` sin normalizar | **Hecho** en T4A1 (D-T4A1-05, `T4P:409-416`; `PushBackPilotMaterializer.cs:210-215`) | — | **No fijado por ninguna prueba**: es M-1 (§2.2) |
| (2) separar el escenario (b) de «Conservar» | Hecho en T4A1 (D-T4A1-02, `T4P:265`) | `WorkspaceDraftPilotMaterializationTests.cs` | Nada |
| (3) base rematerializada igual a `initial` | Hecho en T4A1 | `PushBackPilotMaterializer.cs` | Nada |
| (4) un copiador que lanza en el rebase de Applied deja el estado sobre la base anterior y un `EnterError` diferido sin aplicar | **Abierto**, asignado a T4B «dueña de `CompleteApply`» (`DISP:456-458`); R-T4-04 (`T4P:544`) | `src/RackCad.Application/Workspace/Drafts/DraftSession.cs:531` (`CompleteApply`) | `CompleteApply` robusto ante un copiador que lanza. Va «junto al arrastre ya registrado de `CompleteApply` en `finally` (F4-E16)» |
| (5) aserción sin normalizar del corpus canónico en UI.Tests | Hecho en T4A2 (D-T4A2-08), pendiente del cierre de T4A2 | `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs` | Mantener |
| (6) rechazar escala NaN, 0 o negativa (y, ampliado, infinita) | Hecho en T4A1 (`PushBackPilotMaterializer.cs:205-208`) | — | Nada |
| F4-E07 | `DISP:61` | — | «a T4B: `CompleteApply` en `finally` y el error diferido durante Aplicando». El error diferido ya existe en el núcleo (`DeferredErrorReason`, probado en `TW/WorkspacePilotPrepareGuardTests.cs:366-373`) |
| F4-E16 | `DISP:231` | — | «`CompleteApply` en `finally` y **los avisos de POST visibles aunque falle su relectura**, a T4B» |

**Atención:** F4-E07 y F4-E16 dicen «T4B» con el sentido de la descomposición original de F4 (`F4P:47-48`, donde F4-T4B era «ACTUALIZAR-HOST», con el bloqueo y el contexto de comando). En la descomposición vigente, `CompleteApply` en `finally` es R-T4-06, marcada **(C)** pero «oráculo de T4B y T4C» (`T4P:554-556`). Ver §6.

### 2.6 El «no-freeze» de `_authorize` (regla 5)

- **Sí existe como regla.** Paquete de T4A2, D-T4A2-07 regla 5: dentro de la clase anidada `PilotIntentReader`, «toda aparición de `_authorize(` sigue la forma […]. Hoy es una sola (`:234`), pero **no se exige unicidad, para no congelar una lectura nueva de F4-T4B en esa clase**» (`A2P:387`). Se recoge como INV-F4T4A2-12 «sin exigir unicidad» (`A2P:600`), en la disposición R-B1 (`A2P:711`) y en la de I-1 (`A2P:683`). Origen: `EV/F4-T4A2-PREPARE-UI/coordinator/package-review-v1.md:57`.
- **Su guarda:** `TW/WorkspacePilotReconciliationGuardTests.cs:558-569` (la forma exacta, que termina en `return EditIntentReadResult.Rejected(`), `:571-584` (solo «al menos una dentro y al menos una fuera») y `:602-612` (la autoprueba de no unicidad). `PassGate`, en cambio, exige **exactamente una** `Authorize(` (`:530-535`).
- **Para T4B:** una lectura nueva dentro de `PilotIntentReader` puede añadir su `_authorize`, pero con la forma exacta, que devuelve `EditIntentReadResult.Rejected(`. Una variante que devuelva otro tipo, o que no compruebe `!= RequestDecision.Accepted`, la rompe. Fuera de la clase (`:120` y `:165` de `WorkspaceNavigationHost.cs`) no hay regla.
- **No aparece `_authorize` en `DISP`** salvo en F4-E55 (`DISP:1905`, I-1).

---

## 3. Guardas que T4B tendrá que mantener o extender

Medido: `tests/RackCad.UI.Tests/Workspace/` **no tiene ningún archivo `*Guard*`**. Las guardas de la UI del panel viven en `TW/` y leen la UI como texto. «Rompe» = T4B la incumple si hace lo que su contrato le pide, y la tiene que cambiar de forma declarada.

### A. Lecturas del host, antialias y uso de la base (`TW/WorkspaceEditIntentGuardTests.cs`)

| Archivo:línea | Qué fija | ¿T4B la rompe? | Cómo declararla |
|---|---|---|---|
| `:719-729` (D-1) | Exactamente 6 `var pilot = read.PilotRead;` y 6 `.ReadIntent(` en `WorkspaceHostCore.cs`; ningún otro `.PilotRead` | Solo si T4B añade una lectura por el lector del host (p. ej. O-04 en el host). La caller-owned de MUTATE no pasa por el host | 6 → 7, con la operación nombrada en el comentario de `:726` |
| `:663-684` | Toda base o lectura del piloto solo llega a `Begin`, `DiscardAndReload`, `ConfirmBase`, `KeepMineOnCurrentVersion` o `CheckNow` (`:651`). `CheckBody`: un `.StaleBase` y **cero** `pilot.Read` (`:680-683`) | **Sí**, con el diagnóstico en `TryCheckNow` (necesita `pilot.Read` en `CheckBody`) | Cambio declarado de `:683` y de `AllowedConsumers`/`AllowedAdmissionDiagnostic` |
| `:686-713` | Cuentas por cuerpo; la expresión del diagnóstico solo en `ConfirmBody` y cero en `IntentBody`, `CheckBody`, `KeepBody` y `DiscardAndReloadBody` (`:698-699`); el host nunca nombra `.InitialState` ni `.InitialDraftState` (`:712`) | **Sí** (`:698-699`) por el diagnóstico de `TryCheckNow` | Igual que arriba |
| `:821-855` (M-2) | En el núcleo, `pilot` solo aparece en las formas medidas; `Use(pilot)` es violación | **Sí**, si el host pasa `pilot` entero a una función nueva: `PushBackPilotUpdatePlanner.Prepare(input, pilot, catalog)` o un auxiliar del diagnóstico | Añadir la forma nueva a `PilotForms` (`:823-824`) con su autoprueba |
| `:857-868` (M-3) | La expresión del diagnóstico, exactamente una vez en todo el núcleo | **Sí** con el diagnóstico de `TryCheckNow` (pasaría a 2) | 1 → 2, o factorizar (pero la factorización choca con M-2) |
| `:562-580` | Orden de los pasos y una `ReadIntent` por cuerpo; `PassGate` no lee | No, si no cambia esos cuerpos | — |
| `:598-624` | Ninguna operación de la intención desde el drenaje ni el Idle; `.ReadIntent(` solo en el núcleo | Debe mantenerse | — |
| `:1002-1015` | `WorkspaceHostCore.cs`, `IWorkspaceEditIntentReader.cs` y `WorkspaceEditIntent.cs`, comentarios incluidos: sin `Transaction`, `Database`, `Commit(`; en código, sin `Autodesk.`, `RackCad.Plugin`, `async`, `await`, `Task`, `Guid.NewGuid` | Debe mantenerse: la escritura no puede entrar en esos archivos ni en sus comentarios | — |
| `TW/WorkspaceEditIntentHostTests.cs:1002-1022` | Ningún campo del núcleo es de un tipo de `RackCad.Application.Systems` (el plan lo es) ni `EditIntentReadResult`; **un solo** campo `IWorkspaceEditIntentReader` | **Sí**, si T4B guarda el plan en un campo del host o añade un segundo lector del mismo tipo | Declarado; mejor el plan como valor local de la operación |
| `TW/WorkspaceLifecycleGuardTests.cs:456-458` | `CheckBody` con exactamente 2 `_knownClean`, en `Recovered` y `Conflict` | Posible, si el diagnóstico de `TryCheckNow` cambia esos `case` | Declarado |

### B. Lector del Plugin, barrido y adaptador (lectura caller-owned, R-T4-01)

| Archivo:línea | Qué fija | ¿T4B la rompe? | Cómo declararla |
|---|---|---|---|
| `TW/WorkspaceEditIntentGuardTests.cs:88-118` | `RackSiblingScan.cs`: firmas exactas de `Capture`, `CaptureDrawing(Document document)` y `Traverse(Document, ObjectId, Func<…>)`; constructor de 5 parámetros; **un solo** `new RackSiblingDrawingScan(` y **un solo** `RawEnvelopes =` | **Probable**: la variante caller-owned necesita recibir la transacción. Se evita solo si el cuerpo de `Traverse` se mueve a un interno que reciba la transacción, conservando la firma de `Traverse` y las cuentas en 1 | Si cambia la firma o las cuentas, cambio declarado de `:93-113` |
| `TW/WorkspaceEditIntentGuardTests.cs:120-128` | El barrido no escribe y tiene **un solo** `.Commit(` | No, si la variante no confirma (es lo que pide INV-39) | — |
| `TW/WorkspaceEditIntentGuardTests.cs:189-217` | `RackPilotIntentAdapter.Read` no toma bloqueo ni transacción; `AssertNoOpenTransaction(document, "before")` y `"after"` alrededor de `CaptureDrawing` | **Sí**, si la lectura de MUTATE pasa por `Read`: dentro de MUTATE hay una transacción abierta y el `Debug.Assert` de `TopTransaction == null` saltaría | Una entrada aparte del adaptador (o un archivo nuevo de `Plugin/Views`), con su propia guarda |
| `TW/WorkspaceEditIntentGuardTests.cs:289-337` | El adaptador no escribe (`AdapterWrites`); **un solo** `CaptureDrawing` en el archivo y en `Read`; un `RackBlockData.Read`; cero `GetObject` | **Sí**, si la variante entra en el adaptador | 1 → 2 declarado, o archivo aparte |
| `TW/WorkspaceEditIntentGuardTests.cs:405-434` | Los siete pasos del lector anidado en orden; **una** `RackPilotIntentAdapter.Read`, un `LoadCatalog` y un `_targets.Revalidate` en todo `WorkspaceNavigationHost.cs` | Solo si T4B lee por el host de navegación (la vía O-04 en contexto de comando es de T4C, R-T4-06) | Declarado en quien la añada |
| `TW/WorkspaceEditIntentGuardTests.cs:436-457` | Exactamente **dos** clases en `WorkspaceNavigationHost.cs`; un `new PilotIntentReader(` | **Sí**, si T4B añade una clase anidada ahí | Declarado, o lector en otro archivo |
| `TW/WorkspaceEditIntentGuardTests.cs:459-472` | El host de navegación no nombra `PushBackPilot` ni `RackCad.Application.Systems`; `read.Pilot` una vez | Debe mantenerse | — |
| `TW/WorkspaceEditIntentGuardTests.cs:985-997` | `src/RackCad.Plugin/Workspace/` tiene **9** archivos, sin subcarpetas; un solo `RackPilotIntentAdapter.cs` en `Plugin/Views` | **Sí**, si T4B añade un archivo en `Plugin/Workspace` | Declarado; `Plugin/Views` no tiene censo |
| `TW/WorkspacePilotReconciliationGuardTests.cs:527-612` (regla 5) | `PassGate`: una `Authorize(` exacta. `PilotIntentReader`: toda `_authorize(` con la forma exacta, sin unicidad (§2.6) | No, si respeta la forma | — |

### C. «Sin escrituras authored» y dónde puede vivir la escritura

| Archivo:línea | Qué fija | ¿T4B la rompe? | Cómo declararla |
|---|---|---|---|
| `TW/WorkspacePilotEditGuardTests.cs:72-74`, `:693-739` | `WriteApis` (`ForWrite`, `UpgradeOpen`, `AppendEntity`, `.Erase(`, `.Commit(`, `RackBlockData.Write`, `StartTransaction`, `LockDocument`, `SendStringToExecute`, `Save`…) prohibidas **por cuerpo** en las 15 operaciones del puerto, la compuerta, los tres cuerpos de la reconciliación (oráculo de cero escrituras de PREPARE), tres auxiliares y el archivo de soporte | No, si la escritura va en una operación **nueva**: `:353-362` y `:780-793` prevén «an Update operation that writes» | — |
| `TW/WorkspacePilotEditGuardTests.cs:812-849` (trampas) | En el puerto, el soporte y **todo** el núcleo: ni `Transaction`, `Database`, `LockDocument`, `Commit(` ni `RackLog`. En `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, `WorkspaceHostCore.cs` y `WorkspaceEditIntent.cs`, tampoco en comentarios (`:837-844`). Nombres de tipo sin `Draft`, `Editor`, `Subscription` ni `Tab` (`:803-810`); el puerto no nombra `Draft*` ni `PushBack*` (`:78`) | **Sí**, si la «Update que escribe» se escribe en el núcleo con esas API. Contradice la previsión de `:353-362`, que solo mira `WriteViolations` | La escritura va detrás de un puerto (como `IWorkspaceEditIntentReader`), implementado en el Plugin |
| `TW/WorkspaceLifecycleGuardTests.cs:667-684` | `WriteApis` prohibidas en **archivos enteros**: `IWorkspaceLifecycleAdvisor.cs`, `EventBridgeCore.cs`, `WorkspaceHostCore.cs`, `WorkspaceEditIntent.cs`, `DraftRegistry.cs`, `AutoCadEventBridge.cs` y `WorkspaceController.cs`; ningún campo `Transaction`/`Database`/`Document`/`DBObject`/`ObjectId` | **Sí**, si el controlador o el núcleo escriben | La escritura en otro archivo del Plugin |
| `TW/WorkspaceHostSourceGuardTests.cs:373-378` | En las dos carpetas Workspace: ni `Guid.NewGuid`, ni `new RackEmbedDocument`, ni `RackEmbedComposer`, ni `RackBlockData.Write` | **Sí**, si T4B escribe sobres desde `Plugin/Workspace` o `Application/Workspace` | Escribir desde `Plugin/Views` o `Systems/Shared` |
| `TW/WorkspaceHostSourceGuardTests.cs:382-396` | En las dos carpetas Workspace: ni `Regen`, `Zoom*`, `SetSystemVariable`, `UpdateScreen`, `SendStringToExecute`… (excepción por archivo exacto solo para los efectos de navegación) | **Sí**, si el `Regen` de POST queda en `Plugin/Workspace` | El `Regen` en la costura (`SiblingRedrawTransaction.Post`) o en `Plugin/Views` |
| `TW/WorkspaceHostSourceGuardTests.cs:457-464` | Ni `Xrecord`, `XData`, `DBDictionary`, NOD, `AtomicFile`, `File.` en las dos carpetas | Debe mantenerse | — |
| `TW/WorkspaceHostSourceGuardTests.cs:466-480`; `TW/WorkspaceBridgeSourceGuardTests.cs:203-208` | Toda transacción y todo bloqueo, dentro de `using`; ningún campo de tipo del host | Debe mantenerse (INV-14) | — |
| `TW/WorkspaceBridgeSourceGuardTests.cs:294-307` | En `Plugin/Workspace`: ni `.Commit(`, `AppendEntity`, `UpgradeOpen`, `.Erase(`, `Xrecord`, `SaveAs`… | **Sí**, si la escritura cae en `Plugin/Workspace` | Fuera de esa carpeta |
| `TW/WorkspaceHostSourceGuardTests.cs:65-69` | Ningún `[CommandMethod` en `Plugin/Workspace` salvo el adaptador del comando | Solo con la A-n de Undo (T4C) | — |
| `TW/WorkspacePilotPrepareGuardTests.cs:27-79` | `PushBackPilotUpdatePlan.cs` es puro: sin transacción, bloqueo, `Commit`, escritura, `Autodesk`, Plugin, UI, host, `async`/`Task`, archivos, reflexión ni mutación de un borrador (`TryBeginApply`, `CompleteApply`, `EnterError`…) | **Sí**, si MUTATE o el orquestador se escriben en el archivo del plan | MUTATE y la orquestación, en otro archivo |
| `TW/WorkspacePilotPrepareGuardTests.cs:100-152` | Una sola `Compose` en el plan, en `ComposeMemberEnvelope(RackEmbedDocument source, …)`; en `Systems/PushBack/Pilot/` solo compone el plan; `Application/Workspace` no nombra `RackEmbedComposer` | Debe mantenerse: T4B escribe los textos del plan, no compone | — |
| `TW/WorkspacePilotPrepareGuardTests.cs:154-190` | `ComposeForWrite` tiene **un solo** llamador fuera del materializador (el planificador) y solo después de `.CompareWith(` | **Sí**, si T4B llama a `ComposeForWrite` fuera del plan | Que T4B use solo el plan. La guarda de M-1 se suma a esta |
| `TW/WorkspacePilotPrepareGuardTests.cs:192-251` | El orden de `Prepare` (NotApplicable, PendingText, NothingToApply, `CompareWith`, Conflict, admisión, NotAdmitted, `Build`); `catch` → Invalid; `Build` nunca borra | M-6 cabe sin romperla | — |
| `TW/WorkspacePilotPrepareGuardTests.cs:253-384` (R-3 y M-4) | `ErrorReason` solo junto a `_explicitStatus = DraftStatus.Error` o a null, en todas sus formas | Debe mantenerse si T4B toca `DraftSession.cs` (arrastre (4), M-10) | — |
| `TW/WorkspacePilotPrepareGuardTests.cs:407-430` | Sin `ExecuteInCommandContextAsync`, `async`, `await`, `.Result`, `Task`, `Dispatcher` ni `Timer` en el plan, `DraftSession.cs`, el puerto, el soporte, `WorkspaceEditIntent.cs` y el materializador; el núcleo y el controlador, igual salvo `.Result` | Debe mantenerse | — |
| `TW/WorkspaceDraftPilotIntentGuardTests.cs:30-46` | Los archivos de la intención del piloto: sin `Transaction`, `LockDocument`, `async`, `await` | Debe mantenerse | — |
| `TW/WorkspaceDraftBoundaryTests.cs:258-259`, `:526-538` | `Drafts/` sin `async`, `Task`, `event`, `Regen`…; toda operación pública del núcleo devuelve un valor | Debe mantenerse si T4B cambia `CompleteApply` | — |
| `tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs:70-75`, `:194-206` | Censo exacto de I-54: 11 llamadas a `Compose`, la primera la del plan | Debe mantenerse (T4B no compone) | Una `Compose` nueva sería un cambio declarado del censo |
| `TW/WorkspaceHostHardeningGuardTests.cs:416-525` | La tabla de **30** literales del núcleo y el soporte, con sus cuentas | **Sí**, si T4B añade textos del host (Actualizar, POST, avisos) | 30 → n, con las cuentas antes/después declaradas |
| `TW/WorkspaceLifecycleGuardTests.cs:438-511` | La marca «conocido limpio»: toda operación del puerto que cambie un borrador (`ChangesADraft`, `:476`, que no incluye `TryBeginApply` ni `CompleteApply`) termina la marca | **Sí**, si T4B añade una operación del host que aplica | Extender `ChangesADraft` y `PortMutatingOperations` (`:460-474`) |

### D. Asincronía y contexto de comando (sobre todo T4C)

| Archivo:línea | Qué fija | ¿T4B la rompe? | Cómo declararla |
|---|---|---|---|
| `TW/WorkspaceNavigationHostTests.cs:427-437` | Ningún archivo de las dos carpetas Workspace usa `async` ni `await`; **ningún** archivo de `src` contiene `ExecuteInCommandContextAsync` (`:436`) | No, salvo que T4B adelante el contexto de comando (es R-T4-05, de C) | Excepción por archivo exacto en T4C |
| `TW/WorkspaceHostSourceGuardTests.cs:423-455`; `TW/WorkspaceSourceGuardExceptionTests.cs:262-271` | Sin trabajo en segundo plano, `.Result`, `GetAwaiter` ni `ExecuteInCommandContextAsync` en las carpetas Workspace y en el host de navegación | Debe mantenerse | — |
| `TW/WorkspacePilotCloseSyncGuardTests.cs:851`; `TW/WorkspacePilotSurfaceGuardTests.cs:280-283` | La UI del piloto no alcanza el dibujo (`Transaction`, `Database`, `.Commit(`, `ExecuteInCommandContextAsync`, `Autodesk`) ni programa trabajo | Debe mantenerse | — |

### E. Fachadas caller-owned y la costura de I-55

| Archivo:línea | Qué fija | ¿T4B la rompe? | Cómo declararla |
|---|---|---|---|
| `tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:49-56` | Las fachadas autorizadas son **cuatro** (`ViewBlockDraw`, las dos de Selectivo y `LateralHeaderDrawService`), **ninguna de Push Back** | No la rompe, pero R-T4-02 («guardas de fachadas», `T4P:213`) e INV-39 piden **extenderla** | Añadir las tres de Push Back y la variante de lectura caller-owned |
| `tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:96-249` | La primitiva `RedrawInTransaction` no confirma, no abre transacción, no bloquea, no regenera, no importa ni baja por un wrapper self-owned; la preparación importa y no abre la transacción semántica; los wrappers históricos se conservan | Debe mantenerse; se extiende a Push Back | — |
| `tests/RackCad.Tests/CallerOwnedTransactionGuardTests.cs:86-250` | Para `SystemBlockWriter` y `LateralHeaderDrawService`: MUTATE recibe la transacción del llamador; PREPARE antes de abrirla; la purga después del commit; un solo `Regen` después de la purga | Debe mantenerse | — |
| `tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs:10-22` | `SiblingRedrawTransaction.cs`: **un** `StartTransaction(`, **un** `Commit()`, sin `LockDocument`, con `using (var transaction` | **Posible**: si INV-30 se resuelve dentro de la costura (un `try` propio para `Commit()`), las cuentas pueden mantenerse; un segundo `Commit()` la rompe | INV-30 es aditiva: preferir una frontera sin tocar las cuentas |
| `tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs:24-35` | Un solo `Regen()` en el archivo, en `Post`, ninguno en `Mutate` | Debe mantenerse | — |
| `tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs:40-54` | La inyección Debug existente (`SiblingRedrawDebugFaultInjection.cs`) está bajo `#if DEBUG`, sin `#else` | La inyección de INV-30 en `Commit()` debe seguir el mismo patrón | Guarda nueva equivalente |
| `tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs:61-68` | **Ningún** `*Commands*.cs` del Plugin nombra `SiblingRedrawTransaction` | **Sí**, si el adaptador de Actualizar vive en un archivo `*Commands*` | Adaptador en `Plugin/Views` (como dice R-T4-05) |
| `tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs:71-80` | Unidades y transacción sin política, planificación ni identidad | Debe mantenerse | — |
| `tests/RackCad.Tests/PushBackPluginSourceGuardTests.cs:118-163` | Cada fachada de Push Back llama a `DrawAndPlace` y `RedrawInPlace` y consume solo su builder | Debe mantenerse (G5 de la clausura de T1B) | — |
| `tests/RackCad.Tests/RackSiblingRedrawTests.cs` | Las pruebas Core de la costura de I-55 | Deben seguir en verde (INV-30) | — |

---

## 4. El estado de F4-T1B y lo que deja fijado para T4B

**Fuente:** el borrador de la sesión `hs65/t1b/I-64-f4-t1b-gate-contract-package.md` (v3, no custodiado ni emitido), en la carpeta temporal de la sesión.

- **Estado:** v3, sobre `c3ae1c97`. «No declara GATE PASS y no se emite»; la emisión requiere una orden del Owner, que además decide las interpretaciones de su §9 (`:3-6`). Recomienda una comprobación focalizada de la v3 antes de emitir (`:6`). Si T4A2 cambiara producto antes de emitir T1B, los hechos y la clausura se vuelven a medir (`:10`).
- **Orden:** «antes de F4-T4B» (F4-E51; `:16`).
- **La clausura (§3, `:206-226`):** G1 `src/RackCad.UI/Systems/PushBack/**` (10); G2 `Controls`, `Shell`, `Themes`, `Editor`, `Preview` (43); G3 diez archivos de subdiálogos; G4 `src/RackCad.Application/Systems/PushBack/**` **sin `Pilot/`** (52); G5 las tres fachadas `src/RackCad.Plugin/Systems/PushBack/*.cs`, `RackPushBackCommands.cs` y `KindHandlers/PushBackKindHandler.cs`; G6 diecisiete dependencias medidas. Fuera: `Pilot/`, `Domain/` y tres fragmentos compartidos (`RackMenuCommands.cs`, `Views/RackViewBatchProducts.cs`, `Views/RackViewPlacement.cs`). «T1B no modifica la clausura en neto» y «el alcance de T4B lo decide el paquete de T4B» (`:226`).
- **H-22 (`:184-187`):** las tres fachadas tienen `DrawAndPlace` y `RedrawInPlace` públicos y `PrepareRedraw` y `RedrawInTransaction` internos, **sin ningún llamador en producción**. «T4B añadirá el primero: T1B no lo congela» (CT-27).
- **CT-27 (`:391`):** fija (1) las firmas públicas exactas de las fachadas y la existencia de las caller-owned, y (2) que los **caminos clásicos** no llaman a las caller-owned: `RackPushBackCommands.cs`, `KindHandlers/PushBackKindHandler.cs`, `RackMenuCommands.cs` y `Views/RackViewBatchProducts.cs`. **Consecuencia para T4B:** su llamador de las caller-owned no puede estar en esos cuatro archivos, y no puede cambiar las firmas públicas de las fachadas.
- **CT-24 a CT-26 (`:388-390`):** RACKPUSHBACK, RACKEDITAR (con el orden de H-21: `PrepareAll` antes del primer `RedrawInPlace`, `SyncName` < `EraseViewBlocks` < un `Regen`) y el menú, por guarda de fuente. Coherente con D-11 («no se toca» RACKEDITAR, `T4P:146`).
- **§8 (`:590-602`):** si T4B toca un archivo de la clausura (p. ej. las fachadas, de las que es dueña), las pruebas de T1B siguen en verde (INV-19); si una tuviera que cambiar, es un cambio funcional y se declara al Owner (D13); CT-27 ya no impide el primer llamador.
- **Pruebas nuevas de T1B que T4B debe dejar en verde:** `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs` (CT-24 a CT-27) y las clases de UI `PushBackClassicCharacterization*` (contrato propuesto, `:458-500`). El `ForbiddenWriteScope` de T1B incluye `CallerOwnedFacadeGuardTests.cs` y `PushBackPluginSourceGuardTests.cs` (`:480`): T1B no los toca, así que su extensión es de T4B.

---

## 5. Implicaciones para el contrato de T4B

1. **Precondiciones de emisión, que hoy no se cumplen:**
   - T4A2 cerrada (hoy STOP en `4b2a`, `DISP:1932-1940`);
   - T1B emitida, ejecutada y cerrada (R-T4-12, `T4P:581`);
   - una orden del Owner que pase de «Solo paquetes» a la emisión.

   El paquete de T4B se fija sobre el código real de T4A2 cerrada (`T4P:177`). Si T4A2 cambia producto (intento 3), hay que medir de nuevo.
2. **Alcance de T4B, por las reglas (B):**
   - R-T4-01: lectura caller-owned y su guarda, y la equivalencia de los tres caminos;
   - R-T4-02: MUTATE atómica con la inyección Debug de INV-30 y `TopTransaction == null`;
   - R-T4-03: importación antes de la transacción, con `LegacyDeltas` declarada;
   - R-T4-04: POST con relectura y comparación byte a byte con la expectativa del plan; `CompleteApply` robusto (F4-E25 (4)); C-F4-05 con `eraseByKind` nulo;
   - R-T4-11 y R-T4-12, y la parte de R-T4-13 que toque.

   Los de (C) quedan fuera, salvo como oráculo de R-T4-06.
3. **Cambios declarados ya previsibles:**
   - **M-1:** guarda de fuente de `ComposeForWrite` con autoprueba `Copy(current)`, más el mapa de evidencia;
   - **M-5:** texto propio para un descriptor inválido, más `WorkspacePilotPrepareTests.cs:1772`;
   - **M-6:** `catalog == null` en `Build`;
   - **M-10:** XML de la sobrecarga;
   - **D-1**, si añade una lectura del host;
   - **el diagnóstico de `TryCheckNow`:** `WorkspaceEditIntentGuardTests.cs:683`, `:698-699` y `:864-867`, y quizá `:823-824` y `WorkspaceLifecycleGuardTests.cs:456-458`, más `EditCheckOutcome` y el VM del campo;
   - **la extensión de `CallerOwnedFacadeGuardTests.cs:49-56`** a Push Back y a la variante de lectura (INV-39);
   - **la tabla de literales**, si hay textos nuevos del host (`WorkspaceHostHardeningGuardTests.cs:473`, `:494-496`);
   - **la marca «conocido limpio»**, si hay una operación del host que aplica (`WorkspaceLifecycleGuardTests.cs:460-476`).
4. **Ubicación de la escritura, forzada por las guardas:**
   - Ni `Application/Workspace` ni `Plugin/Workspace` pueden escribir ni regenerar (§3 C). Las razones: las trampas del núcleo, las `WriteApis` por archivo, `RackBlockData.Write` y `Regen` prohibidos, y el censo de 9 archivos.
   - Tampoco el archivo del plan, que es puro.
   - Tampoco un `*Commands*.cs` (la costura de I-55), ni los cuatro caminos clásicos (CT-27 de T1B).
   - **Queda** `src/RackCad.Plugin/Views/` (sin censo) o la costura de `Systems/Shared`, detrás de un puerto de Application probado con dobles.
   - El orquestador de PREPARE → MUTATE → POST en Application no puede nombrar `Transaction`, ni siquiera en comentarios, si vive en `Application/Workspace`.
5. **Pruebas sin AutoCAD:**
   - el Core no carga el Plugin (ADR-0003; `CallerOwnedFacadeGuardTests.cs:25-27`);
   - todo-o-nada (INV-12), la frontera de `Commit()` (INV-30) y POST se prueban en Core con un escritor y un lector transaccionales simulados;
   - el lado del Plugin, solo con guardas de fuente;
   - las pruebas existentes de la costura (`RackSiblingRedrawTests.cs`) siguen en verde.
6. **El plan como valor:**
   - no puede vivir en un campo del núcleo (`WorkspaceEditIntentHostTests.cs:1002-1022`);
   - pasar el resultado del piloto entero a `Prepare` desde el núcleo rompe la regla M-2 (`WorkspaceEditIntentGuardTests.cs:821-855`).

   El contrato tiene que fijar dónde se llama a `Prepare` (núcleo o Plugin) y, en consecuencia, qué guarda se declara.
7. **La lectura caller-owned:**
   - si comparte el adaptador o el barrido, choca con las cuentas exactas: un `CaptureDrawing`, un `new RackSiblingDrawingScan`, la firma de `Traverse` y el `AssertNoOpenTransaction` de `Read` (§3 B);
   - lo más limpio es una entrada aparte con su propia guarda (sin `LockDocument`, `StartTransaction` ni `Commit`), y un interno de `Traverse` que reciba la transacción conservando la firma pública y las cuentas.
8. **La regla 5 no congela:** T4B puede añadir una `_authorize` en `PilotIntentReader` con la forma exacta. `PassGate` sigue con una sola.
9. **R-3 y M-4:** si T4B toca `CompleteApply` o `EnterError`, toda asignación de `ErrorReason` sigue la guarda de `WorkspacePilotPrepareGuardTests.cs:253-384`. El error diferido (`DeferredErrorReason`) ya existe y no se debe romper.

---

## 6. Riesgos y ambigüedades

1. **«Única escritora» frente a T4C:**
   - el Owner dice «T4B remains the first and only task in F4 authorized to write authored DWG state» y la rotula «MUTATE / POST / UNDO» (`COORD/owner-f4t4a2-authorization.md:397-400`);
   - el paquete reparte la escritura con T4C («Sí, la del adaptador del host», `T4P:173`) y deja Undo en T4C.

   Hay que declarar en el paquete de T4B cómo se lee la orden: T4B es la primera que escribe y la «única» se entiende como «T4B y su continuación T4C», o bien T4C no gana escrituras propias.
2. **`CompleteApply` en `finally`:**
   - F4-E07 y F4-E16 lo asignan a «T4B» (`DISP:61`, `:231`), pero con el sentido de la descomposición original (`F4P:47-48`, T4B = ACTUALIZAR-HOST);
   - en la vigente es R-T4-06, marcada (C) aunque «oráculo de T4B y T4C» (`T4P:552-558`).

   Lo mismo pasa con «los avisos de POST visibles aunque falle su relectura» (F4-E16), que en T4P son N-5 (R-T4-04, B) y la UI de avisos (R-T4-09, C). Hay que fijar quién implementa la ranura y el `finally` y quién solo lo prueba.
3. **§3.2 sin repartir:** «A T4B o T4C van INV-30, la parte de escritura de INV-10, INV-39, C-F4-05, las guardas de fachadas y la inyección Debug» (`T4P:213`). R-T4-01, 02 y 04 los ponen en B. Conviene que el paquete lo diga explícitamente.
4. **«Aplicado con avisos»:**
   - D-13 tiene la fila «`Commit()` retornó y POST falló o hubo faltantes → Aplicado con avisos» (`FRZ:528`) y D-T4A2-06 dice que «Solo T4B produce esos estados» (`A2P:359`);
   - R-T4-04 solo enumera `Applied` y `AppliedPostRereadFailed` (`T4P:540-545`).

   El paquete tiene que fijar cuándo T4B produce `AppliedWithWarnings` (purga o `SyncName` fallidos, piezas faltantes de la importación).
5. **Smoke-2:**
   - el Owner dice «not before T4B closes» (`:405`) y la evidencia de T4A2 dice «después de cerrar F4-T4B»;
   - el paquete dice «el procedimiento se fija tras T4C» (`T4P:587`) y Undo y la UI de Actualizar son de T4C.

   Es una precondición, no una fecha: Smoke-2 real necesita T4C.
6. **Lo que T4B no puede cumplir sin AutoCAD:**
   - el «STOP si la vía de escritura no conserva los bytes» (R-T4-04): con dobles solo se mide el texto del doble;
   - `TopTransaction == null` al salir y la atomicidad física;
   - la equivalencia de los tres caminos de lectura en el dibujo real (R-T4-01): en Core solo se ve sobre la misma `BuildStaleBase`;
   - que la caller-owned de Push Back funcione dentro de una transacción ajena (R-09, `FRZ:540-541`);
   - la agrupación de Undo de la importación (`T4P:211`).

   Todo eso es de Smoke-2. Además, sin el `using LockDocument` y el contexto de comando (T4C), el código del Plugin de T4B no tiene llamador de producto hasta T4C.
7. **INV-30 contra la guarda de la costura:**
   - la clasificación de una excepción en `Commit()` como Desconocido, «sin cambiar el contrato observable para Insertar», tiene que convivir con `SiblingRedrawSeamGuardTests.cs:10-22` (un solo `Commit()`, `:13-14` según H-14);
   - la costura convierte hoy esa excepción en `Discarded` (`F4P:21`).

   El paquete tiene que decir si la frontera va dentro de la costura (aditiva) o en una vía propia de Push Back.
8. **El diagnóstico de `TryCheckNow` toca la UI:**
   - llevarlo a la vía `TryCheckNow` (asignado a T4B) exige cambiar `EditCheckOutcome`, el núcleo **y** el VM del campo (`WorkspacePilotFieldViewModel.cs:712-740`);
   - la fila de T4B en §3 no menciona la UI.

   Hay que declarar el archivo de UI en el alcance de T4B, o pasar ese punto a T4C, que es la de la UI de Actualizar.
9. **El conflicto entre M-3 y M-2:**
   - una segunda aparición del diagnóstico rompe M-3 (1 → 2);
   - factorizarla en un auxiliar que recibe `pilot` rompe la regla de M-2 (`Use(pilot)`).

   Cualquier opción exige un cambio declarado de una de las dos guardas de T4A2, que aún no está cerrada.
10. **La previsión de `WorkspacePilotEditGuardTests.cs:353-362` y `:780-793`** (una «Update que escribe» se admite) es engañosa: la misma clase prohíbe `Transaction`, `LockDocument` y `Commit(` en todo el núcleo y el soporte (`:812-849`), y `WorkspaceLifecycleGuardTests.cs:672-684` lo prohíbe en `WorkspaceHostCore.cs` y `WorkspaceController.cs`. Una «Update» del puerto solo puede delegar en un puerto de escritura.
11. **Las líneas citadas por paquetes anteriores se han desplazado:**
   - `DraftSession.cs:474`: `AnotherUpdateInProgress` está hoy en `:515`;
   - `RackSiblingScan.cs:103`: la transacción está hoy en `:112-113`;
   - map-2 de T4A1: las guardas de C2-M-3 están hoy en `:306` … `:587`.

   El paquete de T4B debe citar las líneas de su base.
12. **F4-T4A2 no está cerrada y sus menores del intento 2 no tienen disposición:** m-1, D2-1, D2-2 y la parada `4b2a`. Si el Owner elige el intento 3, el VM cambia y la base de T4B (y la de T1B) se mueve.
13. **Dos nombres para T4B:** el paquete de F4 (`F4P:47-48`) llama F4-T4A a la costura de mutación y F4-T4B al host de Actualizar; la descomposición vigente es otra. Cualquier cita de `F4P` o de disposiciones anteriores a F4-E51 que diga «T4A» o «T4B» hay que traducirla.
