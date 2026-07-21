using System;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The default set of system descriptors, wired with the Application persistence operations RackProjectStore
    /// delegates to. Kept in this companion file so the generic registry mechanics stay free of persistence detail. Each
    /// operation maps <see cref="RackProjectDocument"/> to/from <see cref="RackProject"/> and reproduces the pre-registry
    /// behavior verbatim (frozen by the F1 characterization): same write chain, same build + member reconstruction, same
    /// per-kind "usable" predicates, same messages. The standalone cabecera (Selective) uses the store's shared header
    /// fallback, so its payload writer defers (returns false).
    /// </summary>
    public sealed partial class SystemRegistry
    {
        /// <summary>
        /// The registry for the five system kinds that exist today, in <see cref="RackSystemKind"/> declaration order,
        /// with the labels verbatim and the persistence operations the store delegates to.
        /// </summary>
        public static SystemRegistry Default { get; } = new SystemRegistry(new[]
        {
            new SystemDescriptor(RackSystemKind.Selective, "Cabecera", "la cabecera",
                DeferToHeaderFallback, BuildSelectiveHeader, IsUsableSelectiveHeader),
            new SystemDescriptor(RackSystemKind.PalletFlow, "Sistema dinámico", "el sistema dinámico",
                WriteDynamic, BuildDynamic, IsUsableDynamic),
            new SystemDescriptor(RackSystemKind.SelectiveRack, "Selectivo", "el rack selectivo",
                WriteSelectiveRack, BuildSelectiveRack, IsUsableSelectiveRack),
            new SystemDescriptor(RackSystemKind.Cama, "Cama de rodamiento", "la cama",
                WriteCama, BuildCama, IsUsableCama),
            new SystemDescriptor(RackSystemKind.Larguero, "Larguero", "el larguero",
                WriteLarguero, BuildLarguero, IsUsableLarguero),
        });

        // --- Selective (standalone cabecera) ---
        // The header is written by the store's shared fallback (re-stamping Kind = Selective), so this kind's payload
        // writer defers. Build/validate use the header logic; the same Build also serves an unregistered/undefined Kind.
        private static bool DeferToHeaderFallback(RackProject project, RackProjectDocument document) => false;

        private static RackProject BuildSelectiveHeader(RackProjectDocument document, BracingPanelMemberBuilder builder)
        {
            var header = document.Header?.ToConfiguration();
            if (header != null)
            {
                builder.RefreshPhysicalModel(header);
            }

            return RackProject.ForSelective(header);
        }

        private static bool IsUsableSelectiveHeader(RackProject project)
            => RackDesignValidation.IsUsableHeader(project.Header);

        // --- Pallet flow (dynamic) ---
        private static bool WriteDynamic(RackProject project, RackProjectDocument document)
        {
            if (project.DynamicDesign == null && project.DynamicSystem == null)
            {
                return false;
            }

            document.DynamicSystem = project.DynamicDesign != null
                ? DynamicRackSystemDocument.From(project.DynamicDesign)
                : DynamicRackSystemDocument.From(project.DynamicSystem);
            return true;
        }

        private static RackProject BuildDynamic(RackProjectDocument document, BracingPanelMemberBuilder builder)
        {
            RequirePayload(document.DynamicSystem, "sistema dinámico");
            var design = document.DynamicSystem.ToDesign();
            var system = document.DynamicSystem.ToDomain();
            foreach (var module in system.Modules)
            {
                if (module.AssociatedFrameConfiguration != null)
                {
                    builder.RefreshPhysicalModel(module.AssociatedFrameConfiguration);
                }
            }

            return RackProject.ForDynamic(design, system);
        }

        private static bool IsUsableDynamic(RackProject project)
            => RackDesignValidation.IsUsableDynamic(project.DynamicDesign, project.DynamicSystem);

        // --- Selective rack (full pallet rack) ---
        private static bool WriteSelectiveRack(RackProject project, RackProjectDocument document)
        {
            if (project.SelectiveRack == null)
            {
                return false;
            }

            document.SelectiveRack = project.SelectiveRack;
            return true;
        }

        private static RackProject BuildSelectiveRack(RackProjectDocument document, BracingPanelMemberBuilder builder)
        {
            RequirePayload(document.SelectiveRack, "rack selectivo");
            return RackProject.ForSelectiveRack(document.SelectiveRack);
        }

        private static bool IsUsableSelectiveRack(RackProject project)
            => RackDesignValidation.IsUsableSelective(project.SelectiveRack);

        // --- Cama (flow bed) ---
        private static bool WriteCama(RackProject project, RackProjectDocument document)
        {
            if (project.FlowBed == null)
            {
                return false;
            }

            document.FlowBed = FlowBedDocument.FromDomain(project.FlowBed);
            return true;
        }

        private static RackProject BuildCama(RackProjectDocument document, BracingPanelMemberBuilder builder)
        {
            RequirePayload(document.FlowBed, "cama");
            SchemaGuard.CheckReadable(document.FlowBed.SchemaVersion, FlowBedDocument.CurrentSchemaVersion, "La cama");
            return RackProject.ForCama(document.FlowBed.ToDomain());
        }

        private static bool IsUsableCama(RackProject project)
            => RackDesignValidation.IsUsableFlowBed(project.FlowBed);

        // --- Larguero ---
        private static bool WriteLarguero(RackProject project, RackProjectDocument document)
        {
            if (project.Larguero == null)
            {
                return false;
            }

            document.Larguero = LargueroDocument.FromDomain(project.Larguero);
            return true;
        }

        private static RackProject BuildLarguero(RackProjectDocument document, BracingPanelMemberBuilder builder)
        {
            RequirePayload(document.Larguero, "larguero");
            SchemaGuard.CheckReadable(document.Larguero.SchemaVersion, LargueroDocument.CurrentSchemaVersion, "El larguero");
            return RackProject.ForLarguero(document.Larguero.ToDomain());
        }

        private static bool IsUsableLarguero(RackProject project)
            => RackDesignValidation.IsUsableLarguero(project.Larguero);

        // A wrapper that names a type but omits its payload is corrupt/truncated — fail clearly, with the same message the
        // store used before the migration. Strongly typed (no object) to honor the no-object/no-unchecked-cast rule.
        private static void RequirePayload<T>(T payload, string typeName)
            where T : class
        {
            if (payload == null)
            {
                throw new InvalidOperationException("El proyecto declara tipo '" + typeName + "' pero no contiene sus datos.");
            }
        }
    }
}
