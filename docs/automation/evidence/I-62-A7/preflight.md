# I-62 A-7 — Preflight de la candidata (ronda 1, corregida)

> Supervisión, plano a. Preflight de la candidata `docs/initiatives/I-62-A-7.md` (blob `6c29747d61c5f689a195095ad3712e6d9dba62aa`, PROPUESTA ronda 1, corregida dos veces en sitio por el Coordinator tras los preflights sobre los blobs `a2aed3cc894037d6b42d462fbec9d6d0c7f629a6` y `df8925b26229e7b678d450faa9e983cb5ef6cc13`; no enviada, sin revisión) sobre la base `bb439090f6798a502c077da450edc1319e416fe5`, según decisiones §92, puntos 2, 3, 6 y 7. Aquí solo hay pruebas, resultados y hallazgos; ninguna regla se decide. No se ejecutó Codex, claude ni ningún CLI de modelo, no hubo red ni `gh`, no se leyeron credenciales, no se creó B1 ni ningún repositorio en GitHub y no se publicó nada. El instrumento (`9a829b0e…`) y el aceptante (`434ef1fc…`) se usaron solo desde sus objetos Git, sin modificar. Todo Python con `-I`. Punto de entrada único: `python -I run_a7.py all`.

**Resultado global: 160 de 160 casos se cumplen.** Todos se cumplen. Guardas sobre el commit hipotético: **PASS** (G1-G7); autoprueba: **PASS**, 44 de 44 mutantes muertos.

Versiones: git version 2.54.0.windows.1; Python 3.13.14; pwsh 7.6.7; Windows-11-10.0.26300-SP0.

Las lecturas del texto corregido son las predeterminadas del constructor, del validador y de la precondición. Los conmutadores de las lecturas de la ronda 1 antes de su corrección (`--absent-a=no-closure`, `--path-rule=strict`) solo se usan como evidencia negativa (casos `*-OLD-*`).

## 0. Entorno e identidades (`p0-env.json`)

Componentes reutilizados, copias byte a byte (SHA-256 comprobado contra la fuente): del banco de la medición `make_eic_verify.py` (R7-A), `derive_r4.py`, `designate_m7.py` y `reconstruct_runs.py`; del kit `a6m7-measure`, `reconstruct_runs_m7.py`. El repositorio de la entrega del banco (`D:\r62-a6r2\r3\deliv`) se copió a un área propia para los commits sintéticos de los negativos.

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| ENV-W | HEAD del worktree de I-62 (solo lectura) | bb439090f6798a502c077da450edc1319e416fe5 | bb439090f6798a502c077da450edc1319e416fe5 | sí |
| REUSE-derive_r4.py | copia byte a byte de D:/r62-a6m7/bench/derive_r4.py | SHA-256 3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426 e igual a la fuente | SHA-256 3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426; igual a la fuente: sí | sí |
| REUSE-designate_m7.py | copia byte a byte de D:/r62-a6m7/bench/designate_m7.py | SHA-256 491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6 e igual a la fuente | SHA-256 491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6; igual a la fuente: sí | sí |
| REUSE-make_eic_verify.py | copia byte a byte de D:/r62-a6m7/bench/make_eic_verify.py | SHA-256 58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2 e igual a la fuente | SHA-256 58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2; igual a la fuente: sí | sí |
| REUSE-reconstruct_runs.py | copia byte a byte de D:/r62-a6m7/bench/reconstruct_runs.py | SHA-256 5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da e igual a la fuente | SHA-256 5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da; igual a la fuente: sí | sí |
| REUSE-reconstruct_runs_m7.py | copia byte a byte de <W>/docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/reconstruct_runs_m7.py | SHA-256 1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab e igual a la fuente | SHA-256 1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab; igual a la fuente: sí | sí |
| PIN-instrumento | instrumento desde su objeto Git del repositorio de I-62, sin modificar | blob 9a829b0eeb1210238052b1feac06cfa8f7964aed | blob recalculado 9a829b0eeb1210238052b1feac06cfa8f7964aed; 64764 bytes; SHA-256 b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200 | sí |
| PIN-aceptante | aceptante desde su objeto Git del repositorio de I-62, sin modificar | blob 434ef1fccb0d15762b30244b4a60928120698411 | blob recalculado 434ef1fccb0d15762b30244b4a60928120698411; 111873 bytes; SHA-256 173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0 | sí |
| CAND | candidata A-7 leída por completo | blob 6c29747d61c5f689a195095ad3712e6d9dba62aa | blob 6c29747d61c5f689a195095ad3712e6d9dba62aa, 27472 bytes, 279 líneas, LF | sí |

## 1. Constructor de A7-1 (`make_eic_a7.py`)

«R7-A más»: importa sin cambio `make_eic_verify.py` (tabla de obligaciones, punto fijo, listas y su orden) y solo cambia la revisión en que fija cada entrada (A7-1, regla 2) y el tratamiento de las ausencias (regla 5). Un blob por revisión de lectura en que difiera; un par ya fijado en `CanonicalInputs` no se repite.

| Categoría | Entradas | Revisión | Ausencia |
|---|---|---|---|
| (a) | citas UNIT_DOC / UNIT_CHANGE; `AGENTS.md` (instrucción automática); transitivos del punto fijo (WORKFLOW, README de agent-execution, esquema de la entrega, AUTOMATION_PLAN por «Leer primero») | `AuthorityRevision` | sin entrada, registrada (16.3, 16.24); `AGENTS.md` o AUTOMATION_PLAN ausentes en la AR: sin cierre (el punto fijo de 16.24 del perfil del kit no se puede calcular) |
| (a) | citas EXTERNAL; `WORKFLOW.md` y `AUTOMATION_PLAN.md` en Evaluate | `MainSha_eval` = `K.MainSha` | sin entrada, registrada |
| (a) | mapa, esquema y cada MODIFIED del mapa; `WORKFLOW.md` y `AUTOMATION_PLAN.md` en Validate | EFF = Derive(`MainSha_eval`) | sin entrada, registrada (16.13: MAP_INVALID) |
| (a) | cada MODIFIED del mapa; `AUTOMATION_PLAN.md` en Validate | EFF^1 | sin entrada, registrada |
| (b) | objeto exacto; rutas de `git diff --name-only D.BaseSha..Target.commit`; `decisions/<UnitId>.md` y `state/<UnitId>.yml` | `Target.commit` | objeto ausente: sin cierre; ruta borrada o registro ausente: sin entrada, registrada |
| (c) | contrato, delegación, entrega, TRX en las rutas de la designación | blob Git de los bytes custodiados (y el registrado, si la designación lo trae) | contrato o delegación ausentes, de otro blob o ilegibles: sin cierre; entrega o TRX: sin entrada, registrada, nada sustituido |
| (d) | `{ruta de procedencia, 9a829b0e…}` y su acción (A6-2) | blob de A-6 | solo si la designación lo admite |

Los archivos ADDED y ENTRY del mapa (Validate solo compara su blob, MV-4) no son entradas: en FX-U1 serían 23.

## 2. Validador (`check_a7_closure.py`)

Escrito desde el texto de A-7 (§3.2, §3.3), sin importar el constructor, `make_eic_verify.py` ni `derive_r4.py`: calcula sus propias referencias desde la designación y el `-C` (su propio Derive de EFF, su propia lectura de K, D y H). Cada entrada de las tres listas debe pertenecer al menos a una categoría (si no, FOREIGN); pertenece a una categoría cuando su blob es la referencia de esa categoría para su ruta, así que una entrada en varias categorías coincide con cada referencia (OVERLAP, admitido y registrado). Exige las entradas obligatorias que existen (MISSING); rechaza duplicados dentro de una lista con el mismo `RequiredBy`, (b)/(c)/(d) fuera de `CanonicalInputs`, acciones del instrumento no admitidas, el `RequiredBy` de los transitivos que no esté en la AR y la regla de rutas de 2 (c) (bajo `artifacts/orchestration/`, en un directorio con el `RunId` de los bytes); con el objeto del `Target` ausente, o sin contrato o delegación utilizables, exige que no haya cierre (NoClosureExpected). Una ausencia en (a) no es obligatoria y se registra. Admite la misma entrada en varias listas y una ruta con un blob por revisión de lectura.

## 3. Reproducción sobre R7-A (§92, punto 7) (`p1-repro.json`)

