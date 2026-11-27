using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Diagnostics;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin
{
    /// <summary>
    /// The tab «Propiedades» of the editors of the RackCad window at the door (I-64 A-14, cuarta tanda, va; VA-07): the door reads the
    /// custom properties of a rack through the physical edge (<see cref="CustomPropertiesExecutor.ReadRack"/>, the authority RACKPROPIEDADES
    /// uses), and «Actualizar» writes the draft of the tab after it redrew the rack, in the same command (one «U»), through the edge too
    /// (<see cref="CustomPropertiesExecutor.ExecuteRackDraft"/>). This file decides nothing and opens no transaction of its own.
    /// </summary>
    internal static class RackPropertiesDoorData
    {
        /// <summary>The tab of the rack of a block definition.</summary>
        internal static RackPropertiesRead Read(Document document, ObjectId definitionId)
        {
            var read = CustomPropertiesExecutor.ReadRack(document, definitionId);
            return RackPropertiesRules.Read(read.HasRack ? read.Authority : null);
        }

        /// <summary>
        /// After the redraw of «Actualizar» (only when it redrew the rack): the draft of the tab, written in every view the rack has NOW. The
        /// answer says it and carries what the drawing holds then; when they could not be written it says why, and the editor keeps them.
        /// </summary>
        internal static RackDesignWritten WithRackProperties(Document document, string rackId, RackPropertiesWrite write, RackDesignWritten answer)
        {
            if (write == null || answer == null || !answer.Written) return answer;
            string error = null;
            RackPropertiesRead after = null;
            try
            {
                var blocks = RackCommandSupport.FindRackBlocks(document, rackId);
                var read = blocks.Count == 0 ? null : CustomPropertiesExecutor.ReadRack(document, blocks[0].BlockId);
                if (read == null || !read.HasRack)
                {
                    error = "el rack ya no está en el dibujo.";
                }
                else
                {
                    var result = CustomPropertiesExecutor.ExecuteRackDraft(document, read.Selection, write.Fingerprint, write.Entries, write.UnifiedFrom);
                    if (!result.IsPlanned) error = result.Error;
                    else after = Read(document, blocks[0].BlockId); // what the drawing holds now (the ids the creates minted)
                }
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
            }

            return error == null
                ? answer with { Message = answer.Message + " También escribió sus propiedades.", Properties = after }
                : answer with { Message = answer.Message + " Pero sus propiedades no se escribieron: " + error, PropertiesError = error };
        }

        /// <summary>
        /// T6 pr (VA-S9, Owner 2026-11-27: «Sí, y se escriben al insertarlo»): after the insertion of «Insertar en el dibujo…» of a draft of
        /// «Nuevo» (the NUEVO commands, only when AutoCAD placed the rack), the draft of its tab, written in every view the rack has NOW, in
        /// the same command (one «U»), through the edge (<see cref="CustomPropertiesExecutor.ExecuteRackDraft"/>), over the rack just
        /// inserted while it holds no property (<see cref="RackPropertiesRules.NewRackFingerprint"/>). Then, written or not, what the drawing
        /// holds, so the tab of the editor goes on from the rack of the drawing; the answer is the rule of Application
        /// (<see cref="RackPropertiesRules.AnswerForNewRack"/>).
        /// </summary>
        internal static RackDesignWritten WithNewRackProperties(Document document, string rackId, RackPropertiesWrite write, RackDesignWritten answer)
        {
            if (answer == null || !answer.Written) return answer;
            string error = null;
            if (write != null)
            {
                try
                {
                    var blocks = RackCommandSupport.FindRackBlocks(document, rackId);
                    var read = blocks.Count == 0 ? null : CustomPropertiesExecutor.ReadRack(document, blocks[0].BlockId);
                    if (read == null || !read.HasRack)
                    {
                        error = "el rack nuevo no está en el dibujo.";
                    }
                    else
                    {
                        var fingerprint = RackPropertiesRules.NewRackFingerprint(read.Authority, out error);
                        if (fingerprint != null)
                        {
                            var result = CustomPropertiesExecutor.ExecuteRackDraft(document, read.Selection, fingerprint, write.Entries, null);
                            if (!result.IsPlanned) error = result.Error;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    error = ex.Message;
                }
            }

            RackPropertiesRead after = null;
            try
            {
                var placed = RackCommandSupport.FindRackBlocks(document, rackId);
                if (placed.Count > 0) after = Read(document, placed[0].BlockId); // what the drawing holds now (the ids the creates minted)
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackPropertiesDoorData.WithNewRackProperties", ex);
            }

            return RackPropertiesRules.AnswerForNewRack(answer, write, error, after);
        }
    }
}
