I-65 — DELEGACION F4-T1-PACKAGE-HARDENING — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F4; tarea F4-T1-PACKAGE-HARDENING; intento 0; RunId R20261011T155212Z-11d5;
           DelegationRunId R20261011T154535Z-39b9; AuthorityRevision f64014f312f3b4f849e26dcee6dd713cbace146e;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha f64014f312f3b4f849e26dcee6dd713cbace146e;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T154535Z-39b9
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f4-implementation.md (§0, §1, §2 y §7), docs/initiatives/I-65-f4-preparation.md (§5 y §6),
           docs/initiatives/I-65-f3-ram-elements-adapter.md (§7 y §8), docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes:
           D-08, D-09 reglas 1-10; §3 INV-05, 10 y 14), docs/automation/decisions/I-65.md §21 y §26,
           docs/automation/evidence/I-65-f3/architect-f3-conformance.md (A65-F3-01..12), p1-results.md y p1-run/p1-report.json
           (UNIT_DOC); ADR-0050 (UNIT_CHANGE); AGENTS.md, ADR-0017 y docs/AUTOMATION_PLAN.md §16 (EXTERNAL); vigencia:
           AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/0/R20261011T154535Z-39b9/delegation.json. En resumen: tercera puerta y
           prefijo gate, giro modulo 360, descripcion no comparada y escritura robusta de la lectura de vuelta en la plantilla;
           carpeta embebida expuesta y regla de carpeta endurecida en el paquete; prueba de la carrera del destino; deudas de
           guardas A65-F3-05 y 06.
Alcance: el del paquete (= el del contrato). Permitido: src/RackCad.Application/StructuralAnalysis/Providers/RamElements/
           (cambios en RamBuildScriptTemplate.cs y RamBuildPackageFactory.cs; aditivos en otros si hace falta),
           src/RackCad.Application/StructuralAnalysis/StructuralPackageWriter.cs (SOLO si la prueba de carrera revela un defecto),
           archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/ y las ediciones nombradas de cinco pruebas existentes.
           Prohibido el ForbiddenWriteScope del paquete (las demas pruebas y fixtures existentes; Systems/; assets/; Units/;
           StructuralSections/; otros archivos de StructuralAnalysis; Domain, Plugin, UI, tools, cualquier .csproj, docs/, etc.)
