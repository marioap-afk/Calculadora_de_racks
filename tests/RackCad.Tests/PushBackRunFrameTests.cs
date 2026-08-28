using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42, ERROR 5 y ERROR 10 — el MARCO DE LA CAMA es la única autoridad de colocación y orientación de sus
    /// piezas de extremo, y la misma cama produce la misma pieza en todas las salidas.
    ///
    /// <para>
    /// El dueño reportó largueros ALTOS invertidos «en frentes/topologías especiales» y una colocación de topes en
    /// planta que no se entendía. Las dos cosas tenían la misma causa: la planta pedía las piezas altas a la planta
    /// LOCAL del lado que posee el extremo alto. En una CORRIDA eso es falso — el larguero alto de una corrida no
    /// está en la línea posterior de ese lado, sino al final del recorrido, en el otro extremo del rack—, así que
    /// la planta lo dibujaba en la interfaz y con la orientación del marco contrario. El corte lateral, que sí
    /// resuelve por cama, decía otra cosa.
    /// </para>
    /// <para>
    /// Estas pruebas no miran la fórmula: comparan la PIEZA DIBUJADA contra el eje de la cama que
    /// <see cref="PushBackRunGeometry"/> publica, que es la autoridad neutral de «dónde empieza y dónde acaba esta
    /// cama».
    /// </para>
    /// </summary>
    public class PushBackRunFrameTests
    {
        private const string InOut = "LARGUERO_IN_OUT_C6";
        private const string Redondo = "LARGUERO_ESCALON_TROQUEL_REDONDO";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        /// <summary>
        /// Un rack compuesto con la topología pedida. <paramref name="deepA"/>/<paramref name="deepB"/> permiten
        /// frentes cortos y largos, y <paramref name="gap"/> una calle que atraviesa un hueco.
        /// </summary>
        private static PushBackSystem Build(
            PushBackCellTopology topology,
            PushBackRunDirection direction,
            int slots = 2,
            int levels = 2,
            int deepA = 4,
            int deepB = 4,
            double gap = 0.0,
            Action<PushBackCompositeEditorState> tweak = null)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(slots);
            state.SetGap(gap);
            state.SetDefaults(topology, direction);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }


            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(side).Structure;
                for (var slot = 0; slot < matrix.Count; slot++)
                {
                    state.Of(side).AdjustLevels(slot, levels - matrix.Fronts[slot].LoadLevels);
                    matrix.Fronts[slot].PalletsDeep = side == PushBackSide.A ? deepA : deepB;
                }
            }

            tweak?.Invoke(state);
            return new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;
        }

        public static IEnumerable<object[]> Topologies() => new[]
        {
            new object[] { PushBackCellTopology.SoloA, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.SoloB, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Encontradas, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.BToA },
        };

        /// <summary>
        /// Los casos ESPECIALES que el dueño enumeró: frente corto, frente largo, frente interior, último frente,
        /// hueco que la cama atraviesa y ranura presente en un solo lado. Cada uno se prueba en los dos sentidos.
        /// </summary>
        public static IEnumerable<object[]> Scenarios()
        {
            foreach (var topology in new[]
            {
                PushBackCellTopology.SoloA, PushBackCellTopology.SoloB,
                PushBackCellTopology.Encontradas, PushBackCellTopology.Corrida
            })
            {
                foreach (var direction in new[] { PushBackRunDirection.AToB, PushBackRunDirection.BToA })
                {
                    // frentes de fondos distintos: corto contra largo
                    yield return new object[] { topology, direction, 3, 4, 8, 0.0, false };
                    // hueco entre los dos lados: la corrida lo atraviesa
                    yield return new object[] { topology, direction, 2, 6, 6, 48.0, false };
                    // una ranura presente en un solo lado
                    yield return new object[] { topology, direction, 3, 5, 5, 0.0, true };
                }
            }
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void EveryEndBeam_BelongsToABed_InTheSpecialFronts(
            PushBackCellTopology topology,
            PushBackRunDirection direction,
            int slots,
            int deepA,
            int deepB,
            double gap,
            bool dropLastSlotOnB)
        {
            var system = Build(
                topology, direction, slots: slots, levels: 2, deepA: deepA, deepB: deepB, gap: gap,
                tweak: state =>
                {
                    if (dropLastSlotOnB)
                    {
                        state.SetSlotPresent(PushBackSide.B, slots - 1, false);
                    }
                });

            var axes = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog);
            if (axes.Count == 0)
            {
                return;   // configuración sin ninguna cama construible: nada que orientar
            }

            // Con ranuras HETEROGENEAS hay que mirar los CORTES por poste: el lateral general dibuja la
            // ENVOLVENTE del rack, no la cama de cada ranura, asi que una ranura mas corta no aparece alli.
            var builder = new PushBackSystemLateralBuilder();
            var cuts = builder.Cortes(system, Catalog);
            var lateral = Enumerable.Range(0, cuts.Count)
                .SelectMany(index => builder.Build(system, Catalog, index).Flatten().Instances)
                .ToList();

            foreach (var axis in axes)
            {
                Assert.True(
                    Of(lateral, InOut).Any(i => Math.Abs(i.Insertion.X - axis.LowContact.X) < 12.0
                        && i.MirroredX == !axis.FlowsForward),
                    $"{topology}/{direction} s{slots} {deepA}/{deepB} gap{gap}: falta el larguero BAJO orientado");
                // I-42 (correccion aislada 5): la mano del larguero ALTO ya no sale del sentido del flujo sino del
                // EXTREMO de la cabecera que lo recibe (ultimo poste = orientacion normal).
                Assert.True(
                    Of(lateral, Redondo).Any(i => Math.Abs(i.Insertion.X - axis.HighContact.X) < 12.0
                        && i.MirroredX == !PushBackHighEndHand.AtLastPost(system.Structure, i.Insertion.X)),
                    $"{topology}/{direction} s{slots} {deepA}/{deepB} gap{gap}: falta el larguero ALTO orientado");
            }

            // Y en PLANTA cada larguero alto y cada tope siguen perteneciendo al extremo alto de una cama real.
            foreach (var instance in Planta(system)
                         .Where(i => i.Role == HeaderBlockRole.Tope
                             || string.Equals(i.PieceId, Redondo, StringComparison.OrdinalIgnoreCase)))
            {
                Assert.True(
                    axes.Any(axis => Math.Abs(axis.HighContact.X - instance.Insertion.X) < 12.0),
                    $"{topology}/{direction} s{slots} {deepA}/{deepB} gap{gap}: pieza alta en planta a "
                        + $"X={instance.Insertion.X:0.###} sin cama que acabe ahí");
            }
        }

        private static IReadOnlyList<HeaderBlockInstance> Lateral(PushBackSystem system)
            => new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances;

        private static IReadOnlyList<HeaderBlockInstance> Planta(PushBackSystem system)
            => new PushBackSystemPlantaBuilder().Build(system, Catalog);

        private static IEnumerable<HeaderBlockInstance> Of(IEnumerable<HeaderBlockInstance> source, string pieceId)
            => source.Where(i => string.Equals(i.PieceId, pieceId, StringComparison.OrdinalIgnoreCase));

        // ===================== ERROR 5: la orientación sale del marco de la cama ==============================

        /// <summary>
        /// Los DOS largueros de extremo de una cama salen de SU marco, así que su mano la decide el SENTIDO FÍSICO
        /// del flujo y nada más.
        ///
        /// <para>
        /// La referencia es el Push Back de un solo sentido, que es la geometría que el dueño ya aprobó: una cama
        /// que fluye hacia +X lleva su larguero bajo sin espejo y el alto espejado, porque los dos se miran. Una
        /// cama reflejada —el lado B, o una corrida B→A— lleva exactamente lo contrario, que es lo que hace una
        /// reflexión rígida. Ni el lado, ni el índice del frente, ni «es el último módulo» entran en la decisión.
        /// </para>
        /// <para>
        /// Se comprueba por EXISTENCIA y no emparejando por cercanía: con camas encontradas los dos largueros altos
        /// caen en la misma línea de la interfaz —una por cada sentido— y distinguirlos por coordenada sería
        /// imposible. Lo que se exige es que en el extremo de cada cama haya un larguero con la mano que ese
        /// sentido impone.
        /// </para>
        /// </summary>
        [Theory]
        [MemberData(nameof(Topologies))]
        public void TheEndBeamsMirror_ComesFromTheFlowDirection(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var system = Build(topology, direction);
            var axes = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog);
            Assert.NotEmpty(axes);

            var lateral = Lateral(system);
            var lows = Of(lateral, InOut).ToList();
            var highs = Of(lateral, Redondo).ToList();
            Assert.NotEmpty(lows);
            Assert.NotEmpty(highs);

            foreach (var axis in axes)
            {
                Assert.True(
                    lows.Any(i => Math.Abs(i.Insertion.X - axis.LowContact.X) < 12.0
                        && i.MirroredX == !axis.FlowsForward),
                    $"{topology}/{direction}: sin larguero BAJO con la mano de su sentido en X={axis.LowContact.X:0.###}");
                // I-42 (correccion aislada 5) — EL ALTO YA NO SIGUE AL SENTIDO. Su mano la decide el EXTREMO fisico
                // de la cabecera que lo recibe: ultimo poste, orientacion normal; primer poste o poste interior,
                // invertido. Es la decision del dueño, y sustituye a la regla de un rack de un solo sentido que esta
                // prueba fijaba. El larguero BAJO no entra en esa correccion y conserva la suya.
                Assert.True(
                    highs.Any(i => Math.Abs(i.Insertion.X - axis.HighContact.X) < 12.0
                        && i.MirroredX == !PushBackHighEndHand.AtLastPost(system.Structure, i.Insertion.X)),
                    $"{topology}/{direction}: sin larguero ALTO con la mano de su extremo de cabecera en X={axis.HighContact.X:0.###}");
            }

            // Y nada fuera de los extremos de alguna cama: si sobra un larguero, alguien lo colocó por estructura.
            foreach (var beam in lows.Concat(highs))
            {
                Assert.True(
                    axes.Any(axis => Math.Abs(axis.LowContact.X - beam.Insertion.X) < 12.0
                        || Math.Abs(axis.HighContact.X - beam.Insertion.X) < 12.0),
                    $"{topology}/{direction}: larguero de extremo en X={beam.Insertion.X:0.###} sin cama que acabe ahí");
            }
        }

        /// <summary>
        /// Y la orientación del ALTO se sigue del SENTIDO FÍSICO, no del lado ni del índice del frente: una cama que
        /// fluye hacia +X tiene su alto con la mano contraria a una que fluye hacia −X.
        /// </summary>
        [Fact]
        public void TheHighBeamMirror_FollowsTheFlowDirection_NotTheSide()
        {
            var forward = Build(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            var backward = Build(PushBackCellTopology.Corrida, PushBackRunDirection.BToA);

            var forwardHigh = Of(Lateral(forward), Redondo).Select(i => i.MirroredX).Distinct().ToList();
            var backwardHigh = Of(Lateral(backward), Redondo).Select(i => i.MirroredX).Distinct().ToList();

            Assert.Single(forwardHigh);
            Assert.Single(backwardHigh);
            Assert.NotEqual(forwardHigh[0], backwardHigh[0]);
        }

        // ===================== ERROR 10: el tope pertenece a un HighSupport concreto ==========================

        /// <summary>
        /// PRUEBA VINCULANTE del punto 8 del dueño. En PLANTA, el larguero alto y su tope caen sobre el extremo
        /// ALTO REAL de la cama. Con una corrida, ese extremo está al otro lado del rack; la planta lo dibujaba en
        /// la interfaz porque preguntaba a la estructura del lado, no a la cama.
        /// </summary>
        [Theory]
        [MemberData(nameof(Topologies))]
        public void ThePlantaHighBeamAndItsTope_SitOnTheRealHighEnd(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var system = Build(topology, direction);
            var axes = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog);
            var planta = Planta(system);

            var highXs = Of(planta, Redondo).Select(i => Math.Round(i.Insertion.X, 3)).Distinct().ToList();
            var topeXs = planta
                .Where(i => i.Role == HeaderBlockRole.Tope)
                .Select(i => Math.Round(i.Insertion.X, 3))
                .Distinct()
                .ToList();

            Assert.NotEmpty(highXs);
            foreach (var x in highXs)
            {
                Assert.True(
                    axes.Any(axis => Math.Abs(axis.HighContact.X - x) < 12.0),
                    $"{topology}/{direction}: larguero alto en planta a X={x}, que no es el extremo alto de ninguna cama");
            }

            foreach (var x in topeXs)
            {
                Assert.True(
                    axes.Any(axis => Math.Abs(axis.HighContact.X - x) < 12.0),
                    $"{topology}/{direction}: tope en planta a X={x}, que no es el extremo alto de ninguna cama");
            }
        }

        /// <summary>
        /// Y la planta coincide con el LATERAL pieza a pieza: misma X y misma mano para el larguero alto. Si las dos
        /// vistas discrepan, una de las dos está inventando la posición.
        /// </summary>
        [Theory]
        [MemberData(nameof(Topologies))]
        public void ThePlantaAndTheLateral_AgreeOnTheHighBeam(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var system = Build(topology, direction);

            var lateral = Of(Lateral(system), Redondo)
                .Select(i => (X: Math.Round(i.Insertion.X, 3), i.MirroredX))
                .Distinct()
                .OrderBy(p => p.X).ThenBy(p => p.MirroredX)
                .ToList();
            var planta = Of(Planta(system), Redondo)
                .Select(i => (X: Math.Round(i.Insertion.X, 3), i.MirroredX))
                .Distinct()
                .OrderBy(p => p.X).ThenBy(p => p.MirroredX)
                .ToList();

            Assert.NotEmpty(lateral);
            Assert.Equal(lateral, planta);
        }

        /// <summary>
        /// El caso que hacía imposible ver el defecto contando piezas: una ranura con un nivel CORRIDO y otro
        /// ENCONTRADAS tiene tres largueros altos en tres sitios, no dos.
        /// </summary>
        [Fact]
        public void AMixedSlot_DrawsEachHighBeamWhereItsOwnBedEnds()
        {
            var system = Build(
                PushBackCellTopology.Corrida, PushBackRunDirection.AToB, slots: 1, levels: 2,
                tweak: state => state.SetCell(0, 1, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB));

            var axes = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog);
            var highs = Of(Planta(system), Redondo).Select(i => Math.Round(i.Insertion.X, 3)).OrderBy(x => x).ToList();

            // Dos camas encontradas (una por lado) más una corrida: TRES extremos altos distintos.
            Assert.Equal(3, axes.Count);
            Assert.Equal(3, highs.Count);
            foreach (var x in highs)
            {
                Assert.True(
                    axes.Any(axis => Math.Abs(axis.HighContact.X - x) < 12.0),
                    $"larguero alto a X={x} sin cama que acabe ahí");
            }

            // Y los tres no están en el mismo sitio: dos en la interfaz y uno al final de la corrida.
            Assert.True(highs.Distinct().Count() >= 2);
        }

        /// <summary>
        /// El BOM cuenta UN tope por CAMA con intención activa en su lado ALTO, ni uno más (punto 10 del dueño).
        ///
        /// <para>
        /// La cuenta esperada se deriva de las dos autoridades de usuario —<see cref="PushBackRuns"/>, que enumera
        /// las camas físicas, y la intención por lado guardada en el compuesto—, no de la fórmula del BOM. Ninguna
        /// VISTA sirve para esta comparación: el lateral general colapsa las ranuras (todas se proyectan sobre el
        /// mismo plano), los cortes por poste muestran cada frente en los dos cortes que lo flanquean, y la planta
        /// colapsa los niveles. Que cada pieza dibujada esté en el extremo alto de una cama real se comprueba
        /// aparte.
        /// </para>
        /// </summary>
        [Theory]
        [MemberData(nameof(Topologies))]
        public void TheBom_CountsOneTopePerBedWithIntent(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var system = Build(topology, direction);
            var expected = PushBackRuns.Resolve(system).Runs
                .Count(run => system.Composite.Of(run.HighSide)?.RearTope?.At(run.Slot, run.Level - 1) == true);
            var counted = PushBackBomBuilder.Build(system, Catalog).Components
                .Where(c => string.Equals(c.Category, PushBackBomBuilder.RearTope, StringComparison.Ordinal))
                .Sum(c => c.Quantity);

            Assert.True(expected > 0, "el escenario tiene que traer topes activos");
            Assert.Equal(expected, counted);
        }

        /// <summary>
        /// Y la PLANTA proyecta uno por línea física: el mismo tope que el lateral dibuja en cada nivel de esa cama,
        /// colapsado. Ni más —no inventa uno por nivel— ni menos.
        /// </summary>
        [Theory]
        [MemberData(nameof(Topologies))]
        public void ThePlantaTopes_AreTheCollapsedLateralOnes(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var system = Build(topology, direction);

            var lateral = Lateral(system)
                .Where(i => i.Role == HeaderBlockRole.Tope)
                .Select(i => Math.Round(i.Insertion.X, 3))
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            var planta = Planta(system)
                .Where(i => i.Role == HeaderBlockRole.Tope)
                .Select(i => Math.Round(i.Insertion.X, 3))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            Assert.NotEmpty(lateral);
            Assert.Equal(lateral, planta);
        }
    }
}
