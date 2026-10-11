I-62 — PRIMERA REVISIÓN FORMAL DEL ARCHITECT DE LA ENMIENDA A-6 (instrumento de hechos fijado por blob para el Controller de FX-02 en F6, B-min), preparada según la disposición del Coordinator §77, punto 4, y la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento automático.

Eres el ARCHITECT de esta revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-6, de quienes escribieron el paquete B-min, del Coordinator, del dictamen de materialidad de B-min y de las revisiones de enmiendas anteriores. Revisas en solo lectura la versión exacta de A-6 y devuelves un único veredicto formal (LIFECYCLE §5 y §6). No redactas texto normativo de sustitución, no implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261011T215735Z-c434
InvocationId           = I20261011T215735Z-c434
LogicalReviewRequestId = L20261011T215735Z-c434   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-a6   (tu directorio de trabajo; HEAD separado en el commit, sin ramas locales; sin remoto)
Commit                 = 7b5ffac9d9c04f90b9708a867089d32e885dc815   (decisiones §77 registradas: revisión formal de A-6 autorizada con la condición del punto 4)
Publicación de A-6     = 7b7aba4645ff2430d583c641d817630282a56fdf   (A-6 candidata, PROPUESTA; padre 108d4ca1a395c64c43cfae4f17f5854fe2d01536, sin A-6)
Guardas `run`          = 0c817e1d37409ae6dfb1246c20e08c4509ad86d6   (custodia del resultado `run` de las guardas sobre 7b7aba46 = PASS)
Objeto                 = docs/initiatives/I-62-A-6.md   blob eac473c0d2e58cf1c4594f48a8816b3833af6509   (69 226 bytes, 748 líneas)
Paquete del Architect  = ninguno (este texto hace de paquete)
Orden vigente          = D:\r62-arch-a6-run\order.txt   (disposición §77 del Coordinator; 2 918 bytes, 46 líneas, SHA-256 e23faecbacc7ae33976f211ee381020b88dd08f38067eaa938c5b54ec36c4537)
```

**Recibo.** El invocador comprobó que el objeto tiene el mismo blob `eac473c0…` en `7b7aba46`, en `0c817e1d` y en el commit de arriba, y que no existe en
`108d4ca1`; que `docs/automation/evidence/I-62-A6/` no cambia después de `7b7aba46` salvo por el añadido de `a6-guards-result-run.json`; que, desde
`108d4ca1`, decisiones solo crece por añadido (§77); que el paquete B-min y el dictamen conservan los árboles y blobs que A-6 §6.1 declara; y que
las guardas custodiadas de A-6 reproducen sus resultados: `a6-guards.py run` sobre `108d4ca1..7b7aba46` da una salida idéntica a
`a6-guards-result-run.json`, y `a6-guards.py self-test` en `108d4ca1` una idéntica a `a6-selftest.json` (comprobaciones estáticas de §77, punto 4).

**Condición de lanzamiento (compuerta del Owner).** §77, punto 4 (`order.txt`, línea 21), solo permite lanzar esta revisión cuando conste una
disposición afirmativa del Owner sobre ADR-0046 #9 y se hayan verificado las autorizaciones de revisión vigentes. Esta invocación existe porque el
lanzador lo comprobó inmediatamente antes de lanzarla, en el registro de decisiones de la rama de I-62: la línea del Owner `A6-ADR0046-9-FX02 = A (…)`,
idéntica byte a byte a A-6 L679, registrada como línea completa después de decisiones §77; la línea `CLAUDE-CLI-I62 = A (…)` vigente; y el
binario medido igual al del cierre. Ese registro es posterior a tu clon y no está en él: su ausencia en las decisiones del clon no es un defecto de
A-6. No juzgas la determinación del Owner; juzgas A-6, incluido el alcance que fijan sus dos líneas (§7). La cabecera de A-6 y su §11 dicen «NOT
LAUNCHED» y «REQUIRED antes del acuerdo» porque se escribieron antes de §77 y de esa determinación.

**Objeto y revisión.** Es la primera revisión formal de A-6: no hay hallazgos anteriores. §77, punto 1, acepta para revisión formal el blob
`eac473c0…`; el punto 2 dispone cuatro temas, tres de ellos REQUIRED REVIEW TOPIC; el punto 4 exige veredictos explícitos sobre esos cuatro temas y
sobre los nueve deltas de A-6, y que un hallazgo REQUIRED impida el acuerdo. Los commits del Freeze (`b64a3b64`) y de V14 (`4c617e82`) son anteriores a
un rebase y no están en el clon: V14 se identifica por su blob.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git`, las guardas de A-6, la herramienta de B-min, su aceptante ni sus pruebas.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, sin ramas ni remoto, árbol limpio, los blobs del cierre y el
  SHA-256 de los archivos del run. El auditor la vuelve a verificar después. En `Step0` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. Lee entera, con Read, `D:\r62-arch-a6-run\order.txt` (la orden vigente, 46 líneas). Su punto 1 (líneas 3-9) designa el objeto (ruta en la línea 5,
   blob en la 7); su punto 2 (líneas 10-14) dispone los cuatro temas; su punto 4 (líneas 19-24) ordena esta revisión.
