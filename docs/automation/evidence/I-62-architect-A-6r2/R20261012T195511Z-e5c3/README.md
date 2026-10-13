# I-62 — Kit de la revisión formal de la ronda 2 de A-6 por un Architect independiente con `claude-cli` (R20261012T195511Z-e5c3) — FASES 1 y 2

```text
LogicalReviewRequestId: L20261012T195511Z-e5c3   InvocationId: I20261012T195511Z-e5c3   AttemptSeq: 1   RunId: R20261012T195511Z-e5c3
SessionId:      d95fcaba-f7db-4408-af85-497e565d114e (uuid4, generado una vez el 2026-10-12T19:55:11Z; prep/ids.txt)
Autorización:   disposición del Coordinator §80 (cuerpo pegado, 2 833 bytes, SHA-256 1342b5be1a5de104ac08a1115ddc999d2e6dfbe80100affaa16ea44439fa7f99,
                el que declara decisiones §80), punto 6: preparar el kit de la revisión siguiente (a-f); sin acreditación excepcional. §80, punto 5,
                y §79, punto 7: ninguna revisión nueva sin una disposición posterior. Owner CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62,
                solo lectura). UNA revisión, sin reintento automático
Consumo:        cubierto por CLAUDE-CLI-I62 = A, la misma base que la revisión de la ronda 1 (R20261011T215735Z-c434)
Objeto:         docs/initiatives/I-62-A-6.md, ronda 2, PROPUESTA, blob 86da0ae770e683a647f2724860a4bd890061154d (110 243 bytes, 1 145 líneas),
                publicado en ae9fe0e94b2dea9bc840d358d876deb6a6407391 (padre 2c531b53, con la ronda 1); guardas `run` PASS custodiadas en
                04833bf9f402d0aee325d4ede126622f31d4f1e5
Commit del clon: 0f4d8cbcbe4213e741285e7c3fa090830d38c4a1 (HEAD de la rama en la fase 2; añade r2/readable/)
Ronda 1:        blob eac473c0d2e58cf1c4594f48a8816b3833af6509, en el run como a6-r1.md (kit/a6-r1.md, 69 226 bytes, 748 líneas, extraído con
                `git cat-file blob` del worktree, solo lectura); salida de la revisión R20261011T215735Z-c434 en el cierre (output.json, e90d685b)
Clon / run:     D:\r62-arch-a6r2 (HEAD separado en 0f4d8cbc, sin ramas ni remoto) y D:\r62-arch-a6r2-run (order.txt = day3\s80_body.txt,
                prompt.md = kit/prompt.md, a6-r1.md = blob eac473c0), creados en la fase 2
Transporte:     el de la ronda 1 sin cambio: claude-cli 2.1.293, %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256
                8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa; claude-opus-5-5, effort xhigh; Read, Grep y Glob (+ StructuredOutput);
                --safe-mode, sin MCP; --add-dir del run; prompt por stdin; resultado por --json-schema; tope de 3 600 s; espera de 120 s tras
                `auth status`; ArgsTemplate idéntica a la de la ronda 1
Compuerta D-01: MEASURED sobre R20261008T183600Z-char (claude-cli-characterization.json, SHA-256 20d0de2c…, blob 541956eb), fijada sobre la
                copia del clon D:\r62-arch-a6r2; transport_gate.py byte a byte el de la ronda 1
Compuerta 11:   11-OwnerAndAuthorizationGate (kit/owner_gate.py), NUEVA y BLOQUEANTE: línea del Owner de §78; CLAUDE-CLI-I62 = A y el mismo
                binario; A-6 en la HEAD del worktree con el blob de la ronda 2 fijado; y una disposición del Coordinator numerada después de §80
                que contenga el blob completo y este RunId. Hoy no existe (AuthorizationSections = []): la revisión NO se puede lanzar
Estado:         FASES 1 y 2 terminadas: clon, cierre fijado (214 archivos, 7 035 120 bytes), prompt renderizado, verify_clone AllChecks,
                autoprueba y mutación sobre el clon real, manifiesto (congelación) y preflight en seco, bloqueada SOLO por la autorización de la
                compuerta 11. Nada lanzado; ningún CLI de modelos ejecutado; ni red ni credenciales
```

## Qué pide §80, punto 6, y dónde lo cubre el kit

| Exigencia (`order.txt`, líneas 41-51) | Prompt | Esquema (`rackcad-a6r2-architect-review/0`) | Auditor v5.2-a6r2 |
|---|---|---|---|
| (a) lectura directa de V14 L2333-L2334 | Paso 0.1: Read de V14 con `offset` 2325, `limit` 18; regla de lectura: una cita dentro de A-6 no es una lectura de la fuente | `Step0.V14DirectReadL2333L2334`; `DirectReads` | V14 L2333-L2334 entregadas fielmente por Read en el archivo de V14, y una entrada de `DirectReads` que las cubra (dos motivos) |
| (b) R1-R5 y O1-O3 | comparación del hallazgo literal (salida de la ronda 1), §79 punto 2, `a6-r1.md` y §3.16 de la ronda 2; R3 no puede estar CORRECTED si R3 quedó INCOMPLETE | `Round1FindingResolution` (A62-A6-R1..R5), `Round1OptionalDisposition` (A62-A6-O1..O3) | cobertura exacta; ids sin repetir; no CORRECTED ⇒ REQUIRED nuevo vinculado; CORRECTED ⇒ ninguno; NOT_ADDRESSED ⇒ un hallazgo |
| (c) G1-G6 uno por uno | los seis aspectos de §80, punto 3; `MitigationTested` comprobado en `r2/`, no declarado | `GapAssessment` (G1..G6; `Assessment`, `MitigationTested`) | cobertura; REQUIRES_PIECE/TEXT ⇒ REQUIRED; REQUIRES_OWNER ⇒ `OwnerDecisionRequired`; ACCEPTABLE_RESIDUAL ⇒ ningún REQUIRED |
| (d) materialidad M-01 | M-01 de LIFECYCLE §3 frente a A-6 §5 y al UNKNOWN de la ronda 1 | `M01Verification` (+ `MaterialityConfirmation` M-01..M-08) | A6Declared = A-6 §5; Agreement coherente; DISAGREE ⇒ hallazgo; = fila M-01 de la materialidad |
| (e) texto normativo frente al código real del aceptante | diecisiete afirmaciones de A6-5, regla 2, y A6-3, reglas 5-6; «lee el código» | `AcceptorCodeContrast` (`A6Refs`, `AcceptorRefs` ≥ 1, MATCHES/DIFFERS) | AcceptorRefs solo en `bmin_accept.py` y A6Refs solo en A-6, sobre líneas leídas; DIFFERS ⇒ REQUIRED |
| (f) procedencia e integridad en el momento de uso | trece piezas: escritura de preparación, momento, pasos (i)-(viii), orden declarada, fidelidad por los bytes, fallo cerrado | `ProvenanceIntegrityAtUse` | cobertura; INADEQUATE ⇒ REQUIRED; ADEQUATE ⇒ ninguno |
| sin acreditación excepcional | ídem | — | ACCREDITED exige cero motivos (sin excepciones) |

