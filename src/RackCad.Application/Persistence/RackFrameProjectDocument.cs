using System.Collections.Generic;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Stable, serialization-friendly snapshot of a configuration's source of truth
    /// (metadata, posts, plates, horizontals, panels). Derived data (Members, exceptions)
    /// is intentionally omitted and regenerated on load. Decoupling the file schema from the
    /// domain model keeps saved projects readable across model refactors.
    /// </summary>
    public sealed class RackFrameProjectDocument
    {
        public string SchemaVersion { get; set; } = "1.0";
        public string Name { get; set; }
        public string Units { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }

        // Nullable so legacy projects without these fields fall back to the built-in defaults.
        public int? CelosiaStartTroquel { get; set; }
        public int? DiagonalStartOffsetTroqueles { get; set; }
        public int? DiagonalEndOffsetTroqueles { get; set; }

        public string StandardBaselineId { get; set; }
        public string StandardBaselineVersion { get; set; }
        public PostDocument LeftPost { get; set; }
        public PostDocument RightPost { get; set; }
        public PlateDocument LeftBasePlate { get; set; }
        public PlateDocument RightBasePlate { get; set; }
        public List<HorizontalDocument> Horizontals { get; set; } = new List<HorizontalDocument>();
        public List<PanelDocument> Panels { get; set; } = new List<PanelDocument>();

        public static RackFrameProjectDocument FromConfiguration(RackFrameConfiguration configuration)
        {
            var document = new RackFrameProjectDocument
            {
                Name = configuration.Name,
                Units = configuration.Units,
                Height = configuration.Height,
                Depth = configuration.Depth,
                CelosiaStartTroquel = configuration.CelosiaStartTroquel,
                DiagonalStartOffsetTroqueles = configuration.DiagonalStartOffsetTroqueles,
                DiagonalEndOffsetTroqueles = configuration.DiagonalEndOffsetTroqueles,
                StandardBaselineId = configuration.StandardBaselineId,
                StandardBaselineVersion = configuration.StandardBaselineVersion,
                LeftPost = PostDocument.From(configuration.LeftPost),
                RightPost = PostDocument.From(configuration.RightPost),
                LeftBasePlate = PlateDocument.From(configuration.LeftBasePlate),
                RightBasePlate = PlateDocument.From(configuration.RightBasePlate)
            };

            foreach (var horizontal in configuration.Horizontals)
            {
                document.Horizontals.Add(HorizontalDocument.From(horizontal));
            }

            foreach (var panel in configuration.BracingPanels)
            {
                document.Panels.Add(PanelDocument.From(panel));
            }

            return document;
        }

        public RackFrameConfiguration ToConfiguration()
        {
            var configuration = new RackFrameConfiguration
            {
                Name = Name,
                Units = string.IsNullOrWhiteSpace(Units) ? "in" : Units,
                Height = Height,
                Depth = Depth,
                CelosiaStartTroquel = CelosiaStartTroquel ?? 3,
                DiagonalStartOffsetTroqueles = DiagonalStartOffsetTroqueles ?? 2,
                DiagonalEndOffsetTroqueles = DiagonalEndOffsetTroqueles ?? 2,
                StandardBaselineId = StandardBaselineId,
                StandardBaselineVersion = StandardBaselineVersion,
                LeftPost = LeftPost?.ToDomain(PostSide.Left),
                RightPost = RightPost?.ToDomain(PostSide.Right),
                LeftBasePlate = LeftBasePlate?.ToDomain(PostSide.Left),
                RightBasePlate = RightBasePlate?.ToDomain(PostSide.Right)
            };

            foreach (var horizontal in Horizontals ?? new List<HorizontalDocument>())
            {
                configuration.Horizontals.Add(horizontal.ToDomain());
            }

            foreach (var panel in Panels ?? new List<PanelDocument>())
            {
                configuration.BracingPanels.Add(panel.ToDomain());
            }

            return configuration;
        }
    }

    public sealed class PostDocument
    {
        public string PostCatalogId { get; set; }
        public string Description { get; set; }
        public bool HasReinforcement { get; set; }
        public string ReinforcementCatalogId { get; set; }

        public static PostDocument From(PostAssembly post)
        {
            return post == null ? null : new PostDocument
            {
                PostCatalogId = post.PostCatalogId,
                Description = post.Description,
                HasReinforcement = post.HasReinforcement,
                ReinforcementCatalogId = post.ReinforcementCatalogId
            };
        }

        public PostAssembly ToDomain(PostSide side)
        {
            return new PostAssembly
            {
                Side = side,
                PostCatalogId = PostCatalogId,
                Description = Description,
                HasReinforcement = HasReinforcement,
                ReinforcementCatalogId = ReinforcementCatalogId
            };
        }
    }

    public sealed class PlateDocument
    {
        public string PlateCatalogId { get; set; }
        public string Description { get; set; }
        public string ConnectionPointId { get; set; }

        public static PlateDocument From(BasePlatePlacement plate)
        {
            return plate == null ? null : new PlateDocument
            {
                PlateCatalogId = plate.PlateCatalogId,
                Description = plate.Description,
                ConnectionPointId = plate.ConnectionPointId
            };
        }

        public BasePlatePlacement ToDomain(PostSide side)
        {
            return new BasePlatePlacement
            {
                PostSide = side,
                PlateCatalogId = PlateCatalogId,
                Description = Description,
                ConnectionPointId = ConnectionPointId
            };
        }
    }

    public sealed class HorizontalDocument
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public double Elevation { get; set; }
        public string ProfileId { get; set; }
        public int Quantity { get; set; }
        public FrameSide MountingFace { get; set; }
        public FrameComponentState State { get; set; }
        public string Notes { get; set; }
        public bool IsStandard { get; set; }

        public static HorizontalDocument From(FrameHorizontal horizontal)
        {
            return new HorizontalDocument
            {
                Id = horizontal.Id,
                Number = horizontal.Number,
                Elevation = horizontal.Elevation,
                ProfileId = horizontal.ProfileId,
                Quantity = horizontal.Quantity,
                MountingFace = horizontal.MountingFace,
                State = horizontal.State,
                Notes = horizontal.Notes,
                IsStandard = horizontal.IsStandard
            };
        }

        public FrameHorizontal ToDomain()
        {
            return new FrameHorizontal
            {
                Id = Id,
                Number = Number,
                Elevation = Elevation,
                ProfileId = ProfileId,
                Quantity = Quantity,
                MountingFace = MountingFace,
                State = State,
                Notes = Notes,
                IsStandard = IsStandard
            };
        }
    }

    public sealed class PanelDocument
    {
        public string PanelId { get; set; }
        public int Number { get; set; }
        public string LowerHorizontalId { get; set; }
        public string UpperHorizontalId { get; set; }
        public BracingPattern Arrangement { get; set; }
        public FrameSide MountingFace { get; set; }
        public string DiagonalProfileId { get; set; }
        public DiagonalDirection DiagonalDirection { get; set; }
        public string StartConnectionPointId { get; set; }
        public string EndConnectionPointId { get; set; }
        public bool IsStandard { get; set; }
        public bool IsException { get; set; }

        public static PanelDocument From(BracingPanel panel)
        {
            return new PanelDocument
            {
                PanelId = panel.PanelId,
                Number = panel.Number,
                LowerHorizontalId = panel.LowerHorizontalId,
                UpperHorizontalId = panel.UpperHorizontalId,
                Arrangement = panel.Arrangement,
                MountingFace = panel.MountingFace,
                DiagonalProfileId = panel.DiagonalProfileId,
                DiagonalDirection = panel.DiagonalDirection,
                StartConnectionPointId = panel.StartConnectionPointId,
                EndConnectionPointId = panel.EndConnectionPointId,
                IsStandard = panel.IsStandard,
                IsException = panel.IsException
            };
        }

        public BracingPanel ToDomain()
        {
            return new BracingPanel
            {
                PanelId = PanelId,
                Number = Number,
                LowerHorizontalId = LowerHorizontalId,
                UpperHorizontalId = UpperHorizontalId,
                Arrangement = Arrangement,
                MountingFace = MountingFace,
                DiagonalProfileId = DiagonalProfileId,
                DiagonalDirection = DiagonalDirection,
                StartConnectionPointId = StartConnectionPointId,
                EndConnectionPointId = EndConnectionPointId,
                IsStandard = IsStandard,
                IsException = IsException
            };
        }
    }
}
