# Verificación R20261113T143518Z-c983 de F4-T3B2: interrumpida por el reinicio del equipo

Hechos medidos por la sesión el 2026-11-13 a las 14:45Z:
- **Inicio de la cesión:** 14:35:32Z, con Snapshot G, 0 participantes, HEAD igual al remoto (`23f67f56`) y árbol limpio.
- **Últimas escrituras de la corrida:** las de `events.jsonl` (538261 bytes), a las 14:36Z.
- **Faltan** `output.json`, `exit-code.txt` y `cession-end.txt`.
- **El equipo se reinició:** `LastBootUpTime` 14:38:31Z. El Owner lo dice en el chat: «Se me reinició la computadora».
- **Conclusión:** la corrida murió con el reinicio, sin salida. No es un resultado del Controller: es una corrida interrumpida que hay que repetir.

**Tras el arranque:**
- La app de escritorio de Codex se abrió a las 14:41Z; antes del reinicio no estaba abierta.
- `config.toml` se escribió a las 14:44:48Z:
  - SHA-256 `A859B66CC9A4631FEC01AA2E798AAC4F7E07F911352735A6A35D8169D9639864`;
  - las mismas 104 claves en el mismo orden (`OrderedNamesSha256` igual al de G);
  - algún valor distinto.
- El comprobador lo rechaza: no es el Snapshot G (`7C1516A9…`).
- Por la regla de F4-E13, un cambio de config es STOP y no crea un snapshot nuevo por sí solo. Sin invocaciones de modelo hasta la decisión del Owner.
- **Procesos tras el arranque:** 0 participantes y 0 no atribuibles.
