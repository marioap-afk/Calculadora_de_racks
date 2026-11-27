using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.Systems.Larguero
{
    /// <summary>
    /// The catalog a section of a larguero comes from (I-64 A-14, line D, quinta tanda, part «lg»; LG-02 of the Owner: «Aquí más bien sería
    /// un filtro para las secciones, según el catálogo elegir la sección, una sola página»).
    /// </summary>
    public enum LargueroSectionSource
    {
        /// <summary>The largueros of RackCad: the rows of <c>secciones.csv</c> with rol LARGUERO (the beams of línea that exist today).</summary>
        RackCad = 0,

        /// <summary>The neutral catalog of structural sections (AISC v16.0): W, HSS, C, L and S.</summary>
        Aisc = 1,
    }

    /// <summary>
    /// One section a larguero can be made of, as the page «Nuevo › Larguero» offers it (LG-01 of the Owner: the page CREATES largueros from a
    /// section and a ménsula; a section may be the one of a larguero of línea that exists, but it is called by its TYPE, «Escalón», «Tubo»,
    /// «Canal…», not «Larguero escalón…»). A view of one row of one of the two catalogs: it never resolves geometry by itself.
    /// </summary>
    public sealed class LargueroSectionOption
    {
        internal LargueroSectionOption(
            LargueroSectionSource source,
            string id,
            string name,
            string typeLabel,
            string sourceLabel,
            IReadOnlyList<double> peraltes,
            double? depth,
            string defaultMensulaId,
            string gauge,
            StructuralSectionDefinition aisc)
        {
            Source = source;
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            TypeLabel = typeLabel ?? LargueroSectionNaming.Other;
            SourceLabel = sourceLabel ?? string.Empty;
            Peraltes = peraltes ?? Array.Empty<double>();
            Depth = depth;
            DefaultMensulaId = string.IsNullOrWhiteSpace(defaultMensulaId) ? null : defaultMensulaId.Trim();
            Gauge = string.IsNullOrWhiteSpace(gauge) ? null : gauge.Trim();
            Aisc = aisc;
            // What the search compares, once (the filter runs on every keystroke over about a thousand sections).
            SearchKeys = LargueroSectionNaming.SearchKey(SourceLabel) + "|" + LargueroSectionNaming.SearchKey(Id);
        }

        /// <summary>The name in the catalog and the id, as the search compares them.</summary>
        internal string SearchKeys { get; }

        public LargueroSectionSource Source { get; }

        /// <summary>The exact id of the row: the id of the larguero of RackCad, or the id of the AISC section (<c>AISC-C-C10X15_3</c>).</summary>
        public string Id { get; }

        /// <summary>The name of the section by its type (LG-01): «Escalón calibre 14», «Tubo 1.9"», or the AISC designation «C10X15.3».</summary>
        public string Name
        {
            get => _name;
            internal set
            {
                _name = value ?? string.Empty;
                NameKey = LargueroSectionNaming.SearchKey(_name);
            }
        }

        private string _name = string.Empty;

        /// <summary>The name as the search compares it.</summary>
        internal string NameKey { get; private set; } = string.Empty;

        /// <summary>The type of the section: «Escalón», «Tubo», «Canal», «Ángulo», «Viga W», «Viga S (IPS)» or «Otro».</summary>
        public string TypeLabel { get; }

        /// <summary>What the catalog calls the row («Larguero escalón calibre 14 con ménsula 3 remaches», or «C10X15.3»).</summary>
        public string SourceLabel { get; }

        /// <summary>The name of the catalog of the row: «Largueros de RackCad» or «Perfiles AISC».</summary>
        public string CatalogLabel => LargueroSectionNaming.CatalogLabel(Source);

        /// <summary>A larguero of RackCad: the values its PERALTE grip accepts (secciones.csv); empty for an AISC section.</summary>
        public IReadOnlyList<double> Peraltes { get; }

        /// <summary>Whether the peralte is chosen (a larguero of RackCad) or fixed by the section (AISC: its depth).</summary>
        public bool HasPeralteChoice => Peraltes.Count > 0;

        /// <summary>An AISC section: its depth from the catalog (d of W, C and S; the height of an HSS; the long leg of an L), in inches.</summary>
        public double? Depth { get; }

        /// <summary>A larguero of RackCad: the ménsula of its row (its FK in secciones.csv); null for an AISC section, which has none.</summary>
        public string DefaultMensulaId { get; }

        /// <summary>The gauge the row declares, when it declares one (only the first larguero of the catalog does today).</summary>
        public string Gauge { get; }

        /// <summary>The AISC section behind the option; null for a larguero of RackCad.</summary>
        public StructuralSectionDefinition Aisc { get; }

        /// <summary>The weight of an AISC section in its native unit and the equivalent («15.3 lb/ft (22.8 kg/m)»); empty for RackCad.</summary>
        public string WeightText => Aisc == null ? string.Empty : StructuralSectionLabelFormatter.FormatWeight(Aisc);

        /// <summary>The second line of the row in the list: the peraltes and the ménsula of the catalog, or the weight and the depth.</summary>
        public string Detail(Func<string, string> mensulaLabel)
        {
            if (Source == LargueroSectionSource.Aisc)
            {
                var depth = Depth.HasValue ? " · peralte " + LargueroSectionNaming.Number(Depth.Value) + " in" : string.Empty;
                return WeightText + depth;
            }

            var parts = new List<string>();
            if (Peraltes.Count == 1) parts.Add("peralte " + LargueroSectionNaming.Number(Peraltes[0]) + " in");
            else if (Peraltes.Count > 1)
                parts.Add("peraltes " + LargueroSectionNaming.Number(Peraltes.Min()) + " a " + LargueroSectionNaming.Number(Peraltes.Max()) + " in");
            if (Gauge != null) parts.Add("calibre " + Gauge);
            if (DefaultMensulaId != null) parts.Add("con " + (mensulaLabel?.Invoke(DefaultMensulaId) ?? DefaultMensulaId));
            return string.Join(" · ", parts);
        }

        public override string ToString() => Name + " (" + Id + ")";
    }

    /// <summary>
    /// The names of the sections of a larguero (LG-01): every section is called by its type. A larguero of RackCad gets its type and its name
    /// from its row: the column <c>family</c> when it says something other than «Larguero», else the words of its name («escalón», «tubo»,
    /// «canal»); its name is the name of the row without «Larguero» in front and without «con ménsula …» behind, because the ménsula is chosen
    /// apart. An AISC section is called by its designation, and its type comes from its family. Pure; one place to change it.
    /// </summary>
    public static class LargueroSectionNaming
    {
        public const string RackCadCatalog = "Largueros de RackCad";
        public const string AiscCatalog = "Perfiles AISC";

        public const string Escalon = "Escalón";
        public const string Tube = "Tubo";
        public const string Channel = "Canal";
        public const string Angle = "Ángulo";
        public const string BeamW = "Viga W";
        public const string BeamS = "Viga S (IPS)";
        public const string Other = "Otro";

        /// <summary>The types in the order the filter offers them; a type the catalog adds (its column <c>family</c>) goes after, by name.</summary>
        public static IReadOnlyList<string> TypeOrder { get; } = new[] { Escalon, Tube, Channel, Angle, BeamW, BeamS, Other };

        public static string CatalogLabel(LargueroSectionSource source) => source == LargueroSectionSource.Aisc ? AiscCatalog : RackCadCatalog;

        /// <summary>The type of an AISC family.</summary>
        public static string TypeOf(StructuralSectionFamily family)
        {
            switch (family)
            {
                case StructuralSectionFamily.W: return BeamW;
                case StructuralSectionFamily.S: return BeamS;
                case StructuralSectionFamily.Channel: return Channel;
                case StructuralSectionFamily.Angle: return Angle;
                case StructuralSectionFamily.HssRectangular: return Tube;
                default: return Other;
            }
        }

        /// <summary>The type of a larguero of RackCad: its column <c>family</c> when it is not «Larguero», else the words of its name and id.</summary>
        public static string TypeOf(BeamProfileCatalogEntry beam)
        {
            if (beam == null) return Other;
            var family = (beam.Family ?? string.Empty).Trim();
            if (family.Length > 0 && Fold(family) != "LARGUERO")
            {
                var known = TypeOrder.FirstOrDefault(t => Fold(t) == Fold(family));
                return known ?? Capitalize(family);
            }

            var words = Fold((beam.Label ?? string.Empty) + " " + (beam.Id ?? string.Empty));
            if (words.Contains("ESCALON", StringComparison.Ordinal)) return Escalon;
            if (words.Contains("TUBO", StringComparison.Ordinal)) return Tube;
            if (words.Contains("CANAL", StringComparison.Ordinal)) return Channel;
            return Other;
        }

        /// <summary>
        /// The name of a larguero of RackCad as a section of its type: «Larguero escalón calibre 14 con ménsula 3 remaches» is «Escalón calibre 14»,
        /// «Larguero escalon troquel redondo» is «Escalón troquel redondo» and «Larguero tubo 1.9" con ménsula 3 remaches» is «Tubo 1.9"».
        /// </summary>
        public static string NameOf(BeamProfileCatalogEntry beam, string type)
        {
            if (beam == null) return string.Empty;
            var name = (beam.Label ?? string.Empty).Trim();
            if (Fold(name).StartsWith("LARGUERO ", StringComparison.Ordinal)) name = name.Substring("LARGUERO ".Length).TrimStart();

            var mensula = Fold(name).LastIndexOf(" CON MENSULA", StringComparison.Ordinal);
            if (mensula > 0) name = name.Substring(0, mensula).TrimEnd();
            if (name.Length == 0) return beam.Id ?? string.Empty;

            if (!string.IsNullOrEmpty(type) && type != Other && name.Length >= type.Length
                && Fold(name.Substring(0, type.Length)) == Fold(type)
                && (name.Length == type.Length || !char.IsLetter(name[type.Length])))
                return type + name.Substring(type.Length);

            return Capitalize(name);
        }

        /// <summary>A number of the page: invariant, up to three decimals («3», «3.5», «1.9»).</summary>
        public static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        /// <summary>Upper case without accents, one character per character (so an index of the folded text is an index of the text).</summary>
        internal static string Fold(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var builder = new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                var decomposed = ch.ToString().Normalize(NormalizationForm.FormD);
                builder.Append(char.ToUpperInvariant(decomposed.Length > 0 ? decomposed[0] : ch));
            }

            return builder.ToString();
        }

        /// <summary>What a search compares: folded, without the separators a user does not type (spaces, «-», «_», «.»).</summary>
        internal static string SearchKey(string text)
        {
            var folded = Fold(text);
            var builder = new StringBuilder(folded.Length);
            foreach (var ch in folded)
                if (ch != ' ' && ch != '-' && ch != '_' && ch != '.')
                    builder.Append(ch);
            return builder.ToString();
        }

        private static string Capitalize(string text) =>
            string.IsNullOrEmpty(text) ? string.Empty : char.ToUpper(text[0], CultureInfo.InvariantCulture) + text.Substring(1);
    }

    /// <summary>
    /// The natural order of the sections (LG-12 of the prototype, accepted): the numbers of a name compare as numbers, fractions included, so
    /// W10X12 goes before W10X100, and L10X10X1-1/8 before L10X10X1-1/4. Pure.
    /// </summary>
    public sealed class LargueroNaturalOrder : IComparer<string>
    {
        public static readonly LargueroNaturalOrder Instance = new LargueroNaturalOrder();

        public int Compare(string x, string y)
        {
            var a = Tokens(x);
            var b = Tokens(y);
            for (var i = 0; i < Math.Min(a.Count, b.Count); i++)
            {
                var left = a[i];
                var right = b[i];
                int order;
                if (left.IsNumber && right.IsNumber) order = left.Number.CompareTo(right.Number);
                else if (left.IsNumber != right.IsNumber) order = left.IsNumber ? -1 : 1;
                else order = string.CompareOrdinal(left.Text, right.Text);
                if (order != 0) return order;
            }

            var byCount = a.Count.CompareTo(b.Count);
            return byCount != 0 ? byCount : string.CompareOrdinal(x ?? string.Empty, y ?? string.Empty);
        }

        private readonly struct Token
        {
            public Token(double number)
            {
                IsNumber = true;
                Number = number;
                Text = null;
            }

            public Token(string text)
            {
                IsNumber = false;
                Number = 0.0;
                Text = text;
            }

            public bool IsNumber { get; }

            public double Number { get; }

            public string Text { get; }
        }

        private static List<Token> Tokens(string value)
        {
            var text = LargueroSectionNaming.Fold(value);
            var tokens = new List<Token>();
            var i = 0;
            while (i < text.Length)
            {
                if (char.IsDigit(text[i]))
                {
                    var whole = ReadDecimal(text, ref i);
                    // «1-1/4» (a whole and a fraction) and «1/4» (a fraction): the designations of HSS and L.
                    if (i + 1 < text.Length && text[i] == '-' && char.IsDigit(text[i + 1]) && HasFractionAhead(text, i + 1))
                    {
                        i++;
                        whole += ReadFraction(text, ref i);
                    }
                    else if (i + 1 < text.Length && text[i] == '/' && char.IsDigit(text[i + 1]))
                    {
                        i++;
                        var denominator = ReadDecimal(text, ref i);
                        whole = denominator > 0.0 ? whole / denominator : whole;
                    }

                    tokens.Add(new Token(whole));
                }
                else
                {
                    var start = i;
                    while (i < text.Length && !char.IsDigit(text[i])) i++;
                    tokens.Add(new Token(text.Substring(start, i - start)));
                }
            }

            return tokens;
        }

        private static bool HasFractionAhead(string text, int at)
        {
            var i = at;
            while (i < text.Length && char.IsDigit(text[i])) i++;
            return i + 1 < text.Length && text[i] == '/' && char.IsDigit(text[i + 1]);
        }

        private static double ReadFraction(string text, ref int i)
        {
            var numerator = ReadDecimal(text, ref i);
            i++; // the «/»
            var denominator = ReadDecimal(text, ref i);
            return denominator > 0.0 ? numerator / denominator : 0.0;
        }

        private static double ReadDecimal(string text, ref int i)
        {
            var start = i;
            while (i < text.Length && char.IsDigit(text[i])) i++;
            if (i + 1 < text.Length && text[i] == '.' && char.IsDigit(text[i + 1]))
            {
                i++;
                while (i < text.Length && char.IsDigit(text[i])) i++;
            }

            return double.Parse(text.Substring(start, i - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }
    }

    /// <summary>One type of the filter «Tipo»: its name and how many sections of the catalog it has.</summary>
    public sealed record LargueroSectionType(string Label, int Count);

    /// <summary>
    /// The sections a larguero can be made of, from the two catalogs (LG-02): the largueros of RackCad (secciones.csv, rol LARGUERO) and the
    /// enabled AISC sections. Ordered by type («Escalón», «Tubo», «Canal», «Ángulo», «Viga W», «Viga S (IPS)»), then the catalog (RackCad
    /// first), then the natural order of the name. Filtered by catalog, by type and by text. Pure: the caller loads the catalogs.
    /// </summary>
    public sealed class LargueroSectionCatalog
    {
        private LargueroSectionCatalog(IReadOnlyList<LargueroSectionOption> all) => All = all;

        public static LargueroSectionCatalog Empty { get; } = new LargueroSectionCatalog(Array.Empty<LargueroSectionOption>());

        /// <summary>Every section, in the order of the list.</summary>
        public IReadOnlyList<LargueroSectionOption> All { get; }

        /// <summary>
        /// The sections of <paramref name="rack"/> (its largueros) and of <paramref name="aisc"/> (its enabled sections). Either may be null: a
        /// catalog that could not be read gives no sections, and the caller says why. Two largueros of RackCad whose names by type are the same
        /// (the same section with another ménsula) carry their id, so no two rows of the list read the same.
        /// </summary>
        public static LargueroSectionCatalog Build(RackCatalog rack, StructuralSectionCatalog aisc)
        {
            var options = new List<LargueroSectionOption>();
            foreach (var beam in rack?.BeamProfiles ?? Enumerable.Empty<BeamProfileCatalogEntry>())
            {
                if (beam == null || string.IsNullOrWhiteSpace(beam.Id)) continue;
                var type = LargueroSectionNaming.TypeOf(beam);
                options.Add(new LargueroSectionOption(LargueroSectionSource.RackCad, beam.Id.Trim(), LargueroSectionNaming.NameOf(beam, type), type,
                    beam.Label, PeralteList.Parse(beam.Peraltes), null, beam.Mensula, beam.Gauge, null));
            }

            foreach (var group in options.GroupBy(o => LargueroSectionNaming.Fold(o.Name)).Where(g => g.Count() > 1))
                foreach (var option in group)
                    option.Name = option.Name + " (" + option.Id + ")";

            if (aisc != null)
            {
                foreach (var section in aisc.All.Where(s => s != null && s.IsEnabled && s.Identity != null))
                {
                    options.Add(new LargueroSectionOption(LargueroSectionSource.Aisc, section.SectionId.Value, section.DisplayName,
                        LargueroSectionNaming.TypeOf(section.Family), section.DisplayName, Array.Empty<double>(), DepthOf(section), null, null, section));
                }
            }

            return new LargueroSectionCatalog(options
                .OrderBy(o => TypeRank(o.TypeLabel))
                .ThenBy(o => o.TypeLabel, StringComparer.Ordinal)
                .ThenBy(o => o.Source)
                .ThenBy(o => o.Name, LargueroNaturalOrder.Instance)
                .ToList());
        }

        /// <summary>How many sections a catalog has (null: both).</summary>
        public int CountOf(LargueroSectionSource? source) => source == null ? All.Count : All.Count(o => o.Source == source.Value);

        /// <summary>The types of a catalog (null: both), in the order of the list, each with its count.</summary>
        public IReadOnlyList<LargueroSectionType> TypesOf(LargueroSectionSource? source) =>
            All.Where(o => source == null || o.Source == source.Value)
                .GroupBy(o => o.TypeLabel)
                .Select(g => new LargueroSectionType(g.Key, g.Count()))
                .OrderBy(t => TypeRank(t.Label))
                .ThenBy(t => t.Label, StringComparer.Ordinal)
                .ToList();

        /// <summary>
        /// The sections of a catalog (null: both) and of a type (null or empty: every type) whose name, name in the catalog or id contain
        /// <paramref name="text"/>, without case, accents or the separators a user does not type. Blank text is no filter.
        /// </summary>
        public IReadOnlyList<LargueroSectionOption> Filter(LargueroSectionSource? source, string type, string text)
        {
            var needle = LargueroSectionNaming.SearchKey(text);
            return All.Where(o => source == null || o.Source == source.Value)
                .Where(o => string.IsNullOrEmpty(type) || o.TypeLabel == type)
                .Where(o => needle.Length == 0
                            || o.NameKey.Contains(needle, StringComparison.Ordinal)
                            || o.SearchKeys.Contains(needle, StringComparison.Ordinal))
                .ToList();
        }

        /// <summary>The section of a catalog with that id (exact, without case), or null.</summary>
        public LargueroSectionOption Find(LargueroSectionSource source, string id) =>
            All.FirstOrDefault(o => o.Source == source && string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase));

        /// <summary>The section a new larguero starts with: the first larguero of RackCad of the list (the default of the classic editor), or the first section.</summary>
        public LargueroSectionOption Default => All.FirstOrDefault(o => o.Source == LargueroSectionSource.RackCad) ?? All.FirstOrDefault();

        private static int TypeRank(string type)
        {
            for (var i = 0; i < LargueroSectionNaming.TypeOrder.Count; i++)
                if (LargueroSectionNaming.TypeOrder[i] == type) return type == LargueroSectionNaming.Other ? int.MaxValue : i;
            return LargueroSectionNaming.TypeOrder.Count; // a type of the catalog: after the known ones, before «Otro»
        }

        // The peralte an AISC section gives a larguero: its depth (W, C and S), the height of an HSS, or the long leg of an L.
        private static double? DepthOf(StructuralSectionDefinition section)
        {
            switch (section.Dimensions)
            {
                case WSectionDimensions w: return w.Depth;
                case SSectionDimensions s: return s.Depth;
                case ChannelSectionDimensions c: return c.Depth;
                case HssRectangularSectionDimensions hss: return hss.OverallDepth;
                case AngleSectionDimensions angle: return angle.LongLeg;
                default: return null;
            }
        }
    }
}
