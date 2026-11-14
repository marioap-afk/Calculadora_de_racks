#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T3A (D-T3-03): the text that shows the committed value of the field. It is the same decimal number as the round-trip
    /// text of the value, moved to plain digits (no exponent, no thousands separator) and written with the decimal separator of the
    /// current culture, so that the number parser of the application reads back exactly the same value and the validation of the
    /// field accepts it. Showing a value never creates pending text: this helper only builds a string.
    /// </summary>
    public static class WorkspacePilotValueText
    {
        /// <summary>
        /// The text of <paramref name="value"/>, or null with the <paramref name="reason"/> when the value is not finite or not greater
        /// than zero (the field accepts no other value).
        /// </summary>
        public static string? Show(double value, out string? reason)
        {
            reason = null;
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0.0)
            {
                reason = "El valor no es un número finito mayor que cero.";
                return null;
            }

            var plain = ToPlainDigits(value.ToString("R", CultureInfo.InvariantCulture));
            return plain.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal);
        }

        // The invariant round-trip text is "digits", "digits.digits" or the same with an exponent ("E+20", "E-05"). The point moves by the
        // exponent over the string of digits, padding with zeros, and the zeros that are not significant are removed.
        private static string ToPlainDigits(string roundTrip)
        {
            var exponentAt = roundTrip.IndexOf('E');
            var mantissa = exponentAt < 0 ? roundTrip : roundTrip.Substring(0, exponentAt);
            var exponent = exponentAt < 0
                ? 0
                : int.Parse(roundTrip.Substring(exponentAt + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

            var pointAt = mantissa.IndexOf('.');
            var digits = pointAt < 0 ? mantissa : mantissa.Remove(pointAt, 1);
            var integerDigits = (pointAt < 0 ? mantissa.Length : pointAt) + exponent;

            string integerPart;
            string fractionPart;
            if (integerDigits <= 0)
            {
                integerPart = "0";
                fractionPart = new string('0', -integerDigits) + digits;
            }
            else if (integerDigits >= digits.Length)
            {
                integerPart = digits + new string('0', integerDigits - digits.Length);
                fractionPart = string.Empty;
            }
            else
            {
                integerPart = digits.Substring(0, integerDigits);
                fractionPart = digits.Substring(integerDigits);
            }

            integerPart = integerPart.TrimStart('0');
            if (integerPart.Length == 0) integerPart = "0";
            fractionPart = fractionPart.TrimEnd('0');

            var text = new StringBuilder(integerPart);
            if (fractionPart.Length > 0) text.Append('.').Append(fractionPart);
            return text.ToString();
        }
    }

    /// <summary>
    /// I-64 F4-T3A (D-T3-05): the scope of a modal window of RackCad opened by the panel. It switches the "RackCad modal" flag of the
    /// host on before it returns and off when the last scope that is open is disposed: scopes nest, and disposing a scope twice has
    /// no further effect. It is pure: it only calls the action it receives.
    /// </summary>
    public sealed class WorkspacePanelModalScope
    {
        private readonly Action<bool> _setActive;
        private int _depth;

        public WorkspacePanelModalScope(Action<bool> setActive)
        {
            _setActive = setActive ?? throw new ArgumentNullException(nameof(setActive));
        }

        /// <summary>How many scopes are open now.</summary>
        public int Depth => _depth;

        /// <summary>Opens a scope. The flag is already on when this returns.</summary>
        public IDisposable Enter()
        {
            if (_depth == 0) _setActive(true);
            _depth++;
            return new WorkspacePanelModalHold(this);
        }

        internal void Leave()
        {
            if (_depth == 0) return;
            _depth--;
            if (_depth == 0) _setActive(false);
        }
    }

    /// <summary>The handle of one open <see cref="WorkspacePanelModalScope"/>; disposing it more than once is harmless.</summary>
    internal sealed class WorkspacePanelModalHold : IDisposable
    {
        private WorkspacePanelModalScope? _scope;

        public WorkspacePanelModalHold(WorkspacePanelModalScope scope)
        {
            _scope = scope;
        }

        public void Dispose()
        {
            var scope = _scope;
            _scope = null;
            scope?.Leave();
        }
    }

    /// <summary>
    /// I-64 F4-T3A (D-T3-06): how deep inside a notice of the panel the process is. It is a counter in memory and nothing more.
    /// </summary>
    public sealed class WorkspaceNoticeDepth
    {
        private int _depth;

        public int Value => _depth;

        /// <summary>True while some notice is being delivered.</summary>
        public bool IsInside => _depth > 0;

        public void Enter() => _depth++;

        public void Leave()
        {
            if (_depth > 0) _depth--;
        }
    }

    /// <summary>
    /// I-64 F4-T3A (D-T3-06): the decorator of the port that the panel receives. Inside a notice of the panel the operations that
    /// read the drawing (three since F4-T3A, six since F4-T4A1) answer Waiting without delegating and without reading; everything else
    /// is delegated as it is. So a handler of a state notice can never start a read, however it is written.
    /// </summary>
    public sealed class WorkspacePilotEditGate : IWorkspacePilotEditPort
    {
        /// <summary>The reason of the Waiting answers of the gate.</summary>
        public const string InsideNoticeReason = "Se está entregando un aviso del panel.";

        private readonly IWorkspacePilotEditPort _inner;
        private readonly WorkspaceNoticeDepth _depth;

        public WorkspacePilotEditGate(IWorkspacePilotEditPort inner, WorkspaceNoticeDepth depth)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _depth = depth ?? throw new ArgumentNullException(nameof(depth));
        }

        public EditIntentOutcome TryBeginEdit(EditIntentRequest request) =>
            _depth.IsInside ? EditIntentOutcome.Waiting(InsideNoticeReason) : _inner.TryBeginEdit(request);

        public EditCheckOutcome TryCheckNow(EditIntentRequest request) =>
            _depth.IsInside ? EditCheckOutcome.Waiting(InsideNoticeReason) : _inner.TryCheckNow(request);

        public EditIntentOutcome TryReload(EditIntentRequest request) =>
            _depth.IsInside ? EditIntentOutcome.Waiting(InsideNoticeReason) : _inner.TryReload(request);

        public EditConfirmOutcome TryConfirmBase(EditIntentRequest request) =>
            _depth.IsInside ? EditConfirmOutcome.Waiting(InsideNoticeReason) : _inner.TryConfirmBase(request);

        public EditIntentOutcome TryKeepMine(EditIntentRequest request) =>
            _depth.IsInside ? EditIntentOutcome.Waiting(InsideNoticeReason) : _inner.TryKeepMine(request);

        public EditIntentOutcome TryDiscardAndReload(EditIntentRequest request) =>
            _depth.IsInside ? EditIntentOutcome.Waiting(InsideNoticeReason) : _inner.TryDiscardAndReload(request);

        public PilotFieldView GetField(WorkspaceRequest origin, string rackId) => _inner.GetField(origin, rackId);

        public PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text) =>
            _inner.SetPendingText(origin, rackId, text);

        public PilotFieldResult CommitField(WorkspaceRequest origin, string rackId) => _inner.CommitField(origin, rackId);

        public PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind) =>
            _inner.CrossBoundary(origin, rackId, kind);

        public EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId) => _inner.DiscardRack(origin, rackId);

        public EditDiscardOutcome DiscardDocument(WorkspaceRequest origin) => _inner.DiscardDocument(origin);

        public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin) => _inner.GetRacksAtRisk(origin);

        public IReadOnlyList<PilotLossView> TakeLossNotices() => _inner.TakeLossNotices();

        public IReadOnlyList<PilotCloseView> TakeCloseNotices() => _inner.TakeCloseNotices();
    }
}
