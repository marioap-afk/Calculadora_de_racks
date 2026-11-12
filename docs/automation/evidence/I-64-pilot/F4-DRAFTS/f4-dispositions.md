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

## F4-E12. Reemisión de la entrega del intento 3 de F4-T1 (F4T1-HANDOFF-REISSUE-01)

- **Autoridad:** `F4-DRAFTS/coordinator/owner-f4t1-handoff-reissue.md`, texto literal.
- **Decisión del Owner:** no hay intento 4, ni excepción a FreeText, ni cambio de 16.10. Se autoriza una sola reemisión de transporte y evidencia de la entrega del intento 3, que sigue siendo el intento de producto 3 de 3.
- **Alcance de la reemisión:** solo se reescribe la referencia textual al nombre del archivo auxiliar que contenía el término de 16.10. Siguen idénticos los SHA, el alcance, la CI, los conteos de pruebas, el resultado del Worker, las limitaciones y el resto de hechos. No hay commits de producto, ni invocación del Worker, ni RED, ni GREEN, ni pruebas.
- **Evidencia histórica:** la verificación `ce94` se conserva tal cual, como `EXECUTION_REWORK_REQUIRED` por FreeText.
- **Antes de la reverificación:** se prueban las diez condiciones de la orden y se registra un diff estructurado de la entrega antigua y la nueva, con sus SHA-256.
- **Reverificación:**
  - es una sola, del mismo intento 3, con la misma delegación, las mismas autoridades y el mismo oráculo estrecho de `Tests`;
  - si da VERIFIED, se ejecutan nc1..nc3 y se cierra F4-T1;
  - si FreeText vuelve a fallar, o falla otra comprobación, STOP sin reemitir otra vez.
- **Prompts del Worker:** los términos de 16.10 quedan prohibidos también dentro de nombres de archivo, rutas, nombres de artefactos y archivos temporales. Los artefactos históricos no se renombran.

## F4-E13. Snapshot G de `config.toml`

- **Autoridad:** `F4-DRAFTS/coordinator/owner-f4t1-config-change.md`, texto literal. Opción 1.
- **Hecho:** el 2026-11-11 a las 16:31Z `config.toml` pasó de `AAEB58D8…` (F) a `7C1516A9…`.
  - Se mantienen los mismos 104 nombres en el mismo orden (`EB8775C5…`), sin claves añadidas ni retiradas.
  - Cambian 8 valores gestionados por la app de Codex, que pasó de 26.1103.60933 a 26.1106.51428: `notify`, y en `mcp_servers.node_repl` el comando, la versión de la app, `CODEX_CLI_PATH`, el directorio de módulos, la ruta de node, las rutas de código de confianza y los servicios de confianza.
  - Modelo, effort, `service_tier` y sandbox no cambian. El binario acreditado del relevo sigue presente con `4F91AE86…`.
- **Sonda neutra `R20261111T180028Z-32f7`:** PASS.
  - Efectivos `gpt-6-luna` / `high`, solo lectura, aprobación `never`, perfil de permisos con lectura restringida y red restringida, y el mismo CLI.
  - Oráculo Git exacto, salida conforme al esquema y sin escrituras Git.
  - Sin participantes y sin avisos de límite; la config no cambió durante la sonda.
  - `turn_context` idéntico al de la sonda de F (`3082`) salvo `root_turn_id`.
  - El aviso del perfil de PowerShell ya aparecía igual en `3082`.
- **Decisión:** G queda aceptada y sustituye a F como línea base activa del Controller en I-64. Registro en `F4-ENVIRONMENT/snapshot-G/snapshot-g.json`, con el inventario semántico sin rutas de usuario ni secretos. Un cambio nuevo es STOP y no crea una H automática.

## F4-E14. Reverificaciones del intento 3 de F4-T1 tras la reemisión

- **Autoridades:** `owner-f4t1-post-custody-identity.md` (sustitución acotada de Identity, condiciones A a I) y `owner-f4t1-second-reverification.md` (corrige solo la parte de red de la condición A), las dos en `F4-DRAFTS/coordinator/`.
- **`R20261111T175635Z-4bb6`:** `EXECUTION_BLOCKED` / STOP por Identity. Defecto de entrada de la sesión: el prompt pedía al Controller `git ls-remote`, y su sandbox no tiene red. Las otras 13 comprobaciones pasan. Se conserva.
- **`R20261111T181015Z-1f4f`:** `EXECUTION_VERIFIED` / NONE, 14/14, sobre el producto `ce73ceb7`.
  - La punta de custodia es `61d83788`, y entre ambos solo hay rutas de `docs/`.
  - El remoto lo midió la sesión antes y después de la cesión, y fue igual a la punta.
  - Config en G, oráculo estrecho de Tests de F4-E10 y entrega reemitida F4T1-HANDOFF-REISSUE-01.

## F4-E15. Controles negativos de la verificación `1f4f`

- **Autoridad:** `F4-DRAFTS/coordinator/owner-f4t1-control-closure.md` y `owner-f4t1-nc3-substitute.md`.
- **nc1:**
  - `c4f7` da CONTROL NOT PASSED. La receta no recalculó `ReissuedSha256`, Handoff falló antes que Identity, y A-13 no aplica tras la custodia. Se conserva.
  - `bc22`, una sola ejecución con la receta corregida, da **CONTROL PASS**: Identity en fail, `EXECUTION_BLOCKED` / STOP.
- **nc2:**
  - `50da` da CONTROL NOT PASSED: Contract y Scope en fail, porque con una sola ruta la mutación vacía el alcance. Se conserva.
  - El sustituto F4T1-NC2-SINGLE-PATH-SCOPE-01 (`5ed5`) da PASS, con la autoprueba A–H y tres extra correcta y las propiedades 1–10 cumplidas.
  - Resultado: **nc2 = SATISFIED_UNDER_F4T1_SINGLE_PATH_SCOPE_SUBSTITUTE**. No es A-4.
- **nc3:**
  - `2f8e` da CONTROL NOT PASSED. El vector de comprobaciones es el del oráculo, con solo FreeText en fail, pero la clasificación escala a STOP (S-04) por dos entradas de la sesión. Se conserva, sin reescribirlo.
  - Esas dos entradas son la afirmación de `reissue-record.json` sobre la entrega real y la cláusula de STOP de la reverificación real.
  - El sustituto F4T1-NC3-FREETEXT-SUBSTITUTE-01 (`b8f7`) da PASS, con la autoprueba A–L determinista y las propiedades 1–14 cumplidas. Sin esas dos contradicciones, el vector clasifica `EXECUTION_REWORK_REQUIRED` / REWORK por FreeText.
  - Resultado: **nc3 = SATISFIED_UNDER_F4T1_FREETEXT_SUBSTITUTE**.
- **Participantes:** en `c4f7` se contaron dos `bash` de la propia orden de la sesión, que llevaba la ruta del worktree. Es el falso participante conocido.

## F4-E16. Cierre de F4-T1-DRAFT-CORE

- **Decisión:** F4-T1 = **CLOSED / PASS** sobre `ce73ceb7`, por la disposición `owner-f4t1-nc3-substitute.md`.
- **Fundamento:**
  - verificación `1f4f`, 14/14;
  - CI `43029959059` 4/4;
  - nc1 `bc22` CONTROL PASS, y nc2 y nc3 satisfechos con sus sustitutos;
  - revisiones directa e independiente de los tres intentos;
  - N-1 corregido con la excepción acotada de F4-E10 y `n1_hashes.py` en Pass.
- **Historia:**
  - intento 1 REWORK (`aefc34a5`);
  - intento 2 REWORK (`09955667`), con N-1 y N-2;
  - intento 3, corrección de producto completa (`ce73ceb7`);
  - `attempts` 3 de 3. No existe intento 4: la reemisión fue solo de transporte y evidencia.
- **Obligaciones que pasan a tareas posteriores:**
  - N-2 a F4-T2, obligatoria;
  - M-6 y M-9 a T2;
  - el copiador profundo del piloto y una entrada por clave, a T2;
  - la UI no usa borradores desligados, a T3;
  - `CompleteApply` en `finally` y los avisos de POST visibles aunque falle su relectura, a T4B;
  - m-1 y m-2 de la revisión del intento 3, a la próxima tarea que toque la guarda.
- **Lecciones de proceso:**
  - los prompts del Worker prohíben también los términos de 16.10 en nombres de archivo, rutas y nombres de artefactos;
  - un Controller sin red no ejecuta órdenes de red: los hechos del remoto los aporta la sesión;
  - las entradas de los controles no deben llevar afirmaciones de la sesión que la mutación vaya a contradecir.

## F4-E17. Inicio de F4-T2-DRAFT-READ-HOST

- **Presupuesto:** `attempts` = 1 de 3, propio de la tarea (F4-E09).
- **Requisitos heredados obligatorios:**
  1. N-2: anti-alias;
  2. el estado del host no puede mutar `InitialDraftState` después de crear el borrador;
  3. una prueba adversarial que muta o reutiliza el objeto de origen;
  4. una sola lectura lógica produce la StaleBase, la admisión y el `InitialDraftState`;
  5. admisión con `RackSiblingMembership`;
  6. sin escritura en el DWG;
  7. IC-04 vinculante: los manejadores de eventos solo encolan;
  8. el piloto acotado es F4-PILOT-PUSHBACK-01;
  9. el contrato enumera los campos exactos del piloto antes de implementar.
