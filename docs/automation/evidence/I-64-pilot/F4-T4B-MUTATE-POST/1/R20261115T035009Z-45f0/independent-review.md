# Revisión adversarial independiente del código: I-64 F4-T4B-MUTATE-POST, intento 1

**Commits:** base `3633f08d` (producto de `c3ae1c97`), RED `f4476bc4` (árbol `7eca6b9f`) y GREEN `4314c180` (árbol `0cb46ccc`), en la rama `architecture/workspace-persistente-rackcad`.

**Veredicto:** **ACEPTABLE CON MENORES**: 0 bloqueantes, 1 importante y 10 menores. El importante (I-01) es de letra y de declaración, no de comportamiento. Antes del cierre necesita una disposición: ratificarlo como desviación declarada o hacer una corrección acotada (§10).

---

## 1. Objeto y método

**Lo que leí:**
- **El contrato:** `docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/coordinator/gate-contract.json` (18 invariantes, 26 paradas, 24 `RequiredTests`, alcances).
- **El paquete v7:** `docs/initiatives/I-64-f4-t4b-gate-contract-package.md`, completo.
- **Las disposiciones:** la autorización de emisión del Owner y F4-E59/F4-E60 de `F4-DRAFTS/f4-dispositions.md`.
- **La entrega:** `worker-handoff.json`, en todos sus campos.
- **Los diffs:** BASE..RED, RED..GREEN y BASE..GREEN de los 21 archivos.
- **Las fuentes completas del GREEN:**
  - Application: la corrida, el puerto, la ayuda, `DraftApplyOutcome`, `DraftSession` (de `TryBeginApply` a `CheckNow`), `EditUpdateStatus`/`EditUpdateOutcome` y `TryUpdatePilot`/`UpdateGate`/`PassGate`.
  - Plugin: el escritor y `RackSiblingScan.cs`.
- **Las pruebas:** las cuatro nuevas, completas, y los hunks de las siete cambiadas.
- **Para comparar:**
  - `c3ae1c97:src/RackCad.Plugin/Views/RackSiblingScan.cs`;
  - el camino clásico (`src/RackCad.Plugin/RackPushBackCommands.cs:170-360`);
  - las firmas de las tres fachadas de Push Back;
  - `ViewBlockDraw`, `SystemBlockWriter`, `RackBlockRenamer.SyncName`, `PushBackPilotIntentReader` y `PushBackPilotUpdatePlan`;
  - `WorkspaceHostHardeningGuardTests` (la tabla de 30 literales), `PassGateViolations` y el soporte de pruebas `WorkspaceHostSource`.

**Los scripts:** todos son de solo lectura y están en la carpeta de este informe.
- `ir_code_traverse.py` reconstruye el literal de `c3ae1c97` y la normalización declarada, y los compara con el literal embebido y con el GREEN.
- `ir_code_trx.py`, `ir_code_trx_msgs.py`, `ir_code_trx_redpass.py`, `ir_code_trx_local.py` y `ir_code_trx_cmp.py` analizan los TRX de la CI (RED y GREEN, Core y UI) y los locales. La salida principal está en `ir_code_trx_out.txt`.

Solo usé `git show`, `git diff`, `git log`, `git rev-parse`, `git grep`, `git ls-files` y `git check-ignore`. No compilé ni ejecuté pruebas, y no escribí nada en el worktree.

**Lo que no pude verificar:**
- el estado de los jobs de la CI en GitHub: solo tengo las copias de sus TRX;
- el `TreeSha` de cada corrida local, porque el TRX no lo lleva;
- la medida en Debug de la base, sin registro conservado (M-08);
- el comportamiento del escritor en AutoCAD (Smoke-2);
- el contenido de `docs/` que el paquete cita como mapas.

No leí la revisión directa, para no anclarme en ella.

---

## 2. Punto 1: la corrida y la tabla de frontera (INV-01 a 06, D-T4B-02, D-T4B-05)

Archivo: `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs` en `4314c180`.

**Fila por fila de la tabla del punto 4 de D-T4B-02:**

| Fila del paquete | Código | ¿Conforme? |
|---|---|---|
| Excepción antes de `Seal` (`Begin`, `ReadForPrepare`, `PrepareUnits`, `BeginMutation`, `Reread`, `WriteUnit`) con un aborto que no lanza → NotAppliedFailure con causa | `Begin`/`ReadForPrepare`/`PrepareUnits`/`BeginMutation`: el `catch` de `Run` (`:258-263`, con `_sealReturned` falso). `Reread`/`WriteUnit`: el `catch` de `Mutate` (`:344-349`, con `sealing` falso). `WithAbortFailure` sin fallo devuelve lo decidido (`:419-420`) | Sí |
| El aborto falla con algún `WriteUnit` empezado → CommitThrew(`ABORT_FAILED: …`) | `_writeStarted = true` antes de cada llamada (`:404`). En `:425-428`, CommitThrew con `abort + "; " + causa`. No se afirma ningún rollback | Sí |
| El `Dispose` de `m` lanza sin `Seal` y sin ningún `WriteUnit` → NotAppliedFailure | `:429` | Sí |
| Una excepción **en** `Seal` → CommitThrew | `markSealing()` antes de `Seal()` (`:410-411`) y `SealFailedPrefix` (`:346-347`) | Sí |
| `Seal` vuelve y el `Dispose` de `m` lanza → POST, con aviso | `:362-366` | Sí |
| `Post()` lanza, o falta la relectura → AppliedPostRereadFailed(causa, avisos) | `:449`, `:460` y el `catch` (`:471-474`). `PostFailed` lleva `_warnings` (`:561-562`) | Sí |
| El comparador o un constructor lanzan después de `Seal` → AppliedPostRereadFailed | `ComparePost`, `Read` y las fábricas van dentro del `try` de `Post` (`:444-474`) | Sí (ver M-01) |
| El `Dispose` exterior lanza → la clasificación no cambia, con las dos salvedades | `WithScopeDisposeFailure` (`:532-559`): Admission, Preflight, Failure y CommitThrew añaden la causa; Applied pasa a AppliedWithWarnings; AppliedPostRereadFailed añade un aviso; el Conflicto la descarta | Sí |

