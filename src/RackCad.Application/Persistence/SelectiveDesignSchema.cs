using System;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// The STICKY schema promotion of a selective design (I-47 D-07, C4-9). Sticky means MONOTONE: the
    /// version goes up and never comes back down.
    ///
    /// <para>
    /// The alternative — deciding the major from what the document contains right now — makes it oscillate
    /// <c>1.0, 2.0, 1.0</c> as the user binds and unbinds. Three consequences follow and none of them is
    /// good: "which versions can open this rack" would depend on transient state and be impossible to
    /// explain; the drawing as a whole has already been touched by the new semantics, so claiming
    /// compatibility again would be false; and a down-and-up cycle is a classic source of subtle loss.
    /// </para>
    /// <para>
    /// The cost is real and accepted rather than hidden: a rack bound ONCE stays out of reach of earlier
    /// builds forever, even after it is unbound.
    /// </para>
    /// <para>
    /// The invariant this function guarantees BY ITSELF — not by relying on a read guard running first — is
    /// that the result is never of a LOWER major than what was stored. That is why the higher-major branch
    /// throws instead of quietly returning the current version, which is what the general-purpose
    /// <see cref="SchemaVersionPolicy.ResolveWriteVersion"/> does defensively for documents that have a read
    /// guard in front of them.
    /// </para>
    /// </summary>
    public static class SelectiveDesignSchema
    {
        /// <summary>
        /// The version to stamp when writing a selective design.
        ///
        /// <para><paramref name="hasPropertyValues"/> is evaluated on the authored document ABOUT TO BE
        /// WRITTEN, not the one that was read: binding and saving in the same step must promote.</para>
        ///
        /// <list type="number">
        /// <item>stored MAJOR above what this build reads =&gt; ERROR. Not written, not downgraded, not opened.</item>
        /// <item>stored on the promoted line =&gt; stays there, keeping the higher minor, bound or not.</item>
        /// <item>legacy line WITH bindings =&gt; promotes.</item>
        /// <item>legacy line without bindings =&gt; stays on the legacy line, keeping the higher minor.</item>
        /// </list>
        /// </summary>
        public static string ResolveWriteVersion(string storedVersion, bool hasPropertyValues)
        {
            var storedMajor = SchemaVersionPolicy.MajorOf(storedVersion);

            if (storedMajor > SelectivePalletDesignDocument.SupportedReadMajor)
            {
                throw new InvalidOperationException(
                    "El diseño del selectivo fue creado con una versión más nueva de RackCad (esquema " +
                    storedVersion + "); no se sobrescribe ni se degrada.");
            }

            if (storedMajor == SelectivePalletDesignDocument.SupportedReadMajor)
            {
                return SchemaVersionPolicy.ResolveWriteVersion(
                    storedVersion,
                    SelectivePalletDesignDocument.PromotedSchemaVersion);
            }

            return hasPropertyValues
                ? SelectivePalletDesignDocument.PromotedSchemaVersion
                : SchemaVersionPolicy.ResolveWriteVersion(
                    storedVersion,
                    SelectivePalletDesignDocument.CurrentSchemaVersion);
        }
    }
}
