# I-64 — Paquete del contrato de F4-T4-HOST-C1 (corrección de Smoke-2: el nombre del rack y la contabilidad asociativa)

**Estado:** v2. La v1 tuvo una revisión independiente: EMITIBLE TRAS CORRECCIONES, 0 B / 4 I / 14 m (`F4-DRAFTS/c1/independent-review-v1.md`). Todo se incorporó: §10.

**Producto base:** `b88dabf2`, el GREEN de F4-T4C2-PANEL. La rama `architecture/workspace-persistente-rackcad` está en `d4a24790` (documental encima; `src/` y `tests/` son idénticos a `b88dabf2`).

**Qué corrige:** dos defectos que Smoke-2 encontró en AutoCAD con un dibujo real del Owner, diagnosticados después en AutoCAD con una build de diagnóstico desechable.
- **F4-E72:** el «Actualizar» del piloto redibuja las vistas sin el rótulo del nombre del rack.
- **F4-S2-01:** cada lectura del panel, y cada fin de orden, marca el borrador como «posiblemente obsoleto». De ahí salen F4-S2-02 (borrar no inicia la edición) y el «Limpio (posiblemente obsoleto)» tras «Actualizar».

**Qué no corrige:**
- **F4-S2-03** (la vista no se refresca tras `U` hasta `REGEN`): va a decisión del Owner (D-C1-04).
- **El rendimiento del «Actualizar»** (D-C1-05): anotado al Owner.
- **RACKPROYECTAR:** en una iniciativa aparte (F4-E73).

---

## 1. Autoridades

**Órdenes del Owner** (literales, custodiadas con esta emisión en `F4-DRAFTS/coordinator/`):
- `owner-f4-smoke2-fix-order.md`: «arregla todos los errores que encontramos y después continuamos con el SMOKE, arregla todos los errores posibles ya que no estaré en mi casa». Sustituye el «Smoke-2 primero» de F4-E73;
- `owner-f4e72-decision.md`: «cuando se vayan a corregir los probelams del smoke se corrige el defecto del nombre que afectaría I-64», que autoriza C1.

**Reglas del Owner que siguen en F4** (`owner-f4t4c-decisions-d1-d6.md`):
- D2: el cierre sin el Owner, con 0B/0I, CI y verificación en verde, y controles ordinarios o con sustitutos de firma exacta;
- D3: la corrección en el intento 2 sin el Owner, y nunca el intento 3;
- D4: una replanificación reforzada por delegación;
- D5: volver ante una decisión material nueva.

**Evidencia** (custodiada y redactada en `F4-DRAFTS/c1/`):
- el registro de la corrida de Smoke-2 (pasos 0 a 16);
- el informe de diagnóstico, con el parche y las trazas (`diag1` y `diag2`), estas solo por su SHA-256;
- el mapa de guardas (`map.md`);
- la revisión independiente de la v1.

**Diseño vigente:**
- el paquete de F4-T4C (T4CP: D-T4C-09 en `:858-911`, D-T4C-14 y D-T4C-20);
- el paquete de F4-T4B (el escritor y la corrida) y el de F4-T2B2 (T2B2P: D-T2B2-10 y la regla de devolución `:256`);
- el Freeze (FRZ:216-218, FRZ:228-232 y FRZ:237-238);
- IC-04, IC-08 e IC-09.

## 2. Hechos medidos

**El rótulo del nombre (F4-E72)**
- **H-01. La regla del rótulo:**
  - solo se dibuja con `DrawRackName` y un nombre no vacío, que se recorta (`DynamicViewDecorations.cs:91-94`, `:156`, `:256`);
  - la alcanzan `DynamicSystemFrontalBuilder.cs:171`, `DynamicSystemLateralBuilder.cs:257` y `DynamicSystemPlantaBuilder.cs:74`, que usan los builders frontal, planta y lateral no compuesto de Push Back. `PushBackSystemLateralBuilder.cs:213` es el camino compuesto, que está fuera del piloto;
  - el diseño Push Back no guarda el nombre: está solo en el sobre (`Name`).
- **H-02. Quién asigna el nombre:**
  - RACKEDITAR asigna `system.Name = name` antes de redibujar (`RackPushBackCommands.cs:187-190`), con un único nombre (el de la ventana, o si no, el del sobre elegido) para todas las vistas;
  - **el escritor del piloto no:** `RackPilotUpdateWriter.cs:118` usa `PushBackPilotSystemRebuild.Build(member.DesignText, _catalog)`, que resuelve el diseño sin nombre (`PushBackPilotSystemRebuild.cs:24-36`).
