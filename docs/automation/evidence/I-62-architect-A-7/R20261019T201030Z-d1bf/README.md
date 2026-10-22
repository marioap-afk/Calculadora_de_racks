# I-62 — Kit de la revisión formal única de la RONDA 2 de la enmienda A-7, acotada a A62-A7-R1..R3 y a sus consecuencias (R20261019T201030Z-d1bf)

```text
LogicalReviewRequestId: L20261019T201030Z-d1bf   InvocationId: I20261019T201030Z-d1bf   AttemptSeq: 1   RunId: R20261019T201030Z-d1bf
SessionId:      3dd5ac31-ddf5-4e4a-b732-b913933a9ae3 (uuid4, generado una vez el 2026-10-19T20:10:30Z con los demás ids; prep/p2/ids.txt)
Orden:          decisiones §94 (línea 1846 en b432c40f; cuerpo pegado 3 522 bytes, SHA-256 515ccfa37205aa1d9d1fccbd3eb3c2ed0c2b56b8bb67c56102d1a8b0bea3db0e
                = kit/order.txt = run/order.txt), puntos 5 (fidelidad) y 6 (kit limitado a R1-R3; «Sin revisión formal sin una disposición nueva del
                Coordinator»). §94 es la última sección de decisiones en la HEAD b432c40f del worktree
Autorización:   PENDIENTE. kit/owner_gate.py entrega AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA en None: la compuerta 11 BLOQUEA con el único
                motivo «authorization pending» hasta que una disposición del Coordinator POSTERIOR a §94 que nombre esta revisión se fije con
                pin_authorization.py y el kit se vuelva a congelar. Nada se fijó
Consumo:        cubierto por CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62, solo lectura), la misma base que la ronda 1
Objeto:         docs/initiatives/I-62-A-7.md, ronda 2, PROPUESTA, blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec15 (37 205 bytes), publicado en
                e90b444d5d23288e0b5a59881820377bee0d7d09 (padre b9334f22, con la ronda 1 6c29747d y §94) y registrado en b432c40f (resultado `run` de
                las guardas de r2/, PASS; disposiciones propuestas de A62-A7-O1..O8 en r2-dispositions/); 346 líneas, SHA-256 dbb7db53c566…
Ronda 1:        blob 6c29747d61c5f689a195095ad3712e6d9dba62aa = run/a7-r1.md (premisa, como a6-r3.md en la ronda 4 de A-6); su revisión
                R20261019T010200Z-4a72 (output.json R1-R3 y O1-O8, audit.json NOT_ACCREDITED, README) está en el cierre
Commit del clon: b432c40f8ca7b1e915dff531cb9a8cad6466bb0c (HEAD de la rama; contiene la publicación, el registro de r2 y §94, su última sección)
Clon / run:     D:\r62-arch-a7r2 (HEAD separado en b432c40f, sin ramas ni remoto; clonado desde el directorio común de git) y D:\r62-arch-a7r2-run
                (order.txt = cuerpo §94, a7-r1.md = ronda 1, prompt.md = kit/prompt.md y 43 copias fieles fiel__*.txt). El run de la ronda 1
                (D:\r62-arch-a7-run) y su clon no se tocaron
Transporte:     el de la ronda 1 sin cambio: claude-cli 2.1.299, %APPDATA%\Claude\claude-code\2.1.299\f0584be0fbaf\claude.exe, SHA-256
                295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b (presente y con ese SHA-256 el 2026-10-19); claude-opus-5-5, effort
                xhigh; Read, Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; prompt por stdin; resultado por --json-schema;
                tope de 3 600 s; espera de 120 s tras `auth status`
Orden de lanzamiento: EXACTAMENTE `python -B kit/launch.py`, desde la carpeta que contiene kit/ (esta), sin flags ni variables añadidos
Compuerta D-01: MEASURED sobre kit/claude-cli-characterization.json (SHA-256 4ba6ba58…, byte a byte la de la ronda 1); transport_gate.py byte a byte
Compuerta 11:   BLOQUEANTE: línea del Owner de §78; CLAUDE-CLI-I62 = A y el mismo binario; A-7 en la HEAD = 8a528133 y A-6 = 4bc919dd (acordada);
                decisiones solo por añadido desde el clon; la HEAD desciende del clon; una disposición posterior a §94 fijada (encabezado, blob
                completo, SHA-256 del texto, una sola sección) y ninguna sección posterior. En el kit entregado: PENDIENTE
Estado:         kit preparado, probado y CONGELADO con la autorización PENDIENTE (kit/kit-manifest.json, SHA-256 3499596df4292720a4390aeac7270a8f16ed2708c12590d0b3a3ae37e57425c5); sonda de registro en
                Ok; prueba en seco bloqueada SOLO por la compuerta 11 con el motivo pendiente (4 y 5 no se ejecutan en seco). NO lanzado:
                ningún claude.exe ni CLI de modelos ejecutado; ni red ni credenciales
```

## Qué piden §94 (puntos 5 y 6) y la disposición de la fase 2, y dónde se cubre

| Exigencia | Dónde |
|---|---|
| §94.5: corregir la detección de BOM en el cierre; no tratar como fiel la eliminación silenciosa de un BOM | `kit/fidelity.py` (detector y copia fiel RACKCAD-COPIA-FIEL/1), `make_closure.py --faithful-copies`, auditor v6.0-a7r2 (verificación byte a byte de cada copia; la regla DEGRADED de una lectura del original intacta). Abajo, «Fidelidad» |
| §94.5: si Read no conserva los bytes, una vía permitida con evidencia byte a byte o detener | cada archivo marcado se lee SOLO por su copia fiel en el run (Read sobre un archivo del run, una vía ya permitida); el auditor acredita la línea del original solo por la copia verificada; si no, NOT_ACCREDITED |
| §94.5: reutilizar el transporte y las compuertas acreditados que sigan idénticos | `transport_gate.py`, `c3_analysis.py`, `pin_authorization.py` y la caracterización byte a byte; `owner_gate.py` con las funciones de la ronda 1 byte a byte (cambian objeto, base y autorización pendiente) |
| §94.5: validación incremental (§88) y repetir los negativos del componente de fidelidad | `selftest-fidelity.py` (negativos FD/FE/FV/FR/FN/FX), casos F00-F20 de `selftest-post-review.py`, negativa de `verify_clone.py` con un byte alterado, mutación completa con `fidelity.py`; matriz en `prep/p3/validation-matrix-r2.{json,md}` |
| §94.6: kit limitado a R1-R3 y sus consecuencias | prompt y esquema `rackcad-a7r2-architect-review/0`: resolución de R1-R3, pureza del delta (diez cambios), no regresión de los cinco temas CONFORMS de la ronda 1, materialidad M-01..M-08, OWNER-RESERVED, Q-A7R2-01..03, conformidad opcional de O1-O8, IfAgreed (nueve booleanas); D-1/FC solo si R1-R3 los cambian |
| §94.6: sin revisión formal sin una disposición nueva | autorización PENDIENTE; la prueba en seco queda bloqueada SOLO por la compuerta 11 con el motivo pendiente |
| Fase 2, decisión (a): batería COMPLETA de mutación una vez al final (§88.4: cambiaron reglas del auditor y la semántica de la comprobación 7 del lanzador) | `kit/mutation-post-review.py` → `kit/mutation-result.json` (abajo, «Mutación completa») |
| Fase 2, decisión (b): leer un original marcado en líneas entregadas fielmente es informativo | `Fidelity.FlaggedOriginalReads` (informativo); la regla DEGRADED decide |
| Fase 2, decisión (c): marcar también los archivos con puntos de código no ASCII fuera del conjunto medido | clase `UNMEASURED_CHAR` de `fidelity.py`: 16 archivos la tienen, **13 solo por ella** (añadidos por la decisión (c)); 43 copias en total en lugar de 30 |
| Fase 2, decisión (d): copias grandes aceptadas | la mayor, `I-62-evidence.md`, 435 062 bytes; las 43 suman 2 449 125 bytes |
| Ningún lanzamiento; claude.exe solo en los modos sin modelo | solo `--record-probe` (hijo sustituto: el intérprete de Python) y `--dry-run` (no ejecuta el binario) |

