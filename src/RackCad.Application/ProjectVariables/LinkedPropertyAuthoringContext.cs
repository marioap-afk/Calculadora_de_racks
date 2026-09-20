using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RackCad.Application.Expressions;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// One expression-visible project variable from a single accredited registry snapshot. Visibility is independent
    /// from current numeric usability: a failed symbol must still bind by identity so its structured failure survives.
    /// </summary>
    public sealed class LinkedPropertyAuthoringSymbol
    {
        internal LinkedPropertyAuthoringSymbol(
            VariableId variableId,
            string name,
            VariableType variableType,
            RegistrySymbolResult evaluation)
        {
            VariableId = variableId;
            Name = name ?? string.Empty;
            VariableType = variableType;
            Evaluation = evaluation ?? throw new ArgumentNullException(nameof(evaluation));
        }

        public VariableId VariableId { get; }

        public string Name { get; }

        public VariableType VariableType { get; }

        /// <summary>The G7 result, including diagnostics and root causes when this symbol failed.</summary>
        public RegistrySymbolResult Evaluation { get; }
    }

    /// <summary>
    /// Expression authoring authority for one linked-property type. It carries every compatible symbol from one
    /// accredited snapshot, while direct-reference choices remain a separate healthy-only projection.
    /// </summary>
    public sealed class LinkedPropertyAuthoringContext
    {
        private LinkedPropertyAuthoringContext(
            ExpressionContext expressionContext,
            IReadOnlyList<LinkedPropertyAuthoringSymbol> symbols)
        {
            ExpressionContext = expressionContext ?? throw new ArgumentNullException(nameof(expressionContext));
            Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
        }

        public IReadOnlyList<LinkedPropertyAuthoringSymbol> Symbols { get; }

        internal ExpressionContext ExpressionContext { get; }

        internal static LinkedPropertyAuthoringContext For(
            SelectiveLinkedPropertyDescriptor descriptor,
            UsableProjectVariablesRegistry registry)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            if (registry == null) throw new ArgumentNullException(nameof(registry));

            var registryContext = ProjectVariablesExpressionAdapter.From(registry);
            var evaluation = RegistryEvaluation.Evaluate(registryContext);
            var symbols = new List<LinkedPropertyAuthoringSymbol>();
            var authoringEntries = new List<SymbolEntry>();
            foreach (var target in registry.Targets())
            {
                if (target.VariableType != descriptor.VariableType)
                {
                    continue;
                }

                var id = SymbolId.ProjectVariable(target.VariableId.Value);
                var result = evaluation.Result(id);
                symbols.Add(new LinkedPropertyAuthoringSymbol(
                    target.VariableId,
                    target.Name,
                    target.VariableType,
                    result));
                // The binder context deliberately contains only compatible symbols. The definition is a neutral
                // placeholder: property evaluation consumes the G7 snapshot values/results carried alongside it.
                authoringEntries.Add(new SymbolEntry(
                    id,
                    SymbolScope.Project,
                    target.Name ?? string.Empty,
                    SymbolDefinition.FromLiteral(result.Succeeded ? result.Value : 0.0)));
            }

            return new LinkedPropertyAuthoringContext(
                ExpressionContext.Create(SymbolTable.Create(authoringEntries)),
                new ReadOnlyCollection<LinkedPropertyAuthoringSymbol>(symbols));
        }

        internal IReadOnlyList<LinkedPropertyOption> DirectSelectionOptions()
        {
            var options = new List<LinkedPropertyOption>();
            foreach (var symbol in Symbols)
            {
                if (!symbol.Evaluation.Succeeded)
                {
                    continue;
                }

                options.Add(new LinkedPropertyOption(
                    symbol.VariableId,
                    symbol.Name,
                    symbol.VariableType,
                    symbol.Evaluation.Value));
            }

            return new ReadOnlyCollection<LinkedPropertyOption>(options);
        }
    }
}
