#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4-T2A: shared builders of the pilot test classes. The corpus lives ONCE, in JSON envelope fixtures under
/// tests/RackCad.Tests/Fixtures/I64/, and both test projects read it by path from the repository root.
/// </summary>
internal static class PilotKit
{
    private static readonly Lazy<RackCatalog> LazyCatalog = new(() => JsonRackCatalogProvider.FromBaseDirectory().Load());

    public static RackCatalog Catalog => LazyCatalog.Value;

    /// <summary>The canonical single-sided designs: assembled by the product, so a round trip must give identity.</summary>
    public static readonly string[] Canonical =
    {
        "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado",
    };

    /// <summary>Older drawings, derived from pb-01-base by removing or changing what an older build did not write.</summary>
    public static readonly string[] Legacy =
    {
        "pb-10-sin-datum", "pb-11-sin-i41", "pb-12-poste-nulo", "pb-13-sideb-no-presente", "pb-14-peralte-heredado", "pb-15-sin-composite",
    };

    public static DirectoryInfo RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
        Assert.True(dir != null, "repo root (RackCad.sln) not found");
        return dir!;
    }

    public static string Text(string fixture)
    {
        var path = Path.Combine(RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64", fixture + ".json");
        Assert.True(File.Exists(path), "missing fixture " + path);
        return File.ReadAllText(path);
    }

    public static PushBackPilotDraftState Admitted(string text)
    {
        var result = PushBackPilotMaterializer.Materialize(text, Catalog);
        Assert.Equal(PushBackPilotMaterializationKind.Admitted, result.Kind);
        Assert.NotNull(result.State);
        return result.State!;
    }

    public static PushBackPilotDraftState State(string fixture) => Admitted(Text(fixture));

    public static void WithCulture(string name, Action body)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
            body();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>The inner design JSON of an envelope text.</summary>
    public static string DesignOf(string envelopeText) => new RackEmbedStore().Deserialize(envelopeText)!.Design;

    /// <summary>The persisted document a design becomes, through the same store the product uses.</summary>
    public static string Persist(PushBackDesign design) => new RackProjectStore().Serialize(RackProject.ForPushBack(design));

    /// <summary>
    /// The paths where two JSON documents differ, as "path: before -> after". A path that is on one side only is "&lt;absent&gt;",
    /// and a null against an array (or any other change of kind) is ONE difference at that path.
    /// </summary>
    public static List<string> JsonDiff(string before, string after)
    {
        var result = new List<string>();
        using var a = JsonDocument.Parse(before);
        using var b = JsonDocument.Parse(after);
        Diff(a.RootElement, b.RootElement, "$", result);
        return result;
    }

    private static void Diff(JsonElement a, JsonElement b, string path, List<string> into)
    {
        if (a.ValueKind == JsonValueKind.Object && b.ValueKind == JsonValueKind.Object)
        {
            var names = a.EnumerateObject().Select(p => p.Name).Concat(b.EnumerateObject().Select(p => p.Name)).Distinct().OrderBy(n => n, StringComparer.Ordinal);
            foreach (var name in names)
            {
                var hasA = a.TryGetProperty(name, out var left);
                var hasB = b.TryGetProperty(name, out var right);
                if (hasA && hasB) Diff(left, right, path + "." + name, into);
                else into.Add(path + "." + name + ": " + (hasA ? Short(left) : "<absent>") + " -> " + (hasB ? Short(right) : "<absent>"));
            }

            return;
        }

        if (a.ValueKind == JsonValueKind.Array && b.ValueKind == JsonValueKind.Array && a.GetArrayLength() == b.GetArrayLength())
        {
            var index = 0;
            foreach (var pair in a.EnumerateArray().Zip(b.EnumerateArray()))
            {
                Diff(pair.First, pair.Second, path + "[" + index++ + "]", into);
            }

            return;
        }

        if (a.ValueKind == JsonValueKind.Number && b.ValueKind == JsonValueKind.Number)
        {
            if (a.GetDouble() != b.GetDouble()) into.Add(path + ": " + Short(a) + " -> " + Short(b));
            return;
        }

        if (a.ValueKind != b.ValueKind || a.GetRawText() != b.GetRawText())
        {
            into.Add(path + ": " + Short(a) + " -> " + Short(b));
        }
    }

    private static string Short(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => "{object}",
        JsonValueKind.Array => "[" + element.GetArrayLength() + " items]",
        JsonValueKind.Number => element.GetDouble().ToString("R", CultureInfo.InvariantCulture),
        _ => element.GetRawText(),
    };
}

internal enum PilotFieldClass
{
    Editable,
    Selection,
    ModuleAuthority,
    Origin,
}

/// <summary>
/// I-64 F4-T2A (INV-F4T2A-04, 05): the reflection census of the pilot state. EVERY instance field (public, private and
/// backing) of the graph is classified into exactly one of four classes; a field that is not in the table fails the
/// census, and so does a table entry that no longer exists.
/// </summary>
internal static class PilotCensus
{
    private const PilotFieldClass E = PilotFieldClass.Editable;
    private const PilotFieldClass S = PilotFieldClass.Selection;
    private const PilotFieldClass A = PilotFieldClass.ModuleAuthority;
    private const PilotFieldClass O = PilotFieldClass.Origin;

