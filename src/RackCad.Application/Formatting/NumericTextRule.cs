using System.Globalization;

namespace RackCad.Application.Formatting
{
    /// <summary>
    /// I-64 F4B PB-T4b (D-PBT4-06) — the outcome of one numeric text under <see cref="NumericTextRule"/>. The six statuses are the ones of the
    /// numeric field of the UI, in the same order, because the UI delegates to this rule and translates the status by its position.
    /// </summary>
    public enum NumericTextStatus
    {
        /// <summary>A number that satisfies every rule.</summary>
        Valid,

        /// <summary>Blank and allowed to be blank: valid with a null value.</summary>
        EmptyOptional,

        /// <summary>Blank but required: invalid.</summary>
        EmptyRequired,

        /// <summary>Non-blank but not a number under the localized parser.</summary>
        NotANumber,

        /// <summary>A number below the configured minimum.</summary>
        BelowMinimum,

        /// <summary>A number above the configured maximum.</summary>
        AboveMaximum,
    }

    /// <summary>The immutable result of <see cref="NumericTextRule.Check"/>: the status, the parsed value and the message to show.</summary>
    public readonly struct NumericTextCheck
    {
        internal NumericTextCheck(NumericTextStatus status, double? value, string message)
        {
            Status = status;
            Value = value;
            Message = message;
        }

        public NumericTextStatus Status { get; }

        /// <summary>The parsed number: null when blank; the offending value for the out-of-range statuses.</summary>
        public double? Value { get; }

        /// <summary>The Spanish message, or null when there is nothing to report.</summary>
        public string Message { get; }

        /// <summary>True for a valid number, or a blank text on an optional field.</summary>
        public bool IsValid => Status == NumericTextStatus.Valid || Status == NumericTextStatus.EmptyOptional;
    }

    /// <summary>
    /// I-64 F4B PB-T4b (D-PBT4-06; B-02, B-07) — the one rule of numeric entry: the "point or comma, no grouping" parsing of
    /// <see cref="LocalizedNumberParser"/>, the optional (auto) convention and the rejection of a number out of range. It moved here, unchanged,
    /// from the numeric field of the UI, which keeps its public surface and delegates to it; the rule of the annotation scale of Push Back uses it
    /// too. Pure: no UI and no state.
    /// </summary>
    public static class NumericTextRule
    {
        public const string EmptyRequiredMessage = "Escribe un valor.";
        public const string NotANumberMessage = "Valor no numérico.";

        /// <summary>Checks <paramref name="text"/> against the rules of one field.</summary>
        public static NumericTextCheck Check(
            string text,
            bool isOptional,
            bool integerOnly,
            double? minimum,
            double? maximum,
            bool minimumInclusive,
            bool maximumInclusive)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return isOptional
                    ? new NumericTextCheck(NumericTextStatus.EmptyOptional, null, null)
                    : new NumericTextCheck(NumericTextStatus.EmptyRequired, null, EmptyRequiredMessage);
            }

            double parsed;
            if (integerOnly)
            {
                if (!LocalizedNumberParser.TryInteger(text, out var integer))
                {
                    return new NumericTextCheck(NumericTextStatus.NotANumber, null, NotANumberMessage);
                }

                parsed = integer;
            }
            else if (!LocalizedNumberParser.TryDouble(text, out parsed))
            {
                return new NumericTextCheck(NumericTextStatus.NotANumber, null, NotANumberMessage);
            }

            if (minimum.HasValue)
            {
                var below = minimumInclusive ? parsed < minimum.Value : parsed <= minimum.Value;
                if (below)
                {
                    return new NumericTextCheck(NumericTextStatus.BelowMinimum, parsed, MinimumMessage(minimum.Value, minimumInclusive));
                }
            }

            if (maximum.HasValue)
            {
                var above = maximumInclusive ? parsed > maximum.Value : parsed >= maximum.Value;
                if (above)
                {
                    return new NumericTextCheck(NumericTextStatus.AboveMaximum, parsed, MaximumMessage(maximum.Value, maximumInclusive));
                }
            }

            return new NumericTextCheck(NumericTextStatus.Valid, parsed, null);
        }

        private static string MinimumMessage(double minimum, bool inclusive)
            => $"El valor debe ser {(inclusive ? "≥" : ">")} {Format(minimum)}.";

        private static string MaximumMessage(double maximum, bool inclusive)
            => $"El valor debe ser {(inclusive ? "≤" : "<")} {Format(maximum)}.";

        private static string Format(double value) => value.ToString("0.###", CultureInfo.CurrentCulture);
    }
}
