using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
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
    /// PB-004, consumidores (I-32) — la elevación derivada de Push Back tiene que llegar a TODO lo que cuelga de un
    /// larguero de extremo, no solo a los propios largueros. Aquí se mide sobre el dibujo, pieza a pieza:
    ///
    /// <list type="bullet">
    /// <item>el <b>desviador superior</b> del corte lateral, en el extremo bajo;</item>
    /// <item>el <b>desviador superior</b> del corte frontal bajo;</item>
    /// <item>las <b>cotas</b> y las <b>etiquetas</b> de nivel de los dos cortes.</item>
    /// </list>
    ///
    /// El escenario es deliberadamente JAGGED —tres frentes con distinto número de niveles, distinta profundidad y
    /// distinta altura de primer nivel—, porque es el único que separa las tres consultas del contexto: por frente,
    /// por poste (entre los adyacentes) y por proyección del sistema. Con frentes iguales las tres coinciden y una
    /// prueba verde no probaría nada; la primera prueba del fichero comprueba justo eso antes que ninguna otra.
    ///
    /// Dato MEDIDO, no supuesto: la PROFUNDIDAD por sí sola no mueve la inserción baja —al alargar la cama sube
    /// también el larguero posterior y la diferencia se cancela—, así que quien separa los frentes aquí es la
    /// altura del primer nivel.
    ///
    /// Dos cosas quedan expresamente fuera, con centinela propio: el PRIMER nivel conserva su contrato selectivo
    /// (primer troquel + altura) y el extremo POSTERIOR sigue midiéndose desde <c>EntranceElevation</c>, que es
    /// donde está su larguero ancla.
    /// </summary>
    public class PushBackElevationConsumerTests
    {
        private const double Offset = SelectiveDesviadorPlan.BeamYOffset;   // 6"

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string DesviadorId(RackCatalog catalog)
            => catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DesviadorType)).Id;

        /// <summary>
        /// Tres frentes jagged: niveles [2, 3, 3], profundidades [4, 6, 4] tarimas y primer nivel [4", 12", 8"].
        ///
        /// De ahí salen cuatro postes que ven cosas distintas: el 1 = {f0, f1} gana f1 por NIVELES; el 2 = {f1, f2}
        /// empata a niveles y gana f1 por PROFUNDIDAD —el desempate del resolver—; la proyección del sistema entero
        /// también es f1. Y el corte lateral del poste 1 toma su columna baja de f0, no de f1: ahí es exactamente
        /// donde «por frente» y «por poste» dejan de ser lo mismo.
        /// </summary>
        private static PushBackSystem Jagged(RackCatalog catalog, SafetySide side = SafetySide.Left)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0,
                    NumberLevels = true,
                    NumberFronts = true,
                    Dimensions = DimensionDetail.Detailed
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 4.0 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1, FirstLevelHeight = 12.0 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 8.0 });
            design.Fronts.Add(new PushBackFrontConfig());
            design.Fronts.Add(new PushBackFrontConfig());
            design.Fronts.Add(new PushBackFrontConfig());

            design.Structure.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = DesviadorId(catalog),
                Quantity = 1,
                Side = side,
                DesviadorLongitud = SelectiveSafetyDefaults.DesviadorLongitud,
                DesviadorPrimerNivelAltura = SelectiveSafetyDefaults.DesviadorPrimerNivelAltura
            });

            return new PushBackResolver(catalog).Resolve(design);
        }

        /// <summary>La elevación derivada de un frente, por número de nivel — la autoridad, sin intermediarios.</summary>
        private static IReadOnlyDictionary<int, double> Derived(PushBackSystem system, RackCatalog catalog, int frontIndex)
            => PushBackElevations.LowInsertions(system, catalog, system.Structure.Fronts[frontIndex]);

        /// <summary>
        /// El frente que gana por la regla del RESOLVER —más niveles y, en empate, mayor profundidad— aplicada al
        /// ámbito que se le pase. La prueba la reproduce a mano a propósito: si el contexto usara otra regla, esto
        /// no lo taparía.
        /// </summary>
        private static int WinnerAmong(PushBackSystem system, params int[] frontIndexes)
            => frontIndexes
                .OrderByDescending(index => system.Structure.Fronts[index].LoadBeamLevels.Count)
                .ThenByDescending(index => system.Structure.Fronts[index].EndX - system.Structure.Fronts[index].StartX)
                .First();

        private static int PostWinner(PushBackSystem system, int postIndex)
            => WinnerAmong(system, DynamicFrontGeometry.AdjacentFronts(system.Structure, postIndex)
                .Select(front => front.Index).ToArray());

        private static int ProjectedWinner(PushBackSystem system)
            => WinnerAmong(system, system.Structure.Fronts.Select(front => front.Index).ToArray());

        private static List<HeaderBlockInstance> Of(IEnumerable<HeaderBlockInstance> instances, string pieceId)
            => instances.Where(i => string.Equals(i.PieceId, pieceId, StringComparison.OrdinalIgnoreCase)).ToList();

        private static List<double> Ys(IEnumerable<HeaderBlockInstance> instances)
            => instances.Select(i => Math.Round(i.Insertion.Y, 6)).OrderBy(y => y).ToList();

        // ---------------------------------------------------------------------------------------------------
        // El escenario tiene que ser realmente discriminante, o el resto no probaría nada.
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void TheJaggedScenario_ActuallySeparatesTheThreeQueries()
        {
            var catalog = Catalog;
            var system = Jagged(catalog);

            Assert.Equal(new[] { 2, 3, 3 }, system.Structure.Fronts.Select(f => f.LoadBeamLevels.Count).ToArray());

            var depth = system.Structure.Fronts.Select(f => f.EndX - f.StartX).ToList();
            Assert.True(depth[1] > depth[2], $"f1 debe ser más profundo que f2 ({depth[1]:F3} vs {depth[2]:F3})");

            // La regla del resolver elige frentes distintos según el ámbito.
            Assert.Equal(0, PostWinner(system, 0));
            Assert.Equal(1, PostWinner(system, 1));   // gana por NIVELES
            Assert.Equal(1, PostWinner(system, 2));   // empate a niveles: gana por PROFUNDIDAD
            Assert.Equal(2, PostWinner(system, 3));
            Assert.Equal(1, ProjectedWinner(system));

            // Y las elevaciones derivadas de esos frentes son REALMENTE distintas entre sí.
            var d0 = Derived(system, catalog, 0);
            var d1 = Derived(system, catalog, 1);
            var d2 = Derived(system, catalog, 2);
            Assert.True(Math.Abs(d0[1] - d1[1]) > 1e-6, $"f0 y f1 derivan lo mismo ({d0[1]:F4})");
            Assert.True(Math.Abs(d1[1] - d2[1]) > 1e-6, $"f1 y f2 derivan lo mismo ({d1[1]:F4})");
            Assert.True(Math.Abs(d0[1] - d2[1]) > 1e-6, $"f0 y f2 derivan lo mismo ({d0[1]:F4})");

            // En el poste 1, «por frente» y «por poste» divergen: el corte lateral toma su columna baja de f0
            // mientras que la proyección del poste gana f1. Sin esto las dos consultas serían indistinguibles.
            var lowFrontAtPost1 = DynamicFrontGeometry.AdjacentFronts(system.Structure, 1).OrderBy(f => f.StartX).First();
            Assert.Equal(0, lowFrontAtPost1.Index);
            Assert.NotEqual(lowFrontAtPost1.Index, PostWinner(system, 1));

            // Decision final del dueño: el extremo BAJO es el ANCLA, asi que la derivada CoINCIDE con la elevacion
            // de salida del resolver — y eso es justo lo que hay que garantizar. Lo que sigue discriminando entre
            // ambitos es el extremo ALTO, que se deriva de la longitud de cada cama.
            foreach (var front in system.Structure.Fronts)
            {
                var derived = Derived(system, catalog, front.Index);
                foreach (var level in front.LoadBeamLevels)
                {
                    Assert.True(
                        Math.Abs(derived[level.LevelNumber] - level.ExitElevation) <= 1e-6,
                        $"frente {front.Index} nivel {level.LevelNumber}: el ancla bajo tiene que ser la del resolver");
                }
            }

            var highs = system.Structure.Fronts
                .Select(front => string.Join(
                    ",",
                    PushBackElevations.HighInsertions(system, catalog, front)
                        .OrderBy(entry => entry.Key)
                        .Select(entry => Math.Round(entry.Value, 6))))
                .Distinct()
                .ToList();
            Assert.True(highs.Count > 1, "el fixture jagged tiene que discriminar por el extremo alto");
        }




        /// <summary>
        /// El lateral NO seccionado dibuja sus largueros resolviendo con <c>front = null</c>, es decir sobre la
        /// proyección del sistema; sus anotaciones consultan por proyección. Las dos vías tienen que dar lo mismo o
        /// las cotas mentirían sobre las piezas que acompañan — y son vías distintas, así que no basta con suponerlo.
        /// </summary>
        [Fact]
        public void TheWholeLateral_AnnotatesTheSameElevationsItDraws()
        {
            var catalog = Catalog;
            var system = Jagged(catalog);

            var byNullFront = PushBackElevations.LowInsertions(system, catalog, null);
            var byProjection = Derived(system, catalog, ProjectedWinner(system));
            Assert.NotEmpty(byNullFront);
            Assert.Equal(
                byNullFront.OrderBy(e => e.Key).Select(e => (e.Key, Math.Round(e.Value, 6))).ToList(),
                byProjection.OrderBy(e => e.Key).Select(e => (e.Key, Math.Round(e.Value, 6))).ToList());

            // Y lo que de verdad se dibuja coincide con las dos.
            var beams = PushBackLoadBeamGeometry.LowBeams(system, catalog, null)
                .Select(beam => Math.Round(beam.Insertion.Y, 6)).Distinct().OrderBy(y => y).ToList();
            Assert.Equal(byProjection.Values.Select(y => Math.Round(y, 6)).OrderBy(y => y).ToList(), beams);

            // Las anotaciones del lateral entero caen sobre esas mismas elevaciones.
            var instances = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances;
            Assert.Equal(beams, LevelLabelYs(instances, "LATERAL"));
        }

        // ---------------------------------------------------------------------------------------------------
        // 1. Desviador superior del corte LATERAL = elevación DEL FRENTE bajo − 6"
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TheLateralUpperDesviador_HangsFromTheDerivedLowBeamOfItsOwnFront(int postIndex)
        {
            var catalog = Catalog;
            var system = Jagged(catalog);
            var id = DesviadorId(catalog);

            var drawn = Of(new PushBackSystemLateralBuilder().Build(system, catalog, postIndex).Flatten().Instances, id);
            Assert.NotEmpty(drawn);

            // El corte lateral toma su columna baja del frente adyacente de MENOR StartX, y cuando ese frente tiene
            // menos niveles repite el último. Se reproduce aquí tal cual: lo que se mide es la ELEVACIÓN, no el
            // recuento.
            var adjacent = DynamicFrontGeometry.AdjacentFronts(system.Structure, postIndex);
            var lowLevels = adjacent.OrderBy(front => front.StartX).First().LoadBeamLevels;
            var highLevels = adjacent.OrderByDescending(front => front.EndX).First().LoadBeamLevels;
            var derived = Derived(system, catalog, adjacent.OrderBy(front => front.StartX).First().Index);
            var count = Math.Min(
                DynamicFrontGeometry.LoadLevelsAtPost(system.Structure, postIndex),
                Math.Max(lowLevels.Count, highLevels.Count));

            var expected = new List<double>();
            for (var level = 1; level < count; level++)   // el nivel 0 conserva el contrato selectivo
            {
                var number = lowLevels[Math.Min(level, lowLevels.Count - 1)].LevelNumber;
                expected.Add(Math.Round(derived[number] - Offset, 6));
            }

            Assert.NotEmpty(expected);
            // Se comparan las elevaciones DISTINTAS: un corte es una proyeccion y dibuja cada pieza una vez, asi
            // que dos desviadores que caen en la misma elevacion son UNO. Comparar la lista con repeticiones medía
            // cuantas veces se dibujaba lo mismo, que es justo lo que se corrigio (I-42, ronda post-82e918b).
            Assert.Equal(
                expected.Distinct().OrderBy(y => y).ToList(),
                Ys(drawn).Skip(1).Distinct().ToList());
        }

        // ---------------------------------------------------------------------------------------------------
        // 2. Desviador superior del corte FRONTAL bajo = elevación DEL POSTE − 6"
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void TheLowFrontalUpperDesviador_HangsFromThePostDerivedLowBeam()
        {
            var catalog = Catalog;
            var system = Jagged(catalog);
            var id = DesviadorId(catalog);

            var layout = DynamicFrontGeometry.Compute(system.Structure, catalog);
            var drawn = Of(
                new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida)
                    .Flatten().Instances,
                id);
            Assert.NotEmpty(drawn);

            var checkedUpper = 0;
            foreach (var column in drawn.GroupBy(i => Math.Round(i.Insertion.X, 6)))
            {
                var postIndex = layout.PostPositions
                    .Select((x, index) => (index, distance: Math.Abs(x - column.Key)))
                    .OrderBy(t => t.distance).First().index;
                var derived = Derived(system, catalog, PostWinner(system, postIndex));

                // El más bajo de la columna es el del primer nivel y no cuelga de ningún larguero.
                foreach (var instance in column.OrderBy(i => i.Insertion.Y).Skip(1))
                {
                    var hangs = derived.Values.Any(elevation => Math.Abs(elevation - Offset - instance.Insertion.Y) < 1e-6);
                    Assert.True(hangs,
                        $"desviador frontal del poste {postIndex} a Y={instance.Insertion.Y:F4}: no cuelga de ninguna " +
                        $"elevación derivada ({string.Join(", ", derived.Values.OrderBy(v => v).Select(v => (v - Offset).ToString("F4")))})");
                    checkedUpper++;
                }
            }

            Assert.True(checkedUpper > 0, "el escenario no dibujó ningún desviador superior en el corte frontal bajo");
        }

        // ---------------------------------------------------------------------------------------------------
        // 3. Cotas y etiquetas
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TheLateralDimensionsAndLabels_ReadThePostDerivedElevations(int postIndex)
        {
            var catalog = Catalog;
            var system = Jagged(catalog);

            var instances = new PushBackSystemLateralBuilder().Build(system, catalog, postIndex).Flatten().Instances;
            var derived = Derived(system, catalog, PostWinner(system, postIndex));
            var levelCount = Math.Min(
                DynamicFrontGeometry.LoadLevelsAtPost(system.Structure, postIndex),
                system.Structure.LoadBeamLevels.Count);

            var expected = system.Structure.LoadBeamLevels
                .Take(levelCount)
                .Select(level => Math.Round(derived[level.LevelNumber], 6))
                .OrderBy(y => y)
                .ToList();
            Assert.NotEmpty(expected);

            Assert.Equal(expected, LevelLabelYs(instances, "LATERAL"));
            Assert.All(expected, y => Assert.Contains(y, ElevationDimensionTops(instances, "LATERAL")));
        }

        [Fact]
        public void TheLowFrontalDimensionsAndLabels_ReadTheProjectedSystemElevations()
        {
            var catalog = Catalog;
            var system = Jagged(catalog);

            var instances = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances;

            // La vista frontal recorre la proyección del sistema entera, así que sus anotaciones leen el frente que
            // gana esa proyección: la misma regla del resolver aplicada a TODOS los frentes.
            var derived = Derived(system, catalog, ProjectedWinner(system));
            var expected = system.Structure.LoadBeamLevels
                .Select(level => Math.Round(derived[level.LevelNumber], 6))
                .OrderBy(y => y)
                .ToList();
            Assert.NotEmpty(expected);

            Assert.Equal(expected, LevelLabelYs(instances, "FRONTAL"));
            Assert.All(expected, y => Assert.Contains(y, ElevationDimensionTops(instances, "FRONTAL")));
        }

        /// <summary>Las Y de las etiquetas de NIVEL: la columna numerada a la izquierda del rack.</summary>
        private static List<double> LevelLabelYs(IEnumerable<HeaderBlockInstance> instances, string view)
            => instances
                .Where(i => i.Role == HeaderBlockRole.Annotation
                    && string.Equals(i.View, view, StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(i.Text, out _)
                    && i.Insertion.Y > 0.0)
                .Select(i => Math.Round(i.Insertion.Y, 6))
                .OrderBy(y => y)
                .ToList();

        /// <summary>Las Y superiores de las cotas verticales de ELEVACIÓN, las que arrancan del suelo.</summary>
        private static List<double> ElevationDimensionTops(IEnumerable<HeaderBlockInstance> instances, string view)
            => instances
                .Where(i => i.Role == HeaderBlockRole.Dimension
                    && string.Equals(i.View, view, StringComparison.OrdinalIgnoreCase)
                    && Math.Abs(i.ConnectionAnchor.X - i.Insertion.X) < 1e-9
                    && Math.Abs(i.Insertion.Y) < 1e-9)
                .Select(i => Math.Round(i.ConnectionAnchor.Y, 6))
                .Distinct()
                .ToList();

        // ---------------------------------------------------------------------------------------------------
        // 4. Lo que NO se toca
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// EN EL DINÁMICO, el extremo POSTERIOR del desviador se mide desde <c>EntranceElevation</c> y ahí se queda.
        ///
        /// <para>
        /// Este centinela es del DINÁMICO y solo del Dinámico. Es la rama del builder lateral compartido que el
        /// override de elevaciones no debe tocar, y ahí sigue valiendo.
        /// </para>
        /// <para>
        /// NO es el contrato del Push Back COMPUESTO. Desde I-42 (corrección aislada del desviador) un rack
        /// compuesto no toma sus desviadores de esta rama: los construye por CAMA, en el extremo BAJO de cada una
        /// (<c>PushBackDiverterPlan</c>), porque «izquierda = extremo bajo» es falso en cuanto el lado B tiene su
        /// entrada a la derecha. Un Push Back de un solo sentido sí sigue pasando por aquí, y por eso se comprueba
        /// también que su lado se colapsa al extremo bajo.
        /// </para>
        /// </summary>
        [Fact]
        public void InTheDynamic_TheRearEndDesviador_StillHangsFromTheEntranceElevation()
        {
            var catalog = Catalog;
            var id = DesviadorId(catalog);
            var pushBack = Jagged(catalog, SafetySide.Both);

            // Push Back de UN SOLO SENTIDO: el lado se colapsa al extremo bajo y no hay copia posterior. Se afirma
            // para que, si algún día dejara de colapsarse, esta prueba lo diga en vez de callarlo. El rack
            // COMPUESTO no depende de esto: sus desviadores los pone la cama.
            Assert.All(
                pushBack.SafetySelections.Where(s => string.Equals(s.ElementId, id, StringComparison.OrdinalIgnoreCase)),
                selection => Assert.Equal(SafetySide.Left, selection.Side));

            var dynamicSystem = DynamicJagged(catalog);
            const int postIndex = 2;
            var rear = Of(new DynamicSystemLateralBuilder().Build(dynamicSystem, catalog, postIndex).Flatten().Instances, id)
                .Where(i => i.MirroredX)
                .ToList();
            Assert.NotEmpty(rear);

            var adjacent = DynamicFrontGeometry.AdjacentFronts(dynamicSystem, postIndex);
            var highLevels = adjacent.OrderByDescending(front => front.EndX).First().LoadBeamLevels;
            var lowLevels = adjacent.OrderBy(front => front.StartX).First().LoadBeamLevels;
            var count = Math.Min(
                DynamicFrontGeometry.LoadLevelsAtPost(dynamicSystem, postIndex),
                Math.Max(lowLevels.Count, highLevels.Count));

            var expected = new List<double>();
            for (var level = 1; level < count; level++)
            {
                expected.Add(Math.Round(highLevels[Math.Min(level, highLevels.Count - 1)].EntranceElevation - Offset, 6));
            }

            Assert.NotEmpty(expected);
            Assert.Equal(expected.OrderBy(y => y).ToList(), Ys(rear).Skip(1).ToList());
        }

        /// <summary>La misma estructura jagged, resuelta como sistema DINÁMICO — sin nada de Push Back.</summary>
        private static DynamicRackSystem DynamicJagged(RackCatalog catalog)
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 4.0 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1, FirstLevelHeight = 12.0 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 8.0 });
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = DesviadorId(catalog),
                Quantity = 1,
                Side = SafetySide.Both,
                DesviadorLongitud = SelectiveSafetyDefaults.DesviadorLongitud,
                DesviadorPrimerNivelAltura = SelectiveSafetyDefaults.DesviadorPrimerNivelAltura
            });

            return new DynamicRackSystemResolver(catalog).Resolve(design).System;
        }

        [Fact]
        public void TheFirstLevelDesviador_KeepsTheSelectiveTroquelContract()
        {
            var catalog = Catalog;
            var system = Jagged(catalog);
            var id = DesviadorId(catalog);

            var lowest = Of(new PushBackSystemLateralBuilder().Build(system, catalog, 1).Flatten().Instances, id)
                .Min(i => i.Insertion.Y);

            // Se mide desde el TROQUEL_LARGUERO del poste más la altura configurada, así que no puede coincidir con
            // ninguna elevación de larguero menos 6" — ni la del resolver ni la derivada.
            var derived = Derived(system, catalog, 0);
            Assert.All(derived.Values, elevation =>
                Assert.True(Math.Abs(elevation - Offset - lowest) > 1e-6,
                    "el primer desviador dejó de usar el contrato selectivo de primer troquel + altura"));
            Assert.All(system.Structure.Fronts[0].LoadBeamLevels, level =>
                Assert.True(Math.Abs(level.ExitElevation - Offset - lowest) > 1e-6,
                    "el primer desviador quedó colgando de un larguero"));
        }
    }
}
