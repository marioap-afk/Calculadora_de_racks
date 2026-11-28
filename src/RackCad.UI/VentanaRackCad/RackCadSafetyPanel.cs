using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// What the editable safety of «Extremos y seguridad» needs from the editor of a system (the Selectivo or the Dinámico of the window):
    /// its safety, its table counted as the BOM, and ONE way to write the safety of the draft (a copy is edited, the draft takes it and is
    /// recomposed; an edit that refuses writes nothing). Implemented explicitly by the two editors (T4 seg).
    /// </summary>
    internal interface IRackCadSafetyPanelHost
    {
        RackCadSafetySystem SafetySystem { get; }

        RackCatalog SafetyCatalog { get; }

        bool CanEditSafetyPanel { get; }

        /// <summary>Why nothing can be written now (AutoCAD writes, the rack opened read only).</summary>
        string SafetyBlockedText { get; }

        int SafetyFondoCount { get; }

        /// <summary>The posts of the bota, the protector lateral and the defense (N fronts, N + 1 posts).</summary>
        int SafetyPostCount { get; }

        IReadOnlyList<SelectiveSafetySelection> CopySafetyForPanel();

        /// <summary>The rows of the table, counted as the BOM counts them on the last valid recomposition.</summary>
        IReadOnlyList<RackCadSafetyRow> SafetyRows();

        /// <summary>
        /// Writes the safety: <paramref name="edit"/> changes a copy and returns null, or the reason it refuses (nothing is written); the draft
        /// takes the copy and is recomposed; <paramref name="notice"/> (when not null) is said after. Returns the error, or null.
        /// </summary>
        string WriteSafety(Func<List<SelectiveSafetySelection>, string> edit, string notice);

        /// <summary>A family of the cells (tope, parrilla, guía): the cells with it, the cells it can be in, and the pieces the BOM counts.</summary>
        (int On, int Total, int Pieces) CellFamily(string type);

        /// <summary>«En todas las celdas» / «En ninguna» of a family of the cells.</summary>
        string SetCellFamilyEverywhere(string type, bool on);

        /// <summary>«Elegir celdas en «Matriz»».</summary>
        void ShowMatrix();

        /// <summary>The parrilla plan of the last valid recomposition (Selectivo); empty otherwise.</summary>
        IReadOnlyList<SelectiveParrillaPlan.Cell> SafetyParrillaPlan { get; }

        /// <summary>The narrowest clearance under the length of the desviador (Selectivo); null when none or not known.</summary>
        SelectiveDesviadorPlan.ClearanceIssue SafetyDesviadorClearance { get; }
    }

    /// <summary>
    /// The inspector of one family in «Extremos y seguridad» (T4 seg, SG-01): its variant, its measures, what it places, and its buttons.
    /// «Sin variante» asks inline first what is lost (SG-06). Its fields are refreshed in place, so the box being written survives a write.
    /// </summary>
    public sealed class RackCadSafetyInspector : ObservableObject
    {
        private string _type = string.Empty;
        private string _title = string.Empty;
        private string _letter = string.Empty;
        private string _sub = string.Empty;
        private SelectiveChoiceItem _variant;
        private bool _isAsking;
        private string _askTitle = string.Empty;
        private string _askYesLabel = string.Empty;
        private IReadOnlyList<string> _askLoss = Array.Empty<string>();
        private bool _hasVariant;
        private IReadOnlyList<object> _fields = Array.Empty<object>();
        private string _warning = string.Empty;
        private IReadOnlyList<string> _notes = Array.Empty<string>();
        private string _pieces = string.Empty;
        private IReadOnlyList<RackCadSafetyAction> _actions = Array.Empty<RackCadSafetyAction>();

        internal Action ConfirmRemoveAction;
        internal Action CancelRemoveAction;

        /// <summary>What the inspector says in the line of messages of the editor (a contained failure of its view).</summary>
        internal Action<string, bool> SayAction;

        public string Type { get => _type; private set => SetProperty(ref _type, value); }

        public string Title { get => _title; private set => SetProperty(ref _title, value); }

        public string Letter { get => _letter; private set { if (SetProperty(ref _letter, value ?? string.Empty)) OnPropertyChanged(nameof(HasLetter)); } }

        public bool HasLetter => _letter.Length > 0;

        public string Sub { get => _sub; private set => SetProperty(ref _sub, value ?? string.Empty); }

        /// <summary>«Variante»: «Sin variante (no se dibuja)» and the variants of the catalog.</summary>
        public SelectiveChoiceItem Variant { get => _variant; private set => SetProperty(ref _variant, value); }

        public bool IsAsking { get => _isAsking; private set => SetProperty(ref _isAsking, value); }

        public string AskTitle { get => _askTitle; private set => SetProperty(ref _askTitle, value ?? string.Empty); }

        public string AskYesLabel { get => _askYesLabel; private set => SetProperty(ref _askYesLabel, value ?? string.Empty); }

        /// <summary>What «Sin variante» takes out of the draft, said before it does.</summary>
        public IReadOnlyList<string> AskLoss { get => _askLoss; private set => SetProperty(ref _askLoss, value); }

        public bool HasVariant { get => _hasVariant; private set { if (SetProperty(ref _hasVariant, value)) OnPropertyChanged(nameof(HasNoVariant)); } }

        public bool HasNoVariant => !_hasVariant;

        public string NoVariantText => "Sin variante elegida: no se dibuja ni entra en el BOM. Elige una para ponerla con sus valores por defecto.";

        /// <summary>The measures: numbers (<see cref="DynamicFieldItem"/>), lists (<see cref="SelectiveChoiceItem"/>), boxes and texts.</summary>
        public IReadOnlyList<object> Fields { get => _fields; private set => SetProperty(ref _fields, value); }

        public string Warning { get => _warning; private set => SetProperty(ref _warning, value ?? string.Empty); }

        public IReadOnlyList<string> Notes { get => _notes; private set => SetProperty(ref _notes, value); }

        /// <summary>What the drawing places, counted as the BOM: «En el dibujo: 6 botas, en 3 de 3 postes.»</summary>
        public string Pieces { get => _pieces; private set => SetProperty(ref _pieces, value ?? string.Empty); }

        public IReadOnlyList<RackCadSafetyAction> Actions { get => _actions; private set => SetProperty(ref _actions, value); }

        /// <summary>«Quitar …» of the inline question.</summary>
        public void ConfirmRemove() => ConfirmRemoveAction?.Invoke();

        /// <summary>«Conservar» of the inline question.</summary>
        public void CancelRemove() => CancelRemoveAction?.Invoke();

        internal void Set(string type, string title, string letter, string sub, SelectiveChoiceItem variant, bool hasVariant, bool isAsking, string askTitle,
            string askYes, IReadOnlyList<string> askLoss, IReadOnlyList<object> fields, string warning, IReadOnlyList<string> notes, string pieces,
            IReadOnlyList<RackCadSafetyAction> actions)
        {
            var sameFamily = string.Equals(_type, type, StringComparison.Ordinal);
            Type = type;
            Title = title;
            Letter = letter;
            Sub = sub;
            if (sameFamily && _variant != null) _variant.Refresh(variant);
            else Variant = variant;
            HasVariant = hasVariant;
            IsAsking = isAsking;
            AskTitle = askTitle;
            AskYesLabel = askYes;
            if (!_askLoss.SequenceEqual(askLoss)) AskLoss = askLoss;
            MergeFields(sameFamily, fields);
            Warning = warning;
            if (!_notes.SequenceEqual(notes)) Notes = notes;
            Pieces = pieces;
            MergeActions(sameFamily, actions);
        }

        private static string KeyOf(object field)
            => field switch
            {
                DynamicFieldItem f => "n:" + f.Key,
                SelectiveChoiceItem c => "c:" + c.Key,
                RackCadSafetyCheckField k => "k:" + k.Key,
                RackCadSafetyTextField t => "t:" + t.Key,
                _ => string.Empty,
            };

        private void MergeFields(bool sameFamily, IReadOnlyList<object> next)
        {
            if (sameFamily && _fields.Count == next.Count && _fields.Select(KeyOf).SequenceEqual(next.Select(KeyOf), StringComparer.Ordinal))
            {
                for (var i = 0; i < next.Count; i++)
                {
                    switch (_fields[i])
                    {
                        case DynamicFieldItem f: f.Refresh((DynamicFieldItem)next[i]); break;
                        case SelectiveChoiceItem c: c.Refresh((SelectiveChoiceItem)next[i]); break;
                        case RackCadSafetyCheckField k: k.Refresh((RackCadSafetyCheckField)next[i]); break;
                        case RackCadSafetyTextField t: t.Refresh((RackCadSafetyTextField)next[i]); break;
                    }
                }

                return;
            }

            Fields = next;
        }

        private void MergeActions(bool sameFamily, IReadOnlyList<RackCadSafetyAction> next)
        {
            if (sameFamily && _actions.Count == next.Count && _actions.Select(a => a.Key).SequenceEqual(next.Select(a => a.Key), StringComparer.Ordinal))
            {
                for (var i = 0; i < next.Count; i++) _actions[i].Refresh(next[i]);
                return;
            }

            Actions = next;
        }
    }

    /// <summary>
    /// «Seguridad por poste» editable and the inspector of the families, without the classic dialog (I-64, cuarta tanda, part «seg»; the
    /// prototype «seguridad», SG-01 to SG-08), shared by the Selectivo and the Dinámico editors of the window. Each system keeps its own words
    /// for the sides (SG-02, note of the Owner): the Selectivo «Izquierda / Derecha», the Dinámico «Salida / Entrada». Everything goes to the
    /// draft at once (a list, a level, a box) or when the field is left (a measure), as the rest of the editor; a value refused stays in its
    /// field and writes nothing. «Actualizar» writes the drawing.
    /// </summary>
    public sealed partial class RackCadSafetyPanelViewModel : ObservableObject // T5 pb: partial (RackCadSafetyPanel.PushBack.cs)
    {
        /// <summary>The code of «General (…)» in the list of the bota of a post.</summary>
        internal const string General = "g";

        /// <summary>The code of «Sin variante (no se dibuja)» in the list of the variant (a list item cannot be empty).</summary>
        internal const string NoVariant = "-";

        private readonly IRackCadSafetyPanelHost _host;
        private string _family = SelectiveSafetyDefaults.BotaType;
        private string _asking;
        private IReadOnlyList<RackCadSafetyRowItem> _table = Array.Empty<RackCadSafetyRowItem>();

        internal RackCadSafetyPanelViewModel(IRackCadSafetyPanelHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            Inspector.ConfirmRemoveAction = ConfirmRemove;
            Inspector.CancelRemoveAction = CancelRemove;
            Inspector.SayAction = (text, error) => Say(text, error);
        }

        public bool IsDynamic => _host.SafetySystem == RackCadSafetySystem.Dynamic;

        public bool IsSelective => _host.SafetySystem == RackCadSafetySystem.Selective; // T5 pb: Push Back is neither

        /// <summary>The rows of the table, one per post, and «Total» last (the foot of the approved prototype).</summary>
        public IReadOnlyList<RackCadSafetyRowItem> Table => _table;

        public RackCadSafetyInspector Inspector { get; } = new RackCadSafetyInspector();

        /// <summary>The family the inspector shows.</summary>
        public string Family => _family;

        public bool BootsPicked => Is(_family, SelectiveSafetyDefaults.BotaType);

        public bool SideGuardPicked => Is(_family, SelectiveSafetyDefaults.LateralType);

        public bool DeflectorsPicked => Is(_family, SelectiveSafetyDefaults.DesviadorType);

        public bool DefensePicked => Is(_family, SelectiveSafetyDefaults.DefensaType);

        public string PostHelp => "Cada fila es un poste. Lo que cambias va al borrador en el momento (una medida, al salir del campo), y «Actualizar» lo dibuja. "
                                  + "Pulsa el título de una columna para ver su variante y sus medidas en «Variante y medidas».";

        public string PostFoot => IsPushBack ? PushBackPostFoot // T5 pb
            : IsDynamic
            ? "Contado como el BOM: un protector lateral reemplaza las botas de su poste; con «Salida y entrada», un desviador cuenta dos piezas; cada celda con guía, una pareja."
            : "Contado como el BOM: un protector lateral reemplaza las botas de su poste; con «" + RackCadSafetyWords.Side(RackCadSafetySystem.Selective, SafetySide.Both)
              + "», un desviador cuenta dos piezas.";

        /// <summary>The families of the system, in the order of the prototype.</summary>
        public IReadOnlyList<string> Families => RackCadSafetyEdit.FamiliesOf(_host.SafetySystem);

        /// <summary>The inspector shows that family (a title of a column, or the head of a card).</summary>
        public void Pick(string type)
        {
            if (string.IsNullOrWhiteSpace(type) || !Families.Any(f => Is(f, type))) return;
            _family = Families.First(f => Is(f, type));
            _asking = null;
            OnPropertyChanged(nameof(Family));
            OnPropertyChanged(nameof(BootsPicked));
            OnPropertyChanged(nameof(SideGuardPicked));
            OnPropertyChanged(nameof(DeflectorsPicked));
            OnPropertyChanged(nameof(DefensePicked));
            RefreshInspector();
        }

        /// <summary>The draft changed, or what can be edited: the table and the inspector follow, in place.</summary>
        public void Refresh()
        {
            RefreshTable();
            RefreshInspector();
        }

        // ------------------------------------------------------------------ the table

        private void RefreshTable()
        {
            if (IsPushBack) { RefreshPushBackTable(); return; } // T5 pb: its botas and its defense are by side (RackCadSafetyPanel.PushBack.cs)
            var system = _host.SafetySystem;
            var safety = _host.CopySafetyForPanel();
            var catalog = _host.SafetyCatalog;
            var bota = RackCadSafetyEdit.Selected(safety, catalog, SelectiveSafetyDefaults.BotaType);
            var lateral = RackCadSafetyEdit.Selected(safety, catalog, SelectiveSafetyDefaults.LateralType);
            var desviador = RackCadSafetyEdit.Selected(safety, catalog, SelectiveSafetyDefaults.DesviadorType);
            var defensa = RackCadSafetyEdit.Selected(safety, catalog, SelectiveSafetyDefaults.DefensaType);
            var guia = RackCadSafetyEdit.Selected(safety, catalog, SelectiveSafetyDefaults.GuiaType);
            var rows = _host.SafetyRows();
            var posts = _host.SafetyPostCount;
            var edit = _host.CanEditSafetyPanel;
            var several = _host.SafetyFondoCount > 1;

            var keys = rows.Select(r => r.Post).Concat(new[] { "Total" }).ToList();
            if (!_table.Select(r => r.Post).SequenceEqual(keys, StringComparer.Ordinal))
            {
                _table = rows.Select(r => new RackCadSafetyRowItem(r.Post, false)).Concat(new[] { new RackCadSafetyRowItem("Total", true) }).ToList();
                OnPropertyChanged(nameof(Table));
            }

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var item = _table[i];
                item.Kind = row.Kind;
                SetBoots(item, row, bota, system, edit);
                SetSideGuard(item, row, lateral, system, posts, edit);
                SetDeflectors(item, row, desviador, edit);
                item.Separators = several && row.FramePost.HasValue ? N(row.Separators) : "—";
                SetDefense(item.DefenseExit, row, defensa, RackCadDefenseEnd.Exit, posts, edit);
                SetDefense(item.DefenseEntrance, row, defensa, RackCadDefenseEnd.Entrance, posts, edit);
                item.Guides = guia == null ? "—" : N(row.Guides);
            }

            var total = _table[_table.Count - 1];
            total.Kind = string.Empty;
            total.Boots.Set(null, Dash(bota != null, rows.Sum(r => r.Boots)), string.Empty, false);
            total.SideGuard.Set(null, Dash(lateral != null, rows.Sum(r => r.SideGuards)), string.Empty, false);
            total.Deflectors.Set(0, _ => default, Dash(desviador != null, rows.Sum(r => r.Deflectors)), string.Empty, string.Empty);
            total.Separators = several ? N(rows.Sum(r => r.Separators)) : "—";
            total.DefenseExit.Set(false, false, false, string.Empty, null, Dash(defensa != null, rows.Count(r => r.DefenseExit)), string.Empty, false, null);
            total.DefenseEntrance.Set(false, false, false, string.Empty, null, Dash(defensa != null, rows.Count(r => r.DefenseEntrance)), string.Empty, false, null);
            total.Guides = Dash(guia != null, rows.Sum(r => r.Guides));
        }

        private void SetBoots(RackCadSafetyRowItem item, RackCadSafetyRow row, SelectiveSafetySelection bota, RackCadSafetySystem system, bool edit)
        {
            if (bota == null || !row.FramePost.HasValue)
            {
                item.Boots.Set(null, "—", string.Empty, false);
                return;
            }

            if (row.SideGuards > 0)
            {
                item.Boots.Set(null, "La reemplaza el protector lateral", string.Empty, false);
                return;
            }

            var post = row.FramePost.Value;
            var own = RackCadSafetyEdit.BootOwn(bota, post);
            var options = new[] { new SelectiveChoiceOption(General, "General (" + RackCadSafetyWords.Side(system, RackCadSafetyEdit.BootGeneral(bota)) + ")") }
                .Concat(new[] { SafetySide.None, SafetySide.Left, SafetySide.Right, SafetySide.Both }.Select(s => new SelectiveChoiceOption(Code(s), RackCadSafetyWords.Side(system, s))))
                .ToList();
            var choice = new SelectiveChoiceItem("boot" + N(post), "Botas en " + row.Post, options, own.HasValue ? Code(own.Value) : General, string.Empty,
                value => _host.WriteSafety(copy =>
                {
                    RackCadSafetyEdit.SetBootPost(Find(copy, SelectiveSafetyDefaults.BotaType), post, value == General ? (SafetySide?)null : Side(value));
                    return null;
                }, null), !edit);
            item.Boots.Set(choice, string.Empty, (own.HasValue ? "Propia · " : string.Empty) + Plural(row.Boots, "pieza", "piezas"), own.HasValue);
        }

        private void SetSideGuard(RackCadSafetyRowItem item, RackCadSafetyRow row, SelectiveSafetySelection lateral, RackCadSafetySystem system, int posts, bool edit)
        {
            if (lateral == null || !row.FramePost.HasValue)
            {
                item.SideGuard.Set(null, "—", string.Empty, false);
                return;
            }

            var post = row.FramePost.Value;
            var rule = RackCadSafetyEdit.LateralFollowsRule(lateral, system);
            var side = RackCadSafetyEdit.LateralAt(lateral, post, posts, system);
            var options = new[] { SafetySide.None, SafetySide.Left, SafetySide.Right, SafetySide.Both }.Select(s => new SelectiveChoiceOption(Code(s), RackCadSafetyWords.SideGuard(system, s))).ToList();
            var choice = new SelectiveChoiceItem("lateral" + N(post), "Protector lateral en " + row.Post, options, Code(side), string.Empty, value =>
            {
                var left = false;
                var error = _host.WriteSafety(copy =>
                {
                    left = RackCadSafetyEdit.SetLateralPost(Find(copy, SelectiveSafetyDefaults.LateralType), post, Side(value), _host.SafetyPostCount, system);
                    return null;
                }, null);
                if (error == null && left)
                {
                    Say("El protector lateral va ahora por poste: ya no se recalcula al añadir o quitar frentes. «Volver a la regla de los extremos» lo deshace.");
                }

                return error;
            }, !edit);
            var sub = rule ? "Regla de los extremos" : row.SideGuards > 0 ? Plural(row.SideGuards, "pieza", "piezas") : string.Empty;
            item.SideGuard.Set(choice, string.Empty, sub, false);
        }

        private void SetDeflectors(RackCadSafetyRowItem item, RackCadSafetyRow row, SelectiveSafetySelection desviador, bool edit)
        {
            if (desviador == null || !row.DeflectorKey.HasValue || row.DeflectorLevels <= 0)
            {
                item.Deflectors.Set(0, _ => default, "—", string.Empty, string.Empty);
                return;
            }

            // What the drawing reads at the post, and what an edit of the post writes (the post itself in a Dinámico: the edit first passes a
            // desviador still read the historic way to the reading by post, keeping what it draws; decision of the Owner of 2026-11-26).
            var key = row.DeflectorKey.Value;
            var write = row.DeflectorWriteKey ?? key;
            var sub = Plural(row.Deflectors, "pieza", "piezas");
            item.Deflectors.Set(row.DeflectorLevels, level =>
            {
                var on = RackCadSafetyEdit.DesviadorOn(desviador, key, level);
                var name = row.Post + " · N" + N(level + 1) + (on ? ": con desviador. Pulsa para quitarlo." : ": sin desviador. Pulsa para ponerlo.");
                return (on, edit, name, () => WriteOrSay(copy =>
                {
                    RackCadSafetyEdit.SetDesviador(DesviadorToEdit(copy), write, level, !on);
                    return null;
                }, null));
            }, "—", sub, "Niveles con desviador en " + row.Post);
        }

        private void SetDefense(RackCadSafetyDefenseCell cell, RackCadSafetyRow row, SelectiveSafetySelection defensa, RackCadDefenseEnd end, int posts, bool edit)
        {
            if (!IsDynamic || defensa == null || !row.FramePost.HasValue)
            {
                cell.Set(false, false, false, string.Empty, null, "—", string.Empty, false, null);
                return;
            }

            var post = row.FramePost.Value;
            var state = RackCadSafetyEdit.DefenseAt(defensa, post, end, posts);
            var endName = end == RackCadDefenseEnd.Exit ? "salida" : "entrada";
            var length = new DynamicFieldItem(
                "def" + (end == RackCadDefenseEnd.Exit ? "S" : "E") + N(post),
                "Longitud de la defensa en " + endName + " en " + row.Post + ", en pulgadas; vacía = automática, " + Num(state.Automatic) + " in",
                "in",
                state.OwnLength.HasValue ? Num(state.OwnLength.Value) : string.Empty,
                string.Empty,
                text =>
                {
                    double? value = null;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (!UiSupport.TryNum(text, out var number) || number <= 0.0) return "Debe ser un número mayor que cero, o vacía (automática).";
                        value = number;
                    }

                    return _host.WriteSafety(copy =>
                    {
                        RackCadSafetyEdit.SetDefenseLength(Find(copy, SelectiveSafetyDefaults.DefensaType), post, end, value);
                        return null;
                    }, null);
                },
                !edit || !state.On);
            var sub = !state.On ? "Sin defensa" : state.OwnLength.HasValue ? "Propia" : "Automática · " + Num(state.Automatic) + " in";
            cell.Set(true, state.On, edit, "Defensa en " + endName + " en " + row.Post, length, string.Empty, sub, state.On && state.OwnLength.HasValue,
                on => WriteOrSay(copy =>
                {
                    RackCadSafetyEdit.SetDefenseOn(Find(copy, SelectiveSafetyDefaults.DefensaType), post, end, on);
                    return null;
                }, null));
        }

        // ------------------------------------------------------------------ the inspector

        private void RefreshInspector()
        {
            if (IsPushBack) { RefreshPushBackInspector(); return; } // T5 pb: the families with the authority of Push Back
            var system = _host.SafetySystem;
            var catalog = _host.SafetyCatalog;
            var safety = _host.CopySafetyForPanel();
            var type = _family;
            var selection = RackCadSafetyEdit.Selected(safety, catalog, type);
            var edit = _host.CanEditSafetyPanel;
            var rows = _host.SafetyRows();
            var name = FamilyName(type);
            var variantName = selection == null ? string.Empty : RackCadSafetyEdit.Variants(catalog, type).FirstOrDefault(v => string.Equals(v.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase))?.Label ?? selection.ElementId;
            if (selection == null) _asking = null;

            var options = new[] { new SelectiveChoiceOption(NoVariant, "Sin variante (no se dibuja)") }
                .Concat(RackCadSafetyEdit.Variants(catalog, type).Select(v => new SelectiveChoiceOption(v.Id, v.Label)))
                .ToList();
            var shownVariant = selection == null ? NoVariant
                : options.FirstOrDefault(o => string.Equals(o.Value, selection.ElementId, StringComparison.OrdinalIgnoreCase))?.Value ?? selection.ElementId;
            var variant = new SelectiveChoiceItem("variant", "Variante", options, shownVariant,
                "Del catálogo de seguridad (seguridad.csv). Una sola variante por familia.", value => SetVariant(type, value), !edit);

            var fields = new List<object>();
            var notes = new List<string>();
            var actions = new List<RackCadSafetyAction>();
            var loss = new List<string> { "Variante «" + variantName + "»" };
            var warning = string.Empty;
            var pieces = string.Empty;
            var sub = string.Empty;
            var frames = rows.Where(r => r.FramePost.HasValue).ToList();
            var posts = _host.SafetyPostCount;

            if (Is(type, SelectiveSafetyDefaults.BotaType))
            {
                sub = "En la base del poste. Cada poste, en la tabla.";
                if (selection != null)
                {
                    var general = RackCadSafetyEdit.BootGeneral(selection);
                    fields.Add(new SelectiveChoiceItem("side", "Lado general",
                        new[] { SafetySide.Both, SafetySide.Left, SafetySide.Right, SafetySide.None }.Select(s => new SelectiveChoiceOption(Code(s), RackCadSafetyWords.Side(system, s))).ToList(),
                        Code(general), "Lo toma cada poste que no tenga la suya.",
                        value => _host.WriteSafety(copy => { RackCadSafetyEdit.SetBootGeneral(Find(copy, type), Side(value)); return null; }, null), !edit));
                    var owns = RackCadSafetyEdit.BootOwnCount(selection, posts);
                    pieces = "En el dibujo: " + Plural(frames.Sum(r => r.Boots), "bota", "botas") + ", en " + N(frames.Count(r => r.Boots > 0)) + " de " + Plural(frames.Count, "poste", "postes") + ".";
                    actions.Add(new RackCadSafetyAction("general", "Todos los postes al general", edit && owns > 0, "Botas: todos los postes al general",
                        () => WriteOrSay(copy => { RackCadSafetyEdit.BootPostsToGeneral(Find(copy, type)); return null; }, null)));
                    loss.Add("Lado general «" + RackCadSafetyWords.Side(system, general) + "»");
                    if (owns > 0) loss.Add(Plural(owns, "poste con la suya", "postes con la suya"));
                }

                notes.Add("En la tabla, un poste puede tener la suya: lleva la marca «Propia».");
                notes.Add("Un protector lateral reemplaza las botas de su poste.");
                notes.Add(IsDynamic
                    ? "Salida = extremo bajo; entrada = extremo alto."
                    : "Izquierda = la cara del fondo 1, la cercana; Derecha = la del último fondo. Espejar las vistas no cambia los nombres.");
            }
            else if (Is(type, SelectiveSafetyDefaults.LateralType))
            {
                sub = IsDynamic ? "En los postes de los extremos, con la guía de canal." : "En los postes que elijas, con la guía de canal.";
                if (selection != null)
                {
                    var rule = RackCadSafetyEdit.LateralFollowsRule(selection, system);
                    if (IsDynamic)
                    {
                        fields.Add(new RackCadSafetyTextField("how", "Cómo se reparte", rule
                            ? "Regla de los extremos: en P1 (salida) y en P" + N(posts) + " (entrada). Se recalcula al añadir o quitar frentes."
                            : "Elegido por poste en la tabla. Ya no se recalcula al añadir o quitar frentes."));
                    }

                    var guarded = frames.Where(r => r.SideGuards > 0).ToList();
                    pieces = guarded.Count == 0
                        ? "Elegido, en ningún poste: no se dibuja."
                        : "En el dibujo: " + Plural(guarded.Sum(r => r.SideGuards), "protector", "protectores") + ", en " + string.Join(", ", guarded.Select(r => r.Post)) + ".";
                    if (IsDynamic)
                    {
                        actions.Add(new RackCadSafetyAction("rule", "Volver a la regla de los extremos", edit && !rule, "Protector lateral: volver a la regla de los extremos",
                            () => WriteOrSay(copy => { RackCadSafetyEdit.LateralToRule(Find(copy, type)); return null; }, null)));
                        loss.Add(rule ? "La regla de los extremos" : guarded.Count > 0 ? "En " + string.Join(", ", guarded.Select(r => r.Post)) : "Ningún poste elegido");
                    }
                    else
                    {
                        var ends = posts > 1
                                   && Enumerable.Range(0, posts).All(p => RackCadSafetyEdit.LateralAt(selection, p, posts, system)
                                                                         == (p == 0 ? SafetySide.Left : p == posts - 1 ? SafetySide.Right : SafetySide.None));
                        actions.Add(new RackCadSafetyAction("ends", "Solo en los extremos", edit && !ends, "Protector lateral: solo en los extremos",
                            () => WriteOrSay(copy => { RackCadSafetyEdit.LateralToEnds(Find(copy, type), _host.SafetyPostCount); return null; }, null)));
                        loss.Add(guarded.Count > 0 ? "En " + string.Join(", ", guarded.Select(r => r.Post)) : "Ningún poste elegido");
                    }
                }

                if (IsDynamic)
                {
                    notes.Add("Cambiar un poste en la tabla lo pasa a «Elegido por poste», con los extremos ya escritos, y lo avisa.");
                    notes.Add("Reemplaza las botas de su poste.");
                }
                else
                {
                    notes.Add("«Izquierda» o «Derecha»: de qué lado queda la guía de canal. «Ambos»: guía a los dos lados, para un frente-puente.");
                    notes.Add("Reemplaza las botas de su poste.");
                    notes.Add("Al elegir la variante se pone en P1 («Izquierda») y en P" + N(posts) + " («Derecha»), como hoy.");
                }
            }
            else if (Is(type, SelectiveSafetyDefaults.DesviadorType))
            {
                sub = "Por poste y nivel. Los niveles, en la tabla.";
                if (selection != null)
                {
                    var side = selection.Side == SafetySide.Left || selection.Side == SafetySide.Right ? selection.Side : SafetySide.Both;
                    fields.Add(new SelectiveChoiceItem("side", "Lado",
                        new[] { SafetySide.Both, SafetySide.Left, SafetySide.Right }.Select(s => new SelectiveChoiceOption(Code(s), RackCadSafetyWords.DeflectorSide(system, s, _host.SafetyFondoCount))).ToList(),
                        Code(side), IsDynamic ? "En qué extremo va." : "En qué cara exterior del rack va.",
                        value => _host.WriteSafety(copy => { DesviadorToEdit(copy).Side = Side(value); return null; }, null), !edit));
                    var plan = SelectiveDesviadorPlan.Build(null, null, selection); // with no system: only the measures it uses, validated
                    fields.Add(EvenField("length", "Longitud", plan.Longitud, "Entero par mayor de 8.", "La longitud debe ser un entero par mayor de 8.", edit,
                        (copy, value) => DesviadorToEdit(copy).DesviadorLongitud = value));
                    fields.Add(EvenField("first", "Primer nivel sobre el primer troquel", plan.FirstLevelHeight,
                        "Entero par mayor de 8. N1 existe aunque no haya larguero a piso.", "El primer nivel debe ser un entero par mayor de 8.", edit,
                        (copy, value) => DesviadorToEdit(copy).DesviadorPrimerNivelAltura = value));

                    // The cells the table shows, as the drawing reads them (Read) and as an edit writes them (Write).
                    var shown = rows.Where(r => r.DeflectorKey.HasValue)
                        .SelectMany(r => Enumerable.Range(0, r.DeflectorLevels).Select(l => (Read: r.DeflectorKey.Value, Write: r.DeflectorWriteKey ?? r.DeflectorKey.Value, Level: l)))
                        .ToList();
                    var cells = shown.Select(c => (Key: c.Write, c.Level)).Distinct().ToList();
                    var off = shown.Count(c => !RackCadSafetyEdit.DesviadorOn(selection, c.Read, c.Level));
                    pieces = "En el dibujo: " + Plural(rows.Sum(r => r.Deflectors), "desviador", "desviadores") + (off > 0 ? "; " + Plural(off, "nivel apagado", "niveles apagados") : string.Empty) + ".";
                    actions.Add(new RackCadSafetyAction("all", "En todos los niveles", edit && off > 0, "Desviadores en todos los niveles",
                        () => WriteOrSay(copy => { RackCadSafetyEdit.SetDesviadorAll(DesviadorToEdit(copy), cells, true); return null; }, null)));
                    actions.Add(new RackCadSafetyAction("none", "En ninguno", edit && off < shown.Count, "Desviadores en ningún nivel",
                        () => WriteOrSay(copy => { RackCadSafetyEdit.SetDesviadorAll(DesviadorToEdit(copy), cells, false); return null; },
                            "Desviadores: en ningún nivel. La variante y sus medidas se quedan; no se dibuja ni se cuenta nada.")));
                    var issue = _host.SafetyDesviadorClearance;
                    if (issue != null)
                    {
                        var at = rows.FirstOrDefault(r => r.DeflectorKey == issue.PostIndex)?.Post ?? "P" + N(issue.PostIndex + 1);
                        var recommended = Math.Floor(issue.Clear / 2.0) * 2.0;
                        warning = "Nota: el claro mínimo entre niveles con desviador es de " + Num(issue.Clear) + " in en " + at + ", menor que la longitud de "
                                  + Num(plan.Longitud) + " in. "
                                  + (recommended > 8.0 ? "Prueba " + Num(recommended) + " in o menos, o quita ese nivel en la tabla." : "Quita ese nivel en la tabla.");
                    }

                    loss.Add("Lado «" + RackCadSafetyWords.DeflectorSide(system, side, _host.SafetyFondoCount) + "» · longitud " + Num(plan.Longitud) + " in · primer nivel a "
                             + Num(plan.FirstLevelHeight) + " in");
                    if (off > 0) loss.Add(Plural(off, "nivel apagado", "niveles apagados"));
                }

                notes.Add(IsDynamic ? "Los niveles siguen la numeración de «Matriz»." : "Los niveles siguen la numeración de «Matriz»: N1 es el piso.");
                notes.Add("Los de arriba van 6 in debajo del larguero.");
                notes.Add("Con «" + RackCadSafetyWords.Side(system, SafetySide.Both) + "», cada nivel cuenta dos piezas.");
            }
            else if (Is(type, SelectiveSafetyDefaults.TopeType) || Is(type, SelectiveSafetyDefaults.ParrillaType) || Is(type, SelectiveSafetyDefaults.GuiaType))
            {
                var (on, total, count) = _host.CellFamily(type);
                if (selection != null)
                {
                    actions.Add(new RackCadSafetyAction("all", "En todas las celdas", edit && on < total, name + " en todas las celdas", () => SetEverywhere(type, true)));
                    actions.Add(new RackCadSafetyAction("none", "En ninguna", edit && on > 0, name + " en ninguna celda", () => SetEverywhere(type, false)));
                    actions.Add(new RackCadSafetyAction("matrix", "Elegir celdas en «Matriz»", true, name + ": elegir celdas en «Matriz»", _host.ShowMatrix));
                    if (total - on > 0) loss.Add(Plural(total - on, "celda apagada", "celdas apagadas"));
                }

                if (Is(type, SelectiveSafetyDefaults.TopeType))
                {
                    sub = "Por frente y nivel de larguero, en todo el rack.";
                    if (selection != null) BuildTope(selection, fields, edit, loss);
                    notes.Add("Las celdas se eligen en «Matriz» (T). N1 en el piso, sin larguero, no lleva tope.");
                    notes.Add("«En ninguna» deja la variante y sus medidas, pero no se dibuja ni se cuenta.");
                    pieces = selection == null ? string.Empty : "En el dibujo: " + Plural(count, "tope", "topes") + ", en " + N(on) + " de " + Plural(total, "celda", "celdas") + " con larguero.";
                }
                else if (Is(type, SelectiveSafetyDefaults.ParrillaType))
                {
                    sub = "Una por tarima, por frente y nivel de larguero" + (_host.SafetyFondoCount > 1 ? ", en todos los fondos." : ".");
                    if (selection != null) warning = BuildParrilla(selection, fields, edit, loss);
                    notes.Add("Las celdas se eligen en «Matriz» (P). N1 en el piso, sin larguero, no lleva parrilla.");
                    notes.Add("Una cantidad que no cabe se rechaza aquí, en su campo.");
                    pieces = selection == null ? string.Empty : "En el dibujo: " + Plural(count, "parrilla", "parrillas") + ", en " + N(on) + " de " + Plural(total, "celda", "celdas") + " con larguero.";
                }
                else
                {
                    sub = "Por frente y nivel, solo en la entrada.";
                    notes.Add("Cada celda con G coloca una pareja espejeada, 8 in arriba del larguero IN/OUT de entrada.");
                    notes.Add("La longitud sale del tramo de cabecera.");
                    notes.Add("Un frente en blanco conserva sus celdas, dormidas.");
                    pieces = selection == null ? string.Empty : "En el dibujo: " + Plural(count, "guía", "guías") + ", una pareja en cada una de " + Plural(on, "celda", "celdas") + ".";
                }
            }
            else if (Is(type, SelectiveSafetyDefaults.DefensaType))
            {
                sub = "Por poste, en salida y en entrada.";
                if (selection != null)
                {
                    var own = RackCadSafetyEdit.DefenseOwnEnds(selection);
                    var drawn = rows.Count(r => r.DefenseExit) + rows.Count(r => r.DefenseEntrance);
                    pieces = "En el dibujo: " + Plural(drawn, "defensa", "defensas")
                             + (own > 0 ? "; " + Plural(own, "extremo con medida propia o sin defensa", "extremos con medida propia o sin defensa") : string.Empty) + ".";
                    actions.Add(new RackCadSafetyAction("auto", "Todas automáticas", edit && own > 0, "Defensas: todas automáticas",
                        () => WriteOrSay(copy => { RackCadSafetyEdit.DefenseAllAutomatic(Find(copy, type)); return null; }, null)));
                    if (own > 0) loss.Add(Plural(own, "extremo con medida propia o sin defensa", "extremos con medida propia o sin defensa"));
                }

                notes.Add("Medida vacía = automática: " + Num(RackCad.Application.Systems.Dynamic.DynamicForkliftDefensePlan.EdgeLength) + " in en los postes de orilla y "
                          + Num(RackCad.Application.Systems.Dynamic.DynamicForkliftDefensePlan.IntermediateLength) + " in en los intermedios. Se recalcula al añadir o quitar frentes.");
                notes.Add("Una medida escrita es propia y se queda, aunque coincida con la automática.");
                notes.Add("Desmarca la casilla para quitar la defensa de ese extremo.");
            }

            var asking = selection != null && string.Equals(_asking, type, StringComparison.Ordinal);
            Inspector.Set(type, name, Letter(type), sub, variant, selection != null, asking, "¿Quitar " + name.ToLowerInvariant() + "?",
                "Quitar " + name.ToLowerInvariant(), loss, fields, warning, notes, pieces, actions);
        }

        private void BuildTope(SelectiveSafetySelection selection, List<object> fields, bool edit, List<string> loss)
        {
            var type = SelectiveSafetyDefaults.TopeType;
            var fondos = _host.SafetyFondoCount;
            var shared = fondos > 1 && selection.TopeShared;
            fields.Add(new DynamicFieldItem("saque", "Saque", "in", Num(SelectiveTopePlacement.Saque(selection)), "Cuánto sobresale el bloque. Mayor que cero.", text =>
            {
                if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "El saque debe ser un número mayor que cero.";
                return _host.WriteSafety(copy => { Find(copy, type).TopeSaque = value; return null; }, null);
            }, !edit));
            if (fondos > 1)
            {
                fields.Add(new SelectiveChoiceItem("shared", "Cuántos",
                    new[] { new SelectiveChoiceOption("c", "Uno central, compartido entre fondos"), new SelectiveChoiceOption("f", "Uno por fondo, según el lado") },
                    selection.TopeShared ? "c" : "f", string.Empty,
                    value => _host.WriteSafety(copy => { Find(copy, type).TopeShared = value == "c"; return null; }, null), !edit));
                fields.Add(new SelectiveChoiceItem("fondo", "Fondo",
                    new[] { new SelectiveChoiceOption("-1", "Central (automático)") }.Concat(Enumerable.Range(0, fondos).Select(k => new SelectiveChoiceOption(N(k), "Fondo " + N(k + 1)))).ToList(),
                    selection.TopeFondo >= 0 && selection.TopeFondo < fondos ? N(selection.TopeFondo) : "-1", string.Empty,
                    value => _host.WriteSafety(copy => { Find(copy, type).TopeFondo = int.Parse(value, CultureInfo.InvariantCulture); return null; }, null), !edit));
            }

            // SG-07: «Ninguno (no se dibuja)» only when the rack read has it; with one central tope shared between fondos the side does not count.
            var sides = new List<SelectiveChoiceOption> { Option(SafetySide.Both), Option(SafetySide.Left), Option(SafetySide.Right) };
            if (selection.Side == SafetySide.None) sides.Add(new SelectiveChoiceOption(Code(SafetySide.None), "Ninguno (no se dibuja)"));
            fields.Add(new SelectiveChoiceItem("side", "Lado", sides, Code(selection.Side),
                shared ? "Con uno central, compartido, el lado no cuenta."
                    : fondos > 1 ? "Izquierda = el extremo bajo del hueco entre fondos; Derecha = el alto."
                    : "Izquierda = la cara cercana; Derecha = la lejana.",
                value => _host.WriteSafety(copy => { Find(copy, type).Side = Side(value); return null; }, null), !edit || shared));
            fields.Add(new RackCadSafetyCheckField("frontal", "Dibujar también en la frontal", selection.TopeFrontal, edit, "En lateral y en planta se dibuja siempre.",
                on => WriteOrSay(copy => { Find(copy, type).TopeFrontal = on; return null; }, null)));
            loss.Insert(1, TopeConfig(selection, fondos));
        }

        private SelectiveChoiceOption Option(SafetySide side) => new SelectiveChoiceOption(Code(side), RackCadSafetyWords.Side(_host.SafetySystem, side));

        /// <summary>The configuration of the tope in one line (the card and the question of «Sin variante»).</summary>
        internal static string TopeConfig(SelectiveSafetySelection selection, int fondos)
        {
            var side = RackCadSafetyWords.Side(RackCadSafetySystem.Selective, selection.Side);
            string where;
            if (fondos < 2) where = selection.Side == SafetySide.None ? "no se dibuja" : side;
            else
            {
                where = selection.TopeShared ? "uno central, compartido entre fondos" : "uno por fondo, " + side;
                if (selection.TopeFondo >= 0 && selection.TopeFondo < fondos) where = "en el fondo " + N(selection.TopeFondo + 1) + ", " + where;
            }

            return "Saque " + Num(SelectiveTopePlacement.Saque(selection)) + " in · " + where + " · en la frontal: " + (selection.TopeFrontal ? "sí" : "no") + ".";
        }

        // The parrilla: the two views, the width and the count; a count that does not fit, or a width with which nothing fits, is refused in its
        // field (no MessageBox: the prototype). Returns the warning of the cells where none fits.
        private string BuildParrilla(SelectiveSafetySelection selection, List<object> fields, bool edit, List<string> loss)
        {
            var type = SelectiveSafetyDefaults.ParrillaType;
            fields.Add(new RackCadSafetyCheckField("frontal", "Dibujar en la frontal", selection.ParrillaFrontal, edit, string.Empty,
                on => WriteOrSay(copy => { Find(copy, type).ParrillaFrontal = on; return null; }, null)));
            fields.Add(new RackCadSafetyCheckField("lateral", "Dibujar en la lateral", selection.ParrillaLateral, edit, "En planta no se dibuja. Entran en el BOM aunque no se dibujen.",
                on => WriteOrSay(copy => { Find(copy, type).ParrillaLateral = on; return null; }, null)));
            var plan = _host.SafetyParrillaPlan;
            var off = SelectiveSafetyGrid.OffCellKeys(selection.ParrillaOffCells);
            var onCells = plan.Where(c => !off.Contains((c.Frente, c.Level))).ToList();
            var fits = onCells.Count == 0 ? 0 : onCells.Select(c => SelectiveParrillaPlan.MaxCountIn(c, selection.ParrillaFrente)).Where(m => m > 0).DefaultIfEmpty(0).Min();
            fields.Add(new DynamicFieldItem("width", "Ancho de cada parrilla", "in", selection.ParrillaFrente > 0.0 ? Num(selection.ParrillaFrente) : string.Empty,
                "Vacío = el ancho de la tarima.", text =>
                {
                    var width = 0.0;
                    if (!string.IsNullOrWhiteSpace(text) && (!UiSupport.TryNum(text, out width) || width <= 0.0)) return "Vacío (el ancho de la tarima) o un número mayor que cero.";
                    if (onCells.Count > 0 && onCells.All(c => SelectiveParrillaPlan.CountIn(c, width, selection.ParrillaCantidad) <= 0))
                        return "No cabe ninguna parrilla de " + Num(width) + " in en las celdas con parrilla. Bájalo o déjalo vacío.";
                    var crowded = TooMany(onCells, width, selection.ParrillaCantidad);
                    if (crowded != null) return crowded;
                    return _host.WriteSafety(copy => { Find(copy, type).ParrillaFrente = width; return null; }, null);
                }, !edit));
            fields.Add(new DynamicFieldItem("count", "Cantidad por nivel", string.Empty, selection.ParrillaCantidad > 0 ? N(selection.ParrillaCantidad) : string.Empty,
                "Vacía = las que quepan" + (fits > 0 ? " (" + N(fits) + ")" : string.Empty) + ". En un medio frente, por tramo.", text =>
                {
                    var count = 0;
                    if (!string.IsNullOrWhiteSpace(text) && (!UiSupport.TryInt(text, out count) || count <= 0)) return "Vacía (las que quepan) o un entero mayor que cero.";
                    var crowded = TooMany(onCells, selection.ParrillaFrente, count);
                    if (crowded != null) return crowded;
                    return _host.WriteSafety(copy => { Find(copy, type).ParrillaCantidad = count; return null; }, null);
                }, !edit));
            loss.Insert(1, ParrillaConfig(selection));

            var empty = onCells.Where(c => SelectiveParrillaPlan.CountIn(c, selection.ParrillaFrente, selection.ParrillaCantidad) <= 0).Select(CellName).ToList();
            return empty.Count == 0 ? string.Empty : "No cabe ninguna parrilla en " + Join(empty) + ": ahí no se dibuja ninguna.";
        }

        private static string TooMany(IEnumerable<SelectiveParrillaPlan.Cell> cells, double width, int count)
        {
            if (count <= 0) return null;
            var offenders = cells.Select(c => (Cell: c, Max: SelectiveParrillaPlan.MaxCountIn(c, width))).Where(x => x.Max > 0 && count > x.Max).ToList();
            if (offenders.Count == 0) return null;
            return "No caben " + N(count) + " por nivel en " + Join(offenders.Select(o => CellName(o.Cell) + " (caben " + N(o.Max) + ")").ToList())
                   + ". Baja la cantidad o el ancho, o quita esas celdas.";
        }

        // The cells of the parrilla by their larguero (its grid), not by the level of the matrix: the same larguero is N1 in a front with
        // «Larguero a piso» and N2 in one without it.
        private static string CellName(SelectiveParrillaPlan.Cell cell) => "F" + N(cell.Frente + 1) + " · larguero " + N(cell.Level + 1);

        private static string Join(IReadOnlyList<string> names)
            => string.Join(", ", names.Take(4)) + (names.Count > 4 ? " y " + N(names.Count - 4) + " más" : string.Empty);

        /// <summary>The configuration of the parrilla in one line (as the card).</summary>
        internal static string ParrillaConfig(SelectiveSafetySelection selection)
        {
            var views = new List<string>();
            if (selection.ParrillaFrontal) views.Add("la frontal");
            if (selection.ParrillaLateral) views.Add("la lateral");
            var width = selection.ParrillaFrente > 0.0 ? "de " + Num(selection.ParrillaFrente) + " in de ancho" : "una por tarima, con su ancho";
            var count = selection.ParrillaCantidad > 0 ? N(selection.ParrillaCantidad) + " por nivel" : "cantidad automática";
            return "Se dibujan en " + (views.Count == 0 ? "ninguna vista" : string.Join(" y ", views)) + " · " + width + " · " + count + ".";
        }

        private DynamicFieldItem EvenField(string key, string label, double value, string hint, string error, bool edit, Action<List<SelectiveSafetySelection>, double> write)
            => new DynamicFieldItem(key, label, "in", Num(value), hint, text =>
            {
                if (!UiSupport.TryNum(text, out var number) || !SelectiveDesviadorPlan.IsValidEvenAbove8(number)) return error;
                return _host.WriteSafety(copy => { write(copy, number); return null; }, null);
            }, !edit);

        // ------------------------------------------------------------------ the variant (SG-01, SG-06)

        private string SetVariant(string type, string value)
        {
            var catalog = _host.SafetyCatalog;
            var current = RackCadSafetyEdit.Selected(_host.CopySafetyForPanel(), catalog, type);
            if (value == NoVariant)
            {
                if (current == null) return null;
                _asking = type; // SG-06: ask first, inline, what is lost
                RefreshInspector();
                return null;
            }

            var label = RackCadSafetyEdit.Variants(catalog, type).FirstOrDefault(v => string.Equals(v.Id, value, StringComparison.OrdinalIgnoreCase))?.Label ?? value;
            _asking = null;
            return _host.WriteSafety(copy =>
            {
                RackCadSafetyEdit.SetVariant(copy, catalog, type, value, _host.SafetySystem, _host.SafetyPostCount);
                return null;
            }, current == null ? FamilyName(type) + ": «" + label + "», con sus valores por defecto. Revísalos aquí y en la tabla." : null);
        }

        private void ConfirmRemove()
        {
            var type = _asking;
            if (type == null) return;
            _asking = null;
            var error = _host.WriteSafety(copy =>
            {
                RackCadSafetyEdit.Remove(copy, _host.SafetyCatalog, type);
                return null;
            }, "Se quitó «" + FamilyName(type) + "» del borrador, con sus medidas. «Descartar» lo recupera.");
            if (error != null) Say(error, error: true);
            RefreshInspector();
        }

        private void CancelRemove()
        {
            _asking = null;
            RefreshInspector();
        }

        private void SetEverywhere(string type, bool on)
        {
            var error = _host.SetCellFamilyEverywhere(type, on);
            if (error == null && !on) Say(FamilyName(type) + ": en ninguna celda. La variante y sus medidas se quedan; no se dibuja ni se cuenta nada.");
        }

        // ------------------------------------------------------------------ helpers

        // A write that is not a field: a refusal is said in the line of messages (a field shows its own).
        private void WriteOrSay(Func<List<SelectiveSafetySelection>, string> edit, string notice)
        {
            var error = _host.WriteSafety(edit, notice);
            if (error != null) Say(error, error: true);
        }

        /// <summary>What the panel says in the line of messages of the editor (set by the editor).</summary>
        internal Action<string, bool> SayAction;

        private void Say(string text, bool error = false) => SayAction?.Invoke(text, error);

        // The desviador in the copy an edit of it works on. In a Dinámico still read the historic way it first passes to the reading by post,
        // keeping what it draws (decision of the Owner of 2026-11-26): from this edit on, every post governs its own desviador.
        private SelectiveSafetySelection DesviadorToEdit(List<SelectiveSafetySelection> copy)
        {
            var desviador = Find(copy, SelectiveSafetyDefaults.DesviadorType);
            if (IsDynamic) RackCadSafetyEdit.DesviadorToPosts(desviador, _host.SafetyPostCount);
            return desviador;
        }

        // The family in the copy the edit works on (the write runs only when the inspector or the table showed it chosen).
        private SelectiveSafetySelection Find(List<SelectiveSafetySelection> copy, string type)
            => RackCadSafetyEdit.Selected(copy, _host.SafetyCatalog, type) ?? throw new InvalidOperationException("No hay variante elegida de " + FamilyName(type).ToLowerInvariant() + ".");

        internal static string FamilyName(string type)
        {
            if (Is(type, SelectiveSafetyDefaults.BotaType)) return "Botas";
            if (Is(type, SelectiveSafetyDefaults.LateralType)) return "Protector lateral";
            if (Is(type, SelectiveSafetyDefaults.DesviadorType)) return "Desviadores";
            if (Is(type, SelectiveSafetyDefaults.TopeType)) return "Topes";
            if (Is(type, SelectiveSafetyDefaults.ParrillaType)) return "Parrillas";
            if (Is(type, SelectiveSafetyDefaults.GuiaType)) return "Guía de entrada";
            if (Is(type, SelectiveSafetyDefaults.DefensaType)) return "Defensas de montacargas";
            return type ?? string.Empty;
        }

        private static string Letter(string type)
            => Is(type, SelectiveSafetyDefaults.TopeType) ? "T" : Is(type, SelectiveSafetyDefaults.ParrillaType) ? "P" : Is(type, SelectiveSafetyDefaults.GuiaType) ? "G" : string.Empty;

        internal static string Code(SafetySide side)
            => side switch { SafetySide.Left => "L", SafetySide.Right => "R", SafetySide.Both => "B", _ => "N" };

        internal static SafetySide Side(string code)
            => code switch { "L" => SafetySide.Left, "R" => SafetySide.Right, "B" => SafetySide.Both, _ => SafetySide.None };

        private static bool Is(string actual, string expected) => SelectiveSafetyDefaults.IsType(actual, expected);

        private static string Dash(bool chosen, int count) => chosen ? N(count) : "—";

        private static string Plural(int count, string one, string many) => N(count) + " " + (count == 1 ? one : many);

        private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// The words of the sides, by system (SG-02, note of the Owner: «Se tiene que mantener «Izquierda / Derecha», «Entrada/Salida / Posterior» y
    /// «salida / entrada» según el sistema»). The Selectivo names the faces of its depth (decision 13: «Izquierda» the near one, the front of
    /// fondo 1; «Derecha» the far one); the Dinámico names the ends of its flow (the introduction of its classic dialog: «Izquierda es la salida
    /// y derecha la entrada»; its defense: «Salida / Entrada»). Push Back keeps «Entrada/Salida / Posterior» in its own editor (SG-09). The
    /// ordinals are the same everywhere; only the words change.
    /// </summary>
    internal static class RackCadSafetyWords
    {
        public static string Side(RackCadSafetySystem system, SafetySide side)
            => system == RackCadSafetySystem.PushBack ? Placement(BootPlacements.From(side)) // T5 pb: «Entrada/Salida / Posterior»
            : system == RackCadSafetySystem.Dynamic
                ? side switch { SafetySide.Left => "Salida", SafetySide.Right => "Entrada", SafetySide.Both => "Salida y entrada", _ => "Ninguna" }
                : side switch { SafetySide.Left => SelectiveSafetyView.Left, SafetySide.Right => SelectiveSafetyView.Right, SafetySide.Both => SelectiveSafetyView.BothSides, _ => "Ninguna" };

        public static string SideGuard(RackCadSafetySystem system, SafetySide side)
            => side == SafetySide.None ? "No lleva"
                : system != RackCadSafetySystem.Dynamic && side == SafetySide.Both ? "Ambos (frente-puente)" // T5 pb: also Push Back
                : system == RackCadSafetySystem.PushBack ? Side(RackCadSafetySystem.Selective, side) // T5 pb: its guide, «Izquierda / Derecha»
                : Side(system, side);

        /// <summary>T5 pb: the placements of a bota of Push Back, as its classic section names them (I-42 S1B).</summary>
        public static string Placement(BootPlacement placement)
            => placement switch { BootPlacement.EntryExit => "Entrada/Salida", BootPlacement.Rear => "Posterior", BootPlacement.Both => "Ambas", _ => "Ninguno" };

        public static string DeflectorSide(RackCadSafetySystem system, SafetySide side, int fondos)
        {
            if (system == RackCadSafetySystem.Dynamic || fondos < 2 || side == SafetySide.Both) return Side(system, side);
            return side == SafetySide.Left ? "Izquierda (cara del fondo 1)" : "Derecha (cara del fondo " + fondos.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