Además se conservan los campos de la ronda 1 (temas de §77, punto 2; A6-1..A6-9; separabilidad; M-01..M-08; compatibilidad; superficies;
ubicación; D.5/OV; líneas del Owner; autoridad; `IfAgreed`, ahora con doce campos; `KnownLimitations`; `Reads`; `Step0`). AGREED exige, además de
lo de la ronda 1, R1..R5 CORRECTED, G1..G6 ACCEPTABLE_RESIDUAL, todas las afirmaciones MATCHES y todas las piezas ADEQUATE.

## Independencia del preparador

Quien preparó este kit es un subagente de la sesión principal (la sesión que preparó A-6 y su ronda 2). Leyó la orden y decisiones §77-§81, el kit
de la ronda 1 entero, su salida y su auditoría, el borrador de la ronda 2 (secciones de los deltas, §3.12-§3.16, §5, §7 y §8), las guardas de la
ronda 2 (su interfaz, G10 y G11) y, de `bmin_accept.py`, solo las líneas que G10 cita (para nombrar las afirmaciones del contraste). El prompt no
contiene veredicto esperado ni valoraciones de la preparación; la evidencia de I-62 (`I-62-evidence.md`) queda fuera del cierre, como en la ronda 1.
El kit no ejecuta el paquete B-min, la reproducción ni las guardas en la fase 1.

## Contrato y auditor

- **Cierre** (`kit/make_closure.py`, fase 2, en el commit del clon; cada archivo por su blob; rangos calculados desde encabezados, filas y líneas
  citadas): A-6 de la ronda 2 entera (mapa de secciones, tablas de §3.2, §3.3, §5 y §3.16, filas R1-R5 y O1-O3, bloques G1-G6, reglas de cada delta,
  líneas del Owner, filas de §8); `docs/automation/evidence/I-62-A6/r2/` entero; el paquete B-min entero (árbol 3caa0dfa); el dictamen de B-min
  (output.json y README.md); la salida de la revisión de la ronda 1 (output.json, con rangos por hallazgo); la base normativa de la ronda 1 (V14,
  AUTOMATION_PLAN, ADR-0046, ADR-0048, LIFECYCLE, A-1..A-5, agent-execution README, routing.md, adapters/codex-cli.md, cinco esquemas,
  controller-contracts.md, decisiones con §43, §57, §61, §64 y §73 en adelante); toda fuente de G11 (Sources y GuardSources del resultado `run`
  custodiado), con sus rangos, **incluida `I-62-evidence.md`** (fase 2: decisiones §79, punto 5; secciones §140 en adelante; prefijo comprobado);
  `r2/readable/` con su MANIFEST comprobado; run: `order.txt` y `a6-r1.md` (premisas) y `prompt.md`. **Excluidos:** `docs/automation/evidence/I-62-C1C3/`
  (decisiones §81), la evidencia de la ronda 1 fuera de `r2/`, el resto del directorio de la
  revisión de la ronda 1 (`audit.json`, `README.md`, `kit/`, `launch/`), las rutas que A-6 solo nombra como superficies (G11 Paths sin líneas),
  `requests/`, transcripciones, memoria, sondas, WORKFLOW, mapa de cláusulas, estado y fixture. Comprobaciones de identidad: el padre de la
  publicación tiene `eac473c0`; la publicación, la custodia de las guardas y el clon, el blob de la ronda 2; `r2/` sin cambio tras la publicación
  salvo el resultado `run`; el resultado `run` = Mode RUN, Base = padre, Head = publicación, PASS; la autoprueba = PASS con A6TextBlob y ToolBlob;
  decisiones crece por añadido desde `d094823a` (6a681cec, §80 registrada); la línea (a) del Owner una vez en A-6 y en decisiones §78; los blobs de
  G11 iguales en el clon (salvo decisiones y la evidencia, que crecen); A-6 §5 «Activado» = M-01 no, M-02..M-05 sí, M-06 y M-07 no, M-08 sí.
- **Herramientas y lectura:** las de la ronda 1, sin cambio (solo Read, Grep y Glob, y StructuredOutput; archivos de más de 60 000 bytes por tramos
  de 300 líneas; Grep solo sobre archivos; Glob solo dentro del cierre; toda denegación es violación).
- **Prompt** (`kit/prompt.template.md` → `kit/prompt.md` en la fase 2): neutral; explica el objeto de la ronda 2, las seis exigencias, la regla de
  lectura («toda referencia solo a líneas leídas con Read en ese mismo archivo; una cita dentro de A-6 no es una lectura de la fuente»), la lista G11
  como lista de comprobación, que `a6-r1.md` es el blob `eac473c0` y que la condición de lanzamiento (la disposición posterior del Coordinator) está
  fuera del clon y su ausencia no es un defecto de A-6. Marcadores para el blob, el commit, la publicación, el RunId, las tablas del cierre y la lista
  G11. Vista previa sintética: `prep/phase1/prompt-preview.md` (71 507 bytes, 523 líneas, con las rutas reales del clon y del run y los commits
  sintéticos).
- **Esquema** (`kit/result.schema.json`, `rackcad-a6r2-architect-review/0`; 18 336 caracteres ASCII en forma compacta; `prep/gen_schema.py`): solo
  las palabras clave ya medidas con claude-cli 2.1.293 (type, additionalProperties, required, properties, enum, pattern, minLength, minItems,
  items). Hallazgos nuevos con ids `A62-A6R2-R<n>` / `A62-A6R2-O<n>` (los de la ronda 1 solo se disponen).
- **Compuerta de transporte D-01** (`kit/transport_gate.py`, `kit/c3_analysis.py`): byte a byte los de la ronda 1.
- **Compuerta 11** (`kit/owner_gate.py`, comprobación 11 de `launch.py`, BLOQUEANTE; se evalúa también en seco porque solo lee git; nunca escribe):
  en la HEAD del worktree de I-62 (rev-parse, show, hash-object sin -w y merge-base, con GIT_OPTIONAL_LOCKS=0):
  (a) la línea del Owner `A6-ADR0046-9-FX02 = A (…)` (SHA-256 `83152eae…`) como línea completa después de `## 78.`, ninguna otra línea que empiece
  por «A6-ADR0046-9-FX02 =», y la misma línea en la A-6 de la ronda 2 del commit del clon; (b) `CLAUDE-CLI-I62 = A` (SHA-256 `fbeade41…`) como línea
  completa, ninguna otra «CLAUDE-CLI-I62 =», y el SHA-256 medido del binario = el del cierre; (c) A-6 en la HEAD = el blob de la ronda 2 fijado
  (40 hexadecimales, distinto de `eac473c0`); (d) **autorización**: una sección «## <n>. Disposición del Coordinator…» con n > 80 cuyo cuerpo contenga,
  como tokens literales, el blob completo y el RunId `R20261012T195511Z-e5c3`; además, decisiones del árbol de trabajo = la de la HEAD, la HEAD
  desciende del commit del clon y las decisiones del clon son prefijo exacto de las de la HEAD. Una disposición del Owner (como la §81 de hoy, sobre
  C1-C3) no cuenta, ni un blob abreviado, ni los dos tokens en secciones distintas, ni tokens pegados a otros caracteres. El registro de la
  comprobación lista las secciones encontradas (`AuthorizationSections`) para que la sesión principal confirme, antes de lanzar, que es la
  disposición que autoriza esta revisión.
