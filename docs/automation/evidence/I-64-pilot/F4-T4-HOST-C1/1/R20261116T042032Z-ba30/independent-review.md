# I-64 F4-T4-HOST-C1: revisión adversarial independiente del código

**Objeto:** `git diff 16ce44459f05c6b80d48dee63e1210b1acd5f3e1 b86c3cfe2c39e7b0fea7ff6e62f431c68751d3b1`, en la rama `architecture/workspace-persistente-rackcad`.
- BaseSha `16ce4445`: la emisión del contrato.
- RED `61ffc8288e4cb3af5ac63906e752a9600dafecfb`.
- GREEN `b86c3cfe2c39e7b0fea7ff6e62f431c68751d3b1`.
- Primera delegación de la cadena.

**Método:**
- Solo lectura con `git -C` (`show`, `diff`, `grep`, `log`) y lectura de los artefactos de la corrida.
- No ejecuté dotnet ni ninguna prueba, no hice checkout y no escribí en el worktree.
- Leí el contrato, el paquete v2, IC-04/08/09/10, el diagnóstico, `diag2.patch.txt` y la entrega.
- No leí la revisión directa de la sesión.
- Las rutas son relativas al repositorio. Las líneas citadas son del GREEN salvo que diga RED o Base.

## 1. Veredicto

**ACEPTABLE CON MENORES. 0 B / 0 I / 3 M.**

**OWNER:** ninguno nuevo.

**C1 es correcto:**
- `BuildForMember` cumple D-C1-01 a la letra.
- El nombre llega al rótulo por el único camino de redibujo del piloto.
- No queda otra vía de reconstrucción sin nombre.

**C2 es correcto:**
- La forma y la semántica de `OnObjectEvent` son las de D-C1-02, INV-C1-02 e INV-C1-03.
- `OnSourceEvent` no cambia.
- El puente cambia solo los cinco bloques de objeto y añade `SubjectOf`.
- No encontré ninguna secuencia concreta en la que un cambio ajeno que el piloto deba ver deje de marcar el borrador fuera del residual que IC-10 ya declara (§5).

**Los tres menores son de fidelidad y cosméticos.** Ninguno afecta al producto:
- **M-1:** el RED llevaba ya la llamada final del escritor y la prueba 2 no llegó a estar en rojo.
- **M-2:** un espacio quitado en una aserción existente.
- **M-3:** un comentario de guarda desfasado.

## 2. Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| M-1 | Menor | **El producto del RED tiene un cambio más que los cuerpos inertes de INV-C1-10 y D-C1-09:** el escritor ya llama a `BuildForMember(member, _catalog)`.<br><br>Consecuencia: la prueba 2 (`PrepareUnitsBuildsTheSystemOfEachMemberWithItsRackName`) pasa en el RED, aunque D-C1-09 la cuenta entre «las pruebas RED, que fallan por aserción en el RED».<br><br>La prueba 5 tiene la misma tensión, pero esa es del propio paquete: D-C1-09 pone la forma nueva del puente en el RED, así que su guarda no puede fallar allí.<br><br>La entrega anota las dos entre las que «pasan ya en el RED», pero deja `Deviations: []`.<br><br>El origen es la delegación, que añadió «el escritor ya llama a BuildForMember» al paso RED. | RED `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs:118`.<br><br>Paquete §4 D-C1-09, punto 2.<br><br>INV-C1-10 (contrato): «el enumerado y la forma nueva del puente ya estan», sin el escritor.<br><br>`artifacts/.../R20261116T031655Z-e758/prompt.md:171`.<br><br>TRX `red-ci-core/core.trx`: 11 fallos, sin la prueba 2.<br><br>`worker-handoff.json`, Evidence «Pruebas que pasan ya en el RED». | Sin cambio de código.<br><br>Registrar en el paquete de cierre una desviación declarada de la planificación: las pruebas 2 y 5 son guardas de forma y pasan en el RED.<br><br>Su capacidad de discriminar queda probada por sus autopruebas negativas sobre el texto real, que reproducen la forma anterior a C1:<br>- `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs:304` pone `Build(member.DesignText, _catalog)`;<br>- `tests/RackCad.Tests/Workspace/WorkspaceBridgeObjectSubjectGuardTests.cs:164` vuelve a `OnSourceEvent`. |
| M-2 | Menor | **Cambio no declarado del texto de una aserción existente:** la guarda del cambio 1 pasa de `Assert.Equal(4, declared.Count(...))` a `Assert.Equal(4,declared.Count(...))`. Solo se quita un espacio.<br><br>Es nulo en semántica: el mismo número, el mismo predicado y el mismo resultado. Está en el método que el cambio 1 amplía.<br><br>A mi juicio **no es C1-S02**, porque no cambia ningún número, texto, patrón ni comportamiento de la guarda. Aun así, queda fuera de la letra de los cinco cambios. | `tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs:250` (Base `:246`).<br><br>`git log -S` lo sitúa en `61ffc828`.<br><br>La entrega no lo menciona. | Registrarlo en el cierre como cambio de formato sin efecto.<br><br>Restaurar el espacio si el archivo se vuelve a tocar. No merece un intento por sí solo. |
| M-3 | Menor | **Comentario de guarda desfasado.** La teoría negativa del olvido gana `InlineData("OnObjectEvent")` (cambio 5), pero su comentario sigue diciendo «the negatives of the three entries of the own mutation». | `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:197-198` y `:203`. | Ninguna obligatoria. Actualizar el comentario si el archivo se vuelve a tocar. |

