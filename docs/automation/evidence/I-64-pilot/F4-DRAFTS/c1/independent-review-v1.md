# I-64 F4-T4-HOST-C1 — Revisión independiente v1 del paquete del contrato

> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<worktree-de-diagnostico>`, `<carpeta-de-copias-de-Smoke-2>`, `<carpeta-del-DLL-de-Smoke-2>`, `<build-diag1>`, `<build-diag2>`, `<directorio-de-H0>`). El resto no cambia.


**Objeto:** `hs65/c1/I-64-f4-t4-host-c1-gate-contract-package.md` (v1).
**Código:** worktree `architecture-workspace-persistente-rackcad`, HEAD `d4a24790`; `git diff --stat b88dabf2 HEAD -- src tests` vacío, así que todas las líneas valen sobre `b88dabf2`.
**Método:** solo lectura. No se compiló ni se ejecutó nada; no hubo escrituras git. Se leyeron las trazas `<carpeta-de-copias-de-Smoke-2>\diag\trazas\rackcad-20261116.log.diag1` y `.diag2` (solo `grep`/`sed`).
**Abreviaturas:** `TW/` = `tests/RackCad.Tests/Workspace/`; `EBC` = `src/RackCad.Application/Workspace/EventBridgeCore.cs`; `BRIDGE` = `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`; `T4CP` = `docs/initiatives/I-64-f4-t4c-gate-contract-package.md`; `T2B2P` = `docs/initiatives/I-64-f4-t2b2-gate-contract-package.md`; `ICL` = `docs/initiatives/I-64-implementation-clarifications.md`; `D1T`/`D2T` = trazas diag1/diag2; `SP` = `scratchpad/hs65/`.

## Veredicto

**EMITIBLE TRAS CORRECCIONES** — **0 B, 4 I, 14 m.**

El diseño técnico de C1 y de C2 es correcto. Compila con lo que hay y respeta las guardas de pureza y de frontera. La lista de cambios de guarda de D-C1-03 es **completa y exacta**: no encontré ninguna otra guarda que rompa. Los cuatro importantes son de **argumentación y declaración** frente a lo ratificado (D5, D-T4C-09, el residual de IC-10 y T2B2B), no de código. Se corrigen con texto, sin rediseño.

---

## Importantes

### I-1. C2 no se clasifica frente a D5 «conflicto/stale», y D-C1-04 sí usa D5 para S2-03

- **Evidencia:**
  - el §1 del paquete cita D5 con «conflicto/stale» y «cambio del comportamiento ratificado de T4B o T4C»; `owner-f4t4c-decisions-d1-d6.md` (D5) lo dice literal;
  - C2 cambia qué eventos marcan los borradores, que es la categoría «stale» por su letra (`map.md:191` ya lo anotó);
  - D-C1-04 saca S2-03 por la categoría «Undo» de D5, pero ninguna decisión explica por qué C2 no es una parada C1-S05;
  - IC-10 solo dice «se comunica al Owner».
- **Por qué importa:** con D2 (0B/0I para cerrar sin el Owner), un revisor u Owner puede leer C2 como decisión material nueva de D5. La asimetría con D-C1-04 queda sin explicar.
- **Corrección:** añadir en D-C1-02 (o en una D-C1 nueva) la clasificación explícita:
  - (a) la orden literal del Owner nombra los errores de Smoke-2 y la lectura declarada lista F4-S2-01 (`SP/smoke2/owner-f4-smoke2-fix-order.md`);
  - (b) IC-08 es una aclaración de la sesión, «escalonada… se comunica al Owner» (`ICL:133`), así que su refinamiento va al mismo nivel;
  - (c) C2 restituye el resultado ratificado y esperado: tras Actualizar, «Limpio» sin la marca (IC-09; S2-14, `T4CP:1702`; registro de Smoke-2, paso 14 «esperado Limpio SIN la marca (IC-09)»);
  - (d) no cambia la semántica de ningún cambio de entidad ni de sobre (Xrecord y DBDictionary siguen contando).
  - Conclusión: no es D5; se anota al Owner (regla de la orden). Citar también `owner-f4e72-decision.md`, que ordena C1 en esta tarea (ver m-14).

### I-2. C2 contradice sin declararlo la frase ratificada de D-T4C-09 «los demás manejadores no cambian»

- **Evidencia:**
  - `T4CP:887`: «los demás manejadores no cambian: no leen handles ni el DWG. IC-04 queda intacta»;
  - C2 cambia los cinco manejadores de objeto (`BRIDGE:240-274`) y les hace leer `e.DBObject`;
  - el precedente que hace falta existe, `T4CP:866` («leer `e.GlobalCommandName` es leer un dato en memoria del argumento del evento, de coste constante»), pero el paquete no lo invoca;
  - `T4CP:904` descartó la «pista con handle» por el coste constante de FRZ:217-218;
  - H-10 da por hecho que «filtran por tipo» (FRZ:217) habla del tipo del objeto; la frase admite también la lectura «por tipo de evento o pista». Es una interpretación y hay que declararla.
- **Corrección:**
  - IC-10 (Autoridad) y D-C1-02 declaran que sustituyen, para los cinco manejadores de objeto, la frase de `T4CP:887`;
  - fijan la lectura: la prueba `is` sobre el tipo en tiempo de ejecución de `e.DBObject` es un dato en memoria del argumento, como `e.GlobalCommandName` (`T4CP:866`); no abre el objeto, no lee su handle, su id ni sus propiedades, y su coste es constante;
  - la guarda nueva del ítem 5 de D-C1-08 añade a lo prohibido en `SubjectOf` `Handle`, `ObjectId`, `\.Id\b` y `\.Database`, con su negativa.

### I-3. El residual de IC-10 está mal acotado: «toda orden marca por su `CommandEnded`» no lo cubre

- **Evidencia:**
  - es cierto para las órdenes: `CommandEnded`, `CommandCancelled` y `CommandFailed` encolan `HintKind.CommandEnded` ajeno (`EBC:330-345`), y eso marca (`WorkspaceSession.cs:66-68`);
  - pero el residual real no son las órdenes. Son los cambios **fuera de toda orden**: paletas no modales (Propiedades, Administrador de parámetros), otro complemento que escribe en reposo, reactores. Ahí no hay `CommandEnded`, y un cambio que solo toque objetos asociativos, por ejemplo un parámetro de usuario que nada usa, ya no marcaría;
  - además, el filtro no cubre todo el marco asociativo. `AssocActionBody` y `AssocDependencyBody` no derivan de `AssocAction` ni de `AssocDependency`: `AcDbMgd.xml` (NuGet `autocad.net.model` 25.0.0) documenta `AssocActionBody` como clase base abstracta propia. Siguen contando (es lo conservador). En un dibujo cuya evaluación toque cuerpos, por ejemplo con arreglos asociativos, F4-S2-01 puede seguir apareciendo;
  - solo D1 está medido: `D1T` antes del primer `EXECUTEFUNCTION` (`:1-105`) solo muestra `AssocNetwork h=1AC25E`.
- **Corrección:** reescribir el residual de IC-10:
  - (1) un cambio que solo toque objetos `AssocAction` o `AssocDependency` marca por el fin de su orden si ocurre dentro de una (también si se cancela o falla). Fuera de una orden no marca. Un cambio así no puede alterar un sobre de rack: Xrecord y DBDictionary siguen contando como cambio;
  - (2) el filtro se limita a esas dos jerarquías. Cuerpos y demás clases siguen contando, y solo D1 está medido.
  - Quitar «así que el residual queda cubierto».

### I-4. INV-C1-03 (seguir olvidando la secuencia de cierre) no está validado en AutoCAD, y las trazas muestran que en D1 rompe probablemente cada cierre

- **Evidencia:**
  - diag2, la build validada, filtraba **fuera** de `ContainSession` y antes del núcleo, así que los eventos asociativos **no** olvidaban `_lastAllowedClose` (`diag2.patch:154-186`; `map.md:74`). H-05 y H-08 validan el efecto sobre el marcado, no INV-C1-03;
  - las dos trazas acaban con un `ObjectModified` de `AssocNetwork` unos 150 ms **después** del `CommandEnded(QUIT)` y del `EnteringQuiescentState`: `D1T:72391-72393` y `D2T:39668-39670`. Es decir, plausiblemente dentro de la ventana `BeginDocumentClose` → `DocumentToBeDestroyed`, que no se trazó;
  - con INV-C1-03, en un dibujo con red asociativa la secuencia de cierre quedará rota casi siempre. Va a `RetireConservative`, con posibles «falsos inciertos» (`T2B2P:254-255`);
  - es justo el objeto de la regla de devolución de T2B2B: «si Smoke-2 mide que la secuencia nunca queda intacta en CLOSE o CLOSEALL… la sesión lo devuelve al Owner antes de cambiar el mecanismo» (`T2B2P:256`). Esa misma regla es la razón vinculante de INV-C1-03: el mecanismo no se puede cambiar sin el Owner.
- **Corrección:**
  - D-C1-02 y INV-C1-03 citan `T2B2P:256` y `T2B2P:268` («la olvida en toda entrada pública que muta estado del core») como razón, y no solo «conservar la observación»;
  - declaran los dos hechos de traza;
  - el §8 añade el punto de observación para el bloque de ciclo de vida de Smoke-2: qué vía de retirada se usa en CLOSE, CLOSEALL y QUIT sobre D1, con la regla de devolución de `T2B2P:256` si nunca queda intacta;
  - IC-10 lo repite en una línea.

---

## Menores

**m-1. H-01 es incompleto.**
- La regla del rótulo se alcanza también por `DynamicSystemFrontalBuilder.cs:171` (`AppendFrontal`), `DynamicSystemLateralBuilder.cs:257` y `DynamicSystemPlantaBuilder.cs:74`.
- Esos tres los usan `PushBackSystemFrontalBuilder`, `PushBackSystemPlantaBuilder`, `PushBackCompositePlanta` y el lateral no compuesto de `PushBackSystemLateralBuilder`.
- `PushBackSystemLateralBuilder.cs:213` es solo el camino compuesto (lado A), y los compuestos están fuera del piloto (`PushBackPilotMaterializer.cs:106`; `TW/WorkspacePilotUpdateRunTests.cs:863-864`).
- `map.md:27` («no lo usa ningún otro builder») es falso. La corrección no cambia; hay que corregir la cita.

**m-2. Citas de línea.**
- H-03: la prueba es `TW/WorkspacePilotUpdateRunTests.cs:893-904` y las tres excepciones están en `:901-903` (no en `:897-902`).
- H-04: `EnvelopeText` está en `PushBackPilotUpdatePlan.cs:165`, y la deserialización en `PushBackPilotUpdateRun.cs:195-200`.

**m-3. Falta el contrato de `BuildForMember`.**
- `RackEmbedStore.Deserialize` devuelve `null` con un sobre ilegible (`RackEmbedDocument.cs:97-121`). El archivo no tiene `#nullable`, así que el compilador no avisa, y `embed.Name` daría `NullReferenceException`.
- Hay que fijar:
  - `member` nulo da `ArgumentNullException`;
  - un sobre ilegible da `InvalidOperationException` con una razón constante, como `UnreadableDesignReason`, o deja `Name` nulo. Hay que decidir cuál;
  - el comportamiento con un nombre en blanco.
