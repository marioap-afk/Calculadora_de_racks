using System;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.CustomProperties;
using RackCad.Application.VentanaRackCad;
using Xunit;
using static RackCad.Tests.CustomPropertiesAuthorityTestKit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// T6 pr (I-64, sexta tanda; VA-S9, answered by the Owner on 2026-11-27: «Sí, y se escriben al insertarlo»): the tab «Propiedades» of
    /// a draft of «Nuevo» is the one of VA-07, and the NUEVO command that inserts the rack writes its draft after the insertion, in the same
    /// command (one «U»), only over the rack it has just inserted while it holds no property. The pure rules here; the commands of the door
    /// (AutoCAD cannot run here) on their source.
    /// </summary>
    public class NewRackPropertiesTests
    {
        // A Selectivo of two views just inserted: no view carries properties yet.
        private static RackCustomPropertiesDefinition[] Inserted() => new[]
        {
            View("H1"),
            View("H2", view: "lateral", section: 0),
        };

        private static RackPropertiesWrite NewRackWrite() => new RackPropertiesWrite(null, new[]
        {
            new CustomPropertyDraftEntry(null, "Zona", "A"),
            new CustomPropertyDraftEntry(null, "Nota", "Cliente B"),
        });

        private static RackDesignWritten Placed() => RackDoorRules.AnswerForNewRack(4, RackA, 2, "Selectivo 3");

        [Fact]
        public void TheTabOfANewRackIsWritableEmptyAndHasNothingRead()
        {
            var read = RackPropertiesRules.ForNewRack();

            Assert.True(read.NewRack);
            Assert.True(read.Writable);
            Assert.False(read.Divergent);
            Assert.Null(read.Reason);
            Assert.Empty(read.Entries);
            Assert.Empty(read.Views);
            Assert.Null(read.Fingerprint); // nothing was read: no write of «Actualizar» can pass with it (it is not the fingerprint of any rack)

            // A rack read from the drawing is not a new one.
            Assert.False(RackPropertiesRules.Read(Authority("H1", Inserted())).NewRack);
        }

        [Fact]
        public void TheDraftOfANewRackIsWrittenInEveryViewOfTheRackJustInserted()
        {
            var inserted = Authority("H1", Inserted());
            var fingerprint = RackPropertiesRules.NewRackFingerprint(inserted, out var refusal);

            Assert.Null(refusal);
            Assert.Equal(RackPropertiesRules.Fingerprint(inserted), fingerprint);

            var write = NewRackWrite();
            var plan = RackPropertiesRules.Plan(inserted, write with { Fingerprint = fingerprint });
            Assert.True(plan.IsPlanned, plan.Error);
            Assert.Equal(new[] { "H1", "H2" }, plan.Plan.Select(p => p.Handle).OrderBy(h => h, StringComparer.Ordinal).ToArray());

            var after = Authority(AfterPlan(Inserted(), plan), "H1");
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, after.Outcome);
            Assert.Equal(new[] { ("Zona", "A"), ("Nota", "Cliente B") },
                after.Collection.Document.Entries.Select(e => (e.Name, e.Value)).ToArray());
            Assert.All(after.Collection.Document.Entries, e => Assert.False(string.IsNullOrWhiteSpace(e.Id))); // Application minted the ids

            // The tab of the rack, read again, is the one of a rack of the drawing.
            var tab = RackPropertiesRules.Read(after);
            Assert.False(tab.NewRack);
            Assert.True(tab.Writable);
            Assert.Equal(2, tab.Entries.Count);

            // As written by the tab (no fingerprint), the draft never passes the plan of «Actualizar»: only the NUEVO command writes it.
            Assert.False(RackPropertiesRules.Plan(inserted, write).IsPlanned);
        }

        [Fact]
        public void TheDraftOfANewRackIsNeverWrittenOverPropertiesItDidNotSeeNorOverAReadOnlyRack()
        {
            // The rack already holds properties.
            var holding = Authority("H1", View("H1", properties: Props(Entry(IdA, "Zona", "B"))), View("H2", properties: Props(Entry(IdA, "Zona", "B")), view: "planta"));
            Assert.Null(RackPropertiesRules.NewRackFingerprint(holding, out var refusal));
            Assert.Equal("el rack nuevo ya tenía propiedades al insertarse y no se escriben encima.", refusal);

            // Its views differ.
            var divergent = Authority("H1", View("H1", properties: Props(Entry(IdA, "Zona", "B"))), View("H2", view: "planta"));
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Divergent, divergent.Outcome);
            Assert.Null(RackPropertiesRules.NewRackFingerprint(divergent, out refusal));
            Assert.Equal(divergent.Error.Trim(), refusal);

            // The drawing has a payload this build cannot interpret (the rule of RACKPROPIEDADES: read only).
            var indeterminate = Authority("H1", View("H1"), Uninterpretable("H9"));
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.IndeterminateMembership, indeterminate.Outcome);
            Assert.Null(RackPropertiesRules.NewRackFingerprint(indeterminate, out refusal));
            Assert.Equal(indeterminate.Error.Trim(), refusal);

            // Nothing read.
            Assert.Null(RackPropertiesRules.NewRackFingerprint(null, out refusal));
            Assert.Equal("no se pudieron leer las del rack nuevo.", refusal);
        }

        [Fact]
        public void TheAnswerOfTheInsertionSaysWhatHappenedToThePropertiesAndCarriesWhatTheDrawingHolds()
        {
            var placed = Placed();
            var after = RackPropertiesRules.Read(Authority("H1", Inserted()));

            // Nothing to write: the message of the insertion as it was, with the tab of the rack of the drawing.
            var nothing = RackPropertiesRules.AnswerForNewRack(placed, null, null, after);
            Assert.Equal(placed.Message, nothing.Message);
            Assert.Same(after, nothing.Properties);
            Assert.Null(nothing.PropertiesError);
            Assert.True(nothing.Written);
            Assert.Equal("Selectivo 3", nothing.Name);

            // Written.
            var written = RackPropertiesRules.AnswerForNewRack(placed, NewRackWrite(), null, after);
            Assert.Equal(placed.Message + " También escribió sus propiedades.", written.Message);
            Assert.Same(after, written.Properties);
            Assert.Null(written.PropertiesError);

            // Not written: why, and that the tab keeps them when the rack can hold them.
            var kept = RackPropertiesRules.AnswerForNewRack(placed, NewRackWrite(), "x.", after);
            Assert.Equal(placed.Message + " Pero sus propiedades no se escribieron: x. Siguen en su pestaña «Propiedades»: escríbelas con «Actualizar».", kept.Message);
            Assert.Equal("x.", kept.PropertiesError);
            Assert.True(kept.Written); // the rack IS in the drawing
            var readOnly = RackPropertiesRules.Read(Authority("H1", View("H1"), Uninterpretable("H9")));
            var lost = RackPropertiesRules.AnswerForNewRack(placed, NewRackWrite(), "x.", readOnly);
            Assert.Equal(placed.Message + " Pero sus propiedades no se escribieron: x.", lost.Message);

            // Not placed: the answer does not change (the draft stays in «Nuevo» with its properties).
            var refused = RackDoorRules.AnswerForNewRack(4, RackA, 0, null);
            Assert.Same(refused, RackPropertiesRules.AnswerForNewRack(refused, NewRackWrite(), null, after));
        }

        // ------------------------------------------------------------------ the commands of the door, on their source

        private const string Door = "src/RackCad.Plugin/RackPropertiesDoorData.cs";

        public static TheoryData<string, string, string, string> NewCommands() => new()
        {
            { "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs", @"public\s+void\s+InsertNewSelective\s*\(", "RACKSELECTIVONUEVO", "write.Properties" },
            { "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs", @"public\s+void\s+InsertNewRack\s*\(", "RACKDINAMICONUEVO", "pending.Properties" },
            { "src/RackCad.Plugin/VentanaRackCad/RackCadCamaDoorCommands.cs", @"public\s+void\s+InsertNewCama\s*\(", "RACKCAMANUEVO", "pending.Write.Properties" },
            { "src/RackCad.Plugin/VentanaRackCad/RackCadCantileverDoorCommands.cs", @"public\s+void\s+InsertNewCantilever\s*\(", "RACKCANTILEVERNUEVO", "write.Properties" },
            { "src/RackCad.Plugin/VentanaRackCad/RackCadPushBackDoorCommands.cs", @"public\s+void\s+InsertNewPushBack\s*\(", "RACKPUSHBACKNUEVO", "write.Properties" },
        };

        [Theory]
        [MemberData(nameof(NewCommands))]
        public void EveryNuevoCommandWritesTheTabOfItsRequestAfterTheInsertionInTheSameCommand(string path, string signature, string command, string properties)
        {
            var code = WorkspaceHostSource.Code(path);
            Assert.Contains("[CommandMethod(\"" + command + "\")]", code, StringComparison.Ordinal);
            Assert.True(NewCommandViolations(WorkspaceHostSource.BodyOf(code, signature), properties).Count == 0,
                string.Join(" | ", NewCommandViolations(WorkspaceHostSource.BodyOf(code, signature), properties)));
        }

        // What a NUEVO command does wrong: it must insert first, then count what was placed, and answer through the properties of the door
        // with the properties of ITS request.
        private static System.Collections.Generic.List<string> NewCommandViolations(string body, string properties)
        {
            var violations = new System.Collections.Generic.List<string>();
            var dispatch = body.IndexOf("RackMenuCommands.DispatchInsertion(document,", StringComparison.Ordinal);
            var placed = body.IndexOf("var placed = RackCommandSupport.FindRackBlocks(document, pending.RackId);", StringComparison.Ordinal);
            var answer = body.IndexOf("door?.AnswerWrite(RackPropertiesDoorData.WithNewRackProperties(document, pending.RackId, " + properties + ",", StringComparison.Ordinal);
            if (dispatch < 0) violations.Add("no inserta con el despacho de «Nuevo»");
            if (placed < 0) violations.Add("no cuenta lo colocado");
            if (answer < 0) violations.Add("no responde con las propiedades de su petición (" + properties + ")");
            if (dispatch >= 0 && placed >= 0 && answer >= 0 && !(dispatch < placed && placed < answer)) violations.Add("las propiedades no van tras la inserción");
            if (WorkspaceHostSource.Count(body, @"\bWithNewRackProperties\b") != 1) violations.Add("no escribe las propiedades exactamente una vez");
            return violations;
        }

        [Fact]
        public void TheGuardOfTheNuevoCommandsSeesACommandThatForgetsTheProperties()
        {
            var code = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadPushBackDoorCommands.cs");
            var body = WorkspaceHostSource.BodyOf(code, @"public\s+void\s+InsertNewPushBack\s*\(");
            Assert.Empty(NewCommandViolations(body, "write.Properties"));

            var forgotten = Regex.Replace(body, @"RackPropertiesDoorData\.WithNewRackProperties\(document, pending\.RackId, write\.Properties,\s*", "(");
            Assert.NotEmpty(NewCommandViolations(forgotten, "write.Properties"));
            Assert.NotEmpty(NewCommandViolations(body.Replace("write.Properties,", "null,"), "write.Properties"));
        }

        [Fact]
        public void TheDoorKeepsThePropertiesOfANewDinamicoAndWritesThemOverTheRackJustInsertedThroughTheEdge()
        {
            // The request of a new Dinámico that waits keeps its tab (the other systems keep their whole request with «with»).
            var port = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs");
            var update = WorkspaceHostSource.BodyOf(port, @"public\s+RackCadClassicResult\s+RequestDynamicUpdate\s*\(");
            var pendingNew = update.Substring(update.IndexOf("_pendingNew = new PendingWrite(", StringComparison.Ordinal));
            pendingNew = pendingNew.Substring(0, pendingNew.IndexOf("() => _pendingNew = null", StringComparison.Ordinal));
            Assert.Contains("Properties: write.Properties", pendingNew, StringComparison.Ordinal);

            // The door data: the rule of Application decides over what to write, the edge writes, then the drawing is read and the rule answers.
            var body = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code(Door), @"internal\s+static\s+RackDesignWritten\s+WithNewRackProperties\s*\(");
            var steps = new[]
            {
                "if (answer == null || !answer.Written) return answer;",
                "CustomPropertiesExecutor.ReadRack(document, blocks[0].BlockId)",
                "RackPropertiesRules.NewRackFingerprint(read.Authority, out error)",
                "CustomPropertiesExecutor.ExecuteRackDraft(document, read.Selection, fingerprint, write.Entries, null)",
                "after = Read(document, placed[0].BlockId)",
                "return RackPropertiesRules.AnswerForNewRack(answer, write, error, after);",
            };
            var at = -1;
            foreach (var step in steps)
            {
                var next = body.IndexOf(step, at + 1, StringComparison.Ordinal);
                Assert.True(next > at, "out of order or missing: " + step);
                at = next;
            }

            Assert.Equal(1, WorkspaceHostSource.Count(body, @"\bExecuteRackDraft\b"));
            Assert.DoesNotContain("CustomPropertiesCommit", body, StringComparison.Ordinal);
        }
    }
}