    public static readonly IReadOnlyDictionary<string, PilotFieldClass> Table = new Dictionary<string, PilotFieldClass>(StringComparer.Ordinal)
    {
        ["PushBackPilotDraftState.Editor"] = E,
        ["PushBackPilotDraftState.Inputs"] = E,
        ["PushBackPilotDraftState.Origin"] = O,

        ["PushBackCompositeEditorState.topologies"] = E,
        ["PushBackCompositeEditorState.sideBEverDeclared"] = E,
        ["PushBackCompositeEditorState.DormantCompositeTail"] = A,
        ["PushBackCompositeEditorState.DormantTailLineOverrides"] = A,
        ["PushBackCompositeEditorState.dormantTailConsumed"] = A,
        ["PushBackCompositeEditorState.SideA"] = E,
        ["PushBackCompositeEditorState.SideB"] = E,
        ["PushBackCompositeEditorState.ActiveSide"] = S,
        ["PushBackCompositeEditorState.ActiveSelection"] = S,
        ["PushBackCompositeEditorState.SideBPresent"] = E,
        ["PushBackCompositeEditorState.Gap"] = E,
        ["PushBackCompositeEditorState.CentralSeparator"] = E,
        ["PushBackCompositeEditorState.StructureOverrideA"] = E,
        ["PushBackCompositeEditorState.StructureOverrideB"] = E,
        ["PushBackCompositeEditorState.DefaultTopology"] = E,
        ["PushBackCompositeEditorState.DefaultDirection"] = E,

        ["PushBackTopologyCell.Frente"] = E,
        ["PushBackTopologyCell.Level"] = E,
        ["PushBackTopologyCell.Topology"] = E,
        ["PushBackTopologyCell.Direction"] = E,
        ["PushBackTopologyCell.CorridaDepth"] = E,

        ["PushBackEditorState.structure"] = E,
        ["PushBackEditorState.pushFronts"] = E,
        ["PushBackEditorState.RearTopeSaque"] = E,
        ["PushBackEditorState.RearTopePieceId"] = E,
        ["PushBackEditorState.DefensePieceId"] = E,
        ["PushBackEditorState.workingBaseline"] = A,
        ["PushBackEditorState.moduleSession"] = A,
        ["PushBackEditorState.moduleCommit"] = A,
        ["PushBackEditorState.moduleSignature"] = A,
        ["PushBackEditorState.LastModuleReconciliation"] = A,

        ["DynamicFrontMatrix.fronts"] = E,
        ["DynamicFrontMatrix.selectedCells"] = S,
        ["DynamicFrontMatrix.selectedFrontIndex"] = S,
        ["DynamicFrontMatrix.selectedLevelIndex"] = S,

        ["DynamicEditorFront.Index"] = E,
        ["DynamicEditorFront.IsActive"] = E,
        ["DynamicEditorFront.PalletCount"] = E,
        ["DynamicEditorFront.LoadLevels"] = E,
        ["DynamicEditorFront.PalletsDeep"] = E,
        ["DynamicEditorFront.DepthStartPosition"] = E,
        ["DynamicEditorFront.FirstLevelHeight"] = E,
        ["DynamicEditorFront.Bfr"] = E,
        ["DynamicEditorFront.BeamLength"] = E,
        ["DynamicEditorFront.Cells"] = E,

        ["DynamicEditorCell.PalletFront"] = E,
        ["DynamicEditorCell.PalletHeight"] = E,
        ["DynamicEditorCell.PalletWeight"] = E,
        ["DynamicEditorCell.ClearHeight"] = E,
        ["DynamicEditorCell.InOutBeamCatalogId"] = E,
        ["DynamicEditorCell.InOutBeamDepth"] = E,
        ["DynamicEditorCell.BeamLengthOverride"] = E,
        ["DynamicEditorCell.IntermediateBeamCatalogId"] = E,
        ["DynamicEditorCell.IntermediateBeamDepth"] = E,

        ["PushBackEditorFront.Cells"] = E,
        ["PushBackEditorCell.HighEndBeamPeralte"] = E,
        ["PushBackEditorCell.RearTopeEnabled"] = E,
        ["PushBackEditorCell.PalletsDeepOverride"] = E,
        ["PushBackEditorCell.DrawPallet"] = E,

        ["PushBackEditorInputs.Pallet"] = E,
        ["PushBackEditorInputs.PalletsDeep"] = E,
        ["PushBackEditorInputs.PostCatalogId"] = E,
        ["PushBackEditorInputs.PostPeralte"] = E,
        ["PushBackEditorInputs.PalletTolerance"] = E,
        ["PushBackEditorInputs.FirstLevelDatum"] = E,
        ["PushBackEditorInputs.BeamDepth"] = E,
        ["PushBackEditorInputs.Annotations"] = E,
        ["PushBackEditorInputs.ManualHeaderHeightOverride"] = E,
        ["PushBackEditorInputs.DerivedPostReinforced"] = E,
        ["PushBackEditorInputs.DerivedPostReinforcementHeight"] = E,
        ["PushBackEditorInputs.DerivedPostHeight"] = E,
        ["PushBackEditorInputs.SeparatorCountOverride"] = E,
        ["PushBackEditorInputs.SeparatorSpacingOverride"] = E,
        ["PushBackEditorInputs.SafetySelections"] = E,

        ["PalletSpecification.Front"] = E,
        ["PalletSpecification.Depth"] = E,
        ["PalletSpecification.Height"] = E,
        ["PalletSpecification.Weight"] = E,
        ["PalletSpecification.WeightUnit"] = E,

        ["DynamicAnnotationOptions.NumberFronts"] = E,
        ["DynamicAnnotationOptions.NumberLevels"] = E,
        ["DynamicAnnotationOptions.DrawRackName"] = E,
        ["DynamicAnnotationOptions.AnnotationScale"] = E,
        ["DynamicAnnotationOptions.Dimensions"] = E,
        ["DynamicAnnotationOptions.DimensionViews"] = E,
        ["DynamicAnnotationOptions.DimensionStyle"] = E,

        ["SelectiveSafetySelection.ElementId"] = E,
        ["SelectiveSafetySelection.Quantity"] = E,
        ["SelectiveSafetySelection.Side"] = E,
        ["SelectiveSafetySelection.LowEndOnly"] = E,
        ["SelectiveSafetySelection.AuthoredSide"] = E,
        ["SelectiveSafetySelection.BotaB"] = E,
        ["SelectiveSafetySelection.BootSidesDeclared"] = E,
        ["SelectiveSafetySelection.DerivedAisles"] = E,
        ["SelectiveSafetySelection.NearFace"] = E,
        ["SelectiveSafetySelection.FarFace"] = E,
        ["SelectiveSafetySelection.BothEndsAreLoadFaces"] = E,
        ["SelectiveSafetySelection.SecondLoadFacePosts"] = E,
        ["SelectiveSafetySelection.DesviadorCellsAreByPost"] = E,
        ["SelectiveSafetySelection.PostSides"] = E,
        ["SelectiveSafetySelection.tope"] = E,
        ["SelectiveSafetySelection.desviador"] = E,
        ["SelectiveSafetySelection.defensa"] = E,
        ["SelectiveSafetySelection.guia"] = E,
        ["SelectiveSafetySelection.parrilla"] = E,
        ["SelectiveSafetySelection.bota"] = E,

        ["SelectiveBotaConfig.PieceId"] = E,
        ["SelectiveBotaConfig.Placement"] = E,
        ["SelectiveBotaConfig.Posts"] = E,
        ["SelectiveBotaConfig.Automatic"] = E,
        ["SelectiveBotaConfig.BlankPosts"] = E,
        ["BootPostPlacement.PostIndex"] = E,
        ["BootPostPlacement.Placement"] = E,
        ["DerivedAisleEntry.PostIndex"] = E,
        ["DerivedAisleEntry.Authored"] = E,
        ["SafetyFacePiece.IsNone"] = E,
        ["SafetyFacePiece.ElementId"] = E,
        ["SafetyPostSide.PostIndex"] = E,
        ["SafetyPostSide.Side"] = E,
        ["SelectiveTopeConfig.Shared"] = E,
        ["SelectiveTopeConfig.Fondo"] = E,
        ["SelectiveTopeConfig.Saque"] = E,
        ["SelectiveTopeConfig.Frontal"] = E,
        ["SelectiveTopeConfig.OffCells"] = E,
        ["SelectiveDesviadorConfig.Longitud"] = E,
        ["SelectiveDesviadorConfig.PrimerNivelAltura"] = E,
        ["SelectiveDesviadorConfig.OffCells"] = E,
        ["SelectiveDefensaConfig.Posts"] = E,
        ["SafetyPostDefense.PostIndex"] = E,
        ["SafetyPostDefense.ExitLength"] = E,
        ["SafetyPostDefense.EntranceLength"] = E,
        ["SafetyPostDefense.ExitAuto"] = E,
        ["SafetyPostDefense.EntranceAuto"] = E,
        ["SelectiveGuiaConfig.OffCells"] = E,
        ["SelectiveParrillaConfig.Frontal"] = E,
        ["SelectiveParrillaConfig.Lateral"] = E,
        ["SelectiveParrillaConfig.Frente"] = E,
        ["SelectiveParrillaConfig.Cantidad"] = E,
        ["SelectiveParrillaConfig.OffCells"] = E,
        ["SelectiveGridCell.Frente"] = E,
        ["SelectiveGridCell.Level"] = E,
    };

