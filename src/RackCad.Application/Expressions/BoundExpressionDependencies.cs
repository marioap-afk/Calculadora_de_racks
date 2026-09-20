using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The single pure dependency authority for a bound tree. It returns semantic identities in P11.1 order and never
    /// evaluates the tree, resolves names or consults a symbol table.
    /// </summary>
    public static class BoundExpressionDependencies
    {
        public static IReadOnlyList<SymbolId> DirectDependencies(BoundExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var found = new SortedSet<SymbolId>();
            var pending = new Stack<BoundExpression>();
            pending.Push(expression);

            while (pending.Count > 0)
            {
                switch (pending.Pop())
                {
                    case BoundReference reference:
                        found.Add(reference.Symbol);
                        break;

                    case BoundNegate negate:
                        pending.Push(negate.Operand);
                        break;

                    case BoundBinary binary:
                        pending.Push(binary.Right);
                        pending.Push(binary.Left);
                        break;

                    case BoundCall call:
                        for (var index = call.Arguments.Count - 1; index >= 0; index--)
                        {
                            pending.Push(call.Arguments[index]);
                        }

                        break;
                }
            }

            return new ReadOnlyCollection<SymbolId>(new List<SymbolId>(found));
        }
    }
}
