using System;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// The rack identity (stable GUID + client name) an editor carries for the drawing round-trip, centralizing the
    /// idiom that selective, dynamic and cama each inline today (initiative I-15, E3):
    /// <code>if (string.IsNullOrWhiteSpace(currentId)) currentId = Guid.NewGuid().ToString();</code>
    /// The id is minted lazily on the FIRST insert and then kept stable across re-inserts/edits (so every view-block of
    /// a rack shares it); a design opened from the drawing or the library <see cref="Adopt"/>s its existing id. Pure: no
    /// WPF, no AutoCAD. The GUID factory is injectable so tests are deterministic; production uses <see cref="Guid.NewGuid"/>.
    /// </summary>
    public sealed class RackEditorIdentity
    {
        private readonly Func<string> newId;

        /// <summary>Creates an identity with no id yet (a brand-new design). <paramref name="newIdFactory"/> defaults to
        /// <c>Guid.NewGuid().ToString()</c> — the exact form the editors mint today.</summary>
        public RackEditorIdentity(Func<string> newIdFactory = null)
        {
            newId = newIdFactory ?? (() => Guid.NewGuid().ToString());
        }

        /// <summary>The stable id, or null until <see cref="EnsureId"/> mints one (or <see cref="Adopt"/> supplies it).</summary>
        public string Id { get; private set; }

        /// <summary>The client-facing rack name (may be null/empty; the name is not stable identity — the GUID is).</summary>
        public string Name { get; private set; }

        /// <summary>True once an id exists (minted or adopted).</summary>
        public bool HasId => !string.IsNullOrWhiteSpace(Id);

        /// <summary>Stores the display name verbatim. Trimming stays at the call site, exactly as the editors do it
        /// (raw <c>currentName = name</c> on load; <c>NameBox.Text?.Trim()</c> on insert) — so adopting this helper does
        /// not change what name reaches the drawing.</summary>
        public void SetName(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Mints a GUID on the first call and returns the (now stable) id; subsequent calls keep the same id. This is the
        /// lazy "mint on first insert" the editors do, so re-inserting another view of the same rack reuses the id.
        /// </summary>
        public string EnsureId()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                Id = newId();
            }

            return Id;
        }

        /// <summary>
        /// Adopts the identity of a design opened from the drawing or the library (RACKEDITAR / open-from-library), so a
        /// re-save/redraw keeps the same GUID. A blank <paramref name="id"/> leaves the id unminted (a library template
        /// opened "as new" gets a fresh GUID on insert, matching <c>LoadForNew</c>). The name is stored verbatim, exactly
        /// like the editors' <c>currentName = name</c> on load.
        /// </summary>
        public void Adopt(string id, string name)
        {
            Id = string.IsNullOrWhiteSpace(id) ? null : id;
            Name = name;
        }
    }
}