## 3. Alcance (PD-F4-07): comparación de las listas

Comparé las listas por programa: el diff de `git diff --name-only` contra los arrays del contrato del GREEN.

| Comprobación | Resultado |
|---|---|
| `git diff --name-only 16ce4445 b86c3cfe` | 11 rutas |
| AllowedWriteScope del contrato | 11 rutas |
| diff ⊆ Allowed | **Sí**, sin ninguna ruta fuera |
| Allowed ⊆ diff | **Sí**: los dos conjuntos son **idénticos** |
| diff ∩ Forbidden (igualdad o prefijo con `/` final) | **∅** |
| Allowed ∩ Forbidden | **∅** |
| GREEN (`61ffc828..b86c3cfe`) | Solo 2 rutas: `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs` y `src/RackCad.Application/Workspace/EventBridgeCore.cs`. **Ninguna bajo `tests/`.** |
| RED (`16ce4445..61ffc828`) | Las 11 rutas |

**Rutas del diff, todas permitidas:**
- `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs`;
- `src/RackCad.Application/Workspace/BridgeSource.cs`;
- `src/RackCad.Application/Workspace/EventBridgeCore.cs`;
- `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`;
- `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`;
- en `tests/RackCad.Tests/Workspace/`: `WorkspaceAssociativeBookkeepingTests.cs` (nuevo), `WorkspaceBridgeFacadeSourceGuardTests.cs`, `WorkspaceBridgeObjectSubjectGuardTests.cs` (nuevo), `WorkspaceLifecycleGuardTests.cs`, `WorkspacePilotUpdateGuardTests.cs` y `WorkspacePilotUpdateRunTests.cs`.

**No se tocan:**
- `WorkspaceHostCore.cs`, `WorkspaceSession.cs`, `Hints.cs`, `IWorkspacePilotEditPort.cs`, `IHostEventSource.cs` ni `Drafts/`;
- la corrida, el plan ni el corredor;
- `RackPilotUpdateCommandContext.cs`, `RackSiblingScan.cs` ni `WorkspaceController.cs`;
- `KindHandlers/` ni los `*Commands*.cs`;
- la UI.

Los trailers del RED y del GREEN son `Co-Authored-By: Claude Sonnet 5.5`.

## 4. C1 (D-C1-01, INV-C1-01)

