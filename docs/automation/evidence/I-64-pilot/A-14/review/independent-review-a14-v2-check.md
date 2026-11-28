# Comprobación acotada de I-64 A-14, borrador v2

## 0. Identidad, modo y método

- **Objeto:** `S/I-64-A-14-draft-v2.md`, SHA-256 `65e252e2e24db490dbb5b5261f8dc48b6f428647cea9ebe16ad02cb33b1492e4` (859 líneas). Se cita «v2:n».
- **Tabla de disposición:** `S/a14-v2-disposition.md`, SHA-256 `eb8bd6f6…7ecd`. **Notas:** `S/a14-drafting-notes.md` §10, SHA-256 `1a0a92d4…627e`.
- **Revisión del v1:** `S/independent-review-a14-v1.md`, SHA-256 `d3a13ef0…22b4`, con 29 hallazgos: R2-01..R2-04, I-01..I-11 y M-01..M-14.
- **Decisiones del Owner:** `S/owner-a14-decisions.md`, SHA-256 `f1d0ea5d…4c2e`, con las tres rondas y RACKPANEL. **Replaneación:**
  `S/replaneacion-i64.html`, SHA-256 `79175286…8a4f`, la misma que leyó la revisión del v1.
- **Freeze:** `W/docs/initiatives/I-64-proposal-v5.md`, blob `dc1924ff5a73a4e529fbc5f995621d29351a091b`. Comprobado en el HEAD de W (`fa59bd82`, árbol
  limpio) y en `55e7ca63`. Se cita «V5:n».
- **Fuentes leídas en W, solo lectura:**
  - V5: D-01 a D-23, §10 a §14 y los anexos, enteros;
  - LIFECYCLE §2 a §9, enteros;
  - WORKFLOW §2, §10 `:398`, §11 y §11.6, y las referencias a §4.5;
  - ICL: IC-02, IC-03, IC-05, IC-08, IC-09 e IC-11;
  - OD `:18-80`, `:230-299`;
  - PKG `:59`, `:60`, `:86`, `:171`, `:174` y `:200`;
  - PB-T5 v2.4 `:12-18`, `:241`, `:260-261` y `:276`;
  - el brief en todas las líneas que cita la v2;
  - `superseding-characterization.md` (cabecera y SUS-01/02);
  - ADR-0029 D13, ADR-0047 `:1-45` y `docs/adr/README.md`;
  - las cabeceras de A-1..A-13 y de las A-n de I-63 (campo `Applies-to`);
  - ROADMAP (I-52-AUTH15, I-53, I-53S, I-53D) e `I-64.yml` (estados, SM-03, PASTECLIP, F1-T3C);
  - en `src/`: `RackMenuCommands.cs`, `RackMainMenuWindow.xaml`, `RackSiblingRedrawRun.cs`, `UserSettings.cs` y el censo completo de `CommandMethod`.
- **Modo:** SEPARATE SESSION. Subagente revisor, distinto del redactor. La comprobación anterior de la v2 se interrumpió sin dejar
  resultado; esta se hizo desde cero.
- **Reglas:** W intacto: ni escrituras ni órdenes git que cambien estado. Sin AutoCAD, sin compilar y sin pruebas. La única escritura es este archivo.
- **Uso de la disposición:** la tabla y las notas se leyeron solo para ubicar lo que afirman. Cada estado de §2 se decidió en el texto
  de la v2 y en la fuente.

## 1. Veredicto

**EMITIBLE TRAS CORRECCIONES.**

| Hallazgos del v1 (29) | Resueltos | Parciales | Sin resolver |
|---|---|---|---|
| | 28 | 1 (R2-02) | 0 |

| Hallazgos nuevos de la v2 | Bloqueantes | Importantes | Menores |
|---|---|---|---|
| | 0 | 5 | 14 |

La v2 resuelve casi todo el v1, y bien:
- RACKEDITAR (C-09);
- la composición de «Actualizar todo» sobre la costura de I-55 (C-05);
- la preview de navegación (C-02);
- la privacidad del índice (C-11);
- las tablas completas de cláusulas, invariantes y OV (§4 y §5);
- el orden de versionado (§10).

Todas las citas que comprobé son exactas (§7). No hay rutas locales, nombres, correos ni datos de clientes.

Los cinco importantes salen del mecanismo nuevo de tramos (C-12) y de un delta impreciso sobre un texto literal del Owner:
- `Applies-to`: las unidades de los tramos 2 a 5 no aplicarían A-14 ni A-1..A-13;
- el corte de tramos propuesto ata la fase 6 a la fase 5;
- los tramos 3 y 4 llegan a `main` casi sin Owner Validation de lo que entregan;
- la sustitución «entera» de D-PB-01;
- Q-C3 y Q-C4 quedan abiertos sin un artefacto que los cierre.

Hay que corregirlos **antes del paso 3 de §10**. Desde ese commit A-14 es append-only, y cualquier REQUIRED del Architect costaría
una A-15.

## 2. Los 29 hallazgos del v1, comprobados en el texto de la v2

