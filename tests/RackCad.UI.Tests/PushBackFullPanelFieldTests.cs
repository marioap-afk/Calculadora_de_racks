using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Formatting;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Workspace;
using RackCad.UI.Controls;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4b — the field of the panel on the full draft, from the UI (CT-PBT4B-04, 14 and 18). They run in the STA thread of the
    /// suite, on the collection of the discard prompt, with the invariant culture, the real window of Push Back, the real view-model of the
    /// field over the double of the edit port, and the real view of the panel. No window is shown, no key or focus is real, no configurator is
    /// created and nothing is written. Every step that throws becomes a line (the «EXCEPCION» rule), so a behavior that is missing fails by
    /// ASSERTION on the lines.
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackFullPanelFieldTests
    {
        private static readonly SessionId A = new SessionId(7);
        private const string Rack = "R-1";

        private static T Paso<T>(List<string> log, Func<T> step)
        {
            try
            {
                return step();
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
                return default(T);
            }
        }

        private static void Paso(List<string> log, Action step) => Paso<int>(log, () => { step(); return 0; });

        // ------------------------------------------------------------------------------------------------- CT-04

        // The ten texts of CT-PBT2B-10.
        private static readonly string[] Texts = { string.Empty, "   ", "abc", "0", "-1", "0,5", "0.5", "2.5", "1e3", "1.234,5" };

        [Fact]
        public void CT_PBT4B_04_TheRuleOfTheScaleOfTheApplicationAndTheScaleBoxOfARealWindowAcceptAndRejectTheSameTexts()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var window = Ventana();
                try
                {
                    var box = (NumericField)window.FindName("AnnotationScaleBox");
                    var same = box.Minimum == PushBackAnnotationScaleRule.Minimum
                        && box.MinimumInclusive == PushBackAnnotationScaleRule.MinimumInclusive
                        && !box.IsOptional && !box.IntegerOnly && box.Maximum == null && box.MaximumInclusive;
                    log.Add("parametros minimo=" + PushBackAnnotationScaleRule.Minimum.ToString("R", CultureInfo.InvariantCulture)
                        + " minimo-inclusivo=" + PushBackAnnotationScaleRule.MinimumInclusive + " iguales=" + same);

                    for (var index = 0; index < Texts.Length; index++)
                    {
                        var rule = PushBackAnnotationScaleRule.Validate(Texts[index]);
                        box.Text = Texts[index];
                        log.Add("texto-" + (index + 1).ToString(CultureInfo.InvariantCulture)
                            + " valido-igual=" + (rule.IsValid == !box.HasError)
                            + " mensaje-igual=" + string.Equals(rule.Message, box.ErrorMessage, StringComparison.Ordinal));
                    }
                }
                finally
                {
                    Cerrar(window);
                }
            });

            var expected = new List<string> { "parametros minimo=0 minimo-inclusivo=False iguales=True" };
            for (var n = 1; n <= 10; n++) expected.Add("texto-" + n.ToString(CultureInfo.InvariantCulture) + " valido-igual=True mensaje-igual=True");
            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-14

        private static PilotFieldView Conflict()
            => WorkspacePilotEditPortDouble.View(
                PilotFieldState.Conflict, "2.5", null, null, false, true, true, null, null, PilotUnknownOrigin.None, "A1:EnvelopeText", null);

        private sealed class Rig
        {
            public Rig()
            {
                Port = new WorkspacePilotEditPortDouble();
                Field = new WorkspacePilotFieldViewModel(Port, () => { });
                Port.Set(A, Rack, Conflict());
                Field.Show(A, Rack, false);
                Port.Calls.Clear();
                Port.KeepMineResponder = request =>
                {
                    Requests.Add(request);
                    return request.Witness == null
                        ? EditIntentOutcome.Presented(
                            new[] { "Cambiado fuera: el nombre.", "Cambiado por ti: las anotaciones." },
                            new EditIntentWitness("A1", "texto del sobre A1"),
                            EditWitnessStatus.NotSupplied)
                        : EditIntentOutcome.Kept(EditWitnessStatus.Unchanged);
                };
            }

            public WorkspacePilotEditPortDouble Port { get; }

            public WorkspacePilotFieldViewModel Field { get; }

            public List<EditIntentRequest> Requests { get; } = new List<EditIntentRequest>();
        }

        [Fact]
        public void CT_PBT4B_14_KeepMineAsksTwiceYesTheFirstPresentsTheChangesByGroupsAboveYesAndNoAndTheSecondKeeps()
        {
            var log = new List<string>();
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                Paso(log, () =>
                {
                    rig.Field.RequestKeepMine();
                    log.Add("pedir llamadas-al-puerto=" + string.Join(",", rig.Port.Operations().OrderBy(operation => operation, StringComparer.Ordinal)));
                    rig.Field.ConfirmChoice();
                    log.Add("primer-si TryKeepMine=" + rig.Port.Count(WorkspacePilotEditPortDouble.KeepMineOp)
                        + " testigo=" + (rig.Requests.Count > 0 && rig.Requests[0].Witness != null ? rig.Requests[0].Witness.Key : "null"));
                    log.Add("presentacion texto=[" + rig.Field.KeepMinePresentationText + "] visible=" + rig.Field.ShowKeepMinePresentation);
                });

                Paso(log, () =>
                {
                    var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One(Rack)) };
                    var view = new WorkspacePanelView(
                        new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble(), new WorkspaceBrowserSummaryPortDouble(), new WorkspacePilotEditPortDouble()));
                    var text = view.FindName("BrowserFieldKeepMinePresentationText") as TextBlock;
                    var panel = view.FindName("BrowserFieldChoicePanel") as Border;
                    var yes = view.FindName("BrowserFieldChoiceYesButton") as Button;
                    var inside = false;
                    var before = false;
                    if (text != null && panel != null && yes != null)
                    {
                        DependencyObject parent = text;
                        while (parent != null && !ReferenceEquals(parent, panel)) parent = LogicalTreeHelper.GetParent(parent);
                        inside = parent != null;
                        var container = text.Parent as Panel;
                        var buttons = yes.Parent as Panel;
                        before = container != null && buttons != null && ReferenceEquals(buttons.Parent, container)
                            && container.Children.IndexOf(text) < container.Children.IndexOf(buttons);
                    }

                    log.Add("xaml BrowserFieldKeepMinePresentationText dentro-del-panel-de-la-eleccion=" + inside + " antes-del-si=" + before);
                });

                Paso(log, () =>
                {
                    rig.Field.ConfirmChoice();
                    log.Add("segundo-si TryKeepMine=" + rig.Port.Count(WorkspacePilotEditPortDouble.KeepMineOp)
                        + " testigo=" + (rig.Requests.Count > 1 && rig.Requests[1].Witness != null ? rig.Requests[1].Witness.Key : "null"));
                    log.Add("conservado aviso=[" + rig.Field.NoticeText + "]");
                });

                // «No» after the presentation asks nothing of the port, not even its memory.
                Paso(log, () =>
                {
                    var other = new Rig();
                    other.Field.RequestKeepMine();
                    other.Field.ConfirmChoice();
                    other.Port.Calls.Clear();
                    other.Field.CancelChoice();
                    log.Add("no llamadas-al-puerto=" + other.Port.Calls.Count);
                });
            });

            Assert.Equal(
                string.Join(
                    "\n",
                    new[]
                    {
                        "pedir llamadas-al-puerto=GetField",
                        "primer-si TryKeepMine=1 testigo=null",
                        "presentacion texto=[Cambiado fuera: el nombre.\nCambiado por ti: las anotaciones.] visible=True",
                        "xaml BrowserFieldKeepMinePresentationText dentro-del-panel-de-la-eleccion=True antes-del-si=True",
                        "segundo-si TryKeepMine=2 testigo=A1",
                        "conservado aviso=[Borrador conservado sobre la versión actual del dibujo.]",
                        "no llamadas-al-puerto=0",
                    }),
                string.Join("\n", log));
        }

        // ------------------------------------------------------------------------------------------------- CT-18

        // The eleven cases: the rules of a numeric box, one by one.
        private static readonly (string Text, bool IsOptional, bool IntegerOnly, double? Minimum, double? Maximum, bool MinimumInclusive, bool MaximumInclusive)[] Cases =
        {
            (string.Empty, false, false, null, null, true, true),
            (string.Empty, true, false, null, null, true, true),
            ("abc", false, false, null, null, true, true),
            ("1.5", false, true, null, null, true, true),
            ("3", false, true, null, null, true, true),
            ("0", false, false, 0.0, null, false, true),
            ("0", false, false, 0.0, null, true, true),
            ("10", false, false, null, 10.0, true, false),
            ("10", false, false, null, 10.0, true, true),
            ("2,5", false, false, 0.0, null, true, true),
            ("-1", false, false, 0.0, null, true, true),
        };

        [Fact]
        public void CT_PBT4B_18_TheRuleOfTheApplicationAndTheValidationOfTheNumericBoxGiveTheSameResult()
        {
            var log = new List<string>();
            Sta(() =>
            {
                log.Add("casos=" + Cases.Length);
                for (var index = 0; index < Cases.Length; index++)
                {
                    var c = Cases[index];
                    var ui = NumericFieldValidation.Validate(c.Text, c.IsOptional, c.IntegerOnly, c.Minimum, c.Maximum, c.MinimumInclusive, c.MaximumInclusive);
                    var rule = NumericTextRule.Check(c.Text, c.IsOptional, c.IntegerOnly, c.Minimum, c.Maximum, c.MinimumInclusive, c.MaximumInclusive);
                    log.Add("caso-" + (index + 1).ToString(CultureInfo.InvariantCulture)
                        + " estado-igual=" + ((int)ui.Status == (int)rule.Status)
                        + " valor-igual=" + (ui.Value == rule.Value)
                        + " mensaje-igual=" + string.Equals(ui.Message, rule.Message, StringComparison.Ordinal));
                }
            });

            var expected = new List<string> { "casos=11" };
            for (var n = 1; n <= 11; n++) expected.Add("caso-" + n.ToString(CultureInfo.InvariantCulture) + " estado-igual=True valor-igual=True mensaje-igual=True");
            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }
    }
}
