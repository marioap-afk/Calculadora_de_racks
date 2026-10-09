I-62 — RE-REVISIÓN FORMAL DEL ARCHITECT DE LA ENMIENDA A-4 CORREGIDA (recuperación de FX-02 ante una limitación medida del runtime, F6-OBS-03), preparada según la disposición del Coordinator §62, punto 2, las decisiones §57, punto 3, y la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento automático.

Eres el ARCHITECT de esta re-revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-4, del Coordinator y de las revisiones anteriores, incluida la revisión 1 de A-4. Revisas en solo lectura la versión exacta y devuelves un único veredicto formal de LIFECYCLE §6. No implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261009T150948Z-fdc2
InvocationId           = I20261009T150948Z-fdc2
LogicalReviewRequestId = L20261009T150948Z-fdc2   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-a4r   (tu directorio de trabajo; su única rama, main, está en el commit; sin remoto)
Commit                 = cca8c60e4acfea498844c5d5f8ea14b807fbd4f3   (recibo: decisiones §62, corrección 2 con guardas PASS)
Corrección 2           = a33324957f8778dd286a5a871881d403fd5a2d14   (A-4 corregida, paquete nuevo, guardas ampliadas)
Revisión 1             = a5b50c689ef33aa7b6fa7f7949fd72f58202e369   (custodia de la revisión 1, R20261009T122416Z-bce3, sobre el objeto anterior)
Historia de A-4        = 27ffa26b (4710084b, candidata de la orden §61) → 7d863219 (7f065a3a, revisión 1) → 0d954376 (a3332495, esta versión)
Objeto                 = docs/initiatives/I-62-A-4.md                          blob 0d9543761e3e3a45a94338776f7c1ba763224ab3
Objeto anterior        = blob 7d863219a271b5ec427ef8669435826e19be8f11 = D:\r62-arch-a4r-run\A-4.7d863219.md
Paquete                = docs/initiatives/I-62-architect-package-A-4.md        blob 59052b847ccc13e8be539189ae1b77dc7f9d3623
Revisión 1 (salida)    = docs/automation/evidence/I-62-architect-A-4/R20261009T122416Z-bce3/output.json   blob 8477374495d809bed64283d8af2976e3ce2929e8
Revisión 1 (registro)  = docs/initiatives/I-62-architect-review-A-4.md         blob b464f2c53d40d91cfde46e4a662995bc86c9f08a
Guardas                = docs/automation/evidence/I-62-A4/a4-guards.py             blob 6595d8730456d7a06fc3edbd833dfa1eef11a1bc
                         docs/automation/evidence/I-62-A4/a4-selftest.json         blob ddaf18f6b6df3692836761685630c62417a2d171
                         docs/automation/evidence/I-62-A4/a4-guards-result-c2.json blob 2fce7b58f9da20a17fa19f4955a1bc984e8f0022
