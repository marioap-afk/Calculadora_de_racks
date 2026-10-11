# B-min — paquete experimental de herramientas de hechos y reproducción independiente (I-62, decisiones §75)

> **Experimental, no normativo y fuera de producción.** No entra en F_seed, ni en F_norm, ni en ningún contrato de producción. No se ejecutó ningún
> modelo: ni Codex, ni `claude`, ni otro. No declara la elegibilidad de ningún Controller, ni FX-02 PASS, ni F6 GATE PASS. La especificación que rige
> es `SPEC.md` v0.2, con su fe de erratas (§5).

## 1. Qué es

| Pieza | Archivo | Autor | Hace | No hace |
|---|---|---|---|---|
| Herramienta de hechos | `tool/bmin_facts.py` | autor A (solo la SPEC y AP `f525cb1e`) | 12 órdenes de hechos reproducibles (Git, Scope, contrato, hashes, árbol, lecturas fieles, TRX, entrega, texto libre y mapa de cláusulas de 16.13/16.14) | ningún PASS/FAIL/VERIFIED, ninguna escritura, red ni proceso distinto de `git` |
| Aceptante | `accept/bmin_accept.py` | autor B, independiente (sin ver el código de A) | R1-R10; recalcula los controles críticos con primitivas distintas y rechaza toda discrepancia | `EXECUTION_*`; usar la herramienta como oráculo |
| Controller | — (fuera del paquete) | — | interpreta los hechos y emite su resultado estructurado (§75.3) | — |

Ninguno de los dos autores leyó `probe_tools.py`, `mv_check.py`, `clause_map.py` ni las referencias o comparaciones de las sondas. El oráculo de C-24 no
se comparte con nadie.

## 2. Resultados

| Prueba | Resultado |
|---|---|
| Suite de la herramienta (`tool/tests/test_bmin_facts.py`; reejecutada por la supervisión, `tool-suite.log`) | **66 OK**: unitarias, negativas de fallo cerrado, cero escrituras, determinismo, ASCII y vocabulario |
| Mutación de la herramienta (`tool/tests/mutate.py`; `TOOL-NOTES.md`) | **27/27 mutantes muertos** por sus pruebas, en la corrida de su autor; la supervisión no la repitió, pero ejecutó los 27 mutantes contra el aceptante (abajo) |
| Suite del aceptante (`accept/tests/test_bmin_accept.py`; reejecutada por la supervisión, `accept-suite.log`) | **52 OK**: R1-R10 con caminos MATCH y MISMATCH, mutantes de la herramienta falsa muertos, cero escrituras y determinismo |
| Extremo a extremo sobre un clon nuevo del fixture (`e2e/`, fuera de `D:\r62-fixture`) | 13 órdenes de la herramienta con exit 0; **cero escrituras** (índice, refs, `status --ignored` y lista de archivos iguales antes, después de la herramienta, después del aceptante y al final: True); salidas solo ASCII: True |
| Aceptante sobre las 13 salidas reales | **ACCEPT_FACTS**: R1 MATCH, R2 MATCH, R3 MATCH, R4 NOT_APPLICABLE, R5 MATCH, R6 MATCH, R7 MATCH, R8 MANUAL, R9 MATCH, R10 MATCH |
| Salida alterada (una ruta menos en `diff`, con y sin `FactsSha256` recalculado) | keep-sha → REJECT_FACTS (R3, R5); recompute-sha → REJECT_FACTS (R3, R5) |
| 27 mutantes de la herramienta real, ejecutados sobre los datos reales y juzgados por el aceptante | **13/27 rechazados**, que son **todos los que cambian alguna salida** (13/13); los 14 restantes dejan las 13 salidas idénticas con estos datos y los matan las pruebas unitarias (tabla del §5) |

## 3. Criterios de aceptación de §75.5

