using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// A resolved column–base sub-assembly: two members, four flat pieces, four punch sets and the pattern
    /// they share.
    ///
    /// Immutable, and the ONLY thing downstream consumes. Views, BOM and preview will derive from this and
    /// never from the design, so there is one place where "what does this cantilever actually consist of"
    /// is answered.
    ///
    /// A BLOCKED assembly still exists — it carries its diagnostics and nothing else. Returning null on
    /// failure would force every caller to guess why, and the reason is exactly what the user needs.
    /// </summary>
    public sealed class CantileverColumnBaseAssembly
    {
        private CantileverColumnBaseAssembly(
            CantileverStructuralMemberPlan column,
            CantileverStructuralMemberPlan basePiece,
            CantileverPlatePlan baseFrontPlate,
            CantileverPlatePlan baseRearPlate,
            CantileverPlatePlan columnBottomPlate,
            CantileverGussetPlan gusset,
            IReadOnlyList<CantileverPunchPlan> rearPlatePunches,
            IReadOnlyList<CantileverPunchPlan> columnConnectionPunches,
            IReadOnlyList<CantileverPunchPlan> columnRegularPunches,
            IReadOnlyList<CantileverPunchPlan> columnBottomPlatePunches,
            CantileverColumnBaseConnectionPattern pattern,
            IReadOnlyList<CantileverDiagnostic> diagnostics,
            CantileverEnvelope3D? envelope)
        {
            Column = column;
            Base = basePiece;
            BaseFrontPlate = baseFrontPlate;
            BaseRearPlate = baseRearPlate;
            ColumnBottomPlate = columnBottomPlate;
            Gusset = gusset;
            RearPlatePunches = rearPlatePunches;
            ColumnConnectionPunches = columnConnectionPunches;
            ColumnRegularPunches = columnRegularPunches;
            ColumnBottomPlatePunches = columnBottomPlatePunches;
            Pattern = pattern;
            Diagnostics = diagnostics;
            Envelope = envelope;
        }

        public CantileverStructuralMemberPlan Column { get; }

        public CantileverStructuralMemberPlan Base { get; }

        public CantileverPlatePlan BaseFrontPlate { get; }

        public CantileverPlatePlan BaseRearPlate { get; }

        public CantileverPlatePlan ColumnBottomPlate { get; }

        public CantileverGussetPlan Gusset { get; }

        /// <summary>Punches of the base's rear plate. Same datums as <see cref="ColumnConnectionPunches"/>.</summary>
        public IReadOnlyList<CantileverPunchPlan> RearPlatePunches { get; }

        /// <summary>Punches of the column's connecting face. Same datums as <see cref="RearPlatePunches"/>.</summary>
        public IReadOnlyList<CantileverPunchPlan> ColumnConnectionPunches { get; }

        /// <summary>The column's regular grid, above the connection region.</summary>
        public IReadOnlyList<CantileverPunchPlan> ColumnRegularPunches { get; }

        public IReadOnlyList<CantileverPunchPlan> ColumnBottomPlatePunches { get; }

        /// <summary>The shared authority. Null only when the assembly is blocked.</summary>
        public CantileverColumnBaseConnectionPattern Pattern { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        /// <summary>Conservative world envelope. Null when the assembly is blocked.</summary>
        public CantileverEnvelope3D? Envelope { get; }

        /// <summary>True when something stopped the resolution. A blocked assembly is never drawn or counted.</summary>
        public bool IsBlocked => Diagnostics.Any(d => d.IsBlocking);

        /// <summary>Every structural member, in a deterministic order: column first, then base.</summary>
        public IReadOnlyList<CantileverStructuralMemberPlan> Members =>
            Column == null || Base == null
                ? (IReadOnlyList<CantileverStructuralMemberPlan>)Array.Empty<CantileverStructuralMemberPlan>()
                : new[] { Column, Base };

        /// <summary>Every punch of the sub-assembly, in a deterministic order.</summary>
        public IReadOnlyList<CantileverPunchPlan> AllPunches =>
            RearPlatePunches
                .Concat(ColumnConnectionPunches)
                .Concat(ColumnRegularPunches)
                .Concat(ColumnBottomPlatePunches)
                .ToList();

        internal static CantileverColumnBaseAssembly Blocked(IEnumerable<CantileverDiagnostic> diagnostics) =>
            new CantileverColumnBaseAssembly(
                null, null, null, null, null, null,
                Array.Empty<CantileverPunchPlan>(),
                Array.Empty<CantileverPunchPlan>(),
                Array.Empty<CantileverPunchPlan>(),
                Array.Empty<CantileverPunchPlan>(),
                null,
                diagnostics.ToList(),
                null);

        internal static CantileverColumnBaseAssembly Create(
            CantileverStructuralMemberPlan column,
            CantileverStructuralMemberPlan basePiece,
            CantileverPlatePlan baseFrontPlate,
            CantileverPlatePlan baseRearPlate,
            CantileverPlatePlan columnBottomPlate,
            CantileverGussetPlan gusset,
            IReadOnlyList<CantileverPunchPlan> rearPlatePunches,
            IReadOnlyList<CantileverPunchPlan> columnConnectionPunches,
            IReadOnlyList<CantileverPunchPlan> columnRegularPunches,
            IReadOnlyList<CantileverPunchPlan> columnBottomPlatePunches,
            CantileverColumnBaseConnectionPattern pattern,
            IEnumerable<CantileverDiagnostic> diagnostics)
        {
            var envelope = baseFrontPlate.Envelope()
                .Union(baseRearPlate.Envelope())
                .Union(columnBottomPlate.Envelope())
                .Union(gusset.Envelope())
                .Union(CantileverEnvelope3D.FromPoints(new[] { column.Start, column.End, basePiece.Start, basePiece.End }));

            // The ids of every piece must be unique inside the assembly. Same reason
            // StructuralSectionCatalog.Create rejects a duplicate id: resolving to "the first one" hides a
            // disagreement instead of reporting it.
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var everyId = new[]
                {
                    column.Id, basePiece.Id, baseFrontPlate.Id, baseRearPlate.Id, columnBottomPlate.Id, gusset.Id
                }
                .Concat(rearPlatePunches.Select(p => p.Id))
                .Concat(columnConnectionPunches.Select(p => p.Id))
                .Concat(columnRegularPunches.Select(p => p.Id))
                .Concat(columnBottomPlatePunches.Select(p => p.Id));

            foreach (var id in everyId)
            {
                if (!ids.Add(id.Value))
                {
                    throw new InvalidOperationException("El id de pieza '" + id.Value + "' esta duplicado en el subensamble.");
                }
            }

            return new CantileverColumnBaseAssembly(
                column, basePiece, baseFrontPlate, baseRearPlate, columnBottomPlate, gusset,
                rearPlatePunches, columnConnectionPunches, columnRegularPunches, columnBottomPlatePunches,
                pattern, diagnostics.ToList(), envelope);
        }

        /// <summary>
        /// Deterministic fingerprint of the whole sub-assembly.
        ///
        /// Same input, same signature — which is what makes a regression visible without pinning a picture.
        /// Values are rounded to six decimals, the resolution I-36B's plan signature uses, so that a change
        /// below drawing tolerance does not move the pin.
        /// </summary>
        public string Signature()
        {
            if (IsBlocked)
            {
                return "BLOCKED;" + string.Join("|", Diagnostics.Where(d => d.IsBlocking).Select(d => d.Code).OrderBy(c => c, StringComparer.Ordinal));
            }

            var parts = new List<string>
            {
                "col=" + Column.SectionId.Value + "@" + Format(Column.GeometricLength),
                "base=" + Base.SectionId.Value + "@" + Format(Base.GeometricLength),
                "pattern=" + Pattern.Signature(),
                "plates=" + Format(BaseFrontPlate.Thickness) + "," + Format(BaseRearPlate.Thickness) + "," +
                    Format(ColumnBottomPlate.Thickness) + "," + Format(Gusset.Thickness),
                "gusset=" + Format(Gusset.VerticalLeg) + "x" + Format(Gusset.HorizontalLeg),
                "punches=" + RearPlatePunches.Count + "," + ColumnConnectionPunches.Count + "," +
                    ColumnRegularPunches.Count + "," + ColumnBottomPlatePunches.Count,
                "env=" + Format(Envelope.Value.MinX) + "," + Format(Envelope.Value.MinY) + "," + Format(Envelope.Value.MinZ) +
                    ".." + Format(Envelope.Value.MaxX) + "," + Format(Envelope.Value.MaxY) + "," + Format(Envelope.Value.MaxZ)
            };

            return string.Join(";", parts);
        }

        private static string Format(double value) =>
            Math.Round(value, 6, MidpointRounding.AwayFromZero).ToString("0.######", CultureInfo.InvariantCulture);
    }
}
