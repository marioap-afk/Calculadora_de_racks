using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-16 (G6, guarda) — la política decide QUÉ cotas se emiten y, con ellas, dónde caen las etiquetas de una vista
    /// apagada (ADR-0035). Nada más.
    ///
    /// <para>
    /// Para cada política, en los tres sistemas —Selectivo de dos fondos, Dinámico y Push Back de un sentido y compuesto
    /// A/B—, con la política en el DISEÑO y resuelta por el camino productivo: toda instancia que no es cota ni etiqueta
    /// es idéntica a la del legacy, fila a fila con posición, giro, espejo y parámetros; las etiquetas son las mismas
    /// —texto, altura, giro, espejo— y solo puede cambiar su POSICIÓN; y el BOM es el mismo, componente a componente y
    /// línea a línea, en el mismo orden.
    /// </para>
    /// </summary>
    public class DimensionViewsPhysicalAndBomInvarianceTests
    {
        private const string SelectiveScenario = "selectivo-2-fondos";
        private const string DynamicScenario = "dinamico";
        private const string PushBackSingleSidedScenario = "pushback-un-sentido";
        private const string PushBackCompositeScenario = "pushback-compuesto";

        /// <summary>null (legacy), 0, F, L, P, F|P, 7 y los centinelas 13 y -8.</summary>
        private static readonly IReadOnlyList<int?> GuardPolicies = new int?[] { null, 0, 1, 2, 4, 5, 7, 13, -8 };

        private static readonly IReadOnlyList<string> Scenarios = new[]
        {
            SelectiveScenario, DynamicScenario, PushBackSingleSidedScenario, PushBackCompositeScenario
        };

        public static IEnumerable<object[]> Cases()
            => from scenario in Scenarios
               from policy in GuardPolicies
               select new object[] { scenario, policy };

        private static IReadOnlyList<View> ViewsOf(string scenario, DimensionDetail level, int? policy, RackCatalog catalog)
        {
            switch (scenario)
            {
                case SelectiveScenario:
                    return SelectiveViews(ResolveSelective(level, policy, catalog), catalog);
                case DynamicScenario:
                    return DynamicViews(ResolveDynamic(level, policy, catalog), catalog);
                case PushBackSingleSidedScenario:
                    return PushBackSingleSidedViews(ResolvePushBack(PushBackSingleSidedDesign(level), policy, catalog), catalog);
                default:
                    return PushBackCompositeViews(ResolvePushBack(PushBackCompositeDesign(level), policy, catalog), catalog);
            }
        }

        private static BillOfMaterials BomOf(string scenario, DimensionDetail level, int? policy, RackCatalog catalog)
        {
            switch (scenario)
            {
                case SelectiveScenario:
                    return SelectiveBomBuilder.Build(ResolveSelective(level, policy, catalog), catalog);
                case DynamicScenario:
                    return SystemBomBuilder.Build(ResolveDynamic(level, policy, catalog), catalog);
                case PushBackSingleSidedScenario:
                    return PushBackBomBuilder.Build(ResolvePushBack(PushBackSingleSidedDesign(level), policy, catalog), catalog);
                default:
                    return PushBackBomBuilder.Build(ResolvePushBack(PushBackCompositeDesign(level), policy, catalog), catalog);
            }
        }

        private static SelectiveRackSystem ResolveSelective(DimensionDetail level, int? policy, RackCatalog catalog)
        {
            var design = SelectiveDesign(level, twoFondos: true);
            design.DimensionViews = Policy(policy);
            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            system.Name = RackName;
            return system;
        }

        private static DynamicRackSystem ResolveDynamic(DimensionDetail level, int? policy, RackCatalog catalog)
        {
            var design = DynamicDesign(level, catalog);
            design.DimensionViews = Policy(policy);
            var system = new DynamicRackSystemResolver(catalog).Resolve(design).System;
            system.Name = RackName;
            return system;
        }

        private static PushBackSystem ResolvePushBack(PushBackDesign design, int? policy, RackCatalog catalog)
        {
            design.Structure.DimensionViews = Policy(policy);
            return PushBack(design, catalog);
        }

        /// <summary>La geometría física de una vista y sus etiquetas SIN posición. Las cotas quedan fuera.</summary>
        private static string Physical(IEnumerable<HeaderBlockInstance> instances)
            => string.Join("\n", instances
                .Where(instance => instance.Role != HeaderBlockRole.Dimension)
                .Select(instance => instance.Role == HeaderBlockRole.Annotation ? LabelRow(instance) : PieceRow(instance))
                .OrderBy(row => row, StringComparer.Ordinal));

        private static string PieceRow(HeaderBlockInstance i)
            => FormattableString.Invariant(
                $"{i.View}|{i.Role}|{i.PieceId}|{i.BlockName}|{i.Text}|{i.Insertion.X:0.####}|{i.Insertion.Y:0.####}|{i.ConnectionAnchor.X:0.####}|{i.ConnectionAnchor.Y:0.####}|{i.RotationRadians:0.######}|{(i.MirroredX ? 1 : 0)}|{(i.MirroredY ? 1 : 0)}|{i.DimensionOffset:0.####}|{i.TextHeight:0.####}|{i.DimensionStyleName}|{Params(i)}");

        /// <summary>Una etiqueta sin <c>Insertion</c> ni <c>ConnectionAnchor</c>: lo único que su alcance puede mover.</summary>
        private static string LabelRow(HeaderBlockInstance i)
            => FormattableString.Invariant(
                $"{i.View}|{i.Role}|{i.PieceId}|{i.BlockName}|{i.Text}|{i.RotationRadians:0.######}|{(i.MirroredX ? 1 : 0)}|{(i.MirroredY ? 1 : 0)}|{i.TextHeight:0.####}|{i.DimensionStyleName}|{Params(i)}");

        private static string Params(HeaderBlockInstance i)
            => string.Join(",", i.DynamicParameters.OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => FormattableString.Invariant($"{p.Key}={p.Value:0.####}")));

        /// <summary>El BOM entero, en su orden: cada componente con su receta y cada línea del total de piezas.</summary>
        private static string BomSignature(BillOfMaterials bom)
            => string.Join("\n", bom.Components
                .Select(c => FormattableString.Invariant($"C|{c.Category}|{c.ProfileId}|{c.Description}|{c.Length:0.####}|{c.Quantity}|")
                             + string.Join(";", c.Pieces.Select(Line)))
                .Concat(bom.Lines.Select(line => "L|" + Line(line))));

        private static string Line(BomLine line)
            => FormattableString.Invariant($"{line.Category}|{line.ProfileId}|{line.Description}|{line.Length:0.####}|{line.Quantity}");

        [Theory]
        [MemberData(nameof(Cases))]
        public void T16_ThePhysicalGeometry_IsIdenticalForAnyPolicy_OnlyCotasAndLabelPositionsMayChange(string scenario, int? policy)
        {
            var catalog = Catalog;
            foreach (var level in ActiveLevels)
            {
                var legacy = ViewsOf(scenario, level, null, catalog).ToDictionary(view => view.Key);
                var views = ViewsOf(scenario, level, policy, catalog);
                Assert.Equal(legacy.Keys.OrderBy(key => key, StringComparer.Ordinal), views.Select(view => view.Key).OrderBy(key => key, StringComparer.Ordinal));

                foreach (var view in views)
                {
                    var before = legacy[view.Key].Instances;
                    Assert.Contains(before, instance => instance.Role != HeaderBlockRole.Dimension && instance.Role != HeaderBlockRole.Annotation);
                    Assert.True(
                        string.Equals(Physical(before), Physical(view.Instances), StringComparison.Ordinal),
                        $"{scenario}/{level}/{view.Key} con política {policy?.ToString() ?? "null"}: cambió algo que no es cota ni posición de etiqueta");
                }
            }
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void T16_TheBom_IsIdenticalForAnyPolicy(string scenario, int? policy)
        {
            var catalog = Catalog;
            foreach (var level in ActiveLevels)
            {
                var legacy = BomOf(scenario, level, null, catalog);
                Assert.True(legacy.Lines.Count > 0, $"{scenario}/{level}: el BOM legacy tiene líneas");

                var bom = BomOf(scenario, level, policy, catalog);
                Assert.True(
                    string.Equals(BomSignature(legacy), BomSignature(bom), StringComparison.Ordinal),
                    $"{scenario}/{level} con política {policy?.ToString() ?? "null"}: el BOM cambió");
            }
        }
    }
}
