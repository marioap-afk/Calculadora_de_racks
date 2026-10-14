I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA RONDA 3 DE LA ENMIENDA A-6 (instrumento de hechos fijado por blob para el Controller de FX-02 en F6, B-min), preparada según la disposición del Coordinator §84 y la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento.

Eres el ARCHITECT de esta revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-6 y de sus tres rondas, de quienes escribieron el paquete B-min, del Coordinator, del dictamen de materialidad de B-min y de las revisiones de las rondas 1 y 2. Revisas en solo lectura la versión exacta de la ronda 3 de A-6 y devuelves un único veredicto formal (LIFECYCLE §5 y §6). No redactas texto normativo de sustitución, no implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261013T183701Z-9f80
InvocationId           = I20261013T183701Z-9f80
LogicalReviewRequestId = L20261013T183701Z-9f80   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-a6r3   (tu directorio de trabajo; HEAD separado en el commit, sin ramas locales; sin remoto)
Commit                 = 9b8b66a21050fe264a4ff4802295468a91fce5c7
Publicación ronda 3    = a128efef8f83890f8deae66c286c553bed2663e3   (A-6 ronda 3, PROPUESTA; padre 9cd074aa5668a59ef0030cb494f97ca2ca6333f5, con la A-6 de la ronda 2)
Guardas `run`          = 9a30c9a6503f4e48d22cd9f2486980eb56ad0030   (custodia del resultado `run` de las guardas de la ronda 3 sobre la publicación)
Objeto                 = docs/initiatives/I-62-A-6.md   blob e62f27b8b1329f1b9cfa69a707feb31d22bad1d1   (135 665 bytes, 1 320 líneas)
Ronda 2                = blob 86da0ae770e683a647f2724860a4bd890061154d, en el run como D:\r62-arch-a6r3-run\a6-r2.md (110 243 bytes, 1 145 líneas)
Revisión de la ronda 2 = R20261012T195511Z-e5c3: docs/automation/evidence/I-62-architect-A-6r2/R20261012T195511Z-e5c3/output.json (blob fc357d09…)
Revisión de la ronda 1 = R20261011T215735Z-c434: docs/automation/evidence/I-62-architect-A-6/R20261011T215735Z-c434/output.json (blob e90d685b…)
Paquete del Architect  = ninguno (este texto hace de paquete)
Orden vigente          = D:\r62-arch-a6r3-run\order.txt   (disposición §84 del Coordinator; 3 377 bytes, 85 líneas, SHA-256 8a6b560f1da473ff152732e2e6bf2b82cd60f2df4ddfc7e44f8359edc7bef2e8)
```

**Recibo.** El invocador comprobó que el objeto tiene el blob `e62f27b8…` en la publicación, en la custodia de las guardas y en el commit de arriba, y
que el padre de la publicación tiene el blob de la ronda 2 (`86da0ae7…`); que `docs/automation/evidence/I-62-A6/r3/` no cambia después de la
publicación salvo por el añadido del resultado `run` y de las copias legibles de `r3/readable/`; que decisiones solo crece por añadido desde el
registro de §84; que el paquete B-min, el dictamen, `r2/` y las revisiones de las rondas 1 y 2 conservan sus árboles y blobs; que `a6-r2.md` es,
byte a byte, el blob `86da0ae7…`; y que las guardas custodiadas de la ronda 3 reproducen sus resultados (`run` sobre la publicación y `self-test`).

**Condición de lanzamiento (compuerta 11).** Decisiones §84 (encabezado en la línea 1666; punto 7, línea 1679) autoriza una única revisión
formal de esta candidata con el kit congelado. Esta invocación existe porque el lanzador lo comprobó inmediatamente antes de lanzarla, en el registro
de decisiones de la rama de I-62: la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de §78 como línea completa; la línea `CLAUDE-CLI-I62 = A (…)`
vigente y el binario medido igual al del cierre; A-6 en la HEAD de la rama con el blob de arriba; §84 con su encabezado, el blob completo y el
SHA-256 de su texto fijados en el kit; y ninguna sección posterior a §84. No juzgas esa autorización ni la determinación del Owner; juzgas A-6,
incluido el alcance que fijan sus dos líneas (§7) y el alcance de VERIFY.

**Objeto y revisión.** Es la tercera revisión formal de A-6. La revisión de la ronda 2 terminó en CHANGES REQUIRED con dos REQUIRED (A62-A6R2-R1 y
A62-A6R2-R2) y cinco OPTIONAL (A62-A6R2-O1..O5), literales en su salida. Su auditor la dejó NOT_ACCREDITED por un único motivo, ajeno al contenido:
faltaba `run.json`, por un defecto de registro del lanzador. Decisiones §83, punto 1 (línea 1657), la conserva sin modificación, no concede
acreditación excepcional, no usa su dictamen como acuerdo formal y acepta R1 y R2 como hallazgos técnicos que hay que resolver; su punto 2 (línea
1658) limita los cambios de la ronda 3 a A6-3, regla 6, A6-5 y A6-7, con los mismos blobs del instrumento y del aceptante, y sus puntos 3 y 4
(líneas 1659 y 1660) fijan lo que debe conseguir cada corrección. Decisiones §84, punto 6 (`order.txt`, líneas 36-49; registro en la línea
1678), pide una revisión independiente y ACOTADA sobre:

- el cierre de A62-A6R2-R1;
- el cierre de A62-A6R2-R2;
- la coherencia de N4, nc1 y VERIFY sin instrumento;
- la reconstrucción desde eventos reales;
- el cierre efectivo de entradas;
- la compatibilidad con el aceptante fijado;
- la autorización del Owner y el alcance de VERIFY;
- los riesgos G1-G6;
- los hallazgos opcionales O1-O5.

§84, punto 6, añade: «Lo que dependa de una sesión real se mantiene como precondición con fallo cerrado, sin darlo por acreditado de antemano». Vale
para A-6 y para esta revisión: lo que solo una sesión real puede demostrar no lo das por demostrado, y lo que A-6 deja como precondición lo juzgas
como precondición (si está definida de forma verificable y si su falta produce un fallo cerrado). Sin acreditación excepcional: si no puedes
cumplir un punto con lecturas fieles, no lo des por cumplido.

Los demás aspectos de A-6 (los deltas que la ronda 3 no toca, la materialidad, la separabilidad, la compatibilidad con A-1..A-5, las superficies y
D.5) no se te piden en esta revisión acotada; la ronda 3 declara que no los cambia (§3.17, L1033-1074; §6.2, L1141-1154) y sus guardas lo comprueban
(G5). No se te prohíbe señalar otro defecto material que encuentres en cualquier sección de A-6: hazlo como hallazgo.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git`, las guardas, el instrumento de B-min, su aceptante, sus pruebas ni la evidencia de la ronda 3.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, sin ramas ni remoto, árbol limpio, los blobs del cierre y el
  SHA-256 de los archivos del run. El auditor la vuelve a verificar después. En `Step0` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. **Lectura directa de V14 (obligatoria).** Lee con Read `D:\r62-arch-a6r3\docs\initiatives\I-62-proposal-v14.md` con `offset` 2325 y
   `limit` 18: incluye las líneas 2333-2334 (Anexo C, «Esperado independiente»). Tiene que ser una llamada a Read sobre ese archivo:
   la cita que hace A-6 de esas líneas no es una lectura de V14. Regístrala en `DirectReads` (`docs/initiatives/I-62-proposal-v14.md:2333-2334`).
2. Lee entera, con Read, `D:\r62-arch-a6r3-run\order.txt` (la orden vigente, 85 líneas): puntos 2-4 (líneas 10-31) y punto 6 (36-49).
3. Lee con Read las líneas 1650-1682 de `D:\r62-arch-a6r3\docs\automation\decisions\I-62.md` (decisiones §83 y §84). Comprueba que la línea 1669
   declara el cuerpo pegado de §84 con 3 377 bytes y el SHA-256 de arriba, y que §84, punto 6 (línea 1678), registra los mismos nueve puntos que
   `order.txt`, líneas 36-49 (con la redacción del registro). En §83, punto 1 (línea 1657): la revisión de la ronda 2 conservada, CHANGES
   REQUIRED, auditor NOT_ACCREDITED, sin acreditación excepcional y R1 y R2 aceptados; puntos 3 y 4 (líneas 1659 y 1660): lo que pide cada
   corrección.
4. Lee entero, con Read, `D:\r62-arch-a6r3\docs\automation\evidence\I-62-A6\r3\a6r3-guards-result-run.json` (1446 líneas): las líneas 2-4 deben dar `Mode` RUN, `Base` = el padre de la publicación
   (`9cd074aa5668a59ef0030cb494f97ca2ca6333f5`) y `Head` = la publicación (`a128efef8f83890f8deae66c286c553bed2663e3`), y la línea 1445 el `Result` de conjunto PASS.
5. Lee con Read las líneas 6994-7019 de `D:\r62-arch-a6r3\docs\automation\evidence\I-62-A6\r3\a6r3-selftest.json` (`Summary`, `Result`, `Mode`, `Repo`, `ToolBlob` y `A6TextBlob`): `Result` PASS y
   `A6TextBlob` = el blob del objeto.
