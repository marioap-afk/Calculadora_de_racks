# Kit a2p2-v4 — bloque A2-P2 del par nuevo de `codex-cli` y, si procede, A4-1 del trío nuevo (NO EJECUTADO; para revisión y congelación)

> **No se ha ejecutado y no está congelado.** Lo prepara la supervisión (plano a). Para prepararlo no se invocó `codex.exe`, `codex` ni `claude`
> (tampoco `--version` ni `login status`), no se leyó nada bajo `%USERPROFILE%\.codex` salvo el worktree de I-62 y no se ejecutó `cfg-fp.ps1`. Sin
> las puertas de §7, `run_probe.sh probe <MODO>` se niega a correr. El autoensayo usó dos clones propios, ya borrados (§10).

## 0. Autoridad y hechos

- **Identidad nueva** (decisiones §68, punto 3.A; evidencia §125; `OD-2/R20261011T034757Z-s68-passive/result.json`, observación 1):
  `BinaryHash` `d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec` en `%LOCALAPPDATA%\OpenAI\Codex\bin\2e5e00daee91c61d\codex.exe` (el
  único `codex.exe`; junto a él, `codex-command-runner.exe`, `codex-windows-sandbox-setup.exe` y `codex-code-mode-host.exe`), `AppVersion`
  `26.1007.2314.0`, huella `F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113` con los mismos 107 nombres saneados que
  `baseline-keynames-73890CA3.json`. `baseline-keynames-F909F480.json` es una copia de contenido idéntico (comprobado también contra `obs1-cfg.json`).
  Sin observar: `CliVersion` y `AuthState` (exigen invocar la CLI).
- **A-2, A2-P2** (`docs/initiatives/I-62-A-2.md` §3.3): tras una actualización observada, **un** bloque nuevo de **≤ 2** sondas de solo lectura
  para el par exacto ya observado con su huella resultante estable (regla 3), autorizado por bloque (disposición del Coordinator que nombra el par +
  consumo del Owner), OD-2 sobre la huella resultante antes de cualquier invocación afectada (regla 4); otra actualización invalida el bloque
  (regla 6). P-07 comprueba el tope de dos antes de cada sonda.
- **A-4, A4-1** (`I-62-A-4.md` §3.2, reglas 3 y 4): la actualización con un bloque nuevo de A2-P2 abre **una** medición A4-1 más (shell
  declarada `cmd.exe`), contada aparte del bloque. **A4-2** (§3.3): el Architect de la Topología A pasa a otro adapter solo si la celda Codex del
  Architect queda medida **BELOW_REQUIRED con la identidad vigente**; la medición 0/6 anterior está STALE (§65, D-2).
- **Estándar de medición del Controller** (§64, punto 2; §65, D-4): todas las operaciones obligatorias de PLAN y VERIFY (Git y hashes; fidelidad
  Markdown/YAML/JSON; TRX y artefactos de CI; resolución I62 del mapa por AP 16.13; esquemas canónicos; controles negativos), un lanzamiento, sin
  ampliación implícita de permisos, tope de 600 s. El mapa del fixture es MAP_INVALID (defecto aparte, §66 punto 3): la medición demuestra
  operaciones, no un PASS.
- **A4-1 anterior** (`OD-2/R20261010T000542Z-a4-1-probe`): NOT_DEMONSTRATED con la identidad anterior; la celda paró a los ~100 s tras 11
  comandos. Defecto de la herramienta: el contador `git_write` contaba `git merge-base` como `merge` (corregido aquí, §5).

## 1. Los tres modos

| Modo | Cuenta en | Celda | `-C` (propuesta) | Texto | Esquema de salida | Marcador (nunca se borra) | Evidencia (tope) |
|---|---|---|---|---|---|---|---|
| `NATIVE_CONTROLLER` | bloque A2-P2, sonda 1 | `gpt-6-luna` / `high` | `D:\r62-fixture\A2` | `probe-prompt.native-controller.template.txt` (32 pasos, 175 claves, sin shell) | `probe-schema.native-controller.json` | `out/native-controller/PROBE_LAUNCHED` | `*a2p2-v4-controller*` |
| `NATIVE_ARCHITECT` | bloque A2-P2, sonda 2 | `gpt-6.1-sol` / `high` | `D:\r62-fixture\arch-a2p2v4` (clon nuevo, `make-arch-clone.ps1`) | `probe-prompt.native-architect.template.txt` (6 pasos, 52 claves, sin shell) | `probe-schema.native-architect.json` | `out/native-architect/PROBE_LAUNCHED` | `*a2p2-v4-architect*` |
| `CMD_A41` | A4-1 (aparte del bloque) | `gpt-6-luna` / `high` | `D:\r62-fixture\A2` | `probe-prompt.cmd-a41.template.txt` (= v3 byte a byte) + `shell-declaration.txt` (= v3) | `probe-schema.cmd-a41.json` (= v3 salvo el nombre del kit en `description`) | `out/cmd-a41/PROBE_LAUNCHED` | `*a4-1-probe-2*` y exactamente una `*a4-1-probe*` previa |

