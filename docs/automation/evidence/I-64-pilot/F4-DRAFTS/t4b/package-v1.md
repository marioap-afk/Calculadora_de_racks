# I-64 — Paquete del contrato de F4-T4B (MUTATE y POST del Actualizar del piloto)

**Estado:** v1. La prepara la sesión principal el 2026-11-14. No declara GATE PASS y **no se emite**: solo se prepara y se revisa (`owner-f4t4a2-overnight-decisions.md`, pregunta 4: «Solo paquetes»).

**Precondiciones de emisión, que hoy no se cumplen:**
- F4-T4A2 cerrada. Hoy está en STOP en la verificación `4b2a`, a la espera del Owner.
- F4-T1B emitida, ejecutada y cerrada (R-T4-12). Hoy está en paquete v3.
- Una orden del Owner que autorice la emisión y decida las interpretaciones del §9.

**Código base:** `c3ae1c97`, con el producto de F4-T4A2 en `ae0a7b40`.
- El paquete de T4 pide fijar T4B «sobre el código real de la tarea anterior» (`T4P:177`). Si T4A2 o T1B cambian producto, los hechos del §2 se vuelven a medir.

**Mapas** (se custodian con el contrato): `map-1-plugin-write-path.md` (M1), `map-2-core-apply-plan.md` (M2) y `map-3-carries-guards-rules.md` (M3).

**Abreviaturas:**
- `T4P` = `docs/initiatives/I-64-f4-t4-gate-contract-package.md`
- `A2P` = `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md`
- `FRZ` = `docs/initiatives/I-64-proposal-v5.md`
- `TW` = `tests/RackCad.Tests/Workspace/`

**Presupuesto:** 1 de 3 (F4-E09).

## 1. Autoridades

- **El paquete de F4-T4** (`T4P`):
  - la fila de T4B en §3 (`:172`): «La transacción atómica, la lectura caller-owned (decisión 4 del Owner), las fachadas, las unidades, la importación y POST, probados con puertos simulados y guardas»; escrituras authored: Sí;
  - el orden A1 → A2 → B → C (`:175-177`);
  - las decisiones 3 y 4 del Owner (`:182-183`);
  - §3.1 y §3.2 (`:185-213`);
  - D-T4A1-04, la correspondencia entre `Prepare` y T4B (`:397-407`);
  - §7, **R-T4-01 a R-T4-04, R-T4-11 a R-T4-13 (B) y el oráculo de R-T4-06** (`:526-583`).
- **Las órdenes del Owner:**
  - `owner-f4t4a2-authorization.md`: TOCTOU, «T4B MUST reread/revalidate again immediately before MUTATE» (`:307-317`); el plan inmutable (`:320-334`); «T4B remains the first and only task in F4 authorized to write authored DWG state» (`:395-400`); Smoke-2 no antes de cerrar T4B (`:403-408`);
  - `owner-f4t4-preparation-authorization.md`: el oráculo de T4B, todo o nada y el cerrojo de T1 (`:126-135`), y POST por relectura (`:136-147`);
  - `owner-f4t4-decomposition-decision.md`.
- **El Freeze** (`FRZ`):
  - D-12 (`:463-498`): la autoridad de la relectura dentro de MUTATE, la comparación y O-04;
  - D-13 (`:500-545`): todo o nada, la vía, la tabla por frontera e INV-30 aditiva;
  - INV-06, INV-10 (`:748`), INV-12 (`:750`), INV-14 (`:752`), INV-30 (`:768`) e INV-39 (`:777`);
  - M-02 (`:995`);
  - las obligaciones `:1035` y `:1038`.
- **El paquete de F4:** C-F4-05 (`docs/initiatives/I-64-f4-gate-contract-package.md:61`).
- **F4-E25 (4), F4-E07, F4-E16, F4-E51 y F4-E53 a F4-E56** (`F4-DRAFTS/f4-dispositions.md`).
- **El paquete de F4-T4A2:** §8 (`A2P:653-662`) y D-T4A2-06 (`:349`, `:359`).
- **El paquete de F4-T1B** (cuando se emita): su clausura (§3) y CT-24 a CT-27.

## 2. Hechos medidos en `c3ae1c97`

Hay más detalle en los mapas. Los hechos de aquí condicionan las decisiones.

### 2.1 El núcleo

