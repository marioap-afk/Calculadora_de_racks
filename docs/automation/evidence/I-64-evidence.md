# I-64 — Evidencia de la unidad (ID30, Persistent RackCad Workspace)

Unit / Initiative / Workflow: `I-64` / I-64 / V2. Claim-Id `614371d5-441f-4f14-bac9-f97017105610`.
Contrato: [I-64-workspace-persistente-rackcad.md](../../initiatives/I-64-workspace-persistente-rackcad.md).
Fuentes: [brief del Owner](../../initiatives/I-64-owner-brief.txt) y [confirmación D0-R3](../../initiatives/I-64-coordinator-confirmation-d0-r3.txt).

Estado de esta evidencia: F0 / tarea D0 (reclamo y bootstrap). Las secciones son registros de su momento y no se reescriben
([WORKFLOW](../../WORKFLOW.md) §11.4). Los hechos posteriores al commit que publica este archivo (su CI exacta, el RELEASE de la
ventana) se informan al Coordinator y se registran en un commit posterior: un archivo no puede citar el SHA del commit que lo contiene.
Etiquetas: MEASURED (medido en este host), RECONSTRUCTED (leído de documentos o registros), INFERENCE, UNKNOWN.

## 1. Base de reclamo y clasificación de transición

| Hecho | Valor | Fuente |
|---|---|---|
| Base (`origin/main`) | `819955d61a6da4c811a11fbd11b5dca13f634b7c` (recién obtenida antes del reclamo) | MEASURED: `git fetch` + `git rev-parse origin/main` |
| Commit de reclamo | `6ea9b7e8002eaa69e17fbcf677e7855f40315ec4` (vacío; 2026-10-01T10:01:36-06:00) | MEASURED: `git log` |
| Primer push aceptado (reclamo) | `architecture/workspace-persistente-rackcad` nueva en `origin`, sin force | MEASURED: salida de `git push -u` |
| Rama / worktree | `architecture/workspace-persistente-rackcad` / `%USERPROFILE%\.codex\worktrees\architecture-workspace-persistente-rackcad` (fuera del árbol versionado; distinto del principal del Owner) | MEASURED: `git worktree list` |
| `WORKFLOW_V2_EFFECTIVE_SHA` | `8a021fb67c16dfccd6afc18448ea7e6a71a32364`: único commit con el trailer `Workflow-V2-Normative: I-56` en la historia de `origin/main` (`afd1077c`); primer merge first-parent cuyo segundo padre lo alcanza y el primero no (§11.2); coincide con el tag `integration/I-56` (objeto `41b1835b`); ancestro de la base | MEASURED: `git log --grep`, `git rev-list --first-parent --merges`, `git merge-base --is-ancestor` |
| Fin de la pausa de activación | `integration/I-56`: `Claim pause … end=2026-09-17T22:30:00Z` | MEASURED: `git cat-file -p integration/I-56` |
| Clasificación | **V2, T4** (posterior probado, base con el SHA efectivo, sin pausa). T7/T8 negativos: no consume diseño V1 de I-49/I-52/I-55 ni pertenece conceptualmente a ellas | [WORKFLOW](../../WORKFLOW.md) §11.3 |

Los encabezados de WORKFLOW §11, INITIATIVE_LIFECYCLE, PROMPT_TEMPLATES, AUTOMATION_PLAN y FOUNDATIONS siguen diciendo «not
effective»; la clasificación se apoya en §11.2 y en el tag, no en esos encabezados (orden D0, punto 2).

## 2. Fuentes recibidas

