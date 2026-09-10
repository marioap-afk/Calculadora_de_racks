using System;
using RackCad.Application.ProjectVariables;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G1 — el modelo PURO de variables de proyecto (ID22A), sin persistencia, sin AutoCAD y sin
    /// tocar el Selectivo.
    ///
    /// <para>
    /// Lo que estas pruebas fijan no es "que los tipos existan": es la semantica que el resto de la
    /// iniciativa da por supuesta. Tres invariantes gobiernan el conjunto y ninguna es evidente desde la
    /// firma de un metodo:
    /// </para>
    /// <list type="number">
    /// <item><b>La identidad es el <c>VariableId</c>, nunca el <c>Name</c></b> (ADR-0034 / D-02). Renombrar
    /// es una operacion sin consecuencias para nadie, y eso solo es cierto si ningun camino resuelve por
    /// nombre.</item>
    /// <item><b>La asimetria de comparacion es deliberada</b> (C4-14): <c>VariableId</c> se compara
    /// <c>OrdinalIgnoreCase</c> porque es un GUID cuya caja hexadecimal varia entre escritores;
    /// <c>PropertyId</c> se compara <c>Ordinal</c> porque es un token AUTORADO en el codigo, donde una
    /// diferencia de caja significa que alguien escribio otra cosa.</item>
    /// <item><b>Un estado desconocido no es un literal</b> (doctrina C4.7-1). Una referencia jamas se
    /// comporta como un valor: preguntar por el literal de una referencia FALLA en vez de devolver
    /// <c>default(T)</c>, que es como un <c>0.0</c> se colaria en la geometria sin que nadie lo note.</item>
    /// </list>
    /// </summary>
    public class ProjectVariablesModelTests
    {
        private static VariableDefinition SeisPulgadas() => VariableDefinition.Literal(6.0);

        private static ProjectVariable Holgura(VariableId id, string name = "Holgura vertical")
            => ProjectVariable.Create(id, name, VariableType.Length, SeisPulgadas());

        // ---------------------------------------------------------------- identidad

        [Fact]
        public void RenombrarConservaExactamenteElVariableId()
        {
            var id = VariableId.New();
            var original = Holgura(id, "Holgura vertical");

            var renombrada = original.WithName("Holgura estandar");

            Assert.Equal(id, renombrada.Id);
            Assert.Equal("Holgura estandar", renombrada.Name);
            Assert.Equal("Holgura vertical", original.Name);
        }

        [Fact]
        public void DosVariablesConElMismoNombreTienenIdentidadesDISTINTAS()
        {
            var a = Holgura(VariableId.New(), "Holgura");
            var b = Holgura(VariableId.New(), "Holgura");

            Assert.NotEqual(a.Id, b.Id);
        }

        [Fact]
        public void CadaVariableIdNuevoEsUnico()
        {
            Assert.NotEqual(VariableId.New(), VariableId.New());
        }

        [Fact]
        public void UnVariableIdSeCompara_OrdinalIgnoreCase()
        {
            var texto = Guid.NewGuid().ToString();

            var minusculas = VariableId.Parse(texto.ToLowerInvariant());
            var mayusculas = VariableId.Parse(texto.ToUpperInvariant());

            Assert.Equal(minusculas, mayusculas);
            Assert.True(minusculas == mayusculas);
            Assert.Equal(minusculas.GetHashCode(), mayusculas.GetHashCode());
        }

        [Fact]
        public void UnVariableIdConservaElTextoTalComoSeEscribio()
        {
            var texto = Guid.NewGuid().ToString().ToUpperInvariant();

            Assert.Equal(texto, VariableId.Parse(texto).Value);
        }

        // ---------------------------------------------------------------- PropertyId

        [Fact]
        public void UnPropertyIdSeCompara_Ordinal_YLaCajaSIImporta()
        {
            var canonico = PropertyId.Parse("selective.verticalClearance");
            var otraCaja = PropertyId.Parse("selective.verticalclearance");

            Assert.NotEqual(canonico, otraCaja);
            Assert.True(canonico != otraCaja);
        }

        [Fact]
        public void ElUnicoPropertyIdDeID22A_EsElTokenContractual()
        {
            Assert.Equal("selective.verticalClearance", ProjectPropertyIds.SelectiveVerticalClearance.Value);
            Assert.True(ProjectPropertyIds.IsKnown(ProjectPropertyIds.SelectiveVerticalClearance));
        }

        [Fact]
        public void UnPropertyIdDesconocidoNoSeReconoce()
        {
            Assert.False(ProjectPropertyIds.IsKnown(PropertyId.Parse("selective.palletDepth")));
            Assert.False(ProjectPropertyIds.IsKnown(PropertyId.Parse("selective.verticalclearance")));
        }

        // ---------------------------------------------------------------- tipo y definicion

        [Fact]
        public void UnaVariableDeLongitudDeclaraElTipoLength()
        {
            Assert.Equal(VariableType.Length, Holgura(VariableId.New()).Type);
        }

        [Fact]
        public void LaDefinicionLiteralConservaSuValorTipado()
        {
            var definicion = VariableDefinition.Literal(6.0);

            Assert.Equal(VariableDefinitionKind.Literal, definicion.Kind);
            Assert.Equal(6.0, definicion.LiteralValue);
        }

        [Fact]
        public void DosDefinicionesLiteralesIgualesSonIguales()
        {
            Assert.Equal(VariableDefinition.Literal(6.0), VariableDefinition.Literal(6.0));
            Assert.NotEqual(VariableDefinition.Literal(6.0), VariableDefinition.Literal(7.0));
        }

        // ---------------------------------------------------------------- PropertyValue

        [Fact]
        public void UnLiteralSeDistingueInequivocamenteDeUnaReferencia()
        {
            var literal = PropertyValue<double>.Literal(6.0);
            var referencia = PropertyValue<double>.Reference(VariableId.New());

            Assert.Equal(PropertyValueKind.Literal, literal.Kind);
            Assert.True(literal.IsLiteral);
            Assert.False(literal.IsProjectVariableReference);

            Assert.Equal(PropertyValueKind.ProjectVariableReference, referencia.Kind);
            Assert.False(referencia.IsLiteral);
            Assert.True(referencia.IsProjectVariableReference);
        }

        [Fact]
        public void UnaReferenciaConservaExactamenteSuVariableId()
        {
            var id = VariableId.New();

            Assert.Equal(id, PropertyValue<double>.Reference(id).VariableId);
        }

        [Fact]
        public void PedirElLiteralDeUnaReferencia_FALLA_NoDevuelveDefault()
        {
            var referencia = PropertyValue<double>.Reference(VariableId.New());

            Assert.Throws<InvalidOperationException>(() => referencia.LiteralValue);
        }

        [Fact]
        public void PedirElVariableIdDeUnLiteral_FALLA()
        {
            var literal = PropertyValue<double>.Literal(6.0);

            Assert.Throws<InvalidOperationException>(() => literal.VariableId);
        }

        [Fact]
        public void DosPropertyValueIgualesSonIguales_YUnLiteralNuncaIgualaAUnaReferencia()
        {
            var id = VariableId.New();

            Assert.Equal(PropertyValue<double>.Literal(6.0), PropertyValue<double>.Literal(6.0));
            Assert.Equal(PropertyValue<double>.Reference(id), PropertyValue<double>.Reference(id));
            Assert.NotEqual(PropertyValue<double>.Literal(6.0), PropertyValue<double>.Reference(id));
        }

        // ---------------------------------------------------------------- estados invalidos

        [Fact]
        public void UnVariableIdPorDefectoEstaVacioYNoEsUnaIdentidadValida()
        {
            Assert.True(default(VariableId).IsEmpty);
            Assert.Equal(string.Empty, default(VariableId).Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("no-es-un-guid")]
        [InlineData("9f3c")]
        public void UnVariableIdQueNoEsGuidNoSePuedeConstruir(string texto)
        {
            Assert.False(VariableId.TryParse(texto, out _));
            Assert.Throws<ArgumentException>(() => VariableId.Parse(texto));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UnPropertyIdVacioNoSePuedeConstruir(string texto)
        {
            Assert.False(PropertyId.TryParse(texto, out _));
            Assert.Throws<ArgumentException>(() => PropertyId.Parse(texto));
        }

        [Fact]
        public void UnaVariableSinIdentidadSeRechaza()
        {
            Assert.Throws<ArgumentException>(
                () => ProjectVariable.Create(default, "Holgura", VariableType.Length, SeisPulgadas()));
        }

        [Fact]
        public void UnaVariableConTipoNoDeclaradoSeRechaza()
        {
            Assert.Throws<ArgumentException>(
                () => ProjectVariable.Create(VariableId.New(), "Holgura", (VariableType)0, SeisPulgadas()));
        }

        [Fact]
        public void UnaVariableSinDefinicionSeRechaza()
        {
            Assert.Throws<ArgumentNullException>(
                () => ProjectVariable.Create(VariableId.New(), "Holgura", VariableType.Length, null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UnaVariableSinNombreSeRechaza(string nombre)
        {
            Assert.Throws<ArgumentException>(
                () => ProjectVariable.Create(VariableId.New(), nombre, VariableType.Length, SeisPulgadas()));
        }

        [Fact]
        public void RenombrarAVacioSeRechaza()
        {
            var variable = Holgura(VariableId.New());

            Assert.Throws<ArgumentException>(() => variable.WithName("  "));
        }

        [Fact]
        public void UnaReferenciaAUnVariableIdVacioSeRechaza()
        {
            Assert.Throws<ArgumentException>(() => PropertyValue<double>.Reference(default));
        }

        [Fact]
        public void UnaDefinicionLiteralNoFinitaSeRechaza()
        {
            Assert.Throws<ArgumentException>(() => VariableDefinition.Literal(double.NaN));
            Assert.Throws<ArgumentException>(() => VariableDefinition.Literal(double.PositiveInfinity));
        }
    }
}