- **Auditor v5.2-a6r2** (`kit/post-review.py`, derivado de v5.1-a6 con `prep/adapt_post_review.py` por quince sustituciones exactas; diff en
  `prep/diffs/post-review.diff`): las reglas de lanzamiento, compuerta, stream-json, transcripción, herramientas, lectura, fidelidad, custodia del run
  y clon quedan byte a byte. Motivos nuevos: preflight sin la compuerta 11 completa (líneas fijadas; blob de la ronda 2 en la HEAD, RunId y una
  sección de autorización con los dos tokens); V14 L2333-L2334 no entregadas por Read en V14; `DirectReads` sin una entrada que las cubra;
  `AcceptorRefs` fuera de `bmin_accept.py` o `A6Refs` fuera de A-6 (y, como toda referencia, sobre líneas no leídas); ids repetidos o cobertura
  incompleta de R1..R5, O1..O3, G1..G6, las diecisiete afirmaciones y las trece piezas; las reglas de coherencia de la tabla de arriba; AGREED con un
  R no CORRECTED, una brecha REQUIRES_*, una afirmación DIFFERS o una pieza INADEQUATE; ClauseRefs en cada elemento nuevo; referencias de los
  elementos nuevos en `FindingIds` inexistentes.
- **Lanzador** (`kit/launch.py`, derivado del de la ronda 1 con `prep/adapt_launch.py`; diff en `prep/diffs/launch.diff`): constantes del clon, del
  run y de la sesión; tres archivos del run con `a6-r1.md` = blob `eac473c0`; cierre con el blob de `kit-params.json` y el RunId del kit;
  directorio de proyecto `D--r62-arch-a6r2*` (el de la ronda 1, `D--r62-arch-a6`, ya existe y no debe contar); la comprobación 11 nueva, evaluada antes
  de ejecutar el binario. No reintenta. `--dry-run` no ejecuta el binario.
- **Ayudantes de la fase 2:** `prepare_clone.py` (clon y run, `kit-params.json` con la publicación, su padre, la custodia y el blob), `make_closure.py`,
  `verify_clone.py` (con la reproducción de las guardas de la ronda 2: `run` sobre la publicación y `self-test` sobre la base que registra la
  autoprueba custodiada, ambas enteras iguales a las custodiadas), `make_manifest.py`, `finish_kit.py`; `read_fidelity.py` sin cambio.

## Autoprueba y mutación de la fase 1 (entorno sintético)

Al empezar la fase 1 no existía el commit de la publicación (apareció durante la preparación: punto abierto 1) y la fase 1 no depende de él: prueba
el kit sobre un entorno SINTÉTICO (`prep/make_synthetic_env.py`, bajo
`.syn/` con ruta 8.3 por MAX_PATH; borrado al terminar): un repositorio de tres commits que imita la publicación (C0 padre con los archivos del
cierre tomados de la HEAD del worktree de entonces, `2c531b53`, por `git archive`/`cat-file`, con decisiones en `d094823a`, y la A-6 de la ronda 1; C1 publicación con el borrador de la ronda 2 y `r2/`
desde el scratchpad; C2 custodia con un resultado `run` sintético que lleva la lista G11 del borrador), HEAD separado sin ramas ni remoto, y su cierre
generado por el MISMO `kit/make_closure.py` (134 archivos, 4 351 656 bytes). Diferencias declaradas: commits, resultado `run` y autoprueba
sintéticos (A6TextBlob y ToolBlob sustituidos); los árboles de `I-62-architect-A-6/` y `agent-execution/` no se reproducen enteros
(SyntheticSkip; dos ADR omitidos por MAX_PATH).

- **Autoprueba** (`prep/phase1/selftest-result.json`; `kit/selftest-post-review.py` con `RACKCAD_A6R2_KITDATA` = el entorno sintético): **233/233
  PASS** — 162 casos de auditoría, 41 de la compuerta de transporte y 30 de la compuerta 11 (O00-O27, O10b y la sonda L00) —, con el clon sintético
  limpio y los tres archivos del run idénticos a sus copias al terminar; 27 min (≈ 20:28Z-20:55Z del 2026-10-12). Casos 00-108 de la ronda 1
  adaptados; 108b-108d (registro de la compuerta 11 sin autorización, con otro blob en la HEAD, con otro RunId); 109-153, las reglas nuevas, entre
  ellos el 110, que reproduce el fallo de la ronda 1 (V14 L2333-L2334 citadas habiendo leído solo la cita de A-6: tres motivos), y el 153, la misma
  cita tras la lectura directa (acreditada); positivos coherentes 00, 16, 25, 41, 100-102 y 151-153. L00: `owner_gate.check` sobre el worktree real
  (HEAD `2d7f8535`, A-6 en la HEAD = `86da0ae7`) da Ok = false con un único problema: «falta la autorización». SHA-256 `8a8356c3…`.
- **Mutación** (`prep/phase1/mutation-result.json`; registro en `prep/phase1/mutation.out`): cada sitio de regla de `post-review.py` (131),
  `transport_gate.py` (36) y `owner_gate.py` (15) desactivado por turno contra los 232 casos (sin L00): **182/182 mutantes muertos**, base 232/232;
  8 293 s (20:56:25Z-23:14:38Z). Sobre `post-review.py` `f68042dd…`, `transport_gate.py` `b05fedd1…` y `owner_gate.py` `969844a9…` (los de abajo).
  SHA-256 `ded711b7…`.
- El entorno sintético se borró al terminar (`make_synthetic_env.py --remove`); su resumen está en `prep/phase1/synthetic-env.json` y se regenera
  con `python -B prep/make_synthetic_env.py` (lee el worktree y el scratchpad del momento, así que puede no dar los mismos SHA).

## Fase 2 ejecutada (2026-10-12/13): resultados

Orden de la sesión principal (mensaje de la fase 2): clon en `0f4d8cbcbe4213e741285e7c3fa090830d38c4a1` (HEAD de la rama; desciende de la
publicación `ae9fe0e9…` y de la custodia `04833bf9…`; añade `r2/readable/`); compuerta de autorización sin cambio y `AuthorizationSections`
visibles en la preflight en seco; R3 REPRODUCED (`r2/repro/README.md`, evidencia §157: la frase del prompt queda como contexto); `r2/readable/` y
los canónicos de `r2/` en el cierre, con la copia legible como lectura y cita; `I-62-evidence.md` DENTRO del cierre (decisiones §79, punto 5);
C1-C3 (`docs/automation/evidence/I-62-C1C3/`, decisiones §81) FUERA; medir el cierre y estimar la duración; informar cualquier archivo no UTF-8.

Cambios del kit antes de ejecutar (archivos de preparación en `prep/`): `kit/make_closure.py` (`prep/phase2_edit_make_closure.py`: evidencia
dentro con sus secciones §140 en adelante y su prefijo comprobado; MANIFEST de `readable/` comprobado; C1-C3 fuera y declarado; todos los archivos no
UTF-8 se informan y detienen el script; en la lista G11 del prompt, nota de prefijo para los archivos que crecen por añadido);
`kit/prompt.template.md` (`prep/phase2_edit_prompt.py`: copias legibles, evidencia como evidencia y no autoridad, C1-C3 fuera); `kit/launch.py`
(`prep/adapt_launch.py`, una sustitución más: `AuthorizationSections` en la salida de `--dry-run`). `post-review.py`, `owner_gate.py`,
`transport_gate.py`, el esquema y el selftest no cambian (mismos SHA-256 que en la fase 1).

