# I-64 F4 — Decisiones de la sesión

Registro append-only de las decisiones tomadas por la sesión durante F4, bajo la orden `coordinator/owner-f3-ratification-f4-authorization.md`. Solo aplican a I-64.

## F4-E01. Presupuesto de F4

- **Decisión del Owner:** F4 arranca con presupuesto propio, `attempts` = 1 de 3. El 2 de 3 de F3 no pasa a F4 y su historia queda cerrada.
- **Registro:** `automation_state.attempts` pasa a 1 en el commit que incorpora este archivo, antes de cualquier delegación de F4.

## F4-E02. Régimen de revisión de F4 y siguientes

- **Deuda de proceso (Owner):** un `EXECUTION_VERIFIED` 14/14 no estableció la corrección de producto de F3-T2, donde quedaban seis defectos reales.
- **Regla desde F4:** la verificación del Controller sigue siendo necesaria, pero no suficiente. Toda entrega sustancial de UI o de host recibe además revisión directa del código por la sesión, revisión adversarial independiente del diff y Smoke en host cuando el gate exige comportamiento de host.
- **Uso de intentos:** no se gasta un intento por una preferencia cosmética no contractual. Se corrige, gastándolo si hace falta, cuando un hallazgo incumple el Freeze, el contrato, la seguridad o el comportamiento observable.

## F4-E03. Línea base de `config.toml`

El Owner ratificó F3-E02. **Snapshot F** (`AAEB58D8…`) es la línea base activa para el trabajo del Controller. Snapshot E deja de aceptarse en las comprobaciones. Cualquier otro hash se vuelve a medir contra F y no se acepta por inferencia.

## F4-E04. Limitaciones heredadas de F3

| Id | Disposición del Owner |
|---|---|
| R-1 | Aceptada para F3. La semántica de obsolescencia de F4 es su autoridad natural |
| R-2 | Aceptada. Limpieza de texto posterior |
| R-3 | Aceptada. Deuda de experiencia de F5 o F7 |
| R-4 | Caracterización abierta. F4 no cambia la semántica del eco salvo que sea imprescindible |

**Obligaciones que se arrastran:** SN-06c; la parte de pestañas de SN-10; validación en host de que el manejo de avisos no lee el DWG; validación en host de que los racks borrados desaparecen; Owner Validation con racks reales representativos.

## F4-E05. Conformidad del producto de F3 con la redacción aceptada de IC-04

- **Hecho:** en `206e0f7a`, `AutoCadEventBridge.OnIdle` dispara `Drained`, el controlador abre la ventana y anuncia el estado, y la presentación lee el resumen. La lectura ocurre después del drenaje, pero sincrónicamente dentro de la llamada a `Application.Idle`. No ocurre en manejadores de objetos, de selección ni de comando.
- **Valoración:** la orden admite «después del drenaje», pero pide «una operación separada programada o visible para el usuario» y prohíbe leer «dentro de un callback de Idle simplemente porque se disparó». La frase admite dos lecturas y no se da por conforme por inferencia.
- **Decisión:** obligación explícita de F4-T2, que lo resuelve sacando esa lectura de la pila del Idle. Hasta entonces, la validación en host de F3 no se considera cumplida.

## F4-E06. Paquete del contrato de F4 y revisión independiente

- **Paquete:** `docs/initiatives/I-64-f4-gate-contract-package.md` (v2), preparado desde las autoridades reales.
- **Revisión independiente de la v1:** adversarial, sobre `442e49c3`: 2 BLOQUEANTE, 11 IMPORTANTE y varias MENOR. Todas dispuestas en la v2 (§8 del paquete). Las principales: caracterización del piloto antes de T2 (F4-T1B), reglas explícitas de la StaleBase, proyección única con texto crudo en T2, y la superficie compacta planteada como desviación del Freeze.
- **Decisiones que corresponden al Owner:** C-F4-01 (superficie del piloto) y C-F4-03 (nombre del comando de descarte) antes de T2; C-F4-07 (editor clásico ofrecido frente a IC-03) antes de T3; C-F4-06 (dibujos y operador de Smoke-2) antes de Smoke-2. C-F4-02 reintroduce `ExecuteInCommandContextAsync` en T4B y se avisa al Owner.
- **Decisión:** emitir F4-T1, que no depende de ellas, y devolver las decisiones al Owner antes de T2.

## F4-E07. Corrección de F4-T1 pese a una verificación `EXECUTION_VERIFIED`

