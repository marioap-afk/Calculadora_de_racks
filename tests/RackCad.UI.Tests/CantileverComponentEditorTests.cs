using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Controls;
using RackCad.UI.Editor;
using RackCad.UI.Systems.Cantilever.Components;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-37D ronda 2 — los CUATRO configuradores de componente, conducidos por sus controles reales.
    ///
    /// No se limitan a comprobar que existan controles con cierto nombre: verifican el MODELO efectivo que cada
    /// ventana devuelve y la FIRMA del plan que su preview dibuja. Un configurador que aceptara sin aplicar, o
    /// que mostrara una figura distinta de la que insertaría, pasaría una prueba de nombres y fallaría éstas.
    /// </summary>
    public sealed class CantileverComponentEditorTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string OtherW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";
        private const string ArmChannel = "AISC-C-C10X15_3";
        private const string SeparatorC = "AISC-C-C4X4_5";
        private const string BraceAngle = "AISC-L-L2X2X1_4";

        private static StructuralSectionCatalog Catalog() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        /// <summary>A column–base template that resolves, so a preview and a recipe exist.</summary>
        private static CantileverStationColumnBaseTemplateDesign ColumnBaseTemplate()
        {
            var template = new CantileverStationColumnBaseTemplateDesign
            {
                ColumnSectionId = ColumnW,
                Base = new CantileverBaseDesign { SectionId = ColumnW, Length = 48.0 }
            };

            template.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            template.Connection.Punches.ColumnTopPunchOffset = 4.0;
            return template;
        }

        private static CantileverArmTemplateDesign ArmTemplate() => new CantileverArmTemplateDesign
        {
            Body = new CantileverArmBodyDesign { SectionId = ArmHss, CutLength = 36.0 },
            MountingPlate = new CantileverArmMountingPlateTemplateDesign
            {
                VerticalPunchCount = 2,
                VerticalEndOffset = 1.5
            }
        };

        /// <summary>A resolved line, for the two windows whose piece only exists inside an interval.</summary>
        private static CantileverLineEditorComputation ResolvedLine()
        {
            var design = new CantileverLineDesign
            {
                StationCount = 3,
                ColumnCentreSpacing = 96.0,
                StationTopology = new CantileverLineStationTopologyDesign
                {
                    LevelCount = 2,
                    RequestedClearHeight = 24.0,
                    ColumnBaseTemplate = ColumnBaseTemplate()
                },
                DefaultArmTemplate = ArmTemplate()
            };

            return new CantileverLineEditorAssembler(Catalog()).Build(design);
        }

        private static void Layout(Window window)
        {
            window.Measure(new Size(1000, 700));
            window.Arrange(new Rect(0, 0, 1000, 700));
        }

        // ================= COLUMNA Y BASE =================

        [Fact]
        public void ColumnaBase_AbreCargaSusValoresYResuelve()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), canInsertInAutoCad: true);
                Layout(w);

                return (w.State.ColumnSectionId, w.State.BaseSectionId, w.Assembly != null && !w.Assembly.IsBlocked,
                    w.CurrentPreviewPlan != null, ((TextBlock)w.FindName("BomText")).Text,
                    ((TextBlock)w.FindName("DiagnosticsText")).Text);
            });

            Assert.Equal(ColumnW, r.Item1);
            Assert.Equal(ColumnW, r.Item2);
            Assert.True(r.Item3);
            Assert.True(r.Item4);
            Assert.Contains("Columna", r.Item5);
            Assert.Contains("Sin diagnósticos", r.Item6);
        }

        [Fact]
        public void ColumnaBase_CambiarLaColumnaConFollowArrastraLaBase()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), true);
                Layout(w);

                var picker = (StructuralSectionPicker)w.FindName("ColumnPicker");
                picker.ApplyTemplate();
                picker.SelectedSectionId = OtherW;
                picker.RaiseSectionChosen(OtherW);

                return (w.State.ColumnSectionId, w.State.BaseSectionId, w.State.BaseFollowsColumn);
            });

            Assert.Equal(OtherW, r.Item1);
            Assert.Equal(OtherW, r.Item2);
            Assert.True(r.Item3);
        }

        [Fact]
        public void ColumnaBase_CambiarLaBaseAManoApagaElFollowYLaColumnaYaNoLaArrastra()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), true);
                Layout(w);

                var basePicker = (StructuralSectionPicker)w.FindName("BasePicker");
                basePicker.ApplyTemplate();
                basePicker.RaiseSectionChosen(OtherW);

                var columnPicker = (StructuralSectionPicker)w.FindName("ColumnPicker");
                columnPicker.ApplyTemplate();
                columnPicker.RaiseSectionChosen(ColumnW);

                return (w.State.BaseFollowsColumn, w.State.BaseSectionId, w.State.ColumnSectionId);
            });

            Assert.False(r.Item1);
            Assert.Equal(OtherW, r.Item2); // la base del usuario sobrevive
            Assert.Equal(ColumnW, r.Item3);
        }

        [Fact]
        public void ColumnaBase_UsarMismaSeccionVuelveAEncenderElFollowYLoAplica()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), true);
                Layout(w);

                ((StructuralSectionPicker)w.FindName("BasePicker")).RaiseSectionChosen(OtherW);
                EditorWindowTestSupport.ClickNamed(w, "UseSameSectionButton");

                return (w.State.BaseFollowsColumn, w.State.BaseSectionId);
            });

            Assert.True(r.Item1);
            Assert.Equal(ColumnW, r.Item2);
        }

        [Fact]
        public void ColumnaBase_UnaSeccionNoElegibleComoBaseNoNormalizaYSeVEELDiagnostico()
        {
            // Y se ve AQUI, no al volver a la ventana principal: esa es la regresion 12.
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), true);
                Layout(w);

                ((StructuralSectionPicker)w.FindName("ColumnPicker")).RaiseSectionChosen(SeparatorC);

                return (w.State.BaseSectionId, ((TextBlock)w.FindName("DiagnosticsText")).Text);
            });

            Assert.Equal(ColumnW, r.Item1); // la base no se toco
            Assert.Contains("no es elegible como base", r.Item2, StringComparison.Ordinal);
        }

        [Fact]
        public void ColumnaBase_CancelarTrasVariosCambiosNoMutaElOriginal()
        {
            var original = ColumnBaseTemplate();

            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(original, Catalog(), true);
                Layout(w);

                ((StructuralSectionPicker)w.FindName("ColumnPicker")).RaiseSectionChosen(OtherW);
                ((StructuralSectionPicker)w.FindName("BasePicker")).RaiseSectionChosen(ColumnW);
                EditorWindowTestSupport.SetNumberAndCommit(w, "BaseLengthBox", 72.0);

                var mutated = w.State.Template.Base.Length;
                EditorWindowTestSupport.ClickNamed(w, "RestoreButton");

                return (mutated, w.State.Template.Base.Length, w.State.ColumnSectionId, w.Result == null);
            });

            Assert.Equal(72.0, r.Item1, 9);
            Assert.Equal(48.0, r.Item2, 9);   // restaurar volvio a lo que se abrio
            Assert.Equal(ColumnW, r.Item3);
            Assert.True(r.Item4);             // no se acepto nada

            // Y el original nunca cambio.
            Assert.Equal(ColumnW, original.ColumnSectionId);
            Assert.Equal(48.0, original.Base.Length, 9);
            Assert.True(original.BaseFollowsColumn);
        }

        [Fact]
        public void ColumnaBase_AceptarDevuelveElResultadoYNoLaInstanciaInterna()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), true);
                Layout(w);
                EditorWindowTestSupport.SetNumberAndCommit(w, "BaseLengthBox", 60.0);
                EditorWindowTestSupport.ClickNamed(w, "AcceptButton");

                var accepted = w.Result;
                accepted.Base.Length = 999.0;

                return (accepted != null, w.State.Template.Base.Length);
            });

            Assert.True(r.Item1);
            Assert.Equal(60.0, r.Item2, 9); // mutar lo aceptado no toca el editor
        }

        [Fact]
        public void ColumnaBase_InsertarProduceUnaPeticionConIdentidadPROPIA()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), true);
                Layout(w);
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");

                var request = w.ComponentInsertion;

                return (request != null, request?.Component, request?.Views.Count ?? 0,
                    request?.ComponentId ?? Guid.Empty, request?.BlockName(request.Views[0]));
            });

            Assert.True(r.Item1);
            Assert.Equal(CantileverComponentKind.ColumnBase, r.Item2);
            Assert.Equal(3, r.Item3);                       // frontal, lateral y planta
            Assert.NotEqual(Guid.Empty, r.Item4);
            Assert.StartsWith("RACKCAD_CANTILEVER_COMPONENTE_COLUMNA_BASE", r.Item5);
        }

        [Fact]
        public void ColumnaBase_ElPreviewYLoQueSeInsertaSonELMISMOPlan()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), true);
                Layout(w);

                var preview = w.CurrentPreviewPlan.Signature();
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");
                var inserted = w.ComponentInsertion.Views
                    .Single(v => v.View == CantileverViewKind.Frontal).Signature();

                return (preview, inserted);
            });

            Assert.Equal(r.Item1, r.Item2);
        }

        [Fact]
        public void ColumnaBase_NoSePuedeInsertarUnaPiezaQueNoResuelve()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverColumnBaseWindow(new CantileverStationColumnBaseTemplateDesign(), Catalog(), true);
                Layout(w);
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");

                return (w.ComponentInsertion == null, ((TextBlock)w.FindName("DiagnosticsText")).Text);
            });

            Assert.True(r.Item1);
            Assert.NotEmpty(r.Item2);
        }

        // ================= BRAZO =================

        [Fact]
        public void Brazo_ConservaTodosSusParametrosYLosDevuelveAlAceptar()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverArmWindow(ArmTemplate(), ColumnBaseTemplate(), Catalog());
                Layout(w);

                EditorWindowTestSupport.SetNumberAndCommit(w, "CutLengthBox", 48.0);
                EditorWindowTestSupport.SetNumberAndCommit(w, "SlopeBox", 0.25);
                EditorWindowTestSupport.SetNumberAndCommit(w, "PunchCountBox", 3.0);
                EditorWindowTestSupport.SetNumberAndCommit(w, "VerticalOffsetBox", 2.0);
                EditorWindowTestSupport.SetNumberAndCommit(w, "PlateThicknessBox", 0.5);
                EditorWindowTestSupport.SetNumberAndCommit(w, "EndPlateThicknessBox", 0.375);
                EditorWindowTestSupport.SetNumberAndCommit(w, "ExtraStopBox", 4.0);
                ((ComboBox)w.FindName("EndPlateModeBox")).SelectedIndex = (int)CantileverArmEndPlateMode.Stop;
                EditorWindowTestSupport.ClickNamed(w, "AcceptButton");

                var arm = w.Result;

                return (arm.Body.CutLength, arm.Body.SlopeRisePer12, arm.MountingPlate.VerticalPunchCount,
                    arm.MountingPlate.VerticalEndOffset, arm.MountingPlate.Thickness,
                    arm.EndPlate.Mode, arm.EndPlate.Thickness, arm.EndPlate.ExtraStopHeight);
            });

            Assert.Equal(48.0, r.Item1, 9);
            Assert.Equal(0.25, r.Item2, 9);
            Assert.Equal(3, r.Item3);
            Assert.Equal(2.0, r.Item4.Value, 9);
            Assert.Equal(0.5, r.Item5, 9);
            Assert.Equal(CantileverArmEndPlateMode.Stop, r.Item6);
            Assert.Equal(0.375, r.Item7, 9);
            Assert.Equal(4.0, r.Item8, 9);
        }

        [Theory]
        [InlineData(CantileverArmBodyArrangement.Single, ArmHss)]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelFacing, ArmChannel)]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelBackToBack, ArmChannel)]
        public void Brazo_LosTresArreglosSeEditanYSeDevuelven(
            CantileverArmBodyArrangement arrangement, string sectionId)
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverArmWindow(ArmTemplate(), ColumnBaseTemplate(), Catalog());
                Layout(w);

                ((StructuralSectionPicker)w.FindName("SectionPicker")).RaiseSectionChosen(sectionId);
                ((ComboBox)w.FindName("ArrangementBox")).SelectedIndex = (int)arrangement;
                EditorWindowTestSupport.ClickNamed(w, "AcceptButton");

                return (w.Result.Body.Arrangement, w.Result.Body.SectionId);
            });

            Assert.Equal(arrangement, r.Item1);
            Assert.Equal(sectionId, r.Item2);
        }

        [Fact]
        public void Brazo_UnMargenAusenteSeDiagnosticaYNoSeInventa()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverArmWindow(ArmTemplate(), ColumnBaseTemplate(), Catalog());
                Layout(w);
                EditorWindowTestSupport.SetNumberAndCommit(w, "VerticalOffsetBox", null);

                return (w.Working.MountingPlate.VerticalEndOffset, ((TextBlock)w.FindName("DiagnosticsText")).Text);
            });

            // Un campo en error BLOQUEA la escritura: el diseno conserva lo que tenia y el editor lo dice por su
            // nombre. Escribir null en silencio seria peor que no escribir nada.
            Assert.Equal(1.5, r.Item1.Value, 9);
            Assert.Contains("Margen vertical", r.Item2, StringComparison.Ordinal);
        }

        [Fact]
        public void Brazo_CancelarNoMutaElOriginal()
        {
            var original = ArmTemplate();

            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverArmWindow(original, ColumnBaseTemplate(), Catalog());
                Layout(w);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CutLengthBox", 90.0);
                EditorWindowTestSupport.ClickNamed(w, "RestoreButton");

                return (w.Working.Body.CutLength, w.Result == null);
            });

            Assert.Equal(36.0, r.Item1, 9);
            Assert.True(r.Item2);
            Assert.Equal(36.0, original.Body.CutLength, 9);
        }

        [Fact]
        public void Brazo_InsertarProduceSuPeticionConSuPropioId()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverArmWindow(ArmTemplate(), ColumnBaseTemplate(), Catalog());
                Layout(w);
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");

                var request = w.ComponentInsertion;
                return (request != null, request?.Component, request?.Views.Count ?? 0, request?.ComponentId ?? Guid.Empty);
            });

            Assert.True(r.Item1);
            Assert.Equal(CantileverComponentKind.Arm, r.Item2);
            Assert.True(r.Item3 >= 1); // la lateral siempre; las otras sólo si aportan algo distinto
            Assert.NotEqual(Guid.Empty, r.Item4);
        }

        // ================= SEPARADOR =================

        [Fact]
        public void Separador_AbreConElDefaultAprobadoYSusCuatroAgujeros()
        {
            var line = ResolvedLine();

            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverSeparatorWindow(
                    new CantileverBracingDesign(), Catalog(), line.Line.Separators.First());
                Layout(w);

                return (w.Working.SeparatorSectionId, line.Line.Separators.First().Punches.Count,
                    w.CurrentPreviewPlan != null, ((TextBlock)w.FindName("DerivedText")).Text);
            });

            Assert.Equal(SeparatorC, r.Item1);
            Assert.Equal(4, r.Item2);
            Assert.True(r.Item3);
            Assert.Contains("corte", r.Item4, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Separador_OtraSeccionElegibleSeAceptaYSeDevuelve()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverSeparatorWindow(new CantileverBracingDesign(), Catalog());
                Layout(w);

                ((StructuralSectionPicker)w.FindName("SectionPicker")).RaiseSectionChosen("AISC-C-C5X6_7");
                EditorWindowTestSupport.ClickNamed(w, "AcceptButton");

                return w.Result.SeparatorSectionId;
            });

            Assert.Equal("AISC-C-C5X6_7", r);
        }

        [Fact]
        public void Separador_SoloOfreceCanales()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverSeparatorWindow(new CantileverBracingDesign(), Catalog());
                Layout(w);

                var picker = (StructuralSectionPicker)w.FindName("SectionPicker");
                return picker.AllChoices.All(c => c.Family == StructuralSectionFamily.Channel);
            });

            Assert.True(r);
        }

        [Fact]
        public void Separador_SinSeccionLoDICEYNoSeInsertaSinLineaResuelta()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverSeparatorWindow(
                    new CantileverBracingDesign { SeparatorSectionId = null }, Catalog());
                Layout(w);

                var sinSeccion = ((TextBlock)w.FindName("DiagnosticsText")).Text;
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");

                return (sinSeccion, w.ComponentInsertion == null, ((TextBlock)w.FindName("DiagnosticsText")).Text);
            });

            Assert.Contains("Sin sección", r.Item1, StringComparison.Ordinal);
            Assert.True(r.Item2);
            Assert.Contains("Resuelve primero la línea", r.Item3, StringComparison.Ordinal);
        }

        [Fact]
        public void Separador_ConLineaResueltaSeInsertaConSuPropioId()
        {
            var line = ResolvedLine();

            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverSeparatorWindow(
                    new CantileverBracingDesign(), Catalog(), line.Line.Separators.First());
                Layout(w);
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");

                return (w.ComponentInsertion != null, w.ComponentInsertion?.Component,
                    w.ComponentInsertion?.ComponentId ?? Guid.Empty);
            });

            Assert.True(r.Item1);
            Assert.Equal(CantileverComponentKind.Separator, r.Item2);
            Assert.NotEqual(Guid.Empty, r.Item3);
        }

        // ================= TENSOR =================

        [Fact]
        public void Tensor_ColdRolledEsElDefaultYSuDiametroSeEdita()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverBraceWindow(new CantileverBracingDesign(), Catalog());
                Layout(w);

                var inicial = w.Working.ColdRolled.Diameter;
                EditorWindowTestSupport.SetNumberAndCommit(w, "RodDiameterBox", 1.0);
                EditorWindowTestSupport.ClickNamed(w, "AcceptButton");

                return (inicial, w.Result.ColdRolled.Diameter, w.Result.BraceKind);
            });

            Assert.Equal(0.75, r.Item1, 9);
            Assert.Equal(1.0, r.Item2, 9);
            Assert.Equal(CantileverBraceBodyKind.ColdRolledRound, r.Item3);
        }

        [Fact]
        public void Tensor_EstructuralSinSeccionSeRECHAZAYLoDICE()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverBraceWindow(new CantileverBracingDesign(), Catalog());
                Layout(w);
                ((ComboBox)w.FindName("KindBox")).SelectedIndex = 1;

                return (((TextBlock)w.FindName("DiagnosticsText")).Text, w.Working.BraceSectionId);
            });

            Assert.Contains("sin sección se rechaza", r.Item1, StringComparison.Ordinal);
            Assert.Null(r.Item2);
        }

        [Theory]
        [InlineData("AISC-C-C4X4_5")]
        [InlineData(BraceAngle)]
        public void Tensor_EstructuralAceptaCanalYAngulo(string sectionId)
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverBraceWindow(new CantileverBracingDesign(), Catalog());
                Layout(w);

                ((ComboBox)w.FindName("KindBox")).SelectedIndex = 1;
                ((StructuralSectionPicker)w.FindName("SectionPicker")).RaiseSectionChosen(sectionId);
                EditorWindowTestSupport.ClickNamed(w, "AcceptButton");

                return (w.Result.BraceKind, w.Result.BraceSectionId);
            });

            Assert.Equal(CantileverBraceBodyKind.StructuralSection, r.Item1);
            Assert.Equal(sectionId, r.Item2);
        }

        [Fact]
        public void Tensor_SoloOfreceCanalYAngulo()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverBraceWindow(new CantileverBracingDesign(), Catalog());
                Layout(w);

                return ((StructuralSectionPicker)w.FindName("SectionPicker")).AllChoices
                    .All(c => c.Family == StructuralSectionFamily.Channel || c.Family == StructuralSectionFamily.Angle);
            });

            Assert.True(r);
        }

        [Fact]
        public void Tensor_LaRecetaNombraAdaptadoresYCartabones()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverBraceWindow(new CantileverBracingDesign(), Catalog());
                Layout(w);
                var coldRolled = ((TextBlock)w.FindName("BomText")).Text;

                ((ComboBox)w.FindName("KindBox")).SelectedIndex = 1;
                var structural = ((TextBlock)w.FindName("BomText")).Text;

                return (coldRolled, structural);
            });

            Assert.Contains("adaptadores", r.Item1, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("cartabones", r.Item1, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("perfil", r.Item2, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Tensor_CancelarNoMutaElOriginal()
        {
            var original = new CantileverBracingDesign();

            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverBraceWindow(original, Catalog());
                Layout(w);
                EditorWindowTestSupport.SetNumberAndCommit(w, "RodDiameterBox", 2.0);
                EditorWindowTestSupport.ClickNamed(w, "RestoreButton");

                return (w.Working.ColdRolled.Diameter, w.Result == null);
            });

            Assert.Equal(0.75, r.Item1, 9);
            Assert.True(r.Item2);
            Assert.Equal(0.75, original.ColdRolled.Diameter, 9);
        }

        [Fact]
        public void Tensor_ConLineaResueltaSeInsertaEnUNSoloPlano()
        {
            var line = ResolvedLine();

            var r = StaTestRunner.Run(() =>
            {
                var w = new CantileverBraceWindow(new CantileverBracingDesign(), Catalog(), line.Line.Braces.First());
                Layout(w);
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");

                return (w.ComponentInsertion != null, w.ComponentInsertion?.Views.Count ?? 0,
                    w.ComponentInsertion?.Component);
            });

            Assert.True(r.Item1);
            Assert.Equal(1, r.Item2); // el plano del tensor, sin proyecciones redundantes
            Assert.Equal(CantileverComponentKind.Brace, r.Item3);
        }

        // ================= IDENTIDAD DE LOS COMPONENTES =================

        [Fact]
        public void DosComponentesInsertadosRecibenIdentidadesDISTINTAS()
        {
            var r = StaTestRunner.Run(() =>
            {
                var first = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), true);
                Layout(first);
                EditorWindowTestSupport.ClickNamed(first, "InsertButton");

                var second = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), true);
                Layout(second);
                EditorWindowTestSupport.ClickNamed(second, "InsertButton");

                return (first.ComponentInsertion.ComponentId, second.ComponentInsertion.ComponentId);
            });

            Assert.NotEqual(r.Item1, r.Item2);
        }
    }
}
