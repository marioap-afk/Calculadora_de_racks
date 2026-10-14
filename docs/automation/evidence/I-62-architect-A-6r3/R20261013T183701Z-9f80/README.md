# I-62 — Kit de la revisión formal de la ronda 3 de A-6 por un Architect independiente con `claude-cli` (R20261013T183701Z-9f80)

```text
LogicalReviewRequestId: L20261013T183701Z-9f80   InvocationId: I20261013T183701Z-9f80   AttemptSeq: 1   RunId: R20261013T183701Z-9f80
SessionId:      3f6938fa-d6b4-4494-bacb-684ce6287624 (uuid4, generado una vez el 2026-10-13T18:37:01Z con los demás ids; prep/ids.txt)
Autorización:   disposición del Coordinator §84 (cuerpo pegado, 3 377 bytes, SHA-256 8a6b560f1da473ff152732e2e6bf2b82cd60f2df4ddfc7e44f8359edc7bef2e8
                = kit/order.txt = run/order.txt), registrada en decisiones §84 (línea 1666, commit 06521610). Texto de la sección §84 en el commit
                del clon: 17 líneas, 3 880 bytes, SHA-256 4f0530fc0c6424221b678b3dde69e522a24c5df811ab7b6286daf99921f540ef, FIJADO en
                kit/owner_gate.py (AUTH_TEXT_SHA). Puntos 2-7: un único kit, orden congelada, comprobación previa de registro, errores del lanzador,
                compuerta de transporte, revisión acotada, lanzamiento. Sin acreditación excepcional; sin reintento; tope de 3 600 s
Consumo:        cubierto por CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62, solo lectura), la misma base que las rondas 1 y 2
Objeto:         docs/initiatives/I-62-A-6.md, ronda 3, PROPUESTA, blob e62f27b8b1329f1b9cfa69a707feb31d22bad1d1 (135 665 bytes, 1 320 líneas),
                publicado en a128efef8f83890f8deae66c286c553bed2663e3 (padre 9cd074aa, con la ronda 2, 86da0ae7); guardas `run` PASS custodiadas
                en 9a30c9a6503f4e48d22cd9f2486980eb56ad0030; copias legibles de r3/ en 9b8b66a2
Commit del clon: 9b8b66a21050fe264a4ff4802295468a91fce5c7 (HEAD de la rama; contiene la publicación, la custodia de las guardas, §84 y r3/readable/)
Ronda 2:        blob 86da0ae770e683a647f2724860a4bd890061154d, en el run como a6-r2.md (kit/a6-r2.md, 110 243 bytes, 1 145 líneas, extraído con
                `git cat-file blob` del worktree, solo lectura); salida de la revisión R20261012T195511Z-e5c3 en el cierre (output.json, fc357d09)
Clon / run:     D:\r62-arch-a6r3 (HEAD separado en 9b8b66a2, sin ramas ni remoto; clonado desde el directorio común de git) y D:\r62-arch-a6r3-run
                (order.txt = day3\s84_body.txt, prompt.md = kit/prompt.md, a6-r2.md = blob 86da0ae7)
Transporte:     el de las rondas 1 y 2 sin cambio: claude-cli 2.1.293, %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256
                8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa; claude-opus-5-5, effort xhigh; Read, Grep y Glob (+ StructuredOutput);
                --safe-mode, sin MCP; --add-dir del run; prompt por stdin; resultado por --json-schema; tope de 3 600 s; espera de 120 s tras
                `auth status`; ArgsTemplate idéntica a la de las rondas 1 y 2
Orden:          EXACTAMENTE `python -B kit/launch.py`, desde la carpeta que contiene kit/ (esta), sin flags ni variables añadidos (decisiones §84,
                punto 2); la guarda de launch.py rechaza cualquier otra
Compuerta D-01: MEASURED sobre R20261008T183600Z-char (claude-cli-characterization.json, SHA-256 20d0de2c…, blob 541956eb), fijada sobre la
                copia del clon D:\r62-arch-a6r3; predicados de transport_gate.py byte a byte los de las rondas anteriores
Compuerta 11:   11-OwnerAndAuthorizationGate (kit/owner_gate.py), BLOQUEANTE: línea del Owner de §78; CLAUDE-CLI-I62 = A y el mismo binario; A-6
                en la HEAD = e62f27b8; decisiones §84 con el encabezado fijado, el blob completo como token literal y el SHA-256 de su texto fijado;
                decisiones solo por añadido desde el clon; la HEAD desciende del clon; y BLOQUEO si existe cualquier sección numerada después de §84
Estado:         kit preparado, probado y CONGELADO (kit/kit-manifest.json, SHA-256 771d9f4629a2b82a42c0ff98db08c52da5e297bf7a0c899918cbdced864ea918); sonda de registro en Ok; prueba en seco con
                BlockingChecks = [] y LaterSections = []. NO lanzado: ningún claude.exe ni CLI de modelos ejecutado; ni red ni credenciales.
                Pendiente: custodia por la sesión principal y el lanzamiento único (§84, punto 7)
```

## Qué pide §84 y dónde lo cubre el kit

