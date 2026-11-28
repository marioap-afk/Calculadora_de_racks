I-64 — SMOKE-2
OWNER DISPOSITION / RUN 2

======================================================================
RESULTADO GENERAL

Smoke-2 Run 2:

PASS WITH ONE PRODUCT DEFECT / REWORK REQUIRED

No repetir Smoke-2 completo.

La evidencia obtenida sigue siendo válida y debe custodiarse.

El único defecto de producto que bloquea el cierre final de Smoke-2 es:

STEP 50 — “Ver” no actúa cuando existe una selección previa en el dibujo.

Los demás puntos descritos abajo quedan aceptados como condición del ensayo
o limitación de cobertura.

======================================================================

1. PASOS 70–72 — “SAVE CHANGES?”
    ======================================================================

Decision:

NO LO CLASIFICO COMO FALLO DE RACKCAD.

Reclasificar como:

OBSERVED / TEST-DRAWING CONDITION

Razón:

Se acreditó que D1 obtiene DBMOD != 0 incluso sin intervención del panel:

* DBMOD inicial = 0;
* tras regeneración/ZOOM aparece el bit 1;
* el dibujo contiene objetos/proxies de una aplicación ausente;
* el fenómeno existe independientemente del borrador de RackCad.

Por tanto, la premisa de la guía:

“si aparece Save changes? -> FAIL”

resultó demasiado estricta para este dibujo real.

======================================================================
LO QUE SÍ QUEDÓ PROBADO DEL VETO

Después de responder “No” al Save changes?:

RackCad bloqueó correctamente el cierre con trabajo pendiente en:

* CLOSE;
* CLOSE repetido;
* X de pestaña;
* CLOSEALL;
* QUIT.

Esto cuenta como PASS de la función de veto.

Registrar además:

la pregunta de AutoCAD ocurre ANTES del veto de RackCad cuando el dibujo
tiene modificaciones propias de AutoCAD.

No afirmar que RackCad controla el orden de ese diálogo.

======================================================================
RAMA “SÍ” DEL SAVE

No hace falta repetir Smoke-2 para probarla.

Queda como observación:

si el operador responde “Sí”, AutoCAD puede guardar sus modificaciones
preexistentes antes de que RackCad tenga oportunidad de vetar el cierre.

Esto NO implica que el borrador RackCad se escriba.

No abrir una corrección de producto por este punto salvo que aparezca
evidencia de pérdida o escritura del borrador.

======================================================================
2. PASO 50 — VER CON PRESELECCIÓN

Decision:

CONFIRMED PRODUCT DEFECT

Step 50 remains:

FAIL

Observed:

with an existing AutoCAD selection,
“Ver” performs no visible navigation.

This contradicts the accepted navigation behavior.

Do NOT waive it as a Smoke-2 limitation.

======================================================================
CORRECCIÓN DEL PASO 50

Crear una corrección NARROW.

No reabrir F2 completo.

Objetivo exacto:

“Ver” debe funcionar aunque exista una selección previa en AutoCAD.

Antes de modificar:

reproduce and determine whether the cause is related to:

* implied selection;
* PICKADD;
* focus;
* SetImpliedSelection;
* the selection/view sequence;
* or another host-specific navigation interaction.

Do not guess.

======================================================================
SCOPE DE LA CORRECCIÓN

Preserve all existing navigation semantics:

* NavigationTargets authority;
* immediate revalidation;
* zero authored writes;
* application context + DocumentLock;
* deterministic target set;
* focus returned to drawing;
* 10% View margin;
* Center preserving scale;
* Locate behavior.

Do not use this defect as justification for a navigation refactor.

======================================================================
TARGETED RE-SMOKE

After the correction, do NOT rerun the 88 Smoke-2 steps.

Run a targeted navigation mini-smoke on a representative real rack:

1. no preselection -> Ver;
2. unrelated object preselected -> Ver;
3. rack object preselected -> Ver;
4. multiple objects preselected -> Ver;

and, to guard against regression:

5. Seleccionar;
6. Centrar;
7. Localizar.

For each:

PASS / FAIL / OBSERVED.

If all pass:

STEP 50 = CLOSED

and Smoke-2 may proceed to final disposition without another complete run.

======================================================================
3. UNAVAILABLE TEST DRAWINGS

Decision:

ACCEPT NO DISPONIBLE.

Do not manufacture artificial drawings merely to complete the matrix.

Unavailable physical cases:

