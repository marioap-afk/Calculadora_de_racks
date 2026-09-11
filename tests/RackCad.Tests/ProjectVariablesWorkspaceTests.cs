using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G16 — lo que la ventana central ve, y lo que devuelve.
    ///
    /// <para>
    /// La superficie de producto que faltaba desde G7 no puede traer su propia semántica. Todo lo que decide
    /// —quién consume una variable, si se puede borrar, qué significa reparar— ya está decidido en G5 y G6, y
    /// escribir vive en G11. Lo que G16 añade es exactamente dos cosas: una PROYECCIÓN pura de lo que hay en
    /// el dibujo, y un INTENT por identidad. Nada más, porque cualquier otra cosa sería una segunda regla.
    /// </para>
    /// <para>
    /// El <c>VariableId</c> sí viaja a ESTA ventana —administra variables explícitamente y devuelve intents por
    /// identidad—, pero el <b>nombre nunca es autoridad</b>: renombrar no puede mover una operación de sitio.
    /// </para>
    /// <para>
    /// Y el estado bloqueado no es cosmético. Un registro presente-e-ilegible no ofrece «crea tu primera
    /// variable»: escribir encima destruiría en silencio todo lo que ese registro tuviera.
    /// </para>
    /// </summary>
    public class ProjectVariablesWorkspaceTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string RackB = "5c1f0a22-8e6d-4b03-9c47-2a9f6d1e0b57";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Otra = "11111111-2222-3333-4444-555555555555";
        private const string OtraMas = "99999999-8888-7777-6666-555555555555";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static SelectivePalletDesign Diseno(double clearance)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance, PalletDepth = 48.0 };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);
            return design;
        }

        private static SelectivePalletDesignDocument Doc(
            double literal = 6.0, string variableId = null, string rackId = RackA, string name = "Rack A")
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(literal), rackId, name);

            if (variableId != null)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [Token] = SelectivePropertyValueDocument.ToProjectVariable(variableId),
                };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        /// <summary>
        /// Un documento con DOS vinculos. El segundo token es arbitrario a proposito: con una sola propiedad
        /// registrada, es la unica forma de alcanzar en produccion un rack con dos vinculos no resolubles.
        /// </summary>
        private static SelectivePalletDesignDocument Dos(
            double literal, string primera, string segundoToken, string segunda, string rackId = RackA)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(literal), rackId, "Rack A");

            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [Token] = SelectivePropertyValueDocument.ToProjectVariable(primera),
                [segundoToken] = SelectivePropertyValueDocument.ToProjectVariable(segunda),
            };

            doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            return doc;
        }

        private static ProjectVariableScanEntry Vista(
            SelectivePalletDesignDocument doc, string def = "D1", string rackId = RackA, int refs = 1)
            => ProjectVariableScanEntry.Selective(def, rackId, doc, refs);

        private static ProjectVariablesDocument Registro(double value = 10.0, string id = VarId, string name = "Holgura")
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = id,
                    Name = name,
                    Type = VariableType.Length.ToString(),
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
                },
            };

            return document;
        }

        private static ProjectVariablesWorkspace Abrir(
            ProjectVariablesReadResult registry, params ProjectVariableScanEntry[] entries)
            => ProjectVariablesWorkspace.Build(registry, entries);

        // ================================================================ el registro que se puede editar

        [Fact]
        public void UN_REGISTRO_AUSENTE_ES_UN_PROYECTO_SIN_VARIABLES()
        {
            var workspace = Abrir(ProjectVariablesReadResult.Absent());

            Assert.True(workspace.IsEditable);
            Assert.Empty(workspace.Variables);
            Assert.Null(workspace.Error);
        }

        [Fact]
        public void UNA_VARIABLE_SE_PRESENTA_CON_IDENTIDAD_NOMBRE_TIPO_Y_VALOR()
        {
            var workspace = Abrir(ProjectVariablesReadResult.Readable(Registro(10.0)));

            var row = Assert.Single(workspace.Variables);
            Assert.Equal(VariableId.Parse(VarId), row.Id);
            Assert.Equal("Holgura", row.Name);
            Assert.Equal(VariableType.Length, row.Type);
            Assert.Equal(10.0, row.LiteralValue);
        }

        [Fact]
        public void LOS_CONSUMIDORES_SE_CUENTAN_Y_SE_NOMBRAN()
        {
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0)),
                Vista(Doc(6.0, VarId), "D1"),
                Vista(Doc(6.0, VarId, RackB, "Rack B"), "D2", RackB));

            var row = Assert.Single(workspace.Variables);
            Assert.Equal(2, row.ConsumerCount);
            Assert.Contains(row.Consumers, c => c.RackId == RackA && c.PropertyIds.Contains(Token));
            Assert.Contains(row.Consumers, c => c.RackName == "Rack B");
        }

        [Fact]
        public void UNA_VARIABLE_SIN_CONSUMIDORES_LO_DICE()
        {
            var workspace = Abrir(ProjectVariablesReadResult.Readable(Registro()), Vista(Doc(6.0), "D1"));

            Assert.False(Assert.Single(workspace.Variables).HasConsumers);
        }

        // ================================================================ el registro que NO se puede editar

        [Fact]
        public void UN_REGISTRO_ILEGIBLE_BLOQUEA_LA_VENTANA()
        {
            var workspace = Abrir(ProjectVariablesReadResult.Unreadable("registro corrupto"));

            Assert.False(workspace.IsEditable);
            Assert.Contains("registro corrupto", workspace.Error);
            Assert.Empty(workspace.Variables);
        }

        [Fact]
        public void UN_MAJOR_INCOMPATIBLE_BLOQUEA_LA_VENTANA()
        {
            Assert.False(Abrir(ProjectVariablesReadResult.IncompatibleMajor("major 9")).IsEditable);
        }

        [Fact]
        public void SIN_LECTURA_DEL_REGISTRO_NO_SE_EDITA()
        {
            Assert.False(ProjectVariablesWorkspace.Build(null, new ProjectVariableScanEntry[0]).IsEditable);
        }

        /// <summary>
        /// Un sobre indescifrable Y COLOCADO bloquea: sin identidad no se puede demostrar que no pertenezca a
        /// un rack que consume una de estas variables, así que ningún recuento sería honesto. Uno no colocado
        /// no está en el dibujo y no cuenta (misma asimetría que el BOM).
        /// </summary>
        [Fact]
        public void UN_SOBRE_INDESCIFRABLE_COLOCADO_BLOQUEA_LA_VENTANA()
        {
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro()),
                ProjectVariableScanEntry.UnreadableEnvelope("D-X", directReferenceCount: 1));

            Assert.False(workspace.IsEditable);
            Assert.Contains("D-X", workspace.Error);
        }

        [Fact]
        public void UN_SOBRE_INDESCIFRABLE_NO_COLOCADO_NO_ESTORBA()
        {
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro()),
                ProjectVariableScanEntry.UnreadableEnvelope("D-X", directReferenceCount: 0));

            Assert.True(workspace.IsEditable);
        }

        // ================================================================ las referencias rotas

        [Fact]
        public void UNA_REFERENCIA_ROTA_SE_LISTA_CON_TODO_LO_QUE_HACE_FALTA_PARA_REPARARLA()
        {
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Doc(6.0, VarId), "D1"));

            var broken = Assert.Single(workspace.BrokenBindings);
            Assert.Equal(RackA, broken.RackId);
            Assert.Equal("Rack A", broken.RackName);
            Assert.Equal(Token, broken.PropertyId);
            Assert.Equal(VarId, broken.VariableId);
            Assert.Equal(6.0, broken.StoredLiteral);
        }

        [Fact]
        public void UN_ID_ILEGIBLE_TAMBIEN_SE_LISTA_CRUDO()
        {
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro()),
                Vista(Doc(6.0, "no-es-un-guid"), "D1"));

            Assert.Equal("no-es-un-guid", Assert.Single(workspace.BrokenBindings).VariableId);
        }

        [Fact]
        public void UN_VINCULO_QUE_RESUELVE_NO_ES_UNA_REPARACION()
        {
            var workspace = Abrir(ProjectVariablesReadResult.Readable(Registro()), Vista(Doc(6.0, VarId), "D1"));

            Assert.Empty(workspace.BrokenBindings);
        }

        /// <summary>Las hermanas de un mismo rack son la misma reparación, no tres.</summary>
        [Fact]
        public void LAS_VISTAS_DE_UN_MISMO_RACK_SON_UNA_SOLA_REPARACION()
        {
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Doc(6.0, VarId), "D1"),
                Vista(Doc(6.0, VarId), "D2"),
                Vista(Doc(6.0, VarId), "D3"));

            Assert.Single(workspace.BrokenBindings);
        }

        // ---------------------------------------------------------------- I-48 G4B: el escaneo completo

        [Fact]
        public void UN_RACK_CON_DOS_VINCULOS_NO_RESOLUBLES_LISTA_LOS_DOS()
        {
            // El deadlock que la revision encontro: cuando la ventana paraba en el primer fallo del resolver,
            // un rack con dos vinculos rotos publicaba una fila, y la segunda solo aparecia tras arreglar la
            // primera. Se llega a este estado copiando un rack a un dibujo sin registro.
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Dos(6.0, VarId, "selective.noExiste", Otra), "D1"));

            Assert.Equal(2, workspace.BrokenBindings.Count);
        }

        [Fact]
        public void UN_ESTADO_FATAL_DEJA_TODO_EL_RACK_SIN_REPARAR_Y_LO_EXPLICA()
        {
            // La reparabilidad es del RACK. Con un estado fatal en cualquier parte, ninguna fila es accionable
            // -aunque una de ellas sea, por si sola, un target ausente reparable-, porque el ejecutor necesita
            // un diseno efectivo COMPLETO y no hay ninguno.
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Dos(6.0, VarId, "selective.noExiste", Otra), "D1"));

            Assert.All(workspace.BrokenBindings, row => Assert.False(row.RackCanRepair));
            Assert.All(workspace.BrokenBindings, row => Assert.NotNull(row.RackBlockingReason));
        }

        [Fact]
        public void UN_TARGET_AUSENTE_SIN_NADA_FATAL_SI_ES_REPARABLE()
        {
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Doc(6.0, VarId), "D1"));

            var broken = Assert.Single(workspace.BrokenBindings);
            Assert.True(broken.RackCanRepair);
            Assert.Null(broken.RackBlockingReason);
        }

        [Fact]
        public void EL_VEREDICTO_DEL_RACK_NO_DEPENDE_DEL_ORDEN_EN_QUE_SE_ESCANEA()
        {
            // La fila del token que ordena PRIMERO ya sale marcada como no reparable, asi que el veredicto no
            // se publica antes de terminar el escaneo completo del rack.
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Dos(6.0, VarId, "aaa.antesQueTodo", Otra), "D1"));

            Assert.Equal(2, workspace.BrokenBindings.Count);
            Assert.Equal("aaa.antesQueTodo", workspace.BrokenBindings[0].PropertyId);
            Assert.False(workspace.BrokenBindings[0].RackCanRepair);
        }

        [Fact]
        public void EL_LITERAL_DE_UNA_FILA_SALE_DE_SU_DESCRIPTOR_NO_DE_UN_CAMPO_FIJO()
        {
            // Una propiedad que esta version no conoce no tiene descriptor, asi que no hay literal que
            // prometer. Un campo hardcodeado ensenaria la holgura vertical del rack como si fuera el valor que
            // gobernaria tras reparar, justo cuando el usuario tiene que decidir.
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Dos(6.0, VarId, "selective.noExiste", Otra), "D1"));

            var desconocida = Assert.Single(
                workspace.BrokenBindings.Where(row => row.PropertyId == "selective.noExiste"));
            var conocida = Assert.Single(
                workspace.BrokenBindings.Where(row => row.PropertyId == Token));

            Assert.Equal(0.0, desconocida.StoredLiteral);
            Assert.Equal(6.0, conocida.StoredLiteral);
        }

        [Fact]
        public void UN_REGISTRO_CON_VARIABLEID_DUPLICADO_BLOQUEA_LA_VENTANA()
        {
            // El store lo acepta -no valida unicidad de VariableId- pero la identidad no: cada fila y cada
            // conteo tendrian que elegir una de las dos entradas.
            var document = Registro(10.0, id: VarId, name: "Primera");
            document.Variables.Add(new ProjectVariableDocument
            {
                VariableId = VarId,
                Name = "Segunda",
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 20.0 },
            });

            var workspace = Abrir(ProjectVariablesReadResult.Readable(document));

            Assert.False(workspace.IsEditable);
            Assert.Empty(workspace.Variables);
            Assert.Contains(VarId, workspace.Error);
        }

        [Fact]
        public void LAS_FILAS_DE_VARIABLES_SALEN_ORDENADAS_POR_IDENTIDAD()
        {
            // La ventana enumera la MISMA autoridad acreditada que resuelve un lookup, asi que su orden no
            // depende de la enumeracion de un diccionario.
            var document = Registro(10.0, id: Otra, name: "Segunda");
            document.Variables.Add(new ProjectVariableDocument
            {
                VariableId = VarId,
                Name = "Primera",
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 20.0 },
            });

            var workspace = Abrir(ProjectVariablesReadResult.Readable(document));

            Assert.True(workspace.IsEditable);
            Assert.Equal(
                new[] { Otra, VarId },
                workspace.Variables.Select(row => row.Id.Value).ToArray());
        }

        // ---------------------------------------------------------------- I-48 G4B.1: autoridad authored

        /// <summary>
        /// Dos hermanas del MISMO rack, ambas legibles, identicas salvo en <c>PropertyValues</c>: una apunta a
        /// X y la otra a Y. La autoridad heredada ya dice que eso es divergencia, y no hay hermana correcta.
        /// </summary>
        private static ProjectVariableScanEntry[] HermanasDivergentes()
            => new[]
            {
                Vista(Doc(6.0, VarId), "D1"),
                Vista(Doc(6.0, OtraMas), "D2"),
            };

        [Fact]
        public void DOS_HERMANAS_QUE_SOLO_DIFIEREN_EN_PROPERTYVALUES_SON_DIVERGENTES()
        {
            // Primero se demuestra el hecho de partida: la autoridad heredada YA clasifica esto como
            // divergente. Sin esto, la prueba siguiente no distinguiria un fallo de un escenario mal montado.
            var siblings = HermanasDivergentes();
            var authority = SelectiveAuthoredAuthority.Resolve(RackA, siblings);

            Assert.Equal(AuthoredAuthorityOutcome.Divergent, authority.Outcome);
            Assert.Null(authority.Authored);
        }

        [Fact]
        public void UN_RACK_SIN_AUTORIDAD_AUTHORED_NO_PRODUCE_NINGUNA_REPARACION_ACCIONABLE()
        {
            // El defecto: recorrer las vistas de una en una elige implicitamente la PRIMERA hermana, y publica
            // su vinculo como reparable. Reparar aplicaria el literal de una hermana escogida a dedo.
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)), HermanasDivergentes());

            Assert.All(workspace.BrokenBindings, row => Assert.False(row.RackCanRepair));

            // Y el oraculo POSITIVO, para que la assertion anterior no se cumpla por estar vacia: ninguna de
            // las dos hermanas queda presentada como autoridad, y el rack se reporta con su razon.
            Assert.DoesNotContain(VarId, workspace.BrokenBindings.Select(row => row.VariableId));
            Assert.DoesNotContain(OtraMas, workspace.BrokenBindings.Select(row => row.VariableId));

            var sinAutoridad = Assert.Single(workspace.UnresolvableRacks);
            Assert.Equal(RackA, sinAutoridad.RackId);
            Assert.Contains("divergentes", sinAutoridad.Reason);

            // No bloquea el registro: un rack roto ajeno no puede paralizar toda la administracion.
            Assert.True(workspace.IsEditable);
        }

        [Fact]
        public void CON_HERMANAS_DIVERGENTES_LA_DISPOSICION_NO_DEPENDE_DEL_ORDEN_DE_BARRIDO()
        {
            // No basta con comparar mensajes: lo que no puede cambiar es QUE VariableId y QUE literal quedan
            // presentados como autoridad del rack.
            var directo = HermanasDivergentes();
            var inverso = new[] { directo[1], directo[0] };

            var a = Abrir(ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)), directo);
            var b = Abrir(ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)), inverso);

            Assert.Equal(
                a.BrokenBindings.Select(row => row.VariableId).ToArray(),
                b.BrokenBindings.Select(row => row.VariableId).ToArray());
            Assert.Equal(
                a.BrokenBindings.Select(row => row.StoredLiteral).ToArray(),
                b.BrokenBindings.Select(row => row.StoredLiteral).ToArray());
            Assert.Equal(
                a.BrokenBindings.Select(row => row.RackCanRepair).ToArray(),
                b.BrokenBindings.Select(row => row.RackCanRepair).ToArray());

            // Y el diagnostico tampoco cambia de forma con el orden: mismo rack, misma razon.
            Assert.Equal(
                a.UnresolvableRacks.Select(rack => rack.RackId + "|" + rack.Reason).ToArray(),
                b.UnresolvableRacks.Select(rack => rack.RackId + "|" + rack.Reason).ToArray());
            Assert.Single(a.UnresolvableRacks);
        }

        [Fact]
        public void UNA_HERMANA_ILEGIBLE_TAMPOCO_DEJA_ELEGIR_A_LA_LEGIBLE()
        {
            // Filtrar la ilegible para seguir con las legibles es exactamente lo que hace que una respuesta
            // parcial parezca completa.
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Doc(6.0, VarId), "D1"),
                ProjectVariableScanEntry.SelectiveUnreadableDesign("D2", RackA));

            Assert.All(workspace.BrokenBindings, row => Assert.False(row.RackCanRepair));

            // La hermana legible NO se presenta como autoridad del rack.
            Assert.DoesNotContain(VarId, workspace.BrokenBindings.Select(row => row.VariableId));

            var sinAutoridad = Assert.Single(workspace.UnresolvableRacks);
            Assert.Equal(RackA, sinAutoridad.RackId);
            Assert.Contains("no se puede interpretar", sinAutoridad.Reason);
        }

        /// <summary>Centinela: la via sana no se degrada. Hermanas identicas siguen siendo UNA reparacion.</summary>
        [Fact]
        public void HERMANAS_IDENTICAS_CON_UN_VINCULO_ROTO_SIGUEN_SIENDO_UNA_SOLA_REPARACION()
        {
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Doc(6.0, VarId), "D1"),
                Vista(Doc(6.0, VarId), "D2"),
                Vista(Doc(6.0, VarId), "D3"));

            var broken = Assert.Single(workspace.BrokenBindings);
            Assert.True(broken.RackCanRepair);
            Assert.Equal(VarId, broken.VariableId);
            Assert.Equal(6.0, broken.StoredLiteral);
            Assert.Empty(workspace.UnresolvableRacks);

            // Y el batch de la reparacion existe y describe exactamente esa unica rota.
            Assert.Equal(RackA, broken.RepairBatch.RackId);
            var binding = Assert.Single(broken.RepairBatch.Bindings);
            Assert.Equal(Token, binding.PropertyId);
            Assert.Equal(VarId, binding.VariableId);
            Assert.Equal(6.0, binding.StoredLiteral);
        }

        // ---------------------------------------------------------------- I-48 G4B.1: el batch del rack

        [Fact]
        public void TODAS_LAS_FILAS_DE_UN_RACK_REPARABLE_COMPARTEN_EL_MISMO_BATCH()
        {
            // La unidad de la operacion es el rack, asi que el alcance no puede depender de la fila elegida.
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Doc(6.0, VarId), "D1"),
                Vista(Doc(6.0, VarId), "D2"));

            var batch = Assert.Single(workspace.BrokenBindings).RepairBatch;
            Assert.All(workspace.BrokenBindings, row => Assert.Same(batch, row.RepairBatch));
        }

        [Fact]
        public void UN_RACK_BLOQUEADO_NO_TIENE_BATCH_DE_REPARACION()
        {
            // Sin batch no hay alcance que ofrecer, y RackCanRepair no puede discrepar de eso porque ES eso.
            var workspace = Abrir(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                Vista(Dos(6.0, VarId, "selective.noExiste", Otra), "D1"));

            Assert.Equal(2, workspace.BrokenBindings.Count);
            Assert.All(workspace.BrokenBindings, row => Assert.Null(row.RepairBatch));
            Assert.All(workspace.BrokenBindings, row => Assert.False(row.RackCanRepair));
        }

        // ================================================================ los intents

        [Fact]
        public void CREAR_LLEVA_NOMBRE_Y_VALOR_Y_NO_INVENTA_IDENTIDAD()
        {
            var intent = ProjectVariableIntent.Create("Holgura", 10.0);

            Assert.Equal(ProjectVariableIntentKind.Create, intent.Kind);
            Assert.Equal("Holgura", intent.Name);
            Assert.Equal(10.0, intent.Value);
        }

        [Fact]
        public void RENOMBRAR_VIAJA_POR_IDENTIDAD()
        {
            var intent = ProjectVariableIntent.Rename(VariableId.Parse(VarId), "Otro nombre");

            Assert.Equal(VariableId.Parse(VarId), intent.VariableId);
            Assert.Equal("Otro nombre", intent.Name);
        }

        [Fact]
        /// <summary>
        /// I-48 G4B cambio el ALCANCE de reparar: viaja por RACK y por nada mas. Antes llevaba tambien una
        /// PropertyId, y eso invitaba a creer que acotaba el conjunto; no puede acotarlo, porque una reparacion
        /// parcial no produce el diseno efectivo completo que el executor redibuja.
        /// </summary>
        public void REPARAR_VIAJA_POR_RACK_Y_NO_LLEVA_ALCANCE_DE_PROPIEDAD()
        {
            var intent = ProjectVariableIntent.RepairBroken(RackA, confirmed: true);

            Assert.Equal(RackA, intent.RackId);
            Assert.Null(intent.PropertyId);
            Assert.True(intent.Confirmed);
        }

        // ================================================================ el intent llama a G6, y a nada más

        private static VariableMutationPreflightResult Correr(
            ProjectVariableIntent intent, ProjectVariablesDocument registry, params ProjectVariableScanEntry[] entries)
            => ProjectVariableIntentPreflight.Run(intent, registry, entries);

        [Fact]
        public void CREAR_PRODUCE_UN_PLAN_DE_REGISTRO()
        {
            var result = Correr(ProjectVariableIntent.Create("Holgura", 10.0), ProjectVariablesDocument.CreateNew());

            Assert.True(result.IsSuccess);
            Assert.Equal(RegistryMutationKind.Add, result.Plan.RegistryMutation.Kind);
            Assert.Empty(result.Plan.RackMutations);
        }

        /// <summary>Renombrar no barre el dibujo y no redibuja: por eso la identidad es independiente del nombre.</summary>
        [Fact]
        public void RENOMBRAR_NO_TOCA_NINGUN_RACK()
        {
            var result = Correr(
                ProjectVariableIntent.Rename(VariableId.Parse(VarId), "Holgura General"),
                Registro(),
                Vista(Doc(6.0, VarId), "D1"));

            Assert.True(result.IsSuccess);
            Assert.Equal(RegistryMutationKind.Rename, result.Plan.RegistryMutation.Kind);
            Assert.Empty(result.Plan.RackMutations);
        }

        [Fact]
        public void CAMBIAR_VALOR_ARRASTRA_A_LOS_CONSUMIDORES()
        {
            var result = Correr(
                ProjectVariableIntent.ChangeValue(VariableId.Parse(VarId), 12.0),
                Registro(10.0),
                Vista(Doc(6.0, VarId), "D1"));

            Assert.True(result.IsSuccess);
            Assert.Equal(RegistryMutationKind.ChangeValue, result.Plan.RegistryMutation.Kind);
            Assert.Single(result.Plan.RackMutations);
        }

        [Fact]
        public void BORRAR_CON_CONSUMIDORES_SE_BLOQUEA_Y_LOS_ENSENA()
        {
            var result = Correr(
                ProjectVariableIntent.Delete(VariableId.Parse(VarId)),
                Registro(),
                Vista(Doc(6.0, VarId), "D1"));

            Assert.Equal(VariableMutationOutcome.BlockedByConsumers, result.Outcome);
            Assert.NotEmpty(result.BlockingConsumers);
            Assert.True(result.Plan.IsEmpty);
        }

        [Fact]
        public void BORRAR_SIN_CONSUMIDORES_PRODUCE_PLAN()
        {
            var result = Correr(ProjectVariableIntent.Delete(VariableId.Parse(VarId)), Registro());

            Assert.True(result.IsSuccess);
            Assert.Equal(RegistryMutationKind.Remove, result.Plan.RegistryMutation.Kind);
        }

        /// <summary>Sin confirmación explícita no hay desvinculación masiva: es irreversible desde el editor.</summary>
        [Fact]
        public void DESVINCULAR_TODOS_Y_ELIMINAR_EXIGE_CONFIRMACION()
        {
            var sin = Correr(
                ProjectVariableIntent.UnlinkAllAndDelete(VariableId.Parse(VarId), confirmed: false),
                Registro(),
                Vista(Doc(6.0, VarId), "D1"));

            Assert.False(sin.IsSuccess);
            Assert.True(sin.Plan.IsEmpty);

            var con = Correr(
                ProjectVariableIntent.UnlinkAllAndDelete(VariableId.Parse(VarId), confirmed: true),
                Registro(),
                Vista(Doc(6.0, VarId), "D1"));

            Assert.True(con.IsSuccess);
            Assert.Equal(RegistryMutationKind.Remove, con.Plan.RegistryMutation.Kind);
            Assert.Single(con.Plan.RackMutations);
        }

        [Fact]
        public void REPARAR_SIN_CONFIRMAR_NO_PRODUCE_PLAN()
        {
            var result = Correr(
                ProjectVariableIntent.RepairBroken(RackA, confirmed: false),
                Registro(10.0, id: Otra),
                Vista(Doc(6.0, VarId), "D1"));

            Assert.False(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
        }

        [Fact]
        public void REPARAR_CONFIRMADO_QUITA_EL_VINCULO_Y_DEJA_EL_LITERAL()
        {
            var result = Correr(
                ProjectVariableIntent.RepairBroken(RackA, confirmed: true),
                Registro(10.0, id: Otra),
                Vista(Doc(6.0, VarId), "D1"));

            Assert.True(result.IsSuccess);
            var rack = Assert.Single(result.Plan.RackMutations);
            Assert.False(rack.AuthoredOutput.HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance));
            Assert.Equal(6.0, rack.AuthoredOutput.VerticalClearance);
            Assert.Equal(
                SelectivePalletDesignDocument.PromotedSchemaVersion, rack.AuthoredOutput.SchemaVersion);
        }

        [Fact]
        public void UN_INTENT_NULO_NO_PRODUCE_PLAN()
        {
            Assert.False(ProjectVariableIntentPreflight.Run(null, Registro(), new ProjectVariableScanEntry[0]).IsSuccess);
        }

        // ================================================================ guardas de fuente del comando

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

        private static string Command()
            => File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.Plugin", "RackVariablesCommands.cs"));

        [Fact]
        public void GUARDA_EXISTE_UN_COMANDO_PERMANENTE()
        {
            Assert.Contains("[CommandMethod(\"RACKVARIABLES\")]", Command());
        }

        [Fact]
        public void GUARDA_EL_COMANDO_LEE_EL_REGISTRO_Y_BARRE_EL_DIBUJO()
        {
            var source = Command();

            Assert.Contains("ProjectVariablesRegistry.Read", source);
            Assert.Contains("RackBlockFinder.ScanEnvelopes", source);
            Assert.Contains("ProjectVariableScanProjection.Project", source);
        }

        [Fact]
        public void GUARDA_EL_COMANDO_PASA_POR_EL_PREFLIGHT_DE_G6_Y_EL_EJECUTOR_DE_G11()
        {
            var source = Command();

            Assert.Contains("ProjectVariableIntentPreflight.Run", source);
            Assert.Contains("ProjectVariableMutationExecutor.Execute", source);
        }

        /// <summary>Ni la ventana ni el comando escriben el registro por su cuenta.</summary>
        [Fact]
        public void GUARDA_NADIE_ESCRIBE_EL_NOD_DIRECTAMENTE()
        {
            Assert.DoesNotContain("ProjectVariablesData", Command());
            Assert.DoesNotContain("ProjectVariablesRegistry.TryWrite", Command());
        }

        /// <summary>
        /// La prohibición del contrato, hecha guarda: G16 no reimplementa buscar consumidores, resolver
        /// efectivo, materializar, borrar ni reparar. Todo eso ya existe.
        /// </summary>
        [Fact]
        public void GUARDA_EL_COMANDO_NO_DUPLICA_SEMANTICA()
        {
            var source = Command();

            Assert.DoesNotContain("SelectiveEffectiveDesignResolver", source);
            Assert.DoesNotContain("ProjectVariableConsumerDiscovery", source);
            Assert.DoesNotContain("RegistryMutation.", source);
            Assert.DoesNotContain("MutationPlan.Of", source);
        }

        [Fact]
        public void GUARDA_LA_VENTANA_NO_CONOCE_AUTOCAD()
        {
            var window = File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.UI", "RackProjectVariablesWindow.xaml.cs"));

            Assert.DoesNotContain("Autodesk.AutoCAD", window);
            Assert.DoesNotContain("ObjectId", window);
            Assert.DoesNotContain("Transaction", window);
            Assert.DoesNotContain("ProjectVariablesRegistry", window);
            Assert.DoesNotContain("ProjectVariablesData", window);
        }

        /// <summary>
        /// La ventana central administra; el editor Selectivo no.
        ///
        /// <para>
        /// I-47 G17 reapunta esta guarda: nacio para probar que G16 no adelantaba el vinculo por propiedad, y
        /// G17 ya lo introdujo. Lo que sigue siendo cierto —y es lo que se protege— es que el editor no lleva
        /// los intents de administracion del registro ni construye la lista de opciones: la recibe.
        /// </para>
        /// </summary>
        [Fact]
        public void GUARDA_EL_EDITOR_SELECTIVO_NO_ADMINISTRA_EL_REGISTRO()
        {
            var selective = File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.UI", "Systems", "Selective", "RackSelectiveWindow.xaml.cs"));

            Assert.DoesNotContain("ProjectVariableIntent", selective);
            Assert.DoesNotContain("SelectiveBindingOptions", selective);
            Assert.DoesNotContain("ProjectVariablesWorkspace", selective);
        }
    }
}