    public static string KeyOf(FieldInfo field)
    {
        var name = field.Name;
        if (name.StartsWith("<", StringComparison.Ordinal)) name = name.Substring(1, name.IndexOf('>') - 1);
        return field.DeclaringType!.Name + "." + name;
    }

    public static IEnumerable<FieldInfo> InstanceFields(Type type)
    {
        for (var current = type; current != null && current != typeof(object); current = current.BaseType)
        {
            foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                yield return field;
            }
        }
    }

    private static IEnumerable<Type> ReferencedTypes(Type type)
    {
        if (type.IsArray) { yield return type.GetElementType()!; yield break; }
        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var nested in ReferencedTypes(argument)) yield return nested;
            }
        }

        yield return type;
    }

    private static bool IsProductType(Type type) =>
        type.Namespace != null && type.Namespace.StartsWith("RackCad", StringComparison.Ordinal) && !type.IsEnum;

    /// <summary>
    /// Every instance field of the graph of the pilot state, found WITHOUT any classification as a stopping rule except
    /// the one the table itself declares: the fields of a type that only the module authority, the selection or the origin
    /// reach are not walked (they are classified as a whole).
    /// </summary>
    public static IReadOnlyList<FieldInfo> DiscoverFields()
    {
        var seen = new HashSet<Type>();
        var queue = new Queue<Type>();
        queue.Enqueue(typeof(PushBackPilotDraftState));
        var fields = new List<FieldInfo>();
        while (queue.Count > 0)
        {
            var type = queue.Dequeue();
            if (!seen.Add(type)) continue;
            foreach (var field in InstanceFields(type))
            {
                fields.Add(field);
                var classified = Table.TryGetValue(KeyOf(field), out var kind);
                if (classified && kind != PilotFieldClass.Editable) continue;
                foreach (var referenced in ReferencedTypes(field.FieldType))
                {
                    if (IsProductType(referenced)) queue.Enqueue(referenced);
                }
            }
        }

        return fields;
    }

    /// <summary>The objects of an instance graph that own an Editable-class field, found by walking only Editable fields.</summary>
    public static IEnumerable<(object Owner, FieldInfo Field)> EditableSlots(object root)
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
                if (!Table.TryGetValue(KeyOf(field), out var kind) || kind != PilotFieldClass.Editable) continue;
                yield return (owner, field);
                var value = field.GetValue(owner);
                if (value == null) continue;
                if (value is IEnumerable items && value is not string)
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

    /// <summary>The slot of one classified field, wherever the instance holds it first.</summary>
    public static (object Owner, FieldInfo Field)? FindSlot(object root, string key)
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
                if (KeyOf(field) == key) return (owner, field);
                if (!Table.TryGetValue(KeyOf(field), out var kind)) continue;
                var value = field.GetValue(owner);
                if (value == null) continue;
                if (value is IEnumerable items && value is not string)
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

        return null;
    }

    /// <summary>Every reference-type object an instance graph holds (collections included), by walking ALL fields.</summary>
    public static HashSet<object> AllObjects(object root)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var pending = new Queue<object>();
        pending.Enqueue(root);
        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            if (current is string || !visited.Add(current)) continue;
            if (current is IEnumerable items)
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

    /// <summary>A type is immutable when it is a string or every instance field of it is init-only and immutable.</summary>
    public static bool IsImmutable(Type type)
    {
        if (type == typeof(string) || type.IsPrimitive || type.IsEnum) return true;
        if (!IsProductType(type)) return false;
        return InstanceFields(type).All(field => field.IsInitOnly && (field.FieldType.IsValueType || IsImmutable(field.FieldType)));
    }
}

