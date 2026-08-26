using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (3a validacion) — OWNERSHIP FISICO de la seguridad: una pieza fisica, una identidad, una
    /// materializacion, una linea de BOM.
    ///
    /// <para>
    /// La ronda anterior expreso «dos pasillos» escribiendo <c>Side = Both</c> en cada seleccion. Eso APAGA las
    /// reglas ADAPTATIVAS —que solo valen cuando el usuario no ha elegido lado—, asi que el protector lateral, que
    /// legacy pone SOLO en los postes de las orillas, aparecia en TODOS y por duplicado. Pertenencia, orientacion y
    /// extremo son tres ejes distintos y ninguno puede hablar por otro.
    /// </para>
    /// </summary>
    public class PushBackCompositeSafetyOwnershipTests
    {
        private const int Fronts = 4;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                inputs.SafetySelections.Add(selection);
            }

            return inputs;
        }

        private static PushBackCompositeEditorState State(
            bool sideB, PushBackCellTopology topology = PushBackCellTopology.Encontradas)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(Fronts);
            if (sideB)
            {
                state.SetSideBPresent(true);
                state.SideB.LoadNew();
                state.SetSlotCount(Fronts);
                state.SetDefaults(topology, PushBackRunDirection.AToB);
                // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
                // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
                for (var declared = 0; declared < state.SlotCount; declared++)
                {
                    state.SetSlotPresent(PushBackSide.B, declared, true);
                }

            }

            return state;
        }

        private static PushBackSystem Resolve(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        /// <summary>El id de catalogo de una familia de seguridad, o null si el catalogo no la trae.</summary>
        private static string ElementOf(string family)
            => Catalog.SafetyElements?
                .FirstOrDefault(entry => entry != null && SelectiveSafetyDefaults.IsType(entry.Type, family))?.Id;

        /// <summary>Las instancias DIBUJADAS de una familia, en el lateral general.</summary>
        private static IReadOnlyList<HeaderBlockInstance> Drawn(PushBackSystem system, string family)
        {
            var id = ElementOf(family);
            if (string.IsNullOrWhiteSpace(id))
            {
                return new List<HeaderBlockInstance>();
            }

            // La PLANTA es donde se ve la retícula completa: una fila por linea de postes. El lateral general es UNA
            // elevacion y colapsa esa dimension, asi que ahi no se puede contar «en que postes» cae una pieza.
            return new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Where(instance => instance.Role == HeaderBlockRole.Safety)
                .Where(instance => string.Equals(instance.PieceId, id, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // ================= D: protectores SOLO en las orillas ===================================================

        /// <summary>
        /// PRUEBA VINCULANTE. El protector lateral es de ORILLA: legacy lo pone en el primer poste y en el ultimo, y
        /// en ninguno interior. Un rack compuesto tiene dos caras de carga, y eso cambia DONDE cae la copia lejana —
        /// no a cuantos postes llega la pieza.
        /// </summary>
        [Fact]
        public void SideProtectors_OnlyReachTheEdgePostLines()
        {
            var legacy = Drawn(Resolve(State(sideB: false)), SelectiveSafetyDefaults.LateralType);
            Assert.NotEmpty(legacy);

            var composite = Resolve(State(sideB: true));
            var drawn = Drawn(composite, SelectiveSafetyDefaults.LateralType);
            Assert.NotEmpty(drawn);

            var lines = TransverseLines(composite);
            Assert.True(lines.Count >= 5, "el fixture tiene que traer lineas interiores");

            var first = lines.First();
            var last = lines.Last();

            // Ni una sola pieza en una linea INTERIOR. Con «protector en todos los postes» esto falla de inmediato.
            Assert.All(drawn, instance =>
            {
                var y = Math.Round(instance.Insertion.Y, 3);
                var atEdge = Math.Abs(y - first) < 1e-3 || Math.Abs(y - last) < 1e-3;
                Assert.True(atEdge, "protector en una linea transversal interior: y = " + y);
            });
        }

        /// <summary>
        /// Y son EXACTAMENTE dos, uno por linea de orilla, tanto en un rack de un sentido como en uno compuesto. En
        /// planta el protector se dibuja como una barra a lo largo de toda la profundidad de su linea, asi que una
        /// por orilla cubre las dos caras de carga. El defecto era que aparecia en las CINCO lineas.
        /// </summary>
        [Fact]
        public void SideProtectors_AreExactlyOnePerEdgeLine()
        {
            foreach (var sideB in new[] { false, true })
            {
                var system = Resolve(State(sideB));
                var drawn = Drawn(system, SelectiveSafetyDefaults.LateralType);
                var lines = TransverseLines(system);

                Assert.True(lines.Count >= 5, "el fixture de 4 frentes tiene 5 lineas transversales");
                Assert.Equal(2, drawn.Count);

                var ys = drawn.Select(instance => Math.Round(instance.Insertion.Y, 3)).OrderBy(y => y).ToList();
                Assert.Equal(Math.Round(lines.First(), 3), ys[0], 3);
                Assert.Equal(Math.Round(lines.Last(), 3), ys[1], 3);
            }
        }

        /// <summary>Las Y DISTINTAS de las lineas transversales de postes: una por frontera de frente.</summary>
        private static IReadOnlyList<double> TransverseLines(PushBackSystem system)
            => new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Select(instance => Math.Round(instance.Insertion.Y, 3))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

        // ================= C: ninguna pieza fisica materializada dos veces ======================================

        /// <summary>
        /// PRUEBA DE OWNERSHIP. Dos piezas de seguridad no pueden ocupar el MISMO sitio con la MISMA identidad y la
        /// MISMA orientacion: eso es una pieza contada dos veces, aunque graficamente se superpongan y no se note.
        /// </summary>
        [Theory]
        [InlineData(PushBackCellTopology.SoloA)]
        [InlineData(PushBackCellTopology.SoloB)]
        [InlineData(PushBackCellTopology.Encontradas)]
        [InlineData(PushBackCellTopology.Corrida)]
        public void NoSafetyPiece_IsMaterializedTwiceInTheSamePlace(PushBackCellTopology topology)
        {
            var instances = new PushBackSystemPlantaBuilder()
                .Build(Resolve(State(sideB: true, topology)), Catalog)
                .Where(instance => instance.Role == HeaderBlockRole.Safety)
                .ToList();

            Assert.NotEmpty(instances);

            var duplicates = instances
                .GroupBy(instance => string.Join(
                    "|",
                    instance.PieceId,
                    Math.Round(instance.Insertion.X, 3),
                    Math.Round(instance.Insertion.Y, 3),
                    instance.MirroredX,
                    Math.Round(instance.RotationRadians, 6)))
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            Assert.True(
                duplicates.Count == 0,
                "piezas de seguridad duplicadas en el mismo sitio: " + string.Join(" ; ", duplicates));
        }

        /// <summary>
        /// El BOM del PROTECTOR LATERAL cuenta exactamente las piezas que se materializan: dos, una por linea de
        /// orilla, tanto de un sentido como compuesto. Si «dos pasillos» hubiera llegado a la pertenencia, el BOM
        /// facturaria un protector por cada poste del rack.
        ///
        /// <para>
        /// Se compara SOLO esta familia porque es la unica no indexada por nivel: el desviador, por ejemplo, existe
        /// por poste Y nivel, y la planta —que colapsa los niveles— no es su medida. Compararlos daria un falso
        /// positivo en las dos direcciones.
        /// </para>
        /// </summary>
        [Fact]
        public void TheBom_OfTheSideProtector_MatchesTheMaterializedPieces()
        {
            var id = ElementOf(SelectiveSafetyDefaults.LateralType);
            Assert.False(string.IsNullOrWhiteSpace(id));

            foreach (var sideB in new[] { false, true })
            {
                var system = Resolve(State(sideB));
                var drawn = Drawn(system, SelectiveSafetyDefaults.LateralType).Count;
                var quoted = PushBackBomBuilder.Build(system, Catalog).Components
                    .Where(component => component.Category == SelectiveBomBuilder.Safety)
                    .Where(component => string.Equals(component.ProfileId, id, StringComparison.OrdinalIgnoreCase))
                    .Sum(component => component.Quantity);

                Assert.Equal(2, drawn);
                Assert.Equal(drawn, quoted);
            }
        }

        // ================= El eje es PROPIO: la pertenencia no se toca =========================================

        /// <summary>
        /// La marca de «dos caras de carga» NO escribe el lado de ninguna seleccion. Si lo hiciera, apagaria las
        /// reglas adaptativas de cada familia —es lo que produjo el defecto— y la pertenencia dejaria de ser del
        /// usuario.
        /// </summary>
        [Fact]
        public void DeclaringTwoAisles_NeverWritesTheSideOfASelection()
        {
            var before = new PushBackSafetyAuthority(Catalog).Defaults();
            var lateral = before.FirstOrDefault(selection => string.Equals(
                selection.ElementId, ElementOf(SelectiveSafetyDefaults.LateralType), StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(lateral);
            Assert.Equal(SafetySide.None, lateral.Side);   // el protector es ADAPTATIVO por defecto

            var system = Resolve(State(sideB: true));
            var resolved = system.SafetySelections.First(selection => string.Equals(
                selection.ElementId, lateral.ElementId, StringComparison.OrdinalIgnoreCase));

            Assert.Equal(SafetySide.None, resolved.Side);
            Assert.True(resolved.LowEndOnly, "Push Back sigue siendo un sistema de extremo bajo");
            Assert.True(resolved.BothEndsAreLoadFaces, "y el compuesto declara su segunda cara en su propio eje");
        }

        /// <summary>GUARDA legacy: un rack de un sentido no declara la segunda cara y no cambia en nada.</summary>
        [Fact]
        public void ASingleSidedRack_DoesNotDeclareASecondLoadFace()
        {
            var system = Resolve(State(sideB: false));
            Assert.NotEmpty(system.SafetySelections);
            Assert.All(system.SafetySelections, selection =>
            {
                Assert.True(selection.LowEndOnly);
                Assert.False(selection.BothEndsAreLoadFaces);
            });
        }
    }
}
