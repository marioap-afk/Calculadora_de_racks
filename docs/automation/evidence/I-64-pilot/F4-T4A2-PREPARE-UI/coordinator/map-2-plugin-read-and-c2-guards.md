# Map 2 — Plugin read path and the C2 guards (F4-T4A2 input)

- HEAD of the worktree: `b9e1ed5a` (working tree clean at the time of reading). Read-only research: no repo file was modified, nothing was built, no test was run.
- All paths are repo-relative. Line numbers are at `b9e1ed5a`.
- The only "execution" was an in-memory regex probe (a PowerShell copy of two guard functions, applied to mutated copies of the command file text held in memory; script `probe-guards.ps1` sits next to this map in the session working folder, not in the repo). Its results are in B.3.
- Both test projects reference only Domain, Application (and UI / tools); neither references the Plugin (`tests/RackCad.Tests/RackCad.Tests.csproj:23-31`, `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:25-27`). Plugin behavior can only be pinned by text guards.

---

## A. Product code of RACKPANELDESCARTAR and everything it calls

### A.1 The command file, in full — `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs` (81 lines)

```csharp
 1	using Autodesk.AutoCAD.EditorInput;
 2	using Autodesk.AutoCAD.Runtime;
 3	using RackCad.Application.Workspace;
 4	using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
 5
 6	namespace RackCad.Plugin.Workspace
 7	{
 8	    /// <summary>
 9	    /// The two commands of the workspace panel, with no flags and no alias.
10	    ///
11	    /// RACKPANEL (I-64 F1-T3A, T3B): it obtains the single workspace controller and opens the panel state through it (which starts the
12	    /// event bridge lazily and asks for the first reconciliation), then asks the single palette host to show the palette. It carries no
13	    /// rule of its own and no palette logic, and reads and writes nothing of the drawing.
14	    ///
15	    /// RACKPANELDESCARTAR (I-64 F4-T3C2): the explicit discard of the drafts of the panel of the active document. It never creates the
16	    /// controller: without one it only says so. Every decision and every text lives in Application
17	    /// (<see cref="WorkspaceDocumentDiscardCommand"/>); this adapter only supplies the delegates, the question on the command line and
18	    /// the one writer of the command line of the panel. It opens no dialog and no window and writes nothing of the drawing.
19	    /// </summary>
20	    public sealed class WorkspacePanelCommand
21	    {
22	        [CommandMethod("RACKPANEL")]
23	        public void RackPanel()
24	        {
25	            try
26	            {
27	                WorkspaceController.GetOrCreate().OpenPanel();
28	                WorkspacePaletteHost.GetOrCreate().Show();
29	                AcApplication.DocumentManager.MdiActiveDocument?.Editor.WriteMessage(
30	                    "\nRackCad: panel Workspace mostrado.");
31	            }
32	            catch (System.Exception ex)
33	            {
34	                RackCommandSupport.Report(ex);
35	            }
36	        }
37
38	        [CommandMethod("RACKPANELDESCARTAR")]
39	        public void RackPanelDiscard()
40	        {
41	            try
42	            {
43	                var controller = WorkspaceController.Existing;
44	                WorkspaceDocumentDiscardCommand.Run(
45	                    controller?.PilotEdit,
46	                    () => controller?.Port.GetSnapshot().ActiveSession,
47	                    session => controller.Port.Authorize(new WorkspaceRequest(session)) == RequestDecision.Accepted,
48	                    ConfirmOnCommandLine,
49	                    WriteToCommandLine);
50	            }
51	            catch (System.Exception ex)
52	            {
53	                RackCommandSupport.Report(ex);
54	            }
55	        }
56
57	        // The question of the command: yes or no on the command line, with No as the default. Enter, Escape and cancel all mean no.
58	        private static bool ConfirmOnCommandLine(string question)
59	        {
60	            var document = AcApplication.DocumentManager.MdiActiveDocument;
61	            if (document == null) return false;
62
63	            var options = new PromptKeywordOptions("\n" + question);
64	            options.Keywords.Add("Si");
65	            options.Keywords.Add("No");
66	            options.Keywords.Default = "No";
67	            options.AllowNone = true;
68	            var answer = document.Editor.GetKeywords(options);
69	            return answer.Status == PromptStatus.OK && string.Equals(answer.StringResult, "Si", System.StringComparison.Ordinal);
70	        }
71
72	        /// <summary>
73	        /// The one writer of the command line of the panel: the command and the controller use it, so no other file of the Plugin names the
74	        /// writing API. Without an active document it writes nothing.
75	        /// </summary>
76	        internal static void WriteToCommandLine(string text)
77	        {
78	            AcApplication.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\n" + text);
79	        }
80	    }
81	}
```

The five arguments of `Run` (positional, matched to `WorkspaceDocumentDiscardCommand.Run(port, activeSession, isTarget, confirm, write)`):

| # | Parameter | Plugin argument (line) |
|---|---|---|
| 1 | `IWorkspacePilotEditPort? port` | `controller?.PilotEdit` (45) |
| 2 | `Func<SessionId?> activeSession` | `() => controller?.Port.GetSnapshot().ActiveSession` (46) |
| 3 | `Func<SessionId, bool> isTarget` | `session => controller.Port.Authorize(new WorkspaceRequest(session)) == RequestDecision.Accepted` (47) |
| 4 | `Func<string, bool> confirm` | `ConfirmOnCommandLine` (48) |
| 5 | `Action<string> write` | `WriteToCommandLine` (49) |

Mapping of `ConfirmOnCommandLine` (58-70): `true` iff `answer.Status == PromptStatus.OK` AND `answer.StringResult` ordinal-equals `"Si"`. Every other status, every other text, and "no active document" (61) give `false`. Which concrete `PromptStatus` AutoCAD returns for Enter/Escape with `AllowNone = true` + `Default = "No"` is not decidable from source; the Core test comment defers it to Smoke-2 (`tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs:381-382`).

`controller.Port` at 47 has no null-conditional. It is only evaluated when `controller != null`: `Run` returns at `WorkspaceDocumentDiscardCommand.cs:73-77` when `port` (= `controller?.PilotEdit`) is null, before calling `activeSession` (79) or `isTarget` (87); `PilotEdit` never returns null (`WorkspaceController.cs:123-130` returns the readonly `_pilotEdit` built in the constructor at 51).

Side effect of argument 1: reading `controller.PilotEdit` executes `_ = Navigation;` (`WorkspaceController.cs:127`), which lazily creates the `WorkspaceNavigationHost` and assigns the edit-intent reader (`WorkspaceController.cs:92-110`) if they did not exist yet.

### A.2 The core of the command — `src/RackCad.Application/Workspace/WorkspaceDocumentDiscardCommand.cs` (228 lines; quoted from 24 to 227, the class body)