| Requisito | Evidencia | Resultado |
|---|---|---|
| Firma `public static PushBackSystem BuildForMember(PushBackPilotMemberPlan, RackCatalog)` | `PushBackPilotSystemRebuild.cs:46` | Conforme |
| `member` nulo da `ArgumentNullException` | `:48` | Conforme |
| `catalog` nulo, como `Build` | `:50` → `Build` `:29` | Conforme. El orden es member, catálogo y diseño, luego el sobre. |
| Resuelve con `Build(member.DesignText, catalog)`, con las tres excepciones | `:50` | Conforme |
| Un sobre ilegible da `InvalidOperationException(UnreadableEnvelopeReason)` | `:20` (constante) y `:51-52` | Conforme |
| `Name` en crudo | `:53`: `system.Name = envelope.Name;` | Conforme. Sin recortar y sin cambiar nulo ni blanco. |
| Puro: las guardas de `RebuildPath` (texto en crudo, también los comentarios) | El texto nuevo no nombra Transaction, `\bDatabase\b`, Autodesk, RackCad.Plugin/UI, System.Windows, Task, Dispatcher, System.IO/File/Stream ni `typeof(`. `RackEmbedStore` es de `RackCad.Application.Persistence` y ya estaba importado (`:4`). | Conforme. La CI de Core del GREEN pasa 19063/19063. |
| `Build(string, RackCatalog)` sin cambio | El diff no toca `:26-38` | Conforme |
| El escritor cambia solo `:118` | El diff de `RackPilotUpdateWriter.cs` tiene una línea, `:118` | Conforme |

**¿Llega el nombre al rótulo?** Sí:
- `PushBackSystem.Name` reenvía a `Structure.Name` (`src/RackCad.Domain/Systems/PushBack/PushBackSystem.cs:140-144`), y `Structure` nunca es nulo (`:18`).
- Los tres servicios capturan el mismo `system` en la lambda del plan:
  - `PushBackFrontalDrawService.cs:56-60`;
  - `PushBackPlantaDrawService.cs:51-54`;
  - `PushBackSystemDrawService.cs:58-62`.
- Los builders de Push Back pasan `system.Structure` al builder dinámico:
  - frontal: `PushBackSystemFrontalBuilder.cs:184-199` y `:491-497`;
  - planta: `PushBackSystemPlantaBuilder.cs:58-72`;
  - lateral no compuesto: `PushBackSystemLateralBuilder.cs:294-312`.
- El builder dinámico dibuja el rótulo con `system.DrawRackName && !IsNullOrWhiteSpace(system.Name)` (`DynamicViewDecorations.cs:91`, `:156`, `:256`).

Es la misma cadena que usa RACKEDITAR (`RackPushBackCommands.cs:187-190`). Además, el valor coincide con el del parche validado en AutoCAD (diag2: `system.Name = embed.Name`): `embed` sale de deserializar el mismo `member.EnvelopeText`.

**¿Otra vía sin nombre?** No:
- Los llamadores de `PushBackPilotSystemRebuild.Build` son el escritor (ahora `BuildForMember`) y `PushBackPilotUpdateRun.cs:214`. Este solo pregunta `IsComposite` y no dibuja.
- Los otros `new PushBackResolver` del piloto no dibujan:
  - `PushBackPilotMaterializer.cs:151` carga el editor;
  - `PushBackPilotUpdatePlan.cs:381` calcula los cortes disponibles.
- `PrepareRedraw` de Push Back solo se llama desde el escritor (`RackPilotUpdateWriter.cs:123`, `:126`, `:129`).
- El caso compuesto queda fuera del piloto (`PushBackPilotMaterializer.cs:148`).

## 5. C2 (D-C1-02, INV-C1-02 a 04, IC-10)

