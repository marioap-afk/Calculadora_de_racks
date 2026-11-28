# Revisión adversarial independiente de F4-T1-DRAFT-CORE, intento 2

- **Revisor:** subagente independiente (modelo Opus), sin conclusiones previas, de solo lectura.
- **Encargo:** `independent-review-prompt.md`, junto a este archivo.
- **Diff revisado:** `90a81104..09955667`, con el RED `500a336d`.
- **Rutas sustituidas:** las del directorio temporal local del revisor figuran como `<directorio-temporal>`.
- **Resto del texto:** sin cambios.

---

Revisión adversarial independiente de I-64 F4-T1-DRAFT-CORE, corrección del intento 2 (diff 90a81104..09955667, RED 500a336d).

Veredicto: la corrección resuelve los 13 hallazgos CORREGIR en su caso original. Sin embargo, introduce 1 BLOQUEANTE: en la guarda, la regla de "Bom" quedó más débil que antes, y eso contradice el texto de INV-F4T1-22. Hay además 1 IMPORTANTE (un alias del estado inicial a través de la lectura) y 8 MENORES. El producto cumple el alcance.

Qué hice
- Exporté con git archive tres SHA a directorios temporales fuera del worktree: 09955667 en <directorio-temporal>/f4rv-g, 500a336d en <directorio-temporal>/f4rv-r y 90a81104 en <directorio-temporal>/f4rv-o. Los dejé sin borrar.
- Filtro FullyQualifiedName~RackCad.Tests.Workspace:
  - sobre el GREEN: 1273 superadas y 0 fallos (el mínimo es 1165);
  - sobre el RED: 52 fallos, todos por Assert.*, ninguno por excepción.
- Escribí un programa de sondeo que compila los cinco archivos de Drafts más SessionId.cs y usa solo la API pública: <directorio-temporal>/f4rv-probe/Program.cs. Lo cito como "sonda".
- El worktree sigue limpio, con HEAD = 09955667. El checkout principal también está limpio.

1. LOS HALLAZGOS CORREGIR, REPRODUCIDOS CONTRA 09955667

- I-1: corregido. Con un TState mutable editado en sitio, la sonda da same=False, initial.W=30, dirty=True y veto=True. También pasa tras DiscardAndReload y tras Applied (DraftSession.cs:253, :406, :536; SeparatedFromInitial en :313-314 cubre SetCurrentDraftState, CrossBoundary y KeepMine). Quedan riesgos residuales en N-2 y N-3.
- I-2: corregido. Secuencia: A.TryBeginApply, A.EnterError, luego B.TryBeginApply y A.CompleteApply(CommitThrew). Resultado: B rechazado con AnotherUpdateInProgress; A aceptado como CommitThrew, termina en Error con UnknownOrigin nulo, y la ranura se libera. Discard, DiscardAll y RetireSession ya no liberan la ranura (DraftSession.cs:607-610), y un borrador nuevo del mismo rack sigue rechazado. Queda un residuo en N-4.
- I-3: corregido. La sonda da MATCH=False en los dos sentidos, y [Mutable, NotMember] da lo mismo que [NotMember, Mutable] (DraftStaleBase.cs:80-102, :151-155). Queda un residuo en N-7.
- I-4: parcial. La guarda ya detecta un static readonly mutable (texto y reflexión) y DraftBomLines, pero la regla de Bom se debilitó (N-1).
- M-1 (origen): corregido. De Unknown a Error, la sonda da origin=null (:597).
- M-2: corregido. Una entrada Blocking conserva text=null y su comparación da MATCH (:105-110).
- M-3: corregido. Las pruebas de quitar un fondo y de recuentos de referencias usan ahora conjuntos de entradas distintos. Desapareció la tautología.
- M-4: corregido. Se detectan ValueTask, lock, Monitor, Interlocked, PeriodicTimer, Console., Trace., Debug., HostDatabase y "Commit (", y la guarda de SessionId tiene control negativo. Queda un residuo en N-8.
- M-5: corregido. Las listas del llamador se copian, y Warnings se vacía en TryBeginApply (:466). Queda un residuo en N-6.
- M-6 (orden): corregido. Con el comparador lanzando, la sonda da retired=False, count=1 y registered=True (DraftRegistry.cs:193-194).
- M-7: corregido. needsLazy=False cuando el borrador está desligado (:283).
- M-8: corregido. Orden ordinal real; la sonda da [B Z _x a b ä]. Queda un residuo de cobertura en N-9.
- M-10: corregido. Se sustituyó por la guarda de preview con control negativo (WorkspaceDraftBoundaryTests.cs:545-586).

2. HALLAZGOS NUEVOS