Pasos (los de `finish_kit.py`, ejecutados uno a uno para controlar cada resultado; salidas saneadas en `prep/phase2/step*.out|.raw` y la traza en
`finish-kit.log`):

1. `prepare_clone.py 0f4d8cbc… ae9fe0e9… 04833bf9…` → `D:\r62-arch-a6r2` (git clone --no-local -c core.autocrlf=false --single-branch; HEAD
   separado en `0f4d8cbc`; sin ramas ni remoto) y `D:\r62-arch-a6r2-run` (order.txt, a6-r1.md); `kit/kit-params.json` (padre de la publicación
   `2c531b53…`; añadidos a `r2/` tras la publicación: el resultado `run` y los 28 archivos de `readable/`). Precondiciones: todas en true.
2. `make_closure.py --render-prompt` → `kit/prompt.md` (86 690 bytes, 611 líneas, SHA-256 `1bb85b88…`); 3. copiado al run, idéntico.
4. `make_closure.py` → `kit/closure.json`: **214 archivos, 7 035 120 bytes, 122 821 líneas** (r2: 131 archivos, 3 047 462 bytes; `r2/readable/`:
   28, 1 441 015; paquete B-min: 30, 541 797; el resto, 25 archivos, 2 004 846, con la evidencia de I-62: 366 189 bytes, 4 197 líneas, blob
   `40ebe6b2`, prefijo comprobado del que fija G11). Ningún archivo no UTF-8 (todos los del cierre se decodifican). 31 archivos de más de 60 000 bytes
   (por tramos de 300 líneas); 37 con líneas de más de 2 000 caracteres (27 de ellos con copia legible). MANIFEST de `readable/`: 27 copias con
   blob canónico y SHA-256 comprobados; ninguna línea de una copia pasa de 2 000 caracteres. C1-C3 fuera.
5. Compuerta D-01: `transport_gate.py init` + `set MEASURED` sobre `D:\r62-arch-a6r2\…\claude-cli-characterization.json` (SHA-256 `20d0de2c…`) →
   `kit/transport-gate.json` en MEASURED (SHA-256 `5da1af22…`).
6. `verify_clone.py` → **AllChecks = true** (2 min): HEAD, separado, sin ramas ni remoto, limpio, blobs del cierre, historia del objeto, commits,
   run, caracterización; guardas de la ronda 2 reproducidas desde el clon contra el worktree (solo lectura): `run` 2c531b53..ae9fe0e9 PASS, igual
   entera a la custodiada; `self-test` 586/586 mutantes muertos, igual entera a la custodiada; worktree sin cambio.
7. Autoprueba sobre el clon y el run REALES (`kit/selftest-result.json`): **233/233 PASS** (162 de auditoría, 41 de la compuerta de transporte, 30
   de la compuerta 11 con L00), clon limpio y run idéntico al terminar; 31 min (23:29:05Z-00:00:19Z). L00 (worktree en `0f4d8cbc`, A-6 en la HEAD
   = `86da0ae7`): un único problema, «falta la autorización», `AuthorizationSections` = []. SHA-256 `fc89d992…`.
8. Mutación sobre el clon real (`kit/mutation-result.json`; los archivos del kit cambiaron después de la fase 1): **182/182 mutantes muertos**, base 232/232 (sin L00); `post-review.py` 131/131 (`f68042dd…`), `transport_gate.py` 36/36 (`b05fedd1…`), `owner_gate.py` 15/15 (`969844a9…`); 2026-10-13T00:00:19Z → 2026-10-13T01:49:26Z. SHA-256 `d5833afb…`.
9. `make_manifest.py` → `kit/kit-manifest.json` (congelación): 24 archivos de `kit/` y los tres del run; **SHA-256 `3b06e8375ed571eb225bf7c3ae98f8ef885598fd945ce348725af7fe810c8c85`**.
10. `launch.py --dry-run` → `launch-dry-run.json` (junto a este README; capturado con `-X utf8`): `BlockingChecks` = ["11-OwnerAndAuthorizationGate"]; `NotRunInDryRun` = ["4-Version", "5-Auth"]; `AuthorizationSections` = []; comprobación 11: Problems = ["falta la autorización: ninguna disposición del Coordinator numerada después de §80 contiene, como tokens literales, el blob completo de la ronda 2 y el RunId R20261012T195511Z-e5c3 del kit (decisiones §79, punto 7; §80, punto 5)"]; líneas del Owner A6-ADR0046-9-FX02 [1572] (§78 en la línea 1564) y CLAUDE-CLI-I62 [1085]; A-6 en la HEAD `86da0ae7…` = fijado: True; decisiones por añadido: True; worktree limpio: True; desciende del clon: True; binario = cierre: True; comprobación 9: 21390 caracteres. SHA-256 `3c777c29…`.

**Duración estimada frente al tope de 3 600 s.** La ronda 1 duró 1 318 s (07:16:53Z-07:38:51Z) con 64 llamadas y 5 799 líneas entregadas de 27
archivos (≈ 20,6 s por llamada; ≈ 0,23 s por línea entregada). La ronda 2 exige como mínimo: V14 directa, A-6 entera (1 145 líneas, 4 tramos),
`a6-r1.md` (748, 3 tramos), la salida de la ronda 1, decisiones §79-§80, los rangos de G11 (unas 15 fuentes), las líneas del aceptante que pide el
contraste (hasta 2 816 líneas, 10 tramos), los README de `r2/` y las copias legibles que cite, y los rangos citados de la evidencia: del orden de
120-160 llamadas y 12 000-18 000 líneas. Estimación: **≈ 2 500-4 100 s**; el tope de 3 600 s puede alcanzarse si el revisor lee el aceptante
entero o muchas copias legibles. El cierre (7,0 MB) no se lee entero: los rangos orientan. Ampliar el tope cambiaría `TimeoutSeconds` del cierre
y de `launch.py` (y el kit custodiado): no se ha hecho; queda a decisión de la sesión principal.

## Condiciones exactas para el lanzamiento (nada lanzado)

1. Una disposición del **Coordinator** registrada en `docs/automation/decisions/I-62.md` de la rama, con encabezado «## <n>. Disposición del
   Coordinator…» y n > 80 (hoy hay §81, del Owner, que no cuenta), cuyo cuerpo contenga, como tokens literales, el blob completo
   `86da0ae770e683a647f2724860a4bd890061154d` y el RunId `R20261012T195511Z-e5c3`, con commit en la rama (sin cambios sin commit en decisiones).
2. Que la rama siga descendiendo de `0f4d8cbc…`, que decisiones crezca solo por añadido desde el clon, que A-6 en la HEAD siga siendo `86da0ae7…`,
   que la línea del Owner de §78 y la de CLAUDE-CLI-I62 = A sigan como líneas completas únicas y que el binario siga siendo el de SHA-256 `8693c4a0…`.
3. Custodia del kit por la sesión principal tal como queda (manifiesto SHA-256 abajo); si cambia cualquier archivo de `kit/`, `make_manifest.py`
   (o `--gate`) y volver a custodiar.
