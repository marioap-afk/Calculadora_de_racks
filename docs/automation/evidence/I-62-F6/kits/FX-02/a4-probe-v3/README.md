# Sonda medida de A4-1 — kit a41-v3 (NO EJECUTADO; listo para revisión y congelación)

> **No se ha ejecutado y no está congelado.** Lo prepara la supervisión (plano a). Ningún modelo ni binario de Codex se invocó para prepararlo y no se
> leyó nada bajo `%USERPROFILE%\.codex` salvo el worktree de I-62 indicado por la orden. Sin las puertas de §3, `run_probe.sh probe` se niega a correr.

Base: el borrador `kits/FX-02/drafts/a4-probe-draft/` (corrección 2 de A-4). Autoridad de esta versión:
- A-4 AGREED (decisiones §64, punto 1; objeto `docs/initiatives/I-62-A-4.md`, blob `5e2ba68e`), regla A4-1 en §3.2 (L185-L211);
- disposición de aplicación de §64, punto 2 (celda `codex-cli / gpt-6-luna / high`, shell `cmd.exe`, directorio `D:\r62-fixture\A2`): «Se miden todas
  las operaciones que exige A4-1 para PLAN y VERIFY, no solo las ocho de la sonda anterior»;
- §65, punto 5 (D-1, trío nuevo `3553cd6e…/26.1002.7124.0/73890CA3…`), punto 6 (D-4: una sola medición read-only con Git y hashes, entradas
  Markdown/YAML/JSON con fidelidad, TRX y artefactos de CI, resolución I62 del mapa, esquemas canónicos y controles negativos; sin segundo lanzamiento
  ni ampliación de permisos) y punto 7 (orden O4 → clon A2 → sonda); OD-2f = A y `A4-SONDA-CONSUMO = A` del trío nuevo (bloque del Owner, L1330).

Hechos nuevos tras el borrador: la orden FX-U1-O4 está publicada en `fx/u1` (`95bdc29d`, solo toca `docs/automation/decisions/FX-U1.md`) y el clon
`D:\r62-fixture\A2` existe en `95bdc29d` (S03, evidencia §118). Este kit no lo ha tocado: el autoensayo usó un clon propio en el mismo commit.

## 1. Qué mide

Una sola invocación read-only de la receta 16.4 (sin cambios salvo la shell declarada, A4-1 regla 1): `-C D:\r62-fixture\A2`, `-s read-only`,
`-m gpt-6-luna`, `-c model_reasoning_effort="high"`, `--output-schema`, `-o`, `--json`, stdin cerrado, `pwsh` del runtime primero en el `PATH`, tope de
600 s con `taskkill /T /F`, identidad antes y después. El texto (32 pasos, 176 claves) pide, con la shell declarada:

- **A. Shell**: `ver`, `%ComSpec%`, directorio.
- **B. Git y hashes**: diff de dos revisiones (con `..` y con espacio), diff con pathspec, `merge-base`, `--is-ancestor` (incluido el de `RedSha`),
  `git rev-parse <rev>:<ruta>` en otras revisiones (con un caso que falla), `hash-object`/`rev-parse HEAD:`/`certutil` SHA-256, camino de error con un
  SHA inexistente y su código, `ls-remote`, `status` (con y sin ignorados), `check-ignore`, historia y trailers.
- **C. Fidelidad**: copia exacta de una línea Markdown con 6 caracteres no ASCII (incluido `→`, que no existe en las páginas OEM) y su recuento por
  carácter; valores YAML (incluido un texto no ASCII y la ausencia de `max_attempts`); JSON con campos anidados, arrays completos y cadenas no ASCII;
  obligaciones READ de `AGENTS.md` y blobs del cierre de insumos.
- **D. TRX y CI**: el TRX real custodiado de la corrida 37949808974 y un TRX RED sintético derivado: conteos, nombres, fallidas, filtro
  `FullyQualifiedName~Subtract`, SHA-256 del archivo no versionado frente a `RemoteFacts.TestArtifacts`; entrega (presencia, JSON, campos, SHA-256,
  fecha posterior al inicio); `RemoteFacts`; parte RED de `Tests` (RT ∪ `ChainRedFiles`).
- **E. Mapa de cláusulas, forma I62 de AP 16.13**: Derive (EFF, EFF^1, EFF^2), tabla PRE, Classify, E1-E3 (punto de entrada y subsección fuera de
  bloques de código), Validate MV-1..MV-7 con sus hechos, Resolve pasos 3-5 (MB, diffs por cita EXTERNAL, `AR..BaseSha`, ancestro) y
  `AuthorityResolution`.
- **F. Esquemas canónicos**: la salida PLAN completa (`rackcad-delegation/v2`) y la salida VERIFY completa (`rackcad-controller-verification/v2`) del
  escenario S, incrustadas como subobjetos obligatorios del esquema de salida (§6).
