# I-62 — Kit de la revisión formal única de la RONDA 4 de la enmienda A-7, acotada a A62-A7R3-R1 y a sus consecuencias (R20261024T041128Z-4825)

```text
LogicalReviewRequestId: L20261024T041128Z-4825   InvocationId: I20261024T041128Z-4825   AttemptSeq: 1   RunId: R20261024T041128Z-4825
SessionId:      bd018dcb-165f-437d-bcfc-c8adaed5d774 (uuid4, generado una vez el 2026-10-24T04:11:28Z con los demás ids; prep/ids.txt)
Orden:          decisiones §98 (línea 1915 en 94a04af7; cuerpo pegado 3 740 bytes, SHA-256 8b73975d9822a430f1a726268d4005609974dbdcc85a0674bf00dbb9e9018acf
                = kit/order.txt = run/order.txt), puntos 6 (reutilizar componentes congelados idénticos y sus evidencias; validación incremental)
                y 7 («Preparar el kit acotado, pero no ejecutar revisión formal sin nueva disposición»). §98 es la última sección de decisiones en
                la HEAD 94a04af7 del worktree
Autorización:   FIJADA: decisiones §99, «Disposición del Coordinator: revisión formal única de A-7 ronda 4 (2026-10-24 UTC)», línea 1932 en la
                HEAD de96a7f5; AUTH_TEXT_SHA e69527b801f1fcb7…; fijada con pin_authorization.py (prep/p99/). La versión PENDIENTE del kit está en la
                custodia de 736a3805 (manifiesto 46b9c465…); abajo, «Fijación de §99»
Consumo:        cubierto por CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62, solo lectura), la misma base que las rondas 1 a 3
Objeto:         docs/initiatives/I-62-A-7.md, ronda 4, PROPUESTA, blob ebb3a87b0b33ddf1d6418f4e9f4fb4195d1555d3 (58 618 bytes, 476 líneas), publicado en
                99de338d105640f9390201678f02dd2a1d3f69a8 (padre 2405a919, con la ronda 3 8bcccb2f y §98) y registrado en 94a04af7 (resultado `run` de
                las guardas de r4/, PASS en G1-G7)
Ronda 3:        blob 8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff (commit 37ac6ca4) = run/a7-r3.md (premisa); su revisión R20261023T180846Z-62bd
                (output.json con A62-A7R3-R1 y O1-O3, audit.json ACCREDITED, README) está en el cierre, sin cambio (árbol f5869bff = 068d3368)
Commit del clon: 94a04af7c663eac44016b4011cdffead714a530a (HEAD de la rama; contiene la publicación, el registro de r4 y §98, su última sección)
Clon / run:     D:\r62-arch-a7r4 (HEAD separado en 94a04af7, sin ramas ni remoto; clonado desde el directorio común de git) y D:\r62-arch-a7r4-run
                (order.txt = cuerpo §98, a7-r3.md = ronda 3, prompt.md = kit/prompt.md y 43 copias fieles fiel__*.txt). Los clones y runs de las
                rondas 1 a 3 no se tocaron
Transporte:     el de las rondas 1 a 3 sin cambio: claude-cli 2.1.299, %APPDATA%\Claude\claude-code\2.1.299\f0584be0fbaf\claude.exe, SHA-256
                295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b (presente, con ese SHA-256 y firma Authenticode válida de Anthropic en la prueba en seco final, comprobación 3, 2026-10-24); claude-opus-5-5, effort
                xhigh; Read, Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; prompt por stdin; resultado por --json-schema;
                tope de 3 600 s; espera de 120 s tras `auth status`
Orden de lanzamiento: EXACTAMENTE `python -B kit/launch.py`, desde la carpeta que contiene kit/ (esta), sin flags ni variables añadidos
Compuerta D-01: MEASURED sobre kit/claude-cli-characterization.json (SHA-256 4ba6ba58…, byte a byte la de las rondas 1 a 3); transport_gate.py byte a byte
Compuerta 11:   BLOQUEANTE: línea del Owner de §78; CLAUDE-CLI-I62 = A y el mismo binario; A-7 en la HEAD = ebb3a87b y A-6 = 4bc919dd (acordada);
                decisiones solo por añadido desde el clon; la HEAD desciende del clon; una disposición posterior a §98 fijada (encabezado, blob
                completo, SHA-256 del texto, una sola sección) y ninguna sección posterior. Fijada en §99 (prueba en seco: Ok)
Estado:         kit FIJADO en §99 y vuelto a congelar (kit/kit-manifest.json, SHA-256 90b9cf93171b7c17e02f3d4416b1dd711ddabf9fab1f57eb58571a37ae41cdf5, 73 archivos más el manifiesto); sonda de
                registro en Ok; prueba en seco con BlockingChecks = [] (cero bloqueos; 4 y 5 no se ejecutan en seco)
```

## Qué pide §98 y dónde se cubre

