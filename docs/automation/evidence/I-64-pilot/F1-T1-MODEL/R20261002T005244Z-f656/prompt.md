I-64 — DELEGACION F1-T1-MODEL — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 0; RunId R20261002T005244Z-f656;
           DelegationRunId R20261002T003827Z-3a76; AuthorityRevision e0587355b0e84d98807057a56dfc50591da87489;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha e0587355b0e84d98807057a56dfc50591da87489;
           rama architecture/workspace-persistente-rackcad;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-workspace-persistente-rackcad;
           Owner subagent:R20261002T003827Z-3a76
Autoridades (leer desde Git, no copiar; ruta | seccion | clase):
           - docs/initiatives/I-64-proposal-v5.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-discovery.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-workspace-persistente-rackcad.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 18. Consensus Freeze (I64-CONSENSUS-V5-01) y cierre de F0 | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 19. F1-T0-ADR — Contrato recibido, preflight, relevo de salida y STOP antes de la planificación | UNIT_DOC
           - docs/automation/state/I-64.yml | documento completo | UNIT_DOC
           - docs/initiatives/I-64-f1-gate-contract-proposal.md | documento completo | UNIT_DOC
           - AGENTS.md | documento completo | EXTERNAL
           - docs/WORKFLOW.md | documento completo | EXTERNAL
           - docs/INITIATIVE_LIFECYCLE.md | documento completo | EXTERNAL
           - docs/AUTOMATION_PLAN.md | ## 16. Ejecución delegada bajo orden del Coordinator | EXTERNAL
           - docs/automation/agent-execution/README.md | documento completo | EXTERNAL
           - docs/automation/agent-execution/routing.md | documento completo | EXTERNAL
           - docs/adr/0006-autocad-solo-en-plugin.md | documento completo | EXTERNAL
           - docs/adr/0019-shell-visual-de-editores-por-composicion.md | documento completo | EXTERNAL
           - docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md | documento completo | EXTERNAL
           - docs/adr/0044-hechos-neutrales-de-vistas-compartidas.md | documento completo | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.4 La transaccion la posee `Mutate(units)` (AR4-19) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.5 Capas bloqueadas (AR4-20) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.6 R-25: la primera colocacion sin supervivientes (AR4-21) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.7 Un solo barrido (AR4-42) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.8 Estados e informe (AR4-40) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.9 Guardas caller-owned (AR4-43) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.10 Cuenta completa de transacciones (AR4-44) | EXTERNAL
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
Objetivo (texto literal del contrato; viñetas en linea, sin lineas en blanco):
           Construir la primera fundación pura de Workspace de I-64:
           * ADR 0047 en estado `propuesto`, fiel al Freeze; * modelo puro en RackCad.Application para sesión por documento; * identidad de instancia; * SelectionContext; * estado mínimo de pistas/cola; * reglas puras necesarias para el drenaje; * políticas puras congeladas por F1 que no dependan de AutoCAD.
           La cadena debe demostrar RED→GREEN real.
           No implementar todavía:
           * PaletteSet; * WPF host; * AutoCAD event bridge real; * selección real; * navegación; * escritura authored; * AutoCAD.
           ADR 0047:
           Ruta:
           docs/adr/0047-workspace-persistente-modeless-rackcad.md
           Estado:
           propuesto
           Debe ser una proyección del Anexo A del Freeze.
           No:
           * crea decisiones nuevas; * reinterpreta el Freeze; * adopta optional históricos; * se marca accepted; * se convierte en autoridad superior al Freeze.
           El Worker debe materializar el ADR antes de la implementación GREEN.
           Puede estar en el commit inicial documental o en el RED, siempre que:
           * aparezca antes del GREEN; * permanezca estrictamente fiel al Freeze; * la cadena obtenga un RED técnico real independiente del ADR.
           RED/GREEN:
           La primera cadena debe acreditar un RED real:
           * tests compilables; * selección >= 1; * fallo por aserción de comportamiento congelado aún no implementado; * no fallo artificial de compilación; * no test inventado que falle deliberadamente sin expresar un invariante.
           Después:
           GREEN para el mismo conjunto.
