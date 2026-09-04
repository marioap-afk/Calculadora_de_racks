using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-43, gate 8.6F (ARQ-43-06 + O-43-03): cada frontal <c>Fk</c> representa físicamente al fondo <c>k</c>, así que
    /// consulta la cabecera custom de <c>(k, i)</c> — no la del fondo 0.
    /// <para>
    /// La vista de un fondo es un rack de un solo fondo: su fila de cabeceras ES la fila de ese fondo, con los mismos
    /// índices de poste. Y las consultas de existencia son LECTURAS: no pueden mutar la receta almacenada.
    /// </para>
    /// <para>
    /// <c>EffectiveCustomAt</c> conserva a propósito su semántica in-place (impone el Depth del fondo sobre la
    /// configuración almacenada). Purificarla es ARQ-43-10, un follow-up DIFERIDO fuera de I-43, y aquí se
    /// caracteriza para que este gate no lo absorba por accidente.
    /// </para>
    /// </summary>
    public class SelectiveCabeceraViewPurityTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static RackFrameConfiguration Custom(double height)
            => new RackFrameConfigurationFactory(Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, height, 42.0);

        private static SelectiveBayDesign Bay(int levels = 2)
        {
            var bay = new SelectiveBayDesign();
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 48.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            return bay;
        }

        /// <summary>Un diseño de <paramref name="fondos"/> fondos con 2 frentes cada uno (postes 0..2).</summary>
        private static SelectivePalletDesign Design(int fondos)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = fondos
            };

            design.Bays.Add(Bay());
            design.Bays.Add(Bay());
            for (var k = 1; k < fondos; k++) design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(), Bay() });
            return design;
        }

        private static void SetCustom(SelectivePalletDesign design, int fondo, int post, RackFrameConfiguration cfg)
        {
            if (fondo == 0)
            {
                while (design.PostCabeceras.Count <= post) design.PostCabeceras.Add(null);
                design.PostCabeceras[post] = cfg;
                return;
            }

            while (design.ExtraFondoPostCabeceras.Count < fondo) design.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration>());
            var row = design.ExtraFondoPostCabeceras[fondo - 1];
            while (row.Count <= post) row.Add(null);
            row[post] = cfg;
        }

        private static SelectiveRackSystem Resolve(SelectivePalletDesign design)
            => new SelectiveGeometryResolver().Resolve(design, Catalog);

        private static RackFrameConfiguration ViewCabecera(SelectiveRackSystem system, int fondo, int post)
        {
            var view = SelectiveDepthLayout.FondoSystemView(system, fondo);
            return post >= 0 && post < view.PostCabeceras.Count ? view.PostCabeceras[post] : null;
        }

        /// <summary>La altura del POSTE que el frontal de ese fondo dibuja — el mismo camino que usa la preview.</summary>
        private static double FrontalPostHeight(SelectiveRackSystem system, int fondo, int post)
        {
            var view = SelectiveDepthLayout.FondoSystemView(system, fondo);
            var instances = new SelectiveFrontalBuilder().Build(view, Catalog);
            var posts = instances.Where(i => i.Role == HeaderBlockRole.Post)
                .OrderBy(i => i.Insertion.X)
                .ToList();
            return post >= 0 && post < posts.Count && posts[post].DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var longitud)
                ? longitud
                : 0.0;
        }

        // ======================================================================================
        // N. La vista de cada fondo lleva SU propia cabecera custom
        // ======================================================================================

        [Fact]
        public void N_EachFondosViewCarriesItsOwnCustom()
        {
            var design = Design(2);
            SetCustom(design, 0, 1, Custom(200.0)); // A
            SetCustom(design, 1, 1, Custom(300.0)); // B
            var system = Resolve(design);

            Assert.Equal(200.0, ViewCabecera(system, 0, 1)?.Height);
            Assert.Equal(300.0, ViewCabecera(system, 1, 1)?.Height); // hoy se pierde por el `if (k == 0)`
        }

        [Fact]
        public void N_TheViewKeepsThePostIndex_WithoutCompressingTheRow()
        {
            // La custom vive en el poste 2; los postes 0 y 1 no tienen ninguna. Comprimir la fila movería la cabecera
            // a otro poste, que es peor que perderla.
            var design = Design(2);
            SetCustom(design, 1, 2, Custom(310.0));
            var system = Resolve(design);

            Assert.Null(ViewCabecera(system, 1, 0));
            Assert.Null(ViewCabecera(system, 1, 1));
            Assert.Equal(310.0, ViewCabecera(system, 1, 2)?.Height);
        }

        [Fact]
        public void N_TheViewNeverInventsAStandardCabecera()
        {
            // Ausencia significa "derívala": el builder la construye. Rellenar la fila con cabeceras estándar haría
            // indistinguible una custom de una derivada.
            var system = Resolve(Design(2));
            var view = SelectiveDepthLayout.FondoSystemView(system, 1);

            Assert.All(view.PostCabeceras, cabecera => Assert.Null(cabecera));
        }

        // ======================================================================================
        // O. El frontal de ese fondo dibuja esa cabecera (el camino que consume la preview)
        // ======================================================================================

        [Fact]
        public void O_TheFrontalOfAFondoUsesItsOwnCustomHeight()
        {
            var design = Design(2);
            SetCustom(design, 0, 1, Custom(200.0));
            SetCustom(design, 1, 1, Custom(300.0));
            var system = Resolve(design);

            Assert.Equal(200.0, FrontalPostHeight(system, 0, 1), 4);
            Assert.Equal(300.0, FrontalPostHeight(system, 1, 1), 4);
        }

        [Fact]
        public void Paridad_TheFrontalAndTheLateralAuthorityAgreeOnTheSameCabecera()
        {
            // El frontal lee la vista; lateral, planta y BOM leen EffectiveCustomAt. Las dos rutas tienen que
            // describir el mismo poste del mismo fondo (INV-15).
            var design = Design(2);
            SetCustom(design, 1, 1, Custom(300.0));
            var system = Resolve(design);

            var authority = SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1);
            Assert.NotNull(authority);
            Assert.Equal(authority.Height, FrontalPostHeight(system, 1, 1), 4);
        }

        // ======================================================================================
        // P. Legacy: una custom solo en fondo 0 NO se promueve a los demás
        // ======================================================================================

        [Fact]
        public void P_ALegacyDocumentWithOnlyFondoZeroCustoms_LeavesTheOtherFondosStandard()
        {
            var design = Design(3);
            SetCustom(design, 0, 1, Custom(200.0)); // la única, como un documento anterior a I-43
            var system = Resolve(design);

            Assert.Equal(200.0, ViewCabecera(system, 0, 1)?.Height);
            Assert.Null(ViewCabecera(system, 1, 1)); // NO se copia hacia abajo
            Assert.Null(ViewCabecera(system, 2, 1));
            Assert.NotEqual(200.0, FrontalPostHeight(system, 1, 1));
        }

        // ======================================================================================
        // PUREZA 3: preguntar si existe una custom NO puede mutar la receta almacenada
        // ======================================================================================

        [Fact]
        public void Pureza1_UsableCustomAt_DoesNotTouchTheStoredRecipe()
        {
            // El fondo 1 tiene 60" de tarima, asi que su cabecera EFECTIVA mediria 54"; la receta almacenada dice 42.
            var design = Design(2);
            design.ExtraFondoDepths.Add(60.0);
            SetCustom(design, 1, 1, Custom(300.0));
            var system = Resolve(design);

            var raw = SelectiveCabeceraAuthority.CustomAt(system, 1, 1);
            var depthBefore = raw.Depth;
            var membersBefore = raw.Members?.Count ?? 0;

            var usable = SelectiveCabeceraAuthority.UsableCustomAt(system, 1, 1);

            Assert.Same(raw, usable);                             // la misma receta, ni copia ni version efectiva
            Assert.Equal(depthBefore, raw.Depth);                 // sin imponer el Depth del fondo
            Assert.Equal(membersBefore, raw.Members?.Count ?? 0);  // sin refrescar el modelo fisico
        }

        [Fact]
        public void Pureza1_UsableCustomAt_RefusesAnUnusableRecipe()
        {
            var design = Design(2);
            var broken = Custom(300.0);
            broken.Height = 0.0; // sin altura no es un marco usable
            SetCustom(design, 1, 1, broken);
            var system = Resolve(design);

            Assert.Null(SelectiveCabeceraAuthority.UsableCustomAt(system, 1, 1));
            Assert.Null(SelectiveCabeceraAuthority.UsableCustomAt(system, 1, 0)); // no hay ninguna
        }

        [Fact]
        public void Pureza3_HasCustomAt_IsAPureQuestion()
        {
            var design = Design(2);
            design.ExtraFondoDepths.Add(60.0);
            SetCustom(design, 1, 1, Custom(300.0));
            var system = Resolve(design);

            var raw = SelectiveCabeceraAuthority.CustomAt(system, 1, 1);
            var depthBefore = raw.Depth;
            var membersBefore = raw.Members?.Count ?? 0;

            Assert.True(SelectiveCabeceraAuthority.HasCustomAt(system, 1, 1));
            Assert.False(SelectiveCabeceraAuthority.HasCustomAt(system, 1, 0));
            Assert.False(SelectiveCabeceraAuthority.HasCustomAt(system, 0, 1));

            Assert.Equal(depthBefore, raw.Depth);                // preguntar no cambia nada
            Assert.Equal(membersBefore, raw.Members?.Count ?? 0);
        }

        // ======================================================================================
        // Caracterización: EffectiveCustomAt SIGUE siendo in-place (ARQ-43-10 diferido)
        // ======================================================================================

        [Fact]
        public void EffectiveCustomAt_StillImposesTheFondoDepthInPlace_BecauseArq4310IsDeferred()
        {
            // Esto NO es un defecto pendiente: es el contrato vigente. Purificarlo —copiar la receta e imponer el
            // Depth en una sola frontera— es ARQ-43-10, explícitamente fuera de I-43. Esta prueba existe para que
            // 8.6F no lo absorba por accidente: si alguien purifica EffectiveCustomAt, esto avisa.
            var design = Design(2);
            design.ExtraFondoDepths.Add(60.0);
            SetCustom(design, 1, 1, Custom(300.0));
            var system = Resolve(design);

            var stored = SelectiveCabeceraAuthority.CustomAt(system, 1, 1);
            var effective = SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1);

            Assert.Same(stored, effective);                          // la misma instancia, no una copia
            Assert.Equal(54.0, effective.Depth, 4);                  // 60 − 6, el fondo manda
            Assert.Equal(54.0, stored.Depth, 4);                     // y la almacenada quedó normalizada in-place
        }
    }
}
