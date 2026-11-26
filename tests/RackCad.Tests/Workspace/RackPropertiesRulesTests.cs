using System;
using System.Linq;
using RackCad.Application.CustomProperties;
using RackCad.Application.VentanaRackCad;
using Xunit;
using static RackCad.Tests.CustomPropertiesAuthorityTestKit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 A-14, cuarta tanda (va), VA-07 and VA-08: the tab «Propiedades» of an editor of the RackCad window reads the custom properties of
    /// the rack with the authority of RACKPROPIEDADES, and «Actualizar» writes its whole draft in every view, only over what it read; views
    /// with different properties are unified from a safe source.
    /// </summary>
    public class RackPropertiesRulesTests
    {
        private static RackCad.Application.CustomProperties.RackCustomPropertiesAuthorityResult Single()
            => Authority("H1",
                View("H1", properties: Props(Entry(IdA, "Zona", "A"))),
                View("H2", properties: Props(Entry(IdA, "Zona", "A")), view: "lateral", section: 0));

        private static RackCad.Application.CustomProperties.RackCustomPropertiesAuthorityResult Divergent()
            => Authority("H1",
                View("H1", properties: Props(Entry(IdA, "Zona", "B"))),
                View("H2", properties: Props(), view: "planta"));

        [Fact]
        public void ARackWithOneCollectionIsReadAsItsDraft()
        {
            var read = RackPropertiesRules.Read(Single());

            Assert.True(read.Writable);
            Assert.False(read.Divergent);
            Assert.Equal(2, read.ViewCount);
            var entry = Assert.Single(read.Entries);
            Assert.Equal(("Zona", "A"), (entry.Name, entry.Value));
            Assert.NotNull(read.Fingerprint);
        }

        [Fact]
        public void TheDraftIsWrittenInEveryViewOnlyOverWhatWasRead()
        {
            var authority = Single();
            var read = RackPropertiesRules.Read(authority);
            var write = new RackPropertiesWrite(read.Fingerprint, new[]
            {
                new CustomPropertyDraftEntry(IdA, "Zona", "C"),
                new CustomPropertyDraftEntry(null, "Nota", "x"),
            });

            var plan = RackPropertiesRules.Plan(authority, write);
            Assert.True(plan.IsPlanned, plan.Error);
            Assert.Equal(new[] { "H1", "H2" }, plan.Plan.Select(p => p.Handle).OrderBy(h => h).ToArray());
            var after = Authority(AfterPlan(new[]
            {
                View("H1", properties: Props(Entry(IdA, "Zona", "A"))),
                View("H2", properties: Props(Entry(IdA, "Zona", "A")), view: "lateral", section: 0),
            }, plan), "H1");
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, after.Outcome);
            Assert.Equal(new[] { "Zona", "Nota" }, after.Collection.Document.Entries.Select(e => e.Name).ToArray());

            // Another write changed them since: nothing is planned.
            var moved = Authority("H1",
                View("H1", properties: Props(Entry(IdA, "Zona", "Z"))),
                View("H2", properties: Props(Entry(IdA, "Zona", "Z")), view: "lateral", section: 0));
            var refused = RackPropertiesRules.Plan(moved, write);
            Assert.False(refused.IsPlanned);
            Assert.Equal(RackPropertiesRules.ChangedText, refused.Error);
            Assert.Empty(refused.Plan);
        }

        [Fact]
        public void ADraftThatBreaksARuleWritesNothing()
        {
            var authority = Single();
            var read = RackPropertiesRules.Read(authority);
            var plan = RackPropertiesRules.Plan(authority, new RackPropertiesWrite(read.Fingerprint, new[]
            {
                new CustomPropertyDraftEntry(IdA, "Zona", "A"),
                new CustomPropertyDraftEntry(null, "zona", "dup"),
            }));

            Assert.False(plan.IsPlanned);
            Assert.Empty(plan.Plan);
        }

        [Fact]
        public void ViewsWithDifferentPropertiesAreUnifiedFromASafeSourceIntoEveryView()
        {
            var authority = Divergent();
            var read = RackPropertiesRules.Read(authority);

            Assert.False(read.Writable);
            Assert.True(read.Divergent);
            Assert.Equal(new[] { "Vista frontal", "Vista en planta" }, read.Views.Select(v => v.Label).ToArray());
            Assert.Equal(new[] { "Zona — B", "vacío / sin propiedades" }, read.Views.Select(v => v.Content).ToArray());
            var source = read.Views[0];
            Assert.True(source.CanBeSource);
            Assert.Equal("Zona", Assert.Single(source.Entries).Name);

            // Without a unify the draft of a divergent rack is not written.
            Assert.False(RackPropertiesRules.Plan(authority, new RackPropertiesWrite(read.Fingerprint, source.Entries)).IsPlanned);

            var plan = RackPropertiesRules.Plan(authority, new RackPropertiesWrite(read.Fingerprint, source.Entries, source.Handle));
            Assert.True(plan.IsPlanned, plan.Error);
            Assert.Equal(new[] { "H1", "H2" }, plan.Plan.Select(p => p.Handle).OrderBy(h => h).ToArray());
        }
    }
}
