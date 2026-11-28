# Mapa 2 — Freeze de Actualizar, núcleo de borradores y piloto Push Back (preparación de F4-T4)

Medido en solo lectura sobre la rama `architecture/workspace-persistente-rackcad`, HEAD `7297e1f1`. El código es idéntico al de `4e34bb56`: `git diff --name-only 4e34bb56 7297e1f1` solo lista rutas bajo `docs/`. Todas las rutas son relativas a la raíz del repositorio. No se compiló, no se ejecutaron pruebas y no se abrió AutoCAD.

Notación: `archivo:línea`. **IMPL** = implementado (se dice dónde). **PEND** = pendiente. **PARCIAL** = hay piezas pero no hay camino de producto.

---

## 1. El Freeze de F4 sobre Actualizar y T4

### 1.1 Jerarquía de las fuentes

| Fuente | Naturaleza | Cita |
|---|---|---|
| `docs/initiatives/I-64-proposal-v5.md` | Freeze semántico. «**No** se congelan nombres de tipos, helpers, archivos ni mecánica interna» | `:35-39` |
| `docs/initiatives/I-64-f4-gate-contract-package.md` (v2) | Paquete del gate de F4. Propone la descomposición y «No emite contratos: cada tarea emite el suyo» | `:3` |
| `docs/initiatives/I-64-implementation-clarifications.md` | Aclaraciones IC. «Donde una aclaración … y la redacción congelada difieren en el **mecanismo**, manda la aclaración, solo dentro de su alcance» | `:3` |
| `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md` | Decisiones F4-E01..E50 | — |
| Autorización de preparación de F4-T4 del Owner (texto pegado en la sesión; **aún no versionado** en el repo en `7297e1f1`) | Divide T4 en F4-T4A «PREPARE / STALE / CONFLICT» y F4-T4B «MUTATE / POST / UNDO», con una posible F4-T4C | — |

`docs/initiatives/I-64-architect-package-v5.md` no contiene D-13 ni «siete resultados» (ver §1.7). Es el paquete de re-revisión de A64-PV3-04 y A64-PV4-01 (`:1-106`).

### 1.2 Descomposición del paquete de F4 frente a la del Owner

Paquete de F4, §3 (`I-64-f4-gate-contract-package.md:39-49`):
- `:39`: «La mutación solo entra en T4A y T4B.»
- `:47`, **F4-T4A-MUTATION-SEAM** (Application y Plugin, sin panel): «Entrada caller-owned sobre la proyección de T2 (INV-39); frontera de `Commit()` (INV-30) sin cambiar Insertar; inyección Debug en la frontera de `Commit()`; guardas de las fachadas de Push Back; unidades del piloto con su sobre por vista (INV-10); C-F4-05». Escrituras authored: «Solo en pruebas de costura».
- `:48`, **F4-T4B-ACTUALIZAR-HOST**: «Actualizar explícito en el contexto de C-F4-02; **un único `using LockDocument`** alrededor de PREPARE, MUTATE y POST; comprobación previa; relectura y comparación dentro de MUTATE; revalidación de sesión inmediata; ámbito de mutación propia (C-F4-04); POST con relectura; inyecciones Debug de POST y de su relectura; resultados visibles (INV-31) y reconciliación sin fusión automática». Escrituras: «**Solo aquí**».

| Responsabilidad | Paquete F4 v2 | División del Owner |
|---|---|---|
| Comprobación previa O-04 y comparación con la StaleBase | T4B (`:48`) | T4A |
| Relectura y comparación **dentro de MUTATE** (autoridad, D-12) | T4B (`:48`) | T4A dice «authoritative reread before update»; T4B dice «no stale write» |
| Variante de lectura caller-owned (INV-39) | T4A (`:47`) | sin asignar de forma explícita |
| Frontera de `Commit()` (INV-30) e inyección Debug en ella | T4A (`:47`) | T4B («transaction/mutation boundary», «failure semantics») |
| Unidades por vista (INV-10); guardas de fachadas Push Back (H-04); C-F4-05 | T4A (`:47`) | T4B («integrated I-55 membership authority», «applying ONLY pilot-owned changes») |
| «Comprobar ahora» | T2 (marcado y recarga) y T4B (`:48`, reconciliación) | T4A |
| Revalidación inmediata antes de mutar | T4B (`:48`) | T4A («validation immediately before mutation») |
| Bloqueo único PREPARE+MUTATE+POST, contexto de comando, POST, UNDO, ámbito propio | T4B | T4B |

### 1.3 Decisiones del paquete que tocan a T4

| Id | Archivo:línea | Literal | Estado |
|---|---|---|---|
| C-F4-02 | `I-64-f4-gate-contract-package.md:59` | «`ExecuteInCommandContextAsync` como vía declarada, con excepción por archivo exacto y aviso al Owner … La medición en host (escrituras y UNDO) **va dentro de T4B** … Si UNDO no es un solo paso: A-n material hacia comando registrado (D-13)» | Ratificada (`f4-dispositions.md:106-109`: «T4B puede usar `ExecuteInCommandContextAsync` solo para la mutación explícita de Actualizar … nunca para la navegación»). **PEND**: la guarda la prohíbe hoy en todo el producto (`tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:436`), y `async`/`await` en Plugin y Application (`:434`) |
| C-F4-04 | `:60` | «El ámbito se indexa por (sesión, miembros) y se cierra con el fin del **propio comando**, no tras POST; o la pista de objeto lleva el handle … Sesión, con IC o A-n, en T2 (marcado) y T4B (ámbito)» | Marcado **IMPL** en T2 (IC-08). Ámbito **PEND** (T4B) |
| C-F4-05 | `:61` | «Cero vistas supervivientes (H-06) … No aplicado, con su causa visible en T4B y sin escrituras. Difiere de RACKEDITAR Push Back» | **PEND**. H-06 sigue cierto: `src/RackCad.Application/Views/Redraw/RackSiblingRedrawPlan.cs:92-95` da `DeferToFirstPlacement` |

Hallazgos del paquete que siguen ciertos en `4e34bb56`:
- H-02 (`:21`): la costura convierte en `Discarded` toda excepción de `Mutate`, también la de `Commit()`. Lo confirma `src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs:95-99` (`catch (Exception ex) { mutation = RackSiblingMutationResult.Discarded(null, ex.Message); }`) y `src/RackCad.Plugin/Systems/Shared/SiblingRedrawTransaction.cs:23-29` (`transaction.Commit()` dentro del `try`, y el `catch` da `Discarded`).
- H-03 (`:22`): `RackSiblingScan` abre bloqueo y transacción propios. Lo confirma `src/RackCad.Plugin/Views/RackSiblingScan.cs:112-113` y `:163`, donde además hace `transaction.Commit()` de la lectura.
- H-04 (`:23`): `tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:48-55` sigue con 4 fachadas y ninguna de Push Back.
- H-08 (`:27`) y H-15 (`:34`): ver §3.5. H-16 (`:35`): «`PushBackDesign` no tiene igualdad».

### 1.4 Obligaciones (§5), contrato de T1 (§6) y paradas (§7)

- Obligaciones que nombran T4B (`:79-86`): 11 («La comprobación relee datos authored»), 12 («Ningún cambio externo se sobrescribe sin detectarse», INV-06), 13 («La sesión A nunca escribe B», INV-22), 16 («Pistas propias no cuentan como externas», INV-08), 17 («POST viene de una relectura», INV-11) y 18 («Todo objeto y transacción muere dentro», T4A y T4B).
- §6, StaleBase (`:96-102`): «texto crudo exacto **solo de cada sobre miembro interpretable**» y «MATCH solo si todo es idéntico, sin normalización, con la lista de lo que cambió».
- §6, reconciliación (`:103`): «descartar y recargar, o conservar mi borrador sobre la versión actual tras confirmación explícita. Sin fusión automática».
- §6, Actualizar (`:104`): «una sola operación en curso … Desconocido tiene dos orígenes … Nunca se supone aplicado; POST solo deja Limpio con relectura».
- §7, cierre (`:111`): «evidencia automatizada de INV-04..08, 10..12, 16..21, 25..31 y 39».
- §7, paradas (`:117`): entre ellas «cambiar la semántica authored congelada», «detección de cambios externos no fiable», «UNDO que no sea un solo paso sin la A-n de D-13» y «decisión material nueva de arquitectura».

### 1.5 Invariantes pedidos

