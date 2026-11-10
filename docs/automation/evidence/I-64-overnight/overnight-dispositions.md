# I-64 — Decisiones autónomas del turno nocturno

Registro de cada `OVERNIGHT_OWNER_COORDINATOR_DISPOSITION` tomada bajo la autorización permanente del Owner (literal: `I-64-pilot/F2-T2A-NAVIGATION-HOST/coordinator/coordinator-f2t2a-overnight-autonomy-directive.md`). Solo aplican a I-64; ninguna cambia la semántica global del protocolo. Append-only.

## D-01. Reconciliación con `main`

- **Problema:** `main` avanzó 44 commits (I-67, I-68). WORKFLOW §4 y AUTOMATION_PLAN 16.7 piden rebase con `--force-with-lease`.
- **Opciones:** (a) rebase con force; (b) fusión hacia adelante; (c) seguir con la base antigua.
- **Decisión:** (b), merge `b7e4dc1b` sin conflictos.
- **Fundamento:** la instrucción del Owner prohíbe el force-push y reescribir historia publicada; un rebase cambiaría 116 commits y los SHA verificados de F2-T2A. La opción (c) deja los contratos nuevos con `MainSha` falso.
- **Evidencia:** merge-tree limpio; único archivo común `docs/ROADMAP.md`; I-67 e I-68 tocan Application/Systems, Bom y Domain, sin relación con Workspace; CI del SHA exacto.
- **Riesgo:** al integrar habrá un commit de merge en la rama; lo resuelve la sesión de integración.
- **Alcance:** solo esta rama. La regla global de rebase no cambia.

## D-02. Cierre de F2-T2A

- **Problema:** el estado versionado pide `COORDINATOR_F2_T2A_CLOSE_REQUIRED`.
- **Decisión:** F2-T2A = CLOSED / PASS, como la declara la instrucción del Owner.
- **Fundamento:** producto `EXECUTION_VERIFIED` sobre `44a692a4`; nc1 por A-13; nc2 `03d2` y nc3 `42a7` CONTROL PASS; CI 42755429888 4/4.
- **Alcance:** cierre de esta tarea; no es F2 PASS.

## D-03. Contrato de F2-T2B

- **Problema:** el contrato de T2B lo emitía el Coordinator.
- **Decisión:** lo emite la sesión con las decisiones C-F2T2-01..13 ya aceptadas para F2-T2.
- **Añadidos respecto al paquete:** el campo aditivo `TargetCount` en `WorkspaceNavigationResult`, necesario para la «copia k/M» de SN-03 sin que la UI cuente objetivos; y una excepción por archivo exacto a la guarda que prohíbe `async` en la UI, para esperar `ExecuteAsync` sin bloquear.
- **Fundamento:** alternativas descartadas: que la UI descubra M probando índices (autoridad implícita y dos ejecuciones por clic); un adaptador de continuaciones en el Plugin (más superficie sin beneficio).
- **Alcance:** solo F2-T2B.

## D-04. Corrección de F2-T2B pese a una verificación `EXECUTION_VERIFIED`

- **Problema:** la verificación `ffd2` de la entrega `6c01f644` dio `EXECUTION_VERIFIED`, pero la revisión del código de la sesión y una revisión independiente encontraron defectos de producto que ninguna prueba ejerce. El principal: «Localizar vista» nunca pasa de la primera copia (INV-F2T2B-04, SN-03).
- **Opciones:** (a) cerrar T2B y dejar que Smoke-NAV lo descubra; (b) corrección delegada, consumiendo el último intento; (c) arreglarlo la sesión directamente, fuera de la cadena.
- **Decisión:** (b). `attempts` pasa de 2 a 3 antes de emitir la delegación; `CorrectionOf` = `ffd2`, `FailureClass` = `StopCondition`. Los controles nc1..nc3 no se lanzan sobre `6c01f644` y se reservan para la entrega corregida.
- **Fundamento:** la instrucción del Owner prohíbe marcar PASS solo para avanzar y exige que los gates técnicos sigan vigentes. (a) cerraría un defecto conocido; (c) esquivaría RED, verificación y controles. AUTOMATION_PLAN 16.8 prevé el incremento tras un STOP cuya resolución cambia el trabajo.
- **Evidencia:** `F2-T2B-NAVIGATION-UI/2/R20261110T053114Z-ffd2/analysis.md`, con la secuencia de cada hallazgo.
- **Contrato del intento 3:** `F2-T2B-NAVIGATION-UI/coordinator/gate-contract-attempt3.json`. Se estrecha a dos rutas exactas; los mínimos de pruebas suben a los valores vigentes (143 y 780); el filtro Core pasa a `ExpectRed` false porque la corrección no toca Core; invariantes y prohibiciones sin cambio.
- **Riesgo:** queda agotado el presupuesto de `attempts` de la unidad; otro REWORK sería S-11 y se documentaría como bloqueo real.
- **Alcance:** solo F2-T2B. La verificación `ffd2` se conserva como evidencia y no se reinterpreta.