**La precedencia (6):** se cumple.
- Un Conflict o un Preflight de MUTATE se conservan aunque el `Dispose` de `m` lance (`:432-436`).
- Si `Seal` lanzó y el aborto también, sigue siendo CommitThrew y se añade la causa del aborto (`:430-431`).
- `ABORT_FAILED` solo nace de un NotAppliedFailure con `_writeStarted` (`:425-428`).
- Los dos ámbitos se liberan en `finally` propios, el de `m` dentro del del ámbito de la corrida (`:350-360` y `:264-277`), y sin `using`. Lo fija `TheRunReleasesBothScopesInFinallyBlocksOfItsOwnAndNeverWithAUsing`.

**Excepciones, nulos y `Dispose`:**
- Los nulos del puerto:
  - un ámbito nulo, una lectura nula, un `BeginMutation` nulo o un `Scope` nulo dan NotAppliedFailure (`:249-251`, `:289`, `:332`, `:336`, `:377`);
  - un `Post` nulo o un `Scan` nulo dan AppliedPostRereadFailed (`:449`, `:460`);
  - **una importación nula da NotAppliedPreflight** (`:322`), que no casa con los otros nulos (M-04).
- Una causa vacía de un valor del puerto (un `RejectionCode` o un `Cause` vacíos) hace lanzar a `WithCause` (`DraftApplyOutcome.cs:109-113`). Se captura en `Run` y da NotAppliedFailure, sin escritura: es correcto.
- **¿La corrida puede lanzar?** `Execute` captura todo (`:137-147`) y `Run` es total. No encontré ningún camino posterior a `Seal` que salga de `Run`: `WithScopeDisposeFailure` solo construye con un `PostRead` no nulo y una causa no vacía.
- Pero el `catch` exterior de `Execute` devuelve **siempre** NotAppliedFailure, sin mirar si `Seal` ya volvió. Hoy no se alcanza; es frágil ante un cambio (M-01).

**¿Hay un «no aplicado» con escritura posible?**
- Solo en la fila 1 del paquete: una unidad que lanza con un aborto que no lanza da NotAppliedFailure. Es la letra del paquete, que confía en el aborto (la B del Owner).
- También en los dos caminos estructurales de M-01, inalcanzables hoy.

**¿Hay un «aplicado» sin relectura verificada?** No. `Applied` y `AppliedWithWarnings` solo nacen en `:467-469`, después de `ComparePost(plan, read) == null`, con `read` construido desde `values.Scan` y `plan.OriginKey`.

**La relectura de MUTATE es la autoridad (INV-01 e INV-02).** Va en este orden (`:373-414`):
1. `DocumentMatches`;
2. el origen, elegido otra vez en puro: `ChooseOrigin` con la misma regla de texto exacto entre los `Mutable` que Build (b) (`PushBackPilotUpdatePlan.cs:357-361`);
3. `BuildStaleBase(ScanOf(values, plan.OriginKey))`;
4. `DraftStaleBase.Create(plan.ExpectedBase, plan.ExpectedVariableRegistryText).CompareWith(reread)`;
5. con una diferencia o con otro origen, NotAppliedConflict y cero `WriteUnit`;
6. el destino: las mismas claves `Mutable` y todos los `PreparedBlockMatches`;
7. los supervivientes y las capas, otra vez;
8. un `WriteUnit(key, plan.Envelope(key))` por miembro.

`BuildStaleBase` no depende de `OriginKey`, porque `Analyze` clasifica con `null` (`PushBackPilotIntentReader.cs:119`). Por eso la comprobación `sameOrigin` es redundante con la de la StaleBase y no se puede probar sola: no es un defecto.

**POST con `plan.OriginKey` y sin normalizar.**
- La lectura es `Read(ScanOf(values.Scan, plan.OriginKey), _catalog)` (`:463`).
- `ComparePost` (`:154-174`) compara:
  - el mismo conjunto de claves `Mutable`;
  - los textos con `StringComparison.Ordinal`, sobre el texto crudo que guarda la StaleBase (`PushBackPilotIntentReader.cs:152`);
  - la escala con `double.Equals`.
- No normaliza nada.

**Las decisiones que declara WorkCompleted:**
- **Una importación con `Ok = false` da NotAppliedPreflight** (`:322`).
  - El escritor solo devuelve `Failed` ante precondiciones (`RackPilotUpdateWriter.cs:93` y `:101`). Los fallos reales de importación salen de `PrepareRedraw` lanzando (`ViewBlockDraw.cs:74-86`) y por tanto dan NotAppliedFailure.
  - Un `Failed` puede llegar después de importar los miembros anteriores (OM-5, declarado).
  - Es admisible, aunque la etiqueta «Preflight» sugiere que no se hizo nada. Para ratificar (M-04).
