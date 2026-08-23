namespace RackCad.Domain.Systems.Dynamic
{
    /// <summary>
    /// The height of the DERIVED POSTS of ONE physical line (I-40).
    ///
    /// <para>
    /// A derived post is born between two consecutive separators, so it belongs to the LINE and NOT to any module:
    /// addressing it by <c>ModuleId</c> would be inventing an ownership the geometry does not have. Hence its own
    /// per-line override, sibling of <see cref="DynamicHeaderLineOverride"/> but keyed only by
    /// <see cref="PostIndex"/>.
    /// </para>
    ///
    /// <para>
    /// Absent means the line uses the rack-wide <c>DerivedPostHeight</c>, and that in turn means "inherit the
    /// cabecera height" when it is null — which is what every rack drawn before I-40 does.
    /// </para>
    /// </summary>
    public sealed class DynamicDerivedPostLineOverride
    {
        /// <summary>The physical line: the transverse post index.</summary>
        public int PostIndex { get; set; }

        /// <summary>The LONGITUD its derived posts take. Non-positive is meaningless and treated as absent.</summary>
        public double Height { get; set; }
    }
}