## Independencia del preparador

Las fases 1 y 2 las construyó un subagente de la sesión principal, que se detuvo dos veces por el vigilante de flujo; un segundo subagente las
revisó, escribió este README hasta «Lanzamiento» y se detuvo durante la autoprueba. Ninguno evaluó A-7 ni sus preflights: el prompt no contiene veredicto
esperado ni valoraciones; las definiciones de los estados del esquema son de forma. No leyeron transcripciones, credenciales ni archivos de autenticación.
Recibieron automáticamente el índice de memoria de usuario de la sesión (MEMORY.md); no se usó para el contenido del kit, y el revisor no lo recibe
(--safe-mode, otro proyecto de claude-cli, clon y run propios). **La sesión principal terminó el kit** (2026-10-20 a 2026-10-22): es la sesión que
preparó A-7 ronda 2, así que no es independiente del objeto; sus cambios en el kit se limitan a dos correcciones (la expectativa del caso 48 de la
autoprueba y una docstring del auditor, abajo, paso 8) y no tocan el prompt, el esquema, el cierre, las copias fieles ni ninguna regla del auditor; además
ejecutó los pasos 7-12 y completó las secciones «Preparación ejecutada», «Validación incremental», «Host» y «Archivos».

## Qué cambió frente al kit lanzado de la ronda 1 (fase 1 + fase 2)

Base: `base-launched/kit/` = el kit custodiado de la ronda 1 (manifiesto `c27db93e…`, custodia 3922f835, igual blob a blob a
`docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/kit/` en e71400a2). Fase 1 = `phase1-kit/` (corrección de fidelidad,
aceptada). Diffs unificados de cada componente frente a los dos en `prep/p3/diffs/vs-launched/` y `prep/p3/diffs/vs-phase1/`; tabla en
`prep/p3/diffs/diff-table.json` (`prep/p3/make_diffs_r2.py`). SHA-256 completos al final («Archivos»).

| Componente | Lanzado → ahora | Fase 1 (fidelidad, §94.5) | Fase 2 (ronda 2, §94.6) |
|---|---|---|---|
| `transport_gate.py`, `c3_analysis.py`, `pin_authorization.py`, `claude-cli-characterization.json` | IDÉNTICOS (`e326b5b9`, `4fc6115e`, `a8e40707`, `4ba6ba58`) | — | — |
| `fidelity.py` | NUEVO (`a84825e0`) | detector (BOM, UTF-8 inválido, NUL, CR suelto, controles, línea > 1 900 unidades UTF-16, U+FEFF interior, U+2028/2029, extensiones que Read interpreta), copia fiel RACKCAD-COPIA-FIEL/1 (codificación canónica, decodificación, distribución de líneas, verificación) y `run_files_of` | clase `UNMEASURED_CHAR` (decisión (c)); conjunto medido de 33 → 34 puntos de código (U+2013, medido en las 80 líneas de order.txt que Read entregó fielmente en r1; `prep/p2/r1-literals.json`) |
| `read_fidelity.py` | CAMBIADO (`7b51116e` → `da1457a1`) | marcas del detector por llamada; surrogateescape | sin cambio |
| `post-review.py` | CAMBIADO (`303321f2` → `f5cdef1f`) | v5.6-a7: cada copia fiel verificada contra los bytes del original en el commit; crédito de la línea n del original solo por la copia entregada entera y fiel; canon con surrogateescape; `FlaggedOriginalReads` informativo; la regla DEGRADED intacta | v6.0-a7r2: esquema y coherencia de la ronda 2, lectura directa AUTOMATION_PLAN L616-L617, registro de la compuerta 11 con base §94 y autorización pendiente; las reglas de lanzamiento, transporte, stream-json, transcripción, herramientas, lectura, fidelidad, referencias, custodia, clon, run.json, FrozenCommandGuard, RecordProbe y Environment sin cambio; sesión principal: docstring de la lectura directa (ninguna línea de código) |
| `result.schema.json` | CAMBIADO (`116c393e` → `9e533317`) | — | `rackcad-a7r2-architect-review/0` (`prep/p2/gen_schema_r2.py`, reproducible byte a byte; 11 063 caracteres compactos ASCII; las mismas palabras clave medidas) |
| `owner_gate.py` | CAMBIADO (`879d630a` → `3d420a9f`) | — | objeto 8a528133, base §94, AUTH_* = None (PENDIENTE); funciones de sección, token, autorización y secciones posteriores byte a byte |
| `launch.py` | CAMBIADO (`01f60c7a` → `17dda725`) | comprobación 7 con las copias fieles (`run_files_check`, `fidelity.run_files_of`) | clon, run, session id, patrón `D--r62-arch-a7r2*`, run con a7-r1.md (blob 6c29747d comprobado en la 7); G, R, P, D y las comprobaciones 1-6 y 8-11 sin cambio de lógica |
| `make_closure.py` | CAMBIADO (`a0d41107` → `756459c0`) | detector sobre cada archivo, modo `--faithful-copies`, `FaithfulCopies` y `RunFiles` en el cierre, prompt con la lista de copias; un archivo no UTF-8 ya no detiene | cierre de la ronda 2 (246 archivos), comprobaciones de identidad de la ronda 2, renderizado del prompt de la ronda 2 |
| `verify_clone.py` | CAMBIADO (`feb7d615` → `fb3d8f13`) | run = RunFiles; cada copia verificada contra el original en el commit | objeto, commits, a7-r1.md, revisión de la ronda 1 igual a su custodia, guardas de r2/ (`run` y autoprueba) |
| `prepare_clone.py` | CAMBIADO (`fc0e0b24` → `23ee99ef`) | — | catorce precondiciones de la ronda 2; run con order.txt y a7-r1.md |
| `make_manifest.py`, `finish_kit.py` | CAMBIADOS | run con las copias; pasos 2b y 8b | autorización pendiente; batería completa al final; prueba en seco bloqueada solo por la compuerta 11 |
| `selftest-post-review.py` | CAMBIADO (`e6deb731` → `435f76f4`) | casos F00-F20 y X05 | casos de la ronda 2 (56-121j, 150-151p, O00-O30 con una §95 SINTÉTICA, L00 pendiente); sesión principal: expectativa del caso 48 |
| `selftest-fidelity.py` | NUEVO (`0ad71f0d`) | negativos FD/FE/FV/FR/FN/FX (47) | FD22-FD27 de `UNMEASURED_CHAR` (53) |
| `mutation-post-review.py` | CAMBIADO (`9ed3eca1` → `8c275526`) | batería con `fidelity.py`; memo de verify/layout por contenido | nombres de la ronda 2 |
| `prompt.template.md` | CAMBIADO (`c3ad10d5` → `24b93624`) | sección de copias fieles y reglas de referencia | alcance R1-R3 de la ronda 2 |
| `order.txt`, `a7-r1.md` | CAMBIADO / NUEVO | — | cuerpo §94 (`515ccfa3`); texto de la ronda 1 (blob 6c29747d, `1ab7315a`) |
| `mutation-targeted.py`, `mutation-targeted-result.json`, `mutation-result.json` de la ronda 1 | RETIRADOS | (la fase 1 usó una mutación dirigida, 22/22) | sustituidos por la batería completa (`prep/p2/retired-r1-artifacts/`) |
| generados (`closure.json`, `prompt.md`, `kit-params.json`, `transport-gate.json`, `clone-verification.json`, `selftest-result.json`, `selftest-fidelity-result.json`, `mutation-result.json`, `kit-manifest.json`, 43 `fiel__*.txt`) | REGENERADOS / NUEVOS | — | de la ronda 2 |

## Fidelidad de lectura (§94, punto 5)

