# I-64 F4B: disposiciones de la fase «Push Back completo»

Registro de las decisiones de F4B. Cada entrada cita su fuente literal. La numeración F4B-Enn es propia de la fase. Las disposiciones de F4 siguen en `F4-DRAFTS/f4-dispositions.md`, y F4B no las reabre.

## F4B-E01. Autorización de F4B (Owner y Coordinator, 2026-11-17)

**Fuente literal:** `F4B-DRAFTS/coordinator/owner-f4b-disposition.md`, la respuesta al paquete v1 (`F4B-DRAFTS/package/package-v1.md`).

**Lo que dispone:**
- F4B — PUSH BACK COMPLETO va entre F4 y F5-A. F4 sigue CLOSED / PASS WITH LIMITATIONS y F4B no la reabre. F5-A espera a que F4B cierre.
- D-PB-01: «Editar todo» abre la ventana existente de Push Back en modo WORKSPACE, modal, cargada desde el borrador y sin ninguna escritura en el DWG. El modo debe ser explícito.
- D-PB-03: los racks A/B entran en el alcance, pero solo se admiten cuando el estado sea sin pérdidas.
- D-PB-04: borrar vistas obsoletas, sí; crear vistas no está autorizado.
- D-PB-05: «Conservar mi borrador» con el diseño editable completo; el sobre y los metadatos no vuelven atrás.
- D-PB-06: las correcciones (a) y (b) como cambio declarado.
- D-PB-08, aceptada en su concepto; D-PB-09, con sus casos fabricados rotulados; D-PB-10, 1/3 por tarea, con el intento 3 siempre al Owner.
- Las tareas PB-T0 a PB-T4 aceptadas, la regla de T1B y RACKEDITAR como superficie de compatibilidad.
- **Estados:**
  - PB-T0: CONDITIONALLY AUTHORIZED AFTER v2 REVIEW;
  - PB-T1 a PB-T5: WAITING;
  - F5-A: AUTHORIZED / WAITING FOR F4B;
  - F6 y F7: PENDING.

**Lo que no dispone:** D-PB-07, sobre «Guardar en biblioteca», el BOM y el lote de vistas en el modo WORKSPACE. La orden pide «the exact text of ALL ten D-PB decisions» y trae texto para nueve. La pregunta se hace antes del contrato de PB-T2 (OB-07.1 del paquete).

**Declaraciones de la sesión** (informativas; IR2 R-M-01, R-M-03 y R-M-05):
- se añade la tarea **PB-TC**, con las correcciones declaradas de D-PB-06 y D-PB-08 antes de PB-T1 y su propio presupuesto de 1/3;
- se corrige el marco de D-PB-04: el «Actualizar» clásico nunca crea vistas (H-PB-16), y la decisión del Owner no cambia (OB-04.4);
- D-PB-07 sigue abierta.

**Valores por defecto conservadores** (IR2 R-I-01 y R-I-02):
- en F4B no hay replanificación gratuita ni sustitutos de control: un rechazo de la planificación o un control fallido vuelven al Owner;
- el bloque M de PB-T0 lo opera el Owner presente, salvo orden expresa suya a la sesión.

Las dos son preguntas de procedimiento que no bloquean PB-T0.

## F4B-E02. El paquete de la fase, v3.1: se cumplen las condiciones para preparar PB-T0 (2026-11-17)

**La cadena de revisión:**

| Versión | Revisión | Resultado |
|---|---|---|
| v2 | IR2, revisión adversarial independiente que pide la orden | **APTO TRAS CORRECCIONES** (0/11/14) |
| v3 | IR3, comprobación acotada del mismo revisor | **APTO**: 21 resueltos, 4 parciales con residuo MENOR y 0 sin resolver; nuevos 0/0/3 |
| v3.1 | — | Aplica los residuos y los nuevos de IR3, solo en el texto |

**Decisión de la sesión, por la autorización condicional de la orden:** la revisión de la v2 queda con 0 bloqueantes y 0 importantes sin resolver, y ninguna decisión material de producto nueva hace falta antes de PB-T0. Se puede preparar y emitir PB-T0 sin volver al Owner.

**Siguiente:**
1. El paquete del contrato de PB-T0, bloque C, con su propia revisión independiente.
2. La emisión, con attempts 1/3.
3. El bloque M espera al operador (F4B-E01).

**Las vueltas al Owner ya previstas** (§10 del paquete):
- antes de PB-TC: el efecto de (a) y (b);
- antes de PB-T1: los casos de (c) y (d), y las guardas de T1B;
- antes de PB-T2: D-PB-07;
- antes de PB-T3: la `ExtensionData` anidada.

**Custodia:**
- el paquete: `docs/initiatives/I-64-f4b-push-back-completo-package.md`;
- la historia, los mapas y las revisiones: `F4B-DRAFTS/package/`.

## F4B-E03. Decisiones de procedimiento del Owner para F4B (2026-11-17)

**Fuente literal:** `F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md` (**[DP]**). La sesión las preguntó en el chat con AskUserQuestion mientras se revisaba el paquete del contrato de PB-T0, antes de emitirlo. Vienen de IR2 R-I-01 y R-I-02 y del riesgo R-01 de ese paquete. Se transcriben literales las preguntas, las respuestas y la descripción de cada opción elegida.

**Lo que dispone:**
- **Pregunta 1, «Extender firmas exactas»:**
  - PD-F4-01 → el sustituto de nc2 (lista blanca exacta);
  - PD-F4-04 → el sustituto de nc1 (identidad);
  - solo con la firma **EXACTA**, y cada uso se registra;
  - cualquier otra firma o variación vuelve al Owner antes.
