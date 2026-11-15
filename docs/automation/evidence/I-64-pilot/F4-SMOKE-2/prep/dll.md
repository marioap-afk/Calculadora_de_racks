# I-64 F4 — Smoke-2: el DLL candidato

> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-de-copias-de-Smoke-2>`, `<carpeta-del-DLL-de-Smoke-2>`, `<directorio-de-H0>` y `<unidad-de-datos>`). El resto del texto no cambia.

```text
Smoke-2: DLL candidato (medido por la sesión, 2026-11-15)
Producto: b88dabf254645e83d71ec42d4fd621d19c9b87b7 (F4-T4C2-PANEL GREEN)
Compilado desde el worktree de I-64 en HEAD b5589f3dfc4e69518db3434de0ee107337937560 (commit documental de custodia; git diff b88dabf2 HEAD -- src tests vacio)
Orden: <dotnet-del-usuario> build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -p:UseAutoCADNuGetReferences=true -o <carpeta-del-DLL-de-Smoke-2> --no-incremental
Resultado: Compilacion correcta, 0 advertencias, 0 errores; AutoCAD cerrado (sin acad.exe); sin variables RACKCAD_DEBUG_* en usuario, maquina ni proceso
<carpeta-del-DLL-de-Smoke-2>\RackCad.Plugin.dll  InformationalVersion 1.0.0+b5589f3dfc4e69518db3434de0ee107337937560  SHA-256 8BB8E7E5C6F24E0D3BD3E9B21ED955EC52F723D60C9E9C4C1376E9CAD0797964  LastWriteUtc 2026-11-15T22:28:11.4343243Z
<carpeta-del-DLL-de-Smoke-2>\RackCad.Application.dll  SHA-256 D24C8F84F29003CE8E9F00FD94B80D6E7A834E14E0371940D2088AE31270B057
<carpeta-del-DLL-de-Smoke-2>\RackCad.Domain.dll  SHA-256 A666342804B31BEDE6A08C6BAA57BDC710B07F4A4244FE9B00B236182EF6FC04
<carpeta-del-DLL-de-Smoke-2>\RackCad.UI.dll  SHA-256 3CDBC0F149AF735EE50C39DACBF26AD5FFB7CD761F89A22F2B1C68DC83EB3D72
CI exacta del producto: 43758023582 (b88dabf2) 4/4; CI de la custodia: 43763175054 (b5589f3d) 4/4
DV-05 / O-08: dispensa aceptada por el Owner el 2026-11-15 (owner-f4t4c-package-decisions.md, pregunta 4, «Aceptar todas»): CI exacta + filtros locales, sin la suite completa de UI en local (H-1)
Aislamiento: ninguna otra sesion de Claude en marcha (I-52 e I-62 detenidas); procesos codex.exe = servidores de la app de escritorio de Codex (padre ChatGPT.exe, iniciados 2026-11-14), sin el worktree en su linea de ordenes
```
