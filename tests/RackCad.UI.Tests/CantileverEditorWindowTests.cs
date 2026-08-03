using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Editor;
using RackCad.UI.Systems.Cantilever;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-37D gate 4 — STA tests for the REAL <see cref="RackCantileverWindow"/>.
    ///
    /// They drive it through its own WPF event surface: setting a combo raises the window's SelectionChanged, a
    /// numeric field commits on LostFocus, and a matrix cell is a real Button whose Click runs the real handler. So
    /// what is being tested is the window a user operates, not a shortcut past it.
    ///
    /// What they lock: a new line opens BLOCKED because no section id is invented; the matrix has exactly
    /// station × level × side cells; the apply scopes reach what they claim and store nothing when the arm equals
    /// the default; and the three inserts produce the typed request with the view/section the envelope expects.
    /// </summary>
    public sealed class CantileverEditorWindowTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";

        // ---- Helpers -------------------------------------------------------------------------------------------

        private static void SetSection(Window window, string name, string sectionId)
        {
            var box = window.FindName(name) as ComboBox
                ?? throw new InvalidOperationException("No hay un ComboBox '" + name + "'.");

            box.SelectedValue = sectionId; // raises the window's real SelectionChanged → recompute
        }

        private static void SetCombo(Window window, string name, int index)
        {
            var box = window.FindName(name) as ComboBox
                ?? throw new InvalidOperationException("No hay un ComboBox '" + name + "'.");

            box.SelectedIndex = index;
        }

        /// <summary>
        /// Fills the window with a line that resolves.
        ///
        /// RONDA 2: the LINE fields are typed through their real controls, as a user would. The COMPONENT values
        /// are written on the design, because after the refactor they are not in this window at all — they are
        /// edited in <c>CantileverColumnBaseWindow</c> and <c>CantileverArmWindow</c>, which have their own
        /// tests. Driving them from here would be testing a layout the owner rejected.
        /// </summary>
        private static void Configure(RackCantileverWindow window, int stations = 3, int levels = 3)
        {
            var template = window.Design.StationTopology.ColumnBaseTemplate;
            template.ColumnSectionId = ColumnW;
            template.Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 };
            template.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            template.Connection.Punches.ColumnTopPunchOffset = 4.0;

            window.Design.DefaultArmTemplate = new CantileverArmTemplateDesign
            {
                Body = new CantileverArmBodyDesign { SectionId = ArmHss, CutLength = 36.0 },
                MountingPlate = new CantileverArmMountingPlateTemplateDesign
                {
                    VerticalPunchCount = 2,
                    VerticalEndOffset = 1.5
                }
            };

            EditorWindowTestSupport.SetNumberAndCommit(window, "StationCountBox", stations);
            EditorWindowTestSupport.SetNumberAndCommit(window, "SpacingBox", 96.0);
            EditorWindowTestSupport.SetNumberAndCommit(window, "LevelCountBox", levels);
            EditorWindowTestSupport.SetNumberAndCommit(window, "ClearHeightBox", 24.0);
        }

        /// <summary>Applies an arm to a scope the way the window does, without opening the modal configurator.</summary>
        private static CantileverLineMatrixChange ApplyArm(
            RackCantileverWindow window,
            CantileverLineApplyScope scope,
            CantileverLineCell anchor,
            double cutLength)
        {
            var arm = window.Matrix.Effective(anchor).DeepCopy();
            arm.Body.CutLength = cutLength;
            return window.Matrix.Apply(scope, anchor, arm);
        }

        /// <summary>Every matrix cell button, keyed by the cell it carries in its Tag.</summary>
        private static Dictionary<CantileverLineCell, Button> Cells(RackCantileverWindow window)
        {
            var grid = window.FindName("MatrixGrid") as Grid
                ?? throw new InvalidOperationException("No hay MatrixGrid.");

            return grid.Children
                .OfType<Button>()
                .Where(b => b.Tag is CantileverLineCell)
                .ToDictionary(b => (CantileverLineCell)b.Tag, b => b);
        }

        private static void ClickCell(RackCantileverWindow window, CantileverLineCell cell)
        {
            var button = Cells(window)[cell];
            button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, button));
        }

        // ---- 1. A new line ------------------------------------------------------------------------------------

        [Fact]
        public void ANewLineOpensBlocked_BecauseNoSectionIdIsInvented()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);

                return (w.Session != null,
                    w.Assembler != null,
                    w.Design.StationCount,
                    w.Design.StationTopology.ColumnBaseTemplate.ColumnSectionId,
                    w.Design.DefaultArmTemplate.Body.SectionId,
                    w.CurrentInputsAreValid,
                    ((Button)w.FindName("InsertFrontalButton")).IsEnabled);
            });

            Assert.True(r.Item1);
            Assert.True(r.Item2);
            Assert.Equal(2, r.Item3);          // the minimum line: two stations
            Assert.Null(r.Item4);              // no column section invented
            Assert.Null(r.Item5);              // no arm section invented
            Assert.False(r.Item6);             // and therefore nothing to draw
            Assert.False(r.Item7);
        }

        [Fact]
        public void TheDefaultSeparatorIsTheApprovedFourInchChannel_AndTheBraceHasNoDefaultSection()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                return (w.Design.Bracing.SeparatorSectionId, w.Design.Bracing.BraceSectionId);
            });

            Assert.Equal("AISC-C-C4X4_5", r.Item1);
            Assert.Null(r.Item2); // a structural brace has no approved id: it is rejected, not guessed
        }

        // ---- 2. A configured line resolves --------------------------------------------------------------------

        [Fact]
        public void ConfiguringTheSectionsAndTheMandatoryMarginsResolvesTheLine()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);

                return (w.CurrentInputsAreValid,
                    w.LastComputation?.Line?.StationCount ?? 0,
                    w.LastComputation?.Line?.IntervalCount ?? 0,
                    w.LastComputation?.Bom != null,
                    ((Button)w.FindName("InsertFrontalButton")).IsEnabled,
                    ((TextBlock)w.FindName("StatusText")).Text);
            });

            Assert.True(r.Item1, r.Item6);
            Assert.Equal(3, r.Item2);
            Assert.Equal(2, r.Item3); // three stations, two intervals
            Assert.True(r.Item4);
            Assert.True(r.Item5);
        }

        [Fact]
        public void UnCampoDeLINEAVacioSeReportaPorSuNombre()
        {
            // Los campos de COMPONENTE ya no estan aqui; los de linea si, y siguen diciendo cual falta.
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);
                EditorWindowTestSupport.SetNumberAndCommit(w, "ClearHeightBox", null);

                return (w.CurrentInputsAreValid, ((TextBlock)w.FindName("StatusText")).Text);
            });

            Assert.False(r.Item1);
            Assert.Contains("Claro solicitado", r.Item2, StringComparison.Ordinal);
        }

        [Fact]
        public void UnaLineaSinLosMargenesLEGACYSIGUERESOLVIENDO()
        {
            // I-37D ronda 2, motivo 1: los dos margenes dejaron de ser entradas del diseno. Ninguna linea
            // nueva los trae, y ninguna se bloquea por ello.
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);
                w.Design.StationTopology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = null;
                w.Design.StationTopology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = null;
                EditorWindowTestSupport.SetNumberAndCommit(w, "ClearHeightBox", 24.0);

                return (w.CurrentInputsAreValid, ((TextBlock)w.FindName("StatusText")).Text);
            });

            Assert.True(r.Item1, r.Item2);
        }

        [Fact]
        public void ADefaultThatDoesNotFitTheBoxFormatIsNotDegradedWhenItIsReadBack()
        {
            // REGRESION. The top clear factor is one third and the boxes show three decimals, so a plain
            // "read the box back into the design" turns 0.3333… into 0.333 — three ten-thousandths BELOW the
            // floor its own authority approves. The line then refuses to resolve and the user, who touched
            // nothing, is told the station failed. A field nobody edited must not change the design.
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);

                return (w.Design.StationTopology.TopClearFactor, w.CurrentInputsAreValid,
                    ((TextBlock)w.FindName("StatusText")).Text);
            });

            Assert.Equal(CantileverStationDefaults.TopClearFactor, r.Item1, 12);
            Assert.True(r.Item2, r.Item3);
        }

        [Fact]
        public void EditingThatFieldStillTakesTheValueTheUserTyped()
        {
            // The other half of the same rule: preserving an untouched default must not make the field read-only.
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);
                EditorWindowTestSupport.SetNumberAndCommit(w, "TopClearFactorBox", 0.5);

                return w.Design.StationTopology.TopClearFactor;
            });

            Assert.Equal(0.5, r, 9);
        }

        // ---- 3. The matrix ------------------------------------------------------------------------------------

        [Fact]
        public void TheMatrixHasOneCellPerStationLevelAndActiveSide()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w, stations: 3, levels: 2);
                var single = Cells(w).Count;

                SetCombo(w, "FaceModeBox", 1); // doble: both sides carry arms
                var doble = Cells(w).Count;

                return (single, doble);
            });

            Assert.Equal(6, r.single);  // 3 × 2 × 1 side
            Assert.Equal(12, r.doble);  // 3 × 2 × 2 sides
        }

        [Fact]
        public void ApplyingToOneStationReachesThatStationAndLeavesTheRestOnTheDefault()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w, stations: 3, levels: 2);

                var anchor = new CantileverLineCell(1, 0, CantileverArmSide.PositiveY);
                ClickCell(w, anchor);
                SetCombo(w, "ScopeBox", (int)CantileverLineApplyScope.Station);
                ApplyArm(w, CantileverLineApplyScope.Station, anchor, 60.0);

                var matrix = w.Matrix;

                return (matrix.OverrideCount,
                    matrix.Effective(new CantileverLineCell(1, 1, CantileverArmSide.PositiveY)).Body.CutLength,
                    matrix.Effective(new CantileverLineCell(0, 0, CantileverArmSide.PositiveY)).Body.CutLength,
                    w.CurrentInputsAreValid);
            });

            Assert.Equal(2, r.Item1);        // both cells of station 2, and only those
            Assert.Equal(60.0, r.Item2, 9);
            Assert.Equal(36.0, r.Item3, 9);  // station 1 still follows the line default
            Assert.True(r.Item4);
        }

        [Fact]
        public void AnArmEqualToTheDefaultIsNotStoredAsAnException()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w, stations: 2, levels: 1);

                var anchor = new CantileverLineCell(0, 0, CantileverArmSide.PositiveY);
                ClickCell(w, anchor);
                ApplyArm(w, CantileverLineApplyScope.Cell, anchor, w.Design.DefaultArmTemplate.Body.CutLength);

                return (w.Matrix.OverrideCount, w.Design.ArmCellOverrides.Count);
            });

            Assert.Equal(0, r.Item1);
            Assert.Empty(Enumerable.Range(0, r.Item2)); // the sparse list stayed empty
        }

        [Fact]
        public void UnaOperacionDeMatrizProduceUNASolaRegeneracion()
        {
            // CARACTERIZACION de ronda 2. Aplicar a doce celdas debe reconstruir la linea UNA vez, no doce:
            // una regeneracion por celda es invisible a simple vista y ruinosa en una linea larga.
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w, stations: 3, levels: 2);
                SetCombo(w, "FaceModeBox", 1); // doble: 12 celdas

                var anchor = new CantileverLineCell(0, 0, CantileverArmSide.PositiveY);
                ClickCell(w, anchor);
                SetCombo(w, "ScopeBox", (int)CantileverLineApplyScope.Line);

                ApplyArm(w, CantileverLineApplyScope.Line, anchor, 54.0);

                // «Restaurar» es un gesto REAL de la ventana y hace exactamente lo que hace «Editar brazo»
                // cuando el configurador devuelve algo: una escritura de matriz y UNA reconstruccion.
                var before = w.RecomputeCount;
                EditorWindowTestSupport.ClickNamed(w, "RestoreCellButton");
                var after = w.RecomputeCount;

                return (before, after, w.Matrix.OverrideCount, w.Matrix.Cells.Count);
            });

            Assert.Equal(12, r.Item4);
            Assert.Equal(0, r.Item3);          // «Restaurar» sobre toda la linea borro las doce excepciones
            Assert.Equal(1, r.Item2 - r.Item1); // y lo hizo con UNA sola reconstruccion, no doce
        }

        [Fact]
        public void RestoringAScopeClearsItsExceptions()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w, stations: 2, levels: 2);

                var anchor = new CantileverLineCell(0, 0, CantileverArmSide.PositiveY);
                ClickCell(w, anchor);
                SetCombo(w, "ScopeBox", (int)CantileverLineApplyScope.Line);
                ApplyArm(w, CantileverLineApplyScope.Line, anchor, 72.0);
                var afterApply = w.Matrix.OverrideCount;

                EditorWindowTestSupport.ClickNamed(w, "RestoreCellButton");

                return (afterApply, w.Matrix.OverrideCount);
            });

            Assert.Equal(4, r.Item1);
            Assert.Equal(0, r.Item2);
        }

        // ---- 4. The insert/update contract --------------------------------------------------------------------

        [Fact]
        public void InsertingTheFrontalProducesAWholeLineRequest()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);
                EditorWindowTestSupport.SetText(w, "NameBox", "Linea A");
                EditorWindowTestSupport.ClickNamed(w, "InsertFrontalButton");

                var request = w.InsertionRequest as CantileverInsertionRequest;

                return (w.InsertRequested, request?.View, request?.Section ?? -99, request?.Line != null,
                    request?.Design != null, w.RackName, w.UpdateOnly);
            });

            Assert.True(r.Item1);
            Assert.Equal(RackEmbedDocument.ViewFrontal, r.Item2);
            Assert.Equal(-1, r.Item3);   // the frontal is a view of the LINE
            Assert.True(r.Item4);        // the RESOLVED line rides along; the host never re-resolves it
            Assert.True(r.Item5);
            Assert.Equal("Linea A", r.Item6);
            Assert.False(r.Item7);
        }

        [Fact]
        public void InsertingALateralCarriesItsStationIndex()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);
                EditorWindowTestSupport.SetNumberAndCommit(w, "LateralStationBox", 3.0); // base-one in the box
                EditorWindowTestSupport.ClickNamed(w, "InsertLateralButton");

                var request = w.InsertionRequest as CantileverInsertionRequest;
                return (request?.View, request?.Section ?? -99);
            });

            Assert.Equal(RackEmbedDocument.ViewLateral, r.Item1);
            Assert.Equal(2, r.Item2); // base-zero in the envelope
        }

        [Fact]
        public void InsertingThePlantaIsAlsoAWholeLineView()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);
                EditorWindowTestSupport.ClickNamed(w, "InsertPlantaButton");

                var request = w.InsertionRequest as CantileverInsertionRequest;
                return (request?.View, request?.Section ?? -99);
            });

            Assert.Equal(RackEmbedDocument.ViewPlanta, r.Item1);
            Assert.Equal(-1, r.Item2);
        }

        [Fact]
        public void ALineThatDoesNotResolveCannotBeInserted()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                EditorWindowTestSupport.ClickNamed(w, "InsertFrontalButton"); // still blocked: no sections

                return (w.InsertRequested, ((TextBlock)w.FindName("StatusText")).Text);
            });

            Assert.False(r.Item1);
            Assert.NotEmpty(r.Item2);
        }

        [Fact]
        public void UpdateIsOnlyForALineOpenedFromTheDrawing_AndTheEditKeepsItsGuid()
        {
            var design = new CantileverLineDesign();
            var project = RackProject.ForCantilever(design);

            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);
                var updateForNew = ((Button)w.FindName("UpdateButton")).IsEnabled;

                w.LoadExisting(design, "11111111-2222-3333-4444-555555555555", "Linea guardada", project);
                Configure(w);
                var updateForExisting = ((Button)w.FindName("UpdateButton")).IsEnabled;

                EditorWindowTestSupport.ClickNamed(w, "UpdateButton");
                var request = w.InsertionRequest as CantileverInsertionRequest;

                return (updateForNew, updateForExisting, w.RackId, w.UpdateOnly, request?.View, request?.Section ?? -99);
            });

            Assert.False(r.Item1);
            Assert.True(r.Item2);
            Assert.Equal("11111111-2222-3333-4444-555555555555", r.Item3); // an edit NEVER mints a new GUID
            Assert.True(r.Item4);
            Assert.Null(r.Item5);   // an update redraws every existing view: no single view is requested
            Assert.Equal(-1, r.Item6);
        }

        [Fact]
        public void OpeningFromTheLibraryKeepsTheSourceProjectAndMintsAFreshGuidOnInsert()
        {
            var design = new CantileverLineDesign();
            var source = RackProject.ForCantilever(design);

            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                w.LoadDesignForNew(design, "Plantilla Cantilever", source);
                var idBefore = w.RackId;
                Configure(w);
                EditorWindowTestSupport.ClickNamed(w, "InsertFrontalButton");

                var request = w.InsertionRequest as CantileverInsertionRequest;

                return (idBefore, w.RackId, w.RackName,
                    request != null && ReferenceEquals(request.SourceProject, source));
            });

            Assert.Null(r.Item1);                        // no identity until insert
            Assert.True(Guid.TryParse(r.Item2, out _));  // a FRESH GUID, never the library entry's
            Assert.Equal("Plantilla Cantilever", r.Item3);
            Assert.True(r.Item4);                        // the library metadata rode into the payload (I-11)
        }

        // ---- 5. The preview ------------------------------------------------------------------------------------

        [Fact]
        public void ThePreviewShowsTheViewTheUserSelected_AndTheLateralFollowsItsStationBox()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);

                var frontal = w.CurrentPreviewPlan;

                ((System.Windows.Controls.RadioButton)w.FindName("PreviewLateralRadio")).IsChecked = true;
                EditorWindowTestSupport.SetNumberAndCommit(w, "LateralStationBox", 2.0);
                var lateral = w.CurrentPreviewPlan;

                ((System.Windows.Controls.RadioButton)w.FindName("PreviewPlantaRadio")).IsChecked = true;
                var planta = w.CurrentPreviewPlan;

                return (frontal.View, lateral.View, lateral.StationIndex, planta.View);
            });

            Assert.Equal(CantileverViewKind.Frontal, r.Item1);
            Assert.Equal(CantileverViewKind.Lateral, r.Item2);
            Assert.Equal(1, r.Item3);
            Assert.Equal(CantileverViewKind.Planta, r.Item4);
        }

        // ---- Ronda 3: los interruptores de la PLANTA ---------------------------------------------------------

        private static CheckBox Check(Window window, string name) =>
            window.FindName(name) as CheckBox
                ?? throw new InvalidOperationException("No hay un CheckBox '" + name + "'.");

        [Fact]
        public void LosDosInterruptoresDeLaPlantaNACENApagados()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);

                ((RadioButton)w.FindName("PreviewPlantaRadio")).IsChecked = true;
                var planta = w.CurrentPreviewPlan;

                return (Check(w, "PlantaShowArmsCheck").IsChecked == true,
                    Check(w, "PlantaShowBracesCheck").IsChecked == true,
                    planta.Of(CantileverViewPieceKind.Arm).Any(),
                    planta.Of(CantileverViewPieceKind.Brace).Any(),
                    planta.Of(CantileverViewPieceKind.Column).Any());
            });

            Assert.False(r.Item1);
            Assert.False(r.Item2);
            Assert.False(r.Item3);
            Assert.False(r.Item4);

            // Y lo que la planta existe para enseñar sigue estando.
            Assert.True(r.Item5);
        }

        [Fact]
        public void EncenderUnInterruptorDevuelveSuFamiliaALaPlanta()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);

                ((RadioButton)w.FindName("PreviewPlantaRadio")).IsChecked = true;

                var arms = Check(w, "PlantaShowArmsCheck");
                arms.IsChecked = true;
                arms.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                var conBrazos = w.CurrentPreviewPlan;

                var braces = Check(w, "PlantaShowBracesCheck");
                braces.IsChecked = true;
                braces.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                var conTodo = w.CurrentPreviewPlan;

                return (conBrazos.Of(CantileverViewPieceKind.Arm).Any(),
                    conBrazos.Of(CantileverViewPieceKind.Brace).Any(),
                    conTodo.Of(CantileverViewPieceKind.Arm).Any(),
                    conTodo.Of(CantileverViewPieceKind.Brace).Any());
            });

            Assert.True(r.Item1);
            Assert.False(r.Item2);   // cada interruptor manda SOLO sobre lo suyo
            Assert.True(r.Item3);
            Assert.True(r.Item4);
        }

        [Fact]
        public void LosInterruptoresDeLaPlantaNoVuelvenAResolverLaLinea()
        {
            // La prueba de que son de DIBUJO y no de producto: la línea no se recalcula, así que ni el BOM ni
            // la firma física pueden moverse por tocarlos.
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);

                var before = w.RecomputeCount;

                var arms = Check(w, "PlantaShowArmsCheck");
                arms.IsChecked = true;
                arms.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                var braces = Check(w, "PlantaShowBracesCheck");
                braces.IsChecked = true;
                braces.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                return (before, w.RecomputeCount);
            });

            Assert.Equal(r.Item1, r.Item2);
        }

        [Fact]
        public void LosInterruptoresNoTocanNiLaFrontalNiLaLateral()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);

                var frontalAntes = w.CurrentPreviewPlan.Curves.Count;

                ((RadioButton)w.FindName("PreviewLateralRadio")).IsChecked = true;
                var lateralAntes = w.CurrentPreviewPlan.Curves.Count;

                var arms = Check(w, "PlantaShowArmsCheck");
                arms.IsChecked = true;
                arms.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                var lateralDespues = w.CurrentPreviewPlan.Curves.Count;

                ((RadioButton)w.FindName("PreviewFrontalRadio")).IsChecked = true;
                var frontalDespues = w.CurrentPreviewPlan.Curves.Count;

                return (frontalAntes, frontalDespues, lateralAntes, lateralDespues);
            });

            Assert.Equal(r.Item1, r.Item2);
            Assert.Equal(r.Item3, r.Item4);
        }

        [Fact]
        public void TheLateralOfAStationDoesNotShowTheBracing_BecauseTheBracingLivesBetweenStations()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackCantileverWindow(canInsertInAutoCad: true);
                Configure(w);

                var frontal = w.CurrentPreviewPlan;
                ((System.Windows.Controls.RadioButton)w.FindName("PreviewLateralRadio")).IsChecked = true;
                var lateral = w.CurrentPreviewPlan;

                return (frontal.Of(CantileverViewPieceKind.Separator).Any(),
                    lateral.Of(CantileverViewPieceKind.Separator).Any());
            });

            Assert.True(r.Item1);
            Assert.False(r.Item2);
        }
    }
}