- **Problema:** la verificación `R20261110T184730Z-e455` da `EXECUTION_VERIFIED`, 14/14, sobre `aefc34a5`, y nc1..nc3 dan CONTROL PASS. Aun así, la revisión directa de la sesión y la revisión adversarial independiente (régimen de F4-E02) hallan 4 defectos importantes y 10 menores. El detalle está en `F4-T1-DRAFT-CORE/1/R20261110T184730Z-e455/analysis.md`.
  - **I-1:** inicial y actual son el mismo objeto. Incumple D-10 y anula el veto de cierre con el estado mutable del piloto (D-17, INV-20).
  - **I-2:** la ranura de Aplicando se libera con una operación en vuelo. Incumple D-13.
  - **I-3:** una clave repetida en la StaleBase permite una aceptación falsa. Incumple D-12.
- **Opciones:**
  - (a) cerrar y arrastrar los defectos a T2;
  - (b) corrección delegada, consumiendo un intento;
  - (c) corrección de la sesión fuera de la cadena.
- **Decisión:** (b), por la regla de uso de intentos de F4-E02. `attempts` pasa a 2 de 3. La verificación y los controles del intento 1 se conservan y no se reinterpretan.
- **Alcance de la corrección:**
  - I-1 a I-4;
  - los menores contractuales de coste bajo;
  - invariantes nuevos INV-F4T1-19 a INV-F4T1-24, en el contrato del intento 2.
- **Arrastres:**
  - a T2: la inclinación a Veto ante una excepción en la decisión (M-6) y la guarda sobre el uso de la StaleBase (M-9);
  - a T4B: `CompleteApply` en `finally` y el error diferido durante Aplicando.
- **Riesgo:** queda un único intento para T2, T3, T4A, T4B y Smoke-2. Agotarlo es condición de devolución al Owner.

## F4-E08. F4-T1, intento 2: verificado, con un hallazgo de la revisión que pide decisión del Owner

- **Hechos:**
  - la entrega `09955667` da `EXECUTION_VERIFIED` (`R20261110T194129Z-0d3b`), 14/14, con la CI exacta 4/4, y nc1..nc3 dan CONTROL PASS;
  - la revisión adversarial independiente confirma corregidos los 13 hallazgos del intento 1 (I-4 queda parcial);
  - la misma revisión halla N-1, BLOQUEANTE: la regla de «Bom» de la guarda nueva distingue mayúsculas y ya no detecta `BOM`, contra INV-F4T1-22 («Ninguna regla existente de la guarda se debilita»). El producto cumple, y la guarda recursiva del padre sigue cubriendo una palabra que empiece por BOM;
  - halla también N-2, IMPORTANTE: el estado inicial toma por referencia el objeto de la lectura;
  - y 8 menores. El detalle está en `F4-T1-DRAFT-CORE/2/R20261110T194129Z-0d3b/analysis.md`.
- **Por qué no decide la sesión:**
  - la regla de F4-E02 manda corregir, gastando un intento, un hallazgo que incumple el contrato;
  - el intento disponible es el último de F4 (3 de 3), y gastarlo deja T2, T3, T4A, T4B y Smoke-2 sin margen de corrección;
  - la sesión ya devuelve al Owner antes de T2 (C-F4-01 y C-F4-03).
- **Opciones:**
  - (A) corrección del intento 3 en F4-T1, con N-1 a N-10;
  - (B) cerrar F4-T1 con limitaciones y llevar N-1 y N-2 como correcciones obligatorias al contrato de F4-T2, y N-3 a N-10 como obligaciones de T2, T3 y T4, conservando el intento 3;
  - en cualquiera de las dos, el Owner puede además ampliar el presupuesto de F4.
- **Recomendación de la sesión:** (B). La razón es el presupuesto: el comportamiento del núcleo está verificado; N-1 es un hueco de una guarda de pruebas, cubierto en parte por la guarda del padre; y N-2 es un riesgo latente que crea el lector de T2, donde se corrige y se prueba junto a él. La regla de F4-E02, en su letra, apunta a (A).
- **Estado:** F4-T1 no se cierra hasta la decisión. La entrega `09955667` y toda su evidencia se conservan.
- **Fe de erratas del intento 1:**
  - la nota de procesos del registro del trabajo `R20261110T181603Z-3442` dice que el primer recuento de salida dio 2 participantes y que se repitió; es una frase heredada de la plantilla de F3-T1;
  - en esa cesión el primer recuento dio 0 y no se repitió, como acredita su `processes-exit.json` custodiado;
  - el registro custodiado no se edita.

## F4-E09. Disposición del Owner sobre F4-T1 y las decisiones de F4

