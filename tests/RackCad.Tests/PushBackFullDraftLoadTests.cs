using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3a — the LOADS of the full draft of Push Back (CT-PBT3A-02 to 05): the materializer against the one of the
    /// pilot, the reader against the one of the pilot, AUTH-13 in both readers (F4B-E24) and the copier and the comparer of
    /// the state. The model is not connected to anything: every case reads an envelope text and compares what comes out.
    /// </summary>
    public sealed class PushBackFullDraftLoadTests
    {
        private static RackCatalog Catalog => PilotKit.Catalog;

        private static readonly PushBackEditorDesignAssembler Assembler = new PushBackEditorDesignAssembler(PilotKit.Catalog);
        private static readonly PushBackCompositeEditorAssembler CompositeAssembler = new PushBackCompositeEditorAssembler(PilotKit.Catalog);

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

        private static string EditInner(string envelopeText, Action<JsonNode> edit)
        {
            var envelope = JsonNode.Parse(envelopeText);
            var inner = JsonNode.Parse(envelope["Design"].GetValue<string>());
            edit(inner);
            envelope["Design"] = inner.ToJsonString();
            return envelope.ToJsonString();
        }

        private static PushBackDesign DesignOfText(string envelope)
            => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(envelope).Design).PushBackDesign;

        private static string NameOfText(string envelope) => new RackEmbedStore().Deserialize(envelope).Name;

        private static PushBackEditorComputation Pasada(PushBackEditableState s)
            => PushBackEditableStateSeat.Pass(s, Assembler, CompositeAssembler, PushBackSide.A);

        /// <summary>The envelope of a corpus text whose design is replaced by the given one (the Plugin is not reachable from Core).</summary>
        private static string EnvelopeOf(string corpusText, PushBackDesign design)
        {
            var embed = new RackEmbedStore().Deserialize(corpusText);
            embed.Design = new RackProjectStore().Serialize(RackProject.ForPushBack(design));
            return new RackEmbedStore().Serialize(embed);
        }

        /// <summary>X-01 (§5.0, Core recipe): pb-21-cola, side B declared, selected and its slot 0 present.</summary>
        private static PushBackDesign BuildX01(List<string> log)
        {
            var text = PilotKit.Text("pb-21-cola");
            var state = new PushBackEditableState();
            Paso(log, () => state.LoadDesign(DesignOfText(text), NameOfText(text), Assembler.Resolver));
            Paso(log, () => Pasada(state));
            Paso(log, () => state.Editor.SetSideBPresent(true));
            Paso(log, () => state.Editor.SetActiveSelection(PushBackSideSelection.B));
            Paso(log, () => state.Editor.SetSlotPresent(PushBackSide.B, 0, true));
            var computation = Paso(log, () => Pasada(state));
            return computation == null ? null : new RackProjectStore().Deserialize(PilotKit.Persist(computation.Design)).PushBackDesign;
        }

        /// <summary>F-01 (§5.0, Core recipe): pb-20-ab with the first separator of A and then the first one of B lengthened by 2.</summary>
        private static PushBackDesign BuildF01(List<string> log)
        {
            var text = PilotKit.Text("pb-20-ab");
            var state = new PushBackEditableState();
            Paso(log, () => state.LoadDesign(DesignOfText(text), NameOfText(text), Assembler.Resolver));
            Paso(log, () => Pasada(state));
            PushBackEditorComputation computation = null;
            foreach (var inB in new[] { false, true })
            {
                var separator = Paso(log, () => state.Editor.SideA.ModuleSession.Modules.First(
                    m => m.Kind == DynamicRackModuleKind.Separator && m.ModuleId.StartsWith("B:", StringComparison.Ordinal) == inB));
                if (separator == null) return null;
                Paso(log, () => state.Editor.SideA.ModuleSession.SetLength(separator.ModuleId, separator.Length + 2.0));
                Paso(log, () => state.Editor.SideA.CommitModuleEdits());
                computation = Paso(log, () => Pasada(state));
            }

            return computation == null ? null : new RackProjectStore().Deserialize(PilotKit.Persist(computation.Design)).PushBackDesign;
        }

        private static string Describe(PushBackPilotMaterializationKind kind, string reason)
            => kind + (reason == null ? string.Empty : " motivo=" + reason);

        // ------------------------------------------------------------------------------------------------- CT-02

        private static string MaterializationLine(List<string> log, string label, string text, RackCatalog catalog)
        {
            var full = Paso(log, () => PushBackFullDraftMaterializer.Materialize(text, catalog));
            var pilot = Paso(log, () => PushBackPilotMaterializer.Materialize(text, catalog));
            return label + " completo=" + (full == null ? "<sin resultado>" : Describe(full.Kind, full.Reason))
                + " piloto=" + (pilot == null ? "<sin resultado>" : Describe(pilot.Kind, pilot.Reason));
        }

        [Fact]
        public void CT_PBT3A_02_TheMaterializerIsTheOneOfThePilotExceptForTheCompositeRack()
        {
            var log = new List<string>();
            var baseText = PilotKit.Text("pb-01-base");

            log.Add(MaterializationLine(log, "en-blanco", "   ", Catalog));
            log.Add(MaterializationLine(log, "nulo", null, Catalog));
            log.Add(MaterializationLine(log, "otro-tipo", IntentKit.Rewrite(baseText, e => e.Kind = RackEmbedDocument.KindSelective), Catalog));
            log.Add(MaterializationLine(log, "sin-diseno", IntentKit.Rewrite(baseText, e => e.Design = I58F1Fixtures.Serialize("cabecera")), Catalog));
            log.Add(MaterializationLine(log, "ilegible", IntentKit.Rewrite(baseText, e => e.Design = "{ this is not json"), Catalog));
            log.Add(MaterializationLine(log, "catalogo-nulo", baseText, null));
            log.Add(MaterializationLine(log, "pb-01-base", baseText, Catalog));
            log.Add(MaterializationLine(log, "pb-21-cola", PilotKit.Text("pb-21-cola"), Catalog));
            log.Add(MaterializationLine(log, "pb-20-ab", PilotKit.Text("pb-20-ab"), Catalog));
            log.Add(MaterializationLine(log, "c-13", EditInner(baseText, inner => inner["PushBack"]["Structure"]["PalletTolerance"] = 0), Catalog));

            var unreadable = " motivo=" + PushBackPilotMaterializer.UnreadableOrFutureDiagnostic;
            var c13 = " motivo=" + PushBackPilotMaterializer.MaterializeFailurePrefix + "La holgura transversal debe ser mayor que cero. (Parameter 'design')";
            Assert.Equal(
                new[]
                {
                    "en-blanco completo=Failed" + unreadable + " piloto=Failed" + unreadable,
                    "nulo completo=Failed" + unreadable + " piloto=Failed" + unreadable,
                    "otro-tipo completo=Failed motivo=" + PushBackPilotMaterializer.KindDivergentDiagnostic
                        + " piloto=Failed motivo=" + PushBackPilotMaterializer.KindDivergentDiagnostic,
                    "sin-diseno completo=Failed motivo=" + PushBackPilotMaterializer.NoDesignDiagnostic
                        + " piloto=Failed motivo=" + PushBackPilotMaterializer.NoDesignDiagnostic,
                    "ilegible completo=Failed" + unreadable + " piloto=Failed" + unreadable,
                    "catalogo-nulo completo=Failed motivo=" + PushBackPilotMaterializer.CatalogRequiredDiagnostic
                        + " piloto=Failed motivo=" + PushBackPilotMaterializer.CatalogRequiredDiagnostic,
                    "pb-01-base completo=Admitted piloto=Admitted",
                    "pb-21-cola completo=OutOfPilot motivo=" + PushBackPilotMaterializer.DormantTailOutOfPilotReason
                        + " piloto=OutOfPilot motivo=" + PushBackPilotMaterializer.DormantTailOutOfPilotReason,
                    "pb-20-ab completo=Admitted piloto=OutOfPilot motivo=" + PushBackPilotMaterializer.CompositeOutOfPilotReason,
                    "c-13 completo=Failed" + c13 + " piloto=Failed" + c13,
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-03

        private static bool SameBase(DraftStaleBase a, DraftStaleBase b)
        {
            if (a == null || b == null) return ReferenceEquals(a, b);
            if (a.Entries.Count != b.Entries.Count) return false;
            foreach (var pair in a.Entries)
            {
                if (!b.Entries.TryGetValue(pair.Key, out var other) || other != pair.Value) return false;
            }

            return string.Equals(a.VariableRegistryText, b.VariableRegistryText, StringComparison.Ordinal);
        }

        /// <summary>The diagnostic, the detail, the hint, the key of the origin and the base (entry by entry) of the two readers.</summary>
        private static bool SameRead(PushBackPilotIntentResult pilot, PushBackFullIntentResult full)
        {
            if (pilot.Read.IsAdmitted != full.Read.IsAdmitted) return false;
            if (!string.Equals(pilot.Read.Admission.Diagnostic, full.Read.Admission.Diagnostic, StringComparison.Ordinal)) return false;
            if (!string.Equals(pilot.Detail, full.Detail, StringComparison.Ordinal)) return false;
            if (!string.Equals(pilot.ClassicEditorHint, full.ClassicEditorHint, StringComparison.Ordinal)) return false;
            if (!string.Equals(pilot.OriginKey, full.OriginKey, StringComparison.Ordinal)) return false;
            if (!SameBase(pilot.StaleBase, full.StaleBase)) return false;
            return !full.Read.IsAdmitted || (full.Read.InitialState != null && SameBase(pilot.Read.StaleBase, full.Read.StaleBase));
        }

        private static string ReadLine(List<string> log, string label, PushBackPilotIntentScan scan, RackCatalog catalog)
        {
            var pilot = Paso(log, () => PushBackPilotIntentReader.Read(scan, catalog));
            var full = Paso(log, () => PushBackFullDraftReader.Read(scan, catalog));
            return label + " iguales=" + (pilot != null && full != null && SameRead(pilot, full));
        }

        private static string ZeroEveryPalletHeight(string envelopeText) =>
            IntentKit.Rewrite(envelopeText, e =>
            {
                var design = JsonNode.Parse(e.Design);
                ZeroPalletHeights(design);
                e.Design = design.ToJsonString();
            });

        private static void ZeroPalletHeights(JsonNode node)
        {
            if (node is JsonObject obj)
            {
                foreach (var key in obj.Select(p => p.Key).ToList())
                {
                    if (string.Equals(key, "PalletHeight", StringComparison.Ordinal)) obj[key] = 0;
                    else ZeroPalletHeights(obj[key]);
                }
            }
            else if (node is JsonArray array)
            {
                foreach (var item in array) ZeroPalletHeights(item);
            }
        }

        [Fact]
        public void CT_PBT3A_03_TheFullReaderIsTheOneOfThePilotExceptForTheCompositeRack()
        {
            var log = new List<string>();
            var rack = IntentKit.Rack;
            var other = "22222222-2222-4222-8222-000000000009";
            var baseText = IntentKit.BaseText();
            var lateral = IntentKit.Variant("pb-01-base", rack, view: "lateral", section: 3);
            var withExtra = JsonNode.Parse(baseText).AsObject();
            withExtra["Extra"] = 1;
            var coldId = new RackEmbedStore().Deserialize(PilotKit.Text("pb-21-cola")).Id;

            // The 14 rules of the admission of the pilot, one scan per rule, and an admitted read. The rule 12 is the A/B rack: it
            // is the one the full reader reads differently, and it goes at the end.
            log.Add(ReadLine(log, "regla-01", IntentKit.Scan(string.Empty, "A1", IntentKit.Member("A1", baseText)), Catalog));
            log.Add(ReadLine(log, "regla-02", IntentKit.Scan(rack, "ZZ", IntentKit.Member("A1", baseText)), Catalog));
            log.Add(ReadLine(log, "regla-03", IntentKit.Scan(rack, "A1", IntentKit.Unreadable("A1", rack)), Catalog));
            log.Add(ReadLine(log, "regla-04", IntentKit.Scan(rack, "A1", IntentKit.Custom("A1", string.Empty, "frontal", baseText)), Catalog));
            log.Add(ReadLine(log, "regla-05", IntentKit.Scan(rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-02-modulos", other))), Catalog));
            log.Add(ReadLine(log, "regla-06", IntentKit.Scan(rack, "A1", IntentKit.Member("A1", baseText, xref: true)), Catalog));
            log.Add(ReadLine(log, "regla-07", IntentKit.Scan(rack, "A1", IntentKit.Member("A1", baseText), IntentKit.Unreadable("B2", rack)), Catalog));
            log.Add(ReadLine(log, "regla-08", IntentKit.Scan(rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rack, kind: "selective")), IntentKit.Member("B2", lateral)), Catalog));
            log.Add(ReadLine(log, "regla-09", IntentKit.Scan(rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rack, kind: "selective"))), Catalog));
            log.Add(ReadLine(log, "regla-10", IntentKit.Scan(rack, "A1", IntentKit.Member("A1", baseText), IntentKit.Member("B2", IntentKit.Variant("pb-02-modulos", rack, view: "lateral", section: 3))), Catalog));
            log.Add(ReadLine(log, "regla-11", IntentKit.Scan(rack, "A1", IntentKit.Custom("A1", rack, "frontal", withExtra.ToJsonString())), Catalog));
            log.Add(ReadLine(log, "regla-12-cola", IntentKit.Scan(coldId, "H1", IntentKit.Member("H1", PilotKit.Text("pb-21-cola"))), Catalog));
            log.Add(ReadLine(log, "regla-13", IntentKit.Scan(rack, "A1", IntentKit.Custom("A1", rack, "frontal", ZeroEveryPalletHeight(baseText))), Catalog));
            log.Add(ReadLine(log, "regla-14", IntentKit.Scan(rack, "A1", IntentKit.Member("A1", baseText)), null));
            log.Add(ReadLine(log, "admitido", IntentKit.TwoViews(), Catalog));

            // The compound rack: the pilot refuses it as outside of the pilot, and the full reader admits it with the same base.
            var abText = PilotKit.Text("pb-20-ab");
            var abScan = IntentKit.Scan(new RackEmbedStore().Deserialize(abText).Id, "H1", IntentKit.Member("H1", abText));
            var abPilot = Paso(log, () => PushBackPilotIntentReader.Read(abScan, Catalog));
            var abFull = Paso(log, () => PushBackFullDraftReader.Read(abScan, Catalog));
            log.Add("pb-20-ab piloto=" + (abPilot != null && abPilot.Read.IsAdmitted ? "Admitted" : "NotAdmitted")
                + " completo=" + (abFull != null && abFull.Read.IsAdmitted ? "Admitted" : "NotAdmitted")
                + " base-igual=" + (abPilot != null && abFull != null && SameBase(abPilot.StaleBase, abFull.StaleBase)));

            var expected = new List<string>();
            foreach (var rule in new[] { "regla-01", "regla-02", "regla-03", "regla-04", "regla-05", "regla-06", "regla-07", "regla-08", "regla-09", "regla-10",
                                         "regla-11", "regla-12-cola", "regla-13", "regla-14", "admitido" })
            {
                expected.Add(rule + " iguales=True");
            }

            expected.Add("pb-20-ab piloto=NotAdmitted completo=Admitted base-igual=True");
            Assert.Equal(expected, log);
        }

        // ------------------------------------------------------------------------------------------------- CT-04

        /// <summary>The 28 nested paths of AUTH-13 for Push Back: <c>I58F1Matrix.Nested("pushback")</c> (<c>I58F1Matrix.cs:97-102</c>),
        /// copied as a literal list because <c>Nested</c> is private.</summary>
        private static readonly string[] NestedPaths =
        {
            "PushBack.Structure.Fronts.0", "PushBack.Structure.Fronts.0.Levels.0", "PushBack.Structure.Modules.0",
            "PushBack.Structure.Modules.0.Header", "PushBack.Structure.Modules.0.Header.LeftPost",
            "PushBack.Structure.Modules.0.Header.RightBasePlate", "PushBack.Structure.Modules.0.Header.Horizontals.0",
            "PushBack.Structure.Modules.0.Header.Panels.0", "PushBack.Structure.SafetySelections.0",
            "PushBack.Structure.SafetySelections.0.PostSides.0", "PushBack.Structure.SafetySelections.0.DesviadorOffCells.0",
            "PushBack.Structure.SafetySelections.0.DefensaPosts.0", "PushBack.Structure.SafetySelections.0.GuiaEntradaOffCells.0",
            "PushBack.Structure.SafetySelections.0.ParrillaOffCells.0", "PushBack.Structure.SafetySelections.0.BotaPosts.0",
            "PushBack.Structure.SafetySelections.0.TopeOffCells.0", "PushBack.Structure.SafetySelections.0.BotaBPosts.0",
            "PushBack.Structure.HeaderLineOverrides.0", "PushBack.Structure.HeaderLineOverrides.0.Header",
            "PushBack.Structure.DerivedPostLineOverrides.0",
            "PushBack.Structure", "PushBack.Fronts.0", "PushBack.SideB", "PushBack.SideB.Fronts.0", "PushBack.SideB.FrontConfigs.0",
            "PushBack.Composite", "PushBack.Composite.Topologies.0", "PushBack.RearTopeOffCells.0",
        };

        private static string Auth13Line(List<string> log, string label, string envelopeText)
        {
            var scan = IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", envelopeText));
            var pilot = Paso(log, () => PushBackPilotIntentReader.Read(scan, Catalog));
            var full = Paso(log, () => PushBackFullDraftReader.Read(scan, Catalog));
            return label
                + " piloto=[" + (pilot == null ? "<sin resultado>" : pilot.Read.Admission.Diagnostic) + "]"
                + " completo=[" + (full == null ? "<sin resultado>" : full.Read.Admission.Diagnostic) + "]"
                + " pista=[" + (full == null ? "<sin resultado>" : full.ClassicEditorHint) + "]";
        }

        [Fact]
        public void CT_PBT3A_04_Auth13RejectsEveryUnknownFieldAtEveryLevelInBothReaders()
        {
            var log = new List<string>();
            var rich = I58F1Fixtures.Rich("pushback");
            var baseText = PilotKit.Text("pb-01-base");

            // The control: the same rack with no field added. It is admitted by the full reader; the pilot refuses it afterwards, as an A/B rack.
            var controlScan = IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", IntentKit.Rewrite(baseText, e => e.Design = rich)));
            var controlPilot = Paso(log, () => PushBackPilotIntentReader.Read(controlScan, Catalog));
            var controlFull = Paso(log, () => PushBackFullDraftReader.Read(controlScan, Catalog));
            log.Add("control completo=" + (controlFull != null && controlFull.Read.IsAdmitted ? "Admitted" : "NotAdmitted")
                + " piloto=[" + (controlPilot == null ? "<sin resultado>" : controlPilot.Read.IsAdmitted ? "Admitted" : controlPilot.Read.Admission.Diagnostic) + "]");

            foreach (var path in NestedPaths)
            {
                var design = I58F1Fixtures.Set(rich, path + ".FutureNested", "1");
                log.Add(Auth13Line(log, path, IntentKit.Rewrite(baseText, e => e.Design = design)));
            }

            // The three layers on top: the wrapper of the project, the Push Back block and the envelope itself.
            log.Add(Auth13Line(log, "Future", IntentKit.Rewrite(baseText, e => e.Design = I58F1Fixtures.Set(rich, "Future", "1"))));
            log.Add(Auth13Line(log, "PushBack.Future", IntentKit.Rewrite(baseText, e => e.Design = I58F1Fixtures.Set(rich, "PushBack.Future", "1"))));
            var envelope = JsonNode.Parse(IntentKit.Rewrite(baseText, e => e.Design = rich)).AsObject();
            envelope["Future"] = 1;
            log.Add(Auth13Line(log, "Envelope.Future", envelope.ToJsonString()));

            var unreadable = "No editable aquí: ilegible o de versión futura";
            var expected = new List<string>
            {
                "control completo=Admitted piloto=[" + PushBackPilotIntentReader.OutOfPilotDiagnostic + "]",
            };
            foreach (var path in NestedPaths.Concat(new[] { "Future", "PushBack.Future", "Envelope.Future" }))
            {
                expected.Add(path + " piloto=[" + unreadable + "] completo=[" + unreadable + "] pista=[Usa RACKEDITAR para editarlo con el editor clásico.]");
            }

            Assert.Equal(expected, log);
        }

        // ------------------------------------------------------------------------------------------------- CT-05

        private static bool IsProductType(Type type)
            => type.Namespace != null && type.Namespace.StartsWith("RackCad", StringComparison.Ordinal) && !type.IsEnum;

        private static IEnumerable<FieldInfo> InstanceFields(Type type)
        {
            for (var current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    yield return field;
                }
            }
        }

        private static string KeyOf(FieldInfo field)
        {
            var name = field.Name;
            if (name.StartsWith("<", StringComparison.Ordinal)) name = name.Substring(1, name.IndexOf('>') - 1);
            return field.DeclaringType.Name + "." + name;
        }

        /// <summary>A type is immutable when it is a string or every instance field of it is init-only and immutable.</summary>
        private static bool IsImmutable(Type type)
        {
            if (type == typeof(string) || type.IsPrimitive || type.IsEnum) return true;
            if (!IsProductType(type)) return false;
            return InstanceFields(type).All(field => field.IsInitOnly && (field.FieldType.IsValueType || IsImmutable(field.FieldType)));
        }

        /// <summary>Every reference-type object a graph holds (collections included), by walking ALL fields, and also the keys
        /// and the values of every dictionary: its entries are value types and the dictionary is not a product type.</summary>
        private static HashSet<object> AllObjects(object root)
        {
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var pending = new Queue<object>();
            pending.Enqueue(root);
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                if (current is string || !visited.Add(current)) continue;
                if (current is IDictionary dictionary)
                {
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        if (entry.Key != null && !entry.Key.GetType().IsValueType) pending.Enqueue(entry.Key);
                        if (entry.Value != null && !entry.Value.GetType().IsValueType) pending.Enqueue(entry.Value);
                    }
                }
                else if (current is IEnumerable items)
                {
                    foreach (var item in items)
                    {
                        if (item != null && !item.GetType().IsValueType) pending.Enqueue(item);
                    }
                }

                if (!IsProductType(current.GetType())) continue;
                foreach (var field in InstanceFields(current.GetType()))
                {
                    if (field.FieldType.IsValueType && !field.FieldType.IsGenericType) continue;
                    var value = field.GetValue(current);
                    if (value != null && !value.GetType().IsValueType) pending.Enqueue(value);
                }
            }

            return visited;
        }

        private static HashSet<object> Mutable(IEnumerable<object> objects)
        {
            var set = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var item in objects)
            {
                if (!IsImmutable(item.GetType())) set.Add(item);
            }

            return set;
        }

        /// <summary>The two references D2 declares as shared: the resolved baseline and the module report of each side.</summary>
        private static IEnumerable<object> SharedRoots(PushBackEditableState state)
        {
            foreach (var side in new[] { state.Editor.SideA, state.Editor.SideB })
            {
                if (side.WorkingBaseline != null) yield return side.WorkingBaseline;
                if (side.LastModuleReconciliation != null) yield return side.LastModuleReconciliation;
            }
        }

        // The mutator of the pilot, copied (CT-PBT1B-06): the editable slots of the state, one by one.

        private enum Class { E, S, A, D, T }

        private static readonly Dictionary<string, Class> Table = BuildTable();

        private static Dictionary<string, Class> BuildTable()
        {
            var table = new Dictionary<string, Class>(StringComparer.Ordinal);
            void Add(Class kind, params string[] keys) { foreach (var key in keys) table[key] = kind; }

            Add(Class.E, "PushBackEditableState.Editor", "PushBackEditableState.Pallet", "PushBackEditableState.PalletsDeep",
                "PushBackEditableState.PostCatalogId", "PushBackEditableState.PostPeralte", "PushBackEditableState.PalletTolerance",
                "PushBackEditableState.BeamDepth", "PushBackEditableState.AnnotationScale", "PushBackEditableState.FirstLevelDatum",
                "PushBackEditableState.NumberFronts", "PushBackEditableState.NumberLevels", "PushBackEditableState.DrawRackName",
                "PushBackEditableState.Dimensions", "PushBackEditableState.DimensionStyle", "PushBackEditableState.LoadedDimensionViews",
                "PushBackEditableState.DimensionViewsTouched", "PushBackEditableState.DimensionsFrontal", "PushBackEditableState.DimensionsLateral",
                "PushBackEditableState.DimensionsPlanta", "PushBackEditableState.Advanced", "PushBackEditableState.SafetySelections",
                "PushBackEditableState.Name");

            Add(Class.E, "PushBackCompositeEditorState.topologies", "PushBackCompositeEditorState.sideBEverDeclared",
                "PushBackCompositeEditorState.SideA", "PushBackCompositeEditorState.SideB", "PushBackCompositeEditorState.SideBPresent",
                "PushBackCompositeEditorState.Gap", "PushBackCompositeEditorState.CentralSeparator", "PushBackCompositeEditorState.StructureOverrideA",
                "PushBackCompositeEditorState.StructureOverrideB", "PushBackCompositeEditorState.DefaultTopology", "PushBackCompositeEditorState.DefaultDirection");
            Add(Class.A, "PushBackCompositeEditorState.DormantCompositeTail", "PushBackCompositeEditorState.DormantTailLineOverrides");
            Add(Class.T, "PushBackCompositeEditorState.dormantTailConsumed");
            Add(Class.S, "PushBackCompositeEditorState.ActiveSide", "PushBackCompositeEditorState.ActiveSelection");

            Add(Class.E, "PushBackTopologyCell.Frente", "PushBackTopologyCell.Level", "PushBackTopologyCell.Topology",
                "PushBackTopologyCell.Direction", "PushBackTopologyCell.CorridaDepth");

            Add(Class.E, "PushBackEditorState.structure", "PushBackEditorState.pushFronts", "PushBackEditorState.RearTopeSaque",
                "PushBackEditorState.RearTopePieceId", "PushBackEditorState.DefensePieceId");
            Add(Class.D, "PushBackEditorState.workingBaseline", "PushBackEditorState.LastModuleReconciliation");
            Add(Class.A, "PushBackEditorState.moduleSession", "PushBackEditorState.moduleCommit", "PushBackEditorState.moduleSignature");

            Add(Class.E, "DynamicFrontMatrix.fronts");
            Add(Class.S, "DynamicFrontMatrix.selectedCells", "DynamicFrontMatrix.selectedFrontIndex", "DynamicFrontMatrix.selectedLevelIndex");

            Add(Class.E, "DynamicEditorFront.Index", "DynamicEditorFront.IsActive", "DynamicEditorFront.PalletCount", "DynamicEditorFront.LoadLevels",
                "DynamicEditorFront.PalletsDeep", "DynamicEditorFront.DepthStartPosition", "DynamicEditorFront.FirstLevelHeight",
                "DynamicEditorFront.Bfr", "DynamicEditorFront.BeamLength", "DynamicEditorFront.Cells");
            Add(Class.E, "DynamicEditorCell.PalletFront", "DynamicEditorCell.PalletHeight", "DynamicEditorCell.PalletWeight", "DynamicEditorCell.ClearHeight",
                "DynamicEditorCell.InOutBeamCatalogId", "DynamicEditorCell.InOutBeamDepth", "DynamicEditorCell.BeamLengthOverride",
                "DynamicEditorCell.IntermediateBeamCatalogId", "DynamicEditorCell.IntermediateBeamDepth");
            Add(Class.E, "PushBackEditorFront.Cells", "PushBackEditorCell.HighEndBeamPeralte", "PushBackEditorCell.RearTopeEnabled",
                "PushBackEditorCell.PalletsDeepOverride", "PushBackEditorCell.DrawPallet");

            Add(Class.E, "PushBackEditorInputs.Pallet", "PushBackEditorInputs.PalletsDeep", "PushBackEditorInputs.PostCatalogId",
                "PushBackEditorInputs.PostPeralte", "PushBackEditorInputs.PalletTolerance", "PushBackEditorInputs.FirstLevelDatum",
                "PushBackEditorInputs.BeamDepth", "PushBackEditorInputs.Annotations", "PushBackEditorInputs.ManualHeaderHeightOverride",
                "PushBackEditorInputs.DerivedPostReinforced", "PushBackEditorInputs.DerivedPostReinforcementHeight",
                "PushBackEditorInputs.DerivedPostHeight", "PushBackEditorInputs.SeparatorCountOverride",
                "PushBackEditorInputs.SeparatorSpacingOverride", "PushBackEditorInputs.SafetySelections");
            Add(Class.E, "PalletSpecification.Front", "PalletSpecification.Depth", "PalletSpecification.Height", "PalletSpecification.Weight",
                "PalletSpecification.WeightUnit");
            Add(Class.E, "DynamicAnnotationOptions.NumberFronts", "DynamicAnnotationOptions.NumberLevels", "DynamicAnnotationOptions.DrawRackName",
                "DynamicAnnotationOptions.AnnotationScale", "DynamicAnnotationOptions.Dimensions", "DynamicAnnotationOptions.DimensionViews",
                "DynamicAnnotationOptions.DimensionStyle");

            Add(Class.E, "SelectiveSafetySelection.ElementId", "SelectiveSafetySelection.Quantity", "SelectiveSafetySelection.Side",
                "SelectiveSafetySelection.LowEndOnly", "SelectiveSafetySelection.AuthoredSide", "SelectiveSafetySelection.BotaB",
                "SelectiveSafetySelection.BootSidesDeclared", "SelectiveSafetySelection.DerivedAisles", "SelectiveSafetySelection.NearFace",
                "SelectiveSafetySelection.FarFace", "SelectiveSafetySelection.BothEndsAreLoadFaces", "SelectiveSafetySelection.SecondLoadFacePosts",
                "SelectiveSafetySelection.DesviadorCellsAreByPost", "SelectiveSafetySelection.PostSides", "SelectiveSafetySelection.tope",
                "SelectiveSafetySelection.desviador", "SelectiveSafetySelection.defensa", "SelectiveSafetySelection.guia",
                "SelectiveSafetySelection.parrilla", "SelectiveSafetySelection.bota");
            Add(Class.E, "SelectiveBotaConfig.PieceId", "SelectiveBotaConfig.Placement", "SelectiveBotaConfig.Posts", "SelectiveBotaConfig.Automatic",
                "SelectiveBotaConfig.BlankPosts", "BootPostPlacement.PostIndex", "BootPostPlacement.Placement", "DerivedAisleEntry.PostIndex",
                "DerivedAisleEntry.Authored", "SafetyFacePiece.IsNone", "SafetyFacePiece.ElementId", "SafetyPostSide.PostIndex", "SafetyPostSide.Side",
                "SelectiveTopeConfig.Shared", "SelectiveTopeConfig.Fondo", "SelectiveTopeConfig.Saque", "SelectiveTopeConfig.Frontal",
                "SelectiveTopeConfig.OffCells", "SelectiveDesviadorConfig.Longitud", "SelectiveDesviadorConfig.PrimerNivelAltura",
                "SelectiveDesviadorConfig.OffCells", "SelectiveDefensaConfig.Posts", "SafetyPostDefense.PostIndex", "SafetyPostDefense.ExitLength",
                "SafetyPostDefense.EntranceLength", "SafetyPostDefense.ExitAuto", "SafetyPostDefense.EntranceAuto", "SelectiveGuiaConfig.OffCells",
                "SelectiveParrillaConfig.Frontal", "SelectiveParrillaConfig.Lateral", "SelectiveParrillaConfig.Frente",
                "SelectiveParrillaConfig.Cantidad", "SelectiveParrillaConfig.OffCells", "SelectiveGridCell.Frente", "SelectiveGridCell.Level");
            return table;
        }

        private static readonly HashSet<string> ContainerKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "PushBackEditableState.Editor", "PushBackEditableState.Pallet", "PushBackEditableState.Advanced",
            "PushBackCompositeEditorState.SideA", "PushBackCompositeEditorState.SideB", "PushBackEditorState.structure",
            "PushBackEditorInputs.Pallet", "PushBackEditorInputs.Annotations",
            "SelectiveSafetySelection.BotaB", "SelectiveSafetySelection.tope", "SelectiveSafetySelection.desviador",
            "SelectiveSafetySelection.defensa", "SelectiveSafetySelection.guia", "SelectiveSafetySelection.parrilla", "SelectiveSafetySelection.bota",
        };

        private static readonly HashSet<string> NotMutated = new HashSet<string>(StringComparer.Ordinal)
        {
            "DynamicEditorFront.LoadLevels", "DynamicFrontMatrix.fronts", "PushBackEditorFront.Cells", "PushBackEditorState.pushFronts",
            "SafetyFacePiece.ElementId", "SafetyFacePiece.IsNone",
        };

        private static readonly HashSet<string> AdvancedSix = new HashSet<string>(StringComparer.Ordinal)
        {
            "PushBackEditorInputs.ManualHeaderHeightOverride", "PushBackEditorInputs.DerivedPostReinforced",
            "PushBackEditorInputs.DerivedPostReinforcementHeight", "PushBackEditorInputs.DerivedPostHeight",
            "PushBackEditorInputs.SeparatorCountOverride", "PushBackEditorInputs.SeparatorSpacingOverride",
        };

        private static IEnumerable<(object Owner, FieldInfo Field)> EditableSlots(PushBackEditableState root)
        {
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var pending = new Queue<object>();
            pending.Enqueue(root);
            while (pending.Count > 0)
            {
                var owner = pending.Dequeue();
                if (!visited.Add(owner)) continue;
                foreach (var field in InstanceFields(owner.GetType()))
                {
                    var key = KeyOf(field);
                    if (!Table.TryGetValue(key, out var kind) || kind != Class.E) continue;
                    if (ReferenceEquals(owner, root.Advanced) && !AdvancedSix.Contains(key)) continue;
                    yield return (owner, field);
                    var value = field.GetValue(owner);
                    if (value == null) continue;
                    if (value is IEnumerable items && !(value is string))
                    {
                        foreach (var item in items)
                        {
                            if (item != null && IsProductType(item.GetType())) pending.Enqueue(item);
                        }
                    }
                    else if (IsProductType(value.GetType()))
                    {
                        pending.Enqueue(value);
                    }
                }
            }
        }

        private sealed class NoReplacement { }

        private static bool TryMutate(object owner, FieldInfo field, out Action undo)
        {
            undo = () => { };
            if (ContainerKeys.Contains(KeyOf(field))) return false;

            var type = field.FieldType;
            var current = field.GetValue(owner);
            if ((current is IEnumerable && !(current is string)) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>)))
            {
                return MutateCollection(current, out undo);
            }

            var replacement = Replacement(type, current);
            if (replacement is NoReplacement) return false;
            field.SetValue(owner, replacement);
            undo = () => field.SetValue(owner, current);
            return true;
        }

        private static object Replacement(Type type, object current)
        {
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
            {
                return current == null ? Replacement(underlying, null) : null;
            }

            if (type == typeof(bool)) return !(bool)(current ?? false);
            if (type == typeof(int)) return (int)(current ?? 0) + 1;
            if (type == typeof(double)) return double.IsNaN((double)(current ?? 0.0)) ? 2.0 : (double)(current ?? 0.0) + 1.5;
            if (type == typeof(string)) return ((string)current ?? string.Empty) + "~";
            if (type.IsEnum)
            {
                var values = Enum.GetValues(type).Cast<object>().ToList();
                return values.First(value => !value.Equals(current));
            }

            if (type == typeof(SafetyFacePiece)) return SafetyFacePiece.Of(((SafetyFacePiece)current)?.ElementId + "~");
            if (current == null && !type.IsValueType) return RuntimeHelpers.GetUninitializedObject(type);
            if (!type.IsValueType) return null;
            return new NoReplacement();
        }

        private static bool MutateCollection(object collection, out Action undo)
        {
            var type = collection.GetType();
            var elementType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);
            var element = NewElement(elementType);
            var add = type.GetMethod("Add", new[] { elementType });
            var remove = type.GetMethod("Remove", new[] { elementType });
            add.Invoke(collection, new[] { element });
            undo = () =>
            {
                if (collection is IList list && list.Count > 0)
                {
                    list.RemoveAt(list.Count - 1);
                }
                else
                {
                    remove.Invoke(collection, new[] { element });
                }
            };
            return true;
        }

        private static object NewElement(Type type)
        {
            if (type == typeof(int)) return 77;
            if (type == typeof(double)) return 77.5;
            if (type == typeof(string)) return "x";
            if (type == typeof(ValueTuple<int, int>)) return (88, 99);
            return Activator.CreateInstance(type, nonPublic: true);
        }

        private static PushBackFullDraftState FullOf(List<string> log, string text)
        {
            var result = Paso(log, () => PushBackFullDraftMaterializer.Materialize(text, Catalog));
            return result == null || !result.IsAdmitted ? null : result.State;
        }

        private static bool Eq(List<string> log, PushBackFullDraftState a, PushBackFullDraftState b)
            => Paso(log, () => PushBackFullDraftStateComparer.Instance.Equals(a, b));

        [Fact]
        public void CT_PBT3A_05_TheCopierAndTheComparerOfTheFullStateKeepOnlyTheTwoDeclaredReferences()
        {
            var log = new List<string>();
            var scenarios = new List<(string Name, string Text)>
            {
                ("pb-06-seguridad", PilotKit.Text("pb-06-seguridad")),
                ("pb-07-combinado", PilotKit.Text("pb-07-combinado")),
                ("pb-20-ab", PilotKit.Text("pb-20-ab")),
            };
            var f01 = BuildF01(log);
            var x01 = BuildX01(log);
            scenarios.Add(("F-01", f01 == null ? null : EnvelopeOf(PilotKit.Text("pb-20-ab"), f01)));
            scenarios.Add(("X-01", x01 == null ? null : EnvelopeOf(PilotKit.Text("pb-21-cola"), x01)));

            var expected = new List<string>();
            foreach (var scenario in scenarios)
            {
                var original = FullOf(log, scenario.Text);
                var copy = original == null ? null : Paso(log, () => PushBackFullDraftStateComparer.Copy(original));

                var equal = original != null && copy != null && Eq(log, original, copy) && Eq(log, copy, original);
                var distinct = original != null && copy != null && !ReferenceEquals(original, copy) && !ReferenceEquals(original.Editable, copy.Editable);
                var otherOrigin = original != null && copy != null
                    && !Eq(log, original, new PushBackFullDraftState(copy.Editable, copy.Origin + " "));
                var again = FullOf(log, scenario.Text);
                var twice = original != null && again != null && Eq(log, original, again)
                    && Paso(log, () => PushBackFullDraftStateComparer.Instance.GetHashCode(original) == PushBackFullDraftStateComparer.Instance.GetHashCode(again));
                log.Add(scenario.Name + " copia-igual=" + equal + " distinta-instancia=" + distinct + " origen-cambiado-distinto=" + otherOrigin
                    + " dos-materializaciones-iguales=" + twice);
                expected.Add(scenario.Name + " copia-igual=True distinta-instancia=True origen-cambiado-distinto=True dos-materializaciones-iguales=True");

                // The objects the copy shares with its original: exactly the non-null references of D2 and what they reach.
                if (original == null || copy == null)
                {
                    log.Add(scenario.Name + " compartidos-exactos=False raices>0=False");
                }
                else
                {
                    var shared = Mutable(AllObjects(original.Editable).Intersect(AllObjects(copy.Editable), ReferenceEqualityComparer.Instance));
                    var wanted = Mutable(SharedRoots(original.Editable).SelectMany(root => AllObjects(root)));
                    log.Add(scenario.Name + " compartidos-exactos=" + shared.SetEquals(wanted) + " raices>0=" + SharedRoots(original.Editable).Any());
                }

                expected.Add(scenario.Name + " compartidos-exactos=True raices>0=True");

                // The tail, the commit and the session of each side of the copy share nothing mutable with the WHOLE graph of the original.
                if (original == null || copy == null)
                {
                    log.Add(scenario.Name + " cola-comision-sesion revisados>0=False compartidos=-1");
                }
                else
                {
                    var ownedByCopy = new List<object>();
                    ownedByCopy.AddRange(copy.Editable.Editor.DormantCompositeTail);
                    ownedByCopy.AddRange(copy.Editable.Editor.DormantTailLineOverrides);
                    foreach (var side in new[] { copy.Editable.Editor.SideA, copy.Editable.Editor.SideB })
                    {
                        if (side.ModuleCommit != null) ownedByCopy.Add(side.ModuleCommit);
                        ownedByCopy.Add(side.ModuleSession);
                    }

                    var reached = Mutable(ownedByCopy.Where(item => item != null).SelectMany(root => AllObjects(root)));
                    var common = reached.Intersect(AllObjects(original.Editable), ReferenceEqualityComparer.Instance).Count();
                    log.Add(scenario.Name + " cola-comision-sesion revisados>0=" + (reached.Count > 0) + " compartidos=" + common);
                }

                expected.Add(scenario.Name + " cola-comision-sesion revisados>0=True compartidos=0");
            }

            // Every editable slot of the state, one by one (the mutator of the pilot): the copy carries the mutated value, and the
            // copy taken before does not; undoing the mutation restores the equality.
            foreach (var scenario in scenarios.Where(s => s.Name == "pb-07-combinado" || s.Name == "F-01"))
            {
                var state = FullOf(log, scenario.Text);
                var start = state == null ? null : Paso(log, () => PushBackFullDraftStateComparer.Copy(state));
                var exercised = 0;
                var failures = new List<string>();
                if (state != null && start != null)
                {
                    foreach (var (owner, field) in EditableSlots(state.Editable).ToList())
                    {
                        var key = KeyOf(field);
                        if (NotMutated.Contains(key)) continue;
                        if (!TryMutate(owner, field, out var undo)) continue;
                        exercised++;
                        var taken = Paso(log, () => PushBackFullDraftStateComparer.Copy(state));
                        if (taken == null) { failures.Add(key + " sin copia"); undo(); continue; }
                        if (!Eq(log, taken, state) || !Eq(log, state, taken)) failures.Add(key + " la copia no es igual");
                        if (Eq(log, taken, start) || Eq(log, start, taken)) failures.Add(key + " la copia perdio la mutacion");
                        undo();
                        if (!Eq(log, start, state) || !Eq(log, state, start)) failures.Add(key + " undo no restauro la igualdad");
                    }
                }

                log.Add(scenario.Name + " ranuras>0=" + (exercised > 0) + " fallos=[" + string.Join(" ;; ", failures.OrderBy(f => f, StringComparer.Ordinal)) + "]");
                expected.Add(scenario.Name + " ranuras>0=True fallos=[]");
            }

            Assert.Equal(expected, log);
        }
    }
}