- **G. Controles negativos sobre copias**: nc1-nc4 (RAE §10) y N4, N5, N7a, N7b (orden O4, punto 15, mutaciones exactas); N6 = NOT_APPLICABLE por la
  orden.

**Escenario S** (sintético, rotulado como tal en cada archivo): `stage` escribe en el área transitoria ignorada del clon (`artifacts/orchestration/`,
RAE §2; U-08) una delegación `delegation/v2` válida construida desde el contrato de T1, una entrega `worker-handoff/v1` válida
(`BaseSha` = BASE, `RedSha` = RED, `CurrentSha` = HEAD del clon), los hechos del relevo del trabajo con `RemoteFacts` (la corrida real de `c785def9`
más una corrida RED sintética), la copia exacta del TRX custodiado, el TRX RED sintético y las ocho copias mutadas. `unstage` los retira tras la
sonda, comprueba que no cambiaron y que `git status --porcelain --ignored` vuelve al de antes. Se registra `status --ignored` antes del stage,
después del stage, después de la sonda y después de la retirada.

Resultado esperado del escenario (referencia del autoensayo sobre `95bdc29d`, el mismo commit que A2): Termination, Handoff, Contract, Identity,
Remote, CleanTree, Routing, FreeText y Denials en pass; **Authority** fail (MAP_INVALID, §8 F-MAP-1), Scope fail (rutas fuera de
`AllowedWriteScope`), Ci fail (la corrida es de `c785def9`, no de HEAD), Tests fail (0 seleccionadas), Trailer fail (commits sin el trailer
declarado); `RedPart` pass en Ci y Tests; `EXECUTION_BLOCKED/STOP`, `FailureClass` = Authority. La sonda no tiene que «aprobar» nada: tiene que
reproducir esos hechos. Ningún valor esperado existe en disco mientras corre (la referencia se calcula después, en `compare`).

## 2. Cobertura: operación → fuente → paso → criterio de `compare`

AP = `docs/AUTOMATION_PLAN.md` (blob `f525cb1e`, igual en RackCad y en el fixture); RAE = `docs/automation/agent-execution/README.md` (`592dcfd4`);
CC = `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md`; O4 = orden FX-U1-O4 en `fx/u1:docs/automation/decisions/FX-U1.md`
(`95bdc29d`); DEC = `docs/automation/decisions/I-62.md`. «Clave ausente» = nunca casa, ni con una referencia vacía.