Invariantes del Freeze: los del paquete aceptado (Invariants), con docs/initiatives/I-65-f4-implementation.md §2
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/RamBuildScriptTemplate.cs (plantilla de F3: puertas en
             main(), write_model, read_back con la comparacion del giro en la linea del check de rotation y la de la descripcion,
             escritura final de la lectura de vuelta fuera del try);
           RamBuildPackageFactory.cs (RamBuildPackage, IsAbsoluteNormalisedFolder e IsNormalisedSegment);
           src/RackCad.Application/StructuralAnalysis/StructuralPackageWriter.cs (StructuralPackagePublisher y la costura
             IStructuralPackageFileSystem);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackage*Tests.cs y
             StructuralAnalysisRamElementsFixtures.cs (helpers de REF-CANT-01, tablas de fixture verified y script generado;
             solo lectura salvo StructuralAnalysisRamElementsPackageTemplateTests.cs);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs (StructuralAnalysisGuards.Inv14 y su
             lista de exencion)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisRamElementsHardening,
           minimo 20, RED esperado; suite Core completa en verde en el GREEN; commit RED y push; commit GREEN y push con el
           resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-11); en particular C-01 (editar
           una prueba existente fuera de los cinco archivos o para otro fin), C-03 (la carga util de REF-CANT-01 deja de
           reproducir el golden de RAM), C-07, C-08 (ejecutar Python o RAM Elements) y C-11; ante cualquiera, parar sin continuar
           y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/0/R20261011T155212Z-11d5/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion aceptada de la cadena F4-T1-PACKAGE-HARDENING)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = BaseSha = f64014f312f3b4f849e26dcee6dd713cbace146e;
  ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/0/R20261011T155212Z-11d5/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/0/R20261011T155212Z-11d5/work/.
  Sin AutoCAD, sin Python y sin RAM Elements (no ejecutes la plantilla). Escribe los archivos con la herramienta de escritura
  (no con heredocs de Bash: dañan las barras invertidas). La suite Core de la CI corre en ubuntu-latest: las pruebas deben
  pasar en Linux y en Windows (rutas temporales con Path.GetTempPath(), sin depender del separador del sistema; contenido
  generado con "\n" explicito, nunca Environment.NewLine).
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura de I-65-f4-implementation.md §2 aplicada (no son decisiones nuevas; si algo que necesitas decidir con efecto
  observable no esta decidido ahi ni en el Freeze, no lo inventes: parada S-03 con la pregunta exacta; nombres libres):
  - Tercera puerta (A65-F3-11): una funcion de la plantilla (p. ej. payload_failures(data)) devuelve la lista de fallos de la
    carga util: par distinto de "LB_IN"; miembro con seccion o material vacio o ausente; nodeI o nodeJ que no es un id de nudo;
    carga cuyo miembro no existe; direccion distinta de "GLOBAL_Y"; restraints que no son seis booleanos; ids de nudo, miembro
    o carga repetidos. En main(), el orden es: carga util decodificada; puerta de capacidad; si pasa, m = rm.model() y puerta de
    modelo vacio; puerta de la carga util (se puede evaluar siempre que la carga util se haya decodificado); SOLO DESPUES se
    abre ram-readback.json en modo "x"; si alguna puerta fallo, sus fallos van a differences con el prefijo "gate: " y no hay
    ninguna escritura en el modelo (ni units.set); si no, write_model y read_back. write_model deja de comprobar la carga util
    (sus raise de seccion vacia y direccion desaparecen). T4-21: todo fallo de puerta lleva el prefijo "gate: ".
  - Giro (A65-F3-01): una funcion (p. ej. angle_near(actual, expected, tolerance)) con math.isfinite, d = math.fmod(actual -
    expected, 360.0) llevado a (-180, 180] y abs(d) <= tolerance * max(1.0, abs(expected)); se usa en el check del giro en
    lugar de near(); values sigue guardando el giro leido tal cual.
  - Descripcion (A65-F3-02): se quita el check de la descripcion; members.add sigue pasando description=spec["id"] y values
    sigue guardando member.description.
  - Lectura de vuelta (A65-F3-04): text = json.dumps(...) y out.write(...) dentro de un try; si fallan, se escribe en el mismo
    archivo (si esta abierto) un respaldo minimo json con schema, status = FAILED, analysisInputFingerprint y un campo con el
    error (sin values), protegido a su vez; out.close() en finally; print del estado siempre.
  - Todo lo demas de la plantilla queda igual (D-09): mismos imports, mismas llamadas del paquete (lista del documento de F3
    §7; math.fmod es de math, ya importado), apertura exclusiva unica, categoria como texto, CENTROID, valores por defecto solo
    leidos, tolerancias de la regla 9, solo ASCII, FAILED escrito con la secuencia de escape de la raya.
  - Carpeta (A65-F3-07): propiedad publica de RamBuildPackage con la carpeta embebida (p. ej. PackageFolder), igual a
    options.PackageFolder y al packageFolder de la carga util decodificada del script.
  - Regla de carpeta (A65-F3-08): en IsNormalisedSegment (o equivalente), rechazar un segmento que termina en "." o " " y un
    segmento cuyo nombre antes del primer "." sea, sin distinguir mayusculas, CON, PRN, AUX, NUL, COM1-COM9 o LPT1-LPT9; el
    codigo de diagnostico es el existente de carpeta invalida; las rutas validas de las pruebas de F3 siguen validas.
  - Carrera (A65-F3-09): prueba con una implementacion falsa de IStructuralPackageFileSystem en la que la comprobacion de
    existencia del destino da falso y el destino aparece antes del renombrado (el renombrado falla como lo haria el sistema
    real); el resultado es fallo, el destino preexistente no se toca y no queda carpeta temporal. Si ya se cumple, el escritor
    NO cambia.
  - A65-F3-05: en StructuralAnalysisRamElementsGuardTests.cs y StructuralAnalysisRamElementsAssumptionsTests.cs, el literal
    partido se escribe entero; en StructuralAnalysisSourceGuardTests.cs, StructuralAnalysisGuards.Inv14 exime exactamente cuatro
    rutas exactas (su archivo, StructuralAnalysisRamElementsPackageTemplateTests.cs y esos dos) y la autoprueba de la exencion
    (hoy en StructuralAnalysisRamElementsGuardTests.cs, INV14_EXIME_...) comprueba las cuatro y que otra ruta no queda eximida.
  - A65-F3-06: en StructuralAnalysisModelBuilderTests.cs, la prueba NoExistenAnalyzeNiReadResultsNiEscrituraDeArchivosEnLaCapaNeutral
    deja de buscar «Writer» en los nombres de tipo y afirma, leyendo el codigo fuente de los archivos de
    src/RackCad.Application/StructuralAnalysis/ que no estan en Providers/, que File., Directory., FileStream, FileInfo y
    DirectoryInfo solo aparecen en StructuralPackageWriter.cs; sigue afirmando que no hay miembros Analyze ni ReadResults.
  - Aserciones existentes que encodan el comportamiento viejo de A65-F3-01, 02, 04 y 11 (p. ej. la del orden del except y
    out.write en StructuralAnalysisRamElementsPackageTemplateTests.cs): se editan en ese archivo para el comportamiento nuevo;
    anota cada edicion y su motivo en Evidence. Las variantes sinteticas de la guarda INV-10 siguen fallando.