Freeze                 = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; Proposal V14 commit 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Orden vigente          = D:\r62-arch-a4r-run\order.txt       (disposición §62 del Coordinator; 3 035 bytes, SHA-256 560e043ddec6c64d5ab0ce5ef9025603cb2723b26b017f94924b9dd06479f952)
Orden de la revisión 1 = D:\r62-arch-a4r-run\order-s61.txt   (disposición §61; 3 196 bytes, SHA-256 c82456877990c172110d577d21cc592ed87ebc94dea20387eecb4061c17634a7)
Origen de A-4          = D:\r62-arch-a4r-run\order-s60.txt   (disposición §60; 2 047 bytes, SHA-256 3a7257e16c955e586b2e6c300f42ba20079f0b29481dbb63f461c2a1c2217699)
```

**Recibo de publicación.** El paquete designa el objeto como «blob <el del commit de publicación> (borrador de esta pasada: 0d954376…)». En el
commit de arriba el objeto es `0d954376…` y el paquete `59052b84…`. Todos los insumos del paquete §2 tienen el blob que nombra, salvo tres que
crecieron después de `524b293e`: decisiones (`625a7075…` es prefijo exacto de `83269536…`, que añade §61 y §62), el README del kit de FX-02
(`fd7ef02d…` es prefijo exacto de `ceb38284…`, que añade la línea 356) y la evidencia (`d132626e…` → `a50b1dcb…`: añade §96-§105 y, en §96, la
línea 3029 pasó a ser tres líneas; §84 y §93-§95 son idénticas). El invocador lo comprobó.

**Objeto y re-revisión.** La revisión 1 (sobre `7d863219`) dio CHANGES REQUIRED con A62-A4-01 (REQUIRED) y A62-A4-O1..O8 (OPTIONAL), y el
auditor la dejó NOT_ACCREDITED por una premisa citada con la línea desplazada en uno. La orden vigente (`order.txt`, punto 1) acepta A62-A4-01 como
REQUIRED y O1..O8 como no bloqueantes, y su punto 2 ordena esta re-revisión de la corrección publicada. El objeto es `0d954376`; compruébalo en el
Paso 0. Los commits del Freeze (`b64a3b64`) y de V14 (`4c617e82`) son anteriores a un rebase y no están en el clon: V14 se identifica por su blob.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git` ni las guardas.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, rama única `main`, sin remoto, árbol limpio, los blobs de
  arriba y el SHA-256 de los archivos del run, y que `delta.diff` y `A-4.7d863219.md` son la salida literal de `git` en el clon. El auditor la vuelve
  a verificar después. En `IdentityCheck` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. Lee entera, con Read, `D:\r62-arch-a4r-run\order.txt` (la orden vigente, 55 líneas). Su punto 2 (líneas 15-30) ordena esta re-revisión y
   nombra sus ocho temas (líneas 21-28).
2. Lee con Read las líneas 1-4 de `D:\r62-arch-a4r-run\delta.diff`. La línea `index` debe nombrar `7d863219..0d954376`, y las rutas `a/` y `b/`,
   `docs/initiatives/I-62-A-4.md`.
3. Lee con Read las líneas 1-52 del paquete (`D:\r62-arch-a4r\docs\initiatives\I-62-architect-package-A-4.md`). Su cabecera (1-28) debe designar
   `docs/initiatives/I-62-A-4.md` con el borrador `0d954376…`, y su §0 (30-52) describe el hallazgo y el delta que se re-revisan.
4. Lee con Read, de `D:\r62-arch-a4r\docs\automation\evidence\I-62-A4\a4-guards-result-c2.json`, las líneas 1-7 (`Mode` CORRECTION2, `Head` = la
   corrección 2, `A4Blob` = el blob del objeto) y 1421-1437 (la nota de G5 «frente a 7d863219…» y G5b); y, de
   `D:\r62-arch-a4r\docs\automation\evidence\I-62-A4\a4-selftest.json`, las líneas 30-50 (`A4TextBlob`).