6. Lee con Read, en `D:\r62-arch-a6r3\docs\automation\evidence\I-62-architect-A-6r2\R20261012T195511Z-e5c3\output.json`, las líneas 2-404 (RunId
   R20261012T195511Z-e5c3, `ReviewedBlob` = `86da0ae7…`, `Verdict` CHANGES REQUIRED) y los hallazgos: REQUIRED R1 406-432; R2 433-461; OPTIONAL O1 464-475; O2 476-488; O3 489-500; O4 501-511; O5 512-522.

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`D:\r62-arch-a6r3-run\`):
- `order.txt`: la orden vigente (disposición §84; 3 377 bytes, 85 líneas). Su SHA-256 es el que declara decisiones §84 en el commit;
- `a6-r2.md`: el texto de A-6 de la ronda 2 (blob `86da0ae7…`), byte a byte: los hallazgos de la ronda 2 citan sus líneas. Más de 60 000 bytes: por
  tramos de 300 líneas como máximo. Puede ser referencia (`a6-r2.md:<línea>`);
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es referencia.

Todas las rutas que siguen son relativas al clon `D:\r62-arch-a6r3` y están en el commit de arriba.

**Objeto**, entero (más de 60 000 bytes: por tramos de 300 líneas como máximo): `docs/initiatives/I-62-A-6.md`: cabecera 1-64; 1. Alcance 66-94; 2. Cláusulas anteriores y fuentes 96-393; 3. Delta 395-1074; 3.1 Motivo 397-424; 3.2 Correspondencia con los ocho puntos del dictamen 426-440; 3.3 Identidad por blob de los instrumentos e insumos canónicos 442-463; 3.4 A6-1 465-504; 3.5 A6-2 506-526; 3.6 A6-3 528-595; 3.7 A6-4 597-623; 3.8 A6-5 625-700; 3.9 A6-6 702-716; 3.10 A6-7 718-761; 3.11 A6-8 763-783; 3.12 A6-9 785-968; 3.13 Prohibiciones 970-991; 3.14 Observaciones del dictamen 993-1003; 3.15 Efectos sobre D.5 y la Owner Validation 1005-1015; 3.16 Correcciones de la ronda 2 1017-1031; 3.17 Correcciones de la ronda 3 1033-1074; 4. Semántica antes y después 1076-1100; 5. Materialidad 1102-1117; 6. No cambia producción; compatibilidad con A-1..A-5 1119-1179; 6.1 Diff permitido y superficies prohibidas 1121-1139; 6.2 Compatibilidad con A-1..A-5 1141-1154; 6.3 Guardas mecánicas y preflight 1156-1179; 7. Consecuencias reservadas al Owner 1181-1215; 8. Disposición de las rondas 1 y 2 y preguntas de la ronda 3 1217-1252; 9. Separabilidad 1254-1263; 10. Evidencia 1265-1287; 11. Estado de revisión 1289-1299; Anexo 1301-1320. Lo que la ronda 3 dice
cambiar: §3.17 (L1033-1074: tabla de correcciones L1040-1052, filas R1 (a) L1042; R1 (b) L1043; R1 (c) L1044; R1 (d) L1045; R2 (a) L1046; R2 (b) L1047; O1 L1048; O2 L1049; O3 L1050; O4 L1051; O5 L1052; tabla de precondiciones de R2 L1057-1063, filas
reconstrucción L1059; comparación L1060; identidad L1061; cobertura L1062; rechazo L1063; límites de la reconstrucción de referencia L1065; riesgos G1-G6 en la ronda 3 L1067-1069), A6-3, regla 6
(L572-593), A6-5 (§3.8, L625-700) y A6-7 (§3.10, L718-761).

**Evidencia de la ronda 3** (67 archivos, enteros; evidencia de apoyo, no autoridad: guardas, autoprueba, resultado `run`, pruebas del aceptante
fijado (`r1probes/`), evidencia estática de R2 (`r2static/`), procedimiento de lanzamiento corregido (`launchfix/`), negativos y preflight). Los
resultados JSON de una sola línea (más de 2 000 caracteres) tienen una **copia legible** en `docs/automation/evidence/I-62-A6/r3/readable/` (misma ruta
relativa; líneas de 1 900 caracteres como máximo; `readable/MANIFEST.json` fija, para cada copia, el archivo canónico, su blob y la transformación):
lee la copia legible y **cita la ruta de la copia**, no la del JSON de una sola línea:

```text
docs/automation/evidence/I-62-A6/r3/a6r3-guards-result-preview.json                blob 91026b212aa5172cb11e799cbc202566d0e5f595  23 897 bytes  1 390 líneas
docs/automation/evidence/I-62-A6/r3/a6r3-guards-result-run.json                    blob c0810a3af34c8dceaa8944ff7ff572b81646985d  28 171 bytes  1 446 líneas
docs/automation/evidence/I-62-A6/r3/a6r3-guards.diff                               blob 7d931615f50c7e0b68d90abda8587105cc70883f  88 870 bytes  1 085 líneas
docs/automation/evidence/I-62-A6/r3/a6r3-guards.py                                 blob 103d69c62025b852a6b2233b1a55d78ba30f083d  204 992 bytes  3 111 líneas
docs/automation/evidence/I-62-A6/r3/a6r3-selftest.json                             blob e3a65b8c7737e461fd1d154503c55001a2186416  194 287 bytes  7 020 líneas
docs/automation/evidence/I-62-A6/r3/a6r3-sources-for-reviewer.json                 blob bfc4eacdab81a93d1767260420fd4337e673bd01  16 474 bytes  1 108 líneas
docs/automation/evidence/I-62-A6/r3/adapt_guards_r3.py                             blob c48503c5841cbd573d5f925802fcba9eea986af9  58 726 bytes  685 líneas
docs/automation/evidence/I-62-A6/r3/adapt_neg_r3.py                                blob 7164d3a8eda4b16d178d86b83ab22161fea607d9  11 992 bytes  87 líneas
docs/automation/evidence/I-62-A6/r3/launchfix/README.md                            blob f63a644096c627f5971312d6b51c06c5e2dbd282  21 588 bytes  178 líneas
docs/automation/evidence/I-62-A6/r3/launchfix/adapt_launch_r3.py                   blob 89240efd02b7c1a038407e6b3924d07172c5e8b3  33 908 bytes  525 líneas
docs/automation/evidence/I-62-A6/r3/launchfix/launch.r3-template.diff              blob f7041aab8ce770746917a2258d6550a8522947cc  30 334 bytes  457 líneas
docs/automation/evidence/I-62-A6/r3/launchfix/launch.r3-template.py                blob a7495745cb402e30fca689c2301e19bd7465583c  41 735 bytes  603 líneas
docs/automation/evidence/I-62-A6/r3/launchfix/test-results.json                    blob bdcc6740cebdb321bbe7adecd6a44bcb6b093c39  46 865 bytes  1 461 líneas
docs/automation/evidence/I-62-A6/r3/launchfix/test_launchfix.py                    blob cbb8b1311a71a03872a1a9c6d3775107ebb9c1bf  36 941 bytes  529 líneas
docs/automation/evidence/I-62-A6/r3/make_candidate_commit_a6r3.py                  blob aa32fd3f9bd16323c7a72010b27a198dcade87ff  7 139 bytes  109 líneas
docs/automation/evidence/I-62-A6/r3/neg-fixtures/I-62-A-6.r3.draft.md              blob d52d3969c34e569eed7646a80c8a91a0f24f66b3  133 200 bytes  1 318 líneas
docs/automation/evidence/I-62-A6/r3/neg-fixtures/a6r3-selftest.draft.json          blob d939fd91e6f5f5df8fd56f5c34570c72e5676f39  194 482 bytes  7 020 líneas
docs/automation/evidence/I-62-A6/r3/neg_controls_a6r3.json                         blob d68c8650bac9e437142a6b5c5b2091a758599c65  13 839 bytes  569 líneas
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
docs/automation/evidence/I-62-A6/r3/r2static/benchlib.py                           blob 0c627730035f11308ff842318b5688aa2d385808  7 457 bytes  196 líneas
docs/automation/evidence/I-62-A6/r3/r2static/part-a-events.json                    blob 3eeed656eee7743c3d2b643a827d4c6149f36342  100 721 bytes  1 líneas
docs/automation/evidence/I-62-A6/r3/r2static/part-b-pou.json                       blob e2776514f07e7903ed6a325c17d5450fdf97f868  55 026 bytes  1 líneas
docs/automation/evidence/I-62-A6/r3/r2static/part_a_events.py                      blob afba72f36ce4dbbc0b82ea6227f0c0d3dc7e91ce  19 735 bytes  337 líneas
docs/automation/evidence/I-62-A6/r3/r2static/pou_check.py                          blob 555a1abc29e20ebb07b5582015f7aa5303989929  24 195 bytes  446 líneas
docs/automation/evidence/I-62-A6/r3/r2static/run_pou.py                            blob 66d53ee2c15298e941118b128730b251a16dd0e4  20 579 bytes  380 líneas
docs/automation/evidence/I-62-A6/r3/r2static/summarize.py                          blob eee4b2fbf09c09a6b4328f2ab22bc08844dc5501  2 845 bytes  62 líneas
docs/automation/evidence/I-62-A6/r3/readable/MANIFEST.json                         blob 293aa256994a21815c81777200c48b1565915a85  5 198 bytes  78 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p1-zero.json                 blob 37d83735dc9dc7b1e444f890377c0798d3e9ca28  11 533 bytes  382 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p2-n4.json                   blob 5a1d7166bb61bd737c217034c6cfae54c1f01740  28 123 bytes  646 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p3-nc1.json                  blob 8481cfb7e71a6f7b18944b296c326f6f90ef01be  45 208 bytes  1 162 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p4-t1.json                   blob 948353e861c107e37219ab089aa5916e6fc41d9e  35 608 bytes  768 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p5-mutants.json              blob 6155cc7cc94da97c01f0f6dc228b6262a705704b  27 871 bytes  931 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/p6-reconstruct.json          blob f4f8e73606e1615c93b397c1d9ae06c9c72f305d  48 610 bytes  1 525 líneas
docs/automation/evidence/I-62-A6/r3/readable/r1probes/summary.json                 blob fb78cab88ec1b137551547f25bc15aebc9dd4a3b  30 758 bytes  1 073 líneas
docs/automation/evidence/I-62-A6/r3/readable/r2static/part-a-events.json           blob af2c2d25525d47762e789209581ab35c7d61df1c  131 157 bytes  4 301 líneas
docs/automation/evidence/I-62-A6/r3/readable/r2static/part-b-pou.json              blob 7d9d6ede5101544f1e7f7c98b286c9b561850b9c  70 270 bytes  2 511 líneas
docs/automation/evidence/I-62-A6/r3/text/make_r3_fills.py                          blob 20e2960c336531546b19b03487285781ab330273  5 142 bytes  68 líneas
docs/automation/evidence/I-62-A6/r3/text/make_r3_text.py                           blob c62e2604832dbe99b66888413d14d99e2a303c7b  59 922 bytes  526 líneas
```

**Evidencia de la ronda 2** (159 archivos, enteros; igual que en el cierre de la ronda 2, con sus copias legibles en
`docs/automation/evidence/I-62-A6/r2/readable/`, que se leen y se citan del mismo modo):

```text
docs/automation/evidence/I-62-A6/r2/a6r2-guards-result-preview.json                         blob 18460b9ce3f0f75c5224198a9c260bcdf67c9fca  20 889 bytes  1 190 líneas
docs/automation/evidence/I-62-A6/r2/a6r2-guards-result-run.json                             blob 848324c8071f8550fd854f21d861209c4dc05123  30 267 bytes  1 312 líneas
docs/automation/evidence/I-62-A6/r2/a6r2-guards.py                                          blob dea3db710fb80c23993ad9e8ed903243aa47cfbb  184 704 bytes  2 880 líneas
docs/automation/evidence/I-62-A6/r2/a6r2-selftest.json                                      blob 80196cb348ec79b2ec900968ddc72f4b4fc6c3f8  163 927 bytes  5 940 líneas
docs/automation/evidence/I-62-A6/r2/a6r2-sources-for-reviewer.json                          blob 9c4d1ed2a47fe288cb23fa126667130fefcad1dc  13 537 bytes  917 líneas
docs/automation/evidence/I-62-A6/r2/independence/README.md                                  blob 72c9a7ebfb9ece9c38a2e998ea8f01e568f1ec81  35 217 bytes  349 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/a-mutation.json                    blob 4208d2818f0564255995f89775df42af9b3eff66  143 422 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/b-clausemap.json                   blob c3a6f0c3b6da531239f56cd3230abeef70317ace  15 318 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/build.json                         blob 735100b4e10b8ed414a583f051bd2da1d0010e7f  32 063 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/c-r4.json                          blob e132f401590b449475f85801bdac9529debd6825  88 043 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/d-closure.json                     blob eca5d38d28d7a5c95981aa77fd0d5f5aed6e8b01  25 043 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/e-review.json                      blob aa6d277262108335955046b4f4961aaf00b95e0c  12 277 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/results/manifest.json                      blob 590366246857de92d17eba75611b18e190aa6cc8  1 986 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/build_repos.py                     blob 85dac3f5ec3c1b6dbd40d03f9eaddca0266b6275  18 544 bytes  375 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/finalize.py                        blob 82652d5a459df497b53c11700457c9b2e8061249  2 352 bytes  59 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/part_a.py                          blob 2fdd9e8c365518c48645e0b2cc9e5e6375334cd1  31 441 bytes  522 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/part_b.py                          blob bc7de2bfa50d30bb71fb91898e7a6c2ac509c3c5  24 003 bytes  379 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/part_c.py                          blob d100062e2e041fafa17b325c8de34981d7e00718  12 618 bytes  193 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/part_d.py                          blob e75059bb3ef3b42458487e1a5250c3421d46ca74  21 128 bytes  323 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/r3lib.py                           blob 33c403cf39962aa8ce641e1bbf78a3bdb167a024  20 123 bytes  503 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/r3model.py                         blob bc266bdd6dd4ec3786d56e063555c3e1d335536c  17 267 bytes  327 líneas
docs/automation/evidence/I-62-A6/r2/independence/scripts/review_e.py                        blob 7a9a81c219bf1adef6b53c7bb13e7f3e43557f62  14 766 bytes  220 líneas
docs/automation/evidence/I-62-A6/r2/make_candidate_commit_a6r2.py                           blob a325e16bd3d03ec7f2a297b9c1a5c90f89fc1164  6 988 bytes  108 líneas
docs/automation/evidence/I-62-A6/r2/neg-fixtures/I-62-A-6.r2.draft-0304.md                  blob 5fed7efbad9e551c0647462dc91a6161c21a8c7e  93 912 bytes  976 líneas
docs/automation/evidence/I-62-A6/r2/neg-fixtures/a6r2-selftest.draft-0304.json              blob 928f6dfd8ddfcab4eadd39a1c72b35cce649c601  165 193 bytes  5 952 líneas
docs/automation/evidence/I-62-A6/r2/neg_controls_a6r2.json                                  blob 9213326ed538b73b45cdb7dfe748a86a9ef46464  11 008 bytes  451 líneas
docs/automation/evidence/I-62-A6/r2/neg_controls_a6r2.py                                    blob 280e5b7f19d429d552fe1cc28d5119d50014858c  12 912 bytes  211 líneas
docs/automation/evidence/I-62-A6/r2/preflight.md                                            blob 1854cacc03401ddb9e730565a9ec4b5910f0db99  7 941 bytes  92 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/README.md                                          blob aa7f54c0077465f21fb821d12b57bbc347e2e4d7  21 865 bytes  204 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/inputs-t1/controller-output.json                   blob e017222b5fd6f5d228bcfdecaef58d7cafeb0349  8 610 bytes  171 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/inputs-t1/deleg.json                               blob 3570e9c85ab4d610156070797240437f71feb35c  6 180 bytes  226 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/inputs-t1/handoff.json                             blob 148d5f7143c30d09247bf662cb24530732c5da7a  1 379 bytes  55 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/inputs-t1/relay-facts.json                         blob b8b476ea332b2519c8cc4dc496e62a2b962fc6e2  2 098 bytes  75 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/inputs-t1/request.json                             blob 99e18962419895e2aabfd111d96892c2b59c4c4b  6 500 bytes  249 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/lib_r1r4.py                                        blob a623a33f9b808c0e080d2f84fb545089d32864ac  37 661 bytes  731 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/r1-attacks-extra.json                              blob 0176d27dbeb8d38bbe2ab7bd9a9200df86f6f8ef  10 056 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/r1-attacks.json                                    blob a47c3da0f6ae0ecfe35e2c7c9f0cba5e0b70a8e4  98 661 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/r1-p1-p2-p3.json                                   blob b9eddfbc00f3c3efef7a39444f9f7b0309863ad9  26 790 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/r4-t1.json                                         blob cda60496bff26b1932c843c684e235550d8949ee  24 487 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/r4-t2.json                                         blob 1829bfb40f4f966023b2f76184f4f2988861fdd4  18 926 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/r4-t3.json                                         blob d105103f76d6bcc339ff0e9044d73ab5463bf6ec  13 269 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/r4-t6.json                                         blob e05a361505d3ddaec3a9a56dc76b5e38c82c53fb  17 611 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/run_r1.py                                          blob eff697845fc2c01170121bd0b28886d1fa20f781  31 053 bytes  488 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/run_r1_extra.py                                    blob 1e6286df44a7a0e4f95e99b7e5a14250c79cfc4e  4 919 bytes  76 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/run_r4.py                                          blob 029ef19709c7cb208e31df7d98afca7933b937ed  29 865 bytes  473 líneas
docs/automation/evidence/I-62-A6/r2/r1r4/summarize.py                                       blob 683b99aaba3b3f29c952e61b44250e3c4fe1cd5b  2 962 bytes  66 líneas
docs/automation/evidence/I-62-A6/r2/readable/MANIFEST.json                                  blob 392f265f38506872529e86f473ae03dfc99678f9  15 292 bytes  222 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/a-mutation.json           blob 9e551f2fa1e2a260cda2fe6fa65dd7191cae5594  212 023 bytes  9 227 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/b-clausemap.json          blob c052af8c53f752b359a4efa8376a91fbbe6041eb  19 473 bytes  796 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/build.json                blob 2df963f067537aa5fcc89a92aa1c743c278ac45f  40 773 bytes  1 232 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/c-r4.json                 blob 454a884a71e7483910184b41e8a706c4987c3502  122 395 bytes  4 269 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/d-closure.json            blob b08e615963db6d6a59f184ce001b5bbe286e606e  31 485 bytes  853 líneas
docs/automation/evidence/I-62-A6/r2/readable/independence/results/e-review.json             blob 4009d425df0f69037c0e67a3ef3c3cb9f4e8e571  14 610 bytes  427 líneas
docs/automation/evidence/I-62-A6/r2/readable/r1r4/r1-attacks-extra.json                     blob 6b108a6102c2eb4fb8e13899d37f91a5fb0fee01  12 254 bytes  335 líneas
docs/automation/evidence/I-62-A6/r2/readable/r1r4/r1-attacks.json                           blob 1c0d5602f7e1b95a7f894108cb474aeebb0ed166  122 138 bytes  3 235 líneas
docs/automation/evidence/I-62-A6/r2/readable/r1r4/r1-p1-p2-p3.json                          blob e700eb280b85685ee086b5d462e2369ce1c81348  30 733 bytes  677 líneas
docs/automation/evidence/I-62-A6/r2/readable/r1r4/r4-t1.json                                blob da614688405996822a35695bd18e2c332e952ce2  29 392 bytes  799 líneas
docs/automation/evidence/I-62-A6/r2/readable/r1r4/r4-t2.json                                blob b598ec3b61e5febad89dce0b94c549224f2c1ed2  22 507 bytes  650 líneas
docs/automation/evidence/I-62-A6/r2/readable/r1r4/r4-t3.json                                blob f8d7c09c01446ad724619f1b95ac048522351a2b  15 008 bytes  317 líneas
docs/automation/evidence/I-62-A6/r2/readable/r1r4/r4-t6.json                                blob 7494cced5fcc65cd13b09d0f88359ced1179e431  19 714 bytes  368 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/_work/r3-interrupted/results/build.json  blob b5dc545af61a265c8185bde4f57b1237580af187  40 935 bytes  1 232 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r1-attacks-extra.json               blob ffe3b504f215f4d733c8a6af3ac06be2759e43c2  12 382 bytes  335 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r1-attacks.json                     blob 5048b6025c6d0bae4efdd0244abf4a08993b7a43  119 775 bytes  3 108 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r1-p1-p2-p3.json                    blob 82cadd90cbc52307f278de12fa58f226196d46fd  31 246 bytes  677 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r4-t1.json                          blob 8bb4ba11e68be885819c146b3dbe4791abdb9f3e  29 629 bytes  799 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r4-t2.json                          blob afb73ab9133fbe95372939f7a6d2cb2743b7009e  22 989 bytes  660 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r4-t3.json                          blob 8e47abd4df8d54451873d9ca15f6c5a678e7f741  15 653 bytes  337 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r4-t6.json                          blob be86a1d63d29ab36d3f88c7b7e86839a2aaf934a  19 237 bytes  348 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/a-mutation.json               blob 9b01900483d41dab0196a6f718cebd59c7c5168b  212 444 bytes  9 227 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/b-clausemap.json              blob 105dcfd8fd73cb50dca46df0dd5f39669adfcc32  19 473 bytes  796 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/build.json                    blob b5dc545af61a265c8185bde4f57b1237580af187  40 935 bytes  1 232 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/c-r4.json                     blob 53bdc0c62e1cafdf6ad2b34d54d2e37022aff860  122 395 bytes  4 269 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/d-closure.json                blob ae241c7ad87128d386885f7c570b7a849e3ddfe7  31 515 bytes  853 líneas
docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/e-review.json                 blob 4009d425df0f69037c0e67a3ef3c3cb9f4e8e571  14 610 bytes  427 líneas
docs/automation/evidence/I-62-A6/r2/render/a6r2.src.md                                      blob acebaa0d116965c14402eb7fcb65054407c5bace  59 218 bytes  621 líneas
docs/automation/evidence/I-62-A6/r2/render/fills_ev/DERIVATION_TABLE.md                     blob 5f1988092bb412cb4210b6467743c61f31c95b79  2 557 bytes  29 líneas
docs/automation/evidence/I-62-A6/r2/render/fills_ev/GUARDS.md                               blob cb6d6e6e79a32d848ff6d25883e5899d5f0ec189  1 707 bytes  21 líneas
docs/automation/evidence/I-62-A6/r2/render/fills_ev/INDEPENDENCE_EVIDENCE.md                blob e9cc4b8fe38e40cd1e4927c1180e4f94b3171b6b  8 940 bytes  105 líneas
docs/automation/evidence/I-62-A6/r2/render/fills_ev/OWNER_NOTE.md                           blob 96667428458936be94119762b2ea5856fe56e891  231 bytes  2 líneas
docs/automation/evidence/I-62-A6/r2/render/fills_ev/OWNER_SECTION_NOTE.md                   blob 98b0cc58f37c84f00af0b2644b428ab2e3774632  503 bytes  4 líneas
docs/automation/evidence/I-62-A6/r2/render/make_fills.py                                    blob e740dbdfed78f43f3186b7dc0085edea8526a39d  3 326 bytes  29 líneas
docs/automation/evidence/I-62-A6/r2/render/render_a6r2.py                                   blob 1c46d596de8f769b6e0d4bfe449bec6acda2bfea  4 887 bytes  93 líneas
docs/automation/evidence/I-62-A6/r2/repro/README.md                                         blob a1d6996070a2ea1a4b12445893003c1d1faf440a  7 789 bytes  102 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/cmp-r1r4.json                               blob 812ae75bcbbeb80d43a810e486021521ef3ada39  319 433 bytes  7 034 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/cmp-r3.json                                 blob eaf1bef3b8c2a8e3978fb5bce098edb6a8a0bae2  54 340 bytes  1 301 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/finalize-repro.json                         blob dcee32470908684e5e2dce61e8efe2f7f36ab8a1  7 196 bytes  484 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/guard-after.json                            blob e7083201755adeee1f51bba7362cbc2bbfbebdaf  759 bytes  14 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/guard-before.json                           blob 39a840f8bf3ee86748f0015ca8c40e68a99731db  759 bytes  14 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/guard.py                                    blob 95df35101a88852150482168cfeeffe0cbd8fbf0  1 485 bytes  21 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/lib_r1r4.diff                               blob 1433864e56ed39ff7af2894fb88a1707355f1cc5  377 bytes  11 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/package-hashes-before.txt                   blob f7cf15a612f8ce7ae5bbb80235081a3ed8f77b2c  3 082 bytes  35 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/pairs-r3.json                               blob 96b0209ea0a790fbbe3fe861db9ce4b86ccdc167  600 bytes  10 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3-build_repos.log                          blob 708c348ac3e7eb9774affbf96f81efc9fad7f830  67 bytes  2 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3-interrupted/r3-build_repos.log           blob 074cc42692d5ac9d53c7b56818e937e145660e42  296 bytes  3 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3-interrupted/r3-part_a.log                blob d2b33c42138d1d92377a76febf71704908131c5c  5 812 bytes  94 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3-interrupted/r3-times.log                 blob 7d38b5be46c5b1b8092bead94b3b28d0f0b40217  167 bytes  5 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3-interrupted/results/build.json           blob b6a80ecfb671e9c6b7b36adca37cf6ebb604ad92  32 225 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3-times.log                                blob 561fdb42c0f3870eed08085bfc7725ac230672cb  39 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3c-build_repos.log                         blob 074cc42692d5ac9d53c7b56818e937e145660e42  296 bytes  3 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3c-finalize.resume.log                     blob 90f71edc1211cead5a86e3a94990308f4ceb72ab  1 380 bytes  17 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3c-part_a.log                              blob 84e34fe799474f33fc6e0b6b407529d5d55cc24d  1 637 bytes  48 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3c-part_a.resume.log                       blob 18b0ebd06ccce6cea208332b9a8e9913ea01b130  7 176 bytes  107 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3c-part_b.resume.log                       blob d0b7fe7e363fd143d2ef56af5754b6dbbd5c2d08  4 228 bytes  30 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3c-part_c.resume.log                       blob cee91896ed63c85beffcdf48c16a23b5f9911af0  3 273 bytes  32 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3c-part_d.resume.log                       blob ecec11cbbbac48409ced5cf41d3b48624b3ea856  1 589 bytes  7 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3c-review_e.resume.log                     blob da1ed0a958e526d9a7b15d7448354a8fe8a7818b  606 bytes  4 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/r3c-times.log                               blob 7e6b2acb55bc7d8dda7e9e4223a4b026494de92d  764 bytes  18 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/regen_extra.log                             blob fafaa6c208eeb2c849758f25a5faa3d0b59a5572  584 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/regen_t0.log                                blob dabef2ba30b7734b6e0a1e9e6c8b9cb4e4e7e1fe  1 704 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/run_r1.end                                  blob abf683648dca39cedc23c17755bbd7f991c30297  21 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/run_r1.log                                  blob 1de215ff1c4a106ba34ccd75cd4088dc154e2d2d  2 789 bytes  14 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/run_r1.start                                blob 337c550b72dc2d21a2c91e76602a80a452cb0027  21 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/run_r1_extra.end                            blob 453e29d5147d468c7b50ffd2773a3a8b1a090483  21 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/run_r1_extra.log                            blob 6e2a1c79483fc63ee6f48526f0a8ed0cf4c38666  870 bytes  3 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/run_r1_extra.start                          blob 1c6d31cdb778de6d1c74811fa2a1d9f22fbd4528  21 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/run_r4.end                                  blob de62bbf87197d8d3f2ba2e911d649cda9de0ecd0  21 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/run_r4.log                                  blob b92f465caa64451326024947bd635fe8b8918ead  1 623 bytes  10 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/run_r4.start                                blob 09e0db5d8ec716c7b270ca29a1a86d344419440a  21 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/summarize-hashes.log                        blob d88231fb774ba8d517e13487b6e5b7274b0f7b7c  1 392 bytes  17 líneas
docs/automation/evidence/I-62-A6/r2/repro/_work/summarize.log                               blob ea0a9a10dbb528d06f839068271105da907d1c9a  13 182 bytes  76 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/inputs-t1/controller-output.json             blob ee079a2ae1cb0bef9a14efa1e2e378c55c4b3d32  8 694 bytes  171 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/inputs-t1/deleg.json                         blob 3d90bc4990161af354313220346c87327f8f3d0b  6 186 bytes  226 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/inputs-t1/handoff.json                       blob 7b6ae0a898ef34c536ed699f5f895e9acaa3b2eb  1 385 bytes  55 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/inputs-t1/relay-facts.json                   blob 5869201481a67acaabc61c287678aeda625f8f39  2 104 bytes  75 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/inputs-t1/request.json                       blob 0d61e678254677e86c6470a00076378921cb611b  6 740 bytes  249 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/lib_r1r4.py                                  blob 77f40db00296d31130f34c9d9b34b8862cb84c8d  37 667 bytes  731 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/r1-attacks-extra.json                        blob 879eb1c02dad23925cb387ec04ddf912d10c0b03  10 184 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/r1-attacks.json                              blob 02bd6d6a2a6d925ce94917ba3acfaa8df858f04c  97 408 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/r1-p1-p2-p3.json                             blob 10a750cc3ae10a64095ee856fda40458ba3b3788  27 303 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/r4-t1.json                                   blob 548a67d5c9b3a8f337711a0d532cfee996e6e1d2  24 724 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/r4-t2.json                                   blob 2fab5dca2f1b302b81bb21f414cb6bf9c627a928  19 335 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/r4-t3.json                                   blob 56670dce6adb68840d13c1fd492e63ecd8e3995f  13 768 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/r4-t6.json                                   blob 6e8940b334bfb4a2fe977bc0fce86fcd9ea7891f  17 280 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/run_r1.py                                    blob eff697845fc2c01170121bd0b28886d1fa20f781  31 053 bytes  488 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/run_r1_extra.py                              blob 1e6286df44a7a0e4f95e99b7e5a14250c79cfc4e  4 919 bytes  76 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/run_r4.py                                    blob 029ef19709c7cb208e31df7d98afca7933b937ed  29 865 bytes  473 líneas
docs/automation/evidence/I-62-A6/r2/repro/r1r4/summarize.py                                 blob 683b99aaba3b3f29c952e61b44250e3c4fe1cd5b  2 962 bytes  66 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/README.md                                      blob 72c9a7ebfb9ece9c38a2e998ea8f01e568f1ec81  35 217 bytes  349 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/results/a-mutation.json                        blob 3c393d5c390720a7074b6358e9647c06eb2b4205  143 843 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/results/b-clausemap.json                       blob 3a4460d3bf837d27c36a022640ead0778ce1b10e  15 318 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/results/build.json                             blob b6a80ecfb671e9c6b7b36adca37cf6ebb604ad92  32 225 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/results/c-r4.json                              blob 90a446f57a8149f2930ba1d1de4dad8370839491  88 043 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/results/d-closure.json                         blob 7946ee32e5d77fd53f09fe87f049648a5d02bdb6  25 073 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/results/e-review.json                          blob aa6d277262108335955046b4f4961aaf00b95e0c  12 277 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/results/manifest.json                          blob d7b1f6b2e2d6a838159fe888547b78cb48b57bee  1 986 bytes  1 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/scripts/build_repos.py                         blob 85dac3f5ec3c1b6dbd40d03f9eaddca0266b6275  18 544 bytes  375 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/scripts/finalize.py                            blob 82652d5a459df497b53c11700457c9b2e8061249  2 352 bytes  59 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/scripts/part_a.py                              blob 2fdd9e8c365518c48645e0b2cc9e5e6375334cd1  31 441 bytes  522 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/scripts/part_b.py                              blob bc7de2bfa50d30bb71fb91898e7a6c2ac509c3c5  24 003 bytes  379 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/scripts/part_c.py                              blob d100062e2e041fafa17b325c8de34981d7e00718  12 618 bytes  193 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/scripts/part_d.py                              blob e75059bb3ef3b42458487e1a5250c3421d46ca74  21 128 bytes  323 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/scripts/r3lib.py                               blob 33c403cf39962aa8ce641e1bbf78a3bdb167a024  20 123 bytes  503 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/scripts/r3model.py                             blob bc266bdd6dd4ec3786d56e063555c3e1d335536c  17 267 bytes  327 líneas
docs/automation/evidence/I-62-A6/r2/repro/r3/scripts/review_e.py                            blob 7a9a81c219bf1adef6b53c7bb13e7f3e43557f62  14 766 bytes  220 líneas
docs/automation/evidence/I-62-A6/r2/repro/tools/assemble.py                                 blob 3c588e03b5ce6fb8809b185068b4a1b2c465624f  5 452 bytes  80 líneas
docs/automation/evidence/I-62-A6/r2/repro/tools/compare.py                                  blob fa083f88b3db9c936c2174efc571a90ac3a6e56e  24 550 bytes  547 líneas
docs/automation/evidence/I-62-A6/r2/repro/tools/meta.py                                     blob fac26a4460a4a4342f9ab1caa090b7e6c65cfefb  5 478 bytes  80 líneas
docs/automation/evidence/I-62-A6/r2/repro/tools/regen_extra.py                              blob e5ab63b81b71137452be5f2c9d1120c2a7ac9fdf  3 733 bytes  68 líneas
docs/automation/evidence/I-62-A6/r2/repro/tools/regen_t0.py                                 blob 9555851a25b4543f38cd733601f1250b99616e39  1 653 bytes  28 líneas
```

**Paquete B-min** (30 archivos, enteros; para la compatibilidad con el aceptante fijado lee `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py` (2 816 líneas, por
tramos de 300); para la independencia, el instrumento, las notas, las pruebas y los resultados):

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

**Dictamen de materialidad de B-min y revisiones de las rondas 1 y 2** (salidas estructuradas; la de la ronda 2 trae A62-A6R2-R1, A62-A6R2-R2 y
A62-A6R2-O1..O5 literales y su valoración de G1-G6, líneas 845-925):

```text
docs/automation/evidence/I-62-architect-bmin/R20261011T162328Z-5c5e/output.json   blob 96c3efbc81e9c6814b232774fd8ac7e900deda7d  50 868 bytes  805 líneas
docs/automation/evidence/I-62-architect-bmin/R20261011T162328Z-5c5e/README.md     blob 8ce105e0c2c6efd18eb720799492fdca0b4f7315  30 287 bytes  293 líneas
docs/automation/evidence/I-62-architect-A-6/R20261011T215735Z-c434/output.json    blob e90d685bb829fc7d30eda9bdb05deb3ac66d24f5  57 232 bytes  696 líneas
docs/automation/evidence/I-62-architect-A-6r2/R20261012T195511Z-e5c3/output.json  blob fc357d09f1d345fa5ad55ca5aeffb6dc7510abaa  93 930 bytes  1 601 líneas
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
11. `docs/automation/agent-execution/README.md` (60 779 bytes, 756 líneas; blob `592dcfd4`), §5, §8, §10, §14 y §15: §5 132-150; §8 170-188; §10 217-235; §14 369-531; §14.1 375-383; §14.2 385-403; §14.3 405-427; §14.4 429-437; §14.5 439-486; §14.6 488-498; §14.7 500-531; §15 533-603; §15.1 537-551; §15.2 553-568; §15.3 570-583; §15.4 585-603; §8, salida inválida (INVALID_OUTPUT) 184; §15.2, regla I2 561; §15.2, regla I6 y auditoría de lecturas 565-568.
12. `docs/automation/agent-execution/routing.md` (10 294 bytes, 142 líneas; blob `bba08fc4`), entero: cabecera 1-7; §1 9-23; §2 25-34; §3 36-45; §4 47-59; §5 61-81; §6 83-88; §7 90-93; §8 95-119; §9 121-142.
13. `docs/automation/agent-execution/adapters/codex-cli.md` (4 333 bytes, 59 líneas; blob `155f3469`), entero: cabecera 1-16; Operaciones 18-30; Introspección 32-36; Huella 38-41; Hechos (`Facts`) 43-53; Límites 55-59.
14. `docs/automation/agent-execution/schemas/input-closure.v1.schema.json` (6 911 bytes, 282 líneas; blob `6bdf7184`), entero.
15. `docs/automation/agent-execution/schemas/role-invocation.v1.schema.json` (21 758 bytes, 858 líneas; blob `d54a7ae7`), entero.
16. `docs/automation/agent-execution/schemas/controller-verification.v2.schema.json` (16 528 bytes, 681 líneas; blob `e1faa9ec`), entero.
17. `docs/automation/agent-execution/schemas/delegation.v2.schema.json` (25 471 bytes, 1 002 líneas; blob `dfdbe461`), entero.
18. `docs/automation/agent-execution/schemas/worker-handoff.schema.json` (3 814 bytes, 81 líneas; blob `cc3c9cf7`), entero.
19. `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md` (24 984 bytes, 217 líneas; blob `90929ad1`), entero: cabecera 1-15; §1 17-30; §2 32-95; §2.1 34-65; §2.2 67-74; §2.3 76-84; §2.4 86-95; §3 97-131; §3.1 99-115; §3.2 117-131; §4 133-166; §5 168-179; §6 181-199; §7 201-217.
20. `docs/automation/decisions/I-62.md` (274 515 bytes, 1 682 líneas; blob `be5e376f`), §43, §57, §61, §64 y §73 en adelante: §43 812-824; §57 1105-1117; §61 1191-1204; §64 1274-1304; §73 1479-1498; §74 1500-1513; §75 1515-1529; §76 1531-1546; §77 1548-1562; §78 1564-1584; §79 1586-1599; §80 1601-1615; §81 1617-1632; §82 1634-1648; §83 1650-1664; §84 1666-1682; §83, bytes y SHA-256 del cuerpo pegado 1653; §83, punto 1 1657; §83, punto 2 1658; §83, punto 3 1659; §83, punto 4 1660; §83, punto 5 1661; §83, punto 6 1662; §83, punto 7 1663; §83, punto 8 1664; §84, bytes y SHA-256 del cuerpo pegado 1669; §84, punto 1 1673; §84, punto 2 1674; §84, punto 3 1675; §84, punto 4 1676; §84, punto 5 1677; §84, punto 6 1678; §84, punto 7 1679; §84, punto 8 1680; §84, punto 9 1681; §84, punto 10 1682; §78, línea del Owner A6-ADR0046-9-FX02 1572.
21. `docs/automation/evidence/I-62-evidence.md` (378 315 bytes, 4 320 líneas; blob `f01344fe`), §140 en adelante (citadas por A-6): §140 3780-3820; §141 3822-3830; §142 3832-3852; §143 3854-3880; §144 3882-3936; §145 3938-3945; §146 3947-3972; §147 3974-4014; §148 4016-4025; §149 4027-4048; §150 4050-4062; §151 4064-4083; §152 4085-4092; §153 4094-4127; §154 4129-4134; §155 4136-4143; §156 4145-4151; §157 4153-4172; §158 4174-4197; §159 4199-4224; §160 4226-4233; §161 4235-4268; §162 4270-4280; §163 4282-4288; §164 4290-4312; §165 4314-4320.

