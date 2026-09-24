using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Diagnostics;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Shared;

namespace RackCad.Plugin.Drawing
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

        public static int EnsureForPlan(Database db, HeaderRunPlan plan)
        {
            if (db == null || plan == null)
            {
                return 0;
            }

            var requirements = RackBlockRequirementExtractors.HeaderRun.Extract(plan);
            var result = LibraryBlockAvailabilityFlow.Observe(
                requirements,
                allowImport: true,
                new AutoCadLibraryBlockQuery(db),
                new ImportAdapter(db));
            return result.Import.ImportedCount;
        }

        /// <summary>Imports the exact library identities extracted by Application from a typed plan.</summary>
        public static int EnsureRequirements(
            Database db,
            IReadOnlyList<LibraryBlockRequirement> requirements)
            => db == null || requirements == null
                ? 0
                : EnsureBlocks(db, requirements.Select(requirement => requirement.Key));

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
                // The parsed library DWG is shared with the neutral external-library observer.
                var source = BlockLibraryDatabaseCache.Acquire(path);
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

        private sealed class ImportAdapter : ILibraryBlockImporter
        {
            private readonly Database database;

            internal ImportAdapter(Database database) => this.database = database;

            public LibraryBlockImportResult Ensure(IReadOnlyList<LibraryBlockRequirement> requirements)
                => new LibraryBlockImportResult(true, EnsureRequirements(database, requirements));
        }
    }
}
