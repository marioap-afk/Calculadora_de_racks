using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.Expressions;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>Como termino una evaluacion de <see cref="RackComputedExpressionContext"/> (I-63 D-17.5).</summary>
    public enum RackComputedEvaluationOutcome
    {
        /// <summary>Todas las referencias <c>rack</c> estaban <c>Available</c> y el evaluador del nucleo dio un valor.</summary>
        Evaluated = 1,

        /// <summary>Alguna referencia <c>rack</c> no estaba <c>Available</c>: no se evaluo.</summary>
        ComputedReferencesNotAvailable = 2,

        /// <summary>El evaluador del nucleo fallo (division por cero, resultado no finito, referencia ausente...).</summary>
        EvaluationFailed = 3,
    }

    /// <summary>Una referencia <c>rack</c> del arbol cuyo estado no es <c>Available</c>: <c>(SymbolId, estado, razon?)</c> (I-63 D-17.5).</summary>
    public sealed class RackComputedNotAvailableReference
    {
        internal RackComputedNotAvailableReference(SymbolId symbol, MetricStatus status, UnavailableReason reason)
        {
            Symbol = symbol;
            Status = status;
            Reason = reason;
        }

        public SymbolId Symbol { get; }

        /// <summary><c>NotApplicable</c>, <c>NotSupported</c> o <c>Unavailable</c>, conservados como estados distintos.</summary>
        public MetricStatus Status { get; }

        /// <summary>La razon: solo con <see cref="MetricStatus.Unavailable"/>; nula en los demas estados.</summary>
        public UnavailableReason Reason { get; }

        public override string ToString() => Symbol + ":" + Status + (Reason == null ? string.Empty : "(" + Reason + ")");
    }

    /// <summary>
    /// El resultado de evaluar un arbol en un <see cref="RackComputedExpressionContext"/>: un valor, o ningun valor.
    /// Nunca hay valor parcial: leer <see cref="Value"/> sin evaluacion lanza.
    /// </summary>
    public sealed class RackComputedEvaluation
    {
        private static readonly IReadOnlyList<RackComputedNotAvailableReference> NoReferences =
            new ReadOnlyCollection<RackComputedNotAvailableReference>(Array.Empty<RackComputedNotAvailableReference>());

        private static readonly IReadOnlyList<ExpressionDiagnostic> NoDiagnostics =
            new ReadOnlyCollection<ExpressionDiagnostic>(Array.Empty<ExpressionDiagnostic>());

        private readonly double _value;

        private RackComputedEvaluation(
            RackComputedEvaluationOutcome outcome,
            double value,
            IReadOnlyList<RackComputedNotAvailableReference> notAvailable,
            IReadOnlyList<ExpressionDiagnostic> diagnostics)
        {
            Outcome = outcome;
            _value = value;
            NotAvailableReferences = notAvailable;
            Diagnostics = diagnostics;
        }

        public RackComputedEvaluationOutcome Outcome { get; }

        public double Value
            => Outcome == RackComputedEvaluationOutcome.Evaluated
                ? _value
                : throw new InvalidOperationException("Esta evaluacion no produjo valor: " + Outcome + ".");

        /// <summary>
        /// Todas las referencias <c>rack</c> del arbol que no estan <c>Available</c>, en orden de <see cref="SymbolId"/> y
        /// sin repetir. Vacia salvo con <see cref="RackComputedEvaluationOutcome.ComputedReferencesNotAvailable"/>.
        /// </summary>
        public IReadOnlyList<RackComputedNotAvailableReference> NotAvailableReferences { get; }

        /// <summary>Los diagnosticos del evaluador. Vacia salvo con <see cref="RackComputedEvaluationOutcome.EvaluationFailed"/>.</summary>
        public IReadOnlyList<ExpressionDiagnostic> Diagnostics { get; }

        /// <summary>El arbol enlazado que se evaluo (D-21). Solo lectura: no cambia el resultado de <c>Evaluate</c>.</summary>
        public BoundExpression Expression => null;

        /// <summary>Los <see cref="SymbolId"/> efectivamente leidos (D-21), en orden de <see cref="SymbolId"/> y sin repetir.</summary>
        public IReadOnlyList<SymbolId> ReadSymbols => NoSymbols;

        private static readonly IReadOnlyList<SymbolId> NoSymbols =
            new ReadOnlyCollection<SymbolId>(Array.Empty<SymbolId>());

        internal static RackComputedEvaluation Evaluated(double value)
            => new RackComputedEvaluation(RackComputedEvaluationOutcome.Evaluated, value, NoReferences, NoDiagnostics);

        internal static RackComputedEvaluation NotAvailable(IReadOnlyList<RackComputedNotAvailableReference> references)
            => new RackComputedEvaluation(
                RackComputedEvaluationOutcome.ComputedReferencesNotAvailable,
                0.0,
                new ReadOnlyCollection<RackComputedNotAvailableReference>(references.ToList()),
                NoDiagnostics);

        internal static RackComputedEvaluation Failed(IReadOnlyList<ExpressionDiagnostic> diagnostics)
            => new RackComputedEvaluation(RackComputedEvaluationOutcome.EvaluationFailed, 0.0, NoReferences, diagnostics);
    }

    /// <summary>
    /// El contexto de expresiones de UN rack (I-63 D-14, D-15 y D-17 puntos 4 a 6). Su tabla une las entradas
    /// <c>projectVariable</c> del registro y las entradas <c>rack</c> del catalogo (<c>Computed</c>, ambito
    /// <c>Rack</c>), y se enlaza con ambito <c>Rack</c>: <c>Rack.X</c> enlaza y <c>Project.X</c> da
    /// <c>UnknownNamespace</c>.
    ///
    /// <para>
    /// Solo se construye con los resultados TERMINADOS de la peticion por rack (R2): evaluar nunca resuelve ni lee un
    /// diseno. El enlace no depende del kind. Antes de evaluar se revisa el estado de CADA referencia <c>rack</c> del
    /// arbol, en orden de <see cref="SymbolId"/>: si todas estan <c>Available</c> evalua con el evaluador del nucleo; si
    /// alguna no lo esta devuelve sin evaluar <see cref="RackComputedEvaluationOutcome.ComputedReferencesNotAvailable"/>
    /// con todas las que no lo estan, sin colapsar estados y sin valor parcial.
    /// </para>
    /// <para>
    /// Vive en Application, fuera de <c>RackCad.Application.Expressions</c>: el nucleo no nombra racks ni metricas.
    /// </para>
    /// </summary>
    public sealed class RackComputedExpressionContext
    {
        private readonly RackMetricResults _results;

        private RackComputedExpressionContext(RackMetricResults results, ExpressionContext expressions)
        {
            _results = results;
            Expressions = expressions;
        }

        /// <summary>La tabla enlazable: variables del registro mas las entradas <c>rack</c> del catalogo.</summary>
        public ExpressionContext Expressions { get; }

        /// <summary>
        /// Construye el contexto con los resultados terminados de la peticion y las entradas <c>projectVariable</c> del
        /// mismo documento de registro que uso la fase efectiva de ese rack. Es una costura interna, como la de
        /// <see cref="ExpressionContext"/>.
        /// </summary>
        internal static RackComputedExpressionContext Create(
            RackMetricResults results,
            IEnumerable<SymbolEntry> projectVariableEntries)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (projectVariableEntries == null)
            {
                throw new ArgumentNullException(nameof(projectVariableEntries));
            }

            var entries = new List<SymbolEntry>();
            foreach (var entry in projectVariableEntries)
            {
                if (entry == null)
                {
                    throw new ArgumentException("Una entrada nula no entra en la tabla.", nameof(projectVariableEntries));
                }

                if (entry.Id.Namespace != SymbolNamespace.ProjectVariable || entry.Definition.Kind == SymbolDefinitionKind.Computed)
                {
                    throw new ArgumentException(
                        "Solo entran las entradas projectVariable del registro; las entradas rack las pone el catalogo.",
                        nameof(projectVariableEntries));
                }

                entries.Add(entry);
            }

            foreach (var metric in RackMetricIds.RackMetrics)
            {
                entries.Add(new SymbolEntry(
                    new SymbolId(SymbolNamespace.Rack, metric.Token),
                    SymbolScope.Rack,
                    MemberName(metric),
                    SymbolDefinition.FromComputed()));
            }

            return new RackComputedExpressionContext(results, ExpressionContext.Create(SymbolTable.Create(entries)));
        }

        /// <summary>
        /// Evalua el arbol (ya enlazado en este contexto). <paramref name="projectVariableValues"/> trae el valor de cada
        /// variable que el arbol lee; las referencias <c>rack</c> las toma de los resultados terminados.
        /// </summary>
        public RackComputedEvaluation Evaluate(
            BoundExpression expression,
            IReadOnlyDictionary<SymbolId, double> projectVariableValues = null)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var notAvailable = new List<RackComputedNotAvailableReference>();
            var rackValues = new Dictionary<SymbolId, double>();

            // DirectDependencies ya viene en orden de SymbolId y sin repetidos.
            foreach (var dependency in BoundExpressionDependencies.DirectDependencies(expression))
            {
                if (dependency.Namespace != SymbolNamespace.Rack || !Expressions.Symbols.TryGet(dependency, out _))
                {
                    // Una referencia ausente de la tabla es BrokenReference: la dice el evaluador del nucleo.
                    continue;
                }

                var value = _results[new MetricId(MetricScope.Rack, dependency.Key)];

                if (value.Status == MetricStatus.Available)
                {
                    rackValues.Add(dependency, value.Value);
                }
                else
                {
                    notAvailable.Add(new RackComputedNotAvailableReference(dependency, value.Status, value.Reason));
                }
            }

            if (notAvailable.Count > 0)
            {
                return RackComputedEvaluation.NotAvailable(notAvailable);
            }

            var values = new Dictionary<SymbolId, double>();
            if (projectVariableValues != null)
            {
                foreach (var pair in projectVariableValues)
                {
                    values[pair.Key] = pair.Value;
                }
            }

            foreach (var pair in rackValues)
            {
                values[pair.Key] = pair.Value;
            }

            var result = ExpressionEvaluator.Evaluate(expression, Expressions, values);

            return result.Succeeded
                ? RackComputedEvaluation.Evaluated(result.Value)
                : RackComputedEvaluation.Failed(result.Diagnostics);
        }

        /// <summary>El nombre de miembro del catalogo (D-03): solo sirve para mostrar y para resolver al escribir.</summary>
        private static string MemberName(MetricId metric)
        {
            switch (metric.Token)
            {
                case RackMetricIds.FrentesToken: return "Frentes";
                case RackMetricIds.FrentesVaciosToken: return "FrentesVacios";
                default: throw new InvalidOperationException("La metrica " + metric + " no declara nombre de miembro.");
            }
        }
    }
}
