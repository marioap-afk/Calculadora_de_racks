#nullable enable
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Controls;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D (Selectivo E1-A): the draft of the RackCad window recomposes like the classic editor. For the same inputs (values
    /// with at most three decimals) the persisted design of the draft is the classic window's, byte for byte: a new rack, a rack opened from
    /// the drawing the way RACKEDITAR opens it (and as the door reads it), the same edits of cells and fronts, two fondos, a half front,
    /// custom cabeceras, own peraltes per post and a vertical clearance bound to a project variable, whose binding the write keeps. The
    /// differences are declared and fixed here too: the full precision of a value, and the post that is not in the catalog (or is in it
    /// with other capitals).
    /// </summary>
    public class SelectiveDraftEditorEquivalenceTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private const string RackId = "GUID-SEL-VENTANA";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private static readonly PropertyId Clearance = ProjectPropertyIds.SelectiveVerticalClearance;
        private static readonly SelectivePalletDesignStore Store = new SelectivePalletDesignStore();

        private static string Json(SelectivePalletDesign? design)
            => Store.Serialize(SelectivePalletDesignDocument.From(design!, RackId, "Selectivo A"));

        private static SelectiveMatrixPosition P(int front, int level) => new SelectiveMatrixPosition(front, level);

        private static SelectiveCell Cell(double frente = 42.0, double alto = 60.0, int count = 2)
            => new SelectiveCell { Pallet = new Tarima { Frente = frente, Alto = alto }, PalletCount = count, BeamId = BeamId, BeamPeralte = 4.0 };

        private static SelectiveBayDesign Bay(int levels, bool floor = false, double frente = 42.0, double alto = 60.0)
        {
            var bay = new SelectiveBayDesign { FloorBeam = floor, FloorBeamRiseOverride = 4.0 };
            for (var level = 0; level < levels; level++) bay.Levels.Add(Cell(frente, alto));
            return bay;
        }

        /// <summary>A rack with fronts of different levels, a floor beam, an own peralte per post and drawing options.</summary>
        private static SelectivePalletDesign Structure()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, FloorBeamRise = 4.0,
                PalletDepth = 48.0, DepthCount = 1, DrawBasePlate = true, NumberFronts = true, DrawPallets = true,
                AnnotationScale = 1.5, Dimensions = DimensionDetail.Standard, DimensionViews = DimensionViewVisibility.Frontal | DimensionViewVisibility.Planta,
                DimensionStyle = "ESTILO_RACKCAD",
            };
            design.Bays.Add(Bay(3, floor: true, frente: 40.0));
            design.Bays.Add(Bay(2, frente: 44.0, alto: 55.0));
            foreach (var peralte in new[] { 5.0, 0.0, 0.0 }) design.PostPeraltes.Add(peralte);
            design.CabeceraFondoOverrides.Add(0.0);
            return design;
        }

        private static ProjectVariablesReadResult Register(double value)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = VarId,
                    Name = "Holgura estándar",
                    Type = VariableType.Length.ToString(),
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
                },
            };
            return ProjectVariablesReadResult.Readable(document);
        }

        private static string Authored(SelectivePalletDesign design, bool bound = false)
        {
            var document = SelectivePalletDesignDocument.From(design, RackId, "Selectivo A");
            if (bound)
            {
                document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [ProjectPropertyIds.SelectiveVerticalClearanceToken] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
                };
                document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return Store.Serialize(document);
        }

        /// <summary>
        /// The classic window opened as RACKEDITAR opens it (the register, <c>SelectiveEditorOpen.Resolve</c>, the accredited options and
        /// <c>LoadExisting</c>), and the draft opened as the door reads the same rack (<see cref="RackDoorRules.AnswerForSelectiveOpen"/>).
        /// </summary>
        private static (RackSelectiveWindow Window, SelectiveDraftEditor Draft) OpenBoth(string authoredJson, ProjectVariablesReadResult register)
        {
            var open = SelectiveEditorOpen.Resolve(Store.Deserialize(authoredJson), register);
            Assert.True(open.IsOpen, open.Error);
            var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
            window.SetProjectVariables(LinkedPropertyOptions.ForProperty(Clearance, register));
            window.LoadExisting(Store.Deserialize(authoredJson), open.Design, open.LinkedPropertyStates);

            var read = RackDoorRules.AnswerForSelectiveOpen(1, RackId, RackEmbedDocument.KindSelective, authoredJson, register);
            Assert.True(read.Opens, read.Reason);
            var draft = new SelectiveDraftEditor(window.Session.Catalog);
            Assert.True(draft.LoadFrom(read.EffectiveDesign()!, read.LinkedPropertyStates, read.Name, read.LinkedVariableNames), draft.LastError);
            return (window, draft);
        }

        private static IEnumerable<string> States(IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> states)
            => states.OrderBy(pair => pair.Key.Value, System.StringComparer.Ordinal).Select(pair => pair.Key + ":" + pair.Value);

        private static void Type(Window window, string name, string text)
        {
            EditorWindowTestSupport.SetText(window, name, text);
            SelectiveWindowTestSupport.RaiseLostFocus(window, name);
        }

        private static RackFrameConfiguration Custom(RackSelectiveWindow window, double height)
            => new RackFrameConfigurationFactory(window.Session.Catalog).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, height, 42.0);

        // ------------------------------------------------------------------ the same design

        [Fact]
        public void ANewRackIsTheDesignOfTheClassicEditor()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                try
                {
                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);

                    var draft = SelectiveDraftEditor.CreateNew(window.Session.Catalog);

                    Assert.Null(draft.LastError);
                    Assert.Equal(Json(classic), Json(draft.Design));
                    Assert.Equal(States(window.LinkedPropertyFinalStates), States(draft.LinkedPropertyFinalStates));
                    Assert.Equal(1, draft.FondoCount);
                    Assert.Null(draft.FondoScope); // the window starts in «Todos los fondos» (decision 2)
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void ARackOpenedAsRackEditarOpensItIsTheDesignOfTheClassicEditor()
        {
            StaTestRunner.Run(() =>
            {
                var (window, draft) = OpenBoth(Authored(Structure()), ProjectVariablesReadResult.Absent());
                try
                {
                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);

                    Assert.Equal(Json(classic), Json(draft.Design));
                    Assert.Equal(States(window.LinkedPropertyFinalStates), States(draft.LinkedPropertyFinalStates));
                    Assert.Equal("Selectivo A", draft.Name);
                    Assert.Null(draft.LoadNotice);
                    Assert.Equal(new[] { 3, 2 }, new[] { draft.LevelCount(0, 0), draft.LevelCount(0, 1) });
                    Assert.Equal(5.0, draft.PostPeralteOverride(0));
                    Assert.Null(draft.PostPeralteOverride(1));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void TheSameEditsOfCellsFrontsAndTheRackGiveTheSameDesignAsTheClassicEditor()
        {
            StaTestRunner.Run(() =>
            {
                var (window, draft) = OpenBoth(Authored(Structure()), ProjectVariablesReadResult.Absent());
                try
                {
                    // The classic editor through the same operations of its state that its handlers call (its targets are «Todos»), and
                    // through its boxes for the rack-wide values.
                    var state = window.EditorState;
                    state.CellAt(new SelectiveCellAddress(0, 0, 1)).Alto = 72.0;
                    state.CellAt(new SelectiveCellAddress(0, 1, 0)).PalletCount = 3;
                    state.CellAt(new SelectiveCellAddress(0, 1, 1)).PalletCount = 3;
                    state.CellAt(new SelectiveCellAddress(0, 0, 0)).BeamLength = 100.0;
                    state.CellAt(new SelectiveCellAddress(0, 1, 1)).Clear = 50.0;
                    state.ApplyFloorBeamToTargets(SelectiveFrontApplyScope.Front, 1, true);
                    state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 0, 6.0);
                    state.ApplyLevelCountToTargets(SelectiveFrontApplyScope.Front, 1, 4);
                    state.ApplyBayHeightToTargets(SelectiveFrontApplyScope.Front, 0, 180.0);
                    // The cell editor shows the selected cell again, or the gesture below would copy its old fields into that cell.
                    window.LoadCellEditorForTest();
                    Type(window, "BayCountBox", "3");
                    EditorWindowTestSupport.SetText(window, "PostPeralteBox", "3.5");
                    var tolerance = EditorWindowTestSupport.Find<LinkedPropertyEditor>(window, e => e.Name == "ToleranceEditor");
                    tolerance.Box.Text = "3.5";
                    tolerance.CommitByEnterForTest();
                    ((CheckBox)window.FindName("NumberLevelsCheck")).IsChecked = true;
                    ((ComboBox)window.FindName("DimensionsBox")).SelectedIndex = 3;
                    ((CheckBox)window.FindName("DimensionsPlantaCheck")).IsChecked = false;

                    Assert.Equal(1, draft.SetPalletHeight(new[] { P(0, 1) }, 72.0).Written);
                    Assert.Equal(2, draft.SetPalletCount(new[] { P(1, 0), P(1, 1) }, 3).Written);
                    draft.SetBeamLengthOverride(new[] { P(0, 0) }, 100.0);
                    draft.SetClearOverride(new[] { P(1, 1) }, 50.0);
                    draft.SetFloorBeam(new[] { 1 }, true);
                    draft.SetFloorBeamRise(new[] { 0 }, 6.0);
                    draft.SetLevelCount(new[] { 1 }, 4);
                    draft.SetHeightOverride(new[] { 0 }, 180.0);
                    draft.SetFrontCount(3);
                    draft.PostPeralte = 3.5;
                    draft.PalletTolerance = 3.5;
                    draft.NumberLevels = true;
                    draft.Dimensions = DimensionDetail.Detailed;
                    // The boxes as the classic editor shows the loaded policy (frontal and planta), with planta turned off.
                    Assert.Equal((true, false, true), (draft.ShowsDimensions(DimensionViewKind.Frontal), draft.ShowsDimensions(DimensionViewKind.Lateral),
                        draft.ShowsDimensions(DimensionViewKind.Planta)));
                    draft.SetDimensionViews(frontal: true, lateral: false, planta: false);

                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);
                    Assert.True(draft.Recompose(), draft.LastError);

                    Assert.Equal(Json(classic), Json(draft.Design));
                    Assert.Equal(States(window.LinkedPropertyFinalStates), States(draft.LinkedPropertyFinalStates));
                    Assert.Equal(3, draft.Design!.Bays.Count);
                    Assert.Equal(180.0, draft.Front(0, 0)!.HeightOverride);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void TwoFondosWithTheirOwnDepthLevelsAndSeparationAreTheDesignOfTheClassicEditor()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(fondos: 2);
                try
                {
                    // Fondo 2 only: its depth, the levels of its first front and the separation; then all the fondos: the cabecera depth.
                    window.EditorState.SetTargetFondos(new[] { 1 });
                    Type(window, "FondoBox", "40");
                    window.EditorState.ApplyLevelCountToTargets(SelectiveFrontApplyScope.Front, 0, 3);
                    var separator = ((Panel)window.FindName("SeparatorsHost")).Children.OfType<StackPanel>().Single().Children.OfType<TextBox>().Single();
                    separator.Text = "10";
                    window.EditorState.FollowAllFondos();
                    Type(window, "CabeceraFondoBox", "38");

                    var draft = SelectiveDraftEditor.CreateNew(window.Session.Catalog);
                    draft.SetFondoCount(2);
                    Assert.Equal(new[] { 12.0 }, draft.Separators);
                    draft.SetFondoScope(1);
                    Assert.Equal(1, draft.SetPalletDepth(40.0).Written);
                    draft.SetLevelCount(new[] { 0 }, 3);
                    draft.SetSeparator(0, 10.0);
                    draft.SetFondoScope(null);
                    Assert.Equal(2, draft.SetCabeceraDepth(38.0).Written);

                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);
                    Assert.True(draft.Recompose(), draft.LastError);
                    Assert.Equal(Json(classic), Json(draft.Design));

                    // What the window reads over «Todos»: the depth and the levels vary; over fondo 2 they do not.
                    Assert.True(draft.ReadFondos(fondo => fondo.PalletDepth).Varies);
                    Assert.Equal(38.0, draft.ReadFondos(fondo => fondo.CabeceraDepthOverride).Value);
                    Assert.True(draft.ReadFronts(new[] { 0 }, front => front.LevelCount).Varies);
                    draft.SetFondoScope(1);
                    Assert.Equal(3, draft.ReadFronts(new[] { 0 }, front => front.LevelCount).Value);
                    Assert.Equal(40.0, draft.ReadFondos(fondo => fondo.PalletDepth).Value);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void AFrontThatOnlyOneFondoHasIsWrittenThereAndReportedMissingInTheOther()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(fondos: 2);
                try
                {
                    window.EditorState.SetTargetFondos(new[] { 1 });
                    Type(window, "BayCountBox", "3");
                    window.EditorState.FollowAllFondos();
                    window.EditorState.ApplyFloorBeamToTargets(SelectiveFrontApplyScope.Front, 2, true);
                    window.EditorState.CellAt(new SelectiveCellAddress(1, 2, 0)).Alto = 50.0;

                    var draft = SelectiveDraftEditor.CreateNew(window.Session.Catalog);
                    draft.SetFondoCount(2);
                    draft.SetFondoScope(1);
                    draft.SetFrontCount(3);
                    draft.SetFondoScope(null);
                    var floor = draft.SetFloorBeam(new[] { 2 }, true);
                    var alto = draft.SetPalletHeight(new[] { P(2, 0) }, 50.0);

                    Assert.Equal((1, 1), (floor.Written, floor.Omitted));
                    Assert.Equal((1, 1), (alto.Written, alto.Omitted));
                    Assert.Equal("Parte de lo elegido no existe en todos los fondos: se escribió solo donde existe.", alto.Notice);
                    Assert.Equal(1, draft.ReadCells(new[] { P(2, 0) }, cell => cell.Alto).Missing);
                    Assert.Equal(3, draft.ScopeFrontCount);

                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);
                    Assert.True(draft.Recompose(), draft.LastError);
                    Assert.Equal(Json(classic), Json(draft.Design));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void AHalfFrontIsTheTramosOfTheClassicEditorWithTheChecksOfItsDialog()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                try
                {
                    var tramos = new[] { new SelectiveSegment { Length = 30.0, Loaded = true }, new SelectiveSegment { Length = 0.0, Loaded = false } };
                    window.EditorState.ApplySegmentsToTargets(0, tramos);

                    var draft = SelectiveDraftEditor.CreateNew(window.Session.Catalog);
                    Assert.Equal("El tramo 1 necesita una longitud mayor que 0 (el último se calcula solo).",
                        draft.SetSegments(0, new[] { new SelectiveSegment { Length = 0.0, Loaded = true }, new SelectiveSegment() }).Error);
                    Assert.StartsWith("Los tramos (500 in) no pueden igualar ni superar el frente completo",
                        draft.SetSegments(0, new[] { new SelectiveSegment { Length = 500.0, Loaded = true }, new SelectiveSegment() }).Error);
                    Assert.Equal(1, draft.SetSegments(0, tramos).Written);
                    Assert.True(draft.Front(0, 0)!.IsSplit);

                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);
                    Assert.True(draft.Recompose(), draft.LastError);
                    Assert.Equal(Json(classic), Json(draft.Design));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void CustomCabecerasAreOpenedKeptAndFollowTheDepthOfTheirFondoAsInTheClassicEditor()
        {
            StaTestRunner.Run(() =>
            {
                var probe = SelectiveWindowTestSupport.Open();
                var design = Structure();
                design.DepthCount = 2;
                design.SeparatorLengths.Add(8.0);
                design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2, floor: true), Bay(2, floor: true) });
                design.ExtraFondoDepths.Add(44.0);
                design.CabeceraFondoOverrides.Add(0.0);
                foreach (var cabecera in new[] { null, Custom(probe, 120.0), null }) design.PostCabeceras.Add(cabecera);
                design.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration> { Custom(probe, 96.0) });
                probe.Close();

                var (window, draft) = OpenBoth(Authored(design), ProjectVariablesReadResult.Absent());
                try
                {
                    Assert.True(draft.HasCustomCabecera(0, 1));
                    Assert.True(draft.HasCustomCabecera(1, 0));
                    Assert.False(draft.HasCustomCabecera(0, 0));
                    Assert.Equal(Json(window.BuildDesignForTest(out _)), Json(draft.Design));

                    // A new depth of every fondo: their custom cabeceras take it, in both editors.
                    Type(window, "FondoBox", "50");
                    var published = Json(draft.Design);
                    Assert.Equal(2, draft.SetPalletDepth(50.0).Written);
                    Assert.Equal(published, Json(draft.Design)); // the published design is a copy: the depth reaches it only by recomposing

                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);
                    Assert.True(draft.Recompose(), draft.LastError);
                    Assert.Equal(Json(classic), Json(draft.Design));
                    Assert.Equal(44.0, draft.Design!.PostCabeceras[1]!.Depth);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void OwnPeraltesPerPostAreThoseOfTheClassicEditorWithItsRuleForTheRackPeralte()
        {
            StaTestRunner.Run(() =>
            {
                var (window, draft) = OpenBoth(Authored(Structure()), ProjectVariablesReadResult.Absent());
                try
                {
                    window.EditorState.PostPeraltes[2] = 4.5;
                    window.EditorState.PostPeraltes[0] = 0.0;

                    Assert.Equal(1, draft.SetPostPeralte(2, 4.5).Written);
                    draft.SetPostPeralte(0, null);
                    Assert.Equal("Peralte de poste inválido (deja vacío para usar el peralte del rack).", draft.SetPostPeralte(1, -1.0).Error);
                    Assert.Equal("Ese poste no existe.", draft.SetPostPeralte(3, 4.0).Error);

                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);
                    Assert.True(draft.Recompose(), draft.LastError);
                    Assert.Equal(Json(classic), Json(draft.Design));

                    // The rule of the classic box: a value equal to the peralte of the rack is stored as «takes the peralte of the rack».
                    draft.SetPostPeralte(1, draft.PostPeralte);
                    Assert.Null(draft.PostPeralteOverride(1));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void ABoundVerticalClearanceOpensWithItsVariableIsReadOnlyAndTheWriteKeepsItsBinding()
        {
            StaTestRunner.Run(() =>
            {
                var authored = Authored(Structure(), bound: true);
                var register = Register(10.0);
                var (window, draft) = OpenBoth(authored, register);
                try
                {
                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);
                    Assert.Equal(Json(classic), Json(draft.Design));
                    Assert.Equal(10.0, draft.Design!.VerticalClearance);
                    Assert.Equal(States(window.LinkedPropertyFinalStates), States(draft.LinkedPropertyFinalStates));
                    Assert.True(draft.VerticalClearanceIsLinked);
                    Assert.False(draft.PalletToleranceIsLinked);
                    Assert.Equal("La gobierna la variable de proyecto «Holgura estándar»: en esta ventana es de solo lectura.",
                        draft.LinkedReadOnlyReason(Clearance));

                    // The document written: the reconciler over the same authored document gives the classic one, binding and frozen 6 kept.
                    var snapshot = draft.SnapshotForWrite()!;
                    var fromClassic = LinkedPropertyReconciler.Reconcile(Store.Deserialize(authored), classic!, window.LinkedPropertyFinalStates, register, RackId, "Selectivo A");
                    var fromDraft = LinkedPropertyReconciler.Reconcile(Store.Deserialize(authored), snapshot.Design, snapshot.LinkedPropertyFinalStates, register, RackId, "Selectivo A");
                    Assert.True(fromDraft.IsSuccess, fromDraft.Error);
                    Assert.Equal(Store.Serialize(fromClassic.Authored), Store.Serialize(fromDraft.Authored));
                    Assert.True(fromDraft.Authored.TryGetBinding(Clearance, out _));
                    Assert.Equal(6.0, fromDraft.Authored.VerticalClearance);

                    // The window offers no «=Variable» (decision 12): a change of the linked value is refused and the last design stays.
                    var valid = Json(draft.Design);
                    draft.VerticalClearance = 7.0;
                    Assert.False(draft.Recompose());
                    Assert.Equal("Holgura vertical: La gobierna la variable de proyecto «Holgura estándar»: en esta ventana es de solo lectura.", draft.LastError);
                    Assert.Equal(valid, Json(draft.Design));
                    Assert.True(draft.RestoreLastValid());
                    Assert.Equal(10.0, draft.VerticalClearance);
                    Assert.Null(draft.LastError);
                }
                finally { window.Close(); }
            });
        }

        // ------------------------------------------------------------------ the declared differences

        [Fact]
        public void TheDraftKeepsTheFullPrecisionWhereTheClassicEditorRoundsToThreeDecimals()
        {
            // Declared difference (1): the classic editor writes the post peralte and the separations back through its boxes («0.###»).
            StaTestRunner.Run(() =>
            {
                var design = Structure();
                design.PostPeralte = 3.14159;
                design.DepthCount = 2;
                design.SeparatorLengths.Add(10.12345);
                var (window, draft) = OpenBoth(Authored(design), ProjectVariablesReadResult.Absent());
                try
                {
                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);

                    Assert.Equal(3.142, classic!.PostPeralte, 9);
                    Assert.Equal(10.123, classic.SeparatorLengths[0], 9);
                    Assert.Equal(3.14159, draft.Design!.PostPeralte, 9);
                    Assert.Equal(10.12345, draft.Design.SeparatorLengths[0], 9);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void APostThatTheCatalogDoesNotHaveGivesWayToTheFirstOfTheListAndSaysSoAndAnotherSpellingIsThatPost()
        {
            // Declared difference (2): the classic list falls back silently. A catalog post in another spelling is that post in the draft;
            // with the catalog of today (one post) the classic list ends on the same post, so the design is the same.
            StaTestRunner.Run(() =>
            {
                var unknown = Structure();
                unknown.PostId = "POSTE_X";
                var (window, draft) = OpenBoth(Authored(unknown), ProjectVariablesReadResult.Absent());
                try
                {
                    var first = draft.PostChoices[0].Id.Trim();
                    Assert.Equal(first, window.BuildDesignForTest(out _)!.PostId);
                    Assert.Equal(first, draft.Design!.PostId);
                    Assert.Equal("El poste «POSTE_X» no está en el catálogo: se usa «" + first + "», como en el editor clásico.", draft.LoadNotice);
                }
                finally { window.Close(); }

                var known = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe()).PostChoices.Select(p => p.Id.Trim()).Last();
                var spelled = Structure();
                spelled.PostId = known.ToLowerInvariant() == known ? known.ToUpperInvariant() : known.ToLowerInvariant();
                var (window2, draft2) = OpenBoth(Authored(spelled), ProjectVariablesReadResult.Absent());
                try
                {
                    Assert.Equal(known, draft2.Design!.PostId);
                    Assert.Null(draft2.LoadNotice);
                    Assert.Equal(Json(window2.BuildDesignForTest(out _)), Json(draft2.Design));
                }
                finally { window2.Close(); }
            });
        }

        // ------------------------------------------------------------------ the copies and the last valid design

        [Fact]
        public void TheSnapshotForAutoCadIsACopyThatLaterEditsDoNotReach()
        {
            var draft = SelectiveDraftEditor.CreateNew(UiSupport.LoadCatalogSafe());
            draft.Name = "  Selectivo A ";
            Assert.True(draft.Recompose(), draft.LastError);

            var snapshot = draft.SnapshotForWrite()!;

            Assert.NotSame(draft.Design, snapshot.Design);
            Assert.NotSame(draft.System, snapshot.System);
            Assert.Equal(Store.Serialize(SelectivePalletDesignDocument.From(draft.Design!, null, null)), snapshot.DesignJson);
            Assert.Equal(Json(draft.Design), Json(snapshot.Design));
            Assert.Equal("Selectivo A", snapshot.Name);
            Assert.Equal("Selectivo A", snapshot.System.Name);
            Assert.Equal(draft.System!.Height, snapshot.System.Height);
            Assert.Equal(States(draft.LinkedPropertyFinalStates), States(snapshot.LinkedPropertyFinalStates));

            draft.SetPalletHeight(new[] { P(0, 0) }, 80.0);
            Assert.True(draft.Recompose(), draft.LastError);

            Assert.Equal(60.0, snapshot.Design.Bays[0].Levels[0].Pallet.Alto);
            Assert.Equal(80.0, draft.Design!.Bays[0].Levels[0].Pallet.Alto);
        }

        [Fact]
        public void AnInvalidInputIsRefusedWithTheMessageOfTheClassicEditorAndTheLastValidDesignComesBack()
        {
            var draft = SelectiveDraftEditor.CreateNew(UiSupport.LoadCatalogSafe());
            var valid = Json(draft.Design);
            var system = draft.System;

            draft.PostPeralte = 0.0;
            Assert.False(draft.Recompose());
            Assert.Equal("Peralte de poste inválido.", draft.LastError);
            Assert.Equal(valid, Json(draft.Design));
            Assert.Same(system, draft.System);

            Assert.True(draft.RestoreLastValid());
            Assert.Null(draft.LastError);
            Assert.Equal(3.0, draft.PostPeralte);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(valid, Json(draft.Design));

            Assert.Equal("Cantidad de frentes inválida (mínimo 1).", draft.SetFrontCount(0).Error);
            Assert.Equal("Frente de tarima inválido.", draft.SetPalletFront(new[] { P(0, 0) }, 0.0).Error);
            Assert.Equal("Número de fondos inválido (mínimo 1).", draft.SetFondoCount(0).Error);
        }

        [Fact]
        public void ThePeraltesOfALargueroAreThoseOfTheCatalog()
        {
            var draft = SelectiveDraftEditor.CreateNew(UiSupport.LoadCatalogSafe());
            var beam = draft.Cell(0, 0, 0)!.BeamId;
            var allowed = draft.BeamPeraltes(beam);
            Assert.NotEmpty(allowed);

            var refused = draft.SetBeamPeralte(new[] { P(0, 0) }, 99.0);
            Assert.True(refused.Refused);
            Assert.Equal("El larguero «" + beam + "» no admite un peralte de 99 in en el catálogo.", refused.Error);
            Assert.Equal(1, draft.SetBeamPeralte(new[] { P(0, 0) }, allowed.Last()).Written);
            Assert.Equal(allowed.Last(), draft.Cell(0, 0, 0)!.BeamPeralte);
            Assert.Equal("El larguero «NO_EXISTE» no está en el catálogo.", draft.SetBeam(new[] { P(0, 0) }, "NO_EXISTE").Error);
        }
    }
}
