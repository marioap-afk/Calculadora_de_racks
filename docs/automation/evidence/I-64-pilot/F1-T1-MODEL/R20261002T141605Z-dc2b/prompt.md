I-64 — DELEGACION F1-T1-MODEL — Worker, perfil DEBUGGING (CORRECCION, intento 2)
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 2; RunId R20261002T141605Z-dc2b;
           DelegationRunId R20261002T140200Z-3886; AuthorityRevision e0587355b0e84d98807057a56dfc50591da87489;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha 1504c938dc8ac050a8e0d61c5db6c9977b638600;
           rama architecture/workspace-persistente-rackcad;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-workspace-persistente-rackcad;
           Owner subagent:R20261002T140200Z-3886
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
           Decision del Coordinator para esta correccion: docs/automation/evidence/I-64-evidence.md Sec.25 (en el HEAD)
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
           Esta delegacion es la CORRECCION del REWORK Ci de la verificacion R20261002T063546Z-0cd7.
Alcance: permitido EXACTAMENTE docs/adr/0047-workspace-persistente-modeless-rackcad.md, src/RackCad.Application/Workspace/, tests/RackCad.Tests/Workspace/ (archivo exacto o prefijo terminado en /);
           prohibido docs/initiatives/, docs/automation/, src/RackCad.Plugin/, src/RackCad.UI/, src/RackCad.Domain/, tests/RackCad.UI.Tests/, .github/, assets/, eng/, deploy/, global.json, Directory.Build.props, Directory.Build.targets, RackCad.sln, src/RackCad.Application/RackCad.Application.csproj, tests/RackCad.Tests/RackCad.Tests.csproj, tools/RackCad.StructuralSections.Import/RackCad.StructuralSections.Import.csproj;
           todo lo que no este en el permitido queda fuera (incluidos tests/RackCad.Tests/NamespaceFolderGuardTests.cs,
           docs/adr/README.md, los demas ADR y el resto de src/RackCad.Application/ y tests/RackCad.Tests/)
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
Rutas y archivos calientes: tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs (CS8632 en la linea 62);
           src/RackCad.Application/Workspace/{Hints,SelectionContext,SessionId,WorkspaceSession,WorkspaceSessionRegistry}.cs
           (implementacion del GREEN b38e54f9); eng/ci/verify-autocad-references.ps1 (solo lectura; NO se modifica);
           guardas integradas que deben quedar verdes SIN tocarlas: tests/RackCad.Tests/NamespaceFolderGuardTests.cs
           (TestProjects_KeepExactlyOneAssemblyRootNamespace exige exactamente `namespace RackCad.Tests` en tests/RackCad.Tests;
           EveryProductionFile_DeclaresExactlyOneBlockScopedNamespace exige namespaces de bloque en src/) y
           CantileverRoundTwoSourceGuardTests (ningun proyecto fuera del Plugin nombra Autodesk.).
Criterios de aceptacion (del paquete aceptado):
           - En tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs, habilitar localmente solo el contexto de anotaciones nullable mediante `#nullable enable annotations` o la forma local mínima equivalente aceptada por el compilador, y conservar la semántica de `string?`.
           - No modificar NamespaceFolderGuardTests, scripts de CI, configuración nullable global, archivos .csproj ni RackCad.Plugin; no suprimir ni debilitar avisos con #pragma warning disable o NoWarn, ni quitar cobertura.
           - Entregar un commit RED nuevo con la corrección nullable aplicada y el comportamiento congelado desactivado en src/RackCad.Application/Workspace/; compila y falla por una aserción real con al menos una prueba seleccionada, sin fallo ni aviso artificial.
           - El commit GREEN restaura el comportamiento y no modifica archivos de RT ni de ChainRedFiles.
           - En GREEN, pasan el filtro del contrato y la suite Core completa.
           - La build Release no emite CS8632 ni avisos nuevos CS/MSB/NU/NETSDK. Antes del push final, ejecutar localmente eng/ci/verify-autocad-references.ps1 con rutas aisladas como en .github/workflows/ci.yml, sin requerir AutoCAD. Si causas de entorno ajenas al código impiden ejecutar el script, registrarlo en KnownLimitations y comprobar al menos la build Release del proyecto de pruebas sin avisos CS en los archivos de Workspace.
           - La ejecución de CI de push para el SHA exacto del Worker tiene en success los cuatro jobs requeridos: Tests (Domain + Application), UI Tests (WPF controls, net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD.
           - docs/adr/0047-workspace-persistente-modeless-rackcad.md no cambia y todos los archivos modificados permanecen dentro de AllowedWriteScope.
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
           C-02 se refiere a que OTRA unidad ocupe el numero 0047; el archivo existe en esta rama desde 36c344dc (esta cadena).
           S-01 y S-10 son frontera (Owner Validation, AutoCAD): no se cruzan y se anotan en KnownLimitations; ante cualquier
           otra, parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition y
           TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-64/F1-T1-MODEL/2/R20261002T141605Z-dc2b/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL DEBUGGING
Metodo: reproducir, aislar, explicar y solo entonces corregir.
1. Reproduce el fallo con una prueba o una orden; registra la salida.
2. Formula la causa con evidencia (lineas, valores, commits), no con suposiciones.
3. Si corriges: prueba de regresion vista fallando antes del arreglo, y despues verde.
4. Si la causa queda incierta: entrega PARTIAL con lo medido y las hipotesis
   descartadas; no fuerces un arreglo.

DELTA DE LA TAREA (paquete de correccion attempt 2 aceptado por el Coordinator; regla RED de la aceptacion)
- Celda: claude-sonnet-5-5|subagent, effort medium (Balanced); clase "Depuración".
  ChainBaseSha = e0587355b0e84d98807057a56dfc50591da87489; ChainRedSha (al emitir) = 189353f8df954b05cbbcdc0f15dca0cb3bf21002;
  ChainRedFiles = tests/RackCad.Tests/Workspace/SelectionContextTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs, tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs.
  WorkspaceSelectionContextTests.cs pertenece a ChainRedFiles: la entrega EXIGE UN RED NUEVO (AUTOMATION_PLAN 16.8).
- Causa (Coordinator, evidencia Sec.25): la build Release del job "Build Plugin without AutoCAD" rechaza CS8632 en
  tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs(62,75): `string? id` sin contexto de anotaciones nullable.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) solo bajo artifacts/orchestration/I-64/F1-T1-MODEL/2/R20261002T141605Z-dc2b/tests/<RunRef>/.
  No compiles src/RackCad.Plugin/ (no cambia; sus referencias apuntan a la instalacion de AutoCAD de esta maquina, que no
  se toca; la CI lo compila contra paquetes NuGet). Escribe archivos con la herramienta de edicion, no con heredocs largos.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/workspace-persistente-rackcad = BaseSha. Si no, parada (S-12/S-13). Reproduce primero el aviso: build Release del
  proyecto de pruebas (abajo) y registra la linea CS8632 (Phase RELEVANT).
