using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;
using Xunit.Sdk;

namespace RackCad.Tests
{
    /// <summary>
    /// Compile-safe projection for the G9 RED contract. It consumes the real G7 evaluation, G8 persisted forms and
    /// the inherited mutation seams. Missing G9 concepts fail by semantic name; this helper supplies no mutation,
    /// graph, resolver, repair or concurrency behavior of its own.
    /// </summary>
    internal static class G9ContractTestSupport
    {
        internal const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        internal const string RackB = "5d9e2a10-77b4-4c31-8e06-2f9a4b7c1d38";
        internal const string IdA = G8ContractTestSupport.IdA;
        internal const string IdB = G8ContractTestSupport.IdB;
        internal const string IdC = G8ContractTestSupport.IdC;
        internal const string IdD = G8ContractTestSupport.IdD;
        internal const string MissingId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";

        internal static VariableId Id(string value) => VariableId.Parse(value);

        internal static ProjectVariablesDocument Registry(params string[] variables)
            => G8ContractTestSupport.ReadRegistry(G8ContractTestSupport.RegistryJson(variables));

        internal static string Literal(string id, string name, double value)
            => G8ContractTestSupport.VariableJson(id, name, G8ContractTestSupport.LiteralDefinition(value));

        internal static string Expression(string id, string name, string node)
            => G8ContractTestSupport.VariableJson(id, name, G8ContractTestSupport.ExpressionDefinition(node));

        internal static VariableDefinition ExpressionDefinition(BoundExpression expression)
            => G8ContractTestSupport.CreateExpressionDefinition(expression);

        internal static SelectivePalletDesignDocument Design(
            string rackId = RackA,
            string directVariable = null,
            BoundExpression expression = null,
            double clearance = 6.0)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);
            var document = SelectivePalletDesignDocument.From(
                design,
                rackId,
                "Rack " + rackId.Substring(0, Math.Min(4, rackId.Length)));

            if (directVariable != null || expression != null)
            {
                document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>();
                document.PropertyValues[ProjectPropertyIds.SelectiveVerticalClearanceToken] = directVariable != null
                    ? SelectivePropertyValueDocument.ToProjectVariable(directVariable)
                    : SelectivePropertyValueDocument.FromExpression(expression);
                document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return document;
        }

        internal static ProjectVariableScanEntry View(
            SelectivePalletDesignDocument document,
            string definition = "D1",
            string rackId = RackA)
            => ProjectVariableScanEntry.Selective(definition, rackId, document);

        internal static void AssertEmpty(VariableMutationPreflightResult result)
        {
            Assert.False(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
            Assert.Equal(RegistryMutationKind.None, result.Plan.RegistryMutation.Kind);
            Assert.Empty(result.Plan.RackMutations);
        }

        internal static Type RequireType(string capability, params string[] names)
        {
            var assembly = typeof(MutationPlan).Assembly;
            var type = assembly.GetTypes().FirstOrDefault(candidate =>
                names.Any(name => string.Equals(candidate.Name, name, StringComparison.Ordinal)));
            return type ?? throw Missing(capability);
        }

        internal static void RequirePlanReadSetSurface()
        {
            var readSet = RequireType("PlanReadSet", "PlanReadSet");
            RequireType("SymbolResultObservation", "SymbolResultObservation");
            RequireType("RepairDecisionObservation", "RepairDecisionObservation");
            var planMember = typeof(MutationPlan).GetProperty("PlanReadSet")
                ?? typeof(MutationPlan).GetProperty("ReadSet")
                ?? throw Missing("MutationPlan.PlanReadSet");
            Assert.True(planMember.PropertyType == readSet || readSet.IsAssignableFrom(planMember.PropertyType));
        }

        internal static void RequireRecoverySurface(string caseId)
        {
            RequireNamedCapability(caseId + ": SourceRoots", "SourceRoots");
            RequireNamedCapability(caseId + ": RecoveryUnits", "RecoveryUnits");
            RequireNamedCapability(caseId + ": structured recovery eligibility", "RecoveryEligibility", "RecoveryAssessment");
            RequireNamedCapability(caseId + ": structured blocking reasons", "RecoveryBlockingReason", "RecoveryBlockReason", "BlockingReasons");
        }

        internal static void RequireComparableObservationSurface(string caseId)
        {
            RequirePlanReadSetSurface();
            var observation = RequireType(caseId + ": comparable SymbolResultObservation", "SymbolResultObservation");
            Assert.Contains(observation.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                member => member.Name.IndexOf("Expected", StringComparison.OrdinalIgnoreCase) >= 0
                    || member.Name.IndexOf("Result", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        internal static void RequireRepairDecisionSurface(string caseId)
        {
            RequirePlanReadSetSurface();
            var observation = RequireType(caseId + ": comparable RepairDecisionObservation", "RepairDecisionObservation");
            Assert.Contains(observation.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                member => member.Name.IndexOf("Reason", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        internal static void RequireChangeDefinition()
        {
            var method = typeof(ProjectVariableMutationPreflight).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .SingleOrDefault(candidate => candidate.Name == "ChangeDefinition");
            Assert.NotNull(method ?? throw Missing("ProjectVariableMutationPreflight.ChangeDefinition"));
        }

        internal static void RequireSemanticInspection()
        {
            var outcome = RequireType("semantic InspectBinding outcomes", "BindingInspectionOutcome");
            var names = Enum.GetNames(outcome);
            Assert.Contains(names, name => name.IndexOf("Intrinsic", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.Contains(names, name => name.IndexOf("Upstream", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.Contains(names, name => name.IndexOf("Domain", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        internal static void RequireCommitComparison()
        {
            RequirePlanReadSetSurface();
            var methods = typeof(RegistryCommit).GetMethods(BindingFlags.Public | BindingFlags.Static);
            Assert.Contains(methods, method => method.GetParameters().Any(parameter =>
                parameter.ParameterType.Name == "MutationPlan" || parameter.ParameterType.Name == "PlanReadSet"));
        }

        internal static void RequireNamedCapability(string capability, params string[] names)
        {
            var assembly = typeof(MutationPlan).Assembly;
            var exists = assembly.GetTypes().Any(type => names.Any(name =>
                    type.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                || type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Any(member => names.Any(name => member.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)));
            if (!exists)
            {
                throw Missing(capability);
            }
        }

        private static XunitException Missing(string capability)
            => new XunitException("G9 contract RED: product is missing " + capability + ".");
    }
}
