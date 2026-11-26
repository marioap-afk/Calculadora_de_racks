using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Workspace;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>A read of the page «Variables» (cuarta tanda, va): its number and the drawing it is for.</summary>
    public sealed record VariablesDoorRequest(long RequestId, SessionId Session);

    /// <summary>
    /// What RACKVARIABLESLEER read for the page «Variables»: the register, the sweep of the racks that RACKVARIABLES reads (in one
    /// transaction, without an undo mark) and the custom properties of the project. Plain values: the page projects them purely.
    /// </summary>
    public sealed record VariablesDoorRead(
        long RequestId,
        bool Read,
        ProjectVariablesReadResult Registry,
        IReadOnlyList<ProjectVariableScanEntry> Entries,
        CustomPropertiesReadResult ProjectProperties,
        string Reason)
    {
        public static VariablesDoorRead Refused(long requestId, string reason)
            => new VariablesDoorRead(requestId, false, null, Array.Empty<ProjectVariableScanEntry>(), null, reason);

        /// <summary>The workspace RACKVARIABLES shows, from this same read.</summary>
        public ProjectVariablesWorkspace Workspace() => ProjectVariablesWorkspace.Build(Registry, Entries ?? Array.Empty<ProjectVariableScanEntry>());
    }

    /// <summary>
    /// «Actualizar» of the page «Variables» (VA-02): the whole draft, and what it was read over. <see cref="ProjectProperties"/> is the
    /// draft of the custom properties of the project (VA-06), or null when it does not change them.
    /// </summary>
    public sealed record VariablesDoorWrite(
        long RequestId,
        SessionId Session,
        string RegistryFingerprint,
        ProjectVariablesBatchRequest Batch,
        string PropertiesFingerprint,
        IReadOnlyList<CustomPropertyDraftEntry> ProjectProperties);

    /// <summary>The answer of RACKVARIABLESAPLICAR: whether it wrote, what to tell, and the racks it redrew.</summary>
    public sealed record VariablesDoorWritten(long RequestId, bool Written, string Message, IReadOnlyList<string> RedrawnRackIds = null, int ViewsRedrawn = 0,
        string PropertiesError = null);

    /// <summary>
    /// The door of the page «Variables» to the drawing (cuarta tanda, va). The same rules as the door of a rack: a number per request,
    /// the drawing it was made for, «Dejar de esperar» (<see cref="IRackCadRackDoorPort.Abandon"/>) and <see cref="IRackCadRackDoorPort.RequestStarted"/>
    /// when the write command took it. A window gets it only when its door to the racks also implements it.
    /// </summary>
    public interface IRackCadVariablesDoorPort : IRackCadRackDoorPort
    {
        RackCadClassicResult RequestVariables(VariablesDoorRequest request);

        RackCadClassicResult RequestVariablesUpdate(VariablesDoorWrite write);

        event Action<VariablesDoorRead> VariablesRead;

        event Action<VariablesDoorWritten> VariablesWritten;
    }

    /// <summary>What RACKVARIABLESAPLICAR may write, decided purely on the fresh read: the plan of the variables and the project collection.</summary>
    public sealed record VariablesWritePlan(
        bool CanWrite,
        MutationPlan Variables,
        CustomPropertiesDocument ProjectProperties,
        IReadOnlyList<string> RedrawnRackIds,
        string Reason)
    {
        public static VariablesWritePlan Refused(string reason)
            => new VariablesWritePlan(false, MutationPlan.Empty, null, Array.Empty<string>(), reason);
    }

    /// <summary>The pure rules of the door of «Variables» (cuarta tanda, va), with their tests in Core.</summary>
    public static class VariablesDoorRules
    {
        public const string ChangedSinceRead =
            " cambió en el dibujo desde que se leyó la página (otra orden o un «U»): no se escribió nada. Descarta el borrador y vuelve a leer la página.";

        /// <summary>What the page read of the register, to compare with what the command reads again: its outcome and its persisted shape.</summary>
        public static string RegistryFingerprint(ProjectVariablesReadResult registry)
            => registry == null ? "none" : registry.Outcome + ":" + (ProjectVariableCloning.Canonical(registry.Document) ?? string.Empty);

        /// <summary>The same, for the custom properties of the project.</summary>
        public static string PropertiesFingerprint(CustomPropertiesReadResult properties) => CustomPropertiesBatch.Fingerprint(properties);

        /// <summary>
        /// Plans «Actualizar» of the page on the read of NOW: the register and the project collection must be the ones the page read, the
        /// batch must pass on the fresh sweep, and the project draft must apply. All or nothing: any refusal writes nothing.
        /// </summary>
        public static VariablesWritePlan PlanWrite(
            ProjectVariablesReadResult registry,
            IReadOnlyList<ProjectVariableScanEntry> entries,
            CustomPropertiesReadResult projectProperties,
            VariablesDoorWrite write)
        {
            if (write == null) return VariablesWritePlan.Refused("No hay un borrador que escribir.");
            if (!string.Equals(RegistryFingerprint(registry), write.RegistryFingerprint, StringComparison.Ordinal))
                return VariablesWritePlan.Refused("El registro de variables" + ChangedSinceRead);

            CustomPropertiesDocument properties = null;
            if (write.ProjectProperties != null)
            {
                if (!string.Equals(PropertiesFingerprint(projectProperties), write.PropertiesFingerprint, StringComparison.Ordinal))
                    return VariablesWritePlan.Refused("Las propiedades del proyecto" + ChangedSinceRead.Replace("cambió", "cambiaron"));
                var applied = CustomPropertiesBatch.Apply(projectProperties, write.ProjectProperties);
                if (!applied.Succeeded) return VariablesWritePlan.Refused("Propiedades del proyecto: " + applied.Error + " No se escribió nada.");
                properties = applied.Document;
            }

            var batch = ProjectVariablesBatch.Plan(registry, entries, write.Batch);
            if (!batch.IsSuccess)
                return VariablesWritePlan.Refused(batch.Errors[0].Text + " No se escribió nada.");
            if (batch.Plan.IsEmpty && properties == null) return VariablesWritePlan.Refused("El borrador no cambia nada del dibujo.");
            return new VariablesWritePlan(true, batch.Plan, properties, batch.RedrawnRackIds, null);
        }

        /// <summary>The answer to a write: the racks it redrew, by number, and that one «U» undoes it all.</summary>
        public static VariablesDoorWritten AnswerForWrite(long requestId, bool variablesWritten, IReadOnlyList<string> redrawnRacks, int viewsRedrawn,
            bool propertiesWritten, string propertiesError)
        {
            var racks = redrawnRacks ?? Array.Empty<string>();
            if (propertiesError != null)
            {
                return new VariablesDoorWritten(requestId, variablesWritten, variablesWritten
                    ? "AutoCAD escribió las variables, pero no las propiedades del proyecto: " + propertiesError + " Las propiedades siguen sin escribir."
                    : "No se escribió nada: " + propertiesError, variablesWritten ? racks : Array.Empty<string>(), viewsRedrawn, propertiesError);
            }

            return new VariablesDoorWritten(requestId, variablesWritten || propertiesWritten,
                "AutoCAD escribió el borrador de «Variables»" + (racks.Count == 0
                    ? " sin redibujar ningún rack"
                    : " y redibujó " + racks.Count + (racks.Count == 1 ? " rack" : " racks") + " (" + viewsRedrawn + (viewsRedrawn == 1 ? " vista" : " vistas") + ")")
                + ". Un «U» en AutoCAD lo deshace todo.", racks, viewsRedrawn);
        }
    }
}
