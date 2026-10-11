using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>
    /// The deterministic translation of a Valid neutral model to the system and the convention of the analysis program
    /// (I-65 D-03, D-04, D-08). Pure: no I/O, no clock, no analysis.
    ///
    /// <para>
    /// Positions go from (X, Y, Z) to (X, Z, -Y) with no conversion of units (the pair of the program is the pound and the inch,
    /// as in the model). The neutral support flags [Tx, Ty, Tz, Rx, Ry, Rz] are PERMUTED: dx = Tx, dy = Tz, dz = Ty, rx = Rx,
    /// ry = Rz, rz = Ry. The local axes and the twist follow <see cref="RamLocalAxes"/>, with the depth axis of the member taken to
    /// the program as (x, z, -y). A load is a point load in the direction GLOBAL_Y with the value minus its magnitude, placed as a
    /// percentage of the member. Nodes, members and loads come out in ordinal order of id.
    /// </para>
    /// <para>
    /// A section without a verified row, a row of another material, an orientation V1 does not admit, a release or more than
    /// <see cref="MaxPointLoadsPerMember"/> loads on a member is a blocking diagnostic and there is NEVER a partial plan.
    /// </para>
    /// </summary>
    public static class RamElementsTranslator
    {
        public const string Units = "LB_IN";
        public const string LoadCaseCategory = "OTHER";
        public const string LoadDirection = "GLOBAL_Y";
        public const int MaxPointLoadsPerMember = 24;

        private const double Tolerance = 1e-9;
        private const double PercentSlack = 1e-6;

        public static RamTranslationResult Translate(StructuralModelResult result, RamSectionMaterialTable table)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (result.Outcome != StructuralExportOutcome.Valid || result.Model == null)
            {
                var outcome = result.Outcome == StructuralExportOutcome.Valid ? StructuralExportOutcome.Invalid : result.Outcome;
                var diagnostics = new List<StructuralDiagnostic>(result.Diagnostics)
                {
                    StructuralDiagnostic.Blocking(RamElementsDiagnosticCodes.ModelNotValid, outcome, "model"),
                };

                return Result(diagnostics, null);
            }

            return Translate(result.Model, table);
        }

        public static RamTranslationResult Translate(StructuralModel model, RamSectionMaterialTable table)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (model.Diagnostics.Any(d => d.IsBlocking))
            {
                var blocked = new List<StructuralDiagnostic>(model.Diagnostics)
                {
                    StructuralDiagnostic.Blocking(RamElementsDiagnosticCodes.ModelNotValid, StructuralExportOutcome.Invalid, "model"),
                };

                return Result(blocked, null);
            }

            var diagnostics = new List<StructuralDiagnostic>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            var nodes = IndexNodes(model, diagnostics, seen);
            var restraints = IndexSupports(model, nodes, diagnostics, seen);
            var members = TranslateMembers(model, table, nodes, diagnostics, seen, out var lengths, out var usedRows);
            var loads = TranslateLoads(model, lengths, diagnostics, seen);

            if (diagnostics.Count > 0)
            {
                return Result(diagnostics, null);
            }

            var plan = new RamBuildPlan(
                Units,
                StructuralLoadCase.ReferenceCaseId,
                LoadCaseCategory,
                new ReadOnlyCollection<RamNodePlan>(nodes.Values
                    .OrderBy(node => node.Id, StringComparer.Ordinal)
                    .Select(node => new RamNodePlan(node.Id, ToRam(node.Position), restraints.TryGetValue(node.Id, out var flags) ? flags : default))
                    .ToList()),
                new ReadOnlyCollection<RamMemberPlan>(members.OrderBy(member => member.Id, StringComparer.Ordinal).ToList()),
                new ReadOnlyCollection<RamLoadPlan>(loads.OrderBy(load => load.Id, StringComparer.Ordinal).ToList()),
                new ReadOnlyCollection<RamSectionMapRow>(usedRows.Values.OrderBy(row => row.SectionId.Value, StringComparer.Ordinal).ToList()));

            return Result(diagnostics, plan);
        }

        // ---------------------------------------------------------------- nodes and supports

        private static Dictionary<string, StructuralNode> IndexNodes(
            StructuralModel model, List<StructuralDiagnostic> diagnostics, HashSet<string> seen)
        {
            var nodes = new Dictionary<string, StructuralNode>(StringComparer.Ordinal);

            foreach (var node in model.Nodes)
            {
                if (!nodes.ContainsKey(node.Id))
                {
                    nodes.Add(node.Id, node);
                }
                else
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.ModelReference, StructuralExportOutcome.Invalid, node.Id);
                }
            }

            return nodes;
        }

        private static Dictionary<string, RamRestraints> IndexSupports(
            StructuralModel model,
            Dictionary<string, StructuralNode> nodes,
            List<StructuralDiagnostic> diagnostics,
            HashSet<string> seen)
        {
            var restraints = new Dictionary<string, RamRestraints>(StringComparer.Ordinal);

            foreach (var support in model.Supports)
            {
                if (!nodes.ContainsKey(support.NodeId))
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.ModelReference, StructuralExportOutcome.Invalid, support.NodeId);
                }
                else if (restraints.ContainsKey(support.NodeId))
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.SupportDuplicate, StructuralExportOutcome.Invalid, support.NodeId);
                }
                else
                {
                    restraints.Add(support.NodeId, Permute(support.Flags));
                }
            }

            return restraints;
        }

        /// <summary>
        /// The neutral order is [Tx, Ty, Tz, Rx, Ry, Rz]; the program swaps its second and third axes (Y_RAM = Z, Z_RAM = -Y), so
        /// translations and rotations swap the same way: dx = Tx, dy = Tz, dz = Ty, rx = Rx, ry = Rz, rz = Ry. The flags have no sign.
        /// </summary>
        private static RamRestraints Permute(IReadOnlyList<bool> flags) =>
            new RamRestraints(flags[0], flags[2], flags[1], flags[3], flags[5], flags[4]);

        private static RamVector ToRam(StructuralPosition position) =>
            new RamVector(position.X.Inches + 0.0, position.Z.Inches + 0.0, -position.Y.Inches + 0.0);

        // ---------------------------------------------------------------- members

        private static List<RamMemberPlan> TranslateMembers(
            StructuralModel model,
            RamSectionMaterialTable table,
            Dictionary<string, StructuralNode> nodes,
            List<StructuralDiagnostic> diagnostics,
            HashSet<string> seen,
            out Dictionary<string, double> lengths,
            out Dictionary<string, RamSectionMapRow> usedRows)
        {
            var plans = new List<RamMemberPlan>();
            lengths = new Dictionary<string, double>(StringComparer.Ordinal);
            usedRows = new Dictionary<string, RamSectionMapRow>(StringComparer.Ordinal);

            var loadCounts = model.Loads.GroupBy(load => load.MemberId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            foreach (var member in model.Members.OrderBy(m => m.Id, StringComparer.Ordinal))
            {
                var ok = true;

                if (lengths.ContainsKey(member.Id))
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.ModelReference, StructuralExportOutcome.Invalid, member.Id);
                    continue;
                }

                ok &= CheckRow(member, table, diagnostics, seen, out var row);

                if (member.Releases.AnyReleased)
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.ReleaseUnsupported, StructuralExportOutcome.Unsupported, member.Id);
                    ok = false;
                }

                if (loadCounts.TryGetValue(member.Id, out var count) && count > MaxPointLoadsPerMember)
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.LoadCountExceeded, StructuralExportOutcome.Unsupported, member.Id);
                    ok = false;
                }

                if (!nodes.TryGetValue(member.NodeIId, out var nodeI) || !nodes.TryGetValue(member.NodeJId, out var nodeJ))
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.ModelReference, StructuralExportOutcome.Invalid, member.Id);
                    continue;
                }

                var start = ToRam(nodeI.Position);
                var end = ToRam(nodeJ.Position);
                var along = new RamVector(end.X - start.X, end.Y - start.Y, end.Z - start.Z);
                var length = RamLocalAxes.Length(along);

                if (!(length > 0) || double.IsInfinity(length))
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.MemberGeometry, StructuralExportOutcome.Invalid, member.Id);
                    continue;
                }

                lengths.Add(member.Id, length);

                var l1 = RamLocalAxes.Unit(along);
                var depth = new RamVector(member.PeralteAxis.X + 0.0, member.PeralteAxis.Z + 0.0, -member.PeralteAxis.Y + 0.0);

                if (Math.Abs(RamLocalAxes.Length(depth) - 1) > Tolerance || Math.Abs(RamLocalAxes.Dot(depth, l1)) > Tolerance)
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.MemberGeometry, StructuralExportOutcome.Invalid, member.Id);
                    continue;
                }

                if (!RamLocalAxes.IsAdmitted(l1))
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.OrientationUnsupported, StructuralExportOutcome.Unsupported, member.Id);
                    continue;
                }

                if (!ok || row == null)
                {
                    continue;
                }

                var zeroTwist = RamLocalAxes.ZeroTwist(l1);
                var theta = RamLocalAxes.ThetaDegrees(zeroTwist, depth);

                usedRows[row.SectionId.Value] = row;
                plans.Add(new RamMemberPlan(
                    member.Id,
                    member.NodeIId,
                    member.NodeJId,
                    row.RamSectionName,
                    row.RamMaterial,
                    theta,
                    RamLocalAxes.Rotate(zeroTwist, theta),
                    new ReadOnlyCollection<bool>(member.Releases.Flags.ToList())));
            }

            return plans;
        }

        /// <summary>The row of the section of a member: it must exist, be verified and be the row of the material the model carries.</summary>
        private static bool CheckRow(
            StructuralMember member,
            RamSectionMaterialTable table,
            List<StructuralDiagnostic> diagnostics,
            HashSet<string> seen,
            out RamSectionMapRow row)
        {
            if (!table.TryGetRamRow(member.SectionRef, out row))
            {
                Add(diagnostics, seen, RamElementsDiagnosticCodes.SectionRowMissing, StructuralExportOutcome.Unsupported, member.SectionRef.Value);
                row = null;
                return false;
            }

            if (!row.IsVerified)
            {
                Add(diagnostics, seen, RamElementsDiagnosticCodes.SectionRowUnverified, StructuralExportOutcome.Unsupported, member.SectionRef.Value);
                row = null;
                return false;
            }

            if (row.MaterialId != member.MaterialRef)
            {
                // The model carries another material: this is not the table that built it.
                Add(diagnostics, seen, RamElementsDiagnosticCodes.MaterialMismatch, StructuralExportOutcome.Invalid, member.Id);
                row = null;
                return false;
            }

            return true;
        }

        // ---------------------------------------------------------------- loads

        private static List<RamLoadPlan> TranslateLoads(
            StructuralModel model, Dictionary<string, double> lengths, List<StructuralDiagnostic> diagnostics, HashSet<string> seen)
        {
            var plans = new List<RamLoadPlan>();
            var memberIds = new HashSet<string>(model.Members.Select(m => m.Id), StringComparer.Ordinal);

            foreach (var load in model.Loads.OrderBy(l => l.Id, StringComparer.Ordinal))
            {
                if (!memberIds.Contains(load.MemberId))
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.ModelReference, StructuralExportOutcome.Invalid, load.Id);
                    continue;
                }

                if (!lengths.TryGetValue(load.MemberId, out var length))
                {
                    // The member itself already has a blocking diagnostic.
                    continue;
                }

                var percent = (100.0 * load.DistanceFromNodeI.Inches) / length;

                if (percent < -PercentSlack || percent > 100.0 + PercentSlack || double.IsNaN(percent))
                {
                    Add(diagnostics, seen, RamElementsDiagnosticCodes.LoadPositionRange, StructuralExportOutcome.Invalid, load.Id);
                    continue;
                }

                plans.Add(new RamLoadPlan(
                    load.Id,
                    load.MemberId,
                    LoadDirection,
                    -load.Magnitude.PoundsForce + 0.0,
                    Math.Min(100.0, Math.Max(0.0, percent)) + 0.0));
            }

            return plans;
        }

        // ---------------------------------------------------------------- the result

        private static void Add(List<StructuralDiagnostic> diagnostics, HashSet<string> seen, string code, StructuralExportOutcome outcome, string subject)
        {
            if (seen.Add(code + "|" + subject))
            {
                diagnostics.Add(StructuralDiagnostic.Blocking(code, outcome, subject));
            }
        }

        private static RamTranslationResult Result(List<StructuralDiagnostic> diagnostics, RamBuildPlan plan)
        {
            var outcome = StructuralModelResult.Aggregate(diagnostics);

            return new RamTranslationResult(
                outcome,
                new ReadOnlyCollection<StructuralDiagnostic>(diagnostics),
                outcome == StructuralExportOutcome.Valid ? plan : null);
        }
    }
}