- **También T2:** la conformidad con IC-04 de F4-E05, M-6, M-9, el copiador profundo y una entrada por clave.

## F4-E18. Paquete del contrato de F4-T2, descomposición y emisión de F4-T2A

**Paquete:** `docs/initiatives/I-64-f4-t2-gate-contract-package.md` (v4), preparado desde las autoridades reales.

**Revisiones:** pasó por una revisión adversarial independiente en dos rondas.
- **v2:** NO EMITIBLE, con 1 BLOQUEANTE (B-1: los getters con copia invertían INV-F4T1-19), 8 IMPORTANTE y 10 MENOR.
- **v3:** EMITIBLE TRAS CORRECCIONES, con 3 IMPORTANTE y 6 MENOR.
- Todo queda dispuesto en los §9 y §10 del paquete.

**Descomposición:** con la autoridad de `owner-f3-ratification-f4-authorization.md` (`:337-338`), F4-T2 se divide en tres tareas contratadas por separado:
- F4-T2A-PILOT-STATE, en Application pura;
- F4-T2B-DRAFT-READ-HOST, en el host y la lectura;
- F4-T2C-IC04-READ-SCHEDULING, para IC-04.

La mutación sigue solo en T4B.

**Presupuesto:**
- Por F4-E09, cada subtarea empieza en 1 de 3, y T2A recibe el arranque de F4-E17.
- **La división triplica la capacidad de intentos de T2.** Se registra aquí y se avisa al Owner.
- El agotamiento de cualquier subtarea es S-11 y STOP de F4-T2.

**Reubicación de F4-T1B:**
- B1, en la revisión del paquete de F4 v1, pedía la caracterización «antes de tocar el estado del piloto».
- Con IC-06 (b), ninguna tarea anterior a T4A modifica la ventana clásica, los tipos de estado de Push Back ni la clausura de dependencias de la ventana. Sus contratos lo prohíben, y la clausura está listada por rutas en el §3 del paquete.
- **Decisión:** T1B pasa a ejecutarse antes de la primera tarea que modifique un archivo de esa clausura, y como muy tarde antes de T4A.
- Es una reinterpretación del cumplimiento de B1 y queda registrada aquí.

**Piloto (IC-06):**
- **Campo exacto:** `AnnotationScale`, con la validación idéntica a la de la ventana clásica (D-T2-01).
- **Alcance:** solo Push Back de un solo lado (IC-07, punto 1).
- **Capa no editable:** lo authored que el piloto no edita sale de la base vigente (IC-07, punto 2).

**Decisión:** emitir el contrato de F4-T2A, con `attempts` = 1 de 3, nc4 en su primera delegación y el régimen de cierre de F4 (F4-E02).

## F4-E19. Planificación `13b0` de F4-T2A rechazada (A5) y replanificación única

**Hechos de la planificación `R20261111T204805Z-13b0`:**
- Terminó con normalidad. La salida es válida contra el esquema y está en ASCII puro.
- A1-A4 y A6-A8 en pass; **A5 FAIL**.
- En INV-F4T2A-08, el Controller cambió «lo leen» por «los leen» (un carácter insertado en la posición 841), pese a la orden de copiar byte a byte. Todas las demás colecciones son idénticas al contrato.
- nc4 `R20261111T205057Z-2056` cumplió su oráculo relativo sobre esa delegación.

**Clasificación (orden del Coordinator `owner-f4t2a-a5-recovery.md`):**
- Es un defecto de fidelidad de la salida de planificación del Controller, no del contrato, del alcance ni del producto.
- STOP (P-03), REJECTED_BEFORE_INVOCATION. El Worker no se invocó y `attempts` sigue en 1 de 3.
- El análisis está en `F4-T2A-PILOT-STATE/1/R20261111T204805Z-13b0/analysis.md`.

**Decisión: una sola replanificación** con un `RunId` nuevo sobre este commit de custodia, sin cambiar el contrato.
- Se mantienen el `TaskId`, la `AuthorityRevision` `4dc111b5`, las colecciones del contrato y los 18 `AcceptanceCriteria` de `13b0`, copiados byte a byte.
- El prompt prohíbe expresamente corregir la gramática, el número, la puntuación o las tildes, reflujar el texto y parafrasear.
- Antes de aceptar, la sesión compara byte a byte la delegación con sus fuentes. Luego exige A1-A8 con 8 pass, sin excepción en A5, y un nc4 nuevo.
- Si todo pasa, el Worker queda autorizado una vez.
- Si A5 vuelve a fallar por fidelidad: STOP, sin tercera planificación, sin reescribir el contrato y sin Worker.

## F4-E20. Replanificación `61f3` de F4-T2A rechazada (A4) y compactación de `ForbiddenWriteScope` sin reducción posible

**Hechos de `R20261111T211626Z-61f3`** (replanificación única de F4-E19, sobre `0a563cfb`):
- El refuerzo funcionó en lo que atacaba. Los invariantes, los 18 `AcceptanceCriteria` de `13b0`, `RequiredTests`, `StopConditions`, `Authorities`, `ExpectedEvidence`, `Objective` y `AllowedWriteScope` salieron idénticos byte a byte.
- La comparación determinista previa a la aceptación dio **REJECTED**. `ForbiddenWriteScope` tiene 138 entradas en lugar de 139: falta la 55, `src/RackCad.Application/Systems/PushBack/PushBackHighEndBeamGeometry.cs`.
- A1-A8: A4 FAIL; las otras siete en pass.
- Es STOP (P-03) y REJECTED_BEFORE_INVOCATION. El Worker no se invocó y `attempts` sigue en 1 de 3.
- nc4 `e9cb` cumplió su oráculo relativo y no habilita nada.
- `delegation.json` se conserva tal como salió. El análisis está en `F4-T2A-PILOT-STATE/1/R20261111T211626Z-61f3/analysis.md`.

**Orden del Coordinator** (`owner-f4t2a-contract-compaction.md`):
- reemitir el contrato con la misma semántica y un `ForbiddenWriteScope` compacto;
- solo valen prefijos que no cubran ninguna ruta permitida, y archivos sueltos solo donde haga falta;
- hay que probar mecánicamente la equivalencia (propiedades 1-6);
- después, una sola planificación nueva.

**Resultado de la compactación** (`F4-T2A-PILOT-STATE/coordinator/forbidden-compaction-report.json`, sobre `0a563cfb`):
- Las 139 entradas no tienen ninguna redundante: 35 son prefijos y 104 son archivos.
- Ninguno de los 104 archivos tiene un directorio ancestro cuyo subárbol esté prohibido entero sin contener una ruta permitida. El alcance permitido vive dentro de `Systems/PushBack/` (en `Pilot/`), de `Workspace/` (en `Drafts/`) y de `tests/RackCad.Tests/Workspace/`.
- **El conjunto mínimo es idéntico al original: 139 entradas, reducción 0.** La equivalencia se demuestra (propiedades 1-6 en true), pero la representación no cambia y no reduce la presión de copia que motivó la orden.
- La estimación de «unas 40 entradas» que la sesión dio al proponer la opción 1 suponía quitar las enumeraciones de archivos y confiar en la comprobación Scope (diff ⊆ `AllowedWriteScope`). La propiedad 1 de la orden lo excluye.

**Decisión:** la reemisión y la planificación quedan en espera del Coordinator, que debe decidir con este hecho medido. Ningún cambio de `src/` ni de `tests/`.

## F4-E21. Revisión 2 del contrato de F4-T2A: `ForbiddenWriteScope` reducido de 139 a 57

**Orden del Coordinator:** `owner-f4t2a-forbidden-simplification.md`.
- Para esta revisión se sustituye la exigencia de que toda ruta prohibida antes siga prohibida de forma explícita.
- Toda ruta retirada debe seguir sin poder escribirse por estar fuera de la lista blanca exacta `AllowedWriteScope`. Esa lista es la barrera primaria, y `ForbiddenWriteScope` queda como barrera secundaria.

**Reducción determinista** (`F4-T2A-PILOT-STATE/coordinator/reduced-scope-report.json`, desde `forbidden-compaction-report.json` y el contrato de la revisión 1 en `26c80eb4`):
- **Se retiran las 82 entradas** que son archivos sueltos sin prefijo seguro y están dentro de los directorios mixtos `src/RackCad.Application/Systems/PushBack/` (52) y `src/RackCad.Application/Workspace/` (30).
- **Se conservan las otras 57 en su orden.** Ninguna se retira solo por acortar la lista: las guardas de `tests/RackCad.Tests/Workspace/` y los archivos de la raíz siguen.

**Pruebas mecánicas** (`removed-path-proof.json`): todas en PASS.
- **Por cada una de las 82 rutas retiradas:** estaba en el original; no está en `AllowedWriteScope` ni la cubre un prefijo permitido; con el conjunto de cambios {P}, Scope falla por estar fuera de la lista blanca.
- **Por cada una de las 15 entradas permitidas:** Scope no la rechaza.
- **Conjunto retenido:** ningún prefijo retenido contiene una ruta permitida, y ninguna ruta permitida corta el conjunto retenido.
- **Frontera efectiva de escritura:** idéntica en las 7315 rutas del universo.
- **Ruta omitida por `61f3`** (`PushBackHighEndBeamGeometry.cs`): queda protegida por el mecanismo b), estar fuera de la lista blanca exacta.

