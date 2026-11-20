using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3b — the PREPARE of the write of the full model (CT-PBT3B-01 to 06, 10, 11 and 15): what the plan of an update
    /// writes for a draft of the full model, the views it redraws and the ones it erases, the name, the barrier of the diagnostics
    /// (also for the pilot), AUTH-13 on the write, the carry-over of the envelope and the order of the checks. Every call to the new
    /// API goes through <see cref="Paso{T}"/>, so on the inert skeleton a test fails by ASSERTION and never by an exception.
    /// </summary>
    public sealed class PushBackFullWritePrepareTests
    {
        private static RackCatalog Catalog => PilotKit.Catalog;

        private static string Rack => IntentKit.Rack;

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

        private static PushBackPilotPrepareResult PrepareFull(List<string> log, DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog)
            => Paso(log, () => PushBackPilotUpdatePlanner.PrepareFull(PushBackFullPrepareInput.From(draft), read, catalog));

        private static PushBackPilotPrepareResult PrepareFull(List<string> log, DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read)
            => PrepareFull(log, draft, read, Catalog);

        private static string Describe(PushBackPilotPrepareResult result)
            => result == null ? "<sin resultado>" : result.Kind + (result.Reason == null ? string.Empty : " motivo=[" + result.Reason + "]");

        private static string Keys(IEnumerable<string> keys) => "[" + string.Join(",", keys) + "]";

        private static string R(double value) => value.ToString("R", CultureInfo.InvariantCulture);

        private static bool IsReady(PushBackPilotPrepareResult result) => result != null && result.Kind == PushBackPilotPrepareKind.Ready;

        /// <summary>Edita el JSON INTERIOR de un sobre (la propiedad Design) y devuelve el texto del sobre completo.</summary>
        private static string EditInner(string envelopeText, Action<JsonNode> edit)
        {
            var envelope = JsonNode.Parse(envelopeText);
            var inner = JsonNode.Parse(envelope["Design"].GetValue<string>());
            edit(inner);
            envelope["Design"] = inner.ToJsonString();
            return envelope.ToJsonString();
        }

        private static JsonNode Navegar(JsonNode root, string path)
        {
            var node = root;
            foreach (var segment in path.Split('.'))
            {
                var bracket = segment.IndexOf('[');
                var name = bracket < 0 ? segment : segment.Substring(0, bracket);
                node = node?[name];
                while (bracket >= 0 && node != null)
                {
                    var close = segment.IndexOf(']', bracket);
                    node = node[int.Parse(segment.Substring(bracket + 1, close - bracket - 1), CultureInfo.InvariantCulture)];
                    bracket = segment.IndexOf('[', close);
                }
            }

            return node;
        }

        /// <summary>Num(json, ruta): el numero en esa ruta (bajo <c>PushBack</c>) del JSON persistido, con «R», o <c>&lt;null&gt;</c>.</summary>
        private static string Num(string json, string ruta)
        {
            var node = Navegar(JsonNode.Parse(json)["PushBack"], ruta);
            return node == null ? "<null>" : R(node.GetValue<double>());
        }

        /// <summary>Pone <paramref name="value"/> en la ruta (bajo <c>PushBack</c>) del JSON interior de un sobre.</summary>
        private static void Poner(JsonNode inner, string path, double value)
        {
            var cut = path.LastIndexOf('.');
            var parent = Navegar(inner["PushBack"], path.Substring(0, cut));
            parent[path.Substring(cut + 1)] = value;
        }

        private static string DesignOfMember(PushBackPilotMemberPlan member) => new RackEmbedStore().Deserialize(member.EnvelopeText).Design;

        private static string PayloadOf(string designText) => JsonNode.Parse(designText)["PushBack"].ToJsonString();

        private static string Raw(JsonElement? element) => element.HasValue ? element.Value.GetRawText() : "<null>";

        private static string Extra(RackEmbedDocument embed)
            => embed.ExtensionData == null
                ? "<none>"
                : string.Join(";", embed.ExtensionData.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Key + "=" + pair.Value.GetRawText()));

        private static PushBackPilotIntentScan ThreeViews(string fixture, int lateralPost, string origin)
            => IntentKit.Scan(
                Rack,
                origin,
                IntentKit.Member("A1", PrepareKit.Frontal(fixture)),
                IntentKit.Member("B2", PrepareKit.Lateral(lateralPost, fixture)),
                IntentKit.Member("C3", PrepareKit.Planta(fixture)));

        /// <summary>An admitted read of the full model that is FABRICATED over a scan (the internal constructor of the result): the base of the scan and
        /// the state that materializes the text of its origin. The reader itself does not admit some of what the recipes need (distinct names, a minor
        /// version of the envelope), and the base of a draft is whatever its read carried.</summary>
        private static PushBackFullIntentResult Fabricated(PushBackPilotIntentScan scan)
        {
            var staleBase = PushBackPilotIntentReader.BuildStaleBase(scan);
            var materialized = PushBackFullDraftMaterializer.Materialize(scan.RawEnvelopes[scan.OriginKey], Catalog);
            Assert.True(materialized.IsAdmitted, "the recipe does not materialize: " + materialized.Reason);
            return new PushBackFullIntentResult(staleBase, DraftIntentRead<PushBackFullDraftState>.Admitted(staleBase, materialized.State), scan.OriginKey, null, null);
        }

        private static void Finish(List<string> log, IEnumerable<string> expected)
            => Assert.Equal(string.Join("\n", expected), string.Join("\n", log));

        // ------------------------------------------------------------------------------------------------- CT-01

        private static readonly (string Name, string Path, double Base)[] FilasF =
        {
            ("F1", "Structure.PalletDepth", 48), ("F2", "Structure.PostPeralte", 3), ("F3", "Structure.PalletTolerance", 4),
            ("F4", "Structure.BeamDepth", 6), ("F5", "Structure.AnnotationScale", 1.5), ("F6", "Structure.Fronts[0].FirstLevelHeight", 4),
            ("F7", "Structure.Fronts[0].Levels[0].PalletFront", 42), ("F8", "Structure.Fronts[0].Levels[0].PalletHeight", 60),
            ("F9", "Structure.Fronts[0].Levels[0].PalletWeight", 1000), ("F10", "Structure.Fronts[0].Levels[0].ClearHeight", 6),
            ("F11", "Structure.Fronts[0].Levels[0].BeamLengthOverride", 100),
        };

        private static void Campos(List<string> log)
        {
            var scan = PrepareKit.RealScan();
            var read = FullWriteKit.Read(scan);
            var draft = FullWriteKit.DraftOf(read);
            FullWriteKit.Edit(draft, state =>
            {
                state.Editable.PalletTolerance = 4.5;
                var pallet = state.Editable.Pallet;
                state.Editable.Pallet = new PalletSpecification(pallet.Front, 48.5, pallet.Height, pallet.Weight, pallet.WeightUnit);
                state.Editable.Name = "PB-CAMPOS";
            });
            Assert.True(draft.IsDirty);

            var initial = draft.InitialDraftState;
            var current = PushBackFullDraftStateComparer.Copy(draft.CurrentDraftState);
            var first = PrepareFull(log, draft, read);
            var second = PrepareFull(log, draft, read);
            log.Add("prepare=" + Describe(first));
            if (!IsReady(first)) return;

            var plan = first.Plan;
            var composition = PushBackFullComposition.ForFullWrite(initial, current, Catalog);
            var composed = PayloadOf(PilotKit.Persist(new PushBackCompositeEditorAssembler(Catalog).BuildDesign(composition.Editor, composition.Inputs)));
            var outgoing = DesignOfMember(plan.Members[0]);
            log.Add("campos PalletTolerance=" + Num(outgoing, "Structure.PalletTolerance") + " PalletDepth=" + Num(outgoing, "Structure.PalletDepth"));

            foreach (var member in plan.Members)
            {
                var source = new RackEmbedStore().Deserialize(read.StaleBase.Entries[member.Key].EnvelopeText);
                var written = new RackEmbedStore().Deserialize(member.EnvelopeText);
                log.Add("miembro " + member.Key
                    + " id=" + (written.Id == source.Id)
                    + " kind=" + (written.Kind == source.Kind)
                    + " view=" + (written.View == source.View)
                    + " section=" + (written.Section == source.Section)
                    + " customprops=" + (Raw(written.CustomProperties) == Raw(source.CustomProperties))
                    + " cajon=" + (Extra(written) == Extra(source))
                    + " nombre=" + (written.Name == "PB-CAMPOS")
                    + " diseno=" + (PayloadOf(written.Design) == composed));
            }

            var sourceProject = JsonNode.Parse(new RackEmbedStore().Deserialize(read.StaleBase.Entries[plan.Members[0].Key].EnvelopeText).Design).AsObject();
            var writtenProject = JsonNode.Parse(outgoing).AsObject();
            var aside = sourceProject.Where(pair => pair.Key != "PushBack").Select(pair => pair.Key + "=" + pair.Value?.ToJsonString()).OrderBy(x => x, StringComparer.Ordinal);
            var written2 = writtenProject.Where(pair => pair.Key != "PushBack").Select(pair => pair.Key + "=" + pair.Value?.ToJsonString()).OrderBy(x => x, StringComparer.Ordinal);
            log.Add("proyecto version=" + (sourceProject["SchemaVersion"]?.ToJsonString() == writtenProject["SchemaVersion"]?.ToJsonString())
                + " cajon=" + aside.SequenceEqual(written2, StringComparer.Ordinal)
                + " carga version=" + (sourceProject["PushBack"]["SchemaVersion"]?.ToJsonString() == writtenProject["PushBack"]["SchemaVersion"]?.ToJsonString()));
            log.Add("base-del-borrador=" + plan.ExpectedBase.SequenceEqual(draft.StaleBase.Entries.Values.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                + " erasures=" + plan.Erasures.Count
                + " postorigen-es-origen=" + (plan.PostOriginKey == plan.OriginKey)
                + " planes-iguales=" + (second != null && second.Plan != null && plan.Equals(second.Plan)));
        }

        private static void FilaF(List<string> log, (string Name, string Path, double Base) fila)
        {
            var text = EditInner(PrepareKit.Frontal(), inner => Poner(inner, fila.Path, fila.Base + 0.00044));
            var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", text));
            var read = FullWriteKit.Read(scan);
            var draft = FullWriteKit.DraftOf(read);
            FullWriteKit.Edit(draft, state => state.Editable.Name = "PB-EXACTO");
            var result = PrepareFull(log, draft, read);
            var values = IsReady(result)
                ? result.Plan.Members.Select(member => Num(DesignOfMember(member), fila.Path)).Distinct().OrderBy(x => x, StringComparer.Ordinal)
                : Enumerable.Empty<string>();
            log.Add(fila.Name + " prepare=" + (result == null ? "<sin resultado>" : result.Kind.ToString()) + " diseno=" + Keys(values));
        }

        private static void FilaEstilo(List<string> log, string fixture)
        {
            var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", PrepareKit.Frontal(fixture)));
            var read = FullWriteKit.Read(scan);
            var draft = FullWriteKit.DraftOf(read);
            FullWriteKit.Edit(draft, state => state.Editable.Name = "PB-EXACTO");
            var result = PrepareFull(log, draft, read);
            if (!IsReady(result))
            {
                log.Add(fixture + " prepare=" + Describe(result));
                return;
            }

            var member = result.Plan.Members[0];
            var style = JsonNode.Parse(DesignOfMember(member))["PushBack"]["Structure"]["DimensionStyle"]?.GetValue<string>();
            var system = PushBackPilotSystemRebuild.BuildForMember(member, Catalog);
            log.Add(fixture + " prepare=Ready diseno=" + style + " sistema=" + system.Structure.DimensionStyle);
        }

        [Fact]
        public void CT_PBT3B_01_PrepareFullWritesTheFieldsTheNameAndTheDesignOfTheDraftOverTheEnvelopeThatWasReread()
        {
            var log = new List<string>();
            Paso(log, () => Campos(log));
            foreach (var fila in FilasF) Paso(log, () => FilaF(log, fila));
            foreach (var fixture in new[] { "pb-06-seguridad", "pb-07-combinado" }) Paso(log, () => FilaEstilo(log, fixture));

            Finish(
                log,
                new[]
                {
                    "prepare=Ready",
                    "campos PalletTolerance=4.5 PalletDepth=48.5",
                    "miembro A1 id=True kind=True view=True section=True customprops=True cajon=True nombre=True diseno=True",
                    "miembro B2 id=True kind=True view=True section=True customprops=True cajon=True nombre=True diseno=True",
                    "miembro C3 id=True kind=True view=True section=True customprops=True cajon=True nombre=True diseno=True",
                    "proyecto version=True cajon=True carga version=True",
                    "base-del-borrador=True erasures=0 postorigen-es-origen=True planes-iguales=True",
                    "F1 prepare=Ready diseno=[48.00044]",
                    "F2 prepare=Ready diseno=[3.00044]",
                    "F3 prepare=Ready diseno=[4.00044]",
                    "F4 prepare=Ready diseno=[6]",
                    "F5 prepare=Ready diseno=[1.50044]",
                    "F6 prepare=Ready diseno=[4.00044]",
                    "F7 prepare=Ready diseno=[42.00044]",
                    "F8 prepare=Ready diseno=[60.00044]",
                    "F9 prepare=Ready diseno=[1000.00044]",
                    "F10 prepare=Ready diseno=[6.00044]",
                    "F11 prepare=Ready diseno=[100.00044]",
                    "pb-06-seguridad prepare=Ready diseno=STANDARD-ISO sistema=STANDARD-ISO",
                    "pb-07-combinado prepare=Ready diseno=SIMPLEX sistema=SIMPLEX",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-02

        private static string FrontCase(List<string> log, string label, PushBackPilotIntentScan scan, Action<PushBackFullDraftState> edit)
        {
            var read = FullWriteKit.Read(scan);
            var draft = FullWriteKit.DraftOf(read);
            FullWriteKit.Edit(draft, edit);
            var result = PrepareFull(log, draft, read);
            if (!IsReady(result)) return label + " prepare=" + Describe(result);

            var plan = result.Plan;
            var known = read.StaleBase.Entries.Keys.ToList();
            return label + " prepare=Ready miembros=" + Keys(plan.Members.Select(member => member.Key)) + " borrados=" + Keys(plan.Erasures)
                + " origen=" + plan.OriginKey + " origen-post=" + plan.PostOriginKey
                + " claves-nuevas=" + Keys(plan.Members.Select(member => member.Key).Where(key => !known.Contains(key)));
        }

        [Fact]
        public void CT_PBT3B_02_TheFrontsRedrawEveryViewAndTheObsoleteLateralIsErasedWithTheOriginReanchored()
        {
            var log = new List<string>();
            Paso(log, () => log.Add(FrontCase(log, "mas-frente", PrepareKit.RealScan(),
                state => state.Editable.Editor.SetSlotCount(state.Editable.Editor.SlotCount + 1))));
            Paso(log, () => log.Add(FrontCase(log, "menos-frente-desde-planta", ThreeViews("pb-01-base", 3, "C3"),
                state => state.Editable.Editor.SetSlotCount(state.Editable.Editor.SlotCount - 1))));
            Paso(log, () => log.Add(FrontCase(log, "menos-frente-desde-lateral", ThreeViews("pb-01-base", 3, "B2"),
                state => state.Editable.Editor.SetSlotCount(state.Editable.Editor.SlotCount - 1))));
            Paso(log, () => log.Add(FrontCase(log, "solo-un-lateral", IntentKit.Scan(Rack, "B2", IntentKit.Member("B2", PrepareKit.Lateral(3))),
                state => state.Editable.Editor.SetSlotCount(state.Editable.Editor.SlotCount - 1))));

            Finish(
                log,
                new[]
                {
                    "mas-frente prepare=Ready miembros=[A1,B2,C3] borrados=[] origen=A1 origen-post=A1 claves-nuevas=[]",
                    "menos-frente-desde-planta prepare=Ready miembros=[A1,C3] borrados=[B2] origen=C3 origen-post=C3 claves-nuevas=[]",
                    "menos-frente-desde-lateral prepare=Ready miembros=[A1,C3] borrados=[B2] origen=B2 origen-post=A1 claves-nuevas=[]",
                    "solo-un-lateral prepare=Invalid motivo=[Todas las vistas del rack quedarían obsoletas: no se elimina el último vínculo del rack.]",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-03

        [Fact]
        public void CT_PBT3B_03_AModuleLengthOfEachSideReachesTheOutgoingDesignAndNoViewIsErased()
        {
            var log = new List<string>();
            Paso(log, () =>
            {
                var scan = IntentKit.Scan(
                    Rack, "A1", IntentKit.Member("A1", PrepareKit.Frontal("pb-20-ab")), IntentKit.Member("C3", PrepareKit.Planta("pb-20-ab")));
                var read = FullWriteKit.Read(scan);
                var draft = FullWriteKit.DraftOf(read);
                var changed = new List<string>();
                FullWriteKit.Edit(draft, state =>
                {
                    foreach (var inB in new[] { false, true })
                    {
                        var separator = state.Editable.Editor.SideA.ModuleSession.Modules.First(
                            m => m.Kind == DynamicRackModuleKind.Separator && m.ModuleId.StartsWith("B:", StringComparison.Ordinal) == inB);
                        var length = separator.Length + 2.0;
                        state.Editable.Editor.SideA.ModuleSession.SetLength(separator.ModuleId, length);
                        state.Editable.Editor.SideA.CommitModuleEdits();
                        changed.Add(separator.ModuleId + ":" + R(length));
                    }
                });

                var result = PrepareFull(log, draft, read);
                log.Add("modulos prepare=" + Describe(result));
                if (!IsReady(result)) return;

                var design = new RackProjectStore().Deserialize(DesignOfMember(result.Plan.Members[0])).PushBackDesign;
                var found = changed.Select(item =>
                {
                    var id = item.Substring(0, item.LastIndexOf(':'));
                    var module = design.Structure.Modules.FirstOrDefault(m => m.ModuleId == id);
                    return id + ":" + (module == null ? "<ausente>" : R(module.Length));
                });
                log.Add("modulos-en-el-diseno=" + (found.SequenceEqual(changed) ? "los-dos" : "distintos") + " borrados=" + Keys(result.Plan.Erasures));
            });

            Finish(log, new[] { "modulos prepare=Ready", "modulos-en-el-diseno=los-dos borrados=[]" });
        }

        // ------------------------------------------------------------------------------------------------- CT-04

        /// <summary>The composite rack of CT-PB0-10 («B ranura 1 activa»), persisted: A has its slots 1 and 2 blank and B has the slot 1.</summary>
        private static string CompositeBaseText()
        {
            var state = PushBackFullDraftMaterializer.Materialize(PilotKit.Text("pb-01-base"), Catalog).State.Editable;
            state.Editor.SetSlotPresent(PushBackSide.A, 1, false);
            state.Editor.SetSlotPresent(PushBackSide.A, 2, false);
            state.Editor.SetSideBPresent(true);
            state.Editor.SetSlotPresent(PushBackSide.B, 1, true);
            var pass = PushBackEditableStateSeat.Pass(state, new PushBackEditorDesignAssembler(Catalog), new PushBackCompositeEditorAssembler(Catalog), PushBackSide.A);
            var embed = new RackEmbedStore().Deserialize(PilotKit.Text("pb-01-base"));
            embed.Design = new RackProjectStore().Serialize(RackProject.ForPushBack(pass.Design));
            return new RackEmbedStore().Serialize(embed);
        }

        private static string Cut(string name, int section, bool composite)
        {
            var address = RackViewCodec.Decode(RackEmbedDocument.KindPushBack, RackEmbedDocument.ViewFrontal, section).Address;
            return RackViewBaseName.LinkedPushBackCut(name, address.Variant.PushBackEnd, address.Variant.PushBackSide, composite);
        }

        [Fact]
        public void CT_PBT3B_04_TheSideBIsComposedRejectedAsAFrontalOfAOneWayRackOrRemovedWithItsLaterals()
        {
            var log = new List<string>();

            // (i) pb-01-base with side B declared and a front of B active: the composite design; the frontal of A is redrawn.
            Paso(log, () =>
            {
                var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", PrepareKit.Frontal()), IntentKit.Member("C3", PrepareKit.Planta()));
                var read = FullWriteKit.Read(scan);
                var draft = FullWriteKit.DraftOf(read);
                FullWriteKit.Edit(draft, state =>
                {
                    state.Editable.Editor.SetSlotPresent(PushBackSide.A, 1, false);
                    state.Editable.Editor.SetSlotPresent(PushBackSide.A, 2, false);
                    state.Editable.Editor.SetSideBPresent(true);
                    state.Editable.Editor.SetSlotPresent(PushBackSide.B, 1, true);
                });
                var current = PushBackFullDraftStateComparer.Copy(draft.CurrentDraftState);
                var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(current.Editable.Editor, current.Editable.BuildInputs(new PushBackEditorDesignAssembler(Catalog)));
                var composite = new PushBackResolver(Catalog).Resolve(design).IsComposite;
                var result = PrepareFull(log, draft, read);
                if (!IsReady(result))
                {
                    log.Add("lado-B-declarado prepare=" + Describe(result));
                    return;
                }

                var desired = Paso(log, () => PushBackPilotUpdateRun.DesiredNamesOf(result.Plan, Catalog));
                var name = new RackEmbedStore().Deserialize(result.Plan.Members[0].EnvelopeText).Name;
                var wanted = Cut(name, 0, true);
                log.Add("lado-B-declarado prepare=Ready compuesto=" + composite
                    + " frontal-A-redibujado=" + result.Plan.Members.Any(member => member.Key == "A1")
                    + " nombre-con-lado=" + (desired != null && desired.TryGetValue("A1", out var actual) && actual == wanted && wanted != Cut(name, 0, false)));
            });

            // (ii) pb-20-ab with side B removed and a frontal of B among the members.
            Paso(log, () =>
            {
                var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", PrepareKit.Frontal("pb-20-ab")), IntentKit.Member("FB", PrepareKit.Frontal("pb-20-ab", 2)));
                var read = FullWriteKit.Read(scan);
                var draft = FullWriteKit.DraftOf(read);
                FullWriteKit.Edit(draft, state => state.Editable.Editor.SetSideBPresent(false));
                log.Add("lado-B-quitado-con-frontal-B prepare=" + Describe(PrepareFull(log, draft, read)));
            });

            // (iii) and (iv) the composite rack with its lateral of the post 2, which only exists because of the slot 1 of B, loses side B.
            Paso(log, () =>
            {
                var text = CompositeBaseText();
                string Variant(string view, int section) => IntentKit.Rewrite(text, e => { e.Id = Rack; e.View = view; e.Section = section; });
                var scan = IntentKit.Scan(
                    Rack, "A1",
                    IntentKit.Member("A1", Variant(RackEmbedDocument.ViewFrontal, 0)),
                    IntentKit.Member("B2", Variant(RackEmbedDocument.ViewLateral, 2)),
                    IntentKit.Member("C3", Variant(RackEmbedDocument.ViewPlanta, -1)));
                var read = FullWriteKit.Read(scan);
                var draft = FullWriteKit.DraftOf(read);
                FullWriteKit.Edit(draft, state => state.Editable.Editor.SetSideBPresent(false));
                var result = PrepareFull(log, draft, read);
                log.Add("lado-B-retirado prepare=" + (IsReady(result) ? "Ready miembros=" + Keys(result.Plan.Members.Select(m => m.Key)) + " borrados=" + Keys(result.Plan.Erasures) : Describe(result)));
                if (!IsReady(result)) return;

                var design = new RackProjectStore().Deserialize(DesignOfMember(result.Plan.Members[0])).PushBackDesign;
                log.Add("lado-B-retirado SideB-nulo=" + (design.SideB == null) + " AbsentSlotsB=" + Keys(design.Composite == null ? new string[0] : design.Composite.AbsentSlotsB.Select(slot => slot.ToString(CultureInfo.InvariantCulture))));
            });

            Finish(
                log,
                new[]
                {
                    "lado-B-declarado prepare=Ready compuesto=True frontal-A-redibujado=True nombre-con-lado=True",
                    "lado-B-quitado-con-frontal-B prepare=Invalid motivo=[La vista FB es un frontal del lado B en un rack sin lado B: el panel no la borra ni la redibuja; usa RACKEDITAR.]",
                    "lado-B-retirado prepare=Ready miembros=[A1,C3] borrados=[B2]",
                    "lado-B-retirado SideB-nulo=True AbsentSlotsB=[0,1,2]",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-05

        private static PushBackPilotIntentScan NamedViews(string origin)
            => IntentKit.Scan(
                Rack,
                origin,
                IntentKit.Member("A1", PrepareKit.Frontal("pb-01-base", 0, "Origen")),
                IntentKit.Member("B2", PrepareKit.Lateral(PrepareKit.ExistingPost(), "pb-01-base", "Otro-B")),
                IntentKit.Member("C3", PrepareKit.Planta("pb-01-base", "Otro-C")));

        private static string NamesOf(PushBackPilotUpdatePlan plan)
            => string.Join(" ", plan.Members.Select(member => member.Key + "=" + new RackEmbedStore().Deserialize(member.EnvelopeText).Name));

        [Fact]
        public void CT_PBT3B_05_TheNameOfTheDraftReachesEveryViewAndABlankNameTakesTheOneOfTheOrigin()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var scan = PrepareKit.RealScan();
                var read = FullWriteKit.Read(scan);
                var draft = FullWriteKit.DraftOf(read);
                FullWriteKit.Edit(draft, state => state.Editable.Name = "PB-NUEVO");
                var result = PrepareFull(log, draft, read);
                log.Add("nombre-nuevo prepare=" + (IsReady(result) ? "Ready " + NamesOf(result.Plan) : Describe(result)));
                if (!IsReady(result)) return;

                var desired = Paso(log, () => PushBackPilotUpdateRun.DesiredNamesOf(result.Plan, Catalog));
                var wanted = new Dictionary<string, string>
                {
                    ["A1"] = Cut("PB-NUEVO", 0, false),
                    ["B2"] = RackViewBaseName.LinkedLateral("PB-NUEVO", PrepareKit.ExistingPost()),
                    ["C3"] = RackViewBaseName.LinkedPlanta("PB-NUEVO"),
                };
                log.Add("nombres-deseados-del-clasico=" + (desired != null && desired.Count == 3 && wanted.All(pair => desired.TryGetValue(pair.Key, out var actual) && actual == pair.Value)));
            });

            // The composite design: the frontal carries the side in its desired name.
            Paso(log, () =>
            {
                var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", PrepareKit.Frontal("pb-20-ab")), IntentKit.Member("C3", PrepareKit.Planta("pb-20-ab")));
                var read = FullWriteKit.Read(scan);
                var draft = FullWriteKit.DraftOf(read);
                FullWriteKit.Edit(draft, state => state.Editable.Name = "PB-NUEVO");
                var result = PrepareFull(log, draft, read);
                var desired = IsReady(result) ? Paso(log, () => PushBackPilotUpdateRun.DesiredNamesOf(result.Plan, Catalog)) : null;
                log.Add("nombres-deseados-compuesto=" + (desired != null && desired.TryGetValue("A1", out var actual) && actual == Cut("PB-NUEVO", 0, true)
                    && desired.TryGetValue("C3", out var planta) && planta == RackViewBaseName.LinkedPlanta("PB-NUEVO")));
            });

            // A blank name: every view takes the name of the envelope that was re-read for the origin.
            Paso(log, () =>
            {
                var scan = NamedViews("A1");
                var read = Fabricated(scan);
                var draft = FullWriteKit.DraftOf(read);
                FullWriteKit.Edit(draft, state => state.Editable.Name = string.Empty);
                var result = PrepareFull(log, draft, read);
                log.Add("nombre-en-blanco prepare=" + (IsReady(result) ? "Ready " + NamesOf(result.Plan) : Describe(result)));
            });

            // The pilot keeps the name of each envelope that was re-read.
            Paso(log, () =>
            {
                var scan = NamedViews("A1");
                var read = PrepareKit.Reread(PushBackPilotIntentReader.BuildStaleBase(scan), PilotKit.Admitted(scan.RawEnvelopes["A1"]));
                var draft = PrepareKit.DraftOf(read);
                PrepareKit.SetScale(draft, 2.25);
                var result = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), read, Catalog);
                log.Add("piloto prepare=" + (result.Kind == PushBackPilotPrepareKind.Ready ? "Ready " + NamesOf(result.Plan) : Describe(result)));
            });

            Finish(
                log,
                new[]
                {
                    "nombre-nuevo prepare=Ready A1=PB-NUEVO B2=PB-NUEVO C3=PB-NUEVO",
                    "nombres-deseados-del-clasico=True",
                    "nombres-deseados-compuesto=True",
                    "nombre-en-blanco prepare=Ready A1=Origen B2=Origen C3=Origen",
                    "piloto prepare=Ready A1=Origen B2=Otro-B C3=Otro-C",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-06

        private static void Barrier(List<string> log, string label, string fixture, Action<PushBackFullDraftState> gesture)
        {
            var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", PrepareKit.Frontal(fixture)));
            var read = FullWriteKit.Read(scan);
            var draft = FullWriteKit.DraftOf(read);
            FullWriteKit.Edit(draft, gesture);
            var result = PrepareFull(log, draft, read);
            log.Add(label + " prepare=" + Describe(result));
        }

        [Fact]
        public void CT_PBT3B_06_TheBarrierOfTheDiagnosticsStopsABlockedDesignOfTheFullModelAndOfThePilot()
        {
            var log = new List<string>();

            // (i) the four blocking cases of CT-PBT1B-12, as drafts of the full model.
            Paso(log, () => Barrier(log, "hueco-negativo", "pb-01-base", state => state.Editable.Editor.SetGap(-1)));
            Paso(log, () => Barrier(log, "separador-sin-hueco", "pb-01-base", state => state.Editable.Editor.SetCentralSeparator(true)));
            Paso(log, () => Barrier(log, "override-no-construible", "pb-01-base", state => state.Editable.Editor.SetStructureOverride(PushBackSide.A, 1)));
            Paso(log, () =>
            {
                var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", PrepareKit.Frontal("pb-20-ab")));
                var read = FullWriteKit.Read(scan);
                var draft = FullWriteKit.DraftOf(read);
                FullWriteKit.Edit(draft, state => state.Editable.Editor.SetStructureOverride(PushBackSide.A, 2));
                var composition = PushBackFullComposition.ForFullWrite(draft.InitialDraftState, PushBackFullDraftStateComparer.Copy(draft.CurrentDraftState), Catalog);
                var system = new PushBackResolver(Catalog).Resolve(new PushBackCompositeEditorAssembler(Catalog).BuildDesign(composition.Editor, composition.Inputs));
                var first = PushBackCompositeDiagnostics.Evaluate(system).FirstOrDefault(diagnostic => diagnostic.IsBlocking);
                var result = PrepareFull(log, draft, read);
                log.Add("celda-no-cabe prepare=" + (result == null ? "<sin resultado>" : result.Kind.ToString())
                    + " estado-igual-al-primer-bloqueante-de-Evaluate="
                    + (first != null && result != null && result.Reason == "El diseño tiene un error que impide escribirlo: " + first.Message));
            });

            // (ii) a draft of the PILOT over pb-01-base with the manual structure of A at 1: the pilot admits it, and now the barrier stops it.
            Paso(log, () =>
            {
                var text = EditInner(PrepareKit.Frontal(), inner => inner["PushBack"]["Composite"]["StructureOverrideA"] = 1);
                var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", text));
                var read = IntentKit.Read(scan);
                var draft = PrepareKit.DraftOf(read);
                PrepareKit.SetScale(draft, 2.0);
                var composition = PushBackPilotMaterializer.ComposeForWrite(read.Read.InitialState, draft.CurrentDraftState, Catalog);
                var system = new PushBackResolver(Catalog).Resolve(new PushBackCompositeEditorAssembler(Catalog).BuildDesign(composition.Editor, composition.Inputs));
                var blocked = PushBackEditorOutputGate.IsBlocked(composition.Editor, system);
                var result = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), read, Catalog);
                log.Add("piloto-bloqueado admitido=" + read.Read.IsAdmitted + " bloqueado=" + blocked + " prepare=" + Describe(result));
            });

            // (iii) the designs of the corpus that the pilot admits are not blocked: the same edit gives Ready.
            var fixtures = new[]
            {
                "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado", "pb-08-redondeo",
                "pb-10-sin-datum", "pb-11-sin-i41", "pb-12-poste-nulo", "pb-13-sideb-no-presente", "pb-14-peralte-heredado", "pb-15-sin-composite",
                "pb-31-v2-modulos",
            };
            foreach (var fixture in fixtures)
            {
                Paso(log, () =>
                {
                    var text = PrepareKit.Frontal(fixture);
                    var admitted = PushBackPilotMaterializer.Materialize(text, Catalog).Kind == PushBackPilotMaterializationKind.Admitted;
                    var read = IntentKit.Read(IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", text)));
                    var draft = PrepareKit.DraftOf(read);
                    PrepareKit.SetScale(draft, fixture == "pb-02-modulos" ? 2.5 : 2.0);
                    var result = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), read, Catalog);
                    log.Add(fixture + " admitido=" + admitted + " prepare=" + Describe(result));
                });
            }

            const string blockedPrefix = "El diseño tiene un error que impide escribirlo: ";
            var expected = new List<string>
            {
                "hueco-negativo prepare=Invalid motivo=[" + blockedPrefix + "El hueco (-1\") no es una separacion fisica valida: debe ser cero o positivo. Corrigelo; no se interpreta como otro valor.]",
                "separador-sin-hueco prepare=Invalid motivo=[" + blockedPrefix + "El separador central necesita un hueco mayor que cero: sin el no hay donde colocarlo.]",
                "override-no-construible prepare=Invalid motivo=[" + blockedPrefix + "Lado A: la estructura manual (1) no es construible; el minimo fisico es 2 fondos. Corrige el valor o pulsa «Restaurar estructura» para volver a la propuesta.]",
                "celda-no-cabe prepare=Invalid estado-igual-al-primer-bloqueante-de-Evaluate=True",
                "piloto-bloqueado admitido=True bloqueado=True prepare=Invalid motivo=[El diseño tiene un error que impide escribirlo: Lado A: la estructura manual (1) no es construible; el minimo fisico es 2 fondos. Corrige el valor o pulsa «Restaurar estructura» para volver a la propuesta.]",
            };
            expected.AddRange(fixtures.Select(fixture => fixture + " admitido=True prepare=Ready"));
            Finish(log, expected);
        }

        // ------------------------------------------------------------------------------------------------- CT-10

        private static string Auth13Text(string path)
            => IntentKit.Rewrite(PilotKit.Text("pb-01-base"), e => e.Design = I58F1Fixtures.Set(I58F1Fixtures.Rich("pushback"), path + ".FutureNested", "1"));

        private static void Auth13(List<string> log, string label, string path)
        {
            var text = Auth13Text(path);
            var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", text));
            var materialized = PushBackFullDraftMaterializer.Materialize(text, Catalog);
            Assert.True(materialized.IsAdmitted, "the recipe does not materialize: " + materialized.Reason);

            // A draft born from an admitted read that was FABRICATED over the origin, with the base of that very scan.
            var fabricated = DraftIntentRead<PushBackFullDraftState>.Admitted(PushBackPilotIntentReader.BuildStaleBase(scan), materialized.State);
            var draft = FullWriteKit.DraftOf(fabricated);
            FullWriteKit.Edit(draft, state => state.Editable.AnnotationScale = state.Editable.AnnotationScale == 2.0 ? 2.5 : 2.0);
            var reread = FullWriteKit.Read(scan);
            var dirty = draft.IsDirty;
            log.Add(label + " base-igual=" + draft.StaleBase.CompareWith(reread.StaleBase).IsMatch + " dirty=" + dirty);

            var prepared = PrepareFull(log, draft, reread);
            log.Add(label + " prepare=" + (prepared == null ? "<sin resultado>" : prepared.Kind + (prepared.Reason == null ? string.Empty : " motivo=[" + prepared.Reason + "]")));

            var port = new PilotEraseScopeDouble(FullWriteKit.ItemsOf(scan));
            Assert.True(draft.TryBeginApply().IsStarted);
            var outcome = Paso(log, () => PushBackPilotUpdateRun.ExecuteFull(draft, new PushBackPilotUpdateRequest(100, Rack), port));
            log.Add(label + " corrida=" + (outcome == null ? "<sin resultado>" : outcome.Kind.ToString()) + " llamadas=[" + string.Join(",", port.Calls) + "]");
        }

        [Fact]
        public void CT_PBT3B_10_Auth13RejectsTheWriteOfADesignWithANestedUnknownFieldWithoutAnyCallOfWriting()
        {
            var log = new List<string>();
            Paso(log, () => Auth13(log, "modulo-anidado", "PushBack.Structure.Modules.0"));
            Paso(log, () => Auth13(log, "ladoB-anidado", "PushBack.SideB"));

            const string diagnostic = "No editable aquí: ilegible o de versión futura";
            Finish(
                log,
                new[]
                {
                    "modulo-anidado base-igual=True dirty=True",
                    "modulo-anidado prepare=NotAdmitted motivo=[" + diagnostic + "]",
                    "modulo-anidado corrida=NotAppliedAdmission llamadas=[Begin,ReadForPrepare,Dispose:scope]",
                    "ladoB-anidado base-igual=True dirty=True",
                    "ladoB-anidado prepare=NotAdmitted motivo=[" + diagnostic + "]",
                    "ladoB-anidado corrida=NotAppliedAdmission llamadas=[Begin,ReadForPrepare,Dispose:scope]",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-11

        [Fact]
        public void CT_PBT3B_11_TheEnvelopeThatWasRereadIsCarriedOverAndTheDraftKeepsItsDesignAndItsName()
        {
            var log = new List<string>();

            // D-PB-05, steps 5 to 7: the draft is kept over a new base with another custom properties, another minor version and another name.
            Paso(log, () =>
            {
                var read0 = FullWriteKit.Read(PrepareKit.RealScan());
                var draft = FullWriteKit.DraftOf(read0);
                FullWriteKit.Edit(draft, state =>
                {
                    state.Editable.PalletTolerance = 4.5;
                    state.Editable.Name = "PB-CAMPOS";
                });

                string Other(string text) => IntentKit.Rewrite(text, e =>
                {
                    e.Name = "PB-OTRO";
                    e.SchemaVersion = "1.1";
                    e.CustomProperties = JsonDocument.Parse("{\"Alfa\":1}").RootElement.Clone();
                });
                var read1 = Fabricated(IntentKit.Scan(
                    Rack,
                    "A1",
                    IntentKit.Member("A1", Other(PrepareKit.Frontal())),
                    IntentKit.Member("B2", Other(PrepareKit.Lateral(PrepareKit.ExistingPost()))),
                    IntentKit.Member("C3", Other(PrepareKit.Planta()))));
                var kept = draft.KeepMineOnCurrentVersion(read1.Read, PushBackFullComposition.KeepOn, confirmed: true);
                log.Add("conservar estado=" + kept.Status + " dirty=" + draft.IsDirty);

                var result = PrepareFull(log, draft, read1);
                log.Add("conservar prepare=" + Describe(result));
                if (!IsReady(result)) return;

                foreach (var member in result.Plan.Members)
                {
                    var source = new RackEmbedStore().Deserialize(read1.StaleBase.Entries[member.Key].EnvelopeText);
                    var written = new RackEmbedStore().Deserialize(member.EnvelopeText);
                    log.Add("conservar " + member.Key
                        + " customprops=" + (Raw(written.CustomProperties) == Raw(source.CustomProperties) && written.CustomProperties.HasValue)
                        + " version=" + (written.SchemaVersion == source.SchemaVersion && written.SchemaVersion == "1.1")
                        + " id=" + (written.Id == source.Id)
                        + " nombre-del-borrador=" + (written.Name == "PB-CAMPOS")
                        + " tolerancia-del-borrador=" + Num(DesignOfMember(member), "Structure.PalletTolerance"));
                }
            });

            // The top level, by construction: a re-read that was FABRICATED, whose envelope, project and payload carry an unknown field.
            Paso(log, () =>
            {
                var withFields = JsonNode.Parse(PrepareKit.Frontal()).AsObject();
                var inner = JsonNode.Parse(withFields["Design"].GetValue<string>()).AsObject();
                inner["Future"] = 1;
                inner["PushBack"]["Future"] = 1;
                withFields["Design"] = inner.ToJsonString();
                withFields["Future"] = 1;
                var text = withFields.ToJsonString();

                var scan = IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", text));
                var staleBase = PushBackPilotIntentReader.BuildStaleBase(scan);
                var materialized = PushBackFullDraftMaterializer.Materialize(text, Catalog);
                Assert.True(materialized.IsAdmitted, "the recipe does not materialize: " + materialized.Reason);
                var read = new PushBackFullIntentResult(staleBase, DraftIntentRead<PushBackFullDraftState>.Admitted(staleBase, materialized.State), "A1", null, null);
                var draft = FullWriteKit.DraftOf(read);
                FullWriteKit.Edit(draft, state => state.Editable.Name = "PB-CAMPOS");
                var result = PrepareFull(log, draft, read);
                if (!IsReady(result))
                {
                    log.Add("por-construccion prepare=" + Describe(result));
                    return;
                }

                var written = new RackEmbedStore().Deserialize(result.Plan.Members[0].EnvelopeText);
                var project = JsonNode.Parse(written.Design);
                log.Add("por-construccion sobre=" + (written.ExtensionData != null && written.ExtensionData.ContainsKey("Future"))
                    + " proyecto=" + (project["Future"] != null) + " carga=" + (project["PushBack"]["Future"] != null)
                    + " por-construccion-no-ejercido-por-AUTH-13");
            });

            Finish(
                log,
                new[]
                {
                    "conservar estado=Reloaded dirty=True",
                    "conservar prepare=Ready",
                    "conservar A1 customprops=True version=True id=True nombre-del-borrador=True tolerancia-del-borrador=4.5",
                    "conservar B2 customprops=True version=True id=True nombre-del-borrador=True tolerancia-del-borrador=4.5",
                    "conservar C3 customprops=True version=True id=True nombre-del-borrador=True tolerancia-del-borrador=4.5",
                    "por-construccion sobre=True proyecto=True carga=True por-construccion-no-ejercido-por-AUTH-13",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-15

        private static string Describe(PushBackPilotPrecheck check)
            => check == null ? "<sin resultado>" : check.Proceeds ? "Proceeds=True" : check.Kind + " motivo=" + check.Reason;

        [Fact]
        public void CT_PBT3B_15_ThePrecheckOfTheFullModelAndTheOrderOfPrepareFullAreTheOnesOfThePilot()
        {
            var log = new List<string>();
            Paso(log, () =>
            {
                var read = FullWriteKit.Read(PrepareKit.RealScan());
                DraftSession<PushBackFullDraftState> Born() => FullWriteKit.DraftOf(read);
                DraftSession<PushBackFullDraftState> Dirty()
                {
                    var draft = Born();
                    FullWriteKit.Edit(draft, state => state.Editable.PalletTolerance = 4.5);
                    Assert.True(draft.IsDirty);
                    return draft;
                }

                var pending = Born();
                pending.SetPendingText("name", "texto");
                var failed = Born();
                failed.EnterError("fallo");
                log.Add("precheck-limpio " + Describe(Paso(log, () => PushBackPilotUpdateRun.Precheck(Born()))));
                log.Add("precheck-texto-pendiente " + Describe(Paso(log, () => PushBackPilotUpdateRun.Precheck(pending))));
                log.Add("precheck-en-error " + Describe(Paso(log, () => PushBackPilotUpdateRun.Precheck(failed))));
                log.Add("precheck-sucio " + Describe(Paso(log, () => PushBackPilotUpdateRun.Precheck(Dirty()))));

                var other = DraftKit.Base(DraftKit.Mutable("D1"));
                PushBackFullIntentResult NotAdmitted(DraftStaleBase staleBase) => new PushBackFullIntentResult(
                    staleBase, DraftIntentRead<PushBackFullDraftState>.NotAdmitted("no admitida"), "A1", null, "pista");
                log.Add("limpio-relectura-distinta " + Describe(PrepareFull(log, Born(), NotAdmitted(other))));
                log.Add("sucio-relectura-distinta-no-admitida " + Describe(PrepareFull(log, Dirty(), NotAdmitted(other))));
                var same = Dirty();
                log.Add("sucio-misma-base-no-admitida " + Describe(PrepareFull(log, same, NotAdmitted(same.StaleBase))));
                log.Add("catalogo-nulo-tras-la-comparacion " + Describe(PrepareFull(log, Dirty(), read, null)));
            });

            Finish(
                log,
                new[]
                {
                    "precheck-limpio NothingToApply motivo=No hay cambios que aplicar.",
                    "precheck-texto-pendiente PendingText motivo=Hay texto pendiente: confírmalo antes de actualizar.",
                    "precheck-en-error NotApplicable motivo=El borrador no admite actualizarse en su estado actual.",
                    "precheck-sucio Proceeds=True",
                    "limpio-relectura-distinta NothingToApply",
                    "sucio-relectura-distinta-no-admitida Conflict",
                    "sucio-misma-base-no-admitida NotAdmitted motivo=[no admitida]",
                    "catalogo-nulo-tras-la-comparacion Invalid motivo=[Falta el borrador, la relectura o el catálogo para preparar la actualización.]",
                });
        }
    }
}
