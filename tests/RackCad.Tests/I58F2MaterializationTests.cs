using System;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.PushBack;
using Xunit;
using static RackCad.Tests.I58F1Fixtures;
using static RackCad.Tests.I58F1Oracles;
using O = RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;
namespace RackCad.Tests;

public sealed class I58F2MaterializationTests
{
    [Theory]
    [InlineData("dynamic")]
    [InlineData("pushback-simple")]
    [InlineData("pushback-composite")]
    public void Real_Single_keeps_fallback_local_parity_and_provenance(string mode)
    {
        if (mode == "dynamic")
        {
            var original = Dynamic();
            string raw = Store.Serialize(RackProject.ForDynamic(original));
            var r = RackAuthoredComparatorPorts.Dynamic().Compare(ProductInput(Input(Sibling("dynamic", raw)), "dynamic"));
            Assert.Equal(O.Single, r.Outcome);
            Assert.Equal(7, DynamicPeralte(r.Authored));
            DynamicParity(original, r.Authored);
            Assert.Empty(RetentionViolations(original, r.Authored));
        }
        else
        {
            var original = mode == "pushback-composite" ? Composite() : new PushBackDesign { Structure = Dynamic() };
            string raw = Store.Serialize(RackProject.ForPushBack(original));
            var r = RackAuthoredComparatorPorts.PushBack().Compare(ProductInput(Input(Sibling("pushback", raw)), "pushback"));
            Assert.Equal(O.Single, r.Outcome);
            var resolved = PushBackPeraltes(r.Authored);
            Assert.Equal(7, resolved.Rack);
            if (mode == "pushback-composite") { Assert.Equal(7, resolved.A); Assert.Equal(9, resolved.B); }
            PushBackParity(original, r.Authored);
            Assert.Empty(RetentionViolations(original.Structure, r.Authored.Structure));
        }
    }
}
