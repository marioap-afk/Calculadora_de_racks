namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Pallet parameters that drive a dynamic (pallet flow) system. Dimensions are in inches;
    /// weight is kept as a magnitude with an explicit unit (kg by default) and is NOT converted.
    /// Only <see cref="Depth"/> drives the Phase 1 layout, but all inputs are stored so future
    /// capacity/height/level rules have their data without a model migration.
    /// </summary>
    public sealed class PalletSpecification
    {
        public PalletSpecification()
        {
        }

        public PalletSpecification(double front, double depth, double height, double weight, string weightUnit = "kg")
        {
            Front = front;
            Depth = depth;
            Height = height;
            Weight = weight;
            WeightUnit = weightUnit;
        }

        public double Front { get; set; }
        public double Depth { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public string WeightUnit { get; set; } = "kg";
    }
}
