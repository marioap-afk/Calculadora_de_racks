namespace RackCad.UI
{
    public sealed class ConfiguratorNavigationItem
    {
        public ConfiguratorNavigationItem(string key, string title, string subtitle)
            : this(key, title, subtitle, null, null)
        {
        }

        public ConfiguratorNavigationItem(string key, string title, string subtitle, BracingSegmentEditorRow segment)
            : this(key, title, subtitle, segment, null)
        {
        }

        public ConfiguratorNavigationItem(string key, string title, string subtitle, HorizontalEditorRow horizontal)
            : this(key, title, subtitle, null, horizontal)
        {
        }

        private ConfiguratorNavigationItem(string key, string title, string subtitle, BracingSegmentEditorRow segment, HorizontalEditorRow horizontal)
        {
            Key = key;
            Title = title;
            Subtitle = subtitle;
            Segment = segment;
            Horizontal = horizontal;
            Children = new System.Collections.ObjectModel.ObservableCollection<ConfiguratorNavigationItem>();
            IsExpanded = true;
        }

        public string Key { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public BracingSegmentEditorRow Segment { get; private set; }
        public HorizontalEditorRow Horizontal { get; private set; }
        public System.Collections.ObjectModel.ObservableCollection<ConfiguratorNavigationItem> Children { get; private set; }
        public bool IsExpanded { get; set; }

        public bool IsSegment => Segment != null;
        public bool IsHorizontal => Horizontal != null;
    }
}
