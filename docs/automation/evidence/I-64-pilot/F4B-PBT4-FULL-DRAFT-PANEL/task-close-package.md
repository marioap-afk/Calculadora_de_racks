# Paquete de cierre — F4B-PBT4-FULL-DRAFT-PANEL (PB-T4b)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `c065080d3fd1dd8bb1937b98b81e9a0c1714777b`, con attempts **1 de 3** (una reparación del RED, que no gasta intento).

El borrador del panel del Workspace de Push Back pasa al modelo completo de PB-T3, sobre la lectura completa de PB-T4a y sin traducir estados.
- **El anfitrión** (WHC) abre el borrador completo (`PushBackFullDraftState`) con el diagnóstico de la admisión y el testigo. «Actualizar» va por `PrepareFull` y `ExecuteFull`.
- **«Conservar mi borrador», en dos «Sí»** (D-S2):
  - el primero presenta «Por grupos» y devuelve el testigo (`EditIntentOutcome.Presented`);
  - el segundo, con el testigo vigente, hace `KeepOn`.
- **La regla de la escala, única y en Application** (`NumericTextRule`): `NumericFieldValidation.Validate` delega en ella sin cambiar su API ni su comportamiento, y `PushBackAnnotationScaleRule` la usa.
- **El adaptador de la lectura** (RIA) materializa el modelo completo una sola vez por lectura (D-S4: el `Lazy` del `Primary`).
- **Las 478 pruebas de la lista** quedan saltadas con su motivo literal, cada una sustituida por su copia o su equivalente: 21 archivos de copia y el kit, iguales a los de la precondición 6.
- **El código del piloto queda sin uso;** lo retira PB-T4d.

**Autoridades:**
- **El contrato:** `F4B-PBT4-FULL-DRAFT-PANEL/coordinator/gate-contract.json`, con:
  - el paquete de PB-T4 v4.9;
  - AuthorityRevision `f5ff00ee`;
  - BaseSha = ChainBaseSha `8f783df5`.
- **Las decisiones del Owner:**
  - F4B-E49 y las tandas sobre PB-T4 (F4B-E52);
  - en este cierre, F4B-E53, cinco decisiones: tres sobre el código (la errata de CT-20 (5), los vacíos de M-04 a PB-T4d y el nombre tras «Conservar») y dos sobre los controles (nc1 y nc3, aceptados solo esta vez);
  - F4B-E35 (RR), F4B-E37 (Snapshot K), F4B-E40 (PD-F4B-01), F4B-E43 (el sustituto de nc2) y F4B-E46 (el total exacto de cada rama).
- **Las decisiones de la sesión:** D-S1 a D-S6 (`F4B-DRAFTS/pbt4/session-pbt4b-design-decisions.md`), y el TIMEOUT de la primera verificación (`F4B-DRAFTS/pbt4/session-verify-timeout-decision.md`).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `f5ff00ee` y BASE `8f783df5` | F4B-E52, con la v4.9. El esqueleto lo da el generador custodiado (`green_pbt4b_v45.py`), registrado en la precondición 6. La partición es `e87e024c`, con R_C = 291 y R_UI = 0 |
| Planificación | `R20261121T084940Z-a2b4` | Aceptada al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 15 criterios. nc4 `R20261121T085348Z-e944`: solo A3 en fail. La lectura RR-3 quedó diferida, con falla cerrada |
| Worker | `R20261121T085458Z-5ef5` | IMPLEMENTATION_COMPLETE (claude-sonnet-5-5/high). RED `c1e4f458` (55 rutas), reparación del RED `9b2d1893` y GREEN `c065080d` (nueve archivos de código). Una desviación declarada: CT-20 (5) |
| CI del RED | `44893322578` | tests y ui-tests en failure; build-ui en success; build-plugin omitido. Core 19 605 = 18 841 + 306 Failed + 458 Skipped; UI 2 481 = 2 430 + 3 + 48 |
| CI de la reparación | `44899232904` | Los mismos 306 + 3 fallos, con la misma forma |
| CI del GREEN | `44903818806` | 4/4. Core 19 605 = 19 147 + 0 + 458; UI 2 481 = 2 433 + 0 + 48. Los totales exactos de su rama (F4B-E46) |
| La reparación del RED | `c1e4f458..9b2d1893` | Solo `PushBackFullPanelTests.cs`: CT-PBT4B-08 sin `Name` en «el resto del sobre del rack actual». `rr_check` PASS: RR-1, RR-2 y RR-6, n.º 10 y RR-3 (a), 45 literales sin cambio |
| Revisión directa | — | Conforme, con la desviación de CT-20 (5), que el Owner ratificó |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/1/8,** condicionada a I-01, más OWNER-01 a OWNER-04 |
| Decisiones del Owner | — | I-01: «Ratificar como errata». Los vacíos: «Llevarlas a PB-T4d». El nombre: «Es lo esperado» |
| Verificación 1 | `R20261121T132429Z-db16` | **No productiva, TIMEOUT:** 600 s, 50 órdenes y exit 124, sin `output.json`. El tope de los lanzadores pasa a 1800 s |
| Verificación 2 | `R20261121T133638Z-1b82` | **EXECUTION_VERIFIED** / NONE, 14/14, con la disposición de CT-20 como entrada. Un hallazgo: la disposición solo ratifica la errata |
| nc1 | `R20261121T134602Z-5688` | CONTROL NOT PASSED: la cascada del SHA falso en Handoff, Identity, Remote, Scope, Ci, Tests y Trailer (S-04 y PBT4B-C18). Su sustituto `d3ca` dio FAIL en `d1_FailSetWithinAllowed` y `e2_OtherFailCitesFakeSha`, porque entra Trailer y no cita el SHA falso. Es la sexta variación en F4B. El Owner: «Aceptarlo solo esta vez», así que nc1 = SATISFIED_BY_OWNER_DISPOSITION |
| nc2 | `R20261121T135034Z-464a` | **CONTROL PASS** (Scope, PBT4B-C01) |
| nc3 | `R20261121T135520Z-2bac` | CONTROL NOT PASSED. FreeText detecta la frase, pero antes difiere Tests (PBT4B-C04): lee 171 de los 306 fallos del RED como excepciones. Con INV-PBT4B-02 son los 306 por aserción. El Owner: «Aceptarlo solo esta vez», así que nc3 = SATISFIED_BY_OWNER_DISPOSITION |