| Criterio | Evidencia |
|---|---|
| Integridad de entradas | commits por SHA completo (`BAD_REVISION`); archivos con `--sha256` (`INPUT_HASH_MISMATCH`); R1 (blob de la herramienta), R9 (lecturas dentro del cierre declarado, con `RevPaths`) y R10 |
| Cero escrituras | lista cerrada de subórdenes de Git y entorno `GIT_OPTIONAL_LOCKS=0`; análisis AST de las dos piezas; instantáneas antes y después en las suites y en el extremo a extremo |
| Determinismo y reproducibilidad | bytes de `Facts` iguales en dos ejecuciones (suites); R3 reejecuta cada orden y compara los bytes y `FactsSha256` |
| Fidelidad UTF-8 por ASCII | JSON canónico con `ensure_ascii`; todos los bytes < 0x80; pruebas con «—», «ó» y «ñ»; en el fixture, `read-text` de la línea 1 de `FX-U1.md` sale como `\u2014` y `\u00f3` |
| Comprobaciones negativas | negativas de fallo cerrado (SHA inexistente, archivo ausente, SHA-256 distinto, JSON o XML ilegibles, trailer duplicado, EFF ambiguo, palabra clave de esquema no soportada, suborden prohibida, sin `-I`, dos `claim_id` distintos); salidas alteradas |
| Implementación independiente para los controles críticos | aceptante de otro autor con otras primitivas: `ls-tree` en lugar de `diff`, recorrido propio de padres, trailers desde el mensaje crudo, SHA-1 de blob propio, cobertura y U-19 propias, Derive y MV-1..MV-7 propios; R5-R7 MATCH sobre el fixture |
| Fallo cerrado ante datos ausentes o contradictorios | `Facts` = null más código y exit 2 en la herramienta; REJECT_FACTS en el aceptante ante cualquier MISMATCH o dato ausente (R1, R2, R3 y R9 no admiten dispensa) |
| Divergencia MV-4/MV-7 resuelta sin tocar las sondas | §4 |

## 4. MV-4 y MV-7 (§75.5; análisis estático de la evidencia §143)

La herramienta no elige lectura. Informa cada regla con su ancla y deja al Controller la tabla de defectos (AP L910-L918).

**Hechos sobre el fixture** (EFF `fbe25347799e5b801ef708454212335537448bd1`, mapa `4d49d3e1`):
- **MV-4:**
  - `LiteralViolations` = 7 (L905);
  - `AddedPresentInEff1` = 23 (R61 b, L835; L916);
  - `EntryFilePresentInEff1` = 1 (sin regla que lo convierta en defecto).

  Esto explica los dos recuentos de la sonda: el oráculo contaba 7 y la celda 31, que suma las tres listas.
- **MV-7:** `PointerIsFirstSentenceS16` = True, `PointerIsLastSentenceS163` = True, `S163EqualAfterPointerRemoval` = False. El puntero se quita solo como frase, nunca la línea entera (defecto latente de la
  celda, evidencia §143).

Los resultados brutos de las sondas no se modifican.

## 5. Mutantes de la herramienta frente al aceptante, con los datos reales