Si algo no coincide, o si un archivo del run no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`D:\r62-arch-a4r-run\`):
- `order.txt`: la orden vigente (disposición §62; 3 035 bytes, 55 líneas; punto 1, líneas 3-14; punto 2, líneas 15-30; U-09 (e), línea 36);
- `order-s61.txt`: la orden de la revisión 1 (disposición §61; 3 196 bytes, 57 líneas; su punto 2, líneas 11-18, nombra la candidata `27ffa26b`);
- `order-s60.txt`: el origen de A-4 (disposición §60; 2 047 bytes, 53 líneas; su punto 2, líneas 13-26);
- `delta.diff`: la salida literal de `git diff a5b50c68 a3332495 -- docs/initiatives/I-62-A-4.md` (25 767 bytes, 196 líneas). Es el cambio frente
  al blob revisado `7d863219`, que `a5b50c68` contiene sin cambios desde `7f065a3a`; sus líneas de cambio son las del diff blob a blob
  `7d863219..0d954376`;
- `A-4.7d863219.md`: el objeto anterior literal, salida de `git show a5b50c68:docs/initiatives/I-62-A-4.md` (40 944 bytes, 379 líneas): cabecera
  1-47; §1 49-60; §2 62-128; §3 130-271 (3.1 132-148; 3.2 A4-1 150-173; 3.3 A4-2 175-195; 3.4 A4-3 197-218; 3.5 A4-4 220-244; 3.6 A4-5 246-258;
  3.7 260-271); §4 273-288; §5 290-301; §6 303-311; §7 313-326; §8 328-345; §9 347-354; §10 356-365; anexo 367-379;
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es premisa.

Los SHA-256 de los tres textos de orden son los que declaran decisiones §62, §61 y §60 en el commit.

**Canónicos.** Son la tabla de insumos del paquete §2 (incluidos la salida de la revisión 1 y el validador de producción de F4), el propio paquete,
las guardas de A-4 y, por orden del Coordinator, el registro de la revisión 1, la solicitud de desbloqueo de FX-02 y su clasificación, en el commit,
con rutas relativas al clon `D:\r62-arch-a4r`. El paquete §2 no lista el registro, la solicitud, la clasificación ni las guardas; aquí entran por
orden del Coordinator. La solicitud es un registro de propuestas, no de autorizaciones; decisiones §62, punto 3, dispone solo las partes que nombra.
Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por tramos de 300 líneas como máximo si pasa de 60 000 bytes.
1. `docs/initiatives/I-62-A-4.md`, el objeto, **entero** (48 685 bytes y 423 líneas): cabecera 1-51; §1 53-64; §2 66-132; §3 134-297 (3.1 136-152;
   3.2 A4-1 154-180; 3.3 A4-2 182-202; 3.4 A4-3 204-225; 3.5 A4-4 227-257, regla 5 249-257; 3.6 A4-5 259-271; 3.7 273-297); §4 299-315;
   §5 317-328 (M-02 322; M-05 325); §6 330-341; §7 343-356; §8 358-382 (Q-A4-01..Q-A4-17 362-378; disposición de la revisión 1 380-382);
   §9 384-391; §10 393-402; anexo 404-423.
2. `docs/initiatives/I-62-architect-package-A-4.md`, entero (176 líneas): cabecera 1-28; §0 30-52; §1 54-69; §2 71-99; §3 101-128; §4 130-156;
   §5 158-168; §6 170-176.
3. `docs/automation/evidence/I-62-architect-A-4/R20261009T122416Z-bce3/output.json` (65 855 bytes, 1 139 líneas), la salida de la revisión 1:
   `Verdict` 73; A62-A4-01 75-224; A62-A4-O1 227-261; O2 262-289; O3 290-331; O4 332-359; O5 360-401; O6 402-422; O7 423-464; O8 465-485;
   `Focus` 487-725; `A45Verdict` 726-730; `QuestionDispositions` 731-1007; `NoChangeConfirmation` 1008-1053.
4. `tests/RackCad.Tests/I62/StateV2Validator.Orchestration.cs` (59 224 bytes, 937 líneas): 119-124.
5. `docs/initiatives/I-62-proposal-v14.md` (342 761 bytes): D.3 2431-2528 (fila «Codex, total» en 2442); D.4 2530-2554; D.5 2556-2565; §18, filas
   OD 970-975; §8.4 380-397; §9.2, T3, T3' y T10 613-621; §9.3 649-663; §13, P-22 788; §20.3.2 1132-1139; B.4 1852-1869; B.5 1871-1884; B.8.1,
   `closure` 1953-1955; B.8.3 1992-2004; I-S18 2171-2176; F.1 3074-3086.
6. `docs/initiatives/I-62-A-2.md`: cabecera 1-48; §3 A2-P2 179-239.
7. `docs/initiatives/I-62-A-1.md` (77 984 bytes): cabecera 1-56; §2 FC-01 90-167 (fila D1-21 en 164).
8. `docs/initiatives/I-62-A-3.md` (200 993 bytes): solo contexto, cabecera 1-62 (paquete §2: no hace falta leerla entera).
9. `docs/initiatives/I-62-consensus-freeze.md` (113 líneas).
10. `docs/INITIATIVE_LIFECYCLE.md`: §3 66-94; §5 152-191; §6 193-222.
11. `docs/AUTOMATION_PLAN.md` (155 229 bytes): 16.4 507-543; 16.8 600-618; 16.19-16.21 1043-1180; 16.25 1325-1373.
12. `docs/automation/agent-execution/README.md` (60 779 bytes): §14 369-531 (B1-B10 387-403; referencias 429-437; independencia 439-486;
    A1'-A8' 500-509).
13. `docs/automation/decisions/I-62.md` (198 226 bytes): §55-§62 1053-1255 (§55 1053-1074; §56 1076-1103; §57 1105-1117; §58 1119-1145; §59
    1147-1162; §60 1164-1189; §61 1191-1204; §62 1206-1255).
14. `docs/automation/evidence/I-62-evidence.md` (280 374 bytes): §84 2820-2842; §93-§95 2975-3019; §101-§105 3104-3209 (§101 3104-3119; §102
    3121-3142; §103 3144-3163; §104 3165-3194; §105 3196-3209).
15. `docs/automation/evidence/I-62-F6/FX-02/F6-OBS-03-cmd-route-evaluation.md`, entero (61 líneas).
16. `docs/automation/evidence/I-62-F6/OD-2/R20261009T041427Z-a2p2-block/result.json`, entero (294 líneas).
17. `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md`: §3.2 117-131; §4 133-166; §7 201-217 (210-212).
18. `docs/automation/evidence/I-62-F6/kits/FX-02/README.md` (93 002 bytes): 44; 140; 144; 167; 180; §4, punto 6, 243-249.
19. `docs/automation/evidence/I-62-F6/kits/FX-02/order-FX-U1-O4.template.md`: 28-31; 44; 88-124.
20. Descriptores, enteros: `docs/automation/agent-execution/adapters/codex-cli.md` (59 líneas) y `docs/automation/agent-execution/adapters/claude-cli.md`
    (52 líneas).
21. `docs/automation/agent-execution/routing.md`: §5 61-81; §8 95-119. `docs/automation/agent-execution/model-catalog.md`, entero (137 líneas).
22. `docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/README.md`, entero (15 líneas).
23. `docs/initiatives/I-62-architect-review-A-4.md`, entero (40 líneas), el registro de la revisión 1.
24. `docs/automation/evidence/I-62-F6/requests/FX-02-unblock-request-2026-10-09.md`, entero (173 líneas; §2.1 59-69, con H-1 en 63; fila U-09 en 79;
    §3 101-139).
25. `docs/automation/evidence/I-62-F6/requests/FX-02-decision-classification-2026-10-09.md`, entero (201 líneas; fila 9, U-09 (e), en 41; §2
    108-139).
26. Las guardas y sus resultados, enteros (evidencia de apoyo, no autoridad): `docs/automation/evidence/I-62-A4/a4-guards.py` (120 554 bytes,
    1 815 líneas), `docs/automation/evidence/I-62-A4/a4-selftest.json` (1 183 líneas; `A4TextBlob` en 36 y 48) y
    `docs/automation/evidence/I-62-A4/a4-guards-result-c2.json` (1 518 líneas: cabecera 1-7; G2C2 540-717; G3 718-728; G4 729-737; G5 738-1428,
    con la nota «frente a 7d863219» en 1422; G5b 1429-1437).

Nada más. Los documentos que estos archivos citan y no están en la lista quedan fuera del cierre.

## Reglas de lectura (el invocador audita después cada llamada)

Cada ruta se comprueba por su **efecto**: resuelta contra el directorio de trabajo, con `.` y `..` colapsados y los enlaces resueltos.

- **Read:**
  - usa la ruta absoluta, solo de archivos del cierre bajo `D:\r62-arch-a4r` o de los seis archivos del run;
  - en los archivos de más de 60 000 bytes (V14, A-1, A-3, AUTOMATION_PLAN, el README de agent-execution, decisiones, evidencia, el README del kit de
    FX-02, `output.json` de la revisión 1 y `a4-guards.py`), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** no la necesitas, porque las rutas están arriba. Si la usas, su alcance (`path` y patrón) solo puede contener archivos de la lista. Un
  patrón que alcance cualquier otro archivo cuenta como lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, el fixture
    (`D:\r62-fixture\*`), el resto del directorio de la revisión 1 (solo su `output.json` está en el cierre) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una premisa.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: V14 2352, 2380 y 2382; A-3 1487; decisiones 657, 734, 750, 768, 787, 805 y 819;
  README del kit de FX-02 340 y 355. Una línea truncada no cuenta como entregada: si te hiciera falta, no la cites como premisa y decláralo en
  `KnownLimitations`.
- **Contexto.** Lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el auditor cuenta como
  un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Premisas

- Cita como premisa (`PremiseRefs`) solo líneas que hayas leído enteras con Read:
  - de archivos del cierre, con `Path` relativo al clon (p. ej., `docs/initiatives/I-62-A-4.md`);
  - de `order.txt`, `order-s61.txt`, `order-s60.txt`, `delta.diff` o `A-4.7d863219.md`, con `Path` = `D:\r62-arch-a4r-run\<nombre>`.
- `prompt.md` nunca es premisa.
- `Quote` es el texto literal de esas líneas; en `delta.diff`, con su marca inicial (`+`, `-` o espacio). El invocador comprueba que la cita está en
  esas líneas y que Read te las entregó fielmente.
- **Números de línea exactos.** `LineStart` y `LineEnd` son los números que Read muestra a la izquierda de las líneas citadas, y el texto de `Quote`
  tiene que estar dentro de ese rango. Una premisa con la línea desplazada, aunque sea en uno, no se acredita: la revisión 1 quedó NOT_ACCREDITED
  por eso. Antes de entregar, contrasta cada `PremiseRefs` con la salida de Read de la que sale.

## Qué decidir

Se pide el veredicto del paquete §1 (líneas 54-69) sobre la A-4 exacta. No se te prohíbe señalar otro defecto material que encuentres.

- **Hallazgos anteriores** (`PriorFindingDispositions`), sobre la versión exacta y cada uno una vez:
  - A62-A4-01 (REQUIRED de la revisión 1): `CLOSED` o `STILL_OPEN`, con su base en `PremiseRefs` (al menos una premisa);
  - A62-A4-O1..A62-A4-O8 (OPTIONAL de la revisión 1): `APPLIED`, `NOT_APPLIED` o `N/A`, con su base;
  - en `LinkedFindingIds`, los hallazgos de esta re-revisión que sostienen un `STILL_OPEN` o un `NOT_APPLIED`, o que tratan un efecto lateral de la
    corrección.
- **`Focus`:** una disposición explícita de cada uno de estos ocho temas (orden §62, punto 2), una vez cada uno, como REQUIRED, OPTIONAL o NO_FINDING
  (sin hallazgo), con los `FindingIds` que la sostienen (ninguno con NO_FINDING) y la base. Cada tema contesta también las preguntas del paquete §4
  (líneas 130-156) que le corresponden:
  - `H1_A4-3_NO_FABRICATED_ACCEPTANCE`: H-1 y la ausencia de aceptación fabricada (A4-3; paquete §4, pregunta 8);
  - `H2_A4-4_REAL_INDEPENDENCE`: H-2 y la verificación de una independencia real (A4-4, incluidos el discriminador y los consumidores de la regla 5;
    paquete §4, preguntas 9 y 11);
  - `H3_A4-5_NO_BUDGET_RESET`: H-3 y presupuestos sin reinicios (A4-5; paquete §4, pregunta 10);
  - `F6-OBS-03_A4-1_CMD_ROUTE`: F6-OBS-03 y la ruta `cmd.exe` (A4-1, incluidas las inserciones de A62-A4-O1 y A62-A4-O2 en sus reglas 2 y 3; paquete
    §4, preguntas 1-3 y 11);
  - `A4-2_ALT_ARCHITECT_CLAUDE_CLI`: el Architect alternativo por `claude-cli` (A4-2; paquete §4, preguntas 1, 4 y 5);
  - `MATERIALITY_M02_M05`: la materialidad M-02/M-05 (§5, cabecera y §3.7; paquete §4, preguntas 6 y 11);
  - `U-09e`: U-09 (e), la resolución acreditada de A1'-A8' sin decisión ficticia ni adelanto de resultados desconocidos, revisada con A4-3
    (`order.txt`, línea 36; Q-A4-11);
  - `A4-5_SEPARABILITY`: la separabilidad de A4-5 (§3.6 y §3.7).
- **`A45Verdict`:** el veredicto separado de A4-5 que pide el paquete §1 («el acuerdo puede excluirla»): AGREED, CHANGES REQUIRED, EXCLUDE (el acuerdo
  la excluye) o BLOCKED — OWNER DECISION, con los `FindingIds` que lo sostienen y la base.
- **`QuestionDispositions`:** Q-A4-01..Q-A4-17 de A-4 §8 (líneas 362-378), cada una una vez, como REQUIRED, OPTIONAL o NO_FINDING, con sus
  `FindingIds` (ninguno con NO_FINDING).
- **`DeltaConfirmation`:** si es cierto lo que el paquete §0 (líneas 49-52) declara igual a `7d863219`, comparando el objeto con `A-4.7d863219.md` y
  `delta.diff`: los párrafos de A4-2, A4-3 y A4-5, byte a byte (`A42IdenticalTo7d863219`, `A43IdenticalTo7d863219`, `A45IdenticalTo7d863219`), y
  A4-4, introducción y reglas 1-4 (`A44Rules1to4IdenticalTo7d863219`): CONFIRMED, NOT_CONFIRMED o NOT_DETERMINED, con la base de cada respuesta.
- **Además**, según el paquete §1:
  - si la aplicación de A62-A4-O1..O8 cambia el significado de alguna regla (en las disposiciones de los hallazgos anteriores y, si lo hace, como
    hallazgo);
  - si hace falta o no una decisión del Owner para el acuerdo (`OwnerAuthorityCheck`);
  - si las dos líneas del Owner de A-4 §7 (`A4-SONDA-CONSUMO` y `A4-CLAUDE-FX02-CONSUMO`) solo autorizan consumo o van más allá (aceptan resultados
    por adelantado, amplían OD-3, OD-5 o CLAUDE-CLI-I62, u otra autoridad del Owner) (`OwnerConsumptionLines`: ONLY_CONSUMPTION, EXCEEDS_CONSUMPTION
    o NOT_DETERMINED, con la base);
  - si A-4 cambia o no A-1, A-2, A-3, la fila de topes de D.3, P-01, OD-2, la receta 16.4, los descriptores, el texto de AUTOMATION_PLAN
    16.20-16.21, el texto de B1-B10 y producción (`NoChangeConfirmation`: NO_CHANGE, CHANGE o NOT_DETERMINED, con la base de cada respuesta);
  - tu modo de revisión, si revisor y autor son la misma persona y el contexto que el runtime te inyectó (`ReviewerMode`, `SamePersonAsAuthor`,
    `InjectedContextDeclaration`). Declara también el proveedor del revisor y del autor (paquete §5) en `SamePersonAsAuthor`. Los
    identificadores de cuenta que el runtime te inyecte (correo, uuid de organización u otros) se declaran solo por su tipo, sin reproducir su
    valor;
  - la identidad revisada: commit, ruta y blob.
- **`GuardsVerification`:** no puedes ejecutar las guardas. Lo que afirmes de G1, G2C2, G2D2, G3, G4, G5 y G5b sale de leer `a4-guards.py` y los
  resultados custodiados; lo que no puedas establecer así va como `NOT_VERIFIED`.

Un REQUIRED existe solo si el defecto es material según LIFECYCLE §5, y debe traer:
- un id estable;
- la sección exacta de A-4 (`AffectedDelta`);
- la premisa canónica completa en `PremiseRefs`;
- la autoridad o invariante violado (`ViolatedInvariant`) o un contraejemplo concreto (`Counterexample`), al menos uno de los dos no vacío;
- por qué importa;
- la corrección exacta.

Una precisión sin cambio de significado es OPTIONAL. **Ids nuevos:** REQUIRED `A62-A4-02`, `A62-A4-03`…; OPTIONAL `A62-A4-O9`, `A62-A4-O10`… Un id
anterior aparece en `RequiredFindings` u `OptionalFindings` solo para reformularlo: A62-A4-01 si queda `STILL_OPEN`; A62-A4-O1..O8 si quedan
`NOT_APPLIED`.

## Resultado

Entrega el resultado con **una** llamada a StructuredOutput, con este objeto. Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-architect-review-result/v1 (representación experimental; rigen I-61 y LIFECYCLE)",
  "RunId": "R20261009T150948Z-fdc2", "InvocationId": "I20261009T150948Z-fdc2", "LogicalReviewRequestId": "L20261009T150948Z-fdc2", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN",
  "ReviewedUnit": "I-62", "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-4.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "IdentityCheck": {"Basis": "INVOKER_VERIFIED_CLONE", "DeltaIndexMatchesBlobs": "true | false | null", "DeltaPathIsObject": "true | false | null",
                    "PackageNamesObjectBlob": "true | false | null", "GuardsResultA4BlobMatchesObject": "true | false | null",
                    "GuardsResultComparesWithPreviousBlob": "true | false | null", "SelftestA4TextBlobMatchesObject": "true | false | null", "Note": "..."},
  "InputsRead": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "PriorFindingDispositions": [{"FindingId": "A62-A4-01 | A62-A4-O1 | … | A62-A4-O8", "Disposition": "CLOSED | STILL_OPEN | APPLIED | NOT_APPLIED | N/A",
                                "LinkedFindingIds": [], "Rationale": "...", "PremiseRefs": []}],
  "RequiredFindings": [{"FindingId": "A62-A4-NN", "AffectedDelta": "...", "ViolatedInvariant": "...", "Counterexample": "...", "CorrectionRequired": "...",
                        "PremiseRefs": [{"Path": "...", "Section": "...", "LineStart": 1, "LineEnd": 1, "Quote": "proposición completa"}], "WhyItMatters": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A4-ON", "AffectedDelta": "...", "Note": "...", "PremiseRefs": []}],
  "Focus": [{"Topic": "H1_A4-3_NO_FABRICATED_ACCEPTANCE | H2_A4-4_REAL_INDEPENDENCE | H3_A4-5_NO_BUDGET_RESET | F6-OBS-03_A4-1_CMD_ROUTE | A4-2_ALT_ARCHITECT_CLAUDE_CLI | MATERIALITY_M02_M05 | U-09e | A4-5_SEPARABILITY",
             "Disposition": "REQUIRED | OPTIONAL | NO_FINDING", "FindingIds": [], "Rationale": "...", "PremiseRefs": []}],
  "A45Verdict": {"Verdict": "AGREED | CHANGES REQUIRED | EXCLUDE | BLOCKED — OWNER DECISION | NOT_ASSESSED", "FindingIds": [], "Rationale": "..."},
  "QuestionDispositions": [{"Id": "Q-A4-01 | … | Q-A4-17", "Disposition": "REQUIRED | OPTIONAL | NO_FINDING", "FindingIds": [], "Rationale": "...", "PremiseRefs": []}],
  "DeltaConfirmation": {"A42IdenticalTo7d863219": {"Assessment": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "Note": "..."},
                        "A43IdenticalTo7d863219": {"Assessment": "...", "Note": "..."}, "A45IdenticalTo7d863219": {"Assessment": "...", "Note": "..."},
                        "A44Rules1to4IdenticalTo7d863219": {"Assessment": "...", "Note": "..."}},
  "NoChangeConfirmation": {"A1": {"Assessment": "NO_CHANGE | CHANGE | NOT_DETERMINED", "Note": "..."}, "A2": {"Assessment": "...", "Note": "..."},
                           "A3": {"Assessment": "...", "Note": "..."}, "D3CapsRow": {"Assessment": "...", "Note": "..."}, "P01": {"Assessment": "...", "Note": "..."},
                           "OD2": {"Assessment": "...", "Note": "..."}, "Recipe164": {"Assessment": "...", "Note": "..."}, "Descriptors": {"Assessment": "...", "Note": "..."},
                           "AutomationPlan1620to1621Text": {"Assessment": "...", "Note": "..."}, "B1toB10Text": {"Assessment": "...", "Note": "..."},
                           "Production": {"Assessment": "...", "Note": "..."}},
  "OwnerConsumptionLines": {"Assessment": "ONLY_CONSUMPTION | EXCEEDS_CONSUMPTION | NOT_DETERMINED", "Note": "..."},
  "GuardsVerification": {"Notes": [{"Guard": "G1 | G2C2 | G2D2 | G3 | G4 | G5 | G5b", "ClaimHolds": "HOLDS | DOES_NOT_HOLD | NOT_VERIFIED", "Note": "..."}], "Note": "..."},
  "Materiality": {"Overall": {"M01": "YES | NO", "M02": "...", "M03": "...", "M04": "...", "M05": "...", "M06": "...", "M07": "...", "M08": "..."}, "Note": "tu evaluación"},
  "OwnerAuthorityCheck": {"Spending": "NONE | ...", "Credentials": "NONE | ...", "Destructive": "NONE | ...", "NewOVScenario": "NONE | ...",
                          "OVRemovalOrReassignment": "NONE | ...", "OwnerPolicyChoice": "NONE | ...", "NewOwnerAuthority": "NONE | ...", "OwnerDecisionRequired": "true | false"},
  "IfAgreed": {"A41Agreed": null, "A42Agreed": null, "A43Agreed": null, "A44Agreed": null, "PriorRequiredClosed": null, "NoOwnerDecisionRequired": null,
               "NoChangeToProtectedSurfaces": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: `IdentityCheck`, `PriorFindingDispositions`, `Focus`, `A45Verdict`, `DeltaConfirmation`,
  `Materiality`, `OwnerAuthorityCheck`, `OwnerConsumptionLines`, `NoChangeConfirmation` y `GuardsVerification` llevan **tu** comprobación y **tu**
  evaluación. Donde la plantilla dice `"true | false"` o `"true | false | null"`, el valor es un booleano JSON (o `null`), no un texto.
- `IdentityCheck`: las seis booleanas son lo que compruebas en el Paso 0; `null` en lo que no compruebes.
- Cobertura, una vez cada elemento:
  - `PriorFindingDispositions`: A62-A4-01 y A62-A4-O1..A62-A4-O8. A62-A4-01 va en `CLOSED` o `STILL_OPEN`, con al menos una premisa; cada
    A62-A4-On, en `APPLIED`, `NOT_APPLIED` o `N/A`. Un `STILL_OPEN` o un `NOT_APPLIED` cita en `LinkedFindingIds` al menos un hallazgo de esta
    re-revisión (para A62-A4-01 `STILL_OPEN`, un REQUIRED); todo id de `LinkedFindingIds` existe en `RequiredFindings` u `OptionalFindings`;
  - `Focus`: los ocho temas;
  - `QuestionDispositions`: Q-A4-01..Q-A4-17.
  En `Focus` y `QuestionDispositions`, una disposición REQUIRED u OPTIONAL cita en `FindingIds` hallazgos de `RequiredFindings` u `OptionalFindings`,
  respectivamente.
- **`A45Verdict`:** con CHANGES REQUIRED cita en `FindingIds` al menos un REQUIRED; con BLOCKED — OWNER DECISION, `OwnerDecisionRequired` = true;
  todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`.
