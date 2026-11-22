# I-64 F4B — Decisión del Owner sobre el config.toml tras la actualización de la app de Codex (Snapshot L)

**Fecha:** 2026-11-22. **Canal:** el chat de la sesión principal, con una pregunta de opciones. La pregunta y la respuesta son literales.

**Los hechos:**
- **Dónde se detectó:** al preparar la planificación de PB-T4d, ya emitida (F4B-E56), la comprobación de `config.toml` de la cadena paró con la parada P-01. La preparación `R20261122T171951Z-bc04` creó su carpeta, pero no se lanzó nada.
- **Qué cambió:** la app de Codex se actualizó y reescribió `config.toml` a las 09:29:16Z.
  - El hash pasó de `E1F69847…` (Snapshot K) a `B71650A637551747F522BB1D2F4BA1BB9E25AAFF1A61CE1B7BC066197FFFEBB3`.
  - Los nombres pasan de 104 a 105 (`OrderedNamesSha256` `3ECF8E072CC52AE60A6E6D508DB54DCBFFFAFB04EAF9B52905A2CEAB0E0812E3`): se añade una clave, `mcp_servers.node_repl.env.NODE_REPL_LAZY_TRUSTED_WORKER`, con el valor `"0"`. No sale ninguna.
  - De los valores del inventario seguro, cambian dos, y los dos llevan la versión de la app (26.1111.71704 → 26.1117.41037):
    - `mcp_servers.node_repl.env.BROWSER_USE_CODEX_APP_VERSION`;
    - la ruta del plugin del navegador en `mcp_servers.node_repl.env.NODE_REPL_TRUSTED_SERVICES`.
- **Qué no cambió:** el modelo, el effort, el `service_tier` y el sandbox. El binario acreditado de la CLI (`bin/5449c1f0f26f1f98`, SHA-256 `4F91AE86…`) tampoco.

**Texto de la pregunta, literal:** «La comprobación de la configuración de Codex (P-01) paró la planificación del Controller de PB-T4d. No se lanzó nada. La app de Codex se actualizó hoy, 22-nov, a las 09:29 UTC (26.1111.71704 → 26.1117.41037) y reescribió config.toml, que ya no es la Snapshot K aceptada. Cambian dos valores, los dos con la versión de la app: BROWSER_USE_CODEX_APP_VERSION y la ruta del plugin del navegador en NODE_REPL_TRUSTED_SERVICES. Esta vez también aparece una clave nueva, NODE_REPL_LAZY_TRUSTED_WORKER = "0", en el entorno del servidor node_repl (104 → 105 nombres). No cambian ni el modelo, ni el effort, ni el sandbox, ni el binario acreditado de la CLI. En los casos de la J y la K elegiste «Sonda neutra, luego …». ¿Qué hago?»

**Opciones:**
- **«Sonda neutra, luego L (Recomendado)»:** Corro la sonda neutra con el binario acreditado, como con la J y la K. Si da Pass, el config.toml actual pasa a ser la Snapshot L y lanzo la planificación de PB-T4d. Si no da Pass, vuelvo a ti.
- **«Parar y revisarlo yo»:** No lanzo nada más. Te dejo el diff seguro y espero a que revises tú la configuración de Codex antes de seguir.

**Respuesta del Owner:** «Sonda neutra, luego L (Recomendado)».

**Lo que implica:**
- **La sonda:** la sonda neutra con el binario acreditado, con el procedimiento de la J y la K. La referencia del contexto efectivo es la planificación `R20261121T205457Z-af49` de PB-T4c, que corrió bajo la K con el mismo binario.
- **Si da Pass:** el `config.toml` actual pasa a ser la **Snapshot L**, que sustituye a la K como línea base activa del Controller en I-64, y se lanza la planificación de PB-T4d.
- **Si no da Pass:** se vuelve al Owner.
- **Dónde queda el registro:** el registro de la L y esta decisión se dejan en `artifacts/orchestration/I-64/F4-ENVIRONMENT/`, donde el Controller los lee. Se custodian con el cierre de PB-T4d, porque la cadena de PB-T4d exige que su base sea la emisión.
- **La preparación `R20261122T171951Z-bc04`** queda sin lanzar y no se reutiliza: la planificación se prepara de nuevo con `plan_pre` cuando la L esté aceptada.
