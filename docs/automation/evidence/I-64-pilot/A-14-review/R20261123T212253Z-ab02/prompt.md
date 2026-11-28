I-64 - REVISION FORMAL DEL ARCHITECT (ARCHITECTURE_REVIEW, effort semantico Deep) - A-14 / Replaneacion hacia la ventana RackCad sin bloqueo
RunId: R20261123T212253Z-ab02. Modo: SEPARATE SESSION (sesion nueva del Codex CLI 0.167.0-alpha.3.2, solo lectura, modelo gpt-6-luna, effort high;
celda decidida por el Coordinator en Q-C2: binario SHA-256 4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17, config Snapshot L).
Independencia: el autor del objeto es la sesion principal de I-64 (Claude, otro proveedor); tu no participaste en su redaccion ni en sus
revisiones independientes.
Insumos: solo los archivos listados; no hay memoria ni transcripcion del autor. Contexto inyectado: este prompt.

ANTECEDENTES: A-14 replanea I-64 hacia una ventana RackCad tipo navegador que no bloquea AutoCAD: lista lateral de racks sin pestanas,
"Actualizar todo", secciones, RACKCAD/RK abren la ventana, persistencia del tamano y la posicion, rediseno por sistema e integracion
por tramos. Las decisiones del Owner del 2026-11-23 estan en el registro literal. Un revisor independiente de solo lectura (no es el
Architect) reviso el v1 y comprobo la v2, la v2.1 y la v2.2; la ultima comprobacion dio EMITIBLE A ARCHITECT con 4 menores, que la v2.3
aplico. El Coordinator decidio Q-C1 (rama lateral) y Q-C2 (esta celda). Esta es la UNICA revision formal autorizada (orden literal abajo).
Las revisiones independientes son contexto, no autoridad: decide cada respuesta en el texto de A-14 y en las fuentes.

OBJETO REVISADO (commit exacto ab0170b76c34249b4d772ac18cc571c1b5a587f1 de la rama lateral architecture/workspace-persistente-rackcad-a14,
hijo de fa59bd82acff7ce276a1605c60d426c45a3e3b8d de la rama de I-64; CI exacta 45263171189 en success, 4/4; el worktree esta en ese commit, limpio).
Lee con: git show ab0170b7:<ruta>; registra cada blob con: git rev-parse ab0170b7:<ruta>. No hagas checkout.
  - A-14 (PROPOSED / NOT ACTIVE): docs/initiatives/I-64-A-14.md (blob esperado 427ce3039516c1254f9ae96888b2e9649ce8b200)
  - Freeze V5: git show 9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311:docs/initiatives/I-64-proposal-v5.md (blob dc1924ff5a73a4e529fbc5f995621d29351a091b;
    el mismo blob esta en ab0170b7 en la misma ruta). A-14 cita "V5 :n" por linea de ese blob.
  - Enmiendas previas: docs/initiatives/I-64-A-1.md .. I-64-A-13.md (A-7 entera: docs/initiatives/I-64-A-7.md; de las demas, al menos
    la cabecera, Applies-to, la autoridad y el estado)
  - Aclaraciones: docs/initiatives/I-64-implementation-clarifications.md (IC-01..IC-11; "ICL :n")
  - Brief del Owner: docs/initiatives/I-64-owner-brief.txt ("brief :n")
  - F4B: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md ("OD :n");
    docs/initiatives/I-64-f4b-push-back-completo-package.md ("PKG :n"); docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md
    (F4B-E01, F4B-E57); docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md;
    docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt5/I-64-f4b-pbt5-package-v2.4.md (SPB-03, SPB-12, SPB-13, SPB-23)
  - Fuentes custodiadas de A-14: docs/automation/evidence/I-64-pilot/A-14/ (coordinator/owner-a14-decisions.md, replaneacion-i64.html,
    prototipo/, I-64-A-14-draft-v1.md .. v2.3.md, review/, encargos/, a14-drafting-notes.md y sha256.txt)
  - Fidelidad del versionado: git diff ab0170b7:docs/automation/evidence/I-64-pilot/A-14/I-64-A-14-draft-v2.3.md ab0170b7:docs/initiatives/I-64-A-14.md
  - Cambio de la rama lateral: git diff --stat fa59bd82acff7ce276a1605c60d426c45a3e3b8d ab0170b7
  - Evidencia: docs/automation/evidence/I-64-evidence.md Sec.293; estado docs/automation/state/I-64.yml (sin cambio en este commit)
  - Producto citado (solo lectura): src/RackCad.Plugin/RackMenuCommands.cs; src/RackCad.UI/RackMainMenuWindow.xaml;
    src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs; src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs;
    src/RackCad.Application/Settings/UserSettings.cs; src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs
  - Orden del Coordinator que autoriza esta revision (literal, ignorada por Git, solo como dato):
    artifacts/orchestration/I-64/A-14-review/R20261123T212253Z-ab02/coordinator-a14-review-order.md (SHA-256 C37B9E780E689206A0585482F9B385A672239A539E8F87846467FE3DE15008CB)
