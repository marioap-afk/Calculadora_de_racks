using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Catalogs.Validation
{
    /// <summary>
    /// Reviews a loaded <see cref="RackCatalog"/> (plus the raw <c>secciones.csv</c> rows) and returns a
    /// single diagnostic with severities across the five I-19 categories:
    /// <list type="number">
    ///   <item>duplicate ids (within a catalog and within the unified secciones sheet),</item>
    ///   <item>invalid references/relations (dangling FKs; repeated relationship keys that shadow a row),</item>
    ///   <item>missing blocks or views (blank block name; a referenced view that does not exist),</item>
    ///   <item>rows discarded by an unknown/blank "rol",</item>
    ///   <item>— when a library manifest is supplied — mismatches against <c>blocks-library.dwg</c>.</item>
    /// </list>
    /// PURE: it reads the catalog and reports. It NEVER edits a CSV/JSON and NEVER reads or writes any DWG.
    /// </summary>
    public static class CatalogValidator
    {
        /// <summary>
        /// Validate an already-loaded catalog. Supply <paramref name="rawSecciones"/> (the rows before the
        /// role split) to detect rows discarded by an unknown rol — without them that check is skipped, since
        /// the split has already dropped them. Supply <paramref name="libraryManifest"/> to fold the
        /// blocks-library.dwg comparison into the same report.
        /// </summary>
        public static CatalogValidationReport Validate(
            RackCatalog catalog,
            IReadOnlyList<SeccionCatalogEntry> rawSecciones = null,
            CatalogBlockManifest libraryManifest = null)
        {
            var report = new CatalogValidationReport();

            if (catalog == null)
            {
                report.Add(new CatalogValidationIssue(
                    CatalogValidationSeverity.Error,
                    CatalogValidationCategory.InvalidReference,
                    "NULL_CATALOG",
                    "El catálogo es nulo; no hay nada que validar."));
                return report;
            }

            var pieceIds = BuildPieceIdSet(catalog);
            var viewIds = ToIdSet(catalog.Views);
            var mensulaIds = ToIdSet(catalog.Mensulas);
            var connectionPointIds = ToIdSet(catalog.ConnectionPoints);

            ValidateDuplicateIds(catalog, rawSecciones, report);
            ValidateReferences(catalog, pieceIds, mensulaIds, connectionPointIds, report);
            ValidateBlocksAndViews(catalog, pieceIds, viewIds, report);
            ValidateDiscardedRows(rawSecciones, report);

            if (libraryManifest != null)
            {
                report.AddRange(CatalogBlockManifest.BuildExpected(catalog).Compare(libraryManifest));
            }

            return report;
        }

        /// <summary>
        /// Validate straight from a provider: loads the catalog AND the raw secciones rows so the
        /// discarded-row check is active. This is the entry point a command or the UI would call.
        /// </summary>
        public static CatalogValidationReport Validate(
            JsonRackCatalogProvider provider,
            CatalogBlockManifest libraryManifest = null)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return Validate(provider.Load(), provider.LoadSeccionRows(), libraryManifest);
        }

        /// <summary>Validate the catalog folder at <paramref name="directory"/>.</summary>
        public static CatalogValidationReport ValidateDirectory(
            string directory,
            CatalogBlockManifest libraryManifest = null) =>
            Validate(new JsonRackCatalogProvider(directory), libraryManifest);

        // ---- Category 1: duplicate ids ------------------------------------------------------------------

        private static void ValidateDuplicateIds(
            RackCatalog catalog,
            IReadOnlyList<SeccionCatalogEntry> rawSecciones,
            CatalogValidationReport report)
        {
            // Independent catalogs: an id must be unique within each.
            CheckListDuplicates("Placas base", catalog.BasePlates, report);
            CheckListDuplicates("Ménsulas", catalog.Mensulas, report);
            CheckListDuplicates("Cama de rodamiento", catalog.FlowBedProfiles, report);
            CheckListDuplicates("Seguridad", catalog.SafetyElements, report);
            CheckListDuplicates("Puntos de conexión", catalog.ConnectionPoints, report);
            CheckListDuplicates("Vistas", catalog.Views, report);

            if (rawSecciones != null && rawSecciones.Count > 0)
            {
                // secciones.csv is ONE sheet = ONE id namespace: any repeat (same or cross rol) is a duplicate.
                CheckSeccionDuplicates(rawSecciones, report);
            }
            else
            {
                // No raw rows (legacy split files or a bare catalog): fall back to per-list checks.
                CheckListDuplicates("Postes", catalog.PostProfiles, report);
                CheckListDuplicates("Celosías", catalog.TrussProfiles, report);
                CheckListDuplicates("Separadores", catalog.SpacerProfiles, report);
                CheckListDuplicates("Largueros", catalog.BeamProfiles, report);
            }
        }

        private static void CheckListDuplicates<TEntry>(
            string listLabel,
            IEnumerable<TEntry> entries,
            CatalogValidationReport report)
            where TEntry : CatalogEntryBase
        {
            if (entries == null)
            {
                return;
            }

            foreach (var group in entries
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Id))
                .GroupBy(entry => entry.Id.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var count = group.Count();
                if (count > 1)
                {
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.DuplicateId,
                        "DUPLICATE_ID",
                        "El id aparece " + count + " veces en " + listLabel + "; los lookups sólo ven la primera fila.",
                        group.Key));
                }
            }
        }

        private static void CheckSeccionDuplicates(
            IReadOnlyList<SeccionCatalogEntry> rows,
            CatalogValidationReport report)
        {
            foreach (var group in rows
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.Id))
                .GroupBy(row => row.Id.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var count = group.Count();
                if (count > 1)
                {
                    var roles = string.Join(", ", group
                        .Select(row => string.IsNullOrWhiteSpace(row.Rol) ? "(sin rol)" : row.Rol.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase));

                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.DuplicateId,
                        "DUPLICATE_SECCION_ID",
                        "El id se repite " + count + " veces en secciones.csv (roles: " + roles
                            + "); es una sola hoja con un solo espacio de ids.",
                        group.Key));
                }
            }
        }

        // ---- Category 2: invalid references / relations -------------------------------------------------

        private static void ValidateReferences(
            RackCatalog catalog,
            HashSet<string> pieceIds,
            HashSet<string> mensulaIds,
            HashSet<string> connectionPointIds,
            CatalogValidationReport report)
        {
            foreach (var beam in catalog.BeamProfiles ?? Enumerable.Empty<BeamProfileCatalogEntry>())
            {
                if (beam != null && !string.IsNullOrWhiteSpace(beam.Mensula) && !mensulaIds.Contains(beam.Mensula.Trim()))
                {
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.InvalidReference,
                        "INVALID_MENSULA_REF",
                        "El larguero apunta a una ménsula '" + beam.Mensula.Trim() + "' que no existe.",
                        Display(beam.Id)));
                }
            }

            foreach (var row in catalog.ConnectionLayout ?? Enumerable.Empty<ConnectionLayoutEntry>())
            {
                if (row == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(row.ConnectionPointId) && !connectionPointIds.Contains(row.ConnectionPointId.Trim()))
                {
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.InvalidReference,
                        "INVALID_CONNECTION_POINT_REF",
                        "La colocación referencia un punto de conexión '" + row.ConnectionPointId.Trim() + "' inexistente.",
                        LayoutKey(row)));
                }

                if (!string.IsNullOrWhiteSpace(row.PieceId) && !pieceIds.Contains(row.PieceId.Trim()))
                {
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.InvalidReference,
                        "INVALID_LAYOUT_PIECE_REF",
                        "La colocación referencia una pieza '" + row.PieceId.Trim() + "' que no existe en ningún catálogo.",
                        LayoutKey(row)));
                }
            }

            CheckLayoutKeyDuplicates(catalog, report);
            CheckBlockKeyDuplicates(catalog, report);
        }

        private static void CheckLayoutKeyDuplicates(RackCatalog catalog, CatalogValidationReport report)
        {
            foreach (var group in (catalog.ConnectionLayout ?? Enumerable.Empty<ConnectionLayoutEntry>())
                .Where(row => row != null
                    && !string.IsNullOrWhiteSpace(row.PieceId)
                    && !string.IsNullOrWhiteSpace(row.ConnectionPointId))
                .GroupBy(row => Key(row.PieceId, row.ConnectionPointId, row.View)))
            {
                if (group.Count() > 1)
                {
                    var first = group.First();
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Warning,
                        CatalogValidationCategory.InvalidReference,
                        "DUPLICATE_LAYOUT_KEY",
                        "La combinación pieza+punto+vista tiene " + group.Count()
                            + " filas; FindConnectionLayout sólo usa la primera.",
                        LayoutKey(first)));
                }
            }
        }

        private static void CheckBlockKeyDuplicates(RackCatalog catalog, CatalogValidationReport report)
        {
            foreach (var group in (catalog.Blocks ?? Enumerable.Empty<BlockCatalogEntry>())
                .Where(block => block != null
                    && !string.IsNullOrWhiteSpace(block.PieceId)
                    && !string.IsNullOrWhiteSpace(block.View))
                .GroupBy(block => Key(block.PieceId, block.View)))
            {
                if (group.Count() > 1)
                {
                    var first = group.First();
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Warning,
                        CatalogValidationCategory.InvalidReference,
                        "DUPLICATE_BLOCK_KEY",
                        "La combinación pieza+vista tiene " + group.Count()
                            + " bloques; FindBlock sólo usa el primero.",
                        first.PieceId.Trim() + " @ " + first.View.Trim()));
                }
            }
        }

        // ---- Category 3: missing blocks or views --------------------------------------------------------

        private static void ValidateBlocksAndViews(
            RackCatalog catalog,
            HashSet<string> pieceIds,
            HashSet<string> viewIds,
            CatalogValidationReport report)
        {
            foreach (var block in catalog.Blocks ?? Enumerable.Empty<BlockCatalogEntry>())
            {
                if (block == null)
                {
                    continue;
                }

                var where = BlockKey(block);

                if (string.IsNullOrWhiteSpace(block.BlockName))
                {
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.MissingBlockOrView,
                        "MISSING_BLOCK_NAME",
                        "La fila de bloque no tiene nombre de bloque; no se puede insertar nada.",
                        where));
                }

                if (string.IsNullOrWhiteSpace(block.View))
                {
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.MissingBlockOrView,
                        "MISSING_BLOCK_VIEW",
                        "La fila de bloque no declara vista.",
                        where));
                }
                else if (!viewIds.Contains(block.View.Trim()))
                {
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.MissingBlockOrView,
                        "MISSING_BLOCK_VIEW",
                        "La fila de bloque usa una vista '" + block.View.Trim() + "' que no existe en views.",
                        where));
                }

                // A block whose piece is in no catalog is a WARNING, not an error: generic library-only
                // blocks (e.g. TARIMA_GENERICA) legitimately have no commercial catalog row.
                var piece = (block.PieceId ?? string.Empty).Trim();
                if (piece.Length == 0 || !pieceIds.Contains(piece))
                {
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Warning,
                        CatalogValidationCategory.InvalidReference,
                        "UNRESOLVED_BLOCK_PIECE",
                        "El bloque referencia la pieza '" + (piece.Length == 0 ? "(vacía)" : piece)
                            + "', que no está en ningún catálogo de piezas.",
                        where));
                }
            }

            foreach (var row in catalog.ConnectionLayout ?? Enumerable.Empty<ConnectionLayoutEntry>())
            {
                if (row != null && !string.IsNullOrWhiteSpace(row.View) && !viewIds.Contains(row.View.Trim()))
                {
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.MissingBlockOrView,
                        "MISSING_LAYOUT_VIEW",
                        "La colocación usa una vista '" + row.View.Trim() + "' que no existe en views.",
                        LayoutKey(row)));
                }
            }
        }

        // ---- Category 4: rows discarded by rol ----------------------------------------------------------

        private static void ValidateDiscardedRows(
            IReadOnlyList<SeccionCatalogEntry> rawSecciones,
            CatalogValidationReport report)
        {
            if (rawSecciones == null)
            {
                return;
            }

            foreach (var row in rawSecciones)
            {
                if (row == null)
                {
                    continue;
                }

                if (SeccionRoles.Classify(row.Rol) == SeccionRole.Unknown)
                {
                    var rol = string.IsNullOrWhiteSpace(row.Rol) ? "(vacío)" : row.Rol.Trim();
                    report.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Warning,
                        CatalogValidationCategory.DiscardedRow,
                        "DISCARDED_SECCION_ROW",
                        "La fila de secciones.csv se descarta al cargar porque su rol '" + rol
                            + "' no se reconoce (POSTE / CELOSIA / LARGUERO / SEPARADOR).",
                        Display(row.Id)));
                }
            }
        }

        // ---- Helpers ------------------------------------------------------------------------------------

        private static HashSet<string> BuildPieceIdSet(RackCatalog catalog)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddIds(ids, catalog.PostProfiles);
            AddIds(ids, catalog.TrussProfiles);
            AddIds(ids, catalog.SpacerProfiles);
            AddIds(ids, catalog.BeamProfiles);
            AddIds(ids, catalog.BasePlates);
            AddIds(ids, catalog.Mensulas);
            AddIds(ids, catalog.FlowBedProfiles);
            AddIds(ids, catalog.SafetyElements);
            return ids;
        }

        private static HashSet<string> ToIdSet<TEntry>(IEnumerable<TEntry> entries) where TEntry : CatalogEntryBase
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddIds(ids, entries);
            return ids;
        }

        private static void AddIds<TEntry>(HashSet<string> ids, IEnumerable<TEntry> entries)
            where TEntry : CatalogEntryBase
        {
            if (entries == null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.Id))
                {
                    ids.Add(entry.Id.Trim());
                }
            }
        }

        private static string Key(params string[] parts) =>
            string.Join("|", parts.Select(part => (part ?? string.Empty).Trim().ToUpperInvariant()));

        private static string LayoutKey(ConnectionLayoutEntry row) =>
            Display(row.PieceId) + " / " + Display(row.ConnectionPointId) + " / " + Display(row.View);

        private static string BlockKey(BlockCatalogEntry block) =>
            Display(block.PieceId) + " @ " + Display(block.View);

        private static string Display(string value) =>
            string.IsNullOrWhiteSpace(value) ? "(vacío)" : value.Trim();
    }
}