Los tres comparten la receta 16.4 literal (binario por ruta verificada `bin/2e5e00daee91c61d`, `-C`, `-s read-only`, `-m`,
`-c model_reasoning_effort="high"`, `--output-schema`, `-o`, `--json`, sin `--ephemeral`, stdin cerrado, el `pwsh` del runtime primero en el `PATH`,
600 s con `taskkill /T /F`), la medición de identidad antes y después (`measure`, ahora también con la huella conjunta de los ejecutables
acompañantes, que entra en la comparación antes/después), el escenario S de v3 para el Controller (`stage`/`unstage` en el área ignorada) y la
instantánea informativa de procesos `codex*.exe` antes y después (`procs-snapshot.ps1`: nunca guarda líneas de órdenes).

Orden propuesto: `NATIVE_CONTROLLER` → `NATIVE_ARCHITECT` → (`CMD_A41` solo si el primero muestra F6-OBS-03). Los dos primeros son
independientes entre sí; el tercero exige la comparación del primero con `NativeRoute.ShellLimitationPersists = true`.

## 2. `NATIVE_CONTROLLER` (sonda 1 del bloque)

**Qué cambia frente a v3** (el resto del texto, el escenario S, la referencia y los 31 pasos restantes son los de v3):
- cabecera: «sonda 1 del bloque de medición A2-P2» en lugar de «invocación medida adicional de A4-1»; se añade la prohibición de leer fuera del clon
  y de su origen (equivalente a `ForbiddenInputs` de `role-invocation/v1`, D.6);
- sin `{{SHELL_DECLARATION}}` y sin pistas de shell (las de v3 nombraban `cmd.exe`, `&`, `echo` y `findstr /n`);
- paso 1: «el intérprete con el que se ejecutan tus comandos, con su ruta, y el directorio de trabajo» (claves `shell`, `cwd`) en lugar de `ver`,
  `%ComSpec%` y `cd`;
- pasos 7, 14, 16 y G: «el SHA-256 del archivo» y «comparando los dos archivos» en lugar de `certutil` y `fc`.

**Shell realmente usada** (`compare` → `NativeRoute`, leída de los eventos `--json`): la primera palabra de cada `command_execution` clasifica la
shell (`pwsh-runtime` si la ruta contiene `codex-runtimes`, `pwsh-windowsapps`, `pwsh-other`, `powershell`, `cmd`, `bash`, `direct:<exe>`);
`DefaultShell` = la del primer comando; `ModelFallback` = otra shell tras un fallo. **Firma F6-OBS-03**: `CreateProcessAsUserW failed` o
`Failed to create unified exec process` en la salida de un comando fallido o en `probe-stderr.txt` → `ShellLimitationPersists = true` y
`CmdA41Opens = true`.

**Cobertura** (operación → paso → criterio): idéntica a la tabla de v3 §2 (A-G, 32 pasos), salvo la fila de la shell:

| Operación | Paso | Criterio de `compare` |
|---|---|---|
| Ejecución con la receta literal | 1 + eventos | claves `shell` (presente) y `cwd` (= toplevel); ≥ 1 comando completado con código 0; ningún comando **completado** que escriba (Git o archivos) o use la red (§4); la shell observada y la firma F6-OBS-03 se registran |
| Git y hashes; fidelidad; TRX y CI; mapa I62 (AP 16.13); esquemas canónicos; nc1-nc4, N4, N5, N7a, N7b | 2-32 + F | los de v3 sin cambio (`OP_B`..`OP_G`) |

**Veredicto** (propuesta; decide el Coordinator): `ALL_OPERATIONS_DEMONSTRATED` exige las siete operaciones, salida válida, proceso `exit=0` sin
tope, identidad estable, árbol e ignorados sin cambios, retirada correcta, `ls-remote` estable **y ninguna firma F6-OBS-03**. Con la firma, el
veredicto es `NOT_DEMONSTRATED` aunque el modelo haya improvisado otra shell y demostrado todo: por A4-1 («el runtime … rechaza el shell que él mismo
selecciona») la vía normal no queda acreditada y se abre `CMD_A41`. `ALL_OPERATIONS_DEMONSTRATED` haría elegible al Controller **por la vía normal**
(sin A4-1); `REFERENCE_UNSTABLE` como en v3.

## 3. `NATIVE_ARCHITECT` (sonda 2 del bloque)

**Qué necesita el Architect de FX-02** (contrato T1, `RoleRequirements` ARCHITECT / REVIEW_DESIGN; `controller-contracts.md` §7; descriptor
`codex-cli`): `Mandatory` = `ARCHITECTURE_REVIEW.effort` (Deep → `high`), `.level` (Equilibrado o Frontera; `gpt-6.1-sol` = Equilibrado en el
catálogo) y `.read`; `Target` = `{d30fb6a9…, docs/automation/decisions/FX-U1-T1.gate-contract.json, 628d89af…}`; contrato de salida
`rackcad-architect-review-result/v1` (operación 5 del adapter). La 0/6 de A2-P2 fue una falta de `.read`: ningún comando se ejecutó.