- **H-03. Los llamadores de `Build`:**
  - el escritor (`:118`);
  - `PushBackPilotUpdateRun.cs:214`, solo para `IsComposite`;
  - `WorkspacePilotUpdateRunTests.cs:893-904`, que fija la firma de dos argumentos y sus tres excepciones en `:901-903`.
- **H-04. El sobre ya está en el plan** (`PushBackPilotMemberPlan.EnvelopeText`, `PushBackPilotUpdatePlan.cs:165`), y la corrida ya lo deserializa con `RackEmbedStore` (`PushBackPilotUpdateRun.cs:195-200`). `RackEmbedStore.Deserialize` devuelve `null` con un sobre ilegible (`RackEmbedDocument.cs:97-121`).
- **H-05. Validado en AutoCAD (diag2):** con `system.Name = embed.Name` en el escritor, el rótulo «PB G» se conserva tras «Actualizar», con la escala nueva, y la comprobación de la carga útil de `WriteUnit` no falla.

**La contabilidad asociativa (F4-S2-01)**
- **H-06. Cuándo llegan los eventos:** en D1, un dibujo real con objetos asociativos, llegan eventos `Database.ObjectModified` de un objeto **`AssocNetwork`** (siempre h=1AC25E; es la única clase no entidad que se ve fuera de los redibujos):
  - en cada lectura del panel, con la pila **dentro** de las funciones de lectura de RackCad (`PilotIntentReader.ReadOnActiveDocument`, `WorkspaceNavigationFacts.Scan` y `RackSiblingScan.Traverse`; traza `diag1`);
  - al terminar cada orden: tras «Actualizar», entre `CommandEnded(EXECUTEFUNCTION)` y `EnteringQuiescentState`, antes del drenaje (`diag1:29623-29627`; `diag2:7027-7031`); y también tras `RACKPANEL`, `ZOOM` y la selección implícita (`diag2:1-22`, `:82-94`);
  - **es una inferencia** desde la posición en la traza: AutoCAD reevalúa la red asociativa al bloquear y desbloquear, al confirmar y al terminar cada orden.
- **H-07. Cómo se convierten en una marca:**
  - el puente los reenvía (`AutoCadEventBridge.cs:240-274`) a `EventBridgeCore.OnSourceEvent`, que fuera del ámbito propio encola `DatabaseObjectChanged` ajeno (`EventBridgeCore.cs:346-353`);
  - el drenaje lleva entonces `MarkDraftsPossiblyStale` (`WorkspaceSession.cs:66-68`), y el host marca los borradores (IC-08);
  - tras «Actualizar»: el `CommandEnded(EXECUTEFUNCTION)` cierra el ámbito IC-09 y los `AssocNetwork` que siguen ya cuentan como ajenos.
- **H-08. Validado en AutoCAD (diag2):** sin reenviar los eventos de objetos `AssocAction` y `AssocDependency` (`AssocNetwork` es un `AssocAction`):
  - «Editar» deja «Listo para editar» estable;
  - «Comprobar ahora» deja «Limpio» estable;
  - Retroceso borra a la primera, que es F4-S2-02;
  - «Actualizar» deja «Limpio», sin la marca;
  - `U` y `REDO` siguen marcando, por su `CommandEnded` ajeno (FRZ:237-238).

  **Lo que diag2 no valida:** filtraba antes del núcleo, así que esos eventos no olvidaban la secuencia de cierre. Ver H-13.
- **H-09. Por qué H0 no lo vio:** su dibujo sintético no tenía red asociativa.

**Diseño y guardas**
- **H-10. Lo que dice el Freeze:**
  - FRZ:216-218 dice que los manejadores «no leen ni escriben la base de datos… filtran por tipo con coste constante». Esta corrección lo lee como el tipo en tiempo de ejecución del objeto del evento, y **es una interpretación declarada**: la frase admite también «por tipo de evento»;
  - el precedente es T4CP:866: leer `e.GlobalCommandName` es leer un dato en memoria del argumento, de coste constante;
  - IC-08 (ICL:143, «`MarkDraftsPossiblyStale` nace de cualquier cambio de base de datos») es una aclaración de la sesión, «escalonada… se comunica al Owner» (ICL:133). La corrección refina su letra para los objetos asociativos con IC-10 (§6).
