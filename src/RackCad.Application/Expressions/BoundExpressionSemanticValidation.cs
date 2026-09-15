namespace RackCad.Application.Expressions
{
    /// <summary>
    /// Static semantic checks shared by registry definitions and bound consumer expressions. These checks are
    /// deliberately downstream of persistence: their trees are structurally readable but cannot produce a value.
    /// </summary>
    internal static class BoundExpressionSemanticValidation
    {
        internal static bool IsNonCanonical(BoundExpression expression)
            => expression is BoundNumber number && !number.Unit.HasValue
               || expression is BoundNegate negate
               && negate.Operand is BoundNumber negatedNumber
               && !negatedNumber.Unit.HasValue;
    }
}