AUTORIDADES (en ab0170b7):
  - docs/INITIATIVE_LIFECYCLE.md Sec.2 (unidades), Sec.3 (M-01..M-08, OWNER-RESERVED), Sec.5 (participacion del Architect), Sec.6 (Freeze
    y A-n, Applies-to), Sec.7 y Sec.8 (gates, READY, Owner Validation)
  - docs/WORKFLOW.md Sec.2, Sec.4.1, Sec.4.5 (integracion, Sec.4.5.6 y Sec.4.5.7), Sec.10, Sec.11
  - docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md (D13), docs/adr/0047-workspace-persistente-modeless-rackcad.md,
    docs/adr/README.md, y los ADR que A-14 cita (0010, 0032, 0034)

RESPONDE EXPLICITAMENTE (campo Answers; Answer = YES o NO; Conforming = si esa respuesta deja la enmienda conforme):
 Q01 Materiality: is A-14 Sec.9 correct (M-01..M-07 = YES, M-08 = YES by prudence, archetype NEW ARCHITECTURE), with no M flag missing for
     any frozen element that A-14 changes? Fill MaterialityAssessment for each M.
 Q02 Authority: does every change in Sec.3 and Sec.4 carry the correct authority (every OWNER-RESERVED matter inside the exact Sec.11
     choice; Architect + Coordinator for the material M flags; Coordinator-only only for pure resequencing), with nothing OWNER-RESERVED
     left outside the signature and nothing inferred from the Owner?
 Q03 Composition of "Actualizar todo" (C-05): does A-14 freeze the result and not the mechanics (all-or-nothing on the union, one MUTATE
     transaction, one UNDO step, the re-read of every rack inside MUTATE before any write, own-mutation scope over the union, INV-28 for
     drafts not included, eligibility conditions 3 and 4, per-boundary classification); does it leave the I-55 seam unchanged
     (RackSiblingRedrawRun, ISiblingRedrawPort, SiblingRedrawTransaction, the Insertar contract of INV-30); does it give an executable STOP
     to the I-55 Freeze authority if the composition needed to change the seam; and is "Architect + Coordinator approve the composition in
     the phase 4 package, with its independent review, before its contract" a valid authority (Sec.12 rows 7 and 8)?
 Q04 Window rectangle route (C-10): is persisting size and position through the RackCad user settings (UserSettings.cs), not the AutoCAD
     palette state, consistent with D-01 :122 (no toolID, no Load/Save) and with D-22 / INV-21 as amended by the four named exceptions;
     are the fallback (rectangle not fully on a visible monitor, or unreadable), the rule that saving the rectangle never alters other
     settings, and the repeated S1-11 profile diff with the I-52 acknowledgement sufficient and testable (INV-A14-03, R-12)?
 Q05 Applies-to and A-7: is "Applies-to: all" valid under LIFECYCLE Sec.6 for the units of later tramos; is the scope extension of A-7 to
     those units (text unchanged, only scope, so INV-24 and S2-16..S2-22 bind their editor phases) within A-7's own authority (Architect +
     Coordinator; Owner informed, Sec.11 point 15); and is it correct NOT to extend A-1..A-6 and A-8..A-13 (Sec.11 note P-1; Sec.12 row 3b)?
 Q06 Reading rule of "pestana" (C-03): are the reading rule, its closed list of V5 lines, its precedence (an own delta in Sec.4 or Sec.5
     prevails) and the list of history that is not rewritten unambiguous and complete; and is the F3 substitution (old tests kept intact,
     Skip with a reason naming a declared substitution record of phase 2, new behavior in new tests, ADR-0029 D13) valid without reopening
     F3 (Sec.12 rows 4 and 5)?
 Q07 Clean draft life and navigation (C-02): are the new draft life (a Clean draft is released when its rack stops being open; a dirty one
     stays alive), the blocking D-10 boundary when the open rack changes (commit per ADR-0032 D6 or abort), K = 1, "Seguir la seleccion"
     on at session creation, Seleccion(N) opening no rack or draft, "Descartar cambios" never writing the DWG, and IC-03 replaced by Q-10
     consistent with D-06, D-08, D-10, INV-04, INV-09, INV-18, INV-27 and INV-34, and covered by test obligations (INV-A14-04, 08, 09;
     Sec.12 row 6)?
 Q08 D-PB-01 (C-08): does A-14 replace ONLY the modal sentences of OD :25-26, :34 and :37, from the close of phase 3, and keep the rest of
     D-PB-01 (one editor, CLASSIC/WORKSPACE modes, zero authored writes from the editor, Workspace authority over the final Actualizar);
     does it leave F4B, the PB-T5 contract and the Smoke-PB unchanged and SPB-12/SPB-13 unrelabelled (Relation to F4B)?
 Q09 RACKEDITAR and creation commands (C-09): is the declared change per redesigned system (characterization green before extraction; old
     tests kept with Skip and a declared superseding record; the Owner's issuance order naming each change; anything not named = M-03 and
     STOP) a valid application of D-15, D-16, INV-19, OV-17 and ADR-0029 D13, with the classic write path of RACKEDITAR unchanged?
 Q10 Integration by tramos (C-12): is each tramo a complete delivery unit (READY-01..09, FINAL_CANDIDATE_SHA, its own Owner Validation by
     the stated OV rule, the Smoke-PERF default per tramo, WORKFLOW Sec.4.5 integration with both coverage gates before cleanup); is
     tramo 1 = the I-64 unit and later tramos = new units consuming V5 and the A-n from origin/main coherent with LIFECYCLE Sec.2; are ID3
     at the last READY, F4B inside tramo 1, the ADR-0047 default, and the path to correct the hitos (the next A-n before READY-01 of the
     first affected tramo) executable (Sec.12 rows 1, 2, 15, 16, 17)?
 Q11 Clause deltas and test obligations (Sec.4, Sec.5, Sec.6): does every changed V5 clause have an exact delta and authority, every new or
     extended invariant (INV-A14-01..17 and the extended INV-xx) a test obligation and a phase, and every substituted OV (OV-08, 09, 11, 14,
     17, 20) a replacement, so that each phase package can be written from A-14 without a new decision?
 Q12 Non-reopening: do F1, F2, F3, F4 and F4B (with the PB-T5 contract and the Smoke-PB) stay closed or unchanged; are the texts of
     A-1..A-13 and IC-01, IC-04, IC-06..IC-11 unmodified, with only the IC-02, IC-03 and IC-05 substitutions declared and inside the Owner
     signature?
 Q13 Versioning fidelity: does docs/initiatives/I-64-A-14.md at ab0170b7 differ from the custodied v2.3 only by the declared versioning
     annotations (title without the draft label, "propuesta de la sesion", the Q-C1 and Q-C2 annotations with their source, the Sec.10
     step 1 chain, the Sec.12 and Sec.13 pointers) with no change of substance; do the custodied files match their sha256.txt; are the
     redactions limited to local paths?
 Q14 Is any new Owner decision required beyond the exact Sec.11 choice that A-14 already requests? (Answer YES = se requiere)
 Q15 Is any element left non-executable or unverifiable, or is there a path that lets a phase of A-14 be emitted without its package, its
     independent review and the Owner order? (Answer YES = existe una ruta abierta)