- La prueba 1 debe fijarlo, y fijar también «en crudo»: con un nombre con espacios alrededor, el `Name` reconstruido es idéntico. Hoy solo se prueba el caso en blanco.

**m-4. Paridad con RACKEDITAR.**
- RACKEDITAR aplica **un** nombre, el de la ventana o el del sobre elegido, a todas las vistas (`RackPushBackCommands.cs:187-190`). `BuildForMember` usa el sobre de **cada** miembro.
- Coinciden en un dibujo normal. Hay que declarar la diferencia con nombres divergentes entre vistas, que es coherente con `DesiredNamesOf` (`PushBackPilotUpdateRun.cs:197-202`).
- «En crudo» es la paridad correcta: el único consumidor, el rótulo, recorta (`DynamicViewDecorations.cs:91-94`).

**m-5. La forma de `OnObjectEvent` queda ambigua para las guardas.**
- Falta el tipo de retorno (`bool`, como `OnSourceEvent`); la entrada nueva de `ForgettingEntries` necesita la expresión regular exacta.
- «Primero olvida» debe decir «justo después de `if (!_started) return false;` y antes del `try`»:
  - lo exige `ForgetsBeforeAnyBranch`/`AfterTheStartedReturn` (`TW/WorkspaceLifecycleGuardTests.cs:145-164`);
  - un olvido literalmente primero, antes del retorno de no iniciado, hace fallar la guarda.
