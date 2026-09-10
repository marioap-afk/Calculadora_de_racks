using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G8 — el barrido fisico deja de mentir sobre lo que no entiende.
    ///
    /// <para>
    /// El conteo de referencias estaba atado a que el sobre deserializara. Con esa forma, una definicion
    /// COLOCADA cuyo sobre no se puede interpretar reportaba cero referencias, y quien la mirase concluiria
    /// que no esta en el dibujo. Es la misma clase de defecto que el resto del contrato persigue: un estado
    /// desconocido presentandose como uno conocido y benigno.
    /// </para>
    /// <para>
    /// El conteo vive en el <c>BlockTableRecord</c> y no depende del payload, asi que puede —y debe—
    /// calcularse igual. Lo que este build no sabe es de QUE rack es esa definicion, y eso no se inventa: se
    /// dice.
    /// </para>
    /// <para>
    /// La proyeccion es pura y por eso se prueba aqui; la mitad que recorre la <c>BlockTable</c> vive en el
    /// Plugin, que ninguna suite carga (ADR-0003), y se fija con una guarda de fuente — el mismo instrumento
    /// que el repositorio ya usa para propiedades que solo existen como orden y propiedad en el codigo.
    /// </para>
    /// </summary>
    public class ProjectVariableScanProjectionTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";

        private static string DisenoJson(double clearance = 6.0)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);

            return new SelectivePalletDesignStore()
                .Serialize(SelectivePalletDesignDocument.From(design, RackId, "Rack 1"));
        }

        private static RackEmbedDocument Sobre(string kind = RackEmbedDocument.KindSelective, string id = RackId, string design = null)
            => new RackEmbedDocument
            {
                Id = id,
                Kind = kind,
                View = RackEmbedDocument.ViewFrontal,
                Name = "Rack 1",
                Design = design ?? DisenoJson(),
            };

        // ================================================================ el conteo deja de depender del sobre

        [Fact]
        public void SobreLEGIBLE_ConDosReferencias_CuentaDos()
        {
            var e = ProjectVariableScanProjection.Project("DEF-1", Sobre(), directReferenceCount: 2);

            Assert.Equal(2, e.DirectReferenceCount);
            Assert.True(e.OuterEnvelopeInterpretable);
            Assert.True(e.AuthoredReadable);
        }

        [Fact]
        public void SobreILEGIBLE_ConDosReferencias_SIGUE_CONTANDO_DOS()
        {
            // El caso que da nombre al gate: la definicion ESTA colocada, aunque no sepamos de quien es.
            var e = ProjectVariableScanProjection.Project("DEF-1", null, directReferenceCount: 2);

            Assert.Equal(2, e.DirectReferenceCount);
            Assert.False(e.OuterEnvelopeInterpretable);
        }

        [Fact]
        public void SobreILEGIBLE_SinReferencias_CuentaCero()
        {
            var e = ProjectVariableScanProjection.Project("DEF-1", null, directReferenceCount: 0);

            Assert.Equal(0, e.DirectReferenceCount);
            Assert.False(e.OuterEnvelopeInterpretable);
        }

        // ================================================================ que se puede afirmar y que no

        [Fact]
        public void UnSobreILEGIBLE_ES_INDETERMINADO_Y_NO_INVENTA_IDENTIDAD()
        {
            var e = ProjectVariableScanProjection.Project("DEF-X", null, 1);

            Assert.False(e.OuterEnvelopeInterpretable);
            Assert.Null(e.RackId);
            Assert.Null(e.Kind);
            Assert.Equal("DEF-X", e.DefinitionId);
            Assert.True(e.RackCadDataPresent);
        }

        [Fact]
        public void TodaEntradaDelBarridoLLEVA_DATOS_DE_RACKCAD()
        {
            // El barrido descarta las definiciones sin payload antes de proyectarlas, asi que una entrada
            // proyectada nunca es "un bloque cualquiera del dibujo".
            Assert.True(ProjectVariableScanProjection.Project("DEF-1", null, 0).RackCadDataPresent);
            Assert.True(ProjectVariableScanProjection.Project("DEF-1", Sobre(), 0).RackCadDataPresent);
        }

        [Fact]
        public void UnRackDeOTRO_KIND_ES_AJENO_NoIndeterminado()
        {
            var e = ProjectVariableScanProjection.Project("DEF-1", Sobre(kind: RackEmbedDocument.KindPushBack), 1);

            Assert.True(e.OuterEnvelopeInterpretable);
            Assert.False(e.IsSelective);
            Assert.Equal(RackId, e.RackId);
        }

        [Fact]
        public void UnSelectivoConDISENO_ILEGIBLE_ConservaElRackIdPeroNoElAuthored()
        {
            var e = ProjectVariableScanProjection.Project("DEF-1", Sobre(design: "{no es json"), 1);

            Assert.True(e.OuterEnvelopeInterpretable);
            Assert.True(e.IsSelective);
            Assert.Equal(RackId, e.RackId);
            Assert.False(e.AuthoredReadable);
            Assert.Null(e.Authored);
        }

        [Fact]
        public void UnSelectivoConDISENO_DE_MAJOR_FUTURO_TampocoSeInterpreta()
        {
            var futuro = DisenoJson().Replace("\"SchemaVersion\":\"1.0\"", "\"SchemaVersion\":\"9.0\"");

            var e = ProjectVariableScanProjection.Project("DEF-1", Sobre(design: futuro), 1);

            Assert.True(e.IsSelective);
            Assert.False(e.AuthoredReadable);
        }

        [Theory]
        [InlineData(null, RackEmbedDocument.KindSelective)]
        [InlineData("", RackEmbedDocument.KindSelective)]
        [InlineData("   ", RackEmbedDocument.KindSelective)]
        [InlineData(RackId, null)]
        [InlineData(RackId, "")]
        public void UnSobreSIN_IDENTIDAD_UTILIZABLE_ES_INDETERMINADO(string id, string kind)
        {
            // El sobre "se leyo" pero no permite clasificar. Inventar el RackId seria peor que no saberlo.
            var e = ProjectVariableScanProjection.Project("DEF-1", Sobre(kind: kind, id: id), 1);

            Assert.False(e.OuterEnvelopeInterpretable);
            Assert.Null(e.RackId);
        }

        // ================================================================ enlaza con G5

        [Fact]
        public void UnaEntradaProyectadaSEDECIDE_CON_LAS_REGLAS_DE_G5()
        {
            var entradas = new[]
            {
                ProjectVariableScanProjection.Project("DEF-1", Sobre(), 1),
                ProjectVariableScanProjection.Project("DEF-X", null, 1),
            };

            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                entradas,
                VariableId.Parse("8a1d4e77-2c93-4b60-8f15-6e0b93a7c221"));

            Assert.False(r.IsSuccess);
            Assert.Contains("DEF-X", r.Error);
        }

        [Fact]
        public void UnSobreILEGIBLE_ABORTA_INDEPENDIENTEMENTE_DEL_CONTEO()
        {
            // Para operaciones de variable la asimetria del BOM no aplica: colocada o no, aborta.
            foreach (var count in new[] { 0, 3 })
            {
                var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                    new[] { ProjectVariableScanProjection.Project("DEF-X", null, count) },
                    VariableId.Parse("8a1d4e77-2c93-4b60-8f15-6e0b93a7c221"));

                Assert.False(r.IsSuccess);
            }
        }

        // ================================================================ guarda de fuente sobre el Plugin

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return dir;
        }

        private static string PluginSource(string relative)
            => File.ReadAllText(Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin", relative));

        /// <summary>
        /// La propiedad que ninguna suite puede ejecutar: el conteo NO puede volver a condicionarse a que el
        /// sobre deserialice. Se fija como texto porque el Plugin referencia AutoCAD y no es cargable aqui.
        /// </summary>
        [Fact]
        public void GUARDA_ElConteoNoVuelveAdependerDeQueElSobreDeserialice()
        {
            var source = PluginSource("RackBlockFinder.cs");

            Assert.DoesNotContain("includeReferenceCount && embed != null", source);
            Assert.Matches(
                new Regex(@"includeReferenceCount\s*\r?\n?\s*\?\s*record\.GetBlockReferenceIds", RegexOptions.Singleline),
                source);
        }

        [Fact]
        public void GUARDA_ElBarridoSIGUE_DESCARTANDO_LasDefinicionesSinDatosDeRackCad()
        {
            var source = PluginSource("RackBlockFinder.cs");

            Assert.Contains("if (string.IsNullOrEmpty(json))", source);
        }
    }
}