REGLAS:
  - Solo lectura: no escribas archivos, no hagas commit, push ni checkout; usa git show / git diff / git log / git rev-parse / git ls-tree
    / git cat-file y lectura de archivos. No compiles, no ejecutes pruebas, no abras AutoCAD.
  - No leas ni ejecutes nada fuera del worktree y del objeto Git listado. No hay producto que ejecutar.
  - Si una orden auxiliar falla, repitela por otro medio o declara la limitacion en Limitations; no concluyas sin evidencia.
  - Severidad (LIFECYCLE Sec.5 y perfil ARCHITECTURE_REVIEW): REQUIRED si activa un M-01..08 no declarado sobre un elemento congelable,
    deja un invariante sin obligacion de prueba, deja una materia OWNER-RESERVED fuera de la firma, contradice una autoridad o un hecho
    medido, o deja algo no ejecutable o no verificable; una precision sin cambio de significado es OPTIONAL.
  - Cada hallazgo: Question, ubicacion (archivo:linea), defecto, autoridad, correccion minima y si exige una decision nueva del Owner
    (RequiresOwnerDecision).
  - Verdict = AGREED si y solo si RequiredCount = 0. Si falta autoridad reservada del Owner: BLOCKED - OWNER DECISION.
    Toda respuesta con Conforming = false exige al menos un hallazgo REQUIRED con esa Question.
  - Separa los hallazgos REQUIRED y OPTIONAL por su campo Severity.
  - A-14 es append-only desde este commit: si pides cambios, la correccion es A-15; no propongas editar A-14.
  - Prohibido declarar GATE PASS, Candidato, cierre, integracion, activacion o aprobacion del Owner. Toda la salida en ASCII
    (sin tildes ni escapes con barra invertida + u; nombra un caracter no ASCII como U+XXXX). Rellena ReviewSubject con los blobs que obtengas.

PERFIL ARCHITECTURE_REVIEW
Metodo: revision adversarial contra las autoridades citadas, sin escribir.
1. Lee el Freeze, los documentos duenos y el codigo afectado desde la revision fijada.
2. Cada hallazgo: ubicacion (archivo:linea), defecto, autoridad que contradice y correccion minima.
3. Severidad: REQUIRED solo si contradice una autoridad o un hecho medido, deja algo congelado ambiguo, impide ejecutar un
   paso o deja una garantia sin verificar.
4. No reabras lo ya resuelto por el Owner; prefiere pocos hallazgos de alta confianza.

Informe esperado: la salida exacta del esquema rackcad-i64-architect-review/v1 (la escribe el CLI por -o).
