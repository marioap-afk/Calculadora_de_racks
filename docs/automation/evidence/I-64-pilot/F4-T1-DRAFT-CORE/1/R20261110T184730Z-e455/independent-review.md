# Revisión adversarial independiente de F4-T1-DRAFT-CORE, intento 1

- **Revisor:** subagente independiente (modelo Opus), sin conclusiones previas, de solo lectura.
- **Encargo:** `independent-review-prompt.md`, junto a este archivo.
- **Diff revisado:** `13775d8f..aefc34a5`, con el RED `a362744c`.
- **Rutas sustituidas:** las rutas del directorio temporal local del revisor figuran como `<directorio-temporal>`.
- **Resto del texto:** sin cambios.

---

Revisión adversarial de F4-T1-DRAFT-CORE (13775d8f..aefc34a5, RED a362744c), en texto plano.

Veredicto: no encontré ningún bloqueante. El diff cumple el alcance y la letra de casi todos los invariantes. Hay 4 hallazgos importantes y 10 menores.

Qué verifiqué:
- Alcance: 10 archivos añadidos, exactamente los del AllowedWriteScope. Ningún archivo existente cambia. El GREEN solo toca los 5 de src, así que ninguna prueba del RED se debilitó ni se borró.
- GREEN: exporté el SHA con git archive a <directorio-temporal>/f4g y corrí el filtro FullyQualifiedName~RackCad.Tests.Workspace: 1165 superadas, 0 errores (el mínimo era 902).
- RED: exportado a <directorio-temporal>/f4r. De las 263 pruebas WorkspaceDraft*, 181 fallan y todas por Assert.*, sin excepciones; 82 pasan, casi todas guardas estructurales.
- Escenarios: los reproduje con un programa contra el núcleo GREEN, <directorio-temporal>/f4probe/Program.cs. Abajo cito su salida.
- El worktree no cambió: status limpio, HEAD = aefc34a5.

1. INCUMPLIMIENTOS DE INVARIANTES O DE D-10, D-12, D-13 y D-17

I-1 — IMPORTANTE — INV-F4T1-01, 02 y 13; D-10 (estado mutable compartido)
- Dónde: src/RackCad.Application/Workspace/Drafts/DraftSession.cs:237-238 (nacimiento), :374 (DiscardAndReload) y :478 (Applied).
- Qué pasa: en esos tres sitios CurrentDraftState y InitialDraftState pasan a ser la MISMA referencia. El contrato dice que TState es el estado editable que ya existe en Application. Para el piloto eso es PushBackEditorState o PushBackCompositeEditorState, y son clases mutables con mutadores en sitio (SetFrontCount, ToggleCell, CommitEditorValues, propiedades con set). Si el adaptador edita en sitio, también cambia la referencia de dirty: el comparador ve siempre igualdad, el borrador nunca queda Dirty y el cierre no se veta. Habría pérdida silenciosa, contra D-10, D-17 e INV-20.
- Escenario reproducido (f4probe, caso 1): con un TState mutable y d.CurrentDraftState.W = 45, la salida es "same=True initial.W=45 IsDirty=False veto=False". Tras DiscardAndReload y otra edición en sitio, sigue "IsDirty=False".
- Agravante: las pruebas fijan la identidad de referencia. WorkspaceDraftSessionTests.cs:173 y :639, y WorkspaceDraftApplyTests.cs:272 y :294 usan Assert.Same(Initial, Current).
- Corrección mínima: inyectar un clonador (Func<TState,TState>) que se use al nacer, al recargar y en Applied, para que inicial y actual nunca compartan objeto. D-10 deja la mecánica libre, así que cabe. Alternativa: declarar y probar que TState se trata como inmutable. En las pruebas, cambiar Assert.Same(Initial, Current) por igualdad del comparador.

