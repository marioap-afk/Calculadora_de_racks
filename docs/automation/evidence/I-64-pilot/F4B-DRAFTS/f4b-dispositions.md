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