| Operación obligatoria | Fuente (ruta:línea) | Paso | Criterio de `compare` |
|---|---|---|---|
| Shell declarada `cmd.exe`, sin PowerShell ni intérpretes | A-4 L192-L194; O4 L135-L141 | 1 + eventos | `ComSpec` contiene `cmd.exe`; `cwd` = toplevel; ningún comando prohibido **completado** (los intentos fallidos se registran) y ningún `git` que escriba |
| `git diff --name-only <base>..<head>` reproducible | A-4 L195-L199; AP L636 | 3 | dos listas = referencia e `identical` = yes |
| Diff de dos revisiones y con pathspec (RT) | AP L613; AP L639 | 5 | listas = referencia |
| Identity: rama, worktree, ancestros, `RedSha` entre `BaseSha` y `CurrentSha`, `HEAD` = `origin/<rama>` | AP L634 | 2, 6 | SHA y `yes`/`no` = referencia; rutas normalizadas |
| `git rev-parse <rev>:<ruta>` en otras revisiones, con fallo | AP L731; RAE L240 | 7 | blob o `rc=128` = referencia |
| Blobs y hashes (hash-object, rev-parse, certutil) | A-4 L198; AP L712-L714 | 7, 14-16 | hex = referencia |
| Camino de error con SHA inexistente y su código | §65 punto 6 (DEC L1318) | 8 | código = 128 |
| Remote: `ls-remote` y `origin/main` | AP L635; RAE L158-L168 | 9, 17 | SHA = referencia; si `ls-remote` cambia entre antes, después y la comparación → `REFERENCE_UNSTABLE` |
| CleanTree: estado, ignorados, `check-ignore` | AP L637; O4 L191-L193 | 10 | bloques normalizados y código = referencia |
| Trailer (historia) | AP L640; AP L1200 | 4 | commits y trailers = referencia |
| Entrada Markdown con no ASCII (copia exacta y recuento) | AP L1287-L1293; RAE L570-L583 (F3) | 11 | copia **byte a byte** (salvo el fin de línea); recuento y `carácter=n` exactos |
| Obligaciones READ y acción incompatible de `AGENTS.md` | CC L69-L74; O4 L188-L190 | 11 | lista y orden = referencia |
| Valores YAML, contadores (`attempts`, `max_attempts`), cadenas | CC L101-L108; AP L605; AP L289 | 12 | valores = referencia; no ASCII textual |
| JSON: campos anidados, arrays completos, no ASCII | CC L101-L108 | 13 + F | igualdad JSON (no textual) de cada valor; `Objective` textual en F |
| Blobs del cierre de insumos | AP L1260-L1276; RAE L553-L565 | 13 | hex = referencia |
| TRX: conteos, nombres, fallidas, filtro, `MinSelected` | AP L639; O4 L194-L197 | 14, 15 | enteros y listas = referencia |
| SHA-256 del TRX no versionado = `TestArtifacts[].Sha256` | O4 L194-L197; U-07 (DEC L1254) | 14, 15 | hex y `yes` = referencia |
| Entrega: presencia, JSON, campos, SHA-256, fecha posterior al inicio | AP L631; RAE L34-L42 | 16 | valores = referencia |
| `RemoteFacts` (corrida actual y RED, jobs, `LsRemoteSha`, `OriginMainSha`) | RAE L167; AP L638 | 17 | valores = referencia |
| Parte RED de `Tests` (`git diff RedSha CurrentSha` frente a RT ∪ `ChainRedFiles`) | AP L613-L618; AP L639 | 18 | `yes`/`no` = referencia |
| Derive EFF (trailer único, merge first-parent, EFF^1/EFF^2), tabla PRE | AP L733; AP L745-L749; AP L945-L948 | 19 | conjuntos de SHA, enteros = referencia |
| Classify (estado en BASE, decisión G0, marcadores) | AP L744-L763 | 20 | `blob`, `yes`, `I62` |
| Evaluate E1-E3 (WORKFLOW y 16.13 en `MainSha_eval`, fuera de bloques de código) | AP L782-L794; AP L872-L873 | 21 | blobs, recuentos y líneas = referencia |
| Validate MV-1..MV-7 (esquema, `Surfaces`, completitud, blobs, unicidad, derivación, punteros) | AP L898-L918 | 22 | pass/fail por regla, hechos de cada regla y `MAP_INVALID` + ids = referencia |
| Resolve 3-5 forma I62 (UNIT_* en AR, EXTERNAL en `MainSha_eval`; MB; diffs; ancestro) y `AuthorityResolution` | AP L818-L831; AP L497-L502 | 23 + F | listas y `yes` = referencia; las 6 filas de `AuthorityResolution` iguales campo a campo |
| Salida PLAN `delegation/v2` canónica | CC L99-L108; AP L1236 | F (+24) | válida contra el esquema canónico (validador local); copias exactas de K; contadores, cadena, git, `ExpectedHandoffPath`, `Owner`, `Executor`; `TaskClass` de routing §1 con el perfil y el effort que dan §1-§2 |
| Salida VERIFY `controller-verification/v2` canónica, coherencia de RAE §8 | CC L117-L131; AP L620-L643; RAE L170-L188 | F (+24) | válida contra el esquema canónico; `Result` de las 14 y `RedPart` = referencia; `Classification`/`Disposition`/`FailureClass` = referencia; coherencia de RAE §8; Evidence de Scope con cada ruta del diff |
| Scope con pertenencia por ruta (y `/**` = prefijo) | AP L552-L554; AP L1204-L1207; U-19 (DEC L1234); O4 L201-L202 | F | `Result` = referencia; Evidence con cada ruta |
| FreeText (términos de 16.10) | AP L667-L672 | F, 27 | `Result` = referencia; término hallado |
| nc1-nc4 sobre copias | RAE L217-L235 | 25-28 | campo mutado, código, ruta excluida, término, entrada extra y resultado de la comprobación afectada = referencia |
| N4, N5, N7a, N7b sobre copias (mutaciones exactas) | O4 L216-L228 | 29-32 | presencia, campos, `Selected` y resultados = referencia |

