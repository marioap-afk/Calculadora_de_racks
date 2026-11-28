Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT4-EDIT-ALL-WINDOW (PB-T4c), intento 1. Es RackCad, un plugin de AutoCAD 2025 en C#/.NET 8 con WPF. No participaste en el trabajo. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- **El worktree, PROHIBIDO:** no leas, listes ni ejecutes nada en `<worktree-de-I-64>`, y no escribas esa ruta en ninguna línea de órdenes.
- **El código y los documentos, SOLO por objetos git** desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4ccc\rv\`:
  - `worker-handoff.json`;
  - `relay-record-work.json`, el registro del trabajo con lo que la sesión midió en sus Notes;
  - `relay-subst-check.json`: el detalle de lo que el relevo comprobó (los `Skip` archivo a archivo, los equivalentes con su `EquivalentSha`, el GREEN literal y los `warning CS`);
  - `tests/`, `inv/` y `gen/`, con las TRX, los logs y las salidas de sus guiones y de las herramientas del paquete (`apply-red.txt`, `apply-green.txt` y la del comprobador de los `Skip`);
  - `ci-core/` y `ci-ui/` (la CI del GREEN) y `red-ci-core/` y `red-ci-ui/` (la del RED).

**Las abreviaturas** (las del paquete):
- WHC: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- WEI: `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`;
- el puerto: `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs`;
- FVM: `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`;
- PV: `src/RackCad.UI/Workspace/WorkspacePanelView.xaml` (y su `.xaml.cs`);
- WCo: `src/RackCad.Plugin/Workspace/WorkspaceController.cs`;
- el abridor: `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`;
- PEG: `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs`.

**El objeto.** El diff es `git diff d502601477c21972ca2109b0d5c25c6fddfe4989 7cc1076eae8159294a1498ab556851977223b64c`, con dos commits:

1. **El RED, `280c79beac61d31b03f86488c03686eef4f694bc`.** Trae, sin las CT, lo que escribe la herramienta del paquete `apply_red_pbt4c_v51.py` (INV-PBT4C-02):
   - **El esqueleto, 8 archivos de `src/`.** No es un parche: lo da `green_pbt4c_v51.skeleton`, el generador custodiado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/green_pbt4c_v51.py` (sha256 `a8a3afa2…`), con sus piezas `pbt4c-skeleton-v51.txt` (`f8840c67…`) y `pbt4c-green-v51.txt` (`081ef77e…`). La precondición 6 lo registró en `F4B-DRAFTS/pbt4/cc4c/r5j-rg/apply-red.txt` y en el `Skeleton` de `pbt4c-precondition6.json`. Son cuatro existentes (el puerto con `TryEditAll` y su cuerpo por defecto; WEI con `EditAllStatus` y `EditAllOutcome`; WHC solo con `SetFullEditHost`, sin efecto; FVM con `ShowEditAllButton` en false y `EditAll` sin efecto) y cuatro nuevos, inertes: `IWorkspaceFullEditHost.cs`, `PushBackFullDraftLaunch.cs` y `PushBackFullDraftStatePort.cs` (de `Draft/`) y el abridor. La compuerta, WCo, PV y `RackPushBackSystemWindow.xaml.cs` no cambian en el RED.
   - **Los 4 `Skip` de la lista exacta** (`F4B-DRAFTS/pbt4/pbt4c-substitution-list-4.tsv`, sha256 `9bd4c17a…`), cada uno en la línea `[Fact]` de su prueba, con su motivo literal (`pbt4c_skips_v50.py`), y nada más en esos archivos: PEG:155 (C-5), `PushBackSharedReferenceGuardTests.cs`:306, CT-PBT1B-15 (C-5), `WorkspaceFullCopyGuardTests.cs`:568, CT-PBT4B-20 (C-5), y `PushBackWorkspaceApplyCloseTests.cs`:686, CT-PBT2B-06 (C-3, UI).
   - **Los 4 equivalentes,** nuevos y byte a byte los de `make_equivalents_pbt4c_v50.py`, con su `EquivalentSha` en la precondición 6: `PushBackEditAllPortEquivalenceGuardTests` (la fila de `TryEditAll` al final de `Operations`: 18), `PushBackEditAllNameWritersGuardTests` («archivos=15»), `PushBackEditAllFullCopyGuardTests` (la clase renombrada y `Restore`, que devuelve a `[Fact]` la línea del `Skip` de PEG:155) y, en UI, `PushBackEditAllApplyCloseTests` (la ventana 4 con la pregunta combinada; `ExpectRed` true: cae en el RED).
   - **Las cinco clases de las CT** de la lista cerrada CT-PBT4C-01 a 15 (§5.3 del paquete), un `[Fact]` por CT: `PushBackEditAllTests`, `PushBackEditAllGuardTests`, `PushBackOpenerClassicPathGuardTests` y `PushBackDraftLaunchGuardTests` (Core) y `PushBackEditAllWindowTests` (UI).