Alcance: permitido EXACTAMENTE docs/adr/0047-workspace-persistente-modeless-rackcad.md, src/RackCad.Application/Workspace/, tests/RackCad.Tests/Workspace/ (archivo exacto o prefijo terminado en /);
           prohibido docs/initiatives/, docs/automation/, src/RackCad.Plugin/, src/RackCad.UI/, src/RackCad.Domain/, tests/RackCad.UI.Tests/, .github/, assets/, eng/, deploy/, global.json, Directory.Build.props, Directory.Build.targets, RackCad.sln, src/RackCad.Application/RackCad.Application.csproj, tests/RackCad.Tests/RackCad.Tests.csproj, tools/RackCad.StructuralSections.Import/RackCad.StructuralSections.Import.csproj;
           todo lo que no este en el permitido queda fuera (incluidos docs/adr/README.md, los demas ADR y el resto de
           src/RackCad.Application/ y tests/RackCad.Tests/)
Invariantes del Freeze (vinculantes en su totalidad, aunque los criterios sean menos exhaustivos):
           - INV-F1-T1-01: Cero referencias a AutoCAD desde el modelo puro de Application.
           - INV-F1-T1-02: Una sesión representa una instancia abierta de documento, no ruta/nombre/título.
           - INV-F1-T1-03: El modelo no persiste nada.
           - INV-F1-T1-04: El modelo no escribe authored ni modela ninguna escritura authored.
           - INV-F1-T1-05: SelectionContext soporta los estados exigidos por el Freeze para F1 sin inventar RackId.
           - INV-F1-T1-06: Las pistas son hints, nunca autoridad.
           - INV-F1-T1-07: El modelo de cola/drenaje no exige ausencia de eventos durante modales.
           - INV-F1-T1-08: No existe estado por pestaña que duplique suscripciones o autoridad documental.
           - INV-F1-T1-09: La destrucción de una sesión no permite reemparejarla automáticamente con otro documento posterior.
           - INV-F1-T1-10: MASTER-I63-I64-02 permanece intacto: ninguna métrica, provider, ProjectSummary o agregación entra en este modelo.
           - INV-F1-T1-11: ADR 0047 permanece propuesto y no añade semántica.
           - INV-F1-T1-12: RACKEDITAR y comandos existentes no cambian.
Rutas y archivos calientes: ninguno existente de produccion. Todo lo nuevo vive en src/RackCad.Application/Workspace/
           (namespace RackCad.Application.Workspace), tests/RackCad.Tests/Workspace/ (namespace RackCad.Tests.Workspace,
           xunit) y el ADR. Fuente de diseno (solo lectura): docs/initiatives/I-64-proposal-v5.md §0, D-02, D-03, D-04,
           D-05 (solo lo que F1 exige de seleccion fina, sin enumeracion global), D-06, D-17 (contencion), §10, §11 (fila F1)
           y Anexo A (borrador del ADR). Formato de ADR (solo lectura): docs/adr/README.md y docs/adr/plantilla.md.
           Guardas existentes que deben seguir verdes: CantileverRoundTwoSourceGuardTests (ningun proyecto fuera del
           Plugin nombra Autodesk.).
