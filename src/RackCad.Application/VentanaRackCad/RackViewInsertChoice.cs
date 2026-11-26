using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The three groups of the panel «Insertar vista…», always in this order (DG-06/DG-07 of the prototype «dialogos»).</summary>
    public enum RackViewInsertGroup
    {
        Frontal = 0,
        Lateral = 1,
        Planta = 2,
    }

    /// <summary>One view the panel offers: its label in the order (as «Insertar varias…» of the classic editor names it) and its address.</summary>
    public sealed record RackViewInsertOption(string Label, RackViewAddress Address);

    /// <summary>
    /// One row of «Orden de colocación»: the view, its label, whether the draft can still draw it, and its place among the views that are
    /// inserted (from 1; 0 for a gone one). A view that the draft lost while the panel was open stays in the order, marked, has no number
    /// and is not inserted (DG-08; correction 5 of the review: the numbers count only what is inserted).
    /// </summary>
    public sealed record RackViewInsertEntry(RackViewAddress Address, string Label, bool IsGone, int Position);

    /// <summary>
    /// The choice of the panel «Insertar vista…» of the RackCad window (I-64 A-14, line D, fourth batch, part «ins»; prototype «dialogos»,
    /// DG-01 to DG-08), with no WPF and no AutoCAD, the same for every system: it replaces the classic dialog «Insertar varias vistas»
    /// (<c>RackViewBatchDialog</c>) inside the window, and the editors of Push Back, Cantilever and Cabecera take it when they migrate (DG-07).
    /// <list type="bullet">
    /// <item>The views offered are those the system gives, filtered by the same exposure policy the classic dialog applies
    /// (<see cref="RackViewExposure"/>, <see cref="RackViewProductOperation.Batch"/>: a Cama offers none), and shown in the groups Frontal ·
    /// Lateral · Planta, each one in its natural order (fondo, end, side, post or station by its index), whatever order the system gave.</item>
    /// <item>Each view is marked once (a check box, DG-03): the order is the order of marking and changes with «Subir» / «Bajar» (DG-02).</item>
    /// <item>The panel follows the draft (<see cref="Follow"/>): a marked view the draft can no longer draw stays in the order, gone, and
    /// <see cref="ToInsert"/> leaves it out (DG-08).</item>
    /// <item>Every gesture that cannot do anything does nothing and reports it (false): there is no index out of range (dolor 4).</item>
    /// </list>
    /// </summary>
    public sealed class RackViewInsertChoice
    {
        private readonly RackSystemKind _system;
        private readonly List<RackViewInsertOption> _options = new List<RackViewInsertOption>();
        private readonly List<RackViewAddress> _order = new List<RackViewAddress>();

        // The last label each marked view had while it was offered: a gone view keeps it in the order.
        private readonly Dictionary<RackViewAddress, string> _labels = new Dictionary<RackViewAddress, string>();

        public RackViewInsertChoice(RackSystemKind system, IEnumerable<RackViewInsertOption> options)
        {
            _system = system;
            Offer(options);
        }

        /// <summary>The system whose exposure policy filters the views.</summary>
        public RackSystemKind System => _system;

        /// <summary>The views offered now, in the order of the panel: Frontal · Lateral · Planta, each group in its natural order.</summary>
        public IReadOnlyList<RackViewInsertOption> Options => _options;

        /// <summary>«Orden de colocación»: the marked views, in the order AutoCAD will ask for their points.</summary>
        public IReadOnlyList<RackViewInsertEntry> Order
        {
            get
            {
                var entries = new List<RackViewInsertEntry>(_order.Count);
                var position = 0;
                foreach (var address in _order)
                {
                    var gone = !IsOffered(address);
                    entries.Add(new RackViewInsertEntry(address, LabelOf(address), gone, gone ? 0 : ++position));
                }

                return entries;
            }
        }

        /// <summary>What «Insertar N vistas» sends: the marked views the draft can draw, in their order.</summary>
        public IReadOnlyList<RackViewAddress> ToInsert => _order.Where(IsOffered).ToList();

        public int InsertCount => _order.Count(IsOffered);

        /// <summary>A marked view the draft can no longer draw is in the order (DG-08).</summary>
        public bool HasGone => _order.Any(address => !IsOffered(address));

        public bool IsEmpty => _order.Count == 0;

        /// <summary>The groups that have at least one view, in the order Frontal · Lateral · Planta.</summary>
        public IReadOnlyList<RackViewInsertGroup> Groups =>
            _options.Select(o => GroupOf(o.Address)).Distinct().OrderBy(g => (int)g).ToList();

        public IReadOnlyList<RackViewInsertOption> OptionsOf(RackViewInsertGroup group) =>
            _options.Where(o => GroupOf(o.Address) == group).ToList();

        public bool IsOffered(RackViewAddress address) => _options.Any(o => o.Address.Equals(address));

        public bool IsMarked(RackViewAddress address) => _order.Contains(address);

        /// <summary>
        /// The place of the view among the views that are inserted, from 1 (the number AutoCAD asks for its point in); 0 when it is not
        /// marked or the draft lost it. A gone view before it does not count.
        /// </summary>
        public int PositionOf(RackViewAddress address)
        {
            if (!IsMarked(address) || !IsOffered(address)) return 0;
            return _order.TakeWhile(a => !a.Equals(address)).Count(IsOffered) + 1;
        }

        /// <summary>The row of the view in «Orden de colocación» (gone rows included), from 0; −1 when it is not marked.</summary>
        public int IndexOf(RackViewAddress address) => _order.IndexOf(address);

        /// <summary>A check box of «Vistas de este rack»: marks the view at the end of the order, or unmarks it.</summary>
        public bool Toggle(RackViewAddress address) => IsMarked(address) ? Unmark(address) : Mark(address);

        /// <summary>Marks an offered view at the end of the order; a view already marked, or not offered, does nothing.</summary>
        public bool Mark(RackViewAddress address)
        {
            if (IsMarked(address) || !IsOffered(address)) return false;
            _order.Add(address);
            _labels[address] = LabelOf(address);
            return true;
        }

        /// <summary>Takes the view out of the order (also a gone one).</summary>
        public bool Unmark(RackViewAddress address)
        {
            var index = _order.IndexOf(address);
            return index >= 0 && Remove(index);
        }

        /// <summary>«Subir» of a row of the order; the first row does nothing.</summary>
        public bool MoveUp(int index) => Swap(index, index - 1);

        /// <summary>«Bajar» of a row of the order; the last row does nothing.</summary>
        public bool MoveDown(int index) => Swap(index, index + 1);

        /// <summary>«Quitar» of a row of the order.</summary>
        public bool Remove(int index)
        {
            if (index < 0 || index >= _order.Count) return false;
            var address = _order[index];
            _order.RemoveAt(index);
            _labels.Remove(address);
            return true;
        }

        /// <summary>«Todos los postes» (or the stations): marks, at the end and in their order, every indexed lateral cut not marked yet.</summary>
        public bool MarkAllCuts()
        {
            var changed = false;
            foreach (var option in _options.Where(o => IsIndexedCut(o.Address)))
                changed |= Mark(option.Address);
            return changed;
        }

        /// <summary>«Ninguno»: takes every indexed lateral cut out of the order (also the gone ones); the other views keep their order.</summary>
        public bool UnmarkAllCuts()
        {
            var removed = _order.Where(IsIndexedCut).ToList();
            foreach (var address in removed) Unmark(address);
            return removed.Count > 0;
        }

        /// <summary>Every offered indexed lateral cut is marked.</summary>
        public bool AllCutsMarked => _options.Where(o => IsIndexedCut(o.Address)).All(o => IsMarked(o.Address));

        /// <summary>Some lateral cut (offered or gone) is in the order.</summary>
        public bool AnyCutMarked => _order.Any(IsIndexedCut);

        /// <summary>
        /// The starting point of a rack that is not in the drawing yet (DG-02, as the approved draft of Selectivo: «Frontal · fondo 1»,
        /// «Lateral · poste 1» and «Planta»): the first view of each group, in the order of the groups. A rack of the drawing starts empty.
        /// </summary>
        public void SeedFirstOfEachGroup()
        {
            foreach (var group in Groups)
            {
                var first = _options.FirstOrDefault(o => GroupOf(o.Address) == group);
                if (first != null) Mark(first.Address);
            }
        }

        /// <summary>
        /// The panel follows the draft (DG-08): the views offered are the ones the draft can draw now. The order keeps every marked view;
        /// one that is no longer offered stays, gone, with the label it had, and comes back as it was if the draft can draw it again.
        /// </summary>
        public void Follow(IEnumerable<RackViewInsertOption> options)
        {
            Offer(options);
            foreach (var address in _order)
            {
                var offered = _options.FirstOrDefault(o => o.Address.Equals(address));
                if (offered != null) _labels[address] = offered.Label;
            }
        }

        // ------------------------------------------------------------------ the groups and the short names of the views

        /// <summary>The group of a view: by its kind (Frontal, Lateral, Planta).</summary>
        public static RackViewInsertGroup GroupOf(RackViewAddress address)
        {
            switch (address.Kind)
            {
                case DimensionViewKind.Frontal: return RackViewInsertGroup.Frontal;
                case DimensionViewKind.Lateral: return RackViewInsertGroup.Lateral;
                default: return RackViewInsertGroup.Planta;
            }
        }

        /// <summary>A lateral cut by post or by station: «Todos los postes» / «Ninguno» mark or unmark them together.</summary>
        public static bool IsIndexedCut(RackViewAddress address)
            => address.Kind == DimensionViewKind.Lateral
               && (address.Variant.Kind == RackViewVariantKind.Post || address.Variant.Kind == RackViewVariantKind.Station);

        /// <summary>
        /// The name of the view inside its group (the chip of the prototype): «Fondo 1», «Salida», «Entrada», «Entrada/salida A»,
        /// «Posterior B», «Frontal», «Poste 3», «Estación 2», «Lateral», «Planta».
        /// </summary>
        public static string ChipOf(RackViewAddress address)
        {
            var variant = address.Variant;
            switch (variant.Kind)
            {
                case RackViewVariantKind.Fondo: return "Fondo " + Number(variant.Index);
                case RackViewVariantKind.Post: return "Poste " + Number(variant.Index);
                case RackViewVariantKind.Station: return "Estación " + Number(variant.Index);
                case RackViewVariantKind.FlowEnd: return variant.FlowEnd == RackFlowEnd.Exit ? "Salida" : "Entrada";
                case RackViewVariantKind.PushBackCut:
                    return (variant.PushBackEnd == RackPushBackEnd.EntradaSalida ? "Entrada/salida " : "Posterior ") + variant.PushBackSide;
                default:
                    switch (address.Kind)
                    {
                        case DimensionViewKind.Frontal: return "Frontal";
                        case DimensionViewKind.Lateral: return "Lateral";
                        default: return "Planta";
                    }
            }
        }

        // The natural order inside a group: a whole view first; then the fondo, the end (salida before entrada), the side of Push Back (A
        // before B, entrada/salida before posterior), the post or the station by its index. The classic lists put the laterals of Push Back
        // first and backwards, and the plan of Cantilever before its stations (dolor 6): here every system reads the same.
        private static int RankInGroup(RackViewAddress address)
        {
            var variant = address.Variant;
            switch (variant.Kind)
            {
                case RackViewVariantKind.Whole: return -1;
                case RackViewVariantKind.FlowEnd: return variant.FlowEnd == RackFlowEnd.Exit ? 0 : 1;
                case RackViewVariantKind.PushBackCut:
                    return (variant.PushBackSide == RackPushBackSide.A ? 0 : 2) + (variant.PushBackEnd == RackPushBackEnd.EntradaSalida ? 0 : 1);
                default: return variant.Index;
            }
        }

        private void Offer(IEnumerable<RackViewInsertOption> options)
        {
            var offered = (options ?? Enumerable.Empty<RackViewInsertOption>())
                .Where(o => o != null && !string.IsNullOrWhiteSpace(o.Label)
                            && RackViewExposure.IsExposed(_system, o.Address, RackViewProductOperation.Batch))
                .GroupBy(o => o.Address)
                .Select(g => g.First())
                .Select((option, index) => (option, index))
                .OrderBy(x => (int)GroupOf(x.option.Address))
                .ThenBy(x => RankInGroup(x.option.Address))
                .ThenBy(x => x.index)
                .Select(x => x.option)
                .ToList();
            _options.Clear();
            _options.AddRange(offered);
        }

        private bool Swap(int index, int other)
        {
            if (index < 0 || index >= _order.Count || other < 0 || other >= _order.Count) return false;
            (_order[index], _order[other]) = (_order[other], _order[index]);
            return true;
        }

        private string LabelOf(RackViewAddress address)
        {
            var offered = _options.FirstOrDefault(o => o.Address.Equals(address));
            if (offered != null) return offered.Label;
            return _labels.TryGetValue(address, out var label) ? label : ChipOf(address);
        }

        private static string Number(int index) => (index + 1).ToString(CultureInfo.InvariantCulture);
    }
}
