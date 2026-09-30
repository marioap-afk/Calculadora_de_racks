using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// I-60: the automatic logical name of a NEW rack (<see cref="RackEmbedDocument.Name"/>; never the block definition name, the
    /// AUTH-11 BaseName nor the RackId). Pure: the Plugin supplies the logical names already in the drawing and applies the result
    /// only on the creation paths of a new logical rack. The name is <c>«Prefijo» N</c>, with N one above the largest N of the names
    /// that follow that exact pattern: holes are not filled, case is ignored, and custom names only count when they follow the pattern.
    /// Frozen rule: docs/initiatives/I-60-freeze.md §3.
    /// </summary>
    public static class RackLogicalNameAllocator
    {
        /// <summary>The short product label of each family: the one the BOM shows (<c>IRackKindHandler.BomLabel</c>).</summary>
        public static string PrefixOf(RackSystemKind kind)
        {
            switch (kind)
            {
                case RackSystemKind.SelectiveRack: return "Selectivo";
                case RackSystemKind.PalletFlow: return "Dinámico";
                case RackSystemKind.PushBack: return "Push Back";
                case RackSystemKind.Cantilever: return "Cantilever";
                case RackSystemKind.Selective: return "Cabecera";
                case RackSystemKind.Cama: return "Cama";
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, "No es un rack logico con nombre automatico.");
            }
        }

        /// <summary>
        /// The N (as its decimal digits, no size limit) of a name that follows exactly <c>«prefix» N</c>: outer white space ignored,
        /// one U+0020, the prefix compared ordinally without case and without Unicode normalization, N made of ASCII digits, no
        /// leading zero and at least 1.
        /// </summary>
        public static bool TryReadNumber(string name, string prefix, out string number)
        {
            number = null;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(prefix))
            {
                return false;
            }

            var text = name.Trim();
            if (text.Length < prefix.Length + 2
                || !string.Equals(text.Substring(0, prefix.Length), prefix, StringComparison.OrdinalIgnoreCase)
                || text[prefix.Length] != ' ')
            {
                return false;
            }

            var digits = text.Substring(prefix.Length + 1);
            if (digits[0] < '1' || digits[0] > '9' || digits.Any(c => c < '0' || c > '9'))
            {
                return false;
            }

            number = digits;
            return true;
        }

        /// <summary>The next automatic name of a family, from the logical names already in the drawing.</summary>
        public static string Next(RackSystemKind kind, IEnumerable<string> existingLogicalNames)
        {
            var prefix = PrefixOf(kind);
            var max = "0";
            foreach (var name in existingLogicalNames ?? Enumerable.Empty<string>())
            {
                if (TryReadNumber(name, prefix, out var number) && IsGreater(number, max))
                {
                    max = number;
                }
            }

            return prefix + " " + Increment(max);
        }

        /// <summary>
        /// Whether the name requested for a NEW rack leaves it unnamed: null, empty or white space; for a cabecera also the name of a
        /// built-in template of <see cref="RackFrameTemplateCatalog"/>, which describes the type and every new cabecera carries today.
        /// </summary>
        public static bool IsUnassigned(RackSystemKind kind, string requestedName)
        {
            if (string.IsNullOrWhiteSpace(requestedName))
            {
                return true;
            }

            return kind == RackSystemKind.Selective
                && RackFrameTemplateCatalog.All.Any(template => template.Name != null && string.Equals(
                    template.Name.Trim(), requestedName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>The logical name of a new rack: the requested one when assigned, otherwise the next automatic one.</summary>
        public static string ForNewRack(RackSystemKind kind, string requestedName, IEnumerable<string> existingLogicalNames)
            => IsUnassigned(kind, requestedName) ? Next(kind, existingLogicalNames) : requestedName;

        /// <summary>Compares two decimal digit strings without leading zeros.</summary>
        private static bool IsGreater(string left, string right)
            => left.Length != right.Length
                ? left.Length > right.Length
                : string.CompareOrdinal(left, right) > 0;

        /// <summary>Adds one to a decimal digit string (no overflow: the string grows).</summary>
        private static string Increment(string digits)
        {
            var chars = digits.ToCharArray();
            for (var index = chars.Length - 1; index >= 0; index--)
            {
                if (chars[index] < '9')
                {
                    chars[index]++;
                    return new string(chars);
                }

                chars[index] = '0';
            }

            return "1" + new string(chars);
        }
    }
}
