using System;
using System.Collections.Generic;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>Ambito de una metrica del catalogo (I-63 D-02). No es un namespace de expresiones.</summary>
    public enum MetricScope
    {
        Rack = 1,
        Project = 2,
    }

    /// <summary>
    /// Identidad de una metrica del catalogo (I-63 D-02): <c>(MetricScope, token)</c>. El token es ASCII, empieza
    /// por minuscula, se compara con <see cref="StringComparison.Ordinal"/> y se declara explicitamente: nunca se
    /// deriva de un nombre visible, de <c>enum.ToString()</c> ni de <c>nameof</c>. G1 no crea <c>SymbolId</c>.
    /// </summary>
    public readonly struct MetricId : IEquatable<MetricId>
    {
        public MetricId(MetricScope scope, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Un MetricId declara un token no vacio.", nameof(token));
            }

            Scope = scope;
            Token = token;
        }

        public MetricScope Scope { get; }

        public string Token { get; }

        public bool Equals(MetricId other)
            => Scope == other.Scope && string.Equals(Token, other.Token, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is MetricId other && Equals(other);

        public override int GetHashCode() => (Scope.GetHashCode() * 397) ^ StringComparer.Ordinal.GetHashCode(Token ?? string.Empty);

        public override string ToString() => Scope + "/" + Token;

        public static bool operator ==(MetricId left, MetricId right) => left.Equals(right);

        public static bool operator !=(MetricId left, MetricId right) => !left.Equals(right);
    }

    /// <summary>
    /// Catalogo V1 cerrado (I-63 D-05): los seis <see cref="MetricId"/>, con sus tokens congelados. Ampliarlo exige
    /// una enmienda o una iniciativa.
    /// </summary>
    public static class RackMetricIds
    {
        public const string FrentesToken = "frentes";
        public const string FrentesVaciosToken = "frentesVacios";
        public const string TotalRacksToken = "totalRacks";
        public const string RackCountToken = "rackCount";
        public const string TotalFrentesToken = "totalFrentes";
        public const string TotalFrentesVaciosToken = "totalFrentesVacios";

        public static readonly MetricId Frentes = new MetricId(MetricScope.Rack, FrentesToken);
        public static readonly MetricId FrentesVacios = new MetricId(MetricScope.Rack, FrentesVaciosToken);
        public static readonly MetricId TotalRacks = new MetricId(MetricScope.Project, TotalRacksToken);
        public static readonly MetricId RackCount = new MetricId(MetricScope.Project, RackCountToken);
        public static readonly MetricId TotalFrentes = new MetricId(MetricScope.Project, TotalFrentesToken);
        public static readonly MetricId TotalFrentesVacios = new MetricId(MetricScope.Project, TotalFrentesVaciosToken);

        /// <summary>Las seis metricas del catalogo, en el orden de D-05.</summary>
        public static IReadOnlyList<MetricId> All { get; } = new[]
        {
            Frentes, FrentesVacios, TotalRacks, RackCount, TotalFrentes, TotalFrentesVacios,
        };

        /// <summary>Las metricas <c>(Rack, *)</c>: las que calcula la peticion por rack, en el orden de D-05.</summary>
        public static IReadOnlyList<MetricId> RackMetrics { get; } = new[] { Frentes, FrentesVacios };
    }
}