| Id | Estado | Dónde en la v2 | Comprobación y residuo |
|---|---|---|---|
| R2-01 | **RESUELTO** | C-09 (v2:386-422); C-08 (v2:377-378); C-06 (v2:318-319); C-01 (v2:137); §4 D-15, D-16, §14, anexo A dec. 10; §5 INV-19, INV-A14-17, OV-17 | Se enmiendan de forma explícita D-16 `:586-587`, el STOP `:593`, D-15 `:570-572`, INV-19, M-03 `:996`, OV-17 (en la lista del Owner) y el brief `:393-394`. La caracterización vieja se conserva y se marca `Skip` con registro, como en el patrón de `superseding-characterization.md` (comprobado: «Skip con un motivo que nombra este registro»; pruebas nuevas aparte). Los comandos de creación entran porque la ventana es la misma (PKG `:171`, comprobado). R-07 se actualiza. El precedente de [OD] `:237-243` se cita literal y es exacto |
| R2-02 | **PARCIAL** | C-12 (v2:494-542); §5 (v2:669-676); §6; §8 | **Resuelto:** la regla «tramo = unidad de entrega» (LIFECYCLE §2), READY-01..09, `FINAL_CANDIDATE_SHA`, la conformidad, OV por tramo, ID3 en el último tramo, F4B sin cambio, la limpieza tras §4.5.6 y §4.5.7, y los precedentes (comprobados en ROADMAP `:471-475`). **Residuo:** <br>(1) `Applies-to`, que la revisión del v1 pedía expresamente (LIFECYCLE §6 `:198`), queda mal resuelto: N-01; <br>(2) para Smoke-PERF por tramo, la revisión pedía «una regla explícita»; la v2 da una propuesta y la deja abierta, como también ADR-0047, sin artefacto que los cierre: N-05; <br>(3) la asignación de OV no cubre lo que entregan los tramos 3 y 4: N-03; <br>(4) falta el bootstrap y el Discovery delta de cada unidad: N-06 |
| R2-03 | **RESUELTO** | C-07 (v2:339-348); INV-A14-01, 02; R-13; fase 9 | RK es el mismo método (`RackMenuCommands.cs:21`, comprobado). La entrada de respaldo está en INV-A14-02 y en la lista de retiro |
| R2-04 | **RESUELTO** | §2 dec. 9; C-10 (v2:450); C-11; §8 | Fuera de A-14; ya no es pregunta |
| I-01 | **RESUELTO** | Cabecera (v2:16-24); §4 (v2:555-602); §8 (v2:754) | D-04, D-05, D-07, D-10, D-14, D-18, D-21, §0, §2 y §14 pasan a «Amends» con su delta, y también los anexos A (decisiones 1, 6, 8, 10, 11 y las consecuencias) y B (las dos notas). Las líneas de ADR-0047 están comprobadas (`:18`, `:27`, `:30`, `:33`, `:34`). «Sin cambio» queda en D-01 (host, contingencia, foco y perfil), D-11, D-12, D-19 y §12.1, y es correcto. Residuo menor: los no-objetivos `:79` y `:80` no reciben texto literal nuevo (N-15) |
| I-02 | **RESUELTO** | §5 (v2:606-632, 656-667); C-03 (v2:210-216); §4 §12.5 | Se añaden INV-02 (con RACKCAD y RK repetidos), 05, 07, 08, 15, 18, 20, 25, 37, 38 y 39. OV-11 y OV-20 pasan a la lista del Owner. SP-02 se enmienda. Las obligaciones de IC-02 e IC-05 se sustituyen (ICL `:53`, `:89`, comprobado). Los resultados de F3 se sustituyen con registro, sin reabrir F3. Residuo de precisión, en N-07: la lista de lo que la fase 2 retira de F3 no nombra las pruebas de pestañas de INV-15, INV-09, INV-38 ni INV-02, ni el escenario 5 |
| I-03 | **RESUELTO** | C-05 (v2:253-265, 283-284, 292-296); INV-39; INV-A14-11, 12; R-14 | Composición nueva y costura intacta (comprobado: `Execute` recibe la pertenencia de un rack, `:30-32`). INV-39 se extiende. Arnés con N ≥ 70; la captura compartida queda sin congelar. M-08 y STOP frente al Freeze de I-55. Residuo menor: incoherencia de fases de INV-A14-13..15 (N-09) |
| I-04 | **RESUELTO** | C-05, condiciones 3 y 4 (v2:266-274); INV-A14-15 | La regla sale de V5 `:436-437`, `:487-493`, `:531-532` y `:396`, todas comprobadas |
| I-05 | **RESUELTO** | C-02 (v2:173-178); §4 D-10; INV-A14-07, 08 | La pestaña preview de D-08 desaparece y la preview de navegación de D-10 se conserva (INV-27). La vida del borrador Limpio queda fijada. Residuo de redacción, en N-12 |
| I-06 | **RESUELTO** | C-02 (v2:180-185); INV-A14-06; anexo A dec. 6 | «Encendido» queda congelado con la fuente de V5 (D-06 `:335`, criterio 3, OV-04) |
| I-07 | **RESUELTO** | C-11 (v2:457-492); C-10, excepciones 2 a 4; C-01 (v2:135-136); §6 fase 5; R-08 | La fase 5 no escribe el DWG. Variables va con borrador, «Actualizar todo» y su A-n. Biblioteca lanza el flujo clásico. D-22 recibe excepciones nombradas y el brief `:444-457` se declara cambiado. BOM se apoya en D-05 `:312-313` (consumidor o superficie), un apoyo suficiente. Residuos menores: N-10 (ruta de la biblioteca de bloques en la fase 1), N-11 (criterio nuevo de la fase 5), N-13 (R-08 llama «ligera» a la fase 2) |
| I-08 | **RESUELTO** | C-11 (v2:484-485); INV-A14-10; §4 D-05 | Invariante nuevo con su RED. Se citan D-05 `:312-313` y MASTER `:260` |
| I-09 | **RESUELTO** (ajuste correcto en sustancia) | C-10 (v2:435-443); INV-A14-03; C-01 (v2:133) | S1-11 se repite con F1-T3C (existe: `I-64.yml`, «F1-T3C-PROFILE-DOC = COMPLETE») y el acuse de I-52. Se citan el brief `:474` y la frase «no altera los demás ajustes» (`UserSettings.cs:13-27`, comprobado). **El ajuste:** incluir «acoplada o flotante y el lado» en «posición» es una lectura razonable de «la ventana recuerda», y dejar sin congelar qué valores se guardan es coherente. Pero el texto de la v2 no la marca como lectura de la sesión, como sí hizo M-12 con las fases: N-08 |
| I-10 | **RESUELTO** | C-01 (v2:122-125) | Censo de `CommandMethod` comprobado. Todos los comandos con nombre largo quedan cubiertos: por la lista, por las secciones o por C-07 |
| I-11 | **RESUELTO** | §10 (v2:795-806); pie | El orden queda: revisión, v2, versionado PROPOSED con las fuentes, Architect sobre ese commit, A-15 si pide cambios, Coordinator, Owner y activación. Coincide con el precedente: los veredictos fuera del archivo y la cabecera PENDING (A-13, un solo commit). Residuo menor: no coincide lo que se versiona en el paso 3 (N-14) |
| M-01 | RESUELTO | §6 (v2:716) | OD `:269` y `:293`, comprobados literales |
| M-02 | RESUELTO | C-05 (v2:279) | «Descartar y recargar» (V5 `:488`) |
| M-03 | RESUELTO | §1 (v2:77-80) | ICL `:3`, comprobado. Crea una tensión nueva, menor (N-16) |
| M-04 | RESUELTO | Cabecera (v2:29-31); C-08 | Sin contradicción interna. El alcance de la sustitución es impreciso: N-04 |
| M-05 | RESUELTO (en la forma) | §11 (v2:808-822) | Existe el texto exacto de la firma. Su comprensibilidad y su completitud, en §6 |
| M-06 | RESUELTO | C-08 (v2:370) | «Deja de medir 320 px» |
| M-07 | RESUELTO | C-02 (v2:182-184) | |
| M-08 | RESUELTO | C-05 (v2:275-276); INV-A14-11 | |
| M-09 | RESUELTO | C-05 (v2:281) | «en esa sesión ni en otra» es coherente con V5 `:545`, que no lo limita |
| M-10 | RESUELTO | §5 (v2:667) | |
| M-11 | RESUELTO (ajuste correcto) | §4 §12.5 (v2:589); OV-A14-03; R-03 | Aplicarlo también a OV-A14-03 y a R-03 es correcto: decían lo mismo. El delta de SP-03 debería decir que conserva «incluidos cambios frecuentes de referencias (R-10)» de V5 `:909` (dentro de N-07) |
| M-12 | RESUELTO | §2, dec. 1 (v2:89) | |
| M-13 | RESUELTO | C-11 (v2:465); §4 D-23; §9 M-06 | |
| M-14 | RESUELTO (ajuste correcto) | R-17 (v2:742); §10, paso 3 | Comprobado: PB-T5 v2.4 `:17` es el precedente (`s2-selection.local.json`, «No se custodia»); `:14` habla del guion de emisión. El ajuste de la cita es correcto |

