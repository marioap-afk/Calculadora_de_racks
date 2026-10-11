I-65 — DELEGACION F4-T1-PACKAGE-HARDENING (correccion, intento 1) — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F4; tarea F4-T1-PACKAGE-HARDENING; intento 1; RunId R20261011T163420Z-5bee;
           DelegationRunId R20261011T163137Z-5315; AuthorityRevision f64014f312f3b4f849e26dcee6dd713cbace146e;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha ab2a0fe249f065af0403ce78a25cb17e0826b806;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T163137Z-5315
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), leidas en AuthorityRevision para la unidad y
           en MainSha para lo demas; el analisis de la correccion se cita por CorrectionOf (no es autoridad):
           docs/automation/evidence/I-65-pilot/F4-T1-PACKAGE-HARDENING/R20261011T162337Z-4794/analysis.md
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/1/R20261011T163137Z-5315/delegation.json. En resumen: corregir SOLO el patron de
           la asercion defectuosa de LosDosLiteralesQueEstabanPartidosSeEscribenEnteros y anadir su autoprueba sintetica.
Alcance: el del paquete (= el del contrato), y DENTRO de el la correccion se limita a UN archivo:
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningGuardTests.cs.
           Ningun otro archivo (ni de produccion ni de pruebas); ningun archivo de docs/ ni de docs/automation/.
Invariantes del Freeze: los del paquete aceptado (Invariants)
Rutas y archivos calientes (solo lectura salvo ese archivo):
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningGuardTests.cs (la prueba a corregir);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsGuardTests.cs y
           StructuralAnalysisRamElementsAssumptionsTests.cs (lo que la prueba observa; NO se tocan)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisRamElementsHardening
           (seleccion completa en verde) y suite Core completa en verde; un solo commit y push; worker-handoff.json valido en la
           ruta esperada
No-touch: todo salvo ese archivo; el estado y la evidencia de la unidad (docs/automation/); el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-11); en particular, si la correccion
           exigiera tocar otro archivo o debilitar lo que la prueba observa: parada sin continuar y bloqueo estructurado en la
           entrega (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/1/R20261011T163420Z-5bee/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee el archivo a corregir y los que observa antes de editar.
2. Esta correccion NO lleva commit RED (disposicion del Coordinator DEV-I65-F4-02, decisiones I-65 §27): un unico commit.
3. Ejecuta las pruebas requeridas y la suite Core completa; anota comando, seleccion y resultado.
4. Commit con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; correccion de la cadena F4-T1-PACKAGE-HARDENING, intento 1)
- Celda: la del paquete aceptado. ChainBaseSha f64014f312f3b4f849e26dcee6dd713cbace146e; ChainRedSha
  53bfcd94a99f4d6aa201596349d90427ca9f4229 (RED vigente, acreditado); ChainRedFiles = las nueve rutas del paquete.
  CorrectionOf = R20261011T162337Z-4794 (FailureClass Ci; analisis 2aedd7d0...).
- Defecto: la asercion de LosDosLiteralesQueEstabanPartidosSeEscribenEnteros exige el token entre comillas, pero en
  StructuralAnalysisRamElementsGuardTests.cs el literal entero esta DENTRO de una cadena C# y aparece con la comilla escapada (una
  barra invertida antes de la comilla). El codigo observado es correcto; solo el patron falla.
- Correccion: (1) el patron admite una barra invertida opcional antes de cada comilla que rodea el token, y sigue exigiendo el token
  ENTERO (no lo debilites: debe seguir sin coincidir si el token esta partido en dos literales concatenados); (2) en ese mismo archivo,
  una autoprueba del patron con entradas sinteticas: token partido en dos literales concatenados -> no coincide; token entero entre
  comillas -> coincide; token entero con comillas escapadas -> coincide. Las autopruebas no deben introducir el token del modulo del
  proveedor, del interprete ni de la empresa como literal entero fuera de lo que el archivo ya hacia (INV-14): construyelo como lo
  construye la prueba existente.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/1/R20261011T163420Z-5bee/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/1/R20261011T163420Z-5bee/work/.
  PROHIBIDO invocar un interprete Python por cualquier motivo (ni para comprobar si existe; usa pwsh si necesitas un script);
  sin AutoCAD y sin RAM Elements. Escribe los archivos con la herramienta de edicion (no con heredocs de Bash: danan las barras
  invertidas). La suite Core corre en ubuntu-latest en la CI.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Un solo commit (git add solo de ese archivo) con esta linea en el cuerpo, en UNA sola linea, y git push:
  "Estado: F4 en curso; correccion del Worker de F4-T1-PACKAGE-HARDENING (WorkRunId R20261011T163420Z-5bee, intento 1) pendiente de la verificacion del Controller. attempts = 1."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main. No esperes a la CI: el Principal recoge la
  corrida push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F4-T1-PACKAGE-HARDENING; RunId R20261011T163420Z-5bee; DelegationRunId R20261011T163137Z-5315; Initiative I-65; Gate F4; Attempt 1;
  BaseSha = el de arriba; RedSha = null (sin commit RED, DEV-I65-F4-02); CurrentSha = el SHA del commit (= HEAD = remoto); Branch y
  Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha (debe ser solo ese archivo); TestsExecuted y
  TestResults de cada corrida (RunRef unico, Command completo, Phase GREEN/RELEVANT, TreeSha = commit probado o null si el arbol
  tenia cambios, ResultsFile = TRX); Evidence con: (1) el patron viejo y el nuevo, y por que el nuevo sigue exigiendo el token entero;
  (2) las autopruebas sinteticas y su resultado; (3) los totales de la seleccion y de la suite Core completa; Worker = { Provider
  "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "medium", Trailer "Co-Authored-By: Claude Sonnet 5.5
  <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias
  si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus y CurrentSha.
