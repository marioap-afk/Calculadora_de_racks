using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Preliminary bill of materials for a RESOLVED selective rack, aggregated from the placed block instances:
    /// posts by height, one base plate per post, largueros by length + peralte, and two ménsulas per larguero.
    /// Grouped by (category, piece, length, peralte) with quantities summed. Pure and unit-testable; no AutoCAD.
    /// </summary>
    public static class SelectiveBomBuilder
    {
        public const string Post = "Poste";
        public const string BasePlate = "Placa base";
        public const string Beam = "Larguero";
        public const string Mensula = "Ménsula";

        public static BillOfMaterials Build(IReadOnlyList<HeaderBlockInstance> instances, RackCatalog catalog)
        {
            var raw = new List<RawItem>();

            foreach (var instance in instances ?? Enumerable.Empty<HeaderBlockInstance>())
            {
                switch (instance.Role)
                {
                    case HeaderBlockRole.Post:
                        raw.Add(new RawItem(Post, instance.PieceId, Round(Param(instance, SelectiveRackDefaults.LengthParam)), 0.0, 1));
                        break;
                    case HeaderBlockRole.BasePlate:
                        // Plates of different peralte are different parts; carry the peralte in the grouping key.
                        raw.Add(new RawItem(BasePlate, instance.PieceId, 0.0, Round(Param(instance, SelectiveRackDefaults.PeralteParam)), 1));
                        break;
                    case HeaderBlockRole.Beam:
                        var length = Round(Param(instance, SelectiveRackDefaults.LengthParam));
                        var peralte = Round(Param(instance, SelectiveRackDefaults.PeralteParam));
                        raw.Add(new RawItem(Beam, instance.PieceId, length, peralte, 1));

                        var mensula = catalog?.BeamProfiles
                            .FirstOrDefault(b => string.Equals(b?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase))?.Mensula;
                        if (!string.IsNullOrWhiteSpace(mensula))
                        {
                            raw.Add(new RawItem(Mensula, mensula, 0.0, 0.0, 2));
                        }

                        break;
                }
            }

            var lines = raw
                .GroupBy(item => (item.Category, item.PieceId, item.Length, item.Peralte))
                .Select(group => new BomLine
                {
                    Category = group.Key.Category,
                    ProfileId = group.Key.PieceId,
                    Length = group.Key.Length,
                    Quantity = group.Sum(item => item.Quantity),
                    Description = Describe(catalog, group.Key.Category, group.Key.PieceId, group.Key.Peralte)
                })
                .OrderBy(line => Order(line.Category))
                .ThenBy(line => line.ProfileId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(line => line.Length)
                .ToList();

            return new BillOfMaterials(lines);
        }

        private static string Describe(RackCatalog catalog, string category, string id, double peralte)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(id))
            {
                return id ?? string.Empty;
            }

            string label;
            switch (category)
            {
                case Post:
                    label = catalog.PostProfiles.FindProfile(id)?.Label ?? id;
                    break;
                case BasePlate:
                    label = catalog.BasePlates.FindBasePlate(id)?.Label ?? id;
                    break;
                case Beam:
                    label = catalog.BeamProfiles.FirstOrDefault(b => string.Equals(b?.Id, id, StringComparison.OrdinalIgnoreCase))?.Label ?? id;
                    break;
                case Mensula:
                    label = catalog.Mensulas.FirstOrDefault(m => string.Equals(m?.Id, id, StringComparison.OrdinalIgnoreCase))?.Label ?? id;
                    break;
                default:
                    label = id;
                    break;
            }

            // The peralte distinguishes otherwise-identical largueros and plates.
            return peralte > 0.0
                ? label + " · P" + peralte.ToString("0.###", CultureInfo.InvariantCulture)
                : label;
        }

        private static double Param(HeaderBlockInstance instance, string name)
            => instance.DynamicParameters.TryGetValue(name, out var value) ? value : 0.0;

        private static double Round(double value) => Math.Round(value, 2);

        private static int Order(string category)
        {
            switch (category)
            {
                case Post: return 0;
                case BasePlate: return 1;
                case Beam: return 2;
                case Mensula: return 3;
                default: return 4;
            }
        }

        private readonly struct RawItem
        {
            public RawItem(string category, string pieceId, double length, double peralte, int quantity)
            {
                Category = category;
                PieceId = pieceId;
                Length = length;
                Peralte = peralte;
                Quantity = quantity;
            }

            public string Category { get; }
            public string PieceId { get; }
            public double Length { get; }
            public double Peralte { get; }
            public int Quantity { get; }
        }
    }
}
