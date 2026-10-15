I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA RONDA 4 DE LA ENMIENDA A-6 (instrumento de hechos fijado por blob para el Controller de FX-02 en F6, B-min), acotada a A62-A6R3-R1 y a sus consecuencias directas, preparada según la disposición del Coordinator §85, punto 6, y la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento.

Eres el ARCHITECT de esta revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-6 y de sus cuatro rondas, de los agentes que construyeron las sondas de las rondas 3 y 4 (incluida la implementación independiente de `r4/r1probes/indep/`), de quienes escribieron el paquete B-min, del Coordinator, del dictamen de materialidad de B-min y de las revisiones de las rondas 1 a 3. Revisas en solo lectura la versión exacta de la ronda 4 de A-6 y devuelves un único veredicto formal (LIFECYCLE §5 y §6). No redactas texto normativo de sustitución, no implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261014T180556Z-5e02
InvocationId           = I20261014T180556Z-5e02
LogicalReviewRequestId = L20261014T180556Z-5e02   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-a6r4   (tu directorio de trabajo; HEAD separado en el commit, sin ramas locales; sin remoto)
Commit                 = 088b3227a22ef6ab28b7ba43d5aa90de68d5b08f
Publicación ronda 4    = d66f7f4b6a7031938588378b6eef7f1c3d4e4cf7   (A-6 ronda 4, PROPUESTA; padre ceb8597796b756f199e1c1b153f02e95b820b44e, con la A-6 de la ronda 3 y decisiones §85)
Guardas `run`          = 088b3227a22ef6ab28b7ba43d5aa90de68d5b08f   (custodia del resultado `run` de las guardas de la ronda 4 sobre la publicación)
Objeto                 = docs/initiatives/I-62-A-6.md   blob 4bc919dd4768ab81abc74b70d3a4a3821c07feb5   (151 865 bytes, 1 447 líneas)
Ronda 3                = blob e62f27b8b1329f1b9cfa69a707feb31d22bad1d1, en el run como D:\r62-arch-a6r4-run\a6-r3.md (135 665 bytes, 1 320 líneas)
Revisión de la ronda 3 = R20261013T183701Z-9f80: docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/output.json (blob 691bbc8f…)
Paquete del Architect  = ninguno (este texto hace de paquete)
Orden vigente          = D:\r62-arch-a6r4-run\order.txt   (disposición §85 del Coordinator; 2 805 bytes, 68 líneas, SHA-256 d74e2aa1ce5eacc7dd6323cf3f3acec37fd6eb25f905ca4f2479dbca0cde5232)
```

**Recibo.** El invocador comprobó que el objeto tiene el blob `4bc919dd…` en la publicación, en la custodia de las guardas y en el commit de arriba, y
que el padre de la publicación tiene el blob de la ronda 3 (`e62f27b8…`); que `docs/automation/evidence/I-62-A6/r4/` no cambia después de la
publicación salvo por el añadido del resultado `run`; que decisiones solo crece por añadido desde el registro de §85 y que §85 es su última sección
en el commit del clon; que el paquete B-min, `r2/`, `r2/independence/`, `r3/` y el directorio de la revisión de la ronda 3 conservan sus árboles y
blobs; que `a6-r3.md` es, byte a byte, el blob `e62f27b8…`; y que las guardas custodiadas de la ronda 4 reproducen sus resultados (`run` sobre la
publicación y `self-test`).

**Condición de lanzamiento (compuerta 11).** Decisiones §85, punto 6 (registro en la línea 1696; `order.txt`, líneas 46-49), ordena preparar un kit de
revisión acotado a R1 y a sus consecuencias directas y no lanzar la revisión formal de la ronda 4 «hasta nueva disposición del Coordinator». Esta
invocación existe porque esa disposición nueva, posterior a §85, se fijó en el kit (su número, su encabezado y el SHA-256 de su texto) y el lanzador
comprobó inmediatamente antes de lanzarla, en el registro de decisiones de la rama de I-62: esa sección, una sola vez, con el encabezado y el SHA-256
fijados y el blob completo del objeto; ninguna sección posterior a ella; la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de §78 como línea completa;
la línea `CLAUDE-CLI-I62 = A (…)` vigente y el binario medido igual al del cierre; y A-6 en la HEAD de la rama con el blob de arriba. Esa disposición
es posterior al commit del clon y no está en el cierre. No juzgas esa autorización ni la determinación del Owner; juzgas A-6.

**Objeto y revisión.** Es la cuarta revisión formal de A-6. La revisión de la ronda 3 (R20261013T183701Z-9f80), acreditada por su auditor, terminó en
CHANGES REQUIRED con un REQUIRED (A62-A6R3-R1) y siete OPTIONAL (A62-A6R3-O1..O7), literales en su salida. Decisiones §85 (encabezado en la línea
1684): su punto 1 (línea 1691) registra la revisión de la ronda 3 como ACCREDITED, el dictamen CHANGES REQUIRED, A62-A6R3-R1 como ACCEPTED
REQUIRED y A62-A6R2-R2 como CORRECTED; su punto 2 (línea 1692) autoriza solo preparar la ronda 4 para resolver A62-A6R3-R1, con el objetivo, la
superficie preferida y el límite sobre A6-9, regla 4, que enuncia; su punto 3 (línea 1693) fija las pruebas obligatorias; su punto 4 (línea 1694)
dispone los opcionales O1-O7 y prohíbe por ellos cambios normativos fuera de A6-3, regla 6 (b), sin autorización posterior; sus puntos 5, 7 y 8
(líneas 1695, 1697 y 1698) fijan las guardas, los límites y la entrega; y su punto 6, la preparación de este kit. Se pide una revisión
independiente y ACOTADA sobre:

- el cierre de A62-A6R3-R1;
- si A6-3, regla 6 (b), con sus apartados (i)-(v), aplica la tabla de derivación de forma total y mecánica, sin paso de juicio, en N4 (toda forma de
  entrega ausente), en nc1 (commit inexistente) y con campos ausentes, `null` o vacíos, y si introduce o sustituye algún insumo;
- su compatibilidad con A6-9, regla 4, y con su tabla de derivación: si alguna contradicción exigiría editar A6-9;
- la coherencia de N4 y nc1;
- la adecuación de la evidencia de `r4/r1probes/`;
- la pureza del delta: que solo A6-3, regla 6 (b), cambie normativamente;
- el tratamiento de O1-O7 frente a lo que ordena decisiones §85, punto 4;
- las preguntas de la ronda 4 (Q-A6R4-01..04).

Lo que dependa de una sesión real o de una disposición futura (la medición de A6-7, la prueba previa de A6-5, regla 6, la disposición sobre el cierre
de VERIFY) se mantiene como precondición con fallo cerrado: no lo das por demostrado, y lo que A-6 deja como precondición lo juzgas como
precondición (si está definida de forma verificable y si su falta produce un fallo cerrado). Sin acreditación excepcional: si no puedes cumplir un
punto con lecturas fieles, no lo des por cumplido.

Los demás aspectos de A-6 (los deltas y las reglas que la ronda 4 declara no cambiar, la materialidad, la separabilidad, la compatibilidad con
A-1..A-5, las superficies, D.5 y los riesgos G1-G6) no se te piden en esta revisión acotada; la ronda 4 declara cuáles no cambia (§3.18, L1109-1175) y sus
guardas lo comprueban (G5). No se te prohíbe señalar otro defecto material que encuentres en cualquier sección de A-6: hazlo como hallazgo.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git`, las guardas, el instrumento de B-min, su aceptante, las sondas de `r4/` ni la implementación independiente.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, sin ramas ni remoto, árbol limpio, los blobs del cierre y el
  SHA-256 de los archivos del run. El auditor la vuelve a verificar después. En `Step0` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. **Lectura directa de V14 (obligatoria).** Lee con Read `D:\r62-arch-a6r4\docs\initiatives\I-62-proposal-v14.md` con `offset` 2325 y
   `limit` 18: incluye las líneas 2333-2334 (Anexo C, «Esperado independiente»). Tiene que ser una llamada a Read sobre ese archivo:
   la cita que hace A-6 de esas líneas no es una lectura de V14. Regístrala en `DirectReads` (`docs/initiatives/I-62-proposal-v14.md:2333-2334`).
2. Lee entera, con Read, `D:\r62-arch-a6r4-run\order.txt` (la orden vigente, 68 líneas): punto 1 (líneas 3-9), punto 2 (10-14), punto 3 (15-27), punto 4
   (28-35) y punto 6 (46-49).
3. Lee con Read las líneas 1684-1698 de `D:\r62-arch-a6r4\docs\automation\decisions\I-62.md` (decisiones §85). Comprueba que la línea 1687 declara
   el cuerpo pegado de §85 con 2 805 bytes y el SHA-256 de arriba, y que los puntos 1-8 de §85 (líneas 1691-1698) registran lo que dice
   `order.txt` (con la redacción del registro). En §85, punto 1 (línea 1691): revisión de la ronda 3 ACCREDITED, Architect CHANGES REQUIRED,
   A62-A6R3-R1 ACCEPTED REQUIRED y A62-A6R2-R2 CORRECTED.
4. Lee entero, con Read, `D:\r62-arch-a6r4\docs\automation\evidence\I-62-A6\r4\a6r4-guards-result-run.json` (1704 líneas): las líneas 2-4 deben dar `Mode` RUN, `Base` = el padre de la publicación
   (`ceb8597796b756f199e1c1b153f02e95b820b44e`) y `Head` = la publicación (`d66f7f4b6a7031938588378b6eef7f1c3d4e4cf7`), y la línea 1703 el `Result` de conjunto PASS.
5. Lee con Read las líneas 8204-8229 de `D:\r62-arch-a6r4\docs\automation\evidence\I-62-A6\r4\a6r4-selftest.json` (`Summary`, `Result`, `Mode`, `Repo`, `ToolBlob` y `A6TextBlob`): `Result` PASS y
   `A6TextBlob` = el blob del objeto.
