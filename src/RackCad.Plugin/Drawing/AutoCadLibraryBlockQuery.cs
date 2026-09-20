using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems.Shared;

namespace RackCad.Plugin.Drawing
{
    /// <summary>Observes block-definition availability without importing, repairing or mutating the database.</summary>
    internal sealed class AutoCadLibraryBlockQuery : ILibraryBlockQuery
    {
        private readonly Database database;

        internal AutoCadLibraryBlockQuery(Database database)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public IReadOnlyList<LibraryBlockAvailabilityFact> Query(
            IReadOnlyList<LibraryBlockRequirement> requirements)
        {
            if (requirements == null) throw new ArgumentNullException(nameof(requirements));

            var facts = new List<LibraryBlockAvailabilityFact>(requirements.Count);
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                foreach (var requirement in requirements)
                {
                    facts.Add(new LibraryBlockAvailabilityFact(
                        requirement,
                        blockTable.Has(requirement.Key)
                            ? LibraryBlockAvailability.Found
                            : LibraryBlockAvailability.Missing));
                }

                transaction.Commit();
            }

            return facts;
        }
    }
}