N-1 — BLOQUEANTE — INV-F4T1-22 ("Ninguna regla existente de la guarda se debilita") e INV-F4T1-16 ("lo mismo que WorkspaceModelBoundaryTests")
- Dónde: WorkspaceDraftBoundaryTests.cs:159, :161-162 y :203. Antes (90a81104:152 y :191) las reglas eran `\bBom` y `Bom\b` sin distinguir mayúsculas.
- Qué pasa: el grupo nuevo distingue mayúsculas, así que la regla nueva no contiene a la vieja. Comparación con la regex real, vieja frente a nueva:
  - en el código, "var BOMLines = 1;" y "BOM.Build();" daban True y ahora dan False;
  - en los nombres de miembro, "DraftBOM", "GetBOM", "draftbom" y "DRAFTBOM" daban True y ahora dan False;
  - la regla del padre (`Bom\b`) sigue dando True, así que la guarda del subnamespace es ahora más débil que la del padre.
- Por qué importa: "BOM" en mayúsculas es la grafía habitual del repositorio (335 apariciones en src, RACKBOMTOTAL, ResumenBOM). Ninguna prueba lo detectó porque no había control negativo en mayúsculas. El producto actual cumple.
- Corrección mínima: conservar las alternativas viejas junto a la nueva (`|\bBom|` en I63Concepts y `|Bom\b|` en ForbiddenMemberNames) y añadir los controles negativos "var BOMLines = 1;" y "DraftBOM".

N-2 — IMPORTANTE — D-10 (el inicial es la referencia de dirty) y el espíritu de INV-F4T1-19; es el aliasing "por otra vía" a través del estado de la relectura
- Dónde: InitialDraftState toma por referencia el objeto de la lectura en DraftSession.cs:252 y :434, que usan DiscardAndReload, KeepMine y Applied.
- Las pruebas fijan ese alias con Assert.Same(read/post.InitialState, InitialDraftState) en ocho puntos:
  - WorkspaceDraftApplyTests.cs:275, :494 y :517;
  - WorkspaceDraftRegistryTests.cs:31;
  - WorkspaceDraftSessionTests.cs:722, :825, :1025 y :1050.
- Escenario de la sonda: el host materializa el estado, lo pasa a Admitted y luego enlaza la superficie a ese mismo objeto y lo edita en sitio.
  - Resultado: initial.W=50, current.W=30, dirty=True.
  - Actualizar escribe CurrentDraftState (W=30) y Applied deja el borrador Clean, así que la edición se pierde sin aviso.
  - Pasa lo mismo si tras POST se re-renderiza desde post.InitialState.
- Corrección mínima: copiar al recibir (InitialDraftState = _copy(read.InitialState); CurrentDraftState = _copy(InitialDraftState)) y cambiar esos ocho Assert.Same por igualdad del comparador. Si se prefiere no tocar el núcleo, fijarlo como obligación explícita de T2, T3 y T4: no retener ni editar el objeto entregado en una DraftIntentRead, y enlazar la superficie solo a draft.CurrentDraftState.

N-3 — MENOR — INV-F4T1-19 ("nunca son el mismo objeto")
- Dónde: DraftSession.cs:253, :313-314, :406 y :536.
- Qué pasa: el núcleo confía en el copiador sin verificarlo. Con un copiador identidad (b => b), la sonda da same=True, dirty=False y veto=False: vuelve I-1.
- Además: un copiador superficial de PushBackEditorState compartiría DynamicFrontMatrix y la lista de PushBackEditorFront. El núcleo no puede detectarlo; queda como obligación de T2 y T4 (copia profunda).
- Corrección: para tipos de referencia, comprobar !ReferenceEquals(copia, origen) y tratarlo como error de programación, con su prueba.

N-4 — MENOR — INV-F4T1-20 y D-13 (Aplicando solo mientras hay una operación)
- Dónde: DraftSession.cs:483-484 y DraftApplyOutcome.cs:145.
- Qué pasa, según la sonda:
  - tras ClosedDetached, el borrador y el resultado siguen en Status=Applying aunque HasUpdateInProgress=False, y DeferredErrorReason queda colgado;
  - un segundo CompleteApply sobre el borrador desligado devuelve DraftWasRegistered=True, que es falso.
- Corrección: NotApplying debe informar _registered, y hay que documentar que el Status de un borrador desligado no tiene valor. INV-20 dice "no cambia nada más", así que no propongo tocar _explicitStatus sin decisión.

N-5 — MENOR — D-13 ("avisos visibles en el panel")
- Dónde: DraftSession.cs:526-532.
- Qué pasa: AppliedWithWarnings con una relectura de POST fallida da AppliedPostRereadFailed y Unknown, y los avisos se pierden (la sonda da warnings=[]). INV-09 no cubre esa combinación.
- Corrección: en esa rama, guardar outcome.Warnings si el kind es AppliedWithWarnings.

N-6 — MENOR — INV-F4T1-23
- Dónde: :506 y :538 comparten el array del outcome; :570 comparte el array con DraftCheckResult.
- Qué pasa: un downcast a T[] cambia el borrador. La sonda da "HACK" y "changed via outcome", con sameArray=True.
- Corrección: Array.AsReadOnly, o copiar al asignar.

