using System;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems.Shared;

namespace RackCad.Plugin.Drawing
{
    /// <summary>
    /// Copies AutoCAD placement values into the neutral Application input. It deliberately performs no
    /// classification, projection, acceptance decision or remediation at the Autodesk boundary.
    /// </summary>
    internal static class RackSourcePlacementCaptureAdapter
    {
        internal static RackSourcePlacementInput Capture(
            BlockReference reference,
            BlockTableRecord definition)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            var position = reference.Position;
            var scale = reference.ScaleFactors;
            var normal = reference.Normal;
            var origin = definition.Origin;

            return new RackSourcePlacementInput(
                position.X,
                position.Y,
                position.Z,
                reference.Rotation,
                scale.X,
                scale.Y,
                scale.Z,
                normal.X,
                normal.Y,
                normal.Z,
                origin.X,
                origin.Y,
                origin.Z);
        }
    }
}