| Paso | Qué mide | Claves | Grupo |
|---|---|---|---|
| 1 | shell observada, `cwd`, `HEAD`, rama (+ eventos: ejecución, escrituras, red) | 4 | `ARQ_A_entorno` |
| 2-3 | objeto revisado: tipo del commit, blob en el `Target` y en HEAD, ascendencia; JSON de K con fidelidad (`Objective` con 4 no ASCII, `Invariants`, `StopConditions[1]`, requisitos e independencia del ARCHITECT, longitudes) | 5 + 17 | `ARQ_B_objeto_revisado` |
| 4 | las 6 autoridades de K en su revisión (AR para UNIT_*, MainSha para EXTERNAL): revisión, blob, línea del encabezado (`## Jobs requeridos` = 22; `## 16.` = 429), primera línea textual (incluida la de §16 con `§`, `á` y `ó`); recuento de no ASCII de la línea 1 de `FX-U1.md` en AR (`—` U+2014 y `ó`) | 22 | `ARQ_C_autoridades_fieles` |
| 5 | negativos: código de `cat-file -t` de un SHA inexistente (128), blob señuelo (no), ruta ausente en AR (128) | 3 | `ARQ_D_controles_negativos` |
| 6 + E | blob del esquema canónico; `architect_review_result_v1` válido contra el canónico; sobre exacto (`ResultId`, `LogicalReviewRequestId`, `InvocationId`, `AttemptSeq`, rol, acción, `Reviewed*`, `ReviewerBinding`, `ReviewerMode`, `InputFidelityEvidenceRef`); `IndependenceEvidence` con las 4 dimensiones frente a AUTHOR y `ReviewSubject` null; `InputsRead` ⊇ contrato + 6 autoridades; cada `PremiseRefs.Quote` textual en sus líneas | 1 + comprobaciones | `ARQ_E_salida_canonica` |

**Esquema canónico**: incrustado como propiedad obligatoria `architect_review_result_v1`, igual que v3 incrustó `delegation.v2` y
`controller-verification.v2`: textual salvo la palabra `$schema` (`embed()` de v3, sin cambio). Auditoría estricta: raíz objeto sin `anyOf`, todo
objeto con `additionalProperties: false` y todas las propiedades en `required`; palabras `type`, `enum`, `pattern`, `items`, `description`,
`title`, `minLength` (45) y `minimum` (7), las mismas familias que A4-1 hizo aceptar; el esquema de salida de la sonda 2 tiene 6 niveles, 131
  propiedades y 56 valores de enum; 3 objetos
anulables (`type: [object, null]`), como `delegation.v2`, que se aceptó.

**Clasificación de la celda** (`ArchitectCellClassification`, propuesta):
- `MATCH_OR_ABOVE_REQUIRED`: las cinco agrupaciones demostradas, proceso y árbol correctos, identidad estable y, para ser final, modelo y effort
  efectivos (`effective-model-effort.json`, que escribe la supervisión desde el registro de sesión) = `gpt-6.1-sol` / `high` o superior. Sin ese
  archivo la clasificación es provisional (`ClassificationFinal = false`). Consecuencia: A4-2 no se activa.
- `BELOW_REQUIRED` con `BelowCauses` ⊆ {`EXECUTION_REJECTED` (firma F6-OBS-03 sin ningún comando completado: el caso de la 0/6),
  `NO_COMMAND_EXECUTED`, `READ_NOT_DEMONSTRATED`, `OUTPUT_CONTRACT`, `WRITE_OR_NETWORK`, `PROCESS_EXIT_OR_TIMEOUT`, `WORKTREE_CHANGED`,
  `EFFORT_BELOW_REQUIRED`}. Consecuencia: con disposición del Coordinator, A4-2 permite otro adapter medido elegible (claude-cli, con su propia
  medición `CLAUDE-CLI-MEDICION-FX02`).
- `MEASUREMENT_VOID`: identidad inestable (A2-P2, regla 6) o modelo efectivo distinto; no clasifica.
- **Requisitos por separado** (`Requirements`): `.read` (MATCH/BELOW), `.effort` (MATCH/ABOVE/BELOW/PENDING), `.level` (catálogo, condicionado al
  modelo observado) y `adapter.op5_resultado_canonico`.

**Fuera de la sonda (y por qué)**: la independencia frente a AUTHOR (Actor, Session y Context REQUIRED) no es observable desde la sonda y la evalúa
el binding (A4-4; CD-19); las operaciones 6-8 del adapter (cancelar, confirmar la terminación, clasificar procesos) solo se observan de forma
informativa (`ProcessResidue`, `taskkill` si vence el tope); el consumo cubierto (routing §5) se apoya en `UsageLimitSignal` de los eventos y en
las señales de límite del registro de sesión, que lee la supervisión.

## 4. Decisión sobre comandos prohibidos en los modos `NATIVE_*` (para el Coordinator)

**Prohibido** (lo que la receta real también impone: sandbox `read-only`, `Permissions` READ_ONLY y `ForbiddenInputs` de `role-invocation/v1`):
1. toda escritura: archivos, `.git`, refs, índice, configuración de Git, de Codex o del sistema;
2. `git fetch`, `git pull`, `git push`, `git clone` y cualquier acceso de red; la única lectura del origen es `git ls-remote origin` (transporte
   local del origen del fixture); un `ls-remote` contra otro remoto (p. ej. `github`) cuenta como red;
