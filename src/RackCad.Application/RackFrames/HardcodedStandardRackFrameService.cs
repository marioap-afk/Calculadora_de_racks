using RackCad.Application.Catalogs;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Produces the temporary standard frame used as the plugin's initial configuration.
    /// Delegates to <see cref="RackFrameConfigurationFactory"/> with the STD-3P template so the
    /// build logic lives in one place; only the standard's identity (name/baseline) is overridden.
    /// </summary>
    public sealed class HardcodedStandardRackFrameService
    {
        private readonly RackFrameConfigurationFactory factory;

        public HardcodedStandardRackFrameService()
        {
            factory = new RackFrameConfigurationFactory();
        }

        public HardcodedStandardRackFrameService(RackCatalog catalog)
        {
            factory = new RackFrameConfigurationFactory(catalog);
        }

        public RackFrameConfiguration CreateDefault()
        {
            var template = RackFrameTemplateCatalog.FindById("STD-3P") ?? RackFrameTemplateCatalog.Default;
            var configuration = factory.Build(template, CatalogIds.StandardPost, 132.0, 42.0);

            // Preserve the standard's stable identity regardless of the template's own metadata.
            configuration.Name = "Cabecera estandar temporal";
            configuration.StandardBaselineId = "STD-CABECERA-TEMP-001";
            configuration.StandardBaselineVersion = "0.1";

            return configuration;
        }
    }
}
