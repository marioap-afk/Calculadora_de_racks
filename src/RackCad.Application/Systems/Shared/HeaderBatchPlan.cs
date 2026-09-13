using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// The opaque fingerprint of what a PREPARE read — topology, resolution and normalization inputs (I-53, contract §3.9).
    /// MUTATE recomputes it before the first assignment and refuses with <see cref="HeaderRejectionCode.StaleTargets"/> when
    /// it differs.
    /// <para>
    /// WHAT goes into it is each system's decision (G4, G6); this type only guarantees what the contract needs from any
    /// signature: it is immutable, and two signatures are equal exactly when they were built from the same ordered
    /// components, compared ordinally. The components are kept apart, so ["ab", "c"] and ["a", "bc"] never collide the way
    /// a joined string would.
    /// </para>
    /// </summary>
    public sealed class HeaderBatchSignature : IEquatable<HeaderBatchSignature>
    {
        private readonly string[] components;

        internal HeaderBatchSignature(IEnumerable<string> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            var copy = new List<string>();
            foreach (var component in components)
            {
                if (component == null)
                {
                    throw new ArgumentException("Una firma no admite componentes nulos.", nameof(components));
                }

                copy.Add(component);
            }

            if (copy.Count == 0)
            {
                throw new ArgumentException("Una firma necesita al menos un componente.", nameof(components));
            }

            this.components = copy.ToArray();
        }

        public bool Equals(HeaderBatchSignature other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (components.Length != other.components.Length)
            {
                return false;
            }

            for (var i = 0; i < components.Length; i++)
            {
                if (!string.Equals(components[i], other.components[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) => Equals(obj as HeaderBatchSignature);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var component in components)
            {
                hash.Add(component, StringComparer.Ordinal);
            }

            return hash.ToHashCode();
        }

        public static bool operator ==(HeaderBatchSignature left, HeaderBatchSignature right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(HeaderBatchSignature left, HeaderBatchSignature right) => !(left == right);

        /// <summary>For diagnostics only: the components, in order.</summary>
        public override string ToString() => string.Join(" | ", components);
    }

    /// <summary>
    /// One destination the taxonomy can address but that takes no copy, with its reason (contract §3.5). The address type
    /// is the consuming system's own; this type does not know what it means.
    /// </summary>
    public sealed class HeaderOmission<TAddress>
    {
        internal HeaderOmission(TAddress address, HeaderOmissionReason reason)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (!Enum.IsDefined(reason))
            {
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Motivo de omision fuera del conjunto cerrado.");
            }

            Address = address;
            Reason = reason;
        }

        public TAddress Address { get; }

        public HeaderOmissionReason Reason { get; }

        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", Address, Reason);
    }

    /// <summary>A non-blocking notice about one destination of a prepared plan (contract §3.4).</summary>
    public sealed class HeaderBatchWarning<TAddress>
    {
        internal HeaderBatchWarning(TAddress address, HeaderWarningSeverity severity, string message)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (!Enum.IsDefined(severity))
            {
                throw new ArgumentOutOfRangeException(nameof(severity), severity, "Severidad fuera del conjunto cerrado.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Un aviso necesita un texto para el usuario.", nameof(message));
            }

            Address = address;
            Severity = severity;
            Message = message;
        }

        public TAddress Address { get; }

        public HeaderWarningSeverity Severity { get; }

        /// <summary>What the user is told about this destination.</summary>
        public string Message { get; }

        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture, "{0}: {1} ({2})", Address, Message, Severity);
    }

    /// <summary>
    /// What a PREPARE produced (I-53, contract §3.4): <see cref="Rejected"/> or <see cref="Prepared"/>, and nothing else —
    /// the base constructor is private, so no third form can exist. Nothing is applied in either form.
    /// <para>
    /// It is generic over the consuming system's ADDRESS because the address, the content of the signature and the order
    /// are what differs between systems (contract §3.13); there is no universal destination type. It is a result, not an
    /// engine: no target resolver, PREPARE, MUTATE or callback lives here. Each system implements the protocol over its
    /// own state and states the result of its PREPARE with this type; the precedence of §3.7 is applied there too.
    /// </para>
    /// </summary>
    public abstract class HeaderBatchPlan<TAddress>
    {
        private HeaderBatchPlan()
        {
        }

        /// <summary>The whole request is invalid or unsafe: nothing prepared, nothing applied, no destinations.</summary>
        public sealed class Rejected : HeaderBatchPlan<TAddress>
        {
            internal Rejected(HeaderRejectionCode code)
            {
                if (!Enum.IsDefined(code))
                {
                    throw new ArgumentOutOfRangeException(nameof(code), code, "Codigo de rechazo fuera del conjunto cerrado.");
                }

                Code = code;
            }

            public HeaderRejectionCode Code { get; }
        }

        /// <summary>
        /// Everything is prepared and nothing is applied yet. There is deliberately no <c>Applied</c> here: only a committed
        /// outcome applies, and it is built from this plan (<see cref="HeaderBatchOutcome{TAddress}.Committed"/>).
        /// <para>
        /// It is immutable and it copies its inputs, so a plan that lives one gesture cannot be changed by the consumer's
        /// own lists. The orders are the consumer's deterministic ones (contract §3.12); nothing is re-sorted here.
        /// </para>
        /// </summary>
        public sealed class Prepared : HeaderBatchPlan<TAddress>
        {
            internal Prepared(
                IEnumerable<TAddress> targets,
                IEnumerable<HeaderOmission<TAddress>> omitted,
                IEnumerable<HeaderBatchWarning<TAddress>> warnings,
                HeaderBatchSignature signature)
            {
                Targets = ReadOnlyCopy(targets, nameof(targets));
                if (Targets.Count == 0)
                {
                    // Zero applicable destinations is a rejection (NoTargets or NoApplicableTargets), never a plan.
                    throw new ArgumentException("Un plan preparado necesita al menos un destino aplicable.", nameof(targets));
                }

                Omitted = ReadOnlyCopy(omitted, nameof(omitted));
                Warnings = ReadOnlyCopy(warnings, nameof(warnings));
                Signature = signature ?? throw new ArgumentNullException(nameof(signature));

                foreach (var warning in Warnings)
                {
                    if (warning.Severity == HeaderWarningSeverity.Severe)
                    {
                        RequiresConfirmation = true;
                        break;
                    }
                }
            }

            /// <summary>The applicable destinations, in the consumer's deterministic order.</summary>
            public IReadOnlyList<TAddress> Targets { get; }

            /// <summary>The omitted destinations with their reasons, in the consumer's deterministic order.</summary>
            public IReadOnlyList<HeaderOmission<TAddress>> Omitted { get; }

            /// <summary>The non-blocking notices, in the consumer's deterministic order.</summary>
            public IReadOnlyList<HeaderBatchWarning<TAddress>> Warnings { get; }

            /// <summary>What this PREPARE read; MUTATE compares it before the first assignment.</summary>
            public HeaderBatchSignature Signature { get; }

            /// <summary>True if and only if at least one notice is <see cref="HeaderWarningSeverity.Severe"/>.</summary>
            public bool RequiresConfirmation { get; }
        }

        private static IReadOnlyList<T> ReadOnlyCopy<T>(IEnumerable<T> items, string parameterName)
        {
            if (items == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            var copy = new List<T>();
            foreach (var item in items)
            {
                if (item == null)
                {
                    throw new ArgumentException("Un plan no admite elementos nulos.", parameterName);
                }

                copy.Add(item);
            }

            return new ReadOnlyCollection<T>(copy);
        }
    }
}