4. Repetir `python -B kit/launch.py --dry-run`: `BlockingChecks` = [], la comprobación 11 en Ok y `AuthorizationSections` = exactamente esa
   disposición (el Coordinator verifica la coincidencia); 4 y 5 solo se ejecutan en el lanzamiento real.
5. Lanzar UNA vez `python -B kit/launch.py` (sin reintento; ejecuta `--version` y `auth status`, espera 120 s, vuelve a medir el binario y lanza);
   después, auditor v5.2-a6r2 con la orden de abajo.

## Pasos de la fase 2 (referencia; ejecutados arriba)

1. Comprobar en el worktree (solo lectura) el commit de la publicación, su padre (A-6 de la ronda 1, `eac473c0`) y el commit que custodia el
   resultado `run` (`r2/a6r2-guards-result-run.json`, Mode RUN, Base = padre, Head = publicación, PASS). Si el resultado `run` se custodia con otro
   nombre, ajustar `R2_RUN_RESULT` / `GUARDS_RUN_RESULT` / `RUN_RESULT` / `GRR` en `make_closure.py`, `verify_clone.py`, `prepare_clone.py` y
   `selftest-post-review.py` ANTES del paso 3 (son archivos del kit; la congelación es posterior).
2. Confirmar que la versión final de A-6 conserva lo que `make_closure.py` exige (encabezados `### 3.4 A6-1 …` a `### 3.16`, tabla de §5 con
   «| M | Activado | Dictamen | Ronda 1 | Motivo |», tabla de §3.16, bloques «- **G1. …**» a G6, reglas «> n.» numeradas 1..n, filas de §8, las dos
   líneas del Owner en §7). Si cambia, `make_closure.py` se detiene y hay que adaptar los rangos antes de seguir.
3. `python -B "%TEMP%\claude\D--Documentos-Codex-Calculadora-de-racks\e5124bf0-ed19-48a1-97ba-574baf9d33a2\scratchpad\i62\day3\kit-a6r2-review\kit\finish_kit.py" <CLONE_COMMIT> <PUBLICATION> [<GUARDS_RUN_COMMIT>]`
   — pasos 1-10: clon `D:\r62-arch-a6r2` (git clone --no-local -c core.autocrlf=false --single-branch, HEAD separado, sin ramas ni remoto) y run
   `D:\r62-arch-a6r2-run` (order.txt, a6-r1.md); `kit-params.json`; prompt renderizado y copiado al run; `closure.json`; compuerta D-01 en MEASURED
   sobre la copia del clon del registro de caracterización (SHA-256 `20d0de2c…`); `verify_clone.py` (AllChecks, con las guardas de la ronda 2
   reproducidas); autoprueba y mutación sobre el clon y el run REALES (`kit/selftest-result.json`, `kit/mutation-result.json`); manifiesto
   (`kit/kit-manifest.json`: congelación); `launch.py --dry-run` → `launch-dry-run.json`, que debe bloquearse SOLO en 11-OwnerAndAuthorizationGate y
   SOLO por la autorización ausente (4 y 5 sin ejecutar).
4. Custodia del kit (procedimiento de la sesión principal). Si cambia cualquier archivo de `kit/`, `make_manifest.py` (o `--gate` tras un
   `transport_gate.py set`) y volver a custodiar.
5. **Lanzamiento, solo después de una disposición posterior del Coordinator** (§79, punto 7; §80, punto 5) que contenga el blob completo de la ronda 2
   y el RunId `R20261012T195511Z-e5c3`, registrada en decisiones como «## <n>. Disposición del Coordinator…» (n > 80) y con commit en la rama: repetir
   `launch.py --dry-run` (`BlockingChecks` = [], `AuthorizationSections` = esa disposición) y lanzar una vez `python -B kit/launch.py`.
6. Al terminar: integridad del kit y del run; `python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a6r2\d95fcaba-f7db-4408-af85-497e565d114e.jsonl" D:\r62-arch-a6r2-run\launch\stdout.jsonl D:\r62-arch-a6r2 D:\r62-arch-a6r2-run kit kit/result.schema.json audit.json output.json`.
   El veredicto no es el acuerdo: el acuerdo es del Architect y del Coordinator (LIFECYCLE §6) sobre el blob exacto. Ni A-6 AGREED, ni FX-02 PASS,
   ni F6 GATE PASS se declaran aquí.

## Desviaciones frente a la ronda 1 y frente a la orden, con su motivo

- **Compuerta 11 ampliada a la autorización** (§80, punto 5; §79, punto 7): el kit no se puede lanzar sin una disposición posterior del Coordinator;
  se reconoce por su encabezado y por los dos tokens. Nombre de la comprobación: 11-OwnerAndAuthorizationGate. La línea del Owner se busca después de
  §78 (donde está registrada), no de §77.
- **Clon en el commit de la publicación o en uno posterior que custodie el resultado `run`**: el resultado `run` solo puede existir después del commit
  que publica, y el Paso 0 lo lee (como en la ronda 1, donde el clon estaba en un commit posterior a la publicación y a la custodia).
- **Cierre**: además de lo que nombra la orden (G11, A-6, `r2/`, B-min, decisiones, salida de la ronda 1, run), la base normativa de la ronda 1 y el
  dictamen, para que el revisor pueda juzgar compatibilidad, superficies y materialidad como en la ronda 1; sin la evidencia de la ronda 1 fuera de
  `r2/` ni las rutas que A-6 solo nombra como superficies.
- **Temas de §77, punto 2, conservados** como `TopicVerdicts`: siguen siendo los temas de revisión de A-6 (tres REQUIRED REVIEW TOPIC).
- **Reglas de coherencia añadidas** a las de la orden (declaradas en el prompt): CORRECTED, ACCEPTABLE_RESIDUAL, MATCHES y ADEQUATE sin REQUIRED
  vinculado; NOT_ADDRESSED y DISAGREE de M-01 con un hallazgo; INADEQUATE con un REQUIRED; AGREED también con todas las piezas ADEQUATE; `ClauseRefs`
  en cada elemento nuevo; ids de los hallazgos nuevos en un espacio propio (`A62-A6R2-…`).
- **`a6-r1.md` como archivo del run y premisa** (puede citarse como `a6-r1.md:<línea>`); el lanzador comprueba su blob.
- **Auditor y lanzador derivados por sustituciones exactas** (en la ronda 1, de los de B-min por el mismo método): lo no sustituido queda byte a byte.
- **Autoprueba de la fase 1 sobre un entorno sintético** (la fase 1 no depende del commit de la publicación); la definitiva se repite en la fase 2 sobre el clon real
  (`finish_kit.py`, pasos 7-8). La autoprueba lee el entorno de `RACKCAD_A6R2_KITDATA` si se da; por defecto, el del kit (fase 2).
- **Unión del caso 03** contra un directorio propio bajo `.tmp/st` (en la ronda 1, contra otro clon `D:\r62-arch-a5`).
- **Sonda L00**: la autoprueba ejecuta `owner_gate.check` sobre el worktree real (solo lectura) y exige que hoy se bloquee por la autorización.
- **`I-62-evidence.md` dentro del cierre** (fase 2, decisión de la sesión principal; decisiones §79, punto 5: acceso directo a toda fuente que el
  texto cite): desviación deliberada frente a la ronda 1, que la excluía para que el revisor juzgara sin el anclaje de los resúmenes y valoraciones
  de la sesión que preparó A-6 y este kit. **Riesgo de anclaje declarado**: el prompt la presenta como evidencia de hechos que contrastar, nunca
  como autoridad ni como juicio sobre A-6, y avisa de que §156 y §158 (C1-C3) son ajenos.
