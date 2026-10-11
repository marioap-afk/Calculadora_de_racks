using System;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The single-read entry of the Cantilever structural export (I-65 D-12, A65-F1-08, CD-I65-F4-11).
    ///
    /// <para>Skeleton of the red commit of F4-T2: the behaviour is completed in the next commit.</para>
    /// </summary>
    public static class CantileverStructuralExport
    {
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
            throw new NotImplementedException("The single-read entry arrives in the green commit of F4-T2.");
        }
    }
}
