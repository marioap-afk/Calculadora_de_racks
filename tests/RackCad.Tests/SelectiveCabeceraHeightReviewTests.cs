using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-43, gate 8.6E: la revisión de altura de una cabecera personalizada mira TODOS los fondos destino.
    /// <para>
    /// Es una lectura pura sobre el sistema ya resuelto. Cada fondo tiene su topología y sus alturas, así que la misma
    /// receta puede salir limpia en uno, discrepante en otro y peligrosa en un tercero; y un destino que no tiene ese
    /// poste se OMITE, sin crearlo y sin bloquear a los demás.
    /// </para>
    /// </summary>
    public class SelectiveCabeceraHeightReviewTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveBayDesign Bay(double alto, int levels = 2)
        {
            var bay = new SelectiveBayDesign();
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = alto },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            return bay;
        }

        /// <summary>Un rack cuyos fondos tienen alturas de celda —y por tanto alturas de poste— distintas.</summary>
        private static SelectiveRackSystem SystemWith(params (double Alto, int Frentes)[] fondos)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = fondos.Length
            };

            for (var b = 0; b < fondos[0].Frentes; b++) design.Bays.Add(Bay(fondos[0].Alto));
            for (var k = 1; k < fondos.Length; k++)
            {
                design.ExtraFondoBays.Add(Enumerable.Range(0, fondos[k].Frentes).Select(_ => Bay(fondos[k].Alto)).ToList());
            }

            return new SelectiveGeometryResolver().Resolve(design, Catalog);
        }

        private static double TopOf(SelectiveRackSystem system, int fondo, int post)
            => SelectivePostGeometry.TopLevelYAtPost(SelectiveDepthLayout.BaysOfFondo(system, fondo), post);

        // ---- El helper puro ----

        [Fact]
        public void TopLevelYAtPost_TakesTheTallestOfTheTwoAdjacentBays()
        {
            var system = SystemWith((40.0, 2), (90.0, 2));

            Assert.True(TopOf(system, 1, 1) > TopOf(system, 0, 1)); // cada fondo con su propia geometría
            Assert.Equal(0.0, SelectivePostGeometry.TopLevelYAtPost(null, 1)); // sin bahías no hay nivel
        }

        // ---- La revisión por destino ----

        [Fact]
        public void ASevereFindingIsReported_ForEveryTargetItAffects_NotOnlyTheFirst()
        {
            var system = SystemWith((90.0, 2), (90.0, 2));

            var review = SelectiveCabeceraHeightReview.Of(system, new[] { 0, 1 }, 1, 20.0);

            Assert.True(review.HasSevere);
            Assert.Equal(new[] { 0, 1 }, review.Findings.Select(f => f.FondoIndex));
            Assert.All(review.Findings, f => Assert.Equal(SelectiveCabeceraHeightIssue.Severe, f.Issue));
        }

        [Fact]
        public void TheSameHeightCanBeCleanInOneFondo_AndSevereInAnother()
        {
            // Es el caso que la validación de un solo fondo dejaba pasar: el fondo visible está bien y el otro no.
            var system = SystemWith((40.0, 2), (120.0, 2));
            var okForFondoZero = SelectivePostGeometry.PostHeight(
                SelectiveDepthLayout.BaysOfFondo(system, 0), 1, system.Height);

            var review = SelectiveCabeceraHeightReview.Of(system, new[] { 0, 1 }, 1, okForFondoZero);

            Assert.True(review.HasSevere);
            Assert.Equal(new[] { 1 }, review.Findings.Where(f => f.Issue == SelectiveCabeceraHeightIssue.Severe)
                .Select(f => f.FondoIndex));
            Assert.Contains("F2", review.Describe()); // y el mensaje lo nombra, aunque no se vea
        }

        [Fact]
        public void ATargetWithoutThatPost_IsSkipped_NotReportedAsAProblem()
        {
            var system = SystemWith((48.0, 4), (48.0, 1)); // el fondo 2 solo llega al poste 1

            var review = SelectiveCabeceraHeightReview.Of(system, new[] { 0, 1 }, 4, 200.0);

            Assert.Equal(new[] { 1 }, review.SkippedFondos);
            Assert.DoesNotContain(review.Findings, f => f.FondoIndex == 1);
        }

        [Fact]
        public void AFondoThatSharesFondoZerosMatrix_IsReviewed_NotSkipped()
        {
            // BaysOfFondo cae A PROPOSITO al fondo 0 cuando un fondo no tiene matriz propia: es el caso corriente de
            // doble profundidad, donde todos comparten la misma. Ese fondo SI se revisa — meterle una comprobacion de
            // rango lo dejaria fuera de la validacion, que es justo lo contrario de lo que pide el gate.
            var system = SystemWith((90.0, 2));

            var review = SelectiveCabeceraHeightReview.Of(system, new[] { 0, 1 }, 1, 20.0);

            Assert.Empty(review.SkippedFondos);
            Assert.Equal(new[] { 0, 1 }, review.Findings.Select(f => f.FondoIndex));
        }

        [Fact]
        public void AHeightThatMatchesEveryTarget_IsClean()
        {
            var system = SystemWith((48.0, 2), (48.0, 2));
            var resolved = SelectivePostGeometry.PostHeight(
                SelectiveDepthLayout.BaysOfFondo(system, 0), 1, system.Height);

            var review = SelectiveCabeceraHeightReview.Of(system, new[] { 0, 1 }, 1, resolved);

            Assert.True(review.IsClean);
            Assert.Equal(string.Empty, review.Describe());
        }

        [Fact]
        public void ASevereFindingAbsorbsTheInformativeOneOfTheSameFondo()
        {
            // Quedar por debajo del nivel de carga YA implica diferir del alto resuelto: reportar las dos cosas del
            // mismo fondo sería decir dos veces el mismo problema.
            var system = SystemWith((90.0, 2));

            var review = SelectiveCabeceraHeightReview.Of(system, new[] { 0 }, 1, 20.0);

            Assert.Single(review.Findings);
            Assert.Equal(SelectiveCabeceraHeightIssue.Severe, review.Findings[0].Issue);
        }

        // ---- El mensaje consolidado ----

        [Fact]
        public void TheMessageIsOne_AndNamesFondoPostHeightAndReference()
        {
            var system = SystemWith((90.0, 2), (90.0, 2), (48.0, 1));

            var review = SelectiveCabeceraHeightReview.Of(system, new[] { 0, 1, 2 }, 2, 20.0);
            var text = review.Describe();

            Assert.Contains("poste F3", text);          // el poste, en la numeración de cara al usuario
            Assert.Contains("20 in", text);             // la altura pedida
            Assert.Contains("F1", text);                // los fondos afectados
            Assert.Contains("F2", text);
            Assert.Contains("Sin ese poste", text);     // y los omitidos, dichos aparte
            Assert.Contains("F3", text);
        }

        [Fact]
        public void AnEmptyTargetSet_ProducesNothing()
        {
            var system = SystemWith((48.0, 2));

            var review = SelectiveCabeceraHeightReview.Of(system, new List<int>(), 1, 20.0);

            Assert.True(review.IsClean);
            Assert.Empty(review.SkippedFondos);
        }
    }
}