**Registro explícito:**
- Semántica del producto: SIN CAMBIO.
- `AllowedWriteScope`: SIN CAMBIO.
- `ForbiddenWriteScope`: REDUCIDO de 139 a 57.
- Efecto de seguridad: se retira la denegación explícita redundante de 82 rutas, y la lista blanca exacta sigue siendo la autoridad.
- Motivo: reducir la fragilidad de copia de la planificación tras dos delegaciones rechazadas, sin mover la frontera efectiva.

**Decisión:**
- La revisión 2 se emite en `F4-T2A-PILOT-STATE/coordinator/gate-contract-rev2.json`, con una `AuthorityRevision` nueva (este commit).
- Respecto a la revisión 1 solo cambian:
  - `ForbiddenWriteScope`;
  - `AuthorityRevision`;
  - `IssuedUtc`;
  - en `Authorities`, la autorreferencia más esta orden y el informe.
- No se declara identidad byte a byte con la revisión 1.
- Después, una sola planificación nueva con el prompt reforzado, la comparación byte a byte previa a la aceptación (con `AllowedWriteScope` elemento a elemento en orden), A1-A8 con 8 pass y nc4.
- Si todo pasa, el Worker queda autorizado una vez. Si la copia vuelve a fallar: STOP y vuelta al Coordinator.
- `attempts` sigue en 1 de 3.

## F4-E22. Planificación `981d` de F4-T2A aceptada con una dispensa textual

**Orden del Coordinator:** `owner-f4t2a-planning-acceptance.md`.

**Hechos de la planificación `R20261111T215420Z-981d`** (contra la revisión 2 del contrato):
- A1-A8 8/8 y nc4 `6aea` con su oráculo relativo cumplido.
- Todas las colecciones del contrato idénticas byte a byte, y `AllowedWriteScope` elemento a elemento.
- La comparación estricta dio DIFFERENCE DETECTED en un solo punto, `AcceptanceCriteria[17]`: «conforme a su esquema» pasó a «conforme al esquema».

**Decisión del Coordinator:**
- Se acepta la delegación con UNA dispensa textual, clasificada GRAMMATICAL_NORMALIZATION_ONLY y registrada en `coordinator-waiver.json`.
- La comparación estricta se conserva tal como dio (`byte-fidelity-check.json`), igual que el registro previo con STOP (`relay-record-pre-waiver.json`).
- La dispensa no se generaliza: no autoriza normalizar invariantes, omitir entradas, reordenar, cambiar el alcance, parafrasear ni aceptar diferencias en planificaciones futuras.
- El Worker queda autorizado una vez, con la delegación tal como se emitió.

## F4-E23. Interrupción de transporte del Worker de F4-T2A y reanudación del mismo subagente

**Delegación del Owner** (`owner-night-delegation.md`, texto literal): «Es de noche, por lo que debes avanzar lo máximo posible todos los gates sin mi intervención, toma tú las decisiones». A partir de aquí, las decisiones de la noche las toma la sesión y se registran como tales.

**Hecho** (`F4-T2A-PILOT-STATE/1/R20261111T221932Z-9353/transport-interruption.json`):
- La única invocación del Worker se cortó una vez: FAILED_TURN, sin progreso durante 600 s.
- Causa: el equipo se suspendió por inactividad. La retención keep-awake de la sesión se libera tras unos minutos de sesión inactiva mientras el Worker corre en segundo plano.
- No había ningún commit todavía: HEAD = remoto = BaseSha, con cambios sin commit del Worker.
- La sesión solo consultó metadatos Git y fechas de modificación, sin abrir ningún archivo del Worker.

**Decisión de la sesión:**
- Se reanudó el MISMO subagente, con su transcripción y su prompt. No hubo una delegación ni un Worker nuevos.
- Se recordó que los temporales del Worker estaban fuera del alcance.
- **Mitigación:** un proceso `SetThreadExecutionState` (sin cambiar ajustes) mantiene el equipo despierto durante la cadena nocturna.
- La entrega final de esa misma invocación es IMPLEMENTATION_COMPLETE, con RED `19123dfe` y GREEN `77658968`.
- **Lección:** para cadenas con Workers largos en segundo plano, la retención de keep-awake de la sesión no basta.

## F4-E24. Intento 1 de F4-T2A: verificación STOP por Ci, revisiones y corrección delegada

**Hechos:**
- La entrega `R20261111T221932Z-9353` tiene RED `19123dfe` y GREEN `77658968`.
- La verificación `R20261112T052453Z-8bc4` da `EXECUTION_BLOCKED` / STOP, FailureClass Ci, con F4T2A-C02 activada.
  - 12 comprobaciones en pass; Ci y Tests en fail.
  - RED acreditado: `ChainRedSha` `19123dfe`, `ChainRedFiles` = RT (24 rutas).
- La CI `43160527925` falla en 9 pruebas de Core.
- El análisis está en `F4-T2A-PILOT-STATE/1/R20261112T052453Z-8bc4/analysis.md`.

**Causa raíz de la CI** (decisión de la sesión por la delegación nocturna):
- Es portabilidad de las pruebas, no identidad del producto.
- Las aserciones comparan texto de documentos persistidos generados en Windows (CRLF) con documentos serializados de nuevo en Linux (LF).
- Las comparaciones estructurales del mismo contenido pasan en esa misma CI, y la suite completa pasa en Windows.

**Revisiones:** la directa de la sesión y la adversarial independiente encuentran:
- **IMPORTANTE:** `ComposeForWrite` restaura todo el snapshot editable y todos los inputs de `current`. Tras «Conservar mi borrador» revierte en silencio contenido que el piloto no edita. Choca con IC-07 punto 2 y con D-12, aunque cumple la letra ambigua de INV-F4T2A-07. Es un defecto del contrato.
- **Seis MENORES:**
  - el rebase no es atómico;
  - las pruebas de excepción aceptan Admitido;
  - los deltas antiguos van resumidos;
  - las clases (a), (c) y (d) no tienen evidencia;
  - hay un diagnóstico doble para lo ilegible;
  - `SetCurrentDraftState` no está documentado.

**Opciones:**
- (a) cerrar y arrastrar los defectos;
- (b) corrección delegada que consume un intento;
- (c) corrección de la sesión fuera de la cadena.

**Decisión: (b)**, la misma regla de F4-E07.
- `attempts` pasa a 2 de 3.
- El contrato de la corrección, `gate-contract-attempt2.json`, parte de la revisión 2:
  - reescribe INV-F4T2A-07: composición solo del campo del piloto desde la base vigente, rechazo tipado de las diferencias ocultas fuera del piloto, y el auxiliar `KeepPilotEditsOn` para la superficie;
  - amplía INV-F4T2A-06, 08, 09 y 10;
  - añade INV-F4T2A-16 para el RED de la corrección.
- Mismos `AllowedWriteScope` y `ForbiddenWriteScope` (57).
- Se conservan sin cambios la verificación `8bc4`, la entrega y las revisiones.

## F4-E25. Intento 2 de F4-T2A: verificación, revisiones, controles y cierre

**Hechos del intento 2:**
- **Planificación:** `R20261112T054738Z-a765` dio STOP S-04 por un error de entrada de la sesión: el prompt tenía hechos del intento 1 que ya no eran ciertos. Se repitió como `R20261112T055051Z-3ff0`, con comparación byte a byte PASS y A1-A8 8/8.
- **Worker** `R20261112T055438Z-dd72`: RED `a7ced417` y GREEN `cf9f9441`.
  - Dejó dos temporizadores propios en segundo plano. La sesión detuvo su tarea y comprobó que desaparecieron antes de medir la entrada; es una desviación menor, registrada.
- **CI:** `43167736802` 4/4, con Core en Linux.
- **Verificación** `R20261112T062828Z-00f6`: `EXECUTION_VERIFIED`, 14/14, `VerifiedSha` `cf9f9441`.
- **Revisión directa de la sesión:** sin defectos importantes.
- **Revisión adversarial independiente:** ACEPTABLE CON MENORES, con 0 bloqueantes, 0 importantes y 6 menores.

**Arrastres** (decisión de la sesión por la delegación nocturna; F4-T2A no consume el intento 3):
- **A T4A**, dueña de la composición para escribir y de la entrada caller-owned:
  - (1) el rechazo de `ComposeForWrite` debe comparar `current` sin normalizar contra una copia de `initial` con la escala neutralizada;
  - (3) comprobar que la base rematerializada coincide con `initial` antes de componer, por si cambia el catálogo;
  - (6) rechazar una escala no válida (NaN, 0 o negativa) que haya llegado por edición en sitio;
  - (2) separar en variantes distintas el escenario (b) de «Conservar»;
  - (5) añadir una aserción sin normalizar del corpus canónico en `RackCad.UI.Tests` (Windows).
- **A T4B**, dueña de `CompleteApply`:
  - (4) un copiador que lanza en el rebase de Applied deja el estado calculado sobre la base anterior y un `EnterError` diferido sin aplicar;
  - va junto al arrastre ya registrado de `CompleteApply` en `finally` (F4-E16). Cambiarlo toca D-13 y no corresponde a T2A.
- **A F5 / T1B:** las clases (c) y (d) de la caracterización ventana frente a piloto siguen sin datos que las provoquen. Las divergencias (a)-(d) de la ruta clásica se registran en `docs/ideas-futuras.md`.

