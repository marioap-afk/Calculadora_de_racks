using System;
using System.Collections.Generic;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class RackProductAuthoredGateTests
    {
        [Fact]
        public void ExistingSelectiveSingleProceedsAndPreservesRackIdentity()
        {
            var authored = Authored("Rack A");
            var resolve = Resolve();
            var result = Preparer(resolve).PrepareExisting(
                Accepted(Intent(new[] { ProjectVariableScanEntry.Selective("D1", "rack-existing", authored) })),
                RackAuthoredComparatorPorts.Selective());

            Assert.True(result.IsSuccess);
            Assert.Equal("rack-existing", result.Product.RackId);
            Assert.Equal(RackProductSourceKind.ExistingRack, result.Product.SourceKind);
            Assert.Equal(1, resolve.Calls);
        }

        [Fact]
        public void ExistingSelectiveDivergentAndUnreadableFailBeforeResolve()
        {
            var resolve = Resolve();
            var divergent = Preparer(resolve).PrepareExisting(
                Accepted(Intent(new[]
                {
                    ProjectVariableScanEntry.Selective("D1", "rack-existing", Authored("A")),
                    ProjectVariableScanEntry.Selective("D2", "rack-existing", Authored("B"))
                })), RackAuthoredComparatorPorts.Selective());
            var unreadable = Preparer(resolve).PrepareExisting(
                Accepted(Intent(new[] { ProjectVariableScanEntry.SelectiveUnreadableDesign("D1", "rack-existing") })),
                RackAuthoredComparatorPorts.Selective());

            Assert.Equal(RackProductPrepareFailure.AuthoredDivergent, divergent.Failure);
            Assert.Equal(RackProductPrepareFailure.AuthoredUnreadable, unreadable.Failure);
            Assert.Equal(0, resolve.Calls);
        }

        [Fact]
        public void NewRackUsesAcceptedAuthoredDirectlyWithoutSisterComparator()
        {
            var ids = new RackProductPrepareTestSupport.CountingIdFactory();
            var resolve = Resolve();
            var authored = Authored("New");
            var context = new NewRackCreationContext<SelectivePalletDesignDocument>(
                authored, "New", "{}", ids);
            var intent = new NewRackViewIntent<SelectivePalletDesignDocument>(context,
                new RackProductViewRequest(
                    RackViewProductOperation.CreateFirst,
                    RackSystemKind.SelectiveRack,
                    RackViewAddress.Fondo(0),
                    new SelectiveViewAvailabilityFacts(1, Array.Empty<int>())));

            var result = Preparer(resolve).Prepare(RackProductIntentAcceptance.Accept(intent).Intent);

            Assert.True(result.IsSuccess);
            Assert.Same(authored, resolve.LastInput);
            Assert.Equal(1, resolve.Calls);
        }

        [Fact]
        public void ExistingRackEnvelopePreservesTheChosenSourceMetadata()
        {
            using var properties = JsonDocument.Parse("{\"owner\":\"A\"}");
            using var future = JsonDocument.Parse("42");
            var source = new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindSelective,
                Id = "rack-existing",
                Name = "Rack existing",
                Design = "{\"authored\":1}",
                CustomProperties = properties.RootElement.Clone(),
                ExtensionData = new Dictionary<string, JsonElement>
                {
                    ["FutureField"] = future.RootElement.Clone()
                }
            };
            var authored = Authored("Rack existing");

            var result = Preparer(Resolve()).PrepareExisting(
                Accepted(Intent(new[] { ProjectVariableScanEntry.Selective("D1", "rack-existing", authored) }, source)),
                RackAuthoredComparatorPorts.Selective());

            Assert.True(result.IsSuccess);
            Assert.Equal("A", result.Product.Envelope.CustomProperties.Value.GetProperty("owner").GetString());
            Assert.Equal(42, result.Product.Envelope.ExtensionData["FutureField"].GetInt32());
            Assert.Equal("{\"authored\":1}", result.Product.Envelope.Design);
        }

        [Fact]
        public void ExistingNonSelectiveKindWithoutDemonstratedComparatorFailsClosedBeforeResolve()
        {
            var resolve = new RackProductPrepareTestSupport.CountingResolve<
                RackProductPrepareTestSupport.Authored,
                RackProductPrepareTestSupport.Resolved>(RackEmbedDocument.KindDynamic,
                _ => RackResolveResult<RackProductPrepareTestSupport.Resolved>.Success(
                    RackEmbedDocument.KindDynamic, new RackProductPrepareTestSupport.Resolved()));
            var preparer = RackProductPrepareTestSupport.Preparer(RackSystemKind.PalletFlow, resolve);
            var source = new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindDynamic,
                Id = "dynamic-existing",
                Name = "Dynamic",
                Design = "{}"
            };
            var intent = new ExistingRackViewIntent<object>(
                new ExistingRackContext<object>(source, new object()),
                new RackProductViewRequest(
                    RackViewProductOperation.InsertSibling,
                    RackSystemKind.PalletFlow,
                    RackViewAddress.Post(0),
                    new DynamicViewAvailabilityFacts(new[] { 0 })));

            var result = preparer.PrepareExisting(
                Accepted(intent),
                RackAuthoredComparatorPorts.Dynamic<object, RackProductPrepareTestSupport.Authored>());

            Assert.Equal(RackProductPrepareFailure.AuthoredUnreadable, result.Failure);
            Assert.Equal(0, resolve.Calls);
        }

        private static ExistingRackViewIntent<SelectiveAuthoredComparisonInput> Intent(
            ProjectVariableScanEntry[] siblings,
            RackEmbedDocument envelope = null)
        {
            envelope ??= new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindSelective,
                Id = "rack-existing",
                Name = "Rack existing",
                Design = "{}"
            };
            return new ExistingRackViewIntent<SelectiveAuthoredComparisonInput>(
                new ExistingRackContext<SelectiveAuthoredComparisonInput>(
                    envelope,
                    new SelectiveAuthoredComparisonInput("rack-existing", siblings)),
                new RackProductViewRequest(
                    RackViewProductOperation.InsertSibling,
                    RackSystemKind.SelectiveRack,
                    RackViewAddress.Fondo(0),
                    new SelectiveViewAvailabilityFacts(1, Array.Empty<int>())));
        }

        private static SelectivePalletDesignDocument Authored(string name)
            => new SelectivePalletDesignDocument { Id = "rack-existing", Name = name, PostId = "P" };

        private static AcceptedExistingRackViewIntent<T> Accepted<T>(ExistingRackViewIntent<T> intent)
        {
            var accepted = RackProductIntentAcceptance.Accept(intent);
            Assert.True(accepted.IsAccepted);
            return accepted.Intent;
        }

        private static SelectiveResolve Resolve() => new SelectiveResolve();

        private static RackProductPreparer<SelectivePalletDesignDocument, RackProductPrepareTestSupport.Resolved,
            RackProductPrepareTestSupport.Payload> Preparer(SelectiveResolve resolve)
        {
            Func<RackProductPrepareTestSupport.Resolved, RackViewAddress,
                RackProductPrepareTestSupport.Payload> builder = (_, address) =>
                    new RackProductPrepareTestSupport.Payload(address.ToString());
            return new RackProductPreparer<SelectivePalletDesignDocument, RackProductPrepareTestSupport.Resolved,
                RackProductPrepareTestSupport.Payload>(
                resolve,
                RackViewPreparationPorts.Selective(builder, _ => true,
                    new RackProductPrepareTestSupport.Requirements()),
                (_, __) => RackProductPrepareTestSupport.Frame(),
                (_, __) => "Base");
        }

        private sealed class SelectiveResolve
            : IRackResolvePort<SelectivePalletDesignDocument, RackProductPrepareTestSupport.Resolved>
        {
            public string Kind => RackEmbedDocument.KindSelective;
            internal int Calls { get; private set; }
            internal SelectivePalletDesignDocument LastInput { get; private set; }
            public RackResolveResult<RackProductPrepareTestSupport.Resolved> Resolve(
                SelectivePalletDesignDocument input)
            {
                Calls++;
                LastInput = input;
                return RackResolveResult<RackProductPrepareTestSupport.Resolved>.Success(
                    Kind, new RackProductPrepareTestSupport.Resolved());
            }
        }
    }
}
