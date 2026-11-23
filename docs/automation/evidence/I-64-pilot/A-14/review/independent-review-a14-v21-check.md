# Comprobación acotada de I-64 A-14, borrador v2.1

## 0. Identidad, modo y método

- **Objeto:** `S/I-64-A-14-draft-v2.1.md`, SHA-256 `e4758ae9132dd89d62239307ef8e45da81352faac339ce35858dcda362a25925` (1028 líneas). Se cita
  «v2.1:n».
- **Disposición:** `S/a14-v21-disposition.md`, SHA-256 `c410c93b…ed53`.
- **Notas:** `S/a14-drafting-notes.md` §11, SHA-256 `2d35bec7…f416`. No existe una §12 en las notas; las decisiones nuevas del redactor están
  en las notas §11.3 y en la v2.1 §12.
- **Instrucciones del redactor:** `S/a14-v21-instructions.md`, SHA-256 `f378885a…122a`.
- **Decisiones del Owner:** `S/owner-a14-decisions.md`, sin cambios desde la comprobación anterior (SHA-256 `f1d0ea5d…4c2e`).
- **Hallazgos que se comprueban:** los de mi comprobación de la v2, `S/independent-review-a14-v2-check.md`: R2-02 y N-01..N-19.
- **W:** solo lectura, en HEAD `fa59bd82`.
- **Lecturas nuevas en W:**
  - las cabeceras de A-1..A-13 (Owner, `Applies-to`, alcance);
  - A-1 `:59-73`;
  - A-2..A-6 `:33-35`;
  - OD `:1-60`;
  - PKG `:129`, `:411`.
- **Reutilizado de la comprobación de la v2:** las lecturas de V5, LIFECYCLE, WORKFLOW, ICL, OD, PKG, PB-T5, el brief y el código.
- **Modo:** SEPARATE SESSION, el mismo revisor que la comprobación de la v2. No hubo escrituras en W ni órdenes git que cambien estado, ni
  AutoCAD, compilación o pruebas.
- **Cómo se decidió cada estado:** en el texto de la v2.1 y en la fuente. La tabla de disposición solo sirvió para ubicar.

## 1. Veredicto

**EMITIBLE TRAS CORRECCIONES** (una corrección pequeña, pero toca autoridad del Owner y debe hacerse antes del paso 3 de §10).

| Hallazgos de la v2 (20: R2-02 y N-01..N-19) | Resueltos | Parciales | Sin resolver |
|---|---|---|---|
| | 19 | 1 (N-01) | 0 |

| Hallazgos nuevos de la v2.1 | Bloqueantes | Importantes | Menores |
|---|---|---|---|
| | 0 | 1 | 5 |

La v2.1 aplica bien casi todo:
- `Applies-to: all`;
- el corte con el tramo 1 = fases 0 a 4, con su alternativa visible;
- la regla de OV por tramo y su tabla;
- la tabla exacta de D-PB-01;
- reglas y valores por defecto para Smoke-PERF y ADR-0047;
- cada pendiente con su documento y su momento;
- las dos deducciones de Q-03 y la lectura de «posición» marcadas y llevadas a la firma;
- la firma numerada, con su lista en lenguaje llano.

Lo que falta corregir es el importante (N2-01): al declarar que A-1..A-13 alcanzan a las unidades nuevas, la v2.1 extiende también los
puentes de protocolo de emergencia A-1..A-6. El Owner los autorizó «only for this I-64 unit-scoped exception», y la extensión no figura en
la firma. El origen está en mi corrección de N-01, que pedía declararlo para A-1..A-13 sin distinguir: era demasiado amplia.

## 2. Los hallazgos de la v2, comprobados en el texto de la v2.1