| Exigencia (orden del kit y `order.txt`) | Dónde |
|---|---|
| §84.2: reutilizar el kit anterior y corregir solo los defectos demostrados (lanzamiento con flags añadidos; `run.json` no escrito tras la corrida; decodificación de Windows) | Auditor, lanzador, compuerta de transporte y compuerta 11 derivados por sustituciones exactas (`prep/adapt_*.py`, `prep/subst.py`), con su diff en `prep/diffs/`; `c3_analysis.py` y `read_fidelity.py` byte a byte; el resto, adaptado con su diff frente a la ronda 2 (`prep/make_diffs.py`). Las reglas del auditor de lanzamiento heredado, D-01, stream-json, transcripción, herramientas, fidelidad, referencias, lectura directa, custodia y clon quedan byte a byte (diff `post-review.diff`) |
| §84.2: lanzar exactamente `python -B kit/launch.py`; registrar el entorno | `kit/launch.py` (de la plantilla custodiada `r3/launchfix/launch.r3-template.py`, guarda G): `FROZEN_COMMAND`; se ejecuta desde esta carpeta; `environment_record()` → `preflight.json` (y salida de `--dry-run`): nombres de las variables PYTHON*, GIT_*, CLAUDE* y ANTHROPIC*, valores solo de las PYTHON*; el lanzador no pone ni quita ninguna (Popen sin `env`) |
| §84.3: comprobación previa de registro con un proceso sustituto, en seco y como precondición del lanzamiento real | `launch.py --record-probe` y la sonda P dentro de `--dry-run` y del lanzamiento real, ANTES de la preflight (si falla: código 2, nada escrito, el binario no se ejecuta). Sin modelo, sin `run.json` de la revisión, sin escribir en el clon ni en el run; temporal fuera del run, del clon y del kit, borrado |
| §84.4: evidencia inequívoca de ejecución y custodia | `run.json` con `Outcome` ∈ {COMPLETED, REVIEW_PROCESS_FAILURE, TRANSPORT_FAILURE, POST_RUN_RECORDING_FAILURE, LAUNCH_FAILURE} y `OutcomeCategories`, escrito siempre una vez lanzado el hijo (plantilla R + `set_outcome`); el auditor v5.3 exige COMPLETED, la coherencia de `Outcome` con los campos (lo recalcula con su propia función), `PostRunErrors` = [], sin `LaunchError` ni `Aborted` y todos los campos; informa por separado la categoría de ejecución y el veredicto del Architect (`Categories`); ningún registro se reconstruye |
| §84.5: codificación de Windows con estrategia explícita y pruebas negativas; sin tocar el binario | `transport_gate.decode_cli_output` (UTF-8 estricto; BOM UTF-8 quitado y registrado; nunca consola ni locale; falla cerrado con extracto hexadecimal, sin contexto en `auth status`); `cli_version` y `auth_status` en bytes. Casos G41-G49 (función pura: cp850 «INFORMACIÓN», UTF-8 inválido, UTF-16 con BOM, vacía, no bytes, BOM UTF-8, UTF-8 no ASCII) y G50-G58 (ejecutables sustitutos `.bat`). D-01 sin cambio |
| §84.6: revisión independiente y acotada (nueve puntos; precondiciones de sesión real con fallo cerrado) | `kit/prompt.template.md` → `kit/prompt.md`; esquema `rackcad-a6r3-architect-review/0` (`prep/gen_schema.py`): `Round2FindingResolution`, `NegativeCaseCoherence`, `ReconstructionFromRealEvents`, `EffectiveInputClosure`, `AcceptorCodeContrast` (nueve afirmaciones), `OwnerAuthorizationAndVerifyScope`, `GapAssessment`, `Round2OptionalDisposition`, `RealSessionPreconditions` (ocho, KEPT_FAIL_CLOSED / TREATED_AS_MET ⇒ REQUIRED); auditor v5.3 (coherencia) |
| §84.7: identidad del binario, integridad del clon, manifiesto y fuentes, objeto exacto, vigencia de las autorizaciones, prueba en seco con cero bloqueos, UNA revisión | preflight 1-11 de `launch.py` (11 = §84 y nada después); `verify_clone.py` (AllChecks con las guardas de la ronda 3 reproducidas); `kit-manifest.json` (congelación); `launch-dry-run.json` con `BlockingChecks` = [] |
| §84.8: auditor sobre la corrida completa, resultados sin modificación | orden exacta del auditor abajo |
| §84.9: sin Codex, FX-U2, B1, I-65, ADR-0046 #9 ni D.5 | ningún modelo ni CLI de modelos ejecutado; `docs/automation/evidence/I-62-C1C3/` (C1-C3 y la nota de I-65) fuera del cierre |

## Independencia del preparador

Quien preparó este kit es un subagente de la sesión principal (la sesión que preparó A-6 y sus tres rondas). Leyó la orden (§84), decisiones §78-§84,
el kit de la ronda 2 entero (README y componentes), la salida de la revisión de la ronda 2 (hallazgos y valoración de G1-G6), el README y la
plantilla de `launchfix/`, y de la A-6 de la ronda 3 la cabecera, §3.6 (A6-3), §3.8 (A6-5), §3.10 (A6-7) y de §3.12 (G1-G6) al final. No leyó
`bmin_accept.py` (las afirmaciones del contraste se nombran por lo que A-6 enuncia y por las líneas que lista G11). El prompt no contiene veredicto
esperado ni valoraciones de la preparación; la evidencia de I-62 (`I-62-evidence.md`) está en el cierre, como en la ronda 2, presentada como
evidencia y no como autoridad. El kit no ejecuta el paquete B-min ni las pruebas del aceptante; solo reproduce las guardas de la ronda 3.

## Contrato y auditor

- **Cierre** (`kit/make_closure.py`, en el commit del clon; cada archivo por su blob; rangos calculados desde encabezados, filas y líneas citadas):
  A-6 de la ronda 3 entera (mapa de secciones; tablas de §3.2, §3.3, §5, §3.16 y las dos de §3.17 —correcciones y precondiciones de R2— con sus
  filas R1 (a)-(d), R2 (a)-(b), O1-O5 y las cinco precondiciones; los párrafos de §3.17 sobre precondiciones, límites de la reconstrucción, G1-G6 y
  lanzamiento; bloques G1-G6; reglas «> n.» de cada delta; líneas del Owner y párrafo «Ronda 3» de §7; filas de §8 con Q-A6R2-01..06 y
  Q-A6R3-01..04); `docs/automation/evidence/I-62-A6/r3/` entero (con `r3/readable/` y su MANIFEST comprobado); `r2/` entero (como en la ronda 2,
  con `r2/readable/` y su MANIFEST comprobado; árbol 411949b0 fijado); el paquete B-min entero (árbol 3caa0dfa); el dictamen de B-min (output.json y
  README.md); las salidas de las revisiones de las rondas 1 (e90d685b, rangos por A62-A6-R1..R5 y O1..O3) y 2 (fc357d09, rangos por A62-A6R2-R1, R2
  y O1..O5, y sus miembros GapAssessment y AcceptorCodeContrast); la base normativa de la ronda 2 (V14, AUTOMATION_PLAN, ADR-0046, ADR-0048,
  LIFECYCLE, A-1..A-5, agent-execution README con §10 L219-L228, routing.md, adapters/codex-cli.md, cinco esquemas, controller-contracts.md con
  L44-L45, decisiones con §43, §57, §61, §64 y §73 en adelante, filas de §83 y §84 y sus líneas de bytes y SHA-256); toda fuente de G11 del resultado
  `run` custodiado de la ronda 3 (comprobado igual a `r3/a6r3-sources-for-reviewer.json`), con sus rangos, incluida `I-62-evidence.md` (§140 en
  adelante, con §161-§165; evidencia y no autoridad, como en la ronda 2; prefijo comprobado del blob que fija G11); run: `order.txt` y `a6-r2.md`
  (premisas) y `prompt.md`. **Excluidos:** `docs/automation/evidence/I-62-C1C3/` (C1-C3 y la nota de aislamiento de I-65) y todo material de I-65,
  la evidencia de la ronda 1 fuera de `r2/` y `r3/`, el resto de los directorios de las revisiones de las rondas 1 y 2, las rutas que A-6 solo nombra
  como superficies, `requests/`, transcripciones, memoria, sondas, WORKFLOW, mapa de cláusulas, estado y fixture. Comprobaciones de identidad: el
  padre de la publicación tiene `86da0ae7`; la publicación, la custodia de las guardas y el clon, `e62f27b8`; `r3/` sin cambio tras la publicación
  salvo el resultado `run` y `readable/`; el resultado `run` = Mode RUN, Base = padre, Head = publicación, PASS; la autoprueba = PASS con
  A6TextBlob, ToolBlob (103d69c6) y 694/694; decisiones crece por añadido desde `be5e376f` (06521610, §84 registrada) y su §84 tiene el encabezado,
  el blob y el SHA-256 de texto que fija `owner_gate.py`; las líneas del Owner una vez en A-6 y en decisiones; los blobs de G11 iguales en el clon
  (salvo decisiones y la evidencia, que crecen por añadido desde los de G11); A-6 §5 «Activado» = M-01 no, M-02..M-05 sí, M-06 y M-07 no, M-08 sí.