Criterios de aceptacion (del paquete aceptado):
           - La sesión del modelo representa una instancia abierta de documento y queda destruida al destruirse esa instancia; no se reempareja automáticamente con otro documento posterior (D-03; INV-F1-T1-02, INV-F1-T1-09).
           - Las pistas se modelan solo como hints. El drenaje se permite únicamente en reposo y sin modal RackCad, coalesce pistas, y ejecuta únicamente las operaciones puras permitidas por F1 (D-04; INV-F1-T1-06, INV-F1-T1-07).
           - Se representan los contextos Ninguno, Uno, Seleccion (N), Sin identidad y Diagnostico; “sin Id” usa IsNullOrWhiteSpace y no se inventa RackId (D-05, D-06; INV-F1-T1-05).
           - No se agrega estado por pestaña que duplique suscripciones o autoridad documental (INV-F1-T1-08).
           - El modelo puro de RackCad.Application no referencia AutoCAD (INV-F1-T1-01), no persiste, no escribe ni modela escritura authored, no incorpora métricas/provider/ProjectSummary/agregación de I-63 y no modifica RACKEDITAR ni comandos existentes (INV-F1-T1-03, INV-F1-T1-04, INV-F1-T1-10, INV-F1-T1-12).
           - Las pruebas se ubican en tests/RackCad.Tests/Workspace y usan el espacio de nombres RackCad.Tests.Workspace, de modo que coincide con el filtro contractual FullyQualifiedName~RackCad.Tests.Workspace.
           - El ADR 0047 se crea en la ruta contractual, en estado propuesto, como proyección fiel del Anexo A del Freeze, sin nuevas decisiones ni semántica; está presente antes del commit GREEN (INV-F1-T1-11).
           - La fase RED usa pruebas compilables contra el esqueleto, selecciona al menos una prueba y falla por aserción de comportamiento congelado aún no implementado; no depende de error de compilación ni de fallo artificial. Después, GREEN pasa el mismo conjunto de pruebas.
           - Todos los archivos modificados están dentro de AllowedWriteScope del contrato.
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           - tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.Workspace, minimo 1, RED esperado: si;
           evidencia esperada del contrato: gate/delegation package válido; nc4 según I-61 en la primera delegación aplicable; aceptación A1-A8 antes de Worker; commit RED real; ChainRedSha / ChainRedFiles correctamente acreditados; commit GREEN; pruebas RED y GREEN con selección; worker-handoff válido; push fast-forward; CI exacta del CurrentSha verde; Controller verification válida; diff dentro de AllowedWriteScope; ADR 0047 fiel al Freeze.
           La sesion, no tu, mantiene evidencia y estado (docs/automation/ esta prohibido).
No-touch: todo lo prohibido arriba; el checkout principal D:\Documentos\Codex\Calculadora de racks (nunca escribas alli);
           AutoCAD: el acad.exe abierto en esta maquina pertenece a otra iniciativa: no lo cierres, no lo uses, no cargues DLL,
           no toques perfil, TRUSTEDPATHS, SECURELOAD, ACL ni registro; nada fuera del alcance permitido
Condiciones de parada: S-01..S-14, P-01..P-08 y C-01..C-12 del contrato:
           C-01: origin/main cambia antes de escribir
           C-02: ADR 0047 deja de estar libre
           C-03: se necesita tocar Plugin/UI
           C-04: se necesita AutoCAD
           C-05: se necesita ampliar scope
           C-06: aparece ambigüedad material del Freeze
           C-07: cambia config.toml
           C-08: hay conflicto de ownership
           C-09: la tarea deriva hacia Host/Bridge
           C-10: se intenta introducir métricas de I-63
           C-11: se intenta modificar comandos existentes
           C-12: presupuesto agotado
           S-01 y S-10 son frontera (Owner Validation, AutoCAD): no se cruzan y se anotan en KnownLimitations; ante cualquier
           otra, parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition y
           TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-64/F1-T1-MODEL/0/R20261002T005244Z-f656/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado por el Coordinator; primera delegacion de la cadena)
