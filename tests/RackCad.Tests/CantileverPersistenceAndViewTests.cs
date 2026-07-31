using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D gate 2 and gate 3: the line's persistence and registries, and its three pure view plans.
    ///
    /// The persistence half goes through the REAL <see cref="RackProjectStore"/> and the real
    /// <see cref="SystemRegistry.Default"/>, because what it has to prove is that a Cantilever line survives the
    /// shared infrastructure — not that a hand-built document round-trips through itself.
    /// </summary>
    public class CantileverPersistenceAndViewTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private static CantileverLineDesign Design(int stations = 3, int levels = 2)
        {
            var topology = new CantileverLineStationTopologyDesign
            {
                FaceMode = CantileverStationFaceMode.Single,
                SingleSide = CantileverArmSide.PositiveY,
                LevelCount = levels,
                RequestedClearHeight = 24.0,
                ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                {
                    ColumnSectionId = ColumnW,
                    Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
                }
            };

            topology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            topology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = 4.0;

            return new CantileverLineDesign
            {
                Name = "Linea A",
                StationCount = stations,
                ColumnCentreSpacing = 96.0,
                StationTopology = topology,
                DefaultArmTemplate = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign
                    {
                        Arrangement = CantileverArmBodyArrangement.Single,
                        SectionId = ArmHss,
                        CutLength = 36.0
                    },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign
                    {
                        VerticalPunchCount = 2,
                        VerticalEndOffset = 1.5
                    }
                },
                Bracing = new CantileverBracingDesign()
            };
        }

        private static CantileverLineAssembly Resolve(CantileverLineDesign design) =>
            CantileverLineResolver.Resolve(
                design, Catalog, Factory,
                CantileverCataloguePolicies.ColumnBase(Catalog),
                CantileverCataloguePolicies.Arm(Catalog));

        // ---- 1. the kind is registered, and its names are frozen ---------------------------------------

        [Fact]
        public void CantileverIsTheSeventhKindAndTheSixBeforeItKeepTheirNumbers()
        {
            Assert.Equal(6, (int)RackSystemKind.Cantilever);
            Assert.Equal(7, Enum.GetValues(typeof(RackSystemKind)).Length);
        }

        [Fact]
        public void TheNamesAreFrozenFromTheFirstPersistence()
        {
            // Every one of these travels inside a saved file or a stamped block. Changing one orphans them, which
            // is why they are asserted literally and not derived from a constant.
            Assert.Equal("Cantilever", RackSystemKind.Cantilever.ToString());
            Assert.Equal("cantilever", RackEmbedDocument.KindCantilever);
            Assert.Equal("Cantilever", RackListBuilder.KindLabel("cantilever"));

            var descriptor = SystemRegistry.Default.Get(RackSystemKind.Cantilever);

            Assert.Equal("Cantilever", descriptor.LibraryLabel);
            Assert.Equal("la línea Cantilever", descriptor.ValidationNoun);
            Assert.True(descriptor.SupportsPersistence);
        }

        [Fact]
        public void TheWrapperMajorDoesNotMove()
        {
            // The Cantilever slot is ADDITIVE: an older build reading a file it does not understand skips one
            // unknown key. Bumping the major would make every older build refuse every new file.
            Assert.Equal("2.0", RackProjectDocument.CurrentSchemaVersion);

            var json = new RackProjectStore().Serialize(RackProject.ForCantilever(Design()));

            using var document = JsonDocument.Parse(json);

            Assert.Equal("2.0", document.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("Cantilever", document.RootElement.GetProperty("Kind").GetString());
            Assert.Equal("1.0", document.RootElement
                .GetProperty("Cantilever").GetProperty("SchemaVersion").GetString());
        }

        // ---- 2. the round trip -------------------------------------------------------------------------

        [Fact]
        public void ALineSurvivesSerializeDeserializeAndResolvesToTheSameSignature()
        {
            // The contract of persisting an INTENTION: what comes back resolves to the same physical rack.
            var design = Design(stations: 4, levels: 3);
            var before = Resolve(design);

            Assert.False(before.IsBlocked);

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForCantilever(design)));

            Assert.Equal(RackSystemKind.Cantilever, reloaded.Kind);
            Assert.NotNull(reloaded.CantileverLineDesign);

            var after = Resolve(reloaded.CantileverLineDesign);

            Assert.Equal(before.Signature(), after.Signature());
        }

        [Fact]
        public void TheIdentityAndTheNameSurviveTheRoundTrip()
        {
            var design = Design();
            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForCantilever(design)));

            Assert.Equal(design.Id, reloaded.CantileverLineDesign.Id);
            Assert.Equal("Linea A", reloaded.CantileverLineDesign.Name);
        }

        [Fact]
        public void TheOverridesTravelInTheDocument()
        {
            var design = Design(stations: 3, levels: 2);

            design.ArmCellOverrides.Add(new CantileverArmCellOverride
            {
                StationIndex = 2,
                LevelIndex = 1,
                Side = CantileverArmSide.PositiveY,
                Arm = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign
                    {
                        Arrangement = CantileverArmBodyArrangement.Single,
                        SectionId = ArmHss,
                        CutLength = 60.0
                    },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign
                    {
                        VerticalPunchCount = 2,
                        VerticalEndOffset = 1.5
                    }
                }
            });

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForCantilever(design)));
            var overrides = reloaded.CantileverLineDesign.ArmCellOverrides;

            Assert.Single(overrides);
            Assert.Equal(2, overrides[0].StationIndex);
            Assert.Equal(1, overrides[0].LevelIndex);
            Assert.Equal(CantileverArmSide.PositiveY, overrides[0].Side);
            Assert.Equal(60.0, overrides[0].Arm.Body.CutLength);

            // And the reloaded line really draws the override, not just carries it.
            Assert.Contains(60.0, Resolve(reloaded.CantileverLineDesign).Arms.Select(a => a.Arm.Body.CutLength));
        }

        [Fact]
        public void TheFaceModeIsPersistedAsItsOwnNameAndAnUnknownOneFailsClosed()
        {
            var design = Design();
            design.StationTopology.FaceMode = CantileverStationFaceMode.Double;

            var store = new RackProjectStore();
            var json = store.Serialize(RackProject.ForCantilever(design));

            // A DIRECT enum, written by the shared JsonStringEnumConverter: the member NAME, no special converter.
            Assert.Contains("\"Double\"", json, StringComparison.Ordinal);
            Assert.Equal(
                CantileverStationFaceMode.Double,
                store.Deserialize(json).CantileverLineDesign.StationTopology.FaceMode);

            // A name nobody declared is refused rather than read as Single.
            var tampered = json.Replace("\"Double\"", "\"Triple\"", StringComparison.Ordinal);

            Assert.ThrowsAny<Exception>(() => store.Deserialize(tampered));
        }

        [Fact]
        public void AnUndeclaredFaceModeNumberIsABlockingDiagnosticAndNotSilentlySingle()
        {
            // The other half: a value cast from an int, which no serializer can catch.
            var design = Design();
            design.StationTopology.FaceMode = (CantileverStationFaceMode)99;

            Assert.False(design.TryActiveSides(out var sides));
            Assert.Empty(sides);

            var line = Resolve(design);

            Assert.True(line.IsBlocked);
        }

        [Fact]
        public void UnknownJsonFieldsAndANewerMinorSurviveARewrite()
        {
            var design = Design();
            var store = new RackProjectStore();

            var json = store.Serialize(RackProject.ForCantilever(design))
                .Replace("\"SchemaVersion\": \"1.0\"", "\"SchemaVersion\": \"1.7\",\n    \"DeUnBuildFuturo\": 42",
                    StringComparison.Ordinal);

            var reloaded = store.Deserialize(json);
            var rewritten = store.Serialize(
                RackProject.ForCantilever(reloaded.CantileverLineDesign).WithSourceMetadataFrom(reloaded));

            using var document = JsonDocument.Parse(rewritten);
            var payload = document.RootElement.GetProperty("Cantilever");

            // Never DOWNGRADED, and the field this build does not know is still there (I-11, D3).
            Assert.Equal("1.7", payload.GetProperty("SchemaVersion").GetString());
            Assert.Equal(42, payload.GetProperty("DeUnBuildFuturo").GetInt32());
        }

        [Fact]
        public void AFileFromANewerMajorIsRefusedRatherThanReadWrong()
        {
            var store = new RackProjectStore();
            var json = store.Serialize(RackProject.ForCantilever(Design()))
                .Replace("\"SchemaVersion\": \"1.0\"", "\"SchemaVersion\": \"9.0\"", StringComparison.Ordinal);

            var error = Assert.Throws<InvalidOperationException>(() => store.Deserialize(json));

            Assert.Contains("Cantilever", error.Message, StringComparison.Ordinal);
        }

        // ---- 3. writing FAILS instead of falling back to Selective ------------------------------------

        [Fact]
        public void SavingACantileverWithNoDesignThrowsInsteadOfWritingACabecera()
        {
            // The store's historical fallback re-stamps Kind = Selective and writes a bare header when a payload
            // is missing. For this kind that would turn a programming error into a file that says it is a
            // cabecera: the rack would be gone and nothing would complain until the drawing came out empty.
            var project = RackProject.ForCantilever(null);

            var error = Assert.Throws<InvalidOperationException>(() => new RackProjectStore().Serialize(project));

            Assert.Contains("Cantilever", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void TheOtherKindsKeepTheirFallbackUntouched()
        {
            // The characterization that makes the change above safe: nothing else changed behaviour.
            var json = new RackProjectStore().Serialize(RackProject.ForPushBack(null));

            using var document = JsonDocument.Parse(json);

            Assert.Equal("Selective", document.RootElement.GetProperty("Kind").GetString());
        }

        [Fact]
        public void ADocumentThatDeclaresCantileverWithoutItsPayloadIsRefused()
        {
            var json = JsonSerializer.Serialize(new RackProjectDocument
            {
                Kind = RackSystemKind.Cantilever
            });

            var error = Assert.Throws<InvalidOperationException>(() => new RackProjectStore().Deserialize(json));

            Assert.Contains("Cantilever", error.Message, StringComparison.Ordinal);
        }

        // ---- 4. the usability predicate ---------------------------------------------------------------

        [Fact]
        public void AUsableLineNeedsTwoStationsAPitchALevelASideAndItsTwoSections()
        {
            Assert.True(RackDesignValidation.IsUsableCantilever(Design()));
            Assert.False(RackDesignValidation.IsUsableCantilever(null));

            var oneStation = Design();
            oneStation.StationCount = 1;
            Assert.False(RackDesignValidation.IsUsableCantilever(oneStation));

            var noPitch = Design();
            noPitch.ColumnCentreSpacing = 0.0;
            Assert.False(RackDesignValidation.IsUsableCantilever(noPitch));

            var noLevels = Design();
            noLevels.StationTopology.LevelCount = 0;
            Assert.False(RackDesignValidation.IsUsableCantilever(noLevels));

            var noColumn = Design();
            noColumn.StationTopology.ColumnBaseTemplate.ColumnSectionId = "   ";
            Assert.False(RackDesignValidation.IsUsableCantilever(noColumn));

            var noBase = Design();
            noBase.StationTopology.ColumnBaseTemplate.Base.SectionId = null;
            Assert.False(RackDesignValidation.IsUsableCantilever(noBase));

            var noSide = Design();
            noSide.StationTopology.FaceMode = (CantileverStationFaceMode)99;
            Assert.False(RackDesignValidation.IsUsableCantilever(noSide));
        }

        [Fact]
        public void ThePredicateDoesNotNeedTheSectionCatalogue()
        {
            // A predicate that resolved the line would make "is this design usable?" depend on a catalogue file
            // being present, so a library list would HIDE a rack whose catalogue had moved instead of opening it
            // and reporting why.
            var unknownSections = Design();
            unknownSections.StationTopology.ColumnBaseTemplate.ColumnSectionId = "AISC-W-NO-EXISTE";

            Assert.True(RackDesignValidation.IsUsableCantilever(unknownSections));
            Assert.True(Resolve(unknownSections).IsBlocked);
        }

        // ---- 5. the persisted names are pinned --------------------------------------------------------

        [Fact]
        public void ThePersistedPropertyNamesArePinned()
        {
            // The cost of carrying the design tree itself instead of mirroring it: a domain property RENAMED is a
            // JSON key renamed, which would orphan every saved line. This is the guard that makes that cost
            // payable — a rename fails here instead of in a customer's file.
            var expected = new[]
            {
                "Id", "Name", "StationCount", "ColumnCentreSpacing",
                "StationTopology", "DefaultArmTemplate", "ArmCellOverrides", "Bracing"
            };

            Assert.Equal(expected, PersistedNames(typeof(CantileverLineDesign)));

            Assert.Equal(
                new[]
                {
                    "FaceMode", "SingleSide", "ColumnBaseTemplate", "LevelCount",
                    "FirstLevelPunchIndex", "RequestedClearHeight", "TopClearFactor", "ColumnHeight"
                },
                PersistedNames(typeof(CantileverLineStationTopologyDesign)));

            Assert.Equal(
                new[]
                {
                    "SeparatorSectionId", "PanelCountMode", "ManualPanelCount", "BracedPanelHeight",
                    "CentralEmptySpaceHeight", "BraceKind", "BraceSectionId", "ColdRolled"
                },
                PersistedNames(typeof(CantileverBracingDesign)));

            Assert.Equal(
                new[] { "StationIndex", "LevelIndex", "Side", "Arm" },
                PersistedNames(typeof(CantileverArmCellOverride)));
        }

        [Fact]
        public void TheSeparatorDefaultIsTheExactIdAndNotADesignationLookup()
        {
            Assert.Equal("AISC-C-C4X4_5", CantileverLineDefaults.SeparatorSectionId);
            Assert.Equal("AISC-C-C4X4_5", new CantileverBracingDesign().SeparatorSectionId);

            // And it resolves against the shipped catalogue, so the default is not a guess.
            Assert.True(Catalog.TryGetById(
                StructuralSectionId.Parse(CantileverLineDefaults.SeparatorSectionId), out _));
        }

        /// <summary>How many distinct PIECES of a kind a view draws, however many curves each one takes.</summary>
        private static int Pieces(CantileverViewPlan plan, CantileverViewPieceKind kind) =>
            plan.Of(kind).Select(c => c.PieceId.Value).Distinct(StringComparer.Ordinal).Count();

        private static IReadOnlyList<string> PersistedNames(Type type) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .OrderBy(p => p.MetadataToken)
            .Select(p => p.Name)
            .ToList();

        // ---- 6. the three view plans ------------------------------------------------------------------

        [Fact]
        public void TheInitialSetIsOneFrontalOnePlantaAndONELateral()
        {
            var line = Resolve(Design(stations: 4));
            var plans = CantileverViewPlanBuilder.BuildInitialSet(line, Factory);

            Assert.Equal(3, plans.Count);
            Assert.Equal(CantileverViewKind.Frontal, plans[0].View);
            Assert.Equal(CantileverViewKind.Planta, plans[1].View);
            Assert.Equal(CantileverViewKind.Lateral, plans[2].View);

            // Not four laterals for four stations: a line of twelve would drop twelve blocks nobody asked for.
            Assert.Single(plans.Where(p => p.View == CantileverViewKind.Lateral));
            Assert.Equal(0, plans[2].StationIndex);
        }

        [Fact]
        public void TheWholeLineViewsCarryMinusOneAndTheLateralCarriesItsStation()
        {
            Assert.Equal(-1, CantileverViewPlanBuilder.SectionFor(CantileverViewKind.Frontal, 3));
            Assert.Equal(-1, CantileverViewPlanBuilder.SectionFor(CantileverViewKind.Planta, 3));
            Assert.Equal(3, CantileverViewPlanBuilder.SectionFor(CantileverViewKind.Lateral, 3));

            var line = Resolve(Design(stations: 3));

            Assert.Equal(-1, CantileverViewPlanBuilder
                .Build(line, CantileverViewKind.Frontal, Factory).StationIndex);
            Assert.Equal(2, CantileverViewPlanBuilder
                .Build(line, CantileverViewKind.Lateral, Factory, 2).StationIndex);
        }

        [Fact]
        public void EveryStationHasALateralEvenThoughOnlyOneIsInserted()
        {
            // Inserting one is a decision about the DRAWING. Every station must still be drawable, or changing
            // the selected index would fail for stations nobody had tried.
            var line = Resolve(Design(stations: 4));

            for (var i = 0; i < line.Stations.Count; i++)
            {
                var plan = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Lateral, Factory, i);

                Assert.False(plan.IsEmpty, "La lateral de la estacion " + (i + 1) + " esta vacia.");
                Assert.Equal(i, plan.StationIndex);
                Assert.Empty(plan.Diagnostics);
            }
        }

        [Fact]
        public void ALateralOutsideTheLineIsRefusedAndNotClampedToStationZero()
        {
            var line = Resolve(Design(stations: 2));
            var plan = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Lateral, Factory, 7);

            Assert.True(plan.IsEmpty);
            Assert.NotEmpty(plan.Diagnostics);
        }

        [Fact]
        public void AFrontalShowsEveryStationAndTheBracingAndALateralShowsNeither()
        {
            var line = Resolve(Design(stations: 3));

            var frontal = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Frontal, Factory);
            var lateral = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Lateral, Factory);

            Assert.NotEmpty(frontal.Of(CantileverViewPieceKind.Separator));
            Assert.NotEmpty(frontal.Of(CantileverViewPieceKind.Brace));

            // Counted by PIECE and not by curve: one member's wireframe is several curves — the contour at each
            // end of the run plus the longitudinal edges between them.
            Assert.Equal(3, Pieces(frontal, CantileverViewPieceKind.Column));

            // A lateral looks at ONE station, so the bracing between stations is not part of it.
            Assert.Empty(lateral.Of(CantileverViewPieceKind.Separator));
            Assert.Empty(lateral.Of(CantileverViewPieceKind.Brace));
            Assert.Equal(1, Pieces(lateral, CantileverViewPieceKind.Column));
        }

        [Fact]
        public void AViewPlanIsPUREAndDETERMINISTIC()
        {
            var line = Resolve(Design(stations: 3));

            var a = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Frontal, Factory);
            var b = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Frontal, Factory);

            Assert.Equal(a.Signature(), b.Signature());

            // And a second line built from the same design draws the same picture.
            var again = CantileverViewPlanBuilder.Build(
                Resolve(Design(stations: 3)), CantileverViewKind.Frontal, Factory);

            Assert.Equal(a.Signature(), again.Signature());
        }

        [Fact]
        public void TheFrontalPutsTheLineAcrossAndTheColumnsUp()
        {
            var design = Design(stations: 3);
            var line = Resolve(design);
            var frontal = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Frontal, Factory);

            var bounds = frontal.Bounds;

            // Two pitches wide, and as tall as the columns.
            Assert.True(bounds.Width >= 2 * design.ColumnCentreSpacing);
            Assert.True(bounds.Height >= line.ColumnHeight - 1.0);
        }

        [Fact]
        public void APlantaLooksDownSoTheColumnHeightIsNotInThePicture()
        {
            var design = Design(stations: 3);
            var line = Resolve(design);
            var planta = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Planta, Factory);

            Assert.True(planta.Bounds.Width >= 2 * design.ColumnCentreSpacing);
            Assert.True(
                planta.Bounds.Height < line.ColumnHeight,
                "Una planta no puede ser tan alta como la columna: mira hacia abajo.");
        }

        [Fact]
        public void ABlockedLineDrawsNothingAndSaysWhy()
        {
            var design = Design();
            design.StationTopology.ColumnBaseTemplate.ColumnSectionId = "AISC-W-NO-EXISTE";

            var plan = CantileverViewPlanBuilder.Build(Resolve(design), CantileverViewKind.Frontal, Factory);

            Assert.True(plan.IsEmpty);
            Assert.NotEmpty(plan.Diagnostics);
        }

        [Fact]
        public void EveryCurveKnowsWhichPieceDrewItAndStationScopeKeepsThemApart()
        {
            var line = Resolve(Design(stations: 3));
            var frontal = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Frontal, Factory);

            Assert.All(frontal.Curves, c => Assert.False(c.PieceId.IsEmpty));

            var columns = frontal
                .Of(CantileverViewPieceKind.Column)
                .Select(c => c.PieceId.Value)
                .Distinct()
                .ToList();

            // Three stations, three DISTINCT column ids. Without the station scope they would all be the same id.
            Assert.Equal(3, columns.Count);
            Assert.All(columns, id => Assert.StartsWith("CANT-S", id, StringComparison.Ordinal));
        }

        [Fact]
        public void AColdRolledBraceIsDrawnWithItsPHYSICALWidthAndNotAsABareAxis()
        {
            // ESTE ENUNCIADO SE INVIRTIO, y por decision del dueno
            // (OWNER_REVISED_CANTILEVER_BRACE_VISUAL_REPRESENTATION). Antes se llamaba
            // AColdRolledBraceIsDrawnAsItsAxisAndNotAsAnInventedSection y exigia dos puntos abiertos.
            //
            // La razon de aquella convencion —no poner en el plano una forma que ninguna fila de catalogo
            // respalda— seguia siendo buena, pero el ancho de la banda NO es una seccion inventada: es el
            // DIAMETRO que el diseno ya declaraba, puesto a los dos lados del eje que ya existia. El eje
            // sigue siendo el datum; lo que deja de ser es el dibujo.
            var line = Resolve(Design());
            var frontal = CantileverViewPlanBuilder.Build(line, CantileverViewKind.Frontal, Factory);

            var braces = frontal.Of(CantileverViewPieceKind.Brace).ToList();

            Assert.NotEmpty(braces);

            Assert.All(braces, b =>
            {
                Assert.Equal(4, b.Points.Count);   // dos bordes paralelos y un cierre en cada extremo
                Assert.True(b.IsClosed);
            });

            Assert.NotEmpty(frontal.Of(CantileverViewPieceKind.ColdRolledAdapter));
        }
    }
}
