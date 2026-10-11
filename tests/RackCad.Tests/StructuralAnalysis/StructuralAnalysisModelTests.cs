using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T1 — D-02: the neutral model is immutable, derived and not persisted; its parts, its single reference case
    /// and its flags have the shape the Freeze fixes.
    /// </summary>
    public class StructuralAnalysisModelTests
    {
        private static StructuralLength Inches(double value) => StructuralLength.FromInches(value);

        private static StructuralPosition Position(double x, double y, double z) =>
            new StructuralPosition(Inches(x), Inches(y), Inches(z));

        private static IEnumerable<Type> ModelTypes() =>
            typeof(StructuralModel).Assembly.GetTypes()
                .Where(t => t.Namespace == "RackCad.Application.StructuralAnalysis" && t.IsPublic);

        [Fact]
        public void ElModeloEsInmutable_NingunaPropiedadPublicaTieneSetter()
        {
            var modelTypes = new[]
            {
                typeof(StructuralModel), typeof(StructuralModelMetadata), typeof(StructuralSourceRef), typeof(StructuralNode),
                typeof(StructuralPosition), typeof(StructuralMember), typeof(StructuralMemberReleases), typeof(StructuralSection),
                typeof(StructuralMaterial), typeof(StructuralSupport), typeof(StructuralLoadCase), typeof(StructuralMemberPointLoad),
                typeof(AnalysisAssumption), typeof(StructuralDiagnostic),
            };

            foreach (var type in modelTypes)
            {
                var mutable = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetSetMethod(true) != null)
                    .Select(p => type.Name + "." + p.Name)
                    .ToList();

                Assert.Empty(mutable);
                Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance));
            }
        }

        [Fact]
        public void LasColeccionesDelModeloSonDeSoloLecturaYNoComparteElOrigen()
        {
            var nodes = new List<StructuralNode> { new StructuralNode("N1", Position(0, 0, 0)), new StructuralNode("N2", Position(0, 0, 10)) };
            var source = new StructuralAnalysisFixtureSource();
            var description = source.Describe(StructuralAnalysisFixtures.Scope(), StructuralAnalysisFixtures.TwoLoads().LoadFile);
            var model = StructuralAnalysisFixtures.ValidModel();

            Assert.All(new object[] { model.Nodes, model.Members, model.Sections, model.Materials, model.Supports, model.Loads, model.Assumptions, model.Diagnostics },
                collection => Assert.True(!(collection is System.Collections.IList list) || list.IsReadOnly, "la colección expuesta es mutable"));

            var copy = new StructuralModel(
                model.Metadata, nodes, model.Members, model.Sections, model.Materials, model.Supports, model.Loads, model.Assumptions, model.Diagnostics);

            nodes.Add(new StructuralNode("N3", Position(1, 1, 1)));

            Assert.Equal(2, copy.Nodes.Count);
            Assert.NotEmpty(description.Nodes);
        }

        [Fact]
        public void ElModeloEsDerivadoYNoSePersiste()
        {
            var modelType = typeof(StructuralModel);

            Assert.Empty(modelType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName));
            Assert.False(modelType.IsDefined(typeof(SerializableAttribute), false));
            Assert.DoesNotContain(ModelTypes(), t => t.Name.Contains("Document", StringComparison.Ordinal) || t.Name.Contains("Persist", StringComparison.Ordinal));

            var referenced = ModelTypes()
                .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .Select(p => p.PropertyType.Namespace ?? string.Empty);

            Assert.DoesNotContain(referenced, ns => ns.Contains("Persistence", StringComparison.Ordinal) || ns.StartsWith("RackCad.Domain", StringComparison.Ordinal));
        }

        [Fact]
        public void ElModeloTieneUnUnicoCasoREF1DeCategoriaOtraSinPesoPropio()
        {
            var model = StructuralAnalysisFixtures.ValidModel();

            Assert.Equal("REF1", StructuralLoadCase.ReferenceCaseId);
            Assert.Equal("REF1", model.LoadCase.Id);
            Assert.Equal(StructuralLoadCategory.Other, model.LoadCase.Category);
            Assert.False(model.LoadCase.IncludesSelfWeight);
            Assert.Same(StructuralLoadCase.Reference, model.LoadCase);
        }

        [Fact]
        public void LasLiberacionesTienenDiezBanderasLocalesYNingunaPorOmision()
        {
            var none = StructuralMemberReleases.None("CON-01");

            Assert.Equal(10, StructuralMemberReleases.FlagCount);
            Assert.Equal(10, none.Flags.Count);
            Assert.All(none.Flags, flag => Assert.False(flag));
            Assert.False(none.AnyReleased);
            Assert.Equal("CON-01", none.AssumptionId);

            var flags = Enumerable.Repeat(false, 10).ToArray();
            flags[7] = true;

            Assert.True(new StructuralMemberReleases(flags, "CON-01").AnyReleased);
            Assert.Throws<ArgumentException>(() => new StructuralMemberReleases(Enumerable.Repeat(false, 9), "CON-01"));
            Assert.Throws<ArgumentException>(() => new StructuralMemberReleases(Enumerable.Repeat(false, 11), "CON-01"));
        }

        [Fact]
        public void LosApoyosTienenSeisBanderasGlobalesYUnIdDeSupuesto()
        {
            var support = new StructuralSupport("COL.FOOT", new[] { true, true, true, false, true, true }, "SUP-01");

            Assert.Equal(6, StructuralSupport.FlagCount);
            Assert.Equal("COL.FOOT", support.NodeId);
            Assert.Equal(new[] { true, true, true, false, true, true }, support.Flags);
            Assert.Equal("SUP-01", support.AssumptionId);
            Assert.Throws<ArgumentException>(() => new StructuralSupport("COL.FOOT", new[] { true, true }, "SUP-01"));
            Assert.Throws<ArgumentException>(() => new StructuralSupport("COL.FOOT", Enumerable.Repeat(true, 6), " "));
        }

        [Fact]
        public void LaCargaPuntualEsEnMenosZGlobalConMagnitudDeFuerzaYPosicionDeLongitudTipadas()
        {
            var load = new StructuralMemberPointLoad("L1", "ARM.1.PositiveY", StructuralForce.FromPoundsForce(50), Inches(12));

            Assert.Equal(StructuralLoadDirection.GlobalNegativeZ, load.Direction);
            Assert.Equal(typeof(StructuralForce), typeof(StructuralMemberPointLoad).GetProperty(nameof(StructuralMemberPointLoad.Magnitude)).PropertyType);
            Assert.Equal(typeof(StructuralLength), typeof(StructuralMemberPointLoad).GetProperty(nameof(StructuralMemberPointLoad.DistanceFromNodeI)).PropertyType);
            Assert.Equal(50.0, load.Magnitude.PoundsForce);
            Assert.Equal(12.0, load.DistanceFromNodeI.Inches);
            Assert.Throws<ArgumentException>(() => new StructuralMemberPointLoad(" ", "ARM", StructuralForce.FromPoundsForce(1), Inches(0)));
            Assert.Throws<ArgumentException>(() => new StructuralMemberPointLoad("L", "", StructuralForce.FromPoundsForce(1), Inches(0)));
        }

        [Fact]
        public void LasPropiedadesDeSeccionAusentesSonNulasNuncaCero()
        {
            var id = StructuralSectionId.Parse(StructuralAnalysisFixtures.ColumnSection);
            var absent = new StructuralSection(id, null, null, null, null);
            var present = new StructuralSection(
                id,
                StructuralArea.FromSquareInches(7.65),
                StructuralMomentOfInertia.FromInchesToTheFourth(204),
                StructuralMomentOfInertia.FromInchesToTheFourth(17.3),
                StructuralMomentOfInertia.FromInchesToTheFourth(0.3));

            Assert.Null(absent.Area);
            Assert.Null(absent.Ix);
            Assert.Null(absent.Iy);
            Assert.Null(absent.J);
            Assert.Equal(7.65, present.Area.Value.SquareInches);
            Assert.Equal(0.3, present.J.Value.InchesToTheFourth);
            Assert.Equal(id, present.Id);
        }

        [Fact]
        public void LosNudosYMiembrosExigenIdsNoVaciosYElMiembroGuardaSusReferencias()
        {
            Assert.Throws<ArgumentException>(() => new StructuralNode(" ", Position(0, 0, 0)));
            Assert.Throws<ArgumentNullException>(() => new StructuralNode("N", null));

            var member = new StructuralMember(
                "M1", "N1", "N2", StructuralSectionId.Parse(StructuralAnalysisFixtures.ArmSection),
                StructuralMaterialId.Create("FIXTURE-MATERIAL-ARM"), new StructuralDirection(0, 0, 1),
                StructuralMemberReleases.None("CON-01"), StructuralMemberRole.Arm, "ARM");

            Assert.Equal("M1", member.Id);
            Assert.Equal("N1", member.NodeIId);
            Assert.Equal("N2", member.NodeJId);
            Assert.Equal(StructuralMemberRole.Arm, member.Role);
            Assert.Equal("ARM", member.OriginPiece);
            Assert.Equal(1.0, member.PeralteAxis.Z);
            Assert.Equal("FIXTURE-MATERIAL-ARM", member.MaterialRef.Value);
            Assert.Throws<ArgumentException>(() => new StructuralMember(
                "", "N1", "N2", member.SectionRef, member.MaterialRef, member.PeralteAxis, member.Releases, member.Role, "ARM"));
        }

        [Fact]
        public void ElModeloNoAfirmaValidacionEstructuralEnSusNombresNiSupuestos()
        {
            var model = StructuralAnalysisFixtures.ValidModel();
            var words = new[] { "validated", "certified", "approved", "safe" };
            var texts = model.Assumptions.SelectMany(a => new[] { a.Id, a.Statement })
                .Concat(ModelTypes().SelectMany(t => new[] { t.Name }.Concat(t.GetProperties().Select(p => p.Name))))
                .Concat(typeof(StructuralDiagnosticCodes).GetFields().Select(f => (string)f.GetRawConstantValue()))
                .ToList();

            Assert.All(texts, text => Assert.DoesNotContain(words, w => text.Contains(w, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