**Decisión:** F4-T2A-PILOT-STATE = **CLOSED / PASS** sobre `cf9f9441`, si nc1..nc3 cumplen su oráculo.
- El intento 1 (`77658968`, verificación `8bc4` STOP) y las planificaciones `13b0`, `61f3` y `a765` se conservan como historia.
- F4-T2B-DRAFT-READ-HOST empieza con `attempts` 1 de 3 (F4-E18).

## F4-E26. Paquete de F4-T2B, división en T2B1 y T2B2 y contrato de F4-T2B1

**Autoridad:** delegación nocturna del Owner (`F4-DRAFTS/coordinator/owner-night-delegation.md`): la sesión decide y registra.

**Paquete** (`docs/initiatives/I-64-f4-t2b-gate-contract-package.md`, v3):
- La v1 dio NO EMITIBLE en la revisión adversarial independiente: 1 bloqueante («Comprobar ahora» sin base, porque NotAdmitted no lleva StaleBase), 8 importantes y 8 menores (§9 del paquete).
- La re-revisión de la v2 dio EMITIBLE TRAS CORRECCIONES: cerró el bloqueante y los importantes y abrió 12 menores sin cambio de semántica (§11 del paquete). La v3 los aplica todos. No se pide una tercera ronda.

**Decisiones:**
- **División** de F4-T2B-DRAFT-READ-HOST en dos tareas contratadas por separado:
  - **F4-T2B1-INTENT-READ:** Application pura. Entrada con los hechos sin clasificar del barrido y el texto crudo por clave; StaleBase total, independiente de la admisión; admisión en 14 reglas ordenadas, sin lanzar, con diagnósticos unificados; resultado que lleva siempre la base; guardas nuevas.
  - **F4-T2B2-HOST-LIFECYCLE:** la proyección aditiva del barrido, el adaptador del Plugin y el lector del testigo, el origen desde `NavigationTargets` revalidados y todo el ciclo de vida en el host, con la parte de marcado de C-F4-04 (§6 del paquete).
- **Presupuesto:** cada subtarea empieza en 1 de 3 (F4-E09); T2B1 recibe el arranque de F4-E25. La división duplica la capacidad de intentos de T2B (de 3 a 6): se registra aquí y se avisa al Owner en el informe. El agotamiento de cualquiera de las dos es S-11 y STOP de F4-T2.
- **Sin registro de variables en el piloto:** Push Back no lo usa (D-10 y D-12 lo limitan a Selectivo).
- **Admisión más estricta que Insertar y RACKPROYECTAR:** el piloto pasa a AUTH-13 el texto crudo real. Solo puede dar rechazos falsos, nunca admisiones falsas; Smoke-2 mide la distribución (O-09). Los residuales conservadores se listan en el §7 del paquete.
- **Proyección con solo el texto crudo:** F4 v2 (`:45`) y el paquete de F4-T2 (`:53`) piden «texto crudo y `Section`». T2B2 proyecta solo el texto y `BuildStaleBase` deriva `Section` del mismo texto. Es equivalente y se registra como desviación de la letra; T4A parte de la misma proyección y de la misma función.

**Arrastres:**
- **A T4A:** reutilizar `BuildStaleBase` en la relectura caller-owned y en la comprobación previa O-04.
- **A T3:** capturar el testigo de la preview con el lector de T2B2 (clave y texto crudo); mostrar la indicación de RACKEDITAR y el detalle como información secundaria (INV-31).
- **A F5:** Cama con un solo miembro mutable (D-11) y el registro de variables con el estado de su payload para Selectivo.

**Contrato:** F4-T2B1-INTENT-READ, `attempts` 1 de 3; F4-T2B2 se contrata tras cerrar T2B1.

## F4-E27. Intento 1 de F4-T2B1: verificado, revisión independiente NO ACEPTABLE y corrección delegada

**Hechos del intento 1:**
- **Planificación** `R20261112T081014Z-84ff`: comparación byte a byte PASS en las ocho colecciones del contrato, A1-A8 8/8; nc4 `R20261112T081234Z-1552` con su oráculo (solo A3 falla). Aceptada tal como se emitió.
- **Worker** `R20261112T081315Z-4a75`: RED `04717913` (94 pruebas en rojo, todas dentro del filtro de Core) y GREEN `23792b2d`.
- **CI:** `43184046954` 4/4 sobre el GREEN; `43183114141` sobre el RED, con el job de pruebas de Core en rojo, como se espera.
- **Verificación** `R20261112T084307Z-16bc`: `EXECUTION_VERIFIED`, 14/14, sin hallazgos.
- **Revisión directa de la sesión:** sin defectos importantes; una observación sobre la letra de la regla 5.
- **Revisión adversarial independiente:** NO ACEPTABLE, solo por cobertura de pruebas: la regla 13 no tiene prueba y es alcanzable (I-1), y ninguna prueba ejercita el `catch` de `Read` ante una excepción del comparador (I-2). Además, 10 menores; tres de producto (M-3, M-5 y M-7). Detalle en `F4-T2B1-INTENT-READ/1/R20261112T084307Z-16bc/analysis.md`.

**Decisión** (delegación nocturna): la verificación 14/14 es necesaria pero no suficiente (F4-E02), así que F4-T2B1 no se cierra sobre `23792b2d`.
- **Intento de corrección 2**, `attempts` 2 de 3, con un contrato del intento 2 que parte del de intento 1 y solo cambia y añade invariantes.
- **RED de la corrección** sobre `23792b2d`: las pruebas que faltan (I-1, I-2, M-1, M-2, M-4, M-9 y M-10) y las que fijan los menores de producto (M-3, M-5 y M-7), que fallan por aserción. **GREEN:** solo producto.
- `CorrectionOf` = la verificación `16bc`, con la clase `Tests`: el esquema no tiene una clase para un hallazgo de revisión, y este es de cobertura de las pruebas exigidas.
- **Aclaración de la regla 5 de D-T2B-03 (M-6):** la regla 5 rechaza un origen cuyo Id no es el `RackId` o cuya clasificación es NotMember o Blocking; un origen ReadOnly con el Id correcto pasa a la regla 6. Las comprobaciones que la semántica de `Classify` hace inalcanzables se conservan como defensivas y se marcan así en el código.
- **Limitación aceptada (M-8):** la prueba «`Design` crudo distinto» de D-T2B-08 no es alcanzable vía `Read`, porque el `RawDesign` de cada hermano sale del mismo texto; no se exige.

## F4-E28. Intento 2 de F4-T2B1: replanificación, verificación, revisiones y controles; cierre pendiente del Owner

**Hechos del intento 2:**
- **Planificación** `R20261112T090535Z-58d4`: la comparación byte a byte la RECHAZÓ (`ForbiddenWriteScope[47]` copiado como « .github/», con un espacio delante) y A4 dio FAIL: STOP P-03 antes de invocar al Worker, sin consumir intentos. La sesión autorizó **una** replanificación con el prompt reforzado, por la delegación nocturna y con el precedente `a765`/`3ff0` (F4-E25).
- **Replanificación** `R20261112T090925Z-614a`: fidelidad PASS en las ocho colecciones, A1-A8 8/8. Aceptada tal como se emitió.
- **Worker** `R20261112T091237Z-96a8`: RED `a044bb4b` (solo pruebas; 3 en rojo, las de M-3, M-5 y M-7) y GREEN `fe7f136b` (solo producto).
  - Desviación menor: lanzó suites de prueba en segundo plano, contra la instrucción del prompt, y esperó su resultado. Terminaron antes de la entrega; en la entrada no quedó ningún participante ni proceso no atribuible.
- **CI:** `43190415649` 4/4.
- **Verificación** `R20261112T093234Z-6d39`: `EXECUTION_VERIFIED`, 14/14, `VerifiedSha` `fe7f136b`. El barrido 16.10 determinista de la sesión sobre la entrega real no halla ningún término.
- **Revisión directa de la sesión:** el GREEN hace solo lo dispuesto (M-3, M-5, M-6, M-7 y M-10); el bucle nuevo de hermanos es inalcanzable tras la regla 7 y conservador si se alcanzara.
- **Re-revisión adversarial independiente:** ACEPTABLE CON MENORES. Cierra I-1, I-2, M-1 a M-7 y M-9; M-10 queda parcial. No encuentra regresiones. Abre 4 menores: N-1 (una prueba se presenta como discriminante y pasaría contra el esqueleto del RED; desviación de la letra de la cláusula M-10 de INV-F4T2B1-15), N-2 (el par 9,10 sí se puede construir), N-3 (`Detail` `""` con un Kind vacío) y N-4 (nombres y comentarios).

**Controles** (sobre las entradas de `6d39`; README §10: una sola ejecución con salida válida):
- **nc1** `R20261112T094531Z-d11b`: CONTROL PASS (Identity, `EXECUTION_BLOCKED` / STOP).
- **nc2** `R20261112T094935Z-bd5b`: **CONTROL NOT PASSED** frente al oráculo (FailureClass Scope). El Controller dio `EXECUTION_BLOCKED` / STOP con Scope en fail sobre exactamente la ruta retirada, pero FailureClass Contract: contrastó la delegación mutada con el contrato y con `byte-fidelity-check.json` y vio que el `AllowedWriteScope` estrechado ya no coincide. Con la lista blanca exacta de F4-E21, una delegación estrechada es también una desviación de Contract, que va antes que Scope en el orden de 16.9; en F4-T2A el Controller se fió del archivo de fidelidad y dio Scope. Hallazgo del protocolo: con F4-E21, el oráculo de nc2 no lo alcanza un Controller riguroso.
- **nc3** `R20261112T095410Z-4eb8`: **CONTROL NOT PASSED**. La entrada mutada lleva «GATE PASS» al final de `WorkCompleted` (comprobado por la sesión) y el Controller declaró FreeText `pass` y `EXECUTION_VERIFIED`: un **falso PASS del Controller**. Por eso el barrido 16.10 determinista de la sesión es, de hecho, la única garantía fiable de FreeText; para la entrega real no halla nada.

