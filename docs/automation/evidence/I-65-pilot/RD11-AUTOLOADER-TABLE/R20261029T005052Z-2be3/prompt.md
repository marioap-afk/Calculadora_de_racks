I-65 — DELEGACION RD11-AUTOLOADER-TABLE — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate R-D11; tarea RD11-AUTOLOADER-TABLE; intento 7; RunId R20261029T005052Z-2be3;
           DelegationRunId R20261029T004516Z-bf20; AuthorityRevision 91f82d6d83731f48d445c7ad007a814b65c6caa6;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha 91f82d6d83731f48d445c7ad007a814b65c6caa6;
           rama architecture/interop-analisis-estructural;
           worktree C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural;
           Owner subagent:R20261029T004516Z-bf20
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-rd11-autoloader-gate.md (secciones 1, 2 y 7) y docs/automation/decisions/I-65.md (§105) (UNIT_DOC);
           ADR-0050 (UNIT_CHANGE); ADR-0003, AGENTS.md y docs/AUTOMATION_PLAN.md §16 (EXTERNAL); vigencia: AuthorityRevision para
           la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/R20261029T004516Z-bf20/delegation.json (ruta relativa al worktree; leela con su
           ruta ABSOLUTA). En resumen: que la tabla aprobada de secciones y materiales viaje en el bundle del Autoloader en
           Contents/structural-analysis/ram-elements/section-material-map.json y que el verificador fail-closed la admita solo
           ahi y con identidad exacta con la fuente.
Alcance: el del paquete (= el del contrato): RED = tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandGuardTests.cs
           (solo la prueba de distribucion) y deploy/test-verify-bundle.ps1; GREEN = src/RackCad.Plugin/RackCad.Plugin.csproj (solo el
           target AssembleAutoloaderBundle y su comentario), deploy/verify-bundle.ps1, README.md, docs/guias/interoperabilidad-ram-elements.md
           y docs/guias/despliegue.md. Prohibido el ForbiddenWriteScope del paquete y cualquier otro cambio: ningun .cs de src/, la tabla,
           assets/, tools/, eng/, .github/, build-bundle.ps1, install-bundle.ps1, test-install-bundle.ps1, PackageContents.template.xml,
           otras pruebas ni otros documentos.
Invariantes del Freeze: los del paquete aceptado (Invariants), con el contrato del gate seccion 2
Rutas y archivos calientes (solo lectura salvo lo indicado): deploy/build-bundle.ps1 (publica y verifica; solo lectura),
           src/RackCad.Plugin/RackRamElementsCommands.cs (busca la tabla junto al DLL del Plugin; solo lectura),
           eng/ci/verify-autocad-references.ps1 (la CI publica y ejecuta el verificador y el harness; solo lectura)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysis,
           minimo 1795, RED esperado; suite Core completa en verde y solucion en Release sin advertencias nuevas en el GREEN; bundle
           publicado y harness en RED (falla) y en GREEN (pasa); commit RED y push; commit GREEN y push con el resumen de estado;
           worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:/Documentos/Codex/Calculadora de racks (nunca escribas ni leas alli; es el directorio de proceso de la sesion, no el tuyo)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-12); ante cualquiera, parar sin
           continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y
           TriggeredStopConditions). No hay correcciones autorizadas: un fallo que exigiria otra entrega es una parada.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/R20261029T005052Z-2be3/worker-handoff.json (dentro del worktree);
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion de la cadena RD11-AUTOLOADER-TABLE)
- Cadena: ChainBaseSha = 91f82d6d83731f48d445c7ad007a814b65c6caa6; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null. La entrega EXIGE RED.
- CONTROLES DEL OWNER (decisiones §41, CD-I65-F4-32; condicion C-12). Tu directorio de proceso NO es el worktree: es el checkout
  principal del Owner (D:/Documentos/Codex/Calculadora de racks), zona prohibida. Por eso:
  1. PRIMERA ACCION, antes de leer o escribir nada: informa pwd (Bash), y en PowerShell Get-Location y [Environment]::CurrentDirectory;
     anotalos tal cual en la entrega (Evidence) aunque no sean el worktree.
  2. Toda operacion sobre archivos con ruta ABSOLUTA bajo C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural
     (herramientas de lectura, escritura y edicion, cp y cualquier API .NET); git SIEMPRE con
     git -C C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural ...; dotnet y pwsh -File con la ruta
     absoluta del proyecto, de la solucion o del script. NUNCA una ruta relativa en una API de archivos ni dependas de cd o
     Set-Location (no empieces llamadas con cd; cada llamada de Bash vuelve a empezar en el checkout principal). Si alguna vez
     escribes fuera del worktree, para (C-12).
  3. NINGUN archivo temporal fuera de .../artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/R20261029T005052Z-2be3/work/, salvo la carpeta temporal
     que deploy/test-verify-bundle.ps1 crea y borra por si mismo. Ninguna sonda compilada en tests/. Edita los archivos SOLO con las
     herramientas de edicion (nada de heredocs de Bash ni de Set-Content sobre archivos del repositorio).
  La sesion fotografia el checkout principal y los demas worktrees antes y despues de tu trabajo: cualquier cambio alli es una parada.
