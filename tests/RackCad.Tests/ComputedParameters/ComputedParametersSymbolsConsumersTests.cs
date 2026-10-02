using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.ProjectVariables;
using Xunit;
using static RackCad.Tests.ComputedParametersSymbolsKit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G3-T1 (RED) - INV-18 y D-14: las formulas de propiedad vinculada (RACKEDITAR) y las definiciones de variables
    /// (RACKVARIABLES) NO ofrecen simbolos <c>Rack.*</c>: <c>=Rack.Frentes</c> da <c>UnknownNamespace</c>, con los mismos
    /// mensajes que hoy. Control positivo: la MISMA funcion enlaza la misma expresion en una tabla sintetica con entradas
    /// rack. La variante incorrecta, ofrecer rack en el contexto de propiedad, rompe la asercion principal.
    /// </summary>
    public class ComputedParametersSymbolsConsumersTests
    {
        private const string RackFrentesText = "Rack.Frentes";

        private static readonly VariableId HolguraId = VariableId.Parse(Key(1));

        private static ProjectVariable Holgura()
            => ProjectVariable.Create(HolguraId, "Holgura", VariableType.Length, VariableDefinition.Literal(6.0));

        /// <summary>El contexto que RACKVARIABLES construye para definiciones de variables (ambito Project).</summary>
        private static ExpressionContext VariableDefinitionContext() => ProjectVariablesExpressionAdapter.From(new[] { Holgura() });

        private static LinkedPropertyExpressionAuthoring PropertyAuthoring()
            => new LinkedPropertyExpressionAuthoring(
                new List<LinkedPropertyOption> { new LinkedPropertyOption(HolguraId, "Holgura", VariableType.Length, 6.0) });

        /// <summary>El contexto que RACKEDITAR construye para formulas de propiedad (ambito Rack).</summary>
        private static ExpressionContext PropertyFormulaContext() => ExpressionContext.Create(PropertyAuthoring().Symbols);

        /// <summary>
        /// UN solo helper para el objetivo y para el control: enlaza el texto en el contexto y ambito dados y devuelve
        /// <c>ok</c> o los diagnosticos (codigo y posicion).
        /// </summary>
        private static string Outcome(ExpressionContext context, SymbolScope scope, string text)
        {
            var result = Bind(text, context, scope);

            return result.Succeeded ? "ok" : Describe(result.Diagnostics);
        }

        [Fact]
        public void INV18_AVariableDefinition_DoesNotOfferRack_ButASyntheticRackTableDoes()
        {
            var context = VariableDefinitionContext();

            Assert.All(context.Symbols.Entries, entry => Assert.Equal(SymbolNamespace.ProjectVariable, entry.Id.Namespace));
            Assert.Equal("UnknownNamespace@0+12", Outcome(context, SymbolScope.Project, RackFrentesText));
            Assert.Equal("UnknownNamespace@0+18", Outcome(context, SymbolScope.Project, "Project.TotalRacks"));

            // Control (misma funcion): una tabla sintetica con entradas rack enlaza la misma expresion.
            Assert.Equal("ok", Outcome(RackContext(Variable(1, "Holgura", 6)), SymbolScope.Rack, RackFrentesText));
        }

        [Fact]
        public void INV18_ALinkedPropertyFormula_DoesNotOfferRack_ButASyntheticRackTableDoes()
        {
            var context = PropertyFormulaContext();

            Assert.All(context.Symbols.Entries, entry => Assert.Equal(SymbolNamespace.ProjectVariable, entry.Id.Namespace));
            Assert.Equal("UnknownNamespace@0+12", Outcome(context, SymbolScope.Rack, RackFrentesText));
            Assert.Equal("UnknownNamespace@0+18", Outcome(context, SymbolScope.Rack, "Project.TotalRacks"));

            // Control (misma funcion): una tabla sintetica con entradas rack enlaza la misma expresion.
            Assert.Equal("ok", Outcome(RackContext(Variable(1, "Holgura", 6)), SymbolScope.Rack, RackFrentesText));
        }

        [Fact]
        public void INV18_TheAuthoringMessages_AreTheSameAsForAnyOtherUnknownNamespace()
        {
            var rows = new List<ProjectVariableRow> { new ProjectVariableRow(HolguraId, "Holgura", VariableType.Length, 6.0, null) };

            // RACKVARIABLES: el mensaje de la definicion con =Rack.Frentes es el de cualquier namespace desconocido.
            Assert.False(ProjectVariableDefinitionAuthoring.TryCreate("=Rack.Frentes", rows, out _, out var rackError));
            Assert.False(ProjectVariableDefinitionAuthoring.TryCreate("=Total.Frentes", rows, out _, out var otherError));
            Assert.Contains("UnknownNamespace", rackError, System.StringComparison.Ordinal);
            Assert.Equal(otherError, rackError);

            // RACKEDITAR: el resultado de la formula es el de cualquier namespace desconocido.
            var rack = PropertyAuthoring().Run("=Rack.Frentes");
            var other = PropertyAuthoring().Run("=Total.Frentes");
            Assert.False(rack.Succeeded);
            Assert.False(other.Succeeded);
            Assert.Equal(other.Diagnostics.Select(diagnostic => diagnostic.Code), rack.Diagnostics.Select(diagnostic => diagnostic.Code));

            // Control (misma tabla de enlace que alimenta ambos): con entradas rack la expresion enlaza.
            Assert.Equal("ok", Outcome(RackContext(Variable(1, "Holgura", 6)), SymbolScope.Rack, RackFrentesText));
        }

        [Fact]
        public void INV18_NeitherConsumerTableContainsAnEntryOfScopeRack_ByTheRackNamespace()
        {
            // El ambito Rack de un consumidor de propiedad NO implica el namespace rack: sus tablas solo traen variables.
            foreach (var context in new[] { VariableDefinitionContext(), PropertyFormulaContext() })
            {
                Assert.NotEmpty(context.Symbols.Entries);
                Assert.DoesNotContain(context.Symbols.Entries, entry => entry.Id.Namespace == RackNamespace);
                Assert.DoesNotContain(context.Symbols.Entries, entry => entry.Definition.Kind == ComputedKind);
            }

            // Control (misma consulta): la tabla sintetica SI trae entradas rack Computed.
            var synthetic = RackContext(Variable(1, "Holgura", 6));
            Assert.Contains(synthetic.Symbols.Entries, entry => entry.Id.Namespace == RackNamespace);
            Assert.Contains(synthetic.Symbols.Entries, entry => entry.Definition.Kind == ComputedKind);
        }
    }
}
