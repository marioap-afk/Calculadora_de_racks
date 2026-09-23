# I-58 C1 — diagnostico acotado, no F1

Ejecutar desde la raiz del worktree con `dotnet run --project docs/automation/evidence/I-58-c1-probe/I58.C1.Probe.csproj`.
Cuatro escenarios obligatorios, exit 0 solo si seleccion=4 y asserts satisfechos. Fuera de solucion y suites canonicas.
A/B/C ejecutan RackCantileverWindow real en STA (sin Show ni AutoCAD), controles y request real.
Reflection solo lee el seam interno Design para preparar fixture, nunca compara authored.
Stores/composer y AUTH09/assembler son reales. Los GUID exteriores del factory son fijos para diagnostico;
la identidad interior nueva procede del constructor real. Ningun comparador AUTH13 nuevo se implementa o ejecuta.

ExtractedPlugin.cs reproduce literalmente tres metodos puros actuales: BuildCantileverPayload de
src/RackCad.Plugin/RackCantileverCommands.cs; RestampDesign de
src/RackCad.Plugin/KindHandlers/CantileverKindHandler.cs; RestampEnvelope(Guid) de
src/RackCad.Plugin/RackEnvelopeRestamp.cs. El probe comprueba igualdad textual de cada extraccion con fuente.
El dispatch por kind es un stub Cantilever del harness; la ejecucion de estos metodos es RECONSTRUCTED,
no carga el Plugin/registry ni prueba transacciones, comandos, scan o escritura DWG.
Las hermanas de A/B son payloads sintetizados por el writer extraido; C obtiene request WPF real de
LoadExisting + InsertPlanta. D aplica el restamp reconstruido a ambas con el mismo nuevo GUID.
AUTH10 e I55 membership se inspeccionan por fuente por separado, no se anuncian como ejecutados aqui.
Resultados exact-SHA, SDK, limpieza, duracion y limites se registran fuera de este directorio.
