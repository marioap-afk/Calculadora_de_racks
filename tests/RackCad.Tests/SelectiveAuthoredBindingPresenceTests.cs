using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 G3, correccion focal — <b>PRESENTE PERO ININTERPRETABLE no es AUSENTE</b>.
    ///
    /// <para>
    /// El portador authored decidia si congelar el literal preguntando <c>IsBound</c>, que responde
    /// «puedo interpretar esta referencia». Son dos preguntas distintas, y confundirlas colapsa
    /// <c>PRESENT_BUT_UNINTERPRETABLE</c> en <c>UNBOUND</c>: una entrada con un <c>kind</c> del futuro o un
    /// id ilegible se comportaba como si no hubiera binding, y el guardado <b>sobrescribia el literal
    /// authored</b> — justo el valor congelado que la reparacion de una referencia rota necesita despues.
    /// El resultado seria una perdida silenciosa de la intencion del usuario en el unico caso en que ya
    /// habia un problema.
    /// </para>
    /// <para>
    /// El reparto correcto: el portador mira <b>presencia de la clave</b> y nada mas; quien decide que
    /// significa una referencia que no se entiende es la capa semantica —el resolver ya devuelve
    /// <c>UnknownReferenceKind</c> y <c>MalformedReference</c> como errores tipados, y el probe hara lo
    /// propio—. Persistir bien y resolver bien son responsabilidades separadas.
    /// </para>
    /// </summary>
    public class SelectiveAuthoredBindingPresenceTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static SelectivePalletDesign Diseno(double clearance)
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
            return design;
        }

        private static SelectivePalletDesignDocument ConEntrada(SelectivePropertyValueDocument entrada)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(6.0), RackId, "Rack 1");
            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument> { [Token] = entrada };
            return doc;
        }

        // ================================================================ 1 — kind desconocido

        [Fact]
        public void KindDESCONOCIDO_ElLiteralAuthoredSIGUE_CONGELADO()
        {
            var authored = ConEntrada(new SelectivePropertyValueDocument { Kind = "futureKind", VariableId = VarId });

            var guardado = authored.WithDesign(Diseno(10.0));

            Assert.Equal(6.0, guardado.VerticalClearance);
        }

        [Fact]
        public void KindDESCONOCIDO_ElBindingPERMANECE_INTACTO()
        {
            var authored = ConEntrada(new SelectivePropertyValueDocument { Kind = "futureKind", VariableId = VarId });

            var guardado = authored.WithDesign(Diseno(10.0));

            var entrada = Assert.Single(guardado.PropertyValues);
            Assert.Equal(Token, entrada.Key);
            Assert.Equal("futureKind", entrada.Value.Kind);
            Assert.Equal(VarId, entrada.Value.VariableId);
        }

        // ================================================================ 2 — VariableId malformado

        [Fact]
        public void VariableIdMALFORMADO_ElLiteralAuthoredSIGUE_CONGELADO()
        {
            var authored = ConEntrada(SelectivePropertyValueDocument.ToProjectVariable("no-es-un-guid"));

            Assert.Equal(6.0, authored.WithDesign(Diseno(10.0)).VerticalClearance);
        }

        [Fact]
        public void VariableIdVACIO_ElLiteralAuthoredSIGUE_CONGELADO()
        {
            var authored = ConEntrada(SelectivePropertyValueDocument.ToProjectVariable(null));

            Assert.Equal(6.0, authored.WithDesign(Diseno(10.0)).VerticalClearance);
        }

        // ================================================================ 3 — valor nulo

        /// <summary>
        /// Precondicion demostrada, no supuesta: el store NO rechaza un valor nulo dentro de
        /// <c>PropertyValues</c>, asi que esa forma llega viva hasta el portador y hay que decidirla aqui.
        /// </summary>
        [Fact]
        public void ElStoreNO_RECHAZA_UnValorNuloEnPropertyValues()
        {
            var store = new SelectivePalletDesignStore();
            var authored = ConEntrada(null);

            var releido = store.Deserialize(store.Serialize(authored));

            Assert.True(releido.PropertyValues.ContainsKey(Token));
            Assert.Null(releido.PropertyValues[Token]);
        }

        [Fact]
        public void ValorNULO_SIGUE_SIENDO_PRESENCIA_DE_UN_BINDING_DESCONOCIDO()
        {
            var authored = ConEntrada(null);

            Assert.Equal(6.0, authored.WithDesign(Diseno(10.0)).VerticalClearance);
        }

        // ================================================================ 4 — centinela POSITIVO

        [Fact]
        public void SinLaCLAVE_ElNuevoLiteralSI_SE_ADOPTA()
        {
            var authored = SelectivePalletDesignDocument.From(Diseno(6.0), RackId, "Rack 1");

            Assert.Equal(10.0, authored.WithDesign(Diseno(10.0)).VerticalClearance);
        }

        /// <summary>
        /// Una clave presente pero de OTRA propiedad tampoco congela esta: la comparacion es exacta, no
        /// «hay algo en PropertyValues».
        /// </summary>
        [Fact]
        public void UnaCLAVE_DE_OTRA_PROPIEDAD_NoCongelaEstaPropiedad()
        {
            var authored = SelectivePalletDesignDocument.From(Diseno(6.0), RackId, "Rack 1");
            authored.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                ["selective.palletDepth"] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
            };

            Assert.Equal(10.0, authored.WithDesign(Diseno(10.0)).VerticalClearance);
        }

        [Fact]
        public void LaComparacionDeLaCLAVE_es_ORDINAL()
        {
            var authored = SelectivePalletDesignDocument.From(Diseno(6.0), RackId, "Rack 1");
            authored.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                ["selective.verticalclearance"] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
            };

            Assert.False(authored.HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance));
            Assert.Equal(10.0, authored.WithDesign(Diseno(10.0)).VerticalClearance);
        }

        // ================================================================ las dos preguntas, separadas

        [Theory]
        [InlineData("futureKind", VarId)]
        [InlineData(SelectivePropertyValueDocument.ProjectVariableKind, "no-es-un-guid")]
        public void PRESENTE_PERO_ININTERPRETABLE_NO_ES_AUSENTE(string kind, string variableId)
        {
            var authored = ConEntrada(new SelectivePropertyValueDocument { Kind = kind, VariableId = variableId });

            // hay entrada...
            Assert.True(authored.HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance));

            // ...pero esta version NO puede interpretarla. Son dos respuestas distintas, no una.
            Assert.False(authored.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out _));
        }

        [Fact]
        public void UNA_REFERENCIA_VALIDA_RESPONDE_QUE_SI_A_LAS_DOS()
        {
            var authored = ConEntrada(SelectivePropertyValueDocument.ToProjectVariable(VarId));

            Assert.True(authored.HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance));
            Assert.True(authored.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out var id));
            Assert.Equal(VariableId.Parse(VarId), id);
        }

        [Fact]
        public void SIN_ENTRADA_RESPONDE_QUE_NO_A_LAS_DOS()
        {
            var authored = SelectivePalletDesignDocument.From(Diseno(6.0), RackId, "Rack 1");

            Assert.False(authored.HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance));
            Assert.False(authored.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out _));
        }

        /// <summary>
        /// Y la capa semantica sigue decidiendo lo que el portador no decide: el resolver devuelve un error
        /// TIPADO para las dos formas ininterpretables. Preservar el literal al guardar no las convierte en
        /// validas.
        /// </summary>
        [Theory]
        [InlineData("futureKind", VarId, SelectiveEffectiveOutcome.UnknownReferenceKind)]
        [InlineData(SelectivePropertyValueDocument.ProjectVariableKind, "no-es-un-guid", SelectiveEffectiveOutcome.MalformedReference)]
        public void ElResolverSIGUE_TRATANDOLAS_COMO_ERROR(string kind, string variableId, SelectiveEffectiveOutcome esperado)
        {
            var authored = ConEntrada(new SelectivePropertyValueDocument { Kind = kind, VariableId = variableId });

            var resultado = new RackCad.Application.Systems.Selective.SelectiveEffectiveDesignResolver()
                .Resolve(authored, ProjectVariablesDocument.CreateNew());

            Assert.Equal(esperado, resultado.Outcome);
            Assert.Null(resultado.Design);
        }
    }
}
