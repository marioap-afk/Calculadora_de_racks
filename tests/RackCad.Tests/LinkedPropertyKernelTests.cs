using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-48 gate G4B — el KERNEL generico: escaneo completo, veredicto por rack y resolucion por descriptor.
    ///
    /// <para>
    /// Lo que estas pruebas fijan no se puede leer en las firmas, y una de ellas no se podia ni siquiera
    /// formular antes de este gate:
    /// </para>
    /// <list type="number">
    /// <item><b>La genericidad no es observable con un catalogo de una sola propiedad.</b> Con una unica
    /// propiedad registrada, «cada vinculo escribe por SU descriptor» y «todo vinculo escribe la holgura
    /// vertical» dan exactamente el mismo resultado: el hardcode a nivel de CAMPO pasaria todas las pruebas.
    /// Por eso la genericidad se demuestra con dos propiedades SINTETICAS sobre campos REALES y distintos, a
    /// traves del seam puro del resolver. No se registra ninguna segunda propiedad productiva.</item>
    /// <item><b>El escaneo es completo y determinista.</b> No para en el primer fallo, no descarta lo que no
    /// entiende, y su orden no depende del orden de insercion del mapa persistido.</item>
    /// <item><b>La reparabilidad es del RACK, no de la fila.</b> Un solo estado FATAL en cualquier parte del
    /// rack deja TODO el rack sin reparar, incluidos los targets ausentes, que siguen siendo visibles como
    /// diagnostico.</item>
    /// <item><b>Nada degrada al literal.</b> Los cinco estados no sanos abortan, y el aborto ocurre ANTES de
    /// escribir un solo campo: no queda un diseno a medio aplicar.</item>
    /// </list>
    /// </summary>
    public class LinkedPropertyKernelTests
    {
        private const string ClearanceToken = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        // Dos propiedades que esta build NO registra productivamente. Existen para separar «generico» de
        // «un campo hardcodeado», y apuntan a campos reales y DISTINTOS del documento y del diseno.
        private const string AlphaToken = "test.alphaRise";
        private const string BetaToken = "test.betaDepth";

        private const string GuidA = "11111111-1111-1111-1111-111111111111";
        private const string GuidB = "22222222-2222-2222-2222-222222222222";
        private const string GuidC = "33333333-3333-3333-3333-333333333333";

        private const VariableType TipoSintetico = (VariableType)9001;

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static VariableTargetSnapshot Target(string guid, double value, VariableType type = VariableType.Length)
        {
            Assert.True(VariableTargetSnapshot.TryCreate(Id(guid), type, value, out var snapshot, out _, "V"));
            return snapshot;
        }

        private static UsableProjectVariablesRegistry Registry(params VariableTargetSnapshot[] targets)
        {
            var accreditation = UsableProjectVariablesRegistry.FromTargets(targets);
            Assert.True(accreditation.IsUsable);
            return accreditation.Registry;
        }

        private static LinkedPropertyDescriptorSet SetOf(params SelectiveLinkedPropertyDescriptor[] descriptors)
        {
            Assert.True(LinkedPropertyDescriptorSet.TryCreate(descriptors, out var set, out _));
            return set;
        }

        /// <summary>La holgura vertical: el descriptor productivo, tal cual.</summary>
        private static SelectiveLinkedPropertyDescriptor Clearance()
        {
            Assert.True(SelectiveLinkedProperties.All.TryGetDescriptor(
                PropertyId.Parse(ClearanceToken), out var descriptor));
            return descriptor;
        }

        /// <summary>Propiedad sintetica ALPHA: gobierna <c>FloorBeamRise</c> y nada mas.</summary>
        private static SelectiveLinkedPropertyDescriptor Alpha(VariableType type = VariableType.Length)
            => new SelectiveLinkedPropertyDescriptor(
                PropertyId.Parse(AlphaToken),
                type,
                authored => authored.FloorBeamRise,
                (authored, value) => authored.FloorBeamRise = value,
                design => design.FloorBeamRise,
                (design, value) => design.FloorBeamRise = value);

        /// <summary>Propiedad sintetica BETA: gobierna <c>PalletDepth</c> y nada mas.</summary>
        private static SelectiveLinkedPropertyDescriptor Beta(VariableType type = VariableType.Length)
            => new SelectiveLinkedPropertyDescriptor(
                PropertyId.Parse(BetaToken),
                type,
                authored => authored.PalletDepth,
                (authored, value) => authored.PalletDepth = value,
                design => design.PalletDepth,
                (design, value) => design.PalletDepth = value);

        private static SelectivePalletDesign Diseno()
        {
            var design = new SelectivePalletDesign
            {
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                PalletTolerance = 4.0,
            };

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

        /// <summary>
        /// Un documento con los vinculos EN EL ORDEN DADO. El orden importa: es justo lo que no debe influir.
        /// </summary>
        private static SelectivePalletDesignDocument Doc(params KeyValuePair<string, SelectivePropertyValueDocument>[] bindings)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(), GuidC, "Rack A");

            if (bindings.Length == 0)
            {
                return doc;
            }

            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>();

            foreach (var binding in bindings)
            {
                doc.PropertyValues[binding.Key] = binding.Value;
            }

            doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            return doc;
        }

        private static KeyValuePair<string, SelectivePropertyValueDocument> Bind(string token, string guid)
            => new KeyValuePair<string, SelectivePropertyValueDocument>(
                token, SelectivePropertyValueDocument.ToProjectVariable(guid));

        private static KeyValuePair<string, SelectivePropertyValueDocument> Raw(
            string token, SelectivePropertyValueDocument reference)
            => new KeyValuePair<string, SelectivePropertyValueDocument>(token, reference);

        private static IReadOnlyList<string> Tokens(IReadOnlyList<BindingInspection> inspections)
        {
            var tokens = new List<string>();

            foreach (var inspection in inspections)
            {
                tokens.Add(inspection.PropertyToken);
            }

            return tokens;
        }

        // ================================================================ escaneo: completo y determinista

        [Fact]
        public void EL_ESCANEO_NO_DEPENDE_DEL_ORDEN_DE_INSERCION_DEL_MAPA_PERSISTIDO()
        {
            var registry = Registry(Target(GuidA, 11.0), Target(GuidB, 22.0));
            var descriptors = SetOf(Alpha(), Beta());

            var directo = SelectiveLinkedPropertyKernel.InspectBindings(
                Doc(Bind(AlphaToken, GuidA), Bind(BetaToken, GuidB)), descriptors, registry);

            var inverso = SelectiveLinkedPropertyKernel.InspectBindings(
                Doc(Bind(BetaToken, GuidB), Bind(AlphaToken, GuidA)), descriptors, registry);

            // Mismo orden de salida, y es el Ordinal del token persistido, no el de insercion.
            Assert.Equal(new[] { AlphaToken, BetaToken }, Tokens(directo));
            Assert.Equal(Tokens(directo), Tokens(inverso));
        }

        [Fact]
        public void EL_ESCANEO_NO_DESCARTA_UNA_PROPIEDAD_QUE_NO_ENTIENDE()
        {
            // Descartarla convertiria «esta build no sabe interpretar esto» en «no hay vinculo», que es
            // exactamente el colapso que I-48 existe para impedir.
            var inspections = SelectiveLinkedPropertyKernel.InspectBindings(
                Doc(Bind("selective.noExiste", GuidA), Bind(ClearanceToken, GuidA)),
                SelectiveLinkedProperties.All,
                Registry(Target(GuidA, 11.0)));

            Assert.Equal(2, inspections.Count);
            Assert.Contains("selective.noExiste", Tokens(inspections));
        }

        [Fact]
        public void EL_ESCANEO_NO_PARA_EN_EL_PRIMER_FALLO()
        {
            var inspections = SelectiveLinkedPropertyKernel.InspectBindings(
                Doc(Bind(AlphaToken, GuidC), Bind(BetaToken, GuidC)),
                SetOf(Alpha(), Beta()),
                Registry(Target(GuidA, 11.0)));

            Assert.Equal(2, inspections.Count);
            Assert.All(inspections, i => Assert.Equal(BindingInspectionOutcome.RepairableMissingTarget, i.Outcome));
        }

        [Fact]
        public void UN_DOCUMENTO_SIN_PROPERTYVALUES_NO_PRODUCE_INSPECCIONES()
        {
            var inspections = SelectiveLinkedPropertyKernel.InspectBindings(
                Doc(), SelectiveLinkedProperties.All, Registry());

            Assert.Empty(inspections);
        }

        // ================================================================ veredicto por rack (7 casos)

        [Fact]
        public void RACK_SIN_VINCULOS_ES_HEALTHY_Y_NO_HAY_NADA_QUE_REPARAR()
        {
            var assessment = SelectiveLinkedPropertyKernel.Assess(
                Doc(), SelectiveLinkedProperties.All, Registry());

            Assert.Equal(RackRepairability.Healthy, assessment.Outcome);
            Assert.False(assessment.CanRepair);
            Assert.Empty(assessment.Missing);
            Assert.Null(assessment.BlockingReason);
        }

        [Fact]
        public void RACK_CON_TODOS_LOS_VINCULOS_SANOS_ES_HEALTHY()
        {
            var assessment = SelectiveLinkedPropertyKernel.Assess(
                Doc(Bind(AlphaToken, GuidA), Bind(BetaToken, GuidB)),
                SetOf(Alpha(), Beta()),
                Registry(Target(GuidA, 11.0), Target(GuidB, 22.0)));

            Assert.Equal(RackRepairability.Healthy, assessment.Outcome);
            Assert.False(assessment.CanRepair);
            Assert.Empty(assessment.Missing);
        }

        [Fact]
        public void RACK_CON_UN_TARGET_AUSENTE_ES_REPARABLE()
        {
            var assessment = SelectiveLinkedPropertyKernel.Assess(
                Doc(Bind(ClearanceToken, GuidB)),
                SelectiveLinkedProperties.All,
                Registry(Target(GuidA, 11.0)));

            Assert.Equal(RackRepairability.Repairable, assessment.Outcome);
            Assert.True(assessment.CanRepair);
            Assert.Single(assessment.Missing);
            Assert.Null(assessment.BlockingReason);
        }

        [Fact]
        public void RACK_CON_DOS_TARGETS_AUSENTES_REPORTA_LOS_DOS_Y_SIGUE_REPARABLE()
        {
            // El deadlock que la revision encontro: con dos referencias rotas, reportar solo una hacia que la
            // segunda apareciera nada mas arreglar la primera, y una reparacion parcial no puede aplicarse.
            var assessment = SelectiveLinkedPropertyKernel.Assess(
                Doc(Bind(AlphaToken, GuidB), Bind(BetaToken, GuidC)),
                SetOf(Alpha(), Beta()),
                Registry(Target(GuidA, 11.0)));

            Assert.Equal(RackRepairability.Repairable, assessment.Outcome);
            Assert.True(assessment.CanRepair);
            Assert.Equal(2, assessment.Missing.Count);
        }

        [Fact]
        public void RACK_CON_UN_AUSENTE_Y_UN_FATAL_ESTA_BLOQUEADO_Y_EL_AUSENTE_SIGUE_VISIBLE()
        {
            var assessment = SelectiveLinkedPropertyKernel.Assess(
                Doc(Bind(AlphaToken, GuidB), Raw(BetaToken, new SelectivePropertyValueDocument
                {
                    Kind = "rackProperty",
                    VariableId = GuidA,
                })),
                SetOf(Alpha(), Beta()),
                Registry(Target(GuidA, 11.0)));

            Assert.Equal(RackRepairability.Blocked, assessment.Outcome);
            Assert.False(assessment.CanRepair);
            Assert.True(assessment.IsBlocked);

            // El ausente NO desaparece del diagnostico; lo que desaparece es que sea accionable.
            Assert.Single(assessment.Missing);
            Assert.Single(assessment.Fatal);
            Assert.NotNull(assessment.BlockingReason);
        }

        [Fact]
        public void RACK_CON_UN_TIPO_INCOMPATIBLE_ESTA_BLOQUEADO_Y_NO_ES_HEALTHY()
        {
            // El target EXISTE. Un veredicto de tres estados que preguntara solo «resuelve?» lo habria
            // clasificado como sano, que es el cubo mas peligroso de los tres.
            var assessment = SelectiveLinkedPropertyKernel.Assess(
                Doc(Bind(ClearanceToken, GuidA)),
                SelectiveLinkedProperties.All,
                Registry(Target(GuidA, 11.0, TipoSintetico)));

            Assert.Equal(RackRepairability.Blocked, assessment.Outcome);
            Assert.Empty(assessment.Missing);
            Assert.Single(assessment.Fatal);
            Assert.Equal(BindingInspectionOutcome.FatalIncompatibleTarget, assessment.Fatal[0].Outcome);
        }

        [Fact]
        public void RACK_CON_DOS_FATALES_EXPLICA_EL_PRIMERO_EN_ORDEN_DETERMINISTA()
        {
            var assessment = SelectiveLinkedPropertyKernel.Assess(
                Doc(
                    Raw(BetaToken, new SelectivePropertyValueDocument
                    {
                        Kind = SelectivePropertyValueDocument.ProjectVariableKind,
                        VariableId = "no-soy-un-guid",
                    }),
                    Raw(AlphaToken, new SelectivePropertyValueDocument { Kind = "rackProperty", VariableId = GuidA })),
                SetOf(Alpha(), Beta()),
                Registry(Target(GuidA, 11.0)));

            Assert.Equal(RackRepairability.Blocked, assessment.Outcome);
            Assert.Equal(2, assessment.Fatal.Count);

            // ALPHA ordena antes que BETA en Ordinal, y el motivo publicado es el suyo pese a haberse
            // insertado despues.
            Assert.Equal(AlphaToken, assessment.Fatal[0].PropertyToken);
            Assert.Equal(assessment.Fatal[0].Detail, assessment.BlockingReason);
        }

        // ================================================================ P: las propiedades de una variable

        [Fact]
        public void P_INCLUYE_TODAS_LAS_PROPIEDADES_QUE_APUNTAN_A_LA_MISMA_VARIABLE()
        {
            var properties = SelectiveLinkedPropertyKernel.PropertiesBoundTo(
                Doc(Bind(BetaToken, GuidA), Bind(AlphaToken, GuidA)), Id(GuidA));

            Assert.Equal(2, properties.Count);
            Assert.Equal(AlphaToken, properties[0].Value);
            Assert.Equal(BetaToken, properties[1].Value);
        }

        [Fact]
        public void P_EXCLUYE_LAS_PROPIEDADES_DE_OTRA_VARIABLE()
        {
            var properties = SelectiveLinkedPropertyKernel.PropertiesBoundTo(
                Doc(Bind(AlphaToken, GuidA), Bind(BetaToken, GuidB)), Id(GuidA));

            Assert.Single(properties);
            Assert.Equal(AlphaToken, properties[0].Value);
        }

        [Fact]
        public void P_EXCLUYE_UN_KIND_DESCONOCIDO_Y_UN_ID_ILEGIBLE()
        {
            var doc = Doc(
                Raw(AlphaToken, new SelectivePropertyValueDocument { Kind = "rackProperty", VariableId = GuidA }),
                Raw(BetaToken, new SelectivePropertyValueDocument
                {
                    Kind = SelectivePropertyValueDocument.ProjectVariableKind,
                    VariableId = "no-soy-un-guid",
                }));

            Assert.Empty(SelectiveLinkedPropertyKernel.PropertiesBoundTo(doc, Id(GuidA)));
        }

        [Fact]
        public void P_DE_UN_RACK_SIN_VINCULOS_ES_VACIO()
        {
            Assert.Empty(SelectiveLinkedPropertyKernel.PropertiesBoundTo(Doc(), Id(GuidA)));
        }

        // ================================================================ GENERICIDAD (la prueba de E)

        [Fact]
        public void DOS_PROPIEDADES_RESUELVEN_CADA_UNA_SU_PROPIO_CAMPO()
        {
            // Este es el test que un hardcode a nivel de campo NO puede pasar: si ambos vinculos escribieran
            // la holgura vertical, FloorBeamRise y PalletDepth se quedarian en su literal autored.
            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Bind(AlphaToken, GuidA), Bind(BetaToken, GuidB)),
                Registry(Target(GuidA, 33.0), Target(GuidB, 77.0)),
                SetOf(Alpha(), Beta()));

            Assert.True(resolution.IsSuccess);

            // Los oraculos son los campos CONCRETOS del dominio, no el descriptor que los escribio.
            Assert.Equal(33.0, resolution.Design.FloorBeamRise);
            Assert.Equal(77.0, resolution.Design.PalletDepth);

            // Y el campo historicamente hardcodeado no se toca: conserva su literal autored.
            Assert.Equal(6.0, resolution.Design.VerticalClearance);
            Assert.Equal(4.0, resolution.Design.PalletTolerance);
        }

        [Fact]
        public void RESOLVER_UNA_PROPIEDAD_NO_ESCRIBE_EL_CAMPO_DE_LA_OTRA()
        {
            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Bind(AlphaToken, GuidA)),
                Registry(Target(GuidA, 33.0), Target(GuidB, 77.0)),
                SetOf(Alpha(), Beta()));

            Assert.True(resolution.IsSuccess);
            Assert.Equal(33.0, resolution.Design.FloorBeamRise);

            // BETA no esta vinculada: su campo sigue siendo el literal, no el valor de ninguna variable.
            Assert.Equal(48.0, resolution.Design.PalletDepth);
        }

        [Fact]
        public void EL_RESULTADO_NO_DEPENDE_DEL_ORDEN_DEL_MAPA_PERSISTIDO()
        {
            var registry = Registry(Target(GuidA, 33.0), Target(GuidB, 77.0));
            var descriptors = SetOf(Alpha(), Beta());
            var resolver = new SelectiveEffectiveDesignResolver();

            var directo = resolver.ResolveWith(
                Doc(Bind(AlphaToken, GuidA), Bind(BetaToken, GuidB)), registry, descriptors);

            var inverso = resolver.ResolveWith(
                Doc(Bind(BetaToken, GuidB), Bind(AlphaToken, GuidA)), registry, descriptors);

            Assert.True(directo.IsSuccess);
            Assert.True(inverso.IsSuccess);
            Assert.Equal(directo.Design.FloorBeamRise, inverso.Design.FloorBeamRise);
            Assert.Equal(directo.Design.PalletDepth, inverso.Design.PalletDepth);
        }

        [Fact]
        public void CAMBIAR_UNA_SOLA_VARIABLE_MUEVE_UN_SOLO_CAMPO()
        {
            var descriptors = SetOf(Alpha(), Beta());
            var resolver = new SelectiveEffectiveDesignResolver();
            var authored = Doc(Bind(AlphaToken, GuidA), Bind(BetaToken, GuidB));

            var antes = resolver.ResolveWith(
                authored, Registry(Target(GuidA, 33.0), Target(GuidB, 77.0)), descriptors);

            var despues = resolver.ResolveWith(
                authored, Registry(Target(GuidA, 99.0), Target(GuidB, 77.0)), descriptors);

            Assert.NotEqual(antes.Design.FloorBeamRise, despues.Design.FloorBeamRise);
            Assert.Equal(99.0, despues.Design.FloorBeamRise);

            // La independencia es en los DOS sentidos: mover ALPHA no mueve BETA.
            Assert.Equal(antes.Design.PalletDepth, despues.Design.PalletDepth);
        }

        [Fact]
        public void DOS_PROPIEDADES_A_LA_MISMA_VARIABLE_TOMAN_LAS_DOS_SU_VALOR()
        {
            // N propiedades de un rack pueden estar gobernadas por UNA variable. Nada en el modelo lo prohibe.
            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Bind(AlphaToken, GuidA), Bind(BetaToken, GuidA)),
                Registry(Target(GuidA, 55.0)),
                SetOf(Alpha(), Beta()));

            Assert.True(resolution.IsSuccess);
            Assert.Equal(55.0, resolution.Design.FloorBeamRise);
            Assert.Equal(55.0, resolution.Design.PalletDepth);
        }

        // ================================================================ fail-closed, y sin escritura parcial

        [Fact]
        public void UN_FALLO_EN_UNA_PROPIEDAD_NO_DEJA_LA_OTRA_ESCRITA()
        {
            // El aborto ocurre antes de escribir nada. Si el resolver escribiera a medida que escanea, ALPHA
            // ya estaria aplicada cuando BETA falla, y el rack quedaria con un diseno a medio resolver.
            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Bind(AlphaToken, GuidA), Bind(BetaToken, GuidC)),
                Registry(Target(GuidA, 33.0)),
                SetOf(Alpha(), Beta()));

            Assert.False(resolution.IsSuccess);
            Assert.Null(resolution.Design);
            Assert.Equal(SelectiveEffectiveOutcome.BrokenProjectVariableReference, resolution.Outcome);
            Assert.Equal(BetaToken, resolution.PropertyId.Value);
        }

        [Fact]
        public void UN_TARGET_AUSENTE_NO_DEGRADA_AL_LITERAL_ALMACENADO()
        {
            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Bind(AlphaToken, GuidC)), Registry(Target(GuidA, 33.0)), SetOf(Alpha(), Beta()));

            Assert.False(resolution.IsSuccess);
            Assert.Null(resolution.Design);
            Assert.Equal(GuidC, resolution.VariableId);
        }

        [Fact]
        public void UN_TIPO_INCOMPATIBLE_ABORTA_AUNQUE_LA_VARIABLE_EXISTA()
        {
            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Bind(AlphaToken, GuidA)),
                Registry(Target(GuidA, 33.0, TipoSintetico)),
                SetOf(Alpha(), Beta()));

            Assert.False(resolution.IsSuccess);
            Assert.Equal(SelectiveEffectiveOutcome.BrokenProjectVariableReference, resolution.Outcome);
            Assert.Contains(VariableType.Length.ToString(), resolution.Error);
        }

        [Fact]
        public void UNA_PROPIEDAD_DESCONOCIDA_ABORTA_COMO_UNKNOWNPROPERTYID()
        {
            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Bind("selective.noExiste", GuidA)),
                Registry(Target(GuidA, 33.0)),
                SetOf(Alpha(), Beta()));

            Assert.False(resolution.IsSuccess);
            Assert.Equal(SelectiveEffectiveOutcome.UnknownPropertyId, resolution.Outcome);
        }

        [Fact]
        public void UN_KIND_DESCONOCIDO_ABORTA_COMO_UNKNOWNREFERENCEKIND()
        {
            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Raw(AlphaToken, new SelectivePropertyValueDocument { Kind = "rackProperty", VariableId = GuidA })),
                Registry(Target(GuidA, 33.0)),
                SetOf(Alpha(), Beta()));

            Assert.False(resolution.IsSuccess);
            Assert.Equal(SelectiveEffectiveOutcome.UnknownReferenceKind, resolution.Outcome);
        }

        [Fact]
        public void UN_VARIABLEID_ILEGIBLE_ABORTA_COMO_MALFORMEDREFERENCE()
        {
            // Las dos formas malformadas comparten disposicion pero NO vocabulario publicado: distinguirlas no
            // se puede inferir de si habia o no un id.
            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Raw(AlphaToken, new SelectivePropertyValueDocument
                {
                    Kind = SelectivePropertyValueDocument.ProjectVariableKind,
                    VariableId = "no-soy-un-guid",
                })),
                Registry(Target(GuidA, 33.0)),
                SetOf(Alpha(), Beta()));

            Assert.False(resolution.IsSuccess);
            Assert.Equal(SelectiveEffectiveOutcome.MalformedReference, resolution.Outcome);
            Assert.Equal("no-soy-un-guid", resolution.VariableId);
        }

        // ================================================================ una sola autoridad de VariableId

        [Fact]
        public void UN_REGISTRO_CON_VARIABLEID_DUPLICADO_NO_RESUELVE_NINGUN_RACK()
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = GuidA,
                    Name = "Primera",
                    Type = "Length",
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 11.0 },
                },
                new ProjectVariableDocument
                {
                    VariableId = GuidA,
                    Name = "Segunda",
                    Type = "Length",
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 22.0 },
                },
            };

            var resolution = new SelectiveEffectiveDesignResolver().ResolveAccredited(
                Doc(Bind(ClearanceToken, GuidA)), ProjectVariablesReadResult.Readable(document));

            // Ni 11 ni 22: no se elige ninguna. Un VariableId que designa dos entradas no es una identidad.
            Assert.False(resolution.IsSuccess);
            Assert.Null(resolution.Design);
            Assert.Contains(GuidA, resolution.Error);
        }

        [Fact]
        public void EL_NOMBRE_QUE_SE_MUESTRA_Y_EL_VALOR_QUE_SE_RESUELVE_SALEN_DEL_MISMO_TARGET()
        {
            // La misma autoridad para las dos preguntas. Antes, el nombre lo daba un recorrido propio del
            // documento y el valor lo daba el resolver: con homonimos podian discrepar.
            var registry = Registry(Target(GuidA, 33.0));

            Assert.True(registry.TryGetTarget(Id(GuidA), out var target));

            var resolution = new SelectiveEffectiveDesignResolver().ResolveWith(
                Doc(Bind(AlphaToken, GuidA)), registry, SetOf(Alpha(), Beta()));

            Assert.Equal("V", target.Name);
            Assert.Equal(target.LiteralValue, resolution.Design.FloorBeamRise);
        }

        [Fact]
        public void LA_ENUMERACION_DE_TARGETS_ES_LA_MISMA_AUTORIDAD_QUE_EL_LOOKUP()
        {
            var registry = Registry(Target(GuidB, 22.0), Target(GuidA, 11.0));
            var targets = registry.Targets();

            Assert.Equal(2, targets.Count);
            Assert.Equal(Id(GuidA), targets[0].VariableId);
            Assert.Equal(Id(GuidB), targets[1].VariableId);

            foreach (var target in targets)
            {
                Assert.True(registry.TryGetTarget(target.VariableId, out var found));
                Assert.Same(target, found);
            }
        }

        // ================================================================ produccion: las dos propiedades

        /// <summary>
        /// El catalogo productivo, enumerado en orden determinista. Hasta G4E exigia UNA sola propiedad —la
        /// capability crecia en el gate del proof y no antes—; ahora exige exactamente las dos decididas, en el
        /// orden Ordinal que hace reproducible cualquier diagnostico construido sobre el.
        /// </summary>
        [Fact]
        public void EL_CATALOGO_PRODUCTIVO_DECLARA_LAS_DOS_PROPIEDADES_EN_ORDEN_DETERMINISTA()
        {
            var ordered = SelectiveLinkedProperties.All.Ordered();

            Assert.Equal(2, ordered.Count);
            Assert.Equal(
                new[] { "selective.palletTolerance", ClearanceToken },
                ordered.Select(d => d.PropertyId.Value).ToArray());
        }

        [Fact]
        public void EL_DESCRIPTOR_PRODUCTIVO_LEE_Y_ESCRIBE_EL_CAMPO_DE_LA_HOLGURA()
        {
            var descriptor = Clearance();
            var authored = Doc();
            var design = Diseno();

            Assert.Equal(6.0, descriptor.ReadAuthored(authored));
            Assert.Equal(6.0, descriptor.ReadEffective(design));

            descriptor.WriteAuthored(authored, 13.0);
            descriptor.WriteEffective(design, 17.0);

            // Los oraculos son los campos concretos, no el propio descriptor.
            Assert.Equal(13.0, authored.VerticalClearance);
            Assert.Equal(17.0, design.VerticalClearance);
            Assert.Equal(4.0, authored.FloorBeamRise);
            Assert.Equal(48.0, design.PalletDepth);
        }
    }
}
