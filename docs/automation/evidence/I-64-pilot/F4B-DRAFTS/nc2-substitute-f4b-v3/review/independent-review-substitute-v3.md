# Revisión independiente y adversarial: sustituto de nc2 v3 (`PBTx-NC2-EXACT-WHITELIST-F4B-01`, v3)

**Objeto:** `nc2_exact_whitelist_substitute_f4b_v3.py`, SHA-256 `E68B6E5993B522DF83E7CD4CD2B136DDF503E237FD560B1A97067F24664328C6`, frente a la v2 revisada `nc2_exact_whitelist_substitute_f4b_v2.py`, SHA-256 `C033E96CE147963FEB9C04733E9BFBEC7493FEA33D8FF79B1C2E8AB860798E45`.

**Marco:** la decisión del Owner «Sustituto v3 y revisión (Recomendado)» (`hs65/pbt4dcc/owner-pbt4d-nc2-decision.md`). No participé en escribir la v3. El diagnóstico de la sesión (`_session_t4d/diag_nc2_p10.py`) lo usé solo como contraste al final, no como evidencia.

**Método:** traté de demostrar que la v3 no es equivalente a la v2, o que da un PASS indebido. No lo conseguí. Todo lo que generé está en `hs65/nc2-substitute-f4b-v3/review/`. El worktree y el checkout principal quedaron solo en lectura.

## Veredicto: APTO

| B | I | M |
|---|---|---|
| 0 | 0 | 4 |

- **El cambio de lógica es solo la propiedad 10.** El AST lo confirma: lo demás es idéntico a la v2 nodo a nodo.
- **La propiedad 10 de la v3 es lógicamente equivalente a la de la v2.** La demostración está en el §2. En 300 019 comparaciones sintéticas no hay ninguna discrepancia, y tampoco en 6000 perturbaciones de `evaluate()` completo.
- **La autoprueba** pasa 63 de 63, es determinista e idéntica a la de la v2.
- **En los cuatro casos reales en los que la v2 termina,** la v3 da el mismo `SubstituteResult`, las mismas `Properties` y los mismos `Measured`. Solo cambian el campo informativo y `ScriptSha256`.
- **Sobre PB-T4d, la v3 da PASS 10/10.**
- **Ninguna mutación de PB-T4c da PASS en la v3 con FAIL en la v2.**

Los cuatro M son menores: textos o un campo informativo. Ninguno afecta a una propiedad ni al resultado.

## 1. El diff v2 → v3

**SHA-256:** los dos coinciden con los declarados (`diff-ast-check.txt`). Ninguno de los dos archivos lleva CRLF ni BOM, y los dos tienen los mismos caracteres no ASCII («»).

**El generador:** copié `_make_v3.py` y la v2 a `review/regen/`, con la misma estructura de carpetas, y lo corrí. Reproduce la v3 byte a byte (`E68B6E59…`).

**`diff`** (`diff-v2-v3.txt`) da tres bloques:

| v2 | v3 | Qué cambia |
|---|---|---|
| l. 1 | l. 1 | docstring: `(v2)` pasa a `(v3)` |
| l. 3 | l. 3-4 | docstring: una línea nueva «Version 3 (…)» y la línea «Uso» con el nombre de la v3 y `--task PBT4` |
| l. 210-220 | l. 211-214 | el comentario de la propiedad 10; sale la enumeración (`cands`, `itertools.combinations`, `excl`); el campo `AlternativeCandidatesExcludingPath` pasa de `len(excl)` a una cadena; la propiedad 10 pasa a la forma cerrada |

