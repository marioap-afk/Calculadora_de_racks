using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>One view «Insertar vista…» offers for a Selectivo: its label, as «Insertar varias…» of the classic editor shows it, and its address.</summary>
    public sealed record SelectiveInsertOption(string Label, RackViewAddress Address);

    /// <summary>
    /// What RACKSELECTIVONUEVO decided before the dispatch of «Nuevo» runs (Selectivo E2): the views to insert, in their order, or why
    /// nothing is inserted.
    /// </summary>
    public sealed record SelectiveNewRackPlan(bool CanInsert, IReadOnlyList<RackViewAddress> Views, string Reason)
    {
        internal static SelectiveNewRackPlan Refused(string reason) => new SelectiveNewRackPlan(false, Array.Empty<RackViewAddress>(), reason);
    }

    /// <summary>
    /// The rules of «Insertar vista…» and of the new Selectivo of the RackCad window that need no AutoCAD (I-64 A-14, line D, Selectivo
    /// E2, part A), as the second delivery of Dinámico did them for RACKDINAMICOINSERTAR and RACKDINAMICONUEVO: the views offered, which
    /// of the chosen ones the rack can draw, what RACKSELECTIVONUEVO decides before the dispatch of «Nuevo» (<see cref="PlanNewRack"/>),
    /// and the answers of RACKSELECTIVOINSERTAR and RACKSELECTIVONUEVO (the SAME wording as the door of Dinámico, so the two systems
    /// answer alike).
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

        /// <summary>The refusal of RACKSELECTIVONUEVO when the identity the window gave the new rack is already in the drawing (the text of Dinámico).</summary>
        public const string IdentityInDrawingText = "Ya hay un rack con esa identidad en el dibujo: no se insertó nada.";

        /// <summary>
        /// The refusal of RACKSELECTIVONUEVO when a holgura of the new rack is bound to a project variable: the dispatch of «Nuevo» writes
        /// the authored document from the design alone (as the classic editor opened from RACKNUEVO, which offers no binding), so the
        /// binding would be lost without a word.
        /// </summary>
        public const string LinkedNewRackText =
            "El Selectivo nuevo tiene una holgura vinculada a una variable del proyecto, y un rack nuevo se inserta sin vínculos: no se insertó nada.";

        /// <summary>
        /// What RACKSELECTIVONUEVO decides before it hands the new Selectivo to the dispatch of «Nuevo» (<c>RackMenuCommands.DispatchInsertion</c>,
        /// the one the classic editor opened from RACKNUEVO uses), from the write the window sent, whether the drawing already holds a rack
        /// with its identity, and the catalog AutoCAD draws with. Nothing is inserted when: there is no draft (design and system); the write
        /// is not the one of a new rack; <see cref="RefusalOf"/> refuses it (no view, no identity); the identity is already in the drawing (the
        /// rack would be a copy of another one); a holgura is bound to a variable (<see cref="LinkedNewRackText"/>); or none of the chosen views
        /// can be drawn with the draft (<see cref="DrawableViews"/>, the same facts the dispatch prepares them with). Otherwise the views to
        /// insert are the drawable ones, in their order and without repeats.
        /// </summary>
        public static SelectiveNewRackPlan PlanNewRack(SelectiveDoorWrite write, bool identityInDrawing, RackCatalog catalog)
        {
            if (write?.Design == null || write.System == null) return SelectiveNewRackPlan.Refused("No hay un borrador válido que insertar.");
            if (!write.NewRack) return SelectiveNewRackPlan.Refused("La petición no es la de un rack nuevo: no se insertó nada.");

            var refusal = RefusalOf(write);
            if (refusal != null) return SelectiveNewRackPlan.Refused(refusal);
            if (identityInDrawing) return SelectiveNewRackPlan.Refused(IdentityInDrawingText);

            foreach (var pair in write.LinkedPropertyFinalStates ?? new Dictionary<PropertyId, LinkedPropertyEditState>())
            {
                if (pair.Value != null && pair.Value.Source.Kind != LinkedPropertySourceKind.Literal)
                    return SelectiveNewRackPlan.Refused(LinkedNewRackText);
            }

            var views = DrawableViews(write.InsertViews, write.System, catalog);
            if (views.Count == 0)
                return SelectiveNewRackPlan.Refused("Ninguna de las vistas elegidas se puede dibujar con el borrador: no se insertó nada.");

            return new SelectiveNewRackPlan(true, views, null);
        }
    }
}
