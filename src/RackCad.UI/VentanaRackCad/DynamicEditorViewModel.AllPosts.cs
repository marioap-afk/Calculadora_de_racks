using System.Globalization;
using System.Linq;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 pp (I-64, fifth batch; the request of the Owner of 2026-11-26, «no sé si sea el mismo caso en el dinámico»): the Dinámico has ONE
    /// peralte of the post for the whole rack. The builder stamps it on every cabecera, the custom ones too
    /// (<c>DynamicRackSystemBuilder.ApplyPostPeralte</c>), and the classic header editor cannot give one module another one (§7.12.6), so
    /// there is no peralte post by post or module by module to put back: what was missing is to SEE it. «Peralte de poste» goes from
    /// «Poste y anotaciones» (folded) to «Lo esencial», with «Para todos los postes». The write is the same as before.
    /// <para>
    /// Correction 3 of the review: a base plate with a peralte of its own (<c>PeralteOverride</c>) keeps it when the peralte of the post
    /// changes (the physical model derives the plate from the post only when it has none), so the hint says how many cabeceras have one.
    /// </para>
    /// </summary>
    public sealed partial class DynamicEditorViewModel
    {
        /// <summary>The hint of «Peralte de poste» in «Lo esencial» when no cabecera has a base plate with a peralte of its own.</summary>
        internal const string PostPeralteHint = "Para todos los postes, también los de las cabeceras personalizadas.";

        private DynamicFieldItem PostPeralteEssential() =>
            new DynamicFieldItem("peralte", "Peralte de poste", "in", Num(_editor.PostPeralte), PostPeralteHintNow(), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !(value > 0.0)) return "El peralte de poste debe ser un número mayor que cero.";
                return Apply(() => _editor.PostPeralte = value, null);
            }, !CanEdit);

        // The cabeceras of the last valid system whose left or right base plate has a peralte of its own: those plates do not follow the post.
        private string PostPeralteHintNow()
        {
            var plates = _editor.System?.Modules
                .Where(m => m.IsHeader && m.AssociatedFrameConfiguration != null)
                .Count(m => m.AssociatedFrameConfiguration.LeftBasePlate?.PeralteOverride != null || m.AssociatedFrameConfiguration.RightBasePlate?.PeralteOverride != null) ?? 0;
            if (plates == 0) return PostPeralteHint;
            return PostPeralteHint + " " + (plates == 1
                ? "1 cabecera tiene placas base con peralte propio: no cambian con el poste."
                : plates.ToString(CultureInfo.InvariantCulture) + " cabeceras tienen placas base con peralte propio: no cambian con el poste.");
        }
    }
}
