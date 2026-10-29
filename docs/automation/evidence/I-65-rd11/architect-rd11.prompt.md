I-65 — REVISION DE CONFORMIDAD DEL ARCHITECT — gate correctivo R-D11 (tabla de secciones y materiales en el bundle del Autoloader)
Rol: Architect de I-65, SAME-SESSION ROLE (subagente de contexto independiente del Coordinator), SOLO LECTURA.
Repositorio: rama architecture/interop-analisis-estructural; worktree C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural.
Alcance de la revision: git diff 95aae3f1dd5ea6b86a3923cd98afbee1de79dc48..bf6ef110ab031b9fd62815b2a7f1a56a4288c0b8 (HEAD = bf6ef110ab031b9fd62815b2a7f1a56a4288c0b8): registro de la disposicion 91f82d6d,
           RED 42b9dcdf, GREEN 6a58c2b1 (EXECUTION_VERIFIED 14/14, verificacion R20261029T010523Z-2b56) y el commit de custodia efe7cc06 y la evidencia local bf6ef110.

REGLAS DE OPERACION (obligatorias; controles del Owner, decisiones §41):
- No escribas, no edites, no crees ni borres NINGUN archivo en ningun lugar; no hagas commit, push, fetch, checkout ni stash.
- No compiles ni ejecutes pruebas, scripts de deploy/, AutoCAD, RAM Elements ni ningun interprete (python, python3, py): solo lectura.
- Tu directorio de proceso es el checkout principal del Owner (D:/Documentos/Codex/Calculadora de racks): NO lo leas ni lo uses, tampoco
  con git -C. Lee SOLO con rutas absolutas del worktree de arriba o con git -C <worktree> show <sha>:<ruta> / git -C <worktree> diff.
- Primera accion: informa pwd y anotalo en tu informe.

Autoridades (leer desde Git en HEAD salvo indicacion): docs/automation/decisions/I-65-owner-disposition-ready-decisions-autoloader-gate.original.txt
(orden literal del Owner: §3 alcance permitido y prohibido, §4 protocolo del gate, §6 custodia), docs/automation/decisions/I-65-owner-answer-codex-ceiling-rd11.original.txt,
docs/automation/decisions/I-65.md (§105-§107), docs/initiatives/I-65-rd11-autoloader-gate.md (contrato del gate: §1 RD11-F-01..09,
§2 alcance e invariantes RD-INV-01..07, §4 AC-RD11-01..12, §6 prueba de host preparada), docs/automation/evidence/I-65-evidence.md
(§109-§111) y docs/automation/evidence/I-65-rd11/ (bundle previo, bundle corregido, RED del harness reproducido, instalacion desechable), docs/initiatives/I-65-ready-preparation.md (§4
evaluacion de R-D11), docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes), docs/adr/0050-interoperabilidad-analisis-estructural.md,
docs/adr/0003-referencias-autocad-para-ci.md, docs/automation/evidence/I-65-f4/architect-f4d-conformance.md (A65-F4D-01) y la custodia
de la cadena en docs/automation/evidence/I-65-pilot/RD11-AUTOLOADER-TABLE/ (contrato de gate, delegacion, entrega, verificacion,
controles nc1-nc4 y disposiciones del Coordinator D1-D6).

Preguntas (responde cada una con evidencia concreta: archivo, lineas, sha, blob):
1. Alcance y Freeze: ¿el diff de producto se limita a los siete archivos del contrato del gate §2? ¿El blob de la tabla sigue siendo
   d088e2a9? ¿Algun .cs de src/, tools/, assets/, eng/, .github/, build-bundle.ps1, install-bundle.ps1 o .gitattributes cambia? ¿Se
   conservan el Freeze, ADR-0050 (incluida la lista de permiso de G1) y el contrato de cuatro DLL de ADR-0003? ¿Hay algo que exija una A-n?
2. Target: ¿copia la tabla desde su fuente unica a Contents/structural-analysis/ram-elements/section-material-map.json, la ruta que
   RACKRAMPAQUETE resuelve (RackRamElementsCommands.cs: carpeta del DLL del Plugin) y la misma que el Link del Content? ¿Se conservan la
   regeneracion completa y los demas Copy?
3. Verificador: ¿sigue siendo fail-closed? ¿Admite solo la ruta exacta (-ceq) y rechaza la tabla en otras rutas y archivos extra?
   ¿Exige presencia e igualdad SHA-256 con la fuente? ¿Relaja o cambia alguna comprobacion o mensaje existente? Evalua D6: en la CI la tabla
   del bundle es el blob con CRLF (b22f7d9d) porque el checkout de Windows convierte y la tabla no tiene -text; el verificador compara con la
   fuente del mismo checkout. ¿Satisface «Exigir identidad exacta contra la tabla fuente aprobada» de la orden del Owner (§3.3), o es una
   limitacion que el Owner debe conocer y decidir (p. ej., -text en una unidad autorizada)? Clasificalo con su severidad.
4. Pruebas: ¿las tres pruebas C# y los cuatro casos nuevos del harness observan de verdad RD-INV-01..03? ¿Es honesto el RED (dos fallos por
   asercion en Core; el harness del RED solo en local porque build-plugin tiene needs de tests y build-ui: D2)? ¿Son suficientes las
   pruebas de texto sobre los scripts, dado que el comportamiento lo prueba el harness en la CI del GREEN? Evalua el criterio AC-RD11-02 del
   contrato del gate, que esperaba el job del Plugin en failure en el RED.
5. Documentacion: ¿README, la guia de I-65 y la de despliegue retiran solo la limitacion de CD-I65-F4-10 y conservan literalmente las
   demas (DIF-01, edicion del catalogo de RAM UNKNOWN, A65-F4R-07, NOT_EVALUATED, peso propio, paso manual)? ¿Algun texto presenta la tabla
   como validacion estructural? ¿Queda alguna afirmacion desactualizada sobre el Autoloader en documentos de la unidad (p. ej.,
   docs/initiatives/I-65-foundations-entry.md, I-65-ready-preparation.md) que deba corregirse antes de READY?
6. Proceso: verificacion del Controller y controles nc1-nc4; disposiciones D1 (lectura de estado del checkout principal no declarada por el
   Worker, sin efectos) y D5 (procesos efimeros de la app). ¿Alguna objecion?
7. Comportamiento del comando bajo el Autoloader: la evidencia del gate es estatica (ruta resuelta = ruta del bundle; el instalador copia el
   bundle completo; lector del producto sobre la tabla del bundle). La prueba real dentro de AutoCAD (HB-01..05) esta preparada y no
   autorizada, recomendada en W-R1 sobre el Candidato final. ¿Es suficiente para cerrar este gate? ¿Que riesgos quedan para READY?

Informe (texto, en espanol): veredicto CONFORMANT, CONFORMANT WITH OBSERVATIONS o NOT CONFORMANT; respuestas 1-7; tabla de hallazgos con id
A65-RD11-nn, ubicacion, hallazgo, severidad (REQUIRED u OPTIONAL) y recomendacion; lista «no comprobado sin ejecutar»; pwd informado. No
uses los terminos GATE PASS, Candidato ni integracion como declaracion.