**H-01. La ranura de aplicación** (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs`):
- **`TryBeginApply` (`:508-524`)** solo admite Clean o Dirty, sin texto pendiente y con la ranura libre. Si no, devuelve `AnotherUpdateInProgress` (`:515`; `T4P` citaba `:474`). Cuando admite, deja el borrador en Applying y vacía `Warnings`.
- **`CompleteApply` (`:531-554`)** libera la ranura **antes** de clasificar (`:537-538`).
- **`Classify`:**
  - `Applied` y `AppliedWithWarnings` con una relectura admitida dejan Clean (copian el estado inicial y vacían el pendiente);
  - con una relectura nula o no admitida, dan Unknown(PostReread) con `LastApplyCause` = el diagnóstico de la relectura, y **los avisos del resultado se pierden**;
  - `CommitThrew` da Unknown(CommitThrew).

**H-02. F4-E25 (4) está abierto** (M2 §1). Si el copiador lanza en `Classify`, la ranura ya está liberada, el borrador queda sobre la base anterior y el error diferido no se aplica. La prueba `TW/WorkspaceDraftPilotOwnershipTests.cs:417-458` **exige** hoy que se lance.

**H-03. `DraftApplyOutcome`** (`DraftApplyOutcome.cs`) tiene las fábricas `NotAppliedConflict`, `NotAppliedAdmission`, `NotAppliedPreflight`, `NotAppliedFailure`, `CommitThrew`, `AppliedPostRereadFailed(cause)`, `Applied(postRead)` y `AppliedWithWarnings(postRead, warnings)`.
- «Aplicado con avisos» ya existe en el núcleo.
- `AppliedPostRereadFailed` no lleva avisos.

**H-04. El plan** (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs`) es inmutable y de valores. Lleva `ExpectedBase`, `ExpectedVariableRegistryText` (siempre nulo en el piloto), los miembros y sus sobres de escritura, y `PostExpectation`.
- **No lleva** el `PushBackSystem`, el extremo y el lado del corte frontal, ni ningún `ObjectId` (M1 riesgo 1).
- `Prepare(input, pilot, catalog)` necesita el catálogo.
- Menores abiertos: M-5 (`:379-381`) y M-6 (`:308-311`).

**H-05.** `ComposeForWrite` (`PushBackPilotMaterializer.cs:196-234`) es correcto, pero **ninguna guarda lo fija** (M-1).

**H-06. El host** (`WorkspaceHostCore.cs`) no tiene ninguna operación de Actualizar.
- Varias guardas prohíben en `WorkspaceHostCore.cs`, `IWorkspaceEditIntentReader.cs`, `WorkspaceEditIntent.cs`, `IWorkspacePilotEditPort.cs` y `WorkspacePilotSupport.cs`, **incluso en los comentarios**, `Transaction`, `Database`, `LockDocument` y `Commit(` (`TW/WorkspacePilotEditGuardTests.cs:812-849`; `TW/WorkspaceEditIntentGuardTests.cs:1002-1015`).
- Prohíben también las API de escritura en archivos enteros (`TW/WorkspaceLifecycleGuardTests.cs:667-684`).
- El lector tiene un único método, fijado por guarda. Los borradores solo se alcanzan desde el host.

### 2.2 El Plugin

**H-07. Las tres fachadas de Push Back** exponen `PrepareRedraw` y `RedrawInTransaction`, caller-owned, sin ningún llamador en producción.
- `PrepareRedraw` **importa la biblioteca antes de cualquier transacción** (`ViewBlockDraw.cs:86`) y fija el `ObjectId` del bloque en un `PreparedViewRedraw` (M1 H-1 a H-3).
- `CallerOwnedFacadeGuardTests.Facades` (`:49-56`) no las incluye. Sus propiedades ya se cumplen sobre su texto (M1).

**H-08. No hay ninguna variante caller-owned de la lectura.**
- `CaptureDrawing` → `Traverse` toma su propio bloqueo, su transacción y su `Commit` (`RackSiblingScan.cs:112-163`).
- `ScanEnvelopes` sí es caller-owned, pero omite los xref.
- Las guardas fijan las firmas, un `new RackSiblingDrawingScan(` y un `.Commit(` (`TW/WorkspaceEditIntentGuardTests.cs:88-128`). Son compatibles con extraer un núcleo interno de recorrido que reciba la transacción (M1).

**H-09. INV-30 no tiene precedente.**
- Ningún camino pone `Commit()` en su propio `try`.
- El único inyector Debug es por unidad (`RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT`).
- La costura de I-55 está congelada: `SiblingRedrawSeamGuardTests.cs:10-22` exige un `StartTransaction(` y un `Commit()`.

