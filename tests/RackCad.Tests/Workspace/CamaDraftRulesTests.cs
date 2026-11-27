#nullable enable
using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.FlowBed;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.FlowBed;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64, quinta tanda, part cm (prototype «camas», every question accepted): the pure rules of the editor of the cama of the window. The
/// geometry is the one of the domain (FlowBedLateralBuilder, not changed); what is fixed here is what the window shows of it and how it
/// accepts a value: question 3 (a brake every N rollers, kept by the document only when it has a value), question 4 (the pitch), question 6
/// («Restaurar layout…») and question 8 (a Pushback that becomes Dinámica).
/// </summary>
public class CamaDraftRulesTests
{
    private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

    private const string Roller19 = "RODILLO_DE_TUBO_DE_1.9_CALIBRE_14";
    private const string Roller25 = "RODILLO_DE_TUBO_DE_2.5_CALIBRE_14";

    // ------------------------------------------------------------------ the layout: the builder of the domain, as it is

    [Fact]
    public void TheDefaultsOfTheClassicGiveFortyTwoRollersAndOneBrakeAtFiftySixInchesAsTheClassic()
    {
        var catalog = Catalog;
        var bed = CamaDraftRules.Defaults(catalog);
        Assert.Equal((FlowBedType.Dynamic, Roller19, 96.0, 48.0), (bed.BedType, bed.RollerId, bed.LaneDepth, bed.PalletDepth));
        Assert.Null(bed.RollerPitchOverride);
        Assert.Null(bed.BrakeEveryRollers);

        var layout = CamaDraftRules.Layout(bed, catalog)!;
        Assert.Equal(42, layout.Rollers.Count);
        Assert.Equal(new[] { 56.0 }, layout.Brakes.ToArray());
        Assert.Equal(2.0, layout.Pitch);
        Assert.False(layout.ManualPitch);
        Assert.Equal(0.5, layout.TopeOrigin);
        Assert.Equal(7.0, layout.Rollers[0]);

        // The same instances the drawing and the BOM use (the builder of the domain), measured from the troquel of the tope.
        var instances = new FlowBedLateralBuilder().Build(bed, catalog);
        Assert.Equal(instances.Where(i => i.Role == HeaderBlockRole.Roller).Select(i => i.Insertion.X - 0.5).ToArray(), layout.Rollers.ToArray());
        Assert.Equal(instances.Where(i => i.Role == HeaderBlockRole.Brake).Select(i => i.Insertion.X - 0.5).ToArray(), layout.Brakes.ToArray());

        Assert.Equal(new[]
        {
            "Tope: de 0 a 6.5 in.",
            "Rodillos 1 a 23: de 7 a 51 in, cada 2 in.",
            "Freno 1: a 56 in.",
            "Rodillos 24 a 42: de 58 a 94 in, cada 2 in.",
        }, CamaDraftRules.PositionLines(layout).ToArray());
    }

    [Fact]
    public void APushbackHasNoBrakesAndTheBiggerRollerItsOwnPitch()
    {
        var catalog = Catalog;
        var pushback = CamaDraftRules.Defaults(catalog);
        pushback.BedType = FlowBedType.Pushback;
        var layout = CamaDraftRules.Layout(pushback, catalog)!;
        Assert.Equal(45, layout.Rollers.Count);
        Assert.Empty(layout.Brakes);

        var big = CamaDraftRules.Defaults(catalog);
        big.RollerId = Roller25;
        var bigLayout = CamaDraftRules.Layout(big, catalog)!;
        Assert.Equal(3.0, bigLayout.Pitch);
        Assert.Equal((29, 1), (bigLayout.Rollers.Count, bigLayout.Brakes.Count));
    }

    [Fact]
    public void ABrakeEveryNRollersIsTheRuleOfTheBuilderAndAnEmptyOneIsByThePalletAsAlways()
    {
        var catalog = Catalog;
        var bed = CamaDraftRules.Defaults(catalog);
        bed.BrakeEveryRollers = 10;
        var layout = CamaDraftRules.Layout(bed, catalog)!;

        // Ten rollers, a brake 5 troqueles after the last one, the next roller 2 troqueles after the brake.
        Assert.Equal(7.0 + 9 * 2.0, layout.Rollers[9]);
        Assert.Equal(layout.Rollers[9] + 5.0, layout.Brakes[0]);
        Assert.Equal(layout.Brakes[0] + 2.0, layout.Rollers[10]);
        Assert.True(layout.Brakes.Count > 1);
        var instances = new FlowBedLateralBuilder().Build(bed, catalog);
        Assert.Equal(instances.Count(i => i.Role == HeaderBlockRole.Brake), layout.Brakes.Count);
    }

