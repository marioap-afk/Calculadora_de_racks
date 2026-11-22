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
        "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado", "pb-08-redondeo",
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

    /// <summary>
    /// A persisted document with its line endings normalized (CRLF to LF). The fixtures were generated on Windows, where the
    /// indented JSON of the inner design carries CRLF, and the Core job of the CI runs on Linux, where the same serializer
    /// writes LF: every comparison of persisted documents goes through this method so none depends on the platform.
    /// </summary>
    public static string Normalize(string document) => document.Replace("\r\n", "\n");

    /// <summary>The inner design JSON of an envelope text, with its line endings normalized.</summary>
    public static string DesignOf(string envelopeText) => Normalize(new RackEmbedStore().Deserialize(envelopeText)!.Design);

    /// <summary>The persisted document a design becomes, through the same store the product uses, with its line endings normalized.</summary>
    public static string Persist(PushBackDesign design) => Normalize(new RackProjectStore().Serialize(RackProject.ForPushBack(design)));

    /// <summary>
    /// The leaves where two JSON documents differ, as "path: before -> after", one line per LEAF. A path that is on one side
    /// only reads "&lt;absent&gt;"; a container (object or array) that is new, removed or replaced by a scalar is expanded
    /// to every one of its leaves, each with the text of what stood there before (or "&lt;absent&gt;"), so no new container
    /// is ever summarized as "[3 items]" or "{object}". An empty container is the leaf "[]" or "{}".
    /// </summary>
    public static List<string> JsonDiff(string before, string after)
    {
        var result = new List<string>();
        using var a = JsonDocument.Parse(before);
        using var b = JsonDocument.Parse(after);
        Diff(a.RootElement, b.RootElement, "$", result);
        return result;
    }

    private static bool IsContainer(JsonElement element) =>
        element.ValueKind == JsonValueKind.Object || element.ValueKind == JsonValueKind.Array;

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
                else if (hasA) Leaves(left, path + "." + name, "<absent>", before: true, into);
                else Leaves(right, path + "." + name, "<absent>", before: false, into);
            }

            return;
        }

        if (a.ValueKind == JsonValueKind.Array && b.ValueKind == JsonValueKind.Array)
        {
            var count = Math.Max(a.GetArrayLength(), b.GetArrayLength());
            for (var index = 0; index < count; index++)
            {
                var itemPath = path + "[" + index + "]";
                if (index < a.GetArrayLength() && index < b.GetArrayLength()) Diff(a[index], b[index], itemPath, into);
                else if (index < a.GetArrayLength()) Leaves(a[index], itemPath, "<absent>", before: true, into);
                else Leaves(b[index], itemPath, "<absent>", before: false, into);
            }

            return;
        }

        if (IsContainer(a) && !IsContainer(b)) { Leaves(a, path, Short(b), before: true, into); return; }
        if (!IsContainer(a) && IsContainer(b)) { Leaves(b, path, Short(a), before: false, into); return; }

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

    /// <summary>Every leaf of a container on ONE side of a difference; <paramref name="other"/> is the text of the other side.</summary>
    private static void Leaves(JsonElement element, string path, string other, bool before, List<string> into)
    {
        if (element.ValueKind == JsonValueKind.Object && element.EnumerateObject().Any())
        {
            foreach (var property in element.EnumerateObject()) Leaves(property.Value, path + "." + property.Name, other, before, into);
            return;
        }

        if (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray()) Leaves(item, path + "[" + index++ + "]", other, before, into);
            return;
        }

        var text = element.ValueKind == JsonValueKind.Object ? "{}" : element.ValueKind == JsonValueKind.Array ? "[]" : Short(element);
        into.Add(path + ": " + (before ? text + " -> " + other : other + " -> " + text));
    }

    private static string Short(JsonElement element) => element.ValueKind switch
    {
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

// I-64 F4-T2A-PILOT-STATE: INV-F4T2A-01 to 05 over the state, the adapter, the comparator and the copier of the pilot.
public class WorkspaceDraftPilotStateTests
{

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

    // ------------------------------------------------------------------ INV-F4T2A-03: the adapter contract

    private static bool ContainsStateType(Type type) =>
        type == typeof(PushBackPilotDraftState)
        || type == typeof(PushBackCompositeEditorState)
        || type == typeof(PushBackEditorState)
        || type == typeof(PushBackEditorInputs)
        || (type.IsGenericType && type.GetGenericArguments().Any(ContainsStateType))
        || (type.IsArray && ContainsStateType(type.GetElementType()!));

    // ------------------------------------------------------------------ INV-F4T2A-04: the comparator

    public static IEnumerable<object[]> CorpusNames() =>
        PilotKit.Canonical.Concat(PilotKit.Legacy).Select(name => new object[] { name });

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

    // ------------------------------------------------------------------ INV-F4T2A-05: the deep copier

}
