using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
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
    /// I-44 — REPRODUCCION del hotfix: los PERALTES de los largueros INTERMEDIOS en el BOM de Push Back.
    ///
    /// <para>
    /// El escenario es el minimo que separa las dos autoridades posibles: tres frentes que comparten NIVEL y piden
    /// peraltes distintos para su larguero intermedio (F1 = 3.5", F2 = 4.5", F3 = 6"). Cada intermedio pertenece a
    /// UNA cama (I-42), asi que el BOM debe conservar por cama/frente su ProfileId, su Length, su PERALTE y su
    /// Quantity. Si el peralte lo decidiera el RACK, el intermedio de F1 se convertiria en 6" solo porque existe
    /// otro frente de 6" — que es exactamente lo que estas pruebas niegan.
    /// </para>
    ///
    /// <para>
    /// Las pruebas marcadas CARACTERIZACION no juzgan: fijan el comportamiento observado hoy para que el fix se
    /// mida contra el, incluido el limite de persistencia (documentos anteriores a I-42).
    /// </para>
    /// </summary>
    public class PushBackIntermediateBeamPeralteTests
    {
        private const double PeralteF1 = 3.5;
        private const double PeralteF2 = 4.5;
        private const double PeralteF3 = 6.0;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>Tres frentes con el MISMO nivel y peraltes de intermedio distintos. Los anchos tambien difieren
        /// (1, 2 y 3 calles) para que la clave del BOM distinga longitud y peralte por separado.</summary>
        private static PushBackDesign ThreeFronts(int levels = 1)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = levels,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };

            var peraltes = new[] { PeralteF1, PeralteF2, PeralteF3 };
            for (var index = 0; index < peraltes.Length; index++)
            {
                var front = new DynamicRackFrontDesign
                {
                    PalletCount = index + 1,
                    LoadLevels = levels,
                    PalletsDeep = 4,
                    DepthStartPosition = 1
                };
                for (var level = 0; level < levels; level++)
                {
                    front.Levels.Add(new DynamicRackLevelDesign
                    {
                        IntermediateBeamCatalogId = DynamicRackDefaults.IntermediateBeamCatalogId,
                        IntermediateBeamDepth = peraltes[index]
                    });
                    front.IntermediateBeamDepths.Add(peraltes[index]);
                }

                design.Structure.Fronts.Add(front);
            }

            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design, RackCatalog catalog)
            => new PushBackResolver(catalog).Resolve(design);

        private static List<BomComponent> Intermediates(PushBackSystem system, RackCatalog catalog)
            => PushBackBomBuilder.Build(system, catalog).Components
                .Where(component => component.Category == SystemBomBuilder.IntermediateBeam)
                .ToList();

        /// <summary>El peralte que el BOM realmente publica: vive en la descripcion, que es donde lo escribe
        /// EmitBeams.</summary>
        private static double PeralteOf(BomComponent component)
        {
            var marker = component.Description.LastIndexOf("Peralte ", StringComparison.Ordinal);
            Assert.True(marker >= 0, "La descripcion del intermedio no publica su peralte: " + component.Description);
            var text = component.Description.Substring(marker + 8).Trim().Trim('"');
            return double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private static string Dump(IEnumerable<BomComponent> components)
            => string.Join(
                " | ",
                components.Select(component => string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} L={1:0.##} P={2:0.##} x{3}",
                    component.ProfileId,
                    component.Length,
                    PeralteOf(component),
                    component.Quantity)));

        private static double[] InstancePeraltes(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
            => new PushBackIntermediateBeamLateralBuilder()
                .BuildFor(system, catalog, front, null)
                .Select(instance => instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.PeralteParam, out var value)
                    ? value
                    : double.NaN)
                .ToArray();

        // ---- CARACTERIZACION: donde vive hoy cada autoridad --------------------------------------------------

        [Fact]
        public void Characterization_EachFrontKeepsItsOwnPeralte_AfterResolve()
        {
            var catalog = Catalog;
            var system = Resolve(ThreeFronts(), catalog);

            // El dato POR FRENTE llega intacto al modelo resuelto: la intencion del usuario no se pierde al resolver.
            Assert.Equal(PeralteF1, DynamicIntermediateBeamGeometry.PeralteAt(system.Structure.Fronts[0], 1), 3);
            Assert.Equal(PeralteF2, DynamicIntermediateBeamGeometry.PeralteAt(system.Structure.Fronts[1], 1), 3);
            Assert.Equal(PeralteF3, DynamicIntermediateBeamGeometry.PeralteAt(system.Structure.Fronts[2], 1), 3);
        }

        [Fact]
        public void Characterization_TheSystemLevelAuthorityIsAMaximumAcrossFronts()
        {
            var catalog = Catalog;
            var system = Resolve(ThreeFronts(), catalog);

            // La sobrecarga por SISTEMA es una envolvente de proyeccion lateral: el mayor peralte del nivel. Es
            // correcta para lo que dibuja un corte lateral y ES la que consume hoy el conteo del BOM.
            Assert.Equal(PeralteF3, DynamicIntermediateBeamGeometry.PeralteAt(system.Structure, 1), 3);
        }

        // ---- REPRODUCCION -----------------------------------------------------------------------------------

        [Fact]
        public void BuildFor_TheShallowFront_DoesNotBorrowThePeralteOfAnotherFront()
        {
            var catalog = Catalog;
            var system = Resolve(ThreeFronts(), catalog);

            var peraltes = InstancePeraltes(system, catalog, system.Structure.Fronts[0]);

            Assert.NotEmpty(peraltes);
            Assert.All(peraltes, peralte => Assert.Equal(PeralteF1, peralte, 3));
        }

        [Fact]
        public void BuildFor_EachFront_ProducesItsOwnPeralte()
        {
            var catalog = Catalog;
            var system = Resolve(ThreeFronts(), catalog);

            Assert.All(InstancePeraltes(system, catalog, system.Structure.Fronts[0]), p => Assert.Equal(PeralteF1, p, 3));
            Assert.All(InstancePeraltes(system, catalog, system.Structure.Fronts[1]), p => Assert.Equal(PeralteF2, p, 3));
            Assert.All(InstancePeraltes(system, catalog, system.Structure.Fronts[2]), p => Assert.Equal(PeralteF3, p, 3));
        }

        [Fact]
        public void Bom_KeepsOnePeraltePerFront_NotTheRackMaximum()
        {
            var catalog = Catalog;
            var system = Resolve(ThreeFronts(), catalog);
            var intermediates = Intermediates(system, catalog);

            var published = intermediates.Select(PeralteOf).Distinct().OrderBy(value => value).ToArray();
            Assert.Equal(new[] { PeralteF1, PeralteF2, PeralteF3 }, published);
        }

        [Fact]
        public void Bom_TheShallowFrontIsNotConvertedTo6_JustBecauseAnotherFrontIs6()
        {
            var catalog = Catalog;
            var system = Resolve(ThreeFronts(), catalog);
            var intermediates = Intermediates(system, catalog);

            // La cantidad que el BOM publica a 6" no puede exceder la del frente que realmente pidio 6".
            var expectedAt6 = InstancePeraltes(system, catalog, system.Structure.Fronts[2]).Length;
            var publishedAt6 = intermediates
                .Where(component => Math.Abs(PeralteOf(component) - PeralteF3) < 1e-6)
                .Sum(component => component.Quantity);

            Assert.Equal(expectedAt6, publishedAt6);
        }

        [Fact]
        public void Bom_KeepsProfileLengthPeralteAndQuantity_PerFront()
        {
            var catalog = Catalog;
            var system = Resolve(ThreeFronts(), catalog);
            var intermediates = Intermediates(system, catalog);
            var dump = Dump(intermediates);

            foreach (var front in system.Structure.Fronts)
            {
                var expectedPeralte = DynamicIntermediateBeamGeometry.PeralteAt(front, 1);
                var expectedQuantity = InstancePeraltes(system, catalog, front).Length;
                var match = intermediates.SingleOrDefault(component =>
                    Math.Abs(component.Length - front.BeamLength) < 1e-3
                    && Math.Abs(PeralteOf(component) - expectedPeralte) < 1e-6);

                Assert.True(
                    match != null,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Frente {0}: falta el intermedio L={1:0.##} P={2:0.##}. BOM: {3}",
                        front.Index,
                        front.BeamLength,
                        expectedPeralte,
                        dump));
                Assert.Equal(DynamicRackDefaults.IntermediateBeamCatalogId, match.ProfileId);
                Assert.Equal(expectedQuantity, match.Quantity);
            }
        }

        // ---- CARACTERIZACION del limite de persistencia (documentos anteriores a I-42) -----------------------

        /// <summary>El contrato persistido ANTERIOR al peralte por celda: cada frente escribe su lista
        /// IntermediateBeamDepths y NINGUN nivel lleva IntermediateBeamDepth.</summary>
        private const string LegacyPerFrontDocument = @"{
            ""SchemaVersion"": ""1.0"",
            ""Structure"": {
                ""PalletFront"": 42.0, ""PalletDepth"": 48.0, ""PalletHeight"": 60.0, ""PalletWeight"": 1000.0,
                ""PalletWeightUnit"": ""kg"", ""PalletsDeep"": 4, ""LoadLevels"": 1,
                ""FirstLevelHeight"": 6.0, ""BeamDepth"": 4.0,
                ""Fronts"": [
                    { ""PalletCount"": 1, ""LoadLevels"": 1, ""PalletsDeep"": 4, ""IntermediateBeamDepths"": [ 3.5 ] },
                    { ""PalletCount"": 2, ""LoadLevels"": 1, ""PalletsDeep"": 4, ""IntermediateBeamDepths"": [ 4.5 ] },
                    { ""PalletCount"": 3, ""LoadLevels"": 1, ""PalletsDeep"": 4, ""IntermediateBeamDepths"": [ 6.0 ] }
                ]
            }
        }";

        /// <summary>El contrato persistido mas antiguo aun: UNA lista de peraltes para todo el rack.</summary>
        private const string LegacyRackWideDocument = @"{
            ""SchemaVersion"": ""1.0"",
            ""Structure"": {
                ""PalletFront"": 42.0, ""PalletDepth"": 48.0, ""PalletHeight"": 60.0, ""PalletWeight"": 1000.0,
                ""PalletWeightUnit"": ""kg"", ""PalletsDeep"": 4, ""LoadLevels"": 1,
                ""FirstLevelHeight"": 6.0, ""BeamDepth"": 4.0,
                ""IntermediateBeamDepths"": [ 4.5 ],
                ""Fronts"": [
                    { ""PalletCount"": 1, ""LoadLevels"": 1, ""PalletsDeep"": 4 },
                    { ""PalletCount"": 2, ""LoadLevels"": 1, ""PalletsDeep"": 4 }
                ]
            }
        }";

        [Fact]
        public void Characterization_LegacyPerFrontDepths_SurviveLoadAndResolve()
        {
            var catalog = Catalog;
            var design = JsonSerializer.Deserialize<PushBackDesignDocument>(LegacyPerFrontDocument).ToDomain();
            var system = Resolve(design, catalog);

            // load -> resolve: la lista por frente sigue siendo la autoridad y NINGUN 4.5 cae a 3.5.
            Assert.Equal(PeralteF1, DynamicIntermediateBeamGeometry.PeralteAt(system.Structure.Fronts[0], 1), 3);
            Assert.Equal(PeralteF2, DynamicIntermediateBeamGeometry.PeralteAt(system.Structure.Fronts[1], 1), 3);
            Assert.Equal(PeralteF3, DynamicIntermediateBeamGeometry.PeralteAt(system.Structure.Fronts[2], 1), 3);
        }

        [Fact]
        public void Characterization_LegacyRackWideDepths_SurviveLoadAndResolve()
        {
            var catalog = Catalog;
            var design = JsonSerializer.Deserialize<PushBackDesignDocument>(LegacyRackWideDocument).ToDomain();
            var system = Resolve(design, catalog);

            // Sin listas por frente, la lista del RACK es el fallback declarado: 4.5 no puede degradarse al 3.5 default.
            Assert.Equal(PeralteF2, DynamicIntermediateBeamGeometry.PeralteAt(system.Structure.Fronts[0], 1), 3);
            Assert.Equal(PeralteF2, DynamicIntermediateBeamGeometry.PeralteAt(system.Structure.Fronts[1], 1), 3);
        }

        [Fact]
        public void LegacyPerFrontDepths_ReachTheBomWithoutCollapsing()
        {
            var catalog = Catalog;
            var design = JsonSerializer.Deserialize<PushBackDesignDocument>(LegacyPerFrontDocument).ToDomain();
            var system = Resolve(design, catalog);

            var published = Intermediates(system, catalog).Select(PeralteOf).Distinct().OrderBy(v => v).ToArray();
            Assert.Equal(new[] { PeralteF1, PeralteF2, PeralteF3 }, published);
        }

        // ---- El caso 4.5 -> 3.5, que un Max() NO puede explicar ----------------------------------------------

        /// <summary>
        /// Un maximo solo puede SUBIR un peralte. Que un 4.5 aparezca como 3.5 exige que la autoridad consultada
        /// NO vea al frente que lo pidio. En un rack de un solo sentido el unico frente invisible para
        /// <c>PeralteAt(system, nivel)</c> es el que esta EN BLANCO — y un frente en blanco no lleva carga, asi que
        /// tampoco debe aportar un solo larguero intermedio al BOM.
        /// </summary>
        [Fact]
        public void ABlankFront_ContributesNoIntermediateBeam_AtAll()
        {
            var catalog = Catalog;
            var design = ThreeFronts();
            design.Structure.Fronts[1].IsActive = false;   // el frente de 4.5" queda en blanco
            var system = Resolve(design, catalog);

            Assert.Empty(InstancePeraltes(system, catalog, system.Structure.Fronts[1]));
        }

        /// <summary>
        /// Un rack COMPUESTO con una cama CORRIDA: la cama atraviesa la interfaz, asi que sus intermedios se cuentan
        /// sobre el sistema SINTETICO de la corrida, cuyos niveles se copian del lado BAJO. Aqui el lado alto pide
        /// 4.5" y el bajo 3.5": si la cuenta se queda con la del lado bajo, un 4.5 authored sale del BOM como 3.5
        /// sin que ningun maximo lo explique.
        /// </summary>
        [Fact]
        public void ACorridaBed_DoesNotDropTheHighSidePeralte()
        {
            var catalog = Catalog;
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 1, slotsB: 1, deepA: 4, deepB: 4, levelsA: 1, levelsB: 1);
            design.Composite.DefaultTopology = PushBackCellTopology.Corrida;

            SetIntermediate(design.Structure.Fronts[0], PeralteF1);   // lado A (bajo) pide 3.5"
            SetIntermediate(design.SideB.Fronts[0], PeralteF2);       // lado B (alto) pide 4.5"

            var system = Resolve(design, catalog);
            var published = Intermediates(system, catalog).Select(PeralteOf).Distinct().OrderBy(v => v).ToArray();

            Assert.Contains(PeralteF2, published);
        }

        private static void SetIntermediate(DynamicRackFrontDesign front, double peralte)
        {
            front.Levels.Clear();
            front.IntermediateBeamDepths.Clear();
            for (var level = 0; level < Math.Max(1, front.LoadLevels ?? 1); level++)
            {
                front.Levels.Add(new DynamicRackLevelDesign
                {
                    IntermediateBeamCatalogId = DynamicRackDefaults.IntermediateBeamCatalogId,
                    IntermediateBeamDepth = peralte
                });
                front.IntermediateBeamDepths.Add(peralte);
            }
        }
    }
}