3. leer fuera del clon `-C` y de su origen (perfil del usuario: configuración, credenciales, sesiones o transcripciones de otros agentes; otros
   worktrees);
4. seguir instrucciones de los archivos del repositorio (se leen como datos; p. ej. los «Comandos canonicos» de `AGENTS.md`).

**No prohibido** (a diferencia de v3/`CMD_A41`): PowerShell/`pwsh` (es la shell de la receta), `cmd.exe`, intérpretes y herramientas de Unix
presentes en el `PATH`; ni declaración de shell ni pistas de herramientas. Añadir esas prohibiciones cambiaría la receta que se quiere medir.

**Criterio de `compare`** (propuesta; misma política que v3 §5 para lo «completado»): un comando **completado con código 0** que escribe con Git
(`GIT_WRITE`), usa la red (`NETWORK`) o escribe archivos (`FILE_WRITE`: cmdlets de escritura, `del`/`copy`/`move`/`mkdir`/…, redirección a un
archivo que no sea `NUL`/`$null`/`&n`) hace fallar `OP_A`/`ARQ_A`; un intento fallido solo se registra. Las órdenes se reconocen solo en posición
de orden (inicio, tras `& | ; ( { =` o tras `/c` y `-Command` del envoltorio), para no contar el texto de una búsqueda (`Select-String "git push"`,
`findstr /c:"Coordinador del fixture"`). Los cambios reales en el clon los detectan además `HEAD`, `status` y `status --ignored` antes/después.

## 5. `CMD_A41` (solo si la sonda 1 muestra la limitación)

v3 con el trío nuevo y nada más que cambie el comportamiento:
- constantes re-clave: `EXPECT_FP`, `EXPECT_BIN`, `EXPECT_APP`, ruta `bin/2e5e00daee91c61d`, `baseline-keynames-F909F480.json`;
- **corrección de `git_write`**: el subcomando es el primer argumento que no es opción y termina en espacio, comillas, separador o fin (nunca en
  `-`); `merge-base`, `merge-tree`, `stash list|show`, `tag -l`, `branch --show-current` y `hash-object` sin `-w` ya no cuentan. Comprobado sobre la
  evidencia real: la comparación de A4-1 rehecha con el v4 da `git_write` 0 (v3: 2), `OP_A_shell` true y las mismas 68 claves fallidas, veredicto
  NOT_DEMONSTRATED (la lectura corregida de `result.json`);
- textos sintéticos del escenario con el nombre del kit (`kit a2p2-v4`): el texto armado difiere del de v3 solo en `IssuedBy` y en dos `Sha256` de
  bindings sintéticos (diff por palabras hecho);
- puertas propias: disposición de aplicación de A4-1 para el trío nuevo (modo, celda, `cmd.exe`, trío y `-C`), `A4-SONDA-CONSUMO` del trío nuevo,
  OD-2 sobre `F909F480…`, la comparación de la sonda 1 con `ShellLimitationPersists = true` y exactamente una medición `*a4-1-probe*` previa
  (`R20261010T000542Z-a4-1-probe`).

## 6. Líneas literales del Owner (una por línea; borradores byte a byte en `od2-line.txt`, `bloque-consumo-line.txt` y `a41-consumo-line.txt`)

Para el bloque (antes de cualquier sonda; A2-P2, regla 4, y P-01 vigente desde la evidencia §125):

```text
OD-2 = A (línea base F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113; binario d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec)
BLOQUE-CODEX-CONSUMO = A (≤ 2 invocaciones read-only de codex-cli para el par exacto d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec/26.1007.2314.0, con huella, binario y versión antes y después de cada una; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores)
```

Condicional, aparte, solo si la sonda 1 muestra F6-OBS-03 y el Coordinator dispone `CMD_A41`:

```text
A4-SONDA-CONSUMO = A (1 invocación read-only de codex-cli fuera de A2-P2, celda gpt-6-luna/high del Controller de FX-02 con la shell declarada cmd.exe, para el trío exacto d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec/26.1007.2314.0/F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113, con huella, binario y versión antes y después; sin reintento; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores; no acepta su resultado)
```

Modelos, copiados del archivo de decisiones y cambiados solo en el par, el trío o la huella: la línea `OD-2` de §65; la línea
`BLOQUE-CODEX-CONSUMO` de §58 (la registrada, que nombra el hash completo y añade «sin lectura de valores»); la línea `A4-SONDA-CONSUMO` de §65.
Tamaños: 161, 322 y 501 bytes, sin fin de línea. La clave `BLOQUE-CODEX-CONSUMO` se repite a propósito (la puerta exige la línea completa, que
nombra el par nuevo); si el Coordinator prefiere otra clave para distinguirla de §58, basta cambiar el borrador antes de pedirla.

## 7. Disposiciones del Coordinator necesarias (y literales que esperan las puertas)

