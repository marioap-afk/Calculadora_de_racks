using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace I52Auth15.HostHarness
{
    /// <summary>
    /// SNAP: a sorted, comparable picture of everything AUTH-15 could leave behind in a database.
    ///
    /// Keys (all sorted, ordinal):
    ///   BT:name          handle of every block table record (definitions, anonymous, model and paper spaces)
    ///   BTC:name         entity handles + extension-dictionary flag of every non-layout definition
    ///   LT:/DS:/TS:name  layer, dimension style and text style tables
    ///   LTP:/RA:/UCS:/VW:name  linetype, registered-application, UCS and view tables
    ///   VP:name#handle   viewport table (its "*Active" records share a name, so the handle is part of the key)
    ///   NOD:key          named objects dictionary entries
    ///   MS:handle        model space entities
    ///   LAYOUT:name:h    every paper-space layout's entities
    /// The handseed is deliberately not part of it: an aborted transaction may advance it.
    /// </summary>
    internal sealed class Snapshot
    {
        public SortedDictionary<string, string> Items { get; } = new SortedDictionary<string, string>(StringComparer.Ordinal);

        private static string H(ObjectId id) => id.Handle.ToString();

        /// <summary>Takes the picture THROUGH <paramref name="tr"/> (read-only), so it can be taken inside the caller's
        /// own transaction, including what that transaction has created and not yet committed.</summary>
        public static Snapshot Take(Database db, Transaction tr)
        {
            var s = new Snapshot();

            var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId id in blockTable)
            {
                var record = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                s.Items["BT:" + record.Name] = H(id);

                if (!record.IsLayout)
                {
                    var handles = new List<string>();
                    foreach (ObjectId entityId in record)
                    {
                        handles.Add(H(entityId));
                    }

                    handles.Sort(StringComparer.Ordinal);
                    s.Items["BTC:" + record.Name] =
                        handles.Count + "|" + (record.ExtensionDictionary.IsNull ? "noext" : "ext") + "|" + string.Join(",", handles);
                }
            }

            var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            foreach (ObjectId id in layerTable)
            {
                s.Items["LT:" + ((LayerTableRecord)tr.GetObject(id, OpenMode.ForRead)).Name] = H(id);
            }

            var dimStyles = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForRead);
            foreach (ObjectId id in dimStyles)
            {
                s.Items["DS:" + ((DimStyleTableRecord)tr.GetObject(id, OpenMode.ForRead)).Name] = H(id);
            }

            var textStyles = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);
            foreach (ObjectId id in textStyles)
            {
                s.Items["TS:" + ((TextStyleTableRecord)tr.GetObject(id, OpenMode.ForRead)).Name] = H(id);
            }

            Table(s, tr, "LTP:", db.LinetypeTableId, false);
            Table(s, tr, "RA:", db.RegAppTableId, false);
            Table(s, tr, "UCS:", db.UcsTableId, false);
            Table(s, tr, "VW:", db.ViewTableId, false);
            Table(s, tr, "VP:", db.ViewportTableId, true);

            var nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
            foreach (DBDictionaryEntry entry in nod)
            {
                s.Items["NOD:" + entry.Key] = H(entry.Value);
            }

            var layouts = (DBDictionary)tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
            foreach (DBDictionaryEntry entry in layouts)
            {
                var layout = (Layout)tr.GetObject(entry.Value, OpenMode.ForRead);
                var space = (BlockTableRecord)tr.GetObject(layout.BlockTableRecordId, OpenMode.ForRead);
                var prefix = layout.ModelType ? "MS:" : "LAYOUT:" + layout.LayoutName + ":";
                var count = 0;

                foreach (ObjectId entityId in space)
                {
                    s.Items[prefix + H(entityId)] = entityId.ObjectClass.DxfName;
                    count++;
                }

                s.Items[prefix + "#count"] = count.ToString();
            }

            return s;
        }

        private static void Table(Snapshot s, Transaction tr, string prefix, ObjectId tableId, bool nameAndHandle)
        {
            var table = (SymbolTable)tr.GetObject(tableId, OpenMode.ForRead);

            foreach (ObjectId id in table)
            {
                var name = ((SymbolTableRecord)tr.GetObject(id, OpenMode.ForRead)).Name;
                s.Items[nameAndHandle ? prefix + name + "#" + H(id) : prefix + name] = H(id);
            }
        }

        /// <summary>Opens a throw-away transaction, takes the picture and aborts it. Only valid when the caller has no
        /// transaction of its own open on <paramref name="db"/>.</summary>
        public static Snapshot Now(Database db)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var snapshot = Take(db, tr);
                tr.Abort();
                return snapshot;
            }
        }

        public static bool IsPlacementKey(string key) =>
            key.StartsWith("MS:", StringComparison.Ordinal) || key.StartsWith("LAYOUT:", StringComparison.Ordinal);

        /// <summary>Human-readable differences (empty = identical). UNLIMITED by default: a leak is enumerated in full, with the
        /// handle of every added, removed or changed entry, never summarized.</summary>
        public List<string> Diff(Snapshot other, Func<string, bool> filter = null, int max = int.MaxValue)
        {
            var differences = new List<string>();
            var keys = new SortedSet<string>(Items.Keys.Concat(other.Items.Keys), StringComparer.Ordinal);

            foreach (var key in keys)
            {
                if (filter != null && !filter(key))
                {
                    continue;
                }

                var inThis = Items.TryGetValue(key, out var mine);
                var inOther = other.Items.TryGetValue(key, out var theirs);

                if (inThis && !inOther)
                {
                    differences.Add("removed " + key + " = " + mine);
                }
                else if (!inThis && inOther)
                {
                    differences.Add("added " + key + " = " + theirs);
                }
                else if (!string.Equals(mine, theirs, StringComparison.Ordinal))
                {
                    differences.Add("changed " + key + ": " + mine + " -> " + theirs);
                }

                if (differences.Count >= max)
                {
                    differences.Add("...");
                    break;
                }
            }

            return differences;
        }

        public string Digest()
        {
            var text = string.Join("\n", Items.Select(p => p.Key + "=" + p.Value));
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        }

        public string Summary() => Items.Count + " entries, sha256 " + Digest().Substring(0, 16);
    }
}