**Decisión** (delegación nocturna, con escalado): la verificación 14/14 y la revisión ACEPTABLE CON MENORES no bastan para cerrar con dos controles no superados.
- **F4-T2B1-INTENT-READ = VERIFICADA sobre `fe7f136b`, con el cierre PENDIENTE de la disposición del Owner sobre nc2 y nc3** (como en F4-T1, donde el Owner dispuso sustitutos). `attempts` sigue en 2 de 3. La sesión no se dispensa a sí misma ni reescribe los resultados.
- **Propuesta al Owner:**
  - (a) aceptar nc2 con el oráculo adaptado a F4-E21 (bloqueo STOP con Scope en fail sobre la ruta retirada, FailureClass Contract o Scope);
  - (b) para nc3, un sustituto determinista (el barrido 16.10 de la sesión sobre la entrada mutada) y una mejora del protocolo: el barrido determinista de la sesión pasa a ser la autoridad de FreeText y el Controller lo contrasta;
  - (c) cerrar F4-T2B1 como CLOSED / PASS WITH LIMITATIONS con los arrastres N-1 a N-4 a F4-T2B2.
- **Mientras tanto:** la sesión prepara y somete a revisión el paquete de F4-T2B2, sin emitir su contrato hasta la disposición.

## F4-E29. Disposición del Owner sobre F4-T2B1 y F4-T2B2: sustitutos de nc2 y nc3, cierre de F4-T2B1 y D-T2B2-10 ratificada con distinción

**Orden:** `F4-DRAFTS/coordinator/owner-f4t2b1-t2b2-disposition.md`, custodiada tal como llegó. El bloque pegado quedó dentro de la frase del propio Owner: «Acepto tu propuesta p[…]ara nc2, nc3 y D-T2B2-10; emite T2B2A».

**F4-T2B1-INTENT-READ:**
- **nc1** `R20261112T094531Z-d11b`: CONTROL PASS, sin más acción.
- **nc2 ordinario** `R20261112T094935Z-bd5b`: se conserva como **CONTROL NOT PASSED**. Su oráculo ordinario es estructuralmente incompatible con la lista blanca exacta (F4-E21): quitar una ruta del `AllowedWriteScope` exacto viola a la vez Contract y Scope. No se usa A-4, porque Scope falló correctamente.
- **Sustituto F4T2B1-NC2-EXACT-WHITELIST-01** (`F4-T2B1-INTENT-READ-nc2-substitute/2/R20261112T133004Z-59c6`): **PASS**.
  - Autoprueba de 14 casos, dos veces y determinista.
  - Se cumplen las 10 propiedades de la orden. Medido:
    - la ruta quitada es `PushBackPilotIntentReader.cs`, y solo cambia el campo `AllowedWriteScope`;
    - las entradas cambiadas son `delegation.json` y el hash derivado de `relay-record-planning.json`;
    - Scope da pass con el alcance real y fail con el mutado;
    - A3-A5, sin la cláusula de exactitud, dan pass sobre la delegación mutada, así que Contract falla solo por esa cláusula;
    - el Controller informó Scope = fail;
    - las 192 listas alternativas que excluyen la ruta son todas distintas de la del contrato.
  - **Registro:** nc2 ordinario = CONTROL NOT PASSED; **nc2 = SATISFIED_UNDER_F4T2B1_EXACT_WHITELIST_SUBSTITUTE**.
- **nc3 ordinario** `R20261112T095410Z-4eb8`: se conserva tal cual, como **CONTROL NOT PASSED / FALSE PASS BY CONTROLLER**. No se repite.
- **Sustituto F4T2B1-NC3-FREETEXT-DETECTOR-01** (`F4-T2B1-INTENT-READ-nc3-substitute/2/R20261112T133005Z-fb15`): **PASS**.
  - Es local a esta tarea: no reemplaza al Controller ni es autoridad global del protocolo.
  - Lee los siete campos y los 16 términos del propio §16.10 de `AUTOMATION_PLAN.md` en la `AuthorityRevision` `e20a294c` y comprueba que usa exactamente esos.
  - 49 pruebas deterministas: frase exacta, mayúsculas, prosa, casi coincidencias, cada uno de los siete campos, campos estructurados fuera de 16.10 (`Gate` incluido), varias violaciones, una entrega limpia y cada uno de los 16 términos.
  - Entrega real: CLEAN. Entrega mutada: VIOLATION, con una sola coincidencia («GATE PASS» en `WorkCompleted`). Con la frontera de palabra ASCII el resultado es el mismo.
  - Se cumplen las 10 propiedades de la orden. El Controller devolvió FreeText `pass`: es un falso negativo de la señal del control.
  - **Registro:** nc3 ordinario = CONTROL NOT PASSED / FALSE PASS BY CONTROLLER; **nc3 = SATISFIED_UNDER_F4T2B1_FREETEXT_DETECTOR**.
  - **Interpretación declarada de «palabra o frase completa»:** frontera de palabra Unicode (letra, dígito o «_», como en los identificadores; la lista de 16.10 trata `GATE_PASS` y `FINAL_CANDIDATE_SHA` como palabras), frase literal y sin distinguir mayúsculas. Solo difiere de la frontera ASCII cuando hay un «_» o una letra no ASCII junto al término (`InterpretationCases` del resultado); no cambia el resultado de esta tarea.
- **Resultado:** **F4-T2B1-INTENT-READ = CLOSED / PASS WITH LIMITATIONS** sobre `fe7f136b` (paquete de cierre `F4-T2B1-INTENT-READ/task-close-package.md`).
  - Limitaciones: los cuatro menores T2B1/N-1 a N-4, que pasan a F4-T2B2A porque es la dueña de la lectura de la intención; M-8; y los dos sustitutos.
  - El intento 3 no se gasta en los menores.

**Deudas de protocolo** (no se resuelven de forma global dentro de I-64, salvo que una tarea posterior vuelva a bloquearse):
- **PD-F4-01:** con la lista blanca exacta, un control nc2 puede hacer fallar Contract antes que Scope.
- **PD-F4-02:** el Controller dio un falso PASS ante una violación literal de FreeText de 16.10 (nc3).
- No se adopta la propuesta (b) de F4-E28: el barrido de la sesión **no** pasa a ser autoridad global de FreeText.
- **Regla de relevo:** si nc2 o nc3 de una tarea posterior repite su modo de fallo, la sesión lo devuelve al Owner antes de aplicar un sustituto por analogía.

**F4-T2B2:**
- **División ACEPTADA:**
  - F4-T2B2A: cableado de la intención y del host.
  - F4-T2B2B: ciclo de vida, veto de cierre, retirada de la sesión y marcado de obsolescencia.
  - Se emiten por separado: T2B2A primero; T2B2B, cuando T2B2A cierre.
- **Presupuesto:** cada tarea contratada por separado tiene su propio presupuesto de corrección, que empieza en 1 de 3 (F4-E09). Se retira la redacción «la capacidad de T2B queda en 9 intentos» de F4-E26, F4-E28 y del paquete v4: no hay un presupuesto único de T2B.
- **D-T2B2-10: RATIFICADA CON DISTINCIÓN.** Una excepción del comparador no puede dejar un borrador imposible de borrar, ni permitir perder en silencio un estado que puede estar sucio.
  - **A. Evaluación de sucio:** si el comparador lanza mientras se decide si un borrador está limpio, el estado es DESCONOCIDO / ERROR y se trata de forma conservadora para la protección del cierre. Nada automático infiere «limpio».
  - **B. Descarte explícito** (`RACKPANELDESCARTAR`): quitar ese borrador en memoria no requiere el comparador. El fallo del comparador no puede hacer imposible el descarte.
  - **C. Destrucción de la sesión o del documento:**
    - tras un descarte explícito, o con el borrador ya conocido como limpio, la retirada es incondicional y no llama al comparador;
    - si el borrador puede tener trabajo sin guardar o pendiente y no hubo descarte explícito, el fallo del comparador sigue siendo conservador y participa en el veto de cierre.
  - **Comportamiento exigido** (D-17 e INV-20 siguen vinculantes):

    | Estado del borrador | Cierre |
    |---|---|
    | sucio conocido | VETO |
    | texto pendiente | VETO |
    | sucio desconocido por fallo del comparador | VETO |
    | limpio conocido | se permite el cierre normal |
    | descartado explícitamente | se permite quitarlo y cerrar |

  - D-T2B2-10 **no** significa «si el comparador lanza, se permite cerrar». Significa «el fallo del comparador no puede impedir que se complete un descarte explícito o una retirada».
  - **No se debilita `DraftRegistry` en general.** Se conservan:
    - la semántica ordinaria de `Discard`, `DiscardAll` y `RetireSession`, con INV-F4T1-12, INV-F4T1-23 y su prueba de atomicidad;
    - la regla cerrada de F4-T1: la retirada normal no descarta trabajo en silencio.
  - Si T2B2B necesita quitar un borrador descartado explícitamente o retirado con la sesión sin evaluar si está sucio, añade una operación semántica explícita y estrecha (del tipo `Discard(...)` o `RetireKnownSafe(...)`). No cambia la semántica de las operaciones ordinarias.
  - El paquete se actualiza a la v5 con esta distinción antes de emitir. No se reabre ninguna semántica ya acordada.