- **Herramientas y lectura:** las de las rondas 1 y 2, sin cambio (solo Read, Grep y Glob, y StructuredOutput; archivos de más de 60 000 bytes por
  tramos de 300 líneas; Grep solo sobre archivos; Glob solo dentro del cierre; toda denegación es violación; regla de lectura: toda referencia solo a
  líneas leídas con Read en ese mismo archivo; una cita dentro de A-6 no es una lectura de la fuente; lectura directa obligatoria de V14 L2333-L2334).
- **Prompt** (`kit/prompt.template.md` → `kit/prompt.md`): neutral (sin veredicto esperado); explica el objeto de la ronda 3, los nueve puntos de
  §84, punto 6, la regla de §84 sobre lo que depende de una sesión real (se juzga como precondición con fallo cerrado, sin darlo por acreditado),
  que los demás aspectos de A-6 no se piden pero cualquier defecto material puede señalarse, la regla de lectura, la lista G11, que `a6-r2.md` es el
  blob `86da0ae7` y que la compuerta 11 comprobó §84 antes del lanzamiento. 102 671 bytes, 688 líneas, SHA-256 `fa6f011e…`.
- **Esquema** (`kit/result.schema.json`, `rackcad-a6r3-architect-review/0`; 12 289 caracteres ASCII en forma compacta; `prep/gen_schema.py`): solo
  las palabras clave ya medidas con claude-cli 2.1.293 (type, additionalProperties, required, properties, enum, pattern, minLength, minItems, items).
  Campos: identidad, Step0 (siete booleanas: V14 directa, orden entera, §84 declara bytes y SHA-256, §83 dispone la ronda 2, resultado `run`,
  A6TextBlob, salida de la ronda 2), DirectReads, Reads, hallazgos nuevos `A62-A6R3-R<n>` / `A62-A6R3-O<n>`, `Round2FindingResolution`,
  `NegativeCaseCoherence` (N4, NC1, VERIFY_WITHOUT_INSTRUMENT), `ReconstructionFromRealEvents` y `EffectiveInputClosure`
  (ADEQUATE_AS_FAIL_CLOSED_PRECONDITION | INADEQUATE), `AcceptorCodeContrast` (CHECK_IDS, WAIVABLE_SET, DECISION_FORMULA, EXIT_CODES,
  NOT_APPLICABLE_RULES, R4_VERIFICATION_CONDITION, R4_NOT_RUN_AS_NON_PASS, R4_AUTHORITY_MAP_RULE, R3_R9_WITHOUT_RUNS; con A6Refs y AcceptorRefs),
  `OwnerAuthorizationAndVerifyScope` (LineA, LineB, VerifyOnly, PinnedBlobsUnchanged, NoResultAcceptedInAdvance), `GapAssessment` (G1-G6),
  `Round2OptionalDisposition` (O1-O5: INCORPORATED | PARTLY_INCORPORATED | DECLINED_WITH_REASON | NOT_ADDRESSED), `RealSessionPreconditions`
  (RUNS_RECONSTRUCTION_FROM_REAL_LOG, RUNS_COMPARISON_ON_REAL_RUNS, IDENTITY_AT_USE_IN_REAL_SHELL, GIT_BY_NAME_IS_DECLARED_GIT,
  CLOSURE_COVERAGE_R9_ON_REAL_RUNS, ACCEPTOR_RUN_ON_MEASUREMENT_LOG, COORDINATOR_CLOSURE_AND_RECONSTRUCTION_RULE_DISPOSITION,
  NORMATIVE_READINGS_DISPOSITION_AND_PRETEST: KEPT_FAIL_CLOSED | TREATED_AS_MET), OwnerAuthorityCheck, IfAgreed (once campos), KnownLimitations,
  RecommendedNextAction. Veredictos: AGREED, CHANGES REQUIRED, BLOCKED — OWNER DECISION, NOT_ACCREDITED.
- **Compuerta de transporte** (`kit/transport_gate.py`; diff `prep/diffs/transport_gate.diff`): la compuerta D-01 (predicados, plantilla,
  `evaluate`, registros, `measure_binary`, CLI) byte a byte; nuevas `decode_cli_output` y `run_bytes`; `cli_version` y `auth_status` en bytes y UTF-8
  estricto (fallo cerrado con extracto hexadecimal; en `auth status`, sin contexto). `kit/c3_analysis.py` byte a byte.
- **Compuerta 11** (`kit/owner_gate.py`; diff `prep/diffs/owner_gate.diff`; comprobación 11 de `launch.py`, BLOQUEANTE, evaluada también en seco porque
  solo lee git, con GIT_OPTIONAL_LOCKS=0): (a) la línea del Owner `A6-ADR0046-9-FX02 = A (…)` (SHA-256 `83152eae…`) como línea completa después de
  `## 78.`, ninguna otra «A6-ADR0046-9-FX02 =» y la misma línea en la A-6 de la ronda 3 del commit del clon; (b) `CLAUDE-CLI-I62 = A` (SHA-256
  `fbeade41…`) como línea completa, ninguna otra «CLAUDE-CLI-I62 =», y el SHA-256 medido del binario = el del cierre; (c) A-6 en la HEAD = `e62f27b8`
  (distinto de los blobs de las rondas 1 y 2); (d) **autorización**: exactamente una sección «## 84. », con el encabezado «## 84. Disposición del
  Coordinator: A-6 ronda 3, autorización de la revisión formal (2026-10-13 UTC)», el blob completo como token literal y el SHA-256 de su texto =
  `4f0530fc…` (fijado al congelar); (e) **ninguna sección numerada después de §84** (si la hay, bloquea y la lista en `LaterSections`); además,
  decisiones del árbol de trabajo = la de la HEAD, la HEAD desciende del commit del clon y las decisiones del clon son prefijo exacto de las de la HEAD.
  El registro lista `AuthorizationSections` y `LaterSections`.
- **Auditor v5.3-a6r3** (`kit/post-review.py`, derivado de v5.2-a6r2 con `prep/adapt_post_review.py` por diez sustituciones exactas y la docstring;
  diff `prep/diffs/post-review.diff`): las reglas de lanzamiento heredadas, compuerta D-01, stream-json, transcripción, herramientas, lectura,
  fidelidad, referencias, lectura directa de V14, referencias del contraste con el aceptante, custodia del run y clon quedan byte a byte. Nuevo:
  `run.json` (Outcome = COMPLETED; Outcome y OutcomeCategories = los que el auditor recalcula; PostRunErrors = []; sin LaunchError ni Aborted; los 29
  campos de RUN_JSON_FIELDS); `preflight.json` (compuerta 11 de la ronda 3 con §84 y sin sección posterior; FrozenCommandGuard en Ok, aplicada y con
  `python -B kit/launch.py`; RecordProbe en Ok; Environment con los prefijos fijados, nombres y valores solo de PYTHON*); `Categories` (categoría de
  ejecución, o RUN_JSON_MISSING / OUTCOME_MISSING, y veredicto del Architect, por separado); Step0 de la ronda 3; `collect_refs` y `coherence()` del
  esquema nuevo (unicidad y cobertura de los seis arreglos; no CORRECTED ⇒ REQUIRED; NOT_ADDRESSED ⇒ hallazgo; brechas como en la ronda 2;
  INCOHERENT, INADEQUATE, DIFFERS y TREATED_AS_MET ⇒ REQUIRED, y sus opuestos sin REQUIRED; línea excedida o NOT_CONFIRMED ⇒ hallazgo; ClauseRefs
  en cada elemento; ningún NOT_DETERMINED con veredicto; todo REQUIRED citado por un elemento; AGREED con todo lo exigido).
