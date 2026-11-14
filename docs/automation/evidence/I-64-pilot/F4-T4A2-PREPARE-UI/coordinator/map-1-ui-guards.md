# Map 1 — Tests and guards that constrain F4-T4A2-PREPARE-UI (UI side)

- Repository read at HEAD `b9e1ed5a` (clean tree). Read-only research: nothing built, nothing run.
- Scope: every test in `tests/RackCad.Tests` and `tests/RackCad.UI.Tests` (bin/obj excluded) that reads the source text of the UI of the
  workspace or reflects over its types, plus the UI tests that pin the behaviour and the call log of the field of the pilot.
- All line numbers are at HEAD. "Exact" means an exact count, exact text or exact set; "lower bound" means `>=`.

## 0. Vocabulary and how the source guards read the files

- In the packages, the **«navegador»** (browser) is `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs` (the XAML section titled
  «Navegador de racks»). The **«navegación»** is `src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs` (XAML section «Navegación»).
  G-03 and the guard at `WorkspacePilotCloseSyncGuardTests.cs:849` are about the **browser** VM, not the navigation VM.
- Helper `tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs`:
  - `Src.Code(path)` (:34-39) and `Src.Files(folder)` (:48-58) strip `//` and `/* */` comments (`CodeOnly`, :29-30) but **keep string
    literals**; `Src.Files` returns **only `*.cs`** (XAML is never in it).
  - `Src.Text(path)` (:41-46) keeps comments. `Src.BodyOf` (:67-89) cuts a member body by braces or `=>`…`;`.
- The XAML guards remove `<!-- -->` comments before matching (e.g. `WorkspacePilotCloseSyncGuardTests.cs:588`), except
  `WorkspaceBrowserHostGuardTests.cs:263` (uses `Src.Text`, comments included).
- `WorkspacePaletteHostSourceGuardTests.UiSources()` (:39-55) = UI `*.cs` without comments **plus** UI `*.xaml` without comments.

## 1. Which UI file may call which port operation

### 1.1 The guard at `WorkspacePilotCloseSyncGuardTests.cs:849` (exact text)

```csharp
// :843-847
[Fact]
public void TheUiOfTheTaskReachesNeitherTheIntentionNorTheDrawingNorTheFocusNorADialog()
{
    CheckUiReach(Browser(), new[] { Browser(), Src.Code(CodeBehindPath) });
}

// :849-855
private static readonly Regex IntentionReads = new(@"\b(?:TryBeginEdit|TryReload|TryCheckNow)\b");
private static readonly Regex DrawingReach = new(@"\bTransaction|\bDatabase\b|\.Commit\s*\(|SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync|Autodesk");
private static readonly Regex FocusReach = new(@"\bKeyboard|FocusManager|SetFocus|\.Focus\s*\(|InputManager");
private static readonly Regex DialogReach = new(@"EnterPanelModal|ShowModalWindow|ShowDialog|MessageBox|\bRegistry\b|\bRackLog\b");

// :857-867
private static void CheckUiReach(string browser, IEnumerable<string> codes)
{
    // The browser view-model names none of the three reads; the one that gestures of the field own stays in the field.
    Assert.DoesNotMatch(IntentionReads, browser);
    foreach (var code in codes) { Assert.DoesNotMatch(DrawingReach, code); Assert.DoesNotMatch(FocusReach, code); Assert.DoesNotMatch(DialogReach, code); }
}
```

- `IntentionReads` is applied **only to `WorkspaceBrowserViewModel.cs`** (comments stripped): the browser VM may not contain the tokens
  `TryBeginEdit`, `TryReload` or `TryCheckNow` anywhere (not even in a string). `DrawingReach`, `FocusReach`, `DialogReach` apply to the
  browser VM **and** `WorkspacePanelView.xaml.cs`. Self-test theory :869-902 (18 rows).
- Not in `IntentionReads`: `TryConfirmBase`, `TryKeepMine`, `TryDiscardAndReload` (no UI guard names them; see 1.3).

### 1.2 Other guards on who calls what (all UI files unless stated)

| Operation | Rule | Where (file:line) |
|---|---|---|
| `TryBeginEdit`, `TryReload` | Named only in `WorkspacePilotFieldViewModel.cs` (any other UI file fails); never in a "non-gesture" member | `tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs:27-28, 82-104` (`TheTwoGestureReads` at :94-95) |
| `TryBeginEdit`, `TryReload`, `TryCheckNow` | Never inside the bodies of `Present|Refresh|Show|Clear|RebuildTabs|SyncSurface|StartSurface|ClearSurface|Materialize` (`void`/`bool` members with those exact names), any public constructor of the three VMs, or **any property setter** (`\bset\s*(?=\{|=>)`) of any UI `.cs` | `WorkspacePilotSurfaceGuardTests.cs:64-75, 86-88` |
| `TryCheckNow` | Never inside the T3B1 gesture bodies `BeginEdit|CommitPending|Rematerialize|BeginFieldEdit|BeginFieldEditFromBox|BeginFieldEditFromTab|ReloadField` of any UI file | `WorkspacePilotSurfaceGuardTests.cs:56-61, 90-92` |
| `TryCheckNow` (allowed) | Self-test pins that a **new** member (`CheckNow()` in the field VM, `CheckField()` in the browser VM, `Check()` in the panel VM) is NOT a violation of this guard | `WorkspacePilotSurfaceGuardTests.cs:134-141` — but the browser variant is still forbidden by `WorkspacePilotCloseSyncGuardTests.cs:849/860` |
| `SetPendingText` | Named only inside the `set` of `public string Text` of the field VM; any other UI file or member fails | `WorkspacePilotSurfaceGuardTests.cs:147-189` |
| `DiscardRack` | Called (`.X(` or unqualified non-declaration) only in the browser VM bodies of `CloseTab`, `DiscardField`, `ConfirmDiscard` (each `>=1`); any other UI file (incl. field VM, panel VM) fails | `WorkspacePilotCloseSyncGuardTests.cs:86-123`; self-test :137-152 |
| `DiscardDocument` | Only in `ConfirmDiscard` of the browser VM, exactly 1 | `WorkspacePilotCloseSyncGuardTests.cs:125-135` |
| `GetRacksAtRisk` | Only in `RequestDocumentDiscard` (exactly 1) and `ConfirmDiscard` (exactly 1) of the browser VM | `WorkspacePilotCloseSyncGuardTests.cs:125-135` |
| `TakeCloseNotices`, `TakeLossNotices` | Only in `private void TakeNotices()` of the browser VM (1 each, 2 `try`); `TakeNotices` called exactly once, from `Present` | `WorkspacePilotCloseSyncGuardTests.cs:156-173`; host side :175-197 |
| Panel VM | No `.TakeCloseNotices|.TakeLossNotices|.DiscardRack|.DiscardDocument|.GetRacksAtRisk` | `WorkspacePilotCloseSyncGuardTests.cs:553-559` |
| `ReadSummary`, `IsSessionAlive` | Exactly 1 each in the browser VM, 0 in every other UI file | `tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:244-249`; again `WorkspacePilotCloseSyncGuardTests.cs:484-493` (with `PurgeDeadSessions` loop) |
| `ApplySelectionContext` | Exactly 1 call in the browser VM, `.ApplySelectionContext(x, keepActive)` | `WorkspacePilotCloseSyncGuardTests.cs:221-231` |
| `ExecuteAsync` (navigation port) | Exactly 1 in `WorkspaceNavigationViewModel.cs`, 0 elsewhere | `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:899-912` |
| `GetSnapshot` | Exactly 1 in the panel VM; no `.Authorize(`, `.GetSelectionContext(`, `.ReadSummary(`, `.IsSessionAlive(` there | `WorkspaceBrowserHostGuardTests.cs:233-234`; `WorkspacePaletteHostSourceGuardTests.cs:548-550`; `WorkspaceHostHardeningGuardTests.cs:729` (no `IsSessionAlive` in panel VM) |
| `ContainFailure` | Not called by the six closing gestures `CloseTab, DiscardField, RequestDocumentDiscard, ConfirmDiscard, CancelDiscard, DismissNotice` (M-7); must still be called somewhere | `WorkspacePilotCloseSyncGuardTests.cs:904-962` |
| `CommitField`, `GetField`, `CrossBoundary` | **No guard restricts their caller** in the UI. `\.Commit\s*\(` is forbidden but does not match `CommitField(` | `WorkspaceNavigationHostTests.cs:920`; `WorkspacePilotSurfaceGuardTests.cs:280` |

### 1.3 The three reconciliation reads (`TryConfirmBase`, `TryKeepMine`, `TryDiscardAndReload`)

- **No UI source guard names them** (searched all of `tests/`). They are only in the port guards of the host side
  (`tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:44-64`: 15 operations as a lower bound, `ReadOperations` six reads).
- Consequence: a call to them from `Present`, a setter, a constructor or the browser VM is not caught by any source guard today, and is
  not caught by `Reads()` of the UI double either (M-9, section 6).

## 2. G-01, G-02, G-03, G-04 — original definition and current text