- **H-11. Las guardas que fijan hoy el puente y el núcleo** (mapa `map.md` §2.4; la revisión de la v1 lo confirmó completo):
  - `WorkspaceBridgeFacadeSourceGuardTests.cs:216-236` (la superficie pública de `EventBridgeCore`);
  - `WorkspaceLifecycleGuardTests.cs:114`, `:126-142`, `:145-164` (`ForgetsBeforeAnyBranch`/`AfterTheStartedReturn`), `:171` y `:196-201`;
  - `WorkspaceLifecycleGuardTests.cs:133` y `WorkspaceHostC2SourceGuardTests.cs:205` (la firma de dos parámetros de `OnSourceEvent`, textual).
- **H-12. Los cinco manejadores de objetos** son hoy `ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source))`. `WorkspaceIdleSchedulingGuardTests.cs:129` exige que ese literal siga existiendo en algún sitio, y sigue en `ImpliedSelectionChanged` y en los tres finales de orden.
- **H-13. La secuencia de cierre:**
  - las dos trazas acaban con un `ObjectModified` de `AssocNetwork` unos 150 ms **después** del `CommandEnded(QUIT)` y del `EnteringQuiescentState` (`diag1:72391-72393`; `diag2:39668-39670`), plausiblemente dentro de la ventana `BeginDocumentClose` → `DocumentToBeDestroyed`, que no se trazó;
  - **hoy** (`b88dabf2`) ese evento ya olvida la secuencia de cierre, porque toda entrada pública que muta estado la olvida (T2B2P:268). Por tanto, en D1 la retirada al cerrar es probablemente `RetireConservative`, con posibles «falsos inciertos» (T2B2P:254-255).

## 3. Alcance

**Rutas permitidas (AllowedWriteScope, 11):**
- `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs` (C1);
- `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs` (C1: una llamada);
- `src/RackCad.Application/Workspace/EventBridgeCore.cs` (C2: una entrada nueva);
- `src/RackCad.Application/Workspace/BridgeSource.cs` (C2: el enumerado);
- `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs` (C2: los cinco manejadores de objetos y un clasificador privado);
- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs` (C1);
- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs` (C1);
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs` (C2: el cambio de guarda 1);
- `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs` (C2: los cambios de guarda 2 a 5);
- `tests/RackCad.Tests/Workspace/WorkspaceAssociativeBookkeepingTests.cs` (nuevo, C2);
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeObjectSubjectGuardTests.cs` (nuevo, C2).

**ForbiddenWriteScope.** Se construye **sin ningún prefijo que cubra una ruta permitida** (la comprobación de alcance exige diff ⊆ Allowed y diff ∩ Forbidden = ∅). Incluye:
- las raíces de la solución: `docs/`, `.github/`, `eng/`, `tools/`, `assets/`, `deploy/` y los archivos raíz;
- `src/RackCad.UI/` y `tests/RackCad.UI.Tests/`, completos;
- los archivos sensibles que esta corrección no puede tocar:
  - `WorkspaceHostCore.cs`, `WorkspaceSession.cs`, `Hints.cs`, `IWorkspacePilotEditPort.cs`, `IHostEventSource.cs` y `Drafts/DraftSession.cs` de `src/RackCad.Application/Workspace/`;
  - `PushBackPilotHostRun.cs`, `PushBackPilotUpdateRun.cs` y `PushBackPilotUpdatePlan.cs` de `src/RackCad.Application/Systems/PushBack/Pilot/`;
  - `WorkspaceController.cs` de `src/RackCad.Plugin/Workspace/`;
  - `RackPilotUpdateCommandContext.cs` y `RackSiblingScan.cs` de `src/RackCad.Plugin/Views/`;
  - la clausura de T1B (`src/RackCad.Plugin/KindHandlers/`, las fachadas y los `*Commands*.cs` de `src/RackCad.Plugin/`, archivo a archivo);
- las guardas y pruebas de T2C, T4B, T4C1 y T4C2 que no se tocan:
  - `WorkspaceHostTestSupport.cs`, `WorkspaceBridgeSourceGuardTests.cs`, `WorkspaceHostC2SourceGuardTests.cs`, `WorkspaceIdleSchedulingGuardTests.cs`;
  - `WorkspaceHintDrainTests.cs`, `WorkspaceOwnMutationTests.cs`, `WorkspaceBridgeHintAndDrainTests.cs`;
  - `WorkspacePilotUpdateIntegrationGuardTests.cs`, `WorkspacePilotUpdateHostTests.cs`, `WorkspacePilotUpdateScopeDouble.cs`;
  - `WorkspaceModelBoundaryTests.cs`, `WorkspaceBrowserBoundaryTests.cs`, `WorkspaceSourceGuardExceptionTests.cs`, `WorkspaceLifecycleHostTests.cs`.

