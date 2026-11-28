using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// One level button of the desviador of a post in the editable «Seguridad por poste» (T4 seg, SG-05): «N1»…, on or off, with a name for
    /// UI Automation that says the post, the level and what a click does. Refreshed in place, so the button under the mouse survives a write.
    /// </summary>
    public sealed class RackCadSafetyLevelItem : ObservableObject
    {
        private bool _isOn;
        private bool _isEnabled;
        private string _name = string.Empty;
        private Action _toggle;

        internal RackCadSafetyLevelItem(int level) => Level = level;

        public int Level { get; }

        public string Text => "N" + (Level + 1);

        public bool IsOn { get => _isOn; private set => SetProperty(ref _isOn, value); }

        public bool IsEnabled { get => _isEnabled; private set => SetProperty(ref _isEnabled, value); }

        public string Name { get => _name; private set => SetProperty(ref _name, value); }

        /// <summary>The click: the level on or off in the draft.</summary>
        public void Toggle()
        {
            if (_isEnabled) _toggle?.Invoke();
        }

        internal void Set(bool on, bool enabled, string name, Action toggle)
        {
            IsOn = on;
            IsEnabled = enabled;
            Name = name ?? string.Empty;
            _toggle = toggle;
        }
    }

    /// <summary>A cell of the table with a list (the bota or the protector lateral of a post), or the text that says why it has none.</summary>
    public sealed class RackCadSafetyChoiceCell : ObservableObject
    {
        private SelectiveChoiceItem _choice;
        private string _text = string.Empty;
        private string _sub = string.Empty;
        private bool _isOwn;

        /// <summary>The list of the cell; null when the cell shows a text instead («—», «La reemplaza el protector lateral»).</summary>
        public SelectiveChoiceItem Choice { get => _choice; private set { if (SetProperty(ref _choice, value)) OnPropertyChanged(nameof(HasChoice)); } }

        public bool HasChoice => _choice != null;

        public string Text { get => _text; private set => SetProperty(ref _text, value ?? string.Empty); }

        /// <summary>Under the list: «Propia · 2 piezas», «1 pieza», «Regla de los extremos».</summary>
        public string Sub { get => _sub; private set => SetProperty(ref _sub, value ?? string.Empty); }

        /// <summary>The post has a value of its own (the mark «Propia» of the approved prototype).</summary>
        public bool IsOwn { get => _isOwn; private set => SetProperty(ref _isOwn, value); }

        internal void Set(SelectiveChoiceItem choice, string text, string sub, bool isOwn)
        {
            if (choice != null && _choice != null && _choice.Key == choice.Key) _choice.Refresh(choice);
            else Choice = choice;
            Text = choice == null ? text : string.Empty;
            Sub = sub;
            IsOwn = isOwn;
        }
    }

    /// <summary>The desviador of a post: its level buttons, or «—» when the post has none (no variant, or no load level there).</summary>
    public sealed class RackCadSafetyLevelsCell : ObservableObject
    {
        private IReadOnlyList<RackCadSafetyLevelItem> _levels = Array.Empty<RackCadSafetyLevelItem>();
        private string _text = string.Empty;
        private string _sub = string.Empty;
        private string _name = string.Empty;

        public IReadOnlyList<RackCadSafetyLevelItem> Levels { get => _levels; private set { if (SetProperty(ref _levels, value)) OnPropertyChanged(nameof(HasLevels)); } }

        public bool HasLevels => _levels.Count > 0;

        public string Text { get => _text; private set => SetProperty(ref _text, value ?? string.Empty); }

        public string Sub { get => _sub; private set => SetProperty(ref _sub, value ?? string.Empty); }

        /// <summary>The name of the group of buttons for UI Automation: «Niveles con desviador en P2».</summary>
        public string Name { get => _name; private set => SetProperty(ref _name, value ?? string.Empty); }

        internal void Set(int count, Func<int, (bool On, bool Enabled, string Name, Action Toggle)> level, string text, string sub, string name)
        {
            if (_levels.Count != count) Levels = Enumerable.Range(0, count).Select(l => new RackCadSafetyLevelItem(l)).ToList();
            foreach (var item in _levels)
            {
                var state = level(item.Level);
                item.Set(state.On, state.Enabled, state.Name, state.Toggle);
            }

            Text = count == 0 ? text : string.Empty;
            Sub = sub;
            Name = name;
        }
    }

    /// <summary>
    /// One end of the defense of a post of a Dinámico (SG-03): the box (with a defense or not) and the length (empty = automatic). The
    /// length is a field of the draft as any other: it goes to the draft when it is left or with Enter, and a value refused stays in it.
    /// </summary>
    public sealed class RackCadSafetyDefenseCell : ObservableObject
    {
        private bool _hasDefense;
        private bool _isOn;
        private bool _isEnabled;
        private string _checkName = string.Empty;
        private DynamicFieldItem _length;
        private string _text = string.Empty;
        private string _sub = string.Empty;
        private bool _isOwn;
        private Action<bool> _write;
        private bool _refreshing;

        public bool HasDefense { get => _hasDefense; private set => SetProperty(ref _hasDefense, value); }

        /// <summary>The box: a click writes the end on (automatic) or off at once.</summary>
        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value) return;
                _isOn = value;
                OnPropertyChanged(nameof(IsOn));
                if (!_refreshing) _write?.Invoke(value);
            }
        }

        public bool IsEnabled { get => _isEnabled; private set => SetProperty(ref _isEnabled, value); }

        public string CheckName { get => _checkName; private set => SetProperty(ref _checkName, value ?? string.Empty); }

        public DynamicFieldItem Length { get => _length; private set => SetProperty(ref _length, value); }

        public string Text { get => _text; private set => SetProperty(ref _text, value ?? string.Empty); }

        /// <summary>«Automática · 12 in», «Propia», «Sin defensa».</summary>
        public string Sub { get => _sub; private set => SetProperty(ref _sub, value ?? string.Empty); }

        public bool IsOwn { get => _isOwn; private set => SetProperty(ref _isOwn, value); }

        internal void Set(bool hasDefense, bool on, bool enabled, string checkName, DynamicFieldItem length, string text, string sub, bool isOwn, Action<bool> write)
        {
            HasDefense = hasDefense;
            _refreshing = true;
            try
            {
                IsOn = on;
            }
            finally
            {
                _refreshing = false;
            }

            IsEnabled = enabled;
            CheckName = checkName;
            if (length != null && _length != null && _length.Key == length.Key) _length.Refresh(length);
            else Length = length;
            Text = hasDefense ? string.Empty : text;
            Sub = sub;
            IsOwn = isOwn;
            _write = write;
        }
    }

    /// <summary>One row of the editable «Seguridad por poste» (or its «Total»). Its cells are refreshed in place.</summary>
    public sealed class RackCadSafetyRowItem : ObservableObject
    {
        private string _kind = string.Empty;
        private string _separators = string.Empty;
        private string _guides = string.Empty;

        internal RackCadSafetyRowItem(string post, bool isTotal)
        {
            Post = post;
            IsTotal = isTotal;
        }

        public string Post { get; }

        public bool IsTotal { get; }

        public string Kind { get => _kind; internal set => SetProperty(ref _kind, value ?? string.Empty); }

        public RackCadSafetyChoiceCell Boots { get; } = new RackCadSafetyChoiceCell();

        /// <summary>T5 pb: the botas of side B of a Push Back of two sides (<see cref="Boots"/> is then side A).</summary>
        public RackCadSafetyChoiceCell BootsB { get; } = new RackCadSafetyChoiceCell();

        public RackCadSafetyChoiceCell SideGuard { get; } = new RackCadSafetyChoiceCell();

        public RackCadSafetyLevelsCell Deflectors { get; } = new RackCadSafetyLevelsCell();

        public string Separators { get => _separators; internal set => SetProperty(ref _separators, value ?? string.Empty); }

        public RackCadSafetyDefenseCell DefenseExit { get; } = new RackCadSafetyDefenseCell();

        public RackCadSafetyDefenseCell DefenseEntrance { get; } = new RackCadSafetyDefenseCell();

        public string Guides { get => _guides; internal set => SetProperty(ref _guides, value ?? string.Empty); }
    }

    /// <summary>A box of the inspector of a family («Dibujar también en la frontal»): a click writes at once.</summary>
    public sealed class RackCadSafetyCheckField : ObservableObject
    {
        private bool _isChecked;
        private bool _isEnabled;
        private string _hint;
        private Action<bool> _write;
        private bool _refreshing;

        internal RackCadSafetyCheckField(string key, string label, bool isChecked, bool isEnabled, string hint, Action<bool> write)
        {
            Key = key;
            Label = label;
            _isChecked = isChecked;
            _isEnabled = isEnabled;
            _hint = hint ?? string.Empty;
            _write = write;
        }

        public string Key { get; }

        public string Label { get; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
                if (!_refreshing && _isEnabled) _write?.Invoke(value);
            }
        }

        public bool IsEnabled { get => _isEnabled; private set => SetProperty(ref _isEnabled, value); }

        public string Hint { get => _hint; private set => SetProperty(ref _hint, value ?? string.Empty); }

        internal void Refresh(RackCadSafetyCheckField next)
        {
            _write = next._write;
            IsEnabled = next._isEnabled;
            Hint = next._hint;
            _refreshing = true;
            try
            {
                IsChecked = next._isChecked;
            }
            finally
            {
                _refreshing = false;
            }
        }
    }

    /// <summary>A line of the inspector that says something and edits nothing («Cómo se reparte» of the protector lateral of a Dinámico).</summary>
    public sealed class RackCadSafetyTextField : ObservableObject
    {
        private string _text;

        internal RackCadSafetyTextField(string key, string label, string text)
        {
            Key = key;
            Label = label;
            _text = text ?? string.Empty;
        }

        public string Key { get; }

        public string Label { get; }

        public string Text { get => _text; private set => SetProperty(ref _text, value ?? string.Empty); }

        internal void Refresh(RackCadSafetyTextField next) => Text = next._text;
    }

    /// <summary>A button of the inspector of a family («Todos los postes al general», «En todas las celdas»…), with a unique name.</summary>
    public sealed class RackCadSafetyAction : ObservableObject
    {
        private string _label;
        private bool _isEnabled;
        private string _name;
        private Action _run;

        internal RackCadSafetyAction(string key, string label, bool isEnabled, string name, Action run)
        {
            Key = key;
            _label = label;
            _isEnabled = isEnabled;
            _name = name;
            _run = run;
        }

        public string Key { get; }

        public string Label { get => _label; private set => SetProperty(ref _label, value); }

        public bool IsEnabled { get => _isEnabled; private set => SetProperty(ref _isEnabled, value); }

        public string Name { get => _name; private set => SetProperty(ref _name, value); }

        public void Run()
        {
            if (_isEnabled) _run?.Invoke();
        }

        internal void Refresh(RackCadSafetyAction next)
        {
            Label = next._label;
            IsEnabled = next._isEnabled;
            Name = next._name;
            _run = next._run;
        }
    }
}