| Requisito | Evidencia | Resultado |
|---|---|---|
| Enumerado neutro `ObjectEventSubject { Drawing, AssociativeBookkeeping }` | `BridgeSource.cs:43-47` | Conforme. No nombra ningún tipo del host. |
| `SubjectOf` privado y estático, solo con `is` | `AutoCadEventBridge.cs:351` | Conforme. No abre el objeto ni lee handle, id, propiedades ni la base de datos. |
| Los cinco bloques de objeto con la forma exacta | `AutoCadEventBridge.cs:243`, `:250`, `:257`, `:264`, `:271` | Conforme. `e.DBObject` se evalúa dentro de `ContainSession` (`:390-407`). |
| `ImpliedSelectionChanged`, `CommandWillStart` y los tres finales de orden sin cambio | El diff del puente toca solo `:240-274` y `:347-351` | Conforme |
| Sin `+=`, `-=`, `BeginInvoke`, `Idle`, `Regen` ni suscripción nueva | El diff no los añade | Conforme. Cuentas 16/16/16: `WorkspaceHostC2SourceGuardTests.cs:194-196` y la guarda nueva `:121-125`. |
| El olvido va justo tras `if (!_started) return false;` y antes del `try`, también con la contabilidad | `EventBridgeCore.cs:378-381` | Conforme. Ningún `return` posterior a `:378` precede al olvido. |
| Con contabilidad y fuente de objeto | `EventBridgeCore.cs:383-385` y `:394-396` | Conforme. Las mismas comprobaciones de sesión; `false`, sin pista, sin `SyncIdle` y sin tocar `OwnMutationOpen`, `Quiescent` ni `ForeignCommandStarts`. |
| Todo lo demás, exactamente `OnSourceEvent` | `:380` | Conforme. El doble olvido es idempotente. |
| Excepciones contenidas | La rama de contabilidad tiene su `try/catch` (`:387-391`, `Fault`); la otra delega en `OnSourceEvent`, que contiene las suyas (`:361-365`). `IsObjectSource` no puede lanzar. | Conforme |
| `OnSourceEvent` sin cambio de firma ni de cuerpo | El primer hunk empieza tras su `}` (`:366`) | Conforme |
| Un valor fuera del enumerado | `subject != AssociativeBookkeeping` va a `OnSourceEvent` | Conservador |
| `e.DBObject` nulo | `null is AssocAction` es falso, así que cuenta como `Drawing` | Conservador |

**Análisis adversarial: ¿puede la clasificación ocultar un cambio real?**

- **(a) Un `ObjectErased` u `ObjectAppended` de un `AssocAction` junto a un cambio de entidad del rack.**
  - La entidad, el BTR, el `Xrecord` o el diccionario producen su propio evento, que es `Drawing` y cuenta igual que hoy.
  - Solo cae el evento asociativo que lo acompaña.
  - Lo cubre la fijación `ADrawingObjectEventHasExactlyTheEffectOfASourceEventAlsoInsideTheOwnScope` (`WorkspaceAssociativeBookkeepingTests.cs:173-192`, propio y ajeno).
- **(b) `U`, `REDO` o `UNDO` ajenos.** Son órdenes: su `CommandEnded` marca sin cambio (IC-08 y FRZ:237-238).
  - Lo cubren `AnUndoRightAfterTheUpdateIsForeignAndMarksTheDraftsAlsoWithTheBookkeepingAroundIt` (`:341-367`), incluido un `U` que solo produce contabilidad, y `WorkspaceOwnMutationTests.cs:198-210`, que no se toca.
- **(c) Una edición que solo produce eventos asociativos.**
  - **Dentro de una orden:** marca por su fin. `AnySourceThatIsNotAnObjectOne…` (`:207-224`) fija que `CommandEnded` con cualquier sujeto marca.
  - **Fuera de toda orden** (paleta no modal, reactor u otro complemento en reposo): no marca.
  - Es el **residual 1 declarado** de IC-10, y la prueba 3a lo fija tal como se diseñó.
  - No encontré ningún caso que, sin tocar entidades, BTR ni `Xrecord`, altere lo que lee el piloto (sobre, diseño, nombre, vistas):
    - un parámetro, una restricción o una matriz asociativa que mueve geometría modifica entidades, y esos eventos cuentan;
    - las ediciones desde la paleta Propiedades disparan `ObjectModified` de la entidad.
  - **Sin hallazgo.**
- **(d) La mutación propia.**
  - Dentro del ámbito, los eventos asociativos ya no dejan pista propia. Las entidades y definiciones que redibuja el Actualizar siguen dejando `InvalidateNavigationCaches`, lo que respeta ICL:150 (`AfterTheUpdateClosedItsScope…`, `:95-114`).
  - El cierre del ámbito por el primer fin de orden no depende de eventos de objeto.
- **(e) La secuencia de cierre.** Todo evento de objeto la olvida, de los dos sujetos y en las cinco fuentes (`EveryObjectEventAlsoTheBookkeepingOneStillForgetsTheCloseSequence`, `:241-273`, con su control).
  - El evento asociativo posterior a `QUIT` sigue dejándola sin intacta (residual 3, H-13), como antes de C2.