El generador del contrato comprueba que ninguna permitida queda bajo ninguna prohibida.

## 4. Decisiones

**D-C1-01. C1: el sistema reconstruido lleva el nombre de su sobre.**

Un miembro puro nuevo: `public static PushBackSystem BuildForMember(PushBackPilotMemberPlan member, RackCatalog catalog)`.
- Con `member` nulo lanza `ArgumentNullException`, y con `catalog` nulo, lo mismo que `Build`.
- Resuelve con `Build(member.DesignText, catalog)`: las mismas tres excepciones.
- Deserializa `member.EnvelopeText` con `RackEmbedStore`.
- Si el sobre es ilegible (`null`), lanza `InvalidOperationException` con una razón constante nueva, `UnreadableEnvelopeReason`. Es el mismo trato que el escritor ya da a un sobre ilegible (`RackPilotUpdateWriter.cs:115-116`).
- Asigna `Name = envelope.Name` **en crudo**, sin recortar ni cambiar: el único consumidor, el rótulo, recorta (H-01). Un nombre nulo o en blanco queda tal cual y no hay rótulo, como RACKEDITAR.

El escritor llama a `PushBackPilotSystemRebuild.BuildForMember(member, _catalog)` en `:118`. `Build(string, RackCatalog)` no cambia (H-03).

**Paridad con RACKEDITAR (declarada):** RACKEDITAR aplica un solo nombre a todas las vistas; el piloto usa el sobre de **cada** miembro. Coinciden en un dibujo normal. Con nombres divergentes entre vistas, el piloto conserva el de cada vista, coherente con `DesiredNamesOf` (`PushBackPilotUpdateRun.cs:197-202`).

**Guardas:** se respetan las de pureza de `RebuildPath` (`WorkspacePilotUpdateGuardTests.cs:28`, `:56-71`; `RackEmbedStore` es `RackCad.Application.Persistence`, permitido) y las del escritor (G-1 a G-10 y `WriterStructureViolations`: ninguna cuenta cambia). **Ningún cambio de guarda.**

**D-C1-02. C2: el puente clasifica por tipo y el núcleo decide.**

- **El enumerado,** en `BridgeSource.cs`: `public enum ObjectEventSubject { Drawing, AssociativeBookkeeping }`. Ningún nombre casa con las prohibiciones de nombres de `RackCad.Application.Workspace` (revisión v1: comprobado).
- **El puente.** Los cinco bloques de objetos pasan a ser `ContainSession(a, () => _core.OnObjectEvent(a.Session, a.Source, SubjectOf(e.DBObject)));`, con `private static ObjectEventSubject SubjectOf(DBObject o) => o is AssocAction || o is AssocDependency ? ObjectEventSubject.AssociativeBookkeeping : ObjectEventSubject.Drawing;`.
  - Solo pruebas `is`: no abre el objeto ni lee su handle, su id, sus propiedades ni la base de datos.
  - `e.DBObject` se evalúa dentro de `ContainSession`. Si su getter lanzara, la sesión se degrada como hoy con cualquier excepción del manejador; por eso el clasificador está **contenido**, no es «total».
  - Esta corrección **sustituye, solo para los cinco manejadores de objeto**, la frase de D-T4C-09 «los demás manejadores no cambian: no leen handles ni el DWG» (T4CP:887). Lo hace con la misma lectura que T4CP:866: la prueba `is` sobre el tipo en tiempo de ejecución es un dato en memoria del argumento, de coste constante. IC-04 sigue intacta.
- **El núcleo:** `public bool OnObjectEvent(SessionId session, BridgeSource source, ObjectEventSubject subject)`.
  - **Justo después de `if (!_started) return false;` y antes del `try`,** igual que `OnSourceEvent`, olvida la secuencia de cierre (`_lastAllowedClose = null;`). Lo exige la guarda `ForgetsBeforeAnyBranch`/`AfterTheStartedReturn`.
  - Si `subject` es `AssociativeBookkeeping` y `source` es una de las cinco fuentes de objeto, hace las mismas comprobaciones de sesión que `OnSourceEvent`: una sesión desconocida, destruida o degradada no tiene efecto. Después devuelve `false`, sin pista, sin `SyncIdle` y sin tocar el ámbito propio, el reposo ni los contadores.
  - En cualquier otro caso (`Drawing`, o una fuente que no es de objeto) tiene **exactamente** el efecto de `OnSourceEvent(session, source)` y devuelve lo mismo.
