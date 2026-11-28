# Mapa 3: motivos del Plugin, aviso de veto y menores (F4-T3C2)

Medido en el worktree, rama `architecture/workspace-persistente-rackcad`, HEAD `e89948b9`. Entre `7bd275e4` y HEAD no cambia nada
en `src/` ni en `tests/` (`git diff --stat 7bd275e4 e89948b9 -- src tests` vacío). Entre `7895f397` y HEAD tampoco cambian
`src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs` ni `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`, así que los números
de R-C14 siguen valiendo línea a línea. Las salvedades están abajo: 251 no es un literal y ReadPreviewWitness tiene 4 literales, no 2.
`git ls-files tests | grep /bin/|/obj/` = 0: las búsquedas de `git grep` en tests/ ya excluyen bin/ y obj/.

Helper de las guardas: `tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs:29-30` `CodeOnly` quita `/* */` y `//`, y
`Src.Code` (34-39) lo aplica; los literales entre comillas SÍ quedan dentro de `Code`. `Src.Text` (41-46) es el archivo crudo, con
comentarios.

---

## PARTE A: motivos en inglés del Plugin que llegan al campo del piloto

### A.1 Literales medidos en HEAD

`src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`, clase anidada `PilotIntentReader` (196-254), método
`ReadOnActiveDocument` (220-253):

- 224: `return EditIntentReadResult.NoActiveDocument("There is no active document.");`
- 226: `return EditIntentReadResult.Busy("A command is in progress.");`
- 236: `return EditIntentReadResult.Rejected("The request does not belong to the active document.");`
- 230: `return EditIntentReadResult.Failed(catalog.FailureReason);`. **No es un literal**: es el conducto de los motivos de `LoadCatalog` (80 y 85).
- 251: `: EditIntentReadResult.Failed(read.FailureReason);`. **No es un literal**: es el conducto de los motivos de `Read` (112, 118, 127, 131 y 135). R-C14 cita «251» entre los literales del host; el literal que traduce no está en esa línea.
- 216 (en `ReadIntent`, 207-218): `return EditIntentReadResult.Failed(ex.Message);`, después de `LogFault("WorkspaceNavigationHost.ReadEditIntent", ex);` (215). No es un literal: lleva el mensaje en tiempo de ejecución de cualquier excepción del lector (AutoCAD, `_authorize`, `_targets.Revalidate`…).

`src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`:

- `LoadCatalog` (74-87):
  - 80: `? new RackPilotCatalogResult(null, "The catalog is not available.")`
  - 85: `return new RackPilotCatalogResult(null, "The catalog could not be loaded: " + ex.Message);` (prefijo + `ex.Message`)
- `Read` (94-142):
  - 112: `return new RackPilotIntentReadResult(null, "The preview witness has no key.");`
  - 118: `return new RackPilotIntentReadResult(null, "The scan of the drawing carries no raw envelopes.");`
  - 127: `return new RackPilotIntentReadResult(null, "The document is locked by another operation.");` (catch `LockViolation`, 125)
  - 131: `return new RackPilotIntentReadResult(null, "The drawing could not be read: " + ex.Message);` (catch `Autodesk.AutoCAD.Runtime.Exception`, 129); prefijo + `ex.Message`
  - 135: `return new RackPilotIntentReadResult(null, "The edit intention could not be read: " + ex.Message);` (catch `System.Exception`, 133); prefijo + `ex.Message`
- `ReadPreviewWitness` (149-189) **no tiene llamadores**. `git grep ReadPreviewWitness -- src` solo devuelve 67 (doc) y 149 (definición). Tiene 4 literales, no 2:
  - 157: `"The definition has no handle."`
  - 174: `"The document is locked by another operation."` (**el mismo texto que 127**)
  - 178: `"The preview witness could not be read: " + ex.Message`
  - 182: `"The preview witness could not be read: " + ex.Message`
  
  R-C14 nombra 157 y 174; 178 y 182 están en el mismo método y deben quedar fuera también.
- 200 (`AssertNoOpenTransaction`, `[Conditional("DEBUG")]`): `"The read of the edit intention found a transaction open " + moment + " it ran."`. Es solo un Debug.Assert y no llega al campo.

Los conductos: `RackPilotCatalogResult` (13-27) y `RackPilotIntentReadResult` (29-43) guardan `FailureReason = failureReason ?? string.Empty`
(19, 35). El motivo nunca es null; puede ser `""`.

### A.2 Cadena completa hasta el campo (lector → host → resultado → UI)

1. Gesto en la UI: `WorkspaceBrowserViewModel.BeginFieldEdit` (`src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:552-568`, llama a `Field.BeginEdit()` en 563). `BeginFieldEditFromBox` (575-579) y `BeginFieldEditFromTab` (585-591) delegan en él.
2. `WorkspacePilotFieldViewModel.BeginEdit` (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:191-239`):
   - 202: `var request = new EditIntentRequest(session, rack, null, null);`. **El testigo siempre es null.**
   - 203: `var outcome = stale ? _port.TryReload(request) : _port.TryBeginEdit(request);`
   - 205-212: si la superficie cambió, no se muestra el motivo (`"La superficie cambió durante la lectura; el resultado no se mostró."`).
   - 227-229: `default: _noticeText = Describe(outcome.Status, outcome.Reason);`
   - `Describe` (447-459):
     - `"En espera: " + detail` (Waiting)
     - `"No disponible: "` (Unavailable)
     - `"Rechazado: "` (Rejected)
     - `"Sin borrador: "` (NotFound)
     - `"Fallo: "` (Failed)
     - `detail = string.IsNullOrWhiteSpace(reason) ? "sin detalle." : reason`
   - Se muestra en `NoticeText` (128). XAML: `src/RackCad.UI/Workspace/WorkspacePanelView.xaml:208` `BrowserFieldNoticeText` `Text="{Binding Browser.Field.NoticeText}"`.
3. Plugin: `WorkspaceController.PilotEditAdapter` (`src/RackCad.Plugin/Workspace/WorkspaceController.cs:234-269`; se construye en 49 con `new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth))`). Reenvía: `TryBeginEdit` 243, `TryCheckNow` 245, `TryReload` 247.
4. `WorkspacePilotEditGate` (`src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:153-195`): dentro de un aviso responde `Waiting(InsideNoticeReason)` (167-174); si no, reenvía.
5. Puerto explícito del host (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs`):
   - `TryBeginEdit` 553-565 → `TryBeginPilotEdit` (280-358)
   - `TryReload` 581-593 → `TryReloadPilotEdit` (438-512)
   - Con una lectura en curso responde `Waiting(PilotReadInProgressReason)`.