Para cada designación del banco: E de R7-A reconstruido con la copia del constructor del banco (y comparado con el E custodiado), E de A7-1 con el texto corregido, validación, y la precondición de §3.5. En FX-U1 los blobs coinciden en la AR, EFF, EFF^1 y `MainSha_eval` (EFF = `MainSha_eval` = `fbe25347`, EFF^1 = `930288c5`); `PROMPT_TEMPLATES.md`, MODIFIED en el mapa, no existe ni en EFF ni en EFF^1 y, con la regla 5 (a) corregida, no aporta entrada (Validate dará MAP_INVALID).

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| REPRO-V | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco V.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (256d841be5afcb1c…): sí; E A7 SHA-256 256d841be5afcb1c2c242cddfb0e335b2050bfdb056f1e1e9da8ba6fd8304e06, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-N4 | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco N4.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (0df49e601d881413…): sí; E A7 SHA-256 0df49e601d88141356d560fbf9ffa37ed9e48989e8b64d7d0b91af666f68e30b, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-NC1 | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco NC1.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (413e22d85f07be72…): sí; E A7 SHA-256 413e22d85f07be7288f19d7ba168c540d297eb98c4c3d79c12abbe2c28653966, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-NC2 | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco NC2.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (9072bf06e7239802…): sí; E A7 SHA-256 9072bf06e72398021b1ab6f923b30997ff6dca0f0683ab7cc9d5747199370db0, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-NC3 | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco NC3.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (393a953af1619477…): sí; E A7 SHA-256 393a953af16194779a6a5e1aa98e2e95df8b91a0aa50ef920377c1062a5653fb, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-N5 | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco N5.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (759e200c1f9d19f4…): sí; E A7 SHA-256 759e200c1f9d19f4b8df9099ee0250a8a46b23bf5217c3608843c1146c4470bf, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-N6 | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco N6.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (67e32328066986a4…): sí; E A7 SHA-256 67e32328066986a480199b7076c9a78abfa7670f0ae90939cd3af845d341e020, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-N7A | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco N7A.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (7a62191cb3071947…): sí; E A7 SHA-256 7a62191cb307194786203191512bb139f197f79d450c0549b9692bec4ccd8e00, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-N7AR-OWN | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco N7AR-OWN.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (e4f26b06782651a3…): sí; E A7 SHA-256 e4f26b06782651a3f596d6698bc367bfb9192d234ba976f03919673427dcb95b, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-N7B | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco N7B.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (4dddb72fa66fc648…): sí; E A7 SHA-256 4dddb72fa66fc648f4fb2d3ca64fa714359a4c43c445d1da4e4ed6e833a960ad, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-M | E de A7-1 (texto corregido) frente a E de R7-A (designación del banco M.json) y validador con las lecturas del texto | E A7 = E R7-A byte a byte (A-7 §3.1, §3.4) y el validador lo acepta | R7-A fresco = custodiado (fec2ea0dfb05aee1…): sí; E A7 SHA-256 fec2ea0dfb05aee193946177e2eb78d4f3e35b7a801ec383cfa92aa20959e0be, igual a R7-A: sí; ausencias (a) registradas sin entrada: PROMPT_TEMPLATES.md@EFF, PROMPT_TEMPLATES.md@EFF^1; validador: acepta | sí |
| REPRO-OLD-5A | evidencia negativa: las once designaciones con la regla 5 (a) de la ronda 1 antes de su corrección (una entrada de (a) ausente dejaba el cierre sin calcular), conmutador --absent-a=no-closure | sin cierre en las once (el motivo de F1); el texto corregido no lo hace | sin cierre en 11 de 11; causa: docs/initiatives/PROMPT_TEMPLATES.md absent at EFF, read by 16.13:MV-6 (superseded reading of rule 5 (a): no closure); el validador con esa lectura rechaza 11 de 11 | sí |
| REPRO-OLD-PATH | evidencia negativa: regla 2 (c) de la ronda 1 (artifacts/orchestration/<UnitId>/<I.TaskId>/<Attempt>/<RunId>/), conmutador --path-rule=strict | rechaza las copias de los controles en T1-<control> (el motivo de F2); el texto corregido las admite | rechaza 9 de 11 (N4, NC1, NC2, NC3, N5, N6, N7A, N7AR-OWN, N7B); con el texto corregido, los 11 cumplen la regla 2 (c) | sí |
| REPRO-UNIQ | regla 2 (a) corregida: «una misma ruta puede figurar con un blob por cada revisión de lectura en que difiera, y una misma entrada puede figurar en más de una lista del cierre cuando 16.24 lo exige» | los pares repetidos de los E (heredados de R7-A) solo aparecen en listas distintas o con RequiredBy distinto; ningún duplicado | 3 pares repetidos: AGENTS.md en CanonicalInputs+AutomaticInstructions; docs/WORKFLOW.md en AllowedTransitiveInputs+AllowedTransitiveInputs; docs/automation/agent-execution/README.md en AllowedTransitiveInputs+AllowedTransitiveInputs; duplicados dentro de una lista y con el mismo RequiredBy: 0 | sí |
| CIT-FXU1 | precondición de §3.5 (check_citation_blobs.py, criterio de cobertura) en las once designaciones del banco: toda lectura de contenido de clausemap de una cita en EFF^1, EFF o MainSha_eval es de un blob que fija el cierre | se puede lanzar en las once | cobertura (normativo): LAUNCH_MAY_PROCEED; evidencia negativa de redacciones anteriores: texto LAUNCH_MAY_PROCEED, unit LAUNCH_MAY_PROCEED | sí |
| DES-REG | regla 2 (c) corregida: la designación registra la ruta y el blob de cada artefacto; el validador comprueba el blob registrado y avisa del que falta | el formato a6m7-designation/1 del banco solo registra el del contrato: aviso para DELEG, H y TRX en las once (la próxima preparación del kit tiene que registrarlos; el formato del banco no se cambia) | sin blob registrado: DELEG, H, TRX | sí |
| REPRO-KIT | E congelado del kit a6m7-measure (137c7c3b…) frente a A7-1 y al validador | el validador acepta el E congelado; E A7 = E congelado | SHA-256 del congelado 137c7c3bba6591c84fdef4c5f6ef443f72da1cfd871478968c64a5c7cb5a1152; regla 5 (a) de la ronda 1 (evidencia negativa): sin cierre: AUTHORITY_ABSENT; texto corregido: E A7 = congelado: sí; validador: acepta; validador con la regla 5 (a) de la ronda 1: rechaza (NoClosureExpected) | sí |

| Caso | E de R7-A custodiado en el banco (SHA-256) | E de A7-1 (SHA-256) | ¿Igual? | Regla 5 (a) de la ronda 1 |
|---|---|---|---|---|
| V | `256d841be5afcb1c2c242cddfb0e335b2050bfdb056f1e1e9da8ba6fd8304e06` | `256d841be5afcb1c2c242cddfb0e335b2050bfdb056f1e1e9da8ba6fd8304e06` | sí | sin cierre |
| N4 | `0df49e601d88141356d560fbf9ffa37ed9e48989e8b64d7d0b91af666f68e30b` | `0df49e601d88141356d560fbf9ffa37ed9e48989e8b64d7d0b91af666f68e30b` | sí | sin cierre |
| NC1 | `413e22d85f07be7288f19d7ba168c540d297eb98c4c3d79c12abbe2c28653966` | `413e22d85f07be7288f19d7ba168c540d297eb98c4c3d79c12abbe2c28653966` | sí | sin cierre |
| NC2 | `9072bf06e72398021b1ab6f923b30997ff6dca0f0683ab7cc9d5747199370db0` | `9072bf06e72398021b1ab6f923b30997ff6dca0f0683ab7cc9d5747199370db0` | sí | sin cierre |
| NC3 | `393a953af16194779a6a5e1aa98e2e95df8b91a0aa50ef920377c1062a5653fb` | `393a953af16194779a6a5e1aa98e2e95df8b91a0aa50ef920377c1062a5653fb` | sí | sin cierre |
| N5 | `759e200c1f9d19f4b8df9099ee0250a8a46b23bf5217c3608843c1146c4470bf` | `759e200c1f9d19f4b8df9099ee0250a8a46b23bf5217c3608843c1146c4470bf` | sí | sin cierre |
| N6 | `67e32328066986a480199b7076c9a78abfa7670f0ae90939cd3af845d341e020` | `67e32328066986a480199b7076c9a78abfa7670f0ae90939cd3af845d341e020` | sí | sin cierre |
| N7A | `7a62191cb307194786203191512bb139f197f79d450c0549b9692bec4ccd8e00` | `7a62191cb307194786203191512bb139f197f79d450c0549b9692bec4ccd8e00` | sí | sin cierre |
| N7AR-OWN | `e4f26b06782651a3f596d6698bc367bfb9192d234ba976f03919673427dcb95b` | `e4f26b06782651a3f596d6698bc367bfb9192d234ba976f03919673427dcb95b` | sí | sin cierre |
| N7B | `4dddb72fa66fc648f4fb2d3ca64fa714359a4c43c445d1da4e4ed6e833a960ad` | `4dddb72fa66fc648f4fb2d3ca64fa714359a4c43c445d1da4e4ed6e833a960ad` | sí | sin cierre |
| M | `fec2ea0dfb05aee193946177e2eb78d4f3e35b7a801ec383cfa92aa20959e0be` | `fec2ea0dfb05aee193946177e2eb78d4f3e35b7a801ec383cfa92aa20959e0be` | sí | sin cierre |
| kit (M, congelado) | `137c7c3bba6591c84fdef4c5f6ef443f72da1cfd871478968c64a5c7cb5a1152` | igual al congelado: sí | sí | sin cierre |

El E congelado del kit se valida con la designación del kit apuntada a una copia del repositorio de la entrega y a copias de sus entradas (mismos commits, mismos bytes); `D:\r62-a6m7\m7-run` no se leyó.

## 4. Caso B1-like y residuo declarado (`p2-b1.json`, `b1-like/`, `check_citation_blobs.py`)

