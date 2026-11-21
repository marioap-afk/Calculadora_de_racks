"""PB-T4b (paquete v4.4): el texto literal del GREEN de src/ que prescribe el paquete, construido en memoria.

Módulo: green(repo, rev) -> (base, green): dos dict ruta -> texto (LF). Solo `git show` (objetos git).
- Si la revisión no trae todavía el GREEN de PB-T4a (no tiene IWorkspaceEditIntentOriginReader.cs; por ejemplo 5aaacce3, la emisión de PB-T4a),
  se le aplica antes el GREEN literal de PB-T4a (green_pbt4a.py). Si lo trae (el cierre de PB-T4a), se leen sus archivos tal cual.
- Cada sustitución tiene que casar exactamente una vez (o las veces que dice). Los archivos nuevos no están en `base`.
Las decisiones que aplica: D-PBT4-04 a 09 del paquete, con D-S2 (dos «Sí») y D-S3 (el alias, el testigo desde Draft/) de la sesión.
Ningún `?` de tipo de referencia fuera de un archivo con `#nullable enable` (CA-T4a I-01): WHC y WEI lo llevan; FVM, NumericFieldValidation.cs y los
cuatro archivos nuevos, no.
"""
import importlib.util
import os
import subprocess

HERE = os.path.dirname(os.path.abspath(__file__))
WHC_P = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
WEI_P = 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs'
FVM_P = 'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs'
PV_P = 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml'
NFV_P = 'src/RackCad.UI/Controls/NumericFieldValidation.cs'
RULE_P = 'src/RackCad.Application/Formatting/NumericTextRule.cs'
DRAFT = 'src/RackCad.Application/Systems/PushBack/Draft/'
SCALE_P = DRAFT + 'PushBackAnnotationScaleRule.cs'
ADAPTER_P = DRAFT + 'PushBackFullFieldAdapter.cs'
PRES_P = DRAFT + 'PushBackKeepMinePresentation.cs'
IFACE_P = 'src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs'
EXISTING = [WHC_P, WEI_P, FVM_P, PV_P, NFV_P]
NEW = [RULE_P, SCALE_P, ADAPTER_P, PRES_P]


