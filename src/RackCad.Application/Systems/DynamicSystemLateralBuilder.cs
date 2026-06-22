using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the block plan for a whole dynamic (pallet flow) system in the lateral view: each header
    /// module is the lateral header (reused from <see cref="LateralHeaderLayoutBuilder"/>) shifted to its
    /// position along the run; each separator module gets a separator beam at every vertical level
    /// (<see cref="SeparatorLevelCalculator"/>), anchored on the adjacent post's TROQUEL_SEPARADOR; and a
    /// derived post (header post + plate, reinforced full height by default) is placed wherever two
    /// separators meet. Pure: returns a <see cref="LateralHeaderLayout"/> the AutoCAD drawer turns into a
    /// block. Headers use their LATERAL blocks; separators use the FRONTAL block.
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

            var context = Resolve(system, catalog);
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
                else if (module.Kind == DynamicRackModuleKind.Separator && module.Length > 0.0 && context.SeparatorBlock != null)
                {
                    foreach (var level in context.Levels)
                    {
                        instances.Add(MakeSeparator(context, module.StartX, module.Length, level));
                        separatorCount++;
                    }
                }
            }

            // Derived posts: where two separators are consecutive there is a shared post (header post + plate),
            // reinforced full height by default.
            foreach (var offset in system.GetDerivedPostOffsets())
            {
                AddDerivedPost(instances, context, offset);
            }

            return new LateralHeaderLayout(instances, 0.0, headerCount, separatorCount, 0.0);
        }

        private HeaderContext Resolve(DynamicRackSystem system, RackCatalog catalog)
        {
            var context = new HeaderContext
            {
                SeparatorId = DynamicRackDefaults.SeparatorCatalogId,
                SeparatorBlock = Block(catalog, DynamicRackDefaults.SeparatorCatalogId, DynamicRackDefaults.SeparatorView),
                SeparatorMate = Local(catalog, DynamicRackDefaults.SeparatorCatalogId, DynamicRackDefaults.SeparatorMatePoint, DynamicRackDefaults.SeparatorView),
                Levels = Array.Empty<double>()
            };

            var headerModule = system.Modules.FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration != null);
            if (headerModule == null)
            {
                return context;
            }

            var configuration = headerModule.AssociatedFrameConfiguration;
            context.Height = configuration.Height;
            context.PostId = configuration.LeftPost?.PostCatalogId;
            context.PlateId = configuration.LeftBasePlate?.PlateCatalogId;
            context.Paso = configuration.PasoTroquel > 0.0 ? configuration.PasoTroquel : 2.0;

            var troquelSeparador = Local(catalog, context.PostId, DynamicRackDefaults.SeparatorPostPoint, "LATERAL");
            context.TroquelSeparadorX = troquelSeparador.X;
            context.Montaje = Local(catalog, context.PlateId, "MONTAJE_POSTE", "LATERAL");
            context.FinPoste = Local(catalog, context.PostId, "FIN_POSTE", "LATERAL");
            context.PostBlock = Block(catalog, context.PostId, "LATERAL");
            context.PlateBlock = Block(catalog, context.PlateId, "LATERAL");

            context.Levels = SeparatorLevelCalculator.Levels(context.Height, troquelSeparador.Y, context.Paso);
            return context;
        }

        private static HeaderBlockInstance MakeSeparator(HeaderContext context, double moduleStartX, double moduleLength, double level)
        {
            // Anchor the separator's TROQUEL_CABECERA on the previous header's right-post TROQUEL_SEPARADOR
            // (that post is mirrored, so its troquel sits one offset inside the module start); stretch the
            // beam so its far end reaches the next post's troquel.
            var anchorX = moduleStartX - context.TroquelSeparadorX;
            var length = moduleLength + 2.0 * context.TroquelSeparadorX;
            var anchor = new Point2D(anchorX, level);

            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Separator,
                PieceId = context.SeparatorId,
                BlockName = context.SeparatorBlock,
                View = DynamicRackDefaults.SeparatorView,
                RotationRadians = 0.0,
                ConnectionAnchor = anchor,
                Insertion = new Point2D(anchor.X - context.SeparatorMate.X, anchor.Y - context.SeparatorMate.Y)
            };
            instance.DynamicParameters["LONGITUD"] = length;
            return instance;
        }

        private static void AddDerivedPost(ICollection<HeaderBlockInstance> instances, HeaderContext context, double offset)
        {
            if (string.IsNullOrWhiteSpace(context.PostId) || context.Height <= 0.0)
            {
                return;
            }

            var origin = new Point2D(offset, 0.0);

            // Base plate (same as the header's), mated at the post origin.
            instances.Add(new HeaderBlockInstance
            {
                Role = HeaderBlockRole.BasePlate,
                PieceId = context.PlateId,
                BlockName = context.PlateBlock,
                View = "LATERAL",
                ConnectionAnchor = origin,
                Insertion = new Point2D(origin.X - context.Montaje.X, origin.Y - context.Montaje.Y)
            });

            // The post itself, stretched to the header height.
            var post = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = context.PostId,
                BlockName = context.PostBlock,
                View = "LATERAL",
                ConnectionAnchor = origin,
                Insertion = origin
            };
            post.DynamicParameters["LONGITUD"] = context.Height;
            instances.Add(post);

            // Default reinforcement: a second post mated at FIN_POSTE, full height.
            var reinforcement = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = context.PostId,
                BlockName = context.PostBlock,
                View = "LATERAL",
                ConnectionAnchor = new Point2D(origin.X + context.FinPoste.X, origin.Y + context.FinPoste.Y),
                Insertion = new Point2D(origin.X + context.FinPoste.X, origin.Y + context.FinPoste.Y)
            };
            reinforcement.DynamicParameters["LONGITUD"] = context.Height;
            instances.Add(reinforcement);
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

        private sealed class HeaderContext
        {
            public double Height { get; set; }
            public double Paso { get; set; } = 2.0;
            public string PostId { get; set; }
            public string PlateId { get; set; }
            public string PostBlock { get; set; }
            public string PlateBlock { get; set; }
            public Point2D Montaje { get; set; }
            public Point2D FinPoste { get; set; }
            public double TroquelSeparadorX { get; set; }

            public string SeparatorId { get; set; }
            public string SeparatorBlock { get; set; }
            public Point2D SeparatorMate { get; set; }
            public IReadOnlyList<double> Levels { get; set; }
        }
    }
}