6. Puerta previa `PassGate` (1034-1068), que corta antes del lector con motivos en español:
   - 1039 `"No hay un lector de la intención de edición asignado."` (Unavailable)
   - 1045 `"No hay documento activo."` (Waiting)
   - 1051 `"Hay una ventana de RackCad abierta."` (Waiting)
   - 1057 `"Hay un comando en curso."` (Waiting)
   - 1063 `"La petición no es del documento activo."` (Rejected)
   
   Por eso 224, 226 y 236 solo llegan si el estado cambia entre la puerta y el lector, o cuando el lector ve `CommandInProgress` no vacío y la sonda (`HostProbe.IsActiveDocumentQuiescent`, `WorkspaceController.cs`) decía reposo. Llegan, pero como defensa.
7. La lectura única y su traducción de estado (`EditIntentReadStatus`, `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:21-28`; fábricas 56-62):
   - Intención, `TryBeginPilotEdit` 311-323:
     - `NoActiveDocument` y `Busy` → 318 `return EditIntentOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");`
     - `Rejected` → 320 `return EditIntentOutcome.Rejected(read.Reason ?? "La petición no es del documento activo.");`
     - `default` (`Failed`) → 322 `return EditIntentOutcome.Failed(read.Reason ?? "Falló la lectura de la intención de edición.");`
   - Recarga, `TryReloadPilotEdit` 470-482: 477 y 479 iguales a 318 y 320; 481 `read.Reason ?? "Falló la lectura para la recarga."`.
   - «Comprobar ahora», `TryCheckPilotEdit` 392-404: 399, 401 y 403 (`"Falló la relectura."`) → `EditCheckOutcome`. **Ningún llamador de la UI**: `git grep TryCheckNow -- src` solo da el puerto, la puerta y el adaptador. Hoy no llega al campo.
   - Como el lector del Plugin nunca da `Reason` null, **los textos de reserva `?? "…"` nunca se usan con el lector real**. El motivo inglés pasa tal cual. Un `FailureReason` vacío llega como `""` y el campo muestra `"Fallo: sin detalle."`.
8. Ejemplos de lo que ve el usuario hoy:
   - `"En espera: There is no active document."` y `"En espera: A command is in progress."`
   - `"Rechazado: The request does not belong to the active document."`
   - `"Fallo: The catalog is not available."`, `"Fallo: The document is locked by another operation."` y `"Fallo: The edit intention could not be read: <ex.Message>"`

Alcance práctico de cada literal del adaptador:

- 80: solo si `LateralHeaderDrawService.LoadCatalog()` (= `RackCatalogLoader.Load()`, `src/RackCad.Plugin/RackCatalogLoader.cs:17-43`) devuelve null. Ese cargador captura sus excepciones y devuelve `new RackCatalog()`, así que en la práctica no ocurre.
- 85: casi inalcanzable por la misma razón.
- 112: **inalcanzable hoy**. El campo manda un testigo null (paso 2) y el host cortaría antes un testigo sin clave (`WorkspaceHostCore.cs:298-299`, `376-377`, `455-456`, `"El testigo de la vista previa no tiene clave."`).
- 118: `CaptureDrawing` siempre asigna `RawEnvelopes` (`src/RackCad.Plugin/Views/RackSiblingScan.cs`), así que tampoco ocurre en la práctica.
- 127, 131 y 135 sí pueden ocurrir.

### A.3 Registro, F3 y copias del mismo texto

- Ninguno de los 10 literales va al registro. `LogFault` (`WorkspaceNavigationHost.cs:187`, `RackLog.Exception`) solo recibe la excepción de 213-216. El adaptador no registra nada. El host no reporta `read.Reason`: `Report` se usa solo en los `catch` de 355, 425 y 509.
- Los textos de F3 son **literales distintos con el mismo texto inglés** y quedan sin tocar:
  - `ReadSummaryOnActiveDocument` (110-125):
    - 114 `"There is no active document."`
    - 116 `"A command is in progress."`
    - 118 `"The drawing is not read inside a notice of AutoCAD; it is asked again at the next drain."`
    - 122 `"The request does not belong to the active document."`
  - `RunOnActiveDocument` (154-171):
    - 158 `"There is no active document."`
    - 160 `"A command is in progress."`
  - Adaptador: 174 repite el texto de 127.
- **Consecuencia para la guarda de T3C2**: una guarda al estilo H-07 que exija que `"\"" + english + "\""` no aparezca en TODO `WorkspaceNavigationHost.cs` o en TODO `RackPilotIntentAdapter.cs` fallaría por las copias de F3 y por 174. Tiene que medirse sobre el cuerpo:
  - `Src.BodyOf(host, @"private\s+EditIntentReadResult\s+ReadOnActiveDocument\s*\(")`, que ya existe en `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:41`;
  - `AdapterRead()` (43) y el cuerpo de `LoadCatalog` (363).
  
  La otra opción es fijar conteos exactos de lo que queda en inglés: `"There is no active document."` 2, `"A command is in progress."` 2, `"The request does not belong to the active document."` 1 y `"The document is locked by another operation."` 1.

### A.4 ¿Alguna prueba o guarda fija estos literales en inglés?

Búsqueda de cada texto (`git grep -F` en tests/): **ninguna prueba ni guarda lee estos literales del Plugin.**

- `tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs` (H-07, tabla 398-428) contiene tres textos idénticos: 401 `"The request does not belong to the active document."`, 403 `"The preview witness has no key."`, 423 `"There is no active document."` y 425 `"A command is in progress."`. **Solo los prohíbe en `Code(WorkspaceHostCore.cs) + Code(WorkspacePilotSupport.cs)`** (`LiteralViolations`, 444-463; `all = host + "\n" + support`). No lee el Plugin: traducir el Plugin no la afecta.
- Dobles de F3 que reproducen textos de F3, no del campo:
  - `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulerTests.cs:222, 224, 228`
  - `tests/RackCad.Tests/Workspace/WorkspaceBrowserSummaryTests.cs:392`
  - `tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserSummaryPortDouble.cs:55, 58`
  - `tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserViewModelTests.cs:443`
  - `tests/RackCad.UI.Tests/Workspace/WorkspaceNavigationViewModelTests.cs:518`