6. Lee con Read, en `D:\r62-arch-a6r4\docs\automation\evidence\I-62-architect-A-6r3\R20261013T183701Z-9f80\output.json`, las líneas 2-176 (RunId
   R20261013T183701Z-9f80, `ReviewedBlob` = `e62f27b8…`, `Verdict` CHANGES REQUIRED) y los hallazgos: REQUIRED R1 178-209; OPTIONAL O1 212-224; O2 225-236; O3 237-247; O4 248-258; O5 259-270; O6 271-282; O7 283-292;
   y su `NegativeCaseCoherence` (líneas 336-391).

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`D:\r62-arch-a6r4-run\`):
- `order.txt`: la orden vigente (disposición §85; 2 805 bytes, 68 líneas). Su SHA-256 es el que declara decisiones §85 en el commit;
- `a6-r3.md`: el texto de A-6 de la ronda 3 (blob `e62f27b8…`), byte a byte: la revisión de la ronda 3 cita sus líneas, y la pureza del delta se
  compara con él. Más de 60 000 bytes: por tramos de 300 líneas como máximo. Puede ser referencia (`a6-r3.md:<línea>`);
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es referencia.

Todas las rutas que siguen son relativas al clon `D:\r62-arch-a6r4` y están en el commit de arriba.

**Objeto**, entero (más de 60 000 bytes: por tramos de 300 líneas como máximo): `docs/initiatives/I-62-A-6.md`: cabecera 1-69; 1. Alcance 71-99; 2. Cláusulas anteriores y fuentes 101-406; 3. Delta 408-1175; 3.1 Motivo 410-442; 3.2 Correspondencia con los ocho puntos del dictamen 444-458; 3.3 Identidad por blob de los instrumentos e insumos canónicos 460-481; 3.4 A6-1 483-522; 3.5 A6-2 524-544; 3.6 A6-3 546-628; 3.7 A6-4 630-656; 3.8 A6-5 658-733; 3.9 A6-6 735-749; 3.10 A6-7 751-794; 3.11 A6-8 796-816; 3.12 A6-9 818-1001; 3.13 Prohibiciones 1003-1024; 3.14 Observaciones del dictamen 1026-1036; 3.15 Efectos sobre D.5 y la Owner Validation 1038-1048; 3.16 Correcciones de la ronda 2 1050-1064; 3.17 Correcciones de la ronda 3 1066-1107; 3.18 Corrección de la ronda 4 1109-1175; 4. Semántica antes y después 1177-1202; 5. Materialidad 1204-1219; 6. No cambia producción; compatibilidad con A-1..A-5 1221-1287; 6.1 Diff permitido y superficies prohibidas 1223-1242; 6.2 Compatibilidad con A-1..A-5 1244-1260; 6.3 Guardas mecánicas y preflight 1262-1287; 7. Consecuencias reservadas al Owner 1289-1326; 8. Disposición de las rondas 1 a 3 y preguntas de la ronda 4 1328-1372; 9. Separabilidad 1374-1383; 10. Evidencia 1385-1413; 11. Estado de revisión 1415-1425; Anexo 1427-1447. Lo que la ronda 4 dice
cambiar normativamente: A6-3, regla 6 (b) (§3.6, L546-628; regla 6, L590-626; apartado (b), L600-617: encabezado L600-602, apartados
(i) L603-605; (ii) L606-609; (iii) L610-611; (iv) L612-613; (v) L614-615, frase final L616-617). Lo que A-6 declara descriptivo en la ronda 4: §3.18 (L1109-1175: tabla L1117-1126, filas R1 L1119; O1 L1120; O2 L1121; O3 L1122; O4 L1123; O5 L1124; O6 L1125; O7 L1126;
párrafos cambio normativo L1111-1115; N4 y nc1 L1128-1131; relación con A6-9 L1157-1162; prueba previa L1164-1168; G1-G6 L1170; kit L1172-1175; viñetas derivación L1133-1142; cierre independiente L1143-1145; aceptante fijado L1146-1149; insumos y sustituciones L1150-1155), las celdas de §3.17 que remiten a la ronda 4 (§3.17, L1066-1107; tabla L1073-1085, filas
R1 (a) L1075; R1 (b) L1076; R1 (c) L1077; R1 (d) L1078), la fila de §4 de N4 y nc1 (§4, L1177-1202; fila L1191), el párrafo «Ronda 4» de §7 (L1316-1317) y §8 (L1328-1372: estado de
las preguntas de la ronda 3, filas L1360-1363; preguntas de la ronda 4, filas L1369-1372). La tabla de derivación que la regla 6 (b) aplica es la de A6-9, regla
4 (L841-843), en §3.12 (L818-1001): tabla L848-872 y su párrafo L874-879.

**Evidencia de la ronda 4** (219 archivos, enteros; evidencia de apoyo, no autoridad: derivación del cierre con la regla corregida, implementación
independiente, sustituciones e insumos añadidos o inventados, pruebas del aceptante fijado, guardas, autoprueba, resultado `run`, negativos y
preflight). Los resultados JSON de una sola línea (más de 2 000 caracteres) tienen una **copia legible** en `docs/automation/evidence/I-62-A6/r4/readable/`
(misma ruta relativa; líneas de 1 900 caracteres como máximo; `readable/MANIFEST.json` fija, para cada copia, el archivo canónico, su blob y la
transformación): lee la copia legible y **cita la ruta de la copia**, no la del JSON de una sola línea:

```text
docs/automation/evidence/I-62-A6/r4/a6r4-guards-result-preview.json                          blob 9ad848938355f1002d291971e9ce60ed2b6b9c73  25 693 bytes  1 486 líneas
docs/automation/evidence/I-62-A6/r4/a6r4-guards-result-run.json                              blob b59145f044a8a6695c3616a57901ad1e3b8ae1d1  44 902 bytes  1 704 líneas
docs/automation/evidence/I-62-A6/r4/a6r4-guards.diff                                         blob c4eeeecda61f0f656b3de9afc0db089526755dbf  89 253 bytes  1 153 líneas
docs/automation/evidence/I-62-A6/r4/a6r4-guards.py                                           blob 56b59c5004035ef9ed3bcd30f1ffff33620e3be3  235 791 bytes  3 503 líneas
docs/automation/evidence/I-62-A6/r4/a6r4-selftest.json                                       blob e255e57df361fc00d263d08a87bc98070f431d5c  227 626 bytes  8 230 líneas
docs/automation/evidence/I-62-A6/r4/a6r4-sources-for-reviewer.json                           blob e6c45879d9b310df753f5fa0f04b89d6978100bc  18 061 bytes  1 204 líneas
docs/automation/evidence/I-62-A6/r4/adapt_guards_r4.py                                       blob 87af4da59a0f6178367a15e085d6a395b207a772  65 845 bytes  807 líneas
docs/automation/evidence/I-62-A6/r4/adapt_neg_r4.py                                          blob 0c56290097e85dcea38c29975495aad544c6635f  16 236 bytes  140 líneas
docs/automation/evidence/I-62-A6/r4/make_candidate_commit_a6r4.py                            blob 8d9e2032ab2f9293517e7caf8598cc8022bb43df  7 227 bytes  109 líneas
docs/automation/evidence/I-62-A6/r4/neg-fixtures/I-62-A-6.r4.draft.md                        blob 562fad5592fe08fd68baa80dc0d90641d4db6f40  151 596 bytes  1 447 líneas
docs/automation/evidence/I-62-A6/r4/neg-fixtures/a6r4-selftest.draft.json                    blob 5ddaae474ef4f91cd28fd25905b84c07764914dd  227 690 bytes  8 230 líneas
docs/automation/evidence/I-62-A6/r4/neg_controls_a6r4.json                                   blob 1c0716071acce236b3a8eb970fd7af87717ff491  21 623 bytes  906 líneas
docs/automation/evidence/I-62-A6/r4/neg_controls_a6r4.py                                     blob 5ab0f1a36bfbfa047aa1ab261ca467193a1911ab  18 911 bytes  273 líneas
docs/automation/evidence/I-62-A6/r4/preflight.md                                             blob 9c59baa6aa606ef6367f2b688a77933aeba1d963  7 178 bytes  86 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/README.md                                       blob cdcce82d0706ef48d60cf5e1fd8ab05252124b5f  27 073 bytes  174 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/derive_r4.py                                    blob a6f87be801f9dbf33144e20ef5b6aaee6b11eab8  13 783 bytes  364 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/README.md                                 blob 3808a3053bc74b8392ca47fe413f676d252cc331  8 892 bytes  92 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/SPEC.md                                   blob c809e6d7a0c2b6792a17cebbcc7ef3acb7e43ee8  10 489 bytes  144 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/derive_indep.py                           blob 806a2938d75b7719ed2f836f5fdd66a4c7c386e9  22 777 bytes  587 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/make_synth.py                             blob da6de24bc1171553ed7834c9b3c8630a1a084113  14 511 bytes  280 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/run_synth.log                         blob 55600f18932e82f0fc7f5101f4e9b942862a55a0  713 bytes  25 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/sample.json                           blob d2a49c7f6114d104a0091aaee6a3971b5dc43cf6  14 781 bytes  452 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-all_present.json                  blob 91d17210bf14a89117d530676cb37aed80f98a9b  4 784 bytes  202 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-d_absent_unpinned.json            blob 3b0f2c60ed67581b8541935a5be27e38c8c2ad24  3 505 bytes  161 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-d_branch_null.json                blob 5103e75de9f858d228549fec5a02789c3eb510b1  4 827 bytes  206 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_absent_unpinned.json            blob 79cbc31e9d177dd37d3b0ad284693bdafe50d217  4 441 bytes  197 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_blob_mismatch.json              blob 095485c2ca310d1548067e32bf2bf2b4268a54e4  4 505 bytes  198 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_bom.json                        blob 095485c2ca310d1548067e32bf2bf2b4268a54e4  4 505 bytes  198 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_current_39hex.json              blob b4ef153da1b4b41e876304ab5bd6bbbff7ee7dc7  4 437 bytes  190 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_current_empty.json              blob 43f25f9c1bbe809dc71989cb20ee2a891dd33697  4 436 bytes  190 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_current_missing.json            blob e8df99b7349ca131545d74f126819c4667116118  4 438 bytes  190 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_current_nonexistent.json        blob 669973443a3d5bfe832a91e0c9e01bc50090a543  4 437 bytes  190 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_current_null.json               blob a437b3e9bfe80d4fadcfb7cc1921669393cc60e5  4 435 bytes  190 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_current_tree.json               blob 1e101176284e8957ac8fb2eeb1ee2bb335578b0a  4 437 bytes  190 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_current_upper.json              blob c1803dd1d89f30981cc0251a8d28430c6cf8802e  4 437 bytes  190 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_custody_missing.json            blob 095485c2ca310d1548067e32bf2bf2b4268a54e4  4 505 bytes  198 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_dupkey.json                     blob 095485c2ca310d1548067e32bf2bf2b4268a54e4  4 505 bytes  198 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_invalid_json.json               blob 095485c2ca310d1548067e32bf2bf2b4268a54e4  4 505 bytes  198 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_not_object.json                 blob 095485c2ca310d1548067e32bf2bf2b4268a54e4  4 505 bytes  198 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-h_red_null.json                   blob c00b1bc2af2a7e05d44e6c80f5a0f6255cb42db2  4 845 bytes  207 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-k_absent_unpinned.json            blob 4d9e7254cda0a2877b605942e9d53040d332a7a0  4 848 bytes  212 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-k_authorities_empty.json          blob 2de1fea96c1d18c3c4755e68a35623e12f159fd2  4 636 bytes  196 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-k_invalid_json.json               blob 5f78ab67b0079ef1a199271e61af68ea2a1331a5  4 880 bytes  213 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-mergebase_none.json               blob 1f33134c7c17bfbc681cde60f0116afae32fbd6c  4 823 bytes  204 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-target_nonexistent.json           blob 7a3f029b48b06c97cf9281bd0f10eff5600e8bdf  4 854 bytes  207 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/out/syn-trx_unverifiable.json.stderr.txt  blob 92fc643e39e5580b72c564fdd4af84df8b8875df  79 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/run_synth.py                              blob d1cbdcc79245ed26212177708393ca4ce80ef515  5 224 bytes  106 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-all_present.json                 blob e2b745c9d217a85a40ff1b52c0bd453e5d5cd81c  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-d_absent_unpinned.json           blob 706050af7a0f1743b5df337444b24b50273ab8cf  1 182 bytes  57 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-d_branch_null.json               blob 28ab5a6d3bb396f33d235c3602fb2f1f0b854844  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_absent_unpinned.json           blob d2630544442ff0853c8a09dc9e84c49dd7fad651  1 174 bytes  57 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_blob_mismatch.json             blob 2576f5668753cbeb0e888bf07cd962826c8aaaba  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_bom.json                       blob a1728e1931b173ebfb78a0de87aa8ab7c9ec7b06  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_current_39hex.json             blob a5da5542b3c9419a6d75ea33f39bfdb6282d2af1  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_current_empty.json             blob d530f0bd2491608be89769a9fb305aa123ddd220  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_current_missing.json           blob abba45d328e4db9dac787c16770f3923bbf1d7af  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_current_nonexistent.json       blob f8abcb823f97f86411c23215f44484dea19ba6c8  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_current_null.json              blob b0199a03301c75df4a3f25062e08c36e68a97109  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_current_tree.json              blob c6ac8773b299186ac24087f98db1380d82ebdc8c  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_current_upper.json             blob 1f31e63c2d2271fc520b2764f905460f21482ef5  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_custody_missing.json           blob e2b745c9d217a85a40ff1b52c0bd453e5d5cd81c  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_dupkey.json                    blob 9247b641a15eb20acd93f1d62b9b36a7253c1f7c  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_invalid_json.json              blob 2c8d7d63fd99a4a1e882b84afcbe1cb7e00a507e  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_not_object.json                blob 959ec0585bad47e1a63f3bc7d2d9c6ecb60c2730  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-h_red_null.json                  blob b64a9695695863ce1883292c7b8ed7a59fab8f00  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-k_absent_unpinned.json           blob b8a59c860e13772853014a3bc0473305f2cefedd  1 206 bytes  57 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-k_authorities_empty.json         blob cfd5bc6d181c2b98d04021f9b5968774ce0af50b  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-k_invalid_json.json              blob b972837e3c4ab8c0467455ca6ed409390824fc8b  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-mergebase_none.json              blob 4218d1300c664a32bd968c263bb60c44db4362d0  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-target_nonexistent.json          blob e2b745c9d217a85a40ff1b52c0bd453e5d5cd81c  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/E-trx_unverifiable.json            blob b4426ec5e985f462621764ebef71df98e9c6fbe1  1 309 bytes  61 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/all_present-D.json                 blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/all_present-H.json                 blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/d_absent_unpinned-H.json           blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/d_branch_null-D.json               blob c7a6e89f4473955a1666dd40e1d4af0f55b503ec  280 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/d_branch_null-H.json               blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/expect.json                        blob 93387f98ab3067b290fff4aa388cd8988392f1a3  17 288 bytes  556 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_absent_unpinned-D.json           blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_blob_mismatch-D.json             blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_blob_mismatch-H.json             blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_bom-D.json                       blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_bom-H.json                       blob 24127454a358f370006e9d4a5aeaaf69b19f084a  195 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_39hex-D.json             blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_39hex-H.json             blob 9f852310553e0696e462baeabcefabe5b738269b  191 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_empty-D.json             blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_empty-H.json             blob 0d3f83795acc3cd264f0c7d3427e9657462427a2  152 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_missing-D.json           blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_missing-H.json           blob 8bc7aac9c01b4b5ddc1bd802872b4bfefeb6f89b  133 bytes  5 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_nonexistent-D.json       blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_nonexistent-H.json       blob 1e1d74b08409ebf121a1d0c42ee20d089f759000  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_null-D.json              blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_null-H.json              blob 4d34a158076cf5a369e46c3a151f43cd646df76d  154 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_tree-D.json              blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_tree-H.json              blob d0dafc6e2da3c52966c1d370e0d09c27cd317c7e  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_upper-D.json             blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_current_upper-H.json             blob f4510b5a3e3ee757c866d1055e53a021119a18c4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_custody_missing-D.json           blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_custody_missing-H.json           blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_dupkey-D.json                    blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_dupkey-H.json                    blob 7e5476fc81ba7bd4060a40757ec567bec751cb57  197 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_invalid_json-D.json              blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_invalid_json-H.json              blob 2f7a2c6068f0a3f8e63df5fc27e34da0c89ab848  33 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_not_object-D.json                blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_not_object-H.json                blob 44e2ace7e54d8deb5052e43eb3792aceae144ac8  7 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_red_null-D.json                  blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/h_red_null-H.json                  blob 1c58a0832a77789e665f062d6ece36bf4ed15bb7  154 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/k_absent_unpinned-D.json           blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/k_absent_unpinned-H.json           blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/k_authorities_empty-D.json         blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/k_authorities_empty-H.json         blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/k_authorities_empty-K.json         blob 6ed7279a4ddd7044db877675ef04c778dc5fa432  178 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/k_invalid_json-D.json              blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/k_invalid_json-H.json              blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/k_invalid_json-K.json              blob 6b7a9f4e0c2451d86e695ebd335960855f2b2b8c  9 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/mergebase_none-D.json              blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/mergebase_none-H.json              blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/mergebase_none-K.json              blob 102fb3e6b59e7c74ce1261147d601e56a70ee48e  253 bytes  13 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/t.trx                              blob d75ee1c9ce8719585023bed5e4808fa10a3964b0  32 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/target_nonexistent-D.json          blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/target_nonexistent-H.json          blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/trx_unverifiable-D.json            blob 5480d3df6be1867d2518e16de28b8a0345c8884c  284 bytes  8 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/synvec/trx_unverifiable-H.json            blob 7cbd10b790540ab7c1776deba7c138f8ed4775f4  192 bytes  6 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/sample.json                       blob 02387d63280662493978167d3c4c3ecac0f43df8  1 135 bytes  14 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-all_present.json              blob 61c41d7cb5e5c0ca0549d5aeae8e93175095479b  825 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-d_absent_unpinned.json        blob bb89b60bbad1c607bd48842a7b187bc5a4e21695  735 bytes  25 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-d_branch_null.json            blob ab86fde6ec3c0a61ca0d469e173a3335c8b83521  831 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_absent_unpinned.json        blob 76cfe218cf79100f340c1320b60258f4ab4ae896  727 bytes  25 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_blob_mismatch.json          blob 3fa3eef2b8e09edaf8b600b5fc6fe719f37d8a49  837 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_bom.json                    blob 70a0e61bd31496cd78aa7905cd410c69c22bf9f1  807 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_current_39hex.json          blob 193ea6b8fc383b281cfd123e2cf186d46badbe58  837 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_current_empty.json          blob bddb6ab269aa38542394e3baa8d51fb3b6dad814  837 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_current_missing.json        blob 993e3ffe8fe81eb8f0b74edc81208034bbdc1927  843 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_current_nonexistent.json    blob 01a6b1595a2f87af6b55388266aaea194ac6d7bb  855 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_current_null.json           blob ef14ecec3f799552ce60e347d2aaad0ae5f3e6f2  834 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_current_tree.json           blob d55ba494f3fc69b28fbc73e9a2ed0bfa3056aa1c  834 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_current_upper.json          blob 0482060c05d35be6867f7b20f1c3150342c885f4  837 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_custody_missing.json        blob b9fb01524535ef6dcb2a5306fb196e341960c4ae  838 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_dupkey.json                 blob c92639ed4cedffa116440c5b9dad23b984d8f1c6  816 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_invalid_json.json           blob d0f1e528507110127d4505e555fee845ca441dcf  834 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_not_object.json             blob a71683ee9004537e6d390231bb209918fa45a534  828 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-h_red_null.json               blob 42ce5079e64b4cbbf62749cab89af0facad3b7ca  822 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-k_absent_unpinned.json        blob fd443be675ec8048480a02c4d1caeffec8dd8a07  843 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-k_authorities_empty.json      blob 8d59e8a32da4a8d7f3d4c9afc90ed87faf5d61c5  935 bytes  27 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-k_invalid_json.json           blob 6c6380cefb6295cbdd478a76cf41f49595050c8b  915 bytes  27 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-mergebase_none.json           blob c7d581b31525869349efce1d8956a16ba4a2c63e  915 bytes  27 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-target_nonexistent.json       blob 366a479d97c61a87ef30726e9b8c02f9f09735ae  846 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/indep/vectors/syn-trx_unverifiable.json         blob b03ed135affdb4c49ee5c33a48a8df2f2d6a5038  840 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-N4-ABSENT.closure.json          blob 15f4442894b4dc4568083148d5195def5de2d6e8  12 388 bytes  382 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-N4-ABSENT.derivation.json       blob 0a92f943cd0035e0a04ea2e27bae3f85eb38e2a4  1 048 bytes  53 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-N4-ABSENT.vector.json           blob 3eb482c1f29ed9ba0fbfa81316853ebf671ebb80  1 093 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-N4-STALE.closure.json           blob ec02567568dee14ee76673d668b305dc09a1f552  12 474 bytes  383 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-N4-STALE.derivation.json        blob 0ef853a6b207ebedad86e455e5cca2f4a9c4febc  1 040 bytes  53 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-NC1.closure.json                blob d07d8ce8aecb6b6420127a3728e1cb7a377784fc  14 248 bytes  444 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-NC1.derivation.json             blob 6d0c9f63810e92945c9d754af38f6af3d838c75a  786 bytes  38 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-NC1.vector.json                 blob 0e21b4931d765730007a757e4e996d95248c8ff0  1 073 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-R2.closure.json                 blob 08e86a5e66f708fc9e5d84d58575d1566cc6c54a  14 240 bytes  444 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-R2.vector.json                  blob 9883da055cbda2c6f819892aeaaf702d34a451d7  1 080 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-V.closure.json                  blob 94288f0e6f4811b40d2ecff5bcfcd1acc81bd7b0  14 240 bytes  444 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-V.derivation.json               blob 589994b82408c0cfa5c94224987a0e3bfd162824  582 bytes  28 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/D-V.vector.json                   blob 163d289627496de78ad76e9b7775a8cbf2656c72  1 041 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/E-D-N4-ABSENT.json                blob c4b66e7063847bf5fe5884c83260754e5d05b161  8 550 bytes  272 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/E-D-N4-STALE.json                 blob ba36cac3614d3697d3b96880024c0501b039247c  8 708 bytes  276 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/E-D-NC1.json                      blob 365b28bfd19a5ba219b4701225fbe19cc4b09436  8 710 bytes  276 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/derive/E-v.json                          blob d3cb29f927412dc1315a89d5f78390dcac9c872c  8 702 bytes  276 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4/E.json                              blob b475f473a0b6cdddf0f9796750143a920f40a43b  8 550 bytes  272 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4/closure-n4-subst.closure.json       blob 5dead5be2e6fdbb293eaea4916c0da122d04ac22  14 246 bytes  444 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4/closure-n4-subst.derivation.json    blob 0ac64953626c1acb451f8042c46d0370dd4b286d  604 bytes  28 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4/closure-n4.closure.json             blob 9968626b7d1537bacad35b8bb0ca6e672143f0b9  12 388 bytes  382 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4/closure-n4.derivation.json          blob 54fb4cf01a2798dfcb42cdb59408240139d966d9  1 042 bytes  53 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4/closure-n4.vector.json              blob 9562e967c2e743080cfbeb7b7fc61546a40a94e6  1 038 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4/controller-output.json              blob 630b004308328e269868533fd0b18c52eddca9fc  7 095 bytes  169 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4/request-c.json                      blob e36739887909f0394ddbe0eb1b1f34eb00f3c622  3 770 bytes  144 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4x/request-x-subst.json               blob 7265bcde0bba73ff5c3b840d4973e9bae10a53d8  4 175 bytes  159 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-n4x/request-x.json                     blob 47ffb00dbc02ef4ddb26b2035bd8d20787355af6  4 169 bytes  159 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1/E.json                             blob ee30595021f4d84328a7793c44413e135d2ea18b  8 710 bytes  276 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1/closure-nc1.closure.json           blob 14abb305ba7902be3e25629546d8adf8de826aab  14 248 bytes  444 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1/closure-nc1.derivation.json        blob 929be3aa9f5b09772f0272b26430148e9bb8b67d  796 bytes  38 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1/closure-nc1.vector.json            blob bb992a791195c2f537c2dfe7921452b12c9decb1  1 047 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1/controller-output-V1.json          blob 3a1dd8d75f6583066da66080be96fc404f777ec0  7 424 bytes  169 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1/controller-output-V2.json          blob 0ecd1328681372cc833119886eb576718ff9e2a6  7 339 bytes  169 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1/request-V1.json                    blob 27777eed7bbc36a03a378eafd971caf4290d32ae  5 095 bytes  188 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1/request-V2.json                    blob cbb8175782b3b84b935dfdad1ed20f7f327118b3  4 516 bytes  174 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1v3/controller-output-V3.json        blob f88274c407c683cd6c59e95a7f147fc8e186aaf1  7 307 bytes  169 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1v3/request-V3.json                  blob 3a9a7b1d8a85d688c1356c6bc4a8fd337dedf1e1  4 257 bytes  163 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1x/closure-as-other.json             blob 7c83ff72885bc811546597ce9fb703f9d6d130de  15 566 bytes  489 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1x/request-x-subst.json              blob 373d0474f7901381ac19fe2bd3ed80741ba15592  5 414 bytes  203 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-nc1x/request-x.json                    blob b5e701ff172cfce1fc8346f2c46c5fa44d919519  5 416 bytes  203 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-v/E.json                               blob 2d6a633b3a43e2a170f1831d4b9915b748889f41  8 702 bytes  276 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-v/closure-v-strict.closure.json        blob 33b05dd702d8d0e28a9b0063482104951cf4ab40  13 054 bytes  403 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-v/closure-v-strict.derivation.json     blob d301f4a7c4e7a187091eda27663f02ea4875ddce  602 bytes  28 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-v/closure-v.closure.json               blob cedb337e1fb2787b98bfd9a3e6e7231718402354  14 240 bytes  444 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-v/closure-v.derivation.json            blob a544bd82ed6ae2765b57d92ce2887c78736664d1  588 bytes  28 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-v/closure-v.vector.json                blob 73779541a3d047a65dfa884ba73805a15b00f147  1 021 bytes  26 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-v/controller-output.json               blob 0a344c9f6807c3bbdd9fd8b86e8377f81d55fdb9  8 545 bytes  169 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/inputs/s-v/request.json                         blob b7df17df13a60fd9007ccc36525ca3ffa123fdc4  6 123 bytes  235 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/lib_r1r4.py                                     blob a623a33f9b808c0e080d2f84fb545089d32864ac  37 661 bytes  731 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/lib_r3.py                                       blob 7fdba7674ebcb372dc264e59506adb4242d2903d  24 809 bytes  462 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/lib_r4.py                                       blob 0ba99ca25dc8c8edfdd2a7cc973da0b9f7a3e1b3  11 408 bytes  240 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/make_readme_r4.py                               blob 8e35063f1bdc7db3b2fae7705f3016575aaef45c  5 009 bytes  104 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/p10-nc1.json                                    blob 58d49e2e65093ede0df3197f6c88476ac6fc4e6b  37 501 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/p11-indep.json                                  blob 9feb4029b858f81a3da9c3849c75949f6e92fca6  26 506 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/p7-derive.json                                  blob 906c704ace458d877ef6daaedbcb264fcff4d871  81 082 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/p8-v.json                                       blob 28f4ae4a6983d1a0f3dc741c14bab3a426004d84  28 338 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/p9-n4.json                                      blob 8b912b084a9629f57461bacb79af9c12e477bdc1  27 045 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/reconstruct_runs.py                             blob 8a00ac1adaacd99e67847ef30b8ab3b11992e306  18 603 bytes  416 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/run_r4.py                                       blob 5acb1b530baa75766f822911262f53b8cb4ebcb3  55 857 bytes  794 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/summary.json                                    blob 0332c8d4d9bca302bc40bdfb3df5ea19f325c1e8  10 975 bytes  1 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/table_r3.py                                     blob 9e5dc41147539e1308f2ba497e4b821c70e0af92  14 518 bytes  264 líneas
docs/automation/evidence/I-62-A6/r4/r1probes/table_r4.py                                     blob 552523afa0fb93a5aac8749bcbef18d750b8f6e0  3 817 bytes  80 líneas
docs/automation/evidence/I-62-A6/r4/readable/MANIFEST.json                                   blob e4dc9e609fc8335b84d1e4f9b5c32511d18fc929  3 533 bytes  54 líneas
docs/automation/evidence/I-62-A6/r4/readable/r1probes/p10-nc1.json                           blob 56e12b8e5bc590848f7ce0c28eb50b54749cfca8  45 660 bytes  1 300 líneas
docs/automation/evidence/I-62-A6/r4/readable/r1probes/p11-indep.json                         blob 05b625e186d9c4c188e41183d6a13b683b1227c2  44 435 bytes  2 472 líneas
docs/automation/evidence/I-62-A6/r4/readable/r1probes/p7-derive.json                         blob 8426d94d61209981144962c1a4d0438259c661aa  108 799 bytes  3 787 líneas
docs/automation/evidence/I-62-A6/r4/readable/r1probes/p8-v.json                              blob 05369483f02276289b92a9c3fc8875eefed17c06  32 676 bytes  719 líneas
docs/automation/evidence/I-62-A6/r4/readable/r1probes/p9-n4.json                             blob 95a9b3d34ae1d95e5deafcb7f14ea9d3e10a3d33  32 087 bytes  828 líneas
docs/automation/evidence/I-62-A6/r4/readable/r1probes/summary.json                           blob 55d927618a2143522e0171adcc647c07ac6fbff0  12 548 bytes  370 líneas
docs/automation/evidence/I-62-A6/r4/text/make_r4_fills.py                                    blob b0896e4b208aa8ef4dae6d2fe12bd43deb17ab97  1 811 bytes  33 líneas
docs/automation/evidence/I-62-A6/r4/text/make_r4_text.py                                     blob cf44d45a03b113266783185666f3c054a3458ce0  34 539 bytes  356 líneas
docs/automation/evidence/I-62-A6/r4/text/r6b-r4.txt                                          blob 28d2df881ddb5000040dfc5ee202e6d404e668c7  2 036 bytes  7 líneas
```

**Evidencia de la ronda 3** (41 archivos, enteros: `r1probes/` (las sondas que criticó A62-A6R3-R1), sus copias legibles en
`docs/automation/evidence/I-62-A6/r3/readable/` (con su MANIFEST; se leen y se citan del mismo modo) y los archivos de `r3/` que citan `r4/` o A-6):

```text
docs/automation/evidence/I-62-A6/r3/a6r3-guards.py                                 blob 103d69c62025b852a6b2233b1a55d78ba30f083d  204 992 bytes  3 111 líneas
docs/automation/evidence/I-62-A6/r3/launchfix/README.md                            blob f63a644096c627f5971312d6b51c06c5e2dbd282  21 588 bytes  178 líneas
docs/automation/evidence/I-62-A6/r3/make_candidate_commit_a6r3.py                  blob aa32fd3f9bd16323c7a72010b27a198dcade87ff  7 139 bytes  109 líneas
docs/automation/evidence/I-62-A6/r3/neg_controls_a6r3.py                           blob ba3d27de597ce65d92ee60edc2dce42f3c9803cc  14 312 bytes  224 líneas
docs/automation/evidence/I-62-A6/r3/preflight.md                                   blob 0e84975ce2dbef235d2be70f22d69d12a94b9f96  7 473 bytes  85 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/README.md                             blob 2a354653793223af661c7b911dd5bc171a568d27  31 626 bytes  318 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/m2-request.json                blob 49a1eaddd31126afd073c4d2ee13170b77e03cd3  5 782 bytes  235 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/m5-controller-output.json      blob 52c43211e017d34d1fc5c40cc5badff07f75497a  7 869 bytes  168 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/n4-controller-output.json      blob 0707c189acfe702f4e97ae047e1bd0bb0d9de9c2  7 127 bytes  169 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/n4-request-a-r2.json           blob 6f737e5acb263f57ea77f5c64cef74210a69b1f3  4 164 bytes  157 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/n4-request-c-r3.json           blob 7e9d40f1f1481f6e700510c8b35efc6be19a852b  3 647 bytes  143 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/nc1-V1-controller-output.json  blob 49fd90d3aebab71077f1ae7988db490bd8572aa8  7 469 bytes  169 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/nc1-V2-controller-output.json  blob 25f88a4314ca56a8efe622d323fedbec27a37fdf  7 384 bytes  169 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/nc1-V2-request-r2.json         blob fc2527f83c34f27f6c21ca56e259b50378cf2175  5 358 bytes  202 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/nc1-V2-request-r3.json         blob 6f0c3117a8a087a263f790d72feabadce3160df4  4 779 bytes  188 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/nc1-V3-controller-output.json  blob 2590a098a6f1bc9a62ea2dc78ed071a3ed89a88a  7 352 bytes  169 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/nc1-V3-request-r3.json         blob d8ea797a702050cb53054e2285cd086730dd14ce  4 522 bytes  177 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/t1-controller-output.json      blob 49678656815d24dbd6f6458b46033520f657bc81  8 552 bytes  171 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/t1-request-r3.json             blob d1742603eab56ad75ce9f06f1dedc903129db5cd  6 424 bytes  249 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/inputs/zero-session-events.jsonl      blob d7f1326dfd0ee8ab523f092b6fe1f45acad2549a  4 323 bytes  15 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/lib_r1r4.py                           blob a623a33f9b808c0e080d2f84fb545089d32864ac  37 661 bytes  731 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/lib_r3.py                             blob 7fdba7674ebcb372dc264e59506adb4242d2903d  24 809 bytes  462 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/p1-zero.json                          blob 46aedc5168ff65739ac2eb37b9e5ce212b21779f  9 060 bytes  1 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/p2-n4.json                            blob 2d708d5d5f6242d536ff5e5522587248b9a3faea  24 339 bytes  1 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/p3-nc1.json                           blob 21c2cf9ca59d92ec8ddc09554698476bf94d6286  37 922 bytes  1 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/p4-t1.json                            blob f0f45c83ccf71e91685b3013c9a9400798d70070  31 362 bytes  1 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/p5-mutants.json                       blob ac750dec7c7e4724104ca87db1fb974738c2939a  21 496 bytes  1 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/p6-reconstruct.json                   blob a84a6ddec7e9dca2648b7cf9b6a3236968c4173c  39 097 bytes  1 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/reconstruct_runs.py                   blob 8a00ac1adaacd99e67847ef30b8ab3b11992e306  18 603 bytes  416 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/run_r3.py                             blob cd695b5d120978279edc5d90dbe5d987dbcbddb7  60 343 bytes  832 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/summary.json                          blob 59b312bc09824dddf94adb71f87230244d90dec2  25 409 bytes  1 líneas
docs/automation/evidence/I-62-A6/r3/r1probes/table_r3.py                           blob 9e5dc41147539e1308f2ba497e4b821c70e0af92  14 518 bytes  264 líneas
docs/automation/evidence/I-62-A6/r3/r2static/README.md                             blob 0a232b8add4f9cebc9c6d715bafbdfb5fdcd7fe6  34 323 bytes  327 líneas
docs/automation/evidence/I-62-A6/r3/readable/MANIFEST.json                         blob 293aa256994a21815c81777200c48b1565915a85  5 198 bytes  78 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p1-zero.json                 blob 37d83735dc9dc7b1e444f890377c0798d3e9ca28  11 533 bytes  382 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p2-n4.json                   blob 5a1d7166bb61bd737c217034c6cfae54c1f01740  28 123 bytes  646 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p3-nc1.json                  blob 8481cfb7e71a6f7b18944b296c326f6f90ef01be  45 208 bytes  1 162 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p4-t1.json                   blob 948353e861c107e37219ab089aa5916e6fc41d9e  35 608 bytes  768 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p5-mutants.json              blob 6155cc7cc94da97c01f0f6dc228b6262a705704b  27 871 bytes  931 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p6-reconstruct.json          blob f4f8e73606e1615c93b397c1d9ae06c9c72f305d  48 610 bytes  1 525 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/summary.json                 blob fb78cab88ec1b137551547f25bc15aebc9dd4a3b  30 758 bytes  1 073 líneas
```

**Evidencia de la ronda 2** (24 archivos, enteros: `independence/`, el escenario del banco de la ronda 4, y sus copias legibles en
`docs/automation/evidence/I-62-A6/r2/readable/`, con su MANIFEST):

```text
docs/automation/evidence/I-62-A6/r2/independence/README.md                          blob 72c9a7ebfb9ece9c38a2e998ea8f01e568f1ec81  35 217 bytes  349 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/a-mutation.json            blob 4208d2818f0564255995f89775df42af9b3eff66  143 422 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/b-clausemap.json           blob c3a6f0c3b6da531239f56cd3230abeef70317ace  15 318 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/build.json                 blob 735100b4e10b8ed414a583f051bd2da1d0010e7f  32 063 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/c-r4.json                  blob e132f401590b449475f85801bdac9529debd6825  88 043 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/d-closure.json             blob eca5d38d28d7a5c95981aa77fd0d5f5aed6e8b01  25 043 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/e-review.json              blob aa6d277262108335955046b4f4961aaf00b95e0c  12 277 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/manifest.json              blob 590366246857de92d17eba75611b18e190aa6cc8  1 986 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/build_repos.py             blob 85dac3f5ec3c1b6dbd40d03f9eaddca0266b6275  18 544 bytes  375 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/finalize.py                blob 82652d5a459df497b53c11700457c9b2e8061249  2 352 bytes  59 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/part_a.py                  blob 2fdd9e8c365518c48645e0b2cc9e5e6375334cd1  31 441 bytes  522 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/part_b.py                  blob bc7de2bfa50d30bb71fb91898e7a6c2ac509c3c5  24 003 bytes  379 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/part_c.py                  blob d100062e2e041fafa17b325c8de34981d7e00718  12 618 bytes  193 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/part_d.py                  blob e75059bb3ef3b42458487e1a5250c3421d46ca74  21 128 bytes  323 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/r3lib.py                   blob 33c403cf39962aa8ce641e1bbf78a3bdb167a024  20 123 bytes  503 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/r3model.py                 blob bc266bdd6dd4ec3786d56e063555c3e1d335536c  17 267 bytes  327 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/review_e.py                blob 7a9a81c219bf1adef6b53c7bb13e7f3e43557f62  14 766 bytes  220 líneas
docs/automation/evidence/I-62-A6/r2/readable/MANIFEST.json                          blob 392f265f38506872529e86f473ae03dfc99678f9  15 292 bytes  222 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/a-mutation.json   blob 9e551f2fa1e2a260cda2fe6fa65dd7191cae5594  212 023 bytes  9 227 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/b-clausemap.json  blob c052af8c53f752b359a4efa8376a91fbbe6041eb  19 473 bytes  796 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/build.json        blob 2df963f067537aa5fcc89a92aa1c743c278ac45f  40 773 bytes  1 232 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/c-r4.json         blob 454a884a71e7483910184b41e8a706c4987c3502  122 395 bytes  4 269 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/d-closure.json    blob b08e615963db6d6a59f184ce001b5bbe286e606e  31 485 bytes  853 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/e-review.json     blob 4009d425df0f69037c0e67a3ef3c3cb9f4e8e571  14 610 bytes  427 líneas
```

**Paquete B-min** (30 archivos, enteros; el aceptante fijado es `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py` (2 816 líneas, por tramos de 300); el instrumento,
las notas, las pruebas y los resultados):

```text
docs/automation/evidence/I-62-F6/FX-02/bmin/README.md                           blob 57a0fb0c234a27df96fdb2ea82b3b2bceb3d4bea  11 764 bytes  143 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/SPEC.md                             blob 4d448c6fc4670cdf97b21dcd17fc00ba1bedcc0c  23 863 bytes  316 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept-suite.log                    blob 9cd2f405d7c1355f046f64ee5a66a1feeceb6a45  5 274 bytes  58 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept/ACCEPT-NOTES.md              blob cb9df0289c69617122feaa896728c3a86d2040a8  21 336 bytes  240 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py               blob 434ef1fccb0d15762b30244b4a60928120698411  111 873 bytes  2 816 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept/tests/fake_tool.py           blob 3a3e3d92b2ffe3d0bb52770bdb45787a8d3f2179  14 978 bytes  373 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/accept/tests/test_bmin_accept.py    blob fb6fbeeeab6774e13c7d4ccedbc0e569cd927368  74 902 bytes  1 445 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/e2e-run.log                     blob 6e1f940e02b934599f1389e1f56eb7b6877ba3e9  2 602 bytes  85 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/e2e.py                          blob fbfa3353a1a7df62e7805e5af8954acde5c425d2  15 475 bytes  233 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/acceptor-result.json        blob de2d2c92181cc3ff03c51085ac195cc30c2e1327  6 502 bytes  293 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/e2e-summary.json            blob 5b5972190d898550b22b742d268d90d6c52a172f  7 711 bytes  387 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-clausemap.json     blob 150bbed82950853e3dcb331b99234222f2168543  44 637 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-cleantree.json     blob 05f2026175e9c26df4058e55cc4518a323af2c7d  903 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-contract.json      blob beab3abd40b9ddfae7094eb09d77feac70e32e55  2 294 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-diff.json          blob b34cd0ce331f5ea2dc1a0873c1c56141bc986f1e  2 083 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-freetext.json      blob ac658e4c60c35cdd7e391f402c890860496cdc56  1 733 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-handoff.json       blob 0a29fc6d93e4b186bc5229f748a44c9ee1a313de  1 855 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-hashes.json        blob a4821e2b6f20853611bd4263d8379267f0a17a7e  2 244 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-identity.json      blob be4685808202de4667267b01f55b342ac42b6ce6  2 393 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-read-json.json     blob 3eaaab4f4b8a26485f14e0146b673a211be7030c  1 568 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-read-text-1.json   blob b289013983e4c8bb7a13712f4365015dd1bf499d  1 344 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-read-text-73.json  blob 3a336ed4c59d0c312e64d4f6e4dac91507deee74  1 541 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-scope.json         blob a66e75761bbe8cc023e45b30bd84da3196c8b2dd  3 168 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-trx.json           blob 45e8e29ce034e9addd5eec2c1685540eb250efea  1 101 bytes  1 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool-suite.log                      blob 08f11ab465d8894806e228fc0d4045e6dc6bdeda  177 bytes  6 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/TOOL-NOTES.md                  blob d3218ff79c84a6763db67081408785304a24e0d1  17 434 bytes  215 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py                  blob 9a829b0eeb1210238052b1feac06cfa8f7964aed  64 764 bytes  1 614 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/mutate.py                blob 6967938954219a6a8a2f38dc80ef5a1165966d19  12 145 bytes  280 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/test_bmin_facts.py       blob 0a74bb2dcf18c1c62ffb55ca87da0e259046fe6d  80 642 bytes  1 573 líneas
docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/zero_write_check.py      blob 4188d256b76bc3a375454027d208b98d801c8ccc  3 491 bytes  85 líneas
```

**Revisión de la ronda 3** (salida estructurada; trae A62-A6R3-R1, A62-A6R3-O1..O7 y su `NegativeCaseCoherence` literales):

```text
docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/output.json  blob 691bbc8fabdefaa853c1d6e29465b6c1a1889107  63 624 bytes  859 líneas
```

**Canónicos.** Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por tramos de 300 líneas como máximo si pasa de
60 000 bytes.
1. `docs/initiatives/I-62-proposal-v14.md` (342 761 bytes, 3 323 líneas; blob `34ad80ea`), §18, §20.2, §20.3, §20.3.1, Anexo C, D.1, D.3, D.4, D.5 y D.6: §18 952-977; §18, OV-I62-03 960; §18, OD-5 974; §20.2, invariante de autoridad AUT-I1 1039-1047; §20.2 1027-1047; §20.3, introducción (hasta §20.3.1) 1049-1071; §20.3.1 1073-1130; §20.3.1, clase CanonicalInputs 1079; §20.3.1, pasos 2-4 1088-1099; §20.3.1, verificación 1126-1130; Anexo C 2330-2382; Anexo C, esperado independiente (L2333-L2334): LECTURA DIRECTA OBLIGATORIA 2333-2334; Anexo C, C-24 2363; D.1 2386-2406; D.3 2431-2528; D.4 2530-2554; D.5 2556-2565; D.6 2567-2587; D.3, Topología A: tabla de roles, adapters y topes 2433-2447; D.3, totales de la ronda A 2523; D.4, resultados PASS/FAIL/UNVERIFIED/UNSUPPORTED 2534-2537; D.4, matriz de cierre: FX-02 PASS 2544; D.5, resiembra y FX-02 2558-2560; D.6, dependencias técnicas permitidas y privadas prohibidas 2572-2573; D.6, último párrafo, ancla de A6-3 2586-2587.
2. `docs/AUTOMATION_PLAN.md` (155 229 bytes, 1 732 líneas; blob `f525cb1e`), 16.4, 16.9, 16.10, 16.11, 16.13-16.14 y 16.18-16.24: 16.4 507-543; 16.9 620-665; 16.10 667-672; 16.11 674-708; 16.13 717-928; 16.14 930-948; 16.18 1015-1041; 16.19 1043-1061; 16.20 1063-1142; 16.21 1144-1180; 16.22 1182-1209; 16.23 1211-1254; 16.24 1256-1323; 16.4, transporte 509; 16.4, receta de Codex 541-543; 16.9, tabla de las catorce comprobaciones 628-643; 16.9, primer fallo y precedencia 623-624; 16.9, nunca bastan stdout ni un JSON conforme 646; 16.9, salida conforme pero vacía o falsa: INVALID_OUTPUT 653; 16.11, recuperación 676; 16.13, mapa en I62_EFFECTIVE_SHA 731; 16.13, Validate(M): MV-1..MV-7 902-908; 16.13, tabla de defectos de Validate(M) 910-918; 16.14, punto efectivo 945-946; 16.19, contrato del adapter 1045-1046; 16.19, descriptor y Facts 1048-1052; 16.20, sin celda elegible no se invoca 1093; 16.22, comparación no establecida 1204-1207; 16.23, permisos mínimos 1221; 16.23, PLAN y VERIFY 1240; 16.24, clase CanonicalInputs 1264; 16.24, clases AllowedTransitiveInputs, DeclaredRuntimeContext y ForbiddenInputs 1265-1267; 16.24, cálculo del cierre, pasos 2-4 1271-1276; 16.24, auditoría de lecturas: P-22 y Contexto 1279-1280; 16.24, identidad del revisor 1282; 16.24, fidelidad de los insumos 1287-1288; 16.24, P-22..P-25 1318-1321.
3. `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` (9 871 bytes, 105 líneas; blob `da275e1a`), entero: cabecera 1-8; Contexto 10-24; Decisión 26-69; Alternativas consideradas 71-78; Consecuencias 80-97; Referencias 99-105; Decisión, punto 1 28-43; Decisión, punto 2 44-49; Decisión, punto 3 50-51; Decisión, punto 4 52-58; Decisión, punto 5 59-60; Decisión, punto 6 61-63; Decisión, punto 7 64-66; Decisión, punto 8 67-68; Decisión, punto 9 69.
4. `docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md` (10 832 bytes, 146 líneas; blob `e1bd8d91`), entero: cabecera 1-13; Contexto 15-26; Decisión 28-104; Alternativas consideradas 106-115; Consecuencias 117-135; Referencias 137-146; Decisión, punto 1 59-61; Decisión, punto 2 62-70; Decisión, punto 3 71-74; Decisión, punto 4 75-77; Decisión, punto 5 78-81; Decisión, punto 6 82-87; Decisión, punto 7 88-90; Decisión, punto 8 91-92; Decisión, punto 9 93-104.
5. `docs/INITIATIVE_LIFECYCLE.md` (24 334 bytes, 360 líneas; blob `f19896a8`), §3, §5 y §6: §3 66-94; §3, M-01 83; §3, M-02 84; §3, M-03 85; §3, M-04 86; §3, M-05 87; §3, M-06 88; §3, M-07 89; §3, M-08 90; §3, UNKNOWN cuenta como activado 78-79; §3, OWNER-RESERVED 94; §5 152-191; §5, REQUIRED y OPTIONAL 166; §5, veredictos 171; §6 193-222.
6. `docs/initiatives/I-62-A-1.md` (77 984 bytes, 437 líneas; blob `c01899a7`), cabecera (orientación): cabecera 1-56.
7. `docs/initiatives/I-62-A-2.md` (29 957 bytes, 352 líneas; blob `f1e1d6f0`), cabecera (orientación): cabecera 1-48.
8. `docs/initiatives/I-62-A-3.md` (200 993 bytes, 1 536 líneas; blob `ea6721f7`), cabecera (orientación): cabecera 1-62.
9. `docs/initiatives/I-62-A-4.md` (62 415 bytes, 516 líneas; blob `5e2ba68e`), entera: cabecera 1-56; §1 58-78; §2 80-159; §3 161-361; §3.1 163-183; §3.2 185-211; §3.3 213-233; §3.4 235-256; §3.5 258-287; §3.6 289-301; §3.7 303-343; §3.8 345-361; §4 363-385; §5 387-398; §6 400-417; §7 419-435; §8 437-470; §9 472-479; §10 481-490; Anexo (no normativo) 492-516; §2, hueco 169; §3.2, A4-1, regla 1 192; §3.2, A4-1, reglas 3-4 203-207; §5, M-03 393.
10. `docs/initiatives/I-62-A-5.md` (37 342 bytes, 372 líneas; blob `ad94ceed`), entera: cabecera 1-49; §1 51-72; §2 74-147; §3 149-242; §3.1 151-167; §3.2 169-199; §3.3 201-221; §3.4 223-242; §4 244-259; §5 261-272; §6 274-296; §7 298-317; §8 319-334; §9 336-344; §10 346-354; Anexo (no normativo) 356-372; §3.2, párrafo A5-1 175-199; §3.3, A5-2, regla 4 216; §3.3, fin de A5-2 y de D.3 con A-1..A-5 221.
11. `docs/automation/agent-execution/README.md` (60 779 bytes, 756 líneas; blob `592dcfd4`), §5, §8, §10 (L219-L228), §14 y §15: §5 132-150; §8 170-188; §10 217-235; §14 369-531; §14.1 375-383; §14.2 385-403; §14.3 405-427; §14.4 429-437; §14.5 439-486; §14.6 488-498; §14.7 500-531; §15 533-603; §15.1 537-551; §15.2 553-568; §15.3 570-583; §15.4 585-603; §8, salida inválida (INVALID_OUTPUT) 184; §10, controles negativos 219-228; §15.2, regla I2 561; §15.2, regla I6 y auditoría de lecturas 565-568.
12. `docs/automation/agent-execution/routing.md` (10 294 bytes, 142 líneas; blob `bba08fc4`), entero: cabecera 1-7; §1 9-23; §2 25-34; §3 36-45; §4 47-59; §5 61-81; §6 83-88; §7 90-93; §8 95-119; §9 121-142.
13. `docs/automation/agent-execution/adapters/codex-cli.md` (4 333 bytes, 59 líneas; blob `155f3469`), entero: cabecera 1-16; Operaciones 18-30; Introspección 32-36; Huella 38-41; Hechos (`Facts`) 43-53; Límites 55-59.
14. `docs/automation/agent-execution/schemas/input-closure.v1.schema.json` (6 911 bytes, 282 líneas; blob `6bdf7184`), entero.
15. `docs/automation/agent-execution/schemas/role-invocation.v1.schema.json` (21 758 bytes, 858 líneas; blob `d54a7ae7`), entero.
16. `docs/automation/agent-execution/schemas/controller-verification.v2.schema.json` (16 528 bytes, 681 líneas; blob `e1faa9ec`), entero.
17. `docs/automation/agent-execution/schemas/delegation.v2.schema.json` (25 471 bytes, 1 002 líneas; blob `dfdbe461`), entero.
18. `docs/automation/agent-execution/schemas/worker-handoff.schema.json` (3 814 bytes, 81 líneas; blob `cc3c9cf7`), entero.
19. `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md` (24 984 bytes, 217 líneas; blob `90929ad1`), entero: cabecera 1-15; §1 17-30; §2 32-95; §2.1 34-65; §2.2 67-74; §2.3 76-84; §2.4 86-95; §3 97-131; §3.1 99-115; §3.2 117-131; §4 133-166; §5 168-179; §6 181-199; §7 201-217; negativos con invocación: mismo prompt y Target de la verificación real 23; AuthorityRevision y CanonicalInputs del kit 44-45.
20. `docs/automation/decisions/I-62.md` (277 718 bytes, 1 698 líneas; blob `4e090ed4`), §43, §57, §61, §64 y §73 en adelante: §43 812-824; §57 1105-1117; §61 1191-1204; §64 1274-1304; §73 1479-1498; §74 1500-1513; §75 1515-1529; §76 1531-1546; §77 1548-1562; §78 1564-1584; §79 1586-1599; §80 1601-1615; §81 1617-1632; §82 1634-1648; §83 1650-1664; §84 1666-1682; §85 1684-1698; §84, bytes y SHA-256 del cuerpo pegado 1669; §84, punto 1 1673; §84, punto 2 1674; §84, punto 3 1675; §84, punto 4 1676; §84, punto 5 1677; §84, punto 6 1678; §84, punto 7 1679; §84, punto 8 1680; §84, punto 9 1681; §84, punto 10 1682; §85, bytes y SHA-256 del cuerpo pegado 1687; §85, punto 1 1691; §85, punto 2 1692; §85, punto 3 1693; §85, punto 4 1694; §85, punto 5 1695; §85, punto 6 1696; §85, punto 7 1697; §85, punto 8 1698; §78, línea del Owner A6-ADR0046-9-FX02 1572.
21. `docs/automation/evidence/I-62-evidence.md` (386 950 bytes, 4 395 líneas; blob `fafd4392`), §140 en adelante (citadas por A-6): §140 3780-3820; §141 3822-3830; §142 3832-3852; §143 3854-3880; §144 3882-3936; §145 3938-3945; §146 3947-3972; §147 3974-4014; §148 4016-4025; §149 4027-4048; §150 4050-4062; §151 4064-4083; §152 4085-4092; §153 4094-4127; §154 4129-4134; §155 4136-4143; §156 4145-4151; §157 4153-4172; §158 4174-4197; §159 4199-4224; §160 4226-4233; §161 4235-4268; §162 4270-4280; §163 4282-4288; §164 4290-4312; §165 4314-4320; §166 4322-4339; §167 4341-4358; §168 4360-4366; §169 4368-4395.

**Evidencia de I-62** (`docs/automation/evidence/I-62-evidence.md`, en la lista de canónicos). Está en el cierre, como en las rondas 2 y 3, porque
decisiones §79, punto 5, exige acceso directo a toda fuente que el texto cite y A-6 cita sus secciones (rangos citados: L3780-L3820, L3822-L3830, L3832-L3852, L3854-L3880, L3882-L3936, L3938-L3945, L3947-L3972, L3974-L4014, L4016-L4025, L4027-L4048, L4050-L4062, L4064-L4083, L4085-L4092, L4094-L4127, L4129-L4134, L4136-L4143, L4145-L4151, L4153-L4172, L4174-L4197, L4199-L4224, L4226-L4233, L4235-L4268, L4270-L4280, L4282-L4288, L4290-L4312, L4314-L4320, L4322-L4339, L4341-L4358, L4360-L4366). Es el
registro de la sesión que preparó A-6 y este kit: contiene sus resúmenes y valoraciones. Úsala como evidencia de hechos que puedes contrastar con
los archivos custodiados y con el texto normativo, nunca como autoridad ni como juicio sobre A-6; no adoptes sus conclusiones sin comprobarlas. Sus
§156, §158 y §162 (y decisiones §81 y §82, punto 6) tratan de C1-C3 y de I-65, ajenos a esta revisión.

Nada más. Quedan fuera del cierre, en particular: `docs/automation/evidence/I-62-C1C3/` (C1-C3 y la nota de aislamiento de I-65: ajenos a esta
revisión); el resto de `r2/` (fuera de `independence/` y de sus copias legibles) y de `r3/` (fuera de `r1probes/`, de sus copias legibles y de los
archivos de arriba), incluidos los directorios `r3/r2static/` y `r3/launchfix/`, que A-6 solo nombra; la evidencia de la ronda 1; las salidas de las
revisiones de las rondas 1 y 2 y el dictamen de B-min; el resto de los directorios de las revisiones (`audit.json`, `README.md`, `kit/`, `launch/` y
los registros del lanzamiento); la tarjeta y el paquete OWNER-RESERVED de ADR-0046 #9 y la solicitud de medición
(`docs/automation/evidence/I-62-F6/requests/`); las transcripciones y la memoria; el mapa de cláusulas y su esquema, WORKFLOW, FOUNDATIONS,
`state/I-62.yml` y el fixture. Lo que A-6 o la evidencia citen de esos archivos no lo puedes comprobar: si una conclusión tuya depende de ello,
decláralo en `KnownLimitations`.

## Lista de lectura: fuentes que cita A-6 (G11)

Las guardas de la ronda 4 (G11, en el resultado `run` custodiado; la misma lista que `r4/a6r4-sources-for-reviewer.json`) registran todas las
fuentes y rangos de líneas que cita el texto de A-6. Úsala como lista de comprobación: antes de apoyarte en una cita de A-6 (un bloque «> », una
tabla, «V14 L…», «AP L…», «RAE L…», «DEC L…», `ruta:línea`), lee la fuente en esas líneas con Read y contrástala.

- `docs/AUTOMATION_PLAN.md` (blob `f525cb1e`): L520-L523, L535-L538, L541-L543, L610-L611, L622-L625, L623, L625, L646, L653, L676-L677, L686-L687, L910-L918, L1093, L1204, L1204-L1207, L1221, L1236-L1237, L1237, L1240, L1240-L1241, L1244-L1251, L1251, L1264, L1267, L1271-L1276, L1279-L1280, L1287-L1288
- `docs/INITIATIVE_LIFECYCLE.md` (blob `f19896a8`): L94, L221
- `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` (blob `da275e1a`): L61, L69, L78
- `docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md` (blob `e1bd8d91`): L40, L91-L92
- `docs/automation/agent-execution/README.md` (blob `592dcfd4`): L219-L226, L220, L225-L226, L228, L561, L565-L568, L578
- `docs/automation/decisions/I-62.md` (blob `4e090ed4`): L812-L824, L1105-L1117, L1191-L1204, L1274-L1304, L1479-L1698
- `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/ACCEPT-NOTES.md` (blob `cb9df028`): L41-L42
- `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py` (blob `434ef1fc`): L78, L79, L80, L81, L82, L83, L1884-L1917, L1954-L1998, L2001-L2025, L2134-L2197, L2202-L2215, L2218-L2244, L2247-L2276, L2279-L2283, L2286-L2320, L2323-L2327, L2542-L2609, L2612-L2619, L2622-L2623, L2675-L2699, L2801-L2812
- `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md` (blob `90929ad1`): L19-L23, L23, L44-L45
- `docs/automation/evidence/I-62-evidence.md` (G11 sobre el blob `78a3eed0`, prefijo exacto del blob del cierre `fafd4392`: mismas líneas): L3780-L3820, L3822-L3830, L3832-L3852, L3854-L3880, L3882-L3936, L3938-L3945, L3947-L3972, L3974-L4014, L4016-L4025, L4027-L4048, L4050-L4062, L4064-L4083, L4085-L4092, L4094-L4127, L4129-L4134, L4136-L4143, L4145-L4151, L4153-L4172, L4174-L4197, L4199-L4224, L4226-L4233, L4235-L4268, L4270-L4280, L4282-L4288, L4290-L4312, L4314-L4320, L4322-L4339, L4341-L4358, L4360-L4366  [registro de la sesión que preparó A-6: evidencia, no autoridad]
- `docs/initiatives/I-62-A-4.md` (blob `5e2ba68e`): L169, L192, L203, L206-L207, L393
- `docs/initiatives/I-62-A-5.md` (blob `ad94ceed`): L216, L221
- `docs/initiatives/I-62-proposal-v14.md` (blob `34ad80ea`): L362-L363, L960, L974, L1043, L1079, L1088-L1090, L1098-L1099, L1126-L1130, L2333-L2334, L2363, L2438-L2442, L2441, L2523, L2528, L2534-L2536, L2534-L2537, L2544, L2554, L2558-L2560, L2573, L2580-L2582, L2586-L2587

## Reglas de lectura (el invocador audita después cada llamada)

**Regla de esta revisión.** Toda referencia `ruta:línea` o `ruta:inicio-fin` apunta solo a líneas que leíste con Read **en ese mismo archivo** y que te
llegaron fielmente. Una cita de una fuente dentro de A-6 no es una lectura de la fuente: para citar `docs/initiatives/I-62-proposal-v14.md:2333`
tienes que haber leído con Read la línea 2333 de ese archivo; si solo leíste la cita en A-6, cita la línea de A-6. La ronda 1 quedó sin acreditar
exactamente por eso.

Cada ruta se comprueba por su **efecto**: resuelta contra el directorio de trabajo, con `.` y `..` colapsados y los enlaces resueltos.

- **Read:**
  - usa la ruta absoluta, solo de archivos del cierre bajo `D:\r62-arch-a6r4` o de los tres archivos del run;
  - en los archivos de más de 60 000 bytes (`docs/initiatives/I-62-A-6.md`, `docs/automation/evidence/I-62-A6/r4/a6r4-guards.diff`, `docs/automation/evidence/I-62-A6/r4/a6r4-guards.py`, `docs/automation/evidence/I-62-A6/r4/a6r4-selftest.json`, `docs/automation/evidence/I-62-A6/r4/adapt_guards_r4.py`, `docs/automation/evidence/I-62-A6/r4/neg-fixtures/I-62-A-6.r4.draft.md`, `docs/automation/evidence/I-62-A6/r4/neg-fixtures/a6r4-selftest.draft.json`, `docs/automation/evidence/I-62-A6/r4/r1probes/p7-derive.json`, `docs/automation/evidence/I-62-A6/r4/readable/r1probes/p7-derive.json`, `docs/automation/evidence/I-62-A6/r3/r1probes/run_r3.py`, `docs/automation/evidence/I-62-A6/r3/a6r3-guards.py`, `docs/automation/evidence/I-62-A6/r2/independence/results/a-mutation.json`, `docs/automation/evidence/I-62-A6/r2/independence/results/c-r4.json`, `docs/automation/evidence/I-62-A6/r2/readable/independence/results/a-mutation.json`, `docs/automation/evidence/I-62-A6/r2/readable/independence/results/c-r4.json`, `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/tests/test_bmin_accept.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/test_bmin_facts.py`, `docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/output.json`, `docs/initiatives/I-62-proposal-v14.md`, `docs/AUTOMATION_PLAN.md`, `docs/initiatives/I-62-A-1.md`, `docs/initiatives/I-62-A-3.md`, `docs/initiatives/I-62-A-4.md`, `docs/automation/agent-execution/README.md`, `docs/automation/decisions/I-62.md`, `docs/automation/evidence/I-62-evidence.md`, `a6-r3.md`), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** no la necesitas, porque las rutas están arriba. Si la usas, su alcance (`path` y patrón) solo puede contener archivos de la lista
  (`docs/automation/evidence/I-62-A6/r4/`, `docs/automation/evidence/I-62-A6/r3/r1probes/`, `docs/automation/evidence/I-62-A6/r2/independence/` y
  el directorio del paquete B-min están enteros en el cierre; el resto de `I-62-A6/` no). Un patrón que alcance cualquier otro archivo cuenta como
  lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, el fixture
    (`D:\r62-fixture\*`) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una referencia.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: docs/automation/evidence/I-62-A6/r4/r1probes/p10-nc1.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r4/readable/r1probes/p10-nc1.json`); docs/automation/evidence/I-62-A6/r4/r1probes/p11-indep.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r4/readable/r1probes/p11-indep.json`); docs/automation/evidence/I-62-A6/r4/r1probes/p7-derive.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r4/readable/r1probes/p7-derive.json`); docs/automation/evidence/I-62-A6/r4/r1probes/p8-v.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r4/readable/r1probes/p8-v.json`); docs/automation/evidence/I-62-A6/r4/r1probes/p9-n4.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r4/readable/r1probes/p9-n4.json`); docs/automation/evidence/I-62-A6/r4/r1probes/summary.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r4/readable/r1probes/summary.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p1-zero.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p1-zero.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p2-n4.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p2-n4.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p3-nc1.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p3-nc1.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p4-t1.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p4-t1.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p5-mutants.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p5-mutants.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p6-reconstruct.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p6-reconstruct.json`); docs/automation/evidence/I-62-A6/r3/r1probes/summary.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/summary.json`); docs/automation/evidence/I-62-A6/r2/independence/results/a-mutation.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/a-mutation.json`); docs/automation/evidence/I-62-A6/r2/independence/results/b-clausemap.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/b-clausemap.json`); docs/automation/evidence/I-62-A6/r2/independence/results/build.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/build.json`); docs/automation/evidence/I-62-A6/r2/independence/results/c-r4.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/c-r4.json`); docs/automation/evidence/I-62-A6/r2/independence/results/d-closure.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/d-closure.json`); docs/automation/evidence/I-62-A6/r2/independence/results/e-review.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/e-review.json`); docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-clausemap.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-contract.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-diff.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-hashes.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-identity.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-scope.json 1; docs/initiatives/I-62-proposal-v14.md 2352, 2380, 2382; docs/initiatives/I-62-A-3.md 1487; docs/automation/decisions/I-62.md 657, 734, 750, 768, 787, 805, 819. Una línea truncada no cuenta como entregada: no la cites y, si
  te hiciera falta, decláralo en `KnownLimitations`. Para los JSON de una sola línea de `r2/`, `r3/` y `r4/`, lee y cita la copia de `readable/`
  (arriba).
