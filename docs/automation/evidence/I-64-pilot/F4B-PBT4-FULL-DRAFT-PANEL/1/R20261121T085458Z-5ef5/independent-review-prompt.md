Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT4-FULL-DRAFT-PANEL (PB-T4b), intento 1. Es RackCad, un plugin de AutoCAD 2025 en C#/.NET 8 con WPF. No participaste en el trabajo. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- **El worktree, PROHIBIDO:** no leas, listes ni ejecutes nada en `<worktree-de-I-64>`, y no escribas esa ruta en ninguna línea de órdenes.
- **El código y los documentos, SOLO por objetos git** desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4bcc\rv\`:
  - `worker-handoff.json`;
  - `relay-record-work.json`, el registro del trabajo con lo que la sesión midió en sus Notes;
  - `relay-subst-check.json`: el detalle de lo que el relevo comprobó (los `Skip` archivo a archivo, las copias, la partición medida y los `warning CS`);
  - `tests/` e `inv/`, con las TRX, los logs y las salidas de sus guiones;
  - `ci-core/` y `ci-ui/` (la CI del GREEN), `red-ci-core/` y `red-ci-ui/` (la del RED), y `rr/`, `rr-ci-core/` y `rr-ci-ui/` (la reparación del RED).

**Las abreviaturas** (las del paquete):
- WHC: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- WEI: `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`;
- FVM: `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`;
- PV: `src/RackCad.UI/Workspace/WorkspacePanelView.xaml`;
- RIA: `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`;
- NFV: `src/RackCad.UI/Controls/NumericFieldValidation.cs`.

**El objeto.** El diff es `git diff 8f783df58fb0e5688d58299951fcbdcde727f33e c065080d3fd1dd8bb1937b98b81e9a0c1714777b`, con tres commits:

1. **El RED, `c1e4f4585217abc8c85d90b499a07bb93c84be2c`** (55 rutas). Trae:
   - **El esqueleto.** No es un parche: lo escribe el generador custodiado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/green_pbt4b_v45.py` (sha256 `47662f52…`, con su módulo `green_pbt4b.py`). La precondición 6 registró sus seis archivos en `F4B-DRAFTS/pbt4/cc4b/out-redgreen-v49-r4v/apply-red.txt`. Esos seis archivos de `src/`, inertes, son: `NumericTextRule.cs`, `PushBackAnnotationScaleRule.cs`, `PushBackFullFieldAdapter.cs`, `PushBackKeepMinePresentation.cs`, WEI y FVM. No toca NFV.
   - **Los `Skip` de la lista exacta de 478** (`F4B-DRAFTS/pbt4/pbt4b-substitution-list-478.tsv`, sha256 `8b0fcd0c…`), en 21 archivos: C-0 428, en las nueve clases del panel de F4; C-5 23, las guardas por su letra; y UI 27. En esos archivos solo cambia la línea del atributo, con su motivo literal.
   - **«Saltar y duplicar»:**
     - 21 archivos de copia en el espacio de nombres raíz de su ensamblado, con las clases `FullPanelCopy<Clase>RedTests` y `FullPanelCopy<Clase>GreenTests` (D-S6). El `CopySha` de cada uno está en `F4B-DRAFTS/pbt4/pbt4b-precondition6.json`.
     - En las nueve clases de C-0 hay 356 copias mecánicas, el original con el mapa M-00 a M-08, y 72 equivalentes escritos a mano (D-S1), según el mapa `pbt4b-copy-map-v5.tsv` (`bda79bb7…`).
     - Cuatro `Theory` van partidas por filas (D-S5).
     - El kit es el de la v4.5.
     - La partición Red/Green es `e87e024c…`, con R_C = 291 y R_UI = 0.
   - **Las CT de PB-T4b,** CT-PBT4B-01 a 21. CT-19 y CT-20 pasan en el RED.