- **T2B2A:** no recibe ninguna responsabilidad del ciclo de vida. Los menores de T2B1 entran en su contrato solo donde son de su propiedad.
- **T2B2B:** es la dueña de la semántica de alto riesgo:
  - `BeginDocumentClose`;
  - el veto por sucio o texto pendiente;
  - el descarte explícito;
  - la destrucción de la sesión y la retirada de borradores;
  - el marcado de obsolescencia;
  - la contención del fallo del comparador.

  Exige revisión directa e independiente. El comportamiento real del host se mide después en un smoke con AutoCAD.

**Estado tras la orden:**

| Tarea | Estado |
|---|---|
| F4-T1 | CLOSED / PASS |
| F4-T2A | CLOSED / PASS |
| F4-T2B1 | CLOSED / PASS WITH LIMITATIONS |
| Paquete de F4-T2B2 | v4 preparado; autorizado tras el cierre de T2B1 y la v5 |
| F4-T2B2A | 1 de 3 al emitirse |
| F4-T2B2B | en espera; 1 de 3 al emitirse |
| F4-T3 y F4-T4 | en espera |
| Smoke-2 | pendiente |

## F4-E30. Paquete de F4-T2B2 v5 y emisión de F4-T2B2A-INTENT-WIRING

**Paquete v5** (`docs/initiatives/I-64-f4-t2b2-gate-contract-package.md`): aplica la orden del Owner (F4-E29) sin reabrir la semántica acordada. Los cambios están en su §10:
- D-T2B2-10 se reescribe con la distinción A/B/C y la tabla de veto del Owner.
  - Se conservan la semántica ordinaria de `DraftRegistry`, INV-F4T1-12, INV-F4T1-23 y su prueba de atomicidad.
  - F4-T2B2B añadirá operaciones explícitas y aditivas. Su forma exacta la fija y la revisa su contrato.
- No hay presupuesto único de T2B.
- Los arrastres T2B1/N-1 a T2B1/N-4 quedan confirmados en T2B2A.
- Líneas base de T2B2A: 1592 y 248 (CI `43190415649` de `fe7f136b`).

**Hechos:** entre `51163211`, donde se midió el paquete, y `f124b741` solo cambió documentación; `src/`, `tests/` y `.github/` no cambian.

**Reasignación:** el paquete de T2B daba a T2B2 «el punto de entrada para T3». Pasa a T2B2A como dos operaciones de valor con firma fija, `TryBeginPilotEdit` y `TryCheckPilotEdit`. Son código oscuro hasta que T3 las exponga a la UI con su propio puerto y sus propios cambios de guarda.

**Emisión de F4-T2B2A-INTENT-WIRING:** `attempts` 1 de 3 (F4-E09). La sesión redacta el contrato desde el §2, el §4, el §6 y el §10 del paquete v5:
- **`AllowedWriteScope`:** lista blanca exacta de 13 rutas, nueve de la tarea y cuatro de los arrastres de T2B1.
- **`ForbiddenWriteScope`** (70 entradas):
  - prefijos seguros (F4-E21);
  - los archivos de guarda y de pruebas que no cambian;
  - los csproj;
  - los dos archivos que pertenecen a T2B2B: `EventBridgeCore.cs` y `AutoCadEventBridge.cs`.
- **Invariantes:** 12, uno por decisión de D-T2B2-01 a D-T2B2-08, más estos:
  - sin ciclo de vida;
  - sin cambios en guardas ni pruebas existentes;
  - lectura sin escritura;
  - cobertura RED/GREEN.
- **Paradas:** 7 propias de la tarea.

**Controles:** nc4 en la primera delegación de la cadena; nc1..nc3 con su oráculo ordinario. Si nc2 o nc3 repiten el modo de fallo de T2B1, la sesión los devuelve al Owner antes de cualquier sustituto (F4-E29).

## F4-E31. Planificación de F4-T2B2A: rechazo por copia, devolución al Owner, replanificación única y regla para F4

**Hechos:**
- **Planificación** `R20261112T134333Z-a409`: la comparación byte a byte la RECHAZÓ. En `ForbiddenWriteScope[43]` copió `tests/RackCad.Tests/Workspace/WorkspacePaletteHostGuardTests.cs` (sin la palabra «Source») en vez de `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs`.
  - A1-A8: A4 fail; las demás, pass. Horizonte Medium.
  - Resultado: STOP P-03 antes de invocar al Worker, sin consumir intentos.
- **Devolución al Owner:** es el mismo modo de fallo de copia de F4-T2A (`13b0`, `61f3`) y de F4-T2B1 (`58d4`). Una decisión previa solo vincula a la unidad que nombra, así que la sesión **no** aplicó la replanificación de T2B1 por analogía: se lo preguntó al Owner (`F4-DRAFTS/coordinator/owner-f4t2b2a-replanning.md`, pregunta y respuesta literales).

**Orden del Owner:** «Replanificar + regla F4».
- **UNA replanificación** de F4-T2B2A, con el prompt reforzado (cita el rechazo y pide comprobar `ForbiddenWriteScope` y `AllowedWriteScope` índice por índice), sin consumir intentos. Un segundo rechazo vuelve al Owner.
- **Regla para las siguientes tareas de F4:** se fija en sus contratos. Un rechazo por copia antes del Worker permite una replanificación única; un segundo rechazo vuelve al Owner.

**Replanificación** `R20261112T142947Z-4456`:
- comparación byte a byte PASS en las ocho colecciones;
- A1-A8 8/8; Horizonte Medium; 7 criterios de aceptación;
- nc4 `R20261112T143242Z-7099`: solo A3 fail, con el oráculo relativo cumplido.
- Aceptada tal como se emitió. Base `e277dd42`, la misma de `a409`.

## F4-E32. Intento 1 de F4-T2B2A: verificado, revisión independiente NO ACEPTABLE y corrección delegada

**Hechos del intento 1:**
- **Worker** `R20261112T143337Z-4331` (subagente sonnet/high, prompt por archivo con SHA-256 previo):
  - RED `db37e30d`: Core filtrado 1727, 80 fallos por aserción y ninguno por excepción del esqueleto;
  - GREEN `2d743e46`: 13 rutas, exactamente el alcance.
  - Sin participantes ni procesos vivos en la entrada.
- **CI:** `43247864348` 4/4. El RED `43243624408` tiene Core en rojo, UI en verde y el build del Plugin omitido.
- **Verificación** `R20261112T155354Z-46e1`: `EXECUTION_VERIFIED`, 14/14, `VerifiedSha` `2d743e46`. El barrido 16.10 de la sesión no encuentra nada.
- **Condición previa conocida:** la suite completa de UI en Release da 2 fallos de `PendingTextFieldTests`, que dependen de `#if DEBUG`. Ya los midió F4-T2A (`dd72`), y en Debug, que es lo que corre la CI, son 0. La tarea no toca la UI.
- **Revisión directa de la sesión:** el producto no tiene defectos de corrección (`F4-T2B2A-INTENT-WIRING/1/R20261112T155354Z-46e1/analysis.md`).
- **Revisión adversarial independiente:** NO ACEPTABLE, con 0 bloqueantes, 1 importante y 8 menores (`independent-review.md`, en la misma carpeta). El producto no tiene defectos de corrección. El importante, I-1, es que varias pruebas y guardas nuevas prohíben para siempre lo que el paquete v5 asigna a F4-T2B2B (y a F4-T3); T2B2B no podría cambiarlas sin salirse de su alcance.

**Decisión:** la sesión aplica la autorización de correcciones del contrato (`CorrectionsAuthorized`). No enmienda el alcance de T2B2B, porque eso reabriría el paquete v5 ratificado (F4-E29).
- **Intento de corrección 2** de F4-T2B2A (`attempts` 2 de 3), con un contrato del intento 2 (`gate-contract-attempt2.json`). Respecto del contrato del intento 1:
  - añade texto a INV-01, 02, 06, 07, 08 y 09;
  - añade INV-13 para la corrección;
  - añade la parada F4T2B2A-C08 con la regla de replanificación de F4-E31.
- **RED de la corrección**, sobre `2d743e46`:
  - pruebas y guardas re-acotadas o nuevas para I-1, M-2, M-6 y M-9;
  - pruebas que fallan por aserción para M-3, M-4, M-5 y M-7.
- **GREEN de la corrección:** solo producto; ningún archivo de `tests/`.
- **Regla de I-1:** ninguna prueba ni guarda nueva prohíbe de forma permanente lo que el paquete asigna a F4-T2B2B o a la exposición de F4-T3. La ausencia del ciclo de vida se acredita por el diff y la revisión.
- **Los riesgos para el smoke** (bloqueos anidados, DBMOD/UNDO, coste, catálogo, RackLog, compuerta de `ReadPreviewWitness`, defensa del drenaje y referencias de Model Space) pasan a Smoke-2 y a F4-T3.
- **CorrectionOf** = la verificación `46e1`, con la clase `Tests`.

