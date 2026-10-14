# I-65 F4 — compilación del DLL del smoke (custodia, 2026-10-14)

- **Autoridad:** disposición del Owner «AutoCAD Smoke Pre-Release and Engineering Mapping Decisions» (S-4 = APPROVED; BUILD FOR SMOKE
  = AUTHORIZED); decisiones §54. **La compilación no autoriza la carga:** AUTOCAD EXECUTION = NO; NETLOAD único aprobado en principio,
  no liberado.
- **Ejecutor:** Coordinator, con `acad.exe` = 0 y `RAMElements.exe` = 0 antes y después; sin abrir AutoCAD.

## Identidad

| Elemento | Valor |
|---|---|
| Commit compilado | `745653e1ecf10850a009e6c3d305e44ea55facbe` (árbol limpio; `git diff e610ea49 745653e1 -- src` vacío: código de producto idéntico al verificado de F4-T5) |
| Orden | `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -nologo` con el `dotnet.exe` del usuario (SDK 8.0.423, `global.json`); referencias de AutoCAD locales (`C:\Program Files\Autodesk\AutoCAD 2025`) |
| Resultado | 0 errores; 2 advertencias `MSB3277` (conflictos de versión conocidos de las referencias de AutoCAD; la guía manual §1 las documenta) |
| DLL | `src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll` |
| SHA-256 del DLL | **`2e04a4bb47b04012fab1fd04b0c9d11e5ec3dcaf01ba1aabbb9764a9c5c6e41d`** |
| Tamaño; fecha | 263 680 bytes; 2026-10-14T02:32:29Z |
| Versiones | FileVersion `1.0.0.0`; ProductVersion `1.0.0+745653e1ecf10850a009e6c3d305e44ea55facbe` (compilación determinista; la revisión de Git queda en el ensamblado) |
| Tabla copiada a la salida | `structural-analysis/ram-elements/section-material-map.json`, SHA-256 `5229231e…4048`, igual al recurso del repositorio (filas `proposed`) |
| Salida completa | 29 archivos; manifiesto con el SHA-256 de cada uno: [smoke-dll-745653e1-output-manifest.txt](smoke-dll-745653e1-output-manifest.txt) (LF, SHA-256 `14e860d1d4d4b0ea46f1567d2fc128d5cda8772817227bbf86d3c4d2599dc5b9`; la copia local CRLF citada en la solicitud a I-52 tiene `eea98100…f1a`) |
| Copia congelada | `D:\r65-smoke\745653e1\bin-Debug-net8.0-windows\` (29 archivos; DLL con el mismo SHA-256); fuera del repositorio |
| Cobertura por `TRUSTEDPATHS` | ni la carpeta del worktree ni la copia congelada están cubiertas → con `SECURELOAD` = 1 puede aparecer el aviso de seguridad (carga única aprobada en principio, S-3) |

Antes de la ventana, la sesión comprueba que el DLL y el manifiesto siguen idénticos. Si cambian por una recompilación, se usa la copia
congelada o se repite este registro.

## Referencias frescas para el preflight (solo lectura, 2026-10-14 ~02:35Z)

| Referencia | Valor |
|---|---|
| Perfil actual / número de perfiles | `<<Unnamed Profile>>` / 1 |
| `TRUSTEDPATHS` | 19 entradas; SHA-256 de la cadena `a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498` |
| `SECURELOAD` | 1 |
| `acad.exe` | R25.0.171.0.0; SHA-256 `2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422` |
| ACL (SHA-256 del listado de `icacls`) | `AcadLocation` `7d64e54b…661c`; carpeta del DLL `7a2294e0…7f11`; `D:\I52-CT21D-HOST` `eace16a7…7d07`; `\evidence` `5b0191f7…146a`; `\evidence\HGP-H1-20261001T225354Z-01` `1b541eb8…f4fc`; `\canonical-642ba392\declared-set` `5b1cf306…77ad` |

Las rutas de I-52 solo se listaron, sin entrar ni cambiar nada. Todas las referencias se vuelven a leer en la BASELINE de la ventana.
