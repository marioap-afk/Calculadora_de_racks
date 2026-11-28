using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Larguero;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 A-14, line D, quinta tanda, part «lg» (LG-01 to LG-03 of the Owner): the pure rules of «Nuevo › Larguero», a draft in construction.
    /// The sections of a larguero come from two catalogs (the largueros of RackCad in secciones.csv and the AISC sections), each is called by its
    /// type («Escalón», «Tubo», «Canal…», not «Larguero escalón…»), the list is filtered by catalog, by type and by text and goes in natural
    /// order, and the summary uses the one recipe of the larguero (a profile and 2 ménsulas) and says what is missing. Over the real catalogs
    /// that ship with the tests, and over small catalogs made here for the edge cases.
    /// </summary>
    public class LargueroSectionsTests
    {
        private static RackCatalog Rack => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static StructuralSectionCatalog Aisc => new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static BeamProfileCatalogEntry Beam(string id, string name, string family = "Larguero", string peraltes = "3;4", string mensula = "M1")
            => new BeamProfileCatalogEntry { Id = id, DisplayName = name, Family = family, Peraltes = peraltes, Mensula = mensula };

        private static RackCatalog Small(params BeamProfileCatalogEntry[] beams) => new RackCatalog
        {
            BeamProfiles = beams,
            Mensulas = new List<MensulaCatalogEntry>
            {
                new MensulaCatalogEntry { Id = "M1", DisplayName = "Ménsula uno", Height = 6, Gauge = "10" },
                new MensulaCatalogEntry { Id = "M2", DisplayName = "Ménsula dos", Height = 8 },
            },
        };

        // ------------------------------------------------------------------ LG-01: by its type

        [Fact]
        public void TheLarguerosOfRackCadAreSectionsCalledByTheirType()
        {
            var catalog = LargueroSectionCatalog.Build(Rack, null);
            var rackcad = catalog.All.Where(o => o.Source == LargueroSectionSource.RackCad).ToList();

            Assert.Equal(Rack.BeamProfiles.Count, rackcad.Count);
            var byId = rackcad.ToDictionary(o => o.Id, o => (o.Name, o.TypeLabel));
            Assert.Equal(("Escalón calibre 14", "Escalón"), byId["LARGUERO_ESCALON_CAL14_3_REMACHES"]);
            Assert.Equal(("Escalón con ángulo", "Escalón"), byId["LARGUERO_ESCALON_CON_ANGULO_3_REMACHES"]);
            Assert.Equal(("Escalón de ajuste infinito", "Escalón"), byId["LARGUERO_ESCALON_INFINITO"]);
            Assert.Equal(("Escalón troquel redondo", "Escalón"), byId["LARGUERO_ESCALON_TROQUEL_REDONDO"]); // «escalon» in the catalog, «Escalón» here
            Assert.Equal(("Tubo 1.9\"", "Tubo redondo"), byId["LARGUERO_TUBO_1_9_3_REMACHES"]); // review, point 4: round, not a PTR
            Assert.Equal(("In n out de canal de 6\"", "Canal"), byId["LARGUERO_IN_OUT_C6"]); // cold-formed: not the CPS of AISC

            // No name says «Larguero …» or «… con ménsula …»: the ménsula is chosen apart (LG-01).
            Assert.All(rackcad, o => Assert.DoesNotContain("Larguero", o.Name, StringComparison.OrdinalIgnoreCase));
            Assert.All(rackcad, o => Assert.DoesNotContain("ménsula", o.Name, StringComparison.OrdinalIgnoreCase));
            // What the catalog calls the row is kept beside it, with its id.
            Assert.Equal("Larguero escalón calibre 14 con ménsula 3 remaches", rackcad.Single(o => o.Id == "LARGUERO_ESCALON_CAL14_3_REMACHES").SourceLabel);
        }

        [Fact]
        public void TheColumnFamilyOfTheCatalogNamesTheTypeWhenItSaysSomethingOtherThanLarguero()
        {
            var catalog = LargueroSectionCatalog.Build(Small(
                Beam("A", "Larguero canal CPS 4\" con ménsula 3 remaches", family: "Canal"),
                Beam("B", "Larguero Z 5", family: "Perfil Z"),
                Beam("C", "Larguero raro", family: "")), null);

            Assert.Equal(("Canal CPS 4\"", "Canal"), Pair(catalog.Find(LargueroSectionSource.RackCad, "A")));
            Assert.Equal(("Z 5", "Perfil Z"), Pair(catalog.Find(LargueroSectionSource.RackCad, "B")));
            Assert.Equal(("Raro", "Otro"), Pair(catalog.Find(LargueroSectionSource.RackCad, "C")));
            // A type of the catalog goes after the known ones and before «Otro».
            Assert.Equal(new[] { "Canal", "Perfil Z", "Otro" }, catalog.TypesOf(null).Select(t => t.Label).ToArray());
        }

        [Fact]
        public void TwoLarguerosWithTheSameSectionCarryTheirIdSoNoTwoRowsReadTheSame()
        {
            var catalog = LargueroSectionCatalog.Build(Small(
                Beam("E1", "Larguero escalón calibre 14 con ménsula 3 remaches"),
                Beam("E2", "Larguero escalón calibre 14 con ménsula troquel redondo", mensula: "M2")), null);

            Assert.Equal(new[] { "Escalón calibre 14 (E1)", "Escalón calibre 14 (E2)" }, catalog.All.Select(o => o.Name).ToArray());
        }

        [Fact]
        public void AnAiscSectionIsCalledByItsDesignationAndItsTypeComesFromItsFamily()
        {
            var catalog = LargueroSectionCatalog.Build(null, Aisc);
            var aisc = catalog.All.Where(o => o.Source == LargueroSectionSource.Aisc).ToList();

            Assert.Equal(Aisc.Enabled.Count, aisc.Count);
            var types = aisc.GroupBy(o => o.TypeLabel).ToDictionary(g => g.Key, g => g.Count());
            // The names of the Mexican market (review, point 4): IPR, PTR and CPS, as IPS.
            Assert.Equal(Aisc.ByFamily(StructuralSectionFamily.W).Count, types["Viga W (IPR)"]);
            Assert.Equal(Aisc.ByFamily(StructuralSectionFamily.HssRectangular).Count, types["Tubo rectangular (PTR)"]);
            Assert.Equal(Aisc.ByFamily(StructuralSectionFamily.Channel).Count, types["Canal CPS"]);
            Assert.Equal(Aisc.ByFamily(StructuralSectionFamily.Angle).Count, types["Ángulo"]);
            Assert.Equal(Aisc.ByFamily(StructuralSectionFamily.S).Count, types["Viga S (IPS)"]);

            var channel = aisc.Single(o => o.Name == "C10X15.3");
            Assert.Equal(10.0, channel.Depth);
            Assert.False(channel.HasPeralteChoice);
            Assert.Null(channel.DefaultMensulaId);
            Assert.Equal("15.3 lb/ft (22.8 kg/m)", channel.WeightText);
        }

        // ------------------------------------------------------------------ LG-02: the filter by catalog and type, and the order

        [Fact]
        public void TheListGoesByTypeThenCatalogThenNaturalOrderAndTheTypesUniteTheTwoCatalogs()
        {
            var catalog = LargueroSectionCatalog.Build(Rack, Aisc);

            Assert.Equal(new[] { "Escalón", "Tubo redondo", "Tubo rectangular (PTR)", "Canal", "Canal CPS", "Ángulo", "Viga W (IPR)", "Viga S (IPS)" },
                catalog.TypesOf(null).Select(t => t.Label).ToArray());
            Assert.Equal(new[] { "Escalón", "Tubo redondo", "Canal" }, catalog.TypesOf(LargueroSectionSource.RackCad).Select(t => t.Label).ToArray());
            Assert.Equal(new[] { "Tubo rectangular (PTR)", "Canal CPS", "Ángulo", "Viga W (IPR)", "Viga S (IPS)" },
                catalog.TypesOf(LargueroSectionSource.Aisc).Select(t => t.Label).ToArray());

            // Review, point 4: the round tube of RackCad and the PTR are two types, and so are the cold-formed channel and the CPS.
            Assert.Equal(new[] { "LARGUERO_TUBO_1_9_3_REMACHES" }, catalog.Filter(null, "Tubo redondo", null).Select(o => o.Id).ToArray());
            Assert.All(catalog.Filter(null, "Tubo rectangular (PTR)", null), o => Assert.Equal(LargueroSectionSource.Aisc, o.Source));
            Assert.Equal(new[] { "LARGUERO_IN_OUT_C6" }, catalog.Filter(null, "Canal", null).Select(o => o.Id).ToArray());
            Assert.Equal(Aisc.ByFamily(StructuralSectionFamily.Channel).Count, catalog.Filter(null, "Canal CPS", null).Count);

            // The default is the first larguero in the order of the catalog (BeamProfiles[0]).
            Assert.Equal("LARGUERO_ESCALON_CAL14_3_REMACHES", catalog.Default.Id);
            Assert.Equal(catalog.All.Count, catalog.CountOf(LargueroSectionSource.RackCad) + catalog.CountOf(LargueroSectionSource.Aisc));
        }

        [Theory]
        [InlineData("W10X12", "W10X100")]
        [InlineData("W10X12", "W10X112")]
        [InlineData("L10X10X1-1/8", "L10X10X1-1/4")]
        [InlineData("HSS4X4X1/8", "HSS4X4X1/4")]
        [InlineData("C10X15.3", "C10X20")]
        [InlineData("Escalón calibre 14", "Escalón con ángulo")]
        public void TheNaturalOrderComparesTheNumbersOfANameAsNumbers(string first, string second)
        {
            Assert.True(LargueroNaturalOrder.Instance.Compare(first, second) < 0, first + " antes que " + second);
            Assert.True(LargueroNaturalOrder.Instance.Compare(second, first) > 0, second + " después de " + first);
        }

        [Fact]
        public void TheAiscListOfAFamilyIsInNaturalOrder()
        {
            var w10 = LargueroSectionCatalog.Build(null, Aisc).Filter(LargueroSectionSource.Aisc, "Viga W (IPR)", "W10X").Select(o => o.Name).ToList();

            Assert.True(w10.IndexOf("W10X12") < w10.IndexOf("W10X100"));
            Assert.True(w10.IndexOf("W10X12") < w10.IndexOf("W10X112"));
        }

        [Fact]
        public void TheFilterIsByCatalogByTypeAndByTextWithoutCaseAccentsOrSeparators()
        {
            var catalog = LargueroSectionCatalog.Build(Rack, Aisc);

            Assert.Equal(catalog.CountOf(LargueroSectionSource.RackCad), catalog.Filter(LargueroSectionSource.RackCad, null, null).Count);
            Assert.Equal(6, catalog.Filter(LargueroSectionSource.RackCad, "Escalón", "").Count);
            Assert.Equal(new[] { "LARGUERO_ESCALON_INFINITO" }, catalog.Filter(null, null, "ajuste INFINITO").Select(o => o.Id).ToArray());
            Assert.Equal(6, catalog.Filter(LargueroSectionSource.RackCad, null, "escalon").Count); // no accent typed
            Assert.Contains(catalog.Filter(LargueroSectionSource.Aisc, "Tubo rectangular (PTR)", "hss 4x4"), o => o.Name.StartsWith("HSS4X4", StringComparison.Ordinal));
            Assert.Equal(new[] { "C10X15.3" }, catalog.Filter(LargueroSectionSource.Aisc, null, "c10x15.3").Select(o => o.Name).ToArray());
            // The name in the catalog and the id find the row too.
            Assert.Single(catalog.Filter(LargueroSectionSource.RackCad, null, "in n out"));
            Assert.Single(catalog.Filter(LargueroSectionSource.RackCad, null, "LARGUERO_TUBO"));
            Assert.Empty(catalog.Filter(LargueroSectionSource.RackCad, "Ángulo", null));
        }

        // Review of part «lg», point 4: the search sees the type and its aliases, so «tubo» finds the PTR, and CPS, PTR, IPR and IPS find
        // their families.
        [Fact]
        public void TheSearchSeesTheTypeAndTheNamesOfTheMarket()
        {
            var catalog = LargueroSectionCatalog.Build(Rack, Aisc);
            int Count(StructuralSectionFamily family) => Aisc.ByFamily(family).Count;

            var tubo = catalog.Filter(null, null, "tubo");
            Assert.Equal(1 + Count(StructuralSectionFamily.HssRectangular), tubo.Count); // the round tube of RackCad and every PTR
            Assert.Equal(Count(StructuralSectionFamily.HssRectangular), catalog.Filter(null, null, "PTR").Count);
            Assert.Equal(Count(StructuralSectionFamily.HssRectangular), catalog.Filter(null, null, "HSS").Count);
            Assert.Equal(Count(StructuralSectionFamily.Channel), catalog.Filter(null, null, "cps").Count);
            Assert.Equal(Count(StructuralSectionFamily.W), catalog.Filter(null, null, "IPR").Count);
            Assert.Equal(Count(StructuralSectionFamily.S), catalog.Filter(null, null, "ips").Count);
            Assert.Equal(1 + Count(StructuralSectionFamily.Channel), catalog.Filter(null, null, "canal").Count); // the in n out and every CPS
        }

        [Fact]
        public void TheWordsOfALargueroOfRackCadSayItsTypeWhenTheCatalogDoesNot()
        {
            var catalog = LargueroSectionCatalog.Build(Small(
                Beam("A", "Larguero canal CPS 3 con ménsula 3 remaches"),
                Beam("B", "Larguero tubo PTR 2x2"),
                Beam("C", "Larguero tubo cuadrado 2"),
                Beam("D", "Larguero tubo 1.66\""),
                Beam("E", "Larguero canal formado 5")), null);

            Assert.Equal(("Canal CPS 3", "Canal CPS"), Pair(catalog.Find(LargueroSectionSource.RackCad, "A")));
            Assert.Equal("Tubo rectangular (PTR)", catalog.Find(LargueroSectionSource.RackCad, "B").TypeLabel);
            Assert.Equal("Tubo rectangular (PTR)", catalog.Find(LargueroSectionSource.RackCad, "C").TypeLabel);
            Assert.Equal(("Tubo 1.66\"", "Tubo redondo"), Pair(catalog.Find(LargueroSectionSource.RackCad, "D")));
            Assert.Equal(("Canal formado 5", "Canal"), Pair(catalog.Find(LargueroSectionSource.RackCad, "E")));
        }

        // Review of part «lg», point 7: the default is the first larguero in the order of the catalog, not the first of the list by type.
        [Fact]
        public void TheDefaultIsTheFirstLargueroInTheOrderOfTheCatalog()
        {
            var catalog = LargueroSectionCatalog.Build(Small(
                new BeamProfileCatalogEntry { Id = " " },
                Beam("T", "Larguero tubo 1.9\""),
                Beam("E", "Larguero escalón calibre 14")), null);

            Assert.Equal("E", catalog.All[0].Id); // «Escalón» goes first in the list
            Assert.Equal("T", catalog.Default.Id); // the first row of the catalog with an id
            var aiscOnly = LargueroSectionCatalog.Build(null, Aisc);
            Assert.Same(aiscOnly.All[0], aiscOnly.Default); // no larguero: the first section of the list
        }

        // Review of part «lg», point 5: a character outside the BMP (an emoji) and a digit of another script neither throw nor break the order.
        [Fact]
        public void ACharacterOutsideTheBmpOrADigitOfAnotherScriptDoesNotThrow()
        {
            var emoji = char.ConvertFromUtf32(0x1F600);
            var arabicThree = ((char)0x0663).ToString();
            var catalog = LargueroSectionCatalog.Build(Small(
                Beam("X", "Larguero escalón " + emoji + " 3"),
                Beam("Y", "Larguero escalón " + arabicThree + " 4")), null);

            Assert.Equal("Escalón " + emoji + " 3", catalog.Find(LargueroSectionSource.RackCad, "X").Name);
            Assert.Equal(new[] { "X" }, catalog.Filter(null, null, emoji).Select(o => o.Id).ToArray());
            Assert.Equal(0, LargueroNaturalOrder.Instance.Compare(emoji + "1", emoji + "1"));
            Assert.True(LargueroNaturalOrder.Instance.Compare("A" + arabicThree, "A" + arabicThree + "1") < 0);
        }

        [Fact]
        public void ACatalogThatCouldNotBeReadGivesNoSections()
        {
            Assert.Empty(LargueroSectionCatalog.Build(null, null).All);
            Assert.Null(LargueroSectionCatalog.Build(null, null).Default);
            Assert.Equal(Rack.BeamProfiles.Count, LargueroSectionCatalog.Build(Rack, null).All.Count);
        }

        // ------------------------------------------------------------------ the summary

        [Fact]
        public void TheSummaryOfALargueroOfRackCadUsesTheRecipeOfTheLargueroAndTakesTheAutomaticName()
        {
            var rack = Rack;
            var section = LargueroSectionCatalog.Build(rack, null).Default;

            var summary = LargueroDraftSummary.Of(section, 3.0, 96.0, "MENSULA_3_REMACHES_CAL_10", rack);

            Assert.True(summary.IsComplete);
            Assert.Equal("Larguero 96 in · P3", summary.ProposedName);
            Assert.Equal(
                new[]
                {
                    ("Sección", "Escalón calibre 14 · Escalón"),
                    ("Catálogo", "Largueros de RackCad: «Larguero escalón calibre 14 con ménsula 3 remaches» (LARGUERO_ESCALON_CAL14_3_REMACHES)"),
                    ("Peralte", "3 in"),
                    ("Longitud a corte", "96 in"),
                    ("Ménsula", "Ménsula 3 remaches de cinta calibre 10 · 6 in de alto · calibre 10 · la de su larguero en el catálogo"),
                    ("Calibre", "14"),
                },
                summary.Lines.Select(l => (l.Label, l.Value)).ToArray());

            // The one recipe (LargueroBomBuilder.Component): the larguero, its profile ×1 and its ménsulas ×2.
            var recipe = LargueroBomBuilder.Component(rack, section.Id, 96.0, 3.0, "MENSULA_3_REMACHES_CAL_10", 1);
            Assert.Equal(new[] { recipe.Category }.Concat(recipe.Pieces.Select(p => p.Category)).ToArray(), summary.Bom.Select(b => b.Component).ToArray());
            Assert.Equal(new[] { "1", "1 por larguero", "2 por larguero" }, summary.Bom.Select(b => b.QuantityText).ToArray());
            // The larguero, its profile and its ménsula with the names of THIS page, not the ones of the classic BOM (review, point 6).
            Assert.Equal(new[] { "Escalón calibre 14 · P3", "Escalón calibre 14", "Ménsula 3 remaches de cinta calibre 10" },
                summary.Bom.Select(b => b.Description).ToArray());
            Assert.Equal("Larguero escalón calibre 14 con ménsula 3 remaches · P3", LargueroBomBuilder.DescribeBeam(rack, section.Id, 3.0));
            Assert.NotEqual(LargueroBomBuilder.DescribeBeam(rack, section.Id, 3.0), summary.Bom[0].Description);
            Assert.Equal(new[] { "96", "96", "—" }, summary.Bom.Select(b => b.LengthText).ToArray());
        }

        [Fact]
        public void AnAiscSectionHasTheDepthAsItsPeralteItsWeightAndNoMensulaUntilOneIsChosen()
        {
            var rack = Rack;
            var section = LargueroSectionCatalog.Build(rack, Aisc).Filter(LargueroSectionSource.Aisc, null, "C10X15.3").Single();

            var without = LargueroDraftSummary.Of(section, null, 96.0, null, rack);
            Assert.False(without.IsComplete);
            Assert.Equal(new[] { LargueroDraftSummary.AiscMensulaText }, without.Missing.ToArray());
            Assert.Equal("Larguero 96 in · C10X15.3", without.ProposedName);
            Assert.Equal("10 in, el de la sección (del catálogo)", without.Lines.Single(l => l.Label == "Peralte").Value);
            Assert.Equal("15.3 lb/ft (22.8 kg/m) · 122.4 lb (55.5 kg) a 96 in", without.Lines.Single(l => l.Label == "Peso del perfil").Value);
            // With no ménsula chosen the bill of materials lists none.
            Assert.DoesNotContain(without.Bom, b => b.Component == LargueroBomBuilder.Mensula);

            var with = LargueroDraftSummary.Of(section, null, 96.0, "MENSULA_TROQUEL_REDONDO_CAL_10", rack);
            Assert.True(with.IsComplete);
            Assert.Equal("2 por larguero", with.Bom.Single(b => b.Component == LargueroBomBuilder.Mensula).QuantityText);
            Assert.Equal("Ménsula troquel redondo de cinta calibre 10 · 8 in de alto · calibre 10", with.Lines.Single(l => l.Label == "Ménsula").Value);
        }

        [Fact]
        public void TheSummarySaysWhatIsMissing()
        {
            var rack = Rack;
            var section = LargueroSectionCatalog.Build(rack, null).Default;

            Assert.Equal(new[] { LargueroDraftSummary.NoSectionText, LargueroDraftSummary.NoLengthText, LargueroDraftSummary.NoMensulaText },
                LargueroDraftSummary.Of(null, null, null, null, rack).Missing.ToArray());

            var missing = LargueroDraftSummary.Of(section, 6.5, null, "NO_EXISTE", rack);
            Assert.Equal(new[] { LargueroDraftSummary.NoPeralteText, LargueroDraftSummary.NoLengthText, LargueroDraftSummary.UnknownMensulaText },
                missing.Missing.ToArray());
            Assert.Equal(string.Empty, missing.ProposedName);
            // With no ménsula of the catalog, the bill of materials lists none, not the one of the row of the larguero (the recipe's fallback).
            Assert.Equal(new[] { LargueroBomBuilder.Larguero, LargueroBomBuilder.Perfil }, missing.Bom.Select(b => b.Component).ToArray());
            Assert.Equal("falta", missing.Lines.Single(l => l.Label == "Peralte").Value);
            Assert.Equal("falta", missing.Lines.Single(l => l.Label == "Longitud a corte").Value);
        }

        // Review of part «lg», point 2: the page never says «Larguero sin terminar» and «completo» at once. A larguero of RackCad whose row
        // declares no peralte cannot be completed, and says why; an AISC section whose catalog gives no depth needs none (its name is its
        // designation).
        [Fact]
        public void TheNameAndWhatIsMissingNeverContradictEachOther()
        {
            var rack = Small(Beam("N", "Larguero escalón sin peraltes", peraltes: ""));
            var noPeraltes = LargueroSectionCatalog.Build(rack, null).Find(LargueroSectionSource.RackCad, "N");

            var summary = LargueroDraftSummary.Of(noPeraltes, null, 96.0, "M1", rack);
            Assert.Equal(string.Empty, summary.ProposedName);
            Assert.Equal(new[] { LargueroDraftSummary.NoCatalogPeraltesText }, summary.Missing.ToArray());
            Assert.Equal("falta: el catálogo no da los peraltes de este larguero", summary.Lines.Single(l => l.Label == "Peralte").Value);

            var noDepth = new LargueroSectionOption(LargueroSectionSource.Aisc, "AISC-C-X1", "X1", "Canal CPS", "X1", Array.Empty<double>(), null, null, null, null);
            var aisc = LargueroDraftSummary.Of(noDepth, null, 96.0, "M1", rack);
            Assert.True(aisc.IsComplete);
            Assert.Equal("Larguero 96 in · X1", aisc.ProposedName);
            Assert.Equal("el catálogo no da el peralte de esta sección", aisc.Lines.Single(l => l.Label == "Peralte").Value);

            // Over every section of the real catalogs, with and without a length, a peralte and a ménsula: no name ⇒ not complete.
            var real = Rack;
            foreach (var option in LargueroSectionCatalog.Build(real, Aisc).All)
                foreach (var length in new double?[] { 96.0, null })
                    foreach (var peralte in new double?[] { option.Peraltes.FirstOrDefault(), null, 99.0 })
                        foreach (var mensula in new[] { "MENSULA_3_REMACHES_CAL_10", null })
                        {
                            var each = LargueroDraftSummary.Of(option, peralte, length, mensula, real);
                            Assert.True(each.ProposedName.Length > 0 || !each.IsComplete, option.Id + " sin nombre y completo");
                        }
        }

        // Review of part «lg», point 3: a peralte of the catalog with more than three decimals reads back exactly, so it completes the larguero.
        [Fact]
        public void APeralteWithMoreThanThreeDecimalsCompletesTheLarguero()
        {
            var rack = Small(Beam("P", "Larguero escalón fino", peraltes: "3.12345;4"));
            var section = LargueroSectionCatalog.Build(rack, null).Find(LargueroSectionSource.RackCad, "P");
            var text = LargueroSectionNaming.Number(section.Peraltes[0]);

            Assert.Equal("3.12345", text);
            Assert.Equal("3.5", LargueroSectionNaming.Number(3.5));
            Assert.Equal("1.9", LargueroSectionNaming.Number(1.9));
            var read = double.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
            var summary = LargueroDraftSummary.Of(section, read, 96.0, "M1", rack);
            Assert.True(summary.IsComplete);
            Assert.Equal("Larguero 96 in · P3.12345", summary.ProposedName);
        }

        private static (string Name, string Type) Pair(LargueroSectionOption option) => (option.Name, option.TypeLabel);
    }
}
