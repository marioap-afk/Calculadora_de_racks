using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// Which END of an interval something sits at.
    ///
    /// It is a property of the INTERVAL, not of a station: interval <c>i</c> runs from station <c>i</c> to
    /// station <c>i+1</c>, so its <see cref="Left"/> end is on station <c>i</c> and its <see cref="Right"/>
    /// end on station <c>i+1</c>. Read the other way round — as "the interval on this station's left" — an
    /// interior station would have to decide which of its two intervals owns a shared piece, and the first and
    /// last station would each become a special case (ADR-0027, D3).
    /// </summary>
    public enum CantileverIntervalSide
    {
        /// <summary>The end at the LOWER station index, on that station's +X face.</summary>
        Left = 0,

        /// <summary>The end at the HIGHER station index, on that station's −X face.</summary>
        Right = 1
    }

    /// <summary>
    /// One separator column plate: 3 in × 3 in × 3/8 in with a single centred 9/16 in hole.
    ///
    /// Its identity is <c>(intervalIndex, intervalSide, separatorIndex)</c>. An interior station carries TWO
    /// plates at each separator elevation — one for the interval on either side, on opposite faces of the
    /// column — because each separator bolts to its own plate. Keying the plate to the interval is what stops
    /// the two intervals that meet there from producing the same plate twice (ADR-0027, D5).
    /// </summary>
    public sealed class CantileverSeparatorColumnPlatePlan
    {
        internal CantileverSeparatorColumnPlatePlan(
            int intervalIndex,
            CantileverIntervalSide side,
            int separatorIndex,
            CantileverPlatePlan plate,
            CantileverPunchPlan punch)
        {
            IntervalIndex = intervalIndex;
            Side = side;
            SeparatorIndex = separatorIndex;
            Plate = plate;
            Punch = punch;
        }

        public int IntervalIndex { get; }

        public CantileverIntervalSide Side { get; }

        /// <summary>
        /// The station this plate is welded to, DERIVED from the interval and the end. It is not stored: two
        /// numbers that must agree are one number.
        /// </summary>
        public int StationIndex => Side == CantileverIntervalSide.Left ? IntervalIndex : IntervalIndex + 1;

        /// <summary>Which separator elevation of the interval this plate serves, base zero and ascending.</summary>
        public int SeparatorIndex { get; }

        public CantileverPlatePlan Plate { get; }

        /// <summary>The single centred hole. Its DATUM is what the separator's end punch must match.</summary>
        public CantileverPunchPlan Punch { get; }

        public double ElevationZ => Punch.Datum.V;

        public string Key =>
            string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", IntervalIndex, Side, SeparatorIndex);

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "{0};t={1:0.######};z={2:0.######};x={3:0.######}",
            Key, Plate.Thickness, ElevationZ, Punch.Centre.X);

        public override string ToString() => "SeparatorPlate " + Signature();
    }

    /// <summary>One resolved separator: its profile and its four punches.</summary>
    public sealed class CantileverSeparatorPlan
    {
        internal CantileverSeparatorPlan(
            int intervalIndex,
            int separatorIndex,
            double elevationZ,
            CantileverStructuralMemberPlan member,
            IReadOnlyList<CantileverPunchPlan> punches,
            CantileverPunchPlan leftColumnPunch,
            CantileverPunchPlan leftBracePunch,
            CantileverPunchPlan rightBracePunch,
            CantileverPunchPlan rightColumnPunch)
        {
            IntervalIndex = intervalIndex;
            SeparatorIndex = separatorIndex;
            ElevationZ = elevationZ;
            Member = member;
            Punches = punches;
            LeftColumnPunch = leftColumnPunch;
            LeftBracePunch = leftBracePunch;
            RightBracePunch = rightBracePunch;
            RightColumnPunch = rightColumnPunch;
        }

        public int IntervalIndex { get; }

        public int SeparatorIndex { get; }

        public double ElevationZ { get; }

        public CantileverStructuralMemberPlan Member { get; }

        /// <summary>The four punches, left to right: column, brace, brace, column.</summary>
        public IReadOnlyList<CantileverPunchPlan> Punches { get; }

        public CantileverPunchPlan LeftColumnPunch { get; }

        public CantileverPunchPlan LeftBracePunch { get; }

        public CantileverPunchPlan RightBracePunch { get; }

        public CantileverPunchPlan RightColumnPunch { get; }

        public double CutLength => Member.NominalCutLength;

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "sep{0}/{1};{2}@{3:0.######};z={4:0.######};pch={5}",
            IntervalIndex, SeparatorIndex, Member.SectionId.Value, CutLength, ElevationZ, Punches.Count);

        public override string ToString() => "Separator " + Signature();
    }

    /// <summary>
    /// The end adapter of a cold-rolled brace: a 2 × 2 × 3/16 angle cut to 2 in, with a centred 9/16 hole on
    /// each of its two square faces and two gauge-10 gussets.
    ///
    /// It is modelled with its own geometry and is NOT added to the neutral catalogue: that catalogue holds
    /// standard SECTIONS (ADR-0020) and this is a fabricated part. Putting it there would mean inventing a
    /// provenance (ADR-0027, D7).
    /// </summary>
    public sealed class CantileverColdRolledAdapterPlan
    {
        internal CantileverColdRolledAdapterPlan(
            CantileverPieceId id,
            Point3D origin,
            StructuralSectionId sectionId,
            double leg,
            double cutLength,
            double thickness,
            CantileverPunchPlan separatorFacePunch,
            Point3D rodHoleCentre,
            double rodHoleDiameter,
            int gussetCount,
            int gussetGaugeNumber)
        {
            Id = id;
            Origin = origin;
            SectionId = sectionId;
            Leg = leg;
            CutLength = cutLength;
            Thickness = thickness;
            SeparatorFacePunch = separatorFacePunch;
            RodHoleCentre = rodHoleCentre;
            RodHoleDiameter = rodHoleDiameter;
            GussetCount = gussetCount;
            GussetGaugeNumber = gussetGaugeNumber;
        }

        public CantileverPieceId Id { get; }

        /// <summary>Where the adapter sits: the centre of its separator-facing hole.</summary>
        public Point3D Origin { get; }

        /// <summary>
        /// La sección de catálogo del ángulo. Lo que DIBUJA la pieza.
        ///
        /// Se añadió en la ronda 4 de I-37D: hasta entonces el contorno se construía a mano con
        /// <see cref="Leg"/> y <see cref="Thickness"/>, y salía una L de seis vértices sin filete de raíz ni
        /// radios de punta. Ahora el contorno viene de la tubería de secciones, que es la misma que dibuja
        /// columnas, brazos y separadores.
        /// </summary>
        public StructuralSectionId SectionId { get; }

        public double Leg { get; }

        public double CutLength { get; }

        public double Thickness { get; }

        /// <summary>The hole that bolts to the separator's brace punch. Its datum must match it.</summary>
        public CantileverPunchPlan SeparatorFacePunch { get; }

        /// <summary>
        /// The centre of the hole the rod passes through — one end of the rod's axis.
        ///
        /// It is a POINT and a diameter, and deliberately not a <see cref="CantileverPunchPlan"/>. A punch plan
        /// carries a <see cref="CantileverPunchDatum"/>, and a datum is an axis plus the two world coordinates
        /// that axis does not consume. This hole's axis is the rod's, which is diagonal in the X–Z plane, so no
        /// world axis describes it and any datum written for it would be a fiction that a later comparison
        /// would trust. The separator-facing hole, whose axis IS world Y and which really must coincide with
        /// the separator's, keeps its datum.
        /// </summary>
        public Point3D RodHoleCentre { get; }

        /// <summary>Diameter of the rod hole, inches.</summary>
        public double RodHoleDiameter { get; }

        public int GussetCount { get; }

        /// <summary>
        /// The gauge of this adapter's gussets, as a NUMBER.
        ///
        /// Ten, carried as identity and description. The repository has no gauge table and nothing converts a
        /// gauge to a thickness, so none is invented here (decision 12.30).
        /// </summary>
        public int GussetGaugeNumber { get; }

        /// <summary>The description a BOM line uses for one of this adapter's gussets.</summary>
        public string GussetDescription =>
            string.Format(CultureInfo.InvariantCulture, "Cartabon de adaptador CAL_{0}", GussetGaugeNumber);

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "L{0:0.###}x{0:0.###}x{1:0.####}@{2:0.###};gus={3}xCAL_{4}",
            Leg, Thickness, CutLength, GussetCount, GussetGaugeNumber);

        public override string ToString() => "ColdRolledAdapter " + Signature();
    }

    /// <summary>One resolved brace: a structural profile, or a rod with its two adapters.</summary>
    public sealed class CantileverBracePlan
    {
        internal CantileverBracePlan(
            int intervalIndex,
            int panelIndex,
            char diagonal,
            CantileverBraceBodyKind kind,
            Point3D lowerEnd,
            Point3D upperEnd,
            CantileverStructuralMemberPlan member,
            IReadOnlyList<CantileverPunchPlan> punches,
            double roundDiameter,
            IReadOnlyList<CantileverColdRolledAdapterPlan> adapters)
        {
            IntervalIndex = intervalIndex;
            PanelIndex = panelIndex;
            Diagonal = diagonal;
            Kind = kind;
            LowerEnd = lowerEnd;
            UpperEnd = upperEnd;
            Member = member;
            Punches = punches;
            RoundDiameter = roundDiameter;
            Adapters = adapters;
        }

        public int IntervalIndex { get; }

        public int PanelIndex { get; }

        /// <summary>'A' for lower-left to upper-right, 'B' for lower-right to upper-left.</summary>
        public char Diagonal { get; }

        public CantileverBraceBodyKind Kind { get; }

        /// <summary>The lower end of the brace AXIS: a separator brace punch, or an adapter's rod hole.</summary>
        public Point3D LowerEnd { get; }

        public Point3D UpperEnd { get; }

        /// <summary>The profile, when the brace is a structural section. Null for a cold-rolled rod.</summary>
        public CantileverStructuralMemberPlan Member { get; }

        /// <summary>The two end punches of a structural brace. Empty for a rod: its adapters carry the holes.</summary>
        public IReadOnlyList<CantileverPunchPlan> Punches { get; }

        /// <summary>Rod diameter, for a cold-rolled brace. NaN for a structural one.</summary>
        public double RoundDiameter { get; }

        /// <summary>The two adapters of a cold-rolled brace. Empty for a structural one.</summary>
        public IReadOnlyList<CantileverColdRolledAdapterPlan> Adapters { get; }

        /// <summary>
        /// The geometric length of the brace body.
        ///
        /// For a structural brace it is its nominal cut. For a rod it is the distance between the two adapter
        /// rod holes — the axis, and nothing added for threads, nuts or tolerance (ADR-0027, D7).
        /// </summary>
        public double BodyLength => Member != null
            ? Member.NominalCutLength
            : (UpperEnd - LowerEnd).Length;

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "brc{0}/{1}{2};{3};len={4:0.######}{5}",
            IntervalIndex, PanelIndex, Diagonal, Kind, BodyLength,
            Kind == CantileverBraceBodyKind.ColdRolledRound
                ? ";d=" + RoundDiameter.ToString("0.######", CultureInfo.InvariantCulture) +
                  ";adp=" + Adapters.Count
                : ";sec=" + Member.SectionId.Value);

        public override string ToString() => "Brace " + Signature();
    }

    /// <summary>One braced panel: its two separators and its two crossed braces.</summary>
    public sealed class CantileverBracedPanelPlan
    {
        internal CantileverBracedPanelPlan(
            int intervalIndex,
            int panelIndex,
            CantileverSeparatorPlan lowerSeparator,
            CantileverSeparatorPlan upperSeparator,
            CantileverBracePlan braceA,
            CantileverBracePlan braceB)
        {
            IntervalIndex = intervalIndex;
            PanelIndex = panelIndex;
            LowerSeparator = lowerSeparator;
            UpperSeparator = upperSeparator;
            BraceA = braceA;
            BraceB = braceB;
        }

        public int IntervalIndex { get; }

        public int PanelIndex { get; }

        public CantileverSeparatorPlan LowerSeparator { get; }

        public CantileverSeparatorPlan UpperSeparator { get; }

        /// <summary>Lower-left to upper-right.</summary>
        public CantileverBracePlan BraceA { get; }

        /// <summary>Lower-right to upper-left.</summary>
        public CantileverBracePlan BraceB { get; }

        public IReadOnlyList<CantileverBracePlan> Braces => new[] { BraceA, BraceB };

        /// <summary>
        /// The transverse coordinate the whole X lies in.
        ///
        /// The two braces share it: they are coplanar, may overlap visually, have no central joint and carry
        /// no offset to dodge each other. The MVP does not compute their interference and says so
        /// (ADR-0027, D6).
        /// </summary>
        public double PlaneY => BraceA.LowerEnd.Y;

        public string Signature() =>
            "pnl" + IntervalIndex + "/" + PanelIndex + "{" + BraceA.Signature() + "," + BraceB.Signature() + "}";

        public override string ToString() => "BracedPanel " + Signature();
    }

    /// <summary>
    /// One interval of a line: the space between two adjacent stations, with everything that braces it.
    ///
    /// An interval belongs to the PAIR. Its separators and braces are not the left station's nor the right
    /// station's — which is what stops the first and last station from being special cases, and what stops a
    /// separator from being counted twice when neighbouring intervals are walked (ADR-0027, D3).
    /// </summary>
    public sealed class CantileverIntervalAssembly
    {
        internal CantileverIntervalAssembly(
            int index,
            int leftStationIndex,
            int rightStationIndex,
            double columnCentreSpacing,
            IReadOnlyList<double> separatorElevations,
            IReadOnlyList<CantileverSeparatorColumnPlatePlan> columnPlates,
            IReadOnlyList<CantileverSeparatorPlan> separators,
            IReadOnlyList<CantileverBracedPanelPlan> bracedPanels,
            CantileverBracingLayout layout,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            Index = index;
            LeftStationIndex = leftStationIndex;
            RightStationIndex = rightStationIndex;
            ColumnCentreSpacing = columnCentreSpacing;
            SeparatorElevations = separatorElevations;
            ColumnPlates = columnPlates;
            Separators = separators;
            BracedPanels = bracedPanels;
            Layout = layout;
            Diagnostics = diagnostics;
        }

        public int Index { get; }

        public int LeftStationIndex { get; }

        public int RightStationIndex { get; }

        public double ColumnCentreSpacing { get; }

        public IReadOnlyList<double> SeparatorElevations { get; }

        /// <summary>Two plates per separator: one at each end of the interval.</summary>
        public IReadOnlyList<CantileverSeparatorColumnPlatePlan> ColumnPlates { get; }

        public IReadOnlyList<CantileverSeparatorPlan> Separators { get; }

        public IReadOnlyList<CantileverBracedPanelPlan> BracedPanels { get; }

        public CantileverBracingLayout Layout { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        public bool IsBlocked => Diagnostics.Any(d => d.IsBlocking);

        public IReadOnlyList<CantileverBracePlan> Braces =>
            BracedPanels.SelectMany(p => p.Braces).ToList();

        public IReadOnlyList<CantileverColdRolledAdapterPlan> Adapters =>
            Braces.SelectMany(b => b.Adapters).ToList();

        public IReadOnlyList<CantileverStructuralMemberPlan> Members =>
            Separators.Select(s => s.Member)
                .Concat(Braces.Where(b => b.Member != null).Select(b => b.Member))
                .ToList();

        public IReadOnlyList<CantileverPlatePlan> Plates =>
            ColumnPlates.Select(p => p.Plate).ToList();

        public IReadOnlyList<CantileverPunchPlan> Punches =>
            ColumnPlates.Select(p => p.Punch)
                .Concat(Separators.SelectMany(s => s.Punches))
                .Concat(Braces.SelectMany(b => b.Punches))
                .Concat(Adapters.Select(a => a.SeparatorFacePunch))
                .ToList();

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "int{0}[{1}-{2}]@{3:0.######};{4};plt={5};seps={6};brc={7}",
            Index, LeftStationIndex, RightStationIndex, ColumnCentreSpacing,
            Layout.Signature(),
            string.Join("+", ColumnPlates.Select(p => p.Signature())),
            string.Join("+", Separators.Select(s => s.Signature())),
            string.Join("+", Braces.Select(b => b.Signature())));

        public override string ToString() =>
            "Interval " + Index + " [" + LeftStationIndex + "-" + RightStationIndex + "] seps=" +
            Separators.Count + " braces=" + Braces.Count;
    }
}
