# I-66 — Registros en sombra de R1a (piloto de I-62)

Esta carpeta guarda los registros en sombra del piloto R1a de I-62 «Principal portable» (decisiones de I-62 §105 y §106). **No son autoridad:**
I-61 y WORKFLOW siguen gobernando I-66, y estos registros solo pueden añadir un STOP. Aplican, como comprobaciones adicionales, los textos I62
materializados e inactivos de la revisión `41ccfdcf692fb319bb7e8ff4de7edeaa4f1c5134` de I-62 (`docs/automation/agent-execution/` de esa rama).

| Carpeta | Prueba | Contenido |
|---|---|---|
| `autoverificacion/` | AT-1: autoverificación del Principal A al abrir I-66 | observaciones de la sesión, `preflight-1.json` (`rackcad-preflight/v1`, CUSTODY), su validación (Test-Json en dos fases y reglas C1-C8 de README §13.2, saneamiento §13.4), los negativos y `result.json` |
| `bootstrap/` y `state.v2.yml` | AT-2: BOOTSTRAP `state/v2` en sombra | binding `rackcad-binding/v1` del Principal A (PENDING) con su validación (Test-Json y reglas B1-B10 de README §14.2), el punto BOOTSTRAP `state.v2.yml` (un solo archivo que se reescribe en cada punto) y su productor. Convención SH-1: no existe `I62_EFFECTIVE_SHA`; `protocol.effective_sha` registra la revisión de I-62 cuyos textos aplica la sombra, y `claim_parent_contains_effective` es false porque la base no la contiene |

`MANIFEST.sha256` lista el SHA-256 de cada archivo.
