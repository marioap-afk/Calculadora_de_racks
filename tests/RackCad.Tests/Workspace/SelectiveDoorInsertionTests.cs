using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests.Workspace
{
    /// <summary>
    /// Selectivo E2, part A (I-64 A-14, line D): the rules of «Insertar vista…» and of the new Selectivo of the RackCad window that need
    /// no AutoCAD (<see cref="SelectiveDoorInsertion"/>), and the write of the door that carries them (<see cref="SelectiveDoorWrite"/>).
    /// </summary>
    public sealed class SelectiveDoorInsertionTests
    {
        private static SelectiveDoorWrite Write(SelectiveRackSystem system, IReadOnlyList<RackViewAddress> views = null, bool newRack = false, string rackId = "rack-1")
            => new SelectiveDoorWrite(3, default, rackId, "Selectivo A", new SelectivePalletDesign(), system, null, views, newRack);

        [Fact]
        public void ViewOptions_ListAFrontalPerFondo_ThenTheCuts_ThenThePlan_AsTheClassicInsertarVarias()
        {
            var catalog = Catalog;
            var system = Selective(DimensionDetail.Standard, twoFondos: true, catalog);
            var options = SelectiveDoorInsertion.ViewOptions(system, catalog);

            var fondos = SelectiveDepthLayout.Count(system);
            var cuts = new SelectiveLateralBuilder().Cortes(system, catalog);
            var expected = Enumerable.Range(0, fondos).Select(i => ("Frontal · fondo " + (i + 1), RackViewAddress.Fondo(i)))
                .Concat(cuts.Select(cut => ("Lateral · poste " + (cut.PostIndex + 1), RackViewAddress.Post(cut.PostIndex))))
                .Append(("Planta", RackViewAddress.Whole(DimensionViewKind.Planta)))
                .ToList();

            Assert.Equal(2, fondos);
            Assert.NotEmpty(cuts);
            Assert.Equal(expected, options.Select(option => (option.Label, option.Address)).ToList());
        }

        [Fact]
        public void ViewOptions_WithoutASystem_OfferOneFrontalAndThePlan()
        {
            var options = SelectiveDoorInsertion.ViewOptions(null, null);
            Assert.Equal(new[] { "Frontal · fondo 1", "Planta" }, options.Select(option => option.Label).ToArray());
        }

        [Fact]
        public void DrawableViews_KeepTheOrder_DropRepeats_AndViewsTheRackCannotDraw()
        {
            var catalog = Catalog;
            var system = Selective(DimensionDetail.Standard, twoFondos: false, catalog);
            var post = new SelectiveLateralBuilder().Cortes(system, catalog).First().PostIndex;
            var chosen = new[]
            {
                RackViewAddress.Whole(DimensionViewKind.Planta),
                RackViewAddress.Fondo(0),
                RackViewAddress.Fondo(1),          // the rack has one fondo
                RackViewAddress.Post(post),
                RackViewAddress.Post(999),         // no cut there
                RackViewAddress.Fondo(0),          // repeated
            };

            var drawable = SelectiveDoorInsertion.DrawableViews(chosen, system, catalog);

            Assert.Equal(new[] { RackViewAddress.Whole(DimensionViewKind.Planta), RackViewAddress.Fondo(0), RackViewAddress.Post(post) }, drawable.ToArray());
            Assert.Empty(SelectiveDoorInsertion.DrawableViews(null, system, catalog));
        }

        [Fact]
        public void Write_WithoutViews_IsTheUpdateOfE1_AndPasses()
        {
            var write = new SelectiveDoorWrite(3, default, "rack-1", "Selectivo A", new SelectivePalletDesign(), null, null);
            Assert.False(write.Inserts);
            Assert.False(write.NewRack);
            Assert.Null(SelectiveDoorInsertion.RefusalOf(write));
        }

        [Fact]
        public void Write_WithViews_Inserts_AndANewRackNeedsViewsAndAnIdentity()
        {
            var views = new[] { RackViewAddress.Fondo(0) };
            Assert.True(Write(null, views).Inserts);
            Assert.Null(SelectiveDoorInsertion.RefusalOf(Write(null, views)));
            Assert.Null(SelectiveDoorInsertion.RefusalOf(Write(null, views, newRack: true)));
            Assert.NotNull(SelectiveDoorInsertion.RefusalOf(Write(null, Array.Empty<RackViewAddress>(), newRack: true)));
            Assert.NotNull(SelectiveDoorInsertion.RefusalOf(Write(null, views, newRack: true, rackId: " ")));
            Assert.NotNull(SelectiveDoorInsertion.RefusalOf(null));
        }

        [Theory]
        [InlineData(2, 0, 1)]
        [InlineData(2, 1, 0)]
        [InlineData(0, 0, 2)]
        [InlineData(0, 0, 0)]
        public void AnswerForInsert_IsTheAnswerOfDinamico(int redrawn, int erased, int inserted)
        {
            Assert.Equal(RackDoorRules.AnswerForInsert(5, "r", redrawn, erased, inserted), SelectiveDoorInsertion.AnswerForInsert(5, "r", redrawn, erased, inserted));
        }

        [Theory]
        [InlineData(3, "Selectivo 4")]
        [InlineData(1, null)]
        [InlineData(0, "Selectivo 4")]
        public void AnswerForNewRack_IsTheAnswerOfDinamico_WithTheAutomaticName(int placed, string name)
        {
            var answer = SelectiveDoorInsertion.AnswerForNewRack(5, "r", placed, name);
            Assert.Equal(RackDoorRules.AnswerForNewRack(5, "r", placed, name), answer);
            Assert.Equal(placed > 0, answer.Written);
        }
    }
}