| INV | Proposal V5 | Literal (resumido entre comillas) | Estado en `4e34bb56` |
|---|---|---|---|
| INV-06 | `:744` | «Un borrador obsoleto no sobrescribe … diferencia en atribución, en un sobre o en el registro → No aplicado — Conflicto, cero escrituras» | **PARCIAL**. La comparación es `DraftStaleBase.CompareWith` (`src/RackCad.Application/Workspace/Drafts/DraftStaleBase.cs:141-169`); el resultado `NotAppliedConflict` (`DraftApplyOutcome.cs:69-74`) lleva a Conflicto (`DraftSession.cs:519-523`). Falta la compuerta en un camino de escritura, que no existe. La prueba «lector y escritor simulados» falta |
| INV-08 | `:746` | «Sin reentrada en el Actualizar propio; ámbito por (sesión, miembros) de PREPARE a POST» | **PEND** (sin código de ámbito; `grep` de `OwnMutation`/`MutationScope` vacío). IC-08 lo obliga para T4B |
| INV-10 | `:748` | «Desconocidos y metadatos por vista preservados … cada vista conserva los suyos» | **PEND**. `ComposeForWrite` compone un único diseño desde el sobre de origen (§3.4). El paquete de T2 dice «La metadata de origen del proyecto (`WithSourceMetadataFrom` …) también sale de la base. T4A la fija (INV-10)» (`I-64-f4-t2-gate-contract-package.md:151`) |
| INV-11 | `:749` | «Clasificación de resultados de D-13 en Core … fallo en PREPARE, antes de `Commit()`, en `Commit()`, en POST y en la relectura» | **PARCIAL**. Clasificación **IMPL** en el núcleo: `DraftApplyOutcomeKind` (`DraftApplyOutcome.cs:12-37`) y `Classify` (`DraftSession.cs:515-565`), con pruebas en `WorkspaceDraftApplyTests.cs:402-600`. Falta quien produzca esos resultados |
| INV-22 | `:760` | «Toda petición y ejecución va ligada a su sesión» | **IMPL para las lecturas**: `PassGate` (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:1034-1068`) y la autorización bajo bloqueo del lector del Plugin (`src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:232-236`). **PEND** para Actualizar |
| INV-25 | `:763` | «Toda excepción originada en el panel queda contenida; la pestaña pasa a Error con su borrador» | **PARCIAL**. `DraftSession.EnterError` existe (`DraftSession.cs:601-616`) sin ningún llamador de producto. El `EnterError` de `src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs:292` es el del panel, no el de un borrador |
| INV-28 | `:766` | «Un Actualizar del panel sobre A no deja en Conflicto ni marca el borrador de B» | **PARCIAL**. La parte de Conflicto está **IMPL**: la base excluye a los no miembros (`DraftStaleBase.cs:98-99`; pruebas `WorkspaceDraftStaleBaseTests.cs:329`, `:339` y `:349`). La parte de **marcado** está **PEND**: hoy cualquier `CommandEnded` o cambio de base de datos marca todos los borradores de la sesión (`WorkspaceHostCore.cs:172-173` y `:1015-1019`; `WorkspaceSession.cs:51-56`) |
| INV-29 | `:767` | «Quitar un fondo … no causa Conflicto» | **IMPL** en la base: cuatro valores sin Redraw/Erase (`DraftStaleBase.cs:13-19`); Erase cuenta como Mutable (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:131-134`). Pruebas: `WorkspaceDraftStaleBaseTests.cs:38` y `:359`, y `WorkspaceDraftPilotIntentStaleBaseTests.cs:154` |
| INV-30 | `:768` | «excepción antes de `Commit()` → No aplicado; en `Commit()` → Desconocido con relectura; pruebas existentes de la costura en verde» | **PEND** (H-02). El núcleo ya tiene `CommitThrew` (`DraftApplyOutcome.cs:27`) |
| INV-31 | `:769` | «Aplicado con avisos, No editable aquí y Desconocido se muestran en la UI … con «Comprobar ahora»» | **PARCIAL**. «No editable aquí» con su diagnóstico y la indicación de RACKEDITAR está **IMPL** (C-F4-07). La UI solo rotula los estados («Conflicto», «Aplicando», «Desconocido» y «Error» en `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:395-398`). «Comprobar ahora» no tiene llamador en la UI (`I-64-f4-t3c2-gate-contract-package.md:58`, H-15). Faltan los avisos |
| INV-39 | `:777` | «La relectura de MUTATE usa la misma transacción caller-owned: no abre transacción, no bloquea y no confirma» | **PEND**. El único barrido abre bloqueo y transacción propios y confirma (`RackSiblingScan.cs:112-113` y `:163`) |

### 1.6 Decisiones de la Proposal V5 que cita el paquete

- **Glosario** (`:46`): StaleBase = «Hechos crudos del authored, capturados para detectar cambios externos (D-12). No se usa para calcular dirty». Dirty (`:49`): «Nunca se calcula comparando JSON crudo (D-10)».
- **D-02** (`:165-166`): las lecturas que inicia el usuario, entre ellas «Comprobar ahora» (D-12), se hacen «en contexto de aplicación, con el documento bloqueado».
- **D-04, mutación propia** (`:241-243`): «el ámbito de mutación propia se indexa por (sesión, miembros del rack) y cubre desde PREPARE hasta la relectura de POST … Un Actualizar del rack A no marca los borradores de otros racks».
- **D-08, Error** (`:396`): «una pestaña cuya superficie falla queda en Error, conserva su borrador y ofrece Recargar o Descartar».
- **D-10**: tabla de componentes (`:416-420`); dirty (`:422-423`); «toda recarga sustituye StaleBase e InitialDraftState **a la vez**, desde una sola lectura» (`:432`); «Limpio con pista (O-01)» (`:439`).
- **D-12**:
  - autoridad (`:465-469`): «una relectura **dentro de la transacción de MUTATE** … a través de la misma transacción caller-owned … no puede usar el barrido actual»;
  - lo que se compara (`:470-477`): pertenencia de cuatro valores con `View` y `Section`, «el **texto crudo exacto** de cada sobre miembro interpretable» y el registro de variables solo en Selectivo;
  - «MATCH solo si todo es idéntico. Cualquier diferencia: **Conflicto y cero escrituras authored**» (`:479`);
  - O-04 (`:480-481`): «PREPARE repite la comparación en solo lectura antes de importar nada … No sustituye a la relectura dentro de MUTATE»;
  - «**Sin normalización** … solo pueden dar conflictos falsos, nunca aceptaciones falsas» (`:482-483`); «B (huella normalizada) rechazada por §9.14» (`:484`);
  - «**Comprobar ahora:** relectura de solo lectura que deja el borrador en Conflicto o confirma su estado» (`:486`);
  - reconciliación (`:487-493`): (1) descartar y recargar; (2) «Conservar mi borrador sobre la versión actual: tras una confirmación explícita, StaleBase e InitialDraftState se sustituyen … y el CurrentDraftState se conserva». «Sin fusión automática»;
  - H-LOCK reformulada (`:494-498`): «la importación ocurre en PREPARE, antes de la transacción».
- **D-13**:
  - «**todo o nada sobre el authored del rack**» (`:502-503`); residuo de biblioteca admitido (`:504-505`); «**La mecánica no se congela**» (`:506`);
  - paso 2 (`:510-511`): «El adaptador toma el bloqueo del documento con `using` alrededor de PREPARE, MUTATE y POST»;
  - paso 3, PREPARE (`:512-514`): «sin escrituras authored: comprobación previa de solo lectura (O-04); pertenencia y admisión (D-11); unidades por vista … con Redraw/Erase recalculado desde el CurrentDraftState; capas bloqueadas …; **importación de biblioteca, fuera de la transacción authored**; lista de piezas faltantes»;
  - paso 4, MUTATE (`:515-518`): «**una** transacción … dentro, primero la relectura caller-owned y la comparación (D-12) y la comprobación de destino … por último `Commit()`»;
  - paso 5, POST (`:519-520`): «una relectura de la que salen a la vez la StaleBase y el InitialDraftState nuevos; CurrentDraftState := InitialDraftState; Limpio»;
  - tabla por frontera (`:523-529`) y recuperación de Desconocido (`:531-532`): «si coincide con la StaleBase anterior … el borrador **recupera el estado que tenía antes del intento** (Limpio o Dirty; O3-06); si no, pasa a Conflicto. Nunca se supone aplicado»;
  - obligación de la vía (`:533-535`, INV-30); R-09 (`:540-541`): los escritores caller-owned de Push Back no tienen uso en producto; UNDO (`:542-543`): «**un Actualizar del panel es un solo paso de UNDO**»; `:545`: «No puede haber un Actualizar mientras otro está en curso».
