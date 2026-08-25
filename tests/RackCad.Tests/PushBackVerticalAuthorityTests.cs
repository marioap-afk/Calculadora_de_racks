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
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// DECISIÓN FINAL DEL DUEÑO — UNA sola autoridad vertical, y es el extremo BAJO.
    ///
    /// <para>
    /// Estas pruebas no miran una fórmula: miran las PIEZAS DIBUJADAS. Una pieza física tiene una elevación, y
    /// tiene que ser la misma la mire quien la mire. El defecto que motivó la ronda era exactamente ese: el corte
    /// frontal posterior leía la elevación del resolver mientras el lateral leía la derivada, así que el MISMO
    /// larguero salía en dos troqueles distintos según la vista.
    /// </para>
    /// </summary>
    public class PushBackVerticalAuthorityTests
    {
        private const string InOut = "LARGUERO_IN_OUT_C6";
        private const string Redondo = "LARGUERO_ESCALON_TROQUEL_REDONDO";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackSystem System(RackCatalog catalog, int palletsDeep, double firstLevelHeight = 6.0)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = palletsDeep,
                    LoadLevels = 2,
                    FirstLevelHeight = firstLevelHeight,
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
            new[] { 2, 3, 4, 6, 8, 10 }.Select(deep => new object[] { deep });

        private static List<double> Ys(IEnumerable<HeaderBlockInstance> instances, string pieceId)
            => instances
                .Where(i => string.Equals(i.PieceId, pieceId, StringComparison.Ordinal))
                .Select(i => Math.Round(i.Insertion.Y, 6))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

        /// <summary>
        /// El larguero ALTO sale a la MISMA elevación en el corte lateral y en el frontal posterior. Es el arreglo:
        /// hasta ahora el frontal lo leía del resolver y el lateral de la autoridad derivada.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheHighBeam_HasOneElevation_InEveryView(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var authority = PushBackElevations.HighInsertions(system, catalog, front)
                .Values.Select(y => Math.Round(y, 6)).OrderBy(y => y).ToList();

            var lateral = Ys(
                new PushBackSystemLateralBuilder().Build(system, catalog, 0).Flatten().Instances, Redondo);
            var frontal = Ys(
                new PushBackSystemFrontalBuilder()
                    .BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances,
                Redondo);

            Assert.NotEmpty(authority);
            Assert.Equal(authority, lateral);
            Assert.Equal(authority, frontal);
        }

        /// <summary>Y el BAJO, que es el ancla, también: el lateral y el corte de entrada coinciden.</summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheLowBeam_HasOneElevation_InEveryView(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var authority = PushBackElevations.LowInsertions(system, catalog, front)
                .Values.Select(y => Math.Round(y, 6)).OrderBy(y => y).ToList();

            var lateral = Ys(
                new PushBackSystemLateralBuilder().Build(system, catalog, 0).Flatten().Instances, InOut);
            var frontal = Ys(
                new PushBackSystemFrontalBuilder()
                    .BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances,
                InOut);

            Assert.NotEmpty(authority);
            Assert.Equal(authority, lateral);
            Assert.Equal(authority, frontal);
        }

        /// <summary>
        /// El ancla es de verdad un ancla: el larguero de entrada queda en la elevación que el resolver le dio a su
        /// nivel, sea cual sea el fondo. Ésta es la propiedad que el dueño rechazaba no ver en AutoCAD.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheLowBeam_StaysAtTheRequestedHeight_WhateverTheFondo(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var levels = DynamicFrontGeometry.LoadBeamLevels(system.Structure, front);
            var low = PushBackElevations.LowInsertions(system, catalog, front);

            foreach (var level in levels)
            {
                Assert.Equal(level.ExitElevation, low[level.LevelNumber], 9);
            }
        }

        /// <summary>
        /// Y el ALTO ya no es el ancla: en cuanto el fondo lo pide, se separa de la elevación del resolver. Si
        /// coincidiera siempre, la inversión no estaría hecha y estas pruebas pasarían por casualidad.
        /// </summary>
        [Fact]
        public void TheHighBeam_DivergesFromTheResolverElevation()
        {
            var catalog = Catalog;
            var diverged = 0;

            foreach (var deep in new[] { 2, 3, 4, 6, 8, 10 })
            {
                var system = System(catalog, deep);
                var front = system.Structure.Fronts[0];
                var levels = DynamicFrontGeometry.LoadBeamLevels(system.Structure, front);
                var high = PushBackElevations.HighInsertions(system, catalog, front);

                foreach (var level in levels)
                {
                    if (Math.Abs(high[level.LevelNumber] - level.EntranceElevation) > 1e-6)
                    {
                        diverged++;
                    }
                }
            }

            Assert.True(diverged > 0, "el alto coincidió con el resolver en todos los casos: no se derivó de nada");
        }
    }
}