- **Contexto.** El cierre es grande: lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el
  auditor cuenta como un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ∖ ∩ ∪ « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Referencias (`Evidence`, `ClauseRefs` y `DirectReads`)

- Una referencia es `ruta:línea` o `ruta:inicio-fin`: la ruta, relativa al clon para los archivos del cierre (p. ej.,
  `docs/initiatives/I-62-A-6.md:1324`), y el nombre suelto para los archivos del run que pueden ser premisa (`order.txt:48`,
  `a6-r3.md:1`). Sin rutas absolutas ni barras invertidas. `prompt.md` nunca es referencia.
- Cita solo líneas que hayas leído enteras con Read en ese archivo y que te llegaron fielmente. El invocador comprueba que Read te entregó cada línea
  citada, también en `DirectReads` y en los `ClauseRefs` de los elementos anidados.
- **Números de línea exactos:** los que Read muestra a la izquierda. Una referencia desplazada, aunque sea en una línea, no se acredita. Antes de
  entregar, contrasta cada referencia con la salida de Read de la que sale.

## Qué decidir

Se pide el veredicto formal sobre la A-6 exacta de la ronda 4 (LIFECYCLE §5 y §6), acotado a A62-A6R3-R1 y a sus consecuencias directas (decisiones
§85, punto 6). Contesta con el texto normativo del cierre y con lo que leas.

