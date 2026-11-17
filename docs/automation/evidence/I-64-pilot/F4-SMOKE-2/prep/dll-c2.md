# I-64 F4 — Smoke-2 (reanudacion tras C2): el DLL

> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<carpeta-de-copias-de-Smoke-2>`, `<carpeta-del-DLL-de-Smoke-2>`, `<carpeta-del-DLL-de-C1>`, `<carpeta-del-DLL-de-C2>`, `<dotnet-del-usuario>`). El resto no cambia.

```text
Smoke-2 (reanudacion tras F4-T4-HOST-C2): DLL candidato (medido por la sesion, 2026-11-17 00:33Z)
Producto: 9abc1320743d5e4d472c4db6f5fdf4a96b835a46 (F4-T4-HOST-C2 GREEN)
Compilado desde el worktree de I-64 en HEAD 9f894587f4f600b34abd03931ddced2c4306f1b8 (commit documental de custodia del cierre; git diff 9abc1320 HEAD -- src tests vacio)
Orden: <dotnet-del-usuario> build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -p:UseAutoCADNuGetReferences=true -o <carpeta-del-DLL-de-C2> --no-incremental
Resultado: 0 advertencias, 0 errores; AutoCAD cerrado (sin acad.exe); sin variables RACKCAD_DEBUG_* en usuario, maquina ni proceso; arbol limpio antes y despues
<carpeta-del-DLL-de-C2>\RackCad.Plugin.dll  InformationalVersion 1.0.0+9f894587f4f600b34abd03931ddced2c4306f1b8  SHA-256 798C936698277F418CE47D31CAF2028F9253002DBF9DFEE16774EA923E4D4E48  LastWriteUtc 2026-11-17T00:33:59.2440841Z
<carpeta-del-DLL-de-C2>\RackCad.Application.dll  SHA-256 105C9FB67950D54581A505D021BF6A9F59559ED45786380C9DEA0353ACFDEBF0
<carpeta-del-DLL-de-C2>\RackCad.Domain.dll  SHA-256 EF843E4353E80E1FAB6CC2C1296CC4576D234DB6692EE49D8DE5D99A3FEE09CC
<carpeta-del-DLL-de-C2>\RackCad.UI.dll  SHA-256 5AEDE180D6AAD75186AB09E783DAC60C766C3C7348C3CA82C03A0838C7B83140
CI exacta del producto: 43972738540 (9abc1320) 4/4; CI de la custodia: 43988496393 (9f894587) 4/4
El DLL de C1 (<carpeta-del-DLL-de-C1>) y el anterior (<carpeta-del-DLL-de-Smoke-2>) se conservan sin tocar.
Copias: las de <carpeta-de-copias-de-Smoke-2>\r2\ (37), sin usar: las 37 coinciden con copias.json y los originales siguen intactos 7 de 7 (comprobado 2026-11-17).
```
