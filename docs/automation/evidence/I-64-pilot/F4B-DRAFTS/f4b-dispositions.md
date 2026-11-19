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
