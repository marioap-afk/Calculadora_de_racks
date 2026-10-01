# ADR-0046: Protocolo de ejecución delegada de agentes (Controller Codex, Worker y entrega estructurada)

- **Estado:** **aceptado** (Owner, 2026-10-01; registro en `docs/automation/decisions/I-61.md` §17)
- **Fecha:** 2026-09-30
- **Decisores:** Owner del repositorio (aceptado el 2026-10-01). Redactado por la sesión de I-61 (Claude) en `SAME-SESSION ROLE`.
- **Iniciativa relacionada:** I-61 — Agent Execution, Model Routing & Prompting Protocol (`architecture/protocolo-ejecucion-agentes`)
- **Proposal:** la Proposal congelada de I-61; su ruta, commit y blob se registran en `docs/automation/decisions/I-61.md` al congelarla. Versión en revisión mientras este ADR es
  `propuesto`: `docs/initiatives/I-61-proposal-v9.md`.

## Contexto

Hoy el desarrollo delega a mano. El Coordinator redacta órdenes que un humano traslada entre sesiones, y la elección de modelo y effort es informal. Las entregas son prosa, sin una forma
legible por máquina atribuible por SHA exacto ([Discovery de I-61](../initiatives/I-61-discovery.md) §§1-4, MEASURED). El mandato del Owner pide un protocolo estable: un Coordinator delega
gates a un Execution Controller (Codex), que enruta executor, modelo, effort, perfil y transporte, y delega en workers Claude o Codex con entregas estructuradas, exact-SHA y STOP, sin mover
la autoridad del Coordinator, del Architect ni del Owner.

Los hechos medidos acotan la solución (Discovery §17, §18):

- `codex exec` funciona como Controller **de solo lectura** con stdin cerrado y el `pwsh` del runtime de Codex (MEASURED).
- Un worker Codex con escritura, commit y push **no está demostrado (UNKNOWN)**: en una réplica con worktree anidado, el sandbox `workspace-write` rechazó lanzar procesos.
- El CLI de Claude no está autenticado (MEASURED). Los subagentes de la sesión leen y heredan modelo y effort (MEASURED); que apliquen un modelo y un effort solicitados está pendiente de
  medir.
- Un código de salida 0 con un JSON conforme puede no reflejar trabajo hecho (MEASURED).
- Invocar Codex en un directorio nuevo modificó la configuración global (MEASURED, DEV-G1C-01).

## Decisión

1. **Cambio del alcance de autoridad (OWN-L).** La ejecución delegada no tenía dueño. Este ADR **amplía** el alcance de `AUTOMATION_PLAN.md`, que la Proposal V4 de I-56 (P-17) limitaba a
   su ejecutor, y añade la fila correspondiente a `WORKFLOW.md` §10. Es materia OWNER-RESERVED y su efecto depende de la aceptación de este ADR y de la integración.
   - `AUTOMATION_PLAN.md` gobierna la **ejecución delegada bajo orden explícita del Coordinator** en una sección propia, que contiene el texto normativo de sus reglas: roles y
     declaraciones; precondiciones operativas del relevo; propiedad exclusiva; regla de conteo, topes y precedencia; regla multiseñal y comprobaciones obligatorias; correspondencia de las
     condiciones STOP; y custodia como aplicación de la evidencia por unidad de `WORKFLOW.md`. No es el ejecutor nocturno: no selecciona iniciativas ni requiere activación ni
     `automation.enabled: true`. Las frases de ese plan que limitan su alcance al ejecutor se amplían en consecuencia.
   - Del resto de ese plan aplican exactamente las secciones y cláusulas que fija su sección de ejecución delegada, que copia fielmente la tabla «Secciones de AUTOMATION_PLAN y su
     aplicación a la ejecución delegada» de la Proposal congelada.
     **La ejecución delegada no abre ni actualiza Pull Requests ni exige activación.**
   - La ejecución delegada lee en la revisión que fija su orden los documentos de su propia unidad y los cambios normativos que esa orden declara por sección; toda otra autoridad la lee
     en la punta registrada de `origin/main`, y una rama no puede rebajar una regla global.
   - `WORKFLOW.md` gobierna Git y el relevo; su relevo entre sesiones no cambia.
   - `PROMPT_TEMPLATES.md` gobierna la composición de prompts (sección «Delegación de ejecución»).
   - `AGENTS.md` sigue gobernando la evidencia.
   - Los documentos de `docs/automation/agent-execution/` son **subordinados**: no crean requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la
     autoridad del dominio y el conflicto se eleva como STOP.
2. **Roles y declaraciones.**
   - El Worker solo declara `IMPLEMENTATION_COMPLETE`, `PARTIAL` o `BLOCKED`.
   - El Controller solo declara `EXECUTION_VERIFIED`, `EXECUTION_REWORK_REQUIRED` o `EXECUTION_BLOCKED`. STOP se expresa con un campo de disposición separado.
   - Un trabajo delegado solo vuelve al Coordinator como terminado con `EXECUTION_VERIFIED` sobre el SHA de la entrega o, tras un rebase registrado, su imagen; el Coordinator puede
     rechazarlo. Solo el Coordinator declara GATE PASS,
     y solo el workflow declara Candidato, cierre o integración. Codex no es el Master Orchestrator.
