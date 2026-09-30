using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-60: the automatic logical name of a NEW rack (Freeze docs/initiatives/I-60-freeze.md, obligations N-1..N-5, N-8). Pure authority:
    /// «Prefijo» N with N one above the largest N of the names in the drawing that follow the exact pattern of the family.
    /// </summary>
    public class I60RackLogicalNameTests
    {
        // ================================================================ N-1: numbering

        [Fact]
        public void I60_N1_AnEmptyDrawingStartsAtOne()
        {
            Assert.Equal("Selectivo 1", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, Array.Empty<string>()));
            Assert.Equal("Selectivo 1", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, null));
        }

        [Fact]
        public void I60_N1_TheNextNumberFollowsTheLargestAndHolesAreNotFilled()
        {
            Assert.Equal("Selectivo 2", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { "Selectivo 1" }));
            Assert.Equal("Selectivo 8", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { "Selectivo 1", "Selectivo 2", "Selectivo 7" }));
            Assert.Equal("Selectivo 8", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { "Selectivo 7", "Selectivo 7" }));
        }

        // ================================================================ N-2: case

        [Fact]
        public void I60_N2_CaseIsIgnoredAndOuterSpacesToo()
        {
            Assert.Equal("Selectivo 6", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { "selectivo 2", "SELECTIVO 5", "  Selectivo 3  " }));
            Assert.Equal("Dinámico 4", RackLogicalNameAllocator.Next(RackSystemKind.PalletFlow, new[] { "DINÁMICO 3" }));
        }

        // ================================================================ N-3: custom names

        [Theory]
        [InlineData("Rack A")]
        [InlineData("Selectivo")]
        [InlineData("Selectivo A")]
        [InlineData("Selectivo 03")]
        [InlineData("Selectivo  2")]
        [InlineData("Selectivo 0")]
        [InlineData("Selectivo -4")]
        [InlineData("Selectivo 2b")]
        [InlineData("Selectivo 1 - copia")]
        [InlineData("Mi Selectivo 9")]
        [InlineData("")]
        [InlineData(null)]
        public void I60_N3_CustomNamesDoNotConsumeANumber(string name)
        {
            Assert.Equal("Selectivo 1", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { name }));
        }

        [Fact]
        public void I60_N3_ThePrefixKeepsItsAccents()
        {
            Assert.Equal("Dinámico 1", RackLogicalNameAllocator.Next(RackSystemKind.PalletFlow, new[] { "Dinamico 3" }));
        }

        [Fact]
        public void I60_N3_AnyRackWhoseNameFollowsThePatternConsumesItsNumber()
        {
            // The pattern is read on the text of every logical name in the drawing, whatever the kind of the rack that carries it.
            var names = new[] { "Selectivo 4", "Dinámico 9", "Push Back 2", "Cantilever 1", "Cabecera 5", "Cama 3", "Rack B3" };

            Assert.Equal("Selectivo 5", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, names));
            Assert.Equal("Dinámico 10", RackLogicalNameAllocator.Next(RackSystemKind.PalletFlow, names));
            Assert.Equal("Push Back 3", RackLogicalNameAllocator.Next(RackSystemKind.PushBack, names));
            Assert.Equal("Cantilever 2", RackLogicalNameAllocator.Next(RackSystemKind.Cantilever, names));
            Assert.Equal("Cabecera 6", RackLogicalNameAllocator.Next(RackSystemKind.Selective, names));
            Assert.Equal("Cama 4", RackLogicalNameAllocator.Next(RackSystemKind.Cama, names));
        }

        [Fact]
        public void I60_N1_NHasNoSizeLimit()
        {
            Assert.Equal("Selectivo 2147483648", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { "Selectivo 2147483647" }));
            Assert.Equal("Selectivo 10000000000000000000000000",
                RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { "Selectivo 9999999999999999999999999", "Selectivo 12" }));
            Assert.Equal("Selectivo 100", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { "Selectivo 99", "Selectivo 9" }));
        }

        [Fact]
        public void I60_N3_TheSeparatorAndThePrefixAreExactCharacters()
        {
            var nbsp = "Selectivo" + (char)0x00A0 + "2";
            var tab = "Selectivo" + (char)0x0009 + "2";
            var zeroWidth = "Selectivo " + (char)0x200B + "2";
            var nfd = "Dina" + (char)0x0301 + "mico 3";

            Assert.Equal("Selectivo 1", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { nbsp, tab, zeroWidth }));
            Assert.Equal("Dinámico 1", RackLogicalNameAllocator.Next(RackSystemKind.PalletFlow, new[] { nfd }));
        }

        // ================================================================ N-4: families

        [Fact]
        public void I60_N4_EveryFamilyHasItsOwnIndependentSequence()
        {
            Assert.Equal("Selectivo 1", RackLogicalNameAllocator.Next(RackSystemKind.SelectiveRack, new[] { "Dinámico 3", "Push Back 2" }));
            Assert.Equal("Push Back 1", RackLogicalNameAllocator.Next(RackSystemKind.PushBack, new[] { "Selectivo 3" }));
        }

        [Fact]
        public void I60_N4_ThePrefixesAreTheBomLabelsOfThePlugin()
        {
            foreach (var (kind, handler) in new[]
            {
                (RackSystemKind.SelectiveRack, "SelectiveKindHandler.cs"),
                (RackSystemKind.PalletFlow, "DynamicKindHandler.cs"),
                (RackSystemKind.PushBack, "PushBackKindHandler.cs"),
                (RackSystemKind.Cantilever, "CantileverKindHandler.cs"),
                (RackSystemKind.Selective, "CabeceraKindHandler.cs"),
                (RackSystemKind.Cama, "CamaKindHandler.cs"),
            })
            {
                var source = File.ReadAllText(Path.Combine(Root(), "src", "RackCad.Plugin", "KindHandlers", handler));
                var label = Regex.Match(source, "BomLabel => \"([^\"]+)\";").Groups[1].Value;

                Assert.Equal(label, RackLogicalNameAllocator.PrefixOf(kind));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => RackLogicalNameAllocator.PrefixOf(RackSystemKind.Larguero));
        }

        // ================================================================ N-5: when a new rack is unassigned

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        [InlineData("Rack A", false)]
        [InlineData("Estandar (3 paneles)", false)]
        public void I60_N5_ANewRackIsUnassignedOnlyWithABlankName(string name, bool unassigned)
        {
            Assert.Equal(unassigned, RackLogicalNameAllocator.IsUnassigned(RackSystemKind.SelectiveRack, name));
        }

        [Theory]
        [InlineData("Estandar (3 paneles)", true)]
        [InlineData("  estandar (3 PANELES) ", true)]
        [InlineData("Compacta (2 paneles)", true)]
        [InlineData("Alta (4 paneles, X)", true)]
        [InlineData("Marco del pasillo 4", false)]
        [InlineData("", true)]
        public void I60_N5_ANewCabeceraCarryingItsTemplateNameIsUnassigned(string name, bool unassigned)
        {
            Assert.Equal(unassigned, RackLogicalNameAllocator.IsUnassigned(RackSystemKind.Selective, name));
        }

        [Fact]
        public void I60_N5_ANameTheUserWroteIsKeptAsItIs()
        {
            Assert.Equal("Pasillo A", RackLogicalNameAllocator.ForNewRack(RackSystemKind.SelectiveRack, "Pasillo A", new[] { "Selectivo 3" }));
            Assert.Equal("Selectivo 4", RackLogicalNameAllocator.ForNewRack(RackSystemKind.SelectiveRack, "  ", new[] { "Selectivo 3" }));
            Assert.Equal("Cabecera 1", RackLogicalNameAllocator.ForNewRack(RackSystemKind.Selective, "Estandar (3 paneles)", Array.Empty<string>()));
        }

        // ================================================================ N-8: legacy and persistence

        [Fact]
        public void I60_N8_ALegacyUnnamedRackIsStillSinNombre()
        {
            var legacy = new RackEmbedDocument { Id = "L", Kind = "selective", Name = "", View = "planta", Section = -1, Design = "{}" };

            Assert.Equal("(sin nombre)", RackListBuilder.Build(new[] { legacy }).Single().Name);
        }

        [Fact]
        public void I60_N8_TheAutomaticNameSurvivesSaveAndReopen()
        {
            var name = RackLogicalNameAllocator.Next(RackSystemKind.PalletFlow, new[] { "Dinámico 1" });
            var envelope = RackEmbedComposer.Compose(null, RackEmbedDocument.KindDynamic, "R", name, "lateral", 0, "{}");

            var back = new RackEmbedStore().Deserialize(new RackEmbedStore().Serialize(envelope));

            Assert.Equal("Dinámico 2", back.Name);
            Assert.Equal("R", back.Id);
        }

        private static string Root()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return dir.FullName;
        }
    }
}