* divergent rack;
* A/B rack;
* old Push Back drawing;
* D2 editable-rack scenario where applicable;
* any other class explicitly noted in the run log.

Record each as:

NOT AVAILABLE FOR PHYSICAL SMOKE

not FAIL.

======================================================================
COVERAGE LIMITATION

Physical coverage remains limited for those classes.

This is acceptable because Smoke-2 did physically exercise:

* real Push Back racks;
* 19 admitted Push Back racks in distribution;
* non-pilot Selectivo;
* non-pilot Dinámico;
* header/out-of-pilot behavior;
* real writes;
* failure injection;
* stale/conflict behavior;
* close lifecycle;
* AppliedWithWarnings.

Do not claim that unavailable classes were physically validated.

Existing automated evidence remains their supporting evidence.

======================================================================
FAILURE INJECTIONS

Ratify PASS for:

* write failure before authored change;
* SEAL=before;
* SEAL=after / unknown persistence;
* reconciliation after unknown state.

The observed SEAL=after behavior is particularly important:

the authored write existed,
the state became unknown/conflicted,
and explicit reload recovered the actual authored value.

Keep this evidence.

======================================================================
APPLIED WITH WARNINGS

Step 87:

PASS

This is now physically validated in real AutoCAD.

The expected warning appeared and both linked views received:

AnnotationScale = 2

Preserve the exact evidence.

======================================================================
UPDATE-IN-FLIGHT / CLOSE

Ratify the observed branch:

Actualizar completed before the close command was processed.

Therefore:

PASS for the observed branch (a).

Do NOT claim that a document was physically destroyed while mutation was
mid-transaction.

That narrower race remains unprovoked.

======================================================================
ORIGIN / “NINGÚN SOBRE EDITABLE”

Ratify as DESIGN.

The response:

“No se aplicó: Ningún sobre editable de la relectura coincide con el origen
del borrador.”

after losing the original editable origin is already covered by T4B I-9.

Not a new defect.

======================================================================
IC-04 / INTERNAL CONDITIONS

Keep the rule already used in the guide:

do not fabricate evidence.

Anything impossible to observe in AutoCAD without instrumentation is recorded
as:

NOT PHYSICALLY VERIFIABLE / AUTOMATED EVIDENCE

where appropriate.

======================================================================
ORIGINALS / SAFETY

Ratify:

* 7 originals intact;
* 37 working copies intact where expected;
* no unintended save;
* no acad.exe / accoreconsole remaining;
* no RACKCAD_DEBUG_* variables remaining;
* no RackCad error log entries;
* no relevant Windows application errors.

“Load Once” was the correct choice.

Do not change Trusted Locations / Always Load as part of Smoke-2.

======================================================================
CUSTODY

Authorize custody of Run 2 NOW.

Do not wait for the Step-50 correction to preserve this run.

Custody must preserve:

* run-log-2;
* Owner summary;
* module/DLL hashes;
* PASS / FAIL / OBSERVED classifications;
* unavailable-case list;
* injection results;
* original/copy integrity evidence;
* relevant screenshots/log extracts;
* this Owner disposition.

Redact customer-identifying names where required.

Do not rewrite the raw historical meaning of the run.

The Step-50 correction and mini-smoke become a later ADDENDUM.

======================================================================
DIAGNOSTIC FOLDERS

Do NOT clean diagnostic folders yet if they may help investigate Step 50.

After:

1. Run-2 custody is published;
2. Step-50 cause is understood;
3. any needed evidence is copied/custodied;

then the diagnostic folders may be deleted.

======================================================================
FINAL SMOKE-2 STATUS

Current:

SMOKE-2 RUN 2
PASS WITH ONE PRODUCT DEFECT / REWORK REQUIRED

Blocking item:

STEP 50 — Ver with existing selection

Accepted limitations:

* Save Changes ordering caused by drawing/test condition;
* unavailable physical drawing classes;
* internal conditions not physically observable without instrumentation;
* exact mid-transaction close race not physically provoked.

After Step 50 is corrected and the targeted navigation mini-smoke passes:

Smoke-2 may be signed:

PASS WITH LIMITATIONS

without repeating the full run.

======================================================================
F4 STATUS

Do not declare F4 fully CLOSED yet.

F4 is:

PRODUCT IMPLEMENTATION COMPLETE
PHYSICAL VALIDATION NEAR COMPLETE
ONE TARGETED NAVIGATION DEFECT OPEN

Close F4 after:

* Step-50 correction;
* targeted mini-smoke PASS;
* custody/addendum;
* final Owner signature.