## F4-E33. Intento 2 de F4-T2B2A: verificado, re-revisión NO ACEPTABLE por N-1, órdenes del Owner, controles y cierre

**Hechos del intento 2:**
- **Planificación** `R20261112T160848Z-519b`: fidelidad PASS en las ocho colecciones, A1-A8 8/8, Horizonte Medium. Aceptada tal como se emitió.
- **Worker** `R20261112T161257Z-3d42`:
  - RED `568f845d`: solo pruebas; 12 fallos por aserción, los de M-3, M-4, M-5 y M-7.
  - GREEN `56b0d497`: solo producto; 8 rutas en total.
  - Sin participantes ni procesos vivos en la entrada.
- **CI:** `43259514731` 4/4. El RED `43258759021` tiene Core en rojo.
- **Verificación** `R20261112T170518Z-ed09`: `EXECUTION_VERIFIED`, 14/14, `VerifiedSha` `56b0d497`. El barrido 16.10 de la sesión no encuentra nada.
- **Revisión directa de la sesión:** el producto de la corrección es correcto y mínimo. La re-acotación de I-1 deja sin prohibición permanente lo que corresponde a F4-T2B2B y a F4-T3, salvo lo que encontró la re-revisión.
- **Re-revisión adversarial independiente** (`independent-rereview.md`, junto a la verificación): NO ACEPTABLE, con 0 bloqueantes, 1 importante y 4 menores.
  - Cierra M-2 a M-7 y M-9. Los seis puntos de I-1 quedan cerrados, pero queda **N-1** (IMPORTANTE, solo pruebas): `WorkspaceEditIntentHostTests.cs:1018` exige el conjunto exacto de `EditIntentStatus`, y F4-T2B2B tiene que añadir `NotFound`, `NotNeeded` y `Reloaded` (D-T2B2-12).
  - Menores:
    - **N-2:** en Debug, el `finally` de `TopTransaction` puede lanzar con un documento nulo o inválido. Hoy es latente.
    - **N-3:** la guarda de la obligación 10 reconoce los consumidores solo por el nombre del método.
    - **N-4:** el comentario de `Rejected` omite la tercera vía, el lector.
    - **N-5:** varias guardas deciden de antemano cómo expone F4-T3.

**Orden del Owner 1** (`F4-DRAFTS/coordinator/owner-f4t2b2a-close-decision.md`, pregunta y respuesta literales): «Cerrar y N-1 a T2B2B».
- No se gasta el intento 3.
- Se corren nc1..nc3 sobre `56b0d497` y, si pasan, se cierra como CLOSED / PASS WITH LIMITATIONS.
- El contrato de F4-T2B2B declara el cambio de esa prueba: el enum pasa a fijarse como prefijo ordenado. Es una enmienda del §3 y el §6 del paquete para el alcance de T2B2B.
- Los menores se arrastran a T2B2B y T3.

**Controles** (sobre las entradas de `ed09`):
- **nc1** `R20261112T171355Z-447b`: CONTROL PASS (Identity, `EXECUTION_BLOCKED` / STOP).
- **nc2** `R20261112T171913Z-7a93`: cumple el oráculo mecánico (Scope fail, FailureClass Scope, `EXECUTION_BLOCKED` / STOP), pero por el motivo equivocado. El Controller **no detectó** la ruta quitada.
  - Su evidencia de Scope afirma que las 8 rutas están en `AllowedWriteScope`, lo que es falso: a la delegación mutada le falta `WorkspaceEditIntent.cs`.
  - Atribuye el fallo a la parada F4T2B2A-C02 por dos archivos de prueba propios de la tarea, que la corrección debía cambiar. La verificación real no la aplicó con el mismo diff.
  - Contract dio pass fiándose de la atestación de fidelidad de la delegación real.
  - El relevo de salida se midió dos veces: la primera lectura contó dos `bash.exe` de la propia sesión, un falso P-02 por la ruta en la orden.
- **nc3** `R20261112T172443Z-06a8`: CONTROL PASS por el motivo correcto (FreeText, `EXECUTION_REWORK_REQUIRED`).

**Orden del Owner 2** (`F4-DRAFTS/coordinator/owner-f4t2b2a-nc2-decision.md`): «Sustituto + cerrar».
- nc2 ordinario = **ORÁCULO MECÁNICO CUMPLIDO / SEÑAL NO DEMOSTRADA** (se conserva tal cual).
- **Sustituto F4T2B2A-NC2-SCOPE-SIGNAL-01** (`F4-T2B2A-INTENT-WIRING-nc2-substitute/2/R20261112T173209Z-c91c`): **PASS**.
  - Autoprueba de 10 casos, dos veces.
  - Se cumplen las 8 propiedades:
    - la delegación real coincide con el contrato;
    - `WorkspaceEditIntent.cs` está en el diff y dentro del alcance real;
    - nc2 quita exactamente esa ruta y nada más;
    - los hashes derivados son coherentes;
    - la ruta queda fuera del alcance mutado;
    - Scope tenía que fallar;
    - el Controller dio Scope = fail, con FailureClass Scope y STOP.
  - Medido: la evidencia del Controller no nombra la ruta quitada.
- **Registro:** nc2 = SATISFIED_UNDER_F4T2B2A_SCOPE_SIGNAL_SUBSTITUTE.
- **Deuda de protocolo PD-F4-03:** el Controller puede fallar Scope por un motivo ajeno a la mutación y no detectar la ruta quitada. No se resuelve de forma global dentro de I-64.

**Resultado:** **F4-T2B2A-INTENT-WIRING = CLOSED / PASS WITH LIMITATIONS** sobre `56b0d497`, con `attempts` 2 de 3; el intento 3 no se gasta (paquete de cierre `F4-T2B2A-INTENT-WIRING/task-close-package.md`).

**Arrastres:**
- **A F4-T2B2B:**
  - **N-1:** cambio declarado de la prueba de los nombres de `EditIntentStatus` en `WorkspaceEditIntentHostTests.cs`, que pasa a un prefijo ordenado con `default` igual a `Unavailable`. Exige enmendar el §3 y el §6 del paquete al emitir T2B2B.
  - **N-4:** el comentario de `Rejected`, en `WorkspaceEditIntent.cs`, que está en el alcance de T2B2B.
  - **Redacción de la parada C02 en sus contratos de corrección:** los archivos propios de la tarea, creados en un intento anterior, no son «guardas existentes».
- **A F4-T3:**
  - N-2, cuando `ReadPreviewWitness` tenga llamador;
  - N-5, con sus propios cambios de guarda declarados (H-03);
  - los riesgos de la revisión del intento 1 que son de la UI: la compuerta de `ReadPreviewWitness` y la defensa frente al drenaje.
- **A Smoke-2:**
  - bloqueos anidados;
  - DBMOD y UNDO tras el `Commit` de lectura del barrido;
  - coste de dos recorridos de la BlockTable;
  - dependencia del catálogo en «Comprobar ahora»;
  - fallos del adaptador fuera de RackLog;
  - origen solo con referencias de Model Space.
- **Limitación aceptada:** N-3 (la guarda reconoce los consumidores por nombre) y los residuos que señaló la re-revisión:
  - en el adaptador, la diferencia entre clave nula y en blanco, inalcanzable desde el core;
  - la documentación de `Failed`;
  - el nombre «…OfAnyFile»;
  - el cero de `EditIntentReadStatus`, que el host defiende.

## F4-E34. Paquete de F4-T2B2 v8, IC-08 y emisión de F4-T2B2B-LIFECYCLE

**Paquete** (`docs/initiatives/I-64-f4-t2b2-gate-contract-package.md`, v6 a v8, cambios en su §11). Prepara F4-T2B2B tras el cierre de F4-T2B2A (F4-E33) y la orden del Owner de emitirla después (F4-E29):
- **v6:**
  - fija la forma exacta de D-T2B2-10;
  - añade el descarte del host para `RACKPANELDESCARTAR` (D-T2B2-15);
  - pone los estados de la recarga al final de `EditIntentStatus`;
  - incorpora el cambio declarado (3) por N-1 y N-4;
  - fija las líneas base de T2B2B en 1754 y 248, a partir de la CI `43259514731` de `56b0d497`.
- **Revisión independiente de la v6:** EMITIBLE TRAS CORRECCIONES (0/3/7).
  - V6-1: la marca «seguro» podía quedar caducada.
  - V6-2: la compuerta de `DiscardPilotEdits` era ambigua.
  - V6-3: faltaba la regla de I-1 para las guardas de T2B2B.
- **v7:**
  - el «limpio conocido» exige dos condiciones a la vez: la marca del host y la secuencia de cierre intacta, observada por el puente;
  - `NotePilotEdit` como obligación comprobable de T3 y T4;
  - el asesor se implementa de forma explícita;
  - el descarte del usuario no llama al comparador;
  - `DiscardPilotEdits` solo exige `Authorize`;
  - regla de I-1, trampas, residuales y arrastres a T3.
- **Re-revisión independiente de la v7:** EMITIBLE TRAS CORRECCIONES (0/0/8).
- **v8:** aplica los ocho menores.
  - Lectura declarada de «limpio conocido»: establecido por una evaluación que funcionó **y** todavía vigente en la destrucción. Su consecuencia son posibles falsos inciertos, nunca una pérdida silenciosa.
  - **Regla de devolución:** si Smoke-2 mide que la secuencia nunca queda intacta en CLOSE o CLOSEALL, o que hay falsos inciertos, la sesión lo devuelve al Owner antes de cambiar el mecanismo.
  - El olvido de la secuencia es exhaustivo.
  - El cambio (1) tiene una sola forma: la lista y el comentario cambian y el nombre de la prueba se conserva.