- Falta qué hace con una fuente que no es de objeto.
- Los cinco cambios de D-C1-03 deben darse con su texto exacto, incluida la negativa del ítem 1, por ejemplo `real.Where(n => n != "OnObjectEvent")`.

**m-6. «Total» no es exacto.**
- `e.DBObject` se evalúa en la llamada, fuera del `try/catch` de `SubjectOf`, y las pruebas `is` no lanzan, así que ese `catch` es código muerto.
- Un getter que lanzara degradaría la sesión por `ContainSession` (`BRIDGE:384-401`). Es aceptable, pero hay que decirlo: «contenido» en lugar de «total», o evaluar el getter dentro.

**m-7. El negativo de la prueba 2 sobre «el texto de `b88dabf2`».**
- Una prueba solo lee el árbol de trabajo. Hay que formularlo como mutación del texto real: `BuildForMember(member, _catalog)` → `Build(member.DesignText, _catalog)`, como hacen las autopruebas existentes.

**m-8. Detalles de factibilidad del RED (todas factibles).**
- **3c:**
  - usa `WorkspaceHostRig` tal cual (`TW/WorkspaceHostTestSupport.cs:132`; `Bridge.Core` es público), con `EditIntentKit` y `EditIntentFakeReader` (internos, `TW/WorkspaceEditIntentHostTests.cs:17`, `:35`), como `TW/WorkspaceLifecycleHostTests.cs:48-120`;
  - hay que decirlo y dejar `WorkspaceHostTestSupport.cs` en el prohibido;
  - y decir si la «lectura» se simula llamando a `OnObjectEvent` dentro del lector doble o alrededor de él.
