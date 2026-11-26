#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Controls;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, Selectivo, third wave (β): the tests of the draft of the window that were missing (priority A of the gap analysis of
    /// the entrega 1: the ones that protect the drawing). The counts of fondos and fronts at their limits, the refusals that must write
    /// nothing, the values «vacío = automático» that give the drawing back to the rule, the coercion and the atomicity of the largueros, the
    /// tramos of a half front against the width the classic dialog checks (the possible defect A: it is not one, the width is shared), the
    /// safety kept for the entrega 2, the normalizations of the load, and the writes over all the fondos of different shape.
    /// </summary>
    public class SelectiveDraftEditorGapTests
    {
        private const string C6 = "LARGUERO_IN_OUT_C6"; // a larguero of the catalog whose only peralte is 6

        private static SelectiveDraftEditor Open(SelectivePalletDesign? design = null, IReadOnlyDictionary<PropertyId, LinkedPropertyEditState>? states = null)
        {
            var draft = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe());
            Assert.True(draft.LoadFrom(design ?? Structure(), states, "Selectivo A"), draft.LastError);
            return draft;
        }

        private static SelectivePalletDesign Fondos(int count)
        {
            var design = Structure();
            design.DepthCount = count;
            for (var k = 1; k < count; k++)
            {
                design.SeparatorLengths.Add(10.0 + k);
                design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(3, floor: true, frente: 40.0), Bay(2, frente: 44.0, alto: 55.0) });
                design.ExtraFondoDepths.Add(48.0);
                design.CabeceraFondoOverrides.Add(0.0);
            }

            return design;
        }

        // ------------------------------------------------------------------ fondos and fronts at their limits

        [Fact]
        public void TheCountOfFondosStopsAtFourANewGapTakesTheLastSeparationAndFewerKeepTheOthers()
        {
            var draft = Open(Fondos(2));
            Assert.Equal(new[] { 11.0 }, draft.Separators);

            // The same count writes nothing (no commit, no change to say).
            Assert.Equal(new SelectiveDraftEdit(0, 0, null), draft.SetFondoCount(2));

            // Five is four (MaxDepthCount); the new gaps take the last separation, as the classic fields.
            Assert.False(draft.SetFondoCount(5).Refused);
            Assert.Equal(SelectiveRackDefaults.MaxDepthCount, draft.FondoCount);
            Assert.Equal(new[] { 11.0, 11.0, 11.0 }, draft.Separators);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(4, draft.Design!.DepthCount);

            // A scope of a fondo that disappears goes back to all the fondos; fewer fondos keep the separations that stay.
            Assert.False(draft.SetSeparator(1, 15.0).Refused);
            draft.SetFondoScope(3);
            Assert.Equal(3, draft.FondoScope);
            Assert.False(draft.SetFondoCount(3).Refused);
            Assert.Null(draft.FondoScope);
            Assert.Equal(new[] { 11.0, 15.0 }, draft.Separators);
            Assert.Equal("Número de fondos inválido (mínimo 1).", draft.SetFondoCount(0).Error);
            Assert.Equal(3, draft.FondoCount);
        }

        [Fact]
        public void FewerFrontsAreCutAndANewFrontCopiesTheFloorLargueroRiseOfTheLastOne()
        {
            var draft = Open();
            Assert.False(draft.SetFloorBeam(new[] { 1 }, true).Refused);
            Assert.False(draft.SetFloorBeamRise(new[] { 1 }, 8.0).Refused);

            Assert.False(draft.SetFrontCount(3).Refused);
            Assert.Equal(3, draft.FrontCount(0));
            Assert.Equal(8.0, draft.Front(0, 2)!.FloorBeamRise); // a direct value, copied from the last front
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(8.0, draft.Design!.Bays[2].FloorBeamRiseOverride);

            Assert.False(draft.SetFrontCount(1).Refused);
            Assert.Equal(1, draft.FrontCount(0));
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Single(draft.Design!.Bays);
            Assert.Equal(3, draft.Design!.Bays[0].Levels.Count); // what stays is intact
        }

        // ------------------------------------------------------------------ refusals write nothing

        [Fact]
        public void APalletDepthACabeceraDepthOrASeparationThatIsNotValidWritesNothing()
        {
            var draft = Open(Fondos(3));
            var before = Json(draft.Design);
            foreach (var bad in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity })
            {
                Assert.True(draft.SetPalletDepth(bad).Refused, "profundidad " + bad);
                Assert.True(draft.SetCabeceraDepth(bad).Refused, "cabecera " + bad);
                Assert.True(draft.SetSeparator(0, bad).Refused, "separación " + bad);
                Assert.True(draft.SetPalletFront(new[] { P(0, 0) }, bad).Refused, "ancho " + bad);
                Assert.True(draft.SetBeamLengthOverride(new[] { P(0, 0) }, bad).Refused, "largo " + bad);
                Assert.True(draft.SetClearOverride(new[] { P(0, 1) }, bad).Refused, "claro " + bad);
            }

            // A gap that does not exist: the third of three fondos has none to the next one.
            Assert.Equal("Esa separación no existe.", draft.SetSeparator(2, 12.0).Error);
            Assert.Equal("Esa separación no existe.", draft.SetSeparator(-1, 12.0).Error);
            Assert.Equal("Esa separación no existe.", Open().SetSeparator(0, 12.0).Error); // one fondo, no gap
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(before, Json(draft.Design));

            // The second gap of three fondos is its own.
            Assert.Equal(1, draft.SetSeparator(1, 20.0).Written);
            Assert.Equal(new[] { 11.0, 20.0 }, draft.Separators);
        }

        [Fact]
        public void AFloorLargueroRiseOfZeroIsAValueAndANegativeOneIsRefused()
        {
            var draft = Open();
            Assert.Equal(1, draft.SetFloorBeamRise(new[] { 0 }, 0.0).Written);
            Assert.Equal(0.0, draft.Front(0, 0)!.FloorBeamRise);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(0.0, draft.Design!.Bays[0].FloorBeamRiseOverride); // 0, not the default of 4
            Assert.True(draft.SetFloorBeamRise(new[] { 0 }, -2.0).Refused);
            Assert.True(draft.SetFloorBeamRise(new[] { 0 }, double.PositiveInfinity).Refused);
            Assert.Equal(0.0, draft.Front(0, 0)!.FloorBeamRise);
        }

        // ------------------------------------------------------------------ «vacío = automático» gives the drawing back to the rule

        [Fact]
        public void EmptyManualValuesGiveTheWidthTheClearTheHeightAndTheCabeceraBackToTheRule()
        {
            var draft = Open();
            Assert.True(draft.Recompose(), draft.LastError);
            var width = draft.System!.Bays[0].BeamLength;
            var height = draft.System!.Bays[0].Height;
            var levelY = draft.System!.Bays[0].Levels[1].Y;
            var cabecera = draft.Fondo(0)!.CabeceraDepth;

            draft.SetBeamLengthOverride(new[] { P(0, 0) }, 150.0);
            draft.SetHeightOverride(new[] { 0 }, 300.0);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(150.0, draft.System!.Bays[0].BeamLength);
            Assert.Equal(300.0, draft.System!.Bays[0].Height);

            // The automatic values the window shows beside them are what emptying them gives (the same resolver).
            var automatic = draft.AutomaticValues(new[] { P(0, 0) }, new[] { 0 });
            Assert.Equal(new[] { width }, automatic.BeamLengths);
            Assert.Equal(new[] { height }, automatic.Heights);

            draft.SetClearOverride(new[] { P(0, 1) }, 90.0);
            draft.SetCabeceraDepth(38.0);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.NotEqual(levelY, draft.System!.Bays[0].Levels[1].Y);
            Assert.Equal(38.0, draft.Fondo(0)!.CabeceraDepth);

            draft.SetBeamLengthOverride(new[] { P(0, 0) }, null);
            draft.SetClearOverride(new[] { P(0, 1) }, null);
            draft.SetHeightOverride(new[] { 0 }, null);
            draft.SetCabeceraDepth(null);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(width, draft.System!.Bays[0].BeamLength);
            Assert.Equal(height, draft.System!.Bays[0].Height);
            Assert.Equal(levelY, draft.System!.Bays[0].Levels[1].Y);
            Assert.Null(draft.Fondo(0)!.CabeceraDepthOverride);
            Assert.Equal(cabecera, draft.Fondo(0)!.CabeceraDepth);
            Assert.Equal(Json(Open().Design), Json(draft.Design));
        }

        [Fact]
        public void TheAutomaticLengthOfACellIsTheWidthOfItsFrontWithoutItsManualValueAndAnotherManualOneStillGoverns()
        {
            var draft = Open();
            draft.SetBeamLengthOverride(new[] { P(0, 2) }, 130.0); // another level of the same front
            draft.SetBeamLengthOverride(new[] { P(0, 0) }, 150.0);
            Assert.True(draft.Recompose(), draft.LastError);

            // Emptying F1·N1 leaves the manual 130 of F1·N3 as the widest level: that is the automatic value of F1·N1.
            Assert.Equal(new[] { 130.0 }, draft.AutomaticValues(new[] { P(0, 0) }, Array.Empty<int>()).BeamLengths);
            // Two fronts of different widths: two values; nothing before a valid recomposition.
            Assert.Equal(2, draft.AutomaticValues(new[] { P(0, 0), P(1, 0) }, new[] { 0, 1 }).BeamLengths.Count);
            var fresh = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe());
            Assert.Empty(fresh.AutomaticValues(new[] { P(0, 0) }, new[] { 0 }).BeamLengths);
            // The design itself is not touched by the reading.
            var before = Json(draft.Design);
            draft.AutomaticValues(new[] { P(0, 0), P(0, 2) }, new[] { 0 });
            Assert.Equal(before, Json(draft.Design));
        }

        [Fact]
        public void TheCountsOfTheBomAreThoseOfTheWrittenDesignAndCountingThemChangesNoDesign()
        {
            // I-01 of the review of the polish: the BOM imposes the depth of its fondo on a custom cabecera it reads, in place (ARQ-43-10), and
            // the system of the draft shares the custom cabeceras with its last valid design. Here P1 of fondo 2 keeps 42 in, its rule is 34.
            var draft = Open(TwoFondosWithACustomCabeceraOutOfItsRule());
            var custom = draft.Design!.ExtraFondoPostCabeceras[0][0]!;
            Assert.Equal((42.0, 34.0), (custom.Depth, SelectiveDepthLayout.CabeceraDepthOfFondo(draft.System!, 1)));
            var json = Json(draft.Design);
            var written = draft.SnapshotForWrite()!.DesignJson;
            var members = custom.Members.Count;

            var counts = draft.BomCounts();

            // Counting changes nothing: not the last valid design, not what «Actualizar» writes, not the derived members.
            Assert.Equal(json, Json(draft.Design));
            Assert.Equal(written, draft.SnapshotForWrite()!.DesignJson);
            Assert.Equal((42.0, members), (custom.Depth, custom.Members.Count));
            Assert.Equal(counts, draft.BomCounts());

            // The counts are those of a BOM made apart, on the design that is written, read and resolved again.
            var catalog = draft.Catalog;
            var system = new SelectiveGeometryResolver().Resolve(Store.Deserialize(written)!.ToDomain(), catalog);
            Assert.Equal(SelectiveBomCounts.Of(system, catalog), counts);
            Assert.Equal(new SelectiveBomCounts(7, 22, 6), counts);

            // Nothing before the first valid recomposition.
            Assert.Equal(new SelectiveBomCounts(0, 0, 0), new SelectiveDraftEditor(catalog).BomCounts());
        }

        [Fact]
        public void NoReaderOfTheSystemOfTheDraftOrOfItsWriteChangesTheirDesign()
        {
            // I-1 of the review of the entrega 2, at its origin: the third path that changed the design by reading the system («Insertar
            // vista…», after «Calculado» and the safety tab). The readers of the drawing impose the depth of its fondo on every custom
            // cabecera they read, in place (ARQ-43-10, deferred); the system of the draft and the one of its write are resolved from copies
            // of their own, so whatever reads them, the design and the JSON that is written stay. Here P1 of fondo 2 keeps 42 in, its rule is 34.
            var draft = Open(TwoFondosWithACustomCabeceraOutOfItsRule());
            var catalog = draft.Catalog;
            Assert.Equal((42.0, 34.0), (draft.Design!.ExtraFondoPostCabeceras[0][0]!.Depth, SelectiveDepthLayout.CabeceraDepthOfFondo(draft.System!, 1)));
            var json = Json(draft.Design);
            var write = draft.SnapshotForWrite()!;
            Assert.Equal(json, write.DesignJson);

            void Walk(SelectiveRackSystem system)
            {
                _ = SelectiveBomBuilder.Build(system, catalog);
                var cuts = new SelectiveLateralBuilder().Cortes(system, catalog);
                Assert.NotEmpty(cuts);
                _ = new SelectivePlantaBuilder().BuildPlan(system, catalog);
                _ = new SelectivePlantaBuilder().Build(system, catalog);
                _ = SelectiveDoorInsertion.ViewOptions(system, catalog);
                _ = SelectiveDoorInsertion.DrawableViews(cuts.Select(c => RackViewAddress.Post(c.PostIndex)).ToList(), system, catalog);
                _ = SelectivePostSafetySummary.Build(system, catalog);
                _ = SelectiveParrillaPlan.Cells(system, catalog);
            }

            Walk(draft.System!);
            Walk(write.System);

            Assert.Equal(json, Json(draft.Design));
            Assert.Equal(json, Json(write.Design));
            Assert.Equal(json, draft.SnapshotForWrite()!.DesignJson);
            Assert.Equal(42.0, draft.Design!.ExtraFondoPostCabeceras[0][0]!.Depth);

            // Structurally: no custom cabecera of a system is an object of its design.
            foreach (var (design, system) in new[] { (draft.Design!, draft.System!), (write.Design, write.System) })
            {
                for (var k = 0; k < SelectiveDepthLayout.Count(system); k++)
                {
                    for (var post = 0; post <= SelectiveDepthLayout.BaysOfFondo(system, k).Count; post++)
                    {
                        var stored = SelectiveCabeceraAuthority.CustomAt(design, k, post);
                        if (stored != null) Assert.NotSame(stored, SelectiveCabeceraAuthority.CustomAt(system, k, post));
                    }
                }
            }
        }

        // ------------------------------------------------------------------ the largueros: coercion and atomicity

        [Fact]
        public void ALargueroThatDoesNotAllowThePeralteOfTheCellTakesItsFirstOneAndItsIdIsTheOneOfTheCatalog()
        {
            var draft = Open();
            Assert.Equal(4.0, draft.Cell(0, 0, 0)!.BeamPeralte);

            Assert.Equal(1, draft.SetBeam(new[] { P(0, 0) }, "  " + C6.ToLowerInvariant() + " ").Written);
            Assert.Equal(C6, draft.Cell(0, 0, 0)!.BeamId);
            Assert.Equal(6.0, draft.Cell(0, 0, 0)!.BeamPeralte); // 4 is not a peralte of the C6: its first one

            // Back to a larguero that allows 6: the peralte stays.
            Assert.Equal(1, draft.SetBeam(new[] { P(0, 0) }, BeamId).Written);
            Assert.Equal(6.0, draft.Cell(0, 0, 0)!.BeamPeralte);

            Assert.Equal("Selecciona un larguero.", draft.SetBeam(new[] { P(0, 0) }, " ").Error);
            Assert.Equal("El larguero «NO_EXISTE» no está en el catálogo.", draft.SetBeam(new[] { P(0, 0) }, "NO_EXISTE").Error);
        }

        [Fact]
        public void APeralteThatOneLargueroOfTheSelectionDoesNotAllowWritesItInNoCell()
        {
            var draft = Open();
            draft.SetBeam(new[] { P(0, 1) }, C6);
            var before = (draft.Cell(0, 0, 0)!.BeamPeralte, draft.Cell(0, 0, 1)!.BeamPeralte);

            var edit = draft.SetBeamPeralte(new[] { P(0, 0), P(0, 1) }, 3.5);

            Assert.True(edit.Refused);
            Assert.Equal("El larguero «" + C6 + "» no admite un peralte de 3.5 in en el catálogo.", edit.Error);
            Assert.Equal(before, (draft.Cell(0, 0, 0)!.BeamPeralte, draft.Cell(0, 0, 1)!.BeamPeralte));
            // A peralte both allow is written in both.
            Assert.Equal(2, draft.SetBeamPeralte(new[] { P(0, 0), P(0, 1) }, 6.0).Written);
            Assert.Equal((6.0, 6.0), (draft.Cell(0, 0, 0)!.BeamPeralte, draft.Cell(0, 0, 1)!.BeamPeralte));
        }

        // ------------------------------------------------------------------ levels and tramos

        [Fact]
        public void FewerLevelsDropTheTopOnesAndKeepTheOthers()
        {
            var draft = Open();
            draft.SetPalletHeight(new[] { P(0, 0) }, 50.0);
            Assert.Equal(1, draft.SetLevelCount(new[] { 0 }, 1).Written);
            Assert.Equal(1, draft.LevelCount(0, 0));
            Assert.Equal(50.0, draft.Cell(0, 0, 0)!.Alto);
            Assert.Null(draft.Cell(0, 0, 1));
        }

        [Fact]
        public void LessThanTwoTramosIsAWholeFrontAndWithNoSystemTheLengthIsNotChecked()
        {
            var draft = Open();
            draft.SetSegments(0, new[] { new SelectiveSegment { Length = 30.0, Loaded = true }, new SelectiveSegment() });
            Assert.True(draft.Front(0, 0)!.IsSplit);

            Assert.Equal(1, draft.SetSegments(0, new[] { new SelectiveSegment { Length = 30.0, Loaded = true } }).Written);
            Assert.False(draft.Front(0, 0)!.IsSplit);

            // Before the first valid recomposition there is no width: the check of the length is skipped, as the classic dialog does with 0.
            var fresh = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe());
            Assert.Equal(0.0, fresh.FrontWidth(0));
            Assert.False(fresh.SetSegments(0, new[] { new SelectiveSegment { Length = 900.0, Loaded = true }, new SelectiveSegment() }).Refused);
        }

        [Fact]
        public void WithAllTheFondosTheTramosAreCheckedAgainstTheSharedWidthOfTheFrontAsTheClassicDialogChecksIt()
        {
            // Possible defect A of the gap analysis: «SetSegments validates against the width of ONE fondo with «Todos los fondos»». It is not
            // a defect: the resolver gives every fondo that has a front the SAME width (the master fondo governs), and the classic dialog checks
            // the width of the front in the visible fondo. With fondo 1 narrower in F1, the width is the same in every fondo and every scope.
            var design = TwoFondos();
            design.ExtraFondoBays[0][0] = Bay(3, floor: true, frente: 48.0);
            var authored = Authored(design);

            StaTestRunner.Run(() =>
            {
                var register = ProjectVariablesReadResult.Absent();
                var open = SelectiveEditorOpen.Resolve(Store.Deserialize(authored), register);
                Assert.True(open.IsOpen, open.Error);
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                try
                {
                    window.SetProjectVariables(LinkedPropertyOptions.ForProperty(Clearance, register));
                    window.LoadExisting(Store.Deserialize(authored), open.Design, open.LinkedPropertyStates);
                    var selector = (ComboBox)window.FindName("FondoSelectorBox");
                    var classic = new List<double>();
                    for (var k = 0; k < 2; k++)
                    {
                        selector.SelectedIndex = k;
                        classic.Add(window.TramosFullWidthForTest(0));
                    }

                    var read = Read(1, RackId, design);
                    var draft = new SelectiveDraftEditor(window.Session.Catalog);
                    Assert.True(draft.LoadFrom(read.EffectiveDesign()!, read.LinkedPropertyStates, read.Name), draft.LastError);
                    var widths = new[] { (int?)null, 0, 1 }.Select(scope => draft.WithinScope(scope, () => draft.FrontWidth(0))).ToList();

                    Assert.Equal(classic[0], classic[1]);
                    Assert.All(widths, w => Assert.Equal(classic[0], w));
                    Assert.Equal(draft.System!.FondoBays[0][0].BeamLength, draft.System!.FondoBays[1][0].BeamLength);

                    // The same tramos are refused, or written, whatever the scope.
                    var full = classic[0];
                    foreach (var scope in new[] { (int?)null, 0, 1 })
                    {
                        var refused = draft.WithinScope(scope, () => draft.SetSegments(0, new[] { new SelectiveSegment { Length = full, Loaded = true }, new SelectiveSegment() }));
                        Assert.True(refused.Refused, "ámbito " + scope);
                    }

                    var written = draft.SetSegments(0, new[] { new SelectiveSegment { Length = full - 1.0, Loaded = true }, new SelectiveSegment() });
                    Assert.Equal(2, written.Written);
                    Assert.True(draft.Front(0, 0)!.IsSplit);
                    Assert.True(draft.Front(1, 0)!.IsSplit);
                }
                finally { window.Close(); }
            });
        }

        // ------------------------------------------------------------------ all the fondos, of different shape

        [Fact]
        public void WritesOverAllTheFondosSayWhatTheyOmittedAndAWriteThatReachesNothingSaysSo()
        {
            var draft = Open(TwoFondos()); // F3 only in fondo 2

            Assert.Equal(new SelectiveDraftEdit(1, 1, null), draft.SetLevelCount(new[] { 2 }, 4));
            Assert.Equal(new SelectiveDraftEdit(1, 1, null), draft.SetHeightOverride(new[] { 2 }, 200.0));
            Assert.Equal(new SelectiveDraftEdit(1, 1, null), draft.SetFloorBeamRise(new[] { 2 }, 6.0));
            var beam = draft.SetBeam(new[] { P(2, 0) }, C6);
            Assert.Equal((1, 1), (beam.Written, beam.Omitted));
            Assert.Equal(C6, draft.Cell(1, 2, 0)!.BeamId);

            // Only fondo 1 in the view: F3 is nowhere, nothing is written, and it is said (not a success).
            draft.SetFondoScope(0);
            var nothing = draft.SetPalletHeight(new[] { P(2, 0) }, 70.0);
            Assert.Equal(0, nothing.Written);
            Assert.True(nothing.Omitted > 0);
            Assert.Null(nothing.Error);
            Assert.Equal("Lo elegido no existe en los fondos de la vista: no se escribió nada.", nothing.Notice);
            Assert.Equal(60.0, draft.Cell(1, 2, 0)!.Alto);
        }

        [Fact]
        public void WithinAScopeRunsInThatScopeAndPutsTheViewBack()
        {
            var draft = Open(TwoFondos());
            draft.SetFondoScope(1);

            var all = draft.WithinScope(null, () => draft.ScopeFondos.ToArray());
            Assert.Equal(new[] { 0, 1 }, all);
            Assert.Equal(1, draft.FondoScope);
            Assert.True(draft.WithinScope(null, () => draft.SetPalletHeight(draft.ScopePositions(), 70.0)).Written > 0);
            Assert.Equal(70.0, draft.Cell(0, 0, 0)!.Alto);
            Assert.Equal(70.0, draft.Cell(1, 2, 0)!.Alto);
            Assert.Throws<InvalidOperationException>(() => draft.WithinScope<int>(0, () => throw new InvalidOperationException()));
            Assert.Equal(1, draft.FondoScope); // also after a failure
        }

        // ------------------------------------------------------------------ safety, kept for the entrega 2

        [Fact]
        public void TheSafetyIsCopiedAndOnlyWhatDrawsOrCountsIsKept()
        {
            var draft = Open();
            var draws = new SelectiveSafetySelection { ElementId = "BOTA", Quantity = 0, Side = SafetySide.Both };
            var counts = new SelectiveSafetySelection { ElementId = "PROTECTOR", Quantity = 2, Side = SafetySide.None };
            var nothing = new SelectiveSafetySelection { ElementId = "NADA", Quantity = 0, Side = SafetySide.None };
            var noId = new SelectiveSafetySelection { ElementId = " ", Quantity = 3, Side = SafetySide.Both };

            draft.ReplaceSafety(new[] { draws, counts, nothing, noId, null! });

            var copy = draft.CopySafety();
            Assert.Equal(new[] { "BOTA", "PROTECTOR" }, copy.Select(s => s.ElementId).ToArray());
            Assert.NotSame(draws, copy[0]);
            copy[0].Quantity = 99;
            draws.Quantity = 77;
            Assert.Equal(0, draft.CopySafety()[0].Quantity); // neither the caller nor the copy reach the draft
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(2, draft.Design!.SafetySelections.Count);

            draft.ReplaceSafety(null!);
            Assert.Empty(draft.CopySafety());
        }

        // ------------------------------------------------------------------ the normalizations of the load

        [Fact]
        public void TheLoadNormalizesWhatTheClassicEditorNormalizesAndSaysWhatItPadded()
        {
            // Two fondos with no bays of their own: fondo 2 is fondo 1; its depth 0 takes the base; a depth of 0 is the default.
            var shared = Structure();
            shared.DepthCount = 2;
            shared.PalletDepth = 0.0;
            shared.ExtraFondoDepths.Add(0.0);
            var draft = Open(shared);
            Assert.Equal(2, draft.FondoCount);
            Assert.Equal(draft.FrontCount(0), draft.FrontCount(1));
            Assert.Equal(draft.LevelCount(0, 0), draft.LevelCount(1, 0));
            Assert.Equal(SelectiveRackDefaults.DefaultPalletDepth, draft.Fondo(0)!.PalletDepth);
            Assert.Equal(SelectiveRackDefaults.DefaultPalletDepth, draft.Fondo(1)!.PalletDepth);
            Assert.Equal(new[] { SelectiveRackDefaults.DefaultSeparator }, draft.Separators);

            // A run-wide rise of 0 is a value for the fronts with no own one; a negative one is the default.
            var zero = Structure();
            zero.FloorBeamRise = 0.0;
            zero.Bays[0].FloorBeamRiseOverride = null;
            Assert.Equal(0.0, Open(zero).Front(0, 0)!.FloorBeamRise);
            var negative = Structure();
            negative.FloorBeamRise = -1.0;
            negative.Bays[0].FloorBeamRiseOverride = null;
            Assert.Equal(SelectiveRackDefaults.DefaultFloorBeamRise, Open(negative).Front(0, 0)!.FloorBeamRise);

            // An empty front (a column) is loaded with one level, and said.
            var column = Structure();
            column.Bays.Add(new SelectiveBayDesign());
            column.PostPeraltes.Add(0.0);
            var padded = Open(column);
            Assert.Equal(1, padded.LevelCount(0, 2));
            Assert.Contains("frente(s) vacío(s)", padded.LoadNotice);

            Assert.Throws<ArgumentNullException>(() => new SelectiveDraftEditor(UiSupport.LoadCatalogSafe()).LoadFrom(null!, null, "X"));
        }

        // ------------------------------------------------------------------ the holguras (decision 12)

        [Fact]
        public void ALinkedToleranceIsReadOnlyAndItsBindingIsKeptWhileALiteralOneIsWrittenAsItIsNow()
        {
            var variable = VariableId.New();
            var states = new Dictionary<PropertyId, LinkedPropertyEditState> { [Tolerance] = LinkedPropertyEditState.Reference(4.0, variable) };
            var draft = Open(states: states);

            Assert.True(draft.PalletToleranceIsLinked);
            Assert.False(draft.VerticalClearanceIsLinked);
            Assert.Equal("La gobierna una variable de proyecto: en esta ventana es de solo lectura.", draft.LinkedReadOnlyReason(Tolerance));
            Assert.Null(draft.LinkedReadOnlyReason(Clearance));

            // Changing the linked tolerance cannot be recomposed: the last valid design stays.
            draft.PalletTolerance = 5.0;
            Assert.False(draft.Recompose());
            Assert.StartsWith("Tolerancia horizontal: La gobierna una variable de proyecto", draft.LastError);
            Assert.True(draft.RestoreLastValid());
            Assert.Equal(4.0, draft.PalletTolerance);

            // The clearance is the rack's: its final state is the literal it has now; the tolerance keeps its binding, untouched.
            draft.VerticalClearance = 8.0;
            Assert.True(draft.Recompose(), draft.LastError);
            var final = draft.LinkedPropertyFinalStates;
            Assert.Equal(LinkedPropertyEditState.Literal(8.0), final[Clearance]);
            Assert.Same(states[Tolerance], final[Tolerance]);
            Assert.True(final[Tolerance].IsReference);
        }
    }
}
