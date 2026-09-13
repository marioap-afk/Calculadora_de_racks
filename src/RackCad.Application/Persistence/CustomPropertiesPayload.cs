namespace RackCad.Application.Persistence
{
    /// <summary>What the drawing physically had where the Project collection of custom properties lives.</summary>
    public enum CustomPropertiesPayloadState
    {
        /// <summary>The dictionary entry does not exist: a drawing with no project properties.</summary>
        Absent = 1,

        /// <summary>The entry exists and text could be extracted from it. Whether it MEANS anything is not this layer's call.</summary>
        Present = 2,

        /// <summary>The entry exists but nothing usable could be pulled out of it. Present and unusable, never the same as absent.</summary>
        PresentButUnreadable = 3,
    }

    /// <summary>
    /// The PHYSICAL half of reading the Project collection (I-54 D-07.2): what the storage found, with no opinion about
    /// what it means. The layer that touches AutoCAD produces it; every judgement is <see cref="CustomPropertiesStore"/>'s.
    ///
    /// <para>
    /// The distinction that must survive this boundary is <see cref="CustomPropertiesPayloadState.Absent"/> versus
    /// <see cref="CustomPropertiesPayloadState.PresentButUnreadable"/>: collapsing them would let a corrupt entry read as
    /// "no properties", and the next write would replace it.
    /// </para>
    /// </summary>
    public sealed class CustomPropertiesPayload
    {
        private CustomPropertiesPayload(CustomPropertiesPayloadState state, string text, string error)
        {
            State = state;
            Text = text;
            Error = error;
        }

        public CustomPropertiesPayloadState State { get; }

        /// <summary>The stored text. Only set for <see cref="CustomPropertiesPayloadState.Present"/>.</summary>
        public string Text { get; }

        /// <summary>Why nothing usable could be extracted. Only set for <see cref="CustomPropertiesPayloadState.PresentButUnreadable"/>.</summary>
        public string Error { get; }

        public static CustomPropertiesPayload Absent()
            => new CustomPropertiesPayload(CustomPropertiesPayloadState.Absent, null, null);

        public static CustomPropertiesPayload Present(string text)
            => new CustomPropertiesPayload(CustomPropertiesPayloadState.Present, text, null);

        public static CustomPropertiesPayload Unreadable(string error)
            => new CustomPropertiesPayload(CustomPropertiesPayloadState.PresentButUnreadable, null, error);
    }
}
