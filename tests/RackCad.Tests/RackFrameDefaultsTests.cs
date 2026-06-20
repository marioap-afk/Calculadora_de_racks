using RackCad.Application.RackFrames;
using RackCad.Catalogs.RackFrames;
using RackCad.Domain.RackFrames;
using Shouldly;

namespace RackCad.Tests;

public sealed class RackFrameDefaultsTests
{
    [Fact]
    public void CreateDefaultBuildsStandardRackFrame()
    {
        var service = new HardcodedStandardRackFrameService();

        var configuration = service.CreateDefault();

        configuration.Name.ShouldBe("Cabecera estandar temporal");
        configuration.Height.ShouldBe(132.0);
        configuration.Depth.ShouldBe(42.0);
        configuration.Horizontals.Count.ShouldBe(4);
        configuration.BracingPanels.Count.ShouldBe(3);
        configuration.Horizontals.Select(horizontal => horizontal.Id)
            .ShouldBe(["H1", "H2", "H3", "H4"]);
        configuration.BracingPanels.Select(panel => panel.PanelId)
            .ShouldBe(["P1", "P2", "P3"]);
    }

    [Fact]
    public void RefreshPhysicalModelBuildsHorizontalsAndDiagonals()
    {
        var configuration = new HardcodedStandardRackFrameService().CreateDefault();
        var builder = new BracingPanelMemberBuilder();

        builder.RefreshPhysicalModel(configuration);

        configuration.Members.Count.ShouldBe(7);
        configuration.Members.Count(member => member.MemberType == FrameMemberType.LowerHorizontal).ShouldBe(1);
        configuration.Members.Count(member => member.MemberType == FrameMemberType.IntermediateHorizontal).ShouldBe(2);
        configuration.Members.Count(member => member.MemberType == FrameMemberType.UpperHorizontal).ShouldBe(1);
        configuration.Members.Count(member => member.MemberType == FrameMemberType.DiagonalBrace).ShouldBe(3);
        configuration.BracingPanels.ShouldAllBe(panel => panel.Members.Count == 1);
    }

    [Fact]
    public async Task SampleCatalogLoadsAndValidatesAgainstSchema()
    {
        var repositoryRoot = FindRepositoryRoot();
        var catalogPath = Path.Combine(repositoryRoot, "assets", "catalogs", "rack-frame-catalog.sample.json");
        var schemaPath = Path.Combine(repositoryRoot, "assets", "catalogs", "rack-frame-catalog.schema.json");
        var service = new JsonRackFrameCatalogService();

        var result = await service.ValidateAsync(catalogPath, schemaPath);
        var catalog = await service.LoadAsync(catalogPath);

        result.Errors.ShouldBeEmpty();
        catalog.CatalogId.ShouldBe("RACK-FRAME-CATALOG-SAMPLE");
        catalog.PostProfiles.Select(profile => profile.Id).ShouldContain("POSTE_OMEGA_3X3");
        catalog.ConnectionPoints.Select(point => point.Id).ShouldContain("TroquelCelosia_01");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
