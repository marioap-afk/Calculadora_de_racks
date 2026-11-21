using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4b — the GUARDS of the panel on the full draft that fail on the skeleton and pass on the implementation (CT-PBT4B-12, 15,
    /// 16 and 21). They read the sources as text (the tests do not reference the Plugin) and discriminate through their negative self-tests,
    /// run over text injected in memory, never written. Every text is read without comments.
    ///
    /// <para>
    /// CT-12: nothing translates a state of the pilot into a full one, and the host does not name the pilot. CT-15: the one call of
    /// «keep my draft» in the view-model. CT-16: the field of the panel validates with the one rule, without wiring. CT-21: one materialization
    /// of the full model per read.
    /// </para>
    /// </summary>
    public sealed class PushBackFullPanelGuardTests
    {
        private const string Host = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
        private const string Intent = "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs";
        private const string Support = "src/RackCad.Application/Workspace/WorkspacePilotSupport.cs";
        private const string Adapter = "src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs";
        private const string Navigation = "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs";
        private const string ViewModel = "src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs";
        private const string FieldAdapter = "src/RackCad.Application/Systems/PushBack/Draft/PushBackFullFieldAdapter.cs";
        private const string Controller = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
        private const string OriginByText = "src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs";

        private static int Count(string text, string pattern) => Regex.Matches(text, pattern).Count;

        /// <summary>The text of a member, from its signature to the matching closing brace; empty when the member is not there.</summary>
        private static string BodyOrEmpty(string code, string signaturePattern)
        {
            var match = Regex.Match(code, signaturePattern);
            if (!match.Success) return string.Empty;
            var open = code.IndexOf('{', match.Index + match.Length - 1);
            if (open < 0) return string.Empty;
            var depth = 0;
            for (var i = open; i < code.Length; i++)
            {
                if (code[i] == '{') depth++;
                else if (code[i] == '}' && --depth == 0) return code.Substring(match.Index, i - match.Index + 1);
            }

            return string.Empty;
        }

        // ------------------------------------------------------------------------------------------------- CT-12

        private static readonly (string Name, string Pattern, string Injection)[] PilotNames =
        {
            ("pilot.Read", @"\bpilot\s*\.\s*Read\b", "var injected = pilot.Read;"),
            ("KeepPilotEditsOn", @"\bKeepPilotEditsOn\b", "var injected = PushBackPilotMaterializer.KeepPilotEditsOn;"),
            ("PushBackPilotFieldAdapter", @"\bPushBackPilotFieldAdapter\b", "var injected = new PushBackPilotFieldAdapter();"),
            ("PushBackPilotStateComparer", @"\bPushBackPilotStateComparer\b", "var injected = PushBackPilotStateComparer.Instance;"),
            ("Find<PushBackPilotDraftState>", @"\bFind\s*<\s*PushBackPilotDraftState\s*>", "var injected = _drafts.Find<PushBackPilotDraftState>(session, rack);"),
            ("PushBackPilotUpdateRun.Execute(", @"\bPushBackPilotUpdateRun\s*\.\s*Execute\s*\(", "var injected = PushBackPilotUpdateRun.Execute(draft, request, port);"),
        };

        private const string Translation = @"\bnew\s+PushBackFullDraftState\s*\(";

        /// <summary>The files, among the five, that build a full draft state: nothing translates one from the state of the pilot.</summary>
        private static List<string> Translators(IEnumerable<(string Path, string Code)> files)
            => files.Where(file => Regex.IsMatch(file.Code, Translation)).Select(file => file.Path).ToList();

        /// <summary>The names of the pilot that the text of the host names.</summary>
        private static List<string> PilotNamesIn(string host)
            => PilotNames.Where(name => Regex.IsMatch(host, name.Pattern)).Select(name => name.Name).ToList();

        [Fact]
        public void CT_PBT4B_12_NothingTranslatesAStateOfThePilotIntoAFullOneAndTheHostDoesNotNameThePilot()
        {
            var log = new List<string>();
            var paths = new[] { Host, Intent, Support, Adapter, Navigation };
            var files = paths.Select(path => (Path: path, Code: WorkspaceHostSource.Code(path))).ToList();
            log.Add("sin-traduccion archivos=" + files.Count + " coincidencias=[" + string.Join(", ", Translators(files)) + "]");

            var host = files[0].Code;
            log.Add("whc-sin-piloto nombres=[" + string.Join(", ", PilotNamesIn(host)) + "]");

            var update = BodyOrEmpty(host, @"public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\(");
            log.Add("whc-actualizar ExecuteFull=" + Count(update, @"\bPushBackPilotUpdateRun\s*\.\s*ExecuteFull\s*\(")
                + " Precheck=" + Count(update, @"\bPushBackPilotUpdateRun\s*\.\s*Precheck\s*\("));

            // The self-tests, over text injected in memory: each name injected in the host makes the guard see it.
            foreach (var name in PilotNames)
            {
                log.Add("autoprueba-" + name.Name + "-en-WorkspaceHostCore.cs=" + (PilotNamesIn(host + "\n" + name.Injection + "\n").Contains(name.Name) ? "falla" : "pasa"));
            }

            var support = files[2];
            var injected = new[] { (Path: support.Path, Code: support.Code + "\nvar injected = new PushBackFullDraftState(editable, origin);\n") };
            log.Add("autoprueba-new PushBackFullDraftState-en-WorkspacePilotSupport.cs=" + (Translators(injected).Count > 0 ? "falla" : "pasa"));

            var expected = new List<string>
            {
                "sin-traduccion archivos=5 coincidencias=[]",
                "whc-sin-piloto nombres=[]",
                "whc-actualizar ExecuteFull=1 Precheck=1",
            };
            foreach (var name in new[] { "pilot.Read", "KeepPilotEditsOn", "PushBackPilotFieldAdapter", "PushBackPilotStateComparer", "Find<PushBackPilotDraftState>", "PushBackPilotUpdateRun.Execute(" })
            {
                expected.Add("autoprueba-" + name + "-en-WorkspaceHostCore.cs=falla");
            }

            expected.Add("autoprueba-new PushBackFullDraftState-en-WorkspacePilotSupport.cs=falla");
            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-15

        private const string KeepWitnessRequest = @"kind\s*==\s*ChoiceKind\.KeepMine\s*\?\s*keepWitness\s*:\s*null";
        private const string JoinOfTheLines = @"string\.Join\(\s*""\\n""\s*,\s*_keepLines\s*\)";

        /// <summary>The three lines that the text of the view-model gives: the one call to the port, the witness of the request and a text
        /// that the view does not compose.</summary>
        private static List<string> ViewModelLines(string code)
        {
            var confirm = BodyOrEmpty(code, @"public\s+void\s+ConfirmChoice\s*\(\s*\)");
            var total = Count(code, @"\bTryKeepMine\b");
            var inConfirm = Count(confirm, @"\b_port\s*\.\s*TryKeepMine\s*\(\s*request\s*\)");
            var property = Regex.Match(code, @"KeepMinePresentationText\s*=>\s*([^;]*);");
            var expression = property.Success ? property.Groups[1].Value : string.Empty;
            var literals = Count(expression, "\"(?:\\\\.|[^\"\\\\])*\"");
            return new List<string>
            {
                "fvm TryKeepMine=" + total + " en=" + (total == 1 && inConfirm == 1 ? "ConfirmChoice" : "otro"),
                "fvm peticion-con-testigo=" + (Count(confirm, KeepWitnessRequest) == 1),
                "fvm presentacion-sin-componer=" + (Count(expression, JoinOfTheLines) == 1 && literals == 1),
            };
        }

        [Fact]
        public void CT_PBT4B_15_TheViewModelCallsKeepMineOnceInsideTheChoiceCarriesTheWitnessAndComposesNoText()
        {
            var log = new List<string>();
            var code = WorkspaceHostSource.Code(ViewModel);
            var real = ViewModelLines(code);
            log.AddRange(real);

            // The self-tests, over text injected in memory.
            var second = code + "\n_port.TryKeepMine(request);\n";
            log.Add("autoprueba-segundo-TryKeepMine=" + (ViewModelLines(second).SequenceEqual(real) ? "pasa" : "falla"));

            var composed = Regex.Replace(code, @"(KeepMinePresentationText\s*=>\s*)", "$1\"Cambiado: \" + ");
            log.Add("autoprueba-texto-compuesto=" + (ViewModelLines(composed).SequenceEqual(real) ? "pasa" : "falla"));

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "fvm TryKeepMine=1 en=ConfirmChoice",
                        "fvm peticion-con-testigo=True",
                        "fvm presentacion-sin-componer=True",
                        "autoprueba-segundo-TryKeepMine=falla",
                        "autoprueba-texto-compuesto=falla",
                    }),
                string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-16

        [Fact]
        public void CT_PBT4B_16_TheHostHasOneStaticAdapterOfTheFieldThatCallsTheOneRuleAndTheControllerAssignsNoRule()
        {
            var log = new List<string>();
            var host = WorkspaceHostSource.Code(Host);
            var adapter = WorkspaceHostSource.Code(FieldAdapter);
            var controller = WorkspaceHostSource.Code(Controller);

            log.Add("whc-adaptador PushBackFullFieldAdapter estatico="
                + Count(host, @"\bprivate\s+static\s+readonly\s+PushBackFullFieldAdapter\s+\w+\s*=")
                + " instancia=" + Count(host, @"\b(?:private|public|internal|protected)\s+(?:readonly\s+)?PushBackFullFieldAdapter\s+\w+\s*(?:=|;)"));
            log.Add("adaptador-regla PushBackAnnotationScaleRule.Validate=" + Count(adapter, @"\bPushBackAnnotationScaleRule\s*\.\s*Validate\s*\("));
            log.Add("wco-sin-regla asignaciones=" + Count(controller, @"\b\w*(?:Rule|Validator|Validation)\w*\s*=(?![=>])"));
            log.Add("wco-sin-ui RackCad.UI=" + Count(controller, @"\bRackCad\s*\.\s*UI\b"));

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "whc-adaptador PushBackFullFieldAdapter estatico=1 instancia=0",
                        "adaptador-regla PushBackAnnotationScaleRule.Validate=1",
                        "wco-sin-regla asignaciones=0",
                        "wco-sin-ui RackCad.UI=0",
                    }),
                string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-21

        private const string PrimaryFull = @"\bread\s*\.\s*Full\b";
        private const string FullReadCall = @"PushBackFullDraftReader\.Read\s*\(";
        private const string Deferred = @"new\s+System\.Lazy<PushBackFullIntentResult>\s*\(\s*\(\s*\)\s*=>\s*PushBackFullDraftReader\.Read\s*\(\s*input\s*,\s*catalog\s*\)\s*\)";
        private const string FullOfTheResult = @"internal\s+PushBackFullIntentResult\?\s+Full\s*=>\s*_full\s*\?\.\s*Value\s*;";

        /// <summary>The five lines that the text of the adapter, of the host and of the choice of the origin give.</summary>
        private static List<string> Lines21(string adapter, string host, string origin)
        {
            var read = BodyOrEmpty(adapter, @"internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(");
            var atOrigin = BodyOrEmpty(adapter, @"internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\(");
            var result = BodyOrEmpty(adapter, @"internal\s+sealed\s+class\s+RackPilotIntentReadResult\b");
            var active = BodyOrEmpty(host, @"private\s+EditIntentReadResult\s+ReadOnActiveDocument\s*\(");
            var locked = BodyOrEmpty(active, @"using\s*\(\s*document\.LockDocument\s*\(\s*\)\s*\)");
            var fullAt = Regex.Match(active, PrimaryFull);
            var originCall = Regex.Match(active, @"RackPilotIntentAdapter\.AtOrigin\s*\(");
            var readAt = BodyOrEmpty(origin, @"public\s+static\s+\([^)]*\)\?\s+ReadAt\s*\(");
            return new List<string>
            {
                "ria-Read completo=" + Count(read, FullReadCall) + " diferido=" + Count(read, Deferred),
                "ria-resultado Full-diferido=" + Count(result, FullOfTheResult)
                    + " campo=" + Count(result, @"private\s+readonly\s+System\.Lazy<PushBackFullIntentResult>\?\s+_full\s*;")
                    + " de-un-valor=" + Count(result, @"_full\s*=\s*new\s+System\.Lazy<PushBackFullIntentResult>\s*\(\s*full\s*\)\s*;")
                    + " de-una-lectura-diferida=" + Count(result, @"_full\s*=\s*full\s*;"),
                "ria-AtOrigin Full-del-Primary=" + Count(atOrigin, PrimaryFull),
                "nh-Full=" + Count(host, PrimaryFull) + " dentro-del-bloqueo=" + (Count(locked, PrimaryFull) == 1)
                    + " tras-AtOrigin=" + (fullAt.Success && originCall.Success && fullAt.Index > originCall.Index),
                "readat completo=" + Count(readAt, FullReadCall),
            };
        }

        [Fact]
        public void CT_PBT4B_21_TheFullModelIsMaterializedOnceByReadTheAdapterDefersItAndTheRereadAtTheOriginDoesNotAskForTheOneOfThePrimary()
        {
            var log = new List<string>();
            var adapter = WorkspaceHostSource.Code(Adapter);
            var host = WorkspaceHostSource.Code(Navigation);
            var origin = WorkspaceHostSource.Code(OriginByText);
            var real = Lines21(adapter, host, origin);
            log.AddRange(real);

            // The self-tests, over text injected in memory (never written): each makes the guard see another line than the real one.
            var eager = Regex.Replace(adapter, Deferred, "PushBackFullDraftReader.Read(input, catalog)");
            log.Add("autoprueba-completo-inmediato=" + (Lines21(eager, host, origin).SequenceEqual(real) ? "pasa" : "falla"));

            var inOrigin = Regex.Replace(
                adapter,
                @"(internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\([^)]*\)\s*\{)",
                "$1 var injected = read.Full;");
            log.Add("autoprueba-Full-del-Primary-en-AtOrigin=" + (Lines21(inOrigin, host, origin).SequenceEqual(real) ? "pasa" : "falla"));

            var secondFull = host + "\nvar injected = read.Full;\n";
            log.Add("autoprueba-segundo-Full-en-NH=" + (Lines21(adapter, secondFull, origin).SequenceEqual(real) ? "pasa" : "falla"));

            var stored = Regex.Replace(adapter, FullOfTheResult, "internal PushBackFullIntentResult? Full { get; }");
            log.Add("autoprueba-Full-sin-diferir=" + (Lines21(stored, host, origin).SequenceEqual(real) ? "pasa" : "falla"));

            var twice = Regex.Replace(origin, @"(var\s+input\s*=\s*AtKey\s*\(\s*scan\s*,\s*key\s*\)\s*;)", "$1 var injected = PushBackFullDraftReader.Read(input, catalog);");
            log.Add("autoprueba-segundo-completo-en-ReadAt=" + (Lines21(adapter, host, twice).SequenceEqual(real) ? "pasa" : "falla"));

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "ria-Read completo=1 diferido=1",
                        "ria-resultado Full-diferido=1 campo=1 de-un-valor=1 de-una-lectura-diferida=1",
                        "ria-AtOrigin Full-del-Primary=0",
                        "nh-Full=1 dentro-del-bloqueo=True tras-AtOrigin=True",
                        "readat completo=1",
                        "autoprueba-completo-inmediato=falla",
                        "autoprueba-Full-del-Primary-en-AtOrigin=falla",
                        "autoprueba-segundo-Full-en-NH=falla",
                        "autoprueba-Full-sin-diferir=falla",
                        "autoprueba-segundo-completo-en-ReadAt=falla",
                    }),
                string.Join("\n", log));
        }
    }
}
