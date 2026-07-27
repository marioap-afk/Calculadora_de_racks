using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
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
    /// Owner-validation round 1, defecto 2 (I-32) — la matriz por poste del PROTECTOR LATERAL, recorrida por su
    /// camino REAL de dibujo y no solo por la autoridad.
    ///
    /// Los tres ejes que <see cref="SafetySide"/> mezcla se prueban por separado:
    /// <list type="bullet">
    /// <item><b>Pertenencia</b>: <c>None</c> en un poste lo excluye en las cuatro vistas y en el BOM; los demás no
    /// se ven afectados.</item>
    /// <item><b>Orientación</b>: una elección <c>Right</c> conserva su espejo.</item>
    /// <item><b>Extremo longitudinal</b>: en Push Back <c>Right</c> aterriza en el extremo BAJO — nunca atrás—,
    /// y el frontal POSTERIOR no lleva protector ordinario.</item>
    /// </list>
    /// El Selectivo y el Dinámico conservan su lectura histórica: sin la marca de extremo bajo, <c>Right</c> sigue
    /// yendo al extremo alto.
    /// </summary>
    public class PushBackLateralProtectorEndsTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

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

        private static PushBackSystem Resolve(RackCatalog catalog, params (int Post, SafetySide Side)[] posts)
        {
            var selection = new SelectiveSafetySelection { ElementId = LateralId(catalog), Quantity = 1, Side = SafetySide.None };
            foreach (var (post, side) in posts)
            {
                selection.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = side });
            }

            var design = new PushBackDesign { Structure = Structure(2) };
            design.Structure.SafetySelections.Add(selection);
            return new PushBackResolver(catalog).Resolve(design);
        }

        private static List<HeaderBlockInstance> Pieces(IEnumerable<HeaderBlockInstance> instances, string pieceId)
            => instances.Where(i => string.Equals(i.PieceId, pieceId, StringComparison.OrdinalIgnoreCase)).ToList();

        private static List<HeaderBlockInstance> Lateral(PushBackSystem system, RackCatalog catalog, string id)
            => Pieces(new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances, id);

        private static List<HeaderBlockInstance> Frontal(PushBackSystem system, RackCatalog catalog, string id, PushBackFrontalEnd end)
            => Pieces(new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, end).Flatten().Instances, id);

        private static List<HeaderBlockInstance> Planta(PushBackSystem system, RackCatalog catalog, string id)
            => Pieces(new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances, id);

        private static int Bom(PushBackSystem system, RackCatalog catalog, string id)
            => PushBackBomBuilder.Build(system, catalog).Components
                .Where(c => string.Equals(c.ProfileId, id, StringComparison.OrdinalIgnoreCase))
                .Sum(c => c.Quantity);

        // ---- Pertenencia: None excluye ese poste y solo ese ----

        [Fact]
        public void None_ExcludesThatPostEverywhere_AndLeavesTheOthersAlone()
        {
            var catalog = Catalog;
            var id = LateralId(catalog);

            var all = Resolve(catalog, (0, SafetySide.Left), (1, SafetySide.Left), (2, SafetySide.Left));
            var middleOff = Resolve(catalog, (0, SafetySide.Left), (1, SafetySide.None), (2, SafetySide.Left));

            Assert.Equal(3, Bom(all, catalog, id));
            Assert.Equal(2, Bom(middleOff, catalog, id));

            Assert.Equal(3, Frontal(all, catalog, id, PushBackFrontalEnd.EntradaSalida).Count);
            Assert.Equal(2, Frontal(middleOff, catalog, id, PushBackFrontalEnd.EntradaSalida).Count);
            Assert.Equal(3, Planta(all, catalog, id).Count);
            Assert.Equal(2, Planta(middleOff, catalog, id).Count);
        }

        [Fact]
        public void EveryChoice_KeepsItsPost_AndNoneDrawsNothing()
        {
            var catalog = Catalog;
            var id = LateralId(catalog);

            // Both son DOS orientaciones y por tanto dos piezas; Left y Right, una. El corte frontal solo puede
            // mostrar una en cualquier caso: en esa proyeccion las dos orientaciones se superponen.
            foreach (var (side, pieces) in new[]
            {
                (SafetySide.Left, 1), (SafetySide.Right, 1), (SafetySide.Both, 2),
            })
            {
                var system = Resolve(catalog, (1, side));
                Assert.Equal(pieces, Bom(system, catalog, id));
                Assert.Equal(pieces, Planta(system, catalog, id).Count);
                Assert.Single(Frontal(system, catalog, id, PushBackFrontalEnd.EntradaSalida));
                Assert.Empty(Frontal(system, catalog, id, PushBackFrontalEnd.Posterior));
            }

            var off = Resolve(catalog, (1, SafetySide.None));
            Assert.Equal(0, Bom(off, catalog, id));
            Assert.Empty(Frontal(off, catalog, id, PushBackFrontalEnd.EntradaSalida));
            Assert.Empty(Planta(off, catalog, id));
        }

        // ---- Extremo longitudinal: Right aterriza abajo, nunca atrás ----

        [Fact]
        public void ARightChoice_LandsAtTheLowEnd_NeverBehind()
        {
            var catalog = Catalog;
            var id = LateralId(catalog);

            var left = Resolve(catalog, (1, SafetySide.Left));
            var right = Resolve(catalog, (1, SafetySide.Right));

            // El corte de ENTRADA/SALIDA lleva la pieza en los dos casos...
            Assert.Single(Frontal(left, catalog, id, PushBackFrontalEnd.EntradaSalida));
            Assert.Single(Frontal(right, catalog, id, PushBackFrontalEnd.EntradaSalida));

            // ...y el POSTERIOR no lleva protector ordinario en ninguno.
            Assert.Empty(Frontal(left, catalog, id, PushBackFrontalEnd.Posterior));
            Assert.Empty(Frontal(right, catalog, id, PushBackFrontalEnd.Posterior));

            // Y el BOM cuenta UNA pieza en los dos casos: la elección de orientación no crea ni destruye material.
            Assert.Equal(1, Bom(left, catalog, id));
            Assert.Equal(1, Bom(right, catalog, id));

            // La ORIENTACIÓN sobrevive: la copia Right se dibuja espejada, la Left no.
            Assert.False(Frontal(left, catalog, id, PushBackFrontalEnd.EntradaSalida)[0].MirroredX);
            Assert.True(Frontal(right, catalog, id, PushBackFrontalEnd.EntradaSalida)[0].MirroredX);
        }

        /// <summary>
        /// <c>Both</c> son DOS piezas, tambien en Push Back: el extremo bajo se comparte, pero las dos ORIENTACIONES
        /// siguen siendo dos protectores distintos. La planta —la vista que las separa— dibuja las dos y el BOM las
        /// cuenta; el corte frontal solo puede mostrar una porque en esa proyeccion se superponen.
        /// </summary>
        [Fact]
        public void Both_KeepsBothOrientations_AtTheLowEnd()
        {
            var catalog = Catalog;
            var id = LateralId(catalog);
            var both = Resolve(catalog, (1, SafetySide.Both));

            Assert.Equal(2, Planta(both, catalog, id).Count);
            Assert.Equal(2, Bom(both, catalog, id));
            Assert.Single(Frontal(both, catalog, id, PushBackFrontalEnd.EntradaSalida));
            Assert.Empty(Frontal(both, catalog, id, PushBackFrontalEnd.Posterior));

            // Una sola orientacion es UNA pieza.
            Assert.Equal(1, Bom(Resolve(catalog, (1, SafetySide.Left)), catalog, id));
            Assert.Equal(1, Bom(Resolve(catalog, (1, SafetySide.Right)), catalog, id));
        }

        // ---- Orientación: Right conserva su espejo en su propio sitio ----

        [Fact]
        public void ARightChoice_KeepsItsMirror_WhileStayingAtTheLowEnd()
        {
            var copies = SelectiveSafetyEnds.CopiesForPost(
                LowEnd((0, SafetySide.Right)), 0);

            var copy = Assert.Single(copies);
            Assert.False(copy.AtHighEnd);    // extremo bajo
            Assert.True(copy.Mirrored);      // pero con su orientación intacta
        }

        [Fact]
        public void ALeftChoice_IsNotMirrored()
        {
            var copy = Assert.Single(SelectiveSafetyEnds.CopiesForPost(LowEnd((0, SafetySide.Left)), 0));
            Assert.False(copy.AtHighEnd);
            Assert.False(copy.Mirrored);
        }

        // ---- Selectivo y Dinámico: comportamiento histórico intacto ----

        [Fact]
        public void WithoutTheLowEndMark_RightStillGoesToTheHighEnd()
        {
            var ordinary = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.None };
            ordinary.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Right });
            ordinary.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Both });
            ordinary.PostSides.Add(new SafetyPostSide { PostIndex = 2, Side = SafetySide.None });

            var right = Assert.Single(SelectiveSafetyEnds.CopiesForPost(ordinary, 0));
            Assert.True(right.AtHighEnd);
            Assert.True(right.Mirrored);

            var both = SelectiveSafetyEnds.CopiesForPost(ordinary, 1);
            Assert.Equal(2, both.Count);
            Assert.Contains(both, c => !c.AtHighEnd && !c.Mirrored);
            Assert.Contains(both, c => c.AtHighEnd && c.Mirrored);

            Assert.Empty(SelectiveSafetyEnds.CopiesForPost(ordinary, 2));

            // Y las consultas por extremo siguen dando lo de siempre.
            Assert.True(SelectiveSafetyEnds.DrawsAt(ordinary, 0, highEnd: true));
            Assert.False(SelectiveSafetyEnds.DrawsAt(ordinary, 0, highEnd: false));
            Assert.True(SelectiveSafetyEnds.DrawsAt(ordinary, 1, highEnd: true));
            Assert.True(SelectiveSafetyEnds.DrawsAt(ordinary, 1, highEnd: false));
        }



        private static SelectiveSafetySelection LowEnd(params (int Post, SafetySide Side)[] posts)
        {
            var selection = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.None, LowEndOnly = true };
            foreach (var (post, side) in posts)
            {
                selection.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = side });
            }

            return selection;
        }
    }
}
