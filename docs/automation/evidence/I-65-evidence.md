# I-65 — Evidencia de la unidad (Structural Analysis Interop Foundation + RAM Elements Adapter)

Unit / Initiative / Workflow: `I-65` / I-65 / V2. Claim-Id `0b4aad79-068e-4a57-a2c2-dc8f23406778`.
Contrato: [I-65-interop-analisis-estructural.md](../../initiatives/I-65-interop-analisis-estructural.md).
Decisiones: [I-65.md](../decisions/I-65.md).

Estado de esta evidencia: G0 (reclamo y bootstrap). Las secciones son registros de su momento y no se reescriben
([WORKFLOW](../../WORKFLOW.md) §11.4). Los hechos posteriores al commit que publica este archivo (su CI exacta, el RELEASE de la
ventana) se registran en un commit posterior: un archivo no puede citar el SHA del commit que lo contiene.

## 1. Base de reclamo y clasificación de transición

| Hecho | Valor | Fuente |
|---|---|---|
| Base (`origin/main`) | `bb0d5522e8411f66a51fdfb3f1f0d0514b737453` (recién obtenida antes del reclamo; coincide con el baseline del mandato) | `git fetch` + `git rev-parse origin/main` |
| Commit de reclamo | `32a8475689c9ef8a44837576f3c6f61abd34b07a` (vacío; 2026-10-10T14:58:09-06:00) | `git log` |
| Primer push aceptado (reclamo) | `architecture/interop-analisis-estructural` nueva en `origin`, sin force | salida de `git push -u` |
| Rama / worktree | `architecture/interop-analisis-estructural` / `%USERPROFILE%\.codex\worktrees\architecture-interop-analisis-estructural` (fuera del árbol versionado; distinto del principal del Owner, que no se tocó) | `git worktree list` |
| `WORKFLOW_V2_EFFECTIVE_SHA` | `8a021fb67c16dfccd6afc18448ea7e6a71a32364` (merge que activa V2; el trailer `Workflow-V2-Normative: I-56` está en `afd1077c`); ancestro de la base | `git log --grep`, `git merge-base --is-ancestor` |
| Clasificación | **V2, T4** (unidad nueva posterior, base con el SHA efectivo, sin pausa). T7/T8 evaluados negativos: no consume diseño V1 de I-49/I-52/I-55 ni pertenece a ellas | [WORKFLOW](../../WORKFLOW.md) §11.3 |

## 2. Fuentes recibidas

| Fuente | Bytes | SHA-256 de transporte | Nota |
|---|---:|---|---|
| Mandato del Owner (pegado en el chat de la sesión Coordinator, 2026-10-10T20:39:21Z) | 25 549 | `AE1A04127AA3E9E9E8AA7B26D2329DFD48A67A9DE9B1F285B51EAEB106569E56` | Extraído de la transcripción local (contenido entre las etiquetas de texto pegado, sin las etiquetas), no de una copia transcrita. UTF-8 sin BOM, solo LF, sin LF final. Versionado como [`I-65-owner-mandate.original.txt`](../decisions/I-65-owner-mandate.original.txt) |

**Blob de Git del mandato:** la identidad durable es el blob versionado. Con `core.autocrlf=true`, un checkout en Windows puede convertir
el archivo a CRLF en disco; el SHA-256 del blob (LF) es el que se compara con el de transporte.

## 3. Preflight (2026-10-10, 20:39Z–20:58Z)

- `origin/main` = `bb0d5522…`, igual al último conocido por el Master Orchestrator. `main` local del worktree humano: `95690c28`
  (90 commits detrás, a propósito; no se tocó).
- Ramas remotas: `main`, `architecture/portabilidad-coordinador-principal` (I-62, suspendida), `architecture/workspace-persistente-rackcad`
  (I-64 / ID30) y `feature/rackmirror-espejo-semantico` (I-52). Tags `integration/*` hasta I-63; ningún tag ni rama de I-65.
- `git log --all --grep='I-6[5-9]'`: vacío. Ningún archivo `docs/**/I-65*` en `origin/main` ni en las ramas remotas. Carpetas de paquete
  `D:\IDs`: I-62, I-63, I-64 (sin I-65). → **I-65** (CD-I65-G0-01).