    [Fact]
    public void MoreThanNineLinesOfPositionsAreCutToEightAndSaySo()
    {
        var catalog = Catalog;
        var bed = CamaDraftRules.Defaults(catalog);
        bed.BrakeEveryRollers = 2;
        var lines = CamaDraftRules.PositionLines(CamaDraftRules.Layout(bed, catalog));
        Assert.Equal(9, lines.Count);
        Assert.StartsWith("… y ", lines[8], StringComparison.Ordinal);
        Assert.EndsWith(" líneas más.", lines[8], StringComparison.Ordinal);

        var tooShort = CamaDraftRules.Defaults(catalog);
        tooShort.LaneDepth = 7.0;
        Assert.Equal(new[] { "Tope: de 0 a 6.5 in.", "Sin rodillos: el riel no llega al primer troquel libre después del tope (7.5 in)." },
            CamaDraftRules.PositionLines(CamaDraftRules.Layout(tooShort, catalog)).ToArray());
    }

    // ------------------------------------------------------------------ question 3: the document keeps «Freno cada N» only with a value

    [Fact]
    public void ABedWithoutABrakeEveryNIsWrittenByteForByteAsBeforeAndOneWithItKeepsIt()
    {
        var store = new FlowBedConfigurationStore();
        var bed = new FlowBedConfiguration { BedType = FlowBedType.Dynamic, LaneDepth = 96.0, PalletDepth = 48.0, RollerId = Roller19 };

        // The shape of the document before this part: the same fields, in the same order, and nothing else.
        Assert.Equal(
            "{\"SchemaVersion\":\"1.0\",\"BedType\":\"Dynamic\",\"LaneDepth\":96,\"PalletDepth\":48,\"RollerId\":\"RODILLO_DE_TUBO_DE_1.9_CALIBRE_14\",\"RollerPitchOverride\":null}",
            store.Serialize(bed));

        bed.BrakeEveryRollers = 6;
        var json = store.Serialize(bed);
        Assert.EndsWith(",\"BrakeEveryRollers\":6}", json, StringComparison.Ordinal);
        Assert.Equal(6, store.Deserialize(json)!.BrakeEveryRollers);
        Assert.Equal(6, FlowBedDocument.FromDomain(bed).BrakeEveryRollers);

        // The BOM of the handler reads it from the drawing (CamaKindHandler deserializes the document and builds with it).
        var catalog = Catalog;
        var fromDrawing = store.Deserialize(json)!;
        Assert.Equal(CamaDraftRules.Layout(bed, catalog)!.Brakes.Count,
            new FlowBedLateralBuilder().Build(fromDrawing, catalog).Count(i => i.Role == HeaderBlockRole.Brake));
    }

    // ------------------------------------------------------------------ question 4: the pitch

    [Theory]
    [InlineData(null, 2.0, null)]
    [InlineData(2.0, 2.0, null)]
    [InlineData(4.0, 2.0, null)]
    [InlineData(3.0, 3.0, null)]
    [InlineData(2.5, 2.0, CamaDraftRules.PitchIntegerText)]
    [InlineData(0.0, 2.0, CamaDraftRules.PitchPositiveText)]
    [InlineData(-3.0, 2.0, CamaDraftRules.PitchPositiveText)]
    [InlineData(1.0, 2.0, "El paso no puede ser menor que el rodillo: mínimo 2 in.")]
    [InlineData(2.0, 3.0, "El paso no puede ser menor que el rodillo: mínimo 3 in.")]
    public void ThePitchIsAnIntegerOfInchesNotSmallerThanTheRoller(double? pitch, double minimum, string? expected)
    {
        Assert.Equal(expected, CamaDraftRules.PitchError(pitch, minimum));
    }

    [Fact]
    public void ABedReadWithAPitchTheWindowDoesNotAcceptIsMarkedAndCannotBeBuiltUntilCorrected()
    {
        var catalog = Catalog;
        var bed = CamaDraftRules.Defaults(catalog);
        bed.RollerPitchOverride = 2.5;
        var draft = new CamaDraftEditor(catalog, bed, "Cama 1");

        Assert.Equal(CamaDraftRules.PitchIntegerText, draft.Problems()[CamaDraftRules.PitchKey]);
        Assert.Null(draft.Layout());
        Assert.Null(draft.Bom());

        Assert.Null(draft.SetPitch(3.0));
        Assert.Empty(draft.Problems());
        Assert.Equal(3.0, draft.Layout()!.Pitch);
        Assert.True(draft.Layout()!.ManualPitch);
    }

    [Fact]
    public void TheProblemsOfABedAreOnlyTheOnesThatApplyToItsType()
    {
        var catalog = Catalog;
        var pushback = CamaDraftRules.Defaults(catalog);
        pushback.BedType = FlowBedType.Pushback;
        pushback.PalletDepth = 0.0;
        pushback.BrakeEveryRollers = 0;
        Assert.Empty(CamaDraftRules.Problems(pushback, catalog));

        var dynamic = CamaDraftRules.Defaults(catalog);
        dynamic.PalletDepth = 0.0;
        dynamic.RollerId = "OTRO";
        var problems = CamaDraftRules.Problems(dynamic, catalog);
        Assert.Equal(CamaDraftRules.PalletDepthText, problems[CamaDraftRules.PalletKey]);
        Assert.Equal("El rodillo «OTRO» no está en el catálogo: elige uno de la lista.", problems[CamaDraftRules.RollerKey]);
    }

