# Revisión adversarial independiente — I-64 F4-T2B2B-LIFECYCLE

- **Rango:** `32369c7b..8f249e3a` (RED `a2e0ec13`, GREEN `8f249e3a`), 12 archivos.
- **Autoridades leídas:** `gate-contract.json` (INV-F4T2B2B-01..13, AllowedWriteScope, StopConditions); `owner-f4t2b1-t2b2-disposition.md` (distinción de D-T2B2-10); paquete v8, §3, §5, §6 y §11; IC-08; `task-close-package.md` de T2B2A y `owner-f4t2b2a-close-decision.md` (N-1 y N-4); F4-E33 y F4-E34; la re-revisión de T2B2A (`independent-rereview.md`) para calibrar las severidades de I-1.
- **Método:** análisis estático con `git show`, `git diff` y `git grep`. **No compilé ni ejecuté pruebas.** Las guardas de texto las evalué a mano contra el texto del GREEN, y todas deberían pasar.

## Veredicto

**ACEPTABLE CON MENORES.** Recuento: **0 BLOQUEANTE, 0 IMPORTANTE, 5 MENOR.**

- **Producto:** no encontré defectos de corrección.
  - La tabla de veto del Owner se cumple en sus cinco filas.
  - El descarte explícito y las dos retiradas no dependen del comparador.
  - Ningún camino de este código pierde en silencio un borrador que podría estar sucio, salvo el residual declarado y `Stop()`, también declarado.
  - Ninguno deja un borrador imposible de borrar.
  - `DraftRegistry` no se debilita.
- **Los cinco menores** son de pruebas, guardas y documentación:
  - dos son de la regla de I-1, en la misma clase que N-5 de T2B2A: T3 o T4 pueden declarar el cambio de guarda, así que no es un bloqueo cierto;
  - uno fija como contrato el residual de pérdida silenciosa;
  - uno es de documentación del enum compartido;
  - uno es de huecos de cobertura.

---

## Hallazgos

### M-1. MENOR (regla de I-1): la guarda del controlador fija el argumento del asesor y prohíbe envolverlo

- **Archivo y línea:** `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs`.
  - `:277-287`, `TheControllerWiresTheHostAsTheAdvisorOnceInItsConstructor…`. En `:283` está `AssertInOrder(constructor, "_host = new WorkspaceHostCore(", "_bridge.SetLifecycleAdvisor(_host);")`, junto con el conteo 1 de `:282`.
  - `:237-239`: el texto exacto, sin espacios, del reenvío del puente, `internalvoidSetLifecycleAdvisor(IWorkspaceLifecycleAdvisoradvisor)=>_core.SetLifecycleAdvisor(advisor);`.
- **Escenario:**
  1. F4-T3 cumple la obligación 15 como prevé el paquete, envolviendo el asesor para vaciar el texto vivo antes del consejo: `_bridge.SetLifecycleAdvisor(new LiveTextFlushingAdvisor(_host, …));` en el constructor de `WorkspaceController`.
  2. `IndexOf("_bridge.SetLifecycleAdvisor(_host);")` da -1 y la guarda falla.
  3. Envolver dentro del reenvío del puente también rompe `:237-239`.
  4. Poner el vaciado dentro del cuerpo de `AdviseClose` del host choca con `AdviseCloseForbidden` (`:311-312`, que prohíbe `_drafts`, `_bridge`, `_reader`…) si el vaciado toca los borradores.
  5. Quedan dos rodeos: un campo de devolución de llamada en el host, o una segunda llamada a `SetLifecycleAdvisor` desde otro archivo del Plugin. Ninguno de los dos es el diseño que nombra el paquete.
- **Qué incumple:**
  - INV-F4T2B2B-11: «ninguna … guarda nueva prohíbe de forma permanente … el gancho o vaciado del texto vivo antes del consejo de cierre (**que puede envolver el asesor**)»;
  - D-T2B2-10: «Ninguna guarda fija quién lo llama; F4-T3 podrá envolverlo».
  - La cabecera del propio archivo (`:16-19`) afirma que el envoltorio no está prohibido, y eso es inexacto.
