using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>What the drawing places at one post of a Dinámico: the pieces of each safety family, counted as the BOM counts them.</summary>
    public sealed class DynamicPostSafetyRow
    {
        public DynamicPostSafetyRow(int postIndex) => PostIndex = postIndex;

        /// <summary>Zero-based index of the post (P1 is 0).</summary>
        public int PostIndex { get; }

        public int Boots { get; internal set; }

        public int SideGuards { get; internal set; }

        public int DeflectorsAtExit { get; internal set; }

        public int DeflectorsAtEntrance { get; internal set; }

        public int Defenses { get; internal set; }

        public int EntranceGuides { get; internal set; }
    }

    /// <summary>
    /// The safety of a Dinámico post by post (step 4 of Dinámico in the RackCad window; decision 6 of the prototype: «Seguridad por poste en
    /// sección propia»). It is read from the same projections as the BOM (<see cref="SystemBomBuilder"/>): the plan for the boots, the side
    /// guards and the defenses; the two frontal cuts for the deflectors and the entrance guides. So the rows add up to the safety lines of
    /// the BOM and say nothing the drawing does not place. A piece belongs to the post whose line is the nearest to where it is inserted.
    /// </summary>
    public static class DynamicPostSafetySummary
    {
        public static IReadOnlyList<DynamicPostSafetyRow> Build(DynamicRackSystem system, RackCatalog catalog)
        {
            if (system == null || catalog == null)
            {
                return Array.Empty<DynamicPostSafetyRow>();
            }

            var layout = DynamicFrontGeometry.Compute(system, catalog);
            if (layout?.PostPositions == null || layout.PostPositions.Count == 0)
            {
                return Array.Empty<DynamicPostSafetyRow>();
            }

            var plateId = DynamicFrontGeometry.PlateId(system, catalog);
            var builder = new DynamicSafetyMultiViewBuilder();
            var planta = new List<HeaderBlockInstance>();
            builder.AppendPlanta(planta, system, catalog, layout, plateId);
            var exit = new List<HeaderBlockInstance>();
            builder.AppendFrontal(exit, system, catalog, layout, plateId, DynamicRackEnd.Exit);
            var entrance = new List<HeaderBlockInstance>();
            builder.AppendFrontal(entrance, system, catalog, layout, plateId, DynamicRackEnd.Entrance);

            var rows = Enumerable.Range(0, layout.PostPositions.Count).Select(post => new DynamicPostSafetyRow(post)).ToList();

            // In the plan the post lines run along Y; in a frontal cut, along X.
            foreach (var instance in planta)
            {
                var row = rows[Nearest(layout.PostPositions, instance.Insertion.Y)];
                if (IsType(instance, catalog, SelectiveSafetyDefaults.BotaType)) row.Boots++;
                else if (IsType(instance, catalog, SelectiveSafetyDefaults.LateralType)) row.SideGuards++;
                else if (IsType(instance, catalog, SelectiveSafetyDefaults.DefensaType)) row.Defenses++;
            }

            foreach (var instance in exit)
            {
                if (IsType(instance, catalog, SelectiveSafetyDefaults.DesviadorType))
                    rows[Nearest(layout.PostPositions, instance.Insertion.X)].DeflectorsAtExit++;
            }

            foreach (var instance in entrance)
            {
                var row = rows[Nearest(layout.PostPositions, instance.Insertion.X)];
                if (IsType(instance, catalog, SelectiveSafetyDefaults.DesviadorType)) row.DeflectorsAtEntrance++;
                else if (IsType(instance, catalog, SelectiveSafetyDefaults.GuiaType)) row.EntranceGuides++;
            }

            return rows;
        }

        private static int Nearest(IReadOnlyList<double> positions, double at)
        {
            var best = 0;
            for (var i = 1; i < positions.Count; i++)
            {
                if (Math.Abs(positions[i] - at) < Math.Abs(positions[best] - at)) best = i;
            }

            return best;
        }

        private static bool IsType(HeaderBlockInstance instance, RackCatalog catalog, string type)
        {
            if (instance == null || instance.Role != HeaderBlockRole.Safety || string.IsNullOrWhiteSpace(instance.PieceId))
            {
                return false;
            }

            var element = catalog.SafetyElements?.FirstOrDefault(entry => string.Equals(entry?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));
            return element != null && SelectiveSafetyDefaults.IsType(element.Type, type);
        }
    }
}