- **`r2/readable/`** (fase 2): copias legibles de los 27 JSON de una sola línea, en el cierre junto a los canónicos; el prompt manda leer y citar
  la copia; `make_closure.py` comprueba el MANIFEST. Los sobres de `bmin/e2e/out/` siguen sin copia legible (como en la ronda 1).
- **C1-C3 fuera del cierre** (fase 2; decisiones §81, disposición del Owner): ni `docs/automation/evidence/I-62-C1C3/` ni nada de C1-C3; la
  compuerta 11 no cuenta §81 como autorización (no es del Coordinator).
- **`AuthorizationSections` en la salida de `launch.py --dry-run`** (fase 2) para que el Coordinator compruebe la coincidencia antes de lanzar.
- **`finish_kit.py` no se ejecutó de una vez:** sus diez pasos, uno a uno, con sus mismos comandos (`finish-kit.log`), para revisar cada resultado
  (como en la ronda 1).

## Supuestos no medidos

- La duración frente al tope de 3 600 s: el cierre real pesa 7,0 MB (la ronda 1, 2,23 MB) y el prompt 86,7 KB (la ronda 1, 43,7 KB);
  estimación ≈ 2 500-4 100 s (sección de la fase 2): el tope puede alcanzarse.
- Que el esquema (18 336 caracteres compactos) se comporte con claude-cli como los anteriores (mismas palabras clave) y que la línea de órdenes quede
  por debajo de 32 000 caracteres (medida en la comprobación 9 en seco: ver la fase 2).
- La compactación automática del contexto (no observada antes); el prompt avisa.
- Que la espera de 120 s evite el conflicto de renovación del token.

## Puntos abiertos de la fase 1 (estado tras la fase 2)

Resueltos por la sesión principal en el mensaje de la fase 2: 1 (clon en `0f4d8cbc`), 2 (compuerta tal cual, `AuthorizationSections` visibles),
3 (R3 REPRODUCED), 4 (`r2/readable/`), 5 (evidencia dentro), 6 (cierre medido: 214 archivos, todos UTF-8), 7 y 8 (ayudantes, autoprueba y
mutación ejecutados sobre el clon real), 9 (duración estimada arriba; tope sin cambiar), 10 (prefijo `d094823a` comprobado). Texto original:

1. **La publicación ya está en la rama (observado en solo lectura durante la fase 1; la fase 2 NO se ha empezado).** La HEAD del worktree pasó de
   `2c531b53` a `2d7f8535ef58e4ef474333d7d8d350717a0a3ec5`: `ae9fe0e94b2dea9bc840d358d876deb6a6407391` publica la ronda 2 (A-6 blob
   `86da0ae770e683a647f2724860a4bd890061154d`, el mismo texto que usó el entorno sintético; padre `2c531b53`), `04833bf9f402d0aee325d4ede126622f31d4f1e5`
   custodia `r2/a6r2-guards-result-run.json` (líneas 2-4 Mode RUN, Base `2c531b53`, Head `ae9fe0e9`; `Result` PASS en la línea 1311, la forma que
   espera el kit) y `2d7f8535` añade el diseño C1-C3 (decisiones §81, disposición del **Owner**, que la compuerta no cuenta como autorización). La
   sonda L00 de la autoprueba (worktree real, solo lectura) da hoy un único problema: falta la autorización. Los argumentos de `finish_kit.py` los fija
   el mensaje de la fase 2 (previsiblemente `<CLONE_COMMIT> ae9fe0e9… 04833bf9…`, con el clon en `04833bf9` o en un commit posterior).
2. **La disposición que autorice el lanzamiento** debe estar registrada en decisiones bajo «## <n>. Disposición del Coordinator…» (n > 80) y contener
   literalmente el blob completo de la ronda 2 y `R20261012T195511Z-e5c3`. Riesgo residual: una disposición del Coordinator que mencione ambos tokens
   sin autorizar el lanzamiento pasaría la compuerta; por eso la sesión principal comprueba `AuthorizationSections` en la preflight en seco antes de
   lanzar.
3. **R3.** §80, punto 5: con R3 INCOMPLETE la candidata no puede presentarse como preparada para revisión formal. El kit no comprueba el estado de R3
   (lo juzga el revisor; el prompt le dice que R3 no puede quedar CORRECTED si la evidencia lo registra INCOMPLETE); es una condición para la
   disposición del Coordinator.
4. **Resultados de `r2/` en JSON de una sola línea** (más de 2 000 caracteres: `independence/results/*.json`, `r1r4/*.json`, `repro/*/*.json` y, en la
   publicación, también `repro/_work/*.json`): Read solo entrega los primeros 2 000 caracteres de esas líneas, así que el revisor no puede citar sus
   cifras y depende de los README de `r2/`. El prompt las lista; si la sesión principal quiere que sean legibles, harían falta copias con sangría (un
   añadido posterior a la publicación que `prepare_clone.py` registraría en `R2AddedAfterPublication`), a decidir antes de la fase 2.
5. **`I-62-evidence.md` excluida** como en la ronda 1, aunque A-6 cita 15 rangos suyos (§140-§154) y §79, punto 5, pide «acceso directo a todas las
   fuentes que necesite citar»: el prompt manda declarar en `KnownLimitations` lo que dependa de ella. Si el Coordinator prefiere incluirla, cambian
   `make_closure.py` (EVIDENCE, NotTriggered) y el prompt antes de la fase 2.
6. **`r2/` publicado tiene más archivos que el entorno sintético** (`render/`, `neg-fixtures/`, `repro/_work/`, `repro/README.md`,
   `a6r2-sources-for-reviewer.json`, `a6r2-guards-result-preview.json`, `make_candidate_commit_a6r2.py`): el cierre real será mayor que el sintético
   (134 archivos, 4,35 MB) y, si algún archivo no es texto UTF-8, `make_closure.py` se detiene (STOP: decidir antes de lanzar).
7. **Ayudantes sin ejecutar en la fase 1** (necesitan el commit y el clon): `prepare_clone.py`, `verify_clone.py` (ejecuta las guardas de la ronda 2,
   `run` y `self-test` con 586 mutantes: duración no medida), `make_manifest.py`, `finish_kit.py` y `launch.py` (solo comprobada su sintaxis; sin
   `kit-params.json` no se importa). `make_closure.py` solo se ha ejecutado sobre el entorno sintético.
8. **Autoprueba y mutación definitivas en la fase 2**, sobre el clon y el run reales (`finish_kit.py`, pasos 7-8); el caso 03 crea y quita una unión
   temporal dentro de `D:\r62-arch-a6r2`. La autoprueba sintética tardó 27 min (la de la ronda 1, 11 min).
9. **Duración y tamaño** frente al tope de 3 600 s, y la línea de órdenes (estimada en ≈ 21 600 caracteres): los mide la fase 2 (comprobación 9).
10. **Prefijo de decisiones fijado:** `d094823a` (decisiones en `6a681cec`, §80). Si decisiones se corrige de otra forma que por añadido,
    `make_closure.py` y la compuerta 11 se niegan.