## 3. Las decisiones del Owner: ¿fielmente recogidas?

| Decisión | En la v2 | Juicio |
|---|---|---|
| Ronda 1 (§3, recuerda, RACKCAD, lista después) | §2, dec. 1 a 4; C-01, C-07, C-10 | Fiel. La aprobación de §4 se marca como lectura (M-12) |
| RACKPANEL «puede quedarse por ahora» | §2, dec. 5; C-07 | Fiel |
| **Q-03** RACKEDITAR | C-09 | **Fiel, con una deducción.** Se recogen «la misma pantalla rediseñada, pero en una ventana aparte», «un solo editor por sistema» y «el cambio se declara cuando ocurra». Extenderlo a los **comandos de creación** no está en la respuesta del Owner: lo deduce la sesión porque la ventana es la misma (PKG `:171`). Es correcto, y la firma lo nombra. Pero C-09 y §2, dec. 6 no lo marcan como lectura de la sesión (N-17). También es construcción del redactor que la aprobación del prototipo valga como la decisión del Owner que exige ADR-0029 D13 (v2:409): N-17 |
| **Q-09** Por tramos | C-12; §5; §6 | **Fiel en la regla. Recorta en la práctica** «Cada hito lleva su candidato completo y tu validación». Con la asignación propuesta, la validación del Owner en los tramos 3 y 4 casi se reduce a RACKEDITAR (OV-17, OV-A14-09) y al checklist: N-03. El ejemplo del Owner («tras la ventana con los racks y Push Back») es el tramo 1, y es exacto. Los demás tramos son propuesta, y el Owner los confirma al firmar: correcto. Que cada tramo sea una unidad nueva (rama, reclamo, worktree) es mecánica de proceso, no ampliación de la decisión |
| **Q-07** RK y menú de hoy | C-07; INV-A14-01, 02; fase 9 | Fiel: RK igual, entrada de respaldo hasta la fase 9, ningún comando nuevo. La lista de retiro «incluye la entrada de respaldo», que se deduce de «hasta la fase 9» |
| Q-08 | §2, dec. 9; C-10; C-11; §8 | Fiel |
| **Q-02** Primero racks | C-05 (v2:285-286); C-11 (v2:475-483) | Fiel. Añade «con su propia A-n». No es ampliación: el borrador de variables cambia D-10, D-11, D-12 y D-17, y eso exige una A-n (LIFECYCLE §6). Lo de «solo racks» en la fase 4 es exacto |
| **Q-10** «Abrir en editor clásico» en la fase 2 | C-02 (v2:187-189); INV-A14-09; fase 2 | Fiel, con la limitación declarada («sigue bloqueando hasta que se migre»). IC-03 queda sustituida, y viene de la decisión C-F3-04 del Owner (ICL `:59`, comprobado). La firma no lo nombra (§6) |
| Decisiones previas del mismo día | §2 (v2:101-105) | Fieles. La cita literal de «se eliminaran muchos de los comandos…» es exacta |
| Replaneación §3 y §4 | C-01..C-11; §6 | Hay dos recortes menores frente al texto aprobado: <br>(a) §4, fase 4: «progreso y lista de lo que falló». El progreso solo aparece como tratamiento de riesgo (R-04), no como resultado verificable (N-18); <br>(b) §3, D-08: «los borradores siguen vivos al cambiar». La v2 libera el borrador Limpio al dejar el rack. Es inocuo, porque no tiene cambios, pero conviene decir «los borradores con cambios» (N-12) |

