using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Owner-validation round 2, defecto 1 (I-32) — la cama se coloca por su ORIGEN.
    ///
    /// El bloque atravesaba el larguero por el lado del ORIGEN: se colocaba haciendo coincidir
    /// <c>TROQUEL_IN</c> con el contacto, y como ese punto esta desplazado respecto del origen local del
    /// bloque, todo lo que hay entre el origen y <c>TROQUEL_IN</c> quedaba ANTES del contacto, dentro del
    /// larguero.
    ///
    /// La regla vigente es geometrica y no necesita ninguna medicion nueva:
    /// <list type="number">
    /// <item>el origen local <c>(0,0)</c> del bloque coincide con el contacto fisico del larguero de origen;</item>
    /// <item>la cama se rota en direccion al contacto del otro larguero;</item>
    /// <item>la <b>LONGITUD geometrica</b> es la distancia euclidiana entre los dos contactos
    /// (<see cref="PushBackFlowBedAxis.Length"/>).</item>
    /// </list>
    ///
    /// <c>TROQUEL_IN</c> sigue existiendo como punto interno del bloque, pero deja de ser la autoridad de
    /// tangencia. Y la longitud <b>COMERCIAL</b> se queda donde estaba: solo calcula la subida nominal de
    /// 7/16" por pie. Los dos largueros siguen en sus troqueles y la pendiente sigue siendo la resultante.
    /// </summary>
    public class PushBackBedOriginTangencyTests
    {
        private const double Tol = 1e-9;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackSystem System(RackCatalog catalog, int palletsDeep)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = palletsDeep,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = palletsDeep, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig());
            return new PushBackResolver(catalog).Resolve(design);
        }

        public static IEnumerable<object[]> Fondos() =>
            new[] { 2, 3, 4, 6, 8 }.Select(deep => new object[] { deep });

        /// <summary>Las piezas de la cama del plan REAL, ya aplanadas a coordenadas de mundo.</summary>
        private static List<HeaderBlockInstance> BedPieces(PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
        {
            var plan = front == null
                ? new PushBackSystemLateralBuilder().Build(system, catalog)
                : new PushBackSystemLateralBuilder().Build(system, catalog, front.Index);
            return plan.Flatten().Instances.Where(PushBackPlanComposer.IsBedPiece).ToList();
        }

        private static List<HeaderBlockInstance> Rails(PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
            => BedPieces(system, catalog, front)
                .Where(i => string.Equals(i.PieceId, FlowBedDefaults.RailId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Insertion.Y)
                .ToList();

        private static double Distance(Point2D a, Point2D b)
            => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        // ---------------------------------------------------------------------------------------------------
        // 1. El ORIGEN de la cama es el contacto del larguero
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheBedOrigin_LandsExactlyOnTheLowBeamContact(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front)
                .OrderBy(a => a.ExitMate.Y).ToList();
            var rails = Rails(system, catalog, front);
            Assert.NotEmpty(rails);
            Assert.Equal(axes.Count, rails.Count);

            for (var i = 0; i < rails.Count; i++)
            {
                Assert.Equal(axes[i].ExitMate.X, rails[i].Insertion.X, 9);
                Assert.Equal(axes[i].ExitMate.Y, rails[i].Insertion.Y, 9);
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 2. LONGITUD geometrica == axis.Length, y el otro extremo cae en el otro contacto
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheBedLength_IsTheDistanceBetweenTheTwoContacts(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front)
                .OrderBy(a => a.ExitMate.Y).ToList();
            var rails = Rails(system, catalog, front);

            for (var i = 0; i < rails.Count; i++)
            {
                Assert.True(rails[i].DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var longitud),
                    "el riel no lleva LONGITUD");
                Assert.Equal(axes[i].Length, longitud, 9);

                // Y NO es la longitud comercial: son magnitudes distintas y no deben confundirse.
                Assert.NotEqual(PushBackFlowBedGeometry.ResolveBedLength(system, front), longitud, 3);
            }
        }

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheFarEndOfTheBed_LandsExactlyOnTheRearBeamContact(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front)
                .OrderBy(a => a.ExitMate.Y).ToList();
            var rails = Rails(system, catalog, front);

            for (var i = 0; i < rails.Count; i++)
            {
                var longitud = rails[i].DynamicParameters[SelectiveRackDefaults.LengthParam];
                var end = new Point2D(
                    rails[i].Insertion.X + longitud * Math.Cos(rails[i].RotationRadians),
                    rails[i].Insertion.Y + longitud * Math.Sin(rails[i].RotationRadians));

                Assert.Equal(axes[i].HighMate.X, end.X, 9);
                Assert.Equal(axes[i].HighMate.Y, end.Y, 9);
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 3. Ni geometria residual antes del origen ni sobrepaso por longitud
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Toda pieza de la cama, proyectada sobre el eje desde el contacto bajo, cae en <c>[0, Length]</c>.
        /// Una coordenada NEGATIVA es exactamente lo que el Owner vio: bloque metido dentro del larguero.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void NoBedPiece_SitsBeforeTheOrigin_NorBeyondTheFarContact(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front).ToList();
            var pieces = BedPieces(system, catalog, front);
            Assert.NotEmpty(pieces);

            foreach (var piece in pieces)
            {
                // Cada pieza pertenece al eje de su propio nivel: el mas cercano en Y de origen.
                var axis = axes.OrderBy(a => Math.Abs(a.ExitMate.Y - piece.Insertion.Y)).First();
                var cos = Math.Cos(axis.AngleRadians);
                var sin = Math.Sin(axis.AngleRadians);
                var dx = piece.Insertion.X - axis.ExitMate.X;
                var dy = piece.Insertion.Y - axis.ExitMate.Y;
                var along = dx * cos + dy * sin;

                Assert.True(along >= -Tol,
                    $"{piece.PieceId} queda {(-along):F4}\" ANTES del contacto: el bloque penetra el larguero");
                Assert.True(along <= axis.Length + Tol,
                    $"{piece.PieceId} sobrepasa el contacto posterior en {(along - axis.Length):F4}\"");
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 4. El lateral COMPLETO (front: null) cumple lo mismo
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheWholeLateralBed_IsTangentAtBothEndsToo(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, null)
                .OrderBy(a => a.ExitMate.Y).ToList();
            var rails = Rails(system, catalog, null);
            Assert.NotEmpty(rails);
            Assert.Equal(axes.Count, rails.Count);

            for (var i = 0; i < rails.Count; i++)
            {
                var longitud = rails[i].DynamicParameters[SelectiveRackDefaults.LengthParam];
                Assert.Equal(axes[i].Length, longitud, 9);
                Assert.True(Distance(rails[i].Insertion, axes[i].ExitMate) < 1e-9, "el origen no cae en el contacto bajo");

                var end = new Point2D(
                    rails[i].Insertion.X + longitud * Math.Cos(rails[i].RotationRadians),
                    rails[i].Insertion.Y + longitud * Math.Sin(rails[i].RotationRadians));
                Assert.True(Distance(end, axes[i].HighMate) < 1e-9, "el extremo no cae en el contacto posterior");
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 5. Lo que NO debe moverse
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Los dos largueros siguen en troquel y la pendiente sigue siendo la resultante del ajuste. La
        /// colocacion de la cama es un problema de DIBUJO: no puede tocar la geometria de los largueros.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void BothBeamsStayOnTheirTroqueles_AndTheResultingSlopeIsUntouched(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var grid = PushBackTroquelGrid.Base(system.Structure, catalog);

            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                Assert.Equal(PushBackTroquelGrid.Snap(cell.LowInsertion, grid), cell.LowInsertion, 9);
                Assert.Equal(PushBackTroquelGrid.Snap(cell.RearInsertion, grid), cell.RearInsertion, 9);
            }

            // La subida dibujada es la que sale de los troqueles, no el objetivo nominal.
            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, front))
            {
                var cell = PushBackElevations.Resolve(system, catalog, front)[axis.LevelNumber];
                Assert.Equal(cell.ResultingRise, axis.Rise, 9);
            }
        }

        /// <summary>La longitud COMERCIAL se queda donde estaba: solo calcula la subida nominal.</summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheCommercialLength_StillDrivesOnlyTheNominalRise(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var commercial = PushBackFlowBedGeometry.ResolveBedLength(system, front);
            Assert.Equal(front.EndX - front.StartX, commercial, 9);

            var nominalRise = PushBackBedSlope.Rise(commercial);
            var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
            var grid = PushBackTroquelGrid.Base(system.Structure, catalog);

            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                Assert.Equal(
                    PushBackTroquelGrid.Snap(cell.RearContact.Y - nominalRise - lowMate.Value.Y, grid),
                    cell.LowInsertion,
                    9);
            }
        }
    }
}
