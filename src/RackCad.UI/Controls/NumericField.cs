using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// A text box that validates its own content as a localized measurement (point or comma, no grouping) with an
    /// optional range, exposing the parsed <see cref="Value"/>, an <see cref="HasError"/> flag and a localized
    /// <see cref="ErrorMessage"/>. It concentrates the "parse + range + red error" idiom that every window repeats
    /// on top of <see cref="UiSupport.TryNum"/>. <see cref="Value"/> is an OUTPUT (read the parse; set the field via
    /// <see cref="SetNumber"/> or <see cref="System.Windows.Controls.TextBox.Text"/>), which keeps it free of the
    /// two-way re-entrancy the configurator view-model has to guard against.
    /// Styling stays compatible with <c>FieldBox</c>: on error only the border turns red (#B00020) and reverts to the
    /// applied style when the entry becomes valid again.
    /// </summary>
    public class NumericField : TextBox
    {
        private static readonly Brush ErrorBorderBrush = UiSupport.FrozenBrush(Color.FromRgb(0xB0, 0x00, 0x20));

        private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value), typeof(double?), typeof(NumericField), new PropertyMetadata(null));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey HasErrorPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(HasError), typeof(bool), typeof(NumericField), new PropertyMetadata(false));

        public static readonly DependencyProperty HasErrorProperty = HasErrorPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ErrorMessagePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ErrorMessage), typeof(string), typeof(NumericField), new PropertyMetadata(null));

        public static readonly DependencyProperty ErrorMessageProperty = ErrorMessagePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey StatusPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Status), typeof(NumericFieldStatus), typeof(NumericField),
            new PropertyMetadata(NumericFieldStatus.EmptyOptional));

        public static readonly DependencyProperty StatusProperty = StatusPropertyKey.DependencyProperty;

        public static readonly DependencyProperty IsOptionalProperty = DependencyProperty.Register(
            nameof(IsOptional), typeof(bool), typeof(NumericField), new PropertyMetadata(false, OnRulesChanged));

        public static readonly DependencyProperty IntegerOnlyProperty = DependencyProperty.Register(
            nameof(IntegerOnly), typeof(bool), typeof(NumericField), new PropertyMetadata(false, OnRulesChanged));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double?), typeof(NumericField), new PropertyMetadata(null, OnRulesChanged));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double?), typeof(NumericField), new PropertyMetadata(null, OnRulesChanged));

        public static readonly DependencyProperty MinimumInclusiveProperty = DependencyProperty.Register(
            nameof(MinimumInclusive), typeof(bool), typeof(NumericField), new PropertyMetadata(true, OnRulesChanged));

        public static readonly DependencyProperty MaximumInclusiveProperty = DependencyProperty.Register(
            nameof(MaximumInclusive), typeof(bool), typeof(NumericField), new PropertyMetadata(true, OnRulesChanged));

        public NumericField()
        {
            // Validate the initial (blank) content so Value/Status reflect reality before any keystroke.
            Revalidate();
        }

        /// <summary>Raised after every (re)validation with the fresh result.</summary>
        public event EventHandler<NumericFieldValidationResult> Validated;

        /// <summary>The parsed number, or null when blank on an optional field. Read-only output.</summary>
        public double? Value => (double?)GetValue(ValueProperty);

        public bool HasError => (bool)GetValue(HasErrorProperty);

        public string ErrorMessage => (string)GetValue(ErrorMessageProperty);

        public NumericFieldStatus Status => (NumericFieldStatus)GetValue(StatusProperty);

        /// <summary>When true a blank entry is valid and yields a null <see cref="Value"/> ("auto").</summary>
        public bool IsOptional
        {
            get => (bool)GetValue(IsOptionalProperty);
            set => SetValue(IsOptionalProperty, value);
        }

        public bool IntegerOnly
        {
            get => (bool)GetValue(IntegerOnlyProperty);
            set => SetValue(IntegerOnlyProperty, value);
        }

        public double? Minimum
        {
            get => (double?)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double? Maximum
        {
            get => (double?)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public bool MinimumInclusive
        {
            get => (bool)GetValue(MinimumInclusiveProperty);
            set => SetValue(MinimumInclusiveProperty, value);
        }

        public bool MaximumInclusive
        {
            get => (bool)GetValue(MaximumInclusiveProperty);
            set => SetValue(MaximumInclusiveProperty, value);
        }

        /// <summary>Writes a number (or blank for null) into the box using an invariant-friendly format, then
        /// revalidates. Use this to seed the field from the model without fighting the current culture.</summary>
        public void SetNumber(double? value, string format = "0.###")
        {
            Text = value.HasValue
                ? value.Value.ToString(format, System.Globalization.CultureInfo.CurrentCulture)
                : string.Empty;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            Revalidate();
        }

        private static void OnRulesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((NumericField)d).Revalidate();

        private void Revalidate()
        {
            var result = NumericFieldValidation.Validate(
                Text, IsOptional, IntegerOnly, Minimum, Maximum, MinimumInclusive, MaximumInclusive);

            SetValue(ValuePropertyKey, result.Value);
            SetValue(StatusPropertyKey, result.Status);
            SetValue(HasErrorPropertyKey, result.HasError);
            SetValue(ErrorMessagePropertyKey, result.Message);

            if (result.HasError)
            {
                BorderBrush = ErrorBorderBrush;
            }
            else
            {
                // Revert to whatever the applied style/default supplies, rather than a guessed color.
                ClearValue(BorderBrushProperty);
            }

            Validated?.Invoke(this, result);
        }
    }
}
