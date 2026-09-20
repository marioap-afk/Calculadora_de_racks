using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit.Sdk;

namespace RackCad.Tests
{
    /// <summary>
    /// Compile-safe projection of G10 presentation capabilities. It invokes product behavior and only reads its
    /// result; it never parses, binds, formats, evaluates, or decides recovery in the test layer.
    /// </summary>
    internal static class G10ContractTestSupport
    {
        internal static string SourceKind(LinkedPropertyEditSession session)
            => Text(session.Committed.Source, "Kind");

        internal static object Expression(LinkedPropertyEditSession session)
            => Member(session.Committed.Source, "Expression", "BoundExpression")
               ?? throw Missing("committed LinkedPropertySource.Expression");

        internal static string DiagnosticCode(LinkedPropertyEditSession session)
        {
            var diagnostic = Items(Member(session, "Diagnostics", "DraftDiagnostics", "Diagnostic"))
                .FirstOrDefault() ?? throw Missing("typed editor diagnostics");
            return Text(diagnostic, "Code", "Kind");
        }

        internal static string DefinitionText(ProjectVariableRow row)
            => Text(row, "DefinitionText", "Definition", "CanonicalDefinition");

        internal static bool EvaluationSucceeded(ProjectVariableRow row)
            => Bool(row, "EvaluationSucceeded", "HasEvaluatedValue", "Succeeded");

        internal static double EvaluatedValue(ProjectVariableRow row)
            => Number(row, "EvaluatedValue", "Value");

        internal static string EvaluationDiagnostic(ProjectVariableRow row)
            => Text(row, "EvaluationDiagnostic", "DiagnosticCode", "FailureCode");

        internal static ProjectVariableIntent ChangeDefinitionIntent(VariableId id, VariableDefinition definition)
        {
            var method = typeof(ProjectVariableIntent).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .SingleOrDefault(candidate => candidate.Name == "ChangeDefinition" && candidate.GetParameters().Length == 2);
            if (method == null) throw Missing("ProjectVariableIntent.ChangeDefinition(id, bound definition)");
            return (ProjectVariableIntent)method.Invoke(null, new object[] { id, definition });
        }

        internal static VariableDefinition IntentDefinition(ProjectVariableIntent intent)
            => (VariableDefinition)(Member(intent, "Definition", "BoundDefinition")
                ?? throw Missing("ProjectVariableIntent bound Definition"));

        internal static string PresentationFailure(VariableMutationPreflightResult result)
        {
            var presentation = Member(result, "PresentationFailure", "StructuredFailure", "AttemptedStateFailure")
                ?? throw Missing("structured variable-change presentation failure");
            return string.Join("|", new[]
            {
                Text(presentation, "Category", "Code", "Kind"),
                string.Join(",", Items(Member(presentation, "DiagnosticCodes", "Reasons", "Diagnostics"))
                    .Select(item => item.ToString())),
            });
        }

        private static object Member(object target, params string[] names)
        {
            if (target == null) return null;
            foreach (var name in names)
            {
                var property = target.GetType().GetProperty(name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null) return property.GetValue(target);
            }
            return null;
        }

        private static IEnumerable<object> Items(object value)
        {
            if (value is string || value == null) return Array.Empty<object>();
            return value is IEnumerable sequence ? sequence.Cast<object>() : new[] { value };
        }

        private static string Text(object target, params string[] names)
        {
            var value = Member(target, names);
            if (value == null) throw Missing(string.Join("/", names));
            return value.ToString();
        }

        private static bool Bool(object target, params string[] names)
        {
            var value = Member(target, names);
            if (value is bool result) return result;
            throw Missing(string.Join("/", names));
        }

        private static double Number(object target, params string[] names)
        {
            var value = Member(target, names);
            if (value is double result) return result;
            throw Missing(string.Join("/", names));
        }

        private static XunitException Missing(string capability)
            => new XunitException("G10 contract RED: product behavior is missing: " + capability + ".");
    }
}
