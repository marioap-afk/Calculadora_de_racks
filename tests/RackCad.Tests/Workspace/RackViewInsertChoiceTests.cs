#nullable enable
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

// I-64 A-14, line D, fourth batch, part «ins»: the choice behind the panel «Insertar vista…» (prototype «dialogos», DG-01 to DG-08,
// accepted by the Owner on 2026-11-26), the same for every system (DG-07). Pure: no WPF, no AutoCAD.
public class RackViewInsertChoiceTests
{
    private static readonly RackViewAddress Planta = RackViewAddress.Whole(DimensionViewKind.Planta);

    private static RackViewInsertOption O(string label, RackViewAddress address) => new(label, address);

    // The options of a Selectivo of two fondos and three posts, as SelectiveDoorInsertion.ViewOptions gives them.
    private static RackViewInsertOption[] Selective(int posts = 3) => new[]
        {
            O("Frontal · fondo 1", RackViewAddress.Fondo(0)),
            O("Frontal · fondo 2", RackViewAddress.Fondo(1)),
        }
        .Concat(Enumerable.Range(0, posts).Select(p => O("Lateral · poste " + (p + 1), RackViewAddress.Post(p))))
        .Append(O("Planta", Planta))
        .ToArray();

    [Fact]
    public void TheGroupsAreAlwaysFrontalLateralPlantaAndEachOneInItsNaturalOrder()
    {
        // Push Back today lists the laterals first and backwards (poste n … 1, dolor 6); the panel reads every system the same.
        var pushBack = new RackViewInsertChoice(RackSystemKind.PushBack, new[]
        {
            O("Lateral · poste 3", RackViewAddress.Post(2)),
            O("Lateral · poste 2", RackViewAddress.Post(1)),
            O("Lateral · poste 1", RackViewAddress.Post(0)),
            O("Frontal · posterior B", RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.B)),
            O("Frontal · entrada/salida A", RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A)),
            O("Frontal · posterior A", RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A)),
            O("Frontal · entrada/salida B", RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.B)),
            O("Planta", Planta),
        });
        Assert.Equal(new[] { RackViewInsertGroup.Frontal, RackViewInsertGroup.Lateral, RackViewInsertGroup.Planta }, pushBack.Groups.ToArray());
        Assert.Equal(new[] { "Entrada/salida A", "Posterior A", "Entrada/salida B", "Posterior B", "Poste 1", "Poste 2", "Poste 3", "Planta" },
            pushBack.Options.Select(o => RackViewInsertChoice.ChipOf(o.Address)).ToArray());

        // Cantilever today puts the plan before its stations.
        var cantilever = new RackViewInsertChoice(RackSystemKind.Cantilever, new[]
        {
            O("Frontal", RackViewAddress.Whole(DimensionViewKind.Frontal)),
            O("Planta", Planta),
            O("Lateral · estación 2", RackViewAddress.Station(1)),
            O("Lateral · estación 1", RackViewAddress.Station(0)),
        });
        Assert.Equal(new[] { "Frontal", "Lateral · estación 1", "Lateral · estación 2", "Planta" }, cantilever.Options.Select(o => o.Label).ToArray());

        // The cabecera of the configurator has no frontal; Dinámico has its two ends, salida first.
        var header = new RackViewInsertChoice(RackSystemKind.Selective, new[] { O("Planta", Planta), O("Lateral", RackViewAddress.Whole(DimensionViewKind.Lateral)) });
        Assert.Equal(new[] { RackViewInsertGroup.Lateral, RackViewInsertGroup.Planta }, header.Groups.ToArray());
        var dynamic = new RackViewInsertChoice(RackSystemKind.PalletFlow, new[]
        {
            O("Frontal · entrada", RackViewAddress.FlowEnd(RackFlowEnd.Entrance)),
            O("Frontal · salida", RackViewAddress.FlowEnd(RackFlowEnd.Exit)),
            O("Planta", Planta),
        });
        Assert.Equal(new[] { "Salida", "Entrada", "Planta" }, dynamic.Options.Select(o => RackViewInsertChoice.ChipOf(o.Address)).ToArray());
    }

    [Fact]
    public void TheViewsAreFilteredByTheExposureOfTheSystemAsTheClassicDialogDidAndTheCamaHasNone()
    {
        // A frontal of Dinámico is no view of a Selectivo, and a repeated address is offered once.
        var selective = new RackViewInsertChoice(RackSystemKind.SelectiveRack,
            Selective().Append(O("Frontal · salida", RackViewAddress.FlowEnd(RackFlowEnd.Exit))).Append(O("Planta otra vez", Planta)));
        Assert.Equal(Selective().Select(o => o.Label).ToArray(), selective.Options.Select(o => o.Label).ToArray());

        // DG-07: the Cama is a product of one view: it has no panel.
        var cama = new RackViewInsertChoice(RackSystemKind.Cama, new[] { O("Lateral", RackViewAddress.Whole(DimensionViewKind.Lateral)) });
        Assert.Empty(cama.Options);
        Assert.False(cama.Mark(RackViewAddress.Whole(DimensionViewKind.Lateral)));
    }

    [Fact]
    public void TheOrderIsTheOrderOfMarkingEveryViewIsOnceAndAGestureThatCannotActDoesNothing()
    {
        var choice = new RackViewInsertChoice(RackSystemKind.SelectiveRack, Selective());
        Assert.True(choice.IsEmpty);

        // Gestures on an empty order (dolor 4: «Subir» with nothing chosen asked for index −2).
        Assert.False(choice.MoveUp(0));
        Assert.False(choice.MoveDown(-1));
        Assert.False(choice.Remove(0));
        Assert.Equal(0, choice.InsertCount);

        Assert.True(choice.Toggle(Planta));
        Assert.True(choice.Toggle(RackViewAddress.Post(1)));
        Assert.True(choice.Mark(RackViewAddress.Fondo(1)));
        Assert.False(choice.Mark(Planta)); // DG-03: once per pass
        Assert.False(choice.Mark(RackViewAddress.Post(9))); // not offered
        Assert.Equal(new[] { Planta, RackViewAddress.Post(1), RackViewAddress.Fondo(1) }, choice.ToInsert.ToArray());
        Assert.Equal(new[] { "Planta", "Lateral · poste 2", "Frontal · fondo 2" }, choice.Order.Select(e => e.Label).ToArray());
        Assert.Equal(2, choice.PositionOf(RackViewAddress.Post(1)));
        Assert.Equal(0, choice.PositionOf(RackViewAddress.Post(0)));

        // «Subir» / «Bajar» do nothing at the ends.
        Assert.False(choice.MoveUp(0));
        Assert.False(choice.MoveDown(2));
        Assert.True(choice.MoveUp(2));
        Assert.True(choice.MoveDown(0));
        Assert.Equal(new[] { RackViewAddress.Fondo(1), Planta, RackViewAddress.Post(1) }, choice.ToInsert.ToArray());

        // Unmarked with its box, or «Quitar» of its row.
        Assert.True(choice.Toggle(Planta));
        Assert.True(choice.Remove(0));
        Assert.Equal(new[] { RackViewAddress.Post(1) }, choice.ToInsert.ToArray());
        Assert.False(choice.Remove(5));
    }

    [Fact]
    public void AllThePostsAndNoneKeepTheOrderOfTheOtherViews()
    {
        var choice = new RackViewInsertChoice(RackSystemKind.SelectiveRack, Selective(posts: 4));
        choice.Mark(Planta);
        choice.Mark(RackViewAddress.Post(2));
        Assert.False(choice.AllCutsMarked);
        Assert.True(choice.AnyCutMarked);

        Assert.True(choice.MarkAllCuts());
        Assert.Equal(new[] { Planta, RackViewAddress.Post(2), RackViewAddress.Post(0), RackViewAddress.Post(1), RackViewAddress.Post(3) },
            choice.ToInsert.ToArray());
        Assert.True(choice.AllCutsMarked);
        Assert.False(choice.MarkAllCuts());

        choice.Mark(RackViewAddress.Fondo(0));
        Assert.True(choice.UnmarkAllCuts());
        Assert.Equal(new[] { Planta, RackViewAddress.Fondo(0) }, choice.ToInsert.ToArray());
        Assert.False(choice.AnyCutMarked);
        Assert.False(choice.UnmarkAllCuts());
    }

    [Fact]
    public void ANewRackStartsWithTheFirstViewOfEachGroup()
    {
        // DG-02, as the approved draft of Selectivo: «Frontal · fondo 1», «Lateral · poste 1» and «Planta».
        var selective = new RackViewInsertChoice(RackSystemKind.SelectiveRack, Selective());
        selective.SeedFirstOfEachGroup();
        Assert.Equal(new[] { RackViewAddress.Fondo(0), RackViewAddress.Post(0), Planta }, selective.ToInsert.ToArray());

        // The same rule for Dinámico: «Frontal · salida», the first lateral with a cut, «Planta».
        var dynamic = new RackViewInsertChoice(RackSystemKind.PalletFlow, new[]
        {
            O("Frontal · salida", RackViewAddress.FlowEnd(RackFlowEnd.Exit)),
            O("Frontal · entrada", RackViewAddress.FlowEnd(RackFlowEnd.Entrance)),
            O("Lateral · poste 2", RackViewAddress.Post(1)),
            O("Lateral · poste 3", RackViewAddress.Post(2)),
            O("Planta", Planta),
        });
        dynamic.SeedFirstOfEachGroup();
        Assert.Equal(new[] { RackViewAddress.FlowEnd(RackFlowEnd.Exit), RackViewAddress.Post(1), Planta }, dynamic.ToInsert.ToArray());
    }

    [Fact]
    public void ThePanelFollowsTheDraftAViewItLostStaysMarkedAndIsNotInserted()
    {
        // DG-08: the draft had four posts; the user marked «Poste 4» and then lowered the fronts: the post is gone.
        var choice = new RackViewInsertChoice(RackSystemKind.SelectiveRack, Selective(posts: 4));
        choice.Mark(RackViewAddress.Post(3));
        choice.Mark(Planta);
        choice.Follow(Selective(posts: 3));

        var gone = choice.Order[0];
        Assert.True(gone.IsGone);
        Assert.Equal("Lateral · poste 4", gone.Label); // the label it had
        Assert.True(choice.HasGone);
        Assert.Equal(new[] { Planta }, choice.ToInsert.ToArray());
        Assert.Equal(1, choice.InsertCount);
        Assert.False(choice.Mark(RackViewAddress.Post(3)));

        // Back as it was: the view is inserted again, in its place.
        choice.Follow(Selective(posts: 4));
        Assert.False(choice.HasGone);
        Assert.Equal(new[] { RackViewAddress.Post(3), Planta }, choice.ToInsert.ToArray());

        // A gone view leaves the order with «Quitar», or with «Ninguno» if it is a cut.
        choice.Follow(Selective(posts: 3));
        Assert.True(choice.UnmarkAllCuts());
        Assert.False(choice.HasGone);
    }

    [Fact]
    public void TheShortNameOfEveryViewInItsGroup()
    {
        Assert.Equal("Fondo 2", RackViewInsertChoice.ChipOf(RackViewAddress.Fondo(1)));
        Assert.Equal("Poste 3", RackViewInsertChoice.ChipOf(RackViewAddress.Post(2)));
        Assert.Equal("Estación 1", RackViewInsertChoice.ChipOf(RackViewAddress.Station(0)));
        Assert.Equal("Salida", RackViewInsertChoice.ChipOf(RackViewAddress.FlowEnd(RackFlowEnd.Exit)));
        Assert.Equal("Entrada", RackViewInsertChoice.ChipOf(RackViewAddress.FlowEnd(RackFlowEnd.Entrance)));
        Assert.Equal("Entrada/salida A", RackViewInsertChoice.ChipOf(RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A)));
        Assert.Equal("Posterior B", RackViewInsertChoice.ChipOf(RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.B)));
        Assert.Equal("Frontal", RackViewInsertChoice.ChipOf(RackViewAddress.Whole(DimensionViewKind.Frontal)));
        Assert.Equal("Lateral", RackViewInsertChoice.ChipOf(RackViewAddress.Whole(DimensionViewKind.Lateral)));
        Assert.Equal("Planta", RackViewInsertChoice.ChipOf(Planta));
        Assert.True(RackViewInsertChoice.IsIndexedCut(RackViewAddress.Station(2)));
        Assert.False(RackViewInsertChoice.IsIndexedCut(RackViewAddress.Whole(DimensionViewKind.Lateral)));
    }
}