I-2 — IMPORTANTE — INV-F4T1-09; D-13 ("No puede haber un Actualizar mientras otro está en curso")
- Dónde: DraftSession.cs:530 (EnterError libera la ranura) y :538-547 (Detach); DraftRegistry.cs:157, :170 y :186; lo fija la prueba WorkspaceDraftApplyTests.cs:121-140.
- Qué pasa: EnterError, Discard, DiscardAll y RetireSession liberan la ranura Aplicando mientras la operación del host sigue en vuelo. Después CompleteApply rechaza el resultado (Accepted=false). Así hay dos Actualizar a la vez, y el resultado de D-13 se pierde: un CommitThrew ya no deja Desconocido.
- Escenario (caso 2): se ejecuta A.TryBeginApply y luego A.EnterError("surface threw"). B.TryBeginApply devuelve true. A.CompleteApply(CommitThrew) devuelve accepted=False, A queda en Error y UnknownOrigin queda vacío.
- Es realista con C-F4-02: entre TryBeginApply y la ejecución en contexto de comando la interfaz sigue viva, así que un fallo de superficie o un descarte puede colarse ahí.
- Corrección mínima: la ranura pertenece a la operación, no al estado del borrador. Debe liberarse solo en CompleteApply, que tiene que aceptar el resultado aunque el borrador esté en Error o desligado. Otra opción: EnterError, Discard y Retire durante Aplicando difieren o rechazan.

I-3 — IMPORTANTE — INV-F4T1-06; D-12 (solo conflictos falsos, nunca aceptaciones falsas; sin normalización; orden de iteración)
- Dónde: DraftStaleBase.cs:74-86, sobre todo :81-82; lo fija la prueba WorkspaceDraftStaleBaseTests.cs:93-103.
- Qué pasa: una clave repetida se resuelve como "gana la última", y un NotMember posterior borra un miembro anterior. El resultado depende del orden de enumeración y puede dar un MATCH falso.
- Escenario (caso 3): base [D1 Mutable "{a}"] y relectura [D1 "{b external}", D1 "{a}"] dan MATCH=True: Actualizar sobrescribiría el cambio externo. Además, [Mutable, NotMember] da NotMember y [NotMember, Mutable] da Mutable.
- Corrección mínima: una clave repetida con hechos distintos es error de programación (ArgumentException) o una diferencia que nunca coincide. Ajustar la prueba.

I-4 — IMPORTANTE — INV-F4T1-16 e INV-F4T1-18 (familias que el contrato nombra literalmente y la guarda nueva no detecta)
- Dónde: WorkspaceDraftBoundaryTests.cs:246 y :286-297 (estado estático), :151-152 (`\bBom`) y :191 (`Bom\b`).
- Estado estático mutable: tanto la regex como la reflexión aceptan "private static readonly List<string> Log = new();" o un Dictionary estático readonly (IsInitOnly = true). Probado con la regex: devuelve False.
- Bom: un miembro "DraftBomLines" no lo detecta ni la guarda de código ni la de nombres. Probado: False en las dos.
- El código actual cumple; el defecto es que la guarda no detectaría una violación real.
- Corrección mínima: rechazar campos estáticos cuyo tipo no sea inmutable (colecciones y clases no selladas-inmutables) y añadir control negativo. Buscar "Bom" con distinción de mayúsculas, sin `\b`, y añadir controles negativos "DraftBomLines" y "static readonly List<>".

M-1 — MENOR — INV-F4T1-11 frente a INV-F4T1-14
- Dónde: DraftSession.cs:525-534.
- Qué pasa: EnterError admite cualquier estado de origen.
  - Desde Conflicto es una tercera salida de Conflicto, y además quita la opción KeepMine. Caso 7: "Conflict->Error; keepMine=NotAllowed".
  - Desde Desconocido deja UnknownOrigin=CommitThrew con Status=Error. Caso 10.
- Corrección: decidir y documentar la precedencia. O bien Error conserva el origen y permite KeepMine si venía de Conflicto, o bien se limpia UnknownOrigin al entrar en Error.

M-2 — MENOR — INV-F4T1-06 ("texto crudo SOLO de cada sobre miembro interpretable")
- Dónde: DraftStaleBase.cs:81-82.
- Qué pasa: una entrada Blocking conserva y compara EnvelopeText. Caso 8: "blocking text kept={garbage1 match=False".
- Corrección: descartar el texto también para Blocking.

2. PRUEBAS QUE NO PRUEBAN LO QUE DICEN

M-3 — MENOR — INV-F4T1-03 e INV-F4T1-18
- ThereIsNoPublicPathFromAPreviewOrFromABaseAloneToADraft (WorkspaceDraftRegistryTests.cs:95-114) pasa en el RED. Además excluye "Admitted" (:110), y DraftIntentRead.Admitted(DraftStaleBase.Create(...), estado) sí lleva de una base a un borrador.
- RemovingAFondoOutsideTheDraft... (WorkspaceDraftStaleBaseTests.cs:189) compara la base consigo misma, así que es tautológica.
- ReferenceCountsAreNotPartOfTheBaseNorOfItsInput (WorkspaceDraftStaleBaseTests.cs:200-214) compara dos bases idénticas y pasó en el RED. El escenario mínimo de INV-18 "recuentos de referencias que cambian" no fue un RED que fallara por aserción.
- Corrección: renombrar o ajustar estas pruebas, y dejar a T2 una guarda de fuente que limite quién llama a Admitted y a Create.

