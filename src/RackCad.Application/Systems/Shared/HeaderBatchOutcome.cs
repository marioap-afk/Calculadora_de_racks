using System;
using System.Collections.Generic;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// The final result of a header reuse or batch distribution (I-53, contract §3.4): <see cref="Rejected"/>,
    /// <see cref="Cancelled"/> or <see cref="Committed"/>, and nothing else — the base constructor is private.
    /// <para>
    /// It is separate from <see cref="HeaderBatchPlan{TAddress}"/> on purpose: a plan says what WOULD be applied, an outcome
    /// says what WAS. <c>Applied</c> exists only in <see cref="Committed"/>, and it can only be the prepared plan's own
    /// targets, in the same order — a committed outcome is built from the plan, never from a list of its own. Cancelled is
    /// not Rejected: Rejected means the request was invalid or unsafe; Cancelled means the user chose not to apply a valid
    /// one.
    /// </para>
    /// </summary>
    public abstract class HeaderBatchOutcome<TAddress>
    {
        private HeaderBatchOutcome()
        {
        }

        /// <summary>Nothing was applied because the request was invalid or unsafe — a stale plan at MUTATE included.</summary>
        public sealed class Rejected : HeaderBatchOutcome<TAddress>
        {
            internal Rejected(HeaderRejectionCode code)
            {
                if (!Enum.IsDefined(code))
                {
                    throw new ArgumentOutOfRangeException(nameof(code), code, "Codigo de rechazo fuera del conjunto cerrado.");
                }

                Code = code;
            }

            public HeaderRejectionCode Code { get; }
        }

        /// <summary>
        /// Nothing was applied because the user did not confirm. Confirmation is asked if and only if the plan carries a
        /// severe notice, so a plan that did not require it cannot end cancelled.
        /// </summary>
        public sealed class Cancelled : HeaderBatchOutcome<TAddress>
        {
            internal Cancelled(HeaderBatchPlan<TAddress>.Prepared plan)
            {
                if (plan == null)
                {
                    throw new ArgumentNullException(nameof(plan));
                }

                if (!plan.RequiresConfirmation)
                {
                    throw new InvalidOperationException("Solo puede cancelarse un plan que exigia confirmacion.");
                }
            }
        }

        /// <summary>The prepared copies were assigned: exactly the plan's targets, in the plan's order.</summary>
        public sealed class Committed : HeaderBatchOutcome<TAddress>
        {
            internal Committed(HeaderBatchPlan<TAddress>.Prepared plan)
            {
                if (plan == null)
                {
                    throw new ArgumentNullException(nameof(plan));
                }

                Applied = plan.Targets;
                Omitted = plan.Omitted;
            }

            /// <summary>The prepared plan's <see cref="HeaderBatchPlan{TAddress}.Prepared.Targets"/>, same list and order.</summary>
            public IReadOnlyList<TAddress> Applied { get; }

            /// <summary>The prepared plan's omissions, unchanged.</summary>
            public IReadOnlyList<HeaderOmission<TAddress>> Omitted { get; }
        }
    }
}