- **Con AGREED** (A-4 entera, o sin A4-5 si `A45Verdict` = EXCLUDE):
  - cero REQUIRED (también en `Focus` y en `QuestionDispositions`) y A62-A4-01 `CLOSED`;
  - `A45Verdict` en AGREED o EXCLUDE;
  - `OwnerDecisionRequired` = false y `OwnerConsumptionLines` en ONLY_CONSUMPTION;
  - los once apartados de `NoChangeConfirmation` en NO_CHANGE y los cuatro de `DeltaConfirmation` en CONFIRMED;
  - los ocho campos de `IfAgreed` en `true`.
  En cualquier otro caso, `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true.
- **Con un veredicto** (todo salvo NOT_ACCREDITED): `Materiality.Overall` en YES o NO, `A45Verdict` distinto de NOT_ASSESSED, y ni las booleanas de
  `IdentityCheck` ni `OwnerDecisionRequired` en `null`.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `PriorFindingDispositions`, `Focus` y `QuestionDispositions` pueden ir vacíos.
  - Lo que no hayas evaluado va así: `Materiality.Overall` en `NOT_ASSESSED`; `A45Verdict` en `NOT_ASSESSED`; en `null` las booleanas de
    `IdentityCheck` que no comprobaste y `OwnerDecisionRequired`; `ReviewedCommit` y `ReviewedBlob` con lo observado o en `null`;
    `NoChangeConfirmation` y `OwnerConsumptionLines` en `NOT_DETERMINED`; `DeltaConfirmation` en `NOT_DETERMINED`.
  - `NOT_ASSESSED` y `null` en esos campos solo valen con NOT_ACCREDITED.