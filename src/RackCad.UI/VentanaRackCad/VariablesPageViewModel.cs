using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Units;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>What the draft does with a variable, as the page shows it.</summary>
    public sealed class VariableRowItem : ObservableObject
    {
        private string _name;
        private string _keyForm;
        private ProjectVariableDraftRemoval _removal;
        private string _definitionShown = string.Empty;
        private string _valueText = string.Empty;
        private string _racksText = string.Empty;
        private string _tag = string.Empty;
        private bool _isSelected;
        private bool _hasProblem;
        private int _position;

        internal VariableRowItem(string key, bool isNew, string baseName, string baseKeyForm)
        {
            Key = key;
            IsNew = isNew;
            BaseName = baseName ?? string.Empty;
            BaseKeyForm = baseKeyForm ?? string.Empty;
            _name = BaseName;
            _keyForm = BaseKeyForm;
        }

        /// <summary>The <see cref="VariableId"/> of a variable of the drawing, or a key of the draft for a new one. Never shown (VA-09).</summary>
        public string Key { get; }

        public bool IsNew { get; }

        internal string BaseName { get; }

        internal string BaseKeyForm { get; }

        public string Name
        {
            get => _name;
            internal set
            {
                if (SetProperty(ref _name, value ?? string.Empty)) OnPropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>The definition in key form (<see cref="ProjectVariableFormulaText"/>).</summary>
        internal string KeyForm
        {
            get => _keyForm;
            set => _keyForm = value ?? string.Empty;
        }

        public ProjectVariableDraftRemoval Removal
        {
            get => _removal;
            internal set
            {
                if (SetProperty(ref _removal, value)) OnPropertyChanged(nameof(IsRemoved));
            }
        }

        public bool IsRemoved => _removal != ProjectVariableDraftRemoval.None;

        public string DisplayName => string.IsNullOrWhiteSpace(_name) ? "(sin nombre)" : _name.Trim();

        public string DefinitionShown { get => _definitionShown; internal set => SetProperty(ref _definitionShown, value ?? string.Empty); }

        public string ValueText { get => _valueText; internal set => SetProperty(ref _valueText, value ?? string.Empty); }

        public string RacksText { get => _racksText; internal set => SetProperty(ref _racksText, value ?? string.Empty); }

        /// <summary>«Nueva», «Modificada», «Se eliminará», «No válida», or empty.</summary>
        public string Tag { get => _tag; internal set => SetProperty(ref _tag, value ?? string.Empty); }

        public bool IsSelected { get => _isSelected; internal set => SetProperty(ref _isSelected, value); }

        public bool HasProblem { get => _hasProblem; internal set => SetProperty(ref _hasProblem, value); }

        /// <summary>The position in the list, from 1: part of the automation name of its button.</summary>
        public int Position
        {
            get => _position;
            internal set
            {
                if (SetProperty(ref _position, value)) OnPropertyChanged(nameof(AutomationName));
            }
        }

        public string AutomationName => "Variable " + _position.ToString(CultureInfo.InvariantCulture) + ": " + DisplayName;

        internal bool NameChanged => !string.Equals((_name ?? string.Empty).Trim(), BaseName.Trim(), StringComparison.Ordinal);

        internal bool DefinitionChanged => !string.Equals(_keyForm.Trim(), BaseKeyForm.Trim(), StringComparison.Ordinal);
    }

    /// <summary>A rack that uses the variable of the inspector, by its name and the property, with «Abrir».</summary>
    public sealed record VariableConsumerItem(string RackId, string Text, string OpenAccessibleText);

    /// <summary>A rack with broken references (by its name), what it keeps and whether its repair is in the draft.</summary>
    public sealed class BrokenRackItem : ObservableObject
    {
        private bool _isQueued;

        internal BrokenRackItem(string rackId, string rackName, string propertiesText, string keepsText, bool canRepair, string blockingReason,
            IReadOnlyList<(string Label, double Literal)> bindings)
        {
            RackId = rackId;
            RackName = rackName;
            PropertiesText = propertiesText;
            KeepsText = keepsText;
            CanRepair = canRepair;
            BlockingReason = blockingReason ?? string.Empty;
            Bindings = bindings;
        }

        public string RackId { get; }

        public string RackName { get; }

        public string PropertiesText { get; }

        public string KeepsText { get; }

        public bool CanRepair { get; }

        public string BlockingReason { get; }

        internal IReadOnlyList<(string Label, double Literal)> Bindings { get; }

        public bool IsQueued
        {
            get => _isQueued;
            internal set
            {
                if (SetProperty(ref _isQueued, value)) OnPropertyChanged(nameof(IsNotQueued));
            }
        }

        public bool IsNotQueued => !_isQueued;

        public string RepairAccessibleText => "Reparar " + RackName;

        public string UndoAccessibleText => "No reparar " + RackName;
    }

    /// <summary>One property of a draft of custom properties, as a row of the table of the page (or of the tab «Propiedades» of a rack).</summary>
    public sealed class CustomPropertyRowItem : ObservableObject
    {
        private string _name;
        private string _value;
        private string _tag = string.Empty;
        private string _nameError = string.Empty;
        private string _valueError = string.Empty;
        private int _position;

        internal CustomPropertyRowItem(string key, string id, string name, string value)
        {
            Key = key;
            Id = id;
            _name = name ?? string.Empty;
            _value = value ?? string.Empty;
        }

        /// <summary>The key of the row in the draft: the id of the property, or a key of the draft for a new one. Never shown.</summary>
        public string Key { get; }

        internal string Id { get; }

        public bool IsNew => string.IsNullOrWhiteSpace(Id);

        public string Name { get => _name; set => SetProperty(ref _name, value ?? string.Empty); }

        public string Value { get => _value; set => SetProperty(ref _value, value ?? string.Empty); }

        public string Tag { get => _tag; internal set => SetProperty(ref _tag, value ?? string.Empty); }

        public string NameError { get => _nameError; internal set => SetProperty(ref _nameError, value ?? string.Empty); }

        public string ValueError { get => _valueError; internal set => SetProperty(ref _valueError, value ?? string.Empty); }

        public int Position
        {
            get => _position;
            internal set
            {
                if (SetProperty(ref _position, value))
                {
                    OnPropertyChanged(nameof(NameAutomationName));
                    OnPropertyChanged(nameof(ValueAutomationName));
                    OnPropertyChanged(nameof(RemoveAutomationName));
                }
            }
        }

        private string Label => "la propiedad " + _position.ToString(CultureInfo.InvariantCulture);

        public string NameAutomationName => "Nombre de " + Label;

        public string ValueAutomationName => "Valor de " + Label;

        public string RemoveAutomationName => "Quitar " + Label;

        internal CustomPropertyDraftEntry ToDraft() => new CustomPropertyDraftEntry(Id, _name, _value);
    }

    /// <summary>A rack with an unwritten draft that uses a variable the page changes (VA-04).</summary>
    public sealed record VariablesConflictItem(string RackId, string Title, string Text, string GoLabel);

    /// <summary>
    /// The page «Variables» of the RackCad window (I-64 A-14, cuarta tanda, va; prototype «variables», VA-01 to VA-10), for ONE drawing.
    ///
    /// <para>
    /// It replaces the intermediate step that opened RACKVARIABLES. Everything goes to the draft of the page; the drawing changes only
    /// with ONE «Actualizar», all or nothing, in one order of the door and one «U» (VA-02): the register, its broken references (repair,
    /// asked inline: VA-03) and the custom properties of the project in the second tab (VA-06). Names never repeat (VA-05), and no internal
    /// identifier is shown (VA-09). «=Variable» does not come back to the editors (decision 12 of Selectivo): a rack is still linked in
    /// RACKEDITAR.
    /// </para>
    /// <para>
    /// Every rule is Application's: the batch (<see cref="ProjectVariablesBatch"/>) judges the draft as a whole and says what each row has
    /// wrong, what its values become and which racks are redrawn; the project draft is checked by <see cref="CustomPropertiesBatch"/>. This
    /// class only keeps the draft and says it. It never reads or writes the drawing: the window does, through its door.
    /// </para>
    /// </summary>
    public sealed class VariablesPageViewModel : ObservableObject, ICustomPropertiesTable
    {
        private const string PageNote =
            "Valores del dibujo. Un rack vinculado toma su valor de aquí. Lo que cambies va al borrador de esta página: el dibujo no cambia hasta «Actualizar».";

        private readonly Func<IReadOnlyList<string>, IReadOnlyList<(string RackId, string Name)>> _conflictsOf;
        private readonly Dictionary<string, string> _rackNames = new(StringComparer.OrdinalIgnoreCase);
        private VariablesDoorRead _read;
        private ProjectVariablesWorkspace _workspace;
        private ProjectVariablesBatchResult _analysis;
        private IReadOnlyList<CustomPropertyDraftProblem> _propertyProblems = Array.Empty<CustomPropertyDraftProblem>();
        private IReadOnlyList<CustomPropertyDraftEntry> _baseProperties = Array.Empty<CustomPropertyDraftEntry>();

        // The draft of the properties of the project that a write could not write while it wrote the variables (VA-S7): the next read
        // keeps it, over the collection it reads (correction 2 of the review).
        private IReadOnlyList<CustomPropertyDraftEntry> _keptProperties;
        private VariableRowItem _selected;
        private string _inspectorName = string.Empty;
        private string _inspectorDefinition = string.Empty;
        private string _askingUnlinkKey;
        private BrokenRackItem _askingRepair;
        private int _selectedTab;
        private int _nextKey;
        private bool _isWriting;
        private bool _writeTaken;
        private bool _isReading;
        private bool _otherRequestWaiting;
        private string _message = string.Empty;
        private bool _messageIsError;
        private IReadOnlyList<string> _changes = Array.Empty<string>();
        private IReadOnlyList<VariablesConflictItem> _conflicts = Array.Empty<VariablesConflictItem>();

        /// <param name="session">The drawing of the page.</param>
        /// <param name="conflictsOf">The racks, among the ones given, that have an unwritten draft in the window (VA-04); optional.</param>
        public VariablesPageViewModel(SessionId session, Func<IReadOnlyList<string>, IReadOnlyList<(string RackId, string Name)>> conflictsOf = null)
        {
            Session = session;
            _conflictsOf = conflictsOf;
        }

        public SessionId Session { get; }

        /// <summary>«Actualizar»: the window hands the request of the draft to its door.</summary>
        public event Action<VariablesPageViewModel> UpdateRequested;

        /// <summary>«Leer las variables del dibujo» (or the first time the page is shown): the window asks its door.</summary>
        public event Action<VariablesPageViewModel> ReadRequested;

        /// <summary>«Abrir» of a rack that uses a variable, or «Ir a…» of a conflict: the window opens its page.</summary>
        public event Action<string> OpenRackRequested;

        /// <summary>«Más» › «Abrir la ventana clásica (RACKVARIABLES)».</summary>
        public event Action ClassicRequested;

        public ObservableCollection<VariableRowItem> Rows { get; } = new();

        public ObservableCollection<VariableConsumerItem> Consumers { get; } = new();

        public ObservableCollection<BrokenRackItem> Broken { get; } = new();

        public ObservableCollection<CustomPropertyRowItem> Properties { get; } = new();

        // ------------------------------------------------------------------ the state of the page

        public bool IsLoaded => _read != null;

        public bool IsNotLoaded => _read == null;

        public bool IsReading => _isReading;

        public bool IsWriting => _isWriting;

        public bool IsDirty => IsLoaded && (Rows.Any(r => r.IsNew || r.IsRemoved || r.NameChanged || r.DefinitionChanged)
                                            || Broken.Any(b => b.IsQueued) || PropertiesChanged);

        public string StatusText => _isWriting ? "Escribiendo" : IsDirty ? "Modificado" : "Limpio";

        public string DiffText => _isWriting || !IsLoaded ? string.Empty
            : IsDirty ? Count(_changes.Count, "cambio sin escribir", "cambios sin escribir") : "Sin cambios que escribir.";

        public string Note => PageNote;

        public string NotLoadedText => _isReading
            ? "Leyendo las variables del dibujo en AutoCAD…"
            : "La página todavía no leyó las variables de este dibujo.";

        public bool IsBlocked => IsLoaded && _workspace != null && !_workspace.IsEditable;

        public bool IsNotBlocked => IsLoaded && !IsBlocked;

        public string BlockedText => !IsBlocked ? string.Empty
            : "El registro de variables de este dibujo no se puede administrar con esta versión de RackCad. Detalle: "
              + (string.IsNullOrWhiteSpace(_workspace.Error) ? "sin detalle." : _workspace.Error.Trim())
              + " No se ofrece crear ni editar variables, porque escribir encima borraría lo que guarda."
              + (Broken.Count > 0 ? " Reparar una referencia rota sigue disponible." : string.Empty);

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value)) CancelAsk();
            }
        }

        public bool CanEdit => IsLoaded && !_isWriting;

        public bool CanEditVariables => CanEdit && IsNotBlocked;

        public bool CanUpdate => CanEdit && IsDirty && UpdateBlockedText.Length == 0;

        public bool CanDiscard => CanEdit && IsDirty;

        public bool CanRead => !_isWriting && !_isReading && !IsDirty;

        /// <summary>Why «Actualizar» cannot write the draft now; empty when it can or there is nothing to write.</summary>
        public string UpdateBlockedText
        {
            get
            {
                if (!IsLoaded || !IsDirty || _isWriting) return string.Empty;
                if (_otherRequestWaiting) return "AutoCAD está escribiendo o leyendo otra petición.";
                var problems = ProblemCount;
                if (problems > 0) return problems == 1 ? "Hay 1 valor no válido." : "Hay " + problems + " valores no válidos.";
                if (_conflicts.Count > 0)
                    return "Antes, escribe o descarta el borrador de " + ProjectVariablesBatch.List(_conflicts.Select(c => NameOfRack(c.RackId))) + ".";
                return string.Empty;
            }
        }

        public string Message => _message;

        public bool MessageIsError => _messageIsError;

        public bool HasMessage => _message.Length > 0;

        public string WritingText => !_isWriting ? string.Empty
            : _writeTaken
                ? "La orden ya corre en AutoCAD: se termina o se cancela allí, y la respuesta siempre llega."
                : "Se pidió a AutoCAD que escriba " + Count(_changes.Count, "cambio", "cambios") + " en una sola orden"
                  + (RedrawNames.Count > 0 ? " y redibuje " + ProjectVariablesBatch.List(RedrawNames) : string.Empty)
                  + ". La página no se puede editar hasta que conteste.";

        public string VariablesTabHeader => "Variables de proyecto" + (VariableProblems > 0 ? " (con problemas)" : string.Empty);

        public string PropertiesTabHeader => "Propiedades del proyecto" + (_propertyProblems.Count > 0 ? " (con problemas)" : string.Empty);

        public string CountText => Count(Rows.Count(r => !r.IsRemoved), "variable", "variables");

        public bool HasNoVariables => IsNotBlocked && Rows.Count == 0;

        public bool HasBroken => IsLoaded && Broken.Count > 0;

        // ------------------------------------------------------------------ «Lo que escribirá Actualizar»

        public IReadOnlyList<string> Changes => _changes;

        public bool HasChanges => IsDirty && _changes.Count > 0;

        public string RedrawText => RedrawNames.Count == 0
            ? "No se redibuja ningún rack."
            : "Se redibujan todas las vistas de " + ProjectVariablesBatch.List(RedrawNames) + ", en la misma orden y con un solo «U».";

        public IReadOnlyList<VariablesConflictItem> Conflicts => _conflicts;

        private IReadOnlyList<string> RedrawNames => (_analysis?.RedrawnRackIds ?? Array.Empty<string>()).Select(NameOfRack).ToList();

        /// <summary>The racks «Actualizar» redraws, by id: the window forgets the clean editors of these after the write.</summary>
        public IReadOnlyList<string> RedrawnRackIds => _analysis?.RedrawnRackIds ?? Array.Empty<string>();

        // ------------------------------------------------------------------ the inspector

        public VariableRowItem Selected => _selected;

        public bool HasSelection => _selected != null && IsNotBlocked;

        public bool HasNoSelection => !HasSelection && IsNotBlocked;

        public string NoSelectionText => "Elige una variable de la lista, o crea una con «Nueva variable».";

        public string InspectorTitle => _selected?.DisplayName ?? string.Empty;

        public string InspectorTag => _selected?.Tag ?? string.Empty;

        public string InspectorName
        {
            get => _inspectorName;
            set => SetProperty(ref _inspectorName, value ?? string.Empty);
        }

        public string InspectorDefinition
        {
            get => _inspectorDefinition;
            set => SetProperty(ref _inspectorDefinition, value ?? string.Empty);
        }

        public bool InspectorReadOnly => !CanEditVariables || _selected == null || _selected.IsRemoved;

        public string InspectorNameError => ErrorOf(_selected, ProjectVariableDraftField.Name);

        public string InspectorDefinitionError => ErrorOf(_selected, ProjectVariableDraftField.Definition);

        public string InspectorUnit => _selected != null && _selected.KeyForm.TrimStart().StartsWith("=", StringComparison.Ordinal) ? string.Empty : "in";

        public string InspectorValueText => _selected == null ? string.Empty : _selected.IsRemoved ? "—" : ValueOf(FinalValue(_selected.Key));

        public string InspectorValueHint
        {
            get
            {
                if (_selected == null) return string.Empty;
                if (_selected.IsNew) return "Variable nueva: todavía no está en el dibujo.";
                var before = BaseValue(_selected.Key);
                var after = FinalValue(_selected.Key);
                return before.HasValue && after.HasValue && !Same(before, after)
                    ? "Antes " + Format(before.Value) + " in. Cambia al actualizar."
                    : "El de ahora en el dibujo.";
            }
        }

        public string RemovedText
        {
            get
            {
                if (_selected == null || !_selected.IsRemoved) return string.Empty;
                if (_selected.Removal != ProjectVariableDraftRemoval.UnlinkAllAndDelete) return "Se eliminará al actualizar.";
                var racks = ConsumerNames(_selected.Key);
                return "Se eliminará al actualizar. Antes, " + ProjectVariablesBatch.List(racks)
                       + (racks.Count == 1 ? " deja de seguirla y conserva " : " dejan de seguirla y conservan ") + KeptText(_selected.Key) + ".";
            }
        }

        public bool IsSelectedRemoved => _selected != null && _selected.IsRemoved;

        public bool HasConsumers => Consumers.Count > 0;

        public bool HasNoConsumers => _selected != null && Consumers.Count == 0;

        public bool HasDependents => DependentsOf(_selected?.Key).Count > 0;

        public string NoConsumersText => _selected == null ? string.Empty
            : _selected.IsNew
                ? "Una variable nueva no la usa ningún rack. Hoy un rack se vincula en el editor clásico de Selectivo (RACKEDITAR), con «=»."
                : "Ningún rack la usa todavía.";

        public string DependentsText
        {
            get
            {
                var users = DependentsOf(_selected?.Key);
                return users.Count == 0 ? string.Empty
                    : ProjectVariablesBatch.List(users.Select(k => "«" + RowOf(k)?.DisplayName + "»")) + ". Siguen a esta variable por su identidad.";
            }
        }

        public string DeleteLabel => _selected != null && _selected.IsNew ? "Quitar" : "Eliminar";

        public bool CanDelete => CanEditVariables && _selected != null && !_selected.IsRemoved && DeleteWhy.Length == 0;

        public bool ShowUnlink => _selected != null && !_selected.IsNew && Consumers.Count > 0;

        public bool CanUnlink => CanEditVariables && ShowUnlink && !_selected.IsRemoved && DependentsOf(_selected.Key).Count == 0;

        /// <summary>Why «Eliminar» (or «Desvincular todos y eliminar…») is not offered, said below the buttons (dolores 9 and 17).</summary>
        public string DeleteWhy
        {
            get
            {
                if (_selected == null || _selected.IsRemoved) return string.Empty;
                var users = DependentsOf(_selected.Key);
                var racks = _selected.IsNew ? 0 : ConsumerNames(_selected.Key).Count;
                if (users.Count > 0)
                    return "La usa la fórmula de " + ProjectVariablesBatch.List(users.Select(k => "«" + RowOf(k)?.DisplayName + "»"))
                           + ": cámbiala antes de eliminar esta variable."
                           + (racks == 0 ? string.Empty : racks == 1 ? " También la usa 1 rack." : " También la usan " + racks + " racks.");
                if (racks > 0)
                    return (racks == 1 ? "La usa 1 rack" : "La usan " + racks + " racks")
                           + ", así que «Eliminar» no se ofrece. «Desvincular todos y eliminar…» los desvincula conservando su valor.";
                return string.Empty;
            }
        }

        public bool IsAskingUnlink => _askingUnlinkKey != null && _selected != null && string.Equals(_askingUnlinkKey, _selected.Key, StringComparison.Ordinal);

        public string UnlinkTitle => _selected == null ? string.Empty : "¿Desvincular todos y eliminar «" + _selected.DisplayName + "»?";

        public string UnlinkText
        {
            get
            {
                if (_selected == null) return string.Empty;
                var row = _workspace?.Variables.FirstOrDefault(v => string.Equals(v.Id.Value, _selected.Key, StringComparison.OrdinalIgnoreCase));
                var consumers = row?.Consumers ?? Array.Empty<VariableConsumerSummary>();
                var names = ConsumerNames(_selected.Key);
                return (names.Count == 1 ? "El rack que la usa deja de seguirla y conserva su valor de ahora, " : "Los " + names.Count
                           + " racks que la usan dejan de seguirla y conservan su valor de ahora, ") + KeptText(_selected.Key) + ": "
                       + string.Join(", ", consumers.Select(c => RackName(c.RackId, c.RackName) + " (" + string.Join(", ", c.PropertyIds.Select(PropertyLabel))
                           .ToLower(CultureInfo.CurrentCulture) + ")"))
                       + ". Después se elimina la variable. Va al borrador: el dibujo no cambia hasta «Actualizar».";
            }
        }

        public string RenameNote => "Renombrar no redibuja ningún rack: los racks y las fórmulas siguen a la variable por su identidad, no por su nombre.";

        public string FormulaHelp =>
            "Cada variable es una longitud en pulgadas. Su valor es un número mayor que cero, o «=» y una fórmula con otras variables, + − * /, "
            + "paréntesis, MIN, MAX y ABS, y números con su unidad entre corchetes: 100" + ProjectVariableDiagnosticText.Unit(LengthUnit.Millimeter)
            + ", 2" + ProjectVariableDiagnosticText.Unit(LengthUnit.Foot) + ".";

        // ------------------------------------------------------------------ the inline repair

        public bool IsAskingRepair => _askingRepair != null;

        public string RepairTitle => _askingRepair == null ? string.Empty : "¿Reparar " + _askingRepair.RackName + "?";

        public string RepairYesLabel => _askingRepair == null ? string.Empty : "Reparar " + _askingRepair.RackName;

        public string RepairText => _askingRepair == null ? string.Empty
            : "Reparar actúa sobre todo el rack: se quitan todos sus vínculos rotos (" + _askingRepair.Bindings.Count + "), no solo una fila. "
              + string.Join("; ", _askingRepair.Bindings.Select(b => "Su " + b.Label.ToLower(CultureInfo.CurrentCulture)
                  + " deja de apuntar a la variable que falta y queda en " + Format(b.Literal) + " in, el valor que guardaba"))
              + ". No hay un valor de variable que conservar, así que la geometría puede cambiar. Va al borrador: el dibujo no cambia hasta «Actualizar».";

        // ------------------------------------------------------------------ the project properties (VA-06)

        public string PropertiesCount => Properties.Count + " de " + CustomPropertiesMutations.MaxEntriesOnCreate;

        public bool CanAddProperty => CanEdit && ProjectPropertiesWritable && Properties.Count < CustomPropertiesMutations.MaxEntriesOnCreate;

        public bool ProjectPropertiesWritable => _read?.ProjectProperties != null && _read.ProjectProperties.CanWrite;

        public string ProjectPropertiesNote => !IsLoaded ? string.Empty
            : ProjectPropertiesWritable
                ? "Nombre y valor de texto guardados en el dibujo. Cada propiedad conserva su identidad aunque cambie de nombre. Se escriben con el «Actualizar» de esta página."
                : "Las propiedades del proyecto no se pueden escribir: " + (_read?.ProjectProperties?.Error ?? "no se pudieron leer.");

        public bool HasNoProperties => IsLoaded && Properties.Count == 0;

        public string PropertiesTitle => "Propiedades del proyecto";

        public string PropertiesNote => ProjectPropertiesNote;

        public bool CanEditProperties => CanEdit && ProjectPropertiesWritable;

        public string PropertiesRemovedText
        {
            get
            {
                var removed = _baseProperties.Where(b => Properties.All(p => !string.Equals(p.Id, b.Id, StringComparison.OrdinalIgnoreCase)))
                    .Select(b => "«" + b.Name + "»").ToList();
                return removed.Count == 0 ? string.Empty : "Se quitarán al actualizar: " + ProjectVariablesBatch.List(removed) + ".";
            }
        }

        public string PropertiesHelp =>
            "Hasta 50 propiedades. El nombre es obligatorio, de hasta 80 caracteres y sin repetir otro. El valor puede quedar vacío, admite varias líneas y llega a 1000 caracteres.";

        private bool PropertiesChanged => IsLoaded && ProjectPropertiesWritable
                                           && !CustomPropertiesBatch.IsUnchanged(_read.ProjectProperties.Document, Properties.Select(p => p.ToDraft()).ToList());

        // ------------------------------------------------------------------ the read and the write (by the window)

        /// <summary>The window asked its door to read: the page says it waits.</summary>
        internal void BeginReading()
        {
            _isReading = true;
            RaiseAll();
        }

        internal void EndReading(string failure)
        {
            _isReading = false;
            if (failure != null) Say(failure, true);
            RaiseAll();
        }

        /// <summary>The answer of RACKVARIABLESLEER: the page shows the drawing, and the draft starts over (it never replaces one with changes).</summary>
        internal bool Load(VariablesDoorRead read, Func<string, string> listedName = null)
        {
            _isReading = false;
            if (read == null || !read.Read)
            {
                Say("No se pudieron leer las variables: " + (read?.Reason ?? "sin detalle."), true);
                RaiseAll();
                return false;
            }

            if (IsDirty || _isWriting)
            {
                Say("La página tiene un borrador con cambios: no se reemplazó con la lectura.", false);
                RaiseAll();
                return false;
            }

            _read = read;
            _workspace = read.Workspace();
            _rackNames.Clear();
            foreach (var entry in read.Entries ?? Array.Empty<ProjectVariableScanEntry>())
            {
                if (entry?.RackId == null || entry.Authored == null || _rackNames.ContainsKey(entry.RackId)) continue;
                var name = !string.IsNullOrWhiteSpace(entry.Authored.Name) ? entry.Authored.Name.Trim() : listedName?.Invoke(entry.RackId);
                if (!string.IsNullOrWhiteSpace(name)) _rackNames[entry.RackId] = name.Trim();
            }

            var keyed = _workspace.Variables.Select(v => (v.Id.Value, v.Name)).ToList();
            Rows.Clear();
            foreach (var variable in _workspace.Variables)
                Rows.Add(new VariableRowItem(variable.Id.Value, false, variable.Name, ProjectVariableFormulaText.FromDefinition(variable.Definition, keyed)));

            Broken.Clear();
            foreach (var group in _workspace.BrokenBindings.GroupBy(b => b.RackId, StringComparer.OrdinalIgnoreCase))
            {
                var first = group.First();
                var bindings = group.Select(b => (PropertyLabel(b.PropertyId), b.StoredLiteral)).ToList();
                Broken.Add(new BrokenRackItem(first.RackId, RackName(first.RackId, first.RackName),
                    string.Join(", ", bindings.Select(b => b.Item1)),
                    string.Join(", ", bindings.Select(b => Format(b.StoredLiteral) + " in (el valor que guardaba)")),
                    first.RackCanRepair, first.RackBlockingReason, bindings));
            }

            _baseProperties = read.ProjectProperties?.Document == null
                ? Array.Empty<CustomPropertyDraftEntry>()
                : CustomPropertiesBatch.DraftOf(read.ProjectProperties.Document);
            Properties.Clear();
            foreach (var entry in _keptProperties ?? _baseProperties)
                Properties.Add(new CustomPropertyRowItem(entry.Id ?? "nueva-" + (++_nextKey).ToString(CultureInfo.InvariantCulture), entry.Id, entry.Name, entry.Value));
            _keptProperties = null;

            _askingRepair = null;
            _askingUnlinkKey = null;
            Select(Rows.FirstOrDefault());
            Recompute();
            return true;
        }

        /// <summary>The request of «Actualizar», with what the page read: the batch of the whole draft and the project draft.</summary>
        internal VariablesDoorWrite BuildWrite(long requestId)
        {
            if (_read == null) return null;
            return new VariablesDoorWrite(requestId, Session,
                VariablesDoorRules.RegistryFingerprint(_read.Registry), BatchRequest(),
                VariablesDoorRules.PropertiesFingerprint(_read.ProjectProperties),
                PropertiesChanged ? Properties.Select(p => p.ToDraft()).ToList() : null);
        }

        internal void BeginWriting()
        {
            _isWriting = true;
            _writeTaken = false;
            CancelAsk();
            RaiseAll();
        }

        internal void WriteTaken()
        {
            _writeTaken = true;
            OnPropertyChanged(nameof(WritingText));
        }

        /// <summary>
        /// The answer of RACKVARIABLESAPLICAR. Written: the drawing is the authority, so the page forgets what it read and the window reads it
        /// again; not written: the draft stays, with the reason.
        /// </summary>
        internal void EndWriting(VariablesDoorWritten written)
        {
            _isWriting = false;
            _writeTaken = false;
            if (written != null && written.Written)
            {
                var names = (written.RedrawnRackIds ?? Array.Empty<string>()).Select(NameOfRack).ToList();
                var propertiesFailed = !string.IsNullOrWhiteSpace(written.PropertiesError);
                var count = _changes.Count - (propertiesFailed && PropertiesChanged ? 1 : 0);
                if (propertiesFailed && PropertiesChanged) _keptProperties = Properties.Select(p => p.ToDraft()).ToList();
                _read = null;
                _workspace = null;
                _analysis = null;
                Rows.Clear();
                Broken.Clear();
                Properties.Clear();
                _changes = Array.Empty<string>();
                _conflicts = Array.Empty<VariablesConflictItem>();
                Select(null);
                var said = "Variables escritas en el dibujo: " + Count(count, "cambio", "cambios")
                           + (names.Count == 0 ? "; no se redibujó ningún rack" : (names.Count == 1 ? "; se redibujó " : "; se redibujaron ") + ProjectVariablesBatch.List(names))
                           + ". Un «U» en AutoCAD lo deshace todo.";
                Say(propertiesFailed
                    ? said + " Pero las propiedades del proyecto no se escribieron: " + written.PropertiesError.Trim() + " Siguen en el borrador."
                    : said, propertiesFailed);
            }
            else
            {
                Say(written?.Message ?? "AutoCAD no escribió el borrador.", true);
            }

            RaiseAll();
        }

        /// <summary>The window stopped waiting for a write the command had not taken: the draft stays, not written.</summary>
        internal void StoppedWaiting()
        {
            _isWriting = false;
            _writeTaken = false;
            Say("Se dejó de esperar a AutoCAD: si la orden llega a correr, no escribirá nada. El borrador sigue sin escribir.", false);
            RaiseAll();
        }

        /// <summary>Whether another request of the window waits for AutoCAD (one at a time, for every page and editor).</summary>
        internal void SetOtherRequestWaiting(bool waiting)
        {
            if (_otherRequestWaiting == waiting) return;
            _otherRequestWaiting = waiting;
            RaiseState();
        }

        /// <summary>The window says something about the page (a refusal of the door, for instance).</summary>
        public void Say(string text, bool error)
        {
            _message = text ?? string.Empty;
            _messageIsError = error && _message.Length > 0;
            OnPropertyChanged(nameof(Message));
            OnPropertyChanged(nameof(MessageIsError));
            OnPropertyChanged(nameof(HasMessage));
        }

        /// <summary>The draft changed elsewhere (a rack draft of the window): the conflicts follow (VA-04).</summary>
        public void RefreshConflicts()
        {
            if (!IsLoaded) return;
            ComputeConflicts();
            RaiseState();
        }

        // ------------------------------------------------------------------ gestures

        public void RequestRead()
        {
            if (!CanRead) return;
            Say(string.Empty, false); // smoke T4 (F-2): a reason of an earlier refusal does not outlive a new read the user asked for
            ReadRequested?.Invoke(this);
        }

        public void RequestWrite()
        {
            if (!CanUpdate) return;
            UpdateRequested?.Invoke(this);
        }

        public void Discard()
        {
            if (!CanDiscard || _read == null) return;
            var read = _read;
            _read = null;
            Rows.Clear();
            Load(read);
            Say("Borrador descartado: se muestra lo último que se leyó del dibujo o se escribió en él.", false);
        }

        /// <summary>«Más» › «Comprobar»: what «Actualizar» would do, or why it would not.</summary>
        public void Check()
        {
            if (!IsLoaded) return;
            var problems = AllProblemTexts().ToList();
            if (problems.Count > 0)
                Say("Comprobado: " + Count(problems.Count, "problema", "problemas") + ". " + string.Join(" · ", problems.Take(2)) + (problems.Count > 2 ? " · …" : string.Empty), true);
            else if (_conflicts.Count > 0)
                Say("Comprobado: sin errores, pero " + ProjectVariablesBatch.List(_conflicts.Select(c => NameOfRack(c.RackId)))
                    + (_conflicts.Count == 1 ? " tiene" : " tienen") + " cambios sin escribir y usa una variable que cambia.", true);
            else if (IsDirty)
                Say("Comprobado: sin errores. «Actualizar» escribirá " + Count(_changes.Count, "cambio", "cambios") + " y redibujará "
                    + Count(RedrawNames.Count, "rack", "racks") + ".", false);
            else
                Say("Sin cambios: la página coincide con lo último que se leyó del dibujo o se escribió en él.", false);
        }

        public void OpenClassic() => ClassicRequested?.Invoke();

        public void Select(VariableRowItem row)
        {
            if (row != null && !Rows.Contains(row)) return;
            if (!ReferenceEquals(_selected, row)) _askingUnlinkKey = null;
            _selected = row;
            foreach (var item in Rows) item.IsSelected = ReferenceEquals(item, row);
            InspectorName = row?.Name ?? string.Empty;
            InspectorDefinition = row == null ? string.Empty : Shown(row.KeyForm);
            FillConsumers();
            RaiseInspector();
        }

        public void NewVariable()
        {
            if (!CanEditVariables) return;
            var row = new VariableRowItem("nueva-" + (++_nextKey).ToString(CultureInfo.InvariantCulture), true, string.Empty, string.Empty);
            Rows.Add(row);
            Select(row);
            Recompute();
        }

        /// <summary>The name of the inspector goes to the draft (leaving the box, or Enter); leaving it unchanged does nothing.</summary>
        public void CommitInspectorName()
        {
            if (_selected == null || InspectorReadOnly) return;
            if (string.Equals(InspectorName, _selected.Name, StringComparison.Ordinal)) return;
            _selected.Name = InspectorName;
            Recompute();
        }

        /// <summary>The value or formula of the inspector goes to the draft; its names become identities, so a later rename keeps it.</summary>
        public void CommitInspectorDefinition()
        {
            if (_selected == null || InspectorReadOnly) return;
            if (string.Equals(InspectorDefinition, Shown(_selected.KeyForm), StringComparison.Ordinal)) return;
            _selected.KeyForm = ProjectVariableFormulaText.Encode(InspectorDefinition, Rows.Where(r => !r.IsRemoved).Select(r => (r.Key, r.Name)).ToList());
            Recompute();
            InspectorDefinition = Shown(_selected.KeyForm);
        }

        public void Delete()
        {
            if (!CanDelete) return;
            if (_selected.IsNew)
            {
                var index = Rows.IndexOf(_selected);
                Rows.Remove(_selected);
                Select(Rows.Count == 0 ? null : Rows[Math.Min(index, Rows.Count - 1)]);
            }
            else
            {
                _selected.Removal = ProjectVariableDraftRemoval.Delete;
            }

            _askingUnlinkKey = null;
            Recompute();
        }

        public void UndoDelete()
        {
            if (!CanEdit || _selected == null || !_selected.IsRemoved) return;
            _selected.Removal = ProjectVariableDraftRemoval.None;
            Recompute();
        }

        public void AskUnlink()
        {
            if (!CanUnlink) return;
            _askingRepair = null;
            _askingUnlinkKey = _selected.Key;
            RaiseInspector();
            RaiseRepair();
        }

        public void ConfirmUnlink()
        {
            if (!IsAskingUnlink || !CanUnlink) return;
            _selected.Removal = ProjectVariableDraftRemoval.UnlinkAllAndDelete;
            _askingUnlinkKey = null;
            Recompute();
        }

        public void AskRepair(BrokenRackItem rack)
        {
            if (!CanEdit || rack == null || !rack.CanRepair || rack.IsQueued || !Broken.Contains(rack)) return;
            _askingUnlinkKey = null;
            _askingRepair = rack;
            RaiseRepair();
            RaiseInspector();
        }

        public void ConfirmRepair()
        {
            if (_askingRepair == null || !CanEdit) return;
            _askingRepair.IsQueued = true;
            _askingRepair = null;
            Recompute();
        }

        public void UndoRepair(BrokenRackItem rack)
        {
            if (!CanEdit || rack == null || !rack.IsQueued) return;
            rack.IsQueued = false;
            Recompute();
        }

        public void CancelAsk()
        {
            if (_askingRepair == null && _askingUnlinkKey == null) return;
            _askingRepair = null;
            _askingUnlinkKey = null;
            RaiseRepair();
            RaiseInspector();
        }

        public void OpenRack(string rackId)
        {
            if (!string.IsNullOrWhiteSpace(rackId)) OpenRackRequested?.Invoke(rackId);
        }

        public void AddProperty()
        {
            if (!CanAddProperty) return;
            Properties.Add(new CustomPropertyRowItem("nueva-" + (++_nextKey).ToString(CultureInfo.InvariantCulture), null, string.Empty, string.Empty));
            Recompute();
        }

        public void RemoveProperty(CustomPropertyRowItem row)
        {
            if (!CanEdit || row == null || !Properties.Remove(row)) return;
            Recompute();
        }

        /// <summary>A name or a value of the table of properties was left (or Enter): the page judges the draft again.</summary>
        public void CommitProperties()
        {
            if (!IsLoaded) return;
            Recompute();
        }

        // ------------------------------------------------------------------ the analysis

        private ProjectVariablesBatchRequest BatchRequest()
            => new ProjectVariablesBatchRequest(
                Rows.Select(r => new ProjectVariableDraftRow(r.Key, r.IsNew, r.Name,
                    r.IsNew || r.DefinitionChanged ? r.KeyForm : null, r.Removal)).ToList(),
                Broken.Where(b => b.IsQueued).Select(b => b.RackId).ToList());

        private void Recompute()
        {
            if (_read == null)
            {
                RaiseAll();
                return;
            }

            _analysis = IsNotBlocked || Broken.Any(b => b.IsQueued)
                ? ProjectVariablesBatch.Plan(_read.Registry, _read.Entries, IsBlocked
                    ? new ProjectVariablesBatchRequest(Array.Empty<ProjectVariableDraftRow>(), Broken.Where(b => b.IsQueued).Select(b => b.RackId).ToList())
                    : BatchRequest())
                : null;
            _propertyProblems = ProjectPropertiesWritable
                ? CustomPropertiesBatch.Check(_read.ProjectProperties, Properties.Select(p => p.ToDraft()).ToList())
                : Array.Empty<CustomPropertyDraftProblem>();

            for (var i = 0; i < Rows.Count; i++) Describe(Rows[i], i + 1);
            for (var i = 0; i < Properties.Count; i++) DescribeProperty(Properties[i], i);
            _changes = BuildChanges();
            ComputeConflicts();
            FillConsumers();
            RaiseAll();
        }

        private void Describe(VariableRowItem row, int position)
        {
            row.Position = position;
            var bad = !row.IsRemoved && (ErrorOf(row, ProjectVariableDraftField.Name).Length > 0 || ErrorOf(row, ProjectVariableDraftField.Definition).Length > 0);
            row.HasProblem = bad;
            row.Tag = row.IsRemoved ? "Se eliminará" : bad ? "No válida" : row.IsNew ? "Nueva" : row.NameChanged || row.DefinitionChanged ? "Modificada" : string.Empty;
            row.DefinitionShown = Shown(row.KeyForm) is { Length: > 0 } shown ? shown : "—";
            var after = FinalValue(row.Key);
            var before = BaseValue(row.Key);
            row.ValueText = row.IsRemoved ? "—"
                : ValueOf(after) + (!row.IsNew && after.HasValue && before.HasValue && !Same(before, after) ? " (antes " + Format(before.Value) + ")" : string.Empty);
            row.RacksText = row.IsNew ? "—" : ConsumerNames(row.Key).Count.ToString(CultureInfo.InvariantCulture);
        }

        private void DescribeProperty(CustomPropertyRowItem row, int index)
        {
            row.Position = index + 1;
            var before = _baseProperties.FirstOrDefault(b => string.Equals(b.Id, row.Id, StringComparison.OrdinalIgnoreCase));
            row.Tag = row.IsNew ? "Nueva"
                : before != null && (!string.Equals(before.Name, row.Name.Trim(), StringComparison.Ordinal) || !string.Equals(before.Value, row.Value, StringComparison.Ordinal))
                    ? "Modificada" : string.Empty;
            row.NameError = string.Join(" ", _propertyProblems.Where(p => p.Index == index && p.Field == CustomPropertyDraftField.Name).Select(p => p.Text));
            row.ValueError = string.Join(" ", _propertyProblems.Where(p => p.Index == index && p.Field == CustomPropertyDraftField.Value).Select(p => p.Text));
        }

        private IReadOnlyList<string> BuildChanges()
        {
            var changes = new List<string>();
            if (_analysis != null)
            {
                foreach (var row in Rows)
                {
                    var shown = Shown(row.KeyForm).Trim();
                    if (row.IsNew)
                    {
                        if (row.IsRemoved) continue;
                        var value = FinalValue(row.Key);
                        changes.Add(shown.StartsWith("=", StringComparison.Ordinal)
                            ? "Nueva: «" + row.DisplayName + "», con la fórmula " + shown + (value.HasValue ? " (" + Format(value.Value) + " in)" : string.Empty) + "."
                            : "Nueva: «" + row.DisplayName + "» = " + (shown.Length == 0 ? "—" : shown) + " in.");
                        continue;
                    }

                    if (row.Removal == ProjectVariableDraftRemoval.Delete)
                    {
                        changes.Add("Eliminar «" + row.BaseName + "».");
                        continue;
                    }

                    if (row.Removal == ProjectVariableDraftRemoval.UnlinkAllAndDelete)
                    {
                        var racks = ConsumerNames(row.Key);
                        changes.Add("Desvincular todos y eliminar «" + row.BaseName + "»: " + ProjectVariablesBatch.List(racks)
                                    + (racks.Count == 1 ? " deja de seguirla y conserva " : " dejan de seguirla y conservan ") + KeptText(row.Key) + ".");
                        continue;
                    }

                    if (row.NameChanged) changes.Add("Renombrar «" + row.BaseName + "» a «" + row.DisplayName + "». No redibuja ningún rack.");
                    if (row.DefinitionChanged)
                    {
                        var was = Shown(row.BaseKeyForm).Trim();
                        var before = BaseValue(row.Key);
                        var after = FinalValue(row.Key);
                        changes.Add(!was.StartsWith("=", StringComparison.Ordinal) && !shown.StartsWith("=", StringComparison.Ordinal)
                            ? "«" + row.DisplayName + "»: " + was + " → " + shown + " in."
                            : "«" + row.DisplayName + "»: " + was + " → " + shown
                              + (before.HasValue && after.HasValue && !Same(before, after) ? " (" + Format(before.Value) + " → " + Format(after.Value) + " in)" : string.Empty) + ".");
                    }
                }
            }

            foreach (var rack in Broken.Where(b => b.IsQueued))
                changes.Add("Reparar " + rack.RackName + ": " + string.Join("; ", rack.Bindings.Select(b => b.Label.ToLower(CultureInfo.CurrentCulture) + " queda en " + Format(b.Literal) + " in")) + ".");

            if (PropertiesChanged)
            {
                var draft = Properties.Select(p => p.ToDraft()).ToList();
                var added = draft.Count(d => d.IsNew);
                var removed = _baseProperties.Count(b => draft.All(d => !string.Equals(d.Id, b.Id, StringComparison.OrdinalIgnoreCase)));
                var changed = draft.Count(d => !d.IsNew && _baseProperties.Any(b => string.Equals(b.Id, d.Id, StringComparison.OrdinalIgnoreCase)
                    && (!string.Equals(b.Name, (d.Name ?? string.Empty).Trim(), StringComparison.Ordinal) || !string.Equals(b.Value, d.Value, StringComparison.Ordinal))));
                var parts = new List<string>();
                if (added > 0) parts.Add(added + (added == 1 ? " nueva" : " nuevas"));
                if (changed > 0) parts.Add(changed + (changed == 1 ? " cambiada" : " cambiadas"));
                if (removed > 0) parts.Add(removed + (removed == 1 ? " quitada" : " quitadas"));
                if (parts.Count == 0) parts.Add("cambia el orden");
                changes.Add("Propiedades del proyecto: " + string.Join(", ", parts) + ". No redibuja ningún rack.");
            }

            return changes;
        }

        private void ComputeConflicts()
        {
            var redrawn = _analysis?.RedrawnRackIds ?? Array.Empty<string>();
            var conflicts = redrawn.Count == 0 || _conflictsOf == null ? Array.Empty<(string RackId, string Name)>() : _conflictsOf(redrawn) ?? Array.Empty<(string, string)>();
            _conflicts = conflicts.Select(c =>
            {
                var name = string.IsNullOrWhiteSpace(c.Name) ? NameOfRack(c.RackId) : c.Name.Trim();
                if (!_rackNames.ContainsKey(c.RackId)) _rackNames[c.RackId] = name;
                return new VariablesConflictItem(c.RackId, name + " tiene cambios sin escribir",
                    "Usa una variable que cambia en este borrador. Escribe o descarta antes su borrador: si no, su «Actualizar» se negaría después, "
                    + "porque el valor de la variable ya no sería el que leyó.", "Ir a " + name);
            }).ToList();
        }

        private void FillConsumers()
        {
            Consumers.Clear();
            if (_selected == null || _selected.IsNew || _workspace == null) return;
            var row = _workspace.Variables.FirstOrDefault(v => string.Equals(v.Id.Value, _selected.Key, StringComparison.OrdinalIgnoreCase));
            if (row == null) return;
            foreach (var consumer in row.Consumers)
            {
                var name = RackName(consumer.RackId, consumer.RackName);
                Consumers.Add(new VariableConsumerItem(consumer.RackId,
                    name + " · " + string.Join(", ", consumer.PropertyIds.Select(PropertyLabel)), "Abrir " + name + " en su página"));
            }
        }

        private IEnumerable<string> AllProblemTexts()
        {
            if (_analysis != null)
                foreach (var error in _analysis.Errors)
                {
                    var row = error.Key == null ? null : RowOf(error.Key);
                    yield return row != null ? "«" + row.DisplayName + "»: " + error.Text : error.Text;
                }

            foreach (var problem in _propertyProblems) yield return "Propiedad del proyecto: " + problem.Text;
        }

        private int VariableProblems => _analysis?.Errors.Count ?? 0;

        private int ProblemCount => VariableProblems + _propertyProblems.Count;

        private string ErrorOf(VariableRowItem row, ProjectVariableDraftField field)
            => row == null || _analysis == null ? string.Empty
                : string.Join(" ", _analysis.Errors.Where(e => string.Equals(e.Key, row.Key, StringComparison.OrdinalIgnoreCase) && e.Field == field).Select(e => e.Text));

        private IReadOnlyList<string> DependentsOf(string key)
            => key != null && _analysis != null && _analysis.Dependents.TryGetValue(key, out var users) ? users : Array.Empty<string>();

        private IReadOnlyList<string> ConsumerNames(string key)
        {
            var row = _workspace?.Variables.FirstOrDefault(v => string.Equals(v.Id.Value, key, StringComparison.OrdinalIgnoreCase));
            return row == null ? Array.Empty<string>() : row.Consumers.Select(c => RackName(c.RackId, c.RackName)).Distinct().ToList();
        }

        private string KeptText(string key)
        {
            var value = _analysis != null && _analysis.KeptValues.TryGetValue(key, out var kept) ? kept : BaseValue(key);
            return value.HasValue ? Format(value.Value) + " in" : "su valor";
        }

        private VariableRowItem RowOf(string key) => Rows.FirstOrDefault(r => string.Equals(r.Key, key, StringComparison.OrdinalIgnoreCase));

        private double? FinalValue(string key) => _analysis != null && _analysis.FinalValues.TryGetValue(key, out var v) ? v : null;

        private double? BaseValue(string key) => _analysis != null && _analysis.BaseValues.TryGetValue(key, out var v) ? v
            : _workspace?.Variables.FirstOrDefault(r => string.Equals(r.Id.Value, key, StringComparison.OrdinalIgnoreCase)) is { EvaluationSucceeded: true } found ? found.EvaluatedValue : null;

        private string Shown(string keyForm) => ProjectVariableFormulaText.Decode(keyForm ?? string.Empty, key => RowOf(key)?.Name);

        private string NameOfRack(string rackId) => _rackNames.TryGetValue(rackId ?? string.Empty, out var name) ? name : "un rack sin nombre";

        private string RackName(string rackId, string name)
        {
            if (!string.IsNullOrWhiteSpace(name)) return name.Trim();
            return NameOfRack(rackId);
        }

        internal static string PropertyLabel(string propertyId)
            => string.Equals(propertyId, ProjectPropertyIds.SelectiveVerticalClearance.Value, StringComparison.Ordinal) ? "Holgura vertical"
                : string.Equals(propertyId, ProjectPropertyIds.SelectivePalletTolerance.Value, StringComparison.Ordinal) ? "Tolerancia horizontal"
                : "Otra propiedad";

        private static string ValueOf(double? value) => value.HasValue ? Format(value.Value) + " in" : "Sin valor";

        private static bool Same(double? a, double? b) => a.HasValue && b.HasValue && Math.Abs(a.Value - b.Value) < 1e-9;

        private static string Format(double value) => Math.Round(value, 3).ToString("0.###", CultureInfo.InvariantCulture);

        private static string Count(int count, string one, string many) => count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);

        private void RaiseInspector()
        {
            foreach (var name in new[]
                     {
                         nameof(Selected), nameof(HasSelection), nameof(HasNoSelection), nameof(InspectorTitle), nameof(InspectorTag),
                         nameof(InspectorReadOnly), nameof(InspectorNameError), nameof(InspectorDefinitionError), nameof(InspectorUnit),
                         nameof(InspectorValueText), nameof(InspectorValueHint), nameof(RemovedText), nameof(IsSelectedRemoved), nameof(HasConsumers), nameof(HasNoConsumers), nameof(HasDependents),
                         nameof(NoConsumersText), nameof(DependentsText), nameof(DeleteLabel), nameof(CanDelete), nameof(ShowUnlink), nameof(CanUnlink),
                         nameof(DeleteWhy), nameof(IsAskingUnlink), nameof(UnlinkTitle), nameof(UnlinkText),
                     })
                OnPropertyChanged(name);
        }

        private void RaiseRepair()
        {
            OnPropertyChanged(nameof(IsAskingRepair));
            OnPropertyChanged(nameof(RepairTitle));
            OnPropertyChanged(nameof(RepairYesLabel));
            OnPropertyChanged(nameof(RepairText));
        }

        private void RaiseState()
        {
            foreach (var name in new[]
                     {
                         nameof(IsDirty), nameof(StatusText), nameof(DiffText), nameof(CanUpdate), nameof(CanDiscard), nameof(CanRead),
                         nameof(UpdateBlockedText), nameof(Changes), nameof(HasChanges), nameof(RedrawText), nameof(Conflicts), nameof(WritingText),
                         nameof(VariablesTabHeader), nameof(PropertiesTabHeader), nameof(RedrawnRackIds),
                     })
                OnPropertyChanged(name);
        }

        private void RaiseAll()
        {
            foreach (var name in new[]
                     {
                         nameof(IsLoaded), nameof(IsNotLoaded), nameof(IsReading), nameof(IsWriting), nameof(NotLoadedText), nameof(IsBlocked),
                         nameof(IsNotBlocked), nameof(BlockedText), nameof(CanEdit), nameof(CanEditVariables), nameof(CountText), nameof(HasNoVariables),
                         nameof(HasBroken), nameof(PropertiesCount), nameof(CanAddProperty), nameof(ProjectPropertiesWritable),
                         nameof(ProjectPropertiesNote), nameof(HasNoProperties), nameof(PropertiesRemovedText), nameof(PropertiesNote), nameof(CanEditProperties),
                     })
                OnPropertyChanged(name);
            RaiseState();
            RaiseInspector();
            RaiseRepair();
        }
    }
}
