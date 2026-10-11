# Kit a2p2-v5 — sonda 2 del bloque A2-P2 reasignada al Controller, modo `CONTROLLER_V5` (NO EJECUTADO; para revisión y congelación)

> **No se ha ejecutado y no está congelado.** Lo prepara la supervisión (plano a). Para prepararlo no se invocó `codex.exe`, `codex`, `claude` ni
> ninguna CLI de modelo (tampoco `--version` ni `login status`); no se leyó nada bajo `%USERPROFILE%\.codex` salvo el worktree de I-62 (sin
> escribir en él); no se ejecutó `cfg-fp.ps1`; no se escribió en `D:\r62-fixture\A2`, en `fixture-origin.git` ni en ningún directorio existente de
> `D:\r62-fixture`. El autoensayo usó un clon propio, `D:\r62-fixture\tmp-v5-selftest`, ya borrado (§9). Sin las puertas de §6,
> `run_probe.sh probe CONTROLLER_V5` se niega a correr.

## 0. Autoridad y hechos

- **Disposición (decisiones §73, puntos 4-7).** Opción B: la segunda sonda disponible del bloque A2-P2 se reasigna al Controller; la autorización
  del Owner cubre hasta dos invocaciones read-only para el par exacto y una ya se consumió; sin reiniciar el presupuesto ni repetir la sonda 1.
  Celda `codex-cli / gpt-6-luna / high`; identidad `d13914ce…2a7daec` / `26.1007.2314.0` / `F909F480…1C8113`. Diseño: una invocación conjunta
  de PLAN y VERIFY, distinta de la anterior; sin «Tiempo limitado»; respuesta acotada sin omitir requisitos; sin los controles negativos agregados
  salvo que su ausencia impida demostrar una capacidad REQUIRED; prueba inicial de UTF-8 del `pwsh` nativo, midiendo (sin suponer) si
  `OutputEncoding` se puede fijar bajo ConstrainedLanguage; ni sandbox, ni PATH, ni receta, ni permisos, ni configuración cambian.
- **Bloque** (§69): `BLOQUE-CODEX = AUTORIZADO` para el par exacto; líneas del Owner `OD-2 = A` y `BLOQUE-CODEX-CONSUMO = A` (líneas completas
  1424 y 1425 del archivo de decisiones en `06b92300`; `od2-line.txt` y `bloque-consumo-line.txt`, byte a byte las del kit v4). Estabilidad del
  par: evidencia §127.
- **Directorio** (§55, punto 10, U-03): `-C` del Controller de FX-02 = `D:\r62-fixture\A2`.
- **Sonda 1** (`R20261011T050156Z-a2p2-v4-controller`; evidencia §128; análisis `FX-02/s70` y `FX-02/s72`): NOT_DEMONSTRATED; `pwsh` del runtime
  sin F6-OBS-03; 4 comandos y parada a los ~105 s de 600; ~12,4 k tokens de salida; respuesta de ~24,7 k caracteres, incompleta; el texto no ASCII
  llegó ya como U+FFFD en la salida cruda del comando (consola de `pwsh` no UTF-8; defecto del transporte); `ls-remote` bloqueado por el sandbox.
  Con `cmd.exe` (A4-1) la salida cruda era UTF-8 correcta y el fallo fueron escapes mal formados del modelo (`\u0000f3`).
- **Kit de partida:** `kits/FX-02/a2p2-v4-run` (congelado en `7bcc48f5`). Se conservan su maquinaria (medición de identidad con acompañantes,
  escenario S, instantáneas, receta literal, tope de 600 s, auditor de eventos con la corrección de `git_write`) y se retiran sus tres modos.

## 1. Qué cambia frente a la sonda 1 (no es una repetición)