Clon temporal de `candidate-b.bundle` (custodiado en `MAP-INVALID/`, MAP_VALID: EFF `88ab34c3`, EFF^1 `55d19004`, con los blobs I-61 de la base del mapa). Solo en el clon: una unidad sintética FX-U2 sobre el reclamo `6040097c` (AR `2345c8d9`, BaseSha `a3515d92`, `Target.commit` `a2c8347d`; identidad `*.invalid`, fechas fijas), el objeto suelto del instrumento escrito desde su blob y las entradas custodiadas con las formas del kit de FX-02 y valores sintéticos. Una corrida real de `clausemap` por la forma declarada en pwsh 7 (no es el pwsh del runtime de Codex) y el aceptante fijado con la solicitud de la medición (sin `Verification` ni `RelayFacts`); sin registro de sesión: el caso solo prueba R9. La variante `uc` (AR `88970c80`) cambia el archivo UNIT_CHANGE citado antes de la AR: es el residuo que §3.5 declara.

`check_citation_blobs.py` es la comprobación de fallo cerrado de §3.5 (código 0: se puede lanzar; 1: no hay lanzamiento, S-03; 2: no evaluable). Modo predeterminado y único normativo: `coverage`, el criterio del texto vigente (si alguna lectura de contenido que `clausemap` hace de una cita del contrato en EFF^1, EFF o `MainSha_eval` cae en una revisión en que el blob de esa ruta no es el de una entrada del cierre, no hay lanzamiento; una revisión en que la ruta no existe no aporta lectura). Modos `text` (toda cita con el mismo blob en la AR, EFF, EFF^1 y `MainSha_eval`: la redacción anterior) y `unit` (lo mismo, solo para las citas UNIT_DOC y UNIT_CHANGE) solo como evidencia negativa.

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| B1-PIN-EFF1 | B1-like (clon temporal de candidate-b, MAP_VALID; EFF^1 lleva los blobs anteriores a la activación): entradas de A7-1 en EFF^1 | A7-1 fija en EFF^1 los archivos MODIFIED del mapa (y AUTOMATION_PLAN para MV-7) con blobs distintos de los de la AuthorityRevision; R7-A no | A7: 6 entradas en EFF^1, 6 con blob distinto del de la AR (AUTOMATION_PLAN.md 07b97bd3/AR f525cb1e, INITIATIVE_LIFECYCLE.md 6415c33b/AR f19896a8, WORKFLOW.md 884ed305/AR 446c32ba, README.md 64ae952b/AR 592dcfd4, routing.md 82ca9b38/AR bba08fc4, PROMPT_TEMPLATES.md 0e3ed262/AR 384d0b0c); R7-A contiene 0 de ellas | sí |
| B1-CHECK | validador sobre los dos E del caso B1-like | acepta el E de A7-1; rechaza el de R7-A (faltan las lecturas en EFF^1) | A7: acepta; R7-A: rechaza (faltan 6: AUTOMATION_PLAN.md@EFF^1, INITIATIVE_LIFECYCLE.md@EFF^1, PROMPT_TEMPLATES.md@EFF^1, README.md@EFF^1, WORKFLOW.md@EFF^1, routing.md@EFF^1) | sí |
| B1-DERIVE | derivación del cierre del aceptante (designate_m7 + derive_r4, sin cambio) desde cada E | designación válida en ambos; el cierre de A7 tiene RevPaths en EFF^1 para las lecturas de MV-6/MV-7 que el de R7-A no tiene | A7: {'Commits': 6, 'RevPaths': 69, 'Paths': 47, 'Files': 4}, RevPaths en EFF^1: 9; R7-A: {'Commits': 6, 'RevPaths': 63, 'Paths': 47, 'Files': 4}, RevPaths en EFF^1: 3 | sí |
| B1-CLAUSEMAP | clausemap por la forma declarada (& <git> --no-replace-objects cat-file blob 9a829b0e… \| & <python> -I - clausemap …), objeto suelto escrito en el clon temporal | rc 0, Error nulo, sobre canónico; el instrumento lee contenido en EFF^1 | rc 0; Error None; hechos MV1-MV7 presentes: sí; lecturas de contenido en EFF^1: AGENTS.md, AUTOMATION_PLAN.md, INITIATIVE_LIFECYCLE.md, WORKFLOW.md, README.md, routing.md, PROMPT_TEMPLATES.md, Calculator.cs | sí |
| B1-R9 | aceptante fijado (434ef1fc…) sobre la corrida real de clausemap con cada cierre derivado (solicitud de la medición: sin Verification ni RelayFacts) | R9 MATCH con el cierre de A7-1; R9 MISMATCH con el de R7-A (lecturas en EFF^1 fuera del cierre) | A7: ACCEPT_FACTS rc 0, R9 MATCH; R7-A: REJECT_FACTS rc 1, R9 MISMATCH (12 violaciones; p. ej. GitCalls[18]: object outside the closure: 07b97bd3eef3a36bae3180bd7eb24b176b8049f0; GitCalls[19]: object outside the closure: 6415c33bd2f86fa05444e00b630d7fed5e5d7a22); cero escrituras: sí | sí |
| B1-UC-R9 | residuo declarado (A-7 §3.5, Q-A7-06): el archivo UNIT_CHANGE citado (Calculator.cs) cambió antes de la AuthorityRevision (blob en la AR 50f77b97; en EFF^1, EFF y MainSha_eval 73e8502c); clausemap compara cada cita en EFF^1, EFF y MainSha_eval | el E de A7-1 es válido; con él, R9 MISMATCH sobre clausemap (rechazo falso, nunca aceptación falsa: el comportamiento declarado); la precondición de §3.5 bloquea el lanzamiento (S-03) | validador sobre el E de A7-1: acepta; clausemap rc 0; aceptante: REJECT_FACTS rc 1, R9 MISMATCH (3 violaciones: GitCalls[35]: object outside the closure: 73e8502c5fed4cff98d53e3ca880ac5598fac597; Inputs[18]: rev:path outside the closure; Inputs[19]: rev:path outside the closure); precondición (cobertura): NO_LAUNCH_S-03 | sí |
| CIT-B1-UC | precondición de §3.5 en la variante B1-UC | el criterio de cobertura bloquea (NO_LAUNCH, S-03): clausemap leería Calculator.cs en EFF^1, EFF y MainSha_eval con un blob que el cierre no fija | coverage: NO_LAUNCH_S-03 (Calculator.cs); text: NO_LAUNCH_S-03 (Calculator.cs, AUTOMATION_PLAN.md); unit: NO_LAUNCH_S-03 (Calculator.cs) | sí |
| CIT-B1-MAIN | precondición de §3.5 en el caso B1-like principal (el contrato cita AUTOMATION_PLAN.md como EXTERNAL, como el de FX-U1; AUTOMATION_PLAN.md es MODIFIED en todo mapa válido) | el criterio de cobertura deja lanzar (AUTOMATION_PLAN.md en EFF^1 lo fija el cierre, categoría (a); R9 da MATCH en B1-R9); la redacción anterior («blobs distintos en esas revisiones», modo text) bloqueaba: evidencia negativa de F10 | coverage: LAUNCH_MAY_PROCEED; text: NO_LAUNCH_S-03 (AUTOMATION_PLAN.md: different blobs at AR f525cb1e, EFF f525cb1e, EFF^1 07b97bd3, MainSha_eval f525cb1e); unit: LAUNCH_MAY_PROCEED | sí |
| DES-REG-B1 | designación B1-like con la ruta y el blob de cada artefacto registrados (regla 2 (c) corregida) | ningún aviso de blob sin registrar; el validador comprueba los cuatro | sin blob registrado: ninguno | sí |

## 5. Negativos (§92, punto 7) (`p3-neg.json`)

