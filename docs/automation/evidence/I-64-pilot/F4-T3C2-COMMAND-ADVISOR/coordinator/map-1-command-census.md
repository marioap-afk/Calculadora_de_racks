# Mapa 1 — censo de comandos y guardas que toca F4-T3C2 (RACKPANELDESCARTAR)

Medido en el worktree, rama `architecture/workspace-persistente-rackcad`, HEAD `e89948b9`. `git diff --stat 7bd275e4 HEAD -- src tests` está vacío: el código de producto y de pruebas es el de `7bd275e4`. Solo lectura: `git show`/`git grep`/`git ls-files`/lectura de archivos; en `tests/` solo archivos versionados (ningún `bin/` ni `obj/` está versionado: `git ls-files tests | grep /bin/|/obj/` no devuelve nada). Todas las líneas son del worktree en HEAD.

---

## 1. Estado actual

### 1.1 `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs` (30 líneas, completo)

- 1 `using Autodesk.AutoCAD.Runtime;`
- 2 `using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;`
- 4 `namespace RackCad.Plugin.Workspace` (bloque, 5 `{`)
- 6-11 doc: `RACKPANEL (I-64 F1-T3A, T3B): the one command of the workspace panel, with no alias. ...` (no contiene la subcadena `[CommandMethod(`).
- 12 `public sealed class WorkspacePanelCommand`
- 14 `[CommandMethod("RACKPANEL")]`
- 15 `public void RackPanel()`
- 17 `try`
- 19 `WorkspaceController.GetOrCreate().OpenPanel();`
- 20 `WorkspacePaletteHost.GetOrCreate().Show();`
- 21-22 `AcApplication.DocumentManager.MdiActiveDocument?.Editor.WriteMessage(` / `"\nRackCad: panel Workspace mostrado.");`
- 24 `catch (System.Exception ex)`
- 26 `RackCommandSupport.Report(ex);`

`RackCommandSupport.Report` está en `src/RackCad.Plugin/RackCommandSupport.cs:23-32` (`internal static class RackCommandSupport` en 21): `RackLog.Exception("RackCad command", ex);` (28) y `document?.Editor.WriteMessage("\nRackCad error: " + ex.Message);` (31).

### 1.2 `src/RackCad.Plugin/Workspace/WorkspaceController.cs` (327 líneas)

Estático:
- 23-24 `private static readonly WorkspaceLazyInstance<WorkspaceController> Holder =` / `new WorkspaceLazyInstance<WorkspaceController>(() => new WorkspaceController());`
- 67 `public static WorkspaceController GetOrCreate() => Holder.GetOrCreate();`
- 70 `public static bool IsCreated => Holder.IsCreated;`
- No hay ningún acceso estático que devuelva la instancia sin crearla: solo `IsCreated` (bool). 35 `private WorkspaceController()`.

Público de instancia:
- 64 `public event Action StateChanged;`
- 73 `public IWorkspaceHostPort Port => _port;`
- 79-97 `public IWorkspaceNavigationPort Navigation` (crea `WorkspaceNavigationHost` en el primer acceso, 85-88, y asigna `_host.SetEditIntentReader(_navigation.EditIntentReader);` en 92).
- 103 `public IWorkspaceBrowserSummaryPort BrowserSummary => (IWorkspaceBrowserSummaryPort)Navigation;`
- 110-117 `public IWorkspacePilotEditPort PilotEdit` → 114 `_ = Navigation;` (efecto: crea el host de navegación si no existe) y 115 `return _pilotEdit;`
- 123-127 `public IDisposable EnterPanelModal()`
- 129 `public bool PanelVisible => _host.PanelVisible;`
- 132-136 `OpenPanel()`, 138-142 `ShowPanel()`, 144-148 `HidePanel()`, 150 `RefreshPanel()`, 152 `SetModalRackCadActive(bool active)`, 155 `SetInstrumentationEnabled(bool enabled)`.

Privado relevante:
- 49 `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));`
- 53 `_bridge.SetLifecycleAdvisor(_host);` 54-56 las tres `+=`.
- 206-221 `NotifyStateChanged()` (único `StateChanged?.Invoke()`).
- 234-268 `private sealed class PilotEditAdapter : IWorkspacePilotEditPort`: 261 `public EditDiscardOutcome DiscardDocument(WorkspaceRequest origin) => _gate.DiscardDocument(origin);`, 263 `public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin) => _gate.GetRacksAtRisk(origin);`, 265/267 `TakeLossNotices`/`TakeCloseNotices`.
- 271-294 `private sealed class BridgeLink`: 286 `public SessionId? ActiveSession => _bridge.ActiveSession;`, 288 `TryGetSession`.
- 297-325 `private sealed class HostProbe` (310-311 `ActiveDocumentNativeId`, 317-323 `IsActiveDocumentQuiescent`).

Cómo se obtiene la sesión activa desde fuera del controlador: solo por `Port.GetSnapshot().ActiveSession` (`SessionId?`, `src/RackCad.Application/Workspace/WorkspaceHostSnapshot.cs:37`). Se calcula en `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:223-241`: 225 `var active = _started ? _bridge.ActiveSession : null;`, 227 exige que la sesión exista, 235 `hasDocument ? id : null`. La petición es `public readonly record struct WorkspaceRequest(SessionId Origin);` (`src/RackCad.Application/Workspace/SessionId.cs:22`).

### 1.3 `src/RackCad.Application/Workspace/WorkspaceLazyInstance.cs` (54 líneas)

- 11 `public sealed class WorkspaceLazyInstance<T> where T : class`
- 23-29 `public bool IsCreated` (con `lock`), 31-37 `public int CreateCount`, 39-52 `public T GetOrCreate()`.
- No hay `TryGet`/`Existing`/`Peek`. Pruebas de comportamiento: `tests/RackCad.Tests/Workspace/WorkspaceHostPolicyTests.cs:152-246` (no hay censo de su superficie pública).

### 1.4 El puerto del piloto y la semántica que el comando consumiría