**El AST** (`diff_ast_check.py`):
- Hay 31 nodos de nivel superior. Solo difieren el docstring (nodo 0) y `evaluate` (nodo 21).
- En `evaluate`, 43 sentencias pasan a 40. Solo cambia el bloque de la 10: 5 sentencias de la v2 (`cands = []`, el `for`, `excl`, el campo y la propiedad) pasan a 2.
- Ninguna sentencia de la v3 lee `cands` ni `excl`.
- Son idénticos, por tanto: las propiedades 1 a 9, `covered`, `scope_predicate`, `readme10_mutation`, `plain_a3_a5`, `synthetic`, `EXPECT_FAILED`, `check_task`, `selftest`, `load_inputs`, `read_json_object`, `main`, el análisis de argumentos, `ID_GENERIC`, `TASKS` y `F4B_EXACT_EVIDENCES`.

**Respuesta a la pregunta 1:** no cambia nada más que el docstring, el campo informativo y la propiedad 10. El único resto es que `import itertools` queda sin uso (M-3).

## 2. La equivalencia lógica de la propiedad 10

### 2.1 Notación
Valores en el punto de la propiedad 10:
- `p` = `removed[0]` si hay exactamente una ruta quitada; si no, `None`. `removed` sale de `changed`, que en `main` es `git diff --name-only … .split()`, una lista de `str`.
- `ca` = `ct.get('AllowedWriteScope', [])` y `F` = `ct.get('ForbiddenWriteScope', [])`.

Condiciones:
- A = `p is not None`.
- B = `covered(p, ca)`.
- C = `len(excl) > 0`.
- D = `all(c != ca for c in excl)`.
- E = `not covered(p, F)`.

Las dos versiones:
- v2: `P10 = A and B and C and D and E`, evaluada después de construir `cands` y `excl`.
- v3: `P10 = A and B and E`.

### 2.2 Teorema
Si la v2 termina el bloque sin excepción, su P10 es igual a la de la v3, para cualquier `p`, `ca`, `changed` y `F`.

**Demostración.**
- Si no se cumple A o no se cumple B, las dos dan `False`, con el mismo cortocircuito.
- Supongamos A y B. Basta ver que C y D se cumplen.
- **C:** con `r = 0`, `combinations(ca, 0)` da una sola tupla vacía, y `cands.append([])` es incondicional. `covered(p, [])` es `any` sobre un vacío, así que da `False`. Con A, el filtro de `excl` acepta `[]`. Luego `[] ∈ excl` y `len(excl) ≥ 1`. Con A, la variante de README 10 de `[]` también es `[]`, de modo que en realidad `len(excl) ≥ 2`; es el mínimo medido. Esto no depende de `changed`: si `changed` está vacío, se añade `list(comb)`, que también es `[]`.
- **D:** supongamos que existe `c ∈ excl` con `c == ca`.
  - Que `c` esté en `excl` significa `not covered(p, c)`.
  - La igualdad de listas en Python significa la misma longitud y `c[i] == ca[i]` en cada posición.
  - `covered(p, ·)` solo mira cada elemento a través de `p == e`, `e.endswith('/')` y `p.startswith(e)`. Con dos `str` iguales, esas tres expresiones dan lo mismo.
  - Con A, `changed` no está vacío, porque `removed ⊆ changed`. Por eso la v2 llama a `readme10_mutation` sobre la combinación completa, que evalúa `e.endswith` en todo elemento distinto de `sorted(changed)[0]`. Así que, si la v2 termina, todos los elementos de `ca` son `str`.
  - Entonces `covered(p, c) = covered(p, ca) = True` (por B), lo que contradice que `c` esté en `excl`. D se cumple.
- Con A y B se cumplen C y D, así que v2 = A ∧ B ∧ E = v3. ∎

**Los puntos que pedía mirar el encargo:**
- **Prefijos que acaban en `/`, `p` igual a un prefijo y duplicados:** la demostración solo usa que `covered` depende del valor de los elementos. No usa ni la forma de las entradas ni que sean únicas. `combinations` trabaja por posiciones, así que las listas con duplicados también entran.
- **`readme10_mutation`:** sus variantes solo añaden candidatas a `cands`. Ni C ni D dependen de ellas.
- **`changed` vacío:** en `evaluate` implica `p = None`, y las dos versiones dan `False`. Con un `p` arbitrario y `changed` vacío, `[]` sigue en `excl`.