**H-10. Dónde puede vivir la escritura** (M3 §3 C):
- **No puede** en `Application/Workspace`, `Plugin/Workspace` (9 archivos fijos y `Regen` prohibido), el archivo del plan, un `*Commands*.cs` ni los caminos clásicos (CT-27 de T1B).
- **Puede** en `src/RackCad.Plugin/Views/`, que no tiene censo, y en archivos nuevos de `src/RackCad.Application/Systems/PushBack/Pilot/`, que está fuera de la clausura de T1B.

**H-11. POST:**
- `PurgeAfterCommit` y `SyncName` son *best effort*, con transacción propia, y se tragan las excepciones.
- `ApplyRegen` es la autoridad única del regen.
- `CaptureDrawing` sirve sin cambios para la relectura de después del `Commit`.
- `RackBlockData.Write` trocea el texto cada 255 caracteres y **vuelve en silencio con un JSON vacío** (M1 H-5). Solo el host puede medir si conserva los bytes.

**H-12. `MutationDestinationBinding`** recibe tipos de Selectivo y corre en PREPARE. Para el piloto hay que replicar su semántica dentro de la transacción (M1 H-9).

**H-13. `TopTransaction == null`:** los precedentes usan `Debug.Assert` (el adaptador) o solo registran en el log (el puente, porque un `Assert` terminaría AutoCAD) (M1 riesgo 8).

## 3. Lo que hace T4B y lo que deja a T4C (D-T4B-01)

| Pieza | T4B | T4C |
|---|---|---|
| La lectura caller-owned (R-T4-01, INV-39) y la equivalencia de F4-E26 por construcción | **Sí** | — |
| La corrida pura PREPARE → MUTATE → POST, con su clasificación por frontera (INV-30, INV-06, INV-12, C-F4-05), probada con puertos simulados | **Sí** | — |
| El escritor del Plugin: importación, transacción, relectura, destino, unidades, `Commit` en su propio `try`, la inyección Debug y POST | **Sí** | — |
| La operación del host `TryUpdatePilot`, con la disciplina de la ranura (el oráculo de R-T4-06) y sin contexto de comando | **Sí** (§9 I-2) | Su llamada desde la UI |
| `CompleteApply` robusto (F4-E25 (4)) y los avisos que sobreviven a una relectura fallida (F4-E16) | **Sí** | — |
| Los menores M-1, M-5, M-6 y M-10 | **Sí** | — |
| El contexto de comando, el único `using LockDocument`, la marca de actualización en curso, el ámbito de mutación propia, la UI de Actualizar, Undo, K = 1 y las presentaciones (R-T4-05 a R-T4-10) | — | **Sí** |
| El diagnóstico de admisión en la vía `TryCheckNow` (arrastre del §8 de T4A2) | — | **Sí**, reasignado (§9 I-3) |
| C2-M-3 a C2-M-7 | Solo donde T4B toque esas superficies (ninguna, en el diseño) | Las de su superficie |

**Por qué la operación del host va en T4B:**
- R-T4-06 hace de la ranura y del cerrojo «oráculo de T4B y T4C».
- Los borradores solo se alcanzan desde el host.
- Sin una entrada en el host, la corrida no se puede probar con un borrador real.

La operación es síncrona y no tiene llamador de producto hasta T4C. Así no adelanta nada de C (§9 I-2).

## 4. Decisiones de F4-T4B

### D-T4B-01. El reparto

El del §3.

### D-T4B-02. La corrida pura y su puerto

**Dos archivos nuevos en `src/RackCad.Application/Systems/PushBack/Pilot/`:**
- **`PushBackPilotUpdateRun.cs`**, la corrida pura.
  - Recibe la entrada de valores del borrador (`PushBackPilotPrepareInput.From(draft)`, que calcula el host) y el puerto.
  - Devuelve un `DraftApplyOutcome<PushBackPilotState>` y los avisos.
  - No nombra `Transaction`, `Database`, `LockDocument`, `Commit(` ni ninguna API de AutoCAD. Una guarda de pureza lo fija, con la forma de `TW/WorkspacePilotPrepareGuardTests.cs:27-79`.