2. Lee con Read las líneas 1548-1562 de `D:\r62-arch-a6\docs\automation\decisions\I-62.md` (decisiones §77). Comprueba que la línea 1551
   declara el cuerpo pegado con 2 918 bytes y el SHA-256 de arriba; que el punto 1 (línea 1555) nombra el objeto con el blob `eac473c0…`; y que
   los puntos 2 (línea 1556) y 4 (línea 1558) registran los mismos temas y la misma revisión que las líneas 10-14 y 19-24 de `order.txt`
   (con la redacción del registro, no necesariamente literal).
3. Lee entero, con Read, `D:\r62-arch-a6\docs\automation\evidence\I-62-A6\a6-guards-result-run.json` (129 líneas): las líneas 2-4 deben
   dar `Mode` RUN, `Base` = el padre de la publicación (`108d4ca1…`) y `Head` = la publicación (`7b7aba46…`), y la línea 128 el `Result` de
   conjunto PASS.
4. Lee con Read las líneas 872-894 de `D:\r62-arch-a6\docs\automation\evidence\I-62-A6\a6-selftest.json` (`Summary`, `Result`, `Mode`, `Repo`,
   `ToolBlob` y `A6TextBlob`): `Result` PASS y `A6TextBlob` = el blob del objeto.

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`D:\r62-arch-a6-run\`):
- `order.txt`: la orden vigente (disposición §77; 2 918 bytes, 46 líneas; punto 1, líneas 3-9; punto 2, 10-14; punto 3, 15-18; punto 4, 19-24;
  punto 5, 25-29; punto 6, 30-33; punto 7, 34-36; punto 8, 37-46). Su SHA-256 es el que declara decisiones §77 en el commit;
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es referencia.

Todas las rutas que siguen son relativas al clon `D:\r62-arch-a6` y están en el commit de arriba.

**Objeto**, entero (más de 60 000 bytes: por tramos de 300 líneas como máximo): `docs/initiatives/I-62-A-6.md`: cabecera 1-53; 1. Alcance 55-79; 2. Cláusulas anteriores y fuentes 81-277; 3. Delta 279-587; 3.1 Motivo 281-295; 3.2 Correspondencia con los ocho puntos del dictamen 297-311; 3.3 Identidad por blob de los instrumentos e insumos canónicos 313-333; 3.4 A6-1 335-365; 3.5 A6-2 367-387; 3.6 A6-3 389-408; 3.7 A6-4 410-429; 3.8 A6-5 431-454; 3.9 A6-6 456-470; 3.10 A6-7 472-500; 3.11 A6-8 502-519; 3.12 A6-9 521-541; 3.13 Prohibiciones 543-563; 3.14 Observaciones del dictamen 565-575; 3.15 Efectos sobre D.5 y la Owner Validation 577-587; 4. Semántica antes y después 589-605; 5. Materialidad 607-620; 6. No cambia producción; compatibilidad con A-1..A-5 622-656; 6.1 Diff permitido y superficies prohibidas 624-638; 6.2 Compatibilidad con A-1..A-5 640-648; 6.3 Guardas mecánicas y preflight 650-656; 7. Consecuencias reservadas al Owner 658-681; 8. Preguntas para la revisión 683-700; 9. Separabilidad 702-711; 10. Evidencia 713-721; 11. Estado de revisión 723-734; Anexo 736-748.

**Evidencia de A-6** (12 archivos, enteros; evidencia de apoyo, no autoridad: guardas, autoprueba, resultados y preflight):

```text
docs/automation/evidence/I-62-A6/a6-guards-result-preview.json           blob 5db36f01b3ae5dfd7abfdcefb863ba7bc333d5fd  3 435 bytes  133 líneas
docs/automation/evidence/I-62-A6/a6-guards-result-run.json               blob a8c9ba8bba3047849cfb8bf583dd9d707ffea610  3 314 bytes  129 líneas
docs/automation/evidence/I-62-A6/a6-guards.py                            blob c5cc6168d4a29f94623a8fab90a2d4f5f8df9ffa  88 058 bytes  1 401 líneas
docs/automation/evidence/I-62-A6/a6-selftest.json                        blob 8a61a8c48aeb007d05772cab53679b8f58a97fe4  21 635 bytes  895 líneas
docs/automation/evidence/I-62-A6/make_candidate_commit_a6.py             blob ff181bf23782e98b4e42e4147829bf9ab6440978  6 569 bytes  106 líneas
docs/automation/evidence/I-62-A6/neg_controls_a6.py                      blob f48b30d2c630ef6f09af7fffed638dadf9f75191  7 752 bytes  129 líneas
docs/automation/evidence/I-62-A6/out/a6-guards-result-preview-head.json  blob 0e3dd11c3406b7b5e9cc2d11c2c442c9c2bc3614  2 533 bytes  118 líneas
docs/automation/evidence/I-62-A6/out/candidate-commit.json               blob 3b4da019c3e9cacc52a7771b75b6d2939094aad6  1 340 bytes  54 líneas
docs/automation/evidence/I-62-A6/out/neg_controls_a6.json                blob 672f4f602a4f3eb5d94848ee9c3ae67fa069d920  4 931 bytes  230 líneas
docs/automation/evidence/I-62-A6/out/worktree-digest.json                blob 69c80103655656d4a7f09681baae2d954f9cba8f  825 bytes  17 líneas
docs/automation/evidence/I-62-A6/preflight.md                            blob 69c1d98752bcdbf0a2df0c0f2ca8812cc832f44f  15 874 bytes  168 líneas
docs/automation/evidence/I-62-A6/run_preflight_a6.py                     blob deb2632fdc43cc856865b39ab3ebe4c20a14e0c3  4 640 bytes  83 líneas
```

**Paquete B-min** (30 archivos, enteros; el objeto del dictamen; para el tema Q-A6-12 compara, entre otros, `tool/bmin_facts.py` y
`accept/bmin_accept.py`, sus notas, sus pruebas y los resultados de mutación y de extremo a extremo):

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

**Dictamen de materialidad de B-min** (R20261011T162328Z-5c5e, ACCREDITED; A-6 declara vinculantes sus campos):

```text
docs/automation/evidence/I-62-architect-bmin/R20261011T162328Z-5c5e/output.json  blob 96c3efbc81e9c6814b232774fd8ac7e900deda7d  50 868 bytes  805 líneas
docs/automation/evidence/I-62-architect-bmin/R20261011T162328Z-5c5e/README.md    blob 8ce105e0c2c6efd18eb720799492fdca0b4f7315  30 287 bytes  293 líneas
```

**Canónicos.** Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por tramos de 300 líneas como máximo si pasa de
60 000 bytes.
1. `docs/initiatives/I-62-proposal-v14.md` (342 761 bytes, 3 323 líneas; blob `34ad80ea`), §18, §20.2, §20.3, §20.3.1, Anexo C, D.1, D.3, D.4, D.5 y D.6: §18 952-977; §18, OV-I62-03 960; §18, OD-5 974; §20.2, invariante de autoridad AUT-I1 1039-1047; §20.2 1027-1047; §20.3, introducción (hasta §20.3.1) 1049-1071; §20.3.1 1073-1130; §20.3.1, clase CanonicalInputs 1079; §20.3.1, pasos 2-4 1088-1099; §20.3.1, verificación 1126-1130; Anexo C 2330-2382; Anexo C, esperado independiente 2333-2334; Anexo C, C-24 2363; D.1 2386-2406; D.3 2431-2528; D.4 2530-2554; D.5 2556-2565; D.6 2567-2587; D.3, Topología A: tabla de roles, adapters y topes 2433-2447; D.3, totales de la ronda A 2523; D.4, resultados PASS/FAIL/UNVERIFIED/UNSUPPORTED 2534-2537; D.4, matriz de cierre: FX-02 PASS 2544; D.5, resiembra y FX-02 2558-2560; D.6, dependencias técnicas permitidas y privadas prohibidas 2572-2573; D.6, último párrafo, ancla de A6-3 2586-2587.
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
20. `docs/automation/decisions/I-62.md` (251 878 bytes, 1 562 líneas; blob `04c415ee`), §73-§77: §73 1479-1498; §74 1500-1513; §75 1515-1529; §76 1531-1546; §77 1548-1562; §77, bytes y SHA-256 del cuerpo pegado 1551; §77, punto 1 1555; §77, punto 2 1556; §77, punto 3 1557; §77, punto 4 1558; §77, punto 5 1559; §77, punto 6 1560; §77, punto 7 1561; §77, punto 8 1562.

Nada más. Quedan fuera del cierre, en particular: `docs/automation/evidence/I-62-evidence.md` (la evidencia §140-§150, que A-6 cita en §3.1 y §10:
contiene resúmenes y valoraciones de la sesión que preparó A-6, y se excluye para que juzgues sin ese anclaje); la tarjeta y el paquete
OWNER-RESERVED de ADR-0046 #9 y la solicitud de medición (`docs/automation/evidence/I-62-F6/requests/`); el resto del directorio del dictamen
(`audit.json`, `kit/`, `launch/`); las transcripciones y la memoria; `probe_tools.py` y los `reference.json`/`comparison.json` de las sondas; el mapa de
cláusulas y su esquema, WORKFLOW, `state/I-62.yml` y el fixture. Lo que A-6 cite de esos archivos no lo puedes comprobar: si una conclusión tuya
depende de ello, decláralo en `KnownLimitations`.

## Reglas de lectura (el invocador audita después cada llamada)

Cada ruta se comprueba por su **efecto**: resuelta contra el directorio de trabajo, con `.` y `..` colapsados y los enlaces resueltos.

- **Read:**
  - usa la ruta absoluta, solo de archivos del cierre bajo `D:\r62-arch-a6` o de los dos archivos del run;
  - en los archivos de más de 60 000 bytes (`docs/initiatives/I-62-A-6.md`, `docs/automation/evidence/I-62-A6/a6-guards.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/tests/test_bmin_accept.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/test_bmin_facts.py`, `docs/initiatives/I-62-proposal-v14.md`, `docs/AUTOMATION_PLAN.md`, `docs/initiatives/I-62-A-1.md`, `docs/initiatives/I-62-A-3.md`, `docs/initiatives/I-62-A-4.md`, `docs/automation/agent-execution/README.md`, `docs/automation/decisions/I-62.md`), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** no la necesitas, porque las rutas están arriba. Si la usas, su alcance (`path` y patrón) solo puede contener archivos de la lista
  (`docs/automation/evidence/I-62-A6/` y el directorio del paquete B-min están enteros en el cierre). Un patrón que alcance cualquier otro archivo
  cuenta como lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, el fixture
    (`D:\r62-fixture\*`) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una referencia.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-clausemap.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-contract.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-diff.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-hashes.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-identity.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-scope.json 1; docs/initiatives/I-62-proposal-v14.md 2352, 2380, 2382; docs/initiatives/I-62-A-3.md 1487; docs/automation/decisions/I-62.md 657, 734, 750, 768, 787, 805, 819. Una línea truncada no cuenta como entregada: no la cites y, si
  te hiciera falta, decláralo en `KnownLimitations`.
- **Contexto.** Lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el auditor cuenta como
  un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ∖ ∩ ∪ « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Referencias (`Evidence` y `ClauseRefs`)

- Una referencia es `ruta:línea` o `ruta:inicio-fin`: la ruta, relativa al clon para los archivos del cierre (p. ej.,
  `docs/initiatives/I-62-A-6.md:679`), y `order.txt` para la orden (p. ej., `order.txt:12`). Sin rutas absolutas ni barras invertidas.
  `prompt.md` nunca es referencia.
- Cita solo líneas que hayas leído enteras con Read y que te llegaron fielmente. El invocador comprueba que Read te entregó cada línea citada.
- **Números de línea exactos:** los que Read muestra a la izquierda. Una referencia desplazada, aunque sea en una línea, no se acredita. Antes de
  entregar, contrasta cada referencia con la salida de Read de la que sale.

## Qué decidir

Se pide el veredicto formal sobre la A-6 exacta (LIFECYCLE §5 y §6), con disposiciones explícitas de los cuatro temas de §77, punto 2, y de los nueve
deltas. Contesta con el texto normativo del cierre. No se te prohíbe señalar otro defecto material que encuentres en cualquier sección de A-6.

- **`Verdict`:** `AGREED` (cero REQUIRED abiertos sobre la versión exacta), `CHANGES REQUIRED` (al menos un REQUIRED) o `BLOCKED — OWNER DECISION`
  (falta una autoridad reservada: otra consecuencia OWNER-RESERVED que A-6 no declara, o una que no esté dispuesta; A-6 §7). Un REQUIRED impide el
  acuerdo hasta su disposición y corrección (§77, punto 4).
- **Hallazgos.** Un hallazgo es REQUIRED en los casos de LIFECYCLE §5 (activa M-01..M-08 sobre un elemento congelable, deja un invariante sin
  obligación de prueba, produce comportamiento observable distinto, o revela un elemento materialmente incorrecto, no ejecutable o no verificable);
  una precisión sin cambio de significado es OPTIONAL. Cada hallazgo lleva un id estable (REQUIRED `A62-A6-R1`, `A62-A6-R2`…; OPTIONAL `A62-A6-O1`,
  `A62-A6-O2`…), la sección exacta de A-6 (`Section`, p. ej. «§3.6 A6-3, regla 1»), su evidencia (`Evidence`: al menos una referencia) y la corrección
  (`CorrectionRequired` en un REQUIRED, `SuggestedCorrection` en un OPTIONAL). Un REQUIRED trae además la autoridad o el invariante violado
  (`ViolatedInvariant`) o un contraejemplo concreto (`Counterexample`), al menos uno de los dos no vacío, y por qué importa (`WhyItMatters`).
- **`TopicVerdicts`:** una disposición explícita de cada tema de §77, punto 2, una vez cada uno, con su base (`Rationale`), sus `ClauseRefs` y los
  `FindingIds` que la sostienen:
  - `Q-A6-01` (`order.txt`, línea 11; A-6 §8, L687): el Coordinator acepta como interpretación propuesta que el archivo de la herramienta es insumo
    canónico y que sus resultados son evidencia generada dentro de la invocación, y no acepta B-ext. Comprueba que A-6 (§1, L55-79; §3.3,
    L313-333; A6-1, regla 5; A6-2, reglas 1 y 4) es coherente con esa disposición y no reintroduce B-ext (decisiones §75);
  - `Q-A6-02`, **REQUIRED REVIEW TOPIC** (`order.txt`, línea 12; A-6 §8, L688; A6-3, §3.6, L389-408): acreditar la integridad, la procedencia y la
    autorización de la entrega del objeto Git suelto en el repositorio del `-C`, y que falla cerrado si alguna no se demuestra;
  - `Q-A6-06/07`, **REQUIRED REVIEW TOPIC** (`order.txt`, línea 13; A-6 §8, L692-L693; A6-5, §3.8, L431-454): la correspondencia normativa exacta
    entre REJECT_FACTS (el aceptante de B-min), INVALID_OUTPUT (AUTOMATION_PLAN 16.9; procedimiento §8; contratos del Controller), FAIL y UNVERIFIED
    (V14 D.4, resultados), «no inventar estados» (§77): ni A-6 ni tu evaluación pueden apoyarse en un estado, resultado o efecto que no definan el
    texto normativo del cierre o un delta explícito de A-6;
  - `Q-A6-12`, **REQUIRED REVIEW TOPIC** (`order.txt`, línea 14; A-6 §8, L698; A6-9, §3.12, L521-541): la independencia efectiva del aceptante
    frente a la herramienta, no solo la autoría declarada. Compara el código, las notas, las pruebas y los resultados de ambos en el paquete B-min y
    di si A6-9, regla 3, queda demostrada por evidencia observable o solo declarada, y si A-6 exige lo que falte antes de apoyarse en ella.
  `Disposition`: `SATISFIED` (A-6 cumple lo que §77, punto 2, pide para ese tema), `NOT_SATISFIED` (no lo cumple: cita al menos un hallazgo, y en los
  tres REQUIRED REVIEW TOPIC al menos un REQUIRED) o `NEEDS_OWNER` (el tema no se resuelve sin una decisión reservada al Owner, con
  `OwnerDecisionRequired` = true). Un tema SATISFIED no cita ningún REQUIRED.
- **`DeltaVerdicts`:** cada delta, una vez, `AGREED` o `CHANGES REQUIRED`, con su base, sus `ClauseRefs` y sus `FindingIds`: A6-1 (§3.4), A6-2 (§3.5),
  A6-3 (§3.6), A6-4 (§3.7), A6-5 (§3.8), A6-6 (§3.9), A6-7 (§3.10), A6-8 (§3.11) y A6-9 (§3.12). CHANGES REQUIRED cita al menos un REQUIRED; AGREED,
  ninguno. Todo REQUIRED queda citado por al menos un delta o un tema.
- **`Separability`** (A-6 §9, L702-711): si es cierto que dentro de A-6 ningún delta es separable (`WithinA6NoDeltaSeparable`), que A-6 es separable
  frente a A-1..A-5 (`SeparableFromA1toA5`) y que las dos líneas del Owner son separables entre sí (`OwnerLinesSeparable`): CONFIRMED o NOT_CONFIRMED,
  con la base, sus `ClauseRefs` y, si alguna es NOT_CONFIRMED, al menos un hallazgo citado.
- **`MaterialityConfirmation`:** M-01..M-08 de LIFECYCLE §3, una vez cada uno: `A6Declared` (el valor de la columna «Activado» de A-6 §5, L607-620:
  sí → `yes`, no → `no`), tu evaluación (`ReviewerAssessment`: `yes`, `no` o `UNKNOWN`) y `Agreement` (`AGREE` si coinciden; `DISAGREE` si no, con al
  menos un hallazgo citado), con la base y al menos una `ClauseRef`.
- **`CompatibilityA1toA5`** (A-6 §6.2, L640-648): A-1..A-5, cada una `COMPATIBLE` o `CONFLICT`, con la base.
- **`NoChangeConfirmation`** (A-6 §3.13, L543-563; decisiones §76, punto 2): si A-6 deja sin cambio la receta 16.4 (`Recipe164`), el adapter
  `codex-cli` (descriptor, contrato y esquema de hechos: `AdapterCodexCli`), los esquemas de salida (`OutputSchemas`), los presupuestos, topes y totales
  congelados de D.3, D.5 y D.8 (`FrozenBudgetsD3D5D8`), los roles y la autoridad del Controller (`RolesAndControllerAuthority`), producción
  (`Production`), ADR-0046 y ADR-0048 (`ADR0046AndADR0048`) y V14 fuera de los nueve párrafos añadidos (`V14OutsideNineParagraphs`): NO_CHANGE o
  CHANGE, con la base.
- **`ScopeConfirmation`:** si es cierto lo que A-6 afirma de la ubicación de sus párrafos (§1; «Ubicación» de §3.4-§3.12), comparándola con V14 y A-5:
  seis párrafos añadidos al final de D.3 después de A5-2 (A-5 L221) (`D3AppendAfterA52`), dos al final de D.4 después de V14 L2554
  (`D4AppendAfterV14L2554`), uno al final de D.6 después de V14 L2587 (`D6AppendAfterV14L2587`), y ningún texto congelado ni acordado sustituido
  (`NoFrozenTextReplaced`): CONFIRMED o NOT_CONFIRMED, con la base.
- **`D5AndOwnerValidation`** (A-6 §3.15, L577-587; §77, punto 7, `order.txt`, línea 36): `EXCLUSION_CORRECT` (es correcto que A-6 excluya D.5 y la
  Owner Validation), `COVER_IN_A6` (A-6 tendría que cubrirlas: cita al menos un REQUIRED) u `OWNER_DECISION_REQUIRED` (decidirlo es una materia
  reservada al Owner: `OwnerDecisionRequired` = true), con `OwnerValidationAffected` (YES o NO: si A-6 afecta a la ejecución de la Owner Validation),
  la base, sus `ClauseRefs` y sus `FindingIds`.
- **`OwnerDecisionsScope`** (A-6 §7, L658-681; líneas L679 y L680): si la línea (a) (`LineA`) y la (b) (`LineB`) se limitan al alcance que
  enuncian (`LIMITED_TO_STATED_SCOPE`) o van más allá (`EXCEEDS_STATED_SCOPE`); si son distintas y ninguna implica la otra (`SeparateAndNotImplied`); y
  si ninguna acepta un resultado por adelantado (`NoResultAcceptedInAdvance`): CONFIRMED o NOT_CONFIRMED, con la base y sus `ClauseRefs`.
- **`OwnerAuthorityCheck`:** si el acuerdo exige una decisión del Owner que todavía no consta (`OwnerDecisionRequired`: true o false). La línea (a)
  consta (la comprobó el lanzador) y la (b) solo rige la aplicación: ninguna de las dos cuenta aquí. Con true, nombra cada consecuencia en
  `AdditionalOwnerReservedConsequences`.
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
  "Schema": "rackcad-a6-architect-review/0",
  "RunId": "R20261011T215735Z-c434", "InvocationId": "I20261011T215735Z-c434", "LogicalReviewRequestId": "L20261011T215735Z-c434", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62",
  "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-6.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora de A-6 y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "OrderReadWhole": "true | false | null", "OrderNamesObjectBlob": "true | false | null",
            "Decisions77DeclaresOrderBytesAndSha256": "true | false | null", "Decisions77NamesObjectBlob": "true | false | null",
            "Decisions77TopicsAndReviewMatchOrder": "true | false | null", "GuardsRunModeBaseHeadResult": "true | false | null",
            "SelftestA6TextBlobMatchesObject": "true | false | null", "Note": "..."},
  "Reads": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A6-R1", "Section": "...", "ViolatedInvariant": "...", "Counterexample": "...", "Evidence": ["ruta:inicio-fin"],
                        "WhyItMatters": "...", "CorrectionRequired": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A6-O1", "Section": "...", "Note": "...", "Evidence": ["ruta:línea"], "SuggestedCorrection": "..."}],
  "TopicVerdicts": [{"Topic": "Q-A6-01 | Q-A6-02 | Q-A6-06/07 | Q-A6-12", "Disposition": "SATISFIED | NOT_SATISFIED | NEEDS_OWNER", "Rationale": "...",
                     "ClauseRefs": ["ruta:línea"], "FindingIds": []}],
  "DeltaVerdicts": [{"Delta": "A6-1 | … | A6-9", "Verdict": "AGREED | CHANGES REQUIRED", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "Separability": {"WithinA6NoDeltaSeparable": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "SeparableFromA1toA5": "...", "OwnerLinesSeparable": "...",
                   "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "MaterialityConfirmation": [{"Id": "M-01 | … | M-08", "A6Declared": "yes | no", "ReviewerAssessment": "yes | no | UNKNOWN", "Agreement": "AGREE | DISAGREE",
                               "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "CompatibilityA1toA5": {"A1": {"Assessment": "COMPATIBLE | CONFLICT | NOT_DETERMINED", "Note": "..."}, "A2": {"Assessment": "...", "Note": "..."},
                          "A3": {"Assessment": "...", "Note": "..."}, "A4": {"Assessment": "...", "Note": "..."}, "A5": {"Assessment": "...", "Note": "..."}},
  "NoChangeConfirmation": {"Recipe164": {"Assessment": "NO_CHANGE | CHANGE | NOT_DETERMINED", "Note": "..."}, "AdapterCodexCli": {"Assessment": "...", "Note": "..."},
                           "OutputSchemas": {"Assessment": "...", "Note": "..."}, "FrozenBudgetsD3D5D8": {"Assessment": "...", "Note": "..."},
                           "RolesAndControllerAuthority": {"Assessment": "...", "Note": "..."}, "Production": {"Assessment": "...", "Note": "..."},
                           "ADR0046AndADR0048": {"Assessment": "...", "Note": "..."}, "V14OutsideNineParagraphs": {"Assessment": "...", "Note": "..."}},
  "ScopeConfirmation": {"D3AppendAfterA52": {"Assessment": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "Note": "..."},
                        "D4AppendAfterV14L2554": {"Assessment": "...", "Note": "..."}, "D6AppendAfterV14L2587": {"Assessment": "...", "Note": "..."},
                        "NoFrozenTextReplaced": {"Assessment": "...", "Note": "..."}},
  "D5AndOwnerValidation": {"Disposition": "EXCLUSION_CORRECT | COVER_IN_A6 | OWNER_DECISION_REQUIRED | NOT_DETERMINED",
                           "OwnerValidationAffected": "YES | NO | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "OwnerDecisionsScope": {"LineA": {"Assessment": "LIMITED_TO_STATED_SCOPE | EXCEEDS_STATED_SCOPE | NOT_DETERMINED", "Note": "..."},
                          "LineB": {"Assessment": "...", "Note": "..."}, "SeparateAndNotImplied": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED",
                          "NoResultAcceptedInAdvance": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": []},
  "OwnerAuthorityCheck": {"AdditionalOwnerReservedConsequences": [], "OwnerDecisionRequired": "true | false | null", "Note": "..."},
  "IfAgreed": {"ScopeLimitedToFX02F6Controller": null, "NineAppendOnlyParagraphs": null, "NoChangeToProtectedSurfaces": null, "FourTopicsSatisfied": null,
               "AllNineDeltasAgreed": null, "NoAdditionalOwnerDecisionRequired": null, "D5AndOVExcluded": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: cada campo lleva **tu** comprobación y **tu** evaluación. Donde la plantilla dice
  `"true | false | null"`, el valor es un booleano JSON o `null`, no un texto.
- `Step0`: las siete booleanas son lo que compruebas en el Paso 0; `null` en lo que no compruebes.
- Cobertura, una vez cada elemento: `TopicVerdicts`, los cuatro temas; `DeltaVerdicts`, A6-1..A6-9; `MaterialityConfirmation`, M-01..M-08.
- Todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`; los ids no se repiten.
- **Con un veredicto** (todo salvo NOT_ACCREDITED):
  - las siete booleanas de `Step0` en `true`; `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob` = el commit, la ruta y el blob de arriba;
  - al menos una `ClauseRef` en cada tema, en cada delta, en cada fila de `MaterialityConfirmation`, en `Separability`, en `D5AndOwnerValidation` y en
    `OwnerDecisionsScope`;
  - ningún `NOT_DETERMINED` (en `Separability`, `CompatibilityA1toA5`, `NoChangeConfirmation`, `ScopeConfirmation`, `D5AndOwnerValidation` y
    `OwnerDecisionsScope`) y `OwnerDecisionRequired` distinto de `null`;
  - todo REQUIRED citado por al menos un delta o un tema.