Cadena de custodia: Owner → Coordinator → sesión responsable, como archivos locales en `D:\IDs\I-64\` (fuera del repositorio).

| Fuente | Bytes | SHA-256 medido en el host | Nota |
|---|---:|---|---|
| `00-owner-brief.txt` (brief del Owner) | 18 668 | `f0be18e7a7f408be9f4bc4a66dd7c9a1e47e920124311e87b3ae77dac31df660` | UTF-8 sin BOM, solo LF, sin LF final. Coincide con el valor de la orden D0 y de D0-R3. Copia única byte-idéntica: [`I-64-owner-brief.txt`](../../initiatives/I-64-owner-brief.txt). `<NEXT_FREE_INITIATIVE_ID>` se conserva sin reemplazar (D0-R3) |
| `01-confirmacion-I64-D0-R3.txt` (confirmación del Coordinator) | 3 268 | `8af5ba57a7b533f8ce9d148dfa92f945d189644503cb53d33de3e9140eec0b85` | UTF-8 sin BOM, solo LF, con LF final. Copia única byte-idéntica: [`I-64-coordinator-confirmation-d0-r3.txt`](../../initiatives/I-64-coordinator-confirmation-d0-r3.txt) |
| `01-orden-D0.txt` (orden D0) | 4 443 | `e91bee1239a5cbd9bc3a15714b3a014d3e368c3d34ae7450e9a6ac782f266666` | no se versiona; D0-R3 la actualiza sin reescribirla |
| `02-intake-y-matriz-30-puntos.md` (intake) | 12 279 | `1849239d3821ed07c6f42084b0d4f09ed1aa634201bfc2676742265cabf036b7` | no se versiona; documento de planificación, no Discovery |
| D0-R1 y D0-R2 | — | — | **No recibidas por esta sesión**: D0-R3 dice complementarlas, pero no están en el paquete local. Contenido UNKNOWN; se aplican D0 y D0-R3 |

**Decisión registrada (D0-R3, CD-ID30-NUM-01):** I-64 es el número técnico de ID30, confirmado directamente por el Coordinator;
I-63 queda retirado como candidato de ID30. Esta resolución actualiza las referencias de la orden D0 y del intake a «I-63
candidato»; esos textos históricos no se reescriben.

**Blob de Git:** la identidad durable es el blob versionado. Con `core.autocrlf=true` (sin `.gitattributes` en la base), un
checkout en Windows puede mostrar CRLF en disco; el SHA-256 del blob es el que se compara con el de transporte. La comprobación
sobre el blob del commit de bootstrap se informa con su CI.

## 3. Preflight

**Primer preflight (D0, 2026-10-01 ≈14:57Z–15:07Z): STOP antes del reclamo.** MEASURED:
- `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`; CI de push 36866499374 (`ref` main, ese SHA) con los cuatro jobs en
  `success` (evidencia de CI, no local). Heads remotos: `main` y `feature/rackmirror-espejo-semantico`.
- Bloqueos devueltos: B-01 brief ausente en el host; B-02 número no acreditado (intakes paralelos de I-62 e ID20; carpeta del
  paquete `I-64` frente a «I-63 candidato»); B-03 ventana de ROADMAP no pedida a propósito.
- Sin rama, worktree, commit ni push. Única escritura Git: `git fetch origin --prune --tags` (sin cambios de referencias).

**Resolución:** D0-R3 confirmó I-64 y entregó el brief, verificado arriba (§2). Entretanto, ID20 tomó **I-63** y lo reclamó
(`architecture/parametros-calculados-resumen-proyecto`).

**Preflight diferencial (antes del reclamo, 2026-10-01T16:01:03Z):** MEASURED:
- `origin/main` sin cambio (`819955d6…`); heads remotos: `main`, I-52 (`c1982b2a`) e I-63 (`cba24838`). La rama
  `architecture/workspace-persistente-rackcad` estaba libre en el remoto y en local, y la ruta del worktree no existía.
- Ningún tag ni archivo `docs/**/I-64*` en las referencias remotas. Las únicas menciones de «I-64» en el historial están en el
  bootstrap de I-63 (decisiones y evidencia sobre la numeración); no son reserva ni reclamo.
- Worktree principal del Owner sin tocar (`main` en `95690c28`, limpio); AutoCAD no en ejecución.
- Push del reclamo: aceptado como `* [new branch]` a las 2026-10-01T16:01:44Z; `git ls-remote` devolvió el commit de reclamo.

## 4. Ventanas de `docs/ROADMAP.md`

Canal: mensajería entre sesiones locales de Claude. El canal no registra hora; las horas son aproximadas, del reloj de esta sesión.

**Acuses que esta sesión dio a otros escritores** (todos cerrados con RELEASE antes de pedir la ventana propia):

| Escritor | Acuse (msg_id) | RELEASE recibido | Nota |
|---|---|---|---|
| I-62 (1.ª ventana) | `618376f1-…` | sí (sin escrituras; I-62 se detuvo) | no se reutiliza |
| ID20 / I-63 | `117d185e-…` y `0209a42a-…` (confirmación para I-63) | sí, tras publicar su bootstrap | verificado en Git |
| I-62 (2.ª ventana) | `fa373730-…` | sí (sin escrituras; I-62 sigue sin reclamo) | |

**Ventana propia de I-64:**

| Escritor | Solicitud (msg_id) | Respuesta | Alcance |
|---|---|---|---|
| I-52 (sesión «I - 52») | `f10d0917-…` | **ACUSE** (≈16:00Z) | solo `docs/ROADMAP.md`, una fila nueva; sin HANDOFF, índice de ADR ni host de I-52 |
| I-63 (sesión «I - 63», ID20) | `9df58d93-…` | **ACUSE** (≈16:00Z) | solo `docs/ROADMAP.md` |
| I-62 (sesión «I - 62») | `6a5e1bd6-…` | **ACUSE** (≈16:00Z) | solo `docs/ROADMAP.md`; HANDOFF fuera |

INICIO = cada acuse (los tres, recibidos antes del reclamo). FIN = «RELEASE» de I-64 por el mismo canal tras publicar el bootstrap:
hecho posterior a este commit, que se informa al Coordinator.

Textos recibidos (literales):

```text
[I-52]
ACUSE (I-52) para I-64 (ID30): desde este mensaje hasta tu RELEASE, I-52 no escribe, rebasa ni publica cambios que toquen
docs/ROADMAP.md. Motivo: sin escritura de ROADMAP en curso ni planeada; mi rebase está diferido hasta después de las corridas de
host; sin reclamo ni rama incompatible. Host y demás superficies de I-52 siguen.

