I-65 — DELEGACION F1-T1-NEUTRAL-MODEL — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F1; tarea F1-T1-NEUTRAL-MODEL; intento 0; RunId R20261011T032758Z-3556;
           DelegationRunId R20261011T032206Z-7516; AuthorityRevision 9d0c661968542350929af267ada689b943898206;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha 9d0c661968542350929af267ada689b943898206;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T032206Z-7516
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes): §2 D-01, D-02, D-03, D-06, D-07, D-08, D-10, D-10b, D-12 y D-13,
           y §3 filas INV-01, 02, 03, 04, 05, 11, 14, 15 y 16; docs/automation/decisions/I-65.md §9 (orden del Owner), §10 y §11
           (CD-I65-F1-01..10) (UNIT_DOC, en AuthorityRevision); ADR-0050 (UNIT_CHANGE); AGENTS.md «Convenciones
           arquitectonicas», ADR-0005, ADR-0017, ADR-0021 (punto 10), ADR-0040 D3 y docs/AUTOMATION_PLAN.md §16 (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL/0/R20261011T032206Z-7516/delegation.json. En resumen: propietarios
           MassUnits y ForceUnits; StructuralSectionUnits identico bit a bit; en src/RackCad.Application/StructuralAnalysis/
           (fuera de Providers/) cantidades tipadas, modelo neutral D-02, resultado y diagnosticos D-08, lector estricto del
           archivo de cargas D-10b, perfil explicito de material D-07, resolucion de secciones, BuildStructuralModel D-13 con
           fuente neutral (solo un doble de prueba en esta tarea), forma canonica y huellas D-12; guardas G1-G4, INV-11, 14 y 16.
Alcance: el del paquete (= el del contrato). Permitido: src/RackCad.Application/StructuralAnalysis/,
           src/RackCad.Application/Units/MassUnits.cs, src/RackCad.Application/Units/ForceUnits.cs,
           src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs y archivos NUEVOS de
           tests/RackCad.Tests/StructuralAnalysis/. Prohibido el ForbiddenWriteScope del paquete (StructuralAnalysis/Providers/,
           Systems/, LengthUnits.cs, cualquier .csproj, Domain, Plugin, UI, las suites protegidas, docs/, etc.). Ninguna prueba
           existente se modifica, borra ni renombra: si hiciera falta, parada (C-01)
Invariantes del Freeze: los del paquete aceptado (Invariants), con las concreciones CD-I65-F1-04..10
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/Units/LengthUnits.cs (autoridad de longitud: MillimetersPerInch, InchesPerFoot, ToInches);
           src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs (linea 26: el literal 0.45359237d que pasa a
             MassUnits; conserva el alias de LengthUnits que exige LengthUnitsAuthorityGuardTests);
           src/RackCad.Application/StructuralSections/ (StructuralSectionCatalog.TryGetById, StructuralSectionDefinition con
             NativeUnitSystem, Properties y MaterialGrade, StructuralSectionProperties Area/Ix/Iy/J, StructuralSectionFamily,
             StructuralSectionId): catalogo neutral, solo lectura;
           tests/RackCad.Tests/LengthUnitsAuthorityGuardTests.cs (modelo de guarda: barrida de src/, codigo sin comentarios,
             autoprueba con InlineData; RepoRoot de ExpressionSemanticTestSupport); tests/RackCad.Tests/NamespaceFolderGuardTests.cs
             (namespace RackCad.Tests en pruebas; en produccion namespace = carpeta y declaracion con bloque);
           tests/RackCad.Tests/CantileverSourceGuardTests.cs y StructuralSectionModelTests.cs (deben seguir en verde)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysis,
           minimo 40, RED esperado; suite Core completa en verde en el GREEN; commit RED y push; commit GREEN y push con el
           resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10); en particular C-01 (tocar
           algo fuera del alcance o una prueba existente), C-02 (una decision congelada o CD-I65-F1-04..10 no implementable tal
           como esta), C-03 (StructuralSectionUnits no queda identico bit a bit o cambia su API publica), C-06 (un INV sin RED
           observable), C-07 (inferir material, grado o propiedad mecanica, o exponer un modelo parcial), C-08 (dependencia
           binaria, NuGet, Python o RAM Elements), C-09 (habria que debilitar una guarda existente) y C-10 (cuestion material
           de arquitectura, autoridad o semantica congelada); ante cualquiera, parar sin continuar y devolver el bloqueo
           estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL/0/R20261011T032758Z-3556/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion de la cadena F1-T1-NEUTRAL-MODEL)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = BaseSha = 9d0c661968542350929af267ada689b943898206;
  ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL/0/R20261011T032758Z-3556/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL/0/R20261011T032758Z-3556/work/.
  Sin AutoCAD, sin Python y sin RAM Elements.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura del Freeze aplicada a F1-T1 (no son decisiones nuevas; si el Freeze y CD-I65-F1-04..10 no deciden algo que necesitas
  decidir con efecto observable, no lo inventes: parada S-03 con la pregunta exacta; los nombres de tipos son libres):
  - Unidades (D-03, CD-I65-F1-09): MassUnits declara la libra (0.45359237 kg) y ForceUnits la gravedad estandar (9.80665), la
    tabla cerrada de unidades de fuerza N, kN, lbf, kip y kgf con simbolos ordinales exactos y la UNICA conversion a lbf
    (factores derivados por multiplicacion de los propietarios, sin otros literales). StructuralSectionUnits sustituye solo su
    literal 0.45359237d por el miembro de MassUnits (constante, para que el valor quede identico bit a bit) y no cambia nada
    mas de su API. Las longitudes de entrada en mm pasan por LengthUnits.Authority.ToInches.
  - Cantidades: longitud (in), fuerza (lbf), area (in2) y momento de inercia (in4) como tipos sin constructor publico desde
    double sin unidad y sin operadores de conversion desde double; fabricas con unidad explicita.
  - Modelo (D-02): inmutable; nudos y miembros con id neutral determinista; un solo caso REF1 de categoria neutral Other sin
    peso propio; cargas puntuales en miembro, direccion global -Z, posicion = distancia absoluta desde el nudo I dentro de
    [0, longitud]; liberaciones (10 banderas locales) y apoyos (6 banderas globales) con id de supuesto; cualquier liberacion
    verdadera -> Unsupported. Mas de 24 cargas en un miembro -> Unsupported.
  - Resultado (D-08, CD-I65-F1-04): Valid | Unsupported | Invalid | Unavailable, precedencia Unavailable > Invalid >
    Unsupported > Valid; diagnostico = codigo estable, severidad (bloqueante o no), resultado implicado y sujeto; solo Valid
    expone el modelo.
  - Cargas (D-10b, CD-I65-F1-05): lector estricto de rackcad-structural-loads/v1 con la forma EXACTA de CD-I65-F1-05 (System.Text.Json
    de la BCL; detecta propiedades duplicadas y tipos erroneos de forma explicita); el destino {level, side} lo resuelve la
    fuente (en esta tarea, el doble de prueba); el SHA-256 de los bytes del archivo entra en la Metadata y en la huella.
  - Secciones y material (D-06, D-07, CD-I65-F1-06/07): secciones por StructuralSectionId contra el catalogo neutral pasado
    como entrada; material SOLO por el perfil explicito de filas verificadas; nunca MaterialGrade ni familia, designacion o
    nombre. En el producto no hay filas: las de las pruebas son fixtures declarados como tales.
  - Huellas (D-12, CD-I65-F1-10): SourceFingerprint desde bytes del Design + RackId + Kind + indice de estacion;
    AnalysisInputFingerprint con la version del adaptador como entrada; forma canonica determinista. Prueba de sensibilidad
    por cada componente.
  - Fuente (D-13): abstraccion neutral que, dado el alcance y las cargas leidas, describe nudos, miembros (con
    StructuralSectionId, vector de peralte, rol y pieza de origen), apoyos, liberaciones, supuestos y cargas ya colocadas, o
    devuelve diagnosticos; en esta tarea la implementa solo un doble en las pruebas. La fuente Cantilever es F1-T2.
  - Guardas (CD-I65-F1-08): G1 (lista de permiso vacia), G2, G3, G4, INV-11, INV-14 (excluye el archivo de la propia guarda)
    e INV-16, cada una con autoprueba sobre codigo sintetico; barridas sobre el arbol real con la raiz por RackCad.sln.
