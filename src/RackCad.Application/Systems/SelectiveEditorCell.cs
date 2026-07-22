using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// One editable matrix cell of the selective editor (a bay's level): its pallet (frente/alto), how many pallets sit
    /// side by side, the beam and its peralte, plus optional manual overrides. This is the UI-facing edit model the
    /// advanced editor mutates; <see cref="SelectiveEditorState.BuildBayDesigns"/> turns it into the domain
    /// <see cref="SelectiveCell"/> the resolver consumes. Extracted verbatim from the private <c>Cell</c> of
    /// <c>RackSelectiveWindow</c> (initiative I-20) so the matrix model and its operations become pure and testable.
    /// </summary>
    public sealed class SelectiveEditorCell
    {
        public double Frente = 42.0;
        public double Alto = 60.0;
        public int PalletCount = 2;
        public string BeamId;
        public double BeamPeralte = SelectiveRackDefaults.DefaultBeamPeralte;

        /// <summary>Optional manual overrides (null = auto): larguero length and the clear below this level.</summary>
        public double? BeamLength;
        public double? Clear;

        public bool HasOverride => BeamLength.HasValue || Clear.HasValue;

        public SelectiveEditorCell Clone() => (SelectiveEditorCell)MemberwiseClone();

        public void CopyFrom(SelectiveEditorCell other)
        {
            Frente = other.Frente;
            Alto = other.Alto;
            PalletCount = other.PalletCount;
            BeamId = other.BeamId;
            BeamPeralte = other.BeamPeralte;
            BeamLength = other.BeamLength;
            Clear = other.Clear;
        }
    }
}