| Exigencia | Dónde |
|---|---|
| §98.2-§98.4: A-7 ronda 4 acotada a A62-A7R3-R1, con la matriz de modalidades y el preflight dirigido | el objeto (blob `ebb3a87b`) y el preflight de la ronda 4 entero en el cierre (`I-62-A7/r4/`, 109 archivos: designaciones `a7-designation/4`, constructor, validador, `designate_a7`, guardas, `modalities.md`/`.json`, `summary.json` 245/245 y el resultado `run`); el esquema pide la resolución de A62-A7R3-R1 con siete cambios normativos (`R3R1_*`), la pureza del delta, doce temas de regresión, el dictamen A-5/A-6 y Q-A7R4-01..03; el prompt presenta la matriz como evidencia de apoyo, no como autoridad |
| §98.5: O1-O3 sin enmienda nueva | `S35_RESIDUALS_O1_O2` (residuos de O1 y O2 en §3.5, sin cambio normativo) y `S33_S34_S4_WITH_O3` (enumeración de O3) entre los cambios del esquema; las O de la ronda 3 en `PriorOptionals` del cierre |
| §98.6: reutilizar componentes congelados idénticos y sus evidencias; no reconstruir auditores; validación incremental | 8 componentes byte a byte (`c3_analysis.py`, `claude-cli-characterization.json`, `fidelity.py`, `pin_authorization.py`, `read_fidelity.py`, `selftest-fidelity-result.json`, `selftest-fidelity.py`, `transport_gate.py`); diffs del resto en `prep/diffs/`; autoprueba 321/321 y autoprueba de fidelidad 53/53 sobre los módulos finales; negativa de `verify_clone.py` (cambió); **mutación dirigida** en lugar de la batería completa (abajo): reglas con los mismos sitios que en la ronda 3 y `owner_gate.py` 19/19; el auditor no se reconstruyó (v7.0-a7r4 = el código de reglas de v7.0-a7r3 con los datos de la ronda 4) |
| §98.7: preparar el kit acotado sin ejecutar la revisión | autorización PENDIENTE; la prueba en seco queda bloqueada SOLO por la compuerta 11 con el motivo pendiente |
| §98.8: sin Codex, sin consumir A6-7, sin B1 ni FX-U2, sin tocar A-1..A-6, producción, I-65 ni D.5; I-64 con prioridad | ninguna invocación de modelos (claude.exe solo en `--record-probe`, con un hijo sustituto, y `--dry-run`, que no lo ejecuta); el preflight de A-7 no se vuelve a ejecutar; I-65 fuera del cierre (`NotTriggered`); el worktree de I-64 no se toca (abajo, «Host») |

## Alcance del cierre (321 archivos, 6 441 006 bytes)

| Grupo | Archivos | Bytes |
|---|---|---|
| A-7 ronda 4 (objeto) y base normativa (v14, AUTOMATION_PLAN, RAE, A-1..A-6, ADR-0046 y ADR-0048, contratos del controlador, mapa de cláusulas y esquemas, incluido `relay-record/v2`), fuentes literales L1-L27, decisiones y evidencia de I-62 | 22 | 2 131 458 |
| preflight de la ronda 4 (`I-62-A7/r4/`, entero, con su resultado `run`) | 109 | 1 175 364 |
| preflight de la ronda 3 (`I-62-A7/r3/`, entero, sin cambio) | 58 | 699 595 |
| revisión de la ronda 3 (`output.json`, `audit.json`, `README.md`) | 3 | 281 672 |
| m7/ (s91, s92, banco: README, designaciones, p1, N4, nc1; lecturas) | 55 | 1 027 837 |
| kit `a6m7-measure` entero | 44 | 583 283 |
| paquete B-min entero | 30 | 541 797 |

Fuera del cierre, frente al de la ronda 3: el preflight de la ronda 2 (`I-62-A7/r2/`) y la revisión de la ronda 2 (`R20261019T201030Z-d1bf`); la
ronda 3 encontró lo que de ellos sigue vigente, y su salida está en el cierre. Siguen fuera los preflights de la ronda 1, `r2-dispositions/`, la
revisión de la ronda 1, `I-62-A6/` fuera de lo listado e I-65 (`I-62-C1C3/`); el prompt y `NotTriggered` del cierre lo declaran. Lectura directa
del Paso 0: RAE §10 L219-L226 (`docs/automation/agent-execution/README.md`: los controles negativos sobre las entradas de la verificación
VERIFIED), en su original, que no está marcado.

## Fidelidad de lectura (componentes de §94, punto 5, reutilizados)