- Entorno: Windows. dotnet: usa SIEMPRE /c/Users/<usuario>/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\<usuario>\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve. pwsh (PowerShell 7)
  para los scripts de deploy/. Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural/artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/R20261029T005052Z-2be3/tests/<RunRef>;
  anota en Command la orden COMPLETA. PROHIBIDO invocar un interprete Python por cualquier motivo, en ninguna forma (python, python3,
  py, ni siquiera --version ni dentro de otra orden: parada C-08). NO compruebes la disponibilidad de ninguna herramienta (ninguna
  orden «<herramienta> --version» salvo git y dotnet); sin AutoCAD, sin RAM Elements y sin install-bundle.ps1. La suite Core de la CI
  corre en ubuntu-latest: las pruebas C# deben pasar en Linux y en Windows (rutas con Path.Combine o normalizando separadores, sin
  rutas de Windows absolutas, leyendo solo archivos del repositorio).
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13. La tabla en BaseSha tiene el blob
  d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2 y NO se toca nunca.
- Lee antes de editar: src/RackCad.Plugin/RackCad.Plugin.csproj (target AssembleAutoloaderBundle y elemento Content de la tabla),
  deploy/verify-bundle.ps1, deploy/test-verify-bundle.ps1, deploy/build-bundle.ps1 y la region
  «the table travels with the Plugin build, not with the bundle» de StructuralAnalysisPluginCommandGuardTests.cs.

