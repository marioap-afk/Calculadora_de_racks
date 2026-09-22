using System;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Preparation
{
    public interface IRackIdFactory
    {
        string CreateRackId();
    }

    public sealed class GuidRackIdFactory : IRackIdFactory
    {
        public string CreateRackId() => Guid.NewGuid().ToString();
    }

    public sealed class RackProductViewRequest
    {
        public RackProductViewRequest(
            RackViewProductOperation operation,
            RackSystemKind systemKind,
            RackViewAddress requestedAddress,
            RackViewAvailabilityFacts availability)
        {
            Operation = operation;
            SystemKind = systemKind;
            RequestedAddress = requestedAddress;
            Availability = availability;
        }

        public RackViewProductOperation Operation { get; }
        public RackSystemKind SystemKind { get; }
        public RackViewAddress RequestedAddress { get; }
        public RackViewAvailabilityFacts Availability { get; }
    }

    public sealed class NewRackCreationContext<TAuthored>
    {
        private readonly object identityLock = new object();
        private readonly IRackIdFactory rackIdFactory;
        private string rackId;

        public NewRackCreationContext(
            TAuthored authored,
            string rackName,
            string serializedDesign,
            IRackIdFactory rackIdFactory)
        {
            Authored = authored;
            RackName = rackName;
            SerializedDesign = serializedDesign;
            this.rackIdFactory = rackIdFactory ?? throw new ArgumentNullException(nameof(rackIdFactory));
        }

        public TAuthored Authored { get; }
        public string RackName { get; }
        public string SerializedDesign { get; }
        public string AcceptedRackId => rackId;

        internal string AcceptRackIdentity()
        {
            lock (identityLock)
            {
                if (rackId == null)
                {
                    var created = rackIdFactory.CreateRackId();
                    if (string.IsNullOrWhiteSpace(created))
                    {
                        throw new InvalidOperationException("The rack identity factory returned an empty identity.");
                    }

                    rackId = created;
                }

                return rackId;
            }
        }
    }

    public sealed class ExistingRackContext<TComparisonInput>
    {
        public ExistingRackContext(RackEmbedDocument sourceEnvelope, TComparisonInput comparisonInput)
        {
            SourceEnvelope = sourceEnvelope;
            ComparisonInput = comparisonInput;
        }

        public RackEmbedDocument SourceEnvelope { get; }
        public TComparisonInput ComparisonInput { get; }
    }

    public sealed class NewRackViewIntent<TAuthored>
    {
        public NewRackViewIntent(NewRackCreationContext<TAuthored> creation, RackProductViewRequest view)
        {
            Creation = creation;
            View = view;
        }

        public NewRackCreationContext<TAuthored> Creation { get; }
        public RackProductViewRequest View { get; }
    }

    public sealed class ExistingRackViewIntent<TComparisonInput>
    {
        public ExistingRackViewIntent(ExistingRackContext<TComparisonInput> existing, RackProductViewRequest view)
        {
            Existing = existing;
            View = view;
        }

        public ExistingRackContext<TComparisonInput> Existing { get; }
        public RackProductViewRequest View { get; }
    }

    public sealed class AcceptedNewRackViewIntent<TAuthored>
    {
        internal AcceptedNewRackViewIntent(
            NewRackCreationContext<TAuthored> creation,
            RackProductViewRequest view,
            string rackId,
            ProjectionPolicyDecision policy)
        {
            Creation = creation;
            View = view;
            RackId = rackId;
            Policy = policy;
        }

        public NewRackCreationContext<TAuthored> Creation { get; }
        public RackProductViewRequest View { get; }
        public string RackId { get; }
        public ProjectionPolicyDecision Policy { get; }
    }

    public sealed class AcceptedExistingRackViewIntent<TComparisonInput>
    {
        internal AcceptedExistingRackViewIntent(
            ExistingRackContext<TComparisonInput> existing,
            RackProductViewRequest view,
            ProjectionPolicyDecision policy)
        {
            Existing = existing;
            View = view;
            Policy = policy;
        }

        public ExistingRackContext<TComparisonInput> Existing { get; }
        public RackProductViewRequest View { get; }
        public ProjectionPolicyDecision Policy { get; }
    }
}
