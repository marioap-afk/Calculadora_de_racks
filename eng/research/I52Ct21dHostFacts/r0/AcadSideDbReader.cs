using Autodesk.AutoCAD.DatabaseServices;
using I52Ct21d.HostFacts.Core;
using Rt = Autodesk.AutoCAD.Runtime;

namespace I52Ct21d.HostFacts.R0;

/// <summary>
/// THE ONLY AutoCAD-touching reader of the R0 DLL (I-2). It builds exactly one database, <c>new Database(false, true)</c>, fills it with
/// <c>ReadDwgFile</c> from a PRIVATE COPY and reads it. It never saves it, never attaches it to a document, never opens an object for
/// write and never commits: <c>OpenCloseTransaction</c> objects are only disposed (aborted). The forbidden-API scan allows
/// <c>ReadDwgFile</c>, <c>new Database</c> and the transaction manager of a database ONLY inside this type (design 5.3 item 3).
///
/// Every host behaviour below that the repository cannot establish is marked <c>HOST-TO-CONFIRM</c>; the raw output of the exact
/// build governs and a mismatch makes the affected fact UNKNOWN, never a corrected guess.
/// </summary>
internal sealed class AcadSideDbReader : IDrawingSource
{
    public IDrawingReader OpenPrivateCopy(string privateCopyPath)
    {
        var db = new Database(false, true); // in-memory side database, no default drawing, no document (R0 rule)
        try
        {
            // HOST-TO-CONFIRM: ReadDwgFile(string, FileShare, bool, string) on a private copy; FileShare.Read lets the host keep the file open.
            db.ReadDwgFile(privateCopyPath, FileShare.Read, true, "");
            db.CloseInput(true); // HOST-TO-CONFIRM: releases the input file after a full read
        }
        catch (System.Exception)
        {
            db.Dispose();
            throw;
        }
        return new SideDbView(db);
    }

    private sealed class SideDbView : IDrawingReader
    {
        private readonly Database _db;

        public SideDbView(Database db) => _db = db;

        public void Dispose() => _db.Dispose();

        public IReadOnlyList<BlockRecordInfo> ReadBlockRecords()
        {
            var result = new List<BlockRecordInfo>();
            using var tr = _db.TransactionManager.StartOpenCloseTransaction();
            var table = (BlockTable)tr.GetObject(_db.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId id in table)
            {
                try
                {
                    var btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                    var entities = new List<EntityInfo>();
                    foreach (ObjectId eid in btr) entities.Add(ReadEntity(tr, eid));
                    result.Add(new BlockRecordInfo(btr.Name, btr.IsLayout, btr.IsAnonymous, btr.IsDynamicBlock, entities, null));
                }
                catch (System.Exception ex)
                {
                    result.Add(new BlockRecordInfo("", false, false, false, Array.Empty<EntityInfo>(), "BLOCK_RECORD_UNREADABLE:" + ex.GetType().Name));
                }
            }
            return result; // the transaction is disposed WITHOUT Commit
        }

        private static EntityInfo ReadEntity(Transaction tr, ObjectId id)
        {
            try
            {
                var obj = tr.GetObject(id, OpenMode.ForRead);
                var rx = obj.GetRXClass();
                var rxName = rx.Name;
                var dxfName = rx.DxfName;
                var ent = obj as Entity;

                // HOST-TO-CONFIRM: how a proxy or a custom (third-party) object presents itself to the managed API.
                var proxyOrCustom = obj is ProxyEntity || !rxName.StartsWith("AcDb", StringComparison.Ordinal);

                string? textStyle = obj is DBText t ? t.TextStyleName : obj is MText m ? m.TextStyleName : null;
                string? dimStyle = obj is Dimension dim ? dim.DimensionStyleName : null;

                string? referenced = null;
                var anonymousStatic = false;
                IReadOnlyList<double>? scale = null;
                if (obj is BlockReference br && rxName == "AcDbBlockReference")
                {
                    var targetId = br.IsDynamicBlock ? br.DynamicBlockTableRecord : br.BlockTableRecord;
                    var target = (BlockTableRecord)tr.GetObject(targetId, OpenMode.ForRead);
                    referenced = target.Name;
                    // HOST-TO-CONFIRM: a reference to the anonymous representation of a dynamic block reports IsDynamicBlock = true
                    var refBtr = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
                    anonymousStatic = refBtr.IsAnonymous && !br.IsDynamicBlock;
                    var s = br.ScaleFactors;
                    scale = new[] { s.X, s.Y, s.Z };
                }

                DstyleInfo? dstyle = null;
                if (rxName == "AcDbRotatedDimension" || rxName == "AcDbAlignedDimension")
                    dstyle = ReadDstyle(obj);

                return new EntityInfo(rxName, dxfName, proxyOrCustom, ent?.Layer, ent?.Linetype, textStyle, dimStyle, referenced,
                    anonymousStatic, scale, dstyle, null);
            }
            catch (System.Exception ex)
            {
                return new EntityInfo("", "", false, null, null, null, null, null, false, null, null, "ENTITY_UNREADABLE:" + ex.GetType().Name);
            }
        }

