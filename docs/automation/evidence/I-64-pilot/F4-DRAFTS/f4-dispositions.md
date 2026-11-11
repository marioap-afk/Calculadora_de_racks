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