**Evidencia de I-62** (`docs/automation/evidence/I-62-evidence.md`, en la lista de canónicos). Está en el cierre, como en la ronda 2, porque
decisiones §79, punto 5, exige acceso directo a toda fuente que el texto cite y A-6 cita sus secciones (rangos citados: L3780-L3820, L3822-L3830, L3832-L3852, L3854-L3880, L3882-L3936, L3938-L3945, L3947-L3972, L3974-L4014, L4016-L4025, L4027-L4048, L4050-L4062, L4064-L4083, L4085-L4092, L4094-L4127, L4129-L4134, L4136-L4143, L4145-L4151, L4153-L4172, L4174-L4197, L4199-L4224, L4226-L4233, L4235-L4268, L4270-L4280, L4282-L4288). Es el
registro de la sesión que preparó A-6 y este kit: contiene sus resúmenes y valoraciones. Úsala como evidencia de hechos que puedes contrastar con
los archivos custodiados y con el texto normativo, nunca como autoridad ni como juicio sobre A-6; no adoptes sus conclusiones sin comprobarlas. Sus
§156, §158 y §162 (y decisiones §81 y §82, punto 6) tratan de C1-C3 y de I-65, ajenos a esta revisión.

Nada más. Quedan fuera del cierre, en particular: `docs/automation/evidence/I-62-C1C3/` (C1-C3 y la nota de aislamiento de I-65: ajenos a esta
revisión); la evidencia de la ronda 1 fuera de `r2/` y `r3/`; el resto de los directorios de las revisiones de las rondas 1 y 2 (`audit.json`,
`README.md`, `kit/`, `launch/` y los registros del lanzamiento); la tarjeta y el paquete OWNER-RESERVED de ADR-0046 #9 y la solicitud de medición
(`docs/automation/evidence/I-62-F6/requests/`); las transcripciones y la memoria; `probe_tools.py` y los `reference.json`/`comparison.json` de las
sondas; el mapa de cláusulas y su esquema, WORKFLOW, FOUNDATIONS, `state/I-62.yml` y el fixture. Lo que A-6 cite de esos archivos no lo puedes
comprobar: si una conclusión tuya depende de ello, decláralo en `KnownLimitations`.

