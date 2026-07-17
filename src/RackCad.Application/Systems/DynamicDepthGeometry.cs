using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
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

    public static class DynamicDepthGeometry
    {
        public static DynamicDepthLayout Resolve(
            IEnumerable<DynamicRackFrontDesign> fronts,
            int legacyPalletsDeep)
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
            if (baseRanges.Any(range => range.StartPosition != baseRange.StartPosition))
            {
                throw new ArgumentException("Los frentes con el menor número de fondos deben compartir la misma posición inicial.");
            }

            if (ranges.Any(range => !range.Contains(baseRange)))
            {
                throw new ArgumentException("Cada frente debe contener la estructura completa del frente con menos fondos.");
            }

            return new DynamicDepthLayout(
                baseRange,
                ranges.Max(range => range.EndPosition),
                ranges);
        }

        public static DynamicDepthLayout Resolve(DynamicRackSystem system)
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
            return Resolve(designs, system.PalletsDeep);
        }

        public static DynamicDepthRange AtPost(DynamicRackSystem system, int postIndex)
        {
            var adjacent = DynamicFrontGeometry.AdjacentFronts(system, postIndex);
            if (adjacent.Count == 0)
            {
                return new DynamicDepthRange(1, Math.Max(0, system?.PalletsDeep ?? 0));
            }

            var start = adjacent.Min(front => front.DepthStartPosition);
            var end = adjacent.Max(front => front.DepthStartPosition + front.PalletsDeep - 1);
            return new DynamicDepthRange(start, end - start + 1);
        }

        public static IReadOnlyList<DynamicRackModule> ModulesInRange(
            DynamicRackSystem system,
            DynamicDepthRange range)
            => system?.Modules.Where(module => range.Contains(module.Index + 1)).ToList()
               ?? (IReadOnlyList<DynamicRackModule>)Array.Empty<DynamicRackModule>();

        /// <summary>
        /// A front range may begin or end in a separator. That does not turn the module into a header, but the
        /// separator still needs a physical endpoint post on that transverse line.
        /// </summary>
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