- Paso RED (un solo commit): pruebas NUEVAS (namespace RackCad.Tests, clases StructuralAnalysisRamElementsHardening*), >= 20
  seleccionables, que fallan por asercion o excepcion (nunca compilacion); si hace falta un miembro nuevo para compilar (p. ej.
  la propiedad de la carpeta), se anade en el RED con un cuerpo que las pruebas rechazan; y las ediciones de las cinco pruebas
  existentes. Las pruebas nuevas no contienen los tokens del modulo del proveedor, del interprete ni de la empresa (INV-14).
  Commit RED (git add solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas del RED (si hiciera falta: parada, sin editarlo).
- Paso GREEN: implementa en el alcance; corrida GREEN del filtro (todas en verde) y suite Core completa en verde (anota
  totales); comprueba el alcance del diff y que en tests/ solo hay altas y las cinco ediciones; commit GREEN con esta linea en
  el cuerpo, en UNA sola linea, y git push:
  "Estado: F4 en curso; entrega del Worker de F4-T1-PACKAGE-HARDENING (WorkRunId R20261011T155212Z-11d5) pendiente de la verificacion del Controller. attempts = 0."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F4-T1-PACKAGE-HARDENING; RunId R20261011T155212Z-11d5; DelegationRunId R20261011T154535Z-39b9; Initiative I-65;
  Gate F4; Attempt 0; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD =
  remoto); Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y
  TestResults de cada corrida (RunRef unico, Command completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el
  arbol tenia cambios, ResultsFile = TRX); Evidence con: (1) por correccion (A65-F3-01, 02, 04, 05, 06, 07, 08, 09, 11 y
  T4-21), la prueba que la observa; (2) la lista de aserciones editadas de pruebas existentes, archivo y motivo; (3) el SHA-256
  del script generado de REF-CANT-01 con la tabla de fixture y la ruta fija de prueba, y la confirmacion de que la carga util
  sigue reproduciendo el golden de RAM; (4) si el escritor cambio o no, y por que; (5) las pruebas que pasan ya en el RED y por
  que; Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By:
  Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE
  salvo parada (una S-xx o C-xx de comportamiento STOP va con Disposition STOP); listas vacias si no hay nada.
- Tope: 60 minutos (README de ejecucion delegada). Administra el tiempo: itera con el filtro focal y corre la suite completa al
  final. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