## Lista de lectura: fuentes que cita A-6 (G11)

Las guardas de la ronda 3 (G11, en el resultado `run` custodiado; la misma lista que `r3/a6r3-sources-for-reviewer.json`) registran todas las
fuentes y rangos de líneas que cita el texto de A-6. Úsala como lista de comprobación: antes de apoyarte en una cita de A-6 (un bloque «> », una
tabla, «V14 L…», «AP L…», «RAE L…», «DEC L…», `ruta:línea`), lee la fuente en esas líneas con Read y contrástala.

- `docs/AUTOMATION_PLAN.md` (blob `f525cb1e`): L520-L523, L535-L538, L541-L543, L610-L611, L622-L625, L623, L625, L646, L653, L676-L677, L686-L687, L910-L918, L1093, L1204, L1204-L1207, L1221, L1236-L1237, L1237, L1240, L1240-L1241, L1244-L1251, L1251, L1264, L1267, L1271-L1276, L1279-L1280, L1287-L1288
- `docs/INITIATIVE_LIFECYCLE.md` (blob `f19896a8`): L94, L221
- `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` (blob `da275e1a`): L61, L69, L78
- `docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md` (blob `e1bd8d91`): L40, L91-L92
- `docs/automation/agent-execution/README.md` (blob `592dcfd4`): L219-L226, L220, L225-L226, L228, L561, L565-L568, L578
- `docs/automation/decisions/I-62.md` (G11 sobre el blob `6503ca85`, prefijo exacto del blob del cierre `be5e376f`: mismas líneas): L812-L824, L1105-L1117, L1191-L1204, L1274-L1304, L1479-L1664
- `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/ACCEPT-NOTES.md` (blob `cb9df028`): L41-L42
- `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py` (blob `434ef1fc`): L78, L79, L80, L81, L82, L83, L1884-L1917, L1954-L1998, L2001-L2025, L2134-L2197, L2202-L2215, L2218-L2244, L2247-L2276, L2279-L2283, L2286-L2320, L2323-L2327, L2542-L2609, L2612-L2619, L2622-L2623, L2675-L2699, L2801-L2812
- `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md` (blob `90929ad1`): L19-L23, L44-L45
- `docs/automation/evidence/I-62-evidence.md` (G11 sobre el blob `374f97aa`, prefijo exacto del blob del cierre `f01344fe`: mismas líneas): L3780-L3820, L3822-L3830, L3832-L3852, L3854-L3880, L3882-L3936, L3938-L3945, L3947-L3972, L3974-L4014, L4016-L4025, L4027-L4048, L4050-L4062, L4064-L4083, L4085-L4092, L4094-L4127, L4129-L4134, L4136-L4143, L4145-L4151, L4153-L4172, L4174-L4197, L4199-L4224, L4226-L4233, L4235-L4268, L4270-L4280, L4282-L4288  [registro de la sesión que preparó A-6: evidencia, no autoridad]
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
  - usa la ruta absoluta, solo de archivos del cierre bajo `D:\r62-arch-a6r3` o de los tres archivos del run;
  - en los archivos de más de 60 000 bytes (`docs/initiatives/I-62-A-6.md`, `docs/automation/evidence/I-62-A6/r3/a6r3-guards.diff`, `docs/automation/evidence/I-62-A6/r3/a6r3-guards.py`, `docs/automation/evidence/I-62-A6/r3/a6r3-selftest.json`, `docs/automation/evidence/I-62-A6/r3/neg-fixtures/I-62-A-6.r3.draft.md`, `docs/automation/evidence/I-62-A6/r3/neg-fixtures/a6r3-selftest.draft.json`, `docs/automation/evidence/I-62-A6/r3/r1probes/run_r3.py`, `docs/automation/evidence/I-62-A6/r3/r2static/part-a-events.json`, `docs/automation/evidence/I-62-A6/r3/readable/r2static/part-a-events.json`, `docs/automation/evidence/I-62-A6/r3/readable/r2static/part-b-pou.json`, `docs/automation/evidence/I-62-A6/r2/a6r2-guards.py`, `docs/automation/evidence/I-62-A6/r2/a6r2-selftest.json`, `docs/automation/evidence/I-62-A6/r2/independence/results/a-mutation.json`, `docs/automation/evidence/I-62-A6/r2/independence/results/c-r4.json`, `docs/automation/evidence/I-62-A6/r2/neg-fixtures/I-62-A-6.r2.draft-0304.md`, `docs/automation/evidence/I-62-A6/r2/neg-fixtures/a6r2-selftest.draft-0304.json`, `docs/automation/evidence/I-62-A6/r2/r1r4/r1-attacks.json`, `docs/automation/evidence/I-62-A6/r2/readable/independence/results/a-mutation.json`, `docs/automation/evidence/I-62-A6/r2/readable/independence/results/c-r4.json`, `docs/automation/evidence/I-62-A6/r2/readable/r1r4/r1-attacks.json`, `docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r1-attacks.json`, `docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/a-mutation.json`, `docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/c-r4.json`, `docs/automation/evidence/I-62-A6/r2/repro/_work/cmp-r1r4.json`, `docs/automation/evidence/I-62-A6/r2/repro/r1r4/r1-attacks.json`, `docs/automation/evidence/I-62-A6/r2/repro/r3/results/a-mutation.json`, `docs/automation/evidence/I-62-A6/r2/repro/r3/results/c-r4.json`, `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/tests/test_bmin_accept.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/test_bmin_facts.py`, `docs/automation/evidence/I-62-architect-A-6r2/R20261012T195511Z-e5c3/output.json`, `docs/initiatives/I-62-proposal-v14.md`, `docs/AUTOMATION_PLAN.md`, `docs/initiatives/I-62-A-1.md`, `docs/initiatives/I-62-A-3.md`, `docs/initiatives/I-62-A-4.md`, `docs/automation/agent-execution/README.md`, `docs/automation/decisions/I-62.md`, `docs/automation/evidence/I-62-evidence.md`, `a6-r2.md`), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** no la necesitas, porque las rutas están arriba. Si la usas, su alcance (`path` y patrón) solo puede contener archivos de la lista
  (`docs/automation/evidence/I-62-A6/r2/`, `docs/automation/evidence/I-62-A6/r3/` y el directorio del paquete B-min están enteros en el cierre; el
  resto de `I-62-A6/` no). Un patrón que alcance cualquier otro archivo cuenta como lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, el fixture
    (`D:\r62-fixture\*`) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una referencia.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: docs/automation/evidence/I-62-A6/r3/r1probes/p1-zero.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p1-zero.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p2-n4.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p2-n4.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p3-nc1.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p3-nc1.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p4-t1.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p4-t1.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p5-mutants.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p5-mutants.json`); docs/automation/evidence/I-62-A6/r3/r1probes/p6-reconstruct.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/p6-reconstruct.json`); docs/automation/evidence/I-62-A6/r3/r1probes/summary.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r1probes/summary.json`); docs/automation/evidence/I-62-A6/r3/r2static/part-a-events.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r2static/part-a-events.json`); docs/automation/evidence/I-62-A6/r3/r2static/part-b-pou.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r3/readable/r2static/part-b-pou.json`); docs/automation/evidence/I-62-A6/r2/independence/results/a-mutation.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/a-mutation.json`); docs/automation/evidence/I-62-A6/r2/independence/results/b-clausemap.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/b-clausemap.json`); docs/automation/evidence/I-62-A6/r2/independence/results/build.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/build.json`); docs/automation/evidence/I-62-A6/r2/independence/results/c-r4.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/c-r4.json`); docs/automation/evidence/I-62-A6/r2/independence/results/d-closure.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/d-closure.json`); docs/automation/evidence/I-62-A6/r2/independence/results/e-review.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/independence/results/e-review.json`); docs/automation/evidence/I-62-A6/r2/r1r4/r1-attacks-extra.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/r1r4/r1-attacks-extra.json`); docs/automation/evidence/I-62-A6/r2/r1r4/r1-attacks.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/r1r4/r1-attacks.json`); docs/automation/evidence/I-62-A6/r2/r1r4/r1-p1-p2-p3.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/r1r4/r1-p1-p2-p3.json`); docs/automation/evidence/I-62-A6/r2/r1r4/r4-t1.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/r1r4/r4-t1.json`); docs/automation/evidence/I-62-A6/r2/r1r4/r4-t2.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/r1r4/r4-t2.json`); docs/automation/evidence/I-62-A6/r2/r1r4/r4-t3.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/r1r4/r4-t3.json`); docs/automation/evidence/I-62-A6/r2/r1r4/r4-t6.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/r1r4/r4-t6.json`); docs/automation/evidence/I-62-A6/r2/repro/_work/r3-interrupted/results/build.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/_work/r3-interrupted/results/build.json`); docs/automation/evidence/I-62-A6/r2/repro/r1r4/r1-attacks-extra.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r1-attacks-extra.json`); docs/automation/evidence/I-62-A6/r2/repro/r1r4/r1-attacks.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r1-attacks.json`); docs/automation/evidence/I-62-A6/r2/repro/r1r4/r1-p1-p2-p3.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r1-p1-p2-p3.json`); docs/automation/evidence/I-62-A6/r2/repro/r1r4/r4-t1.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r4-t1.json`); docs/automation/evidence/I-62-A6/r2/repro/r1r4/r4-t2.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r4-t2.json`); docs/automation/evidence/I-62-A6/r2/repro/r1r4/r4-t3.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r4-t3.json`); docs/automation/evidence/I-62-A6/r2/repro/r1r4/r4-t6.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r1r4/r4-t6.json`); docs/automation/evidence/I-62-A6/r2/repro/r3/results/a-mutation.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/a-mutation.json`); docs/automation/evidence/I-62-A6/r2/repro/r3/results/b-clausemap.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/b-clausemap.json`); docs/automation/evidence/I-62-A6/r2/repro/r3/results/build.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/build.json`); docs/automation/evidence/I-62-A6/r2/repro/r3/results/c-r4.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/c-r4.json`); docs/automation/evidence/I-62-A6/r2/repro/r3/results/d-closure.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/d-closure.json`); docs/automation/evidence/I-62-A6/r2/repro/r3/results/e-review.json 1 (copia legible: `docs/automation/evidence/I-62-A6/r2/readable/repro/r3/results/e-review.json`); docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-clausemap.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-contract.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-diff.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-hashes.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-identity.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-scope.json 1; docs/automation/evidence/I-62-architect-A-6/R20261011T215735Z-c434/output.json 103; docs/initiatives/I-62-proposal-v14.md 2352, 2380, 2382; docs/initiatives/I-62-A-3.md 1487; docs/automation/decisions/I-62.md 657, 734, 750, 768, 787, 805, 819. Una línea truncada no cuenta como entregada: no la cites y, si
  te hiciera falta, decláralo en `KnownLimitations`. Para los JSON de una sola línea de `r2/` y de `r3/`, lee y cita la copia de `readable/` (arriba).
- **Contexto.** El cierre es grande: lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el
  auditor cuenta como un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ∖ ∩ ∪ « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Referencias (`Evidence`, `ClauseRefs`, `DirectReads`, `A6Refs` y `AcceptorRefs`)

- Una referencia es `ruta:línea` o `ruta:inicio-fin`: la ruta, relativa al clon para los archivos del cierre (p. ej.,
  `docs/initiatives/I-62-A-6.md:1213`), y el nombre suelto para los archivos del run que pueden ser premisa (`order.txt:44`,
  `a6-r2.md:1`). Sin rutas absolutas ni barras invertidas. `prompt.md` nunca es referencia.
- Cita solo líneas que hayas leído enteras con Read en ese archivo y que te llegaron fielmente. El invocador comprueba que Read te entregó cada línea
  citada, también en `DirectReads`, `A6Refs` y `AcceptorRefs`.
- **Números de línea exactos:** los que Read muestra a la izquierda. Una referencia desplazada, aunque sea en una línea, no se acredita. Antes de
  entregar, contrasta cada referencia con la salida de Read de la que sale.

## Qué decidir

Se pide el veredicto formal sobre la A-6 exacta de la ronda 3 (LIFECYCLE §5 y §6), acotado a los nueve puntos de §84, punto 6. Contesta con el texto
normativo del cierre y con lo que leas.

- **`Verdict`:** `AGREED` (cero REQUIRED abiertos sobre la versión exacta), `CHANGES REQUIRED` (al menos un REQUIRED) o `BLOCKED — OWNER DECISION`
  (falta una autoridad reservada: otra consecuencia OWNER-RESERVED que A-6 no declara, o una que no esté dispuesta; A-6 §7). Un REQUIRED impide el
  acuerdo hasta su disposición y corrección.
- **Hallazgos nuevos.** Un hallazgo es REQUIRED en los casos de LIFECYCLE §5 (activa M-01..M-08 sobre un elemento congelable, deja un invariante sin
  obligación de prueba, produce comportamiento observable distinto, o revela un elemento materialmente incorrecto, no ejecutable o no verificable);
  una precisión sin cambio de significado es OPTIONAL. Los hallazgos de esta ronda llevan ids nuevos (REQUIRED `A62-A6R3-R1`, `A62-A6R3-R2`…;
  OPTIONAL `A62-A6R3-O1`…), distintos de los de las rondas anteriores, que aquí solo se disponen. Cada uno con la sección exacta de A-6 (`Section`),
  su evidencia (`Evidence`: al menos una referencia) y la corrección (`CorrectionRequired` en un REQUIRED, `SuggestedCorrection` en un OPTIONAL). Un
  REQUIRED trae además la autoridad o el invariante violado (`ViolatedInvariant`) o un contraejemplo concreto (`Counterexample`), al menos uno no vacío,
  y por qué importa (`WhyItMatters`).
- **`Round2FindingResolution`** (cierre de R1 y de R2): `A62-A6R2-R1` y `A62-A6R2-R2`, una vez cada uno. Compara el hallazgo literal de la salida de la
  ronda 2 (`Section`, `ViolatedInvariant`, `Counterexample` y `CorrectionRequired`, con sus partes (a)-(d) en R1 y (a)-(b) en R2), lo que pide para él
  decisiones §83 (punto 3 para R1, línea 1659; punto 4 para R2, línea 1660), el texto de la ronda 2 (`a6-r2.md`), el de la ronda 3 (§3.17 y
  las reglas que nombra; la tabla de las correcciones de la ronda 2 está en §3.16, L1017-1031) y la evidencia de `r3/`. §83, punto 4, pide identificar
  qué se comprueba estáticamente y qué exige la medición real de A6-7,
  sin declarar acreditada evidencia que todavía no existe. `Status`: `CORRECTED` (el defecto ya no existe y se cumple lo que pide §83),
  `PARTIALLY_CORRECTED` o `NOT_CORRECTED`; con `Rationale`, `ClauseRefs` y `FindingIds`. Lo que no esté corregido del todo cita al menos un REQUIRED
  nuevo; un CORRECTED no cita ninguno.
- **`NegativeCaseCoherence`** (coherencia de N4, nc1 y VERIFY sin instrumento): `N4` (entrega ausente), `NC1` (`CurrentSha` inexistente) y
  `VERIFY_WITHOUT_INSTRUMENT` (una invocación VERIFY sin ninguna ejecución del instrumento), una vez cada uno: si A-6 los trata de forma total y
  coherente con AUTOMATION_PLAN 16.9 (semántica y precedencia de los fallos sin cambio), 16.11 y 16.22, con RAE §10 (oráculo relativo de los
  controles negativos) y con el aceptante fijado, a través de los fallos cerrados en la invocación de A6-5, regla 1 (L634-639), la solicitud de
  A6-3, regla 6 (c) (L572-593), la tabla de A6-5, regla 2
  (L640-653), la salida no aceptada de su regla 4 (L656-669), el resultado de FX-02 de su regla 5 (L670-673) y el ámbito de su regla 7
  (L684-700), incluidas las comprobaciones que el Controller deja en `not_run` después del primer fallo. Contrasta con las pruebas del aceptante
  fijado de `r3/r1probes/` (copias legibles en `r3/readable/r1probes/`). `Status`: `COHERENT` o `INCOHERENT` (cita un REQUIRED; un COHERENT no cita
  ninguno); con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`ReconstructionFromRealEvents`** (reconstrucción desde eventos reales): la reconstrucción de `Runs` desde el registro de sesión del adapter (A6-3,
  regla 6 (a), L572-593; A6-7, reglas 1, 5 y 7, L726-737, L748-753 y L756-761; la tabla de precondiciones de §3.17 y sus límites,
  L1065), frente a la evidencia estática (`r3/r2static/` y `r3/r1probes/`): si la regla está definida de forma verificable, si todo registro
  que no permita reconstruir las ejecuciones deja la salida sin aceptar y la celda sin elegir, y si A-6 separa lo comprobado estáticamente de lo que
  exige la medición real. `Assessment`: `ADEQUATE_AS_FAIL_CLOSED_PRECONDITION` o `INADEQUATE` (cita un REQUIRED; un ADEQUATE no cita ninguno); con
  `Rationale`, `ClauseRefs` y `FindingIds`.
- **`EffectiveInputClosure`** (cierre efectivo de entradas): la cobertura de las lecturas legítimas del instrumento por el cierre efectivo de las
  invocaciones VERIFY (A6-7, regla 7; A6-5, regla 6, L674-683; la tabla de derivación de A6-9, regla 4, L808-810; A6-4, regla 3,
  L612-621; `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md`, L44-L45; `r2/independence/`, (d); Q-A6R2-06): si la
  disposición previa del Coordinator que exige A-6 está definida de forma verificable y si, sin ella o sin una medición que la demuestre, la medición
  no se hace y el aceptante no se usa. El mismo `Assessment`, con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`AcceptorCodeContrast`** (compatibilidad con el aceptante fijado): cada afirmación de A-6 sobre el resultado del aceptante (A6-5, regla 2,
  L640-653; A6-5, regla 7, L684-700; A6-3, regla 6, L572-593) contrastada con el código real del aceptante fijado (`docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py`, blob
  `434ef1fc…`). Para cada afirmación, una vez: `A6Refs` (las líneas de A-6 que la enuncian), `AcceptorRefs` (las líneas de `bmin_accept.py` que leíste
  con Read y que la confirman o la refutan; al menos una, siempre de ese archivo), `Status` (`MATCHES` o `DIFFERS`; un DIFFERS cita un REQUIRED; un
  MATCHES no cita ninguno), `Rationale` y `FindingIds`. Las citas de código de A-6, de G10, de las notas o de la evidencia no bastan: lee el código.
  Las afirmaciones:
  - `CHECK_IDS`: el conjunto de comprobaciones R1-R10 del resultado y sus estados (`MATCH`, `MISMATCH`, `NOT_APPLICABLE`, `MANUAL`);
  - `WAIVABLE_SET`: qué comprobaciones puede declarar no aplicables la solicitud, y que el aceptante también da `ACCEPT_FACTS` con comprobaciones
    declaradas no aplicables, de modo que la `Decision` sola no basta;
  - `DECISION_FORMULA`: cuándo la `Decision` es `ACCEPT_FACTS` y cuándo `REJECT_FACTS`;
  - `EXIT_CODES`: los códigos de salida 0, 1 y 64 (error de uso) y su relación con la `Decision`;
  - `NOT_APPLICABLE_RULES`: qué datos de la solicitud hacen aplicables R5, R6 y R7 (órdenes en `Runs`; para R6, también una `AuthorityResolution` no
    vacía en la verificación) y qué hace el aceptante cuando se declaran no aplicables, con esos datos o sin ellos;
  - `R4_VERIFICATION_CONDITION`: lo que lee R4 (la salida `controller-verification/v2`, la delegación, la entrega, la ruta ignorada, la revisión de
    los términos y `MainSha`) y qué hace cuando falta uno de esos archivos o cuando la solicitud no lleva `Verification`, frente a A6-3, regla 6 (c),
    y A6-5, regla 2;
  - `R4_NOT_RUN_AS_NON_PASS`: que R4 trata `fail` y `not_run` por igual como distintos de `pass`;
  - `R4_AUTHORITY_MAP_RULE`: que R4 exige que una `Authority` en `pass` no tenga defectos de mapa;
  - `R3_R9_WITHOUT_RUNS`: qué dan R3, R9 y la `Decision` con `Runs` vacío, frente a A6-5, regla 7 (b), y a la última frase de A6-3, regla 6.
- **`OwnerAuthorizationAndVerifyScope`** (autorización del Owner y alcance de VERIFY; A-6 §7, L1181-1215; líneas L1213 y L1214; la (a),
  registrada en decisiones §78, línea 1572; ronda 3 y blobs fijados, L1203-1206; A6-1, reglas 1 y 2, L474-482 y L483-484; A6-8,
  regla 5, L782-783): `LineA` y `LineB` (`LIMITED_TO_STATED_SCOPE` o `EXCEEDS_STATED_SCOPE`, en particular frente a «en su verificación»);
  `VerifyOnly` (el instrumento y el aceptante solo en las invocaciones VERIFY, la planificación sin ellos, y el ámbito de A6-5, regla 7, dentro de
  VERIFY); `PinnedBlobsUnchanged` (la ronda 3 conserva el instrumento `9a829b0e…` y el aceptante `434ef1fc…` que fija la línea (a));
  `NoResultAcceptedInAdvance` (ninguna línea ni regla acepta un resultado por adelantado): `CONFIRMED` o `NOT_CONFIRMED`; con `Rationale`,
  `ClauseRefs` y `FindingIds` (una línea EXCEEDS_STATED_SCOPE o un NOT_CONFIRMED cita al menos un hallazgo).
- **`GapAssessment`** (riesgos G1-G6): cada brecha residual G1..G6 (A-6 §3.12, L785-968: G1 898-909; G2 910-918; G3 919-934; G4 935-954; G5 955-961; G6 962-968; su actualización en la ronda 3, L1067-1069), una vez, por
  separado. Para cada una: el hecho no demostrado, el riesgo, la mitigación, la cobertura real de las pruebas, si exige cambiar un blob y el efecto
  normativo; ninguna brecha se declara cerrada por una mitigación no ensayada. `MitigationTested` (`yes`, `no` o `partial`) es lo que tú compruebas en
  la evidencia (`r2/` y `r3/`), no lo que A-6 dice de sí misma. `Assessment`: `ACCEPTABLE_RESIDUAL` (residuo declarado y aceptable tal como A-6 lo
  acota), `REQUIRES_PIECE_CHANGE` (exige cambiar el instrumento o el aceptante: otro blob, otra corrección de A-6 y otra determinación del Owner,
  A6-8, regla 5; cita un REQUIRED), `REQUIRES_TEXT_CHANGE` (A-6 debe acotarla o tratarla de otra forma; cita un REQUIRED) o `REQUIRES_OWNER_DECISION`
  (con `OwnerDecisionRequired` = true). Un ACCEPTABLE_RESIDUAL no cita ningún REQUIRED. La revisión de la ronda 2 valoró G1-G6 (su salida, líneas
  845-925); valora tú la versión de la ronda 3.
- **`Round2OptionalDisposition`** (hallazgos opcionales O1-O5): `A62-A6R2-O1`..`A62-A6R2-O5`, una vez cada uno, según lo que hace A-6 con cada uno
  (§3.17, filas O1-O5): `INCORPORATED`, `PARTLY_INCORPORATED`, `DECLINED_WITH_REASON` (A-6 no lo incorpora y da un motivo suficiente) o
  `NOT_ADDRESSED` (no lo trata, o su motivo no basta; cita al menos un hallazgo), con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`RealSessionPreconditions`** (lo que depende de una sesión real o de una disposición futura): cada precondición, una vez: `KEPT_FAIL_CLOSED` (A-6
  la mantiene como precondición con fallo cerrado, sin acreditarla de antemano) o `TREATED_AS_MET` (A-6 la da por cumplida con evidencia que todavía no
  existe; cita un REQUIRED; un KEPT_FAIL_CLOSED no cita ninguno), con `Rationale`, `ClauseRefs` y `FindingIds`. Las precondiciones (A-6 §3.17, tabla de
  precondiciones de R2, L1057-1063; A6-7, reglas 1 y 7; A6-5, regla 6; A6-3, reglas 3, 5 y 6, L549-561, L566-571 y L572-593):
  - `RUNS_RECONSTRUCTION_FROM_REAL_LOG`: la reconstrucción de `Runs` sobre el registro de sesión real (la forma exacta con que el runtime registra la
    orden declarada; el sobre sin alterar ningún byte, con LF, separable de la salida de error y sin truncar con los tamaños reales);
  - `RUNS_COMPARISON_ON_REAL_RUNS`: la comparación de argv, entradas, bytes y resultados sobre las ejecuciones reales reconstruidas;
  - `IDENTITY_AT_USE_IN_REAL_SHELL`: la entrega de los bytes del objeto por el shell real de la invocación en la comprobación del punto de uso;
  - `GIT_BY_NAME_IS_DECLARED_GIT`: que `git`, resuelto por nombre en el entorno de la invocación, es el Git declarado;
  - `CLOSURE_COVERAGE_R9_ON_REAL_RUNS`: R9 en `MATCH` sobre las ejecuciones reales, con el cierre dispuesto;
  - `ACCEPTOR_RUN_ON_MEASUREMENT_LOG`: la corrida del aceptante fijado sobre las `Runs` reconstruidas del registro real de la medición de A6-7, con el
    resultado que exige su regla 1;
  - `COORDINATOR_CLOSURE_AND_RECONSTRUCTION_RULE_DISPOSITION`: la disposición del Coordinator que, antes de la medición, fija la cobertura del cierre
    de VERIFY y la regla de reconstrucción de `Runs` (A6-7, regla 7);
  - `NORMATIVE_READINGS_DISPOSITION_AND_PRETEST`: la disposición del Coordinator sobre las lecturas normativas del aceptante y la prueba previa sobre
    los datos del fixture de la ronda (A6-5, regla 6).
- **`DirectReads`:** `{Ref, Purpose}` de cada fuente que leíste directamente para comprobar una cita de A-6. Incluye siempre
  `docs/initiatives/I-62-proposal-v14.md:2333-2334` (Paso 0) y, como mínimo recomendado, una entrada por fuente de la lista G11 en la que te apoyes.
- **`OwnerAuthorityCheck`:** si el acuerdo exige una decisión del Owner que todavía no consta (`OwnerDecisionRequired`: true o false). La línea (a)
  consta (§78) y la (b) solo rige la aplicación: ninguna de las dos cuenta aquí. Si concluyes que hace falta otra determinación (por ejemplo, porque
  una brecha exige otro blob), true, y nombra cada consecuencia en `AdditionalOwnerReservedConsequences`.
- **Preguntas de la ronda 3** (A-6 §8, L1217-1252; filas L1249-1252; y las de la ronda 2, L1238-1243): contéstalas dentro del campo que corresponda; no tienen
  campo propio.
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
  "Schema": "rackcad-a6r3-architect-review/0",
  "RunId": "R20261013T183701Z-9f80", "InvocationId": "I20261013T183701Z-9f80", "LogicalReviewRequestId": "L20261013T183701Z-9f80", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62",
  "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-6.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora de A-6 y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "V14DirectReadL2333L2334": "true | false | null", "OrderReadWhole": "true | false | null",
            "Decisions84DeclaresOrderBytesAndSha256": "true | false | null", "Decisions83RoundTwoDisposition": "true | false | null",
            "GuardsRunModeBaseHeadResult": "true | false | null", "SelftestA6TextBlobMatchesObject": "true | false | null",
            "Round2OutputIdentifiesRunAndFindings": "true | false | null", "Note": "..."},
  "DirectReads": [{"Ref": "docs/initiatives/I-62-proposal-v14.md:2333-2334", "Purpose": "..."}],
  "Reads": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A6R3-R1", "Section": "...", "ViolatedInvariant": "...", "Counterexample": "...", "Evidence": ["ruta:inicio-fin"],
                        "WhyItMatters": "...", "CorrectionRequired": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A6R3-O1", "Section": "...", "Note": "...", "Evidence": ["ruta:línea"], "SuggestedCorrection": "..."}],
  "Round2FindingResolution": [{"Round2Finding": "A62-A6R2-R1 | A62-A6R2-R2", "Status": "CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED", "Rationale": "...",
                               "ClauseRefs": [], "FindingIds": []}],
  "NegativeCaseCoherence": [{"Case": "N4 | NC1 | VERIFY_WITHOUT_INSTRUMENT", "Status": "COHERENT | INCOHERENT", "Rationale": "...", "ClauseRefs": [],
                             "FindingIds": []}],
  "ReconstructionFromRealEvents": {"Assessment": "ADEQUATE_AS_FAIL_CLOSED_PRECONDITION | INADEQUATE | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": [],
                                   "FindingIds": []},
  "EffectiveInputClosure": {"Assessment": "ADEQUATE_AS_FAIL_CLOSED_PRECONDITION | INADEQUATE | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": [],
                            "FindingIds": []},
  "AcceptorCodeContrast": [{"Claim": "CHECK_IDS | WAIVABLE_SET | … | R3_R9_WITHOUT_RUNS", "A6Refs": ["docs/initiatives/I-62-A-6.md:…"],
                            "AcceptorRefs": ["docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py:…"], "Status": "MATCHES | DIFFERS", "Rationale": "...", "FindingIds": []}],
  "OwnerAuthorizationAndVerifyScope": {"LineA": {"Assessment": "LIMITED_TO_STATED_SCOPE | EXCEEDS_STATED_SCOPE | NOT_DETERMINED", "Note": "..."},
                                       "LineB": {"Assessment": "...", "Note": "..."}, "VerifyOnly": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED",
                                       "PinnedBlobsUnchanged": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED",
                                       "NoResultAcceptedInAdvance": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": [],
                                       "FindingIds": []},
  "GapAssessment": [{"Gap": "G1 | … | G6", "Assessment": "ACCEPTABLE_RESIDUAL | REQUIRES_PIECE_CHANGE | REQUIRES_TEXT_CHANGE | REQUIRES_OWNER_DECISION",
                     "MitigationTested": "yes | no | partial", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "Round2OptionalDisposition": [{"Round2Optional": "A62-A6R2-O1 | … | A62-A6R2-O5",
                                 "Status": "INCORPORATED | PARTLY_INCORPORATED | DECLINED_WITH_REASON | NOT_ADDRESSED", "Rationale": "...",
                                 "ClauseRefs": [], "FindingIds": []}],
  "RealSessionPreconditions": [{"Precondition": "RUNS_RECONSTRUCTION_FROM_REAL_LOG | … | NORMATIVE_READINGS_DISPOSITION_AND_PRETEST",
                                "Status": "KEPT_FAIL_CLOSED | TREATED_AS_MET", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "OwnerAuthorityCheck": {"AdditionalOwnerReservedConsequences": [], "OwnerDecisionRequired": "true | false | null", "Note": "..."},
  "IfAgreed": {"ScopeLimitedToFX02F6ControllerVerify": null, "Round3ChangesLimitedToA63R6A65A67": null, "Round2FindingsCorrected": null,
               "NegativeCasesCoherent": null, "ReconstructionAndClosureFailClosed": null, "AcceptorContrastMatches": null,
               "RealSessionPreconditionsKeptFailClosed": null, "GapsAcceptableAsResiduals": null, "OwnerAuthorizationAndVerifyScopeConfirmed": null,
               "NoAdditionalOwnerDecisionRequired": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: cada campo lleva **tu** comprobación y **tu** evaluación. Donde la plantilla dice
  `"true | false | null"`, el valor es un booleano JSON o `null`, no un texto.
- `Step0`: las siete booleanas son lo que compruebas en el Paso 0 (la primera, la lectura directa de V14 L2333-2334); `null` en lo que no compruebes.
- `DirectReads` incluye siempre V14 L2333-2334, también si te detienes en el Paso 0.
- Cobertura, una vez cada elemento: `Round2FindingResolution`, A62-A6R2-R1 y A62-A6R2-R2; `NegativeCaseCoherence`, N4, NC1 y VERIFY_WITHOUT_INSTRUMENT;
  `AcceptorCodeContrast`, las nueve afirmaciones; `GapAssessment`, G1..G6; `Round2OptionalDisposition`, A62-A6R2-O1..O5; `RealSessionPreconditions`,
  las ocho precondiciones.
- Todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`; los ids no se repiten.
- **Con un veredicto** (todo salvo NOT_ACCREDITED):
  - las siete booleanas de `Step0` en `true`; `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob` = el commit, la ruta y el blob de arriba;
  - al menos una `ClauseRef` en cada elemento de `Round2FindingResolution`, `NegativeCaseCoherence`, `GapAssessment`, `Round2OptionalDisposition` y
    `RealSessionPreconditions`, y en `ReconstructionFromRealEvents`, `EffectiveInputClosure` y `OwnerAuthorizationAndVerifyScope` (las afirmaciones de
    `AcceptorCodeContrast` llevan sus `A6Refs` y `AcceptorRefs`);
  - ningún `NOT_DETERMINED` (en `ReconstructionFromRealEvents`, `EffectiveInputClosure` y `OwnerAuthorizationAndVerifyScope`) y `OwnerDecisionRequired`
    distinto de `null`;
  - todo REQUIRED citado en `FindingIds` por al menos un elemento evaluado.
