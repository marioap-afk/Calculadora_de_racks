using System;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>
    /// The deterministic translation of a Valid neutral model to the system and the convention of the analysis program
    /// (I-65 D-03, D-04, D-08). Pure: no I/O, no clock, no analysis.
    /// </summary>
    public static class RamElementsTranslator
    {
        public const string Units = "LB_IN";
        public const string LoadCaseCategory = "OTHER";
        public const string LoadDirection = "GLOBAL_Y";
        public const int MaxPointLoadsPerMember = 24;

        public static RamTranslationResult Translate(StructuralModelResult result, RamSectionMaterialTable table) =>
            throw new NotImplementedException();

        public static RamTranslationResult Translate(StructuralModel model, RamSectionMaterialTable table) =>
            throw new NotImplementedException();
    }
}
