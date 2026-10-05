# I-62 — Kit de la revisión del Architect de la enmienda A-1 (listo para lanzar; no lanzado)

> **Preparación.** La orden del Coordinator «F3 GATE PASS / AUTHORIZE FORMAL ARCHITECT REVIEW OF AMENDMENT A-1» autoriza **una** invocación limpia y
> separada del Architect. La sesión autora no puede crear esa sesión: no tiene `start_session`; `codex-cli` y `codex-desktop-session` están bloqueados por
> OD-2; `claude-cli` no está autenticado y no está en el `PATH` (OD-3); y las sesiones existentes no son limpias. El lanzamiento espera **un clic del Owner**
> en la tarea `task_fce8049e` («Review I-62 amendment A-1 as Architect»). Es un AUTONOMY_GAP: OWNER_CLICK_REQUIRED.

```text
Estado          = READY_TO_LAUNCH; AUTONOMY_GAP = OWNER_CLICK_REQUIRED
RunId           = R20261003T023945Z-cab7  (InvocationId I20261003T023945Z-cab7, LogicalReviewRequestId L20261003T023945Z-cab7, AttemptSeq 1)
Objeto          = docs/initiatives/I-62-A-1.md                    blob 09ca93285975c4b7af6471d6ae91bfa12c94a1fc   (commit bf7b0d9c, CI 37085558300 4/4)
Paquete         = docs/initiatives/I-62-architect-package-A-1.md  blob 071cecba1fb8bdefc9a4e5554dd47a0459f8a1d2
Clon de lectura = D:\r62-arch-a1 (detached en bf7b0d9c, limpio)   Directorio de la corrida = D:\r62-arch-a1-run
```

## 1. Contenido del kit (copias byte a byte del directorio de la corrida)

| Archivo | SHA-256 | Papel |
|---|---|---|
| `prompt.md` | `428eb5810055602b2f4411364140128d52f3741ab93baef7fd0b1f29a05a52bb` | instrucciones completas; el revisor comprueba su hash y lo lee con Read |
| `order.txt` | `a2d96afb1533821b0d6f9badebb3b61989a92a331bca02edc3e11811167af521` | orden literal del Coordinator (7 435 bytes): autoridad y foco |
| `closure.json` | `703cb8d81703df74caad86468e63ef2ff2522e188701f7335f72033ca5edb796` | cierre efectivo: 22 canónicos y 12 transitivos con blob y SHA-256, política de lectura, acciones, prohibidos, lanzamiento |
| `corpus.json` | — | los 61 caracteres no ASCII distintos del corpus del cierre |
| `preflight-read-fidelity.json` | — | preflight del camino de lectura (Read): 3 llamadas, 64 líneas, todas FAITHFUL_NORMALIZED, incluida una línea de 3 683 caracteres |
| `result.schema.json` | — | esquema estricto del resultado (representación experimental de `architect-review-result/v1`) |
| `post-review.py` | — | auditoría posterior (identidad observada, comprobación del prompt, auditoría de lecturas, fidelidad, resultado, premisas, acreditación) |
| `selftest-post-review.py` | — | autoprueba con dos transcripciones sintéticas: la buena da ACCREDITED; la mala da NOT_ACCREDITED por sus tres motivos exactos (MEASURED: ambas PASS) |
| `make_closure.py`, `read_fidelity.py` | — | generadores del cierre y del preflight |

Los SHA-256 son de los bytes LF (`git show <commit>:<ruta> | sha256sum`).

## 2. Lanzamiento (cuando el Owner pulse la tarea)

1. La sesión nueva arranca en un worktree fresco bajo `D:\r62-arch-a1`, que es otro repositorio: no carga la memoria del proyecto de la sesión autora.
   No existe `~/.claude/CLAUDE.md` de usuario.
2. Su primer turno es el texto de la tarea. Comprueba el SHA-256 de `prompt.md`, lo lee y lo sigue.
3. La sesión autora la localiza con `list_sessions` (cwd bajo `D:\r62-arch-a1`). Se suscribe a su fin con `SendMessage` y `notify_when_idle` (sin
   mensaje) y observa su modelo y su effort con `get_session`.

## 3. Después de la revisión (sesión autora)

1. `python post-review.py <transcripción JSONL de la sesión revisora> D:\r62-arch-a1 D:\r62-arch-a1-run result.schema.json <audit.json>`. La transcripción
   está en `~/.claude/projects/<clave del cwd de la revisora>/<sessionId>.jsonl`.
2. `get_session(<id>)`: modelo, effort, permisos y cwd. Esa es la identidad observada; la declarada por el revisor es solo informativa.
3. Con NOT_ACCREDITED, no hay veredicto: se registra el motivo exacto, sin reintento (la orden autoriza una sola invocación).
4. **Custodia** (plantilla de §4); después, commit, push y CI exacta, y STOP.

## 4. Plantilla de custodia

**Directorio:** `docs/automation/evidence/I-62-architect-A-1/R20261003T023945Z-cab7/`.

| Archivo | Origen |
|---|---|
| `README.md` | cabecera: ids, autorización (orden, SHA-256 `a2d96afb…`), objeto, veredicto literal, acreditación, naturaleza experimental de los JSON |
| `prompt.md`, `order.txt`, `closure.json`, `preflight-read-fidelity.json` | copias de este kit |
| `output.json` | bloque JSON final literal del revisor |
| `audit.json` | salida de `post-review.py` |
| `runtime-evidence.json` | `get_session` + `RuntimeIdentity` de la auditoría |
| — (no versionado) | transcripción JSONL: ruta, bytes y SHA-256 en el README |

**Registros:**
- `docs/initiatives/I-62-architect-review-A-1.md`: veredicto literal del Architect, sin disposición de la sesión;
- decisiones §35: la orden «F3 GATE PASS / AUTHORIZE …» (identidad `a2d96afb…`) y, en su caso, la de continuación;
- evidencia §39: lanzamiento, identidad observada, auditoría, acreditación y resultado;
- estado: `a1_status` con el veredicto del Architect y el del Coordinator PENDING.

Sin editar A-1, sin crear A-2 y sin materializar F4.
