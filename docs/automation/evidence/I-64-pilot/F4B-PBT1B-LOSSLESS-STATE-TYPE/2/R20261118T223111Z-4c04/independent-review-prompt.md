Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT1B-LOSSLESS-STATE-TYPE, **intento 2: la CORRECCIÓN de las pruebas** (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** mientras revisas, otro proceso trabaja en el worktree de la rama. Está PROHIBIDO leer, listar o ejecutar nada en `<worktree-de-I-64>` y escribir esa ruta en una línea de órdenes.
- **El código y los documentos,** SOLO por objetos git desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco (están en otra revisión).
- **La entrega del Worker** (handoff, TRX y logs) está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt1b2\rv-a2\`:
  - `worker-handoff.json`;
  - `tests/`, con las corridas locales del Worker;
  - `ci-core/` y `ci-ui/`, con la CI del GREEN;
  - `red-ci-core/` y `red-ci-ui/`, con la CI del RED.

**El objeto:**
- **BaseSha** `2f5d3decb237913e3980aa0bbd84346a7ff91f19`. Contiene la implementación del intento 1 (GREEN `a0a5f699`) y el análisis del Coordinator.
- **RED de la corrección** `126fd65ff138afe839e67788061960c0e911cf1c`: las pruebas corregidas y, en los seis archivos de `src/`, el esqueleto exacto del RED del intento 1 (`2d9febc6`) devuelto.
- **GREEN de la corrección** `a18ba345eddc5333c22b8bacbad5c2ed4dbf5959`: restaura con `git restore` la implementación de `a0a5f699`, sin cambios de producto.
- `git diff 2f5d3dec a18ba345eddc5333c22b8bacbad5c2ed4dbf5959` debe tener SOLO pruebas.
- WorkRunId `R20261118T223111Z-4c04`; delegación `R20261118T222843Z-e354`.

**Qué es PB-T1b:** crea en Application el estado editable de Push Back sin pérdidas (`PushBackEditableState`) con:
- las cargas y `BuildInputs`;
- el copiador, el comparador y el mapa de propiedad;
- `PushBackEditorOutputGate`;
- c-02 y c-09;
- la excepción D2 del Owner: solo la línea base resuelta y el informe de módulos de cada lado se comparten por referencia, con diez condiciones y un límite.

El intento 1 dejó el producto bien y la revisión independiente encontró defectos de PRUEBAS. El Owner decidió corregirlos en este intento (F4B-E22).

**Las autoridades** (en `a18ba345eddc5333c22b8bacbad5c2ed4dbf5959`):
- **El análisis VINCULANTE del Coordinator:** `docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204715Z-a4e2/analysis.md`, con SHA-256 `9891487e4cac8540f3c377ac848e55380ef7dffb76cfec303c4947ead7f8f0bf`.
  - Corrige: I01, I02, M01, M02, M04, M06, M07, M08 y M13.
  - No corrige: M03, M05, M09, M10, M11, M12, M14 y M15.
- **La revisión independiente del intento 1:** `independent-review.md`, junto al análisis.
- **El contrato:** `docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/coordinator/gate-contract.json`.
- **El paquete v2.7:** `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`; en particular el §3 (D-PBT1-02 a 11, 09b y 15), el §5.0, el §5.3 y el §6.0 (la corrección, M-11).
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`, en particular `owner-pbt1b-rework-decisions.md` (F4B-E22). M10 y M14 quedan ratificados solo para PB-T1b: no son defectos de esta entrega.

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Las nueve correcciones, una a una:** para cada una, qué pedía el análisis y qué hay en el diff. ¿Está completa? ¿La prueba nueva o cambiada puede fallar de verdad, sin tautología? En particular:
   - **I01:** G-L1 aplica la regla 1b a los alias, y su autoprueba nueva falla sobre el texto inyectado.
   - **I02:** el caso de CT-10 ya puede fallar.
2. **Nada más que eso:** ¿hay en `tests/` algún cambio que el análisis no pida? ¿Algún punto declarado como no corregido se tocó o se debilitó? ¿Alguna línea esperada se ajustó a posteriori en vez de medirse? Coteja las líneas nuevas con la medición p2 de la entrega, hecha antes del RED.
3. **El régimen de la corrección:**
   - En `126fd65ff138afe839e67788061960c0e911cf1c`, los seis de `src/` tienen exactamente los blobs de `2d9febc6`: compara `git rev-parse`.
   - En `a18ba345eddc5333c22b8bacbad5c2ed4dbf5959`, tienen exactamente los de `a0a5f699`, y `git diff 2f5d3dec a18ba345eddc5333c22b8bacbad5c2ed4dbf5959 -- src` está vacío.
   - El GREEN no toca `tests/`.
4. **El RED, en los TRX de `rv-a2/red-ci-*`:**
   - ¿Falla por aserción todo método de los filtros con ExpectRed? ¿Hay alguno que pase sobre el esqueleto (I-06)?
   - Las guardas de D2 (PushBackSharedReferenceGuardTests) pasan.
   - Ninguna prueba existente falla.
5. **El GREEN,** en los TRX de `rv-a2/ci-*` y `rv-a2/tests/`:
   - la CI 4/4;
   - los recuentos frente al intento 1 (Core 19 086 y UI 2 437) más lo que añaden las correcciones;
   - las tres corridas g1, g2 y g3.

   ¿Hay dependencias de orden, de reloj, de cultura, de rutas o de estado estático?
6. **La interrupción de transporte:** el Worker se cortó a mitad de trabajo (la API se colgó 75 min) y se reanudó en la misma invocación. ¿La entrega lo declara? ¿Quedó algo a medias o duplicado en las pruebas, o una corrida repetida mal citada?
7. **Contención:** ¿se escribió fuera de una ubicación temporal o en el perfil? ¿Hay Plugin o AutoCAD?
8. **Hallazgos para el Owner:** ¿alguna diferencia o pérdida nueva sin declarar?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt1b2\independent-review-code.md`, **sin rutas locales absolutas**. Contenido:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de las nueve correcciones;
- el régimen RED/GREEN;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
