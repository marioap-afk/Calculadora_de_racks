using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
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
    /// NO cubre C-01, C-07 ni C-12 —las tres ventanas, de G3 (T-17..T-19)—, ni C-05 —cerrado en G4 por T-06—, ni C-15
    /// —la copia compartida del compuesto de Push Back, de G6 (T-08)—.
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
    }
}