## D-05. Parada de Authority en la verificación de la corrección

- **Problema:** la verificación `R20261110T061255Z-92b5` de `82bd49b6` dio `EXECUTION_BLOCKED / STOP`, `FailureClass` Authority, porque `docs/automation/evidence/I-64-evidence.md` no figura en `Authorities`, aunque aparece en `git diff --name-only AuthorityRevision BaseSha`.
- **Opciones:** (a) aceptar el bloqueo y emitir otro contrato con esa ruta declarada; (b) reejecutar sin tocar el prompt, sin coste de `attempts` (16.8); (c) aclarar el prompt.
- **Decisión:** (b), una reejecución con el prompt idéntico salvo el `RunId`. La salida `92b5` se conserva tal como se emitió.
- **Fundamento:** 16.3 prohíbe en ese diff solo rutas `EXTERNAL` o con secciones `UNIT_CHANGE`, y define como `UNIT_DOC` la evidencia de la unidad. Las tres rutas del diff lo cumplen. La verificación `ffd2` aceptó un diff de la misma forma como `UNIT_DOC`. No se orientó al Controller para no contaminar la repetición.
- **Evidencia:** `92b5` y `R20261110T061640Z-6b2a` (`EXECUTION_VERIFIED`, 14/14), con la misma entrega, las mismas entradas y el mismo prompt.
- **Riesgo:** el Controller no es determinista en esta lectura; podría repetirse en otra unidad. Queda consumida 1 de las 2 reejecuciones de esta fase.
- **Alcance:** esta verificación. 16.3 no cambia.
- **Incidencia de la sesión:** con `92b5` ya bloqueada se preparó por error el control `b287`. Nunca se lanzó, no produjo salida y se eliminó.

## D-06. nc1 de F2-T2B: el modo de fallo Handoff/Identity reaparece

- **Problema:** nc1 `R20261110T061935Z-2376` da `CONTROL NOT PASSED`. El Controller detecta el `CurrentSha` inexistente y bloquea, con `Identity` en fail, pero también marca `Handoff`, con `FailureClass` StopCondition y S-04. Es el modo de fallo de nc1 en F2-T2A, con otra `FailureClass` (allí, Handoff).
- **Opciones:** (a) reintentar nc1 hasta que coincida; (b) rebajar el oráculo; (c) aplicar el verificador determinista de A-13, sin cambios, a esta ocurrencia; (d) detener F2-T2B.
- **Decisión:** (c), una sola aplicación, con autoprueba previa desde los mismos blobs. A-13 no se modifica ni se amplía: esta decisión es una ocurrencia nueva y solo de I-64. Autoridad: la instrucción nocturna del Owner, §8.
- **Fundamento:** reintentar sería buscar un resultado favorable, porque la salida `2376` es válida. El verificador recibe la ocurrencia por argumentos, ya tuvo revisión del Architect (`bc2b`) y separa la integridad de la entrega de la identidad frente a Git. No hay código nuevo, así que no procede una revisión nueva.
- **Evidencia:** `F2-T2B-NAVIGATION-UI-nc1-bridge/3/`: blob `1c2510f7`, autoprueba 73/73 con el mismo SHA-256 que `selftest-identity-result-a13.json`, `HandoffIntegrity = PASS`, `Identity = FAIL`, `ControlResult = PASS`, objeto candidato `MISSING`, salida `7fa63420…`.
- **Resultado:** `nc1 = SATISFIED_UNDER_I64_IDENTITY_BRIDGE`. La corrida `2376` conserva `CONTROL NOT PASSED`.
- **Riesgo:** el defecto del Controller sigue abierto; ver `task-close-package.md`.
- **Alcance:** nc1 de F2-T2B. Sin cambio del protocolo global ni de la semántica de `Handoff`.

## D-07. Cierre de F2-T2B-NAVIGATION-UI

- **Problema:** el cierre de la tarea correspondía al Coordinator.
- **Decisión:** F2-T2B = CLOSED / PASS sobre `82bd49b6`.
- **Fundamento:** verificación `6b2a` `EXECUTION_VERIFIED` 14/14; nc1 satisfecho bajo el puente (D-06); nc2 `07c1` y nc3 `4df8` CONTROL PASS; CI 42766532521 4/4; T2B-F1, F2 y F3 corregidos y cubiertos por pruebas.
- **Alcance:** cierre de esta tarea; no es F2 PASS. El comportamiento en AutoCAD queda para Smoke-NAV.

