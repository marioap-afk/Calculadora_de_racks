using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T1 — INV-07 and D-04: the rule of the local axes and of the twist against the 12 directions by 3 twists MEASURED in
    /// P1 (<c>p1-report.json</c>, step <c>A1_orientation_coverage</c>), read from the repository root, with tolerance 1e-12.
    /// The admission of V1 (vertical, or at most 10 degrees from the horizontal) is a separate check from the rule.
    /// </summary>
    public class StructuralAnalysisRamElementsAxesTests
    {
        private const string ReportFile = "docs/automation/evidence/I-65-f0/p1-run/p1-report.json";
        private const double Tolerance = 1e-12;

        private static readonly string[] Twists = { "rot0", "rot90", "rot12.3456" };

        private static readonly Dictionary<string, double> TwistDegrees = new Dictionary<string, double>
        {
            ["rot0"] = 0.0, ["rot90"] = 90.0, ["rot12.3456"] = 12.3456,
        };

        private static JsonElement Observed()
        {
            var path = Path.Combine(C.RepoRoot(), ReportFile.Replace('/', Path.DirectorySeparatorChar));
            var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.GetProperty("steps").GetProperty("A1_orientation_coverage").GetProperty("observed");
        }

        private static RamVector Vector(JsonElement array) =>
            new RamVector(array[0].GetDouble(), array[1].GetDouble(), array[2].GetDouble());

        private static IEnumerable<string> Directions() =>
            Observed().EnumerateObject().Where(p => p.Name.Contains("-fwd") || p.Name.Contains("-rev")).Select(p => p.Name).ToList();

        public static IEnumerable<object[]> Coverage() =>
            Directions().SelectMany(direction => Twists.Select(twist => new object[] { direction, twist }));

        private static void AssertNear(RamVector expected, RamVector actual, string label)
        {
            Assert.True(Math.Abs(expected.X - actual.X) <= Tolerance, label + " x: " + expected.X + " frente a " + actual.X);
            Assert.True(Math.Abs(expected.Y - actual.Y) <= Tolerance, label + " y: " + expected.Y + " frente a " + actual.Y);
            Assert.True(Math.Abs(expected.Z - actual.Z) <= Tolerance, label + " z: " + expected.Z + " frente a " + actual.Z);
        }

        private static double Dot(RamVector a, RamVector b) => (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);

        // ---------------------------------------------------------------- the 12 directions by 3 twists of P1

        [Fact]
        public void LaCoberturaDeP1TieneDoceDireccionesYTresGiros()
        {
            var directions = Directions().ToList();

            Assert.Equal(12, directions.Count);

            foreach (var direction in directions)
            {
                var record = Observed().GetProperty(direction);

                foreach (var twist in Twists)
                {
                    Assert.Equal(3, record.GetProperty(twist).GetArrayLength());
                }
            }
        }

        [Theory]
        [MemberData(nameof(Coverage))]
        public void LaReglaReproduceLosEjesLocalesMedidosEnP1ConToleranciaDe1e12(string direction, string twist)
        {
            var record = Observed().GetProperty(direction);
            var l1 = Vector(record.GetProperty("rot0")[0]);
            var axes = RamLocalAxes.Rotate(RamLocalAxes.ZeroTwist(l1), TwistDegrees[twist]);
            var measured = record.GetProperty(twist);

            AssertNear(Vector(measured[0]), axes.L1, direction + " " + twist + " l1");
            AssertNear(Vector(measured[1]), axes.L2, direction + " " + twist + " l2");
            AssertNear(Vector(measured[2]), axes.L3, direction + " " + twist + " l3");
        }

        [Theory]
        [MemberData(nameof(Coverage))]
        public void ElGiroEmitidoReproduceElVectorDePeralteEnL2ConToleranciaDe1e12(string direction, string twist)
        {
            var record = Observed().GetProperty(direction);
            var zeroTwist = RamLocalAxes.ZeroTwist(Vector(record.GetProperty("rot0")[0]));
            var depth = Vector(record.GetProperty(twist)[1]);

            var theta = RamLocalAxes.ThetaDegrees(zeroTwist, depth);

            Assert.True(Math.Abs(TwistDegrees[twist] - theta) <= Tolerance, direction + " " + twist + ": " + theta);
            AssertNear(depth, RamLocalAxes.Rotate(zeroTwist, theta).L2, direction + " " + twist + " l2 tras el giro");
        }

        // ---------------------------------------------------------------- the rule itself

        [Theory]
        [InlineData("vert_+Y-fwd", true)]
        [InlineData("vert_+Y-rev", true)]
        [InlineData("horiz_-Z-fwd", false)]
        [InlineData("horiz_+X-rev", false)]
        [InlineData("incl_YZ-fwd", false)]
        [InlineData("arm_Zup-fwd", false)]
        public void UnMiembroEsVerticalSoloSiSuComponenteHorizontalEsDe1e12OMenos(string direction, bool vertical)
        {
            var l1 = Vector(Observed().GetProperty(direction).GetProperty("rot0")[0]);

            Assert.Equal(vertical, RamLocalAxes.ZeroTwist(l1).IsVertical);
        }

        [Theory]
        [InlineData(1e-13, true)]
        [InlineData(5e-13, true)]
        [InlineData(5e-12, false)]
        public void ElUmbralDeVerticalEsLaComponenteHorizontalDeL1(double horizontal, bool vertical)
        {
            var norm = Math.Sqrt((horizontal * horizontal) + 1.0);
            var frame = RamLocalAxes.ZeroTwist(new RamVector(horizontal / norm, 1.0 / norm, 0.0));

            Assert.Equal(vertical, frame.IsVertical);
        }

        [Fact]
        public void UnMiembroVerticalTomaElEjeL3PositivoZYUnoNoVerticalNoLoHace()
        {
            var vertical = RamLocalAxes.ZeroTwist(new RamVector(0, 1, 0));
            var horizontal = RamLocalAxes.ZeroTwist(new RamVector(0, 0, -1));

            AssertNear(new RamVector(0, 0, 1), vertical.L3, "vertical l3");
            AssertNear(new RamVector(-1, 0, 0), vertical.L2, "vertical l2");
            AssertNear(new RamVector(1, 0, 0), horizontal.L3, "horizontal l3");
            Assert.NotEqual(1.0, horizontal.L3.Z);
        }

        [Theory]
        [InlineData(0.0, 1.0, 0.0, true)]
        [InlineData(0.0, -1.0, 0.0, true)]
        [InlineData(1.0, 0.0, 0.0, true)]
        [InlineData(0.0, 0.0, -1.0, true)]
        public void LaOrientacionVerticalOHorizontalEsAdmitida(double x, double y, double z, bool admitted)
        {
            Assert.Equal(admitted, RamLocalAxes.IsAdmitted(new RamVector(x, y, z)));
        }

        [Theory]
        [InlineData(9.9, true)]
        [InlineData(2.5, true)]
        [InlineData(10.1, false)]
        [InlineData(45.0, false)]
        [InlineData(87.5, false)]
        public void LaInclinacionRespectoDeLaHorizontalDeAlosSumoDiezGradosEsAdmitida(double degrees, bool admitted)
        {
            var radians = degrees * Math.PI / 180.0;

            // A rise in RAM Y over a run along RAM Z, and the same on the other side of the horizontal.
            Assert.Equal(admitted, RamLocalAxes.IsAdmitted(new RamVector(0, Math.Sin(radians), -Math.Cos(radians))));
            Assert.Equal(admitted, RamLocalAxes.IsAdmitted(new RamVector(0, -Math.Sin(radians), Math.Cos(radians))));
            Assert.Equal(admitted, RamLocalAxes.IsAdmitted(new RamVector(Math.Cos(radians), Math.Sin(radians), 0)));
        }

        [Fact]
        public void LaAdmisionDeLasDoceDireccionesDeP1SeparaLaVerticalYLaCasiHorizontalDelBrazoConCuestaPronunciada()
        {
            foreach (var direction in Directions())
            {
                var l1 = Vector(Observed().GetProperty(direction).GetProperty("rot0")[0]);
                var expected = !direction.StartsWith("arm_Zup", StringComparison.Ordinal);

                Assert.True(expected == RamLocalAxes.IsAdmitted(l1), direction);
            }
        }

        [Theory]
        [InlineData(0.0, 1.0, 0.0)]
        [InlineData(0.0, 0.036434127070177715, -0.9993360567820188)]
        [InlineData(1.0, 0.0, 0.0)]
        public void LosEjesSonUnitariosYMutuamenteOrtogonalesParaCualquierGiro(double x, double y, double z)
        {
            foreach (var degrees in new[] { 0.0, 33.3, -90.0, 180.0 })
            {
                var axes = RamLocalAxes.Rotate(RamLocalAxes.ZeroTwist(new RamVector(x, y, z)), degrees);

                Assert.True(Math.Abs(Dot(axes.L1, axes.L1) - 1) <= Tolerance);
                Assert.True(Math.Abs(Dot(axes.L2, axes.L2) - 1) <= Tolerance);
                Assert.True(Math.Abs(Dot(axes.L3, axes.L3) - 1) <= Tolerance);
                Assert.True(Math.Abs(Dot(axes.L1, axes.L2)) <= Tolerance);
                Assert.True(Math.Abs(Dot(axes.L1, axes.L3)) <= Tolerance);
                Assert.True(Math.Abs(Dot(axes.L2, axes.L3)) <= Tolerance);
            }
        }
    }
}
