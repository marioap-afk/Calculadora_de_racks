# claude-cli 2.1.299: caracterización acotada de solo lectura (decisiones §87, puntos 2-3; procedimiento de §56, punto 7)

- **Registro:** [claude-cli-characterization.json](claude-cli-characterization.json), SHA-256 `4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c`. Mismo esquema que
  [R20261008T183600Z-char](../R20261008T183600Z-char/README.md); saneado (sin correo ni organización; rutas de usuario como variables).
- **Binario:** `%APPDATA%/Claude/claude-code/2.1.299/f0584be0fbaf/claude.exe`, 269 231 264 bytes, SHA-256
  `295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b`, Authenticode válido de «Anthropic, PBC» (con sello de tiempo),
  `--version` = `2.1.299 (Claude Code)` en UTF-8 estricto; instalado por la actualización de la app de escritorio del 2026-10-16, que borró 2.1.293.
  `auth status`: loggedIn, claude.ai, firstParty, max (sin leer credenciales; sin guardar correo ni organización).
- **Clon:** `D:/r62-cli-char2` en `e8a3de05`, sin remoto; limpio antes y después de cada sonda.
- **C1** (mismo prompt y flags que en 2.1.293): código 0; `claude-opus-5-5` y `xhigh` observados por mensaje; prompt fiel; lectura 12/12 fiel y solo
  del archivo pedido; salida estructurada correcta; herramientas Read, Grep, Glob y StructuredOutput; sin MCP; `dontAsk`; sin CLAUDE.md, memoria ni
  AGENTS.md; mismos tipos de contexto automático que en 2.1.293.
- **C2:** terminación por el invocador a los 8 s: código 1, sin mensaje `result`, proceso terminado, transcripción conservada, dos órdenes antes.
- **C3** (plantilla exacta del kit de la ronda 4, con `--add-dir`; `char2_c3.py`, derivado por sustituciones exactas de la sonda del kit de A-2,
  diff en `char2_c3.diff`): la lectura dentro del directorio añadido se entrega fiel y la exterior se deniega → **PASS**.
- **Intentos conservados:** el primer C1 (`C1.run1-oauth/`) falló sin llegar al modelo («Failed to refresh OAuth token: another Claude Code process
  is refreshing it…»), lanzado segundos después de `auth status`; el primer C2 (`C2.run1/`) terminó sin ninguna orden en la misma ventana. Se
  repitieron una vez pasados más de 3 minutos, como el lanzador del kit espera 120 s tras `auth status` (la sonda C3 incorpora esa espera).
- **Elegibilidad (decisiones §87, punto 3):** las 14 comprobaciones de `Eligibility.Checks` en verdadero, equivalentes a las de 2.1.293; ARCHITECT
  y REVIEWER **ELIGIBLE** en esta observación; `STALE` = false; los predicados de la compuerta de transporte del kit (`check_base`, `check_c3`) no
  encuentran ningún problema con este registro. CLAUDE-CLI-I62 no cambia. Cualquier cambio del binario, de la versión, de la autenticación, del
  modelo, del effort o de los flags deja la observación STALE.
- Los `stdout.jsonl` y `stderr.txt` de las sondas no se versionan (contienen rutas del perfil del usuario y la identidad de la cuenta); quedan en
  `D:/r62-cli-char2-work`. Ninguna revisión formal se ejecutó.
