using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Expressions;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>The Application-owned authoring boundary used by RACKVARIABLES.</summary>
    public static class ProjectVariableDefinitionAuthoring
    {
        public static bool TryCreate(
            string text,
            IReadOnlyList<ProjectVariableRow> rows,
            out VariableDefinition definition,
            out string error)
        {
            definition = null;
            error = null;
            var source = (text ?? string.Empty).Trim();

            if (!source.StartsWith("=", StringComparison.Ordinal))
            {
                if (double.TryParse(source, NumberStyles.Float, CultureInfo.InvariantCulture, out var literal) &&
                    !double.IsNaN(literal) && !double.IsInfinity(literal) && literal > 0.0)
                {
                    definition = VariableDefinition.Literal(literal);
                    return true;
                }

                error = "El valor tiene que ser un número mayor que cero o una fórmula que empiece con '='.";
                return false;
            }

            var variables = (rows ?? Array.Empty<ProjectVariableRow>())
                .Select(row => ProjectVariable.Create(row.Id, row.Name, row.Type, row.Definition))
                .ToArray();
            var context = ProjectVariablesExpressionAdapter.From(variables);
            var parsed = ExpressionParser.Parse(source.Substring(1));
            if (!parsed.Succeeded)
            {
                error = Describe(parsed.Diagnostics.Select(item => item.Code));
                return false;
            }

            var bound = ExpressionBinder.Bind(parsed.Syntax, context, SymbolScope.Project);
            if (!bound.Succeeded)
            {
                error = Describe(bound.Diagnostics.Select(item => item.Code));
                return false;
            }

            definition = VariableDefinition.Expression(bound.Expression);
            return true;
        }

        private static string Describe(IEnumerable<ExpressionDiagnosticCode> diagnostics)
            => "La fórmula no es válida: " + string.Join(", ", diagnostics.Distinct()) + ".";
    }
}
