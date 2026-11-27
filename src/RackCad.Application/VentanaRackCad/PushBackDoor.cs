using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// T5 pb (I-64, fifth batch): the window asks AutoCAD to redraw ONE Push Back of the drawing of the session with its draft
    /// (RACKPUSHBACKAPLICAR, «Actualizar»), to redraw it and then insert views (RACKPUSHBACKINSERTAR, «Insertar vista…»), or to insert a new
    /// Push Back of the window (RACKPUSHBACKNUEVO, «Insertar en el dibujo…»). The design and the system are COPIES taken when the request
    /// was made and the name is already trimmed; with <paramref name="Properties"/> «Actualizar» also writes the tab «Propiedades» (VA-07).
    /// </summary>
    public sealed record PushBackDoorWrite(long RequestId, SessionId Session, string RackId, string Name, PushBackDesign Design, PushBackSystem System,
        IReadOnlyList<RackViewAddress> InsertViews = null, bool NewRack = false, RackPropertiesWrite Properties = null)
    {
        /// <summary>The write also inserts views.</summary>
        public bool Inserts => InsertViews != null && InsertViews.Count > 0;
    }

    /// <summary>What RACKPUSHBACKAPLICAR decided before drawing: the project of the rack as the drawing has it now, or why nothing is written.</summary>
    public sealed record PushBackApplyPlan(bool CanWrite, RackProject Project, string Reason);

    /// <summary>
    /// T5 pb: the door of the RackCad window to ONE Push Back of the drawing, with the rules of the doors of Dinámico, Selectivo and the
    /// cabecera: a numbered request, the drawing that was active at the request, «Dejar de esperar» (<see cref="IRackCadRackDoorPort.Abandon"/>)
    /// and <see cref="IRackCadRackDoorPort.RequestStarted"/>. RACKPUSHBACKABRIR only reads (no undo marker, no history) and answers with
    /// <see cref="PushBackRead"/>; the writes answer with <see cref="IRackCadRackDoorPort.DesignWritten"/>, and each one is one «U».
    /// </summary>
    public interface IRackCadPushBackDoorPort : IRackCadRackDoorPort
    {
        /// <summary>Asks AutoCAD to read the Push Back (RACKPUSHBACKABRIR); the answer arrives with <see cref="PushBackRead"/>.</summary>
        RackCadClassicResult RequestPushBackDesign(RackDoorRequest request);

        /// <summary>Asks AutoCAD to write the draft as <see cref="PushBackDoorWrite"/> says; the answer arrives with DesignWritten.</summary>
        RackCadClassicResult RequestPushBackUpdate(PushBackDoorWrite write);

        event Action<RackDesignRead> PushBackRead;
    }

    /// <summary>The rules of the door of Push Back that need no AutoCAD, so they are tested where AutoCAD is not.</summary>
    public static class PushBackDoorRules
    {
        /// <summary>What a rack that the window cannot open says, with the way out (the classic editor).</summary>
        public const string UseClassicText = " Usa RACKEDITAR para abrirlo con el editor clásico.";

        /// <summary>
        /// The answer of RACKPUSHBACKABRIR for the envelope of the first view of the rack: its kind must be Push Back and its inner design a
        /// readable Push Back. A one-way rack that keeps the modules of a parked composite tail is outside the guarantee of the editable
        /// state (it would lose them), so the window does not open it, as the full draft of the pilot does not.
        /// </summary>
        public static RackDesignRead AnswerForOpen(long requestId, string rackId, string kind, string name, string designJson, RackPropertiesRead properties = null)
        {
            if (!string.Equals(kind, RackEmbedDocument.KindPushBack, StringComparison.OrdinalIgnoreCase))
                return new RackDesignRead(requestId, rackId, false, kind, name, null, "El rack no es un Push Back: este editor no lo abre.");

            RackProject project;
            try
            {
                project = new RackProjectStore().Deserialize(designJson);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return new RackDesignRead(requestId, rackId, false, kind, name, null, "No se pudieron leer los datos del sistema Push Back. " + ex.Message);
            }

            if (project?.PushBackDesign == null)
                return new RackDesignRead(requestId, rackId, false, kind, name, null, "Datos de sistema Push Back inválidos.");

            if (PushBackEditableState.SupportOf(project.PushBackDesign) == PushBackEditableSupport.DormantCompositeTail)
                return new RackDesignRead(requestId, rackId, false, kind, name, null,
                    "Este Push Back guarda la mitad dormida de un lado B que la ventana todavía no edita." + UseClassicText);

            return new RackDesignRead(requestId, rackId, true, kind, name, designJson, null, properties);
        }

        /// <summary>
        /// What RACKPUSHBACKAPLICAR and RACKPUSHBACKINSERTAR decide before drawing, from the first view of the rack read AGAIN at write time:
        /// the window sends a valid draft, and the rack is still a readable Push Back. The classic editor is modal, so what it opened is what
        /// it writes on; the window is not, so the drawing may have changed in between.
        /// </summary>
        public static PushBackApplyPlan PlanApply(string kind, string designJson, PushBackDoorWrite write)
        {
            const string NotReadable = "El rack del dibujo no es un Push Back legible: no se escribió nada.";
            if (write?.Design == null || write.System == null) return new PushBackApplyPlan(false, null, "No hay un borrador válido que escribir.");
            if (!string.Equals(kind, RackEmbedDocument.KindPushBack, StringComparison.OrdinalIgnoreCase)) return new PushBackApplyPlan(false, null, NotReadable);
            try
            {
                var project = new RackProjectStore().Deserialize(designJson);
                return project?.PushBackDesign == null ? new PushBackApplyPlan(false, null, NotReadable) : new PushBackApplyPlan(true, project, null);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return new PushBackApplyPlan(false, null, NotReadable);
            }
        }

        /// <summary>
        /// Why the door does not take a write (null when it does): a draft without design, system or identity, a new rack without views, or
        /// «Insertar vista…» without views. The rule of the door of Dinámico.
        /// </summary>
        public static string RefusalOf(PushBackDoorWrite write)
        {
            if (write == null || string.IsNullOrWhiteSpace(write.RackId) || write.Design == null || write.System == null)
                return "No hay un borrador válido que escribir.";
            if (write.NewRack && !write.Inserts) return "Elige al menos una vista para insertar el rack.";
            return null;
        }

        /// <summary>
        /// The name the views take when the draft is written: the name of the draft, or the one of the envelope when the draft has none, as
        /// RACKEDITAR names them. The system of the write carries it (this is the only writer of that name for the window).
        /// </summary>
        public static PushBackSystem Named(PushBackSystem system, string draftName, string envelopeName)
        {
            if (system == null) return null;
            var name = string.IsNullOrWhiteSpace(draftName) ? envelopeName : draftName;
            system.Name = name;
            return system;
        }
    }
}