43 archivos marcados por `fidelity.py` (sin cambio) se leen SOLO por su copia fiel RACKCAD-COPIA-FIEL/1 en el run: 17 con `LONG_LINE` (14 solo por ella), 15 con `UNMEASURED_CHAR` (12 solo por ella), 9 con `BOM` (9 solo por ella), 5 con `ZWNBSP` (5 solo por ella); 2 411 637 bytes en total, la mayor
`I-62-evidence.md` (462 766). **El objeto (A-7 ronda 4) SÍ está marcado** (`LONG_LINE` en L237; en la ronda 3 no lo estaba): su copia `fiel__docs__initiatives__I-62-A-7.md.txt` (59 762 bytes, 493 líneas)
es la lectura fiel del objeto (abajo, riesgo 4). La premisa `a7-r3.md` no está marcada. Las copias no se custodian como archivos (sus rutas
pasarían de MAX_PATH en el worktree): se reproducen byte a byte desde `94a04af7` con `kit/fidelity.py` (`prep/regen_faithful_r4.py` →
`prep/faithful-regeneration.json`, AllReproduced: 43/43 copias, 2 411 637 bytes).

## Qué cambió frente al kit de la ronda 3

Base: `base3/` = el kit de la ronda 3 con la autorización PENDIENTE, igual blob a blob a la custodia de `000fd6eb` (los SHA-256 de partida de
`prep/shift_r4.py`). La derivación tiene dos pasos: `prep/shift_r4.py` desplaza una ronda los componentes que dependen de ella (identificadores,
objeto y ronda previa, decisiones §95-§96 → §97-§98, secciones §3.6-§3.7 → §3.7-§3.8, ids de hallazgos y preguntas, rondas y directorios; §93 y §94
son anclas históricas y no se desplazan) y cada `prep/patch_*_r4.py` hace las ediciones semánticas exactas, cada una con su número de apariciones
comprobado. Diffs unificados en `prep/diffs/`, tabla en `prep/diffs/diff-table.json` (`prep/make_diffs_r4.py`).

| Componente | Ronda 3 → ronda 4 | Qué cambia |
|---|---|---|
| `c3_analysis.py`, `claude-cli-characterization.json`, `fidelity.py`, `pin_authorization.py`, `read_fidelity.py`, `selftest-fidelity-result.json`, `selftest-fidelity.py`, `transport_gate.py` | IDÉNTICOS | — |
| `a7-r2.md` | RETIRADO | la premisa de la ronda 3, sustituida por `a7-r3.md` |
| `a7-r3.md` | NUEVO (+394/−0) | la premisa: la ronda 3 (blob 8bcccb2f) en lugar de la ronda 2 |
| `clone-verification.json` | `70e7c6b6` → `79fad1b9` | generado para la ronda 4 |
| `closure.json` | `e399b04a` → `1134b703` | el cierre de la ronda 4 (321 archivos) |
| `finish_kit.py` | `f82cbc31` → `8155e62a` (+27/−21) | constantes de la ronda 4; el paso 12 ejecuta la mutación dirigida (`--full-mutation`, la batería completa) |
| `kit-manifest.json` | `84a9883d` → `46b9c465` | generado para la ronda 4 |
| `kit-params.json` | `b4c1506f` → `35af0deb` | `prepare_clone.py` de la ronda 4 (clon en `94a04af7`) |
| `launch.py` | `87ff62ff` → `93aa11d8` (+31/−31) | constantes de la ronda 4 (clon, run, sesión, premisa a7-r3.md, patrón de proyecto `D--r62-arch-a7r4*`, §98) |
| `make_closure.py` | `2fbbd6a8` → `b168fd50` (+243/−221) | cierre de la ronda 4 (`prep/patch_make_closure_r4.py`, 68 ediciones): lectura directa RAE 219-226, árboles fijados r3/r4/revisión r3, literales L1-L27 y «contratos», rangos de §3.5-§3.8, §97 y §98 |
| `make_manifest.py` | `575e04fa` → `19867c07` (+31/−19) | constantes de la ronda 4 y la cabecera que declara la mutación dirigida (`targeted-mutation-result.json`) |
| `mutation-post-review.py` | `cf120dd8` → `e7c12abd` (+8/−8) | textos de la ronda 4; el arnés (sitios y mutante) no cambia: lo usa `targeted-mutation.py` |
| `mutation-result.json` | RETIRADO | la batería completa no se repite (mutación dirigida, arriba): su resultado de la ronda 3 (209/209) sigue en `000fd6eb` |
| `order.txt` | `adb11535` → `8b73975d` | el cuerpo pegado de §98 (3 740 bytes) en lugar del de §96 |
| `owner_gate.py` | `7e0cf27a` → `9d4a8f96` (+21/−21) | objeto ebb3a87b, base §98, AUTH_* = None (PENDIENTE), RunId; funciones byte a byte (paso A sin ediciones) |
| `post-review.py` | `e1d6b1b2` → `7a024e30` (+26/−17) | v7.0-a7r4 (`prep/patch_auditor_r4.py`): claves del Paso 0 de la ronda 4, mensajes de compatibilidad con §98, punto 4; el código de las reglas es el de v7.0-a7r3 |
| `prepare_clone.py` | `82082790` → `ec34615c` (+39/−39) | constantes de la ronda 4 (clon, run, sesión, premisa a7-r3.md, patrón de proyecto `D--r62-arch-a7r4*`, §98) |
| `prompt.md` | `085776ac` → `57dd3627` | renderizado de `prompt.template.md` con el cierre de la ronda 4 |
| `prompt.template.md` | `05250607` → `3d60a956` (+179/−152) | alcance A62-A7R3-R1, Paso 0, «Qué decidir» y «Resultado» de la ronda 4 (`prep/patch_prompt_r4.py`) |
| `result.schema.json` | `4b003339` → `60aec9b8` | `rackcad-a7r4-architect-review/0` (`prep/gen_schema_r4.py`, reproducible): PriorFindingResolution de A62-A7R3-R1; DeltaPurity con los once cambios de §3.8; RegressionCheck de doce temas; CompatibilityVerdicts A5/A6; Q-A7R4-01..03; Step0 de la ronda 4 (lectura directa de RAE §10 L219-L226); premisa `a7-r3.md`; el mismo patrón de referencias |
| `selftest-post-review.py` | `318ef540` → `86213c6f` (+152/−146) | casos de la ronda 4 (`prep/patch_selftest_r4.py`): lectura directa de RAE en su original (F08), cambios y temas de la ronda 4; los de transporte, lectura, custodia, lanzador, compuertas y fidelidad, sin cambio salvo nombres |
| `selftest-result.json` | `23796b90` → `a6e5738f` | generado para la ronda 4 |
| `targeted-mutation-result.json` | NUEVO | el resultado de la mutación dirigida |
| `targeted-mutation.py` | NUEVO (+119/−0) | la mutación dirigida de §98, punto 6 (arriba) |
| `transport-gate.json` | `4cbd5e35` → `41821d54` | generado para la ronda 4 |
| `verify_clone.py` | `6ccdcb32` → `b6e7a333` (+59/−51) | constantes de la ronda 4; reproduce las guardas de r4/ (`run` y su autoprueba, sin archivos borrados antes de publicar) |

