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

        /// <summary>
        /// Amendment A2 §3.2 (ADR-0041 D5): validez NEUTRAL de la clave de <c>projectVariable</c> —no vacía, igual a su
        /// <c>Trim()</c> y aceptada por <c>Guid.TryParse</c>— en cualquier disposición, y la clave se conserva exactamente
        /// como se escribió. Es la costura neutral de la prueba B de A2 §9.3: toda grafía que la regla vigente de
        /// <c>VariableId</c> acepta llega a <c>SymbolId.Key</c> sin transformarse. La conformidad del adaptador es de G8.
        /// </summary>
        public static TheoryData<string> ClavesValidas => new TheoryData<string>
        {
            ClaveD, ClaveDMayusculas, ClaveN, ClaveNMayusculas, ClaveB, ClaveP, ClaveX, ClaveXMayusculas, ClaveXGrupoCorto,
            ClaveXConCeros, ClaveXConEspacios, ClaveXConEspacioDuro, ClaveXConSeparadorDeLinea, ClaveDCompatSigno,
            ClaveDCompatHex, ClaveBCompatHex, ClavePCompatSigno,
        };

        [Theory]
        [MemberData(nameof(ClavesValidas))]
        public void LA_CLAVE_DE_PROJECTVARIABLE_ACEPTA_TODA_GRAFIA_VALIDA_Y_SE_CONSERVA_EXACTA(string key)
        {
            Assert.True(Guid.TryParse(key, out _), "La fila no es una grafía válida: " + key);

            var id = SymbolId.ProjectVariable(key);

            Assert.Equal(SymbolNamespace.ProjectVariable, id.Namespace);
            Assert.Equal(key, id.Key);
            Assert.Equal(key, new SymbolId(SymbolNamespace.ProjectVariable, key).Key);
            Assert.Equal("projectVariable:" + key, id.ToString());
        }

        /// <summary>A2 §3.2: vacía, con espacio exterior, fragmento, texto que no es GUID o carácter inválido.</summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("3f2b1c9e")]
        [InlineData("not-a-guid")]
        [InlineData(" 3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b")]
        [InlineData("3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b ")]
        [InlineData("\t3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b")]
        [InlineData("3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6g")]
        [InlineData("3f2b1c9e_8a4d_4e6f_9b0a_1c2d3e4f5a6b")]
        [InlineData("{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}}")]
        [InlineData("{+0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}")]
        public void UNA_CLAVE_SIN_VALIDEZ_NEUTRAL_NO_CONSTRUYE_UN_SYMBOLID(string key)
        {
            Assert.ThrowsAny<ArgumentException>(() => SymbolId.ProjectVariable(key));
            Assert.ThrowsAny<ArgumentException>(() => new SymbolId(SymbolNamespace.ProjectVariable, key));
        }

        /// <summary>
        /// El espacio exterior que cuenta es el de <c>Trim()</c>, también Unicode: <c>Guid.TryParse</c> lo recortaría y
        /// aceptaría la clave, pero la regla exige que la clave sea igual a su <c>Trim()</c>.
        /// </summary>
        [Fact]
        public void UNA_CLAVE_NULA_O_CON_ESPACIO_EXTERIOR_UNICODE_SE_RECHAZA()
        {
            var espacioDuro = ((char)0x00A0).ToString();
            var separadorDeLinea = ((char)0x2028).ToString();

            Assert.Throws<ArgumentNullException>(() => SymbolId.ProjectVariable(null));

            foreach (var key in new[] { espacioDuro + ClaveD, ClaveN + separadorDeLinea, espacioDuro + ClaveX })
            {
                Assert.True(Guid.TryParse(key, out _));
                Assert.ThrowsAny<ArgumentException>(() => SymbolId.ProjectVariable(key));
            }
        }

        /// <summary>
        /// La premisa de A2 §4, que exige la prueba G de A2 §9.3: fuera de ASCII, <c>Guid.TryParse</c> no admite dígitos,
        /// letras ni signos, solo espacio en blanco y solo dentro de la disposición X; y un espacio en blanco no tiene
        /// mayúsculas. Con ella, <c>OrdinalIgnoreCase</c> sobre claves admisibles equivale a comparar en <c>Ordinal</c> tras
        /// bajar solo A–Z, que es lo que hace <c>Q(clave)</c>. Se recorre cada unidad de código fuera de ASCII.
        /// </summary>
        [Fact]
        public void UNA_CLAVE_ADMISIBLE_SOLO_CONTIENE_ASCII_O_ESPACIO_EN_BLANCO()
        {
            const string restoD = "f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b";
            const string restoN = "f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b";
            const string xAntes = "{0x3f2b1c9e,";
            const string xDespues = "0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}";
            var espaciosEnX = 0;

            for (var code = 0x80; code <= 0xFFFF; code++)
            {
                var unit = ((char)code).ToString();

                Assert.False(Guid.TryParse(unit + restoD, out _), "D admite U+" + code.ToString("X4"));
                Assert.False(Guid.TryParse(unit + restoN, out _), "N admite U+" + code.ToString("X4"));
                Assert.False(Guid.TryParse("{" + unit + restoD + "}", out _), "B admite U+" + code.ToString("X4"));

                foreach (var x in new[] { xAntes + unit + xDespues, "{0x" + unit + "f2b1c9e," + xDespues })
                {
                    if (Guid.TryParse(x, out _))
                    {
                        Assert.True(char.IsWhiteSpace((char)code), "X admite U+" + code.ToString("X4") + " sin ser espacio en blanco");
                        Assert.Equal(unit, unit.ToUpperInvariant());
                        Assert.Equal(unit, unit.ToLowerInvariant());
                        espaciosEnX++;
                    }
                }
            }

            Assert.True(espaciosEnX > 0, "La sonda de X no encontró ningún espacio en blanco: no está mirando.");
        }

        /// <summary>
        /// A2 §3.1 y §4 (prueba C de A2 §9.3): las grafías de un mismo GUID son identidades DISTINTAS, exactamente como con
        /// la igualdad vigente de <c>VariableId</c>; las mayúsculas no distinguen. La tabla admite la familia completa con
        /// un mismo nombre y sigue rechazando el mismo id escrito con otras mayúsculas.
        /// </summary>
        [Fact]
        public void LAS_GRAFIAS_DE_UN_MISMO_GUID_SON_IDENTIDADES_DISTINTAS()
        {
            var ids = ClavesDistintasA2.Select(SymbolId.ProjectVariable).ToList();

            for (var i = 0; i < ids.Count; i++)
            {
                for (var j = 0; j < ids.Count; j++)
                {
                    Assert.True((i == j) == ids[i].Equals(ids[j]), ids[i] + " frente a " + ids[j]);
                }
            }

            Assert.Equal(SymbolId.ProjectVariable(ClaveD), SymbolId.ProjectVariable(ClaveDMayusculas));
            Assert.Equal(SymbolId.ProjectVariable(ClaveN), SymbolId.ProjectVariable(ClaveNMayusculas));
            Assert.Equal(SymbolId.ProjectVariable(ClaveX), SymbolId.ProjectVariable(ClaveXMayusculas));
            Assert.Equal(SymbolId.ProjectVariable(ClaveX).GetHashCode(), SymbolId.ProjectVariable(ClaveXMayusculas).GetHashCode());

            var familia = Table(FamiliaA2.Select((key, index) => VariableKey(key, "Holgura", index)).ToArray());
            Assert.Equal(6, familia.Entries.Count);
            Assert.Equal(6, familia.FindByDisplayName("Holgura").Count);

            Assert.ThrowsAny<ArgumentException>(() => Table(VariableKey(ClaveX, "A"), VariableKey(ClaveXMayusculas, "B")));
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