| Aspecto | Sonda 1 (a2p2-v4, `NATIVE_CONTROLLER`) | Sonda 2 (a2p2-v5, `CONTROLLER_V5`) |
|---|---|---|
| Texto armado | `f88d3313…` (20 604 caracteres) | ``52aa85d40490f806999e57cf4b14e6857cf3aa33311fd48a8e800172cd87fbbd`` (21 846 caracteres); las puertas rechazan un texto igual al de la sonda 1 |
| Presión de tiempo | «Tiempo limitado. Agrupa … en pocas invocaciones … No repitas lecturas» | ninguna; la plantilla y el armado rechazan esas fórmulas |
| Instrucción de completar | ninguna | «Ejecuta todos los pasos, del 0 al 23, en orden, y responde solo cuando hayas ejecutado el último … para solo después del paso 23» |
| Proceso de la shell | no se decía | «cada comando arranca un proceso nuevo de la shell: lo que ajustes en uno no se conserva en el siguiente» |
| Fidelidad UTF-8 | solo copias exactas en los pasos 11-13 | paso 0 nuevo (§2): salida por defecto, intento medido de fijar `OutputEncoding`, relectura, método elegido y comprobado; las lecturas posteriores usan ese método |
| Pasos / claves | 32 pasos, 175 claves | 24 pasos (0-23), 158 claves |
| Controles negativos | 8 en la misma invocación (nc1-nc4, N4, N5, N7a, N7b) | ninguno (§4) |
| `git ls-remote` | paso 9 | retirado; Remote se comprueba en el registro (AP 16.9, fila 6); un `ls-remote` completado cuenta como red |
| `MainSha` de PLAN | `ls-remote origin refs/heads/main` | `refs/remotes/origin/main` del clon; la puerta exige que sea igual a `ls-remote origin main` antes de lanzar («`origin/main` recién obtenido», AP 16.5 A6) |
| `commands` de cada paso | texto completo (≈ 2,2 k caracteres repetidos en la sonda 1) | solo identificadores `C<n>` (patrón `^C[1-9][0-9]*$`), contrastados con los eventos |
| Claves duplicadas | `HEAD`, `branch`, `toplevel`, `origin-main`, `paths-2`, `identical`, `json:Objective`, `json:Invariants`, `json:MainSha`, `json:AuthorityRevision`, `json:RoutingEnforcement`, `json:RoleRequirements[1]…`, `json:RequiredTests[0].Filter`, `handoff:CurrentSha` | retiradas: las llevan los objetos canónicos, que se comparan exactos |
| `Evidence` de `Checks` | ≤ 300 caracteres | ≤ 150 (Scope sin tope: AP 16.22 exige diff y entrada por ruta) |
| Escenario S | entradas + 8 copias de negativos | las mismas 5 entradas (delegación, entrega, hechos del relevo, TRX actual y RED), sin copias; rótulo «sonda 2 del bloque A2-P2 (Controller, v5)» |
| Comparador | grupos `OP_A`..`OP_G` | `OP_U0`..`OP_F`, acciones PLAN y VERIFY, fidelidad UTF-8 y vínculos de evidencia (§5) |

**Sin cambio:** binario por ruta verificada `bin/2e5e00daee91c61d`, `-C`, `-s read-only`, `-m gpt-6-luna`, `-c model_reasoning_effort="high"`,
`--output-schema`, `-o`, `--json`, sin `--ephemeral`, stdin cerrado, el `pwsh` del runtime primero en el `PATH`, 600 s con `taskkill /T /F`,
medición de identidad antes y después, `procs-snapshot.ps1`, `cfg-fp.ps1` y `baseline-keynames-F909F480.json` (byte a byte los de v4). Los
canónicos `delegation.v2` y `controller-verification.v2` incrustados son **idénticos** a los del esquema de la sonda 1 (aceptados por este binario
en modo estricto); solo cambia el esquema de `steps` (`id` 0-23 y `commands` con `pattern`, palabra que el binario ya aceptó: los canónicos la usan
30 veces).

## 2. Paso 0: fidelidad UTF-8 del `pwsh` nativo

Textos de prueba: **T1** = línea 1 de `docs/automation/decisions/FX-U1.md` en HEAD («—» U+2014, que no existe en las páginas OEM habituales, y «ó»
U+00F3); **T2** = `Objective` de K (ñ, ó, é, ó). Los nombres de construcciones de PowerShell que aparecen en el texto son solo los que manda §73.5.

| Sub-paso | Qué pide | Claves |
|---|---|---|
| a) | en un comando, sin tocar la codificación: escribir T1 y T2 y copiarlos **como llegan**; calcular en la shell los puntos de código de sus no ASCII desde el texto decodificado como UTF-8 y emitirlos en ASCII (`U+XXXX`); el modo de lenguaje (`$ExecutionContext.SessionState.LanguageMode`) | `u0a:cmd`, `u0a:t1`, `u0a:t2`, `u0:t1-cps`, `u0:t2-cps`, `u0:language-mode`, `u0a:faithful` |
| b) | en otro comando, por separado: `[Console]::OutputEncoding = [System.Text.Encoding]::UTF8` y `$OutputEncoding = [System.Text.Encoding]::UTF8`; resultado observado, no supuesto, y error exacto | `u0b:cmd`, `u0b:console-set`, `u0b:console-error`, `u0b:psoutput-set`, `u0b:psoutput-error` |
| c) | en el mismo comando de b), releer T1 y T2 y copiarlos como llegan | `u0c:t1`, `u0c:t2`, `u0c:faithful` |
| d) | elegir para todo lo posterior un método de solo lectura fiel, sin cambiar configuración, permisos ni entorno persistente (`DEFAULT`, `SET_ENCODING` repetido en cada comando, `OTHER` o `NONE`) y comprobarlo en un comando aparte | `u0:method`, `u0:method-detail`, `u0d:cmd`, `u0d:t1`, `u0d:t2`, `u0d:faithful` |