    // ------------------------------------------------------------------ the gestures of the draft

    [Fact]
    public void ARollerBiggerThanTheManualPitchIsRefusedAndTheDraftDoesNotChange()
    {
        var catalog = Catalog;
        var draft = CamaDraftEditor.CreateNew(catalog);
        Assert.Null(draft.SetPitch(2.0));

        Assert.Equal("El paso (2 in) es menor que este rodillo: cambia antes el paso. El rodillo no cambió.", draft.SetRoller(Roller25));
        Assert.Equal(Roller19, draft.RollerId);

        Assert.Null(draft.SetPitch(null));
        Assert.Null(draft.SetRoller(Roller25));
        Assert.Equal(3.0, draft.MinimumPitch);
        Assert.Equal("Ese rodillo no está en el catálogo.", draft.SetRoller("NADA"));
    }

    [Fact]
    public void APushbackWithNoPalletThatBecomesDinamicaTakesFortyEightInchesAndSaysSo()
    {
        var catalog = Catalog;
        var bed = CamaDraftRules.Defaults(catalog);
        bed.BedType = FlowBedType.Pushback;
        bed.PalletDepth = 0.0; // as the classic writes a Pushback
        var draft = new CamaDraftEditor(catalog, bed, "Cama 2");

        draft.SetBedType(FlowBedType.Dynamic, out var note);
        Assert.Equal(48.0, draft.PalletDepth);
        Assert.Equal("Pasó a Dinámica: la profundidad de tarima estaba vacía y toma 48 in, el valor por defecto del editor clásico.", note);
        Assert.Empty(draft.Problems());

        // A Pushback keeps its pallet (it does not apply), and back to Dinámica it is the same value, with nothing to say.
        draft.SetBedType(FlowBedType.Pushback, out note);
        Assert.Null(note);
        Assert.Equal(48.0, draft.PalletDepth);
        Assert.Null(draft.SetPalletDepth(30.0));
        draft.SetBedType(FlowBedType.Dynamic, out note);
        Assert.Null(note);
        Assert.Equal(30.0, draft.PalletDepth);
    }

    [Fact]
    public void RestaurarLayoutTakesAwayWhatWasWrittenByHandAsMarked()
    {
        var catalog = Catalog;
        var draft = CamaDraftEditor.CreateNew(catalog);
        Assert.Empty(draft.Restore(true, true));

        Assert.Null(draft.SetPitch(4.0));
        Assert.Null(draft.SetBrakeEvery(8));
        Assert.Equal(new[] { "el freno cada N rodillos" }, draft.Restore(false, true).ToArray());
        Assert.Equal(4.0, draft.RollerPitch);
        Assert.Null(draft.BrakeEveryRollers);
        Assert.Equal(new[] { "el paso manual" }, draft.Restore(true, true).ToArray());
        Assert.Null(draft.RollerPitch);
    }

    [Fact]
    public void TheValuesOfTheFieldsAreCheckedBeforeTheDraftTakesThem()
    {
        var draft = CamaDraftEditor.CreateNew(Catalog);
        Assert.Equal(CamaDraftRules.LaneDepthText, draft.SetLaneDepth(0.0));
        Assert.Equal(CamaDraftRules.PalletDepthText, draft.SetPalletDepth(-1.0));
        Assert.Equal(CamaDraftRules.BrakesText, draft.SetBrakeEvery(0));
        Assert.Equal((96.0, 48.0), (draft.LaneDepth, draft.PalletDepth));
        Assert.Null(draft.BrakeEveryRollers);

        Assert.Null(draft.SetLaneDepth(120.0));
        Assert.Equal(120.0, draft.Snapshot().LaneDepth);
        var snapshot = draft.Snapshot();
        snapshot.LaneDepth = 10.0;
        Assert.Equal(120.0, draft.LaneDepth); // a snapshot is a copy
    }

    [Fact]
    public void TheBomOfTheDraftIsTheOneOfItsHandler()
    {
        var catalog = Catalog;
        var draft = CamaDraftEditor.CreateNew(catalog);
        var bom = draft.Bom()!;
        var expected = FlowBedBomBuilder.Build(new FlowBedLateralBuilder().Build(draft.Snapshot(), catalog), catalog);
        Assert.Equal(expected.Lines.Select(l => (l.Category, l.ProfileId, l.Length, l.Quantity)).ToArray(),
            bom.Lines.Select(l => (l.Category, l.ProfileId, l.Length, l.Quantity)).ToArray());
        Assert.Equal(42, bom.Lines.Single(l => l.Category == FlowBedBomBuilder.Roller).Quantity);
        Assert.Equal(1, bom.Lines.Single(l => l.Category == FlowBedBomBuilder.Brake).Quantity);
    }
}
