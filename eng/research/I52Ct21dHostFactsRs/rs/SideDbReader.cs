using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;

namespace I52Ct21d.HostFacts.Rs;

/// <summary>
/// The READ-ONLY view over a <see cref="SideDbHandle"/>: every object is opened <c>OpenMode.ForRead</c> (a compile-time constant that the scan
/// resolves), every transaction is an <c>OpenCloseTransaction</c> that is only disposed (never committed). It holds no write member: the
/// allowlist approves here only getters, enumerators, <c>GetObject</c> and the transaction manager. HOST-TO-CONFIRM items are marked.
/// </summary>
internal sealed class SideDbReader
{
    private static string Hex(double v) => DoubleBits.Hex(v);

    private static string Text(object? value)
    {
        if (value is null) return "null";
        if (value is double d) return d.ToString("R", CultureInfo.InvariantCulture);
        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
    }

    private static string? Bits(object? value) => value is double d ? Hex(d) : null;

    private static string TypeName(object? value) => value?.GetType().FullName ?? "null";

    // ---- U-RS-3 ---------------------------------------------------------------------------------------------------------------------------
    /// <summary>T2 / T3: the scale factors, rotation, normal and the column lengths of <c>BlockTransform</c> of every block reference of model space.</summary>
    internal IReadOnlyList<ReferenceReading> ReadScaleReferences(SideDbHandle side)
    {
        var db = side.Database;
        var result = new List<ReferenceReading>();
        using var tr = db.TransactionManager.StartOpenCloseTransaction();
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
        foreach (ObjectId id in modelSpace)
        {
            var handle = id.Handle.ToString();
            try
            {
                var obj = tr.GetObject(id, OpenMode.ForRead);
                if (obj is not BlockReference reference) continue;
                var s = reference.ScaleFactors;
                var n = reference.Normal;
                var m = reference.BlockTransform;
                var columns = new string[3];
                for (var c = 0; c < 3; c++)
                    columns[c] = Hex(Math.Sqrt(m[0, c] * m[0, c] + m[1, c] * m[1, c] + m[2, c] * m[2, c]));
                result.Add(new ReferenceReading(handle, new[] { Hex(s.X), Hex(s.Y), Hex(s.Z) }, Hex(reference.Rotation), new[] { Hex(n.X), Hex(n.Y), Hex(n.Z) }, columns, null));
            }
            catch (System.Exception ex)
            {
                result.Add(new ReferenceReading(handle, null, null, null, null, ex.GetType().Name + ":" + ex.Message));
            }
        }
        return result; // the transaction is disposed WITHOUT Commit
    }

    // ---- U-RS-2 ---------------------------------------------------------------------------------------------------------------------------
    /// <summary>The effective value of each product variable on the drawing's current dimension style (what a fresh dimension would inherit).</summary>
    internal IReadOnlyList<StyleValue> ReadStyleValues(SideDbHandle side)
    {
        var db = side.Database;
        using var tr = db.TransactionManager.StartOpenCloseTransaction();
        var style = (DimStyleTableRecord)tr.GetObject(db.Dimstyle, OpenMode.ForRead);
        return new[]
        {
            Style("DIMSCALE", style.Dimscale), Style("DIMTXT", style.Dimtxt), Style("DIMASZ", style.Dimasz), Style("DIMEXE", style.Dimexe),
            Style("DIMEXO", style.Dimexo), Style("DIMGAP", style.Dimgap), StyleInt("DIMTAD", style.Dimtad), StyleInt("DIMDEC", style.Dimdec),
        };
    }

    private static StyleValue Style(string variable, double value) =>
        new(variable, "System.Double", value, value.ToString("R", CultureInfo.InvariantCulture), Hex(value));

    private static StyleValue StyleInt(string variable, int value) =>
        new(variable, "System.Int32", value, value.ToString(CultureInfo.InvariantCulture), null);

    /// <summary>
    /// The typed values of the <c>ACAD</c> extended data inside the <c>DSTYLE</c> section of every dimension of model space, in host order
    /// (BA-05 V5 2.3.2: after the string <c>DSTYLE</c> and the opening control string <c>{</c>, up to the matching <c>}</c>).
    /// </summary>
    internal IReadOnlyList<DimensionReading> ReadDimensions(SideDbHandle side)
    {
        var db = side.Database;
        var result = new List<DimensionReading>();
        using var tr = db.TransactionManager.StartOpenCloseTransaction();
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
        foreach (ObjectId id in modelSpace)
        {
            var handle = id.Handle.ToString();
            try
            {
                var obj = tr.GetObject(id, OpenMode.ForRead);
                if (obj is not Dimension) continue;
                result.Add(ReadDstyle(handle, obj));
            }
            catch (System.Exception ex)
            {
                result.Add(new DimensionReading(handle, false, false, false, Array.Empty<StoredEntryReading>(), ex.GetType().Name + ":" + ex.Message));
            }
        }
        return result;
    }

