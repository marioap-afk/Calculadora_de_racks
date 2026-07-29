namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The tabulated SECTION PROPERTIES of a shape, all optional (owner decision 12: a section is valid for
    /// drawing and BOM with its geometric dimensions and its weight, even if properties are incomplete).
    ///
    /// The single invariant, and the reason the whole neutral catalog needs a STRICT reader: a value here is
    /// either <c>null</c> — the source does not tabulate it for this shape — or a finite number that was
    /// actually published. It is NEVER zero because a parse failed. That distinction is the difference between
    /// "unknown" and "zero", and a tolerant reader destroys it silently.
    ///
    /// Units are US customary, matching the source's canonical block (ADR-0021 §8): in.², in.⁴, in.³, in.⁶ and
    /// inches, as the AISC Readme defines each variable. Slenderness ratios (<c>bf/2tf</c>, <c>b/t</c>,
    /// <c>b/tdes</c>, <c>h/tw</c>, <c>h/tdes</c>, <c>D/t</c>) are deliberately NOT carried: they are pure
    /// ratios of values already present and belong to the design/slenderness domain that ADR-0017 defers.
    /// </summary>
    public sealed class StructuralSectionProperties
    {
        /// <summary>An all-null instance, for a shape whose properties the source does not publish.</summary>
        public static readonly StructuralSectionProperties Empty = new StructuralSectionProperties();

        // ---- Common to every family ------------------------------------------------------------------

        /// <summary>AISC <c>A</c> — cross-sectional area, in.².</summary>
        public double? Area { get; init; }

        /// <summary>AISC <c>Ix</c> — moment of inertia about x, in.⁴.</summary>
        public double? Ix { get; init; }

        /// <summary>AISC <c>Zx</c> — plastic section modulus about x, in.³.</summary>
        public double? Zx { get; init; }

        /// <summary>AISC <c>Sx</c> — elastic section modulus about x, in.³.</summary>
        public double? Sx { get; init; }

        /// <summary>AISC <c>rx</c> — radius of gyration about x, in.</summary>
        public double? Rx { get; init; }

        /// <summary>AISC <c>Iy</c> — moment of inertia about y, in.⁴.</summary>
        public double? Iy { get; init; }

        /// <summary>AISC <c>Zy</c> — plastic section modulus about y, in.³.</summary>
        public double? Zy { get; init; }

        /// <summary>AISC <c>Sy</c> — elastic section modulus about y, in.³.</summary>
        public double? Sy { get; init; }

        /// <summary>AISC <c>ry</c> — radius of gyration about y, in.</summary>
        public double? Ry { get; init; }

        /// <summary>AISC <c>J</c> — torsional constant, in.⁴.</summary>
        public double? J { get; init; }

        // ---- Principal (z/w) axes: single angles ------------------------------------------------------

        /// <summary>AISC <c>Iz</c> — moment of inertia about the z (minor principal) axis, in.⁴.</summary>
        public double? Iz { get; init; }

        /// <summary>AISC <c>rz</c> — radius of gyration about z, in.</summary>
        public double? Rz { get; init; }

        /// <summary>AISC <c>Sz</c> — elastic section modulus about z, in.³.</summary>
        public double? Sz { get; init; }

        /// <summary>AISC <c>Iw</c> — moment of inertia about the w (major principal) axis, in.⁴.</summary>
        public double? Iw { get; init; }

        /// <summary>AISC <c>tan(α)</c> — tangent of the angle between the y-y and z-z axes, single angles.</summary>
        public double? TanAlpha { get; init; }

        // ---- Torsion and warping ----------------------------------------------------------------------

        /// <summary>AISC <c>Cw</c> — warping constant, in.⁶. Not tabulated for HSS.</summary>
        public double? Cw { get; init; }

        /// <summary>AISC <c>C</c> — HSS torsional constant, in.³. Only HSS publishes it.</summary>
        public double? HssTorsionalConstant { get; init; }

        /// <summary>AISC <c>Wno</c> — normalized warping function (Design Guide 9), in.².</summary>
        public double? Wno { get; init; }

        /// <summary>AISC <c>Sw1</c> — warping statical moment at point 1, in.⁴.</summary>
        public double? Sw1 { get; init; }

        /// <summary>AISC <c>Sw2</c> — warping statical moment at point 2, in.⁴. Channels only.</summary>
        public double? Sw2 { get; init; }

        /// <summary>AISC <c>Sw3</c> — warping statical moment at point 3, in.⁴. Channels only.</summary>
        public double? Sw3 { get; init; }

        /// <summary>AISC <c>Qf</c> — statical moment for a point in the flange over the web edge, in.³.</summary>
        public double? Qf { get; init; }

        /// <summary>AISC <c>Qw</c> — statical moment at mid-depth, in.³.</summary>
        public double? Qw { get; init; }

        /// <summary>AISC <c>ro</c> — polar radius of gyration about the shear centre, in.</summary>
        public double? Ro { get; init; }

        /// <summary>AISC <c>H</c> — flexural constant. Only tabulated for equal-leg angles and channels.</summary>
        public double? FlexuralConstantH { get; init; }

        /// <summary>AISC <c>rts</c> — effective radius of gyration, in.</summary>
        public double? Rts { get; init; }

        /// <summary>AISC <c>ho</c> — distance between the flange centroids, in.</summary>
        public double? Ho { get; init; }

        // ---- Single-angle points A, B, C (AISC Figure 3) ----------------------------------------------

        /// <summary>AISC <c>zA</c> — point A to the centre of gravity along z, in.</summary>
        public double? ZA { get; init; }

        /// <summary>AISC <c>zB</c> — point B to the centre of gravity along z, in.</summary>
        public double? ZB { get; init; }

        /// <summary>AISC <c>zC</c> — point C to the centre of gravity along z, in.</summary>
        public double? ZC { get; init; }

        /// <summary>AISC <c>wA</c> — point A to the centre of gravity along w, in.</summary>
        public double? WA { get; init; }

        /// <summary>AISC <c>wB</c> — point B to the centre of gravity along w, in.</summary>
        public double? WB { get; init; }

        /// <summary>AISC <c>wC</c> — point C to the centre of gravity along w, in.</summary>
        public double? WC { get; init; }

        /// <summary>AISC <c>SwA</c> — elastic section modulus about w at point A, in.³.</summary>
        public double? SwA { get; init; }

        /// <summary>AISC <c>SwB</c> — elastic section modulus about w at point B, in.³. Often absent.</summary>
        public double? SwB { get; init; }

        /// <summary>AISC <c>SwC</c> — elastic section modulus about w at point C, in.³.</summary>
        public double? SwC { get; init; }

        /// <summary>AISC <c>SzA</c> — elastic section modulus about z at point A, in.³.</summary>
        public double? SzA { get; init; }

        /// <summary>AISC <c>SzB</c> — elastic section modulus about z at point B, in.³.</summary>
        public double? SzB { get; init; }

        /// <summary>AISC <c>SzC</c> — elastic section modulus about z at point C, in.³.</summary>
        public double? SzC { get; init; }

        // ---- Perimeters (AISC Design Guide 19) --------------------------------------------------------

        /// <summary>AISC <c>PA</c> — shape perimeter minus one flange surface (or short leg), in.</summary>
        public double? PA { get; init; }

        /// <summary>AISC <c>PA2</c> — single-angle perimeter minus the long leg surface, in.</summary>
        public double? PA2 { get; init; }

        /// <summary>AISC <c>PB</c> — shape perimeter, in.</summary>
        public double? PB { get; init; }

        /// <summary>AISC <c>PC</c> — box perimeter minus one flange surface, in.</summary>
        public double? PC { get; init; }

        /// <summary>AISC <c>PD</c> — box perimeter, in.</summary>
        public double? PD { get; init; }
    }
}