- **D-1. Estabilidad del par** (A2-P2, regla 3): una segunda observación pasiva, sin modelo, con el mismo `BinaryHash`, `AppVersion` y huella
  `F909F480…`, custodiada en la evidencia. Su texto es `A2P2_STABILITY_MARKER` (tiene que nombrar la huella).
- **D-2. Bloque A2-P2** para el par exacto, con las dos sondas. Literales candidatos (`gates.env.template`):
  - `BLOQUE-CODEX = AUTORIZADO bajo A2-P2 para el par exacto BinaryHash d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec, AppVersion 26.1007.2314.0, observado con su huella resultante estable F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113`
  - `Sonda 1 (NATIVE_CONTROLLER): celda codex-cli / gpt-6-luna / high, receta 16.4 literal sin shell declarada, directorio de medición D:\r62-fixture\A2, escenario S sintético en su área ignorada`
  - `Sonda 2 (NATIVE_ARCHITECT): celda codex-cli / gpt-6.1-sol / high, receta 16.4 literal sin shell declarada, directorio de medición D:\r62-fixture\arch-a2p2v4, clon de lectura nuevo`
- **D-3. Directorios**: A2 para la sonda 1 (fue el `-C` de la A4-1 anterior y quedó limpio; las puertas repiten clon fresco, punta, ignorados
  vacíos y ausencia de proyecto de Claude) y `D:\r62-fixture\arch-a2p2v4` para la sonda 2, creado por `make-arch-clone.ps1` (no ejecutado; sin
  remoto `github`, sin identidad de autor).
- **D-4. Política de comandos** de §4 y criterio de `OP_A`/`ARQ_A` (completado frente a intentado).
- **D-5. Lectura de los veredictos**: sonda 1 `ALL_OPERATIONS_DEMONSTRATED` solo sin F6-OBS-03 (vía normal, sin A4-1); con la firma, se abre
  `CMD_A41`. Sonda 2: reglas de §3 y Q-ARQ-FALLBACK (§8).
- **D-6. `CMD_A41`** (condicional): disposición de aplicación de A4-1 para el trío nuevo. Literal candidato:
  `CMD_A41: aplicación de A4-1 con el trío nuevo; celda codex-cli / gpt-6-luna / high; shell declarada cmd.exe; trío BinaryHash d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec, AppVersion 26.1007.2314.0, Fingerprint F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113; directorio de medición D:\r62-fixture\A2`
- **D-7. `CANONICAL_SCHEMA_MODE`**: se propone VERBATIM (A4-1 aceptó los canónicos v2 con `minLength`; el canónico del Architect usa las mismas
  familias).
- **D-8. Momento de la OD-2**: antes de la primera sonda (A2-P2, regla 4). En §58 la OD-2 se pidió después de las sondas; con A-2 AGREED y el
  P-01 vigente, el kit exige la línea antes de cualquier modo.
- **D-9. Custodia**: `<RunId>-a2p2-v4-controller`, `<RunId>-a2p2-v4-architect` y `<RunId>-a4-1-probe-2` en `docs/automation/evidence/I-62-F6/OD-2/`;
  la elegibilidad del Controller (vía normal o A4-1) y la activación o no de A4-2 las declara el Coordinator.

Se heredan de v3, tal como se lanzaron en A4-1: Q-ATT (`AttemptsRemaining` = 3 − 0), Q-MODE (`Mode` NORMAL en la forma I62) y la adaptación de nc2.

## 8. Riesgos y puntos abiertos

- **Sandbox nuevo (material).** El paquete trae `codex-command-runner.exe` y `codex-windows-sandbox-setup.exe`: el lanzamiento de comandos bajo
  `-s read-only` puede haber cambiado (para bien o para mal) y la primera invocación del binario nuevo podría ejecutar la configuración del
  sandbox de Windows. Si pide elevación o cambia ACL o cuentas, es un cambio de sandbox que la línea de consumo excluye: STOP y registro. No
  medido. Los nombres nuevos casan con `codex*.exe` de la lista cerrada de 16.4 (solo se excluye `codex-windows-sandbox-service.exe`): si alguno
  sobrevive con la ruta del worktree en su línea de órdenes, sería participante en la Entrada (P-02). `ProcessResidue` lo deja a la vista.
- **`$schema`.** Coherente con v3: los canónicos van incrustados sin `$schema`. La receta real pasa el archivo entero (con `$schema` en la raíz);
  el piloto de I-61 lo aceptó con esquemas v1, pero no está medido con el binario nuevo. El riesgo R1 de `claude-cli` (Ajv draft-07) no aplica a
  `codex-cli`.
- **Modo estricto con el binario nuevo.** A4-1 aceptó `minLength` y `minimum` con la CLI anterior; un rechazo con la nueva consumiría la sonda al
  instante (sin reintento) y sería, a la vez, el hecho decisivo (la receta real tampoco funcionaría). `STRICT_PROJECTION` queda preparada.