- **Lanzador** (`kit/launch.py`, derivado de la plantilla custodiada `r3/launchfix/launch.r3-template.py` (SHA-256 `4534c63b…`, leída del clon) con
  `prep/adapt_launch.py` por veinte sustituciones exactas y la docstring; diff `prep/diffs/launch.diff` frente a la plantilla): G (guarda de la orden
  congelada, antes de cargar nada), R (registro a prueba de fallos: tasklist en bytes, PostRunErrors, `run.json` siempre una vez lanzado el hijo), P
  (`--record-probe`) y D (sonda en seco y antes de la preflight real) byte a byte; constantes de la ronda 3; comprobación 2 con `LaunchCommand`;
  comprobación 4 con la versión decodificada; `environment_record()`; `outcome_categories()` / `set_outcome()`; salida de `--dry-run` con
  `LaterSections` y `Environment`. No reintenta. `--dry-run` y `--record-probe` no ejecutan el binario.
- **Ayudantes:** `prepare_clone.py` (clon desde el directorio común de git y run; `kit-params.json`), `make_closure.py`, `verify_clone.py` (con la
  reproducción de las guardas de la ronda 3: `run` 9cd074aa..a128efef y `self-test`, ambas enteras iguales a las custodiadas; el worktree, sin cambio
  de HEAD ni de estado), `make_manifest.py`, `finish_kit.py` (pasos 10 y 11 con la orden congelada exacta), `mutation-post-review.py`;
  `read_fidelity.py` sin cambio.

## Preparación ejecutada (2026-10-13/14): resultados

Pasos de `kit/finish_kit.py`, ejecutados uno a uno con sus mismos comandos para revisar cada resultado; salidas saneadas en `prep/phase/`:

1. `prepare_clone.py 9b8b66a2… a128efef… 9a30c9a6…` → `D:\r62-arch-a6r3` (`git clone --no-local -c core.autocrlf=false --single-branch --branch
   architecture/portabilidad-coordinador-principal <directorio común de git> D:\r62-arch-a6r3`; HEAD separado en `9b8b66a2`; sin ramas ni remoto;
   reflog y gc) y `D:\r62-arch-a6r3-run` (order.txt, a6-r2.md); `kit/kit-params.json` (padre de la publicación `9cd074aa…`; añadidos a `r3/` tras la
   publicación: el resultado `run` y los 10 archivos de `readable/`). Precondiciones: todas en true (`prep/phase/step1.out`).
2. `make_closure.py --render-prompt` → `kit/prompt.md` (102 671 bytes, 688 líneas, SHA-256 `fa6f011e…`); 3. copiado al run, idéntico.
4. `make_closure.py` → `kit/closure.json` (SHA-256 `9c124407…`): **282 archivos, 9 612 179 bytes, 174 912 líneas** (r3: 67 archivos, 2 434 503 bytes,
   50 142 líneas; r2: 159, 4 488 477, 90 921; paquete B-min: 30, 541 797, 10 175; el resto, 26 archivos, 2 147 402 bytes, 23 674 líneas, con la
   evidencia de I-62: 378 315 bytes, 4 320 líneas, blob `f01344fe`, prefijo comprobado del que fija G11). **Ningún archivo no UTF-8.** 41 archivos de
   más de 60 000 bytes (por tramos de 300 líneas). **47 archivos con líneas de más de 2 000 caracteres:** 36 con copia legible (27 de `r2/readable/` y
   los 9 resultados JSON de `r3/`, cubiertos por `r3/readable/`), y 11 sin copia (lista en «Supuestos no medidos», punto 5). MANIFEST de
   `r2/readable/` (27 copias) y de `r3/readable/` (9 copias) comprobados: blob canónico, SHA-256 y líneas de 1 900 caracteres como máximo; cada
   `readable/` contiene exactamente su MANIFEST y sus copias. C1-C3 e I-65 fuera.
5. Compuerta D-01: `transport_gate.py init` + `set MEASURED` sobre `D:\r62-arch-a6r3\…\claude-cli-characterization.json` (SHA-256 `20d0de2c…`) →
   `kit/transport-gate.json` en MEASURED (SHA-256 `e9adefa9…`).
6. `verify_clone.py` → **AllChecks = true** (2026-10-13T19:06:09Z-19:10:42Z; `kit/clone-verification.json`, SHA-256 `1e9f3c83…`): HEAD, separado, sin
   ramas ni remoto, limpio, 4 196 archivos en el árbol, sin enlaces, blobs del cierre, historia del objeto, commits, run, caracterización; guardas de
   la ronda 3 reproducidas desde el clon contra el worktree (solo lectura): **`run` 9cd074aa..a128efef PASS, igual entera a la custodiada** y
   **`self-test` 694/694 mutantes muertos, igual entera a la custodiada**; worktree sin cambio de HEAD ni de estado.
7. Autoprueba sobre el clon y el run REALES (`kit/selftest-result.json`, SHA-256 `9856ebfe…`): **274/274 PASS** — 177 casos de auditoría, 61 de la
   compuerta de transporte (G00-G40 de D-01 y G41-G58 de la decodificación, con 9 ejecutables sustitutos), 32 de la compuerta 11 (O00-O28 y la sonda
   L00) y 4 de coherencia lanzador-auditor (X01-X04) —, con el clon limpio y los tres archivos del run idénticos a sus copias al terminar;
   2026-10-13T20:20:29Z-22:02:10Z (102 min; cada caso relee por git los 282 blobs designados). **L00** (worktree en `9b8b66a2`): compuerta 11 en Ok,
   sin problemas, `AuthorizationSections` = [§84, línea 1666, encabezado fijado, blob, SHA-256 `4f0530fc…`], `LaterSections` = [].
