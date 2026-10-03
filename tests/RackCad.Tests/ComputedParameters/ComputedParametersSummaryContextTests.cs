using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G4 / D-21 - el cambio ADITIVO de <c>RackComputedExpressionContext</c>: el resultado de <c>Evaluate</c>
    /// expone el <c>BoundExpression</c> evaluado y la coleccion determinista de <c>SymbolId</c> efectivamente leidos,
    /// sin cambiar el valor, los estados ni los diagnosticos que ya daba (G3).
    /// </summary>
    public class ComputedParametersSummaryContextTests
    {
        private static SymbolId FrentesId => new SymbolId(SymbolNamespace.Rack, RackMetricIds.FrentesToken);

        private static SymbolId FrentesVaciosId => new SymbolId(SymbolNamespace.Rack, RackMetricIds.FrentesVaciosToken);

        private static RackMetricResults Direct(MetricValue frentes, MetricValue frentesVacios)
            => RackMetricResults.Create(
                "rack-1",
                new Dictionary<MetricId, MetricValue>
                {
                    [RackMetricIds.Frentes] = frentes,
                    [RackMetricIds.FrentesVacios] = frentesVacios,
                });

        private static RackComputedExpressionContext Context(RackMetricResults results, params SymbolEntry[] variables)
            => RackComputedExpressionContext.Create(results, variables);

        [Fact]
        public void D21_Evaluate_ExponeElBoundExpressionEvaluado_YLosSymbolIdLeidos_SinCambiarSuResultado()
        {
            var context = Context(Direct(MetricValue.Available(4), MetricValue.Available(1)));
            var expression = BindOk("Rack.Frentes * 2", context.Expressions, SymbolScope.Rack);

            var evaluation = context.Evaluate(expression);

            // Lo que ya daba (G3): sin cambio.
            Assert.Equal(RackComputedEvaluationOutcome.Evaluated, evaluation.Outcome);
            AssertBits(8.0, evaluation.Value);
            Assert.Empty(evaluation.NotAvailableReferences);
            Assert.Empty(evaluation.Diagnostics);

            // Lo aditivo (D-21): el arbol evaluado (la misma instancia) y los SymbolId leidos.
            Assert.Same(expression, evaluation.Expression);
            Assert.Equal(new[] { FrentesId }, evaluation.ReadSymbols);
        }

        [Fact]
        public void D21_LosSymbolIdLeidos_SonDeterministas_SinRepetidos_YEnOrdenDeSymbolId()
        {
            var results = Direct(MetricValue.Available(4), MetricValue.Available(1));
            var context = Context(results, Variable(1, "Holgura", 6), Variable(2, "Base", 10));

            // El texto cita FrentesVacios y Base antes que Frentes y Holgura, y repite Frentes.
            var expression = BindOk(
                "Rack.FrentesVacios + {Base} + Rack.Frentes * {Holgura} + Rack.Frentes",
                context.Expressions,
                SymbolScope.Rack);
            var values = Values((Id(1), 6.0), (Id(2), 10.0));

            var first = context.Evaluate(expression, values);
            var second = context.Evaluate(expression, values);

            AssertBits(1.0 + 10.0 + 4.0 * 6.0 + 4.0, first.Value);

            // El orden es el de SymbolId (el del nucleo), no el de aparicion, y cada simbolo consta UNA vez.
            Assert.Equal(4, first.ReadSymbols.Count);
            Assert.Equal(first.ReadSymbols.OrderBy(symbol => symbol).ToList(), first.ReadSymbols);
            Assert.Equal(BoundExpressionDependencies.DirectDependencies(expression), first.ReadSymbols);
            Assert.Equal(first.ReadSymbols, second.ReadSymbols);
            Assert.Contains(Id(1), first.ReadSymbols);
            Assert.Contains(Id(2), first.ReadSymbols);
            Assert.Contains(FrentesId, first.ReadSymbols);
            Assert.Contains(FrentesVaciosId, first.ReadSymbols);
        }

        [Fact]
        public void D21_ConUnaReferenciaNoDisponible_ElContextoConservaElArbolYLosSimbolosConsultados_SinValorParcial()
        {
            var context = Context(Direct(MetricValue.NotSupported(), MetricValue.Available(1)));
            var expression = BindOk("Rack.FrentesVacios + Rack.Frentes", context.Expressions, SymbolScope.Rack);

            var evaluation = context.Evaluate(expression);

            // Lo que ya daba (G3): no se evalua, la lista de las no disponibles, sin valor parcial.
            Assert.Equal(RackComputedEvaluationOutcome.ComputedReferencesNotAvailable, evaluation.Outcome);
            Assert.Throws<InvalidOperationException>(() => evaluation.Value);
            var notAvailable = Assert.Single(evaluation.NotAvailableReferences);
            Assert.Equal(FrentesId, notAvailable.Symbol);
            Assert.Equal(MetricStatus.NotSupported, notAvailable.Status);

            // Lo aditivo (D-21): el arbol y los simbolos rack cuyo estado se leyo, en orden de SymbolId.
            Assert.Same(expression, evaluation.Expression);
            Assert.Equal(new[] { FrentesId, FrentesVaciosId }.OrderBy(symbol => symbol).ToList(), evaluation.ReadSymbols);
        }
    }
}