- **`Verdict`:** `AGREED` (cero REQUIRED abiertos sobre la versión exacta), `CHANGES REQUIRED` (al menos un REQUIRED, corregible dentro de la
  superficie y los límites de decisiones §85), `REJECTED` (al menos un REQUIRED y la corrección de la ronda 4 no es admisible dentro de la superficie y
  los límites de decisiones §85, puntos 2, 4 y 7: necesitaría otra disposición) o `BLOCKED — OWNER DECISION` (falta una autoridad reservada: otra
  consecuencia OWNER-RESERVED que A-6 no declara, o una que no esté dispuesta; A-6 §7). Un REQUIRED impide el acuerdo hasta su disposición y
  corrección.
- **Hallazgos nuevos.** Un hallazgo es REQUIRED en los casos de LIFECYCLE §5 (activa M-01..M-08 sobre un elemento congelable, deja un invariante sin
  obligación de prueba, produce comportamiento observable distinto, o revela un elemento materialmente incorrecto, no ejecutable o no verificable);
  una precisión sin cambio de significado es OPTIONAL. Los hallazgos de esta ronda llevan ids nuevos (REQUIRED `A62-A6R4-R1`, `A62-A6R4-R2`…;
  OPTIONAL `A62-A6R4-O1`…), distintos de los de las rondas anteriores, que aquí solo se disponen. Cada uno con la sección exacta de A-6 (`Section`),
  su evidencia (`Evidence`: al menos una referencia) y la corrección (`CorrectionRequired` en un REQUIRED, `SuggestedCorrection` en un OPTIONAL). Un
  REQUIRED trae además la autoridad o el invariante violado (`ViolatedInvariant`) o un contraejemplo concreto (`Counterexample`), al menos uno no vacío,
  y por qué importa (`WhyItMatters`).
