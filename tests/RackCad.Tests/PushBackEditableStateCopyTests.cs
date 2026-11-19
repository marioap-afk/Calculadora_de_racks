using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1b — the COPY and the COMPARER of the editable state (CT-PBT1B-06 to 10, 13 and 16): the census of its
    /// fields, the absence of aliases, the mutation of everything that is ordinary editable state, modules, tail and A/B,
    /// the comparer, the inputs of a recompute and the operations that must not touch the two references the Owner
    /// declared as shared (D2): the resolved rack and the module report.
    /// </summary>
    public sealed class PushBackEditableStateCopyTests
    {
        private static RackCatalog Catalog => PilotKit.Catalog;

        private static readonly PushBackEditorDesignAssembler Assembler = new PushBackEditorDesignAssembler(PilotKit.Catalog);
        private static readonly PushBackCompositeEditorAssembler CompositeAssembler = new PushBackCompositeEditorAssembler(PilotKit.Catalog);

        // ------------------------------------------------------------------------------------------------ helpers

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

        /// <summary>The comparer of the state, through the «EXCEPCION» rule (a comparer that throws is never «equal»).</summary>
        private static bool Eq(List<string> log, PushBackEditableState a, PushBackEditableState b)
            => Paso(log, () => PushBackEditableStateComparer.Instance.Equals(a, b));

        private static PushBackEditableState Copy(PushBackEditableState state) => PushBackEditableStateComparer.Copy(state);

        private static PushBackDesign DesignOfText(string envelope)
            => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(envelope).Design).PushBackDesign;

        private static string NameOfText(string envelope) => new RackEmbedStore().Deserialize(envelope).Name;

        private static string Persist(PushBackDesign design) => PilotKit.Persist(design);

        private static PushBackEditorComputation Pasada(PushBackEditableState s)
        {
            PushBackEditorComputation computation;
            try { computation = Assembler.BuildFrom(CompositeAssembler.BuildDesign(s.Editor, s.BuildInputs(Assembler)), PushBackSide.A); }
            catch (Exception) { return null; }
            if (!computation.IsValid) return computation;
            Assembler.AcceptComputation(s.Editor.SideA, computation);
            s.Editor.CommitDormantTailConsumption();
            var standardRestoreLanded = s.Editor.SideA.ModuleCommit?.StandardRestoreRequested == true;
            s.Editor.SideA.ClearModuleCommit();
            if (standardRestoreLanded) s.Editor.Active.RestoreAdvancedRackParameters(s.Advanced);
            _ = s.Editor.SideA.ModuleSession;
            return computation;
        }

        /// <summary>The persisted design a recompute pass gives, or a marker when the pass does not produce one.</summary>
        private static string Salida(PushBackEditableState s)
        {
            var computation = Pasada(s);
            return computation == null || !computation.IsValid ? "<sin diseno>" : Persist(computation.Design);
        }

        private static PushBackEditableState LoadState(List<string> log, PushBackDesign design, string name)
        {
            var state = new PushBackEditableState();
            Paso(log, () => state.LoadDesign(design, name, Assembler.Resolver));
            Pasada(state);
            return state;
        }

        private static PushBackEditableState Fixture(List<string> log, string name)
        {
            var text = PilotKit.Text(name);
            return LoadState(log, DesignOfText(text), NameOfText(text));
        }

        private static PushBackDesign F01Design(List<string> log)
        {
            var state = Fixture(log, "pb-20-ab");
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

        /// <summary>pb-20-ab with side B retired: the tail is parked by the recompute itself (ParkTailOf).</summary>
        private static PushBackEditableState Retired(List<string> log)
        {
            var state = Fixture(log, "pb-20-ab");
            Paso(log, () => state.Editor.SetSideBPresent(false));
            Pasada(state);
            return state;
        }

        private static PushBackEditableState X01(List<string> log)
        {
            var text = PilotKit.Text("pb-21-cola");
            var state = Fixture(log, "pb-21-cola");
            Paso(log, () => state.Editor.SetSideBPresent(true));
            Paso(log, () => state.Editor.SetActiveSelection(PushBackSideSelection.B));
            Paso(log, () => state.Editor.SetSlotPresent(PushBackSide.B, 0, true));
            var computation = Pasada(state);
            if (computation == null || !computation.IsValid) return state;
            return LoadState(log, new RackProjectStore().Deserialize(Persist(computation.Design)).PushBackDesign, NameOfText(text));
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

        private static DynamicRackModuleDesign TailModule(string id, double length)
            => new DynamicRackModuleDesign { ModuleId = id, Length = length, Notes = "cola" };

        /// <summary>The rich state of CT-PBT1B-06: F-01 with a staged edit and an accepted one, every carrier with a
        /// non-default value, topologies, safety selections of every family and a parked tail whose consumption is pending.</summary>
        private static PushBackEditableState Rich(List<string> log) => Enrich(log, F01(log));

        /// <summary>The second rich state of CT-PBT1B-06 (M04): pb-20-ab with side B retired, with the same carriers and a parked tail.</summary>
        private static PushBackEditableState RichRetired(List<string> log) => Enrich(log, Retired(log));

        private static PushBackEditableState Enrich(List<string> log, PushBackEditableState state)
        {
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
                new[] { TailModule("B:M2", 40.0) },
                new[] { new DynamicHeaderLineOverride { PostIndex = 1, ModuleId = "B:M6" } }));
            Paso(log, () => state.Editor.MarkDormantTailConsumed());
            return state;
        }

        // ------------------------------------------------------------------------------------- graph walks (CT-06, 07)

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
        private static HashSet<object> AllObjects(object root, Func<FieldInfo, bool> skip = null)
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
                    if (skip != null && skip(field)) continue;
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

        // ------------------------------------------------------------------------------------------ census (CT-06)

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

        /// <summary>Fields that only hold a part whose own fields are exercised one by one.</summary>
        private static readonly HashSet<string> ContainerKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "PushBackEditableState.Editor", "PushBackEditableState.Pallet", "PushBackEditableState.Advanced",
            "PushBackCompositeEditorState.SideA", "PushBackCompositeEditorState.SideB", "PushBackEditorState.structure",
            "PushBackEditorInputs.Pallet", "PushBackEditorInputs.Annotations",
            "SelectiveSafetySelection.BotaB", "SelectiveSafetySelection.tope", "SelectiveSafetySelection.desviador",
            "SelectiveSafetySelection.defensa", "SelectiveSafetySelection.guia", "SelectiveSafetySelection.parrilla", "SelectiveSafetySelection.bota",
        };

        /// <summary>CT-PBT1B-06 (5), v2.7: the slots that the mutator is not applied to, with their reason. (a) The mutator only
        /// changes the SHAPE of <c>DynamicEditorFront.LoadLevels</c>, <c>DynamicFrontMatrix.fronts</c>,
        /// <c>PushBackEditorFront.Cells</c> and <c>PushBackEditorState.pushFronts</c>, and leaves the matrix and the parallel
        /// push configuration misaligned: the copy goes through <c>Snapshot()</c> and <c>Restore()</c>, which realign them, so
        /// the copy is the aligned version and nothing is lost. (b) <c>SafetyFacePiece</c> is immutable and shared on purpose
        /// (private constructor, read-only properties, a single <c>None</c>, and <c>DeepCopy</c> shares it): mutating its fields
        /// by reflection changes the original, its copy and the copy at once and checks nothing; its replacement is exercised by
        /// the slots <c>NearFace</c> and <c>FarFace</c>.</summary>
        private static readonly HashSet<string> NotMutated = new HashSet<string>(StringComparer.Ordinal)
        {
            "DynamicEditorFront.LoadLevels", "DynamicFrontMatrix.fronts", "PushBackEditorFront.Cells", "PushBackEditorState.pushFronts",
            "SafetyFacePiece.ElementId", "SafetyFacePiece.IsNone",
        };

        /// <summary>The six advanced scopes are the only part of the <c>Advanced</c> carrier that the state uses; everything else
        /// of that carrier (and the annotation options it holds) is outside the state and is neither compared nor copied.</summary>
        private static readonly HashSet<string> AdvancedSix = new HashSet<string>(StringComparer.Ordinal)
        {
            "PushBackEditorInputs.ManualHeaderHeightOverride", "PushBackEditorInputs.DerivedPostReinforced",
            "PushBackEditorInputs.DerivedPostReinforcementHeight", "PushBackEditorInputs.DerivedPostHeight",
            "PushBackEditorInputs.SeparatorCountOverride", "PushBackEditorInputs.SeparatorSpacingOverride",
        };

        private static IReadOnlyList<FieldInfo> DiscoverFields()
        {
            var seen = new HashSet<Type>();
            var queue = new Queue<Type>();
            queue.Enqueue(typeof(PushBackEditableState));
            var fields = new List<FieldInfo>();
            while (queue.Count > 0)
            {
                var type = queue.Dequeue();
                if (!seen.Add(type)) continue;
                foreach (var field in InstanceFields(type))
                {
                    fields.Add(field);
                    if (Table.TryGetValue(KeyOf(field), out var kind) && kind != Class.E) continue;
                    foreach (var referenced in ReferencedTypes(field.FieldType))
                    {
                        if (IsProductType(referenced)) queue.Enqueue(referenced);
                    }
                }
            }

            return fields;
        }

        private static IEnumerable<Type> ReferencedTypes(Type type)
        {
            if (type.IsArray) { yield return type.GetElementType(); yield break; }
            if (type.IsGenericType)
            {
                foreach (var argument in type.GetGenericArguments())
                {
                    foreach (var nested in ReferencedTypes(argument)) yield return nested;
                }
            }

            yield return type;
        }

        /// <summary>The objects that own an Editable-class field, found by walking only Editable fields (and, of the
        /// <c>Advanced</c> carrier, only its six advanced scopes).</summary>
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

        // ----------------------------------------------------------------------------------------------- the mutator

        private sealed class NoReplacement { }

        /// <summary>Mutates one classified field of an instance in a way the comparer must notice, and undoes it.</summary>
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

        // ------------------------------------------------------------------------------------------------- CT-06

        [Fact]
        public void CT_PBT1B_06_EveryFieldOfTheStateIsClassifiedAndEveryEditableFieldSurvivesTheCopy()
        {
            var log = new List<string>();

            var discovered = DiscoverFields().Select(KeyOf).ToList();
            log.Add("sin-clasificar=[" + string.Join(", ", discovered.Where(key => !Table.ContainsKey(key)).Distinct().OrderBy(key => key, StringComparer.Ordinal)) + "]");
            log.Add("obsoletas=[" + string.Join(", ", Table.Keys.Except(discovered).OrderBy(key => key, StringComparer.Ordinal)) + "]");

            // The class D: exactly the two references D2 declares, with their types.
            var sharedFields = DiscoverFields().Where(field => Table.TryGetValue(KeyOf(field), out var kind) && kind == Class.D)
                .Select(field => KeyOf(field) + ":" + field.FieldType.Name).OrderBy(text => text, StringComparer.Ordinal);
            log.Add("clase-D=[" + string.Join(", ", sharedFields) + "]");

            var rich = Rich(log);
            var richRetired = RichRetired(log);
            var baseline = Paso(log, () => Copy(rich));
            log.Add("copia-inicial-igual=" + (baseline != null && Eq(log, baseline, rich)));
            var baselineRetired = Paso(log, () => Copy(richRetired));
            log.Add("copia-inicial-igual-pb-20-ab-sin-B=" + (baselineRetired != null && Eq(log, baselineRetired, richRetired)));

            var exercised = new HashSet<string>(StringComparer.Ordinal);
            var failures = new List<string>();
            var perState = new Dictionary<string, int>(StringComparer.Ordinal);
            var undone = 0;
            foreach (var (label, state, start) in new[] { ("F-01", rich, baseline), ("pb-20-ab-sin-B", richRetired, baselineRetired) })
            {
                perState[label] = 0;
                foreach (var (owner, field) in EditableSlots(state).ToList())
                {
                    var key = KeyOf(field);
                    if (NotMutated.Contains(key)) continue;
                    if (!TryMutate(owner, field, out var undo)) continue;
                    exercised.Add(key);
                    perState[label]++;
                    var copy = Paso(log, () => Copy(state));
                    if (copy == null) { failures.Add(label + " la copia no se pudo tomar: " + key); undo(); continue; }
                    if (!Eq(log, copy, state) || !Eq(log, state, copy)) failures.Add(label + " la copia no es igual: " + key);
                    if (start != null && (Eq(log, copy, start) || Eq(log, start, copy))) failures.Add(label + " la copia perdio la mutacion: " + key);

                    // undo() undoes it: the state is again equal to the copy taken before (as the pilot checks it).
                    undo();
                    undone++;
                    if (start == null || !Eq(log, start, state) || !Eq(log, state, start)) failures.Add(label + " undo no restauro la igualdad: " + key);
                }
            }

            // A reference to a part that holds more fields is exercised through those fields; the rest of the carrier
            // of the six advanced scopes (and the annotation options it holds) is not part of the state.
            var outside = new HashSet<string>(StringComparer.Ordinal)
            {
                "PushBackEditorInputs.Pallet", "PushBackEditorInputs.PalletsDeep", "PushBackEditorInputs.PostCatalogId", "PushBackEditorInputs.PostPeralte",
                "PushBackEditorInputs.PalletTolerance", "PushBackEditorInputs.FirstLevelDatum", "PushBackEditorInputs.BeamDepth",
                "PushBackEditorInputs.Annotations", "PushBackEditorInputs.SafetySelections",
            };
            var missing = Table.Where(pair => pair.Value == Class.E).Select(pair => pair.Key)
                .Where(key => !ContainerKeys.Contains(key) && !outside.Contains(key) && !NotMutated.Contains(key)
                              && !key.StartsWith("DynamicAnnotationOptions.", StringComparison.Ordinal))
                .Except(exercised).OrderBy(key => key, StringComparer.Ordinal).ToList();
            log.Add("campos-E-sin-ejercer=[" + string.Join(", ", missing) + "]");
            log.Add("fallos=[" + string.Join(" ;; ", failures.OrderBy(f => f, StringComparer.Ordinal)) + "]");
            log.Add("ranuras-ejercidas F-01>0=" + (perState["F-01"] > 0) + " pb-20-ab-sin-B>0=" + (perState["pb-20-ab-sin-B"] > 0));
            log.Add("undo-deshace-cada-mutacion=" + (undone > 0 && !failures.Any(failure => failure.Contains("undo no restauro"))));

            // The transient mark of the tail consumption is copied and not compared.
            var marked = Paso(log, () => Copy(rich));
            log.Add("marca-de-consumo-copiada=" + (marked != null && marked.Editor.DormantTailConsumptionPending == rich.Editor.DormantTailConsumptionPending && rich.Editor.DormantTailConsumptionPending));

            Assert.Equal(
                new[]
                {
                    "sin-clasificar=[]",
                    "obsoletas=[]",
                    "clase-D=[PushBackEditorState.LastModuleReconciliation:RackModuleReconciliationResult, PushBackEditorState.workingBaseline:PushBackSystem]",
                    "copia-inicial-igual=True",
                    "copia-inicial-igual-pb-20-ab-sin-B=True",
                    "campos-E-sin-ejercer=[]",
                    "fallos=[]",
                    "ranuras-ejercidas F-01>0=True pb-20-ab-sin-B>0=True",
                    "undo-deshace-cada-mutacion=True",
                    "marca-de-consumo-copiada=True",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-07

        [Fact]
        public void CT_PBT1B_07_ACopySharesNoMutableObjectWithItsOriginalExceptTheTwoDeclaredReferences()
        {
            var log = new List<string>();
            var expected = new List<string>();
            var scenarios = new List<(string Name, PushBackEditableState State)>
            {
                ("pb-06-seguridad", Fixture(log, "pb-06-seguridad")),
                ("pb-07-combinado", Fixture(log, "pb-07-combinado")),
                ("F-01", F01(log)),
                ("pb-20-ab-sin-B", Retired(log)),
                ("X-01", X01(log)),
            };

            foreach (var scenario in scenarios)
            {
                var original = scenario.State;
                var copy = Paso(log, () => Copy(original));
                if (copy == null) copy = new PushBackEditableState();

                // Part 1: the copy is equal, and what graph shares are exactly the non-null references of D2 and what they reach.
                var shared = Mutable(AllObjects(original).Intersect(AllObjects(copy), ReferenceEqualityComparer.Instance));
                var wanted = Mutable(SharedRoots(original).SelectMany(root => AllObjects(root)));
                log.Add(scenario.Name + " parte1 copia-igual=" + Eq(log, original, copy) + " compartidos-exactos=" + shared.SetEquals(wanted) + " raices>0=" + SharedRoots(original).Any());
                expected.Add(scenario.Name + " parte1 copia-igual=True compartidos-exactos=True raices>0=True");

                // Part 2, with no exemption: tail, commit and session of each side of the copy against the WHOLE graph of the original.
                var ownedByCopy = new List<object>();
                ownedByCopy.AddRange(copy.Editor.DormantCompositeTail);
                ownedByCopy.AddRange(copy.Editor.DormantTailLineOverrides);
                foreach (var side in new[] { copy.Editor.SideA, copy.Editor.SideB })
                {
                    if (side.ModuleCommit != null) ownedByCopy.Add(side.ModuleCommit);
                    ownedByCopy.Add(side.ModuleSession);
                }

                var reached = Mutable(ownedByCopy.SelectMany(root => AllObjects(root)));
                var common = reached.Intersect(AllObjects(original), ReferenceEqualityComparer.Instance).Count();
                log.Add(scenario.Name + " parte2 revisados>0=" + (reached.Count > 0) + " compartidos=" + common);
                expected.Add(scenario.Name + " parte2 revisados>0=True compartidos=0");
            }

            // Part 3 (M06): the editable content, the tail, the session and the commit of a state just loaded share no mutable
            // object with the design it was loaded from. They are measured from the state itself, WITHOUT subtracting what the
            // baseline reaches (only the two fields of the class D are not followed), and with a session and a commit that are
            // not null: after the pass a session is open, and one edit is staged and accepted and another one staged.
            foreach (var name in new[] { "pb-06-seguridad", "pb-07-combinado", "pb-20-ab", "pb-21-cola" })
            {
                var text = PilotKit.Text(name);
                var design = DesignOfText(text);
                var state = new PushBackEditableState();
                Paso(log, () => state.LoadDesign(design, NameOfText(text), Assembler.Resolver));
                Pasada(state);
                var first = Paso(log, () => state.Editor.SideA.ModuleSession.Modules.First());
                if (first != null)
                {
                    Paso(log, () => state.Editor.SideA.ModuleSession.SetLength(first.ModuleId, first.Length + 2.0));
                    Paso(log, () => state.Editor.SideA.CommitModuleEdits());
                    Paso(log, () => state.Editor.SideA.ModuleSession.SetLength(first.ModuleId, first.Length + 3.0));
                }

                var fromState = Mutable(AllObjects(state, field => Table.TryGetValue(KeyOf(field), out var kind) && kind == Class.D));
                var common = fromState.Intersect(AllObjects(design), ReferenceEqualityComparer.Instance).Count();
                var sessionOpen = state.Editor.SideA.ModuleSession != null;
                var commitOpen = state.Editor.SideA.ModuleCommit != null;
                log.Add(name + " parte3 revisados>0=" + (fromState.Count > 0) + " sesion-no-nula=" + sessionOpen + " commit-no-nulo=" + commitOpen
                    + " compartidos-con-el-diseno=" + common);
                expected.Add(name + " parte3 revisados>0=True sesion-no-nula=True commit-no-nulo=True compartidos-con-el-diseno=0");
            }

            Assert.Equal(expected, log);
        }

        // ------------------------------------------------------------------------------------------------- CT-08

        /// <summary>Everything that is ordinary editable state, mutated on <paramref name="target"/>; after each group, the
        /// other state must be equal to a copy taken before, compute the same design and hold the same objects.</summary>
        private static List<string> Battery(List<string> log, string scenario, string direction, PushBackEditableState target, PushBackEditableState other)
        {
            var lines = new List<string>();
            var failures = new List<string>();
            var before = Paso(log, () => Copy(other)) ?? new PushBackEditableState();
            var objects = AllObjects(other);
            var design = Salida(Paso(log, () => Copy(other)) ?? new PushBackEditableState());

            void Group(string group)
            {
                var intact = Eq(log, other, before) && Eq(log, before, other);
                var same = objects.SetEquals(AllObjects(other));
                var computes = Salida(Paso(log, () => Copy(other)) ?? new PushBackEditableState()) == design;
                lines.Add(scenario + " " + direction + " " + group + " intacto=" + (intact && same) + " diseno=" + computes);
                if (!(intact && same && computes)) failures.Add(group);
            }

            // Every editable slot, with the mutator, undone after each.
            // Except the fields of SafetyFacePiece (immutable and shared on purpose, v2.7): their replacement is exercised by
            // the slots NearFace and FarFace.
            foreach (var (owner, field) in EditableSlots(target).ToList())
            {
                if (field.DeclaringType == typeof(SafetyFacePiece)) continue;
                if (!TryMutate(owner, field, out var undo)) continue;
                if (!Eq(log, other, before) || !Eq(log, before, other) || !objects.SetEquals(AllObjects(other))) failures.Add("ranura " + KeyOf(field));
                undo();
            }

            Group("ranuras-E");

            // The module authority, through its API.
            var session = target.Editor.SideA.ModuleSession;
            var firstModule = session.Modules.FirstOrDefault();
            if (firstModule != null) session.SetLength(firstModule.ModuleId, firstModule.Length + 5.0);
            var header = target.Editor.SideA.WorkingBaseline?.Structure.Modules.FirstOrDefault(m => m.AssociatedFrameConfiguration != null && m.Kind != DynamicRackModuleKind.Separator);
            if (header != null) session.SetHeaderConfiguration(header.ModuleId, header.AssociatedFrameConfiguration);
            target.Editor.SideA.CommitModuleEdits();
            if (firstModule != null) session.SetLength(firstModule.ModuleId, firstModule.Length + 7.0);
            target.Editor.SideA.CancelModuleEdits();
            target.Editor.ParkDormantTail(new[] { TailModule("B:M9", 33.0) }, new[] { new DynamicHeaderLineOverride { PostIndex = 2, ModuleId = "B:M9" } });
            target.Editor.ClearDormantTail();
            Group("autoridad");

            // The composite intent and the selection, through the API of the composite state.
            target.Editor.SetGap(54.5);
            target.Editor.SetCentralSeparator(true);
            target.Editor.SetStructureOverride(PushBackSide.A, 5);
            target.Editor.ApplyTopology(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, DynamicRackCellScope.All);
            target.Editor.SetSideBPresent(!target.Editor.SideBPresent);
            target.Editor.SetSideBPresent(!target.Editor.SideBPresent);
            target.Editor.SetSlotCount(target.Editor.SlotCount + 1);
            target.Editor.SetSlotPresent(PushBackSide.B, 0, false);
            target.Editor.SetActiveSelection(PushBackSideSelection.Both);
            Group("intencion");

            // The carriers: every property of the state, and the safety selections.
            target.Pallet = new PalletSpecification(1.0, 2.0, 3.0, 4.0, "lb");
            target.PalletsDeep += 1;
            target.PostCatalogId = "OTRO";
            target.PostPeralte += 1.0;
            target.PalletTolerance += 1.0;
            target.BeamDepth += 1.0;
            target.AnnotationScale += 1.0;
            target.FirstLevelDatum = null;
            target.NumberFronts = !target.NumberFronts;
            target.NumberLevels = !target.NumberLevels;
            target.DrawRackName = !target.DrawRackName;
            target.Dimensions = target.Dimensions == DimensionDetail.None ? DimensionDetail.Detailed : DimensionDetail.None;
            target.DimensionStyle = "OTRO";
            target.LoadedDimensionViews = DimensionViewVisibility.Lateral;
            target.DimensionViewsTouched = !target.DimensionViewsTouched;
            target.DimensionsFrontal = !target.DimensionsFrontal;
            target.DimensionsLateral = !target.DimensionsLateral;
            target.DimensionsPlanta = !target.DimensionsPlanta;
            target.Advanced.ManualHeaderHeightOverride = 123.0;
            target.Advanced.DerivedPostReinforced = !target.Advanced.DerivedPostReinforced;
            target.Advanced.DerivedPostReinforcementHeight = 12.0;
            target.Advanced.DerivedPostHeight = 34.0;
            target.Advanced.SeparatorCountOverride = 9;
            target.Advanced.SeparatorSpacingOverride = 5.0;
            target.Name = "otro";
            target.SafetySelections.Add(Selection("N"));
            if (target.SafetySelections.Count > 1) target.SafetySelections[0].Quantity += 3;
            Group("portadores");

            lines.Add(scenario + " " + direction + " fallos=[" + string.Join(", ", failures.Distinct()) + "]");
            return lines;
        }

        [Fact]
        public void CT_PBT1B_08_MutatingAllOrdinaryEditableStateInTheCopyDoesNotChangeTheSource()
        {
            var log = new List<string>();
            var expected = new List<string>();
            var scenarios = new List<(string Name, Func<PushBackEditableState> Build)>
            {
                ("F-01", () => F01(log)),
                ("pb-20-ab-sin-B", () => Retired(log)),
                ("pb-06-seguridad", () => Fixture(log, "pb-06-seguridad")),
            };

            foreach (var scenario in scenarios)
            {
                foreach (var direction in new[] { "copia-no-toca-original", "original-no-toca-copia" })
                {
                    var original = scenario.Build();
                    var copy = Paso(log, () => Copy(original)) ?? new PushBackEditableState();
                    var lines = direction == "copia-no-toca-original"
                        ? Paso(log, () => Battery(log, scenario.Name, direction, copy, original))
                        : Paso(log, () => Battery(log, scenario.Name, direction, original, copy));
                    log.AddRange(lines ?? new List<string> { scenario.Name + " " + direction + " sin-bateria" });
                    foreach (var group in new[] { "ranuras-E", "autoridad", "intencion", "portadores" })
                    {
                        expected.Add(scenario.Name + " " + direction + " " + group + " intacto=True diseno=True");
                    }

                    expected.Add(scenario.Name + " " + direction + " fallos=[]");
                }
            }

            Assert.Equal(expected, log);
        }

        // ------------------------------------------------------------------------------------------------- CT-09

        [Fact]
        public void CT_PBT1B_09_ModulesTailAndSidesBehaveInTheCopyAsInTheOriginal()
        {
            var log = new List<string>();
            var expected = new List<string>();
            var scenarios = new List<(string Name, Func<PushBackEditableState> Build)>
            {
                ("F-01", () => F01(log)),
                ("pb-20-ab-sin-B", () => Retired(log)),
                ("X-01", () => X01(log)),
            };

            foreach (var scenario in scenarios)
            {
                // The copy computes the same design.
                var original = scenario.Build();
                var copy = Paso(log, () => Copy(original)) ?? new PushBackEditableState();
                log.Add(scenario.Name + " la-copia-calcula-el-mismo-diseno=" + (Salida(copy) == Salida(original)));
                expected.Add(scenario.Name + " la-copia-calcula-el-mismo-diseno=True");

                // Retiring B parks the tail and declaring B again brings it back, in the copy as in the original.
                var o2 = scenario.Build();
                var c2 = Paso(log, () => Copy(o2)) ?? new PushBackEditableState();
                foreach (var state in new[] { o2, c2 })
                {
                    Paso(log, () => state.Editor.SetSideBPresent(false));
                    Pasada(state);
                }

                log.Add(scenario.Name + " retirar-B copia-igual-que-original=" + (Salida(c2) == Salida(o2)));
                foreach (var state in new[] { o2, c2 })
                {
                    Paso(log, () => state.Editor.SetSideBPresent(true));
                }

                var designO = Salida(o2);
                var designC = Salida(c2);
                var tail = designO != "<sin diseno>" && new RackProjectStore().Deserialize(designO).PushBackDesign.Structure.Modules.Any(m => m.ModuleId.StartsWith("B:", StringComparison.Ordinal));
                log.Add(scenario.Name + " redeclarar-B copia-igual-que-original=" + (designC == designO) + " cola-de-B-intacta=" + tail);
                expected.Add(scenario.Name + " retirar-B copia-igual-que-original=True");
                expected.Add(scenario.Name + " redeclarar-B copia-igual-que-original=True cola-de-B-intacta=True");

                // Confirming the staged edit in the copy gives the same as in the original.
                var o3 = scenario.Build();
                var c3 = Paso(log, () => Copy(o3)) ?? new PushBackEditableState();
                foreach (var state in new[] { o3, c3 })
                {
                    Paso(log, () => state.Editor.SideA.CommitModuleEdits());
                }

                log.Add(scenario.Name + " confirmar-la-edicion copia-igual-que-original=" + (Salida(c3) == Salida(o3)));
                expected.Add(scenario.Name + " confirmar-la-edicion copia-igual-que-original=True");
            }

            Assert.Equal(expected, log);
        }

        // ------------------------------------------------------------------------------------------------- CT-10

        [Fact]
        public void CT_PBT1B_10_TheComparerSeparatesEveryChangeOfContentAndIgnoresTheSelection()
        {
            var log = new List<string>();

            // Two loads of the same design are equal.
            log.Add("dos-cargas-iguales=" + Eq(log, Fixture(log, "pb-02-modulos"), Fixture(log, "pb-02-modulos")));

            // An open session without changes and a null one are equal (I-03).
            var abierta = Fixture(log, "pb-02-modulos");
            var nula = Paso(log, () => Copy(abierta)) ?? new PushBackEditableState();
            Paso(log, () => nula.Editor.SideA.AdoptLoadedBaseline(nula.Editor.SideA.WorkingBaseline));
            log.Add("sesion-abierta-sin-cambios-vs-nula-iguales=" + Eq(log, abierta, nula) + "," + Eq(log, nula, abierta));

            // N-M-02: a session that keeps the confirmed list of an older baseline of the same signature is not «without changes».
            var anterior = Fixture(log, "pb-31-v2-modulos");
            Paso(log, () => { _ = anterior.Editor.SideA.ModuleSession; });
            anterior.Pallet = new PalletSpecification(anterior.Pallet.Front, 50.0, anterior.Pallet.Height, anterior.Pallet.Weight, anterior.Pallet.WeightUnit);
            var rebuilt = Pasada(anterior);
            // I02: the SAME name as the one of the envelope that `anterior` was loaded with, so the only difference is the session.
            var fresh = rebuilt == null ? new PushBackEditableState() : LoadState(log, rebuilt.Design, NameOfText(PilotKit.Text("pb-31-v2-modulos")));
            log.Add("sesion-de-una-linea-base-anterior-vs-cargada-distintos=" + !Eq(log, anterior, fresh));

            // Every change is told apart.
            var rich = F01(log);
            string Cambio(string label, Action<PushBackEditableState> change)
            {
                var copy = Paso(log, () => Copy(rich)) ?? new PushBackEditableState();
                Paso(log, () => change(copy));
                return label + " distintos=" + (!Eq(log, rich, copy) && !Eq(log, copy, rich));
            }

            log.Add(Cambio("contenido", c => c.Editor.SideA.RearTopeSaque += 1.0));
            log.Add(Cambio("cola", c => c.Editor.ParkDormantTail(new[] { TailModule("B:M9", 11.0) }, new DynamicHeaderLineOverride[0])));
            log.Add(Cambio("sesion-con-cambios", c => c.Editor.SideA.ModuleSession.SetLength(c.Editor.SideA.ModuleSession.Modules.First().ModuleId, 77.0)));
            log.Add(Cambio("commit", c => { c.Editor.SideA.ModuleSession.SetLength(c.Editor.SideA.ModuleSession.Modules.First().ModuleId, 78.0); c.Editor.SideA.CommitModuleEdits(); }));
            log.Add(Cambio("portador", c => c.Name = "otro"));
            log.Add(Cambio("unidad-del-fondo", c => c.Pallet = new PalletSpecification(c.Pallet.Front, c.Pallet.Depth, c.Pallet.Height, c.Pallet.Weight, "lb")));

            // The baseline with the same commit (M-15): the same state, with the baseline of another design of modules.
            var left = Fixture(log, "pb-02-modulos");
            var right = Paso(log, () => Copy(left)) ?? new PushBackEditableState();
            var other = DesignOfText(PilotKit.Text("pb-02-modulos"));
            var module = other.Structure.Modules.FirstOrDefault(m => m != null);
            if (module != null) module.Length += 3.0;
            Paso(log, () => right.Editor.SideA.AdoptLoadedBaseline(Assembler.Resolver.Resolve(other)));
            log.Add("linea-base-distinta-con-el-mismo-commit distintos=" + (!Eq(log, left, right) && !Eq(log, right, left)));

            // Only the selection: equal.
            var selected = Paso(log, () => Copy(rich)) ?? new PushBackEditableState();
            Paso(log, () => selected.Editor.SideA.ToggleCell(1, 0, extendSelection: false));
            Paso(log, () => selected.Editor.SetActiveSide(PushBackSide.B));
            log.Add("solo-la-seleccion iguales=" + (Eq(log, rich, selected) && Eq(log, selected, rich)));

            // NaN is reflexive, and null behaves as in the pilot.
            var nan = Paso(log, () => Copy(rich)) ?? new PushBackEditableState();
            nan.AnnotationScale = double.NaN;
            var nanCopy = Paso(log, () => Copy(nan)) ?? new PushBackEditableState();
            log.Add("NaN-reflexivo=" + (Eq(log, nan, nan) && Eq(log, nan, nanCopy)));
            log.Add("null=" + Eq(log, null, null) + "," + Eq(log, rich, null) + "," + Eq(log, null, rich));

            Assert.Equal(
                new[]
                {
                    "dos-cargas-iguales=True",
                    "sesion-abierta-sin-cambios-vs-nula-iguales=True,True",
                    "sesion-de-una-linea-base-anterior-vs-cargada-distintos=True",
                    "contenido distintos=True",
                    "cola distintos=True",
                    "sesion-con-cambios distintos=True",
                    "commit distintos=True",
                    "portador distintos=True",
                    "unidad-del-fondo distintos=True",
                    "linea-base-distinta-con-el-mismo-commit distintos=True",
                    "solo-la-seleccion iguales=True",
                    "NaN-reflexivo=True",
                    "null=True,False,False",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-13

        [Fact]
        public void CT_PBT1B_13_EveryCallOfBuildInputsGivesNewObjectsWithTheValuesOfTheState()
        {
            var log = new List<string>();
            var state = Rich(log);

            var first = Paso(log, () => state.BuildInputs(Assembler));
            var second = Paso(log, () => state.BuildInputs(Assembler));
            var shared = first == null || second == null
                ? -1
                : Mutable(AllObjects(first)).Intersect(AllObjects(second), ReferenceEqualityComparer.Instance).Count();
            log.Add("dos-llamadas-objetos-compartidos=" + shared);

            log.Add("seis-avanzados="
                + (first != null
                   && first.ManualHeaderHeightOverride == 150.5 && first.DerivedPostReinforced == false && first.DerivedPostReinforcementHeight == 60.5
                   && first.DerivedPostHeight == 140.5 && first.SeparatorCountOverride == 3 && first.SeparatorSpacingOverride == 18.5));
            log.Add("unidad-y-casillas="
                + (first != null && first.Pallet.WeightUnit == "lb" && first.Annotations.NumberFronts && first.Annotations.NumberLevels && first.Annotations.DrawRackName
                   && first.Annotations.DimensionStyle == "STYLE-1" && first.Annotations.Dimensions == DimensionDetail.Standard));

            // A confirmed «Restaurar estandar»: the assembler resets the inputs of the pass (DA:302), never the carrier.
            var restored = Fixture(log, "pb-02-modulos");
            restored.Advanced.ManualHeaderHeightOverride = 150.5;
            restored.Advanced.SeparatorCountOverride = 3;
            Paso(log, () => restored.Editor.SideA.ModuleSession.RequestStandardRestore());
            Paso(log, () => restored.Editor.SideA.CommitModuleEdits());
            var inputs = Paso(log, () => restored.BuildInputs(Assembler));
            Paso(log, () => CompositeAssembler.BuildDesign(restored.Editor, inputs));
            log.Add("restaurar-estandar Advanced-intacto=" + (restored.Advanced.ManualHeaderHeightOverride == 150.5 && restored.Advanced.SeparatorCountOverride == 3)
                + " inputs-de-la-pasada-reiniciados=" + (inputs != null && inputs.ManualHeaderHeightOverride == null && inputs.SeparatorCountOverride == null));

            Assert.Equal(
                new[]
                {
                    "dos-llamadas-objetos-compartidos=0",
                    "seis-avanzados=True",
                    "unidad-y-casillas=True",
                    "restaurar-estandar Advanced-intacto=True inputs-de-la-pasada-reiniciados=True",
                },
                log);
        }

        // ------------------------------------------------------------------------------------------------- CT-16

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

        /// <summary>The two shared references of every state it watches, each measured from the moment it is adopted.</summary>
        private sealed class Watch
        {
            private readonly List<(object Object, string Before)> items = new List<(object, string)>();

            public int Count => items.Count;

            public void Capture(params PushBackEditableState[] states)
            {
                foreach (var state in states)
                {
                    foreach (var reference in SharedRoots(state))
                    {
                        if (items.Any(item => ReferenceEquals(item.Object, reference))) continue;
                        items.Add((reference, Fingerprint(reference)));
                    }
                }
            }

            public bool Intact() => items.All(item => Fingerprint(item.Object) == item.Before);
        }

        private static void Edit(List<string> log, PushBackEditableState state, Watch watch, PushBackEditableState other)
        {
            void Step(Action action) { Paso(log, action); watch.Capture(state, other); }

            Step(() => Pasada(state));
            Step(() => state.Editor.SetSideBPresent(false));
            Step(() => Pasada(state));
            Step(() => state.Editor.SetSideBPresent(true));
            Step(() => Pasada(state));
            Step(() =>
            {
                var session = state.Editor.SideA.ModuleSession;
                var first = session.Modules.First();
                session.SetLength(first.ModuleId, first.Length + 4.0);
                state.Editor.SideA.CommitModuleEdits();
                session.SetLength(first.ModuleId, first.Length + 6.0);
                state.Editor.SideA.CancelModuleEdits();
            });
            Step(() => Pasada(state));
            Step(() => state.Editor.ParkDormantTail(new[] { TailModule("B:M9", 33.0) }, new[] { new DynamicHeaderLineOverride { PostIndex = 2, ModuleId = "B:M9" } }));
            Step(() => state.Editor.ClearDormantTail());
            Step(() => state.Editor.SetGap(54.5));
            Step(() => state.Editor.SetCentralSeparator(true));
            Step(() => state.Editor.SetStructureOverride(PushBackSide.A, 5));
            Step(() => state.Editor.ApplyTopology(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, DynamicRackCellScope.All));
            Step(() => state.Editor.SetSlotCount(state.Editor.SlotCount + 1));
            Step(() => state.Editor.SetSlotPresent(PushBackSide.B, 0, false));
            Step(() => state.Editor.SetActiveSelection(PushBackSideSelection.Both));
            Step(() =>
            {
                // Every carrier of D-PBT1-02 (M02): the pallet, the post, the tolerance, the beam, the scale, the datum, the three
                // boxes, the dimensions with their style and views and the three view boxes, the six advanced scopes, the name and
                // the safety selections (one added and one changed).
                state.Pallet = new PalletSpecification(1.0, 2.0, 3.0, 4.0, "lb");
                state.PalletsDeep += 1;
                state.PostCatalogId = "OTRO";
                state.PostPeralte += 1.0;
                state.PalletTolerance += 1.0;
                state.BeamDepth += 1.0;
                state.AnnotationScale += 1.0;
                state.FirstLevelDatum = null;
                state.NumberFronts = !state.NumberFronts;
                state.NumberLevels = !state.NumberLevels;
                state.DrawRackName = !state.DrawRackName;
                state.Dimensions = state.Dimensions == DimensionDetail.None ? DimensionDetail.Detailed : DimensionDetail.None;
                state.DimensionStyle = "OTRO";
                state.LoadedDimensionViews = DimensionViewVisibility.Lateral;
                state.DimensionViewsTouched = !state.DimensionViewsTouched;
                state.DimensionsFrontal = !state.DimensionsFrontal;
                state.DimensionsLateral = !state.DimensionsLateral;
                state.DimensionsPlanta = !state.DimensionsPlanta;
                state.Advanced.ManualHeaderHeightOverride = 123.0;
                state.Advanced.DerivedPostReinforced = !state.Advanced.DerivedPostReinforced;
                state.Advanced.DerivedPostReinforcementHeight = 12.0;
                state.Advanced.DerivedPostHeight = 34.0;
                state.Advanced.SeparatorCountOverride = 9;
                state.Advanced.SeparatorSpacingOverride = 5.0;
                state.Name = "otro";
                state.SafetySelections.Add(Selection("N"));
                if (state.SafetySelections.Count > 1) state.SafetySelections[0].Quantity += 3;
            });
            Step(() => Pasada(state));
        }

        private static void Validate(List<string> log, PushBackEditableState state, Watch watch, PushBackEditableState other)
        {
            void Step(Action action) { Paso(log, action); watch.Capture(state, other); }

            var rack = state.Editor.SideA.WorkingBaseline;
            Step(() => PushBackEditorOutputGate.Diagnostics(state.Editor, rack));
            Step(() => PushBackEditorOutputGate.IsBlocked(state.Editor, rack));
            Step(() => PushBackEditorOutputGate.StatusOr(state.Editor, rack, "RESERVA"));
            Step(() => state.Editor.IntentDiagnostics());
            Step(() =>
            {
                // An invalid pass: a reinforcement taller than the header makes the assembly throw.
                var before = state.Advanced.DerivedPostReinforcementHeight;
                var reinforced = state.Advanced.DerivedPostReinforced;
                state.Advanced.DerivedPostReinforced = true;
                state.Advanced.DerivedPostReinforcementHeight = 100000.0;
                try { Pasada(state); }
                finally { state.Advanced.DerivedPostReinforcementHeight = before; state.Advanced.DerivedPostReinforced = reinforced; }
            });
            Step(() => PushBackEditableState.SupportOf(PilotKitDesign()));
            Step(() => PushBackEditableState.ResolvePostCatalogId(Catalog, "POSTE-X"));
            Step(() => state.BuildInputs(Assembler));
            Step(() => RackModuleDescriptor.Describe(rack.Structure));
            Step(() => new PushBackSystemLateralBuilder().Cortes(rack, Catalog));
        }

        private static PushBackDesign PilotKitDesign() => DesignOfText(PilotKit.Text("pb-01-base"));

        private static void Reload(List<string> log, PushBackEditableState state, Watch watch, PushBackEditableState other)
        {
            void Step(Action action) { Paso(log, action); watch.Capture(state, other); }

            var text = PilotKit.Text("pb-20-ab");
            Step(() => state.LoadDesign(DesignOfText(text), NameOfText(text), Assembler.Resolver));
            Step(() => Pasada(state));
            Step(() => state.Restore(PilotKitDesign(), Assembler.Resolver));
            Step(() => Pasada(state));
            Step(() => state.LoadNew(Catalog));
            Step(() => Pasada(state));
        }

        [Fact]
        public void CT_PBT1B_16_NoOperationOfTheApiMutatesTheResolvedRackOrTheModuleReport()
        {
            var log = new List<string>();

            var rich = Rich(log);
            var copy = Paso(log, () => Copy(rich)) ?? new PushBackEditableState();
            var x01 = X01(log);

            // «editar» (conditions 3 and 5), on the original, on the copy and on X-01.
            var editing = new Watch();
            editing.Capture(rich, copy, x01);
            Edit(log, rich, editing, copy);
            Edit(log, copy, editing, rich);
            Edit(log, x01, editing, rich);
            log.Add("editar huellas-iguales=" + (editing.Count > 0 && editing.Intact()));

            // «validar» (condition 4).
            var richV = Rich(log);
            var copyV = Paso(log, () => Copy(richV)) ?? new PushBackEditableState();
            var x01V = X01(log);
            var validating = new Watch();
            validating.Capture(richV, copyV, x01V);
            Validate(log, richV, validating, copyV);
            Validate(log, copyV, validating, richV);
            Validate(log, x01V, validating, richV);
            log.Add("validar huellas-iguales=" + (validating.Count > 0 && validating.Intact()));

            // «descartar y recargar» (condition 6), and a copy that is discarded without touching it.
            var richD = Rich(log);
            var copyD = Paso(log, () => Copy(richD)) ?? new PushBackEditableState();
            var discarding = new Watch();
            discarding.Capture(richD, copyD);
            Reload(log, copyD, discarding, richD);
            Reload(log, richD, discarding, copyD);
            Paso(log, () => Copy(richD));
            log.Add("descartar-y-recargar huellas-iguales=" + (discarding.Count > 0 && discarding.Intact()));

            // «comparar» (condition 7): Equals in both directions and GetHashCode mutate nothing...
            var richC = Rich(log);
            var copyC = Paso(log, () => Copy(richC)) ?? new PushBackEditableState();
            var comparing = new Watch();
            comparing.Capture(richC, copyC);
            Paso(log, () => PushBackEditableStateComparer.Instance.Equals(richC, copyC));
            Paso(log, () => PushBackEditableStateComparer.Instance.Equals(copyC, richC));
            Paso(log, () => PushBackEditableStateComparer.Instance.GetHashCode(richC));
            Paso(log, () => PushBackEditableStateComparer.Instance.GetHashCode(copyC));
            log.Add("comparar huellas-iguales=" + (comparing.Count > 0 && comparing.Intact()));

            // M01 (condition 7 (ii)): the comparer reads of a baseline only the declared projection. Two DIFFERENT baselines, equal in
            // the projection: the name of one is mutated (outside the projection) and Equals does not change; as a control that the
            // check can fail, the length of a module (inside the projection) is mutated and Equals does change.
            var projA = Fixture(log, "pb-02-modulos");
            var projB = Paso(log, () => Copy(projA)) ?? new PushBackEditableState();
            Paso(log, () => projB.Editor.SideA.AdoptLoadedBaseline(Assembler.Resolver.Resolve(DesignOfText(PilotKit.Text("pb-02-modulos")))));
            var projBaselineA = projA.Editor.SideA.WorkingBaseline;
            var projBaselineB = projB.Editor.SideA.WorkingBaseline;
            var distinct = projBaselineA != null && projBaselineB != null && !ReferenceEquals(projBaselineA, projBaselineB);
            var projBefore = Eq(log, projA, projB) && Eq(log, projB, projA);
            if (projBaselineB != null) projBaselineB.Name = "mutado desde la prueba (fuera de la proyeccion)";
            var projOutside = Eq(log, projA, projB) && Eq(log, projB, projA);
            var projModule = projBaselineB?.Structure.Modules.FirstOrDefault();
            if (projModule != null) projModule.Length += 1.0;
            var projInside = Eq(log, projA, projB) || Eq(log, projB, projA);
            log.Add("comparar-proyeccion lineas-base-distintas=" + distinct + " iguales-antes=" + projBefore + " iguales-tras-mutar-el-nombre=" + projOutside
                + " iguales-tras-mutar-un-modulo=" + projInside);

            // ... and with two states that share the same baseline, the result does not change when the shared object is
            // mutated: neither outside the projection (its name) nor inside it (the length of a module).
            var sharedA = Rich(log);
            var sharedB = Paso(log, () => Copy(sharedA)) ?? new PushBackEditableState();
            var antes = Eq(log, sharedA, sharedB);
            var baseline = sharedA.Editor.SideA.WorkingBaseline;
            if (baseline != null)
            {
                baseline.Name = "mutado desde la prueba";
                var module = baseline.Structure.Modules.First();
                module.Length += 1.0;
            }

            var despues = Eq(log, sharedA, sharedB) && Eq(log, sharedB, sharedA);
            log.Add("comparar-invariante=" + (antes && despues && baseline != null));

            Assert.Equal(
                new[]
                {
                    "editar huellas-iguales=True",
                    "validar huellas-iguales=True",
                    "descartar-y-recargar huellas-iguales=True",
                    "comparar huellas-iguales=True",
                    "comparar-proyeccion lineas-base-distintas=True iguales-antes=True iguales-tras-mutar-el-nombre=True iguales-tras-mutar-un-modulo=False",
                    "comparar-invariante=True",
                },
                log);
        }
    }
}
