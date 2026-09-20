using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.ProjectVariables;
using RackCad.UI;
using Xunit;
using Xunit.Sdk;

namespace RackCad.UI.Tests
{
    /// <summary>Last-mile text contract over G9 structured recovery state; no eligibility is recomputed here.</summary>
    public sealed class G10RecoveryTextContractTests
    {
        [Fact, Trait("Gate", "G10-RecoveryText")]
        public void RECOVERY_CANDIDATE_NAMES_THE_UNIT_WITHOUT_PROMISING_MORE_THAN_G9()
        {
            var text = Compose(Assessment(true, "A", RackRepairability.Repairable));
            Assert.Contains("Corregir A", text);
            Assert.Contains("permitiría recuperar", text);
        }

        [Fact, Trait("Gate", "G10-A3")]
        public void T_A3_48_A_SAME_RACK_OTHER_INVALID_SOURCES_ALLOWS_FROZEN_THIS_RACK_TEXT()
        {
            var text = Compose(Assessment(false, null, RackRepairability.Repairable,
                reasons: new[] { RecoveryBlockingReason.OtherInvalidSources },
                data: new[] { "rack=K;property=uprightWidth;cause=Domain" }), "K");
            Assert.Contains("este rack", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("otras fuentes inválidas", text, StringComparison.OrdinalIgnoreCase);
        }

        [Fact, Trait("Gate", "G10-A3")]
        public void T_A3_48_B_BLOCKERS_ONLY_IN_ANOTHER_RACK_NEVER_SAY_THIS_RACK()
        {
            var text = Compose(Assessment(false, null, RackRepairability.Repairable,
                reasons: new[] { RecoveryBlockingReason.OtherInvalidSources },
                data: new[] { "rack=K2;property=palletTolerance;cause=MissingTarget" }), "K");
            Assert.DoesNotContain("este rack", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("K2", text);
        }

        [Fact, Trait("Gate", "G10-A3")]
        public void T_A3_48_C_SEVERAL_UNITS_ONLY_DENIES_A_DEMONSTRABLE_ONE_CHANGE_RECOVERY()
        {
            var text = Compose(Assessment(false, null, RackRepairability.Repairable,
                units: new[] { "A", "B" },
                reasons: new[] { RecoveryBlockingReason.SeveralRecoveryUnits }));
            Assert.Contains("No puede garantizarse", text);
            Assert.DoesNotContain("imposible", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("otras fuentes inválidas", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<X>", text);
            Assert.Contains("A", text); Assert.Contains("B", text);
        }

        [Fact, Trait("Gate", "G10-A3")]
        public void T_A3_48_D_NON_SIMPLE_CYCLE_ONLY_DENIES_GUARANTEE_AND_LISTS_MEMBERS()
        {
            var text = Compose(Assessment(false, null, RackRepairability.Repairable,
                roots: new[] { "Cycle(A,B,C)" },
                reasons: new[] { RecoveryBlockingReason.NonSimpleCycle }));
            Assert.Contains("no es simple", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("A", text); Assert.Contains("B", text); Assert.Contains("C", text);
            Assert.DoesNotContain("imposible", text, StringComparison.OrdinalIgnoreCase);
        }

        [Fact, Trait("Gate", "G10-A3")]
        public void T_A3_48_E_DISCOVERY_INDETERMINATE_STATES_ONLY_THE_ABORT_AND_LIMIT_OF_KNOWLEDGE()
        {
            var text = Compose(Assessment(false, null, RackRepairability.Repairable,
                reasons: new[] { RecoveryBlockingReason.DiscoveryIndeterminate },
                data: new[] { "ProbeIndeterminate(K9)" }));
            Assert.Contains("No se pudo completar el descubrimiento", text);
            Assert.Contains("K9", text);
            Assert.DoesNotContain("consume", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("fuentes inválidas", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("imposible", text, StringComparison.OrdinalIgnoreCase);
        }

        [Fact, Trait("Gate", "G10-A3")]
        public void T_A3_48_F_BLOCKED_RACK_HAS_NO_REPAIR_ACTION_OR_WARNING()
        {
            var text = Compose(Assessment(false, null, RackRepairability.Blocked,
                reasons: new[] { RecoveryBlockingReason.DiscoveryIndeterminate }));
            Assert.Contains("bloqueado", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Reparar el rack eliminará", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Repara este rack", text, StringComparison.OrdinalIgnoreCase);
        }

        [Fact, Trait("Gate", "G10-A3")]
        public void T_A3_48_G_REPAIRABLE_RACK_SHOWS_WARNING_WITH_EACH_REMOVED_FORMULA()
        {
            var text = Compose(Assessment(false, null, RackRepairability.Repairable,
                reasons: new[] { RecoveryBlockingReason.OtherInvalidSources },
                data: new[] { "remove:palletTolerance:=A + 2", "remove:uprightWidth:=B" }), repairWillAbort: false);
            Assert.Contains("Reparar el rack eliminará", text);
            Assert.Contains("palletTolerance", text); Assert.Contains("=A + 2", text);
            Assert.Contains("uprightWidth", text); Assert.Contains("=B", text);
        }

        [Fact, Trait("Gate", "G10-A3")]
        public void T_A3_48_H_REPAIR_THAT_WOULD_ABORT_MAKES_NO_DESTRUCTIVE_PROMISE()
        {
            var text = Compose(Assessment(false, null, RackRepairability.Repairable,
                reasons: new[] { RecoveryBlockingReason.DiscoveryIndeterminate },
                data: new[] { "EnvelopeUnclassifiable(D1)", "remove:palletTolerance:=A + 2" }),
                repairWillAbort: true);
            Assert.DoesNotContain("Reparar el rack eliminará", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("se eliminará", text, StringComparison.OrdinalIgnoreCase);
        }

        [Fact, Trait("Gate", "G10-RecoveryText")]
        public void DISCOVERY_ABORT_SUPPRESSES_SUGGESTION_BUT_KEEPS_ALLOWED_REPAIR_WARNING()
        {
            var text = Compose(Assessment(false, null, RackRepairability.Repairable,
                reasons: new[] { RecoveryBlockingReason.DiscoveryIndeterminate },
                data: new[] { "ProbeIndeterminate(K9)", "remove:palletTolerance:=A + 2" }),
                repairWillAbort: false);
            Assert.DoesNotContain("Corregir", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Reparar el rack eliminará", text);
        }

        [Fact, Trait("Gate", "G10-RecoveryText")]
        public void R1_REJECTION_LEADS_WITH_ATTEMPTED_DIVISION_BY_ZERO_AND_LABELS_PRIOR_CONTEXT()
        {
            var attempted = CreateAttemptedFailure("K", "DivisionByZero", new[] { "DivisionByZero" });
            var text = ComposeAttempt(attempted, new[] { "prior:OtherInvalidSources(A)" });
            Assert.Contains("DivisionByZero", text);
            Assert.Contains("antes de la corrección", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Corregir A permitiría recuperar", text, StringComparison.OrdinalIgnoreCase);
        }

        private static RecoveryAssessment Assessment(
            bool candidate, string candidateName, RackRepairability repairability,
            string[] roots = null, string[] units = null, RecoveryBlockingReason[] reasons = null,
            string[] data = null)
        {
            var ctor = typeof(RecoveryAssessment).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();
            return (RecoveryAssessment)ctor.Invoke(new object[]
            {
                candidate, candidateName, roots ?? Array.Empty<string>(), units ?? Array.Empty<string>(),
                reasons ?? Array.Empty<RecoveryBlockingReason>(), data ?? Array.Empty<string>(), repairability,
                Array.Empty<RecoveryDiscoveryAbortCause>(), repairability == RackRepairability.Repairable, "Upstream",
            });
        }

        private static string Compose(RecoveryAssessment assessment, string rackId = "K", bool repairWillAbort = false)
        {
            var methods = typeof(ProjectVariableRepairText).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(method => method.Name is "DescribeRecovery" or "DescribeAssessment" or "ComposeRecovery")
                .ToArray();
            foreach (var method in methods)
            {
                var values = Bind(method.GetParameters(), assessment, rackId, repairWillAbort);
                if (values != null) return method.Invoke(null, values)?.ToString() ?? string.Empty;
            }
            throw new XunitException("G10 contract RED: ProjectVariableRepairText has no structured RecoveryAssessment composer.");
        }

        private static string ComposeAttempt(AttemptedStateFailure attempted, IReadOnlyList<string> prior)
        {
            var method = typeof(ProjectVariableRepairText).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(candidate => candidate.Name is "DescribeAttemptedFailure" or "ComposeAttemptedFailure");
            if (method == null)
                throw new XunitException("G10 contract RED: attempted-state failure text composer is missing.");
            var values = Bind(method.GetParameters(), attempted, prior, "K");
            if (values == null) throw new XunitException("G10 contract RED: attempted-state composer does not consume structured context.");
            return method.Invoke(null, values)?.ToString() ?? string.Empty;
        }

        private static AttemptedStateFailure CreateAttemptedFailure(string rack, string category, string[] codes)
            => (AttemptedStateFailure)Activator.CreateInstance(typeof(AttemptedStateFailure),
                BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { rack, category, codes }, null);

        private static object[] Bind(ParameterInfo[] parameters, params object[] available)
        {
            var result = new object[parameters.Length];
            var used = new bool[available.Length];
            for (var p = 0; p < parameters.Length; p++)
            {
                var found = -1;
                for (var item = 0; item < available.Length; item++)
                    if (!used[item] && available[item] != null && parameters[p].ParameterType.IsInstanceOfType(available[item]))
                    { found = item; break; }
                if (found < 0)
                {
                    if (parameters[p].HasDefaultValue) { result[p] = parameters[p].DefaultValue; continue; }
                    return null;
                }
                used[found] = true; result[p] = available[found];
            }
            return result;
        }
    }
}
