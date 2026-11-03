# I-65 F4 — compilación del DLL del segundo smoke (custodia, 2026-10-16)

- **Autoridad:** disposición del Owner «F4-D Acceptance / Second AutoCAD Smoke Preparation» (DLL BUILD = AUTHORIZED; AUTOCAD EXECUTION
  = NOT AUTHORIZED); decisiones §67. La compilación no autoriza la carga.
- **Ejecutor:** Coordinator, con `acad.exe` = 0 y `RAMElements.exe` = 0 antes y después; sin abrir AutoCAD.

## Identidad

| Elemento | Valor |
|---|---|
| Código compilado | `51fc82f131a287ef66458a433c614c389033769d` (GREEN verificado de F4-D, `EXECUTION_VERIFIED` en `R20261016T161137Z-e795`). Se compiló en el worktree de la unidad con `git checkout --detach 51fc82f1` y árbol limpio, y después se volvió a la rama (`ad595b42`, que solo añade `docs/`: `git diff 51fc82f1 ad595b42 -- src assets tests tools RackCad.sln Directory.Build.props Directory.Build.targets global.json` vacío) |
| Orden | `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -nologo -o D:/r65-smoke/51fc82f1/bin-Debug-net8.0-windows` con el `dotnet.exe` del usuario (SDK 8.0.423, `global.json`); referencias de AutoCAD locales (`C:\Program Files\Autodesk\AutoCAD 2025`). La carpeta de salida es **nueva y dedicada** para no sustituir el DLL del primer smoke |
| Hora | 2026-10-16T18:54:16Z - 18:54:49Z |
| Resultado | 0 errores; 2 advertencias `MSB3277` (los conflictos de versión conocidos de las referencias de AutoCAD, como en el primer smoke) |
| DLL | `D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows\RackCad.Plugin.dll` |
| SHA-256 del DLL | **`fa334b144507fc8922171036edae878f773c003164f4c98302cdc67c9291aedb`** |
| Tamaño; fecha | 263 680 bytes; 2026-10-16T18:54:48Z |
| Versiones | FileVersion `1.0.0.0`; ProductVersion `1.0.0+51fc82f131a287ef66458a433c614c389033769d` (compilación determinista; la revisión de Git queda en el ensamblado); `RackCad.Application.dll` `1.0.0+51fc82f1…`, SHA-256 `1032e763…d380` |
| Salida completa | 32 archivos; manifiesto con el SHA-256 de cada uno: [smoke2-dll-51fc82f1-output-manifest.txt](smoke2-dll-51fc82f1-output-manifest.txt) (SHA-256 `5c9a6d62e84d23317d19a6d67a4883e8f7edb9dac33a94511761220dbdf265d2`). Frente a la salida del primer smoke: tres `deps.json` más (`RackCad.Application`, `RackCad.Domain`, `RackCad.UI`), que copia la compilación con `-o`; cambian los DLL/PDB (SHA de Git incrustado) y la tabla; el catálogo es idéntico |
| CI del código | `51fc82f1`: corrida 38928508008, 4/4 `success`; cierre `ad595b42`: corrida 38934290550, 4/4 `success` |
| DLL del primer smoke | `2e04a4bb…e41d`, **intacto** en `src/RackCad.Plugin/bin/Debug/net8.0-windows/` del worktree y en `D:\r65-smoke\745653e1\`; no se sustituyó |

## Tabla verificada distribuida junto al DLL (A65-F4D-01)

| Medida | Valor |
|---|---|
| Ruta | `D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows\structural-analysis\ram-elements\section-material-map.json` |
| Bytes | 3 279; sin CR; sin BOM |
| SHA-256 de los bytes | `fc3d91464ac48a15730661efcd50ecb770114559c68bdefae67e0816972db619` (= `f4d-table-target.json`) |
| SHA-256 normalizado a LF | `fc3d91464ac48a15730661efcd50ecb770114559c68bdefae67e0816972db619` (igual: el archivo ya es LF) |
| Blob de Git del archivo copiado (`git hash-object --no-filters`) | `d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2` = blob de la tabla en `51fc82f1` y del objetivo en `e886b557` |
| Lectura con el lector del DLL nuevo | `Valid`, 0 diagnósticos; `AISC-HSS-RECT-HSS4X4X_250` y `AISC-W-W12X26` `Verified`; E 29000 ksi, ν 0.3, G nulo, 490 pcf; `verifiedBy` de 43 caracteres con U+00E9 y U+2014; `verifiedOn` 2026-10-16 |

## Entradas efectivas y valores esperados del smoke (A65-F4D-07)

- **Archivo de cargas aprobado:** copia congelada `D:\r65-smoke\51fc82f1\inputs\ref-cant-01.loads.json`, 592 bytes, LF, SHA-256
  `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05`, blob `3855c268…` = `tests/RackCad.Tests/StructuralAnalysis/Fixtures/ref-cant-01.loads.json` en `51fc82f1`.
- **Valores esperados** ([smoke2-expected-inputs.json](smoke2-expected-inputs.json), generado con
  [smoke2-expected-inputs.ps1.txt](smoke2-expected-inputs.ps1.txt) y el `RackCad.Application.dll` de la salida): filas de la tabla
  distribuida con su `canonicalText`; `mapping.rowsSha256` esperado `54ca9f6c80376d857755e68fc16d7de58a771b52cb0a0b7fa452a5df6191c98c`
  (`StructuralFingerprints.MappingRows` sobre las dos filas); `verifiedBy` en UTF-8 `4d6172696f2050c3a972657a20e28094…`. Las huellas
  `sourceFingerprint` y `analysisInputFingerprint` **no** tienen valor esperado previo: dependen del `rackId` del DWG y se comparan contra
  el paquete real (manifiesto, script y línea de comandos), nunca contra fixtures.
- **Validador:** [validate-smoke2-package.py.txt](validate-smoke2-package.py.txt) (16 comprobaciones V-01..V-16, solo lectura);
  autoprueba con paquetes sintéticos ([script](selftest-smoke2-validator.py.txt), [salida](selftest-smoke2-validator.out.txt)): el caso
  correcto da PASS y cada mutación (`verifiedBy` con sustituciones, cargas con CRLF, huella de filas y huella del script) falla exactamente
  en sus comprobaciones.