- **Detector** (`fidelity.scan`, sobre los bytes de cada archivo del cierre en el commit del clon): de los 246 archivos, **43 marcados** (203 sin
  copia). Por clase (un archivo puede tener varias): LONG_LINE 19, UNMEASURED_CHAR 16, BOM 6, ZWNBSP 5; ninguno con INVALID_UTF8, NUL, BARE_CR,
  CONTROL, LINE_SEPARATOR ni SPECIAL_EXTENSION. Combinaciones: LONG_LINE 16, UNMEASURED_CHAR 13, BOM 6, ZWNBSP 5, LONG_LINE+UNMEASURED_CHAR 3.
- **Decisión (c), implementada por el agente anterior y comprobada aquí:** la clase `UNMEASURED_CHAR` marca todo punto de código no ASCII fuera
  del conjunto medido (34 puntos de código que Read entregó fielmente en r1: `prep/measure_r1_literals.py` sobre el audit.json custodiado de r1,
  4 374 líneas, 4 294 de archivos del cierre y 80 de order.txt; `prep/p2/r1-literals.json`), salvo los que ya marcan otras clases. **Añade 13
  archivos** que solo marca ella (A-7 r1 y r2 `a7-guards.py`, el README de la revisión de la ronda 1, `s91/d1/d1.md`, `s91/risk-matrix.md`,
  `readings/readings.md`, el README del kit a6m7-measure, `test_bmin_accept.py`, AUTOMATION_PLAN, A-4, A-1, ADR-0046 e I-62-evidence) y se suma a
  LONG_LINE en V14, A-3 y decisiones: sin la decisión (c) habría 30 copias. Respecto de la fase 1, el conjunto medido pasa de 33 a 34 (U+2013,
  presente en una línea de order.txt de r1 que el auditor de r1 registró FAITHFUL_NORMALIZED).
- **El objeto (A-7 ronda 2) y los archivos premisa del run no tienen marcas:** `I-62-A-7.md`, `order.txt` (68 líneas) y `a7-r1.md` (279 líneas) solo
  contienen ASCII imprimible y puntos de código medidos, sin líneas largas; se leen directamente. `prompt.md` (que nunca se lee ni es referencia;
  llega por stdin) tendría LONG_LINE (línea 441, la lista de canónicos) y U+00DA en las líneas 1, 460 y 474: informativo.
- **Copias fieles** (`make_closure.py --faithful-copies` → `kit/fiel__*.txt`, copiadas byte a byte al run): 43, 2 449 125 bytes (originales:
  2 392 731); la mayor, la de `I-62-evidence.md` (435 062 bytes); 19 con apéndice de continuaciones (las líneas largas). Cada una decodifica en los
  bytes exactos de su original (blob comprobado), es la codificación canónica, no tiene marcas propias y conserva la numeración de líneas
  (comprobado aquí de nuevo con `prep/p3/scan_run_files.py`: cero problemas; run = RunFiles; cada archivo del run = su copia del kit = RunFileHashes).
- **Lecturas del Paso 0 por la copia:** AUTOMATION_PLAN (marcado por U+2212 en la línea 605) y decisiones (marcado) se leen en su copia
  (`fiel__docs__AUTOMATION_PLAN.md.txt` con offset 608 y limit 18; `fiel__docs__automation__decisions__I-62.md.txt`, líneas 1846-1861); la
  referencia es la ruta del original con el mismo número de línea. El objeto se lee en el clon.
- **Auditor:** una lectura de un original marcado sigue siendo DEGRADED si su entrega se altera (motivo de NOT_ACCREDITED); si las líneas leídas
  llegaron fielmente, la lectura queda en `Fidelity.FlaggedOriginalReads` como información (decisión (b)).

## Contrato del kit

- **Cierre** (`kit/make_closure.py` → `kit/closure.json`, en b432c40f, cada archivo por su blob): **246 archivos, 5 946 805 bytes, 94 757 líneas**:
  A-7 ronda 2 entera (objeto) con su mapa (L1-L17 de §2, A7-1 y sus ocho puntos, §3.3-§3.6, §5, §7, §8 con Q-A7R2-01..03); `I-62-A7/r2/` entero (50) y
  `r2-dispositions/optional-dispositions.md`; el preflight de la ronda 1 (35, sin cambio); la revisión de la ronda 1 (output.json, audit.json, README.md); las fuentes de A-7 §2
  con sus rangos, parseadas de §2 (`A7Literals`: L1-L17, entre ellas L16 = AP 16.8 L616-L617 y 16.9 L630, L635, L638, L639, L641 y L643, y L17 = A-6
  L600-L601 y L673-L675); A-5 y A-6 enteras; decisiones (§62, §73, §78, §90-§94 con puntos y cuerpos pegados); `m7/` (55), `r4/` (7), el kit
  a6m7-measure (44) y B-min (30); la base normativa (LIFECYCLE, ADR-0046, ADR-0048, A-1..A-4, el Freeze, V14, AUTOMATION_PLAN, RAE y sus esquemas,
  controller-contracts, el mapa de cláusulas); `I-62-evidence.md` con rangos de orientación §187-§191 (evidencia, no autoridad). Fuera: I-62-C1C3/,
  I-65, el resto de I-62-A6/, los kits de las revisiones anteriores, transcripciones, memoria y fixture. Comprobaciones de identidad
  (`Identity`): ronda 1 en el padre de la publicación; 8a528133 en la publicación, el registro y el clon; A-6 acordada en los cuatro; r2/ sin cambio
  tras la publicación salvo el resultado `run`; la evidencia de la ronda 1 sin cambio; nueve árboles fijados iguales a sus custodias; decisiones por
  añadido desde el registro de §93 y §94 última sección (línea 1846, texto `f53a8fd4…`); resultado `run` PASS (Base b9334f22, Head e90b444d);
  autoprueba de las guardas 46/46; `summary.json` 119/119; la revisión de la ronda 1 (CHANGES REQUIRED, NOT_ACCREDITED, R1-R3, O1-O8).
- **Prompt** (`kit/prompt.template.md` → `kit/prompt.md`, 103 431 bytes, 688 líneas, SHA-256 `40c9de64…`, sin marcadores sin sustituir):
  neutral; Paso 0 con la lectura directa de AP 16.8 L616-L617 (por la copia), order.txt entero, §94 (bytes, SHA-256, puntos), el resultado `run`
  de r2, `summary.json` y el output.json de la ronda 1; el alcance acotado de §94.6; las copias fieles; las reglas de lectura y referencias.
- **Esquema** (`rackcad-a7r2-architect-review/0`): identidad; Step0 (siete booleanas de la ronda 2); DirectReads; Reads; hallazgos
  `A62-A7R2-R<n>`/`-O<n>`; `Round1FindingResolution` (CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED); `DeltaPurity` (diez cambios: R1, R2, R3,
  O1, O7, O2 y las precisiones P1 RED según 16.8, P2 medición sin hechos del relevo, P3 derivación como pieza del invalidador, P4 RED
  indeterminada; WITHIN_R1_R3 | OUTSIDE_R1_R3; CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED); `RegressionCheck` (T04, T05, T06, T07, T09:
  NO_REGRESSION | REGRESSION); `OptionalDispositionConformance` (opcional); `Materiality` (M-01..M-08, A7Declared y AGREE | DISAGREE);
  `OwnerReservedConsequence`; `RelatedImpacts` (ChangedByRound2, Note); `QuestionAnswers` (Q-A7R2-01..03); `IfAgreed` (nueve booleanas);
  KnownLimitations; RecommendedNextAction. Veredictos: AGREED, CHANGES REQUIRED, REJECTED, BLOCKED — OWNER DECISION, NOT_ACCREDITED.
