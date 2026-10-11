using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using Xunit;
using static RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T2 — IDL-08 (tolerances), IDL-10 (inclination) and INV-07 (the peralte vector) on SYNTHETIC placements built in
    /// the test. The idealisation is a pure function of placements, so a tolerance is proved at its edge without having to
    /// find a design that happens to land there.
    /// </summary>
    public class StructuralAnalysisCantileverIdealisationTests
    {
        private static readonly double[] Rotations = { 0.0, 90.0, 12.3456 };

        private static IReadOnlyList<CantileverStructuralModelSource.StationPiece> Arms(params CantileverStructuralModelSource.StationPiece[] arms) => arms;

        private static void AssertBlocked(StructuralModelResult result, StructuralExportOutcome outcome, string code)
        {
            Assert.Equal(outcome, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, d => d.Code == code && d.IsBlocking && d.ImpliedOutcome == outcome);
        }

        // ---------------------------------------------------------------- IDL-08: node fusion (0.001 in)

        [Fact]
        public void UnaJuntaAMenosDe0001InDelPieSeFusionaConElPie()
        {
            var result = BuildFrom(Column(), Arms(Arm(1, ColumnStartZ + 0.0005), Arm(2, 30.0)));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.DoesNotContain(result.Model.Nodes, n => n.Id == "ARM.1.PositiveY.J");
            Assert.Equal("COL.FOOT", result.Model.Members.Single(m => m.Id == "ARM.1.PositiveY").NodeIId);
            Assert.Equal(new[] { "COL.1", "COL.2" }, result.Model.Members.Where(m => m.Id.StartsWith("COL.", StringComparison.Ordinal)).Select(m => m.Id));
        }

        [Fact]
        public void UnaJuntaAMenosDe0001InDeLaCabezaSeFusionaConLaCabeza()
        {
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0), Arm(2, ColumnStartZ + ColumnLength - 0.0005)));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.DoesNotContain(result.Model.Nodes, n => n.Id == "ARM.2.PositiveY.J");
            Assert.Equal("COL.TOP", result.Model.Members.Single(m => m.Id == "ARM.2.PositiveY").NodeIId);
        }

        [Fact]
        public void DosJuntasAMenosDe0001InSeFusionanEnLaPrimera()
        {
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0), Arm(2, 30.0004)));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.DoesNotContain(result.Model.Nodes, n => n.Id == "ARM.2.PositiveY.J");
            Assert.Equal("ARM.1.PositiveY.J", result.Model.Members.Single(m => m.Id == "ARM.2.PositiveY").NodeIId);
        }

        // ---------------------------------------------------------------- IDL-08: minimum column separation (0.05 in)

        [Theory]
        [InlineData(0.0011)]
        [InlineData(0.04)]
        [InlineData(0.0499)]
        public void UnaSeparacionDeColumnaMenorQue005EsInvalidaSinFusionSilenciosa(double gap)
        {
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0), Arm(2, 30.0 + gap)));

            AssertBlocked(result, StructuralExportOutcome.Invalid, StructuralDiagnosticCodes.NodeSeparation);
        }

        [Theory]
        [InlineData(0.0501)]
        [InlineData(0.06)]
        [InlineData(5.0)]
        public void UnaSeparacionDeColumnaDe005OMasEsValida(double gap)
        {
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0), Arm(2, 30.0 + gap)));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Contains(result.Model.Nodes, n => n.Id == "ARM.2.PositiveY.J");
        }

        [Fact]
        public void UnaJuntaAMenosDe005PeroMasDe0001DelPieEsInvalida()
        {
            var result = BuildFrom(Column(), Arms(Arm(1, ColumnStartZ + 0.04)));

            AssertBlocked(result, StructuralExportOutcome.Invalid, StructuralDiagnosticCodes.NodeSeparation);
        }

        // ---------------------------------------------------------------- IDL-08: deviation from the station plane (0.01 in)

        [Theory]
        [InlineData(0.011)]
        [InlineData(-0.02)]
        public void UnBrazoDesviadoDelPlanoMasDe001InEsUnsupported(double shift)
        {
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0, shiftX: shift)));

            AssertBlocked(result, StructuralExportOutcome.Unsupported, StructuralDiagnosticCodes.NodePlaneDeviation);
        }

        [Fact]
        public void UnaColumnaDesviadaDelPlanoMasDe001InEsUnsupported()
        {
            var result = BuildFrom(Column(axisX: 0.011), Arms(Arm(1, 30.0)));

            AssertBlocked(result, StructuralExportOutcome.Unsupported, StructuralDiagnosticCodes.NodePlaneDeviation);
        }

        [Theory]
        [InlineData(0.009)]
        [InlineData(-0.01)]
        public void UnaDesviacionDelPlanoDentroDeLaToleranciaEsValida(double shift)
        {
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0, shiftX: shift)));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.All(result.Model.Nodes, node => Assert.Equal(0.0, node.Position.X.Inches));
        }

        // ---------------------------------------------------------------- IDL-08: angular tolerance (0.01 degrees)

        [Fact]
        public void UnEjeInclinadoFueraDelPlanoMasDe001GradosEsUnsupportedAunqueSuDesplazamientoSeaPequeno()
        {
            // 36 in * sin(0.011 deg) = 0.0069 in: inside the 0.01 in position tolerance, outside the angular one.
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0, outOfPlaneDegrees: 0.011)));

            AssertBlocked(result, StructuralExportOutcome.Unsupported, StructuralDiagnosticCodes.AxisAngleDeviation);
        }

        [Fact]
        public void UnEjeInclinadoFueraDelPlanoMenosDe001GradosEsValido()
        {
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0, outOfPlaneDegrees: 0.009)));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
        }

        [Fact]
        public void UnBrazoParaleloALaColumnaDentroDe001GradosEsUnsupported()
        {
            // A column lying 5 degrees off the horizontal and an arm sloped 5 degrees: the axes never meet.
            var tilt = 5.0 * Math.PI / 180.0;
            var frame = LocalFrame3D.Create(new Point3D(0.0, ColumnAxisY, ColumnStartZ), new Vector3D(0.0, Math.Cos(tilt), Math.Sin(tilt)), Vector3D.UnitX);
            var column = new CantileverStructuralModelSource.StationPiece(
                "CANT-S0-STN-COL",
                PrismaticSectionInstance.Create(RackCad.Application.StructuralSections.StructuralSectionId.Parse(ColumnSection), ColumnLength, frame));

            var result = BuildFrom(column, Arms(Arm(1, 30.0, slopeDegrees: 5.0)));

            AssertBlocked(result, StructuralExportOutcome.Unsupported, StructuralDiagnosticCodes.AxesParallel);
        }

        // ---------------------------------------------------------------- IDL-04 / IDL-08: a joint outside the column

        [Theory]
        [InlineData(95.0)]
        [InlineData(91.0)]
        [InlineData(-5.0)]
        [InlineData(0.2)]
        public void UnNudoBrazoColumnaFueraDelTramoDeLaColumnaEsInvalido(double zAtColumnAxis)
        {
            var result = BuildFrom(Column(), Arms(Arm(1, zAtColumnAxis)));

            AssertBlocked(result, StructuralExportOutcome.Invalid, StructuralDiagnosticCodes.NodeOutsideColumn);
        }

        // ---------------------------------------------------------------- IDL-10: inclination

        [Theory]
        [InlineData(10.5)]
        [InlineData(14.0)]
        [InlineData(45.0)]
        public void UnBrazoConMasDe10GradosDeInclinacionEsUnsupportedSinModelo(double degrees)
        {
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0, slopeDegrees: degrees)));

            AssertBlocked(result, StructuralExportOutcome.Unsupported, StructuralDiagnosticCodes.ArmSlopeUnsupported);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(2.0)]
        [InlineData(9.9)]
        public void UnBrazoConMenosDe10GradosDeInclinacionEsValido(double degrees)
        {
            var result = BuildFrom(Column(), Arms(Arm(1, 30.0, slopeDegrees: degrees)));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
        }

        // ---------------------------------------------------------------- INV-07: the peralte vector

        private static Vector3D Oracle(PrismaticSectionInstance placement)
        {
            // The oracle: the section transform applied to the local depth direction (0, 1), carried to global with the frame.
            var local = placement.SectionTransform().Apply(new Vector2D(0.0, 1.0));
            return (placement.Frame.AxisX * local.X) + (placement.Frame.AxisY * local.Y);
        }

        [Theory]
        [InlineData(0.0, false)]
        [InlineData(0.0, true)]
        [InlineData(90.0, false)]
        [InlineData(90.0, true)]
        [InlineData(12.3456, false)]
        [InlineData(12.3456, true)]
        public void ElVectorDePeralteSeDerivaDelMarcoConGiroYEspejo(double rotation, bool mirrored)
        {
            var column = Column(rotation, mirrored);
            var arm = Arm(1, 30.0, rotationDegrees: rotation, mirrored: mirrored);
            var result = BuildFrom(column, Arms(arm));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);

            foreach (var (member, placement) in new[]
                     {
                         (result.Model.Members.First(m => m.Id == "COL.1"), column.Placement),
                         (result.Model.Members.Single(m => m.Id == "ARM.1.PositiveY"), arm.Placement),
                     })
            {
                var expected = Oracle(placement);
                Assert.True(Near(expected.X, member.PeralteAxis.X, 1e-12), member.Id + " x: " + member.PeralteAxis.X + " vs " + expected.X);
                Assert.True(Near(expected.Y, member.PeralteAxis.Y, 1e-12), member.Id + " y: " + member.PeralteAxis.Y + " vs " + expected.Y);
                Assert.True(Near(expected.Z, member.PeralteAxis.Z, 1e-12), member.Id + " z: " + member.PeralteAxis.Z + " vs " + expected.Z);
            }
        }

        [Fact]
        public void UnaRotacionNoNulaCambiaElVectorDePeralteYNoSeIgnora()
        {
            foreach (var rotation in new[] { 90.0, 12.3456 })
            {
                var plain = BuildFrom(Column(), Arms(Arm(1, 30.0))).Model;
                var turned = BuildFrom(Column(rotation), Arms(Arm(1, 30.0, rotationDegrees: rotation))).Model;

                foreach (var id in new[] { "COL.1", "ARM.1.PositiveY" })
                {
                    var before = plain.Members.First(m => m.Id == id).PeralteAxis;
                    var after = turned.Members.First(m => m.Id == id).PeralteAxis;
                    var distance = Math.Sqrt(Math.Pow(before.X - after.X, 2) + Math.Pow(before.Y - after.Y, 2) + Math.Pow(before.Z - after.Z, 2));
                    Assert.True(distance > 0.1, id + " con giro " + rotation + " no cambio el peralte");
                }
            }
        }

        [Fact]
        public void ElGiroDe90GradosLlevaElPeralteFueraDelPlanoYElDeCeroLoDejaEnElPlano()
        {
            var turned = BuildFrom(Column(90.0), Arms(Arm(1, 30.0, rotationDegrees: 90.0))).Model;
            var column = turned.Members.First(m => m.Id == "COL.1").PeralteAxis;

            Assert.True(Near(-1.0, column.X, 1e-12));
            Assert.True(Near(0.0, column.Y, 1e-12));
            Assert.True(Near(0.0, column.Z, 1e-12));
        }
    }
}
