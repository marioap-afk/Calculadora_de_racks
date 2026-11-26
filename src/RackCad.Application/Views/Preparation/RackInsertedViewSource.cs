using RackCad.Application.Persistence;

namespace RackCad.Application.Views.Preparation
{
    /// <summary>
    /// B-1 (I-64 A-14, line D): the source from which a NEW linked view of a rack of the drawing is prepared when the same edit first
    /// redraws the views that exist with an edited design (RACKEDITAR «Insertar» with changes in its window, «Insertar vista…» of the
    /// RackCad window). <see cref="RackProductPreparer{TAuthored,TResolved,TPayload}.PrepareExisting"/> composes the envelope of the new
    /// view from the identity, the name and the design of its source, while its geometry comes from the resolved (edited) system. The view
    /// that was picked, as the drawing held it, carries the design BEFORE the edit, so the new view kept that design while its siblings
    /// received the edited one. The source is therefore the picked envelope (its view, section, custom properties and extension data,
    /// I-11) with the identity, the name and the serialized design that the edit writes in the views that exist. A new rack has no source:
    /// null stays null.
    /// </summary>
    public static class RackInsertedViewSource
    {
        /// <summary>
        /// The picked <paramref name="source"/> with <paramref name="rackId"/>, <paramref name="rackName"/> and
        /// <paramref name="serializedDesign"/>, the three values the redraw writes in every existing view; null when there is no source.
        /// It is an in-memory copy for the preparation; the envelope of the drawing is not changed here.
        /// </summary>
        public static RackEmbedDocument Edited(RackEmbedDocument source, string rackId, string rackName, string serializedDesign)
        {
            if (source == null)
            {
                return null;
            }

            return RackEmbedComposer.Compose(
                source,
                source.Kind,
                rackId,
                rackName,
                source.View,
                source.Section,
                serializedDesign);
        }
    }
}