def _show(repo, rev, path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    if p.returncode:
        return None
    return p.stdout.decode('utf-8').replace('\r\n', '\n')


def _sub(text, old, new, what, times=1):
    n = text.count(old)
    assert n == times, '%s: %d coincidencias (se esperaban %d)' % (what, n, times)
    return text.replace(old, new)


def t4a_base(repo, rev):
    """Los archivos de src/ que toca PB-T4b, con el GREEN de PB-T4a: de la revisión si ya lo trae, o aplicado en memoria."""
    if _show(repo, rev, IFACE_P) is None:
        spec = importlib.util.spec_from_file_location('gp4a', os.path.join(HERE, 'green_pbt4a.py'))
        gp = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(gp)
        _, g4a = gp.green(repo, rev)
        out = {p: g4a[p] if p in g4a else _show(repo, rev, p) for p in EXISTING}
        mode = 'GREEN de PB-T4a aplicado en memoria sobre ' + rev
    else:
        out = {p: _show(repo, rev, p) for p in EXISTING}
        mode = 'GREEN de PB-T4a leído de ' + rev
    for p, t in out.items():
        assert t is not None and t.count('\r') == 0, p
    return out, mode


# ---------------------------------------------------------------- los cuatro archivos nuevos (sin #nullable; Draft/ sin ninguna directiva)

RULE = '''using System.Globalization;

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
'''

SCALE = '''using RackCad.Application.Formatting;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4b (D-PBT4-06; B-02, B-07; Q-PBT2-3) — the one rule of the annotation scale of a Push Back rack: a required number greater
    /// than zero, checked by <see cref="NumericTextRule"/>, the rule the numeric boxes delegate to. The field of the panel takes it through the
    /// adapter of the full draft, by default: nothing assigns it.
    /// </summary>
    public static class PushBackAnnotationScaleRule
    {
        public const double Minimum = 0.0;

        public const bool MinimumInclusive = false;

        public static DraftFieldValidation Validate(string text)
        {
            var check = NumericTextRule.Check(text, false, false, Minimum, null, MinimumInclusive, true);
            return check.IsValid ? DraftFieldValidation.Valid : DraftFieldValidation.Invalid(check.Message);
        }
    }
}
'''

ADAPTER = '''using System;
using System.Collections.Generic;
using RackCad.Application.Formatting;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4b (D-PBT4-04, D-PBT4-06) — the pending-text adapter of the field of the panel on the FULL draft. It serves the one field of
    /// the panel, <see cref="AnnotationScaleKey"/>, with <see cref="PushBackAnnotationScaleRule"/>. It keeps no reference to any state, so one
    /// static adapter serves every draft; <see cref="Validate"/> changes nothing, and <see cref="Apply"/> edits the state it receives and hands
    /// back the same object, which is what the draft core expects of an adapter.
    /// </summary>
    public sealed class PushBackFullFieldAdapter : IDraftPendingTextAdapter<PushBackFullDraftState>
    {
        /// <summary>The only field key of the panel, compared ordinally. Any other key is invalid.</summary>
        public const string AnnotationScaleKey = "AnnotationScale";

        public const string EmptyMessage = NumericTextRule.EmptyRequiredMessage;

        public const string NotANumberMessage = NumericTextRule.NotANumberMessage;

        /// <summary>The message of the rule for a number that is not greater than zero.</summary>
        public const string NotPositiveMessage = "El valor debe ser > 0.";

        public const string UnknownFieldMessage = "Campo desconocido para el panel.";

        public DraftFieldValidation Validate(string fieldKey, string text)
        {
            if (!string.Equals(fieldKey, AnnotationScaleKey, StringComparison.Ordinal))
            {
                return DraftFieldValidation.Invalid(UnknownFieldMessage);
            }

            return PushBackAnnotationScaleRule.Validate(text);
        }

        public PushBackFullDraftState Apply(PushBackFullDraftState current, IReadOnlyDictionary<string, string> pendingText)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (pendingText == null) throw new ArgumentNullException(nameof(pendingText));

            // The core only calls Apply with text that already passed Validate; a value that does not, or another key, is left alone.
            if (pendingText.TryGetValue(AnnotationScaleKey, out var text)
                && Validate(AnnotationScaleKey, text).IsValid
                && LocalizedNumberParser.TryDouble(text, out var scale))
            {
                current.Editable.AnnotationScale = scale;
            }

            return current;
        }
    }
}
'''

PRES = '''using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4b (D-PBT4-08, D-PBT4-09; OB-05.3) — what "keep my draft" shows above its Yes and No: the changes BY GROUPS (the decision of
    /// the Owner, "Por grupos"), one line per class of change, the structural one and the ones of the views, in that order. Pure: it reads no
    /// drawing and changes nothing. <see cref="Of"/> turns a classification into lines; <see cref="Read"/> classifies a draft against the reread
    /// of its rack with the classifier of the full model and keeps the text of the envelope of the origin of that reread, which is the text of
    /// the witness that the confirmation hands back.
    /// </summary>
    public sealed class PushBackKeepMinePresentation
    {
        private PushBackKeepMinePresentation(IReadOnlyList<string> lines, string originText, string diagnostic)
        {
            Lines = lines;
            OriginText = originText;
            Diagnostic = diagnostic;
        }

        /// <summary>The lines, in the order of the table of D-PBT4-09, each with its final point.</summary>
        public IReadOnlyList<string> Lines { get; }

        /// <summary>True when there is no line.</summary>
        public bool IsEmpty => Lines.Count == 0;

        /// <summary>The raw text of the envelope of the origin of the classified reread; null for <see cref="Of"/> and without admission.</summary>
        public string OriginText { get; }

        /// <summary>The diagnostic of a reread that is not admitted, which is then not classified; null otherwise.</summary>
        public string Diagnostic { get; }

        /// <summary>True when the reread was admitted and classified.</summary>
        public bool IsAdmitted => Diagnostic == null;

        /// <summary>The lines of a classification: each group once, in the order of <see cref="PushBackChangeGroup"/>, joined with commas and a last " y ".</summary>
        public static PushBackKeepMinePresentation Of(PushBackConflictClassification classification)
        {
            if (classification == null) throw new ArgumentNullException(nameof(classification));

            var lines = new List<string>();
            AddGroups(lines, "Cambiado fuera: ", classification.ChangedExternally);
            AddGroups(lines, "Cambiado por ti: ", classification.ChangedByUser);
            AddGroups(lines, "Cambiado en los dos: ", classification.Overlapping);
            AddGroups(lines, "Cambios estructurales: ", classification.Structural);

            var erased = classification.ViewsToErase.Count;
            if (erased == 1) lines.Add("Vistas: se borrará 1 lateral obsoleto.");
            else if (erased > 1) lines.Add("Vistas: se borrarán " + erased.ToString(CultureInfo.InvariantCulture) + " laterales obsoletos.");

            var rejected = classification.ViewsRejected.Count;
            if (rejected == 1) lines.Add("Vistas: 1 no se puede conservar.");
            else if (rejected > 1) lines.Add("Vistas: " + rejected.ToString(CultureInfo.InvariantCulture) + " no se pueden conservar.");

            if (classification.AllViewsObsolete) lines.Add("Todas las vistas quedarían obsoletas.");
            return new PushBackKeepMinePresentation(lines.AsReadOnly(), null, null);
        }

        /// <summary>
        /// Classifies the draft against the reread of its rack: the base is the initial state of the draft, the current rack is the state of the
        /// reread, the draft is its current state, and the members are the mutable envelopes of the base of the reread. A reread that is not
        /// admitted is not classified: the presentation carries its diagnostic and no line. It changes neither the draft nor the reread.
        /// </summary>
        public static PushBackKeepMinePresentation Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (read == null) throw new ArgumentNullException(nameof(read));
            if (!read.Read.IsAdmitted)
            {
                return new PushBackKeepMinePresentation(
                    Array.Empty<string>(), null, read.Read.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic);
            }

            var current = read.Read.InitialState;
            var envelopes = new RackEmbedStore();
            var members = new List<KeyValuePair<string, RackEmbedDocument>>();
            foreach (var entry in read.StaleBase.Entries.Values
                         .Where(e => e.Membership == DraftMembership.Mutable && e.EnvelopeText != null)
                         .OrderBy(e => e.Key, StringComparer.Ordinal))
            {
                var embed = envelopes.Deserialize(entry.EnvelopeText);
                if (embed != null) members.Add(new KeyValuePair<string, RackEmbedDocument>(entry.Key, embed));
            }

            var classification = PushBackConflictClassifier.Classify(draft.InitialDraftState, current, draft.CurrentDraftState, members, catalog);
            return new PushBackKeepMinePresentation(Of(classification).Lines, current.Origin, null);
        }

        private static void AddGroups(List<string> lines, string prefix, IReadOnlyList<PushBackChangeEntry> entries)
        {
            var groups = entries.Select(entry => entry.Group).Distinct().OrderBy(group => (int)group).Select(TextOf).ToList();
            if (groups.Count == 0) return;
            lines.Add(prefix + Join(groups) + ".");
        }

        private static string Join(IReadOnlyList<string> items)
            => items.Count == 1 ? items[0] : string.Join(", ", items.Take(items.Count - 1)) + " y " + items[items.Count - 1];

        private static string TextOf(PushBackChangeGroup group)
        {
            switch (group)
            {
                case PushBackChangeGroup.Name: return "el nombre";
                case PushBackChangeGroup.Rack: return "el rack";
                case PushBackChangeGroup.Annotations: return "las anotaciones";
                case PushBackChangeGroup.Safety: return "la seguridad";
                case PushBackChangeGroup.Fronts: return "los frentes";
                case PushBackChangeGroup.Modules: return "los módulos";
                case PushBackChangeGroup.SideB: return "el lado B";
                case PushBackChangeGroup.Composite: return "la parte compuesta";
                default: return "fuera del editor";
            }
        }
    }
}
'''

# ---------------------------------------------------------------- NumericFieldValidation.cs: delega en la regla de Application (D-PBT4-06)

NFV_OLD_HEAD = 'using System.Globalization;\nusing RackCad.Application.Formatting;\n'
NFV_NEW_HEAD = 'using RackCad.Application.Formatting;\n'
NFV_OLD_CONSTS = ('        public const string EmptyRequiredMessage = "Escribe un valor.";\n'
                  '        public const string NotANumberMessage = "Valor no numérico.";\n')
NFV_NEW_CONSTS = ('        public const string EmptyRequiredMessage = NumericTextRule.EmptyRequiredMessage;\n'
                  '        public const string NotANumberMessage = NumericTextRule.NotANumberMessage;\n')
NFV_BODY_START = '            bool maximumInclusive = true)\n        {\n'
NFV_NEW_BODY = ('            bool maximumInclusive = true)\n'
                '        {\n'
                '            // I-64 F4B PB-T4b (D-PBT4-06): the one rule lives in Application (NumericTextRule); this keeps its public surface and only\n'
                '            // translates the result. The two enumerations have the same members in the same order.\n'
                '            var check = NumericTextRule.Check(text, isOptional, integerOnly, minimum, maximum, minimumInclusive, maximumInclusive);\n'
                '            return new NumericFieldValidationResult((NumericFieldStatus)(int)check.Status, check.Value, check.Message);\n'
                '        }\n'
                '    }\n'
                '}\n')

# ---------------------------------------------------------------- WEI: el estado Presented y lo que lleva (D-PBT4-08)

WEI_KEPT = ('        Kept,\n'
            '    }\n')
WEI_PRESENTED = ('        Kept,\n'
                 '\n'
                 '        // The state below belongs to the presentation of "keep my draft" (I-64 F4B PB-T4b, D-PBT4-08). It comes AFTER the eleven above,\n'
                 '        // which stay as an ordered prefix.\n'
                 '\n'
                 '        /// <summary>\n'
                 '        /// "Keep my draft" without a witness that still holds: nothing was kept and the draft did not change. The outcome carries the lines\n'
                 '        /// of the presentation, by groups, and the witness of the read they describe, which the confirmation hands back.\n'
                 '        /// </summary>\n'
                 '        Presented,\n'
                 '    }\n')
WEI_CTOR_END = ('            Detail = detail;\n'
                '            ClassicEditorHint = classicEditorHint;\n'
                '        }\n'
                '\n'
                '        public EditIntentStatus Status { get; }\n')
WEI_CTOR_END_NEW = ('            Detail = detail;\n'
                    '            ClassicEditorHint = classicEditorHint;\n'
                    '            _presentationLines = null;\n'
                    '            KeepWitness = null;\n'
                    '        }\n'
                    '\n'
                    '        // I-64 F4B PB-T4b (D-PBT4-08): the presentation of "keep my draft", with its lines and the witness of the read they describe.\n'
                    '        private EditIntentOutcome(EditIntentStatus status, EditWitnessStatus witness, IReadOnlyList<string> presentationLines, EditIntentWitness keepWitness)\n'
                    '        {\n'
                    '            Status = status;\n'
                    '            Witness = witness;\n'
                    '            Reason = null;\n'
                    '            Diagnostic = null;\n'
                    '            Detail = null;\n'
                    '            ClassicEditorHint = null;\n'
                    '            _presentationLines = presentationLines;\n'
                    '            KeepWitness = keepWitness;\n'
                    '        }\n'
                    '\n'
                    '        private readonly IReadOnlyList<string>? _presentationLines;\n'
                    '\n'
                    '        public EditIntentStatus Status { get; }\n')
WEI_HINT_PROP = ('        public string? ClassicEditorHint { get; }\n'
                 '\n'
                 '        public bool IsAdmitted => Status == EditIntentStatus.Admitted;\n')
WEI_HINT_PROP_NEW = ('        public string? ClassicEditorHint { get; }\n'
                     '\n'
                     '        /// <summary>The lines of the presentation of "keep my draft" (Presented); empty for every other state.</summary>\n'
                     '        public IReadOnlyList<string> PresentationLines => _presentationLines ?? Array.Empty<string>();\n'
                     '\n'
                     '        /// <summary>The witness of the read that the presentation describes (Presented): the key and the text of the envelope of its origin; null otherwise.</summary>\n'
                     '        public EditIntentWitness? KeepWitness { get; }\n'
                     '\n'
                     '        public bool IsAdmitted => Status == EditIntentStatus.Admitted;\n')
WEI_KEPT_FACTORY = ('        public static EditIntentOutcome Kept(EditWitnessStatus witness) =>\n'
                    '            new(EditIntentStatus.Kept, witness, null, null, null, null);\n')
WEI_PRESENTED_FACTORY = (WEI_KEPT_FACTORY +
                         '\n'
                         '        public static EditIntentOutcome Presented(IReadOnlyList<string> lines, EditIntentWitness keepWitness, EditWitnessStatus witness)\n'
                         '        {\n'
                         '            if (lines == null) throw new ArgumentNullException(nameof(lines));\n'
                         '            if (keepWitness == null) throw new ArgumentNullException(nameof(keepWitness));\n'
                         '            return new EditIntentOutcome(EditIntentStatus.Presented, witness, Array.AsReadOnly(lines.ToArray()), keepWitness);\n'
                         '        }\n')

# ---------------------------------------------------------------- WHC (D-PBT4-04, 07, 08; D-S2, D-S3)

WHC_SUBS = [
    ('using', 'using RackCad.Application.Systems.PushBack.Pilot;\n',
     'using RackCad.Application.Systems.PushBack.Draft;\nusing RackCad.Application.Systems.PushBack.Pilot;\n', 1),
    ('adaptador',
     '        // I-64 F4-T3A (D-T3-02): the one adapter of the pending text of the pilot. It keeps no state, so it is shared; it is never an\n'
     '        // instance field of the host.\n'
     '        private static readonly PushBackPilotFieldAdapter PilotFieldAdapter = new();\n',
     '        // I-64 F4-T3A (D-T3-02): the one adapter of the pending text of the field. It keeps no state, so it is shared; it is never an\n'
     '        // instance field of the host. Since I-64 F4B PB-T4b (D-PBT4-04, D-PBT4-06) it is the adapter of the full draft, with the one rule\n'
     '        // of the scale (PushBackAnnotationScaleRule), by default.\n'
     '        private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();\n', 1),
    ('motivo',
     '        private const string NotKeepableReason =',
     '        private const string FullReadMissingReason = "No editable aquí: la lectura no trae el modelo completo.";\n'
     '        private const string NotKeepableReason =', 1),
    # Begin
    ('begin',
     '                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);\n'
     '\n'
     '                // 8: the base of the read only goes to Begin, with the comparer and the copier of the pilot.\n'
     '                var begun = _drafts.Begin(\n'
     '                    request.Session,\n'
     '                    request.RackId,\n'
     '                    pilot.Read,\n'
     '                    PushBackPilotStateComparer.Instance,\n'
     '                    PushBackPilotStateComparer.Copy);\n',
     '                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);\n'
     '\n'
     '                // I-64 F4B PB-T4b (D-PBT4-04): the draft of the panel is the full one; a read without the full model begins no draft.\n'
     '                var full = read.FullRead;\n'
     '                if (full == null)\n'
     '                    return EditIntentOutcome.NotAdmitted(FullReadMissingReason, null, PushBackPilotIntentReader.ClassicEditorHint, witness);\n'
     '\n'
     '                // 8: the base of the read only goes to Begin, with the comparer and the copier of the full model.\n'
     '                var begun = _drafts.Begin(\n'
     '                    request.Session,\n'
     '                    request.RackId,\n'
     '                    full.Read,\n'
     '                    PushBackFullDraftStateComparer.Instance,\n'
     '                    PushBackFullDraftStateComparer.Copy);\n', 1),
    ('begin-detalle',
     '                            begun.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,\n'
     '                            pilot.Detail,\n'
     '                            pilot.ClassicEditorHint,\n',
     '                            begun.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,\n'
     '                            full.Detail,\n'
     '                            full.ClassicEditorHint,\n', 1),
    # Check
    ('check',
     '                var pilot = read.PilotRead;\n'
     '                if (pilot == null) return EditCheckOutcome.Failed("La relectura no trajo resultado.");\n',
     '                var full = read.FullRead;\n'
     '                if (full == null) return EditCheckOutcome.Failed(FullReadMissingReason);\n', 1),
    ('check-diagnostico',
     '                    admission = pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic;\n'
     '                    hint = admission == null ? null : pilot.ClassicEditorHint;\n',
     '                    admission = full.Read.IsAdmitted ? null : full.Read.Admission.Diagnostic;\n'
     '                    hint = admission == null ? null : full.ClassicEditorHint;\n', 1),
    ('check-base', '                var check = draft.CheckNow(pilot.StaleBase);\n', '                var check = draft.CheckNow(full.StaleBase);\n', 1),
    # Reload
    ('reload',
     '                var pilot = read.PilotRead;\n'
     '                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");\n'
     '                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);\n'
     '\n'
     '                // 8: the read only goes to DiscardAndReload.\n'
     '                var reloaded = draft.DiscardAndReload(pilot.Read);\n',
     '                var pilot = read.PilotRead;\n'
     '                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");\n'
     '                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);\n'
     '                var full = read.FullRead;\n'
     '                if (full == null) return EditIntentOutcome.Failed(FullReadMissingReason);\n'
     '\n'
     '                // 8: the read only goes to DiscardAndReload.\n'
     '                var reloaded = draft.DiscardAndReload(full.Read);\n', 1),
    ('reload-detalle',
     '                            reloaded.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,\n'
     '                            pilot.Detail,\n'
     '                            pilot.ClassicEditorHint,\n',
     '                            reloaded.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,\n'
     '                            full.Detail,\n'
     '                            full.ClassicEditorHint,\n', 2),
    # Confirm
    ('confirm',
     '                var pilot = read.PilotRead;\n'
     '                if (pilot == null) return EditConfirmOutcome.Failed("La relectura no trajo resultado.");\n'
     '\n'
     '                // 7: the base of the reread only goes to ConfirmBase.\n'
     '                var confirm = draft.ConfirmBase(pilot.StaleBase);\n'
     '                var admission = pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic;\n'
     '                var hint = admission == null ? null : pilot.ClassicEditorHint;\n',
     '                var full = read.FullRead;\n'
     '                if (full == null) return EditConfirmOutcome.Failed(FullReadMissingReason);\n'
     '\n'
     '                // 7: the base of the reread only goes to ConfirmBase.\n'
     '                var confirm = draft.ConfirmBase(full.StaleBase);\n'
     '                var admission = full.Read.IsAdmitted ? null : full.Read.Admission.Diagnostic;\n'
     '                var hint = admission == null ? null : full.ClassicEditorHint;\n', 1),
    # Keep (D-PBT4-08 con D-S2)
    ('keep',
     '                var pilot = read.PilotRead;\n'
     '                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");\n'
     '                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);\n'
     '\n'
     '                // 7: the read only goes to the atomic form of KeepMineOnCurrentVersion, which has been confirmed by the gesture itself.\n'
     '                var kept = draft.KeepMineOnCurrentVersion(pilot.Read, PushBackPilotMaterializer.KeepPilotEditsOn, confirmed: true);\n',
     '                var pilot = read.PilotRead;\n'
     '                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");\n'
     '                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);\n'
     '                var full = read.FullRead;\n'
     '                if (full == null) return EditIntentOutcome.Failed(FullReadMissingReason);\n'
     '\n'
     '                // 7 (I-64 F4B PB-T4b, D-PBT4-08): without a witness, or with one that no longer holds, the operation only presents what changed,\n'
     '                // by groups, with the witness of this read; the draft does not change. Only the witness that still holds reaches the keep.\n'
     '                if (request.Witness == null || witness != EditWitnessStatus.Unchanged)\n'
     '                {\n'
     '                    if (!draft.IsRegistered || (draft.Status != DraftStatus.Dirty && draft.Status != DraftStatus.Conflict))\n'
     '                        return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");\n'
     '\n'
     '                    // The classification and its admission are of the pure function of Draft/: the host names the read only in its consumers.\n'
     '                    var presentation = PushBackKeepMinePresentation.Read(draft, full, read.Catalog!);\n'
     '                    if (!presentation.IsAdmitted)\n'
     '                        return EditIntentOutcome.NotAdmitted(presentation.Diagnostic, full.Detail, full.ClassicEditorHint, witness);\n'
     '                    return EditIntentOutcome.Presented(presentation.Lines, new EditIntentWitness(full.OriginKey, presentation.OriginText), witness);\n'
     '                }\n'
     '\n'
     '                // 7, continued: the read only goes to the atomic form of KeepMineOnCurrentVersion, confirmed by the second "Si" with the witness.\n'
     '                var kept = draft.KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true);\n', 1),
    ('keep-detalle',
     '                            kept.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,\n'
     '                            pilot.Detail,\n'
     '                            pilot.ClassicEditorHint,\n',
     '                            kept.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,\n'
     '                            full.Detail,\n'
     '                            full.ClassicEditorHint,\n', 1),
    # Discard and reload
    ('discard',
     '                var pilot = read.PilotRead;\n'
     '                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");\n'
     '                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);\n'
     '\n'
     '                // 7: the read only goes to DiscardAndReload; the status is the one of the reload of the lazy kind.\n'
     '                var reloaded = draft.DiscardAndReload(pilot.Read);\n',
     '                var pilot = read.PilotRead;\n'
     '                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");\n'
     '                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);\n'
     '                var full = read.FullRead;\n'
     '                if (full == null) return EditIntentOutcome.Failed(FullReadMissingReason);\n'
     '\n'
     '                // 7: the read only goes to DiscardAndReload; the status is the one of the reload of the lazy kind.\n'
     '                var reloaded = draft.DiscardAndReload(full.Read);\n', 1),
    # Update (D-PBT4-07)
    ('update-draft', '            DraftSession<PushBackPilotDraftState> draft;\n', '            DraftSession<PushBackFullDraftState> draft;\n', 1),
    ('update-run',
     '                outcome = PushBackPilotUpdateRun.Execute(draft, new PushBackPilotUpdateRequest(documentId, request.RackId), port);\n',
     '                outcome = PushBackPilotUpdateRun.ExecuteFull(draft, new PushBackPilotUpdateRequest(documentId, request.RackId), port);\n', 1),
    ('update-fallo', 'DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure(', 'DraftApplyOutcome<PushBackFullDraftState>.NotAppliedFailure(', 2),
    ('update-cerrado', 'AnswerOfAClosedThatWasNotAccepted(DraftApplyOutcome<PushBackPilotDraftState> outcome,',
     'AnswerOfAClosedThatWasNotAccepted(DraftApplyOutcome<PushBackFullDraftState> outcome,', 1),
    # the nine Find<> and the field
    ('find', 'Find<PushBackPilotDraftState>(', 'Find<PushBackFullDraftState>(', 9),
    ('find-borrador', '        private DraftSession<PushBackPilotDraftState>? FindPilotDraft(',
     '        private DraftSession<PushBackFullDraftState>? FindPilotDraft(', 1),
    ('vista', '        private static PilotFieldView ViewOfPilotDraft(DraftSession<PushBackPilotDraftState> draft)\n',
     '        private static PilotFieldView ViewOfPilotDraft(DraftSession<PushBackFullDraftState> draft)\n', 1),
    ('vista-escala',
     '            string? committed = null;\n'
     '            string? reason;\n'
     '            var annotations = draft.CurrentDraftState.Inputs.Annotations;\n'
     '            if (annotations == null) reason = "El rack no tiene escala de anotación.";\n'
     '            else committed = WorkspacePilotValueText.Show(annotations.AnnotationScale, out reason);\n',
     '            var committed = WorkspacePilotValueText.Show(draft.CurrentDraftState.Editable.AnnotationScale, out var reason);\n', 1),
    ('clave', 'PushBackPilotFieldAdapter.AnnotationScaleKey', 'PushBackFullFieldAdapter.AnnotationScaleKey', 3),
]

# ---------------------------------------------------------------- FVM (D-S2): dos «Sí»; la línea de RG:132 no cambia

FVM_SUBS = [
    ('campos',
     '        private PilotUnknownOrigin _choiceOrigin;\n'
     '        private string _choiceText = string.Empty;\n',
     '        private PilotUnknownOrigin _choiceOrigin;\n'
     '        private string _choiceText = string.Empty;\n'
     '\n'
     '        // I-64 F4B PB-T4b (D-PBT4-08, D-PBT4-09; D-S2): what the first "Si" of "keep my draft" presented, by groups, and the witness of that\n'
     '        // read, which the second "Si" hands back. They belong to the choice and go away with it.\n'
     '        private EditIntentWitness _keepWitness;\n'
     '        private IReadOnlyList<string> _keepLines = Array.Empty<string>();\n', 1),
    ('propiedades',
     '        public string ChoiceText => HasChoice ? _choiceText : string.Empty;\n',
     '        public string ChoiceText => HasChoice ? _choiceText : string.Empty;\n'
     '\n'
     '        /// <summary>The lines of the presentation of "keep my draft", one per line, while its choice waits for the second Yes; empty otherwise.</summary>\n'
     '        public string KeepMinePresentationText => HasChoice && _choice == ChoiceKind.KeepMine ? string.Join("\\n", _keepLines) : string.Empty;\n'
     '\n'
     '        /// <summary>Whether the presentation of "keep my draft" is shown above Yes and No.</summary>\n'
     '        public bool ShowKeepMinePresentation => KeepMinePresentationText.Length > 0;\n', 1),
    ('confirmar-testigo',
     '            var origin = _choiceOrigin;\n'
     '            try\n'
     '            {\n'
     '                ClearNotices();\n',
     '            var origin = _choiceOrigin;\n'
     '            var keepWitness = _keepWitness;\n'
     '            try\n'
     '            {\n'
     '                ClearNotices();\n', 1),
    ('confirmar-peticion',
     '                var request = new EditIntentRequest(session, rack, null, null);\n'
     '                var outcome = kind == ChoiceKind.KeepMine ? _port.TryKeepMine(request) : _port.TryDiscardAndReload(request);\n',
     '                // The first "Si" of "keep my draft" carries no witness and only gets the presentation; the second carries the witness of that read.\n'
     '                var request = new EditIntentRequest(session, rack, null, kind == ChoiceKind.KeepMine ? keepWitness : null);\n'
     '                var outcome = kind == ChoiceKind.KeepMine ? _port.TryKeepMine(request) : _port.TryDiscardAndReload(request);\n', 1),
    ('quitar-eleccion',
     '            _choiceOrigin = PilotUnknownOrigin.None;\n'
     '            _choiceText = string.Empty;\n'
     '        }\n',
     '            _choiceOrigin = PilotUnknownOrigin.None;\n'
     '            _choiceText = string.Empty;\n'
     '            _keepWitness = null;\n'
     '            _keepLines = Array.Empty<string>();\n'
     '        }\n', 1),
    ('presentar',
     '                case EditIntentStatus.Kept when kind == ChoiceKind.KeepMine:\n',
     '                case EditIntentStatus.Presented when kind == ChoiceKind.KeepMine:\n'
     '                    // The first "Si", or a second one whose witness no longer held: the choice is asked again, with the lines and the witness of\n'
     '                    // the read that the host classified. Nothing was kept and the draft did not change.\n'
     '                    AskChoice(ChoiceKind.KeepMine);\n'
     '                    if (_choice != ChoiceKind.KeepMine)\n'
     '                    {\n'
     '                        if (_failure != null) return;\n'
     '                        _noticeText = SituationChangedText;\n'
     '                        RaiseAll();\n'
     '                        return;\n'
     '                    }\n'
     '\n'
     '                    _keepWitness = outcome.KeepWitness;\n'
     '                    _keepLines = outcome.PresentationLines;\n'
     '                    RaiseAll();\n'
     '                    return;\n'
     '                case EditIntentStatus.Kept when kind == ChoiceKind.KeepMine:\n', 1),
    ('avisar',
     '            OnPropertyChanged(nameof(ChoiceText));\n',
     '            OnPropertyChanged(nameof(ChoiceText));\n'
     '            OnPropertyChanged(nameof(KeepMinePresentationText));\n'
     '            OnPropertyChanged(nameof(ShowKeepMinePresentation));\n', 1),
]

PV_OLD = ('                                <TextBlock x:Name="BrowserFieldChoiceText" Text="{Binding Browser.Field.ChoiceText}" TextWrapping="Wrap"\n'
          '                                           Foreground="{DynamicResource ShellTextPrimaryBrush}" />\n')
PV_NEW = (PV_OLD +
          '                                <!-- I-64 F4B PB-T4b (D-PBT4-08, D-PBT4-09): what changed, by groups, above Si and No, after the first Si of "keep my draft". -->\n'
          '                                <TextBlock x:Name="BrowserFieldKeepMinePresentationText" Text="{Binding Browser.Field.KeepMinePresentationText}"\n'
          '                                           TextWrapping="Wrap" Margin="0,4,0,0" Foreground="{DynamicResource ShellTextPrimaryBrush}"\n'
          '                                           Visibility="{Binding Browser.Field.ShowKeepMinePresentation, Converter={StaticResource FlagToVisibility}}" />\n')


def green(repo, rev):
    base, _mode = t4a_base(repo, rev)
    g = {}

    whc = base[WHC_P]
    for what, old, new, times in WHC_SUBS:
        whc = _sub(whc, old, new, 'WHC ' + what, times)
    assert 'PushBackPilotDraftState' not in whc and 'PushBackPilotStateComparer' not in whc and 'KeepPilotEditsOn' not in whc
    assert 'PushBackPilotFieldAdapter' not in whc and 'PushBackPilotUpdateRun.Execute(' not in whc and '.Inputs' not in whc
    assert whc.count('var pilot = read.PilotRead;') == 4 and whc.count('var full = read.FullRead;') == 6
    g[WHC_P] = whc

    wei = base[WEI_P]
    wei = _sub(wei, WEI_KEPT, WEI_PRESENTED, 'WEI enum')
    wei = _sub(wei, WEI_CTOR_END, WEI_CTOR_END_NEW, 'WEI constructor')
    wei = _sub(wei, WEI_HINT_PROP, WEI_HINT_PROP_NEW, 'WEI propiedades')
    wei = _sub(wei, WEI_KEPT_FACTORY, WEI_PRESENTED_FACTORY, 'WEI Presented')
    g[WEI_P] = wei

    fvm = base[FVM_P]
    for what, old, new, times in FVM_SUBS:
        fvm = _sub(fvm, old, new, 'FVM ' + what, times)
    assert fvm.count('TryKeepMine') == 1 and fvm.count('_port.TryKeepMine(request)') == 1
    assert 'var outcome = kind == ChoiceKind.KeepMine ? _port.TryKeepMine(request) : _port.TryDiscardAndReload(request);' in fvm
    assert 'public void RequestKeepMine() => AskChoice(ChoiceKind.KeepMine);' in fvm
    g[FVM_P] = fvm

    g[PV_P] = _sub(base[PV_P], PV_OLD, PV_NEW, 'PV')

    nfv = base[NFV_P]
    nfv = _sub(nfv, NFV_OLD_HEAD, NFV_NEW_HEAD, 'NFV using')
    nfv = _sub(nfv, NFV_OLD_CONSTS, NFV_NEW_CONSTS, 'NFV constantes')
    i = nfv.index(NFV_BODY_START)
    assert nfv.count(NFV_BODY_START) == 1
    g[NFV_P] = nfv[:i] + NFV_NEW_BODY

    g[RULE_P] = RULE
    g[SCALE_P] = SCALE
    g[ADAPTER_P] = ADAPTER
    g[PRES_P] = PRES
    for p in NEW:
        assert p not in base
    return base, g


# ---------------------------------------------------------------- el esqueleto exacto del RED (D-PBT4-06, M-29): la API que compilan las CT, inerte

RULE_SKELETON = RULE[:RULE.index('        /// <summary>Checks <paramref name="text"/> against the rules of one field.</summary>')] + (
    '        /// <summary>Checks <paramref name="text"/> against the rules of one field. INERT in the RED of PB-T4b: the GREEN moves the rule here.</summary>\n'
    '        public static NumericTextCheck Check(\n'
    '            string text,\n'
    '            bool isOptional,\n'
    '            bool integerOnly,\n'
    '            double? minimum,\n'
    '            double? maximum,\n'
    '            bool minimumInclusive,\n'
    '            bool maximumInclusive) => new NumericTextCheck(NumericTextStatus.NotANumber, null, null);\n'
    '    }\n'
    '}\n')

SCALE_SKELETON = SCALE.replace(
    '        public static DraftFieldValidation Validate(string text)\n'
    '        {\n'
    '            var check = NumericTextRule.Check(text, false, false, Minimum, null, MinimumInclusive, true);\n'
    '            return check.IsValid ? DraftFieldValidation.Valid : DraftFieldValidation.Invalid(check.Message);\n'
    '        }\n',
    '        // INERT in the RED of PB-T4b.\n'
    '        public static DraftFieldValidation Validate(string text) => DraftFieldValidation.Invalid(string.Empty);\n')

ADAPTER_SKELETON = ADAPTER[:ADAPTER.index('        public DraftFieldValidation Validate(string fieldKey, string text)\n')] + (
    '        // INERT in the RED of PB-T4b.\n'
    '        public DraftFieldValidation Validate(string fieldKey, string text) => DraftFieldValidation.Invalid(UnknownFieldMessage);\n'
    '\n'
    '        public PushBackFullDraftState Apply(PushBackFullDraftState current, IReadOnlyDictionary<string, string> pendingText) => current;\n'
    '    }\n'
    '}\n')

PRES_SKELETON = PRES[:PRES.index('        /// <summary>The lines of a classification: each group once')] + (
    '        /// <summary>INERT in the RED of PB-T4b: no line.</summary>\n'
    '        public static PushBackKeepMinePresentation Of(PushBackConflictClassification classification) =>\n'
    '            new PushBackKeepMinePresentation(Array.Empty<string>(), null, null);\n'
    '\n'
    '        /// <summary>INERT in the RED of PB-T4b: no line.</summary>\n'
    '        public static PushBackKeepMinePresentation Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog) =>\n'
    '            new PushBackKeepMinePresentation(Array.Empty<string>(), null, null);\n'
    '    }\n'
    '}\n')

FVM_SKELETON_SUB = (
    '        public string ChoiceText => HasChoice ? _choiceText : string.Empty;\n',
    '        public string ChoiceText => HasChoice ? _choiceText : string.Empty;\n'
    '\n'
    '        /// <summary>The lines of the presentation of "keep my draft". INERT in the RED of PB-T4b.</summary>\n'
    '        public string KeepMinePresentationText => string.Empty;\n'
    '\n'
    '        /// <summary>Whether the presentation of "keep my draft" is shown above Yes and No. INERT in the RED of PB-T4b.</summary>\n'
    '        public bool ShowKeepMinePresentation => false;\n')


def skeleton(repo, rev):
    """El src/ del RED exacto de PB-T4b: WEI con su API (la del GREEN, que el anfitrión del RED no usa), FVM con sus dos propiedades inertes y
    los cuatro archivos nuevos inertes (puros, para CT-PBT4B-19). WHC, PV y NumericFieldValidation.cs no cambian (M-29). Devuelve (base, rojo)."""
    base, g = green(repo, rev)
    red = {WEI_P: g[WEI_P], FVM_P: _sub(base[FVM_P], *FVM_SKELETON_SUB, what='FVM esqueleto'), RULE_P: RULE_SKELETON, SCALE_P: SCALE_SKELETON,
           ADAPTER_P: ADAPTER_SKELETON, PRES_P: PRES_SKELETON}
    assert 'NumericTextRule.Check(' not in SCALE_SKELETON and 'PushBackAnnotationScaleRule' not in ADAPTER_SKELETON.split('public DraftFieldValidation Validate')[1]
    return base, red
