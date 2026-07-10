using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Inverse of <see cref="RackFrameConfigurationFactory"/>: snapshots a resolved
    /// <see cref="RackFrameConfiguration"/> back into a reusable <see cref="RackFrameTemplate"/>. Only the
    /// STANDARD shape is captured — horizontal profiles/quantities, the diagonal profile, connection points,
    /// base plate and post — so the result stays the parametric baseline the Build factory regenerates from.
    /// Per-panel exceptions are intentionally NOT stored: they are per-project overrides, not part of a
    /// template (the standard form is what gets reused between projects).
    /// </summary>
    public static class RackFrameTemplateFactory
    {
        public static RackFrameTemplate FromConfiguration(RackFrameConfiguration config, string id, string name)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Prefer the first STANDARD panel (the ones that carry the template's diagonal); fall back to the
            // first panel so a config made only of exceptions still yields a diagonal and connection points.
            var panel = config.BracingPanels?.FirstOrDefault(p => p != null && p.IsStandard)
                        ?? config.BracingPanels?.FirstOrDefault(p => p != null);

            var horizontals = new List<TemplateHorizontal>();

            if (config.Horizontals != null)
            {
                foreach (var horizontal in config.Horizontals)
                {
                    if (horizontal == null)
                    {
                        continue;
                    }

                    horizontals.Add(new TemplateHorizontal
                    {
                        Elevation = horizontal.Elevation,
                        Profile = horizontal.ProfileId,
                        Quantity = horizontal.Quantity > 0 ? horizontal.Quantity : 1
                    });
                }
            }

            if (horizontals.Count == 0)
            {
                // The Build factory reads Horizontals[0].Profile, so a template needs at least one horizontal.
                // Fall back to the captured diagonal profile (horizontals and diagonals share the truss catalog)
                // so the template stays self-describing; empty ids resolve via defaults.json when built.
                horizontals.Add(new TemplateHorizontal
                {
                    Elevation = 0.0,
                    Profile = panel?.DiagonalProfileId,
                    Quantity = 1
                });
            }

            return new RackFrameTemplate
            {
                Id = id,
                Name = name,
                DefaultHeight = config.Height,
                DefaultDepth = config.Depth,
                Horizontals = horizontals,
                DefaultArrangement = panel?.Arrangement ?? BracingPattern.SingleDiagonal,
                DiagonalProfile = panel?.DiagonalProfileId,
                BraceStartConnectionPoint = panel?.StartConnectionPointId,
                BraceEndConnectionPoint = panel?.EndConnectionPointId,
                BasePlate = config.LeftBasePlate?.PlateCatalogId,
                Post = config.LeftPost?.PostCatalogId
            };
        }
    }
}
