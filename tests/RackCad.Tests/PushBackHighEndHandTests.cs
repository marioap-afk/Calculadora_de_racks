using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (corrección aislada 5) — la MANO del larguero ALTO y de su tope la decide el EXTREMO FÍSICO de la
    /// cabecera que recibe ese extremo.
    ///
    /// <para>
    /// Regla del dueño: si la cama termina en el ÚLTIMO poste de la cabecera, el larguero y el tope conservan su
    /// orientación normal; si termina en el PRIMER poste o en un poste interior o suelto, los dos se invierten.
    /// Van SIEMPRE juntos, y la decisión no depende del lado A/B, ni de izquierda/derecha, ni de si la línea está
    /// en el centro o en el borde del rack.
    /// </para>
    /// <para>
    /// Estas pruebas afirman el resultado FÍSICO —hacia dónde mira el escalón respecto del extremo de cabecera al
    /// que se atornilla— y que la misma pieza no cambia de mano entre el lateral y la planta.
    /// </para>
    /// </summary>
    public class PushBackHighEndHandTests
    {
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

        private static PushBackCompositeEditorState State(int slots = 2, int levels = 2)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(side).Structure;
                for (var index = 0; index < matrix.Count; index++)
                {
                    state.Of(side).AdjustLevels(index, levels - matrix.Fronts[index].LoadLevels);
                }
            }

            return state;
        }

        private static PushBackSystem Build(PushBackCompositeEditorState state)
        {
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        private static PushBackSystem Composite(
            PushBackCellTopology topology, PushBackRunDirection direction, int slots = 2)
        {
            var state = State(slots);
            state.SetDefaults(topology, direction);
            return Build(state);
        }

        private static PushBackSystem SingleSided(int fronts = 2)
        {
            var state = new PushBackEditorState();
            state.LoadNew();
            state.Structure.SetFrontCount(fronts);
            var system = new PushBackEditorDesignAssembler(Catalog).Build(state, Inputs()).System;
            Assert.NotNull(system);
            return system;
        }

        // ---- lecturas físicas ------------------------------------------------------------------------------------

        private sealed class Piece
        {
            public double X;
            public bool Mirrored;
            public bool IsTope;
        }

        private static string HighBeamId(PushBackSystem system)
            => string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

        private static IReadOnlyList<Piece> HighPieces(IEnumerable<HeaderBlockInstance> instances, PushBackSystem system)
        {
            var highId = HighBeamId(system);
            return instances
                .Where(instance => instance.Role == HeaderBlockRole.Tope
                    || (instance.Role == HeaderBlockRole.Beam
                        && string.Equals(instance.PieceId, highId, StringComparison.OrdinalIgnoreCase)))
                .Select(instance => new Piece
                {
                    X = Math.Round(instance.Insertion.X, 3),
                    Mirrored = instance.MirroredX,
                    IsTope = instance.Role == HeaderBlockRole.Tope
                })
                .ToList();
        }

        /// <summary>El larguero alto DISTINTO de un rack cuyas camas comparten extremo: una sola respuesta física.</summary>
        private static Piece TheHighBeam(PushBackSystem system)
        {
            var beams = Lateral(system)
                .Where(piece => !piece.IsTope)
                .Select(piece => (piece.X, piece.Mirrored))
                .Distinct()
                .ToList();
            Assert.Single(beams);
            return new Piece { X = beams[0].X, Mirrored = beams[0].Mirrored, IsTope = false };
        }

        private static IReadOnlyList<Piece> Lateral(PushBackSystem system)
            => HighPieces(new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances, system);

        private static IReadOnlyList<Piece> Planta(PushBackSystem system)
            => HighPieces(new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances, system);

        /// <summary>
        /// LA comprobación física: el larguero alto conserva su orientación SÓLO si su extremo cae en el último
        /// poste de la cabecera, y su tope va siempre con la mano contraria — los dos se invierten juntos.
        /// </summary>
        private static void AssertHandFollowsTheHeaderEnd(string label, PushBackSystem system)
        {
            var structure = system.Structure;
            foreach (var view in new[] { Lateral(system), Planta(system) })
            {
                Assert.NotEmpty(view);
                foreach (var piece in view)
                {
                    var atLastPost = PushBackHighEndHand.AtLastPost(structure, piece.X);
                    var expected = piece.IsTope ? atLastPost : !atLastPost;
                    Assert.True(
                        piece.Mirrored == expected,
                        $"{label}: {(piece.IsTope ? "el tope" : "el larguero alto")} en X={piece.X:0.###} " +
                        $"(último poste de la cabecera: {atLastPost}; total {structure.TotalLength:0.###}) " +
                        $"debería ir {(expected ? "invertido" : "en orientación normal")}");
                }
            }
        }

        /// <summary>La MISMA pieza física no puede cambiar de mano entre el lateral y la planta.</summary>
        private static void AssertViewsAgree(string label, PushBackSystem system)
        {
            var lateral = Lateral(system)
                .Select(piece => (piece.X, piece.IsTope, piece.Mirrored)).Distinct().OrderBy(p => p.X).ThenBy(p => p.IsTope).ToList();
            var planta = Planta(system)
                .Select(piece => (piece.X, piece.IsTope, piece.Mirrored)).Distinct().OrderBy(p => p.X).ThenBy(p => p.IsTope).ToList();

            foreach (var piece in lateral)
            {
                Assert.True(
                    planta.Any(other => Math.Abs(other.X - piece.X) < 1e-6
                                        && other.IsTope == piece.IsTope
                                        && other.Mirrored == piece.Mirrored),
                    $"{label}: la pieza en X={piece.X:0.###} cambia de mano entre el lateral y la planta");
            }
        }

        // ---- 1: la regla, en las topologías --------------------------------------------------------------------

        public static IEnumerable<object[]> Topologies() => new[]
        {
            new object[] { PushBackCellTopology.SoloA, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.SoloB, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Encontradas, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.AToB },
            new object[] { PushBackCellTopology.Corrida, PushBackRunDirection.BToA }
        };

        [Theory]
        [MemberData(nameof(Topologies))]
        public void HighBeamStepFacesItsHeaderCenter(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var system = Composite(topology, direction);
            AssertHandFollowsTheHeaderEnd($"{topology}/{direction}", system);
            AssertViewsAgree($"{topology}/{direction}", system);
        }

        /// <summary>
        /// La mano NO sale del lado. Sólo A y Sólo B terminan en el MISMO poste interior, así que sus largueros
        /// altos llevan la MISMA mano — antes llevaban manos opuestas porque la decidía el lado.
        /// </summary>
        [Fact]
        public void HighBeamOrientation_IsNotDerivedFromSideOnly()
        {
            var soloA = Composite(PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
            var soloB = Composite(PushBackCellTopology.SoloB, PushBackRunDirection.AToB);

            var beamA = TheHighBeam(soloA);
            var beamB = TheHighBeam(soloB);

            Assert.Equal(beamA.X, beamB.X, 3);                       // el mismo poste interior
            Assert.Equal(beamA.Mirrored, beamB.Mirrored);            // y por tanto la misma mano
            Assert.True(beamA.Mirrored, "un poste interior invierte");
        }

        /// <summary>Camas encontradas: las dos topan en el mismo poste interior, así que comparten mano.</summary>
        [Fact]
        public void HighBeamOrientation_IsCorrectOnInteriorLine()
        {
            var system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            var beams = Lateral(system).Where(piece => !piece.IsTope).ToList();

            Assert.NotEmpty(beams);
            Assert.Single(beams.Select(piece => piece.Mirrored).Distinct());
            Assert.All(beams, piece => Assert.True(piece.Mirrored));
            AssertHandFollowsTheHeaderEnd("encontradas", system);
        }

        /// <summary>
        /// El ÚLTIMO poste de la cabecera es el único extremo con orientación normal; el PRIMERO invierte, aunque
        /// los dos sean bordes exteriores del rack.
        /// </summary>
        [Fact]
        public void HighBeamOrientation_IsCorrectOnExteriorLine()
        {
            var last = Composite(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            var first = Composite(PushBackCellTopology.Corrida, PushBackRunDirection.BToA);

            var atLast = TheHighBeam(last);
            var atFirst = TheHighBeam(first);

            Assert.Equal(last.Structure.TotalLength, atLast.X, 1);
            Assert.False(atLast.Mirrored, "el último poste conserva la orientación normal");

            Assert.Equal(0.0, atFirst.X, 1);
            Assert.True(atFirst.Mirrored, "el primer poste invierte");
        }

        [Fact]
        public void CorridaAtoB_HighBeamFacesItsHeader()
        {
            var system = Composite(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            AssertHandFollowsTheHeaderEnd("corrida A->B", system);

            var beam = TheHighBeam(system);
            Assert.True(PushBackHighEndHand.AtLastPost(system.Structure, beam.X));
            Assert.False(beam.Mirrored);
        }

        [Fact]
        public void CorridaBtoA_HighBeamFacesItsHeader()
        {
            var system = Composite(PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            AssertHandFollowsTheHeaderEnd("corrida B->A", system);

            var beam = TheHighBeam(system);
            Assert.False(PushBackHighEndHand.AtLastPost(system.Structure, beam.X));
            Assert.True(beam.Mirrored);
        }

        /// <summary>El tope se invierte CON su larguero: es una sola decisión, no dos.</summary>
        [Theory]
        [MemberData(nameof(Topologies))]
        public void RearTope_UsesTheSamePhysicalHandAsItsHighBeam(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var system = Composite(topology, direction);
            foreach (var tope in Lateral(system).Where(piece => piece.IsTope))
            {
                var normal = PushBackHighEndHand.AtLastPost(system.Structure, tope.X);
                var beam = Lateral(system)
                    .Where(piece => !piece.IsTope)
                    .First(piece => PushBackHighEndHand.AtLastPost(system.Structure, piece.X) == normal);

                // Manos contrarias, siempre: el par se invierte junto.
                Assert.NotEqual(beam.Mirrored, tope.Mirrored);
            }
        }

        // ---- geometría del rack ---------------------------------------------------------------------------------

        /// <summary>Con varias ranuras la mano no alterna: todas topan en el mismo extremo de cabecera.</summary>
        [Fact]
        public void MultiFront_HighOrientationDoesNotAlternateIncorrectly()
        {
            var system = Composite(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, slots: 3);
            var beams = Planta(system).Where(piece => !piece.IsTope).ToList();

            Assert.True(beams.Count >= 3);
            Assert.Single(beams.Select(piece => piece.Mirrored).Distinct());
            AssertHandFollowsTheHeaderEnd("multifrente", system);
        }

        /// <summary>Poner una ranura anterior en blanco no voltea el larguero alto de las siguientes.</summary>
        [Fact]
        public void BlankEarlierFront_DoesNotFlipLaterHighBeam()
        {
            var baseline = State(slots: 3);
            baseline.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            var before = Planta(Build(baseline)).Where(piece => !piece.IsTope).Select(piece => piece.Mirrored).Distinct().ToList();

            var blanked = State(slots: 3);
            blanked.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            Assert.True(blanked.SetSlotPresent(PushBackSide.A, 0, false));
            var system = Build(blanked);

            Assert.Equal(before, Planta(system).Where(piece => !piece.IsTope).Select(piece => piece.Mirrored).Distinct().ToList());
            AssertHandFollowsTheHeaderEnd("ranura 0 en blanco", system);
        }

        /// <summary>
        /// Una corrida CORTA acaba en un poste interior: invierte. Y su POSICIÓN no se mueve — la ronda 4B ya la
        /// cerró y esta corrida sólo cambia la mano.
        /// </summary>
        [Fact]
        public void ShortCorrida_PreservesPlacementAndFixesOnlyOrientation()
        {
            var state = State(slots: 3);
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SetCorridaDepth(1, 0, 2);
            var system = Build(state);
            var total = system.Structure.TotalLength;

            var interior = Planta(system)
                .Where(piece => !piece.IsTope && Math.Min(piece.X, total - piece.X) > 5.0)
                .ToList();
            Assert.NotEmpty(interior);
            Assert.All(interior, piece => Assert.True(piece.Mirrored, "un poste interior invierte"));

            // Su tope sigue junto a ese mismo larguero: la posición es la de la ronda 4B.
            foreach (var beam in interior)
            {
                Assert.Contains(
                    Planta(system).Where(piece => piece.IsTope),
                    tope => Math.Abs(tope.X - beam.X) < 2.0);
            }

            AssertHandFollowsTheHeaderEnd("corrida corta", system);
            AssertViewsAgree("corrida corta", system);
        }

        [Theory]
        [InlineData(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB)]
        [InlineData(PushBackCellTopology.Corrida, PushBackRunDirection.AToB)]
        public void WithGap_HandStillFollowsTheHeaderEnd(
            PushBackCellTopology topology, PushBackRunDirection direction)
        {
            var state = State();
            state.SetGap(48.0);
            state.SetDefaults(topology, direction);
            var system = Build(state);

            AssertHandFollowsTheHeaderEnd($"{topology} con calle 48", system);
            AssertViewsAgree($"{topology} con calle 48", system);
        }

        /// <summary>
        /// Un rack de un solo sentido termina en el ÚLTIMO poste de su cabecera, así que su larguero alto y su tope
        /// llevan la orientación normal. La regla es del producto, no del rack compuesto.
        /// </summary>
        [Fact]
        public void SingleSidedRack_EndsAtTheLastPost_AndKeepsTheNormalOrientation()
        {
            var system = SingleSided();
            var beam = Lateral(system).First(piece => !piece.IsTope);

            Assert.Equal(system.Structure.TotalLength, beam.X, 1);
            Assert.False(beam.Mirrored);

            AssertHandFollowsTheHeaderEnd("un solo sentido", system);
            AssertViewsAgree("un solo sentido", system);
        }

        /// <summary>El BOM no depende de la mano: misma pieza, mismas cantidades.</summary>
        [Fact]
        public void Hand_DoesNotChangeTheBom()
        {
            foreach (var system in new[]
            {
                Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB),
                Composite(PushBackCellTopology.Corrida, PushBackRunDirection.BToA),
                SingleSided()
            })
            {
                var topes = PushBackBomBuilder.Build(system, Catalog).Components
                    .Where(component => component.Category == PushBackBomBuilder.RearTope)
                    .ToList();
                Assert.NotEmpty(topes);
                Assert.Single(topes.Select(component => component.ProfileId).Distinct());
            }
        }
    }
}