N-7 — MENOR — D-12 (observabilidad de la ambigüedad)
- Dónde: DraftStaleBase.cs:113 y :122-126.
- Qué pasa:
  - una clave ambigua no aparece en Entries y MembershipOf la lee como NotMember: la sonda da, para Blocking+Mutable, amb=True, MembershipOf=NotMember y entries=0;
  - se informa como diferencia de Membership, igual que un cambio real;
  - no hay forma de enumerar las claves ambiguas;
  - una base ambigua al nacer condena el borrador a Conflicto en cada Actualizar, sin ninguna señal.
- Corrección: exponer AmbiguousKeys ordenadas, y como obligación de T2, que el lector emita una sola entrada por clave.

N-8 — MENOR — INV-F4T1-22 (residuos de la guarda)
- Detección de Console, Trace y Debug (WorkspaceDraftBoundaryTests.cs:108): no detecta `using static System.Console;` ni `using static System.Diagnostics.Debug;` con llamadas sin calificar, ni "Console .WriteLine" con espacio. La sonda da False en los tres.
- Estado estático (IsImmutableFieldType, :267-272): acepta cualquier struct readonly. Así, `static readonly DraftCheckResult Empty = new(..., new List<DraftDifference>())` pasa la guarda de texto y la de reflexión.
- Corrección: `\b(?:Console|Trace|Debug)\s*\.`, más `using static` de esas clases, y exigir de forma recursiva que los campos del struct sean inmutables.

N-9 — MENOR — cobertura de INV-F4T1-20 y 23
- Orden de las claves (WorkspaceDraftSessionTests.cs:1210 y :1224): las pruebas usan solo claves ASCII en minúscula, donde el orden ordinal coincide con el de cultura. Un SortedDictionary con InvariantCulture también pasaría. Faltan claves como "B", "a" y "_x".
- Escenarios sin probar:
  - un error diferido más NotAppliedConflict;
  - un error diferido más AppliedPostRereadFailed;
  - un error diferido, luego Discard y luego CompleteApply;
  - el Status de un borrador desligado tras CompleteApply.
- La prueba ADraftLeftInErrorByADeferredErrorLeavesOnlyThroughDiscardAndReload (WorkspaceDraftApplyTests.cs:283) pasa también en el RED. El diferimiento sí lo cubren otras tres pruebas que fallan en el RED.

N-10 — MENOR — INV-F4T1-15 y robustez
- Dónde: :481-487 y :535-536. La ranura se libera, _explicitStatus se borra y se ejecuta Rebase antes de llamar a _copy.
- Qué pasa:
  - si el copiador lanza, el error diferido nunca se aplica: la sonda da status=Clean, deferred colgado y err=null;
  - una lectura admitida con InitialState nulo termina ahora en NullReferenceException dentro del copiador, cuando antes se aceptaba.
- Corrección: calcular la copia antes de mutar, y rechazar un estado inicial nulo en la frontera con ArgumentNullException.

3. INVARIANTES Y DECISIONES

- INV-01..21, 23 y 24, y D-10, D-12, D-13 y D-17: cumplidos en su letra, salvo las salvedades de N-2..N-7. INV-22 no se cumple por N-1.
- Contradicción prevista, no es un defecto: CommitThrew más un error diferido lleva a Error con UnknownOrigin nulo. Así se cumplen a la vez INV-20, 23 y 14, pero se pierde CheckNow, y el contenido se conserva y veta el cierre sin que haya forma de volver a aplicarlo.

4. PRUEBAS

- El GREEN no toca ninguna prueba: el RED queda intacto.
- Las seis pruebas eliminadas tienen sustituta igual o más fuerte, o su retirada está autorizada (INV-24), con una excepción: la regla de Bom (N-1).
- Las pruebas nuevas que pasan en el RED son guardas estructurales o guardas de regresión de la StaleBase, que es aceptable, más la indicada en N-9.

5. ALCANCE

Limpio. Desde 13775d8f solo cambian rutas del AllowedWriteScope: nueve de los diez archivos (DraftClosePolicy no cambia). No hay host, UI, persistencia ni mutación. Los esqueletos del RED son mínimos.

TABLA FINAL

| Hallazgo | Estado |
|---|---|
| I-1 | corregido (residuos N-2 y N-3) |
| I-2 | corregido (residuo N-4) |
| I-3 | corregido (residuo N-7) |
| I-4 | parcial (N-1) |
| M-1 origen | corregido |
| M-2 | corregido |
| M-3 | corregido |
| M-4 | corregido (residuo N-8) |
| M-5 | corregido (residuo N-6) |
| M-6 orden | corregido |
| M-7 | corregido |
| M-8 | corregido (residuo de cobertura N-9) |
| M-10 | corregido |

RECUENTO POR CLASE: BLOQUEANTE 1 (N-1), IMPORTANTE 1 (N-2), MENOR 8 (N-3 a N-10).
