# PB-T3b: compilación de comprobación del esqueleto corregido (F4B-E45)

**Qué es:** la comprobación nueva que fijó la decisión del Owner «Renombrar los métodos nuevos». Antes de reemitir, la sesión compila el esqueleto del §6.2 sobre una copia exportada de la base, fuera del worktree.

**Cómo se hizo:** `compile_check.sh` y `rename_overloads.py`, en esta misma carpeta.
- **La copia:** `git archive eab27a9e` desde el checkout principal, en una carpeta nueva de ruta corta (`<copia>`), fuera del worktree. No es un worktree de git.
- **El esqueleto:** `blocked-skeleton.patch` del Worker del intento 1 (`R20261120T055235Z-a45b`), aplicado con `git apply --check` y después `git apply`.
- **El compilador:** el SDK de usuario, con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false`. El Plugin se compila con `-o` hacia una carpeta dentro de la copia.
- **Al acabar:** `build-server shutdown`. Ya no queda ningún `dotnet.exe` ni `VBCSCompiler.exe` en marcha.

## Resultado

| Paso | Qué | Resultado |
|---|---|---|
| A, el control negativo | Esqueleto de la v4.5 tal cual, `tests/RackCad.Tests` en Debug | rc 1. Exactamente los dos CS0121 del Worker: `WorkspacePilotUpdateRunTests.cs(669,86)` y `(799,93)`. Cada uno sale dos veces en el log de MSBuild |
| B | El mismo esqueleto con `ExecuteFull` y `ComparePostFull` (una sustitución de cada, exacta) | **`tests/RackCad.Tests` Debug: 0 errores. `tests/RackCad.UI.Tests` Debug: 0 errores. `src/RackCad.Plugin` Release: 0 errores** |

**El checkout principal** sigue sin cambios.

## Segunda comprobación: los tres nombres y la suite de Core ejecutada

El barrido ampliado de la v4.6 encontró un tercer choque, que el compilador no ve. Es una prueba de reflexión: `WorkspacePilotPrepareTests.cs:2155-2160` hace `Assert.Single(... m.Name == "Prepare")` sobre `PushBackPilotUpdatePlanner`. Por eso esta comprobación ejecuta las pruebas además de compilar.

**Cómo se hizo:** con `compile_check3.sh`, `rename_overloads3.py` y `trx_failed.py`.
- Se usa una copia exportada **nueva**, también con `git archive eab27a9e`.
- Se aplica el mismo esqueleto con los tres nombres: `PrepareFull` en PP, y `ExecuteFull` y `ComparePostFull` en UR.
- Se compilan las pruebas de Core y de UI y el Plugin, y se **ejecuta la suite completa de Core** con TRX. La suite de UI no se ejecuta en local, por la regla vigente.

| Paso | Resultado |
|---|---|
| Compilación de `tests/RackCad.Tests` Debug, `tests/RackCad.UI.Tests` Debug y `src/RackCad.Plugin` Release | rc 0, 0 errores en los tres |
| Suite completa de Core (`out-run3.txt`) | **19 107 de 19 107 pasadas, 0 fallidas y 0 omitidas** |
| Control negativo (`compile_check3_ctrl.sh`, `out-run3ctrl.txt`): en la misma copia, `PrepareFull` vuelve a ser `Prepare` y se ejecuta solo `WorkspacePilotPrepareTests` | **Falla 1 de 91, exactamente `WorkspacePilotPrepareTests.ThePlanTypesLiveInThePilotNamespaceAndTheirSignaturesNameNoTypeOfTheHostOrOfTheDrawing`**, la prueba de reflexión. Después se restaura `PrepareFull` |

**Los escaneos de ensamblado:** las pruebas hacen 38 escaneos de este tipo (`GetTypes()` y `GetExportedTypes()`, en 29 archivos), y ningún barrido por nombre los ve. La suite completa de Core ejecutada los cubre: es lo que cierra el hueco del M-07 de la §13.

**Una lectura sobre D-1:** sin los `Skip`, los métodos S-1 a S-6 también pasan sobre el esqueleto. Es lo esperado, porque el esqueleto es inerte: D-1 se salta porque el GREEN cambia el comportamiento que esas pruebas de F4 fijan, no por el esqueleto.

**Un tropiezo de la sesión, sin efecto:** la primera corrida del control usó un filtro con un espacio de nombres equivocado y no seleccionó ninguna prueba (0 de 0). Se corrigió el filtro y se repitió. La salida que vale es la segunda.

**Al acabar:** `build-server shutdown`. No queda ningún `dotnet.exe` ni `testhost.exe` en marcha, y el checkout principal sigue sin cambios.

**Lo que no cubren las dos comprobaciones** (corregido tras el M-06 de la §13 de CA-T3b):
- **Los avisos por código frente a la línea base.** Los mide el Worker en Release, como manda el contrato.
- **La suite de UI.** La segunda comprobación la compila pero no la ejecuta, por la regla vigente. La suite de Core sí se ejecuta entera.
- **Las pruebas nuevas de PB-T3b.** Las escribe el Worker en el RED. Aquí solo están el esqueleto y las pruebas existentes.
- **El texto del esqueleto.** La primera comprobación usa el patch del intento 1 con dos nombres cambiados. La segunda usa ese mismo patch con los tres nombres de la v4.6, y no el texto del §6.2 transcrito de nuevo. La §13 de CA-T3b leyó el esqueleto del intento 1 con los tres nombres y encontró que sus cuerpos coinciden con el §6.2 de la v4.6.