**Relevos:**
- 0 participantes persistentes en toda la cadena;
- la Snapshot K y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Límites, hallazgos y arrastres

| Id | Qué | Va a |
|---|---|---|
| I-01, CT-20 (5) | La comprobación (5) acotada a las nueve clases de C-0. Tal cual daría `False` por cinco apariciones en literales de cadena de dos copias de guarda custodiadas | Ratificada como errata del paquete (Owner), solo este caso |
| OWNER-01, el nombre | Con «Conservar mi borrador» y después «Actualizar», el rack se queda con el nombre del borrador aunque se renombrara fuera del panel. La presentación lo avisa como «Cambiado fuera: el nombre.», pero no dice qué valor gana | Es lo esperado (Owner; D-PB-05, paso 4). Smoke-PB, y la mejora del texto, en `docs/ideas-futuras.md` |
| OWNER-03, los vacíos de M-04 | Dos guardas de PEG pasan en vacío en el GREEN, y su copia hereda uno: `WorkspacePilotEditGuardTests.TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark` (:592-597), `WorkspacePilotEditGuardTests.TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing` (:906-909 y :917-920) y `WorkspacePilotEditGuardFull.cs:255-258` | La lista de PB-T4d, por decisión del Owner; esa lista le vuelve antes de emitirse |
| OWNER-04, el segundo «Sí» | Ninguna CT ejecuta RIA ni NH evaluando el testigo tras `AtOrigin`. Si en AutoCAD el testigo no volviera `Unchanged`, «Conservar» presentaría en bucle | Smoke-PB |
| El coste real de la lectura | Una sola materialización por lectura (D-S4). Su coste se midió sin AutoCAD en la precondición 6, y la séptima tanda del Owner lo dio por cumplido. «Conservar» lee ahora el dibujo dos veces | Smoke-PB |
| RR-3 | La lectura de la cadena deja pendientes tres clases (CT-20, `PushBackFullPanelGuardTests` y `PushBackFullPanelFieldTests`): sus filas remiten a literales que el paquete no enumera | `docs/ideas-futuras.md`. La cadena para si una reparación las toca |
| M-01 a M-08 | Las debilidades de discriminación de CT-08, CT-18 y CT-20; los vacíos; la nota de I-01; la etiqueta «PorAsercion»; el texto «> 0» | M-04, a PB-T4d. M-07, a la cadena de PB-T4c. M-01, M-02, M-03, M-07 y M-08, a `docs/ideas-futuras.md`. M-05 y M-06, de constancia |
| La primera verificación | TIMEOUT a los 600 s por el tamaño de la entrega | El tope de los lanzadores pasa a 1800 s; el relevo acepta un TIMEOUT |
| nc1, la sexta variación en F4B | La cascada del SHA falso llega también a Trailer, con la misma causa dicha con otras palabras, y el sustituto de PD-F4B-01 no la admite | Por disposición del Owner, solo esta vez. El sustituto es candidato a revisarse, sin cambiar nada ahora (`docs/ideas-futuras.md`) |
| nc3 y la etiqueta «PorAsercion» | El control leyó como excepciones 171 de los 306 fallos del RED; la etiqueta del relevo del trabajo solo cuenta los `Assert` (M-07) | Por disposición del Owner, solo esta vez. La etiqueta se corrige en la cadena de PB-T4c |
| Sin AutoCAD | OB-03.2 en el dibujo real, «Actualizar» y «Conservar» reales, y el borrado real de vistas | El Smoke-PB de PB-T5 |
| El registro declarado de las 478 | Completado en este cierre (cinco líneas) | Hecho |

**Siguiente:** el paquete de PB-T4c, por la emisión condicional de F4B-E49 (pregunta 4):
- la comprobación acotada en 0/0;
- la remedición sobre este cierre, sin cambio material;
- la lista que vio el Owner.

PB-T4d vuelve al Owner con su lista antes de emitirse.