- Sesión Coordinator: `claude-opus-5-5` / `xhigh` (`get_session("self")`). Perfil requerido PRINCIPAL_COORDINATION → Long-horizon →
  Frontera → `claude-opus-5-5` / `xhigh` según [routing.md](../agent-execution/routing.md) §3 y el
  [catálogo](../agent-execution/model-catalog.md) (verificado 2026-09-30; STALE desde 2026-12-29). `CONFIGURATION_STATUS = MATCH`.
- **Entorno de RAM Elements (inspección de solo lectura, sin ejecutar el programa):** registro de Windows → «RAM Elements 2026»,
  `DisplayVersion 26.00.01.015`, instalado 2026-08-28 en `C:\Program Files\Bentley\Engineering\RAM Elements\`; `RAMElements.exe`
  `FileVersion 26.00.01.15`. La caracterización de las superficies de integración es materia de Discovery.

## 4. Ventana de `docs/ROADMAP.md`

Solicitud enviada por el canal entre sesiones a las tres sesiones paralelas (2026-10-10T20:49:14Z, 20:49:18Z y 20:49:22Z): archivo
`docs/ROADMAP.md`, solo en la rama de I-65; alcance = una fila propia al final de la tabla de la Fase 6, tras I-38; intervalo por eventos
desde el acuse hasta el RELEASE de I-65 al publicar el bootstrap o al abortar.

| Sesión | Llegada del acuse (UTC) | Texto literal |
|---|---|---|
| «I - 64» (I-64 / ID30) | 2026-10-10T20:49:44Z | «ACUSE I-65 ROADMAP: no escribiré docs/ROADMAP.md hasta tu RELEASE» + motivo: sin escrituras de ROADMAP en curso ni planeadas; su fila está en la Fase 6 tras I-63 (punta `d5bdcb7a`, rebasada sobre `bb0d5522`); frontera ID30 anotada |
| «I - 62» (I-62, suspendida) | 2026-10-10T20:50:18Z | «ACUSE I-65 ROADMAP: no escribiré docs/ROADMAP.md hasta tu RELEASE» + el acuse no levanta la suspensión de I-62 ni concede nada más |
| «I - 52» (I-52) | 2026-10-10T20:57:52Z | «ACUSE I-65 ROADMAP: no escribiré docs/ROADMAP.md hasta tu RELEASE» |

El reclamo se hizo después del tercer acuse. Compatibilidad textual de la fila con las ramas paralelas: comprobada con
`git merge-tree` antes del push del bootstrap (se informa con el commit); no se usa como prueba de exclusividad (C61-G0-03).

## 5. Bootstrap

Archivos nuevos: el contrato `docs/initiatives/I-65-interop-analisis-estructural.md`; `docs/automation/decisions/I-65.md` y
`docs/automation/decisions/I-65-owner-mandate.original.txt`; `docs/automation/state/I-65.yml`; este archivo. Archivo modificado:
`docs/ROADMAP.md` (una fila en la tabla de la Fase 6, tras I-38). **Nada más**: sin cambios en normas globales, HANDOFF, índice de ADR,
FOUNDATIONS, `src/`, `tests/`, `assets/`, `eng/`, CI ni configuración. `automation.enabled: false`.

## 6. Validación de G0

- Comprobación documental: `git diff --name-only` del bootstrap contra la base, todo bajo `docs/`; se informa con el commit.
- CI exigible: la corrida `push` del SHA exacto del bootstrap ([WORKFLOW](../../WORKFLOW.md) §4.5.2, commit documental); es un hecho
  posterior al commit.
- Suites Core/UI locales, builds del Plugin y Owner Validation: **no aplican a G0** (sin cambios de producto); no se ejecutaron.

## 7. Participantes invocados antes del bootstrap

- Dos subagentes de solo lectura con el modelo heredado de la sesión (CD-I65-G0-08): investigación de fuentes oficiales de Bentley
  (web) y caracterización de la fuente estructural de RackCad sobre `origin/main`. No escribieron en el repositorio. Sus resultados
  alimentan Discovery y se citan allí.
- Ningún Controller, Worker ni Architect se invocó en G0.

## 8. Métricas, conformidad, Owner Validation y tag

Métricas: UNKNOWN (sin gates funcionales). Arquetipo inicial: NEW ARCHITECTURE (mandato; M-07 previsto). Conformidad: no aplica todavía.
Owner Validation: por determinar; no aplica a G0. Tag de integración: no existe.

## 9. Hechos posteriores al bootstrap (registrados en F0)

- CI `push` del reclamo `32a8475689c9ef8a44837576f3c6f61abd34b07a`: corrida `38085815893`, **success 4/4** (Tests Domain+Application,
  Build UI, UI Tests, Build Plugin without AutoCAD), `head_sha` exacto.
- CI `push` del bootstrap `214c171dfa886acf1dd7aa461283b7e9a1d5158c`: corrida `38086020592`, **success 4/4**, `head_sha` exacto.
- RELEASE de la ventana de `docs/ROADMAP.md` enviado tras el push del bootstrap a «I - 64» (2026-10-10T21:01:28Z), «I - 62» (21:01:30Z)
  e «I - 52» (21:01:31Z). Ventana cerrada; I-65 no vuelve a escribir ROADMAP hasta el cierre.
- `git merge-tree --write-tree` del árbol del bootstrap contra `origin/architecture/workspace-persistente-rackcad` (`d5bdcb7a`),
  `origin/architecture/portabilidad-coordinador-principal` (`362bec12`) y `origin/feature/rackmirror-espejo-semantico` (`fb6b5648`):
  sin conflicto en los tres (no acredita exclusividad).

## 10. F0 — Discovery y Proposal V1

- Discovery: [I-65-discovery.md](../../initiatives/I-65-discovery.md); Proposal: [I-65-proposal-v1.md](../../initiatives/I-65-proposal-v1.md);
  paquete del Architect: [I-65-architect-package-v1.md](../../initiatives/I-65-architect-package-v1.md); sonda P1 (diseño, **no ejecutada**):
  [ram-probe-p1.py](I-65-f0/ram-probe-p1.py).
- Participantes de F0 (CD-I65-G0-08): cuatro subagentes de solo lectura con el modelo heredado (`claude-opus-5-5`): fuentes oficiales
  web de Bentley; fuente estructural de RackCad sobre `bb0d5522`; «RAM Elements Python API User Manual.pdf» instalado; «OpenRE
  Manual.pdf» instalado. Ninguno escribió en el repositorio ni ejecutó RAM Elements.
- RAM Elements **no se ejecutó** (CD-I65-G0-06). Todas las afirmaciones sobre el entorno son de inspección de archivos y registro.

## 11. F0 — Architect, ronda 1, y Proposal V2

- CI `push` del commit de F0 `0f4bc8849ddc0e491231d841cf43b56badba1e98`: corrida `38087065661`, **success 4/4**, `head_sha` exacto.
- Ronda 1 del Architect sobre `0f4bc884`: **CHANGES REQUIRED** (13 REQUIRED, 6 OPTIONAL). Modo `SAME-SESSION ROLE` (subagente,
  contexto independiente, `claude-opus-5-5`). Texto íntegro en [architect-r1-verdict.md](I-65-f0/architect-r1-verdict.md) con su
  SHA-256.
- Respuesta: Proposal V2 completa, borrador de ADR-0050, sonda P1 v2, adenda §13 de Discovery y paquete de la ronda 2.
- Hallazgo propio al corregir P1: un `SystemExit` dentro del intérprete embebido de RAM podría terminar la aplicación anfitriona
  (`PyErr_Print` trata `SystemExit` saliendo del proceso); la puerta de P1 v2 no lo usa.

## 12. F0 — Architect, ronda 2, y Proposal V2.1

- CI `push` de `8f9ac48ce135ef7cff08feb25f0ec19cc4c6aa7a` (Proposal V2): corrida `38088757141`, **success**, `head_sha` exacto.
- Ronda 2 del Architect sobre `8f9ac48c` (mismo revisor): **CHANGES REQUIRED**. Cerrados A65-V1-02..11, 13 y 14..19; A65-V1-01 `OPEN —
  BLOCKED BY P1` (correcto por diseño); A65-V1-12 abierto por un residual; nuevos A65-V2-01..04 REQUIRED y 05..09 OPTIONAL. Texto
  íntegro en [architect-r2-verdict.md](I-65-f0/architect-r2-verdict.md) con su SHA-256.
- Respuesta: Proposal V2.1 completa (disposición en su §14), sonda P1 v3, ADR-0050 ajustado y paquete de la ronda 3.

