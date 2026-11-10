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

## F3-E03, resultado. `D:\i64-snav` sigue en disco

La comprobación de seguridad integrada de la herramienta rechazó el borrado, también tras la autorización del Owner, porque exige la aprobación de una persona. No se intentó por otra vía. La condición de la orden se cumple y el registro está versionado; el borrado queda como limpieza manual. No bloquea F3.

## F3-E04. Parada S-03 en la verificación de F3-T1

- **Problema:** la verificación `R20261110T142544Z-224d` dio `EXECUTION_BLOCKED / STOP`, `Contract`, S-03, porque la entrega dejó una pregunta abierta: si `Destroy` de una `SessionId` sin sesión de navegador debe bloquear un `GetOrCreate` posterior.
- **Decisión:** como emisora del contrato, la sesión la cierra sin cambiar nada: INV-F3T1-13 dice que una `SessionId` destruida no se resucita, sin distinguir si tuvo sesión de navegador. La implementación ya hace eso.
- **Reejecución:** 1 de 2 por BLOCKED (16.8), sin coste de `attempts`, con la disposición como entrada. `224d` se conserva tal cual.

## F3-E05. `BLOCKED` de `Routing` en la reejecución

- **Problema:** la verificación `R20261110T143036Z-9cbf` dio `EXECUTION_BLOCKED / BLOCKED`, `Routing`, porque comparó `sonnet` con `claude-sonnet-5-5`. `Participant.ModelRequested` guarda el parámetro literal del lanzamiento, que el transporte `subagent` solo admite como alias de familia.
- **Fundamento:** el modelo solicitado por la delegación es `claude-sonnet-5-5` y los 57 turnos de la transcripción del Worker lo usan. La misma forma de registro pasó en F2-T2B, así que el Controller varía en esta lectura.
- **Decisión:** la reejecución 2 de 2 añade solo esa semántica del campo al prompt, sin tocar el registro. Resultado: `R20261110T143422Z-057f`, `EXECUTION_VERIFIED`, 14/14. `9cbf` se conserva.
- **Riesgo:** el presupuesto de reejecuciones de esta entrega queda agotado. El prompt de F3-T2 lleva ambas aclaraciones desde el inicio.

## F3-E06. nc2 de F3-T1 reproduce el defecto conocido de `Scope`

- **Problema:** nc2 `R20261110T144043Z-0464` da `CONTROL NOT PASSED`: `EXECUTION_VERIFIED`, `Scope` en pass con la delegación mutada, sin ninguna otra diferencia frente a la real. Es la firma del defecto registrado en `protocol_debt`.
- **Decisión:** aplicar una vez `I64-SCOPE-BRIDGE-01` (A-4), con el verificador autorizado sin cambios. La salida `0464` conserva su clasificación.
- **Resultado:** `R20261110T144636Z-0548`, REAL PASS y ADVERSARIAL FAIL solo por `OUTSIDE_ALLOWED_WRITE_SCOPE`. `nc2 = SATISFIED_UNDER_I64_SCOPE_BRIDGE`.
- **Alcance:** nc2 de F3-T1. Sin cambio del protocolo global.

## F3-E07. Cierre de F3-T1-BROWSER-CORE

- **Decisión:** F3-T1 = CLOSED / PASS sobre `a92039bb`.
- **Fundamento:** verificación `057f` 14/14; nc1 `1839` y nc3 `8cf6` CONTROL PASS; nc2 bajo el puente (F3-E06); nc4 `8f7e`; CI 42833048949 4/4; revisión del código de la sesión sin defectos.
- **`attempts`:** sigue en 1 de 3: ninguna de las paradas cambió el trabajo.
- **Alcance:** esta tarea. No es F3 PASS.

## F3-E08. Aclaración del criterio 23 de la delegación de F3-T2

- **Problema:** la delegación `R20261110T145323Z-22d9` dice que nueve candidatos solo cambian si una guarda lo exige, y los invariantes obligan a cambiar seis de ellos.
- **Decisión:** al aceptar la delegación, sin editarla, la sesión aclara que manda el contrato y que la condición se aplica a los tres archivos de guardas. El Worker y la verificación recibieron esa disposición.

## F3-E09. Corrección de F3-T2 pese a una verificación `EXECUTION_VERIFIED`

- **Problema:** la verificación `R20261110T154803Z-fc18` dio `EXECUTION_VERIFIED`, 14/14, sobre `2c36b36b`, pero la revisión del código de la sesión y una revisión independiente hallaron seis defectos que ninguna prueba ejerce: T2-F1 a T2-F6, descritos en `analysis.md` junto a esa verificación. Uno incumple P-03 y P-04.
- **Opciones:** (a) cerrar y dejar que lo descubra un Smoke; (b) corrección delegada, consumiendo un intento; (c) corregirlo la sesión fuera de la cadena.
- **Decisión:** (b), con el precedente de D-04. `attempts` pasa a 2 de 3. Los controles nc1..nc3 se reservan para la entrega corregida. La verificación se conserva y no se reinterpreta.
- **Contrato del intento 2:** `F3-T2-BROWSER-UI/coordinator/gate-contract-attempt2.json`, con rutas exactas y los invariantes INV-F3T2-21 a 26 añadidos.
- **Límite:** T2-F4 se resuelve con una acción explícita del usuario y no con relectura automática tras cada drenaje.
- **Limitación conocida, sin corregir:** el caso de `PICKADD=0` descrito en `analysis.md`.