### 2.3 Las excepciones
**Toda excepción de la v3 la lanza también la v2** (o antes):
- `len(ca)`, en el campo de la v3, también lo calcula la v2 en `range(len(ca) + 1)`.
- `covered(p, ca)`: la v2 evalúa `covered(p, list(ca))` para la combinación completa al filtrar `excl`.
- `covered(p, F)` se evalúa en las mismas condiciones en las dos.

**La v2 puede lanzar donde la v3 no lo hace.** Pasa en dos casos:
- (a) agota los recursos: `MemoryError` con 2^37 subconjuntos en PB-T4d;
- (b) `ca` tiene un elemento que no es `str` (`AttributeError` en `readme10_mutation` o en `covered`).

**El caso (b) no puede dar un PASS indebido.**
- Un PASS exige la propiedad 1, y con ella `ra == ca`, de modo que `ra` lleva el mismo elemento.
- La propiedad 2 exige A, y con ella un `changed` no vacío.
- Entonces `readme10_mutation(ra, changed)` (l. 123) lanza la misma excepción en las dos versiones antes de llegar a la 10.
- Mis cuatro sondas (`evaluate_equiv.py`, entero, `null`, lista y objeto en el alcance del contrato y en el de la delegación real) dan FAIL por excepción en las dos.

**El caso (a)** es la diferencia buscada. Con recursos ilimitados la v2 terminaría, y el teorema dice que su resultado sería el de la v3.

### 2.4 Observación O-1: la 10 es consecuencia de la 1 y la 2 (heredado; no cuenta)
- La propiedad 1 exige `ra == ca` y `rd.get('ForbiddenWriteScope') == ct.get('ForbiddenWriteScope')`, porque `ForbiddenWriteScope` está en `SEMANTIC_OPAQUE`.
- La propiedad 2 exige A, `covered(p, ra)` y `scope_predicate(changed, ra, F_rd) == 'pass'`. Esto último incluye que `p` no esté cubierto por F.
- Luego las propiedades 1 y 2 implican A ∧ B ∧ E, que es la 10 en cualquiera de las dos versiones. Ninguna entrada puede fallar solo por la 10, y el resultado global no depende de ella.

Lo comprobé de forma empírica:
- de 6000 perturbaciones, 864 tienen en true la 1 y la 2, y ninguna de ellas da la 10 en false;
- en la autoprueba, la 10 es falsa solo en B y en M, y siempre junto con otras.

Consecuencias:
- La v3 no puede cambiar ningún veredicto en las entradas en las que la v2 termina.
- La 10 tampoco aporta una discriminación propia, ni en la v2 ni en la v3. No es un defecto de la v3.

## 3. La equivalencia empírica

### 3.1 La propiedad 10 aislada (`p10_equiv.py` → `p10-equiv-result.json`, `p10-equiv-run.txt`)
El guion extrae el bloque de la 10 tal cual del texto de cada script, desde `# 10.` hasta `return P, N`. Lo ejecuta con los auxiliares de su propio módulo. Comprueba además que `covered` y `readme10_mutation` tienen el mismo bytecode en los dos.

**Exhaustivo** (240 000 entradas):
- `ca`: todas las listas de longitud 0 a 3, con orden y repetición, sobre 7 fichas: `a`, `a/`, `a/b`, `a/b/`, `b`, `ab` y la cadena vacía;
- `p`: 10 valores, incluidos `None` y los prefijos;
- `changed`: 10 listas, con la lista vacía, duplicados y `p`;
- prohibidas: 6 listas, con `p` y prefijos.

**Aleatorio** (60 000 entradas, semilla 20261122):
- `n` de 0 a 12 (9757 casos con `n` de 9 a 12) y 20 fichas con prefijos, `/` y la cadena vacía;
- duplicados forzados;
- en un 35 % de los casos, `p` se obtiene como en `evaluate` (real = `ca`, mutado por README 10).