Ausentes, sobre el constructor (y el validador cuando hay cierre); reemplazados y ajenos, sobre el validador con copias mutadas del E de V (o del B1-like cuando el caso lo exige). Commits sintéticos solo en la copia del repositorio de la entrega: `f8ef7429` (borra `src/Fixture.Lib/Fixture.Lib.csproj`), `00489394` (borra `docs/automation/state/FX-U1.yml`) y `303174d6` (el registro de decisiones vuelve al blob de la AR).

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| NEG-ABS-AUTH | ausente: una autoridad citada por el contrato no existe en su revisión (UNIT_DOC en la AR) | regla 5 (a) corregida: cierre sin esa entrada, ausencia registrada, nada la sustituye (la trata 16.3); el validador lo acepta | cierre con 26 entradas; ausencias registradas: AUTHORITY:UNIT_DOC docs/automation/decisions/FX-U1-ausente.md: absent at AR: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entrada para la autoridad ausente: no; ausencia registrada: sí | sí |
| NEG-ABS-AUTH-OLD | evidencia negativa: la misma designación con la regla 5 (a) de la ronda 1 (--absent-a=no-closure) | sin cierre (lo que el texto corregido ya no hace) | sin cierre: AUTHORITY_ABSENT (docs/automation/decisions/FX-U1-ausente.md absent at AR, read by AUTHORITY:UNIT_DOC (superseded reading of rule 5 (a): no closure)) | sí |
| NEG-ABS-TARGET | ausente: el objeto exacto del Target no existe en Target.commit | sin cierre (regla 5 (b)) | sin cierre: TARGET (the exact object of the Target is absent at Target.commit (rule 5 (b): no launch)) | sí |
| NEG-ABS-TARGET-CHK | validador: el E de V con ese Target | rechaza: NoClosureExpected (regla 5 (b)) | rechaza: rule 5 (b): the exact object of the Target is absent at Target.commit | sí |
| NEG-ABS-DIFFPATH | ausente: ruta del diff BaseSha..Target.commit borrada en Target.commit (commit sintético f8ef7429 en la copia del repositorio) | cierre sin entrada para esa ruta (regla 5 (b)), causa registrada; el validador lo acepta (positivo) | cierre con 26 entradas; ausencias registradas: DIFF_PATH src/Fixture.Lib/Fixture.Lib.csproj: deleted at Target.commit: no entry (rule 5 (b)); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entrada para src/Fixture.Lib/Fixture.Lib.csproj: no | sí |
| NEG-ABS-RECORD | ausente: el registro de estado de la unidad no existe en Target.commit (commit sintético 00489394) | cierre sin entrada (b) para el registro (regla 5 (b)), causa registrada; el validador lo acepta | cierre con 25 entradas; ausencias registradas: DIFF_PATH docs/automation/state/FX-U1.yml: deleted at Target.commit: no entry (rule 5 (b)); RECORD docs/automation/state/FX-U1.yml: absent at Target.commit: no entry (rule 5 (b)); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entrada (b) del registro: no; la autoridad UNIT_DOC sigue en la AR | sí |
| NEG-ABS-DELEG-AUSENTE | ausente: DELEG sin bytes custodiados | regla 5 (c) corregida: sin cierre y sin lanzamiento, causa registrada, nada sustituido | sin cierre: NO_CLOSURE (delegation absent, with another blob or unreadable: (b) cannot be computed (rule 5 (c): no launch); nothing substitutes it); ausencias registradas: DELEG artifacts/orchestration/FX-U1/T1/0/R20261012T090000Z-d001/output.json: ABSENT: the custodied file does not exist; sin K o sin D, (a) y (b) no se pueden calcular: sin cierre ni lanzamiento (regla 5 (c)) | sí |
| NEG-ABS-DELEG-OTRO-BLOB | ausente: DELEG con bytes de otro blob que el registrado | regla 5 (c) corregida: sin cierre y sin lanzamiento, causa registrada, nada sustituido | sin cierre: NO_CLOSURE (delegation absent, with another blob or unreadable: (b) cannot be computed (rule 5 (c): no launch); nothing substitutes it); ausencias registradas: DELEG artifacts/orchestration/FX-U1/T1/0/R20261012T090000Z-d001/output.json: BLOB_MISMATCH: the custodied bytes do not have the registered blob; sin K o sin D, (a) y (b) no se pueden calcular: sin cierre ni lanzamiento (regla 5 (c)) | sí |
| NEG-ABS-DELEG-ILEGIBLE | ausente: DELEG no legible (la ruta es un directorio) | regla 5 (c) corregida: sin cierre y sin lanzamiento, causa registrada, nada sustituido | sin cierre: NO_CLOSURE (delegation absent, with another blob or unreadable: (b) cannot be computed (rule 5 (c): no launch); nothing substitutes it); ausencias registradas: DELEG artifacts/orchestration/FX-U1/T1/0/R20261012T090000Z-d001/output.json: UNREADABLE: the custodied bytes cannot be read; sin K o sin D, (a) y (b) no se pueden calcular: sin cierre ni lanzamiento (regla 5 (c)) | sí |
| NEG-ABS-H-AUSENTE | ausente: H sin bytes custodiados | regla 5 (c): sin entrada, causa registrada, nada sustituido; el validador acepta el E; como N4 en la derivación | cierre con 25 entradas; ausencias registradas: H artifacts/orchestration/FX-U1/T1/0/R20261012T091000Z-a001/worker-handoff.json: ABSENT: the custodied file does not exist; 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entrada en la ruta designada: no; otras entradas fuera de Git: 2; designate_m7: Ok; derivación como N4: omitidos por INPUT_ABSENT: C1.H.BaseSha, C1.H.CurrentSha, C1.H.RedSha, C1.RevList(D.BaseSha..H.CurrentSha), C6.H | sí |
| NEG-ABS-H-OTRO-BLOB | ausente: H con bytes de otro blob que el registrado | regla 5 (c): sin entrada, causa registrada, nada sustituido; el validador acepta el E; como N4 en la derivación | cierre con 25 entradas; ausencias registradas: H artifacts/orchestration/FX-U1/T1/0/R20261012T091000Z-a001/worker-handoff.json: BLOB_MISMATCH: the custodied bytes do not have the registered blob; 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entrada en la ruta designada: no; otras entradas fuera de Git: 2; designate_m7: Ok; derivación como N4: omitidos por INPUT_ABSENT: C1.H.BaseSha, C1.H.CurrentSha, C1.H.RedSha, C1.RevList(D.BaseSha..H.CurrentSha), C6.H | sí |
| NEG-ABS-H-ILEGIBLE | ausente: H no legible (la ruta es un directorio) | regla 5 (c): sin entrada, causa registrada, nada sustituido; el validador acepta el E; como N4 en la derivación | cierre con 25 entradas; ausencias registradas: H artifacts/orchestration/FX-U1/T1/0/R20261012T091000Z-a001/worker-handoff.json: UNREADABLE: the custodied bytes cannot be read; 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entrada en la ruta designada: no; otras entradas fuera de Git: 2; designate_m7: Ok; derivación como N4: omitidos por INPUT_ABSENT: C1.H.BaseSha, C1.H.CurrentSha, C1.H.RedSha, C1.RevList(D.BaseSha..H.CurrentSha), C6.H | sí |
| NEG-ABS-CONTRACT-AUSENTE | ausente: CONTRACT sin bytes custodiados | regla 5 (c) corregida: sin cierre y sin lanzamiento, causa registrada, nada sustituido | sin cierre: NO_CLOSURE (contract absent, with another blob or unreadable: (a) cannot be computed (rule 5 (c): no launch); nothing substitutes it); ausencias registradas: CONTRACT docs/automation/decisions/FX-U1-T1.gate-contract.json: ABSENT: the Git object of the registered blob is not available in the -C; sin K o sin D, (a) y (b) no se pueden calcular: sin cierre ni lanzamiento (regla 5 (c)) | sí |
| NEG-ABS-CONTRACT-OTRO-BLOB | ausente: CONTRACT con bytes de otro blob que el registrado | regla 5 (c) corregida: sin cierre y sin lanzamiento, causa registrada, nada sustituido | sin cierre: NO_CLOSURE (contract absent, with another blob or unreadable: (a) cannot be computed (rule 5 (c): no launch); nothing substitutes it); ausencias registradas: CONTRACT docs/automation/decisions/FX-U1-T1.gate-contract.json: BLOB_MISMATCH: the custodied bytes do not have the registered blob; sin K o sin D, (a) y (b) no se pueden calcular: sin cierre ni lanzamiento (regla 5 (c)) | sí |
| NEG-ABS-CONTRACT-ILEGIBLE | ausente: CONTRACT no legible (la ruta es un directorio) | regla 5 (c) corregida: sin cierre y sin lanzamiento, causa registrada, nada sustituido | sin cierre: NO_CLOSURE (contract absent, with another blob or unreadable: (a) cannot be computed (rule 5 (c): no launch); nothing substitutes it); ausencias registradas: CONTRACT docs/automation/decisions/FX-U1-T1.gate-contract.json: UNREADABLE: the custodied bytes cannot be read; sin K o sin D, (a) y (b) no se pueden calcular: sin cierre ni lanzamiento (regla 5 (c)) | sí |
| NEG-ABS-TRX-AUSENTE | ausente: TRX sin bytes custodiados | regla 5 (c): sin entrada, causa registrada, nada sustituido; el validador acepta el E | cierre con 25 entradas; ausencias registradas: TRX artifacts/ci/R37949808974/fixture-tests.trx: ABSENT: the custodied file does not exist; 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entrada en la ruta designada: no; otras entradas fuera de Git: 2; designate_m7: TRX_NOT_PINNED | sí |
| NEG-ABS-TRX-OTRO-BLOB | ausente: TRX con bytes de otro blob que el registrado | regla 5 (c): sin entrada, causa registrada, nada sustituido; el validador acepta el E | cierre con 25 entradas; ausencias registradas: TRX artifacts/ci/R37949808974/fixture-tests.trx: BLOB_MISMATCH: the custodied bytes do not have the registered blob; 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entrada en la ruta designada: no; otras entradas fuera de Git: 2; designate_m7: TRX_NOT_PINNED | sí |
| NEG-ABS-TRX-ILEGIBLE | ausente: TRX no legible (la ruta es un directorio) | regla 5 (c): sin entrada, causa registrada, nada sustituido; el validador acepta el E | cierre con 25 entradas; ausencias registradas: TRX artifacts/ci/R37949808974/fixture-tests.trx: UNREADABLE: the custodied bytes cannot be read; 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entrada en la ruta designada: no; otras entradas fuera de Git: 2; designate_m7: TRX_NOT_PINNED | sí |
| NEG-REP-BASE-V | reemplazada: el objeto (Calculator.cs) fijado en BaseSha en vez de Target.commit (V) | rechaza: falta la entrada (b) en Target.commit; el blob de BaseSha coincide aquí con el de la AR (duplicado de la autoridad UNIT_CHANGE) | rechaza: MISSING b:TARGET src/Fixture.Lib/Calculator.cs 380892ee; MISSING b:DIFF src/Fixture.Lib/Calculator.cs 380892ee; DUPLICATE CanonicalInputs src/Fixture.Lib/Calculator.cs 73e8502c | sí |
| NEG-REP-BASE-B1 | reemplazada: el objeto fijado en BaseSha en vez de Target.commit (B1-like, donde BaseSha cambió el objeto) | rechaza: entrada ajena (blob de BaseSha) y falta la del Target | rechaza: FOREIGN src/Fixture.Lib/Calculator.cs 726c25c3 (the blob is not the reference of its category (73e8502c, a04c1096)); MISSING b:TARGET src/Fixture.Lib/Calculator.cs a04c1096; MISSING b:DIFF src/Fixture.Lib/Calculator.cs a04c1096 | sí |
| NEG-REP-AUTH-TGT | reemplazada: la autoridad UNIT_DOC (decisiones) fijada en Target.commit en vez de en su revisión de lectura (AR) | rechaza: falta la autoridad en la AR (la entrada queda duplicada con el registro (b)) | rechaza: MISSING a:AUTHORITY:UNIT_DOC@AR docs/automation/decisions/FX-U1.md d3f3ec72; DUPLICATE CanonicalInputs docs/automation/decisions/FX-U1.md 3fcfffc1 | sí |
| NEG-REP-CONTRACT | reemplazada: el contrato por otro contrato (otro blob en la ruta designada) | rechaza: entrada ajena y falta el contrato custodiado | rechaza: FOREIGN docs/automation/decisions/FX-U1-T1.gate-contract.json e8c3882c (the blob is not the reference of its category (628d89af)); MISSING c:CONTRACT docs/automation/decisions/FX-U1-T1.gate-contract.json 628d89af | sí |
| NEG-REP-H-OTHER | reemplazada: la entrega por la copia de otra corrida (R20261011T081000Z-a000) en una ruta no designada | rechaza: entrada fuera de Git no designada y falta la entrega designada | rechaza: FOREIGN artifacts/orchestration/FX-U1/T1/0/R20261011T081000Z-a000/worker-handoff.json dbf4a289 (off-Git or TRX path not designated, or with another blob than the custodied bytes); MISSING c:H artifacts/orchestration/FX-U1/T1/0/R20261012T091000Z-a001/worker-handoff.json 954c1dbd | sí |
| NEG-REP-EFF1 | reemplazada: la entrada de EFF^1 (WORKFLOW, MV-6/MV-7) por su blob de EFF (B1-like) | rechaza: falta la lectura en EFF^1 (y la entrada queda duplicada) | rechaza: MISSING a:16.13-MV6@EFF^1 docs/WORKFLOW.md 884ed305; DUPLICATE AllowedTransitiveInputs docs/WORKFLOW.md 446c32ba | sí |
| NEG-FOR-TGT-OUT | ajena: un archivo de Target.commit fuera del diff y de los registros (src/Fixture.Lib/Fixture.Lib.csproj) | rechaza: FOREIGN | rechaza: FOREIGN src/Fixture.Lib/Fixture.Lib.csproj bfa77a11 (the path has no reference in any category (outside the diff, the records, the designation and the authorities)) | sí |
| NEG-FOR-OTHER-REPO | ajena: una ruta de otro repositorio (el worktree de I-62: docs/initiatives/I-62-A-6.md, blob 4bc919dd) | rechaza: FOREIGN | rechaza: FOREIGN docs/initiatives/I-62-A-6.md 4bc919dd (the path has no reference in any category (outside the diff, the records, the designation and the authorities)) | sí |
| NEG-FOR-TRX | ajena: un TRX no designado | rechaza: FOREIGN (fuera de Git o TRX no designado) | rechaza: FOREIGN artifacts/ci/R00000000000/otro.trx 8b2f09c6 (off-Git or TRX path not designated, or with another blob than the custodied bytes) | sí |
| NEG-FOR-INSTR | ajena: la entrada del instrumento (y su acción) en una invocación que no lo admite (designación sin instrumento) | rechaza: FOREIGN (instrumento no admitido) y la acción del instrumento | rechaza: FOREIGN docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py 9a829b0e (instrument entry in an invocation that does not admit it); ACTION: instrument action in an invocation that does not admit the instrument | sí |
| NEG-FOR-INSTR-BUILD | constructor con la misma designación sin instrumento (control) | cierre sin la entrada ni la acción del instrumento | entrada del instrumento: no; acción: no | sí |
| NEG-FOR-DUP | ajena: una entrada duplicada con el mismo blob (el registro de decisiones en Target.commit, dos veces) | rechaza: DUPLICATE | rechaza: DUPLICATE CanonicalInputs docs/automation/decisions/FX-U1.md 3fcfffc1 | sí |
| NEG-FOR-WIDE | ajena con el texto corregido: un archivo ADDED del mapa en EFF (docs/automation/agent-execution/adapters/codex-cli.md), que Validate solo compara por su blob | rechaza: FOREIGN (la regla 1 (a) corregida solo admite mapa, esquema, MODIFIED y WORKFLOW/AUTOMATION_PLAN) | rechaza: FOREIGN docs/automation/agent-execution/adapters/codex-cli.md 155f3469 (the path has no reference in any category (outside the diff, the records, the designation and the authorities)) | sí |
| NEG-POS-OVERLAP | positivo de la regla 1 corregida: el registro de decisiones en Target.commit tiene el blob de la AR (commit sintético 303174d6), así que el par es a la vez (a) UNIT_DOC y (b) registro y objeto del diff | una sola entrada, en dos categorías y con el blob de la referencia de cada una; el validador la acepta | cierre con 25 entradas; ausencias registradas: 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); 16.13:MV-6 docs/initiatives/PROMPT_TEMPLATES.md: absent at EFF^1: no entry; treated by the rule that requires it (16.3, 16.13, 16.24); validador: acepta; entradas de ese par: 1; categorías: ['a', 'b'] | sí |
| NEG-PATH-RUNID | regla 2 (c) corregida: la copia de la entrega en un directorio con el RunId de la verificación real y no el de sus bytes (designación N7AR-SAME del banco, el riesgo O6) | rechaza: PATH_RULE (el directorio no lleva el RunId de los bytes) | rechaza: PATH_RULE: H path is not under artifacts/orchestration/ in a directory named with the RunId of its bytes (path rule text) | sí |

