using System;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// The outcome of re-stamping a copy's inner design identity (I-47 G14).
    ///
    /// <para>
    /// It exists because the previous shape could not express failure. The helper returned a string, so a
    /// design it could not re-stamp came back as the ORIGINAL string — and the copy was then written with a
    /// fresh RackId on the envelope and the source's identity inside it. Two racks with mixed semantic
    /// identity, and the defect only becomes visible the first time someone opens the copy and saves, by
    /// which point there is no way to tell which was which.
    /// </para>
    /// <para>
    /// So the failure is a value now, and the caller has to look at it BEFORE it materialises anything.
    /// <see cref="DesignJson"/> is null on failure: there is no "the best we could do".
    /// </para>
    /// </summary>
    public sealed class RestampResult
    {
        private RestampResult(bool success, string designJson, string error)
        {
            IsSuccess = success;
            DesignJson = designJson;
            Error = error;
        }

        public bool IsSuccess { get; }

        /// <summary>The re-stamped design. NULL on failure — never the original.</summary>
        public string DesignJson { get; }

        /// <summary>The visible reason. Null on success.</summary>
        public string Error { get; }

        public static RestampResult Success(string designJson) => new RestampResult(true, designJson, null);

        public static RestampResult Failure(string error) => new RestampResult(false, null, error);
    }

    /// <summary>
    /// Re-stamps the identity a Selective design carries, purely (I-47 G14).
    ///
    /// <para>
    /// The whole transformation is: read the persisted document, give it the copy's identity, write it back.
    /// Everything else must survive untouched — the frozen authored literal, the binding, the schema version
    /// and any field a later build wrote. That is precisely why it round-trips the DOCUMENT rather than the
    /// domain design: the domain cannot carry any of those, so a trip through it would drop them silently.
    /// </para>
    /// <para>
    /// The binding survives ON PURPOSE, and it is the one thing worth saying out loud: a copy is another
    /// RACK, but the project variable governing its clearance is still the same variable of the same drawing.
    /// The rack's identity changes; the variable's does not.
    /// </para>
    /// </summary>
    public static class SelectiveAuthoredRestamp
    {
        public static RestampResult Restamp(string designJson, string newId, string copyName)
        {
            if (string.IsNullOrWhiteSpace(designJson))
            {
                return RestampResult.Failure(
                    "El rack de origen no lleva diseño selectivo que copiar, así que no se puede re-estampar.");
            }

            var store = new SelectivePalletDesignStore();

            try
            {
                var document = store.Deserialize(designJson);
                document.Id = newId;
                document.Name = copyName;
                return RestampResult.Success(store.Serialize(document));
            }
            catch (InvalidOperationException ex)
            {
                // The store SIGNALS by throwing — invalid JSON, a MAJOR above what this build reads, a design
                // with no fronts. All of them mean the same here: this copy cannot be produced.
                return RestampResult.Failure(ex.Message);
            }
        }
    }
}