- **4:** «no abre ni cierra el ámbito» necesita la reflexión sobre `_states`, como `TW/WorkspaceOwnMutationTests.cs:27-32`. Factible.
- **RED:** hay que decir si el `OnObjectEvent` inerte ya lleva el olvido. Eso decide si los cambios 2 a 5 de D-C1-03 fallan en el RED o pasan. Las dos cosas son admisibles, pero hay que declararlo.
- **Filtro:** las clases nuevas en `RackCad.Tests` con prefijo `Workspace` caen en `~RackCad.Tests.Workspace`. Correcto.

**m-9. El ForbiddenWriteScope no se puede «compactar».**
- El de T4C2 lleva los prefijos `src/RackCad.Plugin/` y `src/RackCad.Application/Systems/`, que cubren dos archivos permitidos.
- La comprobación de alcance es «diff ⊆ Allowed y ∩ Forbidden = ∅» (`nc2_exact_whitelist_substitute.py:15-26`).
- Hay que expandir esos prefijos archivo a archivo, como hizo `F4-T4C1-HOST/coordinator/gate-contract.json`.

**m-10. El argumento de D-C1-04 confunde dos formas de la corrección.**
- La forma validada (diag2) **no** toca `DrainOperation`. Pone `AcApplication.Idle += once` y `Editor.Regen()` en el puente (`diag2.patch:70-83`), y con eso rompe:
  - los censos 16/16/16;
  - INV-F1-T2-03 (`Regen` en `TW/WorkspaceBridgeSourceGuardTests.cs:139-140`);
  - la regla del callback de Idle de IC-04 (`ICL:81`).
- Es el diseño (b) del mapa (§3.4) el que añade una operación de drenaje.
- La conclusión se mantiene. Además falta un hecho: no está medido si RACKEDITAR deja la misma vista sin refrescar tras `U`. Si la deja, S2-03 es preexistente y no un defecto del piloto. Es un dato útil para el Owner.

**m-11. D-C1-05.**
- Los «unos 14 s» se midieron con diag2, que registraba cada evento de objeto que no es entidad (`diag2.patch:154-186`; `map.md:194`). Hay que calificarlos de cota superior.
- La orden de corrección lista el rendimiento dentro del alcance, así que debe figurar en «Qué no corrige», con la razón y anotado al Owner.

