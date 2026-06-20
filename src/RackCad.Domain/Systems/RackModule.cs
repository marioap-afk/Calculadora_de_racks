namespace RackCad.Domain.Systems
{
    /// <summary>
    /// One positioned segment of the side-view layout. Modules tile the run along X:
    /// a length-bearing module spans [StartOffset, EndOffset]; an intermediate post has
    /// StartOffset == EndOffset (zero length).
    /// </summary>
    public sealed class RackModule
    {
        public RackModule(int index, RackModuleKind kind, double startOffset, double endOffset)
        {
            Index = index;
            Kind = kind;
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        public int Index { get; }
        public RackModuleKind Kind { get; }
        public double StartOffset { get; }
        public double EndOffset { get; }
        public double Length => EndOffset - StartOffset;
        public bool HasLength => Length > 0.0;
    }
}
