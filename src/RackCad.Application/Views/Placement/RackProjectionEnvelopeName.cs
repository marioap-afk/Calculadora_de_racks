using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// The name a projected view carries in its envelope. <see cref="RackEmbedDocument.Name"/> is the CLIENT-FACING name of the
    /// logical rack and is distinct from the block definition BaseName (AUTH-11): a rack may legally have no name at all (the
    /// editors do not require one, and the list shows «(sin nombre)»). AUTH-15 (after I-52-AUTH15-C1) accepts an envelope whose
    /// Name is blank.
    ///
    /// <para>
    /// The logical name is the one the rack's views carry. When none carries one the rack stays UNNAMED: no synthetic name exists,
    /// so this type returns null and the projected view is composed with the blank name the rack already has (Owner decision:
    /// legacy unnamed racks are projectable and remain unnamed). ID19 adds a linked view of the SAME rack; it does not create a new
    /// identity (unlike RACKDUPLICAR), so it may not name one.
    /// </para>
    /// </summary>
    public static class RackProjectionEnvelopeName
    {
        /// <summary>The name of the logical rack the projected view belongs to, or null when no view of it carries one.</summary>
        public static string LogicalName(string ownName, IEnumerable<string> siblingNames)
        {
            if (!string.IsNullOrWhiteSpace(ownName))
            {
                return ownName.Trim();
            }

            var sibling = (siblingNames ?? Enumerable.Empty<string>()).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
            return string.IsNullOrWhiteSpace(sibling) ? null : sibling.Trim();
        }

        /// <summary>
        /// The source envelope as the projection composes from it: identical in everything except the rack's logical Name, taken
        /// from a sibling view when this view carries none. It is an in-memory copy for composition; the source envelope is never
        /// repaired in the drawing. A named source, and a source no view of which carries a name (an unnamed rack, which stays
        /// unnamed), are returned as they are.
        /// </summary>
        public static RackEmbedDocument WithLogicalName(RackEmbedDocument source, IEnumerable<string> siblingNames)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var name = LogicalName(source.Name, siblingNames);
            if (!string.IsNullOrWhiteSpace(source.Name) || name == null)
            {
                return source;
            }

            return RackEmbedComposer.Compose(
                source,
                source.Kind,
                source.Id,
                name,
                source.View,
                source.Section,
                source.Design);
        }
    }
}
