using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Expressions;
using Xunit.Sdk;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G3-T1 (RED) - utilidades comunes de las pruebas de simbolos rack. Aqui se FIJA la API nueva que G3-T2 debe
    /// implementar sin poder editar estas pruebas: los miembros nuevos de enums se escriben con su valor numerico
    /// definitivo (D-16.1 y D-16.3) y los tipos nuevos de Application se alcanzan por reflexion con nombres y firmas
    /// exactos. Los nombres son ilustrativos en el Freeze (D-08); la forma y las reglas no.
    ///
    /// <para>
    /// Mientras la API no exista, cada ayudante falla EN EJECUCION (<see cref="XunitException"/> con el nombre exacto de
    /// lo que falta, o la excepcion del constructor vigente), nunca por compilacion.
    /// </para>
    /// </summary>
    internal static class ComputedParametersSymbolsKit
    {
        /// <summary>D-16.1: <c>SymbolNamespace.Rack</c>, el segundo miembro, valor 2.</summary>
        internal static readonly SymbolNamespace RackNamespace = (SymbolNamespace)2;

        /// <summary>D-16.3: <c>SymbolDefinitionKind.Computed</c>, el tercer miembro, valor 3.</summary>
        internal static readonly SymbolDefinitionKind ComputedKind = (SymbolDefinitionKind)3;

        internal const string RackContextTypeName = "RackCad.Application.ComputedParameters.RackComputedExpressionContext";

        internal static SymbolId RackId(string token) => new SymbolId(RackNamespace, token);

        internal static SymbolId FrentesId => RackId(RackMetricIds.FrentesToken);

        internal static SymbolId FrentesVaciosId => RackId(RackMetricIds.FrentesVaciosToken);

        internal static Exception Pending(string what)
            => new XunitException("API pendiente de G3-T2 (el RED fija su forma): " + what);

        /// <summary>
        /// D-16.3: <c>SymbolDefinition.FromComputed()</c>, una fabrica sin parametros de una definicion hoja, sin valor.
        /// </summary>
        internal static SymbolDefinition Computed()
        {
            var method = typeof(SymbolDefinition).GetMethod(
                "FromComputed",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                Type.EmptyTypes,
                null);

            if (method == null || method.ReturnType != typeof(SymbolDefinition))
            {
                throw Pending("static SymbolDefinition SymbolDefinition.FromComputed()");
            }

            return (SymbolDefinition)Unwrap(() => method.Invoke(null, null));
        }

        /// <summary>Una entrada rack sintetica: ambito Rack y definicion <c>Computed</c>.</summary>
        internal static SymbolEntry RackEntry(string token, string member, SymbolScope scope = SymbolScope.Rack)
            => new SymbolEntry(RackId(token), scope, member, Computed());

        internal static SymbolEntry Frentes() => RackEntry(RackMetricIds.FrentesToken, "Frentes");

        internal static SymbolEntry FrentesVacios() => RackEntry(RackMetricIds.FrentesVaciosToken, "FrentesVacios");

        /// <summary>Las dos entradas rack del catalogo V1 (D-02, D-05).</summary>
        internal static SymbolEntry[] CatalogRackEntries() => new[] { Frentes(), FrentesVacios() };

        /// <summary>Un contexto con las variables indicadas y las dos entradas rack del catalogo.</summary>
        internal static ExpressionContext RackContext(params SymbolEntry[] variables)
            => Context(variables.Concat(CatalogRackEntries()).ToArray());

        internal static object Unwrap(Func<object> action)
        {
            try
            {
                return action();
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        // ================================================================ RackComputedExpressionContext (D-17 puntos 4 a 6)

        internal static Type RackContextType()
            => typeof(RackMetricResults).Assembly.GetType(RackContextTypeName)
               ?? throw Pending("tipo " + RackContextTypeName);

        /// <summary>
        /// <c>RackComputedExpressionContext.Create(RackMetricResults results, IEnumerable&lt;SymbolEntry&gt; projectVariableEntries)</c>:
        /// estatico. El contexto solo se construye con resultados TERMINADOS de la peticion por rack (D-15 R2), nunca con
        /// la peticion ni con el costado de resolucion.
        /// </summary>
        internal static MethodInfo CreateMethod()
        {
            var create = RackContextType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(method =>
                {
                    var parameters = method.GetParameters();
                    return method.Name == "Create"
                           && parameters.Length == 2
                           && parameters[0].ParameterType == typeof(RackMetricResults)
                           && parameters[1].ParameterType.IsAssignableFrom(typeof(List<SymbolEntry>));
                });

            return create ?? throw Pending("static RackComputedExpressionContext RackComputedExpressionContext.Create(RackMetricResults, IEnumerable<SymbolEntry>)");
        }

        internal static RackContextView CreateRackContext(RackMetricResults results, params SymbolEntry[] variables)
        {
            var instance = Unwrap(() => CreateMethod().Invoke(null, new object[] { results, variables.ToList() }));
            return new RackContextView(instance ?? throw Pending("Create devuelve una instancia no nula"));
        }

        /// <summary>
        /// La vista por reflexion de un <c>RackComputedExpressionContext</c>: la propiedad de tipo
        /// <see cref="ExpressionContext"/> (su tabla une las entradas projectVariable del registro y las rack
        /// <c>Computed</c>; se enlaza con ambito Rack) y <c>Evaluate(BoundExpression ...)</c>.
        /// </summary>
        internal sealed class RackContextView
        {
            private readonly object _instance;

            internal RackContextView(object instance)
            {
                _instance = instance;
            }

            internal ExpressionContext Expressions
            {
                get
                {
                    var property = _instance.GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .FirstOrDefault(candidate => candidate.PropertyType == typeof(ExpressionContext));

                    if (property == null)
                    {
                        throw Pending("propiedad de tipo ExpressionContext en RackComputedExpressionContext (la tabla enlazable)");
                    }

                    return (ExpressionContext)Unwrap(() => property.GetValue(_instance));
                }
            }

            internal SymbolTable Symbols => Expressions.Symbols;

            /// <summary>
            /// <c>Evaluate(BoundExpression expression[, IReadOnlyDictionary&lt;SymbolId, double&gt; projectVariableValues])</c>.
            /// Estas pruebas solo evaluan arboles con referencias rack y numeros; si T2 pide valores de variables se les
            /// pasa un diccionario vacio.
            /// </summary>
            internal EvaluationView Evaluate(BoundExpression expression)
            {
                var method = _instance.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(candidate =>
                    {
                        var parameters = candidate.GetParameters();
                        return candidate.Name == "Evaluate"
                               && parameters.Length >= 1
                               && parameters[0].ParameterType == typeof(BoundExpression);
                    });

                if (method == null)
                {
                    throw Pending("RackComputedExpressionContext.Evaluate(BoundExpression, ...)");
                }

                var arguments = new List<object> { expression };
                foreach (var parameter in method.GetParameters().Skip(1))
                {
                    if (parameter.ParameterType.IsAssignableFrom(typeof(Dictionary<SymbolId, double>)))
                    {
                        arguments.Add(new Dictionary<SymbolId, double>());
                    }
                    else if (parameter.HasDefaultValue)
                    {
                        arguments.Add(parameter.DefaultValue);
                    }
                    else
                    {
                        throw Pending("Evaluate solo puede pedir, ademas del arbol, un diccionario de valores de variables");
                    }
                }

                var raw = Unwrap(() => method.Invoke(_instance, arguments.ToArray()));
                return new EvaluationView(raw ?? throw Pending("Evaluate devuelve un resultado no nulo"));
            }
        }

        /// <summary>Una referencia rack que no esta <c>Available</c>: <c>(SymbolId, estado, razon?)</c> (D-17.5).</summary>
        internal sealed class NotAvailableReference
        {
            internal NotAvailableReference(SymbolId symbol, MetricStatus status, UnavailableReason reason)
            {
                Symbol = symbol;
                Status = status;
                Reason = reason;
            }

            internal SymbolId Symbol { get; }

            internal MetricStatus Status { get; }

            internal UnavailableReason Reason { get; }

            public override string ToString() => Symbol + ":" + Status + (Reason == null ? string.Empty : "(" + Reason + ")");
        }

        /// <summary>
        /// El resultado de <c>Evaluate</c>: <c>Outcome</c> (enum con los miembros <c>Evaluated</c>,
        /// <c>ComputedReferencesNotAvailable</c> y <c>EvaluationFailed</c>), <c>Value</c> (double; lanza
        /// <see cref="InvalidOperationException"/> si no hubo evaluacion: nunca hay valor parcial),
        /// <c>NotAvailableReferences</c> (lista de <c>{ SymbolId Symbol, MetricStatus Status, UnavailableReason Reason }</c>
        /// en orden de SymbolId, vacia salvo con ComputedReferencesNotAvailable) y <c>Diagnostics</c>
        /// (<see cref="ExpressionDiagnostic"/>, vacia salvo con EvaluationFailed).
        /// </summary>
        internal sealed class EvaluationView
        {
            private readonly object _raw;

            internal EvaluationView(object raw)
            {
                _raw = raw;
            }

            internal string Outcome => Get("Outcome")?.ToString();

            internal double Value => (double)Get("Value");

            internal IReadOnlyList<NotAvailableReference> NotAvailable
            {
                get
                {
                    var items = Get("NotAvailableReferences") as System.Collections.IEnumerable
                                ?? throw Pending("propiedad NotAvailableReferences (IEnumerable) en el resultado de Evaluate");

                    return items.Cast<object>()
                        .Select(item => new NotAvailableReference(
                            (SymbolId)GetOf(item, "Symbol"),
                            (MetricStatus)GetOf(item, "Status"),
                            (UnavailableReason)GetOf(item, "Reason")))
                        .ToList();
                }
            }

            internal IReadOnlyList<ExpressionDiagnostic> Diagnostics
                => ((System.Collections.IEnumerable)(Get("Diagnostics")
                    ?? throw Pending("propiedad Diagnostics en el resultado de Evaluate")))
                    .Cast<ExpressionDiagnostic>()
                    .ToList();

            private object Get(string name) => GetOf(_raw, name);

            private static object GetOf(object instance, string name)
            {
                var property = instance.GetType()
                    .GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    ?? throw Pending("propiedad " + name + " en " + instance.GetType().Name);

                return Unwrap(() => property.GetValue(instance));
            }
        }
    }
}