- **Por qué es MENOR:** es el mismo tipo que N-5 de T2B2A. T3 tiene contrato propio y puede declarar el cambio de guarda, y existe al menos un rodeo, así que no es un bloqueo cierto. Es el más directo de los conflictos de I-1 de esta entrega.
- **Arreglo:** fijar solo el orden `_host = new WorkspaceHostCore(` → `_bridge.SetLifecycleAdvisor(` y el conteo 1, sin el argumento. En el puente basta con exigir que el método `internal` llame a `_core.SetLifecycleAdvisor(`.

### M-2. MENOR (regla de I-1 / F4-E33): conjuntos y conteos exactos de archivo entero donde bastaba una cota o el cuerpo propio

Todos en `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs`:

1. **`:386`**, en la prueba `TheMarkIsRemovedByTheNineOperationsOfThePackageAndByNothingElse…`: `Assert.Equal(9, Src.Count(host, @"_knownClean\.Remove\s*\("))` sobre todo `WorkspaceHostCore.cs`.
   - Quitar la marca de más siempre es conservador: solo lleva a `RetireConservative`.
   - El conteo exacto prohíbe que el «descarte de un solo borrador por el host» de T3 (que está en la lista de INV-11) quite la marca con el campo, por analogía con `DiscardPilotEdits`.
   - Las comprobaciones por cuerpo de `:388-395` y de las demás guardas ya fijan lo que esta tarea necesita.
2. **`:459`**: `Assert.Equal(1, Src.Count(Host(), @"\.DiscardAndReload\s*\("))` sobre todo el host.
   - La resolución de Conflicto, Error o Desconocido por PostReread con «descartar y recargar» (D-13; `DraftSession`: «It leaves Error only through DiscardAndReload or Discard») vivirá previsiblemente en el host como otra operación de T3 o T4, y rompe esta guarda.
   - Deshace la disciplina que la corrección de T2B2A adoptó precisamente por I-1 (`WorkspaceEditIntentGuardTests.cs:648-649`: «these counts are about the two operations of this task, not about the whole file»).
   - `:455-458` ya acota la recarga por cuerpo.
3. **`:71`**: `Assert.Equal(2, Src.Count(code, @"\)\s*;"))` en `IWorkspaceLifecycleAdvisor.cs`. Fija exactamente dos métodos, pero el arrastre a T3 «los RackIds del veto para el mensaje de D-17 (CloseDecision es Allow/Veto)» puede necesitar que el asesor los transporte.
4. **`:368-370`**: fija el texto exacto `private readonly Queue<DraftLossNotice> _lossNotices` y el accesor `internal`, y prohíbe `public … LossNotices`. «El acceso público a la cola de avisos» es un arrastre declarado a T3.

- **Qué incumple:** INV-F4T2B2B-11 («las guardas fijan solo lo que esta tarea necesita»; prefijos o cotas en lugar de conjuntos exactos, F4-E33).
- **Por qué es MENOR:** el sucesor puede declarar los cambios y hay rodeos (`NotePilotEdit(session)` en lugar de `_knownClean.Remove`, u otro nombre para la cola pública).

### M-3. MENOR: una prueba fija como comportamiento exigido la pérdida silenciosa del residual

