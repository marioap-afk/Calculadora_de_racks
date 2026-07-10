using RackCad.Application.Catalogs;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Produces the standard frame used as the plugin's initial configuration. Delegates to
    /// <see cref="RackFrameConfigurationFactory"/> with the standard template so the build logic lives in one place;
    /// the standard's identity (name + baseline id/version) is taken from that template — no hardcoded placeholder —
    /// so it tracks whatever the catalog defines as the standard.
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
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();
            // Post/height/depth come from the template (and its defaults), not literals.
            var configuration = factory.Build(template, template.Post, template.DefaultHeight, template.DefaultDepth);

            // Identity comes from the standard template itself (stable id/name), not a hardcoded placeholder.
            configuration.Name = string.IsNullOrWhiteSpace(template.Name) ? "Cabecera estandar" : template.Name;
            configuration.StandardBaselineId = string.IsNullOrWhiteSpace(template.Id) ? "STD-CABECERA" : template.Id;
            configuration.StandardBaselineVersion = "1.0";

            return configuration;
        }
    }
}