2. **El GREEN, `7cc1076eae8159294a1498ab556851977223b64c`.** El texto literal del paquete (`pbt4c-green-v51.txt`, que escribe `apply_green_pbt4c_v51.py`; su sha256, en el `GreenFiles` de la precondición 6): 13 archivos de `src/` (los 9 `UNIT_CHANGE` y los 4 nuevos). Ningún archivo de `Pilot/`, y ninguno de `tests/`.

Además: WorkRunId `R20261121T205731Z-41db`, en `artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-WINDOW/1/`. BaseSha = ChainBaseSha = `d502601477c21972ca2109b0d5c25c6fddfe4989`, el commit de emisión del contrato (F4B-E54) y primera delegación. Su `src/` y sus `tests/` son los del cierre de PB-T4b (F4B-E53, `6adff90d`).

**Qué es PB-T4c** (D-PBT4-12 a D-PBT4-17; el `Objective` del contrato):
- **«Editar todo» en el panel** (D-PBT4-12): el botón y su gesto, que confirma el texto pendiente con `CommitField` antes de abrir (R-PB-10).
- **`TryEditAll`** (D-PBT4-13): en el puerto, con su cuerpo por defecto, en el anfitrión y en sus dos reenvíos (la compuerta de avisos y el adaptador del panel del Plugin); empieza el borrador si hace falta y mantiene la marca de lectura mientras la ventana está abierta (K = 1; D-PBT4-14).
- **El modal y el abridor, en dos piezas** (D-PBT4-15): el modal del panel, en una clase anidada de WCo sobre `IWorkspaceFullEditHost`, y la ventana, sin dueño, en el abridor de `Plugin/Views`; el lanzamiento y el adaptador del puerto de `Draft/` conservan `Origin` y acaban la marca *known clean*.
- **La guarda de texto de OB-01.3** sobre los seis sujetos del camino (D-PBT4-16).
- **«Cerrar» en WORKSPACE con una sola pregunta** si hay a la vez un módulo escenificado y cambios sin aplicar (D-PBT4-17; A-T2b-OWNER-01), y el resumen y «Modificado» al cerrar.
- **Fuera de alcance:** el camino clásico, RACKEDITAR, la ventana en CLASSIC, cualquier escritura en el DWG desde la ventana, el código del piloto (PB-T4d) y AutoCAD, que no se ejecuta.

**Las autoridades** (en `7cc1076eae8159294a1498ab556851977223b64c`):
- **El contrato,** `docs/automation/evidence/I-64-pilot/F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json`:
  - sus invariantes (INV-PBT4C-01 a 14), el AllowedWriteScope (26 rutas), las autoridades UNIT_CHANGE y los RequiredTests (85 filtros, 4 con `ExpectRed`);
  - sus paradas, en especial C02 (ninguna excepción fuera de los 4 `Skip`), C07 (un equivalente que falla donde el paquete no lo espera), C13 (la lista cerrada de declaraciones), C16 (una prueba que falle fuera de la lista vuelve al Owner), C17 (D2) y C18 (la reparación del RED);
  - los totales exactos de cada rama (F4B-E46) y la línea base (la CI del cierre de PB-T4b).
- **El paquete,** `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` (v5.1): D-PBT4-12 a D-PBT4-17, §4.3 (la lista de 4 y las que siguen en verde), §5.3 (las CT, línea a línea), §6.0 (RR-3), §6.3 y el anexo V51-A (el texto literal del GREEN, del esqueleto y de los equivalentes).
- **Las decisiones de diseño de la sesión,** D-S7 a D-S10: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/session-pbt4c-design-decisions.md`.
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`: `owner-pbt4-decisions.md` (pregunta 4), `owner-pbt4c-list-3-decision.md`, `owner-pbt4c-list-4-decision.md`, `owner-pbt2b-close-decisions.md` (A-T2b-OWNER-01) y `owner-pbt4b-code-decisions.md`.
- **Las decisiones anteriores:** F4B-E35, F4B-E46 y D2 (`f4b-dispositions.md` y `owner-d1-d2-decisions.md`).
- **El registro de la sustitución,** la entrada «## PB-T4c» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`.

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - ¿El RED es solo el esqueleto, los 4 `Skip`, los 4 equivalentes y las cinco clases de las CT?
   - ¿El esqueleto es, byte a byte, el que da el generador custodiado (y el que registró `apply-red.txt`)?
   - ¿El GREEN es el texto literal del paquete, archivo a archivo, solo código del alcance, nada de `Pilot/` ni de `tests/`?
   - ¿Toda declaración nueva está en la lista de C13, por nombre y acotada a su archivo?
2. **La lista y los `Skip`.**
   - ¿Lo saltado es exactamente la lista de 4?
   - ¿En esos 4 archivos cambia algo más que la línea del atributo?
   - ¿El motivo es el literal de `pbt4c_skips_v50.py`, y `check_skips_pbt4c_v50.py` da PASA sobre RedSha y sobre CurrentSha?
