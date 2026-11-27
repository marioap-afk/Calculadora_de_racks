#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// T5 ct (I-64, fifth batch; prototype «cantilever»): the draft of a Cantilever line of the RackCad window (<see cref="CantileverDraftEditor"/>)
/// over the recompute authority of the classic editor, the pure rules of its door (<see cref="CantileverDoorRules"/>), and, on the source, the
/// commands of the door, the extraction of the drawing half of RACKEDITAR (<c>ApplyCantileverEdit</c>) and B-1 in Cantilever (CT-11).
/// </summary>
public class CantileverDraftEditorTests
{
    private const string ColumnW = "AISC-W-W10X33";
    private const string BaseW = "AISC-W-W12X26";
    private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";
    private const string ArmChannel = "AISC-C-C10X15_3";
    private const string Angle = "AISC-L-L2X2X3_16";

    private static readonly StructuralSectionCatalog Catalog = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

    private static CantileverDraftEditor NewDraft() => CantileverDraftEditor.CreateNew(Catalog);

    // «Lo esencial» of the prototype: the column, the length of the base, the section and the cut of the default arm; the base follows the column.
    private static CantileverDraftEditor Resolved(int stations = 3, int levels = 3, bool doubleFace = false)
    {
        var draft = NewDraft();
        Assert.Null(draft.Write(CantileverLineField.StationCount, stations.ToString()));
        Assert.Null(draft.Write(CantileverLineField.Spacing, "96"));
        Assert.Null(draft.Write(CantileverLineField.LevelCount, levels.ToString()));
        Assert.Null(draft.Write(CantileverLineField.ClearHeight, "24"));
        if (doubleFace) Assert.Null(draft.Write(CantileverLineField.FaceMode, "Double"));
        Assert.Null(draft.Write(CantileverLineField.ColumnSection, ColumnW));
        Assert.Null(draft.Write(CantileverLineField.BaseSection, BaseW));
        Assert.Null(draft.Write(CantileverLineField.BaseLength, "48"));
        Assert.Null(draft.WriteArm(null, CantileverArmField.Section, ArmHss));
        Assert.Null(draft.WriteArm(null, CantileverArmField.CutLength, "36"));
        Assert.True(draft.IsResolved, string.Join(" | ", draft.Missing.Select(m => m.Message)));
        return draft;
    }

    private static string Json(CantileverLineDesign design) => new RackProjectStore().Serialize(RackProject.ForCantilever(design));