- **`IPushBackPilotUpdatePort.cs`**, el puerto. Sus operaciones son de valores:
  1. **`ReadForPrepare()`:** la lectura de solo lectura con su catálogo (O-04, la comparación previa a importar), como resultado del piloto y catálogo. Vuelve a leer, como pide la orden TOCTOU.
  2. **`Import(plan)`:** la importación de biblioteca, antes de la transacción authored (R-T4-03). Devuelve los avisos de piezas faltantes y, si falla, una causa.
  3. **`Mutate(plan)`:** abre **una** transacción poseída y dentro hace la relectura caller-owned, la comparación con `plan.ExpectedBase`, la comprobación de destino, C-F4-05, las unidades y `Commit()` en su propio `try`. Devuelve un resultado de valores: `Conflict(diferencias)`, `Preflight(causa)`, `Failure(causa)` (antes de `Commit`), `CommitThrew(causa)` o `Committed`.
  4. **`Post(plan)`:** `SyncName`, la purga, un `Regen` y la relectura. Devuelve la relectura y los avisos de los pasos *best effort*.

**La corrida, en orden:**
- `Prepare` con la lectura previa. NotApplicable, PendingText, NothingToApply, Conflict, NotAdmitted o Invalid dan su resultado sin importar ni escribir (D-T4A1-04).
- La importación.
- `Mutate`. Las clases antes de `Commit` dan No aplicado con su causa; `CommitThrew` da `CommitThrew` (INV-30).
- Si `Mutate` dio `Committed`, `Post`:
  - el comparador de POST (D-T4B-05) decide entre `Applied` o `AppliedWithWarnings` y `AppliedPostRereadFailed`;
  - **los avisos sobreviven** en todos los casos (D-T4B-06).

### D-T4B-03. El escritor del Plugin

