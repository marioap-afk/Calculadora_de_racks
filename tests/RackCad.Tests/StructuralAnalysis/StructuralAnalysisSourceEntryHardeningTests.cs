using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using Xunit;
using static RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T2 — the hardening of the model builder and of the Cantilever source: exceptions of the source become a result
    /// (A65-F1-09), the structural rejections of the builder (A65-F1-09), the whole-line block (A65-F1-11), the allow-list of
    /// families (A65-F1-03, IDL-09), the bounded re-orthogonalisation (A65-F1-10, IDL-08) and the inventory of the pieces of the
    /// station (A65-F1-12, CD-I65-F1-14).
    /// </summary>
    public class StructuralAnalysisSourceEntryHardeningTests
    {
        // ---------------------------------------------------------------- test doubles of the neutral source

        private sealed class ThrowingSource : IStructuralModelSource
        {
            private readonly Exception _exception;

            public ThrowingSource(Exception exception)
            {
                _exception = exception;
            }

            public StructuralSourceDescription Describe(StructuralScope scope, StructuralLoadFile loads) => throw _exception;
        }

        private sealed class AlteredSource : IStructuralModelSource
        {
            private readonly Func<StructuralSourceDescription, StructuralSourceDescription> _alter;

            public AlteredSource(Func<StructuralSourceDescription, StructuralSourceDescription> alter)
            {
                _alter = alter;
            }

            public StructuralSourceDescription Describe(StructuralScope scope, StructuralLoadFile loads) =>
                _alter(new StructuralAnalysisFixtureSource().Describe(scope, loads));
        }

        private static StructuralModelResult BuildWith(IStructuralModelSource source, StructuralSectionCatalog catalog = null, IStructuralMaterialProfile profile = null) =>
            StructuralModelBuilder.BuildStructuralModel(
                source,
                StructuralAnalysisFixtures.Scope(),
                StructuralAnalysisFixtures.TwoLoads(),
                profile ?? StructuralAnalysisFixtures.Profile(),
                catalog ?? StructuralAnalysisFixtures.Catalog());

        private static StructuralModelResult BuildAltered(Func<StructuralSourceDescription, StructuralSourceDescription> alter) =>
            BuildWith(new AlteredSource(alter));

        private static StructuralSourceDescription With(
            StructuralSourceDescription d,
            IEnumerable<StructuralNode> nodes = null,
            IEnumerable<StructuralSourceMember> members = null,
            IEnumerable<StructuralSupport> supports = null,
            IEnumerable<AnalysisAssumption> assumptions = null,
            IEnumerable<StructuralMemberPointLoad> loads = null,
            IEnumerable<StructuralDiagnostic> diagnostics = null) =>
            new StructuralSourceDescription(
                nodes ?? d.Nodes, members ?? d.Members, supports ?? d.Supports, assumptions ?? d.Assumptions, loads ?? d.Loads, diagnostics ?? d.Diagnostics);

        private static IEnumerable<string> Codes(StructuralModelResult result) => result.Diagnostics.Select(d => d.Code);

        // ---------------------------------------------------------------- A65-F1-09: exceptions of the source

        [Theory]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(NullReferenceException))]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(IndexOutOfRangeException))]
        public void UnaExcepcionDeLaFuenteEsUnavailableSinModelo(Type exceptionType)
        {
            var result = BuildWith(new ThrowingSource((Exception)Activator.CreateInstance(exceptionType)));

            Assert.Equal(StructuralExportOutcome.Unavailable, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.SourceUnavailable, Codes(result));
        }

        [Fact]
        public void UnaExcepcionDeMemoriaInsuficienteNoSeConvierteEnResultado()
        {
            Assert.Throws<OutOfMemoryException>(() => BuildWith(new ThrowingSource(new OutOfMemoryException())));
        }

        [Fact]
        public void LosArgumentosNulosDelConstructorSiguenLanzandoArgumentNullException()
        {
            var source = new StructuralAnalysisFixtureSource();
            var scope = StructuralAnalysisFixtures.Scope();
            var loads = StructuralAnalysisFixtures.TwoLoads();
            var profile = StructuralAnalysisFixtures.Profile();
            var catalog = StructuralAnalysisFixtures.Catalog();

            Assert.Throws<ArgumentNullException>(() => StructuralModelBuilder.BuildStructuralModel(null, scope, loads, profile, catalog));
            Assert.Throws<ArgumentNullException>(() => StructuralModelBuilder.BuildStructuralModel(source, null, loads, profile, catalog));
            Assert.Throws<ArgumentNullException>(() => StructuralModelBuilder.BuildStructuralModel(source, scope, null, profile, catalog));
            Assert.Throws<ArgumentNullException>(() => StructuralModelBuilder.BuildStructuralModel(source, scope, loads, null, catalog));
            Assert.Throws<ArgumentNullException>(() => StructuralModelBuilder.BuildStructuralModel(source, scope, loads, profile, null));
        }

        // ---------------------------------------------------------------- A65-F1-09: structural rejections of the builder

        [Fact]
        public void UnModeloSinNudosEsInvalidConSuPropioCodigo()
        {
            var result = BuildAltered(d => With(
                d, nodes: new StructuralNode[0], members: new StructuralSourceMember[0], supports: new StructuralSupport[0], loads: new StructuralMemberPointLoad[0]));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.ModelNoNodes, Codes(result));
        }

        [Fact]
        public void UnModeloSinMiembrosEsInvalidConSuPropioCodigo()
        {
            var result = BuildAltered(d => With(d, members: new StructuralSourceMember[0], loads: new StructuralMemberPointLoad[0]));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.ModelNoMembers, Codes(result));
            Assert.DoesNotContain(StructuralDiagnosticCodes.ModelNoNodes, Codes(result));
        }

        [Fact]
        public void DosApoyosEnElMismoNudoSonInvalidos()
        {
            var result = BuildAltered(d => With(
                d, supports: d.Supports.Concat(new[] { new StructuralSupport("COL.FOOT", Enumerable.Repeat(true, StructuralSupport.FlagCount), "SUP-01") })));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SupportDuplicateNode && d.Subject == "COL.FOOT");
        }

        [Fact]
        public void UnSupuestoReferenciadoPorUnApoyoQueNoEstaDeclaradoEsInvalido()
        {
            var result = BuildAltered(d => With(
                d, supports: new[] { new StructuralSupport("COL.FOOT", Enumerable.Repeat(true, StructuralSupport.FlagCount), "SUP-99") }));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.AssumptionUndeclared && d.Subject == "SUP-99");
        }

        [Fact]
        public void UnSupuestoReferenciadoPorUnaLiberacionQueNoEstaDeclaradoEsInvalido()
        {
            var result = BuildAltered(d => With(
                d, members: d.Members.Select(m => new StructuralSourceMember(
                    m.Id, m.NodeIId, m.NodeJId, m.SectionId, m.PeralteAxis, m.Role, m.OriginPiece, StructuralMemberReleases.None("CON-99")))));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.AssumptionUndeclared && d.Subject == "CON-99");
        }

        [Fact]
        public void UnaDescripcionConTodosLosSupuestosDeclaradosYSinApoyosDuplicadosEsValida()
        {
            var result = BuildAltered(d => d);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Model);
            Assert.DoesNotContain(StructuralDiagnosticCodes.AssumptionUndeclared, Codes(result));
            Assert.DoesNotContain(StructuralDiagnosticCodes.SupportDuplicateNode, Codes(result));
        }

        [Fact]
        public void UnaFuenteQueRechazaNoAnadeLosCodigosDeModeloVacio()
        {
            var refusal = StructuralDiagnostic.Blocking(StructuralDiagnosticCodes.StationFaceUnsupported, StructuralExportOutcome.Unsupported, "station-0");
            var result = BuildAltered(d => new StructuralSourceDescription(
                new StructuralNode[0], new StructuralSourceMember[0], new StructuralSupport[0], new AnalysisAssumption[0],
                new StructuralMemberPointLoad[0], new[] { refusal }));

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.DoesNotContain(StructuralDiagnosticCodes.ModelNoNodes, Codes(result));
            Assert.DoesNotContain(StructuralDiagnosticCodes.ModelNoMembers, Codes(result));
        }

        // ---------------------------------------------------------------- A65-F1-03: the allow-list of families

        private static StructuralModelResult BuildWithColumnFamily(StructuralSectionFamily family)
        {
            var catalog = StructuralSectionCatalog.Create(
                new[]
                {
                    StructuralAnalysisFixtures.Section(
                        StructuralAnalysisFixtures.ColumnSection, family, "COLUMN-X", StructuralAnalysisFixtures.Properties(7.65, 204, 17.3, 0.3)),
                    StructuralAnalysisFixtures.Section(
                        StructuralAnalysisFixtures.ArmSection, StructuralSectionFamily.HssRectangular, "HSS4X4X.250", StructuralAnalysisFixtures.Properties(3.37, 7.8, 7.8, 12.8)),
                },
                null);

            return BuildWith(new StructuralAnalysisFixtureSource(), catalog);
        }

        [Theory]
        [InlineData(StructuralSectionFamily.W)]
        [InlineData(StructuralSectionFamily.HssRectangular)]
        [InlineData(StructuralSectionFamily.S)]
        public void LasFamiliasWHssRectangularYSDeLaListaDePermisoSonValidas(StructuralSectionFamily family)
        {
            var result = BuildWithColumnFamily(family);

            Assert.True(result.Outcome == StructuralExportOutcome.Valid && result.Model != null, StructuralAnalysisCantileverFixtures.Describe(result));
        }

        [Theory]
        [InlineData(StructuralSectionFamily.Channel)]
        [InlineData(StructuralSectionFamily.Angle)]
        [InlineData((StructuralSectionFamily)99)]
        [InlineData((StructuralSectionFamily)(-1))]
        public void CualquierOtraFamiliaIncluidoUnValorNoDefinidoDelEnumEsUnsupported(StructuralSectionFamily family)
        {
            var result = BuildWithColumnFamily(family);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, d =>
                d.Code == StructuralDiagnosticCodes.SectionFamilyUnsupported && d.Subject == StructuralAnalysisFixtures.ColumnSection);
        }

        // ---------------------------------------------------------------- A65-F1-11: a block of the whole line is its own finding

        [Fact]
        public void ElBloqueoDeLaLineaCompletaTieneSuPropioCodigoYSuSujetoNombraLaLinea()
        {
            var design = Design();
            design.StationCount = 1; // a line needs two stations: the resolver blocks the whole line

            Assert.True(ResolveLine(design).IsBlocked);

            var result = Build(design);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceLineBlocked && d.Subject == "line");
            Assert.DoesNotContain(StructuralDiagnosticCodes.SourceStationBlocked, Codes(result));
        }

        [Fact]
        public void UnaEstacionQueNoSeResuelveBloqueaLaLineaYSeInformaComoLinea()
        {
            var design = Design();
            design.StationTopology.ColumnBaseTemplate.ColumnSectionId = "AISC-W-W99X999";

            var result = Build(design);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceLineBlocked && d.Subject == "line");
        }

        [Fact]
        public void ElCodigoDeLaEstacionBloqueadaSeConservaYEsDistintoDelDeLaLinea()
        {
            Assert.Equal("SOURCE-STATION-BLOCKED", StructuralDiagnosticCodes.SourceStationBlocked);
            Assert.NotEqual(StructuralDiagnosticCodes.SourceStationBlocked, StructuralDiagnosticCodes.SourceLineBlocked);
        }

        // ---------------------------------------------------------------- A65-F1-10: the re-orthogonalisation is bounded by IDL-08

        private static readonly double AngularLimit = Math.Sin(0.01 * Math.PI / 180.0);

        [Fact]
        public void UnaComponenteEliminadaMayorQueElSenoDeUnaCentesimaDeGradoSeRechaza()
        {
            var accepted = CantileverStructuralModelSource.TryPerpendicular(
                new StructuralDirection(0.0, 1.0, AngularLimit * 1.01), 0.0, 10.0, out _);

            Assert.False(accepted);
        }

        [Fact]
        public void UnaComponenteEliminadaDentroDeLaToleranciaDaElResultadoDeSiempre()
        {
            var accepted = CantileverStructuralModelSource.TryPerpendicular(
                new StructuralDirection(0.0, 1.0, AngularLimit * 0.99), 0.0, 10.0, out var result);

            Assert.True(accepted);
            Assert.Equal(0.0, result.X);
            Assert.True(Near(0.0, result.Z, 1e-12), "z: " + result.Z);
            Assert.True(Near(1.0, Math.Sqrt((result.X * result.X) + (result.Y * result.Y) + (result.Z * result.Z)), 1e-12));
        }

        [Fact]
        public void UnPeralteYaPerpendicularNoSeToca()
        {
            var accepted = CantileverStructuralModelSource.TryPerpendicular(new StructuralDirection(0.0, 1.0, 0.0), 0.0, 10.0, out var result);

            Assert.True(accepted);
            Assert.Equal(1.0, result.Y);
            Assert.Equal(0.0, result.Z);
        }

        // ---------------------------------------------------------------- A65-F1-12 and CD-I65-F1-14: the inventory of the station

        [Theory]
        [InlineData(CantileverMemberRole.Column)]
        [InlineData(CantileverMemberRole.Arm)]
        public void LaColumnaYElCuerpoDeBrazoSonPiezasModeladas(CantileverMemberRole role)
        {
            Assert.Equal(CantileverStructuralModelSource.StationPieceDisposition.Modelled, CantileverStructuralModelSource.Classify(role));
        }

        [Theory]
        [InlineData(CantileverMemberRole.Base)]
        [InlineData(CantileverMemberRole.Separator)]
        [InlineData(CantileverMemberRole.Brace)]
        [InlineData(CantileverMemberRole.ColdRolledAdapter)]
        public void LaBaseLosSeparadoresLosArriostramientosYSusAngulosNoSeModelanPorCdI65F114(CantileverMemberRole role)
        {
            Assert.Equal(CantileverStructuralModelSource.StationPieceDisposition.NotModelled, CantileverStructuralModelSource.Classify(role));
        }

        [Fact]
        public void UnRolSinClasificarEsUnclassified()
        {
            Assert.Equal(
                CantileverStructuralModelSource.StationPieceDisposition.Unclassified,
                CantileverStructuralModelSource.Classify((CantileverMemberRole)99));
        }

        [Fact]
        public void TodoRolDefinidoHoyEstaClasificado()
        {
            foreach (var role in Enum.GetValues(typeof(CantileverMemberRole)).Cast<CantileverMemberRole>())
            {
                Assert.NotEqual(CantileverStructuralModelSource.StationPieceDisposition.Unclassified, CantileverStructuralModelSource.Classify(role));
            }
        }

        [Fact]
        public void UnaPiezaSinteticaSinClasificarHaceLaEstacionUnsupportedConCodigoNuevo()
        {
            var diagnostics = CantileverStructuralModelSource.InventoryDiagnostics(new[]
            {
                new KeyValuePair<string, CantileverMemberRole>("CANT-S0-STN-COL", CantileverMemberRole.Column),
                new KeyValuePair<string, CantileverMemberRole>("CANT-S0-SYNTHETIC-1", (CantileverMemberRole)99),
            });

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal(StructuralDiagnosticCodes.StationPieceUnclassified, diagnostic.Code);
            Assert.Equal(StructuralExportOutcome.Unsupported, diagnostic.ImpliedOutcome);
            Assert.True(diagnostic.IsBlocking);
            Assert.Equal("CANT-S0-SYNTHETIC-1", diagnostic.Subject);
        }

        [Fact]
        public void LasPiezasClasificadasNoDanDiagnosticos()
        {
            var diagnostics = CantileverStructuralModelSource.InventoryDiagnostics(new[]
            {
                new KeyValuePair<string, CantileverMemberRole>("COL", CantileverMemberRole.Column),
                new KeyValuePair<string, CantileverMemberRole>("BASE", CantileverMemberRole.Base),
                new KeyValuePair<string, CantileverMemberRole>("ARM", CantileverMemberRole.Arm),
            });

            Assert.Empty(diagnostics);
        }

        [Fact]
        public void LasPiezasDeRefCant01SeClasificanTodasYLaLineaSigueValida()
        {
            var design = Design();
            var station = ResolveLine(design).Stations[0].Station;
            var pieces = station.Members.Select(m => new KeyValuePair<string, CantileverMemberRole>(m.Id.Value, m.Role)).ToList();

            Assert.Equal(1, pieces.Count(p => p.Value == CantileverMemberRole.Column));
            Assert.Equal(1, pieces.Count(p => p.Value == CantileverMemberRole.Base));
            Assert.Equal(3, pieces.Count(p => p.Value == CantileverMemberRole.Arm));
            Assert.Equal(5, pieces.Count);
            Assert.Empty(CantileverStructuralModelSource.InventoryDiagnostics(pieces));
            Assert.All(
                pieces.Where(p => p.Value == CantileverMemberRole.Column || p.Value == CantileverMemberRole.Arm),
                p => Assert.Equal(CantileverStructuralModelSource.StationPieceDisposition.Modelled, CantileverStructuralModelSource.Classify(p.Value)));
            Assert.Equal(
                CantileverStructuralModelSource.StationPieceDisposition.NotModelled,
                CantileverStructuralModelSource.Classify(CantileverMemberRole.Base));

            var result = Build(design);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Model);
        }
    }
}
