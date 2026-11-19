Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT2C-WORKSPACE-CARRYOVER, intento 1 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- Está PROHIBIDO leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- **El código y los documentos:** SOLO por objetos git desde el checkout principal, que comparte el almacén. Usa `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** (handoff, TRX, logs y salidas de sus scripts) está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt2ccc\rv\`:
  - `worker-handoff.json` y `relay-record-work.json` (el registro del trabajo, con lo que la sesión midió en sus Notes);
  - `tests/`: sus corridas, entre ellas las aisladas de cada mutante (`tests/m1`, `m2` y `m3`) y, si hubo reparación, las repetidas (`tests/rm1`, `rm2` y `rm3`);
  - `ci-core/` y `ci-ui/`: la CI del GREEN;
  - `red-ci-core/` y `red-ci-ui/`: la CI del RED;
  - `rr/`, `rr-ci-core/` y `rr-ci-ui/`, solo si hubo una reparación del RED: su diff y su CI.

**El objeto:**
- El diff neto es `git diff 05beabfdd9efa5bb530860dc156f5832e4e452c2 b19683412ec1202bac141aa80654eddad274a023`: solo la clase nueva, `tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs`. El RED intermedio es `7aeb61707b7ffaccd2e15c1366ade039c7ecd674`: la clase y los tres mutantes en W y WC (`git diff 05beabfdd9efa5bb530860dc156f5832e4e452c2 7aeb61707b7ffaccd2e15c1366ade039c7ecd674`).
- **La reparación del RED (RR, F4B-E35)**, si la hubo: un commit «RED repair» entre `7aeb61707b7ffaccd2e15c1366ade039c7ecd674` y el GREEN. La entrega lo declara en `Deviations` con una entrada que empieza por `RedRepairSha <sha>`, y `RedSha` sigue siendo `7aeb61707b7ffaccd2e15c1366ade039c7ecd674`. La regla, RR-1 a RR-7, está en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`.
- WorkRunId `R20261119T203340Z-05b7`, en `artifacts/orchestration/I-64/F4B-PBT2C-WORKSPACE-CARRYOVER/1/`.
- BaseSha = ChainBaseSha = `05beabfdd9efa5bb530860dc156f5832e4e452c2`, el commit de emisión del contrato (primera delegación). Su `src/` es el del cierre de PB-T2b (`56cb026f`): W tiene el modo WORKSPACE.

**Qué es PB-T2c:** fija con pruebas nuevas, sin cambiar el producto, lo que hereda el modo WORKSPACE de la ventana de Push Back.
- **CT-PBT2C-01:** el paso 4 de D-PB-06: los valores exactos de las filas F, `pb-08-redondeo` y el estilo de cotas llegan al borrador, leídos de él justo tras «Aplicar» y nunca de la petición clásica.
- **CT-PBT2C-02:** el aviso del lado B de D-PB-08, con las doce líneas de CT-PBTC-07 y una de «Aplicar».
- **CT-PBT2C-03:** el censo: los 49 gestos de CT-PBT1C-02 en WORKSPACE, con la tabla custodiada.
- **El régimen** es el de PB-T1a:
  - el RED son la clase y los tres mutantes de una línea (MU-T2C-01 en `Keep`, MU-T2C-02 en `UpdateSideBEmptyNotice` de WC y MU-T2C-03 en `ApplyToDraft`);
  - el GREEN los revierte con `git restore --source=BaseSha`;
  - cada mutante, aislado, hace fallar por aserción el CT de su fila, con su TRX;
  - lo que falle de las pruebas existentes se atribuye a un mutante.

**Las autoridades** (en `b19683412ec1202bac141aa80654eddad274a023`):
- **El contrato** `docs/automation/evidence/I-64-pilot/F4B-PBT2C-WORKSPACE-CARRYOVER/coordinator/gate-contract.json`:
  - sus invariantes INV-PBT2C-xx, sobre todo 01 a 04, 08, 10 y 14;
  - el AllowedWriteScope (3 rutas) y los RequiredTests;
  - las paradas, en especial C02 (la atribución), C04, C07 (con la excepción de la reparación del RED), C13, C15, C17 y C18.
- **El paquete** `docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md` v2.9:
  - el §5.0 (el régimen, el sufijo `descartes`, el momento de cada lectura, los gestos y las recetas);
  - el §5.4 (CT-PBT2C-01 a 03, con sus líneas y sus fuentes, y la tabla cerrada de mutantes);
  - el §6.0 y el §6.3.