## Archivos

```text
SHA-256                                                           bytes  archivo
a76d13d68e18c9451319ba90de02af2cc3216c297bd2a9f8509b72e680f088b9   69226  kit/a6-r1.md                         (= blob eac473c0, ronda 1)
4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7    6070  kit/c3_analysis.py                   (= ronda 1, 4fc6115e)
3c0946d0d47740d29fc5600b27a7701516a7e81b9e0d334631adc1054cc413fe   55231  kit/clone-verification.json
6473227e5975847831ac658dfae57dc2cd3f95a1260a5e51de3a7085a0c57c3a  252862  kit/closure.json
411a9040eadf664b94b125d4ec5077c16282e95b99f88e56d59a2dbaa2b2aac9    6132  kit/finish_kit.py
3b06e8375ed571eb225bf7c3ae98f8ef885598fd945ce348725af7fe810c8c85    6132  kit/kit-manifest.json
6f6a76ed43807daa4eb9b1c758ee10f4842aa5a04866f31c0f4430fc56e1d4a7    4267  kit/kit-params.json
f2f58cec38b9f34e04d04e94f866d7d75a6bbd4864ccfbac734f8a52965f78ac   20736  kit/launch.py
d969bf2766b2424a3c4f7b3228e1c5badd25db739016af08b8cca0b939782c33   70883  kit/make_closure.py
f8ff9934f88e0c914b827ca963fb4fcde4d05fe10d4bc949f5851d525142fd6a    6647  kit/make_manifest.py
1812b82af522c78876149064216d07f66df7d5d1fceec1726c748a2bdd0d38fa    6843  kit/mutation-post-review.py
d5833afb47098d568522e0d61507dcfeb24d0b3a154e1330b3140b87bc77671e   50065  kit/mutation-result.json
1342b5be1a5de104ac08a1115ddc999d2e6dfbe80100affaa16ea44439fa7f99    2833  kit/order.txt                        (= day3\s80_body.txt, cuerpo §80)
969844a928571fe2d72be806fab21b5c7cb332e6ffd5e0c2d6142b027608ff1c   12135  kit/owner_gate.py                    (compuerta 11)
f68042dd28b9bbee63915bee5ba0ee0a872ed896528335a2cd820f1698452938   64618  kit/post-review.py                   (auditor v5.2-a6r2)
2d537e9396eb170713e9f5e80c73ad964ee38ece6fce596d967ac2c8627e920d    8256  kit/prepare_clone.py
1bb85b88a3d93212065355cd13d046e9fa5e333abf4e582e3f4b8c47e9b603b4   86690  kit/prompt.md
2d53646f366e4cd792bf532100870c0370c9e1fdbac1a7b3c2634e6b75d415e9   39725  kit/prompt.template.md
7b51116e5e4876c1416066994e282ad077661547ee5fb1dcb0fbcbd59e413cce    4925  kit/read_fidelity.py                 (= ronda 1, 7b51116e)
4823a0b9cfb6db9a61d046eb23c08da93e1319b182d65e07598e6b0a354484be   28199  kit/result.schema.json               (rackcad-a6r2-architect-review/0)
b4b9fd018622651e5f4f0d387a80bb644999ad66e2265772b70a911fc3239298  105536  kit/selftest-post-review.py
fc89d992c35a55263ccb756128b297e0c3fa6adab6361b8eb41f4b5b41b4f702  115228  kit/selftest-result.json
5da1af22ace1b3b6e4c34ede68f327515099d0690c2b3f517bdd92dc897ae67d    1602  kit/transport-gate.json
b05fedd17b9b6856d618d2ffcde88efc1ceafaa3e6c2d5373667919a718129c6   18828  kit/transport_gate.py                (= ronda 1, b05fedd1)
5691baa3aa62fa4e5535d56709c1139b4f0d306ee6f6f673a837646dd19eb377   18286  kit/verify_clone.py
55e8604c83b15ffbec6d9834adb5d1a7798cc50184f23b29f01a4675dc7910c6    9890  prep/adapt_launch.py
0ed8e65bb865b8385e6042d0427954da4e058f92bb65ff9fca2ab3eb4c6f25f4   22154  prep/adapt_post_review.py
c0bf3b0b06a4cf5a28c36c6d1f23eafa28e4364e01b6a6d0e8cabab09402d6ac    2763  prep/file_table.py
d4dfe9f41713f1a0dc3e681d61c1802ad0cea0c6bf830b01906d9a672d4d7cf5   10736  prep/gen_schema.py
ca3e72e5a3feeeaf23b8f3345887f782c14a882dbe93260819443a8b9dacbeb1     210  prep/ids.txt
fefd8937c3b85a980df2d7decb3b2c9eeb4e7760f6c1283683020f8aee260d43   14275  prep/make_synthetic_env.py
f260bb6f248cf49358f013302f9a1aa5a439db26abde7c6907df9ae609cd1c41    8914  prep/phase2_edit_make_closure.py
70ba86563379ac61dfd3cad87ff8eba7df8761ba9c623ab8022491c8d7f5c3e1    3504  prep/phase2_edit_prompt.py
5d67f81927e51fb4cbf000635173819ae14a9d6ac85c8dd11977982f5bae19ee   14424  prep/phase2_edit_readme.py
19c1549c3b18e40231a2abd6b2845328663bac1cce9644e92d053055900bad89    5518  prep/phase2_fill_readme.py
f6f0120ac6963556b5f6f83fd0fd0555b002be89869bc81077912ca8cdc8f3b7    6772  prep/diffs/finish_kit.diff
5523e38c54ef3c30abaac2e18d9b614ae5a43d9b1484d9b60cb96db3079a8f3f    8884  prep/diffs/launch.diff
65dcb274a032cc64a49c01f35bcf70bbf32d5fa68d6e062491217b305c455f38   75244  prep/diffs/make_closure.diff
1fc633cc3465b634c84cf147b476cd58decdb1f9fff5b39270c9d076419444d5    5427  prep/diffs/make_manifest.diff
d035583ca7e2d085c143cffe98103fad9740446c4e1c133094adc646ca530a0e    3721  prep/diffs/mutation-post-review.diff
bc49092b4d9ddf6be9c74bbdbff6abe31ff21aca24905ea7d63d2f6da8f08065   14693  prep/diffs/owner_gate.diff
a074fe4a0280661b534ae6fd032521184d1da2971096db08315178a28744d31f   21289  prep/diffs/post-review.diff
70049cd32d036bee5aabbdef5e045fe7afc7881408c62505c70d98ecab9b3452   10302  prep/diffs/prepare_clone.diff
491ca2ed2dbc4d2146c86325c1d4d9be259f5d099abe5caf89269bcd54447e45   53815  prep/diffs/prompt.template.diff
1e138abfac7d19969a3f007886144c2f5ffa6f434739127148e42e3f6dc06b30   73653  prep/diffs/selftest-post-review.diff
7796552b418ca4dadae8c999730199c5ea764658ff8ef8b6dea26eb08604ee07   17847  prep/diffs/verify_clone.diff
ded711b7cd93af6547b8f458d93469803e1ed87eacc529b6c647bdfe42e51620   50086  prep/phase1/mutation-result.json     (fase 1, sintético)
0373d0c5ce0016b035d5fa0f2c025136be3a2ff6f75da24cfbd2c319178278d6      42  prep/phase1/mutation-times.txt
4c70d7330b031126e01c5764ede160ee60bb2cc39bea1ee6079823091e12556b   13495  prep/phase1/mutation.out
8e10db1d98496b3fbeeeceafee5b19f5fe9143d119a56257d65ad527c7b5de21   70575  prep/phase1/prompt-preview.md        (vista previa sintética)
8a8356c3977f88433008bc0aefa8e5666171b48decb924f79b75d5d12c663999  117410  prep/phase1/selftest-result.json     (fase 1, sintético)
1ecf04a066ae3ba549265cd09685f8685da70e358101ccf25411c6d889ab7961    4099  prep/phase1/synthetic-env.json
21ede54e124019663f8b1983a1b72dcea683778977bfff1a4da06abadfd6feef    1634  prep/phase2/step1.out
fb3af8d153bf8cc617ca9eead3998322ae598bb1adaccae146d5c0ff7ba259b7     794  prep/phase2/step10.summary
873a974ad02dde428f83520c3d1c814ee3b260aecd1242443cd27e5afc5f9374     133  prep/phase2/step2.out
c859968d74aafda9272be2d13a897eb016930e2c6e9c0054c385d51ec2679d22       7  prep/phase2/step3.out
670846eca2776fc28b1337b3228f71e6f9aeceba97656872a14846809da6546c    5347  prep/phase2/step4.out
4866b6aba4e0a48a135ede2fce3090f69275754d851934ef446972817cd02beb     460  prep/phase2/step5.out
11b97efdd6e1c5b579a5429526bc3893dfaeaacec198981001147c3b0da9d331     410  prep/phase2/step6.raw
da7f22456bba514c036ac33e744663291fd9d0e732d01ea35a073a0861191860      42  prep/phase2/step6.times
1acb0b098f5b3b1a6466b3f71690ddd34dd401e29d0b9dec7f3ac654b8562932   15890  prep/phase2/step7.raw
1dd33be7dc1a1aa2334075a1d9c2954a2b68da829080109cab3900c5dda595fa      42  prep/phase2/step7.times
2d197edd3c4ca5dbddadbc877a962797c27c842f7c4af3df68daee1def947821   13312  prep/phase2/step8.raw
50f505dca2a78717d47ecd9965919b7b78171c035d295115c71df07d817b6dea      42  prep/phase2/step8.times
9a44b3c64c9604d8f1cc8b2b035bd920548ba80413b1f8a358b4bd38c880e7f7     144  prep/phase2/step9.out
```

