using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4a — the path of the full read (CT-PBT4A-01 to 04 and 07): the result of the read carries the full model and the
    /// catalog next to the one of the pilot (<c>FromReads</c>), the five rereads of an open draft go to the origin of the draft by its
    /// text when the reader offers it (A-T3-8), the choice by text and the reread at it are pure, and the Plugin does it over the same
    /// scan. No AutoCAD: the doubles of the host, the pure readers and the text of the Plugin, read as text. A step that throws becomes
    /// an <c>EXCEPCION</c> line and never escapes: the CT fails by its one assertion.
    /// </summary>
    public sealed class PushBackFullReadPathTests
    {
        private const string AdapterPath = "src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs";
        private const string HostPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs";

        // ---------------------------------------------------------------------------------------------- helpers

        /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
        private static T Paso<T>(List<string> log, Func<T> step)
        {
            try
            {
                return step();
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
                return default(T);
            }
        }

        private static void Paso(List<string> log, Action step) => Paso<int>(log, () => { step(); return 0; });

        /// <summary>The name of the type of the exception that a call throws, or <c>ninguna</c>.</summary>
        private static string Thrown(Action call)
        {
            try
            {
                call();
                return "ninguna";
            }
            catch (Exception error)
            {
                return error.GetType().Name;
            }
        }

        private static string Estado(bool admitted) => admitted ? "Admitted" : "NotAdmitted";

        private static string Show(string value) => value ?? "null";

        private static string NullOrSet(object value) => value == null ? "null" : "no-nulo";

        // ------------------------------------------------------------------------------------------------- CT-01

        [Fact]
        public void CT_PBT4A_01_FromReadsCarriesThePilotTheFullReadAndTheCatalogAndTheOtherStatesCarryNeither()
        {
            var log = new List<string>();
            var catalog = PilotKit.Catalog;
            var scan = IntentKit.TwoViews();
            var pilot = Paso(log, () => PushBackPilotIntentReader.Read(scan, catalog));
            var full = Paso(log, () => PushBackFullDraftReader.Read(scan, catalog));

            var joined = Paso(log, () => EditIntentReadResult.FromReads(pilot, full, catalog));
            log.Add("fromreads piloto=" + ReferenceEquals(joined.PilotRead, pilot)
                + " completo=" + ReferenceEquals(joined.FullRead, full)
                + " catalogo=" + ReferenceEquals(joined.Catalog, catalog)
                + " estado=" + joined.Status);

            log.Add("fromreads-nulo piloto=" + Thrown(() => EditIntentReadResult.FromReads(null, full, catalog))
                + " completo=" + Thrown(() => EditIntentReadResult.FromReads(pilot, null, catalog))
                + " catalogo=" + Thrown(() => EditIntentReadResult.FromReads(pilot, full, null)));

            var fromPilot = Paso(log, () => EditIntentReadResult.FromPilot(pilot));
            log.Add("frompilot completo=" + NullOrSet(fromPilot.FullRead) + " catalogo=" + NullOrSet(fromPilot.Catalog));

            var others = new[]
            {
                EditIntentReadResult.NoActiveDocument("x"), EditIntentReadResult.Busy("x"), EditIntentReadResult.Rejected("x"), EditIntentReadResult.Failed("x"),
            };
            log.Add("otros completo=" + (others.All(other => other.FullRead == null) ? "null" : "no-nulo")
                + " catalogo=" + (others.All(other => other.Catalog == null) ? "null" : "no-nulo"));

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "fromreads piloto=True completo=True catalogo=True estado=Read",
                        "fromreads-nulo piloto=ArgumentNullException completo=ArgumentNullException catalogo=ArgumentNullException",
                        "frompilot completo=null catalogo=null",
                        "otros completo=null catalogo=null",
                    }),
                string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-02

        private static string ReadsLine(List<string> log, string label, PushBackPilotIntentScan scan)
        {
            var read = Paso(log, () => PushBackFullReadKit.Reads(scan));
            var pilot = read.PilotRead;
            var full = read.FullRead;
            var same = pilot != null && full != null
                && string.Equals(pilot.OriginKey, full.OriginKey, StringComparison.Ordinal)
                && PushBackFullReadKit.SameBase(pilot.StaleBase, full.StaleBase);
            return label + " piloto=" + Estado(pilot != null && pilot.Read.IsAdmitted)
                + " completo=" + Estado(full != null && full.Read.IsAdmitted)
                + " mismo-escaneo=" + same;
        }

        [Fact]
        public void CT_PBT4A_02_TheFullReadAndThePilotReadAreOfTheSameScanOfTheDrawing()
        {
            var log = new List<string>();
            var baseText = IntentKit.BaseText();
            var abText = PilotKit.Text("pb-20-ab");
            var coldText = PilotKit.Text("pb-21-cola");
            var rich = I58F1Fixtures.Rich("pushback");
            var future = IntentKit.Rewrite(baseText, e => e.Design = I58F1Fixtures.Set(rich, "PushBack.Future", "1"));

            log.Add(ReadsLine(log, "pb-01-base", IntentKit.TwoViews()));
            log.Add(ReadsLine(log, "pb-20-ab", IntentKit.Scan(new RackEmbedStore().Deserialize(abText).Id, "H1", IntentKit.Member("H1", abText))));
            log.Add(ReadsLine(log, "pb-21-cola", IntentKit.Scan(new RackEmbedStore().Deserialize(coldText).Id, "H1", IntentKit.Member("H1", coldText))));
            log.Add(ReadsLine(log, "auth-13", IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", future))));

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "pb-01-base piloto=Admitted completo=Admitted mismo-escaneo=True",
                        "pb-20-ab piloto=NotAdmitted completo=Admitted mismo-escaneo=True",
                        "pb-21-cola piloto=NotAdmitted completo=NotAdmitted mismo-escaneo=True",
                        "auth-13 piloto=NotAdmitted completo=NotAdmitted mismo-escaneo=True",
                    }),
                string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-03

        [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su equivalente en PushBackFullReadPathFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-list-477-decision.md).")]
        public void CT_PBT4A_03_TheFiveRereadsOfAnOpenDraftReadAtTheOriginOfTheDraftAndBeginReadsAsToday()
        {
            var log = new List<string>();

            var opening = new PushBackOriginReaderDouble();
            Paso(log, () => new PushBackFullReadRig(opening).Begin());
            log.Add("Begin lector=" + (opening.Kinds.Count == 1 ? opening.Kinds[0] : opening.Kinds.Count + " lecturas"));

            var expected = new List<string> { "Begin lector=ReadIntent" };
            foreach (var operation in PushBackFullReadKit.Operations)
            {
                expected.Add(operation + " lector=ReadIntentAtOrigin origen-del-borrador=True");
                var reader = new PushBackOriginReaderDouble();
                var rig = new PushBackFullReadRig(reader);
                var draft = Paso(log, () => rig.Begin());
                if (draft == null) continue;

                // The reading is armed right after its last gesture: the origin of the draft is taken before the reread.
                var origin = draft.CurrentDraftState.Origin;
                var before = reader.Kinds.Count;
                Paso(log, () => rig.Run(operation, draft));
                var kinds = reader.Kinds.Skip(before).ToList();
                if (kinds.Count != 1)
                {
                    log.Add(operation + " lector=" + kinds.Count + " lecturas");
                    continue;
                }

                var received = reader.Origins[before];
                log.Add(operation + " lector=" + kinds[0] + (received == null ? string.Empty : " origen-del-borrador=" + string.Equals(received, origin, StringComparison.Ordinal)));
            }

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-04

        /// <summary>The rest of a scan moved to another origin: the rack, the facts, the raw text, the evidence and the witness.</summary>
        private static bool SameRest(PushBackPilotIntentScan scan, PushBackPilotIntentScan moved)
            => moved != null && !ReferenceEquals(moved, scan)
               && string.Equals(scan.RackId, moved.RackId, StringComparison.Ordinal)
               && scan.Facts.SequenceEqual(moved.Facts)
               && scan.RawEnvelopes.Count == moved.RawEnvelopes.Count
               && scan.RawEnvelopes.All(pair => moved.RawEnvelopes.TryGetValue(pair.Key, out var text) && string.Equals(text, pair.Value, StringComparison.Ordinal))
               && string.Equals(scan.CompletenessEvidence, moved.CompletenessEvidence, StringComparison.Ordinal)
               && ReferenceEquals(scan.PreviewWitness, moved.PreviewWitness);

        [Fact]
        public void CT_PBT4A_04_TheOriginIsChosenByTheTextOfTheDraftAndTheRereadAtItIsPure()
        {
            var log = new List<string>();
            var catalog = PilotKit.Catalog;
            var plain = IntentKit.BaseText();
            var lateral = IntentKit.Variant("pb-01-base", IntentKit.Rack, view: "lateral", section: 3);
            var absent = "{ the text of no definition";

            var two = IntentKit.TwoViews();
            var twin = IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", plain), IntentKit.Member("C3", plain));
            var readOnly = IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", plain), IntentKit.Member("C3", lateral, xref: true));

            log.Add("choose uno-mutable=" + Show(Paso(log, () => PushBackOriginByText.Choose(PushBackPilotIntentReader.BuildStaleBase(two), lateral))));
            log.Add("choose ninguno=" + Show(Paso(log, () => PushBackOriginByText.Choose(PushBackPilotIntentReader.BuildStaleBase(two), absent))));
            log.Add("choose varios=" + Show(Paso(log, () => PushBackOriginByText.Choose(PushBackPilotIntentReader.BuildStaleBase(twin), plain))));
            log.Add("choose no-mutable=" + Show(Paso(log, () => PushBackOriginByText.Choose(PushBackPilotIntentReader.BuildStaleBase(readOnly), lateral))));

            var witness = new PushBackPilotPreviewWitness("A1", "{ preview");
            var withWitness = IntentKit.Scan(IntentKit.Rack, "A1", witness, IntentKit.Member("A1", plain), IntentKit.Member("B2", lateral));
            var moved = Paso(log, () => PushBackOriginByText.AtKey(withWitness, "B2"));
            log.Add("atkey clave=" + (moved == null ? "<sin-resultado>" : moved.OriginKey) + " resto-igual=" + SameRest(withWitness, moved));

            // The Primary of the drawing is B2, and the draft was opened at A1: the text of the draft finds A1.
            var elsewhere = IntentKit.Scan(IntentKit.Rack, "B2", IntentKit.Member("A1", plain), IntentKit.Member("B2", lateral));
            var at = Paso(log, () => PushBackOriginByText.ReadAt(elsewhere, plain, catalog));
            var key = at.HasValue ? at.Value.Scan.OriginKey : "<sin-resultado>";
            var pilotAt = at.HasValue ? Estado(at.Value.Pilot.Read.IsAdmitted) : "<sin-resultado>";
            var fullAt = at.HasValue ? Estado(at.Value.Full.Read.IsAdmitted) : "<sin-resultado>";
            var same = at.HasValue && SameRest(elsewhere, at.Value.Scan)
                && string.Equals(at.Value.Pilot.OriginKey, key, StringComparison.Ordinal)
                && string.Equals(at.Value.Full.OriginKey, key, StringComparison.Ordinal)
                && PushBackFullReadKit.SameBase(at.Value.Pilot.StaleBase, at.Value.Full.StaleBase);
            log.Add("readat otro-origen clave=" + key + " piloto=" + pilotAt + " completo=" + fullAt + " mismo-escaneo=" + same);

            var sameOrigin = Paso(log, () => PushBackOriginByText.ReadAt(two, plain, catalog));
            log.Add("readat mismo-origen=" + (sameOrigin.HasValue ? "tupla" : "null"));
            var unmatched = Paso(log, () => PushBackOriginByText.ReadAt(two, absent, catalog));
            log.Add("readat sin-coincidencia=" + (unmatched.HasValue ? "tupla" : "null"));

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "choose uno-mutable=B2",
                        "choose ninguno=null",
                        "choose varios=null",
                        "choose no-mutable=null",
                        "atkey clave=B2 resto-igual=True",
                        "readat otro-origen clave=A1 piloto=Admitted completo=Admitted mismo-escaneo=True",
                        "readat mismo-origen=null",
                        "readat sin-coincidencia=null",
                    }),
                string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-07

        private const string SignatureOfRead =
            @"internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(\s*Document\s+document\s*,\s*string\s+rackId\s*,\s*long\s+originDefinitionHandle\s*,\s*string\??\s+witnessKey\s*,\s*string\??\s+witnessText\s*,\s*RackCatalog\s+catalog\s*\)";

        private static int Count(string text, string pattern) => Regex.Matches(text, pattern).Count;

        /// <summary>The body of a member, from its signature to the matching closing brace; empty when the member is not there.</summary>
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

        /// <summary>The eight lines that the text of the adapter and of the host of the Plugin give.</summary>
        private static List<string> Lines(string adapter, string host)
        {
            var read = BodyOrEmpty(adapter, @"internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(");
            var atOrigin = BodyOrEmpty(adapter, @"internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\(");
            const string pilotRead = @"PushBackPilotIntentReader\.Read\s*\(";
            const string fullRead = @"PushBackFullDraftReader\.Read\s*\(";
            var pilotAt = Regex.Match(read, pilotRead);
            var fullAt = Regex.Match(read, fullRead);
            var afterThePilot = pilotAt.Success && fullAt.Success && fullAt.Index > pilotAt.Index
                && Regex.IsMatch(read, @"PushBackFullDraftReader\.Read\s*\(\s*input\s*,\s*catalog\s*\)");

            var lines = new List<string>
            {
                "ria-firma-de-Read-sin-cambio=" + Regex.IsMatch(adapter, SignatureOfRead),
                "ria-Read completo=" + Count(read, fullRead) + " tras-el-piloto=" + afterThePilot,
                "ria-lectores piloto=" + Count(adapter, pilotRead) + " completo=" + Count(adapter, fullRead) + " escaneos=" + Count(adapter, @"\bnew\s+PushBackPilotIntentScan\s*\("),
                "ria-AtOrigin readat=" + Count(atOrigin, @"PushBackOriginByText\.ReadAt\s*\(")
                    + " piloto=" + Count(atOrigin, pilotRead)
                    + " completo=" + Count(atOrigin, fullRead)
                    + " base=" + Count(atOrigin, @"StaleBase")
                    + " lecturas-del-dibujo=" + Count(atOrigin, @"CaptureDrawing|LockDocument|Transaction|ex\.Message")
                    + " literales=" + Count(atOrigin, "\"(?:\\\\.|[^\"\\\\])*\""),
            };

            var declaration = Regex.Match(host, @"private\s+sealed\s+class\s+PilotIntentReader\s*:\s*([^{]*)\{");
            var interfaces = declaration.Success
                ? string.Join(",", declaration.Groups[1].Value.Split(',').Select(name => name.Trim()))
                : string.Empty;
            lines.Add("nh-lector interfaces=" + interfaces);

            var reread = BodyOrEmpty(host, @"public\s+EditIntentReadResult\s+ReadIntentAtOrigin\s*\(");
            lines.Add("nh-ReadIntentAtOrigin ReadOnActiveDocument=" + Count(reread, @"\bReadOnActiveDocument\s*\("));

            const string atOriginCall = @"RackPilotIntentAdapter\.AtOrigin\s*\(";
            var active = BodyOrEmpty(host, @"private\s+EditIntentReadResult\s+ReadOnActiveDocument\s*\(");
            var locked = BodyOrEmpty(active, @"using\s*\(\s*document\.LockDocument\s*\(\s*\)\s*\)");
            var readAt = active.IndexOf("RackPilotIntentAdapter.Read(", StringComparison.Ordinal);
            var originAt = Regex.Match(active, atOriginCall);
            lines.Add("nh-AtOrigin=" + Count(host, atOriginCall)
                + " dentro-del-bloqueo=" + (Count(locked, atOriginCall) == 1)
                + " tras-Read=" + (readAt >= 0 && originAt.Success && originAt.Index > readAt));
            lines.Add("nh-resultado FromReads=" + Count(host, @"EditIntentReadResult\.FromReads\s*\(") + " FromPilot=" + Count(host, @"EditIntentReadResult\.FromPilot\s*\("));
            return lines;
        }

        [Fact]
        public void CT_PBT4A_07_ThePluginReadsTheFullModelOverTheSameScanAndRereadsAtTheOriginWithoutReadingTheDrawingAgain()
        {
            var log = new List<string>();
            var adapter = WorkspaceHostSource.Code(AdapterPath);
            var host = WorkspaceHostSource.Code(HostPath);
            var real = Lines(adapter, host);
            log.AddRange(real);

            // The self-tests, over text injected in memory (never written): each makes the guard see another line than the real one.
            var withoutFull = Regex.Replace(adapter, @"PushBackFullDraftReader\.Read\s*\(", "PushBackOtherReader.Read(");
            log.Add("autoprueba-sin-lector-completo=" + (Lines(withoutFull, host).SequenceEqual(real) ? "pasa" : "falla"));

            var withDrawing = Regex.Replace(
                adapter,
                @"(internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\([^)]*\)\s*\{)",
                "$1 var injected = RackSiblingScan.CaptureDrawing(null);");
            log.Add("autoprueba-AtOrigin-con-CaptureDrawing=" + (Lines(withDrawing, host).SequenceEqual(real) ? "pasa" : "falla"));

            var second = host + "\nread = RackPilotIntentAdapter.AtOrigin(read, originText, catalog.Catalog);\n";
            log.Add("autoprueba-segundo-AtOrigin=" + (Lines(adapter, second).SequenceEqual(real) ? "pasa" : "falla"));

            var fromPilot = host + "\nvar injected = EditIntentReadResult.FromPilot(read.Pilot);\n";
            log.Add("autoprueba-FromPilot-en-NH=" + (Lines(adapter, fromPilot).SequenceEqual(real) ? "pasa" : "falla"));

            var againPilot = adapter + "\nvar injected = PushBackPilotIntentReader.Read(input, catalog);\n";
            log.Add("autoprueba-segunda-lectura-del-piloto-en-RIA=" + (Lines(againPilot, host).SequenceEqual(real) ? "pasa" : "falla"));

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "ria-firma-de-Read-sin-cambio=True",
                        "ria-Read completo=1 tras-el-piloto=True",
                        "ria-lectores piloto=1 completo=1 escaneos=1",
                        "ria-AtOrigin readat=1 piloto=0 completo=0 base=0 lecturas-del-dibujo=0 literales=0",
                        "nh-lector interfaces=IWorkspaceEditIntentReader,IWorkspaceEditIntentOriginReader",
                        "nh-ReadIntentAtOrigin ReadOnActiveDocument=1",
                        "nh-AtOrigin=1 dentro-del-bloqueo=True tras-Read=True",
                        "nh-resultado FromReads=1 FromPilot=0",
                        "autoprueba-sin-lector-completo=falla",
                        "autoprueba-AtOrigin-con-CaptureDrawing=falla",
                        "autoprueba-segundo-AtOrigin=falla",
                        "autoprueba-FromPilot-en-NH=falla",
                        "autoprueba-segunda-lectura-del-piloto-en-RIA=falla",
                    }),
                string.Join("\n", log));
        }
    }
}