**Resultado:** 300 019 comparaciones y **0 discrepancias**.
- La 10 da `True` en 76 204 y `False` en 223 815.
- Cobertura: `p = None` en 46 562; `p` que acaba en `/` en 62 576; `ca` con duplicados en 117 548; `changed` vacío en 33 125; `p` cubierto solo por un prefijo en 29 524; `p` cubierto por el prohibido en 74 185.

**19 bordes con nombre, todos iguales:**
- `p` igual a un prefijo;
- `p` cubierto solo por un prefijo, o por un prefijo y por la ruta exacta;
- `changed` vacío, con `p` y con `p = None`;
- alcance vacío;
- `p` duplicado;
- el mismo prefijo duplicado;
- `p` prohibido exacto o por prefijo, y un prohibido `ruta/` que no lo cubre;
- la cadena vacía;
- `/`;
- README 10 sobre un prefijo;
- `p` distinto de `sorted(changed)[0]`;
- sin la clave `ForbiddenWriteScope`.

**Sin cadenas** (9 sondas):
- Las de entero, `null`, lista u objeto *después* de la ruta difieren: la v2 lanza `AttributeError` y la v3 da `True`. Es el caso (b) del §2.3, inalcanzable para un PASS.
- Las otras cinco coinciden: un entero antes de la ruta, un booleano, `p = None`, el alcance `null` (`TypeError` en las dos) y el alcance como cadena.

### 3.2 `evaluate()` completo (`evaluate_equiv.py` → `evaluate-equiv-result.json`, `evaluate-equiv-run.txt`)
- **`synthetic()`:** los 57 paquetes de la v2 y de la v3 son iguales. En los 57, `evaluate` da la misma `P` y la misma `N` salvo el campo informativo.
- **Perturbaciones:** 6000, semilla 7, sobre los paquetes A, N, O y AY. Cambian los alcances del contrato, de la delegación real y de la mutada, el diff y el prohibido, con `n ≤ 12`, prefijos y duplicados, y con el bfc recalculado para que la 1 pueda seguir en true. Resultado: **0 discrepancias** en `P`, en `N` sin el campo informativo y en el resultado global. 483 de ellas dan PASS en las dos versiones.

## 4. La autoprueba
- **No hay un modo aparte.** `main()` ejecuta `selftest()` dos veces al empezar toda invocación y exige que las dos coincidan.
- **Cómo la corrí:**
  - dentro del proceso: `selftest()` dos veces por versión (`evaluate_equiv.py`);
  - en las 9 corridas reales (5 de la v3 y 4 de la v2; `selftest-across-runs.txt`).
- **Resultado:**
  - 63 de 63 (57 sintéticos y T1-T6) y determinista;
  - la misma lista, caso a caso, en la v2, en la v3 y en todas las corridas;
  - dan PASS A, N, O, AY, T1 y T4;
  - los 43 casos con `EXPECT_FAILED` rompen exactamente su conjunto: O y AY ninguno, W la 1 y la 8, 28 casos la 5 y 12 casos la 8.
- **Ningún caso aísla la 10** (O-1), así que la autoprueba no detectaría un error de la propiedad 10. La equivalencia descansa en el §2 y el §3, no en la autoprueba.

## 5. Los casos reales: la v2 frente a la v3
Corridas con `python -I -B <script> --task …`, el cwd en el worktree, los mismos argumentos que las corridas registradas y la salida en `review/runs/` (`run_case.py`, `cases.json`). La comparación campo a campo está en `compare_runs.py`, `compare-light.txt` y `compare-pbt4c.txt`.

