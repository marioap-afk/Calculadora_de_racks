using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004, consumidores (I-32) — el contexto neutral de elevaciones, probado como lo que es: un tipo puro sin
    /// nada de Push Back.
    ///
    /// Lo importante que fija este fichero es la REGLA DE PROYECCIÓN. Es la misma del resolver —mayor cantidad de
    /// niveles y, en empate, mayor profundidad— y hasta ahora no tenía red: la única aserción del repo sobre esa
    /// regla usa un caso SIN empate, así que el desempate por profundidad podía romperse sin que nada avisara.
    /// </summary>
    public class RackLevelElevationsTests
    {
        private static RackFrontLevelElevations Front(int index, int levels, double depth, params double[] elevations)
            => new RackFrontLevelElevations(
                index, levels, depth,
                elevations.Select((value, i) => (Number: i + 1, Value: value))
                    .ToDictionary(entry => entry.Number, entry => entry.Value));

        // ---- La regla de proyección ----

        [Fact]
        public void MoreLevelsWins_EvenAgainstAMuchDeeperFront()
        {
            var context = RackLevelElevations.From(new[]
            {
                Front(0, levels: 2, depth: 900.0, 10.0, 20.0),
                Front(1, levels: 3, depth: 100.0, 11.0, 21.0, 31.0),
            });

            Assert.Equal(11.0, context.AtProjectedSystem(1, fallback: -1.0));
        }

        [Fact]
        public void OnATieInLevels_TheDeeperFrontWins()
        {
            var context = RackLevelElevations.From(new[]
            {
                Front(0, levels: 3, depth: 204.0, 10.0, 20.0, 30.0),
                Front(1, levels: 3, depth: 300.0, 11.0, 21.0, 31.0),
                Front(2, levels: 3, depth: 204.0, 12.0, 22.0, 32.0),
            });

            Assert.Equal(11.0, context.AtProjectedSystem(1, fallback: -1.0));
        }

        /// <summary>
        /// Empate en los DOS criterios: gana el frente de menor índice. El resolver lo consigue porque la ordenación
        /// de LINQ es estable y su entrada va por índice; el contexto tiene que reproducirlo, no aproximarlo.
        /// </summary>
        [Fact]
        public void OnATieInBothCriteria_TheLowestFrontIndexWins()
        {
            var context = RackLevelElevations.From(new[]
            {
                Front(2, levels: 3, depth: 204.0, 12.0),
                Front(0, levels: 3, depth: 204.0, 10.0),
                Front(1, levels: 3, depth: 204.0, 11.0),
            });

            Assert.Equal(10.0, context.AtProjectedSystem(1, fallback: -1.0));
        }

        // ---- AtPost: la misma regla, pero SOLO entre los frentes adyacentes ----

        [Fact]
        public void AtPost_ResolvesOnlyAmongTheFrontsAdjacentToThatPost()
        {
            // Tres frentes ⇒ cuatro postes. f1 gana la proyección global, pero no está en todos los postes.
            var context = RackLevelElevations.From(new[]
            {
                Front(0, levels: 2, depth: 204.0, 10.0, 20.0),
                Front(1, levels: 3, depth: 300.0, 11.0, 21.0, 31.0),
                Front(2, levels: 3, depth: 204.0, 12.0, 22.0, 32.0),
            });

            Assert.Equal(10.0, context.AtPost(0, 1, fallback: -1.0));   // {f0}
            Assert.Equal(11.0, context.AtPost(1, 1, fallback: -1.0));   // {f0, f1} → f1 por NIVELES
            Assert.Equal(11.0, context.AtPost(2, 1, fallback: -1.0));   // {f1, f2} → f1 por PROFUNDIDAD
            Assert.Equal(12.0, context.AtPost(3, 1, fallback: -1.0));   // {f2}
            Assert.Equal(11.0, context.AtProjectedSystem(1, fallback: -1.0));

            // El poste 0 NO ve al ganador global: es justo lo que distingue AtPost de AtProjectedSystem.
            Assert.NotEqual(context.AtProjectedSystem(1, -1.0), context.AtPost(0, 1, -1.0));
        }

        [Fact]
        public void AtPost_OutsideTheValidRange_ReturnsTheFallback()
        {
            var context = RackLevelElevations.From(new[] { Front(0, 2, 204.0, 10.0, 20.0) });

            Assert.Equal(-7.0, context.AtPost(-1, 1, fallback: -7.0));
            Assert.Equal(-7.0, context.AtPost(5, 1, fallback: -7.0));
            Assert.Equal(10.0, context.AtPost(0, 1, fallback: -7.0));
            Assert.Equal(10.0, context.AtPost(1, 1, fallback: -7.0));   // un frente ⇒ dos postes
        }

        // ---- La ENVOLVENTE es un ámbito aparte, no una variante de la proyección ----

        /// <summary>
        /// La envolvente llega EXPLÍCITA y no se deduce escogiendo un frente. Aquí se le pasa un mapa que no coincide
        /// con el de ningún frente: si alguna vez se «optimizara» derivándola del ganador, esto lo delataría.
        /// </summary>
        [Fact]
        public void TheEnvelope_IsAnExplicitMap_NotAFrontChosenByTheRule()
        {
            var context = RackLevelElevations.From(
                new[]
                {
                    Front(0, levels: 4, depth: 204.0, 10.0, 20.0, 30.0, 40.0),   // gana la proyección por NIVELES
                    Front(1, levels: 3, depth: 300.0, 11.0, 21.0, 31.0),         // es el más PROFUNDO
                },
                systemEnvelope: new Dictionary<int, double> { [1] = 99.0, [2] = 199.0 });

            Assert.Equal(99.0, context.AtSystemEnvelope(1, fallback: -1.0));
            Assert.Equal(199.0, context.AtSystemEnvelope(2, fallback: -1.0));

            // Y no coincide con NINGÚN frente: no sale de elegir uno.
            Assert.Equal(10.0, context.AtFront(0, 1, fallback: -1.0));
            Assert.Equal(11.0, context.AtFront(1, 1, fallback: -1.0));
        }

        /// <summary>
        /// <c>AtProjectedSystem</c> sigue significando exactamente «el frente que originó la lista proyectada». No se
        /// sobrecarga para significar «el rack entero», que es lo que responde <c>AtSystemEnvelope</c>.
        /// </summary>
        [Fact]
        public void AtProjectedSystem_KeepsMeaningTheProjectedFront_NotTheEnvelope()
        {
            var context = RackLevelElevations.From(
                new[]
                {
                    Front(0, levels: 4, depth: 204.0, 10.0),
                    Front(1, levels: 3, depth: 300.0, 11.0),
                },
                systemEnvelope: new Dictionary<int, double> { [1] = 99.0 });

            Assert.Equal(10.0, context.AtProjectedSystem(1, fallback: -1.0));   // F0: gana por niveles
            Assert.Equal(99.0, context.AtSystemEnvelope(1, fallback: -1.0));    // la envolvente, explícita
            Assert.NotEqual(context.AtProjectedSystem(1, -1.0), context.AtSystemEnvelope(1, -1.0));
        }

        [Fact]
        public void WithNoEnvelopeMap_TheEnvelopeQueryFallsBack_WithoutBorrowingAFront()
        {
            var context = RackLevelElevations.From(new[] { Front(0, 2, 204.0, 10.0, 20.0) });

            Assert.Equal(10.0, context.AtProjectedSystem(1, fallback: -1.0));
            Assert.Equal(-1.0, context.AtSystemEnvelope(1, fallback: -1.0));
        }

        [Fact]
        public void AnEnvelopeAlone_IsStillAValidContext()
        {
            var context = RackLevelElevations.From(
                Array.Empty<RackFrontLevelElevations>(),
                systemEnvelope: new Dictionary<int, double> { [1] = 55.0 });

            Assert.NotNull(context);
            Assert.Equal(55.0, context.AtSystemEnvelope(1, fallback: -1.0));
            Assert.Equal(-1.0, context.AtProjectedSystem(1, fallback: -1.0));
            Assert.Equal(-1.0, context.AtFront(0, 1, fallback: -1.0));
            Assert.Equal(-1.0, context.AtPost(0, 1, fallback: -1.0));
        }

        [Fact]
        public void TheEnvelopeMap_IsCopied_LikeTheFrontMaps()
        {
            var source = new Dictionary<int, double> { [1] = 10.0 };
            var context = RackLevelElevations.From(
                new[] { Front(0, 1, 204.0, 1.0) }, systemEnvelope: source);

            source[1] = 999.0;
            source[2] = 999.0;

            Assert.Equal(10.0, context.AtSystemEnvelope(1, fallback: -1.0));
            Assert.Equal(-1.0, context.AtSystemEnvelope(2, fallback: -1.0));
        }

        // ---- El fallback es literal, siempre ----

        [Fact]
        public void EveryQuery_ReturnsTheLiteralFallback_WhenItHasNoDatum()
        {
            var context = RackLevelElevations.From(new[] { Front(1, 2, 204.0, 10.0, 20.0) });

            Assert.Equal(99.0, context.AtFront(0, 1, fallback: 99.0));    // frente desconocido
            Assert.Equal(99.0, context.AtFront(1, 7, fallback: 99.0));    // nivel desconocido
            Assert.Equal(99.0, context.AtProjectedSystem(7, fallback: 99.0));
            Assert.Equal(99.0, context.AtPost(1, 7, fallback: 99.0));
            Assert.Equal(99.0, context.AtSystemEnvelope(7, fallback: 99.0));
        }

        [Fact]
        public void WithNothingToContribute_ThereIsNoContextAtAll()
        {
            Assert.Null(RackLevelElevations.From(null));
            Assert.Null(RackLevelElevations.From(Array.Empty<RackFrontLevelElevations>()));

            // Un frente sin elevaciones no cuenta: no aporta override y no debe ganar ninguna proyección.
            Assert.Null(RackLevelElevations.From(new[]
            {
                new RackFrontLevelElevations(0, 3, 300.0, new Dictionary<int, double>()),
            }));
        }

        [Fact]
        public void AFrontWithNoElevations_NeverWinsOverOneThatHasThem()
        {
            var context = RackLevelElevations.From(new[]
            {
                new RackFrontLevelElevations(0, 9, 900.0, new Dictionary<int, double>()),   // ganaría la regla…
                Front(1, levels: 2, depth: 100.0, 42.0),                                   // …pero no aporta nada
            });

            Assert.Equal(42.0, context.AtProjectedSystem(1, fallback: -1.0));

            // El poste 0 solo linda con el frente vacío, así que se queda con el fallback: no se le presta la
            // elevación de un frente que no toca. El poste 1 sí linda con el que aporta datos.
            Assert.Equal(-1.0, context.AtPost(0, 1, fallback: -1.0));
            Assert.Equal(42.0, context.AtPost(1, 1, fallback: -1.0));
        }

        // ---- Inmutabilidad ----

        [Fact]
        public void TheContext_DoesNotObserveLaterChangesToTheSourceDictionary()
        {
            var source = new Dictionary<int, double> { [1] = 10.0 };
            var context = RackLevelElevations.From(new[]
            {
                new RackFrontLevelElevations(0, 1, 204.0, source),
            });

            source[1] = 999.0;
            source[2] = 999.0;

            Assert.Equal(10.0, context.AtFront(0, 1, fallback: -1.0));
            Assert.Equal(-1.0, context.AtFront(0, 2, fallback: -1.0));
        }

        // ---- Las extensiones tolerantes a null ----

        [Fact]
        public void TheNullTolerantExtensions_ReturnTheFallback_ForANullContext()
        {
            RackLevelElevations none = null;

            Assert.Equal(5.0, none.OrFront(0, 1, 5.0));
            Assert.Equal(5.0, none.OrPost(0, 1, 5.0));
            Assert.Equal(5.0, none.OrProjectedSystem(1, 5.0));
            Assert.Equal(5.0, none.OrSystemEnvelope(1, 5.0));
        }
    }
}
