using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Domain.Systems;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-33 — el HANDOFF del desviador: qué entrega cada ventana al diálogo compartido y qué gobierna cada cosa.
    ///
    /// <para>
    /// El diálogo tenía DOS decisiones soldadas en un solo parámetro: <c>desviadorLevelsPerPost</c> fijaba la FORMA de
    /// la rejilla y, de paso, ocultaba el selector de cara de pasillo. Push Back quería ambas a la vez, pero el Dinámico
    /// necesita la forma por poste y DEBE conservar su selector: con la regla derivada lo habría perdido en silencio.
    /// Ahora son parámetros independientes.
    /// </para>
    ///
    /// <para>
    /// Las pruebas recorren el camino REAL de <see cref="RackDynamicSystemWindow"/> (carga un diseño y pregunta a la
    /// ventana lo mismo que le pasa al diálogo), no una reconstrucción del cálculo.
    /// </para>
    /// </summary>
    public sealed class BlankFrontDesviadorHandoffTests
    {
        /// <summary>Un diseño dinámico con un frente por cada conteo de niveles pedido.</summary>
        private static DynamicRackDesign Design(params int[] levelsPerFront)
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = levelsPerFront.Max(),
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            foreach (var levels in levelsPerFront)
            {
                design.Fronts.Add(new DynamicRackFrontDesign
                {
                    PalletCount = 1, LoadLevels = levels, PalletsDeep = 4, DepthStartPosition = 1
                });
            }

            return design;
        }

        /// <summary>Abre la ventana real, aplica los frentes en blanco pedidos y devuelve las DOS listas.</summary>
        private static (int[] PerFrente, int[] PerPost) Handoff(int[] levelsPerFront, params int[] blankFronts)
            => StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow();
                try
                {
                    window.LoadDesignForNew(Design(levelsPerFront), "DIN-I33-HANDOFF");
                    foreach (var index in blankFronts)
                    {
                        // Por la casilla REAL de la cabecera, que corre el handler de la ventana.
                        EditorWindowTestSupport.SetFrontBlank(window, "DynamicMatrixGrid", index, blank: true);
                    }

                    return (window.SafetyLevelsPerFrente().ToArray(), window.DesviadorLevelsPerPost().ToArray());
                }
                finally { window.Close(); }
            });

        // ---- Las dos listas son cosas distintas ---------------------------------------------------------------

        [Fact]
        public void ActiveFronts_WithDifferentLevels_GetPerFrenteAndPerPostLists()
        {
            var handoff = Handoff(new[] { 3, 1, 4 });

            // N frentes ⇒ N conteos por frente y N+1 por poste. La guía consume el primero; el desviador, el segundo.
            Assert.Equal(new[] { 3, 1, 4 }, handoff.PerFrente);
            Assert.Equal(new[] { 3, 3, 4, 4 }, handoff.PerPost);
            Assert.Equal(handoff.PerFrente.Length + 1, handoff.PerPost.Length);
        }

        [Fact]
        public void TheLastPost_InheritsItsAdjacentFront_AndNeverFallsToOneArtificially()
        {
            // Éste es el defecto de contrato que el Dinámico arrastraba: entregaba la lista por FRENTE marcada como
            // por poste, así que el índice del último poste caía fuera de la lista y la rejilla lo dejaba en 1 nivel.
            var handoff = Handoff(new[] { 3, 5 });

            Assert.Equal(new[] { 3, 5 }, handoff.PerFrente);
            Assert.Equal(new[] { 3, 5, 5 }, handoff.PerPost);
            Assert.NotEqual(1, handoff.PerPost[handoff.PerPost.Length - 1]);

            // Y con un solo frente el rack tiene dos postes, ambos con los niveles de ese frente.
            Assert.Equal(new[] { 4, 4 }, Handoff(new[] { 4 }).PerPost);
        }

        // ---- Frentes en blanco en cada posición ----------------------------------------------------------------

        [Fact]
        public void ALeadingBlankFront_EmptiesItsColumnAndLeavesTheOthersUntouched()
        {
            var handoff = Handoff(new[] { 3, 2, 4 }, 0);

            Assert.Equal(new[] { 0, 2, 4 }, handoff.PerFrente);
            // El poste 0 solo tiene al frente en blanco: se queda sin niveles. El 1 hereda del frente 1.
            Assert.Equal(new[] { 0, 2, 4, 4 }, handoff.PerPost);
        }

        [Fact]
        public void AMiddleBlankFront_LeavesItsTwoPostsOnTheirActiveNeighbours()
        {
            var handoff = Handoff(new[] { 3, 2, 4 }, 1);

            Assert.Equal(new[] { 3, 0, 4 }, handoff.PerFrente);
            Assert.Equal(new[] { 3, 3, 4, 4 }, handoff.PerPost);
        }

        [Fact]
        public void ATrailingBlankFront_EmptiesOnlyTheLastPost()
        {
            var handoff = Handoff(new[] { 3, 2, 4 }, 2);

            Assert.Equal(new[] { 3, 2, 0 }, handoff.PerFrente);
            // El último poste solo toca al frente en blanco; el anterior conserva el frente 1.
            Assert.Equal(new[] { 3, 3, 2, 0 }, handoff.PerPost);
        }

        [Fact]
        public void TwoConsecutiveBlankFronts_LeaveTheirSharedPostEmpty()
        {
            var handoff = Handoff(new[] { 3, 2, 2, 4 }, 1, 2);

            Assert.Equal(new[] { 3, 0, 0, 4 }, handoff.PerFrente);
            // El poste 2 está entre los dos frentes en blanco: ningún vecino lleva carga.
            Assert.Equal(new[] { 3, 3, 0, 4, 4 }, handoff.PerPost);
        }

        [Fact]
        public void ReactivatingAFront_RestoresBothListsExactly()
        {
            var before = Handoff(new[] { 3, 2, 4 });
            var after = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow();
                try
                {
                    window.LoadDesignForNew(Design(3, 2, 4), "DIN-I33-RT");
                    EditorWindowTestSupport.SetFrontBlank(window, "DynamicMatrixGrid", 1, blank: true);
                    EditorWindowTestSupport.SetFrontBlank(window, "DynamicMatrixGrid", 1, blank: false);
                    return (window.SafetyLevelsPerFrente().ToArray(), window.DesviadorLevelsPerPost().ToArray());
                }
                finally { window.Close(); }
            });

            Assert.Equal(before.PerFrente, after.Item1);
            Assert.Equal(before.PerPost, after.Item2);
        }

        // ---- Lo que la rejilla REAL construye con cada handoff -------------------------------------------------

        /// <summary>The desviador grid built from <paramref name="counts"/> over <paramref name="postCount"/> columns,
        /// always flagged per-post — which is how both the old and the new Dinámico handoffs reached it.</summary>
        private static SafetyDesviadorGridWindow Grid(IReadOnlyList<int> counts, int postCount)
            => new SafetyDesviadorGridWindow(
                "DESVIADOR", "Desviador", system: null, catalog: null,
                longitud: 12.0, firstHeight: 12.0, side: SafetySide.Both,
                offCells: new SelectiveGridCell[0],
                fallbackPostCount: postCount,
                fallbackLevelsPerFrente: counts,
                fallbackLevelsArePerPost: true,
                showSide: true,
                allowBlankColumns: true);

        [Fact]
        public void TheNewHandoff_GivesTheGridTheLevelsTheDrawingPlaces_WhereTheOldOneCollapsedTheLastPost()
        {
            StaTestRunner.Run(() =>
            {
                var handoff = Handoff(new[] { 3, 5 });

                // ANTES: la lista por FRENTE ({3,5}) sobre los N+1 = 3 postes de la rejilla. El índice del último
                // poste cae FUERA de la lista y la rejilla lo deja en 1 nivel — celdas que el dibujo coloca y el
                // usuario no podía apagar.
                var old = Grid(handoff.PerFrente, postCount: handoff.PerFrente.Length + 1);
                try
                {
                    Assert.Equal(3, old.Model.Columns);
                    Assert.True(old.Model.IsAbsent(2, 1), "el último poste ofrecía un solo nivel");
                    Assert.True(old.Model.IsAbsent(2, 4));
                }
                finally { old.Close(); }

                // AHORA: la lista por POSTE ({3,5,5}). El último poste hereda los 5 niveles de su frente adyacente.
                var fixedGrid = Grid(handoff.PerPost, postCount: handoff.PerPost.Length);
                try
                {
                    Assert.Equal(3, fixedGrid.Model.Columns);
                    Assert.False(fixedGrid.Model.IsAbsent(2, 1));
                    Assert.False(fixedGrid.Model.IsAbsent(2, 4));
                    Assert.Equal(2, fixedGrid.Model.AbsentCount);   // solo la cola dentada del poste 0 (3 de 5)
                }
                finally { fixedGrid.Close(); }
            });
        }

        [Fact]
        public void TheNewHandoff_BuildsAnEmptyColumnForEveryBlankNeighbourhood()
        {
            StaTestRunner.Run(() =>
            {
                // Dos frentes en blanco consecutivos: el poste que comparten queda sin celdas, los demás conservan
                // las suyas y ninguna columna se pierde ni se corre de sitio.
                var handoff = Handoff(new[] { 3, 2, 2, 4 }, 1, 2);
                var grid = Grid(handoff.PerPost, postCount: handoff.PerPost.Length);
                try
                {
                    Assert.Equal(5, grid.Model.Columns);           // 4 frentes ⇒ 5 postes, ninguno desaparece
                    for (var level = 0; level < 4; level++)
                    {
                        Assert.True(grid.Model.IsAbsent(2, level));
                    }

                    Assert.False(grid.Model.IsAbsent(0, 2));
                    Assert.False(grid.Model.IsAbsent(4, 3));
                }
                finally { grid.Close(); }
            });
        }

        // ---- El selector de lado es una decisión INDEPENDIENTE de la forma de la rejilla -----------------------

        [Fact]
        public void TheSideSelector_IsIndependentOfThePerPostShape()
        {
            StaTestRunner.Run(() =>
            {
                var elements = new List<RackCad.Application.Catalogs.SafetyElementCatalogEntry>();
                var perPost = new[] { 3, 3, 2 };

                // Entregar la forma por poste NO oculta el selector: el default lo conserva (Selectivo y Dinámico)...
                var shown = new SelectiveSafetyWindow(
                    elements, null, postCount: 3, levelsPerFrente: new[] { 3, 2 },
                    desviadorLevelsPerPost: perPost);
                // ...y ocultarlo NO exige renunciar a la forma por poste (Push Back).
                var hidden = new SelectiveSafetyWindow(
                    elements, null, postCount: 3, levelsPerFrente: new[] { 3, 2 },
                    desviadorLevelsPerPost: perPost, showDesviadorSide: false);
                // El selector tampoco depende de NO entregarla.
                var shownWithoutPerPost = new SelectiveSafetyWindow(
                    elements, null, postCount: 3, levelsPerFrente: new[] { 3, 2 });

                try
                {
                    Assert.True(shown.ShowsDesviadorSide);
                    Assert.False(hidden.ShowsDesviadorSide);
                    Assert.True(shownWithoutPerPost.ShowsDesviadorSide);
                }
                finally
                {
                    shown.Close();
                    hidden.Close();
                    shownWithoutPerPost.Close();
                }
            });
        }

        // ---- Quién pasa qué: guardia de fuente sobre las tres ventanas ----------------------------------------

        private static string Source(string fileName)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se localizó la raíz del repo (RackCad.sln).");
            var path = Path.Combine(dir.FullName, "src", "RackCad.UI", fileName);
            Assert.True(File.Exists(path), "No existe " + path);
            return File.ReadAllText(path);
        }

        [Fact]
        public void Dynamic_HandsBothListsAndKeepsItsSideSelector()
        {
            var source = Source("RackDynamicSystemWindow.xaml.cs");

            Assert.Contains("levelsPerFrente: levels", source);
            Assert.Contains("desviadorLevelsPerPost: DesviadorLevelsPerPost()", source);
            Assert.Contains("showDesviadorSide: true", source);
            Assert.DoesNotContain("showDesviadorSide: false", source);
        }

        [Fact]
        public void PushBack_KeepsTheCanonicalPerPostListAndHidesItsSideSelectorExplicitly()
        {
            var source = Source("RackPushBackSystemWindow.xaml.cs");

            Assert.Contains("desviadorLevelsPerPost: DesviadorLevelsPerPost()", source);
            Assert.Contains("showDesviadorSide: false", source);
        }

        [Fact]
        public void Selective_KeepsEveryDefault_SoItsDialogIsUnchanged()
        {
            var source = Source("RackSelectiveWindow.xaml.cs");

            // El Selectivo no opta por ninguno de los parámetros de I-33 ni por el de PB-002.
            Assert.DoesNotContain("desviadorLevelsPerPost", source);
            Assert.DoesNotContain("showDesviadorSide", source);
            Assert.DoesNotContain("allowBlankFrontColumns", source);
            Assert.DoesNotContain("fallbackLevelsArePerPost", source);
        }

        [Fact]
        public void Guia_ReceivesThePerFrenteList_NotTheNPlusOnePostList()
        {
            // La guía se dibuja por FRENTE y nivel: con 3 frentes debe recibir 3 columnas, no 4.
            var handoff = Handoff(new[] { 3, 2, 4 });

            Assert.Equal(3, handoff.PerFrente.Length);
            Assert.Equal(4, handoff.PerPost.Length);
            Assert.NotEqual(handoff.PerPost, handoff.PerFrente);

            // Y el diálogo sigue entregando esa lista por frente a la guía, no la del desviador.
            var source = Source("SelectiveSafetyWindow.cs");
            Assert.Contains("SelectedElementLabel(row), levelsPerFrente, row.GuiaOffCells", source);
        }
    }
}
