using System;
using System.Collections.Generic;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The Cantilever source of the neutral analysis model (I-65 F1-T2). RED skeleton: it compiles and implements nothing.
    /// </summary>
    public sealed class CantileverStructuralModelSource : IStructuralModelSource
    {
        private readonly CantileverLineDesign _design;
        private readonly StructuralSectionCatalog _catalog;

        public CantileverStructuralModelSource(CantileverLineDesign design, StructuralSectionCatalog catalog)
        {
            _design = design ?? throw new ArgumentNullException(nameof(design));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public StructuralSourceDescription Describe(StructuralScope scope, StructuralLoadFile loads)
        {
            throw new NotImplementedException();
        }

        /// <summary>One resolved piece of the station as the idealisation sees it: its scoped id and its placement.</summary>
        internal sealed class StationPiece
        {
            public StationPiece(string pieceId, PrismaticSectionInstance placement)
            {
                PieceId = pieceId;
                Placement = placement;
            }

            public string PieceId { get; }

            public PrismaticSectionInstance Placement { get; }
        }

        internal static StructuralSourceDescription Idealise(
            StationPiece column, IReadOnlyList<StationPiece> arms, StructuralLoadFile loads)
        {
            throw new NotImplementedException();
        }

        internal static StructuralDirection PeralteAxis(PrismaticSectionInstance placement)
        {
            throw new NotImplementedException();
        }
    }
}