- **Con AGREED:** cero REQUIRED; los nueve deltas en AGREED; los cuatro temas en SATISFIED; `OwnerDecisionRequired` = false; `D5AndOwnerValidation` en
  EXCLUSION_CORRECT; `CompatibilityA1toA5` todo COMPATIBLE; `NoChangeConfirmation` todo NO_CHANGE; `ScopeConfirmation` todo CONFIRMED;
  `OwnerDecisionsScope` con las dos líneas en LIMITED_TO_STATED_SCOPE y los otros dos campos en CONFIRMED; y los ocho campos de `IfAgreed` en `true`. En
  cualquier otro caso, `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true. Con `OwnerDecisionRequired` = true,
  `AdditionalOwnerReservedConsequences` no vacía.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `TopicVerdicts`, `DeltaVerdicts`, `MaterialityConfirmation` y las listas de hallazgos pueden ir vacías.
  - Lo no evaluado va así: `NOT_DETERMINED` en `Separability`, `CompatibilityA1toA5`, `NoChangeConfirmation`, `ScopeConfirmation`,
    `D5AndOwnerValidation` (sus dos campos) y `OwnerDecisionsScope`; `null` en `OwnerDecisionRequired`, en `IfAgreed` y en las booleanas de `Step0` que
    no comprobaste; `ReviewedCommit` y `ReviewedBlob` con lo observado o en `null`; los `Rationale`, «no evaluado».
  - `NOT_DETERMINED` y `null` en esos campos solo valen con NOT_ACCREDITED.