- **Archivo y línea:** `tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs:786-806`, `AnInPlaceEditAfterTheAllowThatIsNotNotedIsTheExactResidualOfTheDesign…`, con `Assert.Equal("silent", Run(noted: false));`.
- **Escenario:** si, tras Smoke-2, el Owner decide endurecer la retirada conocida segura (la regla de devolución de D-T2B2-10 lo prevé), o si T3 o T4 cierran el residual, esta prueba falla. No falla por una regresión, sino porque exige que la pérdida sea silenciosa.
- **Qué incumple:** el residual está declarado, pero la orden del Owner dice que el fallo «must NOT permit silent loss of potentially dirty state», y la prueba convierte ese residual en contrato. Roza I-1 (fija para siempre algo que una tarea posterior puede querer cambiar).
- **Arreglo:** conservar la rama `noted: true` (la nota hace la retirada conservadora y la pérdida se informa) y, para `noted: false`, afirmar solo la vía tomada (conocida segura, sin llamar al comparador), no que «silencio» sea el resultado esperado.

### M-4. MENOR (documentación): la documentación compartida de `EditIntentStatus` es falsa para dos salidas de la recarga

- **Archivo y línea:**
  - `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:82`: `Waiting`, «… Nothing was read.»;
  - `:97`: `NotAdmitted`, «… no draft exists …».
- **Escenario:**
  - **Waiting:** en `WorkspaceHostCore.cs:488-489`, `DraftReloadStatus.NotAllowed` se traduce a `Waiting` **después** de una lectura completa. Pasa cuando el borrador pasa a Aplicando o se desregistra durante la lectura; la prueba `:1213-1228` lo ejercita.
  - **NotAdmitted:** en `:482-487`, el `NotAdmitted` de la recarga **conserva** el borrador existente y su marca, como prueba `:1197-1211`.
  - F4-T3 construirá los textos de D-17 y de la indicación de RACKEDITAR desde estos estados.
- **Qué incumple:** la traducción la manda el contrato (INV-06), así que no es un error de producto. Pero el enum es compartido y su documentación pasa a ser falsa para la recarga, el mismo tipo de defecto que N-4 de T2B2A.
- **Arreglo:** documentar que, en la recarga, `Waiting` puede venir después de la lectura y que `NotAdmitted` deja el borrador intacto.

### M-5. MENOR (cobertura): dos aserciones que el contrato o el nombre de la prueba prometen y no se hacen

1. **INV-06** pide que el `NotAdmitted` de la recarga lleve «el diagnóstico de `DraftReloadResult.Admission` y **el detalle** y la indicación de la lectura».
   - La prueba `WorkspaceLifecycleHostTests.cs:1197-1211` afirma `Diagnostic` y `ClassicEditorHint`, pero ni `Detail` ni `Witness`.
   - La guarda `WorkspaceLifecycleGuardTests.cs:470` solo exige `return EditIntentOutcome.NotAdmitted(`.
   - Un mutante que pase `null` como detalle (o `EditWitnessStatus.NotEvaluated` como testigo) supera todas las pruebas y guardas.
2. **`AnExceptionOfTheDiscardIsContainedAsFailedAndDoesNotDegradeTheSession`** (`:1422-1441`) no comprueba la no degradación que dice su nombre. Su core ni siquiera se arranca. Solo la cubre la guarda de texto `:498` (`_faulted|Contain(`).

---

## Comprobado y correcto

**Alcance y régimen:**
- Las 12 rutas del diff son exactamente el `AllowedWriteScope`; nada del `ForbiddenWriteScope`.
- El RED `a2e0ec13` contiene pruebas, un esqueleto inerte (sin lanzar y con valores no nulos) y los cambios declarados (1) y (3).
- El GREEN `8f249e3a` solo toca `src/` (6 archivos) y ningún archivo de `tests/`.
- Cambio (1): la lista y el comentario, con el nombre de la prueba conservado.
- Cambio (3): `Take(7)` y `default == Unavailable`.
- No cambia ninguna otra prueba ni guarda existente, ni ninguna prueba de F4-T1.
- `Plugin/Workspace` sigue con 9 archivos y `Drafts/` con 5.