- Dobles del lector en el host con otros textos, que no dependen del Plugin: `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs:404-407`, `891-894` y `1058` (`"no document"`, `"busy"`, `"not yours"`, `"could not read"`, `"a".."d"`).
- `"The catalog is not available"`, `"The catalog could not be loaded"`, `"raw envelopes"`, `"locked by another operation"`, `"The drawing could not be read"` y `"The edit intention could not be read"`: 0 apariciones en tests/.
- Las guardas fijan la FORMA, no el texto:
  - `WorkspaceEditIntentGuardTests.cs:109`: `if\s*\(\s*scan\.RawEnvelopes\s*==\s*null\s*\)\s*return\s+new\s+RackPilotIntentReadResult\s*\(\s*null\s*,`
  - `WorkspaceEditIntentGuardTests.cs:354`: lo mismo tras `IsNullOrWhiteSpace(witnessKey)`
  - `WorkspaceEditIntentGuardTests.cs:416-419`: `EditIntentReadResult.NoActiveDocument(` / `.Busy(` / `.Rejected(` / `.Failed(` en `ReadOnActiveDocument`
  - `WorkspaceEditIntentGuardTests.cs:367`: `"catch (System.Exception"` en `LoadCatalog`
  
  Sustituir el literal por otro literal, o por una `const`, mantiene esas formas.

**Conclusión:** se pueden traducir. Nada los fija en inglés.

### A.5 Otros literales ingleses del Plugin que lleguen al campo

Exhaustivo sobre la vía (lector → host → `EditIntentOutcome` → `WorkspacePilotFieldViewModel` → XAML):

- `git grep 'Outcome\.\w*("|PilotFieldResult\.\w*("|ReadResult\.\w*("|new PilotBoundaryResult|PilotFieldView(' -- src/RackCad.Plugin` solo da 224, 226 y 236. `PilotEditAdapter` (`WorkspaceController.cs:234-269`) solo reenvía.
- **No hay otro literal inglés del Plugin en la vía.** Lo único más que llega es texto en tiempo de ejecución, que no es un literal:
  - `WorkspaceNavigationHost.cs:216` `ex.Message`;
  - el sufijo `ex.Message` de 85, 131 y 135, que puede traer inglés de Application, por ejemplo `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentScan.cs:44-71`, `throw new ArgumentException("A scan fact cannot be null." …)` y otros. R-C14 dice que `ex.Message` no cambia.
- Lo demás que ve el campo es de Application y está en español:
  - `PushBackPilotIntentReader.cs:26-56`, los diagnósticos `"No editable aquí: …"` y `ClassicEditorHint = "Usa RACKEDITAR para editarlo con el editor clásico."`;
  - `WorkspaceHostCore.cs`, los motivos de H-07.
- `PilotFieldView.ErrorReason` no tiene productor: `git grep '\.EnterError(' -- src` está vacío.
- `EditIntentOutcome.Detail` no se muestra (el VM del campo no lo lee).

### A.6 Guardas cuyas listas de subcadenas se aplicarían al español de esos dos archivos

Todas sobre `Code`, con los literales dentro. Las marcadas «IC» ignoran mayúsculas; el resto las distingue.

`WorkspaceNavigationHost.cs`, en la carpeta `src/RackCad.Plugin/Workspace/`:

- `tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs:362-371` `TheBridgeFilesCarryNoMetricsProviderProjectSummaryOrAggregationConcept`, IC: `ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom`.
- `WorkspaceBridgeSourceGuardTests.cs:139-176` `TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs`, `\b(?:…)\b`:
  - `SetImpliedSelection|SetCurrentView|ZoomExtents|ZoomWindow|Zoom|PanTo|UpdateScreen|Regen|SendStringToExecute|StartCommand|GetSelection|GetEntity|GetString|GetPoint|GetKeywords|GetInteger|GetDouble|SelectImplied|SelectAll|SelectAtPoint|ShowModalDialog|ShowModelessDialog|ShowDialog|MessageBox|GetObject|OpenCloseTransaction|Highlight|Unhighlight`
- `WorkspaceBridgeSourceGuardTests.cs:333-334` `EntryPoints`: `IExtensionApplication|ExtensionApplication|\[CommandMethod|\[assembly\s*:|ModuleInitializer|static\s+AutoCadEventBridge\s*\(`.
- `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs`:
  - 393-415 `NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType`: `Autodesk\.AutoCAD\.Internal|PaletteSet|AddVisual|System\.Windows|RackCad\.UI|Autodesk\.AutoCAD\.Windows|\bUserControl\b|\bDispatcher\b`.
  - 418-437 `NoHostFileStartsABackgroundThreadOrAsyncWork`, hostForbidden: `…|\bTimer\b|…|\.Wait\s*\(|\.Result\b|GetAwaiter|GetResult|ConfigureAwait|StartNew|ContinueWith|Task\.Delay|async\s+void`. Además, `\.Result\b` en todos los archivos del Plugin.
  - 440-449: `ExecuteInCommandContextAsync|SendStringToExecute|\[CommandMethod|Autodesk\.AutoCAD\.Internal|DllImport|HwndSource`; 0 `async` y 0 `await`.
  - 452-457: `System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32|\bRegistry\b|\bXrecord\b|\bXData\b|\bDBDictionary\b|NamedObjectsDictionary|AtomicFile|UserSettings|Environment\.SetEnvironmentVariable`.
  - 546-552, IC: `ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom`.
- `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs`:
  - 426, sobre el cuerpo `ReadOnActiveDocument`: `_effects|_runner|ReturnFocusToDrawing|SetImpliedSelection|SetCurrentView|\.Focus\s*\(|GetObject|\.Commit\s*\(|OpenMode|ForWrite`.
  - 436: exactamente 2 `class` en el archivo; no cabe otra clase anidada para una tabla de textos.
  - 437: ningún nombre de clase con `Draft|Editor|Subscription|Tab|Stable`.
  - 460: `PushBackPilot|RackCad\.Application\.Systems`.
  - 464: `TryBeginPilotEdit|TryCheckPilotEdit|WorkspaceHostCore`.
  - 472: `\.Result\b|GetResult|StartNew|ContinueWith|\+=|-=`.
  - 473: exactamente 1 `RackLog\.Exception\s*\(`.
  - 474: `\b(?:async|await)\b|\bTask\.(?:Run|Delay|Factory)|Dispatcher`.
  - 459: `Src.Text` debe contener `"result of the pilot"` (comentario de la línea 24).
- `tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs`:
  - 257-264: `SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync`.
  - 38: `Assert.Equal(9, Src.PluginFiles().Count)`; **un archivo nuevo en `src/RackCad.Plugin/Workspace/` lo rompe** (hoy hay 9).
- `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs:273-277`: `Autodesk\.AutoCAD\.Internal|SetFocusToDwgView|IKeyboardInputSink|DllImport`.

`RackPilotIntentAdapter.cs`, en `src/RackCad.Plugin/Views/`, fuera de `PluginFiles()`. Todo está en `WorkspaceEditIntentGuardTests.cs`:

- 138: ningún tipo `public`. 139: exactamente 3 `internal sealed class RackPilot\w+Result`. 140: `Src.Text` empieza por `#nullable enable`.
- 144-153: `HostObjectField`, y 152 `\bstatic\s+(?:readonly\s+)?[\w<>,.?]+\s+\w+\s*(?:;|=(?!>)|\{\s*get)`. **Prohíbe un `static readonly` para una tabla de textos.** Las `private const string` sí caben, como `CompletenessEvidence` (71).
- 187-192: `StartOpenCloseTransaction|OpenCloseTransaction`; `LockDocument|StartTransaction` en `Read`.
- 279: `\bthrow\b`.
- 282-293:
  - `AdapterWrites`: `OpenMode\.(?:ForWrite|ForNotify)|\bForWrite\b|UpgradeOpen|DowngradeOpen|AppendEntity|\.Erase\s*\(|\.Commit\s*\(|RackBlockData\.Write|SetImpliedSelection|SetCurrentView|GetCurrentView|ViewTableRecord|\bViewTable\b|SetSystemVariable|\bZoom\w*|\bRegen\b|UpdateScreen|CreateExtensionDictionary|\.Save\s*\(|SaveAs`, más `ProjectVariables`;
  - 1 `RackBlockData\.Read\s*\(` y 0 `GetObject\s*\(`.
- 356: `new\s+RackEmbedDocument|RackEmbedDocument|RackEmbedStore|RackEmbedComposer`.
- 368, cuerpo de `LoadCatalog`: `LockDocument|StartTransaction|MdiActiveDocument|Document\b` (distingue mayúsculas; «documento» no casa).
- 376-379: `RackCad\.Application\.Workspace|…|WorkspaceNavigation|IWorkspaceEditIntentReader|EditIntentReadResult|EditIntentWitness|WorkspaceHostCore|WorkspaceController`.

Lista de subcadenas de la tabla H-07, la plantilla de «tabla cerrada como H-07»: `WorkspaceHostHardeningGuardTests.cs:440-442`, IC, `Transaction|Database|Commit\(|RackLog|Provider|Metric|LockDocument|Aggregat|\bBom|TotalRacks|ProjectSummary|ComputedParameter|proveedor|metrica|métrica`. Solo la aplica a la columna española de su propia tabla (454).

«no muestra» / «aún no»: **ninguna guarda de Workspace los prohíbe** (`git grep -i "no muestra|a[uú]n no|todav[ií]a no" -- tests` solo da pruebas ajenas: Cantilever, PushBack…).

### A.7 TABLA CERRADA propuesta (sin traducción)

| n | literal inglés exacto | archivo:línea | método | ¿llega al campo? | ¿fijado por prueba? |
|---|---|---|---|---|---|
| 1 | `There is no active document.` | `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:224` | `PilotIntentReader.ReadOnActiveDocument` | Sí: NoActiveDocument → `Waiting` (host 318/477) → `"En espera: …"`. Solo si la puerta `PassGate` (1043-1047) pasó | No (H-07 :423 lo prohíbe solo en host+support; el mismo texto en F3 :114 y :158 no se toca) |
| 2 | `A command is in progress.` | `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:226` | `PilotIntentReader.ReadOnActiveDocument` | Sí: Busy → `Waiting` (318/477) → `"En espera: …"` | No (H-07 :425 solo host+support; F3 :116 y :160 no se tocan) |
| 3 | `The request does not belong to the active document.` | `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:236` | `PilotIntentReader.ReadOnActiveDocument` | Sí: Rejected → `Rejected` (320/479) → `"Rechazado: …"` | No (H-07 :401 solo host+support; F3 :122 no se toca) |
| 4 | `The catalog is not available.` | `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:80` | `RackPilotIntentAdapter.LoadCatalog` | Sí, por el conducto :230 → Failed → `Failed` (322/481) → `"Fallo: …"`. En la práctica no ocurre: `RackCatalogLoader.Load` no devuelve null | No |
| 5 | `The catalog could not be loaded: ` (+ `ex.Message`) | `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:85` | `RackPilotIntentAdapter.LoadCatalog` | Sí, por :230 → `"Fallo: …"` (casi inalcanzable) | No |
| 6 | `The preview witness has no key.` | `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:112` | `RackPilotIntentAdapter.Read` | La vía existe (:251), pero **hoy es inalcanzable**: el testigo del campo es null y el host corta antes uno sin clave | No (H-07 :403 solo host+support) |
| 7 | `The scan of the drawing carries no raw envelopes.` | `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:118` | `RackPilotIntentAdapter.Read` | Sí, por :251 → `"Fallo: …"` (en la práctica no ocurre) | No (solo la forma `return new RackPilotIntentReadResult(null,` :109) |
| 8 | `The document is locked by another operation.` | `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:127` | `RackPilotIntentAdapter.Read` | Sí, por :251 → `"Fallo: …"` | No (el mismo texto en :174, ReadPreviewWitness, queda fuera) |
| 9 | `The drawing could not be read: ` (+ `ex.Message`) | `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:131` | `RackPilotIntentAdapter.Read` | Sí, por :251 → `"Fallo: …"` | No |
| 10 | `The edit intention could not be read: ` (+ `ex.Message`) | `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:135` | `RackPilotIntentAdapter.Read` | Sí, por :251 → `"Fallo: …"` | No |

Fuera de la tabla:

- conductos sin literal: `WorkspaceNavigationHost.cs:216` (`ex.Message`), `:230` y `:251`;
- ReadPreviewWitness, sin llamadores: `RackPilotIntentAdapter.cs:157, 174, 178, 182`;
- F3: `WorkspaceNavigationHost.cs:114, 116, 118, 122, 158, 160`;
- Debug.Assert: `RackPilotIntentAdapter.cs:200`.

---

## PARTE B: el aviso de veto debe nombrar RACKPANELDESCARTAR

### B.1 Texto actual (`src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`)

`RowOf(StoredNotice)` (1129-1142):

- 1131 (pérdida): `return new WorkspaceBrowserNoticeItem(stored.Key, LossText(stored.Loss), false);`
- 1135 (veto de otra sesión): `"No se cerró otro documento con borradores sin aplicar; actívalo para descartarlos."`, `CanDiscardDocument` false.
- 1137-1139, la cabeza del veto de la sesión presentada:
  - `close.IsUncertain || close.RackIds.Count == 0 ? "No se cerró el documento: no se pudo decidir qué borradores están en riesgo."`
  - `: "No se cerró el documento: hay borradores sin aplicar en " + TitlesOf(close.RackIds) + ".";`
- 1140: `var text = head + " Descártalos con \"Descartar borradores de este documento\"; cada pestaña también se descarta con su \"Cerrar\".";`. **Solo nombra la vía del panel.**
- 1141: `return new WorkspaceBrowserNoticeItem(stored.Key, text, true);`

`LossText(PilotLossView)` (1144-1160):