3. **Los equivalentes.**
   - ¿Cada uno es, byte a byte, el que genera `make_equivalents_pbt4c_v50.py` (su `EquivalentSha`)? Compruébalo tú con `git show` y sha256, no con el resumen.
   - ¿Conserva cada uno lo que protegía su original y cambia solo lo que declara el paquete?
   - ¿El de CT-PBT2B-06 cae en el RED por aserción y pasa en el GREEN; los de Core pasan en las dos ramas?
4. **Lo que la entrega declara, y que tienes que juzgar tú:**
   - **(a) Sus desviaciones y sus hallazgos inesperados:** ¿alguno cambia lo que fija el contrato o el paquete? La entrega declara dos, y las tienes que juzgar contra la letra del contrato:
     - **la suite completa de Core en local,** corrida en tres particiones disjuntas cuya suma da los totales exactos;
     - **dos corridas del RED abortadas** por el detector de cuelgue de 10 minutos, mientras corría `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuild…`. Según la entrega, esa prueba tarda de 6 a 14 minutos en esta máquina. El Worker corrió esa clase sola, con 20 minutos en el RED y 30 en el GREEN.

     Responde a tres cosas:
     - ¿El contrato exige una sola corrida de la suite completa en local, o admite particiones?
     - ¿El Worker tenía esa latitud? Si no la tenía, ¿es una desviación que debe decidir el Owner, como la de CT-20 (5) en PB-T4b?
     - ¿Esa prueba tarda lo mismo en la CI de PB-T4b (`44903818806`) que en la de PB-T4c (`44959653142`)? Es decir, ¿la vuelve más lenta PB-T4c, o ya era lenta?
5. **El diseño frente al código,** D-PBT4-12 a 17, punto a punto:
   - el botón, su `Visibility` y el gesto con `CommitField` como frontera;
   - `TryEditAll`: su cuerpo por defecto en el puerto, el anfitrión y los dos reenvíos explícitos, y nada más en el puerto;
   - K = 1: la marca de lectura envuelve `TryEditAll`, el inicio del borrador y `ShowModal`, y la tabla de estados de D-PBT4-14 con el editor abierto;
   - el modal anidado en WCo y el abridor: `ForDraft` con un solo llamador (`RackWorkspaceEditorWindow.Show`) y `EnterPanelModal()` con otro (`FullEditorModal.ShowModal`);
   - el lanzamiento y el adaptador de `Draft/`: `Origin` y la marca *known clean*;
   - la guarda de OB-01.3 sobre los seis sujetos;
   - «Cerrar» en WORKSPACE: solo cambian `PendingWork()` y `WorkspaceStagedAndUnappliedQuestion` (INV-PBT4C-05).
6. **D2 y su límite (lo más importante).**
   - ¿Alguna copia comparte un objeto mutable fuera de las dos referencias declaradas de cada lado?
   - ¿Alguna línea muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?
   - ¿Se traduce algún estado del piloto al modelo completo?
7. **Cada CT** (CT-PBT4C-01 a 15):
   - ¿fija lo que dice el §5.3, línea a línea?
   - ¿las autopruebas de CT-PBT4C-07, 13 y 15 discriminan de verdad?
   - ¿hay un `[Fact]` por CT?
8. **El RED y el GREEN** (las TRX de `rv/`):
   - ¿fallan por aserción (la primera línea del StackTrace bajo `tests/`) los 4 filtros con `ExpectRed`: 9 en Core y, en UI, las 4 CT de UI y el equivalente de CT-PBT2B-06?
   - ¿las filas `Skipped` son las de la línea base (458 en Core y 48 en UI) más las de la lista (3 y 1)?
   - ¿alguna prueba falla fuera de la lista (C16)?
   - ¿cuadran los totales exactos de cada rama?
     - RED: Core 19 619 = 19 149 + 9 + 461, y UI 2 486 = 2 432 + 5 + 49.
     - GREEN: Core 19 619 = 19 158 + 0 + 461, y UI 2 486 = 2 437 + 0 + 49.
   - ¿ningún `warning CS`?
   - ¿la CI del GREEN da 4/4, con el Plugin?
9. **Contención:**
   - ¿alguna escritura en el DWG desde la ventana o después de cerrarla (INV-PBT4C-04: ni `LockDocument`, ni una transacción, ni `Commit(`)?
   - ¿alguna ventana o modal que quede vivo, un `MessageBox` real, el configurador, «Insertar», el BOM o «Guardar en biblioteca» reales?
   - ¿alguna escritura en disco o en el perfil?
   - ¿corrió el Worker algo de la UI fuera de los filtros del contrato?
10. **Hallazgos para el Owner:**
    - ¿alguna decisión que el contrato no le dejaba al Worker?
    - ¿algún riesgo de «Editar todo» o de «Cerrar» con las dos cosas que el Smoke-PB tenga que mirar y el paquete no declare?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4ccc\independent-review-code.md`, **sin rutas locales absolutas**. Si el entorno no te deja escribir el archivo, pon el informe entero en tu respuesta final. El informe lleva:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- la lista, los `Skip` y los equivalentes;
- el punto 4 (a);
- D2;
- las CT;
- los hallazgos OWNER;
- lo no verificado.

**Tu respuesta final,** en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
