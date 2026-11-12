#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T2A (D-10, INV-26, D-T2-03) — the comparator and the deep copier of the pilot state, injected into the draft
    /// core. Dirty is NEVER decided from raw JSON: it is decided here, over the EDITABLE content of the state.
    ///
    /// <para>
    /// The classification of every instance field of the graph is closed by a reflection census in the tests:
    /// <b>Editable</b> content (what <c>Snapshot()</c> carries, the shape topology and every rack-wide input) is compared
    /// exactly (a double by value) and copied deeply; the <b>selection</b> (selected cell, active side) is ignored by the
    /// comparator and carried by the copier; the <b>module and tail authority</b> (working baseline, module session and
    /// commit, module signature, last reconciliation, dormant tail) is ignored and NOT copied — the pilot does not edit
    /// it and it always comes from the base; the <b>origin</b> is ignored and shared because it is an immutable string.
    /// </para>
    /// </summary>
    public sealed class PushBackPilotStateComparer : IEqualityComparer<PushBackPilotDraftState>
    {
        public static PushBackPilotStateComparer Instance { get; } = new PushBackPilotStateComparer();

        public bool Equals(PushBackPilotDraftState? x, PushBackPilotDraftState? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return EditorsEqual(x.Editor, y.Editor) && InputsEqual(x.Inputs, y.Inputs);
        }

        public int GetHashCode(PushBackPilotDraftState obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var hash = new HashCode();
            hash.Add(obj.Editor.SideBPresent);
            hash.Add(obj.Editor.Gap);
            hash.Add(obj.Editor.SideA.Structure.Count);
            hash.Add(obj.Editor.SideB.Structure.Count);
            hash.Add(obj.Inputs.PalletsDeep);
            hash.Add(obj.Inputs.Annotations?.AnnotationScale ?? 0.0);
            hash.Add(obj.Inputs.SafetySelections.Count);
            return hash.ToHashCode();
        }

        // ------------------------------------------------------------------------------------------------ the copier

        /// <summary>
        /// A deep copy equal for this comparer, with the same selection, that shares no mutable object with the original
        /// and does NOT copy the module and tail authority: the copy is born without baseline, without modules and without
        /// tail. The origin is shared because it is an immutable string.
        /// </summary>
        public static PushBackPilotDraftState Copy(PushBackPilotDraftState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return new PushBackPilotDraftState(CopyEditor(state.Editor), CopyInputs(state.Inputs), state.Origin);
        }

        private static PushBackCompositeEditorState CopyEditor(PushBackCompositeEditorState source)
        {
            // Snapshot() and Restore() are the deep copies the editor already owns for its own rollback. A fresh state is
            // used as the target, so nothing of the authority of the source (baseline, module session, tail) is carried.
            var snapshot = source.Snapshot();
            var copy = new PushBackCompositeEditorState();
            copy.Restore(snapshot);

            // Restore() does not carry the multi-side selection: put it back, then put each side's own selection back
            // because changing the selection mirrors the primary cell onto the other side.
            if (copy.ActiveSelection != source.ActiveSelection)
            {
                copy.SetActiveSelection(source.ActiveSelection);
                copy.SideA.Restore(snapshot.SideA);
                copy.SideB.Restore(snapshot.SideB);
            }

            copy.SetActiveSide(source.ActiveSide);
            return copy;
        }

        private static PushBackEditorInputs CopyInputs(PushBackEditorInputs source)
        {
            var copy = new PushBackEditorInputs
            {
                Pallet = CopyPallet(source.Pallet),
                PalletsDeep = source.PalletsDeep,
                PostCatalogId = source.PostCatalogId,
                PostPeralte = source.PostPeralte,
                PalletTolerance = source.PalletTolerance,
                FirstLevelDatum = source.FirstLevelDatum,
                BeamDepth = source.BeamDepth,
                Annotations = CopyAnnotations(source.Annotations),
                ManualHeaderHeightOverride = source.ManualHeaderHeightOverride,
                DerivedPostReinforced = source.DerivedPostReinforced,
                DerivedPostReinforcementHeight = source.DerivedPostReinforcementHeight,
                DerivedPostHeight = source.DerivedPostHeight,
                SeparatorCountOverride = source.SeparatorCountOverride,
                SeparatorSpacingOverride = source.SeparatorSpacingOverride,
            };

            foreach (var safety in source.SafetySelections)
            {
                copy.SafetySelections.Add(safety?.DeepCopy()!);
            }

            return copy;
        }

        private static PalletSpecification CopyPallet(PalletSpecification pallet)
            => pallet == null
                ? null!
                : new PalletSpecification(pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, pallet.WeightUnit);

        private static DynamicAnnotationOptions CopyAnnotations(DynamicAnnotationOptions options)
            => options == null
                ? null!
                : new DynamicAnnotationOptions
                {
                    NumberFronts = options.NumberFronts,
                    NumberLevels = options.NumberLevels,
                    DrawRackName = options.DrawRackName,
                    AnnotationScale = options.AnnotationScale,
                    Dimensions = options.Dimensions,
                    DimensionViews = options.DimensionViews,
                    DimensionStyle = options.DimensionStyle,
                };

        // ------------------------------------------------------------------------------------------ editable content

        private static bool EditorsEqual(PushBackCompositeEditorState x, PushBackCompositeEditorState y)
        {
            var a = x.Snapshot();
            var b = y.Snapshot();
            return a.SideBPresent == b.SideBPresent
                   && a.Gap.Equals(b.Gap)
                   && a.CentralSeparator == b.CentralSeparator
                   && a.StructureOverrideA == b.StructureOverrideA
                   && a.StructureOverrideB == b.StructureOverrideB
                   && a.DefaultTopology == b.DefaultTopology
                   && a.DefaultDirection == b.DefaultDirection
                   && a.SideBEverDeclared == b.SideBEverDeclared
                   && TopologiesEqual(a.Topologies, b.Topologies)
                   && SidesEqual(a.SideA, b.SideA)
                   && SidesEqual(a.SideB, b.SideB);
        }

        private static bool TopologiesEqual(IReadOnlyList<PushBackTopologyCell> a, IReadOnlyList<PushBackTopologyCell> b)
        {
            if (a.Count != b.Count) return false;
            for (var index = 0; index < a.Count; index++)
            {
                var left = a[index];
                var right = b[index];
                if (left.Frente != right.Frente
                    || left.Level != right.Level
                    || left.Topology != right.Topology
                    || left.Direction != right.Direction
                    || left.CorridaDepth != right.CorridaDepth)
                {
                    return false;
                }
            }

            return true;
        }

        // The selection of the snapshot (primary cell and selected cells) is deliberately not read.
        private static bool SidesEqual(PushBackEditorSnapshot a, PushBackEditorSnapshot b)
            => a.RearTopeSaque.Equals(b.RearTopeSaque)
               && string.Equals(a.RearTopePieceId, b.RearTopePieceId, StringComparison.Ordinal)
               && string.Equals(a.DefensePieceId, b.DefensePieceId, StringComparison.Ordinal)
               && FrontsEqual(a.Structure, b.Structure)
               && PushFrontsEqual(a.PushFronts, b.PushFronts);

        private static bool FrontsEqual(IReadOnlyList<DynamicEditorFront> a, IReadOnlyList<DynamicEditorFront> b)
        {
            if (a.Count != b.Count) return false;
            for (var index = 0; index < a.Count; index++)
            {
                var left = a[index];
                var right = b[index];
                if (left.Index != right.Index
                    || left.IsActive != right.IsActive
                    || left.PalletCount != right.PalletCount
                    || left.LoadLevels != right.LoadLevels
                    || left.PalletsDeep != right.PalletsDeep
                    || left.DepthStartPosition != right.DepthStartPosition
                    || !left.FirstLevelHeight.Equals(right.FirstLevelHeight)
                    || !left.Bfr.Equals(right.Bfr)
                    || !left.BeamLength.Equals(right.BeamLength)
                    || !CellsEqual(left.Cells, right.Cells))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CellsEqual(IReadOnlyList<DynamicEditorCell> a, IReadOnlyList<DynamicEditorCell> b)
        {
            if (a.Count != b.Count) return false;
            for (var index = 0; index < a.Count; index++)
            {
                var left = a[index];
                var right = b[index];
                if (!left.PalletFront.Equals(right.PalletFront)
                    || !left.PalletHeight.Equals(right.PalletHeight)
                    || !left.PalletWeight.Equals(right.PalletWeight)
                    || !left.ClearHeight.Equals(right.ClearHeight)
                    || !string.Equals(left.InOutBeamCatalogId, right.InOutBeamCatalogId, StringComparison.Ordinal)
                    || !left.InOutBeamDepth.Equals(right.InOutBeamDepth)
                    || !left.BeamLengthOverride.Equals(right.BeamLengthOverride)
                    || !string.Equals(left.IntermediateBeamCatalogId, right.IntermediateBeamCatalogId, StringComparison.Ordinal)
                    || !left.IntermediateBeamDepth.Equals(right.IntermediateBeamDepth))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool PushFrontsEqual(IReadOnlyList<PushBackEditorFront> a, IReadOnlyList<PushBackEditorFront> b)
        {
            if (a.Count != b.Count) return false;
            for (var front = 0; front < a.Count; front++)
            {
                var left = a[front].Cells;
                var right = b[front].Cells;
                if (left.Count != right.Count) return false;
                for (var index = 0; index < left.Count; index++)
                {
                    if (!left[index].HighEndBeamPeralte.Equals(right[index].HighEndBeamPeralte)
                        || left[index].RearTopeEnabled != right[index].RearTopeEnabled
                        || left[index].PalletsDeepOverride != right[index].PalletsDeepOverride
                        || left[index].DrawPallet != right[index].DrawPallet)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool InputsEqual(PushBackEditorInputs x, PushBackEditorInputs y)
        {
            if (!PalletsEqual(x.Pallet, y.Pallet)
                || x.PalletsDeep != y.PalletsDeep
                || !string.Equals(x.PostCatalogId, y.PostCatalogId, StringComparison.Ordinal)
                || !x.PostPeralte.Equals(y.PostPeralte)
                || !x.PalletTolerance.Equals(y.PalletTolerance)
                || x.FirstLevelDatum != y.FirstLevelDatum
                || !x.BeamDepth.Equals(y.BeamDepth)
                || !AnnotationsEqual(x.Annotations, y.Annotations)
                || !x.ManualHeaderHeightOverride.Equals(y.ManualHeaderHeightOverride)
                || x.DerivedPostReinforced != y.DerivedPostReinforced
                || !x.DerivedPostReinforcementHeight.Equals(y.DerivedPostReinforcementHeight)
                || !x.DerivedPostHeight.Equals(y.DerivedPostHeight)
                || x.SeparatorCountOverride != y.SeparatorCountOverride
                || !x.SeparatorSpacingOverride.Equals(y.SeparatorSpacingOverride))
            {
                return false;
            }

            if (x.SafetySelections.Count != y.SafetySelections.Count) return false;
            for (var index = 0; index < x.SafetySelections.Count; index++)
            {
                if (!StructuralEquality.Equal(x.SafetySelections[index], y.SafetySelections[index])) return false;
            }

            return true;
        }

        private static bool PalletsEqual(PalletSpecification? a, PalletSpecification? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.Front.Equals(b.Front)
                   && a.Depth.Equals(b.Depth)
                   && a.Height.Equals(b.Height)
                   && a.Weight.Equals(b.Weight)
                   && string.Equals(a.WeightUnit, b.WeightUnit, StringComparison.Ordinal);
        }

        private static bool AnnotationsEqual(DynamicAnnotationOptions? a, DynamicAnnotationOptions? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.NumberFronts == b.NumberFronts
                   && a.NumberLevels == b.NumberLevels
                   && a.DrawRackName == b.DrawRackName
                   && a.AnnotationScale.Equals(b.AnnotationScale)
                   && a.Dimensions == b.Dimensions
                   && a.DimensionViews == b.DimensionViews
                   && string.Equals(a.DimensionStyle, b.DimensionStyle, StringComparison.Ordinal);
        }

        /// <summary>
        /// Exact structural equality of the safety selections, over EVERY instance field (public, private and backing) of the
        /// graph, so a field added to a safety family is compared without anyone having to remember it. A double compares by
        /// value, a string ordinally, and a list element by element in order.
        /// </summary>
        private static class StructuralEquality
        {
            private const int MaxDepth = 64;

            public static bool Equal(object? a, object? b) => Equal(a, b, 0);

            private static bool Equal(object? a, object? b, int depth)
            {
                if (ReferenceEquals(a, b)) return true;
                if (a == null || b == null) return false;
                if (depth > MaxDepth) return false;

                var type = a.GetType();
                if (type != b.GetType()) return false;
                if (type.IsPrimitive || type.IsEnum || a is string || a is decimal) return a.Equals(b);

                if (a is IEnumerable left && b is IEnumerable right)
                {
                    var leftItems = left.GetEnumerator();
                    var rightItems = right.GetEnumerator();
                    while (true)
                    {
                        var moreLeft = leftItems.MoveNext();
                        var moreRight = rightItems.MoveNext();
                        if (moreLeft != moreRight) return false;
                        if (!moreLeft) return true;
                        if (!Equal(leftItems.Current, rightItems.Current, depth + 1)) return false;
                    }
                }

                for (var current = type; current != null && current != typeof(object); current = current.BaseType)
                {
                    foreach (var field in current.GetFields(
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    {
                        if (!Equal(field.GetValue(a), field.GetValue(b), depth + 1)) return false;
                    }
                }

                return true;
            }
        }
    }
}
