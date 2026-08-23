using RackCad.Domain.RackFrames;

namespace RackCad.Domain.Systems.Dynamic
{
    /// <summary>
    /// The cabecera configuration of ONE module on ONE physical LINE of the rack (I-40).
    ///
    /// <para>
    /// A rack has two different things that both get called «cabecera», and until I-40 the editor only had the first:
    /// </para>
    /// <list type="number">
    /// <item>the longitudinal MODULE — one entry of <see cref="DynamicRackSystem.Modules"/>, the depth-wise sequence
    /// cabecera/separador/cabecera shared by the whole rack;</item>
    /// <item>the physical LINE — the transverse post line at <c>postIndex</c>, the one the lateral view draws as a
    /// «corte». Every module of the rack materializes once on each line that covers it.</item>
    /// </list>
    ///
    /// <para>
    /// One <c>DynamicRackModuleDesign</c> therefore represents EVERY instance of that cabecera, on every line, and
    /// that is why the model could not express «esta linea distinta de aquella». This override is the minimum that
    /// can: a configuration addressed by <see cref="PostIndex"/> + <see cref="ModuleId"/>, which is exactly the pair
    /// <c>DynamicFrontGeometry.HeaderConfigurationAtPost</c> already receives — the single authority that decides
    /// which configuration a cabecera uses on a line, and which geometry, BOM and preview all consume.
    /// </para>
    ///
    /// <para>
    /// It is an OVERRIDE, never a second model: absent means "use the module's own configuration", which is what
    /// every rack drawn before I-40 does. Identity does not change: the module keeps its <c>ModuleId</c> and the rack
    /// its GUID.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderLineOverride
    {
        /// <summary>The physical line: the transverse post index, the same one the lateral cortes carry.</summary>
        public int PostIndex { get; set; }

        /// <summary>The longitudinal module this override applies to, by its stable id.</summary>
        public string ModuleId { get; set; }

        /// <summary>The configuration that line uses. Null is meaningless and is treated as "no override".</summary>
        public RackFrameConfiguration Header { get; set; }
    }
}
