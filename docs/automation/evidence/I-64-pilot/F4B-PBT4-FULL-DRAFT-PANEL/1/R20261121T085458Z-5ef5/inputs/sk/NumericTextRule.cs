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

        /// <summary>Checks <paramref name="text"/> against the rules of one field. INERT in the RED of PB-T4b: the GREEN moves the rule here.</summary>
        public static NumericTextCheck Check(
            string text,
            bool isOptional,
            bool integerOnly,
            double? minimum,
            double? maximum,
            bool minimumInclusive,
            bool maximumInclusive) => new NumericTextCheck(NumericTextStatus.NotANumber, null, null);
    }
}
