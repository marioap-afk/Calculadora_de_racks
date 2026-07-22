using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Diagnostics;
using RackCad.Application.Headers;
using RackCad.Application.Systems;

namespace RackCad.Plugin.Headers
{
    /// <summary>
    /// Imports block definitions the active drawing is missing from a single library DWG kept next to the
    /// catalogs (<c>blocks-library.dwg</c>). The library is read from disk into a side database (it never has
    /// to be open in AutoCAD) and the needed definitions are cloned in. If the file or a block is absent the
    /// piece is simply left out (reported as missing, same as before), so this is safe and backward compatible.
    /// Must be called inside the document lock but OUTSIDE the drawing transaction.
    /// </summary>
    public static class BlockLibraryImporter
    {
        /// <summary>The library DWG path: the user override (from settings) if set, else the default next to the catalogs.</summary>
        public static string LibraryPath => BlockLibraryLocator.ResolvePath();

        public static int EnsureForPlan(Database db, DynamicSystemPlan plan)
        {
            if (plan == null)
            {
                return 0;
            }

            var names = plan.LooseInstances.Select(i => i.BlockName)
                .Concat(plan.Headers.SelectMany(g => g.Instances).Select(i => i.BlockName));
            return EnsureBlocks(db, names);
        }

        /// <summary>Clones every requested block that the drawing lacks from the library DWG. Returns the count imported.</summary>
        public static int EnsureBlocks(Database db, IEnumerable<string> blockNames)
        {
            if (db == null || blockNames == null)
            {
                return 0;
            }

            var wanted = blockNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (wanted.Count == 0)
            {
                return 0;
            }

            var missing = new List<string>();
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (var name in wanted)
                {
                    if (!blockTable.Has(name))
                    {
                        missing.Add(name);
                    }
                }

                transaction.Commit();
            }

            if (missing.Count == 0)
            {
                return 0;
            }

            var path = LibraryPath;
            if (!File.Exists(path))
            {
                return 0;
            }

            try
            {
                // The parsed library DWG is cached across draws (see AcquireLibrary); we only clone out of it here.
                var source = AcquireLibrary(path);
                if (source == null)
                {
                    return 0;
                }

                var ids = new ObjectIdCollection();
                using (var sourceTransaction = source.TransactionManager.StartTransaction())
                {
                    var sourceTable = (BlockTable)sourceTransaction.GetObject(source.BlockTableId, OpenMode.ForRead);
                    foreach (var name in missing)
                    {
                        if (sourceTable.Has(name))
                        {
                            ids.Add(sourceTable[name]);
                        }
                    }

                    sourceTransaction.Commit();
                }

                if (ids.Count == 0)
                {
                    return 0;
                }

                var mapping = new IdMapping();
                source.WblockCloneObjects(ids, db.BlockTableId, mapping, DuplicateRecordCloning.Ignore, deferTranslation: false);
                return ids.Count;
            }
            catch (Exception ex)
            {
                // Best-effort: a locked/invalid library must not abort the drawing; missing blocks are reported as before.
                RackLog.Exception("Importar bloques de la biblioteca DWG", ex);
                return 0;
            }
        }

        // ---- Session cache of the parsed library DWG ----
        // Reading blocks-library.dwg into a side Database (ReadDwgFile) is the dominant cost of an import, and drawing
        // several racks — or a selective's many cortes — would otherwise re-parse the same unchanged file each time.
        // Keep the parsed Database alive, keyed by the file's signature (path + last-write + size), and reuse it until
        // the file is edited or the chosen library path changes; then drop the stale parse and read a fresh one.
        // AutoCAD document operations are single-threaded, but the lock keeps the swap safe against any reentrancy.
        private static readonly object CacheGate = new object();
        private static string cachedPath;
        private static DateTime cachedWriteUtc;
        private static long cachedLength;
        private static Database cachedLibrary;

        private static Database AcquireLibrary(string path)
        {
            lock (CacheGate)
            {
                var info = new FileInfo(path);
                var writeUtc = info.LastWriteTimeUtc;
                var length = info.Length;

                if (cachedLibrary != null
                    && string.Equals(cachedPath, path, StringComparison.OrdinalIgnoreCase)
                    && cachedWriteUtc == writeUtc
                    && cachedLength == length)
                {
                    return cachedLibrary; // same file, unchanged — reuse the already-parsed database
                }

                var fresh = new Database(false, true);
                try
                {
                    // OpenForReadAndAllShare so it works even if the library DWG is open in AutoCAD.
                    fresh.ReadDwgFile(path, FileOpenMode.OpenForReadAndAllShare, allowCPConversion: true, password: null);
                }
                catch (Exception ex)
                {
                    // Best-effort: a locked/invalid library DWG must not abort the drawing (missing blocks reported).
                    RackLog.Exception("Leer blocks-library.dwg (" + path + ")", ex);
                    fresh.Dispose();
                    return null;
                }

                cachedLibrary?.Dispose(); // the prior parse (if any) is stale now; the last clone out of it already finished
                cachedLibrary = fresh;
                cachedPath = path;
                cachedWriteUtc = writeUtc;
                cachedLength = length;
                return cachedLibrary;
            }
        }
    }
}
