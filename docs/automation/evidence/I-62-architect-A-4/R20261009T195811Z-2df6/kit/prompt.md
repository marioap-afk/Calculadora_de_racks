I-62 — SEGUNDA RE-REVISIÓN FORMAL DEL ARCHITECT DE LA ENMIENDA A-4 (corrección 3: recuperación de FX-02 ante una limitación medida del runtime, F6-OBS-03), preparada según la disposición del Coordinator §63, punto 6, las decisiones §57, punto 3, y la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento automático.

Eres el ARCHITECT de esta re-revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-4, del Coordinator y de las revisiones anteriores, incluidas las revisiones 1 y 2 de A-4. Revisas en solo lectura la versión exacta y devuelves un único veredicto formal de LIFECYCLE §6. No implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = R20261009T195811Z-2df6
InvocationId           = I20261009T195811Z-2df6
LogicalReviewRequestId = L20261009T195811Z-2df6   AttemptSeq = 1
Clon de lectura        = D:\r62-arch-a4r2   (tu directorio de trabajo; su única rama, main, está en el commit; sin remoto)
Commit                 = 24284a4a86f3f4b9552f8de3905f08fa0c81e84a   (recibo: guardas CORRECTION3 PASS sobre la corrección 3)
Corrección 3           = c2dbc22566cff9319f12e521d841b5bfcbb40e0c   (A-4 corregida, paquete nuevo, guardas ampliadas)
Base del delta         = b25dde6b2adda5ed80695373451a2f12721bd2b9   (contiene la A-4 revisada en la revisión 2 sin cambios desde d5b454fd)
Historia de A-4        = 27ffa26b (4710084b) → 7d863219 (7f065a3a; revisión 1) → 0d954376 (a3332495; revisión 2) → 5e2ba68e (c2dbc225; esta versión)
Objeto                 = docs/initiatives/I-62-A-4.md                          blob 5e2ba68efdc1b902aa1c28aff4453b4b6973be7f
Objeto anterior        = blob 0d9543761e3e3a45a94338776f7c1ba763224ab3 = D:\r62-arch-a4r2-run\A-4.0d954376.md   (revisión 2)
Versión de la rev. 1   = blob 7d863219a271b5ec427ef8669435826e19be8f11 = D:\r62-arch-a4r2-run\A-4.7d863219.md   (revisión 1)
Paquete                = docs/initiatives/I-62-architect-package-A-4.md        blob 57c060e15f14ade6aefeda246adf26b2adaf2594
Revisión 2 (salida)    = docs/automation/evidence/I-62-architect-A-4/R20261009T150948Z-fdc2/output.json   blob 54986dab…
Revisión 2 (registro)  = docs/initiatives/I-62-architect-review-A-4-r2.md       blob d19d849d…
Revisión 1 (salida)    = docs/automation/evidence/I-62-architect-A-4/R20261009T122416Z-bce3/output.json   blob 84773744…
Guardas                = docs/automation/evidence/I-62-A4/a4-guards.py             blob 27748730b64dc9df3c30db71cb521ec0806e5d29
                         docs/automation/evidence/I-62-A4/a4-selftest.json         blob 94dbe94b5a87b5cf0474524adc312d39b9f44666
                         docs/automation/evidence/I-62-A4/a4-guards-result-c3.json blob 4d939646208f6769d83c7ec565e0fc6f66a9214e
