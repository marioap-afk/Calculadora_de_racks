using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>Estado en ejecucion de un <see cref="MetricValue"/> (I-63 D-09). Nunca se infiere de un null.</summary>
    public enum MetricStatus
    {
        Available = 1,
        NotApplicable = 2,
        NotSupported = 3,
        Unavailable = 4,
    }

    /// <summary>Razones cerradas de <c>Unavailable</c> de rack (I-63 D-09). Son datos, no texto.</summary>
    public enum UnavailableReasonKind
    {
        EnvelopeUnreadable = 1,
        DesignUnreadable = 2,
        SiblingsDivergent = 3,
        KindIncoherent = 4,
        KindAbsent = 5,
        KindUnknown = 6,
        RegistryUnreadable = 7,
        EffectiveFailed = 8,
        CatalogUnavailable = 9,
        ResolveFailed = 10,
    }

    /// <summary>La razon de un <c>Unavailable</c>. <see cref="EffectiveOutcome"/> solo acompana a <see cref="UnavailableReasonKind.EffectiveFailed"/>.</summary>
    public sealed class UnavailableReason : IEquatable<UnavailableReason>
    {
        private UnavailableReason(UnavailableReasonKind kind, SelectiveEffectiveOutcome? effectiveOutcome)
        {
            Kind = kind;
            EffectiveOutcome = effectiveOutcome;
        }

        public UnavailableReasonKind Kind { get; }

        public SelectiveEffectiveOutcome? EffectiveOutcome { get; }

        public static UnavailableReason Of(UnavailableReasonKind kind)
        {
            if (kind == UnavailableReasonKind.EffectiveFailed)
            {
                throw new ArgumentException("EffectiveFailed lleva su outcome: usa EffectiveFailed(outcome).", nameof(kind));
            }

            return new UnavailableReason(kind, null);
        }

        public static UnavailableReason EffectiveFailed(SelectiveEffectiveOutcome outcome)
            => new UnavailableReason(UnavailableReasonKind.EffectiveFailed, outcome);

        public bool Equals(UnavailableReason other)
            => other != null && Kind == other.Kind && EffectiveOutcome == other.EffectiveOutcome;

        public override bool Equals(object obj) => Equals(obj as UnavailableReason);

        public override int GetHashCode() => (Kind.GetHashCode() * 397) ^ EffectiveOutcome.GetHashCode();

        public override string ToString()
            => EffectiveOutcome.HasValue ? Kind + "(" + EffectiveOutcome.Value + ")" : Kind.ToString();
    }

    /// <summary>Valor de una metrica: un estado cerrado y, solo con <see cref="MetricStatus.Available"/>, un <c>double</c> finito.</summary>
    public sealed class MetricValue : IEquatable<MetricValue>
    {
        private MetricValue(MetricStatus status, double value, UnavailableReason reason)
        {
            Status = status;
            Value = value;
            Reason = reason;
        }

        public MetricStatus Status { get; }

        /// <summary>El valor. Solo tiene sentido con <see cref="MetricStatus.Available"/>; en los demas estados es 0.</summary>
        public double Value { get; }

        /// <summary>La razon. Solo con <see cref="MetricStatus.Unavailable"/>.</summary>
        public UnavailableReason Reason { get; }

        public static MetricValue Available(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentException("Un valor Available es un double finito.", nameof(value));
            }

            return new MetricValue(MetricStatus.Available, value, null);
        }

        public static MetricValue NotApplicable() => new MetricValue(MetricStatus.NotApplicable, 0.0, null);

        public static MetricValue NotSupported() => new MetricValue(MetricStatus.NotSupported, 0.0, null);

        public static MetricValue Unavailable(UnavailableReason reason)
        {
            if (reason == null)
            {
                throw new ArgumentNullException(nameof(reason));
            }

            return new MetricValue(MetricStatus.Unavailable, 0.0, reason);
        }

        public bool Equals(MetricValue other)
            => other != null
               && Status == other.Status
               && Value.Equals(other.Value)
               && Equals(Reason, other.Reason);

        public override bool Equals(object obj) => Equals(obj as MetricValue);

        public override int GetHashCode() => (Status.GetHashCode() * 397) ^ Value.GetHashCode();

        public override string ToString()
            => Status == MetricStatus.Available ? "Available(" + Value + ")"
                : Status == MetricStatus.Unavailable ? "Unavailable(" + Reason + ")"
                : Status.ToString();
    }

    /// <summary>
    /// Resultado de la peticion por rack: un <see cref="MetricValue"/> por CADA metrica <c>(Rack, *)</c> del
    /// catalogo (I-63 D-08), en el orden del catalogo.
    /// </summary>
    public sealed class RackMetricResults
    {
        private readonly Dictionary<MetricId, MetricValue> _values;

        private RackMetricResults(string rackId, Dictionary<MetricId, MetricValue> values)
        {
            RackId = rackId;
            _values = values;
        }

        /// <summary>La grafia canonica del RackId (D-11), o null si ninguna hermana tiene identidad atribuible.</summary>
        public string RackId { get; }

        public MetricValue this[MetricId metric]
        {
            get
            {
                if (!_values.TryGetValue(metric, out var value))
                {
                    throw new KeyNotFoundException("La metrica " + metric + " no es una metrica de rack del catalogo.");
                }

                return value;
            }
        }

        public bool TryGet(MetricId metric, out MetricValue value) => _values.TryGetValue(metric, out value);

        /// <summary>Las metricas presentes, en el orden del catalogo.</summary>
        public IReadOnlyList<MetricId> Metrics
            => RackMetricIds.RackMetrics.Where(_values.ContainsKey).ToList();

        /// <summary>Construye los resultados exigiendo un valor para CADA metrica <c>(Rack, *)</c>.</summary>
        public static RackMetricResults Create(string rackId, IReadOnlyDictionary<MetricId, MetricValue> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var copy = new Dictionary<MetricId, MetricValue>();
            foreach (var metric in RackMetricIds.RackMetrics)
            {
                if (!values.TryGetValue(metric, out var value) || value == null)
                {
                    throw new ArgumentException("Falta el valor de la metrica " + metric + ".", nameof(values));
                }

                copy.Add(metric, value);
            }

            return new RackMetricResults(rackId, copy);
        }

        /// <summary>Todas las metricas <c>(Rack, *)</c> con el mismo valor.</summary>
        public static RackMetricResults Uniform(string rackId, MetricValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Create(rackId, RackMetricIds.RackMetrics.ToDictionary(metric => metric, metric => value));
        }
    }

    /// <summary>Como termino la carga del catalogo (I-63 D-27 / D-25). Un fallo de carga NO es un catalogo valido vacio.</summary>
    public sealed class RackCatalogInput
    {
        private RackCatalogInput(RackCatalog catalog)
        {
            Catalog = catalog;
        }

        /// <summary>El catalogo cargado. Nulo cuando la carga fallo.</summary>
        public RackCatalog Catalog { get; }

        public bool IsLoaded => Catalog != null;

        /// <summary>Un catalogo cargado (puede ser un catalogo VALIDO VACIO).</summary>
        public static RackCatalogInput Loaded(RackCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog), "Un catalogo nulo no es Loaded: usa LoadFailed().");
            }

            return new RackCatalogInput(catalog);
        }

        public static RackCatalogInput LoadFailed() => new RackCatalogInput(null);
    }

    /// <summary>Soporte de diseno de una metrica para un kind (I-63 D-06). Estado del catalogo, independiente de los datos.</summary>
    public enum RackMetricSupport
    {
        Supported = 1,
        NotSupported = 2,
        NotApplicable = 3,
    }

    /// <summary>Fase minima que necesita una metrica <c>Supported</c> (I-63 D-13).</summary>
    public enum RackMetricPhase
    {
        /// <summary>Phi3: diseno efectivo y resuelto.</summary>
        Resolved = 3,
    }

    /// <summary>Declaracion de un provider sobre una metrica: su soporte y, si es <c>Supported</c>, su fase minima.</summary>
    public sealed class RackMetricDeclaration
    {
        private RackMetricDeclaration(RackMetricSupport support, RackMetricPhase? minimumPhase)
        {
            Support = support;
            MinimumPhase = minimumPhase;
        }

        public RackMetricSupport Support { get; }

        /// <summary>Nula para <c>NotSupported</c> y <c>NotApplicable</c>: se emiten sin fase ni coste.</summary>
        public RackMetricPhase? MinimumPhase { get; }

        public static RackMetricDeclaration Supported(RackMetricPhase minimumPhase)
            => new RackMetricDeclaration(RackMetricSupport.Supported, minimumPhase);

        public static RackMetricDeclaration NotSupported()
            => new RackMetricDeclaration(RackMetricSupport.NotSupported, null);

        public static RackMetricDeclaration NotApplicable()
            => new RackMetricDeclaration(RackMetricSupport.NotApplicable, null);
    }

    /// <summary>
    /// Lo que el orquestador ya resolvio para el rack y entrega a un provider (I-63 D-08): o el sistema resuelto
    /// (Phi3), o la razon por la que no hay.
    /// </summary>
    public sealed class RackMetricPrerequisite
    {
        private RackMetricPrerequisite(SelectiveRackSystem system, UnavailableReason failure)
        {
            ResolvedSystem = system;
            Failure = failure;
        }

        /// <summary>El sistema resuelto del Selectivo. Nulo cuando <see cref="Failure"/> existe.</summary>
        public SelectiveRackSystem ResolvedSystem { get; }

        /// <summary>La razon por la que no hay sistema resuelto.</summary>
        public UnavailableReason Failure { get; }

        public bool IsResolved => Failure == null;

        public static RackMetricPrerequisite Resolved(SelectiveRackSystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            return new RackMetricPrerequisite(system, null);
        }

        public static RackMetricPrerequisite Unavailable(UnavailableReason failure)
        {
            if (failure == null)
            {
                throw new ArgumentNullException(nameof(failure));
            }

            return new RackMetricPrerequisite(null, failure);
        }
    }

    /// <summary>La entrada de un provider. El <see cref="Prerequisite"/> es nulo cuando ninguna metrica es <c>Supported</c> para el kind.</summary>
    public sealed class RackMetricInput
    {
        public RackMetricInput(string rackId, string kindToken, RackMetricPrerequisite prerequisite)
        {
            RackId = rackId;
            KindToken = kindToken;
            Prerequisite = prerequisite;
        }

        public string RackId { get; }

        public string KindToken { get; }

        public RackMetricPrerequisite Prerequisite { get; }
    }
}
