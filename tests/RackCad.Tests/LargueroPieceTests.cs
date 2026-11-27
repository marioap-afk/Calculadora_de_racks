using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Larguero;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Larguero;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64, sexta tanda, part «lg» (the answers of the Owner): the page «Larguero» finished. (1) «Un campo nuevo»: the design of a larguero
    /// gains the catalog and the section of an AISC section, with the largueros saved before opening the same and the unknown fields kept
    /// (I-11). (2) «Pieza suelta»: the plan of the piece AutoCAD inserts — the larguero with its two ménsulas, the AISC profile with the
    /// geometry of RACKSECCION and, for the S family, its warning of ADR-0023 — and its sketch (point 3). (6) «Sí, una sola página»: a bare
    /// AISC profile, the same plan RACKSECCION builds. Then the rules of the door (RACKLARGUEROINSERTAR) and the guards of its command, read
    /// as text because the Plugin needs AutoCAD.
    /// </summary>
    public class LargueroPieceTests
    {
        private static RackCatalog Rack => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static StructuralSectionCatalog Aisc => new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly RackProjectStore Store = new();

        private static LargueroSectionCatalog Sections(StructuralSectionCatalog aisc = null) => LargueroSectionCatalog.Build(Rack, aisc ?? Aisc);

        private static MensulaCatalogEntry Mensula(string id) => Rack.Mensulas.Single(m => m.Id == id);

        // ------------------------------------------------------------------ (1) the format: «Un campo nuevo»

        [Fact]
        public void ALargueroOfRackCadRoundTripsAsBeforeAndALargueroOfAiscKeepsItsCatalogAndItsSection()
        {
            var rackcad = new LargueroDesign { Name = "L1", BeamProfileId = "LARGUERO_ESCALON_CAL14_3_REMACHES", Peralte = 4, Length = 96, MensulaOverride = "M" };
            var back = Store.Deserialize(Store.Serialize(RackProject.ForLarguero(rackcad))).Larguero;
            Assert.Equal(("L1", "LARGUERO_ESCALON_CAL14_3_REMACHES", 4.0, 96.0, "M"), (back.Name, back.BeamProfileId, back.Peralte, back.Length, back.MensulaOverride));
            Assert.Null(back.SectionCatalog);
            Assert.Null(back.SectionId);

            var aisc = new LargueroDesign
            {
                Name = "Larguero 144 in · C10X15.3", SectionCatalog = LargueroDesign.AiscCatalog, SectionId = "AISC-C-C10X15_3", Peralte = 10, Length = 144,
                MensulaOverride = "MENSULA_TROQUEL_REDONDO_CAL_10",
            };
            var json = Store.Serialize(RackProject.ForLarguero(aisc));
            using (var document = JsonDocument.Parse(json))
            {
                var payload = document.RootElement.GetProperty("Larguero");
                Assert.Equal("1.1", payload.GetProperty("SchemaVersion").GetString()); // the additive minor of the new field
                Assert.Equal("AISC", payload.GetProperty("SectionCatalog").GetString());
                Assert.Equal("AISC-C-C10X15_3", payload.GetProperty("SectionId").GetString());
                Assert.Equal(JsonValueKind.Null, payload.GetProperty("BeamProfileId").ValueKind);
            }

            var loaded = Store.Deserialize(json);
            Assert.Equal(RackSystemKind.Larguero, loaded.Kind);
            Assert.Equal(("AISC", "AISC-C-C10X15_3", 10.0, 144.0), (loaded.Larguero.SectionCatalog, loaded.Larguero.SectionId, loaded.Larguero.Peralte, loaded.Larguero.Length));
            Assert.Null(loaded.Larguero.BeamProfileId);
            Assert.Equal("MENSULA_TROQUEL_REDONDO_CAL_10", loaded.Larguero.MensulaOverride);
        }

        [Fact]
        public void ALargueroSavedTodayOpensTheSameAndASaveKeepsItsUnknownFields()
        {
            // A file of today, literally (schema 1.0, no catalog of the section): it opens as a larguero of RackCad, as before.
            const string Today = "{\"SchemaVersion\":\"2.0\",\"Kind\":\"Larguero\",\"Larguero\":{\"SchemaVersion\":\"1.0\",\"Name\":\"Larguero 96 in · P3\","
                                 + "\"BeamProfileId\":\"LARGUERO_ESCALON_CAL14_3_REMACHES\",\"Peralte\":3,\"Length\":96,\"MensulaOverride\":null,"
                                 + "\"futureLargueroField\":{\"a\":1}}}";
            var project = Store.Deserialize(Today);
            Assert.Equal("LARGUERO_ESCALON_CAL14_3_REMACHES", project.Larguero.BeamProfileId);
            Assert.Null(project.Larguero.SectionCatalog);
            Assert.Null(project.Larguero.SectionId);
            Assert.True(RackDesignValidation.IsUsableLarguero(project.Larguero));

            // Saved again (from the page, through WithSourceMetadataFrom): the unknown field survives, the version goes to 1.1 (never down).
            var resaved = JsonNode.Parse(Store.Serialize(RackProject.ForLarguero(project.Larguero).WithSourceMetadataFrom(project)))!.AsObject();
            var payload = resaved["Larguero"]!.AsObject();
            Assert.Equal(1, payload["futureLargueroField"]!["a"]!.GetValue<int>());
            Assert.Equal("1.1", payload["SchemaVersion"]!.GetValue<string>());
            Assert.Equal("LARGUERO_ESCALON_CAL14_3_REMACHES", payload["BeamProfileId"]!.GetValue<string>());

            // A file of a newer minor keeps its minor; the unknown fields of a larguero of AISC survive too.
            var aisc = JsonNode.Parse(Store.Serialize(RackProject.ForLarguero(new LargueroDesign
            {
                SectionCatalog = "AISC", SectionId = "AISC-W-W10X12", Length = 120,
            })))!.AsObject();
            aisc["Larguero"]!.AsObject()["SchemaVersion"] = "1.7";
            aisc["Larguero"]!.AsObject()["otherField"] = "x";
            var reopened = Store.Deserialize(aisc.ToJsonString());
            var again = JsonNode.Parse(Store.Serialize(RackProject.ForLarguero(reopened.Larguero).WithSourceMetadataFrom(reopened)))!["Larguero"]!.AsObject();
            Assert.Equal("1.7", again["SchemaVersion"]!.GetValue<string>());
            Assert.Equal("x", again["otherField"]!.GetValue<string>());
            Assert.Equal("AISC-W-W10X12", again["SectionId"]!.GetValue<string>());
        }

        [Fact]
        public void ALargueroNeedsAProfileOfRackCadOrACatalogAndASectionAndHalfOfTheSecondIsNothing()
        {
            Assert.True(RackDesignValidation.IsUsableLarguero(new LargueroDesign { BeamProfileId = "X" }));
            Assert.True(RackDesignValidation.IsUsableLarguero(new LargueroDesign { SectionCatalog = "AISC", SectionId = "AISC-W-W10X12" }));
            Assert.False(RackDesignValidation.IsUsableLarguero(new LargueroDesign()));
            Assert.False(RackDesignValidation.IsUsableLarguero(new LargueroDesign { SectionCatalog = "AISC" }));
            Assert.False(RackDesignValidation.IsUsableLarguero(new LargueroDesign { SectionId = "AISC-W-W10X12" }));
            Assert.False(RackDesignValidation.IsUsableLarguero(new LargueroDesign { SectionCatalog = " ", SectionId = "AISC-W-W10X12" }));

            // The store says it: a «{}» larguero and a catalog without a section are refused as incomplete.
            Assert.Throws<InvalidOperationException>(() => Store.Deserialize("{\"Kind\":\"Larguero\",\"Larguero\":{}}"));
            Assert.Throws<InvalidOperationException>(() => Store.Deserialize("{\"Kind\":\"Larguero\",\"Larguero\":{\"SectionCatalog\":\"AISC\"}}"));
            Assert.NotNull(Store.Deserialize("{\"Kind\":\"Larguero\",\"Larguero\":{\"SectionCatalog\":\"AISC\",\"SectionId\":\"AISC-W-W10X12\"}}").Larguero);
        }

        [Fact]
        public void TheBomOfALargueroOfAiscListsItsSection()
        {
            var bom = LargueroBomBuilder.Build(new LargueroDesign { SectionCatalog = "AISC", SectionId = "AISC-C-C10X15_3", Length = 96, MensulaOverride = "MENSULA_3_REMACHES_CAL_10" }, Rack);
            var component = Assert.Single(bom.Components);
            Assert.Equal("AISC-C-C10X15_3", component.ProfileId);
            Assert.Equal(new[] { "Perfil", "Ménsula" }, component.Pieces.Select(p => p.Category).ToArray());
            Assert.Equal(2, component.Pieces[1].Quantity);
        }

        // ------------------------------------------------------------------ (2, 3) the plan of the larguero and its sketch

        [Fact]
        public void ALargueroOfRackCadIsItsProfileAndTwoMensulasSeatedAsTheBlocksOfTheCatalog()
        {
            var section = Sections().Find(LargueroSectionSource.RackCad, "LARGUERO_ESCALON_CAL14_3_REMACHES");
            var (plan, refusal) = LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, section, 3, 96, Mensula("MENSULA_3_REMACHES_CAL_10"), Rack, null,
                "Larguero 96 in · P3");

            Assert.Null(refusal);
            Assert.Equal(LargueroPieceKind.Larguero, plan.Kind);
            Assert.Equal("Larguero 96 in · P3", plan.Name);
            Assert.Null(plan.SectionPlan);
            Assert.False(plan.IsVisualDerived);
            // INICIO_PERFIL X of its FRONTAL block in connection-layout.csv: the profile starts 1 in from the face of the post.
            Assert.Equal(1.0, plan.Overhang);
            Assert.Equal(98.0, plan.OverallLength);
            Assert.Equal(6.0, plan.MensulaHeight); // mensulas.csv
            Assert.Equal("RACKCAD_LARGUERO_LARGUERO_ESCALON_CAL14_3_REMACHES_P3_L96", plan.BlockBaseName);

            var profile = Assert.Single(plan.Curves, c => c.Role == LargueroCurveRole.Profile);
            Assert.True(profile.IsClosed);
            Assert.Equal((1.0, -3.0, 97.0, 0.0), Box(profile));
            var mensulas = plan.Curves.Where(c => c.Role == LargueroCurveRole.Mensula).Select(Box).ToList();
            Assert.Equal(new[] { (0.0, -6.0, 2.0, 0.0), (96.0, -6.0, 98.0, 0.0) }, mensulas);
            Assert.Equal((0.0, -6.0, 98.0, 0.0), (plan.Bounds.MinX, plan.Bounds.MinY, plan.Bounds.MaxX, plan.Bounds.MaxY));
        }

        [Fact]
        public void ALargueroOfAiscIsTheElevationOfRackseccionMovedToItsSeat()
        {
            var aisc = Aisc;
            var section = Sections(aisc).All.Single(o => o.Source == LargueroSectionSource.Aisc && o.Name == "C10X15.3");
            var factory = new StructuralSectionGeometryFactory(aisc);
            var (plan, refusal) = LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, section, null, 120, Mensula("MENSULA_TROQUEL_REDONDO_CAL_10"), Rack, factory, "");

            Assert.Null(refusal);
            Assert.Equal("C10X15.3", plan.Name); // no name typed: the designation
            Assert.Equal(1.0, plan.Overhang); // an AISC section has no INICIO_PERFIL: the 1 in of today
            Assert.Equal(8.0, plan.MensulaHeight);
            Assert.Equal(SectionViewKind.LongitudinalX, plan.SectionPlan.View);

            // The same plan RACKSECCION builds for that view, each point moved by the same offset.
            var rackseccion = StructuralSectionPlanBuilder.Build(factory.Get(section.Aisc, SectionDetailLevel.Tabulated),
                PrismaticSectionInstance.Create(section.Aisc.SectionId, 120, null, 0.0, false),
                new SectionRepresentationOptions { Viewpoint = SectionViewpoint.Standard(SectionViewKind.LongitudinalX) });
            Assert.Equal(rackseccion.Signature(), plan.SectionPlan.Signature());
            var profiles = plan.Curves.Where(c => c.Role == LargueroCurveRole.Profile).ToList();
            Assert.Equal(rackseccion.Curves.Count, profiles.Count);
            var dx = 1.0 - rackseccion.Bounds.MinX;
            var dy = -rackseccion.Bounds.MaxY;
            for (var i = 0; i < profiles.Count; i++)
                for (var j = 0; j < profiles[i].Points.Count; j++)
                {
                    Assert.Equal(rackseccion.Curves[i].Points[j].X + dx, profiles[i].Points[j].X, 9);
                    Assert.Equal(rackseccion.Curves[i].Points[j].Y + dy, profiles[i].Points[j].Y, 9);
                }

            // The profile runs from the overhang for its length, its top at 0 and its depth the one of the catalog (d of a C10 = 10 in).
            var box = Bounds(profiles);
            Assert.Equal(1.0, box.MinX, 9);
            Assert.Equal(121.0, box.MaxX, 9);
            Assert.Equal(0.0, box.MaxY, 9);
            Assert.Equal(section.Depth!.Value, -box.MinY, 6);
            Assert.Equal(10.0, plan.Peralte, 6);
            Assert.Equal(new[] { (0.0, -8.0, 2.0, 0.0), (120.0, -8.0, 122.0, 0.0) },
                plan.Curves.Where(c => c.Role == LargueroCurveRole.Mensula).Select(Box).ToList());
        }

        [Fact]
        public void ABareProfileIsThePlanOfRackseccionForTheViewAndTheDetailChosen()
        {
            var aisc = Aisc;
            var section = Sections(aisc).All.Single(o => o.Source == LargueroSectionSource.Aisc && o.Name == "W10X12");
            var factory = new StructuralSectionGeometryFactory(aisc);
            foreach (var view in new[] { SectionViewKind.CrossSection, SectionViewKind.LongitudinalY, SectionViewKind.Isometric })
            foreach (var detail in new[] { SectionDetailLevel.Tabulated, SectionDetailLevel.Simplified })
            {
                // No ménsula needed for a bare profile.
                var (plan, refusal) = LargueroPiecePlanner.Build(LargueroPieceKind.Profile, section, null, 60, null, Rack, factory, "otro nombre", view, detail);
                Assert.Null(refusal);
                var rackseccion = StructuralSectionPlanBuilder.Build(factory.Get(section.Aisc, detail),
                    PrismaticSectionInstance.Create(section.Aisc.SectionId, 60, null, 0.0, false),
                    new SectionRepresentationOptions { Viewpoint = SectionViewpoint.Standard(view), Detail = detail });
                Assert.Equal(rackseccion.Signature(), plan.SectionPlan.Signature());
                Assert.Equal("W10X12", plan.Name); // a bare profile is its designation
                Assert.Equal(rackseccion.Curves.Count, plan.Curves.Count);
                Assert.Equal(0.0, plan.MensulaHeight);
                Assert.DoesNotContain(plan.Curves, c => c.Role == LargueroCurveRole.Mensula);
            }
        }

        [Fact]
        public void TheSFamilyCarriesItsWarningOfAdr0023AlwaysAndTheOthersDoNot()
        {
            var aisc = Aisc;
            var sections = Sections(aisc);
            var factory = new StructuralSectionGeometryFactory(aisc);
            var s = sections.All.Single(o => o.Source == LargueroSectionSource.Aisc && o.Name == "S10X25.4");
            var w = sections.All.Single(o => o.Source == LargueroSectionSource.Aisc && o.Name == "W10X12");

            foreach (var kind in new[] { LargueroPieceKind.Larguero, LargueroPieceKind.Profile })
            foreach (var detail in new[] { SectionDetailLevel.Tabulated, SectionDetailLevel.Simplified })
            {
                var plan = LargueroPiecePlanner.Build(kind, s, null, 96, Mensula("MENSULA_3_REMACHES_CAL_10"), Rack, factory, "", SectionViewKind.LongitudinalX, detail).Plan;
                Assert.True(plan.IsVisualDerived);
                Assert.Contains("ADVERTENCIA: geometria visual derivada (ADR-0023)", LargueroDoorRules.CommandLineText(plan, "B"), StringComparison.Ordinal);
                Assert.Contains("NO es apta para CNC", LargueroDoorRules.CommandLineText(plan, "B"), StringComparison.Ordinal);
                Assert.Contains(LargueroDoorRules.VisualDerivedWarning, LargueroDoorRules.Inserted(1, plan, "B").Message, StringComparison.Ordinal);

                var other = LargueroPiecePlanner.Build(kind, w, null, 96, Mensula("MENSULA_3_REMACHES_CAL_10"), Rack, factory, "", SectionViewKind.LongitudinalX, detail).Plan;
                Assert.False(other.IsVisualDerived);
                Assert.DoesNotContain("ADVERTENCIA", LargueroDoorRules.CommandLineText(other, "B"), StringComparison.Ordinal);
                Assert.DoesNotContain("ADR-0023", LargueroDoorRules.Inserted(1, other, "B").Message, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void APieceThatIsNotCompleteHasNoPlanAndSaysWhatIsMissing()
        {
            var aisc = Aisc;
            var sections = Sections(aisc);
            var factory = new StructuralSectionGeometryFactory(aisc);
            var rackcad = sections.Find(LargueroSectionSource.RackCad, "LARGUERO_ESCALON_CAL14_3_REMACHES");
            var c = sections.All.Single(o => o.Source == LargueroSectionSource.Aisc && o.Name == "C10X15.3");
            var mensula = Mensula("MENSULA_3_REMACHES_CAL_10");

            Assert.Equal(LargueroPiecePlanner.NoSectionText, LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, null, 3, 96, mensula, Rack, factory, "").Refusal);
            foreach (var length in new double?[] { null, 0, -5, double.PositiveInfinity, double.NaN })
                Assert.Equal(LargueroPiecePlanner.NoLengthText, LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, rackcad, 3, length, mensula, Rack, factory, "").Refusal);
            Assert.Equal(LargueroPiecePlanner.NoMensulaText, LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, rackcad, 3, 96, null, Rack, factory, "").Refusal);
            Assert.Equal(LargueroPiecePlanner.NoMensulaText, LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, c, null, 96, null, Rack, factory, "").Refusal);
            Assert.Equal(LargueroPiecePlanner.NoPeralteText, LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, rackcad, 7, 96, mensula, Rack, factory, "").Refusal);
            Assert.Equal(LargueroPiecePlanner.NoPeralteText, LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, rackcad, null, 96, mensula, Rack, factory, "").Refusal);
            Assert.Equal(LargueroPiecePlanner.ProfileOnlyAiscText, LargueroPiecePlanner.Build(LargueroPieceKind.Profile, rackcad, 3, 96, mensula, Rack, factory, "").Refusal);
            Assert.Equal(LargueroPiecePlanner.NoAiscCatalogText, LargueroPiecePlanner.Build(LargueroPieceKind.Profile, c, null, 96, null, Rack, null, "").Refusal);
            Assert.All(new[]
            {
                LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, null, 3, 96, mensula, Rack, factory, "").Plan,
                LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, rackcad, 3, 96, null, Rack, factory, "").Plan,
            }, plan => Assert.Null(plan));
        }

        [Fact]
        public void TheSketchShowsBothEndsToOneScaleAndBreaksALongPieceInTheMiddle()
        {
            var section = Sections().Find(LargueroSectionSource.RackCad, "LARGUERO_ESCALON_CAL14_3_REMACHES");
            var plan = LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, section, 3, 96, Mensula("MENSULA_3_REMACHES_CAL_10"), Rack, null, "").Plan;
            var sketch = LargueroPieceSketch.Of(plan);

            // 98 in end to end, 6 in high: each end shows max(12, 4 × 6) = 24 in, and the gap of 3 in between.
            Assert.True(sketch.IsBroken);
            Assert.Equal(24.0, sketch.ShownEnd);
            Assert.Equal(24.0 + LargueroPieceSketch.BreakGap / 2.0, sketch.BreakAt!.Value, 9);
            Assert.Equal((0.0, -6.0, 51.0, 0.0), (sketch.Bounds.MinX, sketch.Bounds.MinY, sketch.Bounds.MaxX, sketch.Bounds.MaxY));
            // Nothing falls in the gap; the heights are those of the piece (the same scale both ways).
            Assert.DoesNotContain(sketch.Lines, l => Inside(l.From.X, 24.0, 27.0) || Inside(l.To.X, 24.0, 27.0));
            Assert.Equal(-3.0, sketch.Lines.Where(l => l.Role == LargueroCurveRole.Profile).Min(l => Math.Min(l.From.Y, l.To.Y)), 9);
            Assert.Equal(-6.0, sketch.Lines.Where(l => l.Role == LargueroCurveRole.Mensula).Min(l => Math.Min(l.From.Y, l.To.Y)), 9);
            // Both ménsulas are there: one at each end.
            var mensulaXs = sketch.Lines.Where(l => l.Role == LargueroCurveRole.Mensula).SelectMany(l => new[] { l.From.X, l.To.X }).ToList();
            Assert.Equal(0.0, mensulaXs.Min(), 9);
            Assert.Equal(51.0, mensulaXs.Max(), 9);

            // A short piece is drawn whole.
            var shortPlan = LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, section, 3, 24, Mensula("MENSULA_3_REMACHES_CAL_10"), Rack, null, "").Plan;
            var whole = LargueroPieceSketch.Of(shortPlan);
            Assert.False(whole.IsBroken);
            Assert.Equal(shortPlan.Curves.Sum(c => c.Points.Count), whole.Lines.Count); // every edge of every closed curve
            Assert.Empty(LargueroPieceSketch.Of(null).Lines);
        }

        [Fact]
        public void TheCrossSectionOfABareProfileIsNeverBroken()
        {
            var aisc = Aisc;
            var section = Sections(aisc).All.Single(o => o.Source == LargueroSectionSource.Aisc && o.Name == "W10X12");
            var plan = LargueroPiecePlanner.Build(LargueroPieceKind.Profile, section, null, 240, null, Rack, new StructuralSectionGeometryFactory(aisc), "",
                SectionViewKind.CrossSection).Plan;
            Assert.False(LargueroPieceSketch.Of(plan).IsBroken);
            var longitudinal = LargueroPiecePlanner.Build(LargueroPieceKind.Profile, section, null, 240, null, Rack, new StructuralSectionGeometryFactory(aisc), "",
                SectionViewKind.LongitudinalX).Plan;
            Assert.True(LargueroPieceSketch.Of(longitudinal).IsBroken);
        }

        // ------------------------------------------------------------------ the rules of the door

        [Fact]
        public void TheDoorRefusesNothingToInsertAndSaysWhatAutoCadDidInTheWordsOfTheUser()
        {
            Assert.Equal("RACKLARGUEROINSERTAR", LargueroDoorRules.InsertCommand);
            Assert.NotNull(LargueroDoorRules.RefusalOf(null));
            Assert.NotNull(LargueroDoorRules.RefusalOf(new LargueroInsertRequest(1, new SessionId(7), null)));

            var section = Sections().Find(LargueroSectionSource.RackCad, "LARGUERO_ESCALON_CAL14_3_REMACHES");
            var plan = LargueroPiecePlanner.Build(LargueroPieceKind.Larguero, section, 3, 96, Mensula("MENSULA_3_REMACHES_CAL_10"), Rack, null, "Larguero 96 in · P3").Plan;
            Assert.Null(LargueroDoorRules.RefusalOf(new LargueroInsertRequest(1, new SessionId(7), plan)));
            Assert.Equal("\nPunto de insercion del larguero (cara del poste, arriba): ", LargueroDoorRules.PointPrompt(plan));

            var inserted = LargueroDoorRules.Inserted(4, plan, "RACKCAD_LARGUERO_X");
            Assert.Equal((4L, true, "RACKCAD_LARGUERO_X"), (inserted.RequestId, inserted.Inserted, inserted.BlockName));
            Assert.StartsWith("AutoCAD insertó el larguero «Larguero 96 in · P3» como una pieza suelta (bloque «RACKCAD_LARGUERO_X»): no es un rack", inserted.Message,
                StringComparison.Ordinal);
            Assert.DoesNotContain("Peso", inserted.Message, StringComparison.Ordinal); // a larguero of RackCad has no tabulated weight
            Assert.Contains("98 in con sus 2 mensulas", LargueroDoorRules.CommandLineText(plan, "RACKCAD_LARGUERO_X"), StringComparison.Ordinal);

            var cancelled = LargueroDoorRules.Cancelled(5);
            Assert.False(cancelled.Inserted);
            Assert.EndsWith("El dibujo no cambió.", cancelled.Message, StringComparison.Ordinal);
            var failed = LargueroDoorRules.Failed(6, "eLockViolation ");
            Assert.Equal("No se pudo insertar: eLockViolation El dibujo no cambió.", failed.Message);
        }

        [Fact]
        public void AnInsertedProfileOfAiscSaysItsWeightAsRackseccion()
        {
            var aisc = Aisc;
            var section = Sections(aisc).All.Single(o => o.Source == LargueroSectionSource.Aisc && o.Name == "C10X15.3");
            var plan = LargueroPiecePlanner.Build(LargueroPieceKind.Profile, section, null, 96, null, Rack, new StructuralSectionGeometryFactory(aisc), "").Plan;
            Assert.Equal("\nPunto de insercion del perfil: ", LargueroDoorRules.PointPrompt(plan));
            Assert.Contains("Peso del perfil: 15.3 lb/ft (22.8 kg/m), 122.4 lb a 96 in.", LargueroDoorRules.Inserted(1, plan, "B").Message, StringComparison.Ordinal);
            var line = LargueroDoorRules.CommandLineText(plan, "RACKCAD_SECCION_X");
            Assert.StartsWith("RackCad: C10X15.3 insertada como bloque 'RACKCAD_SECCION_X'. Longitud 96 in, vista CrossSection, ", line, StringComparison.Ordinal);
            Assert.Contains("\nPeso: C10X15.3", line, StringComparison.Ordinal);
        }

        // ------------------------------------------------------------------ the command, read as text (the Plugin needs AutoCAD)

        private const string CommandFile = "src/RackCad.Plugin/RackLargueroCommands.Ventana.cs";
        private const string PortFile = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.Larguero.cs";
        private const string MaterializerFile = "src/RackCad.Plugin/Drawing/StructuralSections/LargueroPieceMaterializer.cs";

        [Fact]
        public void TheCommandAsksThePointFirstAndPlacesTheBlockInOneTransactionAndOneUndo()
        {
            var code = WorkspaceHostSource.Code(CommandFile);
            Assert.Contains("[CommandMethod(\"RACKLARGUEROINSERTAR\")]", code, StringComparison.Ordinal); // no flags: one undo step, never NoUndoMarker
            Assert.DoesNotContain("NoUndoMarker", code, StringComparison.Ordinal);

            var take = code.IndexOf("TakePendingLargueroInsert()", StringComparison.Ordinal);
            var started = code.IndexOf("door?.Started(pending.RequestId)", StringComparison.Ordinal);
            var otherDrawing = code.IndexOf("pending.Document != RackCadRackDoorPort.KeyOf(document)", StringComparison.Ordinal);
            var refusal = code.IndexOf("LargueroDoorRules.RefusalOf(request)", StringComparison.Ordinal);
            var units = code.IndexOf("RackUnitsGuard.WarnIfNotInches(document)", StringComparison.Ordinal);
            var point = code.IndexOf("GetPoint(", StringComparison.Ordinal);
            var cancelled = code.IndexOf("LargueroDoorRules.Cancelled(", StringComparison.Ordinal);
            var transaction = code.IndexOf("StartTransaction()", StringComparison.Ordinal);
            var definition = code.IndexOf("CreateBlockDefinition", StringComparison.Ordinal);
            var reference = code.IndexOf("InsertReference", StringComparison.Ordinal);
            var commit = code.IndexOf("transaction.Commit()", StringComparison.Ordinal);
            var regen = code.IndexOf("SystemBlockWriter.ApplyRegen(document, regen: true)", StringComparison.Ordinal);
            var answer = code.IndexOf("LargueroDoorRules.Inserted(request.RequestId, plan, blockName)", StringComparison.Ordinal);
            Assert.True(take > 0 && take < started && started < otherDrawing && otherDrawing < refusal && refusal < units && units < point && point < cancelled
                        && cancelled < transaction && transaction < definition && definition < reference && reference < commit && commit < regen && regen < answer,
                "El orden: tomar la petición, Started, el dibujo, la regla, las unidades, el punto (cancelar no crea nada), una transacción con la definición y la "
                + "referencia, Commit, una regeneración y la respuesta.");
            Assert.Equal(commit, code.LastIndexOf("transaction.Commit()", StringComparison.Ordinal)); // one commit: one step of «U»
            Assert.Equal(units, code.LastIndexOf("RackUnitsGuard.WarnIfNotInches", StringComparison.Ordinal)); // once
            Assert.DoesNotContain("Editor.Regen()", code, StringComparison.Ordinal);
            Assert.DoesNotContain("Erase()", code, StringComparison.Ordinal);

            // A failure before the commit says nothing was inserted; after it, the answer says it was.
            var failure = code.IndexOf("LargueroDoorRules.Failed(request.RequestId, ex.Message)", StringComparison.Ordinal);
            Assert.True(failure > commit && failure < regen);
            Assert.Contains("catch (System.Exception ex)", code, StringComparison.Ordinal);
            Assert.DoesNotContain("catch (Exception", code, StringComparison.Ordinal);
        }

        [Fact]
        public void ABareProfileIsTheBlockOfRackseccionAndALargueroItsOwnAndNeitherComputesGeometry()
        {
            var code = WorkspaceHostSource.Code(CommandFile);
            Assert.Contains("plan.Kind == LargueroPieceKind.Profile", code, StringComparison.Ordinal);
            Assert.Contains("StructuralSectionMaterializer.CreateBlockDefinition(document.Database, transaction, plan.SectionPlan, out blockName)", code, StringComparison.Ordinal);
            Assert.Contains("LargueroPieceMaterializer.CreateBlockDefinition(document.Database, transaction, plan, out blockName)", code, StringComparison.Ordinal);

            var materializer = WorkspaceHostSource.Code(MaterializerFile);
            Assert.Contains("foreach (var curve in plan.Curves)", materializer, StringComparison.Ordinal);
            Assert.Contains("polyline.Closed = curve.IsClosed", materializer, StringComparison.Ordinal);
            Assert.Contains("blockTable.Has(", materializer, StringComparison.Ordinal);
            foreach (var forbidden in new[] { "Math.", "StructuralSectionPlanBuilder", "StructuralSectionGeometryFactory", "PrismaticSectionInstance",
                         "LargueroPiecePlanner", "MensulaCatalogEntry", "RackCatalog", "BlockLibraryImporter", "blocks-library", "blockTable[" })
            {
                Assert.DoesNotContain(forbidden, materializer, StringComparison.Ordinal);
                Assert.DoesNotContain(forbidden, code, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ThePortKeepsTheRequestBeforeItIsHandedOverAndAbandonForgetsIt()
        {
            var port = WorkspaceHostSource.Code(PortFile);
            Assert.Contains("LargueroDoorRules.RefusalOf(request)", port, StringComparison.Ordinal);
            Assert.Contains("Send(LargueroDoorRules.InsertCommand + \" \"", port, StringComparison.Ordinal);
            Assert.Contains("_pendingLargueroInsert = new PendingLargueroInsert(request, KeyOf(document))", port, StringComparison.Ordinal);
            Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"LockDocument|StartTransaction|\[CommandMethod"), port);
            Assert.Contains("AbandonLarguero(requestId);", WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs"), StringComparison.Ordinal);
        }

        private static (double, double, double, double) Box(LargueroPieceCurve curve)
        {
            var box = Bounds(new[] { curve });
            return (box.MinX, box.MinY, box.MaxX, box.MaxY);
        }

        private static RackCad.Application.Geometry.Bounds2D Bounds(IEnumerable<LargueroPieceCurve> curves) =>
            RackCad.Application.Geometry.Bounds2D.FromPoints(curves.SelectMany(c => c.Points));

        private static bool Inside(double x, double min, double max) => x > min + 1e-9 && x < max - 1e-9;
    }
}