**Criterio de `compare` (`Utf8Checks`; todos obligatorios):** claves presentes; comandos citados existentes y a) ≠ b); puntos de código exactos
**y emitidos por la shell** en la salida de a); copias de a) y c) **textuales en la salida cruda** del comando citado (lo que el runtime registró,
con sus U+FFFD si los hubo) y banderas coherentes con ellas; modo de lenguaje en la salida de a); por cada asignación, `yes` sin error, o `no` con
el error presente (sin espacios) en la salida de b); método válido y coherente (DEFAULT exige a) fiel; SET_ENCODING, c) fiel con alguna asignación
aplicada); con DEFAULT o SET_ENCODING, T1 y T2 textuales en la salida de d); y **un método fiel comprobado** (`u0d` exactos). `NONE` deja
`OP_U0_utf8` en falso: sería la evidencia de que la receta actual no da UTF-8 fiel (opción C de §72, A-n material).

**Hechos para el Coordinator** (`Utf8Measurement`): modo de lenguaje; salida por defecto fiel o no (declarado, copia exacta, T1/T2 en la salida
cruda); `[Console]::OutputEncoding` y `$OutputEncoding` fijables o no, con el error; fidelidad tras el intento; método y su comprobación; comandos
cuya salida cruda tiene U+FFFD o U+001A.

**Fidelidad de los pasos posteriores** (`Fidelity`, obligatoria para ALL): las claves marcadas (`0:u0d:t1`, `0:u0d:t2`, `10:md-line`,
`10:md-nonascii-count`, `10:md-nonascii-chars`, `11:yaml:automation_state.next_action`, `12:json:Objective.nonascii-count`), todos los campos de
`delegation_v2` con texto no ASCII (`Objective`, `Invariants`, `Authorities`, `ExpectedEvidence`, `RoleRequirements`, `StopConditions`, …) y
`AuthorityResolution`, exactos; y ningún U+FFFD ni carácter de control (p. ej. el `\u0000f3` de A4-1) en la salida, salvo en las copias «como
llegan» de a)-c) y en los campos `error`.

## 3. Cobertura: operación → fuente AP → paso → criterio

| Operación | Fuente | Paso | Criterio de `compare` |
|---|---|---|---|
| Ejecución con la receta literal | AP 16.4; §73.5 | 1 + eventos | `shell` presente, `cwd` = toplevel; ≥ 1 comando completado; ningún comando **completado** que escriba (Git o archivos) o use la red (incluido `git ls-remote`); F6-OBS-03 detectada y registrada (`NativeRoute`) |
| Fidelidad UTF-8 del transporte | §73.5 | 0 | `Utf8Checks` (§2) |
| Identity (rama, worktree, ascendencia, `origin/<rama>`) | AP 16.9 #5 | 2, 6 + `delegation_v2` | `origin-branch`, `mb`, `anc:*`; `BaseSha`, `ExpectedBranch`, `ExpectedWorktree`; `Checks.Identity` |
| Diff y Trailer | AP 16.9 #7, #11 | 3, 4 | `paths`, `commits`, `trailer:<c>`; `Checks.Trailer` |
| Parte RED | AP 16.8; 16.9 #9, #10 | 5, 17 | `diff-red-head`, `rt`, `redpart:touches`; `Ci.RedPart`, `Tests.RedPart` |
| Hashes y blobs | AP 16.24 (cierre de insumos) | 7, 12 | `hash-object`, `rev-parse`, `sha256`, `blob-at-ar`, `closure:*` |
| Camino de error | §73.5 (Git) | 8 | `bad-rc` = 128 |
| CleanTree | AP 16.9 #8 | 9 | `status`, `status-ignored`, `check-ignore`, `check-ignore-rc`; `Checks.CleanTree` |
| Lectura fiel MD / YAML / JSON | AP 16.24; §73.5 | 10, 11, 12 + `delegation_v2` | copias exactas y conteos; `Fidelity` |
| TRX y artefactos de CI | AP 16.9 #9, #10; 16.22 | 13, 14 | conteos, pruebas, filtro, SHA-256 y su `TestArtifacts`; `Checks.Ci`, `Checks.Tests` |
| Handoff | AP 16.9 #2 | 15 | presencia, JSON, campos, SHA-256 frente a `OutputSha256`, fecha; `Checks.Handoff` |
| Remote en el registro | AP 16.9 #6 | 16 | `rf:*`; `Checks.Remote` (sin `ls-remote`) |
| Derive, Classify, E1-E3, Validate MV-1..MV-7, Resolve | AP 16.13 (forma I62), 16.14 | 18-22 | todas las claves; `map-verdict` = MAP_INVALID con `mv-failed` exactos |
| AuthorityResolution y Authority | AP 16.3, 16.13; 16.9 #3 | VERIFY | tabla exacta (`RevisionSha`, `EffectiveSha`, `ClauseMapBlob`); `Checks.Authority` = fail |
| Contract (A3-A5, G4, G5) | AP 16.5, 16.22 | VERIFY | `Checks.Contract` |
| Scope con pertenencia por ruta | AP 16.9 #7, 16.22 | VERIFY | `Checks.Scope` = fail (el escenario tiene rutas sin entrada) y `Evidence` con todas las rutas del diff |
| Termination, Routing, FreeText, Denials | AP 16.9 #1, #12-#14; 16.10; 16.22 | VERIFY | resultados exactos |
| Clasificación, disposición, primer fallo | AP 16.9 | VERIFY | `EXECUTION_BLOCKED` / `STOP` / `Authority`; coherencia RAE §8 |
| PLAN (`rackcad-delegation/v2`) | AP 16.5 A1-A8, 16.8, 16.22; routing §1-§3 | F | válido contra el canónico; copias exactas de K; `Attempt`, `AttemptsRemaining`, `MaxReworkLoops`, cadena, `ExpectedHandoffPath`, `MainSha` = `origin/main`; `Executor`, `Owner`, `IssuedBy`; clase, perfil y effort coherentes |
| Esquemas canónicos | `agent-execution/schemas` | 23 + F | blobs; los dos objetos válidos y completos |