| Id | Estado | Dónde (v2.1) | Comprobación |
|---|---|---|---|
| R2-02 | **RESUELTO** | Cabecera (v2.1:13-15); C-12 (v2.1:550-608); §5 (v2.1:755-766); §12 | Quedan cerrados los cuatro residuos: `Applies-to: all`; Smoke-PERF y ADR-0047 con regla y valor por defecto; los OV por tramo; el bootstrap y el Discovery delta (v2.1:553) |
| N-01 | **PARCIAL** | Cabecera (v2.1:13-15, 27-30); C-12 (v2.1:564-566) | `Applies-to: all` es correcto, y el precedente de I-63 A-1 `:9` («Applies-to = all») está comprobado. Ya no aparece «o el contrato». **Residuo:** la extensión de A-1..A-13 es demasiado amplia y contradice «Corrects / supersedes: Ninguna A-n anterior» (N2-01) |
| N-02 | **RESUELTO** | C-12 (v2.1:579-595); §6 (v2.1:783-784) | Tramo 1 = 0-4 y tramo 2 = 5. Las dependencias están comprobadas: la fase 6 depende de 3 y 4, la 5 de 1 y 4, la 3b de 4; los tramos 2 y 3 dependen solo del 1. La alternativa 0-3 se muestra con sus consecuencias. Un desequilibrio en la presentación, en N2-03 |
| N-03 | **RESUELTO** | C-12, regla (v2.1:567-573); §5 (v2.1:741-766) | Los tramos 3 y 4 llevan los OV de edición por sistema (OV-09..13, 19, 23..26, OV-A14-05 y 06), además de OV-17 y OV-A14-09. OV-07 y OV-22 van al último tramo, y OV-22 también al tramo 1, dentro de «OV-08 a OV-26». OV-A14-01..09 están numerados |
| N-04 | **RESUELTO** | C-08, tabla (v2.1:396-411); cabecera (v2.1:33-34); §8; firma (7) | Cotejado con OD `:22-50`. Se sustituyen solo `:25-26` («as a modal/full-size editing surface»), `:34` y `:37`. Se conservan `:23-24`, «loaded from the Workspace draft», `:29-33`, `:36` y `:39-49`, y `:38` hasta la 3b. Cada texto citado es literal. OB-01.4 cae con `:34`, como debe |
| N-05 | **RESUELTO** | C-12 (v2.1:596-608); §12, pendientes (v2.1:992-1010) | Cada pendiente tiene dueño, documento y momento. Una incoherencia de autoridad en Smoke-PERF queda en N2-02 |
| N-06 | **RESUELTO** | v2.1:553; R-08 (v2.1:823) | LIFECYCLE §2 `:60-64` |
| N-07 | **RESUELTO** | C-03 (v2.1:212-229); §4 §12.5 (v2.1:662) | Precedencia; `:380` y `:659`; las tablas de §18 a §21 como historia; la enumeración de F3; SP-03 con «(R-10)» |
| N-08 | **RESUELTO** | §2, dec. 2; C-10 (v2.1:482-483); firma (9); §12, fila 12 | |
| N-09 | **RESUELTO** | v2.1:723-725 | |
| N-10 | **RESUELTO** | C-07 (v2.1:363-364); C-10, excepción 2 (v2.1:494-496) | Opción elegida: la excepción 2 desde la fase 1. Tiene fuente: el menú ya guarda ese campo (`UserSettings.cs:13`; `RackMainMenuWindow.xaml:150`). La firma no la nombra de forma explícita (§5) |
| N-11 | **RESUELTO** | v2.1:158 | |
| N-12 | **RESUELTO** | C-02 (v2.1:183-186); C-05, cond. 3 (v2.1:283-290); INV-A14-08, 15 | La salvaguarda está razonada con D-10 `:434-437`. El delta de D-14 en §4 (v2.1:649, «con dirty o texto pendiente conserva su borrador al dejarlo») sigue siendo cierto: al salir, el texto se compromete o la salida se aborta |
| N-13 | **RESUELTO** | R-08 | V5 `:982`, «Deep» |
| N-14 | **RESUELTO** | §10, paso 3 (v2.1:890-897); §13; Fuentes | Hay una lista única. No incluye las comprobaciones posteriores (N2-05) |
| N-15 | **RESUELTO** | v2.1:359-361 (`:80`); v2.1:480-481 (`:79`) | Texto literal nuevo |
| N-16 | **RESUELTO** | §1 (v2.1:82-84) | OD `:20`, literal |
| N-17 | **RESUELTO** | §2, dec. 6; C-09 (v2.1:438-441, 450-453); firma (8); §12, filas 10 y 11 | Las dos deducciones están marcadas. La decisión que pide D13 queda definida con contenido: la orden de cada fase nombra el registro fila por fila, y lo que no nombre es STOP |
| N-18 | **RESUELTO** | C-05 (v2.1:267-269); OV-A14-06; §6, fase 4 | |
| N-19 | **RESUELTO** | Cabecera (v2.1:24); C-01 (v2.1:135-144) | |