- **Auditor v6.0-a7r2:** coherencia (resumen): unicidad y cobertura exacta de cada arreglo; resolución abierta ⇒ REQUIRED residual vinculado,
  CORRECTED ⇒ ninguno; REGRESSION, OUTSIDE_R1_R3, DISAGREE, NOT_CONFIRMED ⇒ REQUIRED vinculado (y su opuesto sin él); A7Declared = A-7 §5 del cierre
  (M-01 no, M-02..M-05 sí, M-06..M-08 no); NOT_SATISFIED y DOES_NOT_CONFORM ⇒ un hallazgo; OwnerDecisionRequired true ⇔ consecuencias no vacías;
  ChangedByRound2 true ⇒ Note; con veredicto, ClauseRefs en todo, sin NOT_DETERMINED ni null, todo REQUIRED citado; **AGREED ⇒ cero REQUIRED**,
  R1-R3 CORRECTED, DeltaPurity CONFIRMED, cinco temas NO_REGRESSION, ocho AGREE, sin decisión del Owner e IfAgreed todo true; con otro veredicto,
  IfAgreed todo null.
- **Compuerta 11** (`kit/owner_gate.py`): PENDIENTE; la autoprueba y la mutación usan una §95 SINTÉTICA añadida en memoria a las decisiones del clon
  (nunca escrita en ningún archivo) para los casos que necesitan una autorización fijada, y el caso 151p comprueba que un kit con la autorización
  pendiente no se puede acreditar. La sonda viva L00 ejerce `owner_gate.check` sobre el worktree real (git de solo lectura) con el módulo tal como
  está: pendiente, debe bloquear con el único motivo pendiente.
- **Lanzador:** `--dry-run` y `--record-probe` no ejecutan el binario de claude.

## Preparación ejecutada (2026-10-19 a 2026-10-22): resultados

Pasos de `kit/finish_kit.py`, ejecutados uno a uno con sus mismas órdenes; salidas y tiempos en `prep/p2/`, `prep/p3/` y `prep/p4/` (`chain.log`,
`chain2.log`, `wait-ac.log`):

1. `prepare_clone.py b432c40f… e90b444d… b432c40f…` (2026-10-19T20:17:45Z) → `D:\r62-arch-a7r2` (HEAD separado en b432c40f; sin ramas ni remoto;
   reflog y gc) y `D:\r62-arch-a7r2-run\{order.txt,a7-r1.md}`; `kit/kit-params.json` (SHA-256 `86e42660…`): las catorce precondiciones en true.
2. `make_closure.py --render-prompt` → `kit/prompt.md` (103 431 bytes, SHA-256 `40c9de64…`). 2b. `make_closure.py --faithful-copies` → 43 copias
   fieles. 3. copiados al run, idénticos. 4. `make_closure.py` → `kit/closure.json` (SHA-256 `d19950bb…`): 246 archivos, 43 marcados (abajo, «Fidelidad»).
5. Autorización PENDIENTE: `owner_gate.py` con AUTH_* = None (SHA-256 `3d420a9f…`); nada fijado.
6. Compuerta D-01: `transport_gate.py init` + `set MEASURED kit/claude-cli-characterization.json` (SHA-256 `4ba6ba58…`) → `kit/transport-gate.json`
   en MEASURED (SHA-256 `a6cff479…`).
7. `verify_clone.py` (2026-10-19T21:21:09Z, 3 min 29 s) → **AllChecks = true** (`kit/clone-verification.json`, SHA-256 `845886e9…`): HEAD separado,
   limpio, 5068 archivos, sin enlaces, blobs del cierre y de las dos rondas, a7-r1.md = blob 6c29747d, la revisión de la ronda 1 igual a su custodia,
   run = RunFiles con cada copia fiel verificada contra el original; guardas de la ronda 2 reproducidas desde el clon contra el worktree (solo
   lectura): **`run` b9334f22..e90b444d PASS, igual entera a la custodiada**, y **autoprueba 46/46, igual entera a la custodiada**.
   **Negativa de la ronda 2** (`prep/p3/neg_verify_clone_r2.py` → `prep/p3/neg-verify-result-r2.json`, 2026-10-20T16:02:28Z-16:04:33Z): un byte
   alterado en la copia fiel del TRX con BOM → código 1, AllChecks false, esa copia `FaithfulCopyVerified` false («la copia no reproduce los bytes del
   original»), las otras 42 verificadas, guardas `run` PASS, clon limpio: **PASS**.
8. **Autoprueba COMPLETA** sobre el clon y el run REALES (`kit/selftest-result.json`, SHA-256 `2edc5855…`; 2026-10-20T16:04:43Z → 2026-10-20T20:04:17Z): **310/310 PASS**
   — auditoría 204/204, compuerta de transporte 61/61, compuerta 11 40/40 (O00-O30 con una §95 sintética y la sonda viva L00 en
   estado PENDIENTE: bloquea con el único motivo pendiente), coherencia lanzador-auditor 5/5 —, con el clon limpio y los 46 archivos del run
   idénticos a sus copias al terminar. Historia: la autoprueba del subagente (2026-10-19T21:55:00Z) se cortó sin resultado por una suspensión del host;
   la primera completa de la sesión principal (2026-10-20T14:55:49Z-16:00:19Z, `prep/p4/selftest-result.json`) dio **309/310**: el caso 48
   esperaba el motivo de la lectura directa de la ronda 1 (V14 L2231-L2233) y el auditor da, correctamente, el de la ronda 2 (AP L616-L617). Se corrigió
   esa expectativa (`prep/p4/fix_case48.py`; la misma cadena que esperan los casos 121 y 121b) y, aprovechando, la docstring del auditor que aún
   describía la lectura directa de la ronda 1 (`prep/p4/fix_auditor_doc.py`; ninguna línea de código); diffs en `prep/p4/fixes.diff`. La autoprueba
   se repitió entera (el host se suspendió de 16:14Z a 18:28Z y el resto corrió con batería; la duración lo incluye).
8b. `selftest-fidelity.py` (2026-10-19T22:10:23Z-22:10:51Z; `fidelity.py` y `selftest-fidelity.py` sin cambio desde entonces) → **53/53 PASS**.
9. `make_manifest.py` (congelación previa a la batería) → `kit-manifest.json` con la mutación PENDIENTE.
10. `python -B kit/launch.py --record-probe` (2026-10-20T20:04:20Z) → código 0. 11. `python -B kit/launch.py --dry-run` (2026-10-20T20:04:23Z) → código 2: **`BlockingChecks` =
    [11-OwnerAndAuthorizationGate]** con `Problems` = [«authorization pending…»]; las demás comprobaciones ejecutadas en Ok; `NotRunInDryRun` =
    ['4-Version', '5-Auth'] (`launch-dry-run-before-full-mutation.json`).
12. **Batería COMPLETA de mutación** (`kit/mutation-result.json`, SHA-256 `0215a272…`): línea base 309 casos (sin L00) y la autoprueba de fidelidad en
    PASS; **207 de 207 mutantes muertos**, ninguno superviviente (3 por excepción): `post-review.py` 129/129 (`f5cdef1f`), `transport_gate.py` 40/40 (`e326b5b9`), `owner_gate.py` 19/19 (`3d420a9f`), `fidelity.py` 19/19 (`a84825e0`). Historia: el primer arranque (20:04:57Z) se detuvo
    a propósito a los 32 s porque el equipo iba con batería; `prep/p4/wait_ac_then_chain2.sh` la relanzó al detectar corriente (2026-10-20T20:17:28Z),
    pero el equipo entró en espera moderna por ahorro de batería a las 20:36Z e hibernó a las 21:11Z; el proceso se reanudó con el equipo el
    2026-10-21 hacia las 14:09Z, ya con corriente, volvió a pausarse con el equipo en espera moderna de 15:20Z a 16:52Z y, ya con batería, de 19:03Z
    del 2026-10-21 a 16:35Z del 2026-10-22, y terminó el 2026-10-22T18:12:51Z.
    Ningún cálculo depende del reloj; el clon siguió limpio.
    Después, 9-11 otra vez: manifiesto final (2026-10-22T18:12:52Z; SHA-256 `3499596df4292720a4390aeac7270a8f16ed2708c12590d0b3a3ae37e57425c5`), sonda con código 0 (2026-10-22T18:12:52Z; `record-probe.json`) y prueba en seco
    (2026-10-22T18:12:54Z; `launch-dry-run.json`) con la misma condición: **`BlockingChecks` = [11-OwnerAndAuthorizationGate]** y el único motivo pendiente;
    en Ok: 1-KitIntegrity, 10-TransportGate, 2-ClosureTransport, 3-Binary, 6-Clone, 7-RunFiles, 8-NewSession, 9-CommandLine; `AuthorizationSections` = [], `LaterSections` = []; compuerta 11: línea del Owner [1572], CLAUDE-CLI-I62 [1085], A-7 en la HEAD
    = 8a528133 (True), A-6 = 4bc919dd (True).