**Fuera de la sonda (y por qué):**
- **Routing** (modelo y effort efectivos) y **Termination**/**Denials** reales: son hechos de la sesión (AP L641, L630, L643), no operaciones del
  Controller. En la sonda se miden sobre los hechos sintéticos del relevo. El modelo y el effort efectivos de la propia sonda los lee la supervisión del
  registro de sesión (procedimiento, paso 5), no `compare`.
- **Validación con un validador JSON Schema ejecutable dentro de la sonda**: la shell declarada no tiene intérpretes. Con `--output-schema`, la API
  impone el esquema canónico a la propia salida (modo estricto) y MV-1 se resuelve leyendo el esquema del mapa; `compare` revalida con el validador
  local.
- **Oráculo relativo de nc1-nc3** («las anteriores con el mismo `Result` que en la real»): en el escenario, Authority ya falla en la verificación base,
  así que no es evaluable en la sonda. La sonda mide las operaciones de cada control (copia, campo, re-evaluación); el oráculo lo aplican los controles
  reales de FX-02.
- **nc2, adaptación**: RAE §10 excluye «el primero en orden lexicográfico» del diff de una verificación VERIFIED. Como en el escenario hay rutas sin
  cubrir, se excluye la primera ruta **cubierta** y, por ser de prefijo, la entrada se sustituye por la enumeración de las demás (la regla del propio
  §10). Lo dispone el Coordinator (§8).
- **Fetch** de `origin/main` (A6: «recién obtenido»): prohibido en una sonda read-only; se usa `ls-remote` (lectura) y `refs/remotes/origin/main`.
- **N8-N10**: sin invocación (O4 punto 15); N9 y N10 sin mutación aprobada.

## 3. Puertas (todas, en orden; `run_probe.sh probe` se niega sin ellas)

1. **A-4 AGREED**: `A4_AGREED_MARKER` literal en `DECISIONS_FILE` (candidato en la plantilla, §64 punto 1).
2. **Disposición de aplicación**: `DISPOSITION_MARKER` (celda y shell, §64.2), `DISPOSITION_DIR_MARKER` (contiene `PROBE_DIR`, §64.2) y
   `DISPOSITION_TRIO_MARKER` (nombra `BinaryHash`, `AppVersion` y la huella aceptada, §65.5), todos literales; `CELL`, `SHELL_NAME` y `TRIO` = las
   constantes del script.
3. **`A4-SONDA-CONSUMO = A` del trío nuevo**: `consumo-line.txt` (una línea, sin fin de línea, 501 bytes) aparece como línea completa en
   `DECISIONS_FILE` y nombra `73890CA3…`. Es byte a byte la segunda línea del bloque del Owner de §65 (L1330). La de §64 (`6518EFAB…`) no se usa.
4. **P-07, uno en total**: `out/PROBE_LAUNCHED` con `noclobber`, nunca se borra; ninguna salida `out/probe-*` ni `out/stage-manifest.json`; ninguna
   carpeta `*a4-1-probe*` en `EVIDENCE_ROOT`.
5. **Sin reintento**: no hay bucle; un fallo, un tope vencido o un lanzamiento incierto cuentan como el único.
6. **Identidad aceptada** antes y después (OD-2f): huella `73890CA3…`, 107 nombres saneados iguales a `baseline-keynames-73890CA3.json` (contenido
   idéntico al de la línea base anterior), binario `3553cd6e…` único y app `26.1002.7124.0`. Diferencia → STOP exit 3 (P-01; A4-1 regla 5).
7. **Directorio**: `PROBE_DIR` = `D:\r62-fixture\A2`, clon con `origin` = el origen del fixture, `core.autocrlf=false`, en la rama, `HEAD` =
   `ls-remote origin fx/u1`, **sin nada ignorado** (clon fresco), con `refs/remotes/origin/{fx/u1,main}`, BASE/HEADX/RED/corrida en la historia, RED
   entre BASE y HEAD, BAD_SHA ausente, `IGNORE_PATH` ignorado; sin directorio de proyecto de Claude para A2 (§64.2: «no haya una sesión A2 activa»);
   se niega con `A`, `B`, `B2`, `B3`, `R`, `A6`, `arch`, `arch02`, `evidence-out*`, `supervisor*` y `*.git`.
8. **TRX custodiado** con el SHA-256 `4ccdf2ce…`; **esquema de salida** que incrusta los canónicos de HEAD del clon (`check-schema`).
9. **Congelación** tras la revisión: SHA-256 de los archivos del kit (§11), de `gates.env` y del texto armado (`out/probe-prompt.txt`).

El script **no** puede comprobar (lo hace la supervisión): que los literales de `gates.env` sean de verdad el registro; los invalidadores que no
observa (autenticación, modelo, effort, blobs de catálogo y routing, instancia del host); que la línea `Shell` del bloque de transporte de O4 sea
idéntica a `shell-declaration.txt`.

## 4. Procedimiento (solo con las puertas)

1. `cp gates.env.template gates.env` y sustituir los cuatro `<…>` por sus literales (candidatos comprobados con `grep -F` en `DECISIONS_FILE`).
2. Congelar (puerta 9) y custodiar.
3. `bash run_probe.sh probe`, una vez: arma el texto; comprueba el esquema; mide la identidad; `stage`; crea el marcador; registra `HEAD`, estado,
   `status --ignored` y `ls-remote`; lanza la receta con el tope de 600 s; registra lo mismo después; `unstage`; vuelve a medir la identidad.
4. `bash run_probe.sh compare`: `reference` (git + Python, después de la sonda) y `compare` → `out/comparison.json` con veredicto por operación
   (`OP_A`..`OP_G`), claves fallidas, comprobaciones canónicas, proceso, identidad, árbol, ignorados, retirada y estabilidad de `ls-remote`.
5. La supervisión lee del registro de sesión de **esta** invocación (`out/probe-new-sessions.txt`) solo el modelo y el effort efectivos, sanea las
   salidas (rutas con el usuario → `%USERPROFILE%`/`%LOCALAPPDATA%`) y custodia en `EVIDENCE_ROOT/<RunId>-a4-1-probe/` (activa la puerta 4). Repite las
   comprobaciones #4 y #5 del informe del clon A2 (evidencia §118).
6. Resultado al Coordinator: `ALL_OPERATIONS_DEMONSTRATED` (la elegibilidad la declara él; la línea del Owner «no acepta su resultado»),
   `NOT_DEMONSTRATED` (no elegible, sin repetición: A4-1 regla 4) o `REFERENCE_UNSTABLE` (el origen cambió; lo dispone él, sin reintento automático).

## 5. Herramientas

`probe_tools.py` (solo biblioteca estándar; `jsonschema` **no** está instalado en este host y no hace falta: lleva un validador propio del subconjunto
2020-12 que usan los esquemas; si estuviera, `selftest` lo usaría como contraste): `build-schema`, `check-schema`, `strict-audit`, `validate`,
`build-prompt`, `stage`, `unstage`, `reference`, `compare`, `synth`, `selftest`. `compare` exige todas las claves (una ausente nunca casa) y propone:
`ALL_OPERATIONS_DEMONSTRATED` solo con las siete operaciones demostradas, salida válida contra el esquema usado, proceso `exit=0` sin tope vencido,
identidad estable, `HEAD` y árbol versionado sin cambios y limpios, `status --ignored` igual antes y después de la sonda, retirada correcta y
`ls-remote` estable; `REFERENCE_UNSTABLE` si solo fallan las claves de Remote y `ls-remote` cambió; si no, `NOT_DEMONSTRATED`. Política propuesta
(decide el Coordinator): un comando prohibido (PowerShell, intérprete, herramienta de Unix) **completado con éxito** hace fallar OP_A; uno fallido solo
se registra.

## 6. Esquemas canónicos y modo estricto (hallazgos y riesgo)

- `codex exec --output-schema` envía el esquema **sin modificar** (`codex-rs/exec/src/lib.rs`, `load_output_schema`) como
  `text.format = {type: json_schema, name: "codex_output_schema", strict: output_schema_strict}`, con `output_schema_strict = true` por defecto
  (`codex-rs/codex-api/src/common.rs`, `create_text_param_for_request`; `core/src/client_common.rs`). Consultado en `openai/codex` (rama principal) el
  2026-10-09, no en la etiqueta exacta de `0.162.0-alpha.2`.
- Guía de Structured Outputs (consultada el 2026-10-09): exige `additionalProperties: false` en todo objeto, todas las propiedades en `required` y raíz
  objeto sin `anyOf`; admite para cadenas `pattern` y `format`, para números `minimum`/`maximum`/`exclusive*`/`multipleOf` y para arrays
  `minItems`/`maxItems`; no admite `allOf`, `not`, `if/then/else`, `dependent*`; `minLength` y `maxLength` **no figuran entre las admitidas** y solo
  aparecen en la lista de no admitidas «para modelos ajustados». Límites: 5000 propiedades, 10 niveles, 1000 valores de enum, 120000 caracteres.
- Auditoría (`strict-audit`): `delegation.v2` (`dfdbe461`) y `controller-verification.v2` (`e1faa9ec`) cumplen todo lo estructural (cero objetos sin
  `additionalProperties: false`, cero propiedades fuera de `required`, sin composición). Palabras usadas: `type`, `enum`, `pattern`, `items`,
  `description`, `title`, **`minLength` (17)** y **`minimum` (6)**. El esquema de la sonda: 5 niveles de objeto, 235 propiedades, 232 valores de enum.
- Evidencia previa: ninguna corrida custodiada pasó los esquemas **v2** a `--output-schema`. El piloto de I-61 pasó los **v1** (`delegation.schema.json`,
  `controller-verification.schema.json`: `pattern`, `enum`, uniones con `null`) y Codex los aceptó (relay-record de `g2-pr1`, «`--output-schema`
  delegation.schema.json aceptado»). Los esquemas del Architect v9-v14 no usaban `minLength` ni `minimum`.
- **Riesgo**: `minimum` está documentado; `minLength` no está medido ni documentado como admitido. Si la API lo rechaza, `codex exec` termina al
  instante con un 400 y la sonda queda `NOT_DEMONSTRATED` entera (sin reintento). Ese rechazo también impediría la receta real de PLAN y VERIFY, que
  pasa esos mismos archivos a `--output-schema`.
- **Decisión del kit: `CANONICAL_SCHEMA_MODE = VERBATIM`**, es decir, los dos esquemas canónicos incrustados tal cual, sin la palabra `$schema`, como
  propiedades obligatorias `delegation_v2` y `controller_verification_v2`. Motivo: es la única forma de demostrar en una sola invocación el elemento
  «esquema» de la receta. Un rechazo es en sí el hecho decisivo: sin él la celda no puede producir las salidas canónicas. Alternativa preparada y
  ensayada: `STRICT_PROJECTION` (`probe-schema.strict-projection.json`, sin los 17 `minLength`), que protege el resto de operaciones pero deja **sin
  medir** la aceptación de los canónicos. En los dos modos `compare` valida contra los canónicos textuales. Lo decide el Coordinator (Q-F1).

## 7. Shell declarada e intérpretes

`shell-declaration.txt` conserva la frase del borrador y **añade** la prohibición de intérpretes y de herramientas de Unix: solo órdenes internas de
`cmd.exe`, `git`, y `certutil`, `findstr` y `fc` de System32. Motivos medidos en la sonda 1 del bloque A2-P2 (eventos custodiados, `R20261009T041427Z`):
- `py` no existe en el `PATH` del proceso hijo;
- `find` se resolvió al de `Git\usr\bin` y abortó bajo el sandbox (`CreateFileMapping … fatal error`), así que las herramientas MSYS2 no son fiables;
- `powershell.exe` lanzado desde `cmd` devolvió su propio argumento con código 0, un falso éxito;
- el `pwsh` de WindowsApps falla con «Acceso denegado».

Todo lo que exige la sonda se obtiene sin intérpretes. Los JSON, YAML, XML y esquemas se leen con `type`/`findstr`, los blobs y la historia con
`git` y los SHA-256 con `certutil`. La validación por esquema la impone la API sobre la salida (F) y la recalcula `compare`.

**Requisito para congelar**: O4 punto 4 exige que la invocación declare la shell «con el texto exacto de su línea `Shell`» del bloque de autorización
de transporte, que aún no está publicado. Esa línea tiene que ser byte a byte `shell-declaration.txt` (Q-SHELL).

## 8. Riesgos conocidos y puntos abiertos (deciden el Coordinator o la supervisión antes de congelar)

- **F-MAP-1 (hallazgo material para FX-02, no solo para la sonda).** En el fixture, EFF^1 (`930288c`, F_seed) ya contiene byte a byte las autoridades
  I62. `git diff EFF^1 EFF` solo toca `FIXTURE-MANIFEST.json`. El mapa, copiado de MC_I62, describe el cambio real de RackCad. Con el texto literal de
  16.13, Validate(M) en el fixture da **MAP_INVALID**:
  - MV-3: ningún archivo cambiado frente a 31 en `Files`;
  - MV-4: 7 entradas con blobs distintos de los observados (los `BaseBlob` MODIFIED; `docs/adr/0048…` y `PROMPT_TEMPLATES.md` no existen);
  - MV-6: no hay derivación;
  - MV-7: 16.3 en EFF^1 ya lleva el puntero.

  Consecuencia: el `Authority` de la verificación real de FX-02 sería fail con STOP (S-12; P-15), salvo que una disposición lo trate. La sonda no lo
  resuelve; mide que el Controller llega a esos mismos hechos. Sin un registro previo en la evidencia.
- **Tiempo (600 s fijos).** La sonda 1 de A2-P2 (8 pasos y unas 20 órdenes, con 5049 tokens de salida, de ellos 3537 de razonamiento) tardó unos
  90 s. Esta sonda tiene 32 pasos, 176 claves y dos objetos canónicos completos (unos 8 000-10 000 tokens de salida). El texto pide agrupar los
  comandos (unas 10-15 invocaciones de `cmd.exe`).
  - Estimación: 250-450 s.
  - Riesgo de superar 600 s: **medio**. Sube si el modelo no agrupa los comandos o lee archivos grandes enteros.
  - Si el Coordinator quiere más margen, lo que menos cobertura cuesta recortar son las claves por archivo de MV-6 (paso 22) y N5/N7a (pasos 30-31).
  - Un tope vencido = `NOT_DEMONSTRATED`, sin repetición.
- **Fidelidad bajo `cmd.exe`.** En la sonda 1 de A2-P2, `git log --format=%s` llegó al modelo con U+FFFD en lugar de `ó`. El paso 11 lo pone a prueba
  a propósito (`→` no existe en las páginas OEM). Una degradación es `NOT_DEMONSTRATED` de OP_C: es exactamente lo que A4-1 tiene que saber.
- **`^` y `%` en `cmd.exe`.** `EFF^1`, `EFF^2`, `--format=%B` y `%(trailers…)` exigen comillas o alternativas. No se dan pistas que la orden real no
  daría. Si se quiere, la línea `Shell` puede añadir una advertencia, que la orden heredaría.
- **Sin medir**: `certutil -hashfile` y `git ls-remote` contra el bare local bajo `read-only`.
- **`AttemptsRemaining`**: el estado `/v2` no tiene `max_attempts`. Se usa el valor por defecto de AP §9 (L289, «inicialmente tres»): 3 − 0 = 3. Lo
  confirma el Coordinator (Q-ATT).
- **`Mode` = NORMAL** para todas las citas en la forma I62. El texto lo enuncia como regla, porque el pseudocódigo no nombra el modo de las filas I62.
  Lo confirma el Coordinator (Q-MODE).
- **Ci y Tests fallan por construcción**: la única corrida real con TRX es la de `c785def9` y HEAD es `95bdc29d`. Es intencionado: así ejercen las
  ramas de fallo sin fabricar una corrida de HEAD. La corrida RED es sintética y está rotulada.
- **`ExpectedWorktree`** = toplevel del clon (`D:/r62-fixture/A2`, sin usuario).

Resumen de decisiones pendientes: **Q-F1** (VERBATIM o STRICT_PROJECTION), **Q-SHELL** (línea `Shell` del bloque de transporte = `shell-declaration.txt`,
con la prohibición de intérpretes), **F-MAP-1** (disposición sobre MAP_INVALID en el fixture antes de FX-02), **Q-ATT**, **Q-MODE**, **nc2**
(adaptación), política de comandos prohibidos completados (§5) y aceptación del riesgo de tiempo.

## 9. Autoensayo (obligatorio; hecho)

Se clonó `D:/r62-fixture/fixture-origin.git` (`fx/u1`, `--no-local`, `core.autocrlf=false`) en `D:\r62-fixture\tmp-a41v3-selftest`, en `95bdc29d`, el
mismo commit que A2. Secuencia: `stage` → `reference` → `unstage` → 20 casos con `compare`. El clon temporal se borró al final.

| Caso | Esperado | Obtenido |
|---|---|---|
| P0 salida perfecta (construida desde la referencia) | ALL_OPERATIONS_DEMONSTRATED | igual |
| N1 falta una clave (`status`) / N13 falta una clave de valor vacío (`rt`) | NOT_DEMONSTRATED | igual (OP_B) |
| N2 hash erróneo | NOT_DEMONSTRATED | igual (OP_B) |
| N3 conteo TRX erróneo | NOT_DEMONSTRATED | igual (OP_D) |
| N4 veredicto del mapa erróneo / N4b `ClauseMapBlob` erróneo | NOT_DEMONSTRATED | igual (OP_E) |
| N5 copia no ASCII no textual (`→`→`->`) / N5b `Objective` sin `ñ` | NOT_DEMONSTRATED | igual (OP_C / OP_F) |
| N6 `ls-remote` alterado en la salida | NOT_DEMONSTRATED | igual (OP_B) |
| N6b `ls-remote` del origen cambiado entre antes y después | REFERENCE_UNSTABLE | igual |
| N7 delegación inválida contra el canónico / N8 verificación incoherente / N12 effort incoherente con routing / N16 un `Result` erróneo | NOT_DEMONSTRATED | igual (OP_F) |
| N9 intérprete completado / N10 tope vencido / N11 escritura en el área ignorada | NOT_DEMONSTRATED | igual |
| N14 resultado de nc2 erróneo / N15 campos de N7b erróneos | NOT_DEMONSTRATED | igual (OP_G) |

Las salidas sintéticas validan contra `probe-schema.json` y contra los dos canónicos con el validador local, salvo N7, que no valida, como se
esperaba. `jsonschema` no está disponible y el informe lo registra. `status --ignored`: vacío → `!! artifacts/` → `!! artifacts/` → vacío
(restaurado). Puertas de `run_probe.sh` en un arnés sin `measure` ni `codex`, con un registro sintético y el clon temporal:
- pasan con los literales;
- se niegan sin línea de consumo, con una medición previa en `EVIDENCE_ROOT`, con el trío anterior, con un marcador `<…>`, con un directorio no
  fijado, con un clon con ignorados, con el marcador de lanzamiento, con salidas o escenario previos en `out/`, con el TRX alterado, con un BAD_SHA
  existente y con el trío anterior en el marcador.

Los candidatos de los cuatro literales y la línea de consumo se encontraron con `grep -F`/`-Fx` en el archivo de decisiones real. `bash -n` y
`py_compile` pasan. Detalle: `selftest-report.json`.

## 10. Cambios frente al borrador

| Archivo | Cambio |
|---|---|
| `probe-prompt.template.txt` | 12 → 32 pasos en bloques A-G; escenario S; PLAN y VERIFY canónicos completos; forma I62 de 16.13 (sustituye a `clause-map-step.*`, Q-A4-03 resuelta: RESOLVE en forma I62); nc1-nc4 y N4/N5/N7a/N7b; instrucciones de agrupación por tiempo |
| `probe-schema.json` | `steps` (sin `action`) + `delegation_v2` y `controller_verification_v2` canónicos incrustados (VERBATIM); nuevo `probe-schema.strict-projection.json` |
| `shell-declaration.txt` | añade la prohibición de intérpretes y herramientas de Unix (§7) |
| `probe_tools.py` | reescrito: validador local, auditoría estricta, `stage`/`unstage`, YAML mínimo, Validate/Resolve I62, 14 comprobaciones, routing, negativos, `synth` y `selftest`; copias de stage planas (MAX_PATH) |
| `run_probe.sh` | trío nuevo (`73890CA3…`, `baseline-keynames-73890CA3.json`); `DISPOSITION_TRIO_MARKER`; consumo con la huella nueva; clon fresco sin ignorados y en la punta; RED/BAD_SHA/TRX/esquema comprobados; stage/unstage alrededor del lanzamiento con cuatro instantáneas de `status --ignored`; corrige la comprobación de salidas previas (el `ls` conjunto del borrador nunca se negaba si faltaba uno de los patrones) |
| `gates.env.template` | todo fijado salvo cuatro literales con candidatos; BASE..HEADX = `1a4fc9c6..ae25b596` (Trailer en los dos sentidos, verificado); RED = `d30fb6a9`; nuevas variables del escenario |
| `consumo-line.txt` | línea de §65 (trío `73890CA3…`), 501 bytes, sin fin de línea |
| `baseline-keynames-73890CA3.json` | renombrado; contenido idéntico (107 nombres) |
| retirados | `clause-map-step.RESOLVE.txt`, `clause-map-step.DECLARED_UNAVAILABLE.txt`, `baseline-keynames-6518EFAB.json` |
| sin cambio | `cfg-fp.ps1` |

## 11. Archivos (SHA-256)

| Archivo | Uso | SHA-256 |
|---|---|---|
| `run_probe.sh` | `measure`, `probe` (con stage/unstage) y `compare`, con las puertas | `f33aa9cb82cf9573fdb62b3c3056c8f42c6587bb755b8ada3b217158561e5e12` |
| `probe_tools.py` | esquemas, texto, escenario S, referencia, comparación y autoensayo | `cef55f604328dcaab11659d710034f2472b80948ce6f06e10692d5bcf1962bd6` |
| `probe-prompt.template.txt` | texto de la sonda (32 pasos, bloques A-G) | `b16c3f49539bf35341ef44810501ac4f97835a0dbbf5b85bcddbecfaa8c50b96` |
| `shell-declaration.txt` | declaración de shell (= línea `Shell` del bloque de transporte de O4) | `062c234e06ab718cfd154434967460249d67fe8a8a7a8c0573bf4653ac19faf0` |
| `probe-schema.json` | esquema de salida VERBATIM (canónicos incrustados) | `2e975f97011346ad1595d4f0ccf78c1b5f649995244ffabeb4a275b8ff066309` |
| `probe-schema.strict-projection.json` | alternativa STRICT_PROJECTION (sin `minLength`) | `2883862d0e74ce6f028cad3158776df694a25cdb5e8aff83e320a95c6175ae0e` |
| `gates.env.template` | puertas y parámetros; solo cuatro literales por rellenar | `0b8af4cfac0cf8dec9e5ed44248d80c35b32bffec0a6631c0d4379b469b8f1bd` |
| `consumo-line.txt` | línea literal `A4-SONDA-CONSUMO` de §65 (trío `73890CA3…`) | `496b8d74dd7020646aafd2f603e12477eafd8a169d97c72148c7819f3e706fe1` |
| `cfg-fp.ps1` | huella de `config.toml` (SHA-256 y nombres saneados, nunca valores); sin cambio | `b1d833c0abc06fec6e27367b6df2715e7cdca321f0d6c6fbadb9f634e6a31bab` |
| `baseline-keynames-73890CA3.json` | los 107 nombres saneados (contenido idéntico al de `6518EFAB`) | `076637c550ff0b3109447ef00263ff087749f0aaa71f299d524e43b8cf82b70b` |
| `selftest-report.json` | informe del autoensayo y del arnés de puertas | `ca0fc51e7099bfa7300920375ca2c95fbf1be89b74513d4e715c1737d0d1f7e7` |

`README.md` no se lista a sí mismo; su SHA-256 se registra al congelar.