## 3. Las decisiones nuevas del redactor (v2.1 §12; notas §11.3)

| # | Decisión | ¿Fuente o decisión material nueva? | Quién decide |
|---|---|---|---|
| 1 | Smoke-PERF completo en el READY de cada tramo, por defecto | Tiene fuente: es la lectura conservadora de §12.5 («Smoke-PERF antes de READY»). Pero multiplica las sesiones de host del Owner, hasta cinco Smoke-PERF completos | **Owner**, que confirma el valor por defecto en la firma, materia (11). La reducción va por A-n, con el Coordinator y la conformidad del Owner. La autoridad está escrita de dos formas (N2-02) |
| 2 | ADR-0047 sigue `propuesto` por defecto; el Owner decide en el READY de cada tramo | Tiene fuente: D-21 `:688` («solo el Owner lo acepta»), D-21 `:692-693` (las notas, en el commit de cierre) y `docs/adr/README.md:37` | **Owner** (acepta). El Architect revisa la forma (M-08) si se acepta antes del último tramo |
| 3 | Si el Owner corrige los hitos: A-14 se activa igual, la tabla nueva va por la A-n siguiente y no se avanza en un tramo afectado | La parte de la A-n tiene fuente (LIFECYCLE §8: reasignar OV exige A-n). «Se activa igual» es una decisión de proceso nueva, razonable | **Coordinator**. El Owner la acepta al firmar, porque está en la materia (11). Hay una incoherencia de momento (N2-04) |
| 4 | Si el Owner rechaza otra materia: A-14 no se activa y la corrige la A-n siguiente | Tiene fuente: LIFECYCLE §5 `:173` («BLOCKED — OWNER DECISION») y §6 `:201`. El precedente A-12 → A-13 es análogo, aunque nació de un rechazo del Architect. Retirar la «activación parcial» fue correcto | **Coordinator** |
| 5 | N-10: la ruta de la biblioteca de bloques entra por la excepción 2 desde la fase 1 | Tiene fuente (el menú ya guarda ese campo). Sigue siendo una excepción a D-22 (M-02) | **Architect + Coordinator**. El Owner la ve en la firma, materia (9). Conviene nombrarla allí |
| 6 | Tramo 2 = solo la fase 5; el orden de los tramos 2 y 3, o si van en paralelo, lo fija el Coordinator | Viene de N-02 y de la orden del Coordinator. Los hitos son del Owner (Q-09); el paralelismo es proceso | **Owner** (hitos, en la firma). **Coordinator** (orden y paralelismo, WORKFLOW §2) |
| 7 | IDs de INV-A14 y OV-A14 definitivos | Sin carga material: es nomenclatura. Sustituye la numeración que V5 dejaba al Coordinator | **Coordinator**, en la orden de revisión del Architect |
| 8 | OV-A14-01..09 reescritos; el 05 y el 06 «por sistema» | Vienen de N-03 y de Q-09 | **Autoridad de A-14** (Architect + Coordinator). El Owner, en la firma (11) y (12) |
| 9 | La condición 3 de C-05, como salvaguarda | Tiene fuente: D-10 `:434-437`. No cambia la regla | **Architect** (comprobación) |
| 10 | Q-C renumeradas | Editorial | **Coordinator** |
| — | **No listada:** que A-1..A-13 «aplican también a las unidades de I-64 de los tramos 2 y siguientes» | **Decisión material nueva, sin fuente para A-1..A-6.** Extiende excepciones de protocolo que el Owner firmó «only for this I-64 unit-scoped exception» (N2-01) | **Owner** (y el dueño del protocolo, según el campo de A-1) + Coordinator + Architect. Hoy está asignada solo a Coordinator + Architect (§12, fila 3) |