- **Autoridad:** `F4-DRAFTS/coordinator/owner-f4-disposition-2.md`, texto literal.
- **Presupuesto:** sustituye a F4-E01 para la F4 descompuesta. Cada tarea de F4 contratada por separado empieza con `attempts` 1 de 3 y lleva su propia historia; la historia de F4-T1 no se reescribe. Solo rige en F4 y no cambia AUTOMATION_PLAN. En el estado, `attempts` es el de la tarea de F4 en curso.
- **F4-T1:**
  - el intento 1 queda como REWORK;
  - los tres defectos graves del intento 1 quedan CERRADOS: alias entre inicial y actual, ranura liberada con la operación activa y clave repetida con un «sin cambios» falso;
  - el intento 2 está verificado;
  - el intento 3 de 3 se autoriza solo para N-1, como corrección mínima: restaurar en `WorkspaceDraftBoundaryTests` la detección de BOM que exige INV-F4T1-22, sin distinguir mayúsculas o de forma equivalente, con pruebas de regresión exactas para `BOM`, `Bom`, `bom` y los casos de prefijo, sufijo y nombre de miembro;
  - no entra ningún rediseño de `DraftSession` ni trabajo de T2, T3 o T4;
  - se exigen RED, GREEN, CI exacta, revisión directa, revisión independiente, verificación del Controller y controles normales. Tras PASS se cierra F4-T1, y no se cierra sin corregir N-1.
- **N-2:** es obligación del contrato de F4-T2. Debe establecer una frontera de propiedad: tras la admisión o la materialización, mutar o reutilizar un objeto del host no puede cambiar `InitialDraftState` ni alterar en silencio el cálculo de dirty. Valen como mecanismo un estado inmutable, una copia defensiva en el puerto o al crear la `DraftSession`, u otro con prueba equivalente. Se exige una prueba adversarial que mute o reutilice el objeto de origen tras crear el borrador, que falle antes de la protección y pase después. N-2 no bloquea el cierre de F4-T1.
- **C-F4-01:** se acepta la opción (b) con un límite estrecho, como F4-PILOT-PUSHBACK-01 (IC-06). El contrato de T2 y T3 enumera los campos exactos del piloto antes de implementar; no los elige el Worker.
- **C-F4-03:** se acepta `RACKPANELDESCARTAR`, solo si la ruta contratada de cierre o recuperación necesita un comando. Actúa sobre la sesión y el borrador activos de la Workspace, descarta solo estado en memoria, no escribe nada authored y no afecta a otro documento ni sesión. Ante un destino ambiguo falla con un mensaje claro y sin efecto. No es una API general de comandos del editor.
- **C-F4-07:** se acepta. Para «No editable aquí», F4 muestra el diagnóstico, el motivo si existe y la indicación de usar RACKEDITAR, sin lanzarlo. «Abrir en editor clásico» sigue en F5 (IC-03).
- **C-F4-06:** sin dispensa.
  - Smoke-2 usa dibujos reales y representativos de RackCad, con participación del Owner u operador.
  - La sesión prepara la build exacta, la DLL o el paquete exacto, el procedimiento, los resultados esperados, los formularios de evidencia y las instrucciones de recuperación.
  - F4 no se cierra sin esas observaciones en host.
  - Escenarios mínimos: los 22 de la orden, y además repetir SN-06c, la parte de pestañas de SN-10, Seleccionar, Ver y Localizar sobre racks representativos y la seguridad al cambiar de documento.
- **C-F4-02:** ratificada.
  - T4B puede usar `ExecuteInCommandContextAsync` solo para la mutación explícita de Actualizar, con la excepción limitada a los archivos de mutación que autorice su contrato; nunca para la navegación.
  - T4B prueba en AutoCAD real las obligaciones que enumera la orden.
- **Eventos:** los callbacks no leen el DWG; solo encolan o agrupan pistas. Las relecturas ocurren después, por una ruta de lectura de contexto de aplicación autorizada. T2 y T4 lo preservan y lo prueban.
- **Fe de erratas del intento 1:** nota anexa `erratum-relay-record.md` junto al registro original, sin reescribirlo.

## F4-E10. Excepción acotada a 16.8 para el intento 3 de F4-T1 (N-1)

- **Autoridad:** `F4-DRAFTS/coordinator/owner-f4t1-attempt3-exception.md`, texto literal. Es autoridad de la unidad.
- **Alcance:** solo el intento 3 de 3 de F4-T1 y solo N-1. No relaja 16.8 en general ni crea un cuarto intento. N-2 queda fuera y sigue siendo obligación de F4-T2.
- **Motivo:** las pruebas de regresión y la regla BOM viven en `WorkspaceDraftBoundaryTests.cs`, que ya está en `ChainRedFiles`. Con la regla ordinaria no se puede tener a la vez un RED que falle y un GREEN que corrija la guarda.
- **RED:**
  - solo la parte de pruebas que expone N-1: `BOM`, `Bom`, `bom`, prefijo, sufijo y posición de nombre de miembro;
  - compila y falla por aserción solo en esa superficie;
  - con push y CI exacta, y el job Core en failure por esas pruebas.