Freeze                 = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; Proposal V14 commit 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Orden vigente          = D:\r62-arch-a4r2-run\order.txt       (disposición §63 del Coordinator; 3 345 bytes, SHA-256 00418a5468c96eb1528019263ef259b16914ea7c37da53274ea9fb3ca3a97117)
Orden de la revisión 2 = D:\r62-arch-a4r2-run\order-s62.txt   (disposición §62; 3 035 bytes, SHA-256 560e043ddec6c64d5ab0ce5ef9025603cb2723b26b017f94924b9dd06479f952)
Orden de la revisión 1 = D:\r62-arch-a4r2-run\order-s61.txt   (disposición §61; 3 196 bytes, SHA-256 c82456877990c172110d577d21cc592ed87ebc94dea20387eecb4061c17634a7)
```

**Recibo de publicación.** El paquete designa el objeto como «blob <el del commit de publicación> (borrador de esta pasada: 5e2ba68e…)». En el
commit de arriba el objeto es `5e2ba68e…` y el paquete `57c060e1…`. Todos los insumos del paquete §2 tienen el blob que nombra, salvo tres que
cambiaron después de `524b293e`: decisiones (`625a7075…` es prefijo exacto de `01905d16…`, que es el blob que el paquete nombra «en `d5b454fd`» y
añade §61-§63), la evidencia (`d132626e…` → `1b1431dc…`: añade §96-§112 y, en §96, la línea 3029 pasó a ser tres líneas; §84 y §93-§95 son
idénticas) y el README del kit de FX-02 (`fd7ef02d…` → `2439837a…`: cambia su línea 8 y añade la línea 356; las líneas 44, 140, 144, 167, 180 y
243-249, las que cita el paquete, son idénticas). El invocador lo comprobó.

**Objeto y re-revisión.** La revisión 2 (sobre `0d954376`) dio CHANGES REQUIRED con A62-A4-02 (REQUIRED) y A62-A4-O9..O10 (OPTIONAL), con
A62-A4-01 CLOSED; el auditor la acreditó. La orden vigente (`order.txt`, punto 1) conserva el veredicto, deja A62-A4-01 CLOSED, acepta A62-A4-02
como REQUIRED con la opción B y O9-O10 como no bloqueantes; su punto 2 pide el delta de U-14 y su punto 6 ordena la corrección 3 y esta
re-revisión. El objeto es `5e2ba68e`; compruébalo en el Paso 0. Los commits del Freeze (`b64a3b64`) y de V14 (`4c617e82`) son anteriores a un rebase
y no están en el clon: V14 se identifica por su blob.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git` ni las guardas.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, rama única `main`, sin remoto, árbol limpio, los blobs de
  arriba y el SHA-256 de los archivos del run, y que `delta.diff`, `A-4.0d954376.md` y `A-4.7d863219.md` son la salida literal de `git` en el clon.
  El auditor la vuelve a verificar después. En `IdentityCheck` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. Lee entera, con Read, `D:\r62-arch-a4r2-run\order.txt` (la orden vigente, 74 líneas). Su punto 6 (líneas 33-43) ordena la corrección 3 y esta
   re-revisión.
2. Lee con Read las líneas 1-4 de `D:\r62-arch-a4r2-run\delta.diff`. La línea `index` debe nombrar `0d954376..5e2ba68e`, y las rutas `a/` y `b/`,
   `docs/initiatives/I-62-A-4.md`.
3. Lee con Read las líneas 1-71 del paquete (`D:\r62-arch-a4r2\docs\initiatives\I-62-architect-package-A-4.md`). Su cabecera (1-32) debe designar
   `docs/initiatives/I-62-A-4.md` con el borrador `5e2ba68e…`, y su §0 (34-71) describe las tres partes de la corrección 3 y el delta.
4. Lee con Read, de `D:\r62-arch-a4r2\docs\automation\evidence\I-62-A4\a4-guards-result-c3.json`, las líneas 1-7 (`Mode` CORRECTION3, `Head` = la
   corrección 3, `A4Blob` = el blob del objeto) y 1723-1739 (la nota de G5 «frente a 0d954376…» y G5b); y, de
   `D:\r62-arch-a4r2\docs\automation\evidence\I-62-A4\a4-selftest.json`, las líneas 48-65 (`A4TextBlob`).