- 1148 `"Se perdieron borradores al cerrar un documento: " + string.Join(", ", loss.LostRackIds) + "."`
- 1150 `"Se cerró un documento con borradores que no se pudieron conservar."`
- 1156 `"Sin decidir: " + string.Join(", ", uncertain) + "."`, con `r + " (" + reason + ")"` (1155).

Las etiquetas que cita el texto existen en el XAML:

- `src/RackCad.UI/Workspace/WorkspacePanelView.xaml:132-134` `BrowserDiscardDocumentButton` `Content="Descartar borradores de este documento"`;
- `:165`, botón de la fila;
- `:121` `Content="Cerrar"` de la pestaña;
- `:162` `<TextBlock Text="{Binding Text}" …>` de la fila.

El comando `RACKPANELDESCARTAR` **no existe en src/ en HEAD** (`git grep RACKPANEL -- src` solo da `[CommandMethod("RACKPANEL")]` en `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:14` y comentarios).

### B.2 Pruebas que fijan los textos del aviso

`tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs`:

- `TheVetoOfThePresentedSessionHasItsTextAndTheDiscardButtonAndNamesNoWayToApply` (768-787):
  - 780-782 `Assert.Equal("No se cerró el documento: hay borradores sin aplicar en Nombre R-1, R-3. Descártalos con \"Descartar borradores de este documento\"; cada pestaña también se descarta con su \"Cerrar\".", row.Text);`. **Rompe** al añadir el comando.
  - 783 `Assert.DoesNotContain("Actualizar", row.Text);`
  - 784 `Assert.DoesNotContain("Aplicar", row.Text);`. Distingue mayúsculas; «sin aplicar» pasa.
  - 785 `Assert.DoesNotContain("RACKPANELDESCARTAR", row.Text);`. **Prohíbe exactamente lo que pide T3C2: hay que invertirla.**
- `AnUncertainVetoSaysSoAndTheVetoOfAnotherSessionHasNoButtonUntilItIsPresented` (789-814):
  - 797 `Assert.Contains("no se pudo decidir qué borradores están en riesgo", uncertainRow.Text);`
  - 805 `Assert.Equal("No se cerró otro documento con borradores sin aplicar; actívalo para descartarlos.", other.Text);`
  - 813 `Assert.Contains("hay borradores sin aplicar en", presented.Text);`
- `ANewVetoOfTheSameSessionReplacesTheOldOne…` (816-832), 827 `Assert.Contains("Nombre R-1, R-2", row.Text);`
- `ALossNamesTheLostRacksAndApartTheUncertainOnes…` (834-859), 850 `Assert.Equal("Se perdieron borradores al cerrar un documento: R-1, R-2. Sin decidir: R-3 (falló la comparación).", row.Text);`
- `UnderstoodRemovesOnlyItsRowAndCallsNothingOfThePort` (861-882), 877 `Assert.Contains("R-2", left.Text);`
- `TheVetoOfASessionThatNoLongerLivesIsPurged…` (903-925), 923 `Assert.Contains("Se perdieron borradores", left.Text);`
- `AfterDiscardingARackItLeavesTheVetoOfItsSession…` (927-955):
  - 936 `Assert.Contains("Nombre R-1, Nombre R-2", …Text);`
  - 941 `Assert.Contains("sin aplicar en Nombre R-2.", row.Text);`. Fija el punto tras los títulos.
  - 942 `Assert.DoesNotContain("R-1", row.Text);`. «RACKPANELDESCARTAR» no contiene «R-1».
- `TheDocumentDiscardRemovesTheVetoOfThatSession…` (957-977):
  - 975 `n.Text.StartsWith("No se cerró otro documento", StringComparison.Ordinal)`
  - 976 `n.Text.StartsWith("Se perdieron borradores", StringComparison.Ordinal)`

`tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs`:

- `TheRowOfAVetoOfThePresentedSessionHasItsDiscardButton…` (347-380):
  - 363 `Assert.Equal("Descartar borradores de este documento", discard.Content);`
  - **365** `Assert.Contains("No se cerró el documento", Descendants<TextBlock>(list).Select(t => t.Text).FirstOrDefault(t => t.StartsWith("No se cerró", StringComparison.Ordinal)));`
- `TheRowOfAVetoOfAnotherSessionHasNoDiscardButtonAndTheRowOfALoss…` (382-…), 400 `t.Text.StartsWith("Se perdieron borradores", …) && t.Text.Contains("R-8 (falló)")`.

Etiqueta del botón citada por el aviso, fijada aparte:

- `WorkspacePilotCloseSyncViewTests.cs:127`;
- `tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs:624` `("BrowserDiscardDocumentButton", "OnBrowserDiscardDocumentClick", "Descartar borradores de este documento")`.

Total: **10 métodos de prueba con 18 aserciones sobre los textos del aviso** (12 métodos si se cuentan 127 y 624, que fijan la etiqueta del botón). Al añadir el comando a la cola de 1140 y mantener la cabeza, solo rompen **780-782 (Equal exacto) y 785 (DoesNotContain RACKPANELDESCARTAR)**, ambas en el mismo método. Las demás usan Contains o StartsWith sobre la cabeza, o son de pérdida u otra sesión.

Las guardas de F4-T3B2 no fijan `RowOf` ni `LossText`. `git grep "RowOf|LossText|TitlesOf" -- tests` no da nada del VM; `WorkspacePilotCloseSyncGuardTests.cs:321-342` solo fija la firma de `WorkspaceBrowserNoticeItem(string Key, string Text, bool CanDiscardDocument)` y `Notices`.

### B.3 ¿Se prohíbe nombrar un comando, «RACKPANEL» o una «vía de aplicar» en la UI?

- **Ninguna guarda prohíbe la cadena «RACKPANEL» ni el nombre de un comando en la UI.** Las guardas prohíben LANZARLOS:
  - `tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:257-264` `NeitherTheUiNorTheHostLaunchesCommandsOrOffersTheClassicEditor`: `SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync` en `src/RackCad.UI/Workspace/` + host + facts. En `Src.Text(XAML)` prohíbe `Abrir en editor|editor clásico|editor clasico`, IC. Afecta al XAML, no al VM.
  - `WorkspacePilotCloseSyncGuardTests.cs:851` `DrawingReach` sobre el VM del navegador y el code-behind: `\bTransaction|\bDatabase\b|\.Commit\s*\(|SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync|Autodesk`.
  - `tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs:280`: la misma lista `drawing` sobre la carpeta de la UI.
- La única prohibición de nombrar el comando es la **prueba** `WorkspacePilotCloseSyncTests.cs:785`.
- Precedentes de un comando nombrado en un texto de UI:
  - `src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs:340` `"Abre el panel con RACKPANEL para iniciar el seguimiento del dibujo."`;
  - `ClassicEditorHint` `"Usa RACKEDITAR para editarlo con el editor clásico."` (`PushBackPilotIntentReader.cs:56`, se muestra en `BrowserFieldHintText`, XAML :212).