Un archivo nuevo, `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`, implementa el puerto. **Ningún** `*Commands*.cs, `Plugin/Workspace/` ni camino clásico.

**La lectura caller-owned** (R-T4-01, INV-39):
- un núcleo interno de recorrido en `RackSiblingScan.cs`, que recibe `(Transaction, Database)`;
- `Traverse` conserva su firma, su bloqueo y su único `Commit` como envoltorio de ese núcleo;
- las cuentas de las guardas (`TW/WorkspaceEditIntentGuardTests.cs:88-128`) se conservan: un `new RackSiblingDrawingScan(`, un `RackBlockData.Read(`, un `.Commit(`;
- `eraseByKind` sigue nulo (C-F4-05), con la misma `CompletenessEvidence`;
- la equivalencia de F4-E26 se cumple **por construcción**, porque los tres caminos usan el mismo núcleo y la misma `BuildStaleBase`. La prueba en un dibujo real es de Smoke-2.

**MUTATE**, dentro de una transacción poseída (R-T4-02):
1. La relectura caller-owned, con la identidad de la transacción comparada por `UnmanagedObject` (`RackDefinitionCreator.cs:156-164`).
2. `DraftStaleBase.Create(plan.ExpectedBase, plan.ExpectedVariableRegistryText).CompareWith(relectura)`. Con cualquier diferencia: `Conflict` y **cero unidades** (INV-06). Es la autoridad de D-12.
3. **La comprobación de destino:** la semántica de `MutationDestinationBinding` sobre las claves de `plan.Members` frente a los `Mutable` de la relectura (H-12), y el `ObjectId` de cada `PreparedViewRedraw` igual al de la relectura para su clave.
4. **C-F4-05:** los supervivientes (`Mutable` con `LayoutReferenceCount ≥ 1`) se cuentan en los hechos de la relectura. Con cero: `Preflight` con su causa y cero escrituras.
5. **Las unidades:** `RedrawInTransaction` de la fachada de cada miembro, con el texto de sobre del plan (INV-10). Una unidad que falla: `Failure`, sin `Commit` y con la transacción descartada (INV-12).
6. **`Commit()` en su propio `try`:** si lanza, `CommitThrew`.
7. **La inyección Debug** de INV-30, bajo `#if DEBUG` y sin `#else`, por variables de entorno con el patrón de `SiblingRedrawDebugFaultInjection`: un fallo en la unidad k y un fallo en `Commit()`.
8. **Al salir:** `TopTransaction == null`. Se comprueba registrando en el log y con la clasificación ya hecha, sin `Debug.Assert` (§9 I-7).

**Lo que nunca hace:** borrar una vista (`Erase(` o `EraseViewBlocks`) (R-T4-11); aplicar cambios que el piloto no posee; usar `RackUnitsGuard`, porque es una actualización pura.

**Los datos que el plan no lleva** (H-04):
- **el `PushBackSystem`** se reconstruye en el escritor deserializando el texto de diseño del plan y resolviéndolo con el catálogo de la lectura previa;
- **el extremo y el lado del corte** se obtienen con `RackViewCodec`, el decodificador del plan, como el único;
- **los `ObjectId`** solo viven dentro de la operación del escritor (orden del plan inmutable).

Todo se declara (§9 I-8). Una prueba de Core demuestra que `RackViewCodec` y `DecodeSection` coinciden en las vistas del piloto.

**POST:**
- `SyncName` con los nombres de `RackViewBaseName` (como RACKEDITAR);
- la purga;
- **un** `ApplyRegen`;
- la relectura con `CaptureDrawing` sin cambios.

Los pasos *best effort* que fallan dan avisos.

### D-T4B-04. La importación y su residuo

- **El orden:** PREPARE (O-04 y la comparación de solo lectura) → la importación → MUTATE.
- **El residuo:** un Conflicto que solo detecte MUTATE deja definiciones de biblioteca importadas (OM-5), como admite D-13.
- **Las piezas faltantes** dan avisos (`AppliedWithWarnings` si POST coincide).
- **`LegacyDeltas`** (decisión 3) no es un símbolo del código: es la canonicalización de los dibujos antiguos, que la expectativa de POST ya contiene. T4B no añade código para ella y Smoke-2 la observa con un dibujo antiguo.

### D-T4B-05. El comparador de POST

Es puro y está en la corrida.
- Con la relectura de POST admitida:
  - para **cada** clave de `PostExpectation.Envelopes`, la entrada `Mutable` de la base de POST con un `EnvelopeText` **ordinalmente** igual;
  - el mismo conjunto de claves;
  - la escala igual.
- **Si coincide:** `Applied(postRead)` o `AppliedWithWarnings(postRead, avisos)`.
- **Si no coincide o no está admitida:** `AppliedPostRereadFailed(causa, avisos)`, que da Unknown(PostReread).
- **No se normaliza nada.** Si en el host la vía de escritura no conserva los bytes, es STOP a la sesión y la comparación no se debilita (R-T4-04). Se mide en Smoke-2 (§9 I-6).

### D-T4B-06. El núcleo: `CompleteApply` robusto y los avisos que sobreviven

En `DraftSession.cs`, sin ningún archivo nuevo en `Drafts/` y sin `Transaction`, `Commit(` ni `Debug.`:
- **F4-E25 (4):**
  - la copia del estado se hace **antes** de liberar la ranura;
  - si el copiador o el comparador lanzan, el borrador queda en Unknown(PostReread) con una causa, la ranura se libera exactamente una vez y la excepción no escapa;
  - el error diferido se sigue aplicando.
  - La prueba `TW/WorkspaceDraftPilotOwnershipTests.cs:417-458` cambia de forma declarada: el comportamiento corregido es el que pide el arrastre.
- **F4-E16:**
  - `AppliedPostRereadFailed(cause, warnings = null)` gana un parámetro opcional en `DraftApplyOutcome.cs`;
  - `Classify` conserva `Warnings` también en la rama de la relectura fallida.
- **M-10:** el XML de la sobrecarga de Conservar documenta la precondición «`keepOn` no muta sus argumentos», sin cambiar la fórmula.
- **R-3 y M-4:** toda asignación de `ErrorReason` sigue la guarda `TW/WorkspacePilotPrepareGuardTests.cs:253-384`.

### D-T4B-07. La operación del host `TryUpdatePilot` (§9 I-2)

**Dónde vive:** en `WorkspaceHostCore.cs`. Solo orquesta valores y delega en el puerto, que se inyecta como `SetEditIntentReader` (`:274-275`). En el texto del archivo no aparece ni `Transaction` ni `Commit(`.

**La disciplina de la ranura** (INV-F4T1-20, el oráculo de R-T4-06):
- la sesión y el documento se validan exactamente;
- NothingToApply, PendingText y NotApplicable se deciden **antes** de `TryBeginApply`;
- `TryBeginApply`; si da `AnotherUpdateInProgress`, no tiene efecto;
- la corrida;
- `CompleteApply` en `finally`, **exactamente una vez** por salida;
- Error y Descartar no liberan la ranura antes de tiempo.

**El resultado:** un `EditUpdateOutcome` de valores, en `WorkspaceEditIntent.cs`, con la clase, la causa, las diferencias y los avisos. T4C los presenta.

**El plan:** es un valor local de la operación, nunca un campo (`TW/WorkspaceEditIntentHostTests.cs:1002-1022`).

**Sin cambios:**
- la operación no añade ninguna lectura del lector del host. D-1 (6 lecturas) no cambia: la lectura de T4B va por el puerto de actualización;
- no hay contexto de comando, `async` ni bloqueo: eso es T4C.

**Cambios de guarda declarados**, con sus autopruebas en negativo (§6, INV-F4T4B-14):
- `Operations` (15 → 16);
- `ChangesADraft` y `PortMutatingOperations`;
- la tabla de literales del núcleo (30 → 30 + n);
- `AllowedConsumers` y `PilotForms`, solo si hace falta, con la forma exacta.

### D-T4B-08. Los menores y los arrastres

| Id | Qué hace T4B |
|---|---|
| M-1 | Una guarda de fuente del cuerpo de `ComposeForWrite`: no aplica `Copy(` a `current`, copia `initial` y compara con `current` tal cual. Lleva una autoprueba en negativo con la mutación a `Copy(current)`. El mapa de evidencia se corrige |
| M-5 | `decoded.HasAddress` se comprueba antes de la disponibilidad, con su texto propio («descriptor de vista inválido: usa RACKEDITAR»). `TW/WorkspacePilotPrepareTests.cs:1758` y `:1772` cambian de forma declarada |
| M-6 | `catalog == null` va al principio de `Build`, de modo que un catálogo nulo con diferencias da Conflict |
| M-10 | D-T4B-06 |
| D-1 | Sin cambio (D-T4B-07) |
| F4-E25 (4), F4-E16 | D-T4B-06 |
| Diagnóstico de `TryCheckNow` | **A T4C** (§9 I-3): obliga a tocar el VM del campo, que está fuera del alcance de T4B, y rompe M-3 o M-2 de T4A2 |
| C2-M-3 a C2-M-7 | T4B no toca sus superficies |
| m-1 y D2-1/D2-2 de T4A2 | Los dispone el cierre de T4A2, que corresponde al Owner |

### D-T4B-09. Las guardas

**Extendidas:**
- `CallerOwnedFacadeGuardTests.Facades`, con las tres fachadas de Push Back;
- una guarda de la variante de lectura caller-owned (INV-39: sin `StartTransaction`, `LockDocument` ni `Commit`).

**Nuevas:**
- **la del escritor** (`RackPilotUpdateWriter.cs`), con el patrón de `ProjectVariableMutationExecutorTests:212-337`:
  - un `StartTransaction` y un `Commit()`, con el `Commit()` en su propio `try`;
  - `PrepareRedraw(` antes de `StartTransaction`;
  - dentro, en orden: la relectura, la comparación, el destino, C-F4-05 y las unidades;
  - `SyncName` y la purga después del `Commit`, y un solo `ApplyRegen`;
  - sin `Erase(` ni `EraseViewBlocks`, sin `RackUnitsGuard`, sin `async`/`await` y sin `ExecuteInCommandContextAsync`;
- **la inyección Debug de INV-30**, bajo `#if DEBUG` sin `#else`;
- **la pureza** de la corrida y del puerto;
- **M-1**.

**Se mantienen sin cambios:**
- `SiblingRedrawSeamGuardTests` (la costura de I-55 no se toca: Insertar no cambia, INV-30);
- las pruebas de la costura (`RackSiblingRedrawTests.cs`);
- las guardas del Plugin del piloto que no se declaran;
- las pruebas de T1B (CT-24 a CT-27 y las de UI).

## 5. Trazabilidad

| Regla u obligación | Dónde |
|---|---|
| R-T4-01 / INV-39 / F4-E26 | D-T4B-03 (la lectura), D-T4B-09 |
| R-T4-02 / INV-06 / INV-12 / INV-14 / INV-30 | D-T4B-02, D-T4B-03 |
| R-T4-03 / OM-5 / decisión 3 | D-T4B-04 |
| R-T4-04 / C-F4-05 / F4-E25 (4) | D-T4B-03, D-T4B-05, D-T4B-06 |
| R-T4-06 (oráculo) / INV-F4T1-20 | D-T4B-07 |
| R-T4-11 (nunca borrar, solo lo del piloto) | D-T4B-03 |
| R-T4-12 (T1B antes) | Precondición |
| TOCTOU y el plan inmutable (Owner) | D-T4B-02 (`ReadForPrepare` y la relectura en MUTATE), D-T4B-03 (los `ObjectId` solo en el escritor) |
| INV-10 | D-T4B-03, paso 5 |

## 6. Contrato propuesto de F4-T4B (a fijar en la emisión)

**Identidad:**
- `TaskId` `F4-T4B-MUTATE-POST`;
- I-64, F4;
- `attempts` 1 de 3;
- las celdas de F4-T4A2, con nc4 en la primera delegación y nc1..nc3.

**`AllowedWriteScope`**, que se fija por ruta exacta en la emisión:
- **Application:**
  - nuevos: `Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs` e `IPushBackPilotUpdatePort.cs`;
  - modificados: `Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs` (M-5 y M-6), `Workspace/Drafts/DraftSession.cs`, `Workspace/Drafts/DraftApplyOutcome.cs`, `Workspace/WorkspaceHostCore.cs`, `Workspace/WorkspaceEditIntent.cs` y, si la inyección lo pide, `Workspace/WorkspaceController.cs`.
- **Plugin:** nuevo `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`; modificado `src/RackCad.Plugin/Views/RackSiblingScan.cs` (el núcleo interno de recorrido).
- **Pruebas:**
  - clases nuevas en `tests/RackCad.Tests/Workspace/` con el prefijo `WorkspacePilotUpdate`;
  - `tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs`;
  - los archivos de guarda de los cambios declarados del D-T4B-07 y del D-T4B-08, por ruta exacta;
  - `TW/WorkspaceDraftPilotOwnershipTests.cs` y `TW/WorkspacePilotPrepareTests.cs` (cambios declarados);
  - los dobles de Core.

**`ForbiddenWriteScope`, por raíces:**
- `src/RackCad.UI/` **entero**: T4B no tiene UI;
- `src/RackCad.Plugin/Workspace/`;
- la clausura de T1B (sus fachadas, `RackPushBackCommands.cs` y `KindHandlers/PushBackKindHandler.cs`, entre otros archivos);
- `src/RackCad.Plugin/Systems/Shared/SiblingRedrawTransaction.cs` y el resto de la costura de I-55;
- los `*Commands*.cs`;
- `RackPilotIntentAdapter.cs`;
- `Systems/PushBack/Pilot/PushBackPilotMaterializer.cs` (M-1 es solo una guarda);
- el resto de `src/RackCad.Application/` fuera de lo permitido;
- `src/RackCad.Domain/`, `.github/`, `docs/` y las pruebas de T1B.

**`RequiredTests`:**

| Proyecto | Filtro | `MinSelected` | `ExpectRed` |
|---|---|---|---|
| Core | `FullyQualifiedName~RackCad.Tests.Workspace` | la cuenta de la base + las nuevas (se fija en la emisión) | true |
| Core | `FullyQualifiedName~RackCad.Tests.CallerOwnedFacadeGuardTests\|FullyQualifiedName~RackCad.Tests.SiblingRedraw\|FullyQualifiedName~RackCad.Tests.RackSiblingRedraw` | la de la base | false |
| UI | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 601 | false (sin cambios de UI) |

**Invariantes** (el texto definitivo se fija en la emisión):
- **INV-F4T4B-01:** la relectura dentro de MUTATE es la autoridad (D-12 y TOCTOU). Ningún resultado de PREPARE autoriza escribir.
- **INV-F4T4B-02:** con cualquier diferencia contra `plan.ExpectedBase`, Conflict y cero unidades (INV-06).
- **INV-F4T4B-03:** todo o nada.
  - Un fallo en la unidad k da cero escrituras simuladas (INV-12).
  - Una excepción antes de `Commit()` da No aplicado; una en `Commit()` da `CommitThrew` (INV-30).
  - Las pruebas de la costura de I-55 siguen en verde y su archivo no cambia.
- **INV-F4T4B-04:** una transacción poseída. La relectura no abre transacción, no bloquea y no confirma (INV-39). `TopTransaction == null` al salir (INV-14).
- **INV-F4T4B-05:** la importación va antes de la transacción authored. El residuo OM-5 queda declarado.
- **INV-F4T4B-06:** POST sale de la relectura y se compara byte a byte, sin normalizar. Dirty pasa a false solo desde el estado verificado.
- **INV-F4T4B-07:** C-F4-05 se cumple. Nunca se borra una vista. Solo se aplican los cambios que posee el piloto (R-T4-11).
- **INV-F4T4B-08:** el plan es inmutable y de valores. Los `ObjectId` y las transacciones solo viven dentro de la operación del escritor.
- **INV-F4T4B-09:** la disciplina de la ranura de D-T4B-07.
- **INV-F4T4B-10:** `CompleteApply` es robusto (F4-E25 (4)) y los avisos sobreviven (F4-E16).
- **INV-F4T4B-11:** los menores M-1, M-5, M-6 y M-10.
- **INV-F4T4B-12:** sin UI, sin contexto de comando, sin `async` y sin bloqueo de documento nuevo en el host (T4C).
- **INV-F4T4B-13:** los caminos clásicos y Insertar no cambian. Las pruebas de T1B y las de I-55 siguen en verde.
- **INV-F4T4B-14:** los cambios de guarda son solo los declarados (D-T4B-07, D-T4B-08 y D-T4B-09), cada uno con su autoprueba en negativo.
- **INV-F4T4B-15:** el RED con cuerpos inertes, también en las fábricas, igualdades y reenvíos (la lección de M-11). Sin AutoCAD en ninguna prueba.

**Paradas:**
- **C01:** haría falta tocar la UI, el contexto de comando, `async` o un bloqueo de documento.
- **C02:** haría falta tocar la costura de I-55, las fachadas, los caminos clásicos o el adaptador de lectura del piloto.
- **C03:** un cambio de guarda no declarado.
- **C04:** el plan necesitaría llevar un `ObjectId`, una transacción o un objeto del host.
- **C05:** la comparación de POST necesitaría normalizar algo.
- **C06:** las precondiciones de emisión dejan de cumplirse (T4A2 o T1B cambian producto).
- **C07:** la regla de fidelidad de la planificación (F4-E31, F4-E43).

## 7. Smoke-2

T4B no lo ejecuta, porque Smoke-2 necesita T4C (el contexto de comando, la UI y Undo). Lo que T4B no puede probar sin AutoCAD va a Smoke-2:
- que la vía de escritura y `SyncName` conserven los bytes y los handles (R-T4-04: si no, STOP);
- una sola transacción y `TopTransaction == null` en el host;
- la equivalencia de las tres lecturas en un dibujo real (R-T4-01);
- que la caller-owned de Push Back funcione dentro de una transacción ajena (R-09);
- la agrupación de Undo de la importación;
- el escenario Desconocido (S2-R-10 de T4A2) con la inyección Debug de T4B.

## 8. Lo que queda para F4-T4C

R-T4-05 a R-T4-10, el diagnóstico de `TryCheckNow` (§9 I-3), la llamada de la UI a `TryUpdatePilot`, la presentación de los avisos y de Applying, y C2-M-3 a C2-M-7 donde toque.

## 9. Interpretaciones declaradas que se comunican al Owner (decisiones para la emisión)

| # | Interpretación | Alternativa |
|---|---|---|
| I-1 | **«Única escritora»:** T4B es la primera y la única tarea que **crea** escrituras authored. T4C añade el contexto de comando y el bloqueo alrededor del **mismo** escritor, sin ninguna clase nueva de escritura. Así se concilia la orden («T4B remains the first and only task…», rotulada MUTATE/POST/UNDO) con el reparto del paquete de T4 («Sí, la del adaptador del host», `T4P:173`) | Que T4C no tenga escrituras y T4B incluya también el contexto de comando y Undo, contra la descomposición del Owner |
| I-2 | **La operación del host `TryUpdatePilot` va en T4B**, síncrona y sin llamador de producto hasta T4C, para que la ranura sea oráculo de T4B (R-T4-06) | Solo la corrida pura y el escritor, sin probar la ranura con un borrador real hasta T4C |
| I-3 | **El diagnóstico de `TryCheckNow` pasa a T4C.** Toca el VM del campo, que no es del alcance de T4B, y obliga a cambiar M-3 o M-2 de T4A2. La regla del Owner («T4B if mutation/write code owns it») no lo asigna a T4B | Mantenerlo en T4B y añadir un archivo de UI a su alcance |
| I-4 | **INV-30 va en la vía propia del piloto** (el escritor), no dentro de la costura de I-55, que sigue congelada. Insertar no cambia | La frontera dentro de la costura, con un cambio declarado de `SiblingRedrawSeamGuardTests` |
| I-5 | **«Aplicado con avisos»** usa el `AppliedWithWarnings` que ya existe (purga o `SyncName` fallidos, piezas faltantes). Los avisos sobreviven a una relectura fallida (F4-E16) | — |
| I-6 | **El STOP por bytes no conservados** solo se puede medir en el host: se comprueba en Smoke-2. T4B no lo puede cumplir con dobles | — |
| I-7 | **`TopTransaction == null`** en el escritor se comprueba registrando en el log, no con `Debug.Assert`, que terminaría AutoCAD (como en el puente) | `Debug.Assert`, como el adaptador |
| I-8 | **El escritor reconstruye el `PushBackSystem`** desde el texto de diseño del plan con el catálogo de la lectura previa. Usa `RackViewCodec` como único decodificador del corte | Ampliar el plan con el sistema resuelto, que deja de ser solo valores planos sencillos |

## 10. Disposición de las revisiones

(Pendiente: la revisión independiente de la v1.)