Si algo no coincide, o si un archivo del run no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`D:\r62-arch-a4r2-run\`):
- `order.txt`: la orden vigente (disposición §63; 3 345 bytes, 74 líneas; punto 1, líneas 3-10; punto 2 (U-14), 11-18; punto 3 (U-09 (e)), 19-23;
  puntos 4 y 5, 24-32; punto 6, 33-43; punto 8 (Owner), 54-62);
- `order-s62.txt`: la orden de la revisión 2 (disposición §62; 3 035 bytes, 55 líneas; su punto 2, líneas 15-30);
- `order-s61.txt`: la orden de la revisión 1 (disposición §61; 3 196 bytes, 57 líneas; su punto 2, líneas 11-18);
- `delta.diff`: la salida literal de `git diff b25dde6b c2dbc225 -- docs/initiatives/I-62-A-4.md` (48 403 bytes, 370 líneas). Es el cambio frente al
  blob revisado `0d954376`; sus líneas de cambio son las del diff blob a blob `0d954376..5e2ba68e`;
- `A-4.0d954376.md`: el objeto anterior literal, salida de `git show b25dde6b:docs/initiatives/I-62-A-4.md` (48 685 bytes, 423 líneas): cabecera
  1-51; §1 53-64; §2 66-132; §3 134-297 (3.1 136-152; 3.2 A4-1 154-180; 3.3 A4-2 182-202; 3.4 A4-3 204-225; 3.5 A4-4 227-257; 3.6 A4-5 259-271;
  3.7 273-297); §4 299-315; §5 317-328; §6 330-341; §7 343-356; §8 358-382; §9 384-391; §10 393-402; anexo 404-423;
- `A-4.7d863219.md`: la versión de la revisión 1, salida literal de `git show a5b50c68:docs/initiatives/I-62-A-4.md` (40 944 bytes, 379 líneas; la
  fila «en `a5b50c68`» del paquete §2);
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es premisa.

Los SHA-256 de los tres textos de orden son los que declaran decisiones §63, §62 y §61 en el commit.

**Canónicos.** Son la tabla de insumos del paquete §2 (entre ellos las salidas de las revisiones 1 y 2, el registro de la revisión 2, la conciliación
de U-14 y el validador de producción de F4), el propio paquete, las guardas de A-4 y la solicitud de desbloqueo de FX-02 con su clasificación, que el
paquete §2 deja en el cierre como contexto, en el commit, con rutas relativas al clon `D:\r62-arch-a4r2`. La solicitud es un registro de
propuestas, no de autorizaciones. Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por tramos de 300 líneas como
máximo si pasa de 60 000 bytes.
1. `docs/initiatives/I-62-A-4.md`, el objeto, **entero** (62 415 bytes y 516 líneas; por tramos de 300 líneas como máximo): cabecera 1-56; §1 58-78;
   §2 80-159; §3 161-361 (3.1 163-183; 3.2 A4-1 185-211; 3.3 A4-2 213-233; 3.4 A4-3 235-256; 3.5 A4-4 258-287, regla 1 265-271, regla 5 280-287;
   3.6 A4-5 289-301; 3.7 303-343; 3.8 A4-6 345-361); §4 363-385; §5 387-398 (M-02 392; M-03 393; M-04 394; M-05 395); §6 400-417; §7 419-435 (las
   tres líneas del Owner, 431-435); §8 437-470 (Q-A4-01..Q-A4-22 441-462; disposiciones de las revisiones 1 y 2, 464-470); §9 472-479; §10 481-490;
   anexo 492-516.
2. `docs/initiatives/I-62-architect-package-A-4.md`, entero (211 líneas): cabecera 1-32; §0 34-71 ((A) 38-52; (B) 53-60; (C) 61-63; opcionales
   64-67; delta 68-71); §1 73-88; §2 90-123; §3 125-158; §4 160-190; §5 192-203; §6 205-211.
3. `docs/automation/evidence/I-62-architect-A-4/R20261009T150948Z-fdc2/output.json` (84 438 bytes, 1 452 líneas), la salida de la revisión 2:
   `Verdict` 71; `PriorFindingDispositions` 72-310 (A62-A4-01 73-138); A62-A4-02 312-496; A62-A4-O9 499-540; A62-A4-O10 541-582; `Focus` 584-963;
   `A45Verdict` 964-968; `QuestionDispositions` 969-1298; `DeltaConfirmation` 1299-1316; `NoChangeConfirmation` 1317-1362.
4. `docs/initiatives/I-62-architect-review-A-4-r2.md`, entero (34 líneas), el registro de la revisión 2.
5. `docs/automation/evidence/I-62-F6/FX-02/s62/u14-reconciliation.md`, entero (161 líneas; §2.1 sesiones de Principal 56-64; §3 presupuesto de
   Principal de la ronda A 95-111).
6. `docs/automation/evidence/I-62-architect-A-4/R20261009T122416Z-bce3/output.json` (65 855 bytes, 1 139 líneas), la salida de la revisión 1:
   A62-A4-01 75-224.
7. `tests/RackCad.Tests/I62/StateV2Validator.Orchestration.cs` (59 224 bytes, 937 líneas): 119-124.
8. `docs/initiatives/I-62-proposal-v14.md` (342 761 bytes): D.3 2431-2528 (fila Principal A en 2437; fila «Codex, total» en 2442; totales de la
   ronda A en 2523); D.4 2530-2554; D.5 2556-2565; §18, filas OD 970-975; §8.4 380-397; §9.2, T3, T3' y T10 613-621; §9.3 649-663; §13, P-22 788;
   §20.3.2 1132-1139; B.4 1852-1869; B.5 1871-1884; B.8.1, `closure` 1953-1955; B.8.3 1992-2004; I-S18 2171-2176; B.8.8, materialización
   autorizada, 2184-2185; F.1 3074-3086.
9. `docs/initiatives/I-62-A-2.md`: cabecera 1-48; §3 A2-P2 179-239.
10. `docs/initiatives/I-62-A-1.md` (77 984 bytes): cabecera 1-56; §2 FC-01 90-167 (fila D1-21 en 164).
11. `docs/initiatives/I-62-A-3.md` (200 993 bytes): solo contexto, cabecera 1-62 (paquete §2: no hace falta leerla entera).
12. `docs/initiatives/I-62-consensus-freeze.md` (113 líneas).
13. `docs/INITIATIVE_LIFECYCLE.md`: §3 66-94; §5 152-191; §6 193-222.
14. `docs/AUTOMATION_PLAN.md` (155 229 bytes): 16.4 507-543; 16.8 600-618; 16.19-16.21 1043-1180; 16.25 1325-1373.
15. `docs/automation/agent-execution/README.md` (60 779 bytes): §14 369-531 (B1-B10 387-403; referencias 429-437; independencia 439-486;
    A1'-A8' 500-509).
16. `docs/automation/decisions/I-62.md` (201 947 bytes): §51 972-991 (F6-OBS-01 en 982-985); §55-§63 1053-1272 (§55 1053-1074; §56 1076-1103;
    §57 1105-1117; §58 1119-1145; §59 1147-1162; §60 1164-1189; §61 1191-1204; §62 1206-1255; §63 1257-1272).
17. `docs/automation/evidence/I-62-evidence.md` (291 880 bytes): §84 2820-2842; §93-§95 2975-3019; §101-§112 3104-3337 (entre ellas §104
    3165-3194, §109 3270-3289, §110 3291-3304, §111 3306-3321 y §112 3323-3337).
18. `docs/automation/evidence/I-62-F6/FX-02/F6-OBS-03-cmd-route-evaluation.md`, entero (61 líneas).
19. `docs/automation/evidence/I-62-F6/OD-2/R20261009T041427Z-a2p2-block/result.json`, entero (294 líneas).
20. `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md`: §3.2 117-131; §4 133-166; §7 201-217 (210-212).
21. `docs/automation/evidence/I-62-F6/kits/FX-02/README.md` (93 023 bytes): 44; 140; 144; 167; 180; §4, punto 6, 243-249.
22. `docs/automation/evidence/I-62-F6/kits/FX-02/order-FX-U1-O4.template.md`: 28-31; 44; 88-124.
23. Descriptores, enteros: `docs/automation/agent-execution/adapters/codex-cli.md` (59 líneas) y `docs/automation/agent-execution/adapters/claude-cli.md`
    (52 líneas).
24. `docs/automation/agent-execution/routing.md`: §5 61-81; §8 95-119. `docs/automation/agent-execution/model-catalog.md`, entero (137 líneas).
25. `docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/README.md`, entero (15 líneas).
26. `docs/automation/evidence/I-62-F6/requests/FX-02-unblock-request-2026-10-09.md`, entero (173 líneas; §2.1 59-69, con H-1 en 63; fila U-09 en 79).
27. `docs/automation/evidence/I-62-F6/requests/FX-02-decision-classification-2026-10-09.md`, entero (201 líneas; fila 9, U-09 (e), en 41).
28. Las guardas y sus resultados, enteros (evidencia de apoyo, no autoridad): `docs/automation/evidence/I-62-A4/a4-guards.py` (146 701 bytes,
    2 137 líneas), `docs/automation/evidence/I-62-A4/a4-selftest.json` (1 546 líneas; `A4TextBlob` en 51 y 63) y
    `docs/automation/evidence/I-62-A4/a4-guards-result-c3.json` (1 836 líneas: cabecera 1-7; G2C3 610-911; G3 912-922; G4 923-931; G5 932-1731, con
    la nota «frente a 0d954376» en 1724; G5b 1732-1739).

Nada más. Los documentos que estos archivos citan y no están en la lista quedan fuera del cierre (también la orden O4 y el registro de la revisión 1).

## Reglas de lectura (el invocador audita después cada llamada)

Cada ruta se comprueba por su **efecto**: resuelta contra el directorio de trabajo, con `.` y `..` colapsados y los enlaces resueltos.

- **Read:**
  - usa la ruta absoluta, solo de archivos del cierre bajo `D:\r62-arch-a4r2` o de los siete archivos del run;
  - en los archivos de más de 60 000 bytes (el objeto, las salidas de las revisiones 1 y 2, V14, A-1, A-3, AUTOMATION_PLAN, el README de
    agent-execution, decisiones, evidencia, el README del kit de FX-02 y `a4-guards.py`), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** no la necesitas, porque las rutas están arriba. Si la usas, su alcance (`path` y patrón) solo puede contener archivos de la lista. Un
  patrón que alcance cualquier otro archivo cuenta como lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, el fixture
    (`D:\r62-fixture\*`), el resto de los directorios de las revisiones 1 y 2 (solo sus `output.json` están en el cierre) y el subdirectorio
    `launch\` del run;
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
  - de `order.txt`, `order-s62.txt`, `order-s61.txt`, `delta.diff`, `A-4.0d954376.md` o `A-4.7d863219.md`, con
    `Path` = `D:\r62-arch-a4r2-run\<nombre>`.
- `prompt.md` nunca es premisa.
- `Quote` es el texto literal de esas líneas; en `delta.diff`, con su marca inicial (`+`, `-` o espacio). El invocador comprueba que la cita está en
  esas líneas y que Read te las entregó fielmente.
- **Números de línea exactos.** `LineStart` y `LineEnd` son los números que Read muestra a la izquierda de las líneas citadas, y el texto de `Quote`
  tiene que estar dentro de ese rango. Una premisa con la línea desplazada, aunque sea en uno, no se acredita (la revisión 1 quedó NOT_ACCREDITED
  por eso). Antes de entregar, contrasta cada `PremiseRefs` con la salida de Read de la que sale.

## Qué decidir

Se pide el veredicto del paquete §1 (líneas 73-88) sobre la A-4 exacta. No se te prohíbe señalar otro defecto material que encuentres.

- **Hallazgos anteriores** (`PriorFindingDispositions`), sobre la versión exacta y cada uno una vez:
  - A62-A4-02 (REQUIRED de la revisión 2): `CLOSED` o `STILL_OPEN`, con su base en `PremiseRefs` (al menos una premisa);
  - A62-A4-01 (REQUIRED de la revisión 1, CLOSED en la revisión 2): confirma si sigue cerrado (`CLOSED`) o si la corrección 3 lo reabre
    (`STILL_OPEN`), con al menos una premisa;
  - A62-A4-O9 y A62-A4-O10 (OPTIONAL de la revisión 2): `APPLIED`, `NOT_APPLIED` o `N/A`, con su base;
  - en `LinkedFindingIds`, los hallazgos de esta re-revisión que sostienen un `STILL_OPEN` o un `NOT_APPLIED`, o que tratan un efecto lateral de la
    corrección.
- **`Focus`:** una disposición explícita de cada uno de estos ocho temas, una vez cada uno, como REQUIRED, OPTIONAL o NO_FINDING (sin hallazgo), con
  los `FindingIds` que la sostienen (ninguno con NO_FINDING) y la base. Cada tema contesta también las preguntas del paquete §4 (líneas 160-190) que le
  corresponden:
  - `OPTION_B_A4-4_ARCHITECT`: la opción B de A62-A4-02: la rama de materialización del Architect eliminada de A4-4, el binding del Architect
    aceptado solo por una decisión individual real del Coordinator fuera de la ventana, y la frase nueva de la regla 5 (paquete §4, pregunta 12 (A));
  - `LITERAL_IS18_B5_RAE143_F4`: si I-S18, B.5, RAE §14.3 y los validadores de F4 conservan su significado literal; en particular, la frase explícita
    sobre I-S18 de §3.7 y que ningún UNKNOWN pueda aparecer en una materialización (paquete §4, preguntas 9 y 12 (A));
  - `A4-6_BUDGET_DELTA`: el delta de presupuesto de A4-6: +1 sesión de Principal de la ronda A solo para A2, R contada (sin reclasificar ni
    reiniciar), justificación por F6-OBS-01, ningún otro tope cambiado, P-07, consumo del Owner `A4-PRINCIPAL-A2-CONSUMO`, separabilidad,
    materialidad M-03/M-04 y por qué ni M-02 ni M-05 (paquete §4, preguntas 6 y 12 (B));
  - `H1_H2_H3_STILL_SOUND`: si H-1, H-2 y H-3 (A4-3, A4-4 y A4-5) siguen resueltos tras el cambio (paquete §4, preguntas 8-10);
  - `A4-1_A4-2_UNCHANGED`: la ruta `cmd.exe` del Controller (A4-1) y el Architect por `claude-cli` (A4-2), sin cambio (paquete §4, preguntas 1-5);
  - `MATERIALITY_OVERALL`: la materialidad de A-4 en conjunto (§5; paquete §4, pregunta 6);
  - `SEPARABILITY_A4-5_A4-6`: la separabilidad de A4-5 y de A4-6 (paquete §4, preguntas 10 y 12 (B));
  - `FX-02_DISPOSITIONS_CONTEXT_ONLY`: si las disposiciones ordinarias de FX-02 (U-09 (e), U-16 (1), CD-19/OQ-24) aparecen en A-4 solo como contexto
    no normativo (paquete §4, pregunta 12 (C)).
- **`A45Verdict`** y **`A46Verdict`:** los veredictos separados de A4-5 y de A4-6 (paquete §1: el acuerdo puede excluirlas): AGREED, CHANGES
  REQUIRED, EXCLUDE (el acuerdo la excluye) o BLOCKED — OWNER DECISION, cada uno con los `FindingIds` que lo sostienen y la base.
- **`QuestionDispositions`:** Q-A4-01..Q-A4-22 de A-4 §8 (líneas 441-462), cada una una vez, como REQUIRED, OPTIONAL o NO_FINDING, con sus
  `FindingIds` (ninguno con NO_FINDING).
- **`DeltaConfirmation`:** si es cierto lo que el paquete §0 (líneas 68-71) declara igual a `0d954376`, comparando el objeto con `A-4.0d954376.md` y
  `delta.diff`: los párrafos de A4-1, A4-2, A4-3 y A4-5, byte a byte (`A41IdenticalTo0d954376`, `A42IdenticalTo0d954376`, `A43IdenticalTo0d954376`,
  `A45IdenticalTo0d954376`), y A4-4, introducción y reglas 2-4 (`A44IntroAndRules2to4IdenticalTo0d954376`): CONFIRMED, NOT_CONFIRMED o
  NOT_DETERMINED, con la base de cada respuesta.
- **`OwnerConsumptionLines`:** si las tres líneas del Owner de A-4 §7 (`A4-SONDA-CONSUMO`, `A4-CLAUDE-FX02-CONSUMO` y `A4-PRINCIPAL-A2-CONSUMO`) solo
  autorizan consumo o van más allá (aceptan resultados por adelantado, amplían OD-3, OD-5 o CLAUDE-CLI-I62, u otra autoridad del Owner)
  (`Assessment`: ONLY_CONSUMPTION, EXCEEDS_CONSUMPTION o NOT_DETERMINED), y si solo tienen efecto después del acuerdo
  (`EffectiveOnlyAfterAgreement`: YES, NO o NOT_DETERMINED), con la base.
- **Además**, según el paquete §1:
  - si la aplicación de A62-A4-O9 y O10 cambia el significado de alguna regla (en las disposiciones de los hallazgos anteriores y, si lo hace, como
    hallazgo);
  - si hace falta o no una decisión del Owner para el acuerdo (`OwnerAuthorityCheck`);
  - si A-4 cambia o no A-1, A-2, A-3, la fila «Codex, total» de D.3, cualquier otro tope o total de D.3 distinto de los dos que nombra A4-6 (la fila
    Principal A y el total de sesiones de Principal de la ronda A), P-01, OD-2, la receta 16.4, los descriptores, el texto de AUTOMATION_PLAN
    16.20-16.21, el texto de B1-B10 y producción (`NoChangeConfirmation`: NO_CHANGE, CHANGE o NOT_DETERMINED, con la base de cada respuesta);
  - tu modo de revisión, si revisor y autor son la misma persona y el contexto que el runtime te inyectó (`ReviewerMode`, `SamePersonAsAuthor`,
    `InjectedContextDeclaration`). Declara también el proveedor del revisor y del autor (paquete §5) en `SamePersonAsAuthor`. Los
    identificadores de cuenta que el runtime te inyecte (correo, uuid de organización u otros) se declaran solo por su tipo, sin reproducir su
    valor;
  - la identidad revisada: commit, ruta y blob.
- **`GuardsVerification`:** no puedes ejecutar las guardas. Lo que afirmes de G1, G2C3, G2D3, G3, G4, G5 y G5b sale de leer `a4-guards.py` y los
  resultados custodiados; lo que no puedas establecer así va como `NOT_VERIFIED`.

Un REQUIRED existe solo si el defecto es material según LIFECYCLE §5, y debe traer:
- un id estable;
- la sección exacta de A-4 (`AffectedDelta`);
- la premisa canónica completa en `PremiseRefs`;
- la autoridad o invariante violado (`ViolatedInvariant`) o un contraejemplo concreto (`Counterexample`), al menos uno de los dos no vacío;
- por qué importa;
- la corrección exacta.

Una precisión sin cambio de significado es OPTIONAL. **Ids nuevos:** REQUIRED `A62-A4-03`, `A62-A4-04`…; OPTIONAL `A62-A4-O11`, `A62-A4-O12`… Un id
anterior aparece en `RequiredFindings` u `OptionalFindings` solo para reformularlo: A62-A4-01 o A62-A4-02 si quedan `STILL_OPEN`; A62-A4-O9 o
A62-A4-O10 si quedan `NOT_APPLIED`.

## Resultado

Entrega el resultado con **una** llamada a StructuredOutput, con este objeto. Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-architect-review-result/v1 (representación experimental; rigen I-61 y LIFECYCLE)",
  "RunId": "R20261009T195811Z-2df6", "InvocationId": "I20261009T195811Z-2df6", "LogicalReviewRequestId": "L20261009T195811Z-2df6", "AttemptSeq": 1,
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
  "PriorFindingDispositions": [{"FindingId": "A62-A4-01 | A62-A4-02 | A62-A4-O9 | A62-A4-O10", "Disposition": "CLOSED | STILL_OPEN | APPLIED | NOT_APPLIED | N/A",
                                "LinkedFindingIds": [], "Rationale": "...", "PremiseRefs": []}],
  "RequiredFindings": [{"FindingId": "A62-A4-NN", "AffectedDelta": "...", "ViolatedInvariant": "...", "Counterexample": "...", "CorrectionRequired": "...",
                        "PremiseRefs": [{"Path": "...", "Section": "...", "LineStart": 1, "LineEnd": 1, "Quote": "proposición completa"}], "WhyItMatters": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A4-ON", "AffectedDelta": "...", "Note": "...", "PremiseRefs": []}],
  "Focus": [{"Topic": "OPTION_B_A4-4_ARCHITECT | LITERAL_IS18_B5_RAE143_F4 | A4-6_BUDGET_DELTA | H1_H2_H3_STILL_SOUND | A4-1_A4-2_UNCHANGED | MATERIALITY_OVERALL | SEPARABILITY_A4-5_A4-6 | FX-02_DISPOSITIONS_CONTEXT_ONLY",
             "Disposition": "REQUIRED | OPTIONAL | NO_FINDING", "FindingIds": [], "Rationale": "...", "PremiseRefs": []}],
  "A45Verdict": {"Verdict": "AGREED | CHANGES REQUIRED | EXCLUDE | BLOCKED — OWNER DECISION | NOT_ASSESSED", "FindingIds": [], "Rationale": "..."},
  "A46Verdict": {"Verdict": "AGREED | CHANGES REQUIRED | EXCLUDE | BLOCKED — OWNER DECISION | NOT_ASSESSED", "FindingIds": [], "Rationale": "..."},
  "QuestionDispositions": [{"Id": "Q-A4-01 | … | Q-A4-22", "Disposition": "REQUIRED | OPTIONAL | NO_FINDING", "FindingIds": [], "Rationale": "...", "PremiseRefs": []}],
  "DeltaConfirmation": {"A41IdenticalTo0d954376": {"Assessment": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "Note": "..."},
                        "A42IdenticalTo0d954376": {"Assessment": "...", "Note": "..."}, "A43IdenticalTo0d954376": {"Assessment": "...", "Note": "..."},
                        "A45IdenticalTo0d954376": {"Assessment": "...", "Note": "..."}, "A44IntroAndRules2to4IdenticalTo0d954376": {"Assessment": "...", "Note": "..."}},
  "NoChangeConfirmation": {"A1": {"Assessment": "NO_CHANGE | CHANGE | NOT_DETERMINED", "Note": "..."}, "A2": {"Assessment": "...", "Note": "..."},
                           "A3": {"Assessment": "...", "Note": "..."}, "D3CodexTotalRow": {"Assessment": "...", "Note": "..."},
                           "D3OtherCapsBeyondA46": {"Assessment": "...", "Note": "..."}, "P01": {"Assessment": "...", "Note": "..."},
                           "OD2": {"Assessment": "...", "Note": "..."}, "Recipe164": {"Assessment": "...", "Note": "..."}, "Descriptors": {"Assessment": "...", "Note": "..."},
                           "AutomationPlan1620to1621Text": {"Assessment": "...", "Note": "..."}, "B1toB10Text": {"Assessment": "...", "Note": "..."},
                           "Production": {"Assessment": "...", "Note": "..."}},
  "OwnerConsumptionLines": {"Assessment": "ONLY_CONSUMPTION | EXCEEDS_CONSUMPTION | NOT_DETERMINED", "EffectiveOnlyAfterAgreement": "YES | NO | NOT_DETERMINED", "Note": "..."},
  "GuardsVerification": {"Notes": [{"Guard": "G1 | G2C3 | G2D3 | G3 | G4 | G5 | G5b", "ClaimHolds": "HOLDS | DOES_NOT_HOLD | NOT_VERIFIED", "Note": "..."}], "Note": "..."},
  "Materiality": {"Overall": {"M01": "YES | NO", "M02": "...", "M03": "...", "M04": "...", "M05": "...", "M06": "...", "M07": "...", "M08": "..."}, "Note": "tu evaluación"},
  "OwnerAuthorityCheck": {"Spending": "NONE | ...", "Credentials": "NONE | ...", "Destructive": "NONE | ...", "NewOVScenario": "NONE | ...",
                          "OVRemovalOrReassignment": "NONE | ...", "OwnerPolicyChoice": "NONE | ...", "NewOwnerAuthority": "NONE | ...", "OwnerDecisionRequired": "true | false"},
  "IfAgreed": {"A41Agreed": null, "A42Agreed": null, "A43Agreed": null, "A44Agreed": null, "PriorRequiredClosed": null, "NoOwnerDecisionRequired": null,
               "NoChangeToProtectedSurfaces": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: `IdentityCheck`, `PriorFindingDispositions`, `Focus`, `A45Verdict`, `A46Verdict`,
  `DeltaConfirmation`, `Materiality`, `OwnerAuthorityCheck`, `OwnerConsumptionLines`, `NoChangeConfirmation` y `GuardsVerification` llevan **tu**
  comprobación y **tu** evaluación. Donde la plantilla dice `"true | false"` o `"true | false | null"`, el valor es un booleano JSON (o `null`), no
  un texto.
- `IdentityCheck`: las seis booleanas son lo que compruebas en el Paso 0; `null` en lo que no compruebes.
- Cobertura, una vez cada elemento:
  - `PriorFindingDispositions`: A62-A4-01, A62-A4-02, A62-A4-O9 y A62-A4-O10. A62-A4-01 y A62-A4-02 van en `CLOSED` o `STILL_OPEN`, con al menos
    una premisa cada uno; A62-A4-O9 y A62-A4-O10, en `APPLIED`, `NOT_APPLIED` o `N/A`. Un `STILL_OPEN` o un `NOT_APPLIED` cita en `LinkedFindingIds`
    al menos un hallazgo de esta re-revisión (para un REQUIRED anterior `STILL_OPEN`, un REQUIRED); todo id de `LinkedFindingIds` existe en
    `RequiredFindings` u `OptionalFindings`;
  - `Focus`: los ocho temas;
  - `QuestionDispositions`: Q-A4-01..Q-A4-22.
  En `Focus` y `QuestionDispositions`, una disposición REQUIRED u OPTIONAL cita en `FindingIds` hallazgos de `RequiredFindings` u `OptionalFindings`,
  respectivamente.
- **`A45Verdict` y `A46Verdict`:** con CHANGES REQUIRED citan en `FindingIds` al menos un REQUIRED; con BLOCKED — OWNER DECISION,
  `OwnerDecisionRequired` = true; todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`.
