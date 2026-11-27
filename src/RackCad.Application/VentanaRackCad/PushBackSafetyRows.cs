using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// The rows of the editable «Seguridad por poste» of a Push Back (T5 pb, PB-S03), counted as the BOM of Push Back counts them: the botas of
    /// each side from <see cref="PushBackBootPlan"/> (the BOM replaces the shared ones by them), the desviadores from
    /// <see cref="PushBackDiverterPlan"/> (by identity: line, level and aisle), the protectores laterales from the shared planta and each end of
    /// the defense as the planta draws it (the type of each face, its length and whether the line has that face). The column of the desviador
    /// grid is the post itself (PB-002: the authority reads its cells by post).
    /// </summary>
    public static class PushBackSafetyRows
    {
        /// <param name="levelsPerPost">The levels of the desviador grid at each post (the tallest side when the rack has two).</param>
        public static IReadOnlyList<RackCadSafetyRow> Build(PushBackSystem system, RackCatalog catalog, IReadOnlyList<int> levelsPerPost)
        {
            var structure = system?.Structure;
            if (structure == null || catalog == null) return Array.Empty<RackCadSafetyRow>();
            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            if (layout?.PostPositions == null || layout.PostPositions.Count == 0) return Array.Empty<RackCadSafetyRow>();

            var posts = layout.PostPositions.Count;
            var summary = DynamicPostSafetySummary.Build(structure, catalog);
            var boots = PushBackBootPlan.Resolve(system, catalog);
            var diverters = PushBackDiverterPlan.Resolve(system, catalog);
            var defensa = SelectiveSafetyFamilies.SelectedOfType(structure.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DefensaType);
            var nearId = defensa == null ? null : DynamicDefenseFaces.ElementIdFor(defensa, farEnd: false);
            var farId = defensa == null ? null : DynamicDefenseFaces.ElementIdFor(defensa, farEnd: true);
            var levels = levelsPerPost ?? Array.Empty<int>();

            var rows = new List<RackCadSafetyRow>();
            for (var post = 0; post < posts; post++)
            {
                var exists = DynamicFrontActivation.BoundaryExists(structure, post);
                var setting = DynamicForkliftDefensePlan.ForSelection(defensa, post, posts);
                var count = post < levels.Count ? Math.Max(0, levels[post]) : 0;
                rows.Add(new RackCadSafetyRow
                {
                    Post = "P" + (post + 1),
                    Kind = post == 0 || post == posts - 1 ? "extremo" : "intermedio",
                    FramePost = post,
                    IsEndPost = post == 0 || post == posts - 1,
                    Boots = boots.Count(b => b.PostIndex == post && b.Side == PushBackSide.A),
                    BootsB = boots.Count(b => b.PostIndex == post && b.Side == PushBackSide.B),
                    SideGuards = post < summary.Count ? summary[post].SideGuards : 0,
                    DeflectorKey = exists && count > 0 ? post : (int?)null,
                    DeflectorWriteKey = exists && count > 0 ? post : (int?)null,
                    DeflectorLevels = exists ? count : 0,
                    Deflectors = diverters.Count(d => d.PostLine == post),
                    DefenseExit = defensa != null && exists && setting.DrawsExit && !string.IsNullOrWhiteSpace(nearId)
                                  && DynamicDefenseFaces.HasFace(structure, post, farEnd: false),
                    DefenseEntrance = defensa != null && exists && setting.DrawsEntrance && !string.IsNullOrWhiteSpace(farId)
                                      && DynamicDefenseFaces.HasFace(structure, post, farEnd: true),
                });
            }

            return rows;
        }
    }
}
