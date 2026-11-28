Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT1C-LOSSLESS-STATE-WINDOW, **intento 2: la CORRECCIÓN** (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- PROHIBIDO leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- **El código y los documentos:** SOLO por objetos git desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** (handoff, TRX, logs y la salida de su script de invariantes) está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt1cc2\rv\`:
  - `worker-handoff.json`;
  - `tests/` e `inv/`, si existe;
  - `ci-core/` y `ci-ui/`: la CI del GREEN;
  - `red-ci-core/` y `red-ci-ui/`: la CI del RED.

**El objeto:** la cadena entera de PB-T1c, `git diff 833ea6bfaadfab9afc698aba7f12ad2124a78585 161a0dadccaf5b71886344d834146737b90bb4f3`, con:
- el RED del intento 1, `e5a2198945b0a5e3db18c46b91c64dd83e88d69c`: la costura inerte en W y las tres clases;
- el commit de preparación de la sesión, `d8ccbcb8`, la BaseSha de esta corrección, que custodia el análisis;
- el RED de la corrección, `2faae9da8e559278ebefba12325ccc0103f17208`: SOLO la clase de UI corregida;
- el GREEN `161a0dadccaf5b71886344d834146737b90bb4f3`: solo W y WC.

WorkRunId `R20261119T041505Z-11b9`, en `artifacts/orchestration/I-64/F4B-PBT1C-LOSSLESS-STATE-WINDOW/2/`.

**El análisis VINCULANTE del Coordinator:** `docs/automation/evidence/I-64-pilot/F4B-PBT1C-LOSSLESS-STATE-WINDOW/1/R20261119T025420Z-ee6c/analysis.md`.
- **Por qué hay corrección:** el intento 1 paró con PBT1C-C07. CT-PBT1C-02 comparaba su parte 1 antes del recálculo neutro, y en tres gestos de la parte compuesta el diseño va una pasada por detrás. Ese desfase ya existía.
- **C-1:** la parte 1 de CT-02 se toma tras el recálculo neutro, sin cambiar ninguna línea esperada.
- **C-2:** nada más cambia en `tests/`.
- **C-3:** el GREEN es D-PBT1-13.
- **La prueba de limpieza** puede cambiar SOLO `RackFrameConfigurator.layout`, que escriben pruebas existentes al abrir el configurador real; eso no es parada.

**Qué es PB-T1c:** hacer que la ventana clásica de Push Back guarde su estado editable en el `PushBackEditableState` de PB-T1b, en modo CLASSIC y sin cambio observable, con el cambio mínimo de D-PBT1-13.
- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`;
- **WC** = `….Composite.cs`.

**Las autoridades** (en `161a0dadccaf5b71886344d834146737b90bb4f3`):
- **el contrato** `docs/automation/evidence/I-64-pilot/F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json`, con:
  - sus invariantes INV-PBT1C-01 a 07c: INV-03 con rangos de línea, INV-05 con el censo y las líneas testigo;
  - el AllowedWriteScope (5) y los RequiredTests;
  - las paradas, en especial C04, C13 (por nombre), C14 y C17;
- **el paquete** `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.8: §3 (D-PBT1-13 a 15, D2 y G-L1), §4, §5.0, §5.4 y §6.3;
- **las decisiones del Owner** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`, en particular D2 (`owner-d1-d2-decisions.md`).

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:

0. **La corrección.**
   - ¿C-1 está aplicada exactamente, sin debilitar CT-02? La costura inerte tiene que seguir sin pasarla.
   - ¿Se cumple C-2? Es decir, `git diff e5a21989 2faae9da8e559278ebefba12325ccc0103f17208 -- tests` toca solo esa clase.
   - Comprueba en la base que el diseño de `LastComputation` del compuesto iba una pasada por detrás. ¿Es cierto? ¿Es inocuo para «Actualizar», el BOM y la biblioteca?
1. **Alcance y forma.**
   - El RED es SOLO la costura inerte (una línea en W) más las tres clases. El GREEN, solo W y WC.
   - Cada hunk de `git diff -U0 833ea6bf 161a0dadccaf5b71886344d834146737b90bb4f3 -- src` cae en su rango de INV-PBT1C-03. Lo retirado solo quita líneas, y los cuatro métodos solo ganan `SyncName();`.
   - Ninguna declaración con nombre nuevo, salvo `editable`, `SyncName` y `EditableStateForTest`.
   - Se cumplen INV-PBT1C-04 a 06: los literales de T-SG, las cuentas, el censo, las líneas testigo y las 40 costuras.
2. **El diseño frente al código,** D-PBT1-13 (1) a (9), punto a punto:
   - los reenvíos con el mismo nombre;
   - las cargas con el orden de hoy;
   - `AdoptCarriers` como primera sentencia del `try` de `LoadFromModel`, con las siembras sin tocar;
   - `ReadInputs`, que escribe en el estado con las expresiones de hoy;
   - lo retirado;
   - el criterio de bloqueo;
   - `SyncName` tras cada `SetName` y `Adopt`.

   ¿Algún comportamiento observable cambia, aunque no lo pille ninguna prueba? Piensa en el orden de las siembras, el foco, `suppressSync`, Restaurar, el +frente, A/B, la cola y c-02/c-09.
3. **D2 y su límite (lo más importante).**
   - ¿Alguna línea añadida muta el rack resuelto, el informe de módulos de un lado, `lastComputation.System`, `session.System` o `lastCompositeSystem`?
   - ¿CT-PBT1C-02 (parte 2) y CT-PBT1C-04 (G-L1 con dieciséis autopruebas, k1 y k2 incluidas) aplican EXACTAMENTE las reglas de D-PBT1-15, o les falta algo?
4. **Cada CT** (CT-PBT1C-01 a 04).
   - ¿Fija lo que dice el §5.4, o tiene aserciones tautológicas?
   - **CT-01:** compara de verdad la ventana con el estado solo, sobre el corpus.
   - **CT-02:** el censo de controles completo, con las huellas.
   - **CT-03:** la guarda de texto.
5. **El RED** (los TRX de `rv/red-ci-*`).
   - ¿Fallan por aserción todos los métodos de los filtros con ExpectRed?
   - ¿Pasa CT-04?
   - ¿El filtro UI PushBack falla SOLO por los dos métodos nuevos?
   - ¿Ninguna prueba existente falla?
6. **El GREEN y la estabilidad.**
   - CI 4/4 con Core y UI = la línea base del cierre de PB-T1b (Core 19 086; UI 2 437, con 2 416 y 21) más 2 en cada una.
   - Las huellas de CT-PBT1A-14 tres veces en verde.
   - ¿Hay dependencias de orden, cultura o estado estático? ¿Quedan ventanas o modales vivos?
7. **Contención:** ¿escrituras fuera de una ubicación temporal o en el perfil? ¿Toca el Plugin o AutoCAD?
8. **Hallazgos para el Owner:** ¿alguna diferencia observable sin declarar?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt1cc2\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- D2;
- los CT;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
