using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.Application.Units;
using Xunit;
using Xunit.Abstractions;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T2 — the equivalence of data: the payload DECODED from the script of REF-CANT-01 reproduces the golden of the analysis
    /// program (section 8 of the F3 document) with its tolerances. Every golden number below is a LITERAL copied from that document; none is
    /// computed with the code under test. The material rows are FIXTURES.
    /// </summary>
    public class StructuralAnalysisRamElementsPackageGoldenTests
    {
        private const double Coordinate = 1e-9;
        private const double Component = 1e-12;
        private const double Angle = 1e-9;
        private const double Percent = 1e-9;

        private readonly ITestOutputHelper _output;
        private double _largest;

        public StructuralAnalysisRamElementsPackageGoldenTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // Section 8, first table: node, X_RAM, Y_RAM, Z_RAM, flags (dx dy dz rx ry rz).
        private static readonly (string Id, double X, double Y, double Z, bool[] Flags)[] GoldenNodes =
        {
            ("COL.FOOT", 0.0, 0.25, 6.1, new[] { true, true, true, true, true, true }),
            ("ARM.1.PositiveY.J", 0.0, 20.26716169689737, 6.1, new[] { true, false, false, false, true, true }),
            ("ARM.2.PositiveY.J", 0.0, 48.267161696897375, 6.1, new[] { true, false, false, false, true, true }),
            ("ARM.3.PositiveY.J", 0.0, 76.26716169689738, 6.1, new[] { true, false, false, false, true, true }),
            ("COL.TOP", 0.0, 90.75, 6.1, new[] { true, false, false, false, true, true }),
            ("ARM.1.PositiveY.TIP", 0.0, 21.810300688090436, -36.22609804415268, new[] { true, false, false, false, true, true }),
            ("ARM.2.PositiveY.TIP", 0.0, 49.81030068809044, -36.22609804415268, new[] { true, false, false, false, true, true }),
            ("ARM.3.PositiveY.TIP", 0.0, 77.81030068809044, -36.22609804415268, new[] { true, false, false, false, true, true }),
        };

        // Section 8, second table: member, node I, node J, section, material, theta, l1, l2, l3.
        private static readonly (string Id, string I, string J, string Section, string Material, double Theta, double[] L1, double[] L2, double[] L3)[] GoldenMembers =
        {
            ("COL.1", "COL.FOOT", "ARM.1.PositiveY.J", "W 12X26", "A992 Gr50", -90.0, new[] { 0.0, 1.0, 0.0 }, new[] { 0.0, 0.0, -1.0 }, new[] { -1.0, 0.0, 0.0 }),
            ("COL.2", "ARM.1.PositiveY.J", "ARM.2.PositiveY.J", "W 12X26", "A992 Gr50", -90.0, new[] { 0.0, 1.0, 0.0 }, new[] { 0.0, 0.0, -1.0 }, new[] { -1.0, 0.0, 0.0 }),
            ("COL.3", "ARM.2.PositiveY.J", "ARM.3.PositiveY.J", "W 12X26", "A992 Gr50", -90.0, new[] { 0.0, 1.0, 0.0 }, new[] { 0.0, 0.0, -1.0 }, new[] { -1.0, 0.0, 0.0 }),
            ("COL.4", "ARM.3.PositiveY.J", "COL.TOP", "W 12X26", "A992 Gr50", -90.0, new[] { 0.0, 1.0, 0.0 }, new[] { 0.0, 0.0, -1.0 }, new[] { -1.0, 0.0, 0.0 }),
            ("ARM.1.PositiveY", "ARM.1.PositiveY.J", "ARM.1.PositiveY.TIP", "HSS_SQR 4X4X1_4", "A500 GrC rectangular", 0.0, new[] { 0.0, 0.036434127070177715, -0.9993360567820188 }, new[] { 0.0, 0.9993360567820188, 0.036434127070177715 }, new[] { 1.0, 0.0, 0.0 }),
            ("ARM.2.PositiveY", "ARM.2.PositiveY.J", "ARM.2.PositiveY.TIP", "HSS_SQR 4X4X1_4", "A500 GrC rectangular", 0.0, new[] { 0.0, 0.036434127070177715, -0.9993360567820188 }, new[] { 0.0, 0.9993360567820188, 0.036434127070177715 }, new[] { 1.0, 0.0, 0.0 }),
            ("ARM.3.PositiveY", "ARM.3.PositiveY.J", "ARM.3.PositiveY.TIP", "HSS_SQR 4X4X1_4", "A500 GrC rectangular", 0.0, new[] { 0.0, 0.036434127070177715, -0.9993360567820188 }, new[] { 0.0, 0.9993360567820188, 0.036434127070177715 }, new[] { 1.0, 0.0, 0.0 }),
        };

        // Section 8, third table: load, member, value in lbf (the third is computed from the unit authority, as the document says), position in percent.
        private static readonly (string Id, string Member, double Value, double Position)[] GoldenLoads =
        {
            ("P1", "ARM.1.PositiveY", -1000.0, 57.50128206169155),
            ("P2", "ARM.2.PositiveY", -2000.0, 36.25192309253731),
            ("P3", "ARM.3.PositiveY", -ForceUnits.ToPoundsForce(500, ForceUnit.KilogramForce), 78.75064103084577),
        };

        private JsonElement Payload() => I.PayloadOf(I.Package()).RootElement;

        private void Near(double expected, double actual, double tolerance, string label)
        {
            var difference = Math.Abs(expected - actual);

            _largest = Math.Max(_largest, difference);
            Assert.True(difference <= tolerance, label + ": esperado " + expected.ToString("R") + ", carga util " + actual.ToString("R") + ", diferencia " + difference.ToString("R"));
        }

        private void NearVector(double[] expected, JsonElement actual, double tolerance, string label)
        {
            Assert.Equal(3, actual.GetArrayLength());

            for (var index = 0; index < 3; index++)
            {
                Near(expected[index], actual[index].GetDouble(), tolerance, label + "[" + index + "]");
            }
        }

        [Fact]
        public void LosNudosDeLaCargaUtilDecodificadaDelScriptSonLosDelGoldenDeRam()
        {
            var nodes = Payload().GetProperty("nodes").EnumerateArray().ToList();

            Assert.Equal(GoldenNodes.Select(n => n.Id).OrderBy(i => i, StringComparer.Ordinal), nodes.Select(n => n.GetProperty("id").GetString()));

            foreach (var golden in GoldenNodes)
            {
                var node = nodes.Single(n => n.GetProperty("id").GetString() == golden.Id);

                Near(golden.X, node.GetProperty("x").GetDouble(), Coordinate, golden.Id + " x");
                Near(golden.Y, node.GetProperty("y").GetDouble(), Coordinate, golden.Id + " y");
                Near(golden.Z, node.GetProperty("z").GetDouble(), Coordinate, golden.Id + " z");
                Assert.Equal(golden.Flags, node.GetProperty("restraints").EnumerateArray().Select(e => e.GetBoolean()));
            }

            _output.WriteLine("Mayor diferencia (nudos): " + _largest.ToString("R"));
        }

        [Fact]
        public void LosMiembrosDeLaCargaUtilDecodificadaDelScriptSonLosDelGoldenDeRam()
        {
            var members = Payload().GetProperty("members").EnumerateArray().ToList();

            Assert.Equal(GoldenMembers.Select(m => m.Id).OrderBy(i => i, StringComparer.Ordinal), members.Select(m => m.GetProperty("id").GetString()));

            foreach (var golden in GoldenMembers)
            {
                var member = members.Single(m => m.GetProperty("id").GetString() == golden.Id);

                Assert.Equal(golden.I, member.GetProperty("nodeI").GetString());
                Assert.Equal(golden.J, member.GetProperty("nodeJ").GetString());
                Assert.Equal(golden.Section, member.GetProperty("section").GetString());
                Assert.Equal(golden.Material, member.GetProperty("material").GetString());
                Near(golden.Theta, member.GetProperty("rotation").GetDouble(), Angle, golden.Id + " giro");
                NearVector(golden.L1, member.GetProperty("l1"), Component, golden.Id + " l1");
                NearVector(golden.L2, member.GetProperty("l2"), Component, golden.Id + " l2");
                NearVector(golden.L3, member.GetProperty("l3"), Component, golden.Id + " l3");
            }

            _output.WriteLine("Mayor diferencia (miembros): " + _largest.ToString("R"));
        }

        [Fact]
        public void LasCargasDeLaCargaUtilDecodificadaDelScriptSonLasDelGoldenDeRam()
        {
            var loads = Payload().GetProperty("loads").EnumerateArray().ToList();

            Assert.Equal(GoldenLoads.Select(l => l.Id), loads.Select(l => l.GetProperty("id").GetString()));

            foreach (var golden in GoldenLoads)
            {
                var load = loads.Single(l => l.GetProperty("id").GetString() == golden.Id);

                Assert.Equal(golden.Member, load.GetProperty("member").GetString());
                Assert.Equal("GLOBAL_Y", load.GetProperty("direction").GetString());
                Near(golden.Value, load.GetProperty("value").GetDouble(), Percent, golden.Id + " valor");
                Near(golden.Position, load.GetProperty("percent").GetDouble(), Percent, golden.Id + " posicion");
            }

            _output.WriteLine("Mayor diferencia (cargas): " + _largest.ToString("R"));
        }

        [Fact]
        public void LaCargaUtilCompletaReproduceElGoldenYSuMayorDiferenciaQuedaDentroDeLasToleranciasDeLaSeccion8()
        {
            var payload = Payload();

            foreach (var golden in GoldenNodes)
            {
                var node = payload.GetProperty("nodes").EnumerateArray().Single(n => n.GetProperty("id").GetString() == golden.Id);

                Near(golden.X, node.GetProperty("x").GetDouble(), Coordinate, golden.Id);
                Near(golden.Y, node.GetProperty("y").GetDouble(), Coordinate, golden.Id);
                Near(golden.Z, node.GetProperty("z").GetDouble(), Coordinate, golden.Id);
            }

            foreach (var golden in GoldenMembers)
            {
                var member = payload.GetProperty("members").EnumerateArray().Single(m => m.GetProperty("id").GetString() == golden.Id);

                Near(golden.Theta, member.GetProperty("rotation").GetDouble(), Angle, golden.Id);
                NearVector(golden.L1, member.GetProperty("l1"), Component, golden.Id);
                NearVector(golden.L2, member.GetProperty("l2"), Component, golden.Id);
                NearVector(golden.L3, member.GetProperty("l3"), Component, golden.Id);
            }

            foreach (var golden in GoldenLoads)
            {
                var load = payload.GetProperty("loads").EnumerateArray().Single(l => l.GetProperty("id").GetString() == golden.Id);

                Near(golden.Value, load.GetProperty("value").GetDouble(), Percent, golden.Id);
                Near(golden.Position, load.GetProperty("percent").GetDouble(), Percent, golden.Id);
            }

            _output.WriteLine("MAX-DIFFERENCE " + _largest.ToString("R"));
            Assert.True(_largest <= Coordinate, "La mayor diferencia " + _largest.ToString("R") + " supera 1e-9.");
        }

        [Fact]
        public void LaCargaUtilYElPlanDeLaTraduccionCoincidenCampoACampo()
        {
            var table = F.RefTable();
            var plan = RamElementsTranslator.Translate(F.ValidRefModel(table), table).Plan;
            var payload = Payload();

            var nodes = payload.GetProperty("nodes").EnumerateArray().ToList();
            Assert.Equal(plan.Nodes.Select(n => n.Id), nodes.Select(n => n.GetProperty("id").GetString()));

            for (var index = 0; index < plan.Nodes.Count; index++)
            {
                Assert.Equal(plan.Nodes[index].Position.X + 0.0, nodes[index].GetProperty("x").GetDouble());
                Assert.Equal(plan.Nodes[index].Position.Y, nodes[index].GetProperty("y").GetDouble());
                Assert.Equal(plan.Nodes[index].Position.Z, nodes[index].GetProperty("z").GetDouble());
            }

            var loads = payload.GetProperty("loads").EnumerateArray().ToList();
            Assert.Equal(plan.Loads.Select(l => l.PositionPercent), loads.Select(l => l.GetProperty("percent").GetDouble()));
            Assert.Equal(plan.Loads.Select(l => l.ValuePoundsForce), loads.Select(l => l.GetProperty("value").GetDouble()));
        }
    }
}