## 4. Hallazgos nuevos

### Importante

**N2-01 · La extensión de A-1..A-13 a las unidades nuevas alcanza puentes de emergencia que el Owner autorizó solo para la unidad I-64, y la firma no lo nombra.**
- **Cita:**
  - v2.1:29-30: «Alcance de A-1..A-13: donde dicen "I-64" o "I-64 only", designan la iniciativa conceptual I-64, y aplican también a las
    unidades de I-64 de los tramos 2 y siguientes. No reabre ni reactiva ningún puente consumido»;
  - v2.1:28: «Corrects / supersedes: Ninguna A-n anterior»;
  - v2.1:845 (§8) y §12, fila 3 (autoridad: Coordinator + Architect).
- **Fuente:**
  - A-1 `:73` (I64-SCOPE-BRIDGE-01): «Se aplica a las tareas delegadas futuras de I-64 hasta que una autoridad de protocolo integrada la
    sustituya». Es un puente **vigente**, no consumido. Lo autorizó una orden de emergencia del Owner, y A-1 tiene un campo «Protocol-owner
    decision».
  - A-2 `:33`, A-3 `:33`, A-4 `:33`: «SATISFIED only for this I-64 unit-scoped exception».
  - A-5 `:35` y A-6 `:33`: lo mismo para FreeText. A-5 `:208` añade: «Caduca para I-64 cuando una revisión autoritativa integrada del
    protocolo la sustituya».
  - LIFECYCLE §3 `:94`: cambiar el alcance de una decisión del Owner es `OWNER-RESERVED`.
  - LIFECYCLE §6 `:199` y `:201`: cláusula anterior y delta exacto, y la sustitución va por la siguiente A-n.
- **Por qué falla:**
  - La salvedad «puente consumido» cubre A-8..A-13, que nombran una ocurrencia. No cubre A-1..A-6, que siguen vigentes.
  - Las unidades de los tramos 2 a 5 heredarían, sin firma del Owner, excepciones de protocolo que él limitó a esta unidad.
  - La cabecera dice además que no sustituye ninguna A-n anterior, cuando en la práctica cambia el `Applies-to` de trece.
- **Corrección:**
  - limitar la extensión a A-7 (INV-24, S2-16..S2-22), que es lo que pide C-13, y a cualquier otra A-n que no sea un puente;
  - declarar que A-1..A-6 y A-8..A-13 siguen siendo de la unidad I-64. Si una unidad nueva necesitara un puente, se pide por su A-n, con el
    Owner y el dueño del protocolo;
  - en «Corrects / supersedes», nombrar el delta exacto: «el `Applies-to` de A-7 (solo su alcance)»;
  - si se quisiera extender algún puente, nombrarlo en la firma como materia `OWNER-RESERVED` y añadir al Owner en §12, fila 3;
  - el origen es mi corrección de N-01 en la comprobación de la v2, que pedía declararlo para A-1..A-13 sin distinguir: era demasiado
    amplia.

### Menores