    private static DimensionReading ReadDstyle(string handle, DBObject obj)
    {
        using var rb = obj.GetXDataForApplication("ACAD");
        if (rb is null) return new DimensionReading(handle, false, false, true, Array.Empty<StoredEntryReading>(), null);
        var tvs = rb.AsArray();
        var start = -1;
        for (var i = 0; i < tvs.Length; i++)
            if (tvs[i].TypeCode == 1000 && tvs[i].Value is string s && s == "DSTYLE") { start = i; break; }
        if (start < 0) return new DimensionReading(handle, true, false, true, Array.Empty<StoredEntryReading>(), null);
        if (start + 1 >= tvs.Length || tvs[start + 1].TypeCode != 1002 || !(tvs[start + 1].Value is string open && open == "{"))
            return new DimensionReading(handle, true, true, false, Array.Empty<StoredEntryReading>(), null);
        var entries = new List<StoredEntryReading>();
        var depth = 1;
        for (var j = start + 2; j < tvs.Length; j++)
        {
            var tv = tvs[j];
            if (tv.TypeCode == 1002 && tv.Value is string c)
            {
                if (c == "{") depth++;
                else if (c == "}") depth--;
                if (depth == 0) return new DimensionReading(handle, true, true, true, entries, null);
            }
            entries.Add(new StoredEntryReading(tv.TypeCode, TypeName(tv.Value), Text(tv.Value), Bits(tv.Value)));
        }
        return new DimensionReading(handle, true, true, false, entries, null); // no matching closing control string
    }

    // ---- U-RS-1 ---------------------------------------------------------------------------------------------------------------------------
    internal IReadOnlyList<string> ListDynamicDefinitionNames(SideDbHandle side)
    {
        var db = side.Database;
        var names = new List<string>();
        using var tr = db.TransactionManager.StartOpenCloseTransaction();
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        foreach (ObjectId id in blockTable)
        {
            var record = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
            if (!record.IsLayout && !record.IsAnonymous && record.IsDynamicBlock) names.Add(record.Name);
        }
        return names;
    }

    internal int CountBlockRecords(SideDbHandle side)
    {
        var db = side.Database;
        var count = 0;
        using var tr = db.TransactionManager.StartOpenCloseTransaction();
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        foreach (ObjectId unused in blockTable) count++;
        return count;
    }

    /// <summary>The dynamic-property collection of the reference with the given handle (created in the SCRATCH side database): name, read-only flag, host value type, units type, allowed values.</summary>
    internal IReadOnlyList<DynamicPropertyReading> ReadDynamicProperties(SideDbHandle side, string referenceHandle)
    {
        var db = side.Database;
        using var tr = db.TransactionManager.StartOpenCloseTransaction();
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
        foreach (ObjectId id in modelSpace)
        {
            if (id.Handle.ToString() != referenceHandle) continue;
            var reference = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
            if (!reference.IsDynamicBlock) throw new InvalidOperationException("REFERENCE_IS_NOT_DYNAMIC");
            var result = new List<DynamicPropertyReading>();
            // HOST-TO-CONFIRM: a reference created through the API in a database with no document exposes its property collection
            foreach (DynamicBlockReferenceProperty property in reference.DynamicBlockReferencePropertyCollection)
            {
                var value = property.Value;
                IReadOnlyList<AllowedValueReading>? allowed;
                string? allowedError = null;
                try
                {
                    var raw = property.GetAllowedValues();
                    var list = new List<AllowedValueReading>();
                    foreach (var a in raw) list.Add(new AllowedValueReading(TypeName(a), Text(a), Bits(a)));
                    allowed = list;
                }
                catch (System.Exception ex)
                {
                    allowed = null;
                    allowedError = ex.GetType().Name + ":" + ex.Message;
                }
                result.Add(new DynamicPropertyReading(property.PropertyName, property.ReadOnly, TypeName(value), UnitsName(property.UnitsType), Text(value), Bits(value), allowed, allowedError));
            }
            return result;
        }
        throw new InvalidOperationException("REFERENCE_NOT_FOUND:" + referenceHandle);
    }

    private static string UnitsName(DynamicBlockReferencePropertyUnitsType units) => units.ToString() switch
    {
        "NoUnits" => "NoUnits",
        "Angular" => "Angular",
        "Distance" => "Distance",
        "Area" => "Area",
        _ => "UNKNOWN",
    };
}
