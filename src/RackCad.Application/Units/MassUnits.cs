namespace RackCad.Application.Units
{
    /// <summary>
    /// The ONE owner of the pound mass (I-65 D-03, INV-16): 1 lb = 0.45359237 kg, exact by definition.
    ///
    /// Nothing else under <c>src/</c> may spell this literal; <c>StructuralSectionUnits</c> and <c>ForceUnits</c> consume
    /// it. A guard over the whole product tree keeps it that way.
    /// </summary>
    public static class MassUnits
    {
        /// <summary>Exact by definition: 1 lb = 0.45359237 kg.</summary>
        public const double KilogramsPerPound = 0.45359237;
    }
}
