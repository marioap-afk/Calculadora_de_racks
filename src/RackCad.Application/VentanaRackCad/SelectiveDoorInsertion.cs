using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>One view «Insertar vista…» offers for a Selectivo: its label, as «Insertar varias…» of the classic editor shows it, and its address.</summary>
    public sealed record SelectiveInsertOption(string Label, RackViewAddress Address);

    /// <summary>
    /// The rules of «Insertar vista…» and of the new Selectivo of the RackCad window that need no AutoCAD (I-64 A-14, line D, Selectivo
    /// E2, part A), as the second delivery of Dinámico did them for RACKDINAMICOINSERTAR and RACKDINAMICONUEVO: the views offered, which
    /// of the chosen ones the rack can draw, and the answers of RACKSELECTIVOINSERTAR and RACKSELECTIVONUEVO (the SAME wording as the
    /// door of Dinámico, so the two systems answer alike).
    /// </summary>
    public static class SelectiveDoorInsertion
    {
        /// <summary>
        /// The views «Insertar vista…» offers for a Selectivo, in the order of «Insertar varias…» of the classic editor
        /// (<c>RackSelectiveWindow.InsertBatch_Click</c>): a frontal per fondo, a lateral cut per post that has one, and the plan.
        /// </summary>
        public static IReadOnlyList<SelectiveInsertOption> ViewOptions(SelectiveRackSystem system, RackCatalog catalog)
        {
            var options = new List<SelectiveInsertOption>();
            var fondos = SelectiveDepthLayout.Count(system);
            for (var i = 0; i < fondos; i++)
                options.Add(new SelectiveInsertOption("Frontal · fondo " + (i + 1), RackViewAddress.Fondo(i)));
            if (system != null)
            {
                foreach (var cut in new SelectiveLateralBuilder().Cortes(system, catalog))
                    options.Add(new SelectiveInsertOption("Lateral · poste " + (cut.PostIndex + 1), RackViewAddress.Post(cut.PostIndex)));
            }

            options.Add(new SelectiveInsertOption("Planta", RackViewAddress.Whole(DimensionViewKind.Planta)));
            return options;
        }

        /// <summary>
        /// The chosen views the rack can draw, in their order and without repeats, with the availability rule the drawing uses
        /// (<see cref="SelectiveViewAvailabilityFacts"/>): a fondo the rack has, a post with a cut, the plan. A view the window cannot
        /// offer any more (the draft lost a fondo or a cut after the choice) is left out, so AutoCAD never prepares one it cannot draw.
        /// </summary>
        public static IReadOnlyList<RackViewAddress> DrawableViews(IReadOnlyList<RackViewAddress> chosen, SelectiveRackSystem system, RackCatalog catalog)
        {
            if (chosen == null || chosen.Count == 0) return Array.Empty<RackViewAddress>();
            var posts = system == null
                ? Enumerable.Empty<int>()
                : new SelectiveLateralBuilder().Cortes(system, catalog).Select(cut => cut.PostIndex);
            var facts = new SelectiveViewAvailabilityFacts(SelectiveDepthLayout.Count(system), posts);
            var result = new List<RackViewAddress>();
            foreach (var view in chosen)
            {
                if (facts.Contains(view) && !result.Contains(view)) result.Add(view);
            }

            return result;
        }

        /// <summary>The answer of RACKSELECTIVOINSERTAR: the same as RACKDINAMICOINSERTAR (<see cref="RackDoorRules.AnswerForInsert"/>).</summary>
        public static RackDesignWritten AnswerForInsert(long requestId, string rackId, int redrawn, int erased, int inserted)
            => RackDoorRules.AnswerForInsert(requestId, rackId, redrawn, erased, inserted);

        /// <summary>The answer of RACKSELECTIVONUEVO: the same as RACKDINAMICONUEVO (<see cref="RackDoorRules.AnswerForNewRack"/>).</summary>
        public static RackDesignWritten AnswerForNewRack(long requestId, string rackId, int placed, string name)
            => RackDoorRules.AnswerForNewRack(requestId, rackId, placed, name);

        /// <summary>
        /// Why a write of the window cannot insert, or null when it can: an insertion needs at least one view; a new rack needs a draft
        /// and an identity given by the window. An update (no views, not new) always passes.
        /// </summary>
        public static string RefusalOf(SelectiveDoorWrite write)
        {
            if (write == null || write.Design == null) return "No hay un borrador válido que insertar.";
            if (write.NewRack && !write.Inserts) return "Elige al menos una vista para insertar el rack nuevo.";
            if (write.NewRack && string.IsNullOrWhiteSpace(write.RackId)) return "El rack nuevo no tiene identidad: no se insertó nada.";
            return null;
        }
    }
}
