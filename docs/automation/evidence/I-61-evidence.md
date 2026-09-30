# I-61 — Evidencia de la unidad (Agent Execution, Model Routing & Prompting Protocol)

Unit / Initiative / Workflow: `I-61` / I-61 / V2. Claim-Id `0e2923de-e1a7-41bf-b7db-50ec84217850`.
Contrato: [I-61-protocolo-ejecucion-agentes.md](../../initiatives/I-61-protocolo-ejecucion-agentes.md). Decisiones: [I-61.md](../decisions/I-61.md).

Estado de esta evidencia: **G0 (reclamo y bootstrap)**. No hay Discovery sustantivo, Freeze, piloto, Candidato ni GATE PASS. Lo no ejecutado
figura como **PENDIENTE**.

## 1. Base de reclamo y clasificación de transición

| Hecho | Valor | Fuente |
|---|---|---|
| Base (`origin/main`) | `95690c28dc6268e61dff32a0cbc33cc9fde3d47f` (revalidada en el reclamo; `main` = `origin/main`, árbol limpio, sin operaciones Git) | `git fetch` + `git rev-parse` |
| Commit de reclamo | `21af80e81a3f96649fa6e649374ec8591548a082` (vacío; fecha 2026-09-30T15:44:44-06:00) | `git log` |
| Primer push aceptado (reclamo) | `architecture/protocolo-ejecucion-agentes` nueva en `origin`, sin force | salida de `git push -u` |
| Rama / worktree | `architecture/protocolo-ejecucion-agentes` / `.claude/worktrees/architecture-protocolo-ejecucion-agentes` (del repo principal) | `git worktree list` |
| ID | I-61 libre al reclamar: mayor ID en `origin/main` = I-60; sin I-61 en ramas, tags ni documentos | `git grep`, `git ls-remote` |
| `WORKFLOW_V2_EFFECTIVE_SHA` | `8a021fb67c16dfccd6afc18448ea7e6a71a32364`, ancestro de la base | `git merge-base --is-ancestor` (D0) |
| Fin de la pausa | `integration/I-56` (tag anotado): `Claim pause … end=2026-09-17T22:30:00Z` | `git cat-file -p integration/I-56` (D0) |
| Clasificación | **V2, T4** (posterior probado, base con el SHA efectivo, sin pausa) | [WORKFLOW](../../WORKFLOW.md) §11.3 |

## 2. Fuentes recibidas (entrada de G0)

