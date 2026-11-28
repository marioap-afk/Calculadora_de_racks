Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT4-FULL-READ-PATH (PB-T4a), intento 1 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- Está PROHIBIDO leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- **El código y los documentos:** SOLO por objetos git desde el checkout principal, que comparte el almacén. Usa `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** (handoff, TRX, logs y salidas de sus scripts) está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4acc\rv\`:
  - `worker-handoff.json` y `relay-record-work.json` (el registro del trabajo, con lo que la sesión midió en sus Notes);
  - `tests/`, y `inv/` si existe;
  - `ci-core/` y `ci-ui/`: la CI del GREEN;
  - `red-ci-core/` y `red-ci-ui/`: la CI del RED;
  - `rr/`, `rr-ci-core/` y `rr-ci-ui/`, solo si hubo una reparación del RED: su diff y su CI.

**Las abreviaturas** (las del paquete):
- WEI: `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`;
- WHC: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- RIA: `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`;
- NH: `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`;
- DWG: `tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs`.

**El objeto:**
- El diff es `git diff 5aaacce3b278c1f29375de4f24c7fab232f44ad2 faef702055990d1350a73bf4d254eb9e898d5670`. El RED intermedio es `214f8c75f2ca5a248a5592075c3bac8e667bd362`, con tres partes:
  - **el esqueleto,** aplicado del parche custodiado que fija la emisión. Su ruta y su sha256 están en el registro del trabajo (`rv/relay-record-work.json`, Notes), y el relevo comprobó que el RED es, archivo a archivo, ese parche aplicado a la BaseSha:
    - en WEI, `FullRead`, `Catalog` y `FromReads`, con los dos campos en null;
    - la interfaz nueva `src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs`;
    - `src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs`, con `Choose` que devuelve null;
  - **las pruebas nuevas de Core:** `PushBackFullReadPathTests` (CT-PBT4A-01 a 04 y 07) y `PushBackFullReadGuardTests` (05 y 06), un `[Fact]` por CT, y el kit nuevo `PushBackFullReadKit`. Si la v4.1 del paquete añade otra CT, también es una prueba nueva; el contrato las lista;
  - **el `Skip` de CT-PBT3A-12** en DWG: una sola línea, la única sustitución, con registro.
- RIA, NH y WHC no cambian en el RED. El GREEN toca solo WEI, WHC, RIA, NH y `PushBackOriginByText.cs`, y nunca `tests/`.
- **La reparación del RED (RR, F4B-E35)**, si la hubo: un commit «RED repair» entre `214f8c75f2ca5a248a5592075c3bac8e667bd362` y el GREEN. La entrega lo declara en `Deviations` con una entrada que empieza por `RedRepairSha <sha>`, y `RedSha` sigue siendo `214f8c75f2ca5a248a5592075c3bac8e667bd362`.
  - En PB-T4a solo puede tocar las pruebas nuevas: ni `src/` ni DWG.
  - La regla, RR-1 a RR-7, está en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`.
- WorkRunId `R20261120T193653Z-de54`, en `artifacts/orchestration/I-64/F4B-PBT4-FULL-READ-PATH/1/`.
- BaseSha = ChainBaseSha = `5aaacce3b278c1f29375de4f24c7fab232f44ad2`, el commit de emisión del contrato (primera delegación). Su `src/` y sus `tests/` son los del cierre de PB-T3c (F4B-E48, `7f0d9aa3`; GREEN `938a35f0`).

**Qué es PB-T4a** (D-PBT4-02 y D-PBT4-03): la subtarea aditiva de PB-T4.
- **La lectura completa en el mismo escaneo:** `RackPilotIntentAdapter.Read` llama una vez a `PushBackFullDraftReader.Read(input, catalog)` sobre el **mismo** `input`, justo después del lector del piloto.
- **El resultado:** `EditIntentReadResult` lleva `FullRead` y `Catalog` (`FromReads`); `FromPilot` no cambia. NH devuelve `FromReads(read.Pilot, read.Full, catalog.Catalog)`.
- **Las relecturas con el origen:** las cinco relecturas de un borrador abierto van al origen del borrador por su texto. Cuando el lector implementa `IWorkspaceEditIntentOriginReader`, piden `ReadIntentAtOrigin(…, draft.CurrentDraftState.Origin, …)`.
  - El origen es el único miembro mutable con ese texto (`PushBackOriginByText.Choose`), o el `Primary` de hoy si no hay uno.
  - La elección va en línea en cada cuerpo, con el bloque literal de D-PBT4-03. `Begin` sigue con `ReadIntent` y `ChosenReference`.
- **El cambio observable declarado (M-23):** las relecturas dejan de ir al `Primary`.
- **Lo que no hace:** nada traduce estados; el dibujo se lee una vez por lectura; el anfitrión no usa todavía `FullRead`; el tipo del borrador del panel no cambia. No se ejecuta AutoCAD.

**Las autoridades** (en `faef702055990d1350a73bf4d254eb9e898d5670`):
- **El contrato** `docs/automation/evidence/I-64-pilot/F4B-PBT4-FULL-READ-PATH/coordinator/gate-contract.json`:
  - sus invariantes;
  - el AllowedWriteScope, las autoridades UNIT_CHANGE (WEI, WHC, RIA y NH) y los RequiredTests;
  - las paradas, en especial C04, C07 (con la excepción de la reparación del RED), C13 (por nombre) y C18, y la de D2 («STOP y se reabre L-1»);
  - los totales exactos previstos de la CI (F4B-E46), de una sola rama.
