using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Expressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G5 — utilidades de las pruebas del núcleo sintáctico de expresiones.
    ///
    /// <para>
    /// <see cref="Dump"/> es un ORÁCULO DE PRUEBA y no un formatter: vuelca el árbol sintáctico como expresión S
    /// para afirmar precedencia, asociatividad y forma sin depender de ningún texto canónico. El formatter canónico
    /// de la Proposal V6 (P4) y el round-trip P2.5 dependen del binder, de los ids y del snapshot, y quedan en G6
    /// por el ajuste de calendario G5/G6 que fijó el Coordinador. Nada de esta clase es producción.
    /// </para>
    /// </summary>
    internal static class ExpressionSyntaxTestSupport
    {
        /// <summary>Los dos GUID de los ejemplos de V6 §4.4.</summary>
        internal const string Guid1 = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";

        internal const string Guid2 = "8c1d7e20-4b5a-4c6d-9e7f-102132435465";

        /// <summary>Analiza un texto que DEBE ser sintaxis válida y devuelve su árbol.</summary>
        internal static ExpressionSyntax ParseOk(string text)
        {
            var result = ExpressionParser.Parse(text);

            Assert.True(result.Succeeded, "Se esperaba sintaxis válida para «" + text + "»: " + Describe(result));
            Assert.Empty(result.Diagnostics);
            return result.Syntax;
        }

        /// <summary>Analiza un texto que DEBE fallar y devuelve sus diagnósticos, ya ordenados por el núcleo.</summary>
        internal static IReadOnlyList<ExpressionDiagnostic> ParseFails(string text)
        {
            var result = ExpressionParser.Parse(text);

            Assert.False(result.Succeeded, "Se esperaba un fallo para «" + text + "».");
            Assert.NotEmpty(result.Diagnostics);
            return result.Diagnostics;
        }

        /// <summary>Código y posición de cada diagnóstico, para los mensajes de las aserciones.</summary>
        internal static string Describe(ExpressionParseResult result)
        {
            var diagnostics = result.Diagnostics ?? Array.Empty<ExpressionDiagnostic>();
            return diagnostics.Count == 0
                ? "sin diagnósticos"
                : string.Join(", ", diagnostics.Select(Describe));
        }

        internal static string Describe(ExpressionDiagnostic diagnostic)
            => diagnostic.Code + "@" + (diagnostic.Span.HasValue
                ? diagnostic.Span.Value.Start.ToString(CultureInfo.InvariantCulture) + "+"
                  + diagnostic.Span.Value.Length.ToString(CultureInfo.InvariantCulture)
                : "sin-posición");

        /// <summary>El árbol como expresión S: <c>(+ (num 1) (* (num 2) (num 3)))</c>.</summary>
        internal static string Dump(ExpressionSyntax syntax)
        {
            var builder = new StringBuilder();
            Append(builder, syntax);
            return builder.ToString();
        }

        /// <summary>Ejecuta <paramref name="action"/> con otra cultura del proceso y la restaura siempre.</summary>
        internal static void WithCulture(string name, Action action)
        {
            var culture = CultureInfo.CurrentCulture;
            var uiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
                action();
            }
            finally
            {
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = uiCulture;
            }
        }

        internal static string Repeat(string piece, int count)
            => new StringBuilder(piece.Length * count).Insert(0, piece, count).ToString();

        private static void Append(StringBuilder builder, ExpressionSyntax syntax)
        {
            switch (syntax)
            {
                case NumberSyntax number:
                    builder.Append("(num ").Append(number.Value.ToString("R", CultureInfo.InvariantCulture));
                    if (number.UnitToken != null)
                    {
                        builder.Append(' ').Append(number.UnitToken);
                    }

                    builder.Append(')');
                    break;

                case ReferenceSyntax reference:
                    builder.Append("(ref");
                    if (reference.Name != null)
                    {
                        builder.Append(reference.Name.IsBraced ? " braced " : " bare ").Append(Quote(reference.Name.Text));
                    }

                    if (reference.Qualifier != null)
                    {
                        builder.Append(" #").Append(reference.Qualifier.Id.ToString("D"));
                    }

                    builder.Append(')');
                    break;

                case NamespaceReferenceSyntax reserved:
                    builder.Append("(ns ").Append(Quote(reserved.Namespace.Text));
                    if (reserved.Member != null)
                    {
                        builder.Append(' ').Append(Quote(reserved.Member.Text));
                    }

                    builder.Append(')');
                    break;

                case UnaryExpressionSyntax unary:
                    builder.Append(unary.Operator == SyntaxUnaryOperator.Minus ? "(neg " : "(pos ");
                    Append(builder, unary.Operand);
                    builder.Append(')');
                    break;

                case BinaryExpressionSyntax binary:
                    builder.Append('(').Append(Symbol(binary.Operator)).Append(' ');
                    Append(builder, binary.Left);
                    builder.Append(' ');
                    Append(builder, binary.Right);
                    builder.Append(')');
                    break;

                case ParenthesizedExpressionSyntax parenthesized:
                    builder.Append("(paren ");
                    Append(builder, parenthesized.Expression);
                    builder.Append(')');
                    break;

                case CallExpressionSyntax call:
                    builder.Append("(call ").Append(Quote(call.Function.Text));
                    foreach (var argument in call.Arguments)
                    {
                        builder.Append(' ');
                        Append(builder, argument);
                    }

                    builder.Append(')');
                    break;

                default:
                    throw new InvalidOperationException(
                        "Nodo sintáctico no contemplado por el oráculo: " + (syntax == null ? "null" : syntax.GetType().Name));
            }
        }

        private static string Symbol(SyntaxBinaryOperator op)
        {
            switch (op)
            {
                case SyntaxBinaryOperator.Add: return "+";
                case SyntaxBinaryOperator.Subtract: return "-";
                case SyntaxBinaryOperator.Multiply: return "*";
                case SyntaxBinaryOperator.Divide: return "/";
                default: throw new InvalidOperationException("Operador binario no contemplado: " + op);
            }
        }

        private static string Quote(string text) => "\"" + text + "\"";
    }
}
