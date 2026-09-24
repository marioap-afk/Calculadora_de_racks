using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Views;
using Xunit;

namespace RackCad.UI.Tests
{
    public sealed class RackViewBatchDialogTests
    {
        [Fact]
        public void Presenter_preserves_typed_variants_and_user_order()
        {
            var front = new RackViewBatchOption("Frontal", RackViewAddress.Fondo(2));
            var lateral = new RackViewBatchOption("Lateral", RackViewAddress.Post(4));
            var plant = new RackViewBatchOption("Planta", RackViewAddress.Whole(DimensionViewKind.Planta));
            var presenter = new RackViewBatchDialogPresenter(RackSystemKind.SelectiveRack,
                new[] { front, lateral, plant });

            presenter.Add(plant);
            presenter.Add(front);
            presenter.Add(lateral);
            presenter.MoveUp(2);

            Assert.Equal(new[] { plant.Address, lateral.Address, front.Address }, presenter.Result);
            Assert.Equal(RackViewVariantKind.Post, presenter.Result[1].Variant.Kind);
            Assert.Equal(4, presenter.Result[1].Variant.Index);
        }

        [Fact]
        public void Flow_bed_has_no_batch_options()
        {
            var presenter = new RackViewBatchDialogPresenter(RackSystemKind.Cama,
                new[] { new RackViewBatchOption("Lateral", RackViewAddress.Whole(DimensionViewKind.Lateral)) });

            Assert.Empty(presenter.Available);
        }

        [Fact]
        public void Duplicate_requests_remain_ordered_and_are_not_silently_deduplicated()
        {
            var option = new RackViewBatchOption("Planta", RackViewAddress.Whole(DimensionViewKind.Planta));
            var presenter = new RackViewBatchDialogPresenter(RackSystemKind.SelectiveRack, new[] { option });
            presenter.Add(option);
            presenter.Add(option);

            Assert.Equal(2, presenter.Result.Count);
            Assert.True(presenter.Result.All(item => item.Equals(option.Address)));
        }
    }
}