## 4. Las decisiones nuevas del redactor (notas §10.3) y la autoridad que corresponde

| # | Decisión | Fuente | ¿Material nueva? | Autoridad |
|---|---|---|---|---|
| 1 | Cinco tramos, como PROPUESTA | Q-09: el tramo 1 es el ejemplo del Owner; los demás, no | Sí: los tramos 2 a 5 | **Owner** (confirma en la firma). El Coordinator, la viabilidad. Ojo: el corte 2 = {4, 5} ata la fase 6 a la 5 (N-02) |
| 2 | Tramo 1 = I-64; los siguientes, unidades nuevas | LIFECYCLE §2; WORKFLOW §2, §3 y §4.5; precedentes I-53, I-53S e I-53D | No (proceso) | **Coordinator**. Defectuoso en `Applies-to` (N-01) y en el Discovery delta (N-06) |
| 3 | Regla de lectura de «pestaña» con una lista cerrada | LIFECYCLE §6 `:203` | No, si es editorial | **Coordinator + Architect**: el Architect comprueba que ninguna lectura cambia el significado. Tiene deltas dobles y la lista no es cerrada (N-07) |
| 4 | El borrador Limpio se libera al dejar el rack | D-10 `:440` («cerrar una pestaña limpia»); revisión del v1, I-05 | Sí, cambia D-10 (semántica de vida del borrador) | **Architect + Coordinator** |
| 5 | Elegibilidad para «Actualizar todo» (texto conservado, Conflicto, Desconocido y Error la impiden; «posiblemente obsoleto» entra) | D-10, D-12, D-13 y D-08; «todo o nada» del Owner | Sí (M-04), pero derivada | **Architect + Coordinator**. Conviene que la firma lo diga en una línea, porque el usuario lo verá |
| 6 | «Posición» incluye acoplada o flotante y el lado | Lectura de la decisión 2 | Lectura de una decisión del Owner | **Owner**, que la confirma: marcarla como lectura y nombrarla en la firma (N-08) |
| 7 | Cuatro excepciones nombradas de D-22 | Decisión 2; revisión del v1, I-07; la 4.ª (diseños guardados) es nueva | Sí (M-02) | **Owner + Architect + Coordinator**. Ya está en la firma («las excepciones de D-22») |
| 8 | Variables con su propia A-n | LIFECYCLE §6; Q-02 | No (proceso) | **Coordinator**. La A-n posterior: Owner + Architect + Coordinator |
| 9 | La fase 6 depende también de la 4 | C-05; el orden 0 → … → 4 → 6 de la replaneación | No (dependencia) | **Coordinator** (LIFECYCLE §6). Con los tramos tiene una consecuencia que el Owner debe ver (N-02) |
| 10 | La composición la aprueban Architect + Coordinator en el paquete de la fase 4; si toca la costura, STOP, I-55 y el Owner | V5 `:32-33` y `:449-450`; WORKFLOW §10 | No | **Architect + Coordinator** (correcto) |
| 11 | Asignación de OV por tramo; OV-A14-01..09 provisionales | LIFECYCLE §8 (reasignar = A-n) | Sí: queda incompleta | **Autoridad de A-14** (Architect + Coordinator); el Owner, informado. Hay que completarla (N-03) |
| 12 | La lista de retiro va por su A-n e incluye la entrada de respaldo | Q-07 («hasta la fase 9»); D-16 congelada | No | **Owner** (la lista); la A-n, Owner + Architect + Coordinator |
| 13 | IC-03 sustituida por Q-10; IC-02 e IC-05, en la firma | Q-10 (Owner); las obligaciones las ratificó el Owner | No | **Owner** (ya decidido). Falta nombrar IC-03 en la firma (§6) |
| 14 | D-07 a «Amends» por su texto | LIFECYCLE §6 | No (editorial) | **Coordinator** |

