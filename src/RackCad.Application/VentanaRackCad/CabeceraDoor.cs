using System;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// A write of a cabecera of its own (a rack «Cabecera N» of the list) from the editor of cabeceras of the window (T4 cb, CB-01 A): the
    /// number of the request, the session it was made for, the rack, its name and a COPY of the cabecera. The answer is the
    /// <see cref="RackDesignWritten"/> of the door, as for Dinámico and Selectivo.
    /// </summary>
    public sealed record CabeceraDoorWrite(long RequestId, SessionId Session, string RackId, string Name, RackFrameConfiguration Cabecera);

    /// <summary>
    /// What RACKCABECERAABRIR read for «Editar aquí» of a cabecera: whether it opens here, its kind and name, the cabecera as its project
    /// document (<see cref="RackFrameProjectStore"/>), and, when it does not open, why.
    /// </summary>
    public sealed record CabeceraDesignRead(long RequestId, string RackId, bool Opens, string Kind, string Name, string HeaderJson, string Reason)
    {
        public static CabeceraDesignRead Refused(long requestId, string rackId, string kind, string name, string reason)
            => new CabeceraDesignRead(requestId, rackId, false, kind, name, null, reason);

        /// <summary>The cabecera read, as a new object; null when it does not open or cannot be read.</summary>
        public RackFrameConfiguration Header()
        {
            if (!Opens || string.IsNullOrWhiteSpace(HeaderJson)) return null;
            try
            {
                return new RackFrameProjectStore().Deserialize(HeaderJson);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }

    /// <summary>The plan of RACKCABECERAAPLICAR: it writes, with the project of the rack as the drawing has it, or it does not, and why.</summary>
    public sealed record CabeceraApplyPlan(bool CanWrite, RackProject Project, string Reason);

    /// <summary>
    /// The door of the window to a cabecera of its own (T4 cb): the same rules as the doors of Dinámico and Selectivo (numbered requests,
    /// another drawing, «Dejar de esperar», <see cref="IRackCadRackDoorPort.RequestStarted"/>). RACKCABECERAABRIR only reads (no undo marker);
    /// RACKCABECERAAPLICAR redraws the views of the cabecera (lateral and planta) in place, with one «U» to undo it.
    /// </summary>
    public interface IRackCadCabeceraDoorPort : IRackCadRackDoorPort
    {
        RackCadClassicResult RequestCabeceraDesign(RackDoorRequest request);

        RackCadClassicResult RequestCabeceraUpdate(CabeceraDoorWrite write);

        event Action<CabeceraDesignRead> CabeceraRead;
    }

    /// <summary>The pure rules of the door of the cabecera, with their tests in Core; the commands only call them.</summary>
    public static class CabeceraDoorRules
    {
        /// <summary>The answer of RACKCABECERAABRIR for the envelope of the first view of the rack.</summary>
        public static CabeceraDesignRead AnswerForOpen(long requestId, string rackId, string kind, string name, string designJson)
        {
            if (!string.Equals(kind, RackEmbedDocument.KindCabecera, StringComparison.Ordinal))
                return CabeceraDesignRead.Refused(requestId, rackId, kind, name, "El rack no es una cabecera: este editor no lo abre.");

            RackProject project;
            try
            {
                project = new RackProjectStore().Deserialize(designJson);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return CabeceraDesignRead.Refused(requestId, rackId, kind, name, "No se pudieron leer los datos de la cabecera. " + ex.Message);
            }

            if (project?.Header == null) return CabeceraDesignRead.Refused(requestId, rackId, kind, name, "Datos de cabecera inválidos.");
            var header = project.Header;
            // Correction 7 of the review, the rule of the classic editor (EditCabecera): the inner name is not touched (AUTH-15 admits it
            // blank); the name shown is the inner one, or the one of the envelope when the inner one is blank.
            return new CabeceraDesignRead(requestId, rackId, true, kind, string.IsNullOrWhiteSpace(header.Name) ? name : header.Name,
                new RackFrameProjectStore().Serialize(header), null);
        }

        /// <summary>The plan of RACKCABECERAAPLICAR: the rack must still be a readable cabecera, and the window must send one.</summary>
        public static CabeceraApplyPlan PlanApply(string kind, string designJson, CabeceraDoorWrite write)
        {
            const string NotReadable = "El rack del dibujo no es una cabecera legible: no se escribió nada.";
            if (write?.Cabecera == null || write.Cabecera.Horizontals.Count < 2) return new CabeceraApplyPlan(false, null, "No hay una cabecera válida que escribir.");
            if (!string.Equals(kind, RackEmbedDocument.KindCabecera, StringComparison.Ordinal)) return new CabeceraApplyPlan(false, null, NotReadable);
            try
            {
                var project = new RackProjectStore().Deserialize(designJson);
                return project?.Header == null ? new CabeceraApplyPlan(false, null, NotReadable) : new CabeceraApplyPlan(true, project, null);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return new CabeceraApplyPlan(false, null, NotReadable);
            }
        }
    }
}