/// <summary>A state with EVERY classified field populated, so a census can exercise each one. Built by the editor's own API.</summary>
internal static class PilotRich
{
    public static PushBackPilotDraftState Build()
    {
        var editor = new PushBackCompositeEditorState();
        editor.SideA.LoadNew();
        editor.SideA.SetFrontCount(2);
        editor.SideA.AdjustLevels(0, 1);
        editor.SideA.RearTopeSaque = 4.25;
        editor.SideA.RearTopePieceId = "TOPE-A";
        editor.SideA.DefensePieceId = "DEFENSA-A";
        editor.SideA.Cell(0, 1).RearTopeEnabled = false;
        editor.SideA.Cell(1, 0).PalletsDeepOverride = 3;
        editor.SideA.Cell(1, 0).DrawPallet = true;
        editor.SideA.Cell(1, 0).HighEndBeamPeralte = 4.0;
        editor.SideA.Structure.Fronts[1].Bfr = 12.5;
        editor.SideA.Structure.Fronts[1].BeamLength = 90.5;
        editor.SideA.Structure.Fronts[1].Cells[0].BeamLengthOverride = 72.0;
        editor.SideA.Structure.Fronts[1].Cells[0].ClearHeight = 7.5;
        editor.SideA.ToggleCell(1, 0, extendSelection: false);

        editor.SetSideBPresent(true);
        editor.SideB.LoadNew();
        editor.SetSlotCount(2);
        editor.SetSlotPresent(PushBackSide.B, 0, true);
        editor.SetSlotPresent(PushBackSide.B, 1, true);
        editor.SideB.RearTopePieceId = "TOPE-B";
        editor.SideB.DefensePieceId = "DEFENSA-B";
        editor.SetGap(54.5);
        editor.SetCentralSeparator(true);
        editor.SetStructureOverride(PushBackSide.A, 5);
        editor.SetStructureOverride(PushBackSide.B, 6);
        editor.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.BToA);
        editor.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
        editor.SetCorridaDepth(0, 1, 3);
        editor.SetActiveSelection(PushBackSideSelection.Both);
        editor.SideA.ToggleCell(0, 1, extendSelection: true);
        editor.SetActiveSide(PushBackSide.B);

        // The module authority, populated: baseline, session, commit, signature, reconciliation and a dormant tail.
        var catalog = PilotKit.Catalog;
        var source = PilotKit.State("pb-02-modulos");
        editor.SideA.AdoptLoadedBaseline(new PushBackResolver(catalog).Resolve(
            new PushBackCompositeEditorAssembler(catalog).BuildDesign(source.Editor, source.Inputs)));
        _ = editor.SideA.ModuleSession;
        editor.SideA.CommitModuleEdits();
        editor.SideA.LastModuleReconciliation = (RackModuleReconciliationResult)RuntimeHelpers.GetUninitializedObject(typeof(RackModuleReconciliationResult));
        editor.ParkDormantTail(
            new[] { new DynamicRackModuleDesign { ModuleId = "B:M2", Length = 40.0 } },
            new[] { new DynamicHeaderLineOverride { PostIndex = 1, ModuleId = "B:M6" } });
        editor.MarkDormantTailConsumed();

        var inputs = new PushBackEditorInputs
        {
            Pallet = new PalletSpecification(40.0, 46.0, 58.0, 900.5, "lb"),
            PalletsDeep = 5,
            PostCatalogId = "POSTE-X",
            PostPeralte = 3.25,
            PalletTolerance = 3.5,
            FirstLevelDatum = 1,
            BeamDepth = 5.5,
            Annotations = new DynamicAnnotationOptions
            {
                NumberFronts = true, NumberLevels = true, DrawRackName = true, AnnotationScale = 1.75,
                Dimensions = DimensionDetail.Standard, DimensionViews = DimensionViewVisibility.Frontal | DimensionViewVisibility.Planta, DimensionStyle = "STYLE-1",
            },
            ManualHeaderHeightOverride = 150.5,
            DerivedPostReinforced = false,
            DerivedPostReinforcementHeight = 60.5,
            DerivedPostHeight = 140.5,
            SeparatorCountOverride = 3,
            SeparatorSpacingOverride = 18.5,
        };
        inputs.SafetySelections.Add(Selection("A"));
        inputs.SafetySelections.Add(Selection("B"));
        return new PushBackPilotDraftState(editor, inputs, "origin-text-A");
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
}

// I-64 F4-T2A-PILOT-STATE: INV-F4T2A-01 to 05 over the state, the adapter, the comparator and the copier of the pilot.
public class WorkspaceDraftPilotStateTests
{
    private static readonly PushBackPilotStateComparer Comparer = new();

    // ------------------------------------------------------------------ INV-F4T2A-01: the state groups what exists

    [Fact]
    public void TheStateGroupsTheExistingInstancesAndTheImmutableOrigin()
    {
        var editor = new PushBackCompositeEditorState();
        var inputs = PushBackEditorInputs.NewDesign();

        var state = new PushBackPilotDraftState(editor, inputs, "origin-text");

        Assert.Same(editor, state.Editor);
        Assert.Same(inputs, state.Inputs);
        Assert.Equal("origin-text", state.Origin);
        Assert.IsType<string>(state.Origin);
    }

    [Fact]
    public void TheStateRejectsMissingPartsInsteadOfBuildingAnInconsistentOne()
    {
        Assert.Throws<ArgumentNullException>(() => new PushBackPilotDraftState(null!, PushBackEditorInputs.NewDesign(), "o"));
        Assert.Throws<ArgumentNullException>(() => new PushBackPilotDraftState(new PushBackCompositeEditorState(), null!, "o"));
        Assert.Throws<ArgumentNullException>(() => new PushBackPilotDraftState(new PushBackCompositeEditorState(), PushBackEditorInputs.NewDesign(), null!));
    }

