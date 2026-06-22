using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Bom
{
    /// <summary>
    /// Aggregates a configuration into a preliminary bill of materials: posts and base plates
    /// from the configuration plus the derived horizontal/diagonal members, grouped by
    /// (category, profile, length) with quantities summed. Pure and unit-testable; no AutoCAD.
    /// </summary>
    public static class BomBuilder
    {
        public const string Post = "Poste";
        public const string Reinforcement = "Refuerzo";
        public const string BasePlate = "Placa base";
        public const string Horizontal = "Horizontal";
        public const string Diagonal = "Diagonal";

        public static BillOfMaterials Build(RackFrameConfiguration configuration, RackCatalog catalog)
        {
            var raw = new List<RawItem>();

            if (configuration != null)
            {
                AddPost(raw, configuration.LeftPost, configuration.Height);
                AddPost(raw, configuration.RightPost, configuration.Height);
                AddPlate(raw, configuration.LeftBasePlate);
                AddPlate(raw, configuration.RightBasePlate);

                foreach (var member in configuration.Members ?? Enumerable.Empty<FrameMember>())
                {
                    var category = CategoryFor(member.MemberType);

                    if (category != null)
                    {
                        raw.Add(new RawItem(category, member.ProfileId, Round(member.Length), Math.Max(1, member.Quantity)));
                    }
                }
            }

            var lines = raw
                .GroupBy(item => (item.Category, Id: Normalize(item.ProfileId), item.Length))
                .Select(group => new BomLine
                {
                    Category = group.Key.Category,
                    ProfileId = group.Key.Id,
                    Length = group.Key.Length,
                    Quantity = group.Sum(item => item.Quantity),
                    Description = ResolveDescription(catalog, group.Key.Category, group.Key.Id)
                })
                .OrderBy(line => CategoryOrder(line.Category))
                .ThenBy(line => line.ProfileId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(line => line.Length)
                .ToList();

            return new BillOfMaterials(lines);
        }

        /// <summary>Merges several bills of materials into one, summing quantities of identical lines.</summary>
        public static BillOfMaterials Merge(IEnumerable<BillOfMaterials> boms)
        {
            var lines = (boms ?? Enumerable.Empty<BillOfMaterials>())
                .Where(bom => bom != null)
                .SelectMany(bom => bom.Lines)
                .GroupBy(line => (line.Category, Id: Normalize(line.ProfileId), line.Length))
                .Select(group => new BomLine
                {
                    Category = group.Key.Category,
                    ProfileId = group.Key.Id,
                    Length = group.Key.Length,
                    Quantity = group.Sum(line => line.Quantity),
                    Description = group.Select(line => line.Description).FirstOrDefault(d => !string.IsNullOrEmpty(d)) ?? string.Empty
                })
                .OrderBy(line => CategoryOrder(line.Category))
                .ThenBy(line => line.ProfileId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(line => line.Length)
                .ToList();

            return new BillOfMaterials(lines);
        }

        private static void AddPost(List<RawItem> raw, PostAssembly post, double height)
        {
            if (post == null || string.IsNullOrWhiteSpace(post.PostCatalogId))
            {
                return;
            }

            raw.Add(new RawItem(Post, post.PostCatalogId, Round(height), 1));

            if (post.HasReinforcement && !string.IsNullOrWhiteSpace(post.ReinforcementCatalogId))
            {
                raw.Add(new RawItem(Reinforcement, post.ReinforcementCatalogId, Round(height), 1));
            }
        }

        private static void AddPlate(List<RawItem> raw, BasePlatePlacement plate)
        {
            if (plate != null && !string.IsNullOrWhiteSpace(plate.PlateCatalogId))
            {
                raw.Add(new RawItem(BasePlate, plate.PlateCatalogId, 0.0, 1));
            }
        }

        private static string CategoryFor(FrameMemberType type)
        {
            switch (type)
            {
                case FrameMemberType.LowerHorizontal:
                case FrameMemberType.UpperHorizontal:
                case FrameMemberType.IntermediateHorizontal:
                case FrameMemberType.AdditionalHorizontal:
                    return Horizontal;
                case FrameMemberType.DiagonalBrace:
                    return Diagonal;
                default:
                    return null;
            }
        }

        private static string ResolveDescription(RackCatalog catalog, string category, string id)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(id))
            {
                return string.Empty;
            }

            if (category == BasePlate)
            {
                return catalog.BasePlates.FindBasePlate(id)?.Description ?? string.Empty;
            }

            // Reinforcements are posts; horizontals and diagonals are both truss members.
            var list = category == Post || category == Reinforcement ? catalog.PostProfiles
                : category == Horizontal || category == Diagonal ? catalog.TrussProfiles
                : null;

            return list?.FindProfile(id)?.Description ?? string.Empty;
        }

        private static int CategoryOrder(string category)
        {
            switch (category)
            {
                case Post: return 0;
                case Reinforcement: return 1;
                case BasePlate: return 2;
                case Horizontal: return 3;
                case Diagonal: return 4;
                default: return 5;
            }
        }

        private static double Round(double value)
        {
            return Math.Round(value, 2);
        }

        private static string Normalize(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        private readonly struct RawItem
        {
            public RawItem(string category, string profileId, double length, int quantity)
            {
                Category = category;
                ProfileId = profileId;
                Length = length;
                Quantity = quantity;
            }

            public string Category { get; }
            public string ProfileId { get; }
            public double Length { get; }
            public int Quantity { get; }
        }
    }
}
