Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT3B-FULL-WRITE-RUN, intento 2: la CORRECCIÓN de F4B-E45 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- Está PROHIBIDO leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- **El código y los documentos:** SOLO por objetos git desde el checkout principal, que comparte el almacén. Usa `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** (handoff, TRX, logs y salidas de sus scripts) está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt3bcc2\rv\`:
  - `worker-handoff.json` y `relay-record-work.json` (el registro del trabajo, con lo que la sesión midió en sus Notes);
  - `tests/`, y `inv/` si existe;
  - `ci-core/` y `ci-ui/`: la CI del GREEN;
  - `red-ci-core/` y `red-ci-ui/`: la CI del RED;
  - `rr/`, `rr-ci-core/` y `rr-ci-ui/`, solo si hubo una reparación del RED: su diff y su CI.

**El objeto:**
- El diff es `git diff c7441653f3c1c8a27448ebec8f65f72a2f295f59 c758278548bcdd66e7c55ca15ca1faeb1a431e53`. El RED intermedio es `9fda277e5609e62ff715450eed03220c93df8585`, con tres partes:
  - el esqueleto inerte del API nuevo en PP (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs`) y UR (`…/Pilot/PushBackPilotUpdateRun.cs`), con el camino del piloto sin cambio;
  - la interfaz nueva `…/Pilot/IPushBackPilotViewEraser.cs`;
  - las cuatro clases nuevas de `tests/RackCad.Tests/` y el doble `PushBackFullWriteSupport.cs`, y el `Skip` literal de los seis métodos de D-1 (S-1 a S-6) en los tres archivos de guardas de F4 de `tests/RackCad.Tests/Workspace/`.
