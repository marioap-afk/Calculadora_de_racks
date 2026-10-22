I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA RONDA 2 DE LA ENMIENDA A-7 (admisión de los insumos del cierre de VERIFY de FX-02 en F6), acotada a las correcciones A62-A7-R1..R3 y a sus consecuencias (decisiones §94, punto 6), con la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento.

Eres el ARCHITECT de esta revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-7 y de sus preflights, de la revisión de la ronda 1, de los agentes que construyeron el banco de la medición, el kit `a6m7-measure` y las determinaciones de §91 y §92, de quienes escribieron el paquete B-min, del Coordinator y de las revisiones de A-1..A-6. Revisas en solo lectura la versión exacta de la ronda 2 de A-7 y devuelves un único veredicto formal (LIFECYCLE §5 y §6). No redactas texto normativo de sustitución, no implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261019T201030Z-d1bf
InvocationId           = I20261019T201030Z-d1bf
LogicalReviewRequestId = L20261019T201030Z-d1bf   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-a7r2   (tu directorio de trabajo; HEAD separado en el commit, sin ramas locales; sin remoto)
Commit                 = b432c40f8ca7b1e915dff531cb9a8cad6466bb0c   (registro de la ronda 2: resultado `run` de sus guardas y disposiciones propuestas de O1-O8)
Publicación ronda 2    = e90b444d5d23288e0b5a59881820377bee0d7d09   (A-7 ronda 2, PROPUESTA; padre b9334f22204c95872e7defe41bbdaaf849097585, con la ronda 1 y decisiones §94)
Objeto                 = docs/initiatives/I-62-A-7.md   blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec15   (37 205 bytes, 346 líneas)
Ronda 1                = D:\r62-arch-a7r2-run\a7-r1.md   (blob 6c29747d61c5f689a195095ad3712e6d9dba62aa, commit 6fee8fd6; 27 472 bytes, 279 líneas)
Revisión de la ronda 1 = R20261019T010200Z-4a72 (auditor NOT_ACCREDITED; Architect CHANGES REQUIRED con A62-A7-R1..R3 y A62-A7-O1..O8)
A-6 acordada           = docs/initiatives/I-62-A-6.md   blob 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 (decisiones §90)
Paquete del Architect  = ninguno (este texto hace de paquete)
Orden vigente          = D:\r62-arch-a7r2-run\order.txt   (cuerpo de la disposición §94 del Coordinator; 3 522 bytes, 68 líneas, SHA-256 515ccfa37205aa1d9d1fccbd3eb3c2ed0c2b56b8bb67c56102d1a8b0bea3db0e)
```

**Recibo.** El invocador comprobó que el objeto tiene el blob `8a528133…` en la publicación, en el registro y en el commit de arriba, y que el
padre de la publicación tiene la ronda 1 (`6c29747d…`, el mismo blob que `a7-r1.md`); que la A-6 acordada (`4bc919dd…`) no cambia del padre de la
publicación al commit; que `docs/automation/evidence/I-62-A7/r2/` no cambia después de la publicación salvo por el añadido del resultado `run` y que
su `MANIFEST.sha256` coincide con los archivos; que el preflight de la ronda 1 (`docs/automation/evidence/I-62-A7/` fuera de `r2/`) no cambia desde
el padre de la publicación; que decisiones solo crece por añadido desde el registro de §93 y que §94 es su última sección en el commit del clon; que
la revisión de la ronda 1 custodiada es la de R1-R3 y O1-O8; que el paquete B-min, `agent-execution/`, `m7/s91/`, `m7/s92/`, `m7/bench/`,
`m7/readings/`, el kit `a6m7-measure`, `r2/` y la revisión de la ronda 1 conservan los árboles de los commits que los custodiaron; y que las guardas
custodiadas de la ronda 2 reproducen sus resultados (`run` sobre la publicación y la autoprueba).

**Condición de lanzamiento (compuerta 11).** Decisiones §94, punto 6 (línea 1858; `order.txt`, líneas 43-46), ordena preparar y custodiar un
kit de revisión limitado a R1-R3 y sus consecuencias y prohíbe la revisión formal «sin una disposición nueva del Coordinator». Esta invocación solo
existe si una disposición del Coordinator POSTERIOR a §94, que nombra esta revisión, se fijó en el kit (su número, su encabezado y el SHA-256 de su
texto) y el lanzador comprobó inmediatamente antes de lanzarla, en el registro de decisiones de la rama de I-62: esa sección, una sola vez, con el
encabezado y el SHA-256 fijados y el blob completo del objeto; ninguna sección posterior a ella; la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de
§78 como línea completa; la línea `CLAUDE-CLI-I62 = A (…)` vigente y el binario medido igual al del cierre; y A-7 y A-6 en la HEAD de la rama con
los blobs de arriba. Esa disposición es posterior al commit del clon y no está en el cierre: tu orden vigente es §94 (`order.txt`). No juzgas esa
autorización ni las determinaciones del Owner; juzgas la ronda 2 de A-7.

**Objeto y revisión.** La ronda 1 de A-7 (`a7-r1.md`) añadió una regla, A7-1, al final de V14 D.3, después de A6-8. Su revisión formal
(R20261019T010200Z-4a72) dio CHANGES REQUIRED con tres REQUIRED, que decisiones §94 aceptó (punto 1, línea 1853), y la ronda 2 los corrige en
el mismo archivo (punto 2, línea 1854: «Cambios normativos limitados a» R1, R2 y R3; sin modificar A-6, 16.24, RAE ni los blobs fijados del
instrumento y del aceptante). A-7 §3.6 (L230-243) lista lo que cambió y por qué: A7-1, puntos 1, 2, 5, 6 y 7 (nuevo; el antiguo 7 es el 8) por R1-R3,
y cambios descriptivos (O1, O7, O2) en otras secciones. La revisión de la ronda 2 es ACOTADA (§94, punto 6) a:

- la resolución de cada hallazgo de la ronda 1: A62-A7-R1 (la categoría (c) sin los TRX que lee 16.9 ni los hechos del relevo y `RelayFacts`), R2
  (el invalidador de A6-7 identificado por el SHA-256 de una instancia) y R3 (A7-1 (c) frente a A6-2, regla 1, «la única excepción», sin declararlo);
- la pureza del delta: los cambios normativos solo por R1-R3; los descriptivos (O1, O7, O2) y las cuatro precisiones que el preflight de la ronda 2
  incorporó después (la exigencia de RED según 16.8, la medición sin hechos del relevo, la implementación de la derivación del cierre del aceptante
  como pieza del invalidador y la exigencia de RED indeterminada) dentro de R1 o R2;
- las consecuencias de R1-R3 sobre los temas que la ronda 1 encontró CONFORMS, sin regresión: EFF y EFF^1 en la validación de mapas de B1;
  MAP_INVALID frente a entrada inexistente y cierre no construible; la regla de cobertura de `clausemap`; las entradas de varias categorías; la
  ausencia de cambios no autorizados;
- la reconfirmación de la materialidad (M-01..M-08) y de que no hay ninguna consecuencia OWNER-RESERVED no identificada;
- las preguntas de la ronda 2 (A-7 §8, Q-A7R2-01..03);
- como campo opcional, la conformidad de las disposiciones propuestas de A62-A7-O1..O8 (`r2-dispositions/optional-dispositions.md`; §94, punto 4,
  línea 1856).

Los impactos relacionados (la exclusión de `contract`, D-1, y las propuestas de fallo cerrado FC) se evaluaron en la ronda 1 y no se revisan otra
vez (§94, punto 7, línea 1859): solo los mencionas si R1-R3 los cambian. Lo que dependa de una sesión real o de una disposición futura (la
medición de A6-7, las disposiciones previas a la medición, la nueva preparación del kit de la medición, B1) sigue como precondición con fallo cerrado.
Sin acreditación excepcional: si no puedes cumplir un punto con lecturas fieles, no lo des por cumplido. No se te prohíbe señalar otro defecto
material de A-7 que encuentres: hazlo como hallazgo.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git`, las guardas, el constructor ni el validador del cierre de A7-1, el instrumento de B-min, su aceptante ni el kit de la medición.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, sin ramas ni remoto, árbol limpio, los blobs del cierre y el
  SHA-256 de los archivos del run. El auditor la vuelve a verificar después. En `Step0` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. **Lectura directa de AUTOMATION_PLAN 16.8 (obligatoria).** Lee con Read `D:\r62-arch-a7r2-run\fiel__docs__AUTOMATION_PLAN.md.txt` con `offset` 608 y `limit`
   18: incluye las líneas 616-617 (RED de la cadena: cuándo una entrega «**exige RED**»), que A7-1, punto 1 (c), y la fila L16 de A-7
   §2 citan. Tiene que ser una llamada a Read sobre ese archivo (si es una copia fiel, tiene la misma numeración de líneas): la cita de A-7 no es una
   lectura de AUTOMATION_PLAN. Regístrala en `DirectReads` (`docs/AUTOMATION_PLAN.md:616-617`).
2. Lee entera, con Read, `D:\r62-arch-a7r2-run\order.txt` (la orden vigente, 68 líneas): punto 1 (líneas 3-9), punto 2 (10-16), punto 3 (17-28),
   punto 4 (29-36), punto 5 (37-42), punto 6 (43-46), punto 7 (47-50), punto 8 (51-57) y punto 9 (58-68).
3. Lee con Read las líneas 1846-1861 de `D:\r62-arch-a7r2-run\fiel__docs__automation__decisions__I-62.md.txt` (decisiones §94). Comprueba que la línea 1849 declara el cuerpo pegado de §94 con
   3 522 bytes y el SHA-256 de arriba; que los puntos 1-9 (líneas 1853-1861) registran lo que dice `order.txt` (con la redacción del registro); y
   que el punto 1 acepta A62-A7-R1, R2 y R3 como REQUIRED y el punto 2 autoriza la segunda candidata con cambios limitados a R1-R3.
4. Lee entero, con Read, `D:\r62-arch-a7r2\docs\automation\evidence\I-62-A7\r2\a7-guards-result-run.json` (`docs\automation\evidence\I-62-A7\r2\a7-guards-result-run.json`, 38 líneas): A7Blob L2; Base L4; Head L35; Mode L36; Result L37 deben dar `A7Blob` = el blob del objeto, `Base` = el padre de la
   publicación (abreviado, `b9334f22`), `Head` = la publicación (abreviada, `e90b444d`), `Mode` run y `Result` PASS.
5. Lee con Read, en `D:\r62-arch-a7r2\docs\automation\evidence\I-62-A7\r2\summary.json` (`docs\automation\evidence\I-62-A7\r2\summary.json`), las líneas 3-6 (`Candidate.Blob` = el blob del objeto), 55-57 (`Hold` =
   `Cases` y `NotHolding` vacío) y 72-84 (autoprueba de las guardas de la ronda 2).
