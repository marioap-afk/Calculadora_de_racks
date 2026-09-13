using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-08 (G6) — Push Back por vista, con la política en el DISEÑO y resuelta por el camino productivo.
    ///
    /// <para>
    /// Un sentido: la frontal de entrada-salida y la posterior siguen Frontal, el lateral entero y sus cortes siguen
    /// Lateral y la planta sigue Planta. Compuesto A/B: los CUATRO cortes frontales —entrada-salida y posterior de cada
    /// lado— comparten Frontal; el lateral compuesto, que decora cada lado con su propia sub-estructura, y sus cortes
    /// siguen Lateral; la planta compuesta sigue Planta. La política llega a los dos lados y a la compuesta por C-15
    /// (<c>PushBackCompositeStructure.CopySharedStructuralIntent</c>), exacta e idéntica en A y en B.
    /// </para>
    /// <para>
    /// Una vista encendida es EXACTAMENTE el legacy del mismo diseño sin política, el que fijan los pines de T-05. Una
    /// vista apagada no emite cotas y deja sus etiquetas donde las deja <c>Dimensions = None</c> para esa vista: su
    /// firma entera —cotas, etiquetas y caja de etiquetas— es la de <c>None</c>. El nombre del rack compuesto sigue
    /// saliendo SOLO en la planta: es legacy (T-05) y aquí se preserva, no se corrige.
    /// </para>
    /// </summary>
    public class PushBackDimensionViewsTests
    {
        /// <summary>F, L, P, F|P, 0 y los centinelas 13 (F|P con un bit desconocido) y -8 (bits de signo, las tres apagadas).</summary>
        private static readonly IReadOnlyList<int> ViewPolicies = new[] { 1, 2, 4, 5, 0, 13, -8 };

        public static IEnumerable<object[]> Cases()
            => from level in ActiveLevels
               from policy in ViewPolicies
               select new object[] { level, policy };

        public static IEnumerable<object[]> ExactValues()
            => new int?[] { null, 0, 13, -1, -8 }.Select(value => new object[] { value });

        private static int? AsInt(DimensionViewVisibility? value) => value.HasValue ? (int)value.Value : (int?)null;

        private static PushBackDesign WithPolicy(PushBackDesign design, int? policy)
        {
            design.Structure.DimensionViews = Policy(policy);
            return design;
        }

        /// <summary>
        /// Cada vista contra el legacy (el mismo diseño sin política) y contra <c>None</c> (el mismo diseño sin cotas).
        /// Junta TODAS las vistas que se apartan antes de fallar, para que un fallo diga qué vistas volvieron a legacy.
        /// </summary>
        private static void AssertEveryViewFollowsItsKind(
            int policy, IReadOnlyList<View> views, IReadOnlyList<View> legacyViews, IReadOnlyList<View> noneViews)
        {
            var legacy = legacyViews.ToDictionary(view => view.Key);
            var none = noneViews.ToDictionary(view => view.Key);
            Assert.Equal(legacy.Keys.OrderBy(key => key, StringComparer.Ordinal), views.Select(view => view.Key).OrderBy(key => key, StringComparer.Ordinal));
            Assert.Equal(none.Keys.OrderBy(key => key, StringComparer.Ordinal), views.Select(view => view.Key).OrderBy(key => key, StringComparer.Ordinal));

            var failures = new List<string>();
            foreach (var view in views)
            {
                var on = legacy[view.Key].Instances;
                var off = none[view.Key].Instances;

                // El escenario ejerce lo que se prueba: el legacy tiene cotas y su alcance aparta las etiquetas de None.
                Assert.True(DimensionLegacySignature.DimensionCount(on) > 0, view.Key + ": el legacy de la vista tiene cotas");
                Assert.NotEqual(DimensionLegacySignature.LabelsOnly(off), DimensionLegacySignature.LabelsOnly(on));

                var bitOn = BitIsOn(policy, view.Kind);
                var expected = DimensionLegacySignature.Of(bitOn ? on : off);
                var actual = DimensionLegacySignature.Of(view.Instances);
                if (!string.Equals(expected, actual, StringComparison.Ordinal))
                {
                    failures.Add($"{view.Key} ({view.Kind} {(bitOn ? "encendida: debe ser el legacy" : "apagada: debe ser None")})\n  Expected: {expected}\n  Actual:   {actual}");
                }
            }

            Assert.True(failures.Count == 0, $"política {policy}: {failures.Count} vista(s) no siguen su tipo\n" + string.Join("\n", failures));
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void T08_SingleSided_EntradaSalidaAndPosteriorFollowFrontal_LateralAndCortesLateral_PlantaPlanta(DimensionDetail level, int policy)
        {
            var catalog = Catalog;
            var system = PushBack(WithPolicy(PushBackSingleSidedDesign(level), policy), catalog);
            Assert.False(system.IsComposite);
            Assert.Equal(policy, AsInt(system.Structure.DimensionViews));

            var views = PushBackSingleSidedViews(system, catalog);
            Assert.Equal(new[] { "frontal-entrada-salida", "frontal-posterior" },
                views.Where(view => view.Kind == DimensionViewKind.Frontal).Select(view => view.Key).OrderBy(key => key, StringComparer.Ordinal));
            Assert.Contains(views, view => view.Key == "lateral" && view.Kind == DimensionViewKind.Lateral);
            Assert.True(views.Count(view => view.Key.StartsWith("lateral-corte", StringComparison.Ordinal) && view.Kind == DimensionViewKind.Lateral) >= 2, "el lateral tiene cortes");
            Assert.Single(views, view => view.Kind == DimensionViewKind.Planta);

            AssertEveryViewFollowsItsKind(
                policy,
                views,
                PushBackSingleSidedViews(PushBack(PushBackSingleSidedDesign(level), catalog), catalog),
                PushBackSingleSidedViews(PushBack(PushBackSingleSidedDesign(DimensionDetail.None), catalog), catalog));
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void T08_Composite_TheFourFrontalCutsFollowFrontal_LateralAndCortesLateral_PlantaPlanta(DimensionDetail level, int policy)
        {
            var catalog = Catalog;
            var system = PushBack(WithPolicy(PushBackCompositeDesign(level), policy), catalog);
            Assert.True(system.IsComposite);

            var views = PushBackCompositeViews(system, catalog);
            Assert.Equal(new[] { "frontal-entrada-salida-A", "frontal-entrada-salida-B", "frontal-posterior-A", "frontal-posterior-B" },
                views.Where(view => view.Kind == DimensionViewKind.Frontal).Select(view => view.Key).OrderBy(key => key, StringComparer.Ordinal));
            Assert.Contains(views, view => view.Key == "lateral" && view.Kind == DimensionViewKind.Lateral);
            Assert.True(views.Count(view => view.Key.StartsWith("lateral-corte", StringComparison.Ordinal) && view.Kind == DimensionViewKind.Lateral) >= 2, "el lateral tiene cortes");
            Assert.Single(views, view => view.Kind == DimensionViewKind.Planta);

            AssertEveryViewFollowsItsKind(
                policy,
                views,
                PushBackCompositeViews(PushBack(PushBackCompositeDesign(level), catalog), catalog),
                PushBackCompositeViews(PushBack(PushBackCompositeDesign(DimensionDetail.None), catalog), catalog));

            // Legacy de T-05, preservado con cualquier política: el nombre del rack compuesto sale SOLO en la planta.
            foreach (var view in views)
            {
                var named = view.Instances.Any(instance => instance.Role == HeaderBlockRole.Annotation && instance.Text == RackName);
                Assert.True(named == (view.Key == "planta"), $"{view.Key} con {policy}: el nombre del rack está {(named ? "presente" : "ausente")}");
            }
        }

        /// <summary>
        /// C-15 con el <c>int</c> exacto en sus TRES usos —la sub-estructura de cada lado, la compuesta y la estructura del
        /// lado A que se extrae de un documento compuesto al reabrirlo— y por el resolver productivo: la compuesta y los
        /// dos lados llevan la MISMA política, sin una segunda por lado. <c>null</c> sigue null.
        /// </summary>
        [Theory]
        [MemberData(nameof(ExactValues))]
        public void T08_C15_TheSharedStructuralCopy_CarriesTheExactInt_ToSideA_SideB_AndTheComposite(int? value)
        {
            var catalog = Catalog;
            var design = WithPolicy(PushBackCompositeDesign(DimensionDetail.Standard), value);
            var sideA = PushBackSideConfiguration.ForA(design);
            var sideB = PushBackSideConfiguration.ForB(design);
            var layout = PushBackCompositeStructure.Layout(sideA, sideB, design.Composite);

            Assert.Equal(value, AsInt(PushBackCompositeStructure.SideStructuralDesign(design, sideA, sideB, null).DimensionViews));   // lado A
            Assert.Equal(value, AsInt(PushBackCompositeStructure.SideStructuralDesign(design, sideB, sideA, null).DimensionViews));   // lado B
            Assert.Equal(value, AsInt(PushBackCompositeStructure.Compose(design, sideA, sideB, layout, null, null).DimensionViews)); // compuesta

            var persisted = WithPolicy(PushBackCompositeDesign(DimensionDetail.Standard), value).Structure;
            persisted.Modules.Add(new DynamicRackModuleDesign { ModuleId = PushBackCompositeStructure.GapModuleId });
            var reopenedSideA = PushBackCompositeStructure.SideAStructure(persisted);
            Assert.NotSame(persisted, reopenedSideA);
            Assert.Equal(value, AsInt(reopenedSideA.DimensionViews));                                                                // lado A reabierto

            var system = new PushBackResolver(catalog).Resolve(design);
            Assert.True(system.IsComposite);
            Assert.Equal(value, AsInt(system.Structure.DimensionViews));
            Assert.Equal(value, AsInt(system.Composite.SideA.Local.Structure.DimensionViews));
            Assert.Equal(value, AsInt(system.Composite.SideB.Local.Structure.DimensionViews));
        }
    }
}