- **(f) Un modo de fallo nuevo.** Si el getter de `e.DBObject` lanzara, la sesión se degrada por `ContainSession`. Antes los manejadores no tocaban el argumento.
  - Es lo que declara D-C1-02 («contenido, no total»).
  - diag2 hacía la misma lectura **fuera** de toda contención en todos los eventos de D1, sin ningún ERROR en la traza.

## 6. Lo ratificado no cambia (INV-C1-05, 07, 08, 09)

- **Ámbito propio (IC-09), billete y caducidad:** `OpenOwnMutation`, `CloseOwnMutation`, `NoteForeignCommandStart` y `SetQuiescent` no cambian. El cuerpo de `OnSourceEvent` (`:312-366`) es idéntico al de la Base.
- **Drenaje:** `Hints.cs` y `WorkspaceSession.cs` quedan fuera del diff; `DrainOperation` y `HintKind` no cambian.
- **Sin cambios:** `WorkspaceHostCore.cs`, la corrida, el corredor, el adaptador, el puerto y la UI.
- **Sin S2-03:** ningún `Regen`, `Idle` ni tratamiento de `U`, `UNDO`, `REDO` o `MREDO`.
  - El código de undo de diag2 (`DiagUndoStart`, `DiagUndoEnd`, `AcApplication.Idle += once` y `Editor.Regen()`) no se trasladó.
  - La guarda nueva lo prohíbe (`WorkspaceBridgeObjectSubjectGuardTests.cs:125`).
- **T1B intacta:** ningún `*Commands*.cs`, `KindHandlers/` ni fachada en el diff.
- **Frontera de Application:**
  - `OnObjectEvent(SessionId, BridgeSource, ObjectEventSubject)` no nombra tipos de AutoCAD;
  - `AssocAction` y `AssocDependency` aparecen solo en el Plugin;
  - `WorkspaceBridgeFacadeSourceGuardTests.cs:267-270` sigue en verde.

## 7. Los cinco cambios de guarda (D-C1-03, INV-C1-06)

| # | Declarado | Aplicado (GREEN) | Negativa | En el RED | Conforme |
|---|---|---|---|---|---|
| 1 | `WorkspaceBridgeFacadeSourceGuardTests.cs:216-236`: `DeclaredCoreSurface()` gana `"OnObjectEvent"`. Negativa: la superficie real sin él ya no es igual. | `:216` (comentario); `:230` `var c1 = new[] { "OnObjectEvent" };`; `:231` `.Concat(c1)` | `:248` `Assert.NotEqual(declared, real.Where(n => n != "OnObjectEvent").ToArray())` y `:249` `Assert.Contains`, sobre la superficie real por reflexión | Pasa: el miembro inerte existe | **Sí.** Además, M-2 en `:250`. |
| 2 | `WorkspaceLifecycleGuardTests.cs:114`: la cuenta de `_lastAllowedClose = null;` pasa de 16 a 17 | `:115` `Assert.Equal(17, …)`, con el comentario `:112-114` | La propia cuenta: el texto inerte tiene 16 | Falla por aserción («Values differ») | **Sí** |
| 3 | `:126-142`: `ForgettingEntries` gana `OnObjectEvent` con la firma exacta | `:144`, con el mismo formato y la regex idéntica a D-C1-03 | La recorre `:174-179`, y la teoría 5 la niega | Falla: `:177` «OnObjectEvent does not forget exactly once» | **Sí** |
| 4 | `:171`: `ForgettingEntries.Length` pasa de 16 a 17 | `:173` | Junto con el 3 | El método falla por el 3; la aserción de longitud pasa porque la entrada ya existe | **Sí** |
| 5 | `:196-201`: la teoría negativa gana `InlineData` de `OnObjectEvent` | `:203` | `:211-215`: sin olvido, y con el olvido tras una rama, sobre el cuerpo real | Falla: `:209` `Assert.True` | **Sí.** Además, M-3 en `:197-198`. |

**Ninguna otra guarda existente cambia de semántica.** Los otros dos archivos de pruebas tocados solo **añaden** métodos nuevos, al final:
- `WorkspacePilotUpdateGuardTests.cs` (`:283-310`);
- `WorkspacePilotUpdateRunTests.cs` (`:906-963`).

