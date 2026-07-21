using System.Globalization;
using RackCad.Application.Formatting;

namespace RackCad.UI.Controls
{
    /// <summary>The outcome of validating one <see cref="NumericField"/> entry.</summary>
    public enum NumericFieldStatus
    {
        /// <summary>A number that satisfies every rule.</summary>
        Valid,

        /// <summary>Blank and allowed to be blank (optional → "auto"): valid with a null value.</summary>
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

    /// <summary>Immutable result of <see cref="NumericFieldValidation.Validate"/>: the status, the parsed value
    /// (null when blank-optional) and a ready-to-show Spanish message (null when there is nothing to report).</summary>
    public readonly struct NumericFieldValidationResult
    {
        internal NumericFieldValidationResult(NumericFieldStatus status, double? value, string message)
        {
            Status = status;
            Value = value;
            Message = message;
        }

        public NumericFieldStatus Status { get; }

        /// <summary>The parsed number. Null for <see cref="NumericFieldStatus.EmptyOptional"/> and
        /// <see cref="NumericFieldStatus.EmptyRequired"/>; the offending value is still reported for
        /// out-of-range statuses so callers can echo it.</summary>
        public double? Value { get; }

        /// <summary>A localized message for the error banner, or null when there is nothing to show.</summary>
        public string Message { get; }

        /// <summary>True when the entry is acceptable: a valid number, or blank on an optional field.</summary>
        public bool IsValid => Status == NumericFieldStatus.Valid || Status == NumericFieldStatus.EmptyOptional;

        public bool HasError => !IsValid;
    }

    /// <summary>
    /// The single validation rule for numeric entry, shared by every <see cref="NumericField"/> so the "point or
    /// comma, no grouping" parsing (via <see cref="LocalizedNumberParser"/>), the optional→auto convention and the
    /// range rejection ("rechaza si no cabe") live in ONE testable place instead of being re-implemented per window.
    /// Pure: no WPF, no dispatcher — exercised directly by the UI tests.
    /// </summary>
    public static class NumericFieldValidation
    {
        public const string EmptyRequiredMessage = "Escribe un valor.";
        public const string NotANumberMessage = "Valor no numérico.";

        /// <summary>
        /// Validates <paramref name="text"/> against the field's rules.
        /// </summary>
        /// <param name="text">The raw user text (localized decimal, no thousands separators).</param>
        /// <param name="isOptional">When true a blank entry is valid and yields a null value ("auto").</param>
        /// <param name="integerOnly">When true only whole numbers parse.</param>
        /// <param name="minimum">Inclusive/exclusive lower bound, or null for none.</param>
        /// <param name="maximum">Inclusive/exclusive upper bound, or null for none.</param>
        /// <param name="minimumInclusive">Whether <paramref name="minimum"/> itself is allowed.</param>
        /// <param name="maximumInclusive">Whether <paramref name="maximum"/> itself is allowed.</param>
        public static NumericFieldValidationResult Validate(
            string text,
            bool isOptional,
            bool integerOnly,
            double? minimum,
            double? maximum,
            bool minimumInclusive = true,
            bool maximumInclusive = true)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return isOptional
                    ? new NumericFieldValidationResult(NumericFieldStatus.EmptyOptional, null, null)
                    : new NumericFieldValidationResult(NumericFieldStatus.EmptyRequired, null, EmptyRequiredMessage);
            }

            double parsed;
            if (integerOnly)
            {
                if (!LocalizedNumberParser.TryInteger(text, out var integer))
                {
                    return new NumericFieldValidationResult(NumericFieldStatus.NotANumber, null, NotANumberMessage);
                }

                parsed = integer;
            }
            else if (!LocalizedNumberParser.TryDouble(text, out parsed))
            {
                return new NumericFieldValidationResult(NumericFieldStatus.NotANumber, null, NotANumberMessage);
            }

            if (minimum.HasValue)
            {
                var below = minimumInclusive ? parsed < minimum.Value : parsed <= minimum.Value;
                if (below)
                {
                    return new NumericFieldValidationResult(
                        NumericFieldStatus.BelowMinimum, parsed, MinimumMessage(minimum.Value, minimumInclusive));
                }
            }

            if (maximum.HasValue)
            {
                var above = maximumInclusive ? parsed > maximum.Value : parsed >= maximum.Value;
                if (above)
                {
                    return new NumericFieldValidationResult(
                        NumericFieldStatus.AboveMaximum, parsed, MaximumMessage(maximum.Value, maximumInclusive));
                }
            }

            return new NumericFieldValidationResult(NumericFieldStatus.Valid, parsed, null);
        }

        private static string MinimumMessage(double minimum, bool inclusive)
            => $"El valor debe ser {(inclusive ? "≥" : ">")} {Format(minimum)}.";

        private static string MaximumMessage(double maximum, bool inclusive)
            => $"El valor debe ser {(inclusive ? "≤" : "<")} {Format(maximum)}.";

        private static string Format(double value) => value.ToString("0.###", CultureInfo.CurrentCulture);
    }
}
