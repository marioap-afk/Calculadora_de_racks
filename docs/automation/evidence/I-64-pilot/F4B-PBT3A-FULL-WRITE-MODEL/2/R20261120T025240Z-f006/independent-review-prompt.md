Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT3A-FULL-WRITE-MODEL, intento 2: la CORRECCIÓN de F4B-E42 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- Está PROHIBIDO leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- **El código y los documentos:** SOLO por objetos git desde el checkout principal, que comparte el almacén. Usa `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** (handoff, TRX, logs y salidas de sus scripts) está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt3acc2\rv\`:
  - `worker-handoff.json` y `relay-record-work.json` (el registro del trabajo, con lo que la sesión midió en sus Notes);
  - `tests/`, y `inv/` si existe;
  - `ci-core/` y `ci-ui/`: la CI del GREEN;
  - `red-ci-core/` y `red-ci-ui/`: la CI del RED;
  - `rr/`, `rr-ci-core/` y `rr-ci-ui/`, solo si hubo una reparación del RED: su diff y su CI.

**El objeto:**
- El diff es `git diff 485b5651bcec132761347b40ed38b60cfec0db04 6919e17af9fdd5c702d454a97a3c5b6cc6be1739`: solo archivos nuevos, ocho en `src/RackCad.Application/Systems/PushBack/Draft/` y seis clases de prueba. El RED intermedio es `5dbc9661438112c7be283a79ff5d980e78607067`: el esqueleto inerte y las pruebas.
- **La reparación del RED (RR, F4B-E35)**, si la hubo: un commit «RED repair» entre `5dbc9661438112c7be283a79ff5d980e78607067` y el GREEN. La entrega lo declara en `Deviations` con una entrada que empieza por `RedRepairSha <sha>`, y `RedSha` sigue siendo `5dbc9661438112c7be283a79ff5d980e78607067`. La regla, RR-1 a RR-7, está en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`.
- WorkRunId `R20261120T025240Z-f006`, en `artifacts/orchestration/I-64/F4B-PBT3A-FULL-WRITE-MODEL/2/`.
- BaseSha = ChainBaseSha = `485b5651bcec132761347b40ed38b60cfec0db04`, la reemisión del contrato con la v4.5 (F4B-E42): una corrección sin RED previo, porque el Worker del intento 1 (`R20261120T001738Z-7d92`) paró con PBT3A-C02 antes del RED, sin commits. Su `src/` y sus `tests/` son los de `18e4b783` (la emisión de la v4.4), y su `src/`, el del cierre de PB-T2c (`f273eae1`).
- **La corrección (C-1 de F4B-E42):** el miembro cerrado `ForWrite` de la v4.4 se llama `ForFullWrite`, porque `ForWrite` es un término de la guarda G-9 (`WorkspacePilotUpdateIntegrationGuardTests`), que barre todo Application y el Plugin, y `PushBackFullComposition.cs` nombra el piloto. Ninguna guarda ni prueba existente cambia. El Worker pudo partir de los parches del intento 1, con el nombre cambiado; no son autoridad.

**Qué es PB-T3a:** el modelo del borrador completo de Push Back en Application, sin conectarlo a nada (PB-T3b y PB-T3c lo conectarán).
- el estado y su comparador sobre `PushBackEditableState`;
- el asiento (la pasada y la secuencia de `LoadExisting`) y el materializador;
- el lector completo, sobre el del piloto: AUTH-13 intacto, A/B admitido solo en este modelo y la cola aparcada fuera;
- la composición para escribir (`ForFullWrite`; `ForWrite` en la v4.4) y para «Conservar mi borrador» (`KeepOn`);
- la disposición de las vistas y el clasificador de D-PB-05.

El piloto, el anfitrión, la ventana, el Plugin y Domain no cambian.

**Las autoridades** (en `6919e17af9fdd5c702d454a97a3c5b6cc6be1739`):
- **El contrato** `docs/automation/evidence/I-64-pilot/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json`:
  - sus invariantes INV-PBT3A-xx, sobre todo 01 a 06, 13 y 14;
  - el AllowedWriteScope (14 rutas) y los RequiredTests;
  - las paradas, en especial C04, C07 (con la excepción de la reparación del RED), C13 (por nombre), C15, C17 y C18.
- **El paquete** `docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md` v4.5 (V45-01, el nombre; §4.1, el barrido de colisiones con las guardas):
  - el §3 (D-PBT3-02 a D-PBT3-08, con su código literal, que es la API cerrada; D-PBT3-12 a 14);
  - el §5.0 (las reglas, el momento de cada lectura, los gestos que no cambian nada y los que el catálogo deja sin opción) y el §5.2 (CT-PBT3A-01 a 14, sus oráculos y por qué falla cada una en el RED);
  - el §6.0 (la regla RR y la tabla de RR-3) y el §6.1 (el esqueleto exacto del RED).