- «Vía de aplicar»: solo el nombre de la prueba (`…NamesNoWayToApply`, 769) y sus aserciones 783-784 (`"Actualizar"`, `"Aplicar"`, distinguen mayúsculas). Ninguna guarda la prohíbe.
- Censos de comandos. Crear el comando no forma parte de esta parte, pero el texto lo nombrará:
  - `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs:36-46` (RACKPANEL una vez, en el adaptador);
  - `WorkspaceBridgeSourceGuardTests.cs:340-351` (el adaptador de comandos tiene exactamente un `[CommandMethod`, el de RACKPANEL);
  - `WorkspacePaletteHostSourceGuardTests.cs:326`;
  - `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs:55` `F1PanelCommands = { "RACKPANEL" }`;
  - `tests/RackCad.Tests/SelectiveEditorOpenTests.cs:562-566` `Assert.Equal(39, total)`.

### B.4 Lista de palabras prohibidas de la UI que afectaría al nuevo texto

- `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs:578-595` `TheUiFolderWritesNothingAndCarriesNoMetricsConceptOrNamesOfI63`, sobre `UiSources()` (40-…, `Code` de `.cs` y `.xaml`):
  - 592: `System\.IO|\bFile\.|\bDirectory\.|Registry|UserSettings|RackLog|XData|Xrecord` (quita `BrowserSessionRegistry` del VM, 586-589);
  - 593, IC: `Metric|Provider|\bBom|ProjectSummary|TotalRacks|Aggregat|ComputedParameter`.
- `tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs:661-686` `NewMechanisms`, sobre el Code del navegador, el panel y el campo: `\+=|-=|\bevent\b|\bdelegate\b|\bDispatcher\w*|\w*Timer\b|\bTask\b|\basync\b|\bawait\b|\bKeyboard\b|\.Focus\s*\(|FocusManager`. Los textos se componen con `a = a + b`.
- `WorkspacePilotCloseSyncGuardTests.cs:472-480`:
  - `Subscriptions`: `\+=|-=|\bevent\b|\bdelegate\b|\bstatic\s+(?!readonly\s+string\b)\w+\s+\w+\s*;`. Una `static readonly string` se admite; una `const` también.
  - `Scheduling`: `\bTask\b|\basync\b|\bawait\b|\bDispatcher\b|\w*Timer\b|\bThread\b|\.Result\b|\.Wait\s*\(`.
- `WorkspaceBrowserHostGuardTests.cs:267-276` (`TheBrowserViewModelIsSynchronousHasNoSubscriptionAndHoldsNoStaticStateNorHostObject`), el VM del navegador:
  - `\bTask\b|…|\.Result\b|GetAwaiter`;
  - `\+=|-=|\bevent\b|\bdelegate\b|\bstatic\s+(?!readonly\s+string\b)\w+\s+\w+\s*;`;
  - `Autodesk|RackCad\.Plugin|IWorkspaceNavigationPort|IWorkspaceHostPort|ExecuteAsync|NavigateAsync`.
- `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:914-921`, carpeta de la UI:
  - `Keyboard|FocusManager|SetFocus|\.Focus\s*\(|InputManager`;
  - `TileMode|TILEMODE|LayoutManager|SetSystemVariable|DBDictionary|NamedObjectsDictionary|Xrecord|XData|AppendEntity|\.Commit\s*\(`.
- `WorkspacePilotCloseSyncGuardTests.cs:449-466`: la forma pública del VM tiene exactamente 33 firmas. Cambiar un texto no la toca.

---

## PARTE C: menores arrastrados de F4-T3C1

### R-1: `WhyOf` (Busy) sin prueba que discrimine

`src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:937-948`:

```
private static string WhyOf(EditDiscardOutcome outcome)
{
    var reason = string.IsNullOrWhiteSpace(outcome.Reason) ? "sin detalle." : outcome.Reason;
    switch (outcome.Status)
    {
        // The reason of the port already says it in Spanish: it is shown alone, and the generic text only stands in for a missing one.
        case EditDiscardStatus.Busy: return string.IsNullOrWhiteSpace(outcome.Reason) ? "hay una lectura en curso." : outcome.Reason;
        case EditDiscardStatus.Failed: return "falló. " + reason;
        case EditDiscardStatus.Rejected: return "se rechazó. " + reason;
        default: return reason;
    }
}
```

Antes de T3C1 (`git diff b845b9a6 7bd275e4`): `case EditDiscardStatus.Busy: return "hay una lectura en curso. " + reason;`.

Llamadores:

- 292 `CleanDraftKeptText(WhyOf(outcome))` (CloseTab limpia; `CleanDraftKeptText` 934-935 = `"Se cerró la pestaña; su borrador limpio quedó en memoria: " + why`);
- 487 `"No se descartaron los borradores del documento: " + WhyOf(discarded)`;
- 519 `"No se cerró la pestaña: " + WhyOf(outcome)` (ConfirmDiscard de pestaña);
- 963 `"No se descartó el borrador: " + WhyOf(outcome)` (FieldDiscarded).

El motivo real del host es `PilotReadInProgressReason = "Hay una lectura del piloto en curso."` (`WorkspaceHostCore.cs:44`, devuelto en 727 y 751).

Pruebas con `EditDiscardOutcome.Busy` en `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs`. Las dos solo usan `Assert.Contains`, así que el texto viejo y el nuevo las pasan:

- `AnotherAnswerOfTheConfirmationDoesNotCloseTheTabEither` (251-266):
  - 258 `EditDiscardOutcome.Busy("hay una lectura")`
  - 264 `Assert.Contains("No se cerró la pestaña", …)`
  - 265 `Assert.Contains(status == "Busy" ? "hay una lectura" : "falló el host", rig.Browser.NoticeText);`
- `ACleanDraftClosesWhateverTheDiscardAnswersAndTheNoticeSaysItStayedInMemory` (310-345):
  - 323 `EditDiscardOutcome.Busy("hay una lectura en curso")`
  - 333 `Assert.Contains("Se cerró la pestaña; su borrador limpio quedó en memoria:", …)`
  - 336/341 `Assert.Contains("hay una lectura en curso", …)`

Con el código viejo, 265 vería `"No se cerró la pestaña: hay una lectura en curso. hay una lectura"` y 341 vería `"…: hay una lectura en curso. hay una lectura en curso"`. Ambas siguen pasando.

