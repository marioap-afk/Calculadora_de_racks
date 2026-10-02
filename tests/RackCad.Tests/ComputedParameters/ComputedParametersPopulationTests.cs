using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Views.Insertion;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;
using Xunit.Abstractions;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G2 — la poblacion cotizable (D-10, D-10a, D-11), los agregados (D-12), el veredicto unico de salida
    /// (D-27) y el contador unico de evaluaciones de poblacion (INV-32). Todo sobre un contrato de entrada
    /// sintetico (D-25) que construyen estas pruebas.
    /// </summary>
    public class ComputedParametersPopulationTests
    {
        private const string RackGuid = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string OtherGuid = "9c1d2e3f-4a5b-4c6d-8e7f-0a1b2c3d4e5f";
        private const string BeamId = "BEAM_A";
        private const string Garbage = "{ esto no es un diseno";

        private static readonly string[] SixTokens =
        {
            RackEmbedDocument.KindSelective,
            RackEmbedDocument.KindDynamic,
            RackEmbedDocument.KindPushBack,
            RackEmbedDocument.KindCantilever,
            RackEmbedDocument.KindCabecera,
            RackEmbedDocument.KindCama,
        };

        private readonly ITestOutputHelper _output;

        public ComputedParametersPopulationTests(ITestOutputHelper output)
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

        private static SelectiveBayDesign Bay(int levels)
        {
            var bay = new SelectiveBayDesign();
            for (var i = 0; i < levels; i++)
            {
                bay.Levels.Add(Cell());
            }

            return bay;
        }

        private static SelectivePalletDesign Design(double clearance, params SelectiveBayDesign[] bays)
        {
            var design = new SelectivePalletDesign { PostId = "POST_A", PostPeralte = 3.0, VerticalClearance = clearance };
            foreach (var bay in bays)
            {
                design.Bays.Add(bay);
            }

            return design;
        }

        private static string SelectiveJson(double clearance = 6.0, int bays = 4)
            => new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(
                Design(clearance, Enumerable.Range(0, bays).Select(_ => Bay(2)).ToArray()), RackGuid, "Rack 1"));

        /// <summary>Un Selectivo vinculado a una variable de proyecto que no existe en el registro (referencia rota).</summary>
        private static string BrokenReferenceJson()
        {
            var document = SelectivePalletDesignDocument.From(Design(6.0, Bay(2), Bay(2)), RackGuid, "Rack 1");
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

        private static string Envelope(string kind, string designJson, string id = RackGuid, string name = "Rack 1")
            => new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Id = id,
                Kind = kind,
                View = RackEmbedDocument.ViewFrontal,
                Name = name,
                Design = designJson,
            });

        private static RackDefinitionCapture Cap(
            string key, string kind, string designJson, int refs = 1, string id = RackGuid, string name = "Rack 1")
            => new RackDefinitionCapture(key, Envelope(kind, designJson, id, name), refs);

        private static RackDefinitionCapture Raw(string key, string envelopeJson, int refs = 1)
            => new RackDefinitionCapture(key, envelopeJson, refs);

        private static PushBackDesign PushBackDesignFor(bool blocking)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: blocking ? 8 : 4, deepB: blocking ? 8 : 4, levelsA: 2, levelsB: 2, gap: 0.0);
            if (blocking)
            {
                design.Composite.StructureOverrideA = PushBackCellDepth.MinimumPalletsDeep;
                design.Composite.StructureOverrideB = PushBackCellDepth.MinimumPalletsDeep;
            }

            return design;
        }

        private static string PushBackJson(bool blocking = false)
            => new RackProjectStore().Serialize(RackProject.ForPushBack(PushBackDesignFor(blocking)));

        private static string DynamicJson()
            => new RackProjectStore().Serialize(RackProject.ForDynamic(
                DimensionViewScenarios.DynamicPersistedDesign(
                    DimensionDetail.Standard, JsonRackCatalogProvider.FromBaseDirectory().Load())));

        private static string HeaderJson()
            => new RackProjectStore().Serialize(
                RackProject.ForSelective(new RackCad.Application.RackFrames.HardcodedStandardRackFrameService().CreateDefault()));

        private static string CamaJson()
            => new FlowBedConfigurationStore().Serialize(new FlowBedConfiguration { LaneDepth = 96.0, PalletDepth = 48.0 });

        private static string CantileverJson()
        {
            var topology = new CantileverLineStationTopologyDesign
            {
                FaceMode = CantileverStationFaceMode.Single,
                SingleSide = CantileverArmSide.PositiveY,
                LevelCount = 2,
                RequestedClearHeight = 24.0,
                ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                {
                    ColumnSectionId = "AISC-W-W10X33",
                    Base = new CantileverBaseDesign { SectionId = "AISC-W-W12X26", Length = 48.0 },
                },
            };
            topology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            topology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = 4.0;

            var design = new CantileverLineDesign
            {
                Name = "Linea A",
                StationCount = 3,
                ColumnCentreSpacing = 96.0,
                StationTopology = topology,
                DefaultArmTemplate = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign
                    {
                        Arrangement = CantileverArmBodyArrangement.Single,
                        SectionId = "AISC-HSS-RECT-HSS4X4X_250",
                        CutLength = 36.0,
                    },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign
                    {
                        VerticalPunchCount = 2,
                        VerticalEndOffset = 1.5,
                    },
                },
                Bracing = new CantileverBracingDesign(),
            };

            return new RackProjectStore().Serialize(RackProject.ForCantilever(design));
        }

        private static string ValidJson(string kind)
        {
            switch (kind)
            {
                case RackEmbedDocument.KindSelective: return SelectiveJson();
                case RackEmbedDocument.KindDynamic: return DynamicJson();
                case RackEmbedDocument.KindPushBack: return PushBackJson();
                case RackEmbedDocument.KindCantilever: return CantileverJson();
                case RackEmbedDocument.KindCabecera: return HeaderJson();
                case RackEmbedDocument.KindCama: return CamaJson();
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private sealed class ThrowingList<T> : IReadOnlyList<T>
        {
            public T this[int index] => throw new InvalidOperationException("catalogo roto");

            public int Count => throw new InvalidOperationException("catalogo roto");

            public IEnumerator<T> GetEnumerator() => throw new InvalidOperationException("catalogo roto");

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                => throw new InvalidOperationException("catalogo roto");
        }

        /// <summary>Un catalogo cuyo resolver LANZA al medir: su lista de postes no se puede enumerar.</summary>
        private static RackCatalog ThrowingCatalog()
            => new RackCatalog { PostProfiles = new ThrowingList<ProfileCatalogEntry>() };

        private sealed class AlwaysReadable : IRackMetricDesignReader
        {
            public bool IsReadable(string kindToken, string designJson) => true;
        }

        private static RackCatalogInput EmptyCatalog() => RackCatalogInput.Loaded(new RackCatalog());

        private static RackMetricPopulationInput Input(
            IEnumerable<RackDefinitionCapture> definitions, RackCatalogInput catalog = null)
            => new RackMetricPopulationInput(
                definitions.ToList(), RackCad.Application.Persistence.ProjectVariablesReadResult.Absent(), catalog ?? EmptyCatalog());

        private static ProjectPopulation Evaluate(
            IEnumerable<RackDefinitionCapture> definitions,
            RackCatalogInput catalog = null,
            IRackMetricDesignReader reader = null)
            => ProjectPopulation.Evaluate(Input(definitions, catalog), reader);

        private static PopulationRack Rack(ProjectPopulation population, string rackId)
            => population.Racks.FirstOrDefault(rack => string.Equals(rack.RackId, rackId, StringComparison.Ordinal));

        /// <summary>La pertenencia de un rack; si el rack no esta en la poblacion, falla por asercion (nunca por referencia nula).</summary>
        private static RackMembership MembershipOf(ProjectPopulation population, string rackId)
        {
            var rack = Rack(population, rackId);
            Assert.True(rack != null, "El rack " + rackId + " no esta en la poblacion.");
            return rack.Membership;
        }

        private static void AssertAvailable(MetricValue value, double expected)
        {
            Assert.NotNull(value);
            Assert.Equal(MetricStatus.Available, value.Status);
            Assert.Equal(expected, value.Value);
        }

        private static void AssertUnavailable(MetricValue value, UnavailableReasonKind reason)
        {
            Assert.NotNull(value);
            Assert.Equal(MetricStatus.Unavailable, value.Status);
            Assert.NotNull(value.Reason);
            Assert.Equal(reason, value.Reason.Kind);
        }

        /// <summary>
        /// Las metricas por rack que en G4 entregara <c>ProjectSummary Full</c>: aqui las produce la peticion de G1 sobre
        /// las hermanas de cada RackId (OrdinalIgnoreCase), indexadas por su grafia canonica (la minima en Ordinal).
        /// No depende de la poblacion bajo prueba.
        /// </summary>
        private static Dictionary<string, RackMetricResults> MetricsOf(IReadOnlyList<RackDefinitionCapture> captures)
        {
            var metrics = new Dictionary<string, RackMetricResults>(StringComparer.Ordinal);
            var envelopes = captures
                .Select(capture => new { Capture = capture, Envelope = new RackEmbedStore().Deserialize(capture.EnvelopeJson) })
                .Where(item => item.Envelope != null && !string.IsNullOrWhiteSpace(item.Envelope.Id))
                .ToList();

            foreach (var group in envelopes.GroupBy(item => item.Envelope.Id, StringComparer.OrdinalIgnoreCase))
            {
                var spelling = group.Select(item => item.Envelope.Id).OrderBy(id => id, StringComparer.Ordinal).First();
                metrics[spelling] = new RackMetricRequest(
                    group.Select(item => item.Capture).ToList(),
                    RackCad.Application.Persistence.ProjectVariablesReadResult.Absent(),
                    EmptyCatalog()).Execute();
            }

            return metrics;
        }

        private static string Snapshot(ProjectPopulation population, ProjectPopulationAggregates aggregates)
        {
            var lines = new List<string> { "total=" + population.TotalRacks, "coverage=" + population.CoverageAccredited };
            lines.AddRange(population.Racks.Select(rack =>
                "rack|" + rack.RackId + "|" + rack.KindToken + "|" + rack.DisplayName + "|" + rack.Membership
                + "|" + rack.RepresentativeDefinitionId + "|" + string.Join(",", rack.DefinitionKeys)));
            lines.AddRange(population.Diagnostics.Select(item => "diag|" + item));
            lines.AddRange(population.RackCountBySystem.Select(item => "count|" + item.KindToken + "|" + item.RackCount));
            lines.AddRange(aggregates.BySystem.Select(item =>
                "agg|" + item.KindToken + "|" + item.RackCount + "|" + item.TotalFrentes + "|" + item.TotalFrentesVacios));
            return string.Join("\n", lines);
        }

        // ---------------------------------------------------------------- INV-01

        [Fact]
        public void INV01_TresDefinicionesConReferencias_2_1_0_DelMismoRackId_SonUnSoloRack()
        {
            var json = SelectiveJson();
            var population = Evaluate(new[]
            {
                Cap("DEF-A", RackEmbedDocument.KindSelective, json, refs: 2),
                Cap("DEF-B", RackEmbedDocument.KindSelective, json, refs: 1),
                Cap("DEF-C", RackEmbedDocument.KindSelective, json, refs: 0),
            });

            _output.WriteLine("INV-01 racks=" + population.Racks.Count + " total=" + population.TotalRacks);

            AssertAvailable(population.TotalRacks, 1);
            Assert.Single(population.Racks);
            Assert.Equal(RackMembership.Included, population.Racks[0].Membership);
            Assert.Equal(new[] { "DEF-A", "DEF-B", "DEF-C" }, population.Racks[0].DefinitionKeys);
        }

        [Fact]
        public void INV01_VariasVistasYColocacionesNoAumentanElConteo_DosRacksDan2()
        {
            var json = SelectiveJson();
            var population = Evaluate(new[]
            {
                Cap("A-1", RackEmbedDocument.KindSelective, json, refs: 3, id: RackGuid),
                Cap("A-2", RackEmbedDocument.KindSelective, json, refs: 5, id: RackGuid),
                Cap("B-1", RackEmbedDocument.KindSelective, json, refs: 1, id: OtherGuid),
                Cap("B-2", RackEmbedDocument.KindSelective, json, refs: 2, id: OtherGuid),
            });

            AssertAvailable(population.TotalRacks, 2);
            Assert.Equal(2, population.Racks.Count);
        }

        // ---------------------------------------------------------------- INV-02

        [Fact]
        public void INV02_RackAYrackA_SonUnRack_ConLaGrafiaMinimaOrdinal_AunConElOrdenInvertido()
        {
            var json = SelectiveJson();
            var lowerFirst = new[]
            {
                Cap("DEF-1", RackEmbedDocument.KindSelective, json, id: "rack-a"),
                Cap("DEF-2", RackEmbedDocument.KindSelective, json, id: "Rack-A"),
            };

            foreach (var input in new[] { lowerFirst, lowerFirst.Reverse().ToArray() })
            {
                var population = Evaluate(input);

                AssertAvailable(population.TotalRacks, 1);
                Assert.Single(population.Racks);
                Assert.Equal("Rack-A", population.Racks[0].RackId);
            }
        }

        [Fact]
        public void INV02_UnSobreIlegibleConTanteoRackA_NoFusiona_ElTotalQuedaUnavailable()
        {
            // SchemaVersion de un major futuro: el sobre no deserializa, pero el tanteo ve el Id de primer nivel.
            const string unreadable = "{\"SchemaVersion\":\"9.0\",\"Kind\":\"selective\",\"Id\":\"Rack-A\"}";
            Assert.Null(new RackEmbedStore().Deserialize(unreadable));
            Assert.Equal("Rack-A", RackEnvelopeIdProbe.Probe(unreadable));

            var population = Evaluate(new[]
            {
                Cap("DEF-1", RackEmbedDocument.KindSelective, SelectiveJson(), id: "Rack-A"),
                Raw("DEF-2", unreadable),
            });

            AssertUnavailable(population.TotalRacks, UnavailableReasonKind.CoverageNotAccredited);
            Assert.Single(population.Racks);
            Assert.Equal("Rack-A", population.Racks[0].RackId);
            Assert.Equal(new[] { "DEF-1" }, population.Racks[0].DefinitionKeys);
            Assert.Contains(population.Diagnostics, item =>
                item.Code == PopulationDiagnosticCode.PlacedDefinitionWithoutIdentity && item.DefinitionKey == "DEF-2");
        }

        // ---------------------------------------------------------------- INV-03: E1 y D-10a

        [Theory]
        [InlineData("unreadable", 1, false)]
        [InlineData("idabsent", 1, false)]
        [InlineData("unreadable", 0, true)]
        [InlineData("idabsent", 0, true)]
        public void INV03_E1_UnaDefinicionColocadaSinIdentidadDejaLaCoberturaSinAcreditar_SinColocarSeIgnora(
            string shape, int references, bool accredited)
        {
            var orphan = shape == "unreadable"
                ? Raw("DEF-X", "{ esto no es un sobre", references)
                : Raw("DEF-X", Envelope(RackEmbedDocument.KindSelective, SelectiveJson(), id: "   "), references);

            var population = Evaluate(new[]
            {
                Cap("DEF-1", RackEmbedDocument.KindSelective, SelectiveJson()),
                orphan,
            });

            Assert.Equal(accredited, population.CoverageAccredited);
            Assert.Single(population.Racks);
            Assert.Equal(RackMembership.Included, population.Racks[0].Membership);

            if (accredited)
            {
                AssertAvailable(population.TotalRacks, 1);
                Assert.Empty(population.Diagnostics);
            }
            else
            {
                AssertUnavailable(population.TotalRacks, UnavailableReasonKind.CoverageNotAccredited);
                var diagnostic = Assert.Single(population.Diagnostics);
                Assert.Equal(PopulationDiagnosticCode.PlacedDefinitionWithoutIdentity, diagnostic.Code);
                Assert.Null(diagnostic.RackId);
                Assert.Equal("DEF-X", diagnostic.DefinitionKey);
            }
        }

        // ---------------------------------------------------------------- INV-03: E2, E3, orden y O-PV2-4

        [Fact]
        public void INV03_E2_UnRackSinColocar_EsExcluidoNotPlaced_SinDiagnostico()
        {
            var json = SelectiveJson();
            var population = Evaluate(new[]
            {
                Cap("DEF-A", RackEmbedDocument.KindSelective, json, refs: 0),
                Cap("DEF-B", RackEmbedDocument.KindSelective, json, refs: 0),
            });

            AssertAvailable(population.TotalRacks, 0);
            Assert.Equal(RackMembership.Excluded(RackExclusionReason.NotPlaced), MembershipOf(population, RackGuid));
            Assert.Empty(population.Diagnostics);
            Assert.True(population.CoverageAccredited);
        }

        [Theory]
        [InlineData("todas-KindAbsent", "Excluded(KindAbsent)")]
        [InlineData("todas-KindUnknown-mismo-token", "Excluded(KindUnknown)")]
        [InlineData("KindUnknown-tokens-distintos", "Undetermined(KindIncoherent)")]
        [InlineData("Known-y-KindUnknown", "Undetermined(KindIncoherent)")]
        [InlineData("Known-distintos", "Undetermined(KindIncoherent)")]
        [InlineData("KindAbsent-y-Known", "Undetermined(KindIncoherent)")]
        [InlineData("comparador-Ordinal-Selective", "Excluded(KindUnknown)")]
        public void INV03_E3_KindCoherente_CadaMezclaDaSuClasificacion(string scenario, string expected)
        {
            var json = SelectiveJson();
            var captures = new List<RackDefinitionCapture>();
            switch (scenario)
            {
                case "todas-KindAbsent":
                    captures.Add(Cap("DEF-A", string.Empty, json));
                    captures.Add(Cap("DEF-B", "   ", json));
                    break;
                case "todas-KindUnknown-mismo-token":
                    captures.Add(Cap("DEF-A", "otro-kind", json));
                    captures.Add(Cap("DEF-B", "otro-kind", json));
                    break;
                case "KindUnknown-tokens-distintos":
                    captures.Add(Cap("DEF-A", "otro-kind", json));
                    captures.Add(Cap("DEF-B", "tercer-kind", json));
                    break;
                case "Known-y-KindUnknown":
                    captures.Add(Cap("DEF-A", RackEmbedDocument.KindSelective, json));
                    captures.Add(Cap("DEF-B", "otro-kind", json));
                    break;
                case "Known-distintos":
                    captures.Add(Cap("DEF-A", RackEmbedDocument.KindSelective, json));
                    captures.Add(Cap("DEF-B", RackEmbedDocument.KindPushBack, PushBackJson()));
                    break;
                case "KindAbsent-y-Known":
                    captures.Add(Cap("DEF-A", string.Empty, json));
                    captures.Add(Cap("DEF-B", RackEmbedDocument.KindSelective, json));
                    break;
                default:
                    captures.Add(Cap("DEF-A", "Selective", json));
                    break;
            }

            var population = Evaluate(captures);

            var rack = Rack(population, RackGuid);
            Assert.NotNull(rack);
            Assert.Equal(expected, rack.Membership.ToString());

            if (rack.Membership.Kind == RackMembershipKind.Undetermined)
            {
                AssertUnavailable(population.TotalRacks, UnavailableReasonKind.CoverageNotAccredited);
            }
            else
            {
                AssertAvailable(population.TotalRacks, 0);
            }

            Assert.Contains(population.Diagnostics, item => item.RackId == RackGuid);
        }

        [Fact]
        public void INV03_O_PV2_4_UnaHermanaSinColocarConKindAusenteSiPerteneceAlGrupo_PeroUnaIlegibleSeIgnora()
        {
            var json = SelectiveJson();

            // Con identidad (KindAbsent con Id): es parte del rack y lo deja Undetermined(KindIncoherent).
            var withIdentity = Evaluate(new[]
            {
                Cap("DEF-A", RackEmbedDocument.KindSelective, json, refs: 1),
                Cap("DEF-B", string.Empty, json, refs: 0),
            });

            Assert.Equal(
                RackMembership.Undetermined(RackUndeterminedReason.KindIncoherent),
                MembershipOf(withIdentity, RackGuid));
            AssertUnavailable(withIdentity.TotalRacks, UnavailableReasonKind.CoverageNotAccredited);

            // Sin identidad (sobre ilegible, sin colocar): no tiene grupo al que pertenecer y se ignora.
            var withoutIdentity = Evaluate(new[]
            {
                Cap("DEF-A", RackEmbedDocument.KindSelective, json, refs: 1),
                Raw("DEF-B", "{ esto no es un sobre", refs: 0),
            });

            Assert.Equal(RackMembership.Included, MembershipOf(withoutIdentity, RackGuid));
            AssertAvailable(withoutIdentity.TotalRacks, 1);
        }

        [Fact]
        public void INV03_ElOrdenDeD10_LaPrimeraCondicionQueDecideGana()
        {
            var json = SelectiveJson();

            // E2 antes que E3: todas sin colocar y mezcladas -> NotPlaced, no KindIncoherent.
            var notPlaced = Evaluate(new[]
            {
                Cap("DEF-A", RackEmbedDocument.KindSelective, json, refs: 0),
                Cap("DEF-B", RackEmbedDocument.KindPushBack, PushBackJson(), refs: 0),
            });
            Assert.Equal(RackMembership.Excluded(RackExclusionReason.NotPlaced), MembershipOf(notPlaced, RackGuid));

            // E3 antes que E5: kinds mezclados con disenos ilegibles -> KindIncoherent, no DesignUnreadable.
            var incoherent = Evaluate(new[]
            {
                Cap("DEF-A", RackEmbedDocument.KindSelective, Garbage),
                Cap("DEF-B", RackEmbedDocument.KindPushBack, Garbage),
            });
            Assert.Equal(
                RackMembership.Undetermined(RackUndeterminedReason.KindIncoherent), MembershipOf(incoherent, RackGuid));

            // E5 antes que E4: una hermana ilegible y la otra divergente -> DesignUnreadable, no SiblingsDivergent.
            var unreadable = Evaluate(new[]
            {
                Cap("DEF-A", RackEmbedDocument.KindSelective, SelectiveJson(clearance: 8.0)),
                Cap("DEF-B", RackEmbedDocument.KindSelective, json),
                Cap("DEF-C", RackEmbedDocument.KindSelective, Garbage),
            });
            Assert.Equal(
                RackMembership.Undetermined(RackUndeterminedReason.DesignUnreadable), MembershipOf(unreadable, RackGuid));

            // E6 se alcanza solo tras E1..E5: un Push Back sano con LoadFailed llega a la puerta y queda Undetermined.
            var failed = Evaluate(
                new[] { Cap("DEF-A", RackEmbedDocument.KindPushBack, PushBackJson()) }, RackCatalogInput.LoadFailed());
            Assert.Equal(
                RackMembership.Undetermined(RackUndeterminedReason.CatalogUnavailable), MembershipOf(failed, RackGuid));
        }

        // ---------------------------------------------------------------- INV-03: E4 y E6

        [Fact]
        public void INV03_E4_HermanasDivergentes_SonExcluidasSiblingsDivergent_ConDiagnostico()
        {
            var population = Evaluate(new[]
            {
                Cap("DEF-A", RackEmbedDocument.KindSelective, SelectiveJson(clearance: 6.0)),
                Cap("DEF-B", RackEmbedDocument.KindSelective, SelectiveJson(clearance: 8.0)),
            });

            Assert.Equal(
                RackMembership.Excluded(RackExclusionReason.SiblingsDivergent), MembershipOf(population, RackGuid));
            AssertAvailable(population.TotalRacks, 0);
            var diagnostic = Assert.Single(population.Diagnostics);
            Assert.Equal(PopulationDiagnosticCode.RackExcluded, diagnostic.Code);
            Assert.Equal(RackGuid, diagnostic.RackId);
            Assert.Equal("SiblingsDivergent", diagnostic.Reason);
        }

        [Fact]
        public void INV03_E4_UnaHermanaIlegibleParaLaAutoridad_DaUndeterminedDesignUnreadable()
        {
            // El lector de E5 se sustituye por uno que aprueba todo: asi E4 ve el diseno ilegible y su
            // UnreadableSibling se mapea a Undetermined(DesignUnreadable).
            var population = Evaluate(
                new[] { Cap("DEF-A", RackEmbedDocument.KindSelective, Garbage) }, reader: new AlwaysReadable());

            Assert.Equal(
                RackMembership.Undetermined(RackUndeterminedReason.DesignUnreadable), MembershipOf(population, RackGuid));
            AssertUnavailable(population.TotalRacks, UnavailableReasonKind.CoverageNotAccredited);
        }

        [Theory]
        [InlineData("deny-catalogo-vacio", "Excluded(OutputDenied)")]
        [InlineData("deny-catalogo-real", "Excluded(OutputDenied)")]
        [InlineData("allow", "Included")]
        [InlineData("loadfailed", "Undetermined(CatalogUnavailable)")]
        [InlineData("resolvefailed", "Undetermined(ResolveFailed)")]
        public void INV03_E6_ElVeredictoDeSalida_DeterminaLaPertenenciaDePushBack(string scenario, string expected)
        {
            RackCatalogInput catalog;
            string design;
            switch (scenario)
            {
                case "deny-catalogo-vacio":
                    catalog = EmptyCatalog();
                    design = PushBackJson(blocking: true);
                    break;
                case "deny-catalogo-real":
                    catalog = RackCatalogInput.Loaded(JsonRackCatalogProvider.FromBaseDirectory().Load());
                    design = PushBackJson(blocking: true);
                    break;
                case "loadfailed":
                    catalog = RackCatalogInput.LoadFailed();
                    design = PushBackJson();
                    break;
                case "resolvefailed":
                    catalog = RackCatalogInput.Loaded(ThrowingCatalog());
                    design = PushBackJson();
                    break;
                default:
                    catalog = EmptyCatalog();
                    design = PushBackJson();
                    break;
            }

            var population = Evaluate(new[] { Cap("DEF-A", RackEmbedDocument.KindPushBack, design) }, catalog);

            var rack = Rack(population, RackGuid);
            Assert.NotNull(rack);
            Assert.Equal(expected, rack.Membership.ToString());
            Assert.Equal(RackEmbedDocument.KindPushBack, rack.KindToken);

            if (rack.Membership.Kind == RackMembershipKind.Undetermined)
            {
                AssertUnavailable(population.TotalRacks, UnavailableReasonKind.CoverageNotAccredited);
            }
            else
            {
                AssertAvailable(population.TotalRacks, rack.Membership.Kind == RackMembershipKind.Included ? 1 : 0);
            }
        }

        [Fact]
        public void INV03_E6_LosKindsDistintosDePushBack_NoLeenDisenoNiCatalogo_SeIncluyen()
        {
            // LoadFailed solo afecta a Push Back: el Selectivo no consulta la puerta (Allow sin leer).
            var population = Evaluate(
                new[] { Cap("DEF-A", RackEmbedDocument.KindSelective, SelectiveJson()) }, RackCatalogInput.LoadFailed());

            Assert.Equal(RackMembership.Included, MembershipOf(population, RackGuid));
            AssertAvailable(population.TotalRacks, 1);
        }

        // ---------------------------------------------------------------- INV-06

        [Theory]
        [InlineData(RackEmbedDocument.KindSelective)]
        [InlineData(RackEmbedDocument.KindDynamic)]
        [InlineData(RackEmbedDocument.KindPushBack)]
        [InlineData(RackEmbedDocument.KindCantilever)]
        [InlineData(RackEmbedDocument.KindCabecera)]
        [InlineData(RackEmbedDocument.KindCama)]
        public void INV06_UnDisenoIlegible_DeCadaKind_EsUndeterminedDesignUnreadable_YElTotalNoSeAcredita(string kind)
        {
            var population = Evaluate(new[]
            {
                Cap("DEF-OK", RackEmbedDocument.KindSelective, SelectiveJson(), id: OtherGuid),
                Cap("DEF-A", kind, Garbage),
            });

            var rack = Rack(population, RackGuid);
            Assert.NotNull(rack);
            Assert.Equal(RackMembership.Undetermined(RackUndeterminedReason.DesignUnreadable), rack.Membership);
            AssertUnavailable(population.TotalRacks, UnavailableReasonKind.CoverageNotAccredited);
            Assert.Contains(population.Diagnostics, item =>
                item.Code == PopulationDiagnosticCode.RackUndetermined && item.RackId == RackGuid);
        }

        [Theory]
        [InlineData(RackEmbedDocument.KindDynamic)]
        [InlineData(RackEmbedDocument.KindPushBack)]
        [InlineData(RackEmbedDocument.KindCantilever)]
        [InlineData(RackEmbedDocument.KindCabecera)]
        public void INV06_UnDisenoSinLaSeccionDeSuKind_EsIlegible_NuncaSeExcluyeNiSeCuentaEnSilencio(string kind)
        {
            // Un proyecto legible, pero de OTRO kind: su store lo lee y no encuentra la seccion de este.
            var foreign = kind == RackEmbedDocument.KindCabecera ? PushBackJson() : HeaderJson();

            var population = Evaluate(new[] { Cap("DEF-A", kind, foreign) });

            Assert.Equal(
                RackMembership.Undetermined(RackUndeterminedReason.DesignUnreadable), MembershipOf(population, RackGuid));
            AssertUnavailable(population.TotalRacks, UnavailableReasonKind.CoverageNotAccredited);
        }

        // ---------------------------------------------------------------- INV-07

        [Fact]
        public void INV07_BySystem_SeisTokensEnOrdenFijo_SistemasAusentesRackCountCero_Y_SumaIgualATotalRacks()
        {
            var json = SelectiveJson();
            var captures = new[]
            {
                Cap("A-1", RackEmbedDocument.KindSelective, json, id: RackGuid),
                Cap("B-1", RackEmbedDocument.KindSelective, json, id: OtherGuid),
                Cap("C-1", RackEmbedDocument.KindPushBack, PushBackJson(), id: "pb-1"),
            };

            var population = Evaluate(captures);
            var aggregates = ProjectPopulationAggregator.Aggregate(population, MetricsOf(captures));

            Assert.Equal(SixTokens, aggregates.BySystem.Select(item => item.KindToken));
            Assert.Equal(SixTokens, population.RackCountBySystem.Select(item => item.KindToken));

            AssertAvailable(aggregates.TotalRacks, 3);
            AssertAvailable(aggregates.For(RackEmbedDocument.KindSelective).RackCount, 2);
            AssertAvailable(aggregates.For(RackEmbedDocument.KindPushBack).RackCount, 1);
            foreach (var absent in new[]
                     {
                         RackEmbedDocument.KindDynamic, RackEmbedDocument.KindCantilever,
                         RackEmbedDocument.KindCabecera, RackEmbedDocument.KindCama,
                     })
            {
                AssertAvailable(aggregates.For(absent).RackCount, 0);
            }

            Assert.Equal(MetricStatus.NotSupported, aggregates.For(RackEmbedDocument.KindPushBack).TotalFrentes.Status);
            Assert.Equal(MetricStatus.NotSupported, aggregates.For(RackEmbedDocument.KindPushBack).TotalFrentesVacios.Status);
            Assert.Equal(MetricStatus.NotSupported, aggregates.For(RackEmbedDocument.KindDynamic).TotalFrentes.Status);
            Assert.Equal(MetricStatus.NotSupported, aggregates.For(RackEmbedDocument.KindCantilever).TotalFrentes.Status);
            Assert.Equal(MetricStatus.NotApplicable, aggregates.For(RackEmbedDocument.KindCabecera).TotalFrentes.Status);
            Assert.Equal(MetricStatus.NotApplicable, aggregates.For(RackEmbedDocument.KindCama).TotalFrentesVacios.Status);

            Assert.Equal(
                aggregates.TotalRacks.Value, aggregates.BySystem.Sum(item => item.RackCount.Value));

            // El Selectivo SI suma: 2 racks x 4 frentes (sin vacios).
            AssertAvailable(aggregates.For(RackEmbedDocument.KindSelective).TotalFrentes, 8);
            AssertAvailable(aggregates.For(RackEmbedDocument.KindSelective).TotalFrentesVacios, 0);
        }

        [Fact]
        public void INV07_ConLosSeisKindsPresentes_CadaRackCountEsUno_Y_LaSumaEsSeis()
        {
            var captures = SixTokens
                .Select((kind, index) => Cap("DEF-" + index, kind, ValidJson(kind), id: "rack-" + index))
                .ToList();

            var population = Evaluate(captures);
            var aggregates = ProjectPopulationAggregator.Aggregate(population, MetricsOf(captures));

            AssertAvailable(aggregates.TotalRacks, 6);
            foreach (var token in SixTokens)
            {
                AssertAvailable(aggregates.For(token).RackCount, 1);
            }

            Assert.Equal(6.0, aggregates.BySystem.Sum(item => item.RackCount.Value));
        }

        [Fact]
        public void INV07_ConLaCoberturaSinAcreditar_LosSeisTokensSiguenPresentes_Y_RackCountEsUnavailable()
        {
            var population = Evaluate(new[] { Cap("DEF-A", RackEmbedDocument.KindCama, Garbage) });
            var aggregates = ProjectPopulationAggregator.Aggregate(population, null);

            Assert.Equal(SixTokens, aggregates.BySystem.Select(item => item.KindToken));
            AssertUnavailable(aggregates.TotalRacks, UnavailableReasonKind.CoverageNotAccredited);
            foreach (var item in aggregates.BySystem)
            {
                AssertUnavailable(item.RackCount, UnavailableReasonKind.CoverageNotAccredited);
            }

            AssertUnavailable(aggregates.For(RackEmbedDocument.KindSelective).TotalFrentes, UnavailableReasonKind.CoverageNotAccredited);
        }

        // ---------------------------------------------------------------- INV-08 e INV-10

        [Fact]
        public void INV08_UnIncluidoConFrentesUnavailable_HaceUnavailableElAgregado_SinParcial_YTotalRacksSigueAvailable()
        {
            var good = SelectiveJson();
            var captures = new[]
            {
                Cap("A-1", RackEmbedDocument.KindSelective, good, id: RackGuid),
                Cap("B-1", RackEmbedDocument.KindSelective, BrokenReferenceJson(), id: OtherGuid),
            };

            var population = Evaluate(captures);
            var metrics = MetricsOf(captures);

            // Control: el rack sano SI tiene frentes Available, asi que un agregado parcial daria un numero.
            AssertAvailable(metrics[RackGuid][RackMetricIds.Frentes], 4);

            var aggregates = ProjectPopulationAggregator.Aggregate(population, metrics);

            AssertAvailable(aggregates.TotalRacks, 2);
            var selective = aggregates.For(RackEmbedDocument.KindSelective);
            AssertAvailable(selective.RackCount, 2);
            AssertUnavailable(selective.TotalFrentes, UnavailableReasonKind.MemberMetricUnavailable);
            AssertUnavailable(selective.TotalFrentesVacios, UnavailableReasonKind.MemberMetricUnavailable);
            Assert.Equal(new[] { OtherGuid }, selective.TotalFrentes.Reason.MemberRackIds);
        }

        [Fact]
        public void INV08_UnRackSinMetricaEnLaEntrada_NuncaSeSaltaEnSilencio()
        {
            var captures = new[] { Cap("A-1", RackEmbedDocument.KindSelective, SelectiveJson()) };
            var population = Evaluate(captures);

            var aggregates = ProjectPopulationAggregator.Aggregate(population, new Dictionary<string, RackMetricResults>());

            AssertAvailable(aggregates.TotalRacks, 1);
            AssertUnavailable(aggregates.For(RackEmbedDocument.KindSelective).TotalFrentes, UnavailableReasonKind.MemberMetricUnavailable);
        }

        [Fact]
        public void INV10_UnaVariableRota_NoCambiaTotalRacks_ElSelectivoSigueIncluido_ConFrentesUnavailableEffectiveFailed()
        {
            var captures = new[] { Cap("A-1", RackEmbedDocument.KindSelective, BrokenReferenceJson()) };

            var population = Evaluate(captures);
            var metrics = MetricsOf(captures);
            var aggregates = ProjectPopulationAggregator.Aggregate(population, metrics);

            Assert.Equal(RackMembership.Included, MembershipOf(population, RackGuid));
            AssertAvailable(population.TotalRacks, 1);
            AssertAvailable(aggregates.TotalRacks, 1);

            var frentes = metrics[RackGuid][RackMetricIds.Frentes];
            Assert.Equal(MetricStatus.Unavailable, frentes.Status);
            Assert.Equal(
                UnavailableReason.EffectiveFailed(SelectiveEffectiveOutcome.BrokenProjectVariableReference),
                frentes.Reason);
            AssertUnavailable(aggregates.For(RackEmbedDocument.KindSelective).TotalFrentes, UnavailableReasonKind.MemberMetricUnavailable);
        }

        // ---------------------------------------------------------------- INV-11

        [Fact]
        public void INV11_HermanaNoColocadaYDivergente_ExcluyeSiblingsDivergent_YEsDeterministaAnteElOrdenDeEntrada()
        {
            var json = SelectiveJson(clearance: 6.0);
            var divergent = SelectiveJson(clearance: 9.0);

            var captures = new List<RackDefinitionCapture>
            {
                // Rack del GUID principal: la hermana divergente NO esta colocada.
                Cap("A-1", RackEmbedDocument.KindSelective, divergent, refs: 0, id: "rack-a", name: ""),
                Cap("A-2", RackEmbedDocument.KindSelective, json, refs: 1, id: "Rack-A", name: "  Beta  "),
                // Un rack sano con tres vistas, nombres en blanco y grafias distintas.
                Cap("C-1", RackEmbedDocument.KindSelective, json, refs: 1, id: "rack-c", name: " "),
                Cap("C-2", RackEmbedDocument.KindSelective, json, refs: 0, id: "Rack-C", name: "  Gamma "),
                Cap("C-3", RackEmbedDocument.KindSelective, json, refs: 2, id: "RACK-C", name: "Alfa"),
            };

            Assert.NotEqual(json, divergent);

            ProjectPopulation Forward(out ProjectPopulationAggregates aggregates, IReadOnlyList<RackDefinitionCapture> order)
            {
                var population = Evaluate(order);
                aggregates = ProjectPopulationAggregator.Aggregate(population, MetricsOf(order));
                return population;
            }

            var first = Forward(out var firstAggregates, captures);

            var rackA = Rack(first, "Rack-A");
            Assert.NotNull(rackA);
            Assert.Equal(RackMembership.Excluded(RackExclusionReason.SiblingsDivergent), rackA.Membership);
            Assert.Equal("Beta", rackA.DisplayName);

            var rackC = Rack(first, "RACK-C");
            Assert.NotNull(rackC);
            Assert.Equal(RackMembership.Included, rackC.Membership);
            Assert.Equal("C-1", rackC.RepresentativeDefinitionId);
            Assert.Equal("Gamma", rackC.DisplayName);

            var reference = Snapshot(first, firstAggregates);
            var reversed = captures.AsEnumerable().Reverse().ToList();
            var rotated = captures.Skip(2).Concat(captures.Take(2)).ToList();

            foreach (var order in new[] { reversed, rotated })
            {
                var other = Forward(out var otherAggregates, order);

                Assert.Equal(reference, Snapshot(other, otherAggregates));
                Assert.Equal(first.Racks.Select(rack => rack.Membership), other.Racks.Select(rack => rack.Membership));
                Assert.Equal(
                    first.Racks.Select(rack => rack.RepresentativeDefinitionId),
                    other.Racks.Select(rack => rack.RepresentativeDefinitionId));
                Assert.Equal(first.Racks.Select(rack => rack.DisplayName), other.Racks.Select(rack => rack.DisplayName));
                Assert.Equal(first.Racks.Select(rack => rack.RackId), other.Racks.Select(rack => rack.RackId));
            }
        }

        // ---------------------------------------------------------------- INV-12

        [Fact]
        public void INV12_Allow_PushBackSinMotivo_YLosDemasKindsSinLeerNada()
        {
            Assert.Equal(
                RackOutputVerdictKind.Allow,
                RackOutputVerdict.Evaluate(RackEmbedDocument.KindPushBack, PushBackJson(), EmptyCatalog()).Kind);

            // Los demas kinds (y un token cualquiera) son Allow SIN leer: ni con un diseno basura ni con LoadFailed.
            foreach (var token in SixTokens.Where(item => item != RackEmbedDocument.KindPushBack).Concat(new[] { "otro-kind", null }))
            {
                Assert.Equal(
                    RackOutputVerdictKind.Allow,
                    RackOutputVerdict.Evaluate(token, Garbage, RackCatalogInput.LoadFailed()).Kind);
            }
        }

        [Fact]
        public void INV12_Deny_PushBackConMotivoDeRackBomOutputGate_AunConCatalogoVacioCargado()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var system = new PushBackResolver(catalog).Resolve(PushBackDesignFor(blocking: true));
            var expected = RackBomOutputGate.For(system).Reason;
            Assert.False(string.IsNullOrWhiteSpace(expected));

            var denied = RackOutputVerdict.Evaluate(
                RackEmbedDocument.KindPushBack, PushBackJson(blocking: true), RackCatalogInput.Loaded(catalog));
            Assert.Equal(RackOutputVerdictKind.Deny, denied.Kind);
            Assert.Equal(expected, denied.DenyReason);

            // Loaded(catalogo vacio) SE EVALUA (no es LoadFailed) y puede dar Deny.
            var withEmpty = RackOutputVerdict.Evaluate(RackEmbedDocument.KindPushBack, PushBackJson(blocking: true), EmptyCatalog());
            Assert.Equal(RackOutputVerdictKind.Deny, withEmpty.Kind);
            Assert.False(string.IsNullOrWhiteSpace(withEmpty.DenyReason));
        }

        [Fact]
        public void INV12_Undetermined_DisenoIlegible_LoadFailed_Y_ExcepcionDelResolver()
        {
            // Diseno ilegible: texto basura, o un proyecto legible sin la seccion de Push Back.
            foreach (var design in new[] { Garbage, HeaderJson(), null })
            {
                var decision = RackOutputVerdict.Evaluate(RackEmbedDocument.KindPushBack, design, EmptyCatalog());
                Assert.Equal(RackOutputVerdictKind.Undetermined, decision.Kind);
                Assert.Equal(RackOutputUndeterminedReason.DesignUnreadable, decision.UndeterminedReason);
            }

            // LoadFailed NO es un catalogo vacio valido.
            var loadFailed = RackOutputVerdict.Evaluate(RackEmbedDocument.KindPushBack, PushBackJson(), RackCatalogInput.LoadFailed());
            Assert.Equal(RackOutputVerdictKind.Undetermined, loadFailed.Kind);
            Assert.Equal(RackOutputUndeterminedReason.CatalogUnavailable, loadFailed.UndeterminedReason);

            // El diseno ilegible manda sobre el catalogo no disponible.
            var both = RackOutputVerdict.Evaluate(RackEmbedDocument.KindPushBack, Garbage, RackCatalogInput.LoadFailed());
            Assert.Equal(RackOutputUndeterminedReason.DesignUnreadable, both.UndeterminedReason);

            // Una excepcion del resolver.
            var resolveFailed = RackOutputVerdict.Evaluate(
                RackEmbedDocument.KindPushBack, PushBackJson(), RackCatalogInput.Loaded(ThrowingCatalog()));
            Assert.Equal(RackOutputVerdictKind.Undetermined, resolveFailed.Kind);
            Assert.Equal(RackOutputUndeterminedReason.ResolveFailed, resolveFailed.UndeterminedReason);
        }

        // ---------------------------------------------------------------- INV-13

        [Fact]
        public void INV13_ElHandler_AllowNull_DenyMotivo_UndeterminedNull()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var expected = RackBomOutputGate.For(
                new PushBackResolver(catalog).Resolve(PushBackDesignFor(blocking: true))).Reason;

            Assert.Null(RackOutputVerdict.HandlerBlockedReason(RackEmbedDocument.KindPushBack, PushBackJson(), catalog));
            Assert.Equal(
                expected,
                RackOutputVerdict.HandlerBlockedReason(RackEmbedDocument.KindPushBack, PushBackJson(blocking: true), catalog));

            // Undetermined -> null (fail-open): diseno ilegible, excepcion del resolver.
            Assert.Null(RackOutputVerdict.HandlerBlockedReason(RackEmbedDocument.KindPushBack, Garbage, catalog));
            Assert.Null(RackOutputVerdict.HandlerBlockedReason(RackEmbedDocument.KindPushBack, null, catalog));
            Assert.Null(RackOutputVerdict.HandlerBlockedReason(RackEmbedDocument.KindPushBack, PushBackJson(), ThrowingCatalog()));
        }

        [Fact]
        public void INV13_CatalogoNulo_SeNormalizaAVacioYSeEvaluaComoHoy_NoEsCatalogUnavailable()
        {
            // El oraculo es el comportamiento VIGENTE: PushBackResolver hacia `catalog ?? new RackCatalog()`.
            var legacy = RackBomOutputGate.For(
                new PushBackResolver(null).Resolve(PushBackDesignFor(blocking: true))).Reason;
            Assert.False(string.IsNullOrWhiteSpace(legacy));

            var viaHandler = RackOutputVerdict.HandlerBlockedReason(
                RackEmbedDocument.KindPushBack, PushBackJson(blocking: true), null);

            // Un Push Back que con catalogo vacio da motivo SIGUE dando ese motivo (tratar null como LoadFailed lo dejaria en null).
            Assert.Equal(legacy, viaHandler);
            Assert.Null(RackOutputVerdict.HandlerBlockedReason(RackEmbedDocument.KindPushBack, PushBackJson(), null));

            // Los demas kinds por la via del handler: null, sin leer.
            Assert.Null(RackOutputVerdict.HandlerBlockedReason(RackEmbedDocument.KindSelective, Garbage, null));
        }

        [Fact]
        public void INV13_LaViaDeI63ConLoadFailed_EsUndeterminedCatalogUnavailable_Y_ElHandlerNuncaProduceLoadFailed()
        {
            var viaI63 = RackOutputVerdict.Evaluate(
                RackEmbedDocument.KindPushBack, PushBackJson(blocking: true), RackCatalogInput.LoadFailed());
            Assert.Equal(RackOutputVerdictKind.Undetermined, viaI63.Kind);
            Assert.Equal(RackOutputUndeterminedReason.CatalogUnavailable, viaI63.UndeterminedReason);

            // Mismo diseno bloqueante por la via del handler: con catalogo nulo se evalua y bloquea.
            Assert.NotNull(RackOutputVerdict.HandlerBlockedReason(RackEmbedDocument.KindPushBack, PushBackJson(blocking: true), null));
        }

        // ---------------------------------------------------------------- INV-32 y D-20

        [Fact]
        public void INV32_UnRackMetricRequestDirecto_DejaElContadorEnCero_ProjectPopulationDelMismoRackLoIncrementa()
        {
            var captures = new[]
            {
                Cap("DEF-A", RackEmbedDocument.KindSelective, SelectiveJson(), refs: 1),
                Cap("DEF-B", RackEmbedDocument.KindSelective, SelectiveJson(), refs: 0),
            };

            using (var counter = RackPopulationEvaluationCounter.Begin())
            {
                var direct = new RackMetricRequest(
                    captures, RackCad.Application.Persistence.ProjectVariablesReadResult.Absent(), EmptyCatalog()).Execute();
                var afterDirect = counter.Count;

                // Control positivo con el MISMO contador: el mismo rack, por la poblacion.
                var population = ProjectPopulation.Evaluate(Input(captures));
                var afterPopulation = counter.Count;

                _output.WriteLine(
                    "INV-32 contador tras RackMetricRequest directo=" + afterDirect
                    + " | tras ProjectPopulation del mismo rack=" + afterPopulation);

                Assert.Equal(MetricStatus.Available, direct[RackMetricIds.Frentes].Status);
                Assert.Equal(RackMembership.Included, MembershipOf(population, RackGuid));
                Assert.Equal(0, afterDirect);
                Assert.True(afterPopulation > 0, "la poblacion debe incrementar el contador unico");
            }
        }

        [Fact]
        public void D20_ProjectPopulation_NoTieneCamposDeMetricasPorRack_SoloPertenenciaYConteos()
        {
            foreach (var type in new[] { typeof(ProjectPopulation), typeof(PopulationRack) })
            {
                Assert.DoesNotContain(
                    type.GetProperties(),
                    property => property.PropertyType == typeof(RackMetricResults)
                                || typeof(System.Collections.IDictionary).IsAssignableFrom(property.PropertyType));
            }

            var metricProperties = typeof(ProjectPopulation).GetProperties()
                .Where(property => property.PropertyType == typeof(MetricValue))
                .Select(property => property.Name)
                .ToList();
            Assert.Equal(new[] { "TotalRacks" }, metricProperties);
        }
    }
}
