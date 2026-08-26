using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>Resolved longitudinal range of one transverse front in the shared structural grid.</summary>
    public readonly struct DynamicDepthRange
    {
        public DynamicDepthRange(int startPosition, int palletsDeep)
        {
            StartPosition = startPosition;
            PalletsDeep = palletsDeep;
        }

        public int StartPosition { get; }
        public int PalletsDeep { get; }
        public int EndPosition => StartPosition + PalletsDeep - 1;

        public bool Contains(int position)
            => position >= StartPosition && position <= EndPosition;

        public bool Contains(DynamicDepthRange other)
            => Contains(other.StartPosition) && Contains(other.EndPosition);
    }

    /// <summary>
    /// I-42 — lo que una linea fisica transversal cubre a lo largo de la profundidad: uno o varios tramos.
    ///
    /// <para>
    /// Un rack de un solo sentido tiene siempre UN tramo y esto se comporta igual que el rango de siempre. Un rack
    /// compuesto puede tener dos —el de cada lado— con profundidad sin usar en medio, y por eso la pregunta que
    /// responden los materializadores no es «entre que dos posiciones» sino «esta ESTA posicion cubierta».
    /// </para>
    /// </summary>
    public sealed class DynamicDepthCoverage
    {
        private readonly IReadOnlyList<DynamicDepthRange> segments;

        public DynamicDepthCoverage(IReadOnlyList<DynamicDepthRange> segments)
        {
            this.segments = Normalize(segments);
        }

        /// <summary>
        /// Los tramos se FUSIONAN cuando se solapan o se tocan, de modo que la cobertura queda en tramos minimos y
        /// disjuntos. Sin esto un poste de frontera se contaria una vez por frente adyacente en vez de una vez por
        /// tramo: dos frentes que comparten la misma profundidad son UNA sola linea continua, no dos.
        /// </summary>
        private static IReadOnlyList<DynamicDepthRange> Normalize(IReadOnlyList<DynamicDepthRange> source)
        {
            var ordered = (source ?? Array.Empty<DynamicDepthRange>())
                .Where(segment => segment.PalletsDeep > 0)
                .OrderBy(segment => segment.StartPosition)
                .ThenBy(segment => segment.EndPosition)
                .ToList();

            var result = new List<DynamicDepthRange>();
            foreach (var segment in ordered)
            {
                if (result.Count == 0)
                {
                    result.Add(segment);
                    continue;
                }

                var last = result[result.Count - 1];
                if (segment.StartPosition > last.EndPosition + 1)
                {
                    result.Add(segment);
                    continue;
                }

                var end = Math.Max(last.EndPosition, segment.EndPosition);
                result[result.Count - 1] = new DynamicDepthRange(
                    last.StartPosition, end - last.StartPosition + 1);
            }

            return result;
        }

        /// <summary>Los tramos cubiertos, tal cual los declararon los frentes.</summary>
        public IReadOnlyList<DynamicDepthRange> Segments => segments;

        /// <summary>True si la cobertura no tiene ningun tramo.</summary>
        public bool IsEmpty => segments.Count == 0;

        /// <summary>La primera posicion cubierta.</summary>
        public int StartPosition => segments.Count == 0 ? 1 : segments.Min(segment => segment.StartPosition);

        /// <summary>La ultima posicion cubierta.</summary>
        public int EndPosition => segments.Count == 0 ? 0 : segments.Max(segment => segment.EndPosition);

        /// <summary>Si una posicion concreta esta cubierta por alguno de los tramos.</summary>
        public bool Contains(int position) => segments.Any(segment => segment.Contains(position));
    }

    /// <summary>
    /// Shared pallet-flow depth contract. The shortest front owns the two +6 in end allowances and the standard
    /// header/separator pattern. Every longer front must contain that base range and only extends the pattern; its
    /// own first/last position therefore may remain a separator.
    /// </summary>
    public sealed class DynamicDepthLayout
    {
        public DynamicDepthLayout(
            DynamicDepthRange baseRange,
            int totalPositions,
            IReadOnlyList<DynamicDepthRange> frontRanges)
        {
            BaseRange = baseRange;
            TotalPositions = totalPositions;
            FrontRanges = frontRanges ?? Array.Empty<DynamicDepthRange>();
        }

        public DynamicDepthRange BaseRange { get; }
        public int TotalPositions { get; }
        public IReadOnlyList<DynamicDepthRange> FrontRanges { get; }

        public bool IsAllowancePosition(int position)
            => position == BaseRange.StartPosition || position == BaseRange.EndPosition;

        public bool IsHeaderPosition(int position)
        {
            if (position >= BaseRange.StartPosition && position <= BaseRange.EndPosition)
            {
                return IsBaseHeaderPosition(position - BaseRange.StartPosition + 1, BaseRange.PalletsDeep);
            }

            var distance = position < BaseRange.StartPosition
                ? BaseRange.StartPosition - position
                : position - BaseRange.EndPosition;
            return distance % 2 == 0;
        }

        public static bool IsBaseHeaderPosition(int position, int palletsDeep)
        {
            if (palletsDeep == 2)
            {
                return true;
            }

            if (palletsDeep % 2 == 1)
            {
                return position % 2 == 1;
            }

            var doublingStart = 2 * (palletsDeep / 4);
            return position <= doublingStart
                ? position % 2 == 1
                : (position - doublingStart) % 2 == 0;
        }
    }

    /// <summary>
    /// Si los rangos de profundidad de los frentes deben ANIDAR unos en otros.
    /// </summary>
    public enum DynamicDepthNesting
    {
        /// <summary>
        /// Contrato historico del sistema Dinamico: todo frente contiene el rango del frente con menos fondos. Es lo
        /// que sostiene su patron de cabeceras y separadores, y NO se relaja para el.
        /// </summary>
        Required = 0,

        /// <summary>
        /// I-42 — el Push Back COMPUESTO. Con dos lados enfrentados una ranura puede vivir solo en la mitad de A
        /// (rango pegado al arranque) y otra solo en la mitad de B (rango pegado al final): ninguna contiene a la
        /// otra y las dos son fisicamente reales sobre UNA sola estructura. Solo se relaja el ANIDAMIENTO; el resto
        /// de invariantes —minimo de dos fondos, posicion inicial valida y que alguna ranura arranque en 1— siguen
        /// exigiendose igual.
        /// </summary>
        NotRequired = 1
    }

    public static class DynamicDepthGeometry
    {
        public static DynamicDepthLayout Resolve(
            IEnumerable<DynamicRackFrontDesign> fronts,
            int legacyPalletsDeep)
            => Resolve(fronts, legacyPalletsDeep, DynamicDepthNesting.Required);

        public static DynamicDepthLayout Resolve(
            IEnumerable<DynamicRackFrontDesign> fronts,
            int legacyPalletsDeep,
            DynamicDepthNesting nesting)
        {
            var source = fronts?.Where(front => front != null).ToList()
                         ?? new List<DynamicRackFrontDesign>();
            if (source.Count == 0)
            {
                source.Add(new DynamicRackFrontDesign
                {
                    PalletsDeep = Math.Max(2, legacyPalletsDeep),
                    DepthStartPosition = 1
                });
            }

            var fallback = Math.Max(2, legacyPalletsDeep);
            var ranges = source.Select(front => new DynamicDepthRange(
                    front.DepthStartPosition.GetValueOrDefault(1),
                    front.PalletsDeep.GetValueOrDefault(fallback)))
                .ToList();
            if (ranges.Any(range => range.StartPosition < 1 || range.PalletsDeep < 2))
            {
                throw new ArgumentException("Cada frente requiere al menos 2 fondos y una posición inicial >= 1.");
            }

            if (ranges.Min(range => range.StartPosition) != 1)
            {
                throw new ArgumentException("Al menos un frente debe comenzar en la posición de fondo 1.");
            }

            var minimum = ranges.Min(range => range.PalletsDeep);
            var baseRanges = ranges.Where(range => range.PalletsDeep == minimum).ToList();
            var baseRange = baseRanges[0];
            if (nesting == DynamicDepthNesting.Required)
            {
                if (baseRanges.Any(range => range.StartPosition != baseRange.StartPosition))
                {
                    throw new ArgumentException("Los frentes con el menor número de fondos deben compartir la misma posición inicial.");
                }

                if (ranges.Any(range => !range.Contains(baseRange)))
                {
                    throw new ArgumentException("Cada frente debe contener la estructura completa del frente con menos fondos.");
                }
            }
            else
            {
                // I-42: sin anidamiento el rango BASE deja de poder describir un patron comun, asi que se toma el
                // rango que arranca en la posicion 1 y llega mas lejos — el que gobierna el patron de cabeceras del
                // arranque. Es metadato (BaseDepthStartPosition/BasePalletsDeep): ninguna geometria lo consume.
                baseRange = ranges
                    .Where(range => range.StartPosition == 1)
                    .OrderByDescending(range => range.PalletsDeep)
                    .DefaultIfEmpty(baseRange)
                    .First();
            }

            return new DynamicDepthLayout(
                baseRange,
                ranges.Max(range => range.EndPosition),
                ranges);
        }

        public static DynamicDepthLayout Resolve(DynamicRackSystem system)
            => Resolve(system, DynamicDepthNesting.Required);

        public static DynamicDepthLayout Resolve(DynamicRackSystem system, DynamicDepthNesting nesting)
        {
            if (system == null)
            {
                return new DynamicDepthLayout(new DynamicDepthRange(1, 2), 0, Array.Empty<DynamicDepthRange>());
            }

            var designs = system.Fronts.Select(front => new DynamicRackFrontDesign
            {
                PalletsDeep = front?.PalletsDeep > 0 ? front.PalletsDeep : system.PalletsDeep,
                DepthStartPosition = front?.DepthStartPosition > 0 ? front.DepthStartPosition : 1
            });
            return Resolve(designs, system.PalletsDeep, nesting);
        }

        public static DynamicDepthRange AtPost(DynamicRackSystem system, int postIndex)
        {
            var coverage = CoverageAtPost(system, postIndex);
            return new DynamicDepthRange(
                coverage.StartPosition, coverage.EndPosition - coverage.StartPosition + 1);
        }

        /// <summary>
        /// Los TRAMOS de profundidad que un frente ocupa realmente. Sin tramos declarados es su rango continuo de
        /// siempre, que es el caso de todo rack de un solo sentido.
        /// </summary>
        public static IReadOnlyList<DynamicDepthRange> SegmentsOf(DynamicRackFront front)
        {
            if (front == null)
            {
                return Array.Empty<DynamicDepthRange>();
            }

            if (front.DepthSegments.Count == 0)
            {
                return new[] { new DynamicDepthRange(front.DepthStartPosition, front.PalletsDeep) };
            }

            return front.DepthSegments
                .Where(segment => segment.Positions > 0)
                .Select(segment => new DynamicDepthRange(segment.StartPosition, segment.Positions))
                .ToList();
        }

        /// <summary>
        /// ERROR 4 (I-42) — la ENVOLVENTE LONGITUDINAL que una LINEA FISICA TRANSVERSAL tiene que sostener: la union
        /// de lo que demandan los frentes FISICAMENTE ADYACENTES a esa linea, y nada mas.
        ///
        /// <para>
        /// La linea exterior de un frente corto termina donde termina ESE frente; una linea intermedia se extiende
        /// hasta donde llegue el mas profundo de los dos frentes que separa. Un frente remoto no la alarga: la
        /// envolvente no es el maximo del rack.
        /// </para>
        /// <para>
        /// Devuelve una COBERTURA y no un rango porque en un rack compuesto no tiene por que ser continua: un frente
        /// presente en los dos lados demanda profundidad pegada al arranque y pegada al final, y entre las dos puede
        /// quedar estructura que ese frente no usa. Con un solo tramo —todo rack de un sentido— la cobertura es
        /// exactamente el rango de siempre.
        /// </para>
        /// </summary>
        public static DynamicDepthCoverage CoverageAtPost(DynamicRackSystem system, int postIndex)
        {
            var adjacent = DynamicFrontGeometry.AdjacentFronts(system, postIndex);
            if (adjacent.Count == 0)
            {
                return new DynamicDepthCoverage(
                    new[] { new DynamicDepthRange(1, Math.Max(0, system?.PalletsDeep ?? 0)) });
            }

            return new DynamicDepthCoverage(adjacent.SelectMany(SegmentsOf).ToList());
        }

        public static IReadOnlyList<DynamicRackModule> ModulesInRange(
            DynamicRackSystem system,
            DynamicDepthRange range)
            => system?.Modules.Where(module => range.Contains(module.Index + 1)).ToList()
               ?? (IReadOnlyList<DynamicRackModule>)Array.Empty<DynamicRackModule>();

        /// <summary>Los modulos que una COBERTURA alcanza, en orden. Un tramo sin usar no aporta ninguno.</summary>
        public static IReadOnlyList<DynamicRackModule> ModulesInCoverage(
            DynamicRackSystem system,
            DynamicDepthCoverage coverage)
            => system != null && coverage != null
                ? system.Modules.Where(module => coverage.Contains(module.Index + 1)).ToList()
                : (IReadOnlyList<DynamicRackModule>)Array.Empty<DynamicRackModule>();

        /// <summary>
        /// A front range may begin or end in a separator. That does not turn the module into a header, but the
        /// separator still needs a physical endpoint post on that transverse line.
        /// </summary>
        /// <summary>
        /// Los postes de frontera de una COBERTURA: los de cada uno de sus tramos. Un rack compuesto tiene dos
        /// tramos por linea y por tanto puede necesitar cuatro fronteras, no dos.
        /// </summary>
        public static IReadOnlyList<double> BoundaryPostOffsets(
            DynamicRackSystem system,
            DynamicDepthCoverage coverage)
        {
            var result = new List<double>();
            if (system == null || coverage == null)
            {
                return result;
            }

            foreach (var segment in coverage.Segments)
            {
                foreach (var offset in BoundaryPostOffsets(system, segment))
                {
                    if (!result.Any(existing => Math.Abs(existing - offset) <= 1e-6))
                    {
                        result.Add(offset);
                    }
                }
            }

            return result;
        }

        public static IReadOnlyList<double> BoundaryPostOffsets(
            DynamicRackSystem system,
            DynamicDepthRange range)
        {
            var result = new List<double>();
            if (system == null)
            {
                return result;
            }

            var first = system.Modules.FirstOrDefault(module => module.Index + 1 == range.StartPosition);
            var last = system.Modules.FirstOrDefault(module => module.Index + 1 == range.EndPosition);
            if (first != null && !first.IsHeader)
            {
                result.Add(first.StartX);
            }

            if (last != null && !last.IsHeader && (result.Count == 0 || Math.Abs(last.EndX - result[0]) > 1e-6))
            {
                result.Add(last.EndX);
            }

            return result;
        }

        public static void ResolveCoordinates(DynamicRackSystem system)
        {
            if (system == null)
            {
                return;
            }

            foreach (var front in system.Fronts.Where(front => front != null))
            {
                var first = system.Modules.FirstOrDefault(module => module.Index + 1 == front.DepthStartPosition);
                var lastPosition = front.DepthStartPosition + front.PalletsDeep - 1;
                var last = system.Modules.FirstOrDefault(module => module.Index + 1 == lastPosition);
                front.StartX = first?.StartX ?? 0.0;
                front.EndX = last?.EndX ?? front.StartX;
            }
        }

        public static bool Matches(DynamicRackSystem system, DynamicDepthLayout layout)
        {
            if (system == null || layout == null || system.Modules.Count != layout.TotalPositions)
            {
                return false;
            }

            if (system.BaseDepthStartPosition != layout.BaseRange.StartPosition
                || system.BasePalletsDeep != layout.BaseRange.PalletsDeep)
            {
                return false;
            }

            return true;
        }
    }
}
