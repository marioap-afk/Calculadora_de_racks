using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3a — the CLASSIFIER of the conflicts of D-PB-05 (CT-PBT3A-09 and 10). The product classifier is compared with a
    /// mechanical oracle written again in this class (the leaf diff of the tests and the rules of the table of the package), by
    /// the set of (level, path, group, class, structural). Then a few anchors that do not depend on the derived leaves, and the
    /// coverage of the ownership map by reflection.
    /// </summary>
    public sealed class PushBackFullDraftClassifierTests
    {
        private static RackCatalog Catalog => PilotKit.Catalog;

        private static readonly PushBackEditorDesignAssembler Assembler = new PushBackEditorDesignAssembler(PilotKit.Catalog);
        private static readonly PushBackCompositeEditorAssembler CompositeAssembler = new PushBackCompositeEditorAssembler(PilotKit.Catalog);

        // ---------------------------------------------------------------------------------------------- helpers

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

        private static string EditInner(string envelopeText, Action<JsonNode> edit)
        {
            var envelope = JsonNode.Parse(envelopeText);
            var inner = JsonNode.Parse(envelope["Design"].GetValue<string>());
            edit(inner);
            envelope["Design"] = inner.ToJsonString();
            return envelope.ToJsonString();
        }

        private static PushBackFullDraftState Full(List<string> log, string text)
        {
            var result = Paso(log, () => PushBackFullDraftMaterializer.Materialize(text, Catalog));
            return result == null || !result.IsAdmitted ? null : result.State;
        }

        private static PushBackFullDraftState Copy(List<string> log, PushBackFullDraftState state)
            => state == null ? null : Paso(log, () => PushBackFullDraftStateComparer.Copy(state));

        // ---------------------------------------------------------------------------------------- the mechanical oracle

        /// <summary>The three texts the classifier compares for a state: the envelope (the name and the rest of the envelope of its
        /// origin, without <c>Design</c>), the project of its origin without <c>PushBack</c>, and the persisted <c>PushBack</c> object
        /// of the design built over a copy of the state. In the order of <see cref="PushBackDocumentLevel"/>.</summary>
        private static string[] Texts(PushBackFullDraftState state)
        {
            var copy = PushBackEditableStateComparer.Copy(state.Editable);
            var design = CompositeAssembler.BuildDesign(copy.Editor, copy.BuildInputs(Assembler));
            var payload = JsonNode.Parse(new RackProjectStore().Serialize(RackProject.ForPushBack(design)))["PushBack"].ToJsonString();

            var origin = JsonNode.Parse(state.Origin).AsObject();
            var envelope = new JsonObject { ["Name"] = state.Editable.Name == null ? null : JsonValue.Create(state.Editable.Name) };
            foreach (var property in origin)
            {
                if (property.Key == "Name" || property.Key == "Design") continue;
                envelope[property.Key] = property.Value?.DeepClone();
            }

            var inner = JsonNode.Parse(origin["Design"].GetValue<string>()).AsObject();
            var project = new JsonObject();
            foreach (var property in inner)
            {
                if (property.Key == "PushBack") continue;
                project[property.Key] = property.Value?.DeepClone();
            }

            return new[] { envelope.ToJsonString(), project.ToJsonString(), payload };
        }

        /// <summary>The leaves of a diff of the tests, by path, with the text before and after.</summary>
        private static Dictionary<string, (string Before, string After)> Leaves(string before, string after)
        {
            var result = new Dictionary<string, (string, string)>(StringComparer.Ordinal);
            foreach (var line in PilotKit.JsonDiff(before, after))
            {
                var colon = line.IndexOf(": ", StringComparison.Ordinal);
                var arrow = line.IndexOf(" -> ", colon, StringComparison.Ordinal);
                result[line.Substring(0, colon)] = (line.Substring(colon + 2, arrow - colon - 2), line.Substring(arrow + 4));
            }

            return result;
        }

        // The table of the package, written once more as data: the prefixes of the payload and their group.
        private static readonly (string Prefix, PushBackChangeGroup Group)[] PayloadPrefixes =
        {
            ("$.SchemaVersion", PushBackChangeGroup.OutsideEditor),
            ("$.Structure.Fronts", PushBackChangeGroup.Fronts), ("$.Fronts", PushBackChangeGroup.Fronts),
            ("$.Structure.Modules", PushBackChangeGroup.Modules), ("$.Structure.HeaderLineOverrides", PushBackChangeGroup.Modules),
            ("$.Structure.DerivedPostLineOverrides", PushBackChangeGroup.Modules),
            ("$.SideB", PushBackChangeGroup.SideB), ("$.Composite", PushBackChangeGroup.Composite),
            ("$.Structure.SafetySelections", PushBackChangeGroup.Safety), ("$.RearTopeSaque", PushBackChangeGroup.Safety),
            ("$.RearTopePieceId", PushBackChangeGroup.Safety), ("$.RearTopeOffCells", PushBackChangeGroup.Safety),
            ("$.DefensePieceId", PushBackChangeGroup.Safety),
            ("$.Structure.NumberFronts", PushBackChangeGroup.Annotations), ("$.Structure.NumberLevels", PushBackChangeGroup.Annotations),
            ("$.Structure.DrawRackName", PushBackChangeGroup.Annotations), ("$.Structure.AnnotationScale", PushBackChangeGroup.Annotations),
            ("$.Structure.Dimensions", PushBackChangeGroup.Annotations), ("$.Structure.DimensionViews", PushBackChangeGroup.Annotations),
            ("$.Structure.DimensionStyle", PushBackChangeGroup.Annotations),
            ("$.Structure", PushBackChangeGroup.Rack), ("$.LegacyHighEndBeamPeralte", PushBackChangeGroup.Rack),
        };

        private static bool Starts(string prefix, string path)
            => path == prefix || path.StartsWith(prefix + ".", StringComparison.Ordinal) || path.StartsWith(prefix + "[", StringComparison.Ordinal);

        private static PushBackChangeGroup OracleGroup(int level, string path)
        {
            if (level == (int)PushBackDocumentLevel.Envelope) return path == "$.Name" ? PushBackChangeGroup.Name : PushBackChangeGroup.OutsideEditor;
            if (level == (int)PushBackDocumentLevel.Project) return PushBackChangeGroup.OutsideEditor;
            var covering = PayloadPrefixes.Where(row => Starts(row.Prefix, path)).OrderByDescending(row => row.Prefix.Length).ToList();
            if (covering.Count > 0) return covering[0].Group;
            return PushBackEditableStateOwnership.Of(PushBackDocumentLevel.Payload, path) == PushBackFieldOwner.Editor
                ? PushBackChangeGroup.Rack
                : PushBackChangeGroup.OutsideEditor;
        }

        /// <summary>The set of (level, path, group, class, structural) the oracle gives for a base, the current rack and a draft.</summary>
        private static List<string> OracleSet(PushBackFullDraftState b, PushBackFullDraftState c, PushBackFullDraftState d)
        {
            var textsB = Texts(b);
            var textsC = Texts(c);
            var textsD = Texts(d);
            var result = new List<string>();
            for (var level = 0; level < 3; level++)
            {
                var external = Leaves(textsB[level], textsC[level]);
                var byUser = Leaves(textsB[level], textsD[level]);
                foreach (var path in external.Keys.Union(byUser.Keys, StringComparer.Ordinal))
                {
                    var inExternal = external.ContainsKey(path);
                    var inUser = byUser.ContainsKey(path);
                    var changeClass = inExternal && inUser ? PushBackChangeClass.Overlapping
                        : inExternal ? PushBackChangeClass.ChangedExternally : PushBackChangeClass.ChangedByUser;
                    var group = OracleGroup(level, path);
                    var absent = (inExternal && (external[path].Before == "<absent>" || external[path].After == "<absent>"))
                                 || (inUser && (byUser[path].Before == "<absent>" || byUser[path].After == "<absent>"));
                    var structural = group == PushBackChangeGroup.Modules || group == PushBackChangeGroup.SideB
                                     || group == PushBackChangeGroup.Composite || (group == PushBackChangeGroup.Fronts && absent);
                    result.Add(level + "|" + path + "|" + group + "|" + changeClass + "|" + structural);
                }
            }

            result.Sort(StringComparer.Ordinal);
            return result;
        }

        private static List<string> ProductSet(PushBackConflictClassification classification)
        {
            var result = classification.Entries
                .Select(entry => (int)entry.Level + "|" + entry.Path + "|" + entry.Group + "|" + entry.Class + "|" + entry.IsStructural)
                .ToList();
            result.Sort(StringComparer.Ordinal);
            return result;
        }

        private static bool Has(PushBackConflictClassification classification, PushBackDocumentLevel level, string path, PushBackChangeGroup group,
            PushBackChangeClass changeClass, bool structural)
            => classification.Entries.Any(entry => entry.Level == level && entry.Path == path && entry.Group == group
                                                   && entry.Class == changeClass && entry.IsStructural == structural);

        private static readonly IEnumerable<KeyValuePair<string, RackEmbedDocument>> NoMembers = new KeyValuePair<string, RackEmbedDocument>[0];

        /// <summary>The two lines every case of CT-09 and CT-10 gives with the oracle: the comparison and the number of entries.</summary>
        private static void Compare(
            List<string> log, List<string> expected, string label,
            PushBackFullDraftState b, PushBackFullDraftState c, PushBackFullDraftState d,
            IEnumerable<KeyValuePair<string, RackEmbedDocument>> members, out PushBackConflictClassification classification)
        {
            classification = b == null || c == null || d == null ? null : Paso(log, () => PushBackConflictClassifier.Classify(b, c, d, members, Catalog));
            var oracle = b == null || c == null || d == null ? null : Paso(log, () => OracleSet(b, c, d));
            var product = classification == null ? null : ProductSet(classification);
            log.Add(label + " igual-al-oraculo=" + (oracle != null && product != null && oracle.SequenceEqual(product, StringComparer.Ordinal))
                + " entradas=" + (classification == null ? -1 : classification.Entries.Count));
            expected.Add(label + " igual-al-oraculo=True entradas=" + (oracle == null ? -2 : oracle.Count));
        }

        // ------------------------------------------------------------------------------------------------- CT-09

        private static string AnchorLine(
            string label, PushBackConflictClassification classification, PushBackDocumentLevel level, string path, PushBackChangeGroup group,
            PushBackChangeClass changeClass)
            => label + " ancla " + level + " " + path + " grupo=" + group + " clase=" + changeClass + " estructural=False presente="
               + (classification != null && Has(classification, level, path, group, changeClass, false));

        [Fact]
        public void CT_PBT3A_09_TheClassifierGivesTheLeavesTheGroupsAndTheClassesOfTheOracle()
        {
            var log = new List<string>();
            var expected = new List<string>();
            var baseText = PilotKit.Text("pb-01-base");
            var b = Full(log, baseText);

            // The external changes come from the text of the drawing; the changes of the user, from the draft.
            string WithStructure(string name, double value) => EditInner(baseText, inner => inner["PushBack"]["Structure"][name] = value);

            // (c) the tolerance changed outside to 4.5.
            var c1 = Full(log, WithStructure("PalletTolerance", 4.5));
            Compare(log, expected, "tolerancia-externa", b, c1, Copy(log, b), NoMembers, out var k1);
            log.Add(AnchorLine("tolerancia-externa", k1, PushBackDocumentLevel.Payload, "$.Structure.PalletTolerance", PushBackChangeGroup.Rack, PushBackChangeClass.ChangedExternally));
            expected.Add("tolerancia-externa ancla Payload $.Structure.PalletTolerance grupo=Rack clase=ChangedExternally estructural=False presente=True");

            // (d) the depth of the pallet changed by the user to 48.5.
            var d2 = Copy(log, b);
            if (d2 != null) d2.Editable.Pallet = new PalletSpecification(d2.Editable.Pallet.Front, 48.5, d2.Editable.Pallet.Height, d2.Editable.Pallet.Weight, d2.Editable.Pallet.WeightUnit);
            Compare(log, expected, "fondo-usuario", b, Copy(log, b), d2, NoMembers, out var k2);
            log.Add(AnchorLine("fondo-usuario", k2, PushBackDocumentLevel.Payload, "$.Structure.PalletDepth", PushBackChangeGroup.Rack, PushBackChangeClass.ChangedByUser));
            expected.Add("fondo-usuario ancla Payload $.Structure.PalletDepth grupo=Rack clase=ChangedByUser estructural=False presente=True");

            // The same field changed by both, with the same value and with different values.
            var d3 = Copy(log, b);
            if (d3 != null) d3.Editable.PostPeralte = 3.25;
            Compare(log, expected, "postperalte-mismo-valor", b, Full(log, WithStructure("PostPeralte", 3.25)), d3, NoMembers, out var k3);
            log.Add(AnchorLine("postperalte-mismo-valor", k3, PushBackDocumentLevel.Payload, "$.Structure.PostPeralte", PushBackChangeGroup.Rack, PushBackChangeClass.Overlapping));
            expected.Add("postperalte-mismo-valor ancla Payload $.Structure.PostPeralte grupo=Rack clase=Overlapping estructural=False presente=True");

            var d4 = Copy(log, b);
            if (d4 != null) d4.Editable.PostPeralte = 4.25;
            Compare(log, expected, "postperalte-valores-distintos", b, Full(log, WithStructure("PostPeralte", 3.25)), d4, NoMembers, out var k4);
            log.Add(AnchorLine("postperalte-valores-distintos", k4, PushBackDocumentLevel.Payload, "$.Structure.PostPeralte", PushBackChangeGroup.Rack, PushBackChangeClass.Overlapping));
            expected.Add("postperalte-valores-distintos ancla Payload $.Structure.PostPeralte grupo=Rack clase=Overlapping estructural=False presente=True");

            // The name changed outside.
            Compare(log, expected, "nombre-externo", b, Full(log, IntentKit.Rewrite(baseText, e => e.Name = "PB-OTRO")), Copy(log, b), NoMembers, out var k5);
            log.Add(AnchorLine("nombre-externo", k5, PushBackDocumentLevel.Envelope, "$.Name", PushBackChangeGroup.Name, PushBackChangeClass.ChangedExternally));
            expected.Add("nombre-externo ancla Envelope $.Name grupo=Name clase=ChangedExternally estructural=False presente=True");

            // A custom property added outside: the first leaf of CustomProperties, by ordinal path.
            var withProperties = IntentKit.Rewrite(baseText, e => e.CustomProperties = JsonDocument.Parse("{\"Alfa\":1}").RootElement.Clone());
            Compare(log, expected, "propiedad-personalizada-externa", b, Full(log, withProperties), Copy(log, b), NoMembers, out var k6);
            var first = k6 == null ? null : k6.Entries.Where(entry => entry.Level == PushBackDocumentLevel.Envelope
                                                                      && entry.Path.StartsWith("$.CustomProperties", StringComparison.Ordinal))
                .OrderBy(entry => entry.Path, StringComparer.Ordinal).FirstOrDefault();
            log.Add("propiedad-personalizada-externa ancla Envelope " + (first == null ? "<ninguna>" : first.Path) + " grupo=" + (first == null ? "-" : first.Group.ToString())
                + " clase=" + (first == null ? "-" : first.Class.ToString()) + " estructural=" + (first != null && first.IsStructural) + " presente=" + (first != null));
            expected.Add("propiedad-personalizada-externa ancla Envelope $.CustomProperties.Alfa grupo=OutsideEditor clase=ChangedExternally estructural=False presente=True");

            // (3) The coverage of the ownership map (V42-03; A2-12 of PB-T2, M09): every declared property of the three documents has a row.
            foreach (var (level, type) in new[]
                     {
                         (PushBackDocumentLevel.Envelope, typeof(RackEmbedDocument)),
                         (PushBackDocumentLevel.Project, typeof(RackProjectDocument)),
                         (PushBackDocumentLevel.Payload, typeof(PushBackDesignDocument)),
                     })
            {
                var names = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(property => property.GetGetMethod() != null && property.GetCustomAttribute<JsonExtensionDataAttribute>() == null)
                    .Select(property => property.Name)
                    .OrderBy(name => name, StringComparer.Ordinal);
                foreach (var name in names)
                {
                    var path = "$." + name;
                    var row = PushBackEditableStateOwnership.Table
                        .Where(candidate => candidate.Level == level && Starts(candidate.Path, path))
                        .OrderByDescending(candidate => candidate.Path.Length)
                        .FirstOrDefault();
                    log.Add(level + " " + path + " fila=" + (row == null ? "<ninguna>" : row.Path) + " dueno=" + PushBackEditableStateOwnership.Of(level, path));
                }
            }

            expected.AddRange(new[]
            {
                "Envelope $.CustomProperties fila=$.CustomProperties dueno=Envelope",
                "Envelope $.Design fila=$.Design dueno=Container",
                "Envelope $.Id fila=$.Id dueno=Envelope",
                "Envelope $.Kind fila=$.Kind dueno=Envelope",
                "Envelope $.Name fila=$.Name dueno=Editor",
                "Envelope $.SchemaVersion fila=$.SchemaVersion dueno=Envelope",
                "Envelope $.Section fila=$.Section dueno=Envelope",
                "Envelope $.View fila=$.View dueno=Envelope",
                "Project $.Cantilever fila=$.Cantilever dueno=Envelope",
                "Project $.DynamicSystem fila=$.DynamicSystem dueno=Envelope",
                "Project $.FlowBed fila=$.FlowBed dueno=Envelope",
                "Project $.Header fila=$.Header dueno=Envelope",
                "Project $.Kind fila=$.Kind dueno=Envelope",
                "Project $.Larguero fila=$.Larguero dueno=Envelope",
                "Project $.PushBack fila=$.PushBack dueno=Container",
                "Project $.SchemaVersion fila=$.SchemaVersion dueno=Envelope",
                "Project $.SelectiveRack fila=$.SelectiveRack dueno=Envelope",
                "Payload $.Composite fila=$.Composite dueno=Editor",
                "Payload $.DefensePieceId fila=$.DefensePieceId dueno=Editor",
                "Payload $.Fronts fila=$.Fronts dueno=Editor",
                "Payload $.LegacyHighEndBeamPeralte fila=$.LegacyHighEndBeamPeralte dueno=Editor",
                "Payload $.RearTopeOffCells fila=$.RearTopeOffCells dueno=Editor",
                "Payload $.RearTopePieceId fila=$.RearTopePieceId dueno=Editor",
                "Payload $.RearTopeSaque fila=$.RearTopeSaque dueno=Editor",
                "Payload $.SchemaVersion fila=$.SchemaVersion dueno=Envelope",
                "Payload $.SideB fila=$.SideB dueno=Editor",
                "Payload $.Structure fila=$.Structure dueno=Editor",
            });

            Assert.Equal(expected, log);
        }

        // ------------------------------------------------------------------------------------------------- CT-10

        private static KeyValuePair<string, RackEmbedDocument> Member(string key, string view, int section)
            => new KeyValuePair<string, RackEmbedDocument>(key, new RackEmbedDocument { Kind = RackEmbedDocument.KindPushBack, View = view, Section = section });

        [Fact]
        public void CT_PBT3A_10_TheClassifierMarksTheStructuralChangesAndTheViewsToErase()
        {
            var log = new List<string>();
            var expected = new List<string>();
            var baseText = PilotKit.Text("pb-01-base");
            var b = Full(log, baseText);
            var count = b == null ? 0 : b.Editable.Editor.SlotCount;

            // A front added by the user: a structural entry of the fronts, by the user, that was absent in the base.
            var d1 = Copy(log, b);
            if (d1 != null) d1.Editable.Editor.SetSlotCount(count + 1);
            Compare(log, expected, "+frente", b, Copy(log, b), d1, NoMembers, out var k1);
            log.Add("+frente frentes-estructural-usuario-ausente-en-base="
                + (k1 != null && k1.Entries.Any(entry => entry.Group == PushBackChangeGroup.Fronts && entry.IsStructural
                                                         && entry.Class == PushBackChangeClass.ChangedByUser && entry.BaseValue == "<ausente>")));
            expected.Add("+frente frentes-estructural-usuario-ausente-en-base=True");

            // The length of a module changed outside, over a design that keeps the lengths of its modules.
            var modulesText = PilotKit.Text("pb-02-modulos");
            var modulesBase = Full(log, modulesText);
            var changed = Full(log, EditInner(modulesText, inner => inner["PushBack"]["Structure"]["Modules"][1]["Length"] = 32));
            Compare(log, expected, "modulo-externo", modulesBase, changed, Copy(log, modulesBase), NoMembers, out var k2);
            log.Add("modulo-externo modulos-estructural-externo="
                + (k2 != null && k2.Entries.Any(entry => entry.Group == PushBackChangeGroup.Modules && entry.IsStructural && entry.Class == PushBackChangeClass.ChangedExternally)));
            expected.Add("modulo-externo modulos-estructural-externo=True");

            // Side B declared by the user with one front (the gestures of CT-PB0-10).
            var d3 = Copy(log, b);
            if (d3 != null)
            {
                d3.Editable.Editor.SetSlotPresent(PushBackSide.A, 1, false);
                d3.Editable.Editor.SetSlotPresent(PushBackSide.A, 2, false);
                d3.Editable.Editor.SetSideBPresent(true);
                d3.Editable.Editor.SetSlotPresent(PushBackSide.B, 1, true);
            }

            Compare(log, expected, "lado-B-usuario", b, Copy(log, b), d3, NoMembers, out var k3);
            log.Add("lado-B-usuario ladoB-estructural-usuario="
                + (k3 != null && k3.Entries.Any(entry => entry.Group == PushBackChangeGroup.SideB && entry.IsStructural && entry.Class == PushBackChangeClass.ChangedByUser)));
            expected.Add("lado-B-usuario ladoB-estructural-usuario=True");

            // A front removed by the user, with the lateral of the last post among the current members.
            var d4 = Copy(log, b);
            if (d4 != null) d4.Editable.Editor.SetSlotCount(count - 1);
            var members = new[] { Member("L3", RackEmbedDocument.ViewLateral, 3), Member("P", RackEmbedDocument.ViewPlanta, -1) };
            Compare(log, expected, "-frente", b, Copy(log, b), d4, members, out var k4);
            log.Add("-frente ViewsToErase=[" + (k4 == null ? "<sin resultado>" : string.Join(",", k4.ViewsToErase)) + "] ViewsRejected=["
                + (k4 == null ? "<sin resultado>" : string.Join(",", k4.ViewsRejected)) + "] AllViewsObsolete=" + (k4 != null && k4.AllViewsObsolete));
            expected.Add("-frente ViewsToErase=[L3] ViewsRejected=[] AllViewsObsolete=False");

            Assert.Equal(expected, log);
        }
    }
}