Los dos son pruebas nuevas de D-C1-09, no cambios de guarda. La única alteración de texto existente fuera de los cinco es el espacio de M-2.

## 8. El RED (D-C1-09, INV-C1-10)

Uso los TRX de CI custodiados en la corrida. Sus SHA-256 coinciden con los de `relay-record.json`:

| Archivo | SHA-256 | Corrida y SHA |
|---|---|---|
| `red-ci-core/core.trx` | `FF77E19C…27B6DA` | 43799572094 sobre `61ffc828` |
| `red-ci-ui/ui.trx` | `010B00C3…231B0` | 43799572094 sobre `61ffc828` |
| `ci-core/core.trx` | `9CAE1670…BAA13` | 43801607010 sobre `b86c3cfe` |
| `ci-ui/ui.trx` | `E3549EB3…6813` | 43801607010 sobre `b86c3cfe` |

**Producto del RED:**
- `BuildForMember` es `return Build(member.DesignText, catalog);` (RED `PushBackPilotSystemRebuild.cs:46-49`);
- `OnObjectEvent` es `return OnSourceEvent(session, source);` (RED `EventBridgeCore.cs:372-375`);
- el enumerado y los cinco bloques del puente con `SubjectOf` están en su forma final;
- **también la llamada final del escritor** (M-1).

**Core RED:** 19063 ejecutadas, 19052 pasan y **11 fallan, todas por aserción**. No hay fallos por excepción del esqueleto.
- **Cambios de guarda 2, 3/4 y 5:** `TheCloseSequenceIsOneNullableSessionField…`, `EveryEntryThatChangesTheStateOfTheCore…` y `TheForgettingGuardTrips…(name: "OnObjectEvent")`.
- **Prueba 1:** «Strings differ».
- **Prueba 3a:** sus 5 casos.
- **Pruebas 3b y 3c.**

**Lo que pasa en el RED:**
- el cambio de guarda 1 y todas las fijaciones;
- las pruebas 2 y 5 (M-1);
- **ninguna prueba existente sin cambio falla**: las 11 son nuevas o están cambiadas;
- UI: 2386 pasan y 17 se omiten a propósito, las mismas que en el GREEN.

**El job «Build Plugin without AutoCAD» del RED quedó en `skipped`.** El del GREEN, con el puente idéntico al del RED, está en `success`.

**GREEN:**
- Core: 19063 de 19063;
- UI: 2386 pasan y 17 omitidas;
- los cuatro jobs en `success`;
- Workspace de Core: 3482 seleccionadas, igual en el RED y en el GREEN.

Las 45 pruebas nuevas (3482 − 3437) cuadran con mi recuento de los archivos.

## 9. ¿Discriminan las pruebas?

| Prueba | ¿Falla si se quita lo que defiende? |
|---|---|
| 1 `TheSystemRebuiltForAMember…` | **Sí.** Sin la asignación, falla el `Name` (RED). Si se recorta, falla `:946`. Sin la comprobación de `member`, `NullReferenceException` y falla `:954`. Sin la razón propia, falla `:962`. Cada miembro se prueba con su propio sobre (`:941-949`). |
| 2 `PrepareUnitsBuilds…` (texto del escritor) | No estuvo en rojo (M-1). Sus negativas sobre el **texto real** (`:304-309`) incluyen la forma exacta anterior a C1, `Build(member.DesignText, _catalog)`, así que discrimina. |
| 3a (teoría de 5) | **Sí.** Detecta cualquier pista (propia o ajena) y cualquier enganche de Idle por la contabilidad. |
| 3b | **Sí** contra la pista ajena (RED). No distinguiría una pista **propia** de la contabilidad, porque solo saldría `InvalidateNavigationCaches`, pero eso lo detecta 3a. |
| 3c (lector doble) | **Sí** (RED). Reproduce lo que el núcleo puede reproducir de F4-S2-01: el evento llega **dentro** de la lectura del panel, por la entrada real del núcleo, con el host real; deja pista y el drenaje marca el borrador recién creado. El control con `Drawing` marca (`:162-168`). **No** ejerce `SubjectOf` ni la jerarquía `AssocNetwork : AssocAction`, que necesitan AutoCAD: lo protegen solo la guarda 5 y diag2 (que validó en D1 las mismas pruebas `is`, con el nombre completo de los tipos). |
| Fijación del olvido | **Sí.** Sin el olvido en la rama de contabilidad, `LastIntact` sería verdadero. Pasa en el RED por el olvido de `OnSourceEvent`, como declara D-C1-09. |
| Fijación «no objeto con cualquier sujeto» | **Sí.** Detecta que se quite `IsObjectSource`, porque `CommandEnded` con contabilidad dejaría de marcar. |
| Fijación de ámbito, reposo y contadores | **Sí**, por reflexión sobre `_states`. |
| 5 (texto del puente) | No puede estar en rojo, porque el paquete pone la forma en el RED. Sus negativas sobre el texto real cubren todo lo que defiende: `:164-168`, `:172-182`, `:199-202`, `:214-223`, `:232-234`; cada `Mutate` exige antes que el fragmento exista. |
| «Sin `SyncIdle`» (INV-C1-02) | No tiene prueba discriminante, y en la práctica no se puede observar: sin pistas pendientes, `SyncIdle` no hace nada. Lo cumple el código (`:383-385`). No lo cuento como hallazgo. |

