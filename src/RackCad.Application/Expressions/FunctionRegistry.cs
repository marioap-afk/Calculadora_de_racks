using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The closed set of functions (I-49, Proposal V6 P3.4 and P10.3; ADR-0040 D7). <c>ROUND</c>, <c>CEILING</c> and
    /// <c>FLOOR</c> are deferred (P10.6); <c>IF</c>, <c>AND</c>, <c>OR</c>, comparisons, lookup, arrays, strings,
    /// trigonometry and macros are excluded (P10.7). Adding a member is a code change with review and, for earlier builds,
    /// a change of what they can read (P10.8).
    /// </summary>
    public enum FunctionId
    {
        Min = 1,
        Max = 2,
        Abs = 3,
    }

    /// <summary>
    /// The closed, immutable function registry with ONE productive instance (P10.1–P10.5, P10.9).
    ///
    /// <para>
    /// No runtime registration, reflection, injection, plugins, user functions or scripting. Each function has a canonical
    /// upper-case token from an explicit two-way table compared ordinally, minimum and maximum arity, and a pure,
    /// deterministic implementation over finite doubles. A name typed by a user is looked up ignoring case; the token in
    /// the tree and in the formatter is upper case.
    /// </para>
    /// </summary>
    public sealed class FunctionRegistry
    {
        private static readonly IReadOnlyList<FunctionId> DeclaredFunctions =
            new ReadOnlyCollection<FunctionId>(new[] { FunctionId.Min, FunctionId.Max, FunctionId.Abs });

        private FunctionRegistry()
        {
        }

        public static FunctionRegistry Productive { get; } = new FunctionRegistry();

        /// <summary>The closed table, in declaration order.</summary>
        public IReadOnlyList<FunctionId> Functions => DeclaredFunctions;

        public string Token(FunctionId function)
        {
            switch (function)
            {
                case FunctionId.Min: return "MIN";
                case FunctionId.Max: return "MAX";
                case FunctionId.Abs: return "ABS";
                default: throw new ArgumentOutOfRangeException(nameof(function), function, "Undeclared function.");
            }
        }

        /// <summary>A canonical token, compared ordinally: the reading rule of a persisted tree (P17.10).</summary>
        public bool TryParseToken(string token, out FunctionId function)
        {
            switch (token)
            {
                case "MIN":
                    function = FunctionId.Min;
                    return true;

                case "MAX":
                    function = FunctionId.Max;
                    return true;

                case "ABS":
                    function = FunctionId.Abs;
                    return true;

                default:
                    function = default;
                    return false;
            }
        }

        /// <summary>A name as a user wrote it, ignoring case ordinally (P10.5): <c>max</c> and <c>Max</c> are MAX.</summary>
        public bool TryResolveName(string name, out FunctionId function)
        {
            foreach (var candidate in DeclaredFunctions)
            {
                if (string.Equals(Token(candidate), name, StringComparison.OrdinalIgnoreCase))
                {
                    function = candidate;
                    return true;
                }
            }

            function = default;
            return false;
        }

        public int MinimumArity(FunctionId function)
        {
            switch (function)
            {
                case FunctionId.Min:
                case FunctionId.Max:
                    return 2;

                case FunctionId.Abs:
                    return 1;

                default:
                    throw new ArgumentOutOfRangeException(nameof(function), function, "Undeclared function.");
            }
        }

        public int MaximumArity(FunctionId function)
        {
            switch (function)
            {
                case FunctionId.Min:
                case FunctionId.Max:
                    return 16;

                case FunctionId.Abs:
                    return 1;

                default:
                    throw new ArgumentOutOfRangeException(nameof(function), function, "Undeclared function.");
            }
        }

        internal bool AcceptsArity(FunctionId function, int count)
            => count >= MinimumArity(function) && count <= MaximumArity(function);

        /// <summary>
        /// The pure implementation. The caller has already checked the arity; the result of finite arguments is finite for
        /// the three functions, and the evaluator checks it anyway (P10.2).
        /// </summary>
        internal double Invoke(FunctionId function, IReadOnlyList<double> arguments)
        {
            switch (function)
            {
                case FunctionId.Min:
                {
                    var result = arguments[0];
                    for (var index = 1; index < arguments.Count; index++)
                    {
                        result = Math.Min(result, arguments[index]);
                    }

                    return result;
                }

                case FunctionId.Max:
                {
                    var result = arguments[0];
                    for (var index = 1; index < arguments.Count; index++)
                    {
                        result = Math.Max(result, arguments[index]);
                    }

                    return result;
                }

                case FunctionId.Abs:
                    return Math.Abs(arguments[0]);

                default:
                    throw new ArgumentOutOfRangeException(nameof(function), function, "Undeclared function.");
            }
        }
    }

    /// <summary>
    /// The words a bare name cannot be (P1.7, P10.9, P25.3; ADR-0040 D6): every function name of the registry —the same
    /// table, so a future function reserves its name for FUTURE writes only— plus <c>Rack</c> and <c>Project</c>, reserved
    /// for ID20. All compared ignoring case ordinally, on the WHOLE bare name. They are written with braces
    /// (<c>{MIN}</c>, <c>{Rack}</c>), which are ordinary variable names.
    /// </summary>
    internal static class ExpressionReservedNames
    {
        internal static bool IsReserved(string bareName)
        {
            if (bareName == null)
            {
                return false;
            }

            return FunctionRegistry.Productive.TryResolveName(bareName, out _)
                   || string.Equals(bareName, "Rack", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(bareName, "Project", StringComparison.OrdinalIgnoreCase);
        }
    }
}