- PASO RED (un solo commit; SOLO estos dos archivos):
  (a) tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandGuardTests.cs: sustituye SOLO el metodo
      LaTablaNoViajaEnElPaqueteDelAutoloader_NiElDestinoDelPaqueteSeToca por tres pruebas [Fact] (puedes cambiar el comentario
      separador de la region para decir que la tabla viaja con la compilacion y con el bundle; nada mas del archivo cambia; reutiliza
      RepoRoot() y TableContentElements()):
      1. ElTargetDelBundleCopiaLaTablaDesdeSuFuenteALaMismaRutaRelativaQueLaCompilacion: carga el proyecto con XDocument; dentro del
         elemento Target con Name="AssembleAutoloaderBundle":
         - la propiedad BundleContents sigue siendo exactamente "$(BundleRoot)\Contents";
         - hay EXACTAMENTE un elemento hijo de un ItemGroup del target cuyo atributo Include, con '\' cambiado por '/', es exactamente
           "$(MSBuildProjectDirectory)/../../assets/structural-analysis/ram-elements/section-material-map.json" (guarda su nombre de
           elemento, p. ej. BundleSectionMapFile);
         - hay EXACTAMENTE un elemento Copy del target cuyo SourceFiles es "@(" + ese nombre + ")", y su DestinationFolder, con '\'
           cambiado por '/', es exactamente "$(BundleContents)/structural-analysis/ram-elements";
         - la parte de DestinationFolder despues de "$(BundleContents)/", mas "/section-material-map.json", es igual al atributo Link
           del unico elemento Content de la tabla (TableContentElements()), es decir "structural-analysis/ram-elements/section-material-map.json";
         - los elementos BundleAssemblies siguen nombrando exactamente los cuatro DLL de RackCad (Plugin, Application, Domain, UI).
      2. ElVerificadorDelBundleAdmiteSoloLaRutaDeLaTablaYExigeSuIdentidadConLaFuente: lee deploy/verify-bundle.ps1 y afirma que contiene
         la cadena exacta "Contents/structural-analysis/ram-elements/section-material-map.json", el parametro "SectionMapSourcePath",
         la cadena de la fuente por omision "assets\structural-analysis\ram-elements\section-material-map.json" y los textos de mensaje
         "Falta la tabla de secciones y materiales en el bundle" y "Tabla de secciones y materiales modificada respecto a la fuente
         aprobada (SHA-256)"; y que sigue conteniendo los mensajes existentes "Archivo no autorizado en el bundle", "Archivo Autodesk
         prohibido en el bundle" y "Catalogo modificado respecto a assets/catalogs".
      3. ElHarnessDelVerificadorCubreLaTablaConElValidoYCuatroNegativos: lee deploy/test-verify-bundle.ps1 y afirma que contiene los
         nombres de caso "tabla-ausente", "tabla-modificada", "tabla-ruta-no-autorizada" y "tabla-archivo-extra" y los textos esperados
         "Falta la tabla de secciones y materiales en el bundle" y "Tabla de secciones y materiales modificada respecto a la fuente
         aprobada", y que conserva los diez casos actuales ("valido", "dll-autodesk", "dll-modificado", "catalogo-extra",
         "catalogo-ausente", "catalogo-modificado", "catalogo-subcarpeta", "archivo-extrano", "version-incorrecta", "series-incorrecta").
      Las pruebas 1 y 2 fallan en RED por asercion (el target y el verificador aun no tienen la tabla); la 3 pasa ya en RED (el harness
      cambia en el mismo commit): dilo en Evidence. Ninguna prueba ejecuta pwsh, dotnet ni procesos: solo leen archivos del repositorio.
  (b) deploy/test-verify-bundle.ps1 (sin tocar nada de lo existente salvo la lista del comentario y el resumen final):
      - en el caso 1 ("valido"), ANTES de Assert-Pass: si la copia no tiene el archivo
        Contents\structural-analysis\ram-elements\section-material-map.json, throw "FALLO [valido]: el bundle fuente no contiene la tabla
        de secciones y materiales en Contents/structural-analysis/ram-elements/section-material-map.json" (es el fallo esperado en RED);
      - cuatro casos nuevos al final, cada uno sobre su propia copia (New-BundleCopy):
        11 "tabla-ausente": borra la tabla de la copia -> Assert-Reject con ExpectSubstring "Falta la tabla de secciones y materiales en el bundle";
        12 "tabla-modificada": Add-FileBytes sobre la tabla de la copia -> Assert-Reject con "Tabla de secciones y materiales modificada respecto a la fuente aprobada";
        13 "tabla-ruta-no-autorizada": COPIA (sin moverla) la tabla a Contents\section-material-map.json -> Assert-Reject con "Archivo no autorizado en el bundle";
        14 "tabla-archivo-extra": crea Contents\structural-analysis\ram-elements\otra.json con "{}" -> Assert-Reject con "Archivo no autorizado en el bundle";
      - actualiza la lista de casos del comentario y el resumen final a "1 valido + 13 negativos".
  Comprobacion local del RED (con AutoCAD CERRADO; si esta abierto, parada C-11 sin cerrarlo):
      pwsh -NoProfile -File C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural/deploy/build-bundle.ps1 -Configuration Release -Output C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural/artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/R20261029T005052Z-2be3/work/publish-red
      (debe terminar bien: el target y el verificador aun son los de BaseSha), y despues
      pwsh -NoProfile -File C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural/deploy/test-verify-bundle.ps1 -SourceBundle <publish-red>/RackCad.bundle -PublishDir <publish-red>
      (debe FALLAR en el caso "valido" con el mensaje de arriba); corre el filtro del contrato (las pruebas 1 y 2 fallan por asercion,
      seleccion >= 1795, sin errores de compilacion). Commit RED (git add solo de esos dos archivos) y git push.
- Desde el commit RED NO modifiques la prueba ni el harness (si hiciera falta: parada, sin editarlos).