**Reutilizado por identidad, sin volver a ejecutar** (negativos de sustitución e inflación al nivel de la derivación del cierre del aceptante; `derive_r4.py` y `designate_m7.py` sin cambio):

| Archivo | SHA-256 | Casos | ¿Identidad comprobada? |
|---|---|---|---|
| `docs/automation/evidence/I-62-A6/r4/r1probes/p7-derive.json` | `099000fb80a33bee394bdfc9690398aa7f558e79a11d6fbea3ef71c829de7444` | grupo neg: S-N4-REALH, S-N4-TARGET, S-NC1-G, S-NC1-OTHER, S-N4-E2E, X-N4-COMMIT, X-N4-NX, X-N4-PATH, X-N4-REVPATH, X-N4-FILE, X-N4-REF | sí |
| `docs/automation/evidence/I-62-A6/r4/r1probes/p9-n4.json` | `b3dcf564ef67f9da0dafa21caf9d5d95a2114a8e72b422f972b5d67873efea51` | N4-x, N4-x-subst | sí |
| `docs/automation/evidence/I-62-A6/r4/r1probes/p10-nc1.json` | `068cd82fcb77abff0f5374418ca3bf20b886ee19851fb0b2e1a1b84844a92870` | nc1-x | sí |
| `docs/automation/evidence/I-62-A6/m7/bench/p1-build.json (grupo desig)` | `6a8c57376685465ce39a5b117ad5f4077bc4319b43814a0ba78c36e6c0213b6a` | DES-MULTI, DES-O3-OTHER-RUN, DES-O3-PATTERN, DES-TRX-BAD, DES-MAP-NX | sí |

## 6. Guardas de alcance (`a7-guards.py`; `p4-guards-preview.json`, `p4-guards-selftest.json`)

