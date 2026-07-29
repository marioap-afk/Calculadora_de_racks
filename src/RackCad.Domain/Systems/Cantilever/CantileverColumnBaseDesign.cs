namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// The editable intent of a Cantilever column–base sub-assembly: a column, a base and the connection
    /// they share.
    ///
    /// This is the ROOT of what I-37A persists conceptually, and it holds no resolved coordinate: no punch
    /// elevation, no plate height, no gusset leg, no envelope. Every one of those is derived by the
    /// Application resolver from the sections' geometry, and a design that stored them would be storing an
    /// answer that its own inputs already determine.
    ///
    /// What is deliberately NOT here, because I-37A does not implement it: arms and their slope, levels,
    /// the rest of the station, a second side, spacers, braces and the run. Adding any of them is a later
    /// initiative, not a property.
    /// </summary>
    public sealed class CantileverColumnBaseDesign
    {
        public CantileverColumnDesign Column { get; set; } = new CantileverColumnDesign();

        public CantileverBaseDesign Base { get; set; } = new CantileverBaseDesign();

        public CantileverColumnBaseConnectionDesign Connection { get; set; } =
            new CantileverColumnBaseConnectionDesign();

        public CantileverColumnBaseDesign DeepCopy() =>
            new CantileverColumnBaseDesign
            {
                Column = Column?.DeepCopy() ?? new CantileverColumnDesign(),
                Base = Base?.DeepCopy() ?? new CantileverBaseDesign(),
                Connection = Connection?.DeepCopy() ?? new CantileverColumnBaseConnectionDesign()
            };
    }
}