- **`Round3FindingResolution`** (cierre de A62-A6R3-R1): `A62-A6R3-R1`, una vez. Compara el hallazgo literal de la salida de la ronda 3 (`Section`,
  `ViolatedInvariant`, `Counterexample` y `CorrectionRequired`, con sus partes (1)-(3)), lo que pide decisiones §85 (puntos 2 y 3, líneas 1692 y
  1693), el texto de la ronda 3 (`a6-r3.md`), el de la ronda 4 (A6-3, regla 6 (b), y §3.18) y la evidencia de `r4/`. `Status`: `CORRECTED` (el
  defecto ya no existe y se cumple lo que pide §85), `PARTIALLY_CORRECTED` o `NOT_CORRECTED`; con `Rationale`, `ClauseRefs` y `FindingIds`. Lo que no
  esté corregido del todo cita al menos un REQUIRED nuevo; un CORRECTED no cita ninguno.
- **`ClosureRuleMechanical`** (A6-3, regla 6 (b), apartados (i)-(v), L600-617, frente a la tabla de derivación de A6-9, regla 4, L848-872):
  - `Coverage`, una vez cada caso: `N4_ABSENT_DELIVERY` (toda forma de entrega ausente: E no fija su blob, bytes custodiados ausentes, otro blob, no un
    único objeto JSON en UTF-8, claves repetidas), `NC1_NONEXISTENT_COMMIT` (un valor de commit que no nombra un commit existente) y
    `ABSENT_NULL_EMPTY_FIELDS` (un campo que la tabla lee ausente, `null` o vacío). Para cada caso: `Status` (`COVERED` si la regla fija un único
    resultado de la derivación, `NOT_COVERED` si no) y `JudgmentStepRequired` (true si la supervisión necesitaría un criterio que A-6 no da para
    obtener el cierre); un `NOT_COVERED` o un `JudgmentStepRequired` true cita un REQUIRED; un caso cubierto y sin juicio no cita ninguno;
  - `SubRules`, una vez cada apartado (`i`..`v`): `IntroducesInput` (`yes` si el apartado admite en el cierre un insumo u objeto que la tabla no toma de
    sus entradas, o que no existe) y `SubstitutesInput` (`yes` si un término omitido se sustituye por otro valor, como otra entrega, otro commit, una
    ref, el `Target` o el cierre de una prueba general); un `yes` cita un REQUIRED; `no`/`no` no cita ninguno;
  - `Assessment`: `TOTAL_AND_MECHANICAL` (todos los casos cubiertos, ninguno con juicio y ningún apartado con `yes`; no cita ningún REQUIRED) o
    `NOT_TOTAL_OR_NOT_MECHANICAL` (cita un REQUIRED); con `Rationale`, `ClauseRefs` y `FindingIds` en el conjunto y en cada elemento.
