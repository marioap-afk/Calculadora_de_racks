using System;

namespace RackCad.Application.Headers
{
    /// <summary>
    /// Every editable input for building a lateral header. The builder reads only from here, so there
    /// are no magic numbers buried in the logic — each tunable has a name and a sensible default.
    /// </summary>
    public sealed class LateralHeaderParameters
    {
        // ---- Overall geometry ----

        /// <summary>Header height; drives the post's dynamic <c>LONGITUD</c> parameter (in).</summary>
        public double Height { get; set; } = 132.0;

        /// <summary>Fondo: horizontal separation between the two posts (in).</summary>
        public double Depth { get; set; } = 42.0;

        // ---- Celosía (truss) layout ----

        /// <summary>Punch (troquel) pitch on the post (in). Troqueles repeat every PasoTroquel.</summary>
        public double PasoTroquel { get; set; } = 2.0;

        /// <summary>
        /// 1-based troquel index where the first horizontal sits. With pitch 2", a value of 3 places the
        /// first horizontal (3-1)*2 = 4" above the first troquel.
        /// </summary>
        public int InicioCelosiaTroquel { get; set; } = 3;

        /// <summary>Vertical clear between horizontals = panel height (in). Should be a multiple of PasoTroquel.</summary>
        public double ClaroPanel { get; set; } = 44.0;

        // ---- Diagonals ----

        /// <summary>The diagonal starts this many troqueles above the lower horizontal of its panel.</summary>
        public int OffsetDiagonalInicioTroqueles { get; set; } = 2;

        /// <summary>The diagonal ends this many troqueles below the upper horizontal of its panel.</summary>
        public int OffsetDiagonalFinTroqueles { get; set; } = 2;

        // ---- Top closing ----

        /// <summary>
        /// Leftover clear at the top (ValorClaroTravesaño). A negative value means "auto": compute the
        /// leftover from Height/ClaroPanel. When the resulting gap is &gt; 0 a closing horizontal is added.
        /// </summary>
        public double ValorClaroTravesano { get; set; } = -1.0;

        // ---- Catalog ids: what to insert ----

        public string PostId { get; set; }
        public string BasePlateId { get; set; }

        /// <summary>Truss profile id used for both horizontals and diagonals (they share one catalog).</summary>
        public string TrussProfileId { get; set; }

        // ---- Connection-point names: the logic is anchored on these ----

        public string MontajePostePoint { get; set; } = "MONTAJE_POSTE";
        public string TroquelCelosiaPoint { get; set; } = "TROQUEL_CELOSIA";
        public string CelosiaPoint { get; set; } = "CELOSIA";

        // ---- Dynamic-block parameter names ----

        /// <summary>Dynamic parameter that stretches the post to the header height.</summary>
        public string PostLengthParameter { get; set; } = "LONGITUD";

        /// <summary>Dynamic parameter that stretches a horizontal/diagonal to its length.</summary>
        public string MemberLengthParameter { get; set; } = "Distancia1";

        /// <summary>Orthographic view these blocks live in (lateral by default).</summary>
        public string View { get; set; } = "LATERAL";

        /// <summary>True when the top closing gap should be computed instead of taken from the field.</summary>
        public bool AutoClosing => ValorClaroTravesano < 0.0;

        public void Validate()
        {
            if (Height <= 0.0) throw new ArgumentOutOfRangeException(nameof(Height), "La altura debe ser > 0.");
            if (Depth <= 0.0) throw new ArgumentOutOfRangeException(nameof(Depth), "El fondo debe ser > 0.");
            if (PasoTroquel <= 0.0) throw new ArgumentOutOfRangeException(nameof(PasoTroquel), "El paso de troquel debe ser > 0.");
            if (InicioCelosiaTroquel < 1) throw new ArgumentOutOfRangeException(nameof(InicioCelosiaTroquel), "El troquel de inicio es 1-based (>= 1).");
            if (ClaroPanel <= 0.0) throw new ArgumentOutOfRangeException(nameof(ClaroPanel), "El claro de panel debe ser > 0.");
            if (OffsetDiagonalInicioTroqueles < 0 || OffsetDiagonalFinTroqueles < 0)
                throw new ArgumentOutOfRangeException(nameof(OffsetDiagonalInicioTroqueles), "Los offsets de diagonal no pueden ser negativos.");
        }
    }
}