    [Fact]
    public void TheStateDefinesNoParallelModelPersistsNothingAndAssemblesNoDesign()
    {
        var type = typeof(PushBackPilotDraftState);
        var fields = PilotCensus.InstanceFields(type).Select(PilotCensus.KeyOf).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member.MemberType != MemberTypes.Constructor)
            .Select(member => member.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "PushBackPilotDraftState.Editor", "PushBackPilotDraftState.Inputs", "PushBackPilotDraftState.Origin" }, fields);
        Assert.Equal(new[] { "Editor", "Inputs", "Origin", "get_Editor", "get_Inputs", "get_Origin" }, members);
        Assert.All(type.GetProperties(), property => Assert.DoesNotContain(property.PropertyType, new[] { typeof(PushBackDesign), typeof(PushBackSystem) }));
        Assert.DoesNotContain(
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => !method.IsSpecialName && method.ReturnType == typeof(PushBackDesign));
    }

    // ------------------------------------------------------------------ INV-F4T2A-02: the one field and its validation

    private static readonly (string Text, bool Valid)[] ScaleCases =
    {
        ("1", true), ("1.5", true), ("1,5", true), (" 2 ", true), ("0.001", true), ("+3", true), ("1000000", true),
        ("", false), ("   ", false), ("abc", false), ("1.2.3", false), ("1,2,3", false), ("1 000", false), ("1,000.5", false),
        ("0", false), ("0.0", false), ("-1", false), ("-0.5", false), ("NaN", false), ("Infinity", false), ("-Infinity", false), ("1e999", false),
    };

    public static IEnumerable<object[]> Cultures() =>
        new[] { "es-MX", "en-US", "de-DE", "" }.Select(name => new object[] { name });

    [Theory]
    [MemberData(nameof(Cultures))]
    public void TheScaleValidationIsTheRuleOfTheClassicBoxInEveryCulture(string culture)
    {
        var adapter = new PushBackPilotFieldAdapter();

        PilotKit.WithCulture(culture, () =>
        {
            foreach (var (text, valid) in ScaleCases)
            {
                var verdict = adapter.Validate(PushBackPilotFieldAdapter.AnnotationScaleKey, text);
                Assert.True(valid == verdict.IsValid, $"[{culture}] '{text}' expected valid={valid}");
                Assert.Equal(valid, verdict.Message == null);
            }
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData("annotationscale")]
    [InlineData("ANNOTATIONSCALE")]
    [InlineData("AnnotationScale ")]
    [InlineData("Annotation")]
    [InlineData("RearTopeSaque")]
    [InlineData("name")]
    public void AnyKeyThatIsNotExactlyAnnotationScaleIsInvalidByOrdinalComparison(string key)
    {
        var verdict = new PushBackPilotFieldAdapter().Validate(key, "1.5");

        Assert.False(verdict.IsValid);
        Assert.False(string.IsNullOrEmpty(verdict.Message));
    }

    [Fact]
    public void TheKeyOfTheOneFieldIsExactlyAnnotationScale()
    {
        Assert.Equal("AnnotationScale", PushBackPilotFieldAdapter.AnnotationScaleKey);
        Assert.True(new PushBackPilotFieldAdapter().Validate("AnnotationScale", "1.5").IsValid);
    }

    // ------------------------------------------------------------------ INV-F4T2A-03: the adapter contract

    [Fact]
    public void ValidateMutatesNothing()
    {
        var state = PilotKit.State("pb-01-base");
        var before = PilotKit.State("pb-01-base");
        var adapter = new PushBackPilotFieldAdapter();

        adapter.Validate("AnnotationScale", "9.5");
        adapter.Validate("AnnotationScale", "x");
        adapter.Validate("Other", "9.5");

        Assert.True(Comparer.Equals(before, state));
        Assert.Equal(1.5, state.Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void ApplyReturnsTheSameObjectItReceivedWithTheScaleAppliedInPlace()
    {
        var state = PilotKit.State("pb-01-base");
        var editor = state.Editor;
        var inputs = state.Inputs;
        var annotations = state.Inputs.Annotations;
        var pending = new Dictionary<string, string> { ["AnnotationScale"] = "2.25" };

        var result = new PushBackPilotFieldAdapter().Apply(state, pending);

        Assert.Same(state, result);
        Assert.Same(editor, result.Editor);
        Assert.Same(inputs, result.Inputs);
        Assert.Same(annotations, result.Inputs.Annotations);
        Assert.Equal(2.25, state.Inputs.Annotations.AnnotationScale);
    }

    [Theory]
    [InlineData("es-MX", "2.5")]
    [InlineData("en-US", "2.5")]
    [InlineData("de-DE", "2,5")]
    [InlineData("", "2.5")]
    public void ApplyParsesTheTextInTheCurrentCulture(string culture, string text)
    {
        var state = PilotKit.State("pb-01-base");

        PilotKit.WithCulture(culture, () => new PushBackPilotFieldAdapter().Apply(state, new Dictionary<string, string> { ["AnnotationScale"] = text }));

        Assert.Equal(2.5, state.Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void ApplyLeavesTheStateAloneForAnyPendingTextThatIsNotTheOneValidField()
    {
        var state = PilotKit.State("pb-01-base");
        var adapter = new PushBackPilotFieldAdapter();

        adapter.Apply(state, new Dictionary<string, string>());
        adapter.Apply(state, new Dictionary<string, string> { ["Other"] = "9" });
        adapter.Apply(state, new Dictionary<string, string> { ["AnnotationScale"] = "not a number" });
        adapter.Apply(state, new Dictionary<string, string> { ["AnnotationScale"] = "-4" });

        Assert.Equal(1.5, state.Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void TheAdapterKeepsNoReferenceToAnyState()
    {
        var instanceFields = PilotCensus.InstanceFields(typeof(PushBackPilotFieldAdapter)).ToList();
        Assert.DoesNotContain(instanceFields, field => ContainsStateType(field.FieldType));
        Assert.Empty(instanceFields);

        var one = PilotKit.State("pb-01-base");
        var two = PilotKit.State("pb-02-modulos");
        var adapter = new PushBackPilotFieldAdapter();
        adapter.Apply(one, new Dictionary<string, string> { ["AnnotationScale"] = "3" });
        adapter.Apply(two, new Dictionary<string, string> { ["AnnotationScale"] = "4" });
        Assert.Equal(3.0, one.Inputs.Annotations.AnnotationScale);
        Assert.Equal(4.0, two.Inputs.Annotations.AnnotationScale);
        Assert.Empty(PilotCensus.InstanceFields(typeof(PushBackPilotFieldAdapter)));
    }

    private static bool ContainsStateType(Type type) =>
        type == typeof(PushBackPilotDraftState)
        || type == typeof(PushBackCompositeEditorState)
        || type == typeof(PushBackEditorState)
        || type == typeof(PushBackEditorInputs)
        || (type.IsGenericType && type.GetGenericArguments().Any(ContainsStateType))
        || (type.IsArray && ContainsStateType(type.GetElementType()!));

    [Fact]
    public void TheAdapterRunsInsideTheTwoPhaseCommitOfTheDraftCore()
    {
        var registry = new DraftRegistry();
        var read = DraftIntentRead<PushBackPilotDraftState>.Admitted(DraftKit.DefaultBase(), PilotKit.State("pb-01-base"));
        var draft = registry.Begin(DraftKit.S1, "R1", read, Comparer, PushBackPilotStateComparer.Copy).Draft!;
        draft.SetPendingText("AnnotationScale", "2.25");
        draft.SetPendingText("Other", "1");

        var rejected = draft.CrossBoundary(DraftBoundary.TabChange, new PushBackPilotFieldAdapter());
        Assert.Equal(DraftBoundaryDecision.Aborted, rejected.Decision);
        Assert.Equal("Other", Assert.Single(rejected.InvalidFields).FieldKey);
        Assert.Equal(1.5, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);

        draft.RemovePendingText("Other");
        var current = draft.CurrentDraftState;
        var accepted = draft.CrossBoundary(DraftBoundary.TabChange, new PushBackPilotFieldAdapter());

        Assert.Equal(DraftBoundaryDecision.Proceeded, accepted.Decision);
        Assert.Same(current, draft.CurrentDraftState);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.Equal(1.5, draft.InitialDraftState.Inputs.Annotations.AnnotationScale);
        Assert.True(draft.IsDirty);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    // ------------------------------------------------------------------ INV-F4T2A-04: the comparator

    [Theory]
    [MemberData(nameof(CorpusNames))]
    public void TheComparatorIsReflexiveAndSymmetricAndTwoMaterializationsOfTheSameOriginAreEqual(string fixture)
    {
        var first = PilotKit.State(fixture);
        var second = PilotKit.State(fixture);

        Assert.True(Comparer.Equals(first, first));
        Assert.True(Comparer.Equals(first, second));
        Assert.True(Comparer.Equals(second, first));
        Assert.Equal(Comparer.GetHashCode(first), Comparer.GetHashCode(second));
        Assert.NotSame(first.Editor, second.Editor);
    }

    public static IEnumerable<object[]> CorpusNames() =>
        PilotKit.Canonical.Concat(PilotKit.Legacy).Select(name => new object[] { name });

    [Fact]
    public void TheComparatorIsSymmetricAndSeparatesDifferentDesigns()
    {
        var names = PilotKit.Canonical;
        foreach (var left in names)
        {
            foreach (var right in names)
            {
                var a = PilotKit.State(left);
                var b = PilotKit.State(right);
                Assert.Equal(Comparer.Equals(a, b), Comparer.Equals(b, a));
                Assert.Equal(left == right, Comparer.Equals(a, b));
            }
        }
    }

    [Fact]
    public void OriginsWithDifferentTextAndEquivalentContentAreEqual()
    {
        var text = PilotKit.Text("pb-02-modulos");
        var embed = new RackEmbedStore().Deserialize(text)!;
        embed.Name = "Another name";
        embed.Id = "99999999-9999-4999-8999-999999999999";
        var renamed = new RackEmbedStore().Serialize(embed);

        var project = new RackProjectStore().Deserialize(embed.Design);
        embed.Design = new RackProjectStore().Serialize(project);
        var reserialized = new RackEmbedStore().Serialize(embed);

        Assert.NotEqual(text, renamed);
        Assert.NotEqual(text, reserialized);
        var original = PilotKit.Admitted(text);
        Assert.True(Comparer.Equals(original, PilotKit.Admitted(renamed)));
        Assert.True(Comparer.Equals(original, PilotKit.Admitted(reserialized)));
        Assert.NotEqual(original.Origin, PilotKit.Admitted(renamed).Origin);
    }

    [Fact]
    public void AnEditedScaleMakesTheStatesDifferent()
    {
        var state = PilotKit.State("pb-01-base");
        var edited = PilotKit.State("pb-01-base");
        edited.Inputs.Annotations.AnnotationScale = 1.5000001;

        Assert.False(Comparer.Equals(state, edited));
        Assert.False(Comparer.Equals(edited, state));
    }

    [Fact]
    public void TheComparatorTreatsNullAndNaNWithoutBreakingReflexivity()
    {
        var state = PilotKit.State("pb-01-base");
        state.Inputs.Annotations.AnnotationScale = double.NaN;

        Assert.True(Comparer.Equals(null, null));
        Assert.False(Comparer.Equals(state, null));
        Assert.False(Comparer.Equals(null, state));
        Assert.True(Comparer.Equals(state, state));
        var other = PilotKit.State("pb-01-base");
        other.Inputs.Annotations.AnnotationScale = double.NaN;
        Assert.True(Comparer.Equals(state, other));
    }

    [Fact]
    public void EveryInstanceFieldOfTheGraphIsClassifiedInExactlyOneOfTheFourClasses()
    {
        var discovered = PilotCensus.DiscoverFields().Select(PilotCensus.KeyOf).ToList();
        var unclassified = discovered.Where(key => !PilotCensus.Table.ContainsKey(key)).Distinct().OrderBy(key => key, StringComparer.Ordinal).ToList();
        var stale = PilotCensus.Table.Keys.Except(discovered).OrderBy(key => key, StringComparer.Ordinal).ToList();

        Assert.True(unclassified.Count == 0, "fields without a class: " + string.Join(", ", unclassified));
        Assert.True(stale.Count == 0, "classified fields that no longer exist: " + string.Join(", ", stale));
        Assert.Equal(discovered.Count, discovered.Distinct().Count());
    }

    [Fact]
    public void TheClassesOfTheContractAreExactlyTheFieldsTheContractNames()
    {
        string[] OfClass(PilotFieldClass kind) => PilotCensus.Table.Where(pair => pair.Value == kind)
            .Select(pair => pair.Key.Substring(pair.Key.IndexOf('.') + 1)).OrderBy(name => name, StringComparer.Ordinal).ToArray();

        Assert.Equal(
            new[] { "ActiveSelection", "ActiveSide", "selectedCells", "selectedFrontIndex", "selectedLevelIndex" },
            OfClass(PilotFieldClass.Selection));
        Assert.Equal(
            new[]
            {
                "DormantCompositeTail", "DormantTailLineOverrides", "LastModuleReconciliation", "dormantTailConsumed",
                "moduleCommit", "moduleSession", "moduleSignature", "workingBaseline",
            }.OrderBy(name => name, StringComparer.Ordinal).ToArray(),
            OfClass(PilotFieldClass.ModuleAuthority));
        Assert.Equal(new[] { "Origin" }, OfClass(PilotFieldClass.Origin));
    }

    [Fact]
    public void EveryEditableFieldIsExercisedByTheRichStateAndMutatingItMakesTheStatesDifferent()
    {
        var rich = PilotRich.Build();
        var baseline = PilotRich.Build();
        Assert.True(Comparer.Equals(baseline, rich));

        var editable = PilotCensus.Table.Where(pair => pair.Value == PilotFieldClass.Editable).Select(pair => pair.Key).ToHashSet(StringComparer.Ordinal);
        var exercised = new HashSet<string>(StringComparer.Ordinal);
        var failures = new List<string>();
        foreach (var (owner, field) in PilotCensus.EditableSlots(rich).ToList())
        {
            var key = PilotCensus.KeyOf(field);
            if (!Mutator.TryMutate(owner, field, out var undo)) continue;
            exercised.Add(key);
            if (Comparer.Equals(baseline, rich) || Comparer.Equals(rich, baseline)) failures.Add("mutation not detected: " + key);
            undo();
            if (!Comparer.Equals(baseline, rich)) failures.Add("undo did not restore equality: " + key);
        }

        // A reference to a part that holds more fields is exercised through those fields; every other field must be mutated itself.
        var containers = editable.Where(key => PilotCensus.Table.ContainsKey(key) && IsContainer(key)).ToList();
        var missing = editable.Except(exercised).Except(containers).OrderBy(key => key, StringComparer.Ordinal).ToList();
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Assert.True(missing.Count == 0, "editable fields the rich state does not exercise: " + string.Join(", ", missing));
    }

    private static bool IsContainer(string key) => Mutator.ContainerKeys.Contains(key);

    [Fact]
    public void MutatingEverySelectionFieldTheModuleAuthorityAndTheOriginLeavesTheStatesEqual()
    {
        var rich = PilotRich.Build();
        var baseline = PilotRich.Build();
        var checkedKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var key in PilotCensus.Table.Where(pair => pair.Value != PilotFieldClass.Editable).Select(pair => pair.Key))
        {
            var slot = PilotCensus.FindSlot(rich, key);
            Assert.True(slot.HasValue, "the rich state has no slot for " + key);
            var (owner, field) = slot!.Value;
            Assert.True(Mutator.TryMutate(owner, field, out var undo), "cannot mutate " + key);
            checkedKeys.Add(key);

            Assert.True(Comparer.Equals(baseline, rich), "the comparator saw the change of " + key);
            Assert.True(Comparer.Equals(rich, baseline), "the comparator saw the change of " + key);
            undo();
        }

        Assert.Equal(PilotCensus.Table.Count(pair => pair.Value != PilotFieldClass.Editable), checkedKeys.Count);
    }

    // ------------------------------------------------------------------ INV-F4T2A-05: the deep copier

    [Theory]
    [MemberData(nameof(CorpusNames))]
    public void TheCopyIsEqualForTheComparatorAndSharesTheOriginString(string fixture)
    {
        var state = PilotKit.State(fixture);

        var copy = PushBackPilotStateComparer.Copy(state);

        Assert.NotSame(state, copy);
        Assert.NotSame(state.Editor, copy.Editor);
        Assert.NotSame(state.Inputs, copy.Inputs);
        Assert.True(Comparer.Equals(state, copy));
        Assert.True(Comparer.Equals(copy, state));
        Assert.Same(state.Origin, copy.Origin);
    }

    [Fact]
    public void TheCopyOfTheRichStateKeepsEveryEditableValueAndTheSameSelection()
    {
        var rich = PilotRich.Build();
        var copy = PushBackPilotStateComparer.Copy(rich);

        Assert.True(Comparer.Equals(rich, copy));
        Assert.True(Comparer.Equals(PilotRich.Build(), copy));
        Assert.Equal(rich.Editor.ActiveSide, copy.Editor.ActiveSide);
        Assert.Equal(rich.Editor.ActiveSelection, copy.Editor.ActiveSelection);
        foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
        {
            var left = rich.Editor.Of(side).Structure;
            var right = copy.Editor.Of(side).Structure;
            Assert.Equal(left.SelectedFrontIndex, right.SelectedFrontIndex);
            Assert.Equal(left.SelectedLevelIndex, right.SelectedLevelIndex);
            Assert.Equal(
                left.SelectedCells().Select(cell => (cell.FrontIndex, cell.LevelIndex)).OrderBy(cell => cell).ToArray(),
                right.SelectedCells().Select(cell => (cell.FrontIndex, cell.LevelIndex)).OrderBy(cell => cell).ToArray());
        }
    }

    [Fact]
    public void TheCopySharesNoMutableObjectWithTheOriginal()
    {
        var rich = PilotRich.Build();
        var copy = PushBackPilotStateComparer.Copy(rich);

        var original = PilotCensus.AllObjects(rich);
        var copied = PilotCensus.AllObjects(copy);
        var shared = original.Where(copied.Contains).ToList();

        Assert.NotEmpty(copied);
        var mutable = shared.Where(item => !PilotCensus.IsImmutable(item.GetType())).Select(item => item.GetType().Name).Distinct().ToList();
        Assert.True(mutable.Count == 0, "mutable objects shared with the original: " + string.Join(", ", mutable));
        // What IS shared is only immutable: the origin string and the immutable face pieces.
        Assert.All(shared, item => Assert.True(item is string || item is SafetyFacePiece, item.GetType().Name));
    }

    [Fact]
    public void MutatingEveryFieldOfTheCopyNeverChangesTheOriginal()
    {
        var rich = PilotRich.Build();
        var copy = PushBackPilotStateComparer.Copy(rich);
        var baseline = PilotRich.Build();

        foreach (var (owner, field) in PilotCensus.EditableSlots(copy).ToList())
        {
            // The face pieces are immutable and shared on purpose: they change only by being replaced, which the other fields cover.
            if (field.DeclaringType == typeof(SafetyFacePiece)) continue;
            if (!Mutator.TryMutate(owner, field, out var undo)) continue;
            Assert.True(Comparer.Equals(baseline, rich), "the original changed when the copy changed " + PilotCensus.KeyOf(field));
            undo();
        }
    }

    [Fact]
    public void TheCopyDoesNotCarryTheModuleAndTailAuthority()
    {
        var rich = PilotRich.Build();
        Assert.NotNull(rich.Editor.SideA.WorkingBaseline);
        Assert.NotNull(rich.Editor.SideA.ModuleCommit);
        Assert.NotEmpty(rich.Editor.DormantCompositeTail);

        var copy = PushBackPilotStateComparer.Copy(rich);

        foreach (var key in PilotCensus.Table.Where(pair => pair.Value == PilotFieldClass.ModuleAuthority).Select(pair => pair.Key))
        {
            var slot = PilotCensus.FindSlot(copy, key);
            Assert.True(slot.HasValue, key);
            var value = slot!.Value.Field.GetValue(slot.Value.Owner);
            var isDefault = value == null
                || (value is bool flag && !flag)
                || (value is ICollection collection && collection.Count == 0);
            Assert.True(isDefault, "the copy carries " + key);
        }

        Assert.Null(copy.Editor.SideA.WorkingBaseline);
        Assert.Null(copy.Editor.SideB.WorkingBaseline);
        Assert.Null(copy.Editor.SideA.ModuleCommit);
        Assert.Empty(copy.Editor.DormantCompositeTail);
        Assert.Empty(copy.Editor.DormantTailLineOverrides);
        Assert.False(copy.Editor.DormantTailConsumptionPending);
    }

    [Fact]
    public void ACopyOfAStateMaterializedFromAnOriginIsStillEqualAndNeverHasItsBaseline()
    {
        var state = PilotKit.State("pb-02-modulos");
        Assert.NotNull(state.Editor.SideA.WorkingBaseline);

        var copy = PushBackPilotStateComparer.Copy(state);

        Assert.True(Comparer.Equals(state, copy));
        Assert.Null(copy.Editor.SideA.WorkingBaseline);
    }
}

/// <summary>Mutates one classified field of an instance in a way the comparator must (or must not) notice, and undoes it.</summary>
internal static class Mutator
{
    /// <summary>Fields that only hold a part whose own fields are exercised one by one.</summary>
    public static readonly HashSet<string> ContainerKeys = new(StringComparer.Ordinal)
    {
        "PushBackPilotDraftState.Editor", "PushBackPilotDraftState.Inputs",
        "PushBackCompositeEditorState.SideA", "PushBackCompositeEditorState.SideB",
        "PushBackEditorState.structure",
        "PushBackEditorInputs.Pallet", "PushBackEditorInputs.Annotations",
        "SelectiveSafetySelection.BotaB", "SelectiveSafetySelection.tope", "SelectiveSafetySelection.desviador",
        "SelectiveSafetySelection.defensa", "SelectiveSafetySelection.guia", "SelectiveSafetySelection.parrilla",
        "SelectiveSafetySelection.bota",
    };

    public static bool TryMutate(object owner, FieldInfo field, out Action undo)
    {
        undo = () => { };
        if (ContainerKeys.Contains(PilotCensus.KeyOf(field))) return false;

        var type = field.FieldType;
        var current = field.GetValue(owner);
        if (current is IEnumerable and not string || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>)))
        {
            return MutateCollection(current!, out undo);
        }

        object? replacement = Replacement(type, current);
        if (replacement is NoReplacement) return false;
        field.SetValue(owner, replacement);
        undo = () => field.SetValue(owner, current);
        return true;
    }

    private sealed class NoReplacement
    {
    }

    private static object? Replacement(Type type, object? current)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
        {
            return current == null ? Replacement(underlying, null) : null;
        }

        if (type == typeof(bool)) return !(bool)(current ?? false);
        if (type == typeof(int)) return (int)(current ?? 0) + 1;
        if (type == typeof(double)) return double.IsNaN((double)(current ?? 0.0)) ? 2.0 : (double)(current ?? 0.0) + 1.5;
        if (type == typeof(string)) return ((string?)current ?? string.Empty) + "~";
        if (type.IsEnum)
        {
            var values = Enum.GetValues(type).Cast<object>().ToList();
            return values.First(value => !value.Equals(current));
        }

        if (type == typeof(SafetyFacePiece)) return SafetyFacePiece.Of(((SafetyFacePiece?)current)?.ElementId + "~");
        if (current == null && !type.IsValueType) return RuntimeHelpers.GetUninitializedObject(type);
        if (!type.IsValueType) return null;
        return new NoReplacement();
    }

    private static bool MutateCollection(object collection, out Action undo)
    {
        var type = collection.GetType();
        var elementType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);
        var element = NewElement(elementType);
        var add = type.GetMethod("Add", new[] { elementType })!;
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
                remove!.Invoke(collection, new[] { element });
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
        return Activator.CreateInstance(type, nonPublic: true)!;
    }
}
