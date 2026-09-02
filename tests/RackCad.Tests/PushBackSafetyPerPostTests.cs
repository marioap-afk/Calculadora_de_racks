using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Owner-validation round 1, defecto 2 (I-32) — la matriz POR POSTE de botas y protectores laterales se
    /// perdía: el usuario elegía postes concretos y el rack los ignoraba.
    ///
    /// La causa está en <see cref="PushBackSafetyAuthority"/>: para imponer "solo el extremo bajo" borraba
    /// <see cref="SelectiveSafetySelection.PostSides"/> entera. Pero esa lista mezcla TRES ejes ortogonales:
    /// <list type="number">
    /// <item><b>Pertenencia</b>: qué postes llevan la pieza (una entrada con <c>None</c> la excluye).</item>
    /// <item><b>Orientación</b>: el espejo de la pieza en su sitio (lo usa la planta).</item>
    /// <item><b>Extremo longitudinal</b>: en qué punta del rack se dibuja (lo usan el frontal y el lateral).</item>
    /// </list>
    /// Push Back solo necesita restringir el TERCERO. Borrar la lista para conseguirlo destruye el primero,
    /// que no dice nada sobre el extremo. La restricción pasa a aplicarse donde se decide el extremo.
    /// </summary>
    public class PushBackSafetyPerPostTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string BootId(RackCatalog catalog)
            => catalog.SafetyElements.First(e => SelectiveSafetyDefaults.IsType(e.Type, SelectiveSafetyDefaults.BotaType)).Id;

        private static string LateralId(RackCatalog catalog)
            => catalog.SafetyElements.First(e => SelectiveSafetyDefaults.IsType(e.Type, SelectiveSafetyDefaults.LateralType)).Id;

        private static DynamicRackDesign Structure(int fronts)
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 2,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            for (var i = 0; i < fronts; i++)
            {
                design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1 });
            }

            return design;
        }

        private static SelectiveSafetySelection Boot(RackCatalog catalog, params (int Post, SafetySide Side)[] posts)
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId(catalog), Quantity = 1, Side = SafetySide.None };
            foreach (var (post, side) in posts)
            {
                selection.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = side });
            }

            return selection;
        }

        // ---- La autoridad conserva la pertenencia ----

        [Fact]
        public void Authority_KeepsThePerPostMatrix_InsteadOfClearingIt()
        {
            var catalog = Catalog;
            var source = Boot(catalog, (0, SafetySide.Both), (1, SafetySide.None), (3, SafetySide.Right));

            var authorized = new PushBackSafetyAuthority(catalog).Authorize(new[] { source }).Single();

            Assert.Equal(3, authorized.PostSides.Count);
            Assert.Equal(new[] { 0, 1, 3 }, authorized.PostSides.Select(p => p.PostIndex).OrderBy(i => i).ToArray());
            Assert.Equal(SafetySide.None, authorized.SideForPost(1));   // la exclusión del usuario sobrevive
            Assert.NotEqual(SafetySide.None, authorized.SideForPost(0));
            Assert.NotEqual(SafetySide.None, authorized.SideForPost(3));

            // La fuente nunca se muta.
            Assert.Equal(3, source.PostSides.Count);
            Assert.Equal(SafetySide.Both, source.SideForPost(0));
        }

        [Fact]
        public void Authority_IsIdempotent()
        {
            var catalog = Catalog;
            var authority = new PushBackSafetyAuthority(catalog);
            var once = authority.Authorize(new[] { Boot(catalog, (0, SafetySide.Both), (2, SafetySide.None)) }).Single();
            var twice = authority.Authorize(new[] { once }).Single();

            Assert.Equal(once.PostSides.Count, twice.PostSides.Count);
            Assert.Equal(
                once.PostSides.Select(p => (p.PostIndex, p.Side)).OrderBy(p => p.PostIndex),
                twice.PostSides.Select(p => (p.PostIndex, p.Side)).OrderBy(p => p.PostIndex));
        }

        /// <summary>La restricción al extremo bajo se sigue cumpliendo: ningún poste dibuja en la punta alta.</summary>
        [Fact]
        public void Authority_StillKeepsEverythingAtTheLowEnd()
        {
            var catalog = Catalog;
            var authorized = new PushBackSafetyAuthority(catalog)
                .Authorize(new[] { Boot(catalog, (0, SafetySide.Right), (1, SafetySide.Both)) }).Single();

            Assert.True(authorized.LowEndOnly);
            foreach (var post in new[] { 0, 1 })
            {
                Assert.Equal(SafetySide.Left, SelectiveSafetyEnds.EndsForPost(authorized, post));
            }
        }

        [Fact]
        public void EndsForPost_SeparatesMembershipFromTheLongitudinalEnd()
        {
            // Sin LowEndOnly (Selectivo/Dinámico) el extremo se lee literal.
            var ordinary = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.None };
            ordinary.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Right });
            ordinary.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.None });
            Assert.Equal(SafetySide.Right, SelectiveSafetyEnds.EndsForPost(ordinary, 0));
            Assert.Equal(SafetySide.None, SelectiveSafetyEnds.EndsForPost(ordinary, 1));

            // Con LowEndOnly, la PERTENENCIA manda y el extremo se colapsa al bajo — nunca al revés.
            var lowEnd = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.None, LowEndOnly = true };
            lowEnd.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Right });
            lowEnd.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.None });
            Assert.Equal(SafetySide.Left, SelectiveSafetyEnds.EndsForPost(lowEnd, 0));
            Assert.Equal(SafetySide.None, SelectiveSafetyEnds.EndsForPost(lowEnd, 1));
        }

        // ---- Resolver, vistas y BOM ven los MISMOS postes ----

        [Fact]
        public void Resolver_KeepsThePerPostMatrix_InTheSystemAndInTheStructure()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = Structure(2) };
            design.Structure.SafetySelections.Add(Boot(catalog, (1, SafetySide.Both)));

            var system = new PushBackResolver(catalog).Resolve(design);
            var selection = system.SafetySelections.Single(s => string.Equals(s.ElementId, BootId(catalog), StringComparison.OrdinalIgnoreCase));

            Assert.NotEmpty(selection.PostSides);
            Assert.Equal(SafetySide.None, selection.SideForPost(0));
            Assert.NotEqual(SafetySide.None, selection.SideForPost(1));
            Assert.Equal(SafetySide.None, selection.SideForPost(2));

            var inStructure = system.Structure.SafetySelections.Single(s => string.Equals(s.ElementId, BootId(catalog), StringComparison.OrdinalIgnoreCase));
            Assert.Equal(SafetySide.None, inStructure.SideForPost(0));
            Assert.NotEqual(SafetySide.None, inStructure.SideForPost(1));
        }

        [Fact]
        public void Views_DrawTheBootOnlyOnTheChosenPosts()
        {
            var catalog = Catalog;
            var bootId = BootId(catalog);
            var design = new PushBackDesign { Structure = Structure(2) };
            design.Structure.SafetySelections.Add(Boot(catalog, (1, SafetySide.Both)));
            var system = new PushBackResolver(catalog).Resolve(design);

            var layout = DynamicFrontGeometry.Compute(system.Structure, catalog);
            Assert.Equal(3, layout.PostPositions.Count);

            var frontal = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances
                .Where(i => string.Equals(i.PieceId, bootId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.Single(frontal);

            // Y está en la columna del poste 1, no en la de otro.
            var expectedX = layout.PostPositions[1];
            var nearest = layout.PostPositions
                .Select((x, index) => (index, distance: Math.Abs(x - frontal[0].Insertion.X)))
                .OrderBy(t => t.distance).First().index;
            Assert.Equal(1, nearest);
            Assert.True(Math.Abs(expectedX - layout.PostPositions[nearest]) < 1e-6);

            var planta = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances
                .Where(i => string.Equals(i.PieceId, bootId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.NotEmpty(planta);
            Assert.All(planta, instance => Assert.True(
                Math.Abs(instance.Insertion.Y - layout.PostPositions[1]) < 1e-6,
                "la bota de planta no está en la línea del poste elegido"));
        }

        [Fact]
        public void Bom_CountsTheChosenPostsOnly()
        {
            var catalog = Catalog;
            var bootId = BootId(catalog);

            int Quantity(params (int Post, SafetySide Side)[] posts)
            {
                var design = new PushBackDesign { Structure = Structure(3) };
                design.Structure.SafetySelections.Add(Boot(catalog, posts));
                var system = new PushBackResolver(catalog).Resolve(design);
                return PushBackBomBuilder.Build(system, catalog).Components
                    .Where(c => string.Equals(c.ProfileId, bootId, StringComparison.OrdinalIgnoreCase))
                    .Sum(c => c.Quantity);
            }

            // I-42 (S1B): la eleccion de la BOTA nombra UBICACIONES. «Entrada/Salida» —el Izquierda historico— es
            // UNA por poste; «Ambas» son DOS, porque son dos ubicaciones fisicas distintas y cada una es una pieza.
            Assert.Equal(1, Quantity((1, SafetySide.Left)));
            Assert.Equal(2, Quantity((1, SafetySide.Left), (2, SafetySide.Left)));
            Assert.Equal(2, Quantity((1, SafetySide.Both)));
            Assert.Equal(4, Quantity((1, SafetySide.Both), (2, SafetySide.Both)));
        }

        [Fact]
        public void ExplicitLateralChoice_BeatsTheAdaptiveRule()
        {
            var catalog = Catalog;
            var selection = new SelectiveSafetySelection { ElementId = LateralId(catalog), Quantity = 1, Side = SafetySide.None };
            selection.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            selection.PostSides.Add(new SafetyPostSide { PostIndex = 2, Side = SafetySide.Right });

            var authorized = new PushBackSafetyAuthority(catalog).Authorize(new[] { selection }).Single();

            // El poste 2 fue elegido explícitamente: la regla adaptativa no puede borrarlo.
            Assert.NotEqual(SafetySide.None, DynamicLateralGuardPlan.SideAt(authorized, 2, 3));
            Assert.NotEqual(SafetySide.None, DynamicLateralGuardPlan.SideAt(authorized, 0, 3));
            Assert.Equal(SafetySide.None, DynamicLateralGuardPlan.SideAt(authorized, 1, 3));

            // ...y en las vistas de extremo, ambos van al extremo BAJO.
            Assert.Equal(SafetySide.Left, SelectiveSafetyEnds.EndsForPost(authorized, 0));
            Assert.Equal(SafetySide.Left, SelectiveSafetyEnds.EndsForPost(authorized, 2));
        }

        // ---- Persistencia: la matriz cruza resolve -> snapshot -> documento -> dominio ----

        [Fact]
        public void RoundTrip_ThroughResolveSnapshotAndDocument_KeepsTheMatrix()
        {
            var catalog = Catalog;
            var resolver = new PushBackResolver(catalog);
            var design = new PushBackDesign { Structure = Structure(2) };
            design.Structure.SafetySelections.Add(Boot(catalog, (1, SafetySide.Both), (2, SafetySide.None)));

            var system = resolver.Resolve(design);
            var snapshot = resolver.Snapshot(system);
            var restored = PushBackDesignDocument.FromDomain(snapshot).ToDomain();
            var reResolved = resolver.Resolve(restored);

            var selection = reResolved.SafetySelections.Single(s => string.Equals(s.ElementId, BootId(catalog), StringComparison.OrdinalIgnoreCase));
            Assert.NotEqual(SafetySide.None, selection.SideForPost(1));
            Assert.Equal(SafetySide.None, selection.SideForPost(0));
            Assert.Equal(SafetySide.None, selection.SideForPost(2));
        }

        // ---- Aislamiento: Selectivo y Dinámico leen la matriz literal ----

        [Fact]
        public void OrdinarySelection_KeepsItsLiteralSidesAndOrientation()
        {
            var selection = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.Both };
            selection.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Right });

            // Nadie la marcó: ni pertenencia ni extremo cambian.
            Assert.False(selection.LowEndOnly);
            Assert.Equal(SafetySide.Right, selection.SideForPost(1));
            Assert.Equal(SafetySide.Right, SelectiveSafetyEnds.EndsForPost(selection, 1));
            Assert.Equal(SafetySide.Both, SelectiveSafetyEnds.EndsForPost(selection, 0));

            var copy = selection.DeepCopy();
            Assert.Equal(SafetySide.Right, copy.SideForPost(1));
            Assert.Single(copy.PostSides);
        }
    }
}
