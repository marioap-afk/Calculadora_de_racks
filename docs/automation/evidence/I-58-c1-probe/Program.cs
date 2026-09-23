using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Editor;
using RackCad.UI.Systems.Cantilever;

internal static class Program
{
    private static int selected;
    private static readonly RackProjectStore Projects = new();
    private static readonly RackEmbedStore Envelopes = new();
    [STAThread]
    private static int Main()
    {
        try
        {
            VerifyExtraction();
            var a = Insert("A-new", "10000000-0000-0000-0000-000000000001", null, null, false);
            Report("A-new", a, Sibling(a), false);
            var library = Projects.Deserialize(a.Design);
            var b = Insert("B-library", "20000000-0000-0000-0000-000000000002", library, null, false);
            Report("B-library", b, Sibling(b), false);
            Require(Inner(a) == Inner(b) && a.Id != b.Id, "library retains inner across independent outer racks");
            var c = Insert("C-reopen", null, Projects.Deserialize(b.Design), b, true);
            Report("C-reopen-new-sibling", b, c, false);
            var newId = Guid.Parse("40000000-0000-0000-0000-000000000004");
            var d1 = ExtractedPlugin.RestampEnvelope(Envelopes.Serialize(b), "Copy", newId);
            var d2 = ExtractedPlugin.RestampEnvelope(Envelopes.Serialize(c), "Copy", newId);
            Require(d1.IsSuccess && d2.IsSuccess, "reconstructed restamp succeeds");
            Report("D-restamp-reconstructed", Envelopes.Deserialize(d1.DesignJson), Envelopes.Deserialize(d2.DesignJson), true);
            Console.WriteLine($"SELECTED={selected}; FAILED=0; comparator implemented/executed=NO; AutoCAD executed=NO");
            return selected == 4 ? 0 : 1;
        }
        catch (Exception e) { Console.WriteLine(e); return 1; }
    }
    private static RackEmbedDocument Insert(string label, string outer, RackProject source, RackEmbedDocument existing, bool reopen)
    {
        var w = new RackCantileverWindow(true, () => outer ?? throw new InvalidOperationException("reopen minted identity"));
        if (reopen) w.LoadExisting(source.CantileverLineDesign, existing.Id, existing.Name, source);
        else if (source != null) w.LoadDesignForNew(source.CantileverLineDesign, "Fixture", source);
        // Reflection only accesses the existing diagnostic seam; never equality or product comparison.
        var design = (CantileverLineDesign)typeof(RackCantileverWindow).GetProperty("Design", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(w);
        var innerBefore = design.Id;
        if (source == null)
        {
            var template = design.StationTopology.ColumnBaseTemplate;
            template.ColumnSectionId = "AISC-W-W10X33";
            template.Base = new CantileverBaseDesign { SectionId = "AISC-W-W12X26", Length = 48 };
            design.DefaultArmTemplate = new CantileverArmTemplateDesign {
                Body = new CantileverArmBodyDesign { SectionId = "AISC-HSS-RECT-HSS4X4X_250", CutLength = 36 },
                MountingPlate = new CantileverArmMountingPlateTemplateDesign { VerticalPunchCount = 2, VerticalEndOffset = 1.5 }
            };
        }
        ((TextBox)w.FindName("NameBox")).Text = "Fixture";
        var button = (Button)w.FindName(reopen ? "InsertPlantaButton" : "InsertFrontalButton");
        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        Require(w.InsertionRequest is CantileverInsertionRequest, label + " real WPF insertion request absent");
        var request = (CantileverInsertionRequest)w.InsertionRequest;
        Require(request.Design.Id == innerBefore, label + " editor changed inner");
        Console.WriteLine($"{label}: real WPF request outer={request.RackId}; inner={request.Design.Id}; innerPreserved=True");
        // Actual stores and composer, exact extracted command method; no DWG insertion.
        return Envelopes.Deserialize(ExtractedPlugin.BuildCantileverPayload(request.Design, request.RackId,
            request.RackName, request.View, request.Section, existing, source));
    }
    private static RackEmbedDocument Sibling(RackEmbedDocument first)
        => Envelopes.Deserialize(ExtractedPlugin.BuildCantileverPayload(Projects.Deserialize(first.Design).CantileverLineDesign,
            first.Id, first.Name, RackEmbedDocument.ViewPlanta, -1, first, Projects.Deserialize(first.Design)));
    private static Guid Inner(RackEmbedDocument e) => Projects.Deserialize(e.Design).CantileverLineDesign.Id;
    private static void Report(string label, RackEmbedDocument first, RackEmbedDocument sibling, bool equal)
    {
        selected++;
        var inner = Inner(first);
        Require(inner != Guid.Empty, label + " inner empty");
        Require(first.Id == sibling.Id && inner == Inner(sibling), label + " sibling identity changed");
        Require((Guid.Parse(first.Id) == inner) == equal, label + " unexpected outer/inner relation");
        var calls = 0;
        var assembler = new CantileverLineEditorAssembler(new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load());
        var authored = Projects.Deserialize(first.Design).CantileverLineDesign;
        var port = RackResolvePorts.Cantilever<CantileverLineDesign, CantileverLineEditorComputation>(d => {
            calls++; Require(d.Id == inner, "AUTH09 changed inner"); return assembler.Build(d);
        }, c => c.IsValid ? null : "blocked");
        var result = port.Resolve(authored);
        Require(result.IsSuccess && calls == 1 && result.Resolved.Design.Id == inner, label + " AUTH09 seam failure");
        Console.WriteLine($"{label}: envelope={first.Id}; Line.Id={inner}; outerEqualsInner={equal}; siblingOuterEqual=True; siblingInnerEqual=True; AUTH09input={nameof(CantileverLineDesign)}:{inner}; assemblerCalls={calls}");
        Console.WriteLine("AUTH10: source-inspected only; typed resolved input, no envelope Id parameter. I55 membership: source-inspected only; outer RackId/ProbeId, not inner. Future AUTH13 outcome NOT MEASURED.");
    }
    private static void VerifyExtraction()
    {
        var extracted = File.ReadAllText("docs/automation/evidence/I-58-c1-probe/ExtractedPlugin.cs").Replace("\r\n", "\n");
        foreach (var pair in new[] {
            ("src/RackCad.Plugin/RackCantileverCommands.cs", "        internal static string BuildCantileverPayload("),
            ("src/RackCad.Plugin/KindHandlers/CantileverKindHandler.cs", "        public RestampResult RestampDesign("),
            ("src/RackCad.Plugin/RackEnvelopeRestamp.cs", "        public static RestampResult RestampEnvelope(string payload, string copyName, Guid newId)") })
        {
            var source = File.ReadAllText(pair.Item1).Replace("\r\n", "\n");
            var start = source.IndexOf(pair.Item2, StringComparison.Ordinal);
            Require(start >= 0, "source method missing");
            var end = source.IndexOf('{', start) + 1; var depth = 1;
            while (depth > 0) { if (source[end] == '{') depth++; else if (source[end] == '}') depth--; end++; }
            Require(extracted.Contains(source[start..end], StringComparison.Ordinal), "extraction drift: " + pair.Item1);
        }
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
