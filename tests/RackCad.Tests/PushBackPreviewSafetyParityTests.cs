using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A3-PREVIEW, contrato del dueño) — LO QUE LA VISTA PREVIA ENSEÑA ES LO QUE SE VA A INSERTAR.
    ///
    /// <para>
    /// El dibujo final arma cada corte con <see cref="PushBackSystemFrontalBuilder.BuildPlan(PushBackSystem,
    /// RackCatalog, PushBackFrontalEnd, PushBackSide)"/>, que retira la seguridad local del marco copiado y proyecta
    /// la FISICA del lado y el extremo pedidos —<see cref="PushBackDefensePlan"/> y <see cref="PushBackBootPlan"/>,
    /// resueltas una sola vez sobre el rack entero—. El editor construia sus dos cortes compuestos llamando a
    /// <c>PushBackCompositeFrontal.Build</c> directamente, es decir saltandose ese envoltorio, y por eso enseñaba la
    /// seguridad del marco local en vez de la del lado.
    /// </para>
    ///
    /// <para>
    /// <b>Medido antes del cambio.</b> Con la defensa de A declarada y la de B en «Ninguno», el corte de entrada de
    /// B mostraba las TRES defensas de A (mismas coordenadas) y el dibujo final ninguna; al reves —A en «Ninguno» y
    /// B con pieza— fallaba el corte de A, asi que no era una asimetria de un lado. Con las botas de A en
    /// «Entrada/Salida» y las de B en «Posterior», la vista previa de B las ponia en su entrada (3) y su posterior
    /// vacio, y el dibujo exactamente al reves.
    /// </para>
    ///
    /// <para>
    /// La correccion no reimplementa nada: la vista previa pasa por el MISMO constructor final. Estas pruebas
    /// comparan las dos rutas —preview por <see cref="PushBackEditorDesignAssembler.BuildFrom"/>, dibujo por el
    /// constructor de cortes— pieza a pieza, no solo por cuenta.
    /// </para>
    /// </summary>
    public class PushBackPreviewSafetyParityTests
    {
        private const string RealDefense = "DEFENSA_MONTACARGAS";
        private const string SecondDefense = "DEFENSA_MONTACARGAS_B";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string BootId => Catalog.SafetyElements
            .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)).Id;

        /// <summary>Una COPIA del catalogo con una segunda pieza de defensa: el de fabrica solo trae una.</summary>
        private static RackCatalog TwoPieceCatalog()
        {
            var loaded = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var source = loaded.SafetyElements.First(entry =>
                string.Equals(entry.Id, RealDefense, StringComparison.OrdinalIgnoreCase));
            return new RackCatalog
            {
                PostProfiles = loaded.PostProfiles,
                TrussProfiles = loaded.TrussProfiles,
                BasePlates = loaded.BasePlates,
                FlowBedProfiles = loaded.FlowBedProfiles,
                BeamProfiles = loaded.BeamProfiles,
                Mensulas = loaded.Mensulas,
                SpacerProfiles = loaded.SpacerProfiles,
                ConnectionPoints = loaded.ConnectionPoints,
                Views = loaded.Views,
                Defaults = loaded.Defaults,
                SafetyElements = loaded.SafetyElements.Concat(new[]
                {
                    new SafetyElementCatalogEntry
                    {
                        Id = SecondDefense,
                        DisplayName = source.DisplayName,
                        Description = source.Description,
                        Type = source.Type,
                        Units = source.Units,
                        WeightEach = source.WeightEach,
                    },
                }).ToList(),
                Blocks = loaded.Blocks.Concat(loaded.Blocks
                    .Where(block => string.Equals(block.PieceId, RealDefense, StringComparison.OrdinalIgnoreCase))
                    .Select(block => new BlockCatalogEntry
                    {
                        PieceId = SecondDefense,
                        View = block.View,
                        BlockName = block.BlockName,
                        Layer = block.Layer,
                        Color = block.Color,
                        Scale = block.Scale,
                        Rotation = block.Rotation,
                    })
                    .ToList()).ToList(),
                ConnectionLayout = loaded.ConnectionLayout.Concat(loaded.ConnectionLayout
                    .Where(entry => string.Equals(entry.PieceId, RealDefense, StringComparison.OrdinalIgnoreCase))
                    .Select(entry => new ConnectionLayoutEntry
                    {
                        PieceId = SecondDefense,
                        ConnectionPointId = entry.ConnectionPointId,
                        View = entry.View,
                        LocalX = entry.LocalX,
                        LocalY = entry.LocalY,
                    })
                    .ToList()).ToList(),
            };
        }

        private static PushBackDesign Design(
            string defenseA = PushBackDefaults.NonePieceId,
            string defenseB = PushBackDefaults.NonePieceId,
            double gap = 0.0,
            RackCatalog catalog = null)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 4, deepB: 4, levelsA: 2, levelsB: 2, gap: gap);
            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;
            foreach (var selection in new PushBackSafetyAuthority(catalog ?? Catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            design.DefensePieceId = defenseA;
            design.SideB.DefensePieceId = defenseB;
            return design;
        }

        /// <summary>La intencion de botas POR LADO, tal y como la guarda el editor: general y, si se pide, por poste.</summary>
        private static PushBackDesign WithBoots(
            PushBackDesign design,
            BootPlacement? sideA,
            BootPlacement? sideB,
            IEnumerable<(int Post, BootPlacement Placement)> postsB = null)
        {
            // Se sustituye SOLO la seleccion de botas: el resto de la seguridad del rack sigue declarada.
            var previous = design.Structure.SafetySelections
                .Where(entry => string.Equals(entry.ElementId, BootId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var entry in previous)
            {
                design.Structure.SafetySelections.Remove(entry);
            }

            var selection = new SelectiveSafetySelection
            {
                ElementId = BootId,
                Quantity = 1,
                BootSidesDeclared = true,
            };
            selection.Bota.Placement = sideA;
            selection.BotaB.Placement = sideB;
            foreach (var (post, placement) in postsB ?? Enumerable.Empty<(int, BootPlacement)>())
            {
                selection.BotaB.Posts.Add(new BootPostPlacement { PostIndex = post, Placement = placement });
            }

            design.Structure.SafetySelections.Add(selection);
            return design;
        }

        // ---------------------------------------------------------------- las DOS rutas

        /// <summary>El corte tal y como lo enseña el editor: por su ensamblador, que es lo que ve el usuario.</summary>
        private static HeaderRunPlan Preview(
            PushBackDesign design, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
        {
            var computation = new PushBackEditorDesignAssembler(catalog).BuildFrom(design, side);
            Assert.True(computation.IsValid, computation.Error);
            return end == PushBackFrontalEnd.EntradaSalida
                ? computation.FrontalEntradaSalida
                : computation.FrontalPosterior;
        }

        /// <summary>El corte tal y como se DIBUJA al insertar.</summary>
        private static HeaderRunPlan Final(
            PushBackDesign design, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
            => new PushBackSystemFrontalBuilder()
                .BuildPlan(new PushBackResolver(catalog).Resolve(design), catalog, end, side);

        /// <summary>La identidad estable de una pieza: que es, donde y como se coloca.</summary>
        private static string Identity(HeaderBlockInstance instance)
            => string.Join(
                "|",
                instance.Role,
                instance.PieceId,
                instance.BlockName,
                Math.Round(instance.Insertion.X, 6).ToString("0.######", CultureInfo.InvariantCulture),
                Math.Round(instance.Insertion.Y, 6).ToString("0.######", CultureInfo.InvariantCulture),
                Math.Round(instance.RotationRadians, 6).ToString("0.######", CultureInfo.InvariantCulture),
                instance.MirroredX,
                instance.MirroredY);

        private static IReadOnlyList<string> DefenseSet(HeaderRunPlan plan, RackCatalog catalog)
            => plan.Flatten().Instances
                .Where(instance => PushBackDefensePlan.IsDefense(instance, catalog))
                .Select(Identity)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

        private static IReadOnlyList<string> BootSet(HeaderRunPlan plan, string bootId)
            => plan.Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, bootId, StringComparison.OrdinalIgnoreCase))
                .Select(Identity)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

        private static void AssertDefenseParity(
            PushBackDesign design, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
            => Assert.Equal(
                DefenseSet(Final(design, catalog, end, side), catalog),
                DefenseSet(Preview(design, catalog, end, side), catalog));

        private static void AssertBootParity(
            PushBackDesign design, RackCatalog catalog, PushBackFrontalEnd end, PushBackSide side)
        {
            var bootId = BootId;
            Assert.Equal(
                BootSet(Final(design, catalog, end, side), bootId),
                BootSet(Preview(design, catalog, end, side), bootId));
        }

        private static IEnumerable<(PushBackFrontalEnd End, PushBackSide Side)> FourCuts()
        {
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
            {
                yield return (end, side);
            }
        }

        // ---------------------------------------------------------------- defensa

        [Fact]
        public void Preview_BSideDefenseNone_MatchesFinalOutput()
        {
            // El caso que encontro A2: «Ninguno» tiene que significar CERO PIEZA tambien en la vista previa.
            var catalog = Catalog;
            var design = Design(RealDefense, PushBackDefaults.NonePieceId);

            Assert.Empty(DefenseSet(Preview(design, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B), catalog));
            foreach (var (end, side) in FourCuts())
            {
                AssertDefenseParity(design, catalog, end, side);
            }
        }

        [Fact]
        public void Preview_ASideDefenseNone_MatchesFinalOutput()
        {
            // El simetrico: nada puede estar cableado hacia un lado concreto.
            var catalog = Catalog;
            var design = Design(PushBackDefaults.NonePieceId, RealDefense);

            Assert.Empty(DefenseSet(Preview(design, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A), catalog));
            Assert.NotEmpty(DefenseSet(Preview(design, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B), catalog));
            foreach (var (end, side) in FourCuts())
            {
                AssertDefenseParity(design, catalog, end, side);
            }
        }

        [Fact]
        public void Preview_UsesDefensePieceIdOfItsPhysicalSide()
        {
            // Con DOS piezas distintas no basta con quitar la que sobra: hay que enseñar la del lado que se mira.
            var catalog = TwoPieceCatalog();
            var design = Design(RealDefense, SecondDefense, catalog: catalog);

            var previewA = Preview(design, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A);
            var previewB = Preview(design, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B);

            Assert.NotEmpty(DefenseSet(previewA, catalog));
            Assert.NotEmpty(DefenseSet(previewB, catalog));
            Assert.All(
                previewA.Flatten().Instances.Where(i => PushBackDefensePlan.IsDefense(i, catalog)),
                instance => Assert.Equal(RealDefense, instance.PieceId));
            Assert.All(
                previewB.Flatten().Instances.Where(i => PushBackDefensePlan.IsDefense(i, catalog)),
                instance => Assert.Equal(SecondDefense, instance.PieceId));

            foreach (var (end, side) in FourCuts())
            {
                AssertDefenseParity(design, catalog, end, side);
            }
        }

        // ---------------------------------------------------------------- botas

        [Fact]
        public void Preview_BSideBootNone_MatchesFinalOutput()
        {
            var catalog = Catalog;
            var design = WithBoots(Design(), BootPlacement.EntryExit, BootPlacement.None);

            Assert.NotEmpty(BootSet(Preview(design, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A), BootId));
            Assert.Empty(BootSet(Preview(design, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B), BootId));
            foreach (var (end, side) in FourCuts())
            {
                AssertBootParity(design, catalog, end, side);
            }
        }

        [Fact]
        public void Preview_BootProjectionMatchesFinalPhysicalPlan()
        {
            // Asimetrico de verdad: cada lado en un extremo distinto, y en B un poste que se sale de su general.
            var catalog = Catalog;
            var design = WithBoots(
                Design(),
                BootPlacement.EntryExit,
                BootPlacement.Rear,
                new[] { (0, BootPlacement.None) });

            foreach (var (end, side) in FourCuts())
            {
                AssertBootParity(design, catalog, end, side);
            }

            // Y la vista previa proyecta la resolucion FISICA, no una cuenta parecida.
            var physical = PushBackBootPlan.Resolve(new PushBackResolver(catalog).Resolve(design), catalog);
            var previewed = FourCuts()
                .Sum(cut => BootSet(Preview(design, catalog, cut.End, cut.Side), BootId).Count);
            Assert.Equal(physical.Count, previewed);
        }

        // ---------------------------------------------------------------- hueco cero y los cuatro cortes

        [Fact]
        public void Preview_GapZeroDoesNotBorrowSecurityFromOppositeSide()
        {
            // Con hueco 0 las dos caras interiores coinciden geometricamente. Coincidir no es ser el mismo lado.
            var catalog = Catalog;
            var defense = Design(RealDefense, PushBackDefaults.NonePieceId, gap: 0.0);
            var boots = WithBoots(Design(gap: 0.0), BootPlacement.Rear, BootPlacement.None);

            Assert.Empty(DefenseSet(Preview(defense, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B), catalog));
            Assert.Empty(BootSet(Preview(boots, catalog, PushBackFrontalEnd.Posterior, PushBackSide.B), BootId));
            foreach (var (end, side) in FourCuts())
            {
                AssertDefenseParity(defense, catalog, end, side);
                AssertBootParity(boots, catalog, end, side);
            }
        }

        [Fact]
        public void Preview_AllFourCompositeCutsMatchFinalResolvedSafety()
        {
            var catalog = TwoPieceCatalog();
            var design = WithBoots(
                Design(RealDefense, SecondDefense, gap: 54.0, catalog: catalog),
                BootPlacement.EntryExit,
                BootPlacement.Rear);

            foreach (var (end, side) in FourCuts())
            {
                AssertDefenseParity(design, catalog, end, side);
                AssertBootParity(design, catalog, end, side);
            }
        }

        // ---------------------------------------------------------------- una sola materializacion

        [Fact]
        public void Preview_DoesNotDuplicateResolvedSafety()
        {
            // El envoltorio retira y vuelve a poner: aplicado dos veces duplicaria las piezas o dejaria las locales.
            var catalog = Catalog;
            var design = WithBoots(
                Design(RealDefense, RealDefense), BootPlacement.EntryExit, BootPlacement.EntryExit);

            foreach (var (end, side) in FourCuts())
            {
                var preview = Preview(design, catalog, end, side);
                var final = Final(design, catalog, end, side);

                var defense = DefenseSet(preview, catalog);
                var boots = BootSet(preview, BootId);
                if (end == PushBackFrontalEnd.EntradaSalida)
                {
                    Assert.NotEmpty(defense); // hay algo que duplicar: la prueba no es vacia
                    Assert.NotEmpty(boots);
                }

                Assert.Equal(defense.Distinct().Count(), defense.Count);
                Assert.Equal(boots.Distinct().Count(), boots.Count);

                // Y el corte entero conserva su cuenta: no se pierde ni se anade nada mas que la seguridad resuelta.
                Assert.Equal(final.Flatten().Instances.Count, preview.Flatten().Instances.Count);
            }
        }

        [Fact]
        public void Preview_SingleSidedRackStillMatchesFinalOutput()
        {
            // El rack de un solo sentido pasa por el mismo sitio: su vista previa tampoco puede divergir.
            var catalog = Catalog;
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 0, deepA: 4, deepB: 0, levelsA: 2, levelsB: 0, gap: 0.0);
            design.SideB.IsPresent = false;
            foreach (var selection in new PushBackSafetyAuthority(catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            design.DefensePieceId = RealDefense;

            // La comparacion no puede ser 0 == 0: el corte de entrada lleva sus defensas.
            Assert.NotEmpty(DefenseSet(Preview(design, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A), catalog));
            foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
            {
                AssertDefenseParity(design, catalog, end, PushBackSide.A);
                AssertBootParity(design, catalog, end, PushBackSide.A);
            }
        }
    }
}
