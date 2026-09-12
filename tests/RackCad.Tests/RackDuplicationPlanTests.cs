using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-51 (ID15) G3 — el planificador PURO de RACKDUPLICAR con multiples origenes: clasificar → agrupar →
    /// deduplicar → asignar identidad y nombre. Son las pruebas contractuales T1-T13 del contrato de I-51.
    ///
    /// <para>
    /// Lo que se prueba aqui es exactamente lo que el Plugin no puede probar (ADR-0003): que las vistas de un
    /// mismo rack comparten UN RackId nuevo por destino, que las referencias enlazadas siguen enlazadas, que
    /// no se infieren hermanas, que un payload RackCad ilegible nunca desaparece en silencio y que el nombre
    /// sigue la politica historica por ordinal de destino.
    /// </para>
    /// </summary>
    public class RackDuplicationPlanTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string RackB = "7c1d2e3f-4a5b-4c6d-8e9f-0a1b2c3d4e5f";

        private static readonly string[] RegisteredKinds =
        {
            RackEmbedDocument.KindSelective,
            RackEmbedDocument.KindDynamic,
            RackEmbedDocument.KindPushBack,
            RackEmbedDocument.KindCantilever,
            RackEmbedDocument.KindCabecera,
            RackEmbedDocument.KindCama,
        };

        /// <summary>Mirrors the Plugin's predicate: a registered handler, looked up case-insensitively.</summary>
        private static bool KnownKind(string kind) => RegisteredKinds.Contains(kind, StringComparer.OrdinalIgnoreCase);

        private static string Envelope(
            string id,
            string kind = RackEmbedDocument.KindDynamic,
            string name = "Rack A",
            string view = RackEmbedDocument.ViewFrontal,
            int section = -1,
            string design = "{\"diseno\":1}")
            => new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Kind = kind,
                Id = id,
                Name = name,
                View = view,
                Section = section,
                Design = design,
            });

        private static RackDuplicationSelectedReference Ref(string referenceKey, string definitionKey, bool inModelSpace = true)
            => new RackDuplicationSelectedReference(referenceKey, isBlockReference: true, isInModelSpace: inModelSpace, definitionKey);

        private static RackDuplicationSelectedReference NotABlock(string referenceKey)
            => new RackDuplicationSelectedReference(referenceKey, isBlockReference: false, isInModelSpace: true, definitionKey: null);

        private static RackDuplicationDefinitionSnapshot Def(string key, string payload, string name = null)
            => new RackDuplicationDefinitionSnapshot(key, payload, name ?? "BLOQUE_" + key);

        private static RackDuplicationPlan Build(
            IEnumerable<RackDuplicationSelectedReference> selection,
            IEnumerable<RackDuplicationDefinitionSnapshot> definitions)
            => RackDuplicationPlan.Build(selection.ToList(), definitions.ToList(), KnownKind);

        /// <summary>A deterministic generator that also counts how many ids it handed out.</summary>
        private sealed class IdSequence
        {
            private int next;

            public int Calls { get; private set; }

            public Guid Next()
            {
                Calls++;
                next++;
                return new Guid(next, 0, 0, new byte[8]);
            }
        }

        // ============================================================== T1 — una referencia: lo historico

        [Fact]
        public void T1_UNA_REFERENCIA_REPRODUCE_EL_RESULTADO_HISTORICO()
        {
            var plan = Build(
                new[] { Ref("R1", "D1") },
                new[] { Def("D1", Envelope(RackA, name: "  Rack A  ")) });

            Assert.True(plan.IsSuccess);
            var group = Assert.Single(plan.Groups);
            Assert.Equal(new[] { "D1" }, group.Definitions.Select(definition => definition.DefinitionKey));
            Assert.Equal(new[] { "R1" }, group.References.Select(reference => reference.ReferenceKey));
            Assert.Equal("Rack A", group.BaseName);

            var assigner = plan.CreateDestinationAssigner(new IdSequence().Next);
            var first = assigner.Next();
            var second = assigner.Next();

            Assert.True(first.IsSuccess);
            Assert.Equal(1, first.Ordinal);
            Assert.Equal("Rack A - copia", Assert.Single(first.Groups).CopyName);

            Assert.True(second.IsSuccess);
            Assert.Equal(2, second.Ordinal);
            Assert.Equal("Rack A - copia 2", Assert.Single(second.Groups).CopyName);
        }

        // ============================================================== T2 — tres vistas de A: un rack, un id

        [Fact]
        public void T2_TRES_VISTAS_DEL_MISMO_RACK_SON_UN_GRUPO_CON_UN_SOLO_ID_Y_NOMBRE_POR_DESTINO()
        {
            var plan = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL"), Ref("RP", "DP") },
                new[]
                {
                    Def("DF", Envelope(RackA, view: RackEmbedDocument.ViewFrontal)),
                    Def("DL", Envelope(RackA, view: RackEmbedDocument.ViewLateral, section: 2)),
                    Def("DP", Envelope(RackA, view: RackEmbedDocument.ViewPlanta)),
                });

            Assert.True(plan.IsSuccess);
            var group = Assert.Single(plan.Groups);
            Assert.Equal(RackDuplicationSourceKeyKind.RackId, group.Key.Kind);
            Assert.Equal(RackA, group.Key.Value);
            Assert.Equal(new[] { "DF", "DL", "DP" }, group.Definitions.Select(definition => definition.DefinitionKey));
            Assert.Equal(new[] { "RF", "RL", "RP" }, group.References.Select(reference => reference.ReferenceKey));

            var ids = new IdSequence();
            var destination = plan.CreateDestinationAssigner(ids.Next).Next();

            // UNA identidad para las tres definiciones: el generador se invoca exactamente una vez por destino.
            Assert.True(destination.IsSuccess);
            Assert.Equal(1, ids.Calls);
            var assignment = Assert.Single(destination.Groups);
            Assert.Equal(group.Key, assignment.Key);
            Assert.NotEqual(Guid.Empty, assignment.NewRackId);
            Assert.Equal("Rack A - copia", assignment.CopyName);
        }

        // ============================================================== T3 — A y B: dos racks

        [Fact]
        public void T3_DOS_RACKS_SON_DOS_GRUPOS_CON_IDS_DISTINTOS_Y_NOMBRE_PROPIO()
        {
            var plan = Build(
                new[] { Ref("RA", "DA"), Ref("RB", "DB") },
                new[]
                {
                    Def("DA", Envelope(RackA, name: "Rack A")),
                    Def("DB", Envelope(RackB, name: "Rack B")),
                });

            Assert.True(plan.IsSuccess);
            Assert.Equal(new[] { RackA, RackB }, plan.Groups.Select(group => group.Key.Value));

            var ids = new IdSequence();
            var destination = plan.CreateDestinationAssigner(ids.Next).Next();

            Assert.True(destination.IsSuccess);
            Assert.Equal(2, ids.Calls);
            Assert.Equal(2, destination.Groups.Select(group => group.NewRackId).Distinct().Count());
            Assert.Equal(new[] { "Rack A - copia", "Rack B - copia" }, destination.Groups.Select(group => group.CopyName));
        }

        // ============================================================== T4 — referencias enlazadas

        [Fact]
        public void T4_DOS_REFERENCIAS_DE_LA_MISMA_DEFINICION_SIGUEN_ENLAZADAS()
        {
            var plan = Build(
                new[] { Ref("R1", "D1"), Ref("R2", "D1"), Ref("R1", "D1") },
                new[] { Def("D1", Envelope(RackA)) });

            Assert.True(plan.IsSuccess);
            var group = Assert.Single(plan.Groups);

            // UNA definicion que clonar y TODAS las referencias fisicas; la repetida cuenta una vez.
            Assert.Equal(new[] { "D1" }, group.Definitions.Select(definition => definition.DefinitionKey));
            Assert.Equal(new[] { "R1", "R2" }, group.References.Select(reference => reference.ReferenceKey));
            Assert.All(group.References, reference => Assert.Equal("D1", reference.DefinitionKey));
        }

        // ============================================================== T5 — sin hermanas automaticas

        [Fact]
        public void T5_UNA_VISTA_SELECCIONADA_NO_ARRASTRA_A_SUS_HERMANAS()
        {
            // La instantanea trae tambien el lateral de A, pero nadie lo selecciono.
            var plan = Build(
                new[] { Ref("RF", "DF") },
                new[]
                {
                    Def("DF", Envelope(RackA, view: RackEmbedDocument.ViewFrontal)),
                    Def("DL", Envelope(RackA, view: RackEmbedDocument.ViewLateral, section: 0)),
                });

            Assert.True(plan.IsSuccess);
            var group = Assert.Single(plan.Groups);
            Assert.Equal(new[] { "DF" }, group.Definitions.Select(definition => definition.DefinitionKey));
            Assert.Equal(new[] { "RF" }, group.References.Select(reference => reference.ReferenceKey));
        }

        // ============================================================== T6 — RackId sin distinguir mayusculas

        [Fact]
        public void T6_RACKIDS_QUE_SOLO_DIFIEREN_EN_MAYUSCULAS_SON_EL_MISMO_RACK()
        {
            var plan = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL") },
                new[]
                {
                    Def("DF", Envelope(RackA)),
                    Def("DL", Envelope(RackA.ToUpperInvariant(), view: RackEmbedDocument.ViewLateral, section: 1)),
                });

            Assert.True(plan.IsSuccess);
            var group = Assert.Single(plan.Groups);
            Assert.Equal(2, group.Definitions.Count);

            var lower = RackDuplicationSourceKey.ForRackId(RackA);
            var upper = RackDuplicationSourceKey.ForRackId(RackA.ToUpperInvariant());
            Assert.Equal(lower, upper);
            Assert.Equal(lower.GetHashCode(), upper.GetHashCode());
        }

        // ============================================================== T7 — legacy sin RackId

        [Fact]
        public void T7_UNA_DEFINICION_SIN_RACKID_ES_SU_PROPIO_ORIGEN_Y_NUNCA_SE_FUSIONA_CON_OTRA()
        {
            // Mismo nombre, mismo kind y mismo contenido en las tres: no importa, no hay identidad que las una.
            var plan = Build(
                new[] { Ref("R1", "DNULL"), Ref("R2", "DNULL"), Ref("R3", "DEMPTY"), Ref("R4", "DSPACES") },
                new[]
                {
                    Def("DNULL", Envelope(null, name: "Legacy")),
                    Def("DEMPTY", Envelope(string.Empty, name: "Legacy")),
                    Def("DSPACES", Envelope("   ", name: "Legacy")),
                });

            Assert.True(plan.IsSuccess);
            Assert.Equal(3, plan.Groups.Count);
            Assert.All(plan.Groups, group => Assert.Equal(RackDuplicationSourceKeyKind.Definition, group.Key.Kind));
            Assert.Equal(new[] { "DNULL", "DEMPTY", "DSPACES" }, plan.Groups.Select(group => group.Key.Value));

            // Las dos referencias de la misma definicion legacy siguen enlazadas.
            var linked = plan.Groups[0];
            Assert.Equal(new[] { "DNULL" }, linked.Definitions.Select(definition => definition.DefinitionKey));
            Assert.Equal(new[] { "R1", "R2" }, linked.References.Select(reference => reference.ReferenceKey));
        }

        // ============================================================== T8 — la clave discriminada no colisiona

        [Fact]
        public void T8_UN_RACKID_IGUAL_AL_TEXTO_DE_UN_HANDLE_NO_COLISIONA_CON_ESA_DEFINICION()
        {
            var plan = Build(
                new[] { Ref("R1", "ABC123"), Ref("R2", "D2") },
                new[]
                {
                    Def("ABC123", Envelope(null, name: "Legacy")),
                    Def("D2", Envelope("ABC123", name: "Legacy")),
                });

            Assert.True(plan.IsSuccess);
            Assert.Equal(2, plan.Groups.Count);
            Assert.NotEqual(plan.Groups[0].Key, plan.Groups[1].Key);

            Assert.NotEqual(RackDuplicationSourceKey.ForRackId("ABC123"), RackDuplicationSourceKey.ForDefinition("ABC123"));
            Assert.NotEqual(RackDuplicationSourceKey.ForRackId("abc123"), RackDuplicationSourceKey.ForDefinition("ABC123"));
        }

        // ============================================================== T9 — clasificacion completa

        [Fact]
        public void T9_LO_QUE_NO_ES_RACKCAD_SE_IGNORA_CON_AVISO_Y_EL_RESTO_SE_DUPLICA()
        {
            var plan = Build(
                new[] { NotABlock("X1"), Ref("R1", "DNULL"), Ref("R2", "DEMPTY"), Ref("R3", "DA") },
                new[]
                {
                    Def("DNULL", null),
                    Def("DEMPTY", string.Empty),
                    Def("DA", Envelope(RackA)),
                });

            Assert.True(plan.IsSuccess);
            Assert.Equal(new[] { "DA" }, Assert.Single(plan.Groups).Definitions.Select(definition => definition.DefinitionKey));
            Assert.Equal(1, plan.IgnoredNonBlockReferences);
            Assert.Equal(2, plan.IgnoredWithoutRackData);
            Assert.Equal(0, plan.IgnoredOutsideModelSpace);
            Assert.Equal(2, plan.Notices.Count);
        }

        public static IEnumerable<object[]> UnusableRackPayloads()
        {
            yield return new object[] { "json-invalido", "{ esto no es json" };
            yield return new object[] { "solo-espacios", "   " };
            yield return new object[]
            {
                "major-futuro",
                "{\"SchemaVersion\":\"9.0\",\"Kind\":\"dynamic\",\"Id\":\"" + RackB + "\",\"Name\":\"Rack B\",\"Design\":\"{}\"}",
            };
            yield return new object[] { "kind-vacio", Envelope(RackB, kind: "  ") };
            yield return new object[] { "kind-desconocido", Envelope(RackB, kind: "mezzanine") };
            yield return new object[] { "design-vacio", Envelope(RackB, design: string.Empty) };
            yield return new object[] { "design-espacios", Envelope(RackB, design: "   ") };
        }

        [Theory]
        [MemberData(nameof(UnusableRackPayloads))]
        public void T9_UN_PAYLOAD_RACKCAD_INUTILIZABLE_FALLA_CERRADO_Y_NO_SE_DESCARTA_EN_SILENCIO(string caso, string payload)
        {
            Assert.False(string.IsNullOrEmpty(caso));

            // Una fuente valida al lado: aun asi no se duplica NADA.
            var plan = Build(
                new[] { Ref("RA", "DA"), Ref("RX", "DX") },
                new[]
                {
                    Def("DA", Envelope(RackA)),
                    Def("DX", payload, name: "BLOQUE_ROTO"),
                });

            Assert.False(plan.IsSuccess);
            Assert.Empty(plan.Groups);
            Assert.Contains(plan.Errors, error => error.Contains("BLOQUE_ROTO"));
        }

        [Fact]
        public void T9_EL_DIAGNOSTICO_NOMBRA_EL_RACKID_CUANDO_SE_CONOCE()
        {
            var plan = Build(
                new[] { Ref("RX", "DX") },
                new[] { Def("DX", Envelope(RackB, kind: "mezzanine"), name: "BLOQUE_ROTO") });

            Assert.False(plan.IsSuccess);
            Assert.Contains(plan.Errors, error => error.Contains("BLOQUE_ROTO") && error.Contains(RackB));
        }

        [Fact]
        public void T9_SIN_NINGUNA_FUENTE_VALIDA_SE_ABORTA()
        {
            var plan = Build(
                new[] { NotABlock("X1"), Ref("R1", "DNULL") },
                new[] { Def("DNULL", null) });

            Assert.False(plan.IsSuccess);
            Assert.Empty(plan.Groups);
            Assert.NotEmpty(plan.Errors);
            Assert.Equal(1, plan.IgnoredNonBlockReferences);
            Assert.Equal(1, plan.IgnoredWithoutRackData);
        }

        // ============================================================== T10 — solo Model Space

        [Fact]
        public void T10_LO_QUE_ESTA_FUERA_DEL_MODEL_SPACE_SE_FILTRA_ANTES_DE_CLASIFICAR()
        {
            var plan = Build(
                new[]
                {
                    Ref("RPAPEL", "DSINSNAPSHOT", inModelSpace: false),
                    Ref("RPAPELROTO", "DROTO", inModelSpace: false),
                    Ref("RA", "DA"),
                },
                new[]
                {
                    Def("DROTO", "{ esto no es json"),
                    Def("DA", Envelope(RackA)),
                });

            // Ni la definicion sin instantanea ni la ilegible cuentan: estaban en Paper Space.
            Assert.True(plan.IsSuccess);
            Assert.Equal(new[] { "RA" }, Assert.Single(plan.Groups).References.Select(reference => reference.ReferenceKey));
            Assert.Equal(2, plan.IgnoredOutsideModelSpace);
            Assert.NotEmpty(plan.Notices);
        }

        // ============================================================== T11 — consistencia de Kind y nombre

        [Fact]
        public void T11_UN_RACK_CUYAS_VISTAS_MEZCLAN_KINDS_FALLA_CERRADO()
        {
            var plan = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL") },
                new[]
                {
                    Def("DF", Envelope(RackA, kind: RackEmbedDocument.KindDynamic)),
                    Def("DL", Envelope(RackA, kind: RackEmbedDocument.KindPushBack, view: RackEmbedDocument.ViewLateral)),
                });

            Assert.False(plan.IsSuccess);
            Assert.Empty(plan.Groups);
            Assert.Contains(plan.Errors, error => error.Contains(RackA));
        }

        [Fact]
        public void T11_UN_KIND_QUE_SOLO_DIFIERE_EN_MAYUSCULAS_ES_EL_MISMO()
        {
            var plan = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL") },
                new[]
                {
                    Def("DF", Envelope(RackA, kind: "dynamic")),
                    Def("DL", Envelope(RackA, kind: "DYNAMIC", view: RackEmbedDocument.ViewLateral)),
                });

            Assert.True(plan.IsSuccess);
            Assert.Equal(2, Assert.Single(plan.Groups).Definitions.Count);
        }

        [Fact]
        public void T11_NOMBRES_LOGICOS_DIVERGENTES_FALLAN_CERRADO()
        {
            var plan = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL") },
                new[]
                {
                    Def("DF", Envelope(RackA, name: "Rack A")),
                    Def("DL", Envelope(RackA, name: "Rack B", view: RackEmbedDocument.ViewLateral)),
                });

            Assert.False(plan.IsSuccess);
            Assert.Empty(plan.Groups);
            Assert.Contains(plan.Errors, error => error.Contains(RackA));
        }

        [Fact]
        public void T11_NOMBRES_VACIOS_NO_DIVERGEN_Y_SIN_NINGUNO_LA_BASE_ES_RACK()
        {
            var compatible = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL"), Ref("RP", "DP") },
                new[]
                {
                    Def("DF", Envelope(RackA, name: "Rack A")),
                    Def("DL", Envelope(RackA, name: "  Rack A  ", view: RackEmbedDocument.ViewLateral)),
                    Def("DP", Envelope(RackA, name: string.Empty, view: RackEmbedDocument.ViewPlanta)),
                });

            Assert.True(compatible.IsSuccess);
            Assert.Equal("Rack A", Assert.Single(compatible.Groups).BaseName);

            var anonymous = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL") },
                new[]
                {
                    Def("DF", Envelope(RackB, name: null)),
                    Def("DL", Envelope(RackB, name: "   ", view: RackEmbedDocument.ViewLateral)),
                });

            Assert.True(anonymous.IsSuccess);
            Assert.Equal("Rack", Assert.Single(anonymous.Groups).BaseName);
        }

        // ============================================================== T12 — autoridad authored del Selectivo

        private static string SelectiveEnvelope(string rackId, double palletDepth, string view, int section = -1)
        {
            var design = new SelectivePalletDesign { VerticalClearance = 6.0, PalletDepth = palletDepth };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);

            var json = new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, rackId, "Rack A"));
            return Envelope(rackId, kind: RackEmbedDocument.KindSelective, view: view, section: section, design: json);
        }

        [Fact]
        public void T12_VISTAS_SELECTIVAS_CON_EL_MISMO_AUTHORED_FORMAN_UN_GRUPO()
        {
            var plan = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL") },
                new[]
                {
                    Def("DF", SelectiveEnvelope(RackA, 48.0, RackEmbedDocument.ViewFrontal)),
                    Def("DL", SelectiveEnvelope(RackA, 48.0, RackEmbedDocument.ViewLateral, 0)),
                });

            Assert.True(plan.IsSuccess);
            Assert.Equal(2, Assert.Single(plan.Groups).Definitions.Count);
        }

        [Fact]
        public void T12_VISTAS_SELECTIVAS_CON_AUTHORED_DIVERGENTE_FALLAN_CERRADO()
        {
            var plan = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL") },
                new[]
                {
                    Def("DF", SelectiveEnvelope(RackA, 48.0, RackEmbedDocument.ViewFrontal)),
                    Def("DL", SelectiveEnvelope(RackA, 42.0, RackEmbedDocument.ViewLateral, 0)),
                });

            Assert.False(plan.IsSuccess);
            Assert.Empty(plan.Groups);
            Assert.Contains(plan.Errors, error => error.Contains(RackA));
        }

        [Fact]
        public void T12_UNA_VISTA_SELECTIVA_ILEGIBLE_FALLA_CERRADO_Y_NUNCA_SE_ELIGE_LA_LEGIBLE()
        {
            var plan = Build(
                new[] { Ref("RF", "DF"), Ref("RL", "DL") },
                new[]
                {
                    Def("DF", SelectiveEnvelope(RackA, 48.0, RackEmbedDocument.ViewFrontal)),
                    Def("DL", Envelope(RackA, kind: RackEmbedDocument.KindSelective, view: RackEmbedDocument.ViewLateral, design: "{ esto no es json")),
                });

            Assert.False(plan.IsSuccess);
            Assert.Empty(plan.Groups);
            Assert.Contains(plan.Errors, error => error.Contains(RackA));
        }

        // ============================================================== T13 — N × M y generador invalido

        [Fact]
        public void T13_DOS_RACKS_POR_TRES_DESTINOS_SON_SEIS_IDENTIDADES_DISTINTAS()
        {
            var plan = Build(
                new[] { Ref("RA", "DA"), Ref("RB", "DB") },
                new[]
                {
                    Def("DA", Envelope(RackA, name: "Rack A")),
                    Def("DB", Envelope(RackB, name: "Rack B")),
                });

            var ids = new IdSequence();
            var assigner = plan.CreateDestinationAssigner(ids.Next);
            var destinations = new[] { assigner.Next(), assigner.Next(), assigner.Next() };

            Assert.All(destinations, destination => Assert.True(destination.IsSuccess));
            Assert.Equal(new[] { 1, 2, 3 }, destinations.Select(destination => destination.Ordinal));
            Assert.Equal(6, ids.Calls);
            Assert.Equal(6, destinations.SelectMany(destination => destination.Groups).Select(group => group.NewRackId).Distinct().Count());
            Assert.Equal(
                new[]
                {
                    "Rack A - copia", "Rack B - copia",
                    "Rack A - copia 2", "Rack B - copia 2",
                    "Rack A - copia 3", "Rack B - copia 3",
                },
                destinations.SelectMany(destination => destination.Groups).Select(group => group.CopyName));
        }

        private static RackDuplicationPlan TwoRacks() => Build(
            new[] { Ref("RA", "DA"), Ref("RB", "DB") },
            new[]
            {
                Def("DA", Envelope(RackA, name: "Rack A")),
                Def("DB", Envelope(RackB, name: "Rack B")),
            });

        [Fact]
        public void T13_UN_GUID_VACIO_FALLA()
        {
            var destination = TwoRacks().CreateDestinationAssigner(() => Guid.Empty).Next();

            Assert.False(destination.IsSuccess);
            Assert.Empty(destination.Groups);
            Assert.False(string.IsNullOrWhiteSpace(destination.Error));
        }

        [Fact]
        public void T13_UN_GUID_IGUAL_A_UN_RACKID_DE_ORIGEN_FALLA()
        {
            var destination = TwoRacks().CreateDestinationAssigner(() => Guid.Parse(RackB)).Next();

            Assert.False(destination.IsSuccess);
            Assert.Empty(destination.Groups);
        }

        [Fact]
        public void T13_UN_GUID_REPETIDO_DENTRO_DE_UN_DESTINO_FALLA()
        {
            var repeated = new Guid("11111111-2222-4333-8444-555555555555");
            var destination = TwoRacks().CreateDestinationAssigner(() => repeated).Next();

            Assert.False(destination.IsSuccess);
            Assert.Empty(destination.Groups);
        }

        [Fact]
        public void T13_UN_GUID_REPETIDO_ENTRE_DESTINOS_FALLA()
        {
            var handed = new Queue<Guid>(new[]
            {
                new Guid("11111111-2222-4333-8444-555555555555"),
                new Guid("66666666-7777-4888-9999-aaaaaaaaaaaa"),
                new Guid("11111111-2222-4333-8444-555555555555"),
                new Guid("bbbbbbbb-cccc-4ddd-8eee-ffffffffffff"),
            });

            var assigner = TwoRacks().CreateDestinationAssigner(handed.Dequeue);
            var first = assigner.Next();
            var second = assigner.Next();

            Assert.True(first.IsSuccess);
            Assert.False(second.IsSuccess);
            Assert.Empty(second.Groups);
        }
    }
}