- **Con AGREED** (A-4 entera, o sin A4-5 o sin A4-6 si su veredicto es EXCLUDE):
  - cero REQUIRED (también en `Focus` y en `QuestionDispositions`), y A62-A4-01 y A62-A4-02 `CLOSED`;
  - `A45Verdict` y `A46Verdict` en AGREED o EXCLUDE;
  - `OwnerDecisionRequired` = false, `OwnerConsumptionLines` en ONLY_CONSUMPTION y `EffectiveOnlyAfterAgreement` en YES;
  - los doce apartados de `NoChangeConfirmation` en NO_CHANGE y los cinco de `DeltaConfirmation` en CONFIRMED;
  - los ocho campos de `IfAgreed` en `true`.
  En cualquier otro caso, `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true.
- **Con un veredicto** (todo salvo NOT_ACCREDITED): `Materiality.Overall` en YES o NO, `A45Verdict` y `A46Verdict` distintos de NOT_ASSESSED,
  `DeltaConfirmation` en CONFIRMED o NOT_CONFIRMED, y ni las booleanas de `IdentityCheck` ni `OwnerDecisionRequired` en `null`.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `PriorFindingDispositions`, `Focus` y `QuestionDispositions` pueden ir vacíos.
  - Lo que no hayas evaluado va así: `Materiality.Overall` en `NOT_ASSESSED`; `A45Verdict` y `A46Verdict` en `NOT_ASSESSED`; en `null` las
    booleanas de `IdentityCheck` que no comprobaste y `OwnerDecisionRequired`; `ReviewedCommit` y `ReviewedBlob` con lo observado o en `null`;
    `NoChangeConfirmation`, `OwnerConsumptionLines` (los dos campos) y `DeltaConfirmation` en `NOT_DETERMINED`.
  - `NOT_ASSESSED` y `null` en esos campos solo valen con NOT_ACCREDITED.