```csharp
 24	    public static class WorkspaceDocumentDiscardCommand
 25	    {
 26	        /// <summary>The question of the confirmation (the text of the keywords is up to the command).</summary>
 27	        public const string Question = "¿Descartar los borradores de este documento?";
 28
 29	        private const string NoController =
 30	            "RackCad: No hay borradores del panel Workspace en esta sesión de AutoCAD; no se descartó nada.";
 31
 32	        private const string NoActiveSession =
 33	            "RackCad: El panel Workspace no sigue ningún documento activo en este momento; no se descartó nada.";
 34
 35	        private const string UnknownRisk =
 36	            "RackCad: No se pudo decidir qué borradores están en riesgo; no se descartó nada.";
 37
 38	        private const string NothingAtRisk = "RackCad: No hay borradores en riesgo en este documento; no se descartó nada.";
 39
 40	        private const string NotConfirmed = "RackCad: No se descartó nada.";
 41
 42	        private const string ActiveChanged = "RackCad: El documento activo cambió; no se descartó nada.";
 43
 44	        private const string UnknownSecondRisk =
 45	            "RackCad: No se pudieron volver a comprobar los borradores en riesgo; no se descartó nada.";
 46
 47	        private const string RisksChanged =
 48	            "RackCad: Los borradores en riesgo cambiaron; no se descartó nada. Vuelve a ejecutar RACKPANELDESCARTAR para revisarlos.";
 49
 50	        private const string AmbiguousTarget =
 51	            "RackCad: El panel Workspace no sigue el documento activo en este momento; no se descartó nada.";
 52
 53	        private const string NotDiscarded = "RackCad: No se descartaron los borradores del documento: ";
 54
 61	        public static void Run(
 62	            IWorkspacePilotEditPort? port,
 63	            Func<SessionId?> activeSession,
 64	            Func<SessionId, bool> isTarget,
 65	            Func<string, bool> confirm,
 66	            Action<string> write)
 67	        {
 68	            if (activeSession == null) throw new ArgumentNullException(nameof(activeSession));
 69	            if (isTarget == null) throw new ArgumentNullException(nameof(isTarget));
 70	            if (confirm == null) throw new ArgumentNullException(nameof(confirm));
 71	            if (write == null) throw new ArgumentNullException(nameof(write));
 72
 73	            if (port == null)
 74	            {
 75	                write(NoController);
 76	                return;
 77	            }
 78
 79	            var found = activeSession();
 80	            if (!found.HasValue)
 81	            {
 82	                write(NoActiveSession);
 83	                return;
 84	            }
 85
 86	            var session = found.Value;
 87	            if (!isTarget(session))
 88	            {
 89	                write(AmbiguousTarget);
 90	                return;
 91	            }
 92
 93	            var first = ReadRisks(port, session);
 94	            if (!first.IsKnown)
 95	            {
 96	                write(UnknownRisk);
 97	                return;
 98	            }
 99
100	            if (first.RackIds.Count == 0 && first.UncertainRackIds.Count == 0)
101	            {
102	                write(NothingAtRisk);
103	                return;
104	            }
105
106	            if (first.RackIds.Count > 0)
107	                write("RackCad: Borradores con trabajo sin aplicar: " + string.Join(", ", first.RackIds) + ".");
108	            if (first.UncertainRackIds.Count > 0)
109	                write("RackCad: No se pudo decidir si tienen trabajo sin aplicar: " + string.Join(", ", first.UncertainRackIds) + ".");
110
111	            if (!confirm(Question))
112	            {
113	                write(NotConfirmed);
114	                return;
115	            }
116
117	            var again = activeSession();
118	            if (!again.HasValue || again.Value != session)
119	            {
120	                write(ActiveChanged);
121	                return;
122	            }
123
124	            if (!isTarget(session))
125	            {
126	                write(AmbiguousTarget);
127	                return;
128	            }
129
130	            var second = ReadRisks(port, session);
131	            if (!second.IsKnown)
132	            {
133	                write(UnknownSecondRisk);
134	                return;
135	            }
136
137	            if (!SameList(first.RackIds, second.RackIds) || !SameList(first.UncertainRackIds, second.UncertainRackIds))
138	            {
139	                write(RisksChanged);
140	                return;
141	            }
142
143	            EditDiscardOutcome outcome;
144	            try
145	            {
146	                outcome = port.DiscardDocument(new WorkspaceRequest(session));
147	            }
148	            catch (Exception ex)
149	            {
150	                write(NotDiscarded + "falló. " + ReasonOrDefault(ex.Message, "sin detalle."));
151	                return;
152	            }
153
154	            write(OutcomeText(outcome, first));
155	        }
156
157	        // A read that throws counts as the unknown, which the default value of the view stands for.
158	        private static PilotRiskView ReadRisks(IWorkspacePilotEditPort port, SessionId session)
159	        {
160	            try
161	            {
162	                return port.GetRacksAtRisk(new WorkspaceRequest(session));
163	            }
164	            catch (Exception)
165	            {
166	                return default;
167	            }
168	        }
169
170	        private static string OutcomeText(EditDiscardOutcome outcome, PilotRiskView risks)
171	        {
172	            switch (outcome.Status)
173	            {
174	                case EditDiscardStatus.Discarded:
175	                    return DiscardedText(outcome, risks);
176	                case EditDiscardStatus.Busy:
177	                    return NotDiscarded + ReasonOrDefault(outcome.Reason, "hay una lectura en curso.");
178	                case EditDiscardStatus.Failed:
179	                    return NotDiscarded + "falló. " + ReasonOrDefault(outcome.Reason, "sin detalle.");
180	                default:
181	                    return NotDiscarded + "se rechazó. " + ReasonOrDefault(outcome.Reason, "sin detalle.");
182	            }
183	        }
184
185	        private static string DiscardedText(EditDiscardOutcome outcome, PilotRiskView risks)
186	        {
187	            var text = "RackCad: ";
188	            text = text + (risks.RackIds.Count > 0
189	                ? "Se descartaron los borradores en riesgo: " + string.Join(", ", risks.RackIds) + "."
190	                : "Se descartaron los borradores del documento.");
191	            if (risks.UncertainRackIds.Count > 0)
192	                text = text + " Sin decidir, también descartados: " + string.Join(", ", risks.UncertainRackIds) + ".";
193
194	            var clean = 0;
195	            foreach (var removed in outcome.RemovedRackIds)
196	            {
197	                if (!Contains(risks.RackIds, removed) && !Contains(risks.UncertainRackIds, removed)) clean++;
198	            }
199
200	            return text + " Borradores limpios retirados además: " + clean.ToString(CultureInfo.InvariantCulture) + ".";
201	        }
202
203	        private static string ReasonOrDefault(string? reason, string fallback) =>
204	            string.IsNullOrWhiteSpace(reason) ? fallback : reason!;
205
206	        private static bool Contains(IReadOnlyList<string> list, string rackId) { /* ordinal loop, 206-214 */ }
217	        private static bool SameList(IReadOnlyList<string> first, IReadOnlyList<string> second) { /* count + ordinal per index, 217-226 */ }
227	    }
```
(Lines 55-60 are the XML doc of `Run`; 206-226 are two ordinal list helpers, elided above only in their loop bodies.)

`isTarget` is called twice (87 before the question, 124 after the yes and after the active-session recheck at 117-122). `session` — the value returned by delegate 2 — is the only `SessionId` passed to `GetRacksAtRisk` (162) and `DiscardDocument` (146).

### A.3 The authorization chain called by delegate 3

