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