Prueba que discrimina, en el gesto **CloseTab limpia**. Es el más directo: no hay confirmación de por medio, y `CloseNow` (915-919) hace `PostNotice(noticeAfter)` al final, que reemplaza el texto, así que se puede usar `Assert.Equal` exacto. El montaje es el de 317-320: `Opened(A, "R-1")`, `Pinned(A, "R-2")`, `Present(A, One("R-1"))`, `DiscardRackResponder = (_, _) => EditDiscardOutcome.Busy(reason!)`, `CloseTab("R-1")`.

- Con motivo: `reason = "Hay una lectura del piloto en curso."` → `Assert.Equal("Se cerró la pestaña; su borrador limpio quedó en memoria: Hay una lectura del piloto en curso.", rig.Browser.NoticeText)`. El código viejo daría `"…: hay una lectura en curso. Hay una lectura del piloto en curso."`.
- Sin motivo: `reason = null` (o `" "`) → `Assert.Equal("Se cerró la pestaña; su borrador limpio quedó en memoria: hay una lectura en curso.", rig.Browser.NoticeText)`. El código viejo daría `"…: hay una lectura en curso. sin detalle."`.

Variante opcional por **ConfirmDiscard**, sobre otro llamador: `Protected(A, "R-1")`, `CloseTab("R-1")`, Busy, `ConfirmDiscard()`. Con motivo: `Equal("No se cerró la pestaña: Hay una lectura del piloto en curso.")`; sin motivo: `Equal("No se cerró la pestaña: hay una lectura en curso.")`.

Precedente del mismo patrón (Theory con motivo y con null): `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1390-1405`, `ABusyOrNotEditableRefusalShowsTheReasonOfThePortOnceAndTheGenericTextOnlyWithoutOne` (`"El texto no se aplicó: Hay una lectura del piloto en curso."` / `"El texto no se aplicó: hay una lectura en curso."`).

Ninguna guarda nombra `WhyOf` ni `CleanDraftKeptText` (`git grep -- tests` vacío).

### R-2: `_confirmUnread`

VM (`WorkspaceBrowserViewModel.cs`):

- campo 121: `private bool _confirmUnread;`
- `Ask` (990-1000): `// couldNotRead: the port could not say anything about the tab that is asked to close; the question says so.` / `private void Ask(ConfirmationKind kind, SessionId session, string rack, bool couldNotRead = false)`. **996: `_confirmUnread = couldNotRead;`**
- `RemoveConfirmation` (1002-1011): 1007 `_confirmUnread = false;`
- llamadores de `Ask`: 282 `Ask(ConfirmationKind.CloseTab, session, rackId, !view.HasValue);`, 329 `Ask(ConfirmationKind.DiscardField, session, rack);`, 391 `Ask(ConfirmationKind.DiscardDocument, session, null);`
- uso: `ConfirmationTextOf` 1037-1039, `if (_confirmUnread) return "No se pudo comprobar la pestaña " + TitleOf(_confirmRack) + "; puede tener trabajo sin aplicar. ¿Descartar lo que tenga y cerrarla?";`, y si no, `"La pestaña " + TitleOf(_confirmRack) + " tiene un borrador con trabajo sin aplicar. ¿Descartarlo y cerrar la pestaña?"`.
- `BeginGesture` (894-899) solo borra el aviso; no quita la confirmación.

Prueba actual: `WorkspacePilotCloseSyncTests.cs:1520-1541` `TheClosingConfirmationOfATabThatCouldNotBeReadIsNeutralAndAnyOtherRequestRemovesTheMemoryOfIt`:

- 1525 `Throws[GetFieldOp]`, 1527 `CloseTab("R-1")`, 1530 `Equal("No se pudo comprobar la pestaña Nombre R-1; …")`;
- **1535 `rig.Browser.CancelDiscard();`**, que pone `_confirmUnread = false` en 1007;
- 1537-1540: `Throws.Clear()`, `Set(… isProtected: true)`, `CloseTab("R-1")`, `Equal("La pestaña Nombre R-1 tiene un borrador con trabajo sin aplicar. ¿Descartarlo y cerrar la pestaña?")`.

Por pasar por `CancelDiscard`, sobrevive el mutante que quita 996 (o `_confirmUnread = _confirmUnread || couldNotRead`).

Escenario mínimo que lo discrimina: dos peticiones seguidas, **sin cancelar**.

1. `Opened(A, "R-1")`; `Throws[GetFieldOp] = new InvalidOperationException(…)`; `CloseTab("R-1")` → `HasConfirmation`, texto «No se pudo comprobar…».
2. Sin `CancelDiscard`: `Throws.Clear()`; `Set(A, "R-1", View(PilotFieldState.Dirty, pending: "1:75", isProtected: true))`; `CloseTab("R-1")`. Entra por 280-282 → `Ask(…, false)` con la confirmación pendiente.
3. `Assert.Equal("La pestaña Nombre R-1 tiene un borrador con trabajo sin aplicar. ¿Descartarlo y cerrar la pestaña?", rig.Browser.ConfirmationText)`. Con el mutante seguiría el texto «No se pudo comprobar…».

Ninguna guarda nombra `_confirmUnread` (`git grep -- tests` vacío).

### R-4: documentación frente a implementación

Puerto, `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs`:

- 72: `/// <summary>Takes, oldest first, every notice of lost work that nobody consumed, and leaves the queue empty.</summary>`
- 75: `/// <summary>Takes, oldest first, every notice of a vetoed close that nobody consumed, and leaves the queue empty.</summary>`
- 21-22, regla general: `/// - every exception is contained as a value with a reason and the session is not degraded; the one of <see cref="GetRacksAtRisk"/>` / `///   is contained as the unknown value (<see cref="PilotRiskView.IsKnown"/> false) and its reason goes to the log;`. No dice nada de las colas.

Implementación, `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`:

- `TakeLossNotices` (809-827): `Dequeue` fuera del try (814), un try por aviso (815-823); en el catch, `// The conversion of one notice failed: that notice is lost and stays only in the log; the next one is still taken.` (821) y `Report("WorkspaceHostCore.TakeLossNotices", ex)` (822). La cola sí queda vacía, pero un aviso cuya conversión falle **se pierde** (solo queda en el log) y la lista devuelta puede ser más corta. La doc 72 no lo dice.
- `TakeCloseNotices` (829-843): todo el cuerpo en un try (831-836: `ToArray`, `Clear`, `return`); catch 837-842 con `// The queue is not emptied: it is taken again at the next presentation.` (839), `Report(…)` y `return Array.Empty<PilotCloseView>();`. Si falla, **devuelve vacío y NO vacía la cola**: contradice «leaves the queue empty» de la línea 75.
- Docs viejas cercanas (observación, fuera de R-4):
  - 994 `/// <summary>The notices of lost work that nobody consumed yet, oldest first. F4-T3 consumes them; until then nothing does.</summary>`
  - 515 `… (D-T2B2-15), for the command that F4-T3 will expose.`
  - 540-541 `… Until the panel edits drafts (F4-T3, F4-T4)` / `nothing calls it.` (de `NotePilotEdit`, que hoy sí se llama).
