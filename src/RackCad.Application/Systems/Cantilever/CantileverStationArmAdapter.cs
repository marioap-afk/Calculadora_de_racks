using System;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// THE single adapter from a station's arm TEMPLATE to the <see cref="CantileverArmDesign"/> that I-37B
    /// consumes.
    ///
    /// The template deliberately lacks two things the station owns: the SIDE and the
    /// <c>LowerColumnPunchIndex</c>. This is where they are supplied, and it is the only place — two sites
    /// composing the same design is how they drift apart, and the drift would be invisible because both
    /// would still produce a valid arm (ADR-0026, D2).
    ///
    /// It copies rather than aliases. A resolved design that shared its body object with the template would
    /// let a later edit of the template reach into an already-resolved level.
    /// </summary>
    public static class CantileverStationArmAdapter
    {
        /// <summary>
        /// Builds the arm design for one cell.
        /// </summary>
        /// <param name="template">The effective template of the cell — its override or the station default.</param>
        /// <param name="side">Which side this physical arm is on. There is no <c>Both</c> (ADR-0025, D1).</param>
        /// <param name="lowerColumnPunchIndex">
        /// The index the level layout computed, BASE ZERO. Never read from the template.
        /// </param>
        public static CantileverArmDesign ToArmDesign(
            CantileverArmTemplateDesign template, CantileverArmSide side, int lowerColumnPunchIndex)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (lowerColumnPunchIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lowerColumnPunchIndex), lowerColumnPunchIndex,
                    "El indice del troquel inferior es base cero y no puede ser negativo.");
            }

            var mount = template.MountingPlate ?? new CantileverArmMountingPlateTemplateDesign();

            return new CantileverArmDesign
            {
                Side = side,
                Body = template.Body?.DeepCopy() ?? new CantileverArmBodyDesign(),
                MountingPlate = new CantileverArmMountingPlateDesign
                {
                    Thickness = mount.Thickness,
                    LowerColumnPunchIndex = lowerColumnPunchIndex,
                    VerticalPunchCount = mount.VerticalPunchCount,
                    // Carried across as-is, INCLUDING null. I-37B has no approved default for it and rejects a
                    // design that omits it; inventing one here would launder a missing value into a resolved
                    // one, and the rejection is the whole point.
                    VerticalEndOffset = mount.VerticalEndOffset
                },
                EndPlate = template.EndPlate?.DeepCopy() ?? new CantileverArmEndPlateDesign()
            };
        }
    }
}
