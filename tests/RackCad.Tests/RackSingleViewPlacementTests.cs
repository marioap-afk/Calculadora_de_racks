using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.RackProductPrepareTestSupport;

namespace RackCad.Tests
{
    public sealed class RackSingleViewPlacementTests
    {
        [Fact] public void AllRequirementsFoundPlacesOneReferenceWithEmptyReport()
        {
            var materializer = new Materializer();
            var result = Place(new Report(), materializer);
            Assert.Equal(RackSingleViewPlacementStatus.Placed, result.Status);
            Assert.Empty(result.Report.Items);
            Assert.Equal(1, materializer.CreateCalls);
            Assert.Equal(1, materializer.PlaceCalls);
            Assert.Equal(1, materializer.CompleteCalls);
            Assert.Equal("reference", result.Reference);
        }

        [Fact] public void MissingRequiredPiecePlacesAndReports()
        {
            var report = new Report(new RackRequirementReportItem("POST", "post-1", RackRequirementRole.Required, RackRequirementIssue.Missing));
            var materializer = new Materializer();
            var result = Place(report, materializer);
            Assert.Equal(RackSingleViewPlacementStatus.PlacedWithReport, result.Status);
            Assert.Single(result.Report.Items);
            Assert.Equal("POST", result.Report.Items[0].Key);
            Assert.Equal(1, materializer.PlaceCalls);
        }

        [Fact] public void MissingLibraryPlacesAndReportsExplicitCause()
        {
            var report = new Report(new RackRequirementReportItem("POST", "post-1", RackRequirementRole.Required,
                RackRequirementIssue.Missing, RackRequirementCause.LibraryUnavailable));
            var result = Place(report, new Materializer());
            Assert.Equal(RackSingleViewPlacementStatus.PlacedWithReport, result.Status);
            Assert.Equal(RackRequirementCause.LibraryUnavailable, result.Report.Items[0].Cause);
        }

        [Fact] public void BlankRequiredKeyIsNotFabricatedAndDoesNotBlock()
        {
            var report = new Report(new RackRequirementReportItem(null, "post-1", RackRequirementRole.Required, RackRequirementIssue.InvalidKey));
            var result = Place(report, new Materializer());
            Assert.Equal(RackSingleViewPlacementStatus.PlacedWithReport, result.Status);
            Assert.Null(result.Report.Items[0].Key);
            Assert.Equal(RackRequirementIssue.InvalidKey, result.Report.Items[0].Issue);
        }

        [Fact] public void MissingOptionalVisualPlacesAndReportsWarning()
        {
            var report = new Report(new RackRequirementReportItem("PALLET", "pallet-1", RackRequirementRole.OptionalVisual, RackRequirementIssue.Missing));
            var result = Place(report, new Materializer());
            Assert.Equal(RackSingleViewPlacementStatus.PlacedWithReport, result.Status);
            Assert.Equal(RackRequirementRole.OptionalVisual, result.Report.Items[0].Role);
        }

        [Fact] public void ZeroRequirementsDoesNotInvokeRequirementIoAndPlaces()
        {
            var report = new Report();
            var result = Place(report, new Materializer());
            Assert.Equal(RackSingleViewPlacementStatus.Placed, result.Status);
            Assert.Equal(1, report.Calls);
            Assert.Equal(0, report.ImportCalls);
            Assert.Equal(0, report.QueryCalls);
        }

        [Fact] public void CancellationAttemptsCleanupAndLeavesNoReference()
        {
            var materializer = new Materializer { Cancel = true };
            var result = Place(new Report(), materializer);
            Assert.Equal(RackSingleViewPlacementStatus.Cancelled, result.Status);
            Assert.Equal(1, materializer.CleanupCalls);
            Assert.Null(result.Reference);
            Assert.Equal(0, materializer.CompleteCalls);
        }

        [Fact] public void CancellationReportsCleanupFailure()
        {
            var materializer = new Materializer { Cancel = true, CleanupSucceeds = false };
            var result = Place(new Report(), materializer);
            Assert.Equal(RackSingleViewPlacementStatus.CleanupFailed, result.Status);
            Assert.Equal("cleanup failed", result.Diagnostic);
        }