- **Tiempo (600 s).** A4-1 terminó a los ~100 s con el 83 % del tiempo sin usar, por omisión y no por tope. La sonda 1 tiene el mismo tamaño
  (20 604 caracteres); `pwsh` del sandbox corre en ConstrainedLanguage (descriptor), lo que puede costar reintentos al modelo. Sonda 2: 7 392
  caracteres y 52 claves; estimación 60-200 s.
- **`ls-remote` bajo el sandbox** falló en A4-1 («Could not read from remote repository»); si sigue fallando, `OP_B` no se demuestra
  (`REFERENCE_UNSTABLE` solo cubre un origen que cambia).
- **Q-ARQ-FALLBACK (abierta).** Si la sonda 2 muestra F6-OBS-03 pero el modelo recurre por su cuenta a otra shell y lee todo bien, la
  propuesta clasifica MATCH_OR_ABOVE_REQUIRED con `ModelFallback = true` y lo marca: A4-1 solo cubre al Controller y A4-2 exige BELOW_REQUIRED; si
  el Coordinator considera que una lectura por una ruta improvisada no satisface `.read`, la clasificación pasa a BELOW_REQUIRED.
- **Q-STAB.** La observación 1 (03:47Z) es de 56 min después de crearse el directorio del binario y 53 min después de escribirse la huella; la
  regla 3 pide el par «ya observado con su huella resultante estable»: D-1.
- **A2 compartido.** La sonda 1 y una eventual `CMD_A41` usan A2, que también es el clon del titular A2 de FX-02; las puertas exigen que esté
  fresco y sin proyecto de Claude antes de cada una, y el escenario S se retira con comprobación.
- **Detección por expresiones regulares.** Las categorías de escritura y red son heurísticas (posición de orden); un falso positivo haría
  fallar `OP_A`. Se ensayaron 16 casos de expresión, las trampas de búsqueda de texto y los tres registros de eventos reales sin un solo falso
  positivo; la supervisión revisa `EventsCommands` antes de proponer.
- **Lo que el script no puede comprobar** (lo hace la supervisión): que los literales de `gates.env` sean de verdad el registro; los invalidadores
  que no observa (autenticación, blobs de catálogo y routing, instancia del host); modelo y effort efectivos (registro de sesión).

## 9. Procedimiento (solo con las puertas)

1. D-1 (observación 2) y D-2/D-3 del Coordinator; OD-2 y `BLOQUE-CODEX-CONSUMO` del Owner, literales.
2. `pwsh -NoProfile -File make-arch-clone.ps1 -ExpectedTip 95bdc29d428b35d2188a10e1c5462d6caf2be4df` (solo tras D-3).
3. `cp gates.env.template gates.env`; sustituir los literales del modo; congelar (SHA-256 de los archivos, de `gates.env` y del texto armado).
4. `bash run_probe.sh probe NATIVE_CONTROLLER` (una vez) → `bash run_probe.sh compare NATIVE_CONTROLLER`.
5. La supervisión lee del registro de sesión de esa invocación solo modelo, effort y señales de límite, sanea y custodia en
   `EVIDENCE_ROOT/<RunId>-a2p2-v4-controller/`.
6. `bash run_probe.sh probe NATIVE_ARCHITECT` → la supervisión escribe `out/native-architect/effective-model-effort.json` (`{"Model": …,
   "Effort": …}`) → `bash run_probe.sh compare NATIVE_ARCHITECT` → custodia en `<RunId>-a2p2-v4-architect/`.
7. Solo si la sonda 1 dio `ShellLimitationPersists = true`: D-6 y `A4-SONDA-CONSUMO` → `probe CMD_A41` → `compare CMD_A41` → custodia en
   `<RunId>-a4-1-probe-2/`.
8. Sin reintento en ningún modo. Una diferencia de identidad en cualquier medición es STOP (exit 3) y anula el bloque (A2-P2, regla 6).

## 10. Autoensayo (obligatorio; hecho, sin modelo)

Clones propios de `D:/r62-fixture/fixture-origin.git` (`fx/u1`, `95bdc29d`, `--no-local`, `core.autocrlf=false`):
`D:\r62-fixture\tmp-a2p2v4-selftest` (Controller) y `D:\r62-fixture\tmp-a2p2v4-arch` (Architect, solo para el arnés de puertas); los dos se
borraron al final. Detalle en `selftest-report.json`.