Las claves de `PreconditionChecks` de `kit/kit-params.json` (p. ej., `Round2AtPublicationParent`, `R3EvidenceAbsentAtPublicationParent`) son las
heredadas de `prepare_clone.py`; su lógica usa las constantes de la ronda 4 (ronda previa = `8bcccb2f`, evidencia = `r4/`, evidencia previa = `r3/`,
revisión previa = `R20261023T180846Z-62bd`, última sección = §98).

## Mutación dirigida (decisiones §98, punto 6)

§98, punto 6, manda reutilizar los componentes congelados idénticos y sus evidencias aplicables y validar por incrementos cuando la autoridad y las
dependencias lo permitan. En lugar de repetir la batería completa de la ronda 3 (209/209, 4 h 40 min), `kit/targeted-mutation.py` comprueba
mecánicamente que esa batería sigue siendo la evidencia aplicable y muta lo que cambió:

- **Identidad de las reglas:** lee del clon los cuatro módulos de la ronda 3 (`git cat-file` en `000fd6eb`). `transport_gate.py` y `fidelity.py`
  son byte a byte los mismos; en `post-review.py` y `owner_gate.py` los sitios de mutación del arnés congelado (`mutation-post-review.SITE_RX`, las
  llamadas que añaden un problema) son los mismos, en el mismo orden, con el mismo código una vez vaciado el texto de los literales, y en el mismo
  número que en la batería de la ronda 3 (131, 40, 19 y 19). Cambian solo mensajes, versión, docstring y constantes de datos.
- **`owner_gate.py`** (cambian sus constantes: objeto, §98, RunId): sus sitios, mutados uno a uno con `mutant_source()` del arnés congelado y
  juzgados sobre los casos de la compuerta 11 de la autoprueba (los que los mataron en la batería completa), con la línea base en PASS.

Resultado (`kit/targeted-mutation-result.json`, 2026-10-24T06:59:23Z → 2026-10-24T06:59:34Z, código 0): **AllPass True**; identidad de las reglas True (`post-review.py` 131/131 sitios, `owner_gate.py` 19/19 sitios, `transport_gate.py` 40/40 sitios, byte a byte, `fidelity.py` 19/19 sitios, byte a byte); `owner_gate.py` **19/19** muertos, línea
base sin fallos.

`finish_kit.py` ejecuta esta mutación en el paso 12 (con `--full-mutation`, la batería completa) y `make_manifest.py` la declara en la cabecera.

## Preparación ejecutada (2026-10-24)

1. **Identificadores** generados una vez (`prep/ids.txt`): RunId, InvocationId, LogicalReviewRequestId y SessionId (uuid4).
2. **Esquema** `prep/gen_schema_r4.py` → `kit/result.schema.json` (SHA-256 `60aec9b8088e0f5b43dec592ec4d5e803b44df8247d55663549065aa96e34b3c`; reproducible).
3. **Componentes derivados** del kit de la ronda 3 (`base3/`) en dos pasos: `prep/shift_r4.py` (desplazamiento con marcadores, exige el SHA-256 de
   partida de cada componente) y `prep/patch_small_r4.py`, `prep/patch_auditor_r4.py`, `prep/patch_make_closure_r4.py`, `prep/patch_prompt_r4.py` y
   `prep/patch_selftest_r4.py` (ediciones exactas con su número de apariciones); `kit/targeted-mutation.py` nuevo; diffs en `prep/diffs/`.
