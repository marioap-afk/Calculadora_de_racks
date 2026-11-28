Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT0-CHARACTERIZATION, intento 1 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

Worktree (solo lectura): `<worktree-de-I-64>`, rama `architecture/workspace-persistente-rackcad`. Usa siempre `git -C <worktree>`, sin checkout.

**El objeto es todo el cambio de PB-T0:** `git diff f2eedd9a0d03c13edc0a92a0949d2c0b64bd78f4 28e38d8d51108c22305a5fc41ce84070a2efdbe7`.
- BaseSha = ChainBaseSha `f2eedd9a`: el commit de emisión del contrato.
- RED `8f75a89b092947e4572e7383cc37696776903de1`: las pruebas nuevas más los 20 mutantes deliberados de la tabla cerrada, todos juntos.
- GREEN `28e38d8d51108c22305a5fc41ce84070a2efdbe7`: revierte los mutantes; el producto debe quedar idéntico byte a byte a la BaseSha.
- PB-T0 es SOLO caracterización: «No product cleanup in PB-T0». Fija con pruebas el comportamiento ACTUAL de la ventana clásica de Push Back y de su ensamblador; no lo corrige.

**Autoridades** (léelas en el GREEN):
- el contrato `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json`: Invariants (INV-PBT0-01 a 24; la tabla CERRADA de mutantes es INV-PBT0-14), AllowedWriteScope (16 rutas exactas), ForbiddenWriteScope (265), RequiredTests y StopConditions (en especial PBT0-C04, C07, C09 y C10);
- el paquete `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md`: §2 hechos medidos, §4 los CT-PB0-01 a 21 (lista cerrada, con sus casos, lo que fija cada uno y lo que NO duplica), §4.5 las tablas T-1 a T-8, §5 la tabla de mutantes y sus reglas;
- el paquete de la fase `docs/initiatives/I-64-f4b-push-back-completo-package.md` (§6.2 bloque C, H-PB-24 a H-PB-33);
- el prompt del Worker `artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T185616Z-3c1b/prompt.md` (paso 3: qué hacer si un mutante no discrimina);
- la entrega `artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T185616Z-3c1b/worker-handoff.json`, con los parches en `mutants/` y los TRX en `tests/`, `red-ci-core/`, `red-ci-ui/`, `ci-core/` y `ci-ui/`.

**QUÉ REVISAR**, con evidencia `ruta:línea` en el GREEN o en el RED:

1. **Alcance.** Compara tú las listas, sin fiarte de ningún resumen:
   - `git diff --name-only` BaseSha..CurrentSha frente al AllowedWriteScope exacto; ninguna ruta bajo el ForbiddenWriteScope;
   - `git diff BaseSha CurrentSha -- src` vacío; el RED toca en `src/` solo archivos de MutantScope; el GREEN (RED..CurrentSha) no toca `tests/`;
   - ningún fixture nuevo en `tests/RackCad.Tests/Fixtures/I64` (H-PB-32) ni cambio en guardas, soporte o pruebas existentes.

2. **Los 21 CT frente al §4 del paquete.** Para cada CT-PB0-01 a 21: ¿la clase y el método existen donde dice el paquete (3 en Core y 18 en UI)? ¿Fija lo que el paquete dice que fija, con valores MEDIDOS en la base y no copiados de las lecturas? ¿Duplica algo que el paquete dice que no duplica? ¿Alguna aserción es tautológica, compara un valor consigo mismo o pasaría con cualquier producto? Marca los CT débiles.

3. **La tabla CERRADA de mutantes (INV-PBT0-14, §5).** Para cada `mutants/muNN.patch`: ¿aplica exactamente el cambio de su fila (archivo:línea y texto), o la alternativa enumerada en su fila y solo esa? ¿Respeta las reglas del §5 (ni asignaciones de Owner, ni ShowDialog, ni MessageBox, ni EditorDiscardPrompt, ni comprobaciones de nulo de costuras, ni guardas de RequestDraw, ni el constructor)? ¿`red-all.patch` y el RED `8f75a89b` contienen los 20 y nada más? ¿Algún mutante anula a otro en la conjunción?