    [Fact]
    public void ANewLineIsTheNewLineOfTheClassicEditorAndSaysWhatItLacksAndWhere()
    {
        var draft = NewDraft();

        // The classic LoadNew: a new CantileverLineDesign, nothing invented (no section, no cut).
        var classic = new CantileverLineDesign { Id = draft.Design.Id };
        Assert.Equal(Json(classic), draft.DesignJson);
        Assert.False(draft.IsResolved);
        Assert.False(draft.IsDirty);
        Assert.Null(draft.SnapshotForWrite());

        var missing = draft.Missing;
        Assert.Equal(new[]
        {
            (CantileverDraftEditor.TabComponents, CantileverDraftEditor.ComponentColumn),
            (CantileverDraftEditor.TabComponents, CantileverDraftEditor.ComponentColumn),
            (CantileverDraftEditor.TabComponents, CantileverDraftEditor.ComponentColumn),
            (CantileverDraftEditor.TabComponents, CantileverDraftEditor.ComponentArm),
            (CantileverDraftEditor.TabComponents, CantileverDraftEditor.ComponentArm),
            (CantileverDraftEditor.TabLine, CantileverDraftEditor.ComponentLevels), // the clear of 0 of the classic: the station refuses it
        }, missing.Select(m => (m.Tab, m.Component)).ToArray());
        Assert.Equal("Falta la sección de columna.", missing[0].Message);

        // «Descartar» of a new line goes back to its defaults, which the store cannot read (a line without sections is not usable).
        Assert.Null(draft.Write(CantileverLineField.Spacing, "120"));
        Assert.True(draft.IsDirty);
        draft.Discard();
        Assert.False(draft.IsDirty);
        Assert.Equal(Json(classic), draft.DesignJson);
        Assert.Contains("no hay valor por omisión aprobado", missing[2].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LoEsencialResolvesTheLineAsTheClassicEditorResolvesTheSameDesign()
    {
        var draft = Resolved();
        Assert.Empty(draft.Missing);

        // The same values written by hand, as the controls and the configurators of the classic editor write them.
        var design = new CantileverLineDesign { Id = draft.Design.Id, StationCount = 3, ColumnCentreSpacing = 96.0 };
        design.StationTopology.LevelCount = 3;
        design.StationTopology.RequestedClearHeight = 24.0;
        var state = new CantileverColumnBaseEditorState(design.StationTopology.ColumnBaseTemplate, CantileverCataloguePolicies.ColumnBase(Catalog));
        state.SelectColumn(ColumnW);
        state.SelectBase(BaseW);
        design.StationTopology.ColumnBaseTemplate = state.Accept();
        design.StationTopology.ColumnBaseTemplate.Base.Length = 48.0;
        design.DefaultArmTemplate.Body.SectionId = ArmHss;
        var height = new StructuralSectionGeometryFactory(Catalog).Get(Catalog.GetById(StructuralSectionId.Parse(ArmHss)).SectionId, SectionDetailLevel.Tabulated).Bounds.Height;
        design.DefaultArmTemplate.MountingPlate.VerticalPunchCount = CantileverArmPunchCountRule.For(height,
            CantileverDefaults.ArmMountingPlateVerticalEndOffset, CantileverDefaults.RegularColumnPunchPitch);
        design.DefaultArmTemplate.Body.CutLength = 36.0;
        Assert.Equal(Json(design), draft.DesignJson);

        // And the line, its BOM and its counts are those of the recompute authority of the classic editor.
        var classic = new CantileverLineEditorAssembler(Catalog).Build(design);
        Assert.True(classic.IsValid);
        Assert.Equal(classic.Line.ColumnHeight, draft.Computation.Line.ColumnHeight);
        Assert.Equal(classic.Line.Separators.Count, draft.Computation.Line.Separators.Count);
        Assert.Equal(classic.Bom.Components.Count, draft.BomCopy()!.Components.Count);
    }

    [Theory]
    [InlineData(CantileverLineField.StationCount, "1")]
    [InlineData(CantileverLineField.StationCount, "abc")]
    [InlineData(CantileverLineField.Spacing, "-5")]
    [InlineData(CantileverLineField.Spacing, "0")]
    [InlineData(CantileverLineField.LevelCount, "0")]
    [InlineData(CantileverLineField.TopClearFactor, "0.2")]
    [InlineData(CantileverLineField.FirstLevelPunch, "0")]
    [InlineData(CantileverLineField.ColumnSection, "W10X33")]
    [InlineData(CantileverLineField.ColumnSection, ArmHss)]
    [InlineData(CantileverLineField.SeparatorSection, ColumnW)]
    [InlineData(CantileverLineField.BaseLength, "")]
    [InlineData(CantileverLineField.SingleSide, "Arriba")]
    [InlineData(CantileverLineField.ConnectionPunchesAboveBase, "-1")]
    public void AValueTheFieldRefusesLeavesTheDraftAsItWas(CantileverLineField field, string text)
    {
        var draft = Resolved();
        var before = draft.DesignJson;

        var refused = draft.Write(field, text);

        Assert.False(string.IsNullOrWhiteSpace(refused));
        Assert.Equal(before, draft.DesignJson);
        Assert.True(draft.IsResolved);
    }

    [Fact]
    public void DormantValuesAreRefusedWhileTheirModeDoesNotUseThemAndKeptWhenItChanges()
    {
        var draft = Resolved();

        // dolor 7: «Altura manual» with automatic height, «Cantidad manual» with automatic count, the side with double faces.
        Assert.Equal("Solo con altura manual; la que había se conserva.", draft.Write(CantileverLineField.ManualHeight, "264"));
        Assert.Equal("Solo con cantidad manual; la que había se conserva.", draft.Write(CantileverLineField.ManualPanelCount, "4"));
        Assert.Null(draft.Write(CantileverLineField.HeightMode, "Manual"));
        Assert.Null(draft.Write(CantileverLineField.ManualHeight, "264"));
        Assert.Null(draft.Write(CantileverLineField.HeightMode, "Automatic"));
        Assert.Equal(264.0, draft.Design.StationTopology.ColumnHeight.ManualHeight);
        Assert.Null(draft.Write(CantileverLineField.FaceMode, "Double"));
        Assert.Equal("Con doble cara se usan los dos lados.", draft.Write(CantileverLineField.SingleSide, "NegativeY"));

        // A manual mode with no value is a thing the line lacks (CT-05), with where it is written.
        Assert.Null(draft.Write(CantileverLineField.PanelCountMode, "Manual"));
        Assert.Contains(draft.Missing, m => m.Tab == CantileverDraftEditor.TabBracing);
    }

    [Fact]
    public void TheFirstPunchIsCountedFromOneAndStoredAsTheIndexOfTheClassic()
    {
        var draft = Resolved();
        Assert.Equal("1", draft.TextOf(CantileverLineField.FirstLevelPunch));
        Assert.Null(draft.Write(CantileverLineField.FirstLevelPunch, "3"));
        Assert.Equal(2, draft.Design.StationTopology.FirstLevelPunchIndex);
        Assert.Equal("3", draft.TextOf(CantileverLineField.FirstLevelPunch));
    }

    [Fact]
    public void AWriteOfTheSelectionIsTheExceptionOfEachCellAndTodoWritesThemAllWithoutTouchingTheDefault()
    {
        var draft = Resolved();
        var cell = new CantileverLineCell(2, 2, CantileverArmSide.PositiveY);

        // The example of the inventory: station 3, level 3, +Y with an arm of 60 in.
        Assert.Null(draft.WriteArm(new[] { cell }, CantileverArmField.CutLength, "60"));
        Assert.Equal(60.0, draft.Design.OverrideFor(2, 2, CantileverArmSide.PositiveY).Body.CutLength);
        Assert.Equal(36.0, draft.Design.DefaultArmTemplate.Body.CutLength);
        Assert.Equal(1, draft.Matrix.OverrideCount);

        // Back to the value of the default: the exception equal to the default arm is not stored (the rule of the matrix).
        Assert.Null(draft.WriteArm(new[] { cell }, CantileverArmField.CutLength, "36"));
        Assert.Equal(0, draft.Matrix.OverrideCount);

        // «Varía» for a selection with different values.
        Assert.Null(draft.WriteArm(new[] { cell }, CantileverArmField.CutLength, "60"));
        var two = new[] { cell, new CantileverLineCell(0, 0, CantileverArmSide.PositiveY) };
        Assert.True(draft.ArmText(two, CantileverArmField.CutLength).Varies);
        Assert.Equal("", draft.ArmText(two, CantileverArmField.CutLength).Text);

        // CT-06: «Todo» writes the exception in every cell; the default arm does not change.
        var all = draft.Matrix.Cells.ToList();
        Assert.Null(draft.WriteArm(all, CantileverArmField.CutLength, "40"));
        Assert.Equal(all.Count, draft.Matrix.OverrideCount);
        Assert.Equal(36.0, draft.Design.DefaultArmTemplate.Body.CutLength);

        // The default arm is written alone and does not touch the exceptions.
        Assert.Null(draft.WriteArm(null, CantileverArmField.Slope, "0.5"));
        Assert.All(all, c => Assert.Equal(0.4375, draft.Design.OverrideFor(c.StationIndex, c.LevelIndex, c.Side).Body.SlopeRisePer12));

        // «Volver al brazo por omisión».
        Assert.Null(draft.RestoreCells(all.Take(4).ToList()));
        Assert.Equal(all.Count - 4, draft.Matrix.OverrideCount);
    }

    [Fact]
    public void APairedArmNeedsAChannelBothWays()
    {
        var draft = Resolved();
        var before = draft.DesignJson;
        Assert.Equal("Un canal doble exige una sección de canal (C): elige antes una sección C.",
            draft.WriteArm(null, CantileverArmField.Arrangement, "DoubleChannelFacing"));
        Assert.Equal(before, draft.DesignJson);

        Assert.Null(draft.WriteArm(null, CantileverArmField.Section, ArmChannel));
        Assert.Null(draft.WriteArm(null, CantileverArmField.Arrangement, "DoubleChannelFacing"));
        Assert.Equal("Con canal doble, la sección tiene que ser un canal (C): pasa antes el arreglo a «Sencillo».",
            draft.WriteArm(null, CantileverArmField.Section, ArmHss));
        Assert.Equal(ArmChannel, draft.Design.DefaultArmTemplate.Body.SectionId);
    }

    [Fact]
    public void TheEndPlateAndItsFieldsFollowItsType()
    {
        var draft = Resolved();
        Assert.Equal("Sin placa de extremo.", draft.WriteArm(null, CantileverArmField.EndPlateThickness, "0.5"));
        Assert.Equal("Solo con «Placa con tope».", draft.WriteArm(null, CantileverArmField.ExtraStopHeight, "2"));
        Assert.Null(draft.WriteArm(null, CantileverArmField.EndPlate, "Stop"));
        Assert.Null(draft.WriteArm(null, CantileverArmField.ExtraStopHeight, "2"));
        Assert.Null(draft.WriteArm(null, CantileverArmField.EndPlate, "Cap"));
        Assert.Equal(0.0, draft.Design.DefaultArmTemplate.EndPlate.ExtraStopHeight);
    }

    [Fact]
    public void TheBaseFollowsTheColumnUntilItIsChosenByHand()
    {
        var draft = NewDraft();
        Assert.Null(draft.Write(CantileverLineField.ColumnSection, ColumnW));
        Assert.Equal(ColumnW, draft.Design.StationTopology.ColumnBaseTemplate.Base.SectionId);
        Assert.Null(draft.Write(CantileverLineField.BaseSection, BaseW));
        Assert.False(draft.Design.StationTopology.ColumnBaseTemplate.BaseFollowsColumn);
        Assert.Null(draft.Write(CantileverLineField.ColumnSection, "AISC-W-W12X26"));
        Assert.Equal(BaseW, draft.Design.StationTopology.ColumnBaseTemplate.Base.SectionId);
        Assert.Null(draft.Write(CantileverLineField.BaseFollowsColumn, bool.TrueString));
        Assert.Equal("AISC-W-W12X26", draft.Design.StationTopology.ColumnBaseTemplate.Base.SectionId);
    }

    [Fact]
    public void TheBraceFieldsFollowItsKindAndAStructuralBraceWithoutSectionIsMissing()
    {
        var draft = Resolved();
        Assert.Equal("Solo con tensor estructural (perfil).", draft.Write(CantileverLineField.BraceSection, Angle));
        Assert.Null(draft.Write(CantileverLineField.BraceKind, "StructuralSection"));
        Assert.False(draft.IsResolved);
        Assert.Contains(draft.Missing, m => m.Component == CantileverDraftEditor.ComponentBrace);
        Assert.NotNull(draft.Write(CantileverLineField.BraceSection, ColumnW));
        Assert.Null(draft.Write(CantileverLineField.BraceSection, Angle));
        Assert.True(draft.IsResolved, string.Join(" | ", draft.Missing.Select(m => m.Message)));
    }

    [Fact]
    public void TheAdvancedSequenceIsMaterializedAppliedKeptAndRecovered()
    {
        var draft = Resolved();
        Assert.Null(draft.Write(CantileverLineField.HeightMode, "Manual"));
        Assert.Null(draft.Write(CantileverLineField.ManualHeight, "264"));
        Assert.True(draft.IsResolved, string.Join(" | ", draft.Missing.Select(m => m.Message)));
        var automatic = draft.AutomaticSegments.Select(s => (s.StartElevation, s.EndElevation, s.BracingMode)).ToList();
        Assert.NotEmpty(automatic);

        // «Empezar desde la regla»: the sequence the rule draws now.
        Assert.Null(draft.ToAdvanced(recover: false));
        Assert.True(draft.IsAdvanced);
        Assert.Equal(automatic, draft.SavedSegments.Select(s => (s.StartElevation, s.EndElevation, s.BracingMode)).ToList());

        // «Aplicar secuencia»: the rows of the table, checked by row.
        var rows = CantileverDraftEditor.RowsOf(draft.SavedSegments).ToList();
        Assert.Equal("Tramo 1: «Hasta» tiene que estar por encima de «Desde».", draft.ApplySegments(new[] { new CantileverSegmentRow("10", "5", true) }));
        rows[0] = rows[0] with { Braced = !rows[0].Braced };
        Assert.Null(draft.ApplySegments(rows));
        Assert.Equal(rows[0].Braced, draft.SavedSegments[0].BracingMode != CantileverPanelBracingMode.None);

        // «Volver a la regla» keeps the tramos; «Recuperar mis tramos» gives them back.
        Assert.Null(draft.ToAutomatic());
        Assert.False(draft.IsAdvanced);
        Assert.Equal(rows.Count, draft.SavedSegments.Count);
        Assert.Null(draft.ToAdvanced(recover: true));
        Assert.Equal(rows, CantileverDraftEditor.RowsOf(draft.SavedSegments));
    }

    [Fact]
    public void RestaurarLayoutHasItsFourBoxesAndRestoresWhatIsMarked()
    {
        var draft = Resolved();
        Assert.All(draft.RestoreBoxes(), b => Assert.Equal(0, b.Count));
        Assert.Null(draft.WriteArm(new[] { new CantileverLineCell(2, 2, CantileverArmSide.PositiveY) }, CantileverArmField.CutLength, "60"));
        Assert.Null(draft.Write(CantileverLineField.HeightMode, "Manual"));
        Assert.Null(draft.Write(CantileverLineField.ManualHeight, "264"));
        Assert.Null(draft.Write(CantileverLineField.PanelCountMode, "Manual"));
        Assert.Null(draft.Write(CantileverLineField.ManualPanelCount, "4"));
        Assert.Null(draft.ToAdvanced(recover: false));

        var boxes = draft.RestoreBoxes();
        Assert.Equal(new[] { CantileverDraftEditor.RestoreExceptions, CantileverDraftEditor.RestoreManualHeight, CantileverDraftEditor.RestoreManualCount, CantileverDraftEditor.RestoreAdvanced },
            boxes.Select(b => b.Key).ToArray());
        Assert.All(boxes, b => Assert.Equal(1, b.Count));
        Assert.Contains("E3 · N3 +Y", boxes[0].Losses[0], StringComparison.Ordinal);

        Assert.Null(draft.Restore(new[] { CantileverDraftEditor.RestoreExceptions, CantileverDraftEditor.RestoreManualHeight }));
        Assert.Equal(0, draft.Matrix.OverrideCount);
        Assert.Equal(CantileverStationColumnHeightMode.Automatic, draft.Design.StationTopology.ColumnHeight.Mode);
        Assert.Equal(264.0, draft.Design.StationTopology.ColumnHeight.ManualHeight); // kept as dormant data, as the classic keeps it
        Assert.Equal(CantileverBracedPanelCountMode.Manual, draft.Design.Bracing.PanelCountMode); // not marked: unchanged

        Assert.Null(draft.Restore(new[] { CantileverDraftEditor.RestoreManualCount, CantileverDraftEditor.RestoreAdvanced }));
        Assert.False(draft.IsAdvanced);
        Assert.NotEmpty(draft.SavedSegments); // the tramos are kept without governing
    }

    [Fact]
    public void TheSnapshotIsACopyResolvedFromACopyAndItsJsonIsTheBaseOnceWritten()
    {
        var draft = Resolved();
        var snapshot = draft.SnapshotForWrite()!;
        Assert.NotSame(draft.Design, snapshot.Design);
        Assert.False(snapshot.Line.IsBlocked);
        Assert.Equal(Json(snapshot.Design), snapshot.DesignJson);

        // What is edited afterwards does not reach the copies.
        Assert.Null(draft.Write(CantileverLineField.Spacing, "120"));
        Assert.Equal(96.0, snapshot.Design.ColumnCentreSpacing);
        Assert.Equal(96.0, snapshot.Line.ColumnCentreSpacing);

        // Written: what was SENT is the base; «Descartar» goes back to it.
        var sent = draft.SnapshotForWrite()!;
        draft.MarkWritten(sent.DesignJson);
        Assert.False(draft.IsDirty);
        Assert.Null(draft.Write(CantileverLineField.Spacing, "100"));
        Assert.True(draft.IsDirty);
        draft.Discard();
        Assert.False(draft.IsDirty);
        Assert.Equal(120.0, draft.Design.ColumnCentreSpacing);
    }

    // ------------------------------------------------------------------ corrections of the review

    // Correction 1: a sequence of the rule whose elevations do not fit in 4 decimals (a clear of 20 in: a column of 106.666… in) is applied
    // with its exact values while the text of the row is the one shown for them (Keep of the classic editor), and the line still resolves.
    [Fact]
    public void ApplyingTheSequenceOfTheRuleKeepsItsExactElevationsAndTheLineStillResolves()
    {
        var draft = Resolved();
        Assert.Null(draft.Write(CantileverLineField.ClearHeight, "20"));
        Assert.True(draft.IsResolved, string.Join(" | ", draft.Missing.Select(m => m.Message)));
        var height = draft.Computation.Line.ColumnHeight;
        Assert.True(Math.Abs(height - Math.Round(height, 4)) > 1e-9, "the scenario needs a column that 4 decimals do not write: " + height);

        Assert.Null(draft.ToAdvanced(recover: false));
        var exact = draft.SavedSegments.Select(s => (s.StartElevation, s.EndElevation)).ToList();
        Assert.Contains(exact, e => Math.Abs(e.EndElevation - Math.Round(e.EndElevation, 4)) > 1e-9);

        // «Con tensores» of a tramo changed, and applied: the elevations are the exact ones, and the line resolves.
        var rows = CantileverDraftEditor.RowsOf(draft.SavedSegments).ToList();
        rows[0] = rows[0] with { Braced = !rows[0].Braced };
        Assert.Null(draft.ApplySegments(rows));
        Assert.Equal(exact, draft.SavedSegments.Select(s => (s.StartElevation, s.EndElevation)).ToList());
        Assert.True(draft.IsResolved, string.Join(" | ", draft.Missing.Select(m => m.Message)));

        // A text the user changed is the number it says; the text shown for the value («12.5» for 12.4999999) keeps the value.
        Assert.True(CantileverDraftEditor.Keep("12.6", 12.4999999, out var typed));
        Assert.Equal(12.6, typed);
        Assert.True(CantileverDraftEditor.Keep("12.5", 12.4999999, out var shown));
        Assert.Equal(12.4999999, shown);
        Assert.True(CantileverDraftEditor.Keep(CantileverDraftEditor.Num(106.0 + 2.0 / 3.0), 106.0 + 2.0 / 3.0, out var kept));
        Assert.Equal(106.0 + 2.0 / 3.0, kept);
    }

    // Correction 2: what the field shows for 1/3 («0.3333», the text of its own message) is 1/3, stored exact; less is refused.
    [Fact]
    public void TheTopClearFactorAcceptsTheThirdItShowsAndStoresItExact()
    {
        var draft = Resolved();
        Assert.Null(draft.Write(CantileverLineField.TopClearFactor, "0.5"));
        Assert.Null(draft.Write(CantileverLineField.TopClearFactor, "0.3333"));
        Assert.Equal(CantileverStationDefaults.TopClearFactor, draft.Design.StationTopology.TopClearFactor);
        Assert.Equal("0.3333", draft.TextOf(CantileverLineField.TopClearFactor));
        Assert.NotNull(draft.Write(CantileverLineField.TopClearFactor, "0.3332"));
        Assert.Equal(CantileverStationDefaults.TopClearFactor, draft.Design.StationTopology.TopClearFactor);
    }

    // Correction 4: the rule of RACKEDITAR: the name of the line is the one of the envelope; the inner name is not touched, and renaming
    // writes both (Name_Changed of the classic editor).
    [Fact]
    public void TheNameIsTheOneOfTheEnvelopeAndTheInnerNameIsWrittenOnlyByARename()
    {
        var line = Resolved().Design.DeepCopy();
        line.Name = "Interno";
        var draft = NewDraft();
        draft.Load(line, "Sobre");
        Assert.Equal("Sobre", draft.Name);
        Assert.Equal("Interno", draft.Design.Name);
        Assert.False(draft.IsDirty);
        Assert.Equal(("Sobre", "Interno"), (draft.SnapshotForWrite()!.Name, draft.SnapshotForWrite()!.Design.Name));

        line.Name = null;
        draft.Load(line, "Sobre");
        Assert.Null(draft.Design.Name); // a design with no name is not given one
        Assert.Null(draft.SnapshotForWrite()!.Design.Name);

        Assert.Null(draft.SetName("Nave B"));
        Assert.Equal(("Nave B", "Nave B"), (draft.Name, draft.Design.Name));
        Assert.True(draft.IsDirty);
        draft.Discard();
        Assert.Equal("Sobre", draft.Name);
        Assert.Null(draft.Design.Name);
        Assert.False(draft.IsDirty);
    }

    [Fact]
    public void TheNameIsTheNameOfTheDesignTrimmedAndAnEmptyOneIsNoName()
    {
        var draft = Resolved();
        Assert.Null(draft.SetName("  Nave A  "));
        Assert.Equal("Nave A", draft.Name);
        Assert.Equal("Nave A", draft.SnapshotForWrite()!.Name);
        Assert.Null(draft.SetName("   "));
        Assert.Null(draft.Name);
        Assert.Null(draft.Design.Name);
    }

    // ------------------------------------------------------------------ the rules of the door

    private static string Envelope(CantileverLineDesign design) => Json(design);

    [Fact]
    public void TheDoorOpensAReadableCantileverLineOnly()
    {
        var design = Resolved().Design;
        var open = CantileverDoorRules.AnswerForOpen(7, "CAN-1", RackEmbedDocument.KindCantilever, "Cantilever 1", Envelope(design));
        Assert.True(open.Opens);
        Assert.Equal("Cantilever 1", open.Name);
        Assert.Equal(Json(design), Json(open.Project()!.CantileverLineDesign));

        Assert.False(CantileverDoorRules.AnswerForOpen(7, "CAN-1", RackEmbedDocument.KindDynamic, "X", Envelope(design)).Opens);
        Assert.False(CantileverDoorRules.AnswerForOpen(7, "CAN-1", RackEmbedDocument.KindCantilever, "X", "{no es json").Opens);
        Assert.Null(CantileverDesignRead.Refused(7, "CAN-1", null, null, "x").Project());
    }

    [Fact]
    public void TheDoorWritesOnlyAResolvedLineOnAReadableLineAndInsertsOnlyViewsTheLineHas()
    {
        var snapshot = Resolved(stations: 3).SnapshotForWrite()!;
        var write = new CantileverDoorWrite(9, new RackCad.Application.Workspace.SessionId(1), "CAN-1", "Cantilever 1", snapshot.Design, snapshot.Line, snapshot.DesignJson);
        Assert.True(CantileverDoorRules.PlanApply(RackEmbedDocument.KindCantilever, snapshot.DesignJson, write).CanWrite);
        Assert.False(CantileverDoorRules.PlanApply(RackEmbedDocument.KindSelective, snapshot.DesignJson, write).CanWrite);
        Assert.False(CantileverDoorRules.PlanApply(RackEmbedDocument.KindCantilever, snapshot.DesignJson, write with { Line = null }).CanWrite);
        Assert.Null(CantileverDoorRules.RefusalOf(write));
        Assert.Equal("Elige al menos una vista para insertar la línea.", CantileverDoorRules.RefusalOf(write with { NewRack = true }));

        var views = new[] { RackViewAddress.Whole(DimensionViewKind.Frontal), RackViewAddress.Station(2), RackViewAddress.Station(5), RackViewAddress.Whole(DimensionViewKind.Planta) };
        Assert.Equal(new[] { views[0], views[1], views[3] }, CantileverDoorRules.DrawableViews(views, snapshot.Line).ToArray());
    }

    // ------------------------------------------------------------------ the commands and the drawing half, on the source

    [Fact]
    public void TheCommandsOfTheDoorReadWithoutUndoCompareTheViewsAndAnswerByTheRuleOfTheDoor()
    {
        var code = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadCantileverDoorCommands.cs");
        Assert.Contains("[CommandMethod(\"RACKCANTILEVERABRIR\", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]", code, StringComparison.Ordinal);
        Assert.Contains("[CommandMethod(\"RACKCANTILEVERAPLICAR\")]", code, StringComparison.Ordinal);
        Assert.Contains("[CommandMethod(\"RACKCANTILEVERINSERTAR\")]", code, StringComparison.Ordinal);
        Assert.Contains("[CommandMethod(\"RACKCANTILEVERNUEVO\")]", code, StringComparison.Ordinal);

        var read = WorkspaceHostSource.BodyOf(code, @"public\s+void\s+ReadCantilever\s*\(");
        Assert.True(read.IndexOf("RackSiblingDesigns.Compare", StringComparison.Ordinal) < read.IndexOf("blocks[0].Embed", StringComparison.Ordinal));
        Assert.Contains("CantileverDoorRules.AnswerForOpen", read, StringComparison.Ordinal);
        Assert.DoesNotContain("ApplyCantileverEdit", read, StringComparison.Ordinal);

        var write = WorkspaceHostSource.BodyOf(code, @"public\s+void\s+WriteCantilever\s*\(");
        Assert.True(write.IndexOf("door?.Started(pending.RequestId)", StringComparison.Ordinal) < write.IndexOf("ApplyCantileverEdit", StringComparison.Ordinal));
        Assert.Contains("RackDoorRules.AnswerForApply(pending.RequestId, pending.RackId, redrawn, erased)", write, StringComparison.Ordinal);
        Assert.Contains("RackPropertiesDoorData.WithRackProperties", write, StringComparison.Ordinal);

        var line = WorkspaceHostSource.BodyOf(code, @"private\s+static\s+bool\s+TryLine\s*\(");
        Assert.True(line.IndexOf("RackSiblingDesigns.Compare", StringComparison.Ordinal) < line.IndexOf("CantileverDoorRules.PlanApply", StringComparison.Ordinal));

        var insert = WorkspaceHostSource.BodyOf(code, @"public\s+void\s+InsertCantileverViews\s*\(");
        Assert.Contains("RackDoorRules.AnswerForInsert", insert, StringComparison.Ordinal);
        var nuevo = WorkspaceHostSource.BodyOf(code, @"public\s+void\s+InsertNewCantilever\s*\(");
        Assert.Contains("RackMenuCommands.DispatchInsertion", nuevo, StringComparison.Ordinal);
        Assert.Contains("RackDoorRules.AnswerForNewRack", nuevo, StringComparison.Ordinal);

        // «Dejar de esperar» drops every request of the door of Cantilever.
        var port = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs");
        Assert.Contains("AbandonCantilever(requestId);", WorkspaceHostSource.BodyOf(port, @"public\s+void\s+Abandon\s*\("), StringComparison.Ordinal);
    }

    [Fact]
    public void TheDrawingHalfOfRackeditarIsExtractedAndRackeditarCallsIt()
    {
        var commands = WorkspaceHostSource.Code("src/RackCad.Plugin/RackCantileverCommands.cs");
        var edit = WorkspaceHostSource.BodyOf(commands, @"internal\s+static\s+void\s+EditCantilever\s*\(");
        Assert.Contains("ApplyCantileverEdit(document, blockId, embed, project, window);", edit, StringComparison.Ordinal);
        Assert.DoesNotContain("RedefineBlock", edit, StringComparison.Ordinal);

        var apply = WorkspaceHostSource.BodyOf(commands, @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+ApplyCantileverEdit\s*\(");
        Assert.Contains("RackViewBatchProducts.Cantilever(", apply, StringComparison.Ordinal);
        Assert.Contains("CantileverViewMaterializer.RedefineBlock", apply, StringComparison.Ordinal);
        Assert.Contains("RackUnsupportedSiblingInsert.TryAuthorize", apply, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Matches(apply, @"return\s+\(Redrawn\(updated\),\s*erasedPhantoms\);").Count);
    }

    // B-1 also in Cantilever (CT-11): a new view of a line of the drawing is prepared from the picked envelope with the identity, the name
    // and the design that the edit writes in the views that exist, as Dinámico.
    [Fact]
    public void TheQueueOfCantileverPreparesTheNewViewFromTheEditedSource()
    {
        var products = WorkspaceHostSource.Code("src/RackCad.Plugin/Views/RackViewBatchProducts.cs");
        var cantilever = WorkspaceHostSource.BodyOf(products, @"internal\s+static\s+[\w<>,\s]+\s+Cantilever\s*\(");
        Assert.Matches(@"var\s+serialized\s*=\s*Project\(RackProject\.ForCantilever\(design\),\s*innerSource\);", cantilever);
        Assert.Matches(@"RackAuthoredComparatorPorts\.Cantilever\(\),\s*comparison,\s*RackInsertedViewSource\.Edited\(source,\s*rackId,\s*rackName,\s*serialized\),\s*design,\s*rackId,\s*rackName,\s*serialized,", cantilever);

        // The rule itself on a Cantilever envelope: the picked view with the identity, the name and the design of the edit.
        var drawn = Resolved().DesignJson;
        var picked = RackEmbedComposer.Compose(null, RackEmbedDocument.KindCantilever, "CAN-1", "Cantilever 1", RackEmbedDocument.ViewLateral, 1, drawn);
        var edited = Resolved(stations: 4).DesignJson;
        var source = RackInsertedViewSource.Edited(picked, "CAN-1", "Nave A", edited);
        Assert.Equal((RackEmbedDocument.KindCantilever, "CAN-1", "Nave A", edited), (source.Kind, source.Id, source.Name, source.Design));
        Assert.Equal((picked.View, picked.Section), (source.View, source.Section));
    }
}