4. **Clon y run** (`kit/prepare_clone.py`, 2026-10-24T04:34:22Z): las quince precondiciones en true (`kit/kit-params.json`).
5. **Copias fieles y prompt** (`make_closure.py --faithful-copies` y `--render-prompt`): 43 copias; `prompt.md` de 125 348 bytes y 822 líneas
   (SHA-256 `57dd3627e335952162a422c01472da31eee637366f87aeb8c00aa5afc1c36de9`), que muestra literalmente las 321 rutas del cierre (el renderizado falla si falta alguna).
6. **Cierre** (`make_closure.py`): `kit/closure.json` (SHA-256 `1134b70315ae2919a10bed8653ef1261574ddb0861d391bdeee8af4409c810e2`), 321 archivos, comprobaciones de identidad de la ronda 4 en true.
7. **Compuerta D-01** (`transport_gate.py init` y `set MEASURED`): `kit/transport-gate.json` MEASURED sobre la caracterización `4ba6ba58…`.
8. **verify_clone.py**: AllChecks; guardas de r4/ reproducidas: `run` PASS, entero igual al custodiado; autoprueba PASS, 45/45 mutantes muertos, Added 101,
   entera igual a la custodiada (sin archivos borrados antes de publicar).
9. **Autoprueba** `selftest-post-review.py` (2026-10-24T05:53:56Z → 2026-10-24T06:56:08Z): **321/321** (auditoría 214, compuerta de transporte 61, compuerta 11 40 con la sonda
   viva L00 en estado PENDIENTE, consistencia 6 con X06); clon limpio y run igual al kit al terminar. La primera ejecución (2026-10-24T04:45:10Z →
   05:47:18Z, `prep/selftest-result-run1.json`) dio 319/321 con dos fallos de la derivación, no del auditor: `F00` esperaba leer el original marcado
   de AUTOMATION_PLAN (la lectura directa de la ronda 3) y no el objeto, que en la ronda 4 está marcado; `O03` usaba como «sección posterior» la
   §99, que en la ronda 4 es la autorización sintética. Corregidos en `prep/patch_selftest_r4.py` (F00 con decisiones y A-7; O03 con §101; y los
   nombres de cuatro casos que aún llevaban la sección base de la ronda 3), comprobados aparte con `prep/quick_check_fixes.py` (compuerta 11 39/39,
   fidelidad 16/16) y repetida la autoprueba entera sobre el módulo final.
10. **Cadena** `prep/chain_r4.sh` (registro `prep/chain.log`): autoprueba de fidelidad 53/53 (2026-10-24T06:56:31Z → 2026-10-24T06:56:37Z, código 0); negativa de verify_clone (2026-10-24T06:56:38Z → 2026-10-24T06:59:01Z, código 0; la copia alterada
    no verifica, las otras 42 sí, clon limpio: `prep/neg-verify-result-r4.json`); manifiesto, sonda y prueba en seco previa (bloqueada solo por la
    compuerta 11: `launch-dry-run-before-mutation.json`); **mutación dirigida** (arriba; 2026-10-24T06:59:23Z → 2026-10-24T06:59:34Z, código 0); manifiesto final, sonda (`record-probe.json`,
    RecordProbe y FrozenCommandGuard en Ok) y prueba en seco final (`launch-dry-run.json`): código 2, `BlockingChecks` = [11-OwnerAndAuthorizationGate]
    con el único motivo pendiente; 1-3 y 6-10 en Ok; 4 y 5 no ejecutadas en seco.
11. **Copias fieles reproducidas** desde `94a04af7` (`prep/regen_faithful_r4.py` → `prep/faithful-regeneration.json`, 2026-10-24T06:59:56Z → 2026-10-24T07:00:00Z, código 0): AllReproduced 43/43.
12. **Privacidad** (`prep/privacy_scan_r4.py` → `prep/privacy-final.json`): 159 archivos, 0 coincidencias fuera de las copias fieles exentas (texto del
    repositorio, que no se custodia). Las líneas de órdenes de los procesos no se registran (solo el ejecutable): podrían nombrar archivos de terceros.

## Supuestos no medidos y riesgos residuales

1. **Binario:** el 2.1.299 medido sigue siendo el de la compuerta; la app instala versiones nuevas junto a él. Si una actualización lo borra (como
   pasó con 2.1.293 el 2026-10-16), la comprobación 3 bloquea y hace falta una caracterización nueva.
2. **Duración frente al tope de 3 600 s:** la ronda 2 duró 821 s y la ronda 3, 826 s (55 turnos cada una). La ronda 4 tiene un cierre mayor (321
   archivos, 6,4 MB, frente a 262 y 5,8 MB) y un prompt de 125 348 bytes (110 404 en la ronda 3). El tope no se cambió.