- Doble de las pruebas: `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:85, 88` `"…oldest first; taking them empties the queue."`. Es un comentario del doble, no una guarda.

VM, `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`:

- `RequestDocumentDiscard`, doc 352-356: `/// The Discard button of the drafts of the document (I-64 F4-T3B2, R-B2-2): reads the racks at risk of the presented session.` / `/// With none the notice says nothing was discarded and nothing happens; otherwise it asks, inline, listing the racks by their title` / `/// and, apart, the ones that could not be decided, and it keeps both lists for the second look of <see cref="ConfirmDiscard"/>.`
  - El código 376-382 (`// The unknown is never "nothing at risk"…`, `if (!risks.IsKnown) { … PostNotice(UnknownRiskText); return; }`) no aparece en la doc.
- `ConfirmDiscard`, doc 402-407: `/// The Discard button of the inline confirmation. For a tab … For the` / `/// document it reads the racks at risk again and compares both lists in order and by ordinal text: if they changed nothing is` / `/// discarded and the new list is shown to be reviewed; if not, it discards the drafts of the whole session, closing no tab.`
  - No menciona 439-444, `if (!again.IsKnown) { RemoveConfirmation(); PostNotice(UnknownRiskAgainText); return; }`.
- Textos: 86 `NoRiskText`, 87 `UnknownRiskText = "No se pudo decidir qué borradores están en riesgo en este documento; no se descartó nada."`, 88 `UnknownRiskAgainText = "No se pudieron volver a comprobar los borradores en riesgo; no se descartó nada."`.

¿Alguna guarda fija esa documentación? **No.**

- `git grep -F` en tests/ de «leaves the queue empty», «nobody consumed», «With none the notice says», «reads the racks at risk again», «The Discard button of the drafts of the document», «The Discard button of the inline confirmation», «F4-T3 consumes them» e «is taken again at the next presentation»: 0 resultados.
- Las guardas de las colas y de `IsKnown` usan `Src.Code`, sin comentarios. Por eso las docs `///` quedan fuera:
  - `WorkspaceHostHardeningGuardTests.cs:43-67` y `106-128`;
  - `507-524`: el cuerpo empieza en la firma.
- Guardas `Src.Text` (con comentarios) que sí aplican a la doc nueva del puerto y del host, solo como vocabulario:
  - `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:782-788`, sobre `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, `WorkspaceHostCore.cs` y `WorkspaceEditIntent.cs`:
    - `Transaction|\bDatabase\b|\bCommit\s*\(` (distingue mayúsculas);
    - `Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom|ComputedParameter` (IC).
  - `WorkspaceEditIntentGuardTests.cs:822-830`, sobre `WorkspaceHostCore.cs`, `IWorkspaceEditIntentReader.cs` y `WorkspaceEditIntent.cs`: las mismas dos listas.
  - El VM de la UI no tiene ninguna guarda `Text` sobre su doc. La única `Text` de la carpeta es la de los namespaces, `WorkspacePaletteHostSourceGuardTests.cs:598-605`.

### Guardas de F4-T3C1 (`tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs`) que fijan conteos o estructura

- **`LiteralViolations` (444-463), conteos exactos sobre `Code(WorkspaceHostCore.cs) + Code(WorkspacePilotSupport.cs)`.**
  - Tabla de 398-428, 28 filas, `Table.Length == 28` y 28 distintas por columna (447, 468-470).
  - Veces ≠ 1 (verificadas en HEAD):
    - `"La petición no es del documento activo."` ×6 (host 45, 320, 401, 479, 525, 1063)
    - `"No hay borrador para el rack."` ×3 (46, 387, 466)
    - `"El testigo de la vista previa no tiene clave."` ×3 (299, 377, 456)
    - `"El dibujo no está listo para leerse."` ×3 (318, 399, 477)
    - `"El borrador no quedó desconocido tras una aplicación fallida."` ×2
    - `"El campo no se puede editar ahora."` ×4
  - Las otras 22 filas son ×1. Entre ellas: `"No hay documento activo."` (1045), `"Hay un comando en curso."` (1057), `"Hay una lectura del piloto en curso."` (44) y `"Se está entregando un aviso del panel."` (support 156).
  - Identificadores (431-437): `PilotReadInProgressReason`, `NotThisDocumentReason`, `NoDraftReason` e `InsideNoticeReason`, cada uno exactamente 1 vez con su texto.
  - Siguen en inglés por decisión: `DescribeLoss … "Uncertain racks"` (461) y `"(variable registry)"` (462).
  - Implicación para A: traducir el Plugin no altera estos conteos, ni aunque se usen los mismos textos españoles, porque la guarda no lee el Plugin. **Sí los altera** mover o añadir esos textos dentro del host o del support (por ejemplo, una `const` compartida en Application/Workspace).
  - Cambios del aviso de veto o de la doc: no la afectan, porque la UI no está en su ámbito y los comentarios se quitan.
- `UnknownConsumerViolations` (507-524; prueba 534-539): en `RequestDocumentDiscard` y `ConfirmDiscard`, `!x.IsKnown` antes de `NoRiskText` y `.IsKnown` exactamente 1 vez por cuerpo (Code). Cambiar la doc no la rompe; añadir otro `.IsKnown` en el código sí.
- `RetentionViolations` (553-582): `KeepsActive(..., out bool couldNotCheck)` y `ApplySelection`.
- `CleanCloseViolations` (597-618): en `CloseTab`, `.DiscardRack(` … `catch (Exception ex) { RemoveConfirmation();`.
- `PanelConstructorViolations` (620-657).
- `LimitViolations` (661-686): `NewMechanisms` sobre el Code (con literales) del navegador, el panel y el campo; `Reflection` sobre los seis archivos.
- Ninguna fija `RowOf`, `LossText`, `WhyOf`, `Ask` ni los textos del aviso.
- Del resto (`TakeLossViolations` 43-67, `TakeCloseViolations` 106-128, `SessionDestroyedViolations` 165-193, `RisksViolations` 245-266, `KnownViolations` 302-327 y `PortAdditionViolations` 363-369), todas son de estructura sobre Code. Una corrección solo de documentación no rompe ninguna. Cambiar el comportamiento de `TakeCloseNotices` para que «deje la cola vacía» sí la rompería: 116-126 exigen el cuerpo en un único try, `Clear` una vez dentro del try y el catch exacto `Report(…); return Array.Empty<PilotCloseView>();` sin `Clear`.