4. **La desviación de CT-17 (la pregunta central).** El Worker añadió el caso c-16 (`Dimensions = 3`) a la lista c-01..c-15 del paquete porque con c-10 y c-11 (7 y −1) MU-PB0-16 no discriminaba: según él, la carga normaliza 7 y −1 a 0 antes de la ventana (`DynamicRackSystemDocument.cs:322,429`) y la alternativa W:387 tampoco discrimina. Aplicó la cláusula del paso 3 del prompt («corrige la prueba solo si no caracteriza bien el comportamiento actual que el CT describe; si aun así no hay mutante válido, parada PBT0-C10») en vez de parar.
   - Verifica en el código que 7 y −1 no llegan a W:407 y que la alternativa W:387 no discrimina.
   - ¿El CT-17 original (c-01..c-15) caracterizaba bien el recorte de W:407 que el §2 del paquete atribuye a la ventana? ¿Es c-16 una corrección de la prueba admitida por el paso 3, o un cambio del contrato que debía ser STOP (PBT0-C10 o PBT0-C07: «los hechos del §2 no se sostienen en la base de un modo que cambia un CT»)? Razónalo con el texto exacto del contrato, del paquete y del prompt.
   - Lo mismo para la segunda desviación: CT-17 captura con try/catch la excepción de Abrir en c-12 y c-13 y fija su tipo y mensaje como CARGA-APPLICATION. ¿Era lo que el paquete preveía para esos casos? ¿Cambia un CT por un hecho del §2 que no se sostiene?

5. **El RED.** En la CI del RED (TRX en `red-ci-core/` y `red-ci-ui/`): ¿fallan los 21 métodos nuevos, y por aserción y no por excepción (regla I-01)? Las existentes rotas (19 en Core y 105 en UI según la entrega): ¿la atribución T-8 a mutantes concretos es plausible, o alguna rotura no tiene mutante que la explique?

6. **El GREEN y la estabilidad.** CI 4/4 (TRX en `ci-core/` y `ci-ui/`). ¿Hay dependencias de orden, de reloj, de cultura, de rutas de la máquina, de foco o de ventanas que hagan frágiles las pruebas de UI? ¿Alguna prueba deja una ventana, un modal o un dispatcher vivo (PBT0-C09)?

7. **Contención (INV-PBT0-06 y PBT0-C09).** Ninguna prueba escribe fuera de una ubicación temporal aislada, ni en los datos reales del usuario (la disposición del configurador en el perfil local, la carpeta de biblioteca), ni carga el Plugin ni AutoCAD. ¿Hay algún camino de la ventana que las pruebas disparen y que escriba (Guardar en biblioteca, BOM, el cierre del configurador)?

8. **Hallazgos de producto que la entrega declara y no corrige** (UnexpectedFindings): `LoadExisting` lanza una ArgumentException no capturada con `PalletTolerance = 0`; `pb-21-cola` pierde 667 hojas (la cola GAP y B:*, el objeto SideB) en la salida de la ventana frente al sobre; H-PB-28 confirmado. Para cada uno: ¿está bien fijado? ¿Indica un posible defecto de producto ACTUAL en RACKEDITAR (pérdida de datos al Actualizar, cierre inesperado) que el Owner deba conocer antes de PB-TC o PB-T1? No propongas corregirlo en PB-T0.

**REGLAS:** solo lectura, con git y lectura de archivos. No ejecutes dotnet ni pruebas. No escribas en el worktree ni en ningún otro repositorio. Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt0\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M (bloqueantes, importantes, menores);
- la tabla de hallazgos, con id, gravedad, contenido, evidencia y corrección;
- la comparación de las listas de alcance;
- la tabla de los 20 mutantes (fila, cambio aplicado frente a la tabla, discrimina en aislado sí/no, por aserción sí/no);
- el dictamen razonado sobre las dos desviaciones de CT-17;
- la tabla de los 21 CT con su valoración (bien / débil / no fija lo prometido);
- los hallazgos OWNER (una decisión material nueva o un posible defecto de producto actual que deba conocer);
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M y los ids de los bloqueantes, importantes u OWNER.
