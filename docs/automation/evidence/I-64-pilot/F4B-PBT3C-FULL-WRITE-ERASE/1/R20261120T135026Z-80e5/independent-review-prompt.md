Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT3C-FULL-WRITE-ERASE, intento 1 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- Está PROHIBIDO leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- **El código y los documentos:** SOLO por objetos git desde el checkout principal, que comparte el almacén. Usa `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** (handoff, TRX, logs y salidas de sus scripts) está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt3ccc\rv\`:
  - `worker-handoff.json` y `relay-record-work.json` (el registro del trabajo, con lo que la sesión midió en sus Notes);
  - `tests/`, y `inv/` si existe;
  - `ci-core/` y `ci-ui/`: la CI del GREEN;
  - `red-ci-core/` y `red-ci-ui/`: la CI del RED;
  - `rr/`, `rr-ci-core/` y `rr-ci-ui/`, solo si hubo una reparación del RED: su diff y su CI.

**El objeto:**
- El diff es `git diff 7dfc7c21a0b30f5e04d691768b1717778e5c4ad7 938a35f0f1636b4306d844eb9e34000e8967c05e`. El RED intermedio es `e0940a0efa314ea26777c59ab4e55c09d019bff6`, con dos partes:
  - **el esqueleto inerte de la fachada** `src/RackCad.Plugin/Views/RackViewBlockEraser.cs`, aplicado del parche custodiado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt3/pbt3c-skeleton.patch` (sha256 `a81de435…`): la clase, el método de D-PBT3-11 y solo `throw new NotSupportedException();`, sin comentario;
  - **las dos clases nuevas de Core:** `PushBackViewEraseGuardTests` (CT-PBT3C-01 a 04) y `PushBackWritePathPluginGuardTests` (CT-PBT3C-05), un `[Fact]` por CT.
