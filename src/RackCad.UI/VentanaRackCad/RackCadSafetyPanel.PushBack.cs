using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// What the editable safety needs from the editor of Push Back on top of <see cref="IRackCadSafetyPanelHost"/> (T5 pb, PB-S03): the copy
    /// with the type of defense of each side and the resolved bota, ONE way to write it, the levels of the desviador grid and the resolved
    /// safety of the last valid recomposition (what the drawing reads).
    /// </summary>
    internal interface IPushBackSafetyPanelHost : IRackCadSafetyPanelHost
    {
        bool SafetyTwoSided { get; }

        PushBackSafetyCopy CopyPushBackSafety();

        /// <summary>As <see cref="IRackCadSafetyPanelHost.WriteSafety"/>, on the copy of Push Back (the list and the type of defense of each side).</summary>
        string WritePushBackSafety(Func<PushBackSafetyCopy, string> edit, string notice);

        /// <summary>The levels of the desviador grid at each post (PB-002).</summary>
        IReadOnlyList<int> SafetyDesviadorLevels { get; }

        /// <summary>The safety the last valid recomposition resolved (authorized: low end, the faces of each side); empty before it.</summary>
        IReadOnlyList<SelectiveSafetySelection> ResolvedSafety();
    }

    /// <summary>
    /// T5 pb (I-64, quinta tanda, part «pb», PB-S03; SG-09: Push Back adopts the design of the fourth batch without touching its authority):
    /// the table «Seguridad por poste» and the inspector «Variante y medidas» of a Push Back. Its words are «Entrada/Salida / Posterior» (SG-02,
    /// question 8 of the prototype: by side); the protector lateral keeps «Izquierda / Derecha» of its guide, as the classic dialog. The botas
    /// and the defense are by side (I-42): two columns of botas and one end of defense per side in a rack of two sides; one column of botas and
    /// the two ends («Entrada/Salida» and «Posterior») in a rack of one way. Every write is the one of the classic dialog for the same choice
    /// (<see cref="PushBackSafetyEdit"/>).
    /// </summary>
    public sealed partial class RackCadSafetyPanelViewModel
    {
        private IPushBackSafetyPanelHost PushBackHost => _host as IPushBackSafetyPanelHost;

        public bool IsPushBack => _host.SafetySystem == RackCadSafetySystem.PushBack;

        private bool TwoSided => IsPushBack && PushBackHost?.SafetyTwoSided == true;

        /// <summary>The column of the botas of side B (a Push Back of two sides).</summary>
        public bool ShowsBootsB => TwoSided;

        /// <summary>The two columns of the defense (Dinámico, Push Back).</summary>
        public bool ShowsDefense => IsDynamic || IsPushBack;

        public bool ShowsGuides => IsDynamic;

        public string BootsHeader => TwoSided ? "Botas · lado A" : "Botas";

        public string DefenseExitHeader => !IsPushBack ? "Defensa salida (in)" : TwoSided ? "Defensa lado A (in)" : "Defensa Entrada/Salida (in)";

        public string DefenseEntranceHeader => !IsPushBack ? "Defensa entrada (in)" : TwoSided ? "Defensa lado B (in)" : "Defensa Posterior (in)";

        public string DefenseExitHeadName => !IsPushBack ? "Defensas en salida: ver su variante y sus medidas"
            : TwoSided ? "Defensas del lado A: ver su tipo y sus medidas" : "Defensas en Entrada/Salida: ver su tipo y sus medidas";

        public string DefenseEntranceHeadName => !IsPushBack ? "Defensas en entrada: ver su variante y sus medidas"
            : TwoSided ? "Defensas del lado B: ver su tipo y sus medidas" : "Defensas en Posterior: ver su tipo y sus medidas";

        private string PushBackPostFoot => "Contado como el BOM de Push Back: un protector lateral reemplaza las botas de su poste; cada nivel con desviador, una pieza por pasillo; "
                                           + (TwoSided ? "cada lado tiene sus botas y su defensa, en su pasillo." : "«Posterior» es el extremo alto, donde va el tope.");

        // ------------------------------------------------------------------ the table

        private void RefreshPushBackTable()
        {
            var host = PushBackHost;
            var copy = host.CopyPushBackSafety();
            var catalog = _host.SafetyCatalog;
            var lateral = RackCadSafetyEdit.Selected(copy.Safety, catalog, SelectiveSafetyDefaults.LateralType);
            var desviador = RackCadSafetyEdit.Selected(copy.Safety, catalog, SelectiveSafetyDefaults.DesviadorType);
            var resolvedDefense = RackCadSafetyEdit.Selected(host.ResolvedSafety(), catalog, SelectiveSafetyDefaults.DefensaType);
            var hasBoots = PushBackSafetyEdit.Variants(catalog, SelectiveSafetyDefaults.BotaType).Count > 0;
            var hasDefense = PushBackSafetyEdit.Variants(catalog, SelectiveSafetyDefaults.DefensaType).Count > 0;
            var rows = _host.SafetyRows();
            var edit = _host.CanEditSafetyPanel;
            var sideA = hasBoots ? PushBackSafetyEdit.BootSide(copy, catalog, PushBackSide.A) : null;
            var sideB = hasBoots && copy.TwoSided ? PushBackSafetyEdit.BootSide(copy, catalog, PushBackSide.B) : null;
            var defenseA = hasDefense ? PushBackSafetyEdit.DefenseSide(copy, catalog, PushBackSide.A) : null;
            var defenseB = hasDefense && copy.TwoSided ? PushBackSafetyEdit.DefenseSide(copy, catalog, PushBackSide.B) : null;

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
                SetPushBackBoots(item.Boots, row, sideA, row.Boots, edit);
                SetPushBackBoots(item.BootsB, row, sideB, row.BootsB, edit);
                SetPushBackSideGuard(item, row, lateral, copy.PostCount, edit);
                SetPushBackDeflectors(item, row, desviador, host.SafetyDesviadorLevels, edit);
                item.Separators = "—";
                SetPushBackDefense(item.DefenseExit, row, copy, defenseA, resolvedDefense, farEnd: false, row.DefenseExit, edit);
                SetPushBackDefense(item.DefenseEntrance, row, copy, copy.TwoSided ? defenseB : defenseA, resolvedDefense, farEnd: true, row.DefenseEntrance, edit);
                item.Guides = "—";
            }

            var total = _table[_table.Count - 1];
            total.Kind = string.Empty;
            total.Boots.Set(null, Dash(sideA != null, rows.Sum(r => r.Boots)), string.Empty, false);
            total.BootsB.Set(null, Dash(sideB != null, rows.Sum(r => r.BootsB)), string.Empty, false);
            total.SideGuard.Set(null, Dash(lateral != null, rows.Sum(r => r.SideGuards)), string.Empty, false);
            total.Deflectors.Set(0, _ => default, Dash(desviador != null, rows.Sum(r => r.Deflectors)), string.Empty, string.Empty);
            total.Separators = "—";
            total.DefenseExit.Set(false, false, false, string.Empty, null, Dash(defenseA != null, rows.Count(r => r.DefenseExit)), string.Empty, false, null);
            total.DefenseEntrance.Set(false, false, false, string.Empty, null, Dash(defenseA != null, rows.Count(r => r.DefenseEntrance)), string.Empty, false, null);
            total.Guides = "—";

            OnPropertyChanged(nameof(ShowsBootsB));
            OnPropertyChanged(nameof(BootsHeader));
            OnPropertyChanged(nameof(DefenseExitHeader));
            OnPropertyChanged(nameof(DefenseEntranceHeader));
            OnPropertyChanged(nameof(DefenseExitHeadName));
            OnPropertyChanged(nameof(DefenseEntranceHeadName));
            OnPropertyChanged(nameof(PostFoot));
        }

        // The bota of one post of one side: «General (…)», or a placement of its own, as the per-post grid of the classic section.
        private void SetPushBackBoots(RackCadSafetyChoiceCell cell, RackCadSafetyRow row, PushBackBootSideDraft side, int pieces, bool edit)
        {
            if (side == null || !row.FramePost.HasValue)
            {
                cell.Set(null, "—", string.Empty, false);
                return;
            }

            if (row.SideGuards > 0)
            {
                cell.Set(null, "La reemplaza el protector lateral", string.Empty, false);
                return;
            }

            var post = row.FramePost.Value;
            var own = side.OwnAt(post);
            var letter = Letter(side.Side);
            var general = RackCadSafetyWords.Placement(side.Placement) + (side.Explicit ? string.Empty : ", automática");
            var options = new[] { new SelectiveChoiceOption(General, "General (" + general + ")") }
                .Concat(PushBackSafetyEdit.Placements.Select(p => new SelectiveChoiceOption(PlacementCode(p), RackCadSafetyWords.Placement(p))))
                .ToList();
            var label = "Botas" + (TwoSided ? " del lado " + letter : string.Empty) + " en " + row.Post;
            var choice = new SelectiveChoiceItem("boot" + letter + N(post), label, options, own.HasValue ? PlacementCode(own.Value) : General, string.Empty,
                value => WritePushBack(copy =>
                {
                    PushBackSafetyEdit.SetBootPost(copy, _host.SafetyCatalog, side.Side, post, value == General ? (BootPlacement?)null : PlacementOf(value));
                    return null;
                }, null), !edit);
            var sub = side.IsNone
                ? "Sin tipo en este lado: en espera"
                : (own.HasValue ? "Propia · " : string.Empty) + Plural(pieces, "pieza", "piezas");
            cell.Set(choice, string.Empty, sub, own.HasValue);
        }

        // The protector lateral of a post, as the per-post window of the classic dialog (its guide «Izquierda / Derecha»).
        private void SetPushBackSideGuard(RackCadSafetyRowItem item, RackCadSafetyRow row, SelectiveSafetySelection lateral, int posts, bool edit)
        {
            if (lateral == null || !row.FramePost.HasValue)
            {
                item.SideGuard.Set(null, "—", string.Empty, false);
                return;
            }

            var post = row.FramePost.Value;
            var rule = PushBackSafetyEdit.LateralFollowsRule(lateral);
            var side = DynamicLateralGuardPlan.SideAt(lateral, post, posts);
            var options = new[] { SafetySide.None, SafetySide.Left, SafetySide.Right, SafetySide.Both }
                .Select(s => new SelectiveChoiceOption(Code(s), RackCadSafetyWords.SideGuard(RackCadSafetySystem.PushBack, s))).ToList();
            var choice = new SelectiveChoiceItem("lateral" + N(post), "Protector lateral en " + row.Post, options, Code(side), string.Empty, value =>
            {
                var left = false;
                var error = WritePushBack(copy =>
                {
                    left = PushBackSafetyEdit.SetLateralPost(copy.Safety, _host.SafetyCatalog, post, Side(value), copy.PostCount);
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

        // The levels of the desviador of a post (its grid is by post, PB-002).
        private void SetPushBackDeflectors(RackCadSafetyRowItem item, RackCadSafetyRow row, SelectiveSafetySelection desviador, IReadOnlyList<int> levels, bool edit)
        {
            if (desviador == null || !row.DeflectorKey.HasValue || row.DeflectorLevels <= 0)
            {
                item.Deflectors.Set(0, _ => default, "—", string.Empty, string.Empty);
                return;
            }

            var key = row.DeflectorKey.Value;
            item.Deflectors.Set(row.DeflectorLevels, level =>
            {
                var on = RackCadSafetyEdit.DesviadorOn(desviador, key, level);
                var name = row.Post + " · N" + N(level + 1) + (on ? ": con desviador. Pulsa para quitarlo." : ": sin desviador. Pulsa para ponerlo.");
                return (on, edit, name, () => WritePushBackOrSay(copy =>
                {
                    PushBackSafetyEdit.SetDesviador(copy.Safety, _host.SafetyCatalog, levels, key, level, !on);
                    return null;
                }, null));
            }, "—", Plural(row.Deflectors, "pieza", "piezas"), "Niveles con desviador en " + row.Post);
        }

        // One end of the defense of a post: in a rack of one way «Entrada/Salida» (the near end) and «Posterior» (the far one), both of the
        // section of the rack; in a composite, the face of each side (A the near one, B the far one).
        private void SetPushBackDefense(RackCadSafetyDefenseCell cell, RackCadSafetyRow row, PushBackSafetyCopy copy, PushBackDefenseSideDraft section,
            SelectiveSafetySelection resolved, bool farEnd, bool drawn, bool edit)
        {
            if (section == null || !row.FramePost.HasValue)
            {
                cell.Set(false, false, false, string.Empty, null, "—", string.Empty, false, null);
                return;
            }

            var post = row.FramePost.Value;
            if (copy.TwoSided && !copy.HasFace(section.Side, post))
            {
                cell.Set(false, false, false, string.Empty, null, "—", "Sin cara de ataque en este lado", false, null);
                return;
            }

            var record = section.Posts.FirstOrDefault(r => r.PostIndex == post);
            var auto = record == null || (farEnd ? record.EntranceAuto : record.ExitAuto);
            var stored = record == null ? 0.0 : farEnd ? record.EntranceLength : record.ExitLength;
            var setting = DynamicForkliftDefensePlan.ForSelection(resolved, post, copy.PostCount);
            var resolvedLength = resolved == null ? (auto ? Automatic(copy, post, farEnd) : stored) : farEnd ? setting.EntranceLength : setting.ExitLength;
            var on = resolvedLength > 0.0;
            var own = !auto && on;
            var automatic = Automatic(copy, post, farEnd);
            var endName = EndName(copy, section.Side, farEnd);
            var length = new DynamicFieldItem(
                "def" + (farEnd ? "E" : "S") + N(post),
                "Longitud de la defensa en " + endName + " en " + row.Post + ", en pulgadas; vacía = automática, "
                + (automatic > 0.0 ? Num(automatic) + " in" : "sin defensa"),
                "in",
                own ? Num(stored) : string.Empty,
                string.Empty,
                text =>
                {
                    double? value = null;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (!UiSupport.TryNum(text, out var number) || number <= 0.0) return "Debe ser un número mayor que cero, o vacía (automática).";
                        value = number;
                    }

                    return WritePushBack(c =>
                    {
                        PushBackSafetyEdit.SetDefenseLength(c, _host.SafetyCatalog, section.Side, post, farEnd, value);
                        return null;
                    }, null);
                },
                !edit || !on);
            var sub = section.IsNone ? "Sin tipo en este lado: en espera"
                : !on ? (auto ? "Automática · sin defensa" : "Sin defensa")
                : own ? "Propia"
                : "Automática · " + Num(automatic) + " in";
            if (on && !drawn && !section.IsNone) sub += " · no se dibuja aquí";
            cell.Set(true, on, edit, "Defensa en " + endName + " en " + row.Post, length, string.Empty, sub, own,
                value => WritePushBackOrSay(c =>
                {
                    PushBackSafetyEdit.SetDefenseOn(c, _host.SafetyCatalog, section.Side, post, farEnd, value);
                    return null;
                }, null));
        }

        // The automatic length of one end: 12 in at an edge post and 36 in at an intermediate one; the far end of a rack of one way has none
        // (PB-009), the far face of a composite is the aisle of side B.
        private static double Automatic(PushBackSafetyCopy copy, int post, bool farEnd)
        {
            var setting = DynamicForkliftDefensePlan.At(null, post, copy.PostCount, lowEndOnly: true, secondLoadFace: copy.TwoSided);
            return farEnd ? setting.EntranceLength : setting.ExitLength;
        }

        private static string EndName(PushBackSafetyCopy copy, PushBackSide side, bool farEnd)
            => copy.TwoSided ? "Entrada/Salida del lado " + Letter(side) : farEnd ? "Posterior" : "Entrada/Salida";

        // ------------------------------------------------------------------ the inspector

        private void RefreshPushBackInspector()
        {
            var host = PushBackHost;
            var catalog = _host.SafetyCatalog;
            var copy = host.CopyPushBackSafety();
            var type = _family;
            var edit = _host.CanEditSafetyPanel;
            var rows = _host.SafetyRows();
            var frames = rows.Where(r => r.FramePost.HasValue).ToList();
            var name = FamilyName(type);
            var fields = new List<object>();
            var notes = new List<string>();
            var actions = new List<RackCadSafetyAction>();
            var loss = new List<string>();
            var sub = string.Empty;
            var pieces = string.Empty;
            var hasVariant = false;
            SelectiveChoiceItem variant;

            if (Is(type, SelectiveSafetyDefaults.BotaType) || Is(type, SelectiveSafetyDefaults.DefensaType))
            {
                // The type is of each side, with «Ninguno»: the intent of the side stays (I-42 S1G/7E), so nothing is asked before.
                _asking = null;
                var boots = Is(type, SelectiveSafetyDefaults.BotaType);
                var variants = PushBackSafetyEdit.Variants(catalog, type);
                hasVariant = variants.Count > 0;
                var typeOptions = new[] { new SelectiveChoiceOption(PushBackDefaults.NonePieceId, "Ninguno (este lado no lleva)") }
                    .Concat(variants.Select(v => new SelectiveChoiceOption(v.Id.Trim(), v.Label))).ToList();
                string PieceOf(PushBackSide side) => boots
                    ? PushBackSafetyEdit.BootSide(copy, catalog, side).PieceId
                    : PushBackSafetyEdit.DefenseSide(copy, catalog, side).PieceId;
                SelectiveChoiceItem TypeItem(PushBackSide side, string key)
                    => new SelectiveChoiceItem(key, (boots ? "Tipo de protector" : "Tipo de defensa") + (copy.TwoSided ? " · lado " + Letter(side) : string.Empty),
                        typeOptions, PieceOf(side), side == PushBackSide.A ? "Del catálogo de seguridad (seguridad.csv)." : string.Empty,
                        value => WritePushBack(c =>
                        {
                            if (boots) PushBackSafetyEdit.SetBootPiece(c, catalog, side, value);
                            else PushBackSafetyEdit.SetDefensePiece(c, catalog, side, value);
                            return null;
                        }, PushBackDefenseSidesIsNone(value)
                            ? name + (copy.TwoSided ? " del lado " + Letter(side) : string.Empty) + ": «Ninguno». Lo que tenía cada poste se conserva y vuelve al elegir otra vez un tipo."
                            : null), !edit);
                variant = TypeItem(PushBackSide.A, "variant");
                if (copy.TwoSided) fields.Add(TypeItem(PushBackSide.B, "typeB"));

                if (boots)
                {
                    sub = "En la base del poste, por lado: «Entrada/Salida» es su pasillo de carga y «Posterior», la cara opuesta. Cada poste, en la tabla.";
                    foreach (var side in copy.Sides)
                    {
                        var draft = PushBackSafetyEdit.BootSide(copy, catalog, side);
                        var suffix = copy.TwoSided ? " · lado " + Letter(side) : string.Empty;
                        fields.Add(new SelectiveChoiceItem("general" + Letter(side), "Ubicación general" + suffix,
                            PushBackSafetyEdit.Placements.Select(p => new SelectiveChoiceOption(PlacementCode(p), RackCadSafetyWords.Placement(p))).ToList(),
                            PlacementCode(draft.Placement),
                            draft.Explicit ? "Elegida. La toma cada poste que no tenga la suya." : "Automática: el pasillo de este lado. La toma cada poste que no tenga la suya.",
                            value => WritePushBack(c => { PushBackSafetyEdit.SetBootGeneral(c, catalog, side, PlacementOf(value)); return null; }, null), !edit));
                        actions.Add(new RackCadSafetyAction("general" + Letter(side), "Todos los postes al general" + suffix, edit && draft.Posts.Count > 0,
                            "Botas" + suffix + ": todos los postes al general",
                            () => WritePushBackOrSay(c => { PushBackSafetyEdit.BootPostsToGeneral(c, catalog, side); return null; }, null)));
                    }

                    var count = frames.Sum(r => r.Boots + r.BootsB);
                    pieces = "En el dibujo: " + Plural(count, "bota", "botas")
                             + (copy.TwoSided ? " (lado A: " + N(frames.Sum(r => r.Boots)) + "; lado B: " + N(frames.Sum(r => r.BootsB)) + ")" : string.Empty)
                             + ", en " + N(frames.Count(r => r.Boots + r.BootsB > 0)) + " de " + Plural(frames.Count, "poste", "postes") + ".";
                    notes.Add("En la tabla, un poste puede tener la suya: lleva la marca «Propia».");
                    notes.Add("«Ninguno» en el tipo no borra nada: la ubicación y los postes de ese lado vuelven al elegir otra vez un tipo.");
                    notes.Add("Un protector lateral reemplaza las botas de su poste.");
                    notes.Add(copy.TwoSided
                        ? "«Entrada/Salida» se lee dentro de cada lado: la de A es la cara cercana del rack y la de B, la lejana."
                        : "«Entrada/Salida» = el extremo bajo, por donde se carga y descarga; «Posterior» = el extremo alto, el del tope.");
                }
                else
                {
                    sub = copy.TwoSided
                        ? "Por poste, en el pasillo de cada lado: la «Entrada/Salida» de A y la de B."
                        : "Por poste, en «Entrada/Salida» y en «Posterior».";
                    var drawn = rows.Count(r => r.DefenseExit) + rows.Count(r => r.DefenseEntrance);
                    var own = PushBackSafetyEdit.DefenseSide(copy, catalog, PushBackSide.A).Posts.Sum(r => (r.ExitAuto ? 0 : 1) + (r.EntranceAuto ? 0 : 1));
                    pieces = "En el dibujo: " + Plural(drawn, "defensa", "defensas")
                             + (own > 0 ? "; " + Plural(own, "extremo con medida propia o sin defensa", "extremos con medida propia o sin defensa") : string.Empty) + ".";
                    foreach (var side in copy.Sides)
                    {
                        var suffix = copy.TwoSided ? " · lado " + Letter(side) : string.Empty;
                        var section = PushBackSafetyEdit.DefenseSide(copy, catalog, side);
                        var ownOfSide = section.Posts.Count(r => copy.TwoSided ? !(side == PushBackSide.B ? r.EntranceAuto : r.ExitAuto) : !(r.ExitAuto && r.EntranceAuto));
                        actions.Add(new RackCadSafetyAction("auto" + Letter(side), "Todas automáticas" + suffix, edit && ownOfSide > 0,
                            "Defensas" + suffix + ": todas automáticas",
                            () => WritePushBackOrSay(c => { PushBackSafetyEdit.DefenseAllAutomatic(c, catalog, side); return null; }, null)));
                    }

                    notes.Add("Medida vacía = automática: " + Num(DynamicForkliftDefensePlan.EdgeLength) + " in en los postes de orilla y "
                              + Num(DynamicForkliftDefensePlan.IntermediateLength) + " in en los intermedios. Se recalcula al añadir o quitar frentes.");
                    if (!copy.TwoSided) notes.Add("En «Posterior» la automática es sin defensa (PB-009): marca su casilla para ponerla.");
                    notes.Add("Al marcar una casilla se propone la medida de la regla, como en el diálogo clásico: queda como propia. Vacíala para volver a la automática.");
                    notes.Add("«Ninguno» en el tipo conserva lo de cada poste: vuelve al elegir otra vez un tipo.");
                }
            }
            else
            {
                // The protector lateral and the desviador: one variant for the rack; «Sin variante» asks first what is lost (SG-06), as the
                // «Ninguno» of the classic list takes the family out.
                var selection = RackCadSafetyEdit.Selected(copy.Safety, catalog, type);
                if (selection == null) _asking = null;
                hasVariant = selection != null;
                var variantName = selection == null ? string.Empty
                    : RackCadSafetyEdit.Variants(catalog, type).FirstOrDefault(v => string.Equals(v.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase))?.Label ?? selection.ElementId;
                var options = new[] { new SelectiveChoiceOption(NoVariant, "Sin variante (no se dibuja)") }
                    .Concat(RackCadSafetyEdit.Variants(catalog, type).Select(v => new SelectiveChoiceOption(v.Id, v.Label))).ToList();
                var shown = selection == null ? NoVariant
                    : options.FirstOrDefault(o => string.Equals(o.Value, selection.ElementId, StringComparison.OrdinalIgnoreCase))?.Value ?? selection.ElementId;
                variant = new SelectiveChoiceItem("variant", "Variante", options, shown, "Del catálogo de seguridad (seguridad.csv). Una sola variante por familia.",
                    value => SetPushBackVariant(type, value), !edit);
                loss.Add("Variante «" + variantName + "»");

                if (Is(type, SelectiveSafetyDefaults.LateralType))
                {
                    sub = "En los postes de los extremos, en el extremo de entrada/salida, con la guía de canal.";
                    if (selection != null)
                    {
                        var rule = PushBackSafetyEdit.LateralFollowsRule(selection);
                        fields.Add(new RackCadSafetyTextField("how", "Cómo se reparte", rule
                            ? "Regla de los extremos: en P1 y en P" + N(copy.PostCount) + ", con la guía en lados opuestos. Se recalcula al añadir o quitar frentes."
                            : "Elegido por poste en la tabla. Ya no se recalcula al añadir o quitar frentes."));
                        var guarded = frames.Where(r => r.SideGuards > 0).ToList();
                        pieces = guarded.Count == 0
                            ? "Elegido, en ningún poste: no se dibuja."
                            : "En el dibujo: " + Plural(guarded.Sum(r => r.SideGuards), "protector", "protectores") + ", en " + string.Join(", ", guarded.Select(r => r.Post)) + ".";
                        actions.Add(new RackCadSafetyAction("rule", "Volver a la regla de los extremos", edit && !rule, "Protector lateral: volver a la regla de los extremos",
                            () => WritePushBackOrSay(c => { PushBackSafetyEdit.LateralToRule(c.Safety, catalog); return null; }, null)));
                        loss.Add(rule ? "La regla de los extremos" : guarded.Count > 0 ? "En " + string.Join(", ", guarded.Select(r => r.Post)) : "Ningún poste elegido");
                    }

                    notes.Add("Va en el extremo de entrada/salida: Push Back carga y descarga por un solo extremo.");
                    notes.Add("«Izquierda» o «Derecha»: de qué lado queda la guía de canal, como en el diálogo clásico. «Ambos»: guía a los dos lados.");
                    notes.Add("Cambiar un poste en la tabla lo pasa a «Elegido por poste», con los extremos ya escritos, y lo avisa.");
                    notes.Add("Reemplaza las botas de su poste.");
                }
                else
                {
                    sub = "Por poste y nivel, en el extremo de entrada/salida. Los niveles, en la tabla.";
                    if (selection != null)
                    {
                        var levels = host.SafetyDesviadorLevels;
                        var plan = RackCad.Application.Systems.Selective.SelectiveDesviadorPlan.Build(null, null, selection);
                        fields.Add(DesviadorMeasure("length", "Longitud", plan.Longitud, "Entero par mayor de 8.", "La longitud debe ser un entero par mayor de 8.", edit,
                            (d, value) => d.DesviadorLongitud = value));
                        fields.Add(DesviadorMeasure("first", "Primer nivel sobre el primer troquel", plan.FirstLevelHeight,
                            "Entero par mayor de 8. N1 existe aunque no haya larguero a piso.", "El primer nivel debe ser un entero par mayor de 8.", edit,
                            (d, value) => d.DesviadorPrimerNivelAltura = value));
                        var shownCells = rows.Where(r => r.DeflectorKey.HasValue)
                            .SelectMany(r => Enumerable.Range(0, r.DeflectorLevels).Select(l => (Key: r.DeflectorKey.Value, Level: l))).ToList();
                        var off = shownCells.Count(c => !RackCadSafetyEdit.DesviadorOn(selection, c.Key, c.Level));
                        pieces = "En el dibujo: " + Plural(rows.Sum(r => r.Deflectors), "desviador", "desviadores") + (off > 0 ? "; " + Plural(off, "nivel apagado", "niveles apagados") : string.Empty) + ".";
                        actions.Add(new RackCadSafetyAction("all", "En todos los niveles", edit && off > 0, "Desviadores en todos los niveles",
                            () => WritePushBackOrSay(c => { PushBackSafetyEdit.SetDesviadorAll(c.Safety, catalog, levels, true); return null; }, null)));
                        actions.Add(new RackCadSafetyAction("none", "En ninguno", edit && off < shownCells.Count, "Desviadores en ningún nivel",
                            () => WritePushBackOrSay(c => { PushBackSafetyEdit.SetDesviadorAll(c.Safety, catalog, levels, false); return null; },
                                "Desviadores: en ningún nivel. La variante y sus medidas se quedan; no se dibuja ni se cuenta nada.")));
                        loss.Add("Longitud " + Num(plan.Longitud) + " in · primer nivel a " + Num(plan.FirstLevelHeight) + " in");
                        if (off > 0) loss.Add(Plural(off, "nivel apagado", "niveles apagados"));
                    }

                    notes.Add("Por poste (PB-002) y por nivel; los niveles siguen la numeración de «Matriz».");
                    notes.Add("Va en el extremo de entrada/salida, sin lado que elegir (PB-003): en un rack de dos sentidos, en el pasillo de cada cama.");
                    notes.Add("Los de arriba van 6 in debajo del larguero.");
                }
            }

            var asking = hasVariant && string.Equals(_asking, type, StringComparison.Ordinal);
            Inspector.Set(type, name, string.Empty, sub, variant, hasVariant, asking, "¿Quitar " + name.ToLowerInvariant() + "?",
                "Quitar " + name.ToLowerInvariant(), loss, fields, string.Empty, notes, pieces, actions);
        }

        // The variant of the protector lateral or of the desviador: «Sin variante» is the shared one (SG-06 asks before); another variant is
        // written as the list of the classic dialog writes it (PushBackSafetyEdit.SetVariant: the row again, a new desviador through its grid).
        private string SetPushBackVariant(string type, string value)
        {
            if (value == NoVariant) return SetVariant(type, value);
            var catalog = _host.SafetyCatalog;
            var isNew = RackCadSafetyEdit.Selected(_host.CopySafetyForPanel(), catalog, type) == null;
            var label = RackCadSafetyEdit.Variants(catalog, type).FirstOrDefault(v => string.Equals(v.Id, value, StringComparison.OrdinalIgnoreCase))?.Label ?? value;
            _asking = null;
            return WritePushBack(copy =>
            {
                PushBackSafetyEdit.SetVariant(copy.Safety, catalog, type, value, copy.PostCount, PushBackHost.SafetyDesviadorLevels);
                return null;
            }, isNew ? FamilyName(type) + ": «" + label + "», con sus valores por defecto. Revísalos aquí y en la tabla." : null);
        }

        private DynamicFieldItem DesviadorMeasure(string key, string label, double value, string hint, string error, bool edit, Action<SelectiveSafetySelection, double> write)
            => new DynamicFieldItem(key, label, "in", Num(value), hint, text =>
            {
                if (!UiSupport.TryNum(text, out var number) || !RackCad.Application.Systems.Selective.SelectiveDesviadorPlan.IsValidEvenAbove8(number)) return error;
                return WritePushBack(copy =>
                {
                    PushBackSafetyEdit.SetDesviadorMeasure(copy.Safety, _host.SafetyCatalog, PushBackHost.SafetyDesviadorLevels, d => write(d, number));
                    return null;
                }, null);
            }, !edit);

        // ------------------------------------------------------------------ helpers

        private string WritePushBack(Func<PushBackSafetyCopy, string> edit, string notice)
            => PushBackHost is IPushBackSafetyPanelHost host ? host.WritePushBackSafety(edit, notice) : "El editor no puede escribir la seguridad de Push Back.";

        private void WritePushBackOrSay(Func<PushBackSafetyCopy, string> edit, string notice)
        {
            var error = WritePushBack(edit, notice);
            if (error != null) Say(error, error: true);
        }

        private SelectiveSafetySelection FindIn(PushBackSafetyCopy copy, string type) => Find(copy.Safety, type);

        private static bool PushBackDefenseSidesIsNone(string id) => string.Equals(id, PushBackDefaults.NonePieceId, StringComparison.OrdinalIgnoreCase);

        private static string Letter(PushBackSide side) => side == PushBackSide.B ? "B" : "A";

        internal static string PlacementCode(BootPlacement placement)
            => placement switch { BootPlacement.EntryExit => "E", BootPlacement.Rear => "P", BootPlacement.Both => "A", _ => "N" };

        internal static BootPlacement PlacementOf(string code)
            => code switch { "E" => BootPlacement.EntryExit, "P" => BootPlacement.Rear, "A" => BootPlacement.Both, _ => BootPlacement.None };
    }
}