- **Pregunta 2, «Extender la regla»:** un rechazo de la planificación por la comparación literal permite **una** replanificación con el prompt reforzado, sin gastar intento. Un segundo rechazo vuelve al Owner.
- **Pregunta 3, «La sesión»:** el bloque M de PB-T0 lo opera la sesión por control de pantalla, con las reglas de Smoke-2.

**Qué sustituye:** los valores conservadores de F4B-E01, y solo en esos tres puntos:
- antes, en F4B no había replanificación gratuita ni sustitutos de control;
- antes, el bloque M lo operaba el Owner presente.

**Dónde se aplica en PB-T0:**
- PBT0-C08 (la replanificación);
- PBT0-C11 (los sustitutos, con las firmas literales);
- INV-PBT0-20 (OD-6);
- OD-1 a OD-6 del paquete del contrato.

No se generaliza por el nombre de la comprobación.

## F4B-E04. PB-T0: cierre de las revisiones del paquete, bloque M medido y emisión del contrato (2026-11-17)

**La cadena de revisión del paquete del contrato de PB-T0 (bloque C):**

| Versión | Revisión | Resultado |
|---|---|---|
| v1 | IRC1, revisión adversarial independiente | **EMITIBLE TRAS CORRECCIONES** (0/4/21) |
| v2 | IRC2, comprobación acotada | **EMITIBLE TRAS CORRECCIONES**. De IRC1: 0 importantes sin resolver y M-16 parcial. Nuevos: 1 importante en el anexo del bloque M (N-I1) y 8 menores (N-01 a N-08) |
| v2.1 | La sesión registra el cierre de IRC2 (§9.2 del paquete) | Aplica M-16 y N-01 a N-08 tal como los propone IRC2. N-I1 se corrige en el anexo v2. IRC2 dice que, aplicados así, no hace falta otra revisión |

**Decisión de la sesión, por la autorización condicional de [OD]:**
- quedan 0 bloqueantes y 0 importantes sin resolver;
- no hace falta ninguna decisión material nueva del Owner: el §8 del paquete dice «Ninguna nueva»;
- PB-T0 se emite con **attempts 1/3**.

El Owner, en el chat, el 2026-11-17: «Sí, arranca con la custodia y la emisión de PB-T0».

**El bloque M** (anexo v2, operado por la sesión según F4B-E03, pregunta 3):
- **Cuándo:** el 2026-11-17, de 15:50Z a 18:16Z. Hubo una pausa de 16:45Z a 18:01Z por orden del Owner.
- **Base:** el DLL de C2, el producto de `5ccaa770`.
- **Dibujos:** solo copias. Los originales no se abrieron.
- **Cierre PASS:**
  - originales sin cambios;
  - las 10 copias iguales a su original;
  - sin `RACKCAD_DEBUG_*`.

**Resultados del bloque M** (`F4B-PBT0-CHARACTERIZATION/block-m/block-m-analysis.md`):
- **H-PB-16 confirmado:** el «Actualizar» clásico no crea vistas, ni al añadir un frente ni al convertir a A/B o volver a un sentido.
  - **§6.4 de la fase, fila 1:** PB-T3 iguala al clásico sin crear vistas.
  - **No vuelve al Owner.**
- **Deshacer:** un `U` deshace cada «Actualizar» entero, también con borrado, renombrado o A/B. Una orden nativa posterior no lo absorbe. El eco del `U` no es fiable.
- **Último vínculo:** con todas las vistas obsoletas no se borra nada y la edición no se escribe; el mensaje es «no se pudo actualizar el sistema Push Back».
- **H-077 confirmado:** la vista insertada tras una edición lleva el sobre previo a la edición.
- **Renombrado:** con colisión, solo cambia el sobre.
- **H-PB-18:** el frontal B de un rack que deja de ser compuesto se redibuja como frontal de un sentido, se renombra y conserva `Section` 2. Por defecto, el panel lo sigue rechazando, y las alternativas pasan a la revisión del paquete de PB-T3.
- **M-7, NO DISPONIBLE:** 0 de 22 sobres Push Back guardan un estilo de cotas. Es el dato para el Owner antes de PB-TC, en la vuelta ya prevista.
- **Hechos nuevos para PB-T3:**
  - RACKEDITAR cuenta las vistas por definición;
  - hay referencias anidadas en dibujos reales;
  - la ida y vuelta de A/B cambia `Composite.AbsentSlotsB`.
- **Ningún resultado exige cambiar producto en PB-T0.**

**Desviaciones del anexo (declaradas en el análisis):**
- M-3 y M-6b se hicieron en M2;
- M-2b, en M4 / PB G, con la purga en la copia de un bloque huérfano «A$C…»;
- la carpeta de custodia: el anexo dice `F4B-PB-T0/block-m/`, y se usa la del `TaskId`, `F4B-PBT0-CHARACTERIZATION/block-m/`.

**MA:** las correcciones y matices viven en el análisis (§3 y §6, que es el MA §11 actualizado). MA no se edita: está custodiado como evidencia histórica en `F4B-DRAFTS/package/`.

**Custodia de este paso:**
- el paquete v2.1, en `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md`, con su estado de emisión;
- la v1, la v2, IRC1 (con su encargo) e IRC2, en `F4B-DRAFTS/pbt0/`;
- [DP];
- el anexo, en `F4B-PBT0-CHARACTERIZATION/coordinator/`;
- el bloque M, en `F4B-PBT0-CHARACTERIZATION/block-m/`;
- el contrato y su script generador van en el commit siguiente.