- El escritor (WR, `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`) no cambia en el RED. El GREEN toca solo WR y la fachada.
- **La reparación del RED (RR, F4B-E35)**, si la hubo: un commit «RED repair» entre `e0940a0efa314ea26777c59ab4e55c09d019bff6` y el GREEN. La entrega lo declara en `Deviations` con una entrada que empieza por `RedRepairSha <sha>`, y `RedSha` sigue siendo `e0940a0efa314ea26777c59ab4e55c09d019bff6`. En PB-T3c no puede tocar WR ni la fachada. La regla, RR-1 a RR-7, está en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`.
- WorkRunId `R20261120T135026Z-80e5`, en `artifacts/orchestration/I-64/F4B-PBT3C-FULL-WRITE-ERASE/1/`.
- BaseSha = ChainBaseSha = `7dfc7c21a0b30f5e04d691768b1717778e5c4ad7`, el commit de emisión del contrato (primera delegación). Su `src/` y sus `tests/` son los del cierre de PB-T3b (F4B-E46, `fe1c67f4`).

**Qué es PB-T3c:** dar al escritor de la actualización del Workspace de Push Back la capacidad de borrar una vista obsoleta dentro de la única transacción de MUTATE (D-PBT3-11):
- la fachada nueva `RackViewBlockEraser.EraseInTransaction`, caller-owned: sin bloqueo, sin transacción propia, sin confirmación y sin purga; borra las referencias directas y captura las definiciones anidadas, como el borrado clásico;
- su uso desde `EraseUnit` del ámbito de MUTATE (`RackPilotMutationScope`, que implementa `IPushBackPilotViewEraser`), con `PrepareUnits` y la relectura de MUTATE que cubren las vistas a borrar.

Application, la ventana, el clásico, el anfitrión y Domain no cambian. No se ejecuta AutoCAD: el borrado real en el dibujo es del Smoke-PB.

**Las autoridades** (en `938a35f0f1636b4306d844eb9e34000e8967c05e`):
- **El contrato** `docs/automation/evidence/I-64-pilot/F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json`:
  - sus invariantes INV-PBT3C-01 a 04 y los comunes 07 a 14;
  - el AllowedWriteScope (4 rutas), la autoridad UNIT_CHANGE (WR) y los RequiredTests;
  - las paradas, en especial C04, C07 (con la excepción de la reparación del RED), C13 (por nombre: las cinco declaraciones), C16, C17 y C18;
  - los totales exactos previstos de la CI (F4B-E46), de una sola rama.
- **El paquete** `docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md` v4.7:
  - el §3 (D-PBT3-11, D-PBT3-12 y D-PBT3-14);
  - el §4 y el §4.1 (las guardas del escritor de F4, que siguen en verde, y los barridos de PB-T3c);
  - el §5.0 (las reglas) y el §5.4 (CT-PBT3C-01 a 05, un `[Fact]` cada una, y por qué falla cada una en el RED);
  - el §6.0 (la regla RR y la tabla de RR-3) y el §6.3 (el esqueleto, la lista de PBT3C-C13 y los totales previstos).
- **El registro de sustitución** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md` (la entrada de D-2).
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`: F4B-E46 (`owner-pbt3b-nc3-decision.md`, los totales de cada rama), F4B-E45 (`owner-pbt3b-overload-decision.md`, la lección de las sobrecargas), F4B-E35 y D2 (`owner-d1-d2-decisions.md`).

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - ¿El RED es SOLO la fachada del parche, byte a byte, y las dos clases, con WR sin cambio? ¿El GREEN toca solo WR y la fachada?
   - ¿Toda declaración nueva de WR y de la fachada está entre las cinco de PBT3C-C13? ¿El único cambio de un miembro existente es el parámetro nuevo del constructor interno de `RackPilotMutationScope`? ¿El censo de la entrega cuadra con `git show`?
   - INV-PBT3C-01: ¿no cambian Application, UI, Domain, `RackPushBackCommands.cs` ni `RackCommandSupport.cs`?
2. **El diseño frente al código,** D-PBT3-11, punto a punto:
   - la fachada: ¿en su orden `GetObject(definitionId, OpenMode.ForRead)`, la comprobación de `IsLayout` que lanza, la captura de las anidadas, `GetBlockReferenceIds(directOnly: true, forceValidity: true)`, el `Erase()` de cada referencia, `UpgradeOpen()` y el `Erase()` de la definición? ¿Sin `LockDocument`, `StartTransaction`, `StartOpenCloseTransaction`, `.Commit(`, `Purge`, `Regen`, `SyncName` ni la cadena `PushBack`, y sin nombrar el piloto?
   - `EraseUnit`: ¿`EnsureOwned();`, la búsqueda en `_erasures` que lanza, `_units++;`, `FailUnitIfRequested(_units);`, la llamada a la fachada con `_transaction`, y las anidadas a `_staleDefinitions`? ¿Sin `SyncName`, purga, regeneración ni `Commit`?
   - `PrepareUnits` y la relectura: ¿recorren `plan.Erasures` y resuelven cada clave en `_definitions`, sin `PrepareRedraw` para ellas? ¿`Reread` calcula `matches` sobre `_prepared` y `_erasures`?
   - INV-PBT3C-03: ¿el escritor sigue con tres clases `RackPilot*`, un `StartTransaction`, un `Commit`, una purga y dos `[Conditional("DEBUG")]`? ¿Nombra `GetBlockReferenceIds`, `Erase(`, `EraseViewBlocks`, `ForWrite` o `UpgradeOpen`?
   - CT-PBT3C-04: ¿`RackViewBlockEraser.EraseInTransaction(` aparece exactamente una vez en `src/`, dentro de `EraseUnit`?
3. **D2 y su límite (lo más importante).** ¿Alguna copia comparte un objeto mutable fuera de las dos referencias declaradas? ¿Alguna línea muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?
4. **Cada CT** (CT-PBT3C-01 a 05): ¿fija lo que dice el §5.4? ¿Las autopruebas de CT-01, 04 y 05 discriminan de verdad, o son tautológicas? ¿Un `[Fact]` por CT, sin `Theory`? ¿Leen el texto con o sin comentarios, como las guardas de F4 que copian?
5. **El RED** (los TRX de `rv/red-ci-*`):
   - ¿fallan por aserción los cuatro métodos de `PushBackViewEraseGuardTests`, y pasa CT-05?
   - ¿los seis métodos de D-1 (de PB-T3b) siguen en `Skipped`, y ninguna otra prueba existente falla o se salta?
   - **los totales exactos (F4B-E46):** ¿la CI del RED da el total y cada parte previstos del contrato, sin otra rama?
   - **Si hubo reparación del RED:** ¿cumple RR-1 a RR-6, sin tocar WR ni la fachada? ¿Cambia algún literal de la **tabla de RR-3** de la clase reparada (la que el relevo midió en `rv/relay-record-work.json`, Notes, `TablaDeRR3`)?
6. **El GREEN y la estabilidad:**
   - la CI 4/4, con el Plugin compilado y los totales exactos previstos del GREEN;
   - ¿hay dependencias de orden, de cultura o de estado estático?
7. **Contención:**
   - ¿alguna ventana, un modal real, un `MessageBox`, el configurador, «Insertar», el BOM o «Guardar en biblioteca» reales?
   - ¿alguna escritura en disco o en el perfil?
   - ¿corrió el Worker en local algo de UI fuera de los filtros del contrato, o con un filtro de método?
   - ¿se tocó Application, Domain, el anfitrión, el puerto del panel o AutoCAD?
8. **Hallazgos para el Owner:** ¿algún riesgo del borrado real en el dibujo (capas bloqueadas, definiciones anidadas, referencias que quedan) que el Smoke-PB tenga que mirar y el paquete no declare?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt3ccc\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- D2;
- los CT;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
