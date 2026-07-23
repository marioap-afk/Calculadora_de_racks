using System.Collections.Generic;
using RackCad.Application.Bom;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The pure product of one Push Back editor recompute: the assembled <see cref="Design"/>, the resolved
    /// <see cref="System"/>, the component <see cref="Bom"/> and the four drawable plans (full lateral, both frontal cuts and
    /// planta). It carries only data — no WPF, AutoCAD, Window, Dispatcher, ObjectId or UI control — so the increment-3
    /// window renders it without any product logic. A failed recompute yields <see cref="IsValid"/> = false and a
    /// human-readable <see cref="Error"/>, with the geometry left null (the window keeps its previous drawing, exactly like
    /// the dynamic editor does on an invalid input).
    /// </summary>
    public sealed class PushBackEditorComputation
    {
        private PushBackEditorComputation(
            bool isValid,
            string error,
            PushBackDesign design,
            PushBackSystem system,
            BillOfMaterials bom,
            DynamicSystemPlan lateral,
            DynamicSystemPlan entradaSalida,
            DynamicSystemPlan posterior,
            DynamicSystemPlan planta,
            IReadOnlyList<DynamicLateralCorte> lateralCortes)
        {
            IsValid = isValid;
            Error = error;
            Design = design;
            System = system;
            Bom = bom;
            LateralPlan = lateral;
            FrontalEntradaSalida = entradaSalida;
            FrontalPosterior = posterior;
            PlantaPlan = planta;
            LateralCortes = lateralCortes ?? new List<DynamicLateralCorte>();
        }

        public bool IsValid { get; }
        public string Error { get; }

        /// <summary>The assembled, persistable Push Back design (shared dynamic structure + rear peraltes/topes/safety).</summary>
        public PushBackDesign Design { get; }

        /// <summary>The resolved Push Back system the BOM and plans were built from.</summary>
        public PushBackSystem System { get; }

        public BillOfMaterials Bom { get; }

        /// <summary>The full lateral cut (every post span).</summary>
        public DynamicSystemPlan LateralPlan { get; }

        /// <summary>The low (entrance/exit) frontal cut: complete IN/OUT beams + applicable safety (never a guide).</summary>
        public DynamicSystemPlan FrontalEntradaSalida { get; }

        /// <summary>The rear frontal cut: TROQUEL_REDONDO beams + active rear topes, no normal dynamic safety.</summary>
        public DynamicSystemPlan FrontalPosterior { get; }

        public DynamicSystemPlan PlantaPlan { get; }

        /// <summary>The lateral section at each transverse post (post index + its per-post plan), computed ONCE by the
        /// assembler. The window's lateral preview draws the selected corte's plan without re-invoking any builder.</summary>
        public IReadOnlyList<DynamicLateralCorte> LateralCortes { get; }

        public static PushBackEditorComputation Success(
            PushBackDesign design,
            PushBackSystem system,
            BillOfMaterials bom,
            DynamicSystemPlan lateral,
            DynamicSystemPlan entradaSalida,
            DynamicSystemPlan posterior,
            DynamicSystemPlan planta,
            IReadOnlyList<DynamicLateralCorte> lateralCortes)
            => new PushBackEditorComputation(true, null, design, system, bom, lateral, entradaSalida, posterior, planta, lateralCortes);

        public static PushBackEditorComputation Failure(string error)
            => new PushBackEditorComputation(false, error, null, null, null, null, null, null, null, new List<DynamicLateralCorte>());
    }
}