- **D-17** (`:597-600`): «toda entrada originada en el panel queda contenida … La pestaña afectada pasa a Error y conserva su borrador».
- **Opcionales adoptadas:** O-04 (`:1102`), O-05 (`:1103`), O-11 (`:1109`), O-15 (`:1113`), O3-02 (`:1150`), O3-06 (`:1154`) y O4-05 (`:1172`).

### 1.7 Los «siete resultados» de D-13

- El literal está en `docs/initiatives/I-64-architect-package-v1.md:72`, no en la v5: «| Actualizar | PREPARE / MUTATE / POST; todo o nada; siete resultados | D-13 |».
- La tabla de V1 (`docs/initiatives/I-64-proposal-v1.md:405-415`) tiene siete filas: No aplicado — Conflicto, Admisión, Preflight y Fallo; Aplicado; Aplicado con avisos; y «Aplicado, relectura fallida».
- En V1 la importación iba **dentro** de MUTATE (`:395-402`).
- La V5 sustituye esa tabla por la clasificación por frontera (`:523-529`) y añade **Desconocido** cuando `Commit()` lanza.
- El código tiene ocho tipos (`DraftApplyOutcome.cs:12-37`), fijados por la prueba `TheOutcomeKindsAreExactlyTheEightOfTheTable` (`tests/RackCad.Tests/Workspace/WorkspaceDraftApplyTests.cs:591-597`).

### 1.8 Aclaraciones IC

- **IC-01** (`I-64-implementation-clarifications.md:17`): «**No** se aplica a Actualizar, a escrituras de datos authored … a las rutas de escritura de F4».
- **IC-06** (`:91-111`): el piloto acotado. Requisito 7: «la misma semántica de `DraftSession` tiene que poder soportar después la superficie completa de F5».
- **IC-07, punto 2** (`:123-127`): «Todo lo authored que el piloto no edita sale de la base vigente (`InitialDraftState`) … Después de «Conservar mi borrador», la capa no editable es la de la versión nueva. Un borrador Limpio compone exactamente la base».
- **IC-08** (`:131-153`): marcado grueso por sesión. Obligación vinculante para F4-T4B (`:145-151`): «Las pistas que produce la mutación propia de Actualizar no marcan los borradores … las pistas se agrupan por sesión y por tipo … se drenan después del ámbito … no puede suprimir `InvalidateNavigationCaches` … Oráculo: «tras Actualizar A, el borrador B no queda marcado» (INV-28)».

### 1.9 Decisiones posteriores que asignan trabajo a T4

| Decisión | Archivo:línea | Qué asigna | Estado |
|---|---|---|---|
| F4-E07 y F4-E16 | `f4-dispositions.md:61` y `:231` | «a T4B: `CompleteApply` en `finally` y el error diferido durante Aplicando»; «los avisos de POST visibles aunque falle su relectura, a T4B» (N-5 del análisis `F4-T1-DRAFT-CORE/2/R20261110T194129Z-0d3b/analysis.md:73-75`) | **PEND**. Con `AppliedPostRereadFailed`, `Classify` no guarda avisos (`DraftSession.cs:541-548`), y `TryBeginApply` los vacía (`:481`) |
| F4-E09 | `:106-110` | C-F4-02 ratificada; «los callbacks no leen el DWG … T2 y T4 lo preservan y lo prueban» | regla vigente |
| F4-E18 | `:274-278` | «**Decisión:** T1B pasa a ejecutarse antes de la primera tarea que modifique un archivo de esa clausura, y como muy tarde antes de T4A» | **PEND**. No existe evidencia `F4-T1B*`, y `docs/automation/state/I-64.yml:56` dice «F4-T1B y Smoke-2 pendientes» |
| F4-E24 | `:415-417` | defecto de `ComposeForWrite` corregido en T2A intento 2; menor «el rebase no es atómico» | ver §3.4 |
| F4-E25 | `:450-458` | **A T4A**, «dueña de la composición para escribir y de la entrada caller-owned»: (1) comparar `current` sin normalizar contra una copia de `initial` con la escala neutralizada; (3) comprobar que la base rematerializada coincide con `initial` antes de componer; (6) rechazar una escala no válida (NaN, 0 o negativa); (2) separar las variantes del escenario (b) de «Conservar»; (5) una aserción sin normalizar del corpus canónico en `RackCad.UI.Tests`. **A T4B:** (4) «un copiador que lanza en el rebase de Applied deja el estado calculado sobre la base anterior y un `EnterError` diferido sin aplicar» | (1), (3) y (6) **PEND** (§3.4). (2): existen `WorkspaceDraftPilotMaterializationTests.cs:568` y `:608`; hay que comprobar que cubren lo pedido. (4) **PEND** (§2.5) |
| F4-E26 | `:480` y `:483` | la proyección solo trae el texto crudo, y `BuildStaleBase` deriva `Section`; «**A T4A:** reutilizar `BuildStaleBase` en la relectura caller-owned y en la comprobación previa O-04» | **PEND**. Función lista: `PushBackPilotIntentReader.cs:64-68` |
| F4-E35 | `:871` | «la obligación de llamar a `NotePilotEdit` (también para F4-T4)» | **PEND** para Actualizar |
| `F4-T3B` | `I-64-f4-t3b-gate-contract-package.md:199` | «`TryCheckNow` no se usa en T3B1 (el flujo de obsoleto o conflicto es de T4)» | **PEND** |
| F4-E48 | `f4-dispositions.md:1535` | R-3 de T3C1 pasa a F4-T4 | §4 |
| F4-E50 | `:1671-1680` | C2-M-1..8 a F4-T4, «C2-M-1 y C2-M-2 van primero»; obligaciones de T4 | §4 |
| Testigo | `:1049` | «N-2 de F4-E33 y `ReadPreviewWitness`, a F5» | fuera de T4, ver §3.5 |

---

## 2. El núcleo de borradores (`src/RackCad.Application/Workspace/Drafts/`)

### 2.1 Archivos

Hay cinco: `DraftSession.cs` (629 líneas), `DraftStaleBase.cs` (171), `DraftApplyOutcome.cs` (147), `DraftRegistry.cs` (369) y `DraftClosePolicy.cs` (52).

La guarda `TheDraftsFolderHoldsExactlyTheFiveCoreFilesAndTheParentGuardsRecurseIntoIt` (`tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs:43-50`) impide añadir un archivo sin un cambio declarado. Las demás guardas del núcleo (`:85-575`) prohíben AutoCAD, WPF, persistencia, `async`, eventos, estado estático mutable y tipos del espacio padre salvo `SessionId`, y exigen que toda operación pública devuelva un valor que diga qué cambió (`:529`).

### 2.2 API pública

**`DraftSession.cs`**
- `DraftAdmission` (`:9-18`).
- `DraftIntentRead<TState>` (`:25-52`), con `Admitted(staleBase, initialState)` en `:44` y `NotAdmitted(diagnostic)` en `:50`.
- `DraftStatus` = Clean, Dirty, Conflict, Applying, Unknown, Error (`:58-66`). El comentario dice: «"Possibly stale" is NOT a status: it is an orthogonal mark» (`:55-56`).
- `DraftUnknownOrigin` = CommitThrew, PostReread (`:69-73`).
- `DraftBoundary` (`:76-85`); `IsBlocking` es TabChange, RackChange o Update (`:90-91`).
- `IDraftPendingTextAdapter<TState>` (`:108-115`).
- `DraftBoundaryResult` (`:130-148`), `DraftEditResult` (`:159`), `DraftReloadStatus` y `DraftReloadResult` (`:161-175`) y `DraftMarkResult` (`:177-182`).
- `DraftCheckStatus` = NotApplicable, Recovered, Conflict (`:190-200`); `DraftCheckResult(Status, DraftStatus, Differences)` (`:202`).
- **`DraftSession<TState>`** (`:224`):
  - `StaleBase` (`:266`, `private set`);
  - `InitialDraftState`, que es una **copia** (`:269`);
  - `CurrentDraftState`, **por referencia**, que se edita en sitio (`:272`);
  - `PendingText` (`:278`), `HasPendingText` (`:280`), `IsDirty` (`:283`) y `Status` (`:285`);
  - `IsPossiblyStale` (`:288`) y `NeedsLazyReload` (`:291`);
  - `IsRegistered` (`:294`), `UnknownOrigin` (`:297`), `ConflictDifferences` (`:300`), `LastApplyCause` (`:303`), `Warnings` (`:306`), `ErrorReason` (`:308`) y `DeferredErrorReason` (`:314`);
  - `SetCurrentDraftState` (`:330-335`), `SetPendingText` (`:338-345`), `RemovePendingText` (`:347-353`) y `CrossBoundary` (`:362-389`);
  - `MarkPossiblyStale` (`:393-399`), `DiscardAndReload` (`:407-422`) y `KeepMineOnCurrentVersion(read, confirmed)` (`:428-443`);
  - `TryBeginApply` (`:467-483`), `CompleteApply` (`:490-513`), `CheckNow(DraftStaleBase)` (`:571-589`), `StatusBeforeAttempt` (`:592`) y `EnterError` (`:601-616`).