El escenario S produce 5 comprobaciones en fail (Authority, Scope, Ci, Tests, Trailer) y 9 en pass, con `FailureClass` = Authority y precedencia
STOP: la capacidad de detectar fallos y clasificarlos queda medida sin los negativos.

## 4. Qué se retira y por qué

| Retirado | Justificación |
|---|---|
| Paso 9 de v4 (`git ls-remote`) | VERIFY comprueba Remote «en el registro» con señal sesión → Controller (AP 16.9, fila 6); el `MainSha` de PLAN es el `origin/main` recién obtenido (A6), que obtiene la sesión; el sandbox lo bloqueó en A4-1 y en la sonda 1. Se conservan `rf:lsremote-is-currentsha`, `rf:originmain-is-mainsha` y `Checks.Remote`. |
| `paths-2` e `identical` | repetición de un mismo diff: no es una obligación de PLAN ni de VERIFY |
| Claves duplicadas (§1) | el mismo valor se compara exacto en el objeto canónico que lo lleva |
| nc1 (CurrentSha inexistente → Identity) | el camino de error de Git sigue en el paso 8; Identity se mide con el escenario; un CurrentSha inexistente no añade capacidad REQUIRED |
| nc2 (Scope con una entrada menos) | el escenario ya tiene rutas sin entrada: Scope = fail con pertenencia por ruta es la misma capacidad |
| nc3 (término de gate en la entrega) | FreeText se mide con la regla de 16.10; la detección de un término concreto es otra invocación de VERIFY (agent-execution README §10). Es el único negativo cuyo resultado esperado (fail) no tiene equivalente en el escenario; se juzga que no es una capacidad distinta de aplicar la regla (decide el Coordinator) |
| nc4 (A3 con una entrada extra) | Contract y A3 se miden en VERIFY y la delegación de PLAN tiene que copiar el alcance exacto de K |
| N4 (sin entrega) | Handoff se mide con la entrega presente; la ausencia es una invocación aparte (BLOCKED) |
| N5 (conteos a 0 en `TestArtifacts`) | Tests ya falla en el escenario y los conteos se comparan en los pasos 13-14 |
| N7a / N7b (`RunId` de otra corrida; más `CurrentSha` = `RedSha`) | `handoff:fields-match` e Identity se miden con el escenario |

Los negativos de RAE §10 y de FX-U1-O4 punto 15 siguen siendo obligatorios en la propia FX-02, como invocaciones independientes de VERIFY.

## 5. Comparador (`probe_tools.py compare`) y veredicto

- **Operaciones:** `OP_U0_utf8` (paso 0 + `Utf8Checks`), `OP_A_ejecucion` (1 + eventos), `OP_B_git_y_hashes` (2-9), `OP_C_fidelidad_de_entradas`
  (10-12), `OP_D_trx_y_ci` (13-17), `OP_E_mapa_I62` (18-22 + `AuthorityResolution`), `OP_F_esquemas_canonicos` (23 + PLAN + VERIFY). Cada grupo
  exige además que sus pasos citen al menos un comando ejecutado (`C<n>` con n ≤ comandos de los eventos).
