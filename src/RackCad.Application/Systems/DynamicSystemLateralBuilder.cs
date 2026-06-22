using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

        public DynamicSystemPlan Build(DynamicRackSystem system, RackCatalog catalog)
        {
            if (system == null)
            {
                return new DynamicSystemPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            var context = Resolve(system, catalog);
            var loose = new List<HeaderBlockInstance>();

            // Group identical headers so each distinct header becomes one shared block definition; record
            // the run positions (StartX) where each is placed.
            var groups = new Dictionary<string, HeaderGroupBuilder>();
            var order = new List<string>();

            foreach (var module in system.Modules)
            {
                if (module.IsHeader && module.AssociatedFrameConfiguration != null)
                {
                    var signature = Signature(module.AssociatedFrameConfiguration);

                    if (!groups.TryGetValue(signature, out var group))
                    {
                        var parameters = LateralHeaderParametersFactory.FromConfiguration(module.AssociatedFrameConfiguration);
                        var layout = headerBuilder.Build(module.AssociatedFrameConfiguration, parameters, catalog);
                        group = new HeaderGroupBuilder(HeaderName(module.AssociatedFrameConfiguration), layout.Instances.ToList());
                        groups[signature] = group;
                        order.Add(signature);
                    }

                    group.Offsets.Add(module.StartX);
                }
                else if (module.Kind == DynamicRackModuleKind.Separator && module.Length > 0.0 && context.SeparatorBlock != null)
                {
                    foreach (var level in context.Levels)
                    {
                        loose.Add(MakeSeparator(context, module.StartX, module.Length, level));
                    }
                }
            }

            // Derived posts: where two separators are consecutive there is a shared post (header post + plate),
            // reinforced full height by default.
            foreach (var offset in system.GetDerivedPostOffsets())
            {
                AddDerivedPost(loose, context, offset);
            }

            var headers = order.Select(signature => groups[signature].ToGroup()).ToList();
            return new DynamicSystemPlan(headers, loose);
        }

        /// <summary>Signature of the layout-affecting fields, so identical headers share one block definition.</summary>
        private static string Signature(RackFrameConfiguration c)
        {
            var sb = new StringBuilder();
            sb.Append(c.Height.ToString("0.###", CultureInfo.InvariantCulture)).Append('|')
              .Append(c.Depth.ToString("0.###", CultureInfo.InvariantCulture)).Append('|')
              .Append(c.LeftPost?.PostCatalogId).Append('|').Append(c.LeftBasePlate?.PlateCatalogId).Append('|')
              .Append(c.CelosiaStartTroquel).Append('|').Append(c.DiagonalStartOffsetTroqueles).Append('|').Append(c.DiagonalEndOffsetTroqueles);

            foreach (var h in c.Horizontals.OrderBy(item => item.Elevation))
            {
                sb.Append("|H").Append(h.Elevation.ToString("0.###", CultureInfo.InvariantCulture)).Append(',').Append(h.Quantity).Append(',').Append(h.ProfileId);
            }

            foreach (var p in c.BracingPanels.OrderBy(item => item.Number))
            {
                sb.Append("|P").Append(p.Arrangement).Append(',').Append(p.DiagonalDirection).Append(',').Append(p.DiagonalProfileId);
            }

            return sb.ToString();
        }

        private static string HeaderName(RackFrameConfiguration c)
        {
            return string.Format(CultureInfo.InvariantCulture, "Cabecera F{0:0.##} A{1:0.##}", c.Depth, c.Height);
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
            // (that post is mirrored, so its troquel sits one offset inside the module start). Its length is
            // the separation between headers (the module length), as shown in the preview.
            var anchorX = moduleStartX - context.TroquelSeparadorX;
            var length = moduleLength;
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

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(pieceId, connectionPointId, view);
            return entry == null ? new Point2D(0.0, 0.0) : new Point2D(entry.LocalX, entry.LocalY);
        }

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => catalog?.Blocks.FindBlock(pieceId, view)?.BlockName;

        private sealed class HeaderGroupBuilder
        {
            public HeaderGroupBuilder(string name, IReadOnlyList<HeaderBlockInstance> instances)
            {
                Name = name;
                Instances = instances;
                Offsets = new List<double>();
            }

            public string Name { get; }
            public IReadOnlyList<HeaderBlockInstance> Instances { get; }
            public List<double> Offsets { get; }

            public HeaderGroup ToGroup() => new HeaderGroup(Name, Instances, Offsets);
        }

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