**`DraftStaleBase.cs`**
- `DraftMembership` (`:13-19`).
- `DraftStaleBaseEntry(Key, Membership, View, Section, EnvelopeText)` (`:25`).
- `DraftDifferenceKind` = Membership, View, Section, EnvelopeText, VariableRegistry (`:28-35`).
- `DraftDifference` (`:38`) y `DraftStaleComparison` con `IsMatch` y `Differences` (`:41-51`).
- `DraftStaleBase.Create` (`:80-102`), `Entries` (`:113`), `VariableRegistryText` (`:116`), `MembershipOf` (`:122`), `IsAmbiguous` (`:129`) y `CompareWith` (`:141-169`).

**`DraftApplyOutcome.cs`**
- Los ocho `DraftApplyOutcomeKind` (`:12-37`) y sus fábricas (`:69-105`).
- `DraftApplyRefusal` = None, NotRegistered, InvalidStatus, PendingText, AnotherUpdateInProgress (`:108-123`).
- `DraftApplyStart` (`:126-131`) y `DraftApplyResult(Accepted, Kind, Status, Rebased, DraftWasRegistered)` (`:139-146`).

**`DraftRegistry.cs`**
- `Begin` (`:143-167`), `Find` (`:169`), `Contains` (`:177`), `RackIdsOf` (`:186`) y `RacksAtRiskOf` (`:192`).
- `HasUpdateInProgress` (`:198`), `Discard` (`:204`), `DiscardAll` (`:218`) y `RetireSession` (`:234`).
- Operaciones explícitas de T2B2B: `DiscardByUser` (`:262`), `DiscardAllByUser` (`:276`), `RetireKnownSafe` (`:292`) y `RetireConservative` (`:313`).
- La ranura única de Aplicando: `TryAcquireApplySlot` y `ReleaseApplySlot`, internas (`:356-367`). Comentario: «The slot belongs to the operation, not to the registration of the draft» (`:363`).

**`DraftClosePolicy.cs`**: `EvaluateClose` (`:44-50`).

### 2.3 Máquina de estados

| Desde | Operación | Hacia | Archivo:línea |
|---|---|---|---|
| (nace) | `DraftRegistry.Begin` con lectura admitida | Clean | `DraftRegistry.cs:150-166`; copia el inicial y el actual (`DraftSession.cs:258-259`) |
| Clean o Dirty (derivado) | edición, pendiente o frontera | Clean o Dirty | `IsDirty` = pendiente o `!_comparer.Equals(_current, _initial)` (`:283`); `CanEdit` solo con estado explícito null o Conflict (`:316-317`) |
| Clean o Dirty **sin** pendiente | `TryBeginApply` | Applying | `:467-483`; rechaza otros estados (`:471-472`), el pendiente (`:473`) y otra operación en curso en **todo** el registro (`:474`) |
| Applying | `CompleteApply(NotAppliedConflict)` | Conflict, con las diferencias | `:519-523` |
| Applying | `CompleteApply(NotAppliedAdmission, Preflight o Failure)` | estado derivado previo | `:525-529` |
| Applying | `CompleteApply(CommitThrew)` | Unknown(CommitThrew) | `:531-535` |
| Applying | `CompleteApply(Applied o AppliedWithWarnings)` con `PostRead` admitido | Clean, rebasado | `:537-557`: `Rebase` más `_current := copia` más pendiente vaciado |
| Applying | `Applied` sin `PostRead` admitido, o tipo desconocido | Unknown(PostReread) | `:541-548` y `:559-563` |
| Applying | `EnterError` | sigue Applying; razón diferida | `:606-610`; se aplica tras clasificar (`:503-512`) |
| Unknown(CommitThrew) | `CheckNow` con MATCH | estado derivado (Recovered) | `:577-584` |
| Unknown(CommitThrew) | `CheckNow` con diferencia | Conflict | `:586-588` |
| Unknown(PostReread) | `DiscardAndReload` | Clean | `:407-422` |
| Dirty o Conflict | `KeepMineOnCurrentVersion(read, true)` | Dirty o Clean derivado; conserva el actual y el pendiente | `:428-443` |
| todo salvo Applying | `DiscardAndReload` | Clean | `:410` rechaza Applying |
| no Applying | `EnterError` | Error | `:612-615`; sale solo por `DiscardAndReload` o `Discard` (`:597-599`) |
| cualquiera | `Discard` o retirada | desligado; la ranura **no** se libera | `:624-627`; `DraftRegistry.cs:363-367` |

`Rebase` (`:446-457`) es el único punto que limpia a la vez `StaleBase`, `_initial`, `IsPossiblyStale`, el estado explícito, `UnknownOrigin`, `ConflictDifferences`, `LastApplyCause`, `Warnings` y `ErrorReason`.

### 2.4 StaleBase y comparadores: crudo o semántico

- La **StaleBase compara en crudo**: «Every comparison is ordinal and exact, with no normalization» (`DraftStaleBase.cs:136-140`).
  - Recorre la unión de claves; una clave ausente cuenta como NotMember (`:145-148`).
  - Una clave ambigua nunca coincide (`:151-155`).
  - Compara `Membership`, `View`, `Section` y `EnvelopeText` con `StringComparison.Ordinal` (`:159-162`), y el registro de variables (`:165-166`).
  - Un NotMember se descarta, y un Blocking no guarda texto (`:98-99` y `:105-110`).
- **Dirty es semántico:** usa el `IEqualityComparer<TState>` inyectado (`DraftSession.cs:227` y `:283`). En el piloto es `PushBackPilotStateComparer` (§3.4).
- **Posiblemente obsoleto** es una marca ortogonal (`MarkPossiblyStale`, `:393-399`). Solo `Rebase` la limpia (`:450`). `NeedsLazyReload` = registrado, marcado y Clean (`:291`).

### 2.5 La operación de aplicar (el «lock» de F4-T1)

- Hay una ranura **por registro** (`DraftRegistry._applying`, `:130`). La toma `TryBeginApply` (`DraftSession.cs:474-476`) y solo la libera `CompleteApply` (`:496-497`).
- `EnterError`, `Discard` y la retirada no la liberan. Lo prueba `TheApplyingSlotIsNotFreedByErrorDiscardDiscardAllOrRetireWhileTheOperationIsInFlight` (`WorkspaceDraftApplyTests.cs:128`).
- **Riesgo F4-E25 (4), pendiente (T4B).** En `CompleteApply`, la ranura se libera y el estado explícito pasa a null (`:496-501`) **antes** de `Classify`. Si el copiador lanza en `:550-551`, la excepción escapa con la ranura liberada, el estado derivado sobre la base anterior y la razón diferida sin aplicar.
- **Riesgo DR2-03, pendiente (T4B).** Si el host no llama a `CompleteApply` en `finally`, la ranura queda tomada para todo el proceso (`F4-T1-DRAFT-CORE/2/.../analysis.md:27`).
- `CompleteApply` no tiene llamador de producto. Tampoco `TryBeginApply`, `KeepMineOnCurrentVersion`, `ComposeForWrite`, `KeepPilotEditsOn` ni `DraftSession.EnterError` (`grep` en `src`).

### 2.6 «Comprobar ahora» en el núcleo

