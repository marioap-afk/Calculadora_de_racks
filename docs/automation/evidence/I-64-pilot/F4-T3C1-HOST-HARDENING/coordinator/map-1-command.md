# Mapa 1: superficie del comando y censos (explorador, HEAD `7895f397`)

Los helpers `Src.Code` y `CodeOnly` quitan los comentarios, pero no los literales: las regex se aplican también a los textos de los mensajes.

## 1. `WorkspacePanelCommand.cs` (30 líneas)

- Usings `Autodesk.AutoCAD.Runtime` y el alias `AcApplication` (1-2).
- 12: la clase. 14: `[CommandMethod("RACKPANEL")]`, sin flags. 15: `RackPanel()`.
- 19: `WorkspaceController.GetOrCreate().OpenPanel();`. 20: `WorkspacePaletteHost.GetOrCreate().Show();`.
- 21-22: `MdiActiveDocument?.Editor.WriteMessage("\nRackCad: panel Workspace mostrado.")`.
- 24-26: `catch (System.Exception ex)` con `RackCommandSupport.Report(ex)`.
- Hoy no se usa `IsCreated` en ningún sitio.

## 2. Censos que fallan con un segundo `[CommandMethod]`

- **`WorkspaceHostSourceGuardTests.cs`:**
  - 36-47 cuenta exactamente `RACKPANEL`, y sigue pasando;
  - **49-56 `Equal(1, Count(\[\s*CommandMethod\b))` FALLA (2)**; además prohíbe `CommandFlags`, `CommandClass` y `LispFunction`;
  - 58-63 pasa;
  - 65-73 exige `GetOrCreate()` y `.OpenPanel()` y prohíbe `GetObject`, `SelectImplied`, `LockDocument`, `StartTransaction`, `System.Windows`, `PaletteSet` y `AutoCadEventBridge`;
  - 84-89 admite `WriteMessage` solo en el archivo del comando.
- **`WorkspacePaletteHostSourceGuardTests.cs`:**
  - **320-330: el 325 `Equal(1)` FALLA**; 329 prohíbe `CommandFlags`, `CommandClass` y `LispFunction`;
  - 311-318 prohíbe `no\s+muestra`, `aun\s+no`, `todavia\s+no` y `not\s+shown` (IgnoreCase, sin tilde);
  - 299-309: `GetOrCreate().OpenPanel` va antes que `Show`;
  - **659: `Equal(45, número de [Fact] y [Theory] de WorkspaceHostSourceGuardTests.cs)`**: añadir o quitar una prueba en ese archivo obliga a cambiar el número.
- **`WorkspaceBridgeSourceGuardTests.cs`:**
  - 340-351, `DeclaresEntryPoint`: `if (attributes != 1 || allowed != 1) return true;`. Con dos atributos **FALLAN** `NothingIsHookedWhen…` (320-331) y `WorkspaceSourceGuardExceptionTests.TheRealWorkspaceFilesPassTheRefinedGuard` (106-117);
  - 139-144: `ForbiddenCallNames` incluye **`GetKeywords`**; `HandlerForbiddenCallsFor` (159-163) no exceptúa el comando, así que **FALLAN** 165-175 y 106-117;
  - 216-225: una línea `public` no puede nombrar `Editor`, `Document` ni `Autodesk`; 205-214: tampoco campos de esos tipos;
  - 362-372 prohíbe `Metric`, `Provider`, `Aggregat` y `\bBom` (IgnoreCase), también en los textos.
- **`WorkspaceSourceGuardExceptionTests.cs`:**
  - 30-61: texto inyectado; 33 espera `false` con un solo RACKPANEL y 37-46 esperan `true`;
  - **250-258: exactamente 2 `string.Equals(path, (Reader|Command)AdapterPath, …)` en la guarda del puente**: una excepción nueva para `GetKeywords` lo deja en 3 y FALLA. 229-236 hace lo mismo para Facts y Effects;
  - 72-84 y 195-226: el comando sigue sin `SelectImplied`, `GetObject`, `SetImpliedSelection` ni `SetCurrentView`.
