using System.Text;

namespace RackCad.Application
{
    /// <summary>
    /// Pure string helpers for naming AutoCAD blocks. They live here (not in the Plugin) so they are
    /// unit-testable without referencing AutoCAD — the Plugin just delegates.
    /// </summary>
    public static class BlockNaming
    {
        /// <summary>Make a string safe as an AutoCAD block name (empty → "Cabecera", invalid chars → space).</summary>
        public static string SanitizeBlockName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Cabecera";
            }

            // AutoCAD block names cannot contain < > / \ " : ; ? * | , = `
            var invalid = new[] { '<', '>', '/', '\\', '"', ':', ';', '?', '*', '|', ',', '=', '`' };
            var cleaned = name.Trim();

            foreach (var character in invalid)
            {
                cleaned = cleaned.Replace(character, ' ');
            }

            return cleaned.Trim();
        }

        /// <summary>Collapses internal newlines/tabs/repeated spaces so a CSV display name reads on one line.</summary>
        public static string NormalizeWhitespace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var builder = new StringBuilder(value.Length);
            var previousWasSpace = false;

            foreach (var character in value)
            {
                if (char.IsWhiteSpace(character))
                {
                    if (!previousWasSpace)
                    {
                        builder.Append(' ');
                        previousWasSpace = true;
                    }
                }
                else
                {
                    builder.Append(character);
                    previousWasSpace = false;
                }
            }

            return builder.ToString().Trim();
        }
    }
}