**Siguiente:** la cadena de PB-T0. Comprende:
- la planificación del Controller y la comparación byte a byte;
- A1-A8 y nc4;
- el Worker: RED con los mutantes de la tabla cerrada y GREEN, su inverso exacto;
- la CI exacta;
- las revisiones y la verificación;
- nc1 a nc3, según PBT0-C11.

## F4B-E05. PB-T0: cadena del intento 1, ratificación del Owner sobre CT-PB0-17 y cierre (2026-11-17)

**Autoridades:**
- el contrato de PB-T0 (F4B-E04);
- las decisiones de procedimiento del Owner (F4B-E03);
- la ratificación del Owner sobre CT-PB0-17: `F4B-DRAFTS/coordinator/owner-pbt0-c16-decision.md`, con la pregunta, las opciones y la respuesta literales.

El paquete de cierre es `F4B-PBT0-CHARACTERIZATION/task-close-package.md`.

**La cadena:**
- **Planificación** `eb30`: **rechazada** solo por la copia.
  - ForbiddenWriteScope 152-154 llevaba un segmento `Systems/` añadido, y una Section de Authorities tenía la concordancia cambiada.
  - Fue STOP P-03 antes del Worker.
- **Replanificación única** `e1ae` (PBT0-C08; F4B-E03, pregunta 2): fidelidad PASS 8/8 y A1-A8 8/8, sin gastar intento. nc4 `aa9d` cumplió su oráculo.
- **Worker** `3c1b` (`claude-sonnet-5-5`/high):
  - RED `8f75a89b`: 21 métodos nuevos y los 20 mutantes de la tabla cerrada;
  - GREEN `28e38d8d`: `git diff f2eedd9a 28e38d8d -- src` vacío.
- **CI exacta:**
  - RED `44174025134`: tests y ui-tests en failure, build-ui en success, build-plugin omitido; las existentes rotas están atribuidas a mutantes (T-8);
  - GREEN `44175320836`: 4/4.
- **Revisiones:**
  - la directa, conforme, con D-1 para el Owner;
  - la adversarial independiente, **REWORK 0/1/6**. La importante, I-01, era la desviación de CT-PB0-17 y se resuelve con una disposición, sin código.
- **Verificación** `3cff`: `EXECUTION_VERIFIED` / NONE, 14/14, con la disposición del Owner como entrada.
- **Controles:** nc1 `e427`, nc2 `46b2` y nc3 `b457` dan CONTROL PASS ordinario.
  - Ningún sustituto: no se reprodujo ninguna firma de F4B-E03.
- **Relevos:** 0 participantes, Snapshot I y el binario acreditado.

**La desviación y la disposición del Owner.**
- **El problema:** MU-PB0-16 no discriminaba CT-PB0-17 con c-10 y c-11, porque `ValidDimensions` normaliza 7 y −1 antes de la ventana (`DynamicRackSystemDocument.cs:227` y `:429-432`). La alternativa W:387 tampoco discriminaba.
- **Lo que hizo el Worker:**
  - añadió c-16 (`Dimensions = 3`);
  - fijó c-12 y c-13 como `RECHAZADO <tipo>: <mensaje>`;
  - no paró con PBT0-C10, siguiendo una cláusula del paso 3 de su prompt que el contrato no tenía.
- **La responsabilidad:** es un defecto de la sesión y se registra como lección.
- **Owner:** «Ratificar el cambio (Recomendado)». c-16 y la forma RECHAZADO quedan como enmienda de CT-PB0-17. PBT0-C10 no se aplica a este intento, y no se gasta intento.

**Resultado:** **F4B-PBT0-CHARACTERIZATION = CLOSED / PASS WITH LIMITATIONS** sobre `28e38d8d`, attempts 1 de 3.
- Los 21 CT-PB0 y T1B forman la red de caracterización de PB-TC a PB-T3.
- El §6.4 de la fase ya estaba resuelto en la fila 1 (F4B-E04).
- Los menores (M-01 a M-06 y D-2 a D-5) y la observación de CT-16 van a PB-TC y PB-T1 según el §5 del paquete de cierre.

**Nota del Owner sobre la interfaz** (`F4B-DRAFTS/coordinator/owner-note-ui-deferred.md`, literal):
- los campos o botones extra que vio se arreglan después de la primera etapa; «lo importante es la funcionalidad»;
- no cambia OB-01.5 ni OB-01.6.

**Siguiente:**
1. El paquete de PB-TC, con su revisión independiente.
2. Antes de emitir PB-TC, la vuelta al Owner prevista:
   - el efecto medido de (a) y (b) (T-3, con la lectura OWNER-4);
   - M-7 = 0/22;
   - H-PB-28 (OWNER-2).
3. Antes del contrato de PB-T1: `pb-21-cola` (OWNER-3) y los demás casos de T-2 (OB-06.5).
4. PB-TC no se emite sin la orden del Owner.

## F4B-E06. Decisiones del Owner antes de emitir PB-TC

**Fecha:** 2026-11-17 (noche). **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbtc-decisions.md`.

**El paquete:**
- La sesión principal preparó el paquete del contrato de PB-TC.
- Pasó tres revisiones independientes y adversariales en solo lectura:
  - IRT1, sobre la v1: NO EMITIBLE 1/4/10;
  - ICT2, sobre la v2: EMITIBLE TRAS CORRECCIONES 0/1/9;
  - ICT21, sobre la v2.1: EMITIBLE 0/0/5.
- La historia completa está en `F4B-DRAFTS/pbtc/`.
- La bloqueante de IRT1 (B-01) era un hecho falso: el peralte IN/OUT base no es una pérdida, porque el ensamblador lo normaliza al único peralte del catálogo. Así que las pérdidas de (a) son **diez** campos.

**La vuelta al Owner** que prevén [P]:766, [OD]:153-155 y F4B-E05 se hizo con cinco preguntas, y sus respuestas fueron estas:
- **OD-PBTC-1:** «Corregir, regla Cantilever (Recomendado)»:
  - (a) se corrige con la regla `Keep` (R-03 (i));
  - vale también más allá de la ruta de carga: la selección, «Ambos lados», el alcance, los datos del frente y la biblioteca;
  - PBTC-C14 se mantiene.
- **OD-PBTC-2:** «Corregir (Recomendado)»: (b) se corrige.
- **OD-PBTC-3:** «Texto corto, franja de estado (Recomendado)»: el aviso de D-PB-08 va en `StatusContent`, detrás de `StatusText`, con el literal byte a byte del archivo de decisiones.
- **OD-PBTC-4:** «Restaurar estándar (Recomendado)» = (A). H-PB-28 entra en PB-TC como corrección declarada nueva, **D-PB-11**.
- **OD-PBTC-5:** el Owner no marcó «Otras cajas que redondean» = (B). El caso de H-31 (`GapBox`, `ModuleLengthBox` y las cuatro cajas avanzadas anulables) **no** entra en PB-TC: vuelve al Owner **antes del contrato de PB-T1** (OB-06.5), y PB-TC lo prohíbe de forma explícita (INV-PBTC-15 y PBTC-C10).

**Obligaciones de PB-T2** (M-07 de IRT1; precondición 3 del paquete):
- el paso 4 de D-PB-06 en el modo WORKSPACE;
- el aviso de D-PB-08 en el modo WORKSPACE.

[P] los asigna a PB-T2, y [OD]:144-147 pide probar los dos modos. El contrato de PB-T2 los incluye.

## F4B-E07. Orden del Owner de emitir PB-TC

**Fecha:** 2026-11-17 (noche). **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbtc-emission-order.md`.

- **La orden.** F4B-E05 («Siguiente», punto 4) exige la orden del Owner. El Owner respondió «Sí, emite PB-TC (Recomendado)».
- **Lo que se emite:**
  - el paquete v2.2, que es la v2.1 con los cinco menores de ICT21 aplicados sin otra comprobación, como ICT21 recomienda. Las sustituciones pasan a llamarse SUS-01 a SUS-04, para no chocar con las paradas estándar S-0n;
  - el contrato de `F4B-PBTC-DECLARED-CORRECTIONS`, con attempts 1 de 3.
- **El registro declarado** `F4B-DRAFTS/superseding-characterization.md` nace en estado DECLARADA, con SUS-01 a SUS-04 (OB-T1B.2), y pasa a EJECUTADA al cerrar.
- **Reglas de la cadena:** las de F4B-E03 y D-PB-10, transcritas en PBTC-C08, C11 y C12. PBTC-C14 vuelve al Owner antes del GREEN si el delta de la base de CT-PBTC-10 es material.

## F4B-E08. Cadena, decisión del Owner sobre OWNER-1 y cierre de F4B-PBTC-DECLARED-CORRECTIONS

**Fecha:** 2026-11-18. **Paquete de cierre:** `F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md`.

**La cadena del intento 1:**
- **Planificación:** `8db0` se rechazó solo por la copia. El invariante del índice 15, INV-PBTC-16, se copió sin «en f4b-dispositions.md»; A5 falló y la parada fue STOP P-03.
- **Replanificación única** de PBTC-C08 ([DP], pregunta 2): `7171` dio fidelidad PASS en las 8 colecciones y A1-A8 8/8. No gasta intento. nc4 `b7b5`.
- **Worker** `069b`, con `claude-sonnet-5-5` en effort high:
  - la medición previa de la base (D-PBTC-10) no activa PBTC-C14: F1 y M-02 solo cambian `PalletDepth`, con K1 a K4 vacíos;
  - RED `f665edac`: solo `tests/`;
  - GREEN `4440f67d`: solo W, WC y WX.
- **CI:**
  - RED `44223960356`: ui-tests en failure por los 10 métodos nuevos; tests, build-ui y build-plugin en success;
  - GREEN `44225456328`: 4/4. Las cifras son exactamente las del §6.9 del paquete.
- **Revisiones:**
  - la directa, conforme, con D-1 menor: el prefijo erróneo de las rutas de los artefactos en la entrega;
  - la adversarial independiente, **ACEPTABLE CON MENORES 0/0/5**.
- **Verificación** `a6f6`: `EXECUTION_VERIFIED` / NONE, 14/14.
- **Controles:** nc1 `2812` CONTROL NOT PASSED con una variación de la firma de PD-F4-04 (Remote, Scope, Tests y Trailer en fail además de Handoff, Identity y Ci, todas solo como consumidoras del CurrentSha falso), resuelto por el Owner «Aceptarlo solo esta vez (Recomendado)» (`F4B-DRAFTS/coordinator/owner-pbtc-nc1-decision.md`; `nc1 = SATISFIED_BY_OWNER_DISPOSITION`, sin sustituto y sin ampliar las firmas); nc2 `2db5` y nc3 `412a` CONTROL PASS ordinario.

**OWNER-1 y la decisión del Owner** (`F4B-DRAFTS/coordinator/owner-pbtc-mixed-decision.md`, literal):
- **El hallazgo:** M-01 de la revisión independiente es un hueco del diseño del paquete, no una desviación. En «Ambos lados», con una caja mixta, escribir el número que se ve del exacto de un lado no iguala los dos lados. Ese lado conserva su exacto y la caja vuelve a quedar mixta, con una diferencia menor que 0,0005.
- **La respuesta:** el Owner eligió «Registrar y arreglarlo luego (Recomendado)».
- **El efecto:** queda como limitación conocida, anotada en `superseding-characterization.md`. La propuesta de arreglo vuelve al Owner antes del contrato de PB-T1, junto con el caso H-31, porque es la misma regla `Keep`. No gasta intento.

**Resultado:** **F4B-PBTC-DECLARED-CORRECTIONS = CLOSED / PASS WITH LIMITATIONS** sobre `4440f67d`, con attempts 1 de 3.
- **Correcciones declaradas de RACKEDITAR:** D-PB-06 (a) y (b), D-PB-08 y D-PB-11.
- **Registro declarado:** `superseding-characterization.md` pasa a EJECUTADA, con SUS-01 a SUS-04.
- **Arrastres al paquete de PB-T1:** M-02 a M-05, según el §4 del paquete de cierre.

**Siguiente:**
1. Antes del contrato de PB-T1 (OB-06.5), la vuelta al Owner con:
   - el caso H-31 y OWNER-1;
   - `pb-21-cola` (OWNER-3 de PB-T0);
   - los casos de T-2.
2. PB-T1 no se emite sin la orden del Owner.

## F4B-E09. Decisiones del Owner antes del contrato de PB-T1 (OB-06.5) y autorización condicional de emisión

**Fecha:** 2026-11-18. **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md`.

**Las órdenes previas del Owner** (chat, literales):
- «Avanza todo lo que puedas, me refiero a que no contestaré lo que no sea necesario, no que no trabajes»;
- más tarde, «Ya dormiré, por lo que, tienes que avanzar por tu cuenta todo lo posible, si en necesario controla la computadora sin pedir autorización».

**El expediente** (`F4B-DRAFTS/pbt1/ob065-dossier.md`), de un analista independiente en solo lectura. Midió los casos de OB-06.5:
- H-31;
- OWNER-1;
- c-02;
- `pb-21-cola`;
- los demás de T-2.

También midió los huecos de red y las guardas de T1B (OB-T1B.3 no obliga a preguntar).

**La comprobación de la sesión antes de preguntar** (`F4B-DRAFTS/pbt1/scan_dormant_tail.py`, sobre los inventarios crudos de las copias del bloque M, sin AutoCAD): ningún rack real de un sentido guarda una cola. Los únicos `GAP` y `B:` son de un rack A/B con `SideB` presente.

**Las cuatro respuestas del Owner:**
1. **H-31 y OWNER-1:** «Corregir en PB-TC2 (Recomendado)». Va una tarea propia, **PB-TC2**, antes de PB-T1, con presupuesto de 1/3.
2. **c-02:** «Como hoy, como regla explícita (Recomendado)». PB-T1 reproduce la sustitución de un poste fuera de catálogo como regla explícita del estado. Los demás casos de T-2 se quedan como hoy.
3. **`pb-21-cola`:** «Dejarlo fuera de la garantía (Recomendado)». Un rack de un sentido con cola compuesta guardada no es un «supported classic rack» para la garantía sin pérdidas de PB-T1. El Workspace sigue rechazándolo en todo F4B.
4. **Emisión:** «Sí, emite con esa condición (Recomendado)». PB-TC2 y después PB-T1 se emiten sin volver al Owner si se cumplen dos condiciones: su revisión independiente da 0 bloqueantes y 0 importantes sin resolver, y no aparece ninguna decisión material nueva. Se vuelve al Owner por una decisión nueva, por un control con una firma no prevista, por un intento 3 o por un efecto material sin medir.

**Obligaciones que se anotan:**
- **PB-T2:** se mantienen las de F4B-E06: el paso 4 de D-PB-06 y el aviso de D-PB-08 en modo WORKSPACE.
- **El paquete de PB-T1** recibe:
  - M-07 de la revisión de PB-TC2 (la caja de largo manual que no se vacía en un rack de un sentido con un indicador mixto obsoleto);
  - los arrastres M-02 a M-05 de PB-TC;
  - la regla explícita de c-02;
  - la exclusión de `pb-21-cola`.

## F4B-E10. Emisión de PB-TC2 por la autorización condicional de F4B-E09

- **El paquete:** el de PB-TC2, `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md` v2.
- **Su revisión:**
  - la independiente IRT2-1 de la v1 dio EMITIBLE TRAS CORRECCIONES 0/3/7;
  - la comprobación acotada de la v2 dio EMITIBLE 0/0/3, con N-01 a N-03 aplicados en el texto de emision y ninguna decision nueva.
- **Las condiciones de la pregunta 4** se cumplen: 0 bloqueantes, 0 importantes sin resolver y ninguna decisión material nueva.
- **Lo que se emite:** el contrato de `F4B-PBTC2-DECLARED-CORRECTIONS`, con attempts 1 de 3.
- **El registro** `superseding-characterization.md` añade la sección de PB-TC2 en estado DECLARADA, sin ninguna sustitución. Cierra el «Pendiente» de H-31 y la limitación OWNER-1, que pasan a PB-TC2.

## F4B-E11. Decisiones D1 y D2 del Owner / Coordinator

**Fecha:** 2026-11-18. **Texto literal:** `F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`.

Son las respuestas a las dos vueltas que la sesión preparó en modo autónomo: `F4B-DRAFTS/pbtc2/owner-return-c14.md` y `F4B-DRAFTS/pbt1/owner-return-l1.md`.

**D1 = «CONTINUE EXACT-VALUE FIX»,** sobre PB-TC2, por PBTC2-C14.
- Ratifica que el BOM distinga cantidades con valores exactos distintos (32 @ 150 y 4 @ 150.0004).
- Prohíbe volver a redondear para conservar la agrupación anterior, y prohíbe inventar una tolerancia dentro de PB-TC2.
- Ordena reanudar el mismo trabajo **sin gastar intento** y sin nueva planificación, y conservar la parada y la decisión como evidencia.

**D2 = «ACCEPT TWO-REFERENCE ALIAS EXCEPTION»,** sobre PB-T1, L-1.
- La copia del estado puede compartir **solo** el rack resuelto y el informe de módulos, con **10 condiciones de seguridad** que PB-T1 tiene que probar.
- **El límite:** si PB-T1, PB-T2 o PB-T3 tuvieran que mutar uno de los dos dentro del tiempo de vida del borrador, STOP y se reabre L-1.
- La división de PB-T1 en tres subtareas queda aceptada en principio. Se vuelve a medir sobre el producto final de PB-TC2, y PB-T1a se emite «normally» si no cambia ninguna premisa material.

## F4B-E12. Cierre de F4B-PBTC2-DECLARED-CORRECTIONS

**Paquete de cierre:** `F4B-PBTC2-DECLARED-CORRECTIONS/task-close-package.md`.

**La cadena del intento 1:**
- **Planificación:** `6ac2` se rechazó solo por la copia: ExpectedEvidence[1] cortada; en ForbiddenWriteScope, un nombre cambiado y una entrada repetida. A4 dio fail y la parada fue STOP P-03.
- **Replanificación única** de PBTC2-C08: `c73e` dio fidelidad PASS en las 8 colecciones y A1-A8 8/8. nc4 `6700`.
- **Worker `38d3`:** parada **prevista** PBTC2-C14 en la medición previa (fila T2-3: el BOM de postes, partido), sin commits.
- **Owner D1** (F4B-E11): sigue, sin gastar intento.
- **Worker `037c`**, la reanudación de la misma delegación:
  - la medición `m8b` es idéntica a la de `38d3`;
  - RED `3677847c`, con los 4 métodos nuevos;
  - GREEN `b5c09f0b`, solo en W y WC.
- **CI:**
  - RED `44298811020`: ui-tests en failure;
  - GREEN `44300874797`: 4/4, con UI 2 401/21/2 422 y Core 19 070, exactamente lo previsto.
- **Revisiones:**
  - la directa, conforme;
  - la independiente, **ACEPTABLE CON MENORES 0/0/4**, con IRC-O01 informativo. La línea del BOM ratificada en D1 sale con la descripción larga del catálogo para el mismo perfil. Ya ocurría sin tocar el panel, y cae en las reglas de presentación existentes que D1 admite.
- **Verificación** `a2c6`: `EXECUTION_VERIFIED` 14/14, con D1 como disposición de entrada.
- **Controles:** nc1 `1d1d` CONTROL NOT PASSED (Handoff, Identity y Ci; Remote en pass); el Owner dijo «Es la firma exacta: aplica el sustituto (Recomendado)», pero el sustituto PBTC2-NC1-IDENTITY-SIGNAL-01 dio FAIL por la propiedad 5 (la evidencia de Identity no escribe el SHA falso); el Owner dijo «Repetir nc1; si falla, aceptarlo (Recomendado)»; nc1 repetido `d6fd` CONTROL NOT PASSED (además Remote); queda `nc1 = SATISFIED_BY_OWNER_DISPOSITION` solo para este caso (`F4B-DRAFTS/coordinator/owner-pbtc2-nc1-decisions.md`), sin cambiar las firmas; nc2 `683c` y nc3 `efea` CONTROL PASS ordinario.

**Resultado:** **F4B-PBTC2-DECLARED-CORRECTIONS = CLOSED / PASS WITH LIMITATIONS** sobre `b5c09f0b`, con attempts 1 de 3.
- **Correcciones declaradas de RACKEDITAR:** H-31 (seis cajas con la regla `Keep`) y OWNER-1 (la caja mixta de «Ambos lados» con B presente).
- **Registro:** la sección de PB-TC2 de `superseding-characterization.md` pasa a EJECUTADA. Cierra el «Pendiente» de H-31 y la limitación OWNER-1.

**Siguiente:**
- volver a medir el paquete de PB-T1 v2.2 sobre `b5c09f0b`, con una comprobación acotada;
- emitir PB-T1a por la autorización condicional (F4B-E09 y F4B-E11).

## F4B-E13. Paquete del contrato de PB-T1, remedición sobre PB-TC2 y emisión de PB-T1a

**Fecha:** 2026-11-18. **El paquete:** `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.4, con su registro de mediciones `docs/initiatives/I-64-f4b-pbt1-measurements.md` (apartados A a O). La historia, los encargos, los scripts de medición y las revisiones están en `F4B-DRAFTS/pbt1/`.

**La autoridad de la emisión:**
- F4B-E09, pregunta 4: la autorización condicional de emitir PB-TC2 y después PB-T1 sin volver al Owner.
- F4B-E11, D2 y «PB-T1 PLAN»: «Re-measure it against the final PB-TC2 product before emission. If that remeasurement changes no material premise: issue the first PB-T1 task normally.»

**La división (D-PBT1-01),** aceptada en principio por el Owner en F4B-E11. Son tres subtareas secuenciales, cada una con su cadena y su presupuesto de 1 de 3 (D-PB-10):
- **PB-T1a, `F4B-PBT1A-LOSSLESS-STATE-NET`:** solo pruebas de caracterización (CT-PBT1A-01 a 14), con un RED por los quince mutantes revertidos de la tabla cerrada. El producto no cambia.
- **PB-T1b, `F4B-PBT1B-LOSSLESS-STATE-TYPE`:** `PushBackEditableState` y `PushBackEditorOutputGate` en Application (CT-PBT1B-01 a 16).
- **PB-T1c, `F4B-PBT1C-LOSSLESS-STATE-WINDOW`:** la ventana sobre el estado, con reenvíos del mismo nombre (CT-PBT1C-01 a 04).

**D2, incorporada (D-PBT1-15):**
- **Las diez condiciones de seguridad:** se reparten entre CT-PBT1B-14, 15 y 16, CT-PBT1C-02 y 04, la guarda G-L1 y el invariante INV-PBT1x-14.
- **El límite:** es la parada PBT1x-C17 en cada contrato, con el texto literal del Owner.
- **Lo que heredan PB-T2, PB-T3 y PB-T4:** el arrastre A-2, con la tabla por condición del §10 del paquete.
- **La lectura L-5** extiende el límite a PB-T4 y a la vida de edición clásica. La comprobación acotada de la remedición la juzgó una lectura legítima y no una decisión nueva.

**Las revisiones:**
- **IRT1-1, de la v1:** EMITIBLE TRAS CORRECCIONES 0/8/15. Planteó una decisión del Owner, L-1, que el Owner resolvió con D2.
- **La comprobación acotada de la v2:** EMITIBLE TRAS CORRECCIONES 0/1/10. La sesión corrigió el importante, N-I-01: la pregunta de L-1 estaba sesgada.
- **La comprobación acotada de la remedición (v2.3):** EMITIBLE TRAS CORRECCIONES 0/1/3, sin decisiones nuevas. Sus cuatro hallazgos están aplicados en la v2.4.
  - El importante, V23C-I01, amplía G-L1 en PB-T1b y PB-T1c (indexadores, moldes `as`/`is`, lambdas y la regla 1c nueva). No toca el contrato de PB-T1a.

**La remedición sobre el producto final de PB-TC2** (§8.4 del paquete; registro N). Se hizo en `cd47523b`, con el producto de su GREEN `b5c09f0b`, en solo lectura y sin ningún Worker en el worktree.
- Cubrió R-1 a R-13 y lo que añade el cierre de PB-TC2.
- **Ninguna diferencia es material** según L-6: todas son anclas, recuentos o textos, y la tabla del §8.4 clasifica cada una.
- **Las premisas de D2 se sostienen:** G-L1 da 0 coincidencias sobre la base, y no hay un cuarto escritor de `Name` en Push Back.

**Los anclajes, medidos de nuevo al emitir** (§6.0). La sesión volvió a ejecutar `remeasure_anchors.py` sobre la base de la emisión, y su salida queda custodiada junto al contrato. El generador del contrato comprueba además con sus `assert` los quince textos de la tabla de mutantes.

**Las condiciones de emisión se cumplen:** 0 bloqueantes, 0 importantes sin resolver y ninguna decisión material nueva.

**Lo que se emite:** el contrato de `F4B-PBT1A-LOSSLESS-STATE-NET`, con attempts 1 de 3:
- 9 rutas permitidas: W y WC, solo para los mutantes del RED, y los siete archivos nuevos `PushBackStateNet*`;
- 131 rutas prohibidas;
- 14 invariantes y 30 paradas, con las de [DP] literales, C04 y C17;
- 19 filtros. `ExpectRed` está solo en `PushBackStateNet`, con `MinSelected` 14.

**Las vueltas al Owner que quedan previstas:**
- **Antes de emitir PB-T1b:** la lista de INV-PBT1A-13, si no está vacía (precondición 5).
- **Antes de PB-T2:** D-PB-07.
- **Antes de PB-T3:** la `ExtensionData` anidada (A-4).
- **Siguen las de F4B-E03:** una sola replanificación, y solo por la comparación literal; los sustitutos de nc1 y nc2, solo con la firma exacta de PD-F4-04 y PD-F4-01.
- **Ninguna aceptación puntual se extiende a PB-T1:** ni la de nc1 en PB-TC (F4B-E08) ni la de PB-TC2 (F4B-E12).

**Los arrastres del §10:**
- **A-1:** el modo WORKSPACE y lo demás de PB-T2.
- **A-2:** el borrador sobre el estado, con las obligaciones de D2.
- **A-3:** el rechazo A/B del piloto, en PB-T3 o PB-T4.
- **A-4:** la `ExtensionData` anidada.
- **A-5:** la copia de la carga del piloto.
- **A-6:** M-07, la caja de c-02 y M-01 de PB-T0, que van a `docs/ideas-futuras.md`.

## F4B-E14. Decisiones del Owner tras el Worker de PB-T1a

