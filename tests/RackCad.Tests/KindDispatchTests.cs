using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Pure coverage (no AutoCAD, ADR-0003) for the kind-dispatch mechanism the Plugin's KindHandlerRegistry wraps:
    /// registration invariants (null / blank / duplicate rejection), ordinal vs case-insensitive lookup, the
    /// negative (missing-key) path, immutability of the exposed collection, and the single historic "unrecognized
    /// kind" message.
    /// </summary>
    public class KindDispatchTests
    {
        private sealed class Item
        {
            public Item(string kind) => Kind = kind;
            public string Kind { get; }
        }

        private static KindDispatch<Item> Dispatch(params string[] kinds) =>
            new KindDispatch<Item>(kinds.Select(k => new Item(k)).ToArray(), item => item.Kind);

        // Mirrors the four embedded kinds the Plugin registers, in canonical order.
        private static KindDispatch<Item> FourKinds() => Dispatch(
            RackEmbedDocument.KindSelective,
            RackEmbedDocument.KindDynamic,
            RackEmbedDocument.KindCabecera,
            RackEmbedDocument.KindCama);

        [Fact]
        public void Items_AreInDeclarationOrder()
        {
            var dispatch = FourKinds();

            Assert.Equal(
                new[] { "selective", "dynamic", "cabecera", "cama" },
                dispatch.Items.Select(item => item.Kind).ToArray());
        }

        [Fact]
        public void TryGet_IsCaseSensitive_MirroringTheSwitch()
        {
            var dispatch = FourKinds();

            Assert.True(dispatch.TryGet(RackEmbedDocument.KindSelective, out var hit));
            Assert.Equal("selective", hit.Kind);

            // A different casing is NOT a match for the ordinal lookup (the historic `switch (kind)` behaviour).
            Assert.False(dispatch.TryGet("Selective", out var miss));
            Assert.Null(miss);
        }

        [Fact]
        public void TryGetIgnoreCase_MatchesAnyCasing()
        {
            var dispatch = FourKinds();

            Assert.True(dispatch.TryGetIgnoreCase("CABECERA", out var hit));
            Assert.Equal("cabecera", hit.Kind);
            Assert.True(dispatch.TryGetIgnoreCase(RackEmbedDocument.KindCama, out _));
        }

        [Fact]
        public void TryGet_UnknownOrNullKind_ReturnsFalseAndNull()
        {
            var dispatch = FourKinds();

            Assert.False(dispatch.TryGet("larguero", out var larguero)); // Larguero is deliberately unregistered
            Assert.Null(larguero);
            Assert.False(dispatch.TryGet("noSuchKind", out _));
            Assert.False(dispatch.TryGet(null, out var forNull));
            Assert.Null(forNull);
            Assert.False(dispatch.TryGetIgnoreCase(null, out _));
        }

        [Fact]
        public void Construct_RejectsDuplicateKey_Exact()
        {
            var ex = Assert.Throws<ArgumentException>(() => Dispatch("selective", "selective"));
            Assert.Contains("selective", ex.Message);
        }

        [Fact]
        public void Construct_RejectsDuplicateKey_CaseVariant()
        {
            // A case-variant duplicate would make the ignore-case lookup ambiguous.
            Assert.Throws<ArgumentException>(() => Dispatch("selective", "SELECTIVE"));
        }

        [Fact]
        public void Construct_RejectsNullItem()
        {
            Assert.Throws<ArgumentException>(() =>
                new KindDispatch<Item>(new[] { new Item("selective"), null }, item => item.Kind));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Construct_RejectsBlankKey(string kind)
        {
            Assert.Throws<ArgumentException>(() => Dispatch(kind));
        }

        [Fact]
        public void Construct_RejectsNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new KindDispatch<Item>(null, item => item.Kind));
            Assert.Throws<ArgumentNullException>(() => new KindDispatch<Item>(Array.Empty<Item>(), null));
        }

        [Fact]
        public void Items_AreImmutable_NoCastToMutablePath()
        {
            var source = new List<Item> { new Item("selective"), new Item("dynamic") };
            var dispatch = new KindDispatch<Item>(source, item => item.Kind);

            source.Add(new Item("cabecera")); // mutating the caller's list must not leak in
            Assert.Equal(2, dispatch.Items.Count);

            // The exposed collection is genuinely read-only: it is not the backing List and IList.Add is rejected.
            Assert.IsNotType<List<Item>>(dispatch.Items);
            Assert.Throws<NotSupportedException>(() => ((IList<Item>)dispatch.Items).Add(new Item("cama")));
        }

        [Theory]
        [InlineData("noSuchKind", "RackCad: tipo de rack no reconocido (noSuchKind).")]
        [InlineData("", "RackCad: tipo de rack no reconocido ().")]
        [InlineData(null, "RackCad: tipo de rack no reconocido ().")]
        public void NotRecognizedMessage_IsTheHistoricWording(string kind, string expected)
        {
            Assert.Equal(expected, KindDispatchMessages.NotRecognized(kind));
        }

        // TryResolveAll is the preflight seam RACKBOMTOTAL uses to ABORT the whole command (never a partial BOM)
        // the moment any placed rack's kind has no handler.
        [Fact]
        public void TryResolveAll_AllResolve_ReturnsItemsAlignedToInput()
        {
            var dispatch = FourKinds();

            var ok = dispatch.TryResolveAll(
                new[] { RackEmbedDocument.KindCabecera, RackEmbedDocument.KindSelective }, out var items, out var missing);

            Assert.True(ok);
            Assert.Null(missing);
            Assert.Equal(new[] { "cabecera", "selective" }, items.Select(i => i.Kind).ToArray());
        }

        [Fact]
        public void TryResolveAll_AnyMissing_ReturnsFalseWithFirstUnresolved()
        {
            var dispatch = FourKinds();

            var ok = dispatch.TryResolveAll(
                new[] { RackEmbedDocument.KindSelective, "larguero", RackEmbedDocument.KindCama }, out var items, out var missing);

            Assert.False(ok);
            Assert.Null(items);
            Assert.Equal("larguero", missing); // the FIRST unresolved key, so the command aborts up front
        }

        [Fact]
        public void TryResolveAll_IsOrdinal_LikeTheBomSwitch()
        {
            var dispatch = FourKinds();

            Assert.False(dispatch.TryResolveAll(new[] { "Selective" }, out _, out var missing));
            Assert.Equal("Selective", missing);
        }

        [Fact]
        public void TryResolveAll_RepeatedKinds_ResolveEach()
        {
            var dispatch = FourKinds();

            var ok = dispatch.TryResolveAll(
                new[] { RackEmbedDocument.KindSelective, RackEmbedDocument.KindSelective }, out var items, out _);

            Assert.True(ok);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void TryResolveAll_Empty_ReturnsTrueWithNoItems()
        {
            Assert.True(FourKinds().TryResolveAll(Array.Empty<string>(), out var items, out var missing));
            Assert.Empty(items);
            Assert.Null(missing);
        }

        [Fact]
        public void TryResolveAll_NullKinds_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => FourKinds().TryResolveAll(null, out _, out _));
        }
    }
}