Origin: `docs/initiatives/I-64-f4-t3b-gate-contract-package.md:52-55` (measured before T3B1: G-01 "4 argumentos exactos" in 3 guards,
G-02 4 parameters in 2 guards, G-03 `new WorkspaceBrowserViewModel(browserSummary, RepresentNavigation)`, G-04 "exactamente un `<Button`
desde `x:Name="BrowserSurface"` hasta el **primer** `</Border>`"). T3B1 changed them as declared in §5 (:308-311). Current state:

### G-01 — construction of the panel VM in the palette (now **5 arguments**, 3 guards + 1 negative guard)

Exact regex, identical in the three sites:
`new\s+WorkspacePanelViewModel\s*\(\s*_controller\.Port\s*,\s*_controller\.SetInstrumentationEnabled\s*,\s*_controller\.Navigation\s*,\s*_controller\.BrowserSummary\s*,\s*_controller\.PilotEdit\s*\)`

| Site | Also pins |
|---|---|
| `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs:193` (test :185-196) | no `_controller.(ActiveSession|TryGetSession|Authorize|RequestReconciliation|RefreshPanel|OpenPanel)`; no bridge/core names |
| `tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:214-216` (test :206-219) | `_controller.BrowserSummary` 1, `_controller.Navigation` 1, no `IWorkspaceBrowserSummaryPort|BrowserRackSummar|WorkspaceBrowser`, exactly 2 `+=` |
| `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:948` (test :940-957) | exact `using` list (4), 2 `+=`, no navigation names, `InitialWidth = 320`, `MinimumWidth = 240`, dock sides |
| `tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs:248-272` (negative) | `_controller.PilotEdit` exactly once; palette names none of `IWorkspacePilotEditPort|PilotField|PilotBoundary|EditIntent|WorkspaceBrowser` |

Also `WorkspacePaletteHostSourceGuardTests.cs:181`: exactly one `new WorkspacePanelViewModel(` in the palette host.

### G-02 — signature of the panel VM (5 parameters, now 3 sites)

- `WorkspaceBrowserHostGuardTests.cs:227-230` (test :221-235):
  `public\s+WorkspacePanelViewModel\s*\(\s*IWorkspaceHostPort\s+\w+\s*,\s*Action<bool>\s+\w+\s*,\s*IWorkspaceNavigationPort\s+\w+\s*=\s*null\s*,\s*IWorkspaceBrowserSummaryPort\s+browserSummary\s*=\s*null\s*,\s*IWorkspacePilotEditPort\s+pilotEdit\s*=\s*null\s*\)`
  and exactly 1 `public WorkspacePanelViewModel(` (:230).
- `WorkspacePaletteHostSourceGuardTests.cs:547` (test :537-552): same with `\w+` for the last two names; plus `using RackCad.Application.Workspace;`,
  `sealed class WorkspacePanelViewModel : ObservableObject`, `.GetSnapshot()`, `.GetMeasurementReport()`, no `.Authorize(`/`.GetSelectionContext(`,
  no `Application.Current|GetOrCreate|ServiceLocator|Singleton|.Instance|static (non readonly string) field`.
- `WorkspacePilotCloseSyncGuardTests.cs:550-551` (`PanelSignature`, test :561-565, self-test :571-584): same regex as the first site; plus
  the construction of G-03 and `Assert.DoesNotContain("Building the panel never calls it", Src.Text(panel))` (comments included).
- `WorkspaceHostHardeningGuardTests.cs:717-753`: the constructor body must contain exactly
  `try { Present(InitialSnapshot(), null); } catch (Exception ex) { EnterError(ex); }`, one `Present(InitialSnapshot(), null)` in the file,
  and no `IsSessionAlive` in the panel VM.

### G-03 — construction of the **browser** VM by the panel (3 arguments)

- `WorkspaceBrowserHostGuardTests.cs:231-232`: `new\s+WorkspaceBrowserViewModel\s*\(\s*browserSummary\s*,\s*RepresentNavigation\s*,\s*pilotEdit\s*\)`, exactly 1 `new WorkspaceBrowserViewModel(`.
- `WorkspacePilotCloseSyncGuardTests.cs:556`: same regex.
- Browser constructor itself: `WorkspacePilotSurfaceGuardTests.cs:342-345` — exactly 1 public constructor matching
  `public\s+WorkspaceBrowserViewModel\s*\(\s*IWorkspaceBrowserSummaryPort\s+\w+\s*,\s*Action\s+\w+\s*=\s*null\s*,\s*IWorkspacePilotEditPort\s+\w+\s*=\s*null\s*\)`
  (closed parenthesis: a 4th parameter fails); and in the public shape (§8.1) `"public WorkspaceBrowserViewModel(IWorkspaceBrowserSummaryPort port, Action tabChanged = null, IWorkspacePilotEditPort pilotEdit = null)"`.
- Navigation VM constructed by the panel: `WorkspaceNavigationHostTests.cs:895-896` — exactly 1 `new WorkspaceNavigationViewModel(`, regex `new\s+WorkspaceNavigationViewModel\s*\(\s*navigation\s*,\s*Refresh\s*\)`.

### G-04 — buttons of the surface (now a balanced element and a **lower bound**)

`WorkspaceBrowserHostGuardTests.cs:301-347` (self-tests :349-378):

```csharp
// I-64 F4-T3B1 (G-04): the surface is the COMPLETE element BrowserSurface, from its opening Border to the Border that balances it.
// The openings counted are `<Border` followed by a space or `>` that are not closed by themselves (`/>`); `<Border.Style>` does not
// count, and each `</Border>` closes one. Cutting at the first `</Border>` would end the surface at the first Border nested inside it.
private static string SurfaceElement(string xaml) { ... Regex.Matches(xaml, @"<Border(?=[\s>])[^>]*>|</Border>") ... }

// A lower bound, not an exact count: the later tasks add their buttons to the surface without touching this guard.
Assert.True(Src.Count(surface, @"<Button\b") >= 3, "the surface has fewer buttons than F4-T3B1 declares");
foreach (var (content, handler) in new[] { ("Volver a leer", "OnBrowserSurfaceReloadClick"), ("Editar", "OnBrowserFieldEditClick"), ("Recargar", "OnBrowserFieldReloadClick") })
{
    Assert.Equal(1, Src.Count(surface, "Content=\"" + content + "\""));
    Assert.Equal(1, Src.Count(surface, "Click=\"" + handler + "\""));
    Assert.Matches(@"private\s+void\s+" + handler + @"\s*\(\s*object\s+sender\s*,\s*RoutedEventArgs\s+e\s*\)\s*=>\s*Contained\s*\(", code);
}
// + exact lambda of OnBrowserSurfaceReloadClick => Contained(() => _viewModel.Browser.ReloadSummary()); 1 public ReloadSummary(); 1 .ReloadSummary( in code-behind
```

Today the surface element (XAML :178-239) holds 4 `<Button` (Editar :215, Recargar :220, Descartar :225, Volver a leer :234).
**Adding** buttons to the surface is compatible with G-04; adding a **second** `Content="Editar"`, `Content="Recargar"` or
`Content="Volver a leer"`, or a second `Click="OnBrowserField(Edit|Reload)Click"` / `Click="OnBrowserSurfaceReloadClick"`, breaks it.
A stricter twin exists: `WorkspacePilotCloseSyncGuardTests.cs:722-756` (see §9).

## 3. Keyboard, focus, Dispatcher, `+=`/`event`, `Provider` — every prohibition

### 3.1 Keyboard and focus

| Guard | Files scanned | Exact forbidden text |
|---|---|---|
| `WorkspaceNavigationHostTests.cs:914-922` | every UI `.cs` | `Keyboard|FocusManager|SetFocus|\.Focus\s*\(|InputManager` — note **no `\b`**: any substring `Keyboard` (`KeyboardDevice`, `LostKeyboardFocus`, `Keyboard.PrimaryDevice`, `KeyboardNavigation`) fails; also `TileMode|TILEMODE|LayoutManager|SetSystemVariable|DBDictionary|NamedObjectsDictionary|Xrecord|XData|AppendEntity|\.Commit\s*\(` |
| `WorkspacePilotSurfaceGuardTests.cs:278-297` (theory :299-313, accept :315-321) | every UI `.cs` | focus `\bKeyboard|FocusManager|SetFocus|\.Focus\s*\(|InputManager`; accepts `KeyEventArgs`, `TextCompositionEventArgs`, `e.Handled`, `LostFocusX` |
| `WorkspacePilotCloseSyncGuardTests.cs:853` (`FocusReach`) | browser VM + code-behind | `\bKeyboard|FocusManager|SetFocus|\.Focus\s*\(|InputManager` |
| `WorkspaceHostHardeningGuardTests.cs:758-783` (`NewMechanisms`) | browser VM, panel VM, field VM | `\bKeyboard\b|\.Focus\s*\(|FocusManager` (plus mechanisms, §3.3) |
| `WorkspacePilotCloseSyncGuardTests.cs:797-839` (**M-6 #1**) | code-behind (whole file) and XAML (whole file) | `Key? _launchKey` must exist; body of `OnBrowserFieldBoxPreviewKeyDown` contains `e.IsRepeat` and `e.Key`; whole code-behind must not match `KeyHandlerBad = e\.KeyboardDevice|\bKeyboard\b|\bKeyUp\b|\.Focus\s*\(|FocusManager|e\.SystemKey|e\.ImeProcessedKey|e\.IsDown|e\.IsToggled` (:814); whole XAML has **no `\bKeyUp\b`** (:808) and **exactly one `\bPreviewKeyDown\s*=`** (:809) |
| `WorkspacePilotCloseSyncGuardTests.cs:694-709` (**M-6 #2**) | whole XAML | `\bIsDefault\s*=|\bIsCancel\s*=` forbidden anywhere (:707); self-test :711-720 |
| `WorkspacePaletteHostSourceGuardTests.cs:273-278` | Plugin workspace + UI `.cs` | `Autodesk\.AutoCAD\.Internal|SetFocusToDwgView|IKeyboardInputSink|DllImport` |
| `WorkspacePaletteHostSourceGuardTests.cs:628-645` and `WorkspaceNavigationHostTests.cs:908-911` | code-behind | `_viewModel\.Refresh\s*\(|\bLoaded\b|\bIsVisibleChanged\b|\bSizeChanged\b|\bCompositionTarget\b` (and `Navigation\.Present\s*\(`); must contain `_viewModel.Retry(`, `_viewModel.SetInstrumentation(`, `catch (System.Exception` |
| `WorkspacePilotSurfaceGuardTests.cs:195-230` | XAML + code-behind | XAML: no `\bTextChanged\s*=`, no `SourceUpdated|TargetUpdated`; code-behind: no `TextChanged|SourceUpdated|TargetUpdated`, no `\.Text\s*=(?!=)` |
| `WorkspacePilotSurfaceGuardTests.cs:232-244` | XAML | the one `<TextBox x:Name="BrowserFieldBox">` has `Text="{Binding Browser.Field.Text, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"` and `PreviewMouseLeftButtonDown|PreviewKeyDown|LostFocus="OnBrowserFieldBox\w+"` |
| UI test `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:486-497` | live view (reflection on `EventHandlersStore`) | box: 0 instance `KeyDown`, 0 `KeyUp`, >0 `PreviewKeyDown`; `BrowserFieldPanel`: 0 `KeyDown` |

The **M-6 finding** (T3B2 independent review,
`docs/automation/evidence/I-64-pilot/F4-T3B2-EDIT-CLOSE-SYNC/1/R20261113T151548Z-4f9b/independent-review.md:100-115`, cited there as
`:450-452` and `:389` of that SHA) is exactly the two rows marked M-6 above, now at `WorkspacePilotCloseSyncGuardTests.cs:797-810` and `:707`.
Effect on T4A2: any new keyboard handling (e.g. Esc/Enter on an inline Yes/No) must not add a second `PreviewKeyDown=` or any `KeyUp` to the
XAML, must not use `e.KeyboardDevice`/`Keyboard`/`.Focus(` in the code-behind, and no new button may use `IsDefault`/`IsCancel`.
No guard scans the XAML for `KeyboardNavigation`, `Focusable`, `TabIndex`, `InputBindings`/`KeyBinding` or `Command=` (searched: none).

### 3.2 Dispatcher, timers, tasks

| Guard | Files | Forbidden |
|---|---|---|
| `WorkspacePaletteHostSourceGuardTests.cs:554-581` | every UI `.cs` **and `.xaml`** (except the navigation VM) | `DispatcherTimer|\bDispatcher\b|\w*Timer\b|System\.Timers|\bTask\b|\basync\b|\bawait\b|\bThread\b|ThreadPool|BackgroundWorker|SynchronizationContext|Parallel\.|Sleep\s*\(|UnhandledException|AppDomain`; the navigation VM is the one exception (1 `async`, 1 `await`, 1 `await _port.ExecuteAsync(`, `public async Task NavigateAsync(`) |
| `WorkspacePilotSurfaceGuardTests.cs:283, 288-289` | every UI `.cs` except the navigation VM | `\bDispatcher\b|\w*Timer\b|\bTask\b|\basync\b|\bawait\b|\bThread\b|\.Result\b|\.Wait\s*\(` |
| `WorkspacePilotCloseSyncGuardTests.cs:475-502` (`Scheduling`) | browser VM, field VM | `\bTask\b|\basync\b|\bawait\b|\bDispatcher\b|\w*Timer\b|\bThread\b|\.Result\b|\.Wait\s*\(` |
| `WorkspaceBrowserHostGuardTests.cs:266-278` | browser VM | same + `ThreadPool|Sleep\s*\(|SynchronizationContext|Parallel\.|BackgroundWorker|GetAwaiter`; no `Autodesk|RackCad\.Plugin|IWorkspaceNavigationPort|IWorkspaceHostPort|ExecuteAsync|NavigateAsync` |
| `WorkspaceHostHardeningGuardTests.cs:758-760` | 3 VMs | `\bDispatcher\w*|\w*Timer\b|\bTask\b|\basync\b|\bawait\b` |
| UI test `tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserViewModelTests.cs:948-962` | `WorkspaceBrowserViewModel` by reflection (DeclaredOnly, all visibilities) | no field of type `Timer`/`Thread`/`Task`; no method returning `Task`; no `AsyncStateMachineAttribute`; `GetEvents` empty; sealed |

### 3.3 `+=`, `-=`, `event`, `delegate`, static state

| Guard | Files | Forbidden |
|---|---|---|
| `WorkspacePilotCloseSyncGuardTests.cs:473, 496-502` (theory :519-548) | browser VM, field VM | `\+=|-=|\bevent\b|\bdelegate\b|\bstatic\s+(?!readonly\s+string\b)\w+\s+\w+\s*;` (texts are composed `a = a + b`; `private static readonly string X = ...` accepted) |
| `WorkspacePilotSurfaceGuardTests.cs:325-335` | field VM | same regex; no `Autodesk|RackCad\.Plugin|WorkspaceHostCore|WorkspaceController|IWorkspaceHostPort`; `public sealed class WorkspacePilotFieldViewModel : ObservableObject`; exactly 1 public ctor matching prefix `WorkspacePilotFieldViewModel\s*\(\s*IWorkspacePilotEditPort\s+\w+\s*,\s*Action\s+\w+` |
| `WorkspaceBrowserHostGuardTests.cs:274` | browser VM | same regex |
| `WorkspaceHostHardeningGuardTests.cs:758-783` | browser VM, **panel VM**, field VM | `\+=|-=|\bevent\b|\bdelegate\b|...` plus reflection `System\.Reflection|\bBindingFlags\b|\bActivator\b|\bMethodInfo\b|\bFieldInfo\b|\bPropertyInfo\b|InternalsVisibleTo` |
| Code-behind | — | **not** covered by any `+=` guard (it has one, `_viewModel.PropertyChanged += OnViewModelPropertyChanged;` at `src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs:31`) |

Effect: an `ICommand`/`RelayCommand` in the browser or field VM would need `event EventHandler CanExecuteChanged` → forbidden. There is no
`ICommand` infrastructure in `src/RackCad.UI` (searched). Every button today is a `Click=` handler in the code-behind wrapped in `Contained(...)`.

### 3.4 `Provider` / `IFormatProvider` and other vocabulary

- `WorkspacePaletteHostSourceGuardTests.cs:583-601`, every UI `.cs` **and `.xaml`** (comments stripped, **string literals included**):
  - `Metric|Provider|\bBom|ProjectSummary|TotalRacks|Aggregat|ComputedParameter` with `IgnoreCase` → `IFormatProvider`, "provider",
    "Bomba", "metrica" (without accent) fail; "métrica" and "proveedor" do not.
  - `System\.IO|\bFile\.|\bDirectory\.|Registry|UserSettings|RackLog|XData|Xrecord` (case-sensitive), with the single exception of one
    `BrowserSessionRegistry` in the browser VM.
- `WorkspaceBrowserHostGuardTests.cs:256-264`: no `SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync` in UI `.cs`; XAML text
  (comments included) has no `Abrir en editor|editor clásico|editor clasico` (IgnoreCase).
- `WorkspacePaletteHostSourceGuardTests.cs:524-535`: no `Autodesk|RackCad\.Plugin`, no `\bWorkspaceController\b|\bWorkspacePaletteHost\b|\bWorkspaceHostPort\b|\bWorkspaceSelectionReader\b|\bWorkspaceHostCore\b|\bAutoCadEventBridge\b|\bEventBridgeCore\b` in UI sources.
- `WorkspaceBrowserHostGuardTests.cs:239-254` and `WorkspaceNavigationHostTests.cs:862-877`: the UI never names `NavigationTargets`,
  `NavigationTargetSet`, `NavigationTarget`, `NavigationDefinitionFact`, `INavigationFactsPort`, `BrowserRackFact`, `BrowserRackSummaries`,
  `WorkspaceNavigationHost`, `WorkspaceNavigationFacts`, `NavigationActionPolicy`, `NavigationActionPlan`, `NavigationActionDecision`,
  `INavigationEffects`, `NavigationActionVerdict`, `NavigationEcho`, `WorkspaceNavigationRunner`, `WorkspaceNavigationRequest`.
- `WorkspacePaletteHostSourceGuardTests.cs:603-612`: each UI `.cs` has exactly one line `namespace RackCad.UI.Workspace` (block-scoped).

## 4. Confirmations and dialogs in the UI

### 4.1 How the UI asks today (inline, in the browser VM; no abstraction)

- There is **no** confirmation abstraction in `src/` (no `IConfirm*`, no dialog service; searched `interface I\w*(Confirm|Dialog|Prompt|Question|Ask)`),
  **no** confirmation callback injected into the UI, and **no** `MessageBox` in `src/RackCad.UI/Workspace`.
- The one mechanism is the **single inline confirmation slot of the browser VM** (`src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`):
  - `private enum ConfirmationKind { None, CloseTab, DiscardField, DiscardDocument, }` (:57-63) — comment "The one inline confirmation that may be pending".
  - State `_confirmKind`, `_confirmSession`, `_confirmRack`, `_confirmRacks`, `_confirmUncertain`, `_confirmUnread` (:116-121).
  - `private void Ask(ConfirmationKind kind, SessionId session, string rack, bool couldNotRead = false)` (:993-1002),
    `private void RemoveConfirmation()` (:1004-1013), `private void RaiseConfirmation()` (:1015-1019),
    `private void ValidateConfirmation()` (:1023-1032, ends it on session change, closed tab, or — for DiscardField — surface change),
    `private string ConfirmationTextOf()` (:1034-1052).
  - Public: `public bool HasConfirmation => _confirmKind != ConfirmationKind.None;` (:192),
    `public string ConfirmationText => HasConfirmation ? ConfirmationTextOf() : string.Empty;` (:195),
    `public void ConfirmDiscard()` (:410-528, the "Yes": dispatches on `_confirmKind`), `public void CancelDiscard()` (:531-535, the "No": only `BeginGesture(); RemoveConfirmation();`).
  - Askers: `CloseTab` (:242-307, protected or unreadable tab), `DiscardField` (:314-352, protected draft), `RequestDocumentDiscard` (:360-402).
  - XAML (`src/RackCad.UI/Workspace/WorkspacePanelView.xaml`): `BrowserConfirmationPanel` (:137-154, `Visibility={Binding Browser.HasConfirmation}`),
    `BrowserConfirmationText` (:141), `BrowserConfirmDiscardButton` Content «Descartar», automation «Confirmar el descarte» (:144-147),
    `BrowserCancelDiscardButton` Content «Cancelar», automation «Cancelar el descarte» (:148-151) — between the strip and the surface,
    **outside** `BrowserSurface`. Code-behind `OnBrowserConfirmDiscardClick`/`OnBrowserCancelDiscardClick` (:93, :95).
- The only Yes/No elsewhere in the workspace is **not UI**: `WorkspaceDocumentDiscardCommand.Run(IWorkspacePilotEditPort? port, Func<SessionId?> activeSession, Func<SessionId, bool> isTarget, Func<string, bool> confirm, Action<string> write)`
  (`src/RackCad.Application/Workspace/WorkspaceDocumentDiscardCommand.cs:61-66`), injected by `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:44-49`
  with `private static bool ConfirmOnCommandLine(string question)` (:58, command-line keywords Si/No, default No), pinned by
  `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorGuardTests.cs:130-150` (`Keywords.Add("Si")`, `Keywords.Add("No")`, `Keywords.Default = "No"`, `AllowNone = true`).

### 4.2 Guards that forbid a dialog in the UI

| Guard | Files | Rule |
|---|---|---|
| `WorkspacePaletteHostSourceGuardTests.cs:508-522` | every UI `.cs` + `.xaml` | `<Window\b|:\s*Window\b|\bnew\s+Window\b|ShowDialog|MessageBox`; exactly 1 `<UserControl(?=[\s>])` |
| `WorkspacePilotSurfaceGuardTests.cs:282, 286-287` | every UI `.cs` | `EnterPanelModal|ShowModalWindow|ShowDialog|MessageBox` — **even `EnterPanelModal` is forbidden in UI** |
| `WorkspacePilotCloseSyncGuardTests.cs:855` (`DialogReach`) | browser VM + code-behind | `EnterPanelModal|ShowModalWindow|ShowDialog|MessageBox|\bRegistry\b|\bRackLog\b` |
| `WorkspacePilotEditGuardTests.cs:630-680` | Application, Plugin and UI workspace `.cs` | every `ShowModalWindow(`/`ShowDialog(`/`MessageBox` must be inside `using (...EnterPanelModal())`; "today there is none" |
| `WorkspacePilotEditGuardTests.cs:559-627` | whole product | only the 4 owners call `.SetModalRackCadActive(`; nobody calls it on a controller (self-test injects `src/RackCad.UI/Workspace/WorkspacePaletteHostX.cs`) |
| `WorkspacePilotCloseSyncGuardTests.cs:694-709` | browser section of the XAML | `<ContentControl|<Window\b|ShowDialog|MessageBox`; no `Width/Height/Min*/Max*=`; one `x:Name="BrowserSurface"`; whole XAML no `IsDefault=`/`IsCancel=` |
| `WorkspaceBrowserHostGuardTests.cs:380-400` | browser section | `<Window\b|ShowDialog|MessageBox`; no sizes; `<ItemsPanelTemplate>\s*<WrapPanel\b`; no `<ItemsControl[^>]*BrowserSurface|<ContentControl|<TabControl|<TabItem` |
| `WorkspaceNavigationHostTests.cs:924-938` | navigation section | `<Window\b|ShowDialog|MessageBox`; no sizes; `>=3` `<WrapPanel`; 6 named controls |
| `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs:86-131` | every concrete `Window` type of the RackCad.UI assembly (reflection) | a new `Window` subclass must be in the census (`EveryConcreteWindowTypeIsCensusedWithItsArchetype` :97-107) |
| UI test `WorkspacePilotCloseSyncViewTests.cs:168-194` (T12) | live view | `BrowserConfirmationPanel` holds **exactly 2** buttons, in order `["Descartar", "Cancelar"]`, neither `IsDefault` nor `IsCancel`; `Descendants<Window>(view)` empty; `Window.GetWindow(view)` null |
| UI test `WorkspaceBrowserViewTests.cs:431-450` | live view | no `Window` descendant; `Window.GetWindow(view)` null |

Effect on T4A2: a Yes/No must be inline (a panel/row of buttons in the `UserControl`), with no `IsDefault`/`IsCancel`, no `Window`,
no `MessageBox`, no `EnterPanelModal` in the UI. Reusing `BrowserConfirmationPanel` keeps it at exactly `Descartar`/`Cancelar`
(any other button text there breaks T12 and `WorkspacePilotCloseSyncGuardTests.cs:642-643`); the single `ConfirmationKind` slot means a
new kind would replace/be replaced by a pending close/discard confirmation (pinned by `WorkspacePilotCloseSyncTests.cs:424-451`
"…ANewRequestReplacesIt", :453-471, :473-489, :860-885, :1617-1638, :1640-1659).

### 4.3 How the UI tests exercise confirmations (all inline)

- VM level `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs`: `browser.CloseTab(...)` / `DiscardField()` /
  `RequestDocumentDiscard()` → `Assert.True(HasConfirmation)` + `ConfirmationText` checks → `browser.ConfirmDiscard()` or `CancelDiscard()`
  (e.g. T11 :88-123, :125-155; T12 :157-187; T13 :189-207; T14 :209-233; document TOCTOU :633-721; cancel :723-736; unknown risk :1484-1565).
- View level `WorkspacePilotCloseSyncViewTests.cs`: `Click(Named<Button>(view, "BrowserConfirmDiscardButton"|"BrowserCancelDiscardButton"))`
  (T12 :168-194, T13 :196-215, T14 :217-240, field in Error :312-346, element placement and sizes :424-470).

## 5. Literal texts pinned for the field VM and the panel

- **There is no table of UI literals** (no `LiteralViolations` or equivalent in `tests/RackCad.UI.Tests`, and no source guard over the
  Spanish texts of `WorkspacePilotFieldViewModel.cs`, `WorkspaceBrowserViewModel.cs` or `WorkspacePanelViewModel.cs`). A new UI text needs
  **no registration**; it is only constrained by the vocabulary bans of §3.4 (strings are scanned) and by the individual assertions below.
- The table with exact counts that exists is for the **host**, not the UI: `tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs:421-454`
  (`Table`, 30 rows `(English, Spanish, Times)`), checked by `LiteralViolations` (:470-489) over `WorkspaceHostCore.cs` + `WorkspacePilotSupport.cs`
  (`Assert.Equal(30, Table.Length)` :494; distinct English/Spanish :495-496), 4 identifiers (:457-463), `ForbiddenSubstrings`
  `Transaction|Database|Commit\(|RackLog|Provider|Metric|LockDocument|Aggregat|\bBom|TotalRacks|ProjectSummary|ComputedParameter|proveedor|metrica|métrica`
  IgnoreCase (:466-468). A new host text is registered by adding a row with its exact count (and the "30" constants change).
- UI texts pinned by the UI tests (exact = `Assert.Equal`; contains = `Assert.Contains`/`DoesNotContain`/`StartsWith`):

| Text (source line) | Pinned at (tests/RackCad.UI.Tests/Workspace/…) |
|---|---|
| `NoDraftMarker = "—"` (field VM :22) | by the constant: `WorkspacePilotCloseSyncTests.cs:112, 145, 810`; `WorkspacePilotFieldViewModelTests.cs:509, 563, 700, 721, 884, 1280`; `WorkspacePilotFieldViewTests.cs:200, 576` |
| `"Listo para editar"` (:374) | exact `WorkspacePilotFieldViewModelTests.cs:131, 447`; `WorkspacePilotFieldViewTests.cs:153`; absent :536, 702, 723 |
| `"Sin borrador."` (:370, :400) | exact `WorkspacePilotCloseSyncTests.cs:113, 146, 362, 534, 657`; `WorkspacePilotCloseSyncViewTests.cs:339`; `WorkspacePilotFieldViewModelTests.cs:508, 1281, 1299` |
| `"No editable aquí"` (:370) | exact `WorkspacePilotFieldViewModelTests.cs:579, 590, 601, 1227`; `WorkspacePilotFieldViewTests.cs:644` (the diagnostic texts themselves come from the double) |
| `"Error: " + …`, `"sin detalle."` (:369, :371, :386) | exact `WorkspacePilotFieldViewModelTests.cs:1355, 1361-1364` (theory 1365-1376); `"Error"` absent :1387; `WorkspacePilotCloseSyncTests.cs:361, 591` (DoesNotContain "Error"), :526 (Contains), :763 (not StartsWith "Error") |
| state names `Aplicando`, `Desconocido`, `Sin decidir` (:396-399) | contains `WorkspacePilotFieldViewModelTests.cs:1016-1029` (theory) — `Limpio`, `Modificado`, `Conflicto` are pinned by **no** test |
| `" (posiblemente obsoleto)"` (:377) | contains "posiblemente obsoleto" `WorkspacePilotFieldViewModelTests.cs:619, 643, 664`; absent "obsoleto" :629 |
| `"El texto no se aplicó: "` + `"hay una lectura en curso."` / `"el campo no se puede editar ahora."` (:440-443) | exact `WorkspacePilotFieldViewModelTests.cs:1391-1394` (theory), :1417 |
| `RefusalOf`/`Describe` prefixes `"El texto no es válido: "`, `"En espera: "`, `"No disponible: "`, `"Rechazado: "`, `"Sin borrador: "`, `"Fallo: "`, `"No se pudo abrir la edición: "` (:442, :452-457) | **not pinned** (tests only check that the reason is contained, e.g. :542-567) |
| `"La superficie cambió durante la lectura; el resultado no se mostró."` (:209), `"No se recargó: el borrador se conserva tal como estaba."` (:225) | **not pinned** (only `NoticeText != ""`, :704-705, :727, :664 `NotEqual(string.Empty, …)`) |
| Browser confirmation texts (browser VM :1040-1049) | exact `WorkspacePilotCloseSyncTests.cs:1576, 1626, 1636`; contains :166-167, 435-449, 674, 1524, 1582, 1647 |
| Browser notices (browser VM :86-88, :460, :687-693, :937, :1137-1158) | exact `WorkspacePilotCloseSyncTests.cs:22 (const NoRisk), 675, 901, 946, 1212, 1230, 1244, 1480-1481 (consts), 1613`; contains :246-247 (incl. the double's English `"The double does not discard."`), 264, 333, 549, 650-651, 745-761, 781-813, 893, 909, 923, 1019, 1037-1038, 1131-1140, 1255, 1656-1657 |

- XAML literals pinned by source guards (Content / AutomationProperties.Name / bindings): see §9 (`WorkspacePilotCloseSyncGuardTests.cs:614-663, 728-741`,
  `WorkspaceBrowserHostGuardTests.cs:332-342`) and by UI view tests (`WorkspacePilotCloseSyncViewTests.cs:120-142`: «Descartar borradores de este documento»,
  «Descartar»/«Confirmar el descarte», «Cancelar»/«Cancelar el descarte», «Descartar»/«Descartar el borrador»; `WorkspaceBrowserViewTests.cs:250`: «Volver a leer»).
  Section anchors used by many guards: `Text="Navegador de racks"`, `Text="Navegación"`, `Text="Selección"`, `Text="Instrumentación"`.

## 6. The UI double `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs`

### 6.1 Full member list (`internal sealed class WorkspacePilotEditPortDouble : IWorkspacePilotEditPort`, :25)

- Constants (:27-41): `BeginOp="TryBeginEdit"`, `CheckOp="TryCheckNow"`, `ReloadOp="TryReload"`, `ConfirmBaseOp="TryConfirmBase"`,
  `KeepMineOp="TryKeepMine"`, `DiscardAndReloadOp="TryDiscardAndReload"`, `GetFieldOp`, `SetPendingOp`, `CommitOp`, `BoundaryOp`,
  `DiscardRackOp`, `DiscardDocumentOp`, `RisksOp="GetRacksAtRisk"`, `LossOp="TakeLossNotices"`, `CloseOp="TakeCloseNotices"`.
- `public sealed record Call(string Operation, SessionId Origin, string? RackId, string? Text = null, PilotBoundaryKind? Kind = null);` (:48).
- Private `_fields` dictionary `(SessionId, string) → PilotFieldView` (:50).
- Log: `List<Call> Calls` (:52); `int Count(string operation)` (:54); `IEnumerable<Call> Of(string operation)` (:56);
  `HashSet<string> Operations()` (:59, every operation called at least once); `IEnumerable<Call> Reads()` (:61).
- Hooks/responders: `Action<Call>? ReadHook` (:64); `Func<string, string?> Validate` (:67, valid by default);
  `BeginResponder` (:69), `ReloadResponder` (:71), `ConfirmBaseResponder` (:74, default `EditConfirmOutcome.Unavailable("The double does not confirm.")`),
  `KeepMineResponder` (:77, default `EditIntentOutcome.Unavailable("The double does not keep.")`), `DiscardAndReloadResponder` (:80, default
  `Unavailable("The double does not discard and reload.")`), `SetPendingResponder` (:82), `CommitResponder` (:84), `BoundaryResponder` (:86),
  `DiscardRackResponder` (:89, default `Rejected("The double does not discard.")`), `DiscardDocumentResponder` (:92, same default),
  `RisksResponder` (:95, default two empty lists = known, nothing at risk); `Queue<PilotLossView> LossQueue` (:98); `Queue<PilotCloseView> CloseQueue` (:101);
  `Dictionary<string, Exception> Throws` (:104, thrown after recording).
- **No `CheckNowResponder`**: `TryCheckNow` (:172-178) always answers `EditCheckOutcome.Unavailable("The double does not check.")`.
- Builders: `static PilotFieldView View(PilotFieldState state = Clean, string? committed = "1:50", string? pending = null, string? message = null, bool stale = false, bool isProtected = false, bool canEdit = true, string? committedReason = null, string? errorReason = null)` (:108-118)
  — **no parameter for `unknownOrigin`, `conflictText`, `lastCause`** (the three optional values of `PilotFieldView` added by F4-T4A1,
  `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:208-211`); `static PilotRiskView UnknownRisk()` (:121); `static PilotFieldView NoDraft()` (:123-124);
  `Set` (:126), `Remove` (:128), `Current` (:130-131), `DiscardAsTheHostDoes()` (:138-156).
- Port implementation (:160-329); the six reads all record, run `ReadHook`, then `ThrowIfAsked` (:162-164, 174-176, 182-184, 195-197, 204-206, 213-215).
  `SetPendingText`/`CrossBoundary` record with `Text`/`Kind` (:229, :263). Default model: Begin creates a clean draft or answers AlreadyOpen;
  Reload clears the stale mark; SetPending → Dirty + protected; Commit applies/Invalid; blocking boundary (TabChange/RackChange) Proceeded/Aborted/Kept.

### 6.2 `Reads()` and M-9

```csharp
// :61
public IEnumerable<Call> Reads() => Calls.Where(c => c.Operation == BeginOp || c.Operation == CheckOp || c.Operation == ReloadOp);
// :63-64
/// <summary>Runs inside TryBeginEdit, TryCheckNow and TryReload, as a read that pumps messages would.</summary>
public Action<Call>? ReadHook { get; set; }
```

- `Reads()` counts only `TryBeginEdit`, `TryCheckNow`, `TryReload`; it **omits `TryConfirmBase`, `TryKeepMine`, `TryDiscardAndReload`**,
  although they are recorded (:195, :204, :213) and run `ReadHook` (:196, :205, :214). The `ReadHook` doc comment (:63) is stale in the same way.
  (This is the review finding M-9, which cites `WorkspacePilotEditPortDouble.cs:61-63`.)
- Effect: the **30** `Reads()` assertions below would stay green if a presentation or gesture started calling one of the three
  reconciliation reads. They would only be caught by assertions on `Operations()`, `Calls` or `Count/Of` of those ops (none exists).

### 6.3 Every assertion on `Reads()`

| Expected | Sites |
|---|---|
| `Assert.Empty(…Reads())` (27) | `WorkspacePilotCloseSyncTests.cs:120, 150, 229, 415, 1461`; `WorkspacePilotCloseSyncViewTests.cs:236, 341`; `WorkspacePilotFieldViewModelTests.cs:110 (helper AssertNoReadAndNoMutation, used at :98 and :103), 484, 792, 1047, 1074, 1109, 1215, 1257, 1262, 1314`; `WorkspacePilotFieldViewTests.cs:273, 291, 352, 359, 380, 604` |
| `Assert.Empty(…Reads().Skip(1))` | `WorkspacePilotFieldViewModelTests.cs:644` (at most one read) |
| `Assert.Single(…Reads())` | `WorkspacePilotFieldViewModelTests.cs:1229, 1233`; `WorkspacePilotFieldViewTests.cs:313, 322` |
| `Assert.Equal(2, …Reads().Count())` | `WorkspacePilotFieldViewModelTests.cs:1239`; `WorkspacePilotFieldViewTests.cs:329` |

### 6.4 Assertions on the whole call log that DO see any new operation

| Site | Exact expectation |
|---|---|
| `WorkspacePilotCloseSyncTests.cs:70-71` helper `AssertOnly` = `Operations().SetEquals(...)` | used at :119 `{GetField, DiscardRack}`; :149 `{GetField, GetRacksAtRisk, DiscardDocument}`; :285 `{GetField}`; :307 `{GetField, DiscardRack}` |
| `WorkspacePilotCloseSyncTests.cs:1439-1476` | after `Calls.Clear()`: `Reads()` empty; 0 × SetPending/Commit/Boundary; `Operations() ⊆ {GetField, DiscardRack, DiscardDocument, GetRacksAtRisk, TakeLossNotices, TakeCloseNotices}`; `Count(DiscardDocument) == 1` |
| `WorkspacePilotFieldViewModelTests.cs:1115-1160` (T15) | `Operations() ⊆ {GetField, SetPendingText, CommitField, CrossBoundary, TryBeginEdit, TryReload, TakeLossNotices, TakeCloseNotices}`; 0 × DiscardRack, DiscardDocument, GetRacksAtRisk, **TryCheckNow** (no `Calls.Clear()`: covers construction, refreshes, tab changes, typing, commit) |
| `Assert.Empty(…Calls)` | `WorkspacePilotCloseSyncTests.cs:201, 734, 1366`; `WorkspacePilotCloseSyncViewTests.cs:161, 209`; `WorkspacePilotFieldViewModelTests.cs:86, 738`; `WorkspacePilotFieldViewTests.cs:126` |
| `Assert.DoesNotContain(…Calls, c => c.Operation != GetFieldOp)` | `WorkspacePilotCloseSyncTests.cs:975`; `WorkspacePilotFieldViewTests.cs:398` |
| `never`/`mutation` lists | `WorkspacePilotCloseSyncTests.cs:416-417` and `:1462-1463` `{SetPendingText, CommitField, CrossBoundary}` = 0; `WorkspacePilotFieldViewModelTests.cs:112-118` `{SetPendingText, CommitField, DiscardRack, DiscardDocument}` = 0; `:1151-1158` `{DiscardRack, DiscardDocument, GetRacksAtRisk, TryCheckNow}` = 0 |
| `T17` `WorkspacePilotFieldViewModelTests.cs:1318-1341` | every call's `Origin` is A except the rack R-9 (B); `Calls` not empty |
| Double self-test `WorkspacePilotCloseSyncTests.cs:1391-1434` | default answers of DiscardRack/DiscardDocument (Rejected), risks empty, queues empty, `DiscardAsTheHostDoes`, `Throws` for 5 ops — the six reads are not exercised |

Full sweep of the 180 call-log assertion lines (109 `Of`, 30 `Reads`, 19 `Count`, 9 `Operations`, 13 `Calls`) with test names: Appendix A.

## 7. Current behaviour of the field VM (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`)

### 7.1 Public surface today (no reflection test pins it; see §8)

`const string NoDraftMarker` (:22); ctor `(IWorkspacePilotEditPort port, Action changed = null)` (:49-53); `IsAvailable` (:58);
`Text` get/set (:64-98); `IsEditable` (:101); `IsReadOnly` (:103); `ShowEditButton` (:106); `CanOpenByGesture` (:113); `ShowReloadButton` (:116);
`ShowDiscardButton` (:119); `StateText` (:122); `MessageText` (:125); `NoticeText` (:128); `NotEditableText` (:131); `ClassicEditorHintText` (:134);
`Show(SessionId, string, bool)` (:142-155); `Clear()` (:158-168); `Fail(Exception)` (:171-175); `ShowNotice(string)` (:178-183);
`BeginEdit()` (:191-239); `CommitPending()` (:242-277); `Rematerialize()` (:280-294). `RaiseAll()` (:418-431) raises exactly the 11 bound properties.

### 7.2 States presented (`StateOf`, :367-380; `NameOf`, :389-402)

`PilotFieldState` (port `IWorkspacePilotEditPort.cs:105-124`) = `NotOpen, Clean, Dirty, Conflict, Applying, Unknown, Error, Undecidable` (ordered prefix pinned by `WorkspacePilotEditGuardTests.cs:82`).
Field text: failure → `"Error: " + message`; no draft → `"No editable aquí"` (with diagnostic) or `"Sin borrador."`; `Error` → `"Error: " + ErrorReason ?? CommittedTextReason ?? "sin detalle."`;
otherwise `"Listo para editar"` (ready note, editable, Clean, no pending) or `Limpio|Modificado|Conflicto|Aplicando|Desconocido|Error|Sin decidir`
(default `"Sin borrador."`), + `" (posiblemente obsoleto)"` when stale, + `": " + CommittedTextReason` when there is no committed text.
**The field VM never reads `UnknownOrigin`, `ConflictText` or `LastCause`** (no UI file names them).

### 7.3 Buttons that exist today (`Apply`, :351-365) and what they call

| Button (XAML) | Visibility | Gesture → port |
|---|---|---|
| «Editar» `BrowserFieldEditButton` (:215-219) | `ShowEditButton = no failure && session && (!Exists || (Exists && CanEdit && !editable))` | `OnBrowserFieldEditClick` → `Browser.BeginFieldEdit()` (pins a preview) → `Field.BeginEdit()`: `GetField`, then **`TryReload`** if the draft is Clean+IsPossiblyStale, else **`TryBeginEdit`**, with `new EditIntentRequest(session, rack, null, null)` (:200-203). Also reached by click/key on the read-only box (`BeginFieldEditFromBox`, guarded by `CanOpenByGesture`) and double click on the active tab (`BeginFieldEditFromTab`) |
| «Recargar» `BrowserFieldReloadButton` (:220-224) | `ShowReloadButton = failure || (Exists && State == Error)` | `OnBrowserFieldReloadClick` → `Browser.ReloadField()` → `Field.Rematerialize()`: **memory only** (`GetField`), **no `TryReload`** |
| «Descartar» `BrowserFieldDiscardButton` (:225-229) | `ShowDiscardButton = ShowReloadButton && Exists` | `OnBrowserFieldDiscardClick` → `Browser.DiscardField()`: `GetField`; protected → inline confirmation; else `DiscardRack` |

- There is **no «Comprobar» / «Comprobar ahora» button**, no button for `Unknown(CommitThrew)`, and **no UI call to `TryCheckNow`,
  `TryConfirmBase`, `TryKeepMine` or `TryDiscardAndReload`** (searched `src/RackCad.UI`: zero occurrences).
- `Unknown`, `Applying`, `Undecidable` with `canEdit: false` → read-only, no Edit, no Reload, no Discard (pinned
  `WorkspacePilotFieldViewModelTests.cs:1016-1029`). **`Conflict` with `canEdit: true` is editable today** (pinned by
  `CleanDirtyAndConflictWithTheRightToEditOnAPinnedTabGiveAnEditableBox`, `WorkspacePilotFieldViewModelTests.cs:1002-1014`); a Dirty+stale draft
  is editable (only Clean+stale is read-only, `EditableOf` :344-349; pinned :611-646).
- `BeginEdit` result handling (:214-230): `Admitted|AlreadyOpen|Reloaded|NotNeeded` → ready note; `NotAdmitted` → diagnostic + hint
  (+ notice when stale); other statuses → `Describe(...)` notice. Reentrancy: result not shown if the surface changed during the read (`IsMine`, :205-212).

### 7.4 Pending text and boundaries

- Entry of typed text: only the setter of `Text` (`SetPendingText`, :81) when editable; otherwise the box is restored (:71-76). Refusals → `RefusalOf` notice.
- Commit: `CommitPending()` on Enter (code-behind `PreviewKeyDown`, `src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs:127-131`) and on `LostFocus` (:158):
  `GetField`, then `CommitField` only with `PendingText != null && EditableOf(view)`.
- Boundaries are crossed by the **browser** VM, not the field VM (the field VM never names `PilotBoundaryKind`):
  - **Blocking** `TabChange` — `AllowTabChange` (`WorkspaceBrowserViewModel.cs:834-858`): only to another existing tab and only with pending text in
    the live tab; proceeds on `Proceeded` or `Kept`; otherwise `Field.ShowNotice("No se cambió de pestaña: …")` and the tab does not change; an exception → `Field.Fail(ex)`.
  - **Non-blocking** — `CrossObservedBoundaries` (:873-891): `DocumentChange` on the previous tab when the presented session changed;
    `Dematerialize` on the previous tab of the same session when it had a draft; failures swallowed.
  - `RackChange`, `PanelHide`, `DocumentClose` are not crossed by any UI file.
  - Retention by selection with pending text: `ApplySelection`/`KeepsActive` (:667-710), pinned by `WorkspaceHostHardeningGuardTests.cs:650-692`.

### 7.5 Tests that pin each behaviour (tests/RackCad.UI.Tests/Workspace/…)

| Behaviour | Tests |
|---|---|
| Wiring / no port | `WorkspacePilotFieldViewModelTests.cs:74-91, 93-104`; `WorkspacePilotFieldViewTests.cs:117-130` |
| Text, commit, invalid, mark | `WorkspacePilotFieldViewModelTests.cs:124-201`; view `WorkspacePilotFieldViewTests.cs:389-446, 515-581` |
| TabChange / DocumentChange / Dematerialize | `WorkspacePilotFieldViewModelTests.cs:203-388, 730-839, 1269-1303` |
| First gesture, preview pin, witness-less intention | `WorkspacePilotFieldViewModelTests.cs:390-502`; view `WorkspacePilotFieldViewTests.cs:133-300` |
| Outcomes of the intention (Admitted, AlreadyOpen, Waiting/Unavailable/Rejected/Failed, NotAdmitted) | `WorkspacePilotFieldViewModelTests.cs:504-609`; view :302-332, 631-650 |
| Lazy reload (`TryReload` for Clean+stale) | `WorkspacePilotFieldViewModelTests.cs:611-682` |
| Reentrancy of a read | `WorkspacePilotFieldViewModelTests.cs:684-728` |
| Setter / CommitField rules | `WorkspacePilotFieldViewModelTests.cs:841-998` |
| State → editable/read-only (Clean/Dirty/Conflict editable; Applying/Unknown/Undecidable read-only) | `WorkspacePilotFieldViewModelTests.cs:1002-1029` |
| Error, Reload (no read), containment | `WorkspacePilotFieldViewModelTests.cs:1032-1094, 1243-1267, 1346-1393`; view `WorkspacePilotFieldViewTests.cs:334-363, 583-629` |
| Presentation reads nothing | `WorkspacePilotFieldViewModelTests.cs:1096-1113`; oracle T15 :1115-1160 |
| K = 1 and isolation | `WorkspacePilotFieldViewModelTests.cs:1162-1198, 1318-1341`; view :678-690 |
| Gestures depend on state (double click, NotAdmitted, Error) | `WorkspacePilotFieldViewModelTests.cs:1202-1267`; view :229-277, 365-387 |
| Refusal texts | `WorkspacePilotFieldViewModelTests.cs:1395-1418` |
| Discard of the field in Error / Reload ↔ Discard visibility | `WorkspacePilotCloseSyncTests.cs:88-123, 491-595`; view `WorkspacePilotCloseSyncViewTests.cs:312-346` |
| Keys (N-3 repetition, Enter/Esc) | `WorkspacePilotCloseSyncViewTests.cs:472-570`; `WorkspacePilotFieldViewTests.cs:181-227, 279-300, 448-497` |
| No fixed sizes of field controls (fixed list of 9 names) | `WorkspacePilotFieldViewTests.cs:652-675` (new controls are not in the list); close-sync elements (8 names) `WorkspacePilotCloseSyncViewTests.cs:424-470`; browser section `WorkspaceBrowserViewTests.cs:367-401` |

## 8. Pins of the public members by reflection or by text

### 8.1 By text (exact)

- **Browser VM public shape — 33 exact signatures**: `WorkspacePilotCloseSyncGuardTests.cs:376-470` (`PublicShape` :379-414; `ExtractPublicShape`
  :416-436 = every line starting with `public` inside the class body, cut at the first `=>` or `{`, spaces collapsed, excluding the body of
  `StoredNotice`; `Assert.Equal(33, ...)` ×3 and set equality :442-445). **Adding any public member to the browser VM (e.g. a new gesture
  or a new `Has…`/`…Text` property) breaks this test** unless the list and the three `33` are changed as a declared change.
- Browser VM: `PublicSignatures` each exactly once (:309-334) and the two records (:327, :332); `WorkspaceBrowserTabItem` exact record also at
  `WorkspaceBrowserHostGuardTests.cs:276`.
- Field VM: only `public\s+bool\s+ShowDiscardButton\s*=>` exactly once (`WorkspacePilotCloseSyncGuardTests.cs:333`), the class header and the single
  public constructor prefix (`WorkspacePilotSurfaceGuardTests.cs:332-334`), and `public string Text` with a `set {` block (:159-166, needed by the
  SetPendingText guard). **No exact public shape of the field VM exists.**
- Panel VM: G-02 signature (3 sites, §2) and the documentation phrase ban (`WorkspacePilotCloseSyncGuardTests.cs:558`). **No exact public shape of the panel VM exists.**
- Code-behind: six handlers must be exactly `private void X(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.Y());`
  (`WorkspacePilotCloseSyncGuardTests.cs:781-795`: FieldDiscard→DiscardField, DiscardDocument→RequestDocumentDiscard, ConfirmDiscard, CancelDiscard,
  NoticeDiscardDocument→RequestDocumentDiscard, NoticeDismiss→DismissNotice(NoticeKeyOf(sender))) plus the three of G-04 (`=> Contained(`). New handlers are free in shape.

### 8.2 By reflection (UI tests)

- `WorkspaceBrowserViewModelTests.cs:948-962`: browser VM has no Task-returning or async methods, no events, no Timer/Thread/Task fields, is sealed (does not enumerate names).
- `WorkspaceBrowserViewModelTests.cs:760-786` and `WorkspaceBrowserScenarioHarnessTests.cs:140-197`: `WorkspaceBrowserTabItem` exact property set `{IsActive:Boolean, IsPreview:Boolean, RackId:String, Title:String}`, no events/delegates/WPF/AutoCAD types; browser `PropertyChanged` subscriber count unchanged with 10 tabs (`:149, :166`, via `ObservableObject.PropertyChanged` field, :221-227).
- `WorkspaceBrowserViewModelTests.cs:799-800`: `IWorkspaceBrowserSummaryPort` methods = `{IsSessionAlive, ReadSummary}`.
- `WorkspacePanelInstrumentationTests.cs:156-164`: `WorkspacePanelViewModel.Measurements` has no public setter and type `IReadOnlyList<WorkspaceMeasurementRow>`; a public `SetInstrumentation` exists.
- **No test pins the exact set of public members, commands or properties of `WorkspacePilotFieldViewModel` or `WorkspacePanelViewModel` by reflection.**
- No UI test counts all buttons of the panel: the only whole-view enumerations are `Assert.Single(Descendants<Button>(view).Where(b => b.Name == "BrowserSurfaceReloadButton"))`
  (`WorkspaceBrowserViewTests.cs:249`) and `Assert.Single(Descendants<TextBox>(view), b => b.Name == "BrowserFieldBox")` (`WorkspacePilotFieldViewTests.cs:686`).

## 9. XAML structure guards — what a new button/panel would hit

| Guard | Region | Rule |
|---|---|---|
| `WorkspacePilotCloseSyncGuardTests.cs:722-741` `CheckSurfaceButtons` (theory :743-756) | from the `<Border` of `x:Name="BrowserSurface"` to `Text="Navegación"` | **exactly 1** each of `Content="Editar"`, `Content="Recargar"`, `Content="Volver a leer"`, `Content="Descartar"`. A Yes/No inside the surface with `Content="Descartar"` or a second «Recargar» breaks it; `Content="Descartar y recargar"` and `Content="Conservar mi borrador"` do not match these literals |
| `WorkspaceBrowserHostGuardTests.cs:323-347` (G-04) | balanced `BrowserSurface` element | `>=3` `<Button`; 1 each Volver a leer/Editar/Recargar + handlers |
| `WorkspacePilotCloseSyncGuardTests.cs:614-663` `CheckNewXamlElements` (theory :665-692, 19 mutations) | browser section | `BrowserDiscardDocumentButton`, `BrowserConfirmDiscardButton`, `BrowserCancelDiscardButton`, `BrowserConfirmationPanel`, `BrowserConfirmationText`, `BrowserNoticeText`, `BrowserNoticeList` strictly between the end of `BrowserTabStrip` and `BrowserSurface`, with exact `Click=`, `AutomationProperties.Name=`, `Content=` and `{Binding Browser.…}`; `BrowserFieldDiscardButton` after the surface start with `Click="OnBrowserFieldDiscardClick"`, «Descartar el borrador», `Content="Descartar"`, `{Binding Browser.Field.ShowDiscardButton` |
| `WorkspacePilotCloseSyncGuardTests.cs:694-709` | browser section / whole XAML | new names not in the strip; no `Width/Height/MinWidth/…=`; no `<ContentControl|<Window\b|ShowDialog|MessageBox`; whole XAML no `IsDefault=`/`IsCancel=`; exactly 1 `x:Name="BrowserSurface"` |
| `WorkspaceBrowserHostGuardTests.cs:280-299` | tab template | the template `Border` has no `BorderThickness=`/`BorderBrush=` attributes; style setters/trigger exact |
| `WorkspaceBrowserHostGuardTests.cs:380-400` | browser section | after «Selección», before «Navegación»; no sizes; WrapPanel; no `<TabControl|<TabItem|<ContentControl`; tab handlers present |
| `WorkspacePilotSurfaceGuardTests.cs:232-244` | XAML | the one `BrowserFieldBox` binding and its 3 handlers |
| `WorkspacePaletteHostSourceGuardTests.cs:614-626` | XAML | `<UserControl.Resources>`, `Source="/RackCad.UI;component/Themes/AppStyles.xaml"`, every `Source` with `;component/`, no `Application.Resources|Application.Current`, `AutomationProperties.Name="Workspace"`; no `src/RackCad.UI/App.xaml` |
| `tests/RackCad.UI.Tests/UiSystemBoundaryGuardTests.cs:106-149, 159-189` | every UI XAML (and `.cs` for URIs) | `x:Class` = folder namespace + file name, code-behind beside it with the same namespace; pack URIs assembly-rooted |

## 10. UI-wide and product-wide guards that also read the UI files

- `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs:86-131` — any new `Window` type in RackCad.UI must be censused (reflection).
- `tests/RackCad.Tests/NamespaceFolderGuardTests.cs:100-135` (tests at :101 and :113) — every production `.cs` declares the namespace of its folder, one block-scoped namespace.
- `tests/RackCad.Tests/LengthUnitsAuthorityGuardTests.cs:35-47` (sweep) and test :64 — every product `.cs` (UI included): no unit literals like `"mm"`, `"in"`, `"ft"` arrays, no `25.4`, no `/ 12` conversions outside the authority (self-tests :88, :105).
- `tests/RackCad.Tests/Workspace/WorkspaceNavigationPrivacyGuardTests.cs:22-42` — `src/RackCad.UI/Workspace/` is a panel path (allowed to name navigation types).
- `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs:155-165` — files outside the Plugin/Application workspace (UI included) must not name `WorkspaceController|WorkspaceHostPort|WorkspaceSelectionReader|WorkspaceHostCore`.
- `WorkspacePilotCloseSyncGuardTests.cs:964-984` — the T3B2 UI test classes keep namespace `RackCad.UI.Tests` and names starting with `Workspace` (the filter selects them by `Workspace` prefix); new UI test classes should follow the same rule.
- Other product-wide scanners (`CustomPropertiesEdgeGuardTests.cs:266-276`, `CustomPropertiesEnvelopeGuardTests.cs:223-236`, `ElevationOverrideSourceGuardTests.cs:36-40`,
  `ExpressionCoreGuardTests.cs:82`, `SelectiveEditorOpenTests.cs:487`, `StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs:437`) match names of their own domains only.

## 11. Interaction checklist for T4A2 (consequences, not proposals)

| Change in T4A2 | What constrains or breaks |
|---|---|
| New public gesture/property on the **browser** VM | breaks `WorkspacePilotCloseSyncGuardTests.cs:438-452` (33 signatures) — declared change needed; `TryCheckNow` (and `TryBeginEdit`, `TryReload`) anywhere in the browser VM breaks `:849/:860` |
| New public gesture/property on the **field** VM | no shape guard; `TryCheckNow` allowed in a member not named `BeginEdit|CommitPending|Rematerialize|BeginFieldEdit|BeginFieldEditFromBox|BeginFieldEditFromTab|ReloadField|Present|Refresh|Show|Clear|RebuildTabs|SyncSurface|StartSurface|ClearSurface|Materialize` and not in a setter/ctor (`WorkspacePilotSurfaceGuardTests.cs:56-97`); no `+=`, `event`, `delegate`, static field, `Task`, `Dispatcher`, `Keyboard`, `.Focus(` (§3); no test enumerates the property names raised by `RaiseAll` (only `Text` is asserted, `WorkspacePilotFieldViewModelTests.cs:885`), so new bindable properties need their own `OnPropertyChanged` but break nothing |
| Change of the field VM constructor | prefix regex `WorkspacePilotFieldViewModel\s*\(\s*IWorkspacePilotEditPort\s+\w+\s*,\s*Action\s+\w+` and exactly 1 public ctor (`WorkspacePilotSurfaceGuardTests.cs:333-334`); 1 `new WorkspacePilotFieldViewModel(` only in the browser VM (:347-348); a new dependency would have to come through the browser ctor → breaks G-03 / shape / `:343-345`, and through the panel ctor → breaks G-02 (3 sites) and G-01 (3 sites) |
| «Comprobar ahora» calling `TryCheckNow` from Unknown | today `BeginEdit` never calls it; putting it inside `BeginEdit`/`Rematerialize` breaks `WorkspacePilotSurfaceGuardTests.cs:90-92`; T15 (`WorkspacePilotFieldViewModelTests.cs:1151-1158`) requires 0 × `TryCheckNow` in its tab-change scenario; the double has no responder for it (§6.1) |
| Making Conflict read-only until a choice is made | breaks `WorkspacePilotFieldViewModelTests.cs:1002-1014` (Conflict + canEdit ⇒ editable) |
| Changing the `StateText` of Unknown/Applying/Undecidable | `WorkspacePilotFieldViewModelTests.cs:1016-1029` requires it to **contain** `Desconocido`/`Aplicando`/`Sin decidir` and the box read-only with text `1:75` |
| «Descartar y recargar» via `DiscardRack` | `DiscardRack` only in `CloseTab|DiscardField|ConfirmDiscard` of the browser VM (`WorkspacePilotCloseSyncGuardTests.cs:115-123`); via `TryDiscardAndReload` no guard |
| New button in the surface | allowed by G-04; must not duplicate `Content="Editar"|"Recargar"|"Volver a leer"|"Descartar"` in the surface-to-«Navegación» region (`:728-741`); no sizes, no `IsDefault`/`IsCancel`; handler in code-behind (`Contained`) |
| Yes/No confirmation | inline only (§4.2); reuse of `BrowserConfirmationPanel` keeps exactly «Descartar»/«Cancelar» (T12 `WorkspacePilotCloseSyncViewTests.cs:181-190`; guard `:642-643`); single `ConfirmationKind` slot semantics pinned (§4.2) |
| Keyboard for the confirmation | no second `PreviewKeyDown=`, no `KeyUp` in XAML; no `Keyboard`/`.Focus(`/`e.KeyboardDevice`/`e.SystemKey` in code-behind (M-6, §3.1) |
| New Spanish texts | no registry; must avoid `Provider|Metric|\bBom|Aggregat|ProjectSummary|TotalRacks|ComputedParameter` (IgnoreCase) and `Registry|RackLog|File.|…` (§3.4); host texts (if any) go to the 30-row table |
| Fixing M-9 (`Reads()` with the 3 new ops) | strengthens the 30 sites of §6.3; none of them calls a reconciliation op today, so their expectations do not change |

---

## Appendix A — every call-log assertion on the pilot double (regex sweep at HEAD)

Pattern: lines of `tests/RackCad.UI.Tests/Workspace/*.cs` (the double itself excluded) with `.Reads()`, `.Operations()`/`Ops(`/`AssertOnly(`,
`.Count(<Op>|never|mutation)`, `.Of(WorkspacePilotEditPortDouble.<Op>)` or an `Assert` on `…Pilot.Calls`/`pilot.Calls`. The `WorkspacePilotEditPortDouble.`
prefix is removed from the op constants. "(helper)" marks a private helper. Lines that only clear the log (`Calls.Clear()`) are not listed.

| File:line | Test (or helper) | Kind | Assertion (op constants without the class prefix) |
|---|---|---|---|
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:68` | (helper) Ops | Operations | `private static HashSet<string> Ops(WorkspacePilotEditPortDouble pilot) => pilot.Operations();` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:70` | (helper) AssertOnly | Operations | `private static void AssertOnly(WorkspacePilotEditPortDouble pilot, params string[] operations) =>` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:71` | (helper) AssertOnly | Operations | `Assert.True(pilot.Operations().SetEquals(operations), "called: " + string.Join(",", pilot.Operations().OrderBy(o => o, StringComparer.Ordinal)));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:103` | T11_DiscardingTheFieldInErrorRemovesTheDraftAndThePendingTextAndCallsExactlyTheDiscardAndReadsNothing | Of | `Assert.Empty(pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:107` | T11_DiscardingTheFieldInErrorRemovesTheDraftAndThePendingTextAndCallsExactlyTheDiscardAndReadsNothing | Of | `var discard = Assert.Single(pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:119` | T11_DiscardingTheFieldInErrorRemovesTheDraftAndThePendingTextAndCallsExactlyTheDiscardAndReadsNothing | Operations | `AssertOnly(pilot, GetFieldOp, DiscardRackOp);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:120` | T11_DiscardingTheFieldInErrorRemovesTheDraftAndThePendingTextAndCallsExactlyTheDiscardAndReadsNothing | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:140` | T11_DiscardingTheDraftsOfTheDocumentRemovesThemAndThePendingTextAndCallsExactlyTheRisksAndTheDiscard | Count | `Assert.Equal(2, pilot.Count(RisksOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:141` | T11_DiscardingTheDraftsOfTheDocumentRemovesThemAndThePendingTextAndCallsExactlyTheRisksAndTheDiscard | Of | `var discard = Assert.Single(pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:149` | T11_DiscardingTheDraftsOfTheDocumentRemovesThemAndThePendingTextAndCallsExactlyTheRisksAndTheDiscard | Operations | `AssertOnly(pilot, GetFieldOp, RisksOp, DiscardDocumentOp);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:150` | T11_DiscardingTheDraftsOfTheDocumentRemovesThemAndThePendingTextAndCallsExactlyTheRisksAndTheDiscard | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:170` | T12_ClosingAProtectedTabAsksInlineAndDoesNotCloseIt | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:171` | T12_ClosingAProtectedTabAsksInlineAndDoesNotCloseIt | Of | `Assert.Empty(rig.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:185` | T12_AProtectedTabIsAlsoOneWhenThePortCannotSayAndItAsks | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:201` | T13_CancelKeepsTheTabTheDraftThePendingTextAndTheMarkAndCallsNothing | Calls | `Assert.Empty(rig.Pilot.Calls);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:221` | T14_DiscardCallsDiscardRackWithThatSessionAndRackClosesTheTabAndRemovesTheState | Of | `var discard = Assert.Single(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:229` | T14_DiscardCallsDiscardRackWithThatSessionAndRackClosesTheTabAndRemovesTheState | Reads | `Assert.Empty(rig.Pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:230` | T14_DiscardCallsDiscardRackWithThatSessionAndRackClosesTheTabAndRemovesTheState | Of | `Assert.Empty(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:231` | T14_DiscardCallsDiscardRackWithThatSessionAndRackClosesTheTabAndRemovesTheState | Of | `Assert.Empty(rig.Pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:243` | TheRejectionOfTheConfirmationDoesNotCloseTheTabAndSaysWhy | Of | `Assert.Single(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:284` | ATabWithNoDraftClosesWithoutAnyDiscard | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:285` | ATabWithNoDraftClosesWithoutAnyDiscard | Operations | `AssertOnly(rig.Pilot, GetFieldOp);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:300` | T14b_AKnownCleanTabClosesWithoutAnAuthoredWriteAndWithoutAnUpdate | Of | `var discard = Assert.Single(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:307` | T14b_AKnownCleanTabClosesWithoutAnAuthoredWriteAndWithoutAnUpdate | Operations | `AssertOnly(rig.Pilot, GetFieldOp, DiscardRackOp);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:330` | ACleanDraftClosesWhateverTheDiscardAnswersAndTheNoticeSaysItStayedInMemory | Of | `Assert.Single(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:382` | ClosingATabThatIsNotTheActiveOneKeepsTheActiveTabAndSurface | Of | `Assert.Equal("R-2", Assert.Single(rig.Pilot.Of(DiscardRackOp)).RackId);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:397` | ClosingATabThatDoesNotExistChangesNothingAndCallsNothingToDiscard | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:415` | T14c_ClosingNeverReadsTheIntentionNorWritesAndNeverTouchesTheNavigation | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:417` | T14c_ClosingNeverReadsTheIntentionNorWritesAndNeverTouchesTheNavigation | Count | `Assert.Equal(0, pilot.Count(never));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:465` | AChangeOfThePresentedSessionRemovesTheConfirmation | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:469` | AChangeOfThePresentedSessionRemovesTheConfirmation | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:485` | TheConfirmationOfTheFieldGoesWhenTheLiveSurfaceChanges | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:532` | DiscardingTheFieldOfAnUnprotectedDraftInAFailureDiscardsAtOnceAndTheTabStays | Of | `Assert.Single(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:607` | WithNoRacksAtRiskTheDocumentDiscardOnlySaysSoAndHasNoEffect | Count | `Assert.Equal(1, rig.Pilot.Count(RisksOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:608` | WithNoRacksAtRiskTheDocumentDiscardOnlySaysSoAndHasNoEffect | Of | `Assert.Empty(rig.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:609` | WithNoRacksAtRiskTheDocumentDiscardOnlySaysSoAndHasNoEffect | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:626` | WithRacksAtRiskTheConfirmationListsThemByTitleAndTheUncertainOnesApart | Of | `var asked = Assert.Single(rig.Pilot.Of(RisksOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:628` | WithRacksAtRiskTheConfirmationListsThemByTitleAndTheUncertainOnesApart | Of | `Assert.Empty(rig.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:646` | ConfirmingTheDocumentDiscardReadsTheRisksAgainAndDiscardsWhenTheyDidNotChange | Count | `Assert.Equal(1, rig.Pilot.Count(RisksOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:647` | ConfirmingTheDocumentDiscardReadsTheRisksAgainAndDiscardsWhenTheyDidNotChange | Of | `var discard = Assert.Single(rig.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:672` | TheTocTouListThatChangedIsNotDiscardedAndIsShownAgainWithItsNoticeAndTheOneThatDidNotChangeIsDiscarded | Of | `Assert.Empty(rig.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:679` | TheTocTouListThatChangedIsNotDiscardedAndIsShownAgainWithItsNoticeAndTheOneThatDidNotChangeIsDiscarded | Of | `Assert.Single(rig.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:680` | TheTocTouListThatChangedIsNotDiscardedAndIsShownAgainWithItsNoticeAndTheOneThatDidNotChangeIsDiscarded | Count | `Assert.Equal(2, rig.Pilot.Count(RisksOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:692` | TheTocTouComparisonIsInOrderAndOrdinalAndAnEmptyNewListRemovesTheConfirmation | Of | `Assert.Empty(order.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:701` | TheTocTouComparisonIsInOrderAndOrdinalAndAnEmptyNewListRemovesTheConfirmation | Of | `Assert.Empty(ordinal.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:709` | TheTocTouComparisonIsInOrderAndOrdinalAndAnEmptyNewListRemovesTheConfirmation | Of | `Assert.Empty(uncertain.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:717` | TheTocTouComparisonIsInOrderAndOrdinalAndAnEmptyNewListRemovesTheConfirmation | Of | `Assert.Empty(empty.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:734` | CancelOfTheDocumentDiscardCallsNothing | Calls | `Assert.Empty(rig.Pilot.Calls);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:975` | UnderstoodRemovesOnlyItsRowAndCallsNothingOfThePort | Calls | `Assert.DoesNotContain(rig.Pilot.Calls, c => c.Operation != GetFieldOp);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1092` | TheQueuesAreConsumedOnlyByThePresentationAndNeverByAGesture | Count | `Assert.Equal(0, rig.Pilot.Count(LossOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1093` | TheQueuesAreConsumedOnlyByThePresentationAndNeverByAGesture | Count | `Assert.Equal(0, rig.Pilot.Count(CloseOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1097` | TheQueuesAreConsumedOnlyByThePresentationAndNeverByAGesture | Count | `Assert.Equal(1, rig.Pilot.Count(LossOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1098` | TheQueuesAreConsumedOnlyByThePresentationAndNeverByAGesture | Count | `Assert.Equal(1, rig.Pilot.Count(CloseOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1108` | ThePanelConsumesTheQueuesWhenItIsBuiltAndOnEveryRefresh | Count | `Assert.Equal(1, pilot.Count(LossOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1109` | ThePanelConsumesTheQueuesWhenItIsBuiltAndOnEveryRefresh | Count | `Assert.Equal(1, pilot.Count(CloseOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1115` | ThePanelConsumesTheQueuesWhenItIsBuiltAndOnEveryRefresh | Count | `Assert.Equal(2, pilot.Count(LossOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1213` | ASelectionWithPendingTextOpensTheRackBehindWithANoticeAndKeepsTheActiveTab | Of | `Assert.Empty(rig.Pilot.Of(BoundaryOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1333` | ATabThatIsOnlyDirtyIsNotRetainedTheRackIsActivatedAndThePreviousOneCrossesDematerialize | Of | `var crossing = Assert.Single(rig.Pilot.Of(BoundaryOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1366` | WithoutTheEditPortClosingAndPresentingAreTheOnesOfBeforeWithNoCallsAndNoRetention | Calls | `Assert.Empty(rig.Pilot.Calls);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1461` | TheWholeCloseAndDiscardScenarioNeverReadsTheIntentionNorWritesNorChecksNorTouchesTheNavigation | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1463` | TheWholeCloseAndDiscardScenarioNeverReadsTheIntentionNorWritesNorChecksNorTouchesTheNavigation | Count | `Assert.Equal(0, pilot.Count(never));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1473` | TheWholeCloseAndDiscardScenarioNeverReadsTheIntentionNorWritesNorChecksNorTouchesTheNavigation | Operations | `Assert.True(Ops(pilot).IsSubsetOf(allowed), string.Join(",", Ops(pilot).Except(allowed)));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1474` | TheWholeCloseAndDiscardScenarioNeverReadsTheIntentionNorWritesNorChecksNorTouchesTheNavigation | Count | `Assert.Equal(1, pilot.Count(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1496` | AnUnknownRiskIsNeverNothingAtRiskAndAsksNothingAndDiscardsNothing | Count | `Assert.Equal(1, rig.Pilot.Count(RisksOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1497` | AnUnknownRiskIsNeverNothingAtRiskAndAsksNothingAndDiscardsNothing | Of | `Assert.Empty(rig.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1498` | AnUnknownRiskIsNeverNothingAtRiskAndAsksNothingAndDiscardsNothing | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1544` | AnUnknownSecondLookDoesNotDiscardRemovesTheConfirmationAndSaysSo | Count | `Assert.Equal(1, rig.Pilot.Count(RisksOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1545` | AnUnknownSecondLookDoesNotDiscardRemovesTheConfirmationAndSaysSo | Of | `Assert.Empty(rig.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1563` | AKnownEmptySecondLookStillSaysThereIsNothingAtRisk | Of | `Assert.Empty(rig.Pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:1628` | TheClosingConfirmationOfATabThatCouldNotBeReadIsNeutralAndAnyOtherRequestRemovesTheMemoryOfIt | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:161` | WithoutTheEditPortNothingOfTheTaskIsOfferedAndNoCallReachesAnyPort | Calls | `Assert.Empty(pilot.Calls);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:191` | T12_ClosingAProtectedTabShowsTheInlineConfirmationWithExactlyDescartarAndCancelarAndDoesNotClose | Of | `Assert.Empty(pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:209` | T13_CancelarRemovesTheConfirmationKeepsTheTabAndCallsNothing | Calls | `Assert.Empty(pilot.Calls);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:229` | T14_DescartarCallsDiscardRackWithThatSessionAndRackAndClosesTheTab | Of | `var discard = Assert.Single(pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:236` | T14_DescartarCallsDiscardRackWithThatSessionAndRackAndClosesTheTab | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:253` | TheRefusalOfTheDiscardShowsItsNoticeOnTheViewAndTheTabStays | Of | `Assert.Single(pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:298` | TheDiscardDocumentButtonWithNoRacksAtRiskShowsTheNoticeAndWithSomeAsksInline | Of | `Assert.Empty(pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:307` | TheDiscardDocumentButtonWithNoRacksAtRiskShowsTheNoticeAndWithSomeAsksInline | Of | `Assert.Empty(pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:331` | TheDiscardButtonOfTheFieldInErrorAsksInlineAndDescartarDiscardsTheDraftAndTheTabStays | Of | `Assert.Empty(pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:336` | TheDiscardButtonOfTheFieldInErrorAsksInlineAndDescartarDiscardsTheDraftAndTheTabStays | Of | `Assert.Single(pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:341` | TheDiscardButtonOfTheFieldInErrorAsksInlineAndDescartarDiscardsTheDraftAndTheTabStays | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:372` | TheRowOfAVetoOfThePresentedSessionHasItsDiscardButtonAndUnderstoodRemovesTheRow | Count | `Assert.Equal(1, pilot.Count(RisksOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:417` | TheRowOfAVetoOfAnotherSessionHasNoDiscardButtonAndTheRowOfALossIsRemovedOnlyByUnderstood | Of | `Assert.Empty(pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:465` | TheNewElementsAreBetweenTheStripAndTheSurfaceOutsideTheStripAndHaveNoFixedSize | Of | `Assert.Empty(pilot.Of(DiscardDocumentOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:483` | N3_TheRepetitionOfTheKeyThatLaunchedTheGestureIsHandledOnTheEditableBoxAndTheRepetitionOfAnotherKeyIsNot | Of | `Assert.Single(pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:489` | N3_TheRepetitionOfTheKeyThatLaunchedTheGestureIsHandledOnTheEditableBoxAndTheRepetitionOfAnotherKeyIsNot | Of | `Assert.Empty(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:490` | N3_TheRepetitionOfTheKeyThatLaunchedTheGestureIsHandledOnTheEditableBoxAndTheRepetitionOfAnotherKeyIsNot | Of | `Assert.Empty(pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:537` | N3_EnterAndEscKeepTheirRuleAndTheRepetitionOfEnterThatLaunchedTheGestureDoesNotCommit | Of | `Assert.Single(pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:544` | N3_EnterAndEscKeepTheirRuleAndTheRepetitionOfEnterThatLaunchedTheGestureDoesNotCommit | Of | `Assert.Empty(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:548` | N3_EnterAndEscKeepTheirRuleAndTheRepetitionOfEnterThatLaunchedTheGestureDoesNotCommit | Of | `Assert.Single(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:550` | N3_EnterAndEscKeepTheirRuleAndTheRepetitionOfEnterThatLaunchedTheGestureDoesNotCommit | Of | `Assert.Single(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:566` | N3_AReadOnlyBoxStillHandlesTheRepetitionsAndLaunchesOnlyOnce | Of | `Assert.Single(pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:86` | TheFieldIsNeverNullAndWithoutThePortTheBrowserIsTheOneOfBefore | Calls | `Assert.Empty(rig.Pilot.Calls);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:110` | (helper) AssertNoReadAndNoMutation | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:118` | (helper) AssertNoReadAndNoMutation | Count | `Assert.Equal(0, pilot.Count(mutation));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:136` | T01_AValidTextAndEnterKeepTheExactTextAndCommitItAndTheFieldShowsTheCommittedOneWithNoPending | Of | `var typed = Assert.Single(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:140` | T01_AValidTextAndEnterKeepTheExactTextAndCommitItAndTheFieldShowsTheCommittedOneWithNoPending | Of | `var commit = Assert.Single(rig.Pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:162` | T02_AnInvalidTextStaysInTheBoxWithItsMessageAfterTheCommitAnswersInvalid | Of | `Assert.Single(rig.Pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:214` | T04_ActivatingAnotherTabWithAnInvalidTextAbortsTheCrossingKeepsTheTabAndTheTextAndShowsANotice | Of | `var crossing = Assert.Single(rig.Pilot.Of(BoundaryOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:274` | T05_ADocumentChangeWithAnInvalidTextProceedsCrossesDocumentChangeOnTheTabOfAAndValidatesNothing | Of | `var crossing = Assert.Single(rig.Pilot.Of(BoundaryOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:297` | T05_ActivatingAnotherTabFromADraftThatCannotBeEditedWithPendingTextKeepsAndTheChangeProceeds | Of | `var crossing = Assert.Single(rig.Pilot.Of(BoundaryOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:326` | T06_ComingBackToTheTabShowsTheExactTextOfTheDraftAfterAProceededCrossingAndAfterAKeptOne | Of | `Assert.Single(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:339` | T06_ComingBackToTheTabShowsTheExactTextOfTheDraftAfterAProceededCrossingAndAfterAKeptOne | Of | `var keptCrossing = Assert.Single(kept.Pilot.Of(BoundaryOp), c => c.Kind == PilotBoundaryKind.TabChange);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:347` | T06_ComingBackToTheTabShowsTheExactTextOfTheDraftAfterAProceededCrossingAndAfterAKeptOne | Of | `Assert.Empty(kept.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:364` | T07_AfterADocumentChangeThePendingTextOfAStaysInA | Of | `Assert.Empty(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:365` | T07_AfterADocumentChangeThePendingTextOfAStaysInA | Of | `Assert.Empty(rig.Pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:366` | T07_AfterADocumentChangeThePendingTextOfAStaysInA | Of | `Assert.Empty(rig.Pilot.Of(DiscardRackOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:403` | T09_TheFirstGesturePinsThePreviewBeforeAskingForTheIntentionWithTheEditButtonAndWithTheDoubleClick | Of | `var request = Assert.Single(rig.Pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:439` | T10_ATabWithADraftIsNeverEditableWhileItIsAPreviewAndTheGestureFixesItAndTheDraftIsAlreadyOpen | Of | `Assert.Empty(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:445` | T10_ATabWithADraftIsNeverEditableWhileItIsAPreviewAndTheGestureFixesItAndTheDraftIsAlreadyOpen | Of | `Assert.Single(rig.Pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:463` | T10_AClosedTabOfACleanDraftThatComesBackAsAPreviewIsReadOnlyUntilTheFirstGesture | Of | `Assert.Empty(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:468` | T10_AClosedTabOfACleanDraftThatComesBackAsAPreviewIsReadOnlyUntilTheFirstGesture | Of | `Assert.Single(rig.Pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:484` | T10_APreviewWithAProtectedDraftIsPinnedWhenTheStripIsRebuiltAndTheFieldIsAlreadyEditable | Reads | `Assert.Empty(rig.Pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:518` | AdmittedAndAlreadyOpenRenderTheFieldFromTheDraftAndTheGestureDoesNotApplyAnything | Of | `Assert.Empty(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:519` | AdmittedAndAlreadyOpenRenderTheFieldFromTheDraftAndTheGestureDoesNotApplyAnything | Of | `Assert.Empty(rig.Pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:534` | AlreadyOpenOnADraftThatCannotBeEditedShowsItsStateAndNotTheReadyNote | Of | `Assert.Single(rig.Pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:621` | ACleanDraftThatIsPossiblyStaleIsReadOnlyTypingDoesNotCallSetPendingTextAndTheGestureCallsTheReload | Of | `Assert.Empty(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:626` | ACleanDraftThatIsPossiblyStaleIsReadOnlyTypingDoesNotCallSetPendingTextAndTheGestureCallsTheReload | Of | `Assert.Single(rig.Pilot.Of(ReloadOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:627` | ACleanDraftThatIsPossiblyStaleIsReadOnlyTypingDoesNotCallSetPendingTextAndTheGestureCallsTheReload | Of | `Assert.Empty(rig.Pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:644` | TheStaleMarkThatArrivesWithTheSurfaceAliveMakesTheBoxReadOnlyAtTheNextPresentation | Reads | `Assert.Empty(rig.Pilot.Reads().Skip(1));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:738` | AnotherClickOnTheActiveTabCallsNothingAndAChangeFromACleanDraftDoesNotCrossTheBoundary | Calls | `Assert.Empty(rig.Pilot.Calls);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:742` | AnotherClickOnTheActiveTabCallsNothingAndAChangeFromACleanDraftDoesNotCrossTheBoundary | Of | `Assert.Empty(rig.Pilot.Of(BoundaryOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:755` | ActivatingATabThatDoesNotExistCallsNoBoundary | Of | `Assert.Empty(rig.Pilot.Of(BoundaryOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:772` | AValidPendingTextProceedsWhenAnotherTabIsActivated | Of | `Assert.Single(rig.Pilot.Of(BoundaryOp), c => c.Kind == PilotBoundaryKind.TabChange);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:792` | AnAbortedActivationDoesNotPinNorAskForTheIntentionWithTheDoubleClickOfThatTab | Reads | `Assert.Empty(rig.Pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:814` | ASelectionThatActivatesAnotherRackCrossesDematerializeOnThePreviousTabWhenItHadADraftWithNoPending | Of | `var crossing = Assert.Single(rig.Pilot.Of(BoundaryOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:817` | ASelectionThatActivatesAnotherRackCrossesDematerializeOnThePreviousTabWhenItHadADraftWithNoPending | Of | `Assert.DoesNotContain(rig.Pilot.Of(BoundaryOp), c => c.Kind == PilotBoundaryKind.TabChange);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:829` | ASelectionThatLeavesATabWithoutADraftCrossesNothingAndATabChangeNeverCrossesDematerialize | Of | `Assert.Empty(rig.Pilot.Of(BoundaryOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:833` | ASelectionThatLeavesATabWithoutADraftCrossesNothingAndATabChangeNeverCrossesDematerialize | Of | `Assert.DoesNotContain(rig.Pilot.Of(BoundaryOp), c => c.Kind == PilotBoundaryKind.Dematerialize);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:835` | ASelectionThatLeavesATabWithoutADraftCrossesNothingAndATabChangeNeverCrossesDematerialize | Of | `Assert.DoesNotContain(rig.Pilot.Of(BoundaryOp), c => c.Kind == PilotBoundaryKind.Dematerialize);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:883` | TheSetterOfAFieldThatIsNotEditableDoesNothingAndRestoresTheText | Of | `Assert.Empty(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:930` | ARejectedNotEditableOrBusyTextRestoresTheBoxFromTheDraftAndShowsANotice | Of | `Assert.Single(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:947` | ShowAndPresentNeverCallSetPendingTextEvenWithAPendingText | Of | `Assert.Empty(rig.Pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:948` | ShowAndPresentNeverCallSetPendingTextEvenWithAPendingText | Of | `Assert.Empty(rig.Pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:964` | CommitFieldIsNotCalledWithoutPendingTextAndALostFocusAfterAWaitingGestureDoesNotOverwriteTheNotice | Of | `Assert.Empty(rig.Pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:970` | CommitFieldIsNotCalledWithoutPendingTextAndALostFocusAfterAWaitingGestureDoesNotOverwriteTheNotice | Of | `Assert.Empty(rig.Pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:982` | CommitFieldIsNotCalledWithPendingTextOnADraftThatCannotBeEdited | Of | `Assert.Empty(rig.Pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1047` | AnErrorFromTheDraftLeavesTheBoxReadOnlyWithTheReasonAndTheReloadButtonThatReadsNothing | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1074` | AFailureOfTheViewModelIsContainedInTheFieldAndTheReloadButtonRecoversWithoutAnyRead | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1109` | PresentAndRefreshNeverCallTheThreeReads | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1150` | T15_ChangingTabsCallsNoNavigationAndNeverDiscardsNorChecksNorReadsTheRisks | Operations | `Assert.True(pilot.Operations().IsSubsetOf(allowedOrNotices), string.Join(",", pilot.Operations().Except(allowedOrNotices)));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1158` | T15_ChangingTabsCallsNoNavigationAndNeverDiscardsNorChecksNorReadsTheRisks | Count | `Assert.Equal(0, pilot.Count(never));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1215` | TheDoubleClickOnTheActiveTabWithAnEditableBoxDoesNothingAndKeepsTheNoticeOfTheSurface | Reads | `Assert.Empty(rig.Pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1229` | WithANotAdmittedDiagnosticOfTheSameSurfaceTheDoubleClickDoesNotReadAndOnlyTheEditButtonReadsAgain | Reads | `Assert.Single(rig.Pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1233` | WithANotAdmittedDiagnosticOfTheSameSurfaceTheDoubleClickDoesNotReadAndOnlyTheEditButtonReadsAgain | Reads | `Assert.Single(rig.Pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1239` | WithANotAdmittedDiagnosticOfTheSameSurfaceTheDoubleClickDoesNotReadAndOnlyTheEditButtonReadsAgain | Reads | `Assert.Equal(2, rig.Pilot.Reads().Count());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1257` | WithAnErrorOfTheFieldNoGestureReadsAndOnlyReloadActs | Reads | `Assert.Empty(rig.Pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1262` | WithAnErrorOfTheFieldNoGestureReadsAndOnlyReloadActs | Reads | `Assert.Empty(rig.Pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1314` | WhenThePortCannotSayTheTabIsTreatedAsProtectedWithTheMarkAndAPreviewIsPinned | Reads | `Assert.Empty(rig.Pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1332` | T17_EveryCallCarriesTheSessionAndTheRackOfItsOwnSurfaceAndTheOnesOfANeverCarryB | Calls | `Assert.NotEmpty(rig.Pilot.Calls);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1339` | T17_EveryCallCarriesTheSessionAndTheRackOfItsOwnSurfaceAndTheOnesOfANeverCarryB | Calls | `Assert.Contains(rig.Pilot.Calls, c => c.RackId == "R-9" && c.Origin == B);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1340` | T17_EveryCallCarriesTheSessionAndTheRackOfItsOwnSurfaceAndTheOnesOfANeverCarryB | Calls | `Assert.Contains(rig.Pilot.Calls, c => c.RackId == "R-2" && c.Origin == A);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:126` | TheFieldPanelIsShownOnlyWithThePilotPort | Calls | `Assert.Empty(pilot.Calls);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:149` | TheEditButtonPinsThePreviewAndAsksForTheIntentionAndTheBoxBecomesEditable | Of | `Assert.Single(pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:172` | AClickOnTheReadOnlyBoxIsTheFirstGestureAndAClickOnAnEditableBoxIsNot | Of | `Assert.Single(pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:176` | AClickOnTheReadOnlyBoxIsTheFirstGestureAndAClickOnAnEditableBoxIsNot | Of | `Assert.Single(pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:198` | AKeyOnTheReadOnlyBoxIsTheFirstGestureItIsHandledAndTheBoxTextDoesNotChange | Of | `Assert.Single(pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:202` | AKeyOnTheReadOnlyBoxIsTheFirstGestureItIsHandledAndTheBoxTextDoesNotChange | Of | `Assert.Empty(pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:241` | TheDoubleClickOnTheButtonOfTheActiveTabIsAGesture | Of | `Assert.Single(pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:273` | TheDoubleClickOnATabThatIsNotActiveAfterTheClicksCallsNothing | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:291` | AKeyThatIsARepetitionOfAHeldKeyIsConsumedAndLaunchesNothing | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:296` | AKeyThatIsARepetitionOfAHeldKeyIsConsumedAndLaunchesNothing | Of | `Assert.Single(pilot.Of(BeginOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:313` | WithANotAdmittedDiagnosticOfTheSameSurfaceAClickAndAKeyAndTheDoubleClickDoNotReadAndOnlyTheEditButtonReadsAgain | Reads | `Assert.Single(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:322` | WithANotAdmittedDiagnosticOfTheSameSurfaceAClickAndAKeyAndTheDoubleClickDoNotReadAndOnlyTheEditButtonReadsAgain | Reads | `Assert.Single(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:329` | WithANotAdmittedDiagnosticOfTheSameSurfaceAClickAndAKeyAndTheDoubleClickDoNotReadAndOnlyTheEditButtonReadsAgain | Reads | `Assert.Equal(2, pilot.Reads().Count());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:352` | WithAnErrorOfTheFieldNoClickNorKeyNorDoubleClickReadsAndOnlyReloadActs | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:359` | WithAnErrorOfTheFieldNoClickNorKeyNorDoubleClickReadsAndOnlyReloadActs | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:380` | TheDoubleClickOnTheActiveTabWithAnEditableBoxDoesNothingAndKeepsTheNoticeOfTheSurface | Reads | `Assert.Empty(pilot.Reads());` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:398` | Enter_OnAnEditableBoxWithPendingTextCommitsAndIsHandledAndEscIsHandledWithoutChangingAnything | Calls | `Assert.DoesNotContain(pilot.Calls, c => c.Operation != GetFieldOp);` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:403` | Enter_OnAnEditableBoxWithPendingTextCommitsAndIsHandledAndEscIsHandledWithoutChangingAnything | Of | `Assert.Single(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:416` | LeavingTheBoxCommitsOnlyWithPendingText | Of | `Assert.Empty(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:420` | LeavingTheBoxCommitsOnlyWithPendingText | Of | `Assert.Single(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:456` | ACharacterKeyOnAnEditableBoxIsNotHandledInTheTunnelNorInTheBubbleSoItProducesItsTextInput | Of | `Assert.Empty(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:523` | A_SettingTheTextOfTheBoxWithoutLosingFocusCallsSetPendingTextWithTheExactText | Of | `var typed = Assert.Single(pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:528` | A_SettingTheTextOfTheBoxWithoutLosingFocusCallsSetPendingTextWithTheExactText | Of | `Assert.Empty(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:545` | B_ShowingAndPresentingWithAPendingTextNeverCallSetPendingText | Of | `Assert.Empty(pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:546` | B_ShowingAndPresentingWithAPendingTextNeverCallSetPendingText | Of | `Assert.Empty(pilot.Of(CommitOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:560` | C_ARejectedTextRestoresTheTextOfTheBox | Of | `Assert.Single(pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:575` | ATextTypedInAReadOnlyBoxStateNeverReachesTheDraft | Of | `Assert.Empty(pilot.Of(SetPendingOp));` |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:604` | TheReloadButtonOfTheErrorStateShowsTheDraftAgainWithoutAnyRead | Reads | `Assert.Empty(pilot.Reads());` |