**m-12. Texto de IC-10.**
- Hay que decir que estos eventos tampoco invalidan las cachés de navegación, y por qué eso no choca con «no puede suprimir `InvalidateNavigationCaches`» (`ICL:150`): esa regla rige la mutación propia, que sigue invalidando por sus eventos de entidades y definiciones.
- La causa («reevalúa al bloquear, desbloquear y confirmar») es una inferencia a partir de la posición en la traza. Hay que marcarla así.

**m-13. La atribución de H-06 y H-07 tras «Actualizar» es inexacta.**
- Los eventos que marcan tras el Actualizar llegan entre `CommandEnded(EXECUTEFUNCTION)` y `EnteringQuiescentState`, **antes** del drenaje (`D1T:29623-29627`; `D2T:7027-7031`). No vienen de la relectura de presentación: tras el drenaje de `D2T:7031` no hay ningún evento asociativo hasta el `U` (`:7032`).
- Además, acompañan a todo fin de orden y a la selección: `RACKPANEL` (`D2T:1-14`), `ZOOM` (`D2T:82-94`) y la selección implícita (`D2T:15-22`).
- La prueba 3b ya modela el orden real, pero debe llamarse «la contabilidad del fin de la orden del Actualizar». El «Por qué» de IC-10 debe decir «al terminar cada orden y en cada bloqueo o confirmación, también en las lecturas del panel».

**m-14. Autoridades.**
- Falta `F4-DRAFTS/coordinator/owner-f4e72-decision.md` (literal: «cuando se vayan a corregir los problemas del smoke se corrige el defecto del nombre»), que autoriza C1.
- `F4-DRAFTS/coordinator/owner-f4-smoke2-fix-order.md` y `F4-DRAFTS/c1/map.md` no existen en `d4a24790`; hoy están en `SP/smoke2/`. Hay que custodiarlos en la emisión o citar su ubicación real.

---

## Comprobado sin hallazgo (con evidencia)

- **H-02, H-05 y H-12:**
  - `RackPushBackCommands.cs:187-190` asigna el nombre en crudo;
  - el escritor llama a `Build` en `RackPilotUpdateWriter.cs:118`, tras deserializar el sobre en `:115-117`;
  - la carga útil de `WriteUnit` compara el sobre, no el sistema (`:266-267`), así que el nombre no la afecta;
  - el literal de `TW/WorkspaceIdleSchedulingGuardTests.cs:129` sigue en `BRIDGE:195`, `:215`, `:222` y `:229`.
- **H-03, llamadores de `Build`:** solo `RackPilotUpdateWriter.cs:118`, `PushBackPilotUpdateRun.cs:214` y la prueba. Ninguna guarda fija la superficie de `PushBackPilotSystemRebuild` por reflexión.
- **C1 y las guardas:**
  - `RackEmbedStore` está permitido en `RebuildPath`: es `RackCad.Application.Persistence` y el `using` ya está en `:4`. Ningún token de `Impure` (`TW/WorkspacePilotUpdateGuardTests.cs:56-71`) ni `Transaction` (`:118-124`, `WorkspacePilotUpdateIntegrationGuardTests.cs:81`). Cuidado con no escribir `Database`, con mayúscula, en los comentarios;
  - G-9 (`:681-686`), `WriterStructureViolations` (`:198-226`), G-1 (`:289-311`) y G-7 (`:653-673`) no cambian;
  - los censos de sobres de `CustomPropertiesEnvelopeGuardTests` (construir o componer) no se tocan;
  - el nombre solo lo consumen los rótulos (grep de `system.Name` en `src/`).