**Decisiones nuevas que §10.3 no lista:**

| Decisión | Dónde | Fuente | Autoridad |
|---|---|---|---|
| `Applies-to: I-64 (tramo 1)`, y «las añade a esta lista la A-n **o el contrato**» | v2:13-14 | Ninguna. Contradice LIFECYCLE §6 (N-01) | **Coordinator** (forma). Con Architect, porque afecta qué Freeze consume cada unidad |
| La aprobación del prototipo + Q-03 = la decisión del Owner que pide ADR-0029 D13 | v2:409 | Construcción del redactor | **Owner**: que la orden de cada fase rediseñada nombre el registro de cambios declarados (N-17) |
| La propuesta de Smoke-PERF por tramo (SP-01 y SP-05 + los SP tocados) | v2:532-533 | Ninguna; §12.5 la pone solo antes de READY | **Coordinator**, con la **conformidad del Owner**: cambia sus propios hitos (Q-09: «tu validación»). N-05 |
| ADR-0047, opción (a): aceptarlo en el tramo 1 y complementarlo con un ADR nuevo | v2:534-537 | Ninguna. Choca con D-21 `:692-693` (notas del anexo B al aceptar) | **Owner** (acepta). El Architect revisa la forma (M-08). N-05 |
| Las pruebas de F3 retiradas se marcan `Skip` con registro | v2:210-213 | Patrón de OD `:237-243`; SUS-nn | **Coordinator + Architect** (correcto) |
| «RACKEDITAR se queda» en el retiro | v2:348 | Replaneación §3, aprobada («RACKPROYECTAR y RACKEDITAR se quedan») | Ya decidido (Owner) |

## 5. Hallazgos nuevos

### Importantes

**N-01 · `Applies-to` y unidades de los tramos: A-14 y A-1..A-13 no alcanzarían a las unidades de los tramos 2 a 5.**
- **Cita:**
  - v2:13-14: «Applies-to: I-64 (tramo 1). Las unidades de los tramos siguientes (C-12) no existen todavía; las añade a esta lista la A-n
    o el contrato que las cree»;
  - v2:27: «A-1..A-13 no cambian»;
  - v2:511-514: cada tramo siguiente es una unidad nueva cuyo nombre fija el Coordinator.
- **Fuente:** LIFECYCLE §6 `:198` («`Applies-to: all | <lista explicita de unidades>`»), `:201` (la corrección va por la siguiente A-n,
  nunca por edición) y `:203-204` («Una unidad lee todas las A-n, aplica solo las que la incluyen»).
- **Por qué falla:**
  - Un contrato de gate no puede ampliar el `Applies-to` de una A-n, y A-14 no puede editarse tras su commit.
  - A-1..A-13 dicen `Applies-to: I-64` o «I-64 only»: no incluyen a una unidad con otro nombre.
  - Se perdería, por ejemplo, A-7 (S2-16..S2-22), que C-13 (v2:547-548) extiende a «todos los controles de cada editor alojado», justo
    en los tramos 3 y 4.
- **Corrección:**
  - `Applies-to: all` en A-14, con el precedente de las A-n de I-63 (`Applies-to = all`);
  - en «Corrects / supersedes», una cláusula que declare que A-1..A-13 aplican también a las unidades de I-64 de los tramos siguientes;
  - o bien fijar ya que esas unidades se llaman «I-64-…» y que «I-64» en A-1..A-13 designa la iniciativa conceptual;
  - quitar «o el contrato».

**N-02 · El corte propuesto (tramo 2 = fases 4 y 5) más «la fase 6 depende de la 4» ata la fase 6 a la fase 5, Variables incluida.**
- **Cita:**
  - v2:525: «2 | 4 y 5»;
  - v2:697: la fase 6 «Depende de 3, 4 y sus prototipos»;
  - v2:704-705: «la fase 5 avanza en paralelo»;
  - v2:695: Variables depende de «4 y su A-n».
- **Por qué falla:**
  - El tramo 3 es una unidad nueva cortada de `origin/main` (v2:512). Solo dispone del código de la fase 4 cuando el tramo 2 está
    integrado, y el tramo 2 no se integra hasta que termina la fase 5.
  - La fase 5 incluye Variables, que necesita su propia A-n.
  - Resultado: la fase 6 queda en serie detrás de la 5, contra la intención de «en paralelo» y del orden 0 → 1 → 2 → 3 → 4 → 6.
