using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The Push Back component BOM. It reuses <see cref="SystemBomBuilder.Build"/> as a BLACK BOX for the shared structural
    /// components (cabeceras, separators, derived/reinforced posts, plates, intermediate beams, and the GUIA-free safety),
    /// then SUBSTITUTES the pallet-flow-specific categories: it drops the dynamic IN/OUT-beam category (two beams per level)
    /// and the dynamic bed (length − 4"), and adds — per front and level — ONE low IN/OUT beam and ONE high
    /// <c>LARGUERO_ESCALON_TROQUEL_REDONDO</c>, one OPAQUE bed per lane and level at the FULL structural span, and one rear
    /// tope per ACTIVE cell. No second IN/OUT, no −4" bed, no brakes, no guides, and no double counting between views.
    /// </summary>
    public static class PushBackBomBuilder
    {
        public const string HighEndBeam = "Larguero troquel redondo";
        public const string RearTope = "Tope posterior";

        public static BillOfMaterials Build(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system?.Structure;
            if (structure == null)
            {
                return new BillOfMaterials(new List<BomComponent>());
            }

            // Shared structure from the dynamic BOM, minus the dynamic-flow-specific categories we replace.
            var components = SystemBomBuilder.Build(structure, catalog).Components
                .Where(component => component != null
                    && component.Category != SystemBomBuilder.InOutBeam
                    && component.Category != SystemBomBuilder.Cama)
                .Select(Clone)
                .ToList();

            // I-42: un rack COMPUESTO cuenta EJECUCIONES fisicas de cama, no celdas de una rejilla. Dos camas
            // encontradas son dos; una cama corrida es UNA, aunque atraviese los dos lados. La estructura ya viene
            // del BOM compartido de arriba —cabeceras, separadores (el central incluido, una sola vez), postes
            // derivados, placas y seguridad—, asi que no hay nada que deduplicar despues: el plan ya es correcto.
            if (system.IsComposite)
            {
                var runs = PushBackRuns.Resolve(system);
                // Los INTERMEDIOS tambien pertenecen a una cama: se retiran del BOM compartido y se vuelven a contar
                // por cama, con el MISMO builder que los dibuja. Contarlos sobre la estructura compuesta daria una
                // cantidad que no corresponde a ninguna pieza del plano.
                components.RemoveAll(component => component.Category == SystemBomBuilder.IntermediateBeam);
                AddRunEndBeams(components, system, catalog, runs, SystemBomBuilder.InOutBeam, isHighEnd: false);
                AddRunEndBeams(components, system, catalog, runs, HighEndBeam, isHighEnd: true);
                AddRunBeds(components, runs);
                AddRunRearTopes(components, system, catalog, runs);
                AddRunIntermediates(components, catalog, runs);
                return new BillOfMaterials(components);
            }

            // I-42 (ronda 6A) — los INTERMEDIOS se cuentan con el MISMO builder que los dibuja, tambien en un
            // rack de un solo sentido. El BOM compartido los contaba sobre la ESTRUCTURA —fronteras x niveles— y no
            // aplicaba el fondo EFECTIVO por celda que I-41 introdujo: una escalera de 3 a 8 fondos facturaba 42
            // piezas para un plano de 27. Un rack sin fondos por celda cuenta exactamente lo mismo que antes,
            // porque entonces cada nivel recorre TODAS las fronteras de su frente.
            components.RemoveAll(component => component.Category == SystemBomBuilder.IntermediateBeam);
            AddEndBeams(components, system, catalog, SystemBomBuilder.InOutBeam, isHighEnd: false);
            AddEndBeams(components, system, catalog, HighEndBeam, isHighEnd: true);
            AddBeds(components, system);
            AddRearTopes(components, system, catalog);
            AddFrontIntermediates(components, system, catalog);

            return new BillOfMaterials(components);
        }

        /// <summary>
        /// I-42 — un larguero bajo y uno alto POR CAMA FISICA. Los valores se leen en el marco de la cama (el del
        /// lado o el sintetico de la corrida), que es donde el resolver ya los dejo resueltos.
        /// </summary>
        private static void AddRunEndBeams(
            ICollection<BomComponent> components, PushBackSystem system, RackCatalog catalog,
            PushBackRunSet runs, string category, bool isHighEnd)
        {
            var highId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var grouped = new Dictionary<(string BeamId, double Length, double Peralte), int>();

            foreach (var run in runs.Runs)
            {
                var front = run.Front();
                var source = run.Source?.Structure;
                if (front == null || source == null)
                {
                    continue;
                }

                var length = PushBackLoadBeamGeometry.CellBeamLength(source, front, run.SourceLevel);
                string beamId;
                double peralte;
                if (isHighEnd)
                {
                    beamId = highId;
                    peralte = run.Source.HighEndBeamPeralteAt(run.SourceFrontIndex, run.SourceLevel - 1);
                }
                else
                {
                    var configuration = DynamicRackLevelGeometry.At(source, front, run.SourceLevel);
                    beamId = string.IsNullOrWhiteSpace(configuration.InOutBeamCatalogId)
                        ? (string.IsNullOrWhiteSpace(source.InOutBeamCatalogId)
                            ? DynamicRackDefaults.InOutBeamCatalogId
                            : source.InOutBeamCatalogId)
                        : configuration.InOutBeamCatalogId;
                    peralte = configuration.InOutBeamDepth > 0.0 ? configuration.InOutBeamDepth : source.InOutBeamDepth;
                }

                var key = (beamId, Round(length), Round(peralte));
                grouped[key] = grouped.TryGetValue(key, out var current) ? current + 1 : 1;
            }

            EmitBeams(components, catalog, grouped, category);
        }

        /// <summary>
        /// I-42 — UNA cama por ejecucion fisica y calle. La corrida aporta una sola, de la longitud del rack
        /// entero; las encontradas aportan dos, cada una con su propia longitud.
        /// </summary>
        private static void AddRunBeds(ICollection<BomComponent> components, PushBackRunSet runs)
        {
            var grouped = new Dictionary<double, int>();
            foreach (var run in runs.Runs)
            {
                var front = run.Front();
                if (front == null)
                {
                    continue;
                }

                var length = Round(PushBackCellDepth.BedLength(run.Source, front, run.SourceLevel));
                if (length <= 0.0)
                {
                    continue;
                }

                var lanes = Math.Max(1, front.PalletCount);
                grouped[length] = grouped.TryGetValue(length, out var current) ? current + lanes : lanes;
            }

            EmitBeds(components, grouped);
        }

        /// <summary>
        /// I-42 — como mucho UN tope por cama fisica, y solo en su extremo ALTO. Encontradas admiten dos topes
        /// independientes (uno por cama); una corrida admite exactamente uno, del lado que sea su extremo alto.
        /// </summary>
        private static void AddRunRearTopes(
            ICollection<BomComponent> components, PushBackSystem system, RackCatalog catalog, PushBackRunSet runs)
        {
            // I-42 (correccion aislada 4) — la VARIANTE tambien sale de la cama, no del rack. Se tomaba de
            // system.RearTope, que en un compuesto es la configuracion de un solo lado: con topes distintos en A y en
            // B —medido: LARGUERO_ESCALON_TOPE_DE_3 y POSTE_3_1_5_8_TOPE— los dibujos ponian cada uno el suyo y el
            // BOM contaba los ocho como si fueran del primero. La aplicabilidad ya se preguntaba por cama; ahora la
            // pieza tambien.
            var grouped = new Dictionary<(string PieceId, double Length), int>();
            foreach (var run in runs.Runs)
            {
                var front = run.Front();
                var source = run.Source?.Structure;
                if (front == null || source == null)
                {
                    continue;
                }

                var tope = run.Source.RearTope ?? new PushBackRearTopeConfig();
                if (!tope.At(run.SourceFrontIndex, run.SourceLevel - 1))
                {
                    continue;
                }

                var length = Round(
                    PushBackLoadBeamGeometry.CellBeamLength(source, front, run.SourceLevel)
                    + SelectiveTopePlacement.LengthAllowance);
                var key = (PushBackRearTopeBuilder.ResolvePieceId(catalog, tope), length);
                grouped[key] = grouped.TryGetValue(key, out var current) ? current + 1 : 1;
            }

            foreach (var piece in grouped.GroupBy(entry => entry.Key.PieceId).OrderBy(group => group.Key))
            {
                var label = catalog?.SafetyElements?.FirstOrDefault(entry =>
                    string.Equals(entry?.Id, piece.Key, StringComparison.OrdinalIgnoreCase))?.Label ?? piece.Key;
                EmitTopes(
                    components,
                    piece.ToDictionary(entry => entry.Key.Length, entry => entry.Value),
                    piece.Key,
                    label);
            }
        }

        /// <summary>
        /// Low IN/OUT (one per front x level, resolved PER CELL via <see cref="DynamicRackLevelGeometry.At"/> — id, peralte
        /// and length can differ by cell) or high TROQUEL_REDONDO (one per front x level, peralte per cell, transverse
        /// LONGITUD = the corresponding IN/OUT's). Grouped by ProfileId, length and peralte.
        /// </summary>
        private static void AddEndBeams(ICollection<BomComponent> components, PushBackSystem system, RackCatalog catalog, string category, bool isHighEnd)
        {
            var structure = system.Structure;
            var highId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId) ? PushBackDefaults.HighEndBeamCatalogId : system.HighEndBeamCatalogId;

            var grouped = new Dictionary<(string BeamId, double Length, double Peralte), int>();
            for (var frontIndex = 0; frontIndex < structure.Fronts.Count; frontIndex++)
            {
                var front = structure.Fronts[frontIndex];
                for (var level = 0; level < DynamicFrontActivation.EffectiveLoadLevels(front); level++)
                {
                    string beamId;
                    double peralte;
                    double length;
                    // Both the low IN/OUT and the high TROQUEL_REDONDO of a cell share the SAME transverse length,
                    // resolved per front and level (never front.BeamLength directly for every level).
                    length = PushBackLoadBeamGeometry.CellBeamLength(structure, front, level + 1);
                    if (isHighEnd)
                    {
                        beamId = highId;
                        peralte = system.HighEndBeamPeralteAt(frontIndex, level);
                    }
                    else
                    {
                        var configuration = DynamicRackLevelGeometry.At(structure, front, level + 1);
                        beamId = string.IsNullOrWhiteSpace(configuration.InOutBeamCatalogId)
                            ? (string.IsNullOrWhiteSpace(structure.InOutBeamCatalogId) ? DynamicRackDefaults.InOutBeamCatalogId : structure.InOutBeamCatalogId)
                            : configuration.InOutBeamCatalogId;
                        peralte = configuration.InOutBeamDepth > 0.0 ? configuration.InOutBeamDepth : structure.InOutBeamDepth;
                    }

                    var key = (beamId, Round(length), Round(peralte));
                    grouped[key] = grouped.TryGetValue(key, out var current) ? current + 1 : 1;
                }
            }

            EmitBeams(components, catalog, grouped, category);
        }

        /// <summary>
        /// I-42 — los largueros INTERMEDIOS por cama fisica, contados con el MISMO builder que los dibuja. Es la
        /// unica forma de que la cantidad del BOM y la del plano no puedan divergir: se cuentan las piezas que se
        /// materializan, no una regla paralela sobre la estructura.
        /// </summary>
        private static void AddRunIntermediates(
            ICollection<BomComponent> components, RackCatalog catalog, PushBackRunSet runs)
            => EmitIntermediates(
                components,
                catalog,
                PushBackCompositeContent.Batches(runs, null)
                    .Where(batch => batch.Front != null)
                    .Select(batch => (batch.Source, batch.Front, (IReadOnlyCollection<int>)batch.Levels)));

        /// <summary>
        /// I-42 (ronda 6A) — los intermedios de un rack de UN SOLO SENTIDO: cada frente con TODOS sus niveles. La
        /// enumeracion es lo unico que cambia respecto del compuesto; el conteo es el mismo y vive en un solo sitio.
        /// </summary>
        private static void AddFrontIntermediates(
            ICollection<BomComponent> components, PushBackSystem system, RackCatalog catalog)
            => EmitIntermediates(
                components,
                catalog,
                (system.Structure?.Fronts ?? new List<DynamicRackFront>())
                    .Where(front => front != null)
                    .Select(front => (system, front, (IReadOnlyCollection<int>)null)));

        /// <summary>
        /// LA CUENTA DE INTERMEDIOS, unica para las dos rutas: se materializan las piezas con el MISMO builder que
        /// las dibuja y se agrupan. Es la unica forma de que la cantidad del BOM y la del plano no puedan divergir —
        /// se cuentan las piezas que existen, no una regla paralela sobre la estructura.
        /// </summary>
        private static void EmitIntermediates(
            ICollection<BomComponent> components,
            RackCatalog catalog,
            IEnumerable<(PushBackSystem Source, DynamicRackFront Front, IReadOnlyCollection<int> Levels)> beds)
        {
            var builder = new PushBackIntermediateBeamLateralBuilder();
            var grouped = new Dictionary<(string BeamId, double Length, double Peralte), int>();
            foreach (var bed in beds)
            {
                foreach (var instance in builder.BuildFor(bed.Source, catalog, bed.Front, bed.Levels))
                {
                    var peralte = instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.PeralteParam, out var value)
                        ? value
                        : DynamicRackDefaults.DefaultIntermediateBeamDepth;
                    var key = (instance.PieceId, Round(bed.Front.BeamLength), Round(peralte));
                    grouped[key] = grouped.TryGetValue(key, out var current) ? current + 1 : 1;
                }
            }

            EmitBeams(components, catalog, grouped, SystemBomBuilder.IntermediateBeam);
        }

        /// <summary>
        /// La emision de los largueros de extremo agrupados. Vive en UN sitio: el camino de un solo sentido y el
        /// compuesto solo se diferencian en COMO se cuentan (celdas de un frente frente a camas fisicas), nunca en
        /// como se describen ni como se agrupan.
        /// </summary>
        private static void EmitBeams(
            ICollection<BomComponent> components,
            RackCatalog catalog,
            Dictionary<(string BeamId, double Length, double Peralte), int> grouped,
            string category)
        {
            foreach (var group in grouped.OrderBy(g => g.Key.BeamId, StringComparer.OrdinalIgnoreCase).ThenBy(g => g.Key.Length).ThenBy(g => g.Key.Peralte))
            {
                var label = catalog?.BeamProfiles?.FirstOrDefault(entry => string.Equals(entry?.Id, group.Key.BeamId, StringComparison.OrdinalIgnoreCase))?.Label ?? group.Key.BeamId;
                var description = string.Format(CultureInfo.InvariantCulture, "{0} · Peralte {1:0.##}\"", label, group.Key.Peralte);
                components.Add(new BomComponent
                {
                    Category = category,
                    ProfileId = group.Key.BeamId,
                    Description = description,
                    Length = group.Key.Length,
                    Quantity = group.Value,
                    Pieces = new List<BomLine> { new BomLine { Category = category, ProfileId = group.Key.BeamId, Description = description, Length = group.Key.Length, Quantity = 1 } }
                });
            }
        }

        /// <summary>
        /// One OPAQUE bed per lane and level (its rail/roller recipe is not exploded), length = the CELL's full
        /// structural span. I-41 (PB-015): la longitud se pregunta POR CELDA, porque con fondos escalonados los
        /// niveles de un mismo frente ya no comparten cama — cotizar la del frente entero facturaria riel que no
        /// existe. Sin overrides todas las celdas de un frente responden lo mismo y el BOM sale identico al anterior.
        /// </summary>
        private static void AddBeds(ICollection<BomComponent> components, PushBackSystem system)
        {
            var structure = system.Structure;
            var grouped = new Dictionary<double, int>();
            foreach (var front in structure.Fronts)
            {
                var lanes = Math.Max(1, front.PalletCount);
                for (var level = 0; level < DynamicFrontActivation.EffectiveLoadLevels(front); level++)
                {
                    var length = Round(PushBackCellDepth.BedLength(system, front, level + 1));
                    if (length <= 0.0)
                    {
                        continue;
                    }

                    grouped[length] = grouped.TryGetValue(length, out var current) ? current + lanes : lanes;
                }
            }

            EmitBeds(components, grouped);
        }

        /// <summary>La emision de las camas agrupadas por longitud, compartida por los dos caminos.</summary>
        private static void EmitBeds(ICollection<BomComponent> components, Dictionary<double, int> grouped)
        {
            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                components.Add(new BomComponent
                {
                    Category = SystemBomBuilder.Cama,
                    ProfileId = SystemBomBuilder.Cama,
                    Description = SystemBomBuilder.Cama,
                    Length = group.Key,
                    Quantity = group.Value,
                    Pieces = new List<BomLine>() // opaque: no rail/roller explosion
                });
            }
        }

        /// <summary>One rear tope per ACTIVE cell, of the CONFIGURED variant (PB-005) — the same piece the views place.</summary>
        private static void AddRearTopes(ICollection<BomComponent> components, PushBackSystem system, RackCatalog catalog)
        {
            var structure = system.Structure;
            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            var topeId = PushBackRearTopeBuilder.ResolvePieceId(catalog, rearTope);   // PB-005: one rule, drawing == BOM
            var label = catalog?.SafetyElements?.FirstOrDefault(entry => string.Equals(entry?.Id, topeId, StringComparison.OrdinalIgnoreCase))?.Label ?? topeId;

            var grouped = new Dictionary<double, int>();
            for (var frontIndex = 0; frontIndex < structure.Fronts.Count; frontIndex++)
            {
                var front = structure.Fronts[frontIndex];
                for (var level = 0; level < DynamicFrontActivation.EffectiveLoadLevels(front); level++)
                {
                    if (!rearTope.At(frontIndex, level))
                    {
                        continue;
                    }

                    // Commercial LONGITUD = the cell's transverse beam length (per front x level) + the allowance —
                    // exactly what the lateral/frontal/planta tope blocks carry.
                    var length = Round(PushBackLoadBeamGeometry.CellBeamLength(structure, front, level + 1) + SelectiveTopePlacement.LengthAllowance);
                    grouped[length] = grouped.TryGetValue(length, out var current) ? current + 1 : 1;
                }
            }

            EmitTopes(components, grouped, topeId, label);
        }

        /// <summary>La emision de los topes agrupados por longitud, compartida por los dos caminos.</summary>
        private static void EmitTopes(
            ICollection<BomComponent> components, Dictionary<double, int> grouped, string topeId, string label)
        {
            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                components.Add(new BomComponent
                {
                    Category = RearTope,
                    ProfileId = topeId,
                    Description = label,
                    Length = group.Key,
                    Quantity = group.Value,
                    Pieces = new List<BomLine> { new BomLine { Category = RearTope, ProfileId = topeId, Description = label, Length = group.Key, Quantity = 1 } }
                });
            }
        }

        private static BomComponent Clone(BomComponent source)
            => new BomComponent
            {
                Category = source.Category,
                ProfileId = source.ProfileId,
                Description = source.Description,
                Length = source.Length,
                Quantity = source.Quantity,
                Pieces = source.Pieces.Select(piece => new BomLine
                {
                    Category = piece.Category,
                    ProfileId = piece.ProfileId,
                    Description = piece.Description,
                    Length = piece.Length,
                    Quantity = piece.Quantity
                }).ToList()
            };

        private static double Round(double value) => Math.Round(value, 4);
    }
}