`preview` construye un commit hipotético sobre `bb439090` en un repositorio desnudo temporal propio (alternates de solo lectura; ni refs, ni índice, ni árbol del repositorio de I-62 cambian) con la candidata como `docs/initiatives/I-62-A-7.md` (blob `6c29747d61c5f689a195095ad3712e6d9dba62aa`) y este directorio bajo `docs/automation/evidence/I-62-A7/` (sin `summary.json`, `preflight.md`, `MANIFEST.sha256`, `p4-guards.json` ni su propia salida, generados después; G7 se repite al final sobre el directorio completo). G3 conserva las citas de §2 (sin cambio en la corrección); G4 y G5 comprueban además el texto corregido y rechazan las frases superadas de la ronda 1. Para el commit de publicación: `python -I a7-guards.py run --base <base> --head <commit> --out <json>`. Commit hipotético: `11a94996cbea39d1f41eeffca81ee3abc5878688`.

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| GUARD-PREVIEW | guardas G1-G7 sobre el commit hipotético (base bb439090 + candidata + este directorio) | PASS en las siete | G1 PASS; G2 PASS; G3 PASS; G4 PASS; G5 PASS; G6 PASS; G7 PASS | sí |
| GUARD-SELFTEST | autoprueba: un mutante por condición de guarda | candidata PASS y todos los mutantes muertos | candidata PASS; mutantes muertos 44 de 44 | sí |
| MUT-G1-only-adds | modifica un archivo existente (docs/automation/decisions/I-62.md) | G1 FAIL (G1-only-adds) | G1 FAIL | sí |
| MUT-G1-allowed-paths | añade un archivo fuera de lo permitido (docs/automation/evidence/I-62-A6/extra.txt) | G1 FAIL (G1-allowed-paths) | G1 FAIL | sí |
| MUT-G1-amendment-added | el commit no añade la enmienda | G1 FAIL (G1-amendment-added) | G1 FAIL | sí |
| MUT-G2-protected | cambia el archivo de estado de I-62 (superficie protegida) | G2 FAIL (G2-protected(surface)) | G2 FAIL | sí |
| MUT-G2-kit | cambia un archivo del kit a6m7-measure | G2 FAIL (G2-protected(kit)) | G2 FAIL | sí |
| MUT-G2-I-64/I-65 | añade una ruta de I-65 | G2 FAIL (G2-protected(I-64/I-65)) | G2 FAIL | sí |
| MUT-G2-pinned | cambia el blob de A-5 en el commit | G2 FAIL (G2-pinned) | G2 FAIL | sí |
| MUT-G2-header-claims | la cabecera de A-7 cita otro blob de A-5 | G2 FAIL (G2-header-claims) | G2 FAIL | sí |
| MUT-G3-literal | L1 con una palabra cambiada en la cita | G3 FAIL (G3-literal) | G3 FAIL | sí |
| MUT-G3-lines | L4 con otra línea (2234) | G3 FAIL (G3-literal) | G3 FAIL | sí |
| MUT-G3-blob | L6 con otro blob abreviado de AP | G3 FAIL (G3-blob) | G3 FAIL | sí |
| MUT-G3-anchor | L7 (fila parafraseada) con otro rango de líneas | G3 FAIL (G3-literal) | G3 FAIL | sí |
| MUT-G3-rows | falta la fila L15 | G3 FAIL (G3-rows) | G3 FAIL | sí |
| MUT-G3-extra | el texto cita el fin de A6-8 en otras líneas | G3 FAIL (G3-extra) | G3 FAIL | sí |
| MUT-G4-scope | frase de alcance sin «en F6» | G4 FAIL (G4-scope) | G4 FAIL | sí |
| MUT-G4-nochange | sin la cláusula «sin cambiar el texto de §20.3.1, de B.9, de AUTOMATION_PLAN 16.24» | G4 FAIL (G4-nochange) | G4 FAIL | sí |
| MUT-G4-points | el punto 7 deja de estar numerado | G4 FAIL (G4-points) | G4 FAIL | sí |
| MUT-G4-categories | la categoría (d) sale del punto 1 | G4 FAIL (G4-categories) | G4 FAIL | sí |
| MUT-G4-location | otra ubicación | G4 FAIL (G4-location) | G4 FAIL | sí |
| MUT-G4-table | la tabla de §3.3 pierde la fila (c) | G4 FAIL (G4-table) | G4 FAIL | sí |
| MUT-G4-rule1 | regla 1 sin «al menos» | G4 FAIL (G4-rule1) | G4 FAIL | sí |
| MUT-G4-rule1a | regla 1 (a) sin «cuyo contenido lee» | G4 FAIL (G4-rule1a) | G4 FAIL | sí |
| MUT-G4-rule2a | regla 2 (a) excluye AUTOMATION_PLAN en EFF^1 | G4 FAIL (G4-rule2a) | G4 FAIL | sí |
| MUT-G4-rule2-repeat | regla 2 (a): «en que coincida» en lugar de «en que difiera» | G4 FAIL (G4-rule2-repeat) | G4 FAIL | sí |
| MUT-G4-rule2c | regla 2 (c) sin la ruta del control | G4 FAIL (G4-rule2c) | G4 FAIL | sí |
| MUT-G4-rule5a | regla 5 (a) vuelve a «deja el cierre sin calcular» | G4 FAIL (G4-rule5a) | G4 FAIL | sí |
| MUT-G4-rule5c | regla 5 (c) sin la delegación | G4 FAIL (G4-rule5c) | G4 FAIL | sí |
| MUT-G4-superseded | reinsertada la frase de unicidad de la ronda 1 | G4 FAIL (G4-superseded) | G4 FAIL | sí |
| MUT-G4-residual | sin el rótulo del residuo declarado de §3.5 | G4 FAIL (G4-residual) | G4 FAIL | sí |
| MUT-G4-rule2c-register | regla 2 (c) sin el registro de ruta y blob de cada artefacto | G4 FAIL (G4-rule2c-register) | G4 FAIL | sí |
| MUT-G4-residual-coverage | §3.5: «es el de una entrada del cierre» cambiado | G4 FAIL (G4-residual-coverage) | G4 FAIL | sí |
| MUT-G4-superseded-35 | §3.5: reinsertado el criterio de blobs distintos | G4 FAIL (G4-superseded-35) | G4 FAIL | sí |
| MUT-G4-q06 | sin la pregunta Q-A7-06 | G4 FAIL (G4-q06) | G4 FAIL | sí |
| MUT-G4-table-absence | la columna de ausencia de (c) en §3.3 vuelve a «sin entrada» | G4 FAIL (G4-table-absence) | G4 FAIL | sí |
| MUT-G5-state-table | §11: Architect «ronda 1: LAUNCHED» | G5 FAIL (G5-state-table) | G5 FAIL | sí |
| MUT-G5-state | State AGREED | G5 FAIL (G5-state) | G5 FAIL | sí |
| MUT-G5-architect | Architect review LAUNCHED | G5 FAIL (G5-architect) | G5 FAIL | sí |
| MUT-G5-classification | clasificación no material | G5 FAIL (G5-classification) | G5 FAIL | sí |
| MUT-G5-owner | decisión del Owner requerida | G5 FAIL (G5-owner) | G5 FAIL | sí |
| MUT-G6-values | M-03 = no | G6 FAIL (G6-values) | G6 FAIL | sí |
| MUT-G6-header | la cabecera declara M-05 no | G6 FAIL (G6-header) | G6 FAIL | sí |
| MUT-G7-host-identity | la identidad del host (leída en tiempo de ejecución) en el texto | G7 FAIL (host identity present) | G7 FAIL | sí |
| MUT-G7-users-path | una ruta de perfil de Windows con nombre | G7 FAIL (Users\<name> path) | G7 FAIL | sí |
| MUT-G7-e-mail | una dirección de correo | G7 FAIL (e-mail address) | G7 FAIL | sí |