- **`A69Compatibility`** (A6-9, regla 4, L841-843, y su tabla, L848-872; decisiones §85, punto 2, línea 1692): `NONE` (ninguna contradicción
  que exija editar A6-9; `Contradictions` vacía y sin REQUIRED vinculado) o `CONTRADICTIONS` (cada contradicción en `Contradictions`, con un REQUIRED
  vinculado); con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`NegativeCaseCoherence`** (coherencia de N4 y nc1): `N4` (entrega ausente) y `NC1` (`CurrentSha` inexistente), una vez cada uno: si A-6, con la
  regla 6 (b), los trata de forma total y coherente con AUTOMATION_PLAN 16.9 (semántica y precedencia de los fallos sin cambio), 16.11 y 16.22, con
  RAE §10 (oráculo relativo de los controles negativos; copias de las entradas salvo el campo mutado) y con el aceptante fijado, a través de la
  solicitud de A6-3, regla 6 (L590-626), la tabla de A6-5, regla 2 (L673-686), la salida no aceptada de su regla 4 (L689-702) y el ámbito de su regla 7
  (L717-733). Contrasta con la valoración de la ronda 3 (su `NegativeCaseCoherence`) y con las sondas de `r4/r1probes/` (copias legibles en
  `r4/readable/r1probes/`). `Status`: `COHERENT` o `INCOHERENT` (cita un REQUIRED; un COHERENT no cita ninguno); con `Rationale`, `ClauseRefs` y
  `FindingIds`.
