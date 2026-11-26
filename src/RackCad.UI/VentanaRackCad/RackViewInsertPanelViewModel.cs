using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>The help of each group of the panel, as the system of the editor words it (DG-07: every system has its own).</summary>
    public sealed record RackViewInsertGroupTexts(string Frontal, string Lateral, string Planta);

    /// <summary>
    /// What the editor tells the panel «Insertar vista…» each time it changes: the rack (its name, whether it is new, its system), whether
    /// the draft has changes, why the views cannot be inserted now (empty when they can; DG-05), the kinds of view the drawing already has
    /// (null = not known; DG-04) and the help of each group.
    /// </summary>
    public sealed record RackViewInsertPanelContext(
        string RackName,
        bool IsNewRack,
        string SystemName,
        bool HasChanges,
        string Reason,
        IReadOnlyList<string> ExistingKinds,
        RackViewInsertGroupTexts Texts);

    /// <summary>One check box of «Vistas de este rack»: the view by its short name in its group, and its place in the order when marked.</summary>
    public sealed class RackViewInsertOptionItem : ObservableObject
    {
        private readonly RackViewInsertPanelViewModel _owner;
        private string _label;
        private bool _isChecked;
        private int _position;

        internal RackViewInsertOptionItem(RackViewInsertPanelViewModel owner, RackViewAddress address)
        {
            _owner = owner;
            Address = address;
            Chip = RackViewInsertChoice.ChipOf(address);
        }

        public RackViewAddress Address { get; }

        /// <summary>The name inside its group: «Fondo 1», «Salida», «Poste 3», «Planta».</summary>
        public string Chip { get; }

        /// <summary>The whole name, unique in the panel («Lateral · poste 3»): the name of the box for UI Automation.</summary>
        public string Label { get => _label; private set => SetProperty(ref _label, value); }

        /// <summary>Marked: it is in «Orden de colocación». A click marks it at the end of the order, or takes it out.</summary>
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (value == _isChecked) return;
                _owner.Toggle(Address);
                // The owner refreshes every item; a toggle that did nothing puts the box back as it was.
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        /// <summary>The number of its place in the order («2»), shown on the box while it is marked.</summary>
        public string Number => _position > 0 ? _position.ToString(CultureInfo.InvariantCulture) : string.Empty;

        public bool HasNumber => _position > 0;

        /// <summary>«puesto 2 del orden» for UI Automation (its item status), empty when it is not marked.</summary>
        public string NumberText => _position > 0 ? "puesto " + Number + " del orden" : string.Empty;

        internal void Refresh(string label, bool isChecked, int position)
        {
            Label = label;
            SetProperty(ref _isChecked, isChecked, nameof(IsChecked));
            if (_position == position) return;
            _position = position;
            OnPropertyChanged(nameof(Number));
            OnPropertyChanged(nameof(HasNumber));
            OnPropertyChanged(nameof(NumberText));
        }
    }

    /// <summary>One group of «Vistas de este rack» (Frontal · Lateral · Planta): its title, its help and its views.</summary>
    public sealed class RackViewInsertGroupItem : ObservableObject
    {
        private string _title;
        private string _help;
        private bool _hasCuts;
        private bool _canMarkAll;
        private bool _canMarkNone;
        private string _markAllLabel = "Todos los postes";

        internal RackViewInsertGroupItem(RackViewInsertGroup group)
        {
            Group = group;
        }

        public RackViewInsertGroup Group { get; }

        public string Title { get => _title; private set => SetProperty(ref _title, value); }

        public string Help { get => _help; private set => SetProperty(ref _help, value); }

        public ObservableCollection<RackViewInsertOptionItem> Options { get; } = new ObservableCollection<RackViewInsertOptionItem>();

        /// <summary>The group has lateral cuts by post (or by station): «Todos los postes» and «Ninguno» are offered (dolor 10).</summary>
        public bool HasCuts { get => _hasCuts; private set => SetProperty(ref _hasCuts, value); }

        /// <summary>«Todos los postes», or «Todas las estaciones» for Cantilever.</summary>
        public string MarkAllLabel { get => _markAllLabel; private set => SetProperty(ref _markAllLabel, value); }

        /// <summary>Some cut is not marked yet.</summary>
        public bool CanMarkAll { get => _canMarkAll; private set => SetProperty(ref _canMarkAll, value); }

        /// <summary>Some cut is in the order.</summary>
        public bool CanMarkNone { get => _canMarkNone; private set => SetProperty(ref _canMarkNone, value); }

        // The names of «Todos los postes» and «Ninguno» for UI Automation, which say what they do.
        public string MarkAllName => "Marcar " + MarkAllLabel.ToLowerInvariant();

        public string MarkNoneName => "Quitar " + MarkAllLabel.ToLowerInvariant() + " del orden";

        internal void Refresh(string title, string help, bool hasCuts, string markAllLabel, bool canMarkAll, bool canMarkNone)
        {
            Title = title;
            Help = help;
            HasCuts = hasCuts;
            MarkAllLabel = markAllLabel;
            CanMarkAll = canMarkAll;
            CanMarkNone = canMarkNone;
            OnPropertyChanged(nameof(MarkAllName));
            OnPropertyChanged(nameof(MarkNoneName));
        }
    }

    /// <summary>One row of «Orden de colocación»: its number, the view, whether the draft lost it, and «Subir», «Bajar» and «Quitar».</summary>
    public sealed class RackViewInsertOrderItem : ObservableObject
    {
        private string _number;
        private string _label;
        private bool _isGone;
        private bool _canMoveUp;
        private bool _canMoveDown;

        internal RackViewInsertOrderItem(RackViewAddress address)
        {
            Address = address;
        }

        public RackViewAddress Address { get; }

        public string Number { get => _number; private set => SetProperty(ref _number, value); }

        public string Label { get => _label; private set => SetProperty(ref _label, value); }

        /// <summary>The draft can no longer draw it: it stays in the order, crossed out, and is not inserted (DG-08).</summary>
        public bool IsGone { get => _isGone; private set => SetProperty(ref _isGone, value); }

        public string GoneTag => "Ya no existe en el borrador: no se insertará";

        public bool CanMoveUp { get => _canMoveUp; private set => SetProperty(ref _canMoveUp, value); }

        public bool CanMoveDown { get => _canMoveDown; private set => SetProperty(ref _canMoveDown, value); }

        // The names for UI Automation, unique in the panel (every view is once in the order).
        public string MoveUpName => "Subir " + Label;

        public string MoveDownName => "Bajar " + Label;

        public string RemoveName => "Quitar " + Label + " del orden";

        /// <summary>The whole row for UI Automation: «2: Lateral · poste 3», and why it will not be inserted when the draft lost it.</summary>
        public string AccessibleText => Number + ": " + Label + (IsGone ? "; " + GoneTag : string.Empty);

        internal void Refresh(int index, int count, string label, bool isGone)
        {
            Number = (index + 1).ToString(CultureInfo.InvariantCulture);
            Label = label;
            IsGone = isGone;
            CanMoveUp = index > 0;
            CanMoveDown = index < count - 1;
            OnPropertyChanged(nameof(MoveUpName));
            OnPropertyChanged(nameof(MoveDownName));
            OnPropertyChanged(nameof(RemoveName));
            OnPropertyChanged(nameof(AccessibleText));
        }
    }

    /// <summary>
    /// The panel «Insertar vista…» of the RackCad window (I-64 A-14, line D, fourth batch, part «ins»; the prototype «dialogos», DG-01 to
    /// DG-08, accepted by the Owner on 2026-11-26): what replaces the classic dialog «Insertar varias vistas» inside the window. It is not
    /// modal: it lives in the page of the editor, under its head, and AutoCAD and the editor stay free while the views are chosen. It is the
    /// same for every system (DG-07): an editor gives it its views (<see cref="RackViewInsertOption"/>), its system (the exposure policy of
    /// the classic dialog) and a <see cref="RackViewInsertPanelContext"/>, and receives the views chosen, in their order, when «Insertar N
    /// vistas» is pressed. Push Back, Cantilever and Cabecera take it when their editors migrate; the Cama has none (one view only).
    /// <list type="bullet">
    /// <item>DG-02: the order is the order of marking, changed with «Subir» and «Bajar». A rack of the drawing opens with nothing marked; a new
    /// rack with the first view of each group (<see cref="RackViewInsertChoice.SeedFirstOfEachGroup"/>).</item>
    /// <item>DG-03: every view is a check box, so it is once in a pass; another copy is inserted again.</item>
    /// <item>DG-04: the kinds of view the drawing already has, as the list read them.</item>
    /// <item>DG-05: the panel opens also when the views cannot be inserted now, and says why next to «Insertar N vistas».</item>
    /// <item>DG-06: no name foreseen; a new rack takes the automatic one of I-60 when it has none.</item>
    /// <item>DG-08: the panel follows the draft (<see cref="Follow"/>).</item>
    /// </list>
    /// The lists keep their items and are refreshed in place, so the box or the button with the focus survives a gesture.
    /// </summary>
    public sealed class RackViewInsertPanelViewModel : ObservableObject
    {
        private readonly RackViewInsertChoice _choice;
        private readonly Action<IReadOnlyList<RackViewAddress>> _insert;
        private readonly Action _close;
        private RackViewInsertPanelContext _context;
        private string _failureText = string.Empty;

        /// <param name="system">The system of the rack: its exposure policy filters the views, as the classic dialog does.</param>
        /// <param name="options">The views the draft can draw now.</param>
        /// <param name="context">The rack and the state of its editor.</param>
        /// <param name="insert">What «Insertar N vistas» does with the views chosen (the editor hands them to the window).</param>
        /// <param name="close">What «Cancelar» does (the editor closes the panel).</param>
        public RackViewInsertPanelViewModel(RackSystemKind system, IEnumerable<RackViewInsertOption> options, RackViewInsertPanelContext context,
            Action<IReadOnlyList<RackViewAddress>> insert, Action close)
        {
            _choice = new RackViewInsertChoice(system, options);
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _insert = insert;
            _close = close;
            if (context.IsNewRack) _choice.SeedFirstOfEachGroup();
            Refresh();
        }

        /// <summary>The choice behind the panel (the views offered and the order).</summary>
        public RackViewInsertChoice Choice => _choice;

        public RackViewInsertPanelContext Context => _context;

        // ------------------------------------------------------------------ the texts

        public string Title => _context.IsNewRack
            ? "Insertar el " + _context.SystemName + " nuevo en el dibujo"
            : "Insertar vistas de " + (string.IsNullOrWhiteSpace(_context.RackName) ? "este rack" : _context.RackName.Trim());

        public string Lead =>
            "Marca las vistas y ordénalas en «Orden de colocación»: AutoCAD pide el punto de cada una en ese orden. Mientras eliges, AutoCAD y el editor siguen libres.";

        public ObservableCollection<RackViewInsertGroupItem> Groups { get; } = new ObservableCollection<RackViewInsertGroupItem>();

        public ObservableCollection<RackViewInsertOrderItem> Order { get; } = new ObservableCollection<RackViewInsertOrderItem>();

        public bool IsOrderEmpty => _choice.IsEmpty;

        public string EmptyText => "Todavía no hay vistas: márcalas en «Vistas de este rack». Se añaden aquí en el orden en que las marques.";

        public bool HasGone => _choice.HasGone;

        public string GoneText =>
            "Lo tachado ya no existe en el borrador (cambiaron los fondos o los frentes): no se insertará. Quítalo, o devuelve el borrador a como estaba.";

        /// <summary>DG-04: the kinds of view the drawing already has (a rack of the drawing only).</summary>
        public string ExistingText
        {
            get
            {
                if (_context.IsNewRack) return string.Empty;
                const string again = "una vista que ya existe se coloca otra vez, enlazada al mismo rack.";
                var kinds = (_context.ExistingKinds ?? Array.Empty<string>()).Where(k => !string.IsNullOrWhiteSpace(k)).ToList();
                if (kinds.Count == 0) return "La lista del dibujo no dice qué vistas tiene ya este rack: " + again;
                return "En el dibujo ya hay: " + JoinY(kinds) + " (según la última lectura de la lista). La ventana todavía no sabe cuál es cada una "
                       + "(de qué fondo o de qué poste): " + again;
            }
        }

        public bool HasExistingText => !string.IsNullOrEmpty(ExistingText);

        public int InsertCount => _choice.InsertCount;

        /// <summary>«Insertar 2 vistas», «Insertar con 3 vistas» for a new rack, or «Elige al menos una vista».</summary>
        public string InsertLabel
        {
            get
            {
                var n = _choice.InsertCount;
                if (n == 0) return "Elige al menos una vista";
                return (_context.IsNewRack ? "Insertar con " : "Insertar ") + Plural(n, "vista", "vistas");
            }
        }

        /// <summary>DG-05: why the views cannot be inserted now, next to the button; empty when they can.</summary>
        public string Reason => _context.Reason ?? string.Empty;

        public bool HasReason => !string.IsNullOrEmpty(Reason);

        public bool CanInsert => _choice.InsertCount > 0 && !HasReason;

        /// <summary>What happens when AutoCAD writes (decision 10 of Dinámico: the draft first, in the views that exist).</summary>
        public string Note => _context.IsNewRack
            ? "Rack nuevo: se inserta con una identidad nueva y, si no le pones nombre, toma el automático (I-60)."
            : _context.HasChanges
                ? "Hay cambios: AutoCAD escribe antes el borrador en las vistas que ya existen y luego coloca las nuevas, enlazadas al rack."
                : "Sin cambios: AutoCAD redibuja tal cual las vistas que ya existen y luego coloca las nuevas, enlazadas al rack.";

        public string CancelNote => _context.IsNewRack
            ? "Si cancelas la colocación en AutoCAD, el rack nuevo no se inserta y sigue aquí."
            : "Si cancelas la colocación en AutoCAD, las vistas que ya existían quedan escritas y no se coloca ninguna nueva.";

        // ------------------------------------------------------------------ the gestures

        public bool Toggle(RackViewAddress address) => Changed(_choice.Toggle(address));

        public bool MoveUp(int index) => Changed(_choice.MoveUp(index));

        public bool MoveDown(int index) => Changed(_choice.MoveDown(index));

        public bool Remove(int index) => Changed(_choice.Remove(index));

        /// <summary>The row of the order of that view.</summary>
        public int IndexOf(RackViewAddress address) => _choice.PositionOf(address) - 1;

        /// <summary>«Todos los postes».</summary>
        public bool MarkAllCuts() => Changed(_choice.MarkAllCuts());

        /// <summary>«Ninguno».</summary>
        public bool UnmarkAllCuts() => Changed(_choice.UnmarkAllCuts());

        /// <summary>«Insertar N vistas»: the views the draft can draw, in their order, go to the editor; nothing when it cannot insert now.</summary>
        public bool Insert()
        {
            if (!CanInsert || _insert == null) return false;
            _insert(_choice.ToInsert);
            return true;
        }

        /// <summary>«Cancelar»: the panel closes and nothing is inserted.</summary>
        public void Close() => _close?.Invoke();

        /// <summary>A failure of a gesture of the panel, contained by its view and said in the panel.</summary>
        public string FailureText => _failureText;

        public void SayFailure(string text)
        {
            _failureText = text ?? string.Empty;
            OnPropertyChanged(nameof(FailureText));
        }

        // ------------------------------------------------------------------ the editor

        /// <summary>The draft changed (DG-08): the views it can draw now, and the state of its editor.</summary>
        public void Follow(IEnumerable<RackViewInsertOption> options, RackViewInsertPanelContext context)
        {
            _choice.Follow(options);
            if (context != null) _context = context;
            Refresh();
        }

        /// <summary>Only the state of the editor changed (it can insert now or not, it has changes).</summary>
        public void Update(RackViewInsertPanelContext context)
        {
            if (context == null || Equals(context, _context)) return;
            _context = context;
            RaiseTexts();
            RefreshGroups();
        }

        // ------------------------------------------------------------------ internals

        private bool Changed(bool changed)
        {
            Refresh();
            return changed;
        }

        private void Refresh()
        {
            RefreshGroups();
            RefreshOrder();
            RaiseTexts();
        }

        private void RefreshGroups()
        {
            var groups = _choice.Groups;
            Sync(Groups, groups, g => g.Group, g => new RackViewInsertGroupItem(g));
            foreach (var group in Groups)
            {
                var options = _choice.OptionsOf(group.Group);
                Sync(group.Options, options.Select(o => o.Address).ToList(), o => o.Address, a => new RackViewInsertOptionItem(this, a));
                foreach (var item in group.Options)
                {
                    var option = options.First(o => o.Address.Equals(item.Address));
                    item.Refresh(option.Label, _choice.IsMarked(item.Address), _choice.PositionOf(item.Address));
                }

                var cuts = options.Where(o => RackViewInsertChoice.IsIndexedCut(o.Address)).ToList();
                var stations = cuts.Count > 0 && cuts.All(o => o.Address.Variant.Kind == RackViewVariantKind.Station);
                group.Refresh(TitleOf(group.Group, cuts.Count > 0, stations), HelpOf(group.Group), cuts.Count > 0,
                    stations ? "Todas las estaciones" : "Todos los postes", cuts.Any(o => !_choice.IsMarked(o.Address)), _choice.AnyCutMarked);
            }
        }

        private void RefreshOrder()
        {
            var order = _choice.Order;
            Sync(Order, order.Select(e => e.Address).ToList(), o => o.Address, a => new RackViewInsertOrderItem(a));
            for (var i = 0; i < Order.Count; i++)
                Order[i].Refresh(i, Order.Count, order[i].Label, order[i].IsGone);
        }

        private void RaiseTexts()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(IsOrderEmpty));
            OnPropertyChanged(nameof(HasGone));
            OnPropertyChanged(nameof(ExistingText));
            OnPropertyChanged(nameof(HasExistingText));
            OnPropertyChanged(nameof(InsertCount));
            OnPropertyChanged(nameof(InsertLabel));
            OnPropertyChanged(nameof(Reason));
            OnPropertyChanged(nameof(HasReason));
            OnPropertyChanged(nameof(CanInsert));
            OnPropertyChanged(nameof(Note));
            OnPropertyChanged(nameof(CancelNote));
        }

        private static string TitleOf(RackViewInsertGroup group, bool hasCuts, bool stations)
        {
            switch (group)
            {
                case RackViewInsertGroup.Frontal: return "Frontal";
                case RackViewInsertGroup.Lateral: return hasCuts ? (stations ? "Lateral (corte por estación)" : "Lateral (corte por poste)") : "Lateral";
                default: return "Planta";
            }
        }

        private string HelpOf(RackViewInsertGroup group)
        {
            var texts = _context.Texts;
            if (texts == null) return string.Empty;
            switch (group)
            {
                case RackViewInsertGroup.Frontal: return texts.Frontal ?? string.Empty;
                case RackViewInsertGroup.Lateral: return texts.Lateral ?? string.Empty;
                default: return texts.Planta ?? string.Empty;
            }
        }

        // The items of a list follow the keys in their order: an item that stays is moved, not replaced, so its container (and the focus in
        // it) survives; a new key gets a new item; an item whose key went away is removed.
        private static void Sync<TItem, TKey>(ObservableCollection<TItem> items, IReadOnlyList<TKey> keys, Func<TItem, TKey> keyOf, Func<TKey, TItem> create)
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                if (!keys.Contains(keyOf(items[i]))) items.RemoveAt(i);
            }

            for (var i = 0; i < keys.Count; i++)
            {
                var at = -1;
                for (var j = i; j < items.Count; j++)
                {
                    if (!EqualityComparer<TKey>.Default.Equals(keyOf(items[j]), keys[i])) continue;
                    at = j;
                    break;
                }

                if (at < 0) items.Insert(i, create(keys[i]));
                else if (at != i) items.Move(at, i);
            }
        }

        internal static string JoinY(IReadOnlyList<string> parts)
        {
            if (parts.Count == 0) return string.Empty;
            if (parts.Count == 1) return parts[0];
            return string.Join(", ", parts.Take(parts.Count - 1)) + " y " + parts[parts.Count - 1];
        }

        private static string Plural(int n, string one, string many) => n.ToString(CultureInfo.InvariantCulture) + " " + (n == 1 ? one : many);
    }
}
