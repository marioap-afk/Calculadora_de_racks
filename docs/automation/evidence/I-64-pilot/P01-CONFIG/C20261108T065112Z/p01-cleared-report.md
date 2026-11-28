# I-64 — P-01 CLEARED (vía B: inventario completo + aceptación explícita del Owner)

Informe aparte, como pide la revisión de integración. No incluye la comprobación de AutoCAD.

## Configuración aceptada

| Campo | Valor |
|---|---|
| `config.toml` SHA-256 | `377D0C3C2DA32B0F763501628CF4A4F054FF33C64C40C2D50399801539EAD1E8` |
| Tamaño | 4887 bytes |
| Última escritura | 2026-11-07T18:09:47Z (sin cambios desde la medición del checkpoint) |
| Nombres | 104 |
| SHA-256 de la lista ordenada de nombres | `EB8775C5F9B3CC5C15031ED064391FFE000AB4A7220360C04F2F89D83D49F820` |
| Inventario capturado | 2026-11-08T06:51:12Z |
| Medición en el momento de la aceptación | 2026-11-08T06:57:42Z: mismo SHA-256, mismos 104 nombres, misma última escritura |

**Método:** el mismo extractor de nombres que el inventario `ConfigTomlKeys` de las instantáneas A/B/C (cabeceras de tabla y
claves `nombre =`, en el orden del archivo). Sin valores. Los 12 nombres `[projects.*]` llevan rutas locales: se publican
redactados, y el inventario JSON guarda el SHA-256 de cada nombre exacto para poder comprobarlos sin revelar la ruta.

**Frente a la instantánea A (105 nombres), completo:**

- sale: `followUpQueueMode`
- entra: `migratedFollowUpQueueToSteer`
- sale: `enabled`, `[plugins."code-review@openai-bundled"]`
- entra: `NODE_REPL_INSTRUCTIONS_USE_CASE_COMPUTER_USE`

Los otros 101 nombres, incluidos los 12 `[projects.*]`, son idénticos a A y están en el mismo orden.

## Aceptación del Owner

Opción elegida en el chat de la sesión: **«Acepto 377D0C3C (vía B)»** («Aceptas esta configuración exacta como instantánea nueva (SHA 377D0C3C, lista de nombres EB8775C5). Si la app vuelve a escribir el archivo, P-01 reaparece.»). Registro literal de la pregunta, la opción y la vista
previa en `owner-acceptance.md`. La vista previa mostraba el resumen frente a A y la ruta del inventario completo; el inventario
completo va abajo para la revisión del Coordinator.

## Efecto propuesto

- Con la vía B cumplida, **P-01 = CLEARED para `377D0C3C…D1E8`**. El conjunto aceptado pasa a ser A, B, C y esta configuración.
  No le asigno nombre: si el Coordinator la llama «D», la sesión usará ese nombre.
- Cualquier otro SHA-256 vuelve a ser P-01. La app de ChatGPT/Codex sigue abierta (ChatGPT.exe desde las 18:05Z de ayer y dos
  `codex.exe`) y puede reescribir el archivo, así que el hash se vuelve a medir justo antes de cada invocación de modelo.
- En la lista «sale», `enabled` aparece antes que su tabla por cómo alinea la comparación; en A son las posiciones 52 (la tabla)
  y 53 (su `enabled`).
- No se invocó ningún modelo y no se modificó `config.toml`. Esto no autoriza ninguna invocación por sí mismo: F2 sigue bloqueada
  hasta que la POST-I65 HOST SANITY CHECK dé PASS.

## Archivos (transitorios, sin commit)

En `artifacts/orchestration/I-64/P01-CONFIG/C20261108T065112Z/`:

- `config-inventory.json`: `595CE5E11362411FE6106946FF2368E7B43F09DC9A48F6AF3D177206FF0BD7CB`
- `config-inventory.txt`: `84B9CE1F32C6D49484C341AF23DBF1264F8392CB9519F3E3053B0970044BCA5E`
- `config-at-acceptance.json`: `F0FC96E61235F22E0530D66B10362CA555F47631CF167040A62713B1C84540D0`
- `owner-acceptance.md`: `9D4503189022D0A068C985F816F7831A91BF6507DAE182F6CADFAC24856BEB3D`

