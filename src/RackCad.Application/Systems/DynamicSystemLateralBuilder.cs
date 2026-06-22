using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the block plan for a whole dynamic (pallet flow) system in the lateral view: each header
    /// module is the lateral header (reused from <see cref="LateralHeaderLayoutBuilder"/>) shifted to its
    /// position along the run, and each separator module gets a separator beam at every vertical level
    /// (<see cref="SeparatorLevelCalculator"/>). Pure: it returns a <see cref="LateralHeaderLayout"/> the
    /// AutoCAD drawer can turn into one block. Headers use their LATERAL blocks; separators use the FRONTAL
    /// block (that is how a separator reads in the system's lateral view).
    /// </summary>
    public sealed class DynamicSystemLateralBuilder
    {
        private readonly LateralHeaderLayoutBuilder headerBuilder = new LateralHeaderLayoutBuilder();

        public LateralHeaderLayout Build(DynamicRackSystem system, RackCatalog catalog)
        {
            var instances = new List<HeaderBlockInstance>();

            if (system == null)
            {
                return new LateralHeaderLayout(instances, 0.0, 0, 0, 0.0);
            }

            var separatorLevels = ResolveSeparatorLevels(system, catalog, out var separatorBlock, out var separatorMate, out var separatorId);

            var headerCount = 0;
            var separatorCount = 0;

            foreach (var module in system.Modules)
            {
                if (module.IsHeader && module.AssociatedFrameConfiguration != null)
                {
                    var parameters = LateralHeaderParametersFactory.FromConfiguration(module.AssociatedFrameConfiguration);
                    var layout = headerBuilder.Build(module.AssociatedFrameConfiguration, parameters, catalog);

                    foreach (var instance in layout.Instances)
                    {
                        Shift(instance, module.StartX);
                        instances.Add(instance);
                    }

                    headerCount++;
                }
                else if (module.Kind == DynamicRackModuleKind.Separator && module.Length > 0.0 && separatorBlock != null)
                {
                    foreach (var level in separatorLevels)
                    {
                        instances.Add(MakeSeparator(separatorId, separatorBlock, separatorMate, module.StartX, level, module.Length));
                        separatorCount++;
                    }
                }
            }

            return new LateralHeaderLayout(instances, 0.0, headerCount, separatorCount, 0.0);
        }

        private static IReadOnlyList<double> ResolveSeparatorLevels(
            DynamicRackSystem system, RackCatalog catalog,
            out string separatorBlock, out Point2D separatorMate, out string separatorId)
        {
            separatorId = DynamicRackDefaults.SeparatorCatalogId;
            separatorBlock = Block(catalog, separatorId, DynamicRackDefaults.SeparatorView);
            separatorMate = Local(catalog, separatorId, DynamicRackDefaults.SeparatorMatePoint, DynamicRackDefaults.SeparatorView);

            var headerModule = system.Modules.FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration != null);
            if (headerModule == null)
            {
                return Array.Empty<double>();
            }

            var configuration = headerModule.AssociatedFrameConfiguration;
            var postId = configuration.LeftPost?.PostCatalogId;
            var troquelSeparadorY = Local(catalog, postId, DynamicRackDefaults.SeparatorPostPoint, "LATERAL").Y;
            var paso = configuration.PasoTroquel > 0.0 ? configuration.PasoTroquel : 2.0;

            return SeparatorLevelCalculator.Levels(configuration.Height, troquelSeparadorY, paso);
        }

        private static HeaderBlockInstance MakeSeparator(
            string separatorId, string block, Point2D mate, double startX, double level, double length)
        {
            var anchor = new Point2D(startX, level);
            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Separator,
                PieceId = separatorId,
                BlockName = block,
                View = DynamicRackDefaults.SeparatorView,
                RotationRadians = 0.0,
                ConnectionAnchor = anchor,
                Insertion = new Point2D(anchor.X - mate.X, anchor.Y - mate.Y)
            };
            instance.DynamicParameters["LONGITUD"] = length;
            return instance;
        }

        /// <summary>Shift a header instance along the run (X), keeping its height (Y).</summary>
        private static void Shift(HeaderBlockInstance instance, double dx)
        {
            instance.Insertion = new Point2D(instance.Insertion.X + dx, instance.Insertion.Y);
            instance.ConnectionAnchor = new Point2D(instance.ConnectionAnchor.X + dx, instance.ConnectionAnchor.Y);
        }

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(pieceId, connectionPointId, view);
            return entry == null ? new Point2D(0.0, 0.0) : new Point2D(entry.LocalX, entry.LocalY);
        }

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => catalog?.Blocks.FindBlock(pieceId, view)?.BlockName;
    }
}