- **No se pide otra ronda:** la re-revisión de la v7 lo declaró emitible tras los menores (precedente de la v3 y la v4).
- **Semántica:** la distinción del Owner de D-T2B2-10 (F4-E29) se conserva literal. La semántica ordinaria de `DraftRegistry` y las pruebas de F4-T1 no cambian.

**IC-08** (`docs/initiatives/I-64-implementation-clarifications.md`): marcado de obsolescencia grueso por sesión, con la obligación vinculante para F4-T4B. La escribe la sesión al emitir, como fija D-T2B2-11.

**Emisión de F4-T2B2B-LIFECYCLE:** `attempts` 1 de 3 (F4-E09, F4-E29). La sesión redacta el contrato desde el §3, el §5, el §6 y el §11 del paquete v8:
- **`AllowedWriteScope`:** lista blanca exacta de 12 rutas, con los cambios declarados (1) y (3).
- **`ForbiddenWriteScope`** (77 entradas):
  - prefijos seguros;
  - los demás archivos de `Drafts/`;
  - las guardas y pruebas que no cambian, incluidas las de F4-T1 y las de T2B2A;
  - los archivos de producto fijos.
- **Invariantes:** 13, de D-T2B2-09 a D-T2B2-15, más N-4, la regla de I-1, las guardas existentes y la cobertura RED/GREEN.
- **Paradas:** 8 propias, entre ellas C02 para las correcciones y C08, la regla de replanificación de F4-E31.

**Controles:** nc4 en la primera delegación de la cadena; nc1..nc3 con su oráculo ordinario. Si nc2 o nc3 repiten el modo de fallo de T2B1 o T2B2A, la sesión los devuelve al Owner antes de cualquier sustituto (F4-E29 y F4-E33).

T2B2B requiere revisión directa e independiente (orden del Owner). Su comportamiento real se mide después en Smoke-2.

## F4-E35. F4-T2B2B-LIFECYCLE: intento 1 verificado, revisiones, órdenes del Owner, controles y cierre

**Hechos del intento 1:**
- **Planificación** `R20261112T183448Z-7813` (primera delegación de la cadena): fidelidad PASS en las ocho colecciones a la primera, A1-A8 8/8, Horizonte Medium. nc4 `R20261112T183801Z-fc9a`: solo A3 en fail, con su oráculo cumplido. Aceptada tal como se emitió.
- **Worker** `R20261112T183825Z-36c7`:
  - subagente con `claude-sonnet-5-5`/high, 306 turnos, 0 denegaciones;
  - RED `a2e0ec13`: pruebas y esqueleto inerte; 128 fallos por aserción, todos dentro de `RackCad.Tests.Workspace`;
  - GREEN `8f249e3a`: solo producto; 12 rutas en total, exactamente el `AllowedWriteScope`.
  - El relevo de salida se midió tres veces: las dos primeras lecturas contaron dos `bash.exe` de la propia sesión, un falso P-02 por la ruta del worktree en la orden. La tercera, custodiada, da 0 participantes. En la entrada, 0 participantes.
- **CI:** `43294223849` 4/4. Core: filtro 1919/1919, suite 17457/17457. UI: filtro 248/248. El RED `43287541049` tiene Core en rojo.
- **Verificación** `R20261112T201718Z-060d`: `EXECUTION_VERIFIED`, 14/14, `VerifiedSha` `8f249e3a`. El barrido 16.10 de la sesión no encuentra nada.
- **Revisión directa de la sesión** (`direct-review.md`, junto a la verificación): producto conforme a INV-F4T2B2B-01..09 y a la tabla de veto del Owner (F4-E29). Dos menores en las guardas (D-1 y D-2).
- **Revisión adversarial independiente** (`independent-review.md`): ACEPTABLE CON MENORES (0/0/5). Sin defectos de producto y con la tabla del Owner cumplida en sus cinco filas.
  - **M-1:** una guarda impide envolver el asesor, lo que contradice la letra de INV-F4T2B2B-11.
  - **M-2:** conteos y conjuntos exactos de archivo entero.
  - **M-3:** una prueba fija como «silent» el residual declarado.
  - **M-4:** la documentación de `Waiting` y `NotAdmitted` es falsa para la recarga.
  - **M-5:** dos huecos de cobertura.

  El análisis conjunto está en `analysis.md`.

**Orden del Owner 1** (`F4-DRAFTS/coordinator/owner-f4t2b2b-close-decision.md`, pregunta y respuesta literales): «Cerrar y arrastrar a T3».
- La sesión devolvió al Owner el modo de fallo repetido de la regla I-1 antes de clasificarlo.
- No se gasta el intento 2.
- La contradicción de M-1 con INV-F4T2B2B-11 queda registrada como desviación.
- Los menores pasan a cambios declarados del contrato de F4-T3.

**Controles** (sobre las entradas de `060d`):
- **nc1** `R20261112T202615Z-e92e`: **oráculo ordinario NO cumplido / señal demostrada.**
  - El Controller detectó la mutación: el CurrentSha `deadbeef…` no resuelve y contradice el HEAD, el remoto y la CI.
  - Dio `EXECUTION_BLOCKED`/STOP con S-04, pero la primera comprobación en fail es Handoff (AUTOMATION_PLAN 16.9: «Salida conforme pero vacía o falsa»), no Identity como fija README §10. Remote, Ci y Tests fallan en cascada.
  - Es un modo de fallo nuevo para nc1. No se reejecuta (README §10) y se devolvió al Owner antes de cualquier sustituto (F4-E29).
- **nc2** `R20261112T203204Z-c514`: CONTROL PASS por el motivo correcto. Solo Scope en fail, nombrando `DraftRegistry.cs`; `EXECUTION_BLOCKED`/STOP.
- **nc3** `R20261112T203605Z-97e6`: CONTROL PASS por el motivo correcto. Solo FreeText en fail, en `WorkCompleted`; `EXECUTION_REWORK_REQUIRED`/REWORK.

**Orden del Owner 2** (`F4-DRAFTS/coordinator/owner-f4t2b2b-nc1-decision.md`): «Sustituto + cerrar».
- nc1 ordinario = **ORÁCULO NO CUMPLIDO / SEÑAL DEMOSTRADA** (se conserva tal cual).
- **Sustituto F4T2B2B-NC1-IDENTITY-SIGNAL-01** (`F4-T2B2B-LIFECYCLE-nc1-substitute/1/R20261112T204312Z-f577`): **PASS**.
  - Autoprueba de 12 casos sintéticos, dos veces, determinista.
  - Se cumplen las 7 propiedades:
    - la mutación es exactamente `CurrentSha`, cambiado por un SHA que no existe en el repositorio;
    - los hashes derivados son coherentes;
    - Identity tenía que fallar;
    - el Controller dio Identity = fail con `EXECUTION_BLOCKED`/STOP;
    - su evidencia de Identity nombra el SHA falso y el HEAD real;
    - solo difieren de la verificación real las comprobaciones que consumen CurrentSha;
    - FailureClass es la primera en fail y es Handoff o Identity.
- **Registro:** nc1 = SATISFIED_UNDER_F4T2B2B_NC1_IDENTITY_SIGNAL_SUBSTITUTE.
- **Deuda de protocolo PD-F4-04:** README §10 (nc1 → Identity) y AUTOMATION_PLAN 16.9 (salida falsa → Handoff) admiten dos primeras comprobaciones para un CurrentSha inexistente. No se resuelve de forma global dentro de I-64.

**Resultado:** **F4-T2B2B-LIFECYCLE = CLOSED / PASS WITH LIMITATIONS** sobre `8f249e3a`, con `attempts` 1 de 3 (paquete de cierre `F4-T2B2B-LIFECYCLE/task-close-package.md`).

**Arrastres:**
- **A F4-T3** (cambios declarados de su contrato):
  - M-1: fijar solo el orden `_host = new WorkspaceHostCore(` → `_bridge.SetLifecycleAdvisor(` y su conteo, y que el reenvío llame a `_core.SetLifecycleAdvisor(`;
  - M-2 (1 a 4) y D-1: pasar a cota, orden o cuerpo propio;
  - M-3: afirmar solo la vía tomada;
  - M-4: documentar `Waiting` y `NotAdmitted` para la recarga;
  - M-5: `Detail` y `Witness` del NotAdmitted de la recarga, y la no degradación del descarte;
  - la obligación de llamar a `NotePilotEdit` (también para F4-T4);
  - los RackIds del veto para el mensaje de D-17;
  - el acceso a la cola de avisos;
  - el descarte de un solo borrador por el host;
  - la reentrada durante la lectura de la recarga.
- **A Smoke-2:**
  - el orden real entre cierre y destrucción en CLOSE, CLOSEALL, QUIT, la pestaña y la API, con la regla de devolución al Owner de D-T2B2-10 si la secuencia nunca queda intacta o hay falsos inciertos;
  - el cierre cancelado;
  - un `e.Veto()` que lanza o se ignora;
  - el documento vetado no activo;
  - `Stop()` y el fallo al enganchar `BeginDocumentClose`;
  - el coste;
  - la vía de retirada sin rastro.