- **D-C1-03 es completo.** Revisé todas las guardas que leen `BRIDGE`, `EBC` o `BridgeSource.cs`:
  - `WorkspaceBridgeSourceGuardTests` (manejadores contenidos `:183-189`, campos `:210-237`, superficie pública `:221-245`, llamadas prohibidas `:139-180`, persistencia `:295-305`, I63 `:370-378`);
  - `WorkspaceBridgeFacadeSourceGuardTests` (`:107-140`, `:206-249`);
  - `WorkspaceIdleSchedulingGuardTests` (G-1 `:64-104`, G-6 `:506-523`, G-7, G-8 `:597-618`, G-9 y G-10);
  - `WorkspaceHostC2SourceGuardTests` (`:62-90`, `:190-197`, `:205`);
  - `WorkspaceHostSourceGuardTests` (`:383-421`, `:467-477`, `:495-509`);
  - `WorkspaceLifecycleGuardTests` (`:104-214`, `:216-229`, `:273-289`, `:728-741`);
  - `WorkspacePaletteHostSourceGuardTests` (`:377-408`, `:774-782`);
  - `WorkspacePilotEditGuardTests` (`:737-770`, `:1150-1169`);
  - `WorkspacePilotUpdateIntegrationGuardTests` (G-6 `:590-628`, `:1031-1084`);
  - `WorkspaceEditIntentGuardTests` (`:1203-1224`);
  - `WorkspaceNavigationAuthorityTests` (`:130-171`), `WorkspaceNavigationHostTests` (`:640-652`) y `WorkspaceSourceGuardExceptionTests`;
  - la reflexión en `WorkspaceBridgeHintAndDrainTests.cs:271-296`.
  - Ninguna cuenta las llamadas a `_core.OnSourceEvent`, y ninguna prohíbe `e.DBObject`, `is`, `AssocAction` ni un método `private static` nuevo.
  - Las líneas de manejador siguen siendo una por fuente (16) y contienen `ContainSession(`. `.DBObject` no casa con `\.Database\b` (`TW/WorkspaceBridgeFacadeSourceGuardTests.cs:127-128`).
- **El nombre `ObjectEventSubject`:**
  - no casa con `Subscription|Editor|Draft` ni con `Tab` (`TW/WorkspaceModelBoundaryTests.cs:94-104`; `TW/WorkspaceBrowserBoundaryTests.cs:285-293`);
  - no casa con los conceptos de I-63 (`TW/WorkspaceModelBoundaryTests.cs:81-91`, `TW/WorkspaceBridgeSourceGuardTests.cs:370-378`);
  - no aparece en los archivos del navegador ni de `Drafts/` (`TW/WorkspaceBrowserBoundaryTests.cs:83-100`; `TW/WorkspaceDraftBoundaryTests.cs:421-438`);
  - ninguna guarda de las pruebas de UI lee `Application/Workspace`;
  - `BridgeSource.cs` solo se lee en `TW/WorkspaceNavigationAuthorityTests.cs:169-172`, para otros nombres.
- **Los tipos existen:** `Autodesk.AutoCAD.DatabaseServices.AssocAction`, `AssocDependency` y `AssocNetwork` están en `AcDbMgd.dll` y `AcDbMgd.xml` del NuGet `autocad.net.model` 25.0.0, que es la referencia de CI (`RackCad.Plugin.csproj:11-16`), y en la instalación local. diag2 los usó y compiló (`diag2.patch:44-45`). `BRIDGE:6` ya importa el espacio de nombres.
- **El olvido en `OnSourceEvent`:** `if (!_started) return false;` y luego `_lastAllowedClose = null;`, antes del `try` y de las comprobaciones de sesión (`EBC:314-319`).
- **El alcance:** no hacen falta ni `IHostEventSource`, ni los dobles, ni el enlace, ni el controlador, ni `WorkspaceHostTestSupport.cs`. Todo lo listado hace falta.
- **D-C1-04 frente al Freeze:**
  - FRZ:227-232 («y solo estas»);
  - `Hints.cs:14-21`;
  - `T4CP:845`;
  - FRZ:237-238;
  - `Regen` como escritura de vista (`TW/WorkspaceBridgeSourceGuardTests.cs:137-142`; `TW/WorkspaceHostSourceGuardTests.cs:383-396`);
  - «Undo» en la lista de D5.
  - Correcto, salvo la forma del argumento (m-10).
- **El RED:**
  - 1 y 2 fallan por aserción con el cuerpo inerte y el escritor de `b88dabf2`: el sobre de los kits lleva `Name` «PB-01» (`tests/RackCad.Tests/Fixtures/I64/pb-01-base.json`), y `IntentKit.Variant` permite `name: ""`;
  - 3a, 3b, 3c y 5 fallan;
  - las fijaciones de 4 pasan con la delegación inerte.
  - Ninguna necesita AutoCAD.
