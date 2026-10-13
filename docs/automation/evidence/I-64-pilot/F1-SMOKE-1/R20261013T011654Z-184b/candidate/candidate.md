# I-64 — Candidato de Smoke-1 (preparación; transitorio, ignorado por Git)

Bloque del Candidato (guía de validación manual §7.1, ampliado por T3D §3). Evidencia local producida DESPUÉS del commit, con el
árbol limpio y sobre ese HEAD exacto, con AutoCAD cerrado (0 procesos `acad` antes y después). Las rutas locales se escriben como
`<worktree>`.

```text
Candidate SHA:        4d79e91036acdaa3e62b7b54677f394593f581a0
Arbol limpio:         SI   (git status --porcelain: 0 lineas antes y despues)
Producto vs F1:       git diff --stat e0205b5a9f000bbd44aa1f7556d30f06411c3db7 HEAD -- src tests: vacio
SDK resuelto:         8.0.423 (dotnet de usuario; global.json 8.0.423)
Core Full local:      PASS (13122/13122, 0 omitidas; Debug; 21:47:15Z-21:50:15Z)
UI Full local:        PASS (1701/1718, 17 Skip existentes, 0 con error; Debug; 21:50:15Z-21:58:20Z)
Debug UI build:       PASS (0 advertencias, 0 errores)
Debug Plugin build:   PASS (0 errores; solo MSB3277, avisos conocidos de las referencias de AutoCAD, guia §1)
CI exact SHA:         GREEN (run 38177504770, event push, headSha 4d79e910...; job "UI Tests (WPF controls, net8.0-windows)" success; los cuatro jobs success)
Owner validation:     pending (Smoke-1, hito intermedio; NO autorizado todavia)
Biblioteca de bloques: override de %APPDATA%\RackCad\settings.json (BlockLibraryPath), archivo Base_de_datos_AutoCAD_V.0.dwg, fuera del worktree  SHA-256 B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8 (477525 bytes, mtime 2026-09-09T16:47:48Z); no se toca
DLL cargado:          <worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll  SHA-256 E769219D2F5EDF6E0DE0867633C4A89B94EBBD7F2BD88CF20864CBCF75EF7512 (280576 bytes, 2026-10-11T21:58:35Z)
Otros DLL RackCad:    RackCad.Application.dll  SHA-256 BEF900AB6D830509AD2FD35D8BAEB982544340DB4B9A3A10BD9EB9B4AAEE1E4D
                      RackCad.Domain.dll       SHA-256 D2E88F1D9BC3B1C841508A253A7E7C628C829091E3B7AF27EC76AB0E78599F95
                      RackCad.UI.dll           SHA-256 D7987BB806E3777BEFDE187BA0B9AD1456754FF0E813F72EAEA95C773BE55E64
AutoCAD:              AutoCAD 2025 R25.0.171.0.0 (acad.exe SHA-256 2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422)
```

Reglas que siguen vigentes hasta Smoke-1:
- **Ningún commit** en la rama antes de Smoke-1: el SHA se estampa en el ensamblado (guía §7), y un commit documental produciría
  otro Candidato. La custodia de esta preparación va al commit posterior a Smoke-1.
- No se recompila: el DLL cargado debe ser este archivo, con este SHA-256, en la BASELINE y en el POST (T3D §3, R-INV-02).
- Registros de las suites: `core-test.txt`, `ui-test.txt`, `ui-build.txt`, `plugin-build.txt`, `steps.txt` en esta carpeta.
