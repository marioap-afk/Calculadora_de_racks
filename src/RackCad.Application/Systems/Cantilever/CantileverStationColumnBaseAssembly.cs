using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The column and its base or bases, composed for a station.
    ///
    /// The cardinality is the whole content of this type: **exactly one** column, **exactly one** column
    /// bottom plate, and **one or two** base sides. A double-faced station is not two of these and it is not
    /// two <see cref="CantileverColumnBaseAssembly"/> instances — it is this, with two sides (ADR-0026, D1).
    ///
    /// It composes, and never re-derives. Every dimension comes from the single I-37A resolve behind it, and
    /// the punches keep the datums that resolve produced.
    /// </summary>
    public sealed class CantileverStationColumnBaseAssembly
    {
        private CantileverStationColumnBaseAssembly(
            CantileverStationFaceMode faceMode,
            CantileverStructuralMemberPlan column,
            CantileverPlatePlan columnBottomPlate,
            IReadOnlyList<CantileverStationBaseSide> sides,
            IReadOnlyList<CantileverPunchPlan> columnConnectionPunches,
            IReadOnlyList<CantileverPunchPlan> columnRegularPunches,
            IReadOnlyList<CantileverPunchPlan> columnBottomPlatePunches,
            CantileverColumnBaseConnectionPattern pattern,
            double columnHeight,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            FaceMode = faceMode;
            Column = column;
            ColumnBottomPlate = columnBottomPlate;
            Sides = sides;
            ColumnConnectionPunches = columnConnectionPunches;
            ColumnRegularPunches = columnRegularPunches;
            ColumnBottomPlatePunches = columnBottomPlatePunches;
            Pattern = pattern;
            ColumnHeight = columnHeight;
            Diagnostics = diagnostics;
        }

        public CantileverStationFaceMode FaceMode { get; }

        /// <summary>The one column. Singular by type, not by convention.</summary>
        public CantileverStructuralMemberPlan Column { get; }

        /// <summary>The one column bottom plate. Both faces stand on it.</summary>
        public CantileverPlatePlan ColumnBottomPlate { get; }

        /// <summary>One side for a single station, two for a double one. Positive first when both exist.</summary>
        public IReadOnlyList<CantileverStationBaseSide> Sides { get; }

        /// <summary>
        /// The connection punches on the COLUMN face. One set, shared by both sides.
        ///
        /// One set and not one per side: the holes are in the column, and in a double station the same bolt
        /// line serves both rear plates. Duplicating them would put two holes where the drill makes one.
        /// </summary>
        public IReadOnlyList<CantileverPunchPlan> ColumnConnectionPunches { get; }

        /// <summary>The regular grid the arms of every level select from. One set, whatever the face mode.</summary>
        public IReadOnlyList<CantileverPunchPlan> ColumnRegularPunches { get; }

        public IReadOnlyList<CantileverPunchPlan> ColumnBottomPlatePunches { get; }

        /// <summary>The shared column–base authority of I-37A, carried across unchanged.</summary>
        public CantileverColumnBaseConnectionPattern Pattern { get; }

        /// <summary>The resolved height the column was built with.</summary>
        public double ColumnHeight { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        public bool IsBlocked => Diagnostics.Any(d => d.IsBlocking);

        /// <summary>Every structural member: the column first, then one base per side.</summary>
        public IReadOnlyList<CantileverStructuralMemberPlan> Members =>
            new[] { Column }.Concat(Sides.Select(s => s.Member)).ToList();

        /// <summary>Every flat plate: the column's bottom plate, then each side's two plates.</summary>
        public IReadOnlyList<CantileverPlatePlan> Plates =>
            new[] { ColumnBottomPlate }.Concat(Sides.SelectMany(s => s.Plates)).ToList();

        public IReadOnlyList<CantileverGussetPlan> Gussets => Sides.Select(s => s.Gusset).ToList();

        public IReadOnlyList<CantileverPunchPlan> AllPunches =>
            Sides.SelectMany(s => s.RearPlatePunches)
                .Concat(ColumnConnectionPunches)
                .Concat(ColumnRegularPunches)
                .Concat(ColumnBottomPlatePunches)
                .ToList();

        public CantileverEnvelope3D Envelope() =>
            Sides
                .Select(s => s.Envelope())
                .Aggregate(
                    ColumnBottomPlate.Envelope()
                        .Union(CantileverEnvelope3D.FromPoints(new[] { Column.Start, Column.End })),
                    (acc, e) => acc.Union(e));

        /// <summary>
        /// Composes the station's column–base from ONE resolved I-37A sub-assembly.
        /// </summary>
        /// <param name="assembly">The single resolve. Both sides come from it.</param>
        /// <param name="faceMode">One face or two.</param>
        /// <param name="activeSides">
        /// The sides that carry a base — <c>CantileverStationDesign.ActiveSides()</c>. Passed in rather than
        /// re-derived so "which cells exist" keeps a single authority.
        /// </param>
        public static CantileverStationColumnBaseAssembly Compose(
            CantileverColumnBaseAssembly assembly,
            CantileverStationFaceMode faceMode,
            IReadOnlyList<CantileverArmSide> activeSides)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (activeSides == null)
            {
                throw new ArgumentNullException(nameof(activeSides));
            }

            if (activeSides.Count == 0)
            {
                throw new ArgumentException("Una estacion necesita al menos un lado activo.", nameof(activeSides));
            }

            if (activeSides.Distinct().Count() != activeSides.Count)
            {
                throw new ArgumentException("Un lado activo no puede repetirse.", nameof(activeSides));
            }

            var expected = faceMode == CantileverStationFaceMode.Double ? 2 : 1;

            if (activeSides.Count != expected)
            {
                throw new ArgumentException(
                    "El modo '" + faceMode + "' exige " + expected + " lado(s) activo(s); se recibieron " +
                    activeSides.Count + ".",
                    nameof(activeSides));
            }

            var owner = CantileverPieceTokens.StationOwner;

            var sides = activeSides
                .OrderBy(s => s)
                .Select(s => CantileverStationBaseSideResolver.Compose(assembly, s))
                .ToList();

            return new CantileverStationColumnBaseAssembly(
                faceMode,
                CantileverStructuralMemberPlan.Create(
                    CantileverPieceId.Create(owner, CantileverPieceTokens.Column),
                    assembly.Column.Role,
                    owner,
                    assembly.Column.Placement),
                CantileverPlatePlan.Create(
                    CantileverPieceId.Create(owner, CantileverPieceTokens.ColumnBottomPlate),
                    assembly.ColumnBottomPlate.Kind,
                    assembly.ColumnBottomPlate.Thickness,
                    assembly.ColumnBottomPlate.Normal,
                    assembly.ColumnBottomPlate.NearOffset,
                    assembly.ColumnBottomPlate.Outline),
                sides,
                ReOwn(assembly.ColumnConnectionPunches, owner, CantileverPieceTokens.ColumnConnectionPunch),
                ReOwn(assembly.ColumnRegularPunches, owner, CantileverPieceTokens.ColumnRegularPunch),
                ReOwn(assembly.ColumnBottomPlatePunches, owner, CantileverPieceTokens.ColumnBottomPlatePunch),
                assembly.Pattern,
                assembly.Column.GeometricLength,
                assembly.Diagnostics.ToList());
        }

        private static IReadOnlyList<CantileverPunchPlan> ReOwn(
            IReadOnlyList<CantileverPunchPlan> punches, string owner, string token)
        {
            var id = CantileverPieceId.Create(owner, token);

            return punches
                .Select((p, i) => new CantileverPunchPlan(id.At(i), p.Surface, p.Centre, p.Datum))
                .ToList();
        }

        /// <summary>Deterministic fingerprint.</summary>
        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "mode={0};col={1}@{2:0.######};pbot={3:0.######};sides={4};pch={5},{6},{7}",
            FaceMode,
            Column.SectionId.Value,
            Column.GeometricLength,
            ColumnBottomPlate.Thickness,
            string.Join("+", Sides.Select(s => s.Signature())),
            ColumnConnectionPunches.Count,
            ColumnRegularPunches.Count,
            ColumnBottomPlatePunches.Count);

        public override string ToString() =>
            "StationColumnBase " + FaceMode + " sides=" + Sides.Count;
    }
}