- **Un fallo de los nombres deseados es un aviso y POST sigue** (`:478-489`).
  - El paquete no lo fija.
  - En la práctica no se alcanza: `DesiredNamesOf` usa la misma `PushBackPilotSystemRebuild.Build(member.DesignText, catalog)` que ya pasó en `PrepareUnits`, con el mismo catálogo.
  - La comparación byte a byte sigue verificando lo authored, y los nombres son cosméticos, como en `SyncName`.
  - Admisible. Para ratificar (M-04).

**D-T4B-05:** la tabla del comparador se cumple (`:464-469`): con coincidencia, `Applied` o `AppliedWithWarnings`; si no, `PostFailed`.

---

## 3. Punto 2: el escritor (D-T4B-03, INV-04, 07, 12, 14 y 16)

Archivo: `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs` en `4314c180`.

**Las tres clases:**
- El puerto (`:32-52`) no tiene campos. `Begin` devuelve `new RackPilotUpdateScope(...)` cada vez (`:37`).
- Los objetos de AutoCAD (`Document`, `ObjectId`, `PreparedViewRedraw`, `Transaction`, `Database` y las `staleDefinitions`) solo son campos de las dos clases de ámbito (`:60-66` y `:220-225`).
- No hay `LockDocument`, `Erase(`, `EraseViewBlocks`, `RackUnitsGuard`, `async`/`await`, `NavigationTargets`, `BlockNaming`, `RackViewBaseName`, `ForWrite` ni `UpgradeOpen`.

**Solo operaciones primitivas y hechos:**
- **El documento** se comprueba una vez, en el constructor del ámbito (`:71`).
  - Observación: `Reread` y `Post` construyen sus valores con `DocumentMatches = true` fijo (`:50`). Para el escritor real, la comprobación de documento de la relectura en la corrida (`PushBackPilotUpdateRun.cs:378`) es vacía. La prueba `ADocumentThatChangedBetweenPrepareAndMutateIsAPreflightWithoutWrites` cubre un camino que solo el doble produce. No es un defecto: el ámbito siempre opera sobre `_document`, el autorizado.
- **`ReadForPrepare`** (`:74-86`) usa `CaptureDrawing` y el catálogo.
- **`PrepareUnits`** (`:89-131`) usa las tres `PrepareRedraw`. El sistema sale de la ayuda pura y el corte, de `RackViewCodec`.
- **`BeginMutation`** (`:134-145`) comprueba que `TopTransaction` es nula, o devuelve el rechazo como valor; después llama a `StartTransaction`.
- **`Reread`** (`:240-248`) llama a `TraverseCore(_transaction, _database, ObjectId.Null, null)` y compara la identidad con `UnmanagedObject` en `EnsureOwned` (`:307-313`).
- **`WriteUnit`** (`:250-287`) compara `envelopeText` con `unit.Prepared.PayloadJson` en ordinal (`:255`), que es el texto del plan para esa clave (`:111-123`). Después llama a la `RedrawInTransaction` de su fachada y recoge `staleDefinitions` y `MissingInstances`.
- **`Seal`** (`:289-295`) es el único `Commit()`.

**El mapeo de vista, extremo, lado y poste** coincide con el clásico (`RackPushBackCommands.cs:276-345`):
- frontal: `PushBackEnd == Posterior ? Posterior : EntradaSalida` y `PushBackSide == B ? B : A`;
- lateral: `postIndex = Address.Variant.Index`;
- planta, sin variante.

El reparto del caso por defecto se invierte: el clásico lo usa para Lateral y el escritor para Frontal. Es equivalente, porque `DimensionViewKind` solo tiene tres valores (`DimensionViewPolicy.cs:11-16`) y los enumerados del extremo y del lado, dos (`RackViewAddress.cs:22-32`). Las firmas de `PrepareRedraw` y `RedrawInTransaction` encajan con las fachadas congeladas (`PushBack*DrawService.cs`).

**El orden de `Post`** (`:148-195`) es el del paquete y lo fija la guarda de cuentas de una aparición:
1. `TopTransaction`; si está abierta, devuelve `(true, null, null, null)` sin hacer nada más;
2. `PurgeAfterCommit` una vez, con la **unión** (un `HashSet` compartido con el ámbito de mutación, que el `Dispose` de MUTATE no vacía);
3. `SyncName` por cada clave con nombre deseado;
4. la lectura de nombres en `StartOpenCloseTransaction` con `ForRead`;
5. `ApplyRegen`, cuyo fallo es un hecho: el tipo y el mensaje;
6. `CaptureDrawing`.

**La purga** va fuera del ámbito de mutación: la corrida libera `m` antes de llamar a `Post` (`PushBackPilotUpdateRun.cs:350-366` y `:255`). No hay purga en NotApplied ni en CommitThrew, porque `Post` solo se alcanza con `Seal` devuelto.

**La transacción poseída:**
- No queda abierta: la corrida siempre libera `m`, y `Dispose` llama a `_transaction.Dispose()` (`:297-304`).
- No se confirma dos veces ni sin `Seal`: solo hay un `Commit()`, en `Seal`, que la corrida llama una vez.
- El único resquicio es teórico: una excepción entre `StartTransaction` y la construcción del ámbito, que solo asigna campos.