        [Fact] public void PlacementFailureAttemptsCleanup()
        {
            var materializer = new Materializer { FailPlace = true };
            var result = Place(new Report(), materializer);
            Assert.Equal(RackSingleViewPlacementStatus.PlacementFailed, result.Status);
            Assert.Equal(1, materializer.CleanupCalls);
        }

        [Fact] public void ExceptionAfterDefinitionAttemptsCleanup()
        {
            var materializer = new Materializer { ThrowPlace = true };
            var result = Place(new Report(), materializer);
            Assert.Equal(RackSingleViewPlacementStatus.PlacementFailed, result.Status);
            Assert.Equal(1, materializer.CleanupCalls);
        }

        [Fact] public void ProductIdentityEnvelopeAddressAndBaseNameReachTypedMaterializerUnchanged()
        {
            var product = Product();
            var materializer = new Materializer();
            RackSingleViewPlacement.Place(product, new Report(), materializer);
            Assert.Same(product, materializer.Product);
            Assert.Equal("rack-new", materializer.Product.RackId);
            Assert.Equal(product.Envelope, materializer.Product.Envelope);
            Assert.Equal(product.Prepared.Address, materializer.Product.Prepared.Address);
            Assert.Equal("Base Frontal/Fondo(0)", materializer.Product.Prepared.BaseName);
            Assert.Equal("Base Frontal/Fondo(0)_2", materializer.CreatedName);
        }

        [Theory]
        [InlineData(RackSystemKind.SelectiveRack)]
        [InlineData(RackSystemKind.PalletFlow)]
        [InlineData(RackSystemKind.PushBack)]
        [InlineData(RackSystemKind.Cantilever)]
        [InlineData(RackSystemKind.Selective)]
        [InlineData(RackSystemKind.Cama)]
        public void SixSystemSentinelConsumesPreparedProductWithoutReprepare(RackSystemKind kind)
        {
            var product = Product(kind);
            var materializer = new Materializer { Expected = Kind(kind) };
            var result = RackSingleViewPlacement.Place(product, new Report(), materializer);
            Assert.Equal(RackSingleViewPlacementStatus.Placed, result.Status);
            Assert.Same(product.Prepared.Payload, materializer.Product.Prepared.Payload);
        }

        [Fact]
        public void RealHeaderReportPreservesRequiredOptionalBlankAndFileMissingFacts()
        {
            var required = new HeaderBlockInstance { Role = HeaderBlockRole.Post, PieceId = "post-1", BlockName = "POST" };
            var optional = new HeaderBlockInstance { Role = HeaderBlockRole.Pallet, PieceId = "pallet-1", BlockName = "PALLET" };
            var blank = new HeaderBlockInstance { Role = HeaderBlockRole.Beam, PieceId = "beam-1", BlockName = " " };
            var annotation = new HeaderBlockInstance { Role = HeaderBlockRole.Annotation, PieceId = "label", BlockName = null };
            var plan = new HeaderRunPlan(Array.Empty<HeaderGroup>(), new[] { required, optional, blank, annotation });
            var facts = new[]
            {
                new LibraryBlockAvailabilityFact(new LibraryBlockRequirement("POST"), LibraryBlockAvailability.Missing),
                new LibraryBlockAvailabilityFact(new LibraryBlockRequirement("PALLET"), LibraryBlockAvailability.Missing)
            };

            var report = RackSingleViewRequirementReporting.ForHeaderPlan(plan, facts, libraryUnavailable: true);

            Assert.Equal(3, report.Items.Count);
            Assert.Contains(report.Items, i => i.Key == "POST" && i.Piece == "post-1" && i.Role == RackRequirementRole.Required
                && i.Issue == RackRequirementIssue.Missing && i.Cause == RackRequirementCause.LibraryUnavailable);
            Assert.Contains(report.Items, i => i.Key == "PALLET" && i.Role == RackRequirementRole.OptionalVisual);
            Assert.Contains(report.Items, i => i.Key == null && i.Piece == "beam-1" && i.Issue == RackRequirementIssue.InvalidKey);
            Assert.DoesNotContain(report.Items, i => i.Piece == "label");
        }

