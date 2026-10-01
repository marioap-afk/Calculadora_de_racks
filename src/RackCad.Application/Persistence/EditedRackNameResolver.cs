namespace RackCad.Application.Persistence
{
    /// <summary>
    /// I-61 (D-1a): the logical name an edit route persists. Pure: the Plugin supplies the name read from the editor window and the
    /// name of the drawn envelope. The other five edit routes already apply this rule inline
    /// (<c>string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName</c>); the cama route uses this function.
    /// </summary>
    public static class EditedRackNameResolver
    {
        /// <summary>
        /// Blank or white-space-only edited name resolves to the envelope name (including null); any other edited name is kept as is.
        /// </summary>
        public static string Resolve(string editedName, string envelopeName)
        {
            return editedName;
        }
    }
}
