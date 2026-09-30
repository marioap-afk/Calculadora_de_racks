using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// The name a projected view carries in its envelope. <see cref="RackEmbedDocument.Name"/> is the CLIENT-FACING name of the
    /// logical rack and is distinct from the block definition BaseName (AUTH-11): a rack may legally have no name at all (the
    /// editors do not require one, and the list shows «(sin nombre)»), while AUTH-15 refuses to write an envelope without one.
    ///
    /// <para>
    /// The rule is the one the product already has for a nameless logical rack: the name is the one the rack's views carry, and
    /// when none carries one, the fallback of <see cref="RackDuplicationPlan"/> («Rack»), the only other place that writes a name
    /// into the envelope of a nameless rack. No G15-only fallback exists.
    /// </para>
    /// </summary>
    public static class RackProjectionEnvelopeName
    {
        /// <summary>The name of the logical rack the projected view belongs to.</summary>
        public static string LogicalName(string ownName, IEnumerable<string> siblingNames)
        {
            if (!string.IsNullOrWhiteSpace(ownName))
            {
                return ownName.Trim();
            }

            var sibling = (siblingNames ?? Enumerable.Empty<string>()).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
            return string.IsNullOrWhiteSpace(sibling) ? RackDuplicationPlan.FallbackName : sibling.Trim();
        }

        /// <summary>
        /// The source envelope as the projection composes from it: identical in everything except a usable Name. It is an in-memory
        /// copy for composition; the source envelope is never repaired in the drawing. A named source is returned as is.
        /// </summary>
        public static RackEmbedDocument WithLogicalName(RackEmbedDocument source, IEnumerable<string> siblingNames)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!string.IsNullOrWhiteSpace(source.Name))
            {
                return source;
            }

            return RackEmbedComposer.Compose(
                source,
                source.Kind,
                source.Id,
                LogicalName(source.Name, siblingNames),
                source.View,
                source.Section,
                source.Design);
        }
    }
}
