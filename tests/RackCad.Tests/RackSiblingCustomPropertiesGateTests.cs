using System.Collections.Generic;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.Views.Insertion;
using RackCad.Application.Views.Redraw;
using Xunit;

namespace RackCad.Tests
{
    public class RackSiblingCustomPropertiesGateTests
    {
        [Fact]
        public void Equivalent_mutable_members_proceed_and_read_only_members_are_outside_the_gate()
        {
            var membership = Membership();
            var absent = CustomPropertiesReadResult.Absent();
            var result = RackSiblingCustomPropertiesGate.Evaluate(membership,
                new Dictionary<string, CustomPropertiesReadResult> { ["A"] = absent, ["B"] = absent });
            Assert.True(result.Accepted);
        }

        [Fact]
        public void Divergent_or_unreadable_attributable_member_fails_closed()
        {
            var membership = Membership();
            var readable = new CustomPropertiesStore().Read(CustomPropertiesPayload.Present(
                "{\"SchemaVersion\":\"1.0\",\"Entries\":[]}"));
            var divergent = new CustomPropertiesStore().Read(CustomPropertiesPayload.Present(
                "{\"SchemaVersion\":\"1.0\",\"Entries\":[{\"Id\":\"11111111-1111-1111-1111-111111111111\",\"Name\":\"A\",\"Value\":\"1\"}]}"));

            var mismatch = RackSiblingCustomPropertiesGate.Evaluate(membership,
                new Dictionary<string, CustomPropertiesReadResult> { ["A"] = readable, ["B"] = divergent });
            Assert.False(mismatch.Accepted);
            Assert.Contains("DIVERGENT", mismatch.Diagnostic);

            var unreadable = RackSiblingCustomPropertiesGate.Evaluate(membership,
                new Dictionary<string, CustomPropertiesReadResult> { ["A"] = readable });
            Assert.False(unreadable.Accepted);
            Assert.Contains("UNREADABLE", unreadable.Diagnostic);
        }

        private static RackSiblingMembershipSnapshot Membership()
            => RackSiblingMembership.Classify(new[]
            {
                new RackSiblingScanFact("A", "frontal", true, false, true, "rack", "rack", 1, 0, false),
                new RackSiblingScanFact("B", "planta", false, false, true, "rack", "rack", 1, 0, false),
                new RackSiblingScanFact("X", "lateral", false, true, true, "rack", "rack", 1, 0, false),
            }, "rack", "rack", true);
    }
}