- **Acciones:** `PLAN_delegation_v2` y `VERIFY_controller_verification_v2` (con `AuthorityResolution`).
- **Una clave ausente nunca casa**; un valor que no es cadena tampoco.
- **ALL_OPERATIONS_DEMONSTRATED** solo con: salida válida contra el esquema usado; todas las operaciones y acciones; `Fidelity.AllFaithful`; todos
  los vínculos de evidencia; proceso `exit=0` sin tope; identidad estable (con acompañantes); HEAD y árbol sin cambios; `status --ignored` sin
  cambios durante la sonda; retirada correcta; sin F6-OBS-03; origen estable. **REFERENCE_UNSTABLE** si lo único que falla es la estabilidad del
  origen (instantáneas `ls-remote` de la supervisión). Cualquier otra cosa: **NOT_DEMONSTRATED** (§73.7: ninguna elegibilidad parcial).
- Se conservan `EventsCommandAudit`, `EventsCommands`, `NativeRoute` (F6-OBS-03), `UsageLimitSignal` y `ProcessResidue`; `OutputChars` registra el
  tamaño de la respuesta.

## 6. Puertas de `run_probe.sh` y literales

Literales de decisión (los seis únicos por rellenar en `gates.env`; todos comprobados con `grep -F` en `06b92300` y otra vez en `9f3e3d2a`, que no toca el archivo de decisiones):

| Variable | Literal candidato | Archivo y línea |
|---|---|---|
| `A2P2_PAIR_MARKER` | `BLOQUE-CODEX = AUTORIZADO bajo A2-P2 para el par exacto: BinaryHash d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec, AppVersion 26.1007.2314.0, Fingerprint F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113` | decisiones, 1411 (§69.1) |
| `A2P2_STABILITY_MARKER` | `Par estable (A2-P2, regla 3): huella F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113 sin cambios en las observaciones 1 y 2 (03:47:57Z y 04:46:50Z) y en la verificación previa al bloque` | evidencia, 3594 (§127) |
| `DISP_V5_REASSIGN_MARKER` | `La segunda sonda disponible del bloque A2-P2 se reasigna al Controller en lugar del Architect.` | decisiones, 1489 (§73.4) |
| `DISP_V5_CELL_MARKER` | `Celda: codex-cli / gpt-6-luna / high. Identidad autorizada: BinaryHash d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec, AppVersion 26.1007.2314.0, Fingerprint F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113` | decisiones, 1489 (§73.4) |
| `DISP_V5_DESIGN_MARKER` | `Una invocación conjunta de PLAN y VERIFY, distinta de la anterior.` | decisiones, 1490 (§73.5) |
| `DIR_U03_MARKER` | `` `-C` del Controller de FX-02: `D:\r62-fixture\A2` `` (con las comillas invertidas) | decisiones, 1069 (§55.10) |
| `od2-line.txt` | la línea `OD-2 = A (…)` de §69, línea completa | decisiones, 1424 |
| `bloque-consumo-line.txt` | la línea `BLOQUE-CODEX-CONSUMO = A (…)` de §69, línea completa | decisiones, 1425 |

Comprobaciones, en orden: `gates.env` sin marcadores; `pwsh` del runtime presente; OD-2 y consumo como líneas completas que nombran la identidad;
par con BinaryHash, AppVersion y huella en orden; estabilidad en la evidencia; reasignación que nombra Controller y A2-P2; celda
`codex-cli/gpt-6-luna/high` con la identidad en orden; diseño que nombra PLAN y VERIFY; U-03 que nombra al Controller y fija `CONTROLLER_PROBE_DIR`;
**presupuesto** (exactamente una carpeta `*a2p2-v4-controller*`, que es `PROBE1_RUN` con `comparison.json` y `probe-prompt.sha256`; ninguna
`*a2p2-v4-architect*`; ninguna `*a2p2-v5-controller*`; sin marcador local; total < 2); sin salidas previas ni escenario preparado; **clon**
(directorio admitido, origin = `D:/r62-fixture/fixture-origin.git`, `core.autocrlf=false`, rama `fx/u1`, HEAD = `EXPECTED_TIP` = `ls-remote
origin fx/u1` = `refs/remotes/origin/fx/u1`, `refs/remotes/origin/main` = `ls-remote origin main`, limpio con ignorados, `BAD_SHA` ausente, sin
`%USERPROFILE%\.claude\projects\D--r62-fixture-A2`, SHA y TRX válidos, `IGNORE_PATH` ignorado); **no repetición** (plantilla con el paso 0 y sin
presión de tiempo; texto armado ≠ `f88d3313…` y ≠ el SHA custodiado de la sonda 1); esquema = canónicos de HEAD. Al lanzar, el texto rearmado tiene
que ser el mismo que comprobaron las puertas.

## 7. Preflight de §73.6

