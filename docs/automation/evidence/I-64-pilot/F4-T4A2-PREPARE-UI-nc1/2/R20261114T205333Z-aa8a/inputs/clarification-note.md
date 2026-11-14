NOTA ACLARATORIA DE LA REVERIFICACION DE F4-T4A2-PREPARE-UI, INTENTO 2 (autorizada por el Owner)

Autoridad de esta nota:
- La disposicion del Owner/Coordinator "I-64 - F4-T4 OWNER / COORDINATOR DISPOSITION", seccion 1 (F4-T4A2): OPCION (a), una sola reverificacion del Controller con una nota aclaratoria. La nota debe citar la autoridad exacta del contrato, explicar por que el comportamiento observado esta permitido y no cambiar ningun hecho de producto, de pruebas, de alcance ni de evidencia RED/GREEN.
- La respuesta del Owner a la pregunta de Identity tras la custodia: "Si, mismas A-I" (la misma sustitucion acotada que en F4-T1, solo para esta reverificacion y sus nc1..nc3).
Esta nota no cambia ninguna entrada: el contrato, la delegacion, la entrega, los registros de relevo y los TRX son los mismos que evaluo la verificacion R20261114T165017Z-4b2a, que se conserva como evidencia historica (EXECUTION_BLOCKED / STOP por F4T4A2-C04).

PARTE 1. La parada F4T4A2-C04 de la verificacion 4b2a

Texto exacto de la parada (gate-contract.json de las entradas, StopConditions, Id F4T4A2-C04):
"the browser view-model (its 33-signature public shape), the panel view-model, the navigation view-model, any Plugin file, the read path (the edit intention reader, its adapter or RackSiblingScan), the Themes or Controls folders of the T1B closure, or any file of Drafts would have to change"

Texto exacto de la autoridad (gate-contract.json de las entradas, Invariants, INV-F4T4A2-05, frase de la correccion del intento 2):
"CORRECCION (intento 2, M-3, M-5, M-6 y M-7): tras un descarte del navegador, la eleccion la retira el Rematerialize del navegador en silencio (sin A2-34), desviacion declarada, y la prueba fija el NoticeText real; "No" se prueba tambien en la eleccion de Unknown con PostReread (cero llamadas, A2-36 y A2-12 en ReconcileText); cada prueba de retirada afirma HasChoice antes de retirar; la regla 3 comprueba que BrowserFieldChoiceText precede a Si; la prueba de vista de salir de la caja provoca UIElement.LostFocusEvent sobre la caja real."

Por que el comportamiento observado esta permitido:
1. F4T4A2-C04 se dispara solo si alguno de los archivos que enumera "would have to change". La desviacion declarada en INV-F4T4A2-05 consiste justamente en NO cambiar el view-model del navegador: tras un descarte del navegador, la eleccion la retira el Rematerialize del navegador en silencio, sin A2-34, y la prueba fija el NoticeText real.
2. El contrato del intento 2 es la autoridad vinculante de esta delegacion. Su correccion de INV-F4T4A2-05 enmienda para ese caso concreto la expectativa de A2-34 en Apply del paquete (D-T4A2-03). Leer el paquete contra el contrato en este punto es el error de la verificacion 4b2a.
3. La frase de la entrega "el navegador no se puede tocar (F4T4A2-C04)" explica por que el Worker acepta y fija el comportamiento real; no declara que haga falta cambiar el navegador.
4. Comprueba tu mismo con git que git diff --name-only BaseSha..CurrentSha no contiene src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs ni ninguno de los demas archivos o carpetas que enumera F4T4A2-C04 (Scope ya paso en 4b2a con las mismas entradas).
Si esos hechos se sostienen, F4T4A2-C04 no se dispara, y en Tests la expectativa vigente para ese caso es la frase de la correccion citada arriba (el NoticeText real, sin A2-34).

PARTE 2. Identity tras la custodia (sustitucion acotada A-I; solo esta reverificacion y sus nc1..nc3)

- CurrentSha = el CurrentSha de worker-handoff.json de las entradas es el producto que se verifica. NO lo cambies. La condicion B se evalua con ese CurrentSha: si no es ancestro de HEAD, B falla e Identity = fail.
- HEAD de custodia = c3ae1c970c908ef541840fc583680b86376cae0b: despues de la verificacion 4b2a la sesion custodio el intento 2 (evidencia y analisis) en commits solo de docs/, como pide 16.4.
- En lugar de la parte exacta "HEAD = refs/remotes/origin/<rama> = CurrentSha" de Identity, comprueba:
  A. HEAD = la rama remota del producto;
  B. CurrentSha es ancestro de HEAD;
  C. toda ruta de git diff --name-only CurrentSha..HEAD esta bajo docs/;
  D. hay CERO rutas bajo src/ y tests/;
  E. no hay otros cambios de producto, build o fuente entre CurrentSha y HEAD;
  F. la ascendencia de BaseSha sigue valida;
  G. RedSha sigue en la cadena esperada BaseSha..CurrentSha;
  H. la identidad de la rama es correcta;
  I. el worktree es el correcto y esta limpio.
  Si se cumplen todas, la identidad del producto tras la custodia queda aceptada. Cualquier ruta de producto, prueba o build por encima de CurrentSha: Identity = fail y STOP.
- Remote: la rama remota debe ser la punta de custodia. Hecho medido por la sesion antes de la cesion (y otra vez despues, en el registro de relevo de esta verificacion): ls-remote de refs/heads/architecture/workspace-persistente-rackcad = c3ae1c970c908ef541840fc583680b86376cae0b; origin/main = 812baf879612320653094a8aaf46aca0bac81383 = MainSha. No ejecutes git ls-remote: tu sandbox no tiene red.
- Hecho medido por la sesion antes de invocarte, sobre el producto ae0a7b40 de la verificacion real (no sustituye tu comprobacion de A-I sobre el CurrentSha de las entradas): ae0a7b40 es ancestro de c3ae1c97, y git diff --name-only ae0a7b40..c3ae1c97 tiene exactamente estas 46 rutas, todas bajo docs/ (0 bajo src/ y 0 bajo tests/):
  docs/automation/evidence/I-64-evidence.md
  docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/byte-fidelity-check.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/cession-end.txt
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/cession-start.txt
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/config-entry.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/config-exit.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/custody-manifest.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/delegation.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/gate-contract.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/launcher-pid.txt
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/launcher-winpid.txt
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/processes-entry.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/processes-exit.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/prompt.md
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/relay-record.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T161620Z-f15d/session-extra-checks.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/cession-end.txt
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/cession-start.txt
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/ci-filter-counts.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/config-entry.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/config-exit.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/custody-manifest.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/m1-viewmodel.patch
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/processes-entry.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/processes-exit.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/prompt.md
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/relay-record.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/worker-handoff.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/analysis.md
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/cession-end.txt
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/cession-start.txt
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/config-entry.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/config-exit.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/custody-manifest.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/direct-review.md
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/freetext-scan.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/independent-review.md
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/launcher-winpid.txt
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/output.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/processes-entry.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/processes-exit.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/prompt.md
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/relay-record.json
  docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/stderr.txt
  docs/automation/state/I-64.yml
