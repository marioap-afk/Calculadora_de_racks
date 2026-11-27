using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// A write of ONE Cantilever line from the editor of the window (T5 ct): the number of the request, the session it was made for, the line,
    /// its name (trimmed), COPIES of the design and of the line resolved from that copy, its JSON (the base of «Limpio» once written) and,
    /// as for Dinámico and Selectivo: the views to insert («Insertar vista…», RACKCANTILEVERINSERTAR: first the draft in the views that
    /// exist, then the new ones as linked copies), a new line of the window (<paramref name="NewRack"/>, RACKCANTILEVERNUEVO: inserted with
    /// those views, the identity the window gave it and, with no name, the automatic one of I-60) and the properties of the tab
    /// «Propiedades» (written by «Actualizar» only, VA-S10). Only an update when there are no views. T6 lib: <paramref name="SourceDesignJson"/>
    /// is the design of the library a new line was opened from, as the JSON of its <see cref="RackProject"/> (<see cref="RackLibrarySourceDesign"/>);
    /// in <see cref="CamaDoorWrite"/> the same member carries a <see cref="FlowBedDocument"/>.
    /// </summary>
    public sealed record CantileverDoorWrite(long RequestId, SessionId Session, string RackId, string Name, CantileverLineDesign Design,
        CantileverLineAssembly Line, string DesignJson, IReadOnlyList<RackViewAddress> InsertViews = null, bool NewRack = false,
        RackPropertiesWrite Properties = null, string SourceDesignJson = null) // T6 lib (UT-S10)
    {
        public bool Inserts => InsertViews != null && InsertViews.Count > 0;
    }

    /// <summary>
    /// What RACKCANTILEVERABRIR read for «Editar aquí» of a Cantilever line: whether it opens here, its kind, its name, the line as the project
    /// document of the block (<see cref="RackProjectStore"/>), the properties of the rack (the tab «Propiedades», VA-07) and, when it does
    /// not open, why.
    /// </summary>
    public sealed record CantileverDesignRead(long RequestId, string RackId, bool Opens, string Kind, string Name, string DesignJson, string Reason,
        RackPropertiesRead Properties = null)
    {
        public static CantileverDesignRead Refused(long requestId, string rackId, string kind, string name, string reason)
            => new CantileverDesignRead(requestId, rackId, false, kind, name, null, reason);

        /// <summary>The project read, as a new object (its unknown fields kept, I-11); null when it does not open or cannot be read.</summary>
        public RackProject Project()
        {
            if (!Opens || string.IsNullOrWhiteSpace(DesignJson)) return null;
            try
            {
                return new RackProjectStore().Deserialize(DesignJson);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return null;
            }
        }
    }

    /// <summary>The plan of RACKCANTILEVERAPLICAR and RACKCANTILEVERINSERTAR: it writes, with the project of the line as the drawing has it, or it does not, and why.</summary>
    public sealed record CantileverApplyPlan(bool CanWrite, RackProject Project, string Reason);

    /// <summary>
    /// The door of the window to a Cantilever line (T5 ct): the rules of the doors of Dinámico, Selectivo and Cabecera (numbered requests,
    /// another drawing, «Dejar de esperar», <see cref="IRackCadRackDoorPort.RequestStarted"/>). RACKCANTILEVERABRIR only reads (no undo marker,
    /// no history); RACKCANTILEVERAPLICAR redraws every view of the line in place with the drawing half of «Actualizar» of RACKEDITAR, so one
    /// «U» undoes it; RACKCANTILEVERINSERTAR is «Insertar» of RACKEDITAR (the draft in the views that exist, then the new views);
    /// RACKCANTILEVERNUEVO inserts a new line by the dispatch of «Nuevo». The answers of the writes are <see cref="RackDesignWritten"/>.
    /// </summary>
    public interface IRackCadCantileverDoorPort : IRackCadRackDoorPort
    {
        RackCadClassicResult RequestCantileverDesign(RackDoorRequest request);

        RackCadClassicResult RequestCantileverUpdate(CantileverDoorWrite write);

        event Action<CantileverDesignRead> CantileverRead;
    }

    /// <summary>The pure rules of the door of Cantilever, with their tests in Core; the commands only call them.</summary>
    public static class CantileverDoorRules
    {
        /// <summary>
        /// The answer of RACKCANTILEVERABRIR for the envelope of the first view of the line: it opens a readable Cantilever line, with the name
        /// of the envelope (as RACKEDITAR shows it: <c>LoadExisting(…, embed.Name, …)</c>) and its inner document as the drawing has it.
        /// </summary>
        public static CantileverDesignRead AnswerForOpen(long requestId, string rackId, string kind, string name, string designJson)
        {
            if (!string.Equals(kind, RackEmbedDocument.KindCantilever, StringComparison.OrdinalIgnoreCase))
                return CantileverDesignRead.Refused(requestId, rackId, kind, name, "El rack no es una línea Cantilever: este editor no lo abre.");

            RackProject project;
            try
            {
                project = new RackProjectStore().Deserialize(designJson);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return CantileverDesignRead.Refused(requestId, rackId, kind, name, "No se pudieron leer los datos de la línea Cantilever. " + ex.Message);
            }

            if (project?.CantileverLineDesign == null)
                return CantileverDesignRead.Refused(requestId, rackId, kind, name, "Datos de línea Cantilever inválidos.");
            return new CantileverDesignRead(requestId, rackId, true, kind, name, designJson, null);
        }

        /// <summary>
        /// The plan of a write on a line of the drawing: the window must send a resolved line with its design, and the rack must still be a
        /// readable Cantilever line, as RACKEDITAR checks before its window opens.
        /// </summary>
        public static CantileverApplyPlan PlanApply(string kind, string designJson, CantileverDoorWrite write)
        {
            const string NotReadable = "El rack del dibujo no es una línea Cantilever legible: no se escribió nada.";
            if (write?.Design == null || write.Line == null || write.Line.IsBlocked)
                return new CantileverApplyPlan(false, null, "No hay una línea resuelta que escribir.");
            if (!string.Equals(kind, RackEmbedDocument.KindCantilever, StringComparison.OrdinalIgnoreCase)) return new CantileverApplyPlan(false, null, NotReadable);
            try
            {
                var project = new RackProjectStore().Deserialize(designJson);
                return project?.CantileverLineDesign == null ? new CantileverApplyPlan(false, null, NotReadable) : new CantileverApplyPlan(true, project, null);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return new CantileverApplyPlan(false, null, NotReadable);
            }
        }

        /// <summary>
        /// Why the door refuses a write before handing its command over, or null: a line that does not resolve; an insertion without views; a
        /// new line without an identity.
        /// </summary>
        public static string RefusalOf(CantileverDoorWrite write)
        {
            if (write == null || string.IsNullOrWhiteSpace(write.RackId) || write.Design == null || write.Line == null || write.Line.IsBlocked)
                return "No hay una línea resuelta que escribir.";
            if (write.NewRack && !write.Inserts) return "Elige al menos una vista para insertar la línea.";
            return null;
        }

        /// <summary>The station of a lateral the line no longer has is not drawn (it would show another station under its name).</summary>
        public static IReadOnlyList<RackViewAddress> DrawableViews(IReadOnlyList<RackViewAddress> views, CantileverLineAssembly line)
        {
            var drawable = new List<RackViewAddress>();
            if (views == null || line == null) return drawable;
            var facts = new CantileverViewAvailabilityFacts(line.Stations.Count);
            foreach (var view in views)
            {

                var syntax = RackViewCodec.Encode(Domain.Systems.Shared.RackSystemKind.Cantilever, view);
                var decoded = RackViewCodec.Decode(syntax.Kind, syntax.View, syntax.Section);
                if (decoded.HasAddress && RackViewAvailability.Evaluate(decoded, facts).Status == RackViewAvailabilityStatus.Available) drawable.Add(view);
            }

            return drawable;
        }
    }
}