`CheckNow(DraftStaleBase reread)` (`:571-589`) solo actúa en Unknown con origen CommitThrew. En los demás casos responde NotApplicable (`:574-575`); lo fija `CheckNowOnlyAppliesToUnknownWithOriginCommitThrew` (`WorkspaceDraftApplyTests.cs:660`). **No existe** una comprobación general para Clean, Dirty o un borrador marcado («deja el borrador en Conflicto o confirma su estado», D-12 `:486`).

### 2.7 Lo que el núcleo ya da a T4 y lo que falta

- **Ya da:**
  - la base cruda inmutable y su comparación con diferencias ordenadas;
  - los ocho resultados y su clasificación;
  - la ranura única, que solo libera `CompleteApply`;
  - la razón de Error diferida;
  - la recuperación de Unknown;
  - `DiscardAndReload` y `KeepMine` atómicos frente a un copiador que lanza (`:415-421` y `:439-442`);
  - el rebase de POST desde la relectura;
  - la frontera Update de dos fases (`CrossBoundary(DraftBoundary.Update)`).
- **Falta:**
  - una comprobación general («Comprobar ahora» para Clean o Dirty);
  - una forma de clasificar Conflicto desde PREPARE sin pasar por `TryBeginApply`/`CompleteApply`, si T4A no toma la ranura;
  - avisos con la relectura de POST fallida (N-5);
  - `CompleteApply` robusto ante un copiador que lanza (F4-E25 (4)).

---

## 3. Estado del piloto y del host

### 3.1 `PushBackPilotDraftState` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs:13-33`)

Agrupa una `PushBackCompositeEditorState` (`:23`), unos `PushBackEditorInputs` (`:26`) y `Origin`, el texto crudo del sobre (`:32`): «In the initial state of a draft it is always the CURRENT base, because every read that rebases the draft replaces it» (`:29-31`).

### 3.2 `PushBackPilotFieldAdapter` (`PushBackPilotFieldAdapter.cs:21-72`)

- Un campo, `AnnotationScaleKey = "AnnotationScale"` (`:24`).
- `Validate` (`:35-53`) exige un número > 0 en la cultura actual.
- `Apply` (`:55-71`) edita en sitio `current.Inputs.Annotations.AnnotationScale` y devuelve el mismo objeto.

### 3.3 La lectura de la intención y lo que guarda la StaleBase

- Entrada: `PushBackPilotIntentScan` (`PushBackPilotIntentScan.cs:24-99`), con los hechos sin clasificar, el texto crudo por clave (`:91-92`), `CompletenessEvidence` y el testigo opcional (`:14` y `:98`).
- `PushBackPilotIntentReader.BuildStaleBase` (`PushBackPilotIntentReader.cs:64-68`) y `Read` (`:75-109`). `Analyze` (`:113-146`) clasifica con `RackSiblingMembership.Classify(scan.Facts, scan.RackId, null, false)` (`:119`). La correspondencia es:
  - Redraw y Erase dan Mutable (`:131-134`);
  - ReadOnly da ReadOnly (`:135-137`);
  - BlockingUnreadable da Blocking, sin texto (`:138-140`);
  - NotMember no entra en la base.
- **Contenido de cada entrada** (`Describe`, `:148-153`):
  - `Key` = handle de la definición;
  - `View` = la del hecho del barrido;
  - `Section` = `embed.Section` del sobre deserializado, en cultura invariante;
  - `EnvelopeText` = **el JSON crudo del sobre, tal cual lo leyó `RackBlockData.Read`**;
  - sin registro de variables (Push Back).
- **Conclusión: la StaleBase guarda JSON crudo más tres hechos derivados** (atribución, `View` y `Section`). No guarda hechos authored semánticos del diseño.
- La admisión son 14 reglas (`:157-233`). El estado inicial se materializa desde el **mismo** texto de la entrada de origen (`:225`).
- `PushBackPilotIntentResult` (`PushBackPilotIntentResult.cs:25-60`) lleva siempre la base, también sin admisión (`:43-44`), y es la misma instancia que lleva la lectura admitida (prueba `WorkspaceDraftPilotIntentStaleBaseTests.cs:300`).
- **Testigo de la preview:** `WitnessStatus` (`PushBackPilotIntentReader.cs:240-253`) compara la clave y el texto crudo del origen. En el producto el testigo no se usa:
  - la UI manda `null` (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:202`: `new EditIntentRequest(session, rack, null, null)`);
  - `ReadPreviewWitness` (`src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:149-189`) no tiene llamador;
  - está diferido a F5 (`f4-dispositions.md:1049`).

### 3.4 Composición para escribir: `ComposeForWrite` y su entorno

**`PushBackPilotMaterializer.Materialize(originText, catalog)`** (`PushBackPilotMaterializer.cs:122-177`) usa la ruta de RACKEDITAR: `RackEmbedStore`, la comprobación de kind, `RackProjectStore` y la parte de Application de `LoadExisting`.

**`ComposeForWrite(initial, current, catalog)`** (`:189-215`) compone en tres pasos:
1. Rechazo de lo que el piloto no edita (`:198-201`): `neutral = Copy(current)`, `neutral.scale := initial.scale` y `Equals(initial, neutral)`; si difieren, `NonPilotContentReason` (`:112`).
2. Rematerialización desde `initial.Origin`, la base vigente (`:203-205`).
3. Copia de **solo** `AnnotationScale` de `current` (`:207-208`).

Devuelve `PushBackPilotComposition` (`:60-88`), con `Editor` e `Inputs` **mutables**, que consume `PushBackCompositeEditorAssembler.BuildDesign` (`src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorAssembler.cs:38`).

**Arrastres de F4-E25 que siguen pendientes en este código:**
- (1) el rechazo pasa `current` por `Copy` (`:198`), no compara `current` sin normalizar;
- (3) no comprueba que `fresh` sea igual a `initial` antes de componer (`:203-205`);
- (6) no rechaza NaN, 0 ni valores negativos (`:207-208`).

**Lo que compone:** solo el diseño del **sobre de origen**. No hay unidades por vista, ni preservación por vista de `ExtensionData` (INV-10), ni `WithSourceMetadataFrom` (`src/RackCad.Application/Persistence/RackProject.cs:75`; la ruta clásica lo hace en `src/RackCad.Plugin/RackPushBackCommands.cs:456`).

**Garantía con la base autoritativa.** El paquete de T2 dice: «La relectura de MUTATE se usa solo para comparar con la StaleBase (D-13). Si no hubo cambios, la base y la relectura coinciden» (`I-64-f4-t2-gate-contract-package.md:152`) y «T4 nunca arma un diseño directamente sobre un estado del borrador» (`:153`). Con MATCH crudo, `initial.Origin` es byte a byte el texto del origen de la relectura. Así, lo no editado sale de la base autoritativa y no de copias viejas del borrador.

**`KeepPilotEditsOn(newInitial, current)`** (`:223-232`) da la base nueva con la escala del borrador; la superficie debe fijarla con `SetCurrentDraftState`. Son dos pasos, no atómicos (F4-E24 `:417`). Sin ellos, `ComposeForWrite` rechaza (prueba `WorkspaceDraftPilotMaterializationTests.cs:608`).

**Comparador y copiador** (`PushBackPilotStateComparer.cs`):
- `Equals` compara editor e inputs (`:30-35`);
- `Copy` (`:58-62`) «does NOT copy the module and tail authority» (`:54-56`).

### 3.5 El host: `WorkspaceHostCore` (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs`)

| Operación | Línea | Qué hace | Para T4 |
|---|---|---|---|
| `TryBeginPilotEdit` | `:280-358` | Pasos 1 a 8: RackId, testigo, `PassGate` (`:302-305`), `AlreadyOpen` (`:308`), **una** lectura (`:311`) y `Begin` con el comparador y el copiador del piloto (`:330-335`) | La StaleBase solo entra por `pilot.Read` |
| `TryCheckPilotEdit` | `:365-428` | **Solo** Unknown(CommitThrew) (`:388-389`); una relectura (`:392`) y `draft.CheckNow(pilot.StaleBase)` (`:410`) | No sirve a Clean, Dirty ni a un borrador marcado |
| `TryReloadPilotEdit` | `:438-512` | Solo con `NeedsLazyReload` (`:467`); una lectura y `DiscardAndReload(pilot.Read)` (`:489`) | Recarga perezosa O-01 |
| `DiscardPilotEdits` | `:520-536` | `DiscardAllByUser`, solo con `Authorize` | — |
| `NotePilotEdit` | `:543` | quita la marca de «limpio conocido» | Actualizar debe llamarla |
| Puerto explícito `IWorkspacePilotEditPort` | `:553-843` | 12 operaciones; la marca `_pilotReadInProgress` envuelve las tres lecturas (`:553-593`) | Ninguna operación es Actualizar, «Conservar» ni PREPARE |
| `ViewOfPilotDraft` | `:861-911` | el estado (`:913-924`); `errorReason` solo en Error (`:908-909`) | R-3 (§4) |
| Marcado en el drenaje | `:172-173` y `:1015-1019` | `MarkDraftsPossiblyStale` marca **todos** los borradores del piloto de la sesión | IC-08 e INV-28 |
| `PassGate` | `:1034-1068` | lector, documento activo, modal, reposo y `Authorize` | Patrón para PREPARE |
| `DescribeDifferences` | `:1070-1071` | `clave:Tipo` | Texto del Conflicto |

