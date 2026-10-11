using System;
using System.Linq;
using RackCad.Application.StructuralAnalysis;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T1 — A65-F1-07 and CD-I65-F3-06: the seven assumptions of the Cantilever source state their declared limitation
    /// (I-65 D-10) in neutral terms: no provider name, no provider string and no claim of validation.
    /// </summary>
    public class StructuralAnalysisRamElementsAssumptionsTests
    {
        private static readonly (string Id, string Statement)[] Expected =
        {
            ("SUP-01", "COL.FOOT is fixed in its six degrees of freedom. Declared limitation: no anchor, base plate or slab stiffness is represented."),
            ("SUP-02", "The base is not modelled; the idealisation starts at the column foot. Declared limitation: the real base bears on the floor."),
            ("SUP-03", "Plane analysis: every node other than the foot restrains translation along X and rotation about Y and Z. Declared limitation: no out-of-plane buckling and no bracing effect are represented."),
            ("CON-01", "The arm-to-column connection is continuous: no member end is released. Declared limitation: the real connection is semi-rigid."),
            ("IDL-NO-OFFSETS", "No rigid offsets: each analytical arm runs from the column axis to its tip and includes half the column and the plate. Declared limitation: the arm flexibility is overestimated, and no conclusion about real behaviour is drawn from V1."),
            ("ASM-PROP-01", "Section and material properties used by the analysis are those of the analysis program database, selected by the section and material names of the verified mapping rows. Declared limitation: database editions and material values may differ from the usual ones; they are compared against the catalogue and against the expected values with their cited source."),
            ("LOD-01", "A single load case REF1 carries the loads of the load file; there is no self weight and there are no combinations. Declared limitation: the loads are not derived from pallets, impact, seismic action or tributary areas."),
        };

        private static StructuralModel Model() => C.BuildValid();

        [Fact]
        public void SonExactamenteLosSieteSupuestosEnSuOrden()
        {
            Assert.Equal(Expected.Select(e => e.Id), Model().Assumptions.Select(a => a.Id));
        }

        [Fact]
        public void CadaEnunciadoEsElTextoExactoDeCD_I65_F3_06()
        {
            var assumptions = Model().Assumptions;

            foreach (var expected in Expected)
            {
                Assert.Equal(expected.Statement, assumptions.Single(a => a.Id == expected.Id).Statement);
            }
        }

        [Fact]
        public void CadaEnunciadoDeclaraSuLimitacion()
        {
            Assert.All(Model().Assumptions, a => Assert.Contains("Declared limitation:", a.Statement, StringComparison.Ordinal));
        }

        [Fact]
        public void LosEnunciadosSonNeutralesSinNombreDeProveedorNiCadenaDeProveedorNiAfirmarValidacion()
        {
            var forbidden = new[] { "RAM", "Ram", "Bent" + "ley", "validated", "validation", "certified", "approved", "safe", "A992", "A500", "W 12X26" };

            foreach (var assumption in Model().Assumptions)
            {
                Assert.All(forbidden, word => Assert.DoesNotContain(word, assumption.Statement, StringComparison.Ordinal));
            }
        }

        [Fact]
        public void LaPropiedadDeSeccionYMaterialSeAtribuyeALaBaseDelProgramaPorLosNombresDeLasFilasVerificadas()
        {
            var statement = Model().Assumptions.Single(a => a.Id == "ASM-PROP-01").Statement;

            Assert.Contains("analysis program database", statement, StringComparison.Ordinal);
            Assert.Contains("section and material names of the verified mapping rows", statement, StringComparison.Ordinal);
            Assert.DoesNotContain("explicit material mapping", statement, StringComparison.Ordinal);
        }
    }
}