| Condición | Quién la comprueba |
|---|---|
| Binario, versión y huella exactos (y acompañantes) | **script**: `measure pre-probe` (STOP exit 3) y `post-probe`; **supervisión**: invalidadores no observables (autenticación, blobs de catálogo y routing, instancia del host) |
| Presupuesto restante 1/2 | **script**: `check_budget` |
| No es una repetición de la sonda 1 | **script**: plantilla y SHA del texto; **supervisión / Coordinator**: diferencias de §1 |
| Texto, esquema, comparador y oráculo custodiados | **supervisión**: commit y congelación del kit (SHA-256 de §11 y del texto armado) antes de lanzar; **script**: esquema = canónicos de HEAD y texto al lanzar = texto de las puertas |
| Cobertura de las capacidades requeridas | **supervisión / Coordinator**: §3 y §4 |
| Autopruebas y negativos del comparador | **supervisión**: `selftest-report.json` y `selftest-gates-report.json` (§9) |
| Aislamiento del clon | **script**: `check_controller_dir` |
| Ninguna sesión A2 activa | **script**: directorio de proyecto de Claude; **supervisión**: ninguna sesión abierta sobre A2 en ese momento |
| No compite con trabajo crítico de I-64 | **supervisión** (Coordinator / Principal) |
| Literales de decisión y líneas del Owner | **script**; **supervisión**: que sean de verdad los registros vigentes |

## 8. Procedimiento (solo con las puertas)

1. Verificar en el worktree que §73 sigue vigente y que no hay otra disposición posterior; I-64 sin trabajo crítico en curso.
2. `cp gates.env.template gates.env`; sustituir los seis literales de §6; congelar (SHA-256 de los archivos, de `gates.env` y del texto armado).
3. `bash run_probe.sh probe CONTROLLER_V5` (una vez) → `bash run_probe.sh compare CONTROLLER_V5`.
4. La supervisión lee del registro de sesión solo modelo, effort y señales de límite, sanea y custodia en `EVIDENCE_ROOT/<RunId>-a2p2-v5-controller/`.
5. Sin reintento. Una diferencia de identidad es STOP (exit 3) y anula la medición (A2-P2, regla 6). Con esta sonda el bloque queda agotado.

## 9. Autoensayo (hecho, sin modelo)

Clon propio `D:\r62-fixture\tmp-v5-selftest` de `D:/r62-fixture/fixture-origin.git` (`fx/u1`, `95bdc29d`, `--no-local`, `core.autocrlf=false`),
borrado al final. Detalle en `selftest-report.json` y `selftest-gates-report.json`.

| Prueba | Resultado |
|---|---|
| Comparador (`probe_tools.py selftest`): 8 positivos → ALL_OPERATIONS_DEMONSTRATED (P0 `SET_ENCODING`; **P1 salida por defecto fiel con `OutputEncoding` rechazado**; P2 otro método fiel; P3 `merge-base`, `stash list`, `tag -l` y `branch --show-current` no escriben; P4 trampas de búsqueda de texto; P5 `fetch` fallido; P6 `ls-remote` fallido; P7 `ls-remote` rechazado dentro de un lote que termina con 0); 35 negativos → NOT_DEMONSTRATED (**UTF-8 deformado en un paso posterior**, en un campo canónico y escape mal formado `\x00fa`; ningún método fiel; copia corregida con `yes` sin respaldo; error parafraseado; puntos de código erróneos o no emitidos por la shell; método incoherente; a) y b) en el mismo comando; comando citado inexistente; DEFAULT sin salida fiel; clave del paso 0 ausente; **claves de AP 16.13 ausentes** (Validate `mv3`, Derive `EFF`); **hash erróneo**; **paso omitido**; `C<n>` inexistente o vacío; `AuthorityResolution`; check; canónico inválido; `MainSha`; routing; F6-OBS-03; escritura, redirección, `fetch`, `ls-remote` y red completados; `shell` ausente; tope; área ignorada; identidad inestable; sin salida); 1 → REFERENCE_UNSTABLE (origen cambiado) | 44/44 |
| Esquemas | salidas positivas válidas contra el esquema de salida y contra los dos canónicos (validador local; `jsonschema` no instalado); auditoría estricta: raíz objeto sin `anyOf`, 0 problemas estructurales, 5 niveles, 235 propiedades, 232 valores de enum, `NotEmpiricallyAccepted` vacío; canónicos incrustados idénticos a los del esquema de la sonda 1 | cumple |
| `stage`/`unstage` | `status --ignored` vacío → `!! artifacts/` → vacío | restaurado |
| Arnés de puertas (`selftest-gates.sh`, `check_gates` real con registro sintético): 2 que pasan (G01; G26 con STRICT_PROJECTION) y 27 negativas (sin OD-2, sin consumo, par anterior, marcador `<…>`, sin estabilidad, sin reasignación, celda del Architect, identidad anterior, U-03 de otro directorio, sin disposición de diseño, **bloque agotado por una sonda del Architect**, sin la sonda 1 custodiada, dos sondas 1, v5 ya custodiada, ya lanzada, salidas previas, **repetición por SHA custodiado**, **plantilla de la sonda 1**, **«Tiempo limitado» añadido**, clon con ignorados, clon fuera de `EXPECTED_TIP`, `origin/main` desfasado, `BAD_SHA` existente, directorio no admitido, modo retirado (exit 64), sin `gates.env`, proyecto de Claude en el `-C`) | 29/29; clon limpio y `origin/main` restaurado |
| Auditor sobre eventos custodiados | sonda 1 (`pwsh-runtime`, 4 comandos, 3 correctos, sin F6-OBS-03, intento de `ls-remote` no completado, salidas 2 y 3 con U+FFFD y 2 con U+001A); A4-1 (`cmd`, 11/7, sin F6-OBS-03); bloque A2-P2 anterior `R20261009T041427Z` (`pwsh-windowsapps`, F6-OBS-03 3 veces) | reproduce lo custodiado |
| Texto armado en seco (clon temporal) | 21 846 caracteres (22 030 bytes), `52aa85d40490f806999e57cf4b14e6857cf3aa33311fd48a8e800172cd87fbbd` (≠ `f88d3313…`) |
| `bash -n` (`run_probe.sh`, `selftest-gates.sh`), `py_compile -W error` (`probe_tools.py`) | sin errores |

