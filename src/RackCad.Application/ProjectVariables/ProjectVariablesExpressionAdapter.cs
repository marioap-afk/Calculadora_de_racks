using System;
using System.Collections.Generic;
using RackCad.Application.Expressions;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// Projects an accredited Project Variables snapshot into the neutral expression engine. It preserves identity text
    /// exactly and performs no binding, evaluation or type-specific graph work.
    /// </summary>
    public static class ProjectVariablesExpressionAdapter
    {
        public static ExpressionContext From(IEnumerable<ProjectVariable> variables)
        {
            if (variables == null)
            {
                throw new ArgumentNullException(nameof(variables));
            }

            var entries = new List<SymbolEntry>();
            foreach (var variable in variables)
            {
                if (variable == null)
                {
                    throw new ArgumentException("A Project Variables snapshot cannot contain a null entry.", nameof(variables));
                }

                var definition = variable.Definition.Kind == VariableDefinitionKind.Literal
                    ? SymbolDefinition.FromLiteral(variable.Definition.LiteralValue)
                    : SymbolDefinition.FromExpression(variable.Definition.ExpressionValue);

                entries.Add(new SymbolEntry(
                    SymbolId.ProjectVariable(variable.Id.Value),
                    SymbolScope.Project,
                    variable.Name,
                    definition));
            }

            return ExpressionContext.Create(SymbolTable.Create(entries));
        }

        internal static ExpressionContext From(UsableProjectVariablesRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            var entries = new List<SymbolEntry>();
            foreach (var target in registry.Targets())
            {
                var definition = target.Definition.Kind == VariableDefinitionKind.Literal
                    ? SymbolDefinition.FromLiteral(target.Definition.LiteralValue)
                    : SymbolDefinition.FromExpression(target.Definition.ExpressionValue);
                entries.Add(new SymbolEntry(
                    SymbolId.ProjectVariable(target.VariableId.Value),
                    SymbolScope.Project,
                    target.Name ?? string.Empty,
                    definition));
            }

            return ExpressionContext.Create(SymbolTable.Create(entries));
        }
    }
}