M-4 — MENOR — INV-F4T1-16 e INV-F4T1-18
- La guarda tampoco detecta:
  - ValueTask (tareas);
  - lock, Monitor, Interlocked (hilos);
  - PeriodicTimer (temporizadores);
  - Console, Trace, Debug (E/S y registro);
  - un "Database" pegado a otra palabra, por ejemplo "HostDatabase";
  - "Commit ()" con espacio.
  Todas probadas con la regex real: False. Están en WorkspaceDraftBoundaryTests.cs:107-108 y :243-244.
- La guarda "solo SessionId del namespace padre" (:301-322) no tiene control negativo.
- Corrección: ampliar las regex y añadir un caso inyectado por familia.

Fuera de esto, la cobertura mínima de INV-18 está presente, y las 181 fallas del RED son de aserción.

3. ESTADOS ALCANZABLES INVÁLIDOS

M-5 — MENOR — INV-F4T1-15 (estado mutable compartido)
- Dónde: DraftApplyOutcome.cs:68-72 y :91-96; DraftSession.cs:448 y :480.
- Qué pasa: ConflictDifferences y Warnings guardan la lista del llamador por referencia. Caso 4: si el llamador vacía su lista, queda "status=Conflict conflictDifferences=0".
- Además, los Warnings de un Actualizar anterior sobreviven a uno fallido posterior. Caso 5: "status=Dirty lastCause=builder failed warnings=[old warning]".
- Corrección: copia defensiva (ToArray) y limpiar Warnings en TryBeginApply.

M-6 — MENOR — INV-F4T1-12, 13 y 15
- Dónde: DraftRegistry.cs:180-185; DraftClosePolicy.cs:46.
- Qué pasa: RetireSession marca la sesión como retirada antes de evaluar el comparador inyectado. Si el comparador lanza, la sesión queda a medio retirar con borradores vivos, y EvaluateClose también lanza en lugar de vetar. Caso 6: "isRetired=True countOf=1 draftRegistered=True; EvaluateClose threw".
- Corrección: calcular primero y marcar después; que la decisión de veto, ante una excepción, se incline a Veto.

M-7 — MENOR — INV-F4T1-08
- Dónde: DraftSession.cs:266.
- Qué pasa: NeedsLazyReload es true en un borrador ya desligado. Caso 9: "registered=False needsLazyReload=True".
- Corrección: añadir la condición _owner != null.

M-8 — MENOR — orden de iteración
- Dónde: DraftSession.cs:253 y :332.
- Qué pasa: PendingText y la instantánea que recibe Apply se documentan como "ordenados", pero son un Dictionary, cuyo orden de enumeración no está garantizado. ADR-0032 D6 fija un orden de aplicación.
- Corrección: exponer una vista ordenada (SortedDictionary de solo lectura) o corregir la documentación.

M-9 — MENOR — INV-F4T1-07
- Dónde: DraftStaleBase.cs:89.
- Qué pasa: Entries expone el EnvelopeText crudo en público. Abre una vía para que T2 materialice estado editable desde la StaleBase.
- Corrección: hacerlo internal, o fijar en T2 una guarda contra ese uso.

4. ALCANCE

No encontré incumplimientos. Solo están las 10 rutas permitidas, ningún archivo existente cambia, y no hay conceptos de host, UI, persistencia ni mutación más allá de los nombres de frontera que exige INV-05.

M-10 — MENOR — riesgo de proceso para la tarea siguiente
- Dónde: WorkspaceDraftBoundaryTests.cs:378-388.
- Qué pasa: TheCoreFilesAreTheOnlyDraftFilesAndNoExistingWorkspaceFileMentionsThem prohíbe cualquier consumidor del núcleo en src. T2 tendrá que debilitar esta guarda de T1.
- Corrección: marcarla como guarda solo de T1, o acotarla ya a lo que no debe consumir el núcleo.

RECUENTO POR CLASE: BLOQUEANTE 0, IMPORTANTE 4 (I-1 a I-4), MENOR 10 (M-1 a M-10).