| Caso | `--task` | Rutas en el AWS | v2 | v3 | `Properties` | `Measured` salvo el campo | Autoprueba | Lo único que difiere | Tiempo v2 / v3 |
|---|---|---|---|---|---|---|---|---|---|
| PB-T2a (real `10a9`, nc2 `a799`) | PBT2A | 4 | PASS (campo: 24) | PASS | iguales, 10/10 | iguales | iguales, 63/63 | el campo y `ScriptSha256` | 0,4 s / 0,9 s |
| PB-T3a (copias de la revisión v2: real `6d41`, nc2 `ad66`, planificación `fc50`) | PBT3A | 14 | PASS (24 576) | PASS | iguales, 10/10 | iguales | iguales | el campo y `ScriptSha256` | 0,5 s / 0,8 s |
| PB-T4a (real `8d11`, nc2 `4cda`) | PBT4 | 10 | PASS (1536) | PASS | iguales, 10/10 | iguales | iguales | el campo y `ScriptSha256` | 0,4 s / 1,0 s |
| PB-T4c (real `dd13`, nc2 `2fc5`) | PBT4 | 26 | PASS (100 663 296) | PASS | iguales, 10/10 | iguales | iguales | el campo y `ScriptSha256` | **290,6 s, pico de unos 25 GB** / 1,1 s |

Contrastes con lo registrado:
- **PB-T4a y PB-T4c:** mi corrida de la v2 es idéntica, campo a campo (0 diferencias), a la registrada (`…-nc2-substitute/1/R20261120T220537Z-60ab` y `…/R20261122T000958Z-4719`).
- **PB-T3a:** idéntica a `v2-pbt3a.json` de la revisión de la v2. Las copias de `data/pbt3a/` son iguales byte a byte a los originales del worktree (`6d41`, `ad66` y `fc50`).
- **PB-T2a:** la corrida registrada `c2d3` es del v1 (`6ccaf12d`, 50 autopruebas, sin `--task`). Por eso comparé la v2 con la v3 de hoy. Las propiedades coinciden también con las del v1.

Ningún caso quedó sin terminar: la v2 terminó en los cuatro.

## 6. PB-T4d con la v3 (`review/runs/PBT4D-v3.json`)
Argumentos:
- `--task PBT4`;
- real `F4B-PBT4-RETIRE-PILOT/1/R20261122T194536Z-4c59`;
- nc2 `F4B-PBT4-RETIRE-PILOT-nc2/1/R20261122T195517Z-096b`;
- BaseSha `399c9bec…` y CurrentSha `7db6fd0c…`;
- nota y salida cruda de `R20261122T172759Z-039a`.

**`SubstituteResult = PASS`**, en 0,5 s.
- **Propiedades:** las 10 en true, de `1_AcceptedRealDelegationMatchesContract` a `10_NoAlternativeOneFieldMutation`.
- **`Record.nc2`:** `SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B`.
- **Identificación:** `Id` = `PBT4-NC2-EXACT-WHITELIST-F4B-01`, modo F4B-E21, autoprueba 63/63 y determinista. `TaskBinding` Ok, con `F4B-PBT4-RETIRE-PILOT`, intento 1.
- **La ruta y el Scope:** `ChangedPath` = `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs`. `ScopeOnReal` = pass y `ScopeOnMutated` = fail.
- **La salida cruda:** `RawCollectionsEmptyExact`, `RawPlusContractCollectionsEqualsDelegationJson` y `…Bytes` en true.
- **La exactitud:** `ExactEvidenceMatched` = PB-T3 y `ExactWhitelistBy` = `F4B_EXACT_EVIDENCES+BFC`.
- **Lo que informó el Controller en nc2:** EXECUTION_BLOCKED / STOP / Contract, con Scope en fail.
- **Los SHA:** la nota `CEA95371…` y la salida cruda `7F081AEF…` son los mismos que registró la corrida fallida de la v2 (`R20261122T201101Z-bb30`).

