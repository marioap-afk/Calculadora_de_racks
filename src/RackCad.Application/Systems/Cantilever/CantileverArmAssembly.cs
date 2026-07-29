using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// A resolved arm: its body, the plate that bolts it to the column, that plate's punches, an optional end
    /// plate, and the pattern of column punches it selected.
    ///
    /// Immutable, and the only thing downstream consumes. A BLOCKED assembly still exists and carries its
    /// diagnostics: returning null would force every caller to guess why, and the reason is what the user
    /// needs.
    ///
    /// It carries NO station or level identity. The owner token comes from the caller, and the future station
    /// is what will supply a deterministic one.
    /// </summary>
    public sealed class CantileverArmAssembly
    {
        private CantileverArmAssembly(
            string owner,
            CantileverArmSide side,
            CantileverArmBodyPlan body,
            CantileverPlatePlan mountingPlate,
            IReadOnlyList<CantileverPunchPlan> mountingPunches,
            CantileverPlatePlan endPlate,
            CantileverArmEndPlateMode endPlateMode,
            CantileverArmColumnConnectionPattern connectionPattern,
            IReadOnlyList<CantileverDiagnostic> diagnostics,
            CantileverEnvelope3D? envelope)
        {
            Owner = owner;
            Side = side;
            Body = body;
            MountingPlate = mountingPlate;
            MountingPunches = mountingPunches;
            EndPlate = endPlate;
            EndPlateMode = endPlateMode;
            ConnectionPattern = connectionPattern;
            Diagnostics = diagnostics;
            Envelope = envelope;
        }

        /// <summary>The token the caller supplied. One string, not a back-pointer to a mutable object.</summary>
        public string Owner { get; }

        public CantileverArmSide Side { get; }

        /// <summary>The profiles. Null only when the assembly is blocked.</summary>
        public CantileverArmBodyPlan Body { get; }

        /// <summary>The plate bolted to the column. Null only when the assembly is blocked.</summary>
        public CantileverPlatePlan MountingPlate { get; }

        /// <summary>
        /// The punches of the MOUNTING PLATE only. The column's own punches are NOT duplicated here: they
        /// belong to the column assembly, and the coincidence is shown by datum (ADR-0025, D5).
        /// </summary>
        public IReadOnlyList<CantileverPunchPlan> MountingPunches { get; }

        /// <summary>The cap or stop, or null when the mode is <c>None</c>.</summary>
        public CantileverPlatePlan EndPlate { get; }

        public CantileverArmEndPlateMode EndPlateMode { get; }

        /// <summary>Which column punches this arm selected. Null only when the assembly is blocked.</summary>
        public CantileverArmColumnConnectionPattern ConnectionPattern { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        /// <summary>Conservative world envelope. Null when the assembly is blocked.</summary>
        public CantileverEnvelope3D? Envelope { get; }

        public bool IsBlocked => Diagnostics.Any(d => d.IsBlocking);

        /// <summary>Every structural member of this arm, in deterministic order.</summary>
        public IReadOnlyList<CantileverStructuralMemberPlan> Members =>
            Body?.Members ?? (IReadOnlyList<CantileverStructuralMemberPlan>)Array.Empty<CantileverStructuralMemberPlan>();

        /// <summary>Every plate of this arm, in deterministic order. One or two.</summary>
        public IReadOnlyList<CantileverPlatePlan> Plates =>
            MountingPlate == null
                ? (IReadOnlyList<CantileverPlatePlan>)Array.Empty<CantileverPlatePlan>()
                : EndPlate == null
                    ? new[] { MountingPlate }
                    : new[] { MountingPlate, EndPlate };

        internal static CantileverArmAssembly Blocked(
            string owner, CantileverArmSide side, IEnumerable<CantileverDiagnostic> diagnostics) =>
            new CantileverArmAssembly(
                owner, side, null, null, Array.Empty<CantileverPunchPlan>(), null,
                CantileverArmEndPlateMode.None, null, diagnostics.ToList(), null);

        internal static CantileverArmAssembly Create(
            string owner,
            CantileverArmSide side,
            CantileverArmBodyPlan body,
            CantileverPlatePlan mountingPlate,
            IReadOnlyList<CantileverPunchPlan> mountingPunches,
            CantileverPlatePlan endPlate,
            CantileverArmEndPlateMode endPlateMode,
            CantileverArmColumnConnectionPattern connectionPattern,
            IEnumerable<CantileverDiagnostic> diagnostics)
        {
            var envelope = body.Envelope.Union(mountingPlate.Envelope());

            if (endPlate != null)
            {
                envelope = envelope.Union(endPlate.Envelope());
            }

            // Piece ids must be unique inside the arm. Same reason CantileverColumnBaseAssembly checks it:
            // resolving to "the first one" hides a disagreement instead of reporting it.
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var every = body.Members.Select(m => m.Id)
                .Concat(new[] { mountingPlate.Id })
                .Concat(endPlate == null ? Enumerable.Empty<CantileverPieceId>() : new[] { endPlate.Id })
                .Concat(mountingPunches.Select(p => p.Id));

            foreach (var id in every)
            {
                if (!ids.Add(id.Value))
                {
                    throw new InvalidOperationException(
                        "El id de pieza '" + id.Value + "' esta duplicado en el brazo.");
                }
            }

            return new CantileverArmAssembly(
                owner, side, body, mountingPlate, mountingPunches, endPlate, endPlateMode,
                connectionPattern, diagnostics.ToList(), envelope);
        }

        /// <summary>
        /// Deterministic fingerprint. Same input, same signature — which is what makes a regression visible
        /// without pinning a picture.
        /// </summary>
        public string Signature()
        {
            if (IsBlocked)
            {
                return "BLOCKED;" + string.Join("|",
                    Diagnostics.Where(d => d.IsBlocking).Select(d => d.Code).OrderBy(c => c, StringComparer.Ordinal));
            }

            var parts = new List<string>
            {
                "owner=" + Owner,
                "body=" + Body.Signature(),
                "conn=" + ConnectionPattern.Signature(),
                "mount=" + Format(MountingPlate.Thickness) + "@" + Format(MountingPlate.NearOffset),
                "end=" + EndPlateMode + (EndPlate == null
                    ? string.Empty
                    : ":" + Format(EndPlate.Thickness) + "@" + Format(EndPlate.NearOffset)),
                "punches=" + MountingPunches.Count.ToString(CultureInfo.InvariantCulture),
                "env=" + Format(Envelope.Value.MinX) + "," + Format(Envelope.Value.MinY) + "," +
                    Format(Envelope.Value.MinZ) + ".." + Format(Envelope.Value.MaxX) + "," +
                    Format(Envelope.Value.MaxY) + "," + Format(Envelope.Value.MaxZ)
            };

            return string.Join(";", parts);
        }

        private static string Format(double value) =>
            Math.Round(value, 6, MidpointRounding.AwayFromZero).ToString("0.######", CultureInfo.InvariantCulture);

        public override string ToString() =>
            Owner + " " + Side + " " + (Body?.ToString() ?? "BLOCKED");
    }
}
