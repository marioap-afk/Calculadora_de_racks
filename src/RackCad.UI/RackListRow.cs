namespace RackCad.UI
{
    /// <summary>
    /// One row of the RACKLISTA window. Built by the plugin from a <c>RackListEntry</c> plus the number of
    /// placed copies (block references) it counted in the drawing; the window only displays it.
    /// </summary>
    public sealed class RackListRow
    {
        public RackListRow(string id, string name, string kindLabel, string viewsLabel, int copiesCount)
        {
            Id = id;
            Name = name;
            KindLabel = kindLabel;
            ViewsLabel = viewsLabel;
            CopiesCount = copiesCount;
        }

        /// <summary>Stable rack identity (GUID); not shown in the grid, used to zoom to the chosen rack.</summary>
        public string Id { get; }

        public string Name { get; }

        public string KindLabel { get; }

        public string ViewsLabel { get; }

        /// <summary>Placed copies across ALL the rack's view-blocks.</summary>
        public int CopiesCount { get; }
    }
}
