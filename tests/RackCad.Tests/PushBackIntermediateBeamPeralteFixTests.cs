using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-44 · Gate 3 — EL CONTRATO, cerrado por pruebas. Dos autoridades, nombradas por el llamador:
    ///
    /// <list type="bullet">
    /// <item><c>Build</c> = PROYECCION. Un corte lateral muestra el larguero que se VE, que es el de mayor peralte
    /// entre los frentes que la linea proyecta. Conserva su semantica exacta.</item>
    /// <item><c>BuildFor</c> = PIEZA FISICA de una cama. Su perfil y su peralte son los de ESA celda (frente x
    /// nivel) y de ninguna otra, tambien cuando el llamador pasa un <c>postIndex</c> real desde el dibujo de un
    /// rack compuesto.</item>
    /// </list>
    ///
    /// <para>
    /// La prueba clave es la del ID: dos frentes con el MISMO peralte y perfiles DISTINTOS. Es la unica que
    /// demuestra que el id y el peralte salen de la misma celda y no de dos consultas independientes que coincidian
    /// por usar las dos un maximo.
    /// </para>
    /// </summary>
    public class PushBackIntermediateBeamPeralteFixTests
    {
        private const string Infinito = DynamicRackDefaults.IntermediateBeamCatalogId;   // LARGUERO_ESCALON_INFINITO
        private const string Redondo = "LARGUERO_ESCALON_TROQUEL_REDONDO";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>Una celda authored: su perfil y su peralte.</summary>
        private readonly struct Cell
        {
            public Cell(double peralte, string beamId = Infinito)
            {
                Peralte = peralte;
                BeamId = beamId;
            }

            public double Peralte { get; }
            public string BeamId { get; }
        }

        private static PushBackDesign Design(params Cell[][] fronts)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = fronts[0].Length,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };

            foreach (var cells in fronts)
            {
                var front = new DynamicRackFrontDesign
                {
                    PalletCount = 1,
                    LoadLevels = cells.Length,
                    PalletsDeep = 4,
                    DepthStartPosition = 1
                };
                foreach (var cell in cells)
                {
                    front.Levels.Add(new DynamicRackLevelDesign
                    {
                        IntermediateBeamCatalogId = cell.BeamId,
                        IntermediateBeamDepth = cell.Peralte
                    });
                    front.IntermediateBeamDepths.Add(cell.Peralte);
                }

                design.Structure.Fronts.Add(front);
            }

            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design, RackCatalog catalog)
            => new PushBackResolver(catalog).Resolve(design);

        private static IReadOnlyList<HeaderBlockInstance> Bed(
            PushBackSystem system, RackCatalog catalog, int frontIndex, int level, int postIndex = -1)
            => new PushBackIntermediateBeamLateralBuilder().BuildFor(
                system, catalog, system.Structure.Fronts[frontIndex], new[] { level }, postIndex);

        private static List<HeaderBlockInstance> Projected(
            PushBackSystem system, RackCatalog catalog, int postIndex = -1)
            => new PushBackIntermediateBeamLateralBuilder()
                .Build(system, catalog, postIndex)
                .Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Beam)
                .ToList();

        private static double PeralteOf(HeaderBlockInstance instance)
            => instance.DynamicParameters[SelectiveRackDefaults.PeralteParam];

        private static List<BomComponent> Intermediates(PushBackSystem system, RackCatalog catalog)
            => PushBackBomBuilder.Build(system, catalog).Components
                .Where(component => component.Category == SystemBomBuilder.IntermediateBeam)
                .ToList();

        private static double PeralteOf(BomComponent component)
        {
            var marker = component.Description.LastIndexOf("Peralte ", StringComparison.Ordinal);
            return double.Parse(
                component.Description.Substring(marker + 8).Trim().Trim('"'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture);
        }

        // ---- 1. Matriz multinivel ---------------------------------------------------------------------------

        /// <summary>
        /// Dos frentes con las MISMAS tres alturas en distinto orden: F1 = 3.5 / 4.5 / 6 y F2 = 6 / 3.5 / 4.5. Cada
        /// celda lleva lo suyo, y el BOM cotiza las tres alturas con la cantidad exacta de las seis celdas.
        /// </summary>
        [Fact]
        public void AMultiLevelMatrix_GivesEveryCellItsOwnPeralte()
        {
            var catalog = Catalog;
            var authored = new[]
            {
                new[] { new Cell(3.5), new Cell(4.5), new Cell(6.0) },
                new[] { new Cell(6.0), new Cell(3.5), new Cell(4.5) }
            };
            var system = Resolve(Design(authored), catalog);

            var expected = new Dictionary<double, int>();
            for (var front = 0; front < authored.Length; front++)
            {
                for (var level = 1; level <= authored[front].Length; level++)
                {
                    var peralte = authored[front][level - 1].Peralte;
                    var instances = Bed(system, catalog, front, level);
                    Assert.NotEmpty(instances);
                    Assert.All(instances, instance => Assert.Equal(peralte, PeralteOf(instance), 3));
                    expected[peralte] = expected.TryGetValue(peralte, out var current)
                        ? current + instances.Count
                        : instances.Count;
                }
            }

            var published = Intermediates(system, catalog)
                .GroupBy(PeralteOf)
                .ToDictionary(group => group.Key, group => group.Sum(component => component.Quantity));

            Assert.Equal(
                expected.OrderBy(pair => pair.Key).ToList(),
                published.OrderBy(pair => pair.Key).ToList());
        }

        // ---- 2. Mismo peralte, PERFIL distinto --------------------------------------------------------------

        /// <summary>
        /// La prueba que separa las dos consultas de una sola. Los dos frentes piden 4.5" y perfiles DISTINTOS; si
        /// el id y el peralte vinieran de consultas independientes, el empate de peralte dejaria el id en manos del
        /// orden y los dos frentes cotizarian el mismo perfil.
        /// </summary>
        [Fact]
        public void SamePeralteDifferentProfile_KeepsEachCellsOwnProfileId()
        {
            var catalog = Catalog;
            var system = Resolve(
                Design(new[] { new Cell(4.5, Infinito) }, new[] { new Cell(4.5, Redondo) }),
                catalog);

            Assert.All(Bed(system, catalog, 0, 1), instance => Assert.Equal(Infinito, instance.PieceId));
            Assert.All(Bed(system, catalog, 1, 1), instance => Assert.Equal(Redondo, instance.PieceId));

            // Y el BOM publica DOS lineas que solo se distinguen por el perfil: mismo peralte y misma longitud.
            var intermediates = Intermediates(system, catalog);
            Assert.All(intermediates, component => Assert.Equal(4.5, PeralteOf(component), 3));
            Assert.Equal(
                new[] { Infinito, Redondo }.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
                intermediates.Select(component => component.ProfileId).OrderBy(id => id, StringComparer.Ordinal).ToArray());
            Assert.Equal(
                Bed(system, catalog, 0, 1).Count,
                intermediates.Single(component => component.ProfileId == Infinito).Quantity);
            Assert.Equal(
                Bed(system, catalog, 1, 1).Count,
                intermediates.Single(component => component.ProfileId == Redondo).Quantity);
        }

        // ---- 3. BuildFor con postIndex REAL -----------------------------------------------------------------

        /// <summary>
        /// El dibujo de un rack compuesto llama a <c>BuildFor</c> con un poste real
        /// (<c>PushBackCompositeContent.Lateral</c>). Ese poste sigue colocando, pero NO decide: la cama del frente
        /// de 3.5" conserva su 3.5" aunque el poste que comparte con el vecino proyecte 6".
        /// </summary>
        [Fact]
        public void BuildForWithARealPostIndex_StillUsesTheCellsOwnValues()
        {
            var catalog = Catalog;
            var system = Resolve(
                Design(new[] { new Cell(3.5) }, new[] { new Cell(6.0) }, new[] { new Cell(3.5) }),
                catalog);

            // El poste 1 es el compartido por los frentes 0 y 1: su envolvente por poste es 6".
            Assert.Equal(6.0, DynamicIntermediateBeamGeometry.PeralteAtPost(system.Structure, 1, 1), 3);

            var withPost = Bed(system, catalog, 0, 1, postIndex: 1);
            var withoutPost = Bed(system, catalog, 0, 1);

            Assert.NotEmpty(withPost);
            Assert.All(withPost, instance => Assert.Equal(3.5, PeralteOf(instance), 3));
            Assert.All(withPost, instance => Assert.Equal(Infinito, instance.PieceId));

            // Y el poste no cambia el resultado de la cama: es colocacion, no autoridad.
            Assert.Equal(
                withoutPost.Select(PeralteOf).ToArray(),
                withPost.Select(PeralteOf).ToArray());
        }

        // ---- 4. Build conserva la envolvente ----------------------------------------------------------------

        /// <summary>
        /// La proyeccion NO cambia: con poste sigue siendo la envolvente de los frentes adyacentes, y sin poste la
        /// del rack. Un corte lateral dibuja el larguero que se ve, que es el que tapa a los de detras.
        /// </summary>
        [Fact]
        public void Build_KeepsTheProjectionEnvelope_WithAndWithoutAPost()
        {
            var catalog = Catalog;
            var system = Resolve(
                Design(new[] { new Cell(3.5) }, new[] { new Cell(6.0) }, new[] { new Cell(3.5) }),
                catalog);

            var atPost = Projected(system, catalog, postIndex: 1);
            Assert.NotEmpty(atPost);
            Assert.All(atPost, instance => Assert.Equal(
                DynamicIntermediateBeamGeometry.PeralteAtPost(system.Structure, 1, 1),
                PeralteOf(instance),
                3));
            Assert.All(atPost, instance => Assert.Equal(6.0, PeralteOf(instance), 3));

            var rackWide = Projected(system, catalog);
            Assert.NotEmpty(rackWide);
            Assert.All(rackWide, instance => Assert.Equal(
                DynamicIntermediateBeamGeometry.PeralteAt(system.Structure, 1),
                PeralteOf(instance),
                3));
            Assert.All(rackWide, instance => Assert.Equal(6.0, PeralteOf(instance), 3));
        }
    }
}