- **`CustomPropertiesCommandGuardTests.cs`:** 55 `F1PanelCommands = { "RACKPANEL" }`. **71-75 FALLA** con «sin clasificar en el censo de G7 y G15: RACKPANELDESCARTAR». Lee el texto en bruto.
- **`SelectiveEditorOpenTests.cs:566`: `Equal(39, total)` FALLA (40)**. Cuenta `[CommandMethod(` en bruto.
- **Pasan:**
  - `PushBackRoundTripSourceGuardTests.cs:361-375` (unicidad);
  - `CustomPropertiesHelpCensusTests.cs:91-105`;
  - `RackProjectionCommandGuardTests.cs:82-93`;
  - `WorkspacePilotEditGuardTests.cs:898`;
  - `WorkspaceBrowserHostGuardTests.cs:257-264`;
  - `WorkspaceNavigationHostTests.cs:443`.
- **Restricciones extra sobre el archivo del comando:**
  - `WorkspaceNavigationHostTests.cs:613-618` prohíbe `WorkspaceNavigation`, `NavigationEcho` e `IWorkspaceNavigationPort`;
  - `WorkspacePilotCloseSyncGuardTests.cs:155-170`: `Take(Close|Loss)Notices` solo en 4 archivos, y este no es uno de ellos;
  - `WorkspaceHostSourceGuardTests.cs:393-437` prohíbe `System.Windows`, `Dispatcher`, `Timer` y `async`.

## 3. Patrones del Plugin

- **Sí/No con «No» por defecto:**
  - `RackLayoutCommands.cs:149-155`: `Keywords.Add("Si")`, `Add("No")`, `Default = "No"`, `AllowNone = true`, y compara con `"Si"`, sin tilde;
  - `RackLayoutCommands.Fill.cs:498-514`, `AskYes`: `PromptStatus.None` da el valor por defecto.
- **Otros `GetKeywords`:** `RackCamaCommands.cs:45-49` y `Views/RackProjectionCommandPort.cs:49-66`.
- **`WriteMessage`:** está en 25 archivos del Plugin; en Workspace, solo en el comando (21). `RackCommandSupport.Report` también escribe (30-31).

## 4. Lo que expone el controlador

- 67 `GetOrCreate()`; 70 `public static bool IsCreated`.
- `Holder` es privado (23): **no hay ningún acceso a la instancia ya creada sin `GetOrCreate()`**, y el archivo del controlador ya contiene uno (19). Ninguna guarda cuenta los `GetOrCreate`.
- 73 `Port`: `GetSnapshot().ActiveSession` (host 223-241), null si el puente no arrancó o no hay documento.
- 110-117 `PilotEdit`; el adaptador está en 234-268, con `DiscardDocument` (261) y `GetRacksAtRisk` (263).
- `ActiveSession` y `TryGetSession` no son públicos: solo están en `BridgeLink`, privado (286-288).
- El gate (`WorkspacePilotSupport.cs:188-190`) delega sin filtrar por la profundidad de aviso.
- **Host:**
  - `DiscardDocument` (746-762) pasa por `Authorize` y da Rejected, Busy, Discarded o Failed;
  - `GetRacksAtRisk` (765-807) es solo memoria;
  - `Authorize` (244-245) compara con `MdiActiveDocument` vía `HostProbe` (controlador 306-313).
- `new WorkspaceRequest(SessionId)` (`SessionId.cs:22`).

## 5. Cómo se prueba hoy el Plugin

- Ninguno de los dos proyectos de prueba referencia el Plugin.
- El Plugin solo tiene guardas de texto (`WorkspaceHostSourceSupport.cs:11-15`), algunas con mutaciones.
- El comportamiento se prueba en núcleos de Application con dobles: `WorkspaceHostTestSupport.cs`, con `WorkspaceSimulatedProbe` (11) y `WorkspaceSimulatedReader` (19), y `WorkspacePilotEditHostTests`.
- **Implicación para T3C:** la lógica de `RACKPANELDESCARTAR` (lista, confirmación, TOCTOU, mensajes) y la del envoltorio debería vivir en un núcleo de Application, probable con dobles. El Plugin quedaría como un adaptador fino, cubierto por guardas, como en T2B2A, T2C y T3A.
