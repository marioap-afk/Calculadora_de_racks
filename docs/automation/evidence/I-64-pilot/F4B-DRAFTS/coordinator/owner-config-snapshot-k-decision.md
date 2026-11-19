# I-64 F4B — Decisión del Owner sobre el config.toml tras la actualización de la app de Codex (Snapshot K)

**Fecha:** 2026-11-19. **Canal:** el chat de la sesión principal, con una pregunta de opciones. La pregunta y la respuesta son literales.

**Los hechos:**
- **Dónde se detectó:** al preparar la planificación de PB-T2b, ya emitida (F4B-E36), la comprobación de `config.toml` de la cadena paró con la parada P-01. La preparación `R20261119T154005Z-3194` creó su carpeta, pero no se lanzó nada.
- **Qué cambió:** la app de Codex se actualizó y reescribió `config.toml` a las 14:05:53Z.
  - El hash pasó de `57B0FED6…` (Snapshot J) a `E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE`.
  - Los 104 nombres son los mismos, y en el mismo orden (`OrderedNamesSha256` igual).
  - De los 68 valores del inventario seguro, cambian solo dos, y los dos llevan la versión de la app (26.1111.21308 → 26.1111.71704):
    - `mcp_servers.node_repl.env.BROWSER_USE_CODEX_APP_VERSION`;
    - la ruta del plugin del navegador en `mcp_servers.node_repl.env.NODE_REPL_TRUSTED_SERVICES`.
- **Qué no cambió:** el modelo y el effort. El binario acreditado de la CLI (`bin/5449c1f0f26f1f98`, SHA-256 `4F91AE86…`) tampoco.

**Texto de la pregunta, literal:** «Al preparar la planificación de PB-T2b (ya emitida), la comprobación P-01 paró: la app de Codex se actualizó hoy a las 14:05Z (26.1111.21308 → 26.1111.71704) y reescribió su config.toml, que ya no es la Snapshot J aceptada. No se lanzó nada. Los 104 nombres son los mismos y, de los 68 valores del inventario seguro, solo cambian dos, y los dos son la versión de la app: BROWSER_USE_CODEX_APP_VERSION y la ruta del plugin del navegador en NODE_REPL_TRUSTED_SERVICES. Modelo, effort y el binario acreditado de la CLI no cambian. En el mismo caso de ayer elegiste 'Sonda neutra, luego J'. ¿Qué hago?»

**Opciones:**
- **«Sonda neutra, luego K (Recomendado)»:** Ejecuto la sonda neutra con el binario acreditado, con el mismo procedimiento que la J. Si da Pass, el config.toml actual pasa a ser la Snapshot K, la línea base del Controller, y lanzo la planificación de PB-T2b. Si no da Pass, vuelvo a ti.
- **«Parar y revisarlo yo»:** No lanzo nada más. Te dejo el diff seguro y esperas a revisar la configuración de Codex tú mismo antes de seguir.

**Respuesta del Owner:** «Sonda neutra, luego K (Recomendado)».

**Lo que implica:**
- **La sonda:** la sonda neutra con el binario acreditado, con el procedimiento de la J. La referencia del contexto efectivo es la planificación `R20261119T065628Z-7de7` de PB-T2a, que corrió bajo la J con el mismo binario.
- **Si da Pass:** el `config.toml` actual pasa a ser la **Snapshot K**, que sustituye a la J como línea base activa del Controller en I-64, y se lanza la planificación de PB-T2b.
- **Si no da Pass:** se vuelve al Owner.
- **Dónde queda el registro:** el registro de la K y esta decisión se dejan en `artifacts/orchestration/I-64/F4-ENVIRONMENT/`, donde el Controller los lee. Se custodian con el cierre de PB-T2b, porque la cadena de PB-T2b exige que su base sea la emisión.