- **`OnSourceEvent(SessionId, BridgeSource)` no cambia,** ni su firma ni su cuerpo (H-11).

**D-C1-03. Los cambios de guarda declarados.** Son estos cinco, exactos y con su autoprueba en negativo; ninguno más (si hiciera falta otro, es la parada C1-S02):
1. **`WorkspaceBridgeFacadeSourceGuardTests.cs:216-236`:** `DeclaredCoreSurface()` gana `"OnObjectEvent"`. Su negativa: la superficie real sin `OnObjectEvent` ya no es igual.
2. **`WorkspaceLifecycleGuardTests.cs:114`:** `Assert.Equal(16, …_lastAllowedClose\s*=\s*null…)` pasa a `17`.
3. **`WorkspaceLifecycleGuardTests.cs:126-142`:** `ForgettingEntries` gana la entrada de `OnObjectEvent`, con el mismo formato que las existentes y la firma exacta `public\s+bool\s+OnObjectEvent\s*\(\s*SessionId\s+\w+\s*,\s*BridgeSource\s+\w+\s*,\s*ObjectEventSubject\s+\w+\s*\)`.
4. **`WorkspaceLifecycleGuardTests.cs:171`:** `ForgettingEntries.Length` pasa de `16` a `17`.
5. **`WorkspaceLifecycleGuardTests.cs:196-201`:** la teoría negativa gana un `InlineData` para `OnObjectEvent`.

**D-C1-04. F4-S2-03 queda fuera y va al Owner.**

**Las dos formas de la corrección:**
- **La forma validada en AutoCAD (diag2):** `AcApplication.Idle += once` con `Editor.Regen()` en el puente. No toca `DrainOperation`, pero rompe:
  - los censos 16/16/16 de suscripciones;
  - INV-F1-T2-03 (`Regen` en `WorkspaceBridgeSourceGuardTests.cs:139-140`);
  - la regla del callback de Idle de IC-04 (ICL:81).
- **La forma recomendada** (mapa §3.4): la decisión en Application, una operación de drenaje nueva y un puerto en `Plugin/Views`. Cambia:
  - la lista congelada del drenaje (FRZ:228-232, «y solo estas»; `Hints.cs:14-21`; T4CP:845);
  - la intención de INV-03, INV-F1-T2-03 e INV-F1-T3A-08, que clasifican `Regen` como escritura de la vista.

**Lo que tienen en común:** las dos cambian el efecto tras Undo y Redo (FRZ:237-238), que es la categoría «Undo» de D5. Además, el disparo necesita leer propiedades en un manejador, o aceptar un disparo solo por tipo, con el caso «`U` de una LINE» sin medir.

**Hecho que falta:** no se midió si RACKEDITAR deja la vista igual de desactualizada tras `U`. Si la deja, S2-03 es preexistente y no un defecto del piloto.

**En esta tarea no se implementa.** La propuesta para el Owner está en `F4-DRAFTS/c1/owner-decision-request-s2-03.md`. Mientras tanto, Smoke-2 declara la limitación: tras `U` hace falta `REGEN`, y un `REGEN` entre `U` y `REDO` deja sin Rehacer.

**D-C1-05. El rendimiento no cambia.**
- El «Actualizar» de un rack grande tardó **como mucho** unos 14 s en D1. Es una cota superior: diag2 registraba cada evento de objeto que no es entidad.
- Es la mecánica del redibujo clásico (I-55, OV-RED): borra y crea miles de referencias y termina con un `REGEN` completo de unos 1,4 s.
- La orden de corrección incluye la observación, pero cambiar la mecánica del redibujo excede esta corrección. Se anota al Owner.

**D-C1-06. C2 y D5.** C2 no es una decisión material nueva de D5, por cuatro razones, y se anota al Owner:
- (a) la orden literal del Owner manda corregir los errores de Smoke-2, y F4-S2-01 es uno de ellos;
- (b) IC-08 es una aclaración escalonada de la sesión (ICL:133), y su refinamiento va al mismo nivel;
- (c) C2 restituye el resultado ratificado y esperado: tras «Actualizar», «Limpio» sin la marca (IC-09; S2-14; el paso 14 de Smoke-2 lo esperaba así);
- (d) C2 no cambia la semántica de ningún cambio de entidad ni de sobre: los Xrecord, los DBDictionary y las entidades siguen contando.