3. **Relevo.** Cada invocación de un participante externo es un relevo de WORKFLOW §3. Hay un solo participante operando a la vez. Un Worker termina (commit, push y entrega) antes de que el
   Controller verifique. Un paquete de delegación nunca amplía el contrato de gate del Coordinator: se comprueba antes de invocar al Worker.
4. **Enrutamiento estable separado de un catálogo mutable.**
   - Primero la clase de tarea, luego la elegibilidad por celda modelo × transporte × capacidad (publicada, instalada, autenticada, invocación probada y consumo cubierto por una suscripción o
     cuota existente, acreditado por una fuente oficial o por una invocación medida; lo desconocido no es elegible) y al final el nivel más bajo adecuado. La regla es la misma para el
     Controller y para los workers.
   - Effort semántico (`Routine`, `Balanced`, `Deep`, `Long-horizon`, `Maximum`); escalado con razón registrada; enrutamiento hacia abajo permitido; exigencia del enrutamiento declarada en
     cada delegación.
   - El catálogo de modelos es **no normativo**, registra el tipo de fuente de cada dato, se fecha y se reverifica con la documentación oficial.
5. **Esquemas legibles por máquina**, JSON Schema 2020-12 estrictos: contrato de gate, delegación, entrega de worker, verificación del Controller y registro de relevo
   (`rackcad-*/v1`).
6. **Verificación fail-closed y multiseñal.** Ni stdout, ni un código de salida, ni un JSON conforme bastan. `EXECUTION_VERIFIED` exige que pasen todas las comprobaciones obligatorias.
   **La señal de pruebas de la verificación del Controller es la CI de `push` del SHA exacto; no es evidencia de gate y no sustituye ninguna clase de AGENTS.md.** Precedencia
   STOP > BLOCKED > REWORK.
7. **Presupuesto único de reintentos.** `attempts` del estado canónico, sin reinicios por cambio de modelo, rol o sesión. Un máximo de tres correcciones por clase de fallo dentro de una
   tarea, acotado por los intentos restantes; topes también para las reejecuciones tras un bloqueo y para el número de invocaciones; análisis obligatorio antes de corregir un defecto
   nuevo no relacionado.
8. **Transporte.** La jerarquía y los usos de Computer Use son los del mandato, por referencia: Computer Use es fallback y nunca el bus normativo. Esquemas, relevo y verificación son
   idénticos en todo nivel de transporte. El tráfico transitorio va a `artifacts/orchestration/`, ignorado por Git, y lo que respalda una decisión se versiona en la evidencia de la unidad.
9. **Nivel A:** documentación, esquemas y piloto real. Sin plataforma ni scripts operativos hasta que la evidencia lo justifique.

## Alternativas consideradas

- **Computer Use como bus entre agentes** — frágil y no legible por máquina; el mandato lo reserva como fallback para los usos que enumera.
- **Un único modelo potente para todo** — es justo el problema que motiva el mandato. No se fija modelo; el proveedor solo está fijado para el Controller, por mandato.
- **Autoridad paralela (un `ORCHESTRATION.md` normativo)** — contradice la autoridad única por dominio (WORKFLOW §10; Proposal V4 de I-56, P-17 y P-24).
- **Tráfico en `.agent/`** — no está ignorado por Git (MEASURED); `artifacts/` ya lo está.
- **Controller que también escribe** — invierte la autoridad y rompe la propiedad exclusiva de escritura, con independencia de lo que permita el sandbox.
- **Nivel B o C de entrada** — sin evidencia de piloto que justifique mantener scripts o una plataforma.

## Consecuencias

- **Positivas:**
  - delegación reproducible y atribuible por SHA;
  - modelo y effort explícitos y revisables;
  - prompts sin copiar normas;
  - fallos de transporte detectados en vez de asumidos;
  - el Coordinator conserva la decisión de gate.
- **Negativas y costos aceptados:**
  - la invocación del Worker la ejecuta la sesión responsable como relevo (el Controller de solo lectura no puede invocar en este entorno);
  - la independencia de la verificación es parcial cuando el Worker es un subagente de la sesión responsable;
  - el catálogo exige reverificación periódica;
  - las recetas de invocación dependen del entorno (Windows, `unelevated`).
- **Vigilar:**
  - la frescura del catálogo y los retiros anunciados;
  - los efectos laterales de Codex sobre `~/.codex/config.toml`: un cambio de su hash detiene las invocaciones;
  - el consumo de las invocaciones bajo la configuración del Owner;
  - que la sección nueva de PROMPT_TEMPLATES no crezca hasta repetir normas.

## Referencias

- Mandato del Owner: `docs/automation/decisions/I-61-owner-mandate.txt`.
- Decisiones de I-61: `docs/automation/decisions/I-61.md` (§9.1, cláusulas del Owner; §§10-12).
- Discovery: `docs/initiatives/I-61-discovery.md`.
- Documentación oficial de los proveedores consultada el 2026-09-30 (Discovery §13).
- ADR relacionados: ADR-0001 (ramas por iniciativa) y ADR-0045 (Workflow V2, «Autoridad por dominio»).