- **Con AGREED:** cero REQUIRED; A62-A6R2-R1 y A62-A6R2-R2 en CORRECTED; N4, NC1 y VERIFY_WITHOUT_INSTRUMENT en COHERENT; `ReconstructionFromRealEvents`
  y `EffectiveInputClosure` en ADEQUATE_AS_FAIL_CLOSED_PRECONDITION; las nueve afirmaciones en MATCHES; las ocho precondiciones en KEPT_FAIL_CLOSED;
  G1..G6 en ACCEPTABLE_RESIDUAL; `OwnerAuthorizationAndVerifyScope` con las dos líneas en LIMITED_TO_STATED_SCOPE y los otros tres campos en CONFIRMED;
  `OwnerDecisionRequired` = false; y los once campos de `IfAgreed` en `true`. En cualquier otro caso, `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true. Con `OwnerDecisionRequired` = true,
  `AdditionalOwnerReservedConsequences` no vacía.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `Round2FindingResolution`, `NegativeCaseCoherence`, `AcceptorCodeContrast`, `GapAssessment`, `Round2OptionalDisposition`,
    `RealSessionPreconditions` y las listas de hallazgos pueden ir vacías.
  - Lo no evaluado va así: `NOT_DETERMINED` en `ReconstructionFromRealEvents`, `EffectiveInputClosure` y `OwnerAuthorizationAndVerifyScope`; `null` en
    `OwnerDecisionRequired`, en `IfAgreed` y en las booleanas de `Step0` que no comprobaste; `ReviewedCommit` y `ReviewedBlob` con lo observado o en
    `null`; los `Rationale`, «no evaluado».
  - `NOT_DETERMINED` y `null` en esos campos solo valen con NOT_ACCREDITED.