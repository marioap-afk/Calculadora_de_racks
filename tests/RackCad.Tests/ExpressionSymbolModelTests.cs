using System;
using System.Linq;
using System.Reflection;
using RackCad.Application.Expressions;
using RackCad.Application.Units;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — modelo neutral de símbolos: <c>SymbolId</c>, namespaces, ámbitos, entradas, tabla y contexto (V6 P3, P5,
    /// P6 y P25; ADR-0040 D1, D5).
    ///
    /// <para>
    /// El núcleo no nombra ningún tipo de Project Variables: la relación <c>SymbolId → VariableType</c> vive en el
    /// adaptador de G8 (P8.9). Aquí solo existe un namespace activo, <c>projectVariable</c>, y el ámbito <c>Rack</c> es
    /// una reserva que solo usan entradas sintéticas de prueba (P25.4).
    /// </para>
    /// </summary>
    public class ExpressionSymbolModelTests
    {
        private static SymbolDefinition Uno => SymbolDefinition.FromLiteral(1);

        // ================================================================ namespaces e identidad (P3.2, P5.2)

        [Fact]
        public void EL_UNICO_NAMESPACE_ACTIVO_ES_PROJECTVARIABLE_CON_TOKEN_ORDINAL()
        {
            Assert.Equal(new[] { SymbolNamespace.ProjectVariable }, SymbolNamespaces.All);
            Assert.Equal(new[] { 1 }, Enum.GetValues<SymbolNamespace>().Select(value => (int)value));

            Assert.Equal("projectVariable", SymbolNamespaces.Token(SymbolNamespace.ProjectVariable));
            Assert.True(SymbolNamespaces.TryParseToken("projectVariable", out var parsed));
            Assert.Equal(SymbolNamespace.ProjectVariable, parsed);

            // rack y project quedan reservados conceptualmente para ID20: sin token, sin registro y sin resolución.
            foreach (var token in new[] { "ProjectVariable", "projectvariable", "rack", "Rack", "project", "Project", "", " projectVariable" })
            {
                Assert.False(SymbolNamespaces.TryParseToken(token, out _), "«" + token + "» no es un namespace activo");
            }

            Assert.False(SymbolNamespaces.TryParseToken(null, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => SymbolNamespaces.Token((SymbolNamespace)2));
        }

        [Fact]
        public void LA_CLAVE_DE_PROJECTVARIABLE_COMPARA_ORDINALIGNORECASE_Y_SE_CONSERVA_TAL_COMO_SE_ESCRIBIO()
        {
            var minusculas = SymbolId.ProjectVariable(Guid1);
            var mayusculas = SymbolId.ProjectVariable(Guid1.ToUpperInvariant());

            Assert.Equal(minusculas, mayusculas);
            Assert.True(minusculas == mayusculas);
            Assert.False(minusculas != mayusculas);
            Assert.Equal(minusculas.GetHashCode(), mayusculas.GetHashCode());
            Assert.Equal(0, minusculas.CompareTo(mayusculas));

            Assert.Equal(SymbolNamespace.ProjectVariable, mayusculas.Namespace);
            Assert.Equal(Guid1.ToUpperInvariant(), mayusculas.Key);

            Assert.NotEqual(minusculas, SymbolId.ProjectVariable(Guid2));
            Assert.Same(StringComparer.OrdinalIgnoreCase, SymbolNamespaces.KeyComparer(SymbolNamespace.ProjectVariable));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("3f2b1c9e")]
        [InlineData("{3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44}")]
        [InlineData("3f2b1c9e6d4a4f389b710c2a5e8d1f44")]
        [InlineData(" 3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44")]
        [InlineData("3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44 ")]
        [InlineData("3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f4g")]
        [InlineData("3f2b1c9e_6d4a_4f38_9b71_0c2a5e8d1f44")]
        public void LA_CLAVE_DE_PROJECTVARIABLE_TIENE_QUE_SER_UN_GUID_COMPLETO_EN_FORMA_D(string key)
        {
            Assert.ThrowsAny<ArgumentException>(() => SymbolId.ProjectVariable(key));
            Assert.ThrowsAny<ArgumentException>(() => new SymbolId(SymbolNamespace.ProjectVariable, key));
        }

        [Fact]
        public void UN_NAMESPACE_NO_REGISTRADO_NO_CONSTRUYE_IDENTIDADES()
        {
            Assert.ThrowsAny<ArgumentException>(() => new SymbolId((SymbolNamespace)2, Guid1));
            Assert.ThrowsAny<ArgumentException>(() => new SymbolId((SymbolNamespace)0, Guid1));
        }

        /// <summary>P11.1: namespace en <c>Ordinal</c> y después la clave con el comparador de su namespace.</summary>
        [Fact]
        public void LOS_SYMBOLID_SE_ORDENAN_DE_FORMA_DETERMINISTA()
        {
            var ids = new[] { Key(0xa), Key(2), Key(0xB), Key(1) }.Select(SymbolId.ProjectVariable).ToList();

            ids.Sort();

            Assert.Equal(new[] { Key(1), Key(2), Key(0xa), Key(0xB) }, ids.Select(id => id.Key));
        }

        // ================================================================ ámbitos y entradas (P5.1, P5.3)

        [Fact]
        public void LOS_AMBITOS_SON_PROJECT_Y_RACK_RESERVADO()
        {
            Assert.Equal(new[] { 1, 2 }, Enum.GetValues<SymbolScope>().Select(value => (int)value));
            Assert.Equal(1, (int)SymbolScope.Project);
            Assert.Equal(2, (int)SymbolScope.Rack);
        }

        [Fact]
        public void UNA_ENTRADA_NO_LLEVA_VARIABLETYPE_NI_TIPOS_DE_PROJECT_VARIABLES()
        {
            var propiedades = typeof(SymbolEntry).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Assert.Equal(
                new[] { "Definition", "DisplayName", "Id", "Scope" },
                propiedades.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));

            Assert.DoesNotContain(
                propiedades,
                property => (property.PropertyType.Namespace ?? string.Empty).Contains("ProjectVariables", StringComparison.Ordinal));
        }

        [Fact]
        public void UNA_ENTRADA_EXIGE_ID_NOMBRE_DEFINICION_Y_UN_AMBITO_DECLARADO()
        {
            Assert.ThrowsAny<ArgumentException>(() => new SymbolEntry(null, SymbolScope.Project, "A", Uno));
            Assert.ThrowsAny<ArgumentException>(() => new SymbolEntry(Id(1), SymbolScope.Project, null, Uno));
            Assert.ThrowsAny<ArgumentException>(() => new SymbolEntry(Id(1), SymbolScope.Project, "A", null));
            Assert.ThrowsAny<ArgumentException>(() => new SymbolEntry(Id(1), (SymbolScope)0, "A", Uno));
            Assert.ThrowsAny<ArgumentException>(() => new SymbolEntry(Id(1), (SymbolScope)3, "A", Uno));

            // Que un nombre no esté en blanco es una regla de Project Variables, no del núcleo: la sintaxis de §4
            // representa también el nombre vacío ({}).
            Assert.Equal(string.Empty, new SymbolEntry(Id(1), SymbolScope.Project, string.Empty, Uno).DisplayName);
        }

        [Fact]
        public void LA_DEFINICION_ES_LITERAL_O_EXPRESION_Y_LANZA_FUERA_DE_SU_CASO()
        {
            var literal = SymbolDefinition.FromLiteral(6);
            Assert.Equal(SymbolDefinitionKind.Literal, literal.Kind);
            Assert.Equal(6, literal.LiteralValue);
            Assert.Throws<InvalidOperationException>(() => literal.Expression);

            var arbol = BoundExpression.Number(4, LengthUnit.Inch);
            var expresion = SymbolDefinition.FromExpression(arbol);
            Assert.Equal(SymbolDefinitionKind.Expression, expresion.Kind);
            Assert.Same(arbol, expresion.Expression);
            Assert.Throws<InvalidOperationException>(() => expresion.LiteralValue);

            Assert.ThrowsAny<ArgumentException>(() => SymbolDefinition.FromLiteral(double.NaN));
            Assert.ThrowsAny<ArgumentException>(() => SymbolDefinition.FromLiteral(double.PositiveInfinity));
            Assert.ThrowsAny<ArgumentException>(() => SymbolDefinition.FromExpression(null));
            Assert.Equal(new[] { 1, 2 }, Enum.GetValues<SymbolDefinitionKind>().Select(value => (int)value));
        }

        // ================================================================ tabla (P3.6, P7.2)

        [Fact]
        public void LA_TABLA_RECHAZA_UN_ID_REPETIDO_TAMBIEN_CON_OTRA_GRAFIA()
        {
            Assert.ThrowsAny<ArgumentException>(() => Table(
                new SymbolEntry(SymbolId.ProjectVariable(Guid1), SymbolScope.Project, "A", Uno),
                new SymbolEntry(SymbolId.ProjectVariable(Guid1.ToUpperInvariant()), SymbolScope.Project, "B", Uno)));

            Assert.ThrowsAny<ArgumentException>(() => SymbolTable.Create(null));
            Assert.ThrowsAny<ArgumentException>(() => SymbolTable.Create(new SymbolEntry[] { null }));
        }

        [Fact]
        public void LA_TABLA_BUSCA_POR_NOMBRE_EXACTO_ORDINALIGNORECASE_SIN_RECORTES_NI_NORMALIZACION()
        {
            var precompuesto = "Caf" + (char)0x00E9;
            var descompuesto = "Cafe" + (char)0x0301;

            var table = Table(
                Variable(4, "holgura"),
                Variable(2, "Holgura General"),
                Variable(3, precompuesto),
                Variable(1, "Holgura"));

            Assert.Equal(new[] { Id(1), Id(4) }, table.FindByDisplayName("HOLGURA").Select(entry => entry.Id));
            Assert.Single(table.FindByDisplayName("holgura general"));
            Assert.Single(table.FindByDisplayName(precompuesto));

            Assert.Empty(table.FindByDisplayName("Holgura "));
            Assert.Empty(table.FindByDisplayName(" Holgura"));
            Assert.Empty(table.FindByDisplayName("Holg"));
            Assert.Empty(table.FindByDisplayName("Holgura  General"));
            Assert.Empty(table.FindByDisplayName(descompuesto));
            Assert.Empty(table.FindByDisplayName(null));

            Assert.True(table.TryGet(SymbolId.ProjectVariable(Key(2).ToUpperInvariant()), out var entry));
            Assert.Equal("Holgura General", entry.DisplayName);
            Assert.False(table.TryGet(Id(99), out _));
            Assert.False(table.TryGet(null, out _));
        }

        [Fact]
        public void LA_TABLA_EXPONE_SUS_ENTRADAS_EN_ORDEN_DETERMINISTA()
        {
            var a = Variable(3, "C");
            var b = Variable(1, "A");
            var c = Variable(2, "B");

            Assert.Equal(new[] { Id(1), Id(2), Id(3) }, Table(a, b, c).Entries.Select(entry => entry.Id));
            Assert.Equal(new[] { Id(1), Id(2), Id(3) }, Table(c, a, b).Entries.Select(entry => entry.Id));
            Assert.Empty(SymbolTable.Empty.Entries);
        }

        // ================================================================ contexto (P6)

        [Fact]
        public void EL_CONTEXTO_LLEVA_LA_TABLA_Y_LAS_AUTORIDADES_UNICAS()
        {
            var table = Table(Variable(1, "A"));
            var context = ExpressionContext.Create(table);

            Assert.Same(table, context.Symbols);
            Assert.Same(FunctionRegistry.Productive, context.Functions);
            Assert.Same(LengthUnits.Authority, context.Units);
            Assert.Same(ExpressionLimits.Normative, context.Limits);

            Assert.Same(FunctionRegistry.Productive, ExpressionContext.Create(SymbolTable.Empty).Functions);
            Assert.ThrowsAny<ArgumentException>(() => ExpressionContext.Create(null));
        }

        /// <summary>
        /// P6.4 y P6.5: la construcción desde entradas sintéticas es una costura del núcleo y de las pruebas, no alcanzable
        /// desde Plugin ni UI. Tampoco hay una segunda instancia de las autoridades únicas.
        /// </summary>
        [Fact]
        public void LAS_COSTURAS_DE_CONSTRUCCION_NO_SON_PUBLICAS()
        {
            foreach (var type in new[] { typeof(SymbolTable), typeof(ExpressionContext), typeof(FunctionRegistry), typeof(LengthUnits), typeof(ExpressionLimits) })
            {
                Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
            }

            Assert.DoesNotContain(
                typeof(SymbolTable).GetMethods(BindingFlags.Public | BindingFlags.Static),
                method => !method.IsSpecialName && method.ReturnType == typeof(SymbolTable));

            Assert.DoesNotContain(
                typeof(ExpressionContext).GetMethods(BindingFlags.Public | BindingFlags.Static),
                method => !method.IsSpecialName && method.ReturnType == typeof(ExpressionContext));
        }
    }
}
