using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ComputedParametersSymbolsKit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G3-T1 (RED) - identidad rack en el nucleo: D-02 (validez de clave por namespace, regla de token, comparador
    /// Ordinal), D-16.2 y D-16.3 (<c>SymbolDefinitionKind.Computed</c>, hoja sin valor) e INV-27 (una entrada Computed es
    /// una hoja fuera del registro: <c>RegistryEvaluation</c> y <c>DependencyGraph</c> la rechazan como error de
    /// programacion, R4). El oraculo es el Freeze (Proposal V3 secciones 5, 12 y 13), no el codigo productivo.
    /// </summary>
    public class ComputedParametersSymbolsIdentityTests
    {
        public static TheoryData<string> ValidRackTokens => new TheoryData<string>
        {
            "frentes", "frentesVacios", "a", "x1", "aB9", "frentes2Total",
        };

        public static TheoryData<string> InvalidRackKeys => new TheoryData<string>
        {
            string.Empty,
            " ",
            "Frentes",
            "FRENTES",
            "FrentesVacios",
            "1frentes",
            "frentes-vacios",
            "frentes vacios",
            "frentes_vacios",
            "frentes.",
            "frentes ",
            " frentes",
            "{frentes}",
            "#frentes",
            Guid1,
            "fr" + (char)0x00E9 + "ntes",
            "frentes" + (char)0x0661,
            ((char)0xFF41).ToString() + "frentes",
        };

        // ================================================================ D-02: validez de clave del namespace rack

        [Theory]
        [MemberData(nameof(ValidRackTokens))]
        public void RackKey_AcceptsEveryTokenOfTheD02Rule_AndKeepsItExactly(string token)
        {
            var id = new SymbolId(RackNamespace, token);

            Assert.Equal(RackNamespace, id.Namespace);
            Assert.Equal(token, id.Key);
            Assert.Equal("rack:" + token, id.ToString());
        }

        [Theory]
        [MemberData(nameof(InvalidRackKeys))]
        public void RackKey_RejectsAnythingOutsideTheTokenRule(string key)
        {
            // Control en la MISMA prueba: el mismo constructor y el mismo namespace aceptan un token valido.
            Assert.Equal("frentes", new SymbolId(RackNamespace, "frentes").Key);

            Assert.ThrowsAny<ArgumentException>(() => new SymbolId(RackNamespace, key));
        }

        [Fact]
        public void RackKey_Null_IsRejected_AsForProjectVariable()
        {
            Assert.Equal("frentes", new SymbolId(RackNamespace, "frentes").Key);

            Assert.ThrowsAny<ArgumentException>(() => new SymbolId(RackNamespace, null));
            Assert.Throws<ArgumentNullException>(() => SymbolId.ProjectVariable(null));
        }

        [Fact]
        public void TheProjectVariableKeyRule_IsNotChangedByTheRackRule()
        {
            // El token rack no es un GUID, y un GUID no es un token rack: cada regla vive en su namespace.
            Assert.Equal("frentes", new SymbolId(RackNamespace, "frentes").Key);
            Assert.ThrowsAny<ArgumentException>(() => SymbolId.ProjectVariable("frentes"));
            Assert.ThrowsAny<ArgumentException>(() => new SymbolId(SymbolNamespace.ProjectVariable, "frentesVacios"));
            Assert.ThrowsAny<ArgumentException>(() => new SymbolId(RackNamespace, Guid1));

            Assert.Equal(Guid1, SymbolId.ProjectVariable(Guid1).Key);
        }

        [Fact]
        public void RackKeys_CompareOrdinal_AndCaseSensitive()
        {
            var vacios = RackId("frentesVacios");
            var vaciosLower = RackId("frentesvacios");

            Assert.Equal(RackId("frentesVacios"), vacios);
            Assert.Equal(RackId("frentesVacios").GetHashCode(), vacios.GetHashCode());
            Assert.True(vacios == RackId("frentesVacios"));

            // Ordinal y sensible a mayusculas: a diferencia de projectVariable, no es OrdinalIgnoreCase.
            Assert.NotEqual(vacios, vaciosLower);
            Assert.True(vacios != vaciosLower);
            Assert.False(vacios.Equals(vaciosLower));
            Assert.True(vacios.CompareTo(vaciosLower) < 0, "'V' precede a 'v' en Ordinal");
            Assert.True(RackId("frentes").CompareTo(vacios) < 0, "el prefijo precede al token mas largo");

            Assert.Same(StringComparer.Ordinal, SymbolNamespaces.KeyComparer(RackNamespace));
        }

        [Fact]
        public void TheSymbolIdOrder_IsTheNamespaceTokenOrdinallyThenTheKey()
        {
            var ids = new List<SymbolId> { RackId("frentesVacios"), Id(2), RackId("frentes"), Id(1) };

            ids.Sort();

            // "projectVariable" precede a "rack" en Ordinal; dentro de cada namespace manda su comparador.
            Assert.Equal(new[] { Id(1), Id(2), RackId("frentes"), RackId("frentesVacios") }, ids);
        }

        // ================================================================ D-16.3: Computed es una hoja sin valor

        [Fact]
        public void Computed_IsALeafDefinitionWithoutAValue()
        {
            var definition = Computed();

            Assert.Equal(ComputedKind, definition.Kind);
            Assert.NotEqual(SymbolDefinitionKind.Literal, definition.Kind);
            Assert.NotEqual(SymbolDefinitionKind.Expression, definition.Kind);

            // Misma disciplina que el resto de la union: leer el valor de otro caso lanza.
            Assert.Throws<InvalidOperationException>(() => definition.LiteralValue);
            Assert.Throws<InvalidOperationException>(() => definition.Expression);
        }

        [Fact]
        public void RackEntries_LiveInTheTable_InTheDeterministicSymbolIdOrder()
        {
            var table = Table(FrentesVacios(), Variable(2, "B"), Frentes(), Variable(1, "A"));

            Assert.Equal(new[] { Id(1), Id(2), FrentesId, FrentesVaciosId }, table.Entries.Select(entry => entry.Id));

            Assert.True(table.TryGet(FrentesId, out var entry));
            Assert.Equal("Frentes", entry.DisplayName);
            Assert.Equal(SymbolScope.Rack, entry.Scope);
            Assert.Equal(ComputedKind, entry.Definition.Kind);

            Assert.False(table.TryGet(RackId("zzz"), out _));

            // Una identidad rack repetida es un error de programacion, como cualquier otra.
            Assert.ThrowsAny<ArgumentException>(() => Table(Frentes(), Frentes()));
        }

        // ================================================================ INV-27: Computed es hoja fuera del registro (R4)

        [Fact]
        public void INV27_RegistryEvaluation_RejectsAComputedEntry_AsAProgrammingError()
        {
            // Control: la misma funcion acepta una tabla sin entradas Computed.
            var control = RegistryEvaluation.Evaluate(Context(Variable(1, "A", 6)));
            Assert.True(control.Result(Id(1)).Succeeded);

            var error = Assert.Throws<InvalidOperationException>(
                () => RegistryEvaluation.Evaluate(Context(Variable(1, "A", 6), Frentes())));

            Assert.Contains("Computed", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void INV27_DependencyGraph_RejectsAComputedEntry_AsAProgrammingError()
        {
            // Control: el mismo constructor acepta una tabla sin entradas Computed.
            var control = new DependencyGraph(Table(Variable(1, "A", 6)));
            Assert.Equal(new[] { Id(1) }, control.EvaluationOrder);

            var error = Assert.Throws<InvalidOperationException>(
                () => new DependencyGraph(Table(Variable(1, "A", 6), Frentes())));

            Assert.Contains("Computed", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void INV27_AComputedEntryAloneIsRejectedToo_ByBothEntryPoints()
        {
            Assert.Throws<InvalidOperationException>(() => RegistryEvaluation.Evaluate(Context(Frentes(), FrentesVacios())));
            Assert.Throws<InvalidOperationException>(() => new DependencyGraph(Table(Frentes(), FrentesVacios())));
        }
    }
}
