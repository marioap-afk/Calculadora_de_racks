using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.CustomProperties;
using RackCad.UI;
using RackCad.UI.Shell;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;

namespace RackCad.Plugin
{
    /// <summary>
    /// RACKPROPIEDADES — the custom properties of a rack or of the project, administered from product (I-54 G7).
    ///
    /// <para>
    /// One command for both scopes, with the names the Owner decided (D-18.1, OQ-03): the user picks a rack, or types the
    /// keyword Proyecto. RPR is its only alias and runs the very same method.
    /// </para>
    /// <para>
    /// It carries no rules of its own. It asks, reads through the physical edge, shows the window, hands the one request the
    /// user made to the early preflight of Application and to the executor, and reads the drawing again. Whether a rack is
    /// writable, whether its kind is known, whether a unify is safe, whether what was shown is still what the drawing holds,
    /// and whether a snapshot is stale are all decided behind it, on a fresh read (D-22). Between those, this file decides
    /// nothing.
    /// </para>
    /// <para>
    /// After every operation it goes back to the drawing: the window never holds a model that outlives one write, and the
    /// result of the last operation travels to the next window and to the command line. Closing the window ends the command.
    /// </para>
    /// </summary>
    public sealed class RackPropiedadesCommands
    {
        private const string ProjectKeyword = "Proyecto";

        [CommandMethod("RPR")] public void AliasRackPropiedades() => RackPropiedades();     // RACKPROPIEDADES

        [CommandMethod("RACKPROPIEDADES")]
        public void RackPropiedades()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;

                if (document == null)
                {
                    return;
                }

                var options = new PromptEntityOptions("\nSelecciona un rack o [Proyecto]: ", ProjectKeyword);
                options.SetRejectMessage("\nEse objeto no es un bloque.");
                options.AddAllowedClass(typeof(BlockReference), exactMatch: false);

                var pick = document.Editor.GetEntity(options);

                if (pick.Status == PromptStatus.Keyword)
                {
                    EditProject(document);
                    return;
                }

                if (pick.Status != PromptStatus.OK)
                {
                    return;
                }

                // Selection only, the same convention as picking a rack anywhere else: the reference is resolved to its
                // definition. Nothing of the rack — no payload, no properties — is read here.
                var definitionId = InDocumentTransaction.Run(
                    document,
                    transaction => ((BlockReference)transaction.GetObject(pick.ObjectId, OpenMode.ForRead)).BlockTableRecord);

                EditRack(document, definitionId);
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>The Project loop: read → window → one request → executor on a fresh read → read again.</summary>
        private static void EditProject(Document document)
        {
            EditorStatusMessage status = null;

            while (true)
            {
                var read = CustomPropertiesExecutor.ReadProject(document);
                var window = new RackCustomPropertiesWindow(read.Workspace, null, status);
                AcApplication.ShowModalWindow(window);

                if (window.Intent == null)
                {
                    return;
                }

                // The Project has no early preflight of its own in Application: the executor applies the request to the
                // entry as it is NOW and refuses without writing, which is the answer a preflight would give one step earlier.
                var execution = CustomPropertiesExecutor.ExecuteProject(document, window.Intent);

                status = execution.IsWritten
                    ? EditorStatusMessage.Success(Done(window.Intent.Kind))
                    : EditorStatusMessage.Error(NotWritten(execution.Error));

                Tell(document, status);
            }
        }

        /// <summary>The Rack loop over the picked definition: read → window → one request → preflight → executor → read again.</summary>
        private static void EditRack(Document document, ObjectId definitionId)
        {
            EditorStatusMessage status = null;

            while (true)
            {
                var read = CustomPropertiesExecutor.ReadRack(document, definitionId);

                if (!read.HasRack)
                {
                    Tell(document, EditorStatusMessage.Warning("El bloque elegido no es un rack de RackCad."));
                    return;
                }

                // What the user is about to see of a writable rack, captured from this same read; the executor compares it
                // with the fresh one. A read-only rack offers no write, so there is nothing to capture.
                var displayed = read.Workspace.State == CustomPropertiesWorkspaceState.ReadOnly
                    ? null
                    : RackCustomPropertiesDisplayedState.Capture(read.Authority);

                var window = new RackCustomPropertiesWindow(read.Workspace, displayed, status);
                AcApplication.ShowModalWindow(window);

                if (window.UnifyIntent != null)
                {
                    status = Unify(document, read, window.UnifyIntent);
                }
                else if (window.Intent != null)
                {
                    status = Edit(document, read, displayed, window.Intent);
                }
                else
                {
                    return;
                }

                Tell(document, status);
            }
        }

        private static EditorStatusMessage Edit(
            Document document, CustomPropertiesRackRead read, RackCustomPropertiesDisplayedState displayed, CustomPropertiesIntent intent)
        {
            var preflight = CustomPropertiesPreflight.ForRack(read.Authority, intent);

            if (!preflight.IsAccepted)
            {
                return EditorStatusMessage.Error(NotWritten(preflight.Error));
            }

            var result = CustomPropertiesExecutor.ExecuteRack(document, read.Selection, displayed, intent);

            return result.IsPlanned
                ? EditorStatusMessage.Success(Done(intent.Kind))
                : EditorStatusMessage.Error(NotWritten(result.Error));
        }

        private static EditorStatusMessage Unify(Document document, CustomPropertiesRackRead read, RackCustomPropertiesUnifyIntent intent)
        {
            var preflight = CustomPropertiesPreflight.ForRackUnify(read.Authority, intent);

            if (!preflight.IsAccepted)
            {
                return EditorStatusMessage.Error(NotWritten(preflight.Error));
            }

            var result = CustomPropertiesExecutor.ExecuteRackUnify(document, read.Selection, intent);

            return result.IsPlanned
                ? EditorStatusMessage.Success("Vistas unificadas.")
                : EditorStatusMessage.Error(NotWritten(result.Error));
        }

        private static string Done(CustomPropertiesIntentKind kind)
        {
            switch (kind)
            {
                case CustomPropertiesIntentKind.Create:
                    return "Propiedad creada.";
                case CustomPropertiesIntentKind.Rename:
                    return "Propiedad renombrada.";
                case CustomPropertiesIntentKind.ChangeValue:
                    return "Valor cambiado.";
                default:
                    return "Propiedad eliminada.";
            }
        }

        private static string NotWritten(string error)
            => "No se escribió nada: " + (string.IsNullOrWhiteSpace(error) ? "la operación no se pudo aplicar." : error);

        private static void Tell(Document document, EditorStatusMessage status)
            => document.Editor.WriteMessage("\nRackCad: " + status.Text);
    }
}