| Step | Location | Code |
|---|---|---|
| Controller lookup, never creates | `src/RackCad.Plugin/Workspace/WorkspaceController.cs:83` | `public static WorkspaceController Existing => Holder.Existing;` |
| Port | `WorkspaceController.cs:86` | `public IWorkspaceHostPort Port => _port;` (`_port = new WorkspaceHostPort(_host)` at 48) |
| Port interface | `src/RackCad.Application/Workspace/IWorkspaceHostPort.cs:15-16` | `RequestDecision Authorize(WorkspaceRequest request);` ("No side effects.") |
| Plugin port | `src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs:21` | `public RequestDecision Authorize(WorkspaceRequest request) => _core.Authorize(request);` |
| Host core | `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:246-247` | `public RequestDecision Authorize(WorkspaceRequest request) => _bridge.Authorize(request, _probe.ActiveDocumentNativeId ?? NoDocumentNativeId);` |
| No-document id | `WorkspaceHostCore.cs:51` | `private const long NoDocumentNativeId = -1;` ("it matches no session") |
| Probe (the "document") | `WorkspaceController.cs:356-363` | `var document = AcApplication.DocumentManager.MdiActiveDocument; return document == null ? (long?)null : document.UnmanagedObject.ToInt64();` |
| Bridge link | `WorkspaceController.cs:340-341` | `public RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId) => _bridge.Authorize(request, executingNativeInstanceId);` |
| Bridge facade | `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs:105-106` | `=> _core.Authorize(request, executingNativeInstanceId);` |
| Bridge core | `src/RackCad.Application/Workspace/EventBridgeCore.cs:115-116` | `=> _registry.Authorize(request, executingNativeInstanceId);` |
| Registry (the decision) | `src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs:39-46` | see below |

```csharp
39	        public RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId)
40	        {
41	            if (!TryGet(request.Origin, out _)) return RequestDecision.RejectedSessionDestroyed;
42	            var executing = _live.Find(x => x.NativeInstanceId == executingNativeInstanceId);
43	            return executing != null && executing.Id == request.Origin
44	                ? RequestDecision.Accepted
45	                : RequestDecision.RejectedSessionMismatch;
46	        }
```

Types — `src/RackCad.Application/Workspace/SessionId.cs`:
```csharp
 8	    public readonly record struct SessionId(long Value);
14	    public enum RequestDecision
15	    {
16	        Accepted,
17	        RejectedSessionMismatch,
18	        RejectedSessionDestroyed,
19	    }
22	    public readonly record struct WorkspaceRequest(SessionId Origin);
```
`Accepted` is the enum's first member (value 0, so `default(RequestDecision) == Accepted`).

### A.4 Where the "session" of delegate 2 comes from

`WorkspaceHostCore.GetSnapshot()` — `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:225-243`:
```csharp
225	        public WorkspaceHostSnapshot GetSnapshot()
226	        {
227	            var active = _started ? _bridge.ActiveSession : null;
228	            WorkspaceSession? session = null;
229	            var hasDocument = active.HasValue && _bridge.TryGetSession(active.Value, out session) && session != null;
230	            var id = hasDocument ? active!.Value : default;
...
235	            return new WorkspaceHostSnapshot(
236	                WorkspaceHostPolicy.Status(conditions),
237	                hasDocument ? id : null,
...
```
(`WorkspaceHostSnapshot.ActiveSession` is the second positional member, `src/RackCad.Application/Workspace/WorkspaceHostSnapshot.cs:35-42`; `_bridge.ActiveSession` is `EventBridgeCore.ActiveSession => _active`, `EventBridgeCore.cs:76`.) It is null when the panel was never opened (`_started` false) or the bridge's active session is not live.

### A.5 What "session" and "document" are authorized, and how

- **Session** = the `SessionId` of the bridge's active session (A.4), read through `controller.Port.GetSnapshot().ActiveSession`. It is the identity issued by `WorkspaceSessionRegistry.GetOrCreate(nativeInstanceId)` (`WorkspaceSessionRegistry.cs:14-21`), monotonically and never reused.
- **Document** = the executing document, i.e. `AcApplication.DocumentManager.MdiActiveDocument` at the moment of the call, identified by `UnmanagedObject.ToInt64()` (`WorkspaceController.cs:356-363`); `-1` when there is none (`WorkspaceHostCore.cs:51, 247`). The command file has no way to pass a document: `IWorkspaceHostPort.Authorize` takes only a `WorkspaceRequest`.
- **Rule** (`WorkspaceSessionRegistry.cs:39-46`): `Accepted` iff the session is still live AND the live session registered for the executing native id is that same session. Not live → `RejectedSessionDestroyed`; live but another (or no) document → `RejectedSessionMismatch`.
- **Mapping in the Plugin** (line 47): `isTarget(session) == true` iff the decision is `Accepted`; both rejections → `false` → `AmbiguousTarget` text, nothing read or discarded (A.2: 87-91 / 124-128).
- **Second authorization (host):** `DiscardDocument` re-authorizes the same origin — `WorkspaceHostCore.cs:1036-1052` (`if (Authorize(origin) != RequestDecision.Accepted) return EditDiscardOutcome.Rejected(NotThisDocumentReason);` at 1040; `_pilotReadInProgress` → `Busy` at 1041), then `DiscardPilotEdits` authorizes again (`WorkspaceHostCore.cs:770`). `GetRacksAtRisk` does **not** authorize ("Memory only: no authorization", `WorkspaceHostCore.cs:1054-1097`). The gate of notices passes both through unchanged (`src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:197, 199`) and so does the controller adapter (`WorkspaceController.cs:311, 313`).

---

## B. Guards on the command file, their self-tests, exact counts, and the Core tests

`Src.Code(path)` = file text with `/* */` and `//` comments removed; **string literals are kept** (`tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs:29-39`). `Src.BodyOf(code, signature)` returns from the signature to the matching brace (or to the first `;` after `=>`) and **asserts** if the signature is missing (`WorkspaceHostSourceSupport.cs:67-89`), so every injected self-test text must contain the signature.

### B.1 `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorGuardTests.cs` (the C2 guards)

Constants: `CommandPath = WorkspaceBridgeSourceGuardTests.CommandAdapterPath` (19); `RackPanelDiscardSignature = @"public\s+void\s+RackPanelDiscard\s*\(\s*\)"` (29); `ConfirmSignature = @"private\s+static\s+bool\s+ConfirmOnCommandLine\s*\(\s*string\s+\w+\s*\)"` (30).

**(a) C2-M-2 target — body of `RackPanelDiscard`, lines 99-129**
```csharp
101	    private static IEnumerable<string> DiscardBodyViolations(string code)
102	    {
103	        var body = Src.BodyOf(code, RackPanelDiscardSignature);
104	        if (!Regex.IsMatch(body, @"WorkspaceController\.Existing\b")) yield return "it does not take WorkspaceController.Existing";
105	        if (!Regex.IsMatch(body, @"WorkspaceDocumentDiscardCommand\.Run\s*\(")) yield return "it does not call the core of the command";
106	        if (!Regex.IsMatch(body, @"\.Authorize\s*\(")) yield return "it does not authorize the target against the document";
107	        if (!Regex.IsMatch(body, @"catch\s*\(\s*System\.Exception\s+ex\s*\)\s*\{\s*RackCommandSupport\.Report\s*\(\s*ex\s*\)\s*;\s*\}"))
108	            yield return "it does not report through the shared reporter";
109	        var forbidden = Regex.Match(body, @"\b(?:GetOrCreate|OpenPanel|Show|IsCreated)\b");
110	        if (forbidden.Success) yield return "it names " + forbidden.Value;
111	    }
```
- Real-file test: `RackPanelDiscardUsesExistingAndNeverCreatesNorOpensNorShowsAnything` (113-117) → `Assert.Empty(DiscardBodyViolations(Src.Code(CommandPath)))`.
- Self-tests: `TheDiscardBodyGuardFlagsEachWayOfCreatingOpeningShowingOrSkippingTheTarget` (119-129), six synthetic `InlineData` strings (120-125): no `Existing`; `GetOrCreate`; `OpenPanel`; `WorkspacePaletteHost.GetOrCreate().Show()`; `s => true` (no `.Authorize(`); no catch/report. All use `s => c.Port.Authorize(s)` (no `== RequestDecision.Accepted`, no `new WorkspaceRequest(`), i.e. the current guard does not look at the comparison, the decision, the request, the session variable, nor at arguments 1-2 of `Run`.