## Validación incremental (decisiones §88; §94, punto 5)

Matriz completa en `prep/p3/validation-matrix-r2.md` y `.json` (componente → SHA-256 en el kit lanzado de la ronda 1 → fase 1 → ahora →
dependencias → por qué → controles, con los resultados leídos de sus archivos). Resumen:

- **Idénticos byte a byte al kit lanzado:** `transport_gate.py`, `c3_analysis.py`, `pin_authorization.py` y `claude-cli-characterization.json`; sus
  controles se repiten porque dependen del cierre nuevo (G00-G58, O30, verify_clone) y `transport_gate.py` vuelve a entrar en la batería completa.
- **Cambiados o nuevos:** `fidelity.py`, `read_fidelity.py`, `post-review.py`, `result.schema.json`, `owner_gate.py`, `launch.py`, `make_closure.py`,
  `verify_clone.py`, `prepare_clone.py`, `make_manifest.py`, `finish_kit.py`, `selftest-post-review.py`, `selftest-fidelity.py`,
  `mutation-post-review.py`, `prompt.template.md`, `order.txt`, `a7-r1.md` y los generados. Controles: autoprueba completa (310), autoprueba de
  fidelidad (53, con los negativos FD/FE/FV/FR/FN/FX), verify_clone positiva y su negativa de la ronda 2, **batería completa** de los cuatro módulos
  (207 mutantes; §88, punto 4: cambiaron reglas del auditor, el esquema, la compuerta 11 y la semántica de la comprobación 7 del lanzador) y los
  controles obligatorios de lanzamiento (manifiesto, sonda, prueba en seco).
- **Retirados:** `mutation-targeted.py` y sus resultados, y el `mutation-result.json` de la ronda 1 (`prep/p2/retired-r1-artifacts/`).
- **Cambios de la sesión principal después de la fase 2:** la expectativa del caso 48 (`selftest-post-review.py` `d188e9ee` → `435f76f4`) y la docstring
  de `post-review.py` (`e5626a7e` → `f5cdef1f`); ambos antes de la autoprueba completa y de la batería, que los ejercen tal como quedan.

## Desviaciones frente al kit de la ronda 1, con su motivo

- **Autorización PENDIENTE en lugar de fijada** (§94.6): la ronda 1 se entregó con §93 fijada porque §93 era a la vez la orden y la autorización;
  aquí §94 prohíbe la revisión sin una disposición nueva, así que la prueba en seco termina bloqueada por la compuerta 11 y el kit se tendrá que
  volver a congelar tras la fijación (abajo, «Lanzamiento»).
- **Run de tres archivos más las copias fieles:** vuelve una premisa de la ronda anterior (a7-r1.md, como a6-r3.md en la ronda 4 de A-6), con su
  blob comprobado en la comprobación 7, en verify_clone y en el cierre.
- **Lectura directa:** AUTOMATION_PLAN 16.8 L616-L617 (la definición de «exige RED» que A7-1, punto 1 (c), y L16 citan) en lugar de V14 B.9
  L2231-L2233; por la copia fiel, porque AUTOMATION_PLAN está marcado (UNMEASURED_CHAR). Código del auditor de la regla sin cambio.
- **Mutación completa** en lugar de la dirigida (decisión (a)); `mutation-targeted.py` retirado.
- **Copias fieles en el run** (fase 1) y **clase UNMEASURED_CHAR** (decisión (c)): el revisor lee 43 archivos por su copia.
- **`finish_kit.py`** documenta los pasos; como en la ronda 1, se ejecutaron uno a uno con sus mismas órdenes y no entero.

## Supuestos no medidos y riesgos residuales

1. **Duración frente al tope de 3 600 s.** La ronda 1 duró 1 416 s (90 turnos; Read 61, Grep 27). La ronda 2 es más acotada (R1-R3), pero el
   esquema tiene más arreglos (diez cambios, cinco temas, tres resoluciones) y 43 archivos se leen por copia (las copias de V14, A-3, decisiones y
   evidencia son grandes y van por tramos de 300 líneas). Estimación: 60-120 llamadas, ≈ 1 200-2 800 s. El tope no se cambió.
2. **Compactación automática del contexto:** el prompt avisa; contaría como un segundo mensaje de usuario (NOT_ACCREDITED).
3. **Que el revisor lea un original marcado** (p. ej., AUTOMATION_PLAN por su ruta del clon): si la entrega se altera es DEGRADED y motivo; si no,
   informativo (decisión (b)). El prompt lo prohíbe y da las rutas de las copias.
4. **El conjunto medido es el de r1** (34 puntos de código en 4 374 líneas); un punto de código fuera de él lleva el archivo a la copia, donde va
   como `\xHH`: más bytes que leer, ninguna pérdida. Los caracteres de las copias son ASCII imprimible o medidos.
5. **El hijo sustituto de la sonda no es `claude.exe`:** la sonda demuestra el código común de lanzamiento y registro, no el tamaño ni la duración de
   la salida real; se repite dentro del lanzamiento real.
6. **Entorno heredado:** el lanzador no pone ni quita variables; lo decide la sesión principal antes de lanzar, como en la ronda 1.
7. **Esquema de 11 063 caracteres compactos** (9 809 en la ronda 1): mismas palabras clave medidas y la misma profundidad de anidamiento
   (DeltaPurity.Changes como Materiality.Triggers); su comportamiento con claude-cli no está medido a este tamaño.
8. **Anclaje:** la evidencia de I-62, los `preflight.md` de las dos rondas, las disposiciones propuestas de O1-O8 y las notas son de la sesión que
   preparó A-7; el prompt los presenta como evidencia contrastable, nunca como autoridad.
9. **Docstring de `make_closure.py`:** su lista de clases del detector no nombra `UNMEASURED_CHAR` (sí la nombran `fidelity.py`, el cierre
   (`ReadPolicy`) y el prompt). Solo documentación; no se tocó para no cambiar un componente después de generar el cierre.
10. **Rendimiento y energía del host:** durante la autoprueba hubo procesos de otras aplicaciones del host con uso alto de CPU, cinco suspensiones
    del host y tramos con batería (abajo, «Host»); los tiempos lo incluyen. Ninguna suspensión cambió un resultado: los cálculos no dependen del reloj.
11. **`pin_authorization.py`** conserva su docstring histórica (§85, ronda 4) para no cambiar su SHA-256; su lógica usa `owner_gate.BASE_SECTION`
    (ahora 94).

## Host: I-64 y checkout principal (solo lectura)

- `prep/p3/host_checks_r2.py` (git -C con GIT_OPTIONAL_LOCKS=0; procesos por Win32_Process) a las 2026-10-22T18:13:56Z (`prep/p4/host-checks-final.json`):
  checkout principal en `main`, HEAD `95690c28dc6268e61dff32a0cbc33cc9fde3d47f`, 0 líneas de estado, origin/main local `bb0d5522e8411f66a51fdfb3f1f0d0514b737453`; worktree de I-64 en `09f2f1c3` con 0 líneas
  de estado; worktree de I-62 en `b432c40f` con 0 líneas; clon de la ronda 2 en `b432c40f` con 0 líneas (con ignorados); clon de la ronda 1 en
  `5158e4bc` con 0 líneas; run de la ronda 1 con 3 entradas y la última modificación el 2026-10-19T05:53:50Z (no se tocó); procesos vigilados: acad.exe (iniciado 2026-10-22T10:40:12), que esta preparación no lanzó; ninguno nombra la ruta de I-64 ni un clon r62-arch-a7* en su línea de órdenes; memoria libre 23.6 GB,
  CPU 43 %.