S2-03, en cambio, sí cambia el efecto de Undo y una lista congelada (D-C1-04).

**D-C1-07. IC-10.** La añade la sesión en el commit de emisión (§6). El Worker no la toca.

**D-C1-08. Las pruebas automatizadas.** Ninguna necesita AutoCAD.
- **Filtros de RequiredTests:**
  - Core, `FullyQualifiedName~RackCad.Tests.Workspace`, con **ExpectRed true**. Las clases nuevas empiezan por `Workspace`;
  - los 23 filtros de Core de T4C2, con ExpectRed false;
  - UI, `~RackCad.UI.Tests.Workspace` y `~PushBackClassicCharacterization`, con **ExpectRed false**: la UI no cambia.
- **Los mínimos** se miden de nuevo sobre la CI del GREEN de T4C2 (`43758023582`).

**D-C1-09. El RED y sus pruebas.**

**Los cuerpos inertes del RED:**
- `BuildForMember` devuelve `Build(member.DesignText, catalog)`, sin nombre;
- `OnObjectEvent` es `return OnSourceEvent(session, source);`, **sin** su propio olvido ni su clasificación;
- el enumerado ya está;
- el puente ya llama a `OnObjectEvent` con `SubjectOf`, que es el cambio de forma.

**Con eso, en el RED:**
- el cambio de guarda 1 pasa (la superficie ya tiene el miembro);
- los cambios 2 a 5 fallan por aserción (el texto aún tiene 16 olvidos), y eso se declara;
- las pruebas nuevas de comportamiento fallan.

**Las pruebas RED,** que fallan por aserción en el RED y pasan en el GREEN:
1. **`WorkspacePilotUpdateRunTests.TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope`.** Usa los kits de texto `PrepareKit`, que solo se deserializan (guarda `:848-866`). Comprueba:
   - cada miembro del plan esperado lleva el `Name` de su sobre;
   - un nombre con espacios alrededor sale idéntico, en crudo;
   - un nombre en blanco da `IsNullOrWhiteSpace`;
   - con `member` nulo, `ArgumentNullException`;
   - con un sobre ilegible, `InvalidOperationException(UnreadableEnvelopeReason)`.
2. **`WorkspacePilotUpdateGuardTests.PrepareUnitsBuildsTheSystemOfEachMemberWithItsRackName`,** una guarda de texto del escritor. Comprueba:
   - una sola `PushBackPilotSystemRebuild.BuildForMember(member, _catalog)` en `PrepareUnits`;
   - ningún `PushBackPilotSystemRebuild.Build(` en el escritor;
   - la autoprueba en negativo como mutación del texto real (`BuildForMember(member, _catalog)` → `Build(member.DesignText, _catalog)`).
3. **`WorkspaceAssociativeBookkeepingTests`:**
   - (a) una teoría sobre las cinco fuentes de objeto: un evento de contabilidad no encola pista, no engancha Idle y no falla;
   - (b) **la contabilidad del fin de la orden del Actualizar:** con `OpenOwnMutation`, un `ObjectModified` de dibujo, el `CommandEnded` que cierra, tres de contabilidad y `OnIdle`, las operaciones son `[InvalidateNavigationCaches]`, sin `MarkDraftsPossiblyStale`;
   - (c) con `WorkspaceHostRig` tal cual (`WorkspaceHostTestSupport.cs:132`, que no se toca), `EditIntentKit` y `EditIntentFakeReader`: un lector doble que llama a `Bridge.Core.OnObjectEvent(…, AssociativeBookkeeping)` **dentro** de su lectura, como hace AutoCAD durante el bloqueo, deja el borrador sin la marca tras el drenaje.
4. **Fijaciones,** que pasan ya en el RED y protegen lo ratificado:
   - `Drawing` tiene exactamente el efecto de `OnSourceEvent`, incluida la lista de `WorkspaceBridgeHintAndDrainTests.cs:106-117`;
   - la contabilidad sigue olvidando la secuencia de cierre: `LastIntact == false`, como `WorkspaceOwnMutationTests.cs:292-323`. **En el RED esta pasa por el olvido de `OnSourceEvent`**;
   - no abre ni cierra el ámbito propio ni cambia el reposo, por reflexión sobre `_states`, como `WorkspaceOwnMutationTests.cs:27-32`;
   - una sesión desconocida o degradada no tiene efecto;
   - `U` justo tras el Actualizar sigue siendo ajeno y marca (`WorkspaceOwnMutationTests.cs:198-210`).