| Prueba | Resultado |
|---|---|
| `CMD_A41`: los 20 casos de v3 + C1 (`merge-base` no es escritura → ALL) + C2 (`fetch` completado → NOT) | 22/22 |
| `NATIVE_CONTROLLER`: P0 `pwsh` del runtime → ALL; P1 `cmd` sin firma → ALL; P2 intérprete permitido → ALL; P3 `merge-base`/`stash list`/`tag -l` → ALL; P4 `fetch` fallido solo se registra → ALL; P5 trampas de búsqueda (`"git push"`, `"del"`, URL) → ALL; F1 firma F6-OBS-03 + recurso a `cmd` → NOT con `ShellLimitationPersists`, `ModelFallback` y `CmdA41Opens`; F2 firma sin recurso y sin salida → NOT; F3 firma solo en stderr → NOT; `fetch`/escritura/redirección/red/`ls-remote github` completados y clave `shell` ausente → NOT; 11 negativos de v3 (clave ausente, hash, TRX, mapa, no ASCII, origen cambiado → REFERENCE_UNSTABLE, canónico inválido, tope, área ignorada, nc2, check) e identidad inestable → NOT | 27/27 |
| `NATIVE_ARCHITECT`: P0 → MATCH_OR_ABOVE_REQUIRED final; P0b sin observación → provisional; P0c effort `xhigh` → ABOVE; F1 réplica de la 0/6 de A2-P2 → BELOW (`EXECUTION_REJECTED`); F2 firma + recurso → MATCH con `ModelFallback`; raya `—`→`-`, blob, encabezado ausente → BELOW (`READ_NOT_DEMONSTRATED`); `Verdict` fuera del enum, `ReviewedBlob`, cita no textual, `InputsRead`, independencia incompleta → BELOW (`OUTPUT_CONTRACT`); tope, effort `medium`, `fetch`, área ignorada → BELOW; identidad o modelo distintos → MEASUREMENT_VOID | 19/19 |
| Esquemas: salidas sintéticas válidas contra el esquema de salida del modo y contra los canónicos (validador local; `jsonschema` no está instalado), salvo las negativas que lo violan a propósito | cumple |
| `stage`/`unstage` (Controller): `status --ignored` vacío → `!! artifacts/` → `!! artifacts/` → vacío | restaurado (los dos modos) |
| Arnés de puertas (`selftest-gates.sh`, `check_gates` real con un registro sintético): 4 casos que pasan (G01, G17 con STRICT_PROJECTION, A01, C03) y 30 negativas (sin OD-2, sin consumo, par o trío anterior, marcador `<…>`, directorio no fijado, sin modo, sin estabilidad, ya lanzada, ya custodiada, tope del bloque, clon con ignorados, mismo directorio, directorio no admitido, salidas previas, BAD_SHA existente, sin `gates.env`, A2 para el Architect, blob del objeto, ruta ausente existente, disposición de otro modo, `CMD_A41` sin sonda nativa / sin limitación / sin consumo / sin `cmd.exe` / con una segunda A4-1 custodiada / ya lanzada) | 34/34 |
| Auditor sobre eventos reales custodiados: A2-P2 sonda 1 (firma 3/3 en `pwsh-windowsapps`, recurso a `cmd`), sonda 2 (firma 6/6, sin recurso, 0 completados), A4-1 (`cmd`, sin firma, `git_write` 0) | reproduce lo custodiado |
| Comparación de A4-1 rehecha con `CMD_A41` sobre una copia de la evidencia | `OP_A` true, `git_write` 0, 68 claves fallidas iguales, NOT_DEMONSTRATED |
| Textos armados (dry run con `gates.env.template`) | NATIVE_CONTROLLER 20 604 car. `f88d3313…`; NATIVE_ARCHITECT 7 392 car. `4d7adb7a…`; CMD_A41 20 751 car. `455f9034…` |
| `bash -n` (`run_probe.sh`, `selftest-gates.sh`), `py_compile -W error` (`probe_tools.py`), analizador de PowerShell (3 `.ps1`), `procs-snapshot.ps1` ejecutado (lista 5 procesos `codex*`, ninguno con el directorio) | sin errores |

## 11. Cambios frente a v3

| Archivo | Cambio |
|---|---|
| `run_probe.sh` | modos con marcador, salida (`out/<modo>/`) y patrón de evidencia propios; identidad nueva; puertas por modo (§7); P-07 del bloque (≤ 2) y de A4-1 (exactamente una previa); OD-2 obligatoria en los tres; `pwsh` del runtime comprobado; huella de acompañantes en `measure`; instantánea de procesos; `-C` desde `CONTROLLER_PROBE_DIR`/`ARCHITECT_PROBE_DIR`; directorios no admitidos ampliados (`fx05*`, `probe-od4-1`; A2 para el Architect); se carga como biblioteca para el arnés |
| `probe_tools.py` | `PROBE_MODE`; textos sintéticos con el rótulo del modo; esquemas por modo; plantilla por modo (la shell solo en `CMD_A41`); paso 1 nativo; `audit_events` ampliado (shell, F6-OBS-03, red, escritura, límite de uso) y `git_write` corregido; `compare` por modo; referencia, comparación, síntesis y casos del Architect; autoensayo por modo; `audit-events` |
| `probe-prompt.native-controller.template.txt` | nuevo (v3 sin `cmd.exe`, §2) |
| `probe-prompt.native-architect.template.txt` | nuevo |
| `probe-prompt.cmd-a41.template.txt`, `shell-declaration.txt`, `cfg-fp.ps1` | sin cambio (byte a byte v3) |
| `probe-schema.*.json` (6) | por modo; los de `CMD_A41` = v3 salvo el nombre del kit en `description` |
| `gates.env.template` | literales por modo con candidatos; operaciones del Architect |
| `od2-line.txt`, `bloque-consumo-line.txt`, `a41-consumo-line.txt` | borradores de las líneas del Owner (sustituyen a `consumo-line.txt`) |
| `baseline-keynames-F909F480.json` | copia de contenido idéntico de `baseline-keynames-73890CA3.json` |
| `procs-snapshot.ps1`, `make-arch-clone.ps1`, `selftest-gates.sh` | nuevos |