- **El paquete** de PB-T4 (`docs/initiatives/`, la versión que cita el contrato):
  - D-PBT4-01 a 03;
  - el §4.1 (las guardas que siguen en verde y la única sustitución);
  - el §5.0 y el §5.1 (CT-PBT4A-01 a 07 y los totales);
  - el §6.0 y el §6.1 (el alcance, el esqueleto, la lista de C13, los invariantes y RR).
- **El registro de la sustitución de CT-PBT3A-12** (en `superseding-characterization.md`, o donde lo cite el contrato).
- **Las decisiones del Owner:**
  - las de PB-T4 (`owner-pbt4-decisions.md`);
  - F4B-E46 (`owner-pbt3b-nc3-decision.md`, los totales de cada rama);
  - F4B-E45 (la lección de las sobrecargas);
  - F4B-E35;
  - D2 (`owner-d1-d2-decisions.md`).

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - ¿El RED es SOLO el esqueleto del parche, archivo a archivo, más las pruebas nuevas y el `Skip`, con RIA, NH y WHC sin cambio? ¿El GREEN toca solo el código previsto?
   - ¿Toda declaración nueva está en la lista de C13? ¿WHC no gana ninguna declaración? ¿El censo de la entrega cuadra con `git show`?
   - ¿No cambian la UI, Domain, `RackPushBackCommands.cs` ni `RackCommandSupport.cs`?
2. **El diseño frente al código,** punto a punto:
   - RIA: ¿una sola `CaptureDrawing`, un `LockDocument` y un `StartTransaction`, como en la base? ¿`PushBackFullDraftReader.Read(` una vez, sobre el mismo `input` y tras el lector del piloto? ¿`PushBackOriginByText.Choose(` una vez, sobre `PushBackPilotIntentReader.BuildStaleBase` del escaneo, como UR:645-649? ¿Cae al `originDefinitionHandle` del `Primary` sin coincidencia o con varias?
   - NH: ¿el lector anidado implementa la interfaz nueva por el mismo `ReadOnActiveDocument`, con dos `LockDocument`, una `LoadCatalog()` y una `RackPilotIntentAdapter.Read(`? ¿Devuelve `FromReads`?
   - WHC: ¿solo cambian las cinco sentencias de lectura, con el bloque literal de D-PBT4-03, y `Begin` no? ¿Siguen las nueve `Find<PushBackPilotDraftState>`, los seis `.ReadIntent(` y el orden de cada cuerpo?
   - WEI: ¿`FromReads` lanza `ArgumentNullException` con un nulo? ¿`FromPilot` y los demás estados dejan `FullRead` y `Catalog` en null?
   - `PushBackOriginByText.Choose`: ¿solo los miembros **mutables**, byte a byte, y null con cero o con varios? ¿Es pura?
3. **D2 y su límite (lo más importante).** ¿Alguna copia comparte un objeto mutable fuera de las referencias declaradas? ¿Alguna línea muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado? ¿Se traduce un estado del piloto al completo en algún sitio?
4. **Cada CT** (CT-PBT4A-01 a 07, y la que añada la v4.1):
   - ¿fija lo que dice el §5.1?
   - ¿la autoprueba en negativo de CT-07 discrimina de verdad, o es tautológica?
   - ¿un `[Fact]` por CT, sin `Theory`?
   - ¿los kits nuevos van en archivos nuevos, sin editar `EditIntentKit` ni los demás?
   - CT-06: ¿admite RIA y nada más, como sustituto de CT-PBT3A-12?
5. **El RED** (los TRX de `rv/red-ci-*`):
   - ¿fallan por aserción las CT con `ExpectRed`, y pasan las demás?
   - ¿los seis métodos de D-1 y CT-PBT3A-12 siguen en `Skipped`, y ninguna otra prueba existente falla o se salta?
   - **los totales exactos (F4B-E46):** ¿la CI del RED da el total y cada parte previstos del contrato, sin otra rama?
   - **Si hubo reparación del RED:** ¿cumple RR-1 a RR-6, sin tocar `src/` ni DWG? ¿Cambia algún literal de la **tabla de RR-3** de la clase reparada (la que el relevo midió en `rv/relay-record-work.json`, Notes)?
6. **El GREEN y la estabilidad:**
   - la CI 4/4, con el Plugin compilado y los totales exactos previstos del GREEN;
   - ¿hay dependencias de orden, de cultura o de estado estático?
7. **Contención:**
   - ¿alguna ventana, un modal real, un `MessageBox`, el configurador, «Insertar», el BOM o «Guardar en biblioteca» reales?
   - ¿alguna escritura en disco o en el perfil?
   - ¿corrió el Worker en local algo de UI fuera de los filtros del contrato, o con un filtro de método?
   - ¿se tocó la UI, Domain o AutoCAD?
8. **Hallazgos para el Owner:**
   - ¿algún riesgo de la relectura en el origen que el Smoke-PB tenga que mirar y el paquete no declare? Por ejemplo, el origen editado fuera o borrado por un POST `Applied`;
   - ¿algún coste de la doble lectura (R-T4-16)?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4acc\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- D2;
- los CT;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