        private static RackSingleViewPlacementResult<string> Place(Report report, Materializer materializer)
            => RackSingleViewPlacement.Place(Product(), report, materializer);

        private static RackPreparedProductView<Payload> Product(RackSystemKind kind = RackSystemKind.SelectiveRack)
        {
            var address = kind switch
            {
                RackSystemKind.SelectiveRack => RackViewAddress.Fondo(0),
                RackSystemKind.PalletFlow => RackViewAddress.Post(0),
                RackSystemKind.PushBack => RackViewAddress.Post(0),
                RackSystemKind.Cantilever => RackViewAddress.Station(0),
                RackSystemKind.Selective => RackViewAddress.Whole(DimensionViewKind.Lateral),
                RackSystemKind.Cama => RackViewAddress.Whole(DimensionViewKind.Lateral),
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
            var facts = Facts(kind);
            var resolve = new CountingResolve<Authored, Resolved>(Kind(kind), _ => RackResolveResult<Resolved>.Success(Kind(kind), new Resolved()));
            var request = NewIntent(kind, address, facts, new CountingIdFactory());
            return Preparer(kind, resolve).Prepare(Accepted(request)).Product;
        }

        private static RackViewAvailabilityFacts Facts(RackSystemKind kind) => kind switch
        {
            RackSystemKind.SelectiveRack => new SelectiveViewAvailabilityFacts(1, new[] { 0 }),
            RackSystemKind.PalletFlow => new DynamicViewAvailabilityFacts(new[] { 0 }),
            RackSystemKind.PushBack => new PushBackViewAvailabilityFacts(new[] { 0 }, false),
            RackSystemKind.Cantilever => new CantileverViewAvailabilityFacts(1),
            RackSystemKind.Selective => new CabeceraViewAvailabilityFacts(),
            RackSystemKind.Cama => new CamaViewAvailabilityFacts(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        private sealed class Report : IRackSingleViewRequirementEvaluator<Payload>
        {
            private readonly RackRequirementReport report;
            internal Report(params RackRequirementReportItem[] items) => report = new RackRequirementReport(items);
            internal int Calls { get; private set; }
            internal int ImportCalls { get; private set; }
            internal int QueryCalls { get; private set; }
            public RackRequirementReport Evaluate(RackPreparedProductView<Payload> product) { Calls++; return report; }
        }

        private sealed class Materializer : IRackSingleViewMaterializer<Payload, string, string>
        {
            internal string Expected = RackEmbedDocument.KindSelective;
            internal bool Cancel;
            internal bool FailPlace;
            internal bool ThrowPlace;
            internal bool CleanupSucceeds = true;
            internal int CreateCalls, PlaceCalls, CleanupCalls, CompleteCalls;
            internal string CreatedName;
            internal RackPreparedProductView<Payload> Product;
            public string ExpectedKind => Expected;
            public bool CanPlace(RackPreparedProductView<Payload> product, out string diagnostic) { diagnostic = null; return true; }
            public RackSingleViewDefinition<string> Create(RackPreparedProductView<Payload> product)
            {
                Product = product; CreateCalls++; CreatedName = product.Prepared.BaseName + "_2";
                return new RackSingleViewDefinition<string>("definition", CreatedName);
            }
            public RackSingleViewReference<string> Place(RackSingleViewDefinition<string> definition)
            {
                PlaceCalls++;
                if (ThrowPlace) throw new InvalidOperationException("place exploded");
                if (FailPlace) return RackSingleViewReference<string>.Failed("place failed");
                return Cancel ? RackSingleViewReference<string>.Cancelled() : RackSingleViewReference<string>.Placed("reference");
            }
            public RackSingleViewCleanupResult Cleanup(RackSingleViewDefinition<string> definition)
            { CleanupCalls++; return new RackSingleViewCleanupResult(CleanupSucceeds, CleanupSucceeds ? null : "cleanup failed"); }
            public void Complete(RackPreparedProductView<Payload> product, string reference) => CompleteCalls++;
        }
    }
}
