using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Preliminary bill of materials for a roller bed ("cama de rodamiento"), aggregated from the placed block
    /// instances: the rail (by LONGITUD = lane depth), the end stop (tope), the rollers and — on a dynamic bed —
    /// the brakes (frenos). Grouped by (category, piece, length) with quantities summed. Pure and unit-testable; no
    /// AutoCAD. Mirrors <see cref="SelectiveBomBuilder"/> so the same <c>RackBomWindow</c>/CSV export are reused.
    /// </summary>
    public static class FlowBedBomBuilder
    {
        public const string Rail = "Riel";
        public const string Stop = "Tope";
        public const string Roller = "Rodillo";
        public const string Brake = "Freno";

        public static BillOfMaterials Build(IReadOnlyList<HeaderBlockInstance> instances, RackCatalog catalog)
        {
            var raw = new List<RawItem>();

            foreach (var instance in instances ?? Enumerable.Empty<HeaderBlockInstance>())
            {
                switch (instance.Role)
                {
                    case HeaderBlockRole.Rail:
                        raw.Add(new RawItem(Rail, instance.PieceId, Round(Param(instance, "LONGITUD")), 1));
                        break;
                    case HeaderBlockRole.Stop:
                        raw.Add(new RawItem(Stop, instance.PieceId, 0.0, 1));
                        break;
                    case HeaderBlockRole.Roller:
                        raw.Add(new RawItem(Roller, instance.PieceId, 0.0, 1));
                        break;
                    case HeaderBlockRole.Brake:
                        raw.Add(new RawItem(Brake, instance.PieceId, 0.0, 1));
                        break;
                }
            }

            var lines = raw
                .GroupBy(item => (item.Category, item.PieceId, item.Length))
                .Select(group => new BomLine
                {
                    Category = group.Key.Category,
                    ProfileId = group.Key.PieceId,
                    Length = group.Key.Length,
                    Quantity = group.Sum(item => item.Quantity),
                    Description = Describe(catalog, group.Key.PieceId)
                })
                .OrderBy(line => Order(line.Category))
                .ThenBy(line => line.ProfileId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(line => line.Length)
                .ToList();

            return new BillOfMaterials(lines);
        }

        private static string Describe(RackCatalog catalog, string id)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(id))
            {
                return id ?? string.Empty;
            }

            return catalog.FlowBedProfiles
                .FirstOrDefault(c => string.Equals(c?.Id, id, StringComparison.OrdinalIgnoreCase))?.Label ?? id;
        }

        private static double Param(HeaderBlockInstance instance, string name)
            => instance.DynamicParameters.TryGetValue(name, out var value) ? value : 0.0;

        private static double Round(double value) => Math.Round(value, 2);

        private static int Order(string category)
        {
            switch (category)
            {
                case Rail: return 0;
                case Stop: return 1;
                case Roller: return 2;
                case Brake: return 3;
                default: return 4;
            }
        }

        private readonly struct RawItem
        {
            public RawItem(string category, string pieceId, double length, int quantity)
            {
                Category = category;
                PieceId = pieceId;
                Length = length;
                Quantity = quantity;
            }

            public string Category { get; }
            public string PieceId { get; }
            public double Length { get; }
            public int Quantity { get; }
        }
    }
}
