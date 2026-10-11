using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.Application.Units;
using Xunit;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;
using K = RackCad.Application.StructuralAnalysis.Providers.RamElements.RamElementsDiagnosticCodes;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T1 — D-03, D-04, D-08 and the golden of the document of F3 (section 8) for REF-CANT-01, station 0. Every golden number
    /// below is a LITERAL copied from that document; none is computed with the code under test. The material rows are FIXTURES.
    /// </summary>
    public class StructuralAnalysisRamElementsTranslationTests
    {
        private const double Coordinate = 1e-9;
        private const double Component = 1e-12;
        private const double Angle = 1e-9;
        private const double Percent = 1e-9;

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

        private static RamBuildPlan RefPlan()
        {
            var table = F.RefTable();
            var model = F.ValidRefModel(table);
            var result = RamElementsTranslator.Translate(model, table);

            Assert.True(
                result.Outcome == StructuralExportOutcome.Valid && result.Plan != null,
                "Se esperaba Valid; resultado " + result.Outcome + ": " + F.Codes(result.Diagnostics));
            return result.Plan;
        }

        private static bool[] Flags(RamRestraints r) => new[] { r.Dx, r.Dy, r.Dz, r.Rx, r.Ry, r.Rz };

        private static void AssertNear(double[] expected, RamVector actual, double tolerance, string label)
        {
            Assert.True(Math.Abs(expected[0] - actual.X) <= tolerance, label + " x: " + actual.X);
            Assert.True(Math.Abs(expected[1] - actual.Y) <= tolerance, label + " y: " + actual.Y);
            Assert.True(Math.Abs(expected[2] - actual.Z) <= tolerance, label + " z: " + actual.Z);
        }

        // ---------------------------------------------------------------- the golden of REF-CANT-01

        [Fact]
        public void LaEstacion0DeRefCant01SeTraduceAUnPlanValidoEnLbInConElCasoRef1()
        {
            var plan = RefPlan();

            Assert.Equal("LB_IN", plan.Units);
            Assert.Equal("REF1", plan.LoadCaseId);
            Assert.Equal("OTHER", plan.LoadCaseCategory);
            Assert.Equal(8, plan.Nodes.Count);
            Assert.Equal(7, plan.Members.Count);
            Assert.Equal(3, plan.Loads.Count);
        }

        [Fact]
        public void LosNudosSonLosDelGoldenDeRamConCoordenadasYRestricciones()
        {
            var plan = RefPlan();

            Assert.Equal(GoldenNodes.Select(n => n.Id).OrderBy(i => i, StringComparer.Ordinal), plan.Nodes.Select(n => n.Id));

            foreach (var golden in GoldenNodes)
            {
                var node = plan.Nodes.Single(n => n.Id == golden.Id);
                Assert.True(Math.Abs(golden.X - node.Position.X) <= Coordinate, golden.Id + " x: " + node.Position.X);
                Assert.True(Math.Abs(golden.Y - node.Position.Y) <= Coordinate, golden.Id + " y: " + node.Position.Y);
                Assert.True(Math.Abs(golden.Z - node.Position.Z) <= Coordinate, golden.Id + " z: " + node.Position.Z);
                Assert.Equal(golden.Flags, Flags(node.Restraints));
            }
        }

        [Fact]
        public void LosMiembrosSonLosDelGoldenConSeccionYMaterialDeRamGiroYEjesLocales()
        {
            var plan = RefPlan();

            Assert.Equal(GoldenMembers.Select(m => m.Id).OrderBy(i => i, StringComparer.Ordinal), plan.Members.Select(m => m.Id));

            foreach (var golden in GoldenMembers)
            {
                var member = plan.Members.Single(m => m.Id == golden.Id);

                Assert.Equal(golden.I, member.NodeIId);
                Assert.Equal(golden.J, member.NodeJId);
                Assert.Equal(golden.Section, member.SectionName);
                Assert.Equal(golden.Material, member.MaterialName);
                Assert.True(Math.Abs(golden.Theta - member.ThetaDegrees) <= Angle, golden.Id + " theta: " + member.ThetaDegrees);
                AssertNear(golden.L1, member.LocalAxes.L1, Component, golden.Id + " l1");
                AssertNear(golden.L2, member.LocalAxes.L2, Component, golden.Id + " l2");
                AssertNear(golden.L3, member.LocalAxes.L3, Component, golden.Id + " l3");
            }
        }

        [Fact]
        public void LasTresCargasSonLasDelGoldenConValorYPorcentaje()
        {
            var plan = RefPlan();

            Assert.Equal(GoldenLoads.Select(l => l.Id), plan.Loads.Select(l => l.Id));

            foreach (var golden in GoldenLoads)
            {
                var load = plan.Loads.Single(l => l.Id == golden.Id);

                Assert.Equal(golden.Member, load.MemberId);
                Assert.Equal("GLOBAL_Y", load.Direction);
                Assert.True(Math.Abs(golden.Value - load.ValuePoundsForce) <= Percent, golden.Id + " valor: " + load.ValuePoundsForce);
                Assert.True(Math.Abs(golden.Position - load.PositionPercent) <= Percent, golden.Id + " posicion: " + load.PositionPercent);
            }
        }

        [Fact]
        public void NingunMiembroTraeLiberacionesYSonDiez()
        {
            Assert.All(RefPlan().Members, member =>
            {
                Assert.Equal(StructuralMemberReleases.FlagCount, member.Releases.Count);
                Assert.All(member.Releases, flag => Assert.False(flag));
            });
        }

        [Fact]
        public void LasFilasUsadasSonLasDosDeLaTablaEnOrdenOrdinalDeSeccion()
        {
            var plan = RefPlan();

            Assert.Equal(new[] { F.ArmSection, F.ColumnSection }, plan.UsedRows.Select(r => r.SectionId.Value));
        }

        [Fact]
        public void LaTraduccionEsDeterministaYNoDependeDeLaCultura()
        {
            var first = Describe(RefPlan());
            var original = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
                Assert.Equal(first, Describe(RefPlan()));
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = original;
            }
        }

        private static string Describe(RamBuildPlan plan)
        {
            var nodes = plan.Nodes.Select(n => n.Id + ":" + n.Position.X.ToString("R", CultureInfo.InvariantCulture) + "," + n.Position.Y.ToString("R", CultureInfo.InvariantCulture)
                + "," + n.Position.Z.ToString("R", CultureInfo.InvariantCulture) + ":" + string.Join(string.Empty, Flags(n.Restraints).Select(f => f ? "1" : "0")));
            var members = plan.Members.Select(m => m.Id + ":" + m.NodeIId + ">" + m.NodeJId + ":" + m.SectionName + ":" + m.MaterialName
                + ":" + m.ThetaDegrees.ToString("R", CultureInfo.InvariantCulture));
            var loads = plan.Loads.Select(l => l.Id + ":" + l.MemberId + ":" + l.ValuePoundsForce.ToString("R", CultureInfo.InvariantCulture)
                + ":" + l.PositionPercent.ToString("R", CultureInfo.InvariantCulture));

            return string.Join("|", nodes.Concat(members).Concat(loads));
        }

        // ---------------------------------------------------------------- INV-08: the supports are a permutation, not index to index

        [Theory]
        [InlineData(0, "dx")]
        [InlineData(1, "dz")]
        [InlineData(2, "dy")]
        [InlineData(3, "rx")]
        [InlineData(4, "rz")]
        [InlineData(5, "ry")]
        public void CadaBanderaNeutralSeMapeaALaDeRamDelDocumentoYSoloAEsa(int neutralIndex, string ramFlag)
        {
            var flags = new bool[StructuralSupport.FlagCount];
            flags[neutralIndex] = true;
            var model = F.Column(
                new[] { F.Support("N1", flags), F.Support("N2", false, false, false, false, false, false) });

            var plan = RamElementsTranslator.Translate(model, F.ColumnTable()).Plan;

            Assert.NotNull(plan);
            var restraints = plan.Nodes.Single(n => n.Id == "N1").Restraints;
            var named = new Dictionary<string, bool>
            {
                ["dx"] = restraints.Dx, ["dy"] = restraints.Dy, ["dz"] = restraints.Dz,
                ["rx"] = restraints.Rx, ["ry"] = restraints.Ry, ["rz"] = restraints.Rz,
            };

            Assert.True(named[ramFlag], ramFlag + " debia estar restringida");
            Assert.Equal(1, named.Values.Count(v => v));
        }

        [Fact]
        public void UnNudoSinApoyoNoTieneRestricciones()
        {
            var model = F.Column(new[] { F.Support("N1", true, true, true, true, true, true) });

            var plan = RamElementsTranslator.Translate(model, F.ColumnTable()).Plan;

            Assert.Equal(new[] { false, false, false, false, false, false }, Flags(plan.Nodes.Single(n => n.Id == "N2").Restraints));
        }

        // ---------------------------------------------------------------- D-03 and D-04: coordinates, order, loads

        [Fact]
        public void LasPosicionesSeLlevanDeXYZNeutralAXZMenosYDeRamSinConversion()
        {
            var model = F.Synthetic(
                new[] { F.Node("A", 1.5, 2.25, 3.75), F.Node("B", 1.5, 2.25, 103.75) },
                new[] { new F.SyntheticMember { Id = "M", NodeI = "A", NodeJ = "B" } },
                new[] { F.Support("A", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0]);

            var plan = RamElementsTranslator.Translate(model, F.ColumnTable()).Plan;

            var a = plan.Nodes.Single(n => n.Id == "A").Position;
            var b = plan.Nodes.Single(n => n.Id == "B").Position;
            Assert.Equal((1.5, 3.75, -2.25), (a.X, a.Y, a.Z));
            Assert.Equal((1.5, 103.75, -2.25), (b.X, b.Y, b.Z));
        }

        [Fact]
        public void NudosMiembrosYCargasSalenEnOrdenOrdinalDeIdSinImportarElOrdenDeEntrada()
        {
            var model = F.Synthetic(
                new[] { F.Node("N3", 0, 0, 200), F.Node("N1", 0, 0, 0), F.Node("N2", 0, 0, 100) },
                new[]
                {
                    new F.SyntheticMember { Id = "M2", NodeI = "N2", NodeJ = "N3" },
                    new F.SyntheticMember { Id = "M1", NodeI = "N1", NodeJ = "N2" },
                },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new[] { F.Load("Q2", "M2", 10, 5), F.Load("Q10", "M1", 10, 5), F.Load("Q1", "M1", 10, 5) });

            var plan = RamElementsTranslator.Translate(model, F.ColumnTable()).Plan;

            Assert.Equal(new[] { "N1", "N2", "N3" }, plan.Nodes.Select(n => n.Id));
            Assert.Equal(new[] { "M1", "M2" }, plan.Members.Select(m => m.Id));
            Assert.Equal(new[] { "Q1", "Q10", "Q2" }, plan.Loads.Select(l => l.Id));
        }

        [Fact]
        public void LaCargaSeEmiteConSignoMenosYPosicionEnPorcentajeDelMiembro()
        {
            var model = F.Column(loads: new[] { F.Load("Q", "M1", 250, 25), F.Load("R", "M1", 4, 100), F.Load("S", "M1", 4, 0) });

            var plan = RamElementsTranslator.Translate(model, F.ColumnTable()).Plan;

            var q = plan.Loads.Single(l => l.Id == "Q");
            Assert.Equal(-250.0, q.ValuePoundsForce);
            Assert.True(Math.Abs(25.0 - q.PositionPercent) <= Percent);
            Assert.Equal("GLOBAL_Y", q.Direction);
            Assert.True(Math.Abs(100.0 - plan.Loads.Single(l => l.Id == "R").PositionPercent) <= Percent);
            Assert.Equal(0.0, plan.Loads.Single(l => l.Id == "S").PositionPercent);
        }

        [Fact]
        public void ElGiroDeUnMiembroSeCalculaConElVectorDePeralteLlevadoARam()
        {
            // Neutral member along +Z, depth axis neutral +Y: RAM vertical member, depth (0, 0, -1) in RAM, theta -90 as the columns of the golden.
            var column = new F.SyntheticMember { Peralte = new StructuralDirection(0, 1, 0) };
            var rotated = F.Synthetic(
                new[] { F.Node("N1", 0, 0, 0), F.Node("N2", 0, 0, 100) },
                new[] { column },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0]);

            var plan = RamElementsTranslator.Translate(rotated, F.ColumnTable()).Plan;

            Assert.True(Math.Abs(-90.0 - plan.Members.Single().ThetaDegrees) <= Angle);
            AssertNear(new[] { 0.0, 0.0, -1.0 }, plan.Members.Single().LocalAxes.L2, Component, "l2");
        }

        // ---------------------------------------------------------------- D-08: blocking diagnostics, no partial plan

        private static void AssertBlocked(RamTranslationResult result, StructuralExportOutcome outcome, string code)
        {
            Assert.Equal(outcome, result.Outcome);
            Assert.Null(result.Plan);
            Assert.True(
                result.Diagnostics.Any(d => d.Code == code && d.IsBlocking && d.ImpliedOutcome == outcome),
                "Se esperaba " + code + " y hay " + F.Codes(result.Diagnostics));
        }

        [Fact]
        public void UnaSeccionSinFilaEnLaTablaEsUnsupportedYNoHayPlan()
        {
            var table = F.Table(F.Document(F.ArmRow()));

            AssertBlocked(RamElementsTranslator.Translate(F.Column(), table), StructuralExportOutcome.Unsupported, K.SectionRowMissing);
        }

        [Fact]
        public void UnaFilaProposedEsUnsupportedYNoHayPlan()
        {
            var row = F.ColumnRow();
            row["status"] = "proposed";
            row["verifiedBy"] = null;
            row["verifiedOn"] = null;

            AssertBlocked(RamElementsTranslator.Translate(F.Column(), F.Table(F.Document(row))), StructuralExportOutcome.Unsupported, K.SectionRowUnverified);
        }

        [Fact]
        public void ElMaterialDelModeloDebeSerElDeLaFilaDeSuSeccionOLaTablaNoEsLaQueLoConstruyo()
        {
            var row = F.ColumnRow();
            row["materialId"] = "ASTM-A992-GR65";

            AssertBlocked(RamElementsTranslator.Translate(F.Column(), F.Table(F.Document(row))), StructuralExportOutcome.Invalid, K.MaterialMismatch);
        }

        [Fact]
        public void UnaOrientacionNoAdmitidaEsUnsupportedYNoHayPlan()
        {
            // Neutral (0, 70.7, 70.7): 45 degrees from the horizontal.
            var model = F.Synthetic(
                new[] { F.Node("N1", 0, 0, 0), F.Node("N2", 0, 70.71067811865476, 70.71067811865476) },
                new[] { new F.SyntheticMember() },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0]);

            AssertBlocked(RamElementsTranslator.Translate(model, F.ColumnTable()), StructuralExportOutcome.Unsupported, K.OrientationUnsupported);
        }

        [Fact]
        public void UnMiembroCasiHorizontalDentroDeDiezGradosSiSeTraduce()
        {
            // Neutral (0, 100, 4.375): about 2.5 degrees, as the arm of the reference.
            var model = F.Synthetic(
                new[] { F.Node("N1", 0, 0, 0), F.Node("N2", 0, 100, 4.375) },
                new[] { new F.SyntheticMember { Peralte = new StructuralDirection(0, -0.04370818989361, 0.99904434042550) } },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0]);

            var result = RamElementsTranslator.Translate(model, F.ColumnTable());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Plan);
        }

        [Fact]
        public void MasDeVeinticuatroCargasEnUnMiembroEsUnsupportedYNoHayPlan()
        {
            var loads = Enumerable.Range(1, 25).Select(i => F.Load("Q" + i.ToString("00", CultureInfo.InvariantCulture), "M1", 10, i)).ToArray();

            AssertBlocked(RamElementsTranslator.Translate(F.Column(loads: loads), F.ColumnTable()), StructuralExportOutcome.Unsupported, K.LoadCountExceeded);
        }

        [Fact]
        public void VeinticuatroCargasEnUnMiembroSiSeTraducen()
        {
            var loads = Enumerable.Range(1, 24).Select(i => F.Load("Q" + i.ToString("00", CultureInfo.InvariantCulture), "M1", 10, i)).ToArray();

            var result = RamElementsTranslator.Translate(F.Column(loads: loads), F.ColumnTable());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Equal(24, result.Plan.Loads.Count);
        }

        [Fact]
        public void UnaLiberacionEsUnsupportedYNoHayPlan()
        {
            var released = new F.SyntheticMember();
            released.Releases[4] = true;
            var model = F.Synthetic(
                new[] { F.Node("N1", 0, 0, 0), F.Node("N2", 0, 0, 100) },
                new[] { released },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0]);

            AssertBlocked(RamElementsTranslator.Translate(model, F.ColumnTable()), StructuralExportOutcome.Unsupported, K.ReleaseUnsupported);
        }

        [Fact]
        public void UnVectorDePeralteAlineadoConElMiembroEsInvalidoYNoHayPlan()
        {
            var model = F.Synthetic(
                new[] { F.Node("N1", 0, 0, 0), F.Node("N2", 0, 0, 100) },
                new[] { new F.SyntheticMember { Peralte = new StructuralDirection(0, 0, 1) } },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0]);

            AssertBlocked(RamElementsTranslator.Translate(model, F.ColumnTable()), StructuralExportOutcome.Invalid, K.MemberGeometry);
        }

        [Fact]
        public void UnaCargaSobreUnMiembroInexistenteEsInvalidaYNoHayPlan()
        {
            var model = F.Column(loads: new[] { F.Load("Q", "NOPE", 10, 5) });

            AssertBlocked(RamElementsTranslator.Translate(model, F.ColumnTable()), StructuralExportOutcome.Invalid, K.ModelReference);
        }

        [Fact]
        public void UnModeloConUnDiagnosticoBloqueanteNoSeTraduce()
        {
            var model = F.Synthetic(
                new[] { F.Node("N1", 0, 0, 0), F.Node("N2", 0, 0, 100) },
                new[] { new F.SyntheticMember() },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0],
                new[] { StructuralDiagnostic.Blocking("X-TEST", StructuralExportOutcome.Unsupported, "subject") });

            var result = RamElementsTranslator.Translate(model, F.ColumnTable());

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Plan);
            Assert.Contains(result.Diagnostics, d => d.Code == K.ModelNotValid && d.IsBlocking);
        }

        [Fact]
        public void UnResultadoQueNoEsValidoSePropagaConSuResultadoYSinPlan()
        {
            var table = F.Read(F.DataFileText()).Table;
            var built = F.BuildRef(table);
            Assert.Equal(StructuralExportOutcome.Unsupported, built.Outcome);

            var result = RamElementsTranslator.Translate(built, F.RefTable());

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Plan);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.MaterialRowUnverified);
            Assert.Contains(result.Diagnostics, d => d.Code == K.ModelNotValid);
        }

        [Fact]
        public void UnResultadoValidoSeTraduceComoElModelo()
        {
            var table = F.RefTable();
            var built = F.BuildRef(table);

            var result = RamElementsTranslator.Translate(built, table);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Equal(Describe(RefPlan()), Describe(result.Plan));
        }

        [Fact]
        public void LaPrecedenciaEsInvalidoSobreUnsupported()
        {
            var row = F.ColumnRow();
            row["materialId"] = "ASTM-A992-GR65";
            var model = F.Synthetic(
                new[] { F.Node("N1", 0, 0, 0), F.Node("N2", 0, 70.71067811865476, 70.71067811865476) },
                new[] { new F.SyntheticMember() },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0]);

            var result = RamElementsTranslator.Translate(model, F.Table(F.Document(row)));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Contains(result.Diagnostics, d => d.Code == K.MaterialMismatch);
            Assert.Contains(result.Diagnostics, d => d.Code == K.OrientationUnsupported);
            Assert.Null(result.Plan);
        }

        [Fact]
        public void LosArgumentosNulosLanzanExcepcion()
        {
            Assert.Throws<ArgumentNullException>(() => RamElementsTranslator.Translate((StructuralModel)null, F.ColumnTable()));
            Assert.Throws<ArgumentNullException>(() => RamElementsTranslator.Translate(F.Column(), null));
            Assert.Throws<ArgumentNullException>(() => RamElementsTranslator.Translate((StructuralModelResult)null, F.ColumnTable()));
        }
    }
}