- La preparación no tocó el worktree de I-64 ni el checkout principal (solo `rev-parse` y `status` con GIT_OPTIONAL_LOCKS=0).
- **Energía:** el host se suspendió cinco veces durante la preparación: la noche del 2026-10-19; de 16:14Z a 18:28Z del 2026-10-20; de 20:36Z del
  2026-10-20 a 14:09Z del 2026-10-21 (ahorro de batería, con hibernación); de 15:20Z a 16:52Z del 2026-10-21, ya con corriente (espera moderna: una
  petición de energía por proceso sin ES_DISPLAY_REQUIRED no la impide); y de 19:03Z del 2026-10-21 (cambio a batería) a 16:35Z del 2026-10-22. Desde el 2026-10-20 la sesión retuvo la suspensión por inactividad mientras
  corrían la autoprueba y la mutación, y desde las 18:35Z del 2026-10-21 también la pantalla (peticiones de energía por proceso, sin cambiar ajustes,
  liberadas al terminar la cadena).

## Lanzamiento (no ejecutado; exige una disposición nueva)

1. Una disposición del Coordinator POSTERIOR a §94 que nombre esta revisión (RunId R20261019T201030Z-d1bf, objeto 8a528133) en las decisiones de
   la rama; ninguna sección posterior a ella hasta lanzar y auditar.
2. Fijarla: `python -B kit/pin_authorization.py <texto de decisiones de la HEAD> <número>` (Ready: número posterior a §94, una sola sección, blob
   completo como token, ninguna sección posterior) y sustituir SOLO las tres líneas AUTH_* de `kit/owner_gate.py`.
3. Volver a congelar: `python -B kit/make_manifest.py` (OwnerGateTested registrará que owner_gate.py cambió desde la autoprueba y la mutación; la
   autoprueba de la compuerta 11 y la sonda L00 en estado FIJADO conviene repetirlas, como en la ronda 1, porque ejercen ese archivo).
4. Desde esta carpeta, en la misma herramienta y entorno del lanzamiento: `python -B kit/launch.py --record-probe` (código 0) y
   `python -B kit/launch.py --dry-run` (código 2, `BlockingChecks` = []); después, UNA vez, `python -B kit/launch.py` (tope 3 600 s, sin reintento)
   y el auditor sobre la corrida completa:

```text
python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a7r2\3dd5ac31-ddf5-4e4a-b732-b913933a9ae3.jsonl" D:\r62-arch-a7r2-run\launch\stdout.jsonl D:\r62-arch-a7r2 D:\r62-arch-a7r2-run kit kit/result.schema.json audit.json output.json
```

El auditor informa por separado `Categories.Execution` y `Categories.ArchitectVerdict`; el veredicto no es el acuerdo. Ni A-7 AGREED, ni Controller
ELIGIBLE, ni FX-02 PASS, ni F6 GATE PASS se declaran aquí.

## Archivos y privacidad

`kit/` es el kit congelado (manifiesto: `kit/kit-manifest.json`). Junto a este README: `record-probe.json`, `launch-dry-run.json` y
`launch-dry-run-before-full-mutation.json` (salidas de la orden congelada, convertidas a UTF-8; bytes crudos en `prep/p4/`). `prep/` es la preparación
y no forma parte del kit: `p2/` (fase 2: fuentes, esquema, notas y salidas), `p3/` (diffs frente al kit lanzado y a la fase 1, matriz, negativa de
verify_clone, comprobaciones del host, privacidad, tabla de archivos), `p4/` (sesión principal: correcciones, cadenas, registros de tiempo y salidas
crudas) y los archivos de la fase 1 (`phase/`, `diffs/`, `p1_*.py`, `test_*.py`, `validation-matrix.json`). Fuera de la custodia: `base-launched/` (el
kit de la ronda 1, ya custodiado en e71400a2), `phase1-kit/` (sus SHA-256 están en `prep/p3/diffs/diff-table.json`), `prep/dev/` (ayudas de
desarrollo), `prep/p3/vcneg/` (temporal de la negativa) y las copias previas a las correcciones (`prep/p4/*.before-*.py`; los diffs bastan).

**Copias fieles en la custodia del repositorio:** las 43 `kit/fiel__*.txt` siguen en el kit local (son parte del manifiesto y del run) pero no se
custodian como archivos: cinco de sus rutas, dentro de un worktree bajo %USERPROFILE%\.codex\worktrees\, pasarían de 260 caracteres (MAX_PATH;
`core.longpaths` no está activado), y cada copia es función pura de su original. `prep/p4/regen_faithful.py` las regenera desde `b432c40f`
(`git cat-file blob` del clon y `fidelity.encode` del kit) y comprueba, para las 43, el blob del original, el SHA-256 del cierre y del manifiesto,
la igualdad byte a byte con la copia del kit y `fidelity.verify` sin problemas: **AllReproduced = true** (`prep/p4/faithful-regeneration.json`).

**Privacidad:** `prep/p3/privacy_scan.py` (patrones leídos del entorno y de la configuración global de git en tiempo de ejecución) sobre `kit/` y
`prep/` (`prep/p4/privacy-final.json`): 260 archivos, **0 coincidencias** fuera de las copias byte a byte del repositorio (`fiel__*.txt`,
`a7-r1.md`: 3, exentas porque conservan los bytes de archivos ya versionados); la custodia comprueba además este README y las salidas de primer
nivel. La única coincidencia previa, la ruta corta del perfil en `prep/p4/wait_ac_then_chain2.sh` (la del script de retención en el scratchpad), se
redactó antes de la custodia y el script se copió a `prep/p4/hold_awake.py`. Las rutas del perfil se escriben como %USERPROFILE%, %LOCALAPPDATA%,
%APPDATA% o %TEMP%.

