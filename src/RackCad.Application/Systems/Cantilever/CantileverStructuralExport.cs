using System;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The single-read entry of the Cantilever structural export (I-65 D-12, A65-F1-08, CD-I65-F4-11).
    ///
    /// <para>
    /// It receives what the envelope holds as primitive values: the rack id, the kind exactly as persisted, the station index and
    /// the BYTES of the persisted design text. It also receives the function that decodes those bytes into the design of the
    /// Cantilever line, which the CALLER provides (the command of the Plugin with the project store; the tests the same way): the
    /// station does not depend on persistence, so this file never names the store or the envelope.
    /// </para>
    /// <para>
    /// The function is applied once to those very bytes, and the same bytes, untouched, go to the scope, so the source fingerprint
    /// is computed over what is persisted and never over a design that was read and then written again. The function gets a copy:
    /// whatever it does to its array cannot change what is hashed.
    /// </para>
    /// <para>
    /// It builds nothing by itself: the model comes from <see cref="StructuralModelBuilder"/> with the Cantilever source. It does
    /// not analyse, read results or write files.
    /// </para>
    /// </summary>
    public static class CantileverStructuralExport
    {
        private const string DesignSubject = "design";
        private const string KindSubject = "kind";

        /// <summary>The kind of the envelope for a Cantilever line, as it is persisted; it is compared ignoring case.</summary>
        private const string PersistedKind = "cantilever";

        public static StructuralModelResult Build(
            string rackId,
            string kind,
            int stationIndex,
            byte[] designBytes,
            Func<byte[], CantileverLineDesign> decodeDesign,
            byte[] loadFileBytes,
            IStructuralMaterialProfile profile,
            StructuralSectionCatalog catalog)
        {
            if (rackId == null)
            {
                throw new ArgumentNullException(nameof(rackId));
            }

            if (kind == null)
            {
                throw new ArgumentNullException(nameof(kind));
            }

            if (designBytes == null)
            {
                throw new ArgumentNullException(nameof(designBytes));
            }

            if (decodeDesign == null)
            {
                throw new ArgumentNullException(nameof(decodeDesign));
            }

            if (loadFileBytes == null)
            {
                throw new ArgumentNullException(nameof(loadFileBytes));
            }

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            // The scope keeps a copy of the exact bytes and validates the identity (blank text, negative index) with the same
            // exceptions as everywhere else. It carries the canonical kind of the source, never the text of the envelope.
            var scope = new StructuralScope(rackId, CantileverStructuralModelSource.KindName, stationIndex, designBytes);

            var loads = StructuralLoadFileReader.Read(loadFileBytes);

            if (loads.Outcome != StructuralExportOutcome.Valid || loads.LoadFile == null)
            {
                return StructuralModelResult.Create(loads.Diagnostics, null);
            }

            if (!string.Equals(kind, PersistedKind, StringComparison.OrdinalIgnoreCase))
            {
                return StructuralModelResult.Create(
                    new[]
                    {
                        StructuralDiagnostic.Blocking(
                            StructuralDiagnosticCodes.SourceKindMismatch,
                            StructuralExportOutcome.Invalid,
                            string.IsNullOrWhiteSpace(kind) ? KindSubject : kind),
                    },
                    null);
            }

            CantileverLineDesign design;

            try
            {
                design = decodeDesign(scope.GetDesignBytes());
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception)
            {
                return StructuralModelResult.Create(
                    new[]
                    {
                        StructuralDiagnostic.Blocking(
                            StructuralDiagnosticCodes.SourceUnavailable, StructuralExportOutcome.Unavailable, DesignSubject),
                    },
                    null);
            }

            if (design == null)
            {
                return StructuralModelResult.Create(
                    new[]
                    {
                        StructuralDiagnostic.Blocking(
                            StructuralDiagnosticCodes.SourceDesignAbsent, StructuralExportOutcome.Invalid, DesignSubject),
                    },
                    null);
            }

            return StructuralModelBuilder.BuildStructuralModel(
                new CantileverStructuralModelSource(design, catalog),
                scope,
                loads,
                profile,
                catalog);
        }
    }
}