- **Corrección:** una de dos.
  - Proponer como corte principal la alternativa ya escrita (v2:530: «el tramo 1 hasta la fase 4») o un tramo propio para la fase 5.
  - O declarar en C-12 esa consecuencia, para que el Owner la vea al confirmar los hitos.

**N-03 · La asignación de OV por tramo no cubre lo que entregan los tramos 3 y 4.**
- **Cita:** v2:670-674. Al tramo 1: OV-01 a 06, OV-08 a 26 y OV-A14-01 a 05. Al tramo 2: OV-A14-06 y 07. Al último: OV-07. A los tramos con
  sistemas rediseñados, solo OV-17 y OV-A14-09.
- **Fuente:**
  - C-12 (v2:504): «su Owner Validation … con los OV que el tramo entrega»;
  - LIFECYCLE §8 `:245` (READY-08: «toda obligacion aplicable a esta unidad»);
  - Q-09: «Cada hito lleva su candidato completo y tu validación».
- **Por qué falla:** los tramos 3 y 4 integran en `main` los editores de Selectivo, Dinámico, Cantilever, Cabecera y Cama dentro de la
  ventana, y su entrada en «Actualizar todo» (INV-A14-11 y 12 «y 6, 7, 3b»). Sin embargo, no llevan:
  - OV de borrador, Actualizar, Conflicto, UNDO, foco (OV-10..13, OV-19, OV-23) ni resultados visibles (OV-24..26);
  - el OV de «Actualizar todo» (OV-A14-06).

  Además:
  - OV-22 («registros de los diez escenarios», SP-01..SP-06) va solo al tramo 1, aunque el Smoke-PERF completo está en el último;
  - OV-A14-01..09 solo se numeran por el orden del texto (v2:667).
- **Corrección:**
  - asignar a cada tramo de editores los OV de edición, Actualizar, foco y «Actualizar todo» para sus sistemas, que es reasignación por
    esta misma A-n;
  - poner OV-22 también en el último tramo;
  - numerar explícitamente OV-A14-01..09.

**N-04 · «D-PB-01 … sustituida desde el cierre de la fase 3» no dice qué frases del texto literal del Owner se sustituyen.**
- **Cita:** v2:30-31, v2:372 y la firma, v2:816: «D-PB-01 y OD `:37` sustituidas».
- **Fuente:** D-PB-01 es OD `:22-49`, y contiene garantías que A-14 quiere conservar:
  - la superficie lee y escribe el borrador, con cero escrituras authored (`:29-33`);
  - «Do not build a second full Push Back editor» (`:36`);
  - «RACKEDITAR behavior must remain observably compatible» (`:38`), que C-09 solo cambia en la 3b;
  - «The mode must be explicit. Do not let a Workspace invocation accidentally execute the classic drawing-write path» (`:44-46`).

  LIFECYCLE §6 `:199` exige «clausula anterior y delta exacto».
- **Corrección:**
  - sustituir solo «as a modal/full-size editing surface» (`:25-26`), «the drawing cannot be edited while the modal editor is open»
    (`:34`) y `:37`;
  - declarar vigentes `:29-33`, `:36` y `:39-49`, y `:38` hasta la 3b de Push Back;
  - poner lo mismo, en una línea llana, en la firma.

**N-05 · Q-C3 (Smoke-PERF por tramo) y Q-C4 (ADR-0047) quedan abiertos en un artefacto que se vuelve inmutable en el paso 3, sin decir quién los cierra ni dónde.**
- **Cita:**
  - v2:531-537 («Abierto, para el Coordinator»);
  - v2:796: el paso 2 resuelve solo Q-C1 y Q-C2 antes de versionar;
  - v2:798: «Desde ese commit A-14 es append-only».
- **Fuente:**
  - §12.5 (V5 `:900-925`) pone Smoke-PERF antes de READY. Cambiar qué SP corre en qué READY cambia un resultado congelado del plan de
    gates, y exige una A-n (LIFECYCLE §6). READY-01 del tramo 1 necesita esa regla.
  - D-21 `:692-693`: las notas del anexo B se escriben al aceptar el ADR. Con la opción (a), la nota a ADR-0010 que propone la v2
    (v2:598) describiría «Actualizar todo» un tramo antes de que exista.
  - La revisión del v1 (R2-02) pedía «una regla explícita» para Smoke-PERF.
- **Corrección:**
  - resolver Q-C3 y Q-C4 antes del paso 3, para que A-14 lleve la regla;
  - o escribir que los fija la A-n siguiente antes del READY del tramo 1, con la autoridad de cada uno: Coordinator con la conformidad
    del Owner para Smoke-PERF; el Owner para ADR-0047;
  - en la opción (a), decir qué notas del anexo B se escriben en el tramo 1 y cuáles después.

### Menores