Mis comprobaciones propias, sin código de los sustitutos (`pbt4d_crosscheck.py` → `pbt4d-crosscheck.txt`), dan las 13 en true:
- sha(salida cruda) = `nota.OutputSha256` = relevo real;
- la salida cruda tiene las 8 colecciones vacías;
- la salida cruda más las 8 colecciones del contrato es igual a `delegation.json` byte a byte;
- el relevo de nc2 es el SHA de la delegación mutada;
- el bfc y la nota están atados a la delegación y al contrato;
- `RunId` es igual en la nota, el bfc, el relevo y la delegación;
- AWS real = AWS del contrato, en orden;
- entre la real y nc2 solo cambia `AllowedWriteScope`;
- el mutado es el real sin `sorted(diff)[0]`, en orden;
- el diff está dentro del alcance y fuera del prohibido;
- la ruta quitada no está en el alcance mutado;
- la forma cerrada de la 10.

Hay 37 rutas en el contrato y 37 en el diff, y forman el mismo conjunto.

**La firma** (`inspect-cases.txt`): entre la verificación real y nc2 solo difieren Contract y Scope (de pass a fail). Las condiciones de parada son S-04, P-03 y PBT4D-C01, la misma forma que en PB-T3a. La evidencia de Scope nombra la ruta quitada.

**Contraste (O-4):** el diagnóstico de la sesión da exactamente las mismas `Properties` y `Measured`. Solo difieren `ScriptSha256` y el texto del campo informativo (`compare-pbt4d-diag.txt`).

## 7. Mutaciones de PB-T4c: la v3 no da PASS donde la v2 da FAIL
`mutations_t4c.py` carga PB-T4c como lo hace `main()` y aplica cada mutación en memoria. Las mutaciones «coherentes» recalculan toda la cadena derivada:
- la delegación real, como salida cruda más las 8 colecciones del contrato;
- la copia de nc2, por README 10;
- el relevo de nc2, el bfc y la nota;
- los `InputSha256` de `mutation.json`.

**El control K0** (la reconstrucción sin cambios) reproduce byte a byte los 14 campos del paquete real y da PASS. Las salidas están en `review/mut/`.

**A tamaño real** (26 rutas; la v2 enumera hasta 2^26):

| Mutación | Rutas en el contrato | v2 (campo; tiempo) | v3 (tiempo) | Propiedades en false (iguales en las dos) |
|---|---|---|---|---|
| K0: control coherente sin cambios | 26 | (no corrida: es igual a REAL, §5) | PASS (0,1 s) | — |
| K1: la ruta quitada en el prohibido del contrato y de la real (coherente) | 26 | FAIL (100 663 296; 278,5 s) | FAIL (0,1 s) | 2, 7, **10** |
| K2: el prefijo de su carpeta en el prohibido (coherente) | 26 | FAIL (100 663 296; 279,1 s) | FAIL (0,1 s) | 2, 7, **10** |
| K3: contrato y real sin la ruta; README 10 no quita nada (p = None, coherente) | 25 | FAIL (0; 20,0 s) | FAIL (0,0 s) | 2, 3, 4, 5, 6, 7, 8, **10** |
| K4: contrato sin la ruta y real con ella (el contrato no cubre p) | 25 | FAIL (67 108 864; 144,1 s) | FAIL (0,0 s) | 1, 5, 8, **10** |
| K5: la copia de nc2 no quita nada (p = None, coherente) | 26 | FAIL (0; 39,9 s) | FAIL (0,0 s) | 2, 3, 4, 5, 6, 7, 8, **10** |

La v2 terminó en las cinco. En todas coinciden el resultado, el conjunto de propiedades en false y los `Measured` sin el campo informativo (`mut/full-run.txt`).

**Reducidas a 10 rutas del diff** (la ruta quitada incluida; la v2 termina en menos de un segundo; `mut/mini-run.txt`):

