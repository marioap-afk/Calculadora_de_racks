using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
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
    /// Owner decisions (2026-07-24) for the rear tope, which SUPERSEDE the earlier rules:
    /// <list type="number">
    /// <item>Its ORIGIN sits on the vertical axis of the POST's <c>TROQUEL_SEPARADOR</c> — never the rear beam's contact
    /// points, never a raw <c>placement.X</c> fallback. PLANTA uses the separator point measured in the PLANTA view.</item>
    /// <item>Its ORIENTATION is INVERTED with respect to 10d8eeb, where the Owner measured it upside down.</item>
    /// </list>
    /// The approved elevation (+4"), snap, SAQUE, LONGITUD, OffCells and BOM are pinned unchanged.
    /// </summary>
    public class PushBackRearTopeAnchorTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackDesign BaseStructure() => new DynamicRackDesign
        {
            Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
            PalletsDeep = 4,
            LoadLevels = 3,
            FirstLevelHeight = 6.0,
            BeamDepth = 4.0
        };

        private static PushBackSystem System(RackCatalog catalog)
            => new PushBackResolver(catalog).Resolve(new PushBackDesign { Structure = BaseStructure() });

        private static (string PostId, double Peralte) Post(PushBackSystem system, RackCatalog catalog)
        {
            var postId = DynamicFrontGeometry.PostId(system.Structure, catalog);
            return (postId, DynamicFrontGeometry.PostPeralte(system.Structure, catalog, postId));
        }

        // ---- (1) the anchor is the POST's TROQUEL_SEPARADOR axis ----

        [Fact]
        public void SeparatorAnchor_IsTheCatalogsMeasuredPostPoint_PerView()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var (postId, peralte) = Post(system, catalog);

            foreach (var view in new[] { "LATERAL", "FRONTAL", "PLANTA" })
            {
                var entry = catalog.ConnectionLayout.FindConnectionLayout(
                    postId, PushBackRearTopeBuilder.AnchorPoint(view), view);
                Assert.NotNull(entry);   // the shipped catalog measures the required point in each view

                var resolved = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, view);
                Assert.True(resolved.HasValue);
                var expected = SelectivePostGeometry.Resolve(
                    entry, new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = peralte });
                Assert.Equal(expected.X, resolved.Value.X, 9);
                Assert.Equal(expected.Y, resolved.Value.Y, 9);
            }

            // Owner decision (2026-07-24, final): each view has its OWN point — LATERAL the separator, FRONTAL and
            // PLANTA the post's own TROQUEL_TOPE — and none of them is TROQUEL_LARGUERO.
            Assert.Equal(DynamicRackDefaults.SeparatorPostPoint, PushBackRearTopeBuilder.AnchorPoint("LATERAL"));
            Assert.Equal(PushBackRearTopeBuilder.TopePostPoint, PushBackRearTopeBuilder.AnchorPoint("FRONTAL"));
            Assert.Equal(PushBackRearTopeBuilder.TopePostPoint, PushBackRearTopeBuilder.AnchorPoint("PLANTA"));
            foreach (var view in new[] { "LATERAL", "FRONTAL", "PLANTA" })
            {
                Assert.NotEqual(SelectiveRackDefaults.PostBeamPoint, PushBackRearTopeBuilder.AnchorPoint(view));
            }

            // PLANTA carries its own depth offset, driven by the post peralte — it is NOT the lateral point.
            var lateral = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, "LATERAL").Value;
            var planta = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, "PLANTA").Value;
            Assert.NotEqual(lateral.Y, planta.Y);
        }

        [Fact]
        public void NoAnchor_AndNoRawPlacementFallback_WhenThePostHasNoSeparatorPoint()
        {
            var catalog = Catalog;
            Assert.Null(PushBackRearTopeBuilder.PostAnchorLocal(catalog, "POSTE_SIN_SEPARADOR", 3.0, "LATERAL"));
            Assert.Null(PushBackRearTopeBuilder.PostAnchorLocal(null, "CUALQUIERA", 3.0, "LATERAL"));
        }

        [Fact]
        public void TopeOrigin_SitsOnTheSeparatorAxis_InLateralAndInPlanta()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var (postId, peralte) = Post(system, catalog);
            var builder = new PushBackRearTopeBuilder();
            var rearBeams = DynamicLoadBeamGeometry.Placements(system.Structure, front)
                .Where(p => p.IsEntrance).ToList();
            Assert.NotEmpty(rearBeams);

            foreach (var view in new[] { "LATERAL", "PLANTA" })
            {
                var separator = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, view).Value;
                var topes = builder.Build(system, catalog, 0, front, view);
                Assert.NotEmpty(topes);

                foreach (var tope in topes)
                {
                    var expectedXs = rearBeams
                        .Select(b => b.X + (b.MirroredX ? -separator.X : separator.X))
                        .ToList();
                    Assert.Contains(expectedXs, x => Math.Abs(x - tope.Insertion.X) < 1e-9);

                    // It is NOT the bare placement X: the separator axis is offset from the post's insertion.
                    Assert.DoesNotContain(rearBeams, b => Math.Abs(b.X - tope.Insertion.X) < 1e-9);
                }

                // PLANTA also takes the separator's own depth offset.
                if (string.Equals(view, "PLANTA", StringComparison.Ordinal))
                {
                    Assert.All(topes, tope => Assert.Contains(
                        rearBeams, b => Math.Abs(b.Y + separator.Y - tope.Insertion.Y) < 1e-9));
                }
            }
        }

        [Fact]
        public void TopeAnchor_DoesNotUseTheRearBeamsContactPoints()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var beamId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.NotEmpty(topes);

            var beamEdges = DynamicLoadBeamGeometry.Placements(system.Structure, front)
                .Where(p => p.IsEntrance)
                .SelectMany(p => new[]
                {
                    PushBackDefaults.HighEndBeamLeftBedMatePoint,
                    PushBackDefaults.HighEndBeamRightBedMatePoint
                }.Select(pointId =>
                {
                    var e = catalog.ConnectionLayout.FindConnectionLayout(
                        beamId, pointId, PushBackDefaults.HighEndBeamView);
                    return e == null ? double.NaN : p.X + (p.MirroredX ? -e.LocalX : e.LocalX);
                }))
                .Where(x => !double.IsNaN(x))
                .ToList();

            Assert.All(topes, tope => Assert.DoesNotContain(beamEdges, x => Math.Abs(x - tope.Insertion.X) < 1e-9));
        }

        // ---- (2) the orientation is the inverse of 10d8eeb ----

        [Fact]
        public void ElevationOrientation_IsInvertedWithRespectTo10d8eeb()
        {
            // 10d8eeb dibujaba las elevaciones ESPEJADAS y el dueño midio el tope al reves; desde entonces iba sin
            // espejo. I-42 (correccion aislada 5B) SUSTITUYE esa parte del contrato: el tope va con la mano
            // CONTRARIA a la de su larguero alto, en TODAS las vistas. En el marco identidad —donde el alto iba
            // siempre espejado— las dos formulas dan el mismo valor, que es por lo que 10d8eeb quedaba bien; lo que
            // cambia es que ahora el tope SIGUE a su larguero en vez de ser una constante.
            //
            // El valor de 10d8eeb se conserva donde el alto va espejado, que es lo que aquel escenario media.
            Assert.False(PushBackRearTopeBuilder.Mirrored("LATERAL", beamMirroredX: true));
            Assert.True(PushBackRearTopeBuilder.Mirrored("LATERAL", beamMirroredX: false));

            // La PLANTA usa exactamente la misma relacion: son vistas de PROFUNDIDAD, donde esa mano se ve.
            Assert.False(PushBackRearTopeBuilder.Mirrored("PLANTA", beamMirroredX: true));
            Assert.True(PushBackRearTopeBuilder.Mirrored("PLANTA", beamMirroredX: false));

            // El corte FRONTAL no: ahi la X corre con la reticula TRANSVERSAL y el espejo de una pieza no habla del
            // escalon del larguero, que se ve de canto. Su orientacion es la que el dueño valido y esta correccion
            // no la toca — sigue siendo constante.
            Assert.False(PushBackRearTopeBuilder.ElevationMirrored);
            Assert.False(PushBackRearTopeBuilder.Mirrored("FRONTAL", beamMirroredX: true));
            Assert.False(PushBackRearTopeBuilder.Mirrored("FRONTAL", beamMirroredX: false));
        }

        [Fact]
        public void LateralAndRearFrontalTopes_AreDrawnUnmirrored()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            // I-42 (correccion aislada 5B): el tope va con la mano CONTRARIA a la de su larguero alto. En este
            // escenario ese larguero acaba en una CABECERA, asi que va espejado y el tope sin espejo — el mismo
            // resultado que esta prueba fijaba, ahora derivado de su larguero y no de una constante.
            var beams = new PushBackSystemLateralBuilder().Build(system, catalog, 0).Flatten().Instances
                .Where(i => i.Role == HeaderBlockRole.Beam
                    && string.Equals(i.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.NotEmpty(beams);
            Assert.All(beams, beam => Assert.True(beam.MirroredX));

            var lateral = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.NotEmpty(lateral);
            Assert.All(lateral, tope => Assert.False(tope.MirroredX));

            var frontal = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances
                .Where(i => i.Role == HeaderBlockRole.Tope).ToList();
            Assert.NotEmpty(frontal);
            Assert.All(frontal, tope => Assert.False(tope.MirroredX));
        }

        // ---- the approved contracts stay frozen ----

        [Fact]
        public void ElevationSaqueLongitudSnapAndOffCells_AreUntouched()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            var system = new PushBackResolver(catalog).Resolve(design);
            var front = system.Structure.Fronts[0];

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.Equal(Math.Max(1, front.LoadLevels) - 1, topes.Count);   // OffCells still removes its cell

            // PB-VAL-03 sigue aprobada: la elevacion es el rise-and-snap canonico mas exactamente 4", que son DOS
            // troqueles. Lo que cambia (I-42, ronda post-82e918b) es la REFERENCIA: el tope cuelga de su larguero
            // alto, y desde la inversion vertical ese larguero esta en la elevacion DERIVADA, no en la que el
            // resolver compartido le dio al nivel. Medir desde la del resolver lo dejaba flotando sobre un larguero
            // que ya no esta ahi, y ademas discrepando del corte frontal.
            Assert.Equal(2.0 * SelectiveRackDefaults.TroquelPaso, PushBackRearTopeBuilder.ExtraRise, 9);
            var rearBeams = PushBackElevations.HighInsertions(system, catalog, front).Values.ToList();
            var gridBase = PostGridBase(system, catalog);
            Assert.All(topes, tope => Assert.Contains(
                rearBeams, y => Math.Abs(PushBackRearTopeBuilder.ElevationY(gridBase, y) - tope.Insertion.Y) < 1e-9));

            var expectedLongitud = PushBackLoadBeamGeometry.CellBeamLength(system.Structure, front, 1)
                + SelectiveTopePlacement.LengthAllowance;
            Assert.All(topes, tope =>
            {
                Assert.Equal(PushBackDefaults.RearTopeSaque, tope.DynamicParameters[SelectiveSafetyPlacement.SaqueParam], 9);
                Assert.Equal(expectedLongitud, tope.DynamicParameters[SelectiveRackDefaults.LengthParam], 6);
                Assert.Equal(HeaderBlockRole.Tope, tope.Role);
                Assert.Equal(PushBackRearTopeBuilder.TopePieceId, tope.PieceId);
            });
        }

        private static double PostGridBase(PushBackSystem system, RackCatalog catalog)
        {
            var postId = DynamicFrontGeometry.PostId(system.Structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(system.Structure, catalog, postId);
            return PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, postPeralte, "LATERAL")?.Y ?? 0.0;
        }
    }
}
