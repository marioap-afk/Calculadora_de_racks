using System;
using System.Linq;
using RackCad.Application.Expressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — catálogo CERRADO de diagnósticos de V6 P15.3, completo (ADR-0040 D14 y D8).
    ///
    /// <para>
    /// G6 amplía el catálogo que G5 declaró sin renumerar nada: los doce códigos de sintaxis y límites conservan su valor
    /// y su orden, y se añaden al final los de enlace, los semánticos y el de contrato de frontera. <c>Cycle</c> y
    /// <c>DependencyFailed</c> son de G7 y <c>OutOfRange</c> de los adaptadores y consumidores: G6 los declara, no los
    /// produce. Los tipos de límite amplían los del parser con los normativos del árbol, y el valor 1 de
    /// <c>TextLength</c> no se reutiliza (A1 §6).
    /// </para>
    /// </summary>
    public class ExpressionDiagnosticCatalogTests
    {
        [Fact]
        public void EL_CATALOGO_COMPLETO_TIENE_EL_ORDEN_LOS_VALORES_Y_LAS_CLASES_DE_V6()
        {
            var esperado = new (string Name, ExpressionDiagnosticClass Class)[]
            {
                ("EmptyExpression", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("UnexpectedCharacter", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("UnexpectedToken", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("UnbalancedParenthesis", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("UnterminatedName", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("InvalidNumber", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("AmbiguousDecimalComma", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("UnknownUnit", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("UnitNotAllowedHere", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("UnitSyntaxNotSupported", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("InvalidQualifier", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("LimitExceeded", ExpressionDiagnosticClass.SyntaxAndLimits),
                ("UnknownSymbol", ExpressionDiagnosticClass.Binding),
                ("AmbiguousName", ExpressionDiagnosticClass.Binding),
                ("UnknownNamespace", ExpressionDiagnosticClass.Binding),
                ("UnknownFunction", ExpressionDiagnosticClass.Binding),
                ("ScopeViolation", ExpressionDiagnosticClass.Binding),
                ("ReservedName", ExpressionDiagnosticClass.Binding),
                ("NameRequired", ExpressionDiagnosticClass.Binding),
                ("QualifiedNameMismatch", ExpressionDiagnosticClass.Binding),
                ("OperatorInName", ExpressionDiagnosticClass.Binding),
                ("BrokenReference", ExpressionDiagnosticClass.Semantic),
                ("Cycle", ExpressionDiagnosticClass.Semantic),
                ("DependencyFailed", ExpressionDiagnosticClass.Semantic),
                ("InvalidArguments", ExpressionDiagnosticClass.Semantic),
                ("DivisionByZero", ExpressionDiagnosticClass.Semantic),
                ("NonFiniteResult", ExpressionDiagnosticClass.Semantic),
                ("NonCanonicalForm", ExpressionDiagnosticClass.Semantic),
                ("OutOfRange", ExpressionDiagnosticClass.BoundaryContract),
            };

            var codigos = Enum.GetValues(typeof(ExpressionDiagnosticCode)).Cast<ExpressionDiagnosticCode>().OrderBy(code => (int)code).ToList();

            Assert.Equal(esperado.Select(entry => entry.Name), codigos.Select(code => code.ToString()));
            Assert.Equal(Enumerable.Range(1, esperado.Length), codigos.Select(code => (int)code));
            Assert.Equal(esperado.Select(entry => entry.Class), codigos.Select(ExpressionDiagnostic.ClassOf));
            Assert.Equal(new[] { 1, 2, 3, 4 }, Enum.GetValues<ExpressionDiagnosticClass>().Select(value => (int)value));
        }

        [Fact]
        public void LOS_TIPOS_DE_LIMITE_SON_LAS_DOS_GUARDAS_DEL_PARSER_Y_LOS_TRES_LIMITES_NORMATIVOS()
        {
            Assert.Equal(
                new[] { "SyntacticNesting", "SyntacticTokenCount", "NodeCount", "BoundExpressionDepth", "ArgumentCount" },
                Enum.GetValues(typeof(ExpressionLimitKind)).Cast<ExpressionLimitKind>().OrderBy(kind => (int)kind).Select(kind => kind.ToString()));

            Assert.Equal(new[] { 2, 3, 4, 5, 6 }, Enum.GetValues<ExpressionLimitKind>().Select(value => (int)value).OrderBy(value => value));
        }
    }
}
