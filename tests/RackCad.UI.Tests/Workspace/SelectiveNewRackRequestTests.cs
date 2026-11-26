using System;
using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Selectivo E2 (I-64 A-14, line D, third wave, A2): the new Selectivo of the RackCad window reaches the dispatch of «Nuevo» as the SAME
    /// request the classic editor opened from RACKNUEVO hands it (<c>RackSelectiveWindow.InsertBatch_Click</c>, through
    /// <see cref="RackEditorSession{TDesign,TSystem}.RequestInsertViews"/>): RACKSELECTIVONUEVO builds it with
    /// <see cref="SelectiveDraftOutcome.NewRackRequest"/>.
    /// </summary>
    public sealed class SelectiveNewRackRequestTests
    {
        public static IEnumerable<object[]> FirstViews()
        {
            yield return new object[] { RackViewAddress.Fondo(1), RackEmbedDocument.ViewFrontal };
            yield return new object[] { RackViewAddress.Post(2), RackEmbedDocument.ViewLateral };
            yield return new object[] { RackViewAddress.Whole(DimensionViewKind.Planta), RackEmbedDocument.ViewPlanta };
        }

        [Theory]
        [MemberData(nameof(FirstViews))]
        public void TheRequestOfANewSelectivoIsTheOneOfTheClassicEditorOpenedFromNuevo(RackViewAddress first, string legacyView)
        {
            var design = new SelectivePalletDesign();
            var system = new SelectiveRackSystem();
            var views = new List<RackViewAddress> { first, RackViewAddress.Fondo(0), RackViewAddress.Whole(DimensionViewKind.Planta) };
            if (first.Equals(RackViewAddress.Whole(DimensionViewKind.Planta))) views.RemoveAt(2);

            var window = (SelectiveInsertionRequest)SelectiveDraftOutcome.NewRackRequest("nuevo-1", "", design, system, views);

            // The classic editor: «Insertar varias…» of a new Selectivo, with the identity of its session and the name of its box.
            var session = new RackEditorSession<SelectivePalletDesign, SelectiveRackSystem>(new RackCatalog(), newIdFactory: () => "nuevo-1");
            session.Identity.SetName("");
            session.SetModel(design, system);
            session.RequestInsertViews(views, ctx => new SelectiveInsertionRequest(system, design, ctx.Id, ctx.Name,
                RackViewCodec.Encode(RackSystemKind.SelectiveRack, ctx.Views[0]).View));
            var classic = (SelectiveInsertionRequest)session.InsertionRequest;

            Assert.Equal(RackSystemKind.SelectiveRack, window.Kind);
            Assert.Equal(classic.Kind, window.Kind);
            Assert.Equal(classic.Views, window.Views);
            Assert.Equal(views, window.Views);
            Assert.Equal(classic.View, window.View);
            Assert.Equal(legacyView, window.View, ignoreCase: true);
            Assert.Equal(classic.RackId, window.RackId);
            Assert.Equal(classic.RackName, window.RackName);
            Assert.Same(design, window.Design);
            Assert.Same(system, window.System);
        }

        [Fact]
        public void TheRequestKeepsItsOwnCopyOfTheViews_AndThereIsNoneWithoutViews()
        {
            var views = new List<RackViewAddress> { RackViewAddress.Fondo(0), RackViewAddress.Post(0) };
            var request = SelectiveDraftOutcome.NewRackRequest("nuevo-1", "Selectivo 7", new SelectivePalletDesign(), new SelectiveRackSystem(), views);
            views.Add(RackViewAddress.Whole(DimensionViewKind.Planta));

            Assert.Equal(new[] { RackViewAddress.Fondo(0), RackViewAddress.Post(0) }, request.Views);
            Assert.Equal("Selectivo 7", ((SelectiveInsertionRequest)request).RackName);
            Assert.Null(SelectiveDraftOutcome.NewRackRequest("nuevo-1", "", new SelectivePalletDesign(), new SelectiveRackSystem(), null));
            Assert.Null(SelectiveDraftOutcome.NewRackRequest("nuevo-1", "", new SelectivePalletDesign(), new SelectiveRackSystem(), Array.Empty<RackViewAddress>()));
        }
    }
}