| Id | Cita (v2) | Fuente | Corrección |
|---|---|---|---|
| N-06 | v2:501-510: lo que tiene cada tramo | LIFECYCLE §2 `:60-62`: «Cada unidad hace su propio bootstrap, Discovery delta, gates, READY, Candidato e integracion», con DC-01..06 recomprobados y DC-07..09 actuales | Añadir bootstrap y Discovery delta a la lista de C-12 y contarlos en R-08 |
| N-07 | C-03, v2:204-207: «la lista es cerrada» | Muchas líneas de la lista de lectura tienen además delta propio, con otro texto, en §4 o §5: `:56`, `:89`, `:93`, `:105`, `:186`, `:259`, `:299`, `:325`, `:337-338`, `:402`, `:404`, `:434`, `:545`, `:549-551`, `:740-776`. La lista omite `:380` (encabezado de §5), `:659` (tabla de host de D-18, que solo cubre §4) y las filas de historia de §18-§20 (`:1078`, `:1084`, `:1085`, `:1123`, `:1142`, `:1144`). C-03 (v2:210) no nombra las pruebas de pestañas de INV-15, INV-09, INV-38, INV-02 ni el escenario 5. El delta de SP-03 no dice si conserva «incluidos cambios frecuentes de referencias (R-10)» | Regla de precedencia: «donde §4 o §5 da un delta propio, prevalece sobre la regla de lectura». Añadir §18-§21 a la historia que no se reescribe, completar la enumeración de F3 y aclarar SP-03 |
| N-08 | C-10, v2:435-436: «"Posición" incluye si estaba acoplada o flotante y en qué lado» | Es lectura de la decisión 2, no texto del Owner (patrón de M-12) | Marcarla «lectura de la sesión» y nombrarla en la firma |
| N-09 | C-05, v2:283: «Las fases 6, 7 y 3b extienden las pruebas de "Actualizar todo" (INV-A14-11 a 15)» | La tabla, v2:650-652, pone solo «4» en INV-A14-13, 14 y 15 | Poner «4 (y 6, 7, 3b)» en los tres |
| N-10 | C-07, v2:339-340: «Todo el contenido del menú queda en la ventana» (fase 1) | El menú incluye la ruta de la biblioteca de bloques, que escribe `UserSettings` (`RackMainMenuWindow.xaml:150-164`; `UserSettings.cs:13`). La excepción 2 de D-22 (v2:447) rige desde la fase 5 | En la fase 1, que esa entrada lance el flujo clásico, o que la excepción 2 empiece en la fase 1 |
| N-11 | C-01, v2:151: «Las secciones funcionan dentro de la ventana sin bloquear (fase 5)» | Variables queda pendiente de su A-n (v2:483); la Biblioteca inserta por el flujo clásico (v2:472); R-05 deja subdiálogos modales | Acotar: «… salvo Variables (su A-n) y los subdiálogos de R-05» |
| N-12 | C-02, v2:178: «Un borrador dirty o con texto pendiente sigue vivo al dejar el rack» | Dejar el rack abierto es frontera bloqueable (v2:172): el texto se compromete o la acción se aborta, así que no puede salir «con texto pendiente». La condición 3 de C-05 (texto conservado en borradores no abiertos) no dice qué frontera no bloqueable lo produce con K = 1. La replaneación §3 dice «los borradores siguen vivos al cambiar» | Decir «dirty» (sin «texto pendiente») y nombrar el caso que deja texto conservado en un borrador no abierto, o declarar la condición 3 como salvaguarda. Precisar «los borradores con cambios» |
| N-13 | R-08, v2:733: «Las fases 1 y 2 son ligeras» | La fase 2 lleva F6 (V5 §13 `:982`: «Deep»), el arnés de 70 racks y el registro de sustitución de F3 | «La 1 es ligera; la 2 lleva F6» |
| N-14 | §10, paso 3 (v2:797-798) frente a §13 (v2:854) y las fuentes (v2:55-56) | El paso 3 versiona la v2, el registro del Owner y la replaneación. §13 añade la revisión del v1 y su disposición, y las fuentes, el prototipo (Q-C1) | Una sola lista en el paso 3 |
| N-15 | C-07 (v2:331) y C-10 (v2:426): no-objetivos `:80` y `:79` | Sin texto nuevo literal; C-05 sí lo da para `:78` (v2:289-290). LIFECYCLE §6 `:199`, delta exacto | Escribir el texto nuevo de `:79` y `:80` |
| N-16 | §1, v2:78-79: «IC-11 añadió F4B … sin cambiar resultados congelados» | La viñeta anterior (v2:69-73) dice que F4B se apartó de D-15 punto 1 y de F4 («Push Back editable en el panel») sin declararlo | Atribuirlo: «según OD `:20`, F4B extiende sin reabrir F4»; A-14 declara la diferencia |
| N-17 | C-09, v2:397-400 y v2:409 | La extensión a los comandos de creación y «prototipo + Q-03 = la decisión que pide D13» son deducciones de la sesión. D13 cubre Enter, Escape, foco, tabulación y caminos de cierre, que un prototipo visual puede no mostrar (ADR-0029 D13) | Marcar las dos como lectura de la sesión. Exigir que la orden del Owner de cada fase rediseñada nombre el registro de cambios declarados, como D-PB-06 nombró sus correcciones (SUS-01..04) |
| N-18 | C-05: el progreso solo en R-04 (v2:310, v2:729) | Replaneación §4, fase 4: «progreso y lista de lo que falló» (aprobada por lectura) | Añadir «progreso visible» al resultado de C-05 o a OV-A14-06 |
| N-19 | C-01, v2:23 y v2:130: «el brief lo admite: "unless Freeze explicitly changes it", `:393-394`» | Esa salvedad del brief solo cubre RACKEDITAR (brief `:393-394`). Para `:126-132`, `:444-457`, `:474` y `:475`, la base es que el brief es del Owner (`OWNER-RESERVED`) | Citar `:393-394` solo para RACKEDITAR; para las demás, la decisión del Owner |