## 7. Compatibilidad y preservación (`p5-compat.json`)

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| PIN-AUTOMATION_PLAN.md | blob de docs/AUTOMATION_PLAN.md en la base | f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5 | f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5 | sí |
| PIN-README.md | blob de docs/automation/agent-execution/README.md en la base | 592dcfd45e8743fd83b3423249fbd3c73e3d3055 | 592dcfd45e8743fd83b3423249fbd3c73e3d3055 | sí |
| PIN-I-62-A-1.md | blob de docs/initiatives/I-62-A-1.md en la base | c01899a72b940503bb85a0fab42bc085c603fd0f | c01899a72b940503bb85a0fab42bc085c603fd0f | sí |
| PIN-I-62-A-2.md | blob de docs/initiatives/I-62-A-2.md en la base | f1e1d6f08d3cf677500794c7d0019433a4dc7d4e | f1e1d6f08d3cf677500794c7d0019433a4dc7d4e | sí |
| PIN-I-62-A-3.md | blob de docs/initiatives/I-62-A-3.md en la base | ea6721f78cb63fbc4f9d99563f3262eb36a32c5d | ea6721f78cb63fbc4f9d99563f3262eb36a32c5d | sí |
| PIN-I-62-A-4.md | blob de docs/initiatives/I-62-A-4.md en la base | 5e2ba68efdc1b902aa1c28aff4453b4b6973be7f | 5e2ba68efdc1b902aa1c28aff4453b4b6973be7f | sí |
| PIN-I-62-A-5.md | blob de docs/initiatives/I-62-A-5.md en la base | ad94ceedacbf3dbf70f0217a583978d0e32592fc | ad94ceedacbf3dbf70f0217a583978d0e32592fc | sí |
| PIN-I-62-A-6.md | blob de docs/initiatives/I-62-A-6.md en la base | 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 | 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 | sí |
| PIN-I-62-consensus-freeze.md | blob de docs/initiatives/I-62-consensus-freeze.md en la base | 0f6b860837e2478db8525e1fb30a485bc4646816 | 0f6b860837e2478db8525e1fb30a485bc4646816 | sí |
| PIN-I-62-proposal-v14.md | blob de docs/initiatives/I-62-proposal-v14.md en la base | 34ad80ea1bfff144bfc5169f62920a4c904c1bfa | 34ad80ea1bfff144bfc5169f62920a4c904c1bfa | sí |
| PRES-D3-BUDGET | filas de presupuesto de V14 D.3 (Controller, Codex, totales por ronda: 17 filas, L2431-L2529) | sin cambio (V14 34ad80ea) y no reescritas por A-7 (ningún tramo de más de 60 caracteres de una fila en A-7) | V14 en la base = 34ad80ea: sí; tramo común más largo con A-7: 16 caracteres | sí |
| PRES-A6-6 | oráculo de C-24 (A6-6, A-6 L735-L750) | sin cambio (A-6 4bc919dd) y no reescrito por A-7 | SHA-256 del párrafo 6d8f1a4d836fd68efd8b79a4b879c9430cd3d8d2300350a0a201b6f4a7b56807; tramo común más largo con A-7: 24 caracteres | sí |
| PRES-OWNER-A-6-S7-L1324 | línea del Owner: A-6 §7 L1324 | sin cambio; A-7 no la reescribe ni la altera (solo la nombra) | SHA-256 83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14 (897 bytes); tramo común más largo con A-7: 42 caracteres | sí |
| PRES-OWNER-A-6-S7-L1325 | línea del Owner: A-6 §7 L1325 | sin cambio; A-7 no la reescribe ni la altera (solo la nombra) | SHA-256 b52a46e34aea2926c53234e9ca4241d7e3f16bfe6655fbf3623c74d99128fa44 (876 bytes); tramo común más largo con A-7: 40 caracteres | sí |
| PRES-OWNER-decisiones-S78 | línea del Owner: decisiones §78 | sin cambio (SHA-256 83152eae4f451961…); A-7 no la reescribe ni la altera (solo la nombra) | SHA-256 83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14 (897 bytes); tramo común más largo con A-7: 42 caracteres | sí |
| PRES-OWNER-decisiones-L890 | línea del Owner: decisiones L890 | sin cambio; A-7 no la reescribe ni la altera (solo la nombra) | SHA-256 76fc967585300073a08d4a099ae406c007f0f176899fabe315421ba308aef54a (269 bytes); tramo común más largo con A-7: 20 caracteres | sí |
| PRES-OWNER-decisiones-L931 | línea del Owner: decisiones L931 | sin cambio; A-7 no la reescribe ni la altera (solo la nombra) | SHA-256 7dcb195327e300bb802ea93bab1640c6dd3d0d807677ec110f513bb779057414 (353 bytes); tramo común más largo con A-7: 15 caracteres | sí |
| PRES-OWNER-kit-line-b.txt | línea del Owner: kit line-b.txt | sin cambio; A-7 no la reescribe ni la altera (solo la nombra) | SHA-256 dda4b3f9c1db0183a38e5400777a2a21d524aa4b92ad684d5c5cae1b2fc4cf27 (876 bytes); tramo común más largo con A-7: 40 caracteres | sí |
| PRES-OWNER-kit-line-od2.txt | línea del Owner: kit line-od2.txt | sin cambio; A-7 no la reescribe ni la altera (solo la nombra) | SHA-256 3b33dd56cbae5fddab9a9431e7ad9b83170e6782e0161385bc324789634b7f25 (161 bytes); tramo común más largo con A-7: 7 caracteres | sí |
| PRES-OWNER-78-A6 | la línea (a) de A-6 §7 es byte a byte la registrada en decisiones §78 | idénticas | idénticas | sí |
| COMPAT-A5 | A-5 (A5-1 construcción de B1, A5-2 presupuesto de FX-U2) | A-5 sin cambio (ad94ceed); A-7 §6 declara A5-1 y A5-2 sin cambio y no crea B1 | blob ad94ceedacbf3dbf70f0217a583978d0e32592fc; «A5-1 y A5-2 sin cambio» en A-7: sí; «no crea B1» en A-7: sí | sí |
| COMPAT-A6-RULES | reglas de A-6 que toca A7-1 | solo la referencia de comparación de A6-2, regla 3, dentro del ámbito de A-7; A7-1, regla 7, excluye cualquier otra | «ni ninguna otra regla de A-6» en la regla 7: sí; A-6 sin cambio de blob: sí | sí |

| Regla de A-6 | Cómo la toca A7-1 |
|---|---|
| A6-1 | no tocada |
| A6-2, reglas 1 y 2 | sin cambio: la categoría (d) remite a ellas |
| A6-2, regla 3 | la única tocada: dentro del ámbito de A-7, la referencia de comparación (I6) de las entradas de (a) leídas fuera de la AR, de (b) y de (c) deja de ser la AR; para las autoridades de la unidad sigue siendo la AR; el texto de A6-2 no cambia (A-7 §3.4) |
| A6-3, reglas 1-5 y 7 | sin cambio; la regla 4 (lectura de otro repositorio = P-22) se conserva (A-7 §3.5) |
| A6-3, regla 6 (b) | sin cambio: la derivación usa el cierre de A7-1 tal como es; omisiones (i)-(v) iguales |
| A6-4 | sin cambio: la fidelidad del instrumento, por remisión (A7-1, regla 4 (d)) |
| A6-5, A6-6, A6-8 | no tocadas (oráculos, resultado del aceptante, determinación del Owner) |
| A6-7, reglas 5 y 7 | sin cambio: el cierre de A7-1 es el «cierre de VERIFY» que nombran |
| A6-9, regla 4 y tabla | sin cambio: el cierre del aceptante se deriva del de A7-1 |

## 8. Reconstrucción, comprobación estática (§92, punto 6) (`p6-recon.json`, `recon_static.py`)

Comparación por `ast.dump` (sin atributos de posición) de `reconstruct_runs.py` (`5dc9f78e…`) y `reconstruct_runs_m7.py` (`1eceecee…`), con la normalización declarada en cada fila. **No se declara demostrada la reconstrucción sobre Codex 26.1013**: esto es solo estático; la reconstrucción desde un registro real sigue siendo una precondición con fallo cerrado de la medición.

| Caso | Qué | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| RECON-canon | identidad AST de canon (helper) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-sha256 | identidad AST de sha256 (helper) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-split_outer | identidad AST de split_outer (outer splitter) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-_strategy | identidad AST de _strategy (outer splitter) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-quote_token | identidad AST de quote_token (outer splitter (rendering)) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-render_command | identidad AST de render_command (outer splitter (rendering)) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-lex_pwsh | identidad AST de lex_pwsh (pwsh lexer) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-_abs_exe | identidad AST de _abs_exe (git / interpreter path check) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-extract_envelope | identidad AST de extract_envelope (envelope extraction) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-parse_log | identidad AST de parse_log (completeness) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-NotReconstructible | identidad AST de NotReconstructible (exception class) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-FACTS_SCHEMA | identidad AST de FACTS_SCHEMA (module constant) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-ALLOWED_EVENTS | identidad AST de ALLOWED_EVENTS (module constant) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-TOOL_COMMANDS | identidad AST de TOOL_COMMANDS (module constant) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-OUTER_BARE | identidad AST de OUTER_BARE (module constant) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-INNER_BARE | identidad AST de INNER_BARE (module constant) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-WIN_ABS | identidad AST de WIN_ABS (module constant) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-GIT_NAME | identidad AST de GIT_NAME (module constant) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-PY_NAME | identidad AST de PY_NAME (module constant) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-PWSH_NAME | identidad AST de PWSH_NAME (module constant) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-parse_instrument_command | identidad AST de parse_instrument_command (ref) [lex_pwsh .. args] = _parse_pwsh_inner (v2) [lex_pwsh .. args] (pwsh inner grammar) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-reconstruct | identidad AST de reconstruct (execution identity, exit rules, envelope custody) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-main | identidad AST de main (command line) | idéntica tras la normalización declarada | idéntica | sí |
| RECON-HEADER | cambios de la v2 frente a lo que declara su cabecera | solo lo declarado: RULE, CMD_NAME, CMD_FORBIDDEN, _abs_shell, _parse_cmd_inner, _parse_pwsh_inner (extracción), despachador, campos Rule y Rendering | nuevas o cambiadas: CMD_FORBIDDEN, CMD_NAME, RULE, _abs_shell, _parse_cmd_inner, _parse_pwsh_inner; no declaradas: ninguna; además se retiró del original una sentencia sin efecto: if env.get('Command') != p['Argv'][0]: pass | sí |

| Garantía | Dónde | ¿Idéntica? | Ampliación de la v2 |
|---|---|---|---|
| identidad de las ejecuciones (orden registrada completada que contiene el blob fijado) | reconstruct (filtro inst) | sí | — |
| argv: forma declarada, Git e intérprete por ruta absoluta, -I, orden conocida, argumentos simples | split_outer, _strategy, lex_pwsh, _abs_exe, gramática interior pwsh | sí | shell: _abs_shell admite componentes que empiezan por punto (solo la posición del shell); renderizados R2 (-NoProfile) y R3 (cmd.exe /c) con gramática cerrada nueva (_parse_cmd_inner) |
| bytes: exactamente un sobre, canónico, ASCII, terminado en LF, escrito sin alterar | extract_envelope; reconstruct (_raw) | sí | — |
| resultados y códigos de salida: exit_code entero, status coherente, Error nulo si y solo si exit 0 | reconstruct | sí | — |
| custodia: registro completo y legible; sin él no hay Runs | parse_log; reconstruct (Reconstructible) | sí | — |

## 9. Hallazgos

**Hallazgos de los preflights anteriores (blobs `a2aed3cc894037d6b42d462fbec9d6d0c7f629a6` y `df8925b26229e7b678d450faa9e983cb5ef6cc13`) frente al texto vigente:**

