
## Mediciones de la v4.3 (en `7f0d9aa3`)

## R-42. El contexto de nulabilidad y CA-T4a I-01

| Hecho | Fuente |
|---|---|
| `<Nullable>disable</Nullable>` para todos los proyectos, con el comentario de la política (un proyecto nuevo que quiera tipos de referencia anulables lo activa en su `.csproj`) | `Directory.Build.props:25-28` |
| Ningún `.csproj` lo cambia: `src/RackCad.Application`, `src/RackCad.Domain`, `src/RackCad.Plugin`, `src/RackCad.UI`, `tests/RackCad.Tests`, `tests/RackCad.UI.Tests` y las dos herramientas de `tools/` lo heredan | `git show` de cada `.csproj` |
| Llevan `#nullable enable` en su primera línea: WEI, WHC, RIA, `IWorkspaceEditIntentReader.cs` y `WorkspacePilotSupport.cs`; no lo llevan NH, WCo, FVM, `NumericFieldValidation.cs` ni ningún archivo de `Draft/` (su pureza lo prohíbe) | ídem |
| El esqueleto de la v4.2 daba `PushBackOriginByText.cs(16,29): warning CS8632` en los registros del Plugin, de Core y de UI de la compilación de la sesión; `build-plugin` falla con cualquier `warning CS` | CA-T4a §2 y §6; `eng/ci/verify-autocad-references.ps1` |
| Con la v4.3, la regla nueva no ve ningún `?` de tipo de referencia en lo que el GREEN añade a NH ni a `Draft/PushBackOriginByText.cs`, y sí ve los dos del texto de la v4.2 (contrafactual) | `measure_pbt4_v43-out.txt` |

## R-43. El recorrido de fuera de las marcas (CA-T4a M-01) y el GREEN aplicado

| Hecho | Fuente |
|---|---|
| Las marcas del recorrido (`MARKERS` de `walk_guards_pbt4a.py`) no incluyen `src`, `src/RackCad.Application` ni `src/RackCad.Plugin`, ni ven una ruta compuesta con `Path.Combine` | `walk_guards_pbt4a.py` |
| 146 archivos de prueba fuera del recorrido nombran `src/RackCad.(Application\|Plugin)`, `"src"` o un ayudante de barrido; 97 nombran como literal una carpeta antecesora de un archivo tocado o nuevo | `cat4a_walk_gap-out.txt` (sha256 `2605b5ca…70fb3`) |
| Sus literales, contra las 134 líneas que el GREEN añade o cambia y los dos archivos nuevos: solo fichas genéricas; los que leen de verdad los archivos tocados, leídos: ninguno cae | `cat4a_added_lines-all-out.txt` (`bfb4a5da…952e`); CA-T4a §5 |
| El recorrido, vuelto a correr sobre el GREEN de la v4.3: 46 clases, 1 026 pruebas, 764 / 162 / 100, y las 100 señaladas, las mismas que en la v4.2 | `pbt4a-guard-walk-out.txt`; `pbt4a-guard-walk-flagged-v43.txt` |
| `apply_green_pbt4a.py`, sobre una copia mínima: en `--dry` no escribe; de verdad, escribe los seis archivos, iguales al GREEN y con LF; una segunda corrida para (los archivos ya no son los de la revisión), y una copia con `.git` también | `selftest_apply_green-out.txt` |
| Los sha256 del `src/` del GREEN literal de PB-T4a (v4.3): `Draft/PushBackOriginByText.cs` `63084fae…7537`, `IWorkspaceEditIntentOriginReader.cs` `becb8d69…84ca`, WEI `6be957c3…5285`, WHC `63943612…3431`, RIA `658342fa…154d`, NH `915b560b…884e`; el conjunto, `7d29d396d8181e4ae83c5ec2693ed20e6f71b2846938eb07816967d33f4586cd` | ídem |