## 6. Texto de la firma (§11, v2:810-822)

- **Exacto:** sí. Es un texto literal cerrado, con `<SHA>`, y sigue el patrón de A-11 a A-13.
- **Completo:** casi. Nombra C-01, C-02, C-03, C-05 a C-12, la sustitución de OV-08/09/11/14/17/20 y la autorización de F5-A. Sacar C-04
  respecto de la lista del v1 es correcto: resecuenciar es Coordinator-only (LIFECYCLE §6 `:205`), y lo que cambia su resultado va con
  C-02. Faltan:
  1. **IC-03 sustituida por Q-10.** Es la decisión C-F3-04 del propio Owner (ICL `:59`). Basta media línea.
  2. **El alcance exacto de la sustitución de D-PB-01** (N-04). Tal como está, el Owner firmaría la retirada entera de un texto suyo que
     A-14 quiere conservar en parte.
  3. **La lectura de «posición»** (N-08) y **la deducción de los comandos de creación** (N-17), si no se marcan en el cuerpo.
  4. **Smoke-PERF por tramo**, si se resuelve antes (N-05). Cambia los hitos del propio Owner.
  5. **Qué pasa si el Owner corrige los hitos** al firmar. «[o corrijo]» choca con «la corrección es la A-n siguiente» (v2:821-822).
     Cambiar los tramos reasigna OV (§5), y eso exige A-n. Hay que decir si una corrección de hitos en la firma dispara la A-15.
- **Entendible para el Owner:** poco. Es una sola frase de 8 líneas con identificadores sin glosa («OD `:37`», «IC-02 e IC-05»,
  «OV-08 … OV-20», «el residual nuevo de ID3», «D-22»). Propuesta:
  - mantener la frase exacta para el registro;
  - añadir debajo una lista numerada en lenguaje llano, una línea por materia, por ejemplo: «4. "Actualizar todo" escribe todos los racks
    con cambios de una vez, o ninguno; si un rack tiene un conflicto o texto a medio escribir, no se escribe nada y se te dice cuál»;
  - pedir la firma sobre la frase, con la lista como guía.

## 7. Comprobaciones que pasaron

- **Freeze:** identidad (blob en HEAD y en `55e7ca63`, `9b43dafb` es un commit); W limpio en `fa59bd82`.
- **Citas de V5 comprobadas:**
  - :32-33, :53, :56, :64, :71-80, :82-95, :105-106, :122-147, :151, :173-174, :186, :215-216, :241-243, :259-260, :284-299, :309-313,
    :317-325, :335-339;
  - :382-397, :402-406, :410, :425-440, :449-451, :459-461, :475-477, :487-493, :502-509, :521-545, :549-551, :562-593;
  - :647-662, :679-693, :702-712, :737-778, :790-813, :826-829, :900-960, :995-996, :1188-1225.
- **Otras fuentes:**
  - el brief: `:77`, `:94`, `:126-132`, `:263`, `:371`, `:377`, `:391`, `:393-394`, `:436-438`, `:444-457`, `:474-476`;
  - [OD]: `:20`, `:22-49`, `:76-78`, `:237-245`, `:269`, `:293`;
  - el paquete: `:59`, `:60`, `:86`, `:171`, `:174`, `:200`; ICL: `:3`, `:53`, `:59`, `:89`, `:204`;
  - PB-T5 v2.4: `:17`, `:241`, `:260-261`, `:276`;
  - LIFECYCLE: `:54-64`, `:94`, `:198-206`, `:245`, `:255-258`; WORKFLOW: `:398`, §11.6; `docs/adr/README.md:37`.
- **Código:** `RackMenuCommands.cs:21-34` (RK = RACKCAD, `ShowModalWindow`), el menú, `UserSettings.cs:13-27` y
  `RackSiblingRedrawRun.cs:30-32`.
- **Precedentes y estados:** SM-03, F1-T3C, el incidente de PASTECLIP; los estados de F1 a F4B en la tabla de correspondencia.
- **F4B y PB-T5 sin cambio:** SPB-03 sigue valiendo por la regla de vigencia (v2:107-108). IC-11 y OB-02.3 se conservan dentro de F4B.
- **Materialidad:** coherente con la regla de V5 de reabrir M-02 y M-03 (`:995-996`).
- **Publicabilidad:** sin rutas locales, nombres de personas, correos ni datos de clientes en la v2, la disposición, el registro del Owner y
  la replaneación. Lo que se versionaría es publicable. La evidencia con dibujos reales queda cubierta por R-17.