Transferencia directa de texto (orden C61-G0-07 del Coordinator), escrita **fuera del repositorio** en `D:\Documentos\Codex\I-61-G0-inputs-texto\`.
`SHA256SUMS.txt` de esa carpeta es un manifiesto de **esta transferencia mínima** (2 archivos); no afirma haber recibido ni verificado el ZIP anterior.

| Archivo | Bytes | SHA-256 | Verificación |
|---|---:|---|---|
| `01-owner-mandate.original.txt` | 16 415 | `2cb2770c8c4044cf34862942b88811defc4615376df5edbd75343791a98d098e` | coincide con el manifiesto literal; UTF-8 sin BOM, LF, sin LF final |
| `02-coordinator-decisions.md` | 6 089 | `77681380ce7664c0274799cd824e82914c7c85be77f6e58e3da19ee550e916e9` | coincide con el manifiesto literal; UTF-8 sin BOM, LF, un LF final |

- Versionado: **solo** el mandato, como [`I-61-owner-mandate.txt`](../decisions/I-61-owner-mandate.txt) (una copia; mismo SHA-256 que arriba,
  comprobado sobre el blob de Git). El archivo 02 no se versiona: sus decisiones constan en [I-61.md](../decisions/I-61.md) §2 con atribución al Coordinator.
- Cadena de custodia: Owner → Coordinator → Executor; **sin verificación independiente contra el adjunto original**.
- Los borradores históricos (esquemas, transcripción de catálogo) no se recibieron ni se versionan; no son Freeze ni aprobación de diseño.
- Nota de fin de línea: con `core.autocrlf=true` un checkout en Windows puede convertir el `.txt` a CRLF; el hash canónico es el del blob (LF).

## 3. Coordinación de escritura de `docs/ROADMAP.md` frente a I-52

| Campo | Valor |
|---|---|
| Canal | mensajería entre sesiones locales (sesión de I-61 → sesión «I - 52»); solicitud `msg_id 2fb9ce71-0192-4b3c-ac4f-884b3fc1a0fe` |
| Respuesta | **ACUSE** de la sesión orquestadora de I-52 (única escritora de su rama; afirma autoridad sobre sus escrituras de ROADMAP): no escribe, rebasa ni publica cambios que toquen `docs/ROADMAP.md` hasta el «RELEASE» |
| Archivo y alcance | solo `docs/ROADMAP.md`; el resto del trabajo de I-52 continúa |
| INICIO | el mensaje de acuse (recibido antes del reclamo; el canal no registra hora exacta → no la invento) |
| FIN | mensaje «RELEASE» de I-61 por el mismo canal, enviado tras publicar el bootstrap (`msg_id e6660bd2-5704-4188-9bcf-cc2f9e92cd7d`); no esperó a la CI. La entrega quedó encolada en la sesión de I-52; no hay acuse de lectura (el canal no lo reporta) |
| Estado observado de I-52 antes del reclamo | tip `65e465a71e8e91d60fc9315dace1aac55c0db5de`, 0 detrás y 140 delante de la base; modifica `docs/ROADMAP.md` (líneas 471 y 478 de la base) |
| Compatibilidad textual | una fusión de ensayo de una fila en la tabla Engineering Productivity con la rama de I-52 no produjo conflicto. **No** se usa como prueba de exclusividad (C61-G0-03) |

Observación sin acción (C61-G0-05): la rama de I-52 edita en ROADMAP la fila de I-57; pendiente de procedencia/autorización, no auditada ni corregida.

## 4. Desviación local del preflight (C61-G0-04)

El Executor creó en el repositorio principal un commit **sin ref** `a10f1c0954207a17bc8fd64d1ef5a517ca25b665` para ensayar la fusión descrita en §3,
con un índice temporal que luego borró. Es un efecto local adicional al índice transitorio y una desviación del preflight de solo lectura; no es claim,
commit de producto ni evidencia de exclusividad. No se creó ref ni se ejecutó `gc`/`prune`; no se repitió. Observación atribuida al Executor.

## 5. Bootstrap (contenido versionado de G0)

Archivos nuevos: el contrato `docs/initiatives/I-61-protocolo-ejecucion-agentes.md`; `docs/automation/decisions/I-61.md` y
`docs/automation/decisions/I-61-owner-mandate.txt`; `docs/automation/state/I-61.yml`; este archivo. Archivo modificado: `docs/ROADMAP.md`
(una fila en Engineering Productivity). **Nada más**: sin cambios en normas globales, `HANDOFF`, índice ADR, `src/`, `tests/`, `assets/`, CI ni configuración.
`automation.enabled: false`; las banderas `requires_*: false` no conceden exenciones de Owner Validation ni AutoCAD.

## 6. Validación de G0

- **Comprobación documental:** `git diff --name-only` del bootstrap contra la base: 6 archivos, todos bajo `docs/` (0 fuera de `docs/`).
- **CI exacta del bootstrap** ([WORKFLOW](../../WORKFLOW.md) §4.5.2, commit documental): corrida de `event=push` **36781748452** sobre
  `head_sha` = `c701ff8f8cd3ee58f7dfab529e092d1f5e18190b`: conclusión `success`, 4/4 jobs `success` (Build UI, UI Tests, Tests Domain + Application,
  Build Plugin without AutoCAD). La del commit de reclamo (`21af80e8`, corrida 36781382840): `success`.
- La CI del commit de seguimiento de este registro (§7) es propia de ese SHA y se informa al Coordinator; no se copia aquí (evita la recursión).
- Suites Core/UI locales, builds del Plugin y Owner Validation: **no aplican a G0** (sin cambios de producto, `src/`, `tests/` ni `assets/`);
  no se ejecutaron.

## 7. Registros posteriores a la publicación del bootstrap

- Commit de bootstrap: `c701ff8f8cd3ee58f7dfab529e092d1f5e18190b` (push aceptado sin force sobre el reclamo `21af80e8`).
- `RELEASE` enviado a la sesión de I-52 tras ese push (§3). `origin/main` seguía en `95690c28…` (no avanzó por G0).
- Este registro se publica en un commit de seguimiento solo documental.

## 8. Métricas, conformidad, Owner Validation y tag

Métricas: UNKNOWN (sin gates funcionales). Conformidad: no aplica todavía. Owner Validation: no aplica a G0. Tag de integración: no existe
(`integration/I-61` se crea solo tras la integración).