**`DraftRegistry` (INV-02):**
- `Discard`, `DiscardAll` y `RetireSession` quedan intactos byte a byte; el constructor antiguo de `DraftLossNotice` se conserva (encadenado).
- `DiscardByUser`, `DiscardAllByUser` y `RetireKnownSafe` no tocan `IsDirty`, `Status` ni el comparador. Solo usan `HasPendingText`, que no llama al comparador.
- `RetireConservative`:
  - mira primero el texto pendiente;
  - tiene un `try/catch` por borrador, y un comparador que lanza en uno no contamina a los demás;
  - los perdidos contienen a los inciertos;
  - las listas son ordinales y nunca nulas;
  - los motivos son un `ReadOnlyDictionary` sobre un `SortedDictionary(Ordinal)`.
- Las dos retiradas marcan la sesión como retirada; los dos descartes no.
- Ninguna toca `_applying`, y `Detach` no libera el hueco.
- La validación es `IsNullOrEmpty`, igual que la de las ordinarias.

**`EventBridgeCore` (INV-01 y 04):**
- `SetLifecycleAdvisor` es el único miembro público nuevo, lanza `ArgumentNullException` con null y una segunda llamada sustituye al asesor. El constructor no cambia.
- `OnBeginDocumentClose`:
  - olvida;
  - sin asesor da Allow;
  - traduce la decisión del asesor, con cualquier valor que no sea Allow como Veto;
  - una excepción del asesor da Veto con `Log`, sin `Fault`;
  - solo recuerda un Allow.
- Hay 13 olvidos, cada uno el primero tras `if (!_started) return` y antes de cualquier rama.
  - Las consultas y `SetLifecycleAdvisor` no olvidan.
  - No hay recorrido de `_states`.
  - `OnDocumentToBeDestroyed` olvida también con un id nativo desconocido.
  - La degradación se lee antes de `_states.Remove`.
- El asesor recibe la destrucción después de `Destroy` y de limpiar `_active`, en su propio `try/catch` con `Log`, y antes de `TrySyncIdle`. El asesor del host no llama al puente.

**`WorkspaceHostCore` (INV-03 a 08):**
- Implementación explícita del asesor.
- `AdviseClose`:
  - quita la marca al entrar;
  - llama a `EvaluateClose`;
  - pone la marca como última acción y solo con Allow;
  - una excepción da `Report` y Veto;
  - no lee, no autoriza, no descarta y no retira.
- `SessionDestroyed`:
  - consume la marca;
  - usa `RetireKnownSafe` solo con la marca **y** la secuencia intacta;
  - contiene por separado la retirada y la cola con el registro;
  - solo encola y registra con `HasLoss`, con el contexto `WorkspaceHostCore.DraftLoss` y una `InvalidOperationException` con los perdidos y, aparte, los inciertos y su motivo.
- El marcado va dentro del `try`, antes de `PurgeDead` y de todo retorno temprano, y solo marca los borradores del piloto.
- La marca se quita en los nueve puntos del paquete.
- `NotePilotEdit` no tiene compuerta y no lanza.
- La recarga sigue los pasos 1 a 8 con `PassGate` y la traducción pedida, no degrada la sesión y no se llama desde el drenaje.
- `DiscardPilotEdits` solo exige `Authorize`, y el valor `default(EditDiscardOutcome)` es Rejected con listas no nulas.
- N-4: el comentario de `Rejected` nombra las tres vías.

**Tabla del Owner:** se cumplen las cinco filas, comprobadas en el código y en las pruebas.

**Las pruebas discriminan contra estas implementaciones plausibles e incorrectas:**
- retirar como limpio sin mirar `closeSequenceIntact` (`:706-710`);
- llamar al comparador al descartar (comparador que cuenta llamadas y comparador que lanza);
- no olvidar en alguna entrada (teoría de 15 entradas y la guarda `ForgetsBeforeAnyBranch`, con autoprueba);
- leer la degradación después de `Remove` (`:460-471`);
- un `catch` global en `RetireConservative` (prueba de un borrador que lanza y otro que no);
- no quitar la marca en un Veto (`:823-846`);
- ignorar la autorización en el descarte (`:772-778`).

