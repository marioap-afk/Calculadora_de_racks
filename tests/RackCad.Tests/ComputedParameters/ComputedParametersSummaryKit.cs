using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G4 - fixtures y observadores compartidos por las pruebas de <c>ProjectSummary</c>. El contrato de entrada
    /// (D-25) lo construyen las pruebas. Los contadores se observan en los costados del lector D-26 y de la resolucion
    /// (A-1.3) y en el contador unico de evaluaciones de poblacion (INV-32), nunca en los stores ni con relojes.
    /// </summary>
    internal static class ComputedParametersSummaryKit
    {
        /// <summary>El Id interno de los documentos de diseno (no es el RackId del sobre).</summary>
        internal const string InnerGuid = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";

        internal const string Garbage = "{ esto no es un diseno";

        // ---------------------------------------------------------------- disenos

        private static SelectiveCell Cell()
            => new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            };

        private static SelectivePalletDesignDocument SelectiveDocument(double clearance, params int[] levelsPerBay)
        {
            var design = new SelectivePalletDesign { PostId = "POST_A", PostPeralte = 3.0, VerticalClearance = clearance };
            foreach (var levels in levelsPerBay)
            {
                var bay = new SelectiveBayDesign();
                for (var index = 0; index < levels; index++)
                {
                    bay.Levels.Add(Cell());
                }

                design.Bays.Add(bay);
            }

            return SelectivePalletDesignDocument.From(design, InnerGuid, "Rack 1");
        }

        /// <summary>Un Selectivo cuyo fondo 0 tiene un frente por cada elemento, con ese numero de niveles (0 = vacio).</summary>
        internal static string SelectiveJson(double clearance, params int[] levelsPerBay)
            => new SelectivePalletDesignStore().Serialize(SelectiveDocument(clearance, levelsPerBay));

        /// <summary>Un Selectivo vinculado a una variable de proyecto que no existe en el registro (referencia rota).</summary>
        internal static string BrokenReferenceJson()
        {
            var document = SelectiveDocument(6.0, 2, 2);
            document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [ProjectPropertyIds.SelectiveVerticalClearanceToken] = new SelectivePropertyValueDocument
                {
                    Kind = SelectivePropertyValueDocument.ProjectVariableKind,
                    VariableId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221",
                },
            };
            return new SelectivePalletDesignStore().Serialize(document);
        }

        /// <summary>Un Push Back compuesto SIN motivo de bloqueo: la puerta de salida (D-27) lo deja en <c>Allow</c>.</summary>
        internal static string PushBackJson()
            => new RackProjectStore().Serialize(RackProject.ForPushBack(
                PushBackCompositeStructureTests.Composite(
                    slotsA: 2, slotsB: 2, deepA: 4, deepB: 4, levelsA: 2, levelsB: 2, gap: 0.0)));

        internal static string HeaderJson()
            => new RackProjectStore().Serialize(
                RackProject.ForSelective(new HardcodedStandardRackFrameService().CreateDefault()));

        internal static string CamaJson()
            => new FlowBedConfigurationStore().Serialize(new FlowBedConfiguration { LaneDepth = 96.0, PalletDepth = 48.0 });

        // ---------------------------------------------------------------- capturas

        internal static string Envelope(
            string kind, string designJson, string id, string name = "Rack", string view = RackEmbedDocument.ViewFrontal)
            => new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Id = id,
                Kind = kind,
                View = view,
                Name = name,
                Design = designJson,
            });

        internal static RackDefinitionCapture Cap(
            string key, string rackId, string kind, string designJson, int refs = 1, string name = "Rack",
            string view = RackEmbedDocument.ViewFrontal)
            => new RackDefinitionCapture(key, Envelope(kind, designJson, rackId, name, view), refs);

        internal static RackCatalogInput EmptyCatalog() => RackCatalogInput.Loaded(new RackCatalog());

        internal static RackMetricPopulationInput Input(IEnumerable<RackDefinitionCapture> definitions)
            => new RackMetricPopulationInput(definitions.ToList(), ProjectVariablesReadResult.Absent(), EmptyCatalog());

        /// <summary>Las hermanas de UN RackId, para la peticion por rack (comparador OrdinalIgnoreCase, como D-11).</summary>
        internal static List<RackDefinitionCapture> SiblingsOf(IEnumerable<RackDefinitionCapture> all, string rackId)
            => all.Where(capture => string.Equals(
                    new RackEmbedStore().Deserialize(capture.EnvelopeJson)?.Id, rackId, StringComparison.OrdinalIgnoreCase))
                .ToList();

        internal static RackMetricResults Request(IReadOnlyList<RackDefinitionCapture> siblings)
            => new RackMetricRequest(siblings, ProjectVariablesReadResult.Absent(), EmptyCatalog()).Execute();

        // ---------------------------------------------------------------- observadores

        internal sealed class CountingReader : IRackMetricDesignReader
        {
            private readonly RackMetricDesignReader _inner = new RackMetricDesignReader();

            public int Reads { get; private set; }

            public bool IsReadable(string kindToken, string designJson)
            {
                Reads++;
                return _inner.IsReadable(kindToken, designJson);
            }
        }

        internal sealed class CountingSide : IRackMetricResolutionSide
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

        /// <summary>Lo que observo una peticion <c>Full</c> con los contadores inyectados.</summary>
        internal sealed class FullObservation
        {
            public ProjectSummary Summary { get; set; }

            public int Reads { get; set; }

            public int Resolutions { get; set; }

            public int PopulationEvaluations { get; set; }

            public int ScopedResolutions { get; set; }

            public long Milliseconds { get; set; }
        }

        internal static FullObservation Full(IEnumerable<RackDefinitionCapture> captures)
        {
            var input = Input(captures);
            var reader = new CountingReader();
            var side = new CountingSide();

            using (var population = RackPopulationEvaluationCounter.Begin())
            using (var resolutions = RackMetricResolutionCounter.Begin())
            {
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var summary = ProjectSummary.Evaluate(input, reader, side);
                clock.Stop();

                return new FullObservation
                {
                    Summary = summary,
                    Reads = reader.Reads,
                    Resolutions = side.Resolutions,
                    PopulationEvaluations = population.Count,
                    ScopedResolutions = resolutions.Count,
                    Milliseconds = clock.ElapsedMilliseconds,
                };
            }
        }

        /// <summary>Lo que observo una peticion <c>Population</c> con los mismos contadores.</summary>
        internal sealed class PopulationObservation
        {
            public ProjectPopulation Population { get; set; }

            public int Reads { get; set; }

            public int PopulationEvaluations { get; set; }

            public int ScopedResolutions { get; set; }

            public long Milliseconds { get; set; }
        }

        internal static PopulationObservation Population(IEnumerable<RackDefinitionCapture> captures)
        {
            var input = Input(captures);
            var reader = new CountingReader();

            using (var population = RackPopulationEvaluationCounter.Begin())
            using (var resolutions = RackMetricResolutionCounter.Begin())
            {
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var result = ProjectPopulation.Evaluate(input, reader);
                clock.Stop();

                return new PopulationObservation
                {
                    Population = result,
                    Reads = reader.Reads,
                    PopulationEvaluations = population.Count,
                    ScopedResolutions = resolutions.Count,
                    Milliseconds = clock.ElapsedMilliseconds,
                };
            }
        }

        // ---------------------------------------------------------------- escenario mixto acreditado

        internal const string R1 = "rack-1";
        internal const string R2 = "rack-2";
        internal const string R3 = "rack-3";
        internal const string R4 = "rack-4";
        internal const string R5 = "rack-5";
        internal const string R6 = "rack-6";
        internal const string R7 = "rack-7";

        /// <summary>
        /// Siete RackIds sin cobertura rota: R1 Selectivo colocado de tres vistas (4 frentes, 1 vacio); R2 Selectivo NO
        /// colocado (2 frentes); R3 Push Back; R4 Cabecera; R5 Cama; R6 sin Kind; R7 Selectivo de hermanas divergentes.
        /// </summary>
        internal static List<RackDefinitionCapture> AccreditedMix()
        {
            var four = SelectiveJson(6.0, 2, 2, 0, 2);
            return new List<RackDefinitionCapture>
            {
                Cap("K1-A", R1, RackEmbedDocument.KindSelective, four, refs: 1, name: "Rack Uno", view: RackEmbedDocument.ViewFrontal),
                Cap("K1-B", R1, RackEmbedDocument.KindSelective, four, refs: 0, name: "", view: RackEmbedDocument.ViewPlanta),
                Cap("K1-C", R1, RackEmbedDocument.KindSelective, four, refs: 0, name: "", view: RackEmbedDocument.ViewLateral),
                Cap("K2-A", R2, RackEmbedDocument.KindSelective, SelectiveJson(6.0, 2, 2), refs: 0, name: "Rack Dos"),
                Cap("K3-A", R3, RackEmbedDocument.KindPushBack, PushBackJson(), refs: 1, name: "Rack PB"),
                Cap("K4-A", R4, RackEmbedDocument.KindCabecera, HeaderJson(), refs: 1, name: "Cabecera"),
                Cap("K5-A", R5, RackEmbedDocument.KindCama, CamaJson(), refs: 1, name: "Cama"),
                Cap("K6-A", R6, string.Empty, four, refs: 1, name: "Sin kind"),
                Cap("K7-A", R7, RackEmbedDocument.KindSelective, SelectiveJson(6.0, 2, 2, 2), refs: 1, name: "Divergente"),
                Cap("K7-B", R7, RackEmbedDocument.KindSelective, SelectiveJson(9.0, 2, 2, 2), refs: 0, name: "Divergente"),
            };
        }

        /// <summary><paramref name="racks"/> RackIds Selectivos de tres vistas (frontal colocada, planta y lateral sin colocar).</summary>
        internal static List<RackDefinitionCapture> SelectiveRacksWithThreeViews(int racks)
        {
            var json = SelectiveJson(6.0, 2, 2, 0, 2);
            var captures = new List<RackDefinitionCapture>(racks * 3);
            for (var index = 0; index < racks; index++)
            {
                var id = "rack-" + index.ToString("D4", System.Globalization.CultureInfo.InvariantCulture);
                captures.Add(Cap(id + "-F", id, RackEmbedDocument.KindSelective, json, refs: 1, name: "Rack " + index, view: RackEmbedDocument.ViewFrontal));
                captures.Add(Cap(id + "-P", id, RackEmbedDocument.KindSelective, json, refs: 0, name: "Rack " + index, view: RackEmbedDocument.ViewPlanta));
                captures.Add(Cap(id + "-L", id, RackEmbedDocument.KindSelective, json, refs: 0, name: "Rack " + index, view: RackEmbedDocument.ViewLateral));
            }

            return captures;
        }

        internal static string Describe(MetricValue value)
            => value.Status == MetricStatus.Available
                ? "Available(" + value.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + ")"
                : value.ToString();
    }
}