2. **La reparación del RED, `9b2d18937d3aa03b4cbeaf0546cd0e6b44a4f6fe`**, hijo directo del RED. Solo cambia `tests/RackCad.Tests/PushBackFullPanelTests.cs`: CT-PBT4B-08 deja de comparar `Name` en «el resto del sobre del rack actual». Su regla, la RR de F4B-E35, está en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`, y su lectura RR-3 en las filas de §6.0 del paquete.
3. **El GREEN, `c065080d3fd1dd8bb1937b98b81e9a0c1714777b`.** Toca 9 archivos de código: los cuatro nuevos del esqueleto, WHC, RIA, NFV, PV y FVM. Tiene que ser el texto literal del GREEN del paquete, el que escribe el mismo generador. Ningún archivo de `Pilot/`, y ninguno de `tests/`.

Además: WorkRunId `R20261121T085458Z-5ef5`, en `artifacts/orchestration/I-64/F4B-PBT4-FULL-DRAFT-PANEL/1/`. BaseSha = ChainBaseSha = `8f783df58fb0e5688d58299951fcbdcde727f33e`, el commit de emisión del contrato y primera delegación. Su `src/` y sus `tests/` son los del cierre de PB-T4a.

**Qué es PB-T4b** (D-PBT4-04 a 11 y D-PBT4-21):
- **El borrador del panel pasa al modelo completo** de PB-T3, con la lectura completa de PB-T4a y sin traducir estados.
- **La regla única de la escala, en Application** (`NumericTextRule`): NFV delega en ella sin cambiar su API ni su comportamiento, y `PushBackAnnotationScaleRule` la usa.
- **«Actualizar»** con `PrepareFull` y `ExecuteFull`.
- **«Conservar mi borrador»** con dos «Sí» (D-S2): el primero presenta «Por grupos» y devuelve el testigo; el segundo, con el testigo vigente, hace `KeepOn`.
- **Una sola materialización del modelo completo por lectura** (D-S4: el `Lazy` en RIA).
- **Fuera de alcance:** «Editar todo», RACKEDITAR, la ventana y AutoCAD, que no se ejecuta.

**Las autoridades** (en `c065080d3fd1dd8bb1937b98b81e9a0c1714777b`):
- **El contrato,** `docs/automation/evidence/I-64-pilot/F4B-PBT4-FULL-DRAFT-PANEL/coordinator/gate-contract.json`:
  - sus invariantes, el AllowedWriteScope, las autoridades UNIT_CHANGE y los RequiredTests;
  - sus paradas, en especial C02 (ninguna excepción fuera de los `Skip`), C07, C13, C16 (una prueba que falle fuera de la lista vuelve al Owner), C18 y C20, y la de D2;
  - los totales exactos de cada rama (F4B-E46).
- **El paquete,** `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` (v4.9): D-PBT4-04 a 11, D-PBT4-21, §4.2, §5.2 (las CT, línea a línea), §6.0 (RR-3), §6.2 y los anexos V48-A y V49-A.
- **Las decisiones de diseño de la sesión,** D-S1 a D-S6: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/session-pbt4b-design-decisions.md`.
- **Las siete tandas del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`: `owner-pbt4-decisions.md`, `owner-pbt4b-retire-list-decisions.md`, `owner-pbt4b-breaks-decision.md`, `owner-pbt4a-cost-decision.md`, `owner-pbt4b-list-477-decision.md`, `owner-pbt4b-list-478-decision.md` y `owner-pbt4b-cost-decision.md`.
- **Las decisiones anteriores:** la D-PB-05 de F4B (`f4b-dispositions.md`), F4B-E35, F4B-E46 y D2.
- **El registro de la sustitución,** la entrada «## PB-T4b» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`.

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - ¿El RED es solo el esqueleto, los `Skip`, las copias, el kit y las CT?
   - ¿El esqueleto es, byte a byte, el que da el generador custodiado (y el que registró `apply-red.txt`)?
   - ¿El GREEN es el texto literal del paquete, solo código del alcance, nada de `Pilot/` ni de `tests/`?
   - ¿Toda declaración nueva está en la lista de C13?
2. **La lista y los `Skip`.**
   - ¿Lo saltado es exactamente la lista de 478?
   - ¿En los 21 archivos cambia algo más que la línea del atributo?
   - ¿El motivo literal es el de su causa en el registro?
3. **«Saltar y duplicar».**
   - ¿Cada copia mecánica es, prueba a prueba, el original con el mapa M y nada más? Compáralo tú con `git show`, no con el resumen.
   - ¿Cada equivalente a mano conserva lo que protegía la original y afirma lo nuevo donde el original afirmaba lo viejo?
   - ¿Las cuatro de D-S5 reparten sus filas sin cambiar ninguna?
   - ¿Las copias de C-5 y de UI, y el kit, tienen el `CopySha` del contrato?