Las pruebas que pasan contra el RED son de forma, negativas o autopruebas de guardas, y ninguna se presenta como roja en el RED.

**Plugin:**
- `e.Veto()` se llama si y solo si la decisión es Veto, y los tres literales siguen en la misma línea.
- Siguen 16 `+=`, 16 `-=` y 16 manejadores.
- El controlador cablea el asesor una sola vez tras crear el host, sin `+=` nuevos.

**Observaciones** (no son hallazgos):
- `SetLifecycleAdvisor` no olvida la secuencia, por contrato. Sustituir el asesor entre el Allow y la destrucción daría `intact = true` a un asesor que no evaluó; la marca, que es de cada host, lo acota.
- `_knownClean` puede guardar entradas de sesiones que nunca llegan a `SessionDestroyed`: un `AdviseClose` sobre una sesión muerta, o `Stop()`. Es una fuga mínima e inocua, porque los ids no se reutilizan.
- `EvaluateClose` (política de F4-T1) no veta un borrador que está en Aplicando y no está sucio. Es un punto para T4.

## Riesgos para el smoke con AutoCAD (no son defectos)

1. **Orden real entre el cierre y la destrucción.** ¿Hay entradas intermedias del puente en CLOSE, CLOSEALL, QUIT, la pestaña y la API (`CloseAndDiscard`/`CloseAndSave`)? Candidatas: `CommandEnded`/`Cancelled` de CLOSE, `ImpliedSelectionChanged`, `DocumentActivated` del siguiente documento, el `Idle` durante el diálogo de guardar, `ObjectModified` al guardar y `EnteringQuiescentState`. Si siempre hay alguna, la vía conocida segura no se usa nunca: es seguro, pero se aplica la regla de devolución al Owner por posibles falsos inciertos.
2. **Cierre cancelado** (Cancelar en el aviso de guardar, otro manejador que veta después del nuestro, QUIT vetado por otro documento): ¿qué entrada llega después? Mientras no llegue ninguna, el Allow recordado sigue vivo (residual declarado).
3. **`e.Veto()` que lanza o se ignora** (cierres por API, QUIT): `ContainSession` degrada la sesión, el veto se pierde y el documento se cierra. La retirada es conservadora y la pérdida queda registrada, pero la protección de D-17 falla. Hay que verificar que el veto cancela de verdad en cada vía.
4. **Documento vetado que no es el activo** (CLOSEALL o QUIT): `DiscardPilotEdits` exige que sea el activo (INV-22). T3 debe nombrar el documento.
5. **QUIT (S2-06):** ¿llegan `BeginDocumentClose` y `DocumentToBeDestroyed` para cada documento? Sin `BeginDocumentClose` no hay veto posible.
6. **`Stop()`/`Dispose()`:** destruye sesiones sin llamar al asesor, y deja borradores huérfanos y sin aviso (residual declarado). Hay que comprobar que nada llama a `Dispose` durante la vida de AutoCAD.
7. **Fallo al enganchar `BeginDocumentClose`:** la sesión queda degradada sin veto posible (residual declarado).
8. **Reentrada durante la lectura de la recarga.** Entre el paso 6 (`NeedsLazyReload`) y el 8 (`DiscardAndReload`) no se vuelve a comprobar. Si algo bombea mensajes mientras el lector tiene el documento bloqueado y T3 vuelca texto vivo en el borrador, `DiscardAndReload` lo descartaría. Hoy es inalcanzable; se arrastra a T3.
9. **Coste:** `AdviseClose` evalúa todos los borradores con el comparador en cada intento, y `RetireConservative` corre en cada destrucción con secuencia rota (Smoke-PERF).
10. **Vía de retirada:** sin pérdida no queda rastro, así que observarla depende del procedimiento de Smoke-2 (arrastre ya declarado).
