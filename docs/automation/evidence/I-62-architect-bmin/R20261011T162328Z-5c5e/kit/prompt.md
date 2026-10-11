I-62 — DICTAMEN INDEPENDIENTE Y ACOTADO DEL ARCHITECT SOBRE LA MATERIALIDAD DEL PAQUETE EXPERIMENTAL B-min, preparado según la disposición del Coordinator §75, punto 4, y la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento automático.

Eres el ARCHITECT de este dictamen: un proceso `claude-cli` nuevo, separado de la sesión que preparó B-min, de quienes escribieron su código, del Coordinator y de las revisiones anteriores. Lees en solo lectura y devuelves un único resultado estructurado. No es la revisión formal de una enmienda: la orden lo excluye («No solicitar revisión formal de una enmienda que todavía no existe», `order.txt`, línea 40). No redactas enmiendas ni texto normativo, no implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261011T162328Z-5c5e
InvocationId           = I20261011T162328Z-5c5e
LogicalReviewRequestId = L20261011T162328Z-5c5e   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-bmin   (tu directorio de trabajo; HEAD separado en el commit, sin ramas locales; sin remoto)
Commit                 = 9356edf141c795ea718a360e266334ded10d6a76   (HEAD del clon; contiene el paquete y decisiones §75)
Commit del paquete     = 9356edf141c795ea718a360e266334ded10d6a76   (el paquete tiene el mismo árbol aquí y en el commit de arriba)
Base de la preparación = 460665abd46e4f9f75c896eceecabe11689a6a40   (decisiones §75 registradas; el paquete no existe en ella)
Objeto                 = docs/automation/evidence/I-62-F6/FX-02/bmin/   (30 archivos, 541 797 bytes; tabla abajo)
Orden vigente          = D:\r62-arch-bmin-run\order.txt   (disposición §75 del Coordinator; 3 288 bytes, 74 líneas, SHA-256 21435cd1c0821d8c934117a10e850cb9fa124a06f8dd589719aff8f5b711d630)
```

Archivos del objeto (rutas relativas al clon; blob en el commit del paquete, el mismo en el commit de arriba):

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

**Recibo.** El invocador comprobó que el paquete no existe en la base; que su árbol es el mismo en el commit del paquete y en el commit de arriba;
que, desde la base, decisiones y evidencia solo crecen por añadido; y que los demás archivos del cierre conservan el blob que tenían en la base.

**Qué es el objeto.** La orden (puntos 2 y 3) autoriza preparar B-min como paquete experimental: herramientas deterministas de solo lectura,
entradas cerradas y fijadas por blob, salidas JSON ASCII canónicas, ninguna decisión PASS/FAIL/VERIFIED producida por la herramienta, pruebas y
reproducción R1-R10 por un aceptante, fuera de F_seed, F_norm y de los contratos de producción. `SPEC.md` describe la herramienta, el aceptante y
las pruebas. Este dictamen no juzga la calidad del código ni sus resultados: dictamina la materialidad del **uso** de B-min por el que pregunta la
orden.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no
  puedes ejecutar `git`, la herramienta de B-min, su aceptante ni sus pruebas.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, sin ramas ni remoto, árbol limpio, los blobs de la tabla y
  del cierre, y el SHA-256 de los archivos del run. El auditor la vuelve a verificar después. En `Step0` declara solo lo que compruebes tú con tus
  herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. Lee entera, con Read, `D:\r62-arch-bmin-run\order.txt` (la orden vigente, 74 líneas). Su punto 4 (líneas 27-40) ordena este dictamen: la
   pregunta está en la línea 30 y los entregables en las líneas 33-38.
2. Lee con Read las líneas 1515-1529 de `D:\r62-arch-bmin\docs\automation\decisions\I-62.md` (decisiones §75). Comprueba que la línea
   1518 declara el cuerpo pegado con 3 288 bytes y el SHA-256 de arriba, y que el punto 4 (línea 1525) registra la misma pregunta
   que la línea 30 de `order.txt` (con la redacción del registro, no necesariamente literal).
3. Usa Glob una sola vez, con `path` = `D:\r62-arch-bmin\docs\automation\evidence\I-62-F6\FX-02\bmin` y `pattern` = `**/*`. Las rutas devueltas deben
   ser exactamente las de la tabla del objeto: ni más ni menos.
4. Lee con Read las líneas 1-5 de `D:\r62-arch-bmin\docs\automation\evidence\I-62-F6\FX-02\bmin\SPEC.md` (su cabecera): deben nombrar las
   decisiones §75.

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en
`KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`D:\r62-arch-bmin-run\`):
- `order.txt`: la orden vigente (disposición §75; 3 288 bytes, 74 líneas; punto 1, líneas 3-7; punto 2, 8-20; punto 3, 21-26; punto 4, 27-40;
  punto 5, 41-53; punto 6, 54-57; punto 7, 58-62; punto 8, 63-74). Su SHA-256 es el que declara decisiones §75 en el commit;
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es referencia.

**Objeto**, entero: los 30 archivos de la tabla de arriba, con rutas relativas al clon `D:\r62-arch-bmin`.

**Canónicos**, en el commit, con rutas relativas al clon. Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por
tramos de 300 líneas como máximo si pasa de 60 000 bytes.
1. `docs/AUTOMATION_PLAN.md` (155 229 bytes, 1 732 líneas; blob `f525cb1e`), 16.4, 16.9, 16.10, 16.13-16.14 y 16.18-16.24: 16.4 507-543; 16.9 620-665; 16.10 667-672; 16.13 717-928; 16.14 930-948; 16.18 1015-1041; 16.19 1043-1061; 16.20 1063-1142; 16.21 1144-1180; 16.22 1182-1209; 16.23 1211-1254; 16.24 1256-1323; 16.4, transporte 509; 16.4, receta de Codex 541; 16.9, tabla de las catorce comprobaciones 628-643; 16.13, mapa en I62_EFFECTIVE_SHA 731; 16.13, Validate(M): MV-1..MV-7 902-908; 16.14, punto efectivo 945-946; 16.19, contrato del adapter 1045-1046; 16.19, descriptor y Facts 1048-1052; 16.24, clase CanonicalInputs 1264; 16.24, clases AllowedTransitiveInputs, DeclaredRuntimeContext y ForbiddenInputs 1265-1267; 16.24, cálculo del cierre, paso 2 1271-1274; 16.24, identidad del revisor 1282; 16.24, fidelidad de los insumos 1287; 16.24, P-22..P-25 1318-1321.
2. `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` (9 871 bytes, 105 líneas; blob `da275e1a`), entero: Contexto 10-24; Decisión 26-69; Alternativas consideradas 71-78; Consecuencias 80-97; Referencias 99-105; Decisión, punto 1 28-43; Decisión, punto 2 44-49; Decisión, punto 3 50-51; Decisión, punto 4 52-58; Decisión, punto 5 59-60; Decisión, punto 6 61-63; Decisión, punto 7 64-66; Decisión, punto 8 67-68; Decisión, punto 9 69.
3. `docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md` (10 832 bytes, 146 líneas; blob `e1bd8d91`), entero: Contexto 15-26; Decisión 28-104; Alternativas consideradas 106-115; Consecuencias 117-135; Referencias 137-146; Decisión, punto 1 59-61; Decisión, punto 2 62-70; Decisión, punto 3 71-74; Decisión, punto 4 75-77; Decisión, punto 5 78-81; Decisión, punto 6 82-87; Decisión, punto 7 88-90; Decisión, punto 8 91-92; Decisión, punto 9 93-104.
4. `docs/INITIATIVE_LIFECYCLE.md` (24 334 bytes, 360 líneas; blob `f19896a8`), §2, §3 y §6: §2 34-64; §3 66-94; §3, M-01 83; §3, M-02 84; §3, M-03 85; §3, M-04 86; §3, M-05 87; §3, M-06 88; §3, M-07 89; §3, M-08 90; §3, UNKNOWN cuenta como activado 78-79; §3, OWNER-RESERVED 94; §6 193-222.
5. `docs/initiatives/I-62-proposal-v14.md` (342 761 bytes, 3 323 líneas; blob `34ad80ea`), §20.2, §20.3, Anexo C y Anexo D.1-D.6: §20.2, invariante de autoridad AUT-I1 1039-1047; §20.2 1027-1047; §20.3, introducción (hasta §20.3.1) 1049-1071; §20.3.1 1073-1130; §20.3.1, clase CanonicalInputs 1079; Anexo C 2330-2382; Anexo C, esperado independiente 2333-2334; Anexo C, C-24 2363; D.1 2386-2406; D.2 2408-2429; D.3 2431-2528; D.4 2530-2554; D.5 2556-2565; D.6 2567-2587; D.3, Topología A: tabla de roles, adapters y topes 2433-2447; D.6, dependencias técnicas permitidas y privadas prohibidas 2572-2573.
6. `docs/initiatives/I-62-A-4.md` (62 415 bytes, 516 líneas; blob `5e2ba68e`), entera: §1 58-78; §2 80-159; §3 161-361; §3.1 163-183; §3.2 185-211; §3.3 213-233; §3.4 235-256; §3.5 258-287; §3.6 289-301; §3.7 303-343; §3.8 345-361; §4 363-385; §5 387-398; §6 400-417; §7 419-435; §8 437-470; §9 472-479; §10 481-490; Anexo (no normativo) 492-516.
7. `docs/initiatives/I-62-A-5.md` (37 342 bytes, 372 líneas; blob `ad94ceed`), entera: §1 51-72; §2 74-147; §3 149-242; §3.1 151-167; §3.2 169-199; §3.3 201-221; §3.4 223-242; §4 244-259; §5 261-272; §6 274-296; §7 298-317; §8 319-334; §9 336-344; §10 346-354; Anexo (no normativo) 356-372; §3.2, párrafo A5-1 175-199; §3.2, A5-1, reglas 1-2 178-186.
8. `docs/automation/agent-execution/README.md` (60 779 bytes, 756 líneas; blob `592dcfd4`), §5, §8, §10, §14 y §15: §5 132-150; §8 170-188; §10 217-235; §14 369-531; §14.1 375-383; §14.2 385-403; §14.3 405-427; §14.4 429-437; §14.5 439-486; §14.6 488-498; §14.7 500-531; §15 533-603; §15.1 537-551; §15.2 553-568; §15.3 570-583; §15.4 585-603.
9. `docs/automation/agent-execution/routing.md` (10 294 bytes, 142 líneas; blob `bba08fc4`), entero: §1 9-23; §2 25-34; §3 36-45; §4 47-59; §5 61-81; §6 83-88; §7 90-93; §8 95-119; §9 121-142.
10. `docs/automation/agent-execution/adapters/codex-cli.md` (4 333 bytes, 59 líneas; blob `155f3469`), entero: Operaciones 18-30; Introspección 32-36; Huella 38-41; Hechos (`Facts`) 43-53; Límites 55-59.
11. `docs/automation/agent-execution/schemas/role-invocation.v1.schema.json` (21 758 bytes, 858 líneas; blob `d54a7ae7`), entero.
12. `docs/automation/agent-execution/schemas/input-closure.v1.schema.json` (6 911 bytes, 282 líneas; blob `6bdf7184`), entero.
13. `docs/automation/agent-execution/schemas/controller-verification.v2.schema.json` (16 528 bytes, 681 líneas; blob `e1faa9ec`), entero.
14. `docs/automation/agent-execution/schemas/delegation.v2.schema.json` (25 471 bytes, 1 002 líneas; blob `dfdbe461`), entero.
15. `docs/automation/agent-execution/schemas/worker-handoff.schema.json` (3 814 bytes, 81 líneas; blob `cc3c9cf7`), entero.
16. `docs/automation/agent-execution/schemas/gate-contract.v2.schema.json` (17 854 bytes, 650 líneas; blob `f644bb19`), entero.
17. `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md` (24 984 bytes, 217 líneas; blob `90929ad1`), entero: §1 17-30; §2 32-95; §2.1 34-65; §2.2 67-74; §2.3 76-84; §2.4 86-95; §3 97-131; §3.1 99-115; §3.2 117-131; §4 133-166; §5 168-179; §6 181-199; §7 201-217.
18. `docs/automation/decisions/I-62.md` (245 191 bytes, 1 529 líneas; blob `b761393d`), §73-§75: §73 1479-1498; §74 1500-1513; §75 1515-1529; §75, bytes y SHA-256 del cuerpo pegado 1518; §75, punto 1 1522; §75, punto 2 1523; §75, punto 3 1524; §75, punto 4 1525; §75, punto 5 1526; §75, punto 6 1527; §75, punto 7 1528; §75, punto 8 1529.

Nada más. Quedan fuera del cierre los documentos que estos archivos citan y no están en la lista; en particular, los análisis citados por la
evidencia (`controller-strategy.md`, `mv-static-analysis.md`), la corrida de la sonda CONTROLLER_V5, `probe_tools.py` y los `reference.json` y
`comparison.json` de las sondas, el resto de `kits/FX-02/`, WORKFLOW, el mapa de cláusulas y su esquema, `state/I-62.yml` y el fixture.

## Reglas de lectura (el invocador audita después cada llamada)

Cada ruta se comprueba por su **efecto**: resuelta contra el directorio de trabajo, con `.` y `..` colapsados y los enlaces resueltos.

- **Read:**
  - usa la ruta absoluta, solo de archivos del cierre bajo `D:\r62-arch-bmin` o de los dos archivos del run;
  - en los archivos de más de 60 000 bytes (`docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/tests/test_bmin_accept.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py`, `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/tests/test_bmin_facts.py`, `docs/AUTOMATION_PLAN.md`, `docs/initiatives/I-62-proposal-v14.md`, `docs/initiatives/I-62-A-4.md`, `docs/automation/agent-execution/README.md`, `docs/automation/decisions/I-62.md`), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** solo la del Paso 0, punto 3 (el directorio del objeto está entero en el cierre). Cualquier otro alcance que contenga un archivo fuera de
  la lista cuenta como lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, el fixture
    (`D:\r62-fixture\*`) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una referencia.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-clausemap.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-contract.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-diff.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-hashes.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-identity.json 1; docs/automation/evidence/I-62-F6/FX-02/bmin/e2e/out/envelope-scope.json 1; docs/initiatives/I-62-proposal-v14.md 2352, 2380, 2382; docs/automation/decisions/I-62.md 657, 734, 750, 768, 787, 805, 819. Una línea truncada no cuenta como entregada: no la cites y, si
  te hiciera falta, decláralo en `KnownLimitations`.
- **Contexto.** Lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el auditor cuenta como
  un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ∖ ∩ ∪ « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Referencias a cláusulas (`Ref` y `ClauseRefs`)

- Una referencia es `ruta:línea` o `ruta:inicio-fin`: la ruta, relativa al clon para los archivos del cierre (p. ej.,
  `docs/INITIATIVE_LIFECYCLE.md:83`), y `order.txt` para la orden (p. ej., `order.txt:30`). Sin rutas absolutas ni barras invertidas. `prompt.md`
  nunca es referencia.
- Cita solo líneas que hayas leído enteras con Read y que te llegaron fielmente. El invocador comprueba que Read te entregó cada línea citada.
- **Números de línea exactos:** los que Read muestra a la izquierda. Una referencia desplazada, aunque sea en una línea, no se acredita. Antes de
  entregar, contrasta cada referencia con la salida de Read de la que sale.
- En `ApplicableClauses`, `Excerpt` es texto **literal y contiguo** de las líneas de `Ref`, de 200 caracteres como máximo, con sus marcas de formato
  tal como aparecen (`**`, comillas invertidas, `|`), sin «…», sin comillas añadidas y sin parafrasear (el invocador lo busca con los espacios
  colapsados).

## Qué decidir

La pregunta principal es la de la orden (`order.txt`, línea 30):

> ¿Puede B-min utilizarse como herramienta ACTION_COMPATIBLE con entradas canónicas fijadas por blob bajo el contrato vigente, o modifica
> materialmente AP 16.4/16.19/16.24, ADR-0046, D.3 o los contratos consumidos?

Contesta con el texto normativo del cierre. No se te prohíbe señalar otra cuestión material que encuentres (`Observations`).

- **`Verdict`:** `NOT_MATERIAL` (el uso de B-min por el que pregunta la orden cabe en el contrato vigente sin activar ningún disparador M-01..M-08),
  `MATERIAL` (activa al menos uno) o `BLOCKED_OWNER_DECISION` (el dictamen depende de una decisión reservada al Owner, que nombras en
  `OwnerReserved`). LIFECYCLE §3 cuenta como activado un disparador en UNKNOWN.
- **`ApplicableClauses`:** las cláusulas que aplican, cada una con su `Ref`, un `Excerpt` literal y por qué aplica (`Relevance`).
- **`ActionCompatibility`:** la clasificación, según AP 16.24 (cálculo del cierre, paso 2, L1271-L1274), de la ejecución de la herramienta de B-min,
  fijada por blob, en el uso que describen `SPEC.md` (§0) y la evidencia §144: `READ`, `ACTION_COMPATIBLE`, `ACTION_INCOMPATIBLE`,
  `CONDITIONAL_NOT_TRIGGERED` o `NOT_DETERMINABLE` (el cierre no permite clasificarla), con la base y sus `ClauseRefs`.
- **`CanonicalInputAdmissible`:** si el archivo de la herramienta, fijado por blob, puede figurar en `CanonicalInputs[]` de esa invocación tal como
  define la clase AP L1264 (y V14 L1079) —`yes`—, si no puede —`no`— o si haría falta un cambio —`needs-change`—, con la base y sus `ClauseRefs`.
- **`Materiality`:** M-01..M-08 de LIFECYCLE §3, cada uno **una vez**, con `Activated` = `yes`, `no` o `UNKNOWN`, la base y al menos una
  `ClauseRef`.
- **`AffectedSurfaces`:** una entrada por cada superficie que nombra la pregunta, una vez cada una: `AP_16_4`, `AP_16_19`, `AP_16_24`, `ADR_0046`,
  `V14_D3` y `CONSUMED_CONTRACTS` (los contratos que consumen la invocación y la verificación del Controller: los esquemas del cierre y el
  procedimiento de agent-execution); y, si otra superficie resulta afectada, entradas `OTHER` con su nombre. `Effect`: `NOT_AFFECTED`,
  `WITHIN_CURRENT_TEXT` (el uso cabe en el texto vigente), `REQUIRES_NON_MATERIAL_CHANGE`, `REQUIRES_MATERIAL_CHANGE` o `UNKNOWN`, con la base.
- **`Independence`:** la consecuencia sobre la independencia, una entrada por dimensión, una vez cada una: `ACTOR`, `SESSION`, `CONTEXT` y `PROVIDER`
  (ADR-0048, punto 6; AP 16.21), `C24_ORACLE` (el oráculo de C-24 no se comparte con el Controller: orden, punto 3; V14 L2333-L2334) y
  `TOOL_ACCEPTOR_DIVERSITY` (reproducción independiente por el aceptante: orden, puntos 2 y 3). `Effect`: `NO_EFFECT`, `PRESERVED_WITH_CONDITIONS`,
  `STRENGTHENED`, `WEAKENED` o `UNKNOWN`, con la base; y `Consequence`, la consecuencia de conjunto.
- **`A6`:** si hace falta A-6 (`Needed`: `yes` o `no`) y, si hace falta, **solo su alcance mínimo** en `MinimalDeltaScope`: qué cláusulas o
  superficies tendría que tocar, una por elemento, de 200 caracteres como máximo. No redactes el texto de ninguna enmienda (orden, línea 40).
- **`OwnerReserved`:** las decisiones reservadas al Owner que encuentres (LIFECYCLE §3 y §6; V14 §20.2, AUT-I1), cada una con por qué y sus
  `ClauseRefs`; vacía si no hay ninguna.
- **`SingleInvocationMeasurement`** (observación): si una sola invocación de solo lectura del Controller, con PLAN y VERIFY juntos, podría medir
  B-min sin enmienda (`Possible`: `yes`, `no` o `UNKNOWN`), con las condiciones y la base.
- **Además:**
  - tu modo de revisión, si revisor y autor son la misma persona y el contexto que el runtime te inyectó (`ReviewerMode`, `SamePersonAsAuthor`,
    `ContextDeclaration`). Declara también el proveedor del revisor y del autor en `SamePersonAsAuthor`. Los identificadores de cuenta que el
    runtime te inyecte (correo, uuid de organización u otros) se declaran solo por su tipo, sin reproducir su valor;
  - el objeto revisado (`Object`: el commit del paquete y los archivos de la tabla con sus blobs);
  - lo que leíste (`Reads`: «ruta: rangos»).

## Resultado

Entrega el resultado con **una** llamada a StructuredOutput, con este objeto. Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-bmin-materiality-ruling/0",
  "RunId": "R20261011T162328Z-5c5e", "InvocationId": "I20261011T162328Z-5c5e", "LogicalReviewRequestId": "L20261011T162328Z-5c5e", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62",
  "Object": {"Commit": "...", "Files": [{"Path": "docs/automation/evidence/I-62-F6/FX-02/bmin/...", "Blob": "..."}]},
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión que preparó B-min y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "ContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "OrderReadWhole": "true | false | null", "Decisions75DeclaresOrderBytesAndSha256": "true | false | null",
            "Decisions75Point4MatchesOrderLine30": "true | false | null", "PackageFileSetMatchesTable": "true | false | null",
            "SpecHeaderNamesDecisions75": "true | false | null", "Note": "..."},
  "Reads": ["ruta: rangos"],
  "Verdict": "NOT_MATERIAL | MATERIAL | BLOCKED_OWNER_DECISION | NOT_ACCREDITED",
  "ApplicableClauses": [{"Ref": "ruta:inicio-fin", "Excerpt": "texto literal de esas líneas (≤ 200 caracteres)", "Relevance": "..."}],
  "ActionCompatibility": {"Classification": "READ | ACTION_COMPATIBLE | ACTION_INCOMPATIBLE | CONDITIONAL_NOT_TRIGGERED | NOT_DETERMINABLE", "Rationale": "...", "ClauseRefs": ["ruta:línea"]},
  "CanonicalInputAdmissible": {"Admissible": "yes | no | needs-change | NOT_ASSESSED", "Rationale": "...", "ClauseRefs": []},
  "Materiality": [{"Id": "M-01 | … | M-08", "Activated": "yes | no | UNKNOWN", "Rationale": "...", "ClauseRefs": []}],
  "AffectedSurfaces": [{"Surface": "AP_16_4 | AP_16_19 | AP_16_24 | ADR_0046 | V14_D3 | CONSUMED_CONTRACTS | OTHER", "Name": "...",
                        "Effect": "NOT_AFFECTED | WITHIN_CURRENT_TEXT | REQUIRES_NON_MATERIAL_CHANGE | REQUIRES_MATERIAL_CHANGE | UNKNOWN", "Rationale": "...", "ClauseRefs": []}],
  "Independence": {"Dimensions": [{"Dimension": "ACTOR | SESSION | CONTEXT | PROVIDER | C24_ORACLE | TOOL_ACCEPTOR_DIVERSITY",
                                   "Effect": "NO_EFFECT | PRESERVED_WITH_CONDITIONS | STRENGTHENED | WEAKENED | UNKNOWN", "Rationale": "...", "ClauseRefs": []}],
                   "Consequence": "..."},
  "A6": {"Needed": "yes | no | NOT_ASSESSED", "MinimalDeltaScope": ["cláusula o superficie (≤ 200 caracteres), nunca texto de enmienda"], "Rationale": "...", "ClauseRefs": []},
  "OwnerReserved": [{"Decision": "...", "Why": "...", "ClauseRefs": []}],
  "SingleInvocationMeasurement": {"Possible": "yes | no | UNKNOWN", "Conditions": ["..."], "Rationale": "...", "ClauseRefs": []},
  "Observations": ["..."],
  "KnownLimitations": ["..."]
}
```

- Los valores de la plantilla son formas, no respuestas: cada campo lleva **tu** comprobación y **tu** evaluación. Donde la plantilla dice
  `"true | false | null"`, el valor es un booleano JSON o `null`, no un texto.
- `Step0`: las cinco booleanas son lo que compruebas en el Paso 0; `null` en lo que no compruebes.
- Cobertura, una vez cada elemento: `Materiality`, M-01..M-08; `AffectedSurfaces`, las seis superficies nombradas (y las `OTHER` que añadas);
  `Independence.Dimensions`, las seis dimensiones.
- **Con un veredicto** (todo salvo NOT_ACCREDITED):
  - las cinco booleanas de `Step0` en `true`;
  - `Object` = el commit del paquete y los archivos de la tabla, con sus blobs;
  - `ApplicableClauses` no vacía, y al menos una `ClauseRef` en `ActionCompatibility`, en `CanonicalInputAdmissible`, en `A6`, en cada fila de
    `Materiality` y en cada elemento de `OwnerReserved`;
  - `CanonicalInputAdmissible.Admissible` y `A6.Needed` distintos de `NOT_ASSESSED`;
  - con `A6.Needed` = `yes`, `MinimalDeltaScope` no vacío; con `no`, vacío.
- **Con NOT_MATERIAL:** las ocho filas de `Materiality` en `no`, `A6.Needed` = `no` y ninguna superficie en `REQUIRES_MATERIAL_CHANGE` ni en
  `UNKNOWN`.
- **Con MATERIAL:** al menos una fila de `Materiality` en `yes` o `UNKNOWN`, y `A6.Needed` = `yes`.
- **Con BLOCKED_OWNER_DECISION:** `OwnerReserved` no vacía.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - Lo no evaluado va así: `CanonicalInputAdmissible.Admissible` y `A6.Needed` en `NOT_ASSESSED`; `ActionCompatibility` en `NOT_DETERMINABLE`;
    `Materiality` con las ocho filas en `UNKNOWN`; `SingleInvocationMeasurement.Possible` en `UNKNOWN`; los `Rationale`, «no evaluado»; las
    booleanas de `Step0` que no comprobaste en `null`; `Object` con lo observado o con `null`.
  - Las demás listas pueden ir vacías.