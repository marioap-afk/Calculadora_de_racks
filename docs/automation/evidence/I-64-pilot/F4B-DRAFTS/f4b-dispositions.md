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

## F4B-E17. Paquete de PB-T1 v2.6 y emisión de PB-T1b

**Fecha:** 2026-11-18.

**El paquete:** `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.6, que sustituye a la v2.4 custodiada. La v2.4 y la v2.5 revisada pasan a la historia (`F4B-DRAFTS/pbt1/package-v24.md` y `package-v25.md`). Su registro de mediciones gana el apartado P.

**La autoridad de la emisión:**
- F4B-E09, pregunta 4 (la autorización condicional);
- F4B-E11 (D2 y «PB-T1 PLAN»);
- F4B-E14, pregunta 2: c-09 al reconstruir, «Como hoy, regla explícita».

**Lo que cambia en la v2.5 y la v2.6:**
- **c-09,** la decisión del Owner, entra como regla explícita del estado (D-PBT1-09b, `ResolveWeightUnit` en `AdoptCarriers`):
  - su CT, CT-PBT1B-17, con las líneas literales de CT-PBT1A-02;
  - la fila (f) del paso 1, con dos ramas fijadas y la parada PBT1B-C14, como c-02;
  - HT1-15 y el mapa de controles quedan corregidos: `WeightUnitBox` decide la unidad al reconstruir.
- **El filtro `PushBackEditableState`** pasa de 12 a 13.
- **Los `Objective`** de PB-T1b y PB-T1c citan la v2.6.
- **La verificación** lleva las tres comprobaciones deterministas de F4B-E15.
- **OWNER-3 y OWNER-4** (F4B-E14, pregunta 3) se reproducen tal cual, sin regla propia.

**Los anclajes de PB-T1b** (§6.0, precondición 4), medidos sobre `5043b49a`, el cierre de PB-T1a (registro P):
- `src/` no cambia desde `b5c09f0b`;
- las 104 anclas siguen en su línea y su miembro;
- G-L1 da 0 coincidencias y sus autopruebas saltan;
- los recuentos y la línea base de la CI (`44361363873`: UI 2 436 / 21 / 2 415; Core 19 070) coinciden con la previsión.

La única diferencia material es c-09 (V25-06), y ya está decidida por el Owner.

**La precondición 5** (la autoridad de PB-T1a) queda satisfecha por F4B-E14.

**La comprobación acotada** de la v2.5 (`F4B-DRAFTS/pbt1/independent-review-pbt1b-check.md`): EMITIBLE TRAS CORRECCIONES 0/1/5, sin decisiones nuevas para el Owner; sus seis hallazgos, aplicados en la v2.6.
- **PBT1BC-I01:** a CT-PBT1B-17 le faltaba la línea que fija de punta a punta la rama de c-09 con la caja en «lb». La v2.6 la añade, con lo esperado tomado de la fila (f) del paso 1.
- **M01 a M05** también están aplicados en la v2.6:
  - M01: la fila (f) carga el diseño y se comprueba a sí misma;
  - M02: la razón del RED de CT-16;
  - M03: las autoridades de F4-T2 y F4-T3;
  - M04: la custodia de la comprobación del esqueleto y de la salida en seco;
  - M05: precisiones del texto.
- Como en la remedición de PB-T1a, se corrige en el texto antes de custodiarlo, sin volver al Owner.

**Lo que se emite:** el contrato de `F4B-PBT1B-LOSSLESS-STATE-TYPE`, con attempts 1 de 3:
- 12 rutas permitidas: ES y RMES, los cuatro archivos nuevos de Application y las seis clases de prueba nuevas;
- 213 rutas prohibidas;
- 15 invariantes y 33 paradas;
- 23 filtros, con `ExpectRed` en `PushBackEditableState` (13), `PushBackEditorOutputGate` (1) y `PushBackEditableStateParityTests` (1).

**El GREEN previsto:** Core 19 086 y UI 2 437.

## F4B-E18. Decisión del Owner sobre la planificación de PB-T1b (PBT1B-C08)

**Fecha:** 2026-11-18. **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbt1b-planning-decision.md`. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-planning.md`.

**La planificación** `R20261118T184831Z-aaac` se rechazó solo por la copia, en tres colecciones:
- ForbiddenWriteScope sin una entrada;
- en RequiredTests, un `Project` cambiado;
- en Authorities, un identificador de CT cambiado.

**La replanificación única** `R20261118T185409Z-c45c` se rechazó otra vez solo por la copia. ForbiddenWriteScope salió con 196 de las 213 entradas: faltaban las 17 de la raíz. Las otras siete colecciones eran idénticas.

**La respuesta del Owner:** «La sesión pone la lista (Recomendado)».
- **Solo en esa delegación,** la sesión sustituyó el ForbiddenWriteScope por la lista exacta del contrato. La salida del Controller y su comparación rechazada se conservan.
- **Sobre la delegación reparada:** comparación byte a byte PASS en las ocho colecciones, A1-A8 8/8 y nc4 `R20261118T190107Z-0764` con el oráculo cumplido.
- **El alcance:** no gasta intento y no cambia la regla para otras tareas.

## F4B-E19. Decisión del Owner sobre el config.toml tras el reinicio: Snapshot J

**Fecha:** 2026-11-18. **Texto literal:** `F4B-DRAFTS/coordinator/owner-config-snapshot-j-decision.md`. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-config.md`.

**La pausa y el reinicio.** El Owner pidió pausar todo para reiniciar el equipo. La sesión detuvo el Worker `R20261118T190205Z-6d33` después de sus pasos 0 y 1, sin commits.

**Lo que cambió con el reinicio:**
- la app de Codex se actualizó de 26.1111.21308 a 26.1111.2228.0 y reescribió `config.toml`;
- el nuevo hash es `57B0FED6…`, distinto de la Snapshot I;
- los 104 nombres y los 68 valores del inventario seguro son idénticos;
- el único valor que el inventario oculta es el directorio opaco de la tubería de computer use;
- el binario acreditado no cambió.

Se detectó como P-01 al preparar la reanudación (`R20261118T191840Z-7115`, que no se lanzó).

**La respuesta del Owner:** «Sonda neutra, luego J (Recomendado)».

**La sonda** `R20261118T193210Z-d32c` dio **Pass**:
- el oráculo se cumple, con `gpt-6-luna`/high;
- el sandbox es de solo lectura y la red está restringida;
- la CLI, el binario y la configuración no cambiaron durante la corrida.

Frente a la corrida `c45c` (Snapshot I, hoy) solo cambia el `root_turn_id`. El registro de la sonda de referencia 3082 ya solo existe comprimido, y la comparación usa `c45c`.

**El resultado:** la **Snapshot J** es la línea base activa del Controller en I-64 y sustituye a la I. Su registro está en `F4-ENVIRONMENT/snapshot-J/snapshot-j.json`.

## F4B-E20. Decisiones del Owner sobre la parada PBT1B-C15 y la reemisión de PB-T1b

**Fecha:** 2026-11-18. **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbt1b-c15-decisions.md`. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-c15.md`.

**La parada.** El Worker reanudado `R20261118T193423Z-fb01` repitió el paso 1 con las mismas líneas (c-02 en la rama ordinal y c-09 en la rama «conserva»). Después paró con PBT1B-C15 antes del RED, sin commits:
- la mitad estado → diseño → estado de CT-PBT1B-02 da «distintos» en `pb-15-sin-composite`;
- la diferencia es que el frente 0 del lado B pasa de activo a inactivo, con B no presente;
- es la normalización que ya registra `LegacyDeltas`: `AbsentSlotsB[0]: <absent> -> 0`.

Su lectura de CT-06 dio falsos fallos.

**Las respuestas del Owner:**
- «Normalización conocida (Recomendado)»: no es una pérdida, y se corrige CT-02;
- «Sin gastar intento (Recomendado)»: la delegación nueva sigue en el intento 1 de 3.

**El paquete v2.7:**
- **CT-PBT1B-02** tiene tres pasos por diseño:
  - (i) diseño → estado → diseño, con el oráculo de siempre;
  - (ii) la primera vuelta vista desde el estado: iguales en 17 diseños; en pb-15, solo el frente 0 de B;
  - (iii) estado → diseño → estado desde el estado normalizado: iguales en los 18.
- **CT-PBT1B-06** define «sobrevive a `Copy`» con el `Mutator` del piloto, como CT-08. Excluye, con su ancla, las ranuras que `Restore` vuelve a alinear y `SafetyFacePiece`, que es inmutable.
- **PBT1B-C15** nombra la normalización de pb-15 como excepción.

**La comprobación acotada de la v2.7:** EMITIBLE 0/0/4, sin decisiones nuevas para el Owner; sus cuatro menores, aplicados en el texto de emision de la v2.7.

**Lo que se emite de nuevo:** el contrato de `F4B-PBT1B-LOSSLESS-STATE-TYPE`, con la v2.7. Sigue en el intento 1 de 3, y la delegación es nueva.

## F4B-E21. Decisión del Owner: la sesión pone las colecciones opacas del contrato (desde PB-T1b, en el resto de F4B)

**Fecha:** 2026-11-18. **Texto literal:** `F4B-DRAFTS/coordinator/owner-f4b-opaque-collections-decision.md`. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-planning2.md`.

**Por qué:** el contrato de PB-T1b reemitido con la v2.7 mide unos 56 KB y ya no cabe en la salida del Controller.
- La planificación `R20261118T203710Z-4000` dejó colecciones enteras vacías.
- La replanificación única `R20261118T204045Z-a60d` las dejó todas vacías y lo declaró así: «excede el limite de salida disponible y se trunca».
- Por PBT1B-C08, ese segundo rechazo volvió al Owner.

**Respuesta del Owner:** «La sesión copia; resto de F4B (Recomendado)».
- **Lo que deja de copiar el Controller:** las ocho colecciones opacas. Las emite vacías.
- **Lo que pone la sesión:** esas colecciones, byte a byte desde el contrato (`inject_opaque.py`). Se conservan la salida del Controller y la delegación final.
- **Lo que no cambia:** la comparación byte a byte, A1-A8 y nc4 se hacen sobre la delegación final.

**La primera aplicación:** la planificación `R20261118T204434Z-6063` dio fidelidad PASS en las ocho colecciones, A1-A8 8/8 y nc4 `R20261118T204611Z-3412` con el oráculo cumplido.

## F4B-E22. Decisiones del Owner tras la revisión independiente de PB-T1b (intento 1): corrección en el intento 2

**Fecha:** 2026-11-18. **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbt1b-rework-decisions.md`. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-rework.md`.

**El intento 1:** Worker `R20261118T204715Z-a4e2`.
- RED `2d9febc6`: el esqueleto y las seis clases de prueba.
- GREEN `a0a5f699`: solo `src/` de Application.
- CI 4/4 con las cifras previstas: Core 19 086 y UI 2 437.
- pb-15 da exactamente la normalización conocida.
- **Revisión directa:** conforme.
- **Revisión independiente:** **REWORK 0/2/15**. El producto está bien y D2 se respeta. Los dos importantes son defectos de pruebas que venían del borrador heredado:
  - I01: la guarda G-L1 no aplica la regla 1b a los alias;
  - I02: un caso de CT-10 no puede fallar.

**Las respuestas del Owner:**
1. «Corregir en el intento 2 (Recomendado)». Una delegación de corrección, con `FailureClass` `Tests` y el análisis en `F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204715Z-a4e2/analysis.md`.
   - Arregla I01, I02 y los menores de pruebas M01, M02, M04, M06, M07, M08 y M13.
   - El RED vuelve a poner el esqueleto (§6.0, M-11), y el GREEN restaura la misma implementación.
   - Los menores de producto (M09, M11, M12 y M15) se declaran o pasan como arrastres.
   - El intento 3, si llegara, vuelve al Owner.
2. «Ratificar, solo PB-T1b (Recomendado)».
   - M10: los ayudantes privados del comparador y del mapa fuera de la lista de INV-PBT1B-03.
   - M14: `build-plugin` omitido en la CI del RED, frente a lo que dice ExpectedEvidence[2].
   - Se pasan como disposición a la verificación, y el paquete de PB-T1c corrige el texto.

**El presupuesto:** PB-T1b pasa al **intento 2 de 3**. Las paradas anteriores no gastaron intento (F4B-E20).

## F4B-E23. Decisión del Owner: D-PB-07, BOM y biblioteca en el modo WORKSPACE

**Fecha:** 2026-11-19. **Texto literal:** `F4B-DRAFTS/coordinator/owner-f4b-dpb07-extensiondata-decisions.md`, pregunta 1. **La vuelta:** `F4B-DRAFTS/pbfull/owner-return-dpb07.md`, medida por lectura de objetos git en `2f5d3dec`.

**Respuesta del Owner:** «BOM y biblioteca (Recomendado)».
- En el modo WORKSPACE, el editor ofrece «BOM», con su exportación, y «Guardar en biblioteca».
- Insertar y el lote de vistas siguen ocultos (D-PB-01, OB-01.6). RACKEDITAR no cambia.
- **El contrato de PB-T2 lleva:**
  - dos costuras de prueba (la ventana del BOM y el diálogo de guardado), con S-7, S-9 y CT-22 en verde;
  - la prueba UI-STA de OB-01.3 con BOM y biblioteca, que comprueba:
    - que no hay petición clásica;
    - que el borrador no cambia hasta «Aplicar al borrador»;
    - que el GUID del borrador no cambia;
    - que el archivo de biblioteca conserva los campos desconocidos del rack.
- **Se aceptan** dos salidas a disco fuera del borrador, que pueden reflejar valores no aplicados.

## F4B-E24. Decisión del Owner: la ExtensionData anidada y el paso 6 de D-PB-05 (antes de PB-T3)

**Fecha:** 2026-11-19. **Texto literal:** `F4B-DRAFTS/coordinator/owner-f4b-dpb07-extensiondata-decisions.md`, pregunta 2. **La vuelta:** `F4B-DRAFTS/pbfull/owner-return-extensiondata.md`.

**Respuesta del Owner:** «Basta así (Recomendado)».
- PB-T3 sigue.
- El panel mantiene el rechazo con aviso de AUTH-13 (I-58) para todo campo desconocido a cualquier nivel, y el contrato de PB-T3 lo fija con una prueba.
- La pérdida silenciosa de RACKEDITAR («Actualizar» y «Guardar en biblioteca» con el lector tolerante) queda declarada como **limitación conocida anterior a F4B**, sin tocar el clásico (D-16, INV-19). Queda anotada en `docs/ideas-futuras.md`.

## F4B-E25. Decisión del Owner: las rutas UNIT_CHANGE en la base de una corrección (AUTOMATION_PLAN 16.3), regla de F4B

**Fecha:** 2026-11-19. **Texto literal:** `F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md`. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-verify-stop.md`.

**Por qué:** la verificación `R20261119T014757Z-c78a` del intento 2 de PB-T1b dio EXECUTION_BLOCKED / STOP con FailureClass Authority.
- `git diff --name-only 5e52aaa3 2f5d3dec` contiene ES y RMES, que son rutas UNIT_CHANGE del contrato. Solo las cambiaron el RED `2d9febc6` y el GREEN `a0a5f699` del Worker del intento 1.
- Para la corrección, la sesión no reemitió el contrato, y 16.3 no prevé este caso.
- Las otras 12 comprobaciones pasaron.
- Scope falló por un control extra de la sesión: el diff `ChainBaseSha..CurrentSha` incluye los commits de documentación de la sesión. Es un error de la sesión, corregido en la reverificación.

**Respuesta del Owner:** «Regla F4B y reverificar (Recomendado)».
- **La regla, para las correcciones de F4B:** una ruta UNIT_CHANGE de `git diff --name-only AuthorityRevision BaseSha` no incumple 16.3 si se cumplen las tres condiciones siguientes. El Controller las comprueba commit a commit.
  1. Todo commit del rango que la toca es el RedSha o el CurrentSha de una entrega de un Worker de la misma cadena.
  2. La ruta está en el AllowedWriteScope de la delegación de esa entrega.
  3. Ningún commit de la sesión la toca.
- Las rutas EXTERNAL siguen prohibidas sin excepción.
- **La aplicación:** la reverificación `R20261119T020026Z-dc81` dio EXECUTION_VERIFIED 14/14. No gastó intento ni cambió la entrega.

## F4B-E26. Cierre de F4B-PBT1B-LOSSLESS-STATE-TYPE

**Paquete de cierre:** `F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md`.

**La cadena:**
- **v2.6** (F4B-E17):
  - planificación `aaac` y replanificación única `c45c`, reparada por la sesión (F4B-E18), con nc4 `0764`;
  - Worker `6d33`, detenido por la pausa del Owner;
  - `7115`, que no se lanzó;
  - Worker `fb01`, parado con PBT1B-C15 en pb-15 (F4B-E20, sin gastar intento).
- **La Snapshot J** (F4B-E19): sonda neutra `d32c` en PASS.
- **v2.7:**
  - las planificaciones `4000` y `a60d` se rechazaron por el límite de salida del Controller (F4B-E21);
  - la planificación `6063` se aceptó con las colecciones opacas puestas por la sesión, con nc4 `3412`.
- **Intento 1:** el Worker `a4e2` dejó el producto conforme, con REWORK 0/2/15 de pruebas (F4B-E22).
- **Intento 2 (la corrección):**
  - planificación `e354`, con nc4 `6aa3`;
  - Worker `4c04`, con la interrupción de transporte 1 de 2 reanudada en la misma invocación;
  - RED `126fd65f`: el esqueleto devuelto y las pruebas corregidas;
  - GREEN `a18ba345`: la implementación restaurada, con `git diff 2f5d3dec a18ba345 -- src` vacío.

**CI:**
- RED `44430615074`: tests y ui-tests en failure, build-ui en success y build-plugin omitido (M14);
- GREEN `44438060317`: 4/4, con Core 19 086 y UI 2 437 (2 416 pasadas y 21 omitidas).

**Revisiones del intento 2:**
- la directa, sin hallazgos;
- la independiente, ACEPTABLE CON MENORES 0/0/5 (PBT1B2-IR-M01 a M05: evidencia y alcance de pruebas, sin producto).

**Verificación:** `c78a`, STOP Authority (F4B-E25); la reverificación `dc81`, EXECUTION_VERIFIED 14/14.

**Controles:** sobre la reverificación `dc81`: nc1 `a4e8` **CONTROL PASS** (Identity, STOP), nc2 `448d` **CONTROL PASS** (Scope, STOP) y nc3 `2049` **CONTROL PASS** (FreeText, REWORK), los tres con el oráculo cumplido y sin sustitutos. El registro de relevo de nc3 cuenta una coincidencia de «limit reached», que es el texto de la parada S-11 del README («repeated rework limit reached»), no un límite de uso.

**Resultado:** **F4B-PBT1B-LOSSLESS-STATE-TYPE = CLOSED / PASS WITH LIMITATIONS** sobre `a18ba345`, con attempts 2 de 3.
- `PushBackEditableState`, el copiador, el comparador, el mapa de propiedad y `PushBackEditorOutputGate` quedan en Application, con c-02 y c-09 explícitas y D2 respetada.
- La ventana no cambia: es de PB-T1c.

**Arrastres:**
- **A PB-T2 y PB-T4,** los de la revisión del intento 1: M09, M11, M12 y M15. Van al §10 del paquete de PB-T1 v2.8.
- **Anotados en `docs/ideas-futuras.md`:**
  - los cinco menores de la revisión del intento 2;
  - la limitación de la ExtensionData anidada en RACKEDITAR (F4B-E24).

**Disposiciones de la sesión en esta tarea:**
- la interrupción de transporte 1 de 2 de la fase WORK, reanudada en la misma invocación sin descartar las ediciones;
- el control de cadena del prompt de verificación, corregido;
- el RED `2d9febc6` declarado ChainRedSha sin acreditar. El hecho se declaró al Controller, y Tests pasó.

**Siguiente:** la emisión de PB-T1c con el paquete v2.8 (comprobación acotada hecha; F4B-E09 y F4B-E11).

## F4B-E27. Emisión de F4B-PBT1C-LOSSLESS-STATE-WINDOW (PB-T1c), con el paquete v2.8

**Fecha:** 2026-11-19. **Autoridad de emisión:** la autorización condicional del Owner (F4B-E09, pregunta 4; F4B-E11, «PB-T1 PLAN»). Se emite sin volver al Owner porque la comprobación acotada no deja bloqueantes ni importantes sin resolver y no hay ninguna decisión material nueva.

**El paquete v2.8:** las correcciones que deja PB-T1b para PB-T1c (F4B-E22: «el paquete de PB-T1c corrige el texto»).
- **La evidencia del RED de PB-T1c:** tests y ui-tests en failure, build-ui en success y build-plugin omitido. Con esto se aplica la lección M14.
- **INV-PBT1C-03:** sus miembros van con rangos de línea, documentación incluida. Lo retirado solo quita líneas.
- **PBT1C-C13:** el Worker para ante toda declaración con un nombre nuevo en W o WC, o ante una sobrecarga, salvo `editable`, `SyncName` y `EditableStateForTest`. Es la lección M10, por disposición de la sesión (PD-V28-01, opción (A)): estrecha la regla y no la relaja.
- **INV-PBT1C-05:** lleva el censo de declaraciones y las líneas testigo en ASCII.
- **CT-PBT1C-04:** lleva dieciséis autopruebas. Entre ellas, k1 y k2 de la regla 1b sobre los alias, tomadas de las pruebas corregidas de PB-T1b (`a18ba345`).
- **Otros cambios:**
  - los textos de CT-PBT1B-14 y 15, puestos al día con esas pruebas (PB-T1b no se reemite);
  - la regla de F4B-E25 para las correcciones;
  - los arrastres de PB-T1b en el §10;
  - la Snapshot J.
- **El registro de mediciones** gana el apartado Q: los anclajes de PB-T1c. Se midieron en `2f5d3dec`, que tiene el mismo `src` que el cierre `95373af6`: 59 anclas sin diferencia, la API de Application completa, ningún bloqueante y la simulación mecánica de D-PBT1-13 limpia.

**Las comprobaciones:** comprobacion acotada independiente de la v2.8 EMITIBLE TRAS CORRECCIONES 0/1/7, recomprobacion 0/1/3 y confirmacion final 0/0/1 (el assert del generador, alineado antes de emitir: seco final rc 0 sobre 95373af6), todas aplicadas; sin decisiones materiales nuevas para el Owner.

**El contrato:**
- **Alcance:** 5 rutas: W, WC y las tres clases de prueba.
- **Cadena:** primera delegación; `attempts` 1 de 3.
- **Anclaje:** `src` igual al cierre de PB-T1b.
- **Procedimiento:**
  - la planificación con las colecciones opacas puestas por la sesión (F4B-E21);
  - la verificación con las comprobaciones deterministas de F4B-E15.

## F4B-E28. Decisiones del Owner antes de los contratos de PB-T2 (Q-PBT2-1 a Q-PBT2-4)

**Fecha:** 2026-11-19. **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbt2-decisions.md`.

**El paquete:** PB-T2 v2.1, que se custodiará con la emisión de PB-T2a. Su revisión independiente de la v1 dio NECESITA v2 (0/8/16), y la recomprobación de la v2, LISTA PARA EMITIR TRAS REMEDICIÓN (0/0/7).

**Las respuestas del Owner,** todas «(Recomendado)»:
1. «Aplica sin cerrar; Cerrar avisa». Se aplica solo en el modo WORKSPACE y autoriza esa excepción a ADR-0029 D8.
2. «Lo rechaza, como Actualizar». Es la misma compuerta en los dos modos.
3. «En PB-T4, con prueba ya». PB-T2 añade una prueba de concordancia de la validación de la escala.
4. «Sí, con esa condición»: la emisión condicional de PB-T2a, PB-T2b y PB-T2c.

## F4B-E29. Decisión del Owner antes del contrato de PB-T3b: D-1

**Fecha:** 2026-11-19. **Texto literal:** `F4B-DRAFTS/coordinator/owner-pbt3-d1-decision.md`.

**El paquete:** PB-T3 v4, que se custodiará con la emisión de PB-T3a. Pasó por:
- la revisión independiente de la v1: 1/11/16;
- la recomprobación de la v2: 0/3/4;
- la recomprobación de la v3: 0/1/1, con los números corregidos en la v4.

**Respuesta del Owner:** «Un solo código (Recomendado)».
- Un único escritor para el piloto y el borrador completo.
- PB-T3b sustituye seis pruebas de F4, cada una con su registro declarado y una prueba equivalente que se ejecuta.
- PB-T4 sustituirá tres de esos equivalentes al quitar el piloto: nueve en total por esta decisión.

## F4B-E30. PB-T1c, intento 1: parada PBT1C-C07 tras el RED, y corrección en el intento 2

**Fecha:** 2026-11-19. **El análisis del Coordinator:** `F4B-PBT1C-LOSSLESS-STATE-WINDOW/1/R20261119T025420Z-ee6c/analysis.md`.

**El intento 1:**
- **La planificación** `R20261119T025114Z-43f7` se aceptó en el primer lanzamiento: fidelidad PASS 8/8, A1-A8 8/8 y nc4 `R20261119T025329Z-6a10`.
- **El Worker** `R20261119T025420Z-ee6c` hizo el RED `e5a21989`: la costura inerte y las tres clases.
  - Su CI `44450941610` es conforme: tests y ui-tests en failure, build-ui en success y build-plugin omitido.
  - Paró con **PBT1C-C07** antes del GREEN. CT-PBT1C-02 compara el diseño de la copia del estado con `LastComputation.Design` antes del recálculo neutro. En tres gestos de la parte compuesta, ese diseño va una pasada por detrás: el comportamiento ya existía y es el mismo en la base.
  - Con la comparación tras el recálculo, CT-02 pasa entera sobre la implementación validada sin commit.
- **Lo que no hizo el Worker:** no ajustó la expectativa ni compensó en el producto.

**La disposición de la sesión (16.8):** corrección en el **intento 2 de 3**, con FailureClass `StopCondition`.
- **C-1:** CT-02 toma su parte 1 tras el recálculo neutro.
- **El RED de la corrección:** solo esa clase.
- **El GREEN:** D-PBT1-13 en W y WC.
- **La cadena:** ChainRedSha null y ChainRedFiles vacío, porque el RED no se acreditó.
- **Las autoridades:** F4B-E25 cubre W, que es UNIT_CHANGE en la base.
- **El intento 3, si llegara,** vuelve al Owner.

**Un hallazgo de contención, anterior a F4B.** Pruebas de UI que ya existían crean un `RackFrameConfiguratorWindow` real (RichEditor*, EditorShellAdoptionTests, FirstView* y PushBackHeaderOwnerRoundTwoTests, entre otras), y su `OnClosed` escribe `RackFrameConfigurator.layout` en los datos locales del usuario.
- Cada corrida local de esos filtros sobrescribe la disposición del configurador del usuario.
- **No es una parada de PB-T1c.** PBT1C-C09 rige las pruebas nuevas, y PB-T1c no puede cambiar las existentes.
- **Lo que se hace con él:** se informa al Owner y se anota en `docs/ideas-futuras.md` al cerrar PB-T1c, para una tarea propia.

## F4B-E31. Cierre de F4B-PBT1C-LOSSLESS-STATE-WINDOW (PB-T1 completa)

**Paquete de cierre:** `F4B-PBT1C-LOSSLESS-STATE-WINDOW/task-close-package.md`.

**La cadena:**
- **Emisión** (F4B-E27): con el paquete v2.8, X `af46fcd5` y BASE `833ea6bf`.
- **Intento 1:**
  - la planificación `43f7` se aceptó al primer lanzamiento (fidelidad 8/8 y A1-A8 8/8), con nc4 `6a10`;
  - el Worker `ee6c` dejó el RED `e5a21989` (la costura inerte y las tres clases; CI `44450941610` conforme) y paró con PBT1C-C07 (F4B-E30).
- **Intento 2, la corrección** (F4B-E30, con el análisis custodiado en `d8ccbcb8`):
  - la planificación `cf1f` se aceptó al primer lanzamiento, con nc4 `2c93`;
  - el Worker `11b9` dejó el RED `2faae9da`, solo con C-1: la parte 1 de CT-PBT1C-02 tras el recálculo neutro;
  - y el GREEN `161a0dad`, D-PBT1-13 solo en W y WC.

**CI:**
- RED `44459636816`: tests y ui-tests en failure, build-ui en success y build-plugin omitido.
- GREEN `44463212627`: 4/4, con Core 19 088 y UI 2 439 (2 418 pasadas y 21 omitidas). Es la línea base del cierre de PB-T1b más 2 y más 2, exactamente lo previsto.

**Revisiones:**
- la directa, sin hallazgos;
- la independiente, ACEPTABLE CON MENORES 0/0/5 (PBT1CC2-IR-M01 a M05: comentarios desfasados y huecos de cobertura de las pruebas, sin producto) y un dato informativo para el Owner (OWNER-01: «Restaurar» no recalcula antes de leer el diseño, igual en la base).

**Verificación** `3612`: EXECUTION_VERIFIED 14/14 al primer lanzamiento. Authority pasa con la regla de F4B-E25: W es UNIT_CHANGE en la base y solo la tocó el RED del intento 1.

**Controles:** nc1 `e146` (Identity, STOP), nc2 `1c08` (Scope, STOP) y nc3 `f007` (FreeText, REWORK), los tres **CONTROL PASS** con el oráculo cumplido y sin sustitutos. La coincidencia «limit reached» de sus relevos es el texto de S-11 del README.

**Resultado:** **F4B-PBT1C-LOSSLESS-STATE-WINDOW = CLOSED / PASS WITH LIMITATIONS** sobre `161a0dad`, con attempts 2 de 3.
- La ventana clásica de Push Back guarda su estado editable en `PushBackEditableState`, en modo CLASSIC y sin cambio observable.
- **PB-T1 queda completa:** PB-T1a, PB-T1b y PB-T1c.

**Anotado en `docs/ideas-futuras.md`:**
- el aislamiento del configurador en las pruebas: las pruebas existentes escriben su disposición en el perfil del usuario;
- el desfase de una pasada del diseño calculado del compuesto, que ya existía.

**Siguiente:** la emisión de PB-T2a con el paquete de PB-T2 v2.2, por la autorización condicional de F4B-E28 (pregunta 4); su comprobación acotada ya está hecha.

## F4B-E32. Emisión de F4B-PBT2A-DRAFT-STATE-PORT (PB-T2a), con el paquete de PB-T2 v2.2

**Fecha:** 2026-11-19.

**Autoridad de emisión:** la autorización condicional del Owner, F4B-E28, pregunta 4: «Sí, con esa condición (Recomendado)».
- **Las condiciones que exige:**
  - la revisión de la subtarea da 0 bloqueantes y 0 importantes sin resolver;
  - la remedición sobre el cierre de PB-T1c no cambia nada material.
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3 o un efecto material sin medir. No se da ninguno.

**La revisión:**
- **El paquete de PB-T2:**
  - la revisión independiente de la v1 dio NECESITA v2, 0/8/16;
  - la recomprobación de la v2, LISTA PARA EMITIR TRAS REMEDICIÓN, 0/0/7, aplicada en la v2.1.
- **La comprobación acotada de PB-T2a** (CA-T2a), sobre la v2.1, el contrato propuesto y la salida de sus anclajes, da **EMITIBLE TRAS CORRECCIONES, 0/0/6**, sin decisiones nuevas para el Owner. Sus menores ya están aplicados:
  - M-1 a M-5, en el generador;
  - M-4, también en la v2.2: la referencia de PBT2x-C11;
  - M-6 queda para el paquete antes de emitir PB-T2b.

**La remedición sobre el cierre de PB-T1c** (`5a8366c1`, F4B-E31), hecha por el generador al escribir con `--pt1c-close`:
- el `src` de Application y de los demás proyectos, salvo W y WC, es igual al cierre de PB-T1b;
- W declara `editable`, como en D-PBT1-13;
- ningún campo ni constructor de la ventana captura una referencia del estado;
- la línea base de la CI es la del GREEN de PB-T1c (`44463212627` sobre `161a0dad`, el mismo `src` y los mismos `tests` del cierre): Core 19 088 y UI 2 439, con 21 omitidas. Es exactamente la prevista en el §6.1.

No hay cambios materiales.

**El contrato:**
- PB-T2a es el puerto por el que el editor devuelve su estado al borrador, su adaptador y el lanzamiento tipado del modo WORKSPACE, solo en Application.
- Primera delegación de la cadena; `attempts` 1 de 3.
- La planificación, con las colecciones opacas puestas por la sesión (F4B-E21).
- La verificación, con las comprobaciones deterministas de F4B-E15.

## F4B-E33. PB-T2a, intento 1: parada PBT2A-C07 tras el RED, y corrección en el intento 2

**Fecha:** 2026-11-19. **El análisis del Coordinator:** `F4B-PBT2A-DRAFT-STATE-PORT/1/R20261119T060524Z-55f2/analysis.md`.

**El intento 1:**
- **La planificación** `R20261119T060327Z-ad3e` se aceptó al primer lanzamiento: fidelidad PASS 8/8, A1-A8 8/8 y nc4 `R20261119T060503Z-d5f7`.
- **El RED del Worker** `R20261119T060524Z-55f2` es `c33ac6ac`: el esqueleto inerte del puerto y del lanzamiento, y las dos clases.
- **La parada:** el Worker paró con **PBT2A-C07** antes del GREEN. En CT-PBT2A-02, la línea `launchfor` compara `estado-es-el-actual` **después** de aplicar la edición por el puerto. Como el borrador guarda una copia nueva, la comparación da `False`.
- **El resto, bien:** la implementación, validada sin commit, hace pasar CT-PBT2A-01 y 03.
- **Lo que no hizo el Worker:** no ajustó la prueba ni el producto.

**La disposición de la sesión (16.8):** corrección en el **intento 2 de 3**, con FailureClass `StopCondition`.
- **C-1:** `estado-es-el-actual` se lee antes de aplicar; la línea esperada no cambia.
- **El RED de la corrección:** solo esa clase.
- **El GREEN:** solo los dos archivos nuevos de Application.
- **La cadena:** ChainRedSha null y ChainRedFiles vacío, porque el RED no se acreditó.
- **El intento 3, si llegara,** vuelve al Owner.

## F4B-E34. Cierre de F4B-PBT2A-DRAFT-STATE-PORT, con las decisiones del Owner sobre nc1 y nc2

**Paquete de cierre:** `F4B-PBT2A-DRAFT-STATE-PORT/task-close-package.md`.

**La cadena:**
- **Emisión** (F4B-E32): X `1ee10d3f` y BASE `0dc623ca`.
- **Intento 1:**
  - la planificación `ad3e`, aceptada al primer lanzamiento, con nc4 `d5f7`;
  - el Worker `55f2` dejó el RED `c33ac6ac` y paró con PBT2A-C07.
- **Intento 2, la corrección** (F4B-E33, con el análisis en `17872be8`):
  - la planificación `7de7`, con nc4 `4284`;
  - el Worker `3b39` dejó el RED `fb90c823`, solo con C-1, y el GREEN `5ddeb3b4`, con los dos archivos de Application.

**CI:**
- RED `44478438623`: tests en failure, ui-tests y build-ui en success, build-plugin omitido;
- GREEN `44480898605`: 4/4, con Core 19 092 y UI 2 439, como se preveía.

**Revisiones:**
- la directa, sin hallazgos;
- la independiente, ACEPTABLE CON MENORES 0/0/5, anotada en `docs/ideas-futuras.md`.

**Verificación** `R20261119T074453Z-10a9`: EXECUTION_VERIFIED 14/14.

**Tablas de comparación de los controles** (PBT2A-C11). Cada control se compara con la verificación real `10a9` (EXECUTION_VERIFIED / NONE, 14/14 en pass), comprobación por comprobación. Solo se listan las que difieren; todas las demás quedan en pass, como en la real.

| Control | Resultado | Primera en fail | En fail | Paradas | Firma |
|---|---|---|---|---|---|
| nc1 `R20261119T074938Z-0302` (`CurrentSha` falso, `deadbeef…`, en la entrega) | EXECUTION_BLOCKED / STOP | Handoff | Handoff, Identity, Remote, Scope, Ci y Tests | S-04 | **Variación de PD-F4-04:** la firma solo admite Handoff, Identity y Ci, con Remote en pass. Todas las evidencias en fail citan el SHA falso. Trailer, en pass |
| nc2 `R20261119T075153Z-a799` (una ruta menos en el `AllowedWriteScope` de la delegación: `PushBackDraftEditorLaunch.cs`) | EXECUTION_BLOCKED / STOP | Contract | Contract y Scope | S-04 y PBT2A-C01 | **La firma exacta de PD-F4-01** |
| nc3 `R20261119T075831Z-6b19` | — | — | — | — | CONTROL PASS ordinario |

**Las evidencias de nc2, frente a la firma:**
- **Scope** nombra la ruta quitada, que queda fuera de la lista blanca, y activa PBT2A-C01.
- **Contract:** A3 es subconjunto y A4 coincide en 192 entradas. Falla solo por el contraste del `AllowedWriteScope` estrechado con el contrato y con `byte-fidelity-check.json` (`f4-dispositions.md:520`, `:562`).
- **Las otras doce** quedan en pass, con evidencias de los mismos hechos que la real.

**El sustituto de F4 sobre nc2:** `PBT2A-NC2-EXACT-WHITELIST-01` (`R20261119T081504Z-9d60`) es una copia de `F4T1B-NC2-EXACT-WHITELIST-01` con solo el identificador y el ámbito cambiados. Dio **FAIL** en las propiedades 5 y 8, por el formato de F4B-E21:
- el relevo de la planificación guarda el SHA de la salida cruda del Controller, no el de la delegación con las colecciones inyectadas;
- los contratos de F4B no llevan la cláusula literal de F4 en `ExpectedEvidence`: su exactitud va por la comparación byte a byte.

Por PBT2A-C11, los dos controles volvieron al Owner.

**Decisiones del Owner** (`F4B-DRAFTS/coordinator/owner-pbt2a-controls-decisions.md`, literales):
- **nc1:** «Aceptarlo solo esta vez (Recomendado)». Queda `nc1 = SATISFIED_BY_OWNER_DISPOSITION`, solo para PB-T2a, intento 2. Es la segunda vez en F4B que sale esta variación, tras PB-TC (F4B-E08). Las firmas no cambian.
- **nc2:** «Adaptar el sustituto (Recomendado)».
  - El sustituto `PBT2A-NC2-EXACT-WHITELIST-F4B-01` cambia solo las propiedades 5 y 8 en el modo F4B-E21, más la autoprueba y dos argumentos: la nota de inyección y la salida cruda.
  - Pasó una revisión independiente: APTO TRAS CORRECCIONES 0/4/7; tras corregir, APTO 0/0/3, con las menores aplicadas.
  - Se ejecutó en `R20261119T125505Z-c2d3`: **PASS**, con las 10 propiedades en true y la autoprueba 50 de 50, determinista.
  - Queda `nc2 = SATISFIED_UNDER_PBT2A_EXACT_WHITELIST_SUBSTITUTE_F4B`.
  - En el resto de F4B se reutiliza con solo el identificador y el ámbito cambiados, y solo con la firma exacta de PD-F4-01.
  - Sus encargos, su revisión y sus notas están en `F4B-DRAFTS/pbt2a-nc2-substitute/`.

**Controles:** nc1 por disposición del Owner (solo esta vez), nc2 por el sustituto adaptado (PASS) y nc3 CONTROL PASS. Las salidas ordinarias de nc1 y nc2 y el FAIL del sustituto de F4 se conservan tal cual.

**Resultado:** **F4B-PBT2A-DRAFT-STATE-PORT = CLOSED / PASS WITH LIMITATIONS** sobre `5ddeb3b4`, con attempts 2 de 3. El puerto al borrador y el lanzamiento tipado del modo WORKSPACE quedan en Application.

**Disposición de la sesión:** el relevo del trabajo de la corrección se corrigió para mirar solo `src` y `tests` en la cadena, que incluye el commit de documentación de la sesión.

**Siguiente:** las decisiones de F4B-E35, y después la emisión de PB-T2b con el paquete de PB-T2 v2.7, que añade la regla RR de F4B-E35 a la v2.6, cuando su recomprobación termine limpia, por F4B-E28 (pregunta 4).

## F4B-E35. Decisiones del Owner: la emisión condicional de PB-T3 y la reparación del RED (RR)

**Fuente:** `F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`, con las preguntas y las respuestas literales. Se tomaron en la misma ronda que las de F4B-E34.

- **Pregunta 3, la emisión de PB-T3:** «Sí, con esa condición (Recomendado)».
  - PB-T3a, PB-T3b y PB-T3c se emiten en orden, cada una tras el cierre de la anterior, sin volver al Owner si su comprobación acotada da 0 bloqueantes y 0 importantes sin resolver y la remedición sobre el cierre anterior no cambia nada material, como en F4B-E28 (pregunta 4).
  - Vuelve al Owner por una decisión nueva, un control con una firma no prevista, un intento 3 o un efecto material sin medir.
  - Resuelve DU-1 del generador de PB-T3a.
- **Pregunta 4, la reparación del RED:** «Sí, con esas condiciones (Recomendado)». **La regla RR (RR-1 a RR-7)**, de F4B y desde PB-T2b, en las subtareas cuyo contrato la incorpore:
  - **RR-1. Cuándo:** un solo commit «RED repair» por intento, después del RED y antes del GREEN.
  - **RR-2. Qué toca:** solo las clases de prueba que el RED añadió.
  - **RR-3. Lo esperado no cambia:** ninguna línea esperada cambia; si hace falta cambiar una, es la parada de siempre.
  - **RR-4. El RED se repite:** su criterio se repite entero sobre la reparación, con su CI.
  - **RR-5. Se declara:** la entrega declara la reparación.
  - **RR-6. La cadena:** BaseSha..RedSha solo en `tests/`, RedSha..RedRepairSha solo en lo que añadió el RED, y RedRepairSha..CurrentSha solo en `src/`.
  - **RR-7. Sin coste:** no gasta intento.

  Excepciona AUTOMATION_PLAN 16.8 solo en esas condiciones, como F4B-E25 hizo con 16.3.
- **Motivo:** en PB-T1c y en PB-T2a se gastó un intento por un error de escritura de una prueba nueva que solo aparecía después del RED (F4B-E30 y F4B-E33).

## F4B-E36. Emisión de F4B-PBT2B-WORKSPACE-MODE-WINDOW (PB-T2b), con el paquete de PB-T2 v2.8

**Fecha:** 2026-11-19.

**Autoridad de emisión:** la autorización condicional del Owner, F4B-E28, pregunta 4: «Sí, con esa condición (Recomendado)».
- **Las condiciones que exige:**
  - la revisión de la subtarea da 0 bloqueantes y 0 importantes sin resolver;
  - la remedición sobre el cierre de PB-T2a no cambia nada material.
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3 o un efecto material sin medir. No se da ninguno.

**El cierre de PB-T2a** (F4B-E34, `b29551ae`): PB-T2a CLOSED / PASS WITH LIMITATIONS sobre `5ddeb3b4`, en el intento 2 de 3: la verificación `10a9`, 14/14; nc1 por disposición del Owner (solo esta vez); nc2 por el sustituto adaptado `PBT2A-NC2-EXACT-WHITELIST-F4B-01` (PASS, `c2d3`); nc3 PASS (F4B-E34, con las decisiones del Owner sobre nc1 y nc2).

**La reparación del RED** (F4B-E35, pregunta 4): el contrato incorpora la regla RR del Owner, que empieza en PB-T2b:
- PBT2B-C07 con su excepción, y PBT2B-C18, nueva, para una reparación que rompe la regla;
- la cadena RR-6 en INV-PBT2B-02;
- en la `ExpectedEvidence`, la CI del RED reparado y lo que declara la entrega;
- cómo lee la verificación del Controller una reparación (RR-7): las comprobaciones de 16.9 que la tocan, lo que comprueba de RR y la ayuda mecánica de RR-3 (v2.8).

Una reparación no gasta intento (RR-7).

**La revisión:**
- **La comprobación acotada de PB-T2b** (CA-T2b) se hizo sobre la v2.3, el contrato propuesto y la salida de sus anclajes. Da **EMITIBLE TRAS CORRECCIONES, B 0 / I 3 / M 7**, sin decisiones nuevas para el Owner.
  - **Sus tres importantes** son recetas de prueba con el riesgo que costó el intento 1 de PB-T1c y el de PB-T2a: el momento de una lectura y gestos que no hacen lo que dicen.
  - **La v2.4** las corrige con sus siete menores, y repasa con ese riesgo todas las CT de PB-T2b y PB-T2c. Ese repaso encontró además que la tolerancia de los 17 fixtures es 4, así que la edición de tolerancia de cuatro CT no cambiaba nada (V24-11).
- **Su recomprobación sobre la v2.4,** la §10 del mismo informe, da **EMITIBLE, nuevos B 0 / I 0 / M 4**. Las cuatro menores se aplican en la v2.5: M-08 en el generador y M-09 a M-11 en el texto.
- **La v2.6, la v2.7 y la v2.8:**
  - la v2.6 aplica la comprobación acotada de PB-T2c (CA-T2c) y su recomprobación, que solo cambian PB-T2c;
  - la v2.7 incorpora la regla RR de F4B-E35 en PB-T2b y PB-T2c;
  - la v2.8 aplica sus recomprobaciones (la §11 de CA-T2b y la §10 de CA-T2c), con una importante compartida, la lectura de la verificación, y PB-T2b se emite con ella.
- **La §12, sobre la v2.8,** da **EMITIBLE, nuevos B 0 / I 0 / M 2**, sin decisiones nuevas para el Owner. Sus dos menores se aplican en la cadena de la sesión, sin cambiar el contrato:
  - **M-05:** el relevo y la verificación escriben la lista cerrada de las líneas literales del paquete que la clase lleva en `RedSha`, y aplican sobre ella la parada C07 de RR-3;
  - **M-06:** la verificación comprueba por guion RR-1, RR-2, RR-6, la comprobación n.º 10 desde `RedRepairSha` y la comparación byte a byte, como en F4B-E15.
- **El resultado que lleva el contrato,** con las recomprobaciones que haya: `EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete; recomprobacion v2.4: EMITIBLE, B 0 / I 0 / M 4; M-08 a M-11 aplicadas en la v2.5; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2)`.

**La remedición sobre el cierre de PB-T2a** (`b29551ae`, F4B-E34), hecha por el generador al escribir con `--pt2a-close`:
- `git diff <cierre de PB-T2a> <BaseSha> -- src/RackCad.UI` sale vacío, y W, WC y WX son los blobs del GREEN de PB-T1c (`f8756a88`, `1a3300b4` y `fb094bef`). Por eso las anclas de W y WC y los rangos de INV-PBT2B-03 valen sin cambio.
- El puerto de PB-T2a, ya implementado, tiene la API de D-PBT2-02 y D-PBT2-03.
- Application es el GREEN de PB-T1c más los dos archivos de PB-T2a, y `tests` solo gana sus dos clases. Los recuentos de los RequiredTests son los de `0dc623ca`.
- La línea base de la CI: la corrida `push` `44480898605` sobre `5ddeb3b4`, con el `src` y los `tests` del cierre: Core 19 092 y UI 2 439, con 21 omitidas. Es exactamente la prevista en el §6.2.

No hay cambios materiales.

**El contrato:**
- **Qué es PB-T2b:** la ventana de Push Back en dos modos explícitos:
  - CLASSIC, sin cambio observable;
  - WORKSPACE, solo por la factoría tipada `ForDraft`.

  «Aplicar al borrador» devuelve una copia por el puerto de PB-T2a y no cierra. BOM y biblioteca siguen, con dos costuras de prueba nuevas.
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.

## F4B-E37. Decisión del Owner sobre el config.toml tras la actualización de la app de Codex: Snapshot K

**Fuente:** `F4B-DRAFTS/coordinator/owner-config-snapshot-k-decision.md`, con la pregunta y la respuesta literales.

**Los hechos:**
- **La parada:** la preparación de la planificación de PB-T2b, `R20261119T154005Z-3194`, paró con P-01 sin lanzar nada. La app de Codex se había actualizado y había reescrito `config.toml` a las 14:05:53Z. El hash pasó de `57B0FED6…` (Snapshot J) a `E1F69847…`.
- **Lo que cambió:** los 104 nombres siguen iguales y en el mismo orden. De los 68 valores del inventario seguro cambian solo dos, `mcp_servers.node_repl.env.BROWSER_USE_CODEX_APP_VERSION` y la ruta del plugin del navegador en `NODE_REPL_TRUSTED_SERVICES`, que pasan de 26.1111.21308 a 26.1111.71704.
- **Lo que no cambió:** el modelo, el effort y el binario acreditado de la CLI.

**La respuesta del Owner:** «Sonda neutra, luego K (Recomendado)».

**La sonda neutra** `F4-ENVIRONMENT/probe-R20261119T154313Z-2a9a` da **Pass**:
- binario acreditado `4F91AE86…`, gpt-6-luna / high efectivos, solo lectura, `approval_policy` never y CLI 0.167.0-alpha.3.2;
- frente a la referencia, la planificación `R20261119T065628Z-7de7` (Snapshot J, mismo binario), el contexto efectivo solo difiere en `root_turn_id`;
- el repositorio y `config.toml` no cambiaron durante la sonda.

**Resultado:** la **Snapshot K** (`E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE`) sustituye a la J como línea base activa del Controller en I-64. Su registro está en `F4-ENVIRONMENT/snapshot-K/snapshot-k.json`.
- La comprobación de config de la cadena la admite.
- La planificación de PB-T2b se lanzó con una preparación nueva, `69c3`.
- El registro y esta decisión se custodian con el cierre de PB-T2b, y no antes, porque la base de su cadena tenía que seguir siendo su emisión.
- El contrato de PB-T2b se emitió con «today Snapshot J». Su frase operativa, «the accepted Snapshot in force», es la K desde esta decisión, y su verificación lo recibió así.
- La v2.9 de PB-T2c ya nombra la K (V29-06).

## F4B-E38. Cierre de F4B-PBT2B-WORKSPACE-MODE-WINDOW, con las decisiones del Owner sobre nc1 y OWNER-01

**Paquete de cierre:** `F4B-PBT2B-WORKSPACE-MODE-WINDOW/task-close-package.md`.

**La cadena:**
- **Emisión** (F4B-E36): con el paquete de PB-T2 v2.8, X `a12ea09a` y BASE `59839c71`.
- **Planificación:**
  - **La preparación `3194`** paró por P-01 sin lanzarse. Llevó a la Snapshot K (F4B-E37).
  - **La planificación `69c3`** se aceptó al primer lanzamiento: fidelidad 8/8 (`session-inject`), A1-A8 8/8, nc4 `2540` (A3 fail, oráculo cumplido) y 19 criterios.
  - **Incidencia de la sesión:** ejecutó por error la comparación sobre la salida cruda, que sobrescribió la copia de `byte-fidelity-check.json`. Se restauró byte a byte desde `session-inject`.
- **El Worker `1a87`** (sonnet/high) dio IMPLEMENTATION_COMPLETE:
  - **el RED `a3b86134`:** la costura inerte en W, el archivo del modo y las seis clases;
  - **la primera reparación del RED de F4B, `6bc3497f`** (RR, F4B-E35). Añade un `Sta(() => { });` antes de cada `EditorDiscardPrompt.Substitute`, porque el primer `Sta` del proceso instalaba su aviso permanente y pisaba el de la prueba. Solo hay líneas añadidas en dos clases que añadió el RED, y no cambia ninguna línea esperada;
  - **el GREEN `03b16c88`:** solo W.

**CI:**
- RED `44572405777` y RED reparado `44581043462`: tests y ui-tests en failure, build-ui en success y build-plugin omitido. Fallan solo CT-PBT2B-08 en Core y CT-PBT2B-01 a 07 en UI, por aserción.
- GREEN `44586045707`: 4/4, con Core 19 094 y UI 2 447 (2 426 pasadas y 21 omitidas), como se preveía.

**Revisiones:**
- **La directa:** sin hallazgos.
- **La independiente:** ACEPTABLE CON MENORES 0/0/6, con un hallazgo OWNER.
  - Las cinco corridas de diagnóstico del Worker, con la instrumentación temporal revertida, cumplen 16.8, RR y la limpieza del árbol. Solo incumplen el régimen de las corridas temporales (M-03, menor).
  - Las menores van a `docs/ideas-futuras.md`.

**Verificación** `R20261119T182732Z-422f`: **EXECUTION_VERIFIED** 14/14 al primer lanzamiento, la primera con una reparación del RED.
- La comprobación n.º 10 acredita el RED con `RedSha` y mira RT desde `RedRepairSha`.
- La n.º 11 encuentra el trailer en los tres commits.
- `rr_check` da PASS: RR-1, RR-2, RR-6, la comprobación n.º 10 desde la reparación y la lista cerrada byte a byte (M-05 y M-06 de la §12 de CA-T2b, aplicados en la cadena).

**Tabla de comparación de nc1** (PBT2B-C11), frente a la verificación real `422f` (EXECUTION_VERIFIED / NONE, 14/14 en pass). Solo se listan las que difieren:

| Control | Resultado | Primera en fail | En fail | Paradas | Firma |
|---|---|---|---|---|---|
| nc1 `R20261119T183134Z-bfda` (`CurrentSha` falso `deadbeef…` en la entrega) | EXECUTION_BLOCKED / STOP | StopCondition | Identity, Remote, Ci, Tests y Trailer | S-04 | **No es PD-F4-04:** Handoff pasa; Remote, Tests y Trailer fallan además. Tests y Trailer, también por leer mal `Worker.Trailer` y `RedRepairSha` en `Deviations`, que la entrega trae bien |
| nc2 `R20261119T183325Z-14dd` | CONTROL PASS (Scope, STOP) | — | — | — | — |
| nc3 `R20261119T183456Z-a3bc` | CONTROL PASS (FreeText, REWORK) | — | — | — | — |

**Decisiones del Owner** (`F4B-DRAFTS/coordinator/owner-pbt2b-close-decisions.md`, literales):
- **nc1:** «Aceptarlo solo esta vez (Recomendado)». Queda `nc1 = SATISFIED_BY_OWNER_DISPOSITION`, solo para PB-T2b, intento 1. Es la tercera variación de nc1 en F4B, tras PB-TC (F4B-E08) y PB-T2a (F4B-E34). Las firmas no cambian.
- **OWNER-01:** «Corregir en PB-T4 (Recomendado)». Si al cerrar hay a la vez un módulo escenificado y cambios sin aplicar al borrador, «Cerrar» solo hace la pregunta del módulo (el código literal de D-PBT2-07). El arrastre **A-T2b-OWNER-01** va a PB-T4: allí «Cerrar» hará una sola pregunta que nombre las dos cosas, con su prueba.

**Controles:** nc1 por disposición del Owner, solo esta vez; nc2 y nc3, CONTROL PASS. La coincidencia «limit reached» de los relevos es el texto de S-11 del README.

**Resultado:** **F4B-PBT2B-WORKSPACE-MODE-WINDOW = CLOSED / PASS WITH LIMITATIONS** sobre `03b16c88`, con attempts 1 de 3 y una reparación del RED.
- La ventana de Push Back tiene dos modos explícitos. CLASSIC no cambia de forma observable. WORKSPACE solo se obtiene por `ForDraft`: carga una copia del borrador, «Aplicar al borrador» devuelve una copia por el puerto de PB-T2a y no cierra, y «Insertar» y el lote se rechazan.
- BOM y biblioteca siguen, con sus dos costuras.

**Siguiente:** la emisión de PB-T2c con el paquete de PB-T2 v2.9, tras la remedición sobre este cierre y su recomprobación final (la §13 de CA-T2c), por F4B-E28 (pregunta 4).

## F4B-E39. Emisión de F4B-PBT2C-WORKSPACE-CARRYOVER (PB-T2c), con el paquete de PB-T2 v2.9

**Fecha:** 2026-11-19.

**Autoridad de emisión:** la autorización condicional del Owner, F4B-E28, pregunta 4: «Sí, con esa condición (Recomendado)».
- **Las condiciones que exige:**
  - la revisión de la subtarea da 0 bloqueantes y 0 importantes sin resolver;
  - la remedición sobre el cierre de PB-T2b no cambia nada material;
  - PB-T2b no llega a su intento 3.
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3 o un efecto material sin medir. No se da ninguno.

**El cierre de PB-T2b** (F4B-E38, `56cb026f`): PB-T2b CLOSED / PASS WITH LIMITATIONS sobre `03b16c88`, intento 1 de 3 con la primera reparación del RED de F4B (`6bc3497f`, RR); verificación `422f` 14/14 con `rr_check` PASS; nc1 por disposición del Owner (solo esta vez); nc2 y nc3 PASS; OWNER-01 (Cerrar con módulo escenificado y cambios sin aplicar) como arrastre a PB-T4; F4B-E38. Con él se custodió la decisión del Owner F4B-E37: la Snapshot K sustituye a la J como línea base del `config.toml` del Controller.

**La reparación del RED** (F4B-E35): el contrato incorpora la regla RR del Owner, como el de PB-T2b:
- PBT2C-C07 con su excepción, y PBT2C-C18 para una reparación que rompe la regla;
- la cadena RR-6 en INV-PBT2C-02, con las corridas aisladas sobre la clase reparada;
- en la `ExpectedEvidence`, la CI del RED reparado y lo que declara la entrega;
- cómo lee la verificación del Controller una reparación (RR-7, v2.8), con la ayuda mecánica de RR-3 y la lista cerrada de las líneas fijas de CT-PBT2C-01 y 02 (v2.9).

Una reparación no gasta intento (RR-7).

**La revisión:**
- **La comprobación acotada de PB-T2c** (CA-T2c) se hizo sobre la v2.5, el contrato propuesto y la salida de sus anclajes. Da **EMITIBLE TRAS CORRECCIONES, B 0 / I 1 / M 5**, sin decisiones nuevas para el Owner.
  - **Su importante:** las filas de estilo y de `pb-08-redondeo` de CT-PBT2C-01 no decían de dónde se leen sus datos, y en WORKSPACE la petición clásica es nula.
  - **La v2.6** la corrige, con sus cinco menores.
- **Sus recomprobaciones,** en el mismo informe:
  - **la §9, sobre la v2.6:** EMITIBLE, nuevos B 0 / I 0 / M 3. M-06 a M-08 se aplican en la v2.6;
  - **la §10, sobre la v2.7** (la regla RR de F4B-E35): EMITIBLE TRAS CORRECCIONES, nuevos B 0 / I 1 / M 3. Su importante es la compartida con PB-T2b, la lectura de la verificación, y la v2.8 la corrige con sus menores;
  - **la §11, sobre la v2.8:** EMITIBLE, nuevos B 0 / I 0 / M 2. M-09 se aplica en la v2.9, y M-10 es de la cadena, para el texto de la verificación;
  - **la §12, sobre la v2.9:** EMITIBLE, nuevos B 0 / I 0 / M 2. M-11 y M-12 se aplican en la misma v2.9;
  - **la §13, sobre la v2.9, final sobre el cierre de PB-T2b:** EMITIBLE, nuevos B 0 / I 0 / M 2.
    - M-13: las filas inertes de CT-PBT2C-03 llevan su porqué en la tabla, y su límite está abajo.
    - M-14: la medición se custodia sin el TRX.
- **El resultado que lleva el contrato,** con sus recomprobaciones: `EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2; M-09 aplicada en la v2.9; recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2; M-11 y M-12 aplicadas en la v2.9; recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2 (seccion 13, final sobre el cierre de PB-T2b); M-13 y M-14 aplicadas)`.

**La remedición sobre el cierre de PB-T2b** (`56cb026f`, F4B-E38), hecha por el generador al escribir con `--pt2b-close`:
- `git diff <cierre de PB-T2b> <BaseSha> -- src` sale vacío, y el cierre trae W cambiado, el archivo del modo y su `task-close-package.md`.
- Los textos de los tres mutantes [T2b] se leen sobre `BaseSha`; el de MU-T2C-03, por contenido.
- La tabla de CT-PBT2C-03, con lo esperado de cada uno de los 49 gestos, sale de una corrida temporal de UI en WORKSPACE, fuera de todo commit y con TRX (`measure_pbt2c.py`, v2.6, V26-02). Está custodiada y se comprueba.
  - **Lo que se custodia de la medición** (CA-T2c M-14): la clase temporal (`PushBackPbt2cCt03ProbeTests.cs.txt`), `measure_pbt2c.py` y su salida.
  - **El TRX no se custodia:** lleva el nombre del equipo, el del usuario y rutas locales. Queda identificado por su SHA-256, que va en `pbt2c-anchors-out.txt`, como los TRX de la CI.
- **El límite de cobertura de CT-PBT2C-03** (CA-T2c M-13): tres de las seis filas «nada» son gestos inertes, porque el catálogo solo ofrece una opción: `PostBox=otro`, `CellInOutBeamBox=otro` y `CellInOutPeralteBox=otro`.
  - Su resultado es correcto, pero CT-PBT2C-03 no prueba esos tres controles en el camino al borrador. En CLASSIC, CT-PBT1C-02 tampoco los prueba.
  - La tabla lo dice bajo cada una (`  porque: el catalogo solo ofrece una opcion`), en una línea que el Worker no copia.
  - Si el catálogo gana opciones, esas filas dejarán de ser «nada», y la caracterización lo detectará.
- La Snapshot K: su registro y la decisión de F4B-E37 están en el cierre, y el generador comprueba el SHA-256 y la sonda neutra (v2.9, V29-06).
- `src` desde `161a0dad` cambia solo en PB-T2a y PB-T2b; WC y WX siguen siendo los de `161a0dad`, y los recuentos de las familias, los de `17872be8`.
- La línea base de la CI: la corrida `push` `44586045707` sobre `03b16c88`, con el `src` y los `tests` del cierre: Core 19 094 y UI 2 447, con 21 omitidas. Es exactamente la prevista en el §6.3.

No hay cambios materiales.

**El contrato:**
- **Qué es PB-T2c:** fijar con pruebas nuevas, sin cambiar el producto, lo que hereda el modo WORKSPACE de la ventana de Push Back:
  - el paso 4 de D-PB-06: los valores exactos y el estilo de cotas llegan al borrador;
  - el aviso de D-PB-08;
  - el censo de controles en el camino al borrador.

  El RED son las pruebas nuevas y los tres mutantes de la tabla cerrada MU-T2C-01 a 03, en W y WC; el GREEN los revierte.
- **P-01:** la Snapshot K (F4B-E37; v2.9, V29-06). PB-T2a y PB-T2b se emitieron con la J.
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.

## F4B-E40. Cierre de F4B-PBT2C-WORKSPACE-CARRYOVER (PB-T2 completa), con la decisión del Owner sobre nc1 y la firma PD-F4B-01

**Paquete de cierre:** `F4B-PBT2C-WORKSPACE-CARRYOVER/task-close-package.md`.

**La cadena:**
- **La emisión** (F4B-E39): con el paquete de PB-T2 v2.9, X `92232ed3` y BASE `05beabfd`. CA-T2c y sus seis rondas; la última, la §13 sobre el cierre de PB-T2b, EMITIBLE 0/0/2, con M-13 y M-14 aplicadas. La tabla de CT-PBT2C-03 la midió la sesión en una corrida temporal de UI: 49 filas, de las que aplican 42, en 6 no hay nada que aplicar y 1 se niega.
- **La planificación:**
  - Dos preparaciones no se lanzaron: `b5fa`, por la expresión del pie de la tabla, y `a19d`, por un P-02 falso, porque la línea de órdenes de la sesión llevaba la ruta del worktree.
  - La planificación `65ce` se aceptó al primer lanzamiento: fidelidad 8/8, A1-A8 8/8, nc4 `d138` y 17 criterios.
- **El Worker `05b7`** (sonnet/high) dio IMPLEMENTATION_COMPLETE, sin reparación del RED:
  - el RED `7aeb6170`: los tres mutantes de una línea (W:3713, WC:95 y W:3273) y la clase `PushBackWorkspaceCarryoverTests`;
  - las corridas aisladas `m1` a `m3`: cada mutante hace fallar el CT de su fila, y m3 además CT-02;
  - el GREEN `b1968341`: revierte los mutantes, y `src` queda igual que en la base.

**CI:**
- RED `44618262418`: tests y ui-tests en failure, build-ui en success y build-plugin omitido. Los fallos existentes se atribuyen a los mutantes, entre ellos CT-PBT2B-08 en Core.
- GREEN `44619492077`: 4/4, Core 19 094 y UI 2 450 (2 429 pasadas y 21 omitidas), lo previsto.

**Revisiones:**
- La directa: sin hallazgos.
- La independiente: ACEPTABLE CON MENORES 0/0/2, más OWNER-01, informativo:
  - en WORKSPACE, un lado B declarado sin frentes sobrevive en el borrador mientras vive, y en CLASSIC se pierde al reabrir;
  - el aviso sigue siendo cierto respecto al dibujo;
  - se informa al Owner, y qué escribe el panel queda para PB-T3 y PB-T4.

**Verificación** `R20261119T220812Z-e59c`: **EXECUTION_VERIFIED** 14/14 al primer lanzamiento, con `rr_check` PASS (sin reparación).

**Tabla de comparación de nc1** (PBT2C-C11), frente a la verificación real `e59c`. Solo se listan las comprobaciones que difieren:

| Control | Resultado | Primera en fail | En fail | Paradas | Firma |
|---|---|---|---|---|---|
| nc1 `R20261119T221258Z-ea01` (`CurrentSha` falso `deadbeef…`) | EXECUTION_BLOCKED / STOP | Handoff | Handoff, Identity, Remote, Scope, Ci y Tests | S-04 | **Variación de PD-F4-04**, la misma cascada que en PB-T2a. Todas las evidencias en fail citan el SHA falso. Trailer y las demás, en pass |
| nc2 `R20261119T222128Z-1ff5` | CONTROL PASS (Scope, STOP) | — | — | — | — |
| nc3 `R20261119T222719Z-4f9f` | CONTROL PASS (FreeText, REWORK) | — | — | — | — |

**La decisión del Owner** (`F4B-DRAFTS/coordinator/owner-pbt2c-nc1-decision.md`, literal): «Aceptar y ampliar la firma (Recomendado)».
- **PB-T2c:** `nc1 = SATISFIED_BY_OWNER_DISPOSITION`.
- **La firma nueva PD-F4B-01, «cascada del SHA falso»,** para el resto de F4B, con las condiciones (a) a (f) de la decisión:
  - Handoff primero, STOP con S-04;
  - en fail, solo un subconjunto de {Handoff, Identity, Remote, Scope, Ci, Tests}, con Handoff e Identity;
  - cada evidencia en fail cita el SHA falso y ninguna atribuye otra causa;
  - Identity escribe además el HEAD real.
- **El sustituto** `PBT3x-NC1-CASCADE-SIGNAL-01` lleva autoprueba y revisión independiente antes de los controles de PB-T3a. Cualquier otra variación vuelve al Owner.
- Es la cuarta variación de nc1 en F4B, tras PB-TC (F4B-E08), PB-T2a (F4B-E34) y PB-T2b (F4B-E38).

**Controles:** nc1 por disposición del Owner; nc2 y nc3, CONTROL PASS.

**Resultado:** **F4B-PBT2C-WORKSPACE-CARRYOVER = CLOSED / PASS WITH LIMITATIONS** sobre `b1968341`, con attempts 1 de 3.
- Las obligaciones heredadas de PB-TC y PB-TC2 se cumplen en WORKSPACE, y lo fijan CT-PBT2C-01 a 03.
- **PB-T2 queda completa:** PB-T2a, PB-T2b y PB-T2c.

**Siguiente:** la emisión de PB-T3a con el paquete de PB-T3 v4.4, por F4B-E35 (pregunta 3). Su comprobación acotada CA-T3a dio EMITIBLE en su recomprobación sobre la v4.4. Antes de sus controles, el sustituto de PD-F4B-01, revisado.
## F4B-E41. Emisión de F4B-PBT3A-FULL-WRITE-MODEL (PB-T3a), con el paquete de PB-T3 v4.4

**Fecha:** 2026-11-19.

**Autoridad de emisión:** la autorización condicional del Owner, F4B-E35, pregunta 3: «Sí, con esa condición (Recomendado)». Cubre PB-T3a, PB-T3b y PB-T3c, en ese orden y cada una tras el cierre de la anterior.
- **Las condiciones que exige:**
  - la comprobación acotada de la subtarea da 0 bloqueantes y 0 importantes sin resolver;
  - la remedición sobre el cierre anterior, el de PB-T2c, no cambia nada material.
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3 o un efecto material sin medir. No se da ninguno.

**El cierre de PB-T2c** (F4B-E40, `f273eae1`): PB-T2c CLOSED / PASS WITH LIMITATIONS sobre `b1968341`, intento 1 de 3; verificación `e59c` 14/14; nc1 por disposición del Owner, que además aprueba la firma nueva PD-F4B-01 (cascada del SHA falso) para el resto de F4B, con su sustituto antes de los controles de PB-T3a; nc2 y nc3 PASS; PB-T2 completa; F4B-E40.

**La reparación del RED** (F4B-E35, pregunta 4): el contrato incorpora la regla RR del Owner, como los de PB-T2b y PB-T2c:
- PBT3A-C07 con su excepción, y PBT3A-C18 para una reparación que rompe la regla;
- la cadena RR-6 en INV-PBT3A-02;
- en la `ExpectedEvidence`, la CI del RED reparado, lo que declara la entrega y la lectura entera de la verificación con una reparación (V44-04);
- la ayuda mecánica de RR-3, con la tabla cerrada de cada clase: las líneas se comparan como subcadena y los valores de los gestos, como token (V44-02 y V44-03).

Una reparación no gasta intento (RR-7).

**Las revisiones:**
- **La revisión independiente del paquete** (`independent-review-v1.md`): NECESITA v2 sobre la v1 (1/11/16). Sus recomprobaciones:
  - la §10, sobre la v2: NECESITA v3 (0/3/4);
  - la §11, sobre la v3: NECESITA v4 (0/1/1);
  - la §12, sobre la v4, y la §13, sobre la v4.2: LISTO PARA EMITIR TRAS REMEDICIÓN (0/0/1).
- **La revisión de compatibilidad con PB-T2** (`compat-pbt3-vs-pbt2.md`): COMPATIBLE CON REMEDICIÓN (0/5/7), sobre la v4.1. La aplica la v4.2.
- **La comprobación acotada de PB-T3a** (CA-T3a), sobre el contrato en seco y la v4.3: **EMITIBLE TRAS CORRECCIONES, B 0 / I 1 / M 4**, sin decisiones nuevas para el Owner.
  - **Su importante:** `BeamDepth` era un gesto inerte, porque el catálogo solo ofrece el peralte 6, y el ancla de CT-PBT3A-09 no podía salir. La v4.4 cambia el gesto de CT-PBT3A-09 y de CT-PBT3B-01 a `PalletDepth` 48.5 (V44-01).
  - **Sus menores:** M-01 a M-03 se corrigen en la v4.4 (la evidencia y la tabla de RR-3), y M-04 en el generador.
- **Su recomprobación** (§10), sobre la v4.4: **EMITIBLE, B 0 / I 0 / M 1**. M-05, la custodia de sus guiones, se aplica en el generador.
- **El resultado que lleva el contrato:** `EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; importantes corregidas en el paquete: I-01 (CT-PBT3A-09 y CT-PBT3B-01 sin el gesto inerte de BeamDepth); recomprobacion v4.4: EMITIBLE, B 0 / I 0 / M 1`.

**La remedición sobre el cierre de PB-T2c** (`f273eae1`, F4B-E40). La hace el generador al escribir con `--pt2c-close`, y la salida queda custodiada en `pbt3a-anchors-out.txt`:
- `git diff <cierre de PB-T2c> <BaseSha> -- src` sale vacío, y `src` es el del cierre de PB-T2b;
- las anclas de W se leen por su contenido, y `Recompute`, `LoadFromModel` y `ReadInputs` son iguales a los del cierre de PB-T1c;
- la Snapshot K: su SHA-256 y su sonda neutra (F4B-E37);
- los recuentos y los alcances, con las clases de PB-T2;
- la línea base de la CI: la corrida `push` `44619492077` sobre `b1968341`, con el `src` y los `tests` del cierre: Core 19 094 y UI 2 450, con 21 omitidas. Es exactamente la prevista en el §6.1 del paquete.

No hay cambios materiales.

**El contrato:**
- **Qué es PB-T3a:**
  - crear en Application, solo con archivos nuevos en `src/RackCad.Application/Systems/PushBack/Draft/`, el modelo del borrador completo de Push Back: el estado y su comparador, el asiento, el materializador y el lector, la composición, la disposición de las vistas y el clasificador de D-PB-05;
  - sin conectarlo al panel.

  El RED es el esqueleto inerte y las pruebas nuevas; el GREEN implementa solo los ocho archivos nuevos.
- **P-01:** la Snapshot K (F4B-E37).
- **C11:** nc2 con el sustituto adaptado a F4B (F4B-E34).
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.
## F4B-E42. Parada de F4B-PBT3A-FULL-WRITE-MODEL en el intento 1 (PBT3A-C02), decisión del Owner (Renombrar ForWrite) y reemisión con el paquete de PB-T3 v4.5

**Fecha:** 2026-11-20.

**La parada:** el Worker `R20261120T001738Z-7d92` (delegación `R20261120T001442Z-903c`, intento 1) paró con **PBT3A-C02 antes del commit RED**, sin commits ni push. HEAD siguió en `18e4b783` y el árbol quedó limpio.
- **Lo que falla:** una prueba existente, `WorkspacePilotUpdateIntegrationGuardTests.G9NoFileOfThePluginOrOfApplicationThatNamesThePilotUsesAWriteApiOrALockOutsideTheFourExceptionsWithTheirExactCounts`, sobre el esqueleto exacto de la v4.4.
- **La causa, un defecto del paquete:**
  - el esqueleto hace que `Draft/PushBackFullComposition.cs` nombre el piloto, porque usa `PushBackPilotMaterializer.NonPilotContentReason`;
  - y que declare `ForWrite`, el nombre cerrado de D-PBT3-05;
  - la guarda G-9 prohíbe ese término de escritura en los archivos de Application y del Plugin que nombran el piloto, salvo cuatro excepciones fijas.
- **El Worker** no eligió ninguna salida, porque todas cambiaban el esqueleto, el API cerrado o una prueba existente. Dejó tres parches reutilizables (`wip-red-tree.patch`, `rr/skeleton.patch` y `rr/green-wip.patch`), que no son autoridad.

**La decisión del Owner** (`F4B-DRAFTS/coordinator/owner-pbt3a-g9-decision.md`): «Renombrar ForWrite (Recomendado)». G-9 no se toca, ni ninguna otra prueba existente.

**El paquete v4.5** (V45-01 a V45-05):
- el miembro cerrado pasa a `ForFullWrite`, con la misma firma, en D-PBT3-05, la lista cerrada de C13, `BuildFull` de D-PBT3-09, CT-PBT3A-06 y 11 y el esqueleto del §6.1;
- el barrido de los archivos nuevos frente a todas las guardas existentes (§4.1) no encuentra otra colisión;
- la precondición 11 es la reemisión.

**Su comprobación acotada** (la §11 de CA-T3a): `EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; importantes corregidas en el paquete: I-01 (CT-PBT3A-09 y CT-PBT3B-01 sin el gesto inerte de BeamDepth); recomprobacion v4.4: EMITIBLE, B 0 / I 0 / M 1; recomprobacion v4.5: EMITIBLE, B 0 / I 0 / M 2`.

**La reemisión:** el contrato de `F4B-PBT3A-FULL-WRITE-MODEL` se emite de nuevo con la v4.5 y sustituye al de `18e4b783` (F4B-E41), que queda en la historia de Git.
- **La corrección va en el intento 2 de 3:** la parada del intento 1 consume intento (16.8). `FailureClass` es `StopCondition`; `ChainRedSha` es null y `ChainRedFiles` está vacío, porque el intento 1 no hizo RED. El RED es el entero de la v4.5.
- **El análisis de la sesión** está custodiado en `F4B-PBT3A-FULL-WRITE-MODEL/1/R20261120T001738Z-7d92/analysis.md` (sha256 `7a615033c57403ab52e3b254f81a87fbfad1fc1aae7fd95ad15281b0210e105a`):
  - C-1, el nombre;
  - C-2, el RED entero;
  - C-3, el GREEN;
  - C-4, la reparación del RED, que sigue disponible.
- **Lo que no cambia:** el diseño, D-1, las listas cerradas de CT, los `MinSelected`, el GREEN previsto y las pruebas existentes.

## F4B-E43. Cierre de F4B-PBT3A-FULL-WRITE-MODEL, con la decisión del Owner sobre nc2

**Paquete de cierre:** `F4B-PBT3A-FULL-WRITE-MODEL/task-close-package.md`.

**La cadena:**
- **La emisión** (F4B-E41): X `2d876831` y BASE `18e4b783`, con la v4.4.
- **El intento 1:**
  - la planificación `903c` se aceptó, con nc4 `7aa0`;
  - el Worker `7d92` paró antes del RED con PBT3A-C02, sin commits. El esqueleto del paquete hacía que `PushBackFullComposition.cs` nombrara el piloto y declarara `ForWrite`, y la guarda G9 lo prohíbe.
- **La reemisión y la corrección en el intento 2** (F4B-E42): el Owner eligió «Renombrar ForWrite».
  - Con la v4.5 y `ForFullWrite`, el commit X es `5bda5569`, con el análisis, y el BASE es `485b5651`.
  - El barrido de todas las guardas no encontró otra colisión.
  - La §11 de CA-T3a dio EMITIBLE 0/0/2.
- **La planificación `fc50`** se aceptó al primer lanzamiento, con `CorrectionOf` exacto, nc4 `9811` y 17 criterios.
- **El Worker `f006`** (sonnet/high) dio IMPLEMENTATION_COMPLETE, sin reparación del RED.
  - Partió de los parches del intento 1 con el nombre cambiado.
  - El RED `5dbc9661` lleva el esqueleto y las seis clases; el GREEN `6919e17a` toca solo `src/`.
  - G9 pasa en el RED y en el GREEN.

**CI:**
- RED `44666242222`: tests y ui-tests en failure, build-ui en success y build-plugin omitido;
- GREEN `44668584248`: 4/4, Core 19 107 y UI 2 451 (2 430 pasadas y 21 omitidas), lo previsto.

**Revisiones:**
- la directa, sin hallazgos;
- la independiente, ACEPTABLE CON MENORES 0/0/9, sin hallazgos para el Owner.

**Verificación** `R20261120T034433Z-6d41`: **EXECUTION_VERIFIED** 14/14 al primer lanzamiento.

**Controles:**
- **nc1** `R20261120T035005Z-c736`: CONTROL PASS (Identity, STOP). El sustituto de PD-F4B-01 no hizo falta.
- **nc3** `R20261120T040354Z-5eac`: CONTROL PASS (FreeText, REWORK).
- **nc2** `R20261120T035748Z-ad66`: CONTROL NOT PASSED, con la firma exacta de PD-F4-01.
  - Comparada con la verificación real, solo difieren Contract, la primera, que falla solo por la exactitud de la lista blanca, y Scope, sobre la ruta quitada `PushBackConflictClassifier.cs`. El resultado es STOP, con S-04, P-03 y PBT3A-C01. Las otras doce, en pass.
  - El sustituto adaptado a F4B, `PBT3A-NC2-EXACT-WHITELIST-F4B-01` (`R20261120T040923Z-882e`), dio **FAIL** en la propiedad 8: su literal de la cláusula de exactitud es el de los contratos de PB-T2, y el de PB-T3a está redactado de otra forma.

**La decisión del Owner** (`F4B-DRAFTS/coordinator/owner-pbt3a-nc2-decision.md`, literal): «Aceptar y ampliar el sustituto (Recomendado)».
- **PB-T3a:** `nc2 = SATISFIED_BY_OWNER_DISPOSITION`.
- **De PB-T3b en adelante,** el sustituto admite una lista cerrada de los literales exactos de cada paquete, con autoprueba y revisión independiente antes de usarlo.

**El sustituto de nc1 de PD-F4B-01** (F4B-E40), revisado y APTO tras tres rondas, se custodia en `F4B-DRAFTS/nc1-cascade-substitute/` con su revisión, sus notas y sus límites declarados. En PB-T3a no se aplicó.

**Resultado:** **F4B-PBT3A-FULL-WRITE-MODEL = CLOSED / PASS WITH LIMITATIONS** sobre `6919e17a`, con attempts 2 de 3. El modelo de escritura completa del borrador de Push Back queda en Application, en archivos nuevos: el estado completo, el lector, la composición, la disposición de vistas y el clasificador de conflictos.

**Siguiente:** la emisión de PB-T3b por F4B-E35 (pregunta 3), tras la remedición sobre este cierre y su comprobación acotada. Antes de sus controles, el sustituto de nc2 ampliado.
## F4B-E44. Emisión de F4B-PBT3B-FULL-WRITE-RUN (PB-T3b), con el paquete de PB-T3 v4.5

**Fecha:** 2026-11-20.

**Autoridad de emisión:** la autorización condicional del Owner, F4B-E35, pregunta 3: «Sí, con esa condición (Recomendado)». Cubre PB-T3a, PB-T3b y PB-T3c, en ese orden y cada una tras el cierre de la anterior.
- **Las condiciones que exige:**
  - la comprobación acotada de la subtarea da 0 bloqueantes y 0 importantes sin resolver;
  - la remedición sobre el cierre anterior, el de PB-T3a, no cambia nada material.
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3 o un efecto material sin medir. No se da ninguno.

**El cierre de PB-T3a** (F4B-E43, `c40eea4a`): PB-T3a CLOSED / PASS WITH LIMITATIONS sobre `6919e17a`, intento 2 de 3 tras la reemisión con la v4.5 (RED `5dbc9661`, sin reparación del RED); verificación `6d41` 14/14; nc1 y nc3 PASS; nc2 por disposición del Owner, que además amplía el sustituto de nc2 a una lista cerrada de literales por paquete, de PB-T3b en adelante; CI del GREEN `44668584248` 4/4; F4B-E43.

**D-1, la variante (A)** (F4B-E29): PB-T3b sustituye con registro las seis guardas de F4 S-1 a S-6, con un `Skip` de motivo literal y sus equivalentes E-1 a E-6 de `PushBackPilotUpdateEquivalenceGuardTests`. El registro declarado va en `F4B-DRAFTS/superseding-characterization.md`, en esta emisión. La lista se mide otra vez sobre el texto del GREEN (INV-PBT3B-15), y cualquier otra diferencia es PBT3B-C16, que vuelve al Owner.

**Los controles** (PBT3B-C11), con las decisiones del Owner posteriores a la v4.5:
- **nc1:** la firma PD-F4B-01, la cascada del SHA falso (F4B-E40), con su sustituto `PBT3x-NC1-CASCADE-SIGNAL-01`, revisado APTO y custodiado en el cierre de PB-T3a. Se usa con `--task PBT3B`.
- **nc2:** el sustituto ampliado, la v2 de `PBTx-NC2-EXACT-WHITELIST-F4B-01` (F4B-E43), con su lista cerrada de literales de PB-T2 y de PB-T3, su autoprueba y su revisión independiente (SHA-256 `c033e96c`…). Se usa con `--task PBT3B`. La primera entrada de la `ExpectedEvidence` es, byte a byte, la de PB-T3a, que su lista ya lleva.
- El texto del paquete nombra todavía el sustituto de nc2 v1 y la firma exacta de PD-F4-04; el contrato se aparta de él solo en esas dos salidas, por esas dos decisiones.

**La reparación del RED** (F4B-E35, pregunta 4): el contrato incorpora la regla RR del Owner, con la lectura de PB-T3b:
- PBT3B-C07 con su excepción, y PBT3B-C18 para una reparación que rompe la regla;
- la cadena RR-6 en INV-PBT3B-02;
- una reparación no toca PP, UR, la interfaz nueva ni los tres archivos de guardas de F4 con los `Skip` (RR-2);
- en la `ExpectedEvidence`, la CI del RED reparado y la lectura entera de la verificación con una reparación.

Una reparación no gasta intento (RR-7).

**La comprobación acotada de PB-T3b** (CA-T3b, `independent-review-pbt3b-check.md`, §1 a §12, con su encargo), sobre el contrato en seco y la v4.5:
- **la primera ronda:** EMITIBLE, B 0 / I 0 / M 4, sin importantes y sin decisiones nuevas del Owner;
- **la §12, sobre el seco r2** (el SHA del sustituto de nc2 v2 fijado y su custodia): EMITIBLE, B 0 / I 0 / M 1;
- **el resultado que lleva el contrato:** `EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner; recomprobacion v4.5: EMITIBLE, B 0 / I 0 / M 1`.

**Las menores de CA-T3b y su destino:**
- **M-01:** el bloque citado de `PBT3x-C11` del §6.0 sigue diciendo que C11 de PB-T3b es el de PB-T3a con su prefijo, y `PBT3B-C11` se aparta a propósito, por F4B-E40 y F4B-E43, con cinco cambios. Va a una v4.6 del paquete, solo de texto, antes de PB-T3c.
- **M-02:** tres entradas de la tabla de RR-3 de PB-T3b (las cuatro líneas de CT-10, las once `<Fn> …` de CT-01 (ii) y la línea cortada de CT-06 (ii)) no se enumeran en la fila y se derivan del §5.3; el doble `PushBackFullWriteSupport.cs` se puede reparar y no tiene literales. Hasta la v4.6, la cadena usa la derivación del generador, con la lista en UTF-8. Va a la misma v4.6.
- **M-03:** tres textos de PB-T3a pasan a PB-T3b sin adaptar: «las autopruebas de las tres guardas» de la `ExpectedEvidence`, la UI de paridad de PB-T3a que no vuelve a los `RequiredTests`, y el GREEN previsto de Core (19 128, o 19 124 si la TRX cuenta la `Theory` S-2 como una fila). Ninguno impide emitir. Va a la misma v4.6.
- **M-04:** el sustituto de nc2 v2 entraba solo como script, con el SHA por rellenar. **Resuelto en el seco r2:** el SHA `c033e96c`… fijado, y sus límites declarados y su revisión como autoridades.
- **M-05:** la custodia de la v2 dejaba fuera los ayudantes que citan sus notas y los guiones de su revisión. **Resuelto en esta custodia:** entran redactados, y `CUSTODY-NOTE.md` dice lo que queda fuera y por qué.

**Para el prompt del Worker** (los dos avisos de CA-T3b; los mete la cadena, no son del contrato):
- **`Transaction`:** ningún archivo de `Pilot/` puede contener la palabra, ni en un comentario (UpdateGuard:118-123; IntegrationGuard:81).
- **Los cinco tipos del modelo:** `IPushBackPilotViewEraser.cs` no puede nombrar ninguno de los cinco tipos de CT-PBT3A-12, ni en un comentario: la guarda de PB-T3a solo deja nombrarlos a `Draft/`, PP y UR.

**El generador se cambió en su sitio** para el SHA del sustituto de nc2 v2 y su custodia, y otra vez para `BOUNDED_RESULT`. No queda copia de las versiones anteriores: `generator-change-r2.md` lo explica con las pruebas por comportamiento de la §12.

**La remedición sobre el cierre de PB-T3a** (`c40eea4a`, F4B-E43), en `pbt3b-anchors-out.txt` y en R-21 del registro de mediciones:
- `git diff <cierre de PB-T3a> <BaseSha> -- src tests` sale vacío;
- las anclas de PP, UR y de las guardas, S-1 a S-6 y la lista de `tests/RackCad.Tests/Workspace/` (76 de 79);
- los recuentos y los alcances: 99 entradas de `src/` en el `ForbiddenWriteScope`, una más que las 98 previstas por `Draft/`, la carpeta que creó PB-T3a;
- el censo de los diseños de las clases de F4 que llegan al plan: ninguno bloqueado;
- el barrido de las guardas sobre el cierre: sin colisiones con PP, UR ni la interfaz nueva;
- la línea base de la CI: la corrida `push` `44668584248` sobre `6919e17a`, el GREEN de PB-T3a, con el `src` y los `tests` del cierre: Core 19 107 y UI 2 451, con 21 omitidas; es la prevista en el §6.1 del paquete.

No hay cambios materiales.

**El contrato:**
- **Qué es PB-T3b:** generalizar en su sitio el plan (PP) y la corrida (UR) de la actualización del Workspace de Push Back para el modelo del piloto, conectado, y el modelo completo de PB-T3a, sin conectar, con la barrera, el borrado de laterales obsoletos y la interfaz nueva para borrar. El RED es el esqueleto inerte del API nuevo, la interfaz, las pruebas nuevas y los `Skip` de D-1; el GREEN toca solo PP y UR.
- **`UNIT_CHANGE`:** PP y UR (F4B-E25).
- **P-01:** la Snapshot K (F4B-E37).
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.
## F4B-E45. Parada de F4B-PBT3B-FULL-WRITE-RUN en el intento 1 (PBT3B-C02 y PBT3B-C07), decisión del Owner (Renombrar los métodos nuevos) y reemisión con el paquete de PB-T3 v4.6

**Fecha:** 2026-11-20.

**La parada:** el Worker `R20261120T055235Z-a45b` (delegación `R20261120T054837Z-814c`, intento 1) paró con **PBT3B-C02 y PBT3B-C07 antes del commit RED**, sin commits ni push. HEAD siguió en `eab27a9e` y el árbol quedó limpio.
- **Lo que falla:** con el esqueleto exacto del §6.2 de la v4.5, `RackCad.Tests` no compila. Hay dos CS0121, de llamada ambigua, en una prueba existente de F4 que el contrato prohíbe editar:
  - `WorkspacePilotUpdateRunTests.cs:669`, `PushBackPilotUpdateRun.Execute(null!, setup.Request, setup.Port)`;
  - `WorkspacePilotUpdateRunTests.cs:799`, `PushBackPilotUpdateRun.ComparePost(plan, null)`.
- **La causa, un defecto del paquete:** las firmas existentes no cambian, pero las sobrecargas nuevas del modelo completo, con el mismo nombre y un parámetro de tipo referencia, vuelven ambiguo un `null`. El barrido de guardas del §4.1 de la v4.5 solo miraba el texto.
- **El Worker** no eligió ninguna salida: editar esas dos líneas habría sido C02, y cambiar el API nuevo, C13. Dejó `blocked-skeleton.patch`, el esqueleto probado sin aplicar, que no es autoridad.

**La decisión del Owner** (`F4B-DRAFTS/coordinator/owner-pbt3b-overload-decision.md`): «Renombrar los métodos nuevos (Recomendado)». Los nombres que fija la sesión son `ExecuteFull` y `ComparePostFull`, con la misma firma, y, en una nota posterior de la misma decisión, `PrepareFull`: es el mismo remedio, aplicado al tercer choque que encontró el barrido ampliado. Ninguna firma ni prueba existente cambia.

**El paquete v4.6** (V46-06 a V46-10, con el borrador V46-01 a V46-05):
- `ExecuteFull` y `ComparePostFull`, y `PrepareFull` para la sobrecarga completa de `Prepare`: el barrido ampliado encontró que `WorkspacePilotPrepareTests.cs:2156` exige con `Assert.Single` un solo `Prepare` en `PushBackPilotUpdatePlanner`, y fallaría al ejecutar. `Precheck` sigue como sobrecarga, con su porqué (V46-08);
- el barrido del §4.1 se amplía a la resolución de sobrecargas y a la reflexión (`overload_sweep.py`, con el control positivo de los dos CS0121), y los nombres nuevos contra los literales de las guardas (`name_check_v46.py`): sin choque con los tres nombres;
- la lección del §5.0 y R-22 del registro;
- la precondición 12 es la reemisión.

**La compilación de comprobación** de la sesión, sobre copias exportadas de `eab27a9e`, fuera del worktree (`F4B-DRAFTS/pbt3/compile-check-pbt3b/`):
- con el esqueleto del intento 1 tal cual, los dos CS0121; con `ExecuteFull` y `ComparePostFull`, Core-tests, UI-tests y el Plugin sin errores;
- **la segunda, con los tres nombres:** los tres proyectos sin errores y la suite completa de Core, 19 107 de 19 107. Su control negativo, con `Prepare` en lugar de `PrepareFull`, falla exactamente la prueba de reflexión, 1 de 91 en `WorkspacePilotPrepareTests`.

**Su comprobación acotada** (la §13 de CA-T3b, la recomprobación de la v4.6): `EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner; recomprobacion v4.5: EMITIBLE, B 0 / I 0 / M 1; recomprobacion v4.6: EMITIBLE, B 0 / I 0 / M 2`. Da **EMITIBLE, B 0 / I 0 / M 2**, sin decisiones nuevas del Owner:
- **M-06,** el cierre de la «Segunda comprobación» de `compile-check.md`, desfasado: lo corrigió la sesión antes de la custodia, y se custodia la versión corregida;
- **M-07,** redactar el §5.0 y la precondición 12 como «ejecutar la suite completa de Core» sobre la copia, en lugar de «compilar con las pruebas que hacen reflexión»: **no se aplica a la v4.6**, porque movería su SHA-256 y pediría otra recomprobación. Lo cubre la evidencia de la segunda comprobación (la suite completa de Core en verde), y pasa a la revisión del paquete para PB-T3c.

**La comunicación al Owner del tercer nombre:** `PrepareFull` aplica el remedio que eligió el Owner a un tercer choque; no es una decisión nueva (la nota posterior de la decisión, y la §13.7 de CA-T3b). La sesión se lo comunica al Owner en el chat antes de la delegación nueva, para que pueda objetar a tiempo.

**La reemisión:** el contrato de `F4B-PBT3B-FULL-WRITE-RUN` se emite de nuevo con la v4.6 y sustituye al de `eab27a9e` (F4B-E44), que queda en la historia de Git.
- **La corrección va en el intento 2 de 3:** la parada del intento 1 consume intento (16.8). `FailureClass` es `StopCondition`; `ChainRedSha` es null y `ChainRedFiles` está vacío, porque el intento 1 no hizo RED. El RED es el entero de la v4.6.
- **El análisis de la sesión** está custodiado en `F4B-PBT3B-FULL-WRITE-RUN/1/R20261120T055235Z-a45b/analysis.md` (sha256 `27d3ff05baad1cb59578a38d8e7d47f524d35321749efa4d827c62c63d7434c1`): C-1, los nombres; C-2, el RED entero; C-3, el GREEN; C-4, la reparación del RED, que sigue disponible.
- **La carpeta del Worker del intento 1** se custodia al lado, con su manifiesto: lo que la sesión leyó para la parada, sin binarios.
- **Lo que no cambia:** el diseño, D-1, las listas cerradas de CT, los `MinSelected`, el GREEN previsto y las pruebas existentes.

## F4B-E46. Cierre de F4B-PBT3B-FULL-WRITE-RUN, con la decisión del Owner sobre nc3

**Paquete de cierre:** `F4B-PBT3B-FULL-WRITE-RUN/task-close-package.md`.

**La cadena:**
- **La emisión** (F4B-E44): X `05b6f04e` y BASE `eab27a9e`, con la v4.5.
- **El intento 1:**
  - la planificación `814c` se aceptó, con nc4 `1ca3`;
  - el Worker `a45b` paró antes del RED con PBT3B-C02 y PBT3B-C07, sin commits. Con el esqueleto de la v4.5, `RackCad.Tests` no compilaba: dos CS0121 en una prueba existente de F4.
- **La reemisión y la corrección en el intento 2** (F4B-E45): el Owner eligió «Renombrar los métodos nuevos».
  - Con la v4.6, los nombres son `ExecuteFull`, `ComparePostFull` y `PrepareFull`. El commit X es `2bb3b759`, con el análisis, y el BASE es `c7441653`.
  - La compilación de comprobación de la sesión dio Core 19 107 de 19 107.
  - La §13 de CA-T3b dio EMITIBLE 0/0/2.
- **La planificación `ba4a`** se aceptó al primer lanzamiento, con `CorrectionOf` exacto, nc4 `0d48` y 17 criterios.
- **El Worker `a9e0`** (sonnet/high) dio IMPLEMENTATION_COMPLETE, sin reparación del RED.
  - El RED `9fda277e` lleva el esqueleto, la interfaz de borrado, las cuatro clases nuevas con el doble y los seis `Skip` de D-1.
  - El GREEN `c7582785` toca solo PP y UR.
  - La medición de INV-PBT3B-15 da exactamente la variante (A).

**CI:**
- RED `44704503614`: tests en failure; ui-tests y build-ui en success; build-plugin omitido;
- GREEN `44707482956`: 4/4, Core 19 124 (6 omitidas, las de D-1) y UI 2 451 (2 430 pasadas y 21 omitidas).

**Revisiones:**
- la directa, sin hallazgos bloqueantes ni importantes;
- la independiente, ACEPTABLE CON MENORES 0/0/9, con dos hallazgos para el Owner solo informativos.

**Verificación** `R20261120T091621Z-2b95`: **EXECUTION_VERIFIED** 14/14 al primer lanzamiento.

**Controles:**
- **nc1** `R20261120T092052Z-0d2f`: CONTROL NOT PASSED, con la firma exacta de PD-F4B-01. Su sustituto revisado (`41440edc`, `--task PBT3B`, `R20261120T093907Z-71d6`) dio PASS: **nc1 = SATISFIED_UNDER_PBT3B_NC1_CASCADE_SUBSTITUTE**. Es su primera aplicación.
- **nc2** `R20261120T092518Z-aea7`: CONTROL PASS (Scope, STOP). El sustituto ampliado de nc2 no hizo falta.
- **nc3** `R20261120T093218Z-fd73`: CONTROL NOT PASSED.
  - FreeText detectó la mutación, pero antes falló Tests, que en la verificación real había pasado.
  - Su Controller leyó «19 128 resultados» en ExpectedEvidence[14] y no aceptó los 19 124 de la CI.
  - La sesión midió por clase (`F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/`): la Theory S-2 con `Skip` cuenta como una fila en lugar de cinco. 19 107 − 4 + 21 = 19 124 es la rama «+ 6 omitidas» que el propio contrato admite, y el 19 128 solo vale en la rama «+ 10».
  - Ningún sustituto autorizado cubre nc3.

**La decisión del Owner** (`F4B-DRAFTS/coordinator/owner-pbt3b-nc3-decision.md`, literal): «Aceptar por disposición (Recomendado)».
- **PB-T3b:** `nc3 = SATISFIED_BY_OWNER_DISPOSITION`.
- **Desde PB-T3c,** una evidencia esperada que dependa de cómo cuente la TRX lleva el total exacto de cada rama.

**Resultado:** **F4B-PBT3B-FULL-WRITE-RUN = CLOSED / PASS WITH LIMITATIONS** sobre `c7582785`, con attempts 2 de 3. El plan y la corrida de la actualización del Workspace de Push Back sirven al piloto y al modelo completo, sin conectar este último. Llevan la barrera, el borrado de laterales obsoletos y la interfaz para borrar.

**Siguiente:** la emisión de PB-T3c por F4B-E35 (pregunta 3), tras revisar el paquete sobre este cierre (los marcadores posteriores al cierre de PB-T3b y M-07 de CA-T3b) y su comprobación acotada.

## F4B-E47. Emisión de F4B-PBT3C-FULL-WRITE-ERASE (PB-T3c), con el paquete de PB-T3 v4.7

**Fecha:** 2026-11-20.

**Autoridad de emisión:** la autorización condicional del Owner, F4B-E35, pregunta 3: «Sí, con esa condición (Recomendado)». Cubre PB-T3a, PB-T3b y PB-T3c, en ese orden y cada una tras el cierre de la anterior.
- **Las condiciones que exige:**
  - la comprobación acotada de la subtarea da 0 bloqueantes y 0 importantes sin resolver;
  - la remedición sobre el cierre anterior, el de PB-T3b, no cambia nada material.
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3 o un efecto material sin medir. No se da ninguno.

**El cierre de PB-T3b** (F4B-E46, `fe1c67f4`):
- PB-T3b CLOSED / PASS WITH LIMITATIONS sobre `c7582785`, en el intento 2 de 3, tras la reemisión con la v4.6 (X `2bb3b759` y BASE `c7441653`);
- RED `9fda277e`, sin reparación;
- verificación 14/14;
- nc1 por el sustituto de PD-F4B-01, nc2 PASS y nc3 por disposición del Owner;
- CI del GREEN `44707482956`, 4/4: Core 19 124, con 6 omitidas (las de D-1), y UI 2 451, con 21 omitidas.

**La lección del cierre de PB-T3b** (F4B-E46, `F4B-DRAFTS/coordinator/owner-pbt3b-nc3-decision.md`). Una evidencia esperada que dependa de cómo cuente la TRX lleva el total exacto de cada rama. La v4.7 la aplica:
- CT-PBT3C-01 a 05 son un `[Fact]` cada una;
- la `ExpectedEvidence` de la CI da los recuentos del RED y del GREEN en su única rama, sin alternativa:
  - **RED:** Core 19 129 = 19 119 `Passed` + 4 `Failed` + 6 `Skipped`;
  - **GREEN:** Core 19 129 = 19 123 + 0 + 6;
  - **UI, en los dos:** 2 451 = 2 430 + 21.

**D-2, la premisa «el escritor nunca borra»:** cambia, y lleva su registro declarado **sin `Skip`**. La entrada `## PB-T3c` de `F4B-DRAFTS/superseding-characterization.md` va en esta emisión, y recoge:
- la autoridad, D-PB-04 y OB-04.1;
- la guarda que sigue en verde, UpdateGuard:182-212, con su comentario :188;
- las guardas nuevas, CT-PBT3C-01 a 04;
- los dos comentarios de WR que cambian, :26 y :225.

No se sustituye ni se debilita ninguna guarda; D-1 no cubre el escritor.

**El registro declarado de PB-T3b:** el cierre (F4B-E46) dejó sin rellenar su tabla de ejecución en `F4B-DRAFTS/superseding-characterization.md`. El commit X de esta emisión la completa con lo que ya está custodiado en `fe1c67f4`:
- los SHA del RED y del GREEN, con sus CI;
- la medición de INV-PBT3B-15, conforme con la variante (A).

Cambian solo esas cinco líneas. El texto exacto y sus fuentes están en `F4B-DRAFTS/pbt3/superseding-pbt3b-close-fill.md`.

**Los controles** (PBT3C-C11), con las decisiones del Owner posteriores a la v4.5:
- **nc1:** la firma PD-F4B-01, con su sustituto `PBT3x-NC1-CASCADE-SIGNAL-01` (F4B-E40), revisado y aplicado ya en PB-T3b. Se usa con `--task PBT3C`.
- **nc2:** el sustituto ampliado, la v2 de `PBTx-NC2-EXACT-WHITELIST-F4B-01` (F4B-E43). Su lista ya tiene `PBT3C`, y la primera entrada de la `ExpectedEvidence` es, byte a byte, la de PB-T3a.

**La reparación del RED** (F4B-E35, pregunta 4): la regla RR del Owner, con la lectura de PB-T3c.
- PBT3C-C07 lleva su excepción, y PBT3C-C18 cubre una reparación que rompe la regla.
- La cadena RR-6 va en INV-PBT3C-02.
- Una reparación no toca WR ni la fachada.

Una reparación no gasta intento (RR-7).

**Antes de emitir** (la precondición 13 de la v4.7; V47-02 y V47-05):
- **El barrido de sobrecargas, de reflexión y de nombres** (T3C-1), sobre `fe1c67f4`, salió sin colisión: `overload-sweep-pbt3c-fe1c67f4.txt` y `name-check-pbt3c-fe1c67f4.txt`.
- **La compilación de comprobación de la sesión,** con el esqueleto de la fachada (`pbt3c-skeleton.patch`) sobre una copia exportada de la base, fuera del worktree: Core-tests y UI-tests en Debug y el Plugin en Release (con -o), 0 errores; el tipo y el método del esqueleto en el ensamblado del Plugin; la suite completa de Core sobre la copia exportada de fe1c67f4, 19 124 = 19 118 pasadas + 0 fallidas + 6 omitidas (S-1 a S-6 de D-1). Su custodia está en `F4B-DRAFTS/pbt3/compile-check-pbt3c/`.

**La comprobación acotada de PB-T3c** (CA-T3c, `independent-review-pbt3c-check.md`, con su encargo), sobre el contrato en seco y la v4.7. El resultado que lleva el contrato es `EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner; remedicion sobre fe1c67f4 sin cambios materiales; precondicion 13 cumplida: Core 19124 = 19118 + 0 + 6; recomprobacion v4.7: EMITIBLE, B 0 / I 0 / M 1`.
- **Su M-01 decidió la duda 1 del generador: incluir.** La celda «Y los de §6.2 sin `ExpectRed`» de la v4.7 se lee «con `ExpectRed` false». Así, `PushBackFullWrite` (13) y `PushBackPilotUpdateEquivalenceGuardTests` (6) entran en los `RequiredTests`, que pasan de 34 a 36. La v4.7 no cambia.
- **Sus menores M-02 y M-05** van a `docs/ideas-futuras.md` en el cierre de PB-T3c. M-02 es la nota de V47-06 sobre PBT3B-IR-M01; M-05 es el límite del corte de argumentos del barrido, con el mismo «5» de `overload-sweep-eab27a9e.txt` en la custodia de PB-T3b.

**La remedición sobre el cierre de PB-T3b** (`fe1c67f4`, F4B-E46), en `pbt3c-anchors-out.txt` y en R-23 del registro de mediciones:
- **La base:** `src/` y `tests/` son los del GREEN de PB-T3b, y WR está igual que en la emisión de PB-T3b.
- **Los `MinSelected` de las guardas del escritor:** 86 y 123, los `Passed` de la TRX del GREEN de PB-T3b. Lo heredado no baja del contrato reemitido de PB-T3b.
- **El barrido de las guardas:** de 92 a 94 clases que leen código; 9 coincidencias nuevas y genéricas, ninguna perdida; sin colisión.
- **La fila de CT-05 de la tabla de RR-3,** enumerada (V47-04).
- **La línea base de la CI:** la corrida `push` `44707482956` sobre `c7582785`, el GREEN de PB-T3b, con el `src` y los `tests` del cierre: Core 19 124 (19 118 pasadas y 6 omitidas, las de D-1) y UI 2 451 (2 430 pasadas y 21 omitidas); es la del §6.3 del paquete.

No hay cambios materiales.

**El contrato:**
- **Qué es PB-T3c:** dar al escritor de la actualización del Workspace de Push Back la capacidad de borrar una vista obsoleta dentro de la única transacción de MUTATE. Lo hace por una fachada caller-owned nueva, `RackViewBlockEraser.EraseInTransaction`, y `EraseUnit` en WR.
  - **El RED:** el esqueleto inerte de la fachada y las dos clases de prueba nuevas.
  - **El GREEN:** toca solo WR y la fachada.
- **`UNIT_CHANGE`:** WR (F4B-E25).
- **P-01:** la Snapshot K (F4B-E37).
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.

## F4B-E48. Cierre de F4B-PBT3C-FULL-WRITE-ERASE

**Paquete de cierre:** `F4B-PBT3C-FULL-WRITE-ERASE/task-close-package.md`.

**La cadena:**
- **La emisión** (F4B-E47): X `e3c293b1` y BASE `7dfc7c21`, con la v4.7. CA-T3c dio EMITIBLE 0/0/4 y su recomprobación 0/0/1. La compilación de comprobación de la sesión dio Core 19 124 = 19 118 + 6 omitidas, con su control negativo.
- **La planificación** `R20261120T134718Z-68e5` se aceptó al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 14 criterios. nc4 `R20261120T134930Z-9c1a`.
- **El Worker** `R20261120T135026Z-80e5` (sonnet/high) dio IMPLEMENTATION_COMPLETE, sin reparación del RED, sin desviaciones y sin preguntas abiertas.
  - El RED `e0940a0e` lleva la fachada del parche, igual byte a byte, y las dos clases de guardas.
  - El GREEN `938a35f0` toca solo WR y la fachada.

**CI** (los totales exactos de su única rama, F4B-E46):
- RED `44754080713`: tests en failure; ui-tests y build-ui en success; build-plugin omitido. Core 19 129 = 19 119 + 4 Failed + 6 Skipped; UI 2 451 = 2 430 + 21.
- GREEN `44757675821`: 4/4. Core 19 129 = 19 123 + 0 + 6, las de D-1; UI 2 451 = 2 430 + 21.

**Revisiones:**
- la directa, conforme;
- la independiente, ACEPTABLE CON MENORES 0/0/6. Añade tres hallazgos para el Owner que no son defectos de la entrega, sino riesgos del borrado real para el Smoke-PB: OWNER-1, las referencias de la vista en espacio papel o dentro de bloques del usuario se borran sin aviso; OWNER-2, la purga de las anidadas es de un nivel; OWNER-3, bloques dinámicos.

**Verificación** `R20261120T150458Z-e403`: **EXECUTION_VERIFIED** 14/14 al primer lanzamiento, sin reparación (`rr_check` PASS).

**Controles,** los tres con CONTROL PASS ordinario, sin sustitutos:
- nc1 `R20261120T151131Z-8a51` (Identity, STOP);
- nc2 `R20261120T151911Z-670e` (Scope, STOP);
- nc3 `R20261120T152519Z-2017` (FreeText, REWORK).

El «limit reached» de los eventos de nc1 y nc2 es el texto de la parada S-11 del contrato.

**El registro declarado de D-2** (`F4B-DRAFTS/superseding-characterization.md`): este cierre completa la tabla de ejecución de PB-T3c y la pasa a EJECUTADA. Cambian cinco líneas, con el texto y la fuente en `F4B-DRAFTS/pbt3/superseding-pbt3c-close-fill.md`.

**La custodia, un ajuste declarado del filtro de nombres:** `forceValidity`, el parámetro de `GetBlockReferenceIds` de la API de AutoCAD que aparece en la entrega y en las revisiones, lleva dentro, sin distinguir mayúsculas, uno de los nombres de la lista de FORBIDDEN. El patrón de nombres no pide palabra completa, así que daba un falso positivo. En este cierre solo ese nombre pasa a palabra completa (con `\b` a los dos lados), como el `(?![*])` del cierre de PB-T0. El resto del filtro no cambia, y ningún archivo se excluye por ello.

**Resultado:** **F4B-PBT3C-FULL-WRITE-ERASE = CLOSED / PASS WITH LIMITATIONS** sobre `938a35f0`, con attempts 1 de 3. El escritor de la actualización del Workspace borra las vistas obsoletas del plan dentro de la única transacción de MUTATE, por la fachada caller-owned `RackViewBlockEraser.EraseInTransaction`. La limitación es la de siempre: sin AutoCAD, el borrado real en el dibujo queda para el Smoke-PB (D-PB-09). **PB-T3 queda completa.**

**Siguiente:** el paquete de PB-T4 («Editar todo»), en revisión. Su emisión no está autorizada: va al Owner en una sola vuelta, con la lista de pruebas sustituidas medida sobre este cierre.

## F4B-E49. Decisiones del Owner sobre PB-T4: el piloto, la presentación, las pruebas del panel, la emisión, y la retirada y la lista de PB-T4b

**Fecha:** 2026-11-20. **Registros literales,** en este commit, en `F4B-DRAFTS/coordinator/`:
- `owner-pbt4-decisions.md`: las cuatro preguntas sobre PB-T4. La sesión las hizo en una sola vuelta, tras la revisión independiente del paquete de PB-T4 (`F4B-DRAFTS/pbt4/independent-review-v1.md`, con sus §13 a §15). Los números eran recuentos por texto sobre el cierre de PB-T3c (`7f0d9aa3`, F4B-E48).
- `owner-pbt4b-retire-list-decisions.md`: las dos preguntas sobre PB-T4b, después. Las redactó la revisión, en su §16, porque con las listas exactas de la v4.1 la lista de PB-T4b superaba la cota que había visto el Owner y cambiaban dos premisas de sus respuestas. La sesión las hizo en una sola vuelta.

**Las cuatro respuestas sobre PB-T4, literales:**
- **Pregunta 1, el piloto:** «Retirarlo (Recomendado)». Se retira lo que solo usaba el panel; queda la carga dentro del lector, que el modelo completo usa para AUTH-13. Sus pruebas propias y E-3, E-5 y E-6 de D-1 se sustituyen con registro. Cuándo se retira lo fija la segunda tanda: en PB-T4d.
- **Pregunta 2, la presentación:** «Por grupos (Recomendado)». «Conservar mi borrador» presenta los cambios por grupos.
- **Pregunta 3, las pruebas del panel de F4:** «Saltar y duplicar (Recomendado)». Las 428 se saltan y se duplican con un kit nuevo, como en D-1, con un guion que comprueba que cada copia solo cambia el kit. No hay excepción a la regla de no editar pruebas ni ayudantes.
- **Pregunta 4, la emisión:** «Sí, con condiciones (Recomendado)». Es la emisión condicional de PB-T4a, PB-T4b y PB-T4c, en ese orden y cada una tras el cierre de la anterior.
  - **Las condiciones:** la comprobación acotada da 0 bloqueantes y 0 importantes sin resolver; la remedición no cambia nada material; y la lista emitida es la que vio el Owner o una parte de ella.
  - **Vuelve al Owner** por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista. Si eso pasa en mitad de una subtarea, el Worker para.

**Las dos respuestas sobre PB-T4b, literales:**
- **Pregunta 1, la retirada:** «Retirar tras PB-T4c (Recomendado)». El código del piloto no se retira en PB-T4b: queda sin uso hasta una subtarea propia, **PB-T4d**, que va después de PB-T4c.
  - PB-T4d sustituye las 139 pruebas de la retirada y borra con registro las 55 originales saltadas y los 29 ayudantes, que para entonces ya no corren.
  - **PB-T4d vuelve al Owner con su lista antes de emitirse.** La emisión condicional de la pregunta 4 no la cubre.
- **Pregunta 2, la lista de PB-T4b:** «Sí, esa lista (Recomendado)». La lista exacta de PB-T4b son **470 pruebas,** y el Owner la toma como la que vio:
  - las 428 del panel: 375 copias que comprueba el guion y 53 equivalentes escritos a mano, con su registro;
  - 15 guardas de texto;
  - 27 pruebas de la interfaz de «Conservar».

  PB-T4b se emite con esa lista por la emisión condicional, si se cumple el resto de la condición. Una prueba que falle fuera de ella vuelve al Owner.

**Lo que aplica el paquete:** la v4.3 (`docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md`) quita las ramas no elegidas y guarda las listas en `F4B-DRAFTS/pbt4/`:
- las de sustituciones (`pbt4-substitution-lists.md`);
- la exacta de PB-T4b, fijada por su sha256 (`pbt4b-substitution-list.tsv`);
- y la de PB-T4d (`pbt4d-retire-list.tsv`).

**PB-T4a no cambia con la segunda tanda** (su lista es de 1), ni PB-T4c (de 2): las dos siguen con la emisión condicional de la pregunta 4.

## F4B-E50. Emisión de F4B-PBT4-FULL-READ-PATH (PB-T4a), con el paquete de PB-T4 v4.3

**Fecha:** 2026-11-20.

**Autoridad de emisión:** la decisión del Owner F4B-E49, pregunta 4: «Sí, con condiciones (Recomendado)». Cubre PB-T4a, PB-T4b y PB-T4c, en ese orden y cada una tras el cierre de la anterior.
- **Las condiciones que exige:**
  - la comprobación acotada de la subtarea da 0 bloqueantes y 0 importantes sin resolver;
  - la remedición sobre el cierre anterior, el de PB-T3c, no cambia nada material;
  - la lista emitida es la que vio el Owner o una parte de ella. La de PB-T4a es una sola: CT-PBT3A-12.
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista. No se da ninguno.

**El cierre de PB-T3c** (F4B-E48, `7f0d9aa3`): PB-T3c CLOSED / PASS WITH LIMITATIONS sobre `938a35f0`, en el intento 1 de 3; PB-T3 queda completa. CI del GREEN `44757675821`, 4/4: Core 19 129 = 19 123 + 6 omitidas (las de D-1), y UI 2 451 = 2 430 + 21.

**Qué es PB-T4a:** la lectura de la intención de edición del Workspace trae también el modelo completo de PB-T3 sobre el mismo escaneo (`FromReads`), y las cinco relecturas de un borrador abierto van al origen del borrador por su texto (A-T3-8).
- **Es un cambio observable declarado:** las relecturas dejan de ir al `Primary`.
- **La firma de `RackPilotIntentAdapter.Read` no cambia** (EIG:139-141): RIA gana `AtOrigin`, que NH llama.
- **`AtOrigin` solo llama a `PushBackOriginByText.ReadAt`** (B-08, la §16 de la revisión): las dos relecturas puras sobre el escaneo ya tomado van en `Draft/`, porque EIG:350-364 admite un solo `PushBackPilotIntentReader.Read(` y un solo `new PushBackPilotIntentScan(` en todo RIA.
- **`UNIT_CHANGE`:** WEI, WHC, RIA y NH (F4B-E25).

**La única sustitución, con registro:** CT-PBT3A-12 (`CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes`), con un `Skip` de motivo literal; la sustituye CT-PBT4A-06. La entrada «## PB-T4a» de `F4B-DRAFTS/superseding-characterization.md` va en esta emisión, y su tabla de ejecución se completa al cerrar.

**El total exacto de su única rama** (F4B-E46): PB-T4a añade ocho `[Fact]` y salta uno.
- **RED:** Core 19 137 = 19 125 + 5 `Failed` (CT-PBT4A-01 a 04 y 07) + 7 `Skipped`.
- **GREEN:** Core 19 137 = 19 130 + 0 + 7.
- **UI, en los dos:** 2 451 = 2 430 + 21.

**Los controles** (PBT4A-C11): nc1 con la firma PD-F4B-01 y su sustituto (F4B-E40), y nc2 con el sustituto v2 (F4B-E43), los dos con `--task PBT4`. La primera entrada de la `ExpectedEvidence` es, byte a byte, la del contrato de PB-T3c (`F4B_EXACT_EVIDENCES` del sustituto v2).

**La reparación del RED** (F4B-E35, pregunta 4): la regla RR, con la lectura de PB-T4a. Una reparación no toca `src/` (el esqueleto) ni `PushBackDraftWriteGuardTests.cs` (el `Skip`).

**Antes de emitir** (la precondición 6 del paquete):
- **el barrido de sobrecargas, de reflexión y de nombres** sobre `7f0d9aa3`, sin colisión;
- **el recorrido de las guardas por marcas** sobre el texto literal del GREEN (§4.1: 1 026 pruebas en 46 clases, de las que solo cae la de su lista), la ampliación de CA-T4a (97 archivos, ninguno cae) y las aserciones traducidas (`measure_pbt4_v43-out.txt`, 36 de 36);
- **la compilación de comprobación de la sesión,** sobre copias exportadas de la base, con el esqueleto exacto (`pbt4a-skeleton.patch`) y con el `src/` del GREEN literal (`apply_green_pbt4a.py`): Core-tests y UI-tests en Debug y el Plugin en Release (con -o), 0 errores y ningun aviso CS (solo los MSB3277 locales de las referencias de AutoCAD); los tipos de los dos archivos nuevos del esqueleto en el ensamblado de Application; la suite completa de Core sobre la copia exportada de 7f0d9aa3, 19 129 = 19 122 pasadas + 0 fallidas + 7 omitidas (S-1 a S-6 de D-1 y CT-PBT3A-12), tambien con el GREEN literal del paquete; los filtros de UI del contrato, en verde; control negativo: sin el Skip y con un tipo del modelo nombrado en RIA, CT-PBT3A-12 falla. Su custodia está en `F4B-DRAFTS/pbt4/compile-check-pbt4a/`.

**Fe de erratas de la v4.3:** la precondición 8 dice «las versiones v1 a v4.1» y «los fragmentos de `v41/` y `v42/`», pero la custodia lleva de la v1 a la v4.2 y de `v41/` a `v43/`; y en §4.1, "CA-T4a leyó las 100 señaladas": releyó las que cuentan sobre un archivo entero o sobre un cuerpo que cambia, y aceptó las demás por grupos (CA-T4a, §11). Los dos textos se corregirán en la versión siguiente del paquete.

**La comprobación acotada de PB-T4a** (CA-T4a, `independent-review-pbt4a-check.md`, con su encargo), sobre el contrato en seco y la v4.3. El resultado que lleva el contrato es `EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; las importantes corregidas en el paquete v4.3 (I-01, la anotacion string? de PushBackOriginByText.cs: aviso CS8632 que rompe build-plugin); sin decisiones nuevas del Owner; remedicion sobre 7f0d9aa3 sin cambios materiales; recomprobacion v4.3: EMITIBLE, B 0 / I 0 / M 2`.

**El contrato:**
- **P-01:** la Snapshot K (F4B-E37).
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.

## F4B-E51. Cierre de F4B-PBT4-FULL-READ-PATH, con tres decisiones del Owner sobre la verificación, nc1 y el coste de la lectura

**Paquete de cierre:** `F4B-PBT4-FULL-READ-PATH/task-close-package.md`.

**La cadena:**
- **La emisión** (F4B-E50): X `27c13fab` y BASE `5aaacce3`, con el paquete de PB-T4 v4.3 y las decisiones del Owner de F4B-E49. CA-T4a dio EMITIBLE tras su recomprobación. La compilación de comprobación (precondición 6) dio 0 errores y 0 avisos CS, y Core 19 129 = 19 122 + 7, también con el GREEN literal del paquete.
- **La planificación** `R20261120T193416Z-a53f` se aceptó al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 18 criterios. nc4 es `R20261120T193630Z-735b`.
- **El Worker** `R20261120T193653Z-de54` (sonnet/high) dio IMPLEMENTATION_COMPLETE, sin reparación del RED, sin desviaciones y sin paradas.
  - El RED `214f8c75` lleva el esqueleto del parche, igual byte a byte, las tres clases nuevas y el `Skip` de CT-PBT3A-12.
  - El GREEN `faef7020` toca solo las seis rutas de `src/` del alcance.

**CI** (los totales exactos de su única rama, F4B-E46):
- RED `44810371492`: tests en failure; ui-tests y build-ui en success; build-plugin omitido. Core 19 137 = 19 125 + 5 Failed + 7 Skipped; UI 2 451 = 2 430 + 21.
- GREEN `44814418683`: 4/4. Core 19 137 = 19 130 + 0 + 7; UI 2 451 = 2 430 + 21.

**Revisiones:**
- la directa, conforme;
- la independiente, ACEPTABLE CON MENORES 0/0/7, con tres hallazgos para el Owner:
  - OWNER-01: el origen deriva al `Primary` tras una relectura sin coincidencia;
  - OWNER-02: el coste de la lectura;
  - OWNER-03: dos subcasos de la relectura con origen sin CT.

**La verificación**, en cuatro corridas (las tres primeras se custodian tal cual, como unidades propias):
1. **`R20261120T205531Z-611f`, lanzamiento no productivo.** El prompt, de 36 786 bytes, superaba el límite de la línea de órdenes de Windows, y Codex no arrancó (exit 126, sin invocación del modelo). La cadena bajó los prompts de la verificación y de los controles a menos de 30 000 bytes, con los bloques grandes en `inputs/` y citados por su sha256, y lleva ahora una guarda de tamaño en sus guiones previos. El transporte no cambió.
2. **`R20261120T211710Z-a468`, STOP Authority con S-12.** El Controller no admitió el generador del contrato custodiado en BASE, que no figura en Authorities («cannot be classified under 16.3»). El Owner eligió «Reverificar con aclaración (Recomendado)».
3. **`R20261120T212638Z-de08`, STOP Tests con PBT4A-C07.** Authority pasó. El Controller leyó como un hecho una frase del prompt heredada de todas las cadenas: «Una linea esperada que no se cumple en el GREEN, PBT4A-C07». El Owner eligió «Corregir la frase y reverificar (Recomendado)».
4. **`R20261120T213256Z-8d11`, EXECUTION_VERIFIED, 14/14 y sin hallazgos,** con la frase reescrita como condición explícita y la aclaración de 16.3 en `inputs/coordinator-disposition.md`.

El registro literal de las dos decisiones es `F4B-DRAFTS/coordinator/owner-pbt4a-verify-stop-decision.md`.

**Controles:**
- **nc1** `R20261120T214408Z-bdf3`: CONTROL NOT PASSED, con la cascada del SHA falso. Su sustituto de PD-F4B-01 (`R20261120T220533Z-749e`) dio FAIL en una propiedad, `e2_OtherFailCitesFakeSha`: el fallo de Tests no cita el SHA falso literalmente. Es la quinta variación de nc1 en F4B. El Owner eligió «Aceptar solo esta vez (Recomendado)», así que **nc1 = SATISFIED_BY_OWNER_DISPOSITION** (`F4B-DRAFTS/coordinator/owner-pbt4a-nc1-decision.md`). El sustituto no cambia.
- **nc2** `R20261120T214941Z-4cda`: CONTROL NOT PASSED, con la firma de PD-F4-01 (Contract antes que Scope). Su sustituto v2 (`R20261120T220537Z-60ab`) dio PASS: **nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B**.
- **nc3** `R20261120T215653Z-e244`: CONTROL PASS.

**OWNER-02, el coste de la lectura, medido.** Lo midió la revisión, sin AutoCAD, en copias exportadas de la base y del GREEN, con una prueba temporal que no entró en ningún commit (`F4B-DRAFTS/pbt4/read-cost-pbt4a/`).
- **El aumento por gesto en los fixtures mayores:** de +40 a +80 ms en `Begin`; de +35 a +86 ms en la relectura habitual; de +80 a +175 ms en la relectura con el origen encontrado.
- **De dónde sale:** casi todo es la materialización completa, que se hace dos veces cuando se encuentra el origen.
- **La decisión del Owner:** «Cerrar y optimizar en PB-T4b (Recomendado)» (`F4B-DRAFTS/coordinator/owner-pbt4a-cost-decision.md`). PB-T4a cierra con el coste como limitación conocida. PB-T4b elegirá el origen antes de leer, para materializar una sola vez, y el coste se volverá a medir allí y en el Smoke-PB.

**El registro declarado de CT-PBT3A-12** (`F4B-DRAFTS/superseding-characterization.md`): este cierre completa la tabla de ejecución de PB-T4a y la pasa a EJECUTADA. Cambian cinco líneas, con el texto y la fuente en `F4B-DRAFTS/pbt4/superseding-pbt4a-close-fill.md`.

**Resultado:** **F4B-PBT4-FULL-READ-PATH = CLOSED / PASS WITH LIMITATIONS** sobre `faef7020`, con attempts 1 de 3. La lectura de la intención del Workspace trae también el modelo completo sobre el mismo escaneo (`FromReads`), y las cinco relecturas de un borrador abierto van al origen del borrador por su texto (A-T3-8).

**Las limitaciones:**
- el coste de la lectura (OWNER-02, medido);
- el origen que deriva al `Primary` (OWNER-01);
- dos subcasos sin CT (OWNER-03);
- sin AutoCAD, la relectura real en el origen del dibujo queda para el Smoke-PB.

**Siguiente:** PB-T4b, con su paquete en v4.5 (la lista de 475 que vio el Owner y la optimización de la lectura), su remedición sobre este cierre, la precondición 6 y su comprobación acotada, por la emisión condicional de F4B-E49.

## F4B-E52. Emisión de F4B-PBT4-FULL-DRAFT-PANEL (PB-T4b), con el paquete de PB-T4 v4.9

**Fecha:** 2026-11-21.

**Autoridad de emisión:** la decisión del Owner F4B-E49, pregunta 4: «Sí, con condiciones (Recomendado)». Cubre PB-T4a, PB-T4b y PB-T4c, en ese orden y cada una tras el cierre de la anterior.
- **Las condiciones que exige, cumplidas:**
  - la comprobación acotada de la subtarea, CA-T4b, da 0 bloqueantes y 0 importantes sin resolver: su importante, I-01, está corregida en la v4.9, y su recomprobación da EMITIBLE, 0/0/3;
  - la remedición sobre el cierre anterior, el de PB-T4a, no cambia nada material: la precondición 6 corre sobre copias exportadas de `78a7cef3`;
  - la lista emitida es la que vio el Owner: las 478 de `pbt4b-substitution-list-478.tsv` (sha256 `8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b`), la de 470 de la segunda tanda más las 8 de la tercera, la quinta y la sexta.
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista. Cada prueba que cayó fuera de la lista volvió al Owner antes de emitir (la tercera, la quinta y la sexta tandas), y el coste de la lectura también (la séptima). No queda ninguna.

**El cierre de PB-T4a** (F4B-E51, `78a7cef3`): PB-T4a CLOSED / PASS WITH LIMITATIONS sobre `faef7020`, en el intento 1 de 3. CI del GREEN `44814418683`, 4/4: Core 19 137 = 19 130 + 7 omitidas, y UI 2 451 = 2 430 + 21.

**Qué es PB-T4b:** el borrador del panel del Workspace de Push Back pasa al modelo completo de PB-T3, con la lectura completa de PB-T4a y sin traducir estados.
- **«Actualizar»** escribe por `ExecuteFull`; **«Conservar mi borrador»** pide dos «Sí» (D-S2); la presentación de los cambios es «Por grupos» (la pregunta 2 de la primera tanda).
- **La lectura completa del `Primary` se materializa una sola vez** por lectura (D-S4, de la cuarta tanda).
- **`UNIT_CHANGE`:** WHC, WEI, FVM, PV, `NumericFieldValidation.cs` y RIA (F4B-E25).

**Las 478 sustituciones, con registro** («Saltar y duplicar», la pregunta 3 de la primera tanda): cada una con un `Skip` de motivo literal y su sustituta en una clase `<Clase>Full`, sin tocar la prueba original.
- **C-0:** 428, en 9 clases: 356 copias mecánicas y 72 equivalentes escritos a mano (D-S1), con cuatro `Theory` partidas por filas (D-S5);
- **C-5, las guardas por su letra:** 23;
- **UI:** 27.

La entrada «## PB-T4b» de `F4B-DRAFTS/superseding-characterization.md` va en esta emisión, por clase y por causa, con los motivos literales y las 28 copias sin cambio declaradas como tales. Su tabla de ejecución se completa al cerrar.

**El total exacto de cada rama** (F4B-E46):
- **GREEN:** Core 19 605 = 19 147 + 0 + 458 omitidas; UI 2 481 = 2 433 + 0 + 48.
- **RED:** los mismos totales y las mismas omitidas, con 15 + R_C = 306 `Failed` en Core y 3 + R_UI = 3 en UI. R_C = 291 y R_UI = 0 son los resultados de las clases `FullPanelCopy<Raiz>RedTests` de la partición, medidos en la precondición 6.

**Las siete tandas del Owner sobre PB-T4** (§8.5 de la v4.9), literales en `F4B-DRAFTS/coordinator/`:
1. `owner-pbt4-decisions.md` (F4B-E49): «Retirarlo», «Por grupos», «Saltar y duplicar» y «Sí, con condiciones»;
2. `owner-pbt4b-retire-list-decisions.md` (F4B-E49): «Retirar tras PB-T4c» y «Sí, esa lista»: la lista de PB-T4b, 470;
3. `owner-pbt4b-breaks-decision.md`, nuevo en esta emisión: «Añadirlas a la lista»: la lista pasa a 475;
4. `owner-pbt4a-cost-decision.md` (F4B-E51): «Cerrar y optimizar en PB-T4b»: la materialización única, D-S4;
5. `owner-pbt4b-list-477-decision.md`, nuevo: «Añadirlas a la lista»: 477, con CT-PBT4A-03 y CT-PBT4A-05;
6. `owner-pbt4b-list-478-decision.md`, nuevo: «Añadirla a la lista»: 478, con WHHG `NoneOfThe30…`;
7. `owner-pbt4b-cost-decision.md`, nuevo: «Darlo por cumplido»: el coste de la lectura de la precondición 6, con dos de tres corridas que cumplen todo.
   - **Su nota posterior,** de la sesión, va tras «Lo que queda decidido» y no forma parte de la pregunta ni de la respuesta (CA-T4b M-07). Recoge el argumento de dentro de la primera corrida, que en el GREEN no hay ninguna causa y que una sonda dedicada debería intercalar las dos órdenes.
   - La sesión se lo informa al Owner con esta emisión, sin repetir la pregunta: la nota apoya la opción elegida.

**Las decisiones de diseño de la sesión,** que no son del Owner (`F4B-DRAFTS/pbt4/session-pbt4b-design-decisions.md`, sha256 `8b18add602766c705260e976e3566b151e13567ee3f2b40a0c0ff12bd193e934`):
- **D-S1:** las copias mecánicas que no pueden pasar en el GREEN pasan a equivalentes escritos a mano, con su registro y su texto literal; la lista no cambia por eso. **Aplicada en la v4.8 a dos copias de WPP.**
- **D-S2:** «Conservar mi borrador» con dos «Sí», el flujo compatible con las guardas del lector.
- **D-S3:** lo que se corrige en el paquete, sin decisión.
- **D-S4:** una sola materialización del modelo completo por lectura (en desarrollo de la cuarta tanda; su texto normativo es D-PBT4-21).
- **D-S5:** las cuatro `Theory` que mezclan resultados en el RED se parten por filas.
- **D-S6:** las copias van en el espacio de nombres raíz de su ensamblado, con el prefijo `FullPanelCopy`.
- **La lectura de la sesión de las dos pruebas que marca el paseo de guardas corregido y que no entran en la lista,** porque no fallan:
  - WCAG `TheCatalogBody…` (:468-474): un falso positivo del paseo;
  - WHHG `TheGuardOfTheLiteralsTrips…` (:501-526): pasa en el GREEN, pero en vacío (CA-T4b I-01). Por eso el equivalente de `NoneOfThe30…` trae su autoprueba.

**Fe de erratas** (CA-T4b, §21). Los archivos no se tocan, porque la v4.9 cita su sha256:
- **M-01:** el registro de la sesión (`8b18add6…`, :148 y :150) dice «14 veces» y «las 14 `NotEmpty`»; léase 13. Lo comprobaron la sesión y el revisor, en WHHG:501-526 de `78a7cef3`.
- **M-03:** `step7_inputs_v49.py` no compara las entradas de la copia `r4t`, la tercera corrida del coste. El revisor las comprobó, y son idénticas a las de `r4s` (`independent-review-v1.md`, §21, M-03).

**La comprobación acotada de PB-T4b** (CA-T4b), en `F4B-DRAFTS/pbt4/independent-review-v1.md`, con su encargo `independent-review-pbt4b-check-prompt.md`:
- **§19,** la recomprobación de la v4.5: LISTO PARA LA PRECONDICIÓN 6, 0/0/4;
- **§20,** CA-T4b sobre el contrato en seco y la v4.8: EMITIBLE TRAS CORRECCIONES, 0/1/8;
- **§21,** la recomprobación de la v4.9: EMITIBLE, 0/0/3.

El resultado que lleva el contrato es `EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 8; las importantes corregidas en el paquete v4.9 (I-01, la autoprueba de la guarda de los textos del anfitrion pasa en vacio en el GREEN: el equivalente de NoneOfThe30 trae su autoprueba sobre la tabla nueva, como G-4); sin decisiones nuevas del Owner salvo el coste de la lectura (pb-20-ab x4, origen-es-Primary, primera corrida); precondicion 6 sobre 78a7cef3: pasos 1 a 6 cumplen; recomprobacion v4.9: EMITIBLE, B 0 / I 0 / M 3`.

**Antes de emitir, la precondición 6** de la v4.9, sobre el cierre `78a7cef3`: pasos 1 a 6, otra vez, en dos copias exportadas nuevas (`r4u` y `r4v`): la compilacion de Core-tests, UI-tests y el Plugin en Release sin errores ni avisos CS; la particion, la misma de la v4.8 byte a byte (e87e024c); el RED y el GREEN exactos, con la suite completa de Core y los 15 filtros de UI de la subtarea (sin las CT: Core 19 587 = 18 838 + 291 Failed + 458 omitidas en el RED, y 19 129 + 0 + 458 en el GREEN; R_C = 291 y R_UI = 0), sin ninguna prueba que falle fuera de las 478; el equivalente nuevo de WHHG cae en el RED por su Assert.Empty y pasa en el GREEN con sus 13 Assert.NotEmpty; el paso 7 es el de la v4.8 (CT-06 y el coste de la lectura en tres corridas), con sus entradas sin cambio por sha256 (step7_inputs_v49.py) y el coste dado por cumplido por el Owner (la septima tanda). Su custodia está en `F4B-DRAFTS/pbt4/cc4b/`, redactada y con su manifiesto, y su resumen en `F4B-DRAFTS/pbt4/pbt4b-precondition6.json`.

**Los controles** (PBT4B-C11): nc1 con la firma PD-F4B-01 y su sustituto (F4B-E40), y nc2 con el sustituto v2 (F4B-E43), los dos con `--task PBT4`. La primera entrada de la `ExpectedEvidence` es, byte a byte, la de `F4B_EXACT_EVIDENCES` del sustituto v2.

**La reparación del RED** (F4B-E35): la regla RR, con la lectura de PB-T4b. Una reparación no toca `src/` (el esqueleto), ni el kit, ni las 21 copias, ni los 21 archivos de los `Skip`.

**El contrato:**
- **P-01:** la Snapshot K (F4B-E37).
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.

## F4B-E53. Cierre de F4B-PBT4-FULL-DRAFT-PANEL, con una reparación del RED, cinco decisiones del Owner (tres sobre el código y dos sobre los controles) y la verificación tras un TIMEOUT

**Paquete de cierre:** `F4B-PBT4-FULL-DRAFT-PANEL/task-close-package.md`.

**La cadena:**
- **La emisión** (F4B-E52): X `f5ff00ee` y BASE `8f783df5`, con el paquete de PB-T4 v4.9 y las tandas del Owner sobre PB-T4 (F4B-E49 y siguientes).
  - El esqueleto del RED no es un parche: lo da el generador custodiado `F4B-DRAFTS/pbt4/green_pbt4b_v45.py`, y la precondición 6 registró sus seis archivos en `F4B-DRAFTS/pbt4/cc4b/out-redgreen-v49-r4v/apply-red.txt`.
  - La precondición 6 fija la partición `e87e024c`, con R_C = 291 y R_UI = 0, y el `CopySha` de las 21 copias.
- **La planificación** `R20261121T084940Z-a2b4` se aceptó al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 15 criterios. nc4 es `R20261121T085348Z-e944`: solo A3 en fail, y el oráculo relativo se cumple.
- **El Worker** `R20261121T085458Z-5ef5` (claude-sonnet-5-5/high) dio IMPLEMENTATION_COMPLETE, con una reparación del RED y una desviación declarada.
  - **El RED `c1e4f458`** (55 rutas) lleva:
    - el esqueleto, seis archivos de `src/` iguales byte a byte al registro de la precondición 6;
    - los 478 `Skip`, en 21 archivos;
    - las 21 copias y el kit, iguales a los de la precondición 6;
    - las seis clases de las CT.
  - **La reparación `9b2d1893`,** hijo directo del RED, solo toca `tests/RackCad.Tests/PushBackFullPanelTests.cs`: CT-PBT4B-08 deja de comparar `Name` en «el resto del sobre del rack actual». `Name` es un campo editable, y la fila 08 del §5.2 habla de «los campos no editables, la identidad y la versión».
  - **El GREEN `c065080d`** toca nueve archivos de código, iguales byte a byte al GREEN literal del generador, y ninguno de `tests/` ni de `Pilot/`.

**CI** (los totales exactos de cada rama, F4B-E46):
- **RED `44893322578`:** tests y ui-tests en failure; build-ui en success; build-plugin omitido. Core 19 605 = 18 841 + 306 Failed + 458 Skipped; UI 2 481 = 2 430 + 3 + 48.
- **La reparación `44899232904`:** los mismos 306 + 3 fallos, con la misma forma.
- **GREEN `44903818806`:** 4/4. Core 19 605 = 19 147 + 0 + 458; UI 2 481 = 2 433 + 0 + 48.

**La reparación del RED (RR, F4B-E35):**
- `rr_check` dio PASS en RR-1, RR-2 y RR-6, la n.º 10 y RR-3 (a). La clase reparada lleva 45 literales (42 líneas y 3 valores), todos en RedSha, y ninguno cambia su cuenta.
- **La lectura RR-3 estuvo diferida hasta la reparación:**
  - El generador del contrato emitido no trae `rr3_table_check`, y la lectura de PB-T4a no lee las filas de PB-T4b.
  - La planificación no la exigió, y un `RedRepairSha` sin ella habría parado el relevo, la verificación y `rr_check`. Era una falla cerrada y una parada de herramienta.
  - **Al aparecer la reparación,** la cadena escribió su lectura de las 105 filas de PB-T4b de §6.0 (`pbt4b_rr3_reading.py`, con su autoprueba sobre las filas reales). Resuelve solo lo que el paquete fija.
  - **La clase reparada,** `PushBackFullPanelTests`, queda resuelta.
  - **Quedan pendientes tres clases,** porque sus filas remiten a literales que el paquete no enumera: `WorkspaceFullCopyGuardTests`, `PushBackFullPanelGuardTests` y `PushBackFullPanelFieldTests`. Si una reparación las tocara, la cadena pararía.

**Revisiones:**
- **La directa:** conforme, con una desviación que el Owner ratificó.
- **La independiente:** ACEPTABLE CON MENORES 0/1/8, condicionada a I-01, con cuatro hallazgos para el Owner:
  - OWNER-01: el nombre tras «Conservar» y «Actualizar»;
  - OWNER-02: la disposición de I-01;
  - OWNER-03: los vacíos de M-04;
  - OWNER-04: el segundo «Sí» sin cambios en el dibujo real.

**Las tres decisiones del Owner sobre el código** (`F4B-DRAFTS/coordinator/owner-pbt4b-code-decisions.md`, literales):
1. **CT-20 (5), la desviación I-01: «Ratificar como errata (Recomendado)».**
   - La comprobación (5) vale sobre las nueve clases de C-0 (`WorkspaceFullCopyGuardTests.cs:630-632`). Tal cual daría `False` por cinco apariciones en literales de cadena de dos copias de guarda custodiadas.
   - Solo para este caso y sin cambio de código.
2. **Los vacíos de M-04: «Llevarlas a PB-T4d (Recomendado)».** Dos guardas de PEG, y lo que hereda su copia, pasan en vacío en el GREEN. Entran en la lista de PB-T4d, que vuelve al Owner antes de emitirse.
3. **El nombre tras «Conservar» y «Actualizar»: «Es lo esperado (Recomendado)»** (D-PB-05, paso 4). El Smoke-PB lo comprueba, y la mejora del texto de la presentación queda en `docs/ideas-futuras.md`.

**La verificación**, en dos corridas. La primera se custodia tal cual, como unidad propia:
1. **`R20261121T132429Z-db16`, lanzamiento no productivo por tiempo (TIMEOUT).**
   - El Controller trabajó 600 s, completó 50 órdenes y el lanzador lo cortó con exit 124, sin `turn.completed` ni `output.json`.
   - La sesión la dio por no productiva, como la `611f` de PB-T4a (`F4B-DRAFTS/pbt4/session-verify-timeout-decision.md`): no es una verificación ni gasta intento.
   - El tope de los lanzadores de la verificación y de los controles pasó de 600 a 1800 s, sin otro cambio.
   - El relevo la registra como `TIMEOUT`.
2. **`R20261121T133638Z-1b82`, EXECUTION_VERIFIED / NONE, 14/14,** con `inputs/coordinator-disposition.md` (`F4B-DRAFTS/pbt4/coordinator-disposition-ct20-pbt4b.md`). Tiene un solo hallazgo: la disposición solo ratifica la errata de CT-20 (5).

**Controles:**
- **nc1** `R20261121T134602Z-5688`: CONTROL NOT PASSED, con la cascada del SHA falso.
  - Difieren de la verificación real Handoff, Identity, Remote, Scope, Ci, Tests y Trailer, con S-04 y PBT4B-C18.
  - Su sustituto de PD-F4B-01 (`R20261121T140108Z-d3ca`, ScriptSha256 `41440edc…`, `--task PBT4`) dio FAIL en `d1_FailSetWithinAllowed` y `e2_OtherFailCitesFakeSha`. Trailer está entre los fallos, y su evidencia da la misma causa con otras palabras («no existe el objeto CurrentSha»), sin citar el SHA falso.
  - Es la **sexta variación de nc1 en F4B**, tras PB-TC, PB-TC2, PB-T2a, PB-T2b y PB-T4a.
  - El Owner eligió «Aceptarlo solo esta vez (Recomendado)», así que **nc1 = SATISFIED_BY_OWNER_DISPOSITION**. El sustituto no cambia y no se amplía ninguna firma.
- **nc2** `R20261121T135034Z-464a`: **CONTROL PASS,** ordinario: Scope, con PBT4B-C01.
- **nc3** `R20261121T135520Z-2bac`: CONTROL NOT PASSED.
  - FreeText detecta la frase inyectada, pero antes difiere Tests, con PBT4B-C04: el control lee 171 de los 306 fallos del RED (`NullReferenceException` en código de prueba) como no producidos por aserción.
  - Con la regla de INV-PBT4B-02 (la primera línea del StackTrace bajo `tests/`), los 306 son por aserción, como dieron la verificación real y la revisión independiente (M-07). No hay sustituto de nc3.
  - El Owner eligió «Aceptarlo solo esta vez (Recomendado)», así que **nc3 = SATISFIED_BY_OWNER_DISPOSITION**.
  - La etiqueta «PorAsercion» de las Notes del relevo del trabajo se corrige en la cadena de PB-T4c.

**Las dos decisiones del Owner sobre los controles** (`F4B-DRAFTS/coordinator/owner-pbt4b-controls-decisions.md`, literales): nc1, «Aceptarlo solo esta vez (Recomendado)», y nc3, «Aceptarlo solo esta vez (Recomendado)». Con las tres del código, son las cinco decisiones del Owner de esta cadena.

**El registro declarado de las 478** (`F4B-DRAFTS/superseding-characterization.md`, «## PB-T4b»): este cierre completa su tabla de ejecución y la pasa a EJECUTADA. Cambian cinco líneas, con el texto y la fuente en `F4B-DRAFTS/pbt4/superseding-pbt4b-close-fill.md`.

**Resultado:** **F4B-PBT4-FULL-DRAFT-PANEL = CLOSED / PASS WITH LIMITATIONS** sobre `c065080d`, con attempts 1 de 3.
- **El borrador del panel del Workspace es el modelo completo de PB-T3,** sobre la lectura completa de PB-T4a, sin traducir estados.
- **La regla de la escala,** única y en Application (`NumericTextRule`): `NumericFieldValidation` delega en ella sin cambiar su comportamiento, y `PushBackAnnotationScaleRule` la usa.
- **«Actualizar»** va por `PrepareFull` y `ExecuteFull`.
- **«Conservar mi borrador»** va en dos «Sí», con la presentación «Por grupos» y su testigo.
- **Cada lectura materializa el modelo completo una sola vez.**
- **Las 478 pruebas de la lista** quedan saltadas con su motivo literal, cada una sustituida por su copia o su equivalente.
- **El código del piloto queda sin uso;** lo retira PB-T4d.

**Las limitaciones:**
- el nombre tras «Conservar» y «Actualizar» (OWNER-01): es lo esperado, y lo comprueba el Smoke-PB;
- el segundo «Sí» sin cambios en el dibujo real (OWNER-04): lo comprueba el Smoke-PB;
- el coste real de la lectura, sin AutoCAD;
- los vacíos de M-04, que van a la lista de PB-T4d;
- RR-3, con tres clases pendientes;
- los menores M-01 a M-08, en `docs/ideas-futuras.md`;
- nc1 y nc3, satisfechos por disposición del Owner. La sexta variación de nc1 hace del sustituto un candidato a revisar, sin cambio ahora;
- sin AutoCAD, OB-03.2, «Actualizar», «Conservar» y el borrado de vistas reales quedan para el Smoke-PB.

**Siguiente:** el paquete de PB-T4c, con su emisión condicional (F4B-E49, pregunta 4):
- la comprobación acotada en 0/0;
- la remedición sobre este cierre, sin cambio material;
- la lista que vio el Owner.

PB-T4d vuelve al Owner con su lista antes de emitirse.

## F4B-E54. Emisión de F4B-PBT4-EDIT-ALL-WINDOW (PB-T4c), con el paquete de PB-T4 v5.1

**Fecha:** 2026-11-21.

**Autoridad de emisión:** la decisión del Owner F4B-E49, pregunta 4: «Sí, con condiciones (Recomendado)». Cubre PB-T4a, PB-T4b y PB-T4c, en ese orden y cada una tras el cierre de la anterior. PB-T4c es la última de las tres.
- **Las condiciones que exige, cumplidas:**
  - la comprobación acotada de la subtarea, CA-T4c, da 0 bloqueantes y 0 importantes sin resolver: su importante, I-01, está corregida en la v5.1, y su recomprobación da EMITIBLE, 0/0/1;
  - la remedición sobre el cierre anterior, el de PB-T4b, no cambia nada material: la precondición 6 corre sobre copias exportadas de `6adff90d`;
  - la lista emitida es la que vio el Owner: las 4 de `pbt4c-substitution-list-4.tsv` (sha256 `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de`), las 2 de la pregunta 4 más la de la octava tanda y la de la novena.
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista. Cada prueba que cayó fuera de la lista volvió al Owner antes de emitir (la octava y la novena tandas). No queda ninguna.

**El cierre de PB-T4b** (F4B-E53, `6adff90d`): PB-T4b CLOSED / PASS WITH LIMITATIONS sobre `c065080d`, en el intento 1 de 3, con una reparación del RED. CI del GREEN `44903818806`, 4/4: Core 19 605 = 19 147 + 0 + 458, y UI 2 481 = 2 433 + 0 + 48.

**Qué es PB-T4c:** «Editar todo» en el panel del Workspace de Push Back, sobre el borrador completo de PB-T4b (D-PBT4-12 a D-PBT4-17).
- **Un botón y su gesto,** que confirma el texto pendiente antes de abrir (R-PB-10).
- **`TryEditAll`,** en el puerto, en el anfitrión y en sus dos reenvíos: empieza el borrador si hace falta, mantiene la marca de lectura mientras la ventana está abierta (K = 1) y abre la ventana de Push Back en modo WORKSPACE, modal, sobre el borrador completo, sin traducir estados.
- **«Cerrar» en WORKSPACE** hace una sola pregunta si hay a la vez un módulo escenificado y cambios sin aplicar: es la decisión del Owner A-T2b-OWNER-01 (F4B-E38).
- **Cero escrituras en el DWG** desde la ventana. Ni el camino clásico, ni RACKEDITAR, ni la ventana en CLASSIC cambian.
- **`UNIT_CHANGE`:** las 9 existentes de `src/` del GREEN (F4B-E25).

**Las 4 sustituciones, con registro** («Saltar y duplicar», la pregunta 3 de la primera tanda): cada una con un `Skip` de motivo literal y su equivalente en una clase nueva, que genera el guion desde el original, sin tocar la prueba original.
- **En Core, 3 (C-5):** PEG:155-163, CT-PBT1B-15 y CT-PBT4B-20, esta de la octava tanda;
- **En UI, 1 (C-3):** CT-PBT2B-06, de la novena tanda. Su equivalente cae en el RED, por aserción, y pasa en el GREEN, así que su filtro va con `ExpectRed`.

La entrada «## PB-T4c» de `F4B-DRAFTS/superseding-characterization.md` va en esta emisión, con los motivos literales y el `EquivalentSha` y el cambio declarado de cada equivalente. Su tabla de ejecución se completa al cerrar.

**El total exacto de cada rama** (F4B-E46):
- **GREEN:** Core 19 619 = 19 158 + 0 + 461; UI 2 486 = 2 437 + 0 + 49.
- **RED:** Core 19 619 = 19 149 + 9 + 461; UI 2 486 = 2 432 + 5 + 49, con las 4 CT de UI y el equivalente de CT-PBT2B-06 entre los `Failed`.

**Las tandas del Owner sobre PB-T4c** (§8.5 de la v5.1), nuevas en esta emisión y literales en `F4B-DRAFTS/coordinator/`. Las siete anteriores las declararon F4B-E49, F4B-E51 y F4B-E52.

8. `owner-pbt4c-list-3-decision.md`: «Añadirla a la lista (Recomendado)». La lista pasa de 2 a 3, con CT-PBT4B-20.
9. `owner-pbt4c-list-4-decision.md`: «Añadirla a la lista (Recomendado)». La lista pasa de 3 a 4, con CT-PBT2B-06.

**Las decisiones de diseño de la sesión,** que no son del Owner (`F4B-DRAFTS/pbt4/session-pbt4c-design-decisions.md`, sha256 `d1912d978f03cab40fd88b93b9574170bee93841e43fdaca29b1e1512a4c3e83`). Ninguna cambia la lista ni el comportamiento visible que decidió el Owner:
- **D-S7:** CT-PBT4C-09 escenifica el módulo con `HeaderConfiguratorDialog`, que no construye el configurador real (C05).
- **D-S8:** cada clase nueva de PB-T4c tiene su propio filtro, por su nombre exacto. Ningún filtro nuevo selecciona una clase con el otro `ExpectRed`, y ningún filtro heredado selecciona una clase nueva.
- **D-S9:** el host del editor completo se llama `IWorkspaceFullEditHost`, con `SetFullEditHost` y `_fullEditHost`, para que `WorkspaceModelBoundaryTests.ThereIsNoPerTabStateOrPerTabSubscriptions` siga en verde sin tocarla. Se midió en la copia `r5e`.
- **D-S10:** CT-PBT4C-09 cubre lo que se puede alcanzar. En WORKSPACE, escenificar un módulo deja siempre cambios sin aplicar, y la pregunta del módulo sola es la de CLASSIC.

**La vuelta parada de la precondición 6** (historia; no vale para la emisión), en las copias `r5a` a `r5e`, con las piezas de antes de D-S9 y las listas de 2 y de 3:
- **`r5a`:** solo los dos `Skip` de la lista de 2, sin GREEN. Cae CT-PBT4B-20, y de ahí la octava tanda. Su registro va en `F4B-DRAFTS/pbt4/v50/out-probe-skips-r5a/`.
- **`r5b`:** la compilación del RED y del GREEN con `IWorkspaceFullEditorHost`, sin errores ni avisos CS.
- **`r5c`:** la precondición 6 con la lista de 3. Cae `WorkspaceModelBoundaryTests.ThereIsNoPerTabStateOrPerTabSubscriptions` en las dos ramas, por «Editor», y CT-PBT2B-06 en el GREEN: de ahí D-S9 y la novena tanda.
- **`r5d`:** la sonda de CT-03, 04, 05 y 12 con las piezas de la primera vuelta.
- **`r5e`:** la medida de D-S9, con los tres nombres cambiados: Core 19 608 = 19 147 + 0 + 461 en las dos ramas. Sus salidas de texto van en `F4B-DRAFTS/pbt4/cc4c/r5e/`.

Los registros de `r5b` a `r5d` van en `cc4c/`, con las corridas que valen.

**La autoridad n.º 37:** en el contrato de PB-T4b, «este contrato» apuntaba al contrato de PB-T4a (`F4B-PBT4-FULL-READ-PATH/coordinator/gate-contract.json`). En el de PB-T4c es su propio contrato, `F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json` (§6.3.2 de la v5.1), que escribe la BASE. El contrato anterior, el de PB-T4b, es la n.º 29.

**La comprobación acotada de PB-T4c** (CA-T4c), en `F4B-DRAFTS/pbt4/independent-review-v1.md`, con su encargo `independent-review-pbt4c-check-prompt.md`:
- **§22,** CA-T4c sobre el contrato en seco y la v5.0: EMITIBLE TRAS CORRECCIONES, 0/1/3. Su importante, I-01: los dos archivos nuevos de `Draft/` empezaban por `#nullable enable`, y la pureza de CT-PBT4C-15 lo prohíbe;
- **§23,** la recomprobación de la v5.1: EMITIBLE, 0/0/1.

El resultado que lleva el contrato es `EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 3; las importantes corregidas en el paquete v5.1 (I-01, los dos archivos nuevos de Draft/ empiezan por #nullable enable y la pureza de CT-PBT4C-15 lo prohibe: sin la directiva y sin el ? de ProjectOf); sin decisiones nuevas del Owner; precondicion 6 sobre 6adff90d: pasos 2 a 7 cumplen, sin las CT; recomprobacion v5.1: EMITIBLE, B 0 / I 0 / M 1`.

**Fe de erratas** (CA-T4c, §23 M-01). El paquete no se toca, porque el contrato cita su sha256:
- **En la tabla de §5.3 de la v5.1** (:1628 y :1631), las líneas `conflict` y `error` de CT-03 citan `DraftSession.cs:331-332`, el `CanEdit` del borrador. Lo que decide en `EditAllOfPilot` es `view.CanEdit`, en `WorkspaceHostCore.cs:1805-1806` de `6adff90d`: registrado y Clean, Dirty o Conflict.
- El resultado es el mismo: Conflict abre y Error se rechaza. Valen las dos citas.

**El paso a la v5.1:** corrige CA-T4c I-01 en el esqueleto y en el GREEN. Los dos archivos nuevos de `Draft/` van sin `#nullable enable`, y `ProjectOf` sin el `?` de `RackProject?`, que sin la directiva daría CS8632. Corrige también los menores M-01 a M-03 de la §22. No cambia la lista, los totales ni los filtros, y no necesita al Owner. La v5.0 pasa a la historia.
- **La lección de I-01:** la precondición 6 no corría las CT. La v5.1 le añade el paso 8: el ensayo de las CT de texto sobre `src/` (CT-01, 07, 08, 13 y 15), en las dos ramas, con su control sobre las piezas de la v5.0, que ve I-01, y el censo de C13 y de los nombres.
- **`compile-check.md`** da el resultado de cada clase que corre, también el de las que la custodia solo lleva por sha256.

**Antes de emitir, la precondición 6** de la v5.1, sobre el cierre `6adff90d`: pasos 1 a 9, otra vez, en copias exportadas nuevas de 6adff90d (`r5j` para los pasos 2 a 6, `r5i` para las sondas del paso 7 y el ensayo del paso 8, `r5k` para el coste de IsDirty y `r5l` para el control del ensayo sobre las piezas de la v5.0): la compilacion de Core-tests, UI-tests y el Plugin en Release sin errores ni avisos CS; el RED exacto y el GREEN literal, sin las CT, con la suite completa de Core (19 608 = 19 147 + 0 + 461 en las dos ramas, con las 3 de Core de la lista omitidas con su motivo y sus 3 equivalentes en verde) y los 18 filtros de UI (en el RED solo cae el equivalente de CT-PBT2B-06, por asercion; en el GREEN, 0 Failed), sin ninguna prueba que falle fuera de las 4 (D = 0, K = 4, E = 4); los Skip, comprobados (4 archivos, 4 Skip y nada mas), y los filtros, cada uno solo con su clase (D-S8); las sondas de las lineas de CT-03, 04, 05 y 12 y de CT-09 (D-S10), las mismas de la v5.0 por sha256; el ensayo de las CT de texto (CT-01, 07, 08, 13 y 15) pasa en las dos ramas, con CT-01, 08 y 13 en Failed por asercion en el RED, y su control sobre las piezas de la v5.0 ve I-01; el censo de C13 y de los nombres pasa; el coste de IsDirty, el de PB-T4b, dentro del ruido; y compile-check.md da el resultado de cada clase que corre. Las corridas de la v5.0 (`r5f` a `r5h`) y la vuelta parada (`r5a` a `r5e`), como historia. Su custodia está en `F4B-DRAFTS/pbt4/cc4c/`, redactada y con su manifiesto, y su resumen en `F4B-DRAFTS/pbt4/pbt4c-precondition6.json`.

**Los controles** (PBT4C-C11): nc1 con la firma PD-F4B-01 y su sustituto (F4B-E40), y nc2 con el sustituto v2 (F4B-E43), los dos con `--task PBT4`. La primera entrada de la `ExpectedEvidence` es, byte a byte, la de `F4B_EXACT_EVIDENCES` del sustituto v2.

**La reparación del RED** (F4B-E35): la regla RR, solo en las cinco clases de las CT. Una reparación no toca `src/` (el esqueleto), ni los cuatro equivalentes, ni los cuatro archivos de los `Skip`.

**El contrato:**
- **P-01:** la Snapshot K (F4B-E37).
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.
- **La etiqueta «PorAsercion»** de las Notes del relevo del trabajo cuenta por la regla del contrato o se renombra en esta cadena (F4B-E53, la decisión sobre nc3).
## F4B-E55. Cierre de F4B-PBT4-EDIT-ALL-WINDOW, sin reparación del RED, con dos decisiones del Owner sobre el código

**Paquete de cierre:** `F4B-PBT4-EDIT-ALL-WINDOW/task-close-package.md`.

**La cadena:**
- **La emisión** (F4B-E54): X `a8e9cbe6` y BASE `d5026014`, con el paquete de PB-T4 v5.1 y las tandas del Owner sobre PB-T4 (F4B-E49, con la octava y la novena sobre la lista de PB-T4c).
  - **La lista exacta de 4** (`F4B-DRAFTS/pbt4/pbt4c-substitution-list-4.tsv`, `9bd4c17a…`), la que vio el Owner: PEG:155-163, CT-PBT1B-15 y CT-PBT4B-20 (C-5, Core) y CT-PBT2B-06 (C-3, UI). Cada una con su `Skip` de motivo literal y su equivalente.
  - **El esqueleto del RED no es un parche:** lo da el generador custodiado `F4B-DRAFTS/pbt4/green_pbt4c_v51.py`, con sus dos piezas, y la precondición 6 registró sus ocho archivos en `F4B-DRAFTS/pbt4/cc4c/r5j-rg/apply-red.txt`.
  - **La precondición 6** fija el `EquivalentSha` de los 4 equivalentes y el `GreenFiles` del GREEN literal (`pbt4c-green-v51.txt`).
  - **Sin copias:** la cadena de primera delegación lee esta forma del alcance (los equivalentes, el comprobador de los `Skip` de la ExpectedEvidence y el GREEN literal). La de PB-T4b, con copias, sigue intacta en su rama.
- **La planificación** `R20261121T205457Z-af49` se aceptó al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 14 criterios. nc4 es `R20261121T205711Z-fbe4`: solo A3 en fail, y el oráculo relativo se cumple.
- **El Worker** `R20261121T205731Z-41db` (claude-sonnet-5-5/high) dio IMPLEMENTATION_COMPLETE, sin reparación del RED.
  - **El RED `280c79be`** (21 rutas) lleva:
    - el esqueleto, ocho archivos de `src/` iguales byte a byte al registro de la precondición 6, que escribe la herramienta del paquete `apply_red_pbt4c_v51.py`;
    - los 4 `Skip`, en los 4 archivos de la lista, solo en la línea del atributo;
    - los 4 equivalentes, byte a byte su `EquivalentSha`;
    - las cinco clases de las CT, CT-PBT4C-01 a 15.
  - **El GREEN `7cc1076e`** toca 10 archivos de `src/`. Los 13 del GREEN literal son byte a byte su texto, y no toca ninguno de `tests/` ni de `Pilot/`.
  - **Dos desviaciones de procedimiento local, declaradas:**
    - la suite local de Core en tres particiones disjuntas, con suma exacta;
    - más tiempo del detector de cuelgue local para una clase lenta, tras dos cortes.
    La revisión independiente las da por procedimiento local, que el contrato no regula. La prueba lenta es la de PB-T4b, `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuild…`: 276 s en la CI de PB-T4b, 321 s y 188 s en las de PB-T4c.

**CI** (los totales exactos de cada rama, F4B-E46):
- **RED `44956801926`:** tests y ui-tests en failure; build-ui en success; build-plugin omitido. Core 19 619 = 19 149 + 9 Failed + 461 Skipped; UI 2 486 = 2 432 + 5 + 49. Los 5 de UI son las 4 CT de UI y el equivalente de CT-PBT2B-06.
- **GREEN `44959653142`:** 4/4. Core 19 619 = 19 158 + 0 + 461; UI 2 486 = 2 437 + 0 + 49.

**El relevo del trabajo:** válido. Comprueba la forma del RED, el esqueleto, los `Skip`, los `EquivalentSha`, el GREEN literal y cero `warning CS`.

**Revisiones:**
- **La directa:** conforme, con una desviación que el Owner ratificó.
- **La independiente:** ACEPTABLE CON MENORES 0/1/7, condicionada a I-01, con cinco hallazgos para el Owner:
  - OWNER-01: la disposición de I-01;
  - OWNER-02: «Editar todo» sobre un borrador Clean marcado como posiblemente obsoleto;
  - OWNER-03: el BOM y «Guardar en biblioteca» desde la ventana de «Editar todo»;
  - OWNER-04: lo que §5.5 no lista (la pregunta combinada de «Cerrar», «Modificado» tras cerrar y el aviso de M-04);
  - OWNER-05: la prueba lenta, frente al detector de cuelgue de la CI.

**Las dos decisiones del Owner sobre el código** (`F4B-DRAFTS/coordinator/owner-pbt4c-code-decisions.md`, literales):
1. **CT-PBT4C-11, la desviación I-01: «Ratificar como errata (Recomendado)».**
   - La segunda receta de la prueba (`tests/RackCad.Tests/PushBackEditAllTests.cs:589-600`) sustituye `Editor.SideA.LastModuleReconciliation` por un objeto vacío, y no la línea base resuelta por la de otro fixture (§5.3, fila 11). Es la misma forma que CT-PBT4B-10.
   - La línea esperada `sustituir-referencia estado=Applied isdirty=False` se cumple.
   - Solo para este caso y sin cambio de código.
2. **«Editar todo» sobre un borrador Clean posiblemente obsoleto (OWNER-02): «Aceptarlo y corregir luego (Recomendado)».**
   - Se queda así en PB-T4c. No se escribe nada sin avisar: al aplicar sale el camino de «Conservar mi borrador».
   - El Smoke-PB lo comprueba con un cambio externo del dibujo y un borrador Clean.
   - La corrección, que «Editar todo» recargue antes, como «Editar», queda para PB-T4d o una tarea posterior, que vuelve al Owner con su lista.

**La verificación** `R20261121T234430Z-dd13`: **EXECUTION_VERIFIED / NONE, 14/14,** con `inputs/coordinator-disposition.md` (`F4B-DRAFTS/pbt4/coordinator-disposition-ct11-pbt4c.md`).

**Controles:**
- **nc1:** `R20261121T235038Z-94db`, **CONTROL PASS** ordinario. Para en Identity con S-04, con la cascada del SHA falso en Remote, Scope, Ci y Tests y el oráculo relativo cumplido.
- **nc2:** `R20261121T235550Z-2fc5`, CONTROL NOT PASSED. Para en Contract y Scope con S-04 y PBT4C-C01: es la firma de PD-F4-01 en el modo F4B-E21. El sustituto v2 revisado (`apply_nc2_f4b_pbt4c.sh`, `--task PBT4`, F4B-E43), en la corrida `R20261122T000958Z-4719` de `F4B-PBT4-EDIT-ALL-WINDOW-nc2-substitute/1/`, da **PASS** sin condiciones fallidas: nc2 queda SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B.
- **nc3:** `R20261122T000101Z-3ec1`, **CONTROL PASS** ordinario: FreeText con EXECUTION_REWORK_REQUIRED / REWORK. Con el criterio de «por aserción» corregido en la cadena (M-07 de PB-T4b), Tests ya no difiere.

Los tres controles quedan satisfechos sin ninguna decisión del Owner.

**El registro declarado de las 4** (`F4B-DRAFTS/superseding-characterization.md`, «## PB-T4c»): este cierre completa su tabla de ejecución y la pasa a EJECUTADA. Cambian cinco líneas, con el texto y la fuente en `F4B-DRAFTS/pbt4/superseding-pbt4c-close-fill.md`.

**Resultado:** **F4B-PBT4-EDIT-ALL-WINDOW = CLOSED / PASS WITH LIMITATIONS** sobre `7cc1076e`, con attempts 1 de 3.
- **«Editar todo» en el panel del Workspace de Push Back,** sobre el borrador completo de PB-T4b: el botón y su gesto, que confirma el texto pendiente con `CommitField` antes de abrir.
- **`TryEditAll`,** en el puerto con su cuerpo por defecto, en el anfitrión y en sus dos reenvíos. Mantiene la marca de lectura mientras la ventana está abierta (K = 1).
- **La ventana de Push Back en WORKSPACE, modal,** en dos piezas:
  - el modal del panel, en una clase anidada del controlador del Plugin;
  - la ventana sin dueño, en `Plugin/Views`.
- **«Cerrar» en WORKSPACE** con una sola pregunta si hay a la vez un módulo escenificado y cambios sin aplicar (A-T2b-OWNER-01). El resumen y «Modificado» se muestran al cerrar.
- **Las 4 pruebas de la lista** quedan saltadas con su motivo literal, cada una sustituida por su equivalente.
- **El código del piloto queda sin uso;** lo retira PB-T4d.

**Las limitaciones:**
- «Editar todo» sobre un borrador Clean posiblemente obsoleto no recarga (OWNER-02). Lo acepta el Owner; el Smoke-PB lo comprueba, y la corrección va a PB-T4d o después.
- El BOM y «Guardar en biblioteca» desde la ventana de «Editar todo» escriben en disco (OWNER-03). Lo comprueba el Smoke-PB.
- La pregunta combinada de «Cerrar», «Modificado» tras cerrar y el aviso «No se abrió el editor» cuando sí hubo cambios (OWNER-04; M-04) los comprueba el Smoke-PB.
- La prueba lenta, frente al detector de cuelgue de la CI (OWNER-05), va a PB-T4d.
- Los menores M-01 a M-04 van a `docs/ideas-futuras.md`. M-05 a M-07 quedan de constancia.
- Sin AutoCAD, el Smoke-PB comprueba en el dibujo real:
  - el modal con la paleta deshabilitada;
  - el foco y el teclado;
  - «Actualizar» tras editar;
  - los eventos del puente durante el modal.

**Siguiente:** PB-T4d, la retirada del código del piloto. Vuelve al Owner con su lista antes de emitirse. La lista lleva:
- los vacíos de M-04 de PB-T4b (F4B-E53);
- la recarga de «Editar todo» (OWNER-02);
- la prueba lenta (OWNER-05).

La emisión condicional de F4B-E49 (pregunta 4) termina con PB-T4c.

## F4B-E56. Emisión de F4B-PBT4-RETIRE-PILOT (PB-T4d), con el paquete de PB-T4 v5.4

**Fecha:** 2026-11-22.

**Autoridad de emisión:** la decisión del Owner sobre PB-T4d, `owner-pbt4d-list-decisions.md`, pregunta 4: «Sí, con condiciones (Recomendado)».
- **Las condiciones que exige, cumplidas:**
  - la comprobación acotada de la subtarea, CA-T4d, da 0 bloqueantes y 0 importantes sin resolver: sus dos importantes, I-01 e I-02, las resolvió el Owner (la quinta tanda de PB-T4d) y las corrige la v5.3; sus recomprobaciones dan la de la v5.3, EMITIBLE, 0/0/6; la de la v5.4, EMITIBLE, 0/0/3, y la última, sin bloqueantes ni importantes;
  - la remedición sobre el cierre anterior, el de PB-T4c, no cambia nada material: la precondición 6 corre sobre copias exportadas de `6c2820ba`;
  - la lista emitida es la que eligió el Owner: el alcance B, con las sustitutas, las 29 de WPUR y la rama muerta de `BuildCore`: las 271 filas con acción de `pbt4d-retire-list-271.tsv` (sha256 `05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac`; 284 filas, 13 que se quedan sin tocar).
- **Cuándo se vuelve al Owner:** por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista (PBT4D-C16).

**El cierre de PB-T4c** (F4B-E55, `6c2820ba`): PB-T4c CLOSED / PASS WITH LIMITATIONS sobre `7cc1076e`, en el intento 1 de 3, sin reparación del RED. CI del GREEN `44959653142`, 4/4: Core 19 619 = 19 158 + 0 + 461, y UI 2 486 = 2 437 + 0 + 49.

**Qué es PB-T4d:** la retirada del código del piloto que el panel ya no usa desde PB-T4b (D-PBT4-10), con el alcance B.
- **El GREEN** solo toca `src/`, con el texto literal del paquete: 4 archivos de `Pilot/` cambian y 2 se borran (`PushBackPilotStateComparer.cs` y `PushBackPilotFieldAdapter.cs`). Son los 6 `UNIT_CHANGE` (F4B-E25).
- **El RED** lleva todo lo de `tests/` (D-S11), sin esqueleto: las bajas con registro, las ediciones con registro, los 3 `Skip` con su equivalente, la prueba lenta acotada, los vacíos de M-04, las 29 sustitutas de WPUR, los 3 archivos que se quedan sin pruebas (bloques DELETE) y los 5 archivos nuevos, con las dos CT.
- **Ningún cambio de comportamiento:** ni el panel, ni la ventana, ni RACKEDITAR, ni el Plugin; cero escrituras en el DWG.

**La lista, por acción:** borrar con registro, 198, reescribir con registro, 39, sustituir con registro, 33 y acotar con registro, 1; se quedan sin tocar 13. La entrada «## PB-T4d» de `F4B-DRAFTS/superseding-characterization.md` va en esta emisión, por grupo y acción, con los motivos literales de los `Skip`, los archivos nuevos con su `NewFileSha` y las ediciones con registro. Su tabla de ejecución se completa al cerrar.

**El total exacto de cada rama** (F4B-E46):
- **GREEN:** Core 19 418 = 19 015 + 0 + 403; UI 2 464 = 2 416 + 0 + 48.
- **RED:** Core 19 418 = 19 014 + 1 + 403, con CT-PBT4D-01 como único `Failed`; UI 2 464 = 2 416 + 0 + 48.

**Las tandas del Owner sobre PB-T4d** (§8.5 de la v5.4), nuevas en esta emisión y literales en `F4B-DRAFTS/coordinator/`:

10. `owner-pbt4d-list-decisions.md`: «Alcance B», la recarga de «Editar todo» a una tarea propia después, la prueba lenta acotada en PB-T4d y la emisión condicional.
11. `owner-pbt4d-substitutes-decision.md`: «Sustitutas donde hace falta».
12. `owner-pbt4d-run-coverage-decision.md`: «Sustituir las 21», con la cobertura medida otra vez.
13. `owner-pbt4d-deadcode-27-decisions.md`: «Quitarla en PB-T4d» (la rama muerta de `BuildCore`) y «Sí, 27» (las sustitutas de la corrida).
14. `owner-pbt4d-cat4d-decisions.md`, sobre CA-T4d: «Sustituirlas» (la n.º 48 y la n.º 49 de WPUR pasan a sustitutas: 29) y «Sustituirlos siempre» (la cobertura de la precondición 6 mira todo `src/` que se queda).

**Las decisiones de diseño de la sesión,** que no son del Owner (`F4B-DRAFTS/pbt4/session-pbt4d-design-decisions.md`, sha256 `bd02d2103c7a671c51c1de8901bf76608e4944f98ad0fabcca1fdd253d3801c8`): D-S11 (el RED con todo lo de `tests/`, sin esqueleto, y dos CT), D-S12 (los tres `Skip` con su equivalente, el prefijo `PushBackRetirePilot` y los filtros por clase exacta), D-S13 (M-04 en su sitio), D-S14 (la prueba lenta, cada 97.º toque y el último), D-S15 (los cuatro `cref` y el comentario) y D-S16 (la cobertura de la UI como unión de corridas, y la lista de lo que baja). Ninguna cambia la lista ni el comportamiento visible que decidió el Owner.

**Las decisiones de la sesión sobre el contrato y su emisión** (2026-11-22; tampoco son del Owner):
- **Las autoridades:** de las 70 del contrato de PB-T4c siguen las que no son de su subtarea; la sección se pone al día en la n.º 0, 6, 28, 29, 36 y 37; la n.º 43 pasa, en su sitio, al informe de CA-T4d; la n.º 48 (`owner-pbt4b-code-decisions.md`, M-04) se queda con su sección nueva; salen la 44 a la 47 y la 49 a la 69; se añaden las de §6.4.2, con la quinta tanda detrás de las cuatro del Owner. Los guiones de §6.4.2 se comprueban por sha256, pero no son autoridad.
- **El `ForbiddenWriteScope`:** el literal de §6.4, con las cinco clases de las CT y los cuatro equivalentes de PB-T4c; el paquete manda.
- **La precondición 6:** `Package` y `Task` se leen; `RedDeleted` se lee (los bloques DELETE del RED y `Expected.red-deleted-3`); `Noise` es un dato informativo que no se lee.
- **La custodia de `cc4d/`:** la cobertura en XML y JSON, solo por sha256, como las TRX; la raíz de `pbt4dcc`, `_sim/` y `_tools/` quedan fuera, porque son herramientas de la cadena de la sesión.
- **`RedFiles` de la precondición 6 frente a la v5.4:** cada archivo, el del RED de la v5.4 aplicado en memoria, salvo los de `Reapply.TestsTextChanged` (hoy, `WorkspacePilotUpdateRunTests.cs`), que son los del RED de la v5.3, y el generador comprueba él mismo que los dos textos dan el mismo código sin comentarios ni líneas en blanco; además, `TestsCodeChanged` vacío y `SrcDifferences` 0. **La razón:** la precondición 6 corrió con el texto de la v5.3, y fijar los sha256 del RED de la v5.4 diría que se midió algo que no se midió.
- **La copia exportada:** la etiqueta de copia, antes `r4<letra>` a `r6<letra>`, pasa a `r4` a `r9` seguida de una letra o una cifra (entra `r70`, la de la reaplicación): una carpeta corta, sin nada personal, que se tapa con `<copia>`, también en los textos de fuera de `cc4d/`; lo que tras tapar la copia aún lleve una ruta o un nombre local va solo por sha256, declarado.
- **`_write_cat4d_prompt.py`,** reescrito por la sesión sin rutas ni nombres locales (los lee de un archivo de la sesión que no se custodia), va como texto.
- **Las subcarpetas de la versión:** `v52/{ct,frag,run,seco,sub}`, `v53/{frag,g52,run,sanity}` y `v54/frag`, como texto, con la redacción de siempre (`v52/sub/cov-sides.txt` lo cita un registro del Owner); `v52/try/`, `v53/try/` y `v54/try/`, fuera, por el sha256 de su conjunto.
- **El veredicto de CA-T4d:** se lee de su informe propio, con su sección de recomprobación, con la misma comprobación que `bounded_check` en la forma de ese informe.

**Las erratas que declara la emisión** (la recomprobación 2 de CA-T4d, sobre el diff de la v5.3 a la v5.4: EMITIBLE, B 0 / I 0 / M 3; la sesión las corrige en este commit, sin una v5.5):
- **M-14:** «Lo que cambia» de la cabecera de la v5.4 nombra §7, que no cambia, y omite D-PBT4-10, §5.6 y la apertura de §6.4; la columna «Dónde» de V54-04 omite §8.5. Todo lo que cambia cabe en alguna fila V54.
- **M-15:** la nota de D-S16 de `session-pbt4d-design-decisions.md` (cuyo sha256 cita la v5.4, y no se toca) dice `cov_p6_v53.py`, y lo aplica `cov_p6_v54.py`; la definición de PERDIDA de `cov_p6_v54.py:248-249` y `:327` no se ajustó a la ESTABLE nueva (el caso medido da 0); y su «Uso» (`:17`) dice v53. Nada de esto cambia la medida.
- **M-16:** `RedFiles` de la precondición 6 es el RED de la copia `r6y`, el de la v5.3: en `WorkspacePilotUpdateRunTests.cs`, el RED de la v5.4 da `7ea6170422f48334f18ff2cd3b1cb1b32c79264861dd52a6e6b56570daf7ba13`, que solo difiere en comentarios (lo comprueba el generador; decisión de la sesión). `pbt4d-red-v53.txt`, `pbt4d-green-v53.txt` y `pbt4d-retire-list-270.tsv` se custodian como historia.

**Para el Coordinator:** M-14 es el mismo modo que M-10: «Lo que cambia» escrito a mano y no sacado del diff.

**La autoridad n.º 37** es este contrato, `F4B-PBT4-RETIRE-PILOT/coordinator/gate-contract.json`, que escribe la BASE. El contrato anterior, el de PB-T4c, es la n.º 29.

**La comprobación acotada de PB-T4d** (CA-T4d), en `F4B-DRAFTS/pbt4/independent-review-pbt4d-check.md`, con su encargo `independent-review-pbt4d-check-prompt.md`:
- **sobre la v5.2:** EMITIBLE TRAS CORRECCIONES, 0/2/7. Sus importantes: I-01, dos bajas de WPUR dejaban sin prueba código que se queda; I-02, el universo de la cobertura era más estrecho que lo pedido;
- **sus recomprobaciones:** la de la v5.3, EMITIBLE, 0/0/6; la de la v5.4, EMITIBLE, 0/0/3.

El resultado que lleva el contrato es `EMITIBLE TRAS CORRECCIONES; B 0 / I 2 / M 7; las importantes a corregir en una v5.3 (I-01: las pruebas 48 y 49 de WPUR prueban PushBackPilotSystemRebuild, que se queda, y la lista las borra; I-02: el criterio de cobertura solo mira 436 lineas de 2 archivos); con una decision nueva del Owner (I-01, por su regla de owner-pbt4d-run-coverage-decision.md: sustituir la 48 y la 49, 29 sustitutas, o declarar la perdida); precondicion 6 sobre 6c2820ba: base 19619 = 19158 + 0 + 461 igual a la CI 44959653142 registrada, RED 19416 = 19012 + 1 + 403, GREEN 19416 = 19013 + 0 + 403, UI 2464 = 2416 + 0 + 48, las 436 lineas PASAN pero la suite completa de Core pierde PushBackPilotSystemRebuild.cs:32 y 6 ramas (I-01) y 1 rama de PushBackCompositeEditorState.cs:233 (M-02); GREEN y RED reproducidos por objetos git con los mismos sha256; recomprobacion v5.3: EMITIBLE; B 0 / I 0 / M 6 (M-08 a M-13, para una v5.4 de solo texto); I-01 resuelto (29 sustitutas, 26 por el mapa; PushBackPilotSystemRebuild.cs:32 y sus 6 ramas cubiertas por Core en el GREEN) e I-02 resuelto (cobertura de todo src/ reproducida desde los 43 JSON de coverlet: 0 lineas y 0 ramas perdidas, Core sola solo pierde PushBackCompositeEditorState.cs:233, que cubren 7 filtros de UI en su proceso, PERDIDA 0, bajan 16801 + 118, ninguna solo por las corridas juntas); M-01 a M-07 aplicados; 1 sustituta nueva sobre las 29; sin decisiones nuevas del Owner; los tres desajustes del generador son menores, y el 1 pide fijar los sha256 de los 18 JSON de UI por filtro del GREEN, no solo quitar GreenUiCoverage; precondicion 6 sobre 6c2820ba: base 19619 = 19158 + 0 + 461 igual a la CI, RED 19418 = 19014 + 1 + 403, GREEN 19418 = 19015 + 0 + 403, UI 2464 = 2416 + 0 + 48, copia principal sola, 0 warning CS; recomprobacion v5.4: EMITIBLE; B 0 / I 0 / M 3 (M-14 a M-16, de texto y de custodia, sin decision del Owner); M-08 a M-13 resueltos: el JSON fija el sha256 de los 3 + 18 JSON de UI del GREEN y el completo de las TRX y los JSON de los 19 filtros de la base, recalculados por mi y casan, y 6.4.2 nombra sus 32 claves en orden con RedDeleted contra los DELETE y Noise sin leer; [13] dice D-S11 a D-S16; la cabecera de la v5.3 nombra 0, 1, 6.0, 7 y 9; cov_p6_v54.py exige un filtro en su proceso para ESTABLE solo por la UI, PASA, PERDIDA 0, VARIA 0, ESTABLE 16801 + 118, y en el GREEN hay 0 claves que solo cubran las corridas juntas; el RED v5.4 solo cambia comentarios de WPUR (codigo igual en los 28 archivos, aplicado por mi) y el GREEN solo su cabecera; la lista 271 solo cambia el porque de la fila 282 y anade la 284 (directiva); el paso de 436 a 537 queda explicado; nuevos: M-14 (Lo que cambia de la v5.4 nombra la seccion 7, que no cambia, y omite D-PBT4-10, 5.6 y 6.4, el modo de M-10), M-15 (la nota de D-S16 dice cov_p6_v53.py y la definicion de PERDIDA no se ajusto) y M-16 (RedFiles de WPUR es el RED v5.3 sin declararlo, y 11.9 no custodia las piezas de la v5.3); totales, RequiredTests, filtros y decisiones sin cambio`.

**Antes de emitir, la precondición 6** de la v5.4, sobre el cierre `6c2820ba`: los nueve pasos de 6.4.1, en copias exportadas nuevas de 6c2820ba (`r6y`, la principal, sola: el RED literal y el GREEN literal de la v5.3, la suite completa de Core en las dos ramas, los 18 filtros de UI del contrato, el Plugin y la cobertura del GREEN; `r6w`, la base sin cambios, sola, con cobertura: la referencia de la cobertura ampliada y la base de los MinSelected; `r6z`, el control del ensayo de las CT; y `r70`, las piezas de la v5.4 reaplicadas, que solo cambian comentarios de un archivo de tests/, WorkspacePilotUpdateRunTests.cs): la compilacion de Core, de UI y del Plugin sin errores ni avisos CS, tambien con las piezas de la v5.4; Core en el RED, 19418 = 19014 + 1 + 403, y solo cae CT-PBT4D-01, por asercion; en el GREEN, 19418 = 19015 + 0 + 403; los 18 filtros de UI, 0 Failed en las dos ramas; la cobertura ampliada de todo src/ que se queda (Core completa y la union de los filtros de UI de PB-T4c y de PB-T4d, linea a linea y rama por rama, con el sha256 de cada JSON de cobertura): 0 lineas sin ejecutar y 0 ramas perdidas en el GREEN, PASA; el ensayo de las CT (CT-01 cae en el RED y pasa en el GREEN; CT-02 pasa en las dos) y su control (la base con solo las dos CT: caen las dos; el RED con la variante A: cae CT-01); el censo de C13, PASA; los RequiredTests, 83 heredados y 5 nuevos, sin solapamientos; los totales de UI de la CI, 2464 = 2416 + 0 + 48 en las dos ramas; y compile-check.md da el resultado de cada clase que corre. Las corridas de la v5.2 (`r6a` a `r6u`) y los dos ensayos de la v5.3 (`r6v` y `r6x`), como historia. Su custodia está en `F4B-DRAFTS/pbt4/cc4d/`, redactada y con su manifiesto, y su resumen en `F4B-DRAFTS/pbt4/pbt4d-precondition6.json`.

**Los controles** (PBT4D-C11): nc1 con la firma PD-F4B-01 y su sustituto (F4B-E40), y nc2 con el sustituto v2 (F4B-E43), los dos con `--task PBT4`. La primera entrada de la `ExpectedEvidence` es, byte a byte, la de `F4B_EXACT_EVIDENCES` del sustituto v2.

**La reparación del RED** (F4B-E35): la regla RR, solo en las dos clases de las CT. Una reparación no toca `src/`, ni los 26 archivos existentes de `tests/` del alcance, ni las tres clases nuevas que no son CT.

**El contrato:**
- **P-01:** la Snapshot K (F4B-E37).
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.

## F4B-E57. Decisión del Owner sobre el config.toml tras la actualización de la app de Codex: Snapshot L

**Fuente:** `F4B-DRAFTS/coordinator/owner-config-snapshot-l-decision.md`, con la pregunta y la respuesta literales.

**Los hechos:**
- **La parada:** la preparación de la planificación de PB-T4d, `R20261122T171951Z-bc04`, paró con P-01 sin lanzar nada. La app de Codex se había actualizado el 2026-11-22 y había reescrito `config.toml` a las 09:29Z. El hash pasó de `E1F69847…` (Snapshot K) a `B71650A6…`.
- **Lo que cambió:**
  - `mcp_servers.node_repl.env.BROWSER_USE_CODEX_APP_VERSION` pasa de 26.1111.71704 a 26.1117.41037. Cambian dos valores, los que llevan la versión de la app; la lista exacta está en `ChangedValuesVsK` del registro;
  - entra una clave nueva, `mcp_servers.node_repl.env.NODE_REPL_LAZY_TRUSTED_WORKER = "0"`. Los nombres pasan de 104 a 105 (`OrderedNamesSha256` `3ECF8E07…`).

**La respuesta del Owner:** «Sonda neutra, luego L (Recomendado)».

**La sonda neutra** `F4-ENVIRONMENT/probe-R20261122T172638Z-2c14` da **PASS**:
- binario acreditado `4F91AE86…`, gpt-6-luna / high efectivos, solo lectura y CLI 0.167.0-alpha.3.2: es lo que exige su PASS;
- frente a la referencia, la planificación `R20261121T205457Z-af49` de PB-T4c (Snapshot K, mismo binario), el contexto efectivo solo difiere en `root_turn_id`;
- el repositorio y `config.toml` no cambiaron durante la sonda.

**Resultado:** la **Snapshot L** (`B71650A637551747F522BB1D2F4BA1BB9E25AAFF1A61CE1B7BC066197FFFEBB3`) sustituye a la K como línea base activa del Controller en I-64. Su registro está en `F4-ENVIRONMENT/snapshot-L/snapshot-l.json`.
- **La comprobación de config de la cadena la admite.** `f4/cfg_check_f4.py` lleva ahora la lista de nombres por snapshot (`NAMES_BY`): la L, con sus 105 nombres; las demás, con la lista y los 104 de antes. El cambio lo hizo `_apply_snapshot_l_cfg.py`, con reemplazos exactos.
- **La verificación de PB-T4d nombra la L** en su línea de `config.toml`. Es un reemplazo exacto de esa línea, con `_apply_snapshot_l_verify.py`.
- **La planificación de PB-T4d** se lanzó con una preparación nueva, `039a`. La `bc04` no se reutiliza.
- **La custodia:** el registro y esta decisión se custodian con el cierre de PB-T4d, y no antes, porque la base de su cadena tenía que seguir siendo su emisión.
- **El contrato** de PB-T4d se emitió con «today Snapshot K». Su frase operativa, «the accepted Snapshot in force», es la L desde esta decisión, y su verificación lo recibió así.

**Las herramientas** van en `F4-ENVIRONMENT/snapshot-L/tools/`, con su manifiesto. Son las 15 de la carpeta de la sesión:
- la sonda y el registro, derivados de los de la K con reemplazos exactos (`_gen_snapL.py`), y la comparación con la K (`diff_vs_k.py`);
- los dos cambios de la cadena (`_apply_snapshot_l_cfg.py` y `_apply_snapshot_l_verify.py`), con la copia de antes de cada archivo (`*.before-L.py`).

12 van como texto, con la redacción común. Las 3 que, tras la redacción, aún llevan una ruta o un nombre local van solo por SHA-256, declaradas en el manifiesto: `make_pbt4d_verify.before-L.py` (el generador de la verificación antes del cambio), `record_snapshot_l.py` y `diff_vs_k.py`. `cfg_check_f4.before-L.py` no lleva rutas y va como texto.

## F4B-E58. nc2 de PB-T4d: el sustituto v2 acaba sin veredicto, y la decisión del Owner, la v3 del sustituto con su revisión

**Fuente:** `F4B-DRAFTS/coordinator/owner-pbt4d-nc2-decision.md`, con la pregunta y la respuesta literales.

**Los hechos:**
- **nc2** `R20261122T195517Z-096b` dio CONTROL NOT PASSED: EXECUTION_BLOCKED / STOP, FailureClass Contract, con Contract y Scope en fail y S-04, P-03 y PBT4D-C01. La evidencia de Scope nombra la ruta quitada, `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs`. Es la firma de PD-F4-01 en el modo F4B-E21, la misma que en PB-T3a.
- **El sustituto v2 revisado** (`PBTx-NC2-EXACT-WHITELIST-F4B-01` v2, `C033E96C…`, F4B-E43) no dio veredicto. Corrió en `R20261122T201101Z-bb30`, de `F4B-PBT4-RETIRE-PILOT-nc2-substitute/1/`, y acabó en `FAIL (entrada ausente o mal formada: MemoryError: )`. Su propiedad 10 enumera los 2^n subconjuntos del AllowedWriteScope del contrato, y PB-T4d tiene 37 rutas (PB-T4c tenía 21).
- **El diagnóstico de la sesión** (`diag_nc2_p10.py`) no es evidencia ni un sustituto. Corre el código de la v2 tal cual, con solo la propiedad 10 en su forma cerrada: da PASS en las diez propiedades, y las 63 autopruebas siguen pasando.

**La respuesta del Owner:** «Sustituto v3 y revisión (Recomendado)».

**La v3** es `nc2_exact_whitelist_substitute_f4b_v3.py`, SHA-256 `E68B6E5993B522DF83E7CD4CD2B136DDF503E237FD560B1A97067F24664328C6`. La escribe `_make_v3.py` desde la v2 revisada, con reemplazos exactos.
- **Un solo cambio de lógica:** la propiedad 10, en su forma cerrada, es `p is not None and covered(p, ca) and not covered(p, ForbiddenWriteScope)`. Es la misma proposición: toda lista candidata que no cubre la ruta cambiada difiere de la del contrato, que sí la cubre, y la lista vacía no la cubre.
- **Cambian además** el docstring y el campo informativo `AlternativeCandidatesExcludingPath`.
- **No cambian** el id, la autoprueba, `--task` y su ligadura, ni las propiedades 1 a 9.
- **La firma no cambia:** el sustituto solo se usa con la firma autorizada (F4B-E43, PD-F4-01 en el modo F4B-E21). Cualquier otra variación vuelve al Owner.

**La revisión independiente** (`review/independent-review-substitute-v3.md`): **APTO**, B 0 / I 0 / M 4. Comprobó que la v3 solo cambia la propiedad 10 (el AST, nodo a nodo; el generador la reproduce byte a byte), demostró la equivalencia lógica de la propiedad 10 y la contrastó en 300 019 comparaciones sintéticas y 6000 perturbaciones de `evaluate()` sin ninguna discrepancia; la autoprueba da 63 de 63, determinista e idéntica a la de la v2; en los cuatro casos reales en los que la v2 termina (PB-T2a, PB-T3a, PB-T4a y PB-T4c), la v3 da el mismo resultado, las mismas propiedades y las mismas medidas, salvo el campo informativo y `ScriptSha256` (en PB-T4c, la v2 tarda 290,6 s con un pico de unos 25 GB; la v3, 1,1 s); y ninguna mutación de PB-T4c da PASS en la v3 con FAIL en la v2. Sus cuatro menores son de texto y no cambian el script: M-1, el campo informativo es una frase fija (y falsa con `p = None`); M-2, textos de uso desfasados; M-3, `import itertools` sin uso; y M-4, la pregunta al Owner decía que PB-T4c tenía 21 rutas y tiene 26 (el 21 es el diff del RED que cita la evidencia de Scope); M-4 es una errata de la sesión, que no cambia la decisión.

**La corrida de la v3** sobre este nc2, con `--task PBT4`: `R20261122T212159Z-50c9`, de `F4B-PBT4-RETIRE-PILOT-nc2-substitute/1/`, con el script byte a byte (`E68B6E59…`): `PBT4-NC2-EXACT-WHITELIST-F4B-01 F4B-E21 PASS`, las propiedades 1 a 10 en true y `Record.nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B`.

**Resultado:** con la revisión APTO y la v3 en PASS sobre este nc2, `nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B` para F4B-PBT4-RETIRE-PILOT, intento 1 (F4B-E59).
- La v3 sustituye a la v2 de aquí en adelante, PB-T5 incluida.
- La corrida ordinaria de nc2 y la corrida fallida de la v2 se conservan tal cual.
- La v3 se custodia en `F4B-DRAFTS/nc2-substitute-f4b-v3/`, con su nota de custodia (`CUSTODY-NOTE.md`) y su manifiesto. Lleva el script byte a byte, sus dos generadores (`_make_v3.py` y `_make_apply_v3.py`), la revisión en `review/` y el diagnóstico de la sesión en `diag-sesion/`.

## F4B-E59. Cierre de F4B-PBT4-RETIRE-PILOT, sin reparación del RED, con tres decisiones del Owner sobre el código

**Paquete de cierre:** `F4B-PBT4-RETIRE-PILOT/task-close-package.md`.

**La cadena:**
- **La emisión** (F4B-E56): X `23c29236` y BASE `399c9bec`, con el paquete de PB-T4 v5.4 y las tandas 10 a 14 del Owner sobre PB-T4d.
  - **La lista exacta** es `F4B-DRAFTS/pbt4/pbt4d-retire-list-271.tsv` (`05a4d3d1…`), la del alcance B que eligió el Owner. Tiene 284 filas: 271 con acción (borrar 198, reescribir 39, sustituir 33 y acotar 1) y 13 que se quedan.
  - **Sin esqueleto** (D-S11): el RED lleva todo lo de `tests/`, con el texto literal de la pieza `pbt4d-red-v54.txt`, y el GREEN solo `src/`, con el de `pbt4d-green-v54.txt`. Los dos salen de los generadores custodiados, `red_pbt4d_v53.py` y `green_pbt4d_v53.py`.
  - **La forma de la retirada:** la cadena de primera delegación lee del contrato las rutas de `Pilot/` del AllowedWriteScope, los borrados del RED y del GREEN y la lista. Las formas de PB-T4b y PB-T4c siguen intactas en sus ramas.
- **La Snapshot L** (F4B-E57): la preparación `bc04` paró por P-01 antes de la planificación.
- **La planificación** `R20261122T172759Z-039a` se aceptó al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 12 criterios. nc4 es `R20261122T173145Z-27af`: solo A3 en fail, y el oráculo relativo se cumple.
- **El Worker** `R20261122T173325Z-c9e1` (claude-sonnet-5-5/high) dio IMPLEMENTATION_COMPLETE, sin reparación del RED y sin desviaciones.
  - **El RED `aa65ecfd`** (31 rutas, solo de `tests/`: 23 M, 5 A y 3 D): cada archivo es el texto literal de la pieza del RED, que escribe la herramienta del paquete `apply_red_pbt4d_v53.py`; los 3 que borra no están. Lleva:
    - las bajas y las ediciones con registro;
    - los 3 `Skip`, con su equivalente;
    - la prueba lenta, acotada (D-S14);
    - los vacíos de M-04, en su sitio;
    - las 29 sustitutas de WPUR;
    - las dos CT.
  - **El GREEN `7db6fd0c`** (6 rutas de `Pilot/`: 4 M y 2 D) es el texto literal de la pieza del GREEN, que escribe `apply_green_pbt4d_v53.py`, igual a `GreenFiles` y `GreenDeleted`. No toca `tests/`.

**CI** (los totales exactos de cada rama, F4B-E46):
- **RED `45054587789`:** tests en failure; ui-tests y build-ui en success; build-plugin omitido. Core 19 418 = 19 014 + 1 Failed + 403 Skipped; UI 2 464 = 2 416 + 0 + 48. Solo cae CT-PBT4D-01, por aserción.
- **GREEN `45057868915`:** 4/4. Core 19 418 = 19 015 + 0 + 403; UI 2 464 = 2 416 + 0 + 48.
- **La CI de la emisión** (OWNER-01 de la revisión, informativo): la del commit de emisión `399c9bec`, run `45043090710`, quedó en rojo. El detector de cuelgue de la CI (blame-hang, 5 minutos de inactividad) abortó Core en `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_…`, la prueba lenta que PB-T4d acota (D-S14). Es una causa conocida, resuelta en esta cadena: con la cota del RED, la prueba pasa en 7,6 s en la CI del RED y en 9,2 s en la del GREEN. Ese commit solo llevaba documentos.

**El relevo del trabajo:** válido. Comprueba:
- el RED literal y el GREEN literal;
- C02 de la retirada: 138 pruebas borradas y 45 cambiadas, cada una con su fila de la lista, y ninguna otra;
- las tres clases nuevas que no son CT, con su `NewFileSha`;
- los totales exactos;
- cero `warning CS` en 215 registros de build.

**Revisiones:**
- **La directa:** conforme, con un límite del diseño que el Owner ratificó (M-02, abajo).
- **La independiente:** ACEPTABLE CON MENORES 0/0/3, con tres hallazgos OWNER informativos.
  - **M-01:** `RedFiles` de la precondición 6 lleva, en `WorkspacePilotUpdateRunTests.cs`, el RED de la v5.3. Contra la pieza v5.4 (`7ea61704…`), que es la que manda el contrato, el RED es exacto. Es M-16 de la emisión y queda de constancia.
  - **M-02:** en el RED, las dos autopruebas de archivo de CT-PBT4D-01 no discriminan.
  - **M-03:** quedan nombres de pruebas y comentarios que describen como presente el código retirado.
  - **OWNER-01:** la CI de la emisión (arriba).
  - **OWNER-02:** hay tres caminos de `BuildCore` cuyo texto cambia, para el Smoke-PB.
  - **OWNER-03:** la limpieza de M-03.

**Las tres decisiones del Owner sobre el código** (`F4B-DRAFTS/coordinator/owner-pbt4d-code-decisions.md`, literales):
1. **M-02, las autopruebas de archivo de CT-PBT4D-01: «Ratificar como límite del diseño (Recomendado)».**
   - INV-PBT4D-02 dice que, en el RED, «las autopruebas de CT-PBT4D-01 y 02 fallan sobre el texto inyectado». Las dos de archivo (`tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs:111-115`) inyectan un archivo retirado que en el RED aún existe, así que allí no discriminan. Las otras 12 de CT-01 y todas las de CT-02 discriminan en el RED, y en el GREEN discriminan las 14 de CT-01.
   - El criterio del RED se cumple: CT-PBT4D-01 cae por aserción, con Failed == Selected.
   - Es un límite del diseño literal de la CT y una errata de la letra de INV-PBT4D-02, solo para este caso y sin cambio de código. La verificación lo recibió como disposición (`F4B-DRAFTS/pbt4/coordinator-disposition-m02-pbt4d.md`).
2. **OWNER-02, el Smoke-PB: «Sí, sumarlos (Recomendado)».** El Smoke-PB de PB-T5 suma tres caminos de `BuildCore`, junto a la pasada de «Editar todo» → «Actualizar» que ya estaba prevista:
   - un plan que borra un lateral obsoleto;
   - un borrador con el nombre en blanco, que escribe el del sobre de origen en todas las vistas;
   - el rechazo de la frontal B de un rack sin lado B.
3. **OWNER-03, la limpieza de M-03: «Apuntarla en ideas futuras (Recomendado)».** Va a `docs/ideas-futuras.md` como limpieza opcional. No se hace nada en PB-T4d ni en PB-T5.

**La verificación** `R20261122T194536Z-4c59`: **EXECUTION_VERIFIED / NONE, 14/14,** sin hallazgos, con `inputs/coordinator-disposition.md` (`F4B-DRAFTS/pbt4/coordinator-disposition-m02-pbt4d.md`).
- `rr_check` da PASS: no hubo reparación del RED.
- El relevo es válido.
- El Controller no cita la disposición de M-02, porque no evaluó las autopruebas una a una.

**Controles:**
- **nc1:** `R20261122T195309Z-fc6e` da CONTROL NOT PASSED, con la firma exacta de PD-F4B-01: Handoff es la primera en fail, con S-04, y fallan Handoff, Identity, Remote, Scope, Ci y Tests.
  - El sustituto revisado (F4B-E40, `--task PBT4`) corrió en `R20261122T201052Z-07ea`, de `F4B-PBT4-RETIRE-PILOT-nc1-substitute/1/`, y da `PBT4-NC1-CASCADE-SIGNAL-01 PBT4 PASS`: **nc1 queda SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE.**
  - Lo autoriza el contrato (PBT4D-C11) por F4B-E40. No es una variación nueva.
- **nc2:** `R20261122T195517Z-096b` da CONTROL NOT PASSED, con la firma de PD-F4-01 en el modo F4B-E21 (F4B-E58).
  - El sustituto v2 (`R20261122T201101Z-bb30`) acabó sin veredicto, con MemoryError.
  - La v3, revisada, corrió en `R20261122T212159Z-50c9` y da `PBT4-NC2-EXACT-WHITELIST-F4B-01 F4B-E21 PASS`: **nc2 queda SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B.**
- **nc3:** `R20261122T200420Z-4be7` da **CONTROL PASS** ordinario: FreeText, con EXECUTION_REWORK_REQUIRED / REWORK.

Los tres controles quedan satisfechos: nc1 y nc2 por sus sustitutos (el de nc2, la v3 que decidió el Owner en F4B-E58) y nc3 por su corrida ordinaria.

**El registro declarado de las 271** (`F4B-DRAFTS/superseding-characterization.md`, «## PB-T4d»): este cierre completa su tabla de ejecución y la pasa a EJECUTADA. Cambian cinco líneas, con el texto y la fuente en `F4B-DRAFTS/pbt4/superseding-pbt4d-close-fill.md`.

**Resultado:** **F4B-PBT4-RETIRE-PILOT = CLOSED / PASS WITH LIMITATIONS** sobre `7db6fd0c`, con attempts 1 de 3.
- **Se retira el código del piloto que el panel ya no usa desde PB-T4b** (D-PBT4-10, el alcance B):
  - el comparador, con `StructuralEquality`, y el adaptador del campo, borrados;
  - `ComposeForWrite`, `KeepPilotEditsOn` y `PushBackPilotComposition`, fuera del materializador;
  - `PushBackPilotPrepareInput`, `Prepare` y `Build`, fuera del plan;
  - el `Precheck`, el `Execute` y el `ComparePost` del piloto, fuera de la corrida;
  - la rama muerta de `BuildCore`: sin `allowErase`, y `BuildFull` sin su argumento.

  Salen exactamente las 57 declaraciones de C13, y no entra ninguna nueva.
- **Lo que se queda, una vez:** el lector, `Materialize`, `PushBackPilotDraftState`, las constantes, `PrepareFull`, `PushBackFullPrepareInput`, `BuildFull`, `BuildCore`, `ExecuteFull`, el `Precheck` completo, `ComparePostFull` y `ComparePostCore`.
- **Las 271 filas con acción de la lista:**
  - las bajas, las ediciones y las sustituciones, con registro;
  - los 3 `Skip`, con su equivalente;
  - las 29 sustitutas de WPUR, sobre el modelo completo;
  - los vacíos de M-04, en su sitio (D-S13);
  - la prueba lenta, acotada (D-S14).
- **Ningún cambio de comportamiento:** ni el panel, ni la ventana, ni RACKEDITAR, ni el Plugin. `BuildCore` sin `allowErase` es equivalente para su único llamador, y no hay ninguna escritura en el DWG.

**Las limitaciones:**
- Las dos autopruebas de archivo de CT-PBT4D-01 no discriminan en el RED (M-02). Es un límite del diseño, que ratificó el Owner.
- Los tres caminos de `BuildCore` cuyo texto cambia los comprueba el Smoke-PB en el dibujo real (OWNER-02).
- Los nombres y comentarios desfasados (M-03) van a `docs/ideas-futuras.md` como limpieza opcional (OWNER-03).
- M-01, que es M-16 de la emisión (`RedFiles` de WPUR en el JSON de la precondición 6), queda de constancia: el contrato manda la pieza, y contra ella el RED es exacto.
- La cobertura de INV-PBT4D-06 la da la precondición 6, que PASA. La revisión no la midió otra vez, porque pide coverlet.
- Sin AutoCAD. El Smoke-PB de PB-T5 comprueba en el dibujo real lo que le dejan PB-T3c, PB-T4a, PB-T4b, PB-T4c y PB-T4d.

**Siguiente:** la tarea propia de la recarga de «Editar todo» sobre un borrador Clean posiblemente obsoleto. Es OWNER-02 de PB-T4c, y su sitio lo decidió el Owner en `owner-pbt4d-list-decisions.md`, pregunta 2: «Tarea propia después (Recomendado)». Vuelve al Owner con su lista antes de emitirse. Después viene PB-T5: la CI exacta, el Smoke-PB con la firma del Owner y el gate-close de F4B.

## F4B-E60. Emisión de F4B-PBT4-EDIT-ALL-RELOAD (PB-T4e), con el paquete de PB-T4 v5.6

**Fecha:** 2026-11-23.

**Autoridad de emisión:** la decisión del Owner sobre PB-T4e, la 15.ª tanda, `owner-pbt4e-list-decisions.md`, pregunta 4: «Sí, con condiciones (Recomendado)».
- **Las condiciones que exige, cumplidas:**
  - la comprobación acotada de la subtarea, CA-T4e, da 0 bloqueantes y 0 importantes sin resolver: EMITIBLE, 0/0/5; su recomprobación, la de la v5.6, EMITIBLE, 0/0/2;
  - la remedición sobre el cierre anterior, el de PB-T4d, no cambia nada material: la precondición 6 corre sobre copias exportadas nuevas de `160e019d`;
  - la lista emitida es la que eligió el Owner: **vacía**, ninguna prueba existente sustituida (K = 0, D = 0 y E = 0).
- **Cuándo se vuelve al Owner:** por una decisión nueva, una prueba que falle fuera de la lista (aquí, cualquier prueba existente; PBT4E-C16), un efecto sin medir o un intento 3.

**El cierre de PB-T4d** (F4B-E59, `160e019d`): PB-T4d CLOSED sobre `7db6fd0c`, en el intento 1 de 3, sin reparación del RED. CI del GREEN `45057868915`, 4/4: Core 19 418 = 19 015 + 0 + 403, y UI 2 464 = 2 416 + 0 + 48.

**Qué es PB-T4e:** la recarga de «Editar todo» (D-PBT4-22). Antes de abrir la ventana, un borrador Clean marcado como posiblemente obsoleto se recarga desde el dibujo, sin aviso, con la misma recarga que «Editar»; si la recarga no recarga, la ventana no se abre y el campo muestra lo mismo que «Editar».
- **El GREEN** solo toca `src/`, con el texto literal del paquete (`pbt4e-green-v56.txt`): 8 bloques EDIT en los 4 archivos `UNIT_CHANGE` (F4B-E25): el cuerpo de `EditAllOfPilot` en `WorkspaceHostCore.cs`, el aviso de `EditAll` con su constante en `WorkspacePilotFieldViewModel.cs`, y los comentarios de `WorkspaceEditIntent.cs` e `IWorkspacePilotEditPort.cs`.
- **El RED** solo añade las dos clases nuevas de las CT (`pbt4e-red-v56.txt`), sin esqueleto: `PushBackEditAllReloadTests` (CT-PBT4E-01, 02 y 04, Core) y `PushBackEditAllReloadFieldTests` (CT-PBT4E-03, UI), las dos con `ExpectRed` true.
- **Ningún otro cambio visible:** ni «Editar», ni el botón, ni la ventana, ni RACKEDITAR, ni el Plugin; ningún texto ni estado nuevo; cero escrituras en el DWG.

**La lista:** vacía. Ninguna prueba existente se edita, se salta, se reactiva, se borra ni se sustituye. La entrada «## PB-T4e» de `F4B-DRAFTS/superseding-characterization.md` va en esta emisión, con la lista vacía, las dos clases nuevas con su `NewFileSha` y las decisiones del Owner y de la sesión. Su tabla de ejecución se completa al cerrar.

**El total exacto de cada rama** (F4B-E46):
- **GREEN:** Core 19 421 = 19 018 + 0 + 403; UI 2 465 = 2 417 + 0 + 48.
- **RED:** Core 19 421 = 19 015 + 3 + 403, con CT-PBT4E-01, 02 y 04 como `Failed`; UI 2 465 = 2 416 + 1 + 48, con CT-PBT4E-03.

**La 15.ª tanda del Owner** (§8.5 de la v5.6), nueva en esta emisión y literal en `F4B-DRAFTS/coordinator/owner-pbt4e-list-decisions.md`: «Solo la recarga (Recomendado)», «No abrir y avisar (Recomendado)», «Antes de PB-T5 (Recomendado)» y «Sí, con condiciones (Recomendado)». Su origen, ya custodiado: OWNER-02 de la revisión del código de PB-T4c, que el Owner aceptó con «Aceptarlo y corregir luego» (`owner-pbt4c-code-decisions.md`, pregunta 2), y «Tarea propia después» (`owner-pbt4d-list-decisions.md`, pregunta 2).

**Las decisiones de diseño de la sesión,** que no son del Owner (`F4B-DRAFTS/pbt4/session-pbt4e-design-decisions.md`, sha256 `b21f2af4bab5321620bf8de3aceeb95585a0e2fe9e4cc2ae96f18e37c3e41e6d`):
- **D-S17:** la recarga va en el anfitrión (la opción A de la propuesta de lista): `EditAllOfPilot` llama a `TryReloadPilotEdit(request)`, la misma recarga que «Editar», dentro de la marca de lectura de `TryEditAll` y antes de `ShowModal`. **Su coste** (CA-T4e, M-05): «Toca una función», en la pregunta 1 del Owner, se queda corto; el GREEN cambia el cuerpo de `EditAllOfPilot`, el aviso y la constante de FVM, y comentarios de cuatro archivos.
- **D-S18:** se reutiliza `EditAllStatus.NotBegun`, sin valor nuevo; el campo distingue la recarga por `view.Exists`.
- **D-S19** (tras CA-T4e, M-01; ajusta el sitio de D-S17): la recarga va detrás de las tres comprobaciones de la vista (`Applying` o no registrado, `!view.CanEdit` y el texto pendiente) y antes de `ShowModal`, solo sobre un borrador que no se acaba de empezar y con `NeedsLazyReload`. Así, un borrador «Sin decidir», Conflict, Error o con texto pendiente responde exactamente como hoy, sin evaluar `NeedsLazyReload` ni leer; una línea más de las CT lo fija. Es la v5.6.
- **El `TaskId`,** `F4B-PBT4-EDIT-ALL-RELOAD`, confirmado.
- **R-T4-39 (V55-10), sin decisión nueva del Owner:** sin la recarga, la receta de punta a punta no acaba en el conflicto de «Actualizar», como deducía la propuesta de lista, sino en `NotAppliedPreflight`; ninguna de las cuatro respuestas del Owner depende de eso, la premisa que las sostiene («no se escribe nada sin avisar») sigue en pie, y la medida refuerza la corrección elegida. **M-03 de CA-T4e:** la evaluación corregida nombra también las premisas que el Owner leyó en `pbt4e/list-proposal.md:31` y en la pregunta de OWNER-02 de PB-T4c (`owner-pbt4c-code-decisions.md:25`). **En PB-T5 se le cuenta al Owner, como información y no como decisión,** junto al paso 6 del Smoke-PB: con un borrador Dirty y un cambio de fuera que reescriba el sobre de origen, «Actualizar» puede dar `NotAppliedPreflight` y no «Conservar mi borrador».

Ninguna cambia la lista ni el comportamiento visible que decidió el Owner.

**Las decisiones de la sesión sobre el contrato y su emisión** (2026-11-22; tampoco son del Owner):
- **Las autoridades:** el plan de autoridades de la propuesta, aceptado: de las 62 del contrato de PB-T4d siguen la 0 a la 43, con su sección al día en la n.º 0, 6, 28, 29, 36 y 37, y la n.º 43 pasa, en su sitio, al informe de CA-T4e; salen la 44 a la 61, que son de la subtarea de PB-T4d (sus tandas, su lista, sus piezas, su precondición 6, sus decisiones de la sesión y sus `UNIT_CHANGE`); se añaden las de §6.5.2, con el origen del Owner que admite (`owner-pbt4c-code-decisions.md`; véase «Los orígenes del Owner»).
- **El `ForbiddenWriteScope`:** el literal de §6.5: [PKC-C] en `src/`; en `tests/`, la lista explícita del contrato de PB-T4d, menos las dos rutas nuevas de este alcance, más las cinco clases nuevas de PB-T4d. El generador da 352 entradas.
- **PBT4E-C11 y el sustituto de nc2:** la v5.6 trae literal y entero el texto de PBT4D-C11 con la v3 del sustituto de nc2 (`nc2_exact_whitelist_substitute_f4b_v3.py`, `E68B6E59`, F4B-E58, con su revisión APTO); salen la 14 (la decisión de la K) y de la 22 a la 25 (la v2 de nc2), que §6.5.2 excluye, y las de la v3 y las de la L se añaden al final, en el orden de §6.5.2; el generador comprueba el sha256 de la v3 y sus autoridades, sin derivar nada.
- **La precondición 6:** el generador lee `RedFiles`, `GreenFiles`, `RequiredTests` y `Totals` (§6.5.2) y comprueba además las piezas, la lista vacía, las compilaciones, el RED y el GREEN, la cobertura y `Outcome`; `Probe` y `Control` son informativos, como `Alone` y `GreenUiCoverage`. `Task` lleva el `TaskId` con su nota de provisional, tal cual.
- **La custodia de `cc4e/`:** como `cc4d/`: las TRX, la cobertura en XML y JSON, los binarios y el bytecode, solo por sha256; lo demás, como texto redactado.
- **La copia exportada:** `r[4-9][0-9a-z]` (aquí, `r71` a `r83`) pasa a `<copia>`, como en PB-T4d.
- **El límite de rutas:** ninguna ruta custodiada en X pasa de 156 caracteres relativos.
- **Lo del analista de la lista** (`hs65/pbt4e/`): la propuesta de lista va a `F4B-DRAFTS/pbt4/pbt4e/list-proposal.md`; las copias de lectura de `src/`, blobs de `160e019d`, quedan fuera por el sha256 de su conjunto; la 15.ª tanda va a `coordinator/`.
- **Lo que la redacción no tapa, solo por su sha256:** `_review_t4e/report_check.py`, un guion del revisor que lleva un nombre local, y `_review_t4e/report_check2.py`, por lo mismo (la regla fija de la custodia: lo que tras la redacción casa lo prohibido va solo por sha256); y `cc4e/p6_json_pbt4e_v55.py`, por un falso positivo de la ruta con unidad en una expresión regular de hora.
- **Los orígenes del Owner:** la sesión pidió las dos tandas de origen como autoridad, `owner-pbt4c-code-decisions.md` y `owner-pbt4d-list-decisions.md`; §6.5.2 solo admite la primera y excluye las tandas 10 a 14 (la segunda es la 10.ª), y el paquete manda: quedan 55 autoridades. El origen de PB-T4d ya está custodiado, y lo cita `owner-pbt4e-list-decisions.md`.
- **Los ajustes mecánicos del generador al portarlo a la v5.6,** aceptados por la sesión: las claves de la precondición 6 admiten cifras (`NewLineDS19`); el TSV de los `RequiredTests` es el que nombra la precondición 6 (`required-pbt4e-v56.tsv`); el anexo es el de la versión (V56-A); las tablas por clase de `compile-check.md`, idénticas en `r72-p6/` y `r83-p6/`, son las de la copia principal, `r83`; y el guion de la emisión ya no usa un nombre de PB-T4d (`BOUNDED_RESULT_D`) al leer el seco.

**La autoridad n.º 37** es este contrato, `F4B-PBT4-EDIT-ALL-RELOAD/coordinator/gate-contract.json`, que escribe la BASE. El contrato anterior, el de PB-T4d, es la n.º 29.

**La comprobación acotada de PB-T4e** (CA-T4e), en `F4B-DRAFTS/pbt4/independent-review-pbt4e-check.md`, con su encargo `independent-review-pbt4e-check-prompt.md`: EMITIBLE, 0/0/5; su recomprobación, la de la v5.6, EMITIBLE, 0/0/2. Sus cinco menores: M-01, D-S19; M-02, los límites del método de la cobertura, declarados, y la corrida de la cobertura del GREEN sin las dos clases nuevas; M-03 y M-05, arriba; y M-04, un solo armado del paquete. Los dos menores de su recomprobación, de texto y de custodia, sin decisión del Owner:
- **M-06:** el sitio de la recarga que describe D-S17 lo sustituye D-S19 (`!begunHere && draft.NeedsLazyReload`, tras las tres comprobaciones de la vista; el anexo V56-A). El registro de la sesión no se toca: su sha256, `b21f2af4…`, es el que cita la v5.6.
- **M-07:** el sha256 final del informe de CA-T4e es `c16ae0c2…9198`; el que cita la v5.6, `27636ed5…ef72`, es el de sus primeros 30 371 bytes, la comprobación de la v5.5. Lo comprobó la sesión.

El resultado que lleva el contrato es `EMITIBLE; B 0 / I 0 / M 5; sin importantes que corregir (M-01: con un borrador Sin decidir y marcado, el else-if evalua NeedsLazyReload sin proteger el comparador y TryEditAll pasa de Refused sin Report a Failed con Report, solo en el puerto e inalcanzable desde el panel porque el boton exige CanEdit: declararlo o mover la rama tras las comprobaciones de la vista; M-02: el metodo de cobertura incluye las CT nuevas en el GREEN y difflib empareja mal la llave de WHC:1201; M-03: la evaluacion de R-T4-39 llama pregunta 3 a la opcion 3 de la pregunta 1 y no nombra las premisas de list-proposal.md:31 ni de OWNER-02; M-04: post-p6 registra una v5.5 a241bfbe y la revisada es e597790f; M-05: la pregunta 1 dice toca una funcion); D-S17 y D-S18 se sostienen sobre 160e019d; sin decisiones nuevas del Owner; precondicion 6 sobre 160e019d: base Core 19418 = 19015 + 0 + 403 igual a la CI 45057868915, RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403, UI RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48, las 24 lineas de la sonda iguales a las CT, control campo y anfitrion correcto, cobertura PASA, RequiredTests 90 = 88 + 2 iguales a PB-T4d salvo el n.o 84 con ExpectRed false; GREEN y RED reproducidos por objetos git con los mismos sha256; 99 secciones sin cambio; recomprobacion v5.6: EMITIBLE; B 0 / I 0 / M 2 (M-06: el D-S17 del registro de la sesion sigue describiendo el sitio de la v5.5, un else if antes de las comprobaciones, sin remitir a D-S19; M-07: la v5.6 cita CA-T4e con el sha256 27636ed5, el de antes de esta recomprobacion; los dos de texto o de custodia, sin decision del Owner); M-01 resuelto por D-S19 (la recarga tras las tres comprobaciones de la vista, con !begunHere && NeedsLazyReload; nada evalua la marca antes; marcado-sin-decidir da Failed con Report con el GREEN de la v5.5 en r76 y Refused sin fallos ni lecturas con el de la v5.6 en r82 y r83; las otras 24 lineas iguales); M-02 resuelto (los dos limites declarados en 4.5 y en el guion; la corrida opcional sin las clases nuevas, Core 19418 = 19015 + 0 + 403 y la union de 18 filtros 899 = 869 + 0 + 30 tres veces, PASA con los cuerpos vigilados intactos; las 2 VARIA de RackViewBatchDialogPresenter.cs:37-38 varian entre corridas del mismo codigo, la base r73 0/3 frente a la r79 2/3, y el filtro 04 las cubre siempre); M-03 corregido en el registro; M-04 resuelto (post-p6 nombra la 6b8939ae; frente a la 38efafdb solo cambia la linea 9, y en post-p6 solo las dos lineas de la version); M-05 al registro de la emision; PBT4E-C11 es PBT4D-C11 con los reemplazos declarados y la v3 e68b6e59 (APTO; decision del Owner Sustituto v3 y revision, de aqui en adelante); P-01 con la Snapshot L 69781991 solo en PB-T4e, y 6.0 sigue con la K; 109 secciones sin cambio = 99 + 10; RequiredTests 90 y su tsv identicos a la v5.5; precondicion 6 sobre 160e019d: base Core 19418 = 19015 + 0 + 403 igual a la CI 45057868915, RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403, UI RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48, 21 compilaciones sin warning CS, control campo y anfitrion correcto; GREEN y RED de la v5.6 reproducidos por objetos git con los mismos sha256`.

**Los deslices,** frente a la regla de no usar `-c`, `python -` ni un heredoc:
- **de la sesión:** un `python -I -` con un heredoc vacío, que no ejecutó nada y se paró con TaskStop;
- **del redactor de la v5.5** (`v55/handoff-pbt4e.md`, §5): un `python -I -c "print(1)"`, que solo imprimió 1, y un `python -I -` con un heredoc vacío, que no ejecutó nada y se paró con TaskStop;
- **del redactor del generador:** tres `python -I -` (uno con un heredoc vacío y otro con la entrada vacía), que no ejecutaron nada y se pararon con TaskStop, y un `python -I -c ""` vacío.
**Las desviaciones del redactor de la v5.6** (`F4B-DRAFTS/pbt4/v56/handoff-pbt4e-v56.md`, «Desviaciones e historia», 1 a 6):
1. `r77`, historia: su GREEN daba un `warning CS8604` (`begunHere = draft == null`); el final usa `if (draft == null)` con `begunHere = true` dentro.
2. `r78`, historia: un renombrado dejó al conductor sin la lista de filtros de UI, y no corrió ninguno; corregido (con la comprobación de sus 19 líneas) y repetido en `r83`.
3. Tres ejecuciones de `post_p6_v56.sh`: la 1.ª no escribió ninguna versión; la 2.ª armó una v5.6 `38efafdb…2a47`, descartada, cuyo «Lo que no cambia» atribuía las 109 secciones a PB-T4a a PB-T4d; la 3.ª, con los mismos resúmenes, registros, valores, cobertura y `RequiredTests` byte a byte, armó la que manda, `6b8939ae…f66a`. Solo cambia su línea 9. La descartada va como historia de la v5.6 en `F4B-DRAFTS/pbt4/v56/historia/`.
4. Lo añadido después de `r78`: los totales de la corrida opcional de la cobertura, registrados y comprobados, y el nombre de la copia de la sonda (`r82`).
5. Los ensayos en seco de `make_v56.py`, con valores de prueba, en una carpeta borrada: no son versiones.
6. Desvíos menores: una comprobación de sintaxis con `py_compile`, cuyo bytecode se borró, y dos archivos auxiliares escritos por error en una carpeta temporal del sistema y borrados en seguida.

**Antes de emitir, la precondición 6** de la v5.6, sobre el cierre `160e019d`: los nueve pasos de 6.5.1, en copias exportadas nuevas de 160e019d (`r76`, la linea nueva de D-S19 contra el GREEN de la v5.5: cae; `r82`, el ensayo y la sonda de las CT: las lineas de 5.7 sobre el GREEN literal y las del RED de su tabla; `r83`, la principal, sola: el RED literal de tests/ y el GREEN literal de src/, la suite completa de Core en las dos ramas, los 19 filtros de UI del contrato, cada uno en su proceso, el Plugin, la cobertura del GREEN y la corrida opcional sin las dos clases nuevas; `r79`, la base sin cambios, sola, con cobertura: la referencia de la cobertura y la base de los MinSelected; y `r80` y `r81`, el control de las CT; `r77` y `r78`, dos intentos de la v5.6 que no valen, y `r71` a `r75`, los de la v5.5, como historia): 21 compilaciones de Core, de UI y del Plugin, sin errores ni avisos CS; Core en el RED, 19421 = 19015 + 3 + 403, y solo caen CT-PBT4E-01, 02 y 04, por asercion en su Finish; en el GREEN, 19421 = 19018 + 0 + 403; los 19 filtros de UI, en el RED solo cae CT-PBT4E-03, por asercion, y en el GREEN, 0 Failed; la base, 19418 = 19015 + 0 + 403, la de la CI del GREEN de PB-T4d; la cobertura de todo src/ que se queda (Core completa y la UI de los 19 filtros, linea a linea y rama por rama): 0 lineas sin ejecutar y 0 ramas perdidas en el GREEN, 1 linea de texto cambiado y 8 nuevas con codigo, ejecutadas con todas sus ramas, PASA, y bajan 2, informativas (VARIA 2, PERDIDA 0); la corrida opcional, el GREEN sin las dos clases nuevas (Core 19418 = 19015 + 0 + 403 y la union de los 18 filtros heredados, 899 = 869 + 0 + 30), con la misma cobertura, PASA; la linea nueva de D-S19 (`marcado-sin-decidir`): con el GREEN de la v5.5 da Failed con un fallo registrado, y con el de la v5.6, Refused, sin fallos ni lecturas; el control (con solo la parte del campo del GREEN caen las tres CT de Core y pasa CT-PBT4E-03; con solo la del anfitrion, al reves); los RequiredTests, 88 heredados y 2 nuevos, sin solapamientos; y los totales de UI de la CI, 2465 = 2416 + 1 + 48 en el RED y 2465 = 2417 + 0 + 48 en el GREEN. Su custodia está en `F4B-DRAFTS/pbt4/cc4e/`, redactada y con su manifiesto, y su resumen en `F4B-DRAFTS/pbt4/pbt4e-precondition6.json`.

**Los controles** (PBT4E-C11): nc1 con la firma PD-F4B-01 y su sustituto (F4B-E40), y nc2 con la v3 de su sustituto (F4B-E58), los dos con `--task PBT4`.

**La reparación del RED** (F4B-E35): la regla RR, solo en las dos clases de las CT. Una reparación no toca `src/` ni ningún archivo existente de `tests/`.

**El contrato:**
- **P-01:** la Snapshot L (F4B-E57), con la P-01 literal de la v5.6; el generador comprueba la L, no la K.
- **La cadena:** primera delegación; `attempts` 1 de 3.
- **La planificación:** con las colecciones opacas puestas por la sesión (F4B-E21).
- **La verificación:** con las comprobaciones deterministas de F4B-E15.

## F4B-E61. Cierre de F4B-PBT4-EDIT-ALL-RELOAD (PB-T4e), sin reparación del RED, con la decisión del Owner sobre nc1 y sus decisiones sobre PB-T5

**Paquete de cierre:** `F4B-PBT4-EDIT-ALL-RELOAD/task-close-package.md`.

**La cadena:**
- **La emisión** (F4B-E60): X `2e790aa9` y BASE `7da361eb`, con el paquete de PB-T4 v5.6 y la 15.ª tanda del Owner. El contrato es `F4B-PBT4-EDIT-ALL-RELOAD/coordinator/gate-contract.json` (sha256 `279a638c53b211352d14db137f42af73411d7a838e264dcc95930039ff696351`).
  - **La lista, vacía:** ninguna prueba existente se edita, se salta, se reactiva, se borra ni se sustituye (K = 0, D = 0 y E = 0).
  - **Sin esqueleto:** el RED solo añade las dos clases nuevas de las CT, con el texto literal de la pieza `pbt4e-red-v56.txt`, y el GREEN solo toca `src/`, con el de `pbt4e-green-v56.txt`. Los dos salen de los generadores custodiados, `red_pbt4e_v56.py` y `green_pbt4e_v56.py`.
  - **La forma literal:** la cadena de primera delegación lee del contrato el RED y el GREEN literales y la lista vacía (INV-PBT4E-02 y 07), y para limpia en lo que depende de la versión hasta su seco sobre el contrato emitido. Las formas de PB-T4b, PB-T4c y PB-T4d siguen intactas en sus ramas.
- **La planificación** `R20261123T045429Z-48a1` se aceptó al primer lanzamiento, con la cesión de 04:54:50Z a 04:56:38Z: inyección PASS 8/8 (F4B-E21), A1-A8 8/8 y 13 criterios. nc4 es `R20261123T045726Z-77d7`: solo A3 en fail, y el oráculo relativo se cumple.
- **El Worker** `R20261123T045827Z-6c9e` (claude-sonnet-5-5/high) dio IMPLEMENTATION_COMPLETE, `Disposition` NONE, sin reparación del RED y sin desviaciones. Es el intento 1 de 3.
  - **El RED `265f9e31`** (2 rutas nuevas, solo de `tests/`: 2 A, 662 líneas) son las dos clases de las CT, cada una el texto literal de la pieza del RED, que escribe la herramienta del paquete `apply_red_pbt4e_v56.py`, byte a byte su `NewFileSha`:
    - `tests/RackCad.Tests/PushBackEditAllReloadTests.cs`: CT-PBT4E-01, 02 y 04;
    - `tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs`: CT-PBT4E-03.

    `src/` es el de BaseSha, y ningún archivo existente de `tests/` cambia.
  - **El GREEN `0eac7911`** (las 4 rutas de `src/` del alcance, todas M, con 34 líneas añadidas y 7 quitadas) es el texto literal de la pieza del GREEN, que escribe `apply_green_pbt4e_v56.py`, igual a `GreenFiles`. No toca `tests/`.

**CI** (los totales exactos de cada rama, F4B-E46):
- **RED `45119881969`:** tests y ui-tests en failure; build-ui en success; build-plugin omitido. Core 19 421 = 19 015 + 3 Failed + 403 Skipped; UI 2 465 = 2 416 + 1 + 48. Solo caen las cuatro CT, por aserción: CT-PBT4E-01, 02 y 04 en Core y CT-PBT4E-03 en UI.
- **GREEN `45123886125`:** 4/4, con el Plugin. Core 19 421 = 19 018 + 0 + 403; UI 2 465 = 2 417 + 0 + 48.

**El relevo del trabajo:** válido. Comprueba:
- el RED literal (las 2 pruebas nuevas del alcance, sin esqueleto, 0 copias y 0 `Skip`) y el GREEN literal;
- la lista vacía: ninguna prueba existente se toca, y lo saltado es la línea base (en Core, con las 7 filas de D-1 y CT-PBT3A-12);
- los dos filtros con `ExpectRed`, enteros en Failed en el RED, por la pila bajo `tests/`, y ninguna prueba que falle fuera de ellos;
- los totales exactos;
- cero `warning CS` en 15 registros de build.

**Revisiones:**
- **La directa:** conforme.
- **La independiente** (`391adf87…`, con su encargo): **ACEPTABLE CON MENORES, B 0 / I 0 / M 1.** La entrega es el texto literal de las dos piezas, byte a byte, dentro de las 6 rutas; el diseño de D-PBT4-22 y de D-S17 a D-S19 está en el código tal cual; las 4 CT fijan sus 25 líneas del §5.7; no encontró ningún defecto del código entregado.
  - **M-01 (= OWNER-01), informativo, sin decisión:** la recarga ocurre dentro de `TryEditAll`, y el modal se abre justo después, en la misma llamada. El campo solo se repinta al volver `TryEditAll`, al cerrarse el editor. Así que, por el código, con la ventana de «Editar todo» abierta la paleta, deshabilitada, sigue mostrando «Limpio (posiblemente obsoleto)» y el valor de antes del cambio de fuera, mientras la ventana enseña el valor recargado. Al cerrar queda bien (CT-PBT4E-03). Es la misma falta de repintado que PB-T4c ya tiene, y no es un texto ni un estado nuevo. **Se suma a lo que el Smoke-PB de PB-T5 tiene que mirar o anotar** (los pasos 3 y 8 de la guía, §5.5 del paquete), en el estado (`next_action` y `NEXT_ACTION` de `I-64.yml`) y en `docs/ideas-futuras.md`.
  - **Una observación que no cuenta como hallazgo,** anterior a PB-T4e y fuera de su alcance (C10): los resúmenes del puerto y de `BeginEdit` dicen que solo «Editar» lee el dibujo, inexacto desde PB-T4c. Va a `docs/ideas-futuras.md`.

**La verificación** `R20261123T070111Z-b46b`: **EXECUTION_VERIFIED / NONE, 14/14,** sin hallazgos ni paradas, con `VerifiedSha` `0eac7911` y la cesión de 07:01:53Z a 07:06:12Z.
- `rr_check` da PASS: no hubo reparación del RED.
- El relevo es válido.
- **Un hecho que se declara:** el primer `verify_pre` de la sesión dio 2 participantes falsos, porque la línea de órdenes de la propia sesión llevaba la ruta del worktree (la lección de I-61). Se repitió desde el scratchpad y dio 0 participantes y 0 no atribuibles, antes de lanzar. La verificación no se lanzó sobre la medida falsa.

**Controles:**
- **nc1** `R20261123T070642Z-446d`: CONTROL NOT PASSED, con la cascada del SHA falso (`deadbeef…` en la entrega): EXECUTION_BLOCKED / STOP, FailureClass Handoff, con S-04. Difieren de la verificación real Handoff, Identity, Remote, Scope, Ci y Tests.
  - Su sustituto de PD-F4B-01 (`R20261123T072321Z-9cda`, de `F4B-PBT4-EDIT-ALL-RELOAD-nc1-substitute/1/`; ScriptSha256 `41440edc…`, `--task PBT4`) dio FAIL en una sola propiedad, `e2_OtherFailCitesFakeSha`. Las evidencias de Ci y de Tests dan la misma causa con el nombre del campo («not the handoff CurrentSha» y «CurrentSha is not a commit»), sin escribir el valor del SHA falso. Ninguna atribuye el fallo a otra causa (`e3_NoAlienCause` en true). En PB-T4a falló la misma propiedad, con Tests.
  - Es la **séptima variación de nc1 en F4B**, tras PB-TC, PB-TC2, PB-T2a, PB-T2b, PB-T4a y PB-T4b.
  - El Owner eligió «Aceptarlo solo esta vez (Recomendado)» (`F4B-DRAFTS/coordinator/owner-pbt4e-nc1-decision.md`, con la pregunta y la respuesta literales), así que **nc1 = SATISFIED_BY_OWNER_DISPOSITION**, solo para la corrida `446d`. El sustituto no cambia y no se amplía ninguna firma; si la variación vuelve a aparecer, vuelve al Owner. La corrida ordinaria y la del sustituto se custodian tal cual.
- **nc2** `R20261123T071150Z-beaa`: CONTROL NOT PASSED: EXECUTION_BLOCKED / STOP, FailureClass Contract, con Contract y Scope en fail y S-04, P-03 y PBT4E-C01. La evidencia de Scope nombra la ruta quitada, `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs`. Es la firma de PD-F4-01 en el modo F4B-E21.
  - La v3 de su sustituto (`nc2_exact_whitelist_substitute_f4b_v3.py`, `E68B6E59…`, F4B-E58, `--task PBT4`) corrió en `R20261123T072425Z-00b5`, de `F4B-PBT4-EDIT-ALL-RELOAD-nc2-substitute/1/`, y da `PBT4-NC2-EXACT-WHITELIST-F4B-01 F4B-E21 PASS`: **nc2 queda SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B.**
  - Lo autoriza el contrato (PBT4E-C11). No es una variación nueva.
- **nc3** `R20261123T071718Z-ba7f`: **CONTROL PASS** ordinario: FreeText, con EXECUTION_REWORK_REQUIRED / REWORK.

Los tres controles quedan satisfechos: nc1 por la decisión del Owner, nc2 por la v3 de su sustituto y nc3 por su corrida ordinaria. Los tres, con 0 participantes y 0 no atribuibles persistentes.

**Las decisiones del Owner sobre PB-T5,** tomadas en la misma sesión y en el mismo diálogo que la de nc1 (`owner-pbt5-decisions.md` de la sesión, sha256 `2d63b10d2c7ab15ef6e573df0ee69e895ea678946937a6f09b89101611f12a45`): la emisión condicional («Sí, con condiciones»), la adaptación con revisión de la evidencia, la definición de intento con las reglas de Smoke-2, la dispensa de la UI completa, las inyecciones como en Smoke-2, los casos construidos y rotulados, y el recuento del estilo de cotas antes en otros. Su registro **no** se custodia en este cierre: va con la emisión de PB-T5.

**El registro declarado de PB-T4e** (`F4B-DRAFTS/superseding-characterization.md`, «## PB-T4e»): este cierre completa su tabla de ejecución y la pasa a EJECUTADA. Cambian cinco líneas, con el texto y la fuente en `F4B-DRAFTS/pbt4/superseding-pbt4e-close-fill.md`.

**Los deslices,** declarados:
- **de la sesión:** un `python -I -c ""` vacío, añadido por error al final de una orden, que no ejecutó nada; y la primera preparación de la verificación, con la ruta del worktree en su línea de órdenes y 2 participantes falsos (arriba), repetida desde el scratchpad antes de lanzar;
- **del revisor independiente:** una vez `python -c`, solo de lectura, para listar las claves del JSON del contrato, frente a la regla de su encargo. Él mismo lo declara, y no escribió nada ni cambia ningún resultado;
- **del Worker:** la herramienta de shell pasó a segundo plano sus órdenes largas; en tres llamadas lanzó `gh run watch` con `&` o con `Start-Process` y lo esperó en la misma llamada, y paró enseguida una espera hecha con `ping`. Los procesos de salida y de entrada dan 0 participantes y 0 no atribuibles persistentes, y nada de lo que fija el contrato cambia.

**Resultado:** **F4B-PBT4-EDIT-ALL-RELOAD = CLOSED / PASS WITH LIMITATIONS** sobre `0eac7911`, con attempts 1 de 3.
- **La recarga de «Editar todo»** (D-PBT4-22): antes de abrir la ventana, un borrador Clean marcado como posiblemente obsoleto se recarga desde el dibujo, sin aviso, con la misma recarga que «Editar» (`TryReloadPilotEdit`).
  - Va en el anfitrión, en `EditAllOfPilot`, dentro de la marca de lectura de `TryEditAll` (D-S17), tras las tres comprobaciones de la vista y antes de `ShowModal`, solo sobre un borrador que el gesto no acaba de empezar y con `NeedsLazyReload` (D-S19).
  - Si la recarga no recarga, la ventana no se abre y el campo muestra lo mismo que «Editar»: el diagnóstico, la pista y «No se recargó: el borrador se conserva tal como estaba.» (`EditAllStatus.NotBegun` reutilizado, D-S18, con la constante `NotReloadedText`).
  - Como mucho una lectura del dibujo por gesto, y ninguna durante `ShowModal`.
- **Ningún otro cambio visible:** ni «Editar», ni el botón, ni la ventana, ni RACKEDITAR, ni el Plugin; ningún texto ni estado nuevo, ningún miembro nuevo del puerto ni valor nuevo de `EditAllStatus`; cero escrituras en el DWG. Un borrador Dirty, Conflict, «Sin decidir», Error o con texto pendiente responde como antes.
- **La lista vacía:** ninguna prueba existente cambia; entran solo las dos clases de las CT.

**Las limitaciones:**
- Sin AutoCAD: el gesto real, con el modal real y la lectura real del DWG, lo comprueba el Smoke-PB de PB-T5, con los pasos de §5.5.
- Con la ventana de «Editar todo» abierta, la paleta no se repinta hasta cerrarla (M-01, OWNER-01): es informativo, y la guía del Smoke-PB lo anota.
- La cobertura de INV-PBT4E-06 la da la precondición 6, que PASA sobre el mismo texto literal. La revisión no la midió otra vez.
- La revisión comparó los Skipped de UI con la base solo por su recuento (48), no por sus nombres: no tenía la TRX de UI de la CI base.
- nc1, satisfecho por disposición del Owner, solo esta vez: es su séptima variación en F4B, y el sustituto sigue siendo candidato a revisarse, sin cambio ahora.

**Siguiente:** PB-T5: la CI exacta, el Smoke-PB con la firma del Owner y el gate-close de F4B. Las decisiones del Owner sobre PB-T5 están tomadas (arriba); lo siguiente es la v2.3 de su paquete, su comprobación acotada y la emisión condicional. Este cierre no custodia nada de PB-T5, que tendrá su propia emisión.
