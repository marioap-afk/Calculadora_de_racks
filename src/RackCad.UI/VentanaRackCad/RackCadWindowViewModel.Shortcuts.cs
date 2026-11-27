using System;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 rl (answer of the Owner, 2026-11-27: «La ventana, ya»; VA-S2 completed): RACKVARIABLES and RVA open the page «Variables»; RACKPROPIEDADES
    /// and RPR open the tab «Propiedades» of the editor of the rack they designate, or «Propiedades del proyecto» with «Proyecto». The command
    /// the user typed is the gesture: it asks the window during the call, and the window shows the page with the gestures it already has
    /// (the first read of «Variables», «Editar aquí» of the rack). The read those gestures hand to the door runs inside the command, so no
    /// second command leaves the window; when it answered, the command says so (<see cref="IRackCadShortcutPort.ShortcutDone"/>) and the
    /// window shows the tab.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        /// <summary>The tab «Propiedades del proyecto» of the page «Variables».</summary>
        internal const int ProjectPropertiesTabIndex = 1;

        private IRackCadShortcutPort _shortcutDoor;

        // The rack whose tab «Propiedades» is shown when the read of its «Editar aquí», asked by RACKPROPIEDADES, answers.
        private (SessionId Session, string RackId)? _propertiesAsked;

        // Called once by the constructor.
        private void WireShortcuts()
        {
            _shortcutDoor = _door as IRackCadShortcutPort;
            if (_shortcutDoor == null) return;
            _shortcutDoor.ShortcutAsked += OnShortcutAsked;
            _shortcutDoor.ShortcutDone += OnShortcutDone;
        }

        // The command asks, inside itself: the window pulls the state of the host (the drawing where the command runs) and shows the page.
        private void OnShortcutAsked(RackCadShortcutAsk ask)
        {
            Contained(() =>
            {
                if (ask == null) return;
                Refresh();
                ask.Taken = true;
                _propertiesAsked = null;
                if (ask.Target == RackCadShortcutTarget.RackProperties) ShowRackPropertiesByShortcut(ask);
                else ShowVariablesByShortcut(ask);
            });
        }

        // The read the window asked answered (or there was none): the tab asked.
        private void OnShortcutDone(RackCadShortcutAsk ask)
        {
            Contained(() =>
            {
                if (ask == null) return;
                if (ask.Target == RackCadShortcutTarget.ProjectProperties)
                {
                    if (VariablesPage is VariablesPageViewModel page) page.SelectedTab = ProjectPropertiesTabIndex;
                }
                else if (ask.Target == RackCadShortcutTarget.RackProperties && _propertiesAsked is { } asked)
                {
                    _propertiesAsked = null;
                    var here = Panel.PresentedSession == asked.Session
                               && string.Equals(Panel.Browser.ActiveRackId, asked.RackId, StringComparison.OrdinalIgnoreCase);
                    if (!here || !ShowPropertiesTabOfActiveEditor())
                        ask.Said = "no se abrió el editor del rack. " + (string.IsNullOrWhiteSpace(ActionMessage) ? string.Empty : ActionMessage.Trim());
                }
            });
        }

        // «Variables» (and its tab «Propiedades del proyecto»): shown as by a click; the first time for the drawing it reads it (inside the
        // command). A page with a draft keeps it, as when it is shown by hand.
        private void ShowVariablesByShortcut(RackCadShortcutAsk ask)
        {
            ShowPage(RackCadWindowPage.Variables);
            if (ask.Target == RackCadShortcutTarget.ProjectProperties && VariablesPage is VariablesPageViewModel shown)
                shown.SelectedTab = ProjectPropertiesTabIndex;

            var page = VariablesPage;
            if (page != null && page.IsNotLoaded && !page.IsReading) ask.Said = RackCadShortcutRules.BusyText;
        }

        // The rack: its page, as a click in the list; its editor open shows the tab now; otherwise «Editar aquí» (its read runs inside the
        // command) and the tab when it answered.
        private void ShowRackPropertiesByShortcut(RackCadShortcutAsk ask)
        {
            var rack = ask.RackId;
            var session = Panel.PresentedSession;
            if (rack == null || !session.HasValue)
            {
                ask.Said = "no hay un rack ni un dibujo en la ventana.";
                return;
            }

            if (RackList.KindOf(rack) == null)
            {
                ask.Said = RackCadShortcutRules.NotInTheListText;
                return;
            }

            OpenRack(rack);
            if (!string.Equals(Panel.Browser.ActiveRackId, rack, StringComparison.OrdinalIgnoreCase))
            {
                ask.Said = "no se pudo ir al rack: el rack abierto tiene texto sin aplicar. Termínalo o descártalo en su página y vuelve a intentarlo.";
                return;
            }

            if (ShowPropertiesTabOfActiveEditor()) return;
            if (IsCabeceraRack)
            {
                ask.Said = RackCadShortcutRules.CabeceraHasNoPropertiesText;
                return;
            }

            if (_inFlight != null)
            {
                ask.Said = RackCadShortcutRules.BusyText;
                return;
            }

            _propertiesAsked = (session.Value, rack);
            if (ShowSelectiveOffer) EditSelectiveHere();
            else if (ShowDynamicOffer) EditDynamicHere();
            else if (ShowPushBackOffer) EditPushBackHere();
            else if (ShowCantileverOffer) EditCantileverHere();
            else if (ShowCamaOffer) EditCamaHere();
            else
            {
                _propertiesAsked = null;
                ask.Said = RackCadShortcutRules.NoEditorText;
            }
        }

        // The tab «Propiedades» of the editor of the active rack, when one is open.
        private bool ShowPropertiesTabOfActiveEditor()
        {
            if (ActiveSelectiveEditor is SelectiveEditorViewModel selective)
                selective.SelectedTab = SelectiveEditorViewModel.PropertiesTabIndex;
            else if (ActiveDynamicEditor is DynamicEditorViewModel dynamic)
                dynamic.SelectedTab = DynamicEditorViewModel.PropertiesTabIndex;
            else if (ActivePushBackEditor is PushBackEditorViewModel pushBack)
                pushBack.SelectedTab = PushBackEditorViewModel.PropertiesTabIndex;
            else if (ActiveCantileverEditor is CantileverEditorViewModel cantilever)
                cantilever.SelectedTab = CantileverEditorViewModel.PropertiesTabIndex;
            else if (ActiveCamaEditor is CamaEditorViewModel cama)
                cama.SelectedTab = CamaEditorViewModel.PropertiesTabIndex;
            else
                return false;
            return true;
        }
    }
}
