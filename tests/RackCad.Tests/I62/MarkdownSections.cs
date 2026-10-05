#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>
    /// Sections of a Markdown authority as AUTOMATION_PLAN 16.3 and Proposal V14 Anexo E.3.1/E.4 define them: a section runs from its heading to the
    /// next heading with the same number of <c>#</c> or fewer and includes its subsections; the preamble (<c>preámbulo</c>, level 0) is the text before
    /// the first <c>##</c>; lines inside fenced code blocks are not headings; headings and texts are compared normalized (CRLF → LF, whitespace
    /// collapsed, ends trimmed).
    /// </summary>
    public static class MarkdownSections
    {
        public const string Preamble = "preámbulo";

        public sealed record Section(int Level, string Heading, string Text, int Line);

        public static string Norm(string s) => Regex.Replace(s, @"\s+", " ").Trim();

        public static int HeadingLevel(string line)
        {
            var m = Regex.Match(line, "^(#{1,6}) ");
            return m.Success ? m.Groups[1].Value.Length : 0;
        }

        public static string[] Lines(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        /// <summary>The preamble followed by every heading's section, in document order.</summary>
        public static List<Section> Of(string text)
        {
            var lines = Lines(text);
            var heads = new List<(int Index, int Level)>();
            var fence = false;
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].TrimStart().StartsWith("```", StringComparison.Ordinal))
                {
                    fence = !fence;
                    continue;
                }

                var level = fence ? 0 : HeadingLevel(lines[i]);
                if (level > 0)
                {
                    heads.Add((i, level));
                }
            }

            var firstSecond = heads.Where(h => h.Level >= 2).Select(h => h.Index).DefaultIfEmpty(lines.Length).First();
            var result = new List<Section> { new Section(0, Preamble, Norm(string.Join("\n", lines.Take(firstSecond))), 0) };
            for (var k = 0; k < heads.Count; k++)
            {
                var (index, level) = heads[k];
                var end = heads.Skip(k + 1).Where(h => h.Level <= level).Select(h => h.Index).DefaultIfEmpty(lines.Length).First();
                result.Add(new Section(level, Norm(lines[index]), Norm(string.Join("\n", lines.Skip(index).Take(end - index))), index));
            }

            return result;
        }

        /// <summary>Match(rev, Path, Section) of E.3.1 on one text: the preamble, or every heading whose normalized line equals the normalized section.</summary>
        public static List<Section> Match(string? text, string section)
        {
            if (text == null)
            {
                return new List<Section>();
            }

            return section == Preamble
                ? Of(text).Where(s => s.Level == 0).ToList()
                : Of(text).Where(s => s.Level > 0 && s.Heading == Norm(section)).ToList();
        }

        /// <summary>The raw lines of the single section with that heading (heading line included), or null when it is absent or repeated.</summary>
        public static List<string>? RawLines(string text, string heading)
        {
            var found = Match(text, heading);
            if (found.Count != 1)
            {
                return null;
            }

            var lines = Lines(text);
            var start = found[0].Line;
            var level = found[0].Level;
            var all = Of(text);
            var end = all.Where(s => s.Level > 0 && s.Line > start && s.Level <= level).Select(s => s.Line).DefaultIfEmpty(lines.Length).First();
            return lines.Skip(start).Take(end - start).ToList();
        }

        /// <summary>Normalized heading lines that appear more than once outside code fences (E.4: headings are unique per file).</summary>
        public static List<string> DuplicateHeadings(string text) =>
            Of(text).Where(s => s.Level > 0).GroupBy(s => s.Heading, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
    }
}
