using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;
using static RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T2 — the single-read entry of the Cantilever export (A65-F1-08, D-12, CD-I65-F4-11). The entry receives the bytes of
    /// the persisted design and a decoding function that the caller provides (here, the project store); it never names the store.
    /// </summary>
    public class StructuralAnalysisSourceEntryTests
    {
        private const double Coordinate = 1e-9;
        private const double Component = 1e-12;

        private static CantileverLineDesign Decode(byte[] bytes) =>
            new RackProjectStore().Deserialize(new UTF8Encoding(false).GetString(bytes))?.CantileverLineDesign;

        private static StructuralModelResult Run(
            byte[] design = null,
            Func<byte[], CantileverLineDesign> decode = null,
            string kind = "cantilever",
            int station = 0,
            byte[] loads = null,
            IStructuralMaterialProfile profile = null) =>
            CantileverStructuralExport.Build(
                RackId, kind, station, design ?? DesignBytes(), decode ?? Decode, loads ?? LoadsBytes(), profile ?? Profile(), Catalog);

        private static void AssertNoModel(StructuralModelResult result, StructuralExportOutcome outcome)
        {
            Assert.Equal(outcome, result.Outcome);
            Assert.Null(result.Model);
            Assert.NotEmpty(result.Diagnostics);
        }

        // ---------------------------------------------------------------- REF-CANT-01 through the entry (INV-06)

        [Fact]
        public void RefCant01PorLaEntradaEsValidaYReproduceLosLiteralesDelGoldenDeInv06()
        {
            var result = Run();

            Assert.True(result.Outcome == StructuralExportOutcome.Valid && result.Model != null, Describe(result));
            Assert.Equal(8, result.Model.Nodes.Count);
            Assert.Equal(7, result.Model.Members.Count);

            var foot = result.Model.Nodes.Single(n => n.Id == "COL.FOOT");
            Assert.True(Near(-6.1, foot.Position.Y.Inches, Coordinate) && Near(0.25, foot.Position.Z.Inches, Coordinate));

            var joint = result.Model.Nodes.Single(n => n.Id == "ARM.1.PositiveY.J");
            Assert.True(Near(20.26716169689737, joint.Position.Z.Inches, Coordinate), "ARM.1 J z: " + joint.Position.Z.Inches);

            var tip = result.Model.Nodes.Single(n => n.Id == "ARM.3.PositiveY.TIP");
            Assert.True(Near(36.22609804415268, tip.Position.Y.Inches, Coordinate), "ARM.3 TIP y: " + tip.Position.Y.Inches);
            Assert.True(Near(77.81030068809044, tip.Position.Z.Inches, Coordinate), "ARM.3 TIP z: " + tip.Position.Z.Inches);

            var axis = result.Model.Members.Single(m => m.Id == "ARM.1.PositiveY").PeralteAxis;
            Assert.True(Near(-0.03643412707017777, axis.Y, Component) && Near(0.9993360567820188, axis.Z, Component));
        }

        [Fact]
        public void LaEntradaYLaFuenteDanElMismoModeloGeometrico()
        {
            var viaEntry = Run().Model;
            var viaSource = BuildValid();

            Assert.NotNull(viaEntry);
            Assert.Equal(viaSource.Nodes.Select(n => n.Id), viaEntry.Nodes.Select(n => n.Id));
            Assert.Equal(viaSource.Members.Select(m => m.Id), viaEntry.Members.Select(m => m.Id));

            foreach (var node in viaSource.Nodes)
            {
                var other = viaEntry.Nodes.Single(n => n.Id == node.Id);
                Assert.Equal(node.Position.Y.Inches, other.Position.Y.Inches, 12);
                Assert.Equal(node.Position.Z.Inches, other.Position.Z.Inches, 12);
            }

            Assert.Equal(viaSource.Loads.Select(l => l.Id + l.DistanceFromNodeI.Inches), viaEntry.Loads.Select(l => l.Id + l.DistanceFromNodeI.Inches));
        }

        // ---------------------------------------------------------------- one read: the bytes are the ones that are hashed (D-12)

        [Fact]
        public void LaFuncionDeDecodificacionRecibeExactamenteLosBytesDelDisenoUnaSolaVez()
        {
            var original = DesignBytes();
            var seen = new List<byte[]>();

            var result = Run(original, bytes =>
            {
                seen.Add((byte[])bytes.Clone());
                return Decode(bytes);
            });

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            var received = Assert.Single(seen);
            Assert.Equal(original, received);
        }

        [Fact]
        public void LaSourceFingerprintSeCalculaSobreLosBytesPersistidosIntactos()
        {
            var original = DesignBytes();

            var model = Run(original).Model;

            Assert.NotNull(model);
            Assert.Equal(StructuralFingerprints.Source(original, RackId, "Cantilever", 0), model.Metadata.SourceRef.SourceFingerprint);
            Assert.Equal(RackId, model.Metadata.SourceRef.RackId);
            Assert.Equal(0, model.Metadata.SourceRef.StationIndex);
        }

        [Fact]
        public void UnaFuncionQueAlteraSuCopiaDeLosBytesNoCambiaLaHuella()
        {
            var original = DesignBytes();
            var expected = StructuralFingerprints.Source(original, RackId, "Cantilever", 0);

            var model = Run(original, bytes =>
            {
                var design = Decode(bytes);
                bytes[bytes.Length - 1] = (byte)'z';
                bytes[0] = (byte)'z';
                return design;
            }).Model;

            Assert.NotNull(model);
            Assert.Equal(expected, model.Metadata.SourceRef.SourceFingerprint);
        }

        [Fact]
        public void MismosBytesDanLaMismaHuellaYElMismoModelo()
        {
            var first = Run().Model;
            var second = Run().Model;

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal(first.Metadata.SourceRef.SourceFingerprint, second.Metadata.SourceRef.SourceFingerprint);
            Assert.Equal(StructuralModelCanonicalizer.ToCanonicalJson(first), StructuralModelCanonicalizer.ToCanonicalJson(second));
        }

        [Fact]
        public void UnCambioSoloDeEspaciosEnLosBytesCambiaLaHuellaYConservaLaGeometria()
        {
            var original = DesignBytes();
            var spaced = original.Concat(new byte[] { 32, 10 }).ToArray();

            var first = Run(original).Model;
            var second = Run(spaced).Model;

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.NotEqual(first.Metadata.SourceRef.SourceFingerprint, second.Metadata.SourceRef.SourceFingerprint);
            Assert.Equal(StructuralFingerprints.Source(spaced, RackId, "Cantilever", 0), second.Metadata.SourceRef.SourceFingerprint);
            Assert.Equal(
                first.Nodes.Select(n => n.Id + ":" + n.Position.Y.Inches + ":" + n.Position.Z.Inches),
                second.Nodes.Select(n => n.Id + ":" + n.Position.Y.Inches + ":" + n.Position.Z.Inches));
            Assert.Equal(first.Members.Select(m => m.Id + m.NodeIId + m.NodeJId), second.Members.Select(m => m.Id + m.NodeIId + m.NodeJId));
        }

        // ---------------------------------------------------------------- the kind of the envelope

        [Theory]
        [InlineData("cantilever")]
        [InlineData("CANTILEVER")]
        [InlineData("Cantilever")]
        public void ElKindDelSobreCantileverSeCompararSinDistinguirMayusculasYSeTraduceAlKindCanonico(string kind)
        {
            var result = Run(kind: kind);

            Assert.True(result.Outcome == StructuralExportOutcome.Valid && result.Model != null, Describe(result));
            Assert.Equal("Cantilever", result.Model.Metadata.SourceRef.Kind);
        }

        [Theory]
        [InlineData("selective")]
        [InlineData("pushback")]
        [InlineData("Dynamic")]
        [InlineData(" ")]
        public void OtroKindDelSobreDaElDiagnosticoExistenteSinLlamarALaFuncion(string kind)
        {
            var calls = 0;

            var result = Run(kind: kind, decode: bytes =>
            {
                calls++;
                return Decode(bytes);
            });

            AssertNoModel(result, StructuralExportOutcome.Invalid);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceKindMismatch);
            Assert.Equal(0, calls);
        }

        // ---------------------------------------------------------------- failures are results, not exceptions (A65-F1-09)

        [Theory]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(FormatException))]
        [InlineData(typeof(NullReferenceException))]
        public void UnaExcepcionDeLaFuncionDeDecodificacionEsUnavailableSinModelo(Type exceptionType)
        {
            var result = Run(decode: _ => throw (Exception)Activator.CreateInstance(exceptionType));

            AssertNoModel(result, StructuralExportOutcome.Unavailable);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceUnavailable);
        }

        [Theory]
        [InlineData("this is not json")]
        [InlineData("")]
        public void BytesQueElAlmacenNoPuedeLeerSonUnavailableSinModelo(string text)
        {
            var result = Run(new UTF8Encoding(false).GetBytes(text));

            Assert.Null(result.Model);
            Assert.True(
                result.Outcome == StructuralExportOutcome.Unavailable || result.Outcome == StructuralExportOutcome.Invalid,
                "resultado " + result.Outcome);
            Assert.NotEmpty(result.Diagnostics);
        }

        [Fact]
        public void UnaFuncionQueDevuelveNullEsInvalidSinModeloNiExcepcion()
        {
            var result = Run(decode: _ => null);

            AssertNoModel(result, StructuralExportOutcome.Invalid);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceDesignAbsent);
        }

        [Fact]
        public void UnDisenoQueHaceFallarAlResolverEsUnavailableSinModelo()
        {
            var result = Run(decode: bytes =>
            {
                var design = Decode(bytes);
                design.Bracing = null; // the resolver throws ArgumentNullException
                return design;
            });

            AssertNoModel(result, StructuralExportOutcome.Unavailable);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceUnavailable);
        }

        [Fact]
        public void UnIndiceDeEstacionFueraDeRangoPorLaEntradaEsInvalidSinModelo()
        {
            var result = Run(station: 9);

            AssertNoModel(result, StructuralExportOutcome.Invalid);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceStationRange);
        }

        [Fact]
        public void UnArchivoDeCargasInvalidoPorLaEntradaEsInvalidSinModelo()
        {
            var result = Run(loads: new UTF8Encoding(false).GetBytes("{}"));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.NotEmpty(result.Diagnostics);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void LosArgumentosNulosConservanArgumentNullException(int which)
        {
            Func<byte[], CantileverLineDesign> decode = Decode;
            var design = DesignBytes();
            var loads = LoadsBytes();

            Assert.Throws<ArgumentNullException>(() => CantileverStructuralExport.Build(
                RackId, "cantilever", 0, which == 0 ? null : design, which == 1 ? null : decode, which == 2 ? null : loads,
                which == 3 ? null : Profile(), which == 4 ? null : Catalog));
        }

        // ---------------------------------------------------------------- the shape of the entry (CD-I65-F4-11)

        [Fact]
        public void LaEntradaRecibeUnaFuncionDeDecodificacionYNoNombraElAlmacenNiElSobre()
        {
            var method = Assert.Single(typeof(CantileverStructuralExport).GetMethods().Where(m => m.Name == "Build"));

            Assert.Equal(
                new[]
                {
                    typeof(string), typeof(string), typeof(int), typeof(byte[]), typeof(Func<byte[], CantileverLineDesign>),
                    typeof(byte[]), typeof(IStructuralMaterialProfile), typeof(StructuralSectionCatalog),
                },
                method.GetParameters().Select(p => p.ParameterType));
            Assert.Equal(typeof(StructuralModelResult), method.ReturnType);

            var path = Path.Combine(RepoRoot(), "src", "RackCad.Application", "Systems", "Cantilever", "CantileverStructuralExport.cs");
            Assert.True(File.Exists(path));

            var withoutBlocks = Regex.Replace(File.ReadAllText(path), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            var code = Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);

            foreach (var word in new[] { "Document", "Xrecord", "RackProject", "ViewBuilder", "Preview", "RackSystemKind" })
            {
                Assert.DoesNotContain(word, code, StringComparison.Ordinal);
            }
        }
    }
}
