# I-65 — Disposiciones del Coordinator para la verificacion de F4-D-SECTION-MAP-VERIFIED (WorkRunId R20261016T145459Z-8301)

Texto de entrada de la verificacion del Controller (lecciones de I-61: las disposiciones se entregan como ENTRADA y se registran despues).

- D1 (desviacion declarada en Deviations, admisible): en StructuralAnalysisRamElementsPackageValidationTests, la prueba editada de la
  tabla del producto construye el modelo con la tabla del producto (F.ValidRefModel(shipped.Table)) y no con la tabla de fixture: la
  fabrica exige que los materiales del modelo coincidan con los de la tabla con la que se empaqueta, y los de fixture (E 111.5, peso
  222.5) no son los de la tabla del producto. La prueba conserva lo que observa (paquete Valid con contenido en memoria con la tabla del
  producto); no es un debilitamiento.
- D2 (desviacion de procedimiento, sin efectos; DEV-I65-F4-23): dos llamadas de Bash del Worker escribieron con heredoc un archivo
  temporal de sondeo tests/RackCad.Tests/StructuralAnalysis/TmpProbe.cs dentro del worktree (ruta absoluta) para medir las huellas; se
  borro, nunca entro en un commit y el arbol quedo limpio (K1/K2 y K5 sin efectos). El prompt pedia scripts auxiliares solo en work/ y
  archivos C# solo con las herramientas de edicion. El Coordinator no activa C-12 (todo dentro del worktree) ni C-05 (que evalua sobre las
  escrituras que llegan a Git, comprobadas por Scope); queda registrada como desviacion.
- D3 (causa de la CI roja del GREEN, leida en el codigo): RamBuildPackageFactory.IsAbsoluteNormalisedFolder exige una ruta de carpeta
  WINDOWS absoluta (el script se ejecuta en el programa de analisis, en Windows; A65-F3-07) y da RAM-PACKAGE-FOLDER-INVALID con cualquier
  otra. Las tres pruebas fallidas pasan como carpeta Path.Combine(Path.GetTempPath(), ...), que en el runner Linux es /tmp/...; en
  Windows pasan (entrega). Esa instruccion de la carpeta venia del prompt del Coordinator. No es un diagnostico del modelo ni de la
  tabla: con la tabla verified, REF-CANT-01 da Valid con 0 diagnosticos en la CI (RefCant01ConLaTablaDistribuidaEsValidoConLosMismos
  DiagnosticosQueConLaTablaVerifiedDeFixture pasa en el TRX de Linux). El Coordinator no activa C-03 por esta causa. La CI del CurrentSha
  NO esta en verde: evalua Ci y Tests segun AUTOMATION_PLAN 16.9 con los TRX.
- Primera delegacion de la cadena: ChainBaseSha = BaseSha = e886b557, ChainRedSha null, ChainRedFiles [] -> esta entrega EXIGE RED
  (RedSha fe9b6720). El alcance mecanico de la sesion esta en inputs/scope-mechanical.json; evalua Scope tu mismo, archivo por archivo,
  contra el AllowedWriteScope de la delegacion de entrada.