3. **Esquema de 18 622 caracteres compactos** (18 367 en la ronda 3), con el mismo patrón de referencias por familias del cierre y las mismas
   palabras clave medidas; su comportamiento con claude-cli a este tamaño no está medido.
4. **El objeto se lee por su copia fiel:** A-7 ronda 4 tiene una línea de 1 941 unidades UTF-16 (L237, la fila (c) de §3.3), por debajo del
   límite de Read (2 000) pero por encima del margen de `fidelity.py` (1 900), y está marcada `LONG_LINE`; el prompt da su copia, en la que esa
   línea continúa en líneas marcadas, con las referencias en las líneas del original. Leer el original es informativo si la entrega no se altera,
   y DEGRADED si se altera.
5. **Compactación automática del contexto:** el prompt avisa; contaría como un segundo mensaje de usuario (NOT_ACCREDITED).
6. **El hijo sustituto de la sonda no es `claude.exe`:** la sonda demuestra el código común de lanzamiento y registro, no la salida real.
7. **Mutación dirigida:** la parte A demuestra que los sitios de las reglas son los de la ronda 3; que los casos de la autoprueba de la ronda 4
   matarían cada mutante de `post-review.py`, `transport_gate.py` y `fidelity.py` se infiere de la batería de la ronda 3 (los casos son los de la
   ronda 3 con nombres y datos de la ronda 4) y no se ha vuelto a ejecutar.
8. **Anclaje:** la evidencia de I-62, los `preflight.md` y la matriz son de la sesión que preparó A-7; el prompt los presenta como evidencia
   contrastable, nunca como autoridad.
9. **Preparador no independiente:** la sesión principal, que preparó A-7 ronda 4, preparó este kit; el prompt no contiene veredicto esperado ni
   valoraciones, y el revisor es un proceso nuevo de claude-cli con su propio clon, run y proyecto.
10. **Presupuesto de la medición (riesgo ya registrado en el preflight):** D.3 da 11 lanzamientos base, un fondo de 4 y un tope de 15, sin reserva
    para una reverificación; si basta no está demostrado, y la revisión no lo resuelve.
11. **`pin_authorization.py`** conserva su docstring histórica (ronda 4 de A-6) para no cambiar su SHA-256; su lógica usa `owner_gate.BASE_SECTION`
    (ahora 98).

## Host: I-64 y checkout principal (solo lectura)

- `prep/host_checks_r4.py` (git -C con GIT_OPTIONAL_LOCKS=0; procesos por Win32_Process, solo el ejecutable) a las 2026-10-24T07:00:28Z (`prep/host-checks-final.json`):
  checkout principal en `main`, HEAD `95690c28dc6268e61dff32a0cbc33cc9fde3d47f`, 0 líneas de estado, origin/main local `bb0d5522e8411f66a51fdfb3f1f0d0514b737453`; worktree de I-64 en `09f2f1c3` con 0 líneas de estado;
  worktree de I-62 en `94a04af7` con 0 líneas; clon de la ronda 4 en `94a04af7` con 0 líneas (con ignorados); clon de la ronda 3 en `b9bfae50` con 0 líneas;
  run de la ronda 3 con 46 entradas y la última modificación el 2026-10-24T01:30:39Z (no se tocó); claude-code instalados: 2.1.299, 2.1.308; procesos vigilados: ninguno;
  memoria libre 27.7 GB, CPU 23 %.
- La preparación no tocó el worktree de I-64 ni el checkout principal (solo `rev-parse` y `status` con GIT_OPTIONAL_LOCKS=0).

## Fijación de §99 (decisiones §99, puntos 2-4)

- **Registro:** §99 por añadido en `de96a7f5` (línea 1932; cuerpo pegado de 2 695 bytes, SHA-256 `0bf8aee4d1457d2877964e849846076b2cd045ef7efd1db86e7d0a20d83acc36`). Antes de
  fijar, el kit se cotejó con la custodia de `736a3805`: manifiesto `46b9c465…` igual y los 73 archivos con su SHA-256 (`prep/p99/kit-integrity-before-pin.txt`).
- **Fijación:** `pin_authorization.py` sobre el texto de decisiones de `de96a7f5` (blob `f9aeab80`): Ready; una sola sección §99, posterior a §98,
  con el blob completo de A-7 como token y ninguna sección posterior (`prep/p99/pin.json`). En `owner_gate.py` solo cambian las tres líneas AUTH_*
  (`9d4a8f96` → `f76fa4b9`; `prep/p99/owner_gate.pin.diff`, `prep/p99/apply_pin.py`).
- **Validación incremental (§99, punto 3)** (`prep/p99/targeted-validation.json`): casos de la compuerta 11 39/39; L00 sobre el worktree real en
  estado PINNED (PASS); casos de auditoría que leen la autorización fijada (00, 150-151p, 152, 153) 23/23; mutación dirigida de `owner_gate.py` fijado
  19/19, juzgada sobre los casos de la compuerta 11. Sin repetir la autoprueba de 321 casos ni los 209 mutantes. La cabecera del manifiesto marca
  `owner_gate.py` como cambiado desde la autoprueba y la mutación (`ChangedSinceSelftestOrMutation`): es la fijación, validada aquí.