8. Mutación sobre el clon real (`kit/mutation-result.json`): **183/183 mutantes muertos**, base 273/273 (sin L00): `post-review.py` 127/127 (`b21cb940…`), `transport_gate.py` 40/40 (`e326b5b9…`), `owner_gate.py` 16/16 (`1fff7c1f…`); 2026-10-14T00:41:01Z → 2026-10-14T04:03:51Z; SHA-256 `90940f38…`. 3 de ellos (los sitios L289, L292, L298 de `decode_cli_output`: salida vacía, BOM UTF-16 y UTF-8 inválido) muertos por excepción: con el sitio desactivado, `decode_cli_output` devuelve Ok sin texto y `cli_version` lanza AttributeError en los casos de los ejecutables sustitutos (los casos puros G44-G47, anteriores, también fallan, pero la excepción aborta la pasada del mutante y el arnés solo registra la excepción). Intento 1 (2026-10-13T22:02:23Z → 2026-10-14T00:39:23Z): el arnés heredado de la ronda 2 no capturaba la excepción de un mutante y abortó en el mutante 165 (`transport_gate.py` L289) después de 164 mutantes, todos muertos; se corrigió el arnés (`prep/adapt_misc.py`, sustituciones M4-M5: una excepción de un mutante cuenta como muerte y queda en `KilledBy`; la base sigue sin capturar) y se repitió la mutación entera (registro del intento 1 en `prep/phase/step8-attempt1.raw`). La autoprueba no cambió
9. `make_manifest.py` → `kit/kit-manifest.json` (congelación): 24 archivos de `kit/` y los tres del run; **SHA-256 `771d9f4629a2b82a42c0ff98db08c52da5e297bf7a0c899918cbdced864ea918`**
10. `python -B kit/launch.py --record-probe` (desde esta carpeta, orden congelada exacta) → código 0: `RecordProbe.Ok` y `FrozenCommandGuard.Ok` en true (argv ['-B', 'kit/launch.py', '--record-probe']); registro sustituto con Outcome COMPLETED; `tasklist` con 89 bytes no ASCII (consola cp850) tratado en bytes; `ProcessGoneAfter` true; temporal borrado; sin `<run>/launch`; listado del kit sin cambios. `record-probe.json` (UTF-8, SHA-256 `79837ec8…`; bytes crudos en `prep/phase/step10.raw`)
11. `python -B kit/launch.py --dry-run` (ídem) → código 2 (siempre en seco): `BlockingChecks` = []; `NotRunInDryRun` = ['4-Version', '5-Auth']; `FrozenCommandGuard.Ok` (argv ['-B', 'kit/launch.py', '--dry-run']); `RecordProbeOk` = True; `AuthorizationSections` = [§84, línea 1666, encabezado fijado True, HasBlob True, TextSha256 `4f0530fc…`]; `LaterSections` = []; comprobación 9: 14359 caracteres; compuerta 11: Ok, líneas del Owner A6-ADR0046-9-FX02 [1572] (§78 en la línea 1564) y CLAUDE-CLI-I62 [1085]; A-6 en la HEAD = fijado: True; decisiones por añadido: True; worktree limpio: True; desciende del clon: True; binario = cierre: True; `Environment`: 33 nombres (valores solo de las PYTHON*: PYTHONDONTWRITEBYTECODE, PYTHONUSERBASE), el lanzador no pone ni quita ninguna. `launch-dry-run.json` (UTF-8, SHA-256 `10e3923b…`; bytes crudos en `prep/phase/step11.raw`, codificación de la tubería cp1252)

**Duración estimada frente al tope de 3 600 s.** La ronda 2 duró unos 1 620 s de lanzamiento a fin (03:21:36Z-03:48:35Z, con la preflight y la
espera de 120 s), con 66 llamadas, 67 turnos y 6 099 líneas entregadas de 26 archivos. La ronda 3 es acotada (nueve puntos) pero exige como mínimo:
V14 directa, `order.txt`, decisiones §83-§84, el resultado `run` (1 446 líneas, una lectura), la autoprueba (26 líneas), la salida de la ronda 2
(cabecera y siete hallazgos, unas 520 líneas), las partes cambiadas de A-6 (§3.6, §3.8, §3.10, §3.12, §3.17, §7, §8: unas 700 líneas en 3-5
tramos), los tramos de `a6-r2.md` que citan los hallazgos, las líneas del aceptante para las nueve afirmaciones (las de G11: unas 650 líneas, hasta
2 816 si lo lee entero, 10 tramos) y las copias legibles de `r3/readable/r1probes/` y `r2static/` que use (hasta unas 5 700 líneas). Del orden de
70-130 llamadas y 7 000-14 000 líneas: **≈ 1 500-3 000 s**. El tope puede alcanzarse si el revisor lee enteros el aceptante y las copias legibles
grandes. Ampliar el tope cambiaría `TimeoutSeconds` del cierre y de `launch.py` (y el kit custodiado): no se ha hecho (§84, punto 7: 3 600 s).

## Condiciones exactas para el lanzamiento (nada lanzado)

1. Custodia del kit por la sesión principal tal como queda (manifiesto SHA-256 arriba). Si cambia cualquier archivo de `kit/`, `make_manifest.py`
   (o `--gate` tras un `transport_gate.py set`) y volver a custodiar; ningún archivo del kit se toca después de la congelación.
2. Que la rama siga descendiendo de `9b8b66a2…`, que decisiones crezca solo por añadido desde el clon y **sin ninguna sección posterior a §84** (si
   el Coordinator o el Owner registran otra disposición, la compuerta 11 bloquea hasta revisarla), que A-6 en la HEAD siga siendo `e62f27b8…`, que
   la línea del Owner de §78 y la de CLAUDE-CLI-I62 = A sigan como líneas completas únicas y que el binario siga siendo el de SHA-256 `8693c4a0…`.
3. En la MISMA herramienta, shell y entorno que se usarán para lanzar (la página de códigos decide los bytes de `tasklist`), desde esta carpeta
   (la que contiene `kit/`), sin flags ni variables añadidos:
   - `python -B kit/launch.py --record-probe` → código 0, `RecordProbe.Ok` y `FrozenCommandGuard.Ok` en true;
   - `python -B kit/launch.py --dry-run` → código 2, `BlockingChecks` = [], `FrozenCommandGuard.Ok`, `RecordProbeOk` = true,
     `AuthorizationSections` = la §84 (encabezado fijado, `HasBlob`, `TextSha256Matches`), `LaterSections` = []; 4 y 5 solo se ejecutan en el
     lanzamiento real.
4. Lanzar UNA vez, con la orden congelada exacta (decisiones §84, punto 2), desde esta carpeta:

   ```text
   python -B kit/launch.py
   ```

   Sin reintento. Ejecuta G, la sonda P (si falla: STOP, código 2, nada escrito), la preflight (si falla: código 2), `--version` y `auth status` (en
   bytes, UTF-8 estricto), espera 120 s, vuelve a medir el binario y lanza; `run.json` se escribe siempre una vez lanzado el hijo, con `Outcome`.
5. Al terminar, el auditor sobre la corrida completa, desde esta carpeta y sin flags más allá de los documentados (los resultados se conservan sin
   modificación):

   ```text
   python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a6r3\3f6938fa-d6b4-4494-bacb-684ce6287624.jsonl" D:\r62-arch-a6r3-run\launch\stdout.jsonl D:\r62-arch-a6r3 D:\r62-arch-a6r3-run kit kit/result.schema.json audit.json output.json
   ```

   El auditor informa por separado `Categories.Execution` (el `Outcome` de `run.json`, o RUN_JSON_MISSING / OUTCOME_MISSING) y
   `Categories.ArchitectVerdict`; cualquier defecto de integridad es un motivo y deja NOT_ACCREDITED. El veredicto no es el acuerdo: el acuerdo es
   del Architect y del Coordinator (LIFECYCLE §6) sobre el blob exacto, con una revisión ACCREDITED y una disposición posterior del Coordinator
   (§84, punto 8). Ni A-6 AGREED, ni FX-02 PASS, ni F6 GATE PASS se declaran aquí.

## Desviaciones frente a la ronda 2, con su motivo

