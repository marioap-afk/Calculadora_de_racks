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
