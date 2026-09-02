using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (H17, contrato del dueño) — LA ENVOLVENTE DEL LATERAL GENERAL SALE DE LO QUE EL RACK REALMENTE ALMACENA.
    ///
    /// <para>
    /// El lateral no seccionado dibuja UNA ranura: la envolvente. El criterio historico era el mayor tramo
    /// longitudinal, y una ranura EN BLANCO por los dos lados conserva su claro y su estructura —eso es correcto y
    /// no se toca (A1B-D5)— pero no almacena nada. Con una blank mas profunda que las activas, ese argmax elegia la
    /// blank: el corte quedaba gobernado por una ranura sin cama mientras la ranura activa no salia.
    /// </para>
    ///
    /// <para>
    /// La regla vigente pregunta por las CAMAS FISICAS (<see cref="PushBackRuns"/>) y elige la envolvente entre las
    /// ranuras que tienen alguna; sin ninguna, el criterio de siempre. Esta prueba es la RED DE REGRESION de esa
    /// regla: fija el caso completo —premisa incluida— en vez de comprobar solo que existen camas.
    /// </para>
    ///
    /// <para>
    /// <b>La premisa se valida sola.</b> El fixture solo prueba algo mientras la blank SIGA ganando el argmax
    /// historico; si un cambio futuro de fixture dejara de cumplirlo, la prueba falla en vez de volverse una que ya
    /// no muerde.
    /// </para>
    /// </summary>
    public class PushBackGeneralLateralActiveEnvelopeTests
    {
        private const int BlankSlot = 0;
        private const int ActiveASlot = 1;
        private const int ActiveBSlot = 2;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>
        /// Tres ranuras: la 0 en blanco por los DOS lados y la mas profunda —9 fondos frente a 4—, la 1 solo del
        /// lado A y la 2 solo del lado B.
        /// </summary>
        private static PushBackSystem Build(bool activate = true)
        {
            const int slots = 3;
            const int levels = 2;
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: slots, slotsB: slots, deepA: 4, deepB: 4, levelsA: levels, levelsB: levels, gap: 0.0);
            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;

            // La blank es la ranura mas larga: es lo que le da el argmax historico.
            var deep = new[] { 6, 4, 3 };
            for (var slot = 0; slot < slots; slot++)
            {
                design.Structure.Fronts[slot].PalletsDeep = deep[slot];
                design.SideB.Fronts[slot].PalletsDeep = deep[slot];
            }

            design.Composite.AbsentSlotsA.Add(BlankSlot);
            design.Composite.AbsentSlotsB.Add(BlankSlot);
            if (activate)
            {
                design.Composite.AbsentSlotsB.Add(ActiveASlot);   // ranura 1: solo lado A
                design.Composite.AbsentSlotsA.Add(ActiveBSlot);   // ranura 2: solo lado B
            }
            else
            {
                // Control: NINGUNA ranura almacena.
                design.Composite.AbsentSlotsA.Add(ActiveASlot);
                design.Composite.AbsentSlotsB.Add(ActiveASlot);
                design.Composite.AbsentSlotsA.Add(ActiveBSlot);
                design.Composite.AbsentSlotsB.Add(ActiveBSlot);
            }

            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static double Span(PushBackSystem system, int slot)
        {
            var front = system.Structure.Fronts[slot];
            return front.EndX - front.StartX;
        }

        /// <summary>La ranura que el criterio HISTORICO habria elegido: el mayor tramo, mire o no si almacena.</summary>
        private static int LegacyArgMax(PushBackSystem system)
            => system.Structure.Fronts
                .OrderByDescending(front => front.EndX - front.StartX)
                .ThenBy(front => front.Index)
                .First()
                .Index;

        private static int RunsOf(PushBackSystem system, int slot)
            => PushBackRuns.Resolve(system).Runs.Count(run => run.Slot == slot);

        /// <summary>El lateral GENERAL, por su ruta final: la que consume el dibujo.</summary>
        private static HeaderRunPlan General(PushBackSystem system)
            => new PushBackSystemLateralBuilder().Build(system, Catalog);

        /// <summary>La marca con la que una cama declara de QUE frente es: la misma que compone el lateral.</summary>
        private static string BedMark(int slot)
            => " F" + (slot + 1).ToString(CultureInfo.InvariantCulture);

        private static IReadOnlyList<string> BedGroups(HeaderRunPlan plan)
            => plan.Headers
                .Select(group => group.Name)
                .Where(name => name != null && name.StartsWith("Cama push back", StringComparison.Ordinal))
                .ToList();

        // ---------------------------------------------------------------- la regresion de H17

        [Fact]
        public void GeneralLateral_BlankSlotWithLongestSpanDoesNotGovernTheEnvelope()
        {
            var system = Build();

            // --- premisa, comprobada y no supuesta: sin esto la prueba no probaria nada.
            Assert.Equal(BlankSlot, LegacyArgMax(system));
            Assert.True(
                Span(system, BlankSlot) > Span(system, ActiveASlot),
                FormattableString.Invariant(
                    $"la blank ({Span(system, BlankSlot):0.##}) debe ser mas larga que la activa ({Span(system, ActiveASlot):0.##})"));
            Assert.Equal(0, RunsOf(system, BlankSlot));
            Assert.True(RunsOf(system, ActiveASlot) >= 1);
            Assert.True(RunsOf(system, ActiveBSlot) >= 1);

            // --- el consumidor final: el lateral general, por su ruta de dibujo.
            var beds = BedGroups(General(system));

            Assert.NotEmpty(beds);
            Assert.All(beds, name => Assert.Contains(BedMark(ActiveASlot), name, StringComparison.Ordinal));
            Assert.DoesNotContain(beds, name => name.Contains(BedMark(BlankSlot), StringComparison.Ordinal));
        }

        [Fact]
        public void GeneralLateral_ChosenEnvelopeShowsTheActiveSlotSide()
        {
            var system = Build();
            var plan = General(system);

            // La ranura elegida es de UN lado —la 1 es solo del lado A—, y el corte lo dice con su letra.
            var letters = plan.Flatten().Instances
                .Where(instance => !string.IsNullOrWhiteSpace(instance.Text))
                .Select(instance => instance.Text.Trim())
                .Where(text => text.Length == 1)
                .Distinct()
                .ToList();

            Assert.Contains("A", letters);
            Assert.DoesNotContain("B", letters);
        }

        [Fact]
        public void GeneralLateral_EnvelopeCarriesTheGeometryOfTheActiveBed()
        {
            var system = Build();
            var plan = General(system);

            // La cama dibujada es la de la ranura activa: su longitud es la de ESA ranura, no la de la blank.
            var bed = plan.Headers.Single(group => group.Name.StartsWith("Cama push back", StringComparison.Ordinal));
            var railLength = bed.Instances
                .Select(instance => instance.DynamicParameters.TryGetValue("LONGITUD", out var value) ? value : 0.0)
                .DefaultIfEmpty(0.0)
                .Max();

            Assert.True(railLength > 0.0);
            Assert.True(
                railLength < Span(system, BlankSlot),
                FormattableString.Invariant(
                    $"la cama dibujada ({railLength:0.##}) no puede ser la de la ranura en blanco ({Span(system, BlankSlot):0.##})"));
        }

        // ---------------------------------------------------------------- la rama de reserva, declarada y medida

        [Fact]
        public void GeneralLateral_ARackWithNoStoringSlotIsRejectedBeforeItCanBeDrawn()
        {
            // La regla vigente deja una reserva: «sin ninguna cama, el criterio de siempre». Medido, ese estado NO
            // es alcanzable por el resolver —un rack con todos los frentes en blanco se rechaza—, asi que la reserva
            // es codigo defensivo y no una rama que este corte pueda tomar. Se fija el hecho, que es lo que explica
            // por que no hay un test de esa rama.
            var error = Record.Exception(() => Build(activate: false));

            Assert.IsType<ArgumentException>(error);
            Assert.Contains("al menos un frente", error.Message, StringComparison.OrdinalIgnoreCase);
        }

    }
}