## 10. Riesgos y puntos abiertos

- **Tamaño de la respuesta.** La salida sintética completa mide 25 332-25 541 caracteres: del orden de la respuesta de la sonda 1 (24 651), pero
  aquella estaba incompleta (sin `AuthorityResolution` y con pasos vacíos). El suelo lo fijan los dos objetos canónicos (≈ 11 k) y la `Evidence` de
  Scope que exige AP 16.22; los pasos ya no repiten nada de lo que llevan. Si la celda vuelve a parar pronto, la causa no sería la presión de tiempo
  del texto.
- **Rigor de los vínculos `C<n>`.** Un recuento equivocado de comandos (un `C<n>` mayor que los ejecutados) o copias de a)/c) que no estén textuales
  en la salida citada dejan NOT_DEMONSTRATED. Es intencionado (separa transporte y modelo), pero es más estricto que v4.
- **Puntos de código bajo ConstrainedLanguage.** `ToCharArray()` y `[int]` son de tipos núcleo; si la celda los calcula sobre la salida de `git`
  decodificada por la consola (mojibake), saldrán mal y `OP_U0` falla, como corresponde.
- **`git ls-remote` completado = red** (`OP_A` falla) aunque sea de solo lectura del origen local: el texto lo prohíbe. Un intento fallido solo se
  registra. Decide el Coordinator si lo mantiene.
- **`MainSha` de PLAN** = `refs/remotes/origin/main` con la igualdad a `ls-remote origin main` comprobada por la puerta: lectura de A6 («`origin/main`
  recién obtenido») que confirma el Coordinator.
- **Pistas de PowerShell** en el paso 0 (`[Console]::OutputEncoding`, `$OutputEncoding`, `$ExecutionContext.SessionState.LanguageMode`): las manda
  §73.5; v4 no daba ninguna pista de shell.
- **nc3** (§4): el único negativo sin equivalente de resultado en el escenario.
- **Tope de 600 s:** pedir todos los pasos puede acercarse al tope (la sonda 1 usó ~105 s con 4 comandos); un tope vencido es NOT_DEMONSTRATED.
- **A2 compartido** con el titular A2 de FX-02: las puertas exigen clon fresco y sin proyecto de Claude; el escenario se retira con comprobación.
- **Lo que el script no puede comprobar:** que los literales sean el registro vigente, los invalidadores no observables, el modelo y effort
  efectivos (registro de sesión) y la ausencia de trabajo crítico de I-64.

## 11. Archivos (SHA-256)

