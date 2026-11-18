using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1b — the LOADS of the editable state of Push Back (CT-PBT1B-02 to 05 and 17): the round trip without
    /// losses, the explicit rule of c-02 (the post), the exclusion of the parked tail, the rest of the catalog
    /// dependencies of CT-PB0-17 and the explicit rule of c-09 (the weight unit). The oracle of each one is the window of
    /// today, measured by the earlier tasks of the unit; nothing here opens a window.
    /// </summary>
    public sealed class PushBackEditableStateLoadTests
    {
        private const string P = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";

        private static RackCatalog Catalog => PilotKit.Catalog;

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

        private static PushBackDesign DesignOfText(string envelope)
            => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(envelope).Design).PushBackDesign;

        private static string NameOfText(string envelope) => new RackEmbedStore().Deserialize(envelope).Name;

        private static string Persist(PushBackDesign design) => PilotKit.Persist(design);

        private static string EditInner(string envelopeText, Action<JsonNode> edit)
        {
            var envelope = JsonNode.Parse(envelopeText);
            var inner = JsonNode.Parse(envelope["Design"].GetValue<string>());
            edit(inner);
            envelope["Design"] = inner.ToJsonString();
            return envelope.ToJsonString();
        }

        private static JsonNode StructureNode(JsonNode inner) => inner["PushBack"]["Structure"];

        /// <summary>A recompute pass as the window runs it (W:659-704), adapted to the state.</summary>
        private static PushBackEditorComputation Pasada(
            PushBackEditableState s, PushBackEditorDesignAssembler a, PushBackCompositeEditorAssembler ca, PushBackSide frontalSide)
        {
            PushBackEditorComputation computation;
            try { computation = a.BuildFrom(ca.BuildDesign(s.Editor, s.BuildInputs(a)), frontalSide); }
            catch (Exception) { return null; }
            if (!computation.IsValid) return computation;
            a.AcceptComputation(s.Editor.SideA, computation);
            s.Editor.CommitDormantTailConsumption();
            var standardRestoreLanded = s.Editor.SideA.ModuleCommit?.StandardRestoreRequested == true;
            s.Editor.SideA.ClearModuleCommit();
            if (standardRestoreLanded) s.Editor.Active.RestoreAdvancedRackParameters(s.Advanced);
            _ = s.Editor.SideA.ModuleSession;
            return computation;
        }

        private static string Leer(string json, string path)
        {
            var node = JsonNode.Parse(json)["PushBack"];
            foreach (var segment in path.Split('.')) node = node?[segment];
            return node == null ? "<null>" : node.ToJsonString();
        }

        // The delta of two persisted documents, as the window tests read it: «path: before -> after», sorted ordinally.
        private static List<string> Delta(string x, string y) => PilotKit.JsonDiff(x, y).OrderBy(line => line, StringComparer.Ordinal).ToList();

        private static double R3(double value)
            => double.Parse(value.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

        /// <summary>The class of a delta line: «a» (rounding to «0.###» of the controls), «b» (the dimension style) or none.</summary>
        private static string ClassOf(string line)
        {
            if (line.StartsWith("$.PushBack.Structure.DimensionStyle: ", StringComparison.Ordinal)) return "b";
            var colon = line.IndexOf(": ", StringComparison.Ordinal);
            var arrow = line.IndexOf(" -> ", StringComparison.Ordinal);
            if (colon > 0 && arrow > colon
                && double.TryParse(line.Substring(colon + 2, arrow - colon - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                && double.TryParse(line.Substring(arrow + 4), NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
                && x != y
                && y == R3(x))
            {
                return "a";
            }

            return string.Empty;
        }

        private static string Sha(IEnumerable<string> lines)
        {
            using (var sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", lines))).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        /// <summary>The state loaded with a design and one recompute pass: the pair (state, computation).</summary>
        private static (PushBackEditableState State, PushBackEditorComputation Computation) Loaded(
            List<string> log, PushBackDesign design, string name)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var composite = new PushBackCompositeEditorAssembler(Catalog);
            var state = new PushBackEditableState();
            Paso(log, () => state.LoadDesign(design, name, assembler.Resolver));
            var computation = Pasada(state, assembler, composite, PushBackSide.A);
            return (state, computation);
        }

        private static IEnumerable<string> FixtureNames()
        {
            var dir = System.IO.Path.Combine(PilotKit.RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64");
            return System.IO.Directory.GetFiles(dir, "pb-*.json")
                .Select(System.IO.Path.GetFileNameWithoutExtension)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>X-01, Core recipe (§5.0): pb-21-cola, side B declared, side B selected and its slot 0 present.</summary>
        private static PushBackDesign BuildX01(List<string> log)
        {
            var text = PilotKit.Text("pb-21-cola");
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var composite = new PushBackCompositeEditorAssembler(Catalog);
            var state = new PushBackEditableState();
            Paso(log, () => state.LoadDesign(DesignOfText(text), NameOfText(text), assembler.Resolver));
            Pasada(state, assembler, composite, PushBackSide.A);
            Paso(log, () => { state.Editor.SetSideBPresent(true); return 0; });
            Paso(log, () => { state.Editor.SetActiveSelection(PushBackSideSelection.B); return 0; });
            Paso(log, () => state.Editor.SetSlotPresent(PushBackSide.B, 0, true));
            var computation = Pasada(state, assembler, composite, PushBackSide.A);
            return computation == null ? null : new RackProjectStore().Deserialize(Persist(computation.Design)).PushBackDesign;
        }

        /// <summary>F-01, Core recipe (§5.0): pb-20-ab with the first separator of A and then the first one of B lengthened by 2.</summary>
        private static PushBackDesign BuildF01(List<string> log)
        {
            var text = PilotKit.Text("pb-20-ab");
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var composite = new PushBackCompositeEditorAssembler(Catalog);
            var state = new PushBackEditableState();
            Paso(log, () => state.LoadDesign(DesignOfText(text), NameOfText(text), assembler.Resolver));
            Pasada(state, assembler, composite, PushBackSide.A);
            PushBackEditorComputation computation = null;
            foreach (var inB in new[] { false, true })
            {
                var separator = Paso(log, () => state.Editor.SideA.ModuleSession.Modules.First(
                    m => m.Kind == DynamicRackModuleKind.Separator && m.ModuleId.StartsWith("B:", StringComparison.Ordinal) == inB));
                if (separator == null) return null;
                Paso(log, () => state.Editor.SideA.ModuleSession.SetLength(separator.ModuleId, separator.Length + 2.0));
                Paso(log, () => state.Editor.SideA.CommitModuleEdits());
                computation = Pasada(state, assembler, composite, PushBackSide.A);
            }

            return computation == null ? null : new RackProjectStore().Deserialize(Persist(computation.Design)).PushBackDesign;
        }

        // The exact canonicalization an older drawing gets when it is written (the delta of the legacy corpus, sorted).
        private static readonly IReadOnlyDictionary<string, string[]> LegacyDeltas = new Dictionary<string, string[]>
        {
            ["pb-10-sin-datum"] = new[] { "$.PushBack.Structure.FirstLevelDatum: <absent> -> 1" },
            ["pb-11-sin-i41"] = new[]
            {
                "$.PushBack.Fronts[0].DefaultPalletsDeep: <absent> -> 8", "$.PushBack.Fronts[0].DrawPallets: <absent> -> null", "$.PushBack.Fronts[0].PalletsDeepOverrides: <absent> -> null",
                "$.PushBack.Fronts[1].DefaultPalletsDeep: <absent> -> 8", "$.PushBack.Fronts[1].DrawPallets: <absent> -> null", "$.PushBack.Fronts[1].PalletsDeepOverrides: <absent> -> null",
                "$.PushBack.Fronts[2].DefaultPalletsDeep: <absent> -> 8", "$.PushBack.Fronts[2].DrawPallets: <absent> -> null", "$.PushBack.Fronts[2].PalletsDeepOverrides: <absent> -> null",
            },
            ["pb-12-poste-nulo"] = new[] { "$.PushBack.Structure.HeaderPostCatalogId: null -> \"POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA\"" },
            ["pb-13-sideb-no-presente"] = new[] { "$.PushBack.SideB.IsPresent: false -> <absent>" },
            ["pb-14-peralte-heredado"] = new[]
            {
                "$.PushBack.Fronts[0].HighEndBeamPeraltes[0]: null -> 5", "$.PushBack.Fronts[0].HighEndBeamPeraltes[1]: null -> 5", "$.PushBack.Fronts[0].HighEndBeamPeraltes[2]: null -> 5",
                "$.PushBack.Fronts[1].HighEndBeamPeraltes[0]: null -> 5", "$.PushBack.Fronts[1].HighEndBeamPeraltes[1]: null -> 5", "$.PushBack.Fronts[1].HighEndBeamPeraltes[2]: null -> 5",
                "$.PushBack.Fronts[2].HighEndBeamPeraltes[0]: null -> 5", "$.PushBack.Fronts[2].HighEndBeamPeraltes[1]: null -> 5", "$.PushBack.Fronts[2].HighEndBeamPeraltes[2]: null -> 5",
                "$.PushBack.LegacyHighEndBeamPeralte: 5 -> 3.5",
            },
            ["pb-15-sin-composite"] = new[]
            {
                "$.PushBack.Composite.AbsentSlotsA: <absent> -> null", "$.PushBack.Composite.AbsentSlotsB[0]: <absent> -> 0", "$.PushBack.Composite.CentralSeparator: <absent> -> null",
                "$.PushBack.Composite.DefaultDirection: <absent> -> \"AToB\"", "$.PushBack.Composite.DefaultTopology: <absent> -> \"SoloA\"", "$.PushBack.Composite.Gap: <absent> -> null",
                "$.PushBack.Composite.StructureOverrideA: <absent> -> null", "$.PushBack.Composite.StructureOverrideB: <absent> -> null", "$.PushBack.Composite.Topologies: <absent> -> null",
            },
        };

        // ------------------------------------------------------------------------------------------------- CT-02

        [Fact]
        public void CT_PBT1B_02_TheRoundTripOfEverySupportedRackLosesNothing()
        {
            var log = new List<string>();
            var expected = new List<string>();

            // The source of each design: the INNER document of the envelope as it is written (so a legacy drawing shows its
            // canonicalization), and for the two recipes the persisted design they produce.
            var designs = new List<(string Name, PushBackDesign Design, string RackName, string Source)>();
            foreach (var name in FixtureNames().Where(n => n != "pb-21-cola"))
            {
                var text = PilotKit.Text(name);
                designs.Add((name, DesignOfText(text), NameOfText(text), PilotKit.DesignOf(text)));
            }

            var x01 = BuildX01(log);
            var f01 = BuildF01(log);
            designs.Add(("X-01", x01, "X-01", x01 == null ? null : Persist(x01)));
            designs.Add(("F-01", f01, "F-01", f01 == null ? null : Persist(f01)));

            foreach (var entry in designs)
            {
                var legacyLines = LegacyDeltas.TryGetValue(entry.Name, out var legacy) ? legacy.OrderBy(l => l, StringComparer.Ordinal).ToArray() : Array.Empty<string>();
                expected.Add(entry.Name + " diseno-estado-diseno delta=[" + string.Join(" ;; ", legacyLines) + "]");

                // The known normalization (v2.7, F4B-E20): the design without a Composite block loads front 0 of side B as
                // active, and its output declares that slot absent, so the second load has it inactive; nothing else.
                var isNormalized = entry.Name == "pb-15-sin-composite";
                expected.Add(entry.Name + " primera-vuelta-estado iguales=" + (isNormalized ? "False" : "True")
                    + (isNormalized ? " SideB.Fronts[0].IsActive=True->False normalizada-iguales=True" : string.Empty));
                expected.Add(entry.Name + " estado-diseno-estado iguales=True");

                if (entry.Design == null)
                {
                    log.Add(entry.Name + " sin diseno de receta");
                    continue;
                }

                // S1: the state loaded from D with its pass; D1: the design of that pass.
                var s1 = Loaded(log, entry.Design, entry.RackName);
                var delta = s1.Computation == null ? new List<string> { "sin computacion" } : Delta(entry.Source, Persist(s1.Computation.Design));
                log.Add(entry.Name + " diseno-estado-diseno delta=[" + string.Join(" ;; ", delta) + "]");

                // S2: a new state loaded from D1 with its pass; D2: the design of that pass.
                var s2 = s1.Computation == null ? default : Loaded(log, s1.Computation.Design, entry.RackName);
                var firstEqual = s2.State != null && PushBackEditableStateComparer.Instance.Equals(s1.State, s2.State);
                var firstLine = entry.Name + " primera-vuelta-estado iguales=" + firstEqual;
                if (isNormalized)
                {
                    var before = Paso(log, () => s1.State.Editor.SideB.Structure.IsActive(0));
                    var after = Paso(log, () => s2.State.Editor.SideB.Structure.IsActive(0));
                    Paso(log, () => s1.State.Editor.SetSlotPresent(PushBackSide.B, 0, false));
                    var normalizedEqual = s2.State != null && PushBackEditableStateComparer.Instance.Equals(s1.State, s2.State);
                    firstLine += " SideB.Fronts[0].IsActive=" + before + "->" + after + " normalizada-iguales=" + normalizedEqual;
                }

                log.Add(firstLine);

                // S3: a new state loaded from D2 with its pass; S2 against S3, from the state already normalized.
                var s3 = s2.Computation == null ? default : Loaded(log, s2.Computation.Design, entry.RackName);
                var secondEqual = s3.State != null && PushBackEditableStateComparer.Instance.Equals(s2.State, s3.State);
                log.Add(entry.Name + " estado-diseno-estado iguales=" + secondEqual);
            }

            Assert.Equal(expected, log);
        }

        // ------------------------------------------------------------------------------------------------- CT-03

        private static string Rule(RackCatalog catalog, string requested)
            => "regla [" + (requested ?? "<null>") + "] -> [" + PushBackEditableState.ResolvePostCatalogId(catalog, requested) + "]";

        [Fact]
        public void CT_PBT1B_03_TheRuleOfTheCatalogPostIsExplicitAndOrdinal()
        {
            var log = new List<string>();

            // The branch the step 1 measured (row b: lowercase gives PostBox.SelectedId null): the ORDINAL one.
            var requested = new string[] { P, P.ToLowerInvariant(), "PB0_POSTE_INEXISTENTE", string.Empty, "   ", null };
            foreach (var value in requested)
            {
                log.Add("catalogo " + Rule(Catalog, value));
            }

            var fabricated = new RackCatalog();
            fabricated.Defaults.Post = P;
            fabricated.PostProfiles = new List<ProfileCatalogEntry>
            {
                new ProfileCatalogEntry { Id = " PB1_X " },
                new ProfileCatalogEntry { Id = "PB1_Y" },
                new ProfileCatalogEntry { Id = "pb1_y" },
            };
            foreach (var value in new[] { "PB1_X", " PB1_X ", "PB1_Y", "pb1_y" })
            {
                log.Add("fabricado " + Rule(fabricated, value));
            }

            // The output of the state for c-01 (blank id) and c-02 (an id the catalog does not have).
            foreach (var (label, value) in new[] { ("c-01", string.Empty), ("c-02", "PB0_POSTE_INEXISTENTE") })
            {
                var text = EditInner(PilotKit.Text("pb-01-base"), inner => StructureNode(inner)["HeaderPostCatalogId"] = value);
                var loaded = Loaded(log, DesignOfText(text), NameOfText(text));
                log.Add(label + " HeaderPostCatalogId=" + (loaded.Computation == null ? "<sin computacion>" : loaded.Computation.Design.Structure.HeaderPostCatalogId));
            }

            Assert.Equal(
                new[]
                {
                    "catalogo regla [" + P + "] -> [" + P + "]",
                    "catalogo regla [" + P.ToLowerInvariant() + "] -> [" + P + "]",
                    "catalogo regla [PB0_POSTE_INEXISTENTE] -> [" + P + "]",
                    "catalogo regla [] -> [" + P + "]",
                    "catalogo regla [   ] -> [" + P + "]",
                    "catalogo regla [<null>] -> [" + P + "]",
                    "fabricado regla [PB1_X] -> [PB1_X]",
                    "fabricado regla [ PB1_X ] -> [" + P + "]",
                    "fabricado regla [PB1_Y] -> [PB1_Y]",
                    "fabricado regla [pb1_y] -> [" + P + "]",
                    "c-01 HeaderPostCatalogId=" + P,
                    "c-02 HeaderPostCatalogId=" + P,
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-04

        [Fact]
        public void CT_PBT1B_04_OnlyTheParkedTailOfPb21ColaIsOutsideTheGuarantee()
        {
            var log = new List<string>();
            var names = FixtureNames().ToList();
            foreach (var name in names)
            {
                log.Add(name + " soporte=" + PushBackEditableState.SupportOf(DesignOfText(PilotKit.Text(name))));
            }

            var text = PilotKit.Text("pb-21-cola");
            var loaded = Loaded(log, DesignOfText(text), NameOfText(text));
            var lost = loaded.Computation == null
                ? new List<string> { "sin computacion" }
                : Delta(Persist(DesignOfText(text)), Persist(loaded.Computation.Design)).Where(line => ClassOf(line).Length == 0).ToList();
            log.Add("pb-21-cola delta-filtrado=" + lost.Count + " sha256=" + Sha(lost));

            var pilot = PushBackPilotMaterializer.Materialize(text, Catalog);
            log.Add("pb-21-cola piloto=" + pilot.Kind + " razon=" + pilot.Reason);

            var expected = names
                .Select(name => name + " soporte=" + (name == "pb-21-cola" ? PushBackEditableSupport.DormantCompositeTail : PushBackEditableSupport.Supported))
                .ToList();
            expected.Add("pb-21-cola delta-filtrado=667 sha256=0b54e8ff5c8ad08d262bdfd8e30db37eb0dd40b19797940f8a5fd85b04eed875");
            expected.Add("pb-21-cola piloto=" + PushBackPilotMaterializationKind.OutOfPilot + " razon=" + PushBackPilotMaterializer.DormantTailOutOfPilotReason);
            Assert.Equal(expected, log);
        }

        // ------------------------------------------------------------------------------------------------- CT-05

        private const string MissingBeam = "PB0_VIGA_INEXISTENTE";

        private static JsonNode Level(JsonNode inner, int front) => StructureNode(inner)["Fronts"][front]["Levels"][0];

        private static double TooDeep(JsonNode inner, int front)
        {
            var beamId = Level(inner, front)["InOutBeamCatalogId"].GetValue<string>();
            return DynamicRackLevelGeometry.AllowedPeraltes(Catalog, beamId).Max() + 0.5;
        }

        private static IEnumerable<KeyValuePair<string, Action<JsonNode>>> Cases()
        {
            yield return new KeyValuePair<string, Action<JsonNode>>("c-01 HeaderPostCatalogId vacio", inner => StructureNode(inner)["HeaderPostCatalogId"] = string.Empty);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-03 HighEndBeamPeraltes[0]=7.25", inner => inner["PushBack"]["Fronts"][0]["HighEndBeamPeraltes"][0] = 7.25);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-04 InOutBeamCatalogId inexistente", inner => Level(inner, 0)["InOutBeamCatalogId"] = MissingBeam);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-05 InOutBeamDepth fuera del maximo", inner => Level(inner, 0)["InOutBeamDepth"] = TooDeep(inner, 0));
            yield return new KeyValuePair<string, Action<JsonNode>>("c-06 IntermediateBeamCatalogId inexistente", inner => Level(inner, 0)["IntermediateBeamCatalogId"] = MissingBeam);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-07 InOutBeamDepth fuera del maximo en el frente 1", inner => Level(inner, 1)["InOutBeamDepth"] = TooDeep(inner, 1));
            yield return new KeyValuePair<string, Action<JsonNode>>("c-08 PalletWeightUnit vacia", inner => StructureNode(inner)["PalletWeightUnit"] = string.Empty);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-09 PalletWeightUnit ton", inner => StructureNode(inner)["PalletWeightUnit"] = "ton");
            yield return new KeyValuePair<string, Action<JsonNode>>("c-10 Dimensions 7", inner => StructureNode(inner)["Dimensions"] = 7);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-11 Dimensions -1", inner => StructureNode(inner)["Dimensions"] = -1);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-12 PalletsDeep 1", inner => StructureNode(inner)["PalletsDeep"] = 1);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-13 PalletTolerance 0", inner => StructureNode(inner)["PalletTolerance"] = 0);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-14 BeamDepth 0", inner => StructureNode(inner)["BeamDepth"] = 0);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-15 AnnotationScale 0", inner => StructureNode(inner)["AnnotationScale"] = 0);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-16 Dimensions 3 limite valido", inner => StructureNode(inner)["Dimensions"] = 3);
        }

        private static string LabelOf(string line)
        {
            if (line.StartsWith("$.PushBack.Structure.HeaderPostCatalogId: \"\"", StringComparison.Ordinal))
                return "CARGA-APPLICATION:poste-vacio-a-defecto DynamicRackSystemResolver.cs:64-66";
            if (line.StartsWith("$.PushBack.Fronts[0].HighEndBeamPeraltes[0]: ", StringComparison.Ordinal))
                return "CARGA-APPLICATION:peralte-posterior-normalizado PushBackEditorDesignAssembler.cs:155; PushBackEditorState.cs:259-269";
            if (line.EndsWith("InOutBeamCatalogId: \"PB0_VIGA_INEXISTENTE\" -> \"LARGUERO_IN_OUT_C6\"", StringComparison.Ordinal)
                || line.EndsWith("IntermediateBeamCatalogId: \"PB0_VIGA_INEXISTENTE\" -> \"LARGUERO_ESCALON_INFINITO\"", StringComparison.Ordinal))
                return "CARGA-APPLICATION:viga-inexistente-a-defecto DynamicRackLevelGeometry.cs:41,48,200-204";
            if (line.Contains(".InOutBeamDepth: "))
                return "CARGA-APPLICATION:peralte-fuera-de-catalogo DynamicRackLevelGeometry.cs:42-45; DynamicLoadBeamGeometry.cs:52";
            if (line.StartsWith("$.PushBack.Structure.BeamDepth: ", StringComparison.Ordinal))
                return "CARGA-APPLICATION:peralte-de-larguero-0-a-defecto PushBackEditorState.Load.cs:200";
            return "SIN-ETIQUETA";
        }

        [Fact]
        public void CT_PBT1B_05_TheOtherLossesOfTheCatalogDependenciesAreReproducedAsTheyAre()
        {
            var log = new List<string>();
            foreach (var testCase in Cases())
            {
                var text = EditInner(PilotKit.Text("pb-01-base"), testCase.Value);

                PushBackDesign design;
                try
                {
                    design = DesignOfText(text);
                }
                catch (Exception rejected)
                {
                    // The store refuses the document before any load (the same as the window of today).
                    log.Add(testCase.Key + " :: RECHAZADO " + rejected.GetType().Name + ": " + rejected.Message);
                    continue;
                }

                var assembler = new PushBackEditorDesignAssembler(Catalog);
                var composite = new PushBackCompositeEditorAssembler(Catalog);
                var state = new PushBackEditableState();
                try
                {
                    state.LoadDesign(design, NameOfText(text), assembler.Resolver);
                }
                catch (Exception rejected)
                {
                    // c-13: the load throws the same type and the same message as the window of today, from the resolver.
                    log.Add(testCase.Key + " :: RECHAZADO " + rejected.GetType().Name + ": " + rejected.Message);
                    continue;
                }

                var computation = Pasada(state, assembler, composite, PushBackSide.A);
                if (computation == null || !computation.IsValid)
                {
                    log.Add(testCase.Key + " :: NO-VALIDA");
                    continue;
                }

                var sobre = Persist(DesignOfText(text));
                var salida = Persist(computation.Design);
                var all = Delta(sobre, salida);
                log.Add(testCase.Key + " :: retiradas(a/b)=[" + string.Join(" ;; ", all.Where(line => ClassOf(line).Length > 0)) + "]");
                foreach (var line in all.Where(l => ClassOf(l).Length == 0)) log.Add("   [" + LabelOf(line) + "] " + line);
            }

            Assert.Equal(
                new[]
                {
                    "c-01 HeaderPostCatalogId vacio :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:poste-vacio-a-defecto DynamicRackSystemResolver.cs:64-66] $.PushBack.Structure.HeaderPostCatalogId: \"\" -> \"POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA\"",
                    "c-03 HighEndBeamPeraltes[0]=7.25 :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:peralte-posterior-normalizado PushBackEditorDesignAssembler.cs:155; PushBackEditorState.cs:259-269] $.PushBack.Fronts[0].HighEndBeamPeraltes[0]: 7.25 -> 3.5",
                    "c-04 InOutBeamCatalogId inexistente :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:viga-inexistente-a-defecto DynamicRackLevelGeometry.cs:41,48,200-204] $.PushBack.Structure.Fronts[0].Levels[0].InOutBeamCatalogId: \"PB0_VIGA_INEXISTENTE\" -> \"LARGUERO_IN_OUT_C6\"",
                    "c-05 InOutBeamDepth fuera del maximo :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:peralte-fuera-de-catalogo DynamicRackLevelGeometry.cs:42-45; DynamicLoadBeamGeometry.cs:52] $.PushBack.Structure.Fronts[0].Levels[0].InOutBeamDepth: 6.5 -> 6",
                    "c-06 IntermediateBeamCatalogId inexistente :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:viga-inexistente-a-defecto DynamicRackLevelGeometry.cs:41,48,200-204] $.PushBack.Structure.Fronts[0].Levels[0].IntermediateBeamCatalogId: \"PB0_VIGA_INEXISTENTE\" -> \"LARGUERO_ESCALON_INFINITO\"",
                    "c-07 InOutBeamDepth fuera del maximo en el frente 1 :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:peralte-fuera-de-catalogo DynamicRackLevelGeometry.cs:42-45; DynamicLoadBeamGeometry.cs:52] $.PushBack.Structure.Fronts[1].Levels[0].InOutBeamDepth: 6.5 -> 6",
                    "c-08 PalletWeightUnit vacia :: retiradas(a/b)=[]",
                    "c-09 PalletWeightUnit ton :: retiradas(a/b)=[]",
                    "c-10 Dimensions 7 :: retiradas(a/b)=[]",
                    "c-11 Dimensions -1 :: retiradas(a/b)=[]",
                    "c-12 PalletsDeep 1 :: RECHAZADO InvalidOperationException: El proyecto no tiene datos válidos para el sistema Push Back (¿archivo vacío o incompleto?).",
                    "c-13 PalletTolerance 0 :: RECHAZADO ArgumentException: La holgura transversal debe ser mayor que cero. (Parameter 'design')",
                    "c-14 BeamDepth 0 :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:peralte-de-larguero-0-a-defecto PushBackEditorState.Load.cs:200] $.PushBack.Structure.BeamDepth: 0 -> 6",
                    "c-15 AnnotationScale 0 :: retiradas(a/b)=[]",
                    "c-16 Dimensions 3 limite valido :: retiradas(a/b)=[]",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-17

        private static string Unidad(string persisted)
            => "[PalletDepth=" + Leer(persisted, "Structure.PalletDepth") + " PalletWeightUnit=" + Leer(persisted, "Structure.PalletWeightUnit")
               + " PalletWeight=" + Leer(persisted, "Structure.PalletWeight") + "]";

        private static string Regla(string current, string requested)
            => "regla current=" + (current == null ? "<null>" : "[" + current + "]") + " requested="
               + (requested == null ? "<null>" : "[" + requested + "]") + " -> [" + PushBackEditableState.ResolveWeightUnit(current, requested) + "]";

        [Fact]
        public void CT_PBT1B_17_TheWeightUnitThatTheBoxShowsIsTheOneEveryRebuildUses()
        {
            var log = new List<string>();
            var assembler = new PushBackEditorDesignAssembler(Catalog);

            // Part 1: the rule.
            log.Add(Regla("kg", "kg"));
            log.Add(Regla("kg", "lb"));
            log.Add(Regla("lb", "kg"));
            log.Add(Regla("kg", "ton"));
            log.Add(Regla("lb", "ton"));
            log.Add(Regla("kg", null));
            log.Add(Regla("lb", "   "));
            log.Add(Regla("kg", "KG"));
            log.Add(Regla("ton", "ton"));

            // Part 2: the output, with the sequences of the window (§5.0) over pb-01-base.
            var baseText = PilotKit.Text("pb-01-base");
            var tonText = EditInner(baseText, inner => StructureNode(inner)["PalletWeightUnit"] = "ton");
            var composite = new PushBackCompositeEditorAssembler(Catalog);

            PushBackEditableState Fresh(string text)
            {
                var state = new PushBackEditableState();
                Paso(log, () => state.LoadDesign(DesignOfText(text), NameOfText(text), assembler.Resolver));
                return state;
            }

            string Salida(PushBackEditableState state)
            {
                var computation = Pasada(state, assembler, composite, PushBackSide.A);
                return computation == null ? "<sin computacion>" : Persist(computation.Design);
            }

            void Repalet(PushBackEditableState state, double? depth, string unit)
                => state.Pallet = new PalletSpecification(state.Pallet.Front, depth ?? state.Pallet.Depth, state.Pallet.Height, state.Pallet.Weight, unit ?? state.Pallet.WeightUnit);

            var plain = Fresh(baseText);
            log.Add("carga " + Unidad(Salida(plain)));
            Repalet(plain, null, "lb");
            log.Add("WeightUnitBox=lb " + Unidad(Salida(plain)));
            Repalet(plain, 54.0, null);
            log.Add("DepthBox=54 " + Unidad(Salida(plain)));

            var ton = Fresh(tonText);
            var caja = ton.Pallet.WeightUnit;
            var salida = Unidad(Salida(ton));
            log.Add("carga ton caja=[" + caja + "] salida=" + salida + " actualizar=" + Unidad(Salida(ton)));

            var reconstruye = Fresh(tonText);
            Salida(reconstruye);
            Repalet(reconstruye, 54.0, null);
            log.Add("carga ton y DepthBox=54 " + Unidad(Salida(reconstruye)));

            // The equivalent of the row (f) of the step 1: load «ton», choose «lb», restore, change the depth.
            var restaurada = Fresh(tonText);
            Salida(restaurada);
            Repalet(restaurada, null, "lb");
            var ultima = Pasada(restaurada, assembler, composite, PushBackSide.A);
            Paso(log, () => restaurada.Restore(ultima.Design, assembler.Resolver));
            Salida(restaurada);
            var cajaRestaurada = restaurada.Pallet.WeightUnit;
            Repalet(restaurada, 54.0, null);
            log.Add("restaurar ton lb caja=[" + cajaRestaurada + "] " + Unidad(Salida(restaurada)));

            Assert.Equal(
                new[]
                {
                    "regla current=[kg] requested=[kg] -> [kg]",
                    "regla current=[kg] requested=[lb] -> [lb]",
                    "regla current=[lb] requested=[kg] -> [kg]",
                    "regla current=[kg] requested=[ton] -> [kg]",
                    "regla current=[lb] requested=[ton] -> [lb]",
                    "regla current=[kg] requested=<null> -> [kg]",
                    "regla current=[lb] requested=[   ] -> [kg]",
                    "regla current=[kg] requested=[KG] -> [kg]",
                    "regla current=[ton] requested=[ton] -> [kg]",
                    "carga [PalletDepth=48 PalletWeightUnit=\"kg\" PalletWeight=1000]",
                    "WeightUnitBox=lb [PalletDepth=48 PalletWeightUnit=\"kg\" PalletWeight=1000]",
                    "DepthBox=54 [PalletDepth=54 PalletWeightUnit=\"lb\" PalletWeight=1000]",
                    "carga ton caja=[kg] salida=[PalletDepth=48 PalletWeightUnit=\"ton\" PalletWeight=1000] actualizar=[PalletDepth=48 PalletWeightUnit=\"ton\" PalletWeight=1000]",
                    "carga ton y DepthBox=54 [PalletDepth=54 PalletWeightUnit=\"kg\" PalletWeight=1000]",
                    "restaurar ton lb caja=[lb] [PalletDepth=54 PalletWeightUnit=\"lb\" PalletWeight=1000]",
                },
                log);
        }
    }
}
