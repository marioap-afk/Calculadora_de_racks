# I-65 F4-T5-PLUGIN-COMMAND — analysis.md: STOP C-09 del Worker (intento 4)

Redactado por el Coordinator según AUTOMATION_PLAN §16.11 (2026-10-13).

## Hechos (comprobados mecánicamente)

- Delegación `R20261013T224954Z-16f8` (corrección; Attempt 4, AttemptsRemaining 2; CorrectionOf `R20261011T215838Z-09a6`), Worker
  `R20261013T225238Z-960c` (`claude-sonnet-5-5`/`medium`, 120 mensajes, 67 llamadas). Entrega válida contra el esquema: **BLOCKED/STOP**,
  `TriggeredStopConditions` = C-09.
- Commit RED `3e1ef603` con push (CI `38482239303`: Core `failure`, como corresponde a un RED); **sin GREEN**. HEAD = remoto = `3e1ef603`, árbol limpio.
- Los dos archivos del GREEN (`src/RackCad.Plugin/RackRamElementsCommands.cs` y la edición de `src/RackCad.Plugin/RackCad.Plugin.csproj`) quedaron
  retenidos en `work/hold/` del directorio del trabajo (ignorado), sin commit. Con ellos, según la entrega: filtro 98/98, suite Core 14 367/14 368,
  Plugin compilado hacia `artifacts/` con 0 errores y la solución en Release sin advertencias nuevas.
- Única prueba en rojo con el GREEN: `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace` («2 namespaces»), porque una cadena
  literal de muestra del archivo NUEVO del RED `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandGuardTests.cs` contiene una
  línea que empieza por `namespace` en la columna 0. Corregirlo exige editar un archivo de pruebas del RED, prohibido después del commit RED: C-09.
- Controles del Owner (CD-I65-F4-32): K1/K2 sin cambios en el checkout principal ni en los demás worktrees; K5 sin intérpretes, RAM, AutoCAD ni
  escrituras fuera del worktree; la primera orden fue `pwd`. En la llamada 64 el Worker ejecutó `git -C "D:/Documentos/Codex/Calculadora de racks"
  status --short`: **lectura** del checkout principal, prohibida por el prompt, sin efecto en su contenido (K1/K2).

## Causa raíz

El Worker ejecutó la suite Core completa solo después del commit RED (antes corrió el filtro). La guarda existente `NamespaceFolderGuardTests`
barre los archivos del proyecto de pruebas por líneas que empiezan por `namespace`, y la muestra sintética del RED la activó. Es un defecto del RED
(misma clase que las aserciones defectuosas del RED de F4-T1 y F4-T4), no del código del producto.

## Clasificación

- C-09 del Worker con resolución que **cambia el trabajo** (editar una prueba del RED): corrección según §16.8, `attempts` 4 → 5. No es materia
  OWNER-RESERVED (§16.11): decide el Coordinator dentro del presupuesto del Owner (`max_attempts` 6; Codex 34 de 46).
- La lectura del checkout principal: desviación de procedimiento sin efectos (DEV-I65-F4-20).

## Decisión del Coordinator (CD-I65-F4-39)

Corrección con el **mismo contrato** (`R20261013T224954Z-16f8/gate-contract.json`), Attempt 5, AttemptsRemaining 1, `CorrectionOf` =
`R20261013T225238Z-960c` (StopCondition):

1. **RED** = solo la corrección de `StructuralAnalysisPluginCommandGuardTests.cs`: ninguna línea del archivo empieza por `namespace` salvo su propia
   declaración; las muestras que necesiten esa línea la componen en tiempo de ejecución. Ningún otro cambio de pruebas. **Antes del commit RED, la
   suite Core completa**: los únicos fallos admitidos son los del filtro y los de las tres ediciones nombradas que dependen del archivo del comando
   (censo, conteo 37 → 38 y la lista de G1); `NamespaceFolderGuardTests` y las demás guardas en verde.
2. **GREEN** = el archivo del comando y el elemento `Content` del `.csproj`, sin tocar pruebas; puede partir de las copias retenidas del intento 4,
   custodiadas en la evidencia (no verificadas: el Worker las revisa contra los invariantes).