| Mutación | v2 | v3 | Propiedades en false (iguales en las dos) |
|---|---|---|---|
| mK0: control coherente | PASS | PASS | — |
| mK1: la ruta en el prohibido (coherente) | FAIL | FAIL | 2, 7, **10** |
| mK2: el prefijo de su carpeta en el prohibido (coherente) | FAIL | FAIL | 2, 7, **10** |
| mK3: contrato y real sin la ruta (p = None) | FAIL | FAIL | 2, 3, 4, 5, 6, 7, 8, **10** |
| mK4: contrato sin la ruta y real con ella | FAIL | FAIL | 1, 5, 8, **10** |
| mK5: la copia de nc2 no quita nada (p = None) | FAIL | FAIL | 2, 3, 4, 5, 6, 7, 8, **10** |
| mK6: alcance vacío | FAIL | FAIL | 2, 3, 4, 5, 6, 7, 8, **10** |
| mK7 (contraste): la ruta sustituida por el prefijo de su carpeta | PASS | PASS | — |
| mK8 (contraste): la ruta duplicada | PASS | PASS | — |
| mK9 (contraste): prohibido `ruta/`, que no la cubre | PASS | PASS | — |

En todas coinciden el resultado, el conjunto de propiedades en false y los `Measured` sin el campo informativo. Ninguna da PASS en la v3 con FAIL en la v2.

Como predice O-1, la 10 nunca falla sola: siempre va con la 1 o con la 2. **K1 y K2 (y sus versiones reducidas, mK1 y mK2) son el caso más adversarial:** la cadena es coherente, la 1 queda en true y la 10 cae junto con la 2 y la 7.

## Hallazgos

| Id | Sev. | Línea (v3) | Hallazgo |
|---|---|---|---|
| M-1 | M | 213 | **El campo informativo `AlternativeCandidatesExcludingPath` ya no es una medida, sino una frase fija, y es falsa cuando `p = None`.** Dice «al menos 1» siempre. Con `p = None`, la v2 medía 0 (mK3, mK5 y mK6: `0`). Además cambia de tipo, de entero a cadena. No interviene en ninguna propiedad ni en el resultado. Ningún guion de la cadena lo lee: en `hs65` solo aparece en los sustitutos, el generador, el diagnóstico y mis guiones (grep); en el worktree, fuera de `artifacts/`, solo en copias de evidencias (esa búsqueda quedó parcial por el límite de tiempo). Sugerencia, solo si algún día hay que tocar la v3 por otro motivo: escribir la frase solo con A ∧ B. No se corrige ahora: cambiaría el SHA. |
| M-2 | M | 1, 4, 9, 520 | **Textos desfasados.** El mensaje de uso de la l. 520 nombra `nc2_exact_whitelist_substitute_f4b_v2.py`. La segunda línea «Uso» (l. 9) lleva todavía el nombre del v1 (M-V3, heredado). La l. 4 dice «Lo demas es identico a la v2, byte a byte»: es cierto salvo en el docstring, el comentario y el campo informativo, que la misma l. 4 no menciona. |
| M-3 | M | 12 | **`import itertools` queda sin uso** tras quitar la enumeración. Es inocuo. |
| M-4 | M | (registro de la decisión) | **El dato de PB-T4c en la pregunta al Owner no es exacto.** Dice que PB-T4c tenía 21 rutas, pero su `AllowedWriteScope` tiene 26 en el contrato, en la delegación real y en el diff. La v2 midió 100 663 296 = 1,5·2^26 candidatas; mi corrida de la v2 tardó 291 s y llegó a unos 25 GB de 48. El 21 es el diff `BaseSha..RedSha` de src/tests que cita la evidencia de Scope del nc2 de PB-T4c. No cambia la decisión, y refuerza su motivo: con la v2, PB-T4c ya rozaba el límite de memoria. Conviene anotarlo en la custodia sin tocar el literal. |

## Observaciones (no cuentan)
- **O-1:** la 10 es consecuencia de la 1 y la 2, tanto en la v2 como en la v3 (§2.4). Ninguna entrada falla solo por la 10, y la autoprueba no la aísla. Es heredado.
- **O-2:** con un elemento que no es cadena en el alcance del contrato, la v2 lanza una excepción y la v3 puede dar la 10 en true. Es inalcanzable para un PASS (§2.3); las cuatro sondas dan FAIL en las dos.
- **O-3:** el `Id` y el `GenericId` no cambian entre la v2 y la v3, por la decisión del Owner. Las salidas se distinguen por `ScriptSha256` y por el texto del campo informativo.
- **O-4:** el diagnóstico de la sesión coincide con la v3 sobre PB-T4d en `Properties` y `Measured`. No es evidencia.