| Mutante | Descripción | Salidas que cambian | Aceptante |
|---|---|---|---|
| M01 | coverage rule inverted (E starts with R) | scope | REJECT_FACTS (R7) |
| M02 | sentence boundary off ('.' without following whitespace splits) | clausemap | REJECT_FACTS (R6) |
| M03 | ancestry argument order swapped | identity | REJECT_FACTS (R5) |
| M04 | one diff path dropped | diff, scope, clausemap | REJECT_FACTS (R5, R7) |
| M05 | MV-4 lists merged (R61 b list folded into the literal list) | clausemap | REJECT_FACTS (R6, R7) |
| M06 | pointer removed together with its whole line | clausemap | REJECT_FACTS (R6) |
| M07 | ENTRY file treated as ADDED (R61 b list) | clausemap | REJECT_FACTS (R6, R7) |
| M08 | first-parent merge order reversed | — | **no detectado con estos datos** |
| M09 | trailer matched by substring instead of exact line | — | **no detectado con estos datos** |
| M10 | ensure_ascii=False in the canonical serialization | read-text-1, read-text-73, read-json, clausemap | REJECT_FACTS (R3, R6, R7) |
| M11 | missing -I check removed | — | **no detectado con estos datos** |
| M12 | U-19 normalization removed | scope | REJECT_FACTS (R7) |
| M13 | raw blob header without the NUL separator | hashes | REJECT_FACTS (R5) |
| M14 | free-text whole-word boundary removed | — | **no detectado con estos datos** |
| M15 | closed list of git subcommands removed | — | **no detectado con estos datos** |
| M16 | duplicated trailer accepted | — | **no detectado con estos datos** |
| M17 | unsupported schema keyword accepted | — | **no detectado con estos datos** |
| M18 | MV-3 deleted paths dropped from the changed set | — | **no detectado con estos datos** |
| M19 | state with two distinct claim_id values accepted | — | **no detectado con estos datos** |
| M27 | claim_id lines counted instead of distinct values (real /v2 state rejected) | clausemap | REJECT_FACTS (R3, R6, R7) |
| M20 | last sentence of 16.3 taken as the first | clausemap | REJECT_FACTS (R6) |
| M21 | EFF candidate without the first-parent condition | — | **no detectado con estos datos** |
| M22 | preamble omitted from H1/H2 | — | **no detectado con estos datos** |
| M23 | status --porcelain no longer forced | — | **no detectado con estos datos** |
| M24 | code fences ignored (lines inside ``` count as headings) | — | **no detectado con estos datos** |
| M25 | duplicate JSON keys accepted | — | **no detectado con estos datos** |
| M26 | --no-ext-diff --no-textconv no longer forced | diff, scope, clausemap | REJECT_FACTS (R9) |

Un mutante «no detectado con estos datos» no cambia ningún hecho del escenario de FX-U1 (por ejemplo, el que acepta un trailer duplicado: el fixture
no lo tiene). Esos mutantes los matan las pruebas unitarias de la herramienta, sobre repositorios sintéticos.

## 6. R1-R10 en el extremo a extremo

| R | Estado | Nota |
|---|---|---|
| R1 identidad de la herramienta | MATCH | blob calculado por el aceptante = blob fijado |
| R2 intérprete | MATCH | `-I`, versión y SHA-256 de la imagen del intérprete |
| R3 reejecución | MATCH | bytes de `Facts` y `FactsSha256` iguales en las 13 órdenes |
| R4 de los hechos a la decisión | NOT_APPLICABLE | sin salida del Controller en la integración; cubierto por las pruebas del aceptante |
| R5 Scope y Git | MATCH | implementación independiente |
| R6 Authority (Derive, MV) | MATCH | implementación independiente |
| R7 reglas con lectura normativa | MATCH | cobertura, U-19 y las cuatro listas de MV-4 |
| R8 texto libre de la verificación | MANUAL | control manual del Coordinator |
| R9 lecturas en el cierre | MATCH | cierre declarado: commits del clon, `RevPaths` de los commits del escenario y superficies de 16.13 |
| R10 señales de la sesión | MATCH | SHA-256 de los hechos del relevo (sintéticos) |

## 7. Límites declarados

- No cubierto todavía:
  - la rama de rebase y patch-id de `Identity`;
  - la coherencia de `Trailer` con el modelo efectivo;
  - R4 sin una salida real del Controller.
- El intérprete `python` no forma parte de la receta 16.4 ni del descriptor del adapter; R2 lo fija y lo declara.
- La colocación de la herramienta en una medición queda abierta, porque es la pregunta de materialidad de §75.4: entrada canónica fijada por blob,
  ejecutada desde el objeto (`git cat-file blob <b> | python -I -`), fuera de F_seed y F_norm.
- Tres desajustes de formato entre las dos implementaciones salieron en la integración (vocabulario de entradas reproducidas, forma de
  `U19Normalized` y pathspec `:(literal)`). Se resolvieron con la fe de erratas v0.2 de la SPEC; el aceptante los aplicó sin ver el código de la
  herramienta.

## 8. Archivos

| Archivo | Blob | SHA-256 |
|---|---|---|
| `SPEC.md` | `4d448c6fc467` | `510143f24522211e…` |
| `tool/bmin_facts.py` | `9a829b0eeb12` | `b03c3a19cef60838…` |
| `tool/TOOL-NOTES.md` | `d3218ff79c84` | `f56781bf5cd5fd10…` |
| `tool/tests/test_bmin_facts.py` | `0a74bb2dcf18` | `31f78a4a597f9655…` |
| `tool/tests/mutate.py` | `696793895421` | `24d33bd7589e5e8a…` |
| `tool/tests/zero_write_check.py` | `4188d256b76b` | `ad3408e1864bb691…` |
| `accept/bmin_accept.py` | `434ef1fccb0d` | `173514aa353f6add…` |
| `accept/ACCEPT-NOTES.md` | `cb9df0289c69` | `3751f21685c18c84…` |
| `accept/tests/test_bmin_accept.py` | `fb6fbeeeab67` | `a8602cf05dad495b…` |
| `accept/tests/fake_tool.py` | `3a3e3d92b2ff` | `53df3eb016c4e381…` |
| `e2e/e2e.py` | `fbfa3353a1a7` | `e67230f02a4c2eab…` |
| `e2e/e2e-run.log` | `9a233b14c1c4` | `7a1fa98c1b89ce10…` |
| `tool-suite.log` | `7b01bb7e5303` | `2f51554295890ea0…` |
| `accept-suite.log` | `a2c66962aeb9` | `328f135a4a906c7e…` |
| `e2e/out/acceptor-result.json` | `de2d2c92181c` | `7f9bfd9c881bfc8f…` |
| `e2e/out/e2e-summary.json` | `5b5972190d89` | `b165b686067f73fc…` |

Las salidas `e2e/out/envelope-*.json` son las 13 salidas canónicas de la herramienta sobre el fixture; sus SHA-256 están en `e2e/out/e2e-summary.json`.
