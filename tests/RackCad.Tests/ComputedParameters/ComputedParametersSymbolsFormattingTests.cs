using System;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ComputedParametersSymbolsKit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G3-T1 (RED) - el formatter y la clasificacion canonica con simbolos rack (D-16.6-7): INV-21 (Format, Parse,
    /// Bind con homonimo), INV-22 (id rack ausente: se muestra y nunca enlaza) e INV-23 (un arbol con referencia rack es
    /// <c>Expression</c>, nunca <c>DirectReference</c>).
    ///
    /// <para>
    /// INV-22 fija el resultado OBSERVADO el 2026-10-02 al ejecutar el parser vigente sobre <c>Rack.#{zzz}</c>: un solo
    /// diagnostico <c>InvalidQualifier</c> (codigo 11 del catalogo V6, clase SyntaxAndLimits) sobre el lapso 5+6 (el
    /// cualificador <c>#{zzz}</c>), sin arbol sintactico. Sin arbol sintactico el binder no tiene nada que enlazar, asi que
    /// no hay arbol enlazado. No exige ningun codigo nuevo.
    /// </para>
    /// </summary>
    public class ComputedParametersSymbolsFormattingTests
    {
        private static string Format(BoundExpression tree, ExpressionContext context) => ExpressionFormatter.Format(tree, context.Symbols);

        private static BoundExpression RackRef(SymbolId id) => BoundExpression.Reference(id);

        // ================================================================ D-16.6: un miembro rack presente se escribe siempre Rack.<miembro>

        [Fact]
        public void APresentRackSymbol_IsAlwaysWrittenAsRackDotMember_WithoutQualifierOrBraces()
        {
            var context = RackContext(Variable(1, "Holgura", 6));

            Assert.Equal("Rack.Frentes", Format(RackRef(FrentesId), context));
            Assert.Equal("Rack.FrentesVacios", Format(RackRef(FrentesVaciosId), context));
            Assert.Equal("Rack.Frentes + Rack.FrentesVacios", Format(Add(RackRef(FrentesId), RackRef(FrentesVaciosId)), context));
            Assert.Equal("2 * Rack.Frentes", Format(Mul(Num(2), RackRef(FrentesId)), context));
        }

        // ================================================================ INV-21: Format -> Parse -> Bind con homonimo

        /// <summary>El MISMO helper para el objetivo y para el control: formatea y comprueba que el texto vuelve al mismo arbol.</summary>
        private static string FormatAndRoundTrip(BoundExpression tree, ExpressionContext context)
        {
            var text = Format(tree, context);

            Assert.Equal(tree, BindOk(text, context, SymbolScope.Rack));
            return text;
        }

        [Fact]
        public void INV21_RackFrentesTimesTheHomonymousVariable_FormatsWithoutQualifyingTheVariable_AndRoundTrips()
        {
            var context = RackContext(Variable(1, "Frentes", 3));

            var tree = Mul(RackRef(FrentesId), Ref(1));
            Assert.Equal("Rack.Frentes * Frentes", FormatAndRoundTrip(tree, context));

            // El mismo caso con los operandos al reves.
            Assert.Equal("Frentes + Rack.Frentes", FormatAndRoundTrip(Add(Ref(1), RackRef(FrentesId)), context));
        }

        [Fact]
        public void INV21_ControlHomonymsAreCountedOnlyAmongProjectVariables()
        {
            // Control con el MISMO helper: dos variables homonimas SI se cualifican, como hoy; el miembro rack no cuenta
            // como homonimo de ninguna de las dos.
            var context = RackContext(Variable(1, "Frentes", 3), Variable(2, "Frentes", 4));

            var tree = Mul(RackRef(FrentesId), Ref(1));

            Assert.Equal("Rack.Frentes * Frentes#" + Key(1), FormatAndRoundTrip(tree, context));
        }

        [Fact]
        public void INV21_AVariableAloneInATableWithAHomonymousRackMember_IsNotQualified()
        {
            var context = RackContext(Variable(1, "Frentes", 3));

            // Antes del cambio un indice global daria dos candidatos para "Frentes" y la cualificaria con #guid.
            Assert.Equal("Frentes", Format(Ref(1), context));
            Assert.Equal(Ref(1), BindOk("Frentes", context, SymbolScope.Rack));
        }

        // ================================================================ INV-22: id rack ausente

        [Fact]
        public void INV22_AnAbsentRackId_IsShownAsRackDotHashBraceToken()
        {
            var tree = RackRef(RackId("zzz"));

            foreach (var table in new[] { SymbolTable.Empty, Table(Variable(1, "A")), Table(CatalogRackEntries()) })
            {
                Assert.Equal("Rack.#{zzz}", ExpressionFormatter.Format(tree, table));
                Assert.Equal("Rack.#{zzz} + 1", ExpressionFormatter.Format(Add(tree, Num(1)), table));
            }

            // Control: el mismo arbol con el miembro PRESENTE se escribe con su nombre.
            Assert.Equal("Rack.Frentes", ExpressionFormatter.Format(RackRef(FrentesId), Table(CatalogRackEntries())));
        }

        [Theory]
        [InlineData("Rack.#{zzz}")]
        [InlineData("Rack.#{zzz} * 2")]
        [InlineData("Rack.#{zzz}+1")]
        public void INV22_TheDiagnosticForm_NeverParses_ItIsOneInvalidQualifierAndNoTree(string text)
        {
            var parsed = ExpressionParser.Parse(text);

            Assert.False(parsed.Succeeded);

            var diagnostic = Assert.Single(parsed.Diagnostics);
            Assert.Equal(ExpressionDiagnosticCode.InvalidQualifier, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.SyntaxAndLimits, diagnostic.Class);
            Assert.Equal(new SourceSpan(5, 6), diagnostic.Span);
            Assert.Null(diagnostic.Limit);

            // Sin arbol sintactico no hay nada que enlazar: no existe arbol enlazado.
            Assert.Throws<InvalidOperationException>(() => parsed.Syntax);

            // Determinista: el mismo texto, el mismo diagnostico.
            Assert.Equal(Describe(parsed.Diagnostics), Describe(ExpressionParser.Parse(text).Diagnostics));
        }

        [Fact]
        public void INV22_FormatThenParse_OfAnAbsentRackId_GivesTheFixedDiagnostic_AndNeverATree()
        {
            var shown = ExpressionFormatter.Format(RackRef(RackId("zzz")), Table(CatalogRackEntries()));

            Assert.Equal("Rack.#{zzz}", shown);

            var parsed = ExpressionParser.Parse(shown);

            Assert.False(parsed.Succeeded);
            var diagnostic = Assert.Single(parsed.Diagnostics);
            Assert.Equal(ExpressionDiagnosticCode.InvalidQualifier, diagnostic.Code);
            Assert.Equal(new SourceSpan(5, 6), diagnostic.Span);
            Assert.Throws<InvalidOperationException>(() => parsed.Syntax);
        }

        // ================================================================ INV-23: clasificacion canonica

        /// <summary>El MISMO helper para el objetivo y para el control: enlaza el texto y lo clasifica.</summary>
        private static CanonicalShape Classify(string text, ExpressionContext context)
            => CanonicalShape.Classify(BindOk(text, context, SymbolScope.Rack));

        [Fact]
        public void INV23_ARackReferenceAlone_IsAnExpression_NeverADirectReference()
        {
            var context = RackContext(Variable(1, "Frentes", 3));

            var shape = Classify("Rack.Frentes", context);

            Assert.Equal(CanonicalShapeKind.Expression, shape.Kind);
            Assert.Equal(RackRef(FrentesId), shape.Expression);
            Assert.Throws<InvalidOperationException>(() => shape.Reference);
            Assert.Throws<InvalidOperationException>(() => shape.LiteralValue);

            Assert.Equal(CanonicalShapeKind.Expression, Classify("(+Rack.FrentesVacios)", context).Kind);
            Assert.Equal(CanonicalShapeKind.Expression, Classify("Rack.Frentes * 2", context).Kind);
        }

        [Fact]
        public void INV23_ControlAProjectVariableAlone_StaysADirectReference()
        {
            var context = RackContext(Variable(1, "Frentes", 3));

            var shape = Classify("Frentes", context);

            Assert.Equal(CanonicalShapeKind.DirectReference, shape.Kind);
            Assert.Equal(Id(1), shape.Reference);
        }

        [Fact]
        public void INV23_ClassifyingTheTreeDirectly_DoesNotNeedATable()
        {
            Assert.Equal(CanonicalShapeKind.Expression, CanonicalShape.Classify(RackRef(FrentesId)).Kind);
            Assert.Equal(CanonicalShapeKind.DirectReference, CanonicalShape.Classify(Ref(1)).Kind);
        }
    }
}