- **La comprobación acotada** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt3/independent-review-pbt3a-v43-check.md`.
- **El análisis de la sesión sobre el intento 1** (el que el contrato cita; vinculante: C-1 a C-4).
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`: F4B-E42 (`owner-pbt3a-g9-decision.md`), F4B-E35, D2 (`owner-d1-d2-decisions.md`) y AUTH-13 (F4B-E24).

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - ¿El RED es SOLO el esqueleto exacto del §6.1 en los ocho archivos y las seis clases? ¿El GREEN toca solo los ocho archivos de `Draft/`?
   - ¿Ningún archivo existente de `src/` ni de `tests/` cambia? En particular, ¿G-9 y las demás guardas siguen como en `485b5651bcec132761347b40ed38b60cfec0db04`?
   - C-1: ¿algún archivo de `Draft/` contiene el token `ForWrite`? ¿`ForFullWrite` tiene la firma de D-PBT3-05 en el RED y en el GREEN? ¿Pasa G-9 (y su autoprueba) en `rv/red-ci-core` y en `rv/ci-core`?
   - ¿Toda declaración pública o interna está en la lista cerrada de D-PBT3-02 a D-PBT3-07, y los privados son solo los que admite INV-PBT3A-13?
2. **El diseño frente al código,** D-PBT3-02 a D-PBT3-07, punto a punto:
   - ¿el asiento reproduce la pasada y la secuencia de `LoadExisting` de la ventana?
   - ¿el lector completo llama a `PushBackPilotIntentReader.Read` una vez por lectura, y el materializador a `LoadExisting` una vez? ¿AUTH-13 sigue igual?
   - ¿`ForFullWrite` y `KeepOn` hacen lo del paquete sin componer ni construir un `RackEmbedDocument` (INV-PBT3A-04)?
   - ¿la disposición de las vistas y el clasificador siguen sus tablas, con la fila por defecto?
3. **D2 y su límite (lo más importante).** ¿Alguna copia comparte un objeto mutable fuera de las dos referencias declaradas? ¿Alguna línea muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?
4. **Cada CT** (CT-PBT3A-01 a 14): ¿fija lo que dice el §5.2, con su oráculo? ¿Hay aserciones tautológicas?
   - ¿el oráculo mecánico de CT-09 y CT-10 es independiente del producto?
   - ¿CT-04 se comprueba a sí misma con su línea `control`?
   - las lecciones del §5.0: ¿cada línea se lee en su momento? ¿Algún gesto escribe un valor igual al del diseño o en un campo que el ensamblador ajusta (`BeamDepth`)?
5. **El RED** (los TRX de `rv/red-ci-*`):
   - ¿fallan por aserción todos los métodos de los dos filtros con `ExpectRed`?
   - ¿pasan las guardas, y sus autopruebas discriminan?
   - ¿ninguna prueba existente falla?
   - **Si hubo reparación del RED:**
     - ¿cumple RR-1 a RR-6? Es decir: un solo commit «RED repair» con la tarea; solo clases de prueba que añadió el RED, **ningún archivo de `src/`** (ni el esqueleto); y el GREEN, después, solo `Draft/`.
     - ¿Cambia algún literal de la **tabla de RR-3** de la clase reparada? Es la que el relevo midió en `rv/relay-record-work.json` (Notes, `TablaDeRR3`): las líneas, como subcadena exacta, y los valores de los gestos, como token, con las mismas cuentas que en `5dbc9661438112c7be283a79ff5d980e78607067`. Si una cambia, es parada (C07).
     - ¿Algún hunk cambia el primer argumento de un `Assert.Equal`, una cadena `campo=valor` u otro literal (`RR3AyudaMecanica` y `LiteralesCambiados`)? Esos son indicios: júzgalos.
     - ¿El criterio del RED se cumple sobre su CI (`rv/rr-ci-*`)? ¿Lleva el commit «RED repair» el mismo trailer que el RED y el GREEN?
6. **El GREEN y la estabilidad:**
   - la CI 4/4 con los recuentos previstos (Core + 13; UI + 1);
   - ¿hay dependencias de orden, de cultura o de estado estático?
7. **Contención:**
   - ¿alguna ventana mostrada, un modal real, un `MessageBox`, el configurador, «Insertar», el BOM o «Guardar en biblioteca» reales?
   - ¿alguna escritura en disco o en el perfil?
   - ¿corrió el Worker en local algo de UI fuera de los filtros del contrato, o con un filtro de método?
   - ¿se tocó el Plugin, Domain, la ventana o AutoCAD?
8. **Hallazgos para el Owner:** ¿alguna pérdida o diferencia observable que el modelo fije sin que el paquete la declare?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt3acc2\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- D2;
- los CT;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
