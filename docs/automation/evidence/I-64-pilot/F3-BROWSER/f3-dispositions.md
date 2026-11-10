# I-64 F3 — Decisiones de la sesión

Registro append-only de las decisiones tomadas por la sesión durante F3, bajo la orden `coordinator/owner-f2-close-ratification-f3-authorization.md`. Solo aplican a I-64.

## F3-E01. Presupuesto de F3

- **Decisión del Owner (C-F3-01):** F3 arranca con un presupuesto propio, `attempts` = 1 de 3. El 3 de 3 de F2 no pasa a F3; `attempts` no es un contador global de I-64.
- **Registro:** `automation_state.attempts` pasa a 1 en el commit que incorpora este archivo, antes de cualquier delegación de F3. La historia de intentos de F2 no se reescribe.

## F3-E02. Cambio de `config.toml` antes de la primera invocación de F3

- **Problema:** el 2026-11-10 a las 12:38:39Z se reescribió `config.toml`, y sus valores nuevos apuntan a la instalación de la app de Codex. Su SHA-256 pasó de `2EEDDAAB…` (Snapshot E) a `AAEB58D8…`. Fuera de la lista aceptada, eso es una parada P-01, y la orden del Owner limita las devoluciones a cinco casos que no incluyen este.
- **Comparación con E, que es lo que manda la aceptación de E:** mismos 104 nombres en el mismo orden (`EB8775C5…`), 68 valores, ninguna clave añadida ni retirada. Cambian seis valores, todos rutas de instalación que gestiona la app: `notify`, `mcp_servers.node_repl.command` y `NODE_REPL_NODE_MODULE_DIRS`, `NODE_REPL_NODE_PATH`, `NODE_REPL_TRUSTED_CODE_PATHS` y `CODEX_CLI_PATH` en `mcp_servers.node_repl.env`. No cambian `model`, `model_reasoning_effort`, `service_tier`, `windows.sandbox`, los `trust_level` ni ninguna clave de permisos, aprobación o enrutado. Sin valores con forma de secreto. Detalle en `I-64-pilot/F3-ENVIRONMENT/snapshot-f-baseline.json`.
- **Binario:** `Codex/bin/5449c1f0f26f1f98/codex.exe` sigue presente y con el mismo SHA-256 (`4F91AE86…`). Es el que lanza el relevo; las nuevas rutas de `config.toml` no lo cambian.
- **Sonda neutra `R20261110T134835Z-3082`:** PASS. `gpt-6-luna` / `high`, solo lectura, red restringida, aprobación `never`, oráculo Git exacto, sin aviso de uso. `turn_context` idéntico al de la sonda `6a68` salvo `root_turn_id`.
- **Decisión:** se registra el **Snapshot F** y se continúa, con los mismos criterios con que el Owner y el Coordinator aceptaron E. Vale solo para la celda `gpt-6-luna` / `high` / solo lectura. Cualquier otro hash vuelve a ser parada y se compara con F.
- **Límite:** es una decisión de la sesión, no una aceptación del Owner. Se le comunica en el informe final de F3.

## F3-E03. Limpieza de `D:\i64-snav`

- **Comprobación de seguridad:** es el directorio de diagnóstico esperado (`diagbin`..`diagbin4`, `releasecheck` y dos DXF), 162 archivos sin fuentes ni evidencia (solo `.dll`, `.pdb`, `.csv` y `.json` de build); ningún proceso los tiene abiertos (apertura exclusiva correcta en los 162; el `acad.exe` activo no carga ningún módulo de allí).
- **Representación:** los dos DXF están custodiados con su SHA-256 en `I-64-pilot/F2-SMOKE-NAV/`. Los cinco `RackCad.Plugin.dll` de diagnóstico se registran en `I-64-pilot/F2-SMOKE-NAV/cleanup-i64-snav.json`, porque tres no figuraban aún por hash.
- **Decisión:** borrar el directorio después de versionar ese registro. La limpieza no bloquea F3.
