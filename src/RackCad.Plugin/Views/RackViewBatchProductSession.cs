using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Batch;
using RackCad.Application.Views.Placement;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Plugin.Views
{
    /// <summary>Composes every prepared ID18 view through the existing Foundation and G8 authorities.</summary>
    internal sealed class RackViewBatchProductSession<TAuthored, TResolved, TPayload>
    {
        private readonly RackSystemKind systemKind;
        private readonly RackViewAvailabilityFacts availability;
        private readonly RackProductPreparer<TAuthored, TResolved, TPayload> preparer;
        private readonly IRackAuthoredComparatorPort<RackAuthoredInput, TAuthored> comparator;
        private readonly ExistingRackContext<RackAuthoredInput> existing;
        private readonly NewRackCreationContext<TAuthored> creation;
        private readonly Func<RackPreparedProductView<TPayload>, RackSingleViewPlacementResult<ObjectId>> place;
        private readonly Dictionary<RackViewAddress, RackViewBatchPreparation<RackPreparedProductView<TPayload>>> prepared
            = new Dictionary<RackViewAddress, RackViewBatchPreparation<RackPreparedProductView<TPayload>>>();

        internal RackViewBatchProductSession(
            RackSystemKind systemKind,
            RackViewAvailabilityFacts availability,
            RackProductPreparer<TAuthored, TResolved, TPayload> preparer,
            IRackAuthoredComparatorPort<RackAuthoredInput, TAuthored> comparator,
            RackAuthoredInput comparisonInput,
            RackEmbedDocument source,
            TAuthored authored,
            string rackId,
            string rackName,
            string serializedDesign,
            Func<RackPreparedProductView<TPayload>, RackSingleViewPlacementResult<ObjectId>> place)
        {
            this.systemKind = systemKind;
            this.availability = availability ?? throw new ArgumentNullException(nameof(availability));
            this.preparer = preparer ?? throw new ArgumentNullException(nameof(preparer));
            this.place = place ?? throw new ArgumentNullException(nameof(place));
            if (source == null)
                creation = new NewRackCreationContext<TAuthored>(
                    authored, rackName, serializedDesign, new FixedRackIdFactory(rackId));
            else
            {
                this.comparator = new CachedComparator<TAuthored>(
                    comparator ?? throw new ArgumentNullException(nameof(comparator)));
                existing = new ExistingRackContext<RackAuthoredInput>(source,
                    comparisonInput ?? throw new ArgumentNullException(nameof(comparisonInput)));
            }
        }

        internal RackViewBatchPreparation<RackPreparedProductView<TPayload>> Prepare(RackViewBatchItem item)
        {
            if (prepared.TryGetValue(item.Address, out var cached)) return cached;
            var request = new RackProductViewRequest(
                creation == null ? RackViewProductOperation.InsertSibling : RackViewProductOperation.CreateFirst,
                systemKind, item.Address, availability);
            RackProductPrepareResult<TPayload> result;
            if (creation != null)
            {
                var accepted = RackProductIntentAcceptance.Accept(new NewRackViewIntent<TAuthored>(creation, request));
                if (!accepted.IsAccepted) return Store(item.Address, Failed(accepted.CauseCode, accepted.Diagnostic));
                result = preparer.Prepare(accepted.Intent);
            }
            else
            {
                var accepted = RackProductIntentAcceptance.Accept(
                    new ExistingRackViewIntent<RackAuthoredInput>(existing, request));
                if (!accepted.IsAccepted) return Store(item.Address, Failed(accepted.CauseCode, accepted.Diagnostic));
                result = preparer.PrepareExisting(accepted.Intent, comparator);
            }
            return Store(item.Address, result.IsSuccess
                ? RackViewBatchPreparation<RackPreparedProductView<TPayload>>.Success(result.Product)
                : Failed(result.CauseCode, result.Diagnostic));
        }

        internal bool PrepareAll(RackViewBatchRequest request, out string diagnostic)
        {
            foreach (var item in request.Views)
            {
                var result = Prepare(item);
                if (!result.Succeeded) { diagnostic = result.Diagnostic; return false; }
            }
            diagnostic = null;
            return true;
        }

        internal RackSingleViewPlacementResult<ObjectId> Place(RackPreparedProductView<TPayload> product)
            => place(product);

        internal static RackViewBatchRequest Request(RackProductSourceKind sourceKind, RackSystemKind systemKind,
            string rackId, System.Collections.Generic.IReadOnlyList<RackViewAddress> views)
        {
            var items = new RackViewBatchItem[views.Count];
            for (var index = 0; index < views.Count; index++)
                items[index] = new RackViewBatchItem(rackId, views[index]);
            return new RackViewBatchRequest(sourceKind, systemKind, items);
        }

        private static RackViewBatchPreparation<RackPreparedProductView<TPayload>> Failed(string code, string diagnostic)
            => RackViewBatchPreparation<RackPreparedProductView<TPayload>>.Failed(
                code + (string.IsNullOrWhiteSpace(diagnostic) ? string.Empty : ": " + diagnostic));

        private RackViewBatchPreparation<RackPreparedProductView<TPayload>> Store(
            RackViewAddress address, RackViewBatchPreparation<RackPreparedProductView<TPayload>> result)
        {
            prepared[address] = result;
            return result;
        }

        private sealed class FixedRackIdFactory : IRackIdFactory
        {
            private readonly string rackId;
            internal FixedRackIdFactory(string rackId) => this.rackId = rackId;
            public string CreateRackId() => rackId;
        }

        private sealed class CachedComparator<T> : IRackAuthoredComparatorPort<RackAuthoredInput, T>
        {
            private readonly IRackAuthoredComparatorPort<RackAuthoredInput, T> inner;
            private RackAuthoredComparisonResult<T> result;
            internal CachedComparator(IRackAuthoredComparatorPort<RackAuthoredInput, T> inner) => this.inner = inner;
            public string Kind => inner.Kind;
            public RackAuthoredComparisonResult<T> Compare(RackAuthoredInput input)
                => result ?? (result = inner.Compare(input));
        }
    }
}