## Inventario ordenado completo (solo nombres)

```text
config.toml SHA-256 377D0C3C2DA32B0F763501628CF4A4F054FF33C64C40C2D50399801539EAD1E8, 4887 bytes, ultima escritura 2026-11-07T18:09:47Z, 104 nombres (inventario de solo lectura, sin valores)
  1      model
  2      model_reasoning_effort
  3      service_tier
  4      notify
  5  [windows]
  6      sandbox
  7  [desktop]
  8      keepRemoteControlAwakeWhilePluggedIn
  9      reviewDelivery
 10      conversationDetailMode
 11      ambient-suggestions-enabled
 12      migratedFollowUpQueueToSteer
 13  [desktop.open-in-target-preferences]
 14      global
 15  [desktop.open-in-target-preferences.perPath]
 16  [marketplaces.openai-bundled]
 17      source_type
 18      source
 19  [marketplaces.openai-primary-runtime]
 20      source_type
 21      source
 22  [plugins."documents@openai-primary-runtime"]
 23      enabled
 24  [plugins."spreadsheets@openai-primary-runtime"]
 25      enabled
 26  [plugins."presentations@openai-primary-runtime"]
 27      enabled
 28  [plugins."outlook-email@openai-curated"]
 29      enabled
 30  [plugins."github@openai-curated"]
 31      enabled
 32  [plugins."pdf@openai-primary-runtime"]
 33      enabled
 34  [plugins."template-creator@openai-primary-runtime"]
 35      enabled
 36  [plugins."visualize@openai-bundled"]
 37      enabled
 38  [plugins."chrome@openai-bundled"]
 39      enabled
 40  [plugins."browser@openai-bundled"]
 41      enabled
 42  [plugins."outlook-calendar@openai-curated"]
 43      enabled
 44  [plugins."teams@openai-curated"]
 45      enabled
 46  [plugins."codex-app-tools@openai-bundled"]
 47      enabled
 48  [plugins."unified-computer-use@openai-bundled"]
 49      enabled
 50  [plugins."computer-use@openai-bundled"]
 51      enabled
 52  [features]
 53      js_repl
 54  [projects.<redactado>]
 55      trust_level
 56  [projects.<redactado>]
 57      trust_level
 58  [projects.<redactado>]
 59      trust_level
 60  [projects.<redactado>]
 61      trust_level
 62  [projects.<redactado>]
 63      trust_level
 64  [projects.<redactado>]
 65      trust_level
 66  [projects.<redactado>]
 67      trust_level
 68  [projects.<redactado>]
 69      trust_level
 70  [projects.<redactado>]
 71      trust_level
 72  [projects.<redactado>]
 73      trust_level
 74  [projects.<redactado>]
 75      trust_level
 76  [projects.<redactado>]
 77      trust_level
 78  [mcp_servers.node_repl]
 79      args
 80      command
 81      env_vars
 82      startup_timeout_sec
 83  [mcp_servers.node_repl.env]
 84      NODE_REPL_NATIVE_PIPE_CONNECT_TIMEOUT_MS
 85      NODE_REPL_NODE_MODULE_DIRS
 86      NODE_REPL_NODE_PATH
 87      NODE_REPL_TRUSTED_CODE_PATHS
 88      CODEX_HOME
 89      BROWSER_USE_AVAILABLE_BACKENDS
 90      BROWSER_USE_TINYSKY_ENABLED
 91      NODE_REPL_INSTRUCTIONS_USE_CASE_BROWSER
 92      NODE_REPL_INSTRUCTIONS_USE_CASE_CHROME
 93      BROWSER_USE_CODEX_APP_BUILD_FLAVOR
 94      BROWSER_USE_CODEX_APP_VERSION
 95      NODE_REPL_TRUSTED_SERVICES
 96      SKY_CUA_NATIVE_PIPE
 97      SKY_CUA_NATIVE_PIPE_DIRECTORY
 98      CODEX_CLI_PATH
 99      NODE_REPL_UNTRUSTED_ENV_ALLOWLIST
100      NODE_REPL_INSTRUCTIONS_USE_CASE_COMPUTER_USE
101  [shell_environment_policy.set]
102      NODE_REPL_TRUSTED_BROWSER_CLIENT_SHA256S
103  [tui]
104      screen_reader_detection_done
```
