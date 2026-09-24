using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Diagnostics;
using RackCad.Application.Systems.Shared;

namespace RackCad.Plugin.Drawing
{
    /// <summary>Observes availability and block presence directly in the configured external library DWG.</summary>
    internal sealed class AutoCadExternalLibraryBlockQuery : ILibraryPieceAvailabilityQuery
    {
        public IReadOnlyList<LibraryKeyAvailabilityObservation> Query(
            IReadOnlyList<LibraryBlockRequirement> requirements)
        {
            if (requirements == null) throw new ArgumentNullException(nameof(requirements));

            var path = BlockLibraryLocator.ResolvePath();
            if (!File.Exists(path))
            {
                return Unknown(requirements, LibraryAvailability.FileMissing);
            }

            try
            {
                var source = BlockLibraryDatabaseCache.Acquire(path);
                if (source == null)
                {
                    return Unknown(requirements, LibraryAvailability.Unknown);
                }

                var observations = new List<LibraryKeyAvailabilityObservation>(requirements.Count);
                using (var transaction = source.TransactionManager.StartTransaction())
                {
                    var sourceTable = (BlockTable)transaction.GetObject(source.BlockTableId, OpenMode.ForRead);
                    foreach (var requirement in requirements)
                    {
                        observations.Add(ExternalLibraryAvailabilityFacts.Observe(
                            requirement.Key,
                            LibraryAvailability.Ok,
                            sourceTable.Has(requirement.Key)
                                ? ExternalLibraryBlockObservation.Present
                                : ExternalLibraryBlockObservation.Missing));
                    }

                    transaction.Commit();
                }

                return observations;
            }
            catch (Exception ex)
            {
                RackLog.Exception("Consultar bloques de la biblioteca DWG", ex);
                return Unknown(requirements, LibraryAvailability.Unknown);
            }
        }

        private static IReadOnlyList<LibraryKeyAvailabilityObservation> Unknown(
            IReadOnlyList<LibraryBlockRequirement> requirements,
            LibraryAvailability availability)
        {
            var observations = new List<LibraryKeyAvailabilityObservation>(requirements.Count);
            foreach (var requirement in requirements)
            {
                observations.Add(ExternalLibraryAvailabilityFacts.Observe(
                    requirement.Key,
                    availability,
                    ExternalLibraryBlockObservation.NotObserved));
            }

            return observations;
        }
    }
}
