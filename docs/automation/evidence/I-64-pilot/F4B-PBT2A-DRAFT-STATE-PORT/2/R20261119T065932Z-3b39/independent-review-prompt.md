Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT2A-DRAFT-STATE-PORT, **intento 2: la CORRECCIÓN** (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- Está PROHIBIDO leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- **El código y los documentos:** SOLO por objetos git desde el checkout principal, que comparte el almacén. Usa `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** (handoff, TRX, logs y salidas de sus scripts) está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt2acc2\rv\`:
  - `worker-handoff.json`;
  - `tests/`, y `inv/` si existe;
  - `ci-core/` y `ci-ui/`: la CI del GREEN;
  - `red-ci-core/` y `red-ci-ui/`: la CI del RED.

**El objeto:** la cadena entera de PB-T2a, `git diff 0dc623ca7a4bf1640654440276aa3d27147f48d4 5ddeb3b4ed99cb17d05142c67d47a26e89e238f9`, con:
- el RED del intento 1, `c33ac6ac994af983ca176910c07f009155f3449e`: el esqueleto inerte de los dos archivos nuevos de Application y las dos clases de prueba;
- el commit de preparación de la sesión, `17872be8`, la BaseSha de esta corrección, que custodia el análisis;
- el RED de la corrección, `fb90c823a8905d15ca95ae4d03d9a11fb1ebc159`: SOLO la clase corregida, `tests/RackCad.Tests/PushBackDraftStatePortTests.cs`;
- el GREEN `5ddeb3b4ed99cb17d05142c67d47a26e89e238f9`: solo los dos archivos nuevos de `src/`.

WorkRunId `R20261119T065932Z-3b39`, en `artifacts/orchestration/I-64/F4B-PBT2A-DRAFT-STATE-PORT/2/`. ChainBaseSha = `0dc623ca`, el commit de emisión del contrato. Su `src/` es el del cierre de PB-T1c (`161a0dad`).

**El análisis VINCULANTE del Coordinator:** `docs/automation/evidence/I-64-pilot/F4B-PBT2A-DRAFT-STATE-PORT/1/R20261119T060524Z-55f2/analysis.md` (F4B-E33).
- **Por qué hay corrección:** el intento 1 paró con PBT2A-C07. En CT-PBT2A-02, la línea `launchfor` leía `estado-es-el-actual` **después** de aplicar la edición por el puerto. Como `SetCurrentDraftState` deja en el borrador una copia nueva, con la implementación daba `False`. Sobre el esqueleto no se veía, porque la línea fallaba antes por la regla «EXCEPCION».
- **C-1:** dentro de la lectura de `LaunchFor(draft, p)`, `estado-es-el-actual` e `id` se toman ANTES de aplicar la edición, y `puerto-es-del-borrador`, después. La línea esperada no cambia, ni la regla «EXCEPCION».
- **C-2:** nada más cambia en `tests/`. CT-PBT2A-01, 03 y la guarda CT-PBT2A-04 quedan como en `c33ac6ac`.
- **La desviación declarada de CT-PBT2A-04:** la guarda inyecta cada autoprueba en el texto de los dos archivos y exige que fallen ambos. El paquete dice «en el texto de cada archivo nuevo». No se tocó: es más estricta que la letra. Júzgala tú.
- **C-3:** el GREEN es D-PBT2-02 y D-PBT2-03, solo en los dos archivos nuevos. El Worker pudo partir del parche del intento 1 (`green-wip-uncommitted.patch`), que no es una autoridad.
- **Las autoridades:** el contrato no declara rutas UNIT_CHANGE, así que F4B-E25 no hace falta.
- **La prueba de limpieza** puede cambiar SOLO `RackFrameConfigurator.layout`, que escriben pruebas de UI existentes al cerrar el configurador real. Eso no es parada (F4B-E30).

**Qué es PB-T2a:** crea en Application:
- el puerto por el que el editor completo de Push Back, en modo WORKSPACE, devuelve su estado editado al borrador (`IPushBackDraftStatePort`, `PushBackDraftApplyResult`);
- el adaptador `PushBackDraftSessionPort`, sobre `DraftSession<PushBackEditableState>`;
- el lanzamiento tipado de ese modo (`PushBackDraftEditorLaunch`).

El RED es el esqueleto inerte y las pruebas nuevas. El GREEN implementa solo los dos archivos nuevos de Application. La ventana, el piloto, el host y el Plugin no cambian.

**Las autoridades** (en `5ddeb3b4ed99cb17d05142c67d47a26e89e238f9`):
- **El contrato** `docs/automation/evidence/I-64-pilot/F4B-PBT2A-DRAFT-STATE-PORT/coordinator/gate-contract.json`:
  - sus invariantes INV-PBT2A-xx;
  - el AllowedWriteScope (4 rutas) y los RequiredTests;
  - las paradas, en especial C04, C13 (por nombre) y C17, si existe.
- **El paquete** `docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md` v2.2:
  - el §3 (D-PBT2-01 a 03 y D-PBT2-14, D2);
  - el §4;
  - el §5.0 y el §5.2 (el régimen, el esqueleto y CT-PBT2A-01 a 04);
  - el §6.0 y el §6.1;
  - el §8 (F4B-E28).
- **La comprobación acotada** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt2/independent-review-pbt2a-v21-check.md`.
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`. En particular `owner-pbt2-decisions.md` (F4B-E28) y D2 (`owner-d1-d2-decisions.md`).

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
0. **La corrección.**
   - ¿C-1 está aplicada exactamente, sin debilitar CT-PBT2A-02? El esqueleto tiene que seguir sin pasarla.
   - ¿`estado-es-el-actual` sigue comprobando de verdad que el lanzamiento lleva el estado actual del borrador, y `puerto-es-del-borrador`, que aplicar por su puerto cambia ese borrador?
   - ¿Se cumple C-2? Es decir, `git diff c33ac6ac fb90c823a8905d15ca95ae4d03d9a11fb1ebc159 -- tests` toca solo esa clase, y `git diff c33ac6ac fb90c823a8905d15ca95ae4d03d9a11fb1ebc159 -- src` está vacío.
   - La desviación declarada de CT-PBT2A-04: ¿es aceptable, o debilita algo?
1. **Alcance y forma.**
   - En la cadena, el RED (`c33ac6ac` más `fb90c823a8905d15ca95ae4d03d9a11fb1ebc159`) es SOLO el esqueleto exacto del paquete más las pruebas. El GREEN, solo los dos archivos de `src/`.
   - Ninguna declaración nueva fuera de lo que prescribe el paquete (C13).
   - Se cumplen los invariantes del contrato, medidos desde ChainBaseSha `0dc623ca`: la BaseSha `17872be8` ya contiene el RED del intento 1.
2. **El diseño frente al código,** D-PBT2-01 a 03, punto a punto:
   - el puerto y su resultado;
   - el adaptador sobre `DraftSession`;
   - el lanzamiento tipado.

   ¿Algún comportamiento cambia sin estar declarado?
   ¿Respetan las decisiones de F4B-E28? En PB-T2a son, sobre todo:
   - el rechazo de un diseño bloqueante al aplicar (pregunta 2);
   - que «Aplicar» no llame a `SetName` ni a `SyncName` (V2 I-07).
3. **D2 y su límite (lo más importante).**
   - ¿Alguna línea muta el rack resuelto o el informe de módulos de un lado?
   - ¿El puerto devuelve al borrador una copia sin alias?
   - ¿La guarda G-L1 de PB-T2a aplica las reglas y las autopruebas EXACTAS (las de PB-T1b y PB-T1c, con k1 y k2), o se deja algo?
4. **Cada CT** (CT-PBT2A-01 a 04): ¿fija lo que dice el §5.2? ¿Hay aserciones tautológicas?
5. **El RED de la corrección** (los TRX de `rv/red-ci-*`, sobre `fb90c823a8905d15ca95ae4d03d9a11fb1ebc159`):
   - ¿fallan por aserción todos los métodos de los filtros con ExpectRed?
   - ¿pasa la guarda?
   - ¿ninguna prueba existente falla?
6. **El GREEN y la estabilidad:**
   - la CI 4/4 con los recuentos previstos;
   - ¿hay dependencias de orden, de cultura o de estado estático?
7. **Contención:**
   - ¿alguna escritura fuera de una ubicación temporal o en el perfil? La disposición del configurador puede cambiar por pruebas existentes: es un defecto conocido y anterior, anotado en `docs/ideas-futuras.md`, y no cuenta;
   - ¿se tocó el Plugin o AutoCAD?
8. **Hallazgos para el Owner:** ¿alguna diferencia observable sin declarar?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt2acc2\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- D2;
- los CT;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