        // BA-05 V5 2.3.2: the typed values of the ACAD extended data inside the DSTYLE section (after the string DSTYLE and the opening
        // control string "{", up to the matching "}"), in host order. HOST-TO-CONFIRM: the TypeCode and the runtime type of each value.
        private static DstyleInfo ReadDstyle(DBObject obj)
        {
            using var rb = obj.GetXDataForApplication("ACAD");
            if (rb is null) return new DstyleInfo(false, true, Array.Empty<DstylePair>());
            var tvs = rb.AsArray();
            var start = -1;
            for (var i = 0; i < tvs.Length; i++)
                if (tvs[i].TypeCode == 1000 && tvs[i].Value is string s && s == "DSTYLE") { start = i; break; }
            if (start < 0) return new DstyleInfo(false, true, Array.Empty<DstylePair>());
            if (start + 1 >= tvs.Length || tvs[start + 1].TypeCode != 1002 || !(tvs[start + 1].Value is string open && open == "{"))
                return new DstyleInfo(true, false, Array.Empty<DstylePair>());

            var pairs = new List<DstylePair>();
            var depth = 1;
            for (var j = start + 2; j < tvs.Length; j++)
            {
                var tv = tvs[j];
                if (tv.TypeCode == 1002 && tv.Value is string c)
                {
                    if (c == "{") depth++;
                    else if (c == "}") depth--;
                    if (depth == 0) return new DstyleInfo(true, true, pairs);
                }
                pairs.Add(new DstylePair(tv.TypeCode, tv.Value?.GetType().FullName ?? "null"));
            }
            return new DstyleInfo(true, false, pairs); // no matching closing control string
        }

        public StoredBufferScan ReadStoredBuffers()
        {
            var entries = new List<StoredEntry>();
            var buffers = 0;
            var failures = 0;
            var visited = new HashSet<ObjectId>();
            using var tr = _db.TransactionManager.StartOpenCloseTransaction();

            void AddBuffer(Autodesk.AutoCAD.DatabaseServices.ResultBuffer? rb)
            {
                if (rb is null) return;
                buffers++;
                foreach (var tv in rb.AsArray()) entries.Add(new StoredEntry(tv.TypeCode, tv.Value?.GetType().FullName ?? "null"));
            }

            void WalkDictionary(ObjectId dictId)
            {
                if (dictId.IsNull || !visited.Add(dictId)) return;
                try
                {
                    var dict = (DBDictionary)tr.GetObject(dictId, OpenMode.ForRead);
                    foreach (DBDictionaryEntry e in dict)
                    {
                        try
                        {
                            var value = tr.GetObject(e.Value, OpenMode.ForRead);
                            if (value is Xrecord xr)
                            {
                                using var data = xr.Data;
                                AddBuffer(data);
                            }
                            else if (value is DBDictionary) WalkDictionary(e.Value);
                        }
                        catch (System.Exception) { failures++; }
                    }
                }
                catch (System.Exception) { failures++; }
            }

            WalkDictionary(_db.NamedObjectsDictionaryId);

            var table = (BlockTable)tr.GetObject(_db.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId id in table)
            {
                try
                {
                    var btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                    foreach (ObjectId eid in btr)
                    {
                        try
                        {
                            var obj = tr.GetObject(eid, OpenMode.ForRead);
                            using (var xd = obj.XData) AddBuffer(xd);
                            WalkDictionary(obj.ExtensionDictionary);
                        }
                        catch (System.Exception) { failures++; }
                    }
                }
                catch (System.Exception) { failures++; }
            }
            return new StoredBufferScan(entries, buffers, failures);
        }
    }
}