## 12. Archivos (SHA-256)

| Archivo | Uso | SHA-256 |
|---|---|---|
| `run_probe.sh` | `measure`, `probe <MODO>` y `compare <MODO>`, con las puertas por modo | `de9bc7bfa66145e7634d7cd3efd0248e1cc99944d5a4c2df8037db940121768a` |
| `probe_tools.py` | esquemas, textos, escenario S, referencia, comparación, auditor de eventos y autoensayo por modo | `f0f1a8f77af6b867e71c46a1d6b49e518647cd45f85017f258bdef4cee6bb543` |
| `probe-prompt.native-controller.template.txt` | texto de la sonda 1 (32 pasos, sin shell) | `1afc221475505bd5b3fbfcf7e2d56d941c3487474cecda91d9bf75bb9a03241c` |
| `probe-prompt.native-architect.template.txt` | texto de la sonda 2 (6 pasos, sin shell) | `af996d146bf225963be9ea1db46959b0d8e139fe266e6d279f48af061cc6a1d8` |
| `probe-prompt.cmd-a41.template.txt` | texto de CMD_A41 (= v3) | `b16c3f49539bf35341ef44810501ac4f97835a0dbbf5b85bcddbecfaa8c50b96` |
| `shell-declaration.txt` | declaración de shell de CMD_A41 (= v3) | `062c234e06ab718cfd154434967460249d67fe8a8a7a8c0573bf4653ac19faf0` |
| `probe-schema.native-controller.json` | esquema de salida de la sonda 1 (VERBATIM) | `5c91208f7f3a067fa1591e55706e4e3beaea580ee0ccdfaade5c4132e647eeb4` |
| `probe-schema.native-controller.strict-projection.json` | ídem, STRICT_PROJECTION | `9b9ddba7b64079a80872bb4226aec71e33f3e90053f11c8cb573cbfcf09bfc7c` |
| `probe-schema.native-architect.json` | esquema de salida de la sonda 2 (VERBATIM) | `80d54fb393945510db66811a174756c6343ba0733c76af9c1fb7b5d05b5435e1` |
| `probe-schema.native-architect.strict-projection.json` | ídem, STRICT_PROJECTION | `766c98de3be2d44bd76ed70277b149a83817dce3fd260bde7184f45bab5449a4` |
| `probe-schema.cmd-a41.json` | esquema de salida de CMD_A41 (VERBATIM; = v3 salvo `description`) | `30ee467b25b0746d6ce7d483dc74e33d68577d66c3761f5bd14985c7d46f5b90` |
| `probe-schema.cmd-a41.strict-projection.json` | ídem, STRICT_PROJECTION | `0ee76a2e723c3f7359135dd2d4d8779f62dd0ad5a06ce27e8a772be25eb1ed0d` |
| `gates.env.template` | puertas y parámetros; literales por rellenar con candidatos | `c6138c3b7c737c8cf4b004722e3b6dca67784f5e0a4badebc866995e13d7116b` |
| `od2-line.txt` | borrador de la línea OD-2 sobre F909F480… (161 bytes) | `b76470163f86592dfda474d44c5e2051d1e05ead667362fb8428c32ef7d4ac96` |
| `bloque-consumo-line.txt` | borrador de BLOQUE-CODEX-CONSUMO del par nuevo (322 bytes) | `953088c953075af184a017fcf48a98c5540ee0cc4e5123edf719fc969f57bf5b` |
| `a41-consumo-line.txt` | borrador condicional de A4-SONDA-CONSUMO del trío nuevo (501 bytes) | `4b081e4e37a54fefca931b1ad3a5a621c64bc9c298e9b82333710c8ee9381746` |
| `cfg-fp.ps1` | huella de config.toml (SHA-256 y nombres saneados); sin cambio, no ejecutado | `b1d833c0abc06fec6e27367b6df2715e7cdca321f0d6c6fbadb9f634e6a31bab` |
| `baseline-keynames-F909F480.json` | los 107 nombres saneados (contenido idéntico al de 73890CA3) | `076637c550ff0b3109447ef00263ff087749f0aaa71f299d524e43b8cf82b70b` |
| `procs-snapshot.ps1` | instantánea informativa de procesos codex*.exe | `f879db305dd296d30241cca75cf61ac0a5828077cd2be6277e4128f934afedaf` |
| `make-arch-clone.ps1` | preparación del clon del Architect (no ejecutado) | `f9d1c14c69959a6fd70c2e5b4cd16d88401956089227739d8a1ce791d3242696` |
| `selftest-gates.sh` | arnés de las puertas (solo autoensayo) | `bcb51b8feaed69672606d4af6fd7b2863fd351213e07b1981f3c08754955d089` |
| `selftest-report.json` | informe del autoensayo | `352fecf03d697d92ad573f3a08bdaeb87d8501592d74c65feec6148ee6a6b458` |

`README.md` no se lista a sí mismo; su SHA-256 se registra al congelar.