- `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:64` `EditDiscardOutcome DiscardDocument(WorkspaceRequest origin);`, 70 `PilotRiskView GetRacksAtRisk(WorkspaceRequest origin);`
- Puerta de avisos: `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:153` `public sealed class WorkspacePilotEditGate : IWorkspacePilotEditPort`; solo las tres lecturas esperan dentro de un aviso (167-174); 188 `DiscardDocument(...) => _inner.DiscardDocument(origin);` y 190 `GetRacksAtRisk(...) => _inner.GetRacksAtRisk(origin);` pasan directo.
- Núcleo: `WorkspaceHostCore.cs:746-762` `DiscardDocument`: 750 `if (Authorize(origin) != RequestDecision.Accepted) return EditDiscardOutcome.Rejected(NotThisDocumentReason);`, 751 `if (_pilotReadInProgress) return EditDiscardOutcome.Busy(...)`, 753 `DiscardPilotEdits(origin.Origin)`, 754 `DropCloseNotice`, 757-761 `Failed`. 764-807 `GetRacksAtRisk`: memoria, sin autorización; 805 `return default;` (= `IsKnown` false). 520-536 `public EditDiscardOutcome DiscardPilotEdits(SessionId session)` (doc 515: `for the command that F4-T3 will expose`), pero el núcleo no se expone fuera del controlador.
- `EditDiscardOutcome` (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:294-333`): `Status`, `Reason`, `RemovedRackIds`, `PendingTextRackIds`; `EditDiscardStatus` (275-288) `Rejected, Discarded, Failed, Busy`. `PilotRiskView` (`IWorkspacePilotEditPort.cs:289-312`): `IsKnown`, `RackIds`, `UncertainRackIds`.
- Referencia del flujo existente en la UI: `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:368` `risks = _pilotEdit.GetRacksAtRisk(new WorkspaceRequest(session));`, 377-382 desconocido ≠ nada en riesgo, 384-389 nada en riesgo, 431 relectura, 469 `DiscardDocument`. Texto del veto: 1137-1140 (`" Descártalos con \"Descartar borradores de este documento\"; cada pestaña también se descarta con su \"Cerrar\"."`).

### 1.5 Patrones Sí/No ya existentes en el Plugin (todos fuera de `Workspace/`)

- `src/RackCad.Plugin/RackLayoutCommands.cs:149-155`: `new PromptKeywordOptions("\n¿Colocar la rejilla de todos modos?")`, `Keywords.Add("Si")`, `Keywords.Add("No")`, `Keywords.Default = "No"`, `AllowNone = true`, `editor.GetKeywords(options)`, `result.Status == PromptStatus.OK && string.Equals(result.StringResult, "Si", StringComparison.OrdinalIgnoreCase)`.
- `src/RackCad.Plugin/RackLayoutCommands.Fill.cs:498-514` `private static bool AskYes(Editor editor, string message, bool defaultYes)` (508 `PromptStatus.None` → default). Uso en 123.
- `src/RackCad.Plugin/RackCamaCommands.cs:45-53` (palabras `Dinamica`/`Pushback`, `AllowNone`, `Status != OK && != None` → salir).
- `src/RackCad.Plugin/Views/RackProjectionCommandPort.cs:49-70` (47-48 comentario: el constructor de dos argumentos exige la lista entre corchetes en el mensaje; 62-66 patrón `Keywords.Add`+`Default`+`AllowNone`).
- Convención medida: la palabra clave es `"Si"` sin tilde en los dos Sí/No.

---

## 2. Guardas y pruebas que fallarían o tendrían que cambiar

Notación: «falla» = la prueba real falla con el cambio de producto descrito; «pasa» = sigue pasando pero restringe el código; «condicional» = solo si el diseño hace lo indicado.

### 2(a) Segundo `[CommandMethod("RACKPANELDESCARTAR")]` en `WorkspacePanelCommand.cs` con `GetKeywords`, `PromptKeywordOptions`, `PromptStatus`, `WriteMessage` y llamadas a un núcleo de Application

**Fallan:**

1. `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs:49-56` `TheCommandAdapterHoldsNoOtherCommandAndNoAlias`: 54 `Assert.Equal(1, Src.Count(code, @"\[\s*CommandMethod\b"));` (pasa a 2). 55 `Assert.DoesNotMatch(new Regex(@"CommandFlags|\[assembly|CommandClass|LispFunction|IExtensionApplication"), code);` sigue valiendo si el atributo nuevo no lleva flags.
   Cambio mínimo declarado: el adaptador declara exactamente el conjunto de nombres `{RACKPANEL, RACKPANELDESCARTAR}`, cada uno una vez (por nombre, no por número), y 55 intacta.
2. `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs:320-330` `TheCommandStaysTheOnlyOneWithOneAttributeNoAliasAndTheSharedReporter`: 325 `Assert.Equal(1, Src.Count(code, @"\[\s*CommandMethod\b"));` (pasa a 2). 326 `Assert.Equal(1, Src.Count(code, @"\[CommandMethod\(""RACKPANEL""\)\]"));` sigue en 1. 327 `catch (System.Exception ex)`, 328 `RackCommandSupport.Report(ex)`, 329 `CommandFlags|CommandClass|LispFunction` siguen valiendo.
   Cambio mínimo: el mismo invariante por nombre (RACKPANEL una vez, RACKPANELDESCARTAR una vez, ningún otro), sin flags.
3. `tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs:165-175` `TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs`: aplica `HandlerForbiddenCallsFor(path)` (159-163) a todo `src/RackCad.Plugin/Workspace/`; 139-142 `ForbiddenCallNames` incluye `GetKeywords` (141) y 144 lo envuelve en `\b(?:...)\b`. 159-163 no exceptúa `CommandAdapterPath`, así que `editor.GetKeywords(` en el comando falla. `PromptKeywordOptions` y `PromptStatus` no están en la lista.
   Cambio mínimo: una excepción por ARCHIVO EXACTO para `CommandAdapterPath` que quite solo `GetKeywords` (p. ej. con el `ForbiddenCallsWithout` de 150-151 ya existente), con todos los demás nombres (GetString, GetSelection, GetEntity, SelectImplied, GetObject, Zoom, Regen, MessageBox, ShowDialog...) prohibidos también allí, comparada con `string.Equals(path, CommandAdapterPath, StringComparison.Ordinal)`.
4. `tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs:320-331` `NothingIsHookedWhenThePluginLoadsAndNoCommandOrInitializerIsAdded` vía `DeclaresEntryPoint` 340-351: 344 `var attributes = Regex.Matches(code, @"\[\s*CommandMethod\b").Count;`, 345 `var allowed = Regex.Matches(code, @"\[CommandMethod\(""RACKPANEL""\)\]").Count;`, 346 `if (attributes != 1 || allowed != 1) return true;`, 347 quita solo el de RACKPANEL; 333-334 `EntryPoints` incluye `\[CommandMethod`. Doc 336-339: `may hold ONE <c>[CommandMethod("RACKPANEL")]</c> and nothing else of this kind: no alias, no second command, no other name.`
   Cambio mínimo: en el archivo exacto del comando, exactamente un `RACKPANEL`, a lo sumo un `RACKPANELDESCARTAR` (o exactamente uno, ver 2(a)-condicional 1), ningún otro `[CommandMethod`, y quitar ambos atributos exactos antes de aplicar `EntryPoints`; actualizar el doc 336-339.
5. `tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs:106-117` `TheRealWorkspaceFilesPassTheRefinedGuard`: 113-114 `HandlerForbiddenCallsFor(path)` y 115 `DeclaresEntryPoint(path, code)` sobre los archivos reales. Falla por las dos causas de 3 y 4; el texto de la prueba no necesita cambiar si se refinan las dos funciones.
6. `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs:71-75` `TGrd08_ElCensoDeComandosEsElDeAperturaMasRackPropiedadesRprRackProyectarYRpy`: `CensusViolations` (194-216) añade `"sin clasificar en el censo de G7 y G15: " + extra` (212). Censo: 37-43 `OpeningCommands` (33), 46 `G7Commands`, 49 `G15Commands`, 52 `I65Commands`, 55 `internal static readonly string[] F1PanelCommands = { "RACKPANEL" };`, 58 `Baseline = OpeningCommands.Concat(G7Commands).Concat(G15Commands).Concat(I65Commands).Concat(F1PanelCommands).ToArray();`. Las registraciones se leen en bruto (176-183, `File.ReadAllText`, excluye segmentos `bin`/`obj`; regex 179 `\[CommandMethod\(""(?<name>[^""]*)""`).
   Cambio mínimo: un grupo nuevo con nombre (p. ej. de F4-T3C2) `{ "RACKPANELDESCARTAR" }` concatenado a `Baseline` en 58, y el doc de 57. Las mutaciones 89-106 siguen detectando porque se derivan de `Baseline`.
7. `tests/RackCad.Tests/SelectiveEditorOpenTests.cs:559-567` `GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: 562-564 cuenta en bruto `File.ReadAllText(file).Split("[CommandMethod(").Length - 1` en `src/RackCad.Plugin` (sin excluir `bin/obj`; hoy no hay `.cs` en `obj/` con esa subcadena: en disco también da 39), 566 `Assert.Equal(39, total);`. Doc 534-558: el número no es el invariante (lo es el censo por nombre de `CustomPropertiesCommandGuardTests`) y se reapunta con motivo (párrafos de I-54 G7, I-55 G15, I-65 F4, I-64 F1-T3A).
   Cambio mínimo: 39 → 40 con un `<para>` de I-64 F4-T3C2 que nombre `RACKPANELDESCARTAR` en `WorkspacePanelCommand.cs`. Al ser bruto, ningún comentario ni literal del Plugin puede contener la subcadena `[CommandMethod(` (lo dice el párrafo de I-65 F4, 550-551).

**Falla como consecuencia del cambio mínimo de 3:**

8. `tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs:250-258` `NoFolderWideExclusionWasIntroducedInTheT2Guard`: 257 `Assert.Equal(2, Src.Count(guard, @"string\.Equals\(path, (?:Reader|Command)AdapterPath, StringComparison\.Ordinal\)"));` sobre el código de `WorkspaceBridgeSourceGuardTests.cs`. Hoy las dos apariciones son 160 (`ReaderAdapterPath` en `HandlerForbiddenCallsFor`) y 342 (`CommandAdapterPath` en `DeclaresEntryPoint`). La excepción por archivo exacto de 3 añade una tercera.
   Cambio mínimo: el invariante «toda excepción del guard de T2 se compara por igualdad de ruta completa» (3 apariciones, o contadas por función), conservando 255 (sin `Skip`) y, en 228-236, 234 `Assert.DoesNotMatch(new Regex(@"\.(?:StartsWith|EndsWith|Contains)\s*\([^)]*Adapter"), guard);` y 233 (Facts|Effects = 2, sin cambio).

**Pasan, pero restringen el comando nuevo (todas leen el archivo con `Src.Code`, es decir, sin comentarios y CON literales):**

- `WorkspaceHostSourceGuardTests.cs:36-47` `RackPanelIsRegisteredExactlyOnceAndOnlyInTheCommandAdapter`: 40 cuenta solo el literal exacto `\[CommandMethod\(""RACKPANEL""\)\]`, así que `RACKPANELDESCARTAR` no cuenta. No hay guarda simétrica para el nombre nuevo.
- `WorkspaceHostSourceGuardTests.cs:58-63` ningún otro archivo de `Plugin/Workspace` declara `\[\s*CommandMethod\b`: el comando nuevo tiene que estar en `WorkspacePanelCommand.cs`.
- `WorkspaceHostSourceGuardTests.cs:65-73` `TheCommandOnlyObtainsTheControllerAndOpensThePanel`: 70 `Assert.Contains("WorkspaceController.GetOrCreate()", code);` y 71 `\.OpenPanel\s*\(\s*\)` siguen satisfechos por RACKPANEL; 72 `\bnew\s+WorkspaceController\b|PaletteSet|AddVisual|System\.Windows|AutoCadEventBridge|StartTransaction|LockDocument|SelectImplied|GetObject` aplica a todo el archivo. Ojo: su nombre deja de describir el archivo y NO protege que RACKPANELDESCARTAR no cree el controlador; ese invariante necesita una guarda nueva sobre el cuerpo del método nuevo (p. ej. `Src.BodyOf` sin `GetOrCreate` ni `OpenPanel`/`Show`).
- `WorkspaceHostSourceGuardTests.cs:75-82` exige `catch (System.Exception` y `RackCommandSupport.Report(` en el archivo.
- `WorkspaceHostSourceGuardTests.cs:84-89` `OnlyTheCommandAdapterWritesToTheCommandLine`: `\bWriteMessage\b` prohibido en todo archivo de `Plugin/Workspace` salvo el del comando.
- `WorkspaceHostSourceGuardTests.cs:106-117`: 112 `ModuleInitializer|IExtensionApplication|\[assembly` tras quitar solo `[CommandMethod("RACKPANEL")]` (116-117 `WithoutTheRackPanelAttribute`). El atributo nuevo no contiene esas palabras.
- `WorkspaceHostSourceGuardTests.cs:460-471`: 465-466 toda línea con `StartTransaction|StartOpenCloseTransaction|LockDocument (` debe contener `using`; 468 prohíbe líneas `^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:Transaction|DBObject|Document|Database|Entity|BlockReference|ObjectId|Editor)\b[^(]*[;=]` (p. ej. `private static Editor ActiveEditor => ...` falla; `private static bool AskYes(Editor editor, ...)` pasa).
- `WorkspaceBridgeSourceGuardTests.cs:205-214` (`ForbiddenFieldLines`, campo/propiedad con `Transaction|OpenCloseTransaction|DBObject|Document|Entity|BlockReference` antes de `=`/`;`/`{`) y 216-225 (`PublicSurfaceLeaks`: ninguna línea `public|protected` cuya firma nombre `Document|DocumentCollection|Editor|Database|Transaction|OpenCloseTransaction|DBObject|ObjectId|Entity|Autodesk` o `\w+EventArgs`): los ayudantes con `Editor` deben ser `private`.
- `WorkspaceBridgeSourceGuardTests.cs:228-234` transacciones/bloqueos solo dentro de `using`; 290-302 `PersistenceOrAuthoredWrite`; 353-358 ningún archivo del Plugin FUERA de `Workspace/` nombra `AutoCadEventBridge|RackCad\.Plugin\.Workspace|RackCad\.Application\.Workspace` (un ayudante Sí/No fuera de la carpeta no puede nombrar tipos del workspace).
- `WorkspaceSourceGuardExceptionTests.cs:72-84` (InlineData 74 `Command`), 195-213 (197) y 215-226 (217): para `Command` siguen prohibidos `SelectImplied`, `GetObject`, `SetImpliedSelection`, `SetCurrentView`. La excepción de 3 debe quitar SOLO `GetKeywords`.
- `WorkspaceSourceGuardExceptionTests.cs:36-50`: los 10 textos deben seguir dando `true`; con la regla «un RACKPANEL, a lo sumo un RACKPANELDESCARTAR, nada más» todos siguen en `true` (38 `RACKPANEL`+`RPN`, 42 `RACKPANELS`, 41 con `CommandFlags.Modal`...).
- `WorkspaceSourceGuardExceptionTests.cs:260-281`: los 12 nombres de prueba de `WorkspaceBridgeSourceGuardTests.cs` deben seguir existiendo (no renombrar 165-175 ni 320-331).
- `WorkspacePaletteHostSourceGuardTests.cs:299-309` el orden `WorkspaceController\.GetOrCreate\(\)\s*\.OpenPanel` antes de `WorkspacePaletteHost\.GetOrCreate\(\)\s*\.Show` (primer `Regex.Match`) sigue en RACKPANEL; 137-146 solo el host y el comando nombran `WorkspacePaletteHost`.
- `WorkspacePaletteHostSourceGuardTests.cs:311-318` (textos, ver §5).
- `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:609-622`: 617 incluye `WorkspacePanelCommand.cs`; 621 `Assert.DoesNotMatch(new Regex(@"WorkspaceNavigation|NavigationEcho|IWorkspaceNavigationPort"), Src.Code(path));` (leer `PilotEdit` no nombra nada de eso).
- `tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs:175-190`: 178-184 solo `IWorkspacePilotEditPort.cs`, `WorkspaceHostCore.cs`, `WorkspacePilotSupport.cs`, `WorkspaceController.cs` pueden contener `\bTake(?:Close|Loss)Notices\b` (195), en todo `src/RackCad.Plugin/` y `src/RackCad.Application/`.
- `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:566-571` nadie fuera de 4 dueños llama `.SetModalRackCadActive(` ni lo llama sobre el controlador; 631-646 `ShowModalWindow|ShowDialog|MessageBox` en las carpetas de workspace solo dentro de `using (... EnterPanelModal())`; 898 `[CommandMethod` prohibido en controlador, soporte y puerto.
- `tests/RackCad.Tests/PushBackRoundTripSourceGuardTests.cs:360-375` (nombres únicos sin distinguir mayúsculas; `AllCommandMethods` 65-85), `tests/RackCad.UI.Tests/CustomPropertiesHelpCensusTests.cs:90-105` (97 `registered.Count >= 37`; toda entrada de la ayuda está registrada), `tests/RackCad.Tests/RackProjectionCommandGuardTests.cs:81-94` (ningún nombre con `PROYECTAR` fuera de su archivo): pasan.

**Condicionales:**

1. `WorkspaceSourceGuardExceptionTests.cs:30-34`: 33 `Assert.False(DeclaresEntryPoint(Command, "class C { " + RackPanel + " public void M() {} }"));`. Falla si la regla nueva EXIGE los dos atributos; con «a lo sumo uno» no cambia. Si se exige, cambio mínimo: el texto inyectado lleva los dos.
2. `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:768-787` `TheVetoOfThePresentedSessionHasItsTextAndTheDiscardButtonAndNamesNoWayToApply`: 780-782 `Assert.Equal("No se cerró el documento: hay borradores sin aplicar en Nombre R-1, R-3. Descártalos con \"Descartar borradores de este documento\"; cada pestaña también se descarta con su \"Cerrar\".", row.Text);` y 785 `Assert.DoesNotContain("RACKPANELDESCARTAR", row.Text);`. La autorización del Owner que está junto a este mapa (`owner-f4t3c2-authorization.md`, líneas 74-80) pide que el aviso de veto nombre los dos caminos, incluido `RACKPANELDESCARTAR`: entonces 781 y 785 cambian (785 pasa a exigir que aparezca; 783-784 sin «Actualizar»/«Aplicar» se conservan). Otros textos del veto fijados: 805 (otro documento), 975 (`StartsWith("No se cerró otro documento"`), `WorkspacePilotCloseSyncViewTests.cs:365` (`StartsWith("No se cerró"`).
3. `tests/RackCad.UI.Tests/CustomPropertiesHelpCensusTests.cs:36-41` `TGrd08_ElCensoDeLaAyudaEsElDeAperturaMasRackPropiedadesYRackProyectar`: censo por PARES (Command, Alias) de `RackCommandReference.Commands` (`src/RackCad.UI/RackCommandReference.cs:26`). Hoy ni `RACKPANEL` está en la ayuda. Falla solo si la ayuda gana una entrada; además 99-102 exige que el alias también esté registrado (RACKPANELDESCARTAR no tiene alias).

### 2(b) `public static WorkspaceController Current` (o similar) sin `GetOrCreate`

No falla ninguna guarda medida si es una propiedad/método que lee el `Holder` existente. Restricciones vigentes sobre `WorkspaceController.cs`:

- `WorkspaceHostSourceGuardTests.cs:93-104`: 98 `sealed class WorkspaceController`, 99 `WorkspaceLazyInstance<WorkspaceController>`, 100 `Assert.Equal(1, Src.Count(code, @"\bnew\s+WorkspaceController\s*\("));`, 101-103 lambda, ctor privado y `public static WorkspaceController GetOrCreate(`.
- `WorkspaceHostSourceGuardTests.cs:119-126`: 124 prohíbe `static\s+readonly\s+WorkspaceController\b`, 125 prohíbe `static\s+WorkspaceController\s+\w+\s*=\s*new\b` (un campo estático que cachee la instancia falla; `Current => ...` no casa porque `=>` no es `= new`).
- `WorkspaceHostSourceGuardTests.cs:106-114`: 111 prohíbe `\bstatic\s+(?:Workspace\w+|AutoCadEventBridge)\s*\(\s*\)` (constructor estático). `public static WorkspaceController Current` no casa.
- `WorkspaceHostSourceGuardTests.cs:137-147` un solo `_bridge.Start(`; 184-196 y `WorkspacePaletteHostSourceGuardTests.cs:407-422`, `WorkspaceHostC2SourceGuardTests.cs:130-139`, `WorkspaceLifecycleGuardTests.cs:277-288` (285), `WorkspaceBrowserHostGuardTests.cs:140-151` (148), `WorkspaceNavigationHostTests.cs:666-679` (676), `WorkspacePilotEditGuardTests.cs:507`: exactamente 3 `+=` y 0 `-=`.
- `WorkspacePaletteHostSourceGuardTests.cs:424-432`: 430 `Assert.Equal(1, Src.Count(code, @"\bevent\b"));` (también `WorkspacePilotEditGuardTests.cs:509`).
- `WorkspacePaletteHostSourceGuardTests.cs:488-498`: 497 `\bTimer\b|\bThread\b|\bTask\b|\basync\b|Dispatcher|UnhandledException`; `WorkspacePilotEditGuardTests.cs:510`; `WorkspaceNavigationHostTests.cs:439-453` (445 incluye `ControllerPath`; 443 `\.Wait\s*\(|\.Result\b|GetAwaiter|...|Dispatcher|SendStringToExecute|\[CommandMethod|StartCommand|...|SetFocus|user32`).
- `WorkspacePilotEditGuardTests.cs:463-511`: 504 `Src.Count(controller, @"\bpublic\s+IWorkspacePilotEditPort\b") != 1`. Hueco del regex: `public static IWorkspacePilotEditPort ...` NO se contaría; si el acceso estático devolviera el puerto en vez del controlador, habría que declararlo.
- `WorkspaceHostSourceGuardTests.cs:149-160`: fuera de `src/RackCad.Plugin/Workspace/` y `src/RackCad.Application/Workspace/` nadie nombra `\bWorkspaceController\b` (la UI tampoco: `WorkspacePaletteHostSourceGuardTests.cs:518-529`).
- No hay censo de la superficie pública del controlador (el único censo de nombres públicos es el del host de la paleta, `WorkspacePaletteHostSourceGuardTests.cs:280-295`, `{ "GetOrCreate", "IsCreated", "Show" }`).

Hecho para el diseño: `WorkspaceLazyInstance<T>` no ofrece lectura sin crear. Las dos formas posibles tienen este impacto en guardas: (i) `Holder.IsCreated ? Holder.GetOrCreate() : null` dentro del controlador: ninguna guarda falla (el texto `GetOrCreate` queda en el controlador, no en el comando; la instancia nunca se libera); (ii) un miembro nuevo en `WorkspaceLazyInstance` (Application): no hay censo de sus miembros; aplican las reglas de 2(d) y las pruebas de comportamiento de `WorkspaceHostPolicyTests.cs:152-246`.

Adyacente (no es el censo de comandos, pero la autorización 81-93 lo asigna a T3C2): si el envoltorio del asesor cambia el constructor, fija texto exacto `WorkspaceLifecycleGuardTests.cs:283` `Assert.Equal(1, Src.Count(code, @"\.SetLifecycleAdvisor\s*\("));` y 284 `AssertInOrder(constructor, "_host = new WorkspaceHostCore(", "_bridge.SetLifecycleAdvisor(_host);");`; y `WorkspaceBridgeFacadeSourceGuardTests.cs:206-228` es un censo exacto de los miembros públicos de `EventBridgeCore` (227).

### 2(c) Archivo nuevo en `src/RackCad.Plugin/Workspace/`

Censos exactos que fallan (todos usan `Src.PluginFiles()`, recursivo, sin `bin/obj`):

1. `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs:750-777` `TheFirstPlacementAddsNoPersistenceIdentityAutoloadProfileOrSecurityMechanism`: 767-776 `Assert.Equal(new[] { ...9 rutas... }, Src.PluginFiles().Select(f => f.Path).OrderBy(...).ToArray());`
2. `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:791-798` `TheFoldersAndTheGuardsOfTheOlderTasksKeepTheirShape`: 794 `Assert.Equal(9, Src.PluginFiles().Count);`
3. `tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:29-40`: 38 `Assert.Equal(9, Src.PluginFiles().Count);` y 39 ningún archivo cuya ruta case `Browser|Summary`.
4. `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:804-817`: 808 `Assert.Equal(9, plugin.Length);`, 809 sin subcarpetas, 810 ningún nombre con `Intent`.
5. `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:681-691`: 686 `Assert.Equal(9, files.Length);`, 690 sin subcarpetas.

Cotas inferiores (no cambian): `WorkspaceHostSourceGuardTests.cs:554-568` (558-559 `Assert.Contains` de 5 rutas), `WorkspacePilotEditGuardTests.cs:879-911` (lista por nombre, 910 admite `WorkspaceExtra.cs`).

Cambio mínimo declarado: cada censo pasa a 10 / lista de 10 con el nombre exacto, o (mejor invariante) cota inferior por nombre más «cada archivo nuevo es un archivo exacto declarado de la tarea». Además el archivo nuevo hereda todas las barridas de la carpeta: sin `WriteMessage` (84-89), sin `[CommandMethod` (58-63, y `DeclaresEntryPoint`), sin `GetKeywords` y demás `ForbiddenCallNames` (sin excepción por archivo), `RackLog.` solo dentro de `LogFault` (`WorkspaceHostSourceGuardTests.cs:502-514`), y el bloque `namespace RackCad.Plugin.Workspace` (`tests/RackCad.Tests/NamespaceFolderGuardTests.cs:100-135`). Es decir, sacar el Sí/No a un archivo nuevo de la carpeta falla más guardas que dejarlo en el comando.

### 2(d) Archivo nuevo en `src/RackCad.Application/Workspace/`

No hay censo por número de `Src.ApplicationFiles()`. Cotas inferiores: `WorkspaceHostSourceGuardTests.cs:561-567`, `WorkspaceEditIntentGuardTests.cs:812-814`, `WorkspaceBridgeSourceGuardTests.cs:406-417` (410 `files.Count >= 5`).

Fallaría solo si:
- va en `Drafts/`: `tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs:43-51` `Assert.Equal(ExpectedFiles, DraftFiles().Select(f => f.Path).ToArray());` con 23-30 (5 archivos exactos). Cotas inferiores que no cambian: `WorkspaceLifecycleGuardTests.cs:661-684`, `WorkspacePilotEditGuardTests.cs:874-877`. Además `WorkspaceDraftPilotIntentGuardTests.cs:232-244` (ni `PushBack` ni `Pilot` ni `RackCad.Application.Systems`) y `WorkspaceDraftBoundaryTests.cs:420-450` (los archivos de Drafts no nombran tipos del espacio padre salvo `SessionId`).
- declara un tipo en `RackCad.Application.Workspace` cuyo nombre contenga `Subscription|Editor|Draft` (IgnoreCase) o `Tab`: `tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs:93-105` (101-102), bloqueada por la metaguarda `WorkspaceBrowserBoundaryTests.cs:296-311` y `284-294`.
- el nombre de un tipo nuevo aparece como palabra en los archivos del navegador (`WorkspaceBrowserBoundaryTests.cs:83-100`) o en los 5 de Drafts (`WorkspaceDraftBoundaryTests.cs:440-450`).
- nombra el piloto (`RackCad\.Application\.Systems\.PushBack\.Pilot|\bPushBackPilot\w*`) fuera de 3 archivos: `WorkspaceEditIntentGuardTests.cs:502-516`.
- miembros públicos con `ProjectSummary|Metric|Provider|TotalRacks|Aggregat|Bom\b`: `WorkspaceModelBoundaryTests.cs:80-91`.

Reglas de contenido de toda la carpeta: `WorkspaceModelBoundaryTests.cs:26-37` (sin `Autodesk.`, `System.Windows`, `RackCad.Plugin`, `RackCad.UI`) y 41-49 (`System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32|\bRegistry\.|\bUserSettings\b|\bAtomicFile\b|Transaction|LockDocument|\bDatabase\b|\bCommit\(|\bAutodesk\.`, CON literales); `WorkspaceHostSourceGuardTests.cs:495-500` (sin `RackLog` ni `RackCad.Application.Diagnostics`), 376-390, 392-415, 417-437, 451-458, 524-529, 545-552; `WorkspaceBridgeSourceGuardTests.cs:362-372`, 388-394; `WorkspaceSourceGuardExceptionTests.cs:238-248`; `WorkspaceNavigationHostTests.cs:435`, 451-452; `WorkspacePilotCloseSyncGuardTests.cs:175-190`. Si se tocan literales de `WorkspaceHostCore.cs`/`WorkspacePilotSupport.cs`: `WorkspaceHostHardeningGuardTests.cs:395-473` cuenta cada literal español exacto (p. ej. 401 `"La petición no es del documento activo."`, 6 veces). Si se añade una operación a `IWorkspacePilotEditPort`: `WorkspacePilotEditGuardTests.cs:90-123` es cota inferior (95), pero 73/99 prohíben `\bDraft\w*|\bPushBack\w*|...` en el archivo del puerto (con literales). `IWorkspaceHostPort` es de solo lectura: `WorkspaceHostPortGuardTests.cs:24-28` (`WriteLikeName` incluye `Delete|Remove|Clear|...`).

Cambio mínimo: ninguno si el archivo va directamente en `Workspace/`, con un tipo sin `Draft/Editor/Subscription/Tab` en el nombre; si va en `Drafts/`, la lista exacta 23-30 de `WorkspaceDraftBoundaryTests` gana el nombre.

### 2(e) Archivo de pruebas nuevo en `tests/RackCad.Tests/Workspace/`

No hay censo de archivos de prueba ni de número de pruebas por carpeta. Los únicos censos de número de pruebas son del ARCHIVO `WorkspaceHostSourceGuardTests.cs` (hoy 45, medido con ambos regex):
- `WorkspacePaletteHostSourceGuardTests.cs:641-660`: 659 `Assert.Equal(45, Src.Count(code, @"\[(?:Fact|Theory)\]"));` sobre `Src.CodeOnly` (sin comentarios).
- `WorkspaceLifecycleGuardTests.cs:797` `Assert.Equal(45, Src.Count(Src.Text("tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs"), @"\[(?:Fact|Theory)\b"));` sobre el texto CRUDO (cuenta también comentarios y `[Fact(` con argumentos).
Por eso añadir pruebas a `WorkspaceHostSourceGuardTests.cs` rompe ambas; ponerlas en un archivo nuevo no. Otras metaguardas que leen ese archivo y exigen texto literal: `WorkspacePaletteHostSourceGuardTests.cs:644-657` (G1/G2/G3, sin `Skip`), `WorkspaceIdleSchedulingGuardTests.cs:309-319`. Y `WorkspaceSourceGuardExceptionTests.cs:228-281` lee `WorkspaceBridgeSourceGuardTests.cs`.

Reglas de contenido para cualquier archivo nuevo de pruebas Core: `NamespaceFolderGuardTests.cs:210-248` (exactamente una línea `namespace ` y igual a `RackCad.Tests`; el `;` final se recorta, así que `namespace RackCad.Tests;` vale); `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs:261-269` (INV-14: `ramelements`, y `python|bentley` sin mayúsculas, en el código de toda prueba Core no eximida) y `StructuralAnalysisRamElementsHardeningGuardTests.cs:84-92` (sin literales partidos de esas palabras). Convención de filtros: las clases nuevas empiezan por `Workspace` (`WorkspacePilotCloseSyncGuardTests.cs:964-984`).

---

## 3. Censo global de `[CommandMethod(` y pruebas que cuentan comandos

- `git grep -o -F "[CommandMethod(" -- 'src/*.cs' | wc -l` = 39; todas en `src/RackCad.Plugin` (en disco, incluido `obj/`, también 39). Con RACKPANELDESCARTAR: 40.
- Nombres (39): QCB, QCM, QUICKCABECERA, QUICKCAMA, RA, RACKAYUDA, RACKBOMTOTAL, RACKCABECERA, RACKCAD, RACKCANTILEVER, RACKDUPLICAR, RACKEDITAR, RACKLAYOUT, RACKLISTA, RACKPANEL, RACKPROPIEDADES, RACKPROYECTAR, RACKPUSHBACK, RACKRAMPAQUETE, RACKRELLENAR, RACKSECCION, RACKSELECTIVO, RACKSISTEMADINAMICO, RACKVARIABLES, RB, RCB, RCT, RD, RED, RK, RL, RLY, RPB, RPR, RPY, RR, RS, RSD, RVA.
- Con «RACKPANEL» en el nombre: solo `RACKPANEL`, `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:14`. Ninguna regla de prueba filtra por la subcadena `PANEL` (las de subcadena son `PROYECTAR` en `RackProjectionCommandGuardTests.cs:91-93` y `PROPIEDADES`/`PROYECTAR` en la ayuda, `CustomPropertiesHelpCensusTests.cs:51`, 62).
- Pruebas que cuentan o clasifican comandos:
  - `SelectiveEditorOpenTests.cs:559-567` (bruto, 39) — cambia.
  - `CustomPropertiesCommandGuardTests.cs:62-75` (censo por nombre) y 77-87 (RACKPROPIEDADES/RPR solo en su archivo) — 71-75 cambia.
  - `CustomPropertiesHelpCensusTests.cs:36-41` (pares de la ayuda) y 90-105 (`>= 37`) — pasan salvo 2(a)-condicional 3.
  - `PushBackRoundTripSourceGuardTests.cs:360-375` (duplicados `OrdinalIgnoreCase`) — pasa.
  - `RackProjectionCommandGuardTests.cs:66-94` — pasa.
  - Por archivo propio: `StructuralAnalysis/StructuralAnalysisPluginCommandGuard.cs:141-150` y `StructuralAnalysisPluginCommandGuardTests.cs:302-310` (solo `RackRamElementsCommands.cs`); `CantileverPluginSourceGuardTests.cs:263-264`, `MainMenuStructuralSectionAccessGuardTests.cs:161`, `ProjectVariablesWorkspaceTests.cs:703`, `PushBackPluginSourceGuardTests.cs:50-53`, `StructuralSectionPluginSourceGuardTests.cs:148`, `CustomPropertiesEdgeGuardTests.cs:328` (solo el ejecutor) — pasan.
  - Workspace: `WorkspaceHostSourceGuardTests.cs:36-63`, `WorkspacePaletteHostSourceGuardTests.cs:320-330`, `WorkspaceBridgeSourceGuardTests.cs:320-351`, `WorkspaceSourceGuardExceptionTests.cs:30-61`; prohibiciones de `CommandMethod` en otros alcances: `WorkspaceHostSourceGuardTests.cs:448` (host de navegación), `WorkspaceNavigationHostTests.cs:443`, `WorkspaceBrowserHostGuardTests.cs:256-264` (UI, host de navegación y hechos), `WorkspacePilotSurfaceGuardTests.cs:280` y `WorkspacePilotCloseSyncGuardTests.cs:851` (UI), `WorkspacePilotEditGuardTests.cs:898` (controlador, soporte, puerto).

---

## 4. Ayudantes de prueba para guardas de texto del Plugin

`tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs` (`internal static class WorkspaceHostSource`, 15; alias habitual `using Src = RackCad.Tests.WorkspaceHostSource;`):
- 17 `PluginWorkspace = "src/RackCad.Plugin/Workspace/"`, 18 `ApplicationWorkspace = "src/RackCad.Application/Workspace/"`.
- 20-27 `RepoRoot()` (sube hasta `RackCad.sln`).
- 29-30 `CodeOnly(string text)`: `Regex.Replace(Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline), @"//[^\r\n]*", "")`. Quita comentarios de bloque y de línea (incluido `///`); NO quita literales. Efectos laterales: un `//` o `/*` dentro de un literal corta el resto de la línea/bloque.
- 32 `Exists`, 34-39 `Code(relative)` = `CodeOnly(File.ReadAllText)` con `Assert.True(File.Exists...)`, 41-46 `Text(relative)` crudo.
- 48-58 `Files(folder)`: `Directory.GetFiles(dir, "*.cs", AllDirectories)` en disco, excluye `\obj\` y `\bin\`, rutas relativas con `/`, ordenadas `Ordinal`, código con `CodeOnly`; carpeta inexistente → vacío. 60 `PluginFiles()`, 62 `ApplicationFiles()`, 64 `AllProduct()` = `Files("src")`.
- 67-89 `BodyOf(code, signaturePattern)`: desde la firma hasta la llave que cierra (cuenta `{`/`}` sin mirar literales: una llave dentro de un literal descuadra) o, si `=>` va antes que `{`, hasta el primer `;`. `Assert.True(match.Success, "member not found: ...")`.
- 91 `Count(code, pattern)` = `Regex.Matches(code, pattern).Count`.

`WorkspaceBridgeSourceGuardTests.cs` tiene copias privadas: 16-23 `RepoRoot`, 25-26 `CodeOnly` (idéntico), 31-40 `SourcesIn` (sin ordenar), 42-46 `PluginWorkspace/ApplicationWorkspace/AllProduct`; expone `internal` 53-54 `CommandAdapterPath`/`ReaderAdapterPath`, 62-64 `FactsAdapterPath`/`EffectsAdapterPath`/`HostAdapterPath`, 159-163 `HandlerForbiddenCallsFor(string path)`, 340-351 `DeclaresEntryPoint(string path, string code)`.

`PluginSourceCode` (`tests/RackCad.Tests/SelectiveDuplicationFailClosedTests.cs:586-...`, `internal static class` en `namespace RackCad.Tests`): 611 `Mask(source)` = código sin comentarios NI contenido de literales (mismas longitudes y saltos), 614-619 `StringLiterals(source)`, 622-639 `Members(code)` (miembros con modificador y cuerpo), 642-654 `Calls(code, name)`, 660-685 `Reaches(members, text, names)`. Limitación declarada 591-592: no entiende comillas dentro de huecos de cadenas interpoladas. Lo usan `CustomPropertiesCommandGuardTests.cs:234-235` y `CustomPropertiesEdgeGuardTests.cs:284`.

Otros por archivo: `WorkspacePilotEditGuardTests.cs:38` `Squash`, 40 `TryBody` (nulo si no hay firma); `WorkspaceHostHardeningGuardTests.cs:26` `Squash`, 36 `BodyOrNull`; `WorkspacePilotCloseSyncGuardTests.cs:34-40` `CallCount` (llamadas calificadas o no, que no sean la declaración) y sus `CallsOutside`/`RequiredBody`; `WorkspaceLifecycleGuardTests` `AssertInOrder`.

---

## 5. Listas de palabras prohibidas que verían los textos en español del comando

Todas se aplican sobre `Src.Code`/`CodeOnly` (literales incluidos) de `WorkspacePanelCommand.cs`, salvo que se diga otra cosa.

- `WorkspacePaletteHostSourceGuardTests.cs:317` `new Regex(@"no\s+muestra|aun\s+no|todavia\s+no|not\s+shown", RegexOptions.IgnoreCase)`. Sin `\b`: casa dentro de otras palabras (p. ej. «aun nombre»). Con tilde («aún no», «todavía no») no casa.
- `WorkspacePaletteHostSourceGuardTests.cs:316` `PaletteSet|AddVisual|System\.Windows|RackCad\.UI|Autodesk\.AutoCAD\.Windows|UserControl|Dispatcher` (sin `\b`).
- `WorkspaceHostSourceGuardTests.cs:72` `\bnew\s+WorkspaceController\b|PaletteSet|AddVisual|System\.Windows|AutoCadEventBridge|StartTransaction|LockDocument|SelectImplied|GetObject`.
- `WorkspaceHostSourceGuardTests.cs:55` `CommandFlags|\[assembly|CommandClass|LispFunction|IExtensionApplication`; `WorkspacePaletteHostSourceGuardTests.cs:329` `CommandFlags|CommandClass|LispFunction`.
- I-63 (IgnoreCase, toda la carpeta Plugin y Application): `WorkspaceHostSourceGuardTests.cs:548` `ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom`; `WorkspaceBridgeSourceGuardTests.cs:362-363` la misma. Casos en español: «metrica»/«geometrica»/«simetrica» sin tilde casan `Metric`; «bomba» casa `\bBom`; «métrica» con tilde no.
- `WorkspaceBridgeSourceGuardTests.cs:139-142` (`\b...\b`, todo `Plugin/Workspace`): `SetImpliedSelection|SetCurrentView|ZoomExtents|ZoomWindow|Zoom|PanTo|UpdateScreen|Regen|SendStringToExecute|StartCommand|GetSelection|GetEntity|GetString|GetPoint|GetKeywords|GetInteger|GetDouble|SelectImplied|SelectAll|SelectAtPoint|ShowModalDialog|ShowModelessDialog|ShowDialog|MessageBox|GetObject|OpenCloseTransaction|Highlight|Unhighlight`.
- `WorkspaceHostSourceGuardTests.cs:379` `\b(?:SetImpliedSelection|SetCurrentView|SetSystemVariable|Zoom\w*|Regen|UpdateScreen|Highlight|Unhighlight|SendStringToExecute|SetImpliedSelectionAsync|PanTo)\b`.
- `WorkspaceHostSourceGuardTests.cs:395` `Autodesk\.AutoCAD\.Internal|PaletteSet|AddVisual|System\.Windows|RackCad\.UI|Autodesk\.AutoCAD\.Windows|\bUserControl\b|\bDispatcher\b`; `WorkspaceBridgeSourceGuardTests.cs:393` y `WorkspacePaletteHostSourceGuardTests.cs:277` (`Autodesk\.AutoCAD\.Internal|SetFocusToDwgView|IKeyboardInputSink|DllImport`).
- `WorkspaceHostSourceGuardTests.cs:420` `Task\.Run|Task\.Factory|\bnew\s+Thread\b|ThreadPool|System\.Threading\.Timer|System\.Timers|\bTimer\b|\basync\b|\bawait\b|Parallel\.|BackgroundWorker|SynchronizationContext`; 426 `GetAwaiter|ConfigureAwait|\.Wait\s*\(`; 436 `\.Result\b` (en `PromptResult` usar `.StringResult`/`.Status`, que no casan); `WorkspaceSourceGuardExceptionTests.cs:244` y `WorkspaceNavigationHostTests.cs:435` `\basync\b|\bawait\b`.
- `WorkspaceHostSourceGuardTests.cs:454` `System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32|\bRegistry\b|\bXrecord\b|\bXData\b|\bDBDictionary\b|NamedObjectsDictionary|AtomicFile|UserSettings|Environment\.SetEnvironmentVariable`.
- `WorkspaceBridgeSourceGuardTests.cs:290-293` `System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32|\bRegistry\.|\bUserSettings|\bAtomicFile\b|NamedObjectsDictionary|GetNamedObjectsDictionary|\bDBDictionary\b|\bXrecord\b|\bXData\b|\bXdata\b|\bResultBuffer\b|AppendEntity|UpgradeOpen|DowngradeOpen|\.Commit\(|\.Erase\(|SetXDictionary|SaveAs|\bSave\(|DocumentExtension`.
- `WorkspaceBridgeSourceGuardTests.cs:333-334` `IExtensionApplication|ExtensionApplication|\[CommandMethod|\[assembly\s*:|ModuleInitializer|static\s+AutoCadEventBridge\s*\(` (tras quitar los atributos permitidos).
- `WorkspaceHostSourceGuardTests.cs:492` `RackLog\.Warning`; 512 `RackLog\.` fuera de `LogFault`; 528 `\b(?:class|interface|struct|record)\s+\w*Log\w*\b`; 371 `Guid\.NewGuid|new\s+RackEmbedDocument|RackEmbedComposer|RackBlockData\.Write`; 293 `\bSelectImplied\b|\bGetObject\b|RackBlockData|RackEmbedStore|WorkspaceDefinitionCache`.
- `WorkspaceNavigationHostTests.cs:621` `WorkspaceNavigation|NavigationEcho|IWorkspaceNavigationPort`; 517-518 `ReturnFocusToDrawing`.
- `WorkspacePilotCloseSyncGuardTests.cs:195` `\bTake(?:Close|Loss)Notices\b`; `WorkspaceBridgeFacadeSourceGuardTests.cs:41-42` `WorkspaceSessionRegistry`; `WorkspacePilotEditGuardTests.cs:569-570` `\.SetModalRackCadActive\s*\(`, 634 `\b(?:ShowModalWindow|ShowDialog)\s*\(|\bMessageBox\b` fuera del ámbito modal.
- Todo `src/RackCad.Plugin/` (código sin comentarios): `StructuralAnalysisSourceGuardTests.cs:393-394` G1 `RamElements|ramelements|RAM Elements|\bRam[A-Z]` (sensible a mayúsculas).
- Solo si se añaden textos a la tabla H-07 del host/soporte: `WorkspaceHostHardeningGuardTests.cs:440-442` `Transaction|Database|Commit\(|RackLog|Provider|Metric|LockDocument|Aggregat|\bBom|TotalRacks|ProjectSummary|ComputedParameter|proveedor|metrica|métrica` (IgnoreCase).
- Textos de la UI (si cambia el aviso de veto): `WorkspacePaletteHostSourceGuardTests.cs:592-593` (`System\.IO|\bFile\.|\bDirectory\.|Registry|UserSettings|RackLog|XData|Xrecord` y `Metric|Provider|\bBom|ProjectSummary|TotalRacks|Aggregat|ComputedParameter` IgnoreCase), `WorkspacePilotSurfaceGuardTests.cs:280-283` (`CommandMethod`, `Autodesk`, `MessageBox`, `\bTask\b`... en la UI); `RACKPANELDESCARTAR` no casa ninguna.

---

## 6. Correcciones al mapa anterior (sobre 7895f397)

- `WorkspaceHostSourceGuardTests.cs`: 393-437 → dos pruebas, 392-415 (tipos UI, 395) y 417-437 (hilos/async, 420, 426, 436). Resto correcto (36-47, 49-56, 58-63, 65-73, 84-89).
- `WorkspacePaletteHostSourceGuardTests.cs`: 763-777 → la prueba es 750-777 y la lista 767-776; 659 y 320-330/325, 311-318, 299-309 correctos.
- `WorkspaceSourceGuardExceptionTests.cs`: 229-236 → 228-236; 250-258 correcto, pero falta que cambia por consecuencia (2(a)-8).
- `WorkspaceBridgeSourceGuardTests.cs`: 139-175 correcto (la constante es 139-142, el selector 159-163, la prueba 165-175); 340-351, 205-214, 216-225, 362-372 correctos.
- «Pasaban»: `PushBackRoundTripSourceGuardTests.cs` 360-375 (Fact en 360), `CustomPropertiesHelpCensusTests.cs` 90-105, `RackProjectionCommandGuardTests.cs` 81-94, `WorkspaceBrowserHostGuardTests.cs` 256-264 (no lee el comando: UI, host y hechos), `WorkspaceNavigationHostTests.cs` 442-443 con la lista de archivos en 445 (no incluye el comando).
- `WorkspaceNavigationHostTests.cs:613-618` → 609-622 (lista 612-620, aserción 621).
- `WorkspacePilotCloseSyncGuardTests.cs` (Take(Close|Loss)Notices) → 175-190, lista 178-184, regex 195.
- Patrones Sí/No: `RackCamaCommands.cs` 45-53 y `RackProjectionCommandPort.cs` 49-70 (incluyen la comprobación del estado).
- Faltaban: los otros tres censos `Equal(9, ...)` de `Plugin/Workspace` (`WorkspaceBrowserHostGuardTests.cs:38`, `WorkspaceEditIntentGuardTests.cs:808`, `WorkspaceNavigationHostTests.cs:686`); el censo exacto de Drafts (`WorkspaceDraftBoundaryTests.cs:46`); el veto con `DoesNotContain("RACKPANELDESCARTAR")` (`WorkspacePilotCloseSyncTests.cs:785`); la diferencia de regex entre los dos censos de 45 (659 sin comentarios y `\]`, 797 crudo y `\b`); el hueco `public static IWorkspacePilotEditPort` de `WorkspacePilotEditGuardTests.cs:504`.

---

## Censos que cambian

Definitivos con (a) + (b) tal como se describen (comando en el mismo archivo, `GetKeywords` en ese archivo, acceso estático leyendo el `Holder`):

1. `WorkspaceHostSourceGuardTests.cs:54` → hay 2 `[CommandMethod`, no 1 → el adaptador declara exactamente `{RACKPANEL, RACKPANELDESCARTAR}`, cada uno una vez, sin alias ni flags.
2. `WorkspacePaletteHostSourceGuardTests.cs:325` → ídem → el mismo invariante por nombre; 326 (RACKPANEL = 1) y 327-329 se conservan.
3. `WorkspaceBridgeSourceGuardTests.cs:165-175` (vía 139-142 y 159-163) → `GetKeywords` está prohibido en toda la carpeta y el comando no tiene excepción → excepción por archivo exacto `CommandAdapterPath` que exime solo `GetKeywords`; todo lo demás sigue prohibido allí.
4. `WorkspaceBridgeSourceGuardTests.cs:320-331` (vía `DeclaresEntryPoint` 340-351) → `attributes != 1` → exactamente un RACKPANEL, a lo sumo (o exactamente) un RACKPANELDESCARTAR, ningún otro `[CommandMethod`; quitar ambos antes de `EntryPoints`; doc 336-339.
5. `WorkspaceSourceGuardExceptionTests.cs:106-117` → aplica 3 y 4 a los archivos reales → sin cambio de texto: pasa cuando se refinan las dos funciones (añadir autopruebas del refinamiento en 36-61/72-84).
6. `WorkspaceSourceGuardExceptionTests.cs:257` → la excepción de 3 añade un tercer `string.Equals(path, CommandAdapterPath, StringComparison.Ordinal)` → «toda excepción del guard de T2 es por igualdad de ruta completa» (3 apariciones), manteniendo 234 y 255.
7. `CustomPropertiesCommandGuardTests.cs:71-75` (vía 55/58) → nombre sin clasificar → grupo nuevo con nombre `{ "RACKPANELDESCARTAR" }` concatenado a `Baseline` (por nombre, no por número).
8. `SelectiveEditorOpenTests.cs:566` → censo bruto 39 → 40, con párrafo de I-64 F4-T3C2 (el invariante sigue siendo el censo por nombre).

Condicionales:

- `WorkspaceSourceGuardExceptionTests.cs:33` → solo si la regla exige los dos atributos → el texto inyectado lleva los dos.
- `WorkspacePilotCloseSyncTests.cs:781` y `:785` (UI) → la autorización pide que el aviso de veto nombre `RACKPANELDESCARTAR` → texto exacto nuevo y `Contains` en lugar de `DoesNotContain`; 783-784 se conservan.
- `CustomPropertiesHelpCensusTests.cs:36-41` → solo si la ayuda gana una entrada → par nuevo con nombre (y 99-102 exige alias registrado).
- (c) archivo nuevo en `Plugin/Workspace` → `WorkspacePaletteHostSourceGuardTests.cs:767-776`, `WorkspaceLifecycleGuardTests.cs:794`, `WorkspaceBrowserHostGuardTests.cs:38`, `WorkspaceEditIntentGuardTests.cs:808`, `WorkspaceNavigationHostTests.cs:686` → 10 / lista de 10 por nombre exacto (o cota inferior por nombre).
- (d) archivo en `Drafts/` → `WorkspaceDraftBoundaryTests.cs:46` → lista exacta con el nombre nuevo; tipo con `Draft/Editor/Subscription/Tab` en `RackCad.Application.Workspace` → `WorkspaceModelBoundaryTests.cs:101-102` (bloqueada por `WorkspaceBrowserBoundaryTests.cs:296-311`) → no declarar ese nombre.
- (e) pruebas añadidas DENTRO de `WorkspaceHostSourceGuardTests.cs` → `WorkspacePaletteHostSourceGuardTests.cs:659` y `WorkspaceLifecycleGuardTests.cs:797` → 45 → n (ponerlas en un archivo nuevo no toca ningún censo).
- Adyacente (asesor envuelto): `WorkspaceLifecycleGuardTests.cs:283-284` fija `_bridge.SetLifecycleAdvisor(_host);` literal → invariante «el asesor se instala una vez, tras crear el host, sin suscripciones nuevas».