**(b) C2-M-1 target — `GetKeywords` / `ConfirmOnCommandLine`, lines 131-164**
```csharp
133	    private static IEnumerable<string> KeywordsViolations(string code)
134	    {
135	        if (Src.Count(code, @"\bGetKeywords\b") != 1) yield return "GetKeywords is not named exactly once";
136	        var body = Src.BodyOf(code, ConfirmSignature);
137	        if (Src.Count(body, @"\.GetKeywords\s*\(") != 1) yield return "the one GetKeywords is not inside ConfirmOnCommandLine";
138	        foreach (var needle in new[]
139	                 {
140	                     @"Keywords\.Add\s*\(\s*""Si""\s*\)", @"Keywords\.Add\s*\(\s*""No""\s*\)",
141	                     @"Keywords\.Default\s*=\s*""No""\s*;", @"AllowNone\s*=\s*true\s*;",
142	                     @"\.Status\b", @"\.StringResult\b", @"StringComparison\.Ordinal\b", @"PromptStatus\.OK\b",
143	                 })
144	            if (!Regex.IsMatch(body, needle)) yield return "ConfirmOnCommandLine lacks " + needle;
145	        // The answer of GetKeywords is read with Status and StringResult only: nothing is named Result (a property of other types is banned).
146	        if (Regex.IsMatch(code, @"\bResult\b")) yield return "an identifier is named Result";
147	    }
```
- Real-file test: `TheCommandFileAsksYesOrNoOnlyInConfirmOnCommandLineWithNoAsTheDefault` (149-153).
- Self-tests: `TheKeywordsGuardFlagsAnotherPlaceAnotherDefaultNoAllowNoneOrAnIdentifierCalledResult` (155-164), five synthetic strings (156-160): GetKeywords in another method; no `Default`; `Default = "Si"`; a variable named `Result`; `return a.Status == PromptStatus.OK;` (no StringResult/Ordinal). The needles are **presence-only**: nothing ties `PromptStatus.OK` to `==`, nothing ties the compared literal to `"Si"`, nothing fixes `&&`, a negation, or the `return` expression.

**(c) Other guards of the file that touch the command file** (keep in mind when hardening):
- Pair of commands (36-71): exactly `RACKPANEL`, `RACKPANELDESCARTAR`; `Src.Count(code, @"\[\s*CommandMethod\b") != 2` (43); no `CommandFlags|\[assembly|CommandClass|LispFunction|IExtensionApplication` (44). Self-tests 54-64, 66-71.
- GetOrCreate (75-97): `WorkspaceController\.GetOrCreate\s*\(` exactly once in the file and inside `RackPanel` (78-80). Self-tests 89-97.
- GetKeywords exception by exact path (166-203): `HandlerForbiddenCallsFor(CommandPath)` allows only `GetKeywords` (169-181); every other file keeps it forbidden (183-196); nothing else in `src/RackCad.Plugin/Workspace/` names `GetKeywords` (198-203).
- Boundary (207-231): whole command file must not match `\bEnterPanelModal\b|\bMessageBox\b|\bShowDialog\b|\bShowModalDialog\b|\bWindow\b|\bDispatcher\b|\bTimer\b|\bTask\b|\basync\b|\bawait\b|\bFocus\b|\bActivate\s*\(|\bSetFocus\b|\bBringToFront\b` (211).
- Ported-call guard on the Application core (412-441): in `WorkspaceDocumentDiscardCommand.cs`, `port.` calls are exactly `{GetRacksAtRisk, DiscardDocument}` (428) and none of `DiscardRack|GetField|SetPendingText|CommitField|CrossBoundary|TryBeginEdit|TryReload|TryCheckNow|TakeLossNotices|TakeCloseNotices` (421).
- No test reads `WorkspaceCommandAdvisorGuardTests.cs` itself as text (search for the file name in `tests/` finds only its class declaration).

### B.2 Other guards that read `WorkspacePanelCommand.cs` (exact counts a hardened guard must keep)