- **`EvidenceAdequacy`** (evidencia de `r4/r1probes/`), una vez cada aspecto: `DERIVATION` (derivación del cierre con la regla corregida, desde el
  escenario de `r2/independence/` y el `Target` de la verificación real; RAE §10 y `controller-contracts.md`, L23, L44-L45), `INDEPENDENT_IMPLEMENTATION`
  (cierre calculado de forma independiente, `r4/r1probes/indep/`), `SUBSTITUTIONS` (controles negativos en que una entrada inexistente se sustituye por
  una válida), `ADDED_OR_INVENTED_INPUTS` (rechazo de insumos adicionales o inventados y cotejo del contrato de la solicitud con el cierre efectivo),
  `ACCEPTOR_RUNS` (corridas del aceptante fijado con N4, nc1 y la verificación real) y `O3_STRICT` (`clausemap` en la cobertura del cierre de VERIFY,
  con la variante I6 estricta). `Status`: `ADEQUATE` o `INADEQUATE`; los cinco primeros son las pruebas obligatorias de decisiones §85, punto 3, y un
  INADEQUATE en ellos cita un REQUIRED; `O3_STRICT` corresponde a un opcional (§85, punto 4), y un INADEQUATE en él cita al menos un hallazgo; un
  ADEQUATE no cita ningún REQUIRED. Con `Rationale`, `ClauseRefs` y `FindingIds`. Lo que la evidencia no ejecutó no lo das por ejecutado.
