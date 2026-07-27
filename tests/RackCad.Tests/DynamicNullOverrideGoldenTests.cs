using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004, consumidores (I-32) — el centinela del Dinámico.
    ///
    /// Los builders compartidos ganan un contexto de elevaciones como ÚLTIMO parámetro OPCIONAL. Con el valor por
    /// defecto (null) su ejecución tiene que ser BYTE-IDÉNTICA a la de antes: el Dinámico no lo pasa nunca, así que
    /// no puede notar el cambio.
    ///
    /// Estos pines se capturaron sobre <c>db437c7</c>, ANTES de introducir el contexto, sobre un escenario jagged
    /// —frentes con distinto número de niveles, distinta profundidad y distinta altura de primer nivel— que es
    /// justo donde las tres consultas del contexto darían resultados distintos si alguna se colara. Si uno de
    /// estos pines se mueve, el override dejó de ser opt-in y se filtró al Dinámico.
    ///
    /// NO se re-fijan por conveniencia: un cambio aquí es un fallo, no un valor nuevo que apuntar.
    /// </summary>
    public class DynamicNullOverrideGoldenTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem Scenario(RackCatalog catalog)
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0,
                NumberLevels = true,
                NumberFronts = true,
                DrawRackName = true,
                Dimensions = DimensionDetail.Detailed
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 4.0 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1, FirstLevelHeight = 12.0 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 8.0 });
            foreach (var family in DynamicSafetyDefaults.Build(catalog))
            {
                design.SafetySelections.Add(family);
            }

            return new DynamicRackSystemResolver(catalog).Resolve(design).System;
        }

        private static string Row(HeaderBlockInstance i)
            => FormattableString.Invariant(
                $"{i.View}|{i.Role}|{i.PieceId}|{i.BlockName}|{i.Text}|{i.Insertion.X:0.####}|{i.Insertion.Y:0.####}|{i.ConnectionAnchor.X:0.####}|{i.ConnectionAnchor.Y:0.####}|{i.RotationRadians:0.######}|{(i.MirroredX ? 1 : 0)}|{(i.MirroredY ? 1 : 0)}|{i.DimensionOffset:0.####}|{i.TextHeight:0.####}|{Params(i)}");

        private static string Params(HeaderBlockInstance i)
            => string.Join(",", i.DynamicParameters.OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => FormattableString.Invariant($"{p.Key}={p.Value:0.####}")));

        private static string PlanSignature(HeaderRunPlan plan)
            => string.Join("\n", plan.Flatten().Instances.Select(Row).OrderBy(s => s, StringComparer.Ordinal));

        private static string Sha(string content)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

        private static Dictionary<string, string> Signatures()
        {
            var catalog = Catalog;
            var system = Scenario(catalog);
            var lateral = new DynamicSystemLateralBuilder();
            var frontal = new DynamicSystemFrontalBuilder();
            return new Dictionary<string, string>
            {
                ["lateral"] = Sha(PlanSignature(lateral.Build(system, catalog))),
                ["lateral-corte0"] = Sha(PlanSignature(lateral.Build(system, catalog, 0))),
                ["lateral-corte1"] = Sha(PlanSignature(lateral.Build(system, catalog, 1))),
                ["lateral-corte2"] = Sha(PlanSignature(lateral.Build(system, catalog, 2))),
                ["lateral-corte3"] = Sha(PlanSignature(lateral.Build(system, catalog, 3))),
                ["frontal-salida"] = Sha(PlanSignature(frontal.BuildPlan(system, catalog, DynamicRackEnd.Exit))),
                ["frontal-entrada"] = Sha(PlanSignature(frontal.BuildPlan(system, catalog, DynamicRackEnd.Entrance)))
            };
        }

        // Pines capturados sobre db437c7, antes de que existiera el contexto de elevaciones.
        private static readonly Dictionary<string, string> Pins = new Dictionary<string, string>
        {
            ["lateral"] = "4FA31B4E7432DA20A672D05344B11E6F1928D8D3D6FE1BFD9D45B76F7CD46720",
            ["lateral-corte0"] = "E51BD9CBDB9378958682AAEC191F00A273F9216D3A547EC9AAB52ED4E2E61FB9",
            ["lateral-corte1"] = "659F416C8F7464852929CC7D988F35C2782F39F81D985BD6575CCD6D08B058AE",
            ["lateral-corte2"] = "A4C0BB0AE79D12CF10F5E97488176A873AD1D826E33436D94A49F211BEE25D66",
            ["lateral-corte3"] = "38C9D3B891779DC3ECD30459D0DFD119EF3EDA475DDD067BA82A84B2E7CE532F",
            ["frontal-salida"] = "971812FF30592371B408008D3E09945F5FD96F92EB2679F4E4A252BDEB146109",
            ["frontal-entrada"] = "9638490702D44C342E7F822961826EA21434EE512511F8A314BD9B16AAE15815"
        };

        [Fact]
        public void TheDynamicPlans_StayByteIdentical_WhenNoElevationOverrideIsPassed()
        {
            var actual = Signatures();
            Assert.Equal(Pins.Count, actual.Count);
            var drift = new List<string>();
            foreach (var pin in Pins)
            {
                Assert.True(actual.TryGetValue(pin.Key, out var got), $"falta la firma '{pin.Key}'");
                if (!string.Equals(pin.Value, got, StringComparison.Ordinal))
                {
                    drift.Add($"[\"{pin.Key}\"] = \"{got}\",");
                }
            }

            Assert.True(drift.Count == 0,
                "el override dejó de ser opt-in y se filtró al Dinámico; firmas movidas: " + string.Join(" ", drift));
        }
    }
}