| Id | Cita (v2.1) | Fuente | Corrección |
|---|---|---|---|
| N2-02 | Smoke-PERF por tramo: C-12, autoridad (v2.1:610: `OWNER-RESERVED` por «la regla de Smoke-PERF por tramo»); C-12 (v2.1:598-599: «autoridad del Coordinator y la conformidad del Owner»); §12, fila 16 («Coordinator, con la conformidad del Owner»). «Sobre lo que esté integrado» (v2.1:597) | §12.5 (V5 `:902`): se ejecuta «sobre un Candidato completo», antes de integrar. Lista en lenguaje llano, punto 12: «su prueba de rendimiento» | Elegir una sola autoridad y escribirla igual en los tres sitios. Cambiar «lo que esté integrado» por «el Candidato del tramo». En el punto 12, decir «la prueba de rendimiento completa, con tus dibujos reales, en cada tramo», porque es carga del Owner |
| N2-03 | C-12 (v2.1:587-592) y lista en lenguaje llano, punto 12 | Q-09: «Usas antes lo nuevo y la rama no envejece»; R-11 | Se dan las consecuencias de la alternativa, pero no el coste de la propuesta: el tramo 1 (que ya lleva F1 a F4B) espera además a la fase 4, que es la de más riesgo, con posible STOP frente a la costura de I-55. Eso retrasa la primera integración. Dar los pros y contras de las dos opciones. En §6, fase 4, «cierra el tramo 1» es impreciso: lo cierran su READY y su OV |
| N2-04 | Firma (11): «la versiona la A-n siguiente antes de abrir el primer tramo afectado» (v2.1:930-931) | C-12 (v2.1:594-595): «antes del READY-01 del primer tramo afectado». Con la alternativa, el tramo afectado es el 1, que ya está abierto | Usar en los dos sitios «antes del READY-01 del primer tramo afectado» |
| N2-05 | §10, paso 3 (v2.1:895): «la revisión del v1, la comprobación de la v2 y sus dos tablas de disposición» | Esta comprobación de la v2.1 y las posteriores quedan fuera. LIFECYCLE §5 `:162-163` pide la disposición por ID de los hallazgos previos | «Todas las revisiones y comprobaciones independientes y sus disposiciones» |
| N2-06 | §5, tramo 2 (v2.1:760): «si Variables entra en este tramo, además …» | C-11 (v2.1:524-532): Variables depende de su A-n | Añadir: «si no, el tramo y los OV de Variables los fija su A-n» |

## 5. La firma (§11, v2.1:907-963)

- **Exacta:** sí.
  - La frase tiene 13 materias numeradas, cada una con su C-nn, y una elección de hitos entre corchetes.
  - La materia (7) cita el texto literal de D-PB-01 que se sustituye.
  - El párrafo de después (v2.1:935-937) distingue elegir los hitos (parte de la firma) de rechazar una materia (A-14 no se activa). Eso es
    correcto.
- **Completa:** casi.
  - Recoge IC-03 (2), el alcance de D-PB-01 (7), las dos deducciones (8), la lectura de «posición» (9), Smoke-PERF (11) y qué pasa si se
    corrigen los hitos (11).
  - **Falta:** la extensión de A-1..A-13 a las unidades nuevas, que es `OWNER-RESERVED` en lo que toca a A-1..A-6. Se resuelve quitándola
    (N2-01) o nombrándola.
  - **Conviene añadir:** en (9), «incluida la ruta de la biblioteca de bloques desde la fase 1» (N-10).
- **Entendible para el Owner:** sí, con la lista en lenguaje llano (puntos 1 a 14). Es fiel a la frase y no usa identificadores sueltos.
  Cuatro ajustes:
  1. **Punto 5:** añadir «con un solo Deshacer y viendo el progreso».
  2. **Punto 10:** añadir «y en qué lado». Es parte de la lectura que el Owner confirma.
  3. **Punto 12:** añadir «la prueba de rendimiento completa en cada tramo» y «si cambias los tramos, hace falta una enmienda más antes de
     seguir» (N2-02, N2-04).
  4. **Puntos 9 y 10:** marcarlos con «(confirmas una lectura nuestra)», para que el Owner sepa que ahí no repite una decisión suya, sino
     que la amplía.

## 6. Comprobaciones que pasaron

- **Decisiones del Owner:** siguen fieles. Q-03 tiene las deducciones marcadas. Q-09 tiene los hitos a elegir y validación por tramo. Q-07,
  Q-02 y Q-10 no cambian. El ejemplo literal de Q-09 (tramo 1 = 0-3) queda visible como alternativa.
- **Citas nuevas comprobadas:**
  - OD `:20`, `:22-50`;
  - I-63 A-1 `:9`;
  - V5 `:380`, `:659`, `:902`, `:909`, `:982`;
  - LIFECYCLE `:60-64`;
  - `UserSettings.cs:13` y `RackMainMenuWindow.xaml:150`.
- **Publicabilidad:** ni la v2.1, ni su disposición, ni la comprobación de la v2 (que se versionaría) contienen rutas locales, nombres de
  personas, correos ni datos de clientes.