## Lo no verificado
- **La v2 sobre PB-T4d:** no la corrí. El encargo no lo pedía y son 2^37 subconjuntos. La corrida registrada `bb30` dio `MemoryError`. Para las entradas en las que la v2 no termina, la equivalencia es por demostración (§2.3), no empírica.
- **La memoria de pico de la v2 en PB-T4c:** sale de instantáneas del proceso (unos 24,8 GB), no de una medida exacta.
- **Las propiedades 1 a 9:** no las revisé de nuevo. Son idénticas nodo a nodo a la v2, que ya estaba revisada.
- **La custodia de los artefactos:** no la contrasté con los registros de custodia. Solo comprobé la coherencia interna de PB-T4d (§6) y que las copias de PB-T3a son iguales a los originales.
- **El v1 sobre PB-T2a:** no lo volví a correr. Su corrida registrada es del v1.
- **La corrida oficial de la v3 sobre PB-T4d que haga la sesión, y su custodia:** no las vi. Mi salida está solo en `review/runs/`.
- **El determinismo en otro entorno:** solo lo probé con Python 3.13.14 en esta máquina.
- **El alcance de PB-T5:** queda fuera de este encargo.
- **El guion de aplicación de la v3** (`pbt4dcc/apply_nc2_f4b_v3_pbt4d.sh` y su generador `_make_apply_v3.py`): no lo revisé.

## Ejecuciones y archivos (todo en `review/`)
- `diff_ast_check.py` → `diff-ast-check.txt`; `diff-v2-v3.txt`; `regen/` (el generador reproducido).
- `p10_equiv.py` → `p10-equiv-result.json` y `p10-equiv-run.txt`.
- `evaluate_equiv.py` → `evaluate-equiv-result.json` y `evaluate-equiv-run.txt`.
- `cases.json`, `run_case.py` → `runs/{PBT2A,PBT3A,PBT4A,PBT4C,PBT4D}-v3.{json,txt}` y `runs/{PBT2A,PBT3A,PBT4A,PBT4C}-v2.{json,txt}`.
- `compare_runs.py` → `compare-light.txt`, `compare-pbt4c.txt` y `compare-pbt4d-diag.txt`; `selftest_across_runs.py` → `selftest-across-runs.txt`.
- `inspect_cases.py` → `inspect-cases.txt`; `pbt4d_crosscheck.py` → `pbt4d-crosscheck.txt`; `show_args.py`.
- `mutations_t4c.py` → `mut/*.json`, `mut/mini-run.txt` y `mut/full-run.txt`.

**Avisos sobre mis ejecuciones:**
- Corrí todo con `python -I -B <archivo>`. Los módulos se cargaron con `exec` del texto, sin `import` y sin cachés de bytecode.
- Los sustitutos corrieron con el cwd en el worktree. Mis guiones de `git diff` usaron `cwd=` sin cambiar de directorio.
- En una línea de Bash se coló un `python -I -B -c ""`. Es vacío, no hace nada y no leyó ni escribió nada. Lo declaro porque las reglas prohíben `-c`.
- No toqué la v2, la v3, `_sim/`, Codex, el Controller, CI ni dotnet.
- Al terminar, `git status` del worktree (HEAD `7db6fd0c`) y del checkout principal está limpio, y los SHA de la v2 y de la v3 no cambiaron. No apareció ningún `__pycache__` en `nc2-substitute-f4b-v2/` ni en `nc2-substitute-f4b-v3/`.
- Durante la revisión apareció `nc2-substitute-f4b-v3/_make_apply_v3.py`, de la sesión: genera el guion de aplicación de la v3 a PB-T4d. Lo leí, pero no lo ejecuté ni lo revisé, porque queda fuera de este encargo.