```text
SHA-256                                                              bytes  archivo
05236d172a87dd437cb7b1df5c302aca94bc8d4b53d68d591b18d4f03a62b90c     15483  launch-dry-run-before-full-mutation.json
65d79e55f103bcf41b6e195dc4fceaf2ae55ecc0f85ffb991844eb944be2246b     15483  launch-dry-run.json
990a7bd43773673beada9d16f9046097e324a7f5d028372c10628c56d0cc1939      2919  record-probe.json
1ab7315ad0a2592b4fef50de28c54dc5904123771dd59d24c9768e6a309b5e6f     27472  kit/a7-r1.md
4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7      6070  kit/c3_analysis.py
4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c     17086  kit/claude-cli-characterization.json
845886e97a854e7cddeb80a2643d59a3acf0048f3109ef69f9e983a1cd14d3c3     85225  kit/clone-verification.json
d19950bba9a19a8136a8301aa4535ffbdfc07e633f599a85033cc764d9a14e91    387778  kit/closure.json
a84825e00112965f33c7530ffac6ff84353ff036f2773ee525ebd178e872c68f     21072  kit/fidelity.py
0c9819346b6c7aabda5e61170aa0e389f03a822688170c9cbf02a7860c708ed7     11340  kit/finish_kit.py
3499596df4292720a4390aeac7270a8f16ed2708c12590d0b3a3ae37e57425c5     30464  kit/kit-manifest.json
86e4266095951624a1ba09ceeddd2fc67b84010e3edea2dfeb294b67ade20184      2680  kit/kit-params.json
17dda7259766ac645fd350c112609f678e7ec8ef477ef4783faaa1870e93e6c9     48212  kit/launch.py
756459c09e7c88899c4e7cb1450ae8ac8eec548ca9f5dd3ebff8409f9112fa59    103600  kit/make_closure.py
a7a7f6b3a7af2e212c91c1c49299f02e54b4def29a9a840b6bb364dfbf23ad22     10674  kit/make_manifest.py
8c275526bcfab0d295e74775bc9825fa508f1fe1e29404aaec71439b373f9007     11103  kit/mutation-post-review.py
0215a272f13d0c2ae823f08b5495a2900d682e1e8cc74d2c43105318d0f2c85b     66912  kit/mutation-result.json
515ccfa37205aa1d9d1fccbd3eb3c2ed0c2b56b8bb67c56102d1a8b0bea3db0e      3522  kit/order.txt
3d420a9ff560c5bd88f811edff878da3f2d8056a531e9429b6412b82e5e80b19     18007  kit/owner_gate.py
a8e4070751ae604010149c8678aa9320766c533230bf92de3aa2e88ea6e571cc      5588  kit/pin_authorization.py
f5cdef1ff2ca9d0217b641a2cf6adadbe909785eed75fba4b0fe8573fa467381     73234  kit/post-review.py
23ee99ef1a5e6a9232d0aed23f93559b25f5315626cfa2b2c453b35f4aff4da7     11362  kit/prepare_clone.py
40c9de64b2c9d25bf9d69ee20eef1c72be578775ac3e8c3ca94e5e325d7eef2c    103431  kit/prompt.md
24b93624bf00ed378f47bd336cc28db6117c1e4eb200157d000c9320b0d86aab     37807  kit/prompt.template.md
da1457a1a035221ba41710f1bea6bcd28312c95d66199885accccf3a21f11cfa      6143  kit/read_fidelity.py
9e53331784d4dd5ca4afb371479190c0882e70ec1bdcf196b71be28d3a23a6a3     16544  kit/result.schema.json
df0506164f5019276479cd0d9039a5975974faf2a592b233f477de29c544b890      9062  kit/selftest-fidelity-result.json
0ad71f0d3f3bd4be6986d0851910a095ff573e48193e9c106d7c53fefffd0da3     18240  kit/selftest-fidelity.py
435f76f4dd00aee1c6047cea2ef463b8b524470635296414d6f6d70b6d6ac153    147523  kit/selftest-post-review.py
2edc585526ad07b2c43508f123f2137150d7f0bd167e6795a92eb7e72d48ff5e    171161  kit/selftest-result.json
a6cff4798f23c66fb505d06d4c67ed330830e41df6fc9c7f94c31968def14992      1544  kit/transport-gate.json
e326b5b92da5ff2b53d05a7d64f90dbb09bf098ab4eeda5a92597d6fc36e6fbf     22485  kit/transport_gate.py
fb3d8f133a2337b3119217658c72e00d125fa3c068e38814a7f944d910f0d19e     27921  kit/verify_clone.py
f023fd8fa1a2cb6d6df15abfc8da79b45edb14568de0f0474451b7aab7fcc7d0   2449125  kit/fiel__*.txt (43 copias fieles; SHA-256 de la concatenación de sus SHA-256 en orden de nombre)
9f71990474d5d36b5a9a6227de79b020d871c79c107b1fb06123fcaa46cd7d83      3719  prep/p2/auditor_doc_head.txt
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855         0  prep/p2/closure.err
4045a883578bc0cd8efc03650f495ce27c84d5ee26a9182631c17073aebfca5f     27423  prep/p2/closure.out
d857d16f2b53b9b55692beff8e3c382d491293b60a81aa7e259772a8b5a5e879      8875  prep/p2/coherence_fn_r2.txt
e73bea5db08459db047c01b753ab1d7a17f18d3096008be66ef3ec6e56d4ed54       811  prep/p2/coherence_r2.txt
1ddb7223ec81686e1fbb730577c01c7e05cdbd14219ff205935d50d445769a0b     15586  prep/p2/faithful-copies.out
d1b8671488a4409674713e30851fa15865658fb2746faf53b597f650f4d16bfc      8754  prep/p2/gen_schema_r2.py
8cefeac4a1168adc35d2b954cec703246d568641fc2db69b0d0c837de8ea2f75      9460  prep/p2/identity_checks_r2.txt
54e457ded6e169f866fb6f660bf8d82fbfd5e341beb4672e27760a2cc31a7ea0       501  prep/p2/ids.txt
3c69d779777067242319e8819f8e0531246ad623f5f03beb904f9cb477175818      1436  prep/p2/r1-literals.json
5aac4bca29dfc98ed9571411dd955506a68053b9074a707d984ea4f6a4715282      9919  prep/p2/render_prompt_r2.txt
df0506164f5019276479cd0d9039a5975974faf2a592b233f477de29c544b890      9062  prep/p2/st-fid-dev.json
9033dabae8d0885d4fa4faa50822515bfdec7072815354eb7392d7c16cab2c67      7297  prep/p2/st_base_r2.txt
7edb96b4b14e080dd9d556bc86874db0d2f24b8e11e6035834b4829489abe070     23328  prep/p2/st_cases_r2.txt
23a421d4b4f2a3408f87a47ebcfd6f91e0078412b304995c3977ee838d946b9e      3919  prep/p2/st_doc_r2.txt
b93fdf5e2378ef4814cfaa81d51609af59ac623c7bbddb46e4f1c20b53566efa     11721  prep/p2/st_owner_r2.txt
01dd1815b71be9f34adb5cdafa5b037191116e746ed76479228b5f106f95fd30      3862  prep/p2/verify_clone.out
4d8674aebd6673c3ca5a0ba3eca73bd2fe38dbbae7c5f3ce4767feb4d5f7dcd8        21  prep/p2/verify_clone.start
64129da1dc90fc5c608dbd1a68605182fb3e4e3bdd21d8892da0bd1c5fab64ed       620  prep/p2/phase/gate-check.out
1e2672f977a5a19e6f309868b3e884bef45c5ad1f5d2818a9cda0d45d99c7dad      1007  prep/p2/phase/host-checks-during-selftest.json
ce7990be338c3dd936e466c3fa9484417848559518f85e3cd84f5cf15d6154be        21  prep/p2/phase/selftest-fidelity.end
e218f5beb31518c41eb85236a6fe501ab71b2c1af3a8b3a68f3d04304472010e      3349  prep/p2/phase/selftest-fidelity.raw
c5748c8ba8ace7d823a692c3c239c22e02488fcf25165218b58e7f16b23bdebe        21  prep/p2/phase/selftest-fidelity.start
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855         0  prep/p2/phase/selftest.err
cf205dbb8cea84897b488abcc281bf96698d5e94b1096b16657b4caba9082a22         7  prep/p2/phase/selftest.raw
ca933493cdd74c783e2f49feb26cb022576abac51c4fb2b2ee203cd9e937bfd2        21  prep/p2/phase/selftest.start
ac6a6ebab163150a949b53478bab12a5e1c81f80cf2ef64dbff4550a3c26e3f6     51402  prep/p2/retired-r1-artifacts/mutation-result.json
d1ed12225973bf87cc615c48fc54871075de02a3a98fdb4999321e4a6bb24b81     18496  prep/p2/retired-r1-artifacts/mutation-targeted-result.json
1a3488e4adc002aaf9d0e711807db5909d7947d1d7b046a7871137709117180b      7652  prep/p2/retired-r1-artifacts/mutation-targeted.py
1ab7315ad0a2592b4fef50de28c54dc5904123771dd59d24c9768e6a309b5e6f     27472  prep/p2/src/A-7-r1.md
dbb7db53c5668afdab8bf8cdf0f4b005b4ecdcd7747606015d1d8f7814426d9a     37205  prep/p2/src/A-7-r2.md
b905d1d9e959cbdbe5e2572e04f7176db7b76b526cbacbc7342bfaddaee6273d      3901  prep/p2/src/dec94.md
2f6c63f270745eb101a7fbce7eb777d617e32dce5d27a35fabdf40e62fa34443      5241  prep/p2/src/optional-dispositions.md
ce60758136bc7dc61e0cec749abb3439dc7d5af5f02407268944d7deccde1ed5     53365  prep/p2/src/r2-preflight.md
10ba82bb29be3381054468ef3483417c29bc140707dd8336cd396d83f64e167f     90683  prep/p2/src/run_a7r2.py
f73d63255448c4b3a8fdc323075f5535f943d5e29fd707fa45255a11bd16cd90       668  prep/p3/closure_detail.py
db2b58d5b7ad33c6f20ea93dc6bae324f4dc6f5ffc11fcb864c82c7d2b1bfba3      1428  prep/p3/closure_summary.py
ae34682cc90defe62a42daca33db365b2de19d025c222e9f922ae0c2fc741e3c      1705  prep/p3/file_table.py
83e66336b075a398a9754ffda246620996613cc5639dc21dbb9c527992702a4c      4332  prep/p3/host_checks_r2.py
125e13f7d847e7546ed58d7a09c3211bcf9a777c47d55fbfb4a7339c05c29104      3581  prep/p3/make_diffs_r2.py
1c80db30c4515da3522bcb4a17878275c80251348a8914d204a724139eb50712      8671  prep/p3/make_matrix_r2.py
adf43a5c8229f05b347822d8ee90213719ebbbcdeb64cacf5f8a4b0e24a2e2c1       747  prep/p3/neg-verify-result-r2.json
baf208eccf07cdfead1fe42977db8dd8bbed4cb56aac8f78020073608a78c251      4462  prep/p3/neg_verify_clone_r2.py
3ffe921e7abd3e76959b1cfda2b3a3ef27fed590d82bbfc4d6bcf95205511e1a      2406  prep/p3/priv-pre.json
d66d662ebebcbb716078ce22c0580bbee8a8474998a4cd909340ee46b5801150      3147  prep/p3/privacy_scan.py
7e0c2016281b12754b3a946ec4b492a8df25c546203df30d65ffa727887b2b00      2173  prep/p3/scan_run_files.py
2039043ceb5682d5df5cd19713146c9f81c475b81cd8a72a644d0515a410dbb3     16378  prep/p3/validation-matrix-r2.json
9d4bfe6bc2344ad4b752efd50b9ee3f4bb580ea1787b61c09332c29d24846d08      6849  prep/p3/validation-matrix-r2.md
fd1062b42756c35b40485012d9f062e559a1bd02e883d3a71f7c3168aacb89d2     12544  prep/p3/diffs/diff-table.json
352d917eae49e40ec9cbf2c74bffac27d1c17dd6c8edaa69cef9f205d497cf58     27809  prep/p3/diffs/vs-launched/a7-r1.md.diff
ed7d1b43feca82a8fce686ec40448c82330e46a2af494536dba5bd9e66a31d25     21534  prep/p3/diffs/vs-launched/fidelity.py.diff
de183dd04d3a1a9436ddecfa4d5d58d48d017affe7b7aa3dfa19dec0a4afb4b4     14979  prep/p3/diffs/vs-launched/finish_kit.py.diff
33aa0ca7479e40f4cec9ac942a1d3e5bfd334925ecf6d8345ae64f266ca8a3b7     13548  prep/p3/diffs/vs-launched/launch.py.diff
fc8e0ae2d813d3e6e83e5b73aa80c6e92ba16ea5dd6492b25bec04aa551a5faf     98479  prep/p3/diffs/vs-launched/make_closure.py.diff
9c6cbc2689276802b6a4d5951a010deec625178fb0758fb211f37d78e7293d10     12595  prep/p3/diffs/vs-launched/make_manifest.py.diff
8c3c229924dae7b12c8ff29c041f68dae6072e1a88a9da43fe48993c08eff141     10093  prep/p3/diffs/vs-launched/mutation-post-review.py.diff
77611b8203dbb6296ef152155b04b8a99979341ac16044b8ea10fd89301c4bc4      6413  prep/p3/diffs/vs-launched/mutation-targeted.py.diff
d1efac53099e329bafe248b4076ea59eb735531b8135285db9474a7a0ccbc4e0      7666  prep/p3/diffs/vs-launched/order.txt.diff
34463a2a50baef44e469eb45aed445fd0115b0668a5e50a39ba98df14151b6a4     11151  prep/p3/diffs/vs-launched/owner_gate.py.diff
6ffac66ee8cdd14ae629be136eb46db1ce0d70e2a3c239198aeafcc8ac79c7b1     33508  prep/p3/diffs/vs-launched/post-review.py.diff
07b565ea19e3e5fbe53aac5baef439abf4831d5af7e8ccf09f6186e7f382aee5     13185  prep/p3/diffs/vs-launched/prepare_clone.py.diff
dac91aa7862f83e8c4255617029b52206f9a30a5bafcd07b1b79b82a7260ffb6     64304  prep/p3/diffs/vs-launched/prompt.template.md.diff
4b17a7e1a9ee9050256e5c1325dbe34e8304046f64a0fd18af868e4eae580074      3405  prep/p3/diffs/vs-launched/read_fidelity.py.diff
ae83cfbb6a200b7470d90348d7cd2829c1f355d38f68f5df342e8dd6b19cb35b     18589  prep/p3/diffs/vs-launched/selftest-fidelity.py.diff
340051f8f37a7874df988a9051ea41a49ddfe7e78024c011342109cd4efc83cd    101628  prep/p3/diffs/vs-launched/selftest-post-review.py.diff
703e52491e7a443bbdbcd486eca1f78109aeb220768d3bd5f9dd0f21f8ac02c9     25936  prep/p3/diffs/vs-launched/verify_clone.py.diff
dcce562df6eded24d38ab84546d7fd17b4fc935ae78c21b829f53b4912c3ef6a     27807  prep/p3/diffs/vs-phase1/a7-r1.md.diff
586f77f52020d243110bc3b42d6a7b0a214fdfeb1868ac46862a2184ffacddf8      4491  prep/p3/diffs/vs-phase1/fidelity.py.diff
b1a1bd54d89ec386de52b7ad1c2a83836a989cd1903c28ae64c4f30ee34bdce4     14792  prep/p3/diffs/vs-phase1/finish_kit.py.diff
4eefe57c02e8f0279521421ae421ac4b70ad1c24c08de7fa83232823526c3de9     11936  prep/p3/diffs/vs-phase1/launch.py.diff
fd6c91b56fb2db104fdaa8a4ab693b124c6f78a83a6ee0601a8125c3bde8e902     83503  prep/p3/diffs/vs-phase1/make_closure.py.diff
f15550e40c3eb9fa516363b533e6257ffcad438dee40344488c02b8b9125eda1     11043  prep/p3/diffs/vs-phase1/make_manifest.py.diff
cab007de30c6de7be4d6780814d78d76a1edce31165193c587293c4085713210      3256  prep/p3/diffs/vs-phase1/mutation-post-review.py.diff
1f66c486ee93d3050e90b2e7f769ead89313966f570a12c81ac18f9b38b4331b      7839  prep/p3/diffs/vs-phase1/mutation-targeted.py.diff
5fc0b6bd5c23a0e42c555c2a23eabc929a8106f2c7ed935cc2290607550d509e      7664  prep/p3/diffs/vs-phase1/order.txt.diff
22e3ff543f5c92df1a75ce6ed309a29d07afe68573e4a4d48926a064a2c7d91b     11149  prep/p3/diffs/vs-phase1/owner_gate.py.diff
c55f51c0812c4024b67825cd79f0411cbba9906894acc96b575ef7aa98b9e84f     28608  prep/p3/diffs/vs-phase1/post-review.py.diff
eb2b47a9dd73ddfca8d374a50009f2dc1d6e37a1b1282677a853098010f5065a     13183  prep/p3/diffs/vs-phase1/prepare_clone.py.diff
43a76c975bbc5dcedc472a3d80569aab84ed2c74f1e6259364e668bf3a307e20     64568  prep/p3/diffs/vs-phase1/prompt.template.md.diff
c2fae78324da4c30bba6935a906eaa1098a4ba0c4c3cda70584197bfc8795d88      2355  prep/p3/diffs/vs-phase1/selftest-fidelity.py.diff
c61965e3a3e0db68352e3ad3b4f1de6614cad171c2d90e0aca73a34899039be7     86679  prep/p3/diffs/vs-phase1/selftest-post-review.py.diff
f5d6f5be851f917efd552f38472e2c91fc280a3fc593f3ac6e2fca332d2ed2a7     23490  prep/p3/diffs/vs-phase1/verify_clone.py.diff
```