| File:lines | What it asserts on the command file |
|---|---|
| `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs:36-47` | `[CommandMethod("RACKPANEL")]` registered once in all of `src`, only here |
| `WorkspaceHostSourceGuardTests.cs:49-62` | names exactly `{RACKPANEL, RACKPANELDESCARTAR}`; `Assert.Equal(2, Src.Count(code, @"\[\s*CommandMethod\b"))`; no flags/alias |
| `WorkspaceHostSourceGuardTests.cs:64-69` | no other Plugin/Workspace file declares a command |
| `WorkspaceHostSourceGuardTests.cs:71-79` | `Assert.Contains("WorkspaceController.GetOrCreate()", code)`; `\.OpenPanel\s*\(\s*\)`; DoesNotMatch `\bnew\s+WorkspaceController\b|PaletteSet|AddVisual|System\.Windows|AutoCadEventBridge|StartTransaction|LockDocument|SelectImplied|GetObject` |
| `WorkspaceHostSourceGuardTests.cs:81-88` | Contains `catch (System.Exception` and `RackCommandSupport.Report(` |
| `WorkspaceHostSourceGuardTests.cs:90-95` | only the command file names `WriteMessage` in Plugin/Workspace |
| `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs:136-146` | only the command file and the palette host name `WorkspacePaletteHost` |
| `WorkspacePaletteHostSourceGuardTests.cs:299-309` | `WorkspaceController\.GetOrCreate\(\)\s*\.OpenPanel\s*\(\s*\)` before `WorkspacePaletteHost\.GetOrCreate\(\)\s*\.Show\s*\(\s*\)` |
| `WorkspacePaletteHostSourceGuardTests.cs:311-318` | no `PaletteSet|AddVisual|System\.Windows|RackCad\.UI|Autodesk\.AutoCAD\.Windows|UserControl|Dispatcher`; no "no muestra / aun no / not shown" |
| `WorkspacePaletteHostSourceGuardTests.cs:320-336` | pair names; `Assert.Equal(2, ...CommandMethod...)`; `Assert.Equal(1, ...\[CommandMethod\(""RACKPANEL""\)\]...)`; Contains `catch (System.Exception ex)` and `RackCommandSupport.Report(ex)` |
| `WorkspacePaletteHostSourceGuardTests.cs:757-783` | Plugin/Workspace holds **exactly** nine files (the list includes `WorkspacePanelCommand.cs`) — no new product file can be added there |
| `tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs:139-168` | `ForbiddenCallNames` (139-142); command file uses `CommandForbiddenCalls = ForbiddenCallsWithout("GetKeywords")` (161, 167) |
| `WorkspaceBridgeSourceGuardTests.cs:346-359` | `DeclaresEntryPoint`: in the command file `allowed == 1`, `discard <= 1`, `attributes == allowed + discard` |
| `tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs:21-57, 129-140` | self-tests of `DeclaresEntryPoint` for the pair; real files pass |
| `WorkspaceSourceGuardExceptionTests.cs:273-282` | `Assert.Equal(3, Src.Count(guard, @"string\.Equals\(path, (?:Reader|Command)AdapterPath, StringComparison\.Ordinal\)"))` over the text of `WorkspaceBridgeSourceGuardTests.cs` (and 2 for Facts/Effects at 256; no `.StartsWith/EndsWith/Contains(…Adapter` at 257; no `Skip` at 258, 278) — constrains any edit of `WorkspaceBridgeSourceGuardTests.cs` |
| `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:609-622` | command file must not match `WorkspaceNavigation|NavigationEcho|IWorkspaceNavigationPort` |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:963-988` | lower bound by name of Plugin/Workspace files (includes `WorkspacePanelCommand.cs`) |
| `tests/RackCad.Tests/SelectiveEditorOpenTests.cs:564-572` | **raw** count of `"[CommandMethod("` over all `src/RackCad.Plugin/**/*.cs` (comments included) `== 40` |
| `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs:57-64` | census includes `F4PanelDiscardCommands = { "RACKPANELDESCARTAR" }` |

Constraints that follow directly from these (text facts): the command file must keep `\bGetKeywords\b` exactly once and inside `ConfirmOnCommandLine`; no identifier matching `\bResult\b` anywhere in it (`StringResult` does not match); exactly two `[CommandMethod`; `GetOrCreate(` only once and in `RackPanel`; `RackPanelDiscard` body must not name `GetOrCreate|OpenPanel|Show|IsCreated`.

### B.3 In-memory probe: mutants that the current guards accept

Method: the real file text, comment-stripped like `Src.Code`, with ONE `Replace` per mutant, evaluated with PowerShell copies of `KeywordsViolations` and `DiscardBodyViolations` (same regexes as B.1). Result `[]` = no violation = **the mutant passes the current guard**. The real file gives `[]` for both, as expected.

| Mutant (replaces line 69 or 47 or arg 1/2) | Violations |
|---|---|
| M1a `return !(answer.Status == PromptStatus.OK && string.Equals(answer.StringResult, "Si", …));` | `[]` |
| M1b `return answer.Status != PromptStatus.OK \|\| !string.Equals(answer.StringResult, "Si", …);` | `[]` |
| M1c compare to `"No"` instead of `"Si"` | `[]` |
| M1d `\|\|` instead of `&&` | `[]` |
| M1e `answer.Status != PromptStatus.OK && …"Si"…` | `[]` |
| M1f `… && !string.Equals(answer.StringResult, "Si", …)` | `[]` |
| M2a `… Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted` | `[]` |
| M2b `… == RequestDecision.RejectedSessionMismatch` | `[]` |
| M2c `session => { controller.Port.Authorize(new WorkspaceRequest(session)); return true; }` (ignored) | `[]` |
| M2d `Authorize(new WorkspaceRequest(default)) == …Accepted` (wrong session) | `[]` |
| M2e `Authorize(new WorkspaceRequest(new SessionId(1))) == …Accepted` (wrong session) | `[]` |
| M2f `… == RequestDecision.Accepted \|\| true` (unauthorized accepted) | `[]` |
| M3a arg 1 `controller?.PilotEdit` → `null` | `[]` |
| M3b arg 2 → `() => null` | `[]` |
| M3c arg 2 → `() => new SessionId(1)` | `[]` |

No other guard listed in B.2 inspects `Authorize`, `RequestDecision`, `PromptStatus`, `"Si"` or the `return` of `ConfirmOnCommandLine`; searches of `tests/` for `Port.Authorize`, `RequestDecision.Accepted`, `ConfirmOnCommandLine`, `WriteToCommandLine` outside `WorkspaceCommandAdvisorGuardTests.cs` find only the Core host guard at `WorkspaceLifecycleGuardTests.cs:634` (below) and none on the command file. Both C2-M-1 and C2-M-2 reproduce.

Observation on the existing self-tests: they assert `Assert.NotEmpty(...)` over a `yield` sequence, so any one rule firing satisfies them; synthetic strings can be flagged by a rule other than the targeted one (e.g. the mutant at 120 lacks `WorkspaceController.Existing`, and every one at 120-125 lacks the real argument shapes). Precedents in the repo for self-tests derived from the **real file by a single `Replace`** (preceded by `Assert.Contains(from, real)`): `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:538-555` and `:463-478`; good-text + Replace: `WorkspaceCommandAdvisorGuardTests.cs:275-297`.

Precedent for pinning an authorization comparison literally (Core host, not the command): `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:628-640`, in particular 634:
```csharp
Assert.Matches(@"if\s*\(\s*Authorize\s*\(\s*new\s+WorkspaceRequest\s*\(\s*session\s*\)\s*\)\s*!=\s*RequestDecision\.Accepted\s*\)\s*return\s+EditDiscardOutcome\.Rejected\s*\(", body);
```
(and at 633 `AssertInOrder(body, "Authorize(new WorkspaceRequest(session))", ...)`). Other related pins: `WorkspaceEditIntentGuardTests.cs:504` pins the controller's `(request, nativeId) => _bridge.Authorize(request, nativeId)`; `WorkspaceBridgeFacadeSourceGuardTests.cs:72, 171` pin the facade forwards of `Authorize`; `WorkspaceHostSourceGuardTests.cs:185` requires `_bridge\.Authorize\s*\(` in the controller; `WorkspacePilotEditGuardTests.cs:263-269` requires `\bAuthorize\s*\(` in each mutating host operation (incl. `DiscardDocument`). Not found: any guard on the body of `HostProbe.ActiveDocumentNativeId` (`WorkspaceController.cs:356-363`); `WorkspaceHostC2SourceGuardTests.cs:153-163` pins only `IsActiveDocumentQuiescent` of the probe.

### B.4 Core behavior tests (Application) relevant to authorization

`tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs` (pure core of the command, port double `CommandAdvisorPortDouble` at 17-82 records every call with its session):
- Rig: `CommandRig` 157-218. `isTarget` is a **stub**: `session => { Targets.Add(session); return Authorizes; }` (196-200). No test composes `Run` with a real `Authorize` or with `RequestDecision` (all `Run(` calls in tests: 189, 614, 616, 630-633).
- 220-233 no controller → C01, nothing asked; 235-247 no active session → C02; **249-263 `AnAmbiguousTargetSaysSoWithoutReadingTheRiskWithoutAskingAndWithoutDiscarding`** (`Authorizes = false` → C15, `Targets == [A]`, no port call); 378-395 anything but yes discards nothing; 397-409 nothing discarded before the yes; **411-425** active session changes to B during the question → C08, B never asked, no call with B; 427-438 active session disappears → C08; **440-453 `WhenTheSessionStopsAuthorizingAfterTheYes…`** → C15, `Targets == [A, A]`, one risk read, no discard; **455-466 `TheTargetIsOnlyAskedAboutTheSessionThatTheActiveDelegateJustGave`** → `Targets == [A, A]`, `ActiveReads == 2`, all port calls with A; 539-571 every non-discarded outcome text (incl. `Rejected|…` → "se rechazó."); 573-583 default outcome = rejection; 585-623 delegates not contained; 625-640 null delegates refused.

Host-level tests of the authorization used by the discard:
- `tests/RackCad.Tests/Workspace/WorkspaceHostAuthorizationTests.cs:25-80`: accepted for the active document's session (25-31), `RejectedSessionMismatch` for another document's session (33-39), `RejectedSessionDestroyed` for a destroyed (41-50) or never-existing session (52-58), a recycled native id never revives the old session (60-73), no document open → nothing accepted (75-…).
- `tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs:1376-1399` `DiscardPilotEditsIsRejectedWithoutEffectsWhenAnotherSessionIsActiveWhenThereIsNoActiveDocumentOrTheSessionIsDestroyed`; 1346-1360 works without rest/with modal/without reader; 1362-1374 empty lists.
- `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs:479-514` `TheOperationsOfADocumentNeverTouchTheDraftsOfAnotherAndABlockingMutationOfAnotherDocumentIsRejected` (`DiscardDocument(env.Origin(b))` with A active → `Rejected`, drafts intact, 496/503).
- Registry/bridge: `tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs:72, 87`; `tests/RackCad.Tests/Workspace/WorkspaceBridgeReconciliationTests.cs:311-333`.

---

## C. Plugin read path of the pilot (reader adapter, `CaptureDrawing`)

### C.1 Who reaches the reader at HEAD

- Six Application operations call `_editIntentReader!.ReadIntent(...)` once each, only in `WorkspaceHostCore`: `TryBeginPilotEdit` 313, `TryCheckPilotEdit` 394, `TryReloadPilotEdit` 472, `TryConfirmPilotBase` 556, `TryKeepPilotEdit` 635, `TryDiscardAndReloadPilotEdit` 716 (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs`). Pinned: `.ReadIntent(` appears only in the core (`WorkspaceEditIntentGuardTests.cs:621-623`), 6 times (`:726`).
- Port entries (explicit interface, with the `_pilotReadInProgress` try/finally mark): `TryCheckNow` 813-825, `TryReload` 827-839, `TryConfirmBase` 843-855, `TryKeepMine` 857-869, `TryDiscardAndReload` 871-883 (and `TryBeginEdit` 799-811). A second read while the mark is set → `Waiting("Hay una lectura del piloto en curso.")` (44, 801/815/829/845/859/873).
- UI at HEAD: the only port read calls are `_port.TryReload(request)` / `_port.TryBeginEdit(request)` at `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:203` (lazy reload when `Clean && IsPossiblyStale`, 201). **No text "Comprobar" and no call to `TryCheckNow`, `TryConfirmBase`, `TryKeepMine` or `TryDiscardAndReload` exists in `src/RackCad.UI`** (search: no match). In all of `src`, those four are named only by the controller adapter, the gate and the host core.

### C.2 Checks made in Application before the reader is called (nothing of the drawing is read here)

1. Gate of notices — `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:167-183`: inside a panel notice each of the six reads answers `Waiting(InsideNoticeReason)` (`"Se está entregando un aviso del panel."`, 156) without delegating.
2. `_pilotReadInProgress` mark (C.1).
3. Request errors: blank RackId; witness without key.
4. `PassGate` — `WorkspaceHostCore.cs:1337-1371`, in this order: no reader → `Unavailable`; `!_probe.ActiveDocumentNativeId.HasValue` → `Waiting("No hay documento activo.")`; `_modalRackCadActive` → `Waiting("Hay una ventana de RackCad abierta.")` (**the only modal check of the path**); `!_probe.IsActiveDocumentQuiescent` → `Waiting("Hay un comando en curso.")`; `Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted` → `Rejected("La petición no es del documento activo.")`. The probe's quiescence is `document != null && document.Editor.IsQuiescent && !_bridge.IsCommandActiveInActiveSession` (`WorkspaceController.cs:367-374`).
5. Draft applicability (missing draft → `NotFound`, wrong status → `NotApplicable`/`NotNeeded`/`Waiting`) — all before the read.

### C.3 The Plugin reader — `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:196-254`

Created once in the host constructor: `_editIntentReader = new PilotIntentReader(_targets, _authorize);` (57); `_authorize` is the controller's `(request, nativeId) => _bridge.Authorize(request, nativeId)` (`WorkspaceController.cs:98-101`); assigned to the core by `_host.SetEditIntentReader(_navigation.EditIntentReader)` (`WorkspaceController.cs:105`).

```csharp
196	        private sealed class PilotIntentReader : IWorkspaceEditIntentReader
197	        {
198	            private readonly NavigationTargets _targets;
199	            private readonly Func<WorkspaceRequest, long, RequestDecision> _authorize;
...
207	            public EditIntentReadResult ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness)
208	            {
209	                try
210	                {
211	                    return ReadOnActiveDocument(session, rackId, chosenReference, witness);
212	                }
213	                catch (System.Exception ex)
214	                {
215	                    LogFault("WorkspaceNavigationHost.ReadEditIntent", ex);
216	                    return EditIntentReadResult.Failed(ex.Message);
217	                }
218	            }
219
220	            private EditIntentReadResult ReadOnActiveDocument(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness)
221	            {
222	                var document = AcApplication.DocumentManager.MdiActiveDocument;
223	                if (document == null)
224	                    return EditIntentReadResult.NoActiveDocument("No hay documento activo.");
225	                if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent)
226	                    return EditIntentReadResult.Busy("Hay un comando en curso.");
227
228	                var catalog = RackPilotIntentAdapter.LoadCatalog();
229	                if (!catalog.Succeeded)
230	                    return EditIntentReadResult.Failed(catalog.FailureReason);
231
232	                using (document.LockDocument())
233	                {
234	                    var decision = _authorize(new WorkspaceRequest(session), document.UnmanagedObject.ToInt64());
235	                    if (decision != RequestDecision.Accepted)
236	                        return EditIntentReadResult.Rejected("La petición no es del documento activo.");
237
238	                    var expected = chosenReference.HasValue ? new[] { chosenReference.Value } : Array.Empty<long>();
239	                    var revalidation = _targets.Revalidate(session, rackId, expected);
240	                    var origin = WorkspaceEditIntent.ChooseOriginDefinition(revalidation.Fresh, chosenReference);
241
242	                    var read = RackPilotIntentAdapter.Read(
243	                        document,
244	                        rackId,
245	                        origin,
246	                        witness?.Key,
247	                        witness?.Text,
248	                        catalog.Catalog);
249	                    return read.Succeeded
250	                        ? EditIntentReadResult.FromPilot(read.Pilot)
251	                        : EditIntentReadResult.Failed(read.FailureReason);
252	                }
253	            }
254	        }
```

### C.4 What runs under that lock (two physical traversals, each in its own `using` transaction)

1. `_targets.Revalidate(...)` (`src/RackCad.Application/Workspace/NavigationTargets.cs:108-129`) → `Compute` (139) → `_port.ReadDefinitionFacts(session)` (141) = `WorkspaceNavigationFacts.Scan` (`src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs:26, 48-93`):
   - returns empty facts unless `MdiActiveDocument` is the session's document (52-54);
   - `using (document.LockDocument())` (56, nested inside the reader's lock) and `using (var transaction = document.Database.TransactionManager.StartTransaction())` (57); **no `Commit`** (the transaction is disposed by the `using`);
   - reads via `RackBlockFinder.ScanEnvelopes(transaction, …)` (61; `src/RackCad.Plugin/RackBlockFinder.cs:57-91`, `GetObject(…, OpenMode.ForRead)` at 62, 65, `RackBlockData.Read` at 71) and `transaction.GetObject(…, OpenMode.ForRead)` (64, 70).
2. `RackPilotIntentAdapter.Read(...)` (`src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:94-142`) — takes no lock and opens no transaction **in its own body** (comment 66; guard `WorkspaceEditIntentGuardTests.cs:199`); `AssertNoOpenTransaction(document, "before")` (104, Debug-only, `[Conditional("DEBUG")]` 195-201); witness key check (109-114); `RackSiblingScan.CaptureDrawing(document)` (116); `scan.RawEnvelopes == null` → typed failure (117-118); pure read `PushBackPilotIntentReader.Read(input, catalog)` (120-121); catches: `LockViolation` (125-128), other AutoCAD exception (129-132), any exception (133-136); `finally { AssertNoOpenTransaction(document, "after"); }` (137-141).
   - `CaptureDrawing` = `Traverse(document, ObjectId.Null, null)` (`src/RackCad.Plugin/Views/RackSiblingScan.cs:95-96`). `Traverse` (98-170): `using (document.LockDocument())` (112, nested again) and `using (var transaction = document.Database.TransactionManager.StartTransaction())` (113); every `GetObject` is `OpenMode.ForRead` (115, 118, 131, 132, 138; `AddLayer` 176); `RackBlockData.Read(transaction, definitionId)` (121; `src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:54-89`, `ForRead` at 61, 67, 73); `eraseByKind` is null here, so the erase flag of every fact is false (152); **`transaction.Commit();` at 163** — the commit of a transaction that opened objects only for read; the `using` then disposes it.
- The catalog (`RackPilotIntentAdapter.LoadCatalog()`, 74-87, wraps `LateralHeaderDrawService.LoadCatalog()`) runs **outside** the lock (reader line 228).

Summary table (reader path):

| Aspect | Fact |
|---|---|
| No active document | `NoActiveDocument("No hay documento activo.")` (224) |
| Command / not quiescent | `!string.IsNullOrEmpty(document.CommandInProgress) \|\| !document.Editor.IsQuiescent` → `Busy("Hay un comando en curso.")` (225-226) |
| Modal | not checked in the Plugin reader; checked before in `PassGate` (`WorkspaceHostCore.cs:1352-1356`) |
| DocumentLock | `using (document.LockDocument())` at reader 232 (around authorize, revalidate, origin choice, adapter read); nested `using` locks at `WorkspaceNavigationFacts.cs:56` and `RackSiblingScan.cs:112` |
| Transactions | two, sequential, each `using`: `WorkspaceNavigationFacts.cs:57` (never committed) and `RackSiblingScan.cs:113` (committed at 163) |
| DBObjects | opened only via `transaction.GetObject(..., OpenMode.ForRead)`; no explicit `Dispose` of any DBObject in these files (their lifetime is the transaction's, per the AutoCAD API); the `ResultBuffer` of `xrecord.Data` (`RackBlockData.cs:74, 80`) is enumerated and not disposed explicitly |
| Writes to the drawing | none found: no `ForWrite/ForNotify`, `UpgradeOpen`, `AppendEntity`, `Erase`, `RackBlockData.Write`, selection or view writes in the reader body, the adapter, the facts adapter or the scan; the only `Commit` is the read-only one at `RackSiblingScan.cs:163` |
| Focus / selection / view | none in the reader (no `_effects`, `_runner`, `ReturnFocusToDrawing`) |
| Debug check | `AssertNoOpenTransaction` before/after in `RackPilotIntentAdapter.Read` (Debug only) |

### C.5 Statuses returned by the Plugin reader and their mapping in Application

Reader statuses (`EditIntentReadStatus`, `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:21-28`: `Read, NoActiveDocument, Busy, Rejected, Failed`; factories 50-62):

| Reader result | Where | Reason text |
|---|---|---|
| `NoActiveDocument` | reader 224 | "No hay documento activo." |
| `Busy` | reader 226 | "Hay un comando en curso." |
| `Failed` | reader 230 (catalog) | "El catálogo no está disponible." / "No se pudo cargar el catálogo: " + msg (`RackPilotIntentAdapter.cs:80, 85`) |
| `Rejected` | reader 236 | "La petición no es del documento activo." |
| `Read` (`FromPilot`) | reader 250 | — |
| `Failed` | reader 251 (adapter) | "El testigo de la vista previa no tiene clave." (112), "La exploración del dibujo no trajo las envolventes en bruto." (118), "El documento está bloqueado por otra operación." (127), "No se pudo leer el dibujo: " + msg (131), "No se pudo leer la intención de edición: " + msg (135) |
| `Failed` | reader 216 (outer catch) | `ex.Message`, logged as `WorkspaceNavigationHost.ReadEditIntent` |

There is **no `NotFound` reader status**: `NotFound` is an Application outcome produced before any read (blank RackId or missing draft), e.g. `WorkspaceHostCore.cs:375, 389` (check), 468 (reload), 536, 550 (confirm), 628 (keep), 712 (discard-and-reload).

Application mapping (same switch in all six operations — check 395-406, reload 473-484, confirm 557-568, keep 636-647, discard-and-reload 717-728, begin 314-325):

| Reader status | Outcome |
|---|---|
| `Read` | continue; then `pilot == null` → `Failed(...)` (e.g. 409, 487, 571, 650, 731) |
| `NoActiveDocument`, `Busy` | `Waiting(read.Reason ?? "El dibujo no está listo para leerse.")` |
| `Rejected` | `Rejected(read.Reason ?? NotThisDocumentReason / literal)` |
| anything else (`Failed`) | `Failed(read.Reason ?? "Falló la relectura." / "Falló la lectura para la recarga." / …)` |

### C.6 Guards that pin the reader path

- `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs`
  - 405-434 `TheNestedReaderDoesTheSevenStepsInTheApprovedOrderAndTheAuthorizationComesBeforeTheRevalidation`: in-order `MdiActiveDocument`, `CommandInProgress`, `RackPilotIntentAdapter.LoadCatalog()`, `LockDocument()`, `_authorize(`, `_targets.Revalidate(`, `WorkspaceEditIntent.ChooseOriginDefinition(`, `RackPilotIntentAdapter.Read(`; `using\s*\(\s*document\.LockDocument\s*\(\s*\)\s*\)`; `string\.IsNullOrEmpty\(\s*document\.CommandInProgress\s*\)`; `\.Editor\.IsQuiescent`; Contains the four factories and `RequestDecision.Accepted`; host-wide counts `LoadCatalog(` = 1, `RackPilotIntentAdapter.Read(` = 1, `_targets.Revalidate(` = 1; body DoesNotMatch `_effects|_runner|ReturnFocusToDrawing|SetImpliedSelection|SetCurrentView|\.Focus\s*\(|GetObject|\.Commit\s*\(|OpenMode|ForWrite`. (No self-test; it reads only the real text.) Note: it pins `RequestDecision.Accepted` by presence, not the `!=` comparison.
  - 436-457 the reader is the only nested class (`Src.Count(host, @"\bclass\s+\w+") == 2`), `new PilotIntentReader(_targets, _authorize)` exactly once, it builds no targets/facts/policy.
  - 459-472, 474-486 (no `.Result|GetResult|StartNew|ContinueWith|+=|-=`; one `RackLog.Exception(`; no async/Task.Run/Dispatcher; every `LockDocument(` line has `using`).
  - 490-505 controller wiring: reader assigned once in the `Navigation` getter; the host is built with `(request, nativeId) => _bridge.Authorize(request, nativeId)` (504).
  - 562-580 `PassGate` order `_editIntentReader == null`, `_probe.ActiveDocumentNativeId`, `_modalRackCadActive`, `_probe.IsActiveDocumentQuiescent`, `Authorize(`; gate reads nothing.
  - Adapter: 132-148 shape; 150-174 no host-object fields (+ self-tests); 176-187, 189-204 locks/transactions only in `using`, `Read` body has no `LockDocument|StartTransaction` (199), preview has exactly one of each (201-203); 206-217, 231-267 Debug checks before/after on every path; 269-287 catch order and no `throw`; 289-320 `AdapterWrites` (includes `\.Commit\s*\(`) absent + self-tests; 322-348 exactly one `CaptureDrawing(` (+ self-test of a second one); 350-364, 366-376 (catalog: no lock, no `MdiActiveDocument`).
  - Scan: 74-118 raw envelopes; 120-128 `TheScanStillWritesNothingAndKeepsItsOneCommitOfTheReadOnlyTransaction` (`Assert.Equal(1, Src.Count(scan, @"\.Commit\s*\("))`, no `ForWrite|ForNotify|UpgradeOpen|AppendEntity|.Erase(|RackBlockData.Write|SetImpliedSelection|SetCurrentView|ProjectVariables`).
  - 598-624 no operation is called from the drain or the idle; 745-768 the three reconciliation bodies (one read each, applicability before the read).
- `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorGuardTests.cs:299-345` the three Spanish reasons of `ReadOnActiveDocument` (303-305) and the reasons of `LoadCatalog`/`Read` of the adapter (306-312), English absent.
- `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:426-437` `TheNavigationRunsSynchronouslyAndNoProductFileUsesTheInternalCommand`: no `async`/`await` in the host and in both workspace folders; **436**: `Assert.Empty(Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"ExecuteInCommandContextAsync")).Select(f => f.Path));`. 439-453: host, facts, effects, controller and selection reader must not match `\.Wait\s*\(|\.Result\b|GetAwaiter|…|Dispatcher|SendStringToExecute|\[CommandMethod|StartCommand|Autodesk\.AutoCAD\.Internal|DllImport|HwndSource|SetFocus|user32`. 549-563: facts adapter uses `using` for `LockDocument` and `StartTransaction`, no `.Commit(`/`ForWrite`.
- `tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs:207-216` the navigation host has no read/write exception (`SetImpliedSelection`, `SetCurrentView`, `GetObject`, `SelectImplied` all forbidden in its text); 261-271 no async in the two folders and `Assert.DoesNotContain("ExecuteInCommandContextAsync", hostCode)` (269).
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs`: `ForbiddenCallNames` (139-142) applied to every Plugin/Workspace file by `TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs` (170-180) — the navigation host gets the full list (no exception for `HostAdapterPath` in `HandlerForbiddenCallsFor`, 163-168); 232-239 transactions/locks only inside `using`; 241-248 no fields of host object types; 300-307 `PersistenceOrAuthoredWrite` (incl. `\.Commit\(`) absent from every Plugin/Workspace file. These do **not** cover `src/RackCad.Plugin/Views/` (adapter and scan), which are covered by `WorkspaceEditIntentGuardTests.cs` only.

---

## D. Controller forwards of the pilot port and the guards that pin them

`src/RackCad.Plugin/Workspace/WorkspaceController.cs:277-318` — nested `PilotEditAdapter : IWorkspacePilotEditPort`, built at 51 as `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));`:
```csharp
287	            public EditIntentOutcome TryBeginEdit(EditIntentRequest request) => _gate.TryBeginEdit(request);
289	            public EditCheckOutcome TryCheckNow(EditIntentRequest request) => _gate.TryCheckNow(request);
291	            public EditIntentOutcome TryReload(EditIntentRequest request) => _gate.TryReload(request);
293	            public EditConfirmOutcome TryConfirmBase(EditIntentRequest request) => _gate.TryConfirmBase(request);
295	            public EditIntentOutcome TryKeepMine(EditIntentRequest request) => _gate.TryKeepMine(request);
297	            public EditIntentOutcome TryDiscardAndReload(EditIntentRequest request) => _gate.TryDiscardAndReload(request);
```
(299-317: the other nine operations, also one-line forwards; `DiscardDocument` 311, `GetRacksAtRisk` 313.) The gate (`WorkspacePilotSupport.cs:167-183`) then forwards to the host core's explicit interface members (C.1).

Guards — `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs`:
- `Operations` table (44-61, 15 operations) and `ReadOperations` (64).
- `ControllerViolations` (482-530): `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));` (506); for every operation, the adapter member body, squashed of whitespace, must match `=>_gate\.<SameName>\([\w,]*\);$` (513-518) — a forward to a different operation name or any extra expression fails by construction of the regex; the argument is only constrained to `[\w,]*`. Also: `NotifyStateChanged` enter/leave depth (484-494), `PilotEdit` getter touches `Navigation` before `return _pilotEdit;` (496-504), no public `WorkspaceHostCore`, exactly one public `IWorkspacePilotEditPort`, exactly three `+=`, no `-=`, one `event` (521-529).
  - Real text: `TheControllerCountsTheNoticesWithTryFinallyExposesTheEdgePortThroughANestedAdapterAndNeverTheHost` (532-536).
  - Self-tests (Replace on the real text, after `Assert.Contains(from, controller)`): `TheControllerGuardTripsWithoutTheDepthTheNavigationTheDecoratorOrWhenTheHostIsExposedOrASubscriptionIsAdded` (538-555); forward mutants exist for `TryReload` (543), `TryConfirmBase` (544), `TryKeepMine` (545), `TryDiscardAndReload` (546), each by appending ` with { }`. No self-test mutant targets `TryBeginEdit` or `TryCheckNow`, and none swaps the target operation name.
- `GateViolations` (427-455) on `WorkspacePilotEditGate`: each read operation must contain `_depth.IsInside?` before `:_inner.<Name>(` and `.Waiting(InsideNoticeReason)`; every other operation must be `=>_inner.<Name>\([\w,]*\);$`. Real: 457-461; self-tests (Replace) 463-478 cover all six reads.
- Wiring order in the constructor: `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:300-314` (`_host = new WorkspaceHostCore(`, `_pilotEdit = new PilotEditAdapter(`, `_closeAdvisor = new WorkspaceCloseAdvisorRelay(_host,`, `_bridge.SetLifecycleAdvisor(_closeAdvisor);`; 3 `+=`, 0 `-=`).
- Core implementations of the three F4-T4A1 reads are pinned by `WorkspaceEditIntentGuardTests.cs:745-768` and `:684-711`; behavior tests in `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs` (names `TryConfirmBase`/`TryKeepMine`; not detailed here).