- **Puerto:** `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:26-83`. `PilotFieldState` va de NotOpen a Undecidable (`:86-107`), y `PilotFieldView.ErrorReason` está en `:213-217`. Las 12 operaciones son «a lower bound: a later task may add its own to this port» (`tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:95`).
- **Lector del Plugin:** `PilotIntentReader` (`src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:196-260`), en este orden:
  1. comando y reposo (`:225`);
  2. catálogo fuera del bloqueo (`:228`);
  3. `using (document.LockDocument())` (`:232`);
  4. `_authorize` (`:234`);
  5. `NavigationTargets.Revalidate` y `ChooseOriginDefinition`, solo para el **origen**, nunca para la pertenencia (`:239-240`);
  6. `RackPilotIntentAdapter.Read` (`:242`).
- **`RackPilotIntentAdapter.Read`** (`src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:94-142`): `RackSiblingScan.CaptureDrawing` (`:116`) y la lectura pura (`:120-121`); `Debug.Assert(TopTransaction == null)` antes y después (`:104`, `:140` y `:195-201`).
- **`RackSiblingScan.CaptureDrawing`** (`src/RackCad.Plugin/Views/RackSiblingScan.cs:95-96`) toma **su propio** bloqueo (anidado dentro del del lector) y su propia transacción (`:112-113`), y la **confirma** (`:163`). Vale para la comprobación previa O-04 y para «Comprobar ahora» en contexto de aplicación. **No vale** dentro de MUTATE (INV-39).

### 3.6 Costura de I-55 (base de T4B)

- `ISiblingRedrawPort` (`src/RackCad.Application/Views/Redraw/ISiblingRedrawPort.cs`).
- `RackSiblingRedrawRun.Execute`: Prepare con su `catch`, que da `PrepareFailed` (`RackSiblingRedrawRun.cs:59-63`); capas bloqueadas (`:75`); `Mutate` con `catch` que da `Discarded` (`:95-99`); `Post` con `catch` que da `Committed` más `POST_FAILED` (`:112-117`).
- `RackSiblingRedrawPlan` (`RackSiblingRedrawPlan.cs:25-95`) y `SiblingRedrawTransaction.Mutate` y `ApplyInTransaction` (`SiblingRedrawTransaction.cs:11-52`).
- `MutationDestinationBinding` (`src/RackCad.Application/ProjectVariables/MutationDestinationBinding.cs`) y `SiblingRedrawDebugFaultInjection` (`src/RackCad.Plugin/Systems/Shared/SiblingRedrawDebugFaultInjection.cs`).
- Push Back no pasa por esta costura (H-01).

---

## 4. Arrastres que T4 debe cerrar

### 4.1 R-3 de F4-T3C1: «`ErrorReason` solo en Error»

- **Origen:**
  - `docs/automation/evidence/I-64-pilot/F4-T3C1-HOST-HARDENING/task-close-package.md:88`: «**→ F4-T4:** R-3, porque «`ErrorReason` solo en Error» del host no está discriminado. Es latente: F4-T4 es la que pone borradores en Error»;
  - la clasificación en `.../1/R20261113T181650Z-979b/analysis.md:24`: «Si T3C2 toca el host, guarda por cuerpo»;
  - el detalle en `.../independent-review.md:37-46`.
- **Código:** `WorkspaceHostCore.cs:908-909`: «// The reason of the Error travels only in the state Error, exactly as the draft holds it.» y `var errorReason = state == PilotFieldState.Error ? draft.ErrorReason : null;`.
- **Prueba:** `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs:1643-1663` (`TheReasonOfTheErrorOfADraftCrossesThePortOnlyInTheStateError`). Pasa el mutante sin la condición. El VM sí filtra y lo prueba `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1378`.
- **Guarda:** ninguna mira la condición.
- **Por qué no se discrimina por comportamiento:** con la semántica actual, `ErrorReason` distinto de null implica Error.
  - Solo lo ponen `EnterError` (`DraftSession.cs:612-613`) y la razón diferida (`:509-510`), las dos junto con Error.
  - `Rebase` lo limpia (`:456`).
  - Desde Error no hay `TryBeginApply` (`:471-472`), ni `KeepMine` (`:432`), ni `CheckNow` (`:574`).
- **Cierre posible:** una guarda por cuerpo sobre `:909` como cambio declarado. La autorización del Owner pide «Make this a declared T4 contract change». También sirve una prueba con un estado al que T4 permita llegar con `ErrorReason` y otro estado, si T4 cambia la semántica de Error.

### 4.2 Menores C2-M-1 a C2-M-8 de F4-T3C2

**Origen:** `docs/automation/evidence/I-64-pilot/F4-T3C2-COMMAND-ADVISOR/task-close-package.md:76-86`; detalle en `.../1/R20261113T215110Z-28eb/independent-review.md:18-117` y clasificación en `.../analysis.md:18-27`. Las líneas se midieron en `7297e1f1`.

| Menor | Código de producto (correcto hoy) | Guarda o prueba afectada | Qué falta |
|---|---|---|---|
| **C2-M-1** (primero) | `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:69`: `return answer.Status == PromptStatus.OK && string.Equals(answer.StringResult, "Si", System.StringComparison.Ordinal);` | `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorGuardTests.cs:133-148` (`KeywordsViolations`; las agujas de `:139-145` comprueban `PromptStatus\.OK\b` y `.StringResult` por separado); autopruebas `:156-164` | fijar la expresión del `return` y añadir autopruebas con `"No"` y con `\|\|` |
| **C2-M-2** (primero) | `WorkspacePanelCommand.cs:44-47`: `session => controller.Port.Authorize(new WorkspaceRequest(session)) == RequestDecision.Accepted` | `WorkspaceCommandAdvisorGuardTests.cs:101-111`; la línea `:106` solo exige `\.Authorize\s*\(`; autopruebas `:119-130` | fijar `== RequestDecision.Accepted` y los dos primeros argumentos de `Run`, con su autoprueba en negativo |
| C2-M-3 | `src/RackCad.Application/Workspace/WorkspaceCloseAdvisorRelay.cs:134` (`RememberLoss`) | sin autoprueba en negativo: `:199`, `:234`, `:348`, `:361`, `:444`, `:461` y `:480`. `:469-474` mira el cuerpo de `SessionDestroyed` en lugar de `RememberLoss` | autopruebas y el cuerpo correcto |
| C2-M-4 | `IWorkspaceLifecycleAdvisor` | `WorkspaceCommandAdvisorGuardTests.cs:384-392`: `:391` es `Src.Count(body, @";") != 2`; frente a `WorkspaceLifecycleGuardTests.cs:84-85` | cambio declarado previsible si T4 añade un miembro; acotar el nombre a F4-T3C2 |
| C2-M-5 | — | nombres: `WorkspaceLifecycleGuardTests.cs:295`, `WorkspacePaletteHostSourceGuardTests.cs:321` y `WorkspaceHostSourceGuardTests.cs:50`. Comentarios: `WorkspaceBridgeSourceGuardTests.cs:50-52`, `WorkspaceSourceGuardExceptionTests.cs:10-11` y `:279` | renombrar o documentar de forma declarada (T4 o F7) |
| C2-M-6 | `WorkspacePanelCommand.cs:18` y `:73`: «the one writer of the command line of the panel»; `RackPanel` escribe en `:29` | — | matizar la frase |
| C2-M-7 | `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:356`: «When the risk is unknown (or its read throws)… the notice says that it could not be decided» | — | precisar: cuando la lectura lanza, el aviso es el de `FailGesture` |
| C2-M-8 | — | `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs:372-392` (`AnythingButAYesDiscardsNothing`, tres casos idénticos) | un `[Fact]`; Intro y Esc, a Smoke-2 |