- **`DeltaPurity`** (solo A6-3, regla 6 (b), cambia normativamente; compara A-6 con `a6-r3.md` y con G5 del resultado `run` de las guardas):
  `CONFIRMED` o `NOT_CONFIRMED` (cita un REQUIRED; un CONFIRMED no cita ninguno), con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`OptionalDispositionConformance`** (O1-O7 de la ronda 3): `A62-A6R3-O1`..`A62-A6R3-O7`, una vez cada uno, según lo que hace A-6 con cada uno
  (§3.18, filas O1-O7) frente a lo que ordena decisiones §85, punto 4 (línea 1694; `order.txt`, líneas 28-35): `CONFORMS` o `DOES_NOT_CONFORM`
  (cita al menos un hallazgo), con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`QuestionAnswers`** (preguntas de la ronda 4, A-6 §8, filas L1369-1372): `Q-A6R4-01`..`Q-A6R4-04`, una vez cada una: `Answer` (tu respuesta) y
  `Outcome` (`SATISFIED` si la respuesta no exige ningún cambio de A-6, `NOT_SATISFIED` si lo exige; un NOT_SATISFIED cita al menos un hallazgo), con
  `ClauseRefs` y `FindingIds`.
- **`DirectReads`:** `{Ref, Purpose}` de cada fuente que leíste directamente para comprobar una cita de A-6. Incluye siempre
  `docs/initiatives/I-62-proposal-v14.md:2333-2334` (Paso 0) y, como mínimo recomendado, una entrada por fuente de la lista G11 en la que te apoyes.
- **`OwnerAuthorityCheck`:** si el acuerdo exige una decisión del Owner que todavía no consta (`OwnerDecisionRequired`: true o false). La línea (a)
  consta (decisiones §78, línea 1572; A-6 §7, L1289-1326; líneas L1324 y L1325) y la (b) solo rige la aplicación: ninguna de las dos cuenta aquí.
  Si concluyes que hace falta otra determinación, true, y nombra cada consecuencia en `AdditionalOwnerReservedConsequences`.
- **Además:**
  - tu modo de revisión, si revisor y autor son la misma persona y el contexto que el runtime te inyectó (`ReviewerMode`, `SamePersonAsAuthor`,
    `InjectedContextDeclaration`). Declara también el proveedor del revisor y del autor en `SamePersonAsAuthor`. Los identificadores de cuenta que el
    runtime te inyecte (correo, uuid de organización u otros) se declaran solo por su tipo, sin reproducir su valor;
  - la identidad revisada (`ReviewedCommit`, `ReviewedPath`, `ReviewedBlob`);
  - lo que leíste (`Reads`: «ruta: rangos») y la acción siguiente que recomiendas (`RecommendedNextAction`).

Para la medición de A6-7 y la prueba previa de A6-5, regla 6 (L707-716), que la regla 6 (b) también rige (A6-7, reglas 1 y 7, L759-770 y
L789-794), vale lo dicho arriba sobre las precondiciones: se juzgan como precondiciones con fallo cerrado.

## Resultado

Entrega el resultado con **una** llamada a StructuredOutput, con este objeto. Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-a6r4-architect-review/0",
  "RunId": "R20261014T180556Z-5e02", "InvocationId": "I20261014T180556Z-5e02", "LogicalReviewRequestId": "L20261014T180556Z-5e02", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62",
  "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-6.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora de A-6 y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "V14DirectReadL2333L2334": "true | false | null", "OrderReadWhole": "true | false | null",
            "Decisions85DeclaresOrderBytesAndSha256": "true | false | null", "Decisions85RoundThreeDisposition": "true | false | null",
            "GuardsRunModeBaseHeadResult": "true | false | null", "SelftestA6TextBlobMatchesObject": "true | false | null",
            "Round3OutputIdentifiesRunAndFindings": "true | false | null", "Note": "..."},
  "DirectReads": [{"Ref": "docs/initiatives/I-62-proposal-v14.md:2333-2334", "Purpose": "..."}],
  "Reads": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | REJECTED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A6R4-R1", "Section": "...", "ViolatedInvariant": "...", "Counterexample": "...", "Evidence": ["ruta:inicio-fin"],
                        "WhyItMatters": "...", "CorrectionRequired": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A6R4-O1", "Section": "...", "Note": "...", "Evidence": ["ruta:línea"], "SuggestedCorrection": "..."}],
  "Round3FindingResolution": [{"Round3Finding": "A62-A6R3-R1", "Status": "CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED", "Rationale": "...",
                               "ClauseRefs": [], "FindingIds": []}],
  "ClosureRuleMechanical": {"Assessment": "TOTAL_AND_MECHANICAL | NOT_TOTAL_OR_NOT_MECHANICAL | NOT_DETERMINED",
                            "Coverage": [{"Case": "N4_ABSENT_DELIVERY | NC1_NONEXISTENT_COMMIT | ABSENT_NULL_EMPTY_FIELDS", "Status": "COVERED | NOT_COVERED",
                                          "JudgmentStepRequired": "true | false", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
                            "SubRules": [{"SubRule": "i | ii | iii | iv | v", "IntroducesInput": "yes | no", "SubstitutesInput": "yes | no", "Rationale": "...",
                                          "ClauseRefs": [], "FindingIds": []}],
                            "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "A69Compatibility": {"Status": "NONE | CONTRADICTIONS | NOT_DETERMINED", "Contradictions": ["..."], "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "NegativeCaseCoherence": [{"Case": "N4 | NC1", "Status": "COHERENT | INCOHERENT", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "EvidenceAdequacy": [{"Aspect": "DERIVATION | INDEPENDENT_IMPLEMENTATION | SUBSTITUTIONS | ADDED_OR_INVENTED_INPUTS | ACCEPTOR_RUNS | O3_STRICT",
                        "Status": "ADEQUATE | INADEQUATE", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "DeltaPurity": {"Status": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "OptionalDispositionConformance": [{"Round3Optional": "A62-A6R3-O1 | … | A62-A6R3-O7", "Status": "CONFORMS | DOES_NOT_CONFORM", "Rationale": "...",
                                      "ClauseRefs": [], "FindingIds": []}],
  "QuestionAnswers": [{"Question": "Q-A6R4-01 | … | Q-A6R4-04", "Outcome": "SATISFIED | NOT_SATISFIED", "Answer": "...", "ClauseRefs": [],
                       "FindingIds": []}],
  "OwnerAuthorityCheck": {"AdditionalOwnerReservedConsequences": [], "OwnerDecisionRequired": "true | false | null", "Note": "..."},
  "IfAgreed": {"ScopeLimitedToA63R6bAndDirectConsequences": null, "Round3FindingCorrected": null, "ClosureRuleTotalAndMechanical": null,
               "NoContradictionWithA69Rule4": null, "NegativeCasesCoherent": null, "EvidenceAdequate": null, "DeltaLimitedToA63R6b": null,
               "OptionalDispositionsConform": null, "NoAdditionalOwnerDecisionRequired": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: cada campo lleva **tu** comprobación y **tu** evaluación. Donde la plantilla dice
  `"true | false | null"` o `"true | false"`, el valor es un booleano JSON (o `null`), no un texto.
- `Step0`: las siete booleanas son lo que compruebas en el Paso 0 (la primera, la lectura directa de V14 L2333-2334); `null` en lo que no compruebes.
- `DirectReads` incluye siempre V14 L2333-2334, también si te detienes en el Paso 0.
- Cobertura, una vez cada elemento: `Round3FindingResolution`, A62-A6R3-R1; `ClosureRuleMechanical.Coverage`, los tres casos;
  `ClosureRuleMechanical.SubRules`, los apartados i..v; `NegativeCaseCoherence`, N4 y NC1; `EvidenceAdequacy`, los seis aspectos;
  `OptionalDispositionConformance`, A62-A6R3-O1..O7; `QuestionAnswers`, Q-A6R4-01..04.
- Todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`; los ids no se repiten.
- **Con un veredicto** (todo salvo NOT_ACCREDITED):
  - las siete booleanas de `Step0` en `true`; `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob` = el commit, la ruta y el blob de arriba;
  - al menos una `ClauseRef` en cada elemento de `Round3FindingResolution`, `ClosureRuleMechanical.Coverage`, `ClosureRuleMechanical.SubRules`,
    `NegativeCaseCoherence`, `EvidenceAdequacy`, `OptionalDispositionConformance` y `QuestionAnswers`, y en `ClosureRuleMechanical`,
    `A69Compatibility` y `DeltaPurity`;
  - ningún `NOT_DETERMINED` (en `ClosureRuleMechanical`, `A69Compatibility` y `DeltaPurity`) y `OwnerDecisionRequired` distinto de `null`;
  - todo REQUIRED citado en `FindingIds` por al menos un elemento evaluado.
- **Con AGREED:** cero REQUIRED; A62-A6R3-R1 en CORRECTED; `ClosureRuleMechanical` en TOTAL_AND_MECHANICAL, con los tres casos en COVERED y
  `JudgmentStepRequired` false y los cinco apartados en `no`/`no`; `A69Compatibility` en NONE; N4 y NC1 en COHERENT; los seis aspectos de
  `EvidenceAdequacy` en ADEQUATE; `DeltaPurity` en CONFIRMED; O1..O7 en CONFORMS; `OwnerDecisionRequired` = false; y los diez campos de `IfAgreed` en
  `true`. En cualquier otro caso, `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED o REJECTED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true. Con
  `OwnerDecisionRequired` = true, `AdditionalOwnerReservedConsequences` no vacía.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `Round3FindingResolution`, `NegativeCaseCoherence`, `EvidenceAdequacy`, `OptionalDispositionConformance`, `QuestionAnswers`, las listas de
    `ClosureRuleMechanical` y las listas de hallazgos pueden ir vacías.
  - Lo no evaluado va así: `NOT_DETERMINED` en `ClosureRuleMechanical`, `A69Compatibility` y `DeltaPurity`; `null` en `OwnerDecisionRequired`, en
    `IfAgreed` y en las booleanas de `Step0` que no comprobaste; `ReviewedCommit` y `ReviewedBlob` con lo observado o en `null`; los `Rationale`, «no
    evaluado».
  - `NOT_DETERMINED` y `null` en esos campos solo valen con NOT_ACCREDITED.