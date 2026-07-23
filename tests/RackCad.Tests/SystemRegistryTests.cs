using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// F2 for I-08: the common system descriptor and the SystemRegistry core. These tests pin the registry shape the later
    /// phases will migrate onto (persistence, validation, design library + label) without touching any consumer yet.
    /// </summary>
    public class SystemRegistryTests
    {
        // Deliberate order = RackSystemKind declaration order.
        private static readonly RackSystemKind[] ExpectedOrder =
        {
            RackSystemKind.Selective,
            RackSystemKind.PalletFlow,
            RackSystemKind.SelectiveRack,
            RackSystemKind.Cama,
            RackSystemKind.Larguero,
            RackSystemKind.PushBack,
        };

        [Fact]
        public void Default_ContainsExactlyTheSixKinds_NoneMissingNoDuplicates()
        {
            var kinds = SystemRegistry.Default.Descriptors.Select(d => d.Kind).ToArray();

            Assert.Equal(6, kinds.Length);
            Assert.Equal(kinds.Length, kinds.Distinct().Count()); // no duplicates
            Assert.Equal(
                Enum.GetValues(typeof(RackSystemKind)).Cast<RackSystemKind>().ToHashSet(),
                kinds.ToHashSet()); // no kind omitted
        }

        [Fact]
        public void Default_EnumerationOrderIsStableAndDeliberate()
        {
            Assert.Equal(ExpectedOrder, SystemRegistry.Default.Descriptors.Select(d => d.Kind).ToArray());
            // Deliberate = mirrors the enum's own declaration order.
            Assert.Equal(
                Enum.GetValues(typeof(RackSystemKind)).Cast<RackSystemKind>().ToArray(),
                SystemRegistry.Default.Descriptors.Select(d => d.Kind).ToArray());
        }

        [Theory]
        [InlineData(RackSystemKind.Selective, "Cabecera")]
        [InlineData(RackSystemKind.PalletFlow, "Sistema dinámico")]
        [InlineData(RackSystemKind.SelectiveRack, "Selectivo")]
        [InlineData(RackSystemKind.Cama, "Cama de rodamiento")]
        [InlineData(RackSystemKind.Larguero, "Larguero")]
        [InlineData(RackSystemKind.PushBack, "Push Back")]
        public void Default_EachKind_ReturnsExactVisibleLabel(RackSystemKind kind, string expectedLabel)
        {
            Assert.Equal(expectedLabel, SystemRegistry.Default.Get(kind).LibraryLabel);
        }

        [Fact]
        public void Get_ReturnsTheDescriptorForEachKind()
        {
            foreach (var kind in ExpectedOrder)
            {
                Assert.Equal(kind, SystemRegistry.Default.Get(kind).Kind);
            }
        }

        [Fact]
        public void Get_UnregisteredKind_FailsExplicitly()
        {
            var partial = new SystemRegistry(new[] { new SystemDescriptor(RackSystemKind.Selective, "Cabecera") });

            var ex = Assert.Throws<InvalidOperationException>(() => partial.Get(RackSystemKind.Cama));
            Assert.Contains("Cama", ex.Message);
            Assert.False(partial.TryGet(RackSystemKind.Cama, out _));
        }

        [Fact]
        public void Construct_WithDuplicateKind_Throws()
        {
            Assert.Throws<ArgumentException>(() => new SystemRegistry(new[]
            {
                new SystemDescriptor(RackSystemKind.Cama, "Cama de rodamiento"),
                new SystemDescriptor(RackSystemKind.Cama, "otra"),
            }));
        }

        [Fact]
        public void Registry_DoesNotMutateAfterConstruction()
        {
            var source = new[]
            {
                new SystemDescriptor(RackSystemKind.Selective, "Cabecera"),
                new SystemDescriptor(RackSystemKind.PalletFlow, "Sistema dinámico"),
            };
            var registry = new SystemRegistry(source);

            source[0] = new SystemDescriptor(RackSystemKind.Cama, "Cama de rodamiento"); // mutate the caller's array

            Assert.Equal(2, registry.Descriptors.Count);
            Assert.Equal(RackSystemKind.Selective, registry.Descriptors[0].Kind);
            Assert.True(registry.TryGet(RackSystemKind.Selective, out _));
            Assert.False(registry.TryGet(RackSystemKind.Cama, out _));

            // The exposed collection is read-only: there is no cast-to-mutable path.
            Assert.IsNotType<List<SystemDescriptor>>(registry.Descriptors);
        }

        [Fact]
        public void Descriptor_RejectsEmptyLabel()
        {
            Assert.Throws<ArgumentException>(() => new SystemDescriptor(RackSystemKind.Selective, ""));
            Assert.Throws<ArgumentException>(() => new SystemDescriptor(RackSystemKind.Selective, "   "));
            Assert.Throws<ArgumentException>(() => new SystemDescriptor(RackSystemKind.Selective, null));
        }
    }
}
