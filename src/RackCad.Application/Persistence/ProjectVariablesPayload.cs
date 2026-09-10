namespace RackCad.Application.Persistence
{
    /// <summary>What the drawing physically had where the project-variable register lives.</summary>
    public enum ProjectVariablesPayloadState
    {
        /// <summary>The dictionary entry does not exist. A drawing with no register, which is valid legacy.</summary>
        Absent = 1,

        /// <summary>The entry exists and a payload could be extracted from it. Whether it MEANS anything is not this layer's call.</summary>
        Present = 2,

        /// <summary>
        /// The entry exists but nothing usable could be pulled out of it — an empty record, an unexpected
        /// object, no readable chunks. Present and unusable, which is never the same as absent.
        /// </summary>
        PresentButUnreadable = 3,
    }

    /// <summary>
    /// The PHYSICAL half of reading the register: what the storage found, with no opinion about what it means.
    ///
    /// <para>
    /// It exists so the layer that touches AutoCAD stays as small as the contract demands. The dictionary
    /// lookup and the chunk reassembly are the only things that genuinely need a drawing; deciding whether a
    /// register is usable — versions, unknown types, unknown definition kinds — is schema work, and schema
    /// work belongs where the Core suite can reach it.
    /// </para>
    /// <para>
    /// The distinction that must survive this boundary is <see cref="ProjectVariablesPayloadState.Absent"/>
    /// versus <see cref="ProjectVariablesPayloadState.PresentButUnreadable"/>. Collapsing them would let a
    /// present-but-corrupt register read as "no variables", and the next write would erase every variable in
    /// the drawing without anyone noticing.
    /// </para>
    /// </summary>
    public sealed class ProjectVariablesPayload
    {
        private ProjectVariablesPayload(ProjectVariablesPayloadState state, string json, string error)
        {
            State = state;
            Json = json;
            Error = error;
        }

        public ProjectVariablesPayloadState State { get; }

        /// <summary>The stored text. Only set when <see cref="State"/> is <see cref="ProjectVariablesPayloadState.Present"/>.</summary>
        public string Json { get; }

        /// <summary>Why nothing usable could be extracted. Only set for <see cref="ProjectVariablesPayloadState.PresentButUnreadable"/>.</summary>
        public string Error { get; }

        /// <summary>The drawing has no register entry.</summary>
        public static ProjectVariablesPayload Absent()
            => new ProjectVariablesPayload(ProjectVariablesPayloadState.Absent, null, null);

        /// <summary>The entry exists and this is what it held.</summary>
        public static ProjectVariablesPayload Present(string json)
            => new ProjectVariablesPayload(ProjectVariablesPayloadState.Present, json, null);

        /// <summary>The entry exists and nothing usable came out of it.</summary>
        public static ProjectVariablesPayload Unreadable(string error)
            => new ProjectVariablesPayload(ProjectVariablesPayloadState.PresentButUnreadable, null, error);
    }
}