6. Lee con Read, en `D:\r62-arch-a7r2\docs\automation\evidence\I-62-architect-A-7\R20261019T010200Z-4a72\output.json` (`docs\automation\evidence\I-62-architect-A-7\R20261019T010200Z-4a72\output.json`), las líneas RunId L3; ReviewedBlob L12; RequiredFindings L177-244: `RunId` = R20261019T010200Z-4a72, `ReviewedBlob` = el blob de la ronda 1
   y `RequiredFindings` con A62-A7-R1, R2 y R3.

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`D:\r62-arch-a7r2-run\`):
- `order.txt`: la orden vigente (disposición §94; 3 522 bytes, 68 líneas). Su SHA-256 es el que declara decisiones §94 en el commit. Puede ser
  referencia (`order.txt:<línea>`);
- `a7-r1.md`: el texto de la ronda 1 de A-7 (blob `6c29747d…`), byte a byte, para comparar las correcciones con el texto que la ronda 1 revisó. Puede
  ser referencia (`a7-r1.md:<línea>`);
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es referencia;
- 43 **copias fieles** `fiel__*.txt` (abajo, «Copias fieles»): cada una se lee EN LUGAR de su original; nunca son referencia.

**Copias fieles (lectura byte a byte).** Read no entrega fielmente los bytes de algunos archivos del cierre: elimina en silencio un BOM inicial (una
eliminación silenciosa nunca es una entrega fiel), trunca las líneas largas y no garantiza los bytes de controles, CR sueltos, NUL, UTF-8 inválido,
U+FEFF interior, separadores de línea Unicode ni de los caracteres no ASCII cuya entrega no se ha medido. Cada archivo así tiene en el run una copia
fiel (formato RACKCAD-COPIA-FIEL/1) con los bytes exactos del original y **la misma numeración de líneas**: la línea n de la copia es la línea n del
original. En la copia, `\\` es una barra invertida y `\xHH` es un byte escrito explícitamente (el BOM es `\xEF\xBB\xBF`, un TAB `\x09`, un CR `\x0D`,
cada byte de un carácter no medido o de un UTF-8 inválido el suyo, y un espacio al final de una línea `\x20`); fuera de eso, el texto es literal. Una
línea del original que no cabe termina en `\>` y sigue en el apéndice, después de la última línea del original, en las líneas `@n.2|…`, `@n.3|…`, que
se concatenan en orden y sin separador. El pie de la copia declara la ruta, el blob, el SHA-256 y las marcas del original. **Lee siempre la copia y
nunca el original** de estos archivos:
- `docs/automation/evidence/I-62-A7/r2/a7-guards.py` (UNMEASURED_CHAR: línea 16) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__r2__a7-guards.py.txt` (807 líneas)
- `docs/automation/evidence/I-62-A7/r2/b1-like/main/fixture-tests.trx` (BOM: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__r2__b1-like__main__fixture-tests.trx.txt` (50 líneas)
- `docs/automation/evidence/I-62-A7/r2/b1-like/uc/fixture-tests.trx` (BOM: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__r2__b1-like__uc__fixture-tests.trx.txt` (50 líneas)
- `docs/automation/evidence/I-62-A7/r2/check_a7_closure.py` (ZWNBSP: línea 135) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__r2__check_a7_closure.py.txt` (514 líneas)
- `docs/automation/evidence/I-62-A7/r2/derive_r4.py` (ZWNBSP: línea 49) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__r2__derive_r4.py.txt` (380 líneas)
- `docs/automation/evidence/I-62-A7/r2/inputs/fixture-tests-red.trx` (BOM: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__r2__inputs__fixture-tests-red.trx.txt` (62 líneas)
- `docs/automation/evidence/I-62-A7/a7-guards.py` (UNMEASURED_CHAR: línea 16) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__a7-guards.py.txt` (668 líneas)
- `docs/automation/evidence/I-62-A7/b1-like/main/fixture-tests.trx` (BOM: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__b1-like__main__fixture-tests.trx.txt` (50 líneas)
- `docs/automation/evidence/I-62-A7/b1-like/uc/fixture-tests.trx` (BOM: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__b1-like__uc__fixture-tests.trx.txt` (50 líneas)
- `docs/automation/evidence/I-62-A7/check_a7_closure.py` (ZWNBSP: línea 111) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__check_a7_closure.py.txt` (452 líneas)
- `docs/automation/evidence/I-62-A7/derive_r4.py` (ZWNBSP: línea 49) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A7__derive_r4.py.txt` (380 líneas)
- `docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/README.md` (UNMEASURED_CHAR: línea 233) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-architect-A-7__R20261019T010200Z-4a72__README.md.txt` (466 líneas)
- `docs/automation/evidence/I-62-A6/m7/s91/d1/d1.md` (UNMEASURED_CHAR: línea 159) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__m7__s91__d1__d1.md.txt` (311 líneas)
- `docs/automation/evidence/I-62-A6/m7/s91/risk-matrix.md` (UNMEASURED_CHAR: líneas 15, 28) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__m7__s91__risk-matrix.md.txt` (55 líneas)
- `docs/automation/evidence/I-62-A6/m7/s92/contract/out/s92-nc2-r/acceptor-stdout.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__m7__s92__contract__out__s92-nc2-r__acceptor-stdout.json.txt` (20 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-5 de la copia)
- `docs/automation/evidence/I-62-A6/m7/s92/contract/out/s92-nc2-r/envelope-read-json-CONTRACT.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__m7__s92__contract__out__s92-nc2-r__envelope-read-json-CONTRACT.json.txt` (19 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-4 de la copia)
- `docs/automation/evidence/I-62-A6/m7/s92/contract/out/s92-nc2-r/envelope-read-json-DELEG.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__m7__s92__contract__out__s92-nc2-r__envelope-read-json-DELEG.json.txt` (19 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-4 de la copia)
- `docs/automation/evidence/I-62-A6/m7/bench/p1-build.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__m7__bench__p1-build.json.txt` (44 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-29 de la copia)
- `docs/automation/evidence/I-62-A6/m7/bench/p3-n4.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__m7__bench__p3-n4.json.txt` (25 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-10 de la copia)
- `docs/automation/evidence/I-62-A6/m7/bench/p4-nc1.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__m7__bench__p4-nc1.json.txt` (20 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-5 de la copia)
- `docs/automation/evidence/I-62-A6/m7/readings/readings.md` (UNMEASURED_CHAR: líneas 346, 348) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__m7__readings__readings.md.txt` (431 líneas)
- `docs/automation/evidence/I-62-A6/r4/r1probes/p7-derive.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__r4__r1probes__p7-derive.json.txt` (59 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-44 de la copia)
- `docs/automation/evidence/I-62-A6/r4/r1probes/p9-n4.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__r4__r1probes__p9-n4.json.txt` (31 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-16 de la copia)
- `docs/automation/evidence/I-62-A6/r4/r1probes/p10-nc1.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-A6__r4__r1probes__p10-nc1.json.txt` (37 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-22 de la copia)
- `docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/README.md` (UNMEASURED_CHAR: líneas 169, 189, 209) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__kits__FX-02__a6m7-measure__README.md.txt` (238 líneas)
- `docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/derive_r4.py` (ZWNBSP: línea 49) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__kits__FX-02__a6m7-measure__derive_r4.py.txt` (380 líneas)
- `docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/m7-run.snapshot.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__kits__FX-02__a6m7-measure__frozen__m7-run.snapshot.json.txt` (41 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-26 de la copia)
- `docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/fixture-tests.trx` (BOM: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__kits__FX-02__a6m7-measure__inputs__fixture-tests.trx.txt` (50 líneas)
- `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/tests/test_bmin_accept.py` (UNMEASURED_CHAR: líneas 306, 319) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__accept__tests__test_bmin_accept.py.txt` (1 461 líneas)
- `docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-clausemap.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-clausemap.json.txt` (40 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-25 de la copia)
- `docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-contract.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-contract.json.txt` (18 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-3 de la copia)
- `docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-diff.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-diff.json.txt` (18 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-3 de la copia)
- `docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-hashes.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-hashes.json.txt` (18 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-3 de la copia)
- `docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-identity.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-identity.json.txt` (18 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-3 de la copia)
- `docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-scope.json` (LONG_LINE: línea 1) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-scope.json.txt` (18 líneas; las continuaciones de la línea 1 están en el apéndice, líneas 3-3 de la copia)
- `docs/initiatives/I-62-proposal-v14.md` (LONG_LINE: líneas 2352, 2380, 2382; UNMEASURED_CHAR: líneas 1579, 1594, 1595, 1780, 1864, 2018, 2183, 2195, 3078, 3084, 3131, 3146 y 1 más) → `D:\r62-arch-a7r2-run\fiel__docs__initiatives__I-62-proposal-v14.md.txt` (3 342 líneas; las continuaciones de las líneas 2352, 2380, 2382 están en el apéndice, líneas 3325-3327 de la copia)
- `docs/AUTOMATION_PLAN.md` (UNMEASURED_CHAR: línea 605) → `D:\r62-arch-a7r2-run\fiel__docs__AUTOMATION_PLAN.md.txt` (1 748 líneas)
- `docs/initiatives/I-62-A-4.md` (UNMEASURED_CHAR: líneas 112, 375) → `D:\r62-arch-a7r2-run\fiel__docs__initiatives__I-62-A-4.md.txt` (532 líneas)
- `docs/initiatives/I-62-A-3.md` (LONG_LINE: línea 1487; UNMEASURED_CHAR: líneas 264, 741, 864, 865, 866, 870, 871, 872, 876, 877, 880, 881 y 14 más) → `D:\r62-arch-a7r2-run\fiel__docs__initiatives__I-62-A-3.md.txt` (1 553 líneas; las continuaciones de la línea 1487 están en el apéndice, líneas 1538-1538 de la copia)
- `docs/initiatives/I-62-A-1.md` (UNMEASURED_CHAR: líneas 100, 147) → `D:\r62-arch-a7r2-run\fiel__docs__initiatives__I-62-A-1.md.txt` (453 líneas)
- `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` (UNMEASURED_CHAR: línea 53) → `D:\r62-arch-a7r2-run\fiel__docs__adr__0046-protocolo-de-ejecucion-delegada-de-agentes.md.txt` (121 líneas)
- `docs/automation/decisions/I-62.md` (LONG_LINE: líneas 657, 734, 750, 768, 787, 805, 819; UNMEASURED_CHAR: líneas 73, 929) → `D:\r62-arch-a7r2-run\fiel__docs__automation__decisions__I-62.md.txt` (1 886 líneas; las continuaciones de las líneas 657, 734, 750, 768, 787, 805, 819 están en el apéndice, líneas 1863-1871 de la copia)
- `docs/automation/evidence/I-62-evidence.md` (UNMEASURED_CHAR: líneas 112, 780, 1142, 1366, 1370, 1416, 1955, 2046, 2377, 2922, 3130, 3138 y 5 más) → `D:\r62-arch-a7r2-run\fiel__docs__automation__evidence__I-62-evidence.md.txt` (4 796 líneas)

Todas las rutas que siguen son relativas al clon `D:\r62-arch-a7r2` y están en el commit de arriba.

**Objeto**, entero: `docs/initiatives/I-62-A-7.md` (léelo con Read en `D:\r62-arch-a7r2\docs\initiatives\I-62-A-7.md`): cabecera 1-60; 1. Alcance 62-78; 2. Fuentes 80-100; 3. Delta 102-243; 3.1 Motivo 104-119; 3.2 A7-1 121-184; 3.3 Categorías de entrada 186-193; 3.4 Relación con A-6 y con R7-A 195-211; 3.5 Lo que A-7 no hace 213-228; 3.6 Correcciones de la ronda 2 230-243; 4. Semántica antes y después 245-254; 5. Materialidad 256-267; 6. No cambia producción; compatibilidad con A-1..A-6; guardas y preflight 269-279; 7. Consecuencias reservadas al Owner 281-286; 8. Preguntas para la revisión 288-308; 9. Separabilidad 310-313; 10. Evidencia 315-323; 11. Estado de revisión 325-335; Anexo 337-346. Texto de A7-1: L125-184; puntos 1 L129-142; 2 L143-152; 3 L153-156; 4 L157-159; 5 L160-170; 6 L171-174; 7 L175-181; 8 L182-184;
categorías del punto 1: (a) L131-133; (b) L134-135; (c) L136-139; (d) L140-142. Tabla de categorías de §3.3: L188-193. Relación con A-6 y con R7-A: §3.4 (L195-211). §3.5 (L213-228; residuo
L220-228). Correcciones de la ronda 2, §3.6 (L230-243): R1 L238; R2 L239; R3 L240; O1 L241; O7 L242; O2, O3-O6, O8 L243. Materialidad: §5 (L256-267). Consecuencias reservadas al Owner: §7 (L281-286).
Preguntas: §8 (L288-308; las de la ronda 2, filas L306-308).

**Preflight dirigido de la ronda 2** (`docs/automation/evidence/I-62-A7/r2/`, 50 archivos, enteros; evidencia de apoyo, no autoridad: el
constructor `make_eic_a7.py`, el validador `check_a7_closure.py`, el formato `a7-designation-2.schema.json`, la derivación del lado del aceptante
`designate_a7.py` y `derive_r4.py`, las designaciones y las entradas sintéticas, el caso tipo B1 `b1-like/`, las fases q0-q7, las guardas con su
autoprueba y el resultado `run`; `preflight.md` es su informe):

```text
docs/automation/evidence/I-62-A7/r2/MANIFEST.sha256                           blob c5f46c8a58df08fecf86af960abf021e8b08542f  4 191 bytes  48 líneas
docs/automation/evidence/I-62-A7/r2/a7-designation-2.schema.json              blob 07257f76730973bb8d3bddec328e6e560ce849d0  2 852 bytes  61 líneas
docs/automation/evidence/I-62-A7/r2/a7-guards-result-run.json                 blob 42fc3d5f896ec4f0c34416393e6b409bd77abc8d  543 bytes  38 líneas
docs/automation/evidence/I-62-A7/r2/a7-guards.py                              blob 8a7f00f891c933f16637d1e1b473f4046fd441e1  55 453 bytes  791 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__r2__a7-guards.py.txt
docs/automation/evidence/I-62-A7/r2/b1-like/main/FX-U2-T1.gate-contract.json  blob f1f3b22ab696a22e25f1cc74d4f83cb371c22419  4 836 bytes  170 líneas
docs/automation/evidence/I-62-A7/r2/b1-like/main/deleg.json                   blob 6624d3ae4bc6a63ea1b771f4ccd4138ce7aa01e6  6 175 bytes  223 líneas
docs/automation/evidence/I-62-A7/r2/b1-like/main/designation.json             blob 67ba4cfd8cb123a8478a225c68acc4565bafd70e  2 174 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/b1-like/main/fixture-tests.trx            blob 40295387f9b489d2bf7ff4e517adc2d99c0d35ff  2 772 bytes  34 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__r2__b1-like__main__fixture-tests.trx.txt
docs/automation/evidence/I-62-A7/r2/b1-like/main/handoff.json                 blob e8bb7ccbe04e7a387443c25dda6ad154c0c3ef9a  1 581 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/b1-like/main/relay-facts.json             blob fdbbb3492b9b06bdf615253f3322ee95125a2b6a  3 477 bytes  129 líneas
docs/automation/evidence/I-62-A7/r2/b1-like/uc/FX-U2-T1.gate-contract.json    blob 1a2a3310714fad96e49b34dd5e671e50ed229d4b  4 836 bytes  170 líneas
docs/automation/evidence/I-62-A7/r2/b1-like/uc/deleg.json                     blob 7eab1a42def103c6ac62d57c3f3f69456ad814ed  6 175 bytes  223 líneas
docs/automation/evidence/I-62-A7/r2/b1-like/uc/designation.json               blob 40671c8a53a9eece980110eac1349fc451d877ab  2 165 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/b1-like/uc/fixture-tests.trx              blob 40295387f9b489d2bf7ff4e517adc2d99c0d35ff  2 772 bytes  34 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__r2__b1-like__uc__fixture-tests.trx.txt
docs/automation/evidence/I-62-A7/r2/b1-like/uc/handoff.json                   blob 003246fceca3df2d9d4a3b4ade8e0cd39f81e516  1 581 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/b1-like/uc/relay-facts.json               blob 8c4879aead331e2e9b0fbada7488fd12e22596f6  3 477 bytes  129 líneas
docs/automation/evidence/I-62-A7/r2/check_a7_closure.py                       blob aedfb593f0057c9ca0c19fbc688d5e75fc0cbb11  28 324 bytes  498 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__r2__check_a7_closure.py.txt
docs/automation/evidence/I-62-A7/r2/check_citation_blobs.py                   blob ff15df6c9b565acb7b527dba650d25bbeccfe99f  8 299 bytes  169 líneas
docs/automation/evidence/I-62-A7/r2/derive_r4.py                              blob a6f87be801f9dbf33144e20ef5b6aaee6b11eab8  13 783 bytes  364 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__r2__derive_r4.py.txt
docs/automation/evidence/I-62-A7/r2/designate_a7.py                           blob ca925afa561d7d5c44b44c92014502d79fedbe22  8 948 bytes  169 líneas
docs/automation/evidence/I-62-A7/r2/designate_m7.py                           blob bbf548608ef43fbb6af05f4574fbc9c6785b365b  13 239 bytes  258 líneas
docs/automation/evidence/I-62-A7/r2/designations/M.json                       blob 9c69e6e5e2da517929f0a55cf012ffac63d90f63  1 863 bytes  57 líneas
docs/automation/evidence/I-62-A7/r2/designations/N4.json                      blob 3e1086b6dfd84234ac35abbd08d635bdf6421e26  2 091 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/designations/N5.json                      blob 93716c9bd07733f0329fd930ab0a76498a3bc390  2 091 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/designations/N6.json                      blob 21f81f33d8f8893f2178aa6b255731a8a8e044d5  2 091 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/designations/N7A.json                     blob 6c20130a09414d0fbe39329b22dc1c3c2858f6f9  2 098 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/designations/N7B.json                     blob fcaaf07a0cc6a5e0d2e7b54d48d7c36bea07d5a6  2 098 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/designations/NC1.json                     blob b89fdce96a63ab70f3faa23d8b6dfb9d6ac7758f  2 102 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/designations/NC2.json                     blob c397ebe0f5f3bb33bfd77cb7eed2e14b8950b7da  2 097 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/designations/V.json                       blob dea94a39bd512c9ccb51b120437083ebb2cc4bfa  2 079 bytes  63 líneas
docs/automation/evidence/I-62-A7/r2/inputs/fixture-tests-red.trx              blob 5fd8860bdd6fe21268faf1b10d0df4d9b4743f92  4 264 bytes  46 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__r2__inputs__fixture-tests-red.trx.txt
docs/automation/evidence/I-62-A7/r2/inputs/relay-facts-NC2.json               blob 624ce009751f0044bc810f6a2b6deead0b74f140  4 143 bytes  158 líneas
docs/automation/evidence/I-62-A7/r2/inputs/relay-facts-V.json                 blob 5ad9a3e772fb624748953b30eb49129bf001948a  4 143 bytes  158 líneas
docs/automation/evidence/I-62-A7/r2/lib_r1r4.py                               blob a623a33f9b808c0e080d2f84fb545089d32864ac  37 661 bytes  731 líneas
docs/automation/evidence/I-62-A7/r2/make_eic_a7.py                            blob f652b3268d80696461919ec39fe5b710036d178e  29 894 bytes  489 líneas
docs/automation/evidence/I-62-A7/r2/make_eic_verify.py                        blob 965f42e8460eeb7a0b2770978be6292f05170663  18 620 bytes  363 líneas
docs/automation/evidence/I-62-A7/r2/preflight.md                              blob cd695b10103e6b7f76649789dbcfede1d12697db  53 365 bytes  293 líneas
docs/automation/evidence/I-62-A7/r2/preflight_text_r2.py                      blob a773a8733574182eafd768b0a74d70ecc41179bc  17 552 bytes  209 líneas
docs/automation/evidence/I-62-A7/r2/q0-env.json                               blob 2503b47e3db37ad94b33152265d9944e3721d364  8 134 bytes  200 líneas
docs/automation/evidence/I-62-A7/r2/q1-trx.json                               blob 86762d13ef44048ab4cc58d26d0ac40d3304729a  16 352 bytes  340 líneas
docs/automation/evidence/I-62-A7/r2/q2-relay.json                             blob 4b60aad581783c4271ad13c4f44a6c0c5b0aaec6  4 598 bytes  98 líneas
docs/automation/evidence/I-62-A7/r2/q3-n4-nc1.json                            blob 370714489c87f5b69ef589175512294c2258b2bd  7 788 bytes  219 líneas
docs/automation/evidence/I-62-A7/r2/q4-b1.json                                blob 6199245677b691fbbbb06f5915b760b8e086ba8e  5 492 bytes  136 líneas
docs/automation/evidence/I-62-A7/r2/q5-invalidator.json                       blob e21a76e79af22522f13ac64dcd4eb67ae49a9723  16 798 bytes  309 líneas
docs/automation/evidence/I-62-A7/r2/q6-compat.json                            blob 91d01dbb206f86af6e9f38bbda9aa93a92cc5738  7 271 bytes  194 líneas
docs/automation/evidence/I-62-A7/r2/q7-guards-preview.json                    blob b9c261f8c9cd79cdb5249f07221e211efadb63ae  618 bytes  38 líneas
docs/automation/evidence/I-62-A7/r2/q7-guards-selftest.json                   blob a01d30854ca5447bd35c3f1b4adcc98d1956227c  12 555 bytes  495 líneas
docs/automation/evidence/I-62-A7/r2/q7-guards.json                            blob efe9192b28e72271dfc25cb8a6a8ea8434610932  11 686 bytes  385 líneas
docs/automation/evidence/I-62-A7/r2/run_a7r2.py                               blob 8becb7d9a061d4281201c754fb464e7a492df56c  90 683 bytes  1 079 líneas
docs/automation/evidence/I-62-A7/r2/summary.json                              blob a86f50016668bc253fe61d9ba024ff278d066824  2 904 bytes  94 líneas
```

**Disposiciones propuestas de A62-A7-O1..O8** (`docs/automation/evidence/I-62-A7/r2-dispositions/optional-dispositions.md`; propuestas del Principal para el Coordinator, que no deciden; §94, punto 4): O1 L9; O2 L10; O3 L11; O4 L12; O5 L13; O6 L14; O7 L15; O8 L16; nota de FC-01 L20-23.

**Preflight de la ronda 1** (`docs/automation/evidence/I-62-A7/`, salvo `r2/` y `r2-dispositions/`; 35 archivos, enteros, sin cambio desde
la ronda 1; la ronda 2 reutiliza por identidad su reproducción sobre R7-A, los negativos de (a), (b) y (d), la compatibilidad, la reconstrucción
estática y las guardas sin cambio):

```text
docs/automation/evidence/I-62-A7/MANIFEST.sha256                           blob f87c7c07a3b215cdfeacabe3e6384639a648b4fb  2 845 bytes  33 líneas
docs/automation/evidence/I-62-A7/a7-guards-result-run.json                 blob 4918ac1a28b6e0faeba1d15a1e240040865f1cdc  575 bytes  38 líneas
docs/automation/evidence/I-62-A7/a7-guards.py                              blob 2f0f9677387c88c9833ddbf738d3eb7107081e6d  41 295 bytes  652 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__a7-guards.py.txt
docs/automation/evidence/I-62-A7/b1-like/main/FX-U2-T1.gate-contract.json  blob f1f3b22ab696a22e25f1cc74d4f83cb371c22419  4 836 bytes  170 líneas
docs/automation/evidence/I-62-A7/b1-like/main/deleg.json                   blob 97e125590cd2efe0e73e0f0711b2075219571fc6  6 173 bytes  223 líneas
docs/automation/evidence/I-62-A7/b1-like/main/designation.json             blob 2bf60f834c2a1c01e86c15de25b6a6174a07c64b  1 705 bytes  51 líneas
docs/automation/evidence/I-62-A7/b1-like/main/fixture-tests.trx            blob 40295387f9b489d2bf7ff4e517adc2d99c0d35ff  2 772 bytes  34 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__b1-like__main__fixture-tests.trx.txt
docs/automation/evidence/I-62-A7/b1-like/main/handoff.json                 blob e058fb7bd5493a8bee638ac2a621630a00033356  1 579 bytes  63 líneas
docs/automation/evidence/I-62-A7/b1-like/uc/FX-U2-T1.gate-contract.json    blob 1a2a3310714fad96e49b34dd5e671e50ed229d4b  4 836 bytes  170 líneas
docs/automation/evidence/I-62-A7/b1-like/uc/deleg.json                     blob e0c420b9b2500d63b0c8b9e6b7ec6ac549c96ec0  6 173 bytes  223 líneas
docs/automation/evidence/I-62-A7/b1-like/uc/designation.json               blob 71f2a9ada9e52f1513ed458ff41a0e0539f61ba4  1 698 bytes  51 líneas
docs/automation/evidence/I-62-A7/b1-like/uc/fixture-tests.trx              blob 40295387f9b489d2bf7ff4e517adc2d99c0d35ff  2 772 bytes  34 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__b1-like__uc__fixture-tests.trx.txt
docs/automation/evidence/I-62-A7/b1-like/uc/handoff.json                   blob 8cd8319b92c8c0d918a30dd23f8e0ce59a9b4dc2  1 579 bytes  63 líneas
docs/automation/evidence/I-62-A7/check_a7_closure.py                       blob 500ea477b88aed6ce78c50948edef5a9378e8eef  25 267 bytes  436 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__check_a7_closure.py.txt
docs/automation/evidence/I-62-A7/check_citation_blobs.py                   blob a5d239b0a5cc9efbecf084f1d3c7c47a1fb35ef2  8 110 bytes  168 líneas
docs/automation/evidence/I-62-A7/derive_r4.py                              blob a6f87be801f9dbf33144e20ef5b6aaee6b11eab8  13 783 bytes  364 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A7__derive_r4.py.txt
docs/automation/evidence/I-62-A7/designate_m7.py                           blob bbf548608ef43fbb6af05f4574fbc9c6785b365b  13 239 bytes  258 líneas
docs/automation/evidence/I-62-A7/make_eic_a7.py                            blob dfe36545c9a7be50cc6a0c3aaf499788932259ae  22 103 bytes  381 líneas
docs/automation/evidence/I-62-A7/make_eic_verify.py                        blob 965f42e8460eeb7a0b2770978be6292f05170663  18 620 bytes  363 líneas
docs/automation/evidence/I-62-A7/p0-env.json                               blob e2727cf0c5d96c4df42ec514584c25a15a8caa5f  3 247 bytes  74 líneas
docs/automation/evidence/I-62-A7/p1-repro.json                             blob 06de3ae2c2c97b0603c403f77f46970874ee76d4  49 600 bytes  1 762 líneas
docs/automation/evidence/I-62-A7/p2-b1.json                                blob bed36eb32a43908201c2b0a059a35a0aced74ede  39 828 bytes  1 190 líneas
docs/automation/evidence/I-62-A7/p3-neg.json                               blob 231475adf7ef2edcd8d1adab7033ab0794161031  48 633 bytes  1 446 líneas
docs/automation/evidence/I-62-A7/p4-guards-preview.json                    blob 4b08b2dc21adb1e16d17552f2b814a1483fb8aa5  618 bytes  38 líneas
docs/automation/evidence/I-62-A7/p4-guards-selftest.json                   blob 8f7a8aa6eaeb47b557cd089cbf4207e359a3c3e0  10 276 bytes  450 líneas
docs/automation/evidence/I-62-A7/p4-guards.json                            blob 1af20a5c8f301984c6184ff0e61e6a2281785712  8 922 bytes  350 líneas
docs/automation/evidence/I-62-A7/p5-compat.json                            blob de550fd0a71b56f1e9be6bd6e41f6b7b7d3b972b  10 478 bytes  258 líneas
docs/automation/evidence/I-62-A7/p6-recon.json                             blob 5457983fcd985abf5b1bc8514dbdb489d921b52f  12 738 bytes  451 líneas
docs/automation/evidence/I-62-A7/preflight.md                              blob 94dafe0fb2e4d78dc47906e888142f90ee1cf342  67 514 bytes  347 líneas
docs/automation/evidence/I-62-A7/preflight_text.py                         blob 0df15363abd52ec090ad6041381b608a7feed4ca  18 436 bytes  224 líneas
docs/automation/evidence/I-62-A7/recon_static.py                           blob 23825e6fb9aae63c2fb53bf41103f4481c277842  10 300 bytes  161 líneas
docs/automation/evidence/I-62-A7/reconstruct_runs.py                       blob 8a00ac1adaacd99e67847ef30b8ab3b11992e306  18 603 bytes  416 líneas
docs/automation/evidence/I-62-A7/reconstruct_runs_m7.py                    blob 15e1d4bbbc9d16450d77069bc64899966ccbaa64  24 518 bytes  521 líneas
docs/automation/evidence/I-62-A7/run_a7.py                                 blob 6cf0e36a6c3a0f159da6f3aa8e86fbed1d36497b  74 452 bytes  1 024 líneas
docs/automation/evidence/I-62-A7/summary.json                              blob 6790360b0c682cb9d15ba45d72844c3a861add81  1 133 bytes  67 líneas
```

**Revisión de la ronda 1** (`docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/`, tres archivos): `output.json` (R1-R3 y O1-O8, los
temas, la materialidad, los impactos relacionados y las preguntas: RunId L3; ReviewedBlob L12; Verdict L176; RequiredFindings L177-244; OptionalFindings L245-349; TopicVerdicts L350-518; Materiality L519-623; OwnerReservedConsequence L624-637; RelatedImpacts L638-790; QuestionAnswers L791-874), `audit.json` (`Accreditation` L5729-5734; `Fidelity` L1346-2005) y `README.md`.

```text
docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/README.md    blob cb653450f461a26243163d0eaf32968d8ddd7476  54 441 bytes  450 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-architect-A-7__R20261019T010200Z-4a72__README.md.txt
docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/audit.json   blob 77a15c03efd58930e2262a2419846bc4090d1216  175 762 bytes  5 735 líneas
docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/output.json  blob 4bd02fbfd81e2dd597a5aa78546c5425b42ad8db  61 494 bytes  895 líneas
```

**Evidencia de la medición de A6-7 (`m7/`)** (55 archivos, enteros; orientación para mencionar si R1-R3 cambian D-1 o las propuestas FC: la
determinación de §91, `i6-determination.md`, que A-7 cita como origen; D-1 (`s91/d1/`) y las doce lecturas (`s91/r12/`); los borradores de §92
(`s92/`); del banco, su README, las designaciones, `p1-build.json` y los resultados de N4 y nc1 (`p3-n4.json`, `p4-nc1.json`) con los que compara el
preflight de la ronda 2; y `readings/readings.md`):

```text
docs/automation/evidence/I-62-A6/m7/bench/README.md                                              blob 59a4bb30bb07a696aad39f2e3c9e60ded5b54bd6  63 123 bytes  272 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/M.json                                    blob 4af8da4fbdd106bb25c0b3a4c255df72b8d13f81  1 457 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/N4.json                                   blob c1cc3179589e8aaa27e5653826ab1ded701a4435  1 456 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/N5.json                                   blob 85fe11273f80bc9207e54ec6d9ab5eaae5481065  1 456 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/N6.json                                   blob 4e1fa896645d8e81103a220f87945ccfec6e6b2f  1 456 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/N7A.json                                  blob 35330497f372240730f62847e3b55cba2c8cb5a4  1 462 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/N7AR-OWN.json                             blob 03415ed9108294bc9128b5a6918e90d63ed83b18  1 482 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/N7AR-SAME.json                            blob d888d197b70483cafc74e92d1a3a4dbe31e48aaa  1 486 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/N7B.json                                  blob 59b54be6e59144b85165edbed9a3d2887df9fbc2  1 462 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/NC1.json                                  blob 4716924db0c1d1901ca2fc03a2155402464e660c  1 463 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/NC2.json                                  blob 7b647a8950bb9c9b0e6e29a73f6560eea1b73752  1 461 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/NC3.json                                  blob 5c0f2a70b55c67d74e247347692c2e2869c5ce6a  1 461 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/designations/V.json                                    blob 37ae94d2d2af58e5993510115cee799024bd8dd8  1 445 bytes  47 líneas
docs/automation/evidence/I-62-A6/m7/bench/p1-build.json                                          blob 2964326ae1f7108fc472cbc4c2a34886bfa9ccc3  52 072 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__m7__bench__p1-build.json.txt
docs/automation/evidence/I-62-A6/m7/bench/p3-n4.json                                             blob eb853932353c0270a8c5e383c679b7d3afe9a35f  15 848 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__m7__bench__p3-n4.json.txt
docs/automation/evidence/I-62-A6/m7/bench/p4-nc1.json                                            blob e122832e6e67e68dd9aa672a2e10a64460a8e43d  5 992 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__m7__bench__p4-nc1.json.txt
docs/automation/evidence/I-62-A6/m7/readings/readings.md                                         blob 0991419aad73203add0333ee36c3ab341b427b68  62 466 bytes  415 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__m7__readings__readings.md.txt
docs/automation/evidence/I-62-A6/m7/s91/MANIFEST.sha256                                          blob 1f71e46803c5b78b49421f70d1b2ccee20e4180b  1 455 bytes  17 líneas
docs/automation/evidence/I-62-A6/m7/s91/d1/d1.md                                                 blob efed5af2785a3d5396aebcff29d1524a464cb6e8  29 861 bytes  295 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__m7__s91__d1__d1.md.txt
docs/automation/evidence/I-62-A6/m7/s91/d1/make_summary.py                                       blob 00c0a04f03df02f1ee260d504c4432b4f3898493  9 365 bytes  145 líneas
docs/automation/evidence/I-62-A6/m7/s91/d1/min/d1_min.py                                         blob 2c0e7d9c580964e712fc489733f172c54471a459  16 795 bytes  297 líneas
docs/automation/evidence/I-62-A6/m7/s91/d1/min/min-result.json                                   blob 66fe24d9660e77a79fa702d8d0d661ae9b502faf  14 976 bytes  693 líneas
docs/automation/evidence/I-62-A6/m7/s91/d1/nc2/d1_nc2.py                                         blob cc21774e600783707ab561db376b04578153aa39  30 932 bytes  505 líneas
docs/automation/evidence/I-62-A6/m7/s91/d1/nc2/nc2-result.json                                   blob 30c2bfb1398ccefc2e63e930eeb151d4427709bb  288 225 bytes  6 084 líneas
docs/automation/evidence/I-62-A6/m7/s91/d1/summary.json                                          blob 525c09f7228964c034bc6641be96c44e3b1846c4  35 940 bytes  1 259 líneas
docs/automation/evidence/I-62-A6/m7/s91/i6-determination.md                                      blob 625e863df4b887c702133ebdda72b40b30388672  9 214 bytes  77 líneas
docs/automation/evidence/I-62-A6/m7/s91/r12/probes/probe-r12-result.json                         blob 2bff66a1293df8e1eecde173ac18f1a7cc20c259  13 748 bytes  569 líneas
docs/automation/evidence/I-62-A6/m7/s91/r12/probes/probe_r12.py                                  blob 6e4f64ce5b53ef30c83bed3c7fa098782fc844cb  19 632 bytes  462 líneas
docs/automation/evidence/I-62-A6/m7/s91/r12/r12-citations.json                                   blob 64a31def7e79ed310218d7b7bca7d41eacc29701  32 924 bytes  251 líneas
docs/automation/evidence/I-62-A6/m7/s91/r12/r12-verify-result.json                               blob 11212d5ea6552d9e27c7e45fbae0b0bb649b256a  791 bytes  23 líneas
docs/automation/evidence/I-62-A6/m7/s91/r12/r12.json                                             blob d70c7e3b806825b476865a329db0af7f0c73a88e  30 385 bytes  263 líneas
docs/automation/evidence/I-62-A6/m7/s91/r12/r12.md                                               blob 99105379e8aa638d8f43a2d3be6a33765a96fc32  59 504 bytes  620 líneas
docs/automation/evidence/I-62-A6/m7/s91/r12/verify_r12.py                                        blob f451fdcb1998eee3d2522cd6e4a4cba602ea05ef  10 051 bytes  241 líneas
docs/automation/evidence/I-62-A6/m7/s91/reconstruction-review.md                                 blob 2c6cd983fc3cf5b6d159e02520c10daf228b62c8  5 250 bytes  53 líneas
docs/automation/evidence/I-62-A6/m7/s91/risk-matrix.md                                           blob f801c7bdbb9322ccb3f0d08fc2b384ef02f5a62b  5 133 bytes  39 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__m7__s91__risk-matrix.md.txt
docs/automation/evidence/I-62-A6/m7/s92/MANIFEST.sha256                                          blob f20ba008a43514995c8e255fb59b73d14ae55f93  1 890 bytes  19 líneas
docs/automation/evidence/I-62-A6/m7/s92/contract/contract_crosscheck.py                          blob 6e07070e6269ffb018b8ec0e5e99db8b6df8d08a  5 488 bytes  111 líneas
docs/automation/evidence/I-62-A6/m7/s92/contract/declared_orders.py                              blob 14fda7e8c2fe6968dd027ef979602a20a88bd986  1 759 bytes  24 líneas
docs/automation/evidence/I-62-A6/m7/s92/contract/disposition-draft.md                            blob 5abfd5980d9e837f90346de55a994dc0b08016a4  29 297 bytes  258 líneas
docs/automation/evidence/I-62-A6/m7/s92/contract/out/s92-nc2-r/acceptor-stdout.json              blob 3d49906470b7321aa1ff1a4bbc47ddaa2f05930e  6 696 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__m7__s92__contract__out__s92-nc2-r__acceptor-stdout.json.txt
docs/automation/evidence/I-62-A6/m7/s92/contract/out/s92-nc2-r/controller-output.json            blob 4b0019ebb9f027996d8a1b855fe55b51a7dee044  9 494 bytes  172 líneas
docs/automation/evidence/I-62-A6/m7/s92/contract/out/s92-nc2-r/envelope-read-json-CONTRACT.json  blob 40c3796b02aa9c0cd1448a099b4ad43b2f792338  3 962 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__m7__s92__contract__out__s92-nc2-r__envelope-read-json-CONTRACT.json.txt
docs/automation/evidence/I-62-A6/m7/s92/contract/out/s92-nc2-r/envelope-read-json-DELEG.json     blob be8e5dfa5fd1bfda6af030a58fb8d5fbcee4206e  3 975 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__m7__s92__contract__out__s92-nc2-r__envelope-read-json-DELEG.json.txt
docs/automation/evidence/I-62-A6/m7/s92/contract/out/s92-nc2-r/request.json                      blob 74e492588480692d29c5594a3cfbb134d22a27fc  7 552 bytes  277 líneas
docs/automation/evidence/I-62-A6/m7/s92/contract/run.log                                         blob 668298f7458b6f7342e55fd2a85fe902c7e2257e  1 463 bytes  16 líneas
docs/automation/evidence/I-62-A6/m7/s92/contract/s92-result.json                                 blob b0b6fea2e2b1bbb4515d28c83d5a9cf724b2864b  14 399 bytes  665 líneas
docs/automation/evidence/I-62-A6/m7/s92/contract/s92_bench.py                                    blob 6aaf3bb65eb681b6333377c8376bb3f8e8be8de6  15 871 bytes  250 líneas
docs/automation/evidence/I-62-A6/m7/s92/coordinator-notes.md                                     blob 32fc9c87330024e6b1d5b04d846537a8a439dc36  2 674 bytes  33 líneas
docs/automation/evidence/I-62-A6/m7/s92/readings/probes/probe-proposals-result.json              blob b1f98dbbd0a8e313191f8e01218772364b32e14b  6 302 bytes  276 líneas
docs/automation/evidence/I-62-A6/m7/s92/readings/probes/probe_proposals.py                       blob 32d8b430a0d79d878842d0cea364c5622726e2f7  18 466 bytes  444 líneas
docs/automation/evidence/I-62-A6/m7/s92/readings/proposals-citations.json                        blob 8e90781d58ae2de6106a1a840d99ab4c6c463f33  13 953 bytes  98 líneas
docs/automation/evidence/I-62-A6/m7/s92/readings/proposals-verify-result.json                    blob 9c274de51dc0f2b97c5c165ace23d492b20ec2d2  1 369 bytes  29 líneas
docs/automation/evidence/I-62-A6/m7/s92/readings/proposals.json                                  blob be48183314681cbf5095bc7c817889f42793e566  11 155 bytes  147 líneas
docs/automation/evidence/I-62-A6/m7/s92/readings/proposals.md                                    blob 4dc89ad66fc0c1718bd80054575ed1b71fda493d  31 529 bytes  331 líneas
docs/automation/evidence/I-62-A6/m7/s92/readings/verify_proposals.py                             blob 232337401eaf87a78e96da627f405b781ac99a75  9 314 bytes  221 líneas
```

**Evidencia de la ronda 4 de A-6 que cita el preflight de la ronda 1** (§5, negativos reutilizados por identidad). Los tres JSON son de una sola línea:
léelos por su copia fiel o lee la **copia legible** de `docs/automation/evidence/I-62-A6/r4/readable/` (misma ruta relativa; `readable/MANIFEST.json`
fija, para cada copia, el archivo canónico, su blob y la transformación), que es otro archivo del cierre y se cita con su ruta:

```text
docs/automation/evidence/I-62-A6/r4/r1probes/p10-nc1.json             blob 58d49e2e65093ede0df3197f6c88476ac6fc4e6b  37 501 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__r4__r1probes__p10-nc1.json.txt
docs/automation/evidence/I-62-A6/r4/r1probes/p7-derive.json           blob 906c704ace458d877ef6daaedbcb264fcff4d871  81 082 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__r4__r1probes__p7-derive.json.txt
docs/automation/evidence/I-62-A6/r4/r1probes/p9-n4.json               blob 8b912b084a9629f57461bacb79af9c12e477bdc1  27 045 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-A6__r4__r1probes__p9-n4.json.txt
docs/automation/evidence/I-62-A6/r4/readable/MANIFEST.json            blob e4dc9e609fc8335b84d1e4f9b5c32511d18fc929  3 533 bytes  54 líneas
docs/automation/evidence/I-62-A6/r4/readable/r1probes/p10-nc1.json    blob 56e12b8e5bc590848f7ce0c28eb50b54749cfca8  45 660 bytes  1 300 líneas
docs/automation/evidence/I-62-A6/r4/readable/r1probes/p7-derive.json  blob 8426d94d61209981144962c1a4d0438259c661aa  108 799 bytes  3 787 líneas
docs/automation/evidence/I-62-A6/r4/readable/r1probes/p9-n4.json      blob 95a9b3d34ae1d95e5deafcb7f14ea9d3e10a3d33  32 087 bytes  828 líneas
```

**Kit de la medición `a6m7-measure`** (44 archivos, enteros; congelado y sin lanzar; A-7 §10 lo cita, y la observación 7 del preflight de la
ronda 2 dice que su próxima preparación tiene que adoptar `a7-designation/2`):

```text
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/FREEZE.json                           blob c3bb2eee3dac4b1d4beb1a7674ebf8c31bbf35a9  6 883 bytes  212 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/README.md                             blob 9972b73243165ccefb90536f127b12dd8ca45c4f  23 132 bytes  222 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__kits__FX-02__a6m7-measure__README.md.txt
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/baseline-keynames-F909F480.json       blob dcbc844cd4b5c94d12264b0508c1d92ac6b4c031  4 183 bytes  109 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/build_m7.py                           blob 5d46cd11796275771e04262567bf12f3c394b0e6  7 844 bytes  163 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/cfg-fp.ps1                            blob 4300668a453b639f3777a57039fa8fc609341d12  1 479 bytes  24 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/derive_r4.py                          blob a6f87be801f9dbf33144e20ef5b6aaee6b11eab8  13 783 bytes  364 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__kits__FX-02__a6m7-measure__derive_r4.py.txt
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/designate_m7.py                       blob bbf548608ef43fbb6af05f4574fbc9c6785b365b  13 239 bytes  258 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/disposition-markers.json              blob 616fad3cb1d745cf422298a3ffcc70edbdbdfdf8  1 671 bytes  20 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/E.json                         blob 9107abe6af9d1ff6e225c1e06041d7512f4f6d1d  9 222 bytes  287 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/closure.json                   blob d30cab4d67871d94c8cbd9ec3c41b85d25025703  14 317 bytes  445 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/derivation.json                blob 9d330e5c1781548c0aacf1f023f00f16899c6418  1 793 bytes  71 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/designation-M.json             blob 098661825f779b2661171b25609eb95f24d97674  1 470 bytes  48 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/m7-run.snapshot.json           blob 3f9318c24b515fc966c91f01df7c6f347993407c  45 716 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__kits__FX-02__a6m7-measure__frozen__m7-run.snapshot.json.txt
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/m7-run.snapshot.record.json    blob 4e3e66d26d5d2d65ffd6904f64909bea08822bfe  2 228 bytes  67 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/oracle.json                    blob 57d99b15e5a5bd651d5a7483b22a1b1c3fefd6e2  18 745 bytes  516 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/prompt.m7.armed.sanitized.txt  blob c70ccbeb69368c4aade9e94c827e2c43301a67f2  15 738 bytes  94 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/frozen/vector.json                    blob c464a18a1f7fee3662fd3d5d82c22f21f87a508c  1 043 bytes  26 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/gates_m7.py                           blob 04d16b83523546c6e1743cf79baff031a3d2fd0f  25 980 bytes  524 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/deleg.json                     blob 71cda7f01082b5a98f05db509348995480d5f18f  6 298 bytes  228 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/fixture-tests.trx              blob 40295387f9b489d2bf7ff4e517adc2d99c0d35ff  2 772 bytes  34 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__kits__FX-02__a6m7-measure__inputs__fixture-tests.trx.txt
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/handoff.json                   blob d69d5276635d61b38153c22f93b15a641f9335d8  1 683 bytes  64 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/lib_kit.py                            blob bc264b1123467845f883dd10ab0748baba22f224  13 239 bytes  312 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/line-b.txt                            blob e6a5d469fd688c73946210b40ca305897eca7422  876 bytes  1 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/line-od2.txt                          blob 0350f79124d79a00c02a23e9f8f548f7b858a0cf  161 bytes  1 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/m7-params.json                        blob dd9ce361a1b507160f8757b2790a958b575b4a81  2 777 bytes  28 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/make_eic_verify.py                    blob 965f42e8460eeb7a0b2770978be6292f05170663  18 620 bytes  363 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/pou_check.py                          blob 555a1abc29e20ebb07b5582015f7aa5303989929  24 195 bytes  446 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/pou_m7.py                             blob 82a2e6aa9f3764a31b89f5abfaa7597698f5e21d  7 441 bytes  139 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/prep_clone.py                         blob 992087990bcd51cc1932a7b23b8a209b624e9e17  6 731 bytes  118 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/prep_write.py                         blob 6c204fa70a3c470ec4f45324dcfc9715c5bdbf2f  5 934 bytes  99 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/procs-snapshot.ps1                    blob 4c7e41e91459fa2a1a1b72d30976ff39911b1eea  1 236 bytes  20 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/prompt.m7.template.txt                blob d9093e80bb9803bbb9112cb7f30d30533d07f6a3  6 381 bytes  58 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/reconstruct_runs.py                   blob 8a00ac1adaacd99e67847ef30b8ab3b11992e306  18 603 bytes  416 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/reconstruct_runs_m7.py                blob 15e1d4bbbc9d16450d77069bc64899966ccbaa64  24 518 bytes  521 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/run_m7.sh                             blob 3bce5a9866e5411784494048e4f3b9ea2d6e2a89  7 275 bytes  102 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/schema.m7.json                        blob d4419fb9841564a08d131994c1dc702378219fe2  13 803 bytes  727 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/schema.m7.strict-projection.json      blob 50ba8fd424b108dcd237500e6020fc2f8e3f16af  13 480 bytes  711 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/selftest-report.json                  blob a0d59667621ed04f6a1bfe86fe7d83621468bec1  36 641 bytes  1 321 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/selftest_m7.py                        blob 26085367f892f4837222e3bb3d745aacb502086a  64 690 bytes  909 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/selftest_m7.sh                        blob 56b62bf0f45844be737682093a256d5a5c12f0b2  796 bytes  11 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/table_m7.py                           blob b5c16f5226a902b26e9729602416b9b79a0db679  7 865 bytes  149 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/table_r3.py                           blob 9e5dc41147539e1308f2ba497e4b821c70e0af92  14 518 bytes  264 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/table_r4.py                           blob 552523afa0fb93a5aac8749bcbef18d750b8f6e0  3 817 bytes  80 líneas
docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/tools_m7.py                           blob 3bea9518e4d141208eee6ebaa60c21fbfafe6af6  70 467 bytes  1 220 líneas
```

**Paquete B-min** (30 archivos, enteros; el instrumento fijado es `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py` (1 614 líneas) y el aceptante fijado es `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py`
(2 816 líneas, por tramos de 300)):

```text
docs/automation/evidence/I-62-F6/FX-02/bmin/README.md                           blob 57a0fb0c234a27df96fdb2ea82b3b2bceb3d4bea  11 764 bytes  143 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/SPEC.md                             blob 4d448c6fc4670cdf97b21dcd17fc00ba1bedcc0c  23 863 bytes  316 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept-suite.log                    blob 9cd2f405d7c1355f046f64ee5a66a1feeceb6a45  5 274 bytes  58 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept/ACCEPT-NOTES.md              blob cb9df0289c69617122feaa896728c3a86d2040a8  21 336 bytes  240 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py               blob 434ef1fccb0d15762b30244b4a60928120698411  111 873 bytes  2 816 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept/tests/fake_tool.py           blob 3a3e3d92b2ffe3d0bb52770bdb45787a8d3f2179  14 978 bytes  373 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept/tests/test_bmin_accept.py    blob fb6fbeeeab6774e13c7d4ccedbc0e569cd927368  74 902 bytes  1 445 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__accept__tests__test_bmin_accept.py.txt
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/e2e-run.log                     blob 6e1f940e02b934599f1389e1f56eb7b6877ba3e9  2 602 bytes  85 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/e2e.py                          blob fbfa3353a1a7df62e7805e5af8954acde5c425d2  15 475 bytes  233 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/acceptor-result.json        blob de2d2c92181cc3ff03c51085ac195cc30c2e1327  6 502 bytes  293 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/e2e-summary.json            blob 5b5972190d898550b22b742d268d90d6c52a172f  7 711 bytes  387 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-clausemap.json     blob 150bbed82950853e3dcb331b99234222f2168543  44 637 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-clausemap.json.txt
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-cleantree.json     blob 05f2026175e9c26df4058e55cc4518a323af2c7d  903 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-contract.json      blob beab3abd40b9ddfae7094eb09d77feac70e32e55  2 294 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-contract.json.txt
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-diff.json          blob b34cd0ce331f5ea2dc1a0873c1c56141bc986f1e  2 083 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-diff.json.txt
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-freetext.json      blob ac658e4c60c35cdd7e391f402c890860496cdc56  1 733 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-handoff.json       blob 0a29fc6d93e4b186bc5229f748a44c9ee1a313de  1 855 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-hashes.json        blob a4821e2b6f20853611bd4263d8379267f0a17a7e  2 244 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-hashes.json.txt
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-identity.json      blob be4685808202de4667267b01f55b342ac42b6ce6  2 393 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-identity.json.txt
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-read-json.json     blob 3eaaab4f4b8a26485f14e0146b673a211be7030c  1 568 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-read-text-1.json   blob b289013983e4c8bb7a13712f4365015dd1bf499d  1 344 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-read-text-73.json  blob 3a336ed4c59d0c312e64d4f6e4dac91507deee74  1 541 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-scope.json         blob a66e75761bbe8cc023e45b30bd84da3196c8b2dd  3 168 bytes  1 líneas  → léelo en la copia fiel del run fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__e2e__out__envelope-scope.json.txt
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-trx.json           blob 45e8e29ce034e9addd5eec2c1685540eb250efea  1 101 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool-suite.log                      blob 08f11ab465d8894806e228fc0d4045e6dc6bdeda  177 bytes  6 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/TOOL-NOTES.md                  blob d3218ff79c84a6763db67081408785304a24e0d1  17 434 bytes  215 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py                  blob 9a829b0eeb1210238052b1feac06cfa8f7964aed  64 764 bytes  1 614 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/mutate.py                blob 6967938954219a6a8a2f38dc80ef5a1165966d19  12 145 bytes  280 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/test_bmin_facts.py       blob 0a74bb2dcf18c1c62ffb55ca87da0e259046fe6d  80 642 bytes  1 573 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/zero_write_check.py      blob 4188d256b76bc3a375454027d208b98d801c8ccc  3 491 bytes  85 líneas
```

**Canónicos.** Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por tramos de 300 líneas como máximo si pasa de
60 000 bytes.
1. `docs/initiatives/I-62-proposal-v14.md` (342 761 bytes, 3 323 líneas; blob `34ad80ea`), §18, §20.3, §20.3.1, B.9, Anexo C (C-24), D.3 y D.6: §18 952-977; §18, OV-I62-03 960; §18, OD-5 974; §20.3, introducción (hasta §20.3.1) 1049-1071; §20.3.1 1073-1130; §20.3.1, clase CanonicalInputs 1079; §20.3.1, pasos 2-4 1088-1099; B.9 2223-2252; B.9, Target, AuthorityRevision y CanonicalInputs 2231-2233; B.9, el revisor lee CanonicalInputs y nada más 2250-2251; Anexo C, C-24 2363; D.3 2431-2528; D.6 2567-2587; D.3, Topología A: tabla de roles, adapters y topes 2433-2447. **Léelo solo en su copia fiel** `fiel__docs__initiatives__I-62-proposal-v14.md.txt` (run; misma numeración).
2. `docs/AUTOMATION_PLAN.md` (155 229 bytes, 1 732 líneas; blob `f525cb1e`), 16.3, 16.4, 16.5, 16.6, 16.7, 16.8, 16.9, 16.13, 16.14, 16.22 y 16.24: 16.3 488-505; 16.4 507-543; 16.5 545-567; 16.6 569-580; 16.7 582-598; 16.8 600-618; 16.9 620-665; 16.13 717-928; 16.14 930-948; 16.22 1182-1209; 16.24 1256-1323; 16.3, AuthorityRevision 490; 16.8, RED de la cadena: una entrega «exige RED» (L616-L617): LECTURA DIRECTA OBLIGATORIA 616-617; 16.9, filas 1-14 de la tabla de comprobaciones 630-643; 16.9, tabla de las catorce comprobaciones 628-643; 16.13, Resolve, pasos 0-4 819-827; 16.13, Validate(M): MV-1..MV-7 902-908; 16.13, tabla de defectos de Validate(M): MAP_INVALID 910-918; 16.24, clase CanonicalInputs 1264; 16.24, clases AllowedTransitiveInputs, DeclaredRuntimeContext y ForbiddenInputs 1265-1267; 16.24, cálculo del cierre, pasos 2-4 1271-1276; 16.24, auditoría de lecturas: P-22 y Contexto 1279-1280; 16.24, fidelidad de los insumos 1287-1288; 16.24, P-22..P-25 1318-1321. **Léelo solo en su copia fiel** `fiel__docs__AUTOMATION_PLAN.md.txt` (run; misma numeración).
3. `docs/automation/agent-execution/README.md` (60 779 bytes, 756 líneas; blob `592dcfd4`), §5, §8, §10, §14 y §15: §5 132-150; §8 170-188; §10 217-235; §14 369-531; §14.1 375-383; §14.2 385-403; §14.3 405-427; §14.4 429-437; §14.5 439-486; §14.6 488-498; §14.7 500-531; §15 533-603; §15.1 537-551; §15.2 553-568; §15.3 570-583; §15.4 585-603; §10, controles negativos 219-228; §15.2, regla I2 561; §15.2, regla I6 y auditoría de lecturas 565-568.
4. `docs/initiatives/I-62-A-6.md` (151 865 bytes, 1 447 líneas; blob `4bc919dd`), entera: cabecera 1-69; §1 71-99; §2 101-406; §3 408-1175; §3.1 410-442; §3.2 444-458; §3.3 460-481; §3.4 483-522; §3.5 524-544; §3.6 546-628; §3.7 630-656; §3.8 658-733; §3.9 735-749; §3.10 751-794; §3.11 796-816; §3.12 818-1001; §3.13 1003-1024; §3.14 1026-1036; §3.15 1038-1048; §3.16 1050-1064; §3.17 1066-1107; §3.18 1109-1175; §4 1177-1202; §5 1204-1219; §6 1221-1287; §6.1 1223-1242; §6.2 1244-1260; §6.3 1262-1287; §7 1289-1326; §8 1328-1372; §9 1374-1383; §10 1385-1413; §11 1415-1425; Anexo (no normativo) 1427-1447; A6-3, regla 6 (b) 600-617; §3.12, tabla de derivación del cierre del aceptante (A6-9, regla 4) 848-872; §7, línea del Owner (a) A6-ADR0046-9-FX02 1324; §7, línea del Owner (b) A6-CONTROLLER-MEDICION-CONSUMO 1325; §5, M-01 1212; §5, M-02 1213; §5, M-03 1214; §5, M-04 1215; §5, M-05 1216; §5, M-06 1217; §5, M-07 1218; §5, M-08 1219; A6-1, regla 1 492-500; A6-1, regla 2 501-502; A6-1, regla 3 503-505; A6-1, regla 4 506-509; A6-1, regla 5 510-512; A6-1, regla 6 513-515; A6-1, regla 7 516-518; A6-1, regla 8 519-520; A6-1, regla 9 521-522; A6-2, regla 1 532-535; A6-2, regla 2 536-538; A6-2, regla 3 539-541; A6-2, regla 4 542-543; A6-2, regla 5 544; A6-3, regla 1 555-564; A6-3, regla 2 565-566; A6-3, regla 3 567-579; A6-3, regla 4 580-583; A6-3, regla 5 584-589; A6-3, regla 6 590-626; A6-3, regla 7 627-628; A6-4, regla 1 638-641; A6-4, regla 2 642-644; A6-4, regla 3 645-654; A6-4, regla 4 655-656; A6-5, regla 1 667-672; A6-5, regla 2 673-686; A6-5, regla 3 687-688; A6-5, regla 4 689-702; A6-5, regla 5 703-706; A6-5, regla 6 707-716; A6-5, regla 7 717-733; A6-6, regla 1 743-744; A6-6, regla 2 745-746; A6-6, regla 3 747-749; A6-7, regla 1 759-770; A6-7, regla 2 771; A6-7, regla 3 772-777; A6-7, regla 4 778-780; A6-7, regla 5 781-786; A6-7, regla 6 787-788; A6-7, regla 7 789-794; A6-8, regla 1 805-807; A6-8, regla 2 808-810; A6-8, regla 3 811-812; A6-8, regla 4 813-814; A6-8, regla 5 815-816; A6-9, regla 1 827-828; A6-9, regla 2 829-831; A6-9, regla 3 832-840; A6-9, regla 4 841-843; A6-9, regla 5 844-845; A6-9, regla 6 846; A-7 §1 y §3.2: fin de A6-8 815-816.
5. `docs/initiatives/I-62-A-5.md` (37 342 bytes, 372 líneas; blob `ad94ceed`), entera: cabecera 1-49; §1 51-72; §2 74-147; §3 149-242; §3.1 151-167; §3.2 169-199; §3.3 201-221; §3.4 223-242; §4 244-259; §5 261-272; §6 274-296; §7 298-317; §8 319-334; §9 336-344; §10 346-354; Anexo (no normativo) 356-372; §3.2, párrafo A5-1 175-199; §3.3, A5-2, regla 4 216; §3.3, fin de A5-2 y de D.3 con A-1..A-5 221.
6. `docs/initiatives/I-62-A-4.md` (62 415 bytes, 516 líneas; blob `5e2ba68e`), cabecera (orientación): cabecera 1-56. **Léelo solo en su copia fiel** `fiel__docs__initiatives__I-62-A-4.md.txt` (run; misma numeración).
7. `docs/initiatives/I-62-A-3.md` (200 993 bytes, 1 536 líneas; blob `ea6721f7`), cabecera (orientación): cabecera 1-62. **Léelo solo en su copia fiel** `fiel__docs__initiatives__I-62-A-3.md.txt` (run; misma numeración).
8. `docs/initiatives/I-62-A-2.md` (29 957 bytes, 352 líneas; blob `f1e1d6f0`), cabecera (orientación): cabecera 1-48.
9. `docs/initiatives/I-62-A-1.md` (77 984 bytes, 437 líneas; blob `c01899a7`), cabecera (orientación): cabecera 1-56. **Léelo solo en su copia fiel** `fiel__docs__initiatives__I-62-A-1.md.txt` (run; misma numeración).
10. `docs/initiatives/I-62-consensus-freeze.md` (7 134 bytes, 113 líneas; blob `0f6b8608`), entero: cabecera 1-4; §1 6-41; §2 43-72; §3 74-85; §4 87-103; §5 105-113.
11. `docs/INITIATIVE_LIFECYCLE.md` (24 334 bytes, 360 líneas; blob `f19896a8`), §3, §5 y §6: §3 66-94; §3, M-01 83; §3, M-02 84; §3, M-03 85; §3, M-04 86; §3, M-05 87; §3, M-06 88; §3, M-07 89; §3, M-08 90; §3, UNKNOWN cuenta como activado 78-79; §3, OWNER-RESERVED 94; §5 152-191; §5, REQUIRED y OPTIONAL 166; §5, veredictos 171; §6 193-222.
12. `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` (9 871 bytes, 105 líneas; blob `da275e1a`), entero: cabecera 1-8; Contexto 10-24; Decisión 26-69; Alternativas consideradas 71-78; Consecuencias 80-97; Referencias 99-105; Decisión, punto 1 28-43; Decisión, punto 2 44-49; Decisión, punto 3 50-51; Decisión, punto 4 52-58; Decisión, punto 5 59-60; Decisión, punto 6 61-63; Decisión, punto 7 64-66; Decisión, punto 8 67-68; Decisión, punto 9 69. **Léelo solo en su copia fiel** `fiel__docs__adr__0046-protocolo-de-ejecucion-delegada-de-agentes.md.txt` (run; misma numeración).
13. `docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md` (10 832 bytes, 146 líneas; blob `e1bd8d91`), entero: cabecera 1-13; Contexto 15-26; Decisión 28-104; Alternativas consideradas 106-115; Consecuencias 117-135; Referencias 137-146; Decisión, punto 1 59-61; Decisión, punto 2 62-70; Decisión, punto 3 71-74; Decisión, punto 4 75-77; Decisión, punto 5 78-81; Decisión, punto 6 82-87; Decisión, punto 7 88-90; Decisión, punto 8 91-92; Decisión, punto 9 93-104.
14. `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md` (24 984 bytes, 217 líneas; blob `90929ad1`), entero: cabecera 1-15; §1 17-30; §2 32-95; §2.1 34-65; §2.2 67-74; §2.3 76-84; §2.4 86-95; §3 97-131; §3.1 99-115; §3.2 117-131; §4 133-166; §5 168-179; §6 181-199; §7 201-217; negativos con invocación: mismo prompt y Target de la verificación real 23; AuthorityRevision y CanonicalInputs del kit 44-45.
15. `docs/automation/agent-execution/compatibility/I62-clause-map.json` (19 104 bytes, 626 líneas; blob `4d49d3e1`), entero.
16. `docs/automation/agent-execution/compatibility/clause-map.schema.json` (2 722 bytes, 119 líneas; blob `6b54dda9`), entero.
17. `docs/automation/agent-execution/schemas/input-closure.v1.schema.json` (6 911 bytes, 282 líneas; blob `6bdf7184`), entero.
18. `docs/automation/agent-execution/schemas/role-invocation.v1.schema.json` (21 758 bytes, 858 líneas; blob `d54a7ae7`), entero.
19. `docs/automation/decisions/I-62.md` (313 385 bytes, 1 861 líneas; blob `b3be42d5`), §62, §73, §78 y §90-§94: §62 1206-1255; §73 1479-1498; §78 1564-1584; §90 1768-1787; §91 1789-1803; §92 1805-1823; §93 1825-1844; §94 1846-1861; §90, bytes y SHA-256 del cuerpo pegado 1771; §91, bytes y SHA-256 del cuerpo pegado 1792; §92, bytes y SHA-256 del cuerpo pegado 1808; §93, bytes y SHA-256 del cuerpo pegado 1828; §94, bytes y SHA-256 del cuerpo pegado 1849; §90, punto 1 1775; §90, punto 2 1776; §90, punto 3 1777; §90, punto 4 1778; §90, punto 5 1779; §90, punto 6 1780; §90, punto 7 1781; §91, punto 1 1796; §91, punto 2 1797; §91, punto 3 1798; §91, punto 4 1799; §91, punto 5 1800; §91, punto 6 1801; §91, punto 7 1802; §91, punto 8 1803; §92, punto 1 1812; §92, punto 2 1813; §92, punto 3 1814; §92, punto 4 1815; §92, punto 5 1816; §92, punto 6 1817; §92, punto 7 1818; §92, punto 8 1819; §92, punto 9 1820; §93, punto 1 1836; §93, punto 2 1837; §93, punto 3 1838; §93, punto 4 1839; §93, punto 5 1840; §93, punto 6 1841; §93, punto 7 1842; §93, punto 8 1843; §93, punto 9 1844; §94, punto 1 1853; §94, punto 2 1854; §94, punto 3 1855; §94, punto 4 1856; §94, punto 5 1857; §94, punto 6 1858; §94, punto 7 1859; §94, punto 8 1860; §94, punto 9 1861; §93, objeto identificado y verificado (punto 1) 1830-1832; §90, acuerdo registrado de A-6 (blob 4bc919dd) 1783-1787; §92, registro de las lecturas (punto 5) 1822-1823; §78, línea del Owner A6-ADR0046-9-FX02 1572; §62, U-19 (lectura registrada) 1234; §62, U-19 dispuesta entera 1250. **Léelo solo en su copia fiel** `fiel__docs__automation__decisions__I-62.md.txt` (run; misma numeración).
20. `docs/automation/evidence/I-62-evidence.md` (433 638 bytes, 4 780 líneas; blob `a3544ae9`), §187-§191 (orientación): §187 4696-4701; §188 4703-4716; §189 4718-4745; §190 4747-4753; §191 4755-4780. **Léelo solo en su copia fiel** `fiel__docs__automation__evidence__I-62-evidence.md.txt` (run; misma numeración).

**Evidencia de I-62** (`docs/automation/evidence/I-62-evidence.md`, en la lista de canónicos; rangos de orientación: §187, §188, §189, §190, §191). Es el registro
de las sesiones que prepararon A-7, su ronda 2 y sus preflights: contiene sus resúmenes y valoraciones (también de A-7). Úsala como evidencia de
hechos que puedes contrastar con los archivos custodiados y con el texto normativo, nunca como autoridad ni como juicio sobre A-7; no adoptes sus
conclusiones sin comprobarlas. Lo mismo vale para los `preflight.md` de las dos rondas, las disposiciones propuestas de O1-O8,
`i6-determination.md`, los borradores de `s92/` y `coordinator-notes.md`: son de la misma supervisión.

Nada más. Quedan fuera del cierre, en particular: `docs/automation/evidence/I-62-C1C3/` (C1-C3 y la nota de aislamiento de I-65); el resto de
`docs/automation/evidence/I-62-A6/`; el kit y los registros del lanzamiento de la revisión de la ronda 1 (salvo los tres archivos de arriba) y los
directorios de las revisiones anteriores; el repositorio de la entrega del fixture FX-U1, el bundle `MAP-INVALID/candidate-b.bundle` y los clones
temporales de los preflights; WORKFLOW, FOUNDATIONS, `state/I-62.yml`; las transcripciones y la memoria. Lo que A-7, sus preflights o la evidencia
citen de esos archivos no lo puedes comprobar: si una conclusión tuya depende de ello, decláralo en `KnownLimitations`.

## Lista de lectura: fuentes que cita A-7 §2 (L1-L17)

Las guardas de la ronda 2 (G3, en el resultado `run` custodiado) comprobaron que cada literal de A-7 §2 está en las líneas citadas del blob fijado
de su fuente (L16 con dos grupos de líneas de AUTOMATION_PLAN: 16.8 y 16.9). Úsala como lista de comprobación: antes de apoyarte en una cita de A-7
(una fila de §2, un bloque «> », una referencia «L…»), lee la fuente en esas líneas con Read y contrástala.

- L1 (A-7 §2, línea 84): `docs/initiatives/I-62-proposal-v14.md` (V14 §20.3.1 (`34ad80ea`); blob `34ad80ea`): L1079
- L2 (A-7 §2, línea 85): `docs/initiatives/I-62-proposal-v14.md` (V14 §20.3.1; blob `34ad80ea`): L1098-L1099
- L3 (A-7 §2, línea 86): `docs/initiatives/I-62-proposal-v14.md` (V14 B.9; blob `34ad80ea`): L2231
- L4 (A-7 §2, línea 87): `docs/initiatives/I-62-proposal-v14.md` (V14 B.9; blob `34ad80ea`): L2233
- L5 (A-7 §2, línea 88): `docs/initiatives/I-62-proposal-v14.md` (V14 B.9; blob `34ad80ea`): L2250-L2251
- L6 (A-7 §2, línea 89): `docs/AUTOMATION_PLAN.md` (AP 16.3 (`f525cb1e`); blob `f525cb1e`): L490
- L7 (A-7 §2, línea 90): `docs/AUTOMATION_PLAN.md` (AP 16.13 (Resolve, paso 3); blob `f525cb1e`): L823-L826
- L8 (A-7 §2, línea 91): `docs/AUTOMATION_PLAN.md` (AP 16.13 (Resolve, paso 2; Validate); blob `f525cb1e`): L821, L902-L908
- L9 (A-7 §2, línea 92): `docs/AUTOMATION_PLAN.md` (AP 16.24; blob `f525cb1e`): L1264, L1276, L1279-L1280
- L10 (A-7 §2, línea 93): `docs/automation/agent-execution/README.md` (RAE §15.2 I6 (`592dcfd4`); blob `592dcfd4`): L565
- L11 (A-7 §2, línea 94): `docs/initiatives/I-62-A-6.md` (A-6 A6-2, reglas 1 y 3 (`4bc919dd`); blob `4bc919dd`): L532-L535, L539-L541
- L12 (A-7 §2, línea 95): `docs/initiatives/I-62-A-6.md` (A-6 A6-3, regla 6 (b); blob `4bc919dd`): L600-L617
- L13 (A-7 §2, línea 96): `docs/initiatives/I-62-A-6.md` (A-6 A6-7, reglas 5 y 7; blob `4bc919dd`): L781-L783, L789-L794
- L14 (A-7 §2, línea 97): `docs/initiatives/I-62-A-6.md` (A-6 A6-9, regla 4, y tabla; blob `4bc919dd`): L841-L843, L851-L871
- L15 (A-7 §2, línea 98): `docs/automation/agent-execution/README.md` (RAE §10; blob `592dcfd4`): L219-L222
- L16 (A-7 §2, línea 99): `docs/AUTOMATION_PLAN.md` (AP 16.8 (RED de la cadena); AP 16.9; blob `f525cb1e`): L616-L617, L630, L635, L638, L639, L641, L643
- L17 (A-7 §2, línea 100): `docs/initiatives/I-62-A-6.md` (A-6 A6-3, regla 6 (b); A6-5, regla 2; blob `4bc919dd`): L600-L601, L673-L675
- A-7 §1 y §3.2: `docs/initiatives/I-62-A-6.md` (blob `4bc919dd`): L815-L816
- determinación de §91 que A-7 cita como origen: `docs/automation/evidence/I-62-A6/m7/s91/i6-determination.md` (blob `625e863d`), entera

## Reglas de lectura (el invocador audita después cada llamada)

**Regla de esta revisión.** Toda referencia `ruta:línea` o `ruta:inicio-fin` apunta solo a líneas que leíste con Read **en ese mismo archivo** (o, si
tiene copia fiel, en su copia fiel) y que te llegaron fielmente. Una cita de una fuente dentro de A-7 no es una lectura de la fuente: para citar
`docs/AUTOMATION_PLAN.md:617` tienes que haber leído con Read la línea 617 de ese archivo o de su copia fiel; si solo leíste la cita en A-7, cita la
línea de A-7.

Cada ruta se comprueba por su **efecto**: resuelta contra el directorio de trabajo, con `.` y `..` colapsados y los enlaces resueltos.

- **Read:**
  - usa la ruta absoluta, solo de archivos del cierre bajo `D:\r62-arch-a7r2` o de los archivos del run (`order.txt`, `a7-r1.md`, `prompt.md` y las copias
    fieles);
  - nunca leas con Read el original de un archivo que tiene copia fiel (lista de «Copias fieles»): lee la copia;
  - en los archivos de más de 60 000 bytes (`docs/automation/evidence/I-62-A7/r2/run_a7r2.py`, `docs/automation/evidence/I-62-A7/preflight.md`, `docs/automation/evidence/I-62-A7/run_a7.py`, `docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/output.json`, `docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/audit.json`, `docs/automation/evidence/I-62-A6/m7/s91/d1/nc2/nc2-result.json`, `docs/automation/evidence/I-62-A6/m7/bench/README.md`, `docs/automation/evidence/I-62-A6/r4/readable/r1probes/p7-derive.json`, `docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/selftest_m7.py`, `docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/tools_m7.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/test_bmin_facts.py`, `docs/automation/agent-execution/README.md`, `docs/initiatives/I-62-A-6.md`, `fiel__docs__automation__evidence__I-62-A6__m7__readings__readings.md.txt`, `fiel__docs__automation__evidence__I-62-A6__r4__r1probes__p7-derive.json.txt`, `fiel__docs__automation__evidence__I-62-F6__FX-02__bmin__accept__tests__test_bmin_accept.py.txt`, `fiel__docs__initiatives__I-62-proposal-v14.md.txt`, `fiel__docs__AUTOMATION_PLAN.md.txt`, `fiel__docs__initiatives__I-62-A-4.md.txt`, `fiel__docs__initiatives__I-62-A-3.md.txt`, `fiel__docs__initiatives__I-62-A-1.md.txt`, `fiel__docs__automation__decisions__I-62.md.txt`, `fiel__docs__automation__evidence__I-62-evidence.md.txt`), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** no la necesitas, porque las rutas están arriba. Si la usas, su alcance (`path` y patrón) solo puede contener archivos de la lista
  (`docs/automation/evidence/I-62-A7/r2/`, `docs/automation/evidence/I-62-A6/m7/s91/`, `docs/automation/evidence/I-62-A6/m7/s92/`,
  `docs/automation/evidence/I-62-A6/m7/bench/designations/`, el kit `a6m7-measure` y el directorio del paquete B-min están enteros en el cierre; el
  resto de `I-62-A6/` y de `I-62-architect-A-7/` no). Un patrón que alcance cualquier otro archivo cuenta como lectura fuera del cierre, aunque no
  leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, las áreas
    de trabajo `D:\r62-a7\*`, el fixture (`D:\r62-fixture\*`) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una referencia.
- **Archivos que Read no entrega fielmente** (BOM, líneas largas, controles, CR sueltos, NUL, UTF-8 inválido, caracteres no medidos): léelos solo por
  su copia fiel (arriba). Una línea con `\>` al final solo cuenta como entregada si lees también sus continuaciones `@n.2|…` del apéndice. Si, aun así,
  una línea no te llega fiel, no la cites y, si te hiciera falta, decláralo en `KnownLimitations`.
- **Contexto.** El cierre es grande: lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el
  auditor cuenta como un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ∖ ∩ ∪ « » ^ y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Referencias (`Evidence`, `ClauseRefs` y `DirectReads`)

- Una referencia es `ruta:línea` o `ruta:inicio-fin`: la ruta, relativa al clon para los archivos del cierre (p. ej.,
  `docs/initiatives/I-62-A-7.md:125-184`), y el nombre suelto para los archivos del run que pueden ser premisa (`order.txt:12`, `a7-r1.md:130`). Sin
  rutas absolutas ni barras invertidas. `prompt.md` nunca es referencia.
- Una copia fiel nunca es referencia: cita la ruta del **original** con el número de línea que muestra la copia (es el mismo). El invocador comprueba
  cada copia contra los bytes del original y acredita la línea del original solo si la copia te la entregó entera y fiel.
- Cita solo líneas que hayas leído enteras con Read en ese archivo (o en su copia fiel) y que te llegaron fielmente. El invocador comprueba que Read te
  entregó cada línea citada, también en `DirectReads` y en los `ClauseRefs` de los elementos anidados.
- **Números de línea exactos:** los que Read muestra a la izquierda. Una referencia desplazada, aunque sea en una línea, no se acredita. Antes de
  entregar, contrasta cada referencia con la salida de Read de la que sale.

## Qué decidir

Se pide el veredicto formal sobre la ronda 2 exacta de A-7 (LIFECYCLE §5 y §6), acotado a R1-R3 y a sus consecuencias (decisiones §94, punto 6).
Contesta con el texto normativo del cierre y con lo que leas.

- **`Verdict`:** `AGREED` (cero REQUIRED abiertos sobre la versión exacta), `CHANGES REQUIRED` (al menos un REQUIRED, corregible dentro del alcance de
  §94, punto 2), `REJECTED` (al menos un REQUIRED y la corrección no es admisible dentro de ese alcance: necesitaría otra disposición) o
  `BLOCKED — OWNER DECISION` (falta una autoridad reservada: una consecuencia OWNER-RESERVED que A-7 no identifica, o una que no esté dispuesta; A-7
  §7). Un REQUIRED impide el acuerdo hasta su disposición y corrección.
- **Hallazgos.** Un hallazgo es REQUIRED en los casos de LIFECYCLE §5 (activa M-01..M-08 sobre un elemento congelable, deja un invariante sin
  obligación de prueba, produce comportamiento observable distinto, o revela un elemento materialmente incorrecto, no ejecutable o no verificable);
  una precisión sin cambio de significado es OPTIONAL. Ids de esta revisión: REQUIRED `A62-A7R2-R1`, `A62-A7R2-R2`…; OPTIONAL `A62-A7R2-O1`…. Cada uno
  con la sección exacta de A-7 (`Section`), su evidencia (`Evidence`: al menos una referencia) y la corrección (`CorrectionRequired` en un REQUIRED,
  `SuggestedCorrection` en un OPTIONAL). Un REQUIRED trae además la autoridad o el invariante violado (`ViolatedInvariant`) o un contraejemplo concreto
  (`Counterexample`), al menos uno no vacío, y por qué importa (`WhyItMatters`). Un hallazgo es siempre sobre A-7. Los ids de la ronda 1 (`A62-A7-R1`…)
  solo se usan como identificadores de `Round1FindingResolution` y `OptionalDispositionConformance`, nunca como hallazgos nuevos.
- **`Round1FindingResolution`** (§94, punto 2; A-7 §3.6), una vez cada uno de `A62-A7-R1`, `A62-A7-R2` y `A62-A7-R3`: `Status` `CORRECTED` (la ronda 2
  corrige el hallazgo y no deja nada abierto; no cita ningún REQUIRED), `PARTIALLY_CORRECTED` o `NOT_CORRECTED` (citan el REQUIRED residual de esta
  revisión), con `Rationale`, `ClauseRefs` y `FindingIds`. Lo que pidió cada hallazgo está en `output.json` de la ronda 1 (`RequiredFindings`, L177-244) y en la tabla de
  §3.6; contrasta el texto nuevo con `a7-r1.md` y con las fuentes (A6-2, reglas 1 y 3, L532-535 y L539-541; A6-3, regla 6 (b), L600-617; A6-5,
  regla 2, L673-686; A6-7, reglas 1, 5 y 7, L759-770, L781-786 y L789-794; A6-9, regla 4, L841-843, y su tabla, L848-872; AUTOMATION_PLAN
  16.8 y 16.9).
- **`DeltaPurity`** (§94, punto 2: «Cambios normativos limitados a» R1-R3): en `Changes`, una vez cada cambio de la ronda 2, con `Assessment`
  `WITHIN_R1_R3` (el cambio está dentro de la corrección de R1, R2 o R3, o es descriptivo y no cambia el significado normativo; no cita ningún
  REQUIRED) u `OUTSIDE_R1_R3` (un cambio normativo que no exige R1-R3; cita un REQUIRED):
  - `R1_CATEGORY_C_ROLES`: la categoría (c) con seis papeles cerrados, un artefacto como máximo por papel, ruta y blob registrados, fidelidad por los
    bytes, ausencia con la clasificación de 16.9 y rechazo de lo no designado (A7-1, puntos 1 (c), 2 (c), 5 (c) y 6);
  - `R2_INVALIDATOR_RULE_AND_PROCEDURE`: el invalidador = A7-1 en el blob acordado más el procedimiento nombrado (A7-1, punto 7);
  - `R3_A62_RULE1_PRECISION`: la precisión de A6-2, regla 1, dentro del ámbito (A7-1, punto 1; cabecera; §1; §3.4; §4; §6);
  - `O1_M04_MOTIVE`, `O7_S31_MV6_MV7_CONTENT` y `O2_S35_REVERIFICATION`: los cambios descriptivos de §5 (M-03 y M-04), §3.1 y §3.5;
  - `P1_RED_PER_16_8`: la exigencia de RED según AUTOMATION_PLAN 16.8 en el punto 1 (c) y en la fila L16;
  - `P2_MEASUREMENT_WITHOUT_RELAY_FACTS`: «la invocación de medición … no tiene hechos del relevo» (punto 1 (c));
  - `P3_DERIVATION_AS_INVALIDATOR_PIECE`: la implementación de la derivación del cierre del aceptante entre las piezas del invalidador (punto 7);
  - `P4_RED_UNDETERMINED`: la exigencia de RED indeterminada en la regla 5 (c);
  y en el conjunto, `Status` `CONFIRMED` (todos `WITHIN_R1_R3`) o `NOT_CONFIRMED` (alguno `OUTSIDE_R1_R3`; cita un REQUIRED), con `Rationale`,
  `ClauseRefs` y `FindingIds` en cada cambio y en el conjunto.
- **`RegressionCheck`** (consecuencias de R1-R3 sobre lo que la ronda 1 encontró CONFORMS; `output.json` de la ronda 1, `TopicVerdicts`, L350-518), una vez cada
  tema: `T04_EFF_EFF1_B1_MAP_VALIDATION`, `T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE`, `T06_CLAUSEMAP_COVERAGE_RULE`, `T07_MULTI_CATEGORY_ENTRIES` y
  `T09_NO_UNAUTHORIZED_CHANGES`, con `Status` `NO_REGRESSION` (la ronda 2 conserva la conformidad; no cita ningún REQUIRED) o `REGRESSION` (cita un
  REQUIRED), `Rationale`, `ClauseRefs` y `FindingIds`.
- **`OptionalDispositionConformance`** (OPCIONAL; §94, punto 4): si lo das, una entrada por cada disposición propuesta que evalúes (`A62-A7-O1`..`O8`, sin
  repetir), con `Status` `CONFORMS`, `DOES_NOT_CONFORM` (cita un hallazgo) o `NOT_ASSESSED`, `Rationale`, `ClauseRefs` y `FindingIds`. Sin texto
  normativo adicional salvo que una contradicción REQUIRED lo exija (§94, punto 4).
- **`Materiality`** (LIFECYCLE §3; A-7 §5, L256-267): en `Triggers`, una vez cada disparador `M-01`..`M-08`, con `A7Declared` (lo que declara la columna
  «Activado» de A-7 §5: `yes` para **sí**, `no` para no) y `Assessment` (`AGREE` si coincides con esa declaración después de R1-R3; `DISAGREE` si no,
  y entonces cita un REQUIRED); `Rationale`, `ClauseRefs` y `FindingIds` en cada disparador y en el conjunto.
- **`OwnerReservedConsequence`** (LIFECYCLE §3, OWNER-RESERVED; A-7 §7, L281-286): `OwnerDecisionRequired` (true si identificas una consecuencia
  OWNER-RESERVED que A-7 no identifica o que no está dispuesta; false si no) y `UnidentifiedConsequences` (cada una; vacía con false), con `Rationale`,
  `ClauseRefs` y `FindingIds`. Las líneas del Owner constan (decisiones §78, línea 1572; A-6 §7, L1324 y L1325): ninguna cuenta aquí.
- **`RelatedImpacts`** (§94, punto 7, línea 1859: D-1 y las propuestas FC siguen preparadas, sin aplicar, y no se revisan otra vez):
  `ChangedByRound2` (true si R1-R3 cambian lo que la ronda 1 evaluó de D-1 o de las propuestas FC; false si no) y `Note` (con true, qué cambia), con
  `ClauseRefs` y `FindingIds`. No condiciona el veredicto salvo que revele un defecto de A-7 (entonces, un hallazgo sobre A-7).
- **`QuestionAnswers`** (A-7 §8, filas L306-308): `Q-A7R2-01`..`Q-A7R2-03`, una vez cada una: `Answer` (tu respuesta) y `Outcome` (`SATISFIED` si la
  respuesta no exige ningún cambio de A-7, `NOT_SATISFIED` si lo exige; un NOT_SATISFIED cita al menos un hallazgo), con `ClauseRefs` y `FindingIds`.
- **`DirectReads`:** `{Ref, Purpose}` de cada fuente que leíste directamente para comprobar una cita de A-7. Incluye siempre
  `docs/AUTOMATION_PLAN.md:616-617` (Paso 0) y, como mínimo recomendado, una entrada por fuente de la lista de A-7 §2 en la que te apoyes.
- **Además:**
  - tu modo de revisión, si revisor y autor son la misma persona y el contexto que el runtime te inyectó (`ReviewerMode`, `SamePersonAsAuthor`,
    `InjectedContextDeclaration`). Declara también el proveedor del revisor y del autor en `SamePersonAsAuthor`. Los identificadores de cuenta que el
    runtime te inyecte (correo, uuid de organización u otros) se declaran solo por su tipo, sin reproducir su valor;
  - la identidad revisada (`ReviewedCommit`, `ReviewedPath`, `ReviewedBlob`);
  - lo que leíste (`Reads`: «ruta: rangos») y la acción siguiente que recomiendas (`RecommendedNextAction`).

## Resultado

Entrega el resultado con **una** llamada a StructuredOutput, con este objeto. Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-a7r2-architect-review/0",
  "RunId": "R20261019T201030Z-d1bf", "InvocationId": "I20261019T201030Z-d1bf", "LogicalReviewRequestId": "L20261019T201030Z-d1bf", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62",
  "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-7.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora de A-7 y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "AP168DirectReadL616L617": "true | false | null", "OrderReadWhole": "true | false | null",
            "Decisions94DeclaresOrderBytesAndSha256": "true | false | null", "Decisions94AcceptsR1R3AndAuthorizesSecondCandidate": "true | false | null",
            "GuardsRunModeBaseHeadResult": "true | false | null", "PreflightSummaryCandidateAndTotals": "true | false | null",
            "Round1OutputIdentifiesRunAndFindings": "true | false | null", "Note": "..."},
  "DirectReads": [{"Ref": "docs/AUTOMATION_PLAN.md:616-617", "Purpose": "..."}],
  "Reads": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | REJECTED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A7R2-R1", "Section": "...", "ViolatedInvariant": "...", "Counterexample": "...", "Evidence": ["ruta:inicio-fin"],
                        "WhyItMatters": "...", "CorrectionRequired": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A7R2-O1", "Section": "...", "Note": "...", "Evidence": ["ruta:línea"], "SuggestedCorrection": "..."}],
  "Round1FindingResolution": [{"Round1Finding": "A62-A7-R1 | A62-A7-R2 | A62-A7-R3", "Status": "CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED",
                               "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "DeltaPurity": {"Status": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED",
                  "Changes": [{"Change": "R1_CATEGORY_C_ROLES | … | P4_RED_UNDETERMINED", "Assessment": "WITHIN_R1_R3 | OUTSIDE_R1_R3", "Rationale": "...",
                               "ClauseRefs": [], "FindingIds": []}],
                  "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "RegressionCheck": [{"Topic": "T04_EFF_EFF1_B1_MAP_VALIDATION | … | T09_NO_UNAUTHORIZED_CHANGES", "Status": "NO_REGRESSION | REGRESSION",
                       "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "OptionalDispositionConformance": [{"Round1Optional": "A62-A7-O1 | … | A62-A7-O8", "Status": "CONFORMS | DOES_NOT_CONFORM | NOT_ASSESSED",
                                      "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "Materiality": {"Triggers": [{"Trigger": "M-01 | … | M-08", "A7Declared": "yes | no", "Assessment": "AGREE | DISAGREE", "Rationale": "...",
                                "ClauseRefs": [], "FindingIds": []}],
                  "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "OwnerReservedConsequence": {"OwnerDecisionRequired": "true | false | null", "UnidentifiedConsequences": [], "Rationale": "...", "ClauseRefs": [],
                               "FindingIds": []},
  "RelatedImpacts": {"ChangedByRound2": "true | false | null", "Note": "...", "ClauseRefs": [], "FindingIds": []},
  "QuestionAnswers": [{"Question": "Q-A7R2-01 | Q-A7R2-02 | Q-A7R2-03", "Outcome": "SATISFIED | NOT_SATISFIED", "Answer": "...", "ClauseRefs": [],
                       "FindingIds": []}],
  "IfAgreed": {"ScopeLimitedToR1R3AndConsequences": null, "Round1FindingsCorrected": null, "DeltaLimitedToR1R3": null,
               "NoRegressionOfRound1Conformities": null, "MaterialityConfirmed": null, "NoUnidentifiedOwnerReservedConsequence": null,
               "NoUnauthorizedChanges": null, "RelatedImpactsUnchangedOrDeclared": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: cada campo lleva **tu** comprobación y **tu** evaluación. Donde la plantilla dice
  `"true | false | null"`, el valor es un booleano JSON (o `null`), no un texto. `OptionalDispositionConformance` es opcional: puedes omitirlo.
- `Step0`: las siete booleanas son lo que compruebas en el Paso 0 (la primera, la lectura directa de AUTOMATION_PLAN L616-617); `null` en lo que no
  compruebes.
- `DirectReads` incluye siempre AUTOMATION_PLAN L616-617, también si te detienes en el Paso 0.
- Cobertura, una vez cada elemento: `Round1FindingResolution`, R1, R2 y R3; `DeltaPurity.Changes`, los diez cambios; `RegressionCheck`, los cinco
  temas; `Materiality.Triggers`, M-01..M-08; `QuestionAnswers`, Q-A7R2-01..03. `OptionalDispositionConformance`, si lo das, sin ids repetidos.
- Todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`; los ids no se repiten.
- **Con un veredicto** (todo salvo NOT_ACCREDITED):
  - las siete booleanas de `Step0` en `true`; `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob` = el commit, la ruta y el blob de arriba;
  - `A7Declared` de cada disparador = lo que declara A-7 §5;
  - al menos una `ClauseRef` en cada elemento de `Round1FindingResolution`, `DeltaPurity.Changes`, `RegressionCheck`, `Materiality.Triggers` y
    `QuestionAnswers`, en cada entrada de `OptionalDispositionConformance` que no sea `NOT_ASSESSED`, y en `DeltaPurity`, `Materiality`,
    `OwnerReservedConsequence` y `RelatedImpacts`;
  - ningún `NOT_DETERMINED` (`DeltaPurity.Status`) y ningún `null` en `OwnerDecisionRequired` ni en `ChangedByRound2`;
  - todo REQUIRED citado en `FindingIds` por al menos un elemento evaluado.
- **Con AGREED:** cero REQUIRED; R1, R2 y R3 en CORRECTED; `DeltaPurity` CONFIRMED con los diez cambios `WITHIN_R1_R3`; los cinco temas en
  NO_REGRESSION; los ocho disparadores en AGREE; `OwnerDecisionRequired` = false; y los nueve campos de `IfAgreed` en `true`. En cualquier otro caso,
  `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED o REJECTED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true. Con
  `OwnerDecisionRequired` = true, `UnidentifiedConsequences` no vacía. Con `ChangedByRound2` = true, `Note` no vacía.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `Round1FindingResolution`, `DeltaPurity.Changes`, `RegressionCheck`, `Materiality.Triggers`, `QuestionAnswers` y las listas de hallazgos pueden ir
    vacías.
  - Lo no evaluado va así: `NOT_DETERMINED` en `DeltaPurity.Status`; `null` en `OwnerDecisionRequired`, en `ChangedByRound2`, en `IfAgreed` y en las
    booleanas de `Step0` que no comprobaste; `ReviewedCommit` y `ReviewedBlob` con lo observado o en `null`; los `Rationale`, «no evaluado».
  - `NOT_DETERMINED` y `null` en esos campos solo valen con NOT_ACCREDITED.