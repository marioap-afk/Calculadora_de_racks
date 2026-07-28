using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.UI.StructuralSections;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// El inspector de secciones. Casi todo se prueba sobre su ESTADO, que es puro y no necesita dispatcher;
    /// solo lo que de verdad es visual construye la ventana.
    /// </summary>
    public class StructuralSectionInspectorTests
    {
        private static StructuralSectionCatalog Catalog() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static StructuralSectionInspectorState State() =>
            new StructuralSectionInspectorState(Catalog());

        // ---- Catalogo y filtros ---------------------------------------------------------------------------

        [Fact]
        public void TheInspectorLoadsTheWholeCatalog()
        {
            var state = State();

            Assert.Equal(1011, state.Catalog.Count);
            Assert.True(state.HasSelection);
            Assert.Equal(1011, state.Matches().Count);
        }

        /// <summary>
        /// I-36D: the family filter offers S under its market name, and the authority warning appears for it
        /// and ONLY for it. The warning is not a setting — there is no switch here to turn it off.
        /// </summary>
        [Fact]
        public void TheSFamilyIsOfferedAndCarriesTheVisualDerivedWarning()
        {
            var state = State();
            state.Family = StructuralSectionFamily.S;
            state.EnsureSelectionIsVisible();

            Assert.Equal(28, state.Matches().Count);
            Assert.True(state.IsVisualDerived);

            var warning = state.AuthoritySummary();
            Assert.Contains("VISUAL DERIVADA", warning, StringComparison.Ordinal);
            Assert.Contains("CNC", warning, StringComparison.Ordinal);
            Assert.Contains("AISC", warning, StringComparison.Ordinal);

            Assert.Contains("S / IPS", StructuralSectionInspectorWindow.FamilyLabel(StructuralSectionFamily.S),
                StringComparison.Ordinal);
        }

        [Fact]
        public void TheWarningIsAbsentForEveryTabulatedConstrainedFamily()
        {
            foreach (var family in new[]
                     {
                         StructuralSectionFamily.W, StructuralSectionFamily.HssRectangular,
                         StructuralSectionFamily.Channel, StructuralSectionFamily.Angle
                     })
            {
                var state = State();
                state.Family = family;
                state.EnsureSelectionIsVisible();

                Assert.False(state.IsVisualDerived, family.ToString());
                Assert.Equal(string.Empty, state.AuthoritySummary());
            }
        }

        /// <summary>The plan the preview draws is the SAME artefact, carrying the authority with it.</summary>
        [Fact]
        public void ThePlanTheInspectorBuildsCarriesTheAuthority()
        {
            var state = State();
            state.Family = StructuralSectionFamily.S;
            state.EnsureSelectionIsVisible();

            var plan = state.BuildPlan();

            Assert.NotNull(plan);
            Assert.Equal(SectionGeometryAuthority.VisualDerived, plan.Authority);
            Assert.True(plan.IsVisualDerived);
        }

        [Theory]
        [InlineData(StructuralSectionFamily.W, 289)]
        [InlineData(StructuralSectionFamily.HssRectangular, 525)]
        [InlineData(StructuralSectionFamily.Channel, 32)]
        [InlineData(StructuralSectionFamily.Angle, 137)]
        [InlineData(StructuralSectionFamily.S, 28)]
        public void FilteringByFamilyNarrowsTheList(StructuralSectionFamily family, int expected)
        {
            var state = State();
            state.Family = family;

            Assert.Equal(expected, state.Matches().Count);
            Assert.All(state.Matches(), section => Assert.Equal(family, section.Family));
        }

        [Fact]
        public void SearchingMatchesTheManualLabel()
        {
            var state = State();
            state.Search = "HSS4X4X1/4";

            var match = Assert.Single(state.Matches());
            Assert.Equal("HSS4X4X1/4", match.DisplayName);
        }

        [Fact]
        public void SearchingMatchesTheEdiDesignationToo()
        {
            // La misma seccion se encuentra por su forma EDI, que es la que lleva el id.
            var state = State();
            state.Search = "HSS4X4X.250";

            var match = Assert.Single(state.Matches());
            Assert.Equal("AISC-HSS-RECT-HSS4X4X_250", match.SectionId.Value);
        }

        [Fact]
        public void SearchingIgnoresCaseAndPunctuation()
        {
            var state = State();
            state.Search = " w12x26 ";

            Assert.Contains(state.Matches(), s => s.DisplayName == "W12X26");
        }

        [Fact]
        public void OnlyEnabledSectionsAreOfferedForANewSelection()
        {
            // El overlay distribuido esta vacio, asi que hoy coinciden; lo que fija la prueba es que el
            // inspector consulta la vista de HABILITADAS y no el catalogo completo.
            var state = State();

            Assert.Equal(state.Catalog.Enabled.Count, state.Matches().Count);
            Assert.All(state.Matches(), section => Assert.True(section.IsEnabled));
        }

        [Fact]
        public void ChangingTheFilterKeepsTheSelectionValid()
        {
            var state = State();
            state.Search = "W12X26";
            state.EnsureSelectionIsVisible();
            Assert.Equal("W12X26", state.Selected.DisplayName);

            state.Search = string.Empty;
            state.Family = StructuralSectionFamily.Channel;
            state.EnsureSelectionIsVisible();

            Assert.Equal(StructuralSectionFamily.Channel, state.Selected.Family);
        }

        [Fact]
        public void AFilterThatMatchesNothingLeavesNoSelectionAndNoPlan()
        {
            var state = State();
            state.Search = "NO-EXISTE-ESTA-SECCION";
            state.EnsureSelectionIsVisible();

            Assert.False(state.HasSelection);
            Assert.Null(state.BuildPlan());
            Assert.Equal(0.0, state.TotalWeight());
        }

        // ---- Longitud ---------------------------------------------------------------------------------------

        [Theory]
        [InlineData("0")]
        [InlineData("-5")]
        [InlineData("abc")]
        [InlineData("")]
        public void AnInvalidLengthIsRejectedAndThePreviousOneSurvives(string text)
        {
            var state = State();
            state.Length = 96.0;

            Assert.False(state.TrySetLength(text));
            Assert.Equal(96.0, state.Length);
        }

        [Fact]
        public void AValidLengthIsAccepted()
        {
            var state = State();

            Assert.True(state.TrySetLength("144"));
            Assert.Equal(144.0, state.Length);
        }

        [Fact]
        public void SettingANonPositiveLengthDirectlyIsIgnored()
        {
            var state = State();
            var before = state.Length;

            state.Length = 0.0;
            state.Length = double.NaN;
            state.Length = double.PositiveInfinity;

            Assert.Equal(before, state.Length);
        }

        // ---- Representacion ---------------------------------------------------------------------------------

        [Fact]
        public void ThePlanFollowsTheChosenView()
        {
            var state = State();
            state.Search = "W12X26";
            state.EnsureSelectionIsVisible();
            state.Length = 120.0;

            state.View = SectionViewKind.CrossSection;
            Assert.Equal(12.2, state.BuildPlan().Bounds.Height, 6);

            state.View = SectionViewKind.LongitudinalX;
            Assert.Equal(120.0, state.BuildPlan().Bounds.Width, 6);
        }

        [Fact]
        public void ThePlanFollowsTheChosenDetail()
        {
            var state = State();
            state.Search = "W12X26";
            state.EnsureSelectionIsVisible();

            state.Detail = SectionDetailLevel.Simplified;
            Assert.Equal(SectionFidelity.Simplified, state.BuildPlan().Fidelity);

            state.Detail = SectionDetailLevel.Tabulated;
            Assert.Equal(SectionFidelity.TabulatedComplete, state.BuildPlan().Fidelity);
        }

        [Fact]
        public void RotationAndMirrorReachThePlan()
        {
            var state = State();
            state.Search = "C10X15.3";
            state.EnsureSelectionIsVisible();
            state.View = SectionViewKind.CrossSection;

            var upright = state.BuildPlan().Bounds;

            state.RotationDegrees = 90.0;
            var turned = state.BuildPlan().Bounds;
            Assert.Equal(upright.Height, turned.Width, 6);

            state.RotationDegrees = 0.0;
            state.Mirrored = true;
            var mirrored = state.BuildPlan().Bounds;
            Assert.True(Math.Abs(mirrored.MinX) > mirrored.MaxX);
        }

        [Fact]
        public void ModeAxisAndEnvelopeReachThePlan()
        {
            var state = State();
            state.Search = "W12X26";
            state.EnsureSelectionIsVisible();
            state.View = SectionViewKind.LongitudinalX;

            state.Mode = SectionRepresentationMode.Envelope;
            Assert.Single(state.BuildPlan().Curves);

            state.Mode = SectionRepresentationMode.Wireframe;
            state.ShowAxis = true;
            state.ShowEnvelope = true;
            var plan = state.BuildPlan();

            Assert.NotEmpty(plan.CurvesOf(SectionCurveRole.Axis));
            Assert.Single(plan.CurvesOf(SectionCurveRole.Envelope));
        }

        [Fact]
        public void ThePreviewShowsTheBoreOfATubeSeenSideOn()
        {
            // La ventana no calcula geometria: recibe el plan. Lo que se comprueba aqui es que lo que llega al
            // preview trae ya las lineas interiores, para que un tubo no se dibuje como una barra maciza.
            var state = State();
            state.Search = "HSS4X4X1/4";
            state.EnsureSelectionIsVisible();
            state.View = SectionViewKind.LongitudinalX;

            var plan = state.BuildPlan();

            Assert.NotEmpty(plan.CurvesOf(SectionCurveRole.InteriorGeneratrix));
            Assert.NotEmpty(plan.CurvesOf(SectionCurveRole.Generatrix));
        }

        [Fact]
        public void ThePreviewNeverReceivesADegenerateClosedCurve()
        {
            // Un rectangulo cerrado de area cero se ve como una raya en el preview y como una polilinea
            // imposible de seleccionar en AutoCAD. Las dos superficies consumen el MISMO plan, asi que basta
            // comprobarlo una vez.
            var state = State();

            foreach (var view in new[]
                     {
                         SectionViewKind.CrossSection, SectionViewKind.LongitudinalX,
                         SectionViewKind.LongitudinalY, SectionViewKind.Isometric
                     })
            {
                foreach (var id in new[] { "W12X26", "HSS4X4X1/4", "C10X15.3", "L8X6X1" })
                {
                    state.Search = id;
                    state.EnsureSelectionIsVisible();
                    state.View = view;
                    state.ShowAxis = true;
                    state.ShowEnvelope = true;

                    Assert.All(state.BuildPlan().Curves, curve =>
                        Assert.True(!curve.IsClosed || SectionPlanCurve.ProjectedDimensionality(curve.Points) == 2,
                            id + " " + view + ": " + curve.Role + " cerrada y plana"));
                }
            }
        }

        [Fact]
        public void ChangingTheSelectionRecomputesThePlan()
        {
            var state = State();
            state.Search = "W12X26";
            state.EnsureSelectionIsVisible();
            var first = state.BuildPlan().Signature();

            state.Search = "C15X50";
            state.EnsureSelectionIsVisible();
            var second = state.BuildPlan().Signature();

            Assert.NotEqual(first, second);
            Assert.StartsWith("AISC-C-C15X50", second, StringComparison.Ordinal);
        }

        // ---- Lo que el usuario lee ---------------------------------------------------------------------------

        [Fact]
        public void TheWeightIsTotalAndScalesWithLength()
        {
            var state = State();
            state.Search = "W12X26";
            state.EnsureSelectionIsVisible();

            state.Length = 12.0;
            var oneFoot = state.TotalWeight();

            state.Length = 120.0;
            Assert.Equal(oneFoot * 10.0, state.TotalWeight(), 9);
        }

        [Fact]
        public void TheWeightSummaryShowsTheNativeUnitFirst()
        {
            var state = State();
            state.Search = "W12X26";
            state.EnsureSelectionIsVisible();

            var summary = state.WeightSummary();

            Assert.StartsWith("W12X26 — 26 lb/ft (38.7 kg/m)", summary, StringComparison.Ordinal);
            Assert.Contains("total", summary, StringComparison.Ordinal);
        }

        [Fact]
        public void TheFidelityAndItsDiagnosticsAreShown()
        {
            var state = State();
            state.Search = "C10X15.3";
            state.EnsureSelectionIsVisible();

            Assert.Contains("derivada", state.FidelitySummary(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains(state.Diagnostics(),
                d => d.Code == SectionGeometryDiagnostics.ToeRoundingNotPublished);
        }

        // ---- Fronteras -----------------------------------------------------------------------------------------

        [Fact]
        public void TheInspectorHasNoMemberOrCantileverConcepts()
        {
            // Guarda de alcance: si alguien empieza a convertir el inspector en un configurador de miembro,
            // esta prueba lo dice antes de que llegue lejos.
            var forbidden = new[]
            {
                "Role", "Material", "Load", "Level", "Arm", "Connector", "Bracket", "Punch", "Weld",
                "Cantilever", "Member", "Save", "Persist"
            };

            var names = typeof(StructuralSectionInspectorState).GetProperties().Select(p => p.Name)
                .Concat(typeof(StructuralSectionInspectorState).GetMethods().Select(m => m.Name))
                .ToArray();

            foreach (var word in forbidden)
            {
                Assert.DoesNotContain(names, name => name.Contains(word, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void TheUiDoesNotReferenceAutodesk()
        {
            // La direccion de dependencias de AGENTS: nada de AutoCAD fuera del Plugin.
            var referenced = typeof(StructuralSectionInspectorState).Assembly
                .GetReferencedAssemblies()
                .Select(a => a.Name)
                .ToArray();

            Assert.DoesNotContain(referenced, name => name.StartsWith("Ac", StringComparison.Ordinal) ||
                                                      name.Contains("Autodesk", StringComparison.OrdinalIgnoreCase));
        }
    }
}
