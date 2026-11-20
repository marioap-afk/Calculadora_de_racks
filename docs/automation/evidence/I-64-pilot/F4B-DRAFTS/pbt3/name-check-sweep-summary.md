# I-64 F4B PB-T3 — El barrido de nombres de la v4.5 (§4.1 del paquete; V45-02): `name_check.py` y el resumen de sus salidas

**Qué es:** el método del barrido de colisiones de la v4.5 (F4B-E42) y un resumen de sus salidas. Lo pidió la sesión: el método se custodia, y el volcado entero no.
- **El método:** `name_check.py`, que se custodia al lado, redactado (2 947 bytes, sha256 `e1652a36168df051207732824121fdb1f325a8d5b874c19e8f6dc18d2e053dba`).
  - Lee por objetos git (`git ls-tree` y `git show`) los archivos de `tests/` que leen el código del producto (`Src.AllProduct(`, `Src.Files(`, `GetFiles(`, `EnumerateFiles(`, `Src.Code(`, `ReadAllText(`, `Src.Text(`).
  - Toma cada literal de cadena de dos o más caracteres y lo prueba como expresión regular y como subcadena, en tres líneas de muestra con el nombre candidato: la firma, la llamada y un uso.
  - Informa de los literales cuya coincidencia se solapa con el nombre.
  - La orden: `python -I name_check.py <checkout-principal> 18e4b783 <nombre> [<nombre> ...]`.
- **Las salidas** se quedan fuera de la custodia: son las de `_sweep/` en la carpeta del paquete, sin rutas locales. Van identificadas por su sha256 en la tabla de abajo.

## Las salidas, una a una

| Archivo | Bytes | Líneas | sha256 | Qué dice |
|---|---|---|---|---|
| `namecheck.txt` | 31 919 | 275 | `6595b03aaa071f301025e8ce8ce1caf302d0ab258756e7e37a245b095c737468` | `scanned 164 literals tried 22346`: 164 archivos de `tests/` que leen código, con 22 346 literales probados. Da 274 filas en 36 archivos, con coincidencias para los dos nombres: 32 con `ForFullWrite` y 42 con `ForWrite`. Las de `ForWrite` exacto están en 11 líneas: el término de G-9 (`WorkspacePilotUpdateIntegrationGuardTests:603`, y :112, :119 y :559), el de UpdateGuard (:195), `WorkspaceEditIntentGuardTests:290` y :433, `WorkspacePilotEditGuardTests:983`, `CustomPropertiesEdgeGuardTests:480`, `CustomPropertiesCommandGuardTests:297` y `UiSystemBoundaryGuardTests:113` |
| `namecheck-new.txt` | 24 963 | 228 | `86f298f9736676d042be2744fc0dbc84c6a198d1293f1940b3dcf566234600df` | Los mismos 164 archivos y 22 346 literales. Da 227 filas en 34 archivos: 32 con `ForFullWrite` y **0 con `ForWrite`**. Las 32 son expresiones genéricas que extraen identificadores o sentencias, como el `(?<fqn>...)` de `UiSystemBoundaryGuardTests:113` o el de las asignaciones de `PushBackDraftPortSharedReferenceGuardTests:109`, y no prohíben nombres. Otras 193 son coincidencias de una o dos letras, ruido de expresiones genéricas |
| `scoped.txt` | 5 521 | 91 | `9cfedda6e7f490b864eafa899c08a3e6a59473421e2ebe456dc985cab9a940e8` | Las 91 clases de guarda que barren una carpeta o todo `src/`: 79 de `tests/RackCad.Tests/` y 12 de `tests/RackCad.UI.Tests/` |
| `scopes.txt` | 38 681 | 618 | `21eef0333b20740fa5dd9ad693129babf150cc8351f252fa3c358d3fc3b7258d` | Esas 91 clases, con sus 527 alcances (`combine:`, `glob:` y otros): qué carpeta o archivo lee cada una |
| `helper-callers.txt` | 2 154 | 33 | `22d13d74d52b02cadfe525725d85c177b098a7fdcd81907028bfea960d03706b` | Los 33 archivos de `tests/RackCad.Tests/` que llaman a los ayudantes compartidos de lectura de código (los de `Workspace/` entre ellos) |
| `hits.txt` | 369 214 | 2 479 | `93eef36027d56ac21e79a79aca34d6ac790e96b87350a46391c2718dbb2be5eb` | `scanned guard files: 91`. Son 2 369 coincidencias de los términos de 83 de esas guardas con 14 grupos de archivos nuevos o tocados de PB-T3: los ocho de `Draft/`, PP y UR, y WR y la fachada |
| `grouped.txt` | 89 190 | 681 | `0e93aa37fe85b81e3e9aa0a29104ed86bfd39816fe436c058171ccdf31d9c5e1` | Las mismas coincidencias, agrupadas por archivo de prueba (77) y por línea, con el término y los archivos que lo tienen |

## Lo que sale del barrido (el §4.1 de la v4.5, y el §4 de `v45-delta.md`)

- **La única colisión es G-9 con `ForWrite`** (la v4.4), en `Draft/PushBackFullComposition.cs` y en PP (`BuildFull`). La resuelve `ForFullWrite` (V45-01).
- **Con `ForFullWrite`, ninguna guarda prohíbe el nombre.** Las coincidencias que quedan son expresiones genéricas que extraen identificadores o sentencias.
- **Lo demás de `hits.txt` y `grouped.txt`** son términos de censo o de alcance que el §4.1 leyó uno a uno en su prueba, por su alcance y su sentido. No hay otra colisión.

**El guion que produjo `scoped.txt`, `scopes.txt`, `helper-callers.txt`, `hits.txt` y `grouped.txt` no está en la carpeta del paquete.** Esas cinco salidas se citan solo por su sha256; `name_check.py` es el método que se custodia, y sus dos salidas son las primeras filas de la tabla.
