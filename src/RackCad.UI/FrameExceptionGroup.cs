using System.Collections.ObjectModel;
using System.Globalization;

namespace RackCad.UI
{
    public sealed class FrameExceptionGroup
    {
        public FrameExceptionGroup(string targetId)
        {
            TargetId = targetId;
            Changes = new ObservableCollection<FrameExceptionEditorRow>();
        }

        public string TargetId { get; private set; }
        public ObservableCollection<FrameExceptionEditorRow> Changes { get; private set; }

        public string CountText => Changes.Count.ToString(CultureInfo.InvariantCulture) + " cambio(s)";
    }
}
