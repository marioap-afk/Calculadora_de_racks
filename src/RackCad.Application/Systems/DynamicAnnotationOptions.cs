using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The drawing-annotation choices the dynamic editor writes onto a design (numbering, rack name, text scale, dimension
    /// detail and style). Extracted so <see cref="DynamicEditorDesignAssembler.BuildDesign"/> takes them as plain values
    /// instead of reading the window's checkboxes (I-21). The window fills this from its controls before assembling.
    /// </summary>
    public sealed class DynamicAnnotationOptions
    {
        public bool NumberFronts { get; set; }
        public bool NumberLevels { get; set; }
        public bool DrawRackName { get; set; }
        public double AnnotationScale { get; set; } = 1.0;
        public DimensionDetail Dimensions { get; set; } = DimensionDetail.None;
        public string DimensionStyle { get; set; }
    }
}
