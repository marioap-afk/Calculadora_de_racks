using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.Systems.Cantilever;
using Xunit;
using static RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T2 — INV-06 and IDL-01..08 against the golden of REF-CANT-01, derived by hand (docs/initiatives/I-65-f1-ref-cant-01.md,
    /// sections 2 and 3). Every number below is a LITERAL copied from that document; none is computed with the code under test.
    /// </summary>
    public class StructuralAnalysisCantileverGoldenTests
    {
        private const double Coordinate = 1e-9;
        private const double Component = 1e-12;

        // Section 3, first table: node, x, y, z.
        private static readonly (string Id, double X, double Y, double Z)[] GoldenNodes =
        {
            ("COL.FOOT", 0.0, -6.1, 0.25),
            ("ARM.1.PositiveY.J", 0.0, -6.1, 20.26716169689737),
            ("ARM.2.PositiveY.J", 0.0, -6.1, 48.267161696897375),
            ("ARM.3.PositiveY.J", 0.0, -6.1, 76.26716169689738),
            ("COL.TOP", 0.0, -6.1, 90.75),
            ("ARM.1.PositiveY.TIP", 0.0, 36.22609804415268, 21.810300688090436),
            ("ARM.2.PositiveY.TIP", 0.0, 36.22609804415268, 49.81030068809044),
            ("ARM.3.PositiveY.TIP", 0.0, 36.22609804415268, 77.81030068809044),
        };

        // Section 3, second table: member, node I, node J, section, role, origin piece, peralte vector.
        private static readonly (string Id, string I, string J, string Section, StructuralMemberRole Role, string Piece, double Px, double Py, double Pz)[] GoldenMembers =
        {
            ("COL.1", "COL.FOOT", "ARM.1.PositiveY.J", "AISC-W-W12X26", StructuralMemberRole.Column, "CANT-S0-STN-COL", 0.0, 1.0, 0.0),
            ("COL.2", "ARM.1.PositiveY.J", "ARM.2.PositiveY.J", "AISC-W-W12X26", StructuralMemberRole.Column, "CANT-S0-STN-COL", 0.0, 1.0, 0.0),
            ("COL.3", "ARM.2.PositiveY.J", "ARM.3.PositiveY.J", "AISC-W-W12X26", StructuralMemberRole.Column, "CANT-S0-STN-COL", 0.0, 1.0, 0.0),
            ("COL.4", "ARM.3.PositiveY.J", "COL.TOP", "AISC-W-W12X26", StructuralMemberRole.Column, "CANT-S0-STN-COL", 0.0, 1.0, 0.0),
            ("ARM.1.PositiveY", "ARM.1.PositiveY.J", "ARM.1.PositiveY.TIP", "AISC-HSS-RECT-HSS4X4X_250", StructuralMemberRole.Arm, "CANT-S0-STN-L1-PY-BODY-1", 0.0, -0.03643412707017777, 0.9993360567820188),
            ("ARM.2.PositiveY", "ARM.2.PositiveY.J", "ARM.2.PositiveY.TIP", "AISC-HSS-RECT-HSS4X4X_250", StructuralMemberRole.Arm, "CANT-S0-STN-L2-PY-BODY-1", 0.0, -0.03643412707017777, 0.9993360567820188),
            ("ARM.3.PositiveY", "ARM.3.PositiveY.J", "ARM.3.PositiveY.TIP", "AISC-HSS-RECT-HSS4X4X_250", StructuralMemberRole.Arm, "CANT-S0-STN-L3-PY-BODY-1", 0.0, -0.03643412707017777, 0.9993360567820188),
        };

        private static readonly string[] GoldenAssumptions =
            { "SUP-01", "SUP-02", "SUP-03", "CON-01", "IDL-NO-OFFSETS", "ASM-PROP-01", "LOD-01" };

        // ---------------------------------------------------------------- the inputs (section 2)

        [Fact]
        public void LasColocacionesResueltasDeEntradaSonLasLiteralesDeLaSeccion2()
        {
            var line = ResolveLine(Design());
            Assert.False(line.IsBlocked);
            var station = line.Stations[0].Station;

            var column = station.ColumnBase.Column.Placement;
            AssertPoint(0.0, -6.1, 0.25, column.Frame.Origin);
            AssertVector(0.0, 0.0, 1.0, column.Frame.AxisZ);
            AssertVector(0.0, 1.0, 0.0, column.Frame.AxisY);
            Assert.Equal(90.5, column.Length, 9);
            Assert.Equal(0.0, column.RotationRadians);
            Assert.False(column.Mirrored);

            var baseMember = station.ColumnBase.Sides.Single().Member.Placement;
            AssertPoint(0.0, 0.25, 6.1, baseMember.Frame.Origin);
            AssertVector(0.0, 1.0, 0.0, baseMember.Frame.AxisZ);
            AssertVector(0.0, 0.0, 1.0, baseMember.Frame.AxisY);
            Assert.Equal(48.0, baseMember.Length, 9);

            var armOriginZ = new[] { 20.498672113564037, 48.49867211356404, 76.49867211356404 };

            for (var k = 0; k < 3; k++)
            {
                var arm = station.Levels[k].Arm(RackCad.Domain.Systems.Cantilever.CantileverArmSide.PositiveY).Body.Members.Single().Placement;
                AssertPoint(0.0, 0.25, armOriginZ[k], arm.Frame.Origin);
                AssertVector(0.0, 0.9993360567820188, 0.03643412707017777, arm.Frame.AxisZ);
                AssertVector(0.0, -0.03643412707017777, 0.9993360567820188, arm.Frame.AxisY);
                Assert.Equal(36.0, arm.Length, 9);
                Assert.Equal(0.0, arm.RotationRadians);
                Assert.False(arm.Mirrored);
            }
        }

        // ---------------------------------------------------------------- the model (section 3)

        [Fact]
        public void LaEstacion0DeRefCant01EsValidaYExponeElModelo()
        {
            var result = Build(Design());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Model);
            Assert.Equal(8, result.Model.Nodes.Count);
            Assert.Equal(7, result.Model.Members.Count);
        }

        [Fact]
        public void LosNudosSonExactamenteLosDelGoldenConTolerancia1e9()
        {
            var model = BuildValid();

            Assert.Equal(GoldenNodes.Select(n => n.Id).OrderBy(i => i, StringComparer.Ordinal), model.Nodes.Select(n => n.Id).OrderBy(i => i, StringComparer.Ordinal));

            foreach (var golden in GoldenNodes)
            {
                var node = model.Nodes.Single(n => n.Id == golden.Id);
                Assert.True(Near(golden.X, node.Position.X.Inches, Coordinate), golden.Id + " x: " + node.Position.X.Inches);
                Assert.True(Near(golden.Y, node.Position.Y.Inches, Coordinate), golden.Id + " y: " + node.Position.Y.Inches);
                Assert.True(Near(golden.Z, node.Position.Z.Inches, Coordinate), golden.Id + " z: " + node.Position.Z.Inches);
            }
        }

        [Fact]
        public void LosMiembrosSonExactamenteLosDelGoldenConSeccionRolPiezaYNudos()
        {
            var model = BuildValid();

            Assert.Equal(GoldenMembers.Select(m => m.Id), model.Members.Select(m => m.Id));

            foreach (var golden in GoldenMembers)
            {
                var member = model.Members.Single(m => m.Id == golden.Id);
                Assert.Equal(golden.I, member.NodeIId);
                Assert.Equal(golden.J, member.NodeJId);
                Assert.Equal(golden.Section, member.SectionRef.Value);
                Assert.Equal(golden.Role, member.Role);
                Assert.Equal(golden.Piece, member.OriginPiece);
            }
        }

        [Fact]
        public void LosVectoresDePeralteSonLosDelGoldenConTolerancia1e12()
        {
            var model = BuildValid();

            foreach (var golden in GoldenMembers)
            {
                var axis = model.Members.Single(m => m.Id == golden.Id).PeralteAxis;
                Assert.True(Near(golden.Px, axis.X, Component), golden.Id + " px: " + axis.X);
                Assert.True(Near(golden.Py, axis.Y, Component), golden.Id + " py: " + axis.Y);
                Assert.True(Near(golden.Pz, axis.Z, Component), golden.Id + " pz: " + axis.Z);
            }
        }

        [Fact]
        public void ElPieDeColumnaEstaEmpotradoYLosDemasNudosSoloRestringenTxRyRz()
        {
            var model = BuildValid();

            Assert.Equal(model.Nodes.Count, model.Supports.Count);

            var foot = model.Supports.Single(s => s.NodeId == "COL.FOOT");
            Assert.Equal("SUP-01", foot.AssumptionId);
            Assert.Equal(new[] { true, true, true, true, true, true }, foot.Flags);

            foreach (var support in model.Supports.Where(s => s.NodeId != "COL.FOOT"))
            {
                Assert.Equal("SUP-03", support.AssumptionId);
                // Tx, Ty, Tz, Rx, Ry, Rz: Tx, Ry and Rz restrained; Ty, Tz and Rx free.
                Assert.Equal(new[] { true, false, false, false, true, true }, support.Flags);
            }
        }

        [Fact]
        public void NingunMiembroTieneLiberacionesYTodosCitanCon01()
        {
            var model = BuildValid();

            Assert.Equal(7, model.Members.Count);
            Assert.All(model.Members, member =>
            {
                Assert.Equal("CON-01", member.Releases.AssumptionId);
                Assert.False(member.Releases.AnyReleased);
                Assert.Equal(StructuralMemberReleases.FlagCount, member.Releases.Flags.Count);
            });
        }

        [Fact]
        public void LosSupuestosSonExactamenteLosSieteYEstanEscritosEnTerminosNeutrales()
        {
            var model = BuildValid();

            Assert.Equal(GoldenAssumptions, model.Assumptions.Select(a => a.Id));

            var forbidden = new[] { "validated", "certified", "approved", "safe" };

            foreach (var assumption in model.Assumptions)
            {
                Assert.False(string.IsNullOrWhiteSpace(assumption.Statement));
                Assert.DoesNotContain("RAM", assumption.Statement, StringComparison.Ordinal);
                Assert.All(forbidden, word => Assert.DoesNotContain(word, assumption.Statement, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void SoloLaColumnaYLosBrazosSonMiembrosYLaBaseNoSeModela()
        {
            var model = BuildValid();

            Assert.Equal(4, model.Members.Count(m => m.Role == StructuralMemberRole.Column));
            Assert.Equal(3, model.Members.Count(m => m.Role == StructuralMemberRole.Arm));
            Assert.DoesNotContain(model.Members, m => m.OriginPiece.Contains("-BAS", StringComparison.Ordinal));
        }

        [Fact]
        public void LosNudosNoEstanEnPuntosDeInsercionDeBloqueNiEnCarasDeLasPiezas()
        {
            var model = BuildValid();
            var line = ResolveLine(Design());
            var station = line.Stations[0].Station;
            var insertions = station.Members.Select(m => m.Placement.Frame.Origin).ToList();

            // The column foot is the column's own start by IDL-02; no other node may sit at the origin of any piece
            // (the arms and the base are inserted at their plate face, y = 0.25, not at the column axis).
            foreach (var node in model.Nodes.Where(n => n.Id != "COL.FOOT"))
            {
                foreach (var origin in insertions)
                {
                    var same = Near(origin.X, node.Position.X.Inches, 1e-6)
                        && Near(origin.Y, node.Position.Y.Inches, 1e-6)
                        && Near(origin.Z, node.Position.Z.Inches, 1e-6);
                    Assert.False(same, node.Id + " coincide con un punto de insercion " + origin);
                }
            }

            // Every joint lies on the centroidal axis of the column (y = -6.1), not on the arm plate face (y = 0.25) nor on a flange.
            foreach (var joint in model.Nodes.Where(n => n.Id.EndsWith(".J", StringComparison.Ordinal)))
            {
                Assert.True(Near(-6.1, joint.Position.Y.Inches, Coordinate), joint.Id);
            }
        }

        // ---------------------------------------------------------------- D-04

        [Fact]
        public void LaEstacion1ConservaLasCoordenadasLocalesDeLaEstacion0()
        {
            var zero = BuildValid(null, null, 0);
            var one = BuildValid(null, null, 1);

            Assert.Equal(zero.Nodes.Count, one.Nodes.Count);

            foreach (var node in zero.Nodes)
            {
                var other = one.Nodes.Single(n => n.Id == node.Id);
                Assert.True(Near(node.Position.X.Inches, other.Position.X.Inches, Coordinate), node.Id + " x");
                Assert.True(Near(node.Position.Y.Inches, other.Position.Y.Inches, Coordinate), node.Id + " y");
                Assert.True(Near(node.Position.Z.Inches, other.Position.Z.Inches, Coordinate), node.Id + " z");
            }

            // The pieces ARE the station's own, scoped: the same geometry, another origin piece.
            Assert.Equal("CANT-S1-STN-COL", one.Members.Single(m => m.Id == "COL.1").OriginPiece);
            Assert.Equal("CANT-S1-STN-L2-PY-BODY-1", one.Members.Single(m => m.Id == "ARM.2.PositiveY").OriginPiece);
        }

        [Fact]
        public void LasCoordenadasSonDextrogirasConZArribaYLosBrazosPositiveYCrecenHaciaMasY()
        {
            var model = BuildValid();
            var foot = model.Nodes.Single(n => n.Id == "COL.FOOT").Position;
            var top = model.Nodes.Single(n => n.Id == "COL.TOP").Position;

            Assert.True(top.Z.Inches > foot.Z.Inches, "Z arriba: la cabeza esta sobre el pie");

            foreach (var level in new[] { 1, 2, 3 })
            {
                var joint = model.Nodes.Single(n => n.Id == "ARM." + level + ".PositiveY.J").Position;
                var tip = model.Nodes.Single(n => n.Id == "ARM." + level + ".PositiveY.TIP").Position;
                Assert.True(tip.Y.Inches > joint.Y.Inches, "el brazo PositiveY crece hacia +Y");
                Assert.True(tip.Z.Inches > joint.Z.Inches, "el brazo sube con la pendiente");
            }

            // X (line) x Y (arms) = Z (up): the peralte of every member is perpendicular to its axis, in the Y-Z plane.
            foreach (var member in model.Members)
            {
                var i = model.Nodes.Single(n => n.Id == member.NodeIId).Position;
                var j = model.Nodes.Single(n => n.Id == member.NodeJId).Position;
                var dy = j.Y.Inches - i.Y.Inches;
                var dz = j.Z.Inches - i.Z.Inches;
                var length = Math.Sqrt((dy * dy) + (dz * dz));
                var dot = ((member.PeralteAxis.Y * dy) + (member.PeralteAxis.Z * dz)) / length;
                Assert.True(Math.Abs(dot) <= 1e-9, member.Id + " peralte no perpendicular");
            }
        }

        [Fact]
        public void LasLongitudesSeExpresanComoCantidadesTipadasEnPulgadas()
        {
            var model = BuildValid();
            var tip = model.Nodes.Single(n => n.Id == "ARM.1.PositiveY.TIP").Position;

            Assert.IsType<StructuralLength>(tip.Y);
            Assert.True(Near(36.22609804415268, tip.Y.Inches, Coordinate));
            Assert.Equal(StructuralLength.FromInches(90.75).Inches, model.Nodes.Single(n => n.Id == "COL.TOP").Position.Z.Inches, 9);
        }

        // ---------------------------------------------------------------- IDL-10 / CD-I65-F1-14: what is NOT modelled

        [Fact]
        public void PlacasBaseSeparadoresYArriostramientosNoSeModelanNiHacenUnsupportedLaEstacion()
        {
            var line = ResolveLine(Design());
            var station = line.Stations[0].Station;

            // The line REALLY has the things that are not modelled: plates of the column, the base and the arms.
            Assert.True(station.Plates.Count >= 5, "la estacion tiene placas: " + station.Plates.Count);
            Assert.NotEmpty(station.ColumnBase.Sides);

            var result = Build(Design());
            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Equal(7, result.Model.Members.Count);
            Assert.All(result.Model.Members, m => Assert.True(m.Role == StructuralMemberRole.Column || m.Role == StructuralMemberRole.Arm));
        }

        // ---------------------------------------------------------------- helpers

        private static void AssertPoint(double x, double y, double z, RackCad.Application.Geometry.Point3D actual)
        {
            Assert.True(Near(x, actual.X, Coordinate), "x " + actual.X);
            Assert.True(Near(y, actual.Y, Coordinate), "y " + actual.Y);
            Assert.True(Near(z, actual.Z, Coordinate), "z " + actual.Z);
        }

        private static void AssertVector(double x, double y, double z, RackCad.Application.Geometry.Vector3D actual)
        {
            Assert.True(Near(x, actual.X, Component), "x " + actual.X);
            Assert.True(Near(y, actual.Y, Component), "y " + actual.Y);
            Assert.True(Near(z, actual.Z, Component), "z " + actual.Z);
        }
    }
}
