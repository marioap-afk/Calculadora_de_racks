# PB-T3c: compilación de comprobación del esqueleto de la fachada (precondición 13 de la v4.7)

**Qué es:** la comprobación que pide la precondición 13 de la v4.7 (V47-02, el M-07 de la §13 de CA-T3b). Antes de emitir, la sesión aplica el esqueleto de PB-T3c sobre una copia exportada del cierre de PB-T3b, fuera del worktree, lo compila y **ejecuta la suite completa de Core**. Lo hace porque ni la reflexión por nombre ni los escaneos de ensamblado de `tests/` los ve un barrido por nombre: solo se ven al ejecutar.

**Cómo se hizo:** con `compile_check.sh`, `trx_failed.py` y `trx_outcomes.py`, en esta misma carpeta. El control negativo opcional va con `compile_check_ctrl.sh` (sección propia, más abajo).
- **La copia:** `git archive fe1c67f4efa23f2abb9b6ab9d636df54ae15f343` (el cierre de PB-T3b, F4B-E46) desde el checkout principal, en una carpeta nueva de ruta corta (`<copia>`), fuera del worktree. No es un worktree de git. El guion se niega a reutilizar una carpeta que ya exista.
- **El esqueleto:** `pbt3c-skeleton.patch` (sha256 `a81de435e70d62a40af8262d5d2d297eb36c6828053b0fce325c95af76004906`). Antes de aplicarlo, el guion comprueba que `src/RackCad.Plugin/Views/RackViewBlockEraser.cs` no existe en la base. Después lo aplica con `git apply --check` y `git apply`. El archivo queda con 15 líneas.
- **El compilador:** el SDK de usuario, con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false`.
  - `tests/RackCad.Tests` y `tests/RackCad.UI.Tests`, en Debug;
  - `src/RackCad.Plugin`, en Release, con `-o` hacia una carpeta dentro de la copia.
- **El control positivo:** el tipo `RackViewBlockEraser` y el método `EraseInTransaction` están en `RackCad.Plugin.dll`, es decir, el archivo nuevo se compiló de verdad. Se busca su nombre en la tabla de cadenas del ensamblado.
- **La suite completa de Core:** `dotnet test` sobre `tests/RackCad.Tests` en Debug, con TRX. La suite de UI se compila pero no se ejecuta, por la regla vigente.
- **Al acabar:** `build-server shutdown`. No queda ningún `dotnet.exe`, `VBCSCompiler.exe` ni `testhost.exe` en marcha, y el checkout principal sigue sin cambios (`git status --porcelain` vacío).

## Resultado

| Paso | Resultado |
|---|---|
| El esqueleto | Aplica limpio sobre `fe1c67f4`; el archivo no existía en la base |
| `tests/RackCad.Tests` Debug | rc 0, 0 errores |
| `tests/RackCad.UI.Tests` Debug | rc 0, 0 errores |
| `src/RackCad.Plugin` Release (`-o`) | rc 0, 0 errores |
| Avisos que nombran el esqueleto | 0 en los tres registros |
| El control positivo | `RackViewBlockEraser` y `EraseInTransaction` presentes en `RackCad.Plugin.dll` |
| La suite completa de Core (`out-run1.txt`, `trx-outcomes.txt`) | **19 124 resultados = 19 118 pasadas + 0 fallidas + 6 omitidas.** Es exactamente lo esperado en la precondición 13 |

**Las seis omitidas** son S-1 a S-6 de D-1, con su motivo literal de PB-T3b (`trx-outcomes.txt`); la Theory S-2 cuenta como una fila:
- `WorkspacePilotPrepareGuardTests`: S-1, S-2 y S-3;
- `WorkspacePilotUpdateGuardTests`: S-4;
- `WorkspacePilotUpdateIntegrationGuardTests`: S-5 y S-6.

Coincide fila a fila con la CI del GREEN de PB-T3b (`44707482956`: Core 19 124 = 19 118 + 6). El esqueleto no cambia el recuento ni el resultado de ninguna prueba existente.

**La TRX:** `out/core.trx`, sha256 `5ff50c8506aeeb5bd1ff1ec5e45780ca80d5c7e5a5f6c237517d1c15078e879a`. Se custodia solo por SHA-256, porque lleva usuario, máquina y rutas; su resumen está en `out-run1.txt` y en `trx-outcomes.txt`.

**Sin choque conocido que reproducir:** en PB-T3b había uno (los dos CS0121 y la reflexión de `Prepare`). En PB-T3c, los barridos T3C-1 de la emisión no encuentran ninguno: 0 llamadas ambiguas o rotas, ni reflexión ni grupos de métodos sobre los tipos tocados (`overload-sweep-pbt3c-fe1c67f4.txt`). Además, ningún proyecto de pruebas referencia el Plugin, así que la reflexión y los escaneos de ensamblado no alcanzan la fachada (CA-T3c). Lo que sí alcanza el texto de la fachada son las guardas de texto que leen todo `src/` o todo el Plugin.

## El control negativo de las guardas de texto (opcional, propuesto por CA-T3c)

**Qué prueba:** que las guardas leen el texto de la copia, y en concreto el archivo nuevo del esqueleto. `RepoRoot()` sube hasta el `RackCad.sln` de la copia.

**Cómo se hizo:** con `compile_check_ctrl.sh`, en la misma copia y sin recompilar, porque la guarda lee el texto al ejecutar.
- se mete la cadena `PushBack` en el **código** de la fachada, como un identificador (`private const int PushBackNegativeControl = 0;`) que `Clean` no quita;
- se ejecuta solo CT-PBT1B-15 (`PushBackSharedReferenceGuardTests`), que cuenta los archivos del Plugin que nombran `PushBack`;
- después se restaura el archivo del esqueleto byte a byte (el mismo sha256 antes y después) y se ejecuta otra vez la misma prueba.

| Paso | Resultado (`out-run-ctrl.txt`, `out-ctrl/`) |
|---|---|
| Con la inyección | **Falla 1 de 1.** La prueba espera `archivos=14` y la copia da `archivos=15`: la guarda vio el archivo nuevo |
| Con el esqueleto restaurado | Pasa 1 de 1 |

**Lo que no cubre:**
- **El cuerpo del GREEN de la fachada frente a las guardas de texto que leen todo `src/` o todo el Plugin** (CA-T3c, M-04). Es el hueco real de esta comprobación. El esqueleto solo lanza `NotSupportedException`. Las operaciones que añadirá el GREEN (`GetBlockReferenceIds`, `Erase()`, `UpgradeOpen()`, `OpenMode.ForWrite`) no están en la copia, así que esta corrida no dice si alguna de esas guardas las rechaza. CA-T3c leyó las que leen todo `src/` o el Plugin, unas veinte, y ninguna prohíbe esas operaciones fuera de archivos con nombre. La prueba real será la suite completa sobre el GREEN, en la CI y en la verificación.
- **Los avisos por código frente a la línea base.** Los mide el Worker en Release, como manda el contrato.
- **La suite de UI.** Se compila pero no se ejecuta, por la regla vigente. PB-T3c no añade pruebas de UI.
- **Las pruebas nuevas de PB-T3c.** Las escribe el Worker en el RED. Aquí solo están el esqueleto y las pruebas existentes.
- **El cambio del constructor de `RackPilotMutationScope` y `EraseUnit` en WR.** Son del GREEN, no del esqueleto. Su uso único (WR:156) lo mide el barrido T3C-1.

**Un tropiezo de la sesión, sin efecto:** la primera escritura de `trx-outcomes.txt` salió por la consola, en su codificación (no UTF-8). Se cambió `trx_outcomes.py` para que escriba el archivo en UTF-8 con LF y se volvió a generar desde la misma TRX. La cuenta no cambia.
