using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Larguero
{
    /// <summary>
    /// What «Insertar en el dibujo…» of the page «Larguero» places (I-64, sexta tanda, part «lg»): the larguero with its two ménsulas (the
    /// Owner, point 2: «Pieza suelta»), or, with an AISC section, the bare profile as RACKSECCION draws it (point 6: «Sí, una sola página»).
    /// </summary>
    public enum LargueroPieceKind
    {
        /// <summary>The profile in elevation with its two ménsulas.</summary>
        Larguero = 0,

        /// <summary>Only the profile, with the geometry, the views and the block of RACKSECCION (an AISC section).</summary>
        Profile = 1,
    }

    /// <summary>What a curve of a piece is: the profile, a ménsula, or a note of the drawing (the axis or the envelope of a section).</summary>
    public enum LargueroCurveRole
    {
        Profile = 0,
        Mensula = 1,
        Annotation = 2,
    }

    /// <summary>One polyline of a piece: projected points in inches, a role, and whether it closes (the last point is not repeated).</summary>
    public sealed class LargueroPieceCurve
    {
        public LargueroPieceCurve(LargueroCurveRole role, IReadOnlyList<Point2D> points, bool isClosed)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count < 2) throw new ArgumentException("Una curva necesita al menos dos puntos.", nameof(points));
            Role = role;
            Points = points.ToArray();
            IsClosed = isClosed;
        }

        public LargueroCurveRole Role { get; }

        public IReadOnlyList<Point2D> Points { get; }

        public bool IsClosed { get; }
    }

    /// <summary>
    /// How a ménsula sits at each end of the profile in the elevation of a larguero (supuesto T6lg-S4: the Owner left it [PENDIENTE] and asked
    /// for the simplest choice coherent with the blocks that exist). The FRONTAL block of a larguero of RackCad has its origin on the troquel
    /// of the post and its profile starting <c>INICIO_PERFIL</c> X further (1 in for the one larguero whose row the catalog has today), so:
    /// the ménsula is a plate at each end whose outer edge is the face of the post (x = 0 at the left end), the profile starts
    /// <see cref="OverhangOf"/> further in, the plate is <see cref="PlateWidth"/> wide (half outside the profile, half over it) and as high as
    /// its row in mensulas.csv, and its upper edge is flush with the upper edge of the profile (y = 0: the surface where the tarima rests).
    /// </summary>
    public static class LargueroMensulaSeat
    {
        /// <summary>The overhang when the catalog has no <c>INICIO_PERFIL</c> FRONTAL for the section (every AISC section): the 1 in of today.</summary>
        public const double DefaultOverhang = 1.0;

        /// <summary>The width of the plate of the ménsula along the larguero (mensulas.csv gives only its height).</summary>
        public const double PlateWidth = 2.0;

        /// <summary>The height of a ménsula whose row gives none.</summary>
        public const double DefaultHeight = 6.0;

        /// <summary>The overhang of the profile beyond the face of the post: <c>INICIO_PERFIL</c> X of the FRONTAL of the larguero, else 1 in.</summary>
        public static double OverhangOf(RackCatalog catalog, LargueroSectionOption section)
        {
            if (section?.Source == LargueroSectionSource.RackCad)
            {
                var entry = catalog?.ConnectionLayout.FindConnectionLayout(section.Id, SelectiveRackDefaults.BeamProfileStartPoint, "FRONTAL");
                if (entry != null && entry.LocalX > 0.0 && entry.LocalX < 12.0) return entry.LocalX;
            }

            return DefaultOverhang;
        }
    }

    /// <summary>
    /// The ONE plan of a piece of the page «Larguero»: what its view draws and what AutoCAD inserts (as ADR-0022 §7 for a section: the picture
    /// the user approved and the entities that reach the drawing are the same object). Immutable: the request of the door holds it as it is.
    /// </summary>
    public sealed class LargueroPiecePlan
    {
        internal LargueroPiecePlan(
            LargueroPieceKind kind,
            string name,
            string blockBaseName,
            IReadOnlyList<LargueroPieceCurve> curves,
            StructuralSectionRepresentationPlan sectionPlan,
            StructuralSectionDefinition section,
            double length,
            double peralte,
            double mensulaHeight,
            double overhang,
            string sectionLabel,
            string mensulaLabel)
        {
            if (curves == null || curves.Count == 0) throw new ArgumentException("Una pieza sin curvas no dibuja nada.", nameof(curves));
            Kind = kind;
            Name = name ?? string.Empty;
            BlockBaseName = blockBaseName ?? string.Empty;
            Curves = curves.ToArray();
            SectionPlan = sectionPlan;
            Section = section;
            Length = length;
            Peralte = peralte;
            MensulaHeight = mensulaHeight;
            Overhang = overhang;
            SectionLabel = sectionLabel ?? string.Empty;
            MensulaLabel = mensulaLabel ?? string.Empty;
            Bounds = Bounds2D.FromPoints(Curves.SelectMany(c => c.Points));
        }

        public LargueroPieceKind Kind { get; }

        /// <summary>The name of the larguero (the one typed, or the automatic one), or the designation of a bare profile.</summary>
        public string Name { get; }

        /// <summary>The start of the name of the block of a larguero («RACKCAD_LARGUERO_…»); the block of a bare profile is named by RACKSECCION.</summary>
        public string BlockBaseName { get; }

        /// <summary>Every curve, in inches: for a larguero the profile and its ménsulas; for a bare profile the curves of <see cref="SectionPlan"/>.</summary>
        public IReadOnlyList<LargueroPieceCurve> Curves { get; }

        public Bounds2D Bounds { get; }

        /// <summary>The plan of RACKSECCION of an AISC section (the elevation of a larguero, or the bare profile itself); null for RackCad.</summary>
        public StructuralSectionRepresentationPlan SectionPlan { get; }

        /// <summary>The AISC section; null for a larguero of RackCad.</summary>
        public StructuralSectionDefinition Section { get; }

        public double Length { get; }

        public double Peralte { get; }

        /// <summary>The height of the ménsulas drawn; 0 for a bare profile.</summary>
        public double MensulaHeight { get; }

        /// <summary>Where the profile starts from the outer edge of the left ménsula (<see cref="LargueroMensulaSeat"/>); 0 for a bare profile.</summary>
        public double Overhang { get; }

        /// <summary>The section by the name of the page («Escalón calibre 14», «C10X15.3»).</summary>
        public string SectionLabel { get; }

        public string MensulaLabel { get; }

        /// <summary>The geometry carries a declared convention of RackCad (the S family, ADR-0023): every message about it warns, always.</summary>
        public bool IsVisualDerived => SectionPlan?.IsVisualDerived == true;

        /// <summary>The length of the elevation of a larguero, end to end: the profile and its two overhangs.</summary>
        public double OverallLength => Kind == LargueroPieceKind.Larguero ? Length + 2.0 * Overhang : Length;
    }

    /// <summary>
    /// Builds the plan of a piece of the page «Larguero» (I-64, sexta tanda, part «lg»). Pure. For an AISC section it uses the geometry of
    /// RACKSECCION and nothing else (<see cref="StructuralSectionPlanBuilder"/>, the inspector's defaults: wireframe, no axis, no envelope, no
    /// rotation, no mirror): the elevation of a larguero is its view <see cref="SectionViewKind.LongitudinalX"/>, and a bare profile takes the
    /// view and the detail chosen. A larguero of RackCad has no neutral geometry (its real drawing is the block of the library that Selectivo
    /// places), so its profile is drawn as the rectangle of its length and its peralte (supuesto T6lg-S5).
    /// </summary>
    public static class LargueroPiecePlanner
    {
        public const string NoSectionText = "Elige una sección.";
        public const string NoLengthText = "Escribe una longitud a corte mayor que 0.";
        public const string NoPeralteText = "Elige el peralte.";
        public const string NoMensulaText = "Elige la ménsula: un larguero se dibuja con sus 2 ménsulas.";
        public const string ProfileOnlyAiscText = "Solo un perfil AISC se inserta sin ménsulas (como RACKSECCION).";
        public const string NoAiscCatalogText = "El catálogo de perfiles AISC no está disponible: no hay geometría que dibujar.";

        /// <param name="kind">The larguero with its ménsulas, or the bare profile (an AISC section only).</param>
        /// <param name="section">The section chosen.</param>
        /// <param name="peralte">The peralte of a larguero of RackCad; ignored for an AISC section (its geometry gives its depth).</param>
        /// <param name="length">The length to cut, in inches.</param>
        /// <param name="mensula">The ménsula (required for a larguero).</param>
        /// <param name="catalog">The catalog of RackCad (the overhang of its largueros).</param>
        /// <param name="factory">The geometry of the AISC sections (the one of RACKSECCION); required for an AISC section.</param>
        /// <param name="name">The name of the larguero (typed, or the automatic one).</param>
        /// <param name="view">The view of a bare profile.</param>
        /// <param name="detail">The detail of an AISC section.</param>
        public static (LargueroPiecePlan Plan, string Refusal) Build(
            LargueroPieceKind kind,
            LargueroSectionOption section,
            double? peralte,
            double? length,
            MensulaCatalogEntry mensula,
            RackCatalog catalog,
            StructuralSectionGeometryFactory factory,
            string name,
            SectionViewKind view = SectionViewKind.CrossSection,
            SectionDetailLevel detail = SectionDetailLevel.Tabulated)
        {
            if (section == null) return (null, NoSectionText);
            if (length == null || !(length.Value > 0.0) || double.IsInfinity(length.Value)) return (null, NoLengthText);
            if (kind == LargueroPieceKind.Profile && section.Source != LargueroSectionSource.Aisc) return (null, ProfileOnlyAiscText);
            if (section.Source == LargueroSectionSource.Aisc && (section.Aisc == null || factory == null)) return (null, NoAiscCatalogText);
            var cut = length.Value;

            if (kind == LargueroPieceKind.Profile)
            {
                var plan = SectionPlan(section, factory, cut, view, detail);
                var curves = plan.Curves.Select(c => new LargueroPieceCurve(RoleOf(c.Role), c.Points, c.IsClosed)).ToList();
                return (new LargueroPiecePlan(kind, section.Name, string.Empty, curves, plan, section.Aisc, cut, plan.Bounds.Height, 0.0, 0.0,
                    section.Name, string.Empty), null);
            }

            if (mensula == null) return (null, NoMensulaText);
            var overhang = LargueroMensulaSeat.OverhangOf(catalog, section);
            var height = mensula.Height > 0.0 && !double.IsInfinity(mensula.Height) ? mensula.Height : LargueroMensulaSeat.DefaultHeight;
            var pieces = new List<LargueroPieceCurve>();
            StructuralSectionRepresentationPlan sectionPlan = null;
            double depth;

            if (section.Source == LargueroSectionSource.Aisc)
            {
                // The elevation of RACKSECCION, moved so that the profile starts at the overhang and its upper edge is y = 0.
                sectionPlan = SectionPlan(section, factory, cut, SectionViewKind.LongitudinalX, detail);
                var dx = overhang - sectionPlan.Bounds.MinX;
                var dy = -sectionPlan.Bounds.MaxY;
                foreach (var curve in sectionPlan.Curves)
                    pieces.Add(new LargueroPieceCurve(RoleOf(curve.Role), curve.Points.Select(p => new Point2D(p.X + dx, p.Y + dy)).ToList(), curve.IsClosed));
                depth = sectionPlan.Bounds.Height;
            }
            else
            {
                if (peralte == null || !(peralte.Value > 0.0) || (section.HasPeralteChoice && !section.Peraltes.Contains(peralte.Value)))
                    return (null, NoPeralteText);
                depth = peralte.Value;
                pieces.Add(Rectangle(LargueroCurveRole.Profile, overhang, -depth, overhang + cut, 0.0));
            }

            var overall = cut + 2.0 * overhang;
            var plate = Math.Min(LargueroMensulaSeat.PlateWidth, overall / 2.0);
            pieces.Add(Rectangle(LargueroCurveRole.Mensula, 0.0, -height, plate, 0.0));
            pieces.Add(Rectangle(LargueroCurveRole.Mensula, overall - plate, -height, overall, 0.0));

            var label = string.IsNullOrWhiteSpace(name) ? section.Name : name.Trim();
            return (new LargueroPiecePlan(kind, label, BlockBaseNameOf(section, depth, cut), pieces, sectionPlan, section.Aisc, cut, depth, height, overhang,
                section.Name, mensula.Label), null);
        }

        /// <summary>«RACKCAD_LARGUERO_&lt;id&gt;[_P&lt;peralte&gt;]_L&lt;length&gt;», only with what a name of AutoCAD accepts; a name, never a key.</summary>
        public static string BlockBaseNameOf(LargueroSectionOption section, double peralte, double length)
        {
            var text = new StringBuilder("RACKCAD_LARGUERO_").Append(section?.Id ?? "LARGUERO");
            if (section?.Source == LargueroSectionSource.RackCad) text.Append("_P").Append(Number(peralte));
            text.Append("_L").Append(Number(length));
            var builder = new StringBuilder(text.Length);
            foreach (var ch in text.ToString())
                builder.Append((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '-' ? ch : '_');
            return builder.ToString();
        }

        // The plan of RACKSECCION for the section, exactly as its inspector builds it with its defaults.
        private static StructuralSectionRepresentationPlan SectionPlan(LargueroSectionOption section, StructuralSectionGeometryFactory factory, double length,
            SectionViewKind view, SectionDetailLevel detail)
        {
            var geometry = factory.Get(section.Aisc, detail);
            var instance = PrismaticSectionInstance.Create(section.Aisc.SectionId, length, null, 0.0, false);
            return StructuralSectionPlanBuilder.Build(geometry, instance, new SectionRepresentationOptions
            {
                Viewpoint = SectionViewpoint.Standard(view),
                Mode = SectionRepresentationMode.Wireframe,
                Detail = detail,
                IncludeAxis = false,
                IncludeEnvelope = false,
            });
        }

        private static LargueroCurveRole RoleOf(SectionCurveRole role) =>
            role == SectionCurveRole.Axis || role == SectionCurveRole.Envelope ? LargueroCurveRole.Annotation : LargueroCurveRole.Profile;

        private static LargueroPieceCurve Rectangle(LargueroCurveRole role, double x0, double y0, double x1, double y1) =>
            new LargueroPieceCurve(role, new[] { new Point2D(x0, y0), new Point2D(x1, y0), new Point2D(x1, y1), new Point2D(x0, y1) }, isClosed: true);

        private static string Number(double value) => value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', '_');
    }

    /// <summary>One line of the sketch of a piece: two points of the view, in inches, and what it draws.</summary>
    public readonly record struct LargueroSketchLine(LargueroCurveRole Role, Point2D From, Point2D To);

    /// <summary>
    /// The view of the page «Larguero» (the Owner, point 3: «un alzado esquemático con el peralte y la ménsula a escala»). The plan is drawn to
    /// ONE scale; a long piece seen along its length is broken in the middle (the two ends to scale, with a gap marked as a break), the way a
    /// drawing shows a long beam, so the peralte and the ménsula can be read. Pure: lines in inches, ready to be scaled into any surface.
    /// </summary>
    public sealed class LargueroPieceSketch
    {
        /// <summary>The gap between the two ends of a broken piece, in inches of the sketch.</summary>
        public const double BreakGap = 3.0;

        private LargueroPieceSketch(IReadOnlyList<LargueroSketchLine> lines, double? breakAt, Bounds2D bounds, double shownEnd)
        {
            Lines = lines;
            BreakAt = breakAt;
            Bounds = bounds;
            ShownEnd = shownEnd;
        }

        public IReadOnlyList<LargueroSketchLine> Lines { get; }

        /// <summary>The middle of the gap of a broken piece, in the X of the sketch; null when the piece is whole.</summary>
        public double? BreakAt { get; }

        public bool IsBroken => BreakAt.HasValue;

        public Bounds2D Bounds { get; }

        /// <summary>How much of each end is shown, in inches of the piece; the whole length when it is not broken.</summary>
        public double ShownEnd { get; }

        /// <summary>The sketch of <paramref name="plan"/>; empty for none.</summary>
        public static LargueroPieceSketch Of(LargueroPiecePlan plan)
        {
            if (plan == null) return new LargueroPieceSketch(Array.Empty<LargueroSketchLine>(), null, new Bounds2D(0, 0, 1, 1), 0.0);

            var bounds = plan.Bounds;
            var along = plan.Kind == LargueroPieceKind.Larguero
                        || plan.SectionPlan?.View == SectionViewKind.LongitudinalX || plan.SectionPlan?.View == SectionViewKind.LongitudinalY;
            var height = Math.Max(bounds.Height, 0.001);
            var end = Math.Max(12.0, 4.0 * height);
            var segments = plan.Curves.SelectMany(Segments).ToList();
            if (!along || bounds.Width <= 2.0 * end + BreakGap)
                return new LargueroPieceSketch(segments, null, bounds, bounds.Width);

            var leftEnd = bounds.MinX + end;
            var rightStart = bounds.MaxX - end;
            var shift = rightStart - (leftEnd + BreakGap);
            var lines = new List<LargueroSketchLine>();
            foreach (var line in segments)
            {
                if (Clip(line, bounds.MinX, leftEnd, out var left)) lines.Add(left);
                if (Clip(line, rightStart, bounds.MaxX, out var right))
                    lines.Add(new LargueroSketchLine(right.Role, new Point2D(right.From.X - shift, right.From.Y), new Point2D(right.To.X - shift, right.To.Y)));
            }

            var sketched = new Bounds2D(bounds.MinX, bounds.MinY, bounds.MaxX - shift, bounds.MaxY);
            return new LargueroPieceSketch(lines, leftEnd + BreakGap / 2.0, sketched, end);
        }

        private static IEnumerable<LargueroSketchLine> Segments(LargueroPieceCurve curve)
        {
            for (var i = 0; i + 1 < curve.Points.Count; i++) yield return new LargueroSketchLine(curve.Role, curve.Points[i], curve.Points[i + 1]);
            if (curve.IsClosed && curve.Points.Count > 2)
                yield return new LargueroSketchLine(curve.Role, curve.Points[curve.Points.Count - 1], curve.Points[0]);
        }

        // The part of a line between two values of X (parametric clipping); false when nothing of it is there.
        private static bool Clip(LargueroSketchLine line, double minX, double maxX, out LargueroSketchLine clipped)
        {
            clipped = default;
            var a = line.From;
            var b = line.To;
            var dx = b.X - a.X;
            double t0 = 0.0, t1 = 1.0;
            if (Math.Abs(dx) < 1e-12)
            {
                if (a.X < minX - 1e-9 || a.X > maxX + 1e-9) return false;
            }
            else
            {
                var ta = (minX - a.X) / dx;
                var tb = (maxX - a.X) / dx;
                t0 = Math.Max(t0, Math.Min(ta, tb));
                t1 = Math.Min(t1, Math.Max(ta, tb));
                if (t0 > t1 + 1e-12) return false;
            }

            var from = new Point2D(a.X + dx * t0, a.Y + (b.Y - a.Y) * t0);
            var to = new Point2D(a.X + dx * t1, a.Y + (b.Y - a.Y) * t1);
            if (Math.Abs(from.X - to.X) < 1e-9 && Math.Abs(from.Y - to.Y) < 1e-9 && Math.Abs(dx) >= 1e-12) return false; // a point on the edge
            clipped = new LargueroSketchLine(line.Role, from, to);
            return true;
        }
    }
}