- **Congelación y preflight (§99, puntos 2 y 4)** (`prep/p99/chain_p99.sh`, registro `prep/p99/chain_p99.log`): manifiesto `90b9cf93171b7c17…` (FIJADA §99);
  sonda de registro con código 0 (`record-probe.json`; la del kit pendiente, `record-probe-pending.json`); **prueba en seco con `BlockingChecks` = []**
  (`launch-dry-run.json`; la del kit pendiente, `launch-dry-run-pending.json`): compuerta 11 en Ok con §99 y sin secciones posteriores; binario
  `claude-cli` 2.1.299 con su SHA-256 y Authenticode Valid de Anthropic; integridad del kit; clon limpio en `94a04af7`; run = kit = cierre =
  manifiesto. 43/43 copias fieles reproducidas desde `94a04af7` (`prep/p99/faithful-regeneration-pinned.json`; la primera ejecución de ese paso
  falló solo porque la ruta del clon de la orden quedó con un retorno de carro al escribir el script; corregida y repetida, código 0).
- **Host** (`prep/p99/host-checks-prelaunch.json`, 2026-10-24T18:16:30Z): checkout principal en `main`, HEAD `95690c28`, 0 líneas de estado; worktree de I-64 en
  `09f2f1c3` con 0 líneas y ningún proceso vigilado; corriente conectada (batería al 100 %, sin descarga); la sesión retiene la suspensión por
  inactividad mientras corre el lanzamiento.

## Lanzamiento

1. Una disposición del Coordinator POSTERIOR a §98 que nombre esta revisión (RunId R20261024T041128Z-4825, objeto ebb3a87b) en las decisiones de la
   rama; ninguna sección posterior a ella hasta lanzar y auditar. **Hecho: §99.**
2. Fijarla: `python -B kit/pin_authorization.py <texto de decisiones de la HEAD> <número>` (Ready: número posterior a §98, una sola sección, blob
   completo como token, ninguna sección posterior) y sustituir SOLO las tres líneas AUTH_* de `kit/owner_gate.py`. **Hecho.**
3. Volver a congelar: `python -B kit/make_manifest.py`; repetir la autoprueba de la compuerta 11 y la sonda L00 en estado FIJADO y la mutación dirigida
   de `owner_gate.py`, como en las rondas 2 y 3. **Hecho.**
4. Desde esta carpeta: `python -B kit/launch.py --record-probe` (código 0) y `python -B kit/launch.py --dry-run` (código 2, `BlockingChecks` = []);
   después, UNA vez, `python -B kit/launch.py` (tope 3 600 s, sin reintento) y el auditor sobre la corrida completa:

```text
python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a7r4\bd018dcb-165f-437d-bcfc-c8adaed5d774.jsonl" D:\r62-arch-a7r4-run\launch\stdout.jsonl D:\r62-arch-a7r4 D:\r62-arch-a7r4-run kit kit/result.schema.json audit.json output.json
```

El auditor informa por separado `Categories.Execution` y `Categories.ArchitectVerdict`; el veredicto no es el acuerdo. Ni A-7 AGREED, ni Controller
ELIGIBLE, ni FX-02 PASS, ni F6 GATE PASS se declaran aquí.

## Archivos y privacidad

Los SHA-256 de todos los archivos de esta carpeta están en `MANIFEST.sha256` (la regla `-text` de `.gitattributes` conserva los bytes
exactos). Las 43 copias fieles `kit/fiel__*.txt` se custodian por reproducción (`prep/faithful-regeneration.json`); sus SHA-256 están en
`kit/kit-manifest.json` y en `FaithfulCopies` del cierre. Componentes del kit (sin las copias fieles):

