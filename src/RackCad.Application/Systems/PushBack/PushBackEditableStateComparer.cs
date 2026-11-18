using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The comparer and the deep copier of <see cref="PushBackEditableState"/>. Equality is exact over the EDITABLE content
    /// (a double by value, a string ordinally): the content of both sides, the parked tail, the module authority of each
    /// side and every rack-wide carrier. The selection, the baseline as an object, the reconciliation report and the tail
    /// consumption mark are not compared.
    /// <para>
    /// The copy keeps what the editor holds — modules (staged edit, accepted edit and baseline), parked tail and the
    /// presence of side B — and shares NO mutable object with the original, with exactly one exception decided by the
    /// Owner: the resolved baseline and the last reconciliation report of each side are shared by reference. That is NOT
    /// general shallow copying: nothing else is shared, and nobody may mutate those two objects while the state lives.
    /// </para>
    /// </summary>
    public sealed class PushBackEditableStateComparer : IEqualityComparer<PushBackEditableState>
    {
        public static PushBackEditableStateComparer Instance { get; } = new PushBackEditableStateComparer();

        public bool Equals(PushBackEditableState x, PushBackEditableState y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            var store = new RackFrameProjectStore();
            return EditorsEqual(x.Editor, y.Editor)
                   && TailsEqual(x.Editor, y.Editor, store)
                   && AuthorityEqual(x.Editor.SideA, y.Editor.SideA, store)
                   && AuthorityEqual(x.Editor.SideB, y.Editor.SideB, store)
                   && CarriersEqual(x, y);
        }

        public int GetHashCode(PushBackEditableState obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var hash = new HashCode();
            hash.Add(obj.Editor.SideBPresent);
            hash.Add(obj.Editor.Gap);
            hash.Add(obj.Editor.SideA.Structure.Count);
            hash.Add(obj.Editor.SideB.Structure.Count);
            hash.Add(obj.PalletsDeep);
            hash.Add(obj.AnnotationScale);
            hash.Add(obj.SafetySelections.Count);
            return hash.ToHashCode();
        }

        // ------------------------------------------------------------------------------------------------ the copier

        /// <summary>A deep copy equal for this comparer, with the same selection, that shares no mutable object with the
        /// original except the two declared references of each side (the resolved baseline and the module report).</summary>
        public static PushBackEditableState Copy(PushBackEditableState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var copy = new PushBackEditableState();

            // 1. The editable content and the selection.
            CopyEditor(state.Editor, copy.Editor);

            // 2. The parked tail, copied field by field and literally; and the consumption mark, if it was pending.
            var store = new RackFrameProjectStore();
            DynamicRackModuleDesign CopyModuleOf(DynamicRackModuleDesign module) => new DynamicRackModuleDesign
            {
                ModuleId = module.ModuleId,
                Kind = module.Kind,
                Length = module.Length,
                IsCalculated = module.IsCalculated,
                IsManualOverride = module.IsManualOverride,
                UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                HeaderConfiguration = store.DeepCopy(module.HeaderConfiguration),
                Notes = module.Notes,
            };

            DynamicHeaderLineOverride CopyLineOf(DynamicHeaderLineOverride line) => new DynamicHeaderLineOverride
            {
                PostIndex = line.PostIndex,
                ModuleId = line.ModuleId,
                Header = store.DeepCopy(line.Header),
            };

            copy.Editor.ParkDormantTail(
                state.Editor.DormantCompositeTail.Select(CopyModuleOf).ToList(),
                state.Editor.DormantTailLineOverrides.Select(CopyLineOf).ToList());
            if (state.Editor.DormantTailConsumptionPending)
            {
                copy.Editor.MarkDormantTailConsumed();
            }

            // 3. The module authority of each side, on the side already restored.
            copy.Editor.SideA.CopyAuthorityFrom(state.Editor.SideA);
            copy.Editor.SideB.CopyAuthorityFrom(state.Editor.SideB);

            // 4. The carriers.
            copy.Pallet = CopyPallet(state.Pallet);
            copy.PalletsDeep = state.PalletsDeep;
            copy.PostCatalogId = state.PostCatalogId;
            copy.PostPeralte = state.PostPeralte;
            copy.PalletTolerance = state.PalletTolerance;
            copy.BeamDepth = state.BeamDepth;
            copy.AnnotationScale = state.AnnotationScale;
            copy.FirstLevelDatum = state.FirstLevelDatum;
            copy.NumberFronts = state.NumberFronts;
            copy.NumberLevels = state.NumberLevels;
            copy.DrawRackName = state.DrawRackName;
            copy.Dimensions = state.Dimensions;
            copy.DimensionStyle = state.DimensionStyle;
            copy.LoadedDimensionViews = state.LoadedDimensionViews;
            copy.DimensionViewsTouched = state.DimensionViewsTouched;
            copy.DimensionsFrontal = state.DimensionsFrontal;
            copy.DimensionsLateral = state.DimensionsLateral;
            copy.DimensionsPlanta = state.DimensionsPlanta;
            copy.Advanced.ManualHeaderHeightOverride = state.Advanced.ManualHeaderHeightOverride;
            copy.Advanced.DerivedPostReinforced = state.Advanced.DerivedPostReinforced;
            copy.Advanced.DerivedPostReinforcementHeight = state.Advanced.DerivedPostReinforcementHeight;
            copy.Advanced.DerivedPostHeight = state.Advanced.DerivedPostHeight;
            copy.Advanced.SeparatorCountOverride = state.Advanced.SeparatorCountOverride;
            copy.Advanced.SeparatorSpacingOverride = state.Advanced.SeparatorSpacingOverride;
            foreach (var safety in state.SafetySelections)
            {
                copy.SafetySelections.Add(safety?.DeepCopy());
            }

            copy.Name = state.Name;
            return copy;
        }

        private static void CopyEditor(PushBackCompositeEditorState source, PushBackCompositeEditorState copy)
        {
            // Snapshot() and Restore() are the deep copies the editor already owns for its own rollback. The target is a
            // fresh state, so nothing of the authority of the source is carried by them.
            var snapshot = source.Snapshot();
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
        }

        private static PalletSpecification CopyPallet(PalletSpecification pallet)
            => pallet == null
                ? null
                : new PalletSpecification(pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, pallet.WeightUnit);

        // ----------------------------------------------------------------------------------- tail and module authority

        private static string Persisted(RackFrameProjectStore store, RackFrameConfiguration configuration)
            => configuration == null ? string.Empty : store.Serialize(configuration);

        private static bool TailsEqual(PushBackCompositeEditorState x, PushBackCompositeEditorState y, RackFrameProjectStore store)
        {
            var modulesX = x.DormantCompositeTail;
            var modulesY = y.DormantCompositeTail;
            if (modulesX.Count != modulesY.Count) return false;
            for (var index = 0; index < modulesX.Count; index++)
            {
                if (!ModulesEqual(modulesX[index], modulesY[index], store)) return false;
            }

            var linesX = x.DormantTailLineOverrides;
            var linesY = y.DormantTailLineOverrides;
            if (linesX.Count != linesY.Count) return false;
            for (var index = 0; index < linesX.Count; index++)
            {
                if (!LinesEqual(linesX[index], linesY[index], store)) return false;
            }

            return true;
        }

        private static bool ModulesEqual(DynamicRackModuleDesign a, DynamicRackModuleDesign b, RackFrameProjectStore store)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return string.Equals(a.ModuleId, b.ModuleId, StringComparison.Ordinal)
                   && a.Kind == b.Kind
                   && a.Length.Equals(b.Length)
                   && a.IsCalculated == b.IsCalculated
                   && a.IsManualOverride == b.IsManualOverride
                   && a.UseCalculatedHeaderConfiguration == b.UseCalculatedHeaderConfiguration
                   && string.Equals(a.Notes, b.Notes, StringComparison.Ordinal)
                   && string.Equals(Persisted(store, a.HeaderConfiguration), Persisted(store, b.HeaderConfiguration), StringComparison.Ordinal);
        }

        private static bool LinesEqual(DynamicHeaderLineOverride a, DynamicHeaderLineOverride b, RackFrameProjectStore store)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.PostIndex == b.PostIndex
                   && string.Equals(a.ModuleId, b.ModuleId, StringComparison.Ordinal)
                   && string.Equals(Persisted(store, a.Header), Persisted(store, b.Header), StringComparison.Ordinal);
        }

        private static bool AuthorityEqual(PushBackEditorState x, PushBackEditorState y, RackFrameProjectStore store)
        {
            // Throwaway states carry the authority, so reading a session that was never opened opens it on the throwaway
            // and never on the state being compared.
            var a = new PushBackEditorState();
            a.CopyAuthorityFrom(x);
            var b = new PushBackEditorState();
            b.CopyAuthorityFrom(y);

            // Always: the module intent of the baseline, with structural equality.
            if (!BaselineIntentEqual(a.WorkingBaseline, b.WorkingBaseline, store)) return false;

            // Always: the accepted edit, or both null.
            if (a.ModuleCommit == null || b.ModuleCommit == null)
            {
                if (a.ModuleCommit != null || b.ModuleCommit != null) return false;
            }
            else if (!a.ModuleCommit.SameAs(b.ModuleCommit))
            {
                return false;
            }

            // The staged edit: a session without changes is one with nothing pending whose confirmed list is equivalent to
            // a fresh one over its baseline. Two sessions without changes are equal; if one has changes, both must be the
            // same edit.
            var sessionA = a.ModuleSession;
            var sessionB = b.ModuleSession;
            return (Unchanged(sessionA, a.WorkingBaseline) && Unchanged(sessionB, b.WorkingBaseline))
                   || sessionA.SameEditAs(sessionB);
        }

        private static bool Unchanged(RackModuleEditSession session, PushBackSystem baseline)
            => !session.HasPendingChanges && session.SameEditAs(OpenOver(baseline));

        private static RackModuleEditSession OpenOver(PushBackSystem baseline)
            => baseline?.Structure == null
                ? RackModuleEditSession.Begin(Array.Empty<DynamicRackModuleDesign>())
                : RackModuleEditSession.Begin(baseline.Structure);

        private static bool BaselineIntentEqual(PushBackSystem x, PushBackSystem y, RackFrameProjectStore store)
        {
            // The module intents, the per-line configurations and the derived-post heights a baseline already carries.
            if (!OpenOver(x).Commit().SameAs(OpenOver(y).Commit())) return false;

            var linesX = x?.Structure?.HeaderLineOverrides;
            var linesY = y?.Structure?.HeaderLineOverrides;
            var countX = linesX?.Count ?? 0;
            var countY = linesY?.Count ?? 0;
            if (countX != countY) return false;
            for (var index = 0; index < countX; index++)
            {
                if (!LinesEqual(linesX[index], linesY[index], store)) return false;
            }

            var derivedX = x?.Structure?.DerivedPostLineOverrides;
            var derivedY = y?.Structure?.DerivedPostLineOverrides;
            var derivedCountX = derivedX?.Count ?? 0;
            var derivedCountY = derivedY?.Count ?? 0;
            if (derivedCountX != derivedCountY) return false;
            for (var index = 0; index < derivedCountX; index++)
            {
                if (derivedX[index]?.PostIndex != derivedY[index]?.PostIndex
                    || !(derivedX[index]?.Height).Equals(derivedY[index]?.Height))
                {
                    return false;
                }
            }

            return true;
        }

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

        // ---------------------------------------------------------------------------------------------------- carriers

        private static bool CarriersEqual(PushBackEditableState x, PushBackEditableState y)
        {
            if (!PalletsEqual(x.Pallet, y.Pallet)
                || x.PalletsDeep != y.PalletsDeep
                || !string.Equals(x.PostCatalogId, y.PostCatalogId, StringComparison.Ordinal)
                || !x.PostPeralte.Equals(y.PostPeralte)
                || !x.PalletTolerance.Equals(y.PalletTolerance)
                || !x.BeamDepth.Equals(y.BeamDepth)
                || !x.AnnotationScale.Equals(y.AnnotationScale)
                || x.FirstLevelDatum != y.FirstLevelDatum
                || x.NumberFronts != y.NumberFronts
                || x.NumberLevels != y.NumberLevels
                || x.DrawRackName != y.DrawRackName
                || x.Dimensions != y.Dimensions
                || !string.Equals(x.DimensionStyle, y.DimensionStyle, StringComparison.Ordinal)
                || x.LoadedDimensionViews != y.LoadedDimensionViews
                || x.DimensionViewsTouched != y.DimensionViewsTouched
                || x.DimensionsFrontal != y.DimensionsFrontal
                || x.DimensionsLateral != y.DimensionsLateral
                || x.DimensionsPlanta != y.DimensionsPlanta
                || !x.Advanced.ManualHeaderHeightOverride.Equals(y.Advanced.ManualHeaderHeightOverride)
                || x.Advanced.DerivedPostReinforced != y.Advanced.DerivedPostReinforced
                || !x.Advanced.DerivedPostReinforcementHeight.Equals(y.Advanced.DerivedPostReinforcementHeight)
                || !x.Advanced.DerivedPostHeight.Equals(y.Advanced.DerivedPostHeight)
                || x.Advanced.SeparatorCountOverride != y.Advanced.SeparatorCountOverride
                || !x.Advanced.SeparatorSpacingOverride.Equals(y.Advanced.SeparatorSpacingOverride)
                || !string.Equals(x.Name, y.Name, StringComparison.Ordinal))
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

        private static bool PalletsEqual(PalletSpecification a, PalletSpecification b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.Front.Equals(b.Front)
                   && a.Depth.Equals(b.Depth)
                   && a.Height.Equals(b.Height)
                   && a.Weight.Equals(b.Weight)
                   && string.Equals(a.WeightUnit, b.WeightUnit, StringComparison.Ordinal);
        }

        /// <summary>
        /// Exact structural equality of the safety selections, over EVERY instance field (public, private and backing) of the
        /// graph, so a field added to a safety family is compared without anyone having to remember it. A double compares by
        /// value, a string ordinally, and a list element by element in order.
        /// </summary>
        private static class StructuralEquality
        {
            private const int MaxDepth = 64;

            public static bool Equal(object a, object b) => Equal(a, b, 0);

            private static bool Equal(object a, object b, int depth)
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