- **GREEN:**
  - en el mismo archivo, solo la implementación de la regla BOM;
  - sin añadir, quitar, renombrar ni cambiar pruebas, aserciones ni datos;
  - sin tocar otras familias de guarda, el núcleo ni el código de T2, T3 o T4.
- **Prueba fina:** `F4-T1-DRAFT-CORE/coordinator/n1_hashes.py` calcula, con `git show` en bytes crudos:
  - H1: el bloque de regresión entre marcadores, que debe ser igual en RED y GREEN;
  - H2: el archivo con la región de la regla sustituida, igual en RED y GREEN, de modo que nada cambia fuera de la región;
  - H3: el RED sin el bloque, que debe ser igual al archivo de la base, de modo que el RED solo inserta el bloque.
  La región de la regla son las líneas de comentario contiguas encima de la declaración de `BomInAnyIdentifier` y la propia declaración.
- **Verificación:** solo para `Tests` de este intento, la exclusión de `WorkspaceDraftBoundaryTests.cs` como archivo entero se sustituye por el oráculo estrecho de la orden. El resto de 16.8 y 16.9 no cambia.
- **Regla reparada:** al menos tan fuerte como exige INV-F4T1-22 y como las reglas anteriores, sin tratar como caso especial una cadena literal.
- **Cierre:** con PASS, F4-T1 queda CLOSED / PASS. Si el intento 3 falla por causas de producto, la sesión se detiene, sin ampliar T1 sin una decisión nueva del Owner.

## F4-E11. F4-T1, intento 3: N-1 corregido; la verificación da REWORK por FreeText; STOP (S-11)

- **Producto:** RED `5a27ac8b` y GREEN `ce73ceb7`, solo `WorkspaceDraftBoundaryTests.cs`.
  - CI `43029959059` 4/4; filtro 1300/1300; Core 16838/16838.
  - El RED (`43029071176`) falla con 13 pruebas, todas del bloque y por aserción.
  - `n1_hashes.py` da `Pass` true (H1, H2 y H3).
  - Revisión directa sin defectos.
  - Revisión independiente: 0 bloqueantes, 0 importantes, 2 menores (m-1 cobertura de la regla ante cambios futuros, m-2 precisión del comentario).
- **Verificación `R20261111T140248Z-5556`:** `EXECUTION_BLOCKED` / STOP por `Authority`. La identidad del prompt de la sesión llevaba la AuthorityRevision del intento 2. Fue un error de entrada de la sesión, corregido sin cambiar la entrega. Se conserva.
- **Verificación `R20261111T140847Z-ce94`:** `EXECUTION_REWORK_REQUIRED`, `FailureClass` FreeText.
  - `WorkCompleted` cita un archivo auxiliar del Worker cuyo nombre contiene como palabra completa un término de 16.10.
  - Las otras 13 comprobaciones pasan, incluidas `Tests`, con el oráculo estrecho de F4-E10, y `Ci`.
- **Consecuencia:** corregir la entrega sería el intento 4, con `attempts` en 3 de 3: STOP (S-11). No es un fallo de producto. Se devuelve al Owner, que pidió una decisión nueva para pasar de 3 de 3.
- **Opciones:**
  - (A) **intento 4 acotado a la entrega:** el Worker reescribe `worker-handoff.json` para los mismos commits RED y GREEN, sin commits ni cambios de código ni de pruebas. Después vienen la verificación normal, nc1..nc3 y el cierre;
  - (B) **excepción del Owner a FreeText para esta entrega:** el término solo aparece dentro del nombre citado de un archivo auxiliar, no como declaración de estado. Se cierra sobre `ce94`, con nc1..nc3;
  - (C) **aclaración de la unidad para 16.10 en I-64:** un término dentro de una ruta o un nombre de archivo citados no cuenta. Después, reejecución de la verificación.
- **Recomendación de la sesión:** (A). Conserva intacta la regla de 16.9 (VERIFIED solo con las 14 en pass) y no cambia código.
- **Seguimiento:**
  - m-1 y m-2 a la próxima tarea que toque la guarda.
  - Los prompts siguientes del Worker dirán que los términos de 16.10 también cuentan dentro de los nombres de archivo citados.
  - nc1..nc3 no se ejecutan sobre una verificación REWORK.