### 4.3 Otros arrastres a T4 (§1.9)

- `CompleteApply` en `finally` y la razón diferida (F4-E07 y F4-E16).
- N-5: los avisos de POST cuando falla la relectura.
- F4-E25: (1), (2), (3), (5) y (6) a T4A, y (4) a T4B.
- `BuildStaleBase` en O-04 y en la relectura caller-owned (F4-E26).
- `NotePilotEdit` (F4-E35).
- IC-08 (T4B).
- **T1B, antes de T4A** (F4-E18).

---

## 5. Pruebas que ya fijan la semántica

**Núcleo** (`tests/RackCad.Tests/Workspace/`)
- `WorkspaceDraftApplyTests.cs`:
  - inicio desde Clean o Dirty sin pendiente (`:29` y `:42`), nunca desde otro estado (`:62`);
  - una sola operación en todo el registro (`:86`);
  - la ranura y su liberación (`:108`, `:128`, `:159` y `:204`);
  - el error diferido (`:219`, `:236`, `:261` y `:286`);
  - las listas copiadas al recibirse (`:305-334`) y los avisos (`:349`);
  - Conflicto con las diferencias (`:402`); los demás No aplicado (`:430`); CommitThrew (`:461`);
  - Applied y su rebase (`:484`); con avisos (`:512`); con la relectura de POST fallida (`:544`); nunca Limpio sin `PostRead` (`:565`);
  - los ocho tipos (`:591`);
  - CheckNow (`:614`, `:635`, `:660`, `:684`, `:702` y `:719`);
  - el siguiente Actualizar compara contra la base nueva (`:731`).
- `WorkspaceDraftStaleBaseTests.cs`:
  - cuatro valores (`:38`);
  - la unión de claves (`:285`);
  - otro rack no es cambio (`:329`, `:339` y `:349`);
  - quitar un fondo (`:359`);
  - sin recuento de referencias (`:386`);
  - comparación ordinal y exacta, sin normalización (`:474`);
  - ninguna operación pública construye estado ni datos de escritura (`:562`);
  - la base es inmutable (`:585`).
- `WorkspaceDraftSessionTests.cs`:
  - dirty por comparador y no por JSON (`:283` y `:360`);
  - fronteras (`:428-595`); la frontera Update (`:577` y `:595`);
  - marca (`:612-690`);
  - `DiscardAndReload` (`:712-773`) y `KeepMine` (`:798-875`);
  - Error (`:913-968`);
  - anti-alias tras Applied (`:1043` y `:1079`).
- `WorkspaceDraftRegistryTests.cs` y `WorkspaceDraftRegistryContainmentTests.cs`: registro, retiradas y descarte.

**Piloto**
- `WorkspaceDraftPilotIntentStaleBaseTests.cs`:
  - la tabla de pertenencia (`:102`) y Erase como Mutable (`:154`);
  - texto exacto con sus finales de línea (`:169`);
  - la misma base en el resultado y en la lectura (`:284` y `:300`);
  - Unknown que va a Conflicto con relecturas NotAdmitted (`:552` y `:571`) y que se recupera (`:589` y `:605`).
- `WorkspaceDraftPilotMaterializationTests.cs`: `ComposeForWrite` (`:258-535`) y los escenarios de «Conservar» (`:568` y `:608`).
- `WorkspaceDraftPilotStateTests.cs`: el comparador y el copiador (`:876-1141`), y el adaptador (`:720-822`).
- `WorkspaceDraftPilotOwnershipTests.cs`: anti-alias de `KeepMine` y de Applied (`:144`, `:159`, `:255` y `:263`) y un copiador que lanza en la recarga (`:425`).
- UI: `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`, con `ComposeForWrite` en `:88` y `:304` y el escenario de `KeepMine` contra la ventana en `:271`.

**Host**
- `WorkspaceEditIntentHostTests.cs`: CheckNow a través del host (`:731-905`).
- `WorkspacePilotEditHostTests.cs`: comparador que lanza (`:628`) y `ErrorReason` (`:1644`).

**Guardas que T4 tendrá que tocar de forma declarada**
- `WorkspaceEditIntentGuardTests.cs:632-662` (obligación 10): en todo `WorkspaceHostCore` solo se admiten `.StaleBase`, `.InitialState`, `pilot.Read` o `.Entries` como argumento de `Begin`, `DiscardAndReload` o `CheckNow` (`:635` y `:638`). Una comparación de PREPARE o un «Conservar» en el host la rompen.
- `WorkspaceEditIntentGuardTests.cs:776-786`.
- `WorkspaceDraftBoundaryTests.cs:43`: exactamente cinco archivos.
- `WorkspaceNavigationHostTests.cs:434-436`: sin `async`, `await` ni `ExecuteInCommandContextAsync`.
- `CallerOwnedFacadeGuardTests.cs:48-55`.

**Líneas base de la CI de `4e34bb56`** (corrida `43510803096`, push 4/4; `F4-T3C2-COMMAND-ADVISOR/task-close-package.md:36-37`, `f4-dispositions.md:1644-1645`):
- filtro `FullyQualifiedName~RackCad.Tests.Workspace` (Core): **2668/2668**;
- filtro `FullyQualifiedName~RackCad.UI.Tests.Workspace` (UI): **458/458**. Los filtros están en `F4-T3C2-COMMAND-ADVISOR/coordinator/gate-contract.json:326` y `:332`;
- suite completa de Core: 18206/18206. UI: 2095 pasadas, 0 fallidas y 17 condicionales sin ejecutar.

---

## Lo que ya existe para T4A

- **StaleBase cruda e inmutable** y `CompareWith` sobre la unión de claves, con diferencias tipadas y ordenadas (`DraftStaleBase.cs:80-169`).
- `PushBackPilotIntentReader.BuildStaleBase` y `Read`: una lectura que da la base, la admisión en 14 reglas y el estado inicial (`PushBackPilotIntentReader.cs:64-109`). F4-E26 manda reutilizarla en O-04.
- Lectura en contexto de aplicación con bloqueo, autorización, revalidación del origen y aserción de `TopTransaction` (`WorkspaceNavigationHost.cs:225-248`; `RackPilotIntentAdapter.cs:94-142`). Sirve para O-04 y para «Comprobar ahora».
- Compuerta de host `PassGate` (`WorkspaceHostCore.cs:1034-1068`) y patrón de operación del usuario contenida (`:365-428`).
- «Comprobar ahora» **solo** para Unknown(CommitThrew), en el núcleo (`DraftSession.cs:571-589`) y en el host (`WorkspaceHostCore.cs:365-428`).
- **Política de reconciliación congelada e implementada en el núcleo:**
  - `DiscardAndReload` (`DraftSession.cs:407-422`);
  - `KeepMineOnCurrentVersion` con confirmación (`:428-443`);
  - la recarga perezosa (`:291`; `WorkspaceHostCore.cs:438-512`);
  - la recuperación O3-06 (`DraftSession.cs:577-584`).
- **Composición para escribir:** `ComposeForWrite`, con la capa no editable desde la base y el rechazo tipado (`PushBackPilotMaterializer.cs:189-215`), y `KeepPilotEditsOn` (`:223-232`).
- Ranura Aplicando (`DraftSession.cs:467-483`) y clasificación tipada de No aplicado (Admisión, Preflight, Fallo y Conflicto).
- Separación de componentes ya hecha:
  - `StaleBase` con `private set`, solo por `Rebase` (`:266` y `:446-457`);
  - `InitialDraftState` como copia (`:269`);
  - `CurrentDraftState` por referencia (`:272`);
  - la relectura viaja como `PushBackPilotIntentResult` y no toca el borrador salvo por `Begin`, `CheckNow` o `DiscardAndReload`, que fija la guarda (`WorkspaceEditIntentGuardTests.cs:632-662`).

## Lo que ya existe para T4B

