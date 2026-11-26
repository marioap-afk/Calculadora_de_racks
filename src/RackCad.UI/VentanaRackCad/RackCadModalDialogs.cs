using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Views;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// How the RackCad window shows a classic dialog as a modal of AutoCAD (step 4 of Dinámico): the host shows it over the drawing, so
    /// AutoCAD waits for it as it waits for the classic editors. The palette never shows a modal of its own: without the port the window
    /// does not offer the dialogs.
    /// </summary>
    public interface IRackCadModalPort
    {
        /// <summary>Shows the dialog and waits for it; true when it was accepted.</summary>
        bool? ShowModal(Window dialog);
    }

    /// <summary>
    /// The classic dialogs the editors of the window reuse, built with the arguments the classic editor gives them, so they behave the
    /// same: «Elementos de seguridad…» of Dinámico (<see cref="RackCad.UI.Systems.Dynamic.RackDynamicSystemWindow"/>).
    /// </summary>
    public static class RackCadModalDialogs
    {
        /// <summary>The introduction of the safety dialog of the classic Dinámico editor, word for word.</summary>
        internal const string DynamicSafetyIntroduction =
            "Izquierda es la salida y derecha la entrada. La selección se proyecta en lateral, frontal y "
            + "planta: el protector lateral reemplaza las botas del mismo poste y los desviadores respetan "
            + "la matriz frente por nivel. La defensa de montacargas permite longitud independiente en "
            + "salida y entrada por poste. La guía se coloca por frente y nivel únicamente en la entrada. "
            + "El BOM conserva el conteo físico sin duplicar vistas.";

        /// <summary>The five families of the safety of Dinámico in the catalog, as the classic dialog lists them.</summary>
        internal static List<SafetyElementCatalogEntry> DynamicSafetyElements(RackCatalog catalog)
            => (catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>())
                .Where(element => element != null
                                  && (SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.BotaType)
                                      || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.LateralType)
                                      || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DesviadorType)
                                      || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType)
                                      || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.GuiaType)))
                .ToList();

        /// <summary>The safety dialog of the classic Dinámico editor over copies of the safety of the draft.</summary>
        internal static SelectiveSafetyWindow BuildDynamicSafetyDialog(DynamicDraftEditor draft)
            => new SelectiveSafetyWindow(
                DynamicSafetyElements(draft.Catalog),
                draft.CopySafety(),
                postCount: draft.SafetyPostCount,
                levelsPerFrente: draft.SafetyLevelsPerFront(),
                fondoCount: 1,
                catalog: draft.Catalog,
                fallbackLevelsArePerPost: false,
                introduction: DynamicSafetyIntroduction,
                includeDefensa: true,
                includeGuia: true,
                useDynamicSafetyDefaults: true,
                bootUsesPlacementNames: true,
                desviadorLevelsPerPost: draft.DeflectorLevelsPerPost(),
                allowBlankFrontColumns: true,
                showDesviadorSide: true);

        /// <summary>
        /// The views «Insertar vista…» offers for a Dinámico, as «Insertar varias…» of the classic editor lists them: the two frontal cuts,
        /// a lateral cut by post that has one, and the plan. T4 ins (DG-01): they are chosen in the panel of the editor
        /// (<see cref="RackViewInsertPanelViewModel"/>); the window no longer shows the classic dialog «Insertar varias vistas».
        /// </summary>
        public static IReadOnlyList<RackViewBatchOption> DynamicViewOptions(DynamicRackSystem system, RackCatalog catalog)
        {
            var options = new List<RackViewBatchOption>
            {
                new RackViewBatchOption("Frontal · salida", RackViewAddress.FlowEnd(RackFlowEnd.Exit)),
                new RackViewBatchOption("Frontal · entrada", RackViewAddress.FlowEnd(RackFlowEnd.Entrance)),
                new RackViewBatchOption("Planta", RackViewAddress.Whole(DimensionViewKind.Planta)),
            };
            if (system == null) return options;
            foreach (var cut in new DynamicSystemLateralBuilder().Cortes(system, catalog))
                options.Insert(options.Count - 1, new RackViewBatchOption("Lateral · poste " + (cut.PostIndex + 1), RackViewAddress.Post(cut.PostIndex)));
            return options;
        }

        /// <summary>«Editar seguridad…»: the dialog over AutoCAD; its result, or null when it was cancelled.</summary>
        public static IReadOnlyList<SelectiveSafetySelection> EditDynamicSafety(DynamicDraftEditor draft, IRackCadModalPort modal)
        {
            if (draft == null || modal == null) return null;
            var dialog = BuildDynamicSafetyDialog(draft);
            return modal.ShowModal(dialog) == true ? dialog.Result : null;
        }
    }
}