- **Autorización (compuerta 11):** en la ronda 2 la autorización era una disposición posterior al kit que nombrara el blob y el RunId; en la ronda 3
  la autorización (§84) es anterior al kit y está en el commit del clon, así que la compuerta fija su encabezado y el SHA-256 de su texto al
  congelar, exige el blob completo como token y **bloquea ante cualquier sección posterior a §84** (orden del kit, punto 6). El RunId sigue en el
  registro, pero no se busca en decisiones (§84 no lo nombra). Se retira la comprobación de un único §80.
- **Lanzador:** derivado de la plantilla custodiada de `r3/launchfix/` (no del `launch.py` de la ronda 2 directamente), que ya trae G, R, P y D; se
  añaden `Outcome`/`OutcomeCategories` (§84, punto 4), el registro del entorno (§84, punto 2), la versión decodificada (§84, punto 5) y
  `LaunchCommand` en la comprobación 2. El código de salida del lanzamiento real pasa a ser 0 solo con Outcome COMPLETED (antes: código 0, mensaje
  result, sin PostRunErrors ni LaunchError; ahora también sin Killed/TimedOut y sin TransportFailure).
- **Compuerta de transporte:** `cli_version` devuelve un registro (`Version`, `ReturnCode`, `Decoding`) en lugar de una cadena; `auth_status` añade
  `ReturnCode` y `Decoding` (y `DecodeError` si no decodifica) y trata un JSON que no es objeto como ParseError. D-01, sin cambio.
- **Auditor:** las reglas nuevas de `run.json` y de `preflight.json` y la coherencia del esquema acotado; se retiran las reglas de los campos que el
  esquema ya no tiene (temas, deltas, materialidad, separabilidad, compatibilidad, superficies, D.5, procedencia, M-01); `Categories` en la salida.
- **Esquema y prompt acotados** a los nueve puntos de §84, punto 6 (la ronda 2 cubría la amplitud de §80, punto 6): los aspectos que la ronda 3 no
  cambia no tienen campo; el prompt permite señalar cualquier defecto material como hallazgo.
- **Cierre:** además del de la ronda 2, `r3/` entero (con `readable/`) y la salida de la revisión de la ronda 2; el árbol de `r2/` (411949b0) y el
  directorio de la revisión de la ronda 2 (acf2fa30) fijados; la premisa del run es `a6-r2.md` (en lugar de `a6-r1.md`); el MANIFEST de cada
  `readable/` debe listar exactamente sus archivos; G11 se toma del resultado `run` de la ronda 3 y se comprueba igual a
  `a6r3-sources-for-reviewer.json`; ningún material de I-65.
- **Clon:** desde el directorio común de git del worktree (orden del kit), con los mismos pasos (rama única, HEAD separado, sin ramas ni remoto,
  reflog y gc).
- **Sin entorno sintético:** el commit del clon existía desde el principio; la autoprueba y la mutación se ejecutaron directamente sobre el clon y el
  run reales (en la ronda 2, fase 1 sintética y fase 2 real).
- **Autoprueba:** los `run.json` sintéticos llevan el `Outcome` que calcula la propia `outcome_categories` de `launch.py` (leída con `ast`, sin
  importar `launch.py`), así que cada caso comprueba también que el auditor y el lanzador coinciden; casos X01-X04 de coherencia lanzador-auditor;
  ejecutables sustitutos `.bat` para la decodificación; L00 ahora debe PASAR la compuerta 11.
- **Arnés de mutación:** una excepción provocada por un mutante cuenta como muerte (registrada en `KilledBy` y en `Totals.KilledByException`); en la
  ronda 2 ningún mutante lanzaba y el arnés no lo preveía (intento 1 abortado; ver el paso 8). La base sigue sin capturar excepciones.
- **`verify_clone.py`:** reproduce las guardas de la ronda 3 y comprueba además que el estado (`status --porcelain`) del worktree no cambia.
- **`finish_kit.py`:** los pasos 10 y 11 usan la orden congelada exacta desde la carpeta que contiene `kit/` (en la ronda 2, `sys.executable -X utf8
  -B launch.py --dry-run`, que la guarda rechazaría). Los demás pasos (ayudantes de preparación) siguen con `-X utf8`; no son el lanzamiento.
- **Captura de la prueba en seco y de la sonda:** sin `-X utf8` (la guarda lo rechazaría), así que la salida llega en la codificación de la tubería
  (cp1252, registrada en `FrozenCommandGuard.Encodings.Stdout`); se guardan los bytes crudos (`prep/phase/step10.raw` y `step11.raw`) y su
  conversión a UTF-8 (`record-probe.json` y `launch-dry-run.json`).
- **`finish_kit.py` no se ejecutó de una vez:** sus pasos, uno a uno, con sus mismos comandos, para revisar cada resultado (como en las rondas 1 y 2).

## Supuestos no medidos y puntos abiertos

1. **Duración frente al tope de 3 600 s** (estimación arriba); el tope no se ha cambiado (§84, punto 7).
2. **El hijo sustituto no es `claude.exe`:** la sonda demuestra el código común de lanzamiento y registro en este entorno, no el tamaño ni la duración
   de la salida real, ni el refresco OAuth (`r3/launchfix/README.md`, sección 6). La sonda se repite dentro del lanzamiento real.
3. **Entorno heredado:** el lanzador no pone ni quita variables (§84, punto 2); `claude.exe` hereda el entorno de quien lance, incluidas las variables
   CLAUDE* y ANTHROPIC* que `preflight.json` registra por su nombre (en el entorno de esta preparación hay, por ejemplo, `CLAUDECODE`,
   `ANTHROPIC_BASE_URL` y varias `CLAUDE_CODE_*`; en la ronda 2 era igual). Lo decide la sesión principal antes de lanzar; el kit no lo cambia.
4. **Residuo no corregido (fuera de los defectos demostrados):** el `clone_git` del auditor usa `text=True` sin codificación (regla del clon, byte a
   byte); sus salidas sobre un clon limpio son ASCII. `owner_gate.py` pasa `GIT_OPTIONAL_LOCKS=0` a sus propias lecturas de git (como en la ronda 2);
   no afecta al entorno de `claude.exe`.
5. **Líneas de más de 2 000 caracteres sin copia legible** (11 archivos: los seis sobres de `bmin/e2e/out/`, la salida de la ronda 1 L103, V14
   L2352/L2380/L2382, A-3 L1487, decisiones L657/L734/L750/L768/L787/L805/L819, y `prompt.md`, que no es premisa): como en la ronda 2; el prompt las
   lista y prohíbe citarlas.
6. **Compactación automática del contexto** (no observada antes): el prompt avisa; contaría como un segundo mensaje de usuario.
7. **Que el esquema (12 289 caracteres compactos) se comporte con claude-cli** como los anteriores (mismas palabras clave).
8. **Tiempo de las pruebas en este host:** la autoprueba tardó 102 min y la mutación 3 h 23 min (la carga del host varió mucho; cada lanzamiento de
   git costó hasta 0,5 s). No afecta al lanzamiento, pero el auditor sobre la corrida real tardará del orden de un minuto.

## Archivos y privacidad

