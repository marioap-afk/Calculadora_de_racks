# I-62 — Investigación del repositorio para F4 (Track I; MEASURED el 2026-10-04)

| Tema | Hallazgo | Consecuencia para F4 |
|---|---|---|
| CI de Core | `ubuntu-latest`, `actions/checkout@v4` **sin `fetch-depth`** (profundidad 1); `dotnet test tests/RackCad.Tests/...` | una guarda Core no puede usar historia de Git: C-15, C-20b y C-20c son MC en repositorios desechables (C-18, C-20a y C-38 son Core sin historia) |
| CI de UI y Plugin | `windows-latest`; Build UI, UI Tests y Build Plugin without AutoCAD | sin cambios por F4 |
| Paquetes de prueba | `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `coverlet.collector`; **ningún** lector YAML ni de JSON Schema | DEP-F4-YAML = NOT NEEDED con el lector del subconjunto (`f4/yaml-subset`); los esquemas se validan con oráculos propios (como F2/F3) y, en los MC, con `Test-Json` |
| Procesos y Git en pruebas | ninguna prueba lanza procesos ni Git (`Process.Start` ausente en `tests/`) | conservarlo: los MC con historia viven en `docs/automation/evidence/I-62-F4/` (Python + Git), como F2/F3 |
| Directorios temporales | `Path.GetTempPath()` en `AtomicFileTests` | patrón para pruebas con archivos temporales |
| Lectura de archivos del repositorio | cada clase de guarda tiene su `RepoPath`/`Read`; `PrincipalPortabilityProtocolTests` tiene además `GitBlobSha1` | las guardas de F4 reutilizan los de `PrincipalPortabilityProtocolTests` o una clase auxiliar I62 |
| Fin de línea | `core.autocrlf=true` en local: CRLF en disco y LF en el índice y en la CI de Ubuntu | normalizar CRLF → LF antes de hashear o de comparar secciones (ya lo hacen el lector YAML, el oráculo y el resolver); los blobs se calculan con `git hash-object` (aplica el filtro) |
| Rutas | Windows local (barras invertidas), Ubuntu en CI | rutas relativas con `/` en datos y mapas; `Path.Combine` en C# |
| Git local | 2.54.0.windows.1; `core.longpaths` sin fijar | rutas largas en evidencia: activar `core.longpaths` en los clones desechables (lección de I-36A) |
| PowerShell | 7.6.6, `FullLanguage` en la sesión; el `pwsh` del runtime de Codex corre en ConstrainedLanguage (I-62, medido en V11) | los procedimientos que ejecuta un Controller Codex no dependen de tipos no básicos |
| Clones «limpios» | `git clone` desde una ruta local copia también los objetos inalcanzables (MEASURED en `f4/rebase-proto`) | los MC simulan otra máquina con `git clone --no-local` |
| Escapes en la herramienta Bash | un heredoc que contiene `\n` o `\r\n` dentro de un literal Python escribe saltos reales | escribir los scripts con la herramienta de archivos, no con heredocs |
| SDK | 8.0.423 a nivel de usuario (`%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`); el `dotnet` del `PATH` no resuelve `global.json` | las órdenes de prueba de F4 usan el `dotnet.exe` de usuario |
