using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Units;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — utilidades de las pruebas del núcleo semántico de expresiones.
    ///
    /// <para>
    /// Las tablas de símbolos de estas pruebas son ENTRADAS SINTÉTICAS (V6 P6.5): se construyen con la costura interna
    /// del núcleo, que Plugin y UI no alcanzan. Ningún tipo de Project Variables interviene: G6 no conecta el registro
    /// real, eso es G8.
    /// </para>
    /// </summary>
    internal static class ExpressionSemanticTestSupport
    {
        /// <summary>Un id de variable de proyecto en forma D, determinista por número y en minúsculas.</summary>
        internal static string Key(int n) => "00000000-0000-4000-8000-" + n.ToString("x12", CultureInfo.InvariantCulture);

        internal static SymbolId Id(int n) => SymbolId.ProjectVariable(Key(n));

        internal static SymbolEntry Variable(int n, string name, double literal = 1, SymbolScope scope = SymbolScope.Project)
            => new SymbolEntry(Id(n), scope, name, SymbolDefinition.FromLiteral(literal));

        /// <summary>Una variable con la clave textual exacta que se indica, en cualquier grafía válida (Amendment A2 §3.2).</summary>
        internal static SymbolEntry VariableKey(string key, string name, double literal = 1, SymbolScope scope = SymbolScope.Project)
            => new SymbolEntry(SymbolId.ProjectVariable(key), scope, name, SymbolDefinition.FromLiteral(literal));

        internal static SymbolTable Table(params SymbolEntry[] entries) => SymbolTable.Create(entries);

        internal static ExpressionContext Context(params SymbolEntry[] entries) => ExpressionContext.Create(Table(entries));

        /// <summary>Analiza (tiene que ser sintaxis válida) y enlaza.</summary>
        internal static ExpressionBindResult Bind(string text, ExpressionContext context, SymbolScope scope = SymbolScope.Project)
        {
            var parsed = ExpressionParser.Parse(text);

            Assert.True(
                parsed.Succeeded,
                "Se esperaba sintaxis válida para «" + Short(text) + "»: " + ExpressionSyntaxTestSupport.Describe(parsed));

            return ExpressionBinder.Bind(parsed.Syntax, context, scope);
        }

        internal static BoundExpression BindOk(string text, ExpressionContext context, SymbolScope scope = SymbolScope.Project)
        {
            var result = Bind(text, context, scope);

            Assert.True(result.Succeeded, "Se esperaba un enlace correcto para «" + Short(text) + "»: " + Describe(result.Diagnostics));
            Assert.Empty(result.Diagnostics);
            return result.Expression;
        }

        internal static IReadOnlyList<ExpressionDiagnostic> BindFails(string text, ExpressionContext context, SymbolScope scope = SymbolScope.Project)
        {
            var result = Bind(text, context, scope);

            Assert.False(result.Succeeded, "Se esperaba un fallo de enlace para «" + Short(text) + "».");
            Assert.NotEmpty(result.Diagnostics);
            return result.Diagnostics;
        }

        internal static double EvaluateOk(BoundExpression expression, ExpressionContext context, IReadOnlyDictionary<SymbolId, double> values)
        {
            var result = ExpressionEvaluator.Evaluate(expression, context, values);

            Assert.True(result.Succeeded, "Se esperaba una evaluación correcta: " + Describe(result.Diagnostics));
            Assert.Empty(result.Diagnostics);
            return result.Value;
        }

        internal static IReadOnlyDictionary<SymbolId, double> Values(params (SymbolId Id, double Value)[] pairs)
            => pairs.ToDictionary(pair => pair.Id, pair => pair.Value);

        /// <summary>Código, límite y posición de cada diagnóstico, para los mensajes de las aserciones.</summary>
        internal static string Describe(IReadOnlyList<ExpressionDiagnostic> diagnostics)
        {
            if (diagnostics == null || diagnostics.Count == 0)
            {
                return "sin diagnósticos";
            }

            return string.Join(
                ", ",
                diagnostics.Select(diagnostic =>
                    diagnostic.Code
                    + (diagnostic.Limit.HasValue ? "(" + diagnostic.Limit.Value + ")" : string.Empty)
                    + "@" + (diagnostic.Span.HasValue
                        ? diagnostic.Span.Value.Start.ToString(CultureInfo.InvariantCulture) + "+"
                          + diagnostic.Span.Value.Length.ToString(CultureInfo.InvariantCulture)
                        : "sin-posición")));
        }

        /// <summary>Igualdad de doubles bit a bit: la que exige V6 P9.3 y P14.5.</summary>
        internal static void AssertBits(double expected, double actual)
            => Assert.True(
                BitConverter.DoubleToInt64Bits(expected) == BitConverter.DoubleToInt64Bits(actual),
                "Se esperaban los bits de " + expected.ToString("R", CultureInfo.InvariantCulture)
                + " y se obtuvo " + actual.ToString("R", CultureInfo.InvariantCulture));

        /// <summary>Tokens que el lexer del núcleo delimita en un texto sin errores, sin la marca de fin de texto.</summary>
        internal static int TokenCount(string text)
        {
            var lexed = ExpressionLexer.Tokenize(text);

            Assert.Empty(lexed.Diagnostics);
            return lexed.Tokens.Count - 1;
        }

        internal static BoundExpression Num(double value, LengthUnit? unit = null) => BoundExpression.Number(value, unit);

        internal static BoundExpression Ref(int n) => BoundExpression.Reference(Id(n));

        internal static BoundExpression Neg(BoundExpression operand) => BoundExpression.Negate(operand);

        internal static BoundExpression Add(BoundExpression left, BoundExpression right)
            => BoundExpression.Binary(BoundBinaryOperator.Add, left, right);

        internal static BoundExpression Sub(BoundExpression left, BoundExpression right)
            => BoundExpression.Binary(BoundBinaryOperator.Subtract, left, right);

        internal static BoundExpression Mul(BoundExpression left, BoundExpression right)
            => BoundExpression.Binary(BoundBinaryOperator.Multiply, left, right);

        internal static BoundExpression Div(BoundExpression left, BoundExpression right)
            => BoundExpression.Binary(BoundBinaryOperator.Divide, left, right);

        internal static BoundExpression Call(FunctionId function, params BoundExpression[] arguments)
            => BoundExpression.Call(function, arguments);

        internal static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return dir;
        }

        internal static string Short(string text)
            => text == null ? "<null>"
                : text.Length <= 120 ? text
                : text.Substring(0, 120) + "…(" + text.Length.ToString(CultureInfo.InvariantCulture) + " caracteres)";
    }
}