5. **`WorkspaceBridgeObjectSubjectGuardTests`,** sobre el texto del puente. Comprueba:
   - los cinco bloques de objeto son exactamente `ContainSession(a, () => _core.OnObjectEvent(a.Session, a.Source, SubjectOf(e.DBObject)));`;
   - `SubjectOf` usa solo pruebas `is` sobre `AssocAction` y `AssocDependency` y no nombra `Handle`, `ObjectId`, `\.Id\b`, `\.Database`, `GetObject`, `OpenMode` ni `\.Read\w*\s*\(`;
   - los bloques de `ImpliedSelectionChanged`, `CommandWillStart` y los tres finales de orden no cambian;
   - las autopruebas en negativo.

## 5. Invariantes

- **INV-C1-01 (el nombre):** el sistema que el escritor redibuja lleva el `Name` del sobre de su miembro, en crudo. Nada más del sistema cambia.
- **INV-C1-02 (la contabilidad asociativa):**
  - solo los objetos `AssocAction` y `AssocDependency` dejan de contar como cambio del dibujo;
  - todo otro evento de objeto tiene exactamente el efecto de hoy, propio o ajeno y con las mismas pistas;
  - `CommandEnded` y las demás fuentes no cambian;
  - «toda orden marca» (IC-08) sigue intacto.
- **INV-C1-03 (T2B2B):** todo evento de objeto, también el de contabilidad, sigue olvidando la secuencia de cierre. La razón vinculante es T2B2P:268 («la olvida en toda entrada pública que muta estado del core») y la regla de devolución T2B2P:256: el mecanismo de la secuencia de cierre no se cambia sin el Owner. Por eso C2 no deja de olvidar, aunque diag2 sí lo hiciera (H-13).
- **INV-C1-04 (IC-04, FRZ:218 y D-T4C-09 leída con T4CP:866):**
  - el clasificador es solo por tipo y está contenido en `ContainSession`;
  - ningún manejador lee la base de datos, el handle, el id ni propiedades del objeto;
  - ningún `+=`, `BeginInvoke`, `Idle`, `Regen` ni suscripción nueva: las cuentas 16/16/16 siguen.
- **INV-C1-05 (lo ratificado no cambia):**
  - el ámbito de mutación propia (IC-09);
  - el billete y su caducidad por `CommandWillStart` ajeno;
  - el drenaje y su lista de operaciones, con `DrainOperation` y `HintKind` sin cambios;
  - la corrida, el escritor (salvo la llamada de C1), el puerto del panel y la UI.

  Las guardas G-1 a G-10 de F4-T4C1 y las de T4C2 siguen en verde.
- **INV-C1-06 (cambios de guarda):** solo los cinco de D-C1-03.
- **INV-C1-07 (sin F4-S2-03):** ninguna regeneración, ningún tratamiento de `U`, `UNDO` ni `REDO` y ninguna operación de drenaje nueva.
- **INV-C1-08 (T1B):** el diff no toca la clausura de T1B, las fachadas, `KindHandlers/` ni los `*Commands*.cs`.
- **INV-C1-09 (frontera de Application):** ningún tipo de AutoCAD en una firma de Application, y el enumerado es neutro.
- **INV-C1-10 (el RED):** los cuerpos inertes de D-C1-09; las existentes pasan, salvo los cambios de guarda 2 a 5, declarados; las nuevas fallan por aserción; ninguna prueba necesita AutoCAD.
- **INV-C1-11 (contención, F4-E59):** ninguna prueba escribe fuera de una ubicación temporal aislada ni usa la configuración real; sin la suite completa de UI en local.

## 6. IC-10 (la añade la sesión en la emisión)