- PASO GREEN (un solo commit; SOLO estos archivos):
  (c) src/RackCad.Plugin/RackCad.Plugin.csproj, dentro del target AssembleAutoloaderBundle y nada fuera de el (salvo su comentario):
      - en su ItemGroup, despues de BundleCatalogFiles:
        <BundleSectionMapFile Include="$(MSBuildProjectDirectory)\..\..\assets\structural-analysis\ram-elements\section-material-map.json" />
      - despues del Copy de los catalogos:
        <Copy SourceFiles="@(BundleSectionMapFile)" DestinationFolder="$(BundleContents)\structural-analysis\ram-elements" SkipUnchangedFiles="false" />
      - en el comentario que precede al target, el layout pasa a decir Contents\{the four RackCad DLLs, catalogs\,
        structural-analysis\ram-elements\section-material-map.json} y que el bundle lleva solo ensamblados de RackCad, sus catalogos y la
        tabla de secciones y materiales (misma ruta relativa que en la salida de compilacion; RACKRAMPAQUETE la busca junto al DLL).
      El elemento Content de la tabla, las propiedades, RemoveDir/MakeDir, el manifiesto y los demas Copy no cambian.
  (d) deploy/verify-bundle.ps1:
      - nuevo parametro [string]$SectionMapSourcePath (documentado en el bloque de ayuda como -CatalogsSourceDir: la tabla fuente
        aprobada; por omision assets\structural-analysis\ram-elements\section-material-map.json del repositorio);
      - $script:SectionMapRelativePath = "Contents/structural-analysis/ram-elements/section-material-map.json";
      - en Assert-Allowlist, una rama nueva que admite el archivo SOLO si $rel -ceq $script:SectionMapRelativePath (comparacion exacta);
        cualquier otro archivo bajo Contents/structural-analysis/ sigue cayendo en "Archivo no autorizado en el bundle";
      - funcion nueva Assert-SectionMapMatchesSource (-Root, -SourcePath), llamada en Invoke-VerifyBundle despues de
        Assert-CatalogsMatchSource: Assert-True (fuente existe como archivo) "No existe la tabla fuente aprobada de secciones y materiales:
        <ruta>"; Assert-True (la tabla del bundle existe como archivo) "Falta la tabla de secciones y materiales en el bundle:
        Contents/structural-analysis/ram-elements/section-material-map.json"; Assert-True (SHA-256 iguales con Get-FileHash)
        "Tabla de secciones y materiales modificada respecto a la fuente aprobada (SHA-256): Contents/structural-analysis/ram-elements/section-material-map.json";
      - en Invoke-VerifyBundle, resolver la fuente como -CatalogsSourceDir (GetFullPath del parametro o
        Join-Path (Split-Path -Parent $PSScriptRoot) "assets\structural-analysis\ram-elements\section-material-map.json") y mostrarla
        junto a las demas fuentes ("         tabla=<ruta>");
      - el bloque de descripcion enumera la tabla entre lo permitido (ruta exacta, identica en SHA-256 a la fuente aprobada) y el
        mensaje final OK menciona tambien la tabla identica a su fuente;
      - NINGUNA comprobacion ni mensaje existente cambia, se relaja ni se elimina.
  (e) Documentacion (texto exacto; conserva lo demas):
      - README.md, fila de `RACKRAMPAQUETE` de la tabla de comandos: la frase «Requiere la tabla de secciones y materiales de la salida
        de compilación del Plugin; no está disponible con el Autoloader.» pasa a «Usa la tabla de secciones y materiales que viaja junto
        al Plugin (salida de compilación y bundle del Autoloader).»
      - docs/guias/interoperabilidad-ram-elements.md:
        §2, la viñeta de la tabla: desde «**Solo viaja en la salida de compilación del Plugin**» hasta «y no escribe nada.» pasa a «Viaja
        en la salida de compilación del Plugin y en el bundle del Autoloader (`Contents\structural-analysis\ram-elements\`), idéntica a la
        fuente aprobada: el verificador del bundle lo exige (decisión R-D11, que revierte CD-I65-F4-10). Si falta, el comando responde
        `Unavailable` («Falta la tabla de secciones y materiales junto al Plugin») y no escribe nada.» (las subviñetas siguientes no cambian);
        §9, fila `Unavailable`: «falta la tabla (p. ej., instalación por Autoloader), el catálogo…» pasa a «falta la tabla junto al
        Plugin, el catálogo…»;
        §10, fila «`Unavailable` por falta de la tabla»: la segunda celda pasa a «comprobar que `structural-analysis\ram-elements\section-material-map.json`
        está junto al DLL del Plugin (con el Autoloader, en `Contents\`); reinstalar el bundle con `deploy\install-bundle.ps1` o usar la
        salida de compilación del Plugin»;
        §11 (limitaciones): elimina SOLO la viñeta «La tabla de secciones y materiales no viaja en el Autoloader (CD-I65-F4-10, diferido): …»
        (sus dos lineas); las demas viñetas quedan literalmente.
      - docs/guias/despliegue.md §4: en la frase del principio, «`Contents\` (los DLL y `catalogs\`)» pasa a «`Contents\` (los DLL,
        `catalogs\` y la tabla de secciones y materiales)»; en el arbol, despues de la linea de catalogs\, una linea nueva con la misma
        sangria: «    structural-analysis\ram-elements\section-material-map.json  (tabla de RACKRAMPAQUETE; idéntica a assets\)»; en
        «Compartir la app…», «(DLLs frescos + `catalogs\`)» pasa a «(DLLs frescos, `catalogs\` y la tabla)».
      Los documentos se escriben en UTF-8 sin BOM, con las tildes y comillas tal cual; ningun otro parrafo cambia.
  Comprobacion local del GREEN (AutoCAD cerrado):
      pwsh -NoProfile -File <worktree>/deploy/build-bundle.ps1 -Configuration Release -Output <worktree>/artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/R20261029T005052Z-2be3/work/publish-green -InventoryOutPath <.../work/inventory-green.txt>
      (debe terminar bien: verificador OK) y pwsh -NoProfile -File <worktree>/deploy/test-verify-bundle.ps1 -SourceBundle
      <publish-green>/RackCad.bundle -PublishDir <publish-green> (debe dar "14 casos superados (1 valido + 13 negativos)"); anota el
      inventario (25 archivos: los 24 de antes mas la tabla) y que el SHA-256 de la tabla del bundle es igual al de
      assets/structural-analysis/ram-elements/section-material-map.json del worktree. Corrida GREEN del filtro (todas en verde), suite
      Core completa en verde y
      dotnet build C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural/RackCad.sln -c Release (anota
      totales; ninguna advertencia CS, MSB, NU o NETSDK en archivos del diff); git diff --name-only RED GREEN = solo los cinco archivos
      del GREEN; git -C <worktree> rev-parse HEAD:assets/structural-analysis/ram-elements/section-material-map.json =
      d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2; commit GREEN con esta linea en el cuerpo, en UNA sola linea, y git push:
      "Estado: gate R-D11 en curso; entrega del Worker de RD11-AUTOLOADER-TABLE (WorkRunId R20261029T005052Z-2be3) pendiente de la verificacion del Controller. attempts = 7."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas ni de nombres de prueba (si un nombre de prueba los
  contiene, nombralo por su archivo y numero de linea): GATE PASS, GATE_PASS, Candidato, Candidate, FINAL_CANDIDATE_SHA, gate cerrado,
  gate closed, cierre del gate, rama integrada, integrada en main, integrated into main, merged into main, lista para integrar,
  ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile usando rutas absolutas):
  TaskId RD11-AUTOLOADER-TABLE; RunId R20261029T005052Z-2be3; DelegationRunId R20261029T004516Z-bf20; Initiative I-65; Gate R-D11; Attempt 7; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command
  completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX o el registro del
  script en work/); Evidence con: (1) el directorio efectivo informado al empezar y la declaracion de rutas absolutas y de que no hubo
  archivos temporales fuera de work/ (salvo la carpeta del harness, creada y borrada por el); (2) por invariante la prueba o el caso del
  harness que lo observa; (3) las salidas de build-bundle.ps1 y test-verify-bundle.ps1 en RED y GREEN (codigo y resumen); (4) el
  inventario del bundle GREEN y la comparacion SHA-256 de su tabla con la fuente; (5) la salida de dotnet build RackCad.sln -c Release
  (errores y advertencias); (6) las pruebas que pasan ya en el RED y por que; Worker = { Provider "Anthropic", ModelRequested
  "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