4. **Lo que la entrega declara, y que tienes que juzgar tú:**
   - **(a) CT-PBT4B-20, su comprobación (5).** El paquete (§5.2, fila 20) dice: «(5) Ninguna copia nombra `EditIntentFakeReader`, `EditIntentKit`, `PrepareKit`, `PushBackPilotDraftState`, `PushBackPilotStateComparer`, `PushBackPilotFieldAdapter` ni `PushBackFullReadRig` como palabra completa». El Worker la acotó a las nueve clases de C-0 (`tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs`, alrededor de :630): según él, las copias de guarda nombran esos tipos en el texto que inyectan sus autopruebas.
     - ¿Es una desviación del texto del contrato?
     - ¿Qué copias nombran esos tipos, y dónde: en código o solo en literales inyectados?
     - ¿Qué daría la comprobación (5) tal cual sobre el GREEN?
     - ¿Hay alguna otra comprobación de CT-20, o de otra CT, que el Worker haya cambiado respecto al §5.2?
   - **(b) La reparación del RED.**
     - ¿Cumple RR-1 a RR-6 y la n.º 10?
     - ¿Es legítima la razón? `Name` está en el estado editable (`src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs`), y la fila 08 del §5.2 dice «el resto del sobre del rack actual (los campos no editables, la identidad y la versión)».
     - ¿Cambia alguna línea esperada?
   - **(c) El nombre tras «Conservar» y «Actualizar».** El sobre escrito lleva el nombre del borrador, no el que se cambió fuera del panel. ¿Lo cubre D-PB-05 («gana el diseño EDITABLE completo, pero sobre, identidad, esquema, sesión y ExtensionData no vuelven atrás»)? ¿Lo presenta «Por grupos» antes del segundo «Sí»?
5. **El diseño frente al código,** D-PBT4-04 a 09 y 21, punto a punto:
   - `Begin`, la admisión y su diagnóstico, y el testigo;
   - `FullReadMissingReason`;
   - la regla de la escala: la delegación de NFV, las constantes y que el comportamiento sea idéntico;
   - «Actualizar»;
   - «Conservar» en dos tiempos: `Presented`, `KeepOn` solo tras confirmar con el testigo vigente, y la presentación con sus literales y sus líneas;
   - el `Lazy` de RIA.
6. **D2 y su límite (lo más importante).**
   - ¿Alguna copia comparte un objeto mutable fuera de las referencias declaradas?
   - ¿Alguna línea muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?
   - ¿Se traduce algún estado del piloto al modelo completo?
7. **Cada CT** (CT-PBT4B-01 a 21):
   - ¿fija lo que dice el §5.2, línea a línea?
   - ¿las autopruebas en negativo discriminan de verdad?
   - ¿hay un `[Fact]` por CT?
8. **El RED y el GREEN** (las TRX de `rv/`):
   - ¿fallan por aserción los filtros con `ExpectRed`, en Core y en UI?
   - ¿las filas `Skipped` son las de la base más las de la lista?
   - ¿alguna prueba falla fuera de la lista (C16)?
   - ¿cuadran los totales exactos de cada rama?
     - RED: Core 19 605 = 18 841 + 306 + 458, y UI 2 481 = 2 430 + 3 + 48.
     - GREEN: Core 19 605 = 19 147 + 0 + 458, y UI 2 481 = 2 433 + 0 + 48.
   - ¿ningún `warning CS`?
   - ¿la CI del GREEN da 4/4, con el Plugin?
9. **Contención:**
   - ¿alguna ventana, un modal real, un `MessageBox`, el configurador, «Insertar», el BOM o «Guardar en biblioteca» reales?
   - ¿alguna escritura en disco o en el perfil?
   - ¿corrió el Worker algo de la UI fuera de los filtros del contrato?
10. **Hallazgos para el Owner:**
    - ¿alguna decisión que el contrato no le dejaba al Worker?
    - ¿algún riesgo de «Conservar» en dos tiempos o de la presentación que el Smoke-PB tenga que mirar y el paquete no declare?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4bcc\independent-review-code.md`, **sin rutas locales absolutas**. Si el entorno no te deja escribir el archivo, pon el informe entero en tu respuesta final. El informe lleva:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- la lista y las copias;
- los puntos 4 (a), (b) y (c);
- D2;
- las CT;
- los hallazgos OWNER;
- lo no verificado.

**Tu respuesta final,** en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
