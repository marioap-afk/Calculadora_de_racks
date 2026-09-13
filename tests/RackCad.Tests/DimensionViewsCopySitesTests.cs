using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-13 (G5) — el contrato de los sitios de copia: cada uno, por separado, transporta el <c>int</c> EXACTO.
    /// Se usan centinelas con bits desconocidos y de signo (<c>-8</c>) y con un bit desconocido y dos conocidos
    /// (<c>13</c>), de modo que una máscara o una normalización en cualquier sitio se detecta; y <c>null</c> sigue
    /// siendo null, que es el legacy.
    ///
    /// <para>
    /// Cubre C-02, C-03, C-04, C-06 (Selectivo), C-08, C-09, C-10, C-11 (Dinámico), C-13 y C-14 (Push Back). NO cubre
    /// C-01, C-07 ni C-12 —las tres ventanas, de G3 (T-17..T-19)—, ni C-05 —cerrado en G4 por T-06—, ni C-15 —la copia
    /// compartida del compuesto de Push Back, de G6 (T-08)—.
    /// </para>
    /// </summary>
    public class DimensionViewsCopySitesTests
    {
        public static IEnumerable<object[]> Sentinels()
        {
            yield return new object[] { -8 };
            yield return new object[] { 13 };
        }

        private static int? AsInt(DimensionViewVisibility? value) => value.HasValue ? (int)value.Value : (int?)null;

        // ---- Selectivo ------------------------------------------------------------------------------------------

        private static SelectiveDesignInputs SelectiveInputs(DimensionViewVisibility? policy)
            => new SelectiveDesignInputs
            {
                PostId = TestCatalogIds.Profiles.Posts.Standard,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                Fondo = 48.0,
                DepthCount = 1,
                WorkingDepth = 48.0,
                DrawBasePlate = true,
                AnnotationScale = 1.0,
                Dimensions = DimensionDetail.Standard,
                DimensionViews = policy
            };

        private static SelectiveEditorState OpenedSelectiveState()
        {
            var state = new SelectiveEditorState { DefaultBeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet };
            state.InitMatrix(2, 3);
            state.FondoMatrices.Add(state.SnapshotWorking(48.0, 0.0));
            state.SelectedFondo = 0;
            return state;
        }

        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C02_SelectiveDesignInputs_HoldsTheExactInt(int sentinel)
        {
            Assert.Equal(sentinel, AsInt(SelectiveInputs((DimensionViewVisibility)sentinel).DimensionViews));
            Assert.Null(SelectiveInputs(null).DimensionViews);
        }

        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C03_SelectiveEditorState_BuildDesign_CopiesTheInputsExactly(int sentinel)
        {
            var design = OpenedSelectiveState().BuildDesign(SelectiveInputs((DimensionViewVisibility)sentinel));
            Assert.NotNull(design);
            Assert.Equal(sentinel, AsInt(design.DimensionViews));

            Assert.Null(OpenedSelectiveState().BuildDesign(SelectiveInputs(null)).DimensionViews);
        }

        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C04_SelectiveGeometryResolver_CopiesTheDesignExactly(int sentinel)
        {
            var design = SelectiveDesign(DimensionDetail.Standard, twoFondos: false);
            design.DimensionViews = (DimensionViewVisibility)sentinel;
            Assert.Equal(sentinel, AsInt(new SelectiveGeometryResolver().Resolve(design, Catalog).DimensionViews));

            design.DimensionViews = null;
            Assert.Null(new SelectiveGeometryResolver().Resolve(design, Catalog).DimensionViews);
        }

        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C06_SelectivePalletDesignDocument_MapsBothDirectionsExactly(int sentinel)
        {
            var design = SelectiveDesign(DimensionDetail.Standard, twoFondos: false);
            design.DimensionViews = (DimensionViewVisibility)sentinel;
            Assert.Equal(sentinel, SelectivePalletDesignDocument.From(design, "I50-ID", RackName).DimensionViews);   // From

            design.DimensionViews = null;
            var document = SelectivePalletDesignDocument.From(design, "I50-ID", RackName);
            Assert.Null(document.DimensionViews);
            document.DimensionViews = sentinel;
            Assert.Equal(sentinel, AsInt(document.ToDomain().DimensionViews));                                         // ToDomain
            document.DimensionViews = null;
            Assert.Null(document.ToDomain().DimensionViews);
        }

        // ---- Dinámico ------------------------------------------------------------------------------------------

        private const string DynamicPostId = "POSTE_OMEGA_3X3";

        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C08_DynamicAnnotationOptions_HoldsTheExactInt(int sentinel)
        {
            Assert.Equal(sentinel, AsInt(new DynamicAnnotationOptions { DimensionViews = (DimensionViewVisibility)sentinel }.DimensionViews));
            Assert.Null(new DynamicAnnotationOptions().DimensionViews);
        }

        /// <summary>
        /// C-09 copia las opciones DESPUÉS del snapshot, como hace con <c>Dimensions</c>: lo que el editor declara manda
        /// sobre lo que traía el sistema, y unas opciones sin política dejan el diseño en legacy.
        /// </summary>
        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C09_DynamicEditorDesignAssembler_BuildDesign_CopiesTheOptionsExactly_AfterTheSnapshot(int sentinel)
        {
            var catalog = Catalog;
            var builder = new DynamicRackSystemBuilder(catalog);
            var assembler = new DynamicEditorDesignAssembler(catalog, builder, new DynamicRackSystemResolver(catalog));
            var system = builder.BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"), 5, RackFrameTemplateCatalog.Default, DynamicPostId, 132.0, 3.0);
            system.DimensionViews = (DimensionViewVisibility)(sentinel == 13 ? -8 : 13);

            DynamicRackDesign Build(DynamicAnnotationOptions options)
                => assembler.BuildDesign(
                    system, new DynamicFrontMatrix(),
                    levels: 3, firstLevel: 6.0, beamDepth: DynamicRackDefaults.DefaultBeamDepth,
                    headerPostCatalogId: DynamicPostId,
                    palletsDeep: 5, postPeralte: 3.0, palletTolerance: DynamicRackDefaults.DefaultPalletTolerance,
                    annotations: options, safetySelections: null);

            Assert.Equal(sentinel, AsInt(Build(new DynamicAnnotationOptions { DimensionViews = (DimensionViewVisibility)sentinel }).DimensionViews));
            Assert.Null(Build(new DynamicAnnotationOptions()).DimensionViews);
        }

        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C10_DynamicRackSystemResolver_CopiesExactly_DesignToSystem_AndSystemToDesign(int sentinel)
        {
            var catalog = Catalog;
            var resolver = new DynamicRackSystemResolver(catalog);

            var design = DynamicDesign(DimensionDetail.Standard, catalog);
            design.DimensionViews = (DimensionViewVisibility)sentinel;
            Assert.Equal(sentinel, AsInt(resolver.Resolve(design).System.DimensionViews));                    // diseño → sistema

            var legacyDesign = DynamicDesign(DimensionDetail.Standard, catalog);
            var system = resolver.Resolve(legacyDesign).System;
            Assert.Null(system.DimensionViews);
            system.DimensionViews = (DimensionViewVisibility)sentinel;
            var snapshot = resolver.Snapshot(
                system, legacyDesign.LoadLevels, legacyDesign.FirstLevelHeight, legacyDesign.BeamDepth, legacyDesign.HeaderPostCatalogId);
            Assert.Equal(sentinel, AsInt(snapshot.DimensionViews));                                            // sistema → diseño
        }

        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C11_DynamicRackSystemDocument_TheFourMappingsCarryTheExactInt(int sentinel)
        {
            var catalog = Catalog;
            var design = DynamicDesign(DimensionDetail.Standard, catalog);
            design.DimensionViews = (DimensionViewVisibility)sentinel;
            Assert.Equal(sentinel, DynamicRackSystemDocument.From(design).DimensionViews);                   // From(design)

            var system = Dynamic(DimensionDetail.Standard, catalog);
            system.DimensionViews = (DimensionViewVisibility)sentinel;
            Assert.Equal(sentinel, DynamicRackSystemDocument.From(system).DimensionViews);                   // From(system)

            var document = DynamicRackSystemDocument.From(DynamicDesign(DimensionDetail.Standard, catalog));
            Assert.Null(document.DimensionViews);
            document.DimensionViews = sentinel;
            Assert.Equal(sentinel, AsInt(document.ToDesign().DimensionViews));                               // ToDesign
            Assert.Equal(sentinel, AsInt(document.ToDomain().DimensionViews));                               // ToDomain
        }

        // ---- Push Back ------------------------------------------------------------------------------------------

        private static PushBackDesign PushBackDesignWith(DimensionViewVisibility? policy)
        {
            var design = PushBackSingleSidedDesign(DimensionDetail.Standard);
            design.Structure.DimensionViews = policy;
            return design;
        }

        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C13_PushBackEditorState_Load_RecoversThePolicyIntoTheAnnotations(int sentinel)
        {
            var resolver = new PushBackResolver(Catalog);
            var inputs = new PushBackEditorState().LoadFromDesign(PushBackDesignWith((DimensionViewVisibility)sentinel), resolver);
            Assert.Equal(sentinel, AsInt(inputs.Annotations.DimensionViews));

            Assert.Null(new PushBackEditorState().LoadFromDesign(PushBackDesignWith(null), resolver).Annotations.DimensionViews);
        }

        /// <summary>
        /// Por qué importa C-13: el editor de Push Back guarda sus anotaciones por C-09, así que reabrir un rack y
        /// guardarlo sin tocar nada perdería la política si la carga no la devolviera a las opciones.
        /// </summary>
        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C13_ThePushBackEditor_ReopenedAndSavedUntouched_KeepsTheExactInt(int sentinel)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);

            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(PushBackDesignWith((DimensionViewVisibility)sentinel), assembler.Resolver);
            Assert.Equal(sentinel, AsInt(assembler.BuildDesign(state, inputs).Structure.DimensionViews));

            var legacyState = new PushBackEditorState();
            var legacyInputs = legacyState.LoadFromDesign(PushBackDesignWith(null), assembler.Resolver);
            Assert.Null(assembler.BuildDesign(legacyState, legacyInputs).Structure.DimensionViews);
        }

        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C14_PushBackMirror_TheReflectedAndTheClonedStructure_CarryTheExactInt(int sentinel)
        {
            var source = PushBackSingleSided(DimensionDetail.Standard, Catalog).Structure;
            source.DimensionViews = (DimensionViewVisibility)sentinel;

            Assert.Equal(sentinel, AsInt(PushBackMirror.Structure(source).DimensionViews));                          // una reflexión
            Assert.Equal(sentinel, AsInt(PushBackMirror.Structure(source, index => index == 0).DimensionViews));   // re-declarando ranuras
            Assert.Equal(sentinel, AsInt(PushBackMirror.Clone(source).DimensionViews));                              // dos reflexiones

            source.DimensionViews = null;
            Assert.Null(PushBackMirror.Structure(source).DimensionViews);
            Assert.Null(PushBackMirror.Clone(source).DimensionViews);
        }

        /// <summary>
        /// <c>PushBackRuns.Clone</c> es privado y se alcanza por <c>BuildCorrida</c>: A→B resuelve la corrida sobre ese
        /// clon (dos reflexiones) y B→A sobre una reflexión. La política se asigna al sistema compuesto YA resuelto,
        /// porque la copia compartida que la llevaría desde el diseño es C-15, de G6.
        /// </summary>
        [Theory]
        [MemberData(nameof(Sentinels))]
        public void T13_C14_PushBackRuns_BothCorridaFrames_CarryTheExactInt(int sentinel)
        {
            var design = PushBackCompositeStructureTests.Composite(slotsA: 1, slotsB: 1, deepA: 5, deepB: 8, levelsA: 1, levelsB: 1);
            design.Composite.DefaultTopology = PushBackCellTopology.Corrida;
            var system = new PushBackResolver(Catalog).Resolve(design);

            system.Structure.DimensionViews = (DimensionViewVisibility)sentinel;
            Assert.Equal(sentinel, AsInt(PushBackRuns.BuildCorrida(system, PushBackRunDirection.AToB, 10).Structure.DimensionViews));   // PushBackRuns.Clone
            Assert.Equal(sentinel, AsInt(PushBackRuns.BuildCorrida(system, PushBackRunDirection.BToA, 10).Structure.DimensionViews));   // PushBackMirror.Structure

            system.Structure.DimensionViews = null;
            Assert.Null(PushBackRuns.BuildCorrida(system, PushBackRunDirection.AToB, 10).Structure.DimensionViews);
            Assert.Null(PushBackRuns.BuildCorrida(system, PushBackRunDirection.BToA, 10).Structure.DimensionViews);
        }
    }
}
