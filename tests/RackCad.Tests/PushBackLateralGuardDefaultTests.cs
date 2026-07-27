using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Owner-validation round 2, defecto 2 (I-32) — el DEFAULT del protector lateral.
    ///
    /// Sin seleccion explicita, un rack lleva protector en los dos postes de los EXTREMOS y en ninguno interior:
    /// el primero sin espejo y el ultimo espejado. En Push Back faltaba el ULTIMO.
    ///
    /// La causa es una confusion de ejes que ya conocemos y que aqui se colo en la regla ADAPTATIVA:
    /// <c>DynamicLateralGuardPlan.SideAt</c> devolvia <c>Right</c> para el ultimo poste, y como Push Back es un
    /// sistema de extremo bajo, el codigo interpretaba ese <c>Right</c> como «extremo posterior» y lo borraba. Pero
    /// en la regla adaptativa <c>Right</c> es <b>ORIENTACION</b>, no extremo: el ultimo poste lleva su protector
    /// delante, espejado, porque protege la otra cara del pasillo.
    ///
    /// La matriz EXPLICITA por poste —ya aprobada— no se toca: solo cambia la ruta adaptativa, y lo hace emitiendo
    /// copias fisicas con su <c>AtHighEnd</c> y su <c>Mirrored</c> por separado, en vez de un <c>SafetySide</c> que
    /// mezcla los dos.
    /// </summary>
    public class PushBackLateralGuardDefaultTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string LateralId(RackCatalog catalog)
            => catalog.SafetyElements.First(e => SelectiveSafetyDefaults.IsType(e.Type, SelectiveSafetyDefaults.LateralType)).Id;

        /// <summary>Un rack Push Back NUEVO: la seguridad es exactamente la que siembra la autoridad.</summary>
        private static PushBackDesign NewRack(RackCatalog catalog, int fronts)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0
                }
            };
            for (var i = 0; i < fronts; i++)
            {
                design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1 });
                design.Fronts.Add(new PushBackFrontConfig());
            }

            foreach (var selection in new PushBackSafetyAuthority(catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            return design;
        }

        private static PushBackSystem Resolve(RackCatalog catalog, int fronts)
            => new PushBackResolver(catalog).Resolve(NewRack(catalog, fronts));

        private static List<HeaderBlockInstance> Guards(IEnumerable<HeaderBlockInstance> instances, string id)
            => instances.Where(i => string.Equals(i.PieceId, id, StringComparison.OrdinalIgnoreCase)).ToList();

        private static List<HeaderBlockInstance> Frontal(PushBackSystem system, RackCatalog catalog, PushBackFrontalEnd end)
            => Guards(new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, end).Flatten().Instances, LateralId(catalog));

        private static List<HeaderBlockInstance> Planta(PushBackSystem system, RackCatalog catalog)
            => Guards(new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances, LateralId(catalog));

        private static int Bom(PushBackSystem system, RackCatalog catalog)
            => PushBackBomBuilder.Build(system, catalog).Components
                .Where(c => string.Equals(c.ProfileId, LateralId(catalog), StringComparison.OrdinalIgnoreCase))
                .Sum(c => c.Quantity);

        /// <summary>El poste al que pertenece una pieza, recuperado de su propia coordenada.</summary>
        private static int PostOf(double coordinate, IReadOnlyList<double> postPositions)
            => postPositions
                .Select((x, index) => (index, distance: Math.Abs(x - coordinate)))
                .OrderBy(t => t.distance).First().index;

        // ---------------------------------------------------------------------------------------------------
        // El default: primero y ultimo, ninguno interior
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        public void ANewPushBackRack_GuardsTheFirstAndTheLastPost_AndNoneInBetween(int fronts)
        {
            var catalog = Catalog;
            var system = Resolve(catalog, fronts);
            var layout = DynamicFrontGeometry.Compute(system.Structure, catalog);
            var postCount = layout.PostPositions.Count;
            Assert.Equal(fronts + 1, postCount);

            var drawn = Frontal(system, catalog, PushBackFrontalEnd.EntradaSalida);
            var posts = drawn.Select(i => PostOf(i.Insertion.X, layout.PostPositions)).OrderBy(p => p).ToList();

            Assert.Equal(new[] { 0, postCount - 1 }, posts);
        }

        [Fact]
        public void BothDefaultGuards_AreOnTheEntranceExitCut_AndNoneOnTheRearCut()
        {
            var catalog = Catalog;
            var system = Resolve(catalog, 3);

            Assert.Equal(2, Frontal(system, catalog, PushBackFrontalEnd.EntradaSalida).Count);
            Assert.Empty(Frontal(system, catalog, PushBackFrontalEnd.Posterior));
        }

        /// <summary>
        /// El primero sin espejo y el ultimo espejado. Es el corazon del defecto: el ultimo existe y lo que cambia
        /// respecto al primero es la ORIENTACION, no el extremo.
        /// </summary>
        [Fact]
        public void TheFirstGuardIsNotMirrored_AndTheLastOneIs()
        {
            var catalog = Catalog;
            var system = Resolve(catalog, 3);
            var layout = DynamicFrontGeometry.Compute(system.Structure, catalog);
            var lastPost = layout.PostPositions.Count - 1;

            var byPost = Frontal(system, catalog, PushBackFrontalEnd.EntradaSalida)
                .ToDictionary(i => PostOf(i.Insertion.X, layout.PostPositions), i => i);

            Assert.True(byPost.ContainsKey(0), "falta el protector del PRIMER poste");
            Assert.True(byPost.ContainsKey(lastPost), "falta el protector del ULTIMO poste");
            Assert.False(byPost[0].MirroredX, "el primer protector no debe ir espejado");
            Assert.True(byPost[lastPost].MirroredX, "el ultimo protector debe ir espejado");
        }

        [Fact]
        public void ThePlantaAndTheBom_BothCountTwo()
        {
            var catalog = Catalog;
            var system = Resolve(catalog, 3);

            Assert.Equal(2, Planta(system, catalog).Count);
            Assert.Equal(2, Bom(system, catalog));
        }

        // ---------------------------------------------------------------------------------------------------
        // El default es ADAPTATIVO: sigue al ultimo poste cuando el rack crece o se reduce
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void GrowingOrShrinkingTheRack_MovesTheDefaultToTheNewLastPost()
        {
            var catalog = Catalog;

            foreach (var fronts in new[] { 2, 3, 4, 6 })
            {
                var system = Resolve(catalog, fronts);
                var layout = DynamicFrontGeometry.Compute(system.Structure, catalog);
                var lastPost = layout.PostPositions.Count - 1;
                var posts = Frontal(system, catalog, PushBackFrontalEnd.EntradaSalida)
                    .Select(i => PostOf(i.Insertion.X, layout.PostPositions))
                    .OrderBy(p => p)
                    .ToList();

                Assert.Equal(new[] { 0, lastPost }, posts);
                Assert.Equal(2, Bom(system, catalog));
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // Guardar y reabrir conserva el comportamiento
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void SavingAndReopening_KeepsTheDefaultBehaviour()
        {
            var catalog = Catalog;
            var store = new RackProjectStore();

            var reopened = store.Deserialize(store.Serialize(RackProject.ForPushBack(NewRack(catalog, 3))));
            var system = new PushBackResolver(catalog).Resolve(reopened.PushBackDesign);
            var layout = DynamicFrontGeometry.Compute(system.Structure, catalog);
            var lastPost = layout.PostPositions.Count - 1;

            var byPost = Frontal(system, catalog, PushBackFrontalEnd.EntradaSalida)
                .ToDictionary(i => PostOf(i.Insertion.X, layout.PostPositions), i => i);

            Assert.Equal(new[] { 0, lastPost }, byPost.Keys.OrderBy(p => p).ToArray());
            Assert.False(byPost[0].MirroredX);
            Assert.True(byPost[lastPost].MirroredX);
            Assert.Equal(2, Bom(system, catalog));
        }

        // ---------------------------------------------------------------------------------------------------
        // Una matriz EXPLICITA sustituye al default
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void AnExplicitMatrix_ReplacesTheAdaptiveDefault()
        {
            var catalog = Catalog;
            var design = NewRack(catalog, 3);
            var lateral = design.Structure.SafetySelections
                .First(s => string.Equals(s.ElementId, LateralId(catalog), StringComparison.OrdinalIgnoreCase));

            // El usuario elige EXACTAMENTE el poste interior 1, y solo ese.
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Left });

            var system = new PushBackResolver(catalog).Resolve(design);
            var layout = DynamicFrontGeometry.Compute(system.Structure, catalog);
            var posts = Frontal(system, catalog, PushBackFrontalEnd.EntradaSalida)
                .Select(i => PostOf(i.Insertion.X, layout.PostPositions))
                .OrderBy(p => p)
                .ToList();

            Assert.Equal(new[] { 1 }, posts);
            Assert.Equal(1, Bom(system, catalog));
        }

        // ---------------------------------------------------------------------------------------------------
        // El Dinamico conserva su lectura historica
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// En un sistema SIN extremo bajo, el ultimo poste sigue llevando su protector en el extremo ALTO. Es la
        /// diferencia real entre los dos sistemas, y la que hace que la correccion de Push Back no sea un cambio
        /// del Dinamico: alli el extremo alto existe y se usa.
        /// </summary>
        [Fact]
        public void InAnOrdinarySystem_TheLastGuardStillGoesToTheHighEnd()
        {
            var ordinary = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.None };

            var first = Assert.Single(DynamicLateralGuardPlan.CopiesAt(ordinary, 0, 3));
            Assert.False(first.AtHighEnd);
            Assert.False(first.Mirrored);

            var last = Assert.Single(DynamicLateralGuardPlan.CopiesAt(ordinary, 2, 3));
            Assert.True(last.AtHighEnd);
            Assert.True(last.Mirrored);

            Assert.Empty(DynamicLateralGuardPlan.CopiesAt(ordinary, 1, 3));
        }

        /// <summary>Y en uno de extremo bajo, el ultimo se queda DELANTE, espejado. Nunca atras, y nunca ausente.</summary>
        [Fact]
        public void InALowEndOnlySystem_TheLastGuardStaysAtTheLowEnd_Mirrored()
        {
            var lowEnd = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.None, LowEndOnly = true };

            var first = Assert.Single(DynamicLateralGuardPlan.CopiesAt(lowEnd, 0, 3));
            Assert.False(first.AtHighEnd);
            Assert.False(first.Mirrored);

            var last = Assert.Single(DynamicLateralGuardPlan.CopiesAt(lowEnd, 2, 3));
            Assert.False(last.AtHighEnd);   // delante
            Assert.True(last.Mirrored);     // pero espejado

            Assert.Empty(DynamicLateralGuardPlan.CopiesAt(lowEnd, 1, 3));
        }
    }
}
