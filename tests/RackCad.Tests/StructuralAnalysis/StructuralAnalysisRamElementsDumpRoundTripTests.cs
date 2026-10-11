using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using Xunit.Abstractions;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;
using S = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T3 — the round trip of REF-CANT-01 (D-04 forwards, its inverse in the reader). The RamBuildPlan of the F3 translation (with the
    /// table of fixture rows, all verified FIXTURES) is written as the synthetic dump the build would leave in the model; the reader must give
    /// back the nodes, the axes, the support flags and the loads of the neutral model to a tolerance of 1e-9. Neither the translation nor the
    /// golden is touched: if they do not meet, the stop condition C-03 applies.
    /// </summary>
    public class StructuralAnalysisRamElementsDumpRoundTripTests
    {
        private const double Tolerance = 1e-9;

        private readonly ITestOutputHelper _output;

        public StructuralAnalysisRamElementsDumpRoundTripTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private sealed class Reference
        {
            public StructuralModel Model;
            public RamBuildPlan Plan;
            public JsonObject Dump;
        }

        private static Reference Build()
        {
            var table = F.RefTable();
            var model = F.ValidRefModel(table);
            var translation = RamElementsTranslator.Translate(model, table);

            Assert.True(translation.Outcome == StructuralExportOutcome.Valid && translation.Plan != null, "La traduccion de REF-CANT-01 no es Valid.");
            return new Reference { Model = model, Plan = translation.Plan, Dump = DumpOf(translation.Plan) };
        }

        /// <summary>The dump a build of the plan leaves in the model: program ids that are not the position, the neutral id as description.</summary>
        private static JsonObject DumpOf(RamBuildPlan plan)
        {
            var nodeIds = plan.Nodes.Select((node, index) => (node.Id, Program: 101 + index)).ToDictionary(p => p.Id, p => p.Program, StringComparer.Ordinal);
            var memberIds = plan.Members.Select((member, index) => (member.Id, Program: 201 + index)).ToDictionary(p => p.Id, p => p.Program, StringComparer.Ordinal);

            var nodes = new JsonArray();

            foreach (var node in plan.Nodes)
            {
                var r = node.Restraints;
                nodes.Add(S.Node(nodeIds[node.Id], node.Id, node.Position.X, node.Position.Y, node.Position.Z, r.Dx, r.Dy, r.Dz, r.Rx, r.Ry, r.Rz));
            }

            var members = new JsonArray();

            foreach (var member in plan.Members)
            {
                var axes = member.LocalAxes;
                var json = S.Member(
                    memberIds[member.Id],
                    member.Id,
                    nodeIds[member.NodeIId],
                    nodeIds[member.NodeJId],
                    new[] { axes.L1.X, axes.L1.Y, axes.L1.Z },
                    new[] { axes.L2.X, axes.L2.Y, axes.L2.Z },
                    new[] { axes.L3.X, axes.L3.Y, axes.L3.Z },
                    member.ThetaDegrees);
                json["section"] = member.SectionName;
                json["material"] = member.MaterialName;
                json["releases"] = S.Bools(member.Releases.ToArray());
                members.Add(json);
            }

            var memberLoads = new JsonArray();

            foreach (var group in plan.Loads.GroupBy(load => load.MemberId, StringComparer.Ordinal))
            {
                var loads = group.Select(load => S.Load(load.Direction.Substring(load.Direction.Length - 1), load.ValuePoundsForce, load.PositionPercent, true)).ToArray();
                memberLoads.Add(S.MemberLoads(memberIds[group.Key], loads));
            }

            var dump = S.Valid();
            dump["nodes"] = nodes;
            dump["members"] = members;
            dump["loadCases"] = new JsonArray(new JsonObject
            {
                ["id"] = plan.LoadCaseId,
                ["category"] = plan.LoadCaseCategory,
                ["selfWeight"] = S.Numbers(0, 0, 0),
                ["memberLoads"] = memberLoads,
            });
            return dump;
        }

        private static StructuralModelReadback ReadOf(Reference reference)
        {
            var result = S.Read(reference.Dump);

            Assert.True(
                result.Outcome == StructuralExportOutcome.Valid && result.Readback != null,
                "El volcado sintetico de REF-CANT-01 no se lee: " + result.Outcome + " " + S.Codes(result));
            return result.Readback;
        }

        private static double Diff(double actual, double expected) => Math.Abs(actual - expected);

        private static (double X, double Y, double Z) Unit((double X, double Y, double Z) v)
        {
            var length = Math.Sqrt((v.X * v.X) + (v.Y * v.Y) + (v.Z * v.Z));
            return (v.X / length, v.Y / length, v.Z / length);
        }

        private static (double X, double Y, double Z) Cross((double X, double Y, double Z) a, (double X, double Y, double Z) b) =>
            ((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));

        private static double Compare(Reference reference, StructuralModelReadback readback, List<string> failures)
        {
            var model = reference.Model;
            var maximum = 0.0;

            void Check(string label, double actual, double expected)
            {
                var difference = Diff(actual, expected);
                maximum = Math.Max(maximum, difference);

                if (difference > Tolerance * Math.Max(1.0, Math.Abs(expected)))
                {
                    failures.Add(label + ": " + actual + " frente a " + expected);
                }
            }

            if (readback.Nodes.Count != model.Nodes.Count)
            {
                failures.Add("nudos: " + readback.Nodes.Count + " frente a " + model.Nodes.Count);
            }

            if (readback.Members.Count != model.Members.Count)
            {
                failures.Add("miembros: " + readback.Members.Count + " frente a " + model.Members.Count);
            }

            var nodes = readback.Nodes.ToDictionary(n => n.Description, StringComparer.Ordinal);
            var members = readback.Members.ToDictionary(m => m.Description, StringComparer.Ordinal);

            foreach (var node in model.Nodes)
            {
                var read = nodes[node.Id];
                Check("nudo " + node.Id + " X", read.Position.X.Inches, node.Position.X.Inches);
                Check("nudo " + node.Id + " Y", read.Position.Y.Inches, node.Position.Y.Inches);
                Check("nudo " + node.Id + " Z", read.Position.Z.Inches, node.Position.Z.Inches);

                var support = model.Supports.SingleOrDefault(s => s.NodeId == node.Id);
                var expected = support == null ? new bool[6] : support.Flags.ToArray();

                if (!expected.SequenceEqual(read.SupportFlags))
                {
                    failures.Add("banderas del nudo " + node.Id + ": " + string.Join(",", read.SupportFlags) + " frente a " + string.Join(",", expected));
                }
            }

            foreach (var member in model.Members)
            {
                var read = members[member.Id];
                var nodeI = model.Nodes.Single(n => n.Id == member.NodeIId).Position;
                var nodeJ = model.Nodes.Single(n => n.Id == member.NodeJId).Position;

                if (nodes.Values.Single(n => n.Id == read.NodeIId).Description != member.NodeIId
                    || nodes.Values.Single(n => n.Id == read.NodeJId).Description != member.NodeJId)
                {
                    failures.Add("conectividad del miembro " + member.Id);
                }

                var l1 = Unit((nodeJ.X.Inches - nodeI.X.Inches, nodeJ.Y.Inches - nodeI.Y.Inches, nodeJ.Z.Inches - nodeI.Z.Inches));
                var l2 = (member.PeralteAxis.X, member.PeralteAxis.Y, member.PeralteAxis.Z);
                var l3 = Cross(l1, l2);

                Check("miembro " + member.Id + " L1 X", read.Axis1.X, l1.X);
                Check("miembro " + member.Id + " L1 Y", read.Axis1.Y, l1.Y);
                Check("miembro " + member.Id + " L1 Z", read.Axis1.Z, l1.Z);
                Check("miembro " + member.Id + " L2 X", read.Axis2.X, l2.Item1);
                Check("miembro " + member.Id + " L2 Y", read.Axis2.Y, l2.Item2);
                Check("miembro " + member.Id + " L2 Z", read.Axis2.Z, l2.Item3);
                Check("miembro " + member.Id + " L3 X", read.Axis3.X, l3.X);
                Check("miembro " + member.Id + " L3 Y", read.Axis3.Y, l3.Y);
                Check("miembro " + member.Id + " L3 Z", read.Axis3.Z, l3.Z);

                var planned = reference.Plan.Members.Single(p => p.Id == member.Id);

                if (read.SectionName != planned.SectionName || read.MaterialName != planned.MaterialName)
                {
                    failures.Add("seccion o material del miembro " + member.Id);
                }

                var expectedLoads = model.Loads.Where(l => l.MemberId == member.Id).OrderBy(l => l.Id, StringComparer.Ordinal).ToList();
                var readLoads = readback.LoadCases.Single().Loads.Where(l => l.MemberId == read.Id).ToList();

                if (expectedLoads.Count != readLoads.Count)
                {
                    failures.Add("cargas del miembro " + member.Id + ": " + readLoads.Count + " frente a " + expectedLoads.Count);
                    continue;
                }

                for (var index = 0; index < expectedLoads.Count; index++)
                {
                    if (readLoads[index].Axis != StructuralReadbackLoadAxis.GlobalZ)
                    {
                        failures.Add("direccion de la carga " + expectedLoads[index].Id + ": " + readLoads[index].Axis);
                    }

                    // the neutral load is the magnitude along minus Z; the read force is signed along plus Z
                    Check("carga " + expectedLoads[index].Id + " valor", readLoads[index].Value.PoundsForce, -expectedLoads[index].Magnitude.PoundsForce);
                    Check("carga " + expectedLoads[index].Id + " distancia", readLoads[index].DistanceFromNodeI.Inches, expectedLoads[index].DistanceFromNodeI.Inches);
                }
            }

            return maximum;
        }

        // ---------------------------------------------------------------- the round trip

        [Fact]
        public void ElVolcadoSinteticoDeRefCant01SeLeeValido()
        {
            var reference = Build();
            var readback = ReadOf(reference);

            Assert.NotEmpty(readback.Nodes);
            Assert.NotEmpty(readback.Members);
            Assert.Single(readback.LoadCases);
            Assert.Empty(readback.Combinations);
        }

        [Fact]
        public void LaIdaYVueltaDeRefCant01ReproduceNudosEjesBanderasYCargasAToleranciaDeUnNanoMetro()
        {
            var reference = Build();
            var failures = new List<string>();
            var maximum = Compare(reference, ReadOf(reference), failures);

            _output.WriteLine("ROUNDTRIP-MAX-DIFF " + maximum.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + " nodes=" + reference.Model.Nodes.Count + " members=" + reference.Model.Members.Count + " loads=" + reference.Model.Loads.Count);
            Assert.True(failures.Count == 0, "La ida y vuelta no reproduce el modelo neutral (parada C-03):\n" + string.Join("\n", failures));
            Assert.True(maximum <= Tolerance, "Diferencia maxima " + maximum);
        }

        [Fact]
        public void LaIdaYVueltaCubreTodosLosNudosMiembrosYCargasDelModelo()
        {
            var reference = Build();
            var readback = ReadOf(reference);

            Assert.Equal(reference.Model.Nodes.Count, readback.Nodes.Count);
            Assert.Equal(reference.Model.Members.Count, readback.Members.Count);
            Assert.Equal(reference.Model.Loads.Count, readback.LoadCases.Single().Loads.Count);
            Assert.Equal(reference.Model.Supports.Count(s => s.Flags.Any(f => f)), readback.Nodes.Count(n => n.SupportFlags.Any(f => f)));
            Assert.True(reference.Model.Loads.Count > 0 && reference.Model.Supports.Count > 0, "REF-CANT-01 debe tener cargas y apoyos.");
        }

        [Fact]
        public void LaIdaYVueltaDevuelveLasBanderasDeCadaNudoApoyadoDelModelo()
        {
            // the flags of REF-CANT-01 are symmetric under the permutation (Ty = Tz and Ry = Rz are equal in every support), so the ORDER
            // of the permutation is proven by the single-flag cases of the reader tests; here the whole model must come back flag by flag
            var reference = Build();
            var readback = ReadOf(reference);

            Assert.NotEmpty(reference.Model.Supports);

            foreach (var support in reference.Model.Supports)
            {
                var node = reference.Model.Nodes.Single(n => n.Id == support.NodeId);
                var read = readback.Nodes.Single(n => n.Description == node.Id);

                Assert.Equal(support.Flags.ToArray(), read.SupportFlags.ToArray());
            }
        }

        [Fact]
        public void LaIdaYVueltaNoCambiaElModeloNeutralNiElPlan()
        {
            var reference = Build();
            var before = reference.Dump.ToJsonString();

            ReadOf(reference);

            Assert.Equal(before, reference.Dump.ToJsonString());
            Assert.Equal(before, DumpOf(reference.Plan).ToJsonString());
        }

        [Fact]
        public void UnaAlteracionDelVolcadoSiLaVeLaComparacion()
        {
            var reference = Build();
            var failures = new List<string>();

            reference.Dump["nodes"].ObjectAt(0)["coordinates"] = S.Numbers(
                reference.Plan.Nodes[0].Position.X + 0.001, reference.Plan.Nodes[0].Position.Y, reference.Plan.Nodes[0].Position.Z);

            var maximum = Compare(reference, ReadOf(reference), failures);

            Assert.NotEmpty(failures);
            Assert.True(maximum > Tolerance);
        }
    }
}