```text
SHA-256                                                           bytes  archivo
f34b0adcaaf0387483981018c5a26df47e020c5c4f3962737938b9b319f71959  110243  kit/a6-r2.md                         (= blob 86da0ae7, ronda 2)
4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7    6070  kit/c3_analysis.py                   (= ronda 2, 4fc6115e)
1e9f3c832451d6086197758bce519f2d3a0c42088180e4303d26f44e4a1546e1   68359  kit/clone-verification.json
9c124407b52bce888e7e9603f95d8357e5bc7a4648c39ae8577c2ba58352da19  331121  kit/closure.json
a9b14a59ea48c7c38a9f4f8598f1dc42c76e23c68a1d59fe6c644a4053258d1c    7271  kit/finish_kit.py
771d9f4629a2b82a42c0ff98db08c52da5e297bf7a0c899918cbdced864ea918    6247  kit/kit-manifest.json
285d5a4628d8b659107c7571c8c06b56ec4e8873b0894aa14f98968519331069    2976  kit/kit-params.json
2093f7ba21b4be124e3869f82ee60f6da8caf3fca35261852ca7610a78414c1c   45570  kit/launch.py                        (de launch.r3-template.py)
cba49486bfb5193b97d3cd90e6e3ef9fca040fe4b4a348de2aaf6402517904c1   79938  kit/make_closure.py
51f939adc6c32b0c0813089504ce954bf50bf6bc4e9c1215db42884b06fb3f84    6976  kit/make_manifest.py
24aa18ea75d7a6f70732a95cf4642aa9255cbb27a338b09333d25fe9cfe32830    7611  kit/mutation-post-review.py
90940f38a487547bc61d317df8464d4f1a195e7230addff96c69eb37906c38ad   51776  kit/mutation-result.json
8a6b560f1da473ff152732e2e6bf2b82cd60f2df4ddfc7e44f8359edc7bef2e8    3377  kit/order.txt                        (= day3\s84_body.txt, cuerpo §84)
1fff7c1f82d8a164978532114407ddfa50d9d2e0df52269b3a9afaf755b07ee7   14254  kit/owner_gate.py                    (compuerta 11, §84)
b21cb94045192f0855fa61faa55496183e8c2577d202726c79e7e27899680120   66971  kit/post-review.py                   (auditor v5.3-a6r3)
8a3e638ab2b932029145f01a7b1a5b29840cb86d5ffaae73614d41f3ae1ccce9    9080  kit/prepare_clone.py
fa6f011ecfdeb79f71a108b2a6a5cca52be17ec0ca2d8207a7351f7851ac632b  102671  kit/prompt.md
cf7e1888158887dc8c438fc55515ccf41986e642c51e1631d60908f7f2118f59   39951  kit/prompt.template.md
7b51116e5e4876c1416066994e282ad077661547ee5fb1dcb0fbcbd59e413cce    4925  kit/read_fidelity.py                 (= ronda 2, 7b51116e)
2ce2eafb72b1abbe9b01cc81962e11fd7e5c82b8007217751144dc32b7fb1633   18059  kit/result.schema.json               (rackcad-a6r3-architect-review/0)
bbb994a0a037081ef40927de383f6733e0f75b5bc589a120abef215f8db95f0a  118425  kit/selftest-post-review.py
9856ebfec4b9eaafbb5df7791e844424078ecd5c5ab4142eacb88663dc112070  137448  kit/selftest-result.json
e9adefa97f2babfb4e8b17c24798e02bcd43c2d24dac8c83972f9573784cb02c    1602  kit/transport-gate.json
e326b5b92da5ff2b53d05a7d64f90dbb09bf098ab4eeda5a92597d6fc36e6fbf   22485  kit/transport_gate.py                (D-01 sin cambio; decodificación §84.5)
3ef35b440a3587167523b88076ab092af056f64ede9b276441db7d82821d0dde   19156  kit/verify_clone.py
3284b0c89ca5bc9cb744b4992be80a8865e2ad80cb93586457c2d0a675430622   22144  prep/adapt_launch.py
74280f67e5a3469e1cf991a828a1d9e20761c3806957d0844f87bfd8e0fc594f   17247  prep/adapt_misc.py
771aa22d1786061bdd8878f3826958df61d7fb0cb765405f272c2ea9379e162a   15783  prep/adapt_owner_gate.py
b1f6121cd7308a3e08c0f25b08819eaacc48cd1b3cea1725488e1e03bf91c7f9   35168  prep/adapt_post_review.py
281ab123d1293220b48c233a20bc404e9fd7d5bb94aa09d5c092fcf4297f4374    7394  prep/adapt_transport_gate.py
610e6b88db1cf6476124f361ee1c76a9de7ccd48d519497df62d629558ad186b    9045  prep/assemble_readme.py
06606ccd169a9339abad10a93f0555e4e2179ee744b25955dfc82a83ba854386    2895  prep/file_table.py
a7e4aefe0adcfa04db36ec7f52209885a0b1b679bd66b64d27c5d8cf48046e0d    9726  prep/gen_schema.py
3631b8709858e68a4c27dfef8c3ec3c446cda8c825230e2484e949141e65c23f     210  prep/ids.txt
e336cb7c643f191c4391681c85936fd2e41be2c0708d405ff8c045dedd3e9fcc    2228  prep/make_diffs.py
adfb5a0b34cd66cb25e45ad3558e04a42ea45bdbb7b6a7de6dfda21bffac451b    1996  prep/subst.py
52097c3c9af536e4734c0ea079d91ed34ba99267cc3933893d388ddb08a8d1db    1052  prep/dev/quick_cases.py
f9dc8d94e72f89071ed08d405c83c99b80e67647aad5c3b5f5c32cb09f51b0ec    8314  prep/diffs/finish_kit.diff
c9c07acdf9df52089905ea9c0e921015de6de1cf0e0e4f0b1e41547c68d5963a   14429  prep/diffs/gen_schema.diff
991ab48296124ef8b03864fd620b159875eb8d6a6e03bd332b485b695167de6e   24303  prep/diffs/launch.diff
3860c877c59bdda986d126669228eb41fc793a08c7e8593f4d00f74dc51667a1   65618  prep/diffs/make_closure.diff
3d263a1d7353fcdf51111f523908f3a0c6de5b1c671afcba7b390459ebc8de1e    5043  prep/diffs/make_manifest.diff
b40249b25e5f3a4a9b6a0b2e85152fc76bfe99442980190db565ea1d22fca481    4073  prep/diffs/mutation-post-review.diff
254cb45041b5ca5e04cdd62262c032cd27d0bc1fb814733e5d1128a9689f53f7   15296  prep/diffs/owner_gate.diff
145770c5cef0c9098d639f42b0b73dbf9d3f9f2d60b49f828c41eb2821ef5214   40043  prep/diffs/post-review.diff
e13aab95aab2920b8a0e4556ca8e234cb91e524018030824d7e95d2fdb02e5b2   12168  prep/diffs/prepare_clone.diff
02916f20121397fce238ce8f9308a6e244a98c18524c0d588e9c7ff2463e4a54   61364  prep/diffs/prompt.template.diff
a66e0d093b798143424dac590cea170e929247b2fe364e06e5ecd7f954371db9   27121  prep/diffs/result.schema.diff
05c2dc6c88f0fbc2a5dc0219f3612c1c761cec1465afaf93203dea730bfb3106  126138  prep/diffs/selftest-post-review.diff
094a038eee8147179905d769149bc4fe399eff0ac4703873ef27b2729dde4ae4    4969  prep/diffs/transport_gate.diff
79f78b4bc61f2cf80d8fb5c68c9d225531a1e908df7696f813d4ff3c48b6372a   14761  prep/diffs/verify_clone.diff
ab4c9b03d0d621ec1eff42d121a9e138c94cec34a297842d8d6793c90dcf2d3a    1465  prep/phase/step1.out
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855       0  prep/phase/step10.err
fcb2ee5a62231538914edba40c202688e379ee702a76517e5f8671ff2b8b499a    2917  prep/phase/step10.raw
d6cbd391b7072f1c5bad626ce1818bdbd2857cf4950dd24bd507b21ca1c42846      47  prep/phase/step10.times
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855       0  prep/phase/step11.err
11f328a4dcf0d4e3358ca09dadad5e74ff9064d90eb3c2386522b1015fa193f3    8480  prep/phase/step11.raw
1ad6a7624c64d8553cc5791d243d3ad97df703b5713ef1b2d32e566f3789730f      47  prep/phase/step11.times
b0c778a7e0bae44247b8baaf506e058e0b78f483f00b32daa54d2596c2c69445   13787  prep/phase/step4.out
ee6f2d546fe72a9df860935d9785a55c229517baf62ddbcc52ccd13ad17e944c     223  prep/phase/step5.out
d9cf6ac8426a9ac6619bfb1047d72478530bdf63d10011b107f49843db0f5c43     411  prep/phase/step6.raw
4860052f18837f8c55e6ca10f393186ebe3a62d4de208f345e8c9ca337c063e5      42  prep/phase/step6.times
49b1150ee23c43e80ddf3ddf639f43bf3d1bfbeac313aff993a2afcfe56fcc66   18957  prep/phase/step7.raw
fe26bc3dffc4b7dc31a816bcdb625e8337a0dc423b8d1dfd28fa7806e48c7310      42  prep/phase/step7.times
4929d3ed2bd43c99d8db3b9a02238ae29e08884f96c3036d56b5027906aeeb0a   13582  prep/phase/step8-attempt1.raw
627cfa3ffe550aef14a04108e90ea3c23dc603c371cc4d7443b7e7e7b36b53b6      42  prep/phase/step8-attempt1.times
0e3025e33ce88ec0bdd71b1dd14a8670fdfa4e84867189fdca4987e9ea5a4187   13984  prep/phase/step8.raw
710b5e709ec9a7690570b54b8417b331d4d7ebbef3ba8c859c2808d22a354fd7      42  prep/phase/step8.times
292cbe2507abe8a23f3ed64c9608c99e616afa1ae2dc151f75b5fa31536d6547     145  prep/phase/step9.out
10e3923b8e533e5d38e438bf46054060f7ffe7293e057df535728aa6fda03408    8491  launch-dry-run.json
79837ec8ff0c0b5d16349633019605d4b5c7082236c6d9f88f6286810c8449c9    2919  record-probe.json
```