[I-63]
ACUSE — I-63 (ID20) no escribirá, rebasará ni publicará cambios que toquen docs/ROADMAP.md desde este mensaje hasta tu «RELEASE».
Motivo: el bootstrap de I-63 (cba24838) ya está publicado, no tengo escrituras de ROADMAP pendientes y espero la revisión de mi
Coordinator.

[I-62]
ACUSE — I-62 no escribe, rebasa ni publica cambios en docs/ROADMAP.md desde este mensaje hasta tu «RELEASE» por este canal.
Motivo: I-62 no tiene reclamo y sigue esperando el acuse de «I - 52», así que no tiene escritura de ROADMAP pendiente.
```

Nota de integración (de I-63): las dos ramas insertan su fila justo después de I-60; quien integre en segundo lugar resolverá un
conflicto textual de filas adyacentes en su rebase. La compatibilidad textual no acredita exclusividad (C61-G0-03).

## 5. Bootstrap

Archivos nuevos: el contrato `docs/initiatives/I-64-workspace-persistente-rackcad.md`; `docs/initiatives/I-64-owner-brief.txt`;
`docs/initiatives/I-64-coordinator-confirmation-d0-r3.txt`; este archivo. Archivo modificado: `docs/ROADMAP.md` (una fila en la
tabla de la Fase 6, tras I-60). **Nada más**: sin cambios en normas globales, HANDOFF, índice de ADR, FOUNDATIONS, `src/`, `tests/`,
`assets/`, `eng/`, `deploy/`, `.github/`, soluciones, `global.json` ni `Directory.Build.*`. `automation.enabled: false`. Sin archivo
de estado ni registro de decisiones en D0 (fuera del alcance documental de la orden; el estado se crea antes de la primera delegación).

## 6. Validación de D0

- Comprobación documental: `git diff --name-only` del bootstrap contra la base, todo bajo `docs/`; se informa con el commit.
- CI exigible: la corrida `push` del SHA exacto del bootstrap ([WORKFLOW](../../WORKFLOW.md) §4.5.2, commit documental); es un hecho
  posterior al commit y se informa al Coordinator.
- Suites Core/UI locales, builds del Plugin y Owner Validation: **no aplican a D0** (sin cambios de producto); no se ejecutaron.

## 7. Intersecciones activas observadas (para DC-07)

- **I-52** (`feature/rackmirror-espejo-semantico`, tip `c1982b2a`, §248). MEASURED: 21 detrás / 151 delante de `819955d6`
  (merge-base `95690c28`); 0 archivos bajo `src/`, `tests/` y `assets/`; toca `docs/ROADMAP.md` (3 líneas), `docs/HANDOFF.md` (+393)
  y `docs/adr/README.md` (+1); el resto, `docs/automation`, `docs/initiatives` y `eng/`. RECONSTRUCTED (§§247-248): host no
  iniciado (`S1-A..S4 = NOT_ELIGIBLE`, `FIRST_NON_GOVERNING_HOST_RUN = NOT_AUTHORIZED`, `REBASE_BEFORE_HOST = NO`, `BOUND_PRODUCT_SHA
  = 95690c28`); su producto planificado recorre `RACKEDITAR` → Actualizar → redibujo. UNKNOWN: archivos de su implementación futura.
- **I-63** (ID20). MEASURED: reclamo `cdae6421` y bootstrap `cba24838` (fila en Fase 6 tras I-60). RECONSTRUCTED: frontera de su
  contrato (ID20 = semántica, providers y agregación; ID30 = inventario runtime de navegación y UI); cruces previstos en enumeración
  de racks por RackId, RACKLISTA y comandos de inventario.
- **I-62** (portabilidad del Principal): reserva confirmada; sin reclamo al abrir I-64; espera el acuse de I-52.

## 8. Capacidades observadas para la ejecución delegada (sin instalar ni configurar nada)

MEASURED (2026-10-01):
- `gh` 2.96.0 autenticado (permisos `repo`, `workflow`). SDK .NET: el `dotnet` del `PATH` es 10.0.401 y no resuelve `global.json`;
  el de usuario resuelve **8.0.423**.
- AutoCAD 2025 instalado (`R25.0.171.0.0`), no en ejecución; no se ejecutó.
- Codex CLI 0.159.2 en `%LOCALAPPDATA%\OpenAI\Codex\bin\de8a38d2100ae498\codex.exe`, «Logged in using ChatGPT», fuera del `PATH`. El
  directorio de `bin` más reciente (`5cb96978…`) no contiene `codex.exe`: una receta que tome «la versión más nueva» fallaría.
- CLI `claude` 2.1.270 con `loggedIn: false`.
- `~/.codex/config.toml`: SHA-256 `42E15A039EFD7E1A9197423AFB14B4358C7D60C89FEDDD734583891DB6E732A5`, modificado
  2026-10-01T05:30:20Z; distinto de `89F375C6…` registrado por I-61. Sin delegación abierta no es P-01; antes de la primera hace falta
  una línea base nueva. Motivo del cambio: UNKNOWN.
- Sesión responsable: `claude-opus-5-5` / `xhigh` (observado con `get_session`); PRINCIPAL_COORDINATION → Frontera → MATCH con el
  catálogo (verificación 2026-09-30).

RECONSTRUCTED (catálogo en la base, sin sondas nuevas): Controller `gpt-6-luna`/`high`; Worker `claude-sonnet-5-5` `medium`/`high`;
`claude-opus-5-5` subagente solo lectura/herramientas; Haiku `STALE` desde 2026-10-01; Fable sin medir. Ningún Controller, Worker,
Architect ni subagente se invocó en D0. `AuthorityRevision`, `BaseSha`, `RunId` y contratos de gate: no existen todavía.

## 9. Métricas, conformidad, Owner Validation y tag

Métricas: UNKNOWN (sin gates funcionales). Arquetipo inicial: NEW ARCHITECTURE (brief). Conformidad: no aplica todavía. Owner
Validation: requerida por el brief; no aplica a D0. Tag de integración: no existe.
