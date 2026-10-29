# I-66 — Handoff del QH para el Principal siguiente (piloto R1a de I-62, AT-3)

Este documento es el handoff publicado en el QH de I-66 (decisiones de I-62 §107, punto 4). Lo lee la sesión nueva e independiente que el Owner abra
para continuar I-66 («B»). No contiene la respuesta esperada: B la reconstruye desde el repositorio. **I-61 y WORKFLOW son la única autoridad de I-66**;
los registros en sombra de `docs/automation/evidence/I-66-R1/` solo añaden comprobaciones de STOP.

## 1. Reglas de aislamiento

- Usa solo este repositorio y Git (rama `fix/placedclone-copia-anotaciones` de `origin`, más las revisiones que citan sus documentos), con sus documentos
  (`CLAUDE.md`, `AGENTS.md` y lo que enlazan). No leas transcripciones ni memoria de otras sesiones, carpetas temporales ni otros worktrees.
- Tus dependencias técnicas (Git, `gh`, el SDK de .NET, PowerShell, Python) se enumeran en `inputs` con la clase TECHNICAL.
- Un hecho que el estado canónico no establezca se declara `UNKNOWN`. No lo deduzcas ni lo inventes.

## 2. Autoverificación

Antes de decidir, observa tu sesión (`get_session("self")`: modelo y effort) y haz tu preflight `rackcad-preflight/v1` para la acción RESUME_DECISION
según `docs/automation/agent-execution/README.md` §12-§13 en la revisión `41ccfdcf` de I-62 (`git show 41ccfdcf:docs/automation/agent-execution/README.md`,
tras `git fetch origin architecture/portabilidad-coordinador-principal`). El preflight CUSTODY de A, en `autoverificacion/preflight-1.json`, sirve de
ejemplo de forma. Si tu configuración queda en BELOW_REQUIRED o UNKNOWN, dilo y detente.

## 3. Reconstrucción y decisión de reanudación

Reconstruye desde Git el último punto durable de `docs/automation/evidence/I-66-R1/state.v2.yml`, el commit que lo contiene, las decisiones
(`docs/automation/decisions/I-66.md`), el Freeze, el Discovery y el trabajo hecho. Escribe un único objeto JSON conforme a
[`response.r1.schema.json`](response.r1.schema.json), con:

- `facts`: los hechos de la unidad en su último punto durable;
- `decision`: la siguiente decisión del protocolo;
- `inputs`: cada entrada que leíste, con su clase (CANONICAL, TECHNICAL o AUTOMATIC) y su SHA-256 cuando sea un archivo;
- `preflight`: tu preflight para RESUME_DECISION.

## 4. Publicación antes del oráculo

1. Guarda la respuesta como `docs/automation/evidence/I-66-R1/reanudacion/B-response.json` y tu preflight como
   `docs/automation/evidence/I-66-R1/reanudacion/B-preflight-resume.json`. **No cambies** `state.v2.yml` ni ningún otro archivo.
2. Haz un commit solo con esos dos archivos, con el trailer `R1a-Session: <tu sessionId>`, y empújalo a `origin` sin force.
3. Avisa a la supervisión (la sesión «I - 62»; búscala con `ListAgents`) con el SHA del commit y el SHA-256 de la respuesta, calculado como
   SHA-256 de `json.dumps(respuesta, sort_keys=True, ensure_ascii=False)` en UTF-8. Si no puedes avisarla, díselo al Owner.
4. Detente y espera el resultado de la comparación mecánica. El oráculo está sellado fuera del repositorio; su SHA-256 se publica en la evidencia de
   I-62 (rama `architecture/portabilidad-coordinador-principal`) antes de que empieces.

## 5. Después de la comparación

- **Comparación FAIL, o cualquier STOP:** no tomes la custodia. Lo decide el Coordinator.
- **Comparación PASS:** toma la custodia con un QR ORDINARY según la designación de las decisiones §8 (preflight CUSTODY, binding del Principal B,
  aceptación con su marcador y `state.v2.yml` en `record_version` 4) y continúa por trabajo directo hasta el gate siguiente (Freeze §8), siempre dentro de
  lo que I-61 y WORKFLOW permitan.
