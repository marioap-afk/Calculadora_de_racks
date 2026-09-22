using System;
using System.Linq;
using RackCad.Application.Views.Preparation;
using Xunit;

namespace RackCad.Tests
{
    public sealed class NoProductPlannerGuardTests
    {
        [Fact]
        public void ProductPreparePublicContractHasNoObjectDynamicJsonReflectionOrAutoCad()
        {
            var types = typeof(RackProductPreparer<,,>).Assembly.GetExportedTypes()
                .Where(type => type.Namespace == "RackCad.Application.Views.Preparation")
                .ToArray();

            Assert.NotEmpty(types);
            Assert.DoesNotContain(types.SelectMany(type => type.GetProperties()),
                property => property.PropertyType == typeof(object));
            Assert.DoesNotContain(types.SelectMany(type => type.GetMethods()),
                method => method.Name.Contains("Json", StringComparison.OrdinalIgnoreCase)
                    || method.Name.Contains("Reflect", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(types.SelectMany(type => type.GetMethods())
                    .SelectMany(method => method.GetParameters())
                    .Select(parameter => parameter.ParameterType),
                TypeGraphContainsAutoCad);
        }

        [Fact]
        public void ProductPrepareDelegatesToSharedAuthoritiesAndContainsNoMaterializationTypes()
        {
            var source = I55ProductCharacterizationTestSupport.Code(
                "src", "RackCad.Application", "Views", "Preparation", "RackProductPrepare.cs");
            var acceptance = I55ProductCharacterizationTestSupport.Code(
                "src", "RackCad.Application", "Views", "Preparation", "RackProductIntentAcceptance.cs");
            var envelope = I55ProductCharacterizationTestSupport.Code(
                "src", "RackCad.Application", "Views", "Preparation", "RackViewEnvelopeComposition.cs");

            Assert.Contains("ProjectionAvailabilityPolicy.EvaluateNewView", acceptance);
            Assert.Contains("resolve.Resolve", source);
            Assert.Contains("prepare.Prepare", source);
            Assert.Contains("RackEmbedComposer.Compose", envelope);
            Assert.DoesNotContain("new LibraryBlockRequirement", source);
            Assert.DoesNotContain("SanitizeBlockName", source);
            Assert.DoesNotContain("BlockTableRecord", source + acceptance + envelope);
            Assert.DoesNotContain("BlockReference", source + acceptance + envelope);
            Assert.DoesNotContain("Autodesk.AutoCAD", source + acceptance + envelope);
            Assert.DoesNotContain("Guid.NewGuid", source);
        }

        [Fact]
        public void SharedFoundationDoesNotReferenceProductPreparation()
        {
            foreach (var file in new[]
            {
                "RackResolveContract.cs",
                "RackViewPreparation.cs",
                "RackViewBaseName.cs",
                "LibraryBlockRequirements.cs",
                "RackAuthoredComparator.cs"
            })
            {
                var source = I55ProductCharacterizationTestSupport.Code(
                    "src", "RackCad.Application", "Systems", "Shared", file);
                Assert.DoesNotContain("RackCad.Application.Views.Preparation", source);
                Assert.DoesNotContain("RackProductPreparer", source);
            }
        }

        private static bool TypeGraphContainsAutoCad(Type type)
        {
            if (type.FullName != null && type.FullName.StartsWith("Autodesk.AutoCAD.", StringComparison.Ordinal))
                return true;
            if (type.IsArray || type.IsByRef || type.IsPointer)
                return TypeGraphContainsAutoCad(type.GetElementType());
            return type.IsGenericType && type.GetGenericArguments().Any(TypeGraphContainsAutoCad);
        }
    }
}