- La máquina de estados de aplicar: `TryBeginApply`, `CompleteApply` y `Classify`, con los ocho resultados, el rebase de POST solo con relectura admitida, Unknown con sus dos orígenes y la razón de Error diferida (`DraftSession.cs:467-565`).
- La ranura única por registro, que no liberan `Discard`, `Error` ni la retirada (`DraftRegistry.cs:130` y `:356-367`).
- La costura de I-55: `RackSiblingRedrawRun`, `RackSiblingRedrawPlan`, `SiblingRedrawTransaction.ApplyInTransaction`, `MutationDestinationBinding` y la inyección Debug de I-55. INV-30 queda pendiente.
- `RackSiblingMembership.Classify` como autoridad de pertenencia, ya usada por la base (`PushBackPilotIntentReader.cs:119`).
- El marcado grueso (IC-08) y `NotePilotEdit` (`WorkspaceHostCore.cs:543`).
- C-F4-02 ratificada (`f4-dispositions.md:106-109`).
- `WithSourceMetadataFrom` (`RackProject.cs:75`) y el `BuildDesign` del ensamblador compuesto (`PushBackCompositeEditorAssembler.cs:38`).

## Huecos

1. **Ningún camino de producto llama** a `TryBeginApply`, `CompleteApply`, `KeepMineOnCurrentVersion`, `ComposeForWrite`, `KeepPilotEditsOn` ni `DraftSession.EnterError`.
2. **«Comprobar ahora» general** (Clean, Dirty o marcado → MATCH o Conflicto) no existe ni en el núcleo ni en el host. D-12 lo pide (`:486`).
3. **PREPARE no existe:** ni la comprobación previa O-04, ni la admisión repetida, ni el plan, ni las capas bloqueadas, ni la lista de faltantes.
4. **No hay plan de mutación inmutable.** `PushBackPilotComposition` es mutable (`PushBackPilotMaterializer.cs:60-88`), `PushBackDesign` no tiene igualdad (H-16), y no hay unidades por vista ni preservación de `ExtensionData` por vista (INV-10).
5. **Arrastres de `ComposeForWrite` sin hacer:** F4-E25 (1), (3) y (6) (§3.4).
6. **«Conservar mi borrador» no tiene operación de host** que haga en un paso `KeepMine` más `KeepPilotEditsOn` más `SetCurrentDraftState`.
7. **Puerto y UI:** el puerto (12 operaciones) no tiene Actualizar, PREPARE, «Comprobar ahora» general ni reconciliación. La UI no tiene botones de Actualizar ni de reconciliación, y `TryCheckNow` no tiene llamador. Faltan los avisos de INV-31.
8. **INV-39:** no hay variante caller-owned de la lectura, porque `CaptureDrawing` abre su propio bloqueo y su propia transacción y confirma. INV-30 tampoco está: la costura da `Discarded` cuando lanza `Commit()`.
9. **INV-08 e IC-08:** sin ámbito de mutación propia. Las pistas no llevan rack y se agrupan por tipo y por sesión (`WorkspaceSession.cs:24-25` y `:46-57`).
10. `CompleteApply` robusto ante un copiador que lanza, y en `finally` (F4-E25 (4) y DR2-03); avisos con la relectura de POST fallida (N-5).
11. R-3 y C2-M-1..8 (§4).
12. **T1B no está ejecutada** (`I-64.yml:56`) y F4-E18 la exige «como muy tarde antes de T4A».

## Riesgos o decisiones materiales

1. **Comparación semántica o cruda.**
   - **El Freeze fija la comparación cruda exacta para detectar cambios externos:** D-12 `:474`, `:479` y `:482-484`; el paquete `:99-102`; la prueba `WorkspaceDraftStaleBaseTests.cs:474`.
   - La comparación semántica solo rige para Dirty (D-10 `:422-423`, INV-26).
   - La StaleBase actual guarda JSON crudo con atribución, `View` y `Section`, así que **no permite** una comparación semántica de hechos authored del diseño.
   - La frase del Owner («not raw serialized formatting where the Freeze says semantic comparison applies») no se activa para el Conflicto, porque el Freeze no pide semántica ahí. Normalizar la comparación cambiaría la semántica congelada (parada de `:117`; la alternativa B se rechazó en `:484`).
   - Consecuencia aceptada por el Freeze: **conflictos falsos** posibles, por ejemplo si algo reserializa un sobre sin cambiar su significado. Conviene registrar esta lectura en el paquete y medir la frecuencia en Smoke-2.
2. **La división del Owner frente al paquete de F4.** No contradice la semántica congelada, porque la descomposición no es Freeze y «La mecánica no se congela» (`:506`). Pero obliga a reasignar y declarar:
   - (a) D-13 paso 2 exige **un único bloqueo alrededor de PREPARE, MUTATE y POST** (`:510-511`; paquete `:48`). El PREPARE de T4A tiene que ser una función que T4B ejecute dentro de su bloqueo y en contexto de comando. «Comprobar ahora» es otra operación, en contexto de aplicación (`:165-166`).
   - (b) La **autoridad** es la relectura **dentro de MUTATE** (`:465-469`), y O-04 «No sustituye» a esa relectura (`:481`). La «authoritative reread before update» de T4A solo puede ser la comprobación previa. La variante caller-owned de lectura (INV-39) es de solo lectura y podría quedar en T4A con su guarda, pero su uso dentro de la transacción es de T4B.
   - (c) **La importación de biblioteca está en PREPARE** (`:512-514` y `:494`) y es una escritura en el DWG, el residuo OM-5. Con «PREPARE cero escrituras», pasa a T4B, antes de la transacción authored. Hay que declararlo.
   - (d) «Validation immediately before mutation» es la revalidación de sesión y de destino **dentro de MUTATE** (paquete `:48`; `:515-517`). T4A solo puede dar el validador puro.
   - (e) INV-30, INV-39, INV-10, C-F4-05, las guardas de fachadas (H-04) y la inyección Debug en `Commit()` estaban en T4A (`:47`) y hay que reasignarlos a T4B o a T4C.
3. **La ranura Aplicando durante PREPARE.**
   - Si el PREPARE de Actualizar no toma `TryBeginApply`, el borrador sigue editable (`CanEdit`, `DraftSession.cs:316-317`) entre PREPARE y MUTATE, y `KeepMine` o `DiscardAndReload` pueden cambiar la base que el plan comparó.
   - Si la toma, toda salida de PREPARE debe cerrar con `CompleteApply` y un No aplicado. Hoy hacer `CompleteApply` en `finally` es obligación de T4B (DR2-03).
   - Además, clasificar Conflicto desde PREPARE solo es posible con `CompleteApply(NotAppliedConflict)` o con una API nueva del núcleo.
   - Hay que decidirlo en el paquete. Una API nueva del núcleo es aditiva y sigue el precedente D-T2B2-10 (pruebas en archivos nuevos y F4-T1 sin cambios), pero toca la guarda de los cinco archivos si añade archivo.
4. **«Comprobar ahora» general.**
   - Exige una operación nueva del núcleo y del host, y un cambio declarado de la guarda de la obligación 10 (`WorkspaceEditIntentGuardTests.cs:632-662`).
   - No está especificado si un MATCH limpia la marca de «posiblemente obsoleto»: hoy solo la limpia `Rebase`.
5. **Ámbito de mutación propia frente a «Other racks must not be masked».**
   - Con pistas sin rack y agrupadas por sesión (`WorkspaceSession.cs:24-25` y `:51-56`; H-08), suprimir las pistas del Actualizar propio durante su ventana también ocultaría un cambio externo de otro rack en esa misma ventana.
   - C-F4-04 deja dos vías (`:60`): cerrar el ámbito con el fin del **propio comando**, o que la pista lleve el handle. Esto concreta D-04 y requiere una IC o una A-n.
   - Es una decisión de diseño de T4B, pero condiciona INV-28 e IC-08.
6. **T1B antes de T4A** (F4-E18 `:277`). Hay que ejecutarla o volver a disponerla de forma registrada antes de emitir T4A. La disposición original es de la sesión y responde a B1 de la revisión de la v1 (`:274-278`).
7. **La costura de I-55 para el piloto.** Push Back no la usa (H-01) y sus escritores caller-owned no tienen uso en producto (R-09, `:540-541`). La primera prueba física es Smoke-2. INV-30 exige un cambio aditivo sin alterar Insertar (`RackSiblingRedrawRun.cs:95-99`).
8. **UNDO como un solo paso** (O-11, `:542-543`): si no se cumple, hace falta una A-n material hacia un comando registrado (C-F4-02 `:59`). Solo se puede probar en el host.
9. **INV-27 y el testigo:** el testigo está diferido a F5 (`f4-dispositions.md:1049`) y la UI manda `null`, pero INV-27 sigue en la lista de cierre de F4 (paquete `:111`). Es un riesgo para el cierre de F4, no para T4A.