- Paso RED NUEVO (un solo commit con el cambio de la prueba y el comportamiento desactivado):
  1. En tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs anade, como primera linea del archivo, la directiva
     local `#nullable enable annotations` (C# 8+; si el compilador del repositorio no la aceptara, la forma local minima
     equivalente que conserve `string?`). Nada mas cambia en ese archivo: se conservan `namespace RackCad.Tests`, los nombres
     Workspace...Tests y todas las pruebas. Sin #pragma warning disable, sin NoWarn, sin tocar .csproj, CI, Plugin ni
     NamespaceFolderGuardTests. Las otras 3 pruebas de la cadena no cambian.
  2. Comportamiento desactivado: en src/RackCad.Application/Workspace/ quita el comportamiento congelado (cuerpos que no lo
     cumplen, como los del RED 189353f8) conservando firmas, tipos y namespaces DE BLOQUE; sin NotImplementedException
     masivas, sin errores de compilacion y sin avisos nuevos.
  3. Corre el filtro del contrato sobre el arbol del RED: compila y FALLA POR ASERCION del comportamiento congelado con
     seleccion > 0; las guardas (incluida NamespaceFolderGuardTests) pasan. Nada de prueba que falle incondicionalmente ni
     aviso artificial. Commit RED (git add solo de archivos del alcance) y git push; vuelve a correr el filtro sobre el
     commit (TreeSha = RedSha).
- Desde el commit RED NO modifiques ni anadas archivos bajo tests/ (RT y ChainRedFiles); si una prueba necesitara
  cambio, parada y bloqueo estructurado.
- Paso GREEN:
  1. Restaura el comportamiento en src/RackCad.Application/Workspace/ (la implementacion de b38e54f9 es la referencia)
     hasta que el filtro quede verde. No toques el ADR 0047.
  2. Commit GREEN local (git add solo de archivos del alcance) con este cuerpo de resumen de estado:
     "Estado: F1 en curso; entrega del Worker de la correccion 2 de F1-T1-MODEL (WorkRunId R20261002T141605Z-dc2b, intento 2)
     pendiente de la verificacion del Controller. attempts = 2. Owner pendiente: aceptacion del ADR 0047 (propuesto) y
     Smoke-1 tras el cierre de F1."
  3. ANTES del push del GREEN, sobre el commit GREEN (TreeSha = CurrentSha, arbol limpio):
     a) el filtro del contrato (todas en verde) y la suite Core completa tests/RackCad.Tests/RackCad.Tests.csproj (0 fallidas);
     b) comprobacion de CI sin avisos: ejecuta una vez eng/ci/verify-autocad-references.ps1 con pwsh 7 tal como en
        .github/workflows/ci.yml (NuGet.Config temporal solo con nuget.org, rutas aisladas absolutas FUERA del repositorio,
        p. ej. bajo %TEMP%, y DOTNET_ROOT = C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet con ese dotnet
        delante en el PATH). En esta maquina se espera que pare en Assert-CleanRunner porque AutoCAD 2025 esta instalado
        (causa de entorno): NO modifiques el script ni intentes saltarte esa comprobacion; registra el resultado exacto en
        KnownLimitations;
     c) comprobacion equivalente sin avisos (la semantica de Assert-DotNetSuccess -NoWarnings): dotnet build -c Release
        --no-incremental de tests/RackCad.Tests/RackCad.Tests.csproj y de tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj; la
        salida debe tener 0 lineas que casen con la expresion `\bwarning\s+(MSB|CS|NU|NETSDK)\d+\b` y 0 errores; registra
        cada build en TestsExecuted como Phase RELEVANT con el conteo.
     Si a), b) o c) no cumplen por causa del codigo, no hagas push del GREEN: parada y bloqueo estructurado.
  4. git push del GREEN.
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que
  deje dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas. No abras ni toques AutoCAD.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F1-T1-MODEL; RunId R20261002T141605Z-dc2b; DelegationRunId R20261002T140200Z-3886; Initiative I-64; Gate F1; Attempt 2;
  BaseSha = el de arriba; RedSha = el SHA del commit RED NUEVO; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y
  TestResults de cada corrida (RunRef unico, Command exacto, Phase RED/GREEN/RELEVANT/MUTATION, TreeSha = commit probado o
  null si el arbol tenia cambios, ResultsFile = TRX o null; FailedTests con los nombres reales); Worker = { Provider
  "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "medium", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