**Fecha:** 2026-11-18. **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbt1a-decisions.md`. **La vuelta:** `F4B-DRAFTS/pbt1a/owner-return-pbt1a.md`, preparada con la revisión directa y con la independiente (REWORK 0/1/9).

1. **CT-02** (I-01): «Ratificar (Recomendado)».
   - El Worker añadió pasos a CT-02 durante la medición, antes de las pruebas definitivas, para que MU-T1A-02 discriminara: la fila suponía que «lb» llega sola a la salida, y no llega.
   - El contrato mandaba parar (PBT1A-C04). El Owner lo ratifica **solo para este caso**, como c-16 de PB-T0 (F4B-E05). La regla SIN LATITUD y PBT1A-C04 no cambian.
   - La revisión independiente pasa a ACEPTABLE CON MENORES 0/0/9.
2. **c-09 al reconstruir** (OWNER-2): «Como hoy, regla explícita (Recomendado)».
   - Un rack guardado en «ton» pasa a «kg», con el mismo número, cuando otra edición reconstruye la estructura: `WeightUnitBox` decide el estado.
   - PB-T1b lo reproduce como una regla explícita del estado con su CT, igual que c-02 (F4B-E09, pregunta 2). RACKEDITAR no cambia.
3. **Los demás hallazgos de INV-PBT1A-13** (OWNER-3 y OWNER-4): «Tal cual, anotar mejoras (Recomendado)».
   - Los dos hallazgos:
     - «lb» y `PalletsDeepBox` no llegan solos a la salida;
     - aceptar Seguridad sin cambios añade el portador de botas.
   - PB-T1 los reproduce, y se anotan en `docs/ideas-futuras.md`.
   - Con las preguntas 2 y 3, la precondición 5 de PB-T1b queda satisfecha.

## F4B-E15. Decisión del Owner sobre los controles de PB-T1a

**Fecha:** 2026-11-18. **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbt1a-nc1-decisions.md`. **La vuelta:** `F4B-DRAFTS/pbt1a/owner-return-controls.md`.

**La primera ronda:** los tres controles sobre la verificación `06b0` dieron CONTROL NOT PASSED.
- **nc1** `8b4e`: detecta el SHA falso, pero la evidencia de Identity no lo nombra (no es la firma exacta de PD-F4-04).
- **nc2** `996a`: no detecta el `AllowedWriteScope` estrechado.
- **nc3** `bdb7`: no detecta « GATE PASS.» al final de `WorkCompleted` (no ejecutó ninguna búsqueda).

Son falsos negativos del verificador con una entrega grande. Por F4B-E03 y PBT1A-C11, ninguno tenía salida sin el Owner.

**Respuesta del Owner:** «Reforzar y repetir (Recomendado)».
- **El refuerzo:** el prompt de la verificación gana tres comprobaciones deterministas obligatorias:
  - FreeText por script, sobre el texto completo de los siete campos libres;
  - Scope, contado por script contra el `AllowedWriteScope` de la delegación;
  - Identity, con el `CurrentSha` de la entrega y el HEAD medido escritos literalmente.
- **La repetición:** se repiten una vez la verificación y nc1 a nc3.
- **El alcance:** vale también para PB-T1b y PB-T1c. No gasta intento.

**La segunda ronda:**
- **Verificación** `b71b`: `EXECUTION_VERIFIED` 14/14, con las tres comprobaciones por script.
- **Controles,** los tres **CONTROL PASS** ordinarios, con el oráculo cumplido y sin sustitutos:
  - nc1 `3203`: Identity primero; Remote y Ci como consumidores del SHA falso; STOP S-04;
  - nc2 `77ec`: Scope en fail;
  - nc3 `7969`: FreeText en fail.

## F4B-E16. Cierre de F4B-PBT1A-LOSSLESS-STATE-NET

**Paquete de cierre:** `F4B-PBT1A-LOSSLESS-STATE-NET/task-close-package.md`.

**La cadena del intento 1:**
- **Planificación** `d049`, en el primer lanzamiento: fidelidad PASS en las 8 colecciones y A1-A8 8/8. nc4 `7228`.
- **Worker** `d31d`: IMPLEMENTATION_COMPLETE, sin paradas.
  - RED `15f1f225`: los siete archivos nuevos y los quince mutantes de la tabla, exactamente.
  - GREEN `73d30982`: la reversión; `git diff a3b63ccc 73d30982 -- src` vacío.
  - Cada mutante aislado hace fallar el CT de su fila.
- **CI:**
  - RED `44333116533`: ui-tests en failure (`PushBackStateNet` 14/14 por aserción; 42 pruebas existentes atribuidas a los mutantes); los otros tres trabajos en success.
  - GREEN `44338475965`: 4/4, con UI 2 415/21/2 436 y Core 19 070, exactamente lo previsto.
- **Revisiones:**
  - la directa, conforme;
  - la independiente, REWORK 0/1/9, que pasa a ACEPTABLE CON MENORES 0/0/9 con F4B-E14.
- **Verificación** `06b0`: `EXECUTION_VERIFIED` 14/14, con la ratificación de CT-02 como disposición de entrada; la verificación reforzada `b71b` (F4B-E15), también 14/14.
- **Controles:** primera ronda sobre la verificación `06b0`: nc1 `8b4e`, nc2 `996a` y nc3 `bdb7` CONTROL NOT PASSED (falsos negativos del verificador); el Owner dijo «Reforzar y repetir (Recomendado)» (F4B-E15); segunda ronda sobre la verificación reforzada `b71b` (`EXECUTION_VERIFIED` 14/14): nc1 `3203`, nc2 `77ec` y nc3 `7969` **CONTROL PASS** ordinario, sin sustitutos.

**Resultado:** **F4B-PBT1A-LOSSLESS-STATE-NET = CLOSED / PASS WITH LIMITATIONS** sobre `73d30982`, con attempts 1 de 3.
- La red de caracterización de la ventana clásica queda fijada con catorce CT y quince mutantes.
- Sin cambios de producto.

**Anotado en `docs/ideas-futuras.md`:**
- A-6 de PB-T1: M-07, la caja de c-02 que se ve vacía y M-01 de PB-T0;
- OWNER-3 y OWNER-4 de F4B-E14;
- c-09 como mejora futura de producto.

**Siguiente:**
- los anclajes de PB-T1b sobre este cierre y su generador, con la regla explícita de c-09 (F4B-E14, pregunta 2);
- su comprobación acotada;
- su emisión, por la autorización de F4B-E09 y F4B-E11.