- Paso RED (un solo commit):
  1. Esqueleto COMPILABLE de producto (namespace = carpeta, con bloque): tipos y firmas definitivos con cuerpos que lanzan
     NotImplementedException o devuelven un valor que las pruebas rechazan; MassUnits y ForceUnits con sus constantes;
     StructuralSectionUnits SIN cambiar todavia (asi la guarda de INV-16 falla en el RED).
  2. Pruebas en archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/, namespace RackCad.Tests, clases cuyo nombre
     empieza por StructuralAnalysis (no StructuralAnalysisCantilever), >= 40 pruebas seleccionables que cubren todos los
     invariantes del paquete.
  3. Corre el filtro del contrato: compila, selecciona >= 40 y FALLA (asercion o excepcion; nunca compilacion). Las
     autopruebas de las guardas sobre codigo sintetico pueden pasar ya en el RED: registralo en Evidence. Commit RED (git add
     solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas del RED (si una prueba necesitara cambio: parada, sin editarla).
- Paso GREEN:
  1. Implementa en el alcance.
  2. Corrida GREEN del filtro (todas en verde) y suite Core completa en verde (anota totales; incluye
     StructuralSectionModelTests, LengthUnitsAuthorityGuardTests, NamespaceFolderGuardTests y CantileverSourceGuardTests).
  3. Comprueba: git diff --name-only BaseSha..HEAD dentro del alcance permitido y fuera del prohibido; git diff --name-status
     BaseSha..HEAD -- tests/ solo con altas (A) bajo tests/RackCad.Tests/StructuralAnalysis/.
  4. Commit GREEN (git add solo de archivos del alcance) con esta linea en el cuerpo, en UNA sola linea, y git push:
     "Estado: F1 en curso; entrega del Worker de F1-T1-NEUTRAL-MODEL (WorkRunId R20261011T032758Z-3556) pendiente de la verificacion del Controller. attempts = 0."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F1-T1-NEUTRAL-MODEL; RunId R20261011T032758Z-3556; DelegationRunId R20261011T032206Z-7516; Initiative I-65; Gate F1;
  Attempt 0; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults
  de cada corrida (RunRef unico, Command completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol
  tenia cambios, ResultsFile = TRX); Evidence con: (1) por invariante, la prueba que lo observa; (2) la igualdad bit a bit de
  StructuralSectionUnits; (3) la forma del archivo de cargas y del perfil de material; (4) las pruebas que pasan ya en el RED
  y por que; Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "high", Trailer
  "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED;
  Disposition NONE salvo parada (una S-xx o C-xx de comportamiento STOP va con Disposition STOP); listas vacias si no hay nada.
- Tope: 60 minutos (README de ejecucion delegada). Administra el tiempo: itera con el filtro focal y corre la suite completa al
  final. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