| Archivo | Bytes | SHA-256 |
|---|---|---|
| `kit/a7-r3.md` | 45 668 | `1146c622e84068306d59897abafc18ce032bd2707e47708a0193a3510b194cbd` |
| `kit/c3_analysis.py` | 6 070 | `4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7` |
| `kit/claude-cli-characterization.json` | 17 086 | `4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c` |
| `kit/clone-verification.json` | 102 924 | `79fad1b9400a8abd98af3ee3ca4f0f8b9572b6d98c245956016cbbfef5ab3e18` |
| `kit/closure.json` | 503 448 | `1134b70315ae2919a10bed8653ef1261574ddb0861d391bdeee8af4409c810e2` |
| `kit/fidelity.py` | 21 072 | `a84825e00112965f33c7530ffac6ff84353ff036f2773ee525ebd178e872c68f` |
| `kit/finish_kit.py` | 12 105 | `8155e62aafdaf0bd61993b167c7deb1f24cae7069acc09279e292c61dd0e84ae` |
| `kit/kit-params.json` | 2 724 | `35af0debf77e2f788e3899b3063d02f1877984d127628659fc65eb21a8175cc5` |
| `kit/launch.py` | 48 260 | `93aa11d819c2e8fd748e131e783feda736e02f4ccc36f15402e6a7252375b7a0` |
| `kit/make_closure.py` | 111 553 | `b168fd501b15b678295f1307d7043dc359102d30ea9ec54aae51c5646938e57b` |
| `kit/make_manifest.py` | 12 009 | `19867c0743320a460bc53620f80177a96ecee932aacdc6be9ab359cad746bd8d` |
| `kit/mutation-post-review.py` | 11 182 | `e7c12abdd76150555487a03557e12b2727a041ee67bde4c9a6c9c0929251f58e` |
| `kit/order.txt` | 3 740 | `8b73975d9822a430f1a726268d4005609974dbdcc85a0674bf00dbb9e9018acf` |
| `kit/owner_gate.py` | 18 161 | `f76fa4b967247c00ded6adca1515f586d623bbd38e5e587780cb220cd2061460` |
| `kit/pin_authorization.py` | 5 588 | `a8e4070751ae604010149c8678aa9320766c533230bf92de3aa2e88ea6e571cc` |
| `kit/post-review.py` | 77 053 | `7a024e30e80bf75f2b90073462c06a70ed5d8fc2f480c3fcd90ca7efeb518897` |
| `kit/prepare_clone.py` | 11 550 | `ec34615caaf1e36f7165757d677fcde469973af04f978b83654302a76b468c36` |
| `kit/prompt.md` | 125 348 | `57dd3627e335952162a422c01472da31eee637366f87aeb8c00aa5afc1c36de9` |
| `kit/prompt.template.md` | 44 748 | `3d60a9569e65c515760ed758d48ba5cbe814c177d7da32d31a8e2f50763c20fd` |
| `kit/read_fidelity.py` | 6 143 | `da1457a1a035221ba41710f1bea6bcd28312c95d66199885accccf3a21f11cfa` |
| `kit/result.schema.json` | 24 213 | `60aec9b8088e0f5b43dec592ec4d5e803b44df8247d55663549065aa96e34b3c` |
| `kit/selftest-fidelity-result.json` | 9 062 | `df0506164f5019276479cd0d9039a5975974faf2a592b233f477de29c544b890` |
| `kit/selftest-fidelity.py` | 18 240 | `0ad71f0d3f3bd4be6986d0851910a095ff573e48193e9c106d7c53fefffd0da3` |
| `kit/selftest-post-review.py` | 154 367 | `86213c6fa6a196b1966c09d53ca4c61929151a39a3c1b1519348b81386fa3572` |
| `kit/selftest-result.json` | 181 477 | `a6e5738f4d85bb20df7ca2640f36a0bb5bad5d4d44b7659751f2f7016f59ec5a` |
| `kit/targeted-mutation-result.json` | 8 898 | `93c4c1fa1d988db84bb6fa0b5b08fae186e0759f976878a3d66f58fecd3c0c4b` |
| `kit/targeted-mutation.py` | 7 124 | `4d7656b0c488a927669a169f89d77af0ab3fc6868c9cd8b2aca71d767a4d3bb8` |
| `kit/transport-gate.json` | 1 544 | `41821d5404a56af20175e328e12492049e0cf1206688557ff150061e51f30f7c` |
| `kit/transport_gate.py` | 22 485 | `e326b5b92da5ff2b53d05a7d64f90dbb09bf098ab4eeda5a92597d6fc36e6fbf` |
| `kit/verify_clone.py` | 30 635 | `b6e7a333254e5e6c0920e3923eebbb359758c853e6dee0166a416de0da4c6d06` |
| `kit/kit-manifest.json` | 30 826 | `90b9cf93171b7c17e02f3d4416b1dd711ddabf9fab1f57eb58571a37ae41cdf5` |
| `record-probe.json` | 2 919 | `86dbe5fec5798d7a64f4233f968f04be9d917fd0e8218737552afdd2aa8ebbc8` |
| `record-probe-pending.json` | 2 919 | `cad13d1ea5a78910070b46ffa264e2981bebbdf91f57f24247f1605219880db8` |
| `launch-dry-run.json` | 16 374 | `e52cf57671dbab07590d30b11e8568669b993efb2670da0a89b7f44ef972bfce` |
| `launch-dry-run-pending.json` | 15 570 | `a76ddd32d5c9462b208ef46e2cd1b8213d65a26560bb3a534c960fd17f4aec3a` |
| `launch-dry-run-before-mutation.json` | 15 570 | `8b684c5765f82e6b89b994eb0a64f99b3a6f3ea0dc77ad6db5391217d931503a` |

`prep/` contiene los scripts de derivación y de preparación, la salida del desplazamiento (`prep/shifted/`), los diffs frente al kit de
la ronda 3, las salidas crudas de la cadena y los resultados de la negativa, la reproducción, el host y la privacidad. `base3/` (el kit de
la ronda 3 de partida) no se vuelve a custodiar: está en `000fd6eb`. Ninguna transcripción, credencial ni archivo de autenticación.
