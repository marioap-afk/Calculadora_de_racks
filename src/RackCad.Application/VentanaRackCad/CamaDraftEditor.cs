using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.FlowBed;
using RackCad.Domain.Systems.FlowBed;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>One roller of the catalog as the editor of the cama offers it: its id, its label, its diameter and its capacity.</summary>
    public sealed record CamaRollerOption(string Id, string Label, double Diameter, double CapacityKg, string Gauge)
    {
        /// <summary>The pitch of this roller when the bed has none of its own: its diameter rounded up to the troquel of 1 in.</summary>
        public double AutomaticPitch => CamaDraftRules.PitchOf(Diameter);
    }

    /// <summary>
    /// Where the builder of the cama places its pieces, measured from the troquel of the tope (TROQUEL_TOPE of the rail): the rail, the
    /// tope, the rollers and the brakes, with the pitch used and whether it is the one written by hand. It is read from the instances of
    /// <see cref="FlowBedLateralBuilder"/>, the same ones the drawing and the BOM use, so the scheme never tells another story.
    /// </summary>
    public sealed record CamaLayout(double RailLength, double TopeOrigin, IReadOnlyList<double> Rollers, IReadOnlyList<double> Brakes, double Pitch, bool ManualPitch)
    {
        /// <summary>The length the tope occupies from its troquel (6.5 in).</summary>
        public double TopeLength => FlowBedDefaults.TopeOccupiedLength;
    }

    /// <summary>
    /// The pure rules of the editor of the cama of the RackCad window (I-64, quinta tanda, part cm; prototype «camas», every question
    /// accepted). The geometry is the one of the domain and is NOT changed here: the rollers and the brakes are the ones
    /// <see cref="FlowBedLateralBuilder"/> places (the fixed pitch and, on a dynamic bed, a brake by the pallet depth or every N rollers).
    /// What is new is how a value is accepted: question 4 (the pitch is an integer of inches and not smaller than the roller, as the bed of
    /// Dinámico), question 8 (a Pushback that becomes Dinámica takes 48 in of pallet when it had none) and question 6 («Restaurar layout…»
    /// takes away what was written by hand). Every text is here, in one place.
    /// </summary>
    public static class CamaDraftRules
    {
        /// <summary>The defaults of the classic editor (RackFlowBedWindow): 96 in of bed and 48 in of pallet.</summary>
        public const double DefaultLaneDepth = 96.0;

        public const double DefaultPalletDepth = 48.0;

        public const string TypeKey = "type";
        public const string RollerKey = "roller";
        public const string LaneKey = "lane";
        public const string PalletKey = "pallet";
        public const string PitchKey = "pitch";
        public const string BrakesKey = "brakes";

        public const string LaneDepthText = "Debe ser un número mayor que cero.";
        public const string PalletDepthText = "Debe ser un número mayor que cero: la cama Dinámica pone los frenos según la tarima.";
        public const string PitchPositiveText = "El paso debe ser un número mayor que cero.";
        public const string PitchIntegerText = "Los rodillos se cuelgan en los troqueles: el paso debe ser un número entero de pulgadas.";
        public const string BrakesText = "Debe ser un entero mayor o igual que 1.";

        /// <summary>Question 8 of the prototype: what the editor says when a Pushback with no pallet becomes Dinámica.</summary>
        public static readonly string PalletFilledText = "Pasó a Dinámica: la profundidad de tarima estaba vacía y toma "
            + Num(DefaultPalletDepth) + " in, el valor por defecto del editor clásico.";

        /// <summary>The smallest pitch the minimum (and automatic) pitch of a roller allows: its diameter rounded up to the troquel.</summary>
        public static double PitchOf(double diameter)
        {
            var grid = FlowBedDefaults.TroquelPitch > 0.0 ? FlowBedDefaults.TroquelPitch : 1.0;
            return Math.Max(grid, Math.Ceiling(diameter / grid) * grid); // the rule of FlowBedLateralBuilder.RollerPitch
        }

        /// <summary>The rollers of the catalog (role RODILLO), one per id, ordered by their label as the list of the classic editor.</summary>
        public static IReadOnlyList<CamaRollerOption> Rollers(RackCatalog catalog)
            => (catalog?.FlowBedProfiles ?? Enumerable.Empty<FlowBedComponentCatalogEntry>())
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Id)
                                && string.Equals(entry.Role, FlowBedDefaults.RollerRole, StringComparison.OrdinalIgnoreCase))
                .GroupBy(entry => entry.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(entry => entry.Label, StringComparer.CurrentCultureIgnoreCase)
                .Select(entry => new CamaRollerOption(entry.Id.Trim(), string.IsNullOrWhiteSpace(entry.Label) ? entry.Id.Trim() : entry.Label,
                    entry.Diameter, entry.CapacityKg, entry.Gauge))
                .ToList();

        /// <summary>A new bed with the defaults of the classic editor: Dinámica, the default roller (or the first one), 96 in, 48 in, automatic pitch.</summary>
        public static FlowBedConfiguration Defaults(RackCatalog catalog)
        {
            var rollers = Rollers(catalog);
            var roller = rollers.FirstOrDefault(r => string.Equals(r.Id, FlowBedDefaults.RollerId, StringComparison.OrdinalIgnoreCase)) ?? rollers.FirstOrDefault();
            return new FlowBedConfiguration
            {
                BedType = FlowBedType.Dynamic,
                RollerId = roller?.Id ?? FlowBedDefaults.RollerId,
                LaneDepth = DefaultLaneDepth,
                PalletDepth = DefaultPalletDepth,
            };
        }

        /// <summary>A copy of a bed: nothing the copy does reaches the original.</summary>
        public static FlowBedConfiguration Copy(FlowBedConfiguration bed)
            => bed == null
                ? null
                : new FlowBedConfiguration
                {
                    BedType = bed.BedType,
                    LaneDepth = bed.LaneDepth,
                    PalletDepth = bed.PalletDepth,
                    RollerId = bed.RollerId,
                    RollerPitchOverride = bed.RollerPitchOverride,
                    BrakeEveryRollers = bed.BrakeEveryRollers,
                };

        public static string LaneDepthError(double value) => value > 0.0 ? null : LaneDepthText;

        public static string PalletDepthError(double value) => value > 0.0 ? null : PalletDepthText;

        /// <summary>Question 4: empty is the pitch of the roller; otherwise an integer of inches, not smaller than the roller.</summary>
        public static string PitchError(double? pitch, double minimum)
        {
            if (!pitch.HasValue) return null;
            var value = pitch.Value;
            if (!(value > 0.0)) return PitchPositiveText;
            if (Math.Abs(value - Math.Round(value)) > 1e-9) return PitchIntegerText;
            if (value < minimum - 1e-9) return "El paso no puede ser menor que el rodillo: mínimo " + Num(minimum) + " in.";
            return null;
        }

        public static string BrakesError(int? every) => !every.HasValue || every.Value >= 1 ? null : BrakesText;

        /// <summary>What a roller refuses when the pitch written by hand is smaller than it (the rule of the bed of Dinámico).</summary>
        public static string RollerRefusal(double pitch) => "El paso (" + Num(pitch) + " in) es menor que este rodillo: cambia antes el paso. El rodillo no cambió.";

        /// <summary>The minimum (and automatic) pitch of the roller of a bed.</summary>
        public static double MinimumPitch(FlowBedConfiguration bed, RackCatalog catalog) => PitchOf(RollerOf(bed?.RollerId, catalog)?.Diameter ?? 0.0);

        /// <summary>The roller of an id in the catalog, or null.</summary>
        public static CamaRollerOption RollerOf(string rollerId, RackCatalog catalog)
        {
            var id = string.IsNullOrWhiteSpace(rollerId) ? FlowBedDefaults.RollerId : rollerId.Trim();
            return Rollers(catalog).FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// What is wrong with a bed as it is (a bed read from the drawing or from the library can carry what the window does not accept,
        /// question 4: «Una cama del dibujo con un paso así se abre marcada, y «Actualizar» pide corregirlo»), by the key of its field.
        /// Empty when it can be written. The pallet and the brakes only count on a dynamic bed: a Pushback has no brakes.
        /// </summary>
        public static IReadOnlyDictionary<string, string> Problems(FlowBedConfiguration bed, RackCatalog catalog)
        {
            var problems = new Dictionary<string, string>(StringComparer.Ordinal);
            if (bed == null)
            {
                problems[LaneKey] = LaneDepthText;
                return problems;
            }

            var rollers = Rollers(catalog);
            if (rollers.Count > 0 && RollerOf(bed.RollerId, catalog) == null)
                problems[RollerKey] = "El rodillo «" + (bed.RollerId ?? string.Empty) + "» no está en el catálogo: elige uno de la lista.";
            if (LaneDepthError(bed.LaneDepth) is string lane) problems[LaneKey] = lane;
            if (bed.BedType == FlowBedType.Dynamic && PalletDepthError(bed.PalletDepth) is string pallet) problems[PalletKey] = pallet;
            if (PitchError(bed.RollerPitchOverride, MinimumPitch(bed, catalog)) is string pitch) problems[PitchKey] = pitch;
            if (bed.BedType == FlowBedType.Dynamic && BrakesError(bed.BrakeEveryRollers) is string brakes) problems[BrakesKey] = brakes;
            return problems;
        }

        /// <summary>Where the builder places the pieces of a bed, from the troquel of the tope; null when the bed cannot be built.</summary>
        public static CamaLayout Layout(FlowBedConfiguration bed, RackCatalog catalog)
        {
            if (bed == null || !(bed.LaneDepth > 0.0)) return null;
            var instances = new FlowBedLateralBuilder().Build(bed, catalog);
            var origin = CatalogLookup.Local(catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailTopePoint, FlowBedDefaults.View).X;
            var rollers = instances.Where(i => i.Role == HeaderBlockRole.Roller).Select(i => Round(i.Insertion.X - origin)).OrderBy(x => x).ToList();
            var brakes = instances.Where(i => i.Role == HeaderBlockRole.Brake).Select(i => Round(i.Insertion.X - origin)).OrderBy(x => x).ToList();
            var manual = bed.RollerPitchOverride.HasValue && bed.RollerPitchOverride.Value > 0.0;
            var grid = FlowBedDefaults.TroquelPitch > 0.0 ? FlowBedDefaults.TroquelPitch : 1.0;
            var pitch = manual ? Math.Max(grid, bed.RollerPitchOverride.Value) : MinimumPitch(bed, catalog);
            return new CamaLayout(bed.LaneDepth, origin, rollers, brakes, pitch, manual);
        }

        /// <summary>
        /// The list of positions under the scheme (question 2 of the prototype): the tope, the runs of rollers between brakes and each brake,
        /// measured from the troquel of the tope. More than nine lines are cut to eight and «… y N líneas más.».
        /// </summary>
        public static IReadOnlyList<string> PositionLines(CamaLayout layout)
        {
            if (layout == null) return Array.Empty<string>();
            var lines = new List<string> { "Tope: de 0 a " + Num(layout.TopeLength) + " in." };
            if (layout.Rollers.Count == 0)
            {
                var first = Math.Ceiling(layout.TopeLength);
                lines.Add("Sin rodillos: el riel no llega al primer troquel libre después del tope (" + Num(first + layout.TopeOrigin) + " in).");
                return lines;
            }

            var number = 1;
            var brakeNumber = 0;
            var run = new List<double> { layout.Rollers[0] };
            for (var i = 1; i <= layout.Rollers.Count; i++)
            {
                double? brake = null;
                if (i < layout.Rollers.Count)
                {
                    var from = layout.Rollers[i - 1];
                    var to = layout.Rollers[i];
                    var between = layout.Brakes.Where(b => b > from && b < to).ToList();
                    if (between.Count == 0)
                    {
                        run.Add(to);
                        continue;
                    }

                    brake = between[0];
                }

                lines.Add(RunLine(run, number, layout.Pitch));
                number += run.Count;
                if (brake.HasValue)
                {
                    brakeNumber++;
                    lines.Add("Freno " + brakeNumber.ToString(CultureInfo.InvariantCulture) + ": a " + Num(brake.Value) + " in.");
                    run = new List<double> { layout.Rollers[i] };
                }
            }

            if (lines.Count > 9)
            {
                var more = lines.Count - 8;
                lines = lines.Take(8).ToList();
                lines.Add("… y " + more.ToString(CultureInfo.InvariantCulture) + (more == 1 ? " línea más." : " líneas más."));
            }

            return lines;
        }

        /// <summary>The BOM of a bed (riel, tope, rodillos y frenos), the one of its handler (CamaKindHandler) on a copy; null when it cannot be built.</summary>
        public static BillOfMaterials Bom(FlowBedConfiguration bed, RackCatalog catalog)
            => bed == null || !(bed.LaneDepth > 0.0) ? null : FlowBedBomBuilder.Build(new FlowBedLateralBuilder().Build(Copy(bed), catalog), catalog);

        /// <summary>«1 rodillo», «42 rodillos».</summary>
        public static string Count(int count, string one, string many) => count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);

        /// <summary>A number as the editor shows it (up to two decimals, a point).</summary>
        public static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

        private static string RunLine(IReadOnlyList<double> run, int first, double pitch)
        {
            if (run.Count == 1) return "Rodillo " + first.ToString(CultureInfo.InvariantCulture) + ": a " + Num(run[0]) + " in.";
            var last = first + run.Count - 1;
            return "Rodillos " + first.ToString(CultureInfo.InvariantCulture) + " a " + last.ToString(CultureInfo.InvariantCulture) + ": de "
                   + Num(run[0]) + " a " + Num(run[run.Count - 1]) + " in, cada " + Num(pitch) + " in.";
        }

        private static double Round(double value) => Math.Round(value, 6);
    }

    /// <summary>
    /// The draft of ONE cama de rodamiento in the RackCad window (I-64, quinta tanda, part cm): a copy of the bed it was opened with and its
    /// name. Every gesture checks its value first and, when it refuses it, says why and changes nothing: the draft is always a bed the
    /// window accepts, except for what it was opened with (<see cref="Problems"/>, question 4). It never writes the drawing.
    /// </summary>
    public sealed class CamaDraftEditor
    {
        private static readonly FlowBedConfigurationStore Store = new();

        private FlowBedConfiguration _bed;

        /// <param name="catalog">The catalog of the plugin (rollers, the troquel of the tope, the blocks).</param>
        /// <param name="bed">The bed it opens; it keeps a copy.</param>
        /// <param name="name">The name of the rack (the one of the envelope for a cama of the drawing).</param>
        /// <param name="source">The document of the bed as the drawing or the library had it, so a save to the library keeps its unknown fields (I-11).</param>
        public CamaDraftEditor(RackCatalog catalog, FlowBedConfiguration bed, string name, FlowBedDocument source = null)
        {
            Catalog = catalog ?? new RackCatalog();
            _bed = CamaDraftRules.Copy(bed) ?? CamaDraftRules.Defaults(Catalog);
            Name = name ?? string.Empty;
            Source = source;
        }

        /// <summary>A new bed with the defaults of the classic editor (<see cref="CamaDraftRules.Defaults"/>), with no name.</summary>
        public static CamaDraftEditor CreateNew(RackCatalog catalog) => new CamaDraftEditor(catalog, CamaDraftRules.Defaults(catalog), string.Empty);

        public RackCatalog Catalog { get; }

        public string Name { get; set; }

        /// <summary>The document it was opened from (the drawing or the library), or null.</summary>
        public FlowBedDocument Source { get; }

        public FlowBedType BedType => _bed.BedType;

        public bool IsDynamic => _bed.BedType == FlowBedType.Dynamic;

        public string RollerId => _bed.RollerId;

        public double LaneDepth => _bed.LaneDepth;

        public double PalletDepth => _bed.PalletDepth;

        public double? RollerPitch => _bed.RollerPitchOverride;

        public int? BrakeEveryRollers => _bed.BrakeEveryRollers;

        public IReadOnlyList<CamaRollerOption> RollerOptions => CamaDraftRules.Rollers(Catalog);

        /// <summary>The roller of the bed in the catalog, or null when the catalog does not have it.</summary>
        public CamaRollerOption Roller => CamaDraftRules.RollerOf(_bed.RollerId, Catalog);

        public double MinimumPitch => CamaDraftRules.MinimumPitch(_bed, Catalog);

        /// <summary>The bed as its document, the base of «Modificado» (the name is compared apart).</summary>
        public string Json => Store.Serialize(_bed);

        /// <summary>A copy of the bed, for the write and the BOM: nothing done afterwards reaches it.</summary>
        public FlowBedConfiguration Snapshot() => CamaDraftRules.Copy(_bed);

        /// <summary>«Descartar»: back to a bed (a copy of it) and its name.</summary>
        public void LoadFrom(FlowBedConfiguration bed, string name)
        {
            _bed = CamaDraftRules.Copy(bed) ?? CamaDraftRules.Defaults(Catalog);
            Name = name ?? string.Empty;
        }

        /// <summary>What is wrong with the bed now, by the key of its field; empty when it can be written.</summary>
        public IReadOnlyDictionary<string, string> Problems() => CamaDraftRules.Problems(_bed, Catalog);

        /// <summary>Where the builder places its pieces; null while the bed has a problem.</summary>
        public CamaLayout Layout() => Problems().Count > 0 ? null : CamaDraftRules.Layout(_bed, Catalog);

        /// <summary>«Tipo de cama». A Pushback keeps its pallet and its brakes (they do not apply); a Pushback with no pallet that becomes
        /// Dinámica takes 48 in and says so in <paramref name="note"/> (question 8).</summary>
        public void SetBedType(FlowBedType type, out string note)
        {
            note = null;
            if (_bed.BedType == type) return;
            _bed.BedType = type;
            if (type == FlowBedType.Dynamic && !(_bed.PalletDepth > 0.0))
            {
                _bed.PalletDepth = CamaDraftRules.DefaultPalletDepth;
                note = CamaDraftRules.PalletFilledText;
            }
        }

        /// <summary>«Rodillo»: a roller of the catalog; refused when the pitch written by hand is smaller than it.</summary>
        public string SetRoller(string rollerId)
        {
            var roller = CamaDraftRules.RollerOf(rollerId, Catalog);
            if (roller == null) return "Ese rodillo no está en el catálogo.";
            if (_bed.RollerPitchOverride.HasValue && _bed.RollerPitchOverride.Value < roller.AutomaticPitch - 1e-9)
                return CamaDraftRules.RollerRefusal(_bed.RollerPitchOverride.Value);
            _bed.RollerId = roller.Id;
            return null;
        }

        public string SetLaneDepth(double value)
        {
            var error = CamaDraftRules.LaneDepthError(value);
            if (error == null) _bed.LaneDepth = value;
            return error;
        }

        public string SetPalletDepth(double value)
        {
            var error = CamaDraftRules.PalletDepthError(value);
            if (error == null) _bed.PalletDepth = value;
            return error;
        }

        /// <summary>«Paso de rodillos»: null is the pitch of the roller (question 4 for a value).</summary>
        public string SetPitch(double? pitch)
        {
            var error = CamaDraftRules.PitchError(pitch, MinimumPitch);
            if (error == null) _bed.RollerPitchOverride = pitch;
            return error;
        }

        /// <summary>«Freno cada N rodillos» (question 3): null is a brake by the pallet depth, as always.</summary>
        public string SetBrakeEvery(int? every)
        {
            var error = CamaDraftRules.BrakesError(every);
            if (error == null) _bed.BrakeEveryRollers = every;
            return error;
        }

        /// <summary>«Restaurar layout…» (question 6): what was written by hand goes, as marked; the names of what went, empty when nothing.</summary>
        public IReadOnlyList<string> Restore(bool pitch, bool brakes)
        {
            var gone = new List<string>();
            if (pitch && _bed.RollerPitchOverride.HasValue)
            {
                _bed.RollerPitchOverride = null;
                gone.Add("el paso manual");
            }

            if (brakes && _bed.BrakeEveryRollers.HasValue)
            {
                _bed.BrakeEveryRollers = null;
                gone.Add("el freno cada N rodillos");
            }

            return gone;
        }

        /// <summary>The BOM of the draft (a copy); null while the bed has a problem.</summary>
        public BillOfMaterials Bom() => Problems().Count > 0 ? null : CamaDraftRules.Bom(_bed, Catalog);
    }
}
