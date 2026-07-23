using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI;
using RackCad.UI.Controls;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-18b increment 3a — STA tests for the REAL <see cref="RackPushBackSystemWindow"/>. They lock that the window is a thin
    /// shell over the pure model: it adopts the shared session + the editor state + the assembler; LoadNew/LoadDesignForNew/
    /// LoadExisting behave; the jagged topes matrix, scope apply and safety filter work through the real controls; recompute
    /// accepts only valid models; the view selector + insert/update produce the typed <see cref="PushBackInsertionRequest"/>;
    /// the library payload preserves metadata without flagging an insert; and RackCad.UI references no AutoCAD assembly.
    /// </summary>
    public sealed class PushBackEditorWindowTests
    {
        // ---- Fixtures + helpers ----------------------------------------------------------------------------------

        private static PushBackDesign SampleDesign(int front0Levels = 3, int front1Levels = 2)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = Math.Max(front0Levels, front1Levels),
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = front0Levels, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = front1Levels, PalletsDeep = 4, DepthStartPosition = 3 });
            var f0 = new PushBackFrontConfig();
            for (var i = 0; i < front0Levels; i++) f0.HighEndBeamPeraltes.Add(4.5);
            var f1 = new PushBackFrontConfig();
            for (var i = 0; i < front1Levels; i++) f1.HighEndBeamPeraltes.Add(5.5);
            design.Fronts.Add(f0);
            design.Fronts.Add(f1);
            return design;
        }

        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);

        // ---- 2/3/4. Construction + adoption + LoadNew ------------------------------------------------------------

        [Fact]
        public void LoadNew_AdoptsSessionStateAssembler_OneFront_Selection00_Peralte35_TopesActive()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                return w.Session != null && w.State != null && w.Assembler != null
                    && w.State.Structure.Count == 1
                    && w.State.Structure.SelectedFrontIndex == 0 && w.State.Structure.SelectedLevelIndex == 0
                    && Math.Abs(w.State.Cell(0, 0).HighEndBeamPeralte - 3.5) < 1e-6
                    && w.State.Cell(0, 0).RearTopeEnabled
                    && w.HasValidModel && w.LastComputation != null && w.LastComputation.IsValid
                    && w.TopeModel != null && w.TopeModel.Columns == 1 && w.TopeModel.AbsentCount == 0;
            });

            Assert.True(ok);
        }

        // ---- 5. LoadDesignForNew ---------------------------------------------------------------------------------

        [Fact]
        public void LoadDesignForNew_LoadsData_KeepsSource_MintsFreshGuidOnInsert()
        {
            var design = SampleDesign();
            var source = RackProject.ForPushBack(design);

            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadDesignForNew(design, "Plantilla PB", source);
                var idBefore = w.RackId;
                var fronts = w.State.Structure.Count;
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");
                var request = w.InsertionRequest as PushBackInsertionRequest;
                return (fronts, idBefore, w.RackId, w.RackName, w.InsertRequested,
                    request != null && ReferenceEquals(request.SourceProject, source));
            });

            Assert.Equal(2, r.fronts);
            Assert.Null(r.idBefore);                       // no identity until insert
            Assert.True(Guid.TryParse(r.RackId, out _));   // a FRESH GUID was minted (not a library id)
            Assert.Equal("Plantilla PB", r.RackName);
            Assert.True(r.InsertRequested);
            Assert.True(r.Item6);                          // the library source project rode into the payload (I-11)
        }

        // ---- 6. LoadExisting -----------------------------------------------------------------------------------

        [Fact]
        public void LoadExisting_KeepsGuidAndName_EnablesUpdate()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(), "GUID-PB", "PB existente");
                return (w.RackId, w.RackName, Btn(w, "UpdateButton").IsEnabled, w.State.Structure.Count);
            });

            Assert.Equal("GUID-PB", r.RackId);
            Assert.Equal("PB existente", r.RackName);
            Assert.True(r.Item3);          // "Actualizar" enabled for an existing rack
            Assert.Equal(2, r.Item4);
        }

        // ---- 7. Rear peralte differs between cells -------------------------------------------------------------

        [Fact]
        public void RearPeralte_CanDifferBetweenCells_ThroughTheControls()
        {
            var (p00, p01) = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true); // 1 front, 3 levels
                Combo(w, "RearPeralteBox").SelectedItem = 5.0;
                EditorWindowTestSupport.ClickNamed(w, "ApplyCellButton");
                Combo(w, "SelectedLevelBox").SelectedIndex = 1;
                Combo(w, "RearPeralteBox").SelectedItem = 4.0;
                EditorWindowTestSupport.ClickNamed(w, "ApplyCellButton");
                return (w.State.Cell(0, 0).HighEndBeamPeralte, w.State.Cell(0, 1).HighEndBeamPeralte);
            });

            Assert.Equal(5.0, p00, 4);
            Assert.Equal(4.0, p01, 4);
            Assert.NotEqual(p00, p01);
        }

        // ---- 8. Jagged topes matrix: absent cells, single-cell click, all/none --------------------------------

        [Fact]
        public void TopesMatrix_IsJagged_ClickChangesOneCell_AllNoneWork()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(front0Levels: 3, front1Levels: 2), "G", "N");
                var model = w.TopeModel;
                var absent12 = model.IsAbsent(1, 2);   // front 1 has only 2 levels -> (1,2) absent
                var absent02 = model.IsAbsent(0, 2);

                // A single click toggles ONLY that cell and does NOT rebuild the model (shape unchanged).
                var before = w.TopeModel;
                w.TopeModel.Toggle(0, 0);
                var sameModel = ReferenceEquals(before, w.TopeModel);
                var cell00Off = !w.State.Cell(0, 0).RearTopeEnabled;
                var cell01On = w.State.Cell(0, 1).RearTopeEnabled;

                EditorWindowTestSupport.ClickNamed(w, "TopesNoneButton");
                var allOff = !w.State.Cell(0, 0).RearTopeEnabled && !w.State.Cell(0, 1).RearTopeEnabled && !w.State.Cell(1, 0).RearTopeEnabled;
                EditorWindowTestSupport.ClickNamed(w, "TopesAllButton");
                var allOn = w.State.Cell(0, 0).RearTopeEnabled && w.State.Cell(1, 1).RearTopeEnabled;

                return (model.Columns, model.Rows, absent12, absent02, sameModel, cell00Off, cell01On, allOff, allOn);
            });

            Assert.Equal(2, r.Columns);
            Assert.Equal(3, r.Rows);
            Assert.True(r.absent12);
            Assert.False(r.absent02);
            Assert.True(r.sameModel);   // a click never rebuilds the whole matrix
            Assert.True(r.cell00Off);
            Assert.True(r.cell01On);    // only (0,0) changed
            Assert.True(r.allOff);
            Assert.True(r.allOn);
        }

        // ---- 9. Structure changes sync matrix + state ---------------------------------------------------------

        [Fact]
        public void ChangingFrontsAndLevels_SyncsMatrixAndState()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true); // 1 front, 3 levels
                EditorWindowTestSupport.ClickNamed(w, "AddFrontButton");
                var frontsAfterAdd = w.State.Structure.Count;
                var columnsAfterAdd = w.TopeModel.Columns;
                EditorWindowTestSupport.ClickNamed(w, "AddLevelButton");
                var levelsAfterAdd = w.State.Structure.Fronts[w.State.Structure.SelectedFrontIndex].LoadLevels;
                var rowsAfterAdd = w.TopeModel.Rows;
                return (frontsAfterAdd, columnsAfterAdd, levelsAfterAdd, rowsAfterAdd);
            });

            Assert.Equal(2, r.frontsAfterAdd);
            Assert.Equal(2, r.columnsAfterAdd);   // the matrix grew a column with the new front
            Assert.Equal(4, r.levelsAfterAdd);    // 3 -> 4 levels on the selected front
            Assert.Equal(4, r.rowsAfterAdd);      // the matrix grew a row
        }

        // ---- Multi-selection via the visible cell-selection matrix --------------------------------------------

        [Fact]
        public void CellSelection_BuildThree_ApplyToSelection_ChangesExactlyThoseCells()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(front0Levels: 3, front1Levels: 3), "G", "N"); // 2 fronts x 3 levels
                w.CellSelectionModel.SetSelected(0, 2, true); // check (0,2) -> add to the edit selection
                w.CellSelectionModel.SetSelected(1, 1, true); // check (1,1) -> add; (1,1) becomes primary
                var count = w.State.Structure.SelectedCellCount;
                var set = w.State.Structure.IsSelected(0, 0) && w.State.Structure.IsSelected(0, 2) && w.State.Structure.IsSelected(1, 1);
                var primary = w.State.Structure.SelectedFrontIndex == 1 && w.State.Structure.SelectedLevelIndex == 1;

                ((ComboBox)w.FindName("RearPeralteBox")).SelectedItem = 6.0;
                ((System.Windows.Controls.CheckBox)w.FindName("RearTopeActiveCheck")).IsChecked = false;
                EditorWindowTestSupport.SetText(w, "CellPalletFrontBox", "49");
                EditorWindowTestSupport.ClickNamed(w, "ApplySelectedButton");

                bool Changed(int f, int l) => Math.Abs(w.State.Cell(f, l).HighEndBeamPeralte - 6.0) < 1e-6 && !w.State.Cell(f, l).RearTopeEnabled;
                var changed = Changed(0, 0) && Changed(0, 2) && Changed(1, 1);
                var intact = w.State.Cell(0, 1).RearTopeEnabled && Math.Abs(w.State.Cell(0, 1).HighEndBeamPeralte - 6.0) > 1e-6
                             && w.State.Cell(1, 0).RearTopeEnabled && w.State.Cell(1, 2).RearTopeEnabled;
                return (count, set, primary, changed, intact);
            });

            Assert.Equal(3, r.count);
            Assert.True(r.set);
            Assert.True(r.primary);   // the last checked cell is primary
            Assert.True(r.changed);   // exactly the three selected cells changed
            Assert.True(r.intact);    // the others are untouched
        }

        [Fact]
        public void CellSelection_CannotLeaveSelectionEmpty_AndSkipsAbsentCells()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(front0Levels: 3, front1Levels: 2), "G", "N"); // (1,2) is absent
                var absent = w.CellSelectionModel.IsAbsent(1, 2);

                // Only (0,0) is selected after load; unchecking it must be refused (never empty).
                w.CellSelectionModel.SetSelected(0, 0, false);
                var stillSelected = w.State.Structure.IsSelected(0, 0) && w.State.Structure.SelectedCellCount == 1;
                var modelReverted = w.CellSelectionModel.IsSelected(0, 0);
                return (absent, stillSelected, modelReverted);
            });

            Assert.True(r.absent);          // a level that does not exist is an absent cell
            Assert.True(r.stillSelected);   // the last cell cannot be unselected
            Assert.True(r.modelReverted);   // and the visual uncheck was reverted
        }

        [Fact]
        public void SelectingViaFrontLevelCombos_ReplacesSelectionWithASingleCell_AndSyncsTheMatrix()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(front0Levels: 3, front1Levels: 3), "G", "N");
                w.CellSelectionModel.SetSelected(1, 1, true); // multi-selection {(0,0),(1,1)}, primary now front 1
                Assert.Equal(2, w.State.Structure.SelectedCellCount);

                ((ComboBox)w.FindName("SelectedFrontBox")).SelectedIndex = 0; // combo -> single cell back on front 0
                var single = w.State.Structure.SelectedCellCount == 1;
                var matrixSingle = w.CellSelectionModel.SelectedCount == 1;
                return (single, matrixSingle);
            });

            Assert.True(r.Item1);   // the combo replaced the selection with one cell
            Assert.True(r.Item2);   // and the visible matrix synced to a single checked cell
        }

        // ---- 10. Apply by scope --------------------------------------------------------------------------------

        [Fact]
        public void ApplyAll_SetsThePeralteOnEveryCell()
        {
            var all6 = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(front0Levels: 2, front1Levels: 2), "G", "N");
                Combo(w, "RearPeralteBox").SelectedItem = 6.0;
                EditorWindowTestSupport.ClickNamed(w, "ApplyAllButton");
                return Math.Abs(w.State.Cell(0, 0).HighEndBeamPeralte - 6.0) < 1e-6
                    && Math.Abs(w.State.Cell(0, 1).HighEndBeamPeralte - 6.0) < 1e-6
                    && Math.Abs(w.State.Cell(1, 0).HighEndBeamPeralte - 6.0) < 1e-6
                    && Math.Abs(w.State.Cell(1, 1).HighEndBeamPeralte - 6.0) < 1e-6;
            });

            Assert.True(all6);
        }

        // ---- 11. GUIA is never offered nor persisted ----------------------------------------------------------

        [Fact]
        public void Guia_IsNeverOfferedNorPersisted()
        {
            var catalog = RackCad.Application.Catalogs.JsonRackCatalogProvider.FromBaseDirectory().Load();
            var hasGuiaInCatalog = catalog.SafetyElements.Any(e => SelectiveSafetyDefaults.IsType(e.Type, SelectiveSafetyDefaults.GuiaType));

            var (offersGuia, persistsGuia) = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var offers = w.SafetyElementsForDialog().Any(e => SelectiveSafetyDefaults.IsType(e.Type, SelectiveSafetyDefaults.GuiaType));
                var persists = w.LastComputation.System.SafetySelections
                    .Any(s => s.ElementId != null && s.ElementId.IndexOf("GUIA", StringComparison.OrdinalIgnoreCase) >= 0);
                return (offers, persists);
            });

            Assert.True(hasGuiaInCatalog);   // the catalog DOES have guides...
            Assert.False(offersGuia);        // ...but the dialog never offers them...
            Assert.False(persistsGuia);      // ...and none reaches the resolved system.
        }

        // ---- 12/13. Valid vs invalid recompute ----------------------------------------------------------------

        [Fact]
        public void ValidRecompute_SetsSessionModel_AndAcceptsTheBaseline()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                return (w.HasValidModel, w.Session.Design != null, w.Session.System != null, w.State.WorkingBaseline != null);
            });

            Assert.True(r.HasValidModel);
            Assert.True(r.Item2);   // session received the design
            Assert.True(r.Item3);   // session received the system
            Assert.True(r.Item4);   // AcceptComputation advanced the baseline
        }

        [Fact]
        public void InvalidField_KeepsLastModel_DisablesActions_BlocksInsert_ThenCorrectionReEnables()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(), "GUID-VAL", "PB"); // existing rack -> Update is also meaningfully gated
                var validModel = w.LastComputation;
                var validSystem = w.Session.System;
                var startedValid = w.CurrentInputsAreValid;

                // A zero pallet depth is invalid (> 0). Type it and tab out -> the recompute is blocked.
                TypeAndCommit(w, "DepthBox", "0");

                var nowInvalid = !w.CurrentInputsAreValid;
                var keptModel = ReferenceEquals(validModel, w.LastComputation);   // last valid model NOT replaced
                var keptSystem = ReferenceEquals(validSystem, w.Session.System);
                var actionsDisabled = !Btn(w, "InsertButton").IsEnabled && !Btn(w, "UpdateButton").IsEnabled
                                      && !Btn(w, "BomButton").IsEnabled && !Btn(w, "SaveLibraryButton").IsEnabled;
                var libraryNull = w.BuildLibraryProjectForTest() == null; // null while the current inputs are invalid

                EditorWindowTestSupport.ClickNamed(w, "InsertButton"); // attempt to insert -> no request produced
                var noRequest = !w.InsertRequested && w.InsertionRequest == null;

                // Correct the value -> re-validates and a NEW model is used.
                TypeAndCommit(w, "DepthBox", "48");
                var reValid = w.CurrentInputsAreValid;
                var newModelUsed = w.LastComputation != null && w.LastComputation.IsValid && !ReferenceEquals(validModel, w.LastComputation);
                var actionsReEnabled = Btn(w, "InsertButton").IsEnabled && Btn(w, "UpdateButton").IsEnabled
                                       && Btn(w, "BomButton").IsEnabled && Btn(w, "SaveLibraryButton").IsEnabled;

                return (startedValid, nowInvalid, keptModel, keptSystem, actionsDisabled, libraryNull, noRequest, reValid, newModelUsed, actionsReEnabled);
            });

            Assert.True(r.startedValid);
            Assert.True(r.nowInvalid);
            Assert.True(r.keptModel);
            Assert.True(r.keptSystem);
            Assert.True(r.actionsDisabled);
            Assert.True(r.libraryNull);
            Assert.True(r.noRequest);
            Assert.True(r.reValid);
            Assert.True(r.newModelUsed);
            Assert.True(r.actionsReEnabled);
        }

        [Fact]
        public void NumericFields_LocalizedParsing_Range_AndOptional()
        {
            StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var depth = (NumericField)w.FindName("DepthBox");                    // must be > 0
                var overrideBox = (NumericField)w.FindName("CellBeamLengthOverrideBox"); // optional, > 0

                depth.Text = "48.5";
                Assert.False(depth.HasError);
                Assert.Equal(48.5, depth.Value.Value, 4);   // decimal with a point

                depth.Text = "48,5";
                Assert.False(depth.HasError);
                Assert.Equal(48.5, depth.Value.Value, 4);   // decimal with a comma (localized, no grouping)

                depth.Text = "abc";
                Assert.True(depth.HasError);
                Assert.Null(depth.Value);                   // not a number

                depth.Text = "0";
                Assert.True(depth.HasError);                // out of range (must be > 0)

                depth.Text = "48";
                Assert.False(depth.HasError);               // corrected

                overrideBox.Text = string.Empty;
                Assert.False(overrideBox.HasError);
                Assert.Null(overrideBox.Value);             // optional blank = auto (no override)

                overrideBox.Text = "-1";
                Assert.True(overrideBox.HasError);          // optional, but still must be > 0
            });
        }

        private static void TypeAndCommit(RackPushBackSystemWindow window, string name, string text)
        {
            var field = (NumericField)window.FindName(name);
            field.Text = text;
            field.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, field)); // "typed + tabbed out" -> recompute
        }

        // ---- Lateral preview follows the chosen corte ---------------------------------------------------------

        [Fact]
        public void LateralPreview_UsesTheSelectedCorte_AndInsertSectionMatches()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadDesignForNew(SampleDesign(), "PB", null); // a multi-front system: distinct lateral cortes
                ((ComboBox)w.FindName("ViewBox")).SelectedIndex = 0; // lateral
                var cortes = w.LastComputation.LateralCortes.Count;

                ((ComboBox)w.FindName("LateralSectionBox")).SelectedIndex = 0;
                var sig1 = PlanSignature(w.CurrentPreviewPlan);
                ((ComboBox)w.FindName("LateralSectionBox")).SelectedIndex = 1;
                var sig2 = PlanSignature(w.CurrentPreviewPlan);

                EditorWindowTestSupport.ClickNamed(w, "InsertButton"); // insert the SHOWN corte (index 1)
                return (cortes, sig1, sig2, w.InsertView, w.InsertSection);
            });

            Assert.True(r.cortes >= 2);
            Assert.NotEqual(r.sig1, r.sig2);                          // the preview plan changed with the corte
            Assert.Equal(RackEmbedDocument.ViewLateral, r.InsertView);
            Assert.Equal(1, r.InsertSection);                        // InsertSection matches the shown corte
        }

        private static string PlanSignature(DynamicSystemPlan plan)
        {
            if (plan == null) return "null";
            var instances = plan.Headers.SelectMany(g => g.Instances).Concat(plan.LooseInstances);
            return string.Join("|", instances
                .Select(i => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}@{1:F2},{2:F2}", i.PieceId, i.Insertion.X, i.Insertion.Y))
                .OrderBy(s => s, StringComparer.Ordinal));
        }

        // ---- 14. View + section selector ----------------------------------------------------------------------

        [Fact]
        public void ViewSelector_MapsEachViewToTheEmbedContract()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var lateralSections = Combo(w, "LateralSectionBox").Items.Count;

                Combo(w, "ViewBox").SelectedIndex = 1; // Frontal entrada/salida
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");
                var es = (w.InsertView, w.InsertSection);
                return (lateralSections, es.InsertView, es.InsertSection);
            });

            Assert.True(r.lateralSections >= 1);                 // lateral has at least one corte
            Assert.Equal(RackEmbedDocument.ViewFrontal, r.InsertView);
            Assert.Equal((int)PushBackFrontalEnd.EntradaSalida, r.InsertSection);
        }

        // ---- 15. RequestInsert (lateral) ----------------------------------------------------------------------

        [Fact]
        public void Insert_Lateral_ProducesTheTypedPayload()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadDesignForNew(SampleDesign(), "PB nueva", null);
                Combo(w, "ViewBox").SelectedIndex = 0; // Lateral
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");
                var request = w.InsertionRequest as PushBackInsertionRequest;
                return (w.InsertRequested, w.UpdateOnly, w.InsertView, request?.Kind, request?.System != null, request?.Design != null);
            });

            Assert.True(r.InsertRequested);
            Assert.False(r.UpdateOnly);
            Assert.Equal(RackEmbedDocument.ViewLateral, r.InsertView);
            Assert.Equal(RackSystemKind.PushBack, r.Item4);
            Assert.True(r.Item5);
            Assert.True(r.Item6);
        }

        // ---- 16. RequestUpdate --------------------------------------------------------------------------------

        [Fact]
        public void Update_KeepsGuidAndSource_RedrawsInPlace()
        {
            var design = SampleDesign();
            var source = RackProject.ForPushBack(design);

            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(design, "GUID-UPD", "PB", source);
                EditorWindowTestSupport.ClickNamed(w, "UpdateButton");
                var request = w.InsertionRequest as PushBackInsertionRequest;
                return (w.InsertRequested, w.UpdateOnly, w.InsertView, w.InsertSection, w.RackId,
                    request != null && ReferenceEquals(request.SourceProject, source));
            });

            Assert.True(r.InsertRequested);
            Assert.True(r.UpdateOnly);
            Assert.Null(r.InsertView);
            Assert.Equal(-1, r.InsertSection);
            Assert.Equal("GUID-UPD", r.RackId);
            Assert.True(r.Item6);
        }

        // ---- 17. An additional view while editing an existing rack --------------------------------------------

        [Fact]
        public void ExistingRack_CanRequestAnAdditionalView_KeepingTheGuid()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(), "GUID-ADD", "PB");
                Combo(w, "ViewBox").SelectedIndex = 2; // Frontal posterior
                EditorWindowTestSupport.ClickNamed(w, "InsertButton");
                return (w.InsertRequested, w.UpdateOnly, w.InsertView, w.InsertSection, w.RackId);
            });

            Assert.True(r.InsertRequested);
            Assert.False(r.UpdateOnly);      // a normal insert, not an update
            Assert.Equal(RackEmbedDocument.ViewFrontal, r.InsertView);
            Assert.Equal((int)PushBackFrontalEnd.Posterior, r.InsertSection);
            Assert.Equal("GUID-ADD", r.RackId); // the existing GUID is preserved
        }

        // ---- 18. Library payload preserves metadata and does not flag an insert -------------------------------

        [Fact]
        public void LibraryProject_IsThePushBackPayload_PreservesMetadata_AndDoesNotInsert()
        {
            var store = new RackProjectStore();
            // A source project a newer build wrote, carrying an unknown wrapper field the current build must preserve (I-11).
            var node = System.Text.Json.Nodes.JsonNode.Parse(store.Serialize(RackProject.ForPushBack(SampleDesign()))).AsObject();
            node["futureField"] = "keep-me";
            var source = store.Deserialize(node.ToJsonString());

            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(), "GUID-LIB", "PB", source);
                var project = w.BuildLibraryProjectForTest();
                return (project?.Kind, project?.PushBackDesign != null, w.InsertRequested, store.Serialize(project));
            });

            Assert.Equal(RackSystemKind.PushBack, r.Item1);
            Assert.True(r.Item2);                     // the active Push Back payload
            Assert.False(r.InsertRequested);          // saving never flags an insert
            Assert.Contains("futureField", r.Item4);  // I-11 source metadata preserved through the library project
            Assert.Contains("keep-me", r.Item4);
        }

        // ---- 19. No AutoCAD types --------------------------------------------------------------------------

        [Fact]
        public void RackCadUi_ReferencesNoAutoCadAssembly()
        {
            var referenced = typeof(RackPushBackSystemWindow).Assembly.GetReferencedAssemblies();
            Assert.DoesNotContain(referenced, a =>
                a.Name.IndexOf("Autodesk", StringComparison.OrdinalIgnoreCase) >= 0
                || a.Name.StartsWith("AcMgd", StringComparison.OrdinalIgnoreCase)
                || a.Name.StartsWith("AcDbMgd", StringComparison.OrdinalIgnoreCase)
                || a.Name.StartsWith("AcCoreMgd", StringComparison.OrdinalIgnoreCase));
        }

        // ---- 20. Existing editor windows still construct (regression) ----------------------------------------

        [Fact]
        public void ExistingEditorWindows_StillConstructAlongsidePushBack()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var dynamicWindow = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var pushBack = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                return dynamicWindow.Session != null && pushBack.Session != null && pushBack.HasValidModel;
            });

            Assert.True(ok);
        }
    }
}