Junto a este README: `launch-dry-run.json` (salida de `python -B kit/launch.py --dry-run`, convertida a UTF-8) y `record-probe.json` (salida de `python -B kit/launch.py --record-probe`). `kit/` es el kit congelado (manifiesto: `kit/kit-manifest.json`). `prep/` es la preparación y no se custodia como kit: `subst.py` y `adapt_*.py` (derivan por sustituciones exactas el auditor, el lanzador, la compuerta de transporte, la compuerta 11 y los tres auxiliares), `gen_schema.py`, `make_diffs.py`, `file_table.py`, `assemble_readme.py` (este README), `ids.txt`, `diffs/` (cada archivo frente al de la ronda 2, o frente a la plantilla en el caso de `launch.py`), `phase/` (salidas saneadas de cada paso) y `dev/quick_cases.py` (ejecución parcial de la autoprueba durante el desarrollo).

**Privacidad:** `prep/file_table.py` comprueba que ningún archivo de `kit/`, de `prep/` ni de esta carpeta (este README incluido) contiene el nombre de usuario de Windows, largo u 8.3, ni una dirección de correo (salvo las de prueba del dominio reservado example.invalid): sin coincidencias. Las rutas del perfil se escriben como %USERPROFILE%, %LOCALAPPDATA%, %APPDATA% o %TEMP%; del entorno solo constan nombres de variables y los valores de las PYTHON*, y de `auth status` nada se registra todavía (solo se ejecuta en el lanzamiento real, y entonces solo loggedIn, authMethod y apiProvider).

## Resultado (añadido tras la corrida, sesión principal; decisiones §84)

```text
Sesión:          3f6938fa-d6b4-4494-bacb-684ce6287624, de 2026-10-14T04:19:12Z a 2026-10-14T04:37:51Z, Outcome COMPLETED, salida 0, sin errores posteriores
Uso:             54 turnos; herramientas {"Read": 43, "Grep": 9, "StructuredOutput": 1}
Veredicto:       CHANGES REQUIRED
REQUIRED:        1 (A62-A6R3-R1)
OPTIONAL:        7
R1-R2 ronda 2:   A62-A6R2-R1 PARTIALLY_CORRECTED; A62-A6R2-R2 CORRECTED
Negativos:       N4 INCOHERENT; NC1 INCOHERENT; VERIFY_WITHOUT_INSTRUMENT COHERENT
Reconstrucción:  ADEQUATE_AS_FAIL_CLOSED_PRECONDITION; cierre efectivo: ADEQUATE_AS_FAIL_CLOSED_PRECONDITION
Aceptante:       9 MATCHES, 0 DIFFERS
G1-G6:           G1 ACCEPTABLE_RESIDUAL; G2 ACCEPTABLE_RESIDUAL; G3 ACCEPTABLE_RESIDUAL; G4 ACCEPTABLE_RESIDUAL; G5 ACCEPTABLE_RESIDUAL; G6 ACCEPTABLE_RESIDUAL
O1-O5 ronda 2:   A62-A6R2-O1 DECLINED_WITH_REASON; A62-A6R2-O2 INCORPORATED; A62-A6R2-O3 PARTLY_INCORPORATED; A62-A6R2-O4 PARTLY_INCORPORATED; A62-A6R2-O5 INCORPORATED
Precondiciones:  KEPT_FAIL_CLOSED (8)
Acreditación:    ACCREDITED por el auditor v5.3-a6r3 (0 motivos); sin acreditación excepcional
Clon tras corrida: HEAD 9b8b66a2, estado limpio
```

El veredicto no es el acuerdo: A-6 no se declara AGREED hasta una revisión ACCREDITED sin REQUIRED y una disposición posterior del Coordinator
(decisiones §84, punto 8). Ni A-6 AGREED, ni FX-02 PASS, ni F6 GATE PASS.

Archivos añadidos: `output.json` y `audit.json` (byte a byte, sin modificar), `runtime-evidence.json`, `launch/` (saneado), `launch-dry-run-s84.json`,
`preconditions-s84.json`, `launch-run.log` y `post-review.log`. La transcripción y `stdout.jsonl` no se versionan; sus SHA-256 están en
`runtime-evidence.json`.
