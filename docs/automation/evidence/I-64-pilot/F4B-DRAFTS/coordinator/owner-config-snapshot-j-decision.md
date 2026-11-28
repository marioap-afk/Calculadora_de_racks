# I-64 F4B — Decisión del Owner sobre el config.toml tras el reinicio (Snapshot J)

**Fecha:** 2026-11-18. **Canal:** el chat de la sesión principal, con una pregunta de opciones; la respuesta es literal. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-config.md`.

**Los hechos:**
- Tras el reinicio del equipo, la app de Codex se actualizó: de `26.1111.21308` a `26.1111.2228.0`.
- La app reescribió `config.toml`. El hash pasó de `0B47701A…` (Snapshot I) a `57B0FED6…`.
- Los 104 nombres son los mismos, y los 68 valores del inventario seguro son idénticos a los de la I.
- Lo único que puede haber cambiado es el directorio opaco de la tubería de computer use (`mcp_servers.node_repl.env.SKY_CUA_NATIVE_PIPE_DIRECTORY`) o el formato.
- El binario de la CLI acreditado no cambió.
- Se detectó como la parada P-01 al preparar la reanudación del Worker de PB-T1b. No se lanzó nada.

**Texto de la pregunta, literal:** «Tras el reinicio, la app de Codex se actualizó (26.1111.21308 → 26.1111.2228.0) y reescribió su config.toml, que ya no es la Snapshot I aceptada (es la parada P-01; no se lanzó nada). Los 104 nombres y los 68 valores del inventario seguro son idénticos a los de la I; solo puede haber cambiado el directorio opaco de la tubería de computer use, que la app regenera. El binario de la CLI acreditado no cambió. Es el mismo caso que H → I, cuando elegiste «Sonda neutra, luego I». ¿Qué hago?»

**Respuesta del Owner:** «Sonda neutra, luego J (Recomendado)».

**Lo que implica:**
- **La sonda:** se ejecuta la sonda neutra con el binario acreditado, con el procedimiento de la Snapshot I.
- **Si da Pass:** el `config.toml` actual pasa a ser la **Snapshot J**, la línea base activa del Controller en I-64. Sustituye a la I, y se reanuda PB-T1b.
- **Si no da Pass:** se vuelve al Owner.