- **La salida de los anclajes** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt2/pbt2c-anchors-out.txt`: los textos de los mutantes y la tabla de las 49 filas de CT-PBT2C-03, que es autoridad.
- **La comprobación acotada** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt2/independent-review-pbt2c-v25-check.md`, con sus recomprobaciones (§9 a §12).
- **Los oráculos:** CT-PBTC-02, 05 y 07 (`tests/RackCad.UI.Tests/PushBackDeclaredCorrection*Tests.cs`) y CT-PBT1C-02 (`Gestos()` de `PushBackWindowOnStateTests.cs`).
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`: F4B-E06 y E09 (`owner-pbt1-pre-decisions.md`) y D2 (`owner-d1-d2-decisions.md`).

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - ¿El RED es SOLO la clase y las tres sustituciones de una línea de la tabla, cada una en su línea y con su sangría? ¿Nada más en W ni en WC?
   - ¿El GREEN es exactamente la reversión, con los blobs de W y WC iguales a los de `05beabfdd9efa5bb530860dc156f5832e4e452c2`?
   - ¿Ninguna prueba ni ayudante existente cambia (`PushBackWorkspaceSupport` incluido)?
2. **Cada CT frente al §5.4:**
   - **CT-01:** ¿lee todo del borrador justo tras «Aplicar», con `c = Pasada(Copy(draft.CurrentDraftState), A)`? ¿`Persist(c.Design)`, `c.System.Structure.AnnotationScale`, el estilo de `c.Design` y `c.System`, y `Cotas` aplicado a `c` y no a `w.LastComputation`? ¿Nunca la petición clásica? ¿El gesto neutro es el de `DrawRackNameCheck`?
   - **CT-02:** ¿los gestos de CT-PBTC-07 copiados? ¿Las doce líneas, con ` descartes=1` en la última de cada ventana? ¿La tercera ventana con «Aplicar» y `diseno-del-borrador-compuesto` leído tras él?
   - **CT-03:** ¿los 49 gestos de `Gestos()` con `Compara` = true, en su orden y sobre el mismo diseño? ¿Las 49 líneas copiadas de la tabla, sin las líneas `porque:`? ¿Una ventana nueva por gesto?
   - ¿Hay aserciones tautológicas? ¿Alguna línea se lee en otro momento del que dice su CT (las lecciones de F4B-E30 y F4B-E33)?
3. **Los mutantes** (`rv/tests/m1` a `m3`):
   - ¿cada TRX muestra el CT de su fila fallando por aserción, y no por excepción?
   - ¿Los mutantes discriminan lo que dice su fila, o el CT fallaría por otra razón?
4. **El RED** (`rv/red-ci-*`):
   - ¿el filtro de la clase falla entero por aserción?
   - **La tabla de atribución:** ¿cada prueba existente que falla en el RED (Core y UI) está atribuida a un mutante con un análisis creíble? ¿Falta alguna?
   - **Si hubo reparación del RED:**
     - ¿cumple RR-1 a RR-6? Es decir: un solo commit «RED repair» con la tarea; **solo la clase nueva, sin W ni WC**; y el GREEN, después, solo la reversión.
     - ¿Cambia alguna línea esperada, literal u oráculo (RR-3)? Compruébalo con `git diff 7aeb61707b7ffaccd2e15c1366ade039c7ecd674 <RedRepairSha>`. ¿Corrige solo el cuándo o el cómo de una lectura o de un gesto?
     - ¿La lista cerrada de PB-T2c sigue byte a byte? Es la que el relevo midió y registró en `rv/relay-record-work.json` (Notes, `ListaCerradaDePBT2c`): las 49 de CT-PBT2C-03 y las fijas de CT-PBT2C-01 y 02. Si una cambia, es parada (C07). ¿Algún hunk cambia el primer argumento de un `Assert.Equal`, una cadena `campo=valor` u otro literal (`RR3AyudaMecanica` y `LiteralesCambiados`)? Esos son indicios: júzgalos.
     - ¿Se repitieron las corridas aisladas sobre la clase reparada (`rv/tests/rm1` a `rm3`), después del commit «RED repair»? ¿El criterio del RED se cumple sobre su CI (`rv/rr-ci-*`)? ¿Lleva el commit «RED repair» el mismo trailer que el RED y el GREEN?
5. **D2 y su límite:** ¿alguna línea nueva muta el rack resuelto o el informe de módulos, o comparte un objeto mutable fuera de las dos referencias declaradas?
6. **El GREEN y la estabilidad:**
   - la CI 4/4 con los recuentos previstos (Core igual; UI + 3);
   - ¿hay dependencias de orden, de cultura o de estado estático (el aviso de descarte sustituido, la colección)?
7. **Contención:**
   - ¿alguna ventana mostrada, un modal real, un `MessageBox`, el configurador o «Insertar» reales?
   - ¿alguna escritura en disco fuera de `ArchivoTemporal`, o en el perfil? La disposición del configurador puede cambiar por pruebas existentes (INV-PBT2C-10): es un defecto conocido y anterior, anotado en `docs/ideas-futuras.md`, y no cuenta;
   - ¿corrió el Worker en local algo de UI fuera de los filtros del contrato, o con un filtro de método?
   - ¿se tocó el Plugin, WX o AutoCAD?
8. **Hallazgos para el Owner:** ¿alguna diferencia observable entre CLASSIC y WORKSPACE que las pruebas fijen sin que el paquete la declare?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt2ccc\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla CT → código;
- los mutantes y la tabla de atribución;
- D2;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
