using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — el DESVIADOR de una cama Push Back, resuelto en el marco de la CAMA.
    ///
    /// <para>
    /// Contrato físico: <c>Diverter(run) = LowEnd(run)</c>. Un desviador guía la tarima al ENTRAR, así que pertenece
    /// siempre al extremo por el que se carga, y ese extremo lo sabe la cama. No depende del lado A/B, ni de
    /// izquierda/derecha en coordenadas de rack, ni del extremo alto, ni de la elevación que el resolver compartido
    /// le dio al nivel.
    /// </para>
    /// <para>
    /// Existe porque el desviador se había quedado fuera del pipeline por camas. El corte lateral compuesto lo
    /// heredaba del builder dinámico, que conserva la regla de un rack de un solo sentido —«izquierda = extremo
    /// bajo, derecha = extremo alto»— y en un rack compuesto eso es falso: el lado B tiene su extremo bajo a la
    /// DERECHA. El lado A funcionaba por coincidencia y el B salía a la elevación del extremo contrario. Además una
    /// clasificación por línea no puede expresar que, en la MISMA línea, el nivel 1 corra A→B y el nivel 2 B→A; la
    /// cama sí tiene esa granularidad.
    /// </para>
    /// <para>
    /// En el marco de una cama el flujo avanza SIEMPRE hacia +X, así que aquí el extremo bajo es el arranque del
    /// frente y la pieza se emite sin espejo. Llevarla al mundo —incluida la mano— es trabajo de la MISMA reflexión
    /// rígida que mueve el riel, los rodillos, los largueros y el tope de esa cama.
    /// </para>
    /// </summary>
    public static class PushBackDiverterPlan
    {
        private const string View = "LATERAL";

        /// <summary>
        /// Los desviadores de las camas de <paramref name="front"/> en el sistema <paramref name="system"/>, todos
        /// en su extremo BAJO. <paramref name="levels"/> acota a los niveles que esta llamada materializa (un nivel
        /// puede pertenecer a una cama corrida y no a la de este lado); null = todos.
        /// </summary>
        public static IReadOnlyList<HeaderBlockInstance> Lateral(
            PushBackSystem system,
            RackCatalog catalog,
            DynamicRackFront front,
            IReadOnlyCollection<int> levels = null,
            int postIndex = -1)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null || front == null)
            {
                return result;
            }

            var selection = SelectiveSafetyFamilies.SelectedOfType(
                structure.SafetySelections, catalog?.SafetyElements, SelectiveSafetyDefaults.DesviadorType);
            if (selection == null)
            {
                return result;
            }

            var block = CatalogLookup.Block(catalog, selection.ElementId, View);
            if (string.IsNullOrWhiteSpace(block))
            {
                return result;
            }

            var longitud = SelectiveDesviadorPlan.IsValidEvenAbove8(selection.DesviadorLongitud)
                ? selection.DesviadorLongitud
                : SelectiveSafetyDefaults.DesviadorLongitud;
            var firstHeight = SelectiveDesviadorPlan.IsValidEvenAbove8(selection.DesviadorPrimerNivelAltura)
                ? selection.DesviadorPrimerNivelAltura
                : SelectiveSafetyDefaults.DesviadorPrimerNivelAltura;

            // El primer nivel conserva su contrato SELECTIVO: mide desde el primer troquel del poste, no desde el
            // larguero. Es la regla aprobada y no la toca esta corrección.
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var firstY = FirstTroquelY(catalog, postId, DynamicFrontGeometry.PostPeralte(structure, catalog, postId))
                + firstHeight;

            // LA AUTORIDAD: la elevación del larguero de ENTRADA de cada cama de este frente.
            var low = PushBackElevations.LowInsertions(system, catalog, front);
            var off = SelectiveSafetyGrid.OffCellKeys(selection.DesviadorOffCells);
            var cell = SelectiveDesviadorPlan.CellKey(
                selection, postIndex >= 0 ? postIndex : front.Index, structure.Fronts.Count);

            foreach (var level in DynamicFrontGeometry.LoadBeamLevels(structure, front))
            {
                if (levels != null && !levels.Contains(level.LevelNumber))
                {
                    continue;
                }

                if (off.Contains((cell, level.LevelNumber - 1)))
                {
                    continue;   // esta celda tiene el desviador desactivado
                }

                var y = level.LevelNumber <= 1
                    ? firstY
                    : (low.TryGetValue(level.LevelNumber, out var resolved) ? resolved : level.ExitElevation)
                      - SelectiveDesviadorPlan.BeamYOffset;

                result.Add(new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Safety,
                    PieceId = selection.ElementId,
                    BlockName = block,
                    View = View,
                    // En el marco de la cama el extremo BAJO es el arranque del frente, y la pieza va sin espejo.
                    // La mano y la posición en el mundo las pone la reflexión rígida de esa cama.
                    Insertion = new Point2D(front.StartX, y),
                    ConnectionAnchor = new Point2D(front.StartX, y),
                    MirroredX = false
                });

                if (longitud > 0.0)
                {
                    result[result.Count - 1].DynamicParameters[SelectiveRackDefaults.LengthParam] = longitud;
                }
            }

            return result;
        }

        /// <summary>
        /// Si una instancia ya colocada es un DESVIADOR, segun el catalogo. Lo usa el corte compuesto para retirar
        /// los que emitio el builder dinamico antes de reponerlos por cama: dos autoridades para la misma pieza es
        /// exactamente lo que esta correccion elimina.
        /// </summary>
        public static bool IsDiverter(HeaderBlockInstance instance, RackCatalog catalog)
        {
            if (instance == null || instance.Role != HeaderBlockRole.Safety)
            {
                return false;
            }

            return SelectiveSafetyFamilies
                .VariantsOfType(catalog?.SafetyElements, SelectiveSafetyDefaults.DesviadorType)
                .Any(entry => string.Equals(entry?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>El primer troquel del poste, que es desde donde mide el primer nivel (contrato Selectivo).</summary>
        private static double FirstTroquelY(RackCatalog catalog, string postId, double peralte)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(
                postId, SelectiveRackDefaults.PostBeamPoint, SelectiveRackDefaults.View);
            return SelectivePostGeometry.Resolve(entry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = peralte
            }).Y;
        }
    }
}
