using System;
using System.Buffers;
using System.Text;

namespace RackCad.Application.CustomProperties
{
    /// <summary>What walking a text by Unicode scalar values found.</summary>
    internal enum CustomPropertyTextScan
    {
        /// <summary>Well-formed UTF-16 with no Unicode noncharacter.</summary>
        WellFormed = 1,

        /// <summary>At least one unpaired surrogate. Nothing else about the text is trustworthy.</summary>
        MalformedUtf16 = 2,

        /// <summary>Well-formed UTF-16 that contains at least one Unicode noncharacter.</summary>
        ContainsNoncharacter = 3,
    }

    /// <summary>
    /// The text rules that the store and the pure mutations must apply IDENTICALLY (I-54 D-05, C-3, C-F1): the store
    /// accredits a name on read with them, and a mutation validates a new name or value with them.
    ///
    /// <para>
    /// The order is the contract. First the text must be well-formed UTF-16 — a lone surrogate would otherwise be
    /// replaced by U+FFFD on serialization, silently, and <c>Normalize(FormC)</c> throws on it. Then a name must not
    /// contain a Unicode noncharacter: with ICU, <c>Normalize(FormC)</c> also throws on U+FFFE, and a stricter platform
    /// normalizer may throw on the other 65. Only a name that passed both steps — an ACCREDITED name — ever reaches NFC.
    /// </para>
    /// </summary>
    internal static class CustomPropertyText
    {
        /// <summary>One pass by Unicode scalar value (<see cref="Rune"/>). An unpaired surrogate wins over everything else.</summary>
        internal static CustomPropertyTextScan Scan(string text)
        {
            var remaining = text.AsSpan();
            var noncharacter = false;

            while (!remaining.IsEmpty)
            {
                if (Rune.DecodeFromUtf16(remaining, out var rune, out var consumed) != OperationStatus.Done)
                {
                    return CustomPropertyTextScan.MalformedUtf16;
                }

                noncharacter |= IsNoncharacter(rune.Value);
                remaining = remaining.Slice(consumed);
            }

            return noncharacter ? CustomPropertyTextScan.ContainsNoncharacter : CustomPropertyTextScan.WellFormed;
        }

        /// <summary>
        /// The 66 Unicode noncharacters: U+FDD0..U+FDEF, and every scalar whose low 16 bits are FFFE or FFFF, in any plane.
        /// </summary>
        internal static bool IsNoncharacter(int scalar)
            => (scalar >= 0xFDD0 && scalar <= 0xFDEF) || (scalar & 0xFFFE) == 0xFFFE;

        /// <summary>
        /// NFC of a name that is already ACCREDITED. False only if the platform normalizer still refuses it.
        ///
        /// <para>
        /// The <see cref="ArgumentException"/> catch is the agreed defence (C-F1), and nothing broader: the form is a
        /// constant and the argument is a non-null accredited string, so the only cause of that exception here is a code
        /// point a stricter platform normalizer rejects. With the precondition no executed case reaches it, on ICU or on NLS.
        /// </para>
        /// </summary>
        internal static bool TryNormalizeAccredited(string accreditedName, out string normalized)
        {
            try
            {
                normalized = accreditedName.Normalize(NormalizationForm.FormC);
                return true;
            }
            catch (ArgumentException)
            {
                normalized = null;
                return false;
            }
        }

        /// <summary>
        /// The form two accredited names are compared in (D-05.1, D-05.2): NFC, or the name itself when the defence of
        /// <see cref="TryNormalizeAccredited"/> fired. The comparison that uses it is <c>OrdinalIgnoreCase</c>.
        /// </summary>
        internal static string ComparableName(string accreditedName)
            => TryNormalizeAccredited(accreditedName, out var normalized) ? normalized : accreditedName;

        /// <summary>U+0000..U+001F or U+007F: the control characters a name never admits.</summary>
        internal static bool ContainsControlCharacter(string text)
        {
            foreach (var c in text)
            {
                if (IsControl(c))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The same set, except the three a value admits: tab, line feed and carriage return.</summary>
        internal static bool ContainsControlCharacterOtherThanLineBreakOrTab(string text)
        {
            foreach (var c in text)
            {
                if (IsControl(c) && c != (char)0x09 && c != (char)0x0A && c != (char)0x0D)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsControl(char c) => c < (char)0x20 || c == (char)0x7F;
    }
}
