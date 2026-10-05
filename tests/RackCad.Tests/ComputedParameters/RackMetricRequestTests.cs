using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;
using Xunit.Abstractions;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G1 — la peticion por rack (D-17) con la precedencia D-28. Los contadores se observan en los costados
    /// del lector D-26 y de la resolucion (A-1.3), nunca en los stores.
    /// </summary>
    public class ComputedParametersRackMetricRequestTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string BeamId = "BEAM_A";

        private readonly ITestOutputHelper _output;

        public ComputedParametersRackMetricRequestTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ---------------------------------------------------------------- fixtures

        private static SelectiveCell Cell()
            => new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = BeamId,
                BeamPeralte = 4.5,
            };

        /// <summary>Un frente de diseno con <paramref name="levels"/> celdas (0 = sin celdas).</summary>
        private static SelectiveBayDesign Bay(int levels, bool floorBeam = false)
        {
            var bay = new SelectiveBayDesign { FloorBeam = floorBeam };
            for (var i = 0; i < levels; i++)
            {
                bay.Levels.Add(Cell());
            }

            return bay;
        }

        private static SelectivePalletDesign Design(params SelectiveBayDesign[] fondo0)
        {
            var design = new SelectivePalletDesign { PostId = "POST_A", PostPeralte = 3.0 };
            foreach (var bay in fondo0)
            {
                design.Bays.Add(bay);
            }

            return design;
        }

        private static string DesignJson(SelectivePalletDesign design)
            => new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, RackId, "Rack 1"));

        private static string Envelope(string kind, string designJson, string id = RackId, string view = RackEmbedDocument.ViewFrontal)
            => new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Id = id,
                Kind = kind,
                View = view,
                Name = "Rack 1",
                Design = designJson,
            });

        private static RackDefinitionCapture Capture(string key, string kind, string designJson, int references = 1)
            => new RackDefinitionCapture(key, Envelope(kind, designJson), references);

        /// <summary>Las dos vistas (frontal y planta) de un Selectivo, con el mismo diseno.</summary>
        private static List<RackDefinitionCapture> SelectiveViews(SelectivePalletDesign design)
        {
            var json = DesignJson(design);
            return new List<RackDefinitionCapture>
            {
                Capture("DEF-A", RackEmbedDocument.KindSelective, json),
                Capture("DEF-B", RackEmbedDocument.KindSelective, json, references: 0),
            };
        }

        private static string PushBackJson()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0,
                },
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Fronts.Add(new PushBackFrontConfig());
            return new RackProjectStore().Serialize(RackProject.ForPushBack(design));
        }

        private static string HeaderJson()
            => new RackProjectStore().Serialize(RackProject.ForSelective(new HardcodedStandardRackFrameService().CreateDefault()));

        private sealed class CountingReader : IRackMetricDesignReader
        {
            private readonly RackMetricDesignReader _inner = new RackMetricDesignReader();

            public int Reads { get; private set; }

            public bool IsReadable(string kindToken, string designJson)
            {
                Reads++;
                return _inner.IsReadable(kindToken, designJson);
            }
        }

        private sealed class CountingSide : IRackMetricResolutionSide
        {
            private readonly RackMetricResolutionSide _inner = new RackMetricResolutionSide();

            public int Resolutions { get; private set; }

            public RackMetricResolutionResult Resolve(
                SelectivePalletDesignDocument authored, ProjectVariablesReadResult registry, RackCatalogInput catalog)
            {
                Resolutions++;
                return _inner.Resolve(authored, registry, catalog);
            }
        }

        private sealed class Observation
        {
            public RackMetricResults Results { get; set; }

            public int Reads { get; set; }

            public int Resolutions { get; set; }
        }

        private Observation Run(string scenario, IReadOnlyList<RackDefinitionCapture> siblings)
        {
            var reader = new CountingReader();
            var side = new CountingSide();
            var results = new RackMetricRequest(
                siblings,
                ProjectVariablesReadResult.Absent(),
                RackCatalogInput.Loaded(new RackCatalog()),
                reader,
                side).Execute();

            _output.WriteLine(
                scenario + " | resoluciones=" + side.Resolutions + " lecturas=" + reader.Reads
                + " | frentes=" + results[RackMetricIds.Frentes] + " frentesVacios=" + results[RackMetricIds.FrentesVacios]);

            return new Observation { Results = results, Reads = reader.Reads, Resolutions = side.Resolutions };
        }

        private static void AssertBoth(RackMetricResults results, MetricStatus status, UnavailableReasonKind? reason = null)
        {
            foreach (var metric in RackMetricIds.RackMetrics)
            {
                Assert.Equal(status, results[metric].Status);
                if (reason.HasValue)
                {
                    Assert.NotNull(results[metric].Reason);
                    Assert.Equal(reason.Value, results[metric].Reason.Kind);
                }
            }
        }

        // ---------------------------------------------------------------- INV-04

        [Fact]
        public void INV04_Fondo0Con4FrentesUnoVacio_Y_Fondo1Con6_FrentesEsCuatro()
        {
            var design = Design(Bay(2), Bay(2), Bay(0), Bay(2));
            design.DepthCount = 2;
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2), Bay(2), Bay(2), Bay(2), Bay(2), Bay(2) });

            var observation = Run("INV-04 fondo0=4 (1 vacio), fondo1=6", SelectiveViews(design));

            Assert.Equal(MetricStatus.Available, observation.Results[RackMetricIds.Frentes].Status);
            Assert.Equal(4.0, observation.Results[RackMetricIds.Frentes].Value);
            Assert.Equal(1.0, observation.Results[RackMetricIds.FrentesVacios].Value);
        }

        [Fact]
        public void INV04_DisenoSinCeldas_DaLoMismo_LasBahiasDelFondo0()
        {
            var design = Design(Bay(0), Bay(0), Bay(0), Bay(0));
            design.DepthCount = 2;
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(1), Bay(1), Bay(1), Bay(1), Bay(1), Bay(1) });

            var observation = Run("INV-04 diseno sin celdas en el fondo 0", SelectiveViews(design));

            Assert.Equal(MetricStatus.Available, observation.Results[RackMetricIds.Frentes].Status);
            Assert.Equal(4.0, observation.Results[RackMetricIds.Frentes].Value);
            Assert.Equal(4.0, observation.Results[RackMetricIds.FrentesVacios].Value);
        }

        // ---------------------------------------------------------------- INV-05

        [Fact]
        public void INV05_TarimaDePisoSinLarguero_NoEsVacia_UnFrenteSinCeldasSi()
        {
            // frente 0: una sola celda sin larguero a piso => tarima de piso, ningun nivel (NO vacio).
            // frente 1: sin celdas (vacio). frente 2: larguero a piso con una celda (un nivel, NO vacio).
            var design = Design(Bay(1), Bay(0), Bay(1, floorBeam: true));

            var observation = Run("INV-05 piso sin larguero / sin celdas / larguero a piso", SelectiveViews(design));

            Assert.Equal(3.0, observation.Results[RackMetricIds.Frentes].Value);
            Assert.Equal(1.0, observation.Results[RackMetricIds.FrentesVacios].Value);
        }

        // ---------------------------------------------------------------- INV-14 + A-1.3

        [Fact]
        public void INV14_Selectivo_UnaSolaResolucion_EnElCostadoDeResolucion()
        {
            var observation = Run("INV-14 selectivo valido", SelectiveViews(Design(Bay(2), Bay(2), Bay(2), Bay(2))));

            Assert.Equal(MetricStatus.Available, observation.Results[RackMetricIds.Frentes].Status);
            Assert.Equal(1, observation.Resolutions);
        }

        [Fact]
        public void INV14_PushBack_CeroResolucionesYCeroLecturas_EnElCostadoDelLector()
        {
            var json = PushBackJson();

            // Control positivo: el diseno ES legible, asi que el 0 observado no es un diseno que fallo al leerse.
            Assert.True(new RackMetricDesignReader().IsReadable(RackEmbedDocument.KindPushBack, json));

            var observation = Run(
                "INV-14 pushback legible",
                new List<RackDefinitionCapture> { Capture("DEF-A", RackEmbedDocument.KindPushBack, json) });

            AssertBoth(observation.Results, MetricStatus.NotSupported);
            Assert.Equal(0, observation.Reads);
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV14_Cabecera_CeroResolucionesYCeroLecturas_EnElCostadoDelLector()
        {
            var json = HeaderJson();

            Assert.True(new RackMetricDesignReader().IsReadable(RackEmbedDocument.KindCabecera, json));

            var observation = Run(
                "INV-14 cabecera legible",
                new List<RackDefinitionCapture> { Capture("DEF-A", RackEmbedDocument.KindCabecera, json) });

            AssertBoth(observation.Results, MetricStatus.NotApplicable);
            Assert.Equal(0, observation.Reads);
            Assert.Equal(0, observation.Resolutions);
        }

        // ---------------------------------------------------------------- INV-16

        [Fact]
        public void INV16_KindsMezclados_DanUnavailableKindIncoherent()
        {
            var selective = DesignJson(Design(Bay(2)));
            var pushBack = PushBackJson();

            var observation = Run(
                "INV-16 kinds mezclados",
                new List<RackDefinitionCapture>
                {
                    Capture("DEF-A", RackEmbedDocument.KindSelective, selective),
                    Capture("DEF-B", RackEmbedDocument.KindPushBack, pushBack),
                });

            AssertBoth(observation.Results, MetricStatus.Unavailable, UnavailableReasonKind.KindIncoherent);
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV16_KindAusente_DaUnavailableKindAbsent()
        {
            var observation = Run(
                "INV-16 kind ausente",
                new List<RackDefinitionCapture> { Capture("DEF-A", string.Empty, DesignJson(Design(Bay(2)))) });

            AssertBoth(observation.Results, MetricStatus.Unavailable, UnavailableReasonKind.KindAbsent);
            Assert.Equal(0, observation.Resolutions);
        }

        // ---------------------------------------------------------------- INV-34 (peticion) + A-1.3

        [Fact]
        public void INV34_PushBackConDisenoIlegible_NotSupported_ConCeroLecturasYCeroResoluciones()
        {
            // Control positivo: ese texto SI es ilegible para el lector, de modo que leerlo antes del paso 2
            // (E5 antes del soporte) daria Unavailable(DesignUnreadable) y lecturas > 0.
            Assert.False(new RackMetricDesignReader().IsReadable(RackEmbedDocument.KindPushBack, "{ esto no es un diseno"));

            var observation = Run(
                "INV-34 pushback ilegible",
                new List<RackDefinitionCapture> { Capture("DEF-A", RackEmbedDocument.KindPushBack, "{ esto no es un diseno") });

            AssertBoth(observation.Results, MetricStatus.NotSupported);
            Assert.Equal(0, observation.Reads);
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV34_CabeceraConDisenoIlegible_NotApplicable_ConCeroLecturasYCeroResoluciones()
        {
            var observation = Run(
                "INV-34 cabecera ilegible",
                new List<RackDefinitionCapture> { Capture("DEF-A", RackEmbedDocument.KindCabecera, "{ esto no es un diseno") });

            AssertBoth(observation.Results, MetricStatus.NotApplicable);
            Assert.Equal(0, observation.Reads);
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV34_SelectivoConHermanasDivergentes_SiblingsDivergent_ConCeroResoluciones()
        {
            var one = Design(Bay(2), Bay(2));
            var other = Design(Bay(2), Bay(2));
            other.VerticalClearance = one.VerticalClearance + 2.0;

            var observation = Run(
                "INV-34 selectivo divergente",
                new List<RackDefinitionCapture>
                {
                    Capture("DEF-A", RackEmbedDocument.KindSelective, DesignJson(one)),
                    Capture("DEF-B", RackEmbedDocument.KindSelective, DesignJson(other)),
                });

            AssertBoth(observation.Results, MetricStatus.Unavailable, UnavailableReasonKind.SiblingsDivergent);
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV34_SelectivoConDisenoIlegible_DesignUnreadable_ConCeroResoluciones()
        {
            var observation = Run(
                "INV-34 selectivo ilegible",
                new List<RackDefinitionCapture>
                {
                    Capture("DEF-A", RackEmbedDocument.KindSelective, DesignJson(Design(Bay(2)))),
                    Capture("DEF-B", RackEmbedDocument.KindSelective, "{ esto no es un diseno"),
                });

            AssertBoth(observation.Results, MetricStatus.Unavailable, UnavailableReasonKind.DesignUnreadable);
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void D17_HermanasSinNingunRackIdIdentificable_LanzaArgumentException()
        {
            var illegible = new List<RackDefinitionCapture>
            {
                new RackDefinitionCapture("DEF-A", "{ esto no es un sobre", 1),
            };
            var blankId = new List<RackDefinitionCapture>
            {
                new RackDefinitionCapture("DEF-B", Envelope(RackEmbedDocument.KindSelective, DesignJson(Design(Bay(2))), id: "  "), 1),
            };

            foreach (var siblings in new[] { illegible, blankId })
            {
                var request = new RackMetricRequest(
                    siblings, ProjectVariablesReadResult.Absent(), RackCatalogInput.Loaded(new RackCatalog()));
                Assert.Throws<System.ArgumentException>(() => request.Execute());
            }
        }

        [Fact]
        public void INV34_SelectivoValido_Available()
        {
            var observation = Run("INV-34 selectivo valido", SelectiveViews(Design(Bay(2), Bay(2), Bay(2), Bay(2))));

            AssertBoth(observation.Results, MetricStatus.Available);
            Assert.Equal(4.0, observation.Results[RackMetricIds.Frentes].Value);
            Assert.Equal(0.0, observation.Results[RackMetricIds.FrentesVacios].Value);
            Assert.Equal(RackId, observation.Results.RackId);
        }
    }
}