| Id | Hallazgo | Corrección del texto | Estado ahora | Casos |
|---|---|---|---|---|
| F1 | regla 5 (a) literal: sin cierre en FX-U1 (PROMPT_TEMPLATES.md) | 5 (a): sin entrada; la tratan 16.3, 16.13 o 16.24 | resuelto: E A7 = E R7-A en las 11 y en el kit | REPRO-*, REPRO-OLD-5A, NEG-ABS-AUTH(-OLD) |
| F2 | `<TaskId>` de 2 (c) frente a la ruta de los controles | 2 (c): ruta de la tarea o del control (RAE §10), directorio = `RunId` de los bytes | resuelto: las 11 cumplen; N7AR-SAME rechazada | REPRO-OLD-PATH, NEG-PATH-RUNID |
| F3 | «una ruta figura una vez por cada blob distinto» | sustituida: un blob por revisión en que difiera; la misma entrada en varias listas si 16.24 lo exige | resuelto | REPRO-UNIQ |
| F4 | contrato o delegación ausentes | 5 (c): sin cierre y sin lanzamiento; entrega o TRX: sin entrada | resuelto | NEG-ABS-CONTRACT-*, NEG-ABS-DELEG-*, NEG-ABS-H-* (como N4), NEG-ABS-TRX-* |
| F5 | «los archivos del mapa» en lectura amplia | 1 (a) y 2 (a) estrechadas | resuelto: un ADDED del mapa es FOREIGN | NEG-FOR-WIDE |
| F6 | lecturas de `clausemap` fuera de A7-1 | residuo declarado en §3.5 y Q-A7-06; comprobación de fallo cerrado del kit (S-03) | residuo esperado y bloqueado por la precondición | B1-UC-R9, CIT-B1-UC |
| F7 | E3 no nombrada | `WORKFLOW.md` y `AUTOMATION_PLAN.md` en Evaluate (`MainSha_eval`) | resuelto | REPRO-* |
| F8 | cabecera de `reconstruct_runs_m7.py` | fuera de A-7 | sin cambio (precisión de la cabecera, neutral) | RECON-HEADER |
| F9 | precedencia entre categorías | regla 1: al menos una; con varias, el blob de cada referencia | resuelto | NEG-POS-OVERLAP |
| F10 | la precondición de §3.5 («blobs distintos en esas revisiones») bloqueaba el caso B1-like principal por la cita EXTERNAL de AUTOMATION_PLAN.md | §3.5: criterio de cobertura (toda lectura de contenido de `clausemap` de una cita está fijada por el cierre) | resuelto: el principal se puede lanzar, B1-UC sigue bloqueado, FX-U1 pasa; la redacción anterior queda como evidencia negativa (modo `text`) | CIT-B1-MAIN, CIT-B1-UC, CIT-FXU1 |
| F11 | ausencia de la ruta en alguna revisión, no definida en §3.5 | «una revisión en que la ruta no existe no aporta lectura» | resuelto: una cita ausente en su revisión de lectura y leída en otra revisión queda fuera del cierre y bloquea por cobertura | CIT-* (modo `coverage`) |
| F12 | «con el blob registrado»: el formato de designación solo registra el blob del contrato | 2 (c): la designación «registra la ruta y el blob de cada uno» (contrato, delegación, entrega y TRX) | texto resuelto; el formato `a6m7-designation/1` del banco no se cambia: registra solo el blob del contrato, y la próxima preparación del kit tiene que registrar los de la delegación, la entrega y el TRX | DES-REG, DES-REG-B1, NEG-ABS-*-OTRO-BLOB |

**Nota para la próxima preparación del kit (F12).** Con la regla 2 (c) vigente, cada designación tiene que registrar la ruta y el blob de los cuatro artefactos. El formato `a6m7-designation/1` que usan el banco y el kit actual solo trae `Blob` para el contrato; el validador comprueba el blob registrado cuando está y avisa cuando falta (`UnregisteredBlobs`; DES-REG: DELEG, H y TRX en las once designaciones del banco). Las designaciones B1-like de este preflight ya registran los cuatro (DES-REG-B1). El formato del banco no se ha cambiado.

**Observación sin efecto.** La regla 2 (c) solo restringe el segmento del `RunId`; la función `Name(p)` de C6 (A6-9) sigue necesitando `artifacts/orchestration/<u>/<t>/<a>/<RunId>/output.json|worker-handoff.json`, que `designate_m7.py` (KPR-2) comprueba antes de derivar. Las once designaciones del banco y las B1-like cumplen ambas.

## 10. Archivos

Publicables en `docs/automation/evidence/I-62-A7/` (este directorio). `MANIFEST.sha256` lista todos, incluido este documento.

| Archivo | SHA-256 |
|---|---|
| `a7-guards.py` | `8a7f91d13e74952ffe350c2f907fafb4a34157eb3a40b33ec096f886b1f31e9e` |
| `check_a7_closure.py` | `99dfaeb942e91bdda78601c61efb7f062bc2fc5640ec48a4a14b2e56c540cb9c` |
| `check_citation_blobs.py` | `65fad0645c6b5cfbde8f19c833403ed5d25f1dd10ab8eca7ebea302f998d641c` |
| `derive_r4.py` | `3de12c54e3893b7c8689825f69bc8fd2a9cea767eaa6d0f77d5361f1ff29c426` |
| `designate_m7.py` | `491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6` |
| `make_eic_a7.py` | `91263e991faa91dfcf781f003d04e4f05a42ffe20d0f8ab737986db749742e1a` |
| `make_eic_verify.py` | `58f13c6a8564fe9877e3a5bb40e8d46fccf1ce67ac174ccd6a4298ddad03a8d2` |
| `p0-env.json` | `2e780484d1494041c7d0b7f5530f64692bcb5b24d235edcda26d3c554b351d7a` |
| `p1-repro.json` | `19dab08fa2dd1d2e8262ebab7f1cf2ddc3f9ea97747656cd6874e21cf9871dc1` |
| `p2-b1.json` | `f7897712a051bc0d6ff8c6b83c85cfcfd3effa2977bd5f104a93871ae28f1377` |
| `p3-neg.json` | `564c174450a2d63bddbb2d573b1487dbfeccd16b1e0472fddff852b8febcdd19` |
| `p4-guards-preview.json` | `c34c2100dd37b8e5b6119539cc6d3cfa2501642b35321f338cb899d0dea63e08` |
| `p4-guards-selftest.json` | `7a670e4945fece2c6a54d24d7eadc052d1c721c5693aabac8b91c49ecd9c6732` |
| `p4-guards.json` | `2568b8a114395fe61c1b71b9aee05634d422b2ac80ebd68b088edd3c04efdd3c` |
| `p5-compat.json` | `c5c18cd6919d53bbdd1e4f540f2bbf71696772b0f1e500b577601ec36f6382bc` |
| `p6-recon.json` | `7e2b108ad00e9d70f8682252dac92fc6385fd79840e79e9a245e251cf17bb378` |
| `preflight_text.py` | `0b980e597fc851b6ffd93244262e9b18400dd87a212b9ad88da8971f22bc88dd` |
| `recon_static.py` | `07a3a78d02840dff10e541b9fcdf2e36656dbb7612663180c2cf26f9bddfb6bd` |
| `reconstruct_runs.py` | `5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da` |
| `reconstruct_runs_m7.py` | `1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab` |
| `run_a7.py` | `33434ac208cc8f5d9069f256e421189b9dff277cae4573536140830e300f3ee8` |
| `summary.json` | `28146d11613fd9295037d8973719d080f65ad4876ad4fc848bb42df3878a7e68` |
| `b1-like/main/FX-U2-T1.gate-contract.json` | `b759226ed289425ff972908c16f243dbbb6494eec510a3698e3897da5cae2239` |
| `b1-like/main/deleg.json` | `aa7a956a532526b61238b41b311210f9b944ae00c1c05b45919d2beb6dae7944` |
| `b1-like/main/designation.json` | `41ea369f8b0c9291777c09a195bc4e48a225e63cb2bd1ca0abe1a46a4108a6b0` |
| `b1-like/main/fixture-tests.trx` | `4ccdf2cecba561259b877a471d577f1afa6ac4a9ad00ceb4fa0417e7c61a7513` |
| `b1-like/main/handoff.json` | `725d9147e239d5b1ff041a85b564d255d61f21b40990b6376af94c09e320fa71` |
| `b1-like/uc/FX-U2-T1.gate-contract.json` | `7ddd714a92a7065a66dc59d99f804cc776476fd01e425747030f3a7fe414e932` |
| `b1-like/uc/deleg.json` | `c495a441ce3ff134a801cae004010e4f6aebb902f20a46d0ae53d73e0447c728` |
| `b1-like/uc/designation.json` | `4c67e18ecee32206dedd364417a90e5dfc999753c31531e67e16447a30c22c2a` |
| `b1-like/uc/fixture-tests.trx` | `4ccdf2cecba561259b877a471d577f1afa6ac4a9ad00ceb4fa0417e7c61a7513` |
| `b1-like/uc/handoff.json` | `7f199a2f06fc044b1ab34a076d56fe64f99a5c141e7b7e65414b9b2f1f150adc` |

