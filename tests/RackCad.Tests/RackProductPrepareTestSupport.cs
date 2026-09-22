using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Tests
{
    internal static class RackProductPrepareTestSupport
    {
        internal sealed class Authored { }
        internal sealed class Resolved { }
        internal sealed class Payload
        {
            internal Payload(string value) => Value = value;
            internal string Value { get; }
        }

        internal sealed class CountingIdFactory : IRackIdFactory
        {
            private readonly string value;
            internal CountingIdFactory(string value = "rack-new") => this.value = value;
            internal int Calls { get; private set; }
            public string CreateRackId()
            {
                Calls++;
                return value;
            }
        }

        internal sealed class CountingResolve<TInput, TResolved> : IRackResolvePort<TInput, TResolved>
        {
            private readonly Func<TInput, RackResolveResult<TResolved>> result;
            internal CountingResolve(string kind, Func<TInput, RackResolveResult<TResolved>> result)
            {
                Kind = kind;
                this.result = result;
            }

            public string Kind { get; }
            internal int Calls { get; private set; }
            public RackResolveResult<TResolved> Resolve(TInput input)
            {
                Calls++;
                return result(input);
            }
        }

        internal sealed class Requirements : IRackBlockRequirementExtractor<Payload>
        {
            public IReadOnlyList<LibraryBlockRequirement> Extract(Payload payload)
                => new[] { new LibraryBlockRequirement("library-key") };
        }

        internal static RackProductPreparer<Authored, Resolved, Payload> Preparer(
            RackSystemKind systemKind,
            CountingResolve<Authored, Resolved> resolve,
            Action onBuild = null,
            bool failBuild = false)
        {
            Func<Resolved, RackViewAddress, Payload> builder = (_, address) =>
            {
                onBuild?.Invoke();
                if (failBuild) throw new InvalidOperationException("prepare boom");
                return new Payload(address.ToString());
            };

            return new RackProductPreparer<Authored, Resolved, Payload>(
                resolve,
                PreparationPort(systemKind, builder),
                (_, __) => Frame(),
                (_, address) => "Base " + address);
        }

        internal static NewRackViewIntent<Authored> NewIntent(
            RackSystemKind systemKind,
            RackViewAddress address,
            RackViewAvailabilityFacts facts,
            CountingIdFactory factory,
            RackViewProductOperation operation = RackViewProductOperation.CreateFirst,
            NewRackCreationContext<Authored> context = null)
        {
            var creation = context ?? new NewRackCreationContext<Authored>(
                new Authored(), "Rack A", "{\"design\":1}", factory);
            return new NewRackViewIntent<Authored>(
                creation,
                new RackProductViewRequest(operation, systemKind, address, facts));
        }

        internal static AcceptedNewRackViewIntent<Authored> Accepted(NewRackViewIntent<Authored> intent)
        {
            var accepted = RackProductIntentAcceptance.Accept(intent);
            if (!accepted.IsAccepted)
                throw new InvalidOperationException("The test expected an accepted product intent: " + accepted.CauseCode);
            return accepted.Intent;
        }

        internal static string Kind(RackSystemKind systemKind)
        {
            switch (systemKind)
            {
                case RackSystemKind.SelectiveRack: return RackEmbedDocument.KindSelective;
                case RackSystemKind.PalletFlow: return RackEmbedDocument.KindDynamic;
                case RackSystemKind.PushBack: return RackEmbedDocument.KindPushBack;
                case RackSystemKind.Cantilever: return RackEmbedDocument.KindCantilever;
                case RackSystemKind.Selective: return RackEmbedDocument.KindCabecera;
                case RackSystemKind.Cama: return RackEmbedDocument.KindCama;
                default: throw new ArgumentOutOfRangeException(nameof(systemKind));
            }
        }

        internal static RackViewFrameResult Frame() => RackViewFrame.TryCreate(
            RackViewAxisMap.RunHeight,
            RackPhysicalPoint.Zero,
            0,
            10,
            RackFrameEndpointConvention.PhysicalRunAxes,
            RackPhysicalVector.Zero);

        private static IRackViewPreparationPort<Resolved, Payload> PreparationPort(
            RackSystemKind systemKind,
            Func<Resolved, RackViewAddress, Payload> builder)
        {
            Func<RackViewAddress, bool> supports = _ => true;
            var requirements = new Requirements();
            switch (systemKind)
            {
                case RackSystemKind.SelectiveRack: return RackViewPreparationPorts.Selective(builder, supports, requirements);
                case RackSystemKind.PalletFlow: return RackViewPreparationPorts.Dynamic(builder, supports, requirements);
                case RackSystemKind.PushBack: return RackViewPreparationPorts.PushBack(builder, supports, requirements);
                case RackSystemKind.Cantilever: return RackViewPreparationPorts.Cantilever(builder, supports, requirements);
                case RackSystemKind.Selective: return RackViewPreparationPorts.Cabecera(builder, supports, requirements);
                case RackSystemKind.Cama: return RackViewPreparationPorts.Cama(builder, supports, requirements);
                default: throw new ArgumentOutOfRangeException(nameof(systemKind));
            }
        }
    }
}