Junto a este README: `launch-dry-run.json` (salida literal de `launch.py --dry-run`, capturada con `-X utf8`) y `finish-kit.log` (traza de los diez pasos). `kit/` es el kit congelado (manifiesto: `kit/kit-manifest.json`). `prep/` es la preparación y no se custodia como kit: `gen_schema.py`, `adapt_post_review.py` y `adapt_launch.py` (derivan esquema, auditor y lanzador), `phase2_edit_*.py` (los cambios de la fase 2 por sustituciones exactas), `phase2_fill_readme.py`, `make_synthetic_env.py` (entorno sintético de la fase 1), `file_table.py` (esta tabla y la comprobación de privacidad: ningún archivo contiene el nombre de usuario ni un correo), `ids.txt`, `diffs/` (cada archivo adaptado frente al de la ronda 1), `phase1/` (autoprueba, mutación y vista previa sintéticas) y `phase2/` (salidas saneadas de cada paso).

## Resultado (añadido tras la corrida, sesión principal; decisiones §82)

```text
Sesión:          d95fcaba-f7db-4408-af85-497e565d114e, lanzador de 2026-10-13T03:21:36Z a 2026-10-13T03:48:35Z (sin run.json: defecto de registro del lanzador, launch-recording-defect.json)
Resultado CLI:   success, is_error False, 1490620 ms
Uso:             67 turnos; herramientas {"Read": 57, "Grep": 8, "StructuredOutput": 1}
Veredicto:       CHANGES REQUIRED
REQUIRED:        2 (A62-A6R2-R1, A62-A6R2-R2)
OPTIONAL:        5
R1-R5:           A62-A6-R1 CORRECTED; A62-A6-R2 PARTIALLY_CORRECTED; A62-A6-R3 CORRECTED; A62-A6-R4 PARTIALLY_CORRECTED; A62-A6-R5 CORRECTED
O1-O3:           A62-A6-O1 INCORPORATED; A62-A6-O2 INCORPORATED; A62-A6-O3 INCORPORATED
G1-G6:           G1 ACCEPTABLE_RESIDUAL (ensayada: partial); G2 ACCEPTABLE_RESIDUAL (ensayada: no); G3 ACCEPTABLE_RESIDUAL (ensayada: partial); G4 ACCEPTABLE_RESIDUAL (ensayada: partial); G5 ACCEPTABLE_RESIDUAL (ensayada: no); G6 ACCEPTABLE_RESIDUAL (ensayada: no)
M-01:            A-6 no, activado no, AGREE
Deltas:          A6-1 AGREED; A6-2 AGREED; A6-3 CHANGES REQUIRED; A6-4 AGREED; A6-5 CHANGES REQUIRED; A6-6 AGREED; A6-7 CHANGES REQUIRED; A6-8 AGREED; A6-9 AGREED
Temas:           Q-A6-01 SATISFIED; Q-A6-02 SATISFIED; Q-A6-06/07 NOT_SATISFIED; Q-A6-12 SATISFIED
Aceptante:       17 MATCHES, 0 DIFFERS
Procedencia:     13 ADEQUATE, 0 INADEQUATE
Acreditación:    NOT_ACCREDITED por el auditor v5.2-a6r2 (1 motivos); sin acreditación excepcional; la acreditación final es del Coordinator
Clon tras corrida: HEAD 0f4d8cbc, estado limpio
```

El veredicto no es el acuerdo: el acuerdo de A-6 es del Architect y del Coordinator sobre el blob exacto, y exige una revisión ACCREDITED, cero
hallazgos REQUIRED y los criterios formales (decisiones §82, punto 5). Ni A-6 AGREED, ni FX-02 PASS, ni F6 GATE PASS.

Archivos añadidos: `output.json` y `audit.json` (byte a byte, sin modificar), `runtime-evidence.json`, `launch/` (saneado),
`launch-dry-run-s82.json` (la prueba en seco sobre `5c1118e5`), `preconditions-s82.json`, `launch-run.log` y `post-review.log`. La transcripción y
`stdout.jsonl` no se versionan; sus SHA-256 están en `runtime-evidence.json`.

**Defecto de registro del lanzador** (`launch-recording-defect.json`): la sesión principal lanzó `python -X utf8 -B kit/launch.py` en vez de la orden
documentada `python -B kit/launch.py`. Después de que terminara el proceso de claude-cli, la consulta a `tasklist` no se pudo decodificar en modo UTF-8
y `launch.py` terminó sin escribir `launch/run.json`. El auditor, ejecutado una vez y sin cambios, da NOT_ACCREDITED con ese único motivo. No se
reconstruye `run.json`, no hay reintento ni acreditación excepcional.