**Guardas de texto con negativa real:** todas las nuevas mutan el texto real leído del repositorio, y las de los cambios 1 y 5 operan sobre la superficie y el cuerpo reales.

## 10. Defectos lógicos (punto 8)

- **Falso limpio:** ninguno fuera del residual 1 de IC-10 (§5c).
- **Pista perdida:** ninguna. Los eventos `Drawing` y todos los finales de orden conservan su pista. La rama de contabilidad no desengancha Idle ni descarta pistas pendientes.
- **Secuencia de cierre que deja de olvidarse:** ninguna (`:379`).
- **Excepción no contenida:** ninguna (§5).
- **Guarda esquivada:**
  - G-1 (`WorkspaceIdleSchedulingGuardTests.cs`) solo mira los bloques `case` y las líneas `handler =`, no `SubjectOf`. La guarda nueva lo cubre, y es más estricta.
  - `ForbiddenFieldLines` y `PublicSurfaceLeaks` (`WorkspaceBridgeSourceGuardTests.cs:209-230`) no aplican a un método privado.
  - El literal de `WorkspaceIdleSchedulingGuardTests.cs:129` sigue en `:195`, `:215`, `:222` y `:229`.
- **Contención (INV-C1-11):** las pruebas nuevas son de memoria y de lectura de fuentes. No escriben archivos, no usan configuración real y no crean modal ni dispatcher. Ninguna carga el Plugin ni AutoCAD.

## 11. OWNER

**Ninguna decisión material nueva.**
- La lectura de FRZ:216-218 como «tipo en tiempo de ejecución del objeto del evento» y la sustitución de T4CP:887 para los cinco manejadores ya están declaradas en H-10, D-C1-02, D-C1-06 e IC-10, con «se comunica al Owner».
- El código no va más allá de lo declarado.

## 12. Lo no verificado

- **Pruebas y builds:** no ejecuté ninguna prueba ni ningún build, por regla.
  - Me baso en los TRX de CI custodiados (hashes comprobados frente a `relay-record.json`) y en las conclusiones de los jobs del registro.
  - No consulté GitHub para confirmar las corridas 43799572094 y 43801607010.
- **Compilación del Plugin en el RED:** el job estuvo en `skipped`. La entrega declara un build Debug local del Plugin en el RED (`rp`), y no lo comprobé.
- **Avisos del build Release** («82 avisos, sin nuevos»): cifra de la entrega, no comprobada.
- **Comportamiento en AutoCAD:**
  - el rótulo con este build, y no solo con diag2;
  - la jerarquía `AssocNetwork : AssocAction` en el runtime real;
  - que `e.DBObject` no lance en otros dibujos.

  Solo D1 está medido, y la validación física es la reanudación de Smoke-2.
- **Coste de crear el envoltorio de `e.DBObject` en cada evento de objeto** (unos 20 000 en un Actualizar de D1): no se midió con un build sin trazas. La cota de unos 14 s de diag2 ya incluía esa lectura.
- **Corridas locales de la carpeta `tests/` de la corrida:** no las abrí. Usé la CI.