- El GREEN toca solo PP y UR.
- **La reparación del RED (RR, F4B-E35)**, si la hubo: un commit «RED repair» entre `9fda277e5609e62ff715450eed03220c93df8585` y el GREEN. La entrega lo declara en `Deviations` con una entrada que empieza por `RedRepairSha <sha>`, y `RedSha` sigue siendo `9fda277e5609e62ff715450eed03220c93df8585`. En PB-T3b no puede tocar `src/` ni los tres archivos de guardas con los `Skip`. La regla, RR-1 a RR-7, está en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`.
- WorkRunId `R20261120T075251Z-a9e0`, en `artifacts/orchestration/I-64/F4B-PBT3B-FULL-WRITE-RUN/2/`.
- BaseSha = ChainBaseSha = `c7441653f3c1c8a27448ebec8f65f72a2f295f59`, la reemisión del contrato con la v4.6 (F4B-E45): una corrección sin RED previo, porque el Worker del intento 1 (`R20261120T055235Z-a45b`) paró con PBT3B-C02 y PBT3B-C07 antes del RED, sin commits: con el esqueleto de la v4.5, `RackCad.Tests` no compilaba (dos CS0121 en `WorkspacePilotUpdateRunTests.cs:669` y `:799`, que pasan `null` a `Execute` y a `ComparePost`). Su `src/` y sus `tests/` son los de `eab27a9e` (la emisión de la v4.5), y su `src/`, el del cierre de PB-T3a (F4B-E43).
- **La corrección (C-1 de F4B-E45 y la v4.6, V46-06):** los métodos públicos nuevos del modelo completo llevan nombre propio, con la misma firma: `ExecuteFull` y `ComparePostFull` en `PushBackPilotUpdateRun` (los fija la sesión) y `PrepareFull` en `PushBackPilotUpdatePlanner` (el barrido ampliado encontró un tercer choque, por reflexión: `WorkspacePilotPrepareTests.cs:2155-2160` exige un solo `Prepare`). Ninguna firma, guarda ni prueba existente cambia. El Worker pudo partir del `blocked-skeleton.patch` del intento 1, con los tres nombres cambiados; no es autoridad.

**Qué es PB-T3b:** la generalización EN SU SITIO del plan y de la corrida de la actualización del Workspace de Push Back, con la variante (A) de D-1 (F4B-E29: un solo código), para que sirvan el modelo del piloto, conectado, y el modelo completo de PB-T3a, sin conectar.
- la barrera de diagnósticos con `PushBackEditorOutputGate` para los dos modelos (el único cambio visible del piloto, sobre un diseño bloqueado);
- el borrado de laterales obsoletos con la regla del último vínculo, y el origen de POST reanclado cuando se borra el origen;
- el rechazo del frontal B de un rack sin lado B y el renombrado con el nombre del borrador;
- `ComparePost` genérico (la sobrecarga del modelo completo es `ComparePostFull`) y la capacidad de borrar como interfaz nueva, con un solo `BeginMutation` y un solo `Seal`.

El Plugin, el anfitrión, el puerto del panel, la ventana, el clásico y Domain no cambian; no se crean vistas.

**Las autoridades** (en `c758278548bcdd66e7c55ca15ca1faeb1a431e53`):
- **El contrato** `docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json`:
  - sus invariantes INV-PBT3B-xx, sobre todo 01 a 06 y 15;
  - el AllowedWriteScope (11 rutas), las autoridades UNIT_CHANGE (PP y UR) y los RequiredTests;
  - las paradas, en especial C04, C07 (con la excepción de la reparación del RED), C13 (por nombre, con la lista de la variante (A)), C14, C16, C17 y C18.
- **El paquete** `docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md` v4.6 (C-1 de F4B-E45, los nombres; §4.1, el barrido ampliado con la resolución de sobrecargas):
  - el §3 (D-PBT3-09 y D-PBT3-10, con la variante (A); D-PBT3-12 a 14);
  - el §4 y el §4.1 (las guardas que siguen en verde);
  - el §5.0 (las reglas, el momento de cada lectura, los gestos que no cambian nada) y el §5.3 (CT-PBT3B-01 a 15, sus oráculos, por qué falla cada una en el RED, los equivalentes E-1 a E-6, el motivo literal del `Skip` y la medición sobre el GREEN);
  - el §6.0 (la regla RR y la tabla de RR-3) y el §6.2 (el esqueleto exacto del RED y la lista de PBT3B-C13);
  - el §8.2 (D-1).
- **El registro de sustitución** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`.
- **El análisis de la sesión sobre el intento 1** (el que el contrato cita; vinculante: C-1 a C-4).
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`: F4B-E45 (`owner-pbt3b-overload-decision.md`), F4B-E29 (`owner-pbt3-d1-decision.md`), F4B-E35, D2 (`owner-d1-d2-decisions.md`) y AUTH-13 (F4B-E24).

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - ¿El RED es SOLO el esqueleto exacto del §6.2 en PP y UR, con el camino del piloto sin cambio, la interfaz, las cinco pruebas nuevas y el `Skip`? ¿El GREEN toca solo PP y UR?
   - En los tres archivos de guardas de F4, ¿el `git diff -U0 c7441653f3c1c8a27448ebec8f65f72a2f295f59 c758278548bcdd66e7c55ca15ca1faeb1a431e53` son solo las seis líneas de atributo con el motivo literal, cada una en el método S-n que le toca?
   - ¿Toda declaración nueva de PP, UR e `IPushBackPilotViewEraser.cs` está en la lista de PBT3B-C13 de la variante (A)? ¿El censo de la entrega cuadra con `git show`? ¿Cada miembro público e interno de PP y UR en `c7441653f3c1c8a27448ebec8f65f72a2f295f59` conserva su firma (INV-PBT3B-03)?
2. **El diseño frente al código,** D-PBT3-09 y D-PBT3-10, punto a punto:
   - ¿`BuildCore` compone una sola vez, con el nombre de una sola expresión? ¿`Erasures` solo se llena si se admite borrar y solo con `ObsoleteLateral`?
   - ¿`PostOriginKey` se reancla cuando se borra el origen? ¿El frontal B de un rack de un sentido se rechaza?
   - ¿`Execution<TState>` tiene un solo `BeginMutation`, un solo `Seal`, todo `EraseUnit` entre ambos, y los ámbitos en `finally` propios?
   - ¿El piloto conserva su `Prepare`, sus llamadas al puerto, su `ComparePost` y su origen de POST, salvo la barrera (INV-PBT3B-04)?
   - C-1: ¿`PushBackPilotUpdatePlan.cs` declara `PrepareFull` y `PushBackPilotUpdateRun.cs` `ExecuteFull` y `ComparePostFull`, con la firma del paquete, sin ninguna `Prepare`, `Execute` ni `ComparePost` con el modelo completo, y con las firmas existentes idénticas? ¿Compila `WorkspacePilotUpdateRunTests.cs` sin tocarlo (sus `null` de `:669` y `:799`) y pasa `WorkspacePilotPrepareTests.cs:2155-2160` (un solo `Prepare`)?
3. **D2 y su límite (lo más importante).** ¿Alguna copia comparte un objeto mutable fuera de las dos referencias declaradas? ¿Alguna línea muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?
4. **Cada CT** (CT-PBT3B-01 a 15): ¿fija lo que dice el §5.3, con su oráculo? ¿Hay aserciones tautológicas?
   - ¿los equivalentes E-1 a E-6 cubren lo de S-1 a S-6 y empiezan afirmando la forma nueva?
   - ¿CT-06 (ii) y CT-10 se comprueban a sí mismas?
   - las lecciones del §5.0: ¿cada línea se lee en su momento (CT-07 y CT-12 tras una corrida `Applied`)? ¿Algún gesto escribe un valor igual al del diseño?
5. **El RED** (los TRX de `rv/red-ci-*`):
   - ¿fallan por aserción todos los métodos de los dos filtros con `ExpectRed`?
   - ¿pasa CT-13, y sus autopruebas discriminan?
   - ¿los seis métodos de D-1 están en `Skipped` y ninguna otra prueba existente falla o se salta?
   - **la medición de INV-PBT3B-15:** ¿es la esperada de la variante (A)?
   - **Si hubo reparación del RED:** ¿cumple RR-1 a RR-6, sin tocar `src/` ni los tres archivos de guardas? ¿Cambia algún literal de la **tabla de RR-3** de la clase reparada (la que el relevo midió en `rv/relay-record-work.json`, Notes, `TablaDeRR3`)? ¿Los indicios (`RR3AyudaMecanica` y `LiteralesCambiados`)?
6. **El GREEN y la estabilidad:**
   - la CI 4/4 con los recuentos previstos (Core + 21; los `Skipped` de Core en + 6 o + 10; UI sin cambio);
   - ¿hay dependencias de orden, de cultura o de estado estático?
7. **Contención:**
   - ¿alguna ventana mostrada, un modal real, un `MessageBox`, el configurador, «Insertar», el BOM o «Guardar en biblioteca» reales?
   - ¿alguna escritura en disco o en el perfil?
   - ¿corrió el Worker en local algo de UI fuera de los filtros del contrato, o con un filtro de método?
   - ¿se tocó el Plugin, Domain, el anfitrión, el puerto del panel o AutoCAD?
8. **Hallazgos para el Owner:** ¿alguna pérdida o diferencia observable del piloto que el paquete no declare?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt3bcc2\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- D2;
- los CT;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