- Celda: claude-sonnet-5-5|subagent, effort medium (Balanced); clase "Implementación de pruebas".
  ChainBaseSha = BaseSha = e0587355b0e84d98807057a56dfc50591da87489; ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta (el directorio actual del shell
  puede volver al checkout principal). dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) solo bajo artifacts/orchestration/I-64/F1-T1-MODEL/0/R20261002T005244Z-f656/tests/<RunRef>/.
  No compiles src/RackCad.Plugin/ (no cambia; la CI lo compila sin AutoCAD).
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/workspace-persistente-rackcad = BaseSha; docs/adr/0047-* no existe. Si no, parada (S-12/S-13/C-02).
- Paso RED (un solo commit, con TODAS las pruebas de la cadena):
  1. ADR docs/adr/0047-workspace-persistente-modeless-rackcad.md en estado `propuesto`, con las secciones de la plantilla,
     como proyeccion fiel del Anexo A de la Proposal V5 congelada (sin decisiones, semantica ni opcionales nuevos; no se
     marca aceptado). No edites docs/adr/README.md (fuera de alcance): anota en KnownLimitations que su indice no lista
     el ADR 0047.
  2. Tipos del modelo puro en src/RackCad.Application/Workspace/ (sin referencias a AutoCAD, WPF ni Plugin; sin E/S ni
     persistencia) como ESQUELETO COMPILABLE: firmas completas y cuerpos que aun no cumplen el comportamiento congelado
     (p. ej. valores por defecto), nunca NotImplementedException que haga fallar todas las pruebas por excepcion.
  3. Pruebas xunit en tests/RackCad.Tests/Workspace/ (namespace RackCad.Tests.Workspace) que expresan los criterios y los
     invariantes comprobables del modelo: sesion por instancia y no reemparejo tras destruirse (identificador nativo
     reciclado -> sesion nueva y vacia; peticion de la sesion A con B activo -> rechazo sin efectos); pistas = hints que
     solo invalidan o marcan; cola coalescida y drenaje solo en reposo y sin modal RackCad, con pistas que llegan durante
     un modal o un comando y esperan; solo las operaciones de drenaje permitidas; contextos Ninguno, Uno, Seleccion (N)
     con recuento, Sin identidad y Diagnostico, con "sin Id" = string.IsNullOrWhiteSpace y sin inventar RackId; claves
     (sesion, RackId) que no cruzan sesiones; ausencia de estado por pestana que duplique suscripciones; y una guarda de
     texto sobre src/RackCad.Application/Workspace/ que falle si aparece Autodesk. o una escritura/persistencia.
     Cada prueba falla por asercion contra el esqueleto (las de guarda pueden pasar ya en RED). Nada de pruebas que
     fallen deliberadamente sin expresar un invariante.
  4. Corre el filtro del contrato: debe compilar y FALLAR POR ASERCION con seleccion > 0 (no por error de compilacion).
     Commit RED (git add solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ni anadas archivos bajo tests/ (son RT; si una prueba necesitara cambio, parada y
  bloqueo estructurado, no la edites).
- Paso GREEN:
  1. Implementa el comportamiento en src/RackCad.Application/Workspace/ hasta que el filtro quede verde; ajusta el ADR
     solo si hace falta para la fidelidad al Anexo A.
  2. Corridas GREEN del filtro (seleccion > 0, todas en verde). Relevantes: la suite Core completa
     tests/RackCad.Tests/RackCad.Tests.csproj (anota totales) y dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
     (valida la API de Application).
  3. Commit GREEN (git add solo de archivos del alcance) con este cuerpo de resumen de estado, y git push:
     "Estado: F1 en curso; entrega del Worker de F1-T1-MODEL (WorkRunId R20261002T005244Z-f656) pendiente de la verificacion del
     Controller. attempts = 0. Owner pendiente: aceptacion del ADR 0047 (propuesto) y Smoke-1 tras el cierre de F1."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que
  deje dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F1-T1-MODEL; RunId R20261002T005244Z-f656; DelegationRunId R20261002T003827Z-3a76; Initiative I-64; Gate F1; Attempt 0;
  BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y
  TestResults de cada corrida (RunRef unico, Command exacto, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null
  si el arbol tenia cambios, ResultsFile = TRX o null); Worker = { Provider "Anthropic", ModelRequested
  "claude-sonnet-5-5", EffortRequested "medium", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
