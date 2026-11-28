using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3a — the COMPOSITION of the full model (CT-PBT3A-06, 07 and 11): what is composed to write, the composition
    /// of «Conservar mi borrador» and the proof that none of the new operations mutates the two references D2 declares as
    /// shared (the resolved rack and the module report). Nothing is connected to the panel.
    /// </summary>
    public sealed class PushBackFullDraftCompositionTests
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

        private static PushBackDesign DesignOfText(string envelope)
            => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(envelope).Design).PushBackDesign;

        private static string NameOfText(string envelope) => new RackEmbedStore().Deserialize(envelope).Name;

        private static string Persist(PushBackDesign design) => PilotKit.Persist(design);

        private static PushBackEditorComputation Pasada(PushBackEditableState s)
            => PushBackEditableStateSeat.Pass(s, Assembler, CompositeAssembler, PushBackSide.A);

        private static string EnvelopeOf(string corpusText, PushBackDesign design)
        {
            var embed = new RackEmbedStore().Deserialize(corpusText);
            embed.Design = new RackProjectStore().Serialize(RackProject.ForPushBack(design));
            return new RackEmbedStore().Serialize(embed);
        }

        private static PushBackFullDraftState Full(List<string> log, string text)
        {
            var result = Paso(log, () => PushBackFullDraftMaterializer.Materialize(text, Catalog));
            return result == null || !result.IsAdmitted ? null : result.State;
        }

        private static PushBackFullDraftState Copy(List<string> log, PushBackFullDraftState state)
            => state == null ? null : Paso(log, () => PushBackFullDraftStateComparer.Copy(state));

        private static bool Eq(List<string> log, PushBackFullDraftState a, PushBackFullDraftState b)
            => Paso(log, () => PushBackFullDraftStateComparer.Instance.Equals(a, b));

        private static PushBackEditableState LoadState(List<string> log, PushBackDesign design, string name)
        {
            var state = new PushBackEditableState();
            Paso(log, () => state.LoadDesign(design, name, Assembler.Resolver));
            Pasada(state);
            return state;
        }

        /// <summary>F-01 (§5.0, Core recipe): pb-20-ab with the first separator of A and then the first one of B lengthened by 2.</summary>
        private static PushBackDesign F01Design(List<string> log)
        {
            var text = PilotKit.Text("pb-20-ab");
            var state = LoadState(log, DesignOfText(text), NameOfText(text));
            PushBackEditorComputation computation = null;
            foreach (var inB in new[] { false, true })
            {
                var separator = Paso(log, () => state.Editor.SideA.ModuleSession.Modules.First(
                    m => m.Kind == DynamicRackModuleKind.Separator && m.ModuleId.StartsWith("B:", StringComparison.Ordinal) == inB));
                if (separator == null) return null;
                Paso(log, () => state.Editor.SideA.ModuleSession.SetLength(separator.ModuleId, separator.Length + 2.0));
                Paso(log, () => state.Editor.SideA.CommitModuleEdits());
                computation = Pasada(state);
            }

            return computation == null ? null : new RackProjectStore().Deserialize(Persist(computation.Design)).PushBackDesign;
        }

        /// <summary>F-01 with an edit staged on top of an accepted one: a session with changes and a commit.</summary>
        private static PushBackEditableState F01(List<string> log)
        {
            var design = F01Design(log);
            var state = new PushBackEditableState();
            if (design == null) return state;
            state = LoadState(log, design, "F-01");
            var separator = Paso(log, () => state.Editor.SideA.ModuleSession.Modules.First(
                m => m.Kind == DynamicRackModuleKind.Separator && !m.ModuleId.StartsWith("B:", StringComparison.Ordinal)));
            if (separator == null) return state;
            Paso(log, () => state.Editor.SideA.ModuleSession.SetLength(separator.ModuleId, separator.Length + 1.0));
            Paso(log, () => state.Editor.SideA.CommitModuleEdits());
            Paso(log, () => state.Editor.SideA.ModuleSession.SetLength(separator.ModuleId, separator.Length + 3.0));
            return state;
        }

        /// <summary>X-01 (§5.0, Core recipe): the state of pb-21-cola with side B declared, selected and its slot 0 present.</summary>
        private static PushBackDesign X01Design(List<string> log)
        {
            var text = PilotKit.Text("pb-21-cola");
            var state = LoadState(log, DesignOfText(text), NameOfText(text));
            Paso(log, () => state.Editor.SetSideBPresent(true));
            Paso(log, () => state.Editor.SetActiveSelection(PushBackSideSelection.B));
            Paso(log, () => state.Editor.SetSlotPresent(PushBackSide.B, 0, true));
            var computation = Pasada(state);
            return computation == null || !computation.IsValid ? null : new RackProjectStore().Deserialize(Persist(computation.Design)).PushBackDesign;
        }

        private static SelectiveSafetySelection Selection(string tag)
        {
            var selection = new SelectiveSafetySelection
            {
                ElementId = "SAFETY-" + tag,
                Quantity = 2,
                Side = SafetySide.Left,
                LowEndOnly = true,
                AuthoredSide = SafetySide.Right,
                NearFace = SafetyFacePiece.Of("NEAR-" + tag),
                FarFace = SafetyFacePiece.Of("FAR-" + tag),
                BothEndsAreLoadFaces = true,
                DesviadorCellsAreByPost = true,
                BootSidesDeclared = true,
                Tope = new SelectiveTopeConfig { Shared = false, Fondo = 1, Saque = 2.5, Frontal = true },
                Desviador = new SelectiveDesviadorConfig { Longitud = 5.5, PrimerNivelAltura = 3.5 },
                Parrilla = new SelectiveParrillaConfig { Frontal = false, Lateral = false, Frente = 4.5, Cantidad = 2 },
            };
            selection.SecondLoadFacePosts.Add(1);
            selection.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Both });
            selection.DerivedAisles.Add(new DerivedAisleEntry { PostIndex = 2, Authored = SafetySide.Left });
            selection.Tope.OffCells.Add(new SelectiveGridCell { Frente = 1, Level = 2 });
            selection.Desviador.OffCells.Add(new SelectiveGridCell { Frente = 2, Level = 1 });
            selection.Guia.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            selection.Parrilla.OffCells.Add(new SelectiveGridCell { Frente = 3, Level = 0 });
            selection.Defensa.Posts.Add(new SafetyPostDefense { PostIndex = 1, ExitLength = 12.5, EntranceLength = 36.5, ExitAuto = true, EntranceAuto = true });
            selection.Bota.PieceId = "BOTA-" + tag;
            selection.Bota.Placement = BootPlacement.Rear;
            selection.Bota.Automatic = BootPlacement.Both;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 1, Placement = BootPlacement.EntryExit });
            selection.Bota.BlankPosts.Add(3);
            selection.BotaB.PieceId = "BOTA-B-" + tag;
            selection.BotaB.Placement = BootPlacement.EntryExit;
            selection.BotaB.Automatic = BootPlacement.Rear;
            selection.BotaB.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = BootPlacement.Rear });
            selection.BotaB.BlankPosts.Add(4);
            return selection;
        }

        /// <summary>The rich state of CT-PBT1B-06: F-01 with a staged edit and an accepted one, every carrier with a non-default
        /// value, safety selections of every family and a parked tail whose consumption is pending.</summary>
        private static PushBackEditableState Rich(List<string> log)
        {
            var state = F01(log);
            Paso(log, () => state.Editor.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.BToA));
            Paso(log, () => state.Editor.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB));
            Paso(log, () => state.Editor.SetCorridaDepth(0, 1, 3));
            state.Pallet = new PalletSpecification(40.0, 46.0, 58.0, 900.5, "lb");
            state.PalletsDeep = 5;
            state.PostCatalogId = "POSTE-X";
            state.PostPeralte = 3.25;
            state.PalletTolerance = 3.5;
            state.BeamDepth = 5.5;
            state.AnnotationScale = 1.75;
            state.FirstLevelDatum = 1;
            state.NumberFronts = true;
            state.NumberLevels = true;
            state.DrawRackName = true;
            state.Dimensions = DimensionDetail.Standard;
            state.DimensionStyle = "STYLE-1";
            state.LoadedDimensionViews = DimensionViewVisibility.Frontal | DimensionViewVisibility.Planta;
            state.DimensionViewsTouched = true;
            state.DimensionsFrontal = true;
            state.DimensionsLateral = true;
            state.DimensionsPlanta = true;
            state.Advanced.ManualHeaderHeightOverride = 150.5;
            state.Advanced.DerivedPostReinforced = false;
            state.Advanced.DerivedPostReinforcementHeight = 60.5;
            state.Advanced.DerivedPostHeight = 140.5;
            state.Advanced.SeparatorCountOverride = 3;
            state.Advanced.SeparatorSpacingOverride = 18.5;
            state.SafetySelections.Add(Selection("A"));
            state.SafetySelections.Add(Selection("B"));
            state.Name = "Rack rico";
            Paso(log, () => state.Editor.ParkDormantTail(
                new[] { new DynamicRackModuleDesign { ModuleId = "B:M2", Length = 40.0, Notes = "cola" } },
                new[] { new DynamicHeaderLineOverride { PostIndex = 1, ModuleId = "B:M6" } }));
            Paso(log, () => state.Editor.MarkDormantTailConsumed());
            return state;
        }

        // ------------------------------------------------------------------------------------------------- CT-06

        private static string Line(string label, PushBackFullWriteComposition composition)
            => label + " composed=" + composition.IsComposed + " reason=[" + composition.Reason + "]";

        private static string Rejection(List<string> log, string label, PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog)
        {
            var composition = Paso(log, () => PushBackFullComposition.ForFullWrite(initial, current, catalog));
            return composition == null ? label + " <sin resultado>" : Line(label, composition);
        }

        /// <summary>A composed case: the design of the composition against the one of a recompute pass over a copy of the current
        /// state, the name, that the gesture reached the design, and that the current state did not change (comparer and print).</summary>
        private static string ComposedLine(
            List<string> log, string label, PushBackFullDraftState initial, PushBackFullDraftState current, Func<PushBackFullWriteComposition, bool> reached)
        {
            var before = Copy(log, current);
            var printBefore = Paso(log, () => Fingerprint(current.Editable));
            var composition = Paso(log, () => PushBackFullComposition.ForFullWrite(initial, current, Catalog));
            if (composition == null || !composition.IsComposed) return label + " composed=False reason=[" + composition?.Reason + "]";

            var written = Paso(log, () => Persist(CompositeAssembler.BuildDesign(composition.Editor, composition.Inputs)));
            var passed = Paso(log, () =>
            {
                var computation = Pasada(Copy(log, current).Editable);
                return computation == null ? null : Persist(computation.Design);
            });
            var startDesign = Paso(log, () =>
            {
                var computation = Pasada(Copy(log, initial).Editable);
                return computation == null ? null : Persist(computation.Design);
            });
            var printAfter = Paso(log, () => Fingerprint(current.Editable));
            return label + " composed=True diseno-igual=" + (written != null && string.Equals(written, passed, StringComparison.Ordinal))
                + " nombre-igual=" + string.Equals(composition.Name, current.Editable.Name, StringComparison.Ordinal)
                + " cambio-llego=" + reached(composition) + " distinto-del-inicial=" + (written != null && !string.Equals(written, startDesign, StringComparison.Ordinal))
                + " current-intacto=" + (before != null && Eq(log, before, current) && Eq(log, current, before))
                + " huella-igual=" + (printBefore != null && string.Equals(printBefore, printAfter, StringComparison.Ordinal));
        }

        [Fact]
        public void CT_PBT3A_06_ForFullWriteComposesTheDraftOverAFreshBaseAndRejectsWhatItCannotWrite()
        {
            var log = new List<string>();
            var abText = PilotKit.Text("pb-20-ab");
            var initial = Full(log, abText);
            var baseline = Copy(log, initial);

            // The rejections.
            foreach (var (label, scale) in new[] { ("escala-NaN", double.NaN), ("escala-infinito", double.PositiveInfinity), ("escala-0", 0.0), ("escala--1", -1.0) })
            {
                var current = Copy(log, initial);
                if (current != null) current.Editable.AnnotationScale = scale;
                log.Add(Rejection(log, label, initial, current, Catalog));
            }

            var staleInitial = Copy(log, initial);
            if (staleInitial != null) staleInitial.Editable.PalletTolerance += 1;
            log.Add(Rejection(log, "base-cambiada", staleInitial, Copy(log, initial), Catalog));
            var unreadable = initial == null ? null : new PushBackFullDraftState(initial.Editable, "{ not json");
            log.Add(Rejection(log, "origen-ilegible", unreadable, Copy(log, initial), Catalog));
            log.Add(Rejection(log, "inicial-nulo", null, Copy(log, initial), Catalog));
            log.Add(Rejection(log, "catalogo-nulo", initial, Copy(log, initial), null));

            // The compositions: a module length, the gap of the composite rack and the name.
            var withModule = Copy(log, initial);
            if (withModule != null)
            {
                var separator = Paso(log, () => withModule.Editable.Editor.SideA.ModuleSession.Modules.First(
                    m => m.Kind == DynamicRackModuleKind.Separator && !m.ModuleId.StartsWith("B:", StringComparison.Ordinal)));
                if (separator != null)
                {
                    Paso(log, () => withModule.Editable.Editor.SideA.ModuleSession.SetLength(separator.ModuleId, separator.Length + 2.0));
                    Paso(log, () => withModule.Editable.Editor.SideA.CommitModuleEdits());
                }
            }

            log.Add(withModule == null ? "modulo <sin resultado>" : ComposedLine(log, "modulo", initial, withModule, composition => composition.Editor.SideA.ModuleSession != null));

            var withGap = Copy(log, initial);
            if (withGap != null) Paso(log, () => withGap.Editable.Editor.SetGap(30));
            log.Add(withGap == null ? "hueco <sin resultado>" : ComposedLine(log, "hueco", initial, withGap, composition => composition.Editor.Gap == 30));

            var withName = Copy(log, initial);
            if (withName != null) withName.Editable.Name = "PB-NUEVO";
            log.Add(withName == null ? "nombre <sin resultado>" : ComposedLine(log, "nombre", initial, withName, composition => composition.Name == "PB-NUEVO"));

            // The initial state was not touched by any of the cases.
            log.Add("inicial-intacto=" + (baseline != null && initial != null && Eq(log, baseline, initial)));

            var invalidScale = " composed=False reason=[" + PushBackPilotMaterializer.InvalidScaleReason + "]";
            Assert.Equal(
                new[]
                {
                    "escala-NaN" + invalidScale,
                    "escala-infinito" + invalidScale,
                    "escala-0" + invalidScale,
                    "escala--1" + invalidScale,
                    "base-cambiada composed=False reason=[" + PushBackPilotMaterializer.BaseChangedReason + "]",
                    "origen-ilegible composed=False reason=[" + PushBackPilotMaterializer.UnreadableOrFutureDiagnostic + "]",
                    "inicial-nulo composed=False reason=[Falta el estado inicial o el actual.]",
                    "catalogo-nulo composed=False reason=[" + PushBackPilotMaterializer.CatalogRequiredDiagnostic + "]",
                    "modulo composed=True diseno-igual=True nombre-igual=True cambio-llego=True distinto-del-inicial=True current-intacto=True huella-igual=True",
                    "hueco composed=True diseno-igual=True nombre-igual=True cambio-llego=True distinto-del-inicial=True current-intacto=True huella-igual=True",
                    "nombre composed=True diseno-igual=True nombre-igual=True cambio-llego=True distinto-del-inicial=False current-intacto=True huella-igual=True",
                    "inicial-intacto=True",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-07

        [Fact]
        public void CT_PBT3A_07_KeepOnKeepsTheEditableStateOfTheDraftOverTheNewOrigin()
        {
            var log = new List<string>();
            var baseText = PilotKit.Text("pb-01-base");
            var newOrigin = IntentKit.Rewrite(baseText, e => e.Name = "PB-OTRO");

            var newInitial = Full(log, newOrigin);
            var current = Full(log, baseText);
            if (current != null) current.Editable.PalletTolerance = 4.5;

            // The composition: the editable state of the draft and the origin of the new base; no argument changes.
            var newInitialBefore = Copy(log, newInitial);
            var currentBefore = Copy(log, current);
            var kept = newInitial == null || current == null ? null : Paso(log, () => PushBackFullComposition.KeepOn(newInitial, current));
            log.Add("keepon editable-de-current=" + (kept != null && current != null && PushBackEditableStateComparer.Instance.Equals(kept.Editable, current.Editable))
                + " origen-de-newInitial=" + (kept != null && newInitial != null && string.Equals(kept.Origin, newInitial.Origin, StringComparison.Ordinal)));
            log.Add("keepon argumentos-sin-cambios=" + (kept != null && Eq(log, newInitialBefore, newInitial) && Eq(log, currentBefore, current)));
            if (kept != null && current != null && newInitial != null)
            {
                var shared = Mutable(AllObjects(kept.Editable).Intersect(AllObjects(current.Editable), ReferenceEqualityComparer.Instance));
                var wanted = Mutable(SharedRoots(current.Editable).SelectMany(root => AllObjects(root)));
                var withNew = Mutable(AllObjects(kept.Editable).Intersect(AllObjects(newInitial.Editable), ReferenceEqualityComparer.Instance));
                log.Add("keepon compartidos-con-current-solo-d2=" + shared.IsSubsetOf(wanted) + " compartidos-con-newInitial=" + withNew.Count);
            }
            else
            {
                log.Add("keepon compartidos-con-current-solo-d2=False compartidos-con-newInitial=-1");
            }

            // The draft: a read of the full reader, a born draft, an edit that leaves it Dirty and «Conservar mi borrador».
            var scan = IntentKit.Scan(IntentKit.Rack, "A1", IntentKit.Member("A1", baseText));
            var read = Paso(log, () => PushBackFullDraftReader.Read(scan, Catalog));
            var registry = new DraftRegistry();
            var begun = read == null || !read.Read.IsAdmitted
                ? null
                : Paso(log, () => registry.Begin(DraftKit.S1, IntentKit.Rack, read.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy));
            var draft = begun == null ? null : begun.Draft;
            if (draft != null)
            {
                var edited = PushBackFullDraftStateComparer.Copy(draft.CurrentDraftState);
                edited.Editable.PalletTolerance = 4.5;
                Paso(log, () => draft.SetCurrentDraftState(edited));
            }

            var dirtyBefore = draft != null && draft.IsDirty;
            var reread = Paso(log, () => PushBackFullDraftReader.Read(scan, Catalog));
            var reload = draft == null || reread == null
                ? (DraftReloadResult?)null
                : Paso(log, () => (DraftReloadResult?)draft.KeepMineOnCurrentVersion(reread.Read, PushBackFullComposition.KeepOn, confirmed: true));
            log.Add("conservar dirty-antes=" + dirtyBefore + " estado=" + (reload.HasValue ? reload.Value.Status.ToString() : "<sin resultado>")
                + " dirty-despues=" + (draft != null && draft.IsDirty)
                + " base-nueva=" + (draft != null && reread != null && ReferenceEquals(draft.StaleBase, reread.Read.StaleBase))
                + " tolerancia-conservada=" + (draft != null && draft.CurrentDraftState.Editable.PalletTolerance == 4.5));

            Assert.Equal(
                new[]
                {
                    "keepon editable-de-current=True origen-de-newInitial=True",
                    "keepon argumentos-sin-cambios=True",
                    "keepon compartidos-con-current-solo-d2=True compartidos-con-newInitial=0",
                    "conservar dirty-antes=True estado=Reloaded dirty-despues=True base-nueva=True tolerancia-conservada=True",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-11

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

        private static bool IsImmutable(Type type)
        {
            if (type == typeof(string) || type.IsPrimitive || type.IsEnum) return true;
            if (!IsProductType(type)) return false;
            return InstanceFields(type).All(field => field.IsInitOnly && (field.FieldType.IsValueType || IsImmutable(field.FieldType)));
        }

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

        private static IEnumerable<object> SharedRoots(PushBackEditableState state)
        {
            foreach (var side in new[] { state.Editor.SideA, state.Editor.SideB })
            {
                if (side.WorkingBaseline != null) yield return side.WorkingBaseline;
                if (side.LastModuleReconciliation != null) yield return side.LastModuleReconciliation;
            }
        }

        /// <summary>The deep fingerprint of an object graph: every object with its type and each instance field (public,
        /// private and backing) with its value; numbers with «R» and the invariant culture, strings as they are,
        /// references by their index in the walk, lists and dictionaries in order.</summary>
        private static string Fingerprint(object root)
        {
            var index = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
            var order = new List<object>();
            int Idx(object value)
            {
                if (!index.TryGetValue(value, out var position))
                {
                    position = order.Count;
                    index[value] = position;
                    order.Add(value);
                }

                return position;
            }

            string Value(object value)
            {
                if (value == null) return "null";
                if (value is string text) return "\"" + text + "\"";
                if (value is double number) return number.ToString("R", CultureInfo.InvariantCulture);
                if (value is float single) return single.ToString("R", CultureInfo.InvariantCulture);
                if (value is Enum || value.GetType().IsPrimitive) return Convert.ToString(value, CultureInfo.InvariantCulture);
                if (value.GetType().IsValueType)
                {
                    return "{" + string.Join(",", value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Select(field => field.Name + "=" + Value(field.GetValue(value)))) + "}";
                }

                return "#" + Idx(value);
            }

            var builder = new StringBuilder();
            Idx(root);
            for (var position = 0; position < order.Count; position++)
            {
                var current = order[position];
                var type = current.GetType();
                builder.Append('#').Append(position).Append(' ').Append(type.FullName).Append('\n');
                if (current is IDictionary dictionary)
                {
                    foreach (DictionaryEntry entry in dictionary) builder.Append("  [").Append(Value(entry.Key)).Append("]=").Append(Value(entry.Value)).Append('\n');
                }
                else if (current is IEnumerable items && !(current is string))
                {
                    foreach (var item in items) builder.Append("  ").Append(Value(item)).Append('\n');
                }

                if (!IsProductType(type)) continue;
                foreach (var field in InstanceFields(type))
                {
                    builder.Append("  ").Append(field.Name).Append('=').Append(Value(field.GetValue(current))).Append('\n');
                }
            }

            using (var sha = SHA256.Create())
            {
                var hash = string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString())).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
                return hash + (root is PushBackSystem rack ? " Name=[" + rack.Name + "]" : string.Empty);
            }
        }

        /// <summary>The two shared references of every state it watches, each measured from the moment it is captured.</summary>
        private sealed class Watch
        {
            private readonly List<(object Object, string Before)> items = new List<(object, string)>();

            public int Count => items.Count;

            public void Capture(params PushBackFullDraftState[] states)
            {
                foreach (var state in states)
                {
                    if (state == null) continue;
                    foreach (var reference in SharedRoots(state.Editable))
                    {
                        if (items.Any(item => ReferenceEquals(item.Object, reference))) continue;
                        items.Add((reference, Fingerprint(reference)));
                    }
                }
            }

            public bool Intact() => items.All(item => Fingerprint(item.Object) == item.Before);
        }

        private static string Operation(List<string> log, string name, Watch watch, Action operation, params PushBackFullDraftState[] states)
        {
            watch.Capture(states);
            Paso(log, operation);
            return name + " huellas-iguales=" + (watch.Count > 0 && watch.Intact());
        }

        [Fact]
        public void CT_PBT3A_11_NoNewOperationMutatesTheResolvedRackOrTheModuleReport()
        {
            var log = new List<string>();
            var abText = PilotKit.Text("pb-20-ab");

            // The rich state of CT-PBT1B-06, and X-01, as full states: each carries its own origin text.
            var rich = new PushBackFullDraftState(Rich(log), abText);
            var x01Design = X01Design(log);
            var x01Text = x01Design == null ? null : EnvelopeOf(PilotKit.Text("pb-21-cola"), x01Design);
            var x01 = x01Text == null ? null : Full(log, x01Text);
            var initial = Full(log, abText);
            var members = new[] { new KeyValuePair<string, RackEmbedDocument>("L0", new RackEmbedDocument { Kind = RackEmbedDocument.KindPushBack, View = RackEmbedDocument.ViewLateral, Section = 0 }) };

            log.Add(Operation(log, "Copy", new Watch(), () =>
            {
                PushBackFullDraftStateComparer.Copy(rich);
                PushBackFullDraftStateComparer.Copy(x01);
            }, rich, x01));

            log.Add(Operation(log, "ForFullWrite", new Watch(), () =>
            {
                PushBackFullComposition.ForFullWrite(initial, rich, Catalog);
                PushBackFullComposition.ForFullWrite(initial, x01, Catalog);
            }, initial, rich, x01));

            log.Add(Operation(log, "KeepOn", new Watch(), () =>
            {
                PushBackFullComposition.KeepOn(initial, rich);
                PushBackFullComposition.KeepOn(x01, rich);
            }, initial, rich, x01));

            log.Add(Operation(log, "Classify", new Watch(), () =>
            {
                PushBackConflictClassifier.Classify(initial, initial, rich, members, Catalog);
                PushBackConflictClassifier.Classify(initial, rich, x01, members, Catalog);
            }, initial, rich, x01));

            log.Add(Operation(log, "Of", new Watch(), () =>
            {
                PushBackViewDispositions.Of(members, rich.Editable.Editor.SideA.WorkingBaseline, Catalog);
                PushBackViewDispositions.Of(members, x01.Editable.Editor.SideA.WorkingBaseline, Catalog);
            }, rich, x01));

            log.Add(Operation(log, "Equals", new Watch(), () =>
            {
                PushBackFullDraftStateComparer.Instance.Equals(rich, x01);
                PushBackFullDraftStateComparer.Instance.Equals(x01, rich);
                PushBackFullDraftStateComparer.Instance.GetHashCode(rich);
                PushBackFullDraftStateComparer.Instance.GetHashCode(x01);
            }, rich, x01));

            Assert.Equal(
                new[]
                {
                    "Copy huellas-iguales=True",
                    "ForFullWrite huellas-iguales=True",
                    "KeepOn huellas-iguales=True",
                    "Classify huellas-iguales=True",
                    "Of huellas-iguales=True",
                    "Equals huellas-iguales=True",
                },
                log);
        }
    }
}
