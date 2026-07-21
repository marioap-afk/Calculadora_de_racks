using System;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The design library lists .rackcad.json files, inferring type (cabecera vs dinámico) and display name.</summary>
    public class RackDesignLibraryTests
    {
        [Fact]
        public void List_InfersTypeAndName_AndSkipsForeignFiles()
        {
            var dir = Path.Combine(Path.GetTempPath(), "RackCadLibTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var store = new RackProjectStore();

                // A cabecera WITH a name -> the entry uses the name.
                var header = new RackFrameConfigurationFactory(JsonRackCatalogProvider.FromBaseDirectory().Load())
                    .Build(
                        RackFrameTemplateCatalog.Default,
                        TestCatalogIds.Profiles.Posts.Standard,
                        132.0,
                        42.0);
                header.Name = "Cabecera A";
                store.Save(RackProject.ForSelective(header), Path.Combine(dir, "cab" + RackProjectStore.FileExtension));

                // A (valid) dynamic system -> inferred as Dinamico.
                var dynamic = new DynamicRackSystemBuilder(JsonRackCatalogProvider.FromBaseDirectory().Load())
                    .BuildDefault(new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"), palletsDeep: 2,
                        headerTemplate: RackFrameTemplateCatalog.Default,
                        headerPostCatalogId: TestCatalogIds.Profiles.Posts.Standard,
                        headerHeight: 132.0);
                store.Save(RackProject.ForDynamic(dynamic), Path.Combine(dir, "sistema" + RackProjectStore.FileExtension));

                // A foreign file -> skipped, not thrown.
                File.WriteAllText(Path.Combine(dir, "ruido.txt"), "no soy un diseño");

                var entries = RackDesignLibrary.List(dir);

                Assert.Equal(2, entries.Count);
                Assert.Contains(entries, e => e.Kind == RackSystemKind.Selective && e.Name == "Cabecera A" && e.KindLabel == "Cabecera");
                Assert.Contains(entries, e => e.Kind == RackSystemKind.PalletFlow && e.KindLabel == "Sistema dinámico");
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void List_MissingFolder_ReturnsEmpty()
        {
            var missing = Path.Combine(Path.GetTempPath(), "RackCadLibMissing_" + Guid.NewGuid().ToString("N"));
            Assert.Empty(RackDesignLibrary.List(missing));
        }

        // --- I-08 characterization: canonical Kind + visible labels + List behavior (now registry-driven) ---

        // F4: the entry's Kind is the canonical RackSystemKind, and its visible label comes from the registry descriptor
        // (not a parallel switch). A custom registry that relabels a kind is reflected by the entry, proving the wiring.
        [Fact]
        public void List_ResolvesLabelFromRegistryDescriptor_IncludingACustomRegistry()
        {
            InTempDir(dir =>
            {
                new RackProjectStore().Save(RackProject.ForCama(new FlowBedConfiguration { LaneDepth = 120.0 }),
                    Path.Combine(dir, "cama" + RackProjectStore.FileExtension));

                // Default registry -> the descriptor's exact label.
                var byDefault = Assert.Single(RackDesignLibrary.List(dir));
                Assert.Equal(RackSystemKind.Cama, byDefault.Kind);
                Assert.Equal("Cama de rodamiento", byDefault.KindLabel);

                // A custom registry that relabels Cama -> the entry reflects that label (labels are registry-sourced).
                var custom = new SystemRegistry(new[] { new SystemDescriptor(RackSystemKind.Cama, "Etiqueta Custom") });
                var byCustom = Assert.Single(RackDesignLibrary.List(dir, custom));
                Assert.Equal(RackSystemKind.Cama, byCustom.Kind);
                Assert.Equal("Etiqueta Custom", byCustom.KindLabel);
            });
        }

        // F4: a kind with no registered descriptor is omitted (tolerant, like an unreadable file) and does not break the
        // rest of the listing — the agreed decision for an unregistered kind.
        [Fact]
        public void List_UnregisteredKind_IsOmitted_WithoutBreakingTheList()
        {
            InTempDir(dir =>
            {
                var store = new RackProjectStore();
                store.Save(RackProject.ForLarguero(new LargueroDesign { BeamProfileId = "BEAM_X" }),
                    Path.Combine(dir, "larguero" + RackProjectStore.FileExtension));
                store.Save(RackProject.ForCama(new FlowBedConfiguration { LaneDepth = 120.0 }),
                    Path.Combine(dir, "cama" + RackProjectStore.FileExtension));

                var withoutLarguero = new SystemRegistry(
                    SystemRegistry.Default.Descriptors.Where(d => d.Kind != RackSystemKind.Larguero));

                var entry = Assert.Single(RackDesignLibrary.List(dir, withoutLarguero)); // larguero omitted, cama survives
                Assert.Equal(RackSystemKind.Cama, entry.Kind);
                Assert.Equal("Cama de rodamiento", entry.KindLabel);
            });
        }

        // Name precedence: Header/SelectiveRack/Larguero payload names are used; Cama and Dinamico intentionally fall back
        // to the file name even though their payloads may carry a name.
        [Fact]
        public void List_Larguero_UsesPayloadName_ThenFileNameWhenBlank()
        {
            InTempDir(dir =>
            {
                var store = new RackProjectStore();
                store.Save(RackProject.ForLarguero(new LargueroDesign { Name = "L1", BeamProfileId = "BEAM_X" }),
                    Path.Combine(dir, "archivoL" + RackProjectStore.FileExtension));
                store.Save(RackProject.ForLarguero(new LargueroDesign { Name = null, BeamProfileId = "BEAM_X" }),
                    Path.Combine(dir, "sinNombre" + RackProjectStore.FileExtension));

                var entries = RackDesignLibrary.List(dir);

                var named = Assert.Single(entries, e => e.Path.EndsWith("archivoL" + RackProjectStore.FileExtension));
                Assert.Equal(RackSystemKind.Larguero, named.Kind);
                Assert.Equal("Larguero", named.KindLabel);
                Assert.Equal("L1", named.Name);

                var unnamed = Assert.Single(entries, e => e.Path.EndsWith("sinNombre" + RackProjectStore.FileExtension));
                Assert.Equal("sinNombre", unnamed.Name);
            });
        }

        [Fact]
        public void List_Cama_UsesFileNameNotPayload()
        {
            InTempDir(dir =>
            {
                new RackProjectStore().Save(RackProject.ForCama(new FlowBedConfiguration { LaneDepth = 120.0 }),
                    Path.Combine(dir, "miCama" + RackProjectStore.FileExtension));

                var entry = Assert.Single(RackDesignLibrary.List(dir));
                Assert.Equal(RackSystemKind.Cama, entry.Kind);
                Assert.Equal("Cama de rodamiento", entry.KindLabel);
                Assert.Equal("miCama", entry.Name); // the FlowBed payload name is not consulted
            });
        }

        [Fact]
        public void List_SelectiveRack_UsesPayloadName()
        {
            InTempDir(dir =>
            {
                var design = new SelectivePalletDesign { PostId = "P", PalletDepth = 48.0 };
                design.Bays.Add(new SelectiveBayDesign());
                new RackProjectStore().Save(
                    RackProject.ForSelectiveRack(SelectivePalletDesignDocument.From(design, "id-9", "Rack Nueve")),
                    Path.Combine(dir, "archivoS" + RackProjectStore.FileExtension));

                var entry = Assert.Single(RackDesignLibrary.List(dir));
                Assert.Equal(RackSystemKind.SelectiveRack, entry.Kind);
                Assert.Equal("Selectivo", entry.KindLabel);
                Assert.Equal("Rack Nueve", entry.Name);
            });
        }

        [Fact]
        public void List_OrdersByModifiedDescending()
        {
            InTempDir(dir =>
            {
                var store = new RackProjectStore();
                var older = Path.Combine(dir, "viejo" + RackProjectStore.FileExtension);
                var newer = Path.Combine(dir, "nuevo" + RackProjectStore.FileExtension);
                store.Save(RackProject.ForLarguero(new LargueroDesign { BeamProfileId = "BEAM_X" }), older);
                store.Save(RackProject.ForLarguero(new LargueroDesign { BeamProfileId = "BEAM_X" }), newer);

                var now = DateTime.UtcNow;
                File.SetLastWriteTimeUtc(older, now.AddHours(-2));
                File.SetLastWriteTimeUtc(newer, now);

                var entries = RackDesignLibrary.List(dir);

                Assert.Equal(2, entries.Count);
                Assert.EndsWith("nuevo" + RackProjectStore.FileExtension, entries[0].Path);
                Assert.EndsWith("viejo" + RackProjectStore.FileExtension, entries[1].Path);
            });
        }

        [Fact]
        public void List_SkipsUnreadableDesignFileIndividually_WithoutFailingTheList()
        {
            InTempDir(dir =>
            {
                new RackProjectStore().Save(RackProject.ForLarguero(new LargueroDesign { BeamProfileId = "BEAM_X" }),
                    Path.Combine(dir, "bueno" + RackProjectStore.FileExtension));
                // A .rackcad.json that fails to load must be skipped individually, not fail the whole listing.
                File.WriteAllText(Path.Combine(dir, "malo" + RackProjectStore.FileExtension), "{ esto no es json valido");

                var entries = RackDesignLibrary.List(dir);

                var entry = Assert.Single(entries);
                Assert.EndsWith("bueno" + RackProjectStore.FileExtension, entry.Path);
            });
        }

        private static void InTempDir(Action<string> body)
        {
            var dir = Path.Combine(Path.GetTempPath(), "RackCadLibChar_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                body(dir);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