## D-08. Smoke-NAV detenido en SN-01 y corrección estrecha F2-T2-HOST-C1

- **Problema:** en AutoCAD real, con el candidato `7bdcbfd9` (ocurrencia `R20261110T064058Z-c883`), Seleccionar y Ver no tienen efecto: dos `*Cancel*`, la selección se pierde y la vista no cambia. Las 1791 pruebas de UI y las 16318 de Core no podían verlo, porque ningún proyecto de prueba ejecuta el Plugin.
- **Diagnóstico** (cuatro builds locales instrumentados, sin commit, con la guarda de procesos; `logs/` y `operator/session-log.md`):
  1. `ExecuteInCommandContextAsync` ejecuta el callback en el comando interno `EXECUTEFUNCTION`, con dos `*Cancel*` que vacían la selección implícita. La selección escrita no sobrevive: es el `UNKNOWN` que el Freeze encargó a Smoke-NAV, y sale negativo.
  2. `SetCurrentView` se llama dentro de una transacción que se cierra sin `Commit`, y la vista se revierte. `ZoomToRack`, que funciona en producción, confirma su transacción.
  3. La devolución del foco no influye (variante sin ella: mismo resultado).
- **Opciones:** (a) dejar F2 sin cerrar y esperar al Owner; (b) corrección estrecha; (c) un comando oculto con `UsePickSet`, prohibido por C-F2T2-02.
- **Decisión:** (b). `F2-T2-HOST-C1`: en `WorkspaceNavigationHost.cs` y `WorkspaceNavigationEffects.cs`, ejecutar la acción en contexto de aplicación, sin comando interno, comprobando que no hay comando en curso y con el documento bloqueado; y aplicar la vista fuera de la transacción de lectura. La ejecuta la sesión con RED, GREEN, verificación local, CI exacta y paquete de cierre delta, con el mismo formato que `F1-T3B-HOST-UI-C1`.
- **Fundamento:** la variante 4 demostró en AutoCAD real Seleccionar (`sslength` = 4, la selección sobrevive), Ver, Centrar, Localizar y la vuelta del foco, sin marcar el dibujo como modificado. La revalidación sigue ocurriendo justo antes de actuar, ahora en un único paso síncrono.
- **Presupuesto:** `attempts` sigue en 3 de 3. Ni se reinicia ni se incrementa: como C1 de F1, no es una corrección delegada de un Worker, sino un hallazgo de hito.
- **Desviación del Freeze que requiere ratificación:** D-07, INV-40 y O4-06 dicen «en contexto de comando». El mecanismo pasa a contexto de aplicación con bloqueo. La intención (documento, sesión, existencia y atribución comprobados justo antes de actuar) se conserva. El Owner debe ratificarlo en la Owner Validation.
- **Alcance:** solo navegación de F2. El candidato `7bdcbfd9` queda como `HISTORICAL_SMOKENAV_STOP_CANDIDATE` y no se reutiliza. Sus evidencias no se reetiquetan.
- **Riesgo:** la navegación deja de pasar por un comando, de modo que no entra en la pila de comandos de AutoCAD. Smoke-NAV sobre el candidato nuevo lo comprueba.

## D-09. Cierre de F2-T2-HOST-C1 y disposición de Smoke-NAV

- **Problema:** cerrar la corrección y disponer el hito, que correspondían al Coordinator.
- **F2-T2-HOST-C1 = CLOSED / PASS** sobre `003be859`. RED `ce48067e`, `85affa68` y `8050f512`; GREEN `cdef2d44` y `003be859`; CI 42777768225 4/4; Core 16320/16320; UI 1792 más 17 omitidas. Una revisión independiente del diff no encontró defectos bloqueantes, y sus dos observaciones se corrigieron antes del candidato.
- **Smoke-NAV = PASS sobre `003be859`** (ocurrencia `R20261110T073451Z-e221`, operada por la sesión): SN-01..SN-07 y SN-10 en PASS, SN-09 medido, SN-06c y la vía de pestañas de SN-10 `NOT_APPLICABLE_UNTIL_F3`.
- **SN-08 = LIMITADA, no cuenta como PASS:** la ejecución síncrona no deja intervalo en el que una petición quede pendiente, así que la carrera no se puede provocar; no se observó efecto cruzado entre documentos. La comprobación de sesión sigue en el host y la cubren las pruebas de T2A.
- **F2 = PASS** con esa limitación y con la desviación de D-08 pendiente de ratificación del Owner. No es Owner Validation.
- **Fundamento:** evidencia real en AutoCAD con el candidato exacto, guarda de procesos sin STOP y perfil `CLEAN`.
- **Riesgo:** el piloto usó dibujos sintéticos con la geometría controlada; la Owner Validation debe repetirlo con racks reales.
- **Alcance:** F2. No integra en `main`.
