using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.Views
{
    public sealed class RackViewBatchOption
    {
        public RackViewBatchOption(string label, RackViewAddress address)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Address = address;
        }

        public string Label { get; }
        public RackViewAddress Address { get; }
        public override string ToString() => Label;
    }

    /// <summary>Typed UI composition only: exposure and order live here; preparation remains outside the dialog.</summary>
    public sealed class RackViewBatchDialogPresenter
    {
        public RackViewBatchDialogPresenter(RackSystemKind kind, IEnumerable<RackViewBatchOption> candidates)
        {
            Kind = kind;
            Available = new ObservableCollection<RackViewBatchOption>((candidates ?? Array.Empty<RackViewBatchOption>())
                .Where(item => item != null && RackViewExposure.IsExposed(kind, item.Address, RackViewProductOperation.Batch)));
            Selected = new ObservableCollection<RackViewBatchOption>();
        }

        public RackSystemKind Kind { get; }
        public ObservableCollection<RackViewBatchOption> Available { get; }
        public ObservableCollection<RackViewBatchOption> Selected { get; }
        public IReadOnlyList<RackViewAddress> Result => Array.AsReadOnly(Selected.Select(item => item.Address).ToArray());

        public void Add(RackViewBatchOption option) { if (option != null && Available.Contains(option)) Selected.Add(option); }
        public void RemoveAt(int index) { if (index >= 0 && index < Selected.Count) Selected.RemoveAt(index); }
        public void MoveUp(int index) => Move(index, index - 1);
        public void MoveDown(int index) => Move(index, index + 1);

        private void Move(int from, int to)
        {
            if (from < 0 || from >= Selected.Count || to < 0 || to >= Selected.Count) return;
            Selected.Move(from, to);
        }

        public static bool TryShow(
            Window owner,
            RackSystemKind kind,
            IEnumerable<RackViewBatchOption> candidates,
            out IReadOnlyList<RackViewAddress> views)
        {
            var presenter = new RackViewBatchDialogPresenter(kind, candidates);
            var dialog = new RackViewBatchDialog(presenter) { Owner = owner };
            var accepted = dialog.ShowDialog() == true;
            views = accepted ? presenter.Result : Array.Empty<RackViewAddress>();
            return accepted;
        }
    }
}