| Archivo | Uso | Bytes | SHA-256 |
|---|---|---|---|
| `run_probe.sh` | `measure`, `probe CONTROLLER_V5` y `compare CONTROLLER_V5`, con las puertas de §6 | 21224 | `d769aabace2abe0533f0a448f1e5dff0786d722c9b1549cec4e64098e3f2bff9` |
| `probe_tools.py` | esquemas, texto, escenario S, referencia, comparación (paso 0, fidelidad, vínculos, PLAN/VERIFY), auditor de eventos y autoensayo | 147774 | `33394bda5e2c446513bbbb11da89165c9d48176d1f5b4f36709b0bccc7cffc6a` |
| `probe-prompt.controller-v5.template.txt` | texto de la sonda (pasos 0-23) | 20041 | `2af1bb21fc2ff5ae3964fb8209d7f48e7c14d351fb12bf50aa0fb07632fdc3dd` |
| `probe-schema.controller-v5.json` | esquema de salida (VERBATIM) | 37000 | `4375dbe5a7df492b2c357ba97f2b3227fa89e58a61477ad262354319a0b67dea` |
| `probe-schema.controller-v5.strict-projection.json` | ídem, STRICT_PROJECTION (sin `minLength`) | 36550 | `174e8b942d0ada81c9136885ac9c3d558e92c9d8b3fc7baeb864dcab0253bccb` |
| `gates.env.template` | puertas y parámetros; seis literales de decisión por rellenar, con candidatos | 6250 | `eb65107b707284fe78f610534b6a3e047dfa150adc4ec974aa4378be0105ab6d` |
| `od2-line.txt` | línea OD-2 del Owner de §69 (= v4) | 161 | `b76470163f86592dfda474d44c5e2051d1e05ead667362fb8428c32ef7d4ac96` |
| `bloque-consumo-line.txt` | línea BLOQUE-CODEX-CONSUMO del Owner de §69 (= v4) | 322 | `953088c953075af184a017fcf48a98c5540ee0cc4e5123edf719fc969f57bf5b` |
| `cfg-fp.ps1` | huella de config.toml (= v4; no ejecutado) | 1479 | `b1d833c0abc06fec6e27367b6df2715e7cdca321f0d6c6fbadb9f634e6a31bab` |
| `baseline-keynames-F909F480.json` | los 107 nombres saneados (= v4) | 4291 | `076637c550ff0b3109447ef00263ff087749f0aaa71f299d524e43b8cf82b70b` |
| `procs-snapshot.ps1` | instantánea informativa de procesos codex*.exe (= v4) | 1236 | `f879db305dd296d30241cca75cf61ac0a5828077cd2be6277e4128f934afedaf` |
| `selftest-gates.sh` | arnés de las puertas (solo autoensayo) | 10795 | `01c420e393c478fe9bedbef7c70004f8aa377ad2e42a15b018b0b00b155918e6` |
| `selftest-report.json` | informe del autoensayo del comparador | 32276 | `8b893cb34dd92e8359a11f19bc0e71eb13f6292b28ee2a4449fcfaa9ab32c5f5` |
| `selftest-gates-report.json` | informe del arnés de puertas | 6285 | `eea0c429eec05c3c94a5868bead1adaf53d1117f989873acffcc8f2787943833` |

`README.md` no se lista a sí mismo; su SHA-256 se registra al congelar.

## 12. Revisión de la supervisión antes de congelar (sustituye los valores de §1, §9 y §11 que cambian)

- **Cambio único: qué numera `C<n>`.** El comparador indexa la salida cruda por invocación de la shell (cada `item.completed` de tipo
  `command_execution`, también las fallidas). El texto decía a la vez «comando» para cada orden y «agrupar varios comandos en una invocación», de
  modo que una celda que agrupara órdenes podía numerar órdenes y no invocaciones: el paso 0 habría fallado por la redacción, no por la capacidad.
  Ahora el texto define la invocación (cada ejecución de la shell, un proceso nuevo), permite agrupar órdenes dentro de ella y define
  `C<n>` como el orden de la invocación desde 1, contadas las que fallaron; lo mismo en a)-d) del paso 0 y en la descripción de `commands` del
  esquema. Ningún criterio, clave, paso ni puerta cambia. Diff completo en `review-diff.txt`.
- **Esquemas regenerados** con `build-schema` sobre un clon temporal de `fx/u1` (`95bdc29d`, borrado): solo cambia esa descripción;
  `check-schema` OK (canónicos de HEAD `dfdbe461…` y `e1faa9ec…`) y `strict-audit` dentro de los límites documentados.
- **Autoensayos repetidos tras el cambio:** comparador PASS 44/44 y puertas PASS 29/29 (clon temporal limpio y borrado).
- **Texto armado en seco** (clon temporal): `6437e184bcd342eea8a87ec635455a7c7ae7b623a55156ad7b3a34c8c2b8d400`, 21 967 caracteres; distinto de `f88d3313…` (sonda 1).
  El texto que cuenta es el que arman las puertas sobre `D:\r62-fixture\A2` al lanzar (`out/controller-v5/gate-prompt.sha256`).
- **Puntos abiertos de §10, disposición de la supervisión para esta sonda** (supuestos registrados, no decisiones nuevas del Coordinator):
  - tamaño de la respuesta: se acepta; su suelo lo fijan requisitos obligatorios (§73.5 prohíbe omitirlos);
  - nc3: se retira con los demás negativos agregados (§73.5); los negativos de RAE §10 siguen obligatorios en la propia FX-02;
  - `MainSha` = `refs/remotes/origin/main` con la igualdad a `ls-remote origin main` comprobada por la puerta: lectura de A6 registrada como
    supuesto de la medición;
  - `ls-remote` completado = red: se mantiene (el texto lo prohíbe y VERIFY no lo necesita).
- **`gates.env`:** los seis literales de §6, cada uno presente una sola vez en su archivo (decisiones o evidencia) en `9f3e3d2a`.
