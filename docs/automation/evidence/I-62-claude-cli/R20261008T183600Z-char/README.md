# claude-cli: caracterización acotada de solo lectura (decisiones §56, punto 7; Owner CLAUDE-CLI-I62 = A)

- **Registro:** [claude-cli-characterization.json](claude-cli-characterization.json), SHA-256 `20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c`.
  Lo fija la compuerta de transporte del kit de la re-revisión de A-2. Está saneado: sin correo, sin organización y con las rutas de usuario como variables.
- **C0** (`C1-v2.1.270/`): `%USERPROFILE%/.local/bin/claude.exe` 2.1.270. La API rechaza `claude-opus-5-5` («requires 2.1.280 or newer»); sin consumo.
- **C1:** binario `%APPDATA%/Claude/claude-code/2.1.293/83cb0bd7fed4/claude.exe` (gestionado por la app de escritorio; Authenticode Anthropic;
  SHA-256 `8693c4a0…`), solo lectura (Read, Grep, Glob), `--safe-mode`, `--strict-mcp-config`, `dontAsk`. Resultado: modelo `claude-opus-5-5` y effort `xhigh`
  observados por mensaje; prompt y lectura (12/12) fieles; salida estructurada; exit 0; sin CLAUDE.md, memoria ni ganchos.
- **C2:** terminación por el invocador a los 8 s: exit 1, sin mensaje `result`, transcripción conservada, proceso terminado.
- **C3** (plantilla exacta del kit, con `--add-dir`): la lectura dentro del directorio añadido se entrega fiel y la exterior se deniega → **PASS**.
  Una primera ejecución de C3 se ejecutó, pero el analizador del kit falló al leer una entrada `system/permission_denied` de stream-json (`message` en texto).
  Se corrigió el analizador, se apartaron sus artefactos (no versionados; directorio de la sonda `D:/r62-c3-probe.run1`) y C3 se repitió una vez. El
  `stdout.jsonl` de las sondas no se versiona porque contiene rutas del perfil del usuario.
- **Elegibilidad:** ARCHITECT y REVIEWER ELIGIBLE en esta observación. Cualquier cambio del binario, de la versión, de la autenticación, del modelo, del
  effort o de los flags deja la observación STALE.