> **IC-10. La contabilidad asociativa de AutoCAD no es un cambio del dibujo (F4-T4-HOST-C1).**
>
> **Autoridad:**
> - la orden del Owner de corregir los hallazgos de Smoke-2;
> - FRZ:216-218, leído como el tipo en tiempo de ejecución del objeto del evento, con el precedente T4CP:866;
> - IC-08, cuya letra («cualquier cambio de base de datos») refina.
>
> Sustituye la frase de D-T4C-09 «los demás manejadores no cambian» (T4CP:887) solo para los cinco manejadores de objeto. No cambia el Freeze y se comunica al Owner.
>
> **Regla:** un evento de objeto de la base de datos cuyo objeto es del marco asociativo de AutoCAD (`AssocAction`, que incluye `AssocNetwork`, o `AssocDependency`):
> - no encola ninguna pista;
> - no marca los borradores;
> - no invalida las cachés de navegación;
> - sigue olvidando la secuencia de cierre (T2B2P:268).
>
> El puente clasifica solo por tipo, en el manejador y con coste constante, sin abrir el objeto. El núcleo decide. Todo otro evento de objeto y todo `CommandEnded` siguen como en IC-08. No choca con «no puede suprimir `InvalidateNavigationCaches`» (ICL:150), porque esa regla rige la mutación propia, que sigue invalidando por sus eventos de entidades y definiciones.
>
> **Por qué:** en un dibujo con objetos asociativos, AutoCAD modifica la red asociativa en cada bloqueo o confirmación, también en las lecturas del propio panel, y al terminar cada orden. Es una inferencia por la posición en la traza. Eso marcaba el borrador tras cada lectura (Smoke-2, F4-S2-01).
>
> **Residual declarado:**
> 1. Un cambio que solo toque objetos `AssocAction` o `AssocDependency`:
>    - **dentro de una orden:** marca igual, por el fin de esa orden, también si se cancela o falla;
>    - **fuera de toda orden** (una paleta no modal, otro complemento en reposo o un reactor): no marca.
>
>    Un cambio así no puede alterar un sobre de rack: los Xrecord, los DBDictionary y las entidades siguen contando.
> 2. El filtro se limita a esas dos jerarquías. `AssocActionBody`, `AssocDependencyBody` y las demás clases siguen contando, que es lo conservador. Solo D1 está medido.
> 3. En D1, el evento asociativo posterior a `QUIT` probablemente deja la secuencia de cierre sin intacta. Es el mismo comportamiento que antes de esta IC, y Smoke-2 lo observa (T2B2P:256).

## 7. Paradas propias (además de las comunes S y P y las C de T4C2 que apliquen)

- **C1-S01:** la corrección exige cambiar la firma o el cuerpo de `OnSourceEvent`, o `DrainOperation`, `HintKind`, `WorkspaceHostCore.cs`, `WorkspaceSession.cs`, el puerto, la UI, el corredor, el adaptador o el ámbito propio.
- **C1-S02:** un cambio de guarda fuera de los cinco de D-C1-03.
- **C1-S03:** el clasificador necesita leer propiedades del objeto, su handle o id, o la base de datos.
- **C1-S04:** cualquier tratamiento de `U`, `UNDO`, `REDO` o `MREDO`, o cualquier `Regen`.
- **C1-S05:** una decisión material nueva de D5. **STOP al Owner.**
- **C1-S06:** un fallo de fidelidad literal de la planificación permite una replanificación reforzada (D4); un segundo fallo es STOP al Owner.

## 8. Después

- **El cierre:** por la regla D2.
- **Smoke-2:** se reanuda con el Owner presente, con el DLL nuevo y la guía v3 ajustada. Se recomienda repetirlo desde el bloque 1, porque el producto cambia.
- **Puntos de observación nuevos en Smoke-2:**
  - el rótulo del nombre tras «Actualizar»;
  - el estado «Listo para editar» o «Limpio» estable tras «Editar» y «Comprobar ahora»;
  - en el bloque de CLOSE, CLOSEALL y QUIT sobre D1, qué vía de retirada se ve (H-13, con la regla de devolución T2B2P:256 si la secuencia nunca queda intacta).
- **F4-S2-03:** espera la decisión del Owner (D-C1-04).

## 10. Disposición de la revisión independiente de la v1 (0 B / 4 I / 14 m)

| Hallazgos | Se incorporan en |
|---|---|
| I-1 | D-C1-06; la autoridad de `owner-f4e72-decision.md` en el §1 |
| I-2 | D-C1-02, INV-C1-04, IC-10 y la prueba 5 |
| I-3 | El residual de IC-10 |
| I-4 | H-13, INV-C1-03, IC-10 y el §8 |
| m-1 y m-2 | H-01 a H-04 |
| m-3 y m-4 | D-C1-01 y la prueba 1 |
| m-5 | D-C1-02 y D-C1-03 |
| m-6 | D-C1-02 («contenido») |
| m-7 | La prueba 2 |
| m-8 | La prueba 3c, el RED inerte y `WorkspaceHostTestSupport.cs` en el prohibido |
| m-9 | El §3 |
| m-10 | D-C1-04 |
| m-11 | D-C1-05 |
| m-12 | IC-10 |
| m-13 | H-06, H-07 y la prueba 3b |
| m-14 | El §1 y la custodia de la emisión |