**§9 I-7:** pide que «al salir, se comprueba registrando en el log». El escritor no registra nada; la comprobación de `Post` se convierte en una clasificación, AppliedPostRereadFailed(`TOP_TRANSACTION_OPEN`). Ver M-02.

---

## 4. Punto 3: el movimiento de `Traverse` (INV-17, D-T4B-04)

**Comprobado con `ir_code_traverse.py`** contra `git show c3ae1c97:src/RackCad.Plugin/Views/RackSiblingScan.cs`:
- **`TraverseBaseline.CoreLiteral`** (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs:868-930`):
  - **normalizado, es igual** a `:103-110` + `:115-161` + `:166-169` con la única sustitución `document.Database.BlockTableId` → `database.BlockTableId` en `:115`, que aparece una sola vez;
  - sin la sustitución, difiere;
  - en crudo incluye además las dos líneas en blanco de `:111` y `:162`, que el paquete asigna al envoltorio. La normalización las anula (M-09).
- **`RestLiteral`**, con `@NS@ ` → `namespace `, es en crudo, salvo CRLF, idéntico a `c3ae1c97` sin `:98-170`.
- **En el GREEN** (`src/RackCad.Plugin/Views/RackSiblingScan.cs:112-179`):
  - el cuerpo del núcleo sin llaves, normalizado, es igual al literal;
  - el resto del archivo sin los dos miembros, normalizado, es igual al resto de la base;
  - el diff por líneas, sin sangría, entre la base y el GREEN solo muestra las líneas del envoltorio y de la firma, más la sustitución.

**¿Es una refactorización sin cambio de comportamiento?** Sí.
- `Traverse` (`:98-110`) es exactamente el bloqueo, la transacción, la llamada, el `Commit` y el `return`.
- La única diferencia observable es que el resultado se construye ahora **dentro** del `using` y antes del `Commit`; antes se construía después del `Dispose`. La construcción es solo de objetos gestionados (`List`, `Dictionary` y `ReadOnlyDictionary`), así que no cambia nada.
- No hay `try`/`catch` en el archivo.
- `TraverseCore` es `internal static` y está justo después de `Traverse`.

**La normalización** es la declarada: CRLF → LF, fuera de comentarios fuera de los literales, colapso de blancos y recorte (`:385-444`). Protege además los literales de carácter, lo que es más estricto que lo declarado.

**¿Hay cambios que la guarda no vería?**
- El colapso de blancos se aplica también **dentro** de los literales de cadena, y `@$"` no se reconoce como literal literal (*verbatim*).
- Hoy no hay ningún literal de cadena ni en el núcleo ni en el resto, así que no hay exposición. Un literal nuevo, de todos modos, se vería como token.
- La normalización no reordena nada.
- La posición del par `Traverse` y núcleo dentro de la clase no se fija. Está declarado (N-6) y no tiene efecto.

**Las autopruebas negativas son reales.** Mutan el texto real:
- del núcleo: `directOnly`, `IsAnonymous`, `envelope != null &&`, `forceValidity`, `rawEnvelopes` y `ForWrite` (`:529-546`);
- del envoltorio (`:568-581`);
- del orden (`:583-592`);
- del resto (`:603-624`).

Dos debilidades (M-07):
- la fila `"public IReadOnlyList<RackSiblingScanFact>"` (`:609`) nunca casa, porque el archivo usa `internal`, y cae en silencio en el respaldo de la línea de la clase (`:613-618`);
- la comprobación «el núcleo va justo después de `Traverse`» (`:502`) lee el texto crudo, así que un comentario de documentación en `TraverseCore` la haría saltar: un falso positivo.

**El censo** (`:643-675`) cuenta las llamadas `TraverseCore\s*\(`. Una referencia como grupo de métodos en otro archivo no se contaría, frente a «ningún otro archivo de `src/` lo nombra» (M-07).

---

## 5. Punto 4: `CompleteApply` por rama y `TryUpdatePilot` (INV-09, INV-10, D-T4B-06, D-T4B-07)

**`DraftSession.cs` (`4314c180`), rama por rama:**

| Rama | Código | ¿Conforme? |
|---|---|---|
| Applied y AppliedWithWarnings | `StageApplied` copia y compara **antes** de `ReleaseApplySlot` (`:541-544`, `:588-607`). Si falla, Unknown(PostReread) con la causa y los avisos (`:683-692`) | Sí (ver M-05) |
| CommitThrew | Se conserva (`:673-677`). La rama de `Recover` (`:627-633`) es inalcanzable, porque el estado explícito ya está fijado antes de evaluar `Status` | Sí |
| NotApplied* | Si el cálculo del estado lanza (`Status` → comparador), `Recover` da `_explicitStatus = DraftStatus.Error;` seguido de `ErrorReason = cause;` (`:646-653`), nunca «aplicado» | Sí |
| Todas | La ranura se libera una vez (`:543-544`). `ClassifyContained` captura todo (`:610-620`). `ClosedDetached` no evalúa el comparador (`:541` y `:545-546`). El bloque diferido (`:555-559`) es idéntico byte a byte al de la base `3633f08d:…DraftSession.cs:549-553` | Sí |

- AppliedPostRereadFailed conserva los avisos (`:701-706`; `DraftApplyOutcome.cs:88-92`, con copia).
- **¿Un NotApplied* puede acabar en «aplicado»?** No, ni en `Classify`, ni en `Recover`, ni en el host.

**`TryUpdatePilot` (`WorkspaceHostCore.cs:805-905`):**
- **El orden 1-7 es el del paquete:**
  1. `UpdateGate`;
  2. el rack en blanco o sin borrador → `NoDraftReason`;
  3. `Precheck`;
  4. `TryBeginApply`;
  5. `Execute`;
  6. `CompleteApply(outcome)` en un único `finally` (`:863-866`), con `NotAppliedFailure` por defecto (`:852`);
  7. `NotePilotEdit` y `_knownClean.Remove(request.Session)`, literal.
- **`Failed`** solo aparece en el `catch` que envuelve los pasos 1-4 (`:844-849`).
- **La puerta** (`:908-933`) mira el documento activo, después el modal y después `Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted`, con `NotThisDocumentReason`, que lleva el mismo texto que `PassGate`. No tiene reposo ni lector.
- **El fin de la marca de limpio** va en Conflict, CommitThrew, Applied, AppliedWithWarnings, AppliedPostRereadFailed y Error, y no en Admission, Preflight ni Failure, que no cambian el estado. Es razonable.
- **Las diferencias** se leen después de `CompleteApply`, con `DescribeDifferences`.

**Observaciones:**
- Cualquier `closed.Status == Error` da `ClosedInError` (`:872-877`), también un error diferido que llega tras un Applied o un Conflict. La respuesta pierde entonces la clasificación por frontera, los avisos y las diferencias (M-03).
- `!closed.Accepted` responde «La actualización no llegó a ejecutarse», sea cual sea el resultado de la corrida (`:870`). Hoy es inalcanzable (M-01).

**Los textos del apartado «Textos» de D-T4B-07 no se cumplen (I-01):**
- `UpdateGate` usa dos literales nuevos del núcleo (`:915` y `:921`), no constantes que ya existen.
- La causa `UpdateNotRunReason` es una constante nueva del núcleo (`:48`, usada en `:852`, `:861` y `:870`), no una constante de la corrida.
- El paquete dice que «los rechazos y las causas son constantes de la corrida». Eso se podía cumplir sin tocar la tabla de 30 literales, porque `WorkspaceHostHardeningGuardTests.cs:470-487` solo cuenta los 30 literales concretos. Por tanto, la justificación de WorkCompleted («para no alterar la tabla de 30 literales») no se sostiene.
- La guarda que se escribió para esta regla (`WorkspaceEditIntentGuardTests.cs:1108-1110`) solo mira el cuerpo de `TryUpdatePilot`. El motivo llega por `out reason` desde `UpdateGate` y la esquiva, mientras su comentario afirma que se cumple.
- `Deviations` está vacío.

---

## 6. Punto 5: el RED (INV-15)

**El esqueleto es inerte.**
- **Puerto:**
  - `Succeeded` y `Failed` dan `(false, null)`;
  - `Started` y `Rejected` dan `(null, null)`;
  - los constructores de valores solo asignan.
- **`EditUpdateOutcome`:** todas las fábricas dan `default`.
- **`plan.Envelope`** da `null`.
- **`AppliedPostRereadFailed`** descarta los avisos.
- **La corrida:**
  - `Precheck` da `Go()` con `Proceeds = false`;
  - `Execute` da NotAppliedFailure(`inert`);
  - `DesiredNamesOf` da un diccionario vacío.
- **La ayuda:** `Build` da `new PushBackSystem()`.
- **El escritor:** valores sin AutoCAD (`f4476bc4:src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`).
- **`TryUpdatePilot`** da `default`.
- `RackSiblingScan.cs` y `DraftSession.cs` no cambian en el RED: lo confirma `git diff --name-only 3633f08d f4476bc4 -- src`.
- **Observación:** el `ComparePost` del RED devuelve `null`, la respuesta de coincidencia. Es permisivo, pero ninguna prueba pasa en el RED por eso.

**`red-ci-core/core.trx`:**
- 18684 resultados y 132 fallos, todos de clases `Workspace*`:
  - `WorkspacePilotUpdateRunTests` 55, `WorkspacePilotUpdateGuardTests` 34 y `WorkspacePilotUpdateHostTests` 30, que suman las 119 declaradas;
  - `WorkspaceDraftPilotOwnershipTests` 4, la `Theory` dividida;
  - `WorkspaceEditIntentGuardTests` 3, nuevas;
  - `WorkspaceLifecycleGuardTests` 3, que dependen de `PortMutatingOperations` y `ChangesADraft`, cambiadas y declaradas;
  - `WorkspacePilotEditGuardTests` 1, la fila nueva `which: 3`;
  - `WorkspacePilotPrepareTests` 2, M-5 y M-6.
- Ninguno tiene un `Message` con el prefijo de un tipo cualificado seguido de « : », y la primera línea de todos los `StackTrace` está bajo `tests/`.
- El conjunto de fallos es igual al de la corrida local `r01` y al `FailedTests` de la entrega.

**Las 21 clases de regresión pasan en el RED**, con cuentas iguales o mayores que `MinSelected` (`CallerOwnedFacadeGuardTests` 81, es decir, 48 + 33).

**`red-ci-ui/ui.trx`:** 2310 resultados, 2293 en verde y 17 `NotExecuted` previas. `RackCad.UI.Tests.Workspace` pasa 601/601 y `PushBackClassicCharacterization`, 55/55.

**Las guardas que pasan en el RED frente a D-T4B-04 (8):**
- **Fallan, como exige el paquete:**
  - (1), (2), (4) y la región de (4b);
  - las positivas de D-T4B-09: `StartTransaction`, `StartOpenCloseTransaction`, `Commit`, purga, el orden de `Post`, los `finally` y Debug.
- **Pasan:**
  - 19 métodos de la guarda: prohibiciones, (3) y (4b) fuera de la región, las negativas de INV-39 y de los censos, M-1, la forma del puerto, `#nullable`/namespace y 3 de las 5 filas de la negativa del envoltorio;
  - 5 de las de host: la forma del enum y del `struct`, y tres filas cuyo comportamiento ya cumple la base.
- **Ninguna positiva es tautológica** en el sentido de pasar en el RED por debilidad: las que pasan son de forma o de comportamiento previo.
- **Hay dos con nombres que prometen más de lo que comprueban** (M-06):
  - `TheAnswerOfAnUpdateAlwaysComesFromTheCloseAndTheOutcomeNeverNamesADraft` (`WorkspacePilotUpdateHostTests.cs:400-414`) es solo reflexión;
  - `TheFactoriesOfTheOutcomeCarryTheirStatusTheirReasonAndCopiesOfTheLists` (`:436-458`) no comprueba ninguna copia, y `Closed` no copia (`WorkspaceEditIntent.cs:477-483`).
- La lista declarada en Evidence no es exacta (M-08).

---

## 7. Punto 6: la calidad de las pruebas

**El doble** (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateScopeDouble.cs`) permite probar todas las filas:
- `ThrowAt` y `HookAt` en cada llamada, también en los dos `Dispose`;
- respuestas nulas;
- el rechazo;
- la importación fallida;
- las capas;
- `PreparedBlockMatches`;
- `ForeignAtPost`;
- `RegenFailure`;
- `PostOverride`;
- los textos después de `Seal`.

Las escrituras quedan *staged* y el aborto las descarta (`:208-234`). Hay dos límites:
- `ThrowAt["Seal"]` lanza antes de aplicar, así que no se puede representar «`Seal` lanza después del commit». La clasificación es la misma, CommitThrew, y el resto queda para Smoke-2 (S2-R-10);
- el aborto que falla también descarta lo staged. Basta, porque solo se prueba la clasificación.

**Pruebas débiles o tautológicas (M-06):**
- `TheRunKeepsNoTextOfTheDrawingBetweenTwoRuns` (`WorkspacePilotUpdateRunTests.cs:826-835`) compara dos montajes independientes, así que no puede detectar estado compartido entre corridas.
- `ThePostComparisonSeesAnotherScaleInTheStateThatTheRereadMaterializes` (`:800-810`) solo exige `NotNull`, y su comentario admite que los sobres también difieren: la rama `PostScaleDiffersReason` (`PushBackPilotUpdateRun.cs:172-173`) no se ejerce nunca.
- `ThePostComparisonDoesNotNormalizeAnything` (`:754-777`) solo exige la clase de resultado, no `POST_ENVELOPE_DIFFERS`: una relectura no admitida también la haría pasar.
- `TheDesiredNamesFollowTheRuleOfTheClassicEditorPerView` (`:691-705`) cubre una por vista, como pide el paquete, pero solo en un rack no compuesto y en entrada-salida. Ni la rama compuesta (`composite ??=`, `PushBackPilotUpdateRun.cs:205-206`) ni el extremo posterior se ejercen.

**Las guardas de fuente:**
- Las negativas mutan textos exactos, con variantes LF y CRLF. Si el texto cambia, fallan de forma visible: son frágiles, pero ruidosas, que es lo aceptable.
- Los casos silenciosos o de falso positivo están en M-07.

**Los cambios de guarda existentes:** solo los de D-T4B-07 y D-T4B-09, y en el alcance:
- `PortMutatingOperations`, con `TryUpdatePilot` al final y `Skip(5)` sobre 4 cuerpos;
- `ChangesADraft`, con `TryBeginApply` y `CompleteApply`;
- los cuerpos de escritura, con la fila 3;
- el drenaje y el Idle;
- el censo de portadores;
- `CallerOwnedFacadeGuardTests.Facades`, con las tres fachadas;
- la `Theory` dividida;
- M-5 y M-6.

Tienen autoprueba negativa, salvo el censo de portadores, que va por reflexión. La contención como `Failed` se hizo con guardas nuevas (`WorkspaceEditIntentGuardTests.cs:1048-1096`) y no cambiando `:583-596`. Ver M-10.

---

## 8. Punto 7: la contención (INV-18, la autorización del Owner y F4-E59)

- Las cuatro pruebas nuevas y los cambios trabajan en memoria: el doble, `DraftRegistry`, `WorkspaceHostRig` y los lectores falsos.
- Las guardas leen las fuentes del repositorio y no escriben. No hay `File.`, `Directory.`, `Environment.`, `Dispatcher`, ventanas ni `SetEnvironmentVariable` en ellas: lo comprobé con `git grep` sobre `4314c180`.
- Ninguna prueba ejerce con entradas válidas un camino con diálogo o con escritura real.
- **En local:**
  - de UI, solo los dos filtros (`ru01`, `ru02`, `gu01` y `gu02`);
  - las tres corridas no declaradas (`tests/g0/w.trx`, `tests/g1/full.trx` y `tests/r1/full.trx`) son de **Core**: lo comprueban los nombres y el `storage` del TRX.
- INV-18 se cumple. La omisión en la entrega es M-08.

---

## 9. Punto 8: la honestidad de la entrega

**Se sostiene:**
- 3103 seleccionadas, con 132 fallos en el RED y 0 en el GREEN, en local y en la CI;
- 18684 en la suite completa de Core;
- las 21 clases y los dos filtros de UI;
- +33 en `CallerOwnedFacadeGuardTests`;
- `git diff --name-only f4476bc4 4314c180 -- tests` vacío;
- los 21 archivos, todos dentro de `AllowedWriteScope` y ninguno en el prohibido;
- `green-src.patch` igual, byte a byte, a `git diff 3633f08d 4314c180 -- src`;
- los builds: Plugin Release con 2 avisos MSB3277 y 0 errores, en el RED y en el GREEN; Plugin Debug con 0 y 0; Tests con 130 avisos de xUnit, idénticos en el RED y en el GREEN y ninguno en los archivos nuevos.

**No se sostiene, o no se puede verificar (M-08):**
- `TestsExecuted` omite tres corridas locales: `g0` (2923, 7 fallos), `g1` (18678, Core) y `r1` (18684, 130 fallos, un estado del RED anterior a su commit).
- La lista «Guardas y pruebas que pasan ya en el RED»:
  - omite 5 métodos nuevos o cambiados de clases existentes que pasan en la CI del RED: `TheNoReadGuardOfTheUpdateDetectsEachWayToReadInTheRealBody` (4 filas), la fila nueva de `TheDrainGuardDetectsACallOfTheIntentionInTheRealTextOfTheDrainOfTheHostCore`, `NoOperationOfTheIntentionIsCalledFromTheDrainOrTheIdleOfAnyFile`, `TheSignaturesOfTheTwoOperationsAndTheirValuesNameNoTypeOfADraftNorOfThePilot` y `TheUpdateOfThePilotIsAPublicMethodOfTheCore…`;
  - da por pasada `TheGuardOfTheWrapperTripsOnEachChangeOfTheWrapper`, con 2 de sus 5 filas en rojo.
- La medida de la base en Debug («0 y 0») no tiene registro conservado.
- La justificación de los textos nuevos (I-01) no es exacta.
- `Deviations` vacío frente a I-01.

---

## 10. Tabla de hallazgos

| Id | Gravedad | Contenido | Evidencia | Disposición propuesta |
|---|---|---|---|---|
| I-01 | IMPORTANTE | **El apartado «Textos» de D-T4B-07 no se cumple y no se declara como desviación.** La puerta propia usa dos literales nuevos del núcleo y la causa `UpdateNotRunReason` es una constante nueva del núcleo, cuando «los rechazos y las causas son constantes de la corrida». Las constantes de la corrida no habrían alterado la tabla de 30 literales, que solo cuenta esos 30 textos, así que la justificación de WorkCompleted no se sostiene. La guarda escrita para la regla solo mira el cuerpo de `TryUpdatePilot`, y el motivo la esquiva por `out reason`. `Deviations: []` | `4314c180:src/RackCad.Application/Workspace/WorkspaceHostCore.cs:48`, `:852`, `:861`, `:870`, `:915` y `:921`; `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:1108-1110`; `WorkspaceHostHardeningGuardTests.cs:470-487`; paquete D-T4B-07, «Textos» | Elegir una de dos: (a) ratificarlo expresamente como desviación declarada, con una adenda a la entrega, o (b) una corrección acotada: los tres textos como constantes de `PushBackPilotUpdateRun`, y la guarda extendida a `UpdateGate` con su negativa |
| M-01 | MENOR | **Totalidad defensiva.** El `catch` exterior de `Execute` da siempre NotAppliedFailure, sin mirar si `Seal` volvió. En el host, `!closed.Accepted` responde «no llegó a ejecutarse», sea cual sea el resultado. Hoy los dos son inalcanzables, pero un cambio futuro daría «no aplicado» con la escritura ya confirmada | `PushBackPilotUpdateRun.cs:137-147`; `WorkspaceHostCore.cs:870` | Anotarlo. Endurecimiento opcional: un estado de `Seal` visible desde fuera de `Run`, o CommitThrew en lugar de NotAppliedFailure |
| M-02 | MENOR | §9 I-7, «al salir, se comprueba registrando en el log», no está implementado. Solo existe la comprobación de `Post`, convertida en clasificación | `RackPilotUpdateWriter.cs:156-158` (no hay registro en el archivo) | Declarar que la clasificación AppliedPostRereadFailed(`TOP_TRANSACTION_OPEN`) sustituye al registro, o llevar el registro a T4C |
| M-03 | MENOR | Todo `Status == Error` da `ClosedInError`, también un error diferido (`EnterError` durante la corrida) tras un Applied o un Conflict. La respuesta pierde la clasificación, los avisos y las diferencias, aunque la escritura se hizo. La fila 17 del paquete solo nombra el respaldo de I-10 | `WorkspaceHostCore.cs:872-877`; `WorkspacePilotUpdateHostTests.cs:383-397` | Declararlo como interpretación, o que T4C muestre también la clasificación |
| M-04 | MENOR | **Las decisiones que el paquete no fija.** Una importación con `Ok = false`, **o nula**, da NotAppliedPreflight; una lectura o un inicio de MUTATE nulos dan NotAppliedFailure, así que no casan. Un `Failed` puede llegar después de importar (OM-5). Un fallo de los nombres deseados da un aviso; es inalcanzable en la práctica y la comparación byte a byte sigue verificando | `PushBackPilotUpdateRun.cs:321-322` y `:478-489`; `RackPilotUpdateWriter.cs:93` y `:101` | Que el Coordinator lo ratifique. Opcional: la importación nula como NotAppliedFailure |
| M-05 | MENOR | `CompleteApply` evalúa el comparador dos veces: en `StageApplied`, antes de liberar la ranura, y en `Status`, tras el `Rebase`. Un comparador no determinista que lance la segunda vez deja Unknown(PostReread) con `Rebased = false` sobre un borrador ya rebasado. La rama CommitThrew de `Recover` es inalcanzable | `DraftSession.cs:600`, `:694-698` y `:627-633` | Anotarlo como límite. Ninguna acción obligatoria |
| M-06 | MENOR | **Pruebas débiles o con nombres que prometen más de lo que comprueban:** `TheRunKeepsNoText…` (montajes independientes); `…SeesAnotherScale…` (la rama de escala, sin ejercer); `…DoesNotNormalizeAnything` (solo la clase de resultado); los nombres deseados sin el caso compuesto ni el posterior; `TheAnswerOfAnUpdateAlwaysComesFromTheClose…` (solo reflexión); `…CopiesOfTheLists` (no comprueba copias y `Closed` no copia) | `WorkspacePilotUpdateRunTests.cs:691-705`, `:754-777`, `:800-810` y `:826-835`; `WorkspacePilotUpdateHostTests.cs:400-414` y `:436-458`; `WorkspaceEditIntent.cs:477-483`; `PushBackPilotUpdateRun.cs:172-173` y `:205-206` | Pruebas de seguimiento en T4C o en un menor aparte: la causa exacta, la escala sola, el caso compuesto y posterior, y renombrar o completar las dos de host |
| M-07 | MENOR | **Robustez de las guardas:** (a) la fila `public IReadOnlyList…` de la negativa del resto nunca casa y cae en silencio en el respaldo; (b) «el núcleo justo después de `Traverse`» lee el texto crudo, así que un comentario de documentación daría un falso positivo; (c) el censo no ve una referencia como grupo de métodos; (d) la normalización colapsa los blancos dentro de los literales y no reconoce `@$"`, sin exposición hoy | `WorkspacePilotUpdateGuardTests.cs:609-618`, `:502`, `:645-657` y `:385-444` | Anotarlo. Endurecimiento opcional |
| M-08 | MENOR | **La evidencia, incompleta o inexacta:** tres corridas locales que no se declaran (`g0`, `g1` y `r1`, todas de Core, así que INV-18 se cumple); la lista de las que pasan en el RED omite 5 métodos y da por pasado uno con 2/5 filas en rojo; la base en Debug, sin registro | `artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012820Z-7a4a/tests/g0/w.trx`, `tests/g1/full.trx` y `tests/r1/full.trx`; `red-ci-core/core.trx`; `worker-handoff.json`, `TestsExecuted`, `Evidence[2]` y `[16]` | Una adenda a la entrega en la custodia |
| M-09 | MENOR | **Cambios cosméticos sin motivo en líneas existentes:** `NotKeepableReason ="…"` y `PrepareFailurePrefix ="…"` pierden el espacio, y ninguna guarda lo exige. El `CoreLiteral` en crudo incluye las líneas en blanco de `:111` y `:162`, que no tienen efecto | `WorkspaceHostCore.cs:49`; `PushBackPilotUpdatePlan.cs:301`; `WorkspacePilotUpdateGuardTests.cs:876-877` y `:924-925` | Restaurarlos cuando se vuelva a tocar el archivo, o aceptarlos |
| M-10 | MENOR | **La letra de INV-14:** el censo de portadores se amplía sin autoprueba negativa (es por reflexión); la contención como `Failed` va en guardas nuevas y no como cambio de `:583-596`; hay una prueba de reflexión nueva sin declarar en `WorkspaceEditIntentHostTests`, inocua, que duplica otra de host | `WorkspaceEditIntentHostTests.cs:984` y `:1003-1021`; `WorkspaceEditIntentGuardTests.cs:583-596` y `:1048-1096` | Que el Coordinator lo ratifique |

---

## 11. Veredicto

**ACEPTABLE CON MENORES**: 0 bloqueantes, 1 importante (I-01) y 10 menores (M-01 a M-10).

**El producto se ajusta al contrato en lo que decide el comportamiento:**
- la tabla de frontera, con su precedencia y sus dos salvedades;
- la relectura como autoridad;
- POST sin normalizar y con `plan.OriginKey`;
- el escritor de operaciones primitivas, con una transacción, la purga (b) en `Post` y su orden;
- el movimiento exacto de `Traverse`;
- `CompleteApply` por rama;
- la ranura;
- el RED, por aserción;
- la contención.

**I-01** es una desviación de la letra de D-T4B-07 que la entrega no declara. Necesita una disposición antes del cierre: ratificarlo o hacer la corrección acotada de la tabla. Ninguno de los menores impide el cierre